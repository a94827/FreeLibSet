// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Форматизатор для объекта <see cref="System.Data.DataView"/>
  /// </summary>
  public sealed class DataViewDBxSqlFormatter : CoreDBxSqlFormatter
  {
    #region Имя таблицы и поля

    /// <summary>
    /// Имена должны заключаться в квадратные скобки, а не кавычки
    /// </summary>
    protected override CoreDBxSqlFormatter.EnvelopMode NameEnvelopMode
    {
      get { return EnvelopMode.Brackets; }
    }

    /// <summary>
    /// Генерирует исключение, так как в выражениях для DataView не могут использоваться альясы таблиц
    /// </summary>
    /// <param name="buffer">Не используется</param>
    /// <param name="tableAlias">Не используется</param>
    /// <param name="columnName">Не используется</param>
    internal protected override void FormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      throw new NotImplementedException("Альясы таблиц не поддерживаются в DataView");
    }

    /// <summary>
    /// Для DataView не используются ссылочные поля.
    /// Поля с точками трактуются как обычные, а не ссылочные поля.
    /// Если свойство <see cref="DBxFormatExpressionInfo.NullAsDefaultValue"/> установлено, то форматируется функция COALESCE(),
    /// иначе вызывается <see cref="DBxSqlFormatter.FormatColumnName(DBxSqlBuffer, string)"/>.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Выражение - имя поля</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected override void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo)
    {
      bool useCoalesce = false;
      DBxColumnType wantedType = formatInfo.WantedColumnType;
      if (formatInfo.NullAsDefaultValue)
      {
        DBxColumnStruct colStr;
        buffer.ColumnStructs.TryGetValue(column.ColumnName, out colStr);
        if (colStr != null)
        {
          useCoalesce = colStr.Nullable;
          wantedType = colStr.ColumnType;
        }
        else
          useCoalesce = true;
      }

      if (useCoalesce)
      {
        if (wantedType == DBxColumnType.Unknown)
          throw new InvalidOperationException("Для столбца \"" + column.ColumnName + "\" требуется обработка значения NULL. Не найдено описание структуры столбца и не передан требуемый тип данных");

        DBxFunction f2 = new DBxFunction(DBxFunctionKind.Coalesce, column, new DBxConst(GetDefaultValue(wantedType), wantedType));
        FormatExpression(buffer, f2, new DBxFormatExpressionInfo()); // рекурсивный вызов форматировщика
      }
      else
        FormatColumnName(buffer, column.ColumnName);
    }

    #endregion

    #region Типы данных

    /// <summary>
    /// Специальное форматирование для <see cref="TimeSpan"/> с использованием функции "CONVERT()"
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value"></param>
    /// <param name="useDate"></param>
    /// <param name="useTime"></param>
    protected override void OnFormatDateTimeValue(DBxSqlBuffer buffer, DateTime value, bool useDate, bool useTime)
    {
      if ((!useDate) & useTime)
      {
        // 24.05.2023. Отдельно TimeSpan нельзя форматировать как DateTime, то есть нельзя использовать "[MyCol]=#12:34:56#" 
        // Требуется использовать функцию CONVERT(). Она для преобразование строки в TimeSpan использует метод System.Xml.XmlConvert.ToTimeSpan().

        buffer.SB.Append("CONVERT(\'");
        buffer.SB.Append(System.Xml.XmlConvert.ToString(value.TimeOfDay));
        buffer.SB.Append("\',System.TimeSpan)");
        return;
      }
      base.OnFormatDateTimeValue(buffer, value, useDate, useTime);
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
      switch (function)
      {
        case DBxFunctionKind.Coalesce: return "ISNULL";
        default:
          return base.GetFunctionName(function);
      }
    }

    /// <summary>
    /// Специальная реализация функции ABS(), которой нет в <see cref="System.Data.DataView"/> через IIF().
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="function">Выражение - функция</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected override void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      switch (function.Function)
      {
        case DBxFunctionKind.Coalesce:
          // 02.06.2023
          // В отличие от функции COALESCE в базах данных, функция ISNULL в DataView поддерживает только два аргумента
          FormatFunctionCoalesceAsIsNullWith2args(buffer, function, formatInfo);
          break;
        case DBxFunctionKind.Abs:
          FormatFunctionAbs(buffer, function, formatInfo);
          break;
        default:
          base.OnFormatFunction(buffer, function, formatInfo);
          break;
      }
    }

    private void FormatFunctionAbs(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      // 23.05.2023
      buffer.SB.Append("IIF(");
      this.FormatExpression(buffer, function.Arguments[0], formatInfo);
      buffer.SB.Append(">=0,");
      this.FormatExpression(buffer, function.Arguments[0], formatInfo);
      buffer.SB.Append(",");
      DBxFunction fnNeg = new DBxFunction(DBxFunctionKind.Neg, function.Arguments[0]);
      OnFormatFunction(buffer, fnNeg, formatInfo);
      buffer.SB.Append(")");
    }

    /// <summary>
    /// Выбрасывает исключение, так как <see cref="System.Data.DataView"/> не поддерживает агрегатные функции.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="function"></param>
    /// <param name="formatInfo"></param>
    protected override void OnFormatAggregateFunction(DBxSqlBuffer buffer, DBxAggregateFunction function, DBxFormatExpressionInfo formatInfo)
    {
      throw new NotSupportedException("Для DataView не поддерживаются агрегатные функции");
    }

    #endregion

    #region Типы данных

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="buffer">Не используется</param>
    /// <param name="column">Не используется</param>
    internal protected override void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
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
    /// Специальная обработка для <see cref="TimeSpan"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="filter"></param>
    protected override void OnFormatCompareFilter(DBxSqlBuffer buffer, CompareFilter filter)
    {
      if (filter.ColumnTypeInternal == DBxColumnType.Time && (!filter.ComparisionToNull))
      {
        switch (filter.Kind)
        {
          case CompareKind.Equal:
          case CompareKind.NotEqual:
            DoFormatTimeSpanCompareFilter(buffer, filter);
            return;

            //default:
            // Не выбрасываем исключение, т.к. этот метод используется в DBxFilter.ToString()
            //  throw new NotSupportedException("В DataTable для TimeSpan не поддерживаются операции сравнения больше/меньше. Можно использовать только сравнение на равенство");
        }
      }
      base.OnFormatCompareFilter(buffer, filter);
    }

    private void DoFormatTimeSpanCompareFilter(DBxSqlBuffer buffer, CompareFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.WantedColumnType = DBxColumnType.Time;
      formatInfo.NullAsDefaultValue = filter.NullAsDefaultValue;

      DBxConst cnst1 = filter.Expression1.GetConst();
      if (cnst1 != null)
      {
        TimeSpan v = (TimeSpan)DBxTools.Convert(cnst1.Value, DBxColumnType.Time);
        string s = System.Xml.XmlConvert.ToString(v);
        OnFormatStringValue(buffer, s);
      }
      else
      {
        buffer.SB.Append("CONVERT(");
        buffer.FormatExpression(filter.Expression1, formatInfo);
        buffer.SB.Append(",System.String)");
      }

      buffer.SB.Append(GetSignStr(filter.Kind));

      DBxConst cnst2 = filter.Expression2.GetConst();
      if (cnst2 != null)
      {
        TimeSpan v = (TimeSpan)DBxTools.Convert(cnst2.Value, DBxColumnType.Time);
        string s = System.Xml.XmlConvert.ToString(v);
        OnFormatStringValue(buffer, s);
      }
      else
      {
        buffer.SB.Append("CONVERT(");
        buffer.FormatExpression(filter.Expression2, formatInfo);
        buffer.SB.Append(",System.String");
      }
    }

    /// <summary>
    /// Запись фильтра <see cref="CompareFilter"/> в режиме сравнения значения с NULL.
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
      buffer.FormatValue(GetDefaultValue(columnType), DBxColumnType.Unknown);
      buffer.SB.Append(")=");
      buffer.FormatValue(GetDefaultValue(columnType), DBxColumnType.Unknown);
    }

    /// <summary>
    /// Специальная реализация для типов <see cref="TimeSpan"/> и <see cref="Guid"/>.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="filter"></param>
    protected override void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      if (filter.ColumnTypeInternal == DBxColumnType.Guid && filter.Values.Length > 1)
      {
        DoFormatGuidValuesFilter(buffer, filter);
        return;
      }
      if (filter.ColumnTypeInternal == DBxColumnType.Time && filter.Values.Length > 1)
      {
        DoFormatTimeSpanValuesFilter(buffer, filter);
        return;
      }
      base.OnFormatValuesFilter(buffer, filter);
    }

    private void DoFormatGuidValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = filter.NullAsDefaultValue;
      formatInfo.WantedColumnType = DBxColumnType.Guid;

      buffer.SB.Append("CONVERT(");
      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append(",System.String) IN (");
      for (int i = 0; i < filter.Values.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");

        Guid v = (Guid)DBxTools.Convert(filter.Values.GetValue(i), DBxColumnType.Guid);
        OnFormatGuidValue(buffer, v);
      }
      buffer.SB.Append(')');
    }

    private void DoFormatTimeSpanValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = filter.NullAsDefaultValue;
      formatInfo.WantedColumnType = DBxColumnType.Time;

      buffer.SB.Append("CONVERT(");
      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append(",System.String) IN (");
      for (int i = 0; i < filter.Values.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");

        TimeSpan v = (TimeSpan)DBxTools.Convert(filter.Values.GetValue(i), DBxColumnType.Time);
        string s = System.Xml.XmlConvert.ToString(v);
        OnFormatStringValue(buffer, s);
      }
      buffer.SB.Append(')');
    }

    #endregion

    #region Строковые фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// DataView игнорирует или учитывает регистр, в зависимости от настроек <see cref="System.Data.DataTable.CaseSensitive"/>.
    /// Поэтому, здесь нельзя учитывать свойство <see cref="StringValueFilter.IgnoreCase"/>.
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
    /// Автономная версия этого метода есть в ExtTools.dll. Метод <see cref="DataTools.GetDataViewLikeExpressionString(string)"/>.
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
    internal protected override void FormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      throw new NotImplementedException("DataView не поддерживает именованные параметры SQL-запросов, т.к. не содержит методов, выполняющих запросы");
      //Buffer.SB.Append("[?]");
    }

    #endregion
  }
}
