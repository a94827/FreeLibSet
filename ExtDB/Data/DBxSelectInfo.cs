// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

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
    /// <param name="alias">Альяс. Может быть не задан, если <paramref name="expression"/> - <see cref="DBxColumn"/></param>
    public DBxNamedExpression(DBxExpression expression, string alias)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      //if (String.IsNullOrEmpty(alias))
      //  throw new ArgumentNullException("alias");
      _Expression = expression;
      _Alias = alias ?? String.Empty;

      if (_Alias.Length == 0)
      {
        if (!(expression is DBxColumn))
          //_Alias = ((DBxColumn)expression).ColumnName;
          //else
          throw new ArgumentException(String.Format(Res.DBxNameExpression_Arg_AliasRequired,
            expression.GetType()), "alias");
      }
    }

    /// <summary>
    /// Создать выражение для поля <see cref="DBxColumn"/>.
    /// Для выражений других типов (<see cref="DBxFunction"/>) используйте конструктор задающий альяс в явном виде.
    /// Альяс устанавливается равным имени поля.
    /// </summary>
    /// <param name="expression">Выражение <see cref="DBxColumn"/>. Если передано другое выражение, выбрасывается исключение. Не может быть null.</param>
    public DBxNamedExpression(DBxExpression expression)
      :this(expression, String.Empty)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение (имя простого поля, ссылочное поле или функция).
    /// Не может быть null
    /// </summary>
    public DBxExpression Expression { get { return _Expression; } }
    private readonly DBxExpression _Expression;

    /// <summary>
    /// Альяс (в SQL-запросе используется в инструкции "AS")
    /// Может быть пустой строкой, если <see cref="Expression"/> ссылается на <see cref="DBxColumn"/>.
    /// Для выражений <see cref="DBxFunction"/> и <see cref="DBxAggregateFunction"/> альяс должен быть задан обязательно.
    /// </summary>
    public string Alias { get { return _Alias; } }
    private readonly string _Alias;

    /// <summary>
    /// Имя результирующего столбца.
    /// Если задано свойство <see cref="Alias"/>, то возвращается указанный альяс. Иначе возвращается <see cref="DBxColumn.ColumnName"/> из свойства <see cref="Expression"/>.
    /// </summary>
    public string ResultColumnName
    {
      get
      {
        if (_Alias.Length == 0)
          return ((DBxColumn)_Expression).ColumnName;
        else
          return _Alias;
      }
    }



    ///// <summary>
    ///// Возвращает true, если альяс таблицы требуется обязательно
    ///// </summary>
    //public bool AliasRequired
    //{
    //  get
    //  {
    //    DBxColumn col = Expression as DBxColumn;
    //    if (col == null)
    //      return true;
    //    if (col.ColumnName.IndexOf('.') >= 0)
    //      return true;
    //    return col.ColumnName != Alias;
    //  }
    //}

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

    string IObjectWithCode.Code 
    { 
      get 
      {
        if (_Alias.Length == 0)
          return ((DBxColumn)_Expression).ColumnName;
        else
          return _Alias; 
      }
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Сравнение с другим объектом
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>true, если совпадают</returns>
    public override bool Equals(object obj)
    {
      DBxNamedExpression other = obj as DBxNamedExpression;
      if (Object.ReferenceEquals(other, null))
        return false;

      return String.Equals(this._Alias, other._Alias, StringComparison.Ordinal) &&
        this._Expression == other._Expression;
    }

    /// <summary>
    /// Возвращает хэш-код
    /// </summary>
    /// <returns>хэш-код</returns>
    public override int GetHashCode()
    {
      return _Alias.GetHashCode();
    }

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
    /// Добавить выражение <see cref="DBxColumn"/> без указания альяса
    /// </summary>
    /// <param name="expression">Объект <see cref="DBxColumn"/>. Если передано другое выражение, будет выброшено исключение. Не может быть null.</param>
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
    /// Создает объект выражения <see cref="DBxColumn"/> и добавляет его в список.
    /// Если имя <paramref name="columnNames"/> содержит запятые, создается несколько столбцов.
    /// </summary>
    /// <param name="columnNames">Имя поля. Может содержать точки для получения ссылочных полей. Должно быть задано. 
    /// Может содержать запятые для задания нескольких полей</param>
    public void Add(string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnNames");
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
    /// Создает объект выражения <see cref="DBxColumn"/> и добавляет его в список с указанием альяса.
    /// Можно задать только одно имя поля.
    /// </summary>
    /// <param name="columnName">Имя поля. Может содержать точки для получения ссылочных полей. Должно быть задано. 
    /// Не может содержать запятые</param>
    /// <param name="alias">Альяс</param>
    public void Add(string columnName, string alias)
    {
      Add(new DBxColumn(columnName), alias);
    }

    /// <summary>
    /// Создает несколько выражений <see cref="DBxColumn"/> и добавляет их в список.
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
    /// Вызывает <see cref="DBxExpression.GetColumnNames(DBxColumnList)"/> для всех выражений, входящих в список.
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
  /// Данные для запросов SELECT (методы <see cref="IDBxConReadOnlyBase.FillSelect(DBxSelectInfo)"/> и <see cref="IDBxCon.ReaderSelect(DBxSelectInfo)"/>.
  /// Поддерживает выражения FROM, WHERE, ORDER BY, HAVING, GROUP BY, LIMIT/TOP (количество отобржаемых строк, но не "окно").
  /// </summary>
  [Serializable]
  public sealed class DBxSelectInfo : ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект.
    /// Для использования объекта, как минимум, должно быть установлено свойство <see cref="TableName"/>.
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
    /// Свойство должно быть установлено.
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
    /// Если ограничений на доступ к полям нет, то список будет эквивалентен "SELECT * FROM".
    /// </summary>
    public DBxNamedExpressionList Expressions { get { return _Expressions; } }
    private readonly DBxNamedExpressionList _Expressions;

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
    /// Заполняет список <see cref="GroupBy"/> выражениями из списка выражений <see cref="Expressions"/>.
    /// Предполагается, что список <see cref="Expressions"/> заполнен и содержит как агрегатные функции, так и обычные выражения
    /// </summary>
    public void InitGroupBy()
    {
      if (Expressions.Count == 0)
        throw new InvalidOperationException(Res.DBxSelectInfo_Err_GroupByWithoutExpression);

      if (GroupBy.Count > 0)
        throw new InvalidOperationException(Res.DBxSelectInfo_Err_InitGroupByAlreadyCalled);

      for (int i = 0; i < Expressions.Count; i++)
      {
        bool hasColumn;
        bool hasAggregate;
        GetExpressionInfo(Expressions[i].Expression, out hasColumn, out hasAggregate);
        if (hasColumn)
        {
          if (hasAggregate)
            throw new InvalidOperationException(String.Format(Res.DBxSelectInfo_Err_ColumnAndAggMixed,
              i, Expressions[i].Expression.ToString()));
          else
            GroupBy.Add(Expressions[i].Expression);
        }
      }
    }

    private static void GetExpressionInfo(DBxExpression expression, out bool hasColumn, out bool hasAggregate)
    {
      hasColumn = false;
      hasAggregate = false;
      DoGetExpressionInfo(expression, ref hasColumn, ref hasAggregate);
    }

    private static void DoGetExpressionInfo(DBxExpression expression, ref bool hasColumn, ref bool hasAggregate)
    {
      if (expression is DBxAggregateFunction)
        hasAggregate = true;
      else if (expression is DBxColumn)
        hasColumn = true;
      else if (expression is DBxFunction)
      {
        DBxFunction f = (DBxFunction)expression;
        for (int i = 0; i < f.Arguments.Length; i++)
          DoGetExpressionInfo(f.Arguments[i], ref hasColumn, ref hasAggregate);
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
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);
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
    /// Получить список имен полей, используемых в выражениях (<see cref="Expressions"/>, <see cref="GroupBy"/>), фильтрах (<see cref="Where"/>, <see cref="Having"/>), порядке сортировки (<see cref="OrderBy"/>).
    /// Предполагается, что список <see cref="Expressions"/> заполнен, иначе выбрасывается исключение.
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public void GetColumnNames(DBxColumnList list)
    {
#if DEBUG
      if (list == null)
        throw new ArgumentNullException("list");
#endif

      if (Expressions.Count == 0)
        throw new InvalidOperationException(Res.DBxSelectInfo_Err_NoExpressions);

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

    #region Клонирование

    /// <summary>
    /// Создает копию объекта DBxSelectInfo
    /// </summary>
    /// <returns>Новый объект </returns>
    public DBxSelectInfo Clone()
    {
      DBxSelectInfo res = new DBxSelectInfo();
      res.TableName = this.TableName;
      foreach (DBxNamedExpression expr in this.Expressions)
        res.Expressions.Add(expr); // классы однократной записи можно просто взять по ссылке
      res.Where = this.Where;
      if (HasGroupBy)
      {
        foreach (DBxExpression expr in this.GroupBy)
          res.GroupBy.Add(expr);
      }
      res.Having = this.Having;
      res.OrderBy = this.OrderBy;
      res.Unique = this.Unique;
      res.MaxRecordCount = this.MaxRecordCount;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }


  // TODO: Пока InSelectFilter не работает, не знаю, куда его деть

  /// <summary>
  /// Фильтр по подзапросу в виде "Поле/Выражение IN (SELECT ...)".
  /// Этот фильтр не должен использоваться в автономных просмотрах <see cref="DataView"/>, так как не поддерживает метод TestFilter()
  /// </summary>
  [Serializable]
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
  public class InSelectFilter : DBxOneExpressionFilter // TODO: Пока не работает, т.к. не знаю, как форматировать подзапросы
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <param name="selectInfo">Подзапрос</param>
    /// <param name="columnType">Тип данных</param>
    public InSelectFilter(DBxExpression expression, DBxSelectInfo selectInfo, DBxColumnType columnType)
      : base(expression)
    {
#if DEBUG
      if (selectInfo == null)
        throw new ArgumentNullException("selectInfo");
#endif
      if (selectInfo.Expressions.Count != 1)
        throw new ArgumentException("В подзапросе должно в результат входить только одно поле/выражение", "selectInfo");
      if (selectInfo.OrderBy != null)
        throw new ArgumentException("В подзапросе не должно быть выражения ORDER BY", "selectInfo");
      if (selectInfo.MaxRecordCount != 0)
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
    private readonly DBxSelectInfo _SelectInfo;

    /// <summary>
    /// Тип данных.
    /// Имеет смысл задавать только в случае, когда тип данных в массиве не соответствует типу данных столбца,
    /// например, для столбца типа Guid, а Values содержит строки
    /// </summary>
    public DBxColumnType ColumnType { get { return _ColumnType; } }
    private readonly DBxColumnType _ColumnType;

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
}
