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
  /// Описание структуры одной таблицы данных
  /// </summary>
  [Serializable]
  public class DBxTableStruct : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Вложенные классы

    /// <summary>
    /// Список столбцов для свойства <see cref="Columns"/>
    /// </summary>
    [Serializable]
    public class ColumnCollection : NamedList<DBxColumnStruct>
    {
      #region Защищенные конструкторы

      internal ColumnCollection()
        : base(true)
      {
      }

      internal ColumnCollection(int capacity)
        : base(capacity, true)
      {
      }

      #endregion

      #region IsReadOnly

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
      /// Возвращает список имен столбцов в виде объекта <see cref="DBxColumns"/>.
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
      /// Добавление поля первичного ключа с заданным именем.
      /// Устанавливает <see cref="DBxColumnStruct.Nullable"/>=false.
      /// Тип данных <see cref="DBxColumnStruct.ColumnType"/> временно устанавливается в <see cref="DBxColumnType.Unknown"/>.
      /// Он заменяется на реальный тип.
      /// Если требуется ключевое поле определеного типа, используйте другие методы добавления, например, <see cref="AddInt32(string, bool)"/> или
      /// <see cref="AddInt64(string, bool)"/>, передавая false в качестве аргумента nullable.
      /// </summary>
      /// <returns>Описание поля</returns>
      public DBxColumnStruct AddId(string columnName)
      {
#if DEBUG
        if (String.IsNullOrEmpty(columnName))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif
        if (Count > 0)
          throw new InvalidOperationException(String.Format(Res.DBxTableStruct_Err_PrimaryKeyColumnMustBeFirst, columnName));

        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Unknown;
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
          throw ExceptionFactory.ArgOutOfRange("maxLength", maxLength, 1, 255);
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
      /// Добавить поле, содержащее 2-байтное целое со знаком.
      /// </summary>
      /// <param name="columnName">Имя поля</param>
      /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddInt16(string columnName, bool nullable)
      {
        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Int16;
        item.Nullable = nullable;
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавить поле, содержащее 4-байтное целое со знаком
      /// </summary>
      /// <param name="columnName">Имя поля</param>
      /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddInt32(string columnName, bool nullable)
      {
        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Int32;
        item.Nullable = nullable;
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавить поле, содержащее 4-байтное целое со знаком
      /// </summary>
      /// <param name="columnName">Имя поля</param>
      /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddInt64(string columnName, bool nullable)
      {
        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Int64;
        item.Nullable = nullable;
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
      public DBxColumnStruct AddInteger(string columnName, long minValue, long maxValue, bool nullable)
      {
        if (minValue > maxValue)
          throw ExceptionFactory.ArgRangeInverted("minValue", minValue, "maxValue", maxValue);

        if (minValue == 0L && maxValue == 0L)
          return AddInt32(columnName, nullable);

        DBxColumnStruct item = new DBxColumnStruct(columnName);

        if (minValue >= 0L)
        {
          // Беззнаковые типы
          if (maxValue<=Byte.MaxValue)
            item.ColumnType = DBxColumnType.Byte;
          else if (maxValue <= UInt16.MaxValue)
            item.ColumnType = DBxColumnType.UInt16;
          else if (maxValue <= UInt32.MaxValue)
            item.ColumnType = DBxColumnType.UInt32;
          else 
            item.ColumnType = DBxColumnType.UInt64;
        }
        else
        {
          // Знаковые типы
          if (minValue >= SByte.MinValue && maxValue <= SByte.MaxValue)
            item.ColumnType = DBxColumnType.SByte;
          else if (minValue >= Int16.MinValue && maxValue <= Int16.MaxValue)
            item.ColumnType = DBxColumnType.Int16;
          else if (minValue >= Int32.MinValue && maxValue <= Int32.MaxValue)
            item.ColumnType = DBxColumnType.Int32;
          else
            item.ColumnType = DBxColumnType.Int64;
        }
        item.MinValue = minValue;
        item.MaxValue = maxValue;
        item.Nullable = nullable;
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
      public DBxColumnStruct AddInteger(string columnName, int minValue, int maxValue, bool nullable)
      {
        return AddInteger(columnName, (long)minValue, (long)maxValue, nullable);
      }

      /// <summary>
      /// Добавить целочисленное поле для хранения значений в заданном диапазоне (подбирается подходящий тип поля)
      /// Если поле предназначено для хранения перечислимого значения, можно использовать функцию <see cref="DataTools.GetEnumRange(Type)"/>  
      /// для получения диапазона.
      /// </summary>
      /// <param name="columnName">Имя поля</param>
      /// <param name="range">Диапазон значений, которые должны храниться в поле</param>
      /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddInteger(string columnName, MinMax<Int32> range, bool nullable)
      {
        if (range.HasValue)
          return AddInteger(columnName, (long)(range.MinValue), (long)(range.MaxValue), nullable);
        else
          return AddInt32(columnName, nullable);
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
        item.ColumnType = DBxColumnType.Single;
        item.Nullable = nullable;
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
        item.ColumnType = DBxColumnType.Double;
        item.Nullable = nullable;
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавить поле, содержащее денежную сумму.
      /// </summary>
      /// <param name="columnName">Имя поля</param>
      /// <param name="nullable">True, если поле может содержать пустое значение, False, если значение поля является обязательным</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddDecimal(string columnName, bool nullable)
      {
        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Decimal;
        item.Nullable = nullable;
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавить логическое поле, содержащее значения True или False.
      /// Поле не может содержать значение NULL, а значением по умолчанию является false.
      /// (<see cref="DBxColumnStruct.DefaultValue"/>=false).
      /// </summary>
      /// <param name="columnName">Имя поля</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddBoolean(string columnName)
      {
        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Boolean;
        item.DefaultValue = false; // 01.06.2023
        Add(item);
        return item;
      }

      /// <summary>
      /// Объявление ссылочного поля.
      /// Полю временно назначается тип <see cref="DBxColumnType.Unknown"/>, который заменяется на тип ключевого поля таблицы <paramref name="masterTableName"/>.
      /// </summary>
      /// <param name="columnName">Имя ссылочного поля</param>
      /// <param name="masterTableName">Имя таблицы, на которую выполняется ссылка</param>
      /// <param name="nullable">Является ли ссылка необязательной</param>
      /// <param name="refType">Действия при удалении строки, на которую есть ссылка</param>
      /// <returns>Созданный объект объявления поля</returns>
      public DBxColumnStruct AddReference(string columnName, string masterTableName, bool nullable, DBxRefType refType)
      {
        if (String.IsNullOrEmpty(masterTableName))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("masterTableName");

        if (refType == DBxRefType.Clear && (!nullable))
          throw new ArgumentException(Res.DBxTableStruct_Arg_RefTypeClearForNotNull, "refType");

        DBxColumnStruct item = new DBxColumnStruct(columnName);
        item.ColumnType = DBxColumnType.Unknown;
        item.MasterTableName = masterTableName;
        item.Nullable = nullable;
        item.RefType = refType;
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавление ссылочного поля (<see cref="DBxColumnStruct.RefType"/>=Disallow)
      /// Полю временно назначается тип <see cref="DBxColumnType.Unknown"/>, который заменяется на тип ключевого поля таблицы <paramref name="masterTableName"/>.
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
      /// Удобно, например, для установки свойства <see cref="DBxColumnStruct.Comment"/> после вызова AddXXX().
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
    /// Список столбцов для свойства <see cref="Indexes"/>
    /// </summary>
    [Serializable]
    public class IndexCollection : NamedList<DBxIndexStruct>
    {
      #region Защищенные конструкторы

      internal IndexCollection()
      {
      }

      internal IndexCollection(int capacity)
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
      /// Добавляет описание индекса, основанного на заданном списке имен столбцов.
      /// Имена столбцов задаются через запятую. Нельзя задавать признаки "ASC" / "DESC".
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
      /// Возвращает описание последнего добавленного индекса.
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

      /// <summary>
      /// Поиск индекса для указанного списка столбцов.
      /// Возвращает описание индекса или null, если индекс не найден.
      /// Порядок столбцов имеет значение.
      /// </summary>
      /// <param name="columns">Список столбцов</param>
      /// <returns>Описание индекса или null</returns>
      public DBxIndexStruct FindByColumns(DBxColumns columns)
      {
        if (columns == null)
          return null;
        if (columns.IsEmpty)
          return null;

        foreach (DBxIndexStruct str in this)
        {
          //if (str.Columns == columns)
          if (ColumnsAreEqual(str.Columns, columns))
            return str;
        }
        return null;
      }

      private static bool ColumnsAreEqual(DBxColumns a, DBxColumns b)
      {
        if (a.Count != b.Count)
          return false;
        for (int i = 0; i < a.Count; i++)
        {
          if (!String.Equals(a[i], b[i], StringComparison.Ordinal))
            return false;
        }
        return true;
      }


      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает новое пустое описание таблицы с заданным именем
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public DBxTableStruct(string tableName)
      : base(tableName)
    {
      _Columns = new ColumnCollection();
      _Indexes = new IndexCollection();
      _AutoPrimaryKey = true;
      _AutoCreate = true;
    }

    /// <summary>
    /// Конструктор для клонирования
    /// </summary>
    private DBxTableStruct(DBxTableStruct source, string tableName)
      : base(tableName)
    {
      _Columns = new ColumnCollection(source.Columns.Count);
      for (int i = 0; i < source.Columns.Count; i++)
        _Columns.Add(source.Columns[i].Clone());
      _Indexes = new IndexCollection(source.Indexes.Count);
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
    public ColumnCollection Columns { get { return _Columns; } }
    private readonly ColumnCollection _Columns;

    /// <summary>
    /// Имена всех объявленных столбцов
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
            throw new ArgumentException(String.Format(Res.DBxStruct_Arg_ColumnNotFound, this.TableName, value[i]));
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
    /// Первичный ключ в виде списка объектов <see cref="DBxColumnStruct"/>.
    /// По возможности, следует использовать свойство <see cref="PrimaryKey"/>.
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
    /// При создании/обновлении структуры таблицы возникнет ошибка, если первичным ключом
    /// является поле, отличное от целочисленного, а <see cref="AutoPrimaryKey"/>=true.
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
    /// Список описаний индексов.
    /// Первичный ключ не входит в индексы.
    /// </summary>
    public IndexCollection Indexes { get { return _Indexes; } }
    private readonly IndexCollection _Indexes;

    /// <summary>
    /// Комментарий к таблице (если поддерживается базой данных)
    /// </summary>
    public string Comment
    {
      get { return _Comment ?? String.Empty; }
      set
      {
        CheckNotReadOnly();
        _Comment = value;
      }
    }
    private string _Comment;

    /// <summary>
    /// Возвращает true, если есть комментарий к таблице, или хотя бы к одному из столбцов или индексов.
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
    /// Если true (по умолчанию), то метод DBx.UpdateStruct() (ExtDB.dll) будет создавать таблицу с помощью CREATE TABLE, если ее не существует, или
    /// проверять ее структуру.
    /// В базе данных могут быть таблицы, которые должны создаваться из пользовательского кода, но при этом в других таблицах могут быть ссылки
    /// на них, а также могут выполняться запросы к этим таблицам.
    /// Например, в SQLite могут создаваться виртуальные таблицы для полнотекстного поиска. Для такой таблицы следует установить <see cref="DBxTableStruct.AutoCreate"/>=false,
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
    /// Ограничения <see cref="DataColumn.AllowDBNull"/> устанавливаются.
    /// Ограничения <see cref="DataColumn.MaxLength"/> устанавливается.
    /// Первичный ключ <see cref="DataTable.PrimaryKey"/> для таблицы не устанавливаются.
    /// Индексы <see cref="FreeLibSet.Data.DBxIndexStruct"/> не учитываются.
    /// </summary>
    /// <returns>Пустой объект <see cref="DataTable"/></returns>
    public DataTable CreateDataTable()
    {
      DataTable table = new DataTable(TableName);
      for (int i = 0; i < Columns.Count; i++)
        table.Columns.Add(Columns[i].CreateDataColumn());
      return table;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание структуры была переведена в режим просмотра
    /// </summary>
    public bool IsReadOnly { get { return _Columns.IsReadOnly; } }

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
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
    /// <returns>Новый объект <see cref="DBxTableStruct"/></returns>
    public DBxTableStruct Clone()
    {
      return Clone(this.TableName);
    }

    /// <summary>
    /// Создает копию описания таблицы, доступную для редактирования (<see cref="IsReadOnly"/>=false).
    /// Эта перегрузка позволяет заменить имя таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы для новой структуры <see cref="DBxTableStruct.TableName"/></param>
    /// <returns>Новый объект <see cref="DBxTableStruct"/></returns>
    public DBxTableStruct Clone(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      return new DBxTableStruct(this, tableName);
    }

    object ICloneable.Clone()
    {
      return Clone();
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
    /// <param name="columns">Список столбцов. Не может быть пустым</param>
    public DBxIndexStruct(string indexName, DBxColumns columns)
      : base(indexName)
    {
      if (columns == null)
        throw new ArgumentNullException("columns");
      if (columns.Count == 0)
        throw ExceptionFactory.ArgIsEmpty("columns");
      _Columns = columns;
    }

    private DBxIndexStruct(DBxIndexStruct source, string indexName)
      : base(indexName)
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
      get { return _Comment ?? String.Empty; }
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
    /// Генерирует исключение при <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    internal void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию описания индекса.
    /// Созданная копия не привязана к описанию таблицы и имеет значение <see cref="IsReadOnly"/>=false.
    /// </summary>
    /// <returns>Новый экземпляр описания индекса</returns>
    public DBxIndexStruct Clone()
    {
      return new DBxIndexStruct(this, this.IndexName);
    }

    /// <summary>
    /// Создает копию описания индекса.
    /// Созданная копия не привязана к описанию таблицы и имеет значение <see cref="IsReadOnly"/>=false.
    /// </summary>
    /// <param name="indexName">Имя нового индекса</param>
    /// <returns>Новый экземпляр описания индекса</returns>
    public DBxIndexStruct Clone(string indexName)
    {
      return new DBxIndexStruct(this, indexName);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }
}
