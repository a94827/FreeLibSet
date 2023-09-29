using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Drawing.Reporting;
using FreeLibSet.IO;
using FreeLibSet.Reporting;
using FreeLibSet.Shell;

#pragma warning disable 1591

namespace FreeLibSet.Forms.Reporting
{
  #region Перечисления

  /// <summary>
  /// Режим, для которого выводится диалог параметров
  /// </summary>
  public enum BROutAction
  {
    /// <summary>
    /// Диалог "Параметры страницы"
    /// </summary>
    Print,

    /// <summary>
    /// Выполняется команда "Экспорт в файл".
    /// Диалог показывается после выбора имени файла
    /// </summary>
    Export,

    /// <summary>
    /// Параметры команды "Файл" - "Отправить"
    /// </summary>
    SendTo,
  }

  /// <summary>
  /// Источник выполнения команды
  /// </summary>
  public enum BROutCommandSource
  {
    /// <summary>
    /// Выполняется команда для основного объекта <see cref="BRMenuOutItem"/>
    /// </summary>
    OutItem,

    /// <summary>
    /// Выполняется команда из диалога предварительного просмотра.
    /// В этом режиме команды экспорта в файл и отправки не будут создавать новый отчет, а будет использован отчет, который выведен в просмотре
    /// </summary>
    PrintPreview
  }

  /// <summary>
  /// Целевой тип выгрузки для диалога параметров
  /// </summary>
  public enum BROutDestination
  {
    /// <summary>
    /// Используется для режима <see cref="BROutAction.Print"/>
    /// </summary>
    Print,

    PDF,

    TIFF,

    Word,

    Excel,

    Writer,

    Calc,

    HTML
  }

  #endregion

  #region Делегаты

  public class BRMenuOutItemInitDialogEventArgs : EventArgs
  {
    internal BRMenuOutItemInitDialogEventArgs(SettingsDialog dialog)
    {
      _Dialog = dialog;
    }

    /// <summary>
    /// Заполняемый диалог. У него должны заполняться вкладки <see cref="SettingsDialog.Pages"/>.
    /// </summary>
    public SettingsDialog Dialog { get { return _Dialog; } }
    private SettingsDialog _Dialog;

    public BROutAction Action { get { return _Action; } internal set { _Action = value; } }
    private BROutAction _Action;

    public BROutDestination Destination { get { return _Destination; } internal set { _Destination = value; } }
    private BROutDestination _Destination;

    /// <summary>
    /// Выбранное пользователем имя файла при <see cref="Action"/>=<see cref="BROutAction.Export"/>.
    /// </summary>
    public AbsPath FilePath { get { return _FilePath; } internal set { _FilePath = value; } }
    private AbsPath _FilePath;

    public void AddFontPage()
    {
      new BRPageSetupFont(Dialog);
    }
  }

  public delegate void BRMenuOutItemInitDialogEventHandler(object sender, BRMenuOutItemInitDialogEventArgs args);

  public class BRMenuOutItemCreateReportEventArgs : EventArgs
  {
    public BRMenuOutItemCreateReportEventArgs()
    {
    }

    public BROutAction Action { get { return _Action; } internal set { _Action = value; } }
    private BROutAction _Action;

    public BROutDestination Destination { get { return _Destination; } internal set { _Destination = value; } }
    private BROutDestination _Destination;

    /// <summary>
    /// Выбранное пользователем имя файла при <see cref="Action"/>=<see cref="BROutAction.Export"/>.
    /// </summary>
    public AbsPath FilePath { get { return _FilePath; } internal set { _FilePath = value; } }
    private AbsPath _FilePath;

    public BRReport Report
    {
      get
      {
        if (_Report == null)
          _Report = new BRReport();
        return _Report;
      }
      set
      {
        _Report = value;
      }
    }
    private BRReport _Report;
  }

  public delegate void BRMenuOutItemCreateReportEventHandler(object sender, BRMenuOutItemCreateReportEventArgs args);

  #endregion

  public class BRMenuOutItem : EFPMenuOutItem
  {
    #region Конструктор

    public BRMenuOutItem(string code)
      : base(code)
    {
      _SettingsData = new SettingsDataList();
      _SettingsData.Add(new BRPageSetup());
      _SettingsData.Add(new BRBitmapSettings());
      _SettingsData.Add(new BRExportSettings());

      InitSendToItems(SendToItems, false);
    }

    private static void InitSendToItems(NamedList<EFPSendToItem> lst, bool isPreviewDlg)
    {
      EFPSendToItem item;
      if (EFPDataGridView.CanSendToMicrosoftExcel) // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!
      {
        item = new EFPSendToItem("Excel");
        item.MenuText = "Microsoft Excel";
        item.Image = EFPApp.MainImages.Images["MicrosoftExcel"];
        item.ToolTipText = "Оправить в " + MicrosoftOfficeTools.ExcelDisplayName;
        lst.Add(item);
      }
      if (BRReportODSWriter.IsSupported)
      {
        OpenOfficeInfo[] offices = OpenOfficeTools.GetPartInstallations(OpenOfficePart.Calc);
        for (int i = 0; i < offices.Length; i++)
        {

          OpenOfficeInfo info = offices[i];
          OpenOfficePartInfo calc = info.Parts[OpenOfficePart.Calc];
          item = new EFPSendToItem("Calc", StdConvert.ToString(i));
          item.MenuText = calc.DisplayName;
          item.Tag = info;
          FileAssociationItem fa = calc.FileAssociation;
          item.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
          item.ToolTipText = "Оправить в " + calc.DisplayName;
          item.SubMenuText = "Calc";
          lst.Add(item);
        }
      }

      // Отправку в PDF и HTML разрешаем только в диалоге предварительного просмотра, чтобы не было завала кнопок в табличном просмотре
      if (isPreviewDlg)
      {
        FileAssociations FAs;
        if (PdfFileTools.PdfLibAvailable)
        {
          FAs = EFPApp.FileExtAssociations[".pdf"];
          for (int i = 0; i < FAs.Count; i++)
          {
            FileAssociationItem fa = FAs[i];

            item = new EFPSendToItem("PDF", StdConvert.ToString(i + 1));
            item.MenuText = fa.DisplayName;
            item.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
            item.SubMenuText = "PDF";
            item.Tag = fa;
            lst.Add(item);
          }
        }

        FAs = EFPApp.FileExtAssociations[".html"];
        for (int i = 0; i < FAs.Count; i++)
        {
          FileAssociationItem fa = FAs[i];

          item = new EFPSendToItem("HTML", StdConvert.ToString(i + 1));
          item.MenuText = fa.DisplayName;
          item.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
          item.SubMenuText = "HTML";
          item.Tag = fa;
          lst.Add(item);
        }
      }
    }

    private static bool SendToCalcSupported
    {
      get
      {
        if (!BRReportODSWriter.IsSupported)
          return false;

        return false;
      }
    }

    #endregion

    #region Данные

    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName ?? String.Empty; }
      set { _ConfigSectionName = value; }
    }
    private string _ConfigSectionName;

    /// <summary>
    /// Список данных параметров страницы.
    /// Должен быть заполнен до вывода формы на экран.
    /// При первом показе диалога параметров странице, печати, экспорте вызывается <see cref="SettingsDataList.ReadConfig(CfgPart, SettingsPart)"/>,
    /// при условии, что свойство <see cref="ConfigSectionName"/> установлено.
    /// При повторных действиях чтение не выполняется.
    /// После каждого показа диалога, включая повторные, вызывается <see cref="SettingsDataList.WriteConfig(CfgPart, SettingsPart)"/>.
    /// </summary>
    public SettingsDataList SettingsData { get { return _SettingsData; } }
    private readonly SettingsDataList _SettingsData;

    private bool _ReadConfigCalled;

    private string UserConfigCategory { get { return "PageSetup"; } }

    private string MachineConfigCategory
    {
      get
      {
        if (EFPApp.ConfigManager.Persistence == EFPConfigPersistence.Network)
          return "MachinePageSetup";
        else
          return "PageSetup";
      }
    }

    private void CallReadConfig()
    {
      if (!_ReadConfigCalled)
      {
        string name = GetConfigSectionName();
        if (name.Length > 0)
        {
          SettingsPart usedParts = SettingsData.UsedParts;

          CfgPart cfg;
          if ((usedParts & SettingsPart.User) != 0)
          {
            using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, UserConfigCategory), EFPConfigMode.Read, out cfg))
            {
              SettingsData.ReadConfig(cfg, SettingsPart.User);
            }
          }
          if ((usedParts & SettingsPart.Machine) != 0)
          {
            using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, MachineConfigCategory), EFPConfigMode.Read, out cfg))
            {
              SettingsData.ReadConfig(cfg, SettingsPart.Machine);
            }
          }
          //if ((usedParts & SettingsPart.NoHistory) != 0)
          //{
          //  using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, NoHistoryCategory), EFPConfigMode.Read, out cfg))
          //  {
          //    Data.ReadConfig(cfg, SettingsPart.NoHistory);
          //  }
          //}
        }
        _ReadConfigCalled = true;
      }
    }

    private void CallWriteConfig()
    {
      string name = GetConfigSectionName();
      if (name.Length > 0)
      {
        SettingsPart usedParts = SettingsData.UsedParts;

        CfgPart cfg;
        if ((usedParts & SettingsPart.User) != 0)
        {
          using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, UserConfigCategory), EFPConfigMode.Write, out cfg))
          {
            SettingsData.WriteConfig(cfg, SettingsPart.User);
          }
        }
        if ((usedParts & SettingsPart.Machine) != 0)
        {
          using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, MachineConfigCategory), EFPConfigMode.Write, out cfg))
          {
            SettingsData.WriteConfig(cfg, SettingsPart.Machine);
          }
        }
        //if ((usedParts & SettingsPart.NoHistory) != 0)
        //{
        //  using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, NoHistoryCategory), EFPConfigMode.Read, out cfg))
        //  {
        //    Data.ReadConfig(cfg, SettingsPart.NoHistory);
        //  }
        //}
      }
    }

    private string GetConfigSectionName()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        return "Default";
      else
        return ConfigSectionName;
    }

    #endregion

    #region CreateReport Построение отчета

    public event BRMenuOutItemCreateReportEventHandler CreateReport;

    internal BRReport PerformCreateReport(BROutAction action, BROutDestination destination, AbsPath filePath)
    {
      BRMenuOutItemCreateReportEventArgs args;
      EFPApp.BeginWait("Создание отчета \"" + DisplayName + "\"");
      try
      {
        CallReadConfig();
        args = new BRMenuOutItemCreateReportEventArgs();
        args.Action = action;
        args.Destination = destination;
        args.FilePath = filePath;
        if (CreateReport != null)
          CreateReport(this, args);
      }
      finally
      {
        EFPApp.EndWait();
      }
      return args.Report;
    }

    #endregion

    #region InitDialog

    public event BRMenuOutItemInitDialogEventHandler InitDialog;

    internal bool ShowDialog(SettingsDialog dialog, BROutAction action, BROutDestination destination, AbsPath filePath, BROutCommandSource commandSource, bool forceWriteConfig)
    {
      CallReadConfig();

      dialog.ConfigSectionName = String.Empty; // Сами читаем параметры
      dialog.Data = this.SettingsData;
      if (action == BROutAction.Print)
      {
        if (dialog.Data.GetItem<BRPageSetup>() != null)
        {
          new BRPageSetupPaper(dialog);
          new BRPageSetupMargins(dialog);
        }
        // Должен делать вызывающий код
        //if (dialog.Data.GetItem<BRFontSettings>() != null)
        //  new BRPageSetupFont(dialog);
      }
      if (destination == BROutDestination.TIFF && dialog.Data.GetItem<BRBitmapSettings>() != null)
        new BRPageSetupBitmap(dialog);

      bool useEventHandler;
      if (commandSource == BROutCommandSource.OutItem)
        useEventHandler = true;
      else
        useEventHandler = (action == BROutAction.Print);

      if (useEventHandler && InitDialog != null)
      {
        BRMenuOutItemInitDialogEventArgs args = new BRMenuOutItemInitDialogEventArgs(dialog);
        args.Action = action;
        args.Destination = destination;
        args.FilePath = filePath;
        InitDialog(this, args);
      }

      // Если нет ни одной страницы, диалог не показываем, но считаем, что пользователь нажал ОК
      if (dialog.Pages.Count == 0)
      {
        if (action == BROutAction.Print)
          EFPApp.MessageBox("Нет параметров страницы", dialog.Title);

        if (forceWriteConfig)
          CallWriteConfig();
        return true;
      }

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        CallWriteConfig();
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Печать

    public override bool CanPrint { get { return true; } }

    public override bool Print(bool defaultPrinter)
    {
      BRReport report = PerformCreateReport(BROutAction.Print, BROutDestination.Print, AbsPath.Empty);
      BRPaginatorPageInfo[] pages;
      PrintDocument pd = FreeLibSet.Drawing.Reporting.BRReportPainter.CreatePrintDocument(report, out pages);

      if (!defaultPrinter)
      {
        PrintDialog dlg = new PrintDialog();
        dlg.Document = pd;
        //dlg.AllowCurrentPage = true;
        //dlg.AllowSelection = true;
        dlg.AllowSomePages = true;
        dlg.PrinterSettings.MinimumPage = 1;
        dlg.PrinterSettings.MaximumPage = pages.Length;
        dlg.PrinterSettings.FromPage = 1;
        dlg.PrinterSettings.ToPage = pages.Length;
        dlg.UseEXDialog = true; // Иначе не работает в Windows-7 64 bit

        if (dlg.ShowDialog() != DialogResult.OK)
          return false;
      }

      // TODO: Принтер по умолчанию из настроек
      try
      {
        if (EFPApp.BackgroundPrinting.Enabled /*&& AllowBackground ???*/ )
          EFPApp.BackgroundPrinting.Add(pd);
        else
        {
          EFPApp.BeginWait("Идет печать документа", "Print");
          try
          {
            pd.Print();
          }
          finally
          {
            EFPApp.EndWait();
          }
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка печати документа");
        return false;
      }
    }

    #endregion

    #region Параметры страницы

    public override bool CanPageSetup { get { return true; } }

    public override bool PageSetup()
    {
      CallReadConfig();
      SettingsDialog dlg = new SettingsDialog();
      dlg.Title = "Параметры страницы";
      dlg.ImageKey = "PageSetup";
      return ShowDialog(dlg, BROutAction.Print, BROutDestination.Print, AbsPath.Empty, BROutCommandSource.OutItem, false);
    }

    #endregion

    #region Предварительный просмотр

    public override bool CanPrintPreview { get { return true; } }

    /// <summary>
    /// Устанавливается в true на время вывода диалога предварительного просмотра
    /// </summary>
    private bool _InsidePreviewDialog;

    public override void PrintPreview()
    {
      BRReport report = PerformCreateReport(BROutAction.Print, BROutDestination.Print, AbsPath.Empty);
      _InsidePreviewDialog = true;
      try
      {
        Form previewForm = new Form();
        previewForm.Text = DisplayName;
        previewForm.Icon = EFPApp.MainImages.Icons["Preview"];
        previewForm.StartPosition = FormStartPosition.CenterScreen;
        EFPApp.SetFormSize(previewForm, 75, 75);
        previewForm.WindowState = FormWindowState.Maximized;

        EFPFormProvider efpPreviewForm = new EFPFormProvider(previewForm);
        efpPreviewForm.ConfigSectionName = "PrintPreview";

        EFPControlWithToolBar<ExtPrintPreviewControl> cwt = new EFPControlWithToolBar<ExtPrintPreviewControl>(efpPreviewForm, previewForm);
        EFPExtPrintPreviewControl efpPreview = new EFPExtPrintPreviewControl(cwt); 
        efpPreview.ConfigSectionName = efpPreviewForm.ConfigSectionName;
        efpPreview.Control.Document = FreeLibSet.Drawing.Reporting.BRReportPainter.CreatePrintDocument(report);

        efpPreview.CommandItems.OutHandler.Items.Add(new PreviewOutItem(this, efpPreviewForm, efpPreview)); 

        EFPApp.ShowDialog(previewForm, true);
      }
      finally
      {
        _InsidePreviewDialog = false;
      }
    }

    #endregion

    #region Команды печати в окне предварительного просмотра

    private class PreviewOutItem : EFPMenuOutItem
    {
      #region Конструктор

      public PreviewOutItem(BRMenuOutItem owner, EFPFormProvider efpPreviewForm, EFPExtPrintPreviewControl efpPreview)
        : base("Print")
      {
        _Owner = owner;
        _efpPreviewForm = efpPreviewForm;
        _efpPreview = efpPreview;

        BRMenuOutItem.InitSendToItems(SendToItems, true);
      }

      private BRMenuOutItem _Owner;
      EFPFormProvider _efpPreviewForm;
      EFPExtPrintPreviewControl _efpPreview;

      #endregion

      #region Печать

      public override bool CanPrint { get { return true; } }

      public override bool Print(bool defaultPrinter)
      {
        bool res = _Owner.Print(defaultPrinter);
        if (res)
          _efpPreviewForm.CloseForm(DialogResult.Cancel);
        return res;
      }

      #endregion

      #region Параметры страницы

      public override bool CanPageSetup { get { return true; } }

      public override bool PageSetup()
      {
        if (!_Owner.PageSetup())
          return false;
        BRReport report = _Owner.PerformCreateReport(BROutAction.Print, BROutDestination.Print, AbsPath.Empty);
        _efpPreview.Control.Document = FreeLibSet.Drawing.Reporting.BRReportPainter.CreatePrintDocument(report);
        _efpPreview.Control.InvalidatePreview();
        _efpPreview.Control.StartPage = 0;
        return true;
      }

      #endregion

      #region Сохранение в файл

      public override bool CanExport { get { return true; } }

      public override void Export()
      {
        _Owner.DoExport(BROutCommandSource.PrintPreview);
      }

      #endregion

      #region Отправить

      public override void SendTo(EFPSendToItem item)
      {
        _Owner.SendTo(item);
      }

      #endregion
    }

    #endregion

    #region Сохранение в файл

    public override bool CanExport { get { return true; } }

    public override void Export()
    {
      DoExport(BROutCommandSource.OutItem);
    }
    internal void DoExport(BROutCommandSource commandSource)
    {
      List<string> fileCodes = new List<string>();
      List<string> filters = new List<string>();
      if (PdfFileTools.PdfLibAvailable)
      {
        fileCodes.Add("PDF");
        filters.Add("Файлы PDF|*.pdf");
      }
      fileCodes.Add("TIFF");
      filters.Add("Файлы TIFF|*.tif");
      fileCodes.Add("HTML");
      filters.Add("Файлы HTML|*.html");
      fileCodes.Add("ExcelXML");
      filters.Add("Microsoft Excel 2003 XML|*.xml");
      if (BRReportODSWriter.IsSupported)
      {
        fileCodes.Add("ODS");
        filters.Add("OpenOffice/LibreOffice Calc (ODS)|*.ods");
      }

      BRExportSettings exportSettings = SettingsData.GetRequired<BRExportSettings>();

      CallReadConfig();

      SaveFileDialog dlg1 = new SaveFileDialog();
      dlg1.Filter = String.Join("|", filters.ToArray());
      dlg1.FilterIndex = Array.IndexOf<string>(fileCodes.ToArray(), exportSettings.FileType) + 1; // FilterIndex нумеруется с 1
      if (!exportSettings.ExportDir.IsEmpty)
        dlg1.InitialDirectory = exportSettings.ExportDir.Path;
      if (EFPApp.ShowDialog(dlg1) != DialogResult.OK)
        return;

      BROutDestination destination;
      string fileCode = fileCodes[dlg1.FilterIndex - 1];
      switch (fileCode)
      {
        case "PDF": destination = BROutDestination.PDF; break;
        case "TIFF": destination = BROutDestination.TIFF; break;
        case "HTML": destination = BROutDestination.HTML; break;
        case "ExcelXML": destination = BROutDestination.Excel; break;
        case "ODS": destination = BROutDestination.Calc; break;
        default: throw new BugException("fileCode=" + fileCode);
      }

      exportSettings.FileType = fileCodes[dlg1.FilterIndex - 1];

      AbsPath filePath = new AbsPath(dlg1.FileName);
      exportSettings.ExportDir = filePath.ParentDir;

      BRReport report;
      if (commandSource == BROutCommandSource.PrintPreview)
        report = PerformCreateReport(BROutAction.Print, BROutDestination.Print, AbsPath.Empty);
      else
        report = PerformCreateReport(BROutAction.SendTo, destination, AbsPath.Empty);

      SettingsDialog dlg2 = new SettingsDialog();
      dlg2.Title = "Экспорт в " + filePath.FileName;
      dlg2.ImageKey = "Save";
      if (!ShowDialog(dlg2, BROutAction.Export, destination, filePath, commandSource, true))
        return;


      EFPApp.BeginWait("Сохранение файла " + filePath.FileName);
      try
      {
        FileTools.ForceDirs(filePath.ParentDir);

        switch (destination)
        {
          case BROutDestination.PDF:
            BRPdfReportPainter.CreateFile(report, filePath);
            break;
          case BROutDestination.TIFF:
            BRReportPainter.CreateTIFFFile(report, filePath, _SettingsData.GetRequired<BRBitmapSettings>(), null);
            break;
          case BROutDestination.HTML:
            BRReportHtmlWriter htmlWriter = new BRReportHtmlWriter();
            htmlWriter.CreateFile(report, filePath);
            break;
          case BROutDestination.Excel:
            switch (fileCode)
            {
              case "ExcelXML":
                BRReportExcel2003XmlWriter excel2003Writer = new BRReportExcel2003XmlWriter();
                excel2003Writer.CreateFile(report, filePath);
                break;
              default:
                throw new BugException();
            }
            break;
          case BROutDestination.Calc:
            BRReportODSWriter odsWriter = new BRReportODSWriter();
            odsWriter.CreateFile(report, filePath);
            break;
          default:
            throw new BugException("destination=" + destination.ToString());
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion

    #region Отправить

    /// <summary>
    /// Если true, то при передаче в Word/Excel будет использоваться OLE для создания документов. Это медленный способ.
    /// Если false (по умолчанию), то создается временный файл, а OLE используется только для открытия документа.
    /// Свойство используется в отладочных целях.
    /// Если установлен старый MS Office 2000, который не может читать файлы в формате XML, то OLE используется независимо от этой настройки.
    /// </summary>
    public static bool OLEPreferred
    {
      get { return _OLEPreferred; }
      set { _OLEPreferred = value; }
    }
    private static bool _OLEPreferred = false;


    public override void SendTo(EFPSendToItem item)
    {
      BROutDestination destination;
      switch (item.MainCode)
      {
        case "Excel": destination = BROutDestination.Excel; break;
        case "Calc": destination = BROutDestination.Calc; break;
        case "HTML": destination = BROutDestination.HTML; break;
        case "PDF": destination = BROutDestination.PDF; break;
        default:
          throw new BugException("MainCode=" + item.MainCode);
      }

      BROutCommandSource commandSource = BROutCommandSource.OutItem;
      if (_InsidePreviewDialog)
        commandSource = BROutCommandSource.PrintPreview;

      SettingsDialog dlg = new SettingsDialog();
      dlg.Title = "Отправить в " + item.MenuText;
      dlg.Image = item.Image;
      if (!ShowDialog(dlg, BROutAction.SendTo, destination, AbsPath.Empty, commandSource, false))
        return;

      BRReport report;
      if (commandSource == BROutCommandSource.PrintPreview)
        report = PerformCreateReport(BROutAction.Print, BROutDestination.Print, AbsPath.Empty);
      else
        report = PerformCreateReport(BROutAction.SendTo, destination, AbsPath.Empty);

      switch (destination)
      {
        case BROutDestination.Excel:
          DoSendToExcel(report);
          break;
        case BROutDestination.Calc:
          using (Splash spl = new Splash("Создание книги в Excel"))
          {
            AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("ods");
            BRReportODSWriter creator = new BRReportODSWriter();
            //creator.Splash = spl;
            creator.CreateFile(report, filePath);

            OpenOfficeInfo officeInfo = (OpenOfficeInfo)(item.Tag);
            officeInfo.Parts[OpenOfficePart.Calc].OpenFile(filePath, true);
          }
          break;
        case BROutDestination.HTML:
          using (Splash spl = new Splash("Создание HTML-файла"))
          {
            AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("html");
            BRReportHtmlWriter writer = new BRReportHtmlWriter();
            //writer.Splash = spl;
            writer.CreateFile(report, filePath);
            BRExportSettings exportSettings = SettingsData.GetRequired<BRExportSettings>();
            FileAssociationItem fa = (FileAssociationItem)(item.Tag);
            fa.Execute(filePath);
          }
          break;
        case BROutDestination.PDF:
          using (Splash spl = new Splash("Создание PDF-файла"))
          {
            AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("pdf");
            BRPdfReportPainter.CreateFile(report, filePath);
            FileAssociationItem fa = (FileAssociationItem)(item.Tag);
            fa.Execute(filePath);
          }
          break;
        default:
          throw new NotImplementedException();
      }
    }

    private void DoSendToExcel(BRReport report)
    {
      if (!OLEPreferred)
      {
        if (EFPApp.MicrosoftExcelVersion.Major >= MicrosoftOfficeTools.MicrosoftOffice_XP)
        {
          using (Splash spl = new Splash(new string[] {
          "Создание файла",
          "Запуск Microsoft Excel"}))
          {
            BRReportExcel2003XmlWriter writer = new BRReportExcel2003XmlWriter();
            AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("xml");
            writer.CreateFile(report, filePath);
            spl.Complete();
            MicrosoftOfficeTools.OpenWithExcel(filePath, true);
            spl.Complete();
          }
          return;
        }
      }

      using (Splash spl = new Splash("Создание книги в Excel"))
      {
        BRReportOLEExcelSender creator = new BRReportOLEExcelSender();
        creator.Splash = spl;
        creator.Send(report);
      }
    }

    #endregion
  }

  /// <summary>
  /// Диалог просмотра отчета <see cref="BRReport"/>
  /// </summary>
  public sealed class BRPrintPreviewDialog
  {
    #region Конструктор

    public BRPrintPreviewDialog()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - просматриваемый отчет
    /// </summary>
    public BRReport Report { get { return _Report; } set { _Report = value; } }
    private BRReport _Report;

    public string ConfigSectionName
    {
      get { return _ConfigSectionName; }
      set { _ConfigSectionName = value; }
    }
    private string _ConfigSectionName;

    #endregion

    #region Показ диалога

    public void ShowDialog()
    {
      if (Report == null)
        EFPApp.ErrorMessageBox("Отчет не присоединен");

      BRMenuOutItem outItem = new BRMenuOutItem("1");
      if (!String.IsNullOrEmpty(Report.DocumentProperties.Title))
        outItem.DisplayName = Report.DocumentProperties.Title;
      else
        outItem.DisplayName = "Отчет";
      outItem.CreateReport += OutItem_CreateReport;
      outItem.ConfigSectionName = ConfigSectionName;
      outItem.PrintPreview();
    }

    private void OutItem_CreateReport(object sender, BRMenuOutItemCreateReportEventArgs args)
    {
      args.Report = this.Report;
    }

    #endregion
  }

  internal class BRExportSettings : ISettingsDataItem
  {
    #region Сохранение в файл

    /// <summary>
    /// Тип файла при экспорте ("PDF", "TIFF", ...)
    /// </summary>
    public string FileType;

    /// <summary>
    /// Каталог для экспорта
    /// </summary>
    public AbsPath ExportDir;

    #endregion

    #region ISettingsDataItem

    public SettingsPart UsedParts { get { return SettingsPart.User | SettingsPart.Machine; } }

    public void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      switch (part)
      {
        case SettingsPart.User:
          cfg.SetString("FileType", FileType);
          break;
        case SettingsPart.Machine:
          cfg.SetString("ExportDir", ExportDir.Path);
          break;
      }
    }

    public void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      switch (part)
      {
        case SettingsPart.User:
          FileType = cfg.GetString("FileType");
          break;
        case SettingsPart.Machine:
          ExportDir = new AbsPath(cfg.GetString("ExportDir"));
          break;
      }
    }

    #endregion
  }
}
