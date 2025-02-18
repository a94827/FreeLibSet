// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using System.Data.Common;
using FreeLibSet.Logging;
using System.Data;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Обработчик двоичных данных и файлов, хранящихся в базе данных. Для баз данных ограниченного размера (например, MS SQL Server Express Edition)
  /// можно использовать фрагментирование. При этом в основной базе данных хранится только таблица с идентификаторами и контрольными суммами, а сами данные хранятся в отдельных базах данных,
  /// что позволяет преодолеть ограничение на размер.
  /// Исключается дублирование одинаковыех блоков двоичных данных. Проверка уникальности использует контрольную сумму MD5.
  /// Ссылка на объект хранится в <see cref="DBxRealDocProviderGlobal"/>.
  /// Все методы объекта являются потокобезопасными после вызова <see cref="DBxBinDataHandler.SetReadOnly()"/>.
  /// </summary>
  /// <remarks>
  /// Порядок использования:
  /// <list type="bullet">
  /// <item><description>1. Создать <see cref="DBxBinDataHandler"/> и установить управляющие свойства</description></item>
  /// <item><description>2. Инициализировать описания баз данных. Использовать <see cref="DBxBinDataHandler.AddMainTableStructs(DBxStruct)"/> и 
  /// <see cref="DBxBinDataHandler.AddSectionTableStructs(DBxStruct)"/>для добавления описаний таблиц</description></item>
  /// <item><description>3. Создать/обновить базы данных</description></item>
  /// <item><description>4. Присоединить объекты <see cref="DBxEntry"/> к <see cref="DBxBinDataHandler"/></description></item>
  /// <item><description>5. Создать <see cref="DBxRealDocProviderGlobal"/>. При этом <see cref="DBxBinDataHandler.IsReadOnly"/> примет значение true</description></item>
  /// </list>
  /// </remarks>
  public sealed class DBxBinDataHandler : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает обработчик
    /// </summary>
    public DBxBinDataHandler()
    {
      _SectionEntries = new List<DBxEntry>();
      _DBSizeLimitExceededHandlerEnabled = true;

      _BinDataCacheTableStruct = new DBxTableStruct("BinData");
      _BinDataCacheTableStruct.Columns.AddId();
      _BinDataCacheTableStruct.Columns.AddString("MD5", 32, false);
      _BinDataCacheTableStruct.Columns.AddInt("Length", 0, Int32.MaxValue);
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Надо ли использовать фрагментацию базы данных.
    /// По умолчанию - false. Все таблицы хранятся в одной базе данных.
    /// Используется для баз данных, имеющих лимитированный размер, например, MS SQL Server Express.
    /// </summary>
    public bool UseFragmentation
    {
      get { return _UseFragmentation; }
      set
      {
        CheckNotReadOnly();
        _UseFragmentation = value;
      }
    }
    private bool _UseFragmentation;

    /// <summary>
    /// Будут ли в базе данных храниться ссылки на двоичные данные?
    /// По умолчанию - false.
    /// Должно быть установлено в true одно из двух или оба свойства <see cref="UseBinData"/> и <see cref="UseFiles"/>.
    /// </summary>
    public bool UseBinData
    {
      get { return _UseBinData; }
      set
      {
        CheckNotReadOnly();
        _UseBinData = value;
      }
    }
    private bool _UseBinData;


    /// <summary>
    /// Будут ли в базе данных храниться файлы?
    /// По умолчанию - false.
    /// Должно быть установлено в true одно из двух или оба свойства <see cref="UseBinData"/> и <see cref="UseFiles"/>.
    /// </summary>
    public bool UseFiles
    {
      get { return _UseFiles; }
      set
      {
        CheckNotReadOnly();
        _UseFiles = value;
      }
    }
    private bool _UseFiles;

    #endregion

    #region Объявления таблиц

    /// <summary>
    /// Добавляет описания структур таблиц "BinData" и "FileNames".
    /// Структура и наличие таблиц зависит от управляющих свойств <see cref="UseFragmentation"/>, <see cref="UseBinData"/> и <see cref="UseFiles"/>.
    /// </summary>
    /// <param name="dbStruct">Описание базы данных, в которой будут созданы таблицы</param>
    public void AddMainTableStructs(DBxStruct dbStruct)
    {
      DBxTableStruct ts;

      #region Таблица BinData

      if (UseBinData || UseFiles)
      {
        ts = new DBxTableStruct("BinData");
        ts.Columns.AddId();
        ts.Columns.AddString("MD5", 32, false); // Сумма MD5
        ts.Columns.AddInt("Length", 0, Int32.MaxValue); // Размер блока в байтах
        ts.Columns.LastAdded.Nullable = false; // 20.07.2021

        if (UseFragmentation)
        {
          ts.Columns.AddInt("Section", 0, Int16.MaxValue);
          ts.Columns.LastAdded.Nullable = false; // 20.07.2021
        }
        else
          ts.Columns.AddBinary("Contents"); // Содержимое блока

        ts.Indexes.Add("MD5");
        dbStruct.Tables.Add(ts);
      }

      #endregion

      #region Таблица FileNames

      if (UseFiles)
      {
        // Таблица имен файлов
        // "Ссылка на файл" в документе означает значение поля "Id" этой таблицы
        // При загрузке файла выполняется поиск файла с таким именем, датами и с таким содержимым.
        // При необходимости, добавляется запись в эту таблицу и в таблицу FileContents
        // Путь к файлу не хранится. Если требуется хранить каталог, то следует использовать отдельное 
        // текстовое поле
        ts = new DBxTableStruct("FileNames");
        ts.Columns.AddId();
        ts.Columns.AddString("Name", 160, false); // Имя файла без пути
        ts.Columns.AddReference("Data", "BinData", false);
        ts.Columns.AddDateTime("CreationTime", true); // Время создания файла
        ts.Columns.AddDateTime("LastWriteTime", true); // Время изменения файла

        ts.Indexes.Add("Data,Name,CreationTime,LastWriteTime"); // Думаю, что лучше "разнообразное" числовое поле сделать первым
        dbStruct.Tables.Add(ts);
      }

      #endregion
    }

    /// <summary>
    /// Добавляет описание структуры таблицы "BinDataStorage"
    /// </summary>
    /// <param name="dbStruct">Описание базы данных, в которой будут созданы таблицы</param>
    public void AddSectionTableStructs(DBxStruct dbStruct)
    {
      if (!UseFragmentation)
        throw new InvalidOperationException("Свойство UseFragmentation не установлено");

      if (UseBinData || UseFiles)
      {
        DBxTableStruct ts = new DBxTableStruct("BinDataStorage");
        ts.Columns.AddId();
        ts.Columns.AddBinary("Contents"); // Содержимое блока
        dbStruct.Tables.Add(ts);
      }
      else
        throw new InvalidOperationException("Не установлены свойства UseBinData и UseFiles");
    }

    #endregion

    #region Ссылки на базы данных

    /// <summary>
    /// Точка доступа к основной базе данных, в которой объявлены таблицы "BinData" и "FileNames".
    /// Это свойство должно быть установлено обязательно.
    /// </summary>
    public DBxEntry MainEntry
    {
      get { return _MainEntry; }
      set
      {
        CheckNotReadOnly();
        _MainEntry = value;
      }
    }
    private DBxEntry _MainEntry;

    /// <summary>
    /// Событие вызывается при добавлении новой точки подключения, если используется фрагментация (<see cref="UseFragmentation"/>=true).
    /// Для доступа к новому объекту <see cref="DBxEntry"/> используйте вызов <see cref="GetSectionEntry(int)"/> со значением <see cref="SectionEntryCount"/>.
    /// </summary>
    public event EventHandler SectionEntryAdded;

    /// <summary>
    /// Добавить точку входу для базы данных секции.
    /// Если <see cref="UseFragmentation"/>=true, то должна быть добавлена хотя бы одна точка входа.
    /// Если <see cref="UseFragmentation"/>=false, вызов метода не разрешается.
    /// Секции могут добавляться после установки свойства <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="dbEntry">Точка входа в базу данных</param>
    public void AddSectionEntry(DBxEntry dbEntry)
    {
      if (dbEntry == null)
        throw new ArgumentNullException("dbEntry");
      if (!UseFragmentation)
        throw new InvalidOperationException("Добавлять секционные базы данных можно только при UseFragmentation=true");

      lock (_SectionEntries)
      {
        for (int i = 0; i < _SectionEntries.Count; i++)
        {
          if (_SectionEntries[i].DB == dbEntry.DB)
            throw new InvalidOperationException("Повторно добавление точки входа в базу данных " + dbEntry.DB.ToString());
        }
        _SectionEntries.Add(dbEntry);
      }

      if (SectionEntryAdded != null)
        SectionEntryAdded(this, EventArgs.Empty);
    }

    /// <summary>
    /// Список точек входа в секционные базы данных.
    /// При добавлении новой строки двоичных данных всегда используется последняя секция.
    /// На время работы списка требуется блокировка, так как новые точки могут добавляться динамически.
    /// </summary>
    private List<DBxEntry> _SectionEntries;

    /// <summary>
    /// Возвращает количество добавленных секционных баз данных
    /// </summary>
    public int SectionEntryCount
    {
      get
      {
        lock (_SectionEntries)
        {
          return _SectionEntries.Count;
        }
      }
    }

    /// <summary>
    /// Возвращает все добавленные точки входа в секционные базы данных в виде массива
    /// </summary>
    /// <returns>Массив точек входа</returns>
    public DBxEntry[] GetSectionEntries()
    {
      lock (_SectionEntries)
      {
        return _SectionEntries.ToArray();
      }
    }

    /// <summary>
    /// Получить точку входу для заданной секции.
    /// Нумерация секций идет с 1, а не с 0
    /// </summary>
    /// <param name="sectionNum">Номер секции (1,2, ...)</param>
    /// <returns>Точка входа</returns>
    public DBxEntry GetSectionEntry(int sectionNum)
    {
      DBxEntry en;
      lock (_SectionEntries)
      {
        en = _SectionEntries[sectionNum - 1];
      }

#if DEBUG
      if (Object.ReferenceEquals(en.DB, MainEntry.DB))
        throw new BugException("База данных секции совпадает с основной базой данных");
#endif

      return en;
    }

    /// <summary>
    /// Проверяет, что в списке <see cref="GetSectionEntries()"/> есть достаточное количество баз данных, соответствующих секциям в таблице "BinData".
    /// </summary>
    public void CheckBinDataSections()
    {
      if (!UseFragmentation)
        return;

      int maxSection;
      using (DBxCon con = new DBxCon(MainEntry))
      {
        maxSection = DataTools.GetInt(con.GetMaxValue("BinData", "Section", null));
      }

      if (maxSection > SectionEntryCount)
        throw new InvalidOperationException("Недостаточное количество точек входа во фрагментированные базы данных (" + SectionEntryCount.ToString() + ")." +
          "В таблице BinData используется секция с номером " + maxSection.ToString());

      // "Лишние" секции могут быть, это не ошибка
    }

    /// <summary>
    /// Часть структуры таблицы "BinData", разрешенная для буферизации.
    /// Содержит поля "Id", "MD5" и "Length".
    /// </summary>
    public DBxTableStruct BinDataCacheTableStruct { get { return _BinDataCacheTableStruct; } }
    private DBxTableStruct _BinDataCacheTableStruct;

    #endregion

    #region IReadOnlyObject

    /// <summary>
    /// Возвращает true, если нельзя задавать управляющие свойства.
    /// Не имеет отношения к возможности записи самих двоичных данных.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Запрещает дальнейшую установку управляющих свойств.
    /// Повторные вызовы игнорируются.
    /// Метод не предназначен для использования напрямую в пользовательском коде.
    /// </summary>
    public void SetReadOnly()
    {
      if (_IsReadOnly)
        return;

      if (!(_UseFiles || _UseBinData))
        throw new BugException("Должно быть установлено хотя бы одно из свойств: UseBinData или UseFiles. Если в программе не храняться такие данные, то объект DBxBinDataHandler создавать не следует");
      _IsReadOnly = true;

      if (_MainEntry == null)
        throw new NullReferenceException("Не было установлено свойство MainEntry");

      if (_UseFragmentation)
      {
        // ReSharper disable once InconsistentlySynchronizedField
        if (_SectionEntries.Count == 0)
          throw new InvalidOperationException("Не задано ни одной точки входа в SectionEntries");
      }
      else
      {
        // ReSharper disable once InconsistentlySynchronizedField
        if (_SectionEntries.Count > 0)
          throw new InvalidOperationException("Нельзя задаватьточки входа в SectionEntries, если FUseFragmentation=false");
      }
    }

    #endregion

    #region Двоичные данные

    /// <summary>
    /// Выполняет поиск в таблице "BinData" для заданной суммы MD5.
    /// Возвращает идентификатор найденной записи или 0, если таких данных еще нет.
    /// </summary>
    /// <param name="md5">Вычисленная заранее сумма блока</param>
    /// <returns>Идентификатор</returns>
    public Int32 FindBinData(string md5)
    {
      if (String.IsNullOrEmpty(md5))
        return 0;

      using (DBxCon con = new DBxCon(MainEntry))
      {
        return con.FindRecord("BinData", "MD5", md5);
      }
    }

    /// <summary>
    /// Выполняет поиск в таблице "BinData" для заданной суммы MD5.
    /// Возвращает идентификатор найденной записи или 0, если таких данных еще нет.
    /// Выполняет дополнительную проверку длины.
    /// Наличие блоков с одинаковой контрольной суммой но разной длиной не допускается. При обнаружении такого блока в базе данных выбрасывается исключение.
    /// </summary>
    /// <param name="md5">Вычисленная заранее сумма блока</param>
    /// <param name="length">Ожидаемая длина блока данных в байтах</param>
    /// <returns>Идентификатор</returns>
    /// <exception cref="DBxConsistencyException">Обнаружено наличие существующего блока с такой же контрольной суммой, но другой длиной</exception>
    public Int32 FindBinDataWithLenChecking(string md5, int length)
    {
      if (String.IsNullOrEmpty(md5))
        return 0;

      using (DBxCon con = new DBxCon(MainEntry))
      {
        Int32 id = con.FindRecord("BinData", "MD5", md5);
        if (id != 0)
        {
          Int32 realLength = DataTools.GetInt(con.GetValue("BinData", id, "Length"));
          if (realLength != length)
            throw new DBxConsistencyException("В таблице BinData для блока данных с Id=" + id.ToString() + " расходится длина. В базе данных: " +
              realLength.ToString() + ", запрошенная длина: " + length.ToString());
        }
        return id;
      }
    }

    private static readonly DBxColumns _AppendBinDataColumns1 = new DBxColumns("MD5,Length");
    private static readonly DBxColumns _AppendBinDataColumns2 = new DBxColumns("MD5,Length,Section");

    /// <summary>
    /// Объект, для которого выполняется блокирование на время добавления записи в таблицу "BinData" и "BinDataStorage".
    /// Не стоит использовать _SectionEntries, так как тогда будут блокироваться параллельные операции чтения данных.
    /// </summary>
    private object _AppendBinDataLockObj = new object();

    /// <summary>
    /// Добавляет блок двоичных данных в таблицу "BinData".
    /// Если такой блок уже есть, возвращается идентификатор существующей записи.
    /// </summary>
    /// <param name="contents">Данные для записи</param>
    /// <returns>Идентификатор блока</returns>
    public Int32 AppendBinData(byte[] contents)
    {
      if (contents == null)
        return 0;

      Int32 id;
      string md5 = DataTools.MD5Sum(contents);

      // Для предотвращения ошибки одновременного добавления нескольких блоков, выполняем блокировку
      lock (_AppendBinDataLockObj)
      {
        using (DBxCon con = new DBxCon(MainEntry))
        {
          id = con.FindRecord("BinData", "MD5", md5);
          if (id == 0)
          {
            if (UseFragmentation)
            {
              // Делаем 2 попытки добавить запись
              // Между ними можно попробовать добавить базу данных
              try
              {
                id = DoAppendBinDataWhenFragmentation(0, contents, md5, con);
              }
              catch (Exception e)
              {
                if (!MainEntry.DB.IsDBSizeLimitExceededException(e))
                  throw;
                if (DBSizeLimitExceeded == null)
                  throw;
                if (!_DBSizeLimitExceededHandlerEnabled)
                  throw;

                _DBSizeLimitExceededHandlerEnabled = false; // временно отключаем
                // Вызываем пользовательское событие
                DBSizeLimitExceeded(this, EventArgs.Empty);

                // Пробуем еще раз
                id = DoAppendBinDataWhenFragmentation(0, contents, md5, con);

                // Получилось - включаем флажок обратно
                _DBSizeLimitExceededHandlerEnabled = true;
              }
            }
            else // Фрагментация не используется.
            {
              con.TransactionBegin();
              id = con.AddRecordWithIdResult("BinData", _AppendBinDataColumns1, new object[] { md5, contents.Length });
              con.WriteBlob("BinData", id, "Contents", contents);
              con.TransactionCommit();
            }

          }
        } // using Con
      } // lock

      return id;
    }

    /// <summary>
    /// Строка из 32 пробелов.
    /// Это не сумма MD5 для пустой строки.
    /// </summary>
    internal static readonly string EmptyMD5 = new string(' ', 32);

    private Int32 DoAppendBinDataWhenFragmentation(Int32 binDataId, byte[] contents, string md5, DBxCon con)
    {
      // 18.11.2020
      // Не используем DBxTransactionArray.
      // Также используем запись в "BinData" за два прохода. Сначала добавляем запись с пустым MD5, а в конце заменяем на правильное значение.
      // При этом выполняется на один SQL-запрос больше, но так будет надежнее.
      // Не используем транзакцию для основной таблицы "BinData". Если произойдет сбой при записи в "BinDataStorage",
      // то в "BinData" пусть остается фиктивная запись с нулевым MD5. Это позволит избежать ошибок при последующей
      // записи, т.к. неизвестно, есть запись в "BinDataStorage" или нет.
      // Однако, все же сделаем попытку удалить лишнее.

      // 18.11.2020
      // Метод используется как при обычной работе, так и при импорте

      int section = SectionEntryCount; // нумерация начиная с 1

      // Записываем недоделанную запись с фиктивным MD5
      if (binDataId == 0)
      {
        // Основной вариант вызова метода - AppendBinData()
        binDataId = con.AddRecordWithIdResult("BinData", _AppendBinDataColumns2, new object[] { EmptyMD5, contents.Length, section });
        if (binDataId == 0)
          throw new BugException("Id=0");
      }
      else
      {
        // Вызов при импорте данных

        con.AddRecord("BinData", new DBxColumns("Id,MD5,Length,Section"),
          new object[] { binDataId, EmptyMD5, contents.Length, section });
      }

      bool binDataStorageWritten = false;
      try
      {
        // Записываем двоичные данные в таблицу секции
        DBxEntry en2 = GetSectionEntry(section);
        using (DBxCon con2 = new DBxCon(en2))
        {
          //#if DEBUG
          //          if (!en2.DB.Struct.Tables.Contains("BinDataStorage"))
          //            throw new BugException("Нет таблицы BinDataStorage");
          //#endif

          con2.TransactionBegin();
          con2.AddRecord("BinDataStorage", "Id", binDataId);
          con2.WriteBlob("BinDataStorage", binDataId, "Contents", contents);
          con2.TransactionCommit();
          binDataStorageWritten = true;
        }

        // Только в случае успеха заменяем MD5
        con.SetValue("BinData", binDataId, "MD5", md5);
      }
      catch (Exception e)
      {
        System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + ". DBxBinDataHandler. Error when writing data to \"BinDataStorage\" table for Section #" + section.ToString() + " for Id=" + binDataId.ToString());
        System.Diagnostics.Trace.Indent();
        try
        {
          System.Diagnostics.Trace.WriteLine(e.GetType().ToString());
          System.Diagnostics.Trace.WriteLine(e.Message);
        }
        finally
        {
          System.Diagnostics.Trace.Unindent();
        }

        // Пытаемся удалить неправильные данные
        try
        {
          if (binDataStorageWritten)
          {
            using (DBxCon con2 = new DBxCon(GetSectionEntry(section)))
            {
              con2.Delete("BinDataStorage", new ValueFilter("Id", binDataId));
            }
          }
          con.Delete("BinData", new ValueFilter("Id", binDataId));
          System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + ". DBxBinDataHandler. Record for Id=" + binDataId.ToString() + " successfully deleted");
        }
        catch
        {
          System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + ". DBxBinDataHandler. Record for Id=" + binDataId.ToString() + " cannot be deleted. There is a stub in \"BinData\"");
          // Здесь все ошибки проглатываем
        }

        throw;
      }
      return binDataId;
    }

    /// <summary>
    /// Это событие вызывается, когда используется фрагментирование баз данных для хранения двоичных данных и текущая база данных
    /// переполнилась.
    /// Обработчик должен создать новую базу данных и вызвать <see cref="AddSectionEntry(DBxEntry)"/>
    /// </summary>
    public event EventHandler DBSizeLimitExceeded;

    /// <summary>
    /// Можно ли вызывать обработчик события <see cref="DBSizeLimitExceeded"/>?
    /// Если вызов обработчика не помог и возникло повторное исключение - плохая идея создавать третью, четвертую базу данных.
    /// </summary>
    private bool _DBSizeLimitExceededHandlerEnabled;

    /// <summary>
    /// Возвращает двоичные данные для заданного идентификатора
    /// </summary>
    /// <param name="binDataId">Идентификатор в таблице "BinData"</param>
    /// <returns>Блок данных</returns>
    public byte[] GetBinData(Int32 binDataId)
    {
      if (binDataId == 0)
        return null;

      using (DBxCon con = new DBxCon(MainEntry))
      {
        if (UseFragmentation)
        {
          // Сначала требуется определить секцию для хранения данных
          int section = DataTools.GetInt(con.GetValue("BinData", binDataId, "Section"));
          if (section == 0)
            throw new ArgumentException("Неправильный идентификатор записи для таблицы BinData Id=" + binDataId.ToString(), "binDataId");

          DBxEntry entry = GetSectionEntry(section);
          using (DBxCon con2 = new DBxCon(entry))
          {
            return con2.ReadBlob("BinDataStorage", binDataId, "Contents");
          }
        }
        else
        {
          // Возвращаем блок данных из основной таблицы
          return con.ReadBlob("BinData", binDataId, "Contents");
        }
      }
    }

    /// <summary>
    /// Возвращает общее количество блоков двоичных данных.
    /// Метод предназначен для статистических целей.
    /// </summary>
    /// <returns></returns>
    public int GetBinDataCount()
    {
      if (UseBinData)
      {
        using (DBxCon con = new DBxCon(MainEntry))
        {
          // Возвращаем блок данных из основной таблицы
          return con.GetRecordCount("BinData");
        }
      }
      else
        return 0;
    }

    #endregion

    #region Файлы

    /// <summary>
    /// Максимальная длина файла, который (теоретически) можно хранить в базе данных
    /// </summary>
    public const long MaxFileLength = (long)Int32.MaxValue;

    private static readonly DBxColumns _FindDBFileColumns = new DBxColumns("Data,Name,CreationTime,LastWriteTime");

    /// <summary>
    /// Выполняет поиск файла в таблице FileNames и BinData
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="mD5">Сумма MD5 для содержания файла</param>
    /// <returns>Идентификатор записи в таблице FileNames</returns>
    public Int32 FindDBFile(StoredFileInfo fileInfo, string mD5)
    {
      Int32 binDataId;
      return FindDBFile(fileInfo, mD5, out binDataId);
    }

    /// <summary>
    /// Выполняет поиск файла в таблице FileNames и BinData.
    /// Эта перегрузка дополнительно возвращает идентификатор содержимого файла в таблице BinData.
    /// Метод может не найти файл (вернуть 0), но при этом найти данные (в <paramref name="binDataId"/> будет
    /// записано ненулевое значение).
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="md5">Сумма MD5 для содержания файла</param>
    /// <param name="binDataId">Сюда записывается идентификатор в таблице BinData, если содержимое файла второй раз записывать не нужно</param>
    /// <returns>Идентификатор записи в таблице FileNames</returns>
    public Int32 FindDBFile(StoredFileInfo fileInfo, string md5, out Int32 binDataId)
    {
      if (fileInfo.Length > MaxFileLength)
        throw new ArgumentException("Слишком длинный файл");

      // Сначала ищем по содержимому файла
      binDataId = FindBinDataWithLenChecking(md5, (int)(fileInfo.Length));
      if (binDataId == 0)
        return 0;

      // 07.04.2017
      // Можно было бы использовать версию FindRecord() со списком полей и значений, но там могут быть null.
      // Придется собирать фильтр вручную

      DBxFilter[] filters = new DBxFilter[4];
      filters[0] = new ValueFilter("Data", binDataId);
      filters[1] = new ValueFilter("Name", fileInfo.Name);
      filters[2] = new ValueFilter("CreationTime", fileInfo.CreationTime, typeof(DateTime));
      filters[3] = new ValueFilter("LastWriteTime", fileInfo.LastWriteTime, typeof(DateTime));

      using (DBxCon con = new DBxCon(MainEntry))
      {
        return con.FindRecord("FileNames", new AndFilter(filters));
      }
    }

    /// <summary>
    /// Объект, для которого выполняется блокирование на время добавления записи в таблицу FileNames
    /// Добавление содержимого файлв в таблицу BinData выполняется методом AppendBinData() вне этой блокировки.
    /// </summary>
    private object _AppendDBFileLockObj = new object();

    /// <summary>
    /// Добавление файла в базу данных.
    /// Если в базе данных уже есть такой файл, возвращается существуюзий идентификатор
    /// </summary>
    /// <param name="file">Контейнер с файлом</param>
    /// <returns>Идентификатор в таблице FileNames</returns>
    public Int32 AppendDBFile(FileContainer file)
    {
      if (file == null)
        return 0;

      if (file.FileInfo.Length > MaxFileLength)
        throw new ArgumentException("Слишком длинный файл");

      // Сначала ищем по содержимому файла
      Int32 binDataId = AppendBinData(file.Content);
      if (binDataId == 0)
        throw new BugException("AppendBinData вернул 0");

      return AppendDBFile(file.FileInfo, binDataId);
    }

    /// <summary>
    /// Добавление файла в базу данных.
    /// Если в базе данных уже есть такой файл, возвращается существуюзий идентификатор
    /// </summary>
    /// <param name="fileInfo">Имя файла и другая информация</param>
    /// <param name="binDataId">Содержимое файла - уже существующие данные в таблице BinData</param>
    /// <returns>Идентификатор в таблице FileNames</returns>
    public Int32 AppendDBFile(StoredFileInfo fileInfo, Int32 binDataId)
    {
      if (fileInfo.IsEmpty)
        throw new ArgumentException("Информация о файле не задана", "fileInfo");
      if (binDataId <= 0)
        throw new DBxNoIdArgumentException("Неправильный идентификатор данных в таблице BinData. DataId=" + binDataId.ToString(), "binDataId");

      DBxFilter[] filters = new DBxFilter[4];
      filters[0] = new ValueFilter("Data", binDataId);
      filters[1] = new ValueFilter("Name", fileInfo.Name);
      filters[2] = new ValueFilter("CreationTime", fileInfo.CreationTime, typeof(DateTime));
      filters[3] = new ValueFilter("LastWriteTime", fileInfo.LastWriteTime, typeof(DateTime));

      using (DBxCon con = new DBxCon(MainEntry))
      {
        // 07.04.2017
        // TODO: Можно было бы не использовать блокировку, но метод FindOrAddRecord() пока не надежный
        // К тому же значения null правильно не обрабатываются
        lock (_AppendDBFileLockObj)
        {
          Int32 fileId = con.FindRecord("FileNames", new AndFilter(filters));
          if (fileId == 0)
            fileId = con.AddRecordWithIdResult("FileNames", _FindDBFileColumns,
              new object[] { binDataId, fileInfo.Name, fileInfo.CreationTime, fileInfo.LastWriteTime });
          return fileId;
        }
      }
    }


    private static readonly DBxColumns _GetDBFileInfoColumns = new DBxColumns("Name,Data.Length,CreationTime,LastWriteTime");

    /// <summary>
    /// Возвращает информацию о файле (имя, длина, время создания и записи) по заданному идентификатору
    /// Если задан нулевой или неверный идентификатор, генерируется исключение
    /// </summary>
    /// <param name="fileId">Идентификатор записи в таблице FileNames</param>
    /// <returns>Информация о файле</returns>
    public StoredFileInfo GetDBFileInfo(Int32 fileId)
    {
      object[] values;
      using (DBxCon con = new DBxCon(MainEntry))
      {
        values = con.GetValues("FileNames", fileId, _GetDBFileInfoColumns);
      }

      string Name = DataTools.GetString(values[0]);
      if (Name.Length == 0)
        throw new ArgumentException("Запрошен несуществующий идентификатор записи в таблице FileNames Id=" + fileId.ToString());

      return new StoredFileInfo(Name, DataTools.GetInt(values[1]), DataTools.GetNullableDateTime(values[2]), DataTools.GetNullableDateTime(values[3]));
    }

    private static readonly DBxColumns _GetDBFileColumns = new DBxColumns("Name,Data.Length,CreationTime,LastWriteTime,Data");

    /// <summary>
    /// Получение файла в виде контейнера FileContainer по идентификатору в таблице FileNames.
    /// Если Id=0 возвращается null.
    /// Если задан недействительный идентификатор файла, генерируется исключение
    /// </summary>
    /// <param name="fileId">Идентификатор записи в таблице FileNames</param>
    /// <returns>Контейнер с файлом</returns>
    public FileContainer GetDBFile(Int32 fileId)
    {
      if (fileId == 0)
        return null;

      object[] values;
      using (DBxCon con = new DBxCon(MainEntry))
      {
        values = con.GetValues("FileNames", fileId, _GetDBFileColumns);
      }

      string name = DataTools.GetString(values[0]);
      if (name.Length == 0)
        throw new ArgumentException("Запрошен несуществующий идентификатор записи в таблице FileNames Id=" + fileId.ToString()); // это никогда не возникнет

      StoredFileInfo fileInfo = new StoredFileInfo(name, DataTools.GetInt(values[1]), DataTools.GetNullableDateTime(values[2]), DataTools.GetNullableDateTime(values[3]));

      Int32 binDataId = DataTools.GetInt(values[4]);
      byte[] contents = GetBinData(binDataId);
      if (contents == null)
        throw new BugException("GetBinData() вернул null");

      return new FileContainer(fileInfo, contents);
    }

    /// <summary>
    /// Возвращает общее количество записей файлов.
    /// Метод предназначен для статистических целей
    /// </summary>
    /// <returns></returns>
    public int GetDBFileCount()
    {
      if (UseFiles)
      {
        using (DBxCon con = new DBxCon(MainEntry))
        {
          // Возвращаем блок данных из основной таблицы
          return con.GetRecordCount("FileNames");
        }
      }
      else
        return 0;
    }

    #endregion

    #region Копирование баз данных

    /// <summary>
    /// Импорт данных из другого комплекта баз данных
    /// </summary>
    /// <param name="src">Исходный набор данных</param>
    public void ImportData(DBxBinDataHandler src)
    {
      if (src == null)
        throw new ArgumentNullException("src");

      this.SetReadOnly();
      src.SetReadOnly();

      using (DBxCon resMainCon = new DBxCon(MainEntry))
      {
        #region Проверка отсутствия данных

        if (UseBinData || UseFiles)
        {
          if (!resMainCon.IsTableEmpty("BinData"))
            throw new InvalidOperationException("Таблица \"BinData\" уже содержит строки. Импорт возможен, только если таблица пуста");
        }
        if (UseFiles)
        {
          if (!resMainCon.IsTableEmpty("FileNames"))
            throw new InvalidOperationException("Таблица \"FileNames\" уже содержит строки. Импорт возможен, только если таблица пуста");
        }

        #endregion

        if ((UseBinData || UseFiles) && (src.UseBinData || src.UseFiles))
          ImportBinData(src, resMainCon);
        if (UseFiles && src.UseFiles)
          ImportFileNames(src, resMainCon);
      }
    }

    /// <summary>
    /// Копируем таблицу двоичных данных.
    /// При этом исходный и текущий набор могут использовать или не использовать фрагментацию
    /// баз данных
    /// </summary>
    /// <param name="src"></param>
    /// <param name="resMainCon"></param>
    private void ImportBinData(DBxBinDataHandler src, DBxCon resMainCon)
    {
      lock (_AppendBinDataLockObj)
      {
        using (DBxCon srcCon = new DBxCon(src.MainEntry)) // соединение для перебора исходной таблицы BinData
        {
          using (DbDataReader rdr = srcCon.ReaderSelect("BinData", new DBxColumns("Id,MD5")))
          {
            while (rdr.Read())
            {
              Int32 id = rdr.GetInt32(0);
              string md5 = rdr.GetString(1);

              byte[] contents = src.GetBinData(id); // независимо от фрагментации в исходном наборе
#if DEBUG
              if (contents == null)
                throw new NullReferenceException("Contents==null");
#endif
              DoImportBinDataRec(id, md5, contents, resMainCon);
            }
          }
        }
      }
    }

    private void DoImportBinDataRec(Int32 binDataId, string md5, byte[] contents, DBxCon con)
    {
      if (UseFragmentation)
      {
        // Делаем 2 попытки добавить запись
        // Между ними можно попробовать добавить базу данных
        try
        {
          DoAppendBinDataWhenFragmentation(binDataId, contents, md5, con);
        }
        catch (Exception e)
        {
          if (!MainEntry.DB.IsDBSizeLimitExceededException(e))
            throw;
          if (DBSizeLimitExceeded == null)
            throw;
          if (!_DBSizeLimitExceededHandlerEnabled)
            throw;

          _DBSizeLimitExceededHandlerEnabled = false; // временно отключаем
          // Вызываем пользовательское событие
          System.Diagnostics.Trace.WriteLine("DBxBinDataHandler.DBSizeLimitExceeded event handler is calling ...");
          try
          {
            DBSizeLimitExceeded(this, EventArgs.Empty);
          }
          catch (Exception e2)
          {
            System.Diagnostics.Trace.WriteLine("DBxBinDataHandler.DBSizeLimitExceeded event handler failed!");
            LogoutTools.LogoutException(e2, "DBxBinDataHandler.DBSizeLimitExceeded event handler error");
            System.Diagnostics.Trace.WriteLine("There will be no more attempts to create new segment database.");
            System.Diagnostics.Trace.WriteLine("Free disk space and restart the application");
            throw;
          }
          System.Diagnostics.Trace.WriteLine("DBxBinDataHandler.DBSizeLimitExceeded event handler successfully called");


          // Пробуем еще раз
          DoAppendBinDataWhenFragmentation(binDataId, contents, md5, con);

          // Получилось - включаем флажок обратно
          _DBSizeLimitExceededHandlerEnabled = true;
        }
      }
      else // Фрагментация не используется.
      {
        con.TransactionBegin();
        con.AddRecord("BinData", new DBxColumns("Id,MD5,Length"), new object[] { binDataId, md5, contents.Length });
        con.WriteBlob("BinData", binDataId, "Contents", contents);
        con.TransactionCommit();
      }
    }

    private void ImportFileNames(DBxBinDataHandler src, DBxCon resMainCon)
    {
      using (DBxCon srcCon = new DBxCon(src.MainEntry)) // соединение для перебора исходной таблицы BinData
      {
        using (DbDataReader rdr = srcCon.ReaderSelect("FileNames",
          new DBxColumns("Id,Name,Data,CreationTime,LastWriteTime")))
        {
          resMainCon.AddRecords("FileNames", rdr);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Выполняет проверку целостности двоичных данных, которые обрабатываются <see cref="DBxBinDataHandler"/>.
  /// После настройки свойств должен быть вызван метод <see cref="DBxBinDataValidator.Validate()"/>.
  /// </summary>
  public sealed class DBxBinDataValidator
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект проверки
    /// </summary>
    /// <param name="globalData">Глобальные данных для документов. Не может быть null. Должно быть установлено свойство <see cref="DBxRealDocProviderGlobal.BinDataHandler"/>.</param>
    public DBxBinDataValidator(DBxRealDocProviderGlobal globalData)
    {
#if DEBUG
      if (globalData == null)
        throw new ArgumentNullException("globalData");
#endif

      if (globalData.BinDataHandler == null)
        throw new ArgumentException("Переданный объект не содержит ссылки на DBxBinDataHandler");


      _GlobalData = globalData;
      _Errors = new ErrorMessageList();
      _UseDocRefValidation = true;
      _UseBinDataValidation = true;
      _UseTrace = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылки на объекты баз данных и <see cref="DBxBinDataHandler"/>.
    /// </summary>
    public DBxRealDocProviderGlobal GlobalData { get { return _GlobalData; } }
    private readonly DBxRealDocProviderGlobal _GlobalData;

    /// <summary>
    /// Сюда добавляются сообщения об ошибках
    /// </summary>
    public ErrorMessageList Errors { get { return _Errors; } }
    private readonly ErrorMessageList _Errors;

    /// <summary>
    /// Если установлено в true (по умолчанию), то будет проверяться ссылочная целостность для документов и поддокументов.
    /// Актуально для базы данных истории. Для основной базы данных проверка не выполняется, так как ссылочная целость обеспечивается СУБД.
    /// </summary>
    public bool UseDocRefValidation { get { return _UseDocRefValidation; } set { _UseDocRefValidation = value; } }
    private bool _UseDocRefValidation;

    /// <summary>
    /// Если установлено в true (по умолчанию), то будет выполняться проверка двоичных данных на реальный доступ к блокам, соответствие контрольным суммам и длине.
    /// </summary>
    public bool UseBinDataValidation { get { return _UseBinDataValidation; } set { _UseBinDataValidation = value; } }
    private bool _UseBinDataValidation;

    /// <summary>
    /// Если установлено в true (по умолчанию), то будет использована трассировка
    /// </summary>
    public bool UseTrace { get { return _UseTrace; } set { _UseTrace = value; } }
    private bool _UseTrace;

    #endregion

    #region Основной метод

    /// <summary>
    /// Основной метод
    /// Выполняет проверку целостности двоичных данных
    /// </summary>
    public void Validate()
    {
      #region Строки для процентного индикатора

      List<string> splashItems = new List<string>();
      if (UseDocRefValidation)
        splashItems.Add("Проверка ссылок в документах");

      int SectionEntryCount = 0;
      if (UseBinDataValidation)
      {
        splashItems.Add("Проверка двоичных данных");
        splashItems.Add("Поиск повторов в двоичных данных");
        if (_GlobalData.BinDataHandler.UseFragmentation)
        {
          // Запоминаем количество секций заранее.
          // Вдруг в процессе проверке добавится новая секция
          SectionEntryCount = _GlobalData.BinDataHandler.SectionEntryCount;
          for (int section = 1; section <= SectionEntryCount; section++)
          {
            DBxEntry en = _GlobalData.BinDataHandler.GetSectionEntry(section);
            splashItems.Add("Проверка БД " + en.DB.DatabaseName);
          }
        }
      }
      if (splashItems.Count == 0)
        return; // все флажки сброшены

      #endregion

      if (UseTrace)
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + ". DBxBinDataValidator started");
        Trace.IndentLevel++;
      }
      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(splashItems.ToArray());
      try
      {
        if (UseDocRefValidation)
        {
          if (ValidateRefs(spl))
            spl.Complete();
          else
            spl.Skip();
        }

        if (UseBinDataValidation)
        {
          ValidateBinDataSums(spl);
          spl.Complete();

          ValidateBinDataRepeats(spl);
          spl.Complete();

          for (int section = 1; section <= SectionEntryCount; section++)
          {
            try
            {
              ValidateBinDataStorage(section, spl);
              spl.Complete();
            }
            catch (Exception e)
            {
              e.Data["CheckBinDataStorages.Section"] = section;
              throw;
            }
          }
        }
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
        if (UseTrace)
        {
          Trace.IndentLevel--;
          Trace.WriteLine(DateTime.Now.ToString("G") + ". DBxBinDataValidator finished");
        }
      }
    }

    #endregion

    #region Проверка ссылок из документов

    private bool ValidateRefs(ISimpleSplash spl)
    {
      if (UseTrace)
        Trace.WriteLine("Search for references in document tables...");
      DBxBinDataRefFinder finder = new DBxBinDataRefFinder();
      finder.FindRefs(GlobalData, true);

      if (finder.IsEmpty)
      {
        if (UseTrace)
          Trace.WriteLine("There are no references to validate. All possible references are maintained by database");
        return false; // такое может быть, если история не ведется
      }

      using (DBxConBase con1 = GlobalData.MainDBEntry.CreateCon())
      {
        // При проверке могут встретиться таблицы, которые не объявлены в программе, но которые надо проверять
        con1.NameCheckingEnabled = false; // для основной БД не очень актуально

        DoValidateRefs(con1, finder.MainBinDataRefs, con1, "BinData", spl);
        DoValidateRefs(con1, finder.MainFileNameRefs, con1, "FileNames", spl);
        if (GlobalData.UndoDBEntry != null)
        {
          using (DBxConBase con2 = GlobalData.UndoDBEntry.CreateCon())
          {
            con2.NameCheckingEnabled = false; // а вот для Undo - очень даже актуально

            DoValidateRefs(con2, finder.UndoBinDataRefs, con1, "BinData", spl);
            DoValidateRefs(con2, finder.UndoFileNameRefs, con1, "FileNames", spl);
          }
        }
#if DEBUG
        else if (finder.UndoBinDataRefs.Count > 0 || finder.UndoFileNameRefs.Count > 0)
          throw new BugException("Лишние Undo");
#endif
      }
      return true;
    }

    /// <summary>
    /// Поиск для одного списка
    /// </summary>
    /// <param name="detCon">Подключение к БД, в которой находятся таблицы из списка</param>
    /// <param name="refList">Список таблиц и ссылочных полей</param>
    /// <param name="masterCon">Подключение к (основной) базе данных, где находится <paramref name="masterTableName"/></param>
    /// <param name="masterTableName">Имя таблицы "BinData" или "FileNames"</param>
    /// <param name="spl">Текущая экранная заставка</param>
    private void DoValidateRefs(DBxConBase detCon, DBxTableColumnList refList, DBxConBase masterCon, string masterTableName, ISimpleSplash spl)
    {
      foreach (DBxTableColumnName tc in refList)
      {
        string oldPT = spl.PhaseText;

        string refText = "DB=" + detCon.DB.DatabaseName + ", TableName=\"" + tc.TableName + "\", ColumnName=\"" + tc.ColumnName + "\"";
        spl.PhaseText = "Поиск ссылок. " + refText;
        if (UseTrace)
          Trace.WriteLine(refText + "...");

        Int32 lastId = DataTools.GetInt(detCon.GetMaxValue(tc.TableName, "Id", null));
        spl.PercentMax = lastId;
        spl.AllowCancel = true;
        for (Int32 firstId = 1; firstId <= lastId; firstId += 1000)
        {
          spl.Percent = firstId - 1;

          Int32 thisLastId = Math.Min(firstId + 1000 - 1, lastId);

          DBxFilter[] filters = new DBxFilter[3];
          filters[0] = new ValueFilter("Id", firstId, CompareKind.GreaterOrEqualThan);
          filters[1] = new ValueFilter("Id", thisLastId, CompareKind.LessOrEqualThan);
          filters[2] = new NotNullFilter(tc.ColumnName, DBxColumnType.Int);
          Int32[] refIds = detCon.GetUniqueIntValues(tc.TableName,
            tc.ColumnName,
            new AndFilter(filters));
          if (refIds.Length > 0)
          {
            Int32[] realIds = masterCon.GetUniqueIntValues(masterTableName, "Id", new IdsFilter(refIds));
            if (realIds.Length != refIds.Length)
            {
              ArrayIndexer<Int32> realIdIndexer = new ArrayIndexer<Int32>(realIds);

              // Есть ошибки. Нужен детальный поиск
              DataTable table = detCon.FillSelect(tc.TableName,
                new DBxColumns(new string[] { "Id", tc.ColumnName }),
                new AndFilter(filters));
              foreach (DataRow row in table.Rows)
              {
                Int32 id = DataTools.GetInt(row, "Id");
                Int32 refId = DataTools.GetInt(row, tc.ColumnName);
                if (refId == 0)
                  throw new BugException("RefId=0");
                if (!realIdIndexer.Contains(refId))
                {
                  Errors.AddError(refText + "Id=" + id.ToString() + ". Неправильная ссылка " + refId.ToString() + ". В таблице \"" + masterTableName + "\" нет записи с таким идентификатором");
                  if (UseTrace)
                    Trace.TraceError("Error reference. " + refText + ". RefId=" + refId.ToString() + " (Id=" + id.ToString() + ")");
                }
              }

              if (Errors.Severity != ErrorMessageKind.Error)
                throw new BugException("Ошибка углубленного поиска ссылки в блоке");
            }
          }
        }
        spl.PercentMax = 0;
        spl.AllowCancel = false;
        spl.PhaseText = oldPT;
      }
    }

    #endregion

    #region Проверка целостности двоичных данных

    private void ValidateBinDataSums(ISimpleSplash spl)
    {
      if (UseTrace)
        Trace.WriteLine("Validating table BinData...");
      using (DBxCon con = new DBxCon(_GlobalData.BinDataHandler.MainEntry))
      {
        spl.PercentMax = con.GetRecordCount("BinData");
        spl.AllowCancel = true;

        int cnt = 0;
        int cntError = 0;
        using (DbDataReader rdr = con.ReaderSelect("BinData", new DBxColumns("Id,Length,MD5")))
        {
          while (rdr.Read())
          {
            cnt++;
            Int32 id = rdr.GetInt32(0);
            int length = rdr.GetInt32(1);
            string md5 = rdr.GetString(2);
            if (md5 == DBxBinDataHandler.EmptyMD5)
            {
              // 19.11.2020
              Errors.AddWarning("Таблица BinData, Id=" + id.ToString() + ". Нулевое значение MD5. Не проверяется");
              if (UseTrace)
                Trace.TraceWarning("BinData, Id=" + id.ToString() + ". MD5 is empty");
            }
            else
            {
              byte[] data;
              try
              {
                data = _GlobalData.BinDataHandler.GetBinData(id);
#if DEBUG
                if (data == null)
                  throw new NullReferenceException("Data==null");
#endif

                if (data.Length != length)
                {
                  Errors.AddError("Таблица BinData, Id=" + id.ToString() + ". Неправильная длина блока данных. Ожидалось: " + length.ToString() + ", прочитано: " + data.Length.ToString());
                  if (UseTrace)
                    Trace.TraceError("BinData, Id=" + id.ToString() + ". Invalid Length");
                  cntError++;
                }
                else
                {
                  string realMD5 = DataTools.MD5Sum(data);
                  if (md5 != realMD5)
                  {
                    Errors.AddError("Таблица BinData, Id=" + id.ToString() + ". Неправильный блок данных. Неправильная контрольная сумма");
                    if (UseTrace)
                      Trace.TraceError("BinData, Id=" + id.ToString() + ". Invalid MD5");
                    cntError++;
                  }
                }
              }
              catch (Exception e)
              {
                Errors.AddError("Таблица BinData, Id=" + id.ToString() + ". Ошибка чтения блока данных. " + e.Message);
                if (UseTrace)
                  Trace.TraceError("BinData, Id=" + id.ToString() + ". Cannot read the data. " + e.Message);
                cntError++;
              }
            }

            spl.IncPercent();
          }
        }
        spl.PercentMax = 0;

        Errors.AddInfo("Записей в таблице BinData (" + _GlobalData.BinDataHandler.MainEntry.DB.DisplayName + "): " + cnt.ToString() + ", с ошибками: " + cntError.ToString());
      }
    }

    #endregion

    #region Проверка повторов в таблице "BinData"

    private void ValidateBinDataRepeats(ISimpleSplash spl)
    {
      if (UseTrace)
        Trace.WriteLine("Search for MD5 repeats...");
      bool hasRepeats = false;
      using (DBxCon con = new DBxCon(_GlobalData.BinDataHandler.MainEntry))
      {
        spl.PercentMax = con.GetRecordCount("BinData");
        spl.AllowCancel = true;
        Int32 prevId = 0;
        string prevMD5 = null;
        using (DbDataReader rdr = con.ReaderSelect("BinData", new DBxColumns("Id,MD5"), null, new DBxOrder("MD5")))
        {
          while (rdr.Read())
          {
            Int32 id = rdr.GetInt32(0);
            string md5 = rdr.GetString(1);
            if (prevMD5 != null)
            {
              if (md5 == prevMD5)
              {
                Errors.AddWarning("Повтор в таблице BinData. Для Id=" + prevId.ToString() + " и Id=" + id.ToString() + " задана одинаковая сумма MD5=\"" + md5 + "\"");
                if (UseTrace)
                  Trace.TraceWarning("BinData has an MD5 twice. Id=" + prevId.ToString() + " and Id=" + id.ToString() + ". MD5=\"" + md5 + "\"");
                hasRepeats = true;
              }
            }
            prevId = id;
            prevMD5 = md5;

            spl.IncPercent();
          }
        }
        spl.PercentMax = 0;

        if (!hasRepeats)
          Errors.AddInfo("Повторов в таблице BinData (" + _GlobalData.BinDataHandler.MainEntry.DB.DisplayName + ") не обнаружено");
      }
    }

    #endregion

    #region Проверка фрагментированных баз

    private void ValidateBinDataStorage(int section, ISimpleSplash spl)
    {
      DBxEntry entry2 = _GlobalData.BinDataHandler.GetSectionEntry(section);
      spl.PhaseText = "Проверка BinDataStorage в базе данных " + entry2.DB.DisplayName;
      if (UseTrace)
        Trace.WriteLine("Validating " + entry2.DB.DisplayName + "...");
      bool hasWarnings = false;
      using (DBxCon con1 = new DBxCon(_GlobalData.BinDataHandler.MainEntry))
      {
        using (DBxCon con2 = new DBxCon(entry2))
        {
          spl.PercentMax = con2.GetRecordCount("BinDataStorage");
          spl.AllowCancel = true;

          using (DbDataReader rdr = con2.ReaderSelect("BinDataStorage", new DBxColumns("Id")))
          {
            while (rdr.Read())
            {
              try
              {
                Int32 id = rdr.GetInt32(0);
                //int WantedSection = DataTools.GetInt(Con1.GetValue("BinData", Id, "Section"));
                object oWantedSection;
                if (!con1.GetValue("BinData", id, "Section", out oWantedSection)) // 04.06.2020
                {
                  Errors.AddWarning("База данных " + entry2.DB.DisplayName + ", таблица BinDataStorage, Id=" + id.ToString() + ". Такого идентификатора нет в основной таблице BinData");
                  if (UseTrace)
                    Trace.TraceWarning(entry2.DB.DisplayName + ", BinDataStorage, Id=" + id.ToString() + ". There is no such Id in BinData");
                  hasWarnings = true;
                }
                else
                {
                  int wantedSection = DataTools.GetInt(oWantedSection);
                  if (wantedSection != section)
                  {
                    // Это считается предупреждением а не ошибкой.
                    // Либо блок данных присутствует сразу в двух секциях, и тогда лишний блок никому не мешает.
                    // Либо блока нет в нужной секции, тогда ошибка уже была выловлена при основной проверке
                    Errors.AddWarning("База данных " + entry2.DB.DisplayName + ", таблица BinDataStorage, Id=" + id.ToString() + ". В основной таблице BinData для этого идентификатора задана секция №" + wantedSection.ToString() + ", а не №" + section.ToString());
                    if (UseTrace)
                      Trace.TraceWarning(entry2.DB.DisplayName + ", BinDataStorage, Id=" + id.ToString() + ". BinData section number is different for this Id.");

                    hasWarnings = true;
                  }
                }
              }
              catch (Exception e)
              {
                Errors.AddError("База данных " + entry2.DB.DisplayName + ", таблица BinDataStorage. Неперехваченная ошибка: " + e.Message);
                if (UseTrace)
                  Trace.TraceError(entry2.DB.DisplayName + ", BinDataStorage. Exception occured. " + e.Message);
              }
              spl.IncPercent();
            }
          }
          spl.PercentMax = 0;
          spl.AllowCancel = false;
          if (hasWarnings)
            Errors.AddInfo("Обнаружены предупреждения по базе данных " + entry2.DB.DisplayName);
          else
            Errors.AddInfo("В BinDataStorage секции №" + section.ToString() + " (" + entry2.DB.DisplayName + ") нет ошибок");

        } // using Con2
      } // using Con1
    }

    #endregion
  }

  /// <summary>
  /// Результат поиска ссылок
  /// </summary>
  internal class DBxBinDataRefFinder
  {
    #region Конструктор

    /// <summary>
    /// Создает объект с пустыми списками
    /// </summary>
    public DBxBinDataRefFinder()
    {
      _MainBinDataRefs = new DBxTableColumnList();
      _UndoBinDataRefs = new DBxTableColumnList();
      _MainFileNameRefs = new DBxTableColumnList();
      _UndoFileNameRefs = new DBxTableColumnList();
    }

    #endregion

    #region Поиск

    /// <summary>
    /// Основной метод поиска.
    /// Заполняет списки
    /// </summary>
    /// <param name="globalData">Инициализированный объект DBxRealDocProviderGlobal </param>
    /// <param name="skipRealRefs">Если true, то для основной базы данных будут исключены реальные ссылочные поля.
    /// Это позволяет сэкономить время на проверке целостности данных, так как ссылочная целостность</param>
    public void FindRefs(DBxRealDocProviderGlobal globalData, bool skipRealRefs)
    {
#if DEBUG
      if (globalData == null)
        throw new ArgumentNullException("globalData");
#endif
      if (MainBinDataRefs.IsReadOnly)
        throw new InvalidOperationException("Повторный вызов");

      // Поиск в DBxDocTypes
      DBxTableColumnList binDataRefs = new DBxTableColumnList();
      DBxTableColumnList fileNameRefs = new DBxTableColumnList();
      FindFromDocTypes(globalData.DocTypes, binDataRefs, fileNameRefs);

      // Недостающая ссылка из FileNames
      if (globalData.BinDataHandler.UseFiles)
        binDataRefs.Add(new DBxTableColumnName("FileNames", "Data"));

      // Поиск в реальной структуре основной базы данных недостающих ссылок
      DBxStruct mainRealStr = globalData.MainDBEntry.DB.GetRealStruct();
      foreach (DBxTableStruct ts in mainRealStr.Tables)
      {
        foreach (DBxColumnStruct cs in ts.Columns)
        {
          // Не проверяем флаги UseXXX в DBxBinDataHandler.
          // 

          if (String.Equals(cs.MasterTableName, "BinData", StringComparison.OrdinalIgnoreCase))
            binDataRefs.Add(new DBxTableColumnName(ts.TableName, cs.ColumnName));
          else if (String.Equals(cs.MasterTableName, "FileNames", StringComparison.OrdinalIgnoreCase))
            fileNameRefs.Add(new DBxTableColumnName(ts.TableName, cs.ColumnName));
        }
      }

      if (skipRealRefs)
      {
        // Проверяем, объявлены ли в основной базе данных поля, как ссылочные. Вероятно, ответ всегда "Да"
        foreach (DBxTableColumnName tc in binDataRefs)
        {
          DBxColumnStruct cs = mainRealStr.Tables.GetRequired(tc.TableName).Columns.GetRequired(tc.ColumnName);
          if (String.IsNullOrEmpty(cs.MasterTableName))
            MainBinDataRefs.Add(tc);
          else if (!String.Equals(cs.MasterTableName, "BinData", StringComparison.OrdinalIgnoreCase))
            throw new BugException("В реальной структуре БД " + globalData.MainDBEntry.DB.DatabaseName +
              ", в таблице " + tc.TableName + ", поле " + tc.ColumnName + ", объявлена ссылка на таблицу \"" +
              cs.MasterTableName + "\", а не на \"BinData\"");
        }

        foreach (DBxTableColumnName tc in fileNameRefs)
        {
          DBxColumnStruct cs = mainRealStr.Tables.GetRequired(tc.TableName).Columns.GetRequired(tc.ColumnName);
          if (String.IsNullOrEmpty(cs.MasterTableName))
            MainFileNameRefs.Add(tc);
          else if (!String.Equals(cs.MasterTableName, "FileNames", StringComparison.OrdinalIgnoreCase))
            throw new BugException("В реальной структуре БД " + globalData.MainDBEntry.DB.DatabaseName +
              ", в таблице " + tc.TableName + ", поле " + tc.ColumnName + ", объявлена ссылка на таблицу \"" +
              cs.MasterTableName + "\", а не на \"FileNames\"");
        }
      }
      else
      {
        MainBinDataRefs.AddRange(binDataRefs);
        MainFileNameRefs.AddRange(fileNameRefs);
      }

      // Поиск в базе данных undo
      if (globalData.UndoDBEntry != null)
      {
        DBxStruct undoRealStr = globalData.UndoDBEntry.DB.GetRealStruct();

        foreach (DBxTableColumnName tc in binDataRefs)
        {
          DBxTableStruct ts = undoRealStr.Tables[tc.TableName];
          if (ts == null)
            continue;
          DBxColumnStruct cs = ts.Columns[tc.ColumnName];
          if (cs == null)
            continue;

          if (!String.IsNullOrEmpty(cs.MasterTableName))
            throw new BugException("В реальной структуре БД " + globalData.UndoDBEntry.DB.DatabaseName +
              ", в таблице " + tc.TableName + ", поле " + tc.ColumnName + " объявлено ссылочным");

          UndoBinDataRefs.Add(tc);
        }

        foreach (DBxTableColumnName tc in fileNameRefs)
        {
          DBxTableStruct ts = undoRealStr.Tables[tc.TableName];
          if (ts == null)
            continue;
          DBxColumnStruct cs = ts.Columns[tc.ColumnName];
          if (cs == null)
            continue;

          if (!String.IsNullOrEmpty(cs.MasterTableName))
            throw new BugException("В реальной структуре БД " + globalData.UndoDBEntry.DB.DatabaseName +
              ", в таблице " + tc.TableName + ", поле " + tc.ColumnName + " объявлено ссылочным");

          UndoFileNameRefs.Add(tc);
        }
      }

      // Предотвращаем повторный вызов
      SetReadOnly();
    }

    /// <summary>
    /// Поиск ссылок, объявленных в DBxDocTypes
    /// </summary>
    /// <param name="docTypes"></param>
    /// <param name="binDataRefs"></param>
    /// <param name="fileNameRefs"></param>
    private void FindFromDocTypes(DBxDocTypes docTypes, DBxTableColumnList binDataRefs, DBxTableColumnList fileNameRefs)
    {
      foreach (DBxDocType dt in docTypes)
      {
        FindFromDocTypeBase(dt, binDataRefs, fileNameRefs);
        foreach (DBxSubDocType sdt in dt.SubDocs)
          FindFromDocTypeBase(sdt, binDataRefs, fileNameRefs);
      }
    }

    private void FindFromDocTypeBase(DBxDocTypeBase dt, DBxTableColumnList binDataRefs, DBxTableColumnList fileNameRefs)
    {
      foreach (DBxDocTypeBinDataRef r in dt.BinDataRefs)
        binDataRefs.Add(new DBxTableColumnName(dt.Name, r.Column.ColumnName));
      foreach (DBxDocTypeFileRef r in dt.FileRefs)
        fileNameRefs.Add(new DBxTableColumnName(dt.Name, r.Column.ColumnName));
    }

    private void SetReadOnly()
    {
      MainBinDataRefs.SetReadOnly();
      UndoBinDataRefs.SetReadOnly();
      MainFileNameRefs.SetReadOnly();
      UndoBinDataRefs.SetReadOnly();
    }

    #endregion

    #region Результаты поиска

    /// <summary>
    /// Ссылки на таблицу BinData в основной базе данных
    /// </summary>
    public DBxTableColumnList MainBinDataRefs { get { return _MainBinDataRefs; } }
    private DBxTableColumnList _MainBinDataRefs;

    /// <summary>
    /// Псевдоссылки в БД истории на двоичные данные
    /// </summary>
    public DBxTableColumnList UndoBinDataRefs { get { return _UndoBinDataRefs; } }
    private DBxTableColumnList _UndoBinDataRefs;

    /// <summary>
    /// Ссылки на таблицу BinData в основной базе данных
    /// </summary>
    public DBxTableColumnList MainFileNameRefs { get { return _MainFileNameRefs; } }
    private DBxTableColumnList _MainFileNameRefs;

    /// <summary>
    /// Ссылки на таблицу BinData в основной базе данных
    /// </summary>
    public DBxTableColumnList UndoFileNameRefs { get { return _UndoFileNameRefs; } }
    private DBxTableColumnList _UndoFileNameRefs;

    /// <summary>
    /// Возвращает true, если все списки пустые
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return MainBinDataRefs.Count == 0 &&
          UndoBinDataRefs.Count == 0 &&
          MainFileNameRefs.Count == 0 &&
          UndoFileNameRefs.Count == 0;
      }
    }

    #endregion
  }
}
