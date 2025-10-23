// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.Serialization;
using System.Globalization;
using System.ComponentModel;
using FreeLibSet.Core;

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
    /// Используется фильтрами <see cref="CompareFilter"/>, <see cref="ValueInListFilter"/>.
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
    /// в <see cref="DataViewDBxSqlFormatter"/>
    /// </summary>
    public DBxColumnType WantedColumnType { get { return _WantedColumnType; } set { _WantedColumnType = value; } }
    private DBxColumnType _WantedColumnType;

    /// <summary>
    /// Если false, то выражение будет заключено в круглые скобки, если это необходимо.
    /// Если выражение является <see cref="DBxFunction"/>, которое реализуется как оператор (Add, Substract, Multiply, Divide),
    /// то оно заключается в скобки. Остальные выражения (поля, константы, вызовы функций) в скобки не заключаются, независимо от значения свойства.
    /// Для определения необходимости использования скобок используется метод <see cref="DBxSqlFormatter.AreParenthesesRequired(DBxExpression)"/>.
    /// Если флаг установлен в true, то скобки применяться не будут. Используется, например, при задании аргументов функции.
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
      if (WantedColumnType != DBxColumnType.Unknown)
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
  /// Форматизатор частей SQL-запроса.
  /// Методы данного класса не являются потокобезопасными.
  /// Содержит <see cref="StringBuilder"/>.
  /// </summary>
  public sealed class DBxSqlBuffer
  {
    #region Конструкторы

    /// <summary>
    /// Создает буфер форматирования по умолчанию (для объекта <see cref="DataView"/>).
    /// Используется форматизатор <see cref="DBxSqlFormatter.Default"/>.
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
    /// Основной объект буфера для заполнения.
    /// </summary>
    public StringBuilder SB { get { return _SB; } }
    private readonly StringBuilder _SB;

    #endregion

    #region Форматизатор

    /// <summary>
    /// Форматизатор.
    /// Задается в конструкторе.
    /// </summary>
    public DBxSqlFormatter Formatter { get { return _Formatter; } }
    private readonly DBxSqlFormatter _Formatter;

    #endregion

    #region Информационные таблицы

    /// <summary>
    /// Альясы таблиц для имен полей.
    /// Используются при форматировании выражений <see cref="DBxColumn"/>.
    /// Ключ - имя поля. Могут быть как составные поля с точками, так и простые поля.
    /// Значение - альяс таблицы.
    /// </summary>
    public IDictionary<string, string> ColumnTableAliases { get { return _ColumnTableAliases; } }
    private readonly Dictionary<string, string> _ColumnTableAliases;

    /// <summary>
    /// Описания столбцов, используемых в запросе.
    /// Нужны для определения необходимости проверки значений на NULL и определения типа данных при вызове COALESCE()
    /// Ключ - имя поля (простого или с точками).
    /// Значение - Описание поля из структуры таблицы.
    /// </summary>
    public IDictionary<string, DBxColumnStruct> ColumnStructs { get { return _ColumnStructs; } }
    private readonly Dictionary<string, DBxColumnStruct> _ColumnStructs;

    #endregion

    #region Вызываемые методы

    /// <summary>
    /// Очищает буфер <see cref="SB"/> и списки <see cref="ColumnTableAliases"/>, <see cref="ColumnStructs"/>.
    /// </summary>
    public void Clear()
    {
      _SB.Length = 0;
      _ColumnTableAliases.Clear();
      _ColumnStructs.Clear();
    }

    /// <summary>
    /// Отформатировать имя таблицы. Обычно имя заключается в кавычки или квадратные скобки.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public void FormatTableName(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

      _Formatter.FormatTableName(this, tableName);
    }

    /// <summary>
    /// Отформатировать имя столбца. Обычно имя заключается в кавычки или квадратные скобки.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public void FormatColumnName(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
      _Formatter.FormatColumnName(this, columnName);
    }

    /// <summary>
    /// Форматирование имени столбца, которому предшествует имя альяса таблицы. Используется в запросах SELECT, в которых есть конструкция JOIN
    /// </summary>
    /// <param name="tableAlias">Альяс имени таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    public void FormatColumnName(string tableAlias, string columnName)
    {
      if (String.IsNullOrEmpty(tableAlias))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableAlias");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      _Formatter.FormatColumnName(this, tableAlias, columnName);
    }

    /// <summary>
    /// Форматировать тип значения для заданного описания столбца.
    /// Выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="column">Описание структуры столбца</param>
    public void FormatValueType(DBxColumnStruct column)
    {
      if (column == null)
        throw new ArgumentNullException("column");

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
      if (expression == null)
        throw new ArgumentNullException("expression");

      if (!(formatInfo.NoParentheses) && _Formatter.AreParenthesesRequired(expression))
      {
        formatInfo.NoParentheses = true; // отключаем обязательно
        SB.Append("(");
        _Formatter.FormatExpression(this, expression, formatInfo);
        SB.Append(")");
      }
      else // исправлено 31.12.2019
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
      if (filter == null)
        throw new ArgumentNullException("filter");

      // 02.06.2023
      // Некоторые БД (например, OleDB) для инструкции "BEETWEEN x AND y" выполняют выворачивание диапазона, когда x>y.
      // Это приводит к включению лишних строк.
      // Лучше централизованно исключить использование таких фильтров
      switch (filter.Degeneration)
      {
        case DBxFilterDegeneration.AlwaysTrue:
          _Formatter.FormatFilter(this, DummyFilter.AlwaysTrue);
          return;
        case DBxFilterDegeneration.AlwaysFalse:
          _Formatter.FormatFilter(this, DummyFilter.AlwaysFalse);
          return;
      }

      _Formatter.FormatFilter(this, filter);
    }

    /// <summary>
    /// Отформатировать порядок сортировки
    /// </summary>
    /// <param name="order">Порядок сортировки</param>
    public void FormatOrder(DBxOrder order)
    {
      if (order == null)
        throw new ArgumentNullException("order");

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
        throw ExceptionFactory.ArgWrongCollectionCount("columnTypes", columnTypes, values.Length);
#endif

      for (int i = 0; i < values.Length; i++)
      {
        if (i > 0)
          SB.Append(", ");
        FormatValue(values[i], columnTypes[i]);
      }
    }

    // Убрано 07.06.2024
    ///// <summary>
    ///// Форматирование оператора SELECT.
    ///// Этот метод не предназначен для использования в пользовательском коде.
    ///// Имена и типы столбцов в создаваемой таблице могут не совпадать с запрошенными.
    ///// </summary>
    ///// <param name="info">Заполненные параметры оператора SELECT</param>
    ///// <param name="validator">Объект для проверки имен (свойство <see cref="DBxConBase.Validator"/>)</param>
    //public void FormatSelect(DBxSelectInfo info, DBxNameValidator validator)
    //{
    //  DBxSelectFormatter fsf = new DBxSelectFormatter(info, validator);
    //  fsf.Format(this);
    //}

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
   *   DBxSqlChainFormatter
   *   
   *   С 26.09.2024 иерархия снова меняется.
   *   - Из класса DBxSqlFormatter убирается большая часть реализации.
   *   - Новый класс CoreDBxSqlFormatter, куда переносится функционал BaseDBxSqlFormatter.
   *   - DataViewDBxSqlFormatter выводится из CoreDBxSqlFormatter, а BaseDBxSqlFormatter используется только для баз данных.
   * Новая иерархия:
   * DBxSqlFormatter
   *   CoreDBxSqlFormatter
   *     DataViewDBxSqlFormatter
   *     BaseDBxSqlFormatter
   *       SqlDBxSqlFormatter
   *       OracleDBxSqlFormatter
   *       OledbDBxSqlFormatter
   *       NpgsqlDBxSqlFormatter
   *       SQLiteDBxSqlFormatter
   *   DBxSqlChainFormatter
   */


  ///// <summary>
  ///// Виды имен, используемых в SQL-запросах
  ///// </summary>
  //public enum DBxNameKind { Table, Index, Column}

  /// <summary>
  /// Базовый класс форматизатора частей SQL-запроса.
  /// Не содержит никаких конкретный реализаций форматирования.
  /// Класс <see cref="CoreDBxSqlFormatter"/> содержит стандартные реализации форматирования. От него наследуются конкретные реализации форматировщиков для баз данных и <see cref="DataView"/>.
  /// Все реализации производных классов должны быть потокобезопасными!
  /// </summary>
  public abstract class DBxSqlFormatter
  {
    /*
     * В версиях методов без аргумента StringBuiler, временный объект StringBuiler создается каждый
     * раз заново. Нельзя многократно использовать один объект, т.к. методы могут вызываться асинхронно
     */

    // 26.09.2024
    // Абстрактные методы OnXXX() объявлены как internal protected, а не protected, чтобы их можно было вызывать из ChainDBxSqlFormatter

    #region Имя таблицы и имя поля

    // Проверка имен таблиц и столбцов не выполняется в форматизаторе.
    // Единственная проверка - что имя задано.
    // За это отвечает класс DBxConBase.

    /// <summary>
    /// Форматирование имени таблицы.
    /// Записывает в <paramref name="buffer"/>.SB имя таблицы <paramref name="tableName"/>,
    /// заключенное в кавычки или квадратные скобки.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableName">Имя таблицы</param>
    internal protected abstract void FormatTableName(DBxSqlBuffer buffer, string tableName);

    /// <summary>
    /// Форматирование имени столбца.
    /// Записывает в <paramref name="buffer"/>.SB имя столбца <paramref name="columnName"/>,
    /// заключенное в кавычки или квадратные скобки.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="columnName">Имя столбца</param>
    internal protected abstract void FormatColumnName(DBxSqlBuffer buffer, string columnName);

    /// <summary>
    /// Форматирование имени столбца c префиксом имени таблицы.
    /// Используется в запросах SELECT с инструкцией JOIN.
    /// Записывает в <paramref name="buffer"/>.SB имя таблицы и столбца,
    /// заключенное в кавычки или квадратные скобки. 
    /// Таблица и поле разделяются точкой.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableAlias">Альяс таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    internal protected abstract void FormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName);

    #endregion

    #region Тип поля

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Описание столбца</param>
    internal protected abstract void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column);

    #endregion

    #region Значение

    /// <summary>
    /// Форматирование значения поля
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>,
    /// возможно, заключенное в апострофы или другие символы.
    /// Также отвечает за экранирование символов строкового значения
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="columnType">Тип значения</param>
    internal protected abstract void FormatValue(DBxSqlBuffer buffer, object value, DBxColumnType columnType);

    #endregion

    #region Выражения

    /// <summary>
    /// Форматирование выражения.
    /// Обычно этот метод не переопределяется.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="expression">Элемент порядка сортировки. Не может быть null</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей</param>
    internal protected abstract void FormatExpression(DBxSqlBuffer buffer, DBxExpression expression, DBxFormatExpressionInfo formatInfo);

    /// <summary>
    /// Возвращает true, если выражение <paramref name="expression"/> требуется заключать в круглые скобки для правильного порядка вычислений.
    /// Возвращает true для выражений-математических операций.
    /// </summary>
    /// <param name="expression">Выражение, которое, может быть, нужно заключать в скобки</param>
    /// <returns>true, если скобки требуются</returns>
    internal protected abstract bool AreParenthesesRequired(DBxExpression expression);

    #endregion

    #region Фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    internal protected abstract void FormatFilter(DBxSqlBuffer buffer, DBxFilter filter);

    #endregion

    #region Порядок сортировки

    /// <summary>
    /// Форматирование порядка сортировки.
    /// Вызывает <paramref name="buffer"/>.FormatOrderItem() для каждого элемента и
    /// добавляет суффикс "DESC" по необходимости.
    /// Нет необходимости переопределять этот метод.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="order">Порядок сортировки</param>
    internal protected abstract void FormatOrder(DBxSqlBuffer buffer, DBxOrder order);

    #endregion

    #region Форматирование для SELECT

    /// <summary>
    /// Способ задания ограничения MaxRecordCount в SELECT.
    /// По умолчанию используется форма SELECT TOP x.
    /// </summary>
    public abstract DBxSelectMaxRecordCountMode SelectMaxRecordCountMode { get; }

    /// <summary>
    /// Нужно ли корректировать типы данных в наборе, возвращаемом оператором SELECT
    /// Используется только в SQLite.
    /// </summary>
    public abstract bool UseTypeCorrectionInSelectResult { get; }

    #endregion

    #region Список параметров запроса

    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра, например, "@P1",
    /// "@P2",... Способы указания параметров в SQL-запросах зависят от базы данных
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...). В некоторых базах данных
    /// (MS Jet OLEDB Provider) может не использоваться (все параметры обозначаюются как "[?]")</param>
    internal protected abstract void FormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex);

    /// <summary>
    /// Подготовка значения, передаваемого в качестве параметра запроса.
    /// Значение <see cref="DBNull"/> заменяется на null.
    /// Затем вызывается виртуальный метод <see cref="OnPrepareParamValue(object, DBxColumnType)"/>.
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
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Тип данных</param>
    /// <returns>Скорректированное значение</returns>
    internal protected abstract object OnPrepareParamValue(object value, DBxColumnType columnType);

    #endregion

    #region Прочие параметры SQL запросов

    /// <summary>
    /// Максимальная длина SQL-запроса в символах
    /// </summary>
    public abstract int MaxSqlLength { get; }

    /// <summary>
    /// Максимальное количество строк данных в запросе INSERT INTO таблица (столбцы) VALUES (значения) [, (значения), ...].
    /// По умолчанию возвращает 1 - многострочная вставка не поддерживается.
    /// </summary>
    public abstract int MaxInsertIntoValueRowCount { get; }

    #endregion

    #region Статический экземпляр класса

    /// <summary>
    /// Статический экземпляр форматизатора SQL-запросов для объекта <see cref="DataView"/>.
    /// Используется для получения текстового представления.
    /// </summary>
    public static readonly DBxSqlFormatter Default = new FreeLibSet.Data.DataViewDBxSqlFormatter();

    #endregion
  }

  /// <summary>
  /// Цепочечный форматизатор частей SQL-запроса.
  /// Можно создать производный класс, который будет переопределять форматирование какого-либо элемента.
  /// Для непереопределенных методов вызывается основной форматизатор. Таким образом формируется цепочка.
  /// </summary>
  public class ChainDBxSqlFormatter : DBxSqlFormatter
  {
    #region Конструктор

    /// <summary>
    /// Создает форматизатор
    /// </summary>
    /// <param name="source">Предыдущий элемент цепочки, которому передаются запросы. Должен быть задан</param>
    public ChainDBxSqlFormatter(DBxSqlFormatter source)
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
    private readonly DBxSqlFormatter _Source;

    #endregion

    #region Имя таблицы и имя поля

    /// <summary>
    /// Форматирование имени таблицы.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableName">Имя таблицы</param>
    internal protected override void FormatTableName(DBxSqlBuffer buffer, string tableName)
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
    internal protected override void FormatColumnName(DBxSqlBuffer buffer, string columnName)
    {
      _Source.FormatColumnName(buffer, columnName);
    }


    /// <summary>
    /// Форматирование имени столбца c префиксом имени таблицы.
    /// Используется в запросах SELECT с инструкцией JOIN
    /// Записывает в <paramref name="buffer"/>.SB имя таблицы и столбца,
    /// заключенное в кавычки или квадратные скобки. Таблица и поле разделяются точкой.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="tableAlias">Альяс таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    internal protected override void FormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      _Source.FormatColumnName(buffer, tableAlias, columnName);
    }

    #endregion

    #region Тип поля

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Описание столбца</param>
    internal protected override void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
    {
      _Source.FormatValueType(buffer, column);
    }

    #endregion

    #region Значение

    /// <summary>
    /// Форматирование значения поля
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="Value">Записываемое значение</param>
    /// <param name="columnType">Тип значения</param>
    internal protected override void FormatValue(DBxSqlBuffer buffer, object Value, DBxColumnType columnType)
    {
      _Source.FormatValue(buffer, Value, columnType);
    }

    #endregion

    #region Выражения

    /// <summary>
    /// Форматирование выражения.
    /// Обычно этот метод не переопределяется.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="expression">Элемент порядка сортировки. Не может быть null</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей</param>
    internal protected override void FormatExpression(DBxSqlBuffer buffer, DBxExpression expression, DBxFormatExpressionInfo formatInfo)
    {
      _Source.FormatExpression(buffer, expression, formatInfo);
    }

    /// <summary>
    /// Метод должен вернуть true, если выражение может потребоваться заключить в скобки, когда оно входит в состав другого выражения
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <returns>Потребность в скобках</returns>
    internal protected override bool AreParenthesesRequired(DBxExpression expression)
    {
      return _Source.AreParenthesesRequired(expression);
    }

    #endregion

    #region Фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    internal protected override void FormatFilter(DBxSqlBuffer buffer, DBxFilter filter)
    {
      _Source.FormatFilter(buffer, filter);
    }

    #endregion

    #region Порядок сортировки

    /// <summary>
    /// Форматирование порядка сортировки.
    /// Вызывает <paramref name="buffer"/>.FormatOrderItem() для каждого элемента и
    /// добавляет суффикс "DESC" по необходимости.
    /// Нет необходимости переопределять этот метод.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="order">Порядок сортировки</param>
    internal protected override void FormatOrder(DBxSqlBuffer buffer, DBxOrder order)
    {
      _Source.FormatOrder(buffer, order);
    }

    #endregion

    #region Форматирование для SELECT


    /// <summary>
    /// Способ задания ограничения MaxRecordCount в SELECT.
    /// По умолчанию используется форма SELECT TOP x.
    /// </summary>
    public override DBxSelectMaxRecordCountMode SelectMaxRecordCountMode { get { return _Source.SelectMaxRecordCountMode; } }

    /// <summary>
    /// Нужно ли корректировать типы данных в наборе, возвращаемом оператором SELECT
    /// Используется только в SQLite.
    /// </summary>
    public override bool UseTypeCorrectionInSelectResult { get { return _Source.UseTypeCorrectionInSelectResult; } }

    #endregion

    #region Список параметров запроса

    /// <summary>
    /// Форматирование параметра. Вызывает <see cref="Source"/>.FormatParamPlaceholder().
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...)</param>
    internal protected override void FormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      Source.FormatParamPlaceholder(buffer, paramIndex);
    }

    /// <summary>
    /// Подготовка значения, передаваемого в качестве параметра запроса.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Тип данных</param>
    /// <returns>Скорректированное значение</returns>
    internal protected override object OnPrepareParamValue(object value, DBxColumnType columnType)
    {
      return _Source.OnPrepareParamValue(value, columnType);
    }

    #endregion

    #region Прочие параметры SQL запросов

    /// <summary>
    /// Максимальная длина SQL-запроса в символах
    /// </summary>
    public override int MaxSqlLength { get { return _Source.MaxSqlLength; } }

    /// <summary>
    /// Максимальное количество строк данных в запросе INSERT INTO таблица (столбцы) VALUES (значения) [, (значения), ...].
    /// По умолчанию возвращает 1 - многострочная вставка не поддерживается.
    /// </summary>
    public override int MaxInsertIntoValueRowCount { get { return _Source.MaxInsertIntoValueRowCount; } }

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
