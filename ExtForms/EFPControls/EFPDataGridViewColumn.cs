// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using FreeLibSet.DBF;
using FreeLibSet.Formatting;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.Forms.Reporting;

/*
 * Дополнительные описания для стоблцов табличного просмотра
 * 
 * Свойство EFPDataGridView.Columns содержит псевдоколлекцию объектов EFPDataGridViewColumn,
 * по одному для каждого столбца. Объекты EFPDataGridViewColumn создаются автоматически,
 * независимо от того, как был добавлен столбец: с помощью EFPDataGridViewColumns.AddXXX(),
 * методом DataGridView.Columns.Add() или с помощью объекта GridProducer.
 * На самом деле объекты EFPDataGridViewColumn хранятся в DataGridViewColumn.Tag, а не
 * в коллекции EFPDataGridViewColumns. 
 */

namespace FreeLibSet.Forms
{
  #region GridProducerColumnCellToolTipTextNeededEventHandler

  /// <summary>
  /// Аргументы события <see cref="EFPDataGridViewColumn.CellToolTextNeeded"/>
  /// </summary>
  public class EFPDataGridViewCellToolTipTextNeededEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="column">Столбец табличного просмотра</param>
    /// <param name="rowIndex">Индекс строки</param>
    public EFPDataGridViewCellToolTipTextNeededEventArgs(EFPDataGridViewColumn column, int rowIndex)
    {
      _Column = column;
      _RowIndex = rowIndex;
      _ToolTipText = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Столбец провайдера табличного просмотра, к которому относится ячейка
    /// </summary>
    public EFPDataGridViewColumn Column { get { return _Column; } }
    private readonly EFPDataGridViewColumn _Column;

    /// <summary>
    /// Провайдер табличного просмотра.
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _Column.ControlProvider; } }

    /// <summary>
    /// Индекс строки табличного просмотра, к которому относится ячейка
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private readonly int _RowIndex;

    /// <summary>
    /// Возвращает строку данных для ячейки, если табличный просмотр привязан к таблице данных.
    /// Иначе возвращает null.
    /// </summary>
    public DataRow Row { get { return ControlProvider.GetDataRow(_RowIndex); } }

    /// <summary>
    /// Имя столбца табличного просмотра, к которому относится ячейка (свойство <see cref="EFPDataGridViewColumn.Name"/>)
    /// </summary>
    public string ColumnName { get { return _Column.Name; } }

    /// <summary>
    /// Сюда должен быть помещен текст подсказки
    /// </summary>
    public string ToolTipText { get { return _ToolTipText; } set { _ToolTipText = value; } }
    private string _ToolTipText;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPDataGridViewColumn.CellToolTextNeeded"/>
  /// </summary>
  /// <param name="sender">Источник события (столбец)</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewCellToolTipTextNeededEventHandler(object sender,
    EFPDataGridViewCellToolTipTextNeededEventArgs args);

  #endregion

  /// <summary>
  /// Базовая часть интерфейса <see cref="IEFPDataViewColumn"/>, которая реализуется в <see cref="EFPGridProducerColumn"/>
  /// </summary>
  public interface IEFPDataViewColumnBase
  {
    /// <summary>
    /// Условное имя столбца
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Отображаемое имя столбца. Используется в диалоге параметров страницы
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Ширина столбца в текстовых единицах (условная)
    /// </summary>
    double TextWidth { get; }

    /// <summary>
    /// Минимально допустимая ширина столбца в текстовых единицах (условная)
    /// </summary>
    double MinTextWidth { get; }

    /// <summary>
    /// Возвращает true, если столбец занимает свободное пространство в просмотре
    /// </summary>
    bool AutoGrow { get; }

    /// <summary>
    /// Имя группы для синхронизации размеров столбцов. Если несколько столбцов имеют
    /// одинаковое и непустое значение этого свойства, то изменение размера одного из
    /// этих столбцов приводит к синхронному изменению размеров остальных столбцов группы.
    /// </summary>
    string SizeGroup { get; }

    /// <summary>
    /// Возвращает true (обычно), если столбец может быть выведен в отчет
    /// </summary>
    bool Printable { get; }
  }

  /// <summary>
  /// Общая часть описаний для <see cref="EFPDataGridViewColumn"/> и <see cref="EFPDataTreeViewColumn"/>
  /// </summary>
  public interface IEFPDataViewColumn : IEFPDataViewColumnBase
  {
    #region Свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    IEFPDataView ControlProvider { get; }


    /// <summary>
    /// Если столбец был создан с помощью <see cref="EFPGridProducer"/>, то ссылка на генератор столбца,
    /// иначе - null.
    /// </summary>
    IEFPGridProducerColumn ColumnProducer { get; }

    /// <summary>
    /// Индекс столбца в списке <see cref="IEFPDataViewColumns"/>
    /// </summary>
    int Index { get; }

    /// <summary>
    /// Возвращает true, если столбец является видимым (актуально только для <see cref="EFPDataGridViewColumn"/> 
    /// </summary>
    bool Visible { get; }

    /// <summary>
    /// Ширина столбца в пикселях
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Ширина столбца в пунктах, в зависимости от разрешения экрана
    /// </summary>
    int WidthPt { get; }
    /**
    /// <summary>
    /// Возвращает true, если столбец может быть экспортирован в DBF-формат (все столбцы, кроме изображений).
    /// Эта настройка не может задаваться в прикладном коде
    /// </summary>
    bool IsDbfSupported { get; }

    /// <summary>
    /// Имя и тип DBF-поля, заданные в прикладном коде.
    /// Может возвращать пустую <see cref="DbfFieldInfo"/>, если значения должны быть определены автоматически.
    /// Пустое значение не означает, что поле нельзя экспортировать.
    /// Пользователь может переопределить имя поля в диалоге параметров.
    /// </summary>
    DbfFieldInfo DbfInfo { get; }
    **/
    #endregion
  }

  /// <summary>
  /// Общая часть коллекций столбцов <see cref="EFPDataGridViewColumns"/> и <see cref="EFPDataTreeViewColumns"/>
  /// </summary>
  public interface IEFPDataViewColumns : IEnumerable<IEFPDataViewColumn>
  {
    /// <summary>
    /// Возвращает количество столбцов в просмотре
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Доступ к столбцу по индексу.
    /// </summary>
    /// <param name="index">Индекс столбца </param>
    /// <returns>Столбец</returns>
    IEFPDataViewColumn this[int index] { get; }

    /// <summary>
    /// Доступ по имени столбца
    /// Если просмотр не содержит столбца с таким именем, возвращается null
    /// </summary>
    /// <param name="name">Имя столбца <see cref="IEFPDataViewColumnBase.Name"/></param>
    /// <returns>Столбец </returns>
    IEFPDataViewColumn this[string name] { get; }

    /// <summary>
    /// Возвращает столбец, связанный с заданным генератором столбцов.
    /// Возвращает null, если для данного генератора не было создано столбца в просмотре.
    /// </summary>
    /// <param name="columnProducer">Генератор столбцов</param>
    /// <returns>Столбец или null</returns>
    IEFPDataViewColumn this[IEFPGridProducerColumn columnProducer] { get; }

    /// <summary>
    /// Поиск столбца в табличном просмотре по имени
    /// </summary>
    /// <param name="name">Имя столбца <see cref="IEFPDataViewColumnBase.Name"/></param>
    /// <returns>Индекс столбца или (-1), если столбец не найден</returns>
    int IndexOf(string name);
  }

  /// <summary>
  /// Столбец в <see cref="EFPDataGridView"/> 
  /// </summary>
  public class EFPDataGridViewColumn : IEFPDataViewColumn
  {
    #region Защищенный конструктор

    internal EFPDataGridViewColumn(EFPDataGridView controlProvider, DataGridViewColumn gridColumn)
    {
      _ControlProvider = controlProvider;
      _GridColumn = gridColumn;

#if DEBUG
      if (!Object.ReferenceEquals(GridColumn.DataGridView, controlProvider.Control))
      {
        if (gridColumn.DataGridView == null)
          throw new ArgumentException("Столбец не присоединен к табличному просмотру", "gridColumn");
        else
          throw new ArgumentException("Столбец присоединен к другому табличному просмотру", "gridColumn");
      }
#endif

      //лишнее gridColumn.Tag = this;

      _CanIncSearch = false;

      _ColorType = EFPDataGridViewColorType.Normal;
      _LeftBorder = EFPDataGridViewBorderStyle.Default;
      _RightBorder = EFPDataGridViewBorderStyle.Default;

      _CustomOrderColumnName = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private readonly EFPDataGridView _ControlProvider;

    IEFPDataView IEFPDataViewColumn.ControlProvider { get { return ControlProvider; } }

    /// <summary>
    /// Столбец табличного просмотра
    /// </summary>
    public DataGridViewColumn GridColumn
    {
      get
      {
        // 03.10.2023
        // В процессе работы столбец DataGridViewColumn может быть заменен на другой экземпляр объекта (с сохранением свойств)
        if (!Object.ReferenceEquals(_GridColumn.DataGridView, _ControlProvider.Control))
          ReInitGridColumnRef();
        return _GridColumn;
      }
    }
    private DataGridViewColumn _GridColumn;

    private void ReInitGridColumnRef()
    {
      for (int i = 0; i < _ControlProvider.Control.ColumnCount; i++)
      {
        DataGridViewColumn col = _ControlProvider.Control.Columns[i];
        if (col.Tag == this)
        {
          _GridColumn = col;
          return;
        }
      }
    }


    /// <summary>
    /// Индекс столбца в табличном просмотре. Индекс соответствует порядку добавления столбцов. Столбцы могут быть скрыты или переставлены местами. 
    /// </summary>
    public int Index { get { return _GridColumn.Index; } }

    /// <summary>
    /// Имя столбца. В отличие от свойства <see cref="DataGridViewColumn.Name"/>, всегда определено
    /// </summary>
    public string Name
    {
      get
      {
        if (String.IsNullOrEmpty(GridColumn.Name))
        {
          if (String.IsNullOrEmpty(GridColumn.DataPropertyName))
            return "Column" + GridColumn.Index.ToString();
          else
            return GridColumn.DataPropertyName;
        }
        else
          return GridColumn.Name;
      }
      set
      {
        GridColumn.Name = value;
      }
    }

    /// <summary>
    /// Возвращает отображаемое наименование столбца
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
        {
          if (!String.IsNullOrEmpty(GridColumn.HeaderText))
            return GridColumn.HeaderText.Replace(Environment.NewLine, " ");
          if (!String.IsNullOrEmpty(GridColumn.ToolTipText))
            return GridColumn.ToolTipText.Replace(Environment.NewLine, " ");
          return Name;
        }
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Пользовательские данные (вместо занятого <see cref="DataGridViewBand.Tag"/>)
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Если столбец был создан с помощью <see cref="EFPGridProducer"/>, то ссылка на генератор столбца,
    /// иначе - null.
    /// </summary>
    public IEFPGridProducerColumn ColumnProducer { get { return _ColumnProducer; } set { _ColumnProducer = value; } }
    private IEFPGridProducerColumn _ColumnProducer;

    /// <summary>
    /// Может ли для столбца выполняться суммирование.
    /// По умолчанию - false - суммирование не выполняется.
    /// Если столбец создан с помощью <see cref="EFPGridProducer"/>, то используется значение свойство 
    /// </summary>
    public bool Summable { get { return _Summable; } set { _Summable = value; } }
    private bool _Summable;

    bool IEFPDataViewColumn.Visible { get { return GridColumn.Visible; } }

    #endregion

    #region Поиск по первым буквам

    /// <summary>
    /// Установка в true разрешает быстрый поиск по первым буквам для этого столбца
    /// </summary>
    public bool CanIncSearch { get { return _CanIncSearch; } set { _CanIncSearch = value; } }
    private bool _CanIncSearch;

    /// <summary>
    /// Используется при поиске по первым буквам
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } set { _MaskProvider = value; } }
    private IMaskProvider _MaskProvider;

    /// <summary>
    /// Реализация поиска по буквам для столбца
    /// </summary>
    /// <param name="searchStr">Строка, которую надо найти</param>
    /// <param name="nextSearch">Если true, то выполняется продолжение поиска (нажата F3), а не новый поиск</param>
    /// <returns>true, если поиск успешно выполнен</returns>
    internal bool PerformIncSearch(string searchStr, bool nextSearch)
    {
      // Для пустой строки поиска позиционируемся на первую строку
      if (String.IsNullOrEmpty(searchStr))
      {
        if (nextSearch)
        {
          if (ControlProvider.Control.Rows.Count > 1)
          {
            if (ControlProvider.CurrentRowIndex == ControlProvider.Control.RowCount - 1)
              ControlProvider.CurrentRowIndex = 0;
            else
              ControlProvider.CurrentRowIndex = ControlProvider.CurrentRowIndex + 1;
          }
          else
            return false;
        }
        else
        { // !NextSearch
          if (ControlProvider.Control.Rows.Count > 0)
            ControlProvider.CurrentRowIndex = 0;
          else
            return false;
        }
        return true;
      }

      DataView orgView = ControlProvider.SourceAsDataView;
      if (ControlProvider.IncSearchDataView == null)
      {
        ControlProvider.IncSearchDataViewIsManual = false;
        if (orgView == null)
          ControlProvider.IncSearchDataViewIsManual = true;
        else
        {
          if (String.IsNullOrEmpty(GridColumn.DataPropertyName))
            ControlProvider.IncSearchDataViewIsManual = true; // динамически вычисляемый столбец
          else
          {
            if (!orgView.Table.Columns.Contains(Name))
              ControlProvider.IncSearchDataViewIsManual = true; // динамически вычисляемый столбец, отсутствующий в
            // исходном набор
          }
        }

        if (ControlProvider.IncSearchDataViewIsManual)
        {
          // Создаем временную таблицу из одного столбца, в котором будеи искать
          EFPApp.BeginWait("Загрузка данных столбца для поиска по первым буквам");
          try
          {
            DataTable tempTable = new DataTable();
            tempTable.Columns.Add(Name, typeof(string));
            for (int i = 0; i < ControlProvider.Control.RowCount; i++)
            {
              ControlProvider.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
              EFPDataGridViewCellAttributesEventArgs CellArgs = ControlProvider.DoGetCellAttributes(GridColumn.Index);
              object v = CellArgs.Value;
              if (v == null || v is DBNull)
                tempTable.Rows.Add(String.Empty);
              else
                tempTable.Rows.Add(v.ToString());
            }
            ControlProvider.IncSearchDataView = tempTable.DefaultView;
          }
          finally
          {
            EFPApp.EndWait();
          }
        }
        else
        {
#if DEBUG
          if (orgView == null)
            throw new BugException("OrgView==null");
#endif
          ControlProvider.IncSearchDataView = new DataView(orgView.Table);
          ControlProvider.IncSearchDataView.Sort = orgView.Sort;
        }

        nextSearch = false; // некуда продолжать
      }

      DataRow foundRow; // Строка в EFPDataGridView.IncSearchDataView, на которую надо позиционироваться
      if (nextSearch)
      {
        if (ControlProvider.IncSearchDataView.Count < 2)
          return false; // некуда переходить

        DataRow currRow;
        if (ControlProvider.IncSearchDataViewIsManual)
          currRow = ControlProvider.IncSearchDataView.Table.Rows[ControlProvider.CurrentRowIndex];
        else
          currRow = ControlProvider.CurrentDataRow;
        int currIdx = DataTools.FindDataRowViewIndex(ControlProvider.IncSearchDataView, currRow);
        if (currIdx < 0)
          // Переместились на какую-то строку, которая не соответствует условию
          // Возвращаемся на первую подходящую (как если начать поиск)
          foundRow = ControlProvider.IncSearchDataView[0].Row;
        else
        {
          if (currIdx == ControlProvider.IncSearchDataView.Count - 1)
            // Были на последней строке - переходим к первой
            foundRow = ControlProvider.IncSearchDataView[0].Row;
          else
            foundRow = ControlProvider.IncSearchDataView[currIdx + 1].Row;
        }
      }
      else //!NextSearch
      {
        // Устанавливаем фильтр
        // Исходный фильтр:
        string orgViewRowFilter = ControlProvider.IncSearchDataViewIsManual ? String.Empty : orgView.RowFilter;
        string prevSearchFilter = ControlProvider.IncSearchDataView.RowFilter; // если не найдем - вернем обратно
        string s1;
        if (ControlProvider.IncSearchDataView.Table.Columns[Name].DataType == typeof(string))
          s1 = "[" + Name + "] " + DataTools.GetDataViewLikeExpressionString(searchStr);
        else
          s1 = "CONVERT([" + Name + "], 'System.String') " + DataTools.GetDataViewLikeExpressionString(searchStr);
        if (String.IsNullOrEmpty(orgViewRowFilter))
          ControlProvider.IncSearchDataView.RowFilter = s1;
        else
          ControlProvider.IncSearchDataView.RowFilter = "(" + orgViewRowFilter + ") AND " + s1;

        // Позиционируемся на первую строку в выбранном подмножестве
        if (ControlProvider.IncSearchDataView.Count == 0)
        {
          ControlProvider.IncSearchDataView.RowFilter = prevSearchFilter;
          return false;
        }
        else
          foundRow = ControlProvider.IncSearchDataView[0].Row;
      }

      // Выполняем позиционирование
      if (ControlProvider.IncSearchDataViewIsManual)
      {
        int p = ControlProvider.IncSearchDataView.Table.Rows.IndexOf(foundRow);
        ControlProvider.CurrentRowIndex = p;
      }
      else
        ControlProvider.CurrentDataRow = foundRow;

      return true;
    }

    #endregion

    #region Ширина столбца

    /// <summary>
    /// Ширина столбца в пикселях (дублирование <see cref="DataGridViewColumn.Width"/> ).
    /// При попытке установить ширину меньше DataGridView.MinimimumWidth устанавливается равной минимальной ширине,
    /// чтобы избежать выброса исключения <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public int Width
    {
      get { return GridColumn.Width; }
      set
      {
        GridColumn.Width = Math.Max(value, GridColumn.MinimumWidth); // 23.03.2018 
      }
    }

    /// <summary>
    /// Ширина столбца в пунктах, в зависимости от разрешения экрана
    /// При попытке установить ширину меньше DataGridView.MinimimumWidth устанавливается равной минимальной ширине,
    /// чтобы избежать выброса исключения <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public int WidthPt
    {
      get { return (int)(Math.Round(GridColumn.Width / ControlProvider.Measures.DpiX * 72)); }
      set { this.Width = (int)Math.Round(value * ControlProvider.Measures.DpiX / 72); }
    }

    /// <summary>
    /// Ширина столбца в текстовых единицах (условная)
    /// При попытке установить ширину меньше DataGridView.MinimimumWidth устанавливается равной минимальной ширине,
    /// чтобы избежать выброса исключения <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public double TextWidth
    {
      get { return ControlProvider.Measures.GetColumnWidthChars(GridColumn.Width); }
      set { this.Width = ControlProvider.Measures.GetTextColumnWidth(value); }
    }

    /// <summary>
    /// Минимальная ширина столбца в текстовых единицах (условная)
    /// При попытке установить ширину меньше DataGridView.MinimimumWidth устанавливается равной минимальной ширине,
    /// чтобы избежать выброса исключения <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public double MinTextWidth
    {
      get { return ControlProvider.Measures.GetColumnWidthChars(GridColumn.MinimumWidth); }
      set { GridColumn.MinimumWidth = ControlProvider.Measures.GetTextColumnWidth(value); }
    }

    bool IEFPDataViewColumnBase.AutoGrow { get { return GridColumn.InheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.Fill; } }

    #endregion

    #region Синхронное изменение размеров столбцов

    /// <summary>
    /// Имя группы для синхронизации размеров столбцов. Если несколько столбцов имеют
    /// одинаковое и непустое значение этого свойства, то изменение размера одного из
    /// этих столбцов приводит к синхронному изменению размеров остальных столбцов группы.
    /// По умолчанию - не задано
    /// </summary>
    public string SizeGroup { get { return _SizeGroup; } set { _SizeGroup = value; } }
    private string _SizeGroup;

    /// <summary>
    /// Установить размеры всех столбцов группы по текущему столбцу 
    /// </summary>
    internal void ResizeGroup()
    {
      if (String.IsNullOrEmpty(SizeGroup))
        return;
      for (int i = 0; i < ControlProvider.Columns.Count; i++)
      {
        EFPDataGridViewColumn thisCol = ControlProvider.Columns[i];
        if (thisCol == this)
          continue;
        if (thisCol.SizeGroup == SizeGroup)
        {
          if (thisCol.GridColumn.InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.None)
            continue; // 29.03.2013

          thisCol.GridColumn.Width = GridColumn.Width;
        }
      }
    }

    #endregion

    #region Подсказка для ячейки

    /// <summary>
    /// Событие вызывается при наведении курсора на ячейку, относящуюся к столбцу
    /// Обработчик может установить свойство ToolTipText.
    /// К подсказке могут быть добавлены дополнительные строки, если это указано
    /// в текущей конфигурации просмотра
    /// </summary>
    public event EFPDataGridViewCellToolTipTextNeededEventHandler CellToolTextNeeded;

    /// <summary>
    /// Получить текст подсказки для ячейки с помощью обработчика события CellToopTextNeeded. 
    /// </summary>
    /// <param name="rowIndex">Номер строки</param>
    /// <param name="text">Текст подсказки</param>
    internal void CallCellToolTextNeeded(int rowIndex, ref string text)
    {
      if (CellToolTextNeeded != null)
      {
        EFPDataGridViewCellToolTipTextNeededEventArgs args = new EFPDataGridViewCellToolTipTextNeededEventArgs(this, rowIndex);
        args.ToolTipText = text;
        CellToolTextNeeded(this, args);
        text = args.ToolTipText;
      }
    }

    #endregion

    #region Сортировка

    /// <summary>
    /// Имя столбца, используемого для произвольной сортировки
    /// Когда столбец добавляется методом AddXXX(), свойство устанавливается равным DataPropertyName
    /// </summary>
    public string CustomOrderColumnName
    {
      get { return _CustomOrderColumnName; }
      set { _CustomOrderColumnName = value; }
    }
    private string _CustomOrderColumnName;

    /// <summary>
    /// True, если разрешена произвольная сортировка по этому столбцу.
    /// Когда столбец добавляется методом AddXXX(), свойство устанавливается в true, если аргумент isDataColumn=true.
    /// Это свойство дублирует <see cref="CustomOrderColumnName"/> 
    /// </summary>
    public bool CustomOrderAllowed
    {
      get { return !String.IsNullOrEmpty(CustomOrderColumnName); }
      set
      {
        if (value)
          CustomOrderColumnName = Name;
        else
          CustomOrderColumnName = String.Empty;
      }
    }

    #endregion

    #region Раскраска столбцов

    /// <summary>
    /// Цвет столбца. Используется, если для текущей строки в событии 
    /// <see cref="EFPDataGridView.GetRowAttributes"/> не определен более приоритетный цвет
    /// На момент вызова обработчика <see cref="EFPDataGridView.GetCellAttributes"/>, если он задан,
    /// цвет уже применен
    /// </summary>
    public EFPDataGridViewColorType ColorType { get { return _ColorType; } set { _ColorType = value; } }
    private EFPDataGridViewColorType _ColorType;

    /// <summary>
    /// Окрашивание всех значений в столбце "серым" цветом
    /// </summary>
    public bool Grayed { get { return _Grayed; } set { _Grayed = value; } }
    private bool _Grayed;

    /// <summary>
    /// Рамка для левой границы столбца.
    /// На момент вызова обработчика <see cref="EFPDataGridView.GetCellAttributes"/>, если он задан,
    /// стиль рамки уже применен
    /// </summary>
    public EFPDataGridViewBorderStyle LeftBorder { get { return _LeftBorder; } set { _LeftBorder = value; } }
    private EFPDataGridViewBorderStyle _LeftBorder;

    /// <summary>
    /// Рамка для правой границы столбца.
    /// На момент вызова обработчика <see cref="EFPDataGridView.GetCellAttributes"/>, если он задан,
    /// стиль рамки уже применен
    /// </summary>
    public EFPDataGridViewBorderStyle RightBorder { get { return _RightBorder; } set { _RightBorder = value; } }
    private EFPDataGridViewBorderStyle _RightBorder;

    #endregion

    #region Печать

    /// <summary>
    /// Возвращает true для всех столбцов, кроме изображений
    /// </summary>
    public bool Printable
    {
      get
      {
        if (GridColumn is DataGridViewImageColumn)
          return false;
        else
          return true;
      }
    }

    /// <summary>
    /// Многострочные заголовки при печати таблицы
    /// </summary>
    public string[] PrintHeaders { get { return _PrintHeaders; } set { _PrintHeaders = value; } }
    private string[] _PrintHeaders;

    /// <summary>
    /// Многострочные заголовки при печати таблицы (свойство PrintHeaders)
    /// Версия для установки в виде одной строки с заменой символов:
    /// "|" - разделитель многострочного заголовка
    /// "^" - мягкий перенос
    /// "_" - неразрывный пробел
    /// </summary>
    public string PrintHeadersSpec
    {
      get { return DataTools.StrFromSpecCharsArray(PrintHeaders); }
      set { PrintHeaders = DataTools.StrToSpecCharsArray(value); }
    }

    /// <summary>
    /// Устанавливает признак печати столбца.
    /// Если <see cref="Printable"/>=false, то никаких действий не выполняется.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <param name="value">Признак печати</param>
    public void SetPrinted(string defCfgCode, bool value)
    {
      GetSettings(defCfgCode).View.SetColumnPrinted(this, value);
    }

    /// <summary>
    /// Возвращает признак печати столбца.
    /// Если <see cref="Printable"/>=false, то возвращает false.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <returns>true, если столбец должен быть напечатан</returns>
    public bool GetPrinted(string defCfgCode)
    {
      return GetSettings(defCfgCode).View.GetColumnPrinted(this);
    }

    /// <summary>
    /// Признак печати столбца в настройках по умолчанию.
    /// </summary>
    public bool Printed
    {
      get { return GetPrinted(String.Empty); }
      set { SetPrinted(String.Empty, value); }
    }

    /// <summary>
    /// Установить ширину столбца при печати в единицах 0.1мм.
    /// Если установлено свойство <see cref="SizeGroup"/>, то ширина будет установлена для всех столбцов данной группы.
    /// Нулевое значение задает размер по умолчанию, исходя из параметров шрифта и ширины колонки в просмотре.
    /// Если установлено свойство <see cref="PrintAutoGrow"/> или вызван метод <see cref="SetPrintAutoGrow(string, bool)"/>,
    /// то задает минимальную ширину столбца, которая может быть увеличена для заполнения области печати.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <param name="value">Ширина</param>
    public void SetPrintWidth(string defCfgCode, int value)
    {
      GetSettings(defCfgCode).View.SetColumnPrintWidth(this, value);
    }

    /// <summary>
    /// Получить ширину столбца при печати в единицах 0.1мм.
    /// Если установлено свойство <see cref="PrintAutoGrow"/> или вызван метод <see cref="SetPrintAutoGrow(string, bool)"/>,
    /// то возвращает минимальную ширину столбца, которая может быть увеличена для заполнения области печати.
    /// Если ширина не была явно установлена, возвращает нулевое значение.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <returns>Ширина или 0</returns>
    public int GetPrintWidth(string defCfgCode)
    {
      return GetSettings(defCfgCode).View.GetColumnPrintWidth(this);
    }

    /// <summary>
    /// Ширину столбца при печати в единицах 0.1мм в настройках по умолчанию.
    /// Если ширина не была явно установлена, возвращает нулевое значение, при этом размер столбца будет определен автоматически, исходя из
    /// ширины столбца в просмотре и параметров шрифта, заданных для печати.
    /// </summary>
    public int PrintWidth
    {
      get { return GetPrintWidth(String.Empty); }
      set { SetPrintWidth(String.Empty, value); }
    }

    /// <summary>
    /// Получить ширину столбца при печати в единицах 0.1мм.
    /// Если установлено свойство <see cref="PrintAutoGrow"/> или вызван метод <see cref="SetPrintAutoGrow(string, bool)"/>,
    /// то возвращает минимальную ширину столбца, которая может быть увеличена для заполнения области печати.
    /// Если ширина не была явно установлена, возвращает значение, определяемое из ширины столбца на экране и параметров шрифта при печати.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <returns>Ширина</returns>
    public int GetRealPrintWidth(string defCfgCode)
    {
      return GetSettings(defCfgCode).View.GetRealColumnWidth(this, GetSettings(defCfgCode).Font);
    }

    /// <summary>
    /// Получить ширину столбца при печати в единицах 0.1мм в настройке по умолчанию.
    /// Если установлено свойство <see cref="PrintAutoGrow"/> или вызван метод <see cref="SetPrintAutoGrow(string, bool)"/>,
    /// то возвращает минимальную ширину столбца, которая может быть увеличена для заполнения области печати.
    /// Если ширина не была явно установлена, возвращает значение, определяемое из ширины столбца на экране и параметров шрифта при печати.
    /// </summary>
    public int RealPrintWidth { get { return GetRealPrintWidth(String.Empty); } }

    /// <summary>
    /// Задать признак автоматического увеличения ширины столбца при печати для заполнения ширины столбца.
    /// Если true, то <see cref="SetPrintWidth(string, int)"/> задает минимальную ширину столбца.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <param name="value">true, если столбец должен участвовать в заполнении листа при печати</param>
    public void SetPrintAutoGrow(string defCfgCode, bool value)
    {
      GetSettings(defCfgCode).View.SetColumnAutoGrow(this, value);
    }

    /// <summary>
    /// Получить признак автоматического увеличения ширины столбца при печати для заполнения ширины столбца.
    /// Если true, то <see cref="SetPrintWidth(string, int)"/> задает минимальную ширину столбца.
    /// </summary>
    /// <param name="defCfgCode">Имя набора настроек по умолчанию. Пустая строка - основная настройка</param>
    /// <returns>true, если столбец должен участвовать в заполнении листа при печати</returns>
    public bool GetPrintAutoGrow(string defCfgCode)
    {
      return GetSettings(defCfgCode).View.GetColumnAutoGrow(this);
    }

    /// <summary>
    /// Признак автоматического увеличения ширины столбца при печати для заполнения ширины столбца в настройках по умолчанию.
    /// Если true, то <see cref="PrintWidth"/> задает минимальную ширину столбца.
    /// По умолчанию свойство возвращает true, если <see cref="DataGridViewColumn.AutoSizeMode"/>=<see cref="DataGridViewAutoSizeColumnMode.Fill"/>, 
    /// например, если столбец был создан с помощью <see cref="EFPDataGridViewColumns.AddTextFill(string)"/>.
    /// </summary>
    public bool PrintAutoGrow
    {
      get { return GetPrintAutoGrow(String.Empty); }
      set { SetPrintAutoGrow(String.Empty, value); }
    }

    private BRDataViewMenuOutSettings GetSettings(string defCfgCode)
    {
      BRDataGridViewMenuOutItem outItem = ControlProvider.DefaultOutItem;
      if (outItem == null)
        throw new InvalidOperationException("Стандартный вариант печати табличного просмотра был удален");
      return outItem[defCfgCode];
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Горизонтальное выравнивание ячейки
    /// Осуществляет доступ к свойству Column.DefaultCellStyle.Alignment,
    /// преобразуя его в формат выравнивания для текста.
    /// При установке свойства существующее выравнивание ячейки по вертикали сохраняется
    /// </summary>
    public HorizontalAlignment TextAlign
    {
      get
      {
        return WinFormsTools.GetTextAlign(GridColumn.DefaultCellStyle.Alignment);
      }
      set
      {
        GridColumn.DefaultCellStyle.Alignment = WinFormsTools.GetCellAlign(value, GridColumn.DefaultCellStyle.Alignment);
      }
    }

    /// <summary>
    /// Выполняет вызов <see cref="DataGridView.InvalidateColumn(int)"/>.
    /// </summary>
    public void InvalidateColumn()
    {
      ControlProvider.Control.InvalidateColumn(GridColumn.Index);
    }

    /// <summary>
    /// Возвращает свойство <see cref="Name"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Name;
    }

    #endregion

    #region Для DBF-файлов

    /// <summary>
    /// Описание поля для сохранения в Dbf формате, если было задано в явном
    /// виде или получено из <see cref="EFPGridProducerColumn"/>. Иначе-пустая структура
    /// </summary>
    public DbfFieldInfo DbfInfo { get { return _DbfInfo; } set { _DbfInfo = value; } }
    private DbfFieldInfo _DbfInfo;

    /// <summary>
    /// Имя DBF-поля для этого столбца по умолчанию
    /// Текущее значение свойства <see cref="DbfInfo"/> не учитывается
    /// </summary>
    public string DefaultDbfName
    {
      get
      {
        string nm;
        if (String.IsNullOrEmpty(GridColumn.Name))
          nm = GridColumn.DataPropertyName;
        else
          nm = GridColumn.Name;
        if (!String.IsNullOrEmpty(nm))
        {
          nm = nm.ToUpper();
          if (DbfFieldInfo.IsValidFieldName(nm))
            return nm;
        }
        return "FIELD" + (GridColumn.Index + 1).ToString("00000");
      }
    }

    /// <summary>
    /// Описание поля по умолчанию
    /// Не бывает значение null. При первой попытке чтения свойства анализирует
    /// исходные данные для определения типа столбца
    /// </summary>
    public DbfFieldInfo DefaultDbfInfo
    {
      get
      {
        if (_DefaultDbfInfo.IsEmpty)
          _DefaultDbfInfo = CreateDefaultDbfInfo();

        return _DefaultDbfInfo;
      }
      //set
      //{
      //  FDefaultDbfInfo = value;
      //}
    }
    private DbfFieldInfo _DefaultDbfInfo;

    private DbfFieldInfo CreateDefaultDbfInfo()
    {
      if (GridColumn is DataGridViewCheckBoxColumn)
        return DbfFieldInfo.CreateBool(DefaultDbfName);

      if (GridColumn is DataGridViewTextBoxColumn)
      {
        switch (SizeGroup)
        {
          case "Date":
            return DbfFieldInfo.CreateDate(DefaultDbfName);
          case "DateTime":
            return DbfFieldInfo.CreateString(DefaultDbfName, 19);
        }

        Type t = ((DataGridViewTextBoxColumn)GridColumn).ValueType;
        if (t == null)
          t = typeof(string);
        switch (t.Name)
        {
          case "Byte":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 3);
          case "SByte":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 4);
          case "Int16":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 6);
          case "UInt16":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 5);
          case "Int32":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 11);
          case "UInt32":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 10);
          case "Int64":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 22); // ???
          case "UInt64":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 21);
          case "Single":
          case "Double":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 20, 8); // ???
          case "Decimal":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 20, 2);
        }

        if (!String.IsNullOrEmpty(GridColumn.DataPropertyName))
        {
          DataTable tbl = ControlProvider.SourceAsDataTable;
          if (tbl != null)
          {
            int p = tbl.Columns.IndexOf(GridColumn.DataPropertyName);
            if (p >= 0)
            {
              if (tbl.Columns[p].MaxLength > 0)
              {
                if (tbl.Columns[p].MaxLength <= 255)
                  return DbfFieldInfo.CreateString(DefaultDbfName, tbl.Columns[p].MaxLength);
                else
                  return DbfFieldInfo.CreateMemo(DefaultDbfName);
              }
            }
          }
        }
      }

      // Возвращаем строковое поле

      // Перебираем все значения, выискивая самое длинное
      int w = 1;
      EFPApp.BeginWait("Определение ширины столблца \"" + GridColumn.HeaderText + "\"");
      try
      {
        for (int i = 0; i < ControlProvider.Control.RowCount; i++)
        {
          ControlProvider.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
          EFPDataGridViewCellAttributesEventArgs CellArgs = ControlProvider.DoGetCellAttributes(GridColumn.Index);
          if (CellArgs.Value != null)
          {
            w = Math.Max(w, CellArgs.Value.ToString().Length);
            if (w > 255)
              break;
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (w > 255)
        return DbfFieldInfo.CreateMemo(DefaultDbfName);
      else
        return DbfFieldInfo.CreateString(DefaultDbfName, w);
    }

    #endregion
  }

  /// <summary>
  /// Псевдоколлекция столбцов - реализация свойства <see cref="EFPDataGridView.Columns"/>
  /// </summary>
  public class EFPDataGridViewColumns : IEnumerable<EFPDataGridViewColumn>, IEFPDataViewColumns
  {
    #region Защищенный конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого создается столбец</param>
    internal protected EFPDataGridViewColumns(EFPDataGridView controlProvider)
    {
      _ControlProvider = controlProvider;
      controlProvider.Control.ColumnWidthChanged += new DataGridViewColumnEventHandler(Grid_ColumnWidthChanged);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private EFPDataGridView _ControlProvider;

    #endregion

    #region Свойства и методы доступа к элементам коллекции

    /// <summary>
    /// Возвращает реальное количество столбцов табличного просмотра <see cref="DataGridView.ColumnCount"/>
    /// </summary>
    public int Count
    {
      get
      {
        return ControlProvider.Control.Columns.Count;
      }
    }

    /// <summary>
    /// Доступ к столбцу, соответствующему объекту <see cref="DataGridViewColumn"/>
    /// </summary>
    /// <param name="gridColumn">Столбец табличного просмотра</param>
    /// <returns>Столбец в <see cref="EFPDataGridView"/></returns>
    public EFPDataGridViewColumn this[DataGridViewColumn gridColumn]
    {
      get
      {
#if DEBUG
        if (gridColumn == null)
          throw new ArgumentNullException("gridColumn");
        if (gridColumn.DataGridView != null)
        {
          if (!Object.ReferenceEquals(gridColumn.DataGridView, ControlProvider.Control))
            throw new ArgumentException("Столбец относится к другому табличному просмотру", "gridColumn");
        }
#endif
        if (gridColumn.Tag == null)
          gridColumn.Tag = new EFPDataGridViewColumn(ControlProvider, gridColumn); // исправлено 29.06.2021
        EFPDataGridViewColumn res = (EFPDataGridViewColumn)(gridColumn.Tag);
#if DEBUG
        if (!Object.ReferenceEquals(gridColumn, res.GridColumn))
          throw new BugException("GridColumn");
#endif

        return res;
      }
    }

    /// <summary>
    /// Доступ к столбцу по индексу.
    /// </summary>
    /// <param name="index">Индекс столбца в списке <see cref="DataGridViewColumnCollection"/></param>
    /// <returns>Столбец в <see cref="EFPDataGridView"/></returns>
    public EFPDataGridViewColumn this[int index]
    {
      get
      {
        return this[ControlProvider.Control.Columns[index]];
      }
    }

    IEFPDataViewColumn IEFPDataViewColumns.this[int index]
    {
      get { return this[index]; }
    }


    /// <summary>
    /// Доступ по имени столбца
    /// Если просмотр не содержит столбца с таким именем, возвращается null
    /// </summary>
    /// <param name="name">Имя столбца <see cref="EFPDataGridViewColumn.Name"/></param>
    /// <returns>Столбец в <see cref="EFPDataGridView"/></returns>
    public EFPDataGridViewColumn this[string name]
    {
      get
      {
        if (String.IsNullOrEmpty(name))
          return null;
        for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
        {
          if (this[i].Name == name)
            return this[i];
        }
        return null;
      }
    }

    IEFPDataViewColumn IEFPDataViewColumns.this[string name]
    {
      get { return this[name]; }
    }

    /// <summary>
    /// Возвращает столбец <see cref="EFPDataGridViewColumn"/>, связанные с заданным генератором столбцов.
    /// Возвращает null, если для данного генератора не было создано столбца в просмотре.
    /// </summary>
    /// <param name="columnProducer">Генератор столбцов</param>
    /// <returns>Столбец или null</returns>
    public EFPDataGridViewColumn this[IEFPGridProducerColumn columnProducer]
    {
      get
      {
        if (columnProducer == null)
          return null;

        for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
        {
          if (this[i].ColumnProducer == columnProducer)
            return this[i];
        }
        return null;
      }
    }

    IEFPDataViewColumn IEFPDataViewColumns.this[IEFPGridProducerColumn columnProducer]
    {
      get { return this[columnProducer]; }
    }


    /// <summary>
    /// Поиск столбца в табличном просмотре по имени
    /// </summary>
    /// <param name="name">Имя столбца <see cref="EFPDataGridViewColumn.Name"/></param>
    /// <returns>Индекс столбца или (-1), если столбец не найден</returns>
    public int IndexOf(string name)
    {
      if (String.IsNullOrEmpty(name))
        return -1;
      for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
      {
        if (this[i].Name == name)
          return i;
      }
      return -1;
    }

    #endregion

    #region Методы добавления столбцов

    #region Текст

    /// <summary>
    /// Добавляет текстовый столбец <see cref="DataGridViewTextBoxColumn"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца <see cref="DataColumn.ColumnName"/></param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство <see cref="DataGridViewColumn.DataPropertyName"/>).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    /// <param name="alignment">Горизонтальное выравнивание</param>
    /// <returns>Объект столбца табличного просмотра (а не <see cref="EFPDataGridViewColumn"/>)</returns>
    public DataGridViewTextBoxColumn AddText(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth, DataGridViewContentAlignment alignment)
    {
      if (textWidth < 1)
        throw new ArgumentOutOfRangeException("textWidth");
      if (minTextWidth < 1)
        throw new ArgumentOutOfRangeException("minTextWidth");

      if (minTextWidth > textWidth)
        throw new ArgumentOutOfRangeException("minTextWidth", minTextWidth,
          "Минимальная ширина столбца (" + minTextWidth.ToString() + ") не может быть больше, чем устанавливаемая (" + textWidth.ToString() + ")");

      DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
      col.Name = columnName;
      if (isDataColumn)
        col.DataPropertyName = columnName;
      col.HeaderText = headerText;
      col.Width = textWidth * 8 + 10; // !!!
      col.MinimumWidth = minTextWidth * 8 + 10; // !!!
      col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      col.FillWeight = 1; // 08.02.2017

      /*
       * 08.02.2017
       * В Net Framework, похоже, есть ошибка.
       * Сумма значений свойств DataGridViewColumn.FillWeight, добавленных в просмотр, не должна превышать 32767.
       * Учитываются и те столбцы, в которых свойство не используется (в т.ч. с явно заданной шириной).
       * Значением свойства по умолчанию является 100, поэтому максимум может быть 320 столбцов.
       * Чтобы снять это ограничение, устанавливаем значение, равное 1. Оно будет переопредено для столбцов,
       * для которых задан пропорциональный режим ширины (AddTextFill())
       */

      switch (ControlProvider.Control.SelectionMode)
      {
        case DataGridViewSelectionMode.ColumnHeaderSelect:
        case DataGridViewSelectionMode.FullColumnSelect:
          col.SortMode = DataGridViewColumnSortMode.Programmatic; // 28.10.2015 Иначе не работает
          break;
      }

      ControlProvider.Control.Columns.Add(col);
      col.DefaultCellStyle.Alignment = alignment;
      //col.DefaultCellStyle.WrapMode = DataGridViewTriState.False; // 23.03.2018

      //GridHandlerColumn ghCol = GridHandler.Columns[col];
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateString(ghCol.DefaultDbfName, TextWidth);

      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.CustomOrderAllowed = isDataColumn;

      return col;
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddText(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth)
    {
      return AddText(columnName, isDataColumn, headerText, textWidth, minTextWidth, DataGridViewContentAlignment.MiddleLeft);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn.
    /// Задает горизонтальное выравнивание по левому краю.
    /// Задает минимальную ширину столбца равной 5 символам, если меньшее значение не задано в параметре <paramref name="textWidth"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddText(string columnName, bool isDataColumn, string headerText, int textWidth)
    {
      return AddText(columnName, isDataColumn, headerText, textWidth, Math.Min(textWidth, 5));
    }


    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn.
    /// Задает горизонтальное выравнивание по левому краю.
    /// Задает ширину столбца равной 10 символам, а минимальную - 5 символам.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddText(string columnName, bool isDataColumn, string headerText)
    {
      return AddText(columnName, isDataColumn, headerText, 10);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Задает горизонтальное выравнивание по левому краю.
    /// Задает ширину столбца равной 10 символам, а минимальную - 5 символам.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddText(string columnName)
    {
      return AddText(columnName, true, columnName, 10);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn,
    /// занимающий определенную часть свободного места табличного просмотра.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="fillWeight">Процент свободного места, занимаемый столбцом.
    /// Не обязательно, чтобы все столбцы занимали ровно 100%.
    /// См. описание свойства DataGridViewColumn.FillWeight при AutoSizeMode=Fill.
    /// В отличие от DataGridViewColumn.FillWeight, параметр имеет целочисленный тип.</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddTextFill(string columnName, bool isDataColumn, string headerText, int fillWeight, int minTextWidth)
    {
      DataGridViewTextBoxColumn col = AddText(columnName, isDataColumn, headerText, minTextWidth, minTextWidth);
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = fillWeight;
      return col;
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn,
    /// занимающий все свободное места табличного просмотра (FillWeight=100).
    /// Минимальная ширина столбца равна 5 символам.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddTextFill(string columnName, bool isDataColumn, string headerText)
    {
      return AddTextFill(columnName, isDataColumn, headerText, 100, 5);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn,
    /// занимающий все свободное места табличного просмотра (FillWeight=100).
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Минимальная ширина столбца равна 5 символам.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddTextFill(string columnName)
    {
      return AddTextFill(columnName, true, columnName, 100, 5);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn,
    /// занимающий определенную часть свободного места табличного просмотра.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Минимальная ширина столбца равна 5 символам.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <param name="fillWeight">Процент свободного места, занимаемый столбцом.
    /// Не обязательно, чтобы все столбцы занимали ровно 100%.
    /// См. описание свойства DataGridViewColumn.FillWeight при AutoSizeMode=Fill.
    /// В отличие от DataGridViewColumn.FillWeight, параметр имеет целочисленный тип.</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddTextFill(string columnName, int fillWeight)
    {
      return AddTextFill(columnName, true, columnName, fillWeight, 5);
    }

    #endregion

    #region Дата и время

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты без времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddDate(string columnName, bool isDataColumn, string headerText)
    {
      return AddDateTime(columnName, isDataColumn, headerText, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты без времени.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddDate(string columnName)
    {
      return AddDateTime(columnName, true, columnName, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты и времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddDateTime(string columnName, bool isDataColumn, string headerText)
    {
      return AddDateTime(columnName, isDataColumn, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты и времени.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddDateTime(string columnName)
    {
      return AddDateTime(columnName, true, columnName, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты и/или времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="kind">Тип даты и/или времени</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddDateTime(string columnName, bool isDataColumn, string headerText, EditableDateTimeFormatterKind kind)
    {
      EditableDateTimeFormatter formatter = EditableDateTimeFormatters.Get(kind);
      DataGridViewTextBoxColumn col = AddText(columnName, isDataColumn, headerText, formatter.TextWidth, formatter.TextWidth);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      col.DefaultCellStyle.Format = formatter.Format;
      col.DefaultCellStyle.FormatProvider = formatter.FormatProvider;
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = kind.ToString();
      ghCol.MaskProvider = formatter.MaskProvider;

      //ghCol.CanIncSearch = true;

      return col;
    }

    #endregion

    #region Числа

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения числа с фиксированной точкой.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах, включая знак числа и десятичную точку</param>
    /// <param name="decimalPlaces">Количество знаков после десятичной точки</param>
    /// <param name="sizeGroup">Группа столбцов, имеющих одинаковую ширину.
    /// См. описание свойства EFPDataGridViewColumn.SizeGroup.</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddFixedPoint(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
#if DEBUG
      if (decimalPlaces < 0)
        throw new ArgumentException("Количество знаков после запятой не может быть отрицательным", "DecimalPlaces");
#endif
      DataGridViewTextBoxColumn col = AddText(columnName, isDataColumn, headerText, textWidth, decimalPlaces + 2);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      if (decimalPlaces > 0)
        col.DefaultCellStyle.Format = "0." + new string('0', decimalPlaces);
      else
        col.DefaultCellStyle.Format = "0";
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = sizeGroup;
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateNum(ghCol.DefaultDbfName, TextWidth, DecimalPlaces); 
      return col;
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения целого числа.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах, включая знак числа</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddInt(string columnName, bool isDataColumn, string headerText, int textWidth)
    {
      DataGridViewTextBoxColumn col = AddText(columnName, isDataColumn, headerText, textWidth);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      //DocGridHandler.Columns[col].SizeGroup = "Int";
      //EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateNum(ghCol.DefaultDbfName, TextWidth);
      return col;
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения целого числа.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Ширина столбца устанавливается равной 5 символам
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewTextBoxColumn AddInt(string columnName)
    {
      return AddInt(columnName, true, columnName, 5);
    }

    #endregion

    #region Логический

    /// <summary>
    /// Добавляет столбец-флажок DataGridViewCheckBoxColumn.
    /// Свойство EFPDataGridViewColumn.SizeGroup устанавливается равным "CheckBox".
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewCheckBoxColumn AddBool(string columnName, bool isDataColumn, string headerText)
    {
      ExtDataGridViewCheckBoxColumn col = new ExtDataGridViewCheckBoxColumn();
      col.Name = columnName;
      if (isDataColumn)
        col.DataPropertyName = columnName;
      col.HeaderText = headerText;
      col.Width = _ControlProvider.Measures.CheckBoxColumnWidth;
      col.MinimumWidth = col.Width;
      col.FillWeight = 1; // 08.02.2017
      ControlProvider.Control.Columns.Add(col);
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = "CheckBox";
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateBool(ghCol.DefaultDbfName);
      ghCol.CustomOrderAllowed = isDataColumn;

      return col;
    }

    /// <summary>
    /// Добавляет столбец-флажок DataGridViewCheckBoxColumn.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Свойство EFPDataGridViewColumn.SizeGroup устанавливается равным "CheckBox".
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewCheckBoxColumn AddBool(string columnName)
    {
      return AddBool(columnName, true, columnName);
    }

    #endregion

    #region Значок

    /// <summary>
    /// Добавляет столбец-флажок DataGridViewImageColumn.
    /// Столбец этого типа не может привязываться к полю данных.
    /// Предполагается, что устанавливается обработчик события EFPDataGridView.CellValueNeeded
    /// для получения изображения.
    /// Заголовок столбца не задается.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewImageColumn AddImage(string columnName)
    {
      DataGridViewImageColumn col = new DataGridViewImageColumn();
      if (!String.IsNullOrEmpty(columnName))
      {
        col.Name = columnName;
        col.HeaderText = String.Empty;
      }
      col.Width = ControlProvider.Measures.ImageColumnWidth;
      col.FillWeight = 1; // 08.02.2017
      col.Resizable = DataGridViewTriState.False;
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      ControlProvider.Control.Columns.Add(col);
      return col;
    }

    /// <summary>
    /// Добавляет столбец-флажок DataGridViewImageColumn.
    /// Столбец этого типа не может привязываться к полю данных.
    /// Предполагается, что устанавливается обработчик события EFPDataGridView.CellValueNeeded
    /// для получения изображения.
    /// Заголовок столбца не задается.
    /// Эта перегрузка метода не задает имя столбца. Она обычно используется, когда в просмотре
    /// есть единственный столбец значка, который имеет индекс 0.
    /// </summary>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewImageColumn AddImage()
    {
      return AddImage(null);
    }

    #endregion

    #region Комбоблок

    /// <summary>
    /// Добавляет текстовый столбец с выпадающим списком DataGridViewComboBoxColumn.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewComboBoxColumn AddCombo(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth)
    {
      if (textWidth < 1)
        throw new ArgumentOutOfRangeException("textWidth");
      if (minTextWidth < 1)
        throw new ArgumentOutOfRangeException("minTextWidth");

      if (minTextWidth > textWidth)
        throw new ArgumentOutOfRangeException("minTextWidth", minTextWidth,
          "Минимальная ширина столбца (" + minTextWidth.ToString() + ") не может быть больше, чем устанавливаемая (" + textWidth.ToString() + ")");

      DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
      col.Name = columnName;
      if (isDataColumn)
        col.DataPropertyName = columnName;
      col.HeaderText = headerText;
      col.Width = textWidth * 8 + 10; // !!!
      col.MinimumWidth = minTextWidth * 8 + 10; // !!!
      col.FillWeight = 1; // 08.02.2017
      col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      ControlProvider.Control.Columns.Add(col);

      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.CustomOrderAllowed = isDataColumn;

      return col;
    }

    /// <summary>
    /// Добавляет текстовый столбец с выпадающим списком DataGridViewComboBoxColumn,
    /// занимающий определенную часть свободного места табличного просмотра.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="FillWeight">Процент свободного места, занимаемый столбцом.
    /// Не обязательно, чтобы все столбцы занимали ровно 100%.
    /// См. описание свойства DataGridViewColumn.FillWeight при AutoSizeMode=Fill.
    /// В отличие от DataGridViewColumn.FillWeight, параметр имеет целочисленный тип.</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewComboBoxColumn AddComboFill(string columnName, bool isDataColumn, string headerText, int FillWeight, int minTextWidth)
    {
      DataGridViewComboBoxColumn col = AddCombo(columnName, isDataColumn, headerText, minTextWidth, minTextWidth);
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = FillWeight;
      return col;
    }

    /// <summary>
    /// Добавляет текстовый столбец с выпадающим списком DataGridViewComboBoxColumn,
    /// занимающий все свободное места табличного просмотра (FillWeight=100).
    /// Минимальная ширина столбца равна 5 символам.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewComboBoxColumn AddComboFill(string columnName, bool isDataColumn, string headerText)
    {
      return AddComboFill(columnName, isDataColumn, headerText, 100, 5);
    }

    /// <summary>
    /// Добавляет текстовый столбец с выпадающим списком DataGridViewComboBoxColumn,
    /// занимающий все свободное места табличного просмотра (FillWeight=100).
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Минимальная ширина столбца равна 5 символам.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewComboBoxColumn AddComboFill(string columnName)
    {
      return AddComboFill(columnName, true, columnName, 100, 5);
    }

    /// <summary>
    /// Добавляет текстовый столбец с выпадающим списком DataGridViewComboBoxColumn,
    /// занимающий определенную часть свободного места табличного просмотра.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// Минимальная ширина столбца равна 5 символам.
    /// Задает горизонтальное выравнивание по левому краю.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <param name="fillWeight">Процент свободного места, занимаемый столбцом.
    /// Не обязательно, чтобы все столбцы занимали ровно 100%.
    /// См. описание свойства DataGridViewColumn.FillWeight при AutoSizeMode=Fill.
    /// В отличие от DataGridViewColumn.FillWeight, параметр имеет целочисленный тип.</param>
    /// <returns>Объект столбца табличного просмотра (а не EFPDataGrodViewColumn)</returns>
    public DataGridViewComboBoxColumn AddComboFill(string columnName, int fillWeight)
    {
      return AddComboFill(columnName, true, columnName, fillWeight, 5);
    }

    #endregion

    #region Вспомогательные свойства

    /// <summary>
    /// Возвращает последний столбец EFPDataGridViewColumn, добавленный с помощью методов AddXXX().
    /// Значение не определено, если выполнялось добавление столбцов непосредственно
    /// в табличный просмотр DataGridView.Columns
    /// Свойство удобно использовать для установки дополнительных атрибутов столбца,
    /// которые не были установлены в методе AddXXX()
    /// </summary>
    public EFPDataGridViewColumn LastAdded
    {
      get
      {
        if (ControlProvider.Control.ColumnCount == 0)
          return null;
        else
          return this[ControlProvider.Control.Columns[ControlProvider.Control.ColumnCount - 1]];
      }
    }

    #endregion

    #endregion

    #region Очистка столбцов

    /// <summary>
    /// Удаление всех столбцов табличного просмотра
    /// Вызывает метод DataGridViewColumnCollection.Clear(), предварительно вызвав CancelEdit() и временно отключив присоединенный источник 
    /// данных.
    /// Метод предназначен, чтобы предотвратить появление ошибки NullReferenceException
    /// </summary>
    public virtual void Clear()
    {
      ControlProvider.Control.CancelEdit();
      object oldDS = ControlProvider.Control.DataSource;
      ControlProvider.Control.DataSource = null;
      try
      {
        ControlProvider.Control.CurrentCell = null;
        ControlProvider.Control.Columns.Clear();
      }
      finally
      {
        ControlProvider.Control.DataSource = oldDS;
      }
    }

    #endregion

    #region Синхронное изменение размеров столбцов

    private bool _InsideColumnWidthChanged = false;

    private bool _ColumnWidthChangedErrorWasShown = false;

    private void Grid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs args)
    {
      if (_InsideColumnWidthChanged)
        return;
      _InsideColumnWidthChanged = true;
      try
      {
        try
        {
          this[args.Column].ResizeGroup();
        }
        catch (Exception e) // 29.03.2013
        {
          if (!_ColumnWidthChangedErrorWasShown)
          {
            _ColumnWidthChangedErrorWasShown = true;
            EFPApp.ShowException(e, "Ошибка при установке размеров одинаковых столбцов");
          }
        }
      }
      finally
      {
        _InsideColumnWidthChanged = false;
      }
    }

    #endregion

    #region Методы групповой установки свойств столбцов

    /// <summary>
    /// Установить свойство DataGridViewColumn.ReadOnly для всех столбцов
    /// </summary>
    /// <param name="value">Устанавливаемое значение свойства</param>
    public void SetReadOnly(bool value)
    {
      for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
        ControlProvider.Control.Columns[i].ReadOnly = value;
    }

    #endregion

    #region Отладочные средства

    /// <summary>
    /// Массив имен всех столбцов в просмотре (отладочное средство)
    /// Свойства DataGridViewColumn.GroupName
    /// </summary>
    public string[] GridColumnNames
    {
      get
      {
        string[] a = new string[ControlProvider.Control.Columns.Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = ControlProvider.Control.Columns[i].Name;
        return a;
      }
    }

    /// <summary>
    /// Массив имен всех столбцов в просмотре (отладочное средство)
    /// Свойства EFPDataGridViewColumn.Name
    /// </summary>
    public string[] ControlProviderColumnNames
    {
      get
      {
        string[] a = new string[Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = this[i].Name;
        return a;
      }
    }
    #endregion

    #region IEnumerable<EFPDataGridViewColumn> Members

    /// <summary>
    /// Перечислитель по столбцам EFPDataGridViewColumn.
    /// Порядок перечисления соответствует DataGridViewColumn.Index, а не порядку отображения столбцов в просмотре.
    /// Скрытые столбцы также перечисляются.
    /// </summary>
    public struct Enumerator : IEnumerator<EFPDataGridViewColumn>, IEnumerator<IEFPDataViewColumn>
    {
      #region Конструктор

      internal Enumerator(EFPDataGridViewColumns columns)
      {
        _Columns = columns;
        _Index = -1;
      }

      #endregion

      #region Поля

      private EFPDataGridViewColumns _Columns;
      private int _Index;

      #endregion

      #region IEnumerator<EFPDataGridViewColumn> Members

      /// <summary>
      /// Возвращает очередной столбец
      /// </summary>
      public EFPDataGridViewColumn Current { get { return _Columns[_Index]; } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      IEFPDataViewColumn IEnumerator<IEFPDataViewColumn>.Current { get { return Current; } }


      object System.Collections.IEnumerator.Current { get { return _Columns[_Index]; } }

      /// <summary>
      /// Переход к следующему столбцу
      /// </summary>
      /// <returns>true, если есть следующий столбец</returns>
      public bool MoveNext()
      {
        _Index++;
        return _Index < _Columns.Count;
      }

      void System.Collections.IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по столбцам
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<EFPDataGridViewColumn> IEnumerable<EFPDataGridViewColumn>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<IEFPDataViewColumn> IEnumerable<IEFPDataViewColumn>.GetEnumerator()
    {
      return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }
}
