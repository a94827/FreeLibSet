// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;

namespace FreeLibSet.Data.Npgsql
{
  /// <summary>
  /// Форматизатор SQL-выражений для PostGreSQL
  /// </summary>
  public class NpgsqlDBxSqlFormatter : BaseDBxSqlFormatter
  {
    #region Имя таблицы и поля

    /// <summary>
    /// Имена должны заключаться в кавычки
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode { get { return EnvelopMode.Quotation; } }

    #endregion

    #region Типы значений

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
          //buffer.SB.Append("CHARACTER(");
          buffer.SB.Append("CHAR("); // 30.12.2019
          buffer.SB.Append(column.MaxLength);
          buffer.SB.Append(")");
          break;
        case DBxColumnType.Memo:
          buffer.SB.Append("TEXT");
          break;
        case DBxColumnType.Binary:
          buffer.SB.Append("BYTEA"); // ???
          break;
        case DBxColumnType.Guid:
          buffer.SB.Append("UUID"); // 25.08.2021
          break;
        default:
          base.OnFormatValueType(buffer, column);
          break;
      }
    }

    #endregion

    #region Значения

    /// <summary>
    /// Форматирование логического значения.
    /// Возвращает строковые значения 
    /// </summary>
    /// <param name="buffer">Буфер для записи значения</param>
    /// <param name="value">Записываемое значение</param>
    protected override void OnFormatBooleanValue(DBxSqlBuffer buffer, bool value)
    {
      buffer.SB.Append(value ? @"'1'" : @"'0'");
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

    #endregion

    #region Функции

    /// <summary>
    /// Определена функция "LENGTH"
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    protected override string GetFunctionName(DBxFunctionKind function)
    {
      switch(function)
      {
        case DBxFunctionKind.Length:
          return "Length"; // 29.05.2023
        default:
          return base.GetFunctionName(function);
      }
    }

    /// <summary>
    /// Вместо функции IIF должен использоваться оператор CASE
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="function"></param>
    /// <param name="formatInfo"></param>
    protected override void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      if (function.Function == DBxFunctionKind.IIf)
        base.FormatIIfFunctionAsCaseOperator(buffer, function, formatInfo, false);
      else
        base.OnFormatFunction(buffer, function, formatInfo);
    }

    /// <summary>
    /// Переопределение для функции IIF
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    protected override bool OnAreParenthesesRequired(DBxExpression expression)
    {
      DBxFunction function = expression as DBxFunction;
      if (function != null)
      {
        if (function.Function == DBxFunctionKind.IIf)
          return true;
      }

      return base.OnAreParenthesesRequired(expression);
    }

    #endregion

    #region FormatFilter

    /// <summary>
    /// Форматирование фильтра Выражение LIKE Шаблон%.
    /// Если требуется сравнение без учета регистра, используется нестандартный оператор ILIKE.
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
      if ( filter.IgnoreCase && StringIsCaseSensitive(filter.Value))
        buffer.SB.Append(" ILIKE '");
      else
        buffer.SB.Append(" LIKE '");

      MakeEscapedChars(buffer, filter.Value, new char[] { '%', '_', '\'' }, "\\", "");
      buffer.SB.Append("%\'");
    }


    // TODO: ?

    ///// <summary>
    ///// Заменяем функцию ISNULL(a,b) на COALESCE
    ///// </summary>
    ///// <param name="Buffer"></param>
    ///// <param name="Filter"></param>
    //protected override void OnFormatValueFilter(DBxSqlBuffer Buffer, ValueFilter Filter)
    //{
    //  // Для просто значения null используем функцию IsNull()
    //  if (Filter.Value == null || Filter.Value is DBNull)
    //  {
    //    if (Filter.Kind != ValueFilterKind.Equal)
    //      throw new InvalidOperationException("Значение NULL в фильтре сравнения допускается только для сравнения на равенство (поле \"" + Filter.ColumnName + "\")");

    //    Buffer.FormatColumnName(Filter.ColumnName);
    //    Buffer.SB.Append(" IS NULL");
    //    return;
    //  }

    //  if (Filter.Kind == ValueFilterKind.Equal)
    //  {
    //    // Для значений 0 и false используем ISNULL() в комбинации со сравнением
    //    if (DataTools.IsEmptyValue(Filter.Value))
    //    {
    //      Buffer.SB.Append("COALESCE(");
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append(',');
    //      Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //      Buffer.SB.Append(")=");
    //      Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //      return;
    //    }
    //  }

    //  Buffer.FormatColumnName(Filter.ColumnName);
    //  Buffer.SB.Append(GetSignStr(Filter.Kind));
    //  Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //}

    // TODO: ?

    ///// <summary>
    ///// Форматирование фильтра
    ///// </summary>
    ///// <param name="Buffer"></param>
    ///// <param name="Filter"></param>
    //protected override void OnFormatValuesFilter(DBxSqlBuffer Buffer, ValuesFilter Filter)
    //{
    //  if (Filter.Values.Length == 1)
    //  {
    //    // Как обычный ValueFilter
    //    object Value = Filter.Values.GetValue(0);
    //    if (DataTools.IsEmptyValue(Value)) // 04.03.2016
    //    {
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append(" IS NULL");
    //    }
    //    else
    //    {
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append('=');
    //      Buffer.FormatValue(Value, DBxColumnType.Unknown);
    //    }
    //    return;
    //  }

    //  // Сложный фильтр использует IN

    //  for (int j = 0; j < Filter.Values.Length; j++)
    //  {
    //    object Value = Filter.Values.GetValue(j);
    //    // Как обычный ValueFilter
    //    if (DataTools.IsEmptyValue(Value)) // 04.03.2016
    //    {
    //      Buffer.SB.Append("COALESCE(");
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append(',');
    //      Buffer.FormatValue(Value, DBxColumnType.Unknown);
    //      Buffer.SB.Append(") IN (");
    //      for (int i = 0; i < Filter.Values.Length; i++)
    //      {
    //        if (i > 0)
    //          Buffer.SB.Append(", ");
    //        Buffer.FormatValue(Filter.Values.GetValue(i), DBxColumnType.Unknown);
    //      }
    //      Buffer.SB.Append(')');

    //      return;
    //    }
    //  }

    //  Buffer.FormatColumnName(Filter.ColumnName);
    //  Buffer.SB.Append(" IN (");
    //  for (int i = 0; i < Filter.Values.Length; i++)
    //  {
    //    if (i > 0)
    //      Buffer.SB.Append(", ");
    //    Buffer.FormatValue(Filter.Values.GetValue(i), DBxColumnType.Unknown);
    //  }
    //  Buffer.SB.Append(')');
    //}

    //protected override void OnFormatDateRangeInclusionFilter(DBxSqlBuffer Buffer, DateRangeInclusionFilter Filter)
    //{
    //  // В текущей реализации компонент времени не поддерживается
    //  // TODO: Надо проверить, что получается
    //  Buffer.SB.Append("(COALESCE(");
    //  Buffer.FormatColumnName(Filter.FirstColumnName);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatValue(DateTime.MinValue, DBxColumnType.Unknown);
    //  Buffer.SB.Append(")<=");
    //  Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);

    //  Buffer.SB.Append(") AND (COALESCE(");
    //  Buffer.FormatColumnName(Filter.LastColumnName);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatValue(DateTime.MaxValue, DBxColumnType.Unknown);
    //  Buffer.SB.Append(")>=");
    //  Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //  Buffer.SB.Append(")");
    //}

    #endregion

    #region FormatOrder

    /// <summary>
    /// Форматирование порядка сортировки.
    /// Добавляет признак NULLS FIRST / NULLS LAST для совместимости с остальными базами данных,
    /// где NULL считается меньше, чем любое другое значение
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="order">Порядок сортировки</param>
    protected override void OnFormatOrder(DBxSqlBuffer buffer, DBxOrder order)
    {
      for (int i = 0; i < order.Parts.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");
        buffer.FormatExpression(order.Parts[i].Expression, new DBxFormatExpressionInfo());

        bool useNulls = true;
        DBxColumn column = order.Parts[i].Expression as DBxColumn;
        if (column != null)
        {
          DBxColumnStruct colStr;
          if (buffer.ColumnStructs.TryGetValue(column.ColumnName, out colStr))
            useNulls = colStr.Nullable;
        }

        if (order.Parts[i].SortOrder == ListSortDirection.Descending)
        {
          buffer.SB.Append(" DESC");
          if (useNulls)
            buffer.SB.Append(" NULLS LAST");
        }
        else
        {
          if (useNulls)
            buffer.SB.Append(" NULLS FIRST");
        }
      }
    }

    #endregion

    #region SELECT

    /// <summary>
    /// Используется форма SELECT ... LIMIT x
    /// </summary>
    public override DBxSelectMaxRecordCountMode SelectMaxRecordCountMode
    { get { return DBxSelectMaxRecordCountMode.Limit; } }

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
      //buffer.SB.Append(paramIndex + 1).ToString();
      buffer.SB.Append((paramIndex + 1).ToString()); // 28.12.2020
    }

    #endregion
  }
}
