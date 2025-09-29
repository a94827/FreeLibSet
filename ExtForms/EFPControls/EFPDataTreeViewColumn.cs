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
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using System.Collections;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.Forms.Reporting;
using FreeLibSet.Models.Tree;
using FreeLibSet.UICore;

/*
 * Дополнительные описания для стоблцов TreeViewAdv
 * TreeViewAdv определяет объекты TreeColumn, у которых есть заголовок, признак видимости, сортировка.
 * Также есть объекты NodeControl, которые выводят значения
 */

namespace FreeLibSet.Forms
{

#if XXX
  #region GridProducerColumnCellToolTipTextNeededEventHandler

  public class EFPDataTreeViewCellToolTipTextNeededEventArgs : EventArgs
  {
  #region Конструктор

    public EFPDataTreeViewCellToolTipTextNeededEventArgs(EFPDataTreeViewColumn Column, int RowIndex)
    {
      FColumn = Column;
      FRowIndex = RowIndex;
      ToolTipText = String.Empty;
    }

  #endregion

  #region Свойства

    public EFPDataTreeViewColumn Column { get { return FColumn; } }
    private EFPDataTreeViewColumn FColumn;

    public EFPDataTreeView ControlProvider { get { return FColumn.ControlProvider; } }

    public int RowIndex { get { return FRowIndex; } }
    private int FRowIndex;

    //public DataRow Row { get { return ControlProvider.GetDataRow(FRowIndex); } }

    public string ColumnName { get { return FColumn.Name; } }

    /// <summary>
    /// Сюда должен быть помещен текст подсказки
    /// </summary>
    public string ToolTipText;

  #endregion
  }

  public delegate void EFPDataTreeViewCellToolTipTextNeededEventHandler(object Sender,
    EFPDataTreeViewCellToolTipTextNeededEventArgs Args);

  #endregion

#endif

  /// <summary>
  /// Объект расширенного описания столбца в <see cref="EFPDataTreeView"/>.
  /// Столбцы не используются, если <see cref="TreeViewAdv.UseColumns"/>=false.
  /// </summary>
  public class EFPDataTreeViewColumn : IEFPDataViewColumn
  {
    #region Защищенный конструктор

    internal EFPDataTreeViewColumn(EFPDataTreeView controlProvider, TreeColumn treeColumn, BindableControl nodeControl, string name)
    {
      _ControlProvider = controlProvider;
      _TreeColumn = treeColumn;
      _NodeControl = nodeControl;
      _Name = name;

      _CanIncSearch = false;

      _ColorType = UIDataViewColorType.Normal;
      _LeftBorder = UIDataViewBorderStyle.Default;
      _RightBorder = UIDataViewBorderStyle.Default;

      if (nodeControl is InteractiveControl)
        _DbfPreliminaryInfo = new DbfFieldTypePreliminaryInfo();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
    private readonly EFPDataTreeView _ControlProvider;

    IEFPDataView IEFPDataViewColumn.ControlProvider { get { return ControlProvider; } }

    /// <summary>
    /// Столбец древовидного просмотра <see cref="TreeViewAdv"/>
    /// </summary>
    public TreeColumn TreeColumn { get { return _TreeColumn; } }
    private readonly TreeColumn _TreeColumn;

    /// <summary>
    /// Объект работы с ячейками столбца
    /// </summary>
    public BindableControl NodeControl { get { return _NodeControl; } }
    private readonly BindableControl _NodeControl;

    /// <summary>
    /// Индекс столбца в <see cref="EFPDataTreeViewColumns"/> 
    /// </summary>
    public int Index { get { return _ControlProvider.Columns.IndexOf(_Name); } } // TODO: Надо переделать

    /// <summary>
    /// Имя столбца. Всегда определено
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Пользовательские данные 
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Если столбец был создан с помощью <see cref="EFPGridProducer"/>, то ссылка на генератор столбца,
    /// иначе - null.
    /// </summary>
    public IEFPGridProducerColumn ColumnProducer { get { return _ColumnProducer; } set { _ColumnProducer = value; } }
    private IEFPGridProducerColumn _ColumnProducer;

    bool IEFPDataViewColumn.Visible { get { return true; } }

    /// <summary>
    /// Имя столбца, отображаемое в диалоге параметров страницы.
    /// Если не задано в явном виде, возвращает свойство <see cref="FreeLibSet.Controls.TreeColumn.Header"/> или <see cref="Name"/>
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
        {
          if (String.IsNullOrEmpty(TreeColumn.Header))
            return Name;
          else
            return TreeColumn.Header.Replace(Environment.NewLine, " ");
        }
        else
          return _DisplayName;
      }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

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

#if XXX
    /// <summary>
    /// Реализация поиска по буквам для столбца
    /// </summary>
    /// <param name="SearchStr">Строка, которую надо найти</param>
    /// <returns>true, если поиск успешно выполнен</returns>
    internal bool PerformIncSearch(string SearchStr, bool NextSearch)
    {
      // Для пустой строки поиска позиционируемся на первую строку
      if (String.IsNullOrEmpty(SearchStr))
      {
        if (NextSearch)
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

      DataView OrgView = ControlProvider.SourceAsDataView;
      if (ControlProvider.IncSearchDataView == null)
      {
        ControlProvider.IncSearchDataViewIsManual = false;
        if (OrgView == null)
          ControlProvider.IncSearchDataViewIsManual = true;
        else
        {
          if (String.IsNullOrEmpty(GridColumn.DataPropertyName))
            ControlProvider.IncSearchDataViewIsManual = true; // динамически вычисляемый столбец
          else
          {
            if (!OrgView.Table.Columns.Contains(Name))
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
            DataTable TempTable = new DataTable();
            TempTable.Columns.Add(Name, typeof(string));
            for (int i = 0; i < ControlProvider.Control.RowCount; i++)
            {
              ControlProvider.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
              EFPDataGridViewCellAttributesEventArgs CellArgs = ControlProvider.DoGetCellAttributes(GridColumn.Index);
              object v = CellArgs.Value;
              if (v == null || v is DBNull)
                TempTable.Rows.Add(String.Empty);
              else
                TempTable.Rows.Add(v.ToString());
            }
            ControlProvider.IncSearchDataView = TempTable.DefaultView;
          }
          finally
          {
            EFPApp.EndWait();
          }
        }
        else
        {
          ControlProvider.IncSearchDataView = new DataView(OrgView.Table);
          ControlProvider.IncSearchDataView.Sort = OrgView.Sort;
        }

        NextSearch = false; // некуда продолжать
      }

      DataRow FoundRow; // Строка в EFPDataGridView.IncSearchDataView, на которую надо позиционироваться
      if (NextSearch)
      {
        if (ControlProvider.IncSearchDataView.Count < 2)
          return false; // некуда переходить

        DataRow CurrRow;
        if (ControlProvider.IncSearchDataViewIsManual)
          CurrRow = ControlProvider.IncSearchDataView.Table.Rows[ControlProvider.CurrentRowIndex];
        else
          CurrRow = ControlProvider.CurrentDataRow;
        int CurrIdx = DataTools.FindDataRowViewIndex(ControlProvider.IncSearchDataView, CurrRow);
        if (CurrIdx < 0)
          // Переместились на какую-то строку, которая не соответствует условию
          // Возвращаемся на первую подходящую (как если начать поиск)
          FoundRow = ControlProvider.IncSearchDataView[0].Row;
        else
        {
          if (CurrIdx == ControlProvider.IncSearchDataView.Count - 1)
            // Были на последней строке - переходим к первой
            FoundRow = ControlProvider.IncSearchDataView[0].Row;
          else
            FoundRow = ControlProvider.IncSearchDataView[CurrIdx + 1].Row;
        }
      }
      else //!NextSearch
      {
        // Устанавливаем фильтр
        // Исходный фильтр:
        string OrgViewRowFilter = ControlProvider.IncSearchDataViewIsManual ? String.Empty : OrgView.RowFilter;
        string PrevSearchFilter = ControlProvider.IncSearchDataView.RowFilter; // есои не найдем - вернем обратно
        string s1;
        if (ControlProvider.IncSearchDataView.Table.Columns[Name].DataType == typeof(string))
          s1 = "[" + Name + "] " +  DataTools.GetDataViewLikeExpressionString(SearchStr);
        else
          s1 = "CONVERT([" + Name + "], 'System.String') " + DataTools.GetDataViewLikeExpressionString(SearchStr);
        if (String.IsNullOrEmpty(OrgViewRowFilter))
          ControlProvider.IncSearchDataView.RowFilter = s1;
        else
          ControlProvider.IncSearchDataView.RowFilter = "(" + OrgViewRowFilter + ") AND " + s1;

        // Позиционируемся на первую строку в выбранном подмножестве
        if (ControlProvider.IncSearchDataView.Count == 0)
        {
          ControlProvider.IncSearchDataView.RowFilter = PrevSearchFilter;
          return false;
        }
        else
          FoundRow = ControlProvider.IncSearchDataView[0].Row;
      }

      // Выполняем позиционирование
      if (ControlProvider.IncSearchDataViewIsManual)
      {
        int p = ControlProvider.IncSearchDataView.Table.Rows.IndexOf(FoundRow);
        ControlProvider.CurrentRowIndex = p;
      }
      else
        ControlProvider.CurrentDataRow = FoundRow;

      return true;
    }
#endif

    #endregion

    #region Ширина столбца

    /// <summary>
    /// Ширина столбца в пикселях (дублирование <see cref="FreeLibSet.Controls.TreeColumn.Width"/>)
    /// </summary>
    public int Width
    {
      get { return TreeColumn.Width; }
      set { TreeColumn.Width = value; }
    }

    /// <summary>
    /// Ширина столбца в пунктах, в зависимости от разрешения экрана
    /// </summary>
    public int WidthPt
    {
      get { return (int)(Math.Round(TreeColumn.Width / ControlProvider.Measures.DpiX * 72)); }
      set { TreeColumn.Width = (int)Math.Round(value * ControlProvider.Measures.DpiX / 72); }
    }

    /// <summary>
    /// Ширина столбца в текстовых единицах (условная)
    /// </summary>
    public double TextWidth
    {
      get { return ControlProvider.Measures.GetColumnWidthChars(TreeColumn.Width); }
      set { TreeColumn.Width = ControlProvider.Measures.GetTextColumnWidth(value); }
    }

    /// <summary>
    /// Минимальная ширина столбца в текстовых единицах (условная)
    /// </summary>
    public double MinTextWidth
    {
      get { return ControlProvider.Measures.GetColumnWidthChars(TreeColumn.MinColumnWidth); }
      set { TreeColumn.MinColumnWidth = ControlProvider.Measures.GetTextColumnWidth(value); }
    }

    /// <summary>
    /// Относительная ширина столбца.
    /// В отличие от <see cref="DataGridView"/>, элемент <see cref="TreeViewAdv"/> не поддерживает автоматический подбор размеров по ширине.
    /// Это свойство реализует недостающую возможность.
    /// Нулевое значение (по умолчанию) означает отсутствие автоматического расширения.
    /// </summary>
    public int FillWeight
    {
      get { return _FillWeight; }
      set
      {
        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);
        _FillWeight = value;
      }
    }
    private int _FillWeight;

    bool IEFPDataViewColumnBase.AutoGrow { get { return _FillWeight!=0; } } 

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
        EFPDataTreeViewColumn thisCol = ControlProvider.Columns[i];
        if (thisCol == this)
          continue;
        if (thisCol.SizeGroup == SizeGroup)
        {
          //if (thisCol.GridColumn.InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.None)
          //  continue; // 29.03.2013

          thisCol.TreeColumn.Width = TreeColumn.Width;
        }
      }
    }

    #endregion

    #region Подсказка для ячейки

#if XXX
    /// <summary>
    /// Событие вызывается при наведении курсора на ячейку, относящуюся к столбцу
    /// Обработчик может установить свойство Args.ToolTipText.
    /// К подсказке могут быть добавлены дополнительные строки, если это указано
    /// в текущей конфигурации просмотра
    /// </summary>
    public event EFPDataGridViewCellToolTipTextNeededEventHandler CellToopTextNeeded;

    /// <summary>
    /// Получить текст подсказки для ячейки
    /// Вызывается событие CellToopTextNeeded. Если столбец создан с помощью
    /// GridProducer, то добавляется текст, созданный продюсером
    /// </summary>
    /// <param name="RowIndex">Номер строки</param>
    /// <returns>Текст подсказки</returns>
    internal string GetCellToolTipText(int RowIndex)
    {
      if (RowIndex < 0)
        return String.Empty;
      List<string> a = new List<string>(); // Собираем подсказку из нескольких строк
      if (CellToopTextNeeded != null)
      {
        EFPDataGridViewCellToolTipTextNeededEventArgs Args = new EFPDataGridViewCellToolTipTextNeededEventArgs(this, RowIndex);
        CellToopTextNeeded(this, Args);
        if (Args.ToolTipText != null)
          a.Add(Args.ToolTipText);
      }

      ControlProvider.AddGridProducerToolTip(this, a, RowIndex);
      // Преобразуем в строку
      return String.Join(Environment.NewLine, a.ToArray());
    }
#endif

    #endregion

    #region Раскраска столбцов

    // TODO: Не используется

    /// <summary>
    /// Цвет столбца. Используется, если для текущей строки в событии 
    /// EFPDataTreeView.GetRowAttributres не определен более приоритетный цвет
    /// На момент вызова обработчика EFPDataGridView.GetCellAttributres, если он задан,
    /// цвет уже применен
    /// </summary>
    public UIDataViewColorType ColorType { get { return _ColorType; } set { _ColorType = value; } }
    private UIDataViewColorType _ColorType;

    /// <summary>
    /// Окрашивание всех значений в столбце "серым" цветом
    /// </summary>
    public bool Grayed { get { return _Grayed; } set { _Grayed = value; } }
    private bool _Grayed;

    /// <summary>
    /// Рамка для левой границы столбца
    /// На момент вызова обработчика EFPDataGridView.GetCellAttributres, если он задан,
    /// стиль рамки уже применен
    /// </summary>
    public UIDataViewBorderStyle LeftBorder { get { return _LeftBorder; } set { _LeftBorder = value; } }
    private UIDataViewBorderStyle _LeftBorder;

    /// <summary>
    /// Рамка для правой границы столбца
    /// На момент вызова обработчика EFPDataGridView.GetCellAttributres, если он задан,
    /// стиль рамки уже применен
    /// </summary>
    public UIDataViewBorderStyle RightBorder { get { return _RightBorder; } set { _RightBorder = value; } }
    private UIDataViewBorderStyle _RightBorder;

    #endregion

    #region Печать

    /// <summary>
    /// Возвращает true для всех столбцов, кроме изображений
    /// </summary>
    public bool Printable
    {
      get
      {
        if (NodeControl is NodeIcon)
          return false;
        else
          return true;
      }
    }

    /// <summary>
    /// Многострочные заголовки при печати таблицы.
    /// Если свойство не установлено (свойство возвращает null), то используется <see cref="FreeLibSet.Controls.TreeColumn.Header"/>.
    /// </summary>
    public string[] PrintHeaders { get { return _PrintHeaders; } set { _PrintHeaders = value; } }
    private string[] _PrintHeaders;

    /// <summary>
    /// Многострочные заголовки при печати таблицы (свойство <see cref="PrintHeaders"/>)
    /// Версия для установки в виде одной строки с заменой символов:
    /// "|" - разделитель многострочного заголовка
    /// "^" - мягкий перенос
    /// "_" - неразрывный пробел
    /// </summary>
    public string PrintHeadersSpec
    {
      get { return StringTools.StrFromSpecCharsArray(PrintHeaders); }
      set { PrintHeaders = StringTools.StrToSpecCharsArray(value); }
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
    /// По умолчанию свойство возвращает true для первого столбца в списке.
    /// </summary>
    public bool PrintAutoGrow
    {
      get { return GetPrintAutoGrow(String.Empty); }
      set { SetPrintAutoGrow(String.Empty, value); }
    }

    private BRDataViewMenuOutSettings GetSettings(string defCfgCode)
    {
      BRDataTreeViewMenuOutItem outItem = ControlProvider.DefaultOutItem;
      if (outItem == null)
        throw new InvalidOperationException(Res.EFPDataView_Err_DefaultOutItemDeleted);
      return outItem[defCfgCode];
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Горизонтальное выравнивание ячейки
    /// Осуществляет доступ к свойству <see cref="BaseTextControl.TextAlign"/>.
    /// </summary>
    public HorizontalAlignment TextAlign
    {
      get
      {
        BaseTextControl tc = NodeControl as BaseTextControl;
        if (tc == null)
          return HorizontalAlignment.Center;
        else
          return tc.TextAlign;
      }
      set
      {
        BaseTextControl tc = NodeControl as BaseTextControl;
        if (tc != null)
          tc.TextAlign = value;
        else
          throw new InvalidOperationException();
      }
    }

    /// <summary>
    /// Возвращает свойство <see cref="Name"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    /// Получение значения для столбца, созданного с помощью <see cref="EFPGridProducerColumn"/>
    /// </summary>
    /// <param name="sender">Ссылка на <see cref="NodeControl"/></param>
    /// <param name="args">Аргументы события</param>
    internal void NodeControl_ValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      EFPDataViewRowValues rowValues = ControlProvider.GetRowValues(args.Node);
      args.Value = ((EFPGridProducerColumn)ColumnProducer).GetValue(rowValues);
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
    /// Предварительная информация о типе данных, используемая при экспорте столбца в DBF-формат.
    /// В отличие от <see cref="DbfInfo"/>, это свойство не может устанавливаться в прикладном коде.
    /// Если столбец не может быть экспортирован, свойство возвращает null.
    /// </summary>
    public DbfFieldTypePreliminaryInfo DbfPreliminaryInfo { get { return _DbfPreliminaryInfo; } }
    private DbfFieldTypePreliminaryInfo _DbfPreliminaryInfo;

    #endregion

    #region Перебор значений

    private class ValueEnumerable : System.Collections.IEnumerable
    {
      #region Конструктор

      internal ValueEnumerable(EFPDataTreeViewColumn column)
      {
        _Column = column;
        if (column.ControlProvider.Control.Model != null)
          _ModelEnumerable = new TreePathEnumerable(column.ControlProvider.Control.Model);
      }

      private EFPDataTreeViewColumn _Column;
      private IEnumerable<TreePath> _ModelEnumerable;

      #endregion

      #region IEnumerable

      public IEnumerator GetEnumerator()
      {
        IEnumerator<TreePath> modelEnumerator = null;
        if (_ModelEnumerable != null)
          modelEnumerator = _ModelEnumerable.GetEnumerator();
        return new ValueEnumerator(_Column, modelEnumerator);
      }

      #endregion
    }


    private class ValueEnumerator : IEnumerator
    {
      #region Конструктор

      internal ValueEnumerator(EFPDataTreeViewColumn column, IEnumerator<TreePath> modelEnumerator)
      {
        this._Column = column;
        _ModelEnumerator = modelEnumerator;
      }

      private EFPDataTreeViewColumn _Column;
      private IEnumerator<TreePath> _ModelEnumerator;

      #endregion

      #region IEnumerator

      public object Current
      {
        get
        {
          TreeNodeAdv node = _Column.ControlProvider.Control.FindNode(_ModelEnumerator.Current, true);
          if (node == null)
            throw new BugException("Tree node not found");

          InteractiveControl nc = _Column.NodeControl as InteractiveControl;
          if (nc == null)
            return null;
          else
            return nc.GetValue(node);
        }
      }

      public bool MoveNext()
      {
        if (_ModelEnumerator != null)
          return _ModelEnumerator.MoveNext();
        else
          return false;
      }

      void IEnumerator.Reset()
      {
        if (_ModelEnumerator != null)
          _ModelEnumerator.Reset();
      }

      #endregion
    }

    IEnumerable IEFPDataViewColumn.ValueEnumerable
    {
      get { return new ValueEnumerable(this); }
    }

    #endregion
  }

  /// <summary>
  /// Псевдоколлекция столбцов - реализация свойства <see cref="EFPDataTreeView.Columns"/>.
  /// Используется, если <see cref="TreeViewAdv.UseColumns"/>=true, иначе список пустой.
  /// </summary>
  public class EFPDataTreeViewColumns : IEnumerable<EFPDataTreeViewColumn>, IEFPDataViewColumns
  {
    #region Защищенный конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    /// <param name="controlProvider">Владелец</param>
    internal protected EFPDataTreeViewColumns(EFPDataTreeView controlProvider)
    {
      _ControlProvider = controlProvider;
      _ColumnDict = new Dictionary<TreeColumn, EFPDataTreeViewColumn>();
      //_NameDict = new Dictionary<string, EFPDataTreeViewColumn>();
      controlProvider.Control.ColumnWidthChanged += new EventHandler<TreeColumnEventArgs>(Control_ColumnWidthChanged);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
    private EFPDataTreeView _ControlProvider;

    #endregion

    #region Свойства и методы доступа к элементам коллекции

    /// <summary>
    /// Возвращает количество столбцов в просмотре
    /// </summary>
    public int Count { get { return ControlProvider.Control.Columns.Count; } }

    /// <summary>
    /// Нет возможности хранить в TreeColumn ссылку на EFPDataTreeViewColumn.
    /// Используем словарь, который надо периодически проверять на актуальность
    /// </summary>
    private Dictionary<TreeColumn, EFPDataTreeViewColumn> _ColumnDict;

    //private Dictionary<string, EFPDataTreeViewColumn> _NameDict;

    private void UpdateColumnDict()
    {
      if (ControlProvider.Control.IsDisposed)
        throw new ObjectDisposedException("TreeViewAdv");

      if (!ControlProvider.Control.UseColumns)
      {
        _ColumnDict.Clear();
        return;
      }

      #region Удаляем столбцы, которых больше нет в TreeViewAdv

      List<TreeColumn> lstDel = null;
      foreach (KeyValuePair<TreeColumn, EFPDataTreeViewColumn> pair in _ColumnDict)
      {
        if (pair.Key.Owner == null)
        {
          if (lstDel == null)
            lstDel = new List<TreeColumn>();
          lstDel.Add(pair.Key);
        }
      }

      if (lstDel != null)
      {
        foreach (TreeColumn treeCol in lstDel)
          _ColumnDict.Remove(treeCol);
      }

      #endregion

      #region Добавляем недостающие столбцы

      SingleScopeStringList names = new SingleScopeStringList(false);
      foreach (KeyValuePair<TreeColumn, EFPDataTreeViewColumn> pair in _ColumnDict)
        names.Add(pair.Value.Name);

      foreach (TreeColumn treeCol in ControlProvider.Control.Columns)
      {
        if (!_ColumnDict.ContainsKey(treeCol))
        {
          string colName = "Noname_" + (treeCol.Index + 1).ToString();
          int cnt = 1;
          while (names.Contains(colName))
          {
            cnt++;
            colName = "Noname_" + (treeCol.Index + 1).ToString() + "_" + cnt.ToString();
          }

          BindableControl mainNC = null;
          foreach (NodeControl nc in ControlProvider.Control.NodeControls)
          {
            BindableControl bc = nc as BindableControl;
            if (bc == null)
              continue;

            if (nc.ParentColumn != treeCol)
              continue;

            if (bc is InteractiveControl)
            {
              mainNC = bc;
              break;
            }
            if (mainNC == null)
              mainNC = bc;
          }
          EFPDataTreeViewColumn efpCol = new EFPDataTreeViewColumn(ControlProvider, treeCol, mainNC, colName);
          _ColumnDict.Add(treeCol, efpCol);
        }
      }

      #endregion
    }


    /// <summary>
    /// Доступ к столбцу, соответствующему объекту <see cref="TreeColumn"/>
    /// </summary>
    /// <param name="treeColumn">Столбец в элементе<see cref="TreeViewAdv"/></param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[TreeColumn treeColumn]
    {
      get
      {
#if DEBUG
        if (treeColumn == null)
          throw new ArgumentNullException("treeColumn");
        //if (GridColumn.DataGridView != null)
        //{
        //  if (GridColumn.DataGridView != GridHandler.Control)
        //    throw new ArgumentException("Столбец относится к другому табличному просмотру", "GridColumn");
        //}
#endif

        EFPDataTreeViewColumn res;
        if (_ColumnDict.TryGetValue(treeColumn, out res))
          return res;

        if (treeColumn.Owner == null)
          throw ExceptionFactory.ArgProperty("treeColumn", treeColumn, "Owner", treeColumn.Owner, null);

        if (!Object.ReferenceEquals(treeColumn.Owner.TreeView, ControlProvider.Control))
          throw ExceptionFactory.ObjectProperty(treeColumn.Owner, "TreeView", treeColumn.Owner.TreeView, new object[] { ControlProvider.Control });

        UpdateColumnDict();
        return _ColumnDict[treeColumn];
      }
    }

    /// <summary>
    /// Доступ к столбцу по индексу <see cref="TreeColumn.Index"/>.
    /// </summary>
    /// <param name="index">Индекс столбца табличного просмотра</param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[int index]
    {
      get
      {
        if (index < 0 || index >= ControlProvider.Columns.Count)
          throw ExceptionFactory.ArgOutOfRange("index", index, 0, ControlProvider.Columns.Count-1);
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
    /// <param name="name">Имя столбца</param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[string name]
    {
      get
      {
        // Может быть, нужна оптимизация

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
    /// Доступ к столбцу, созданному с помощью <see cref="EFPGridProducer"/>.
    /// Если нет такого столбца, возвращается null.
    /// </summary>
    /// <param name="columnProducer">Генератор столбца</param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[IEFPGridProducerColumn columnProducer]
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
    /// Возвращает индекс столбца по именм
    /// </summary>
    /// <param name="name">Имя столбца</param>
    /// <returns>Индекс столбца</returns>
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

    #region IEnumerable<EFPDataTreeViewColumn>

    /// <summary>
    /// Перечислитель для столбцов
    /// </summary>
    public struct Enumerator : IEnumerator<EFPDataTreeViewColumn>, IEnumerator<IEFPDataViewColumn>
    {
      #region Защищенный конструктор

      internal Enumerator(EFPDataTreeViewColumns columns)
      {
        _Columns = columns;
        _CurrentIndex = -1;
      }

      #endregion

      #region Поля

      private readonly EFPDataTreeViewColumns _Columns;
      private int _CurrentIndex;

      /// <summary>
      /// Возвращает текущий объект столбца в перечислении
      /// </summary>
      public EFPDataTreeViewColumn Current { get { return _Columns[_CurrentIndex]; } }

      IEFPDataViewColumn IEnumerator<IEFPDataViewColumn>.Current { get { return Current; } }

      object IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// Переход к следующему перечисляемому столбцу
      /// </summary>
      /// <returns>True, если есть столбцы</returns>
      public bool MoveNext()
      {
        _CurrentIndex++;
        return _CurrentIndex < _Columns.Count;
      }

      void IEnumerator.Reset()
      {
        _CurrentIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Создает перечислитель по всем столбцам просмотра, включая скрытые
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<EFPDataTreeViewColumn> IEnumerable<EFPDataTreeViewColumn>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator<IEFPDataViewColumn> IEnumerable<IEFPDataViewColumn>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Методы добавления столбцов

    #region Обобщенный метод AddControl()

    /// <summary>
    /// После созданный столбец при вызове метода AddControl()
    /// </summary>
    private EFPDataTreeViewColumn _LastAddedColumn;

    /// <summary>
    /// Добавляет <paramref name="nodeControl"/> в коллекцию элементов дерева <see cref="TreeViewAdv.NodeControls"/>.
    /// Создает столбец <see cref="TreeColumn"/>, инициализирует его свойства, и присоединяет к нему <paramref name="nodeControl"/>, устанавливая свойство <see cref="NodeControl.ParentColumn"/>.
    /// Если <see cref="TreeViewAdv.UseColumns"/>=false, то выполняется только добавление <paramref name="nodeControl"/>, а столбец не создается.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="nodeControl">Созданный элемент <see cref="NodeControl"/>, например <see cref="NodeTextBox"/>. Элемент не должен быть присоединен к дереву</param>
    /// <param name="columnName">Имя столбца, которое может быть использовано для поиска столбца</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    public void AddControl(BindableControl nodeControl, string columnName, string headerText, int textWidth, int minTextWidth)
    {
      if (nodeControl == null)
        throw new ArgumentNullException("nodeControl");
      if (nodeControl.Parent != null)
        throw ExceptionFactory.ObjectPropertyAlreadySet(nodeControl, "Parent");

      _LastAddedColumn = null;

      //if (!ControlProvider.Control.UseColumns)
      //  throw new InvalidOperationException("Нельзя добавлять столбцы, когда UseColumns=false");

      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
      if (textWidth < 1)
        throw ExceptionFactory.ArgOutOfRange("textWidth", textWidth, 1, null);
      if (minTextWidth < 1 || minTextWidth > textWidth)
        throw ExceptionFactory.ArgOutOfRange("minTextWidth", minTextWidth, 1, textWidth);

      TreeColumn treeCol = null;
      if (ControlProvider.Control.UseColumns)
      {
        int w1 = ControlProvider.Measures.GetTextColumnWidth(textWidth);
        int w2 = ControlProvider.Measures.GetTextColumnWidth(minTextWidth);
        treeCol = new TreeColumn(headerText, w1);
        treeCol.MinColumnWidth = w2;
        ControlProvider.Control.Columns.Add(treeCol);
      }

      if (ControlProvider.Control.UseColumns)
        nodeControl.ParentColumn = treeCol;

      ControlProvider.Control.NodeControls.Add(nodeControl);

      if (ControlProvider.Control.UseColumns)
      {
        EFPDataTreeViewColumn efpCol = new EFPDataTreeViewColumn(ControlProvider, treeCol, nodeControl, columnName);
        _ColumnDict.Add(treeCol, efpCol);
        _LastAddedColumn = efpCol;
      }
    }

    #endregion

    #region Text

    /// <summary>
    /// Создает элемент <see cref="NodeTextBox"/> для отображения текста и добавляет его в коллекцию. Если <see cref="TreeViewAdv.UseColumns"/>=true, то также создается столбец <see cref="TreeColumn"/>.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца, которое можно использовать для поиска объекта <see cref="EFPDataTreeViewColumn"/>. 
    /// Если <paramref name="isDataColumn"/>=true, то также задает свойство данных <see cref="BindableControl.DataPropertyName"/>.</param>
    /// <param name="isDataColumn">Если true, то элемент будет автоматически извлекать данные из объектов модели. 
    /// При этом <paramref name="columnName"/> должно указывать на имя свойства или поля объектов узлов модели, или на имя поля, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// Если false, то извлечение данных должно выполняться прикладным кодом</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    /// <returns>Созданный элемент</returns>
    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth)
    {
      NodeTextBox nodeControl = new NodeTextBox();
      //tb.EditEnabled = !ReadOnly;

      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, minTextWidth);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.DbfPreliminaryInfo.Type = 'C';
        _LastAddedColumn.DbfPreliminaryInfo.Length = textWidth;
      }

      return nodeControl;
    }


    /// <summary>
    /// Создает элемент <see cref="NodeTextBox"/> для отображения текста и добавляет его в коллекцию. Если <see cref="TreeViewAdv.UseColumns"/>=true, то также создается столбец <see cref="TreeColumn"/>.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца, которое можно использовать для поиска объекта <see cref="EFPDataTreeViewColumn"/>. 
    /// Если <paramref name="isDataColumn"/>=true, то также задает свойство данных <see cref="BindableControl.DataPropertyName"/>.</param>
    /// <param name="isDataColumn">Если true, то элемент будет автоматически извлекать данные из объектов модели. 
    /// При этом <paramref name="columnName"/> должно указывать на имя свойства или поля объектов узлов модели, или на имя поля, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// Если false, то извлечение данных должно выполняться прикладным кодом</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="columnFormat">Формат столбца</param>
    /// <returns>Созданный элемент</returns>
    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText, UITextColumnFormat columnFormat)
    {
      NodeTextBox nodeControl = new NodeTextBox();
      //tb.EditEnabled = !ReadOnly;

      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, columnFormat.TextWidth, columnFormat.MinTextWidth);
      nodeControl.TextAlign = (HorizontalAlignment)(int)(columnFormat.TextAlign);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.SizeGroup = columnFormat.SizeGroup;
        columnFormat.InitDbfPreliminaryInfo(_LastAddedColumn.DbfPreliminaryInfo);
      }

      return nodeControl;
    }

    /// <summary>
    /// Создает элемент <see cref="NodeTextBox"/> для отображения текста, занимающий определенную часть свободного места просмотра.
    /// Задает горизонтальное выравнивание по левому краю.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца, которое можно использовать для поиска объекта <see cref="EFPDataTreeViewColumn"/>. 
    /// Если <paramref name="isDataColumn"/>=true, то также задает свойство данных <see cref="BindableControl.DataPropertyName"/>.</param>
    /// <param name="isDataColumn">Если true, то элемент будет автоматически извлекать данные из объектов модели. 
    /// При этом <paramref name="columnName"/> должно указывать на имя свойства или поля объектов узлов модели, или на имя поля, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// Если false, то извлечение данных должно выполняться прикладным кодом</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="fillWeight">Процент свободного места, занимаемый столбцом.
    /// Не обязательно, чтобы все столбцы занимали ровно 100%.
    /// См. описание свойства <see cref="EFPDataTreeViewColumn.FillWeight"/>.</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <returns>Созданный элемент</returns>
    public NodeTextBox AddTextFill(string columnName, bool isDataColumn, string headerText, int fillWeight, int minTextWidth)
    {
      NodeTextBox nodeControl = AddText(columnName, isDataColumn, headerText, minTextWidth, minTextWidth);
      if (_LastAddedColumn != null)
        _LastAddedColumn.FillWeight = fillWeight;
      return nodeControl;
    }

    #endregion

    #region Дата и время

    /// <summary>
    /// Добавляет столбец <see cref="NodeDateTimeBox"/> для отображения даты без времени.
    /// Используется формат отображения <see cref="EditableDateTimeFormatterKind.Date"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDate(string columnName, bool isDataColumn, string headerText)
    {
      return AddDateTime(columnName, isDataColumn, headerText, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавляет столбец <see cref="NodeDateTimeBox"/> для отображения даты и времени.
    /// Используется формат отображения <see cref="EditableDateTimeFormatterKind.DateTime"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDateTime(string columnName, bool isDataColumn, string headerText)
    {
      return AddDateTime(columnName, isDataColumn, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавляет столбец <see cref="NodeDateTimeBox"/> для отображения даты и/или времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="kind">Тип даты и/или времени</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDateTime(string columnName, bool isDataColumn, string headerText, EditableDateTimeFormatterKind kind)
    {
      NodeDateTimeBox nodeControl = new NodeDateTimeBox();
      nodeControl.Formatter = EditableDateTimeFormatters.Get(kind);
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, nodeControl.Formatter.TextWidth, nodeControl.Formatter.TextWidth);

      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.SizeGroup = kind.ToString();
        if (kind == EditableDateTimeFormatterKind.Date)
          _LastAddedColumn.DbfPreliminaryInfo.Type = 'D';
        else
        {
          _LastAddedColumn.DbfPreliminaryInfo.Type = 'C';
          _LastAddedColumn.DbfPreliminaryInfo.Length = nodeControl.Formatter.TextWidth;
        }
      }

      return nodeControl;
    }

    #endregion

    #region Числа

    /// <summary>
    /// Добавляет столбец <see cref="NodeDecimalEditBox"/> для просмотра числовых значений
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах (включая десятичную точку)</param>
    /// <param name="decimalPlaces">Количество знаков после запятой</param>
    /// <param name="sizeGroup">Имя группы для синхронного изменения размеров столбцов</param>
    /// <returns>Созданный объект</returns>
    public NodeDecimalEditBox AddDecimal(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      NodeDecimalEditBox nodeControl = new NodeDecimalEditBox();
      nodeControl.DecimalPlaces = decimalPlaces;
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.SizeGroup = sizeGroup;
        _LastAddedColumn.DbfPreliminaryInfo.Type = 'N';
        _LastAddedColumn.DbfPreliminaryInfo.Length = textWidth;
        _LastAddedColumn.DbfPreliminaryInfo.Precision = decimalPlaces;
        _LastAddedColumn.DbfPreliminaryInfo.PrecisionIsDefined = true;
      }
      return nodeControl;
    }

    /// <summary>
    /// Добавляет столбец <see cref="NodeDoubleEditBox"/> для просмотра числовых значений
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах (включая десятичную точку)</param>
    /// <param name="decimalPlaces">Количество знаков после запятой</param>
    /// <param name="sizeGroup">Имя группы для синхронного изменения размеров столбцов</param>
    /// <returns>Созданный объект</returns>
    public NodeDoubleEditBox AddDouble(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      NodeDoubleEditBox nodeControl = new NodeDoubleEditBox();
      nodeControl.DecimalPlaces = decimalPlaces;
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.SizeGroup = sizeGroup;
        _LastAddedColumn.DbfPreliminaryInfo.Type = 'N';
        _LastAddedColumn.DbfPreliminaryInfo.Length = textWidth;
        _LastAddedColumn.DbfPreliminaryInfo.Precision = decimalPlaces;
        _LastAddedColumn.DbfPreliminaryInfo.PrecisionIsDefined = true;
      }
      return nodeControl;
    }

    /// <summary>
    /// Добавляет столбец <see cref="NodeSingleEditBox"/> для просмотра числовых значений
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах (включая десятичную точку)</param>
    /// <param name="decimalPlaces">Количество знаков после запятой</param>
    /// <param name="sizeGroup">Имя группы для синхронного изменения размеров столбцов</param>
    /// <returns>Созданный объект</returns>
    public NodeSingleEditBox AddSingle(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      NodeSingleEditBox nodeControl = new NodeSingleEditBox();
      nodeControl.DecimalPlaces = decimalPlaces;
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.SizeGroup = sizeGroup;
        _LastAddedColumn.DbfPreliminaryInfo.Type = 'N';
        _LastAddedColumn.DbfPreliminaryInfo.Length = textWidth;
        _LastAddedColumn.DbfPreliminaryInfo.Precision = decimalPlaces;
        _LastAddedColumn.DbfPreliminaryInfo.PrecisionIsDefined = true;
      }
      return nodeControl;
    }

    /// <summary>
    /// Добавляет столбец <see cref="NodeInt32EditBox"/> для просмотра числовых значений
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах (включая десятичную точку)</param>
    /// <returns>Созданный объект</returns>
    public NodeInt32EditBox AddInt32(string columnName, bool isDataColumn, string headerText, int textWidth)
    {
      NodeInt32EditBox nodeControl = new NodeInt32EditBox();
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.DbfPreliminaryInfo.Type = 'N';
        _LastAddedColumn.DbfPreliminaryInfo.Length = textWidth;
        _LastAddedColumn.DbfPreliminaryInfo.Precision = 0;
        _LastAddedColumn.DbfPreliminaryInfo.PrecisionIsDefined = true;
      }
      return nodeControl;
    }

    #endregion

    #region Boolean

    /// <summary>
    /// Добавляет столбец-флажок <see cref="NodeCheckBox"/> для просмотра логических значений.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Созданный объект</returns>
    public NodeCheckBox AddCheckBox(string columnName, bool isDataColumn, string headerText)
    {
      NodeCheckBox nodeControl = new NodeCheckBox();
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, 3, 3);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.TreeColumn.Width = ControlProvider.Measures.CheckBoxColumnWidth;
        _LastAddedColumn.TreeColumn.MinColumnWidth = LastAdded.TreeColumn.Width;
        _LastAddedColumn.SizeGroup = "CheckBox";

        _LastAddedColumn.DbfPreliminaryInfo.Type = 'L';
      }
      return nodeControl;
    }


    #endregion

    #region Image

    /// <summary>
    /// Добавляет столбец-изображение <see cref="NodeIcon"/> для просмотра значков.
    /// Столбец привязывается к данным.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <returns>Созданный объект</returns>
    public NodeIcon AddImage(string columnName)
    {
      NodeIcon nodeControl = new NodeIcon();
      //if (isDataColumn)
      //  nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, String.Empty, 3, 3);
      if (ControlProvider.Control.UseColumns)
      {
        LastAdded.TreeColumn.Width = ControlProvider.Measures.ImageColumnWidth;
        LastAdded.TreeColumn.MinColumnWidth = LastAdded.TreeColumn.Width;
        LastAdded.TreeColumn.MaxColumnWidth = LastAdded.TreeColumn.Width;
        LastAdded.SizeGroup = "Image";
      }

      return nodeControl;
    }

    #endregion

#if XXX

    public DataGridViewImageColumn AddImage()
    {
      return AddImage(null);
    }

    public DataGridViewComboBoxColumn AddCombo(string FieldName, bool DataColumn, string HeaderText, int TextWidth, int MinTextWidth)
    {
      if (TextWidth < 1)
        throw new ArgumentOutOfRangeException("TextWidth");
      if (MinTextWidth < 1)
        throw new ArgumentOutOfRangeException("MinTextWidth");

      if (MinTextWidth > TextWidth)
        throw new ArgumentOutOfRangeException("MinTextWidth", MinTextWidth,
          "Минимальная ширина столбца (" + MinTextWidth.ToString() + ") не может быть больше, чем устанавливаемая (" + TextWidth.ToString() + ")");

      DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
      col.Name = FieldName;
      if (DataColumn)
        col.DataPropertyName = FieldName;
      col.HeaderText = HeaderText;
      col.Width = TextWidth * 8 + 10; // !!!
      col.MinimumWidth = MinTextWidth * 8 + 10; // !!!
      col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      ControlProvider.Control.Columns.Add(col);

      return col;
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName, bool DataColumn, string HeaderText, int FillWeight, int MinTextWidth)
    {
      DataGridViewComboBoxColumn col = AddCombo(FieldName, DataColumn, HeaderText, MinTextWidth, MinTextWidth);
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = FillWeight;
      return col;
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName, bool DataColumn, string HeaderText)
    {
      return AddComboFill(FieldName, DataColumn, HeaderText, 100, 5);
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName)
    {
      return AddComboFill(FieldName, true, FieldName, 100, 5);
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName, int FillWeight)
    {
      return AddComboFill(FieldName, true, FieldName, FillWeight, 5);
    }

#endif

    #region Ссылки

    /// <summary>
    /// Создает элемент <see cref="NodeLink"/> для отображения гиперссылки и добавляет его в коллекцию. Если <see cref="TreeViewAdv.UseColumns"/>=true, то также создается столбец <see cref="TreeColumn"/>.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца, которое можно использовать для поиска объекта <see cref="EFPDataTreeViewColumn"/>. 
    /// Если <paramref name="isDataColumn"/>=true, то также задает свойство данных <see cref="BindableControl.DataPropertyName"/>.</param>
    /// <param name="isDataColumn">Если true, то элемент будет автоматически извлекать данные из объектов модели. 
    /// При этом <paramref name="columnName"/> должно указывать на имя свойства или поля объектов узлов модели, или на имя поля, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// Если false, то извлечение данных должно выполняться прикладным кодом</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    /// <returns>Созданный элемент</returns>
    public NodeLink AddLink(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth)
    {
      NodeLink nodeControl = new NodeLink();

      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, minTextWidth);
      if (_LastAddedColumn != null)
      {
        _LastAddedColumn.DbfPreliminaryInfo.Type = 'C';
        _LastAddedColumn.DbfPreliminaryInfo.Length = textWidth;
      }

      return nodeControl;
    }

    /// <summary>
    /// Создает элемент <see cref="NodeLink"/> для отображения гиперссылки, занимающий определенную часть свободного места просмотра.
    /// Задает горизонтальное выравнивание по левому краю.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца, которое можно использовать для поиска объекта <see cref="EFPDataTreeViewColumn"/>. 
    /// Если <paramref name="isDataColumn"/>=true, то также задает свойство данных <see cref="BindableControl.DataPropertyName"/>.</param>
    /// <param name="isDataColumn">Если true, то элемент будет автоматически извлекать данные из объектов модели. 
    /// При этом <paramref name="columnName"/> должно указывать на имя свойства или поля объектов узлов модели, или на имя поля, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// Если false, то извлечение данных должно выполняться прикладным кодом</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="fillWeight">Процент свободного места, занимаемый столбцом.
    /// Не обязательно, чтобы все столбцы занимали ровно 100%.
    /// См. описание свойства <see cref="EFPDataTreeViewColumn.FillWeight"/>.</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах (количество символов средней ширины)</param>
    /// <returns>Созданный элемент</returns>
    public NodeLink AddLinkFill(string columnName, bool isDataColumn, string headerText, int fillWeight, int minTextWidth)
    {
      NodeLink nodeControl = AddLink(columnName, isDataColumn, headerText, minTextWidth, minTextWidth);
      if (_LastAddedColumn != null)
        _LastAddedColumn.FillWeight = fillWeight;
      return nodeControl;
    }

    #endregion


    /// <summary>
    /// Возвращает последний столбец <see cref="EFPDataTreeViewColumn"/>, добавленный с помощью методов AddXXX().
    /// Свойство удобно использовать для установки дополнительных атрибутов столбца,
    /// которые не были установлены в методе AddXXX().
    /// В режиме <see cref="TreeViewAdv.UseColumns"/>=false свойство возвращает null, так как столбцы не создаются
    /// </summary>
    public EFPDataTreeViewColumn LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

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
      //ControlProvider.Control.CancelEdit();
      ITreeModel oldModel = ControlProvider.Control.Model;
      ControlProvider.Control.Model = null;
      try
      {
        //ControlProvider.Control.CurrentCell = null;
        ControlProvider.Control.Columns.Clear();
        ControlProvider.Control.NodeControls.Clear();
      }
      finally
      {
        ControlProvider.Control.Model = oldModel;
      }
    }

    #endregion

    #region Синхронное изменение размеров столбцов

    private bool _InsideColumnWidthChanged = false;

    private bool _ColumnWidthChangedErrorWasShown = false;

    private void Control_ColumnWidthChanged(object sender, TreeColumnEventArgs args)
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
            DebugTools.ShowException(e);
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
    /// Установить свойство <see cref="InteractiveControl.EditEnabled"/> для всех столбцов
    /// </summary>
    /// <param name="value">Устанавливаемое значение (инвертированное) свойства <see cref="InteractiveControl.EditEnabled"/></param>
    public void SetReadOnly(bool value)
    {
      for (int i = 0; i < ControlProvider.Control.NodeControls.Count; i++)
      {
        InteractiveControl nc = ControlProvider.Control.NodeControls[i] as InteractiveControl;
        if (nc == null)
          continue;
        nc.EditEnabled = !value;
      }
    }

    #endregion

    #region Отладочные средства

    /// <summary>
    /// Массив имен всех столбцов в просмотре (отладочное средство)
    /// Свойства <see cref="EFPDataTreeViewColumn.Name"/> 
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
  }

}
