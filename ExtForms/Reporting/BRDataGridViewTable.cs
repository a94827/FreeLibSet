using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;
using FreeLibSet.Reporting;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Reporting
{
  /// <summary>
  /// Базовый класс для <see cref="BRDataGridViewMenuOutItem"/> и <see cref="BRDataTreeViewMenuOutItem"/>
  /// </summary>
  public abstract class BRDataViewMenuOutItemBase : BRMenuOutItem
  {
    #region Конструктор

    internal BRDataViewMenuOutItemBase(string code, IEFPDataView controlProvider)
      : base(code)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      _ControlProvider = controlProvider;
      SettingsData.Add(new BRFontSettingsDataItem());
      SettingsData.Add(new BRDataViewSettingsDataItem());
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public IEFPDataView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPDataView _ControlProvider;

    /// <summary>
    /// Доступ к настройкам печати просмотра по умолчанию
    /// </summary>
    public BRDataViewMenuOutSettings Default { get { return new BRDataViewMenuOutSettings(SettingsData); } }

    /// <summary>
    /// Доступ к именным настройкам печати просмотра.
    /// </summary>
    /// <param name="defCfgCode">Код для именной настройки</param>
    /// <returns>Объект для доступа к настройкам</returns>
    public BRDataViewMenuOutSettings this[string defCfgCode]
    {
      get
      {
        if (String.IsNullOrEmpty(defCfgCode))
          return Default;
        else
          return new BRDataViewMenuOutSettings(SettingsData.DefaultConfigs[defCfgCode]);
      }
    }

    /// <summary>
    /// Добавление именной настройки печати просмотра
    /// </summary>
    /// <param name="defCfgCode">Код для именной настройки</param>
    /// <param name="displayName">Отображаемое название настройки</param>
    /// <returns>Объект для доступа к настройкам</returns>
    public BRDataViewMenuOutSettings Add(string defCfgCode, string displayName)
    {
      if (String.IsNullOrEmpty(defCfgCode))
        throw new ArgumentNullException("defCfgCode");
      SettingsData.DefaultConfigs[defCfgCode].DisplayName = displayName;
      return new BRDataViewMenuOutSettings(SettingsData.DefaultConfigs[defCfgCode]);
    }

    #endregion

    #region Заголовок и табличка фильтров

    /// <summary>
    /// Заголовок отчета.
    /// Свойство может устанавливаться из прикладного кода или обработчика события <see cref="TitleNeeded"/>. 
    /// Установка из прикладного кода отключает вызов события <see cref="TitleNeeded"/>.
    /// </summary>
    public string Title
    {
      get { return _Title ?? String.Empty; }
      set
      {
        _Title = value;
        _TitleHasBeenSet = true;
      }
    }
    private string _Title;

    private bool _TitleHasBeenSet;

    /// <summary>
    /// Фильтры отчета.
    /// Список может заполняться из прикладного кода или обработчика события <see cref="TitleNeeded"/>. 
    /// Заполнение из прикладного кода отключает вызов события <see cref="TitleNeeded"/>.
    /// </summary>
    public EFPReportFilterItems FilterInfo
    {
      get
      {
        if (_FilterInfo == null)
          _FilterInfo = new EFPReportFilterItems();
        return _FilterInfo;
      }
    }
    private EFPReportFilterItems _FilterInfo;

    /// <summary>
    /// Событие вызывается при формировании отчета в <see cref="BRMenuOutItem.OnCreateReport(BRMenuOutItemCreateReportEventArgs)"/>.
    /// Обработчик события может установить свойство <see cref="Title"/> и заполнить табличку фильтров <see cref="FilterInfo"/>.
    /// Установка этих свойств из прикладного кода предотвращает вызов обработчика события.
    /// </summary>
    public event EventHandler TitleNeeded;

    /// <summary>
    /// Если свойства <see cref="Title"/> и <see cref="FilterInfo"/> не были установлены из прикладного кода до первого вызова этого метода,
    /// вызывает обработчик события <see cref="TitleNeeded"/>. 
    /// В противном случае никаких действий не выполняется.
    /// </summary>
    protected void InitTitle()
    {
      if (!_InitTitleCalled)
      {
        _InitTitleCalled = true;
        if ((!_TitleHasBeenSet) && FilterInfo.Count == 0)
          _UseTitleNeeded = true;
      }

      if (_UseTitleNeeded)
      {
        Title = GetAutoTitle(this.ControlProvider);
        FilterInfo.Clear();

        if (TitleNeeded != null)
          TitleNeeded(this, EventArgs.Empty);
      }
    }

    private bool _InitTitleCalled;

    private bool _UseTitleNeeded;



    /// <summary>
    /// Возвращает текст для заголовка
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    /// <returns>Текст автоматически определяемого заголовка</returns>
    private static string GetAutoTitle(IEFPDataView controlProvider)
    {
      if (!String.IsNullOrEmpty(controlProvider.DocumentProperties.Title))
        return controlProvider.DocumentProperties.Title;

      Form frm = controlProvider.Control.FindForm();
      if (frm != null)
      {
        if (!String.IsNullOrEmpty(frm.Text))
          return frm.Text;
      }
      return controlProvider.DisplayName;
    }


    private bool _ForceTableTitleAndFilterBands;

    internal void AddTitleAndFilterBands(BRSection sect, BRMenuOutItemCreateReportEventArgs args)
    {
      InitTitle();

      BRDataViewSettingsDataItem viewSettings = SettingsData.GetRequired<BRDataViewSettingsDataItem>();


      #region Заголовок

      bool expTableHeader = _ForceTableTitleAndFilterBands ? true: viewSettings.ExpTableHeader;

      if (expTableHeader && (!String.IsNullOrEmpty(Title)))
      {
        BRTable table = sect.Bands.Add(1, 1);
        table.Cells.Value = Title;
        table.Cells.CellStyle.Bold = true;
        table.Cells.CellStyle.HAlign = BRHAlign.Center;
        table.Cells.CellStyle.WrapMode = BRWrapMode.WordWrap;
      }

      #endregion

      #region Табличка фильтров

      bool expTableFilters = _ForceTableTitleAndFilterBands ? true: viewSettings.ExpTableFilters;
      if ( expTableFilters && FilterInfo.Count > 0)
      {
        int wantedW = 0;
        for (int i = 0; i < FilterInfo.Count; i++)
        {
          int w, h;

          Drawing.Reporting.BRMeasurer.Default.MeasureString(BRPaginator.PrepareStringForMeasure(FilterInfo[i].DisplayName), sect.Report.DefaultCellStyle, out w, out h);
          wantedW = Math.Max(wantedW, w);
        }
        wantedW += sect.Report.DefaultCellStyle.LeftMargin + sect.Report.DefaultCellStyle.LeftMargin;
        wantedW = Math.Min(wantedW, sect.PageSetup.PrintAreaWidth / 2);

        BRTable table = sect.Bands.Add(FilterInfo.Count, 3);
        table.TopMargin = 20; // Отступ от заголовка
        table.BottomMargin = 20; // Отступ от таблицы данных
        table.DefaultCellStyle.HAlign = BRHAlign.Left;
        table.DefaultCellStyle.WrapMode = BRWrapMode.WordWrap;
        table.Cells.ColumnIndex = 0;
        table.Cells.ColumnInfo.SetWidth(wantedW, false);
        table.Cells.ColumnIndex = 1; // Зазор
        table.Cells.ColumnInfo.SetWidth(50, false);

        table.Cells.ColumnIndex = 2;
        table.Cells.ColumnCellStyle.BottomBorder = BRLine.Thin;

        for (int i = 0; i < FilterInfo.Count; i++)
        {
          table.SetValue(i, 0, FilterInfo[i].DisplayName);
          table.SetValue(i, 2, FilterInfo[i].Value);
        }
      }

      #endregion
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Если свойство <see cref="BRMenuOutItem.ConfigSectionName"/> не установлено и <see cref="EFPMenuOutItem.Code"/>="Control", 
    /// то возвращается <see cref="IEFPControl.ConfigSectionName"/>, а не "Default".
    /// </summary>
    /// <returns></returns>
    protected override string GetConfigSectionName()
    {
      if (String.IsNullOrEmpty(ConfigSectionName) &&
        String.Equals(Code, "Control", StringComparison.Ordinal) &&
        (!String.IsNullOrEmpty(ControlProvider.ConfigSectionName)))
        return ControlProvider.ConfigSectionName; // 07.03.2024

      return base.GetConfigSectionName();
    }

    /// <summary>
    /// Добавляет форматы экспорта в текстовые форматы, не связанные с <see cref="BRReport"/>
    /// </summary>
    /// <param name="args"></param>
    protected override void OnPrepare(EFPMenuOutItemPrepareEventArgs args)
    {
      base.OnPrepare(args);

      // Пусть эти форматы будут в конце
      base.ExportFileItems.Add(new EFPExportFileItem("TXT", "Текст (разделитель - табуляция) ", "*.txt"));
      base.ExportFileItems.Add(new EFPExportFileItem("CSV", "Текст CSV", "*.csv"));
      if (ControlProvider.Columns.Count > 0) // Для TreeViewAdv без столбцов пока не реализовано
        base.ExportFileItems.Add(new EFPExportFileItem("DBF3", "Файлы dBase III", "*.dbf"));
    }


    /// <summary>
    /// Многократно вызывается при подготовке списка команд меню к использованию
    /// </summary>
    /// <param name="args"></param>
    protected override void OnPrepareAction(EventArgs args)
    {
      InitTitle();
      _ForceTableTitleAndFilterBands = true;
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseExpTableHeader = !String.IsNullOrEmpty(Title);
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseExpTableFilters = FilterInfo.Count > 0;
      base.OnPrepareAction(args);
    }

    /// <summary>
    /// Инициализация блока диалога параметров
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnInitDialog(BRMenuOutItemInitDialogEventArgs args)
    {
      switch (args.DialogKind)
      {
        case BRDialogKind.PageSetup:
          args.AddFontPage();
          new BRDataViewPageSetupColumns(args.Dialog, ControlProvider);
          new BRDataViewPageSetupAppearance(args.Dialog, ControlProvider, false);
          break;
        case BRDialogKind.ControlSendTo:
          if (BRSendToItemCodes.IsTextApp(args.ActionInfo.SendToItem.MainCode) ||
            BRSendToItemCodes.IsWorksheetApp(args.ActionInfo.SendToItem.MainCode))
          {
            _ForceTableTitleAndFilterBands = false;
            new BRDataViewPageSetupSendTo(args.Dialog, ControlProvider, args.DialogKind);
          }
          break;
        case BRDialogKind.ControlExportFile:
          if (BRExportFileItemCodes.IsTextDocument(args.ActionInfo.ExportFileItem.Code) ||
            BRExportFileItemCodes.IsWorksheet(args.ActionInfo.ExportFileItem.Code))
          {
            _ForceTableTitleAndFilterBands = false;
            new BRDataViewPageSetupSendTo(args.Dialog, ControlProvider, args.DialogKind);
          }
          break;
      }
      base.OnInitDialog(args);
    }

    /// <summary>
    /// Обработка текстовых форматов экспорта
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="item">Описание формата экспорта</param>
    public override void ExportFile(AbsPath filePath, EFPExportFileItem item)
    {
      switch (item.Code)
      {
        case "CSV":
        case "TXT":
          if (ShowExportTextFileDialog(filePath, item))
          {
            BRDataViewFileText fileCreator = new BRDataViewFileText(this.SettingsData.GetRequired<BRDataViewSettingsDataItem>());
            fileCreator.CreateFile(ControlProvider, filePath, item.Code == "CSV");
          }
          break;
        case "DBF3":
          if (ShowExportDbfFileDialog(filePath, item))
          {
            BRDataViewFileDbf fileCreator = new BRDataViewFileDbf(this.SettingsData.GetRequired<BRDataViewSettingsDataItem>());
            fileCreator.CreateFile(ControlProvider, filePath);
          }
          break;
        default:
          base.ExportFile(filePath, item);
          break;
      }
    }

    private bool ShowExportTextFileDialog(AbsPath filePath, EFPExportFileItem item)
    {
      #region Создание SettingsDialog 

      CallReadConfig();
      SettingsDialog dialog = new SettingsDialog();
      dialog.ConfigSectionName = GetConfigSectionName();
      dialog.Data = this.SettingsData;
      dialog.Title = "Экспорт в " + filePath.FileName;
      dialog.ImageKey = "Save";

      new BRDataViewPageSetupText(dialog, ControlProvider, item.Code == "CSV");
      if (dialog.Data.GetRequired<BRDataViewSettingsDataItem>().UseBoolMode)
        new BRDataViewPageSetupAppearance(dialog, ControlProvider, true);

      #endregion

      #region Показ диалога

      if (dialog.ShowDialog() != DialogResult.OK)
        return false;

      CallWriteConfig();
      return true;

      #endregion
    }


    private bool ShowExportDbfFileDialog(AbsPath filePath, EFPExportFileItem item)
    {
      #region Создание SettingsDialog 

      CallReadConfig();
      SettingsDialog dialog = new SettingsDialog();
      dialog.ConfigSectionName = GetConfigSectionName();
      dialog.Data = this.SettingsData;
      dialog.Title = "Экспорт в " + filePath.FileName;
      dialog.ImageKey = "Save";

      new BRDataViewPageSetupDbf(dialog, ControlProvider);

      #endregion

      #region Показ диалога

      if (dialog.ShowDialog() != DialogResult.OK)
        return false;

      CallWriteConfig();
      return true;

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Объект для печати/экспорта иерархического просмотра
  /// </summary>
  public sealed class BRDataGridViewMenuOutItem : BRDataViewMenuOutItemBase
  {
    #region Конструктор

    /// <summary>
    /// Создание объекта для вывода табличного просмотра
    /// </summary>
    /// <param name="code">Код объекта. Обычно, "Control"</param>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public BRDataGridViewMenuOutItem(string code, EFPDataGridView controlProvider)
      : base(code, controlProvider)
    {
      DisplayName = "Табличный просмотр";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPDataGridView ControlProvider { get { return (EFPDataGridView)(base.ControlProvider); } }

    #endregion

    #region Обработчики событий

    /// <summary>
    /// Подготовка к выполнению действий
    /// </summary>
    protected override void OnPrepareAction(EventArgs args)
    {
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseExpColumnHeaders = ControlProvider.Control.ColumnHeadersVisible;
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseColorStyle = true;

      bool hasBoolColumns = false;
      foreach (EFPDataGridViewColumn col in ControlProvider.VisibleColumns)
      {
        if (col.GridColumn is DataGridViewCheckBoxColumn)
        {
          hasBoolColumns = true;
          break;
        }
      }
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseBoolMode = hasBoolColumns;

      base.OnPrepareAction(args);
    }

    /// <summary>
    /// Создание отчета из табличного просмотра
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnCreateReport(BRMenuOutItemCreateReportEventArgs args)
    {
      BRFontSettingsDataItem fontSettings = SettingsData.GetRequired<BRFontSettingsDataItem>();
      fontSettings.InitCellStyle(args.Report.DefaultCellStyle);
      args.Report.DocumentProperties = ControlProvider.DocumentProperties.Clone();
      BRSection sect = args.Report.Sections.Add();
      sect.PageSetup = SettingsData.GetItem<BRPageSettingsDataItem>().PageSetup;
      AddTitleAndFilterBands(sect, args);
      if (ControlProvider.Columns.Count > 0)
        sect.Bands.Add(new BRDataGridViewTable(sect, ControlProvider, SettingsData, args.ActionInfo.Action == BRAction.SendTo));

      base.OnCreateReport(args);
    }

    #endregion
  }

  /// <summary>
  /// Доступ к именным настройкам печати/экспорта табличного или иерархического просмотра или настройкам по умолчанию.
  /// </summary>
  public struct BRDataViewMenuOutSettings
  {
    internal BRDataViewMenuOutSettings(SettingsDataListBase list)
    {
      _List = list;
    }

    private SettingsDataListBase _List;

    /// <summary>
    /// Параметры страницы
    /// </summary>
    public BRPageSetup PageSetup { get { return _List.GetRequired<BRPageSettingsDataItem>().PageSetup; } }

    /// <summary>
    /// Параметры шрифта
    /// </summary>
    public BRFontSettingsDataItem Font { get { return _List.GetRequired<BRFontSettingsDataItem>(); } }

    /// <summary>
    /// Параметры просмотра
    /// </summary>
    public BRDataViewSettingsDataItem View { get { return _List.GetRequired<BRDataViewSettingsDataItem>(); } }
  }


  /// <summary>
  /// Виртуальная таблица для табличного просмотра <see cref="EFPDataGridView"/>
  /// </summary>
  internal class BRDataGridViewTable : BRVirtualTable
  {
    #region Вложенные классы

    private struct ColumnInfo
    {
      #region Поля

      /// <summary>
      /// Индекс столбца
      /// </summary>
      public int ColumnIndex;

      /// <summary>
      /// Описатель столбца. 
      /// </summary>
      public EFPDataGridViewColumn EFPColumn;

      /// <summary>
      /// Ширина столбца в единицах 0.1мм
      /// </summary>
      public int Width;

      public bool AutoGrow;

      public bool Repeatable;

      #endregion

      #region Методы

      public override string ToString()
      {
        return EFPColumn.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Класс нужен для вызова базового конструктора <see cref="BRVirtualTable"/>, чтобы передать количество строк и столбцов
    /// </summary>
    private class InternalInfo
    {
      #region Конструктор

      /// <summary>
      /// Описатель печатаемого столбца
      /// В TreeViewAdv и EFPDataTreeView одному столбцу TreeColumn/EFPDataTreeViewColumn может соответствовать несколько элементов NodeControl.
      /// При печати они разделяются на отдельные столбцы.
      /// </summary>
      public InternalInfo(EFPDataGridView controlProvider, SettingsDataList settingsData, bool useExport)
      {
        if (controlProvider == null)
          throw new ArgumentNullException("controlProvider");
        if (settingsData == null)
          throw new ArgumentNullException("settingsData");
        _ControlProvider = controlProvider;
        _ViewData = settingsData.GetRequired<BRDataViewSettingsDataItem>();
        BRPageSetup pageSetup = settingsData.GetRequired<BRPageSettingsDataItem>().PageSetup;
        BRFontSettingsDataItem fontData = settingsData.GetRequired<BRFontSettingsDataItem>();

        InitRows(useExport);

        InitColumns(useExport, pageSetup, fontData);

        _FirstDataRow = _Headers.RowCount;

        _SubHeaderNumberRowIndex = -1;
        if (_ViewData.ColumnSubHeaderNumbers != BRDataViewColumnSubHeaderNumbersMode.None && _Headers2.RowCount == 1)
        {
          _SubHeaderNumberRowIndex = _FirstDataRow;
          _FirstDataRow++;
        }

        if (FirstDataRow == 0 && RowIndexes.Length == 0)
        {
          // Если нет ни одной строки, то возникнет исключение при создании BRBand.
          // Добавляем одну пустую строку "заголовка"
          _Headers = new BRColumnHeaderArray(1, Columns.Length);
          _FirstDataRow = 1;
        }
      }

      private void InitRows(bool useExport)
      {
        if (useExport && _ViewData.ExpRange == EFPDataViewExpRange.Selected)
        {
          // Выбранные узлы в просмотре
          _RowIndexes = _ControlProvider.SelectedRowIndices;
        }
        else
        {
          _RowIndexes = new int[_ControlProvider.Control.RowCount];
          for (int i = 0; i < _RowIndexes.Length; i++)
            _RowIndexes[i] = i;
        }
      }


      private void InitColumns(bool useExport, BRPageSetup pageSetup, BRFontSettingsDataItem fontData)
      {
        List<ColumnInfo> lst = new List<ColumnInfo>();

        List<string[]> headList = new List<string[]>();
        List<string[]> headList2 = new List<string[]>();

        int cntCol = 0;

        IEnumerable<EFPDataGridViewColumn> columns;
        if (useExport && _ViewData.ExpRange == EFPDataViewExpRange.Selected)
          columns = ControlProvider.SelectedColumns;
        else
          columns = ControlProvider.VisibleColumns;

        foreach (EFPDataGridViewColumn efpCol in columns)
        {
          if (!efpCol.GridColumn.Visible)
            continue;
          if (!efpCol.Printable)
            continue;

          if (!(useExport && _ViewData.ExpRange == EFPDataViewExpRange.Selected))
          {
            if (!_ViewData.GetColumnPrinted(efpCol))
              continue;
          }

          ColumnInfo ci = new ColumnInfo();
          ci.ColumnIndex = efpCol.GridColumn.Index;
          ci.EFPColumn = efpCol;
          ci.Width = _ViewData.GetRealColumnWidth(efpCol, fontData);
          ci.AutoGrow = _ViewData.GetColumnAutoGrow(efpCol);

          ci.Repeatable = cntCol < _ViewData.RepeatedColumnCount;

          lst.Add(ci);

          if (efpCol.PrintHeaders != null)
            headList.Add(efpCol.PrintHeaders);
          else if (!String.IsNullOrEmpty(efpCol.GridColumn.HeaderText))
            headList.Add(new string[1] { efpCol.GridColumn.HeaderText });
          else
            headList.Add(DataTools.EmptyStrings);
          headList2.Add(new string[1] { (cntCol + 1).ToString() });

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

      }

      #endregion

      #region Свойства

      public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
      private EFPDataGridView _ControlProvider;

      public BRDataViewSettingsDataItem ViewData { get { return _ViewData; } }
      private BRDataViewSettingsDataItem _ViewData;

      public int[] RowIndexes { get { return _RowIndexes; } }
      private int[] _RowIndexes;


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

    #endregion

    #region Конструктор

    public BRDataGridViewTable(BRSection section, EFPDataGridView controlProvider, SettingsDataList settingsData, bool useExport)
      : this(section, new InternalInfo(controlProvider, settingsData, useExport))
    {
    }

    private BRDataGridViewTable(BRSection section, InternalInfo info)
      : base(section, info.RowIndexes.Length + info.FirstDataRow, info.Columns.Length)
    {
      _Info = info;
      _SB = new StringBuilder();
    }

    private InternalInfo _Info;

    private StringBuilder _SB;

    #endregion

    #region Переход к строке/столбцу

    private EFPDataGridViewRowAttributesEventArgs _RowArgs;
    private EFPDataGridViewCellAttributesEventArgs _CellArgs;

    private void GotoCell(int rowIndex, int columnIndex)
    {
      rowIndex = _Info.RowIndexes[rowIndex - _Info.FirstDataRow];
      if (columnIndex >= 0)
        columnIndex = _Info.Columns[columnIndex].ColumnIndex;
      GotoCell2(rowIndex, columnIndex);
    }
    private void GotoCell2(int rowIndex, int columnIndex)
    {
      if (_RowArgs == null || rowIndex != _RowArgs.RowIndex)
      {
        _RowArgs = _Info.ControlProvider.DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.Print);
        _CellArgs = null;
      }
      if (columnIndex >= 0 && (_CellArgs == null || columnIndex != _CellArgs.ColumnIndex))
        _CellArgs = _Info.ControlProvider.DoGetCellAttributes(columnIndex);
    }

    #endregion

    #region Реализация методов

    protected override object GetValue(int rowIndex, int columnIndex)
    {
      if (rowIndex >= _Info.FirstDataRow)
      {
        GotoCell(rowIndex, columnIndex);
        return DoGetValue();
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

    private object DoGetValue()
    {
      if (!_CellArgs.ContentVisible)
        return null;

      object v = _CellArgs.Value;
      if (v is Boolean)
        v = _Info.ViewData.GetBoolValue((bool)v);
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

        GotoCell(rowIndex, columnIndex);

        if (_Info.ViewData.BorderStyle == BRDataViewBorderStyle.All)
        {
          style.TopBorder = GetBorder(_CellArgs.TopBorder);
          style.BottomBorder = GetBorder(_CellArgs.BottomBorder);
        }

        if (_Info.ViewData.BorderStyle == BRDataViewBorderStyle.All || _Info.ViewData.BorderStyle == BRDataViewBorderStyle.Vertical)
        {
          style.LeftBorder = GetBorder(_CellArgs.LeftBorder);
          style.RightBorder = GetBorder(_CellArgs.RightBorder);
        }

        if (_CellArgs.DiagonalUpBorder != UIDataViewBorderStyle.Default)
          style.DiagonalUp = GetBorder(_CellArgs.DiagonalUpBorder);
        if (_CellArgs.DiagonalDownBorder != UIDataViewBorderStyle.Default)
          style.DiagonalDown = GetBorder(_CellArgs.DiagonalDownBorder);


        if (_Info.ViewData.ColorStyle != BRDataViewColorStyle.NoColors)
        {
          EFPDataGridViewExcelCellAttributes excelAttrs = EFPDataGridView.GetExcelCellAttr(_CellArgs);
          if (_Info.ViewData.ColorStyle == BRDataViewColorStyle.Gray)
          {
            if (!excelAttrs.BackColor.IsEmpty)
            {
              int v3 = (excelAttrs.BackColor.R + excelAttrs.BackColor.G + excelAttrs.BackColor.B) / 3;
              style.BackColor = new BRColor(v3, v3, v3);
            }
          }
          else
          {
            style.ForeColor = FromColor(excelAttrs.ForeColor);
            style.BackColor = FromColor(excelAttrs.BackColor);
          }
          if (!(_CellArgs.Value is bool)) // CheckBox курсивом - это что-то
          {
            style.Bold = excelAttrs.Bold;
            style.Italic = excelAttrs.Italic;
            style.Underline = excelAttrs.Underline;
          }
        }

        if (_CellArgs.GridColumn is DataGridViewCheckBoxColumn)
        {
          style.HAlign = BRHAlign.Center;
          style.VAlign = BRVAlign.Center;
        }
        else
        {
          switch (_CellArgs.CellStyle.Alignment)
          {
            case DataGridViewContentAlignment.TopLeft: style.HAlign = BRHAlign.Left; style.VAlign = BRVAlign.Top; break;
            case DataGridViewContentAlignment.TopCenter: style.HAlign = BRHAlign.Center; style.VAlign = BRVAlign.Top; break;
            case DataGridViewContentAlignment.TopRight: style.HAlign = BRHAlign.Right; style.VAlign = BRVAlign.Top; break;
            case DataGridViewContentAlignment.MiddleLeft: style.HAlign = BRHAlign.Left; style.VAlign = BRVAlign.Center; break;
            case DataGridViewContentAlignment.MiddleCenter: style.HAlign = BRHAlign.Center; style.VAlign = BRVAlign.Center; break;
            case DataGridViewContentAlignment.MiddleRight: style.HAlign = BRHAlign.Right; style.VAlign = BRVAlign.Center; break;
            case DataGridViewContentAlignment.BottomLeft: style.HAlign = BRHAlign.Left; style.VAlign = BRVAlign.Bottom; break;
            case DataGridViewContentAlignment.BottomCenter: style.HAlign = BRHAlign.Center; style.VAlign = BRVAlign.Bottom; break;
            case DataGridViewContentAlignment.BottomRight: style.HAlign = BRHAlign.Right; style.VAlign = BRVAlign.Bottom; break;
            default: throw new BugException("Неизвестное выравнивание");
          }
          style.WrapMode = BRWrapMode.WordWrap;
        }
        style.IndentLevel = _CellArgs.IndentLevel;

        style.Format = _CellArgs.CellStyle.Format;
        style.FormatProvider = _CellArgs.CellStyle.FormatProvider;

        #endregion
      }
      else if (rowIndex < _Info.Headers.RowCount)
      {
        #region Заголовки

        style.HAlign = BRHAlign.Center;
        if (_Info.ViewData.BorderStyle != BRDataViewBorderStyle.NoBorders)
          style.AllBorders = BRLine.Thin;
        style.WrapMode = BRWrapMode.WordWrap;

        #endregion
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        #region Нумерация столбцов

        style.HAlign = BRHAlign.Center;
        if (_Info.ViewData.BorderStyle != BRDataViewBorderStyle.NoBorders)
          style.AllBorders = BRLine.Thin;

        #endregion
      }
      else
        throw new BugException();
    }

    private static BRLine GetBorder(UIDataViewBorderStyle borderStyle)
    {
      switch (borderStyle)
      {
        case UIDataViewBorderStyle.Default:
          return BRLine.Thin;
        case UIDataViewBorderStyle.Thin:
          return BRLine.Medium;
        case UIDataViewBorderStyle.Thick:
          return BRLine.Thick;
        default:
          return BRLine.None;
      }
    }

    private static BRColor FromColor(Color color)
    {
      if (color.IsEmpty)
        return BRColor.Auto;
      else
        return new BRColor(color.R, color.G, color.B);
    }

    protected override void FillColumnInfo(int columnIndex, BRColumnInfo columnInfo)
    {
      if (_Info.Columns[columnIndex].Width > 0)
        columnInfo.SetWidth(_Info.Columns[columnIndex].Width, _Info.Columns[columnIndex].AutoGrow);
      columnInfo.Repeatable = _Info.Columns[columnIndex].Repeatable;
    }

    protected override void FillRowInfo(int rowIndex, BRRowInfo rowInfo)
    {
      if (rowIndex < _Info.Headers.RowCount)
      {
        rowInfo.KeepWithNext = true;
        if (_Info.ViewData.ColumnSubHeaderNumbers != BRDataViewColumnSubHeaderNumbersMode.Replace)
          rowInfo.Repeatable = true;
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        rowInfo.KeepWithNext = true;
        rowInfo.Repeatable = true;
      }
      else
      {
        // Таблица данных
        if (rowIndex == (_Info.RowIndexes.Length - 1 + _Info.FirstDataRow))
          rowInfo.KeepWithPrev = true;
        GotoCell(rowIndex, -1);
        rowInfo.KeepWithNext = _RowArgs.PrintWithNext;
        rowInfo.KeepWithPrev = _RowArgs.PrintWithPrevious;
      }
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
