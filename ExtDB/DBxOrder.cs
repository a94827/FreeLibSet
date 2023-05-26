// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Последовательность полей для сортировки с признаками ASC/DESC.
  /// Класс однократной записи
  /// </summary>
  [Serializable]
  public sealed class DBxOrder
  {
    #region Конструкторы

    /// <summary>
    /// Создает порядок сортировки на основании одной или нескольких записей.
    /// Создает копию списка <paramref name="parts"/>
    /// </summary>
    /// <param name="parts">Массив полей сортировки. Должен быть задан хотя бы один параметр</param>
    public DBxOrder(params DBxOrderPart[] parts)
      : this((ICollection<DBxOrderPart>)parts)
    {
    }

    /// <summary>
    /// Создает порядок сортировки на основании одной или нескольких записей.
    /// Создает копию списка <paramref name="parts"/>
    /// </summary>
    /// <param name="parts">Коллекция полей сортировки. Не может быть пустой</param>
    public DBxOrder(ICollection<DBxOrderPart> parts)
    {
#if DEBUG
      if (parts == null)
        throw new ArgumentNullException("parts");
#endif

      if (parts.Count < 1)
        throw new ArgumentException("Список частей сортировки пустой", "parts");

      _Parts = new DBxOrderPart[parts.Count];
      parts.CopyTo(_Parts, 0);

#if DEBUG
      for (int i = 0; i < _Parts.Length; i++)
      {
        if (_Parts[i].IsEmpty)
          throw new ArgumentException("Элемент сортировки с индексом " + i.ToString() + " не задан", "parts");
      }
#endif
    }


    /// <summary>
    /// Создает порядок сортировки на основании одного или нескольких выражений.
    /// Каждое выражение преобразуется в <see cref="DBxOrderPart"/> с сортировкой по возрастанию.
    /// </summary>
    /// <param name="expressions">Коллекция выражений сортировки. Не может быть пустой</param>
    public DBxOrder(ICollection<DBxExpression> expressions)
      : this(GetItems(expressions))
    {
    }

    private static DBxOrderPart[] GetItems(ICollection<DBxExpression> expressions)
    {
      if (expressions == null)
        throw new ArgumentNullException("expressions");

      DBxOrderPart[] a = new DBxOrderPart[expressions.Count];
      int cnt = 0;
      foreach (DBxExpression expr in expressions)
      {
        if (expr == null)
          throw new ArgumentNullException("expressions[" + cnt + "]");
        a[cnt] = new DBxOrderPart(expr, ListSortDirection.Ascending);
        cnt++;
      }
      return a;
    }

    /// <summary>
    /// Создает порядок сортировки для одного поля по возрастанию или убыванию
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="sortOrder">Порядок сортировки: по возрастанию или по убыванию</param>
    public DBxOrder(string columnName, ListSortDirection sortOrder)
      : this(new DBxOrderPart[] { new DBxOrderPart(new DBxColumn(columnName), sortOrder) })
    {
    }

    /// <summary>
    /// Создает порядок сортировки для одного поля по возрастанию.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DBxOrder(string columnName)
      : this(columnName, ListSortDirection.Ascending)
    {
    }

    /// <summary>
    /// Создает порядок сортировки для одного поля или другого элемента по возрастанию или убыванию.
    /// </summary>
    /// <param name="expression">Выражение сортировки (обычно, <see cref="DBxColumn"/>) без указания ASC/DESC</param>
    /// <param name="sortOrder">Порядок сортировки: по возрастанию или по убыванию</param>
    public DBxOrder(DBxExpression expression, ListSortDirection sortOrder)
      : this(new DBxOrderPart[] { new DBxOrderPart(expression, sortOrder) })
    {
    }

    /// <summary>
    /// Создает порядок сортировки для одного поля или другого элемента по возрастанию.
    /// </summary>
    /// <param name="expression">Элемент списка сортировки (поле или функция) без указания ASC/DESC</param>
    public DBxOrder(DBxExpression expression)
      : this(expression, ListSortDirection.Ascending)
    {
    }

    /// <summary>
    /// Создает порядок сортировки на основании одного или нескольких выражений.
    /// Каждое выражение преобразуется в <see cref="DBxOrderPart"/> с сортировкой по возрастанию
    /// </summary>
    /// <param name="expressions">Коллекция элементов сортировки. Не может быть пустой</param>
    public DBxOrder(params DBxExpression[] expressions)
      : this((ICollection<DBxExpression>)expressions)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список элементов сортировки в выражении ORDER BY.
    /// Каждый элемент задает поле (или выражении) и признак ASC/DESC
    /// </summary>
    public DBxOrderPart[] Parts { get { return _Parts; } }
    private readonly DBxOrderPart[] _Parts;

    #endregion

    #region Прочие методы

    /// <summary>
    /// Получить информацию, необходимую для сортировки табличного просмотра с
    /// помощью щелчка на заговке столбца.
    /// Возвращает имя первого (основного) столбца сортировки и признак сортировки
    /// по возрастанию или по убыванию.
    /// </summary>
    /// <param name="columnName">Имя первого столбца. В табличном просмотре этот столбец
    /// будет помечен треугольнчиком</param>
    /// <param name="sortOrder">Сюда помещается порядок сортировки: по возрастанию или по убыванию</param>
    public void GetFirstColumnInfo(out string columnName, out ListSortDirection sortOrder)
    {
      columnName = String.Empty;
      sortOrder = ListSortDirection.Ascending;
      if (Parts.Length > 0)
      {
        DBxColumnList list = new DBxColumnList();
        Parts[0].Expression.GetColumnNames(list);
        if (list.Count > 0)
        {
          columnName = list[0];
          sortOrder = Parts[0].SortOrder;
        }
      }
    }

    /// <summary>
    /// Получение списка имен полей, участвующих в сортировке.
    /// Метод вызывает <see cref="DBxOrderPart"/>.GetColumnNames() для всех элементов в списке сортировки
    /// </summary>
    /// <param name="list">Список для заполнения. Не может быть null</param>
    public void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < Parts.Length; i++)
        Parts[i].Expression.GetColumnNames(list);
    }

    /// <summary>
    /// Возвращает текстовое представление как для оператора ORDER BY (без ключевых слов)
    /// в формате DataView.
    /// Может быть использовано для установки свойства DataView.Sort 
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      DBxSqlBuffer buffer = new DBxSqlBuffer();
      buffer.FormatOrder(this);
      return buffer.SB.ToString();
    }

    /// <summary>
    /// Задание префикса для всех имен полей, входящих в порядок сортировки.
    /// Создает и возвращает новый объект <see cref="DBxOrder"/>, в котором содержатся новые объекты <see cref="DBxOrderPart"/>.
    /// </summary>
    /// <param name="prefix">Префикс имени поля (обычно, имя ссылочного поля с точкой на конце)</param>
    /// <returns>Новый заполненный объект <see cref="DBxOrder"/></returns>
    public DBxOrder SetColumnNamePrefix(string prefix)
    {
      DBxOrderPart[] parts2 = new DBxOrderPart[Parts.Length];
      for (int i = 0; i < Parts.Length; i++)
        parts2[i] = Parts[i].SetColumnNamePrefix(prefix);
      return new DBxOrder(parts2);
    }

    #endregion

    #region Операторы

#if XXX
    public static DataOrder operator +(DataOrder Order1, DataOrder Order2)
    {
      if (Order1 == null)
        return Order2;

      if (Order2 == null)
        return Order1;

      DataOrderItenInfo[] Infos = new DataOrderItenInfo(Order1.Infos.Length + Order2.Infos.Length);
      Order1.Infos.CopyTo(Infos, 0);
      Order2.Infos.CopyTo(Infos, Order1.Infos.Length);

      return new DataOrder(Infos);
    }
#endif

    #endregion

    #region Статические методы и поля

    /// <summary>
    /// Получение выражения из списка имен полей, разделенных запятыми
    /// Имена полей могут быть заключены в квадратные скобки. За именем поля может быть признак сортировки "ASC" или "DESC".
    /// См. метод <see cref="DataTools.GetDataViewSortColumnNames(string, out string[], out ListSortDirection[])"/>, используемый для парсинга строки.
    /// Функция возвращает null, если список полей пуст.
    /// </summary>
    /// <param name="dataViewSort">Порядок сортировки в формате <see cref="System.Data.DataView.Sort"/></param>
    /// <returns>Объект <see cref="DBxOrder"/> или null</returns>
    public static DBxOrder FromDataViewSort(string dataViewSort)
    {
      return FromDataViewSort(dataViewSort, String.Empty);
    }

    /// <summary>
    /// Получение выражения из списка имен полей, разделенных запятыми
    /// Имена полей могут быть заключены в квадратные скобки. За именем поля может быть признак сортировки "ASC" или "DESC".
    /// См. метод <see cref="DataTools.GetDataViewSortColumnNames(string, out string[], out ListSortDirection[])"/>, используемый для парсинга строки.
    /// Функция возвращает null, если список полей пуст.
    /// Эта перегрузка предназначена для использования со ссылочным полем.
    /// </summary>
    /// <param name="dataViewSort">Порядок сортировки в формате <see cref="System.Data.DataView.Sort"/></param>
    /// <param name="refColumnName">Имя ссылочного поля, которое будет добавлено перед каждым столбцом в порядке сортировки</param>
    /// <returns>Объект <see cref="DBxOrder"/> или null</returns>
    public static DBxOrder FromDataViewSort(string dataViewSort, string refColumnName)
    {
      if (String.IsNullOrEmpty(dataViewSort))
        return null;

      string[] aColNames;
      ListSortDirection[] aSorts;
      DataTools.GetDataViewSortColumnNames(dataViewSort, out aColNames, out aSorts);
      DBxOrderPart[] parts = new DBxOrderPart[aSorts.Length];
      for (int i = 0; i < aColNames.Length; i++)
      {
        string colName2 = aColNames[i];
        if (!String.IsNullOrEmpty(refColumnName))
          colName2 = refColumnName + "." + colName2;
        parts[i] = new DBxOrderPart(colName2, aSorts[i]);
      }
      return new DBxOrder(parts);
    }

    /// <summary>
    /// Порядок сортировки по полю "Id"
    /// </summary>
    public static readonly DBxOrder ById = new DBxOrder("Id");

    #endregion
  }


  /// <summary>
  /// Хранение ссылки на поле и признака ASC/DESC для одного элемента сортировки ORDER BY
  /// </summary>
  [Serializable]
  public struct DBxOrderPart
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый элемент порядка сортировки с заданным признаком ASC/DESC
    /// </summary>
    /// <param name="expression">Элемент порядка сортировки (обычно, <see cref="DBxColumn"/>)</param>
    /// <param name="sortOrder">Порядок сортировки: по возрастанию или по убыванию</param>
    public DBxOrderPart(DBxExpression expression, ListSortDirection sortOrder)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      _Expression = expression;
      _SortOrder = sortOrder;
    }

    /// <summary>
    /// Создает новый элемент порядка сортировки по возрастанию
    /// </summary>
    /// <param name="expression">Элемент порядка сортировки (обычно, <see cref="DBxColumn"/>)</param>
    public DBxOrderPart(DBxExpression expression)
      : this(expression, ListSortDirection.Ascending)
    {
    }

    /// <summary>
    /// Создает новый элемент порядка сортировки для поля с заданным признаком ASC/DESC
    /// </summary>
    /// <param name="columnName">Имя поля для создания <see cref="DBxColumn"/></param>
    /// <param name="sortOrder">Порядок сортировки: по возрастанию или по убыванию</param>
    public DBxOrderPart(string columnName, ListSortDirection sortOrder)
      : this(new DBxColumn(columnName), sortOrder)
    {
    }

    /// <summary>
    /// Создает новый элемент порядка сортировки для поля по возрастанию
    /// </summary>
    /// <param name="columnName">Имя поля для создания <see cref="DBxColumn"/></param>
    public DBxOrderPart(string columnName)
      : this(new DBxColumn(columnName), ListSortDirection.Ascending)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Элемент сортировки (обычно, поле DBxOrderColumn)
    /// </summary>
    public DBxExpression Expression { get { return _Expression; } }
    private readonly DBxExpression _Expression;

    /// <summary>
    /// Признак обрабтной сортировки DESC
    /// </summary>
    public ListSortDirection SortOrder { get { return _SortOrder; } }
    private readonly ListSortDirection _SortOrder;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _Expression == null; } }

    /// <summary>
    /// Возвращает SQL-представления элемента в формате свойства <see cref="System.Data.DataView.Sort"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Expression == null)
        return String.Empty;

      DBxSqlBuffer buffer = new DBxSqlBuffer();
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = false; // Нельзя делать true, т.к. при сортировке для DataView тип данных неизвестен
      buffer.FormatExpression(_Expression, formatInfo);
      if (_SortOrder == ListSortDirection.Descending)
        buffer.SB.Append(" DESC");
      return buffer.ToString();
    }

    #endregion

    #region Клонирование

    /// <summary>
    /// Создает копию объекта <see cref="DBxOrderPart"/> с заданным префиксом (обычно, имя ссылочного поля с точкой).
    /// </summary>
    /// <param name="prefix">Префикс</param>
    /// <returns>Копия <see cref="DBxOrderPart"/></returns>
    public DBxOrderPart SetColumnNamePrefix(string prefix)
    {
      if (IsEmpty)
        return new DBxOrderPart();
      else
        return new DBxOrderPart(Expression.SetColumnNamePrefix(prefix), SortOrder);
    }

    #endregion
  }
}
