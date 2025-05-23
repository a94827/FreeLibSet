﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
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
    Decimal,

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
  /// Описание одного поля в структуре таблицы данных
  /// </summary>
  [Serializable]
  public class DBxColumnStruct : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание столбца с неизвестным типом.
    /// Свойство <see cref="Nullable"/> принимает значение True.
    /// Остальные свойства имеют нулевое значение.
    /// </summary>
    /// <param name="columnName">Имя столбца. Не может быть пустой строкой</param>
    public DBxColumnStruct(string columnName)
      : base(columnName)
    {
      _ColumnType = DBxColumnType.Unknown;
      _DefaultValue = DBNull.Value;
    }

    private DBxColumnStruct(DBxColumnStruct source, string columnName)
      : base(columnName)
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
    /// Имя столбца. Задается в конструкторе. Не может быть пустой строкой
    /// </summary>
    public string ColumnName { get { return base.Code; } }

    /// <summary>
    /// Тип столбца. По умолчанию - <see cref="DBxColumnType.Unknown"/>.
    /// </summary>
    public DBxColumnType ColumnType
    {
      get { return _ColumnType; }
      set
      {
        CheckNotReadOnly(); // 07.06.2023
        _ColumnType = value;
      }
    }
    private DBxColumnType _ColumnType;

    /// <summary>
    /// Максимальная длина для текстового поля. По умолчанию - 0 - длина не указана.
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
    /// Свойства <see cref="Nullable"/> и <see cref="DefaultValue"/> являются взаимоисключающими. Может быть установлено только одно из двух свойств, 
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
    /// Свойства <see cref="Nullable"/> и <see cref="DefaultValue"/> являются взаимоисключающими. Может быть установлено только одно из двух свойств, 
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
    /// Устанавливает свойство <see cref="DefaultValue"/> равным константному значению соответствующего типа
    /// </summary>
    public void SetDefaultValue()
    {
      DefaultValue = DBxTools.GetDefaultValue(this.ColumnType);
    }

    /// <summary>
    /// Возвращает <see cref="DefaultValue"/> как объект <see cref="DBxConst"/>.
    /// Если значение по умолчанию не было установлено, возвращает null.
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
    /// Для ссылочных полей - имя таблицы, на которое ссылается поле. По умолчанию - пустая строка
    /// </summary>
    public string MasterTableName
    {
      get { return _MasterTableName ?? String.Empty; }
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
    /// Для ссылочных полей - правила при удалении строки со ссылочным полем.
    /// По умолчанию - <see cref="DBxRefType.Disallow"/>
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
    /// Минимально допустимое значение для числового поля. По умолчанию - 0.
    /// Если значения <see cref="MinValue"/> и <see cref="MaxValue"/> оба равны 0, то диапазон значений не определен.
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
    /// Максимально допустимое значение для числового поля. По умолчанию - 0.
    /// Если значения <see cref="MinValue"/> и <see cref="MaxValue"/> оба равны 0, то диапазон значений не определен.
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
    /// Комментарий к столбцу (если поддерживается базой данных).
    /// По умолчанию - пустая строка
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
    /// Возвращает true, если описание столбца находится в режиме "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    /// <summary>
    /// Переводит описание столбца в режим "Только чтение"
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию описания столбца.
    /// Копия не привязана к описанию таблицы и имеет <see cref="IsReadOnly"/>=false.
    /// Созданный столбец имеет то же имя, что и текущий объект.
    /// </summary>
    /// <returns>Копия описания столбца</returns>
    public DBxColumnStruct Clone()
    {
      return new DBxColumnStruct(this, this.ColumnName);
    }


    /// <summary>
    /// Создает копию описания столбца.
    /// Копия не привязана к описанию таблицы и имеет <see cref="IsReadOnly"/>=false.
    /// </summary>
    /// <param name="columnName">Имя нового столбца</param>
    /// <returns>Копия описания столбца</returns>
    public DBxColumnStruct Clone(string columnName)
    {
      return new DBxColumnStruct(this, columnName);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Создание столбца для DataTable

    /// <summary>
    /// Определение типа данных для <see cref="DataColumn.DataType"/>.
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
              return typeof(Double); // 07.06.2023
            else if (MinValue >= Single.MinValue && MaxValue <= Single.MaxValue)
              return typeof(Single);
            else
              return typeof(Double);

          case DBxColumnType.Decimal:
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
            throw new NotSupportedException("Unknown column type " + ColumnType.ToString());
        }
      }
    }

    /// <summary>
    /// Создает объект <see cref="DataColumn"/>.
    /// </summary>
    /// <returns>Столбец для таблицы <see cref="DataTable"/></returns>
    public DataColumn CreateDataColumn()
    {
      return CreateDataColumn(this.ColumnName);
    }

    /// <summary>
    /// Создает объект <see cref="DataColumn"/> с возможностью переопределить имя столбца.
    /// Устанавливает свойства 
    /// <see cref="DataColumn.ColumnName"/>,
    /// <see cref="DataColumn.DataType"/>, 
    /// <see cref="DataColumn.AllowDBNull"/>,
    /// <see cref="DataColumn.MaxLength"/>
    /// </summary>
    /// <returns>Столбец для таблицы <see cref="DataTable"/></returns>
    public DataColumn CreateDataColumn(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      Type t = DataType;
      if (t == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DataType");
      DataColumn column = new DataColumn(columnName, t);
      column.AllowDBNull = Nullable;
      if (DefaultValue != null)
        column.DefaultValue = DefaultValue; // 07.06.2024
      if (MaxLength > 0)
        column.MaxLength = MaxLength;
      return column;
    }

    #endregion

    #region DbType

    ///// <summary>
    ///// Возвращает тип данных для столбца
    ///// </summary>
    //internal DbType DbType
    //{
    //  get
    //  {
    //    switch (ColumnType)
    //    {
    //      case DBxColumnType.String: return DbType.String;
    //      case DBxColumnType.Int: return DbType.Int64; // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!
    //      case DBxColumnType.Float: return DbType.Double;
    //      case DBxColumnType.Decimal: return DbType.Decimal;
    //      case DBxColumnType.Boolean: return DbType.Boolean;
    //      case DBxColumnType.Date: return DbType.Date;
    //      case DBxColumnType.DateTime: return DbType.DateTime;
    //      case DBxColumnType.Time: return DbType.Time;
    //      case DBxColumnType.Guid: return DbType.Guid;
    //      case DBxColumnType.Memo: return DbType.String;
    //      case DBxColumnType.Xml: return DbType.Xml;
    //      case DBxColumnType.Binary: return DbType.Binary;
    //      default: return DbType.Object; // ?
    //    }
    //  }
    //}

    #endregion
  }
}
