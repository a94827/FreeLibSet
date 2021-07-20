using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// Классы для конструирования SQL-выражений ORDER BY

/*
 * 20.09.2011
 * 1. Класс DataOrder сделан неабстрактным. Он должен содержать список объектов
 *    DataOrderItem. Для каждого элемента также хранится признак Descend
 *    Структура DataOrderItemInfo хранит DataOrderItem и призак Descend
 * 2. Класс MultiDataOrder убрать
 * 3. DataOrderItem является абстрактным базовым классом для DataOrderColumn и
 *    DataOrderIsNullColumn (и, возможно, других классов). 
 * 4. DataOrderColumn возвращает имя поля без DESC
 * 5. DataOrderIsNullColumn хранит тип данных (?)
 */

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// Последовательность полей для сортировки с признаками DESC.
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
      for (int i=0; i<_Parts.Length;i++)
      {
        if (_Parts[i].IsEmpty)
          throw new ArgumentException("Элемент сортировки с индексом "+i.ToString()+" не задан", "parts");
      }
#endif
    }

    
    /// <summary>
    /// Создает порядок сортировки на основании одного или нескольких выражений.
    /// Каждое выражение преобразуется в DBxOrderItem с сортировкой по возрастанию
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
    /// <param name="ColumnName">Имя поля</param>
    public DBxOrder(string ColumnName)
      : this(ColumnName, ListSortDirection.Ascending)
    {
    }

    /// <summary>
    /// Создает порядок сортировки для одного поля или другого элемента по возрастанию или убыванию.
    /// </summary>
    /// <param name="expression">Выражение сортировки (обычно, DBxColumn) без указания ASC/DESC</param>
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
    /// Каждое выражение преобразуется в DBxOrderItem с сортировкой по возрастанию
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
    private DBxOrderPart[] _Parts;

    #endregion

    #region Прочие методы

    /// <summary>
    /// Получить информацию, необходимую для сортировки табличного просмотра с
    /// помощью щелчка на заговке столбца
    /// Возвращает имя первого (основного) столбца сортировки и признак сортировки
    /// по возрастанию или по убыванию
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
        DBxColumnList List = new DBxColumnList();
        Parts[0].Expression.GetColumnNames(List);
        if (List.Count > 0)
        {
          columnName = List[0];
          sortOrder = Parts[0].SortOrder;
        }
      }
    }

    /// <summary>
    /// Получение списка имен полей, участвующих в сортировке.
    /// Метод вызывает DBxOrderItem.GetColumnNames() для всех элементов в списке сортировки
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
      DBxSqlBuffer Buffer = new DBxSqlBuffer();
      Buffer.FormatOrder(this);
      return Buffer.SB.ToString();
    }

    /// <summary>
    /// Задание префикса для всех имен полей, входящих в порядок сортировки.
    /// Создает и возвращает новый объект DBxOrder, в котором содержатся новые объекты DBxOrderItem.
    /// </summary>
    /// <param name="prefix">Префикс имени поля (обычно, имя ссылочного поля с точкой на конце)</param>
    /// <returns>Новый заполненный объект DBxOrder</returns>
    public DBxOrder SetColumnNamePrefix(string prefix)
    {
      DBxOrderPart[] Infos2 = new DBxOrderPart[Parts.Length];
      for (int i = 0; i < Parts.Length; i++)
        Infos2[i] = Parts[i].SetColumnNamePrefix(prefix);
      return new DBxOrder(Infos2);
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
    /// Именам полей может предшествовать знак "!", задающий сортировку данного поля
    /// по убыванию
    /// Функция возвращает null, если список полей пуст
    /// </summary>
    /// <param name="dataViewSort">Порядок сортировки в формате DataView.Sort</param>
    /// <returns>Объект DBxOrder или null</returns>
    public static DBxOrder FromDataViewSort(string dataViewSort)
    {
      return FromDataViewSort(dataViewSort, String.Empty);
    }

    /// <summary>
    /// Получение выражения из списка имен полей, разделенных запятыми
    /// После имен полей может идти "ASC" или "DESC", задающий сортировку данного поля
    /// по убыванию
    /// Функция возвращает null, если список полей пуст.
    /// Эта перегрузка предназначена для использования со ссылочным полем.
    /// </summary>
    /// <param name="dataViewSort">Порядок сортировки в формате DataView.Sort</param>
    /// <param name="refColumnName">Имя ссылочного поля, которое будет добавлено перед каждым столбцом в порядке сортировки</param>
    /// <returns>Объект DBxOrder или null</returns>
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
    /// Порядок сортировки по идентификатору "Id"
    /// </summary>
    public static readonly DBxOrder ById = new DBxOrder("Id");

    #endregion
  }


  /// <summary>
  /// Хранение ссылки на поле и признака ASC/DESC для одного элемента сортировки GROUP BY
  /// </summary>
  [Serializable]
  public struct DBxOrderPart
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый элемент порядка сортировки с заданным признаком ASC/DESC
    /// </summary>
    /// <param name="expression">Элемент порядка сортировки (обычно, DBxColumn)</param>
    /// <param name="sortOrder">Порядок сортировки: по возрастанию или по убыванию</param>
    public DBxOrderPart(DBxExpression expression, ListSortDirection sortOrder)
    {
      if (expression==null)
        throw new ArgumentNullException("expression");
      _Expression = expression;
      _SortOrder = sortOrder;
    }

    /// <summary>
    /// Создает новый элемент порядка сортировки по возрастанию
    /// </summary>
    /// <param name="expression">Элемент порядка сортировки (обычно, DBxColumn)</param>
    public DBxOrderPart(DBxExpression expression)
      :this(expression, ListSortDirection.Ascending)
    {
    }

    /// <summary>
    /// Создает новый элемент порядка сортировки для поля с заданным признаком ASC/DESC
    /// </summary>
    /// <param name="columnName">Имя поля для создания DBxColumn</param>
    /// <param name="sortOrder">Порядок сортировки: по возрастанию или по убыванию</param>
    public DBxOrderPart(string columnName, ListSortDirection sortOrder)
      :this(new DBxColumn(columnName), sortOrder)
    {
    }

    /// <summary>
    /// Создает новый элемент порядка сортировки для поля по возрастанию
    /// </summary>
    /// <param name="columnName">Имя поля для создания DBxColumn</param>
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
    private DBxExpression _Expression;

    /// <summary>
    /// Признак обрабтной сортировки DESC
    /// </summary>
    public ListSortDirection SortOrder { get { return _SortOrder; } }
    private ListSortDirection _SortOrder;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _Expression == null; } }

    /// <summary>
    /// Возвращает SQL-представления элемента в формате свойства DataView.Sort
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Expression == null)
        return String.Empty;

      DBxSqlBuffer Buffer = new DBxSqlBuffer();
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = false; // Нельзя делать true, т.к. при сортировке для DataView тип данных неизвестен
      Buffer.FormatExpression(_Expression, formatInfo);
      if (_SortOrder==ListSortDirection.Descending)
        Buffer.SB.Append(" DESC");
      return Buffer.ToString();
    }

    #endregion

    #region Клонирование

    /// <summary>
    /// Создает копию объекта DBxOrderItem с заданным префиксом (обычно, имя ссылочного поля с точкой).
    /// </summary>
    /// <param name="prefix">Префикс</param>
    /// <returns>Копия DBxOrderItemInfo с новым DBxOrderItem</returns>
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
