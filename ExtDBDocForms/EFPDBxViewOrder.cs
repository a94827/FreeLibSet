using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.ComponentModel;
using System.Data;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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


namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// Расширенная сортировка табличного просмотра, основанного на наборе данных
  /// Поддерживает задание полей с помощью выражений DBxOrder, в том числе, содержащих функции.
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

      string ColumnName;
      ListSortDirection SortOrder;
      _Order.GetFirstColumnInfo(out ColumnName, out SortOrder);
      SortInfo = new EFPDataGridViewSortInfo(ColumnName, SortOrder);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Порядок сортировки для SQL-запроса SELECT .. ORDER BY
    /// </summary>
    public DBxOrder Order { get { return _Order; } }
    private DBxOrder _Order;

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
    /// Реализация интерфейса INamedValuesAccess.
    /// Возвращает значения по умолчанию для всех столбцов данных таблицы
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
        string[] Names = new string[_Table.Columns.Count];
        for (int i = 0; i < Names.Length; i++)
          Names[i] = _Table.Columns[i].ColumnName;
        return Names;
      }

      #endregion
    }

    /// <summary>
    /// Выполняет сортировку в табличного просмотре.
    /// Провайдер должен иметь тип, производный от EFPDBxGridView.
    /// Источником данных должен быть объект DataView (свойство EFPDataGridView.SourceAsDataView должно возвращать непустое значение).
    /// Устанавливает свойство DataView.Sort.
    /// Если среди компонентов сортировки есть объекты, отличные от DBxOrderColumn,
    /// то в таблицу добавляются вычисляемые столбцы "$$Sort_XXX", для которых устанавливается свойство DataColumn.Expression.
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

      DBxSqlBuffer Buf = new DBxSqlBuffer();
      Buf.Clear();
      for (int i = 0; i < Order.Parts.Length; i++)
      {
        if (i > 0)
          Buf.SB.Append(',');
        if ((Order.Parts[i].Expression) is DBxColumn)
          // Простое поле
          Buf.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
        else
        {
          // Вычисляемое поле
          DBxSqlBuffer Buf2 = new DBxSqlBuffer();
          Buf2.Clear();
          Buf2.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
          string Expr = Buf2.SB.ToString();
          // Имя столбца
          string ExprColName = "$$Sort_" + DataTools.MD5SumFromString(Expr);

          // 16.10.2019
          // Тип данных для столбца проверяем по другому
          DefValAccess dva = new DefValAccess(dv.Table);
          object DefVal = Order.Parts[i].Expression.GetValue(dva, false);
          if (DefVal == null)
            throw new NullReferenceException("Для выражения \"" + Expr + "\" порядка сортировки \"" + DisplayName + "\" не удалось вычислить значение по умолчанию, чтобы определить тип данных");
          Type DataType = DefVal.GetType();


          // Столбец добавляется только при необходимости, чтобы исключить размножение
          // столбцов при каждом переключении сортировки
          if (!dv.Table.Columns.Contains(ExprColName))
          {
            DataColumn Col = new DataColumn(ExprColName, DataType, Expr);
            dv.Table.Columns.Add(Col);
          }

          // В Sort добавляется имя вычисляемого столбца
          Buf.SB.Append(ExprColName);
        }

        // Признак обратной сортировки
        if (Order.Parts[i].SortOrder == ListSortDirection.Descending)
          Buf.SB.Append(" DESC");
      }
      dv.Sort = Buf.SB.ToString();
    }
    #endregion
  }

  /// <summary>
  /// Реализация свойства EFPDBxGridView.Orders.
  /// Также используется для EFPDBxGridProducer.Orders.
  /// </summary>
  public class EFPDBxViewOrders : EFPDataViewOrders
  {
    #region Методы добавления

    /// <summary>
    /// Создает порядок сортировки EFPDBxGridViewOrder и добавляет его в список
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
      EFPDBxViewOrder Item = new EFPDBxViewOrder(order.ToString(), order);
      Item.DisplayName = displayName;
      if (!sortInfo.IsEmpty)
        Item.SortInfo = sortInfo;
      base.Add(Item);
      //// Если первое поле в порядке сортировки присутствует в просмотре, то столбец
      //// можно щелкать по заголовку
      //EFPDataGridViewColumn Column = GetUsedColumn(Item); // 19.06.2019
      //if (Column != null)
      //  Column.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
      return Item;
    }

    /// <summary>
    /// Создает порядок сортировки EFPDBxGridViewOrder и добавляет его в список.
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
