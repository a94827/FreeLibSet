using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

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

namespace FreeLibSet.Data
{
  /// <summary>
  /// Форматизатор для объекта System.Data.DataView
  /// </summary>
  public sealed class DataViewDBxSqlFormatter : BaseDBxSqlFormatter
  {
    #region Имя таблицы и поля

    /// <summary>
    /// Имена должны заключаться в квадратные скобки, а не кавычки
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode
    {
      get { return EnvelopMode.Brackets; }
    }

    /// <summary>
    /// Генерирует исключение, так как в выражениях для DataView не могут использоваться альясы таблиц
    /// </summary>
    /// <param name="buffer">Не используется</param>
    /// <param name="tableAlias">Не используется</param>
    /// <param name="columnName">Не используется</param>
    protected override void OnFormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      throw new NotImplementedException("Альясы таблиц не поддерживаются в DataView");
    }

    /// <summary>
    /// Для DataView не используются ссылочные поля.
    /// Поля с точками трактуются как обычные, а не ссылочные поля.
    /// Если свойство DBxFormatExpressionInfo.NullAsDefaultValue установлено, то форматируется функция COALESCE(),
    /// иначе вызывается OnFormatColumnName()
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Выражение - имя поля</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected override void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo)
    {
      if (formatInfo.NullAsDefaultValue)
      {
        DBxFunction f2 = new DBxFunction(DBxFunctionKind.Coalesce, column, new DBxConst(DBxTools.GetDefaultValue(formatInfo.WantedColumnType), formatInfo.WantedColumnType));
        OnFormatExpression(buffer, f2, new DBxFormatExpressionInfo()); // рекурсивный вызов форматировщика
      }
      else
        OnFormatColumnName(buffer, column.ColumnName);
    }


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
      switch(function)
      {
        case DBxFunctionKind.Coalesce: return "ISNULL"; 
        default:
          return base.GetFunctionName(function);
      } 
    }

    /// <summary>
    /// Выбрасывает исключения
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="function"></param>
    /// <param name="formatInfo"></param>
    protected override void OnFormatAgregateFunction(DBxSqlBuffer buffer, DBxAgregateFunction function, DBxFormatExpressionInfo formatInfo)
    {
      throw new NotSupportedException("Для DataView не поддерживаются агрегатные функции");
    }

    #endregion

    #region Типы данных

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="Buffer">Не используется</param>
    /// <param name="Column">Не используется</param>
    protected override void OnFormatValueType(DBxSqlBuffer Buffer, DBxColumnStruct Column)
    {
      throw new NotImplementedException("DataView не имеет реализации DDL");
    }

    #endregion

    #region Фильтры

    /// <summary>
    /// Возвращает false.
    /// DataView не поддерживает инструкцию BETWEEN.
    /// </summary>
    protected override bool BetweenInstructionSupported { get { return false; } }

    #region Прочие фильтры



    /// <summary>
    /// Запись фильтра CompareFilter в режиме сравнения значения с NULL.
    /// Добавляет "NOT ISNULL(Выражение, Значение-по-Умолчанию)=Значение-по-Умолчанию"
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
      formatInfo.NullAsDefaultValue = false; // обязательно
      formatInfo.WantedColumnType = columnType;
      formatInfo.NoParentheses = true;
      buffer.FormatExpression(expression, formatInfo);
      buffer.SB.Append(", ");
      buffer.FormatValue(DBxTools.GetDefaultValue(columnType), DBxColumnType.Unknown);
      buffer.SB.Append(")=");
      buffer.FormatValue(DBxTools.GetDefaultValue(columnType), DBxColumnType.Unknown);
    }


    #endregion

    #region Строковые фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// DataView игнорирует или учитывает регистр, в зависимости от настроек DataTable.
    /// Поэтому, здесь нельзя учитывать свойство Filter.IgnoreCase
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter)
    {

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NoParentheses = false; // могут потребоваться скобки

      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append("=");
      buffer.FormatValue(filter.Value, DBxColumnType.String);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Автономная версия этого метода есть в ExtTools.dll. Метод DataTools.GetDataViewLikeExpressionString()
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
    {

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NoParentheses = false; // могут потребоваться скобки
      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append(" LIKE \'");
      string s = filter.Value;
      s = s.Replace("\'", "\"\""); // 21.08.2017
      MakeEscapedChars(buffer, s, new char[] { '*', '%', '[', ']' }, "[", "]");
      buffer.SB.Append("*\'");
    }


    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NoParentheses = true; // можно без скобок

      buffer.SB.Append("SUBSTRING(");
      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append(',');
      buffer.SB.Append(filter.StartIndex + 1);
      buffer.SB.Append(',');
      buffer.SB.Append(filter.Value.Length);
      buffer.SB.Append(") = ");
      buffer.FormatValue(filter.Value, DBxColumnType.Unknown);
    }

    #endregion

    #region Прочие фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatInSelectFilter(DBxSqlBuffer buffer, InSelectFilter filter)
    {
      throw new NotImplementedException("DataView не поддерживает фильтр IN с подзапросом");
    }

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatDummyFilter(DBxSqlBuffer buffer, DummyFilter filter)
    {
      if (filter.IsTrue)
        buffer.SB.Append(@"'1'='1'");
      else
        buffer.SB.Append(@"'1'='0'");
    }

    #endregion

    #endregion

    #region Порядок сортировки

    // TODO: ?

    ///// <summary>
    ///// Форматирование элемента порядка сортировки.
    ///// Вызывает <paramref name="Buffer"/>.FormatColumnName()
    ///// Метод не добавляет суффиксы "ASC" и "DESC".
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="OrderItem">Элемент порядка сортировки</param>
    //protected override void OnFormatOrderColumn(DBxSqlBuffer Buffer, DBxOrderColumn OrderItem)
    //{
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //}

    // TODO:?

    ///// <summary>
    ///// Форматирование элемента порядка сортировки.
    ///// Метод не добавляет суффиксы "ASC" и "DESC".
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="OrderItem">Элемент порядка сортировки</param>
    //protected override void OnFormatOrderColumnIfNull(DBxSqlBuffer Buffer, DBxOrderColumnIfNull OrderItem)
    //{
    //  Buffer.SB.Append(CoalesceFunctionName);
    //  Buffer.SB.Append("(");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatOrderItem(OrderItem.IfNull);
    //  Buffer.SB.Append(")");
    //}

    #endregion

    #region Параметры SQL-запроса

    /// <summary>
    /// Генерирует исключение, т.к. DataView не поддерживает именованные параметры
    /// </summary>
    /// <param name="buffer">Не используется</param>
    /// <param name="paramIndex">Не используется</param>
    protected override void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      throw new NotImplementedException("DataView не поддерживает именованные параметры SQL-запросов, т.к. не содержит методов, выполняющих запросы");
      //Buffer.SB.Append("[?]");
    }

    #endregion
  }
}
