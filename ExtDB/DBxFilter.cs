using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Text;
using AgeyevAV;

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

/*
 * Объекты фильтрации, производные от DataFilter, являются общими для клиента и
 * сервера. Их можно передавать от клиента к серверу с помощью сериализации
 *
 * Параметры каждого фильтра задаются в конструкторе и не могут меняться после этого,
 * все поля предназначены только для чтения.
 *
 * Фильтры можно комбинировать с помощью логических операций.
 * 
 * Для создания собственных фильтров необходимо:
 * 1. Создать класс фильтра
 *    [Serializable]
 *    public class MyFilter: DBxFilter
 *    {
 *    }
 * 
 * 2. Создать цепочечный форматизатор для фильтра
 *    public class MyFormatter: DBxSqlChainFormatter
 *    {
 *       // Переопределяем виртуальный метод
 *       public override void FormatFilter(DBxFilter Filter, StringBuilder sb, DBxSQLFormatInfo FormatInfo)
 *       {
 *         if (Filter is MyFilter)
 *         {
 *           // sb.Append(...); 
 *         } 
 *         else
 *           base.FormatFilter(Filter, sb, FormatInfo) 
 *       }
 *    }
 * 
 * 3. Создать собственный класс базы данных, производный от одного из стандартных, и в конструкторе вызвать SetFormatter
 * 
 *    public class MyDB: DBxSqlServer
 *    {
 *       public MyDB(...)
 *         : base(...)
*        {
 *         DBxSqlFormatter MainObj = base.Formatter;
 *         base.SetFormatter(new MyFormatter(MainObj));
 *       }
 *    }
 */

namespace AgeyevAV.ExtDB
{
  #region Перечисления

  /// <summary>
  /// Вырождение фильтра.
  /// Возвращается свойством DBxFilter.Degeneration
  /// </summary>
  [Serializable]
  public enum DBxFilterDegeneration
  {
    /// <summary>
    /// Нормальный, невырожденный фильтр
    /// </summary>
    None,

    /// <summary>
    /// Фильтр вырожден и ни на что не влияет, то есть пропускает все строки
    /// </summary>
    AlwaysTrue,

    /// <summary>
    /// Фильтр не пропускает ни одной строки. Возвращаемый просмотр будет всегда пустым
    /// </summary>
    AlwaysFalse
  }

  #endregion

  #region Абстрактные классы

  /// <summary>
  /// Базовый класс создания фильтров для SQL-выражений WHERE
  /// </summary>
  [Serializable]
  public abstract class DBxFilter
  {
    #region Переопределяемые свойства и методы

    /// <summary>
    /// Возвращает признак вырождения фильтра, то есть пропускает ли он все строки без ограничений или,
    /// наоборот, не пропускает ни одной строки.
    /// Непереопределенное свойство возвращает None, то есть фильтр не является вырожденным.
    /// </summary>
    /// <returns>true, если фильтр может быть исключен из рассмотрения</returns>
    public virtual DBxFilterDegeneration Degeneration { get { return DBxFilterDegeneration.None; } }

    /// <summary>
    /// Получить список имен полей, требуемых для фильтра (включая вложенные фильтры)
    /// Каждое поле в список входит один раз.
    /// </summary>
    /// <param name="list">Список для добавления полей. Не может быть null</param>
    public abstract void GetColumnNames(DBxColumnList list);

    /// <summary>
    /// Создание списка фильтров.
    /// Непереопределенный метод добавляет текущий фильтр в список.
    /// Составные фильтры AndFilter, OrFilter и NotFilter вызывают метод рекурсивно для добавления входящих фильтров
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public virtual void GetAllFilters(List<DBxFilter> list)
    {
      list.Add(this);
    }


    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Список <paramref name="rowValues"/> должен содержать все поля, которые требуются для фильтра и возвращаются GetColumnNames
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public abstract bool TestFilter(INamedValuesAccess rowValues);

    #endregion

    #region Методы работы с DataTable и DataView

    /// <summary>
    /// Заполнить в таблице значение одного столбца для строк, проходящих фильтр
    /// </summary>
    /// <param name="table">Таблица, к которой будет применен фильтр</param>
    /// <param name="columnName">Иия заполняемого столбца</param>
    /// <param name="value">Значение поля</param>
    public void SetColumnValues(DataTable table, string columnName, object value)
    {
      DataRow[] Rows = table.Select(ToString());
      if (Rows.Length > 0)
      {
        int p = table.Columns.IndexOf(columnName);
#if DEBUG
        if (p < 0)
          throw new ArgumentException("Столбец \"" + columnName + "\" не найден в таблице \"" + table.TableName + "\"");
#endif
        for (int i = 0; i < Rows.Length; i++)
          Rows[i][p] = value;
      }
    }

    /// <summary>
    /// Присоединить указанный фильтр Filter к уже заданному в просмотре DataView.RowFilter
    /// с помощью логической функции AND.
    /// Если фильтр DataView.RowFilter не был установлен, то фильтр Filter устанавливается
    /// напрямую, без использования функции AND
    /// Если Filter "вырожден" (AlwaysTrue==true), то никаких действий не выполняется
    /// </summary>
    /// <param name="dv">Просмотр DataView, к которому присоединяется фильтр</param>
    public void AddToDataViewRowFilter(DataView dv)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      dv.RowFilter = AddToDataViewRowFilter(dv.RowFilter);
    }

    /// <summary>
    /// Присоединить указанный фильтр Filter к уже заданному <paramref name="rowFilter"/>.
    /// с помощью логической функции AND.
    /// Если фильтр <paramref name="rowFilter"/> содержит пустую строку, то 
    /// возвращается обычное представление фильтра ToString()
    /// напрямую, без использования функции AND
    /// Если Filter "вырожден" (Degeneration==AlwaysTrue), то никаких действий не выполняется.
    /// </summary>
    /// <param name="rowFilter">Текущий установленный фильтр в DataView.RowFilter</param>
    /// <returns>Новое значение для DataView.RowFilter</returns>
    public string AddToDataViewRowFilter(string rowFilter)
    {
      if (this.Degeneration == DBxFilterDegeneration.AlwaysTrue)
        return rowFilter;

      DBxSqlBuffer Buffer = new DBxSqlBuffer();
      if (String.IsNullOrEmpty(rowFilter))
        Buffer.FormatFilter(this);
      else
      {
        if (this is AndFilter)
        {
          // Упрощенный вариант без лишних скобок
          Buffer.SB.Append("(");
          Buffer.SB.Append(rowFilter);
          Buffer.SB.Append(") AND ");
          Buffer.FormatFilter(this);
        }
        else
        {
          Buffer.SB.Append("(");
          Buffer.SB.Append(rowFilter);
          Buffer.SB.Append(") AND (");
          Buffer.FormatFilter(this);
          Buffer.SB.Append(")");
        }
      }
      return Buffer.SB.ToString();
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает текст фильтра в формате свойства DataView.RowFilter
    /// </summary>
    /// <returns>SQL-выражение</returns>
    public override string ToString()
    {
      DBxSqlBuffer Buffer = new DBxSqlBuffer();
      Buffer.FormatFilter(this);
      return Buffer.SB.ToString();
    }

#if DEBUG
    /// <summary>
    /// Проверка корректности имени поля. Наличие поля в таблице не
    /// проверяется. Если имя неправильное, выбрасывается исключение
    /// </summary>
    /// <param name="columnName">Проверяемое имя поля</param>
    protected void CheckColumnName(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      const string BadChars = " ,;";
      for (int i = 0; i < BadChars.Length; i++)
      {
        int p = columnName.IndexOf(BadChars[i]);
        if (p >= 0)
          throw new ArgumentException("Имя поля \"" + columnName + "\" содержит недопустимый символ \"" +
            BadChars[i] + "\" в позиции " + (p + 1).ToString());
      }
    }
#endif

    #endregion

    #region Операторы

    /// <summary>
    /// Возвращает AndFilter, если оба фильтра заданы.
    /// Учитывает вырожденные фильтры (свойство DBxFilter.Degeneration)
    /// </summary>
    /// <param name="filter1">Первый фильтр</param>
    /// <param name="filter2">Второй фильтр</param>
    /// <returns>Объединенный фильтр</returns>
    public static DBxFilter operator &(DBxFilter filter1, DBxFilter filter2)
    {
      if (filter1 != null && filter1.Degeneration == DBxFilterDegeneration.AlwaysTrue)
        filter1 = null;
      if (filter2 != null && filter2.Degeneration == DBxFilterDegeneration.AlwaysTrue)
        filter2 = null;

      if (filter1 == null)
        return filter2;

      if (filter2 == null)
        return filter1;

      if (filter1.Degeneration == DBxFilterDegeneration.AlwaysFalse || filter2.Degeneration == DBxFilterDegeneration.AlwaysFalse)
        return DummyFilter.AlwaysFalse;

      return new AndFilter(filter1, filter2);
    }

    /// <summary>
    /// Возвращает OrFilter, если оба фильтра заданы.
    /// Учитывает вырожденные фильтры (свойство DBxFilter.Degeneration).
    /// </summary>
    /// <param name="filter1">Первый фильтр</param>
    /// <param name="filter2">Второй фильтр</param>
    /// <returns>Объединенный фильтр</returns>
    public static DBxFilter operator |(DBxFilter filter1, DBxFilter filter2)
    {
      if (filter1 != null && filter1.Degeneration == DBxFilterDegeneration.AlwaysFalse)
        filter1 = null;
      if (filter2 != null && filter2.Degeneration == DBxFilterDegeneration.AlwaysFalse)
        filter2 = null;

      if (filter1 == null)
        return filter2;

      if (filter2 == null)
        return filter1;

      if (filter1.Degeneration == DBxFilterDegeneration.AlwaysTrue || filter2.Degeneration == DBxFilterDegeneration.AlwaysTrue)
        return DummyFilter.AlwaysTrue;

      return new OrFilter(filter1, filter2);
    }

    /// <summary>
    /// Возвращает NotFilter.
    /// Если <paramref name="filter"/> сам является NotFilter, то возвращается NotFilter.BaseFilter.
    /// </summary>
    /// <param name="filter">Исходный фильтр</param>
    /// <returns>NotFilter</returns>
    public static DBxFilter operator !(DBxFilter filter)
    {
      if (filter == null)
        return DummyFilter.AlwaysFalse; // 24.07.2019

      switch (filter.Degeneration)
      {
        case DBxFilterDegeneration.AlwaysTrue:
          return DummyFilter.AlwaysFalse;
        case DBxFilterDegeneration.AlwaysFalse:
          return DummyFilter.AlwaysTrue;
      }

      if (filter is NotFilter)
      {
        // Двойное условие NOT

        return ((NotFilter)filter).BaseFilter;
      }

      return new NotFilter(filter);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс фильтра, который использует одно поле или выражение
  /// </summary>
  [Serializable]
  public abstract class DBxSingleExpressionFilter : DBxFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Выражение. Не может быть null. Не может быть константой</param>
    protected DBxSingleExpressionFilter(DBxExpression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
#if DEBUG
      if (expression.GetConst() != null)
        throw new ArgumentException("Константа не может быть аргументом", "expression");
#endif

      _Expression = expression;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обычно - имя поля
    /// </summary>
    public DBxExpression Expression { get { return _Expression; } }
    private DBxExpression _Expression;

    //public string ColumnName
    //{
    //  get
    //  {
    //    if (_Expression is DBxColumn)
    //      return ((DBxColumn)_Expression).ColumnName;
    //    if (_Expression is DBxRefColumn)
    //      return String.Join(".", ((DBxRefColumn)_Expression).ColumnNames);
    //    return String.Empty;
    //  }
    //}

    #endregion

    #region Методы

    /// <summary>
    /// Добавляет в список имена полей из выражения Expression.
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public override sealed void GetColumnNames(DBxColumnList list)
    {
      _Expression.GetColumnNames(list);
    }

    #endregion
  }


  /// <summary>
  /// Базовый класс фильтра, который использует два поля или выражения
  /// </summary>
  [Serializable]
  public abstract class DBxTwoExpressionsFilter : DBxFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression1">Первое выражение. Не может быть null</param>
    /// <param name="expression2">Второе выражение. Не может быть null</param>
    protected DBxTwoExpressionsFilter(DBxExpression expression1, DBxExpression expression2)
    {
      if (expression1 == null)
        throw new ArgumentNullException("expression1");
      if (expression2 == null)
        throw new ArgumentNullException("expression2");

      _Expression1 = expression1;
      _Expression2 = expression2;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Первое выражение
    /// </summary>
    public DBxExpression Expression1 { get { return _Expression1; } }
    private DBxExpression _Expression1;

    /// <summary>
    /// Второе выражение
    /// </summary>
    public DBxExpression Expression2 { get { return _Expression2; } }
    private DBxExpression _Expression2;


    #endregion

    #region Методы

    /// <summary>
    /// Добавляет в список имена полей из Expression1 и Expression2.
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public override sealed void GetColumnNames(DBxColumnList list)
    {
      _Expression1.GetColumnNames(list);
      _Expression2.GetColumnNames(list);
    }

    #endregion
  }

#if XXX

  /// <summary>
  /// Базовый класс фильтра, который использует несколько полей или выражений
  /// </summary>
  public abstract class DBxExpressionsFilter : DBxFilter
  {
  #region Конструктор

    protected DBxExpressionsFilter(params DBxExpression[] expressions)
    {
      for (int i = 0; i < expressions.Length; i++)
      {
        if (expressions[i] == null)
          throw new ArgumentNullException("expressions[" + i.ToString() + "]");
      }
      _Expressions = expressions;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Выражение, используемое в левой части фильтра. Обычно - имя поля.
    /// Не может быть null.
    /// </summary>
    protected DBxExpression[] Expressions { get { return _Expressions; } }
    private DBxExpression[] _Expressions;

    public sealed override void GetColumnNames(SingleScopeList<DBxColumnBase> list)
    {
      for (int i = 0; i < _Expressions.Length; i++)
        _Expressions[i].GetColumnNames(list);
    }

  #endregion
  }

#endif

  #endregion

  #region Перечисление CompareKind

  /// <summary>
  /// Режимы сравнения поля с константой для фильтра ValueFilter
  /// </summary>
  [Serializable]
  public enum CompareKind
  {
    /// <summary>
    /// Основной режим сравнения на равенство
    /// </summary>
    Equal,

    /// <summary>
    /// Значение поля меньше, чем указанное
    /// </summary>
    LessThan,

    /// <summary>
    /// Значение поля меньше или равно указанному
    /// </summary>
    LessOrEqualThan,

    /// <summary>
    /// Значение поля больше, чем указанное
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Значение поля больше или равно указанному
    /// </summary>
    GreaterOrEqualThan,

    /// <summary>
    /// Не равно указанному значению
    /// </summary>
    NotEqual,
  }

  #endregion

  /// <summary>
  /// Фильтр сравнения значений двух полей вида "Поле1=Поле2" или для
  /// отношений "больше" и "меньше" или "не равно"
  /// Реакция на сравнение "больше" и "меньше", если одно из полей имеет значение NULL не предсказуема,
  /// поэтому следует проверять NULL в отдельном фильтре или использовать обязательные поля
  /// </summary>
  [Serializable]
  public class CompareFilter : DBxTwoExpressionsFilter
  {
    #region Конструкторы

    #region С выражениями

    /// <summary>
    /// Создает фильтр, используя выражения произвольного типа
    /// </summary>
    /// <param name="expression1">Первое выражение (левая часть сравнения). Не может быть null</param>
    /// <param name="expression2">Второе выражение (правая часть сравнения). Не может быть null</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="nullAsDefaultValue">Если true, и поля, входящие в выражения, могут принимать значения NULL, то значения NULL будут заменены на значения по умолчанию</param>
    /// <param name="columnType">Тип данных, которые должны возвращать выражения. Если Unknown, то тип данных будет вычислен, исходя из констант,
    /// входящих в выражения. Имеет значение только при <paramref name="nullAsDefaultValue"/>=true</param>
    public CompareFilter(DBxExpression expression1, DBxExpression expression2, CompareKind kind, bool nullAsDefaultValue, DBxColumnType columnType)
      : base(expression1, expression2)
    {
      _Kind = kind;
      _NullAsDefaultValue = nullAsDefaultValue;
      _ColumnType = columnType;
    }

    /// <summary>
    /// Создает фильтр, используя выражения произвольного типа
    /// </summary>
    /// <param name="expression1">Первое выражение (левая часть сравнения). Не может быть null</param>
    /// <param name="expression2">Второе выражение (правая часть сравнения). Не может быть null</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="nullAsDefaultValue">Если true, и поля, входящие в выражения, могут принимать значения NULL, то значения NULL будут заменены на значения по умолчанию</param>
    public CompareFilter(DBxExpression expression1, DBxExpression expression2, CompareKind kind, bool nullAsDefaultValue)
      : this(expression1, expression2, kind, nullAsDefaultValue, DBxColumnType.Unknown)
    {
    }

    /// <summary>
    /// Создает фильтр, используя выражения произвольного типа
    /// </summary>
    /// <param name="expression1">Первое выражение (левая часть сравнения). Не может быть null</param>
    /// <param name="expression2">Второе выражение (правая часть сравнения). Не может быть null</param>
    /// <param name="kind">Режим сравнения</param>
    public CompareFilter(DBxExpression expression1, DBxExpression expression2, CompareKind kind)
      : this(expression1, expression2, kind, false, DBxColumnType.Unknown)
    {
    }

    /// <summary>
    /// Создает фильтр на равенство, используя выражения произвольного типа
    /// </summary>
    /// <param name="expression1">Первое выражение (левая часть сравнения). Не может быть null</param>
    /// <param name="expression2">Второе выражение (правая часть сравнения). Не может быть null</param>
    public CompareFilter(DBxExpression expression1, DBxExpression expression2)
      : this(expression1, expression2, CompareKind.Equal, false, DBxColumnType.Unknown)
    {
    }

    #endregion

    #region С именами полей

    /// <summary>
    /// Создает фильтр сравнения значений двух полей.
    /// Для полей создаются объекты выражений DBxColumn.
    /// </summary>
    /// <param name="columnName1">Имя первого поля (левая часть сравнения)</param>
    /// <param name="columnName2">Второе выражение (правая часть сравнения)</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="nullAsDefaultValue">Если true, и поля, входящие в выражения, могут принимать значения NULL, то значения NULL будут заменены на значения по умолчанию</param>
    /// <param name="columnType">Тип данных, которые должны возвращать выражения. Если Unknown, то тип данных будет вычислен, исходя из констант,
    /// входящих в выражения. Имеет значение только при <paramref name="nullAsDefaultValue"/>=true</param>
    public CompareFilter(string columnName1, string columnName2, CompareKind kind, bool nullAsDefaultValue, DBxColumnType columnType)
      : this(new DBxColumn(columnName1), new DBxColumn(columnName2), kind, nullAsDefaultValue, columnType)
    {

    }

    /// <summary>
    /// Создает фильтр сравнения значений двух полей.
    /// Для полей создаются объекты выражений DBxColumn.
    /// </summary>
    /// <param name="columnName1">Имя первого поля (левая часть сравнения)</param>
    /// <param name="columnName2">Второе выражение (правая часть сравнения)</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="nullAsDefaultValue">Если true, и поля, входящие в выражения, могут принимать значения NULL, то значения NULL будут заменены на значения по умолчанию</param>
    public CompareFilter(string columnName1, string columnName2, CompareKind kind, bool nullAsDefaultValue)
      : this(new DBxColumn(columnName1), new DBxColumn(columnName2), kind, nullAsDefaultValue, DBxColumnType.Unknown)
    {

    }

    /// <summary>
    /// Создает фильтр сравнения значений двух полей.
    /// Для полей создаются объекты выражений DBxColumn.
    /// </summary>
    /// <param name="columnName1">Имя первого поля (левая часть сравнения)</param>
    /// <param name="columnName2">Второе выражение (правая часть сравнения)</param>
    /// <param name="kind">Режим сравнения</param>
    public CompareFilter(string columnName1, string columnName2, CompareKind kind)
      : this(new DBxColumn(columnName1), new DBxColumn(columnName2), kind, false, DBxColumnType.Unknown)
    {

    }

    /// <summary>
    /// Фильтр проверки на равенство полей
    /// </summary>
    /// <param name="columnName1">Имя первого поля (левая часть сравнения)</param>
    /// <param name="columnName2">Второе выражение (правая часть сравнения)</param>
    public CompareFilter(string columnName1, string columnName2)
      : this(new DBxColumn(columnName1), 
          new DBxColumn(columnName2), // 25.12.2020
          CompareKind.Equal, 
          false, 
          DBxColumnType.Unknown)
    {
    }

    #endregion

    #endregion

    #region Свойства

    /// <summary>
    /// Режим сравнения "равно", "больше" или "меньше"
    /// </summary>
    public CompareKind Kind { get { return _Kind; } }
    private CompareKind _Kind;

    /// <summary>
    /// Если true и поле имеет значение NULL, то оно приводится к значению по умолчанию для заданного типа
    /// </summary>
    public bool NullAsDefaultValue { get { return _NullAsDefaultValue; } }
    private bool _NullAsDefaultValue;

    /// <summary>
    /// Тип данных.
    /// Имеет смысл задавать только в тех случаях, когда сравниваются два столбца, а не столбец и константа, а тип данных не может быть
    /// определен из структуры базы данных
    /// </summary>
    public DBxColumnType ColumnType { get { return _ColumnType; } }
    private DBxColumnType _ColumnType;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Использует метод Comparer.DefaultInvariant.Compare().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      object v1 = Expression1.GetValue(rowValues, NullAsDefaultValue);
      object v2 = Expression2.GetValue(rowValues, NullAsDefaultValue);

      if (NullAsDefaultValue && ColumnType != DBxColumnType.Unknown)
      {
        if (v1 == null)
          v1 = DBxTools.GetDefaultValue(ColumnType);
        if (v2 == null)
          v2 = DBxTools.GetDefaultValue(ColumnType);
      }

      if (Object.ReferenceEquals(v1, null) && Object.ReferenceEquals(v2, null))
        return true;
      if (Object.ReferenceEquals(v1, null) || Object.ReferenceEquals(v2, null))
        return false;

      int res = Comparer.DefaultInvariant.Compare(v1, v2);
      switch (Kind)
      {
        case CompareKind.Equal: return res == 0;
        case CompareKind.NotEqual: return res != 0;
        case CompareKind.GreaterThan: return res > 0;
        case CompareKind.LessThan: return res < 0;
        case CompareKind.GreaterOrEqualThan: return res >= 0;
        case CompareKind.LessOrEqualThan: return res <= 0;
        default:
          throw new BugException("Неизвестный Kind=" + Kind.ToString());
      }
    }

    #endregion
  }


  /// <summary>
  /// Фильтр сравнения значения поле с константым значением.
  /// Этот фильтр является частным случаем CompareFilter, когда первым выражением является DBxColumn, а вторым выражением является константа DBxConst.
  /// Если требуется сравнение не с константой, а одного выражения с другим, используйте CompareFilter.
  /// Класс переопределяет только конструкторы базового класса.
  /// Свойство NullAsDefaultValue устанавливается автоматически, в зависимости от значения, с которым выполняется сравнение.
  /// </summary>
  [Serializable]
  public class ValueFilter : CompareFilter
  {
    #region Конструкторы

    #region Для выражения

    /// <summary>
    /// Создает фильтр сравнения значения выражения с указанием условия.
    /// </summary>
    /// <param name="expression">Выражение для левой части условия (обычно DBxColumn)</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="dataType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(DBxExpression expression, object constValue, CompareKind kind, Type dataType)
      : base(expression, new DBxConst(constValue, DBxTools.DataTypeToColumnType(dataType)), kind, GetNullAsDefaultValue(constValue), DBxTools.DataTypeToColumnType(dataType))
    {
    }

    /// <summary>
    /// Создает фильтр сравнения значения выражения с указанием условия.
    /// </summary>
    /// <param name="expression">Выражение для левой части условия (обычно DBxColumn)</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="columnType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(DBxExpression expression, object constValue, CompareKind kind, DBxColumnType columnType)
      : base(expression, new DBxConst(constValue, columnType), kind, GetNullAsDefaultValue(constValue), columnType)
    {
    }

    /// <summary>
    /// Создает фильтр сравнения значения выражения с указанием условия.
    /// Нельзя использовать эту версию, если константа равна null а поле может принимать значения NULL.
    /// </summary>
    /// <param name="expression">Выражение для левой части условия (обычно DBxColumn)</param>
    /// <param name="constValue">Значение константы</param>
    /// <param name="kind">Режим срав</param>
    public ValueFilter(DBxExpression expression, object constValue, CompareKind kind)
      : this(expression, constValue, kind, DBxColumnType.Unknown)
    {
    }

    /// <summary>
    /// Создает фильтр проверки выражения на равенство значению
    /// </summary>
    /// <param name="expression">Выражение для левой части условия (обычно DBxColumn)</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="dataType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(DBxExpression expression, object constValue, Type dataType)
      : base(expression, new DBxConst(constValue, DBxTools.DataTypeToColumnType(dataType)), CompareKind.Equal, GetNullAsDefaultValue(constValue), DBxTools.DataTypeToColumnType(dataType))
    {
    }

    /// <summary>
    /// Создает фильтр проверки выражения на равенство значению
    /// </summary>
    /// <param name="expression">Выражение для левой части условия (обычно DBxColumn)</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="columnType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(DBxExpression expression, object constValue, DBxColumnType columnType)
      : base(expression, new DBxConst(constValue, columnType), CompareKind.Equal, GetNullAsDefaultValue(constValue), columnType)
    {
    }

    /// <summary>
    /// Создает фильтр проверки поля на равенство значению
    /// Нельзя использовать эту версию, если константа равна null а поле может принимать значения NULL.
    /// </summary>
    /// <param name="expression">Выражение для левой части условия (обычно DBxColumn)</param>
    /// <param name="constValue">Значение константы</param>
    public ValueFilter(DBxExpression expression, object constValue)
      : this(expression, constValue, CompareKind.Equal, DBxColumnType.Unknown)
    {
    }

    #endregion

    #region Для имени поля

    /// <summary>
    /// Создает фильтр сравнения значения поля с указанием условия.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="dataType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(string columnName, object constValue, CompareKind kind, Type dataType)
      : base(new DBxColumn(columnName), new DBxConst(constValue, DBxTools.DataTypeToColumnType(dataType)), kind, GetNullAsDefaultValue(constValue), DBxTools.DataTypeToColumnType(dataType))
    {
    }

    /// <summary>
    /// Создает фильтр сравнения значения поля с указанием условия.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="kind">Режим сравнения</param>
    /// <param name="columnType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(string columnName, object constValue, CompareKind kind, DBxColumnType columnType)
      : base(new DBxColumn(columnName), new DBxConst(constValue, columnType), kind, GetNullAsDefaultValue(constValue), columnType)
    {
    }

    /// <summary>
    /// Создает фильтр сравнения значения поля с указанием условия.
    /// Нельзя использовать эту версию, если константа равна null а поле может принимать значения NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="constValue">Значение константы</param>
    /// <param name="kind">Режим срав</param>
    public ValueFilter(string columnName, object constValue, CompareKind kind)
      : this(columnName, constValue, kind, DBxColumnType.Unknown)
    {
    }

    /// <summary>
    /// Создает фильтр проверки поля на равенство значению
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="dataType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(string columnName, object constValue, Type dataType)
      : base(new DBxColumn(columnName), new DBxConst(constValue, DBxTools.DataTypeToColumnType(dataType)), CompareKind.Equal, GetNullAsDefaultValue(constValue), DBxTools.DataTypeToColumnType(dataType))
    {
    }

    /// <summary>
    /// Создает фильтр проверки поля на равенство значению
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="constValue">Константное значение</param>
    /// <param name="columnType">Тип данных, который может хранится в поле. Нужен для сравнения, когда <paramref name="constValue"/> равно null</param>
    public ValueFilter(string columnName, object constValue, DBxColumnType columnType)
      : base(new DBxColumn(columnName), new DBxConst(constValue, columnType), CompareKind.Equal, GetNullAsDefaultValue(constValue), columnType)
    {
    }

    /// <summary>
    /// Создает фильтр проверки поля на равенство значению
    /// Нельзя использовать эту версию, если константа равна null а поле может принимать значения NULL.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="constValue">Значение константы</param>
    public ValueFilter(string columnName, object constValue)
      : this(columnName, constValue, CompareKind.Equal, DBxColumnType.Unknown)
    {
    }

    #endregion

    private static bool GetNullAsDefaultValue(object constValue)
    {
      if (constValue == null)
        return false;
      if (constValue is DBNull)
        return false;
      if (constValue is DateTime)
        return false;

      return constValue.Equals(DataTools.GetEmptyValue(constValue.GetType()));
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Создать фильтры по значениям полей и объединить их функцией AND.
    /// Если поля не заданы, возвращается null.
    /// Если задано только одно поле, то возвращается ValueFilter, иначе
    /// возвращается AndFilter.
    /// </summary>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <returns>Объект фильтра или null</returns>
    public static DBxFilter CreateFilter(DBxColumns columnNames, object[] values)
    {
      if (columnNames.Count != values.Length)
        throw new ArgumentException("Список значений не совпадает со списком имен полей");
      if (columnNames.Count == 0)
        return null;
      if (columnNames.Count == 1)
        return new ValueFilter(columnNames[0], values[0]);
      DBxFilter[] Filters = new DBxFilter[columnNames.Count];
      for (int i = 0; i < Filters.Length; i++)
        Filters[i] = new ValueFilter(columnNames[i], values[i]);
      return new AndFilter(Filters);
    }

    /// <summary>
    /// Создать фильтры по значениям полей и объединить их функцией AND.
    /// Если поля не заданы, возвращается null.
    /// Если задано только одно поле, то возвращается ValueFilter, иначе
    /// возвращается AndFilter.
    /// </summary>
    /// <param name="columnNamesAndValues">Имена и значения полей</param>
    /// <returns>Объект фильтра или null</returns>
    public static DBxFilter CreateFilter(Hashtable columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);

      return CreateFilter(new DBxColumns(ColumnNames), Values);
    }

    /*
    /// <summary>
    /// Фильтр для поля Deleted=false (для документов и поддокументов)
    /// </summary>
    public static readonly ValueFilter DeletedFalse = new ValueFilter("Deleted", false);

    /// <summary>
    /// Фильтр для поля DocId.Deleted=false (для поддокументов)
    /// </summary>
    public static readonly ValueFilter DocIdDeletedFalse = new ValueFilter("DocId.Deleted", false);
     * */

    #endregion
  }

  /// <summary>
  /// Фильтр по значению поля идентификатора Id или другого поля ColumnName. Допустимое значение
  /// идентификатора может быть одно или несколько.
  /// Реализует условие вида: "Поле/Выражение IN ( Список значений )"
  /// </summary>
  [Serializable]
  public class IdsFilter : DBxSingleExpressionFilter
  {
    #region Конструкторы

    #region С заданным именем поля

    /// <summary>
    /// Создает фильтр для выражения.
    /// Список идентификаторов не может быть пустым.
    /// Список <paramref name="ids"/> переводится в режим "Только чтение".
    /// </summary>
    /// <param name="expression">Выражение, возвращающее числовой идентификатор</param>
    /// <param name="ids">Список идентификаторов. Не может быть пустым</param>
    public IdsFilter(DBxExpression expression, IdList ids)
      : base(expression)
    {
#if DEBUG
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif
      if (ids.Count == 0)
        throw new ArgumentException("Массив идентификаторов не может быть пустым", "ids");

      ids.SetReadOnly();
      _Ids = ids;
    }

    /// <summary>
    /// Создает фильтр для заданного поля.
    /// Список идентификаторов не может быть пустым.
    /// Список <paramref name="ids"/> переводится в режим "Только чтение".
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="ids">Список идентификаторов. Не может быть пустым</param>
    public IdsFilter(string columnName, IdList ids)
      : this(new DBxColumn(columnName), ids)
    {
    }

    /// <summary>
    /// Создает фильтр для выражения.
    /// Массив идентификаторов не может быть пустым и не может содержать значения 0.
    /// </summary>
    /// <param name="expression">Выражение, возвращающее числовой идентификатор</param>
    /// <param name="ids">Массив идентификаторов</param>
    public IdsFilter(DBxExpression expression, Int32[] ids)
      : this(expression, new IdList(ids))
    {
    }

    /// <summary>
    /// Создает фильтр для заданного поля.
    /// Массив идентификаторов не может быть пустым и не может содержать значения 0.
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="ids">Массив идентификаторов</param>
    public IdsFilter(string columnName, Int32[] ids)
      : this(new DBxColumn(columnName), new IdList(ids))
    {
    }

    /// <summary>
    /// Создает фильтр для выражения.
    /// Идентификатор не может быть равен 0.
    /// </summary>
    /// <param name="expression">Выражение, возвращающее числовой идентификатор</param>
    /// <param name="Id">Идентификатор</param>
    public IdsFilter(DBxExpression expression, Int32 Id)
      : this(expression, new IdList(new Int32[1] { Id }))
    {
    }


    /// <summary>
    /// Создает фильтр для заданного поля.
    /// Идентификатор не может быть равен 0.
    /// </summary>
    /// <param name="ColumnName">Имя числового столбца</param>
    /// <param name="Id">Идентификатор</param>
    public IdsFilter(string ColumnName, Int32 Id)
      : this(new DBxColumn(ColumnName), new IdList(new Int32[1] { Id }))
    {
    }

    #endregion

    #region Для поля "Id"

    /// <summary>
    /// Создает фильтр для поля "Id".
    /// Список идентификаторов не может быть пустым.
    /// Список <paramref name="Ids"/> переводится в режим "Только чтение"
    /// </summary>
    /// <param name="Ids">Список идентификаторов</param>
    public IdsFilter(IdList Ids)
      : this("Id", Ids)
    {
    }

    /// <summary>
    /// Создает фильтр для поля "Id".
    /// Массив идентификаторов не может быть пустым и не может содержать значения 0.
    /// </summary>
    /// <param name="Ids">Массив идентификаторов</param>
    public IdsFilter(Int32[] Ids)
      : this("Id", Ids)
    {
    }

    /// <summary>
    /// Создает фильтр для поля "Id".
    /// Идентификатор не может быть равен 0.
    /// </summary>
    /// <param name="Id">Идентификатор</param>
    public IdsFilter(Int32 Id)
      : this("Id", Id)
    {
    }

    #endregion

    #endregion

    #region Свойства

    /// <summary>
    /// Массив идентификаторов.
    /// Не может быть пустым и содержать значения 0.
    /// </summary>
    public IdList Ids { get { return _Ids; } }
    private IdList _Ids;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод IdList.Contains().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      Int32 v = DataTools.GetInt(Expression.GetValue(rowValues, true));
      return Ids.Contains(v);
    }

    #endregion

    #region Статический метод

#if XXX
    /// <summary>
    /// Создание фильтра для ссылочного поля по массиву идентификаторов
    /// В отличие от конструктора IdsFilter() допускает пустой массив в качестве
    /// аргумента и наличие 0 в списке идентификаторов.
    /// Если массив идентификаторов пуст, то возвращает объект фильтра null.
    /// Если массив содержит идентификатор 0, то возвращается OR-фильтр из фильтра
    /// ISNULL и оставшего списка идентификаторов. Если массив состоит из единственного
    /// идентификатора 0, то возвращается только фильтр ISNULL
    /// </summary>
    /// <param name="ColumnName"></param>
    /// <param name="Values"></param>
    /// <returns></returns>
    public static DBxFilter Create(string ColumnName, Int32[] Ids)
    {
      if (Ids == null)
        return null;
      if (Ids.Length == 0)
        return null;
      int pZero = Array.IndexOf<Int32>(Ids, 0);
      if (pZero < 0)
        return new IdsFilter(ColumnName, Ids); // обычный конструктор

      // Есть идентификатор 0
      if (Ids.Length == 1)
        // только 0 и есть
        return new ValueFilter(ColumnName, null, typeof(Int32));

      // Требуется OR-фильтр

      // Собираем массив Values без идентификатора 0
      Int32[] Ids2 = new Int32[Ids.Length - 1];
      for (int i = 0; i < pZero; i++)
        Ids2[i] = Ids[i];
      for (int i = pZero + 1; i < Ids.Length; i++)
        Ids2[i - 1] = Ids[i];

      return new OrFilter(new ValueFilter(ColumnName, null, typeof(Int32)),
        new IdsFilter(ColumnName, Ids2));
    }
#endif

    #endregion
  }

  /// <summary>
  /// Фильтр по массиву значений произвольного типа вида "Поле/Выражение IN (Значение1, Значение2, ...)".
  /// Для числовых полей идентификаторов используйте IdsFilter.
  /// </summary>
  [Serializable]
  public class ValuesFilter : DBxSingleExpressionFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <param name="values">Допустимые значения. Не может быть пустым массивом</param>
    /// <param name="columnType">Тип данных</param>
    public ValuesFilter(DBxExpression expression, Array values, DBxColumnType columnType)
      : base(expression)
    {
#if DEBUG
      if (values == null)
        throw new ArgumentNullException("values");
#endif
      if (values.Length == 0)
        throw new ArgumentException("Список значений пуст", "values");

      _Values = values;
      _ColumnType = columnType;
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <param name="values">Допустимые значения. Не может быть пустым массивом</param>
    public ValuesFilter(DBxExpression expression, Array values)
      : this(expression, values, DBxColumnType.Unknown)
    {
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца таблицы данных</param>
    /// <param name="values">Допустимые значения. Не может быть пустым массивом</param>
    public ValuesFilter(string columnName, Array values)
      : this(new DBxColumn(columnName), values)
    {
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца таблицы данных</param>
    /// <param name="values">Допустимые значения. Не может быть пустым массивом</param>
    /// <param name="columnType">Тип данных</param>
    public ValuesFilter(string columnName, Array values, DBxColumnType columnType)
      : this(new DBxColumn(columnName), values, columnType)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Допустимые значения
    /// </summary>
    public Array Values { get { return _Values; } }
    private Array _Values;

    /// <summary>
    /// Тип данных.
    /// Имеет смысл задавать только в случе, когда тип данных в массиве не соответствует типу данных столбца,
    /// например, для столбца типа Guid, а Values содержит строки
    /// </summary>
    public DBxColumnType ColumnType { get { return _ColumnType; } }
    private DBxColumnType _ColumnType;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Сравнивает значение поля с каждым значением в списке Values и возвращает true в случае обнаружения совпадения.
    /// Использует метод Comparer.DefaultInvariant.Compare() для сравнения.
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      if (Values.Length == 0)
        return false; // в текущей реализации не может быть никогда

      object v = Expression.GetValue(rowValues, true);
      if (v is DBNull || Object.ReferenceEquals(v, null))
        v = DataTools.GetEmptyValue(Values.GetValue(0).GetType()); // ?? надо ли?

      for (int i = 0; i < Values.Length; i++)
      {
        int res = Comparer.DefaultInvariant.Compare(v, Values.GetValue(i));
        if (res == 0)
          return true;
      }

      return false;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по подзапросу в виде "Поле/Выражение IN (SELECT ...)".
  /// Этот фильтр не должен использоваться в автономных просмотрах DataView, так как не поддерживает метод TestFilter()
  /// </summary>
  [Serializable]
  public class InSelectFilter : DBxSingleExpressionFilter // TODO: Пока не работает, т.к. не знаю, как форматировать подзапросы
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <param name="selectInfo">Подзапрос</param>
    /// <param name="columnType">Тип данных</param>
    public InSelectFilter (DBxExpression expression, DBxSelectInfo selectInfo, DBxColumnType columnType)
      : base(expression)
    {
#if DEBUG
      if (selectInfo == null)
        throw new ArgumentNullException("selectInfo");
#endif
      if(selectInfo.Expressions.Count!=1)
        throw new ArgumentException("В подзапросе должно в результат входить только одно поле/выражение", "selectInfo");
      if (selectInfo.OrderBy!=null)
        throw new ArgumentException("В подзапросе не должно быть выражения ORDER BY", "selectInfo");
      if (selectInfo.MaxRecordCount!=0)
        throw new ArgumentException("В подзапросе не должно быть ограничения на количество записей", "selectInfo");
      if (selectInfo.HasGroupBy)
        throw new ArgumentException("В подзапросе не должно быть выражения GROUP BY", "selectInfo");


      _SelectInfo = selectInfo;
      _ColumnType = columnType;
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <param name="selectInfo">Подзапрос</param>
    public InSelectFilter(DBxExpression expression, DBxSelectInfo selectInfo)
      : this(expression, selectInfo, DBxColumnType.Unknown)
    {
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца таблицы данных</param>
    /// <param name="selectInfo">Подзапрос</param>
    public InSelectFilter(string columnName, DBxSelectInfo selectInfo)
      : this(new DBxColumn(columnName), selectInfo)
    {
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца таблицы данных</param>
    /// <param name="selectInfo">Подзапрос</param>
    /// <param name="columnType">Тип данных</param>
    public InSelectFilter(string columnName, DBxSelectInfo selectInfo, DBxColumnType columnType)
      : this(new DBxColumn(columnName), selectInfo, columnType)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Подзапрос
    /// </summary>
    public DBxSelectInfo SelectInfo { get { return _SelectInfo; } }
    private DBxSelectInfo _SelectInfo;

    /// <summary>
    /// Тип данных.
    /// Имеет смысл задавать только в случе, когда тип данных в массиве не соответствует типу данных столбца,
    /// например, для столбца типа Guid, а Values содержит строки
    /// </summary>
    public DBxColumnType ColumnType { get { return _ColumnType; } }
    private DBxColumnType _ColumnType;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Метод не реализован
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Комбинация из нескольких условий, объединенных условием "И"
  /// </summary>
  [Serializable]
  public class AndFilter : DBxFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает AND-фильтр для двух условий
    /// </summary>
    /// <param name="filter1">Первый объединяемый фильтр. Не может быть null</param>
    /// <param name="filter2">Второй объединяемый фильтр. Не может быть null</param>
    public AndFilter(DBxFilter filter1, DBxFilter filter2)
    {
#if DEBUG
      if (filter1 == null)
        throw new ArgumentNullException("filter1");
      if (filter2 == null)
        throw new ArgumentNullException("filter2");
#endif

      if (filter1 is AndFilter)
      {
        if (filter2 is AndFilter)
        {
          _Filters = new DBxFilter[((AndFilter)filter1)._Filters.Length + ((AndFilter)filter2)._Filters.Length];
          ((AndFilter)filter1)._Filters.CopyTo(_Filters, 0);
          ((AndFilter)filter2)._Filters.CopyTo(_Filters, ((AndFilter)filter1)._Filters.Length);
        }
        else
        {
          _Filters = new DBxFilter[((AndFilter)filter1)._Filters.Length + 1];
          ((AndFilter)filter1)._Filters.CopyTo(_Filters, 0);
          _Filters[_Filters.Length - 1] = filter2;
        }
      }
      else
      {
        if (filter2 is AndFilter)
        {
          _Filters = new DBxFilter[1 + ((AndFilter)filter2)._Filters.Length];
          _Filters[0] = filter1;
          ((AndFilter)filter2)._Filters.CopyTo(_Filters, 1);
        }
        else
        {
          _Filters = new DBxFilter[2];
          _Filters[0] = filter1;
          _Filters[1] = filter2;
        }
      }
    }

    /// <summary>
    /// Создает AND-фильтр из массива фильтров.
    /// Для создания фильтра из списка неизвестной заранее длины используйте статические методы
    /// FromArray() и FromList().
    /// </summary>
    /// <param name="filters">Входящий фильтры. Массив не может содержать меньше двух фильтров</param>
    public AndFilter(DBxFilter[] filters)
    {
#if DEBUG
      if (filters == null)
        throw new ArgumentNullException("filters");
      if (Array.IndexOf<DBxFilter>(filters, null) >= 0)
        throw new ArgumentException("Массив фильтров не может содержать значения null", "filters");
#endif
      _Filters = filters;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список фильтров
    /// </summary>
    public DBxFilter[] Filters { get { return _Filters; } }
    private DBxFilter[] _Filters;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Признак вырождения фильтра.
    /// Если все входящие фильтры являются вырожденными (возвращают AlwaysTrue), то возвращает AlwaysTrue.
    /// Если хотя бы один из фильтров возвращает AlwaysFalse, то возвращает AlwaysFalse
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        bool AllAreTrue = true;
        for (int i = 0; i < _Filters.Length; i++)
        {
          switch (_Filters[i].Degeneration)
          {
            case DBxFilterDegeneration.AlwaysFalse:
              return DBxFilterDegeneration.AlwaysFalse;
            case DBxFilterDegeneration.None:
              AllAreTrue = false;
              break;
          }
        }

        if (AllAreTrue)
          return DBxFilterDegeneration.AlwaysTrue;
        else
          return DBxFilterDegeneration.None;
      }
    }

    /// <summary>
    /// Добавляет в список поля входящих фильтров, рекурсивно вызывая для них GetColumnNames()
    /// </summary>
    /// <param name="list">Список для заполнения</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < _Filters.Length; i++)
        _Filters[i].GetColumnNames(list);
    }

    /// <summary>
    /// Сначала добавляет себя в список фильтров, затем рекурсивно вызывает метод для всех входящий фильтров
    /// </summary>
    /// <param name="list">Заполняемый список фильтров. Не может быть null</param>
    public override void GetAllFilters(List<DBxFilter> list)
    {
      base.GetAllFilters(list);
      for (int i = 0; i < _Filters.Length; i++)
        _Filters[i].GetAllFilters(list);
    }

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод для всех входящих в список фильтров и возвращает false, если хотя бы один фильтр вернул false.
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      for (int i = 0; i < _Filters.Length; i++)
      {
        if (!_Filters[i].TestFilter(rowValues))
          return false;
      }
      return true;
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Создание AND-фильтра из массива фильтров по необходимости, если число
    /// фильтров в массиве больше 1. Если список содержит только один фильтр,
    /// то он возвращается. Если список пустой, возвращается null
    /// </summary>
    /// <param name="filters">Массив фильтров</param>
    /// <returns>Фильтр или null</returns>
    public static DBxFilter FromArray(DBxFilter[] filters)
    {
      if (filters == null)
        return null;

      // 18.02.2019
      // Проверяем вырожденные фильтры
      for (int i = 0; i < filters.Length; i++)
      {
        switch (filters[i].Degeneration)
        {
          case DBxFilterDegeneration.AlwaysTrue:
            // Есть вырожденный фильтр - нужно убрать часть фильтров
            List<DBxFilter> lst = new List<DBxFilter>(filters.Length);
            lst.AddRange(filters);
            return FromList(lst);

          case DBxFilterDegeneration.AlwaysFalse:
            return DummyFilter.AlwaysFalse;
        }
      }

      // Нет ни одного вырожденного фильтра

      switch (filters.Length)
      {
        case 0:
          return null;
        case 1:
          return filters[0];
        default:
          return new AndFilter(filters);
      }
    }

    /// <summary>
    /// Создание AND-фильтра из списка фильтров по необходимости, если число
    /// фильтров в массиве больше 1. Если список содержит только один фильтр,
    /// то он возвращается. Если список пустой, возвращается null
    /// </summary>
    /// <param name="filters">Список фильтров</param>
    /// <returns>Фильтр или null</returns>
    public static DBxFilter FromList(List<DBxFilter> filters)
    {
      if (filters == null)
        return null;

      // 18.02.2019
      // Проверяем вырожденность фильтров
      for (int i = 0; i < filters.Count; i++)
      {
        switch (filters[i].Degeneration)
        {
          case DBxFilterDegeneration.AlwaysTrue:
            // Убираем вырожденные фильтры
            List<DBxFilter> Filters2 = new List<DBxFilter>(filters.Count - 1);
            for (int j = 0; j < filters.Count; j++)
            {
              switch (filters[j].Degeneration)
              {
                case DBxFilterDegeneration.None:
                  Filters2.Add(filters[j]);
                  break;
                case DBxFilterDegeneration.AlwaysFalse:
                  return DummyFilter.AlwaysFalse;
              }
            }
            filters = Filters2;
            break;
        }
      }

      switch (filters.Count)
      {
        case 0:
          return null;
        case 1:
          return filters[0];
        default:
          return new AndFilter(filters.ToArray());
      }
    }

    #endregion
  }

  /// <summary>
  /// Комбинация из нескольких условий, объединенных условием "ИЛИ"
  /// </summary>
  [Serializable]
  public class OrFilter : DBxFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает фильтр на основании двух существующих.
    /// Оба фильтра должны быть заданы
    /// </summary>
    /// <param name="filter1">Первый фильтр</param>
    /// <param name="filter2">Второй фильтр</param>
    public OrFilter(DBxFilter filter1, DBxFilter filter2)
    {
#if DEBUG
      if (filter1 == null)
        throw new ArgumentNullException("filter1");
      if (filter2 == null)
        throw new ArgumentNullException("filter2");
#endif

      if (filter1 is OrFilter)
      {
        if (filter2 is OrFilter)
        {
          _Filters = new DBxFilter[((OrFilter)filter1)._Filters.Length + ((OrFilter)filter2)._Filters.Length];
          ((OrFilter)filter1)._Filters.CopyTo(_Filters, 0);
          ((OrFilter)filter2)._Filters.CopyTo(_Filters, ((OrFilter)filter1)._Filters.Length);
        }
        else
        {
          _Filters = new DBxFilter[((OrFilter)filter1)._Filters.Length + 1];
          ((OrFilter)filter1)._Filters.CopyTo(_Filters, 0);
          _Filters[_Filters.Length - 1] = filter2;
        }
      }
      else
      {
        if (filter2 is OrFilter)
        {
          _Filters = new DBxFilter[1 + ((OrFilter)filter2)._Filters.Length];
          _Filters[0] = filter1;
          ((OrFilter)filter2)._Filters.CopyTo(_Filters, 1);
        }
        else
        {
          _Filters = new DBxFilter[2];
          _Filters[0] = filter1;
          _Filters[1] = filter2;
        }
      }
    }

    /// <summary>
    /// Создает фильтр на основании массива.
    /// Массив не может содержать значения null. 
    /// </summary>
    /// <param name="filters">Массив фильтров</param>
    public OrFilter(DBxFilter[] filters)
    {
#if DEBUG
      if (filters == null)
        throw new ArgumentNullException("filters");
      if (Array.IndexOf<DBxFilter>(filters, null) >= 0)
        throw new ArgumentException("Массив фильтров не может создержать значения null", "filters");
#endif
      _Filters = filters;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список фильтров, входлящих в объединение
    /// </summary>
    public DBxFilter[] Filters { get { return _Filters; } }
    private DBxFilter[] _Filters;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Добавляет в список поля входящих фильтров, используя рекурсивный вызов
    /// </summary>
    /// <param name="list">Список для заполнения</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < _Filters.Length; i++)
        _Filters[i].GetColumnNames(list);
    }

    /// <summary>
    /// Сначала добавляет себя в список фильтров, затем рекурсивно вызывает метод для всех входящий фильтров
    /// </summary>
    /// <param name="list">Заполняемый список фильтров. Не может быть null</param>
    public override void GetAllFilters(List<DBxFilter> list)
    {
      base.GetAllFilters(list);
      for (int i = 0; i < _Filters.Length; i++)
        _Filters[i].GetAllFilters(list);
    }

    /// <summary>
    /// Признак вырождения фильтра.
    /// Если хотя бы один входящий фильтр является вырожденным (возвращает AlwaysTrue), то возвращает AlwaysTrue.
    /// Если все фильтры возвращают AlwaysFalse, то возвращает AlwaysFalse
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        bool AllAreFalse = true;
        for (int i = 0; i < _Filters.Length; i++)
        {
          switch (_Filters[i].Degeneration)
          {
            case DBxFilterDegeneration.AlwaysTrue:
              return DBxFilterDegeneration.AlwaysTrue;
            case DBxFilterDegeneration.None:
              AllAreFalse = false;
              break;
          }
        }

        if (AllAreFalse)
          return DBxFilterDegeneration.AlwaysFalse;
        else
          return DBxFilterDegeneration.None;
      }
    }



    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод для всех входящих в список фильтров и возвращает true, если хотя бы один фильтр вернул true.
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      for (int i = 0; i < _Filters.Length; i++)
      {
        if (_Filters[i].TestFilter(rowValues))
          return true;
      }
      return false;
    }


    #endregion

    #region Статические методы

    /// <summary>
    /// Создание OR-фильтра из массива фильтров по необходимости, если число
    /// фильтров в массиве больше 1. Если список содержит только один фильтр,
    /// то он возвращается. Если список пустой, возвращается null.
    /// Если хотя бы один из фильтров является "вырожденным" (DBxFilter.AlwaysTrue() возвращает true),
    /// то возвращается null.
    /// </summary>
    /// <param name="filters">Фильтры</param>
    /// <returns>OrFilter или один из исходных фильтров</returns>
    public static DBxFilter FromArray(DBxFilter[] filters)
    {
      if (filters == null)
        return null;

      // 18.02.2019
      // Учитываем вырожденные фильтры
      for (int i = 0; i < filters.Length; i++)
      {
        switch (filters[i].Degeneration)
        {
          case DBxFilterDegeneration.AlwaysTrue:
            return null;
          case DBxFilterDegeneration.AlwaysFalse:
            // Есть вырожденный фильтр - нужно убрать часть фильтров
            List<DBxFilter> lst = new List<DBxFilter>(filters.Length);
            lst.AddRange(filters);
            return FromList(lst);
        }
      }

      // Нет вырожденных фильтров

      switch (filters.Length)
      {
        case 0:
          return DummyFilter.AlwaysFalse; // 24.07.2019
        case 1:
          return filters[0];
        default:
          return new OrFilter(filters);
      }
    }

    /// <summary>
    /// Создание OR-фильтра из списка фильтров по необходимости, если число
    /// фильтров в массиве больше 1. Если список содержит только один фильтр,
    /// то он возвращается. Если список пустой, возвращается null
    /// Если хотя бы один из фильтров является "вырожденным" (DBxFilter.AlwaysTrue() возвращает true),
    /// то возвращается null.
    /// </summary>
    /// <param name="filters">Фильтры</param>
    /// <returns>OrFilter или один из исходных фильтров</returns>
    public static DBxFilter FromList(List<DBxFilter> filters)
    {
      if (filters == null)
        return DummyFilter.AlwaysFalse; // 24.07.2019

      // 18.02.2019
      // Учитываем вырожденные фильтры
      for (int i = 0; i < filters.Count; i++)
      {
        switch (filters[i].Degeneration)
        {
          case DBxFilterDegeneration.AlwaysTrue:
            return null;
          case DBxFilterDegeneration.AlwaysFalse:
            // Убираем вырожденные фильтры
            List<DBxFilter> Filters2 = new List<DBxFilter>(filters.Count - 1);
            for (int j = 0; j < filters.Count; j++)
            {
              switch (filters[j].Degeneration)
              {
                case DBxFilterDegeneration.None:
                  Filters2.Add(filters[j]);
                  break;
                case DBxFilterDegeneration.AlwaysTrue:
                  return DummyFilter.AlwaysTrue;
              }
            }
            filters = Filters2;
            break;
        }
      }

      // Вырожденных фильтров нет

      switch (filters.Count)
      {
        case 0:
          return DummyFilter.AlwaysFalse; // 24.07.2019
        case 1:
          return filters[0];
        default:
          return new OrFilter(filters.ToArray());
      }
    }

    #endregion
  }

  /// <summary>
  /// Инверсия условия (NOT)
  /// </summary>
  [Serializable]
  public class NotFilter : DBxFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр "NOT".
    /// </summary>
    /// <param name="baseFilter">Базовый фильтр. Не может быть null</param>
    public NotFilter(DBxFilter baseFilter)
    {
#if DEBUG
      if (baseFilter == null)
        throw new ArgumentNullException("baseFilter");
#endif
      _BaseFilter = baseFilter;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основновной фильтр, результат которого инвертируется
    /// </summary>
    public DBxFilter BaseFilter { get { return _BaseFilter; } }
    private DBxFilter _BaseFilter;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Добавляет в список поля из BaseFilter
    /// </summary>
    /// <param name="list">Список для заполнения</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      BaseFilter.GetColumnNames(list);
    }

    /// <summary>
    /// Сначала добавляет себя в список фильтров, затем рекурсивно вызывает метод для базового фильтра
    /// </summary>
    /// <param name="list">Заполняемый список фильтров. Не может быть null</param>
    public override void GetAllFilters(List<DBxFilter> list)
    {
      base.GetAllFilters(list);
      BaseFilter.GetAllFilters(list);
    }

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод базового фильтра и возвращает инвертированное значение.
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      return !_BaseFilter.TestFilter(rowValues);
    }

    /// <summary>
    /// Возвращает признак вырождения, если базовый фильтр является вырожденным
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        switch (BaseFilter.Degeneration)
        {
          case DBxFilterDegeneration.AlwaysTrue:
            return DBxFilterDegeneration.AlwaysFalse;
          case DBxFilterDegeneration.AlwaysFalse:
            return DBxFilterDegeneration.AlwaysTrue;
          default:
            return DBxFilterDegeneration.None;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр числового поля по диапазону значений
  /// Поддерживаются полуоткрытые интервалы
  /// </summary>
  [Serializable]
  public class NumRangeFilter : DBxSingleExpressionFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="expression">Выражение, возвращающее числовое значение</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public NumRangeFilter(DBxExpression expression, decimal? minValue, decimal? maxValue)
      : base(expression)
    {
      _MinValue = minValue;
      _MaxValue = maxValue;
    }


    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public NumRangeFilter(string columnName, decimal? minValue, decimal? maxValue)
      : this(new DBxColumn(columnName), minValue, maxValue)
    {
    }

    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public NumRangeFilter(string columnName, double? minValue, double? maxValue)
      : this(columnName, (decimal?)minValue, (decimal?)maxValue)
    {
    }

    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public NumRangeFilter(string columnName, float? minValue, float? maxValue)
      : this(columnName, (decimal?)minValue, (decimal?)maxValue)
    {
    }

    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public NumRangeFilter(string columnName, int? minValue, int? maxValue)
      : this(columnName, (decimal?)minValue, (decimal?)maxValue)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Минимальное значение или null
    /// </summary>
    public decimal? MinValue { get { return _MinValue; } }
    private decimal? _MinValue;

    /// <summary>
    /// Максимальное значение или null
    /// </summary>
    public decimal? MaxValue { get { return _MaxValue; } }
    private decimal? _MaxValue;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Если не задано ни MinValue ни MaxValue, то возвращает AlwaysTrue.
    /// Если задано оба ограничения, но MinValue больше MaxValue, возвращает AlwaysFalse.
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        if (MinValue.HasValue && MaxValue.HasValue)
        {
          if (MinValue.Value > MaxValue.Value)
            return DBxFilterDegeneration.AlwaysFalse;
        }
        else if ((!MinValue.HasValue) && (!MaxValue.HasValue))
          return DBxFilterDegeneration.AlwaysTrue;

        return DBxFilterDegeneration.None;
      }
    }

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      if (_MinValue.HasValue || _MaxValue.HasValue)
      {
        decimal v = DataTools.GetDecimal(Expression.GetValue(rowValues, false));
        if (_MinValue.HasValue)
        {
          if (v < _MinValue.Value)
            return false;
        }
        if (_MaxValue.HasValue)
        {
          if (v > _MaxValue.Value)
            return false;
        }
      }
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по полю, содержащему дату, по диапазону дат
  /// Поле может содержать компонент времени (DateTime)
  /// Поддерживаются полуоткрытые интервалы
  /// </summary>
  [Serializable]
  public class DateRangeFilter : DBxSingleExpressionFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="expression">Выражение, возвращающее дату</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public DateRangeFilter(DBxExpression expression, DateTime? minValue, DateTime? maxValue)
      : base(expression)
    {
      if (minValue.HasValue)
        _MinValue = minValue.Value.Date;
      if (maxValue.HasValue)
        _MaxValue = maxValue.Value.Date;
    }

    /// <summary>
    /// Конструктор диапазона значений поля. Если <paramref name="minValue"/> или <paramref name="maxValue"/> равны
    /// null, то диапазон считается (полу)открытым.
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    /// <param name="minValue">Минимальное значение или null</param>
    /// <param name="maxValue">Максимальное значение или null</param>
    public DateRangeFilter(string columnName, DateTime? minValue, DateTime? maxValue)
      : this(new DBxColumn(columnName), minValue, maxValue)
    {
    }

    /// <summary>
    /// Конструктор диапазона, равного одному году
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="year">Год</param>
    public DateRangeFilter(string columnName, int year)
      : this(columnName, DataTools.BottomOfYear(year), DataTools.EndOfYear(year))
    {
    }

    /// <summary>
    /// Конструктор диапазона, равного одному месяцу
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц (1-12)</param>
    public DateRangeFilter(string columnName, int year, int month)
      : this(columnName, DataTools.BottomOfMonth(year, month), DataTools.EndOfMonth(year, month))
    {
    }

    /// <summary>
    /// Конструктор диапазона, равного одному дню
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц (1-12)</param>
    /// <param name="day">День</param>
    public DateRangeFilter(string columnName, int year, int month, int day)
      : this(columnName, new DateTime(year, month, day), new DateTime(year, month, day))
    {
    }

    /// <summary>
    /// Конструктор диапазона, равного одному дню
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="date">Дата</param>
    public DateRangeFilter(string columnName, DateTime date)
      : this(columnName, date, date)
    {
    }

    /// <summary>
    /// Конструктор для интервала дат
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="range">Диапазон дат</param>
    public DateRangeFilter(string columnName, DateRange range)
      : this(columnName, range.FirstDate, range.LastDate)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Минимальное значение диапазона или null
    /// </summary>
    public DateTime? MinValue { get { return _MinValue; } }
    private DateTime? _MinValue;

    /// <summary>
    /// Максимальное значение диапазона или null
    /// </summary>
    public DateTime? MaxValue { get { return _MaxValue; } }
    private DateTime? _MaxValue;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Если не задано ни MinValue ни MaxValue, то возвращает AlwaysTrue.
    /// Если задано оба ограничения, но MinValue больше MaxValue, возвращает AlwaysFalse.
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        if (MinValue.HasValue && MaxValue.HasValue)
        {
          if (MinValue.Value > MaxValue.Value)
            return DBxFilterDegeneration.AlwaysFalse;
        }
        else if ((!MinValue.HasValue) && (!MaxValue.HasValue))
          return DBxFilterDegeneration.AlwaysTrue;

        return DBxFilterDegeneration.None;
      }
    }

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод DataTools.DateInRange().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      if (_MinValue.HasValue || _MaxValue.HasValue)
      {
        DateTime? v = DataTools.GetNullableDateTime(Expression.GetValue(rowValues, false));
        if (!v.HasValue)
          return false;

        return DataTools.DateInRange(v.Value.Date, _MinValue, _MaxValue);
      }
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон дат.
  /// В фильтр входят строки, в диапазон дат которых попадает указанная дата
  /// Поддерживаются полуоткрытые интервалы
  /// Компоненты времени не поддерживаются
  /// </summary>
  [Serializable]
  public class DateRangeInclusionFilter : DBxTwoExpressionsFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression1">Выражение, возвращающее значение типа "Дата", задающее начало диапазона</param>
    /// <param name="expression2">Выражение, возвращающее значение типа "Дата", задающее задающего конец диапазона</param>
    /// <param name="value">Дата, попадание которой в диапазон проверяется</param>
    public DateRangeInclusionFilter(DBxExpression expression1, DBxExpression expression2, DateTime value)
      : base(expression1, expression2)
    {
      _Value = value.Date;
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя первого столбца типа "Дата", задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя второго столбца типа "Дата", задающего конец диапазона</param>
    /// <param name="value">Дата, попадание которой в диапазон проверяется</param>
    public DateRangeInclusionFilter(string firstColumnName, string lastColumnName, DateTime value)
      : this(new DBxColumn(firstColumnName), new DBxColumn(lastColumnName), value)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Дата, попадание которой в диапазон проверяется
    /// </summary>
    public DateTime Value { get { return _Value; } }
    private DateTime _Value;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод DataTools.DateInRange().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      DateTime? v1 = DataTools.GetNullableDateTime(Expression1.GetValue(rowValues, false));
      DateTime? v2 = DataTools.GetNullableDateTime(Expression2.GetValue(rowValues, false));

      return DataTools.DateInRange(Value, v1, v2);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон дат.
  /// В фильтр входят строки, в диапазон дат которых попадает любая из дат в указанном диапазоне
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале
  /// Компоненты времени не поддерживаются
  /// </summary>
  [Serializable]
  public class DateRangeCrossFilter : DBxTwoExpressionsFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает фильтр.
    /// Эта версия поддерживает полуоткрытый проверяемый интервал
    /// </summary>
    /// <param name="expression1">Выражение, возвращающее значение типа "Дата", для первого поля</param>
    /// <param name="expression2">Выражение, возвращающее значение типа "Дата", для второго поля</param>
    /// <param name="firstDate">Проверяемый диапазон дат - начальная дата</param>
    /// <param name="lastDate">Проверяемый диапазон дат - конечная дата</param>
    public DateRangeCrossFilter(DBxExpression expression1, DBxExpression expression2, DateTime? firstDate, DateTime? lastDate)
      : base(expression1, expression2)
    {
#if DEBUG
      if (!(firstDate.HasValue || lastDate.HasValue))
        throw new ArgumentNullException("firstDate", "Хотя бы одна из дат диапазона должна быть задана");
#endif

      if (firstDate.HasValue)
        _FirstDate = firstDate.Value.Date;
      if (lastDate.HasValue)
        _LastDate = lastDate.Value.Date;
    }

    /// <summary>
    /// Создает фильтр.
    /// Эта версия поддерживает полуоткрытый проверяемый интервал
    /// </summary>
    /// <param name="firstColumnName">Имя первого поля</param>
    /// <param name="lastColumnName">Имя второго поля</param>
    /// <param name="firstDate">Проверяемый диапазон дат - начальная дата</param>
    /// <param name="lastDate">Проверяемый диапазон дат - конечная дата</param>
    public DateRangeCrossFilter(string firstColumnName, string lastColumnName, DateTime? firstDate, DateTime? lastDate)
      : this(new DBxColumn(firstColumnName), new DBxColumn(lastColumnName), firstDate, lastDate)
    {
    }

    /// <summary>
    /// Создает фильтр.
    /// Эта версия поддерживает закрытый проверяемый интервал
    /// </summary>
    /// <param name="firstColumnName">Имя первого поля</param>
    /// <param name="lastColumnName">Имя второго поля</param>
    /// <param name="range">Проверяемый диапазон дат</param>
    public DateRangeCrossFilter(string firstColumnName, string lastColumnName, DateRange range)
      : this(firstColumnName, lastColumnName, range.FirstDate, range.LastDate)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Проверяемый диапазон дат - начальная дата
    /// </summary>
    public DateTime? FirstDate { get { return _FirstDate; } }
    private DateTime? _FirstDate;

    /// <summary>
    /// Проверяемый диапазон дат - конечная дата
    /// </summary>
    public DateTime? LastDate { get { return _LastDate; } }
    private DateTime? _LastDate;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод DataTools.DateRangeCrosses().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      DateTime? v1 = DataTools.GetNullableDateTime(Expression1.GetValue(rowValues, false));
      DateTime? v2 = DataTools.GetNullableDateTime(Expression2.GetValue(rowValues, false));

      return DataTools.DateRangeCrossed(v1, v2, FirstDate, LastDate);
    }

    #endregion
  }

  /// <summary>
  /// Проверка поля на значение NULL
  /// </summary>
  [Serializable]
  public class NotNullFilter : /*ValueFilter*/ CompareFilter /* 14.11.2019 */
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Проверяемое выражение</param>
    /// <param name="columnType">Тип данных, которые может возвращать выражение</param>
    public NotNullFilter(DBxExpression expression, DBxColumnType columnType)
      : base(expression, new DBxConst(null, columnType), CompareKind.NotEqual)
    {
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="dataType">Тип данных, которые хранятся в поле</param>
    public NotNullFilter(string columnName, Type dataType)
      : this(new DBxColumn(columnName), DBxTools.DataTypeToColumnType(dataType))
    {
    }

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="columnType">Тип данных, которые содержит поле</param>
    public NotNullFilter(string columnName, DBxColumnType columnType)
      : this(new DBxColumn(columnName), columnType)
    {
    }

    #endregion
  }

#if XXXXXXX
    [Serializable]
/// <summary>
  /// Фильтр по шаблону LIKE
  /// "%" в шаблоне означает "Любые символы"
  /// !!! 1. Надо переделать шаблонный символ на "*"
  /// !!! 2. Обрабатывать символ "?"
  /// !!! 3. Обрабатывать символ "["
  /// </summary>
  public class LikeFilter : DataFilter
  {
  #region Конструктор

    public LikeFilter(string ColumnName, string Template)
    {
#if DEBUG
      CheckColumnName(ColumnName);
#endif
      FColumnName = ColumnName;
      FTemplate = Template;
    }

  #endregion

  #region Свойства

    public string ColumnName { get { return FColumnName; } }
    private string FColumnName;

    public string Template { get { return FTemplate; } }
    private string FTemplate;

  #endregion

  #region Переопределяемые методы

    public override string GetSQL(DataSQLFormatInfo Format)
    {
      if (Format.Kind==DataSQLKind.DataView)
        return Format.GetColumnName(ColumnName) + " LIKE \'" + Template.Replace('%', '*') + "\'";
      else
        return Format.GetColumnName(ColumnName) + " LIKE \"" + Template + "\"";
    }

  #endregion
  }
#endif

  /// <summary>
  /// Строковый фильтр с учетом регистра или без него.
  /// Если требуется "сравнительный" фильтр для строки, используйте ValueFilter
  /// </summary>
  [Serializable]
  public class StringValueFilter : DBxSingleExpressionFilter
  {
    #region Конструктор

    /// <summary>
    /// Эта версия конструктора может создать фильтр с учетом регистра или без учета
    /// </summary>
    /// <param name="expression">Выражение, возвращающее строку</param>
    /// <param name="value">Начало строки</param>
    /// <param name="ignoreCase">Если true, то регистр будет игнорироваться</param>
    public StringValueFilter(DBxExpression expression, string value, bool ignoreCase)
      : base(expression)
    {
      if (value == null)
        _Value = String.Empty;
      else
        _Value = value;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Эта версия конструктора может создать фильтр с учетом регистра или без учета
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="value">Начало строки</param>
    /// <param name="ignoreCase">Если true, то регистр будет игнорироваться</param>
    public StringValueFilter(string columnName, string value, bool ignoreCase)
      : this(new DBxColumn(columnName), value, ignoreCase)
    {
    }

    /// <summary>
    /// Эта версия конструктора создаст фильтр с точным сравнением с учетом регистра.
    /// Такой фильтр полностью эквивалентен ValueFilter
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="value">Начало строки</param>
    public StringValueFilter(string columnName, string value)
      : this(columnName, value, false)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Сравниваемое значение
    /// </summary>
    public string Value { get { return _Value; } }
    private string _Value;

    /// <summary>
    /// Нужно ли игнорировать регистр символов
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод String.Compare().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      string v = DataTools.GetString(Expression.GetValue(rowValues, false));
      return String.Equals(v, Value, IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
    }

    #endregion
  }


  /// <summary>
  /// Упрощенная реализация строкового фильтра LIKE. Тестирует значение строкового
  /// поля, что оно начинается с заданных символов
  /// Все символы в строке Value трактуются "как есть", то есть без шаблонных
  /// символов. Если строка Value содержит символы, трактуемые провайдером базы
  /// данных как шаблонные, то шаблон будет изменен так, чтобы символы трактовались
  /// буквально.
  /// Строка Value может быть пустой строкой.
  /// Поддерживается сравнение с учетом регистра или без него
  /// </summary>
  [Serializable]
  public class StartsWithFilter : DBxSingleExpressionFilter
  {
    #region Конструктор

    /// <summary>
    /// Эта версия конструктора может создать фильтр с учетом регистра или без учета
    /// </summary>
    /// <param name="expression">Выражение, возвращающее строковое значение</param>
    /// <param name="value">Начало строки</param>
    /// <param name="ignoreCase">Если true, то регистр будет игнорироваться</param>
    public StartsWithFilter(DBxExpression expression, string value, bool ignoreCase)
      : base(expression)
    {
      if (value == null)
        _Value = String.Empty;
      else
        _Value = value;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Эта версия конструктора может создать фильтр с учетом регистра или без учета
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="value">Начало строки</param>
    /// <param name="ignoreCase">Если true, то регистр будет игнорироваться</param>
    public StartsWithFilter(string columnName, string value, bool ignoreCase)
      : this(new DBxColumn(columnName), value, ignoreCase)
    {
    }

    /// <summary>
    /// Эта версия конструктора создаст фильтр с точным сравнением с учетом регистра
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="value">Начало строки</param>
    public StartsWithFilter(string columnName, string value)
      : this(columnName, value, false)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Строка, с которой должно начинаться значение поля
    /// </summary>
    public string Value { get { return _Value; } }
    private string _Value;

    /// <summary>
    /// Нужно ли игнорировать регистр символов
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Возвращает AlwaysTrue, если Value не задано
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        if (String.IsNullOrEmpty(Value))
          return DBxFilterDegeneration.AlwaysTrue;
        else
          return DBxFilterDegeneration.None;
      }
    }


    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод String.StartsWith().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      string v = DataTools.GetString(Expression.GetValue(rowValues, false));
      return Value.StartsWith(v, IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по подстроке. Извлекает из текстового поля подстроку в заданной
  /// позиции и сравнивает ее с заданной
  /// </summary>
  [Serializable]
  public class SubstringFilter : DBxSingleExpressionFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор фильтра.
    /// </summary>
    /// <param name="expression">Выражение, возвращающее строковое значение</param>
    /// <param name="startIndex">Начальная позиция. Первая позиция имеет индекс 0, а не 1</param>
    /// <param name="value">Сравниваемое значение. Не может быть пустой строкой</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр символов</param>
    public SubstringFilter(DBxExpression expression, int startIndex, string value, bool ignoreCase)
      : base(expression)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException("startIndex", startIndex, "Начальная позиция не может быть отрицательной");
      if (String.IsNullOrEmpty(value))
        throw new ArgumentNullException("value");
      _StartIndex = startIndex;
      _Value = value;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Конструктор фильтра.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="startIndex">Начальная позиция. Первая позиция имеет индекс 0, а не 1</param>
    /// <param name="value">Сравниваемое значение. Не может быть пустой строкой</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр символов</param>
    public SubstringFilter(string columnName, int startIndex, string value, bool ignoreCase)
      : this(new DBxColumn(columnName), startIndex, value, ignoreCase)
    {
    }

    /// <summary>
    /// Конструктор фильтра.
    /// Эта версия создает регистрочувствительный фильтр (IgnoreCase=false).
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="startIndex">Начальная позиция. Первая позиция имеет индекс 0, а не 1</param>
    /// <param name="value">Сравниваемое значение. Не может быть пустой строкой</param>
    public SubstringFilter(string columnName, int startIndex, string value)
      : this(columnName, startIndex, value, false)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Начальная позиция. Первая позиция имеет индекс 0, а не 1
    /// </summary>
    public int StartIndex { get { return _StartIndex; } }
    private int _StartIndex;

    /// <summary>
    /// Сравниваемое значение. Не может быть пустой строкой
    /// </summary>
    public string Value { get { return _Value; } }
    private string _Value;

    /// <summary>
    /// Нужно ли игнорировать регистр символов
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка условия фильтра для строки данных.
    /// Вызывает метод String.IndexOf().
    /// </summary>
    /// <param name="rowValues">Объект доступа к списку именованных значений. Обычно - DBxColumnValueArray</param>
    /// <returns>true, если условие фильтра выполняется</returns>
    public override bool TestFilter(INamedValuesAccess rowValues)
    {
      string v = DataTools.GetString(Expression.GetValue(rowValues, false));
      return Value.IndexOf(v, IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) >= 0;
    }

    #endregion
  }


  /// <summary>
  /// Фиктивный фильтр, который всегда возвращает TRUE или FALSE
  /// </summary>
  [Serializable]
  public class DummyFilter : DBxFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="isTrue">Если true, то условие фильтра всегда выполняется.
    /// Если false, условие никогда не выполняется</param>
    public DummyFilter(bool isTrue)
    {
      _IsTrue = isTrue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если true, то условие фильтра всегда выполняется.
    /// Если false, условие никогда не выполняется    
    /// </summary>
    public bool IsTrue { get { return _IsTrue; } }
    private bool _IsTrue;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="list">Игнорируется</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      // Нет полей
    }

    /// <summary>
    /// Возвращает IsTrue
    /// </summary>
    /// <param name="RowValues">Игнорируется</param>
    /// <returns>IsTrue</returns>
    public override bool TestFilter(INamedValuesAccess RowValues)
    {
      return IsTrue;
    }

    /// <summary>
    /// Возвращает AlwaysTrue или AlwaysFalse, так как фильтр является вырожденным
    /// </summary>
    public override DBxFilterDegeneration Degeneration
    {
      get
      {
        if (IsTrue)
          return DBxFilterDegeneration.AlwaysTrue;
        else
          return DBxFilterDegeneration.AlwaysFalse;
      }
    }

    #endregion

    #region Статические экземпляры

    /// <summary>
    /// Статический экземпляр фиктивного фильтра, пропускающего все строки без фильтрации
    /// </summary>
    public static readonly DummyFilter AlwaysTrue = new DummyFilter(true);

    /// <summary>
    /// Статический экземпляр фильтра, который не пропускает ни одной строки
    /// </summary>
    public static readonly DummyFilter AlwaysFalse = new DummyFilter(false);

    #endregion
  }

  /// <summary>
  /// Генератор фильтров по массивам идентификаторов.
  /// Проблема: Некоторые операции могут применяться к большому и заранее неизвестному
  /// числу записей, определяемых идентификаторами.
  /// Существует ограничение на число идентификаторов в SQL-запросе вида
  /// SELECT .... WHERE xxx IN [Id1, Id2, Id3, ....]
  /// В тоже время, выполнять отдельный запрос для каждого идентификатора также
  /// нецелесообразно.
  /// Существует некоторое оптимальное количество идентификаторов в предложении IN.
  /// Если исходный массив идентификаторов не длинный, выгодно выполнить один запрос.
  /// Иначе лучше выполнить два или более запросов с подмассивами идентификаторов.
  /// Для разбиения массивов применяется IdsFilterGenerator.
  /// Конструктор IdsFilterGenerator получает исходный массив идентификаторов и описатель
  /// формата базы данных для определения оптимальной длины массива. Конструктор
  /// делит исходный массив идентификаторов на подмассивы и запоминает их.
  /// Затем вызывается метод CreateFilters(), которому передается имя ссылочного поля
  /// или "Id". Метод создает один или несколько объектов IdsFilter, которые доступны
  /// через индексированное свойство.
  /// </summary>
  public class IdsFilterGenerator : IEnumerable<DBxFilter>
  {
    #region Конструкторы

    /// <summary>
    /// Создает генератор с количеством идентификаторов в группе, равным 100 
    /// </summary>
    /// <param name="allIds">Полный список идентификаторов. 
    /// Массив может иметь любую длину, включая нулевую.
    /// Массив не может содержать значения 0</param>
    public IdsFilterGenerator(Int32[] allIds)
      : this(allIds, 100)
    {
    }

    /// <summary>
    /// Создает генератор с количеством идентификаторов в группе, равным 100 
    /// </summary>
    /// <param name="allIds">Полный список идентификаторов. 
    /// Список может иметь любую длину, включая нулевую.</param>
    public IdsFilterGenerator(IdList allIds)
      : this(allIds.ToArray())
    {
    }

    /// <summary>
    /// Создает генератор с заданным количеством идентификаторов в группе 
    /// </summary>
    /// <param name="allIds">Полный список идентификаторов. 
    /// Массив может иметь любую длину, включая нулевую.
    /// Массив не может содержать значения 0</param>
    /// <param name="maxCount">Количество элементов в группе (не меньше 1)</param>
    public IdsFilterGenerator(Int32[] allIds, int maxCount)
    {
      Init(allIds, maxCount);

      // TODO: Пока никак не определяем оптимальное число идентификаторов

      // Заглушка
      _Filters = new DBxFilter[0];
    }

    /// <summary>
    /// Создает генератор с заданным количеством идентификаторов в группе 
    /// </summary>
    /// <param name="allIds">Полный список идентификаторов. 
    /// Список может иметь любую длину, включая нулевую.</param>
    /// <param name="maxCount">Количество элементов в группе (не меньше 1)</param>
    public IdsFilterGenerator(IdList allIds, int maxCount)
      : this(allIds.ToArray(), maxCount)
    {
    }

    private void Init(Int32[] allIds, int maxCount)
    {
      // Число групп
      int n1 = (allIds.Length + maxCount - 1) / maxCount;
      _IdArrays = new Int32[n1][];
      for (int i = 0; i < n1; i++)
      {
        int StartIndex = i * maxCount;
        int n2 = Math.Min(maxCount, allIds.Length - StartIndex);
        _IdArrays[i] = new Int32[n2];
        Array.Copy(allIds, StartIndex, _IdArrays[i], 0, n2);
      }
    }

    #endregion

    #region CreateFilters

    /// <summary>
    /// Создает фильтры для заданного имени столбца
    /// </summary>
    /// <param name="columnName">Имя числового столбца</param>
    public void CreateFilters(string columnName)
    {
      DBxFilter[] a = new DBxFilter[_IdArrays.Length];
      for (int i = 0; i < _IdArrays.Length; i++)
        a[i] = new IdsFilter(columnName, _IdArrays[i]);

      _Filters = a;
    }

    /// <summary>
    /// Создает фильтры для столбца "Id"
    /// </summary>
    public void CreateFilters()
    {
      CreateFilters("Id");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает массив идентификаторов для одной группы
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Int32[] GetIds(int index)
    {
      return _IdArrays[index];
    }

    /// <summary>
    /// Массивы идентификаторов, разбитые на группы подходящего размера
    /// </summary>
    private Int32[][] _IdArrays;

    /// <summary>
    /// Возвращает количество групп
    /// </summary>
    public int Count { get { return _IdArrays.Length; } }

    /// <summary>
    /// Возвращает фильтр для группы
    /// </summary>
    /// <param name="index">Индекс группы в диапазоне от 0 до Count-1</param>
    /// <returns>Фильтр</returns>
    public DBxFilter this[int index]
    {
      get
      {
#if DEBUG
        if (_Filters == null)
          throw new InvalidOperationException("Не было вызова метода CreateFilters");
#endif
        return _Filters[index];
      }
    }

    private DBxFilter[] _Filters;

    #endregion

    #region IEnumerable<DBxFilter> Members

    /// <summary>
    /// Возвращает перечислитель по фильтрам.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerator<DBxFilter> GetEnumerator()
    {
      return new ArrayEnumerator<DBxFilter>(_Filters);
    }

    IEnumerator<DBxFilter> IEnumerable<DBxFilter>.GetEnumerator()
    {
      return new ArrayEnumerator<DBxFilter>(_Filters);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<DBxFilter>(_Filters);
    }

    #endregion

    #region Методы для восстановления полного списка

    /// <summary>
    /// Возвращает список идентификаторов во всех группах
    /// </summary>
    /// <returns>Список идентификаторов</returns>
    public IdList GetWholeIdList()
    {
      IdList List = new IdList();
      for (int i = 0; i < _IdArrays.Length; i++)
        List.Add(GetIds(i));
      return List;
    }

    /// <summary>
    /// Возвращает общее количество идентификаторов во всех группах
    /// </summary>
    public int AllIdCount
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _IdArrays.Length; i++)
          cnt += _IdArrays[i].Length;
        return cnt;
      }
    }

    /// <summary>
    /// Возвращает массив, содержащий все идентификаторы во всех группах
    /// </summary>
    /// <returns>Полный массив идентификаторов</returns>
    public Int32[] GetAllIds()
    {
      Int32[] a = new Int32[AllIdCount];
      int cnt = 0;
      for (int i = 0; i < _IdArrays.Length; i++)
      {
        Array.Copy(_IdArrays[i], 0, a, cnt, _IdArrays[i].Length);
        cnt += _IdArrays[i].Length;
      }
      return a;
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает количество групп и общее количество идентификаторов (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Group Count=" + Count.ToString() + ", All Id Count=" + AllIdCount.ToString();
    }

    #endregion
  }
}