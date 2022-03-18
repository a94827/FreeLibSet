// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Выражение, дополненное альясом.
  /// Альяс используется для именования полей в таблице результатов запроса SELECT с помощью конструкции "AS".
  /// </summary>
  [Serializable]
  public sealed class DBxNamedExpression : IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создать выражение с альясом
    /// </summary>
    /// <param name="expression">Выражение. Не может быть null</param>
    /// <param name="alias">Альяс. Должен быть задан</param>
    public DBxNamedExpression(DBxExpression expression, string alias)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(alias))
        throw new ArgumentNullException("alias");
      _Expression = expression;
      _Alias = alias;
    }

    /// <summary>
    /// Создать выражение для поля.
    /// Альяс устанавливается равным имени поля.
    /// </summary>
    /// <param name="expression">Выражение DBxColumn. Если передано другое выражение, выбрасывается исключение. Не может быть null.</param>
    public DBxNamedExpression(DBxExpression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      _Expression = expression;
      if (expression is DBxColumn)
        _Alias = ((DBxColumn)expression).ColumnName;
      else
        throw new ArgumentException("Для использования выражения типа " + expression.GetType() + " требуется задавать альяс в явном виде");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение (имя простого поля, ссылочное поле или функция).
    /// Не может быть null
    /// </summary>
    public DBxExpression Expression { get { return _Expression; } }
    private DBxExpression _Expression;

    /// <summary>
    /// Альяс (в SQL-запросе используется в инструкции "AS")
    /// Не может быть пустой строкой
    /// </summary>
    public string Alias { get { return _Alias; } }
    private string _Alias;

    /// <summary>
    /// Возвращает true, если альяс таблицы требуется обязательно
    /// </summary>
    public bool AliasRequired
    {
      get
      {
        DBxColumn col = Expression as DBxColumn;
        if (col == null)
          return true;
        if (col.ColumnName.IndexOf('.') >= 0)
          return true;
        return col.ColumnName != Alias;
      }
    }

    /// <summary>
    /// Возвращает свойство Alias (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Alias;
    }

    #endregion

    #region IObjectWithCode members

    string IObjectWithCode.Code { get { return _Alias; } }

    #endregion
  }

  /// <summary>
  /// Список выражений, которые должны возвращаться оператором SELECT.
  /// Кроме выражений хранятся альясы имен полей результирующей таблицы.
  /// </summary>
  [Serializable]
  public sealed class DBxNamedExpressionList : NamedList<DBxNamedExpression>
  {
    #region Дополнительные методы

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    /// <summary>
    /// Добавить выражение без указания альяса
    /// </summary>
    /// <param name="expression">Объект DBxColumn. Если передано другое выражение, будет выброшено исключение. Не может быть null.</param>
    public void Add(DBxExpression expression)
    {
      base.Add(new DBxNamedExpression(expression));
    }

    /// <summary>
    /// Добавить выражение с указанием альяса.
    /// </summary>
    /// <param name="expression">Выражение. Не может быть null.</param>
    /// <param name="alias">Альяс. Должен быть задан</param>
    public void Add(DBxExpression expression, string alias)
    {
      base.Add(new DBxNamedExpression(expression, alias));
    }

    /// <summary>
    /// Создает объект выражения DBxColumn и добавляет его в список.
    /// Если имя <paramref name="columnNames"/> содержит запятые, создается несколько столбцов.
    /// </summary>
    /// <param name="columnNames">Имя поля. Может содержать точки для получения ссылочных полей. Должно быть задано. Может содержать запятые для задания нескольких полей</param>
    public void Add(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw new ArgumentNullException("columnNames");
      if (columnNames.IndexOf(',') >= 0)
      {
        string[] a = columnNames.Split(',');
        for (int i = 0; i < a.Length; i++)
          Add(new DBxColumn(a[i]));
      }
      else
        Add(new DBxColumn(columnNames));
    }


    /// <summary>
    /// Создает объект выражения DBxColumn и добавляет его в список с указанием альяса.
    /// Можно задать только одно имя поля.
    /// </summary>
    /// <param name="columnName">Имя поля. Может содержать точки для получения ссылочных полей. Должно быть задано. Не может содержать запятые</param>
    /// <param name="alias">Альяс</param>
    public void Add(string columnName, string alias)
    {
      Add(new DBxColumn(columnName), alias);
    }

    /// <summary>
    /// Создает несколько выражений DBxColumn и добавляет их в список.
    /// </summary>
    /// <param name="columnNames">Список имен полей. Если null, то никаких действий не выполняется</param>
    public void Add(DBxColumns columnNames)
    {
      if (columnNames == null)
        return;
      for (int i = 0; i < columnNames.Count; i++)
        this.Add(new DBxColumn(columnNames[i]));
    }

    /// <summary>
    /// Вызывает DBxExpression.GetColumnNames() для всех выражений, входящих в список.
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public void GetColumnNames(DBxColumnList list)
    {
#if DEBUG
      if (list == null)
        throw new ArgumentNullException("list");
#endif
      for (int i = 0; i < Count; i++)
        this[i].Expression.GetColumnNames(list);
    }

    #endregion
  }

  /// <summary>
  /// Данные для запросов SELECT (методы IDBxCon.FillSelect() и IDBxCon.ReaderSelect()
  /// </summary>
  [Serializable]
  public sealed class DBxSelectInfo
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект.
    /// Для использования объекта, как минимум, должно быть установлено свойство TableName.
    /// </summary>
    public DBxSelectInfo()
    {
      _Expressions = new DBxNamedExpressionList();
      // Инициализируем при обращении
      //_GroupBy = new List<DBxExpression>();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя основной таблицы, относительно которой задаются поля.
    /// Свойство должно быть установлено
    /// </summary>
    public string TableName
    {
      get { return _TableName; }
      set { _TableName = value; }
    }
    private string _TableName;

    /// <summary>
    /// Список выражений (обычно, имен полей), которые требуется выбрать.
    /// Если список пустой, то будут выбраны все поля таблицы, к которым у пользователя есть доступ.
    /// Если ограничений на доступ к полям нет, то список будет эквивалентен "SELECT * FROM"
    /// </summary>
    public DBxNamedExpressionList Expressions { get { return _Expressions; } }
    private DBxNamedExpressionList _Expressions;

    /// <summary>
    /// Фильтр для условия WHERE. Если не задан, то будут выбираться все строки таблицы
    /// </summary>
    public DBxFilter Where
    {
      get { return _Where; }
      set { _Where = value; }
    }
    private DBxFilter _Where;

    /// <summary>
    /// Список выражений для GROUP BY.
    /// По умолчанию список пустой
    /// </summary>
    public IList<DBxExpression> GroupBy
    {
      get
      {
        if (_GroupBy == null)
          _GroupBy = new List<DBxExpression>();
        return _GroupBy;
      }
    }
    private List<DBxExpression> _GroupBy;

    internal bool HasGroupBy
    {
      get
      {
        if (_GroupBy == null)
          return false; // чаще всего так и бывает
        else
          return _GroupBy.Count > 0;
      }
    }

    /// <summary>
    /// Заполняет список GroupBy выражениями из списка выраженийExpressions.
    /// Предполагается, что список Expressions заполнен и содержит как агрегатные функции, так и обычные выражения
    /// </summary>
    public void InitGroupBy()
    {
      if (Expressions.Count == 0)
        throw new InvalidOperationException("Не задан список выражений. Для оператора \"SELECT * FROM xxx\" не может использоваться выражение GROUP BY");

      if (GroupBy.Count > 0)
        throw new InvalidOperationException("Список GroupBy уже заполнен. Повторный вызов метода не допускается");

      for (int i = 0; i < Expressions.Count; i++)
      {
        bool hasColumn;
        bool hasAgregate;
        GetExpressionInfo(Expressions[i].Expression, out hasColumn, out hasAgregate);
        if (hasColumn && hasAgregate)
          throw new InvalidOperationException("Выражение Expressions[" + i.ToString() + "] (" + Expressions[i].Expression.ToString() + ") содержит одновременно и агрегатную функцию и ссылку на поле таблицы");
        else if (hasColumn)
          GroupBy.Add(Expressions[i].Expression);
      }
    }

    private static void GetExpressionInfo(DBxExpression expression, out bool hasColumn, out bool hasAgregate)
    {
      hasColumn = false;
      hasAgregate = false;
      DoGetExpressionInfo(expression, ref hasColumn, ref hasAgregate);
    }

    private static void DoGetExpressionInfo(DBxExpression expression, ref bool hasColumn, ref bool hasAgregate)
    {
      if (expression is DBxAgregateFunction)
        hasAgregate = true;
      else if (expression is DBxColumn)
        hasColumn = true;
      else if (expression is DBxFunction)
      {
        DBxFunction f = (DBxFunction)expression;
        for (int i = 0; i < f.Arguments.Length; i++)
          DoGetExpressionInfo(f.Arguments[i], ref hasColumn, ref hasAgregate);
      }
    }

    /// <summary>
    /// Фильтр для выражения HAVING.
    /// По умолчанию - null - фильтр не задан
    /// </summary>
    public DBxFilter Having { get { return _Having; } set { _Having = value; } }
    private DBxFilter _Having;

    /// <summary>
    /// Порядок сортировки ORDER BY.
    /// Если не задан, то порядок возвращаемых строк непредсказуем.
    /// </summary>
    public DBxOrder OrderBy
    {
      get { return _OrderBy; }
      set { _OrderBy = value; }
    }
    private DBxOrder _OrderBy;

    /// <summary>
    /// Если установлено значение, отличное от 0, то используется предикат TOP n
    /// </summary>
    public int MaxRecordCount
    {
      get { return _MaxRecordCount; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _MaxRecordCount = value;
      }
    }
    private int _MaxRecordCount;

    /// <summary>
    /// Если установить в true, то используется предикат DISTINCT
    /// </summary>
    public bool Unique
    {
      get { return _Unique; }
      set { _Unique = value; }
    }
    private bool _Unique;

    #endregion

    #region Методы

    /// <summary>
    /// Получить список имен полей, используемых в выражениях, фильтрах, порядке сортировки.
    /// Предполагается, что список Expressions заполнен, иначе выбрасывается исключение.
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public void GetColumnNames(DBxColumnList list)
    {
#if DEBUG
      if (list == null)
        throw new ArgumentNullException("list");
#endif

      if (Expressions.Count == 0)
        throw new InvalidOperationException("Список выражений Expressions не заполнен");

      Expressions.GetColumnNames(list);

      if (Where != null)
        Where.GetColumnNames(list);

      if (HasGroupBy)
      {
        for (int i = 0; i < GroupBy.Count; i++)
          GroupBy[i].GetColumnNames(list);
      }
      if (Having != null)
        Having.GetColumnNames(list);

      if (OrderBy != null)
      {
        // OrderBy.GetColumnNames(list);
        // 25.12.2019
        // В инструкции OrderBy могут идти как ссылки на поля таблицы, так и альясы из списка Expressions
        // Альясы не надо добавлять в список полей
        DBxColumnList list2 = new DBxColumnList();
        for (int i = 0; i < OrderBy.Parts.Length; i++)
        {
          list2.Clear();
          OrderBy.Parts[i].Expression.GetColumnNames(list2);
          for (int j = 0; j < list2.Count; j++)
          {
            if (!Expressions.Contains(list2[j])) // нет такого альяса?
              list.Add(list2[j]);
          }
        }
      }
    }

    #endregion
  }
}
