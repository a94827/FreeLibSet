// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /*
   * Структура базы данных
   * ---------------------
   * 
   * Библиотека реализует следующие сценарии получения описаний структуры базы данных
   * 1. Извлекается реальная структура на основании вызовов DBConnection.GetSchema()
   * 2. Задается желаемая структура. При этом в запросах можно использовать только те таблицы и поля,
   *    которые описаны в желаемой структуре. На основании желаемой структуры можно выполнить создание / обновление
   *    реальной структуры БД (Создание / обновление структуры БД не является обязательным)
   * 3. Комбинированный метод. Извлекается реальная структура, в нее вносятся необходимые дополнения (например,
   *    описания ссылочных полей, которые нельзя определить из реальной структуры), затем измененная структура
   *    замещает исходную. При этом создание / обновление структуры таблиц БД не выполняется
   * 
   * Сценарий 1
   * По умолчанию используется автоматический способ получения реальной структуры. Никаких действий для этого
   * выполнять не требуется. При обращении к свойству DBx.Struct[ИмяТаблицы], если необходимо, выполняется
   * вызов DBConnection.GetSchema() и возвращается объект DBxTableStruct с доступом только для чтения
   * 
   * Полная реальная структура базы данных может быть очень большой, если база данных содержит множество
   * таблиц. Поэтому нецелесообразно загружать ее за один раз, особенно, если запросы выполняются к небольшому
   * числу таблиц. Загрузка структуры выполняется по одной таблице.
   * 
   * Сценарий 2
   * Для ручного описания структуры требуется создать объект DBxStruct и добавить в него описания таблиц.
   * После этого должно быть установлено свойство DBx.Struct. В результате, объект DBxStruct переводится
   * в режим "только для просмотра"
   * 
   * Чтобы выполнить создание / обовление структуры таблицы на основании DBx.Struct, должен быть явно вызван 
   * метод DBx.UpdateDBStruct()
   * 
   * Сценарий 3
   * Сначала берется реальная структура таблицы (читается свойство DBx.Struct) и выполняется вызов метод
   * Clone(), который создает копию объекта DBxStruct, который можно модифицировать (ReadOnly=false).
   * После этого, в копию вносятся изменения. Затем, свойство DBx.Struct устанавливается
   * 
   * Сериализация
   * ------------
   * Описания являются сериализуемыми, т.к. должны уметь передаваться от сервера к клиенту. Если сделать класс 
   * DBxStruct сериализуемым, то перед передачей должны быть загружены описания для всех таблиц.
   * Если сделать объект DBxStruct передаваемым по ссылке (Marshal-by-Reference), то при каждом обращении
   * клиента к описанию таблицы, оно будет передаваться заново.
   *
   * Решение.
   * Класс DBxStruct не является не сериализуемым, ни Marshal-by-reference. Вместо этого, объект ссылается на
   * промежуточный объект, производный от DBxStructSource, который является Marshal-by-reference, и обеспечивает 
   * загрузку описаний (DBxRealStructSource извлекает описание из объекта DBx, а DBxRemoteStructSource - из 
   * другого объекта DBxStruct). Объект DBxStruct содержит буферизуемый список описаний. Если объект содержит
   * структуру, созданную вручную (сценарии 2 и 3), то ссылки на DBxStructSource нет. Для передачи структуры
   * клиенту, на сервере создается объект DBxRemoteStructSource, который передается по сслыке клиенту. 
   * Клиент создает собственный объект DBxStruct, который ссылается на полученный DBxRemoteStructSource)
   */

  /// <summary>
  /// Базовый класс для DBxRealStructSource и DBxRemoteStructSource 
  /// </summary>
  public interface IDBxStructSource
  {
    #region Методы

    /// <summary>
    /// Получить полный список имен таблиц в виде массива (без структуры)
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    string[] GetAllTableNames();

    /// <summary>
    /// Получить описание структуры одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненное описание структуры таблицы</returns>
    DBxTableStruct GetTableStruct(string tableName);

    #endregion
  }

  /// <summary>
  /// Переходник между DBxStruct и DBx для извлечения описаний таблиц
  /// </summary>
  public sealed class DBxRealStructSource : MarshalByRefObject, IDBxStructSource
  {
    #region Конструктор

    /// <summary>
    /// Созает переходник
    /// </summary>
    /// <param name="entry">Точка подключения</param>
    public DBxRealStructSource(DBxEntry entry)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");

      _Entry = entry;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Точка подключения. 
    /// При вызове методов для нее создается соединение с базой данных.
    /// Методы соединения используются для извлечения информации.
    /// Задается в конструкторе.
    /// </summary>
    public DBxEntry Entry { get { return _Entry; } }
    private readonly DBxEntry _Entry;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Получить полный список имен таблиц в виде массива (без структуры)
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    public string[] GetAllTableNames()
    {
      string[] a;
      using (DBxConBase Con = Entry.CreateCon())
      {
        a = Con.GetAllTableNamesFromSchema();
      }
      return a;
    }

    /// <summary>
    /// Получить описание структуры одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненное описание структуры таблицы</returns>
    public DBxTableStruct GetTableStruct(string tableName)
    {
      DBxTableStruct Obj;

      using (DBxConBase Con = Entry.CreateCon())
      {
        Obj = Con.GetRealTableStructFromSchema(tableName);
      }

      return Obj;
    }

    #endregion
  }

  /// <summary>
  /// Переходник для передачи описаний структуры таблиц от сервера к клиенту
  /// Экземпляр объекта, присоедиеннный к объекту, возвращаемому DBx.Struct, передается по ссылке
  /// клиенту. На стороне клиента создается собственный DBxStruct, присоединенный к этому объекту
  /// </summary>
  public sealed class DBxRemoteStructSource : MarshalByRefObject, IDBxStructSource
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="source">Структура базы данных.
    /// Ссылка сохраняется в private-поле, чтобы к ней нельзя было получить неавторизованный доступ</param>
    public DBxRemoteStructSource(DBxStruct source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
    }

    #endregion

    #region Поля

    /// <summary>
    /// Эта ссылка напрямую клиенту не видна
    /// </summary>
    private readonly DBxStruct _Source;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Получить полный список имен таблиц в виде массива (без структуры)
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    public string[] GetAllTableNames()
    {
      return _Source.AllTableNames;
    }

    /// <summary>
    /// Получить описание структуры одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненное описание структуры таблицы</returns>
    public DBxTableStruct GetTableStruct(string tableName)
    {
      return _Source.Tables[tableName];
    }

    #endregion
  }

  /// <summary>
  /// Описание реальной структуры базы данных
  /// Все обращения к присоединенной к базе данных структуре, являются потокобезопасными. 
  /// В процессе ручного заполнения, обращения не являются потокобезопасными
  /// </summary>
  public sealed class DBxStruct : IReadOnlyObject, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор создает описание структуры для ручного заполнения
    /// После заполнения структуры должно быть установлено свойство DBx.Struct.
    /// Чтобы создать или обновить реальную структуру БД, также должен быть вызван метод DBx.UpdateStruct()
    /// </summary>
    public DBxStruct()
      : this((IDBxStructSource)null)
    {
    }

    /// <summary>
    /// Этот конструктор должен вызываться на стороне клиента.
    /// Если <paramref name="source"/> не null, то структура будет находиться в режиме "только для чтения".
    /// </summary>
    /// <param name="source">Источник данных для получения структуры, реализующий передачу marshal-by-Reference</param>
    public DBxStruct(IDBxStructSource source)
    {
      _Tables = new DBxTableStructList(this);
      _Source = source;
      if (source != null)
        _IsReadOnly = true;
    }

    /// <summary>
    /// Этот конструктор должен вызываться на стороне клиента
    /// Структура будет находиться в режиме "только для чтения".
    /// </summary>
    /// <param name="con">Полученное клиентское соединение с базой данных. Не может быть null</param>
    public DBxStruct(DBxCon con)
      : this(con.StructSource)
    {
    }

    /// <summary>
    /// Конструктор для копирования
    /// </summary>
    /// <param name="source">Оригинальная структура</param>
    private DBxStruct(DBxStruct source)
    {
      _Tables = new DBxTableStructList(this, source.Tables);
      _Source = null;
      _IsReadOnly = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Источник для загрузки описаний таблиц, если список предназначен для получения реальной структуры
    /// базы данных или передан клиенту по сети
    /// </summary>
    public IDBxStructSource Source { get { return _Source; } }
    private readonly IDBxStructSource _Source;

    /// <summary>
    /// Полный список таблиц
    /// </summary>
    public string[] AllTableNames { get { return _Tables.AllTableNames; } }

    /// <summary>
    /// Доступ к структуре таблиц
    /// </summary>
    public DBxTableStructList Tables { get { return _Tables; } }
    private readonly DBxTableStructList _Tables;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание структуры было переведено в режим просмотра
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит описание структуры в режим просмотра.
    /// Повторные вызовы метода игнорируются
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary> 
    /// Возвращает копию объекта, доступную для редактирования (IsReadOnly=false). 
    /// Копия не ссылается на источник описаний таблиц Source. 
    /// В процессе копирования реальной структуры данных извлекаются описания для всех таблиц.
    /// </summary>
    public DBxStruct Clone()
    {
      return new DBxStruct(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию структуры базы данных, если текущий объект предназначен только для чтения.
    /// Если свойство IsReadOnly=false, возвращается ссылка на текущий объект
    /// </summary>
    /// <returns></returns>
    public DBxStruct CloneIfReadOnly()
    {
      if (IsReadOnly)
        return Clone();
      else
        return this;
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Создает объект DataSet, содержащий пустые DataTable для всех таблицы
    /// </summary>
    /// <returns>DataSet с таблицами, но без строк</returns>
    public DataSet CreateDataSet()
    {
      DataSet ds = new DataSet();
      string[] a = AllTableNames; // этот вызов может быть медленным
      for (int i = 0; i < a.Length; i++)
      {
        DBxTableStruct ts = Tables[AllTableNames[i]];
        ds.Tables.Add(ts.CreateDataTable());
      }
      return ds;
    }

    /// <summary>
    /// Создает пустую таблицу с заданным списком столбцов.
    /// Можно использовать ссылочные столбцы, содержащие "."
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов. Если null, то возвращаются все столбцы таблицы (DBxTableStruct.CreateDataTable)</param>
    /// <returns></returns>
    public DataTable CreateDataTable(string tableName, DBxColumns columnNames)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      DBxTableStruct ts = Tables[tableName];
      if (ts == null)
        throw new ArgumentException("Неизвестное имя таблицы \"" + tableName + "\". Нет описания структуры таблицы", "tableName");

      if (columnNames == null)
        return ts.CreateDataTable();

      DataTable Table = new DataTable(ts.TableName);
      for (int i = 0; i < columnNames.Count; i++)
      {
        DBxColumnStruct ColDef = this.FindColumn(tableName, columnNames[i]);
        if (ColDef == null)
          throw new ArgumentException("Для таблицы \"" + tableName + "\" не определен столбец \"" + columnNames[i] + "\"", "columnNames");
        Table.Columns.Add(ColDef.CreateDataColumn(columnNames[i]));
      }
      return Table;
    }

    /// <summary>
    /// Поиск столбца таблиц по имени.
    /// Позволяет использовать в качестве имени столбца ссылочное поле, содержащее точки.
    /// Возвращает null, если таблица или столбец не найдены
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описание столбца</returns>
    public DBxColumnStruct FindColumn(string tableName, string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        return null;
      DBxTableStruct ts = Tables[tableName];
      while (ts != null)
      {
        int p = columnName.IndexOf('.');
        if (p >= 0)
        {
          // Ссылочное поле
          string RefColName = columnName.Substring(0, p);
          columnName = columnName.Substring(p + 1);
          DBxColumnStruct RefCol = ts.Columns[RefColName];
          if (RefCol == null)
            return null;
          if (String.IsNullOrEmpty(RefCol.MasterTableName))
            return null;
          ts = Tables[RefCol.MasterTableName];
        }
        else
        {
          // Обычно поле
          return ts.Columns[columnName];
        }
      }
      return null;
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Source == null)
        return "Manual, Tables.Count=" + _Tables.Count.ToString();
      else
        return "Real";
    }

    /// <summary>
    /// Возвращает true, если есть комментарий хотя бы к одной из таблиц, столбцу или индексу
    /// </summary>
    public bool HasComments
    {
      get
      {
        for (int i = 0; i < Tables.Count; i++)
        {
          if (Tables[i].HasComments)
            return true;
        }
        return false;
      }
    }

    #endregion

    #region Проверка структуры базы данных

    /// <summary>
    /// Проверка структуры базы данных.
    /// Вызываются методы DBxTableStruct.CheckStruct() и проверяет корректность ссылочных полей.
    /// Эта версия не проверяет корректность имен обычных полей
    /// </summary>
    public void CheckStruct()
    {
      CheckStruct(null);
    }

    /// <summary>
    /// Проверка структуры базы данных.
    /// Вызываются методы DBxTableStruct.CheckStruct() и проверяет корректность ссылочных полей.
    /// Эта версия проверяет корректность имен обычных полей, если параметр <paramref name="db"/> задан.
    /// </summary>
    /// <param name="db">База данных для проверки корректности имен полей или null</param>
    public void CheckStruct(DBx db)
    {
      foreach (DBxTableStruct Table in Tables)
      {
        Table.CheckStruct(db);

        foreach (DBxColumnStruct Col in Table.Columns)
        {
          if (!String.IsNullOrEmpty(Col.MasterTableName))
          {
            DBxTableStruct MasterTable = Tables[Col.MasterTableName];
            if (MasterTable == null)
              throw new DBxStructException(Table, "В таблице \"" + Table.TableName + "\" ссылочное поле \"" + Col.ColumnName + "\" ссылается на несуществующую таблицу \"" + Col.MasterTableName + "\"");
            switch (MasterTable.PrimaryKey.Count)
            {
              case 0:
                throw new DBxStructException(Table, "В таблице \"" + Table.TableName + "\" ссылочное поле \"" + Col.ColumnName + "\" ссылается на таблицу \"" + MasterTable.TableName + "\", которая не имеет первичного ключа");
              case 1:
                DBxColumnStruct PKCol = MasterTable.Columns[MasterTable.PrimaryKey[0]];
                if (PKCol.ColumnType != Col.ColumnType)
                  throw new DBxStructException(Table, "В таблице \"" + Table.TableName + "\" ссылочное поле \"" + Col.ColumnName + "\" типа \"" + Col.ColumnType.ToString() + "\" ссылается на таблицу \"" + MasterTable.TableName + "\", которая имеет первичный ключ по полю \"" + PKCol.ColumnType + "\"типа " + PKCol.ColumnType);
                break;
              default:
                throw new DBxStructException(Table, "В таблице \"" + Table.TableName + "\" ссылочное поле \"" + Col.ColumnName + "\" ссылается на таблицу \"" + MasterTable.TableName + "\", которая имеет составной первичный ключ");
            }
          }
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Список описаний структуры таблиц для реализации свойства DBxStruct.Tables
  /// </summary>
  public sealed class DBxTableStructList : IList<DBxTableStruct>
  {
    #region Защищенные конструкторы

    internal DBxTableStructList(DBxStruct owner)
    {
      _Owner = owner;
      _List = new NamedList<DBxTableStruct>(true);
    }

    internal DBxTableStructList(DBxStruct owner, DBxTableStructList source)
    {
      _Owner = owner;
      _List = new NamedList<DBxTableStruct>(source.AllTableNames.Length, true);
      _AllTableNames = source.AllTableNames;
      for (int i = 0; i < source.AllTableNames.Length; i++)
      {
        DBxTableStruct OrgTable = source[source.AllTableNames[i]];
        _List.Add(OrgTable.Clone());
      }
    }

    #endregion

    #region Поля

    private readonly DBxStruct _Owner;

    private readonly NamedList<DBxTableStruct> _List;

    #endregion

    #region Полный список имен таблиц

    internal string[] AllTableNames
    {
      get
      {
        lock (_List)
        {
          if (_AllTableNames == null)
          {
            if (_Owner.Source == null)
            {
              // Собираем список таблиц из созданных структур
              _AllTableNames = new string[_List.Count];
              for (int i = 0; i < _List.Count; i++)
                _AllTableNames[i] = _List[i].TableName;
            }
            else
            {
              // Извлекаем список таблиц из источника
              _AllTableNames = _Owner.Source.GetAllTableNames();
            }
          }
          return _AllTableNames;
        }
      }
    }
    private string[] _AllTableNames;

    #endregion

    #region Поиск таблицы по имени

    // При обращениях, для синхронизации используется блокировка объекта FList 
    // Блокировка требуется только для "читающих" методов,
    // т.к. "записывающие" методы могут вызываться только в процессе ручного создания (всегда синхронного),
    // а затем - блокируются IsReadOInly

    /// <summary>
    /// Возвращает структуру таблицы по имени.
    /// Если нет такой таблицы, возвращается null
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура таблицы</returns>
    public DBxTableStruct this[string tableName]
    {
      get
      {
        if (String.IsNullOrEmpty(tableName))
          return null;

        lock (_List)
        {
          DBxTableStruct Obj = _List[tableName];
          if (Obj == null && _Owner.Source != null)
          {

            if (DataTools.IndexOf(AllTableNames, tableName, _List.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0)
            {
              Obj = _Owner.Source.GetTableStruct(tableName);
              _List.Add(Obj);
            }
          }

          return Obj;
        }
      }
    }

    /// <summary>
    /// Возвращает структуру таблицы по имени.
    /// Если нет такой таблицы, выбрасывается исключение
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Структура таблицы</returns>
    public DBxTableStruct GetRequired(string tableName)
    {
      DBxTableStruct tstr = this[tableName];
      if (tstr == null)
      {
        if (String.IsNullOrEmpty(tableName))
          throw new ArgumentNullException("tableName");
        else
          throw new ArgumentException("Нет описания структуры таблицы \"" + tableName + "\"", "tableName");
      }
      return tstr;
    }


    #endregion

    #region IList<DBxRealTableStruct> Members

    /// <summary>
    /// Реализация интерфейса IList
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int IndexOf(DBxTableStruct item)
    {
      lock (_List)
      {
        return _List.IndexOf(item);
      }
    }

    /// <summary>
    /// Реализация интерфейса IList
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, DBxTableStruct item)
    {
      _Owner.CheckNotReadOnly();
      _List.Insert(index, item);
      _AllTableNames = null;
    }

    /// <summary>
    /// Реализация интерфейса IList
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
      _Owner.CheckNotReadOnly();
      _List.RemoveAt(index);
      _AllTableNames = null;
    }

    /// <summary>
    /// Возвращает или присоединяет структуру таблицы по индексу
    /// </summary>
    /// <param name="index">Индекс таблицы в списке</param>
    /// <returns>Структура таблицы</returns>
    public DBxTableStruct this[int index]
    {
      get
      {
        lock (_List)
        {
          return _List[index];
        }
      }
      set
      {
        _Owner.CheckNotReadOnly();
        _List[index] = value;
        _AllTableNames = null;
      }
    }

    #endregion

    #region ICollection<DBxRealTableStruct> Members

    /// <summary>
    /// Добавляет описание таблицы в описание базы данных
    /// </summary>
    /// <param name="item">Описание таблицы</param>
    public void Add(DBxTableStruct item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      _Owner.CheckNotReadOnly();
      _List.Add(item);
      _AllTableNames = null;
    }

    /// <summary>
    /// Добавляет пустое описание таблицы в описание базы данных
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Описание структуры таблицы, которое следует заполнить</returns>
    public DBxTableStruct Add(string tableName)
    {
      DBxTableStruct item = new DBxTableStruct(tableName);
      Add(item);
      return item;
    }

    /// <summary>
    /// Удаляет описания всех таблиц
    /// </summary>
    public void Clear()
    {
      _Owner.CheckNotReadOnly();
      _List.Clear();
      _AllTableNames = null;
    }

    /// <summary>
    /// Реализация интерфейса ICollection
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(DBxTableStruct item)
    {
      /*
      lock (FList)
      {
        return FList.Contains(item);
      }
       * */

      // Нужна собственная реализация
      return Contains(item.TableName);
    }

    /// <summary>
    /// Возвращает true, если в списке есть описание структуры таблицы с заданным именем
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Наличие описания</returns>
    public bool Contains(string tableName)
    {
      /*
      lock (FList)
      {
        return FList.Contains(TableName);
      }
       * */

      DBxTableStruct Item = this[tableName];
      return Item != null;
    }

    /// <summary>
    /// Реализация интерфейса ICollection
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(DBxTableStruct[] array, int arrayIndex)
    {
      lock (_List)
      {
        _List.CopyTo(array, arrayIndex);
      }
    }

    /// <summary>
    /// Реализация интерфейса ICollection
    /// </summary>
    public int Count
    {
      get
      {
        lock (_List)
        {
          return _List.Count;
        }
      }
    }

    /// <summary>
    /// Реализация интерфейса ICollection
    /// </summary>
    public bool IsReadOnly
    {
      get { return _Owner.IsReadOnly; }
    }

    /// <summary>
    /// Реализация интерфейса ICollection
    /// Удаление описания таблицы.
    /// Таблица из базы данных не удаляется.
    /// </summary>
    /// <param name="item">Описание таблицы</param>
    /// <returns>true, если описание было удалено</returns>
    public bool Remove(DBxTableStruct item)
    {
      _Owner.CheckNotReadOnly();
      _AllTableNames = null;
      return _List.Remove(item);
    }

    /// <summary>
    /// Удаление описания таблицы.
    /// Таблица из базы данных не удаляется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>true, если описание было удалено</returns>
    public bool Remove(string tableName)
    {
      _Owner.CheckNotReadOnly();
      _AllTableNames = null;
      return _List.Remove(tableName);
    }

    #endregion

    #region IEnumerable<DBxRealTableStruct> Members

    #region Собственный класс перебора

    /// <summary>
    /// Создает перечислитель по таблицам
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<DBxTableStruct>
    {
      #region Конструктор

      internal Enumerator(DBxStruct owner, string[] allTableNames)
      {
        _Owner = owner;
        _AllTableNames = allTableNames;
        _CurrIndex = -1;
      }

      #endregion

      #region Поля

      private readonly DBxStruct _Owner;

      /// <summary>
      /// Используем собственную копию списка таблиц для перебора, чтобы не отслеживать возможные изменения
      /// </summary>
      private readonly string[] _AllTableNames;

      private int _CurrIndex;

      #endregion

      #region IEnumerator<DBxTableStruct> Members

      /// <summary>
      /// Текущая таблица
      /// </summary>
      public DBxTableStruct Current
      {
        get
        {
          string TableName = _AllTableNames[_CurrIndex];
          return _Owner.Tables[TableName];
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get
        {
          return Current;
        }
      }

      /// <summary>
      /// Переход к следующей таблице
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _CurrIndex++;
        return _CurrIndex < _AllTableNames.Length;
      }

      void IEnumerator.Reset()
      {
        _CurrIndex = -1;
      }

      #endregion
    }

    #endregion

    /// <summary>
    /// Возвращает перечислитель по описаниям структур DBxTableStruct для всех таблиц (свойство AllTableNames)
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Owner, AllTableNames);
    }

    IEnumerator<DBxTableStruct> IEnumerable<DBxTableStruct>.GetEnumerator()
    {
      return new Enumerator(_Owner, AllTableNames);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Owner, AllTableNames);
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает описание последней добавленной таблицы.
    /// Обычно использование свойства не имеет смысла.
    /// </summary>
    public DBxTableStruct LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

    #endregion
  }                  


#if XXX
  /// <summary>
  /// Требуемая структура базы данных, которую можно сравнить с реальной и добавить
  /// недостающие таблицы, поля, индексы
  /// </summary>
  public class DBxStruct:IReadOnlyObject
  {
  #region Перечисления

    /// <summary>
    /// Возможные типы полей
    /// </summary>
    public enum AccDepFieldType
    {
      /// <summary>
      /// Строковое поле. Длина поля задается при его объявлении
      /// </summary>
      String,

      /// <summary>
      /// 4-байтное целое со знаком
      /// </summary>
      LongInt,

      /// <summary>
      /// 2-байтное целое со знаком
      /// </summary>
      SmallInt,

      /// <summary>
      /// 1-байтное целое со знаком (пока допустимы значения от 0 до 127)
      /// </summary>
      TinyInt,

      /// <summary>
      ///  Денежный тип
      /// </summary>
      Money,

      /// <summary>
      /// 4-байтное число с плавающей точкой
      /// </summary>
      Single,

      /// <summary>
      /// 8-байтное число с плавающей точкой
      /// </summary>
      Double,

      /// <summary>
      /// Логическое поле
      /// </summary>
      Boolean,

      /// <summary>
      /// Поле для хранения даты без времени
      /// </summary>
      Date,

      /// <summary>
      /// Поле для хранения даты и времени
      /// </summary>
      Time,

      /// <summary>
      /// Ссылочное поле на идентификатор Id в другой таблице
      /// </summary>
      Reference,

      /// <summary>
      /// Длинный текст до 65536 байт
      /// </summary>
      Memo,

      /// <summary>
      /// Текстовое МЕМО-поле, содержащее данные в Xml-формате, которые читаются
      /// и записываются с помощью объектов ConfigSection
      /// </summary>
      XmlConfig,

      /// <summary>
      /// Двоичные (не текстовые) данные
      /// </summary>
      Binary,

      /// <summary>
      /// Ссылка на файл в таблице files.mdb (идентификатор)
      /// Аналогично LongInt (отличие используется при проверке документов)
      /// </summary>
      FileReference,
    }

    /// <summary>
    /// Поведение при удалении строки, на которую ссылается поле
    /// </summary>
    public enum RefType
    {
      /// <summary>
      /// Удаление запрещено. Будет выдано сообщение о невозможности удалить
      /// строку, потому что на нее ссылается другая таблица
      /// </summary>
      Disallow,

      /// <summary>
      /// При удалении строки в главной таблице будут удалены все строки
      /// в подчиненной таблице
      /// </summary>
      Delete,

      /// <summary>
      /// При удалении строки в главной таблице, в подчиненной таблице будут
      /// очищены недействительные ссылки
      /// </summary>
      Clear,

      /// <summary>
      /// Фиктивная ссылка. Создается обычное числовое поле, о котором база данных
      /// не знает, что оно ссылочное. Проверка целостности ссылки лежит на программе
      /// </summary>
      Emulation,
    }

  #endregion

  #region Конструктор

    public DBStruct()
    {
      FTables = new TableCollection(this);
      FErrorMessages = new ErrorMessageList();

      FActiveLocks = new List<DatabaseLock>();
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Список объявленных таблиц
    /// </summary>
    public TableCollection Tables { get { return FTables; } }
    private TableCollection FTables;

    /// <summary>
    /// Список сообщений об ошибках и предупреждениях, возникших при обновлении структуры данных
    /// </summary>
    public ErrorMessageList ErrorMessages { get { return FErrorMessages; } }
    private ErrorMessageList FErrorMessages;

    /// <summary>
    /// Возвращает true, если формирование структуры уже закончено
    /// </summary>
    public bool ReadOnly { get { return FReadOnly; } }
    private bool FReadOnly;

    public void SetReadOnly()
    {
      FReadOnly = true;
      for (int i = 0; i < Tables.Count; i++)
        Tables[i].SetReadOnly();
    }

    internal void CheckNotReadOnly()
    {
      if (FReadOnly)
        throw new BugException("Формирование структуры базы данных уже закончено");
    }

  #endregion

  #region Методы


    /// <summary>
    /// Получение набора пустых таблиц. Кроме вызова GetEmptyDataTable() для всех
    /// объявлений таблиц, выполняется создание ограничений DataContrains
    /// </summary>
    /// <returns></returns>
    public DataSet GetEmptyDataSet()
    {
      int i, j;
      DataSet ds = new DataSet();
      // При передаче клиенту используем компактный формат сериализации 
      ds.RemotingFormat = SerializationFormat.Binary;
      // Сначала нужно создать бланки всех таблиц, иначе нельзя будет добавить ограничения
      for (i = 0; i < Tables.Count; i++)
      {
        DataTable dt = Tables[i].GetEmptyDataTable();
        ds.Tables.Add(dt);
      }
      // Теперь перебираем все ссылочные поля для создания ограничений
      for (i = 0; i < Tables.Count; i++)
      {
        DataTable tbl = ds.Tables[i];
        for (j = 0; j < Tables[i].Fields.Count; j++)
        {
          Field FieldDef = Tables[i].Fields[j];
          if (FieldDef.FieldType == AccDepFieldType.Reference)
          {
            ForeignKeyConstraint fk = new ForeignKeyConstraint("FK_" + Tables[i].TableName + "_" + FieldDef.FieldName,
              ds.Tables[FieldDef.MasterTableName].Columns["Id"],
              tbl.Columns[FieldDef.FieldName]);
            //fk.DeleteRule=???
            tbl.Constraints.Add(fk);
          }
        }
      }
      return ds;
    }


  #endregion

  #region Блокировка таблиц в БД

    /// <summary>
    /// Список активных блокировок базы данных
    /// При работе с элементами списка должен блокироваться список
    /// </summary>
    internal List<DatabaseLock> ActiveLocks { get { return FActiveLocks; } }
    private List<DatabaseLock> FActiveLocks;


#if XXXXXXXXXXXXX
    /// <summary>
    /// Заполняется вызовом CreateTableLocks() после того, как все объяления таблиц добавлены
    /// </summary>
    private Dictionary<string, DBStruct.Table> FTableLocks;

    /// <summary>
    /// Заполнение списка объектов блокировки таблиц
    /// </summary>
    internal void CreateTableLocks()
    { 
#if DEBUG
      if (FTableLocks != null)
        throw new InvalidOperationException("Повторный вызов CreateTableLocks()");
#endif

      FTableLocks = new Dictionary<string, Table>(Tables.Count);
      for (int i = 0; i < Tables.Count; i++)
        FTableLocks.Add(Tables[i].TableName, Tables[i]);
    }

    /// <summary>
    /// Получить объект для блокировки таблицы. Возвращает null, если база данных
    /// не использует механизм блокировки таблиц
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <returns>Объект блокировки или null</returns>
    internal ServerExecLock GetTableLock(string TableName)
    {
      if (FTableLocks == null)
        return null;
      else
        return FTableLocks[TableName];
    }

#endif
  #endregion

  #region Получение отладочных таблиц описаний

    internal DataTable CreateTableTableDefs()
    {
      DataTable ResTable = CreateEmptyTableTableDefs();

      foreach (Table TableDef in Tables)
        AddTableTableDefs(ResTable, TableDef);
      return ResTable;
    }

    internal static DataTable CreateEmptyTableTableDefs()
    {
      DataTable ResTable = new DataTable();
      ResTable.Columns.Add("TableName", typeof(string));
      ResTable.Columns.Add("HasCalcFields", typeof(bool));
      return ResTable;
    }

    internal static void AddTableTableDefs(DataTable ResTable, Table TableDef)
    {
      DataRow ResRow = ResTable.NewRow();
      ResRow["TableName"] = TableDef.TableName;
      ResRow["HasCalcFields"] = TableDef.HasCalcFields;
      ResTable.Rows.Add(ResRow);
    }

    internal DataTable CreateTableFieldDefs()
    {
      DataTable ResTable = CreateEmptyTableFieldDefs();

      foreach (Table TableDef in Tables)
        AddTableFieldDefs(ResTable, TableDef);
      return ResTable;
    }

    internal static DataTable CreateEmptyTableFieldDefs()
    {
      DataTable ResTable = new DataTable();
      ResTable.Columns.Add("TableName", typeof(string));
      ResTable.Columns.Add("FieldName", typeof(string));
      ResTable.Columns.Add("FieldType", typeof(string));
      ResTable.Columns.Add("CanBeEmpty", typeof(bool));
      ResTable.Columns.Add("Length", typeof(int));
      ResTable.Columns.Add("Calculated", typeof(bool));
      ResTable.Columns.Add("MasterTableName", typeof(string));
      ResTable.Columns.Add("RefType", typeof(string));
      return ResTable;
    }

    internal static void AddTableFieldDefs(DataTable ResTable, Table TableDef)
    {
      foreach (Field FieldDef in TableDef.Fields)
      {
        DataRow ResRow = ResTable.NewRow();
        ResRow["TableName"] = TableDef.TableName;
        ResRow["FieldName"] = FieldDef.FieldName;
        ResRow["FieldType"] = FieldDef.FieldType.ToString();
        ResRow["CanBeEmpty"] = FieldDef.CanBeEmpty;
        if (FieldDef.Length != 0)
          ResRow["Length"] = FieldDef.Length;
        ResRow["Calculated"] = FieldDef.Calculated;
        if (FieldDef.FieldType == AccDepFieldType.Reference)
        {
          ResRow["MasterTableName"] = FieldDef.MasterTableName;
          ResRow["RefType"] = FieldDef.RefType.ToString();
        }
        ResTable.Rows.Add(ResRow);
      }
    }

    internal DataTable CreateTableVTReferenceDefs()
    {
      DataTable ResTable = CreateEmptyTableVTReferenceDefs();

      foreach (Table TableDef in Tables)
        AddTableVTReferenceDefs(ResTable, TableDef);
      return ResTable;
    }

    internal static DataTable CreateEmptyTableVTReferenceDefs()
    {
      DataTable ResTable = new DataTable();
      ResTable.Columns.Add("TableName", typeof(string));
      ResTable.Columns.Add("VTRName", typeof(string));
      ResTable.Columns.Add("TableFieldName", typeof(string));
      ResTable.Columns.Add("IdFieldName", typeof(string));
      return ResTable;
    }

    internal static void AddTableVTReferenceDefs(DataTable ResTable, Table TableDef)
    {
      if (TableDef.VTReferencesCount > 0)
      {
        foreach (VTReference vtr in TableDef.VTReferences)
        {
          DataRow ResRow = ResTable.NewRow();
          ResRow["TableName"] = TableDef.TableName;
          ResRow["VTRName"] = vtr.Name;
          ResRow["TableFieldName"] = vtr.TableField.FieldName;
          ResRow["IdFieldName"] = vtr.IdField.FieldName;
          ResTable.Rows.Add(ResRow);
        }
      }
    }

    internal DataTable CreateTableVTReferenceTableDefs()
    {
      DataTable ResTable = CreateEmptyTableVTReferenceTableDefs();

      foreach (Table TableDef in Tables)
        AddTableVTReferenceTableDefs(ResTable, TableDef);
      return ResTable;
    }

    internal static DataTable CreateEmptyTableVTReferenceTableDefs()
    {
      DataTable ResTable = new DataTable();
      ResTable.Columns.Add("TableName", typeof(string));
      ResTable.Columns.Add("VTRName", typeof(string));
      ResTable.Columns.Add("MasterTableName", typeof(string));
      ResTable.Columns.Add("MasterTableId", typeof(Int32));
      return ResTable;
    }

    internal static void AddTableVTReferenceTableDefs(DataTable ResTable, Table TableDef)
    {
      if (TableDef.VTReferencesCount > 0)
      {
        foreach (VTReference vtr in TableDef.VTReferences)
        {
          for (int i = 0; i < vtr.MasterTableNames.Length; i++)
          {
            DataRow ResRow = ResTable.NewRow();
            ResRow["TableName"] = TableDef.TableName;
            ResRow["VTRName"] = vtr.Name;
            ResRow["MasterTableName"] = vtr.MasterTableNames[i];
            ResRow["MasterTableId"] = vtr.MasterTableIds[i];
            ResTable.Rows.Add(ResRow);
          }
        }
      }
    }

    internal DataTable CreateTableIndexDefs()
    {
      DataTable ResTable = CreateEmptyTableIndexDefs();

      foreach (Table TableDef in Tables)
        AddTableIndexDefs(ResTable, TableDef);
      return ResTable;
    }

    internal static DataTable CreateEmptyTableIndexDefs()
    {
      DataTable ResTable = new DataTable();
      ResTable.Columns.Add("TableName", typeof(string));
      ResTable.Columns.Add("FieldNames", typeof(string));
      return ResTable;
    }

    internal static void AddTableIndexDefs(DataTable ResTable, Table TableDef)
    {
      foreach (Index IndexDef in TableDef.Indices)
      {
        DataRow ResRow = ResTable.NewRow();
        ResRow["TableName"] = TableDef.TableName;
        ResRow["FieldNames"] = IndexDef.FieldNames;
        ResTable.Rows.Add(ResRow);
      }
    }

  #endregion

    public class Table : ServerExecLock
    {
  #region Конструктор

      public Table(string TableName)
        : base("Структура таблицы \"" + TableName + "\"")
      {
        FTableName = TableName;
        FFields = new FieldCollection(this);

        AddRecordLocker = new ServerExecLock("Добавление строк в таблицу \""+TableName+"\"");
      }

  #endregion

  #region Свойства

      /// <summary>
      /// Имя таблицы в базе данных
      /// </summary>
      public string TableName { get { return FTableName; } }
      private String FTableName;

      /// <summary>
      /// Список объявленных полей для таблицы
      /// </summary>
      public FieldCollection Fields { get { return FFields; } }
      private FieldCollection FFields;

      /// <summary>
      /// true, если существуют вычисляемые поля
      /// </summary>
      public bool HasCalcFields
      {
        get
        {
          for (int i = 0; i < Fields.Count; i++)
          {
            if (Fields[i].Calculated)
              return true;
          }
          return false;
        }
      }

      /// <summary>
      /// Возвращает список имен вычисляемых полей или null, если нет ни одного поля
      /// </summary>
      public DataFields CalcFields
      {
        get
        {
          List<string> lst = null;
          for (int i = 0; i < Fields.Count; i++)
          {
            if (Fields[i].Calculated)
            {
              if (lst == null)
                lst = new List<string>();
              lst.Add(Fields[i].FieldName);
            }
          }
          if (lst == null)
            return null;
          else
            return new DataFields(lst.ToArray());
        }
      }

      /// <summary>
      /// Возвращает список имен всех полей, кроме вычисляемых или null, если нет ни одного поля
      /// </summary>
      public DataFields NonCalcFields
      {
        get
        {
          List<string> lst = null;
          for (int i = 0; i < Fields.Count; i++)
          {
            if (!Fields[i].Calculated)
            {
              if (lst == null)
                lst = new List<string>();
              lst.Add(Fields[i].FieldName);
            }
          }
          if (lst == null)
            return null;
          else
            return new DataFields(lst.ToArray());
        }
      }

      /// <summary>
      /// Объявления ссылок на переменные таблицы.
      /// В списке Fields также присутствуют поля, которые реализуют ссылки
      /// </summary>
      public VTReferenceCollection VTReferences
      {
        get
        {
          if (FVTReferences == null)
            FVTReferences = new VTReferenceCollection(this);
          return FVTReferences;
        }
      }
      private VTReferenceCollection FVTReferences;

      public int VTReferencesCount
      {
        get
        {
          if (FVTReferences == null)
            return 0;
          else
            return FVTReferences.Count;
        }
      }

      /// <summary>
      /// Список объявлений индексов таблицы
      /// Индекс первичного ключа по полю Id не добавляется
      /// Для поддокументов индекс по DocId+порядок сортировки будет добавлен автоматически
      /// </summary>
      public IndexCollection Indices
      {
        get
        {
          if (FIndices == null)
            FIndices = new IndexCollection(this);
          return FIndices;
        }
      }
      private IndexCollection FIndices;


      /// <summary>
      /// Возвращает true, если формирование структуры уже закончено
      /// </summary>
      public bool ReadOnly { get { return FReadOnly; } }
      private bool FReadOnly;

      public void SetReadOnly()
      {
        FReadOnly = true;
      }

      internal void CheckNotReadOnly()
      {
        if (FReadOnly)
          throw new BugException("Формирование структуры базы данных уже закончено");
      }

  #endregion

  #region Методы

#if XXXXX
      /// <summary>
      /// Скопировать объявление таблицы вместе со всеми полями
      /// </summary>
      /// <returns></returns>
      public Table Clone()
      {
        Table res=new Table(FTableName);
        foreach(Field f in Fields)
        {
          Field NewFld = f.Clone();
          res.Fields.AddField(NewFld);
          f.FTable=res;
        }
        if (FVTReferences != null)
        {
          foreach (VTReference vtr in FVTReferences)
          { 
            VTReference NewVTR=vtr.c
          }
        }
        return res;
      }
#endif

      /// <summary>
      /// Получение пустой таблицы DataTable нужной структурой
      /// </summary>
      /// <returns></returns>
      public DataTable GetEmptyDataTable()
      {
        DataTable Table = new DataTable(TableName);
        // Поля "Id" нет в описании таблицы
        Table.Columns.Add("Id", typeof(int));
        Table.Columns[0].AutoIncrement = true;
        if (HasDocIdField)
          Table.Columns.Add("DocId", typeof(int));


        foreach (DBStruct.Field fld in Fields)
        {
          DataColumn Column = Table.Columns.Add(fld.FieldName, fld.SystemType);
          if (Column.DataType == typeof(DateTime))
            Column.DateTimeMode = DataSetDateTime.Unspecified;
        }

        InitDataRowLimits(Table);

        // Устанавливаем первичный ключ
        DataTools.SetPrimaryKey(Table, "Id");

        return Table;
      }

      /// <summary>
      /// True, если таблица должна содержать поле DocId
      /// (используется в GetEmptyDataTable)
      /// </summary>
      public bool HasDocIdField = false;

      public void InitDataRowLimits(DataTable Table)
      {
        foreach (DataColumn Column in Table.Columns)
        {
          int p = Fields.IndexOf(Column.ColumnName);
          if (p < 0)
            continue; // служебное поле
          DBStruct.Field FieldDef = Fields[p];
          if (FieldDef.FieldType == DBStruct.AccDepFieldType.String && Column.DataType == typeof(string))
            Column.MaxLength = FieldDef.Length;
          /* Нельзя !
          if (FieldDef.Type==DatabaseStruct.FieldType.String ||
              FieldDef.Type==DatabaseStruct.FieldType.Date ||
              FieldDef.Type==DatabaseStruct.FieldType.Time)
            Column.AllowDBNull=FieldDef.CanBeEmpty;
          */
        }
      }

      /// <summary>
      /// Получение набора данных, содержащего одну пустую таблицу нужной структуры
      /// </summary>
      /// <returns>Набор DataSet с одной таблицей DataTable</returns>
      public DataSet GetEmptyDatSet()
      {
        DataTable Table = GetEmptyDataTable();
        DataSet ds = new DataSet();
        ds.Tables.Add(Table);
        return ds;
      }

      public override string ToString()
      {
        return TableName;
      }

  #endregion

  #region Вспомогательные объекты

      internal readonly ServerExecLock AddRecordLocker;

  #endregion
    }

    public class TableCollection : IEnumerable<Table>
    {
  #region Защищенный конструктор

      internal TableCollection(DBStruct Owner)
      {
        FOwner = Owner;
        FItems = new List<Table>();
      }

  #endregion

  #region Свойства

      /// <summary>
      /// Объект-владелец
      /// </summary>
      public DBStruct Owner { get { return FOwner; } }
      private DBStruct FOwner;

      public Table this[string TableName]
      {
        get
        {
          int p = IndexOf(TableName);
          if (p < 0)
            throw new Exception("Таблица не обнаружена в описании структуры БД: " + TableName);
          return FItems[p];
        }
      }

      public Table this[int Index]
      {
        get
        {
          return FItems[Index];
        }
      }

      public int Count
      {
        get
        {
          return FItems.Count;
        }
      }

      /// <summary>
      /// Список имен всех таблиц в виде массива
      /// </summary>
      public string[] TableNames
      {
        get
        {
          string[] a = new string[FItems.Count];
          for (int i = 0; i < FItems.Count; i++)
            a[i] = FItems[i].TableName;
          return a;
        }
      }

  #endregion

  #region Методы

      public Table Add(string TableName)
      {
#if DEBUG
        Owner.CheckNotReadOnly();
#endif
        Table t = new Table(TableName);
        FItems.Add(t);
        return t;
      }

      public void Add(Table t)
      {
#if DEBUG
        Owner.CheckNotReadOnly();
#endif
        FItems.Add(t);
      }

      public bool Contains(string TableName)
      {
        return IndexOf(TableName) >= 0;
      }

      public int IndexOf(string TableName)
      {
        for (int i = 0; i < FItems.Count; i++)
        {
          if ((FItems[i]).TableName == TableName)
            return i;
        }
        return -1;
      }

  #endregion

  #region Реализация IEnumerable

      IEnumerator<Table> IEnumerable<Table>.GetEnumerator()
      {
        return FItems.GetEnumerator();
      }

      public IEnumerator GetEnumerator()
      {
        return FItems.GetEnumerator();
      }

  #endregion

  #region Внутренняя реализация

      private List<Table> FItems;

  #endregion
    }


    public class Field
    {
  #region Защищенный конструктор

      internal Field(string FieldName, AccDepFieldType FieldType)
      {
        FFieldName = FieldName;
        FFieldType = FieldType;
        FLength = 0;
        FMasterTableName = null;
        FRefType = RefType.Disallow;
        Calculated = false;
      }


  #endregion

  #region Свойства

      /// <summary>
      /// Имя поля
      /// </summary>
      public string FieldName { get { return FFieldName; } }
      internal string FFieldName;

      /// <summary>
      /// Тип поля
      /// </summary>
      public AccDepFieldType FieldType { get { return FFieldType; } }
      internal AccDepFieldType FFieldType;

      /// <summary>
      /// Длина поля для Type=String
      /// </summary>
      public int Length { get { return FLength; } }
      internal int FLength;

      /// <summary>
      /// Может ли быть поле пустым (для Type=String, Date, DateTime и Reference)
      /// </summary>
      public bool CanBeEmpty { get { return FCanBeEmpty; } }
      internal bool FCanBeEmpty;

      /// <summary>
      /// Имя таблицы, на которое ссылается поле (т.е. содержащей идентифкатор Id)
      /// (для Type=Reference)
      /// </summary>
      public string MasterTableName { get { return FMasterTableName; } }
      internal string FMasterTableName;

      /// <summary>
      /// Способ удаления строки таблицы MasterTableName при наличии ссылки на нее
      /// </summary>
      public RefType RefType { get { return FRefType; } }
      internal RefType FRefType;

      /// <summary>
      /// Объект таблицы, к которому относится объявление поля
      /// </summary>
      public Table Table { get { return FTable; } }
      internal Table FTable; // Таблица, к которой присоединено поле


      /// <summary>
      /// Получение системного типа .NET Framework для поля данного типа
      /// </summary>
      public Type SystemType
      {
        get
        {
          switch (FFieldType)
          {
            case AccDepFieldType.String: return typeof(String);
            case AccDepFieldType.Date: return typeof(DateTime);
            case AccDepFieldType.Time: return typeof(DateTime);
            case AccDepFieldType.Boolean: return typeof(Boolean);
            case AccDepFieldType.LongInt: return typeof(Int32);
            case AccDepFieldType.SmallInt: return typeof(Int16);
            case AccDepFieldType.TinyInt: return typeof(Byte);
            case AccDepFieldType.Money: return typeof(Decimal);
            case AccDepFieldType.Single: return typeof(Single);
            case AccDepFieldType.Double: return typeof(Double);
            case AccDepFieldType.Reference: return typeof(Int32);
            case AccDepFieldType.Memo: return typeof(String);
            case AccDepFieldType.XmlConfig: return typeof(String);
            case AccDepFieldType.Binary: return typeof(byte[]);
            case AccDepFieldType.FileReference: return typeof(Int32);
            default: throw new BugException("Неизвестный тип " + FieldType.ToString());
          }
        }
      }

      /// <summary>
      /// true, если данное поле является вычисляемым
      /// </summary>
      public bool Calculated
      {
        get { return FCalculated; }
        set
        {
#if DEBUG
          if (value)
          {
            switch (FieldType)
            {
              case AccDepFieldType.String:
              case AccDepFieldType.Reference:
              case AccDepFieldType.Date:
              case AccDepFieldType.Time:
                if (!CanBeEmpty)
                  throw new InvalidExpressionException("Нельзя сделать поле \"" + FieldName +
                    "\" таблицы \"" + Table.TableName + "\" вычисляемом, т.к. свойство CanBeEmpty не установлено");
                break;
            }
          }
#endif
          FCalculated = value;
        }
      }
      private bool FCalculated;

  #endregion

  #region Методы

      /// <summary>
      /// Скопировать объявление поля
      /// </summary>
      /// <returns>Новый объект Field</returns>
      public Field Clone()
      {
        Field res = new Field(FieldName, FieldType);
        res.FLength = Length;
        res.FCanBeEmpty = CanBeEmpty;
        res.FMasterTableName = MasterTableName;
        res.FRefType = RefType;
        return res;
      }

      public override string ToString()
      {
        return FieldName + " (" + FieldType.ToString() + ")";
      }

  #endregion
    }

    public class FieldCollection : IEnumerable<Field>
    {
  #region Защищенный конструктор

      internal FieldCollection(Table ATable)
      {
        FTable = ATable;
        FItems = new List<Field>();
      }

  #endregion

  #region Свойства

      public Field this[string FieldName]
      {
        get
        {
          int p = IndexOf(FieldName);
          if (p < 0)
            throw new Exception("Поле в коллекции не найдено: " + FieldName);
          return FItems[p];
        }
      }
      public Field this[int Index]
      {
        get
        {
          return FItems[Index];
        }
      }

      public int Count
      {
        get
        {
          return FItems.Count;
        }
      }

      private Table Table { get { return FTable; } }
      private Table FTable;

      /// <summary>
      /// Получение массива имен полей
      /// </summary>
      /// <returns>Массив строк</returns>
      public string[] FieldNames
      {
        get
        {
          string[] a = new string[FItems.Count];
          for (int i = 0; i < FItems.Count; i++)
            a[i] = FItems[i].FieldName;
          return a;
        }
      }

  #endregion

  #region Методы

      public bool Contains(string FieldName)
      {
        return IndexOf(FieldName) >= 0;
      }

      public int IndexOf(string FieldName)
      {
        for (int i = 0; i < FItems.Count; i++)
        {
          if (((Field)FItems[i]).FieldName == FieldName)
            return i;
        }
        return -1;
      }

      /// <summary>
      /// Получить список имен общих полей, которые присутсвтвуют и в этом списке,
      /// и в другом списке.
      /// Тип полей не учитывается.
      /// Если нет ни одного общего поля, то возвращается null
      /// </summary>
      /// <param name="OtherFields">Список определений полей в другой таблице</param>
      /// <returns>Набор общих полей</returns>
      public DataFields GetCommonFields(FieldCollection OtherFields)
      {
        List<string> lst = new List<string>();
        for (int i = 0; i < Count; i++)
        {
          if (OtherFields.Contains(this[i].FieldName))
            lst.Add(this[i].FieldName);
        }
        if (lst.Count == 0)
          return null;
        else
          return new DataFields(lst);
      }

  #endregion

  #region Методы добавления полей

#if DEBUG
      private void CheckNewFieldName(string FieldName)
      {
        if (String.IsNullOrEmpty(FieldName))
          throw new ArgumentNullException("FieldName");
        if (FieldName.IndexOfAny(new char[] { ' ', '.', ',', ':', ';' }) >= 0)
          throw new ArgumentException("Имя поля \"" + FieldName + "\" содерджит недопустимые символы");
        if (Contains(FieldName))
          throw new ArgumentException("Повторное добавление поля с именем \"" + FieldName + "\" в таблицу \"" + FTable.TableName + "\"");
      }
#endif

      /// <summary>
      /// Добавить строковое поле
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <param name="Length">Максимальная длина строки (от 1 до 255 символов)</param>
      /// <param name="CanBeEmpty">True, если поле может содержать пустую строку, False-если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddString(string FieldName, int Length, bool CanBeEmpty)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
        if (Length < 1 || Length > 255)
          throw new ArgumentOutOfRangeException("Length", Length, "Длина строкового поля может быть от 1 до 255 символов");
#endif
        Field f = new Field(FieldName, AccDepFieldType.String);
        f.FTable = FTable;
        f.FLength = Length;
        f.FCanBeEmpty = CanBeEmpty;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле типа "Дата"
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <param name="CanBeEmpty">True, если поле может содержать пустую дату, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddDate(string FieldName, bool CanBeEmpty)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Date);
        f.FTable = FTable;
        f.FCanBeEmpty = CanBeEmpty;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле типа "Дата и время"
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <param name="CanBeEmpty">True, если поле может содержать пустую дату, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddDateTime(string FieldName, bool CanBeEmpty)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Time);
        f.FTable = FTable;
        f.FCanBeEmpty = CanBeEmpty;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле, содержащее 4-байтное целое со знаком
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddLongInt(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.LongInt);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле, содержащее 2-байтное целое со знаком
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddSmallInt(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.SmallInt);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле, содержащее 1-байтное целое со знаком (пока допускаются значения от 0 до 127)
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddTinyInt(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.TinyInt);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле, содержащее денежную сумму
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddMoney(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Money);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле, содержащее 4-байтное число с плавающей точкой
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddSingle(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Single);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить поле, содержащее 8-байтное число с плавающей точкой
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddDouble(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Double);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавить логическое поле, содержащее значения True или False
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddBoolean(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Boolean);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Объявление ссылочного поля
      /// </summary>
      /// <param name="FieldName">Имя ссылочного поля</param>
      /// <param name="MasterTableName">Имя таблицы, на которую выполняется ссылка</param>
      /// <param name="CanBeEmpty">Является ли ссылка необязательной</param>
      /// <param name="RefType">Действия при удалении строки, на которую есть ссылка</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddReference(string FieldName, string MasterTableName, bool CanBeEmpty, RefType RefType)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
        if (String.IsNullOrEmpty(MasterTableName))
          throw new ArgumentNullException("MasterTableName");
#endif
        if (RefType == RefType.Clear && (!CanBeEmpty))
          throw new ArgumentException("Тип ссылки не может быть Clear, если CanBeEmpty=false", "RefType");

        Field f = new Field(FieldName, AccDepFieldType.Reference);
        f.FTable = FTable;
        f.FMasterTableName = MasterTableName;
        f.FCanBeEmpty = CanBeEmpty;
        f.FRefType = RefType;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавление ссылочного поля  (RefType=Disallow)
      /// </summary>
      /// <param name="FieldName">Имя ссылочного поля</param>
      /// <param name="MasterTableName">Имя таблицы, на которую выполняется ссылка</param>
      /// <param name="CanBeEmpty">Является ли ссылка необязательной</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddReference(string FieldName, string MasterTableName, bool CanBeEmpty)
      {
        return AddReference(FieldName, MasterTableName, CanBeEmpty, RefType.Disallow);
      }

      /// <summary>
      /// Добавление обязательного ссылочного поля  (CanBeEmpty=false, RefType=Disallow)
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <param name="MasterTableName">Таблица, на которую ссылается поле</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddReference(string FieldName, string MasterTableName)
      {
        return AddReference(FieldName, MasterTableName, false, RefType.Disallow);
      }

      /// <summary>
      /// Добавление поля, задающего идентификатор таблицы документа
      /// </summary>
      /// <param name="FieldName">Имя ссылочного поля</param>
      /// <param name="CaneEmpty">Может ли поле принимать значение null</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddDocTableReference(string FieldName, bool CaneEmpty)
      {
        return AddReference(FieldName, "DocTables", CaneEmpty, RefType.Disallow);
      }

      /// <summary>
      /// Добавление МЕМО-поля
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddMemo(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Memo);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавление поля для хранения ConfigSection
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddXmlConfig(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.XmlConfig);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавление поля с двоичными данными
      /// </summary>
      /// <param name="FieldName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public Field AddBinary(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.Binary);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      /// <summary>
      /// Добавление поля, содержащего ссылку на файл
      /// </summary>
      /// <param name="FieldName"></param>
      /// <returns></returns>
      public Field AddFileReference(string FieldName)
      {
#if DEBUG
        FTable.CheckNotReadOnly();
        CheckNewFieldName(FieldName);
#endif
        Field f = new Field(FieldName, AccDepFieldType.FileReference);
        f.FTable = FTable;
        FItems.Add(f);
        return f;
      }

      public void AddPhoto()
      {
        AddFileReference("Фото");
        AddFileReference("ФотоМиниатюра");
      }

  #endregion

  #region Реализация IEnumerable

      IEnumerator<Field> IEnumerable<Field>.GetEnumerator()
      {
        return FItems.GetEnumerator();
      }

      public IEnumerator GetEnumerator()
      {
        return FItems.GetEnumerator();
      }

  #endregion

  #region Внутренняя реализация

      private List<Field> FItems;

      internal void AddField(Field fld)
      {
        if (fld.FTable != null)
          throw new Exception("Нельзя повторно присоединить поле к таблице");
        fld.FTable = FTable;
        FItems.Add(fld);
      }

  #endregion
    }

    /// <summary>
    /// Ссылка на одну из нескольких таблиц.
    /// Реализуется с помощью двух числовых полей типа Int32. Первое поле имеет имя
    /// "ХххТаблица", где "Ххх" - имя ссылки. В нем хранится идентификатор
    /// таблицы документа или поддокумента TableId (из таблицы DocTables).
    /// Второе поле имеет имя "ХххИдентификатор", в нем храниться идентификатор Id
    /// в таблице, на которую выполняется ссылка.
    /// Ссылка считается пустой, если оба поля имеют значение Null. В противном 
    /// случае ссылка должна быть корректной. Допустимость пустой ссылки определяется
    /// свойством Field.CanBeEmpty.
    /// Описатель ссылки имеет имя ("Ххх"), указатель на оба поля и список имен таблиц,
    /// на которые могут выполняться ссылки. Ссылка недействительна, если ссылается на
    /// таблицу, которой нет в списке
    /// </summary>
    public class VTReference
    {
  #region Защищенный конструктор

      internal VTReference(string Name, Field TableField, Field IdField)
      {
        FName = Name;
        FTableField = TableField;
        FIdField = IdField;
        MasterTableNamesArray = new List<string>();
      }

  #endregion

  #region Свойства

      /// <summary>
      /// Имя ссылки
      /// </summary>
      public string Name { get { return FName; } }
      private string FName;

      /// <summary>
      /// Числовое поле, содержащее идентификатор таблицы
      /// </summary>
      public Field TableField { get { return FTableField; } }
      private Field FTableField;

      /// <summary>
      /// Числовое поле, содержащее идентификатор строки в таблице, на которую
      /// выполняется ссылка
      /// </summary>
      public Field IdField { get { return FIdField; } }
      private Field FIdField;

      /// <summary>
      /// Имена таблиц, на которые возможна ссылка. Свойство становиться доступно
      /// после инициализации БД
      /// </summary>
      public string[] MasterTableNames { get { return FMasterTableNames; } }
      private string[] FMasterTableNames;

      /// <summary>
      /// Идентификаторы таблиц, на которые возможна ссылка, то есть список возможных
      /// значений для поля TableField. Свойство становиться доступно
      /// после инициализации БД
      /// </summary>
      public Int32[] MasterTableIds { get { return FMasterTableIds; } }
      private Int32[] FMasterTableIds;

      /// <summary>
      /// Добавление таблицы, на которую возможна ссылка. Метод доступен, пока не
      /// выполнена инициализация БД.
      /// Возможен повторный вызов метода для той же мастер-таблицы, таблица 
      /// добавляется однократно.
      /// </summary>
      /// <param name="MasterTableName">Имя мастер-таблицы</param>
      public void AddMasterTableName(string MasterTableName)
      {
#if DEBUG
        FTableField.Table.CheckNotReadOnly();
        if (MasterTableNamesArray == null)
          throw new InvalidOperationException("Нельзя добавлять Master-таблицу после инициализации БД");
        if (string.IsNullOrEmpty(MasterTableName))
          throw new ArgumentNullException("MasterTableName");
#endif
        if (!MasterTableNamesArray.Contains(MasterTableName))
          MasterTableNamesArray.Add(MasterTableName);
      }

      /// <summary>
      /// Внутренний список, куда добавляются имена таблиц до инициализации
      /// </summary>
      private List<string> MasterTableNamesArray;

  #endregion

  #region Методы инициализации

      /// <summary>
      /// Преобразование списка в массив
      /// </summary>
      internal void InternalInitPhase1()
      {
        FMasterTableNames = MasterTableNamesArray.ToArray();
        MasterTableNamesArray = null;
      }

      /// <summary>
      /// Присоединение массива идентификаторов таблиц
      /// </summary>
      /// <param name="MasterTableIds"></param>
      internal void InternalInitPhase2(int[] MasterTableIds)
      {
#if DEBUG
        if (MasterTableIds.Length != MasterTableNames.Length)
          throw new ArgumentException("Неправильная длина массива", "MasterTableIds");
#endif
        FMasterTableIds = MasterTableIds;
      }

  #endregion
    }

    /// <summary>
    /// Реализация свойства Table.VTReferenceCollection
    /// </summary>
    public class VTReferenceCollection : IEnumerable<VTReference>
    {
  #region Защищенный конструктор

      internal VTReferenceCollection(Table Table)
      {
        FTable = Table;
      }

  #endregion

  #region Свойства

      public Table Table { get { return FTable; } }
      private Table FTable;

      public int Count
      {
        get
        {
          if (FItems == null)
            return 0;
          else
            return FItems.Count;
        }
      }

      public VTReference this[int Index]
      {
        get { return FItems[Index]; }
      }

      public VTReference this[string Name]
      {
        get
        {
          int p = IndexOf(Name);
          if (p < 0)
            return null;
          else
            return FItems[p];
        }
      }

      private List<VTReference> FItems;

  #endregion

  #region Методы

      /// <summary>
      /// Добавить ссылку. Создается два числовых поля к описанию полей
      /// </summary>
      /// <param name="NamePart">Имя ссылки</param>
      /// <returns>Созданный объект ссылки</returns>
      public VTReference Add(string Name)
      {
#if DEBUG
        Table.CheckNotReadOnly();
        if (String.IsNullOrEmpty(Name))
          throw new ArgumentNullException("Name");
#endif
        Field TableField = Table.Fields.AddLongInt(Name + "Таблица");
        Field IdField = Table.Fields.AddLongInt(Name + "Идентификатор");
        VTReference Item = new VTReference(Name, TableField, IdField);
        if (FItems == null)
          FItems = new List<VTReference>();
        FItems.Add(Item);
        return Item;
      }

      public int IndexOf(string Name)
      {
        if (String.IsNullOrEmpty(Name))
          return -1;
        if (FItems == null)
          return -1;
        for (int i = 0; i < FItems.Count; i++)
        {
          if (FItems[i].Name == Name)
            return i;
        }
        return -1;
      }

  #endregion

  #region IEnumerable Members

      public IEnumerator GetEnumerator()
      {
        if (FItems == null)
          FItems = new List<VTReference>();
        return FItems.GetEnumerator();
      }

      IEnumerator<VTReference> IEnumerable<VTReference>.GetEnumerator()
      {
        if (FItems == null)
          FItems = new List<VTReference>();
        return FItems.GetEnumerator();
      }

  #endregion
    }


    /// <summary>
    /// Объявление индекса в таблице
    /// </summary>
    public class Index
    {
  #region Защищенный конструктор

      internal Index(string FieldNames)
      {
        FFieldNames = FieldNames;
      }


  #endregion

  #region Свойства

      /// <summary>
      /// Имена полей, разделенных запятыми
      /// </summary>
      public string FieldNames { get { return FFieldNames; } }
      internal string FFieldNames;

      /// <summary>
      /// Объект таблицы, к которой относится объявление индекса
      /// </summary>
      public Table Table { get { return FTable; } }
      internal Table FTable;


  #endregion

  #region Методы


#if XXXXXX
      /// <summary>
      /// Скопировать объявление индекса
      /// </summary>
      /// <returns>Новый объект Field</returns>
      public Index Clone()
      {
        Index res = new Index(FieldNames);
        return res;
      }
#endif

      public override string ToString()
      {
        return FieldNames;
      }

  #endregion
    }
    /// <summary>
    /// Реализация свойства TableDef.Indices
    /// </summary>
    public class IndexCollection : IEnumerable<Index>
    {
  #region Защищенный конструктор

      internal IndexCollection(Table Table)
      {
        FTable = Table;
        FItems = new List<Index>();
      }

  #endregion

  #region Свойства

      public Index this[string FieldNames]
      {
        get
        {
          int p = IndexOf(FieldNames);
          if (p < 0)
            throw new Exception("Индекс с полями \"" + FieldNames + "\" не найден в таблице " + Table.ToString());
          return FItems[p];
        }
      }
      public Index this[int Index]
      {
        get
        {
          return FItems[Index];
        }
      }

      public int Count
      {
        get
        {
          return FItems.Count;
        }
      }

      private Table Table { get { return FTable; } }
      private Table FTable;

  #endregion

  #region Методы

      public bool Contains(string FieldNames)
      {
        return IndexOf(FieldNames) >= 0;
      }

      public int IndexOf(string FieldNames)
      {
        for (int i = 0; i < FItems.Count; i++)
        {
          if ((FItems[i]).FieldNames == FieldNames)
            return i;
        }
        return -1;
      }

  #endregion

  #region Методы добавления индексов

      /// <summary>
      /// Добавить описание индекса.
      /// На момент вызова уже должны быть объявлены все поля, входящие в индекс
      /// Если такой индекс уже был добавлен, возвращается существующий объект.
      /// Возвращаемый объект индекса может перестать быть действительным после
      /// добавления другого индекса, т.к. он может заменить его.
      /// </summary>
      /// <param name="SearchFieldNames">Имена полей в индексе, разделенные запятыми</param>
      /// <returns>Созданный объект объявления индекса</returns>
      public Index Add(string FieldNames)
      {
#if DEBUG
        Table.CheckNotReadOnly();
        if (string.IsNullOrEmpty(FieldNames))
          throw new ArgumentNullException("FieldNames");
        string[] a = FieldNames.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] == "Id")
          {
            if (i == 0)
              throw new InvalidOperationException("Индекс по полю Id объявлять нельзя");
            else
              continue;
          }
          if (Table.Fields.IndexOf(a[i]) < 0)
            throw new ArgumentException("Неправильное объявление индекса \"" + FieldNames +
              "\" для таблицы \"" + Table.TableName + "\". В таблице нет поля \"" + a[i] + "\"");
        }
#endif
        // Проверяем, не был ли уже добавлен индекс с теми же самыми полями.
        // Также проверяем, нет ли более сложного индекса с такими же начальными полями
        for (int i = FItems.Count - 1; i >= 0; i--)
        {
          if (FItems[i].FieldNames == FieldNames)
            return FItems[i];
          if (FItems[i].FieldNames.StartsWith(FieldNames + ","))
            return FItems[i];

          // Если есть простой индекс, который совпадает с началом добавляемого индекса,
          // то убираем его
          if (FieldNames.StartsWith(FItems[i].FieldNames + ","))
            FItems.RemoveAt(i);
        }

        // Создаем объект индекса
        Index Obj = new Index(FieldNames);

        Obj.FTable = FTable;
        FItems.Add(Obj);
        return Obj;
      }

  #endregion

  #region Реализация IEnumerable

      public IEnumerator GetEnumerator()
      {
        return FItems.GetEnumerator();
      }

      IEnumerator<Index> IEnumerable<Index>.GetEnumerator()
      {
        return FItems.GetEnumerator();
      }

  #endregion

  #region Внутренняя реализация

      private List<Index> FItems;

  #endregion
    }
  }
#endif
}
