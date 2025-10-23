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
    private readonly SqlDBx _DB;

    #endregion

    #region Имена таблиц и полей

    /// <summary>
    /// Имена заключаются в квадратные скобки, а не в кавычки
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode { get { return EnvelopMode.Brackets; } }

    #endregion

    #region Функции

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
        case DBxFunctionKind.Coalesce:
          if (_DB.IsSqlServer2008R2orNewer) // м.б., 2008
            return "COALESCE";
          else
            return "ISNULL";
        default:
          return base.GetFunctionName(function);
      }
    }

    /// <summary>
    /// Спциальная реализация для SQL Server 2005
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="function"></param>
    /// <param name="formatInfo"></param>
    protected override void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      // SQL Server 2005-2008R2 (и новее?) не поддерживает операции вида SELECT [Col1]>=0 ...
      // Требуется оформлять их в виде SELECT CASE WHEN [Col1]>=0 THEN CONVERT(bit,1) WHEN (Col1)<0 THEN CONVERT(bit,0) END ...
      if (DBxTools.IsComparision(function.Function))
      {
        FormatOperatorCompare2005(buffer, function, formatInfo);
        return;
      }

      if (function.Function == DBxFunctionKind.IIf)
      {
        base.FormatIIfFunctionAsCaseOperator(buffer, function, formatInfo, true);
        return;
      }
      if (function.Function == DBxFunctionKind.Coalesce && 
        (!_DB.IsSqlServer2008R2orNewer))
      {
        base.FormatFunctionCoalesceAsIsNullWith2args(buffer, function, formatInfo);
        return;
      }

      base.OnFormatFunction(buffer, function, formatInfo);
    }

    private void FormatOperatorCompare2005(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      switch (function.Function)
      {
        case DBxFunctionKind.Equal:
          DoFormatOperatorCompare2005(buffer, function, formatInfo, "=", "<>");
          break;
        case DBxFunctionKind.LessThan:
          DoFormatOperatorCompare2005(buffer, function, formatInfo, "<", ">=");
          break;
        case DBxFunctionKind.LessOrEqualThan:
          DoFormatOperatorCompare2005(buffer, function, formatInfo, "<=", ">");
          break;
        case DBxFunctionKind.GreaterThan:
          DoFormatOperatorCompare2005(buffer, function, formatInfo, ">", "<=");
          break;
        case DBxFunctionKind.GreaterOrEqualThan:
          DoFormatOperatorCompare2005(buffer, function, formatInfo, ">=", "<");
          break;
        case DBxFunctionKind.NotEqual:
          DoFormatOperatorCompare2005(buffer, function, formatInfo, "<>", "=");
          break;
        default:
          throw new BugException();
      }
    }


    private void DoFormatOperatorCompare2005(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo, string signTrue, string signFalse)
    {
      formatInfo.NoParentheses = false;
      if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
        formatInfo.WantedColumnType = DBxColumnType.Int32;

      buffer.SB.Append("CASE WHEN ");
      buffer.FormatExpression(function.Arguments[0], formatInfo);
      buffer.SB.Append(signTrue);
      buffer.FormatExpression(function.Arguments[1], formatInfo);
      buffer.SB.Append(" THEN CONVERT(bit, 1) WHEN ");
      buffer.FormatExpression(function.Arguments[0], formatInfo);
      buffer.SB.Append(signFalse);
      buffer.FormatExpression(function.Arguments[1], formatInfo);
      buffer.SB.Append(" THEN CONVERT(bit, 0) END");
    }


    /// <summary>
    /// Форматирование выражения-функции или математической операции.
    /// Специальная реализация функции AVG() для целочисленного столбца.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="function">Выражение - функция</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected override void OnFormatAggregateFunction(DBxSqlBuffer buffer, DBxAggregateFunction function, DBxFormatExpressionInfo formatInfo)
    {
      // 31.05.2023
      // Если функция AVG() вызвана для целочисленного столбца, то среднее возвращается тоже как целое число.
      // Надо добавить вызов CONVERT() для получения правильного результата.
      // NULL'ы обрабатываются правильно.
      if (function.Function == DBxAggregateFunctionKind.Avg && IsIntegerExpression(function, buffer))
      {
        buffer.SB.Append("AVG(CONVERT(FLOAT,");
        buffer.FormatExpression(function.Argument, new DBxFormatExpressionInfo());
        buffer.SB.Append("))");
        return;
      }

      base.OnFormatAggregateFunction(buffer, function, formatInfo);
    }

    private static bool IsIntegerExpression(DBxExpression expression, DBxSqlBuffer buffer)
    {
      List<DBxExpression> list = new List<DBxExpression>();
      expression.GetAllExpressions(list);

      bool hasInteger = false;
      bool hasNotInteger = false;
      foreach (DBxExpression expr2 in list)
      {
        DBxColumn col = expr2 as DBxColumn;
        if (col != null)
        {
          DBxColumnStruct colStr;
          if (buffer.ColumnStructs.TryGetValue(col.ColumnName, out colStr))
          {
            if (DBxTools.IsIntegerType(colStr.ColumnType))
              hasInteger = true;
            else
              hasNotInteger = true;
          }
        }
        DBxConst cnst = expr2 as DBxConst;
        if (cnst != null)
        {
          if (DBxTools.IsIntegerType(cnst.ColumnType))
            hasInteger = true;
          else
            hasNotInteger = true;
        }
      }

      return hasInteger && (!hasNotInteger);
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
    protected override void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
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

        case DBxColumnType.Double:
          buffer.SB.Append("FLOAT");
          break;

        case DBxColumnType.Decimal:
          buffer.SB.Append("DECIMAL");
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
          base.FormatValueType(buffer, column);
          break;
      }
    }

    #endregion

    #region Форматирование значений

    /// <summary>
    /// Для значений <see cref="DateTime"/> возвращает 01.01.1753.
    /// </summary>
    /// <param name="columnType"></param>
    /// <returns></returns>
    protected override object GetDefaultValue(DBxColumnType columnType)
    {
      switch (columnType)
      {
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
          return SqlDBx.MinDateTimeSqlServer2005; // 04.06.2023. Для совместимости с сервером MS SQL Server 2005
        default:
          return base.GetDefaultValue(columnType);
      }
    }

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

    #region Строковые фильтры

    /// <summary>
    /// Форматирование фильтра Выражение=СтрокоеЗначение.
    /// Добавляет справа от выражения оператор COLLATE, чтобы обеспечить правильный учет или игнорирование регистра
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;

      buffer.FormatExpression(filter.Expression, formatInfo);
      if (StringIsCaseSensitive(filter.Value))
      {
        if (filter.IgnoreCase)
          buffer.SB.Append(" COLLATE Latin1_General_CI_AS");
        else
          buffer.SB.Append(" COLLATE Latin1_General_CS_AS");
      }
      buffer.SB.Append("=");
      buffer.FormatValue(filter.Value, DBxColumnType.String);
    }

    /// <summary>
    /// Форматирование фильтра Выражение LIKE Шаблон%.
    /// Добавляет справа от выражения оператор COLLATE, чтобы обеспечить правильный учет или игнорирование регистра
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NoParentheses = false;
      buffer.FormatExpression(filter.Expression, formatInfo);
      if (StringIsCaseSensitive(filter.Value))
      {
        if (filter.IgnoreCase)
          buffer.SB.Append(" COLLATE Latin1_General_CI_AS");
        else
          buffer.SB.Append(" COLLATE Latin1_General_CS_AS");
      }
      buffer.SB.Append(" LIKE '");

      MakeEscapedChars(buffer, filter.Value, new char[] { '%', '_', '[', '\'' }, "[", "]");
      buffer.SB.Append("%\'");
    }

    /// <summary>
    /// Форматирование фильтра SUBSTRING(Выражение,Позиция,Длина)=СтроковоеВыражение.
    /// Добавляет справа от выражения оператор COLLATE, чтобы обеспечить правильный учет или игнорирование регистра
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter)
    {
      DBxExpression expr1 = new DBxFunction(DBxFunctionKind.Substring,
          filter.Expression,
          new DBxConst(filter.StartIndex + 1),
          new DBxConst(filter.Value.Length));

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;
      buffer.FormatExpression(expr1, formatInfo);
      if (StringIsCaseSensitive(filter.Value))
      {
        if (filter.IgnoreCase)
          buffer.SB.Append(" COLLATE Latin1_General_CI_AS");
        else
          buffer.SB.Append(" COLLATE Latin1_General_CS_AS");
      }
      buffer.SB.Append("=");
      buffer.FormatValue(filter.Value, DBxColumnType.String);
    }

    #endregion

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
  }
}
