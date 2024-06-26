﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.ComponentModel;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// Расширенная сортировка табличного просмотра, основанного на наборе данных.
  /// Поддерживает задание полей с помощью выражений <see cref="DBxOrder"/>, в том числе, содержащих функции.
  /// Для этого в набор данных добавляются виртуальные вычисляемые столбцы.
  /// </summary>
  public class EFPDBxViewOrder : EFPDataViewOrder
  {
    #region Конструктор

    /// <summary>
    /// Создает объект сортировки
    /// </summary>
    /// <param name="name">Условное имя для порядка сортировки</param>
    /// <param name="order">Порядок сортировки для ORDER BY</param>
    public EFPDBxViewOrder(string name, DBxOrder order)
      :base(name, String.Empty)
    {
      if (order == null)
        throw new ArgumentNullException("order");
      _Order = order;

      string columnName;
      ListSortDirection sortOrder;
      _Order.GetFirstColumnInfo(out columnName, out sortOrder);
      SortInfo = new EFPDataGridViewSortInfo(columnName, sortOrder);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Порядок сортировки для SQL-запроса SELECT .. ORDER BY
    /// </summary>
    public DBxOrder Order { get { return _Order; } }
    private readonly DBxOrder _Order;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName + " (" + Order.ToString() + ")";
    }

    #endregion

    #region Выполнение сортировки

    /// <summary>
    /// Реализация интерфейса <see cref="INamedValuesAccess"/>.
    /// Возвращает значения по умолчанию для всех столбцов данных таблицы.
    /// </summary>
    private class DefValAccess : INamedValuesAccess
    {
      #region Конструктор

      public DefValAccess(DataTable table)
      {
        _Table = table;
      }

      private DataTable _Table;

      #endregion

      #region INamedValuesAccess Members

      public object GetValue(string name)
      {
        int p = _Table.Columns.IndexOf(name);
        if (p < 0)
          throw new ArgumentException("Неизвестный столбец \"" + name + "\"");

        return DataTools.GetEmptyValue(_Table.Columns[p].DataType);
      }

      public bool Contains(string name)
      {
        return _Table.Columns.Contains(name);
      }

      public string[] GetNames()
      {
        string[] names = new string[_Table.Columns.Count];
        for (int i = 0; i < names.Length; i++)
          names[i] = _Table.Columns[i].ColumnName;
        return names;
      }

      #endregion
    }

    /// <summary>
    /// Выполняет сортировку в табличного просмотре.
    /// Провайдер должен иметь тип, производный от <see cref="EFPDBxGridView"/>.
    /// Источником данных должен быть объект <see cref="DataView"/> (свойство <see cref="EFPDataGridView.SourceAsDataView"/> должно возвращать непустое значение).
    /// Устанавливает свойство <see cref="DataView.Sort"/>.
    /// Если среди компонентов сортировки есть объекты, отличные от <see cref="DBxColumn"/>,
    /// то в таблицу добавляются вычисляемые столбцы "$$Sort_XXX", для которых устанавливается свойство <see cref="DataColumn.Expression"/>.
    /// </summary>     
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого нужно установить порядок сортировки.</param>
    public override void PerformSort(IEFPDataView controlProvider)
    {
      //if (!(controlProvider is EFPDBxGridView))
      //  throw new InvalidOperationException("Неправильный тип ControlProvider: " + controlProvider.GetType().ToString());

      DataView dv = controlProvider.SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException("Источником данных просмотра не является DataView");

      // Обычно, порядок сортировки задается нормальными полями и вычисляемые столбцы
      // не нужны. При наличии выражения, например, DBxOrderColumnIfNull, его нельзя
      // задавать в DataView.Sort. Вместо этого создается вычисляемый столбец,
      // который содержит установленное свойство Expression. По нему выполняется
      // сортировка.
      // В одном DataOrder может быть несколько выражений, через запятую, а, также,
      // обычные поля. Вычисляемые столбцы используются только для вычисляемых
      // DataOrderItem, а для простых полей не используются

      DBxSqlBuffer buf = new DBxSqlBuffer();
      buf.Clear();
      for (int i = 0; i < Order.Parts.Length; i++)
      {
        if (i > 0)
          buf.SB.Append(',');
        if ((Order.Parts[i].Expression) is DBxColumn)
          // Простое поле
          buf.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
        else
        {
          // Вычисляемое поле
          DBxSqlBuffer buf2 = new DBxSqlBuffer();
          buf2.Clear();
          buf2.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
          string expr = buf2.SB.ToString();
          // Имя столбца
          string exprColName = "$$Sort_" + DataTools.MD5SumFromString(expr);

          // 16.10.2019
          // Тип данных для столбца проверяем по другому
          DefValAccess dva = new DefValAccess(dv.Table);
          object defVal = Order.Parts[i].Expression.GetValue(dva);
          if (defVal == null)
            throw new NullReferenceException("Для выражения \"" + expr + "\" порядка сортировки \"" + DisplayName + "\" не удалось вычислить значение по умолчанию, чтобы определить тип данных");
          Type dataType = defVal.GetType();


          // Столбец добавляется только при необходимости, чтобы исключить размножение
          // столбцов при каждом переключении сортировки
          if (!dv.Table.Columns.Contains(exprColName))
          {
            DataColumn col = new DataColumn(exprColName, dataType, expr);
            dv.Table.Columns.Add(col);
          }

          // В Sort добавляется имя вычисляемого столбца
          buf.SB.Append(exprColName);
        }

        // Признак обратной сортировки
        if (Order.Parts[i].SortOrder == ListSortDirection.Descending)
          buf.SB.Append(" DESC");
      }
      dv.Sort = buf.SB.ToString();
    }
    #endregion
  }

  /// <summary>
  /// Реализация свойства <see cref="EFPDBxGridView.Orders"/>.
  /// Также используется для <see cref="EFPDBxGridProducer.Orders"/>.
  /// </summary>
  public class EFPDBxViewOrders : EFPDataViewOrders
  {
    #region Методы добавления

    /// <summary>
    /// Создает порядок сортировки <see cref="EFPDBxViewOrder"/> и добавляет его в список.
    /// </summary>
    /// <param name="order">Порядок сортировки для SELECT .. ORDER BY</param>
    /// <param name="displayName">Отображаемое имя для меню</param>
    /// <param name="sortInfo">Столбец табличного просмотра для нажатия мышью</param>
    /// <returns>Созданный порядок сортировки</returns>
    public EFPDBxViewOrder Add(DBxOrder order, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
#if DEBUG
      if (order == null)
        throw new ArgumentNullException("order");
#endif     
      string name = order.ToString();
      EFPDBxViewOrder item = new EFPDBxViewOrder(order.ToString(), order);
      item.DisplayName = displayName;
      if (!sortInfo.IsEmpty)
        item.SortInfo = sortInfo;
      base.Add(item);
      //// Если первое поле в порядке сортировки присутствует в просмотре, то столбец
      //// можно щелкать по заголовку
      //EFPDataGridViewColumn Column = GetUsedColumn(Item); // 19.06.2019
      //if (Column != null)
      //  Column.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
      return item;
    }

    /// <summary>
    /// Создает порядок сортировки <see cref="EFPDBxViewOrder"/> и добавляет его в список.
    /// Столбец табличного просмотра для нажатия мышью определяется автоматически.
    /// </summary>
    /// <param name="order">Порядок сортировки для SELECT .. ORDER BY</param>
    /// <param name="displayName">Отображаемое имя для меню</param>
    /// <returns>Созданный порядок сортировки</returns>
    public EFPDBxViewOrder Add(DBxOrder order, string displayName)
    {
      return Add(order, displayName, EFPDataGridViewSortInfo.Empty);
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Описание одного порядка сортировки
  /// </summary>
  public class EFPGridProducerOrder
  {
    #region Конструкторы

    /// <summary>
    /// Основной конструктор из объекта DBxOrder. Поддерживаются как простые поля для сортировки, так и выражения
    /// </summary>
    /// <param name="dataOrder">Объект сортировки для SQL-запроса</param>
    /// <param name="displayName">Название порядка сортировки для выбора из меню</param>
    public EFPGridProducerOrder(DBxOrder dataOrder, string displayName)
    {
#if DEBUG
      if (dataOrder == null)
        throw new ArgumentNullException("dataOrder");
      if (String.IsNullOrEmpty(displayName))
        throw new ArgumentNullException("displayName");
#endif


#if XXX
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < DataOrder.Infos.Length; i++)
      {
        if (i > 0)
          sb.Append(',');

        if (DataOrder.Infos[i].Descend)
          sb.Append('!');

        if (DataOrder.Infos[i].Item is DBxOrderColumn)
          sb.Append(((DBxOrderColumn)(DataOrder.Infos[i].Item)).ColumnName);
        else
          throw new InvalidOperationException("Порядок сортировки " + DataOrder.ToString() + " в позиции " + i.ToString() + " содержит элемент, не являющийся простым полем. Преобразование в GridProducerOrder невозможно");
      }

      FColumnNames = sb.ToString();
#endif
      _DataOrder = dataOrder;


      _DisplayName = displayName;

      _ConfigName = dataOrder.ToString();
    }

    /// <summary>
    /// Конструктор из списка полей с запятыми.
    /// Можно задавать обратный порядок сортировки, но нельзя использовать выражения
    /// </summary>
    /// <param name="columnNames">Список имен столбцов для сортировки, разделенных запятыми. Поля не должны заключаться в скобки "[]". 
    /// Для указания порядка сортировки по убыванию задается символ "!" перед именем поля</param>
    /// <param name="displayName">Название порядка сортировки для выбора из меню</param>
    public EFPGridProducerOrder(string columnNames, string displayName)
      : this(DBxOrder.FromColumnNames(columnNames), displayName)
    {
    }

    /// <summary>
    /// Конструктор из объекта DBxColumns.
    /// Нельзя задать обратный порядок сортировки
    /// </summary>
    /// <param name="columnNames">Список имен полей для сортировки</param>
    /// <param name="displayName">Название порядка сортировки для выбора из меню</param>
    public EFPGridProducerOrder(DBxColumns columnNames, string displayName)
      : this(columnNames.AsString, displayName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает порядок сортировки как объект DBxOrder.
    /// Свойство задается в конструкторе
    /// </summary>
    public DBxOrder DataOrder { get { return _DataOrder; } }
    private DBxOrder _DataOrder;

    /// <summary>
    /// Отображаемое имя для выбора порядка сортировки через меню
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    /// <summary>
    /// Столбец сортировки для табличного просмотра. 
    /// Если SortInfo.IsEmpty=true (значение по умолчанию), то сортировка выподняется для первого столбца в
    /// списке ColumnNames
    /// </summary>
    public EFPDataGridViewSortInfo SortInfo { get { return _SortInfo; } set { _SortInfo = value; } }
    private EFPDataGridViewSortInfo _SortInfo;

    /// <summary>
    /// Список необходимых столбцов, которые все должны присутствовать в просмотре, чтобы данный порядок сортировки был разрешен
    /// По умолчанию возвращается список полей, по которым выполняется сортировка (свойство Columns)
    /// Установка специального списка столбцов может потребоваться, если в наборе данных присутствуют дублирующие поля, одно из
    /// которых используется для отображения (используется в GridProducerColumn), а второе содержит значение для сортировки (и не отображается)
    /// </summary>
    public DBxColumns RequiredColumns
    {
      get
      {
        if (_RequiredColumns == null)
          return Columns;
        else
          return _RequiredColumns;
      }
      set
      {
        _RequiredColumns = value;
      }
    }
    private DBxColumns _RequiredColumns;

    /// <summary>
    /// Возвращает массив столбцов в виде объекта DBxColumns
    /// Информация о порядке сортировки (по возрастанию или убыванию) теряется
    /// </summary>
    public DBxColumns Columns
    {
      get
      {
        if (_Columns == null)
        {
          DBxColumnList lst = new DBxColumnList();
          _DataOrder.GetColumnNames(lst);
          _Columns = new DBxColumns(lst);
        }
        return _Columns;
      }
    }
    private DBxColumns _Columns;

    /// <summary>
    /// Столбец сортировки по умолчанию
    /// Используется, когда столбец сортировки не задан в явном виде (SortInfo.IsEmpty=true)
    /// </summary>
    public EFPDataGridViewSortInfo DefaultSortInfo
    {
      get
      {
        DBxColumnList lst = new DBxColumnList();
        _DataOrder.Parts[0].Expression.GetColumnNames(lst);
        if (lst.Count == 0)
          throw new InvalidOperationException("Элемент сортировки " + _DataOrder.Parts[0].Expression.ToString() + " не вернул ни одного имени поля");

        return new EFPDataGridViewSortInfo(lst[0], _DataOrder.Parts[0].SortOrder==ListSortDirection.Desc);
      }
    }

    /// <summary>
    /// Текст для сохранения выбранного порядка в настройках пользователя
    /// </summary>
    public string ConfigName { get { return _ConfigName; } }
    private string _ConfigName;

    #endregion

    #region Методы

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName + " (" + ConfigName + ")";
    }

    /// <summary>
    /// Установить свойство SortInfo на столбец, заданный индексом ColumnIndex
    /// (нумерация столбцов начинается с нуля)
    /// </summary>
    /// <param name="columnIndex"></param>
    public void SetSortInfo(int columnIndex)
    {
#if DEBUG
      if (columnIndex < 0 || columnIndex >= DataOrder.Parts.Length)
        throw new ArgumentOutOfRangeException("columnIndex", columnIndex, "Индекс имени столбца вне диапазона");
#endif
      DBxOrderPart Item = DataOrder.Parts[columnIndex];

      DBxColumnList lst = new DBxColumnList();
      Item.Expression.GetColumnNames(lst);
      if (lst.Count > 0)
        SortInfo = new EFPDataGridViewSortInfo(lst[0], Item.SortOrder==ListSortDirection.Desc);
      else
        throw new InvalidOperationException("Элемент сортировки " + Item.Expression.ToString() + " не вернул ни одного имени поля");
    }

    #endregion
  }

  /// <summary>
  /// Коллекция порядков сортировки для свойства Gridproducer.Orders
  /// </summary>
  public class EFPGridProducerOrders : List<EFPGridProducerOrder>
  {
    #region Методы

    /// <summary>
    /// Добавить порядок сортировки.
    /// </summary>
    /// <param name="columnNames">Имена столбцов, разделенные запятыми. Для указания сортировки по убыванию, имени поля должен предшествать "!"</param>
    /// <param name="displayName">Отображаемое имя. Используется при построении локального меню просмотра</param>
    /// <param name="sortInfo">Указание способа выбора порядка сортировки в просмотре: по какому столбцу щелкать и вкакую сторону рисуется треугольник</param>
    /// <returns>Объект GridProducerOrder</returns>
    public EFPGridProducerOrder Add(string columnNames, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
      EFPGridProducerOrder Item = new EFPGridProducerOrder(columnNames, displayName);
      Item.SortInfo = sortInfo;
      base.Add(Item);
      return Item;
    }

    /// <summary>
    /// Добавить порядок сортировки.
    /// Эта версия автоматически определяет, по какому столбцу нужно тыкать мышью и куда рисуется треугольник,
    /// исходя из первого столбца в списке<paramref name="columnNames"/>. Если в просмотре не отображается первый
    /// столбец из списка сортировки, порядок сортировки можно будет выбирать только из локального меню
    /// </summary>
    /// <param name="columnNames">Имена столбцов, разделенные запятыми. Для указания сортировки по убыванию, имени поля должен предшествать "!"</param>
    /// <param name="displayName">Отображаемое имя. Используется при построении локального меню просмотра</param>
    /// <returns>Объект GridProducerOrder</returns>
    public EFPGridProducerOrder Add(string columnNames, string displayName)
    {
      EFPGridProducerOrder Item = new EFPGridProducerOrder(columnNames, displayName);
      base.Add(Item);
      return Item;
    }

    /// <summary>
    /// Добавить порядок сортировки.
    /// Эта версия задает отображаемое имя, равное списку столбцов и применяется редко.
    /// Эта версия автоматически определяет, по какому столбцу нужно тыкать мышью и куда рисуется треугольник,
    /// исходя из первого столбца в списке<paramref name="columnNames"/>. Если в просмотре не отображается первый
    /// столбец из списка сортировки, порядок сортировки можно будет выбирать только из локального меню
    /// </summary>
    /// <param name="columnNames">Имена столбцов, разделенные запятыми. Для указания сортировки по убыванию, имени поля должен предшествать "!"</param>
    /// <returns>Объект GridProducerOrder</returns>
    public EFPGridProducerOrder Add(string columnNames)
    {
      return Add(columnNames, columnNames);
    }

    #endregion
  }
#endif
}
