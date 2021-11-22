// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Core;

namespace FreeLibSet.Data.SqlClient
{
  /// <summary>
  /// Форматизатор SQL-выражений для Microsoft SQL Server
  /// </summary>
  public class SqlDBxSqlFormatter : BaseDBxSqlFormatter
  {
    #region Конструктор

    /// <summary>
    /// Создает форматизатор
    /// </summary>
    /// <param name="db">База данных</param>
    public SqlDBxSqlFormatter(SqlDBx db)
    {
      _DB = db;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к базе данных нужен, чтобы отличать версии MS SQL Server.
    /// </summary>
    private SqlDBx _DB;

    #endregion

    #region Имена таблиц и полей

    /// <summary>
    /// Имена заключаются в квадратные скобки, а не в кавычки
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode { get { return EnvelopMode.Brackets; } }

    #endregion

    #region Выражения

    /// <summary>
    /// Возвращает имя функции.
    /// Для провайдеров БД от Microsoft возвращает "ISNULL" вместо "COALESCE".
    /// </summary>
    /// <param name="function">Вид функции</param>
    /// <returns>Нестандартное имя функции</returns>
    protected override string GetFunctionName(DBxFunctionKind function)
    {
      switch (function)
      {
        case DBxFunctionKind.Coalesce: return "ISNULL";
        default:
          return base.GetFunctionName(function);
      }
    }

    #endregion

    #region Типы данных

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Описание столбца</param>
    protected override void OnFormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
    {
      switch (column.ColumnType)
      {
        case DBxColumnType.String:
          buffer.SB.Append("NCHAR(");
          buffer.SB.Append(column.MaxLength);
          buffer.SB.Append(")");
          break;

        case DBxColumnType.Boolean:
          buffer.SB.Append("BIT");
          break;

        case DBxColumnType.Int:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("INT");
          else if (column.MinValue >= 0 && column.MaxValue <= 255)
            buffer.SB.Append("TINYINT");
          else if (column.MinValue >= Int16.MinValue && column.MaxValue <= Int16.MaxValue)
            buffer.SB.Append("SMALLINT");
          else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
            buffer.SB.Append("INT");
          else
            buffer.SB.Append("BIGINT");
          break;

        case DBxColumnType.Float:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("FLOAT");
          else if (column.MinValue >= Single.MinValue && column.MaxValue <= Single.MaxValue)
            buffer.SB.Append("REAL");
          else
            buffer.SB.Append("FLOAT");
          break;

        case DBxColumnType.Money:
          buffer.SB.Append("MONEY");
          break;

        case DBxColumnType.Date:
          // Только дата
          if (_DB.IsSqlServer2008orNewer)
            buffer.SB.Append("DATE");
          else
            buffer.SB.Append("DATETIME");
          break;

        case DBxColumnType.DateTime:
          // Дата и время
          if (_DB.IsSqlServer2008orNewer)
            buffer.SB.Append("DATETIME2(0)");
          else
            buffer.SB.Append("DATETIME");
          break;

        case DBxColumnType.Time:
          // Только время
          if (_DB.IsSqlServer2008orNewer)
            buffer.SB.Append("TIME(0)");
          else
            buffer.SB.Append("DATETIME");
          break;

        case DBxColumnType.Memo:
          buffer.SB.Append("NVARCHAR(MAX)");
          break;

        case DBxColumnType.Xml:
          buffer.SB.Append("XML");
          break;

        case DBxColumnType.Binary:
          buffer.SB.Append("VARBINARY(MAX)");
          break;

        case DBxColumnType.Guid:
          buffer.SB.Append("UNIQUEIDENTIFIER"); // 28.04.2020
          break;

        default:
          throw new BugException("Неизвестный тип поля " + column.ColumnType.ToString());
      }
    }

    #endregion

    #region Форматирование значений

    /// <summary>
    /// Логические значения записываются как строки
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="value">Значение</param>
    protected override void OnFormatBooleanValue(DBxSqlBuffer buffer, bool value)
    {
      buffer.SB.Append(value ? '1' : '0');
    }

    /// <summary>
    /// Преобразование значения даты и/или времени.
    /// Этот метод вызывается из OnFormatValue().
    /// </summary>
    /// <param name="buffer">Буфер для записи значения</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="useDate">если true, то должен быть записан компонент даты</param>
    /// <param name="useTime">если true, то должен быть записан компонент времени</param>
    protected override void OnFormatDateTimeValue(DBxSqlBuffer buffer, DateTime value, bool useDate, bool useTime)
    {
      // Даты задаются в формате "'YYYYMMDD'"
      // Дата и время задается в формате "'YYYYMMDD HH:MM:SS.SSS'"
      buffer.SB.Append("'");
      if (useDate)
        buffer.SB.Append(value.ToString("yyyyMMdd", CultureInfo.InvariantCulture));

      if (useTime)
      {
        if (useDate)
          buffer.SB.Append(' ');
        buffer.SB.Append(value.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture));
      }
      buffer.SB.Append("'");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="columnType"></param>
    /// <returns></returns>
    protected override object OnPrepareParamValue(object value, DBxColumnType columnType)
    {
      if (columnType == DBxColumnType.Guid)
      {
        if (value is String)
          return new Guid((string)value);
        if (value is byte[])
          return new Guid((byte[])value);
      }

      return value;
    }

    #endregion

    #region FormatFilter

    #endregion

    #region FormatOrder

    // TODO: ?
    ///// <summary>
    ///// Форматирование элемента порядка сортировки.
    ///// Вызывает виртуальные методы для записи конкретных элементов сортировки.
    ///// Метод не должен добавлять суффиксы "ASC" и "DESC".
    ///// Обычно этот метод не переопределяется
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="OrderItem">Элемент порядка сортировки</param>
    //protected override void OnFormatOrderColumnIfNull(DBxSqlBuffer Buffer, DBxOrderColumnIfNull OrderItem)
    //{
    //  Buffer.SB.Append("IF(");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append(" IS NULL, ");
    //  Buffer.FormatOrderItem(OrderItem.IfNull);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append(")");
    //}

    #endregion

    #region Параметры запроса

    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра, "@P1",
    /// "@P2",... 
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...).</param>
    protected override void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      buffer.SB.Append("@P");
      // buffer.SB.Append(paramIndex + 1).ToString();
      buffer.SB.Append((paramIndex + 1).ToString()); // 28.12.2020
    }

    #endregion
  }
}
