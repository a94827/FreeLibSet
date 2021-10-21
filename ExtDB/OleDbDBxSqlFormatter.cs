using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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

    #region FormatFilter

    // TODO: ?

//    /// <summary>
//    /// Форматирование фильтра.
//    /// Записывает в <paramref name="Buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
//    /// </summary>
//    /// <param name="Buffer">Буфер для записи</param>
//    /// <param name="Filter">Фильтр</param>
//    protected override void OnFormatValueFilter(DBxSqlBuffer Buffer, ValueFilter Filter)
//    {
//      // Для просто значения null используем функцию IsNull()
//      if (Filter.Value == null || Filter.Value is DBNull)
//      {
//#if DEBUG
//        if (Filter.Kind != ValueFilterKind.Equal)
//          throw new InvalidOperationException("Значение null в фильтре сравнения допускается только для сравнения на равенство");
//#endif

//        Buffer.SB.Append("ISNULL(");
//        Buffer.FormatColumnName(Filter.ColumnName);
//        Buffer.SB.Append(')');
//        return;
//      }

//      if (Filter.Kind == ValueFilterKind.Equal)
//      {
//        Buffer.SB.Append("ISNULL(");
//        Buffer.FormatColumnName(Filter.ColumnName);
//        Buffer.SB.Append(") OR (");
//        Buffer.FormatColumnName(Filter.ColumnName);
//        Buffer.SB.Append(GetSignStr(Filter.Kind));
//        Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
//        Buffer.SB.Append(')');
//        return;
//      }

//      base.OnFormatValueFilter(Buffer, Filter);
//    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      // TODO: Требуется специальное форматирование при наличии значений 0 и false, т.к. могут быть NULL'ы
      throw new NotImplementedException();
      //base.OnFormatValuesFilter(Buffer, Filter);
    }


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

    // Х.З. Не проверял

    ///// <summary>
    ///// Форматирование фильтра.
    ///// Записывает в <paramref name="Buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="Filter">Фильтр</param>
    //protected override void OnFormatSubstringFilter(DBxSqlBuffer Buffer, SubstringFilter Filter)
    //{
    //  base.OnFormatSubstringFilter();
    //  if (Filter.IgnoreCase)
    //    Buffer.SB.Append("UPPER("); // не проверял
    //  Buffer.SB.Append("Mid(");
    //  Buffer.FormatColumnName(Filter.ColumnName);
    //  Buffer.SB.Append(',');
    //  Buffer.SB.Append(Filter.StartIndex + 1);
    //  Buffer.SB.Append(',');
    //  Buffer.SB.Append(Filter.Value.Length);
    //  Buffer.SB.Append(")");
    //  if (Filter.IgnoreCase)
    //    Buffer.SB.Append(")");
    //  Buffer.SB.Append(" = ");

    //  string v = Filter.Value;
    //  if (Filter.IgnoreCase)
    //    v = v.ToUpperInvariant();
    //  Buffer.FormatValue(v, DBxColumnType.String);
    //}

    /// <summary>
    /// Возвращает true
    /// </summary>
    /// <param name="filter">Фильтр, для которого определяется необходимость окружить его скобками</param>
    /// <returns>Необходимость в скобках</returns>
    protected override bool FilterNeedsParentheses(DBxFilter filter)
    {
      // TODO: 
      return true;
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
    protected override void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      buffer.SB.Append("[?]");
    }

    #endregion
  }
}
