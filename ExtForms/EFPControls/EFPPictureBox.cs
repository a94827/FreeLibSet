// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;
using FreeLibSet.Drawing;

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Провайдер для PictureBox
  /// Не добавляется никакой дополнительной функциональности, за исключением 
  /// возможности управления видимостью с помощью свойства VisibleEx.
  /// </summary>
  public class EFPPictureBox : EFPControl<PictureBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPPictureBox(EFPBaseProvider baseProvider, PictureBox control)
      : base(baseProvider, control, true)
    {
    }

    #endregion
  }

  /// <summary>
  /// Провайдер кнопки, содержащей миниатюрное изображение.
  /// При нажатии кнопки открывается полноразмерный просмотр изображения.
  /// Просматриваемое (большое) изображение задается свойством MainImage.
  /// 
  /// Если загрузка большого изображения может быть медленной (например, 
  /// изображение хранится в базе данных), то можно задать маленькое изображение,
  /// задав свойство ThumbnailImage, и присоединить обработчик события MainImageNeeded.
  /// Тогда загрузка будет выполнена только при нажатии кнопки.
  /// 
  /// Свойство ReadOnly определяет возможность изменения большого изображения пользователем
  /// (по умолчанию разрешено).
  /// 
  /// Для кнопки используются команды локального меню, предназначенные для загрузки/сохранения
  /// изображения в файле на диске. Также поддерживается загрузка сканированного изображения
  /// через TWAIN-интерфейс.
  /// </summary>
  public class EFPThumbnailPictureButton : EFPControl<Button>, IEFPReadOnlyControl
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPThumbnailPictureButton(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPThumbnailPictureButton(IEFPControlWithToolBar<Button> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      Init();
    }

    private void Init()
    {
      _NoImageText = "No image";
      _MaxMainImageSize = new Size(int.MaxValue, int.MaxValue);
      _MaxThumbnailSize = new Size(160, 160);
      Control.FlatStyle = FlatStyle.Flat;
      if (!DesignMode)
        Control.Click += new EventHandler(Control_Click);
      InitButtonImage();
      Control.Disposed += new EventHandler(Control_Disposed);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает объект EFPThumbnailPictureButtonCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPThumbnailPictureButtonCommandItems(this);
    }

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPThumbnailPictureButtonCommandItems CommandItems
    {
      get
      {
        return (EFPThumbnailPictureButtonCommandItems)(base.CommandItems);
      }
    }

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    #endregion

    #region Основное изображение

    /// <summary>
    /// Основное (большое) изображение.
    /// Установка свойства сразу изменяет маленькое изображение
    /// При установке свойства не выполняется преобразование изображения, даже
    /// если оно больше ограничивающих размеров. 
    /// Кроме того, объект EFPThumbnailPictureButton становится владельцем присоединенного
    /// изображения.
    /// Используйте метод SetMainImage(), если требуется учесть ограничение в размерах и/или
    /// Вы не можете передать изображение "в собственность".
    /// 
    /// Первоначально изображение не установлено, при этом на кнопке отображается текст, задаваемый свойством NoImageText.
    /// В процессе работы допускается удаление изображения присвоением свойству значения null.
    /// </summary>
    public Image MainImage
    {
      get
      {
        if (_MainImage == null && (ThumbnailImage != null))
          OnMainImageNeeded();
        return _MainImage;
      }
      set
      {
        if (value == _MainImage)
          return;
        _MainImage = value;
        if (!_InsideImageNeeded)
        {
          if (value == null)
            ThumbnailImage = null;
          else
            ThumbnailImage = WinFormsTools.CreateThumbnailImage(value, MaxThumbnailSize);
        }
      }
    }

    private Image _MainImage;

    /// <summary>
    /// Задать большое изображение.
    /// Учитывается ограничение MaxMainImageSize и, при необходимости, изображение уменьшается
    /// с сохранением пропорций.
    /// Затем устанавливается свойство MainImage.
    /// Свойство MainImage всегда будет содержать копию изображения.
    /// Поэтому, может потребоваться удалить переданное изображение, если у него нет другого владельца.
    /// </summary>
    /// <param name="value">Устанавливаемое изображение или null</param>
    public void SetMainImage(Image value)
    {
      if (value == null)
      {
        MainImage = null;
        return;
      }


      Size newSize;
      if (ImagingTools.IsImageShrinkNeeded(value, MaxMainImageSize, out newSize))
        MainImage = new Bitmap(value, newSize);
      else
        MainImage = new Bitmap(value); // копия
    }

    /// <summary>
    /// Вызывается, если есть маленькое изображение, но нет большого.
    /// Обработчик должен установить свойство MainImage
    /// </summary>
    public event EventHandler MainImageNeeded;

    private void OnMainImageNeeded()
    {
      if (MainImageNeeded == null)
        return;
      if (_InsideImageNeeded)
        return;
      _InsideImageNeeded = true;
      try
      {
        MainImageNeeded(this, EventArgs.Empty);
      }
      finally
      {
        _InsideImageNeeded = false;
      }
    }

    private bool _InsideImageNeeded = false;

    /// <summary>
    /// Сбрасывает в null свойства MainImage и ThumbnailImage.
    /// После этого на кнопке отображается текст, задаваемый свойством NoImageText
    /// </summary>
    public void Clear()
    {
      _MainImage = null; // достаточно очистить ссылку, установка свойства ThumbnailImage=null выполнит перерисовку
      ThumbnailImage = null;
    }

    #endregion

    #region Маленькое изображение (значок)

    /// <summary>
    /// Текст "Нет изображения".
    /// Выводится на кнопке, когда свойство ThumnailImage=null.
    /// </summary>
    public string NoImageText
    {
      get { return _NoImageText; }
      set
      {
        if (value == _NoImageText)
          return;
        _NoImageText = value;
        InitButtonImage();
      }
    }
    private string _NoImageText;

    /// <summary>
    /// Маленькое изображение.
    /// Установка свойства перерисовывает кнопку, но не меняет основное изображение
    /// Установка значения null (по умолчанию) выводит на кнопке текст, задаваемый
    /// свойством NoImageText
    /// </summary>
    public Image ThumbnailImage
    {
      get { return Control.Image; }
      set
      {
        if (value == Control.Image)
          return;
        Control.Image = value;
        InitButtonImage();
        CommandItems.PerformRefreshItems();
        if (ImageChanged != null)
          ImageChanged(this, EventArgs.Empty);
      }
    }

    private void InitButtonImage()
    {
      if (Control.Image == null)
        Control.Text = NoImageText;
      else
        Control.Text = String.Empty;
    }

    /// <summary>
    /// Возвращает true если нет (маленького) изображения
    /// </summary>
    public bool IsEmpty
    {
      get { return ThumbnailImage == null; }
    }

    /// <summary>
    /// Событие вызывается при установке свойства ThumbnailImage.
    /// Косвенно событие вызывается и при установке большого изображения.
    /// </summary>
    public event EventHandler ImageChanged;

    #endregion

    #region Максимальный размер изображения

    /// <summary>
    /// Максимальный размер изображения.
    /// По умолчанию int.MaxValue x int.MaxValue - неограничен 
    /// </summary>
    public Size MaxMainImageSize
    {
      get { return _MaxMainImageSize; }
      set
      {
        if (value.Width < 100 || value.Height < 100)
          throw new ArgumentOutOfRangeException("Недопустимый максимальный размер изображения: " + value.ToString());
        _MaxMainImageSize = value;
      }
    }
    private Size _MaxMainImageSize;

    /// <summary>
    /// Максимальный размер миниатюры.
    /// По умолчанию - 160x160
    /// </summary>
    public Size MaxThumbnailSize
    {
      get { return _MaxThumbnailSize; }
      set
      {
        if (value.Width < 16 || value.Width > 320 || value.Height < 16 || value.Height > 320)
          throw new ArgumentOutOfRangeException("Недопустимый размер миниатюры: " + value.ToString());
        _MaxThumbnailSize = value;
      }
    }
    private Size _MaxThumbnailSize;

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Если установить свойство в true, то кнопка будет использоваться только для просмотра 
    /// изображения.
    /// Если false (по умолчанию), то будут доступны команды локального меню для загрузки изображения
    /// из файла, буфера обмена и сканера.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        if (value == _ReadOnly)
          return;
        _ReadOnly = value;
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
        CommandItems.PerformRefreshItems();
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство для ReadOnly
    /// </summary>
    public DepValue<Boolean> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyEx.Source = value;
      }
    }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<Boolean>(false,ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
      }
    }
    /// <summary>
    /// Выходная часть свойства ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyEx;

    private void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region Обработчики кнопки

    void Control_Disposed(object sender, EventArgs args)
    {
      if (_MainImage != null)
      {
        _MainImage.Dispose();
        _MainImage = null;
      }
    }

    internal void Control_Click(object sender, EventArgs args)
    {
      if (MainImage == null)
      {
        EFPApp.ShowTempMessage("Нет изображения для просмотра");
        return;
      }

      Form frm = new Form();
      frm.Text = "Просмотр изображения (" + MainImage.Width.ToString() + "x" + MainImage.Height.ToString() + ")";
      frm.Icon = EFPApp.MainImages.Icons["View"];
      frm.WindowState = FormWindowState.Maximized;
      frm.FormBorderStyle = FormBorderStyle.FixedDialog;
      frm.MaximizeBox = false;
      // Закрытие формы по <Esc>
      FreeLibSet.Controls.FormButtonStub.AssignCancel(frm);

      EFPFormProvider efpForm = new EFPFormProvider(frm);

      PictureBox pb = new PictureBox();
      pb.Dock = DockStyle.Fill;
      pb.SizeMode = PictureBoxSizeMode.CenterImage;
      frm.Controls.Add(pb);
      pb.Image = MainImage;
      EFPApp.ShowDialog(frm, true);
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню кнопки просмотра изображения
  /// </summary>
  public class EFPThumbnailPictureButtonCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает команды
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPThumbnailPictureButtonCommandItems(EFPThumbnailPictureButton controlProvider)
      :base(controlProvider)
    {
      ciOpen = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Open);
      ciOpen.Enabled = true;
      ciOpen.Click += new EventHandler(ciOpen_Click);
      Add(ciOpen);

      ciSave = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Save);
      ciSave.Click += new EventHandler(ciSave_Click);
      Add(ciSave);

      ciSaveAs = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SaveAs);
      ciSaveAs.MenuText = null;
      ciSaveAs.Click += new EventHandler(ciSave_Click);
      ciSaveAs.Usage = EFPCommandItemUsage.Menu;
      Add(ciSaveAs);

      ciClear = new EFPCommandItem("Edit", "Clear");
      ciClear.MenuText = "Очистить изображение";
      ciClear.ImageKey = "No";
      ciClear.Click += new EventHandler(ciClear_Click);
      Add(ciClear);

      if (!EnvironmentTools.IsMono) // не совместимо
      {
        ciScan = new EFPCommandItem("Edit", "Acquire");
        ciScan.MenuText = "Со сканера или камеры";
        ciScan.ImageKey = "Scan";
        ciScan.Click += new EventHandler(ciScan_Click);
        Add(ciScan);
      }
      AddSeparator();

      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.Click += new EventHandler(ciCut_Click);
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Click += new EventHandler(ciCopy_Click);
      Add(ciCopy);

      ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
      ciPaste.Click += new EventHandler(ciPaste_Click);
      Add(ciPaste);

      AddSeparator();

      ciView = new EFPCommandItem("Edit", "View");
      ciView.MenuText = "Просмотр изображения";
      ciView.ImageKey = "View";
      ciView.Usage = EFPCommandItemUsage.Menu;
      ciView.MenuRightText = EFPCommandItem.GetShortCutText(Keys.Space);
      ciView.Click += new EventHandler(controlProvider.Control_Click);
      Add(ciView);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPThumbnailPictureButton ControlProvider { get { return (EFPThumbnailPictureButton)(base.ControlProvider); } }

    #endregion

    #region Команды загрузки и сохранения

    private EFPCommandItem ciOpen, ciClear, ciSave, ciSaveAs;

    void ciOpen_Click(object sender, EventArgs args)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter =
        "Все изображения|*.BMP;*.DIB;*.JPG;*.JPEG;*.GIF;*.PNG;*TIF;*.TIFF|" +
        "JPEG|*.JPEG;*.JPG|" +
        "Точечные рисунки (BMP)|*.BMP;*.DIB|" +
        "GIF|*.GIF|" +
        "PNG|*.PNG|" +
        "TIFF|*.TIF;*.TIFF";
      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      Image img;
      try
      {
        img = Image.FromFile(dlg.FileName);
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox("Не удалось загрузить файл изображения \"" + dlg.FileName + "\". " + e.Message);
        return;
      }

      ControlProvider.SetMainImage(img);
      img.Dispose();
    }

    void ciClear_Click(object sender, EventArgs args)
    {
      ControlProvider.Clear();
    }

    private static int _LastSaveFilterIndex = 1;

    void ciSave_Click(object sender, EventArgs args)
    {
      if (ControlProvider.MainImage == null)
      {
        EFPApp.ShowTempMessage("Нет изображения");
        return;
      }

      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter =
        "JPEG|*.JPG|" +
        "Точечные рисунки BMP|*.BMP|" +
        "GIF|*.GIF|" +
        "PNG|*.PNG|" +
        "TIFF|*.TIF";
      dlg.FilterIndex = _LastSaveFilterIndex;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      _LastSaveFilterIndex = dlg.FilterIndex;
      switch (dlg.FilterIndex)
      {
        case 1:
          ControlProvider.MainImage.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
          break;
        case 2:
          ControlProvider.MainImage.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
          break;
        case 3:
          ControlProvider.MainImage.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Gif);
          break;
        case 4:
          ControlProvider.MainImage.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
          break;
        case 5:
          ControlProvider.MainImage.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
          break;
        default:
          throw new BugException("Неизвестный индекс формата файла: " + dlg.FilterIndex.ToString());
      }
    }

    #endregion

    #region Сканирование

    private EFPCommandItem ciScan;

    private void ciScan_Click(object sender, EventArgs args)
    {
#if WIA
      Interop.WIALib.WiaClass wiaManager = new Interop.WIALib.WiaClass();

      object selectUsingUI = System.Reflection.Missing.Value;

      // let user select device
      Interop.WIALib.ItemClass wiaRoot = (Interop.WIALib.ItemClass)wiaManager.Create(
          ref selectUsingUI);

      // this call shows the common WIA dialog to let
      // the user select a picture:
      Interop.WIALib.CollectionClass wiaPics = wiaRoot.GetItemsFromUI(Interop.WIALib.WiaFlag.SingleImage,
          Interop.WIALib.WiaIntent.ImageTypeColor) as Interop.WIALib.CollectionClass;

      // enumerate all the pictures the user selected
      foreach (object wiaObj in wiaPics)
      {
        Interop.WIALib.ItemClass wiaItem = (Interop.WIALib.ItemClass)System.Runtime.InteropServices.Marshal.CreateWrapperOfType(
            wiaObj, typeof(Interop.WIALib.ItemClass));

        // transfer picture to our temporary file
        string FileName = "C:\aaa.bmp";
        wiaItem.Transfer(FileName, false);
      }

#else
      EFPTwainHandler handler = new EFPTwainHandler();
      try
      {
        Bitmap[] bmps = handler.Acqiire();
        if (bmps.Length > 0)
          ControlProvider.SetMainImage(bmps[0]);
        // Уничтожением Bitmap занимается EFPTwainHandler 
      }
      finally
      {
        handler.Dispose();
      }
#endif
    }

    #endregion

    #region Команды буфера обмена

    private EFPCommandItem ciCut, ciCopy, ciPaste;

    void ciCut_Click(object sender, EventArgs args)
    {
      if (ControlProvider.MainImage == null)
      {
        EFPApp.ShowTempMessage("Нет изображения для копирования в буфер обмена");
        return;
      }
      EFPApp.Clipboard.SetImage(ControlProvider.MainImage);
      ControlProvider.Clear();
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (ControlProvider.MainImage == null)
      {
        EFPApp.ShowTempMessage("Нет изображения для копирования в буфер обмена");
        return;
      }
      EFPApp.Clipboard.SetImage(ControlProvider.MainImage);
    }

    void ciPaste_Click(object sender, EventArgs args)
    {
      Image img = EFPApp.Clipboard.GetImage();
      if (EFPApp.Clipboard.HasError)
        return;
      if (img == null)
      {
        EFPApp.ShowTempMessage("Буфер обмена не содержит изображения");
        return;
      }
      ControlProvider.SetMainImage(img);
    }

    #endregion

    #region Просмотр

    private EFPCommandItem ciView;

    #endregion

    #region Инициализация состояния команд

    /// <summary>
    /// Дополнительная инициализация команд меню
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      string auxText = String.Empty;
      if (ControlProvider.MaxMainImageSize.Width != int.MaxValue)
        auxText = ". Изображение будет уменьшено до размера " + ControlProvider.MaxMainImageSize.Width.ToString() +
          "x" + ControlProvider.MaxMainImageSize.Height.ToString() + " пикселей";
      ciOpen.ToolTipText = "Загрузить изображение из файла" + auxText;
      ciPaste.ToolTipText = "Вставить изображение из буфера обмена" + auxText;

      PerformRefreshItems();
    }


    /// <summary>
    /// Установка свойства Enabled для команд меню.
    /// Вызывается при смене изображения и при изменении свойства EFPThumbnailPictureButton.ReadOnly
    /// </summary>
    public virtual void PerformRefreshItems()
    {
      ciOpen.Enabled = !ControlProvider.ReadOnly;
      ciSave.Enabled = !ControlProvider.IsEmpty;
      ciClear.Enabled = (!ControlProvider.ReadOnly) && (!ControlProvider.IsEmpty);
      ciScan.Enabled = !ControlProvider.ReadOnly;
      ciCut.Enabled = (!ControlProvider.ReadOnly) && (!ControlProvider.IsEmpty);
      ciCopy.Enabled = !ControlProvider.IsEmpty;
      ciPaste.Enabled = !ControlProvider.ReadOnly;
      ciView.Enabled = !ControlProvider.IsEmpty;
    }

    #endregion
  }
}
