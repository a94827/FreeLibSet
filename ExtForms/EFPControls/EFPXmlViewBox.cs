// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using FreeLibSet.IO;
using FreeLibSet.DependedValues;
using System.ComponentModel;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Shell;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер управляющего элемента для просмотра HTML-страниц.
  /// Для просмотра XML-документов используйте EFPXmlViewBox.
  /// </summary>
  public class EFPWebBrowser : EFPControl<WebBrowser>
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPWebBrowser(EFPBaseProvider baseProvider, WebBrowser control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPWebBrowser(IEFPControlWithToolBar<WebBrowser> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      Init();
    }

    private void Init()
    {
      if (!DesignMode)
        Control.PreviewKeyDown += new PreviewKeyDownEventHandler(Control_PreviewKeyDown);
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Создает объект EFPWebBrowserCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPWebBrowserCommandItems(this);
    }

    #endregion

    #region Обработчики управляющего элемента

    void Control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs args)
    {
      try
      {
        //  EFPApp.CommandItems.PerformShortCut(Args.KeyData); // иначе не работают стандартные команды меню 
        // 19.10.2018
        // Проверяем всю цепочку команд, включая локальное меню.
        // Иначе, например, Ctrl-P не работает, если в главном меню нет команды "Печать".
        EFPCommandItems.PerformKeyDown(args.KeyData);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента XmlViewBox для просмотра XML-документов
  /// </summary>
  public class EFPXmlViewBox : EFPWebBrowser
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPXmlViewBox(EFPBaseProvider baseProvider, XmlViewBox control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
        control.XmlDataChanged += new EventHandler(Control_XmlDataChanged);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPXmlViewBox(IEFPControlWithToolBar<XmlViewBox> controlWithToolBar)
      : base(controlWithToolBar.BaseProvider, controlWithToolBar.Control)
    {
      if (!DesignMode)
        Control.XmlDataChanged += new EventHandler(Control_XmlDataChanged);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Возвращает управляющий элемент XmlViewBox.
    /// </summary>
    public new XmlViewBox Control
    {
      get { return (XmlViewBox)(base.Control); }
    }

    #endregion

    #region XmlBytes

    /// <summary>
    /// Дублирует XmlViewBox.XmlBytes
    /// </summary>
    public byte[] XmlBytes
    {
      get { return Control.XmlBytes; }
      set { Control.XmlBytes = value; }
    }

    /// <summary>
    /// Управляемое свойство для XmlBytes.
    /// </summary>
    public DepValue<byte[]> XmlBytesEx
    {
      get
      {
        InitXmlBytesEx();
        return _XmlBytesEx;
      }
      set
      {
        InitXmlBytesEx();
        _XmlBytesEx.Source = value;
      }
    }

    private void InitXmlBytesEx()
    {
      if (_XmlBytesEx == null)
      {
        _XmlBytesEx = new DepInput<byte[]>(XmlBytes,XmlBytesEx_ValueChanged);
        _XmlBytesEx.OwnerInfo = new DepOwnerInfo(this, "XmlBytesEx");
      }
    }
    private DepInput<byte[]> _XmlBytesEx;

    private void XmlBytesEx_ValueChanged(object sender, EventArgs args)
    {
      XmlBytes = _XmlBytesEx.Value;
    }

    /// <summary>
    /// Вызывается при изменении XML-данных непосредственно в XmlViewBox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_XmlDataChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideXmlDataChanged)
        {
          _InsideXmlDataChanged = true;
          try
          {
            _TempFilePath = AbsPath.Empty; // сброс временного файла

            if (_XmlBytesEx != null)
              _XmlBytesEx.OwnerSetValue(Control.XmlBytes);
            if (_XmlDocumentEx != null)
              _XmlDocumentEx.OwnerSetValue(Control.XmlDocument);
            if (_XmlTextEx != null)
              _XmlTextEx.OwnerSetValue(Control.XmlText);

            Validate();
          }
          finally
          {
            _InsideXmlDataChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private bool _InsideXmlDataChanged;

    #endregion

    #region XmlDocument

    /// <summary>
    /// Доступ XmlViewBox.XmlDocument
    /// </summary>
    public XmlDocument XmlDocument
    {
      get { return Control.XmlDocument; }
      set { Control.XmlDocument = value; }
    }

    /// <summary>
    /// Свойство, разрешающее примение состояния CheckState.Grayed
    /// По умолчанию - false (кнопка на два положения)
    /// </summary>
    public DepValue<XmlDocument> XmlDocumentEx
    {
      get
      {
        InitXmlDocumentEx();
        return _XmlDocumentEx;
      }
      set
      {
        InitXmlDocumentEx();
        _XmlDocumentEx.Source = value;
      }
    }

    private void InitXmlDocumentEx()
    {
      if (_XmlDocumentEx == null)
      {
        _XmlDocumentEx = new DepInput<XmlDocument>(XmlDocument,XmlDocumentEx_ValueChanged);
        _XmlDocumentEx.OwnerInfo = new DepOwnerInfo(this, "XmlDocumentEx");
      }
    }
    private DepInput<XmlDocument> _XmlDocumentEx;

    private void XmlDocumentEx_ValueChanged(object sender, EventArgs args)
    {
      XmlDocument = _XmlDocumentEx.Value;
    }

    #endregion

    #region XmlText

    /// <summary>
    /// Дублирует XmlViewBox.XmlText
    /// </summary>
    public string XmlText
    {
      get { return Control.XmlText; }
      set { Control.XmlText = value; }
    }

    /// <summary>
    /// Управляемое свойство для XmlText
    /// </summary>
    public DepValue<string> XmlTextEx
    {
      get
      {
        InitXmlTextEx();
        return _XmlTextEx;
      }
      set
      {
        InitXmlTextEx();
        _XmlTextEx.Source = value;
      }
    }

    private void InitXmlTextEx()
    {
      if (_XmlTextEx == null)
      {
        _XmlTextEx = new DepInput<string>(XmlText,XmlTextEx_ValueChanged);
        _XmlTextEx.OwnerInfo = new DepOwnerInfo(this, "XmlTextEx");
      }
    }
    private DepInput<string> _XmlTextEx;

    private void XmlTextEx_ValueChanged(object sender, EventArgs args)
    {
      XmlText = _XmlTextEx.Value;
    }

    #endregion

    #region Получение XML-документа в виде файла

    #region Свойство FileName

    /// <summary>
    /// Имя файла без пути
    /// Свойство может использоваться, если XMl-данные установлены с помощью свойств XmlDocument или XmlBytes, но требуется, чтобы при экспорте
    /// в блокнот показывалось осмысленное имя файла.
    /// Если свойство не было установлено в явном виде и не было установлено свойство XmlViewBox.XmlFilePath,
    /// то возвращается константа DefaultFileName. Таким образом, всегда возвращается непустая строка.
    /// </summary>
    public string FileName
    {
      get
      {                           
        if (String.IsNullOrEmpty(_FileName))
        {
          if (Control.SourceKind == XmlViewBoxSourceKind.File)
            return Path.GetFileName(Control.XmlFilePath);
          else
            return DefaultFileName;
        }
        else
          return _FileName;
      }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
          value.IndexOf(Path.AltDirectorySeparatorChar) >= 0 ||
          value.IndexOf(Path.VolumeSeparatorChar) >= 0)

          throw new ArgumentException(Res.EFPXmlViewBox_Arg_FileNameWithPath);

        _FileName = value;
        _TempFilePath = AbsPath.Empty; // сброс временного файла
      }
    }
    private string _FileName;

    /// <summary>
    /// Имя XML-файла по умолчанию.
    /// Используется, если свойство FileName не было установлено в явном виде
    /// </summary>
    public const string DefaultFileName = "nonamed.xml";

    /// <summary>
    /// Свойство возвращает true, если свойство FileName не было установлено в явном виде, или оно совпадает с именем файла, заданным в 
    /// XmlViewBox.XmlFileParh
    /// </summary>
    public bool IsDefaultFileName
    {
      get
      {
        if (String.IsNullOrEmpty(_FileName))
          return true;

        if (Control.SourceKind == XmlViewBoxSourceKind.File)
        {
          AbsPath path1 = new AbsPath(Control.XmlFilePath);
          AbsPath path2 = path1.ParentDir + _FileName;
          return path1 == path2; // с учетом регистра
        }

        return false;
      }
    }

    #endregion

    #region GetFilePath()

    /// <summary>
    /// Если при вызове GetFilePath() был создан временный файл, сохраняем путь к нему, чтобы не создавать еще один ненужный файл
    /// </summary>
    private AbsPath _TempFilePath;

    /// <summary>
    /// Возвращает путь к файлу.
    /// Если XML-данные были установлены с помощью свойства XmlFilePath, то будет возвращен путь к оригинальному файлу
    /// Если данные были установлены с помощью XmlDocument, XmlBytes или XmlText, то будет возвращен файл ао временном каталоге.
    /// Также временный файл используется, если свойство FileName задает имя файла, отличное от заданного в XmlFilePath
    /// Если XML-данные не были установлены, генерируется исключение InvalidOperationException
    /// </summary>
    /// <returns>Полный путь к файлу</returns>
    public AbsPath GetFilePath()
    {
      if (!_TempFilePath.IsEmpty)
        return _TempFilePath;


      if (Control.SourceKind == XmlViewBoxSourceKind.None)
        throw new InvalidOperationException(Res.EFPXmlViewBox_Err_Empty);
      if (Control.SourceKind == XmlViewBoxSourceKind.File && IsDefaultFileName)
        return new AbsPath(Control.XmlFilePath);

      AbsPath res = EFPApp.SharedTempDir.GetFixedTempFileName(FileName); // используем временную переменную Res на случай исключения

      switch (Control.SourceKind)
      {
        case XmlViewBoxSourceKind.Bytes:
#if DEBUG
          if (Control.XmlBytes == null)
            throw new BugException("Control.XmlBytes==null");
#endif
          File.WriteAllBytes(res.Path, Control.XmlBytes);
          break;

        case XmlViewBoxSourceKind.XmlDocument:
#if DEBUG
          if (Control.XmlDocument == null)
            throw new BugException("Control.XmlDocument==null");
#endif
          XmlTools.WriteXmlDocument(res, Control.XmlDocument);
          break;

        default:
          throw ExceptionFactory.ObjectProperty(Control, "SourceKind", Control.SourceKind, null);
      }

      _TempFilePath = res; // запоминаем для повторного использования
      return _TempFilePath;
    }

    #endregion

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Создает объект EFPXmlViewBoxCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPXmlViewBoxCommandItems(this);
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для EFPWebBrowser.
  /// Добавляет в просмотр HTML-докумементов команды печати и некоторые другие команды, реализуемые WebBrowser'ом,
  /// команды "Открыть" / "Открыть с помощью" и команды отравки в Word/Writer.
  /// </summary>
  public class EFPWebBrowserCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает объект. Этот конструктор используется исключительно в EFPWebBrowser
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    internal EFPWebBrowserCommandItems(EFPWebBrowser owner)
      :this(owner, ".html")
    { 
    }

    /// <summary>
    /// Создает конструктор
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="fileExt">Расширение файла, например, ".html" или ".xml"</param>
    protected EFPWebBrowserCommandItems(EFPWebBrowser controlProvider, string fileExt)
      :base(controlProvider)
    {
      ciPrint = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Print);
      ciPrint.Click += new EventHandler(ciPrint_Click);
      ciPrint.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      ciPrint.GroupBegin = true;
      Add(ciPrint);

      ciPrintDefault = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PrintDefault);
      ciPrintDefault.Click += new EventHandler(ciPrintDefault_Click);
      Add(ciPrintDefault);

      ciPageSetup = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PageSetup);
      ciPageSetup.Click += new EventHandler(ciPageSetup_Click);
      Add(ciPageSetup);

      ciPreview = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PrintPreview);
      ciPreview.Click += new EventHandler(ciPreview_Click);
      ciPreview.GroupEnd = true;
      Add(ciPreview);

      _FileAssociationsHandler = new EFPFileAssociationsCommandItemsHandler(this, fileExt);
      _FileAssociationsHandler.FileNeeded += new System.ComponentModel.CancelEventHandler(FileAssociationsHandler_FileNeeded);


      MenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
      MenuSendTo.GroupEnd = true;
      Add(MenuSendTo);

      ciSendToMicrosoftWord = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToMicrosoftWord);
      ciSendToMicrosoftWord.Parent = MenuSendTo;
      ciSendToMicrosoftWord.Click += new EventHandler(SendToMicrosoftWord);
      Add(ciSendToMicrosoftWord);

      ciSendToOpenOfficeWriter = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToOpenOfficeWriter);
      ciSendToOpenOfficeWriter.Parent = MenuSendTo;
      ciSendToOpenOfficeWriter.Click += new EventHandler(SendToOpenOfficeWriter);
      Add(ciSendToOpenOfficeWriter);
      AddSeparator();

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Click += new EventHandler(ciCopy_Click);
      ciCopy.GroupBegin = true;
      Add(ciCopy);

      ciSelectAll = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SelectAll);
      ciSelectAll.Click += new EventHandler(ciSelectAll_Click);
      ciSelectAll.GroupEnd = true;
      Add(ciSelectAll);

      ciFullScreen = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FullScreen);
      ciFullScreen.GroupBegin = true;
      ciFullScreen.GroupEnd = true;
      ciFullScreen.Enabled = true;
      ciFullScreen.Click += new EventHandler(ciFullScreen_Click);
      Add(ciFullScreen);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPWebBrowser ControlProvider { get { return (EFPWebBrowser)(base.ControlProvider); } }

    #endregion

    #region Печать

    private readonly EFPCommandItem ciPrint, ciPrintDefault, ciPreview, ciPageSetup;

    void ciPrint_Click(object sender, EventArgs args)
    {
      ControlProvider.Control.ShowPrintDialog();
    }

    void ciPrintDefault_Click(object sender, EventArgs args)
    {
      ControlProvider.Control.Print();
    }

    void ciPreview_Click(object sender, EventArgs args)
    {
      ControlProvider.Control.ShowPrintPreviewDialog();
    }

    void ciPageSetup_Click(object sender, EventArgs args)
    {
      ControlProvider.Control.ShowPageSetupDialog();
    }

    #endregion

    #region "Открыть" и "Открыть с помощью"

    /// <summary>
    /// Обработчик команд "Открыть" и "Открыть с помощью"
    /// </summary>
    protected EFPFileAssociationsCommandItemsHandler FileAssociationsHandler { get { return _FileAssociationsHandler; } }
    private readonly EFPFileAssociationsCommandItemsHandler _FileAssociationsHandler;

    /// <summary>
    /// Получение файла
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected virtual void FileAssociationsHandler_FileNeeded(object sender, CancelEventArgs args)
    {
      // для HTML-файла каждый раз создаем новую копию

      if (ControlProvider.Control.DocumentStream == null)
      {
        EFPApp.ShowTempMessage(Res.EFPXmlViewBox_Err_NoDoc);
        args.Cancel = true;
        return;
      }

      FileAssociationsHandler.FilePath = EFPApp.SharedTempDir.GetTempFileName("HTML");
      FileTools.WriteStream(FileAssociationsHandler.FilePath, ControlProvider.Control.DocumentStream);
    }

    #endregion

    #region Файл - отправить

    /// <summary>
    /// Подменю "Отправить"
    /// </summary>
    protected EFPCommandItem MenuSendTo;

    private readonly EFPCommandItem ciSendToMicrosoftWord, ciSendToOpenOfficeWriter;

    private string CreateTempFile()
    {
      if (ControlProvider.Control.DocumentStream == null)
      {
        EFPApp.ShowTempMessage(Res.EFPXmlViewBox_Err_NoDoc);
        return null;
      }
      AbsPath tempFilePath = EFPApp.SharedTempDir.GetTempFileName("HTML");
      FileTools.WriteStream(tempFilePath, ControlProvider.Control.DocumentStream);
      return tempFilePath.Path;
    }

    /// <summary>
    /// Обработчик команды "Отправить"-"Microsoft Word"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    protected virtual void SendToMicrosoftWord(object sender, EventArgs args)
    {
      string fileName = CreateTempFile();
      if (fileName != null)
      {
        /*
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = MicrosoftOfficeTools.WordPath.Path;
        psi.Arguments = "\"" + FileName + "\"";
        try
        {
          Process.Start(psi);
        }
        catch (Exception e)
        {
          MessageBox.Show("Не удалось запустить Microsoft Word. " + e.Message);
        }
         * */

        try
        {
          MicrosoftOfficeTools.OpenWithWord(new AbsPath(fileName), true);
        }
        catch (Exception e)
        {
          EFPApp.ErrorMessageBox(String.Format(Res.EFPApp_Err_OpenWith,
            "Microsoft Word",
            e.Message));
        }
      }
    }

    /// <summary>
    /// Обработчик команды "Отправить"-"Writer"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    protected virtual void SendToOpenOfficeWriter(object sender, EventArgs args)
    {
      string fileName = CreateTempFile();
      if (fileName != null)
      {
        try
        {
          EFPApp.UsedOpenOffice.Parts[OpenOfficePart.Writer].OpenFile(new AbsPath(fileName), true);
        }
        catch (Exception e)
        {
          EFPApp.ErrorMessageBox(String.Format(Res.EFPApp_Err_OpenWith,
            EFPApp.OpenOfficeKindName + " Writer. ",
            e.Message));
        }
      }
    }

    #endregion

    #region Правка

    private readonly EFPCommandItem ciCopy, ciSelectAll;

    void ciCopy_Click(object sender, EventArgs args)
    {
      ControlProvider.Control.Document.ExecCommand("copy", false, null);
    }

    void ciSelectAll_Click(object sender, EventArgs args)
    {
      ControlProvider.Control.Document.ExecCommand("selectall", false, null);
    }

    #endregion

    #region Полноэкранный режим

    /// <summary>
    /// Признак просмотра в полноэкранном режиме
    /// </summary>
    internal bool IsFullScreen
    {
      get { return ciFullScreen.Checked; }
      set { ciFullScreen.Checked = value; }
    }

    private readonly EFPCommandItem ciFullScreen;

    void ciFullScreen_Click(object sender, EventArgs args)
    {
      // Переключение полноэкранного режима
      if (IsFullScreen)
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

        EFPWebBrowser efpNewControl;
        // Управляющий элемент 
        if (ControlProvider.Control is XmlViewBox)
        {
          XmlViewBox newControl = new XmlViewBox();
          newControl.Dock = DockStyle.Fill;
          frm.Controls.Add(newControl);
          newControl.XmlBytes = ((XmlViewBox)(ControlProvider.Control)).XmlBytes;

          efpNewControl = new EFPXmlViewBox(efpForm, newControl);
        }
        else
        {
          WebBrowser newControl = new WebBrowser();
          newControl.Dock = DockStyle.Fill;
          newControl.AllowNavigation = false;
          newControl.AllowWebBrowserDrop = false;
          newControl.WebBrowserShortcutsEnabled = false;
          newControl.IsWebBrowserContextMenuEnabled = false;
          frm.Controls.Add(newControl);
          newControl.DocumentStream = ControlProvider.Control.DocumentStream;

          efpNewControl = new EFPWebBrowser(efpForm, newControl);
        }
        ((EFPWebBrowserCommandItems)(efpNewControl.CommandItems)).IsFullScreen = true;


        // Закрытие формы по <Esc>
        FormButtonStub.AssignCancel(frm);

        // Выводим форму
        EFPApp.ShowDialog(frm, true);
      }
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для просмотра XML-документов.
  /// Новые команды, по сравнению с EFPWebBrowserCommandItems, не добавляются
  /// </summary>
  public class EFPXmlViewBoxCommandItems : EFPWebBrowserCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает команды
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPXmlViewBoxCommandItems(EFPXmlViewBox controlProvider)
      : base(controlProvider, ".xml")
    {
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Возвращает провайдер управляющего элемента
    /// </summary>
    public new EFPXmlViewBox ControlProvider { get { return (EFPXmlViewBox)(base.ControlProvider); } }

    #endregion

    #region "Открыть" и "Открыть с помощью"

    /// <summary>
    /// Вызывает <see cref="EFPXmlViewBox.GetFilePath()"/>
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    protected override void FileAssociationsHandler_FileNeeded(object sender, CancelEventArgs args)
    {
      if (ControlProvider.Control.IsEmpty)
      {
        EFPApp.ShowTempMessage(Res.EFPFileAssociationsCommandItemsHandler_Err_NoData);
        args.Cancel = true;
        return;
      }

      FileAssociationsHandler.FilePath = ControlProvider.GetFilePath();
    }

    #endregion;

    #region Файл-Отправить

    /// <summary>
    /// Реализация команды "Отправить" - "Writer"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    protected override void SendToOpenOfficeWriter(object sender, EventArgs args)
    {
      if (ControlProvider.Control.IsEmpty)
      {
        EFPApp.ShowTempMessage(Res.EFPFileAssociationsCommandItemsHandler_Err_NoData);
        return;
      }

      AbsPath filePath = ControlProvider.GetFilePath();

      try
      {
        EFPApp.UsedOpenOffice.Parts[OpenOfficePart.Writer].OpenFile(filePath, true);
      }
      catch (Exception e)
      {
        MessageBox.Show(String.Format(Res.EFPApp_Err_OpenWith, EFPApp.OpenOfficeKindName + " Writer", e.Message));
      }
    }

    ///// <summary>
    ///// Реализация команды "Отправить" - "Microsoft Word"
    ///// </summary>
    ///// <param name="sender">Не используется</param>
    ///// <param name="args">Не используется</param>
    //protected override void SendToMicrosoftWord(object sender, EventArgs args)
    //{
    //  // Ничего нельзя сделать, т.к. Word криво воспринимает xml-файлы (рисует какие-то странные схемы для них)
    //  base.SendToMicrosoftWord(sender, args);
    //}

    #endregion;
  }
}
