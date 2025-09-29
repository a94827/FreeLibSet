// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Выборка документов. В одной выборке могут присутствовать документы разных
  /// видов. Документ не может входить в выборку дважды.
  /// Класс не является потокобезопасным.
  /// </summary>
  [Serializable]
  public sealed class DBxDocSelection: ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создать пустую выборку документов
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных (см. описание свойства)</param>
    public DBxDocSelection(string dbIdentity)
    {
      if (String.IsNullOrEmpty(dbIdentity))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("dbIdentity");

      _Data = new DataSet();
      //_Data.RemotingFormat = SerializationFormat.Binary;
      _DBIdentity = dbIdentity;
    }

    /// <summary>
    /// Создает выборку из одного документа
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных (см. описание свойства)</param>
    /// <param name="tableName">Имя таблицы документа DBxDocType.Name</param>
    /// <param name="id">Идентификатор документа</param>
    public DBxDocSelection(string dbIdentity, string tableName, Int32 id)
      : this(dbIdentity)
    {
      Add(tableName, id);
    }

    /// <summary>
    /// Создает выборку из нескольких документа
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных (см. описание свойства)</param>
    /// <param name="tableName">Имя таблицы документа DBxDocType.Name</param>
    /// <param name="ids">Идентификаторы документов</param>
    public DBxDocSelection(string dbIdentity, string tableName, IEnumerable<Int32> ids)
      : this(dbIdentity)
    {
      Add(tableName, ids);
    }

    /// <summary>
    /// Создает копию выборки документов
    /// </summary>
    /// <param name="source">Исходная выборка документов</param>
    public DBxDocSelection(DBxDocSelection source)
      :this(source.DBIdentity)
    {
      Add(source);
    }

    /// <summary>
    /// Создает выборку документов, куда помещает идентификаторы только одного вида документов из исходной выборки
    /// </summary>
    /// <param name="source">Исходная выборка документов</param>
    /// <param name="tableName">Имя таблицы</param>
    public DBxDocSelection(DBxDocSelection source, string tableName)
      : this(source.DBIdentity)
    {
      Add(tableName, source[tableName]);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор базы данных (задается в конструкторе)
    /// Используется при работе с буфером обмена, чтобы не допустить вставку данных из программы,
    /// работающей с другой базой данных
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private readonly string _DBIdentity;

    /// <summary>
    /// Основные данные. В каждой таблице имеется единственное поле Id
    /// </summary>
    private readonly DataSet _Data;

    /// <summary>
    /// Список имен таблиц, входящих в выборку
    /// </summary>
    public string[] TableNames
    {
      get
      {
        if (_TableNames == null)
        {
          _TableNames = new string[_Data.Tables.Count];
          for (int i = 0; i < _TableNames.Length; i++)
            _TableNames[i] = _Data.Tables[i].TableName;
        }
        return _TableNames;
      }
    }
    [NonSerialized]
    [XmlIgnore]
    private string[] _TableNames;

    /// <summary>
    /// Получить массив идентификаторов заданной таблицы, входящих в выборку.
    /// Если нет идентификаторов для таблицы <paramref name="tableName"/>, то возвращается пустой массив.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Массив идентификаторов</returns>
    public IIdSet<Int32> this[string tableName]
    {
      get
      {
        int p = _Data.Tables.IndexOf(tableName);
        if (p < 0)
          return IdList<Int32>.Empty;
        else
          return IdTools.GetIds<Int32>(_Data.Tables[p]);
      }
    }

    /// <summary>
    /// Получить число записей заданного вида.
    /// Если таблица не входит в выборку, возвращается 0.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Количество идентификаторов</returns>
    public int GetCount(string tableName)
    {
      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return 0;
      else
        return _Data.Tables[p].Rows.Count;
    }

    /// <summary>
    /// Общее число ссылок в выборке
    /// </summary>
    public int TotalCount
    {
      get
      {
        int cnt = 0;
        foreach (DataTable table in _Data.Tables)
          cnt += table.Rows.Count;
        return cnt;
      }
    }

    /// <summary>
    /// Возвращает true, если выборка не содержит ни одного документа
    /// </summary>
    public bool IsEmpty
    {
      get { return TotalCount == 0; }
    }

    #endregion

    #region Добавление / удаление документов

    ///// <summary>
    ///// Внутренний метод доступа к таблице с идентификаторами
    ///// </summary>
    ///// <param name="TableName"></param>
    ///// <returns></returns>
    //public DataTable GetTable(string TableName)
    //{
    //  return ReadyTable(TableName, true);
    //}

    private DataTable ReadyTable(string tableName, bool create)
    {
#if DEBUG
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
#endif
      int p = _Data.Tables.IndexOf(tableName);
      if (p >= 0)
        return _Data.Tables[p];
      if (!create)
        return null;

      _TableNames = null; // список имен таблиц больше недействителен

      // Добавляем таблицу
      DataTable table = _Data.Tables.Add(tableName);
      table.Columns.Add("Id", typeof(Int32));
      DataTools.SetPrimaryKey(table, "Id");
      return table;
    }

    /// <summary>
    /// Добавить документ в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="id">Идентификатор документа</param>
    /// <returns>1, если документ был добавлен, 0, если нет (уже есть в выборке или <paramref name="id"/>=0</returns>
    public int Add(string tableName, Int32 id)
    {
      if (id == 0)
        return 0;
      DataTable resTable = ReadyTable(tableName, true);
      DataRow dummyRow;
      return DataTools.FindOrAddPrimaryKeyRow(resTable, id, out dummyRow) ? 1 : 0;
    }

    /// <summary>
    /// Добавить несколько документов в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для добавления</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(string tableName, IEnumerable<Int32> ids)
    {
      if (ids == null)
        return 0;
      DataTable resTable = null;
      int cnt = 0;
      foreach (Int32 id in ids)
      {
        if (id == 0)
          continue;
        resTable = ReadyTable(tableName, true);
        DataRow dummyRow;
        if (DataTools.FindOrAddPrimaryKeyRow(resTable, id, out dummyRow))
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Добавить документы из другой выборки
    /// </summary>
    /// <param name="source">Исходная выборка</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(DBxDocSelection source)
    {
      if (source == null)
        return 0;

      int cnt = 0;
      foreach (DataTable srcTable in source._Data.Tables)
      {
        DataTable resTable = ReadyTable(srcTable.TableName, true);
        foreach (DataRow srcRow in srcTable.Rows)
        {
          // Можно не проверять удаленные строки
          Int32 id = (Int32)(srcRow["Id"]);
          DataRow dummyRow;
          if (DataTools.FindOrAddPrimaryKeyRow(resTable, id, out dummyRow))
            cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Добавить документы в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="table">Таблица, содержащая столбец "Id" с идентификаторами добавляемых документов</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(string tableName, DataTable table)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      DataTable resTable = ReadyTable(tableName, true);
      int cnt = 0;
      foreach (DataRow SrcRow in table.Rows)
      {
        if (SrcRow.RowState == DataRowState.Deleted)
          continue; // 27.09.2017

        Int32 id = (Int32)(SrcRow["Id"]);
        DataRow dummyRow;
        if (DataTools.FindOrAddPrimaryKeyRow(resTable, id, out dummyRow))
          cnt++;
      }

      return cnt;
    }

    /// <summary>
    /// Добавить документы в выборку.
    /// Свойство <paramref name="table"/>.TableName определяет вид документов.
    /// </summary>
    /// <param name="table">Таблица, содержащая столбец "Id" с идентификаторами добавляемых документов</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(DataTable table)
    {
      if (table == null)
        return 0;
      return Add(table.TableName, table);
    }

    /// <summary>
    /// Удалить документ из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="id">Идентификатор удаляемого документа</param>
    /// <returns>1, если документ был удален, 0, если такого документа нет в выборке</returns>
    public int Remove(string tableName, Int32 id)
    {
      if (id == 0)
        return 0;
      DataTable resTable = ReadyTable(tableName, false);
      if (resTable == null)
        return 0;
      DataRow row = resTable.Rows.Find(id);
      if (row != null)
      {
        resTable.Rows.Remove(row);
        return 1;
      }
      else
        return 0;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName, IEnumerable<Int32> ids)
    {
      if (ids == null)
        return 0;
      DataTable resTable = ReadyTable(tableName, false);
      if (resTable == null)
        return 0;

      int cnt = 0;
      foreach (Int32 id in ids)
      {
        DataRow row = resTable.Rows.Find(id);
        if (row != null)
        {
          resTable.Rows.Remove(row);
          cnt++;
        }
      }

      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="source">Выборка, содержащая документы, которые надо удалить из текущей выборки</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(DBxDocSelection source)
    {
      if (source == null)
        return 0;

      int cnt = 0;
      foreach (DataTable srcTable in source._Data.Tables)
      {
        DataTable resTable = ReadyTable(srcTable.TableName, false);
        if (resTable == null)
          continue;
        foreach (DataRow srcRow in srcTable.Rows)
        {
          Int32 id = (Int32)(srcRow["Id"]);
          DataRow row = resTable.Rows.Find(id);
          if (row != null)
          {
            resTable.Rows.Remove(row);
            cnt++;
          }
        }
      }

      return cnt;
    }

    /// <summary>
    /// Удалить все ссылки заданного вида
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName)
    {
      DataTable resTable = ReadyTable(tableName, false);
      if (resTable == null)
        return 0;
      int cnt = resTable.Rows.Count;
      _Data.Tables.Remove(resTable);
      _TableNames = null;
      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="table">Таблица, в которой есть столбец "Id", содержащий идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName, DataTable table)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      DataTable resTable = ReadyTable(tableName, false);
      if (resTable == null)
        return 0;

      int cnt = 0;
      foreach (DataRow srcRow in table.Rows)
      {
        if (srcRow.RowState == DataRowState.Deleted)
          continue; // 27.09.2017
        Int32 id = (Int32)(srcRow["Id"]);
        DataRow Row = resTable.Rows.Find(id);
        if (Row != null)
        {
          resTable.Rows.Remove(Row);
          cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки.
    /// Свойство <paramref name="table"/>.TableName задает вид удаляемых документов.
    /// </summary>
    /// <param name="table">Таблица, в которой есть столбец "Id", содержащий идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(DataTable table)
    {
      if (table == null)
        return 0;
      return Remove(table.TableName, table);
    }

    /// <summary>
    /// Удаляет из текущей выборки все ссылки, которых нет в выборке <paramref name="source"/>.
    /// Если <paramref name="source"/> - пустая выборка, то будут удалены все документы.
    /// Таким образом, текущая выборка становится пересечением текущей выборки и <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Выборка для сравнения (не изменяется)</param>
    /// <returns>Число удаленных ссылок</returns>
    public int RemoveNeg(DBxDocSelection source)
    {
      int cnt = 0;
      if (source == null)
      {
        cnt = TotalCount;
        Clear();
        return cnt;
      }
      if (source.IsEmpty)
      {
        cnt = TotalCount;
        Clear();
        return cnt;
      }

      for (int i = _Data.Tables.Count - 1; i >= 0; i--)
      {
        DataTable resTable = _Data.Tables[i];
        DataTable srcTable = source.ReadyTable(resTable.TableName, false);
        if (srcTable == null)
        {
          cnt += resTable.Rows.Count;
          _Data.Tables.RemoveAt(i);
          _TableNames = null;
          continue;
        }

        for (int j = resTable.Rows.Count - 1; j >= 0; j--)
        {
          DataRow resRow = resTable.Rows[j];
          Int32 id = (Int32)(resRow["Id"]);
          if (srcTable.Rows.Find(id) == null)
          {
            resTable.Rows.RemoveAt(j);
            cnt++;
          }
        }
      }

      return cnt;
    }

    /// <summary>
    /// Очистка всей выборки
    /// </summary>
    public void Clear()
    {
      _Data.Tables.Clear();
      _TableNames = null;
    }

    #endregion

    #region Поиск

    /// <summary>
    /// Получить первый идентификатор заданного документа или 0, если выборка не
    /// содержит таких документов
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <returns>Идентификатор первого документа таблицы или 0</returns>
    public Int32 GetSingleId(string tableName)
    {
      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return 0;
      else
      {
        if (_Data.Tables[p].Rows.Count == 0)
          return 0;
        else
          return DataTools.GetInt32(_Data.Tables[p].Rows[0], "Id");
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть заданный документ
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="id">Идентификатор документа для поиска</param>
    /// <returns>true, если идентификатор есть в выборке</returns>
    public bool Contains(string tableName, Int32 id)
    {
      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
        return _Data.Tables[p].Rows.Find(id) != null;
    }

    /// <summary>
    /// Возвращает true, если в выборке есть хотя бы один документ из списка.
    /// Если список идентификаторов пуст, возвращает false.
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для поиска</param>
    /// <returns>true, если документ найден</returns>
    public bool ContainsAny(string tableName, IEnumerable<Int32> ids)
    {
      if (ids == null)
        return false; // 19.01.2021

      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
      {
        foreach (Int32 id in ids)
        {
          if (_Data.Tables[p].Rows.Find(id) != null)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть хотя бы один документ из списка.
    /// Если выборка <paramref name="source"/> - пустая, возвращается false.
    /// </summary>
    /// <param name="source">Выборка документов, которые надо найти</param>
    /// <returns>true, если документ найден</returns>
    public bool ContainsAny(DBxDocSelection source)
    {
      if (source == null)
        return true; // 19.01.2021
      if (source.IsEmpty)
        return true; // 19.01.2021

      foreach (DataTable srcTable in source._Data.Tables)
      {
        DataTable resTable = ReadyTable(srcTable.TableName, false);
        if (resTable == null)
          continue;
        foreach (DataRow srcRow in srcTable.Rows)
        {
          // Можно не проверять удаленные строки
          Int32 id = (Int32)(srcRow["Id"]);
          if (resTable.Rows.Find(id) != null)
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в выборке есть все документы из списка.
    /// Если список идентификаторов пустой, возвращается true.
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для поиска</param>
    /// <returns>true, если документы найдены</returns>
    public bool ContainsAll(string tableName, IEnumerable<Int32> ids)
    {
      if (ids == null)
        return true;

      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
      {
        foreach (Int32 id in ids)
        {
          if (_Data.Tables[p].Rows.Find(id) == null)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть все документы из другой выборки.
    /// Если выборка <paramref name="source"/> пустая, возвращается true.
    /// </summary>
    /// <param name="source">Выборка документов, которые надо найти</param>
    /// <returns>true, если документы найдены</returns>
    public bool ContainsAll(DBxDocSelection source)
    {
      if (source == null)
        return true;
      if (source.IsEmpty)
        return true;

      foreach (DataTable srcTable in source._Data.Tables)
      {
        if (srcTable.Rows.Count == 0)
          continue;

        DataTable resTable = ReadyTable(srcTable.TableName, false);
        if (resTable == null)
          return false;

        foreach (DataRow srcRow in srcTable.Rows)
        {
          // Можно не проверять удаленные строки
          Int32 id = (Int32)(srcRow["Id"]);
          if (resTable.Rows.Find(id) == null)
            return false;
        }
      }
      return true;
    }

    #endregion

    #region Сериализация

    [OnSerializing()]
    private void OnSerializingMethod(StreamingContext context)
    {
      // Перед сериализацией сохраняем все изменения
      //_Data.AcceptChanges();
      SerializationTools.PrepareDataSet(_Data); // 07.07.2022
    }

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Запись в секцию конфигурацию
    /// В переданном узле создается полдузел "Tables" с вложенными подузлами, соответствующими
    /// именам таблиц в списке TableNames. Для каждого подузла создается строковый параметр "Ids".
    /// В строку записываются идентификаторы документов, разделенные запятыми.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (IsEmpty)
        cfg.Remove("Tables");
      else
      {
        CfgPart part2 = cfg.GetChild("Tables", true);
        part2.Clear();
        for (int i = 0; i < TableNames.Length; i++)
        {
          IIdSet<Int32> ids = this[TableNames[i]];
          if (ids.Count == 0)
            continue;
          CfgPart part3 = part2.GetChild(TableNames[i], true);
          string s = StdConvert.ToString(ids.ToArray());
          part3.SetString("Ids", s);
        }
      }
    }

    /// <summary>
    /// Чтение выборки, которая была записана вызовом WriteConfig()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      Clear();
      CfgPart part2 = cfg.GetChild("Tables", false);
      if (part2 == null)
        return;
      string[] tableNames = part2.GetChildNames();
      for (int i = 0; i < tableNames.Length; i++)
      {
        CfgPart part3 = part2.GetChild(tableNames[i], false);
        if (part3 == null)
          continue; // ошибка
        string s = part3.GetString("Ids");
        Int32[] ids = StdConvert.ToInt32Array(s);
        Add(tableNames[i], ids);
      }
    }

    #endregion

    // TODO: Нет доступа к сохраненным документам выборки

#if XXXX

    #region Нормализация

    /// <summary>
    /// Возвращает true, если выборка содержит ссылки на другие выборки и
    /// нуждается в нормализации
    /// </summary>
    public bool NormalizeNeeded
    {
      get { return GetCount("Выборки") > 0; }
    }

    /// <summary>
    /// Нормализация выборки.
    /// Заменяет ссылки на другие выборки их содержимым
    /// Возвращает true, если нормализация выполнена
    /// </summary>
    /// <param name="BufTables">Доступ к буферизации таблиц</param>
    /// <returns>true, если нормализация выполнена</returns>
    public bool Normalize(BufTables BufTables)
    {
      Int32[] Ids = this["Выборки"];
      if (Ids.Length == 0)
        return false;
      List<Int32> ReadyIds = new List<Int32>();
      for (int i = 0; i < Ids.Length; i++)
        NormalizeOneDocSel(Ids[i], ReadyIds, BufTables);
      Remove("Выборки");
      return true;
    }

    /// <summary>
    /// Рекурсивная процедура нормализации одной выборки
    /// </summary>
    /// <param name="DocSelId">Идентификатор текущей выборки</param>
    /// <param name="ReadyIds">Список уже обработанных выборок</param>
    /// <param name="BufTables">Доступ к данным</param>
    private void NormalizeOneDocSel(Int32 DocSelId, List<Int32> ReadyIds, BufTables BufTables)
    {
      if (ReadyIds.Contains(DocSelId))
        return; // уже обработана

      // Предотвращаем повторную обработку
      ReadyIds.Add(DocSelId);

      // Загружаем идентификаторы выборки
      string XmlText = DataTools.GetString(BufTables.GetValue("Выборки", DocSelId, "Данные"));
      ConfigSection Sect = new ConfigSection();
      Sect.AsXmlText = XmlText;
      DBxDocSelection DocSel2 = new DBxDocSelection(DBIdentity);
      DocSel2.ReadConfig(Sect);

      // Добавляем ссылки из другой выборки
      for (int i = 0; i < DocSel2.TableNames.Length; i++)
      {
        Int32[] Ids2 = DocSel2[DocSel2.TableNames[i]];
        if (DocSel2.TableNames[i] == "Выборки")
        {
          // Рекурсивный вызов
          for (int j = 0; j < Ids2.Length; j++)
            NormalizeOneDocSel(Ids2[j], ReadyIds, BufTables);
        }
        else
          // Обычное добавление
          Add(DocSel2.TableNames[i], Ids2);
      }
    }

    #endregion

#endif

    #region Текстовое представление

    /// <summary>
    /// Для отладки.
    /// Для вывода содержимого выборки пользователю используйте перегрузку метода с аргументом DBxDocTextHandlers.
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      //sb.Append("TotalCount=");
      //sb.Append(TotalCount);
      for (int i = 0; i < _Data.Tables.Count; i++)
      {
        if (i > 0)
          sb.Append(", ");
        else
          sb.Append(": ");

        sb.Append(_Data.Tables[i].TableName);
        sb.Append(" (");
        sb.Append(_Data.Tables[i].Rows.Count);
        sb.Append(")");
      }
      return sb.ToString();
    }

    /// <summary>
    /// Улучшенный метод получения текстового представления для выборки документов
    /// </summary>
    /// <param name="textHandlers">Обработчики текстового представления документов</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(DBxDocTextHandlers textHandlers)
    {
#if DEBUG
      if (textHandlers == null)
        throw new ArgumentNullException("textHandlers");
#endif

      if (IsEmpty)
        return Res.DBxDocSelection_Msg_Empty;
      if (TotalCount == 1)
      {
        DBxDocType docType = textHandlers.DocTypes[_Data.Tables[0].TableName];
        string sDTN = docType.SingularTitle;
        Int32 docId = this[docType.Name].SingleId;
        return sDTN + " " + textHandlers.GetTextValue(docType.Name, docId);
      }

      StringBuilder sb = new StringBuilder();
      //sb.Append("TotalCount=");
      //sb.Append(TotalCount);
      for (int i = 0; i < _Data.Tables.Count; i++)
      {
        if (i > 0)
          sb.Append(", ");
        else
          sb.Append(": ");

        DBxDocType docType = textHandlers.DocTypes[_Data.Tables[i].TableName];
        int n = _Data.Tables[i].Rows.Count;
        if (n == 1)
          sb.Append(docType.SingularTitle);
        else
          sb.Append(docType.PluralTitle);

        sb.Append(" (");
        sb.Append(n);
        sb.Append(")");
      }
      return sb.ToString();
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию выборки
    /// </summary>
    /// <returns></returns>
    public DBxDocSelection Clone()
    {
      return new DBxDocSelection(this);
    }

    object ICloneable.Clone()
    {
      return new DBxDocSelection(this);
    }

    #endregion
  }
}
