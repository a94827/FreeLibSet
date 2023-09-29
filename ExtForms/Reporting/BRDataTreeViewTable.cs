using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Core;
using FreeLibSet.Models.Tree;
using FreeLibSet.Reporting;

#pragma warning disable 1591


namespace FreeLibSet.Forms.Reporting
{
  #region Перечисления

  /// <summary>
  /// Оформление границ таблицы при печати
  /// </summary>
  internal enum PrintGridBorderStyle
  {
    /// <summary>
    /// Нет линий
    /// </summary>
    NoBorders,

    /// <summary>
    /// Заголовки столбцов обведены рамкой, других линий нет
    /// </summary>
    OnlyHeaders,

    /// <summary>
    /// Столбцы разделены вертикальными линиями, между строками линий нет
    /// </summary>
    Vertical,

    /// <summary>
    /// Сетка. Рисуются линии между строками и столбцами
    /// </summary>
    All
  }

  /// <summary>
  /// Режим вывода нумерации колонок под заголовками столбцов
  /// (свойство GridPageSetup.ColumnSubHeaderNumbers)
  /// </summary>
  internal enum ColumnSubHeaderNumbersMode
  {
    /// <summary>
    /// Нумерация колонок не используется (по умолчанию)
    /// </summary>
    None,

    /// <summary>
    /// Нумерация колонок выводится на каждой странице под заголовками столбцов
    /// </summary>
    All,

    /// <summary>
    /// На первой странице выводятся заголовки столбцов и нумерация колонок, на
    /// второй странице и дальше выводятся номера столбцов без заголовков
    /// </summary>
    Replace
  }

  /// <summary>
  /// Способ раскраски ячеек таблицы
  /// </summary>
  internal enum PrintGridColorStyle
  {
    /// <summary>
    /// При печати раскраска не используется (весь фон - белый)
    /// </summary>
    NoColors,

    /// <summary>
    /// При печати используются те же цвета, что и на экране
    /// </summary>
    Screen,

    /// <summary>
    /// Испольлзуются серые цвета для выделения строк
    /// </summary>
    Gray
  }

  ///// <summary>
  ///// Межстрочный интервал
  ///// </summary>
  //internal enum PrintGridRowSpacing
  //{
  //  /// <summary>
  //  /// Не сжатый (определяется метрикой выбранного шрифта)
  //  /// </summary>
  //  Lead100,


  //  /// <summary>
  //  /// Частично сжатый (оставлено 3/4 Lead space)
  //  /// </summary>
  //  Lead75,

  //  /// <summary>
  //  /// Половинный (половина интервала, определенного метрикой выбранного шрифта)
  //  /// </summary>
  //  Lead50,

  //  /// <summary>
  //  /// Сжатый на 3/4 (оставлена четверть Lead space)
  //  /// </summary>
  //  Lead25,

  //  /// <summary>
  //  /// Сжатый (межстрочный интервал равен высоте шрифта без Lead space)
  //  /// </summary>
  //  Lead0,
  //}

  #endregion

  /// <summary>
  /// Данные параметров страницы для печати EFPDataGridView и EFPDataTreeView.
  /// Содержит список выбранных столбцов для печати, их размеры и параметры для оформления.
  /// Дополняет параметры страницы BRPageSetup и параметры шрифта BRFontSettings
  /// </summary>
  internal class BRDataViewData : SettingsDataItem
  {
    // Есть в Windows-10
    // Нет символов в Linux
    public const char CheckBoxUncheckedChar = '\u2610';
    public const char CheckBoxCheckedChar = '\u2611';
    public const string CheckBoxUncheckedStr = "\u2610";
    public const string CheckBoxCheckedStr = "\u2611";

    #region Конструктор

    public BRDataViewData()
    {
      _ColumnDict = new Dictionary<string, ColumnInfo>();
      _ColumnSubHeaderNumbers = ColumnSubHeaderNumbersMode.None;
      _RepeatedColumnCount = 0;
      _ExpRange = EFPDataViewExpRange.All;
      _UseExpColumnHeaders = true;
      _ExpColumnHeaders = true;
      _UseColorStyle = true;
      _UseBoolMode = true;
      _BoolMode = EFPDataViewExpExcelBoolMode.Boolean;

      _CellLeftMargin = BRReport.AppDefaultCellStyle.LeftMargin;
      _CellTopMargin = BRReport.AppDefaultCellStyle.TopMargin;
      _CellRightMargin = BRReport.AppDefaultCellStyle.RightMargin;
      _CellBottomMargin = BRReport.AppDefaultCellStyle.BottomMargin;
    }

    #endregion

    #region Вложенные классы

    private class ColumnInfo
    {
      #region Поля

      public bool Print;

      public int Width;

      #endregion
    }

    #endregion

    #region Вкладка "Столбцы"

    /// <summary>
    /// Данные столбцов
    /// </summary>
    private readonly Dictionary<string, ColumnInfo> _ColumnDict;

    /// <summary>
    /// Получить флажок печати столбца
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public bool GetColumnPrint(IEFPDataViewColumn column)
    {
      if (!column.Printable)
        return false;
      ColumnInfo ci;
      if (_ColumnDict.TryGetValue(column.Name, out ci))
        return ci.Print;
      else
        return true;
    }

    /// <summary>
    /// Установить флажок печати флажка
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    public void SetColumnPrint(IEFPDataViewColumn column, bool value)
    {
      if (!column.Printable)
        return;

      ColumnInfo ci;
      if (!_ColumnDict.TryGetValue(column.Name, out ci))
      {
        ci = new ColumnInfo();
        ci.Print = true;
        ci.Width = GetWidth(column);
        _ColumnDict.Add(column.Name, ci);
      }
      ci.Print = value;
    }

    /// <summary>
    /// Получить ширину столбца в единицах 0.1мм
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public int GetColumnWidth(IEFPDataViewColumn column)
    {
      if (!column.Printable)
        return 0;
      ColumnInfo ci;
      if (_ColumnDict.TryGetValue(column.Name, out ci))
        return ci.Width;
      else
        return GetWidth(column);
    }

    /// <summary>
    /// Установить ширину столбца в единицах 0.1мм
    /// </summary>
    /// <param name="column"></param>
    /// <param name="value"></param>
    public void SetColumnWidth(IEFPDataViewColumn column, int value)
    {
      if (!column.Printable)
        return;

      ColumnInfo ci;
      if (!_ColumnDict.TryGetValue(column.Name, out ci))
      {
        ci = new ColumnInfo();
        ci.Print = true;
        ci.Width = GetWidth(column);
        _ColumnDict.Add(column.Name, ci);
      }
      ci.Width = value;
    }

    private static int GetWidth(IEFPDataViewColumn column)
    {
      return (int)Math.Round(column.WidthPt / 72.0 * 254.0, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Количество повторяемых в начале каждой полосы столбцов
    /// </summary>
    public int RepeatedColumnCount { get { return _RepeatedColumnCount; } set { _RepeatedColumnCount = value; } }
    private int _RepeatedColumnCount;

    /// <summary>
    /// Нумерация колонок под заголовками столбцов
    /// (по умолчанию - None)
    /// </summary>
    public ColumnSubHeaderNumbersMode ColumnSubHeaderNumbers
    {
      get { return _ColumnSubHeaderNumbers; }
      set { _ColumnSubHeaderNumbers = value; }
    }
    private ColumnSubHeaderNumbersMode _ColumnSubHeaderNumbers;


    #endregion

    #region Вкладка "Оформление"

    #region Управляющие свойства

    /// <summary>
    /// Наличие свойства <see cref="ColorStyle"/> (только для <see cref="EFPDataGridView"/>)
    /// </summary>
    public bool UseColorStyle { get { return _UseColorStyle; } set { _UseColorStyle = value; } }
    private bool _UseColorStyle;

    /// <summary>
    /// Наличие логических значений в просмотре
    /// </summary>
    public bool UseBoolMode { get { return _UseBoolMode; } set { _UseBoolMode = value; } }
    private bool _UseBoolMode;

    #endregion

    /// <summary>
    /// Стиль оформления рамок при печати табличного просмотра
    /// </summary>
    public PrintGridBorderStyle BorderStyle { get { return _BorderStyle; } set { _BorderStyle = value; } }
    private PrintGridBorderStyle _BorderStyle;

    /// <summary>
    /// Стиль оформления цвета ячеек и текста при печати
    /// </summary>
    public PrintGridColorStyle ColorStyle { get { return _ColorStyle; } set { _ColorStyle = value; } }
    private PrintGridColorStyle _ColorStyle;

    ///// <summary>
    ///// Межстрочный интервал
    ///// </summary>
    //public PrintGridRowSpacing RowSpacing { get { return _RowSpacing; } set { _RowSpacing = value; } }
    //private PrintGridRowSpacing _RowSpacing;

    /// <summary>
    /// Режим вывода логических значений
    /// </summary>
    public EFPDataViewExpExcelBoolMode BoolMode { get { return _BoolMode; } set { _BoolMode = value; } }
    private EFPDataViewExpExcelBoolMode _BoolMode;

    public int CellLeftMargin { get { return _CellLeftMargin; } set { _CellLeftMargin = value; } }
    private int _CellLeftMargin;

    public int CellTopMargin { get { return _CellTopMargin; } set { _CellTopMargin = value; } }
    private int _CellTopMargin;

    public int CellRightMargin { get { return _CellRightMargin; } set { _CellRightMargin = value; } }
    private int _CellRightMargin;

    public int CellBottomMargin { get { return _CellBottomMargin; } set { _CellBottomMargin = value; } }
    private int _CellBottomMargin;

    #endregion

    #region Вкладка "Экспорт"

    #region Управляющие свойства

    /// <summary>
    /// Наличие свойства <see cref="ExpColumnHeaders"/>.
    /// Устанавливается в true, если просмотр содержит заголовки столбцов
    /// </summary>
    public bool UseExpColumnHeaders { get { return _UseExpColumnHeaders; } set { _UseExpColumnHeaders = value; } }
    private bool _UseExpColumnHeaders;

    #endregion

    /// <summary>
    /// При экспорте в текстовые форматы: какой диапазон использовать: весь просмотр (по умолчанию) или только выбранные ячейки
    /// </summary>
    public EFPDataViewExpRange ExpRange { get { return _ExpRange; } set { _ExpRange = value; } }
    private EFPDataViewExpRange _ExpRange;

    /// <summary>
    /// При экпорте в текстовые форматы: true (по умолчанию) - выводить заголовки столбцов
    /// </summary>
    public bool ExpColumnHeaders { get { return _ExpColumnHeaders; } set { _ExpColumnHeaders = value; } }
    private bool _ExpColumnHeaders;

    #endregion

    #region ISettingsDataItem

    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      CfgPart cfg2 = cfg.GetChild("Columns", true);
      cfg2.Clear();
      foreach (KeyValuePair<string, ColumnInfo> pair in _ColumnDict)
      {
        CfgPart cfg3 = cfg2.GetChild(pair.Key, true);
        cfg3.SetBool("Print", pair.Value.Print);
        cfg3.SetInt("Width", pair.Value.Width);
      }
      cfg.SetInt("RepeatedColumnCount", RepeatedColumnCount);
      cfg.SetEnum<ColumnSubHeaderNumbersMode>("ColumnSubHeaderNumbers", ColumnSubHeaderNumbers);

      cfg.SetEnum<PrintGridBorderStyle>("BorderStyle", BorderStyle);
      if (UseColorStyle)
        cfg.SetEnum<PrintGridColorStyle>("ColorStyle", ColorStyle);
      //cfg.SetEnum<PrintGridRowSpacing>("RowSpacing", RowSpacing);
      if (UseBoolMode)
        cfg.SetEnum<EFPDataViewExpExcelBoolMode>("BoolMode", BoolMode);
      cfg.SetInt("CellLeftMargin", CellLeftMargin);
      cfg.SetInt("CellTopMargin", CellTopMargin);
      cfg.SetInt("CellRightMargin", CellRightMargin);
      cfg.SetInt("CellBottomMargin", CellBottomMargin);

      cfg.SetEnum<EFPDataViewExpRange>("ExpRange", ExpRange);
      cfg.SetBool("ExpColumnHeaders", ExpColumnHeaders);
    }

    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      _ColumnDict.Clear();
      CfgPart cfg2 = cfg.GetChild("Columns", false);
      if (cfg2 != null)
      {
        foreach (string colName in cfg2.GetChildNames())
        {
          CfgPart cfg3 = cfg2.GetChild(colName, false);
          ColumnInfo ci = new ColumnInfo();
          ci.Print = cfg3.GetBoolDef("Print", true);
          ci.Width = cfg3.GetInt("Width");
          _ColumnDict.Add(colName, ci);
        }
      }
      RepeatedColumnCount = cfg.GetInt("RepeatedColumnCount");
      ColumnSubHeaderNumbers = cfg.GetEnum<ColumnSubHeaderNumbersMode>("ColumnSubHeaderNumbers");

      BorderStyle = cfg.GetEnumDef<PrintGridBorderStyle>("BorderStyle", BorderStyle);
      if (UseColorStyle)
        ColorStyle = cfg.GetEnumDef<PrintGridColorStyle>("ColorStyle", ColorStyle);
      //RowSpacing = cfg.GetEnumDef<PrintGridRowSpacing>("RowSpacing", RowSpacing);
      if (UseBoolMode)
        BoolMode = cfg.GetEnumDef<EFPDataViewExpExcelBoolMode>("BoolMode", BoolMode);
      CellLeftMargin = cfg.GetIntDef("CellLeftMargin", CellLeftMargin);
      CellTopMargin = cfg.GetIntDef("CellTopMargin", CellTopMargin);
      CellRightMargin = cfg.GetIntDef("CellRightMargin", CellRightMargin);
      CellBottomMargin = cfg.GetIntDef("CellBottomMargin", CellBottomMargin);

      ExpRange = cfg.GetEnumDef<EFPDataViewExpRange>("ExpRange", EFPDataViewExpRange.All);
      ExpColumnHeaders = cfg.GetBoolDef("ExpColumnHeaders", ExpColumnHeaders);
    }

    #endregion
  }

  /// <summary>
  /// Виртуальная таблица для иерархического просмотра
  /// </summary>
  public class BRDataTreeViewTable : BRVirtualTable
  {
    #region Конструктор

    /// <summary>
    /// Описатель печатаемого столбца
    /// В TreeViewAdv и EFPDataTreeView одному столбцу TreeColumn/EFPDataTreeViewColumn может соответствовать несколько элементов NodeControl.
    /// При печати они разделяются на отдельные столбцы.
    /// </summary>
    private struct ColumnInfo
    {
      #region Поля

      /// <summary>
      /// Описатель столбца. Null для единственного столбца в режиме TreeViewAdv.UseColumns=false
      /// </summary>
      public EFPDataTreeViewColumn EFPColumn;

      /// <summary>
      /// Объект для извлечения данных в режиме "1 NodeControl - 1 BRColumn". 
      /// </summary>
      public InteractiveControl NodeControl;

      /// <summary>
      /// Объекты для извлечения данных в режиме "несколько NodeControl - 1 RBColumn"
      /// </summary>
      public InteractiveControl[] NodeControls;

      /// <summary>
      /// Ширина столбца в единицах 0.1мм
      /// </summary>
      public int Width;

      /// <summary>
      /// True для первого NodeControl в столбце
      /// </summary>
      public bool FirstInColumn;

      /// <summary>
      /// True для последнего NodeControl в столбце
      /// </summary>
      public bool LastInColumn;

      /// <summary>
      /// Если true, то для этого столбца будет устанавливаться отступ
      /// </summary>
      public bool UseIndent;

      public bool Repeatable;

      #endregion

      #region Методы

      public override string ToString()
      {
        if (EFPColumn == null)
          return NodeControl.ToString();
        else
          return EFPColumn.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Класс нужен для вызова базового конструктора BRVirtualTable, чтобы передать количество строк и столбцов
    /// </summary>
    private class InternalInfo
    {
      #region Конструктор

      public InternalInfo(EFPDataTreeView controlProvider, SettingsDataList settingsData, bool useExport)
      {
        if (controlProvider == null)
          throw new ArgumentNullException("controlProvider");
        if (settingsData == null)
          throw new ArgumentNullException("settingsData");
        _ControlProvider = controlProvider;
        _ViewData = settingsData.GetRequired<BRDataViewData>();
        BRPageSetup pageSetup = settingsData.GetRequired<BRPageSetup>();

        InitRows(useExport);

        InitColumns(useExport, pageSetup);

        _FirstDataRow = _Headers.RowCount;

        _SubHeaderNumberRowIndex = -1;
        if (controlProvider.Control.UseColumns && _ViewData.ColumnSubHeaderNumbers != ColumnSubHeaderNumbersMode.None && _Headers2.RowCount == 1)
        {
          _SubHeaderNumberRowIndex = _FirstDataRow;
          _FirstDataRow++;
        }

        if (FirstDataRow == 0 && RowNodes.Length == 0)
        {
          // Если нет ни одной строки, то возникнет исключение при создании BRBand.
          // Добавляем одну пустую строку "заголовка"
          _Headers = new BRColumnHeaderArray(1, Columns.Length);
          _FirstDataRow = 1;
        }
      }

      private void InitRows(bool useExport)
      {
        if (_ControlProvider.Control.Model == null)
        {
          //_Rows = new TreePath[0];
          _RowNodes = new TreeNodeAdv[0];
        }
        else
        {
          List<TreeNodeAdv> lstNodes = new List<TreeNodeAdv>();

          if (useExport && _ViewData.ExpRange == EFPDataViewExpRange.Selected)
          {
            // Выбранные узлы в просмотре

            foreach (TreeNodeAdv node in _ControlProvider.Control.SelectedNodes)
            {
              //TreePath path=_ControlProvider.Control.GetPath(node)
              lstNodes.Add(node);
            }
          }
          else
          {
            // Все узлы модели

            foreach (TreePath path in new TreePathEnumerable(_ControlProvider.Control.Model))
            {
              TreeNodeAdv node = _ControlProvider.Control.FindNode(path, true);
              if (node == null)
                throw new BugException("Не найден узел дерева");
              lstNodes.Add(node);
            }
          }
          _RowNodes = lstNodes.ToArray();
        }
      }


      private void InitColumns(bool useExport, BRPageSetup pageSetup)
      {
        if (_ControlProvider.GetFirstNodeControl<InteractiveControl>() == null)
          throw new InvalidOperationException("Просмотр не содержит объектов InteractiveControl");

        // Выводимые столбцы для TreeViewAdv не зависят от режима ExpRange, так как выбирается строка целиком

        List<ColumnInfo> lst = new List<ColumnInfo>();
        bool useIndent = true;
        if (_ControlProvider.Control.UseColumns)
        {
          #region UseColumns=true

          List<string[]> headList = new List<string[]>();
          List<string[]> headList2 = new List<string[]>();

          int cntCol = 0;
          foreach (EFPDataTreeViewColumn efpCol in ControlProvider.Columns)
          {
            if (!efpCol.TreeColumn.IsVisible)
              continue;
            if (!efpCol.Printable)
              continue;

            if (!_ViewData.GetColumnPrint(efpCol))
              continue;

            int w = _ViewData.GetColumnWidth(efpCol);
            InteractiveControl[] ctrs = ControlProvider.GetNodeControls<InteractiveControl>(efpCol.TreeColumn);
            for (int i = 0; i < ctrs.Length; i++)
            {
              ColumnInfo ci = new ColumnInfo();
              ci.EFPColumn = efpCol;
              ci.NodeControl = ctrs[i];
              ci.Width = w / ctrs.Length; // TODO: Распределение ширины
              ci.FirstInColumn = (i == 0);
              ci.LastInColumn = (i == (ctrs.Length - 1));
              if (ctrs[i] is BaseTextControl && useIndent)
              {
                ci.UseIndent = true;
                useIndent = false;
              }

              ci.Repeatable = cntCol < _ViewData.RepeatedColumnCount;

              lst.Add(ci);

              if (efpCol.PrintHeaders != null)
                headList.Add(efpCol.PrintHeaders);
              else if (!String.IsNullOrEmpty(efpCol.TreeColumn.Header))
                headList.Add(new string[1] { efpCol.TreeColumn.Header });
              else
                headList.Add(DataTools.EmptyStrings);
              headList2.Add(new string[1] { (cntCol + 1).ToString() });
            }

            cntCol++;
          }

          if (useExport && (!_ViewData.ExpColumnHeaders))
          {
            _Headers = new BRColumnHeaderArray(0, headList.Count);
            _Headers2 = null;
          }
          else
          {
            _Headers = new BRColumnHeaderArray(headList.ToArray(), _ControlProvider.ColumnHeaderMixedSpanAllowed);
            _Headers2 = new BRColumnHeaderArray(headList2.ToArray());

#if DEBUG
            if (_Headers2.RowCount > 1)
              throw new BugException("Headers2");
#endif
          }

          _Columns = lst.ToArray();

          #endregion
        }
        else
        {
          #region UseColumns = false

          ColumnInfo ci = new ColumnInfo();

          InteractiveControl[] ctrs = ControlProvider.GetNodeControls<InteractiveControl>();
          if (ctrs.Length == 1)
            ci.NodeControl = ctrs[0];
          else
            ci.NodeControls = ctrs;
          ci.UseIndent = true;
          ci.Width = 0;
          ci.FirstInColumn = true;
          ci.LastInColumn = true;
          _Columns = new ColumnInfo[1] { ci };
          _Headers = new BRColumnHeaderArray(0, 1);
          _Headers2 = null;

          #endregion
        }
      }


      public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
      private EFPDataTreeView _ControlProvider;

      public BRDataViewData ViewData { get { return _ViewData; } }
      private BRDataViewData _ViewData;

      public TreeNodeAdv[] RowNodes { get { return _RowNodes; } }
      private TreeNodeAdv[] _RowNodes;


      /// <summary>
      /// Индекс первой строки данных, с учетом заголовков
      /// </summary>
      public int FirstDataRow { get { return _FirstDataRow; } }
      private int _FirstDataRow;

      public ColumnInfo[] Columns { get { return _Columns; } }
      private ColumnInfo[] _Columns;

      public BRColumnHeaderArray Headers { get { return _Headers; } }
      private BRColumnHeaderArray _Headers;

      /// <summary>
      /// Номера столбцов как однострочный объект заголовков
      /// </summary>
      public BRColumnHeaderArray Headers2 { get { return _Headers2; } }
      private BRColumnHeaderArray _Headers2;


      /// <summary>
      /// Индекс строки, в которой выводятся номера столбцов 1,2,3...
      /// Если нумерация не используется, то возвращает (-1).
      /// </summary>
      public int SubHeaderNumberRowIndex { get { return _SubHeaderNumberRowIndex; } }
      private int _SubHeaderNumberRowIndex;

      #endregion
    }

    public BRDataTreeViewTable(BRSection section, EFPDataTreeView controlProvider, SettingsDataList settingsData, bool useExport)
      : this(section, new InternalInfo(controlProvider, settingsData, useExport))
    {
    }

    private BRDataTreeViewTable(BRSection section, InternalInfo info)
      : base(section, info.RowNodes.Length + info.FirstDataRow, info.Columns.Length)
    {
      _Info = info;
      _SB = new StringBuilder();
    }

    private InternalInfo _Info;

    private StringBuilder _SB;

    #endregion

    #region Реализация методов

    protected override object GetValue(int rowIndex, int columnIndex)
    {
      if (rowIndex >= _Info.FirstDataRow)
      {
        TreeNodeAdv node = _Info.RowNodes[rowIndex - _Info.FirstDataRow];
        InteractiveControl nc = _Info.Columns[columnIndex].NodeControl;
        if (nc != null)
        {
          // Обычный режим: один столбец отчета - один NodeControl
          return DoGetValue(node, nc);

        }
        else
        {
          // Несколько NodeControl для столбца отчета
          // Превращаем каждое значение в строку

          _SB.Length = 0;

          for (int i = 0; i < _Info.Columns[columnIndex].NodeControls.Length; i++)
          {
            object v = DoGetValue(node, _Info.Columns[columnIndex].NodeControls[i]);
            if (v == null)
              continue;
            if (v is IFormattable)
            {
              BaseFormattedTextControl ctlFormat = _Info.Columns[columnIndex].NodeControls[i] as BaseFormattedTextControl;
              if (ctlFormat != null)
                v = ((IFormattable)v).ToString(ctlFormat.Format, ctlFormat.FormatProvider);
            }

            string s = v.ToString();
            if (s.Length > 0)
            {
              if (_SB.Length > 0)
                _SB.Append(" ");
              _SB.Append(s);
            }
          }

          return _SB.ToString();
        }
      }
      else if (rowIndex < +_Info.Headers.RowCount)
      {
        return _Info.Headers.Text[rowIndex, columnIndex];
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
        return _Info.Headers2.Text[0, columnIndex];
      else
        throw new BugException();
    }

    private object DoGetValue(TreeNodeAdv node, InteractiveControl nc)
    {
      object v = nc.GetValue(node);
      if (v is Boolean)
      {
        switch (_Info.ViewData.BoolMode)
        {
          case EFPDataViewExpExcelBoolMode.Boolean:
            break;
          case EFPDataViewExpExcelBoolMode.Brackets:
            //v = (bool)v ? "[X]" : "[ ]";
            v = (bool)v ? BRDataViewData.CheckBoxCheckedStr : BRDataViewData.CheckBoxUncheckedStr;
            break;
          case EFPDataViewExpExcelBoolMode.Digit:
            v = (bool)v ? 1 : 0;
            break;
          default:
            throw new BugException("BoolMode=" + _Info.ViewData.BoolMode.ToString());
        }
      }
      return v;
    }

    protected override void FillCellStyle(int rowIndex, int columnIndex, BRCellStyle style)
    {
      style.LeftMargin = _Info.ViewData.CellLeftMargin;
      style.TopMargin = _Info.ViewData.CellTopMargin;
      style.RightMargin = _Info.ViewData.CellRightMargin;
      style.BottomMargin = _Info.ViewData.CellBottomMargin;

      if (rowIndex >= _Info.FirstDataRow)
      {
        #region Область данных

        if (_Info.ViewData.BorderStyle == PrintGridBorderStyle.All)
        {
          style.TopBorder = BRLine.Thin;
          style.BottomBorder = BRLine.Thin;
        }

        if (_Info.ViewData.BorderStyle == PrintGridBorderStyle.All || _Info.ViewData.BorderStyle == PrintGridBorderStyle.Vertical)
        {
          if (_Info.Columns[columnIndex].FirstInColumn)
          {
            //if (_Info.Columns[columnIndex].EFPColumn == null)
            style.LeftBorder = BRLine.Thin;
            //else (_Info.Columns[columnIndex].EFPColumn.LeftBorder.)
          }
          else
            style.LeftBorder = BRLine.None;

          if (_Info.Columns[columnIndex].LastInColumn)
          {
            //if (_Info.Columns[columnIndex].EFPColumn == null)
            style.RightBorder = BRLine.Thin;
            //else (_Info.Columns[columnIndex].EFPColumn.LeftBorder.)
          }
          else
            style.LeftBorder = BRLine.None;
        }

        style.VAlign = BRVAlign.Center;
        style.WrapMode = BRWrapMode.WordWrap;

        if (_Info.Columns[columnIndex].UseIndent)
          style.IndentLevel = _Info.RowNodes[rowIndex - _Info.FirstDataRow].Level - 1;
        if (_Info.Columns[columnIndex].NodeControl == null)
        {
          style.HAlign = BRHAlign.Left;
          style.VAlign = BRVAlign.Center;
        }
        else
        {
          BaseTextControl ctlText = _Info.Columns[columnIndex].NodeControl as BaseTextControl;
          if (ctlText != null)
          {
            switch (ctlText.TextAlign)
            {
              case System.Windows.Forms.HorizontalAlignment.Left: style.HAlign = BRHAlign.Left; break;
              case System.Windows.Forms.HorizontalAlignment.Center: style.HAlign = BRHAlign.Center; break;
              case System.Windows.Forms.HorizontalAlignment.Right: style.HAlign = BRHAlign.Right; break;
            }
            switch (ctlText.VerticalAlign)
            {
              case System.Windows.Forms.VisualStyles.VerticalAlignment.Top: style.VAlign = BRVAlign.Top; break;
              case System.Windows.Forms.VisualStyles.VerticalAlignment.Center: style.VAlign = BRVAlign.Center; break;
              case System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom: style.VAlign = BRVAlign.Bottom; break;
            }
          }
          else
          {
            style.HAlign = BRHAlign.Center;
            style.VAlign = BRVAlign.Center;
          }
          BaseFormattedTextControl ctlFormat = _Info.Columns[columnIndex].NodeControl as BaseFormattedTextControl;
          if (ctlFormat != null)
          {
            style.Format = ctlFormat.Format;
            style.FormatProvider = ctlFormat.FormatProvider;
          }
        }

        #endregion
      }
      else if (rowIndex < _Info.Headers.RowCount)
      {
        #region Заголовки

        style.HAlign = BRHAlign.Center;
        if (_Info.ViewData.BorderStyle != PrintGridBorderStyle.NoBorders)
          style.AllBorders = BRLine.Thin;
        style.WrapMode = BRWrapMode.WordWrap;

        #endregion
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        #region Нумерация столбцов

        style.HAlign = BRHAlign.Center;
        if (_Info.ViewData.BorderStyle != PrintGridBorderStyle.NoBorders)
          style.AllBorders = BRLine.Thin;

        #endregion
      }
      else
        throw new BugException();
    }

    protected override void FillColumnInfo(int columnIndex, BRColumnInfo columnInfo)
    {
      if (_Info.Columns[columnIndex].Width > 0)
        columnInfo.SetWidth(_Info.Columns[columnIndex].Width, false);
      columnInfo.Repeatable = _Info.Columns[columnIndex].Repeatable;
    }

    protected override void FillRowInfo(int rowIndex, BRRowInfo rowInfo)
    {
      if (rowIndex < _Info.Headers.RowCount)
      {
        rowInfo.KeepWithNext = true;
        if (_Info.ViewData.ColumnSubHeaderNumbers != ColumnSubHeaderNumbersMode.Replace)
          rowInfo.Repeatable = true;
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        rowInfo.KeepWithNext = true;
        rowInfo.Repeatable = true;
      }
      else if (rowIndex == (_Info.RowNodes.Length - 1))
        rowInfo.KeepWithPrev = true;
    }

    protected override BRRange GetMergeInfo(int rowIndex, int columnIndex)
    {
      if (rowIndex < _Info.Headers.RowCount)
      {
        int firstRowIndex, firstColumn, rowCount, columnCount;
        _Info.Headers.GetMergeArea(rowIndex, columnIndex, out firstRowIndex, out firstColumn, out rowCount, out columnCount);
        return new BRRange(firstRowIndex, firstColumn, rowCount, columnCount);
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        int firstRowIndex, firstColumn, rowCount, columnCount;
        _Info.Headers2.GetMergeArea(0, columnIndex, out firstRowIndex, out firstColumn, out rowCount, out columnCount);
        return new BRRange(firstRowIndex + rowIndex, firstColumn, rowCount, columnCount);
      }
      else
        return base.GetMergeInfo(rowIndex, columnIndex);
    }

    #endregion
  }
}
