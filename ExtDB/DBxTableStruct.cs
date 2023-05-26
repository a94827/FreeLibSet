// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Описание реальной структуры одной таблицы данных
  /// </summary>
  [Serializable]
  public class DBxTableStruct : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создает новое пустое описание таблицы с заданным именем
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public DBxTableStruct(string tableName)
      : base(tableName)
    {
      _Columns = new DBxColumnStructList();
      _Indexes = new DBxIndexStructList();
      _AutoPrimaryKey = true;
      _AutoCreate = true;
    }

    /// <summary>
    /// Конструктор для клонирования
    /// </summary>
    private DBxTableStruct(DBxTableStruct source, string tableName)
      : base(tableName)
    {
      _Columns = new DBxColumnStructList(source.Columns.Count);
      for (int i = 0; i < source.Columns.Count; i++)
        _Columns.Add(source.Columns[i].Clone());
      _Indexes = new DBxIndexStructList(source.Indexes.Count);
      for (int i = 0; i < source.Indexes.Count; i++)
        _Indexes.Add(source.Indexes[i].Clone());
      _PrimaryKey = source._PrimaryKey; // копируется, если задано в явном виде
      _AutoPrimaryKey = source._AutoPrimaryKey;
      _Comment = source._Comment;
      _AutoCreate = source._AutoCreate; // 21.07.2021
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя таблицы
    /// </summary>
    public string TableName { get { return base.Code; } }

    /// <summary>
    /// Список описаний столбцов
    /// </summary>
    public DBxColumnStructList Columns { get { return _Columns; } }
    private readonly DBxColumnStructList _Columns;

    /// <summary>
    /// Имена всех обхявленных столбцов
    /// </summary>
    public string[] AllColumnNames
    {
      get
      {
        string[] a = new string[Columns.Count];
        for (int i = 0; i < Columns.Count; i++)
          a[i] = Columns[i].ColumnName;
        return a;
      }
    }

    /// <summary>
    /// Имена полей, образующих первичный ключ.
    /// При объявлении структуры таблицы, если свойство не задано в явном виде,
    /// используется первое поле в списке полей.
    /// Теоретически, можно объявить таблицу без первичного ключа, присвоив свойству пустое значение.
    /// Поля должны присутствовать в списке Columns
    /// </summary>
    public DBxColumns PrimaryKey
    {
      get
      {
        if (_PrimaryKey == null)
          return GetDefaultPrimaryKey();
        else
          return _PrimaryKey;
      }
      set
      {
        CheckNotReadOnly();

        if (value == null)
          throw new ArgumentNullException();
        for (int i = 0; i < value.Count; i++)
        {
          DBxColumnStruct colStr = Columns[value[i]];
          if (colStr == null)
            throw new ArgumentException("В списке полей нет \"" + value[i] + "\"");
        }

        _PrimaryKey = value;
      }
    }
    private DBxColumns _PrimaryKey;

    /// <summary>
    /// Возвращает первичный ключ по умолчанию
    /// </summary>
    /// <returns></returns>
    private DBxColumns GetDefaultPrimaryKey()
    {
      if (Columns.Count == 0)
        return DBxColumns.Empty;
      else
        return new DBxColumns(Columns[0].ColumnName);
    }

    /// <summary>
    /// Первичный ключ в виде списка объектов DBxColumnStruct.
    /// По возможности, следует использовать свойство PrimaryKey
    /// </summary>
    public DBxColumnStruct[] PrimaryKeyColumns
    {
      get
      {
        DBxColumns pk = PrimaryKey;
        DBxColumnStruct[] a = new DBxColumnStruct[pk.Count];
        for (int i = 0; i < pk.Count; i++)
          a[i] = Columns[pk[i]];
        return a;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        switch (value.Length)
        {
          case 0:
            PrimaryKey = DBxColumns.Empty;
            break;
          case 1:
            PrimaryKey = new DBxColumns(value[0].ColumnName);
            break;
          default:
            string[] a = new string[value.Length];
            for (int i = 0; i < value.Length; i++)
              a[i] = value[i].ColumnName;
            PrimaryKey = new DBxColumns(a);
            break;
        }
      }
    }

    /// <summary>
    /// Автоматическое присвоение значения полю первичного ключа.
    /// Если свойство установлено в true (по умолчанию), то при добавлении поля первичного ключа
    /// будет установлен признак автоинкремента.
    /// При создании/обновлении структуры таблицы возникнет ошибка, если свойство первичным ключом
    /// является поле, отличное от целочисленного, а AutoPrimaryKey=true.
    /// Если для таблицы задано отстутсвие первичного ключа, свойство игнорируется.
    /// </summary>
    public bool AutoPrimaryKey
    {
      get { return _AutoPrimaryKey; }
      set
      {
        CheckNotReadOnly();
        _AutoPrimaryKey = value;
      }
    }
    private bool _AutoPrimaryKey;

    /// <summary>
    /// Список описаний индексов
    /// Первичный ключ не входит в индексы
    /// </summary>
    public DBxIndexStructList Indexes { get { return _Indexes; } }
    private readonly DBxIndexStructList _Indexes;

    /// <summary>
    /// Комментарий к таблице (если поддерживается базой данных)
    /// </summary>
    public string Comment
    {
      get { return _Comment; }
      set
      {
        CheckNotReadOnly();
        _Comment = value;
      }
    }
    private string _Comment;

    /// <summary>
    /// Возвращает true, если есть комментарий к таблице, или хотя бы к одному из столбцов или индексов
    /// </summary>
    public bool HasComments
    {
      get
      {
        if (!String.IsNullOrEmpty(this.Comment))
          return true;
        for (int i = 0; i < Columns.Count; i++)
        {
          if (!String.IsNullOrEmpty(Columns[i].Comment))
            return true;
        }
        for (int i = 0; i < Indexes.Count; i++)
        {
          if (!String.IsNullOrEmpty(Indexes[i].Comment))
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Если true (по умолчанию), то метод DBx.UpdateStruct() будет создавать таблицу с помощью CREATE TABLE, если ее не существует, или
    /// проверять ее структуру.
    /// В базе данных могут быть таблицы, которые должны создаваться из пользовательского кода, но при этом в других таблицах могут быть ссылки
    /// на них, а также могут выполняться запросы к этим таблицам.
    /// Например, в SQLite могут создаваться виртуальные таблицы для полнотекстного поиска. Для такой таблицы следует установить DBxTableStruct.AutoCreate=false,
    /// и выполнять SQL-запрос "CREATE VIRTUAL TABLE"
    /// </summary>
    public bool AutoCreate
    {
      get { return _AutoCreate; }
      set 
      {
        CheckNotReadOnly();
        _AutoCreate = value;
      }
    }
    private bool _AutoCreate;

    #endregion

    #region Методы

    /// <summary>
    /// Создает пустую таблицу <see cref="System.Data.DataTable"/> со всеми полями, описанными в структуре таблицы.
    /// Для каждого описания столбца вызывается метод <see cref="FreeLibSet.Data.DBxColumnStruct.CreateDataColumn()"/>.
    /// Ограничения <see cref="System.Data.DataColumn.MaxLength"/> не устанавливается.
    /// Индексы <see cref="FreeLibSet.Data.DBxIndexStruct"/> не учитываются.
    /// </summary>
    /// <returns>Пустой DataTable</returns>
    public DataTable CreateDataTable()
    {
      DataTable table = new DataTable(TableName);
      for (int i = 0; i < Columns.Count; i++)
        table.Columns.Add(Columns[i].CreateDataColumn());
      return table;
    }

    /// <summary>
    /// Установка свойcтв DataColumn.MaxLength.
    /// Таблица <paramref name="table"/> содержит столбцы, для которых устанавливаются ограничения.
    /// Если в стаблице есть посторонние столбцы, они пропускаются
    /// </summary>
    /// <param name="table">Таблица со столбцами, для которых требуется установить свойства</param>
    public void InitDataRowLimits(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      foreach (DataColumn column in table.Columns)
      {
        int p = Columns.IndexOf(column.ColumnName);
        if (p < 0)
          continue; // служебное поле
        DBxColumnStruct colDef = Columns[p];
        if (colDef.ColumnType == DBxColumnType.String && column.DataType == typeof(string))
          column.MaxLength = colDef.MaxLength;
        /* Нельзя !
        if (FieldDef.Type==DatabaseStruct.FieldType.String ||
            FieldDef.Type==DatabaseStruct.FieldType.Date ||
            FieldDef.Type==DatabaseStruct.FieldType.Time)
          Column.AllowDBNull=FieldDef.CanBeEmpty;
        */
      }
    }


    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание структуры была переведена в режим просмотра
    /// </summary>
    public bool IsReadOnly { get { return _Columns.IsReadOnly; } }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      _Columns.CheckNotReadOnly();
    }

    /// <summary>
    /// Переводит описание структуры в режим просмотра.
    /// Повторные вызовы игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      _Columns.SetReadOnly();
      _Indexes.SetReadOnly();

      if (_PrimaryKey == null)
        _PrimaryKey = GetDefaultPrimaryKey(); // Я думаю, можно обойтись без lock
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию описания таблицы, доступную для редактирования (<see cref="IsReadOnly"/>=false)
    /// </summary>
    /// <returns>Новый объект DBxTableStruct</returns>
    public DBxTableStruct Clone()
    {
      return Clone(this.TableName);
    }

    /// <summary>
    /// Создает копию описания таблицы, доступную для редактирования (<see cref="IsReadOnly"/>=false).
    /// Эта перегрузка позволяет заменить имя таблицы
    /// </summary>
    /// <returns>Новый объект <see cref="DBxTableStruct"/></returns>
    /// <param name="tableName">Имя таблицы для новой структурой <see cref="DBxTableStruct.TableName"/></param>
    public DBxTableStruct Clone(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException(tableName);
      return new DBxTableStruct(this, tableName);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Методы проверки

    /// <summary>
    /// Проверяет корректность описаний таблицы.
    /// Обычно следует проверять структуру базы данных в-целом, а не отдельной таблицы,
    /// так как там проверяется корректность ссылочных полей.
    /// Эта версия не проверяет корректность имен обычных полей
    /// </summary>
    public void CheckStruct()
    {
      CheckStruct(null);
    }

    /// <summary>
    /// Проверяет корректность описаний таблицы.
    /// Обычно следует проверять структуру базы данных в-целом, а не отдельной таблицы,
    /// так как там проверяется корректность ссылочных полей.
    /// Эта версия может проверять корректность имен обычных полей
    /// </summary>
    /// <param name="db">База данных для проверки корректности имен полей или null</param>
    public void CheckStruct(DBx db)
    {
      if (db != null)
      {
        string errorText;
        if (!db.IsValidTableName(TableName, out errorText))
          throw new DBxStructException(this, "Неправильное имя таблицы \"" + TableName + "\"." + errorText);
      }

      foreach (DBxColumnStruct colDef in Columns)
        colDef.CheckStruct(this, db);
    }

    /// <summary>
    /// Проверяет, что первичным ключом таблицы является единственное целочисленное поле.
    /// Если это не так, генерируется DBxPrimaryKeyException.
    /// </summary>
    /// <returns>Имя поля первичного ключа</returns>
    public string CheckTablePrimaryKeyInt32()
    {
      switch (PrimaryKey.Count)
      {
        case 1:
          DBxColumnStruct colDef = Columns[PrimaryKey[0]];
          if (colDef.DataType != typeof(Int32))
            throw new DBxPrimaryKeyException("Таблица \"" + TableName + "\" имеет первичный ключа по полю \"" + colDef.ColumnName + "\", которое имеет тип ("+colDef.ColumnType.ToString()+"), отличный от Int32");
          return colDef.ColumnName;
        case 0:
          throw new DBxPrimaryKeyException("Таблица \"" + TableName + "\" не имеет первичного ключа. Требуется первичный ключ по целочисленному полю");
        default:
          throw new DBxPrimaryKeyException("Таблица \"" + TableName + "\" имеет составной первичный ключ. Требуется первичный ключ по единственному целочисленному полю");
      }
    }

    #endregion
  }

  /// <summary>
  /// Список столбцов для свойства DBxTableStruct.Columns
  /// </summary>
  [Serializable]
  public class DBxColumnStructList : NamedList<DBxColumnStruct>
  {
    #region Защищенные конструкторы

    internal DBxColumnStructList()
      : base(true)
    {
    }

    internal DBxColumnStructList(int capacity)
      : base(capacity, true)
    {
    }

    #endregion

    #region ReadOnly

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
      for (int i = 0; i < Count; i++)
        this[i].SetReadOnly();
    }

    #endregion

    #region Имена столбцов

    /// <summary>
    /// Добавляет имена всех столбцов в список.
    /// </summary>
    /// <param name="list">Заполняемый список</param>
    public void GetColumnNames(DBxColumnList list)
    {
#if DEBUG
      if (list == null)
        throw new ArgumentNullException("list");
#endif

      list.CheckNotReadOnly();

      for (int i = 0; i < Count; i++)
        list.Add(this[i].ColumnName);
    }

    /// <summary>
    /// Возвращает список имен столбцов в виде объекта DBxColumns.
    /// Каждый раз создается новый объект.
    /// </summary>
    public DBxColumns Columns
    {
      get
      {
        string[] a = new string[Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = this[i].ColumnName;
        return new DBxColumns(a);
      }
    }

    #endregion

    #region Методы добавления столбцов

    /// <summary>
    /// Добавление целочисленого поля первичного ключа "Id"
    /// </summary>
    /// <returns>Описание поля</returns>
    public DBxColumnStruct AddId()
    {
      return AddId("Id");
    }

    /// <summary>
    /// Добавление целочисленного поля первичного ключа с заданным именем
    /// </summary>
    /// <returns>Описание поля</returns>
    public DBxColumnStruct AddId(string columnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
#endif
      if (Count > 0)
        throw new InvalidOperationException("Ключевое поле \"" + columnName + "\" должно быть первым в списке");

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.Nullable = false;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить строковое поле
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="maxLength">Максимальная длина строки (от 1 до 255 символов)</param>
    /// <param name="nullable">True, если поле может содержать пустую строку, False-если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddString(string columnName, int maxLength, bool nullable)
    {
      if (maxLength < 1 || maxLength > 255)
        throw new ArgumentOutOfRangeException("maxLength", maxLength, "Длина строкового поля может быть от 1 до 255 символов");
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.String;
      item.MaxLength = maxLength;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле типа "Дата" (без времени)
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустую дату, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddDate(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Date;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле типа "Дата и время"
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустую дату, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddDateTime(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.DateTime;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле типа "время" (без даты)
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddTime(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Time;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 4-байтное целое со знаком
    /// Поле может содержать значение NULL
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 4-байтное целое со знаком
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить целочисленное поле для хранения значений в заданном диапазоне (подбирается подходящий тип поля)
    /// Поле может содержать значение NULL
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="minValue">Диапазон значений, которые должны храниться в поле - минимальное значение</param>
    /// <param name="maxValue">Диапазон значений, которые должны храниться в поле - максимальное значение</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, int minValue, int maxValue)
    {
      if (minValue > maxValue)
        throw new ArgumentException("Максимальное значение не может быть меньше минимального", "maxValue");

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.MinValue = minValue;
      item.MaxValue = maxValue;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить целочисленное поле для хранения значений в заданном диапазоне (подбирается подходящий тип поля)
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="minValue">Диапазон значений, которые должны храниться в поле - минимальное значение</param>
    /// <param name="maxValue">Диапазон значений, которые должны храниться в поле - максимальное значение</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, int minValue, int maxValue, bool nullable)
    {
      if (minValue > maxValue)
        throw new ArgumentException("Максимальное значение не может быть меньше минимального", "maxValue");

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.MinValue = minValue;
      item.MaxValue = maxValue;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить целочисленное поле для хранения значений в заданном диапазоне (подбирается подходящий тип поля)
    /// Поле может содержать значение NULL.
    /// Если поле предназначено для хранения перечислимого значения, можно использовать функцию DataTools.GetEnumRange()
    /// для получения диапазона
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="range">Диапазон значений, которые должны храниться в поле</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, MinMax<Int32> range)
    {
      if (range.HasValue)
        return AddInt(columnName, range.MinValue, range.MaxValue);
      else
        return AddInt(columnName);
    }

    /// <summary>
    /// Добавить целочисленное поле для хранения значений в заданном диапазоне (подбирается подходящий тип поля)
    /// Если поле предназначено для хранения перечислимого значения, можно использовать функцию DataTools.GetEnumRange()
    /// для получения диапазона.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="range">Диапазон значений, которые должны храниться в поле</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, MinMax<Int32> range, bool nullable)
    {
      if (range.HasValue)
        return AddInt(columnName, range.MinValue, range.MaxValue, nullable);
      else
        return AddInt(columnName, nullable);
    }

    /// <summary>
    /// Добавить целочисленное поле.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="minValue">Минимальное значение, которое нужно хранить в поле</param>
    /// <param name="maxValue">Максимальное значение, которое нужно хранить в поле</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, long minValue, long maxValue)
    {
      if (minValue > maxValue)
        throw new ArgumentException("Максимальное значение не может быть меньше минимального", "maxValue");

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.MinValue = minValue;
      item.MaxValue = maxValue;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить целочисленное поле.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="minValue">Минимальное значение, которое нужно хранить в поле</param>
    /// <param name="maxValue">Максимальное значение, которое нужно хранить в поле</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt(string columnName, long minValue, long maxValue, bool nullable)
    {
      if (minValue > maxValue)
        throw new ArgumentException("Максимальное значение не может быть меньше минимального", "maxValue");

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.MinValue = minValue;
      item.MaxValue = maxValue;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 2-байтное целое со знаком.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt16(string columnName)
    {
      return AddInt(columnName, Int16.MinValue, Int16.MaxValue);
    }

    /// <summary>
    /// Добавить поле, содержащее 2-байтное целое со знаком.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt16(string columnName, bool nullable)
    {
      return AddInt(columnName, Int16.MinValue, Int16.MaxValue, nullable);
    }

    /// <summary>
    /// Добавить поле, содержащее 8-байтное целое со знаком.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt64(string columnName)
    {
      return AddInt(columnName, Int64.MinValue, Int64.MaxValue);
    }

    /// <summary>
    /// Добавить поле, содержащее 8-байтное целое со знаком.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddInt64(string columnName, bool nullable)
    {
      return AddInt(columnName, Int64.MinValue, Int64.MaxValue, nullable);
    }

    /// <summary>
    /// Добавить поле, содержащее денежную сумму.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddMoney(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Money;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее денежную сумму.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddMoney(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Money;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 4-байтное число с плавающей точкой.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddSingle(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Float;
      item.MinValue = Single.MinValue;
      item.MaxValue = Single.MaxValue;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 4-байтное число с плавающей точкой.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddSingle(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Float;
      item.MinValue = Single.MinValue;
      item.MaxValue = Single.MaxValue;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 8-байтное число с плавающей точкой.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddDouble(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Float;
      item.MinValue = Double.MinValue;
      item.MaxValue = Double.MaxValue;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле, содержащее 8-байтное число с плавающей точкой.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddDouble(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Float;
      item.MinValue = Double.MinValue;
      item.MaxValue = Double.MaxValue;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить логическое поле, содержащее значения True или False.
    /// Поле может содержать значение NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddBoolean(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Boolean;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить логическое поле, содержащее значения True или False.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddBoolean(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Boolean;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    /// <summary>
    /// Объявление ссылочного поля
    /// </summary>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <param name="masterTableName">Имя таблицы, на которую выполняется ссылка</param>
    /// <param name="nullable">Является ли ссылка необязательной</param>
    /// <param name="refType">Действия при удалении строки, на которую есть ссылка</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddReference(string columnName, string masterTableName, bool nullable, DBxRefType refType)
    {
      if (String.IsNullOrEmpty(masterTableName))
        throw new ArgumentNullException("masterTableName");

      if (refType == DBxRefType.Clear && (!nullable))
        throw new ArgumentException("Тип ссылки не может быть DBxRefType.Clear, если nullable=false", "refType");

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Int;
      item.MasterTableName = masterTableName;
      item.Nullable = nullable;
      item.RefType = refType;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавление ссылочного поля (RefType=Disallow)
    /// </summary>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <param name="masterTableName">Имя таблицы, на которую выполняется ссылка</param>
    /// <param name="nullable">Является ли ссылка необязательной</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddReference(string columnName, string masterTableName, bool nullable)
    {
      return AddReference(columnName, masterTableName, nullable, DBxRefType.Disallow);
    }

    /// <summary>
    /// Добавление обязательного ссылочного поля  (Nullable=false, RefType=Disallow)
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="masterTableName">Таблица, на которую ссылается поле</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddReference(string columnName, string masterTableName)
    {
      return AddReference(columnName, masterTableName, false, DBxRefType.Disallow);
    }

    /// <summary>
    /// Добавление МЕМО-поля, хранящего текст
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddMemo(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Memo;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавление поля для хранения конфигурационных данных в XML-формате
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddXmlConfig(string columnName)
    {
      // В текущей реализации не отличается от прочих XML-полей

      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Xml;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавление поля для хранения данных в XML-формате, кроме конфигурационных данных
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddXml(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Xml;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавление поля с двоичными данными
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddBinary(string columnName)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Binary;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить поле типа GUID
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="nullable">True, если поле может содержать значение NULL, False-если значение поля является обязательным</param>
    /// <returns>Созданный объект объявления поля</returns>
    public DBxColumnStruct AddGuid(string columnName, bool nullable)
    {
      DBxColumnStruct item = new DBxColumnStruct(columnName);
      item.ColumnType = DBxColumnType.Guid;
      item.Nullable = nullable;
      Add(item);
      return item;
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает описание последнего добавленного столбца.
    /// Удобно, например, для установки свойства Comment после вызова AddXXX().
    /// </summary>
    public DBxColumnStruct LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

    /// <summary>
    /// Возвращает true, если в списке столбцов есть BLOB-поля.
    /// Ищет поля типа Memo, Xml и Binary.
    /// </summary>
    /// <returns>Наличие полей</returns>
    public bool ContainsBlob()
    {
      for (int i = 0; i < Count; i++)
      {
        switch (this[i].ColumnType)
        { 
          case DBxColumnType.Binary:
          case DBxColumnType.Memo:
          case DBxColumnType.Xml:
            return true;
        }
      }
      return false;
    }

    #endregion
  }

  /// <summary>
  /// Описание одного реального поля в базе данных
  /// </summary>
  [Serializable]
  public class DBxColumnStruct : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание столбца с неизвестным типом.
    /// Свойство Nullable принимает значение True.
    /// Остальные свойства имеют нулевое значение.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public DBxColumnStruct(string columnName)
      : base(columnName)
    {
      _ColumnType = DBxColumnType.Unknown;
      _DefaultValue = DBNull.Value;
    }

    private DBxColumnStruct(DBxColumnStruct source)
      : base(source.ColumnName)
    {
      _ColumnType = source.ColumnType;
      _MaxLength = source.MaxLength;
      _DefaultValue = source._DefaultValue;
      _MasterTableName = source.MasterTableName;
      _MinValue = source.MinValue;
      _MaxValue = source.MaxValue;
      _RefType = source.RefType;
      _Comment = source.Comment;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя столбца
    /// </summary>
    public string ColumnName { get { return base.Code; } }

    /// <summary>
    /// Тип столбца
    /// </summary>
    public DBxColumnType ColumnType
    {
      get { return _ColumnType; }
      set { _ColumnType = value; }
    }
    private DBxColumnType _ColumnType;

    /// <summary>
    /// Максимальная длина для текстового поля
    /// </summary>
    public int MaxLength
    {
      get { return _MaxLength; }
      set
      {
        CheckNotReadOnly();
        _MaxLength = value;
      }
    }
    private int _MaxLength;

    /// <summary>
    /// true, если поле может содержать значение null.
    /// Свойства Nullable и DefaultValue являются взаимоисключающими. Может быть установлено только одно из двух свойств, 
    /// или оба свойства не установлены.
    /// </summary>
    public bool Nullable
    {
      get { return _DefaultValue is DBNull; }
      set
      {
        CheckNotReadOnly();
        if (value)
          _DefaultValue = DBNull.Value;
        else
          _DefaultValue = null;
      }
    }

    // Нельзя задавать выражения в конструкции DEFAULT.
    // По крайней мере, MS SQL, PostgreSQL и SQLite поддерживают только константные выражения
    ///// <summary>
    ///// Значения по умолчанию для столбца.
    ///// Свойства Nullable и Default являются взаимоисключающими. Может быть установлено только одно из двух свойств, 
    ///// или оба свойства не установлены.
    ///// </summary>
    //public DBxExpression Default
    //{
    //  get { return _Default; }
    //  set
    //  {
    //    CheckNotReadOnly();
    //    _Default = value;
    //    if (value != null)
    //      _Nullable = false;
    //  }
    //}
    //private DBxExpression _Default;

    /// <summary>
    /// Значение по умолчанию для столбца. Задается выражением ADD COLUMN ... DEFAULT (DefaultValue)
    /// Свойства Nullable и DefaultValue являются взаимоисключающими. Может быть установлено только одно из двух свойств, 
    /// или оба свойства не установлены.
    /// </summary>
    public object DefaultValue
    {
      get 
      {
        if (_DefaultValue is DBNull)
          return null;
        else
          return _DefaultValue; 
      }
      set
      {
        CheckNotReadOnly();
        _DefaultValue = value;
      }
    }

    /// <summary>
    /// Используется для реализации свойств DefaultValue и Nullable.
    /// Nullable=true задается значением DBNull
    /// </summary>
    private object _DefaultValue;

    /// <summary>
    /// Устанавливает свойство DefaultValue равным константному значению соответствующего типа
    /// </summary>
    public void SetDefaultValue()
    {
      DefaultValue = DBxTools.GetDefaultValue(this.ColumnType);
    }

    /// <summary>
    /// Возвращает DefaultValue как объект DBxConst
    /// </summary>
    public DBxConst DefaultExpression
    {
      get
      {
        if (DefaultValue == null)
          return null;
        else
          return new DBxConst(DefaultValue, ColumnType);
      }
      set
      {
        if (value == null)
          DefaultValue = null;
        else
          DefaultValue = value.Value;
      }
    }

    /// <summary>
    /// Для ссылочных полей - имя таблицы, на которое ссылается поле
    /// </summary>
    public string MasterTableName
    {
      get { return _MasterTableName; }
      set
      {
        CheckNotReadOnly();
        _MasterTableName = value;
      }
    }
    private string _MasterTableName;
    /*
        /// <summary>
        /// Для ссылочных полей - имя ключевого поля (обычно "Id"), на которое ссылается поле
        /// </summary>
        public string RefColumnName 
        {
          get { return FRefColumnName; }
          set
          {
            CheckNotReadOnly();
            FRefColumnName = value;
          }
        }
        private string FRefColumnName;
      */

    /// <summary>
    /// Для ссылочных полей - правила при удалении строки со ссылочным полем
    /// </summary>
    public DBxRefType RefType
    {
      get { return _RefType; }
      set
      {
        CheckNotReadOnly();
        _RefType = value;
      }
    }
    private DBxRefType _RefType;

    /// <summary>
    /// Минимально допустимое значение для числового поля
    /// </summary>
    public double MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotReadOnly();
        _MinValue = value;
      }
    }
    private double _MinValue;

    /// <summary>
    /// Максимально допустимое значение для числового поля
    /// </summary>
    public double MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotReadOnly();
        _MaxValue = value;
      }
    }
    private double _MaxValue;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb);
      return sb.ToString();
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <param name="sb">Сюда записывается текстовое представление</param>
    public void ToString(StringBuilder sb)
    {
      sb.Append(ColumnName);

      sb.Append(" (");
      sb.Append(ColumnType.ToString());
      sb.Append(")");

      if (MaxLength > 0)
      {
        sb.Append(", MaxLength=");
        sb.Append(MaxLength);
      }

      if (Nullable)
        sb.Append(", NULL");
      else
        sb.Append(", NOT NULL");

      if (MinValue != 0 || MaxValue != 0)
      {
        sb.Append(", min=");
        sb.Append(MinValue);
        sb.Append(", max=");
        sb.Append(MaxValue);
      }

      if (!String.IsNullOrEmpty(MasterTableName))
      {
        sb.Append(", RefTableName=\"");
        sb.Append(MasterTableName);
        sb.Append("\" (");
        sb.Append(RefType);
        sb.Append(")");
      }
    }

    /// <summary>
    /// Комментарий к столбцу (если поддерживается базой данных)
    /// </summary>
    public string Comment
    {
      get { return _Comment; }
      set
      {
        CheckNotReadOnly();
        _Comment = value;
      }
    }
    private string _Comment;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание столбца находится в режиме "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию описания столбца.
    /// Копия не привязана к описанию таблицы и имеет IsReadOnly=false
    /// </summary>
    /// <returns>Копия описания столбца</returns>
    public DBxColumnStruct Clone()
    {
      return new DBxColumnStruct(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Создание столбца для DataTable

    /// <summary>
    /// Определение типа данных для DataColumn.DataType
    /// </summary>
    public Type DataType
    {
      get
      {
        switch (ColumnType)
        {
          case DBxColumnType.Unknown:
            return null;

          case DBxColumnType.String:
            return typeof(string);

          case DBxColumnType.Int:
            if (MinValue == 0 && MaxValue == 0)
              return typeof(Int32);
            else if (MinValue >= Byte.MinValue && MaxValue <= Byte.MaxValue)
              return typeof(Byte);
            else if (MinValue >= SByte.MinValue && MaxValue <= SByte.MaxValue)
              return typeof(SByte);
            else if (MinValue >= Int16.MinValue && MaxValue <= Int16.MaxValue)
              return typeof(Int16);
            else if (MinValue >= UInt16.MinValue && MaxValue <= UInt16.MaxValue)
              return typeof(UInt16);
            else if (MinValue >= Int32.MinValue && MaxValue <= Int32.MaxValue)
              return typeof(Int32);
            else if (MinValue >= UInt32.MinValue && MaxValue <= UInt32.MaxValue)
              return typeof(UInt32);
            else if (MinValue >= Int64.MinValue && MaxValue <= Int64.MaxValue)
              return typeof(Int64);
            else if (MinValue >= UInt64.MinValue && MaxValue <= UInt64.MaxValue)
              return typeof(UInt64);
            else
              return null; // не знаю, что вернуть

          case DBxColumnType.Float:
            if (MinValue == 0 && MaxValue == 0)
              return typeof(Single);
            else if (MinValue >= Single.MinValue && MaxValue <= Single.MaxValue)
              return typeof(Single);
            else
              return typeof(Double);

          case DBxColumnType.Money:
            return typeof(Decimal);

          case DBxColumnType.Boolean:
            return typeof(Boolean);

          case DBxColumnType.Date:
          case DBxColumnType.DateTime:
            return typeof(DateTime);

          case DBxColumnType.Time:
            return typeof(TimeSpan);

          case DBxColumnType.Guid:
            //return typeof(string); // Guid храниться не может
            // 28.04.2020. 
            // Нет, может. Это во встроенной справке VS 2005 для DataColumn.DataType почему-то не указан тип System.Guid.
            // Проверено на начальной, без service pack, версии Net Framework 2.0.50727.42
            // Работает сортировка DataView.Sort, DataView.Find(), Primary key и DataTable.Rows.Find().
            return typeof(Guid); 

          case DBxColumnType.Memo:
          case DBxColumnType.Xml:
            return typeof(string);

          case DBxColumnType.Binary:
            return typeof(byte[]);

          default:
            throw new NotSupportedException("Неизвестный тип столбца " + ColumnType.ToString());
        }
      }
    }

    /// <summary>
    /// Создает объект DataColumn
    /// </summary>
    /// <returns>Столбец для таблицы DataTable</returns>
    public DataColumn CreateDataColumn()
    {
      return CreateDataColumn(this.ColumnName);
    }

    /// <summary>
    /// Создает объект DataColumn с возможностью переопределить имя столбца
    /// </summary>
    /// <returns>Столбец для таблицы DataTable</returns>
    public DataColumn CreateDataColumn(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      Type t = DataType;
      if (t == null)
        throw new InvalidOperationException("Не удалось определить тип данных для столбца " + ToString());
      DataColumn column = new DataColumn(columnName, t);
      column.AllowDBNull = Nullable;
      if (MaxLength > 0)
        column.MaxLength = MaxLength;
      return column;
    }

    #endregion

    #region Проверка

    internal void CheckStruct(DBxTableStruct table, DBx db)
    {
      if (db != null)
      {
        string ErrorText;
        if (!db.IsValidColumnName(ColumnName, false, out ErrorText))
          throw new DBxStructException(table, "Неправильное имя столбца \"" + ColumnName + "\". " + ErrorText);
      }

      switch (ColumnType)
      {
        case DBxColumnType.String:
          if (MaxLength < 1)
            throw new DBxStructException(table, "Для текстового столбца \"" + ColumnName + "\" не задана длина строки");
          break;
        case DBxColumnType.Int:
        case DBxColumnType.Float:
          if (MinValue != 0.0 || MaxValue != 0.0)
          {
            if (MinValue > MaxValue)
              throw new DBxStructException(table, "Для числового столбца \"" + ColumnName + "\" не задан неправильный диапазон значений: {" + MinValue.ToString() + ":" + MaxValue.ToString() + "}");
          }
          break;
      }
    }

    #endregion
  }

  #region Перечисление DBxColumnType

  /// <summary>
  /// Возможные типы полей
  /// </summary>
  public enum DBxColumnType
  {
    /// <summary>
    /// Не удалось определить тип поля
    /// </summary>
    Unknown,

    /// <summary>
    /// Строковое поле. 
    /// При определении поля должно быть установлено свойство <see cref="DBxColumnStruct.MaxLength"/>
    /// </summary>
    String,

    /// <summary>
    /// Целочисленное поле.
    /// При определении поля могут быть заданы свойства <see cref="DBxColumnStruct.MinValue"/> и <see cref="DBxColumnStruct.MaxValue"/> для определения наиболее подходящего типа
    /// поля в базе данных
    /// </summary>
    Int,

    /// <summary>
    /// Число с плавающей точкой
    /// При определении поля могут быть заданы свойства <see cref="DBxColumnStruct.MinValue"/> и <see cref="DBxColumnStruct.MaxValue"/> для определения наиболее подходящего типа
    /// поля в базе данных
    /// </summary>
    Float,

    /// <summary>
    ///  Денежный тип
    /// </summary>
    Money,

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
    DateTime,

    /// <summary>
    /// Поле для хранения интервала времени или времени суток без даты (TimeSpan)
    /// </summary>
    Time,

    /// <summary>
    /// Поле типа GUID.
    /// Если база данных не поддерживает хранение GUId'ов в собственном формате,
    /// используется строковое поле длиной 36 символов в формате "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    /// </summary>
    Guid,

    /// <summary>
    /// Длинный текст
    /// </summary>
    Memo,

    /// <summary>
    /// Текстовое МЕМО-поле, содержащее данные в Xml-формате
    /// </summary>
    Xml,

    /// <summary>
    /// Двоичные (не текстовые) данные произвольной длины
    /// </summary>
    Binary,
  }

  #endregion

  #region Перечисление DBxRefType

  /// <summary>
  /// Поведение при удалении строки таблицы (Master), на которую ссылается поле в текущей таблице (Details)
  /// </summary>
  public enum DBxRefType
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
    /// Фиктивная ссылка. Создается обычное поле, о котором база данных
    /// не знает, что оно ссылочное. Не используется Foreign Key и не создается индекс.
    /// Проверка целостности ссылки лежит на программе.
    /// </summary>
    Emulation,
  }

  #endregion

  /// <summary>
  /// Список столбцов для свойства DBxTableStruct.Columns
  /// </summary>
  [Serializable]
  public class DBxIndexStructList : NamedList<DBxIndexStruct>
  {
    #region Защищенные конструкторы

    internal DBxIndexStructList()
    {
    }

    internal DBxIndexStructList(int capacity)
      : base(capacity)
    {
    }

    #endregion

    #region ReadOnly

    internal new void SetReadOnly()
    {
      base.SetReadOnly();

      for (int i = 0; i < Count; i++)
        this[i].SetReadOnly(); // 18.09.2019
    }

    #endregion

    #region Добавление индекса

    /// <summary>
    /// Добавляет описание индекса, основанного на заданном списке имен столбцов
    /// </summary>
    /// <param name="columns">Имена столбцов</param>
    /// <returns>Описание индекса</returns>
    public DBxIndexStruct Add(DBxColumns columns)
    {
      DBxIndexStruct item = new DBxIndexStruct("Index" + (Count + 1).ToString(), columns);
      base.Add(item);
      return item;
    }

    /// <summary>
    /// Добавляет описание индекса, основанного на заданном списке имен столбцов/
    /// Имена столбцов задаются через запятую
    /// </summary>
    /// <param name="columns">Имена столбцов</param>
    /// <returns>Описание индекса</returns>
    public DBxIndexStruct Add(string columns)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columns))
        throw new ArgumentNullException("columns");
#endif

      return Add(new DBxColumns(columns));
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает описание последнего добавленного столбца.
    /// Удобно использовать для установки свойства Comment после вызова метода Add().
    /// </summary>
    public DBxIndexStruct LastAdded
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

  /// <summary>
  /// Описание одного индекса в базе данных
  /// </summary>
  [Serializable]
  public class DBxIndexStruct : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание индекса
    /// </summary>
    /// <param name="indexName">Имя индекса. Должно быть задано</param>
    /// <param name="columns">Список столбцо. Не может быть пустым</param>
    public DBxIndexStruct(string indexName, DBxColumns columns)
      : base(indexName)
    {
      if (columns == null)
        throw new ArgumentNullException("columns");
      if (columns.Count == 0)
        throw new ArgumentException("Список полей не задан", "columns");
      _Columns = columns;
    }

    private DBxIndexStruct(DBxIndexStruct source)
      : base(source.IndexName)
    {
      _Columns = source.Columns;
      _Comment = source.Comment;
      _IsReadOnly = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя индекса (создается автоматически при добавлении индекса в список)
    /// </summary>
    public string IndexName { get { return base.Code; } }

    /// <summary>
    /// Имена столбцов.
    /// </summary>
    public DBxColumns Columns { get { return _Columns; } }
    private readonly DBxColumns _Columns;

    /// <summary>
    /// Комментарий к индексу (если поддерживается базой данных)
    /// </summary>
    public string Comment
    {
      get { return _Comment; }
      set
      {
        CheckNotReadOnly();
        _Comment = value;
      }
    }
    private string _Comment;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание индекса было добавлено в список, а он был переведен в состояние просмотра.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию описания индекса.
    /// Созданная копия не привязана к описанию таблицы
    /// </summary>
    /// <returns></returns>
    public DBxIndexStruct Clone()
    {
      return new DBxIndexStruct(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }
}
