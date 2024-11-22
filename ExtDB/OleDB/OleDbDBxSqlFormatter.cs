// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#if !MONO

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Data.OleDb
{
  /// <summary>
  /// Форматизатор SQL-запросов для MS Jet OLE DB Provider
  /// </summary>
  public class OleDbDBxSqlFormatter : BaseDBxSqlFormatter
  {
    #region Имена таблиц и полей

    /// <summary>
    /// Имена полей должны заключаться в квадратные скобки
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode { get { return EnvelopMode.Brackets; } }

    #endregion

    #region FormatValue

    /// <summary>
    /// Для OLE DB строка берется в кавычки. Кавычки внутри строки удваиваются.
    /// </summary>
    /// <param name="buffer">Буфер для форматирования SQL-запроса</param>
    /// <param name="value">Строковая константа</param>
    protected override void OnFormatStringValue(DBxSqlBuffer buffer, string value)
    {
      buffer.SB.Append('\"');
      buffer.SB.Append(((string)value).Replace("\"", "\"\""));
      buffer.SB.Append('\"');
    }

    #endregion

    #region Выражения и функции

    // Не требуется, т.к. подстановка вызова функции Coalesсe выполняется базовым классом
    //protected override void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo)
    //{
    //  base.OnFormatColumn(buffer, column, formatInfo);
    //}

    /// <summary>
    /// Нестандартные имена функций для OleDb
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    protected override string GetFunctionName(DBxFunctionKind function)
    {
      switch (function)
      {
        case DBxFunctionKind.Substring:
          return "MID";

        case DBxFunctionKind.Upper:
          return "UCASE";
        case DBxFunctionKind.Lower:
          return "LCASE";

        // Функции "NZ" нет в OleDB, хотя она доступна из самого Access
        //case DBxFunctionKind.Coalesce:
        //  return "NZ"; // 01.06.2023

        default:
          return base.GetFunctionName(function);
      }
    }

    /// <summary>
    /// Специальная реализация для <see cref="DBxFunctionKind.Coalesce"/>. Вместо функции "COALESCE(A,B)", 
    /// которой нет в OleDB для Access, используется "IIF(ISNULL(A),B,A)".
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="function"></param>
    /// <param name="formatInfo"></param>
    protected override void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      if (function.Function == DBxFunctionKind.Coalesce)
        FormatFunctionCoalesce(buffer, function, formatInfo);
      else
        base.OnFormatFunction(buffer, function, formatInfo);
    }

    private void FormatFunctionCoalesce(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      int currPos = 0;

      DBxFormatExpressionInfo formatInfo2 = new DBxFormatExpressionInfo();
      formatInfo2.WantedColumnType = formatInfo.WantedColumnType;
      formatInfo2.NullAsDefaultValue = false;
      formatInfo2.NoParentheses = true;

      DoFormatFunctionCoalesce(buffer, function, formatInfo2, currPos);
    }

    private void DoFormatFunctionCoalesce(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo, int currPos)
    {
      buffer.SB.Append("IIF(ISNULL(");
      buffer.FormatExpression(function.Arguments[currPos], formatInfo);
      buffer.SB.Append("),");
      if (currPos == function.Arguments.Length - 2)
        buffer.FormatExpression(function.Arguments[currPos + 1], formatInfo);
      else
        DoFormatFunctionCoalesce(buffer, function, formatInfo, currPos + 1); // рекурсивный вызов
      buffer.SB.Append(",");
      buffer.FormatExpression(function.Arguments[currPos], formatInfo);
      buffer.SB.Append(")");
    }

    #endregion

    #region FormatFilter

    ///// <summary>
    ///// Форматирование фильтра.
    ///// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    ///// </summary>
    ///// <param name="buffer">Буфер для записи</param>
    ///// <param name="filter">Фильтр</param>
    //protected override void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    //{
    //  // TODO: ??? Требуется специальное форматирование при наличии значений 0 и false, т.к. могут быть NULL'ы

    //  base.OnFormatValuesFilter(buffer, filter);
    //}


    /// <summary>
    /// Запись фильтра CompareFilter в режиме сравнения значения с NULL.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="expression">Выражение, которое надо сравнить с NULL (левая часть условия)</param>
    /// <param name="columnType">Тип данных</param>
    /// <param name="kind">Режим сравнения: Equal или NotEqual</param>
    protected override void OnFormatNullNotNullCompareFilter(DBxSqlBuffer buffer, DBxExpression expression, DBxColumnType columnType, CompareKind kind)
    {
      if (kind == CompareKind.NotEqual)
        buffer.SB.Append("NOT ");
      buffer.SB.Append("ISNULL(");
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = false;
      formatInfo.WantedColumnType = columnType;
      formatInfo.NoParentheses = true;
      buffer.FormatExpression(expression, formatInfo);
      buffer.SB.Append(')');
    }

    /// <summary>
    /// Реализация с помощью функции STRCOMP(), когда чувствительность к регистру имеет значение
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="filter"></param>
    protected override void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter)
    {
      // 02.06.2023
      // Обычное сравнение строк "=" является нечувствительным к регистру
      //bool nullAsDefaultValue = filter.Value.Length == 0;
      bool nullAsDefaultValue = true; // Перед фильтром может быть NOT и тогда не попадут строки с NULL

      if (StringIsCaseSensitive(filter.Value))
      {
        buffer.SB.Append("STRCOMP(");
        DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
        formatInfo.WantedColumnType = DBxColumnType.String;
        formatInfo.NullAsDefaultValue = nullAsDefaultValue;
        formatInfo.NoParentheses = true;
        FormatExpression(buffer, filter.Expression, formatInfo);
        buffer.SB.Append(",");
        OnFormatStringValue(buffer, filter.Value);
        buffer.SB.Append(",");
        if (filter.IgnoreCase)
          buffer.SB.Append("1"); // vbTextCompare
        else
          buffer.SB.Append("0"); // vbBinaryCompare
        buffer.SB.Append(")=0");
      }
      else
      {
        // Используем обычный фильтр "="
        CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.Value), CompareKind.Equal, nullAsDefaultValue);
        buffer.FormatFilter(filter2);
      }
    }

    /// <summary>
    /// Использование функций STRCOMP() и MID()
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="filter"></param>
    protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
    {
      buffer.SB.Append("STRCOMP(MID(");
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NullAsDefaultValue = true; 
      formatInfo.NoParentheses = true;
      FormatExpression(buffer, filter.Expression, formatInfo);
      buffer.SB.Append(",1,");
      buffer.SB.Append(StdConvert.ToString(filter.Value.Length));
      buffer.SB.Append("),");
      FormatValue(buffer, filter.Value, DBxColumnType.String);
      buffer.SB.Append(",");
      if (filter.IgnoreCase)
        buffer.SB.Append("1"); // vbTextCompare
      else
        buffer.SB.Append("0"); // vbBinaryCompare
      buffer.SB.Append(")=0");
    }

    /// <summary>
    /// Использование функций STRCOMP() и MID()
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="filter"></param>
    protected override void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter)
    {
      buffer.SB.Append("STRCOMP(MID(");
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NullAsDefaultValue = true; 
      formatInfo.NoParentheses = true;
      FormatExpression(buffer, filter.Expression, formatInfo);
      buffer.SB.Append(",");
      buffer.SB.Append(StdConvert.ToString(filter.StartIndex + 1));
      buffer.SB.Append(",");
      buffer.SB.Append(StdConvert.ToString(filter.Value.Length));
      buffer.SB.Append("),");
      FormatValue(buffer, filter.Value, DBxColumnType.String);
      buffer.SB.Append(",");
      if (filter.IgnoreCase)
        buffer.SB.Append("1"); // vbTextCompare
      else
        buffer.SB.Append("0"); // vbBinaryCompare
      buffer.SB.Append(")=0");
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    /// <param name="filter">Фильтр, для которого определяется необходимость окружить его скобками</param>
    /// <returns>Необходимость в скобках</returns>
    protected override bool FilterNeedsParentheses(DBxFilter filter)
    {
      if (filter is StringValueFilter || filter is StartsWithFilter || filter is SubstringFilter)
        return true;
      else
        return base.FilterNeedsParentheses(filter);
    }

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
    //  Buffer.SB.Append("IIF(ISNULL(");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append("), ");
    //  Buffer.FormatOrderItem(OrderItem.IfNull);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append(")");
    //}

    #endregion

    #region Параметры запроса

    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра "[?]"
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Игнорируется, так как все параметры обозначаются одинаково</param>
    protected override void FormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      buffer.SB.Append("[?]");
    }

    #endregion
  }
}
#endif // !MONO
