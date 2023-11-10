using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Core;

#pragma warning disable 0414

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Провайдер элемента предварительного просмотра
  /// </summary>
  public class EFPExtPrintPreviewControl : EFPControl<ExtPrintPreviewControl>
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPExtPrintPreviewControl(EFPBaseProvider baseProvider, ExtPrintPreviewControl control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер для управляющего элемента и панели инструментов
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    public EFPExtPrintPreviewControl(EFPControlWithToolBar<ExtPrintPreviewControl> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      Init();
    }

    private void Init()
    {
      Control.Rows = 1;
      Control.Columns = 1;
      Control.AutoZoom = false;
      Control.Zoom = 1.0;
      Control.DocumentChanged += Control_DocumentChanged;
      _PageCount = -1;

      base.InitConfigHandler();
    }


    #endregion

    #region Обработчики документа

    private void Control_DocumentChanged(object sender, EventArgs args)
    {
      RemovePrevDocumentHandlers();
      _PageCount = -1;
      _RealPageCount = 0;
      if (Control.Document != null)
      {
        Control.Document.BeginPrint += Document_BeginPrint;
        Control.Document.PrintPage += Document_PrintPage;
        Control.Document.EndPrint += Document_EndPrint;
      }
      _PrevDocument = Control.Document;
    }

    /// <summary>
    /// Отсоединяет обработчики событий объекта <see cref="PrintDocument"/>.
    /// </summary>
    protected override void OnDisposed()
    {
      RemovePrevDocumentHandlers();
      base.OnDisposed();
    }

    private void RemovePrevDocumentHandlers()
    {
      if (_PrevDocument != null)
      {
        _PrevDocument.PrintPage -= Document_PrintPage;
        _PrevDocument = null;
      }
    }

    private PrintDocument _PrevDocument;


    private int _InternalPageCounter;
    private void Document_BeginPrint(object sender, PrintEventArgs args)
    {
      _InternalPageCounter = 0;
    }

    private void Document_PrintPage(object sender, PrintPageEventArgs args)
    {
      if (IsShowGrid && ((PrintDocument)sender).PrintController.IsPreview)
        DrawGrid(args.Graphics, args.PageSettings.PaperSize, args.PageSettings.Landscape);
      _InternalPageCounter++;
    }

    private void Document_EndPrint(object sender, PrintEventArgs args)
    {
      _RealPageCount = _InternalPageCounter;
      CommandItems.InitEnabled();
    }

    #endregion

    #region Количество страниц

    /// <summary>
    /// Количество страниц в документе.
    /// Может быть задано из внешнего кода после установки свойства <see cref="ExtPrintPreviewControl.Document"/> или определено автоматически
    /// </summary>
    public int PageCount
    {
      get
      {
        if (_PageCount < 0)
          return _RealPageCount;
        else
          return _PageCount;
      }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _PageCount = value;
        CommandItems.InitEnabled();
      }
    }
    private int _PageCount;

    private int _RealPageCount;

    #endregion

    #region Сохранение настроек

    /// <summary>
    /// 
    /// </summary>
    /// <param name="categories"></param>
    /// <param name="rwMode"></param>
    /// <param name="actionInfo"></param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);

      categories.Add(EFPConfigCategories.PageSetup);
    }

    /// <summary>
    /// Сохраняет масштаб и количество отображаемых страниц
    /// </summary>
    /// <param name="category"></param>
    /// <param name="cfg"></param>
    /// <param name="actionInfo"></param>
    public override void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.WriteConfigPart(category, cfg, actionInfo);
      if (category == EFPConfigCategories.PageSetup)
      {
        cfg.SetInt("PreviewPageRows", Control.Rows);
        cfg.SetInt("PreviewPageColumns", Control.Columns);
        if (Control.AutoZoom)
          cfg.SetInt("PreviewZoom", 0);
        else
          cfg.SetInt("PreviewZoom", (int)(Control.Zoom * 100));
      }
    }

    /// <summary>
    /// Восстанавливает масштаб и количество отображаемых страниц
    /// </summary>
    /// <param name="category"></param>
    /// <param name="cfg"></param>
    /// <param name="actionInfo"></param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.ReadConfigPart(category, cfg, actionInfo);

      try
      {
        Control.Rows = cfg.GetIntDef("PreviewPageRows", 1);
        Control.Columns = cfg.GetIntDef("PreviewPageColumns", 1);
        int zoom = cfg.GetIntDef("PreviewZoom", 100);
        if (zoom == 0)
          Control.AutoZoom = true;
        else
        {
          Control.AutoZoom = false;
          Control.Zoom = zoom / 100.0;
        }
      }
      catch { }
    }

    /// <summary>
    /// Вызывается при изменении масштаба или количества отображаемых страниц
    /// </summary>
    internal protected void OnZoomChanged()
    {
      ConfigHandler.Changed[EFPConfigCategories.PageSetup] = true;
    }

    #endregion

    #region Локальное меню

    internal bool IsFullScreen;

    /// <summary>
    /// Создает новый объект <see cref="EFPExtPrintPreviewControlCommandItems"/>
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPExtPrintPreviewControlCommandItems(this, IsFullScreen);
    }

    /// <summary>
    /// Возвращает команды локального меню
    /// </summary>
    public new EFPExtPrintPreviewControlCommandItems CommandItems
    {
      get { return (EFPExtPrintPreviewControlCommandItems)(base.CommandItems); }
    }

    /// <summary>
    /// Если true, то будет прорисована координатная сетка
    /// </summary>
    public bool IsShowGrid { get { return _IsShowGrid; } set { _IsShowGrid = value; } }
    private bool _IsShowGrid;

    #endregion

    #region Печать сетки

    /// <summary>
    /// Рисование сетки
    /// </summary>
    /// <param name="graphics">Контекст вывода графики</param>
    /// <param name="paperSize">Размер бумаги</param>
    /// <param name="isLandscape"></param>
    public static void DrawGrid(Graphics graphics, PaperSize paperSize, bool isLandscape)
    {

      Color color = Color.FromArgb(196, 196, 64);
      Pen pen = new Pen(color);
      pen.Width = 0;
      Font font = new Font("Arial", 10);
      Brush brush = new SolidBrush(color);
      try
      {
        GraphicsUnit oldPU = graphics.PageUnit;
        graphics.PageUnit = GraphicsUnit.Millimeter;
        try
        {
          // Размеры бумаги даны в единицах 1/1000 дюйма
          // Переводим их в миллиметры
          float paperWidth = paperSize.Width / 100f * 25.4f;
          float paperHeight = paperSize.Height / 100f * 25.4f;
          if (isLandscape)
          {
            float tmp = paperWidth;
            paperWidth = paperHeight;
            paperHeight = tmp;
          }

          // Вертикальные линейки
          for (float x = 0f; x < paperWidth; x += 10f)
          {
            graphics.DrawLine(pen, x, 0f, x, paperHeight);

            string txt = (x / 10f).ToString();

            SizeF sz = graphics.MeasureString(txt, font);

            graphics.DrawString(txt, font, brush,
              x - (sz.Width / 2), 6 - (sz.Height / 2));
            graphics.DrawString(txt, font, brush,
              x - (sz.Width / 2), paperHeight - 6 - (sz.Height / 2));

          }
          // Горизонтальные линейки
          for (float y = 0f; y < paperHeight; y += 10f)
          {
            graphics.DrawLine(pen, 0f, y, paperWidth, y);

            string txt = (y / 10f).ToString();

            SizeF sz = graphics.MeasureString(txt, font);


            graphics.DrawString(txt, font, brush,
              6 - (sz.Width / 2), y - (sz.Height / 2));
            graphics.DrawString(txt, font, brush,
              paperWidth - 6 - (sz.Width / 2), y - (sz.Height / 2));
          }
        }
        finally
        {
          graphics.PageUnit = oldPU;
        }
      }
      finally
      {
        pen.Dispose();
        font.Dispose();
        brush.Dispose();
      }
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для <see cref="EFPExtPrintPreviewControl"/>
  /// </summary>
  public class EFPExtPrintPreviewControlCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Этот класс, кроме основного режима использования в <see cref="EFPExtPrintPreviewControl"/>,
    /// используется внутри себя при переключении просмотра в полноэкранный режим
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="isFullScreen">true при запуске в полноэкранном режиме</param>
    public EFPExtPrintPreviewControlCommandItems(EFPExtPrintPreviewControl controlProvider, bool isFullScreen)
      : base(controlProvider)
    {
      _IsFullScreen = isFullScreen;

      _OutHandler = new EFPMenuOutHandler(this);

      DelayedPrint = false;

      ciFirstPage = new EFPCommandItem("View", "FirstPage");
      ciFirstPage.MenuText = "Первая страница";
      ciFirstPage.ShortCut = Keys.Control | Keys.PageUp;
      ciFirstPage.Click += new EventHandler(FirstPageClick);
      ciFirstPage.GroupBegin = true;
      ciFirstPage.ImageKey = "FirstLeft";
      Add(ciFirstPage);

      ciPrevPage = new EFPCommandItem("View", "PreviousPage");
      ciPrevPage.MenuText = "Предыдущая страница";
      //ciPrevPage.ShortCut = Keys.PageUp;
      ciPrevPage.MenuRightText = EFPCommandItem.GetShortCutText(Keys.PageUp);
      ciPrevPage.Click += new EventHandler(PrevPageClick);
      ciPrevPage.ImageKey = "Left";
      Add(ciPrevPage);

      //ciGotoPage = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Goto);
      ciGotoPage = new EFPCommandItem("View", "GotoPage");
      ciGotoPage.MenuText = "Перейти к странице ...";
      ciGotoPage.Click += new EventHandler(GotoPageClick);
      ciGotoPage.StatusBarText = "Страница ??? из ???";
      ciGotoPage.ToolTipText = "Отображаемая страница в просмотре";
      Add(ciGotoPage);

      ciNextPage = new EFPCommandItem("View", "NextPage");
      ciNextPage.MenuText = "Следующая страница";
      //ciNextPage.ShortCut = Keys.PageDown;
      ciNextPage.MenuRightText = EFPCommandItem.GetShortCutText(Keys.PageDown);
      ciNextPage.Click += new EventHandler(NextPageClick);
      ciNextPage.ImageKey = "Right";
      Add(ciNextPage);

      ciLastPage = new EFPCommandItem("View", "LastPage");
      ciLastPage.MenuText = "Последняя страница";
      ciLastPage.ShortCut = Keys.Control | Keys.PageDown;
      ciLastPage.Click += new EventHandler(LastPageClick);
      ciLastPage.ImageKey = "LastRight";
      ciLastPage.GroupEnd = true;
      Add(ciLastPage);

      ciFullScreen = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FullScreen);
      ciFullScreen.Click += new EventHandler(FullScreenClick);
      ciFullScreen.GroupBegin = true;
      ciFullScreen.GroupEnd = true;
      ciFullScreen.Enabled = true;
      ciFullScreen.Checked = isFullScreen;
      Add(ciFullScreen);

      ciScale = new EFPCommandItem("View", "ScaleMenu");
      ciScale.MenuText = "Масштаб";
      ciScale.ImageKey = "Scale";
      ciScale.ToolTipText = "Масштаб";
      ciScale.GroupBegin = true;
      ciScale.GroupEnd = true;
      Add(ciScale);

      ciSetScale = new EFPCommandItem("View", "Scale");
      ciSetScale.Parent = ciScale;
      ciSetScale.MenuText = "Установить масштаб ...";
      ciSetScale.Click += new EventHandler(SetScaleClick);
      ciSetScale.ImageKey = "Scale";
      ciSetScale.StatusBarText = "Масштаб ??? %";
      ciSetScale.ToolTipText = "Масштаб просмотра изображения";
      ciSetScale.GroupBegin = true;
      ciSetScale.GroupEnd = true;
      Add(ciSetScale);

      ciScale50 = new EFPCommandItem("View", "Scale50");
      ciScale50.Parent = ciScale;
      ciScale50.MenuText = "Масштаб 50 %";
      ciScale50.ImageKey = "Scale50";
      ciScale50.GroupBegin = true;
      ciScale50.ToolTipText = "Половинный размер";
      ciScale50.Click += new EventHandler(Scale50Click);
      Add(ciScale50);

      ciScale100 = new EFPCommandItem("View", "Scale100");
      ciScale100.Parent = ciScale;
      ciScale100.MenuText = "Масштаб 100 %";
      ciScale100.ImageKey = "Scale100";
      ciScale100.ToolTipText = "Один к одному";
      ciScale100.Click += new EventHandler(Scale100Click);
      Add(ciScale100);

      ciScale200 = new EFPCommandItem("View", "Scale200");
      ciScale200.Parent = ciScale;
      ciScale200.MenuText = "Масштаб 200 %";
      ciScale200.ImageKey = "Scale200";
      ciScale200.ToolTipText = "Двукратное увеличение";
      ciScale200.Click += new EventHandler(Scale200Click);
      Add(ciScale200);

      ciScale500 = new EFPCommandItem("View", "Scale500");
      ciScale500.Parent = ciScale;
      ciScale500.MenuText = "Масштаб 500 %";
      ciScale500.ImageKey = "Scale500";
      ciScale500.GroupEnd = true;
      ciScale500.ToolTipText = "Пятикратное увеличение";
      ciScale500.Click += new EventHandler(Scale500Click);
      Add(ciScale500);

      controlProvider.Control.WheelZoom += WheelZoomHandler;

      ciZoomIn = new EFPCommandItem("View", "ZoomIn");
      ciZoomIn.Parent = ciScale;
      ciZoomIn.MenuText = "Увеличить изображение";
      ciZoomIn.ImageKey = "ZoomIn";
      ciZoomIn.GroupBegin = true;
      ciZoomIn.ToolTipText = "Увеличить";
      ciZoomIn.ShortCut = Keys.Control | Keys.I;
      ciZoomIn.Click += new EventHandler(ZoomInClick);
      Add(ciZoomIn);

      ciZoomOut = new EFPCommandItem("View", "ZoomOut");
      ciZoomOut.Parent = ciScale;
      ciZoomOut.MenuText = "Уменьшить изображение";
      ciZoomOut.ImageKey = "ZoomOut";
      ciZoomOut.GroupEnd = true;
      ciZoomOut.ToolTipText = "Уменьшить";
      ciZoomOut.ShortCut = Keys.Control | Keys.U;
      ciZoomOut.Click += new EventHandler(ZoomOutClick);
      Add(ciZoomOut);

      ciOnePage = new EFPCommandItem("View", "OnePage");
      ciOnePage.Parent = ciScale;
      ciOnePage.MenuText = "Страница целиком";
      ciOnePage.ImageKey = "OnePage";
      ciOnePage.GroupBegin = true;
      ciOnePage.ToolTipText = "Одна страница";
      ciOnePage.Click += new EventHandler(OnePageClick);
      Add(ciOnePage);

      ciTwoPages = new EFPCommandItem("View", "TwoPages");
      ciTwoPages.Parent = ciScale;
      ciTwoPages.MenuText = "Две страницы рядом";
      ciTwoPages.ImageKey = "TwoPages";
      ciTwoPages.ToolTipText = "Две страницы";
      ciTwoPages.Click += new EventHandler(TwoPagesClick);
      Add(ciTwoPages);

      ciAllPages = new EFPCommandItem("View", "AllPages");
      ciAllPages.Parent = ciScale;
      ciAllPages.MenuText = "Все страницы";
      ciAllPages.ImageKey = "FourPages";
      ciAllPages.GroupEnd = true;
      ciAllPages.ToolTipText = "Все страницы";
      ciAllPages.Click += new EventHandler(AllPagesClick);
      Add(ciAllPages);

      ciShowGrid = new EFPCommandItem("View", "ShowGrid");
      ciShowGrid.MenuText = "Координатная сетка";
      ciShowGrid.ImageKey = "ShowGrid";
      ciShowGrid.GroupBegin = true;
      ciShowGrid.GroupEnd = true;
      ciShowGrid.ToolTipText = "Сетка";
      ciShowGrid.Click += new EventHandler(ShowGridClick);
      Add(ciShowGrid);

      //FSaveTypes = new AccDepFileTypes();
      //ciSave = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Save);
      //ciSave.Click += new EventHandler(Save_Click);
      //ciSave.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      //Add(ciSave);

      //ciSaveAs = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SaveAs);
      //ciSaveAs.MenuText = null;
      //ciSaveAs.Click += new EventHandler(SaveAs_Click);
      //ciSaveAs.Usage = EFPCommandItemUsage.Menu;
      //Add(ciSaveAs);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Click += new EventHandler(Copy_Click);
      Add(ciCopy);

    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Подготовка списка команд
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      ControlProvider.Control.Resize += new EventHandler(ControlResize);
      ControlProvider.Control.StartPageChanged += new EventHandler(ControlPageChanged);

      //if (SaveTypes.Count > 0 || SpecialSave != null)
      //  ciSave.Usage |= EFPCommandItemUsage.ToolBar;

      if (Copy == null)
        ciCopy.Usage = EFPCommandItemUsage.None;

      InitEnabled();
    }

    /// <summary>
    /// Возвращает провайдер управляющего элемента
    /// </summary>
    public new EFPExtPrintPreviewControl ControlProvider { get { return (EFPExtPrintPreviewControl)(base.ControlProvider); } }

    ///// <summary>
    ///// Если установлено значение, отличное от null, то устанавливается заголовок
    ///// формы, в которой расположен управляющий элемент просмотра. К тексту свойства
    ///// добавляется номер текущей страницы и масштаб
    ///// </summary>
    //public string FormTitle;

    #endregion

    #region Команды изменения масштаба

    private EFPCommandItem ciScale, ciSetScale,
      ciScale50, ciScale100, ciScale200, ciScale500,
      ciZoomIn, ciZoomOut, ciOnePage, ciTwoPages, ciAllPages;

    private void SetScaleClick(object sender, EventArgs args)
    {
      int prc = (int)(Math.Round(ControlProvider.Control.Zoom * 100));
      IntInputDialog dlg = new IntInputDialog();
      dlg.Title = "Масштаб";
      dlg.Value = prc;
      dlg.Prompt = "Введите масштаб в процентах";
      dlg.Minimum = 10;
      dlg.Maximum = 1000;
      if (dlg.ShowDialog() == DialogResult.OK)
        SetScale((double)prc / 100);
    }

    void Scale50Click(object sender, EventArgs args)
    {
      SetScale(0.5);
    }
    void Scale100Click(object sender, EventArgs args)
    {
      SetScale(1.0);
    }
    void Scale200Click(object sender, EventArgs args)
    {
      SetScale(2.0);
    }
    void Scale500Click(object sender, EventArgs args)
    {
      SetScale(5.0);
    }

    void ZoomInClick(object sender, EventArgs args)
    {
      SetScale(ControlProvider.Control.Zoom * 1.2);
    }

    void ZoomOutClick(object sender, EventArgs args)
    {
      SetScale(ControlProvider.Control.Zoom / 1.2);
    }

    /// <summary>
    /// Обработчик для события WheelZoom
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void WheelZoomHandler(object sender, ZoomDirectionEventArgs args)
    {
      if (args.Direction == ZoomDirection.ZoomIn)
        ZoomInClick(null, null);
      else
        ZoomOutClick(null, null);
    }

    private void SetScale(double v)
    {
      if (v < 0.1) v = 0.1;
      if (v > 10.0) v = 10.0;

      ControlProvider.Control.Rows = 1;
      ControlProvider.Control.Columns = 1;
      ControlProvider.Control.AutoZoom = false;
      ControlProvider.Control.Zoom = v;
      ControlProvider.OnZoomChanged();
      InitEnabled();
    }

    void OnePageClick(object sender, EventArgs args)
    {
      SetPages(1, 1);
    }

    void TwoPagesClick(object sender, EventArgs args)
    {
      SetPages(1, 2);
    }

    void AllPagesClick(object sender, EventArgs args)
    {
      int n = (int)(Math.Ceiling(Math.Sqrt((double)(ControlProvider.PageCount))));
      int m = (int)(Math.Ceiling((double)(ControlProvider.PageCount) / (double)n));
      SetPages(n, m);

      ControlProvider.Control.StartPage = 0;
    }

    private void SetPages(int n, int m)
    {
      ControlProvider.Control.Rows = n;
      ControlProvider.Control.Columns = m;
      ControlProvider.Control.AutoZoom = true;
      ControlProvider.OnZoomChanged();
      InitEnabled();
    }

    #endregion

    #region Команды переключения страниц

    private EFPCommandItem ciFirstPage, ciPrevPage, ciNextPage, ciLastPage;
    private EFPCommandItem ciGotoPage;

    private void FirstPageClick(object sender, EventArgs args)
    {
      if (ControlProvider.PageCount == 0)
      {
        EFPApp.ShowTempMessage("Нет документа");
        return;
      }
      ControlProvider.Control.StartPage = 0;
    }

    private void PrevPageClick(object sender, EventArgs args)
    {
      if (ControlProvider.PageCount == 0)
      {
        EFPApp.ShowTempMessage("Нет документа");
        return;
      }
      int p = ControlProvider.Control.StartPage - ControlProvider.Control.Rows * ControlProvider.Control.Columns;
      if (p < 0)
        p = 0;
      ControlProvider.Control.StartPage = p;
    }

    private void NextPageClick(object sender, EventArgs args)
    {
      if (ControlProvider.PageCount == 0)
      {
        EFPApp.ShowTempMessage("Нет документа");
        return;
      }
      int p = ControlProvider.Control.StartPage + ControlProvider.Control.Rows * ControlProvider.Control.Columns;
      ControlProvider.Control.StartPage = p;
    }

    private void LastPageClick(object sender, EventArgs args)
    {
      if (ControlProvider.PageCount == 0)
      {
        EFPApp.ShowTempMessage("Нет документа");
        return;
      }
      int p = ControlProvider.PageCount - 1;
      ControlProvider.Control.StartPage = p;
    }

    private void GotoPageClick(object sender, EventArgs args)
    {
      if (ControlProvider.PageCount == 0)
      {
        EFPApp.ShowTempMessage("Нет документа");
        return;
      }
      int p = ControlProvider.Control.StartPage + 1;
      IntInputDialog dlg = new IntInputDialog();
      dlg.Title = "Перейти к странице";
      dlg.Prompt = "Введите номер страницы";
      dlg.Minimum = 1;
      dlg.Maximum = ControlProvider.PageCount;
      dlg.Increment = 1;
      dlg.Value = p;
      if (dlg.ShowDialog() == DialogResult.OK)
        ControlProvider.Control.StartPage = p - 1;
    }

    void ControlPageChanged(object sender, EventArgs args)
    {
      InitEnabled();
    }

    #endregion

    #region Команда печати

    /// <summary>
    /// Сюда может быть добавлен объект, реализующий команды печати и параметров страницы.
    /// Также могут быть реализованы команды экспорта в файл и "Отправить"
    /// </summary>
    public EFPMenuOutHandler OutHandler { get { return _OutHandler; } }
    private readonly EFPMenuOutHandler _OutHandler;

    /// <summary>
    /// true для полноэкранного режима, чтобы после выхода из него
    /// запустить печать
    /// </summary>
    private bool DelayedPrint;



    #endregion

    #region Команды буфера обмена

    private EFPCommandItem ciCopy;

    /// <summary>
    /// Если установлен обработчик, то доступна команда "Копировать"
    /// </summary>
    public event EventHandler Copy;

    private void Copy_Click(object sender, EventArgs args)
    {
      Copy(this, EventArgs.Empty);
    }

    #endregion

    #region Команда переключения полноэкранного режима

    private EFPCommandItem ciFullScreen;

    /// <summary>
    /// true, если текущий просмотр находится в полноэкранном режиме
    /// </summary>
    private bool _IsFullScreen;


    void FullScreenClick(object sender, EventArgs args)
    {
      // Переключение полноэкранного режима
      if (_IsFullScreen)
      {
        // Закрываем текущую форму
        ControlProvider.Control.FindForm().Close();
      }
      else
      {
        // Создаем новую форму, в которой будет просмотр
        Form frm = new Form();
        frm.FormBorderStyle = FormBorderStyle.None;
        frm.Location = new Point(0, 0);
        frm.Size = Screen.FromControl(ControlProvider.Control).Bounds.Size;
        frm.ShowInTaskbar = false;
        EFPFormProvider efpForm = new EFPFormProvider(frm);

        // Управляющий элемент 
        ExtPrintPreviewControl newControl = new ExtPrintPreviewControl();
        newControl.Dock = DockStyle.Fill;
        newControl.Document = ControlProvider.Control.Document;
        newControl.Zoom = ControlProvider.Control.Zoom;
        newControl.AutoZoom = ControlProvider.Control.AutoZoom;
        newControl.Rows = ControlProvider.Control.Rows;
        newControl.Columns = ControlProvider.Control.Columns;
        newControl.StartPage = ControlProvider.Control.StartPage;
        frm.Controls.Add(newControl);

        EFPExtPrintPreviewControl efpNewControl = new EFPExtPrintPreviewControl(efpForm, newControl);
        efpNewControl.IsFullScreen = true;
        efpNewControl.ConfigSectionName = ControlProvider.ConfigSectionName;
        //newControl.WheelZoom += new ZoomDirectionEventHandler(NewItems.WheelZoomHandler);
        // NewItems.PageCount = PageCount;
        // efpNewControl.CommandItems = NewItems;

        // Закрытие формы по <Esc>
        FormButtonStub.AssignCancel(frm);


        // Выводим форму
        EFPApp.ShowDialog(frm, true);

        // TODO:
        //if (NewItems.DelayedPrint)
        //{
        //  // В полноэкранном режиме была нажата кнопка печати
        //  FOwner.PageSetup.Print(true);
        //}
      }
    }

    #endregion

    #region Команда масштабной сетки

    private EFPCommandItem ciShowGrid;


    void ShowGridClick(object sender, EventArgs e)
    {
      ControlProvider.IsShowGrid = !ControlProvider.IsShowGrid;
      ControlProvider.Control.InvalidatePreview();
      InitEnabled();
    }

    #endregion

#if XXX
    #region Команда "Файл-Сохранить"

    /// <summary>
    /// Если присоединить обработчик к этому событию, то он будет вызываться при
    /// выполнении команд "Сохранить" и "Сохранить как". Он может выполнить 
    /// собственные действия и установить свойство Cancel, чтобы не выполнять 
    /// стандартные действия по сохранению данных таблицы
    /// </summary>
    public event AccDepPrintPreviewSpecialSaveEventHandler SpecialSave;

    /// <summary>
    /// Форматы для сохранения данных предварительного просмотра
    /// </summary>
    public AccDepFileTypes SaveTypes { get { return FSaveTypes; } }
    private AccDepFileTypes FSaveTypes;

    private EFPCommandItem ciSave, ciSaveAs;

    void Save_Click(object Sender, EventArgs Args)
    {
      DoSave(Sender, false);
    }
    void SaveAs_Click(object Sender, EventArgs Args)
    {
      DoSave(Sender, true);
    }
    void DoSave(object Sender, bool SaveAs)
    {
      if (FOwner.PageSetup == null)
      {
        EFPApp.ShowTempMessage("Нет присоединенного документа");
        return;
      }

      if (SpecialSave != null)
      {
        AccDepPrintPreviewSpecialSaveEventArgs SpecArgs = new AccDepPrintPreviewSpecialSaveEventArgs(FOwner, (EFPCommandItem)Sender, SaveAs);
        SpecialSave(this, SpecArgs);
        if (SpecArgs.Cancel)
          return;
      }

      SaveTypes.PerformSave(Owner.PageSetup.ConfigSectionName);
    }

    #endregion

    #region Команды SendTo

    protected EFPCommandItem MenuSendTo;

    /// <summary>
    /// Добавление всех возможных команд в Send To
    /// </summary>
    internal void AddSendTo(bool ToPaperDocEditor)
    {
      if (MenuSendTo != null)
        return;
      MenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
      MenuSendTo.GroupBegin = true;
      MenuSendTo.GroupEnd = true;
      Add(MenuSendTo);

      if (ToPaperDocEditor)
        AddSendTo(AccDepMainMenu.SendToPaperDoc);
      if (AccDepPaperDocPageSetupBase.CanSendToMicrosoftWord)
        AddSendTo(EFPApp.CommandItems[EFPAppStdCommandItems.SendToMicrosoftWord]);
      if (AccDepPaperDocPageSetupBase.CanSendToMicrosoftExcel)
        AddSendTo(EFPApp.CommandItems[EFPAppStdCommandItems.SendToMicrosoftExcel]);
      if (AccDepPaperDocPageSetupBase.CanSendToOpenOfficeWriter)
        AddSendTo(EFPApp.CommandItems[EFPAppStdCommandItems.SendToOpenOfficeWriter]);
      if (AccDepPaperDocPageSetupBase.CanSendToOpenOfficeCalc)
        AddSendTo(EFPApp.CommandItems[EFPAppStdCommandItems.SendToOpenOfficeCalc]);
      if (AccDepPaperDocPageSetupBase.CanSendToPdf)
        AddSendTo(EFPApp.CommandItems["ФайлОтправить", "PDF"]);
    }

    private void AddSendTo(EFPCommandItem MainMenuItem)
    {

      EFPCommandItem ci = new EFPCommandItem(MainMenuItem);
      ci.Parent = MenuSendTo;
      ci.Tag = MainMenuItem.Name;
      ci.Click += new EventHandler(SendTo_Click);
      Add(ci);
    }

    private void SendTo_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      string Name = (string)(ci.Tag);
      if (Owner.PageSetup == null)
      {
        EFPApp.MessageBox("Нет просматриваемого документа", ci.DisplayName);
        return;
      }
      Owner.PageSetup.PerformSendTo(Name);
    }

    #endregion

#endif

    #region Прочие методы

    /// <summary>
    /// Установка состояния всех команд меню
    /// </summary>
    public void InitEnabled()
    {
      if (ControlProvider.Control.Document == null)
      {
        ciFirstPage.Enabled = false;
        ciPrevPage.Enabled = false;
        ciNextPage.Enabled = false;
        ciLastPage.Enabled = false;
        _OutHandler.Enabled = false;
        return;
      }
      _OutHandler.Enabled = true;


      ciFirstPage.Enabled = (ControlProvider.Control.StartPage > 0);
      ciPrevPage.Enabled = (ControlProvider.Control.StartPage > 0);
      ciNextPage.Enabled = ControlProvider.Control.StartPage < (ControlProvider.PageCount - 1);
      ciLastPage.Enabled = ControlProvider.Control.StartPage < (ControlProvider.PageCount - 1);

      ciGotoPage.Enabled = ControlProvider.PageCount > 1;
      int firstPage = ControlProvider.Control.StartPage + 1;
      int lastPage = firstPage + ControlProvider.Control.Rows * ControlProvider.Control.Columns - 1;
      if (lastPage > ControlProvider.PageCount)
        lastPage = ControlProvider.PageCount;

      string pageStatusText;
      if (ControlProvider.PageCount == 0)
        pageStatusText = "Нет страниц";
      else
      {
        if (lastPage == firstPage)
          pageStatusText = "Страница " + firstPage.ToString() + " из " + ControlProvider.PageCount.ToString();
        else
          pageStatusText = "Страницы " + firstPage.ToString() + "-" + lastPage.ToString() + " из " + ControlProvider.PageCount.ToString();
      }
      ciGotoPage.StatusBarText = pageStatusText;
      ciGotoPage.MenuRightText = (ControlProvider.Control.StartPage + 1).ToString();

      int prc = (int)(Math.Round(ControlProvider.Control.Zoom * 100.0));

      ciSetScale.MenuRightText = prc.ToString() + " %";
      string ScaleStatusText = "Масштаб " + prc.ToString() + " %";
      ciSetScale.StatusBarText = ScaleStatusText;

      bool isPrc = !ControlProvider.Control.AutoZoom; // В Mono свойство Zoom остается установленным при AutoZoom=true. Если не проверять, то будут одновременно помечены и кнопки масштаба, и количества страниц

      ciScale50.Checked = isPrc && (prc == 50);
      ciScale100.Checked = isPrc && (prc == 100);
      ciScale200.Checked = isPrc && (prc == 200);
      ciScale500.Checked = isPrc && (prc == 500);

      ciTwoPages.Enabled = ControlProvider.PageCount > 1;
      ciAllPages.Enabled = ControlProvider.PageCount > 1;

      ciOnePage.Checked = ControlProvider.Control.AutoZoom && ControlProvider.Control.Rows == 1 && ControlProvider.Control.Columns == 1;
      ciTwoPages.Checked = ControlProvider.Control.AutoZoom && ControlProvider.Control.Rows == 1 && ControlProvider.Control.Columns == 2;
      ciAllPages.Checked = ControlProvider.Control.AutoZoom &&
        (ControlProvider.Control.Rows * ControlProvider.Control.Columns) >= ControlProvider.PageCount && ControlProvider.PageCount > 2;

      ciShowGrid.Checked = ControlProvider.IsShowGrid;

      //if (FormTitle != null)
      //{
      //  Owner.InternalSetFormText(Control, FormTitle + " (" + PageStatusText + ", " + ScaleStatusText + ")");
      //}
    }

    /// <summary>
    /// Обработчик изменения размеров просмотра. При этом может меняться
    /// масштаб, если Control.AutoZoom установлено в true.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ControlResize(object sender, EventArgs args)
    {
      // Может измениться масштаб изображения
      InitEnabled();
    }

    #endregion

  }
}
