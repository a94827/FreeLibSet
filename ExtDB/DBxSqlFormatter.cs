using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.Serialization;
using System.Globalization;
using System.ComponentModel;

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
  /// Опции форматирования выражений
  /// </summary>
  public struct DBxFormatExpressionInfo
  {
    #region Свойства

    /// <summary>
    /// Если true, то при извлечении значения поля, которое может принимать значение NULL, требуется заменять значения NULL 
    /// на значение поля по умолчанию с помощью функции COALESCE() или ISNULL().
    /// Используется фильтрами CompareFilter(), IdsFilter(), ValuesFilter()
    /// </summary>
    public bool NullAsDefaultValue
    {
      get { return _NullAsDefaultValue; }
      set { _NullAsDefaultValue = value; }
    }
    private bool _NullAsDefaultValue;

    /// <summary>
    /// Тип данных, который желательно получить.
    /// Параметр применяется при использовании функции COALESCE(), когда форматировщик не имеет информации о типе поля, например,
    /// в DataViewSqlDbxFormatter
    /// </summary>
    public DBxColumnType WantedColumnType { get { return _WantedColumnType; } set { _WantedColumnType = value; } }
    private DBxColumnType _WantedColumnType;

    /// <summary>
    /// Если false, то выражение будет заключено в круглые скобки, если это необходимо.
    /// Если выражение является DBxFunction, которое реализуется как оператор (Add, Substract, Multiply, Divide),
    /// то оно заключается в скобки. Остальные выражения (поля, константы, вызовы функций) в скобки не заключаются, независимо от значения свойства.
    /// Для определения необходимости использования скобок используется метод DBxSqlFormatter.AreParenthesesRequired().
    /// Если флаг установлен в true, то скобки применяться не будут. Используется, например, при задании аргументов функции
    /// </summary>
    public bool NoParentheses { get { return _NoParentheses; } set { _NoParentheses = value; } }
    private bool _NoParentheses;

    #endregion

    #region Методы

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      if (WantedColumnType!=DBxColumnType.Unknown)
      {
        sb.Append(WantedColumnType.ToString());
        sb.Append(", ");
      }
      if (NullAsDefaultValue)
        sb.Append("NullAsDefaultValue, ");
      if (NoParentheses)
        sb.Append("NoParentheses");
      else
        sb.Append("Parentheses");
      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Форматизатор частей SQL-запроса
  /// Методы данного класса не являются потокобезопасными
  /// Содержит StringBuilder
  /// </summary>
  public sealed class DBxSqlBuffer
  {
    #region Конструкторы

    /// <summary>
    /// Создает буфер форматирования по умолчанию (для объекта DataView).
    /// Используется форматизатор DBxSqlFormatter.Default.
    /// </summary>
    public DBxSqlBuffer()
      : this(DBxSqlFormatter.Default)
    {
    }

    /// <summary>
    /// Создает буфер с указанным форматировщиком
    /// </summary>
    /// <param name="formatter">Форматировщик. Не может быть null</param>
    public DBxSqlBuffer(DBxSqlFormatter formatter)
    {
      if (formatter == null)
        throw new ArgumentNullException("formatter");
      _Formatter = formatter;

      _SB = new StringBuilder();
      _ColumnTableAliases = new Dictionary<string, string>();
      _ColumnStructs = new Dictionary<string, DBxColumnStruct>();
    }

    #endregion

    #region StringBuilder

    /// <summary>
    /// Основной объект буфера для заполнения
    /// </summary>
    public StringBuilder SB { get { return _SB; } }
    private StringBuilder _SB;

    #endregion

    #region Форматизатор

    /// <summary>
    /// Форматизатор.
    /// Задается в конструкторе
    /// </summary>
    public DBxSqlFormatter Formatter { get { return _Formatter; } }
    private DBxSqlFormatter _Formatter;

    #endregion

    #region Информационные таблицы

    /// <summary>
    /// Альясы таблиц для имен полей.
    /// Используются при форматировании выражений DBxColumn.
    /// Ключ - имя поля. Могут быть как составные поля с точками, так и простые поля.
    /// Значение - альяс таблицы.
    /// </summary>
    public IDictionary<string, string> ColumnTableAliases { get { return _ColumnTableAliases; } }
    private Dictionary<string, string> _ColumnTableAliases;

    /// <summary>
    /// Описания столбцов, используемых в запросе.
    /// Нужны для определения необходимости проверки значений на NULL и определения типа данных при вызове COALESCE()
    /// Ключ - имя поля (простого или с точками)
    /// Значение - Описание поля из структуры DBxStruct.
    /// </summary>
    public IDictionary<string, DBxColumnStruct> ColumnStructs { get { return _ColumnStructs; } }
    private Dictionary<string, DBxColumnStruct> _ColumnStructs;

    #endregion

    #region Вызываемые методы

    /// <summary>
    /// Очищает буфер SB и списки ColumnTableAliases, ColumnStructs.
    /// </summary>
    public void Clear()
    {
      _SB.Length = 0;
      _ColumnTableAliases.Clear();
      _ColumnStructs.Clear();
    }

    /// <summary>
    /// Отформатировать имя таблицы. Обычно имя заключается в кавычки или квадратные скобки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public void FormatTableName(string tableName)
    {
      _Formatter.FormatTableName(this, tableName);
    }

    /// <summary>
    /// Отформатировать имя столбца. Обычно имя заключается в кавычки или квадратные скобки
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public void FormatColumnName(string columnName)
    {
      _Formatter.FormatColumnName(this, columnName);
    }

    /// <summary>
    /// Форматирование имени столбца, которому предшествует имя альяса таблицы. Используется в запросах SELECT, в которых есть конструкция JOIN
    /// </summary>
    /// <param name="tableAlias">Альяс имени таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    public void FormatColumnName(string tableAlias, string columnName)
    {
      _Formatter.FormatColumnName(this, tableAlias, columnName);
    }

    /// <summary>
    /// Форматировать тип значения для заданного описания столбца.
    /// Выражение NULL/NOT NULL не добавляется
    /// </summary>
    /// <param name="column">Описание структуры столбца</param>
    public void FormatValueType(DBxColumnStruct column)
    {
      _Formatter.FormatValueType(this, column);
    }


    /// <summary>
    /// Отформатировать значение
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Целевой формат</param>
    public void FormatValue(object value, DBxColumnType columnType)
    {
      _Formatter.FormatValue(this, value, columnType);
    }

    /// <summary>
    /// Форматировать выражение
    /// </summary>
    /// <param name="expression">Выражение. Не может быть null</param>
    /// <param name="formatInfo">Опции форматирования</param>
    public void FormatExpression(DBxExpression expression, DBxFormatExpressionInfo formatInfo)
    {
      _Formatter.FormatExpression(this, expression, formatInfo);
    }

    ///// <summary>
    ///// Форматировать выражение с опциями форматирования по умолчанию
    ///// </summary>
    ///// <param name="expression">Выражение. Не может быть null</param>
    //public void FormatExpression(DBxExpression expression)
    //{
    //  _Formatter.FormatExpression(this, expression, new DBxFormatExpressionInfo());
    //}

    /// <summary>
    /// Отформатировать фильтр
    /// </summary>
    /// <param name="filter">Фильтр</param>
    public void FormatFilter(DBxFilter filter)
    {
      _Formatter.FormatFilter(this, filter);
    }

    /// <summary>
    /// Отформатировать порядок сортировки
    /// </summary>
    /// <param name="order">Порядок сортировки</param>
    public void FormatOrder(DBxOrder order)
    {
      _Formatter.FormatOrder(this, order);
    }


    /// <summary>
    /// Добавить в буфер фрагмент текста SQL-запроса, содержащий ссылку на параметр.
    /// Для Microsoft SQL Server используются именованные параметры "@P1", "@P2", ...
    /// Для Ole DB именованные параметры не используются, вместо них используются
    /// символы "[?]"
    /// Параметры в запросе нумеруются с 0 и должны идти по порядку, не повторяясь
    /// </summary>
    /// <param name="paramIndex">Индекс параметра</param>
    public void FormatParamPlaceholder(int paramIndex)
    {
      _Formatter.FormatParamPlaceholder(this, paramIndex);
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Добавление списка имен полей (для оператора APPEND)
    /// </summary>
    /// <param name="columns">Имена столбцов</param>
    public void FormatCSColumnNames(DBxColumns columns)
    {
      for (int i = 0; i < columns.Count; i++)
      {
        if (i > 0)
          SB.Append(", ");
        FormatColumnName(columns[i]);
      }
    }


    /// <summary>
    /// Добавление списка значений через запятую (для оператора APPEND)
    /// </summary>
    /// <param name="values">Значения</param>
    /// <param name="columnTypes">Типы значений</param>
    public void FormatCSValues(object[] values, DBxColumnType[] columnTypes)
    {
#if DEBUG
      if (columnTypes.Length != values.Length)
        throw new ArgumentException("Неправильная длина columnTypes", "columnTypes");
#endif

      for (int i = 0; i < values.Length; i++)
      {
        if (i > 0)
          SB.Append(", ");
        FormatValue(values[i], columnTypes[i]);
      }
    }

    /// <summary>
    /// Добавляет в буфер выражение ON DELETE XXX в соответствии с типом ссылки
    /// </summary>
    /// <param name="refType">Требуемое действие</param>
    public void FormatRefColumnDeleteAction(DBxRefType refType)
    {
      switch (refType)
      {
        case DBxRefType.Disallow:
          SB.Append(" ON DELETE NO ACTION");
          break;
        case DBxRefType.Delete:
          SB.Append(" ON DELETE CASCADE");
          break;
        case DBxRefType.Clear:
          SB.Append(" ON DELETE SET NULL");
          break;
        case DBxRefType.Emulation:
          throw new InvalidOperationException("В режиме Emulation не должно вызываться");
        default:
          throw new ArgumentException("Неизвестный режим " + refType.ToString(), "refType");
      }
    }

    /// <summary>
    /// Форматирование оператора SELECT.
    /// Этот метод не предназначен для использования в пользовательском коде.
    /// Имена и типы столбцов в создаваемой таблице могут не совпадать с запрошенными
    /// </summary>
    /// <param name="info">Заполненные параметры оператора SELECT</param>
    /// <param name="validator">Объект для проверки имен (свойство DBxConBase.)</param>
    public void FormatSelect(DBxSelectInfo info, DBxNameValidator validator)
    {
      DBxSelectFormatter fsf = new DBxSelectFormatter(info, validator);
      fsf.Format(this);
    }

    //public object Tag;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return "=>" + Formatter.ToString();
    }

    #endregion
  }

  #region Перечисление DBxSelectMaxRecordCountMode

  /// <summary>
  /// Способ указания ограничения на число строк в запросе SELECT
  /// </summary>
  public enum DBxSelectMaxRecordCountMode
  {
    /// <summary>
    /// Используется конструкция "SELECT TOP x ..." (MS SQL Server)
    /// </summary>
    Top,

    /// <summary>
    /// Используется конструкция "SELECT ... LIMIT x"
    /// </summary>
    Limit
  }

  #endregion

  /*
   * Иерархия классов форматировщиков, использовавшая до 09.10.2019
   * DBxSqlFormatter
   *   DataViewDBxSqlFormatter
   *     BaseDBxSqlFormatter
   *       SqlDBxSqlFormatter
   *       OracleDBxSqlFormatter
   *       OledbDBxSqlFormatter
   *       NpgsqlDBxSqlFormatter
   *       SQLiteDBxSqlFormatter
   *   DBxSqlChainFormatter
   *     FillSelectFormatter
   * 
   * C 10.10.2019 иерархия меняется.
   * - Класс FillSelectFormatter больше не наследуется от DBxSqlFormatter
   * - DBxSqlChainFormatter упраздняется
   * - Базовый класс DBxSqlFormatter сохраняется ничего не делающим, на случай, если цепочки вновь понадобятся
   * - DataViewDBxSqlFormatter выводится из BaseDBxSqlFormatter, а не наоборот.
   * Новая иерархия:
   * DBxSqlFormatter
   *   BaseDBxSqlFormatter
   *     DataViewDBxSqlFormatter
   *     SqlDBxSqlFormatter
   *     OracleDBxSqlFormatter
   *     OledbDBxSqlFormatter
   *     NpgsqlDBxSqlFormatter
   *     SQLiteDBxSqlFormatter
   */


  ///// <summary>
  ///// Виды имен, используемых в SQL-запросах
  ///// </summary>
  //public enum DBxNameKind { Table, Index, Column}

  /// <summary>
  /// Базовый класс форматизатора частей SQL-запроса
  /// Все реализации производных классов должны быть потокобезопасными!
  /// </summary>
  public abstract class DBxSqlFormatter
  {
    /*
     * В версиях методов без аргумента StringBuiler, временный объект StringBuiler создается каждый
     * раз заново. Нельзя многократно использовать один объект, т.к. методы могут вызываться асинхронно
     */


    #region Имя таблицы и имя поля

    // Проверка имен таблиц и столбцов не выполняется в форматизаторе.
    // Единственная проверка - что имя задано
    // За это отвечает класс DBxConBase

    internal void FormatTableName(DBxSqlBuffer buffer, string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      OnFormatTableName(buffer, tableName);
    }

    /// <summary>
    /// Форматирование имени таблицы.
    /// Записывает в <paramref name="buffer"/>.SB имя таблицы <paramref name="tableName"/>,
    /// заключенное в кавычки или квадратные скобки
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableName">Имя таблицы</param>
    protected abstract void OnFormatTableName(DBxSqlBuffer buffer, string tableName);

    internal void FormatColumnName(DBxSqlBuffer buffer, string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
      OnFormatColumnName(buffer, columnName);
    }

    /// <summary>
    /// Форматирование имени столбца.
    /// Записывает в <paramref name="buffer"/>.SB имя столбца <paramref name="columnName"/>,
    /// заключенное в кавычки или квадратные скобки
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="columnName">Имя столбца</param>
    protected abstract void OnFormatColumnName(DBxSqlBuffer buffer, string columnName);

    internal void FormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      if (String.IsNullOrEmpty(tableAlias))
        throw new ArgumentNullException("tableAlias");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      OnFormatColumnName(buffer, tableAlias, columnName);
    }

    /// <summary>
    /// Форматирование имени столбца c префиксом имени таблицы.
    /// Используется в запросах SELECT с инструкцией JOIN
    /// Записывает в <paramref name="buffer"/>.SB имя таблицы и столбца,
    /// заключенное в кавычки или квадратные скобки. Таблица и поле разделяются точкой
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableAlias">Альяс таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    protected abstract void OnFormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName);

    #endregion

    #region Тип поля

    internal void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
    {
      if (column == null)
        throw new ArgumentNullException("column");

      OnFormatValueType(buffer, column);
    }

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Описание столбца</param>
    protected abstract void OnFormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column);

    #endregion

    #region Значение
    /*
    public string FormatValue(object Value)
    {
      StringBuilder sb = new StringBuilder();
      FormatValue(Value, sb);
      return sb.ToString();
    }
 * */

    internal void FormatValue(DBxSqlBuffer buffer, object value, DBxColumnType columnType)
    {
      OnFormatValue(buffer, value, columnType);
    }

    /// <summary>
    /// Форматирование значения поля
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>,
    /// возможно, заключенное в апострофы или другие символы.
    /// Также отвечает за экранирование символов строкового значения
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="columnType">Тип значения</param>
    protected abstract void OnFormatValue(DBxSqlBuffer buffer, object value, DBxColumnType columnType);

    #endregion

    #region Фильтры

    internal void FormatFilter(DBxSqlBuffer buffer, DBxFilter filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      OnFormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Вызывает виртуальный метод для фильтра нужного типа.
    /// Обычно нет необходимости переопределять этот метод
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatFilter(DBxSqlBuffer buffer, DBxFilter filter)
    {
      if (filter is CompareFilter)
        OnFormatCompareFilter(buffer, (CompareFilter)filter);
      else if (filter is IdsFilter)
        OnFormatIdsFilter(buffer, (IdsFilter)filter);
      else if (filter is ValuesFilter)
        OnFormatValuesFilter(buffer, (ValuesFilter)filter);
      else if (filter is StringValueFilter)
        OnFormatStringValueFilter(buffer, (StringValueFilter)filter);
      else if (filter is AndFilter)
        OnFormatAndFilter(buffer, (AndFilter)filter);
      else if (filter is OrFilter)
        OnFormatOrFilter(buffer, (OrFilter)filter);
      else if (filter is NotFilter)
        OnFormatNotFilter(buffer, (NotFilter)filter);
      else if (filter is NumRangeFilter)
        OnFormatNumRangeFilter(buffer, (NumRangeFilter)filter);
      else if (filter is DateRangeFilter)
        OnFormatDateRangeFilter(buffer, (DateRangeFilter)filter);
      else if (filter is DateRangeInclusionFilter)
        OnFormatDateRangeInclusionFilter(buffer, (DateRangeInclusionFilter)filter);
      else if (filter is DateRangeCrossFilter)
        OnFormatDateRangeCrossFilter(buffer, (DateRangeCrossFilter)filter);
      else if (filter is StartsWithFilter)
        OnFormatStartsWithFilter(buffer, (StartsWithFilter)filter);
      else if (filter is SubstringFilter)
        OnFormatSubstringFilter(buffer, (SubstringFilter)filter);
      else if (filter is InSelectFilter)
        OnFormatInSelectFilter(buffer, (InSelectFilter)filter);
      else if (filter is DummyFilter)
        OnFormatDummyFilter(buffer, (DummyFilter)filter);
      else
        throw new ArgumentException("Неподдерживаемый тип фильтра: " + filter.GetType().ToString());
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatIdsFilter(DBxSqlBuffer buffer, IdsFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatAndFilter(DBxSqlBuffer buffer, AndFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatOrFilter(DBxSqlBuffer buffer, OrFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatNotFilter(DBxSqlBuffer buffer, NotFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatNumRangeFilter(DBxSqlBuffer buffer, NumRangeFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatDateRangeFilter(DBxSqlBuffer buffer, DateRangeFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatDateRangeInclusionFilter(DBxSqlBuffer buffer, DateRangeInclusionFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatDateRangeCrossFilter(DBxSqlBuffer buffer, DateRangeCrossFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatCompareFilter(DBxSqlBuffer buffer, CompareFilter filter);
    
    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatInSelectFilter(DBxSqlBuffer buffer, InSelectFilter filter);

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected abstract void OnFormatDummyFilter(DBxSqlBuffer buffer, DummyFilter filter);

    #endregion

    #region Порядок сортировки

    internal void FormatOrder(DBxSqlBuffer buffer, DBxOrder order)
    {
      if (order == null)
        throw new ArgumentNullException("order");

      OnFormatOrder(buffer, order);
    }

    /// <summary>
    /// Форматирование порядка сортировки.
    /// Вызывает <paramref name="buffer"/>.FormatOrderItem() для каждого элемента и
    /// добавляет суффикс "DESC" по необходимости.
    /// Нет необходимости переопределять этот метод
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="order">Порядок сортировки</param>
    protected virtual void OnFormatOrder(DBxSqlBuffer buffer, DBxOrder order)
    {
      for (int i = 0; i < order.Parts.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");
        buffer.FormatExpression(order.Parts[i].Expression, new DBxFormatExpressionInfo());
        if (order.Parts[i].SortOrder==ListSortDirection.Descending)
          buffer.SB.Append(" DESC");
      }
    }

    #endregion

    #region Выражения

    internal void FormatExpression(DBxSqlBuffer buffer, DBxExpression expression, DBxFormatExpressionInfo formatInfo)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      if (!(formatInfo.NoParentheses) && AreParenthesesRequired(expression))
      {
        formatInfo.NoParentheses = true; // отключаем обязательно
        buffer.SB.Append("(");
        OnFormatExpression(buffer, expression, formatInfo);
        buffer.SB.Append(")");
      }
      else // исправлено 31.12.2019
        OnFormatExpression(buffer, expression, formatInfo);
    }


    /// <summary>
    /// Форматирование выражения.
    /// Обычно этот метод не переопределяется
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="expression">Элемент порядка сортировки. Не может быть null</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей</param>
    protected virtual void OnFormatExpression(DBxSqlBuffer buffer, DBxExpression expression, DBxFormatExpressionInfo formatInfo)
    {
      if (expression is DBxColumn)
        OnFormatColumn(buffer, (DBxColumn)expression, formatInfo);
      else if (expression is DBxFunction)
        OnFormatFunction(buffer, (DBxFunction)expression, formatInfo);
      else if (expression is DBxConst)
        OnFormatValue(buffer, ((DBxConst)expression).Value, ((DBxConst)expression).ColumnType);
      else if (expression is DBxAgregateFunction)
        OnFormatAgregateFunction(buffer, (DBxAgregateFunction)expression, formatInfo);
      else
        throw new ArgumentException("Неподдерживаемый тип поля сортировки: " + expression.GetType().ToString());
    }

    /// <summary>
    /// Форматирование выражения, возвращающего значение столбца.
    /// Для запросов к базе данных выполняется подстановка для ссылочных полей и указанием альяса таблицы, если задано.
    /// Для DataView вызывается OnFormatColumnName.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="column">Выражение-столбец. Не может быть null</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей</param>
    protected abstract void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo);

    /// <summary>
    /// Форматирование функции или арифметического выражения
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="function">Объект фунцкии</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей. Используется при рекурсивном вызове Buffer.FormatExpression</param>
    protected abstract void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo);

    /// <summary>
    /// Форматирование агрегатной функции
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="function">Объект фунцкии</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей. Используется при рекурсивном вызове Buffer.FormatExpression</param>
    protected abstract void OnFormatAgregateFunction(DBxSqlBuffer buffer, DBxAgregateFunction function, DBxFormatExpressionInfo formatInfo);

    /// <summary>
    /// Метод должен вернуть true, если выражение может потребоваться заключить в скобки, когда оно входит в состав другого выражения
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <returns>Потребность в скобках</returns>
    internal bool AreParenthesesRequired(DBxExpression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      return OnAreParenthesesRequired(expression);
    }

    /// <summary>
    /// Метод должен вернуть true, если выражение может потребоваться заключить в скобки, когда оно входит в состав другого выражения
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <returns>Потребность в скобках</returns>
    protected abstract bool OnAreParenthesesRequired(DBxExpression expression);

    #endregion

    #region Форматирование для SELECT

    /// <summary>
    /// Способ задания ограничения MaxRecordCount в SELECT.
    /// По умолчанию используется форма SELECT TOP x
    /// </summary>
    public virtual DBxSelectMaxRecordCountMode SelectMaxRecordCountMode { get { return DBxSelectMaxRecordCountMode.Top; } }

    /// <summary>
    /// Нужно ли корректировать типы данных в наборе, возвращаемом оператором SELECT
    /// Используется только в SQLite.
    /// </summary>
    public virtual bool UseTypeCorrectionInSelectResult { get { return false; } }
    // (использовать оператор TYPES перед SELECT - не помогает).


    #endregion

    #region Список параметров запроса

    internal void FormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      OnFormatParamPlaceholder(buffer, paramIndex);
    }
            
    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра, например, "@P1",
    /// "@P2",... Способы указания параметров в SQL-запросах зависят от базы данных
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...). В некоторых базах данных
    /// (MS Jet OLEDB Provider) может не использоваться (все параметры обозначаюются как "[?]")</param>
    protected abstract void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex);

    /// <summary>
    /// Подготовка значения, передаваемого в качестве параметра запроса.
    /// Значение DBNull заменяется на null.
    /// Затем вызывается виртуальный метод OnPrepareParamValue()
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Тип данных</param>
    /// <returns>Скорректированное значение</returns>
    public object PrepareParamValue(object value, DBxColumnType columnType)
    {
      if (value is DBNull)
        value = null;
      return OnPrepareParamValue(value, columnType);
    }

    /// <summary>
    /// Подготовка значения, передаваемого в качестве параметра запроса.
    /// Непереопределенный метод возвращает <paramref name="value"/> неизменным.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Тип данных</param>
    /// <returns>Скорректированное значение</returns>
    protected virtual object OnPrepareParamValue(object value, DBxColumnType columnType)
    {
      return value;
    }

    #endregion

    #region Прочие параметры SQL запросов

    /// <summary>
    /// Максимальная длина SQL-запроса в символах
    /// </summary>
    public virtual int MaxSqlLength { get { return Int16.MaxValue; } }

    /// <summary>
    /// Максимальное количество строк данных в запросе INSERT INTO таблица (столбцы) VALUES (значения) [, (значения), ...]
    /// По умолчанию возвращает 1 - многострочная вставка не поддерживается
    /// </summary>
    public virtual int MaxInsertIntoValueRowCount { get { return 1; } }

    #endregion


    #region Статический экземпляр класса

    /// <summary>
    /// Статический экземпляр форматизатора SQL-запросов для объекта DataView
    /// Используется для получения текстового представления
    /// </summary>
    public static readonly DBxSqlFormatter Default = new FreeLibSet.Data.DataViewDBxSqlFormatter();

    #endregion
  }

  /// <summary>
  /// Цепочечный форматизатор частей SQL-запроса.
  /// Можно создать производный класс, который будет переопределять форматирование какого-либо элемента.
  /// Для непереопределенных методов вызывается основной форматизатор. Таким образом формируеется цепочка
  /// </summary>
  public class DBxSqlChainFormatter : DBxSqlFormatter
  {
    #region Конструктор

    /// <summary>
    /// Создает форматизатор
    /// </summary>
    /// <param name="source">Предыдущий элемент цепочки, которому передаются запросы. Должен быть задан</param>
    public DBxSqlChainFormatter(DBxSqlFormatter source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Предыдущий элемент цепочки, которому передаются запросы на форматирование, если соответствующий
    /// метод не переопределен
    /// </summary>
    public DBxSqlFormatter Source { get { return _Source; } }
    private DBxSqlFormatter _Source;

    #endregion

    #region Передача выполнения основному объекту

    /// <summary>
    /// Форматирование имени таблицы.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableName">Имя таблицы</param>
    protected override void OnFormatTableName(DBxSqlBuffer buffer, string tableName)
    {
      _Source.FormatTableName(buffer, tableName);
    }

    /// <summary>
    /// Форматирование имени столбца.
    /// Записывает в <paramref name="buffer"/>.SB имя столбца <paramref name="columnName"/>,
    /// заключенное в кавычки или квадратные скобки
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="columnName">Имя столбца</param>
    protected override void OnFormatColumnName(DBxSqlBuffer buffer, string columnName)
    {
      _Source.FormatColumnName(buffer, columnName);
    }


    /// <summary>
    /// Форматирование имени столбца c префиксом имени таблицы.
    /// Используется в запросах SELECT с инструкцией JOIN
    /// Записывает в <paramref name="buffer"/>.SB имя таблицы и столбца,
    /// заключенное в кавычки или квадратные скобки. Таблица и поле разделяются точкой
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableAlias">Альяс таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    protected override void OnFormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      _Source.FormatColumnName(buffer, tableAlias, columnName);
    }

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="Buffer">Буфер для создания SQL-запроса</param>
    /// <param name="Column">Описание столбца</param>
    protected override void OnFormatValueType(DBxSqlBuffer Buffer, DBxColumnStruct Column)
    {
      _Source.FormatValueType(Buffer, Column);
    }

    /// <summary>
    /// Форматирование значения поля
    /// </summary>
    /// <param name="Buffer">Буфер для записи</param>
    /// <param name="Value">Записываемое значение</param>
    /// <param name="ColumnType">Тип значения</param>
    protected override void OnFormatValue(DBxSqlBuffer Buffer, object Value, DBxColumnType ColumnType)
    {
      _Source.FormatValue(Buffer, Value, ColumnType);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatIdsFilter(DBxSqlBuffer buffer, IdsFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatAndFilter(DBxSqlBuffer buffer, AndFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatOrFilter(DBxSqlBuffer buffer, OrFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatNotFilter(DBxSqlBuffer buffer, NotFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatNumRangeFilter(DBxSqlBuffer buffer, NumRangeFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatDateRangeFilter(DBxSqlBuffer buffer, DateRangeFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatDateRangeInclusionFilter(DBxSqlBuffer buffer, DateRangeInclusionFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatDateRangeCrossFilter(DBxSqlBuffer buffer, DateRangeCrossFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatCompareFilter(DBxSqlBuffer buffer, CompareFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatInSelectFilter(DBxSqlBuffer buffer, InSelectFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Записывает в <paramref name="buffer"/>.SB фрагмент SQL-запроса для фильтра (без слова "WHERE")
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void OnFormatDummyFilter(DBxSqlBuffer buffer, DummyFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }


    /// <summary>
    /// Форматирование выражения, возвращающего значение столбца.
    /// Для запросов к базе данных выполняется подстановка для ссылочных полей и указанием альяса таблицы, если задано.
    /// Для DataView вызывается OnFormatColumnName.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="column">Выражение-столбец. Не может быть null</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей</param>
    protected override void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo)
    {
      _Source.FormatExpression(buffer, column, formatInfo);
    }

    /// <summary>
    /// Форматирование функции или арифметического выражения
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="function">Объект фунцкии</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей. Используется при рекурсивном вызове Buffer.FormatExpression</param>
    protected override void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      _Source.FormatExpression(buffer, function, formatInfo);
    }

    /// <summary>
    /// Форматирование агрегатной функции
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="function">Объект фунцкии</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей. Используется при рекурсивном вызове Buffer.FormatExpression</param>
    protected override void OnFormatAgregateFunction(DBxSqlBuffer buffer, DBxAgregateFunction function, DBxFormatExpressionInfo formatInfo)
    {
      _Source.FormatExpression(buffer, function, formatInfo);
    }

    /// <summary>
    /// Метод должен вернуть true, если выражение может потребоваться заключить в скобки, когда оно входит в состав другого выражения
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <returns>Потребность в скобках</returns>
    protected override bool OnAreParenthesesRequired(DBxExpression expression)
    {
      return _Source.AreParenthesesRequired(expression);
    }

    /// <summary>
    /// Форматирование параметра. Вызывает Source.FormatParamPlaceholder()
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...)</param>
    protected override void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      Source.FormatParamPlaceholder(buffer, paramIndex);
    }

    /// <summary>
    /// Способ задания ограничения MaxRecordCount в SELECT.
    /// По умолчанию используется форма SELECT TOP x
    /// </summary>
    public override DBxSelectMaxRecordCountMode SelectMaxRecordCountMode
    { get { return _Source.SelectMaxRecordCountMode; } }

    /// <summary>
    /// Нужно ли корректировать типы данных в наборе, возвращаемом оператором SELECT
    /// Используется только в SQLite.
    /// </summary>
    public override bool UseTypeCorrectionInSelectResult
    { get { return _Source.UseTypeCorrectionInSelectResult; } }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return GetType().Name + "->" + Source.ToString();
    }

    #endregion
  }
}
