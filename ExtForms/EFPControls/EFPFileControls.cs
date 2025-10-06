// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using FreeLibSet.DependedValues;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using System.ComponentModel;

namespace FreeLibSet.Forms
{
  /*
   * Провайдеры управляющих элементов для полей выбора файлов и каталогов
   */

  /// <summary>
  /// Базовый класс для <see cref="EFPFilePathValidator"/> и <see cref="EFPFolderPathValidator"/> 
  /// </summary>
  public abstract class EFPPathValidatorBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта
    /// </summary>
    /// <param name="controlProvider">Провайдер основного управляющего элемента (текстового поля или комбоблока). Не может быть null</param>
    public EFPPathValidatorBase(IEFPSimpleTextBox controlProvider)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      _ControlProvider = controlProvider;

      _AmbientPathValidateMode = TestPathMode.FormatOnly;

      _ControlProvider.ValidateWhenFocusChanged = true;
      _ControlProvider.Validating += ControlProvider_Validating;
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Основной управляюший элемент для ввода пути.
    /// Задается в конструкторе.
    /// </summary>
    public IEFPSimpleTextBox ControlProvider { get { return _ControlProvider; } }
    private IEFPSimpleTextBox _ControlProvider;

    #endregion

    #region PathValidateMode

    /// <summary>
    /// Режим проверки.
    /// Если свойство не установлено в явном виде, возвращает значение свойства <see cref="AmbientPathValidateMode"/>.
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get { return _PathValidateMode ?? _AmbientPathValidateMode; }
      set
      {
        CheckValidateMode(value);

        if (_PathValidateMode.HasValue)
        {
          if (value == _PathValidateMode)
            return;

          _PathValidateMode = value;
        }
        else
        {
          _PathValidateMode = value;

          if (value == _AmbientPathValidateMode)
            return;
        }

        _ControlProvider.Validate();
      }
    }
    private TestPathMode? _PathValidateMode;

    /// <summary>
    /// Режим проверки по умолчанию, используемый, если свойство <see cref="PathValidateMode"/> не установлено в явном виде.
    /// Провайдеры кнопки "Обзор" <see cref="EFPFileDialogButton"/> и <see cref="EFPFolderBrowserButton"/> и устанавливают это свойство, в зависимости
    /// от текущих значений других свойств.
    /// </summary>
    public TestPathMode AmbientPathValidateMode
    {
      get { return _AmbientPathValidateMode; }
      set
      {
        CheckValidateMode(value);

        if (value == _AmbientPathValidateMode)
          return;

        TestPathMode oldMode = this.PathValidateMode;
        _AmbientPathValidateMode = value;
        if (this.PathValidateMode != oldMode)
          ControlProvider.Validate();
      }
    }
    private TestPathMode _AmbientPathValidateMode;

    /// <summary>
    /// Сбрасывает свойство <see cref="PathValidateMode"/> в исходное состояние, равное <see cref="AmbientPathValidateMode"/>.
    /// </summary>
    public void ResetPathValidateMode()
    {
      if (!_PathValidateMode.HasValue)
        return;

      bool NeedsValidate = (_PathValidateMode.Value != _AmbientPathValidateMode);
      _PathValidateMode = null;
      if (NeedsValidate)
        _ControlProvider.Validate();
    }

    /// <summary>
    /// Выполняет проверку при установке свойств <see cref="PathValidateMode"/> и <see cref="AmbientPathValidateMode"/>.
    /// Должен выбрасывать исключение, если устанавливается недопустимое значение.
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    protected virtual void CheckValidateMode(TestPathMode value)
    {
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Предотвращение вложенного вызова
    /// </summary>
    private bool _InsideControlProvider_Validating;

    void ControlProvider_Validating(object sender, UIValidatingEventArgs args)
    {
      if (_InsideControlProvider_Validating)
        args.SetError(Res.EFPPathValidator_Err_NestedValidation);
      else
      {
        _InsideControlProvider_Validating = true;
        try
        {
          DoControlProvider_Validating();
        }
        finally
        {
          _InsideControlProvider_Validating = false;
        }
      }
    }

    private string _LastValidatedText;

    private string _LastValidateMessage;

    void DoControlProvider_Validating()
    {
      if (_ControlProvider.ValidateState == UIValidateState.Error)
        return;
      if (String.IsNullOrEmpty(_ControlProvider.Text))
        return;
      if (PathValidateMode == TestPathMode.None)
        return;

      if (_ControlProvider.BaseProvider.ValidateReason == EFPFormValidateReason.ValidateForm || _ControlProvider.BaseProvider.ValidateReason == EFPFormValidateReason.Closing)
      {
        if (PrepareText())
          _LastValidatedText = null;
      }

      if (_LastValidatedText != null)
      {
        if (_ControlProvider.BaseProvider.ValidateReason == EFPFormValidateReason.Unknown && _ControlProvider.Control.ContainsFocus)
        {
          if (_LastValidatedText == _ControlProvider.Text)
          {
#if DEBUG
            if (_LastValidateMessage == null)
              throw new BugException("ValidateMessage==null");
#endif
            // Не надо еще раз проверять
            if (_LastValidateMessage.Length > 0)
              _ControlProvider.SetError(_LastValidateMessage);
            return;
          }

          // Упрощенная проверка
          string errorText;
          if (!Validate(_ControlProvider.Text, TestPathMode.FormatOnly, out errorText))
            _ControlProvider.SetError(errorText);
          return;
        }
      }

      // Полная проверка
      _LastValidatedText = _ControlProvider.Text;
      if (Validate(_LastValidatedText, PathValidateMode, out _LastValidateMessage))
        _LastValidateMessage = String.Empty; // а не null;
      else
        _ControlProvider.SetError(_LastValidateMessage);
    }

    /// <summary>
    /// Переопределяется для <see cref="EFPFolderPathValidator"/>
    /// </summary>
    /// <returns>true, если текст изменился</returns>
    protected virtual bool PrepareText()
    {
      return false;
    }

    /// <summary>
    /// Вызывает подходящий метод для проверки пути
    /// </summary>
    /// <param name="text">Проверяемый путь</param>
    /// <param name="mode">Режим проверки</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>True, если путь корректный</returns>
    protected abstract bool Validate(string text, TestPathMode mode, out string errorText);

    #endregion
  }

  /// <summary>
  /// Автономный объект для проверки текстового поля на правильность ввода пути к файлу.
  /// Обычно используется провайдером кнопки "Обзор" <see cref="EFPFileDialogButton"/> , который использует внутреннюю копию валидатора.
  /// Прикладной код может использовать этот объект, если имеется поле ввода, но нет кнопки "Обзор".
  /// Может задаваться как абсолютный, так и относительный путь.
  /// Проверка формата пути выполняется при каждом изменении текста. 
  /// Проверка существования пути или его части выполняется только при открытии/закрытии формы или при получении/потере
  /// фокуса ввода элементом. 
  /// </summary>
  public class EFPFilePathValidator : EFPPathValidatorBase
  {
    #region Конструктор

    /// <summary>
    /// Создает экземпляр валидатора и присоединяет его к текстовому полю.
    /// Для поля добавляется обработчик события <see cref="EFPControlBase.Validating"/> и устанавливается свойство <see cref="EFPControlBase.ValidateWhenFocusChanged"/>=true.
    /// </summary>
    /// <param name="controlProvider">Провайдер основного управляющего элемента (текстового поля или комбоблока). 
    /// Не может быть null</param>
    public EFPFilePathValidator(IEFPSimpleTextBox controlProvider)
      : base(controlProvider)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вызывает <see cref="EFPFileTools.TestFilePath(string, TestPathMode, out string)"/> для проверки
    /// </summary>
    /// <param name="text">Проверяемый путь</param>
    /// <param name="mode">Режим проверки</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>True, если путь корректный</returns>
    protected override bool Validate(string text, TestPathMode mode, out string errorText)
    {
      return EFPFileTools.TestFilePath(text, mode, out errorText);
    }

    #endregion
  }

  /// <summary>
  /// Автономный объект для проверки текстового поля на правильность ввода пути к каталогу.
  /// Обычно используется провайдером кнопки "Обзор" <see cref="EFPFolderBrowserButton"/>, который использует внутреннюю копию валидатора.
  /// Прикладной код может использовать этот объект, если имеется поле ввода, но нет кнопки "Обзор".
  /// Может задаваться как абсолютный, так и относительный путь.
  /// Имя каталога в Windows должно заканчиваться символом обратной косой черты <see cref="System.IO.Path.DirectorySeparatorChar"/>.
  /// Проверка формата пути выполняется при каждом изменении текста. 
  /// Проверка существования пути или его части выполняется только при открытии/закрытии формы или при получении/потере
  /// фокуса ввода элементом. 
  /// Если пользователь ввел текст без завершающего символа, то символ добавляется автоматически при проверке формы или когда фокус ввода покидает поле.
  /// </summary>
  public class EFPFolderPathValidator : EFPPathValidatorBase
  {
    #region Конструктор

    /// <summary>
    /// Создает экземпляр валидатора и присоединяет его к текстовому полю.
    /// Для поля добавляется обработчик события <see cref="EFPControlBase.Validating"/> и устанавливается свойство <see cref="EFPControlBase.ValidateWhenFocusChanged"/>=true.
    /// </summary>
    /// <param name="controlProvider">Провайдер основного управляющего элемента (текстового поля или комбоблока). 
    /// Не может быть null</param>
    public EFPFolderPathValidator(IEFPSimpleTextBox controlProvider)
      : base(controlProvider)
    {
      controlProvider.Leave += ControlProvider_Leave;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выбрасывает исключение для <paramref name="value"/>=<see cref="TestPathMode.FileExists"/>.
    /// </summary>
    /// <param name="value">Проверяемый режим</param>
    protected override void CheckValidateMode(TestPathMode value)
    {
      base.CheckValidateMode(value);
      if (value == TestPathMode.FileExists)
        throw ExceptionFactory.ArgUnknownValue("value", value,  new object[] {
          TestPathMode.None, TestPathMode.FormatOnly, TestPathMode.RootExists, TestPathMode.DirectoryExists });
    }

    private void ControlProvider_Leave(object sender, EventArgs args)
    {
      PrepareText();
    }

    /// <summary>
    /// Если пользователь ввел или вставил из буфера обмена путь без слэша, то проверяем, не будет ли формат пути правильным, если добавить слэш.
    /// Если добавление решает проблему, то меняем значение.
    /// </summary>
    /// <returns>true, если текст изменился</returns>
    protected override bool PrepareText()
    {
      bool res = false;
      try
      {
        string s = ControlProvider.Text;
        if (!String.IsNullOrEmpty(s))
        {
          if (s[s.Length - 1] != Path.DirectorySeparatorChar)
          {
            s += Path.DirectorySeparatorChar;
            string errorText;
            if (EFPFileTools.TestDirSlashedPath(s, TestPathMode.FormatOnly, out errorText))
            {
              ControlProvider.Text = s;
              res = true;
            }
          }
        }
      }
      catch { }
      return res;
    }

    /// <summary>
    /// Вызывает <see cref="EFPFileTools.TestDirSlashedPath(string, TestPathMode, out string)"/> для проверки
    /// </summary>
    /// <param name="text">Проверяемый путь</param>
    /// <param name="mode">Режим проверки</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>True, если путь корректный</returns>
    protected override bool Validate(string text, TestPathMode mode, out string errorText)
    {
      return EFPFileTools.TestDirSlashedPath(text, mode, out errorText);
    }

    #endregion
  }

  /// <summary>
  /// Кнопка "Обзор" для выбора каталога, имя которого вводится в поле ввода или комбоблоке с историей.
  /// Предполагается, что в текстовом поле, к которому присоединена кнопка, находится путь к каталогу, заканчивающийся обратной косой чертой (<see cref="System.IO.Path.DirectorySeparatorChar"/>).
  /// Кнопка также может использоваться совместно с текстовым полем только для чтения, когда обработка каталога должна выполнятся 
  /// после выбора каталога, а не при закрытии формы.
  /// Событие <see cref="EFPButton.Click"/> вызывается только после того, как пользователь выбрал каталог.
  /// </summary>
  public class EFPFolderBrowserButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента кнопки.
    /// </summary>
    /// <param name="mainProvider">Провайдер основного управляющего элемента,
    /// предназначенного для ввода пути. Обычно это <see cref="EFPTextBox"/> или <see cref="EFPHistComboBox"/></param>
    /// <param name="control">Управляющий элемент - кнопка "Обзор"</param>
    public EFPFolderBrowserButton(IEFPTextBox mainProvider, Button control)
      : base(mainProvider.BaseProvider, control)
    {
      _MainProvider = mainProvider;
      SetMainImageKey("Open");
      VisibleEx = mainProvider.VisibleEx;
      EnabledEx = mainProvider.EnabledEx;
      base.ToolTipText = Res.EFPFolderBrowserButton_ToolTip_Default;
      
      _PathHandler = new UIPathInputHandler(mainProvider.TextEx);
      _PathHandler.UseSlashedPath = true;

      _PathValidator = new EFPFolderPathValidator(mainProvider);
      _PathValidator.AmbientPathValidateMode = TestPathMode.DirectoryExists;

      InitDragDrop(mainProvider.Control);
      InitDragDrop(this.Control);

      if (mainProvider.CommandItems.PasteHandler != null)
      {
        EFPPasteFormat fmtFileDrop = new EFPPasteFormat(DataFormats.FileDrop);
        fmtFileDrop.AutoConvert = false;
        fmtFileDrop.DisplayName = Res.EFPFolderBrowserButton_Name_FormatFileDrop;
        fmtFileDrop.TestFormat += FmtFileDrop_TestFormat;
        fmtFileDrop.Paste += FmtFileDrop_Paste;
        mainProvider.CommandItems.PasteHandler.Insert(0, fmtFileDrop); // должно быть до текста
      }

      mainProvider.DefaultButton = control;
    }

    #endregion

    #region Обработчик путей

    private readonly UIPathInputHandler _PathHandler;

    /// <summary>
    /// Текущий выбранный каталог (абсолютный путь).
    /// Если свойство <see cref="MainProvider"/>.Text не определяет корректный путь (с учетом возможного добавления символа <see cref="System.IO.Path.DirectorySeparatorChar"/> в конце строки), свойство возвращает <see cref="AbsPath.Empty"/>.
    /// Свойство <see cref="PathValidateMode"/> не влияет на это свойство.
    /// </summary>
    public AbsPath Path
    {
      get { return _PathHandler.Path; }
      set { _PathHandler.Path = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Path"/>
    /// </summary>
    public DepValue<AbsPath> PathEx
    {
      get { return _PathHandler.PathEx; }
      set { _PathHandler.PathEx = value; }
    }


    /// <summary>
    /// Текущий выбранный каталог как объект <see cref="FreeLibSet.IO.RelPath"/>.
    /// Если свойство <see cref="MainProvider"/>.Text не определяет корректный путь (с учетом возможного добавления символа <see cref="System.IO.Path.DirectorySeparatorChar"/> в конце строки), свойство возвращает <see cref="AbsPath.Empty"/>.
    /// Свойство <see cref="PathValidateMode"/> не влияет на это свойство.
    /// </summary>
    public RelPath RelPath
    {
      get { return _PathHandler.RelPath; }
      set { _PathHandler.RelPath = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="RelPath"/>
    /// </summary>
    public DepValue<RelPath> RelPathEx
    {
      get { return _PathHandler.RelPathEx; }
      set { _PathHandler.RelPathEx = value; }
    }


    /// <summary>
    /// Управляемое свойство, которое возвращает true, если <see cref="RelPath"/>.IsEmpty=false.
    /// В отличие от <see cref="MainProvider"/>.IsEmpty, свойство будет возвращать false, если задан неправильный путь.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx { get { return _PathHandler.IsNotEmptyEx; } }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Провайдер основного поля ввода или комбоблока (задается в конструкторе)
    /// </summary>
    public IEFPTextBox MainProvider { get { return _MainProvider; } }
    private IEFPTextBox _MainProvider;

    /// <summary>
    /// Описание, появляющееся в блоке диалога выбора папки при нажатии кнопки
    /// Если свойство не задано, то в качестве пояснения будет использовано
    /// <see cref="MainProvider"/>.DisplayName
    /// </summary>
    public string Description { get { return _Description ?? String.Empty; } set { _Description = value; } }
    private string _Description;

    /// <summary>
    /// Наличие кнопки "Создать папку" в блоке диалога
    /// </summary>
    public bool ShowNewFolderButton
    {
      get { return _ShowNewFolderButton; }
      set
      {
        _ShowNewFolderButton = value;
        _PathValidator.AmbientPathValidateMode = value ? TestPathMode.RootExists : TestPathMode.DirectoryExists;
      }
    }
    private bool _ShowNewFolderButton;

    /// <summary>
    /// Базовый каталог, который используется при вводе относительных путей, для получения свойства <see cref="Path"/> из введенного текста.
    /// По умолчанию является текущим рабочим каталогом.
    /// </summary>
    public AbsPath BasePath
    {
      get { return _PathHandler.BasePath; }
      set { _PathHandler.BasePath = value; }
    }

    #endregion

    #region Проверка введенного текста

    private EFPFolderPathValidator _PathValidator;

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства <see cref="ShowNewFolderButton"/>. При <see cref="ShowNewFolderButton"/>=true возвращает <see cref="TestPathMode.RootExists"/>,
    /// а при false - <see cref="TestPathMode.DirectoryExists"/>
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get { return _PathValidator.PathValidateMode; }
      set { _PathValidator.PathValidateMode = value; }
    }

    /// <summary>
    /// Сбрасывает свойство <see cref="PathValidateMode"/> в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidator.ResetPathValidateMode();
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Выводит стандартный блок диалога выбора каталога <see cref="FolderBrowserDialog"/>.
    /// Если пользователь закрыл диалог кнопкой "ОК", устанавливается свойство <see cref="MainProvider"/>.Text.
    /// Затем вызывается обработчик события <see cref="EFPButton.Click"/>.
    /// </summary>
    protected override void OnClick()
    {
      using (FolderBrowserDialog dlg = new FolderBrowserDialog())
      {
        if (String.IsNullOrEmpty(Description))
          dlg.Description = MainProvider.DisplayName;
        else
          dlg.Description = Description;
        dlg.ShowNewFolderButton = ShowNewFolderButton;
        try
        {
          AbsPath dir = new AbsPath(MainProvider.Text);
          dlg.SelectedPath = dir.Path;
        }
        catch { } // 26.06.2024
        if (EFPApp.ShowDialog(dlg) == DialogResult.OK)
        {
          AbsPath path = new AbsPath(dlg.SelectedPath);
          MainProvider.Text = path.SlashedPath;
          base.OnClick();
        }
      }
    }

    #endregion

    #region Drag-and-drop

    private void InitDragDrop(Control control)
    {
      control.AllowDrop = true;
      control.DragEnter += Control_DragEnter;
      control.DragDrop += Control_DragDrop;
    }

    private void Control_DragEnter(object sender, DragEventArgs args)
    {
      try
      {
        if (args.Data.GetDataPresent(DataFormats.FileDrop))
          args.Effect = DragDropEffects.Link;
      }
      catch { }
    }

    private void Control_DragDrop(object sender, DragEventArgs args)
    {
      try
      {
        DoControl_DragDrop(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPFolderBrowserButton_Err_DragDrop, e.Message));
      }
    }

    private void DoControl_DragDrop(DragEventArgs args)
    {
      if (!args.Data.GetDataPresent(DataFormats.FileDrop))
        return;

      string[] a = (string[])(args.Data.GetData(DataFormats.FileDrop));
      if (a.Length != 1)
      {
        EFPApp.ShowTempMessage(Res.EFPFolderBrowserButton_Err_DragDropMulti);
        return;
      }

      AbsPath path = new AbsPath(a[0]);
      string errorText;
      if (!EFPFileTools.TestDirSlashedPath(path.SlashedPath,
        TestPathMode.DirectoryExists, // всегда проверяем, что это каталог, а не файл, независимо от свойства PathValidateMode
        out errorText))
      {
        EFPApp.ShowTempMessage(errorText);
        return;
      }

      MainProvider.Text = path.SlashedPath;
    }

    #endregion

    #region Буфер обмена

    private void FmtFileDrop_TestFormat(object sender, EFPTestDataObjectEventArgs args)
    {
      args.DataImageKey = "WindowsExplorer";
      //args.Appliable = true;
      if (args.Data.GetDataPresent(DataFormats.FileDrop, false)) // 04.12.2023
        args.Result = EFPTestDataObjectResult.Ok;
    }

    private void FmtFileDrop_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      string[] a = args.Data.GetData(DataFormats.FileDrop) as string[];
      if (a == null)
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_NoFileDrop);
        return;
      }
      if (a.Length != 1)
      {
        EFPApp.ShowTempMessage(Res.EFPFolderBrowserButton_Err_PasteMulti);
        return;
      }

      AbsPath path = new AbsPath(a[0]);
      string errorText;
      if (!EFPFileTools.TestDirSlashedPath(path.SlashedPath,
        TestPathMode.DirectoryExists, // всегда проверяем, что это каталог, а не файл, независимо от свойства PathValidateMode
        out errorText))
      {
        EFPApp.ShowTempMessage(errorText);
        return;
      }

      MainProvider.Text = path.SlashedPath;
    }

    #endregion
  }

  /// <summary>
  /// Кнопка "Обзор" для выбора файла, имя которого вводится в поле ввода или комбоблоке с историей.
  /// Кнопка также может использоваться совместно с текстовым полем только для чтения, когда обработка файла должна выполнятся 
  /// после выбора файла, а не при закрытии формы.
  /// Предполагается, что текстовое поле содержит полный путь к файлу.
  /// Событие <see cref="EFPButton.Click"/> вызывается только после того, как пользователь выбрал файл.
  /// </summary>
  public class EFPFileDialogButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента кнопки
    /// </summary>
    /// <param name="mainProvider">Провайдер основного управляющего элемента,
    /// предназначенного для ввода пути. Обычно это <see cref="EFPTextBox"/> или <see cref="EFPHistComboBox"/>. Не иожет быть null</param>
    /// <param name="control">Управляющий элемент - кнопка "Обзор"</param>
    public EFPFileDialogButton(IEFPTextBox mainProvider, Button control)
      : base(mainProvider.BaseProvider, control)
    {
      _MainProvider = mainProvider;
      SetMainImageKey("Open");
      VisibleEx = mainProvider.VisibleEx;
      EnabledEx = mainProvider.EnabledEx;
      base.ToolTipText = Res.EFPFileDialogButton_ToolTip_Default;

      _PathHandler = new UIPathInputHandler(mainProvider.TextEx);
      _PathHandler.UseSlashedPath = false;

      _PathValidator = new EFPFilePathValidator(mainProvider);
      Mode = FileDialogMode.Read;

      InitDragDrop(mainProvider.Control);
      InitDragDrop(this.Control);

      if (mainProvider.CommandItems.PasteHandler != null)
      {
        EFPPasteFormat fmtFileDrop = new EFPPasteFormat(DataFormats.FileDrop);
        fmtFileDrop.AutoConvert = false;
        fmtFileDrop.DisplayName = Res.EFPFileDialogButton_Name_DragDropFormat;
        fmtFileDrop.TestFormat += FmtFileDrop_TestFormat;
        fmtFileDrop.Paste += FmtFileDrop_Paste;
        mainProvider.CommandItems.PasteHandler.Insert(0, fmtFileDrop); // должно быть до текста
      }

      mainProvider.DefaultButton = control;
    }

    #endregion

    #region Обработка путей

    private readonly UIPathInputHandler _PathHandler;

    /// <summary>
    /// Текущий выбранный файл (абсолютный путь).
    /// Если свойство <see cref="MainProvider"/>.Text не определяет корректный путь, свойство возвращает <see cref="AbsPath.Empty"/>.
    /// Свойство <see cref="PathValidateMode"/> не влияет на это свойство.
    /// </summary>
    public AbsPath Path
    {
      get { return _PathHandler.Path; }
      set { _PathHandler.Path = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Path"/>
    /// </summary>
    public DepValue<AbsPath> PathEx
    {
      get { return _PathHandler.PathEx; }
      set { _PathHandler.PathEx = value; }
    }


    /// <summary>
    /// Текущий выбранный файл как объект <see cref="FreeLibSet.IO.RelPath"/>.
    /// Если свойство <see cref="MainProvider"/>.Text не определяет корректный путь, свойство возвращает <see cref="AbsPath.Empty"/>.
    /// Свойство <see cref="PathValidateMode"/> не влияет на это свойство.
    /// </summary>
    public RelPath RelPath
    {
      get { return _PathHandler.RelPath; }
      set { _PathHandler.RelPath = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="RelPath"/>
    /// </summary>
    public DepValue<RelPath> RelPathEx
    {
      get { return _PathHandler.RelPathEx; }
      set { _PathHandler.RelPathEx = value; }
    }

    /// <summary>
    /// Управляемое свойство, которое возвращает true, если <see cref="Path"/>.IsEmpty=false.
    /// В отличие от <see cref="MainProvider"/>.IsEmpty, свойство будет возвращать false, если введенный текст задает путь в неправильном формате.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx { get { return _PathHandler.IsNotEmptyEx; } }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Провайдер основного поля ввода или комбоблока (задается в конструкторе)
    /// </summary>
    public IEFPTextBox MainProvider { get { return _MainProvider; } }
    private IEFPTextBox _MainProvider;

    /// <summary>
    /// Описание, появляющееся в блоке диалога выбора папки при нажатии кнопки
    /// Если свойство не задано, то в качестве пояснения будет использовано
    /// <see cref="MainProvider"/>.DisplayName
    /// </summary>
    public string Title { get { return _Title ?? String.Empty; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Значение для свойства Filter (с разделителями "|")
    /// </summary>
    public string Filter { get { return _Filter ?? String.Empty; } set { _Filter = value; } }
    private string _Filter;

    /// <summary>
    /// Вариант диалога для выбора существующего файла или записи
    /// </summary>
    public FileDialogMode Mode
    {
      get { return _Mode; }
      set
      {
        _Mode = value;
        _PathValidator.AmbientPathValidateMode = (value == FileDialogMode.Write) ? TestPathMode.RootExists : TestPathMode.FileExists;
      }
    }
    private FileDialogMode _Mode;

    /// <summary>
    /// Каталог.
    /// При открытии диалога используется в качестве свойства <see cref="FileDialog.InitialDirectory"/>,
    /// если в поле ввода не задано имя файла.
    /// После того, как файл выбран в диалоге, содержит каталог, соответствующий файлу <see cref="FileDialog.FileName"/>.
    /// Если путь к файлу введен вручную, свойство не обновляется.
    /// </summary>
    public AbsPath Directory
    {
      get { return _Directory; }
      set { _Directory = value; }
    }
    private AbsPath _Directory;

    /// <summary>
    /// Базовый каталог, который используется при вводе относительных путей, для получения свойства <see cref="Path"/> из введенного текста.
    /// По умолчанию является текущим рабочим каталогом.
    /// </summary>
    public AbsPath BasePath
    {
      get { return _PathHandler.BasePath; }
      set { _PathHandler.BasePath = value; }
    }

    #endregion

    #region Проверка введенного текста

    private EFPFilePathValidator _PathValidator;

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства <see cref="Mode"/>. При <see cref="Mode"/>=<see cref="FileDialogMode.Write"/> возвращает <see cref="TestPathMode.RootExists"/>,
    /// а при <see cref="FileDialogMode.Read"/> - <see cref="TestPathMode.FileExists"/>
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get { return _PathValidator.PathValidateMode; }
      set { _PathValidator.PathValidateMode = value; }
    }

    /// <summary>
    /// Сбрасывает свойство <see cref="PathValidateMode"/> в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidator.ResetPathValidateMode();
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Выводит стандартный блок диалога выбора файла <see cref="OpenFileDialog"/> или <see cref="SaveFileDialog"/>,
    /// в зависимости от свойства <see cref="Mode"/>. Свойство <see cref="FileDialog.FileName"/>  устанавливается равным текущему значению в поле ввода.
    /// Если диалог закрыт нажатием кнопки "ОК" (файл выбран пользователем), устанавливается свойство <see cref="MainProvider"/>.Text.
    /// Затем вызывается обработчик события <see cref="EFPButton.Click"/>.
    /// </summary>
    protected override void OnClick()
    {
      FileDialog dlg;
      if (Mode == FileDialogMode.Read)
        dlg = new OpenFileDialog();
      else
        dlg = new SaveFileDialog();

      using (dlg)
      {
        if (String.IsNullOrEmpty(Title))
          dlg.Title = MainProvider.DisplayName;
        else
          dlg.Title = Title;
        dlg.Filter = Filter;
        dlg.FileName = MainProvider.Text;
        if (String.IsNullOrEmpty(dlg.FileName))
          dlg.InitialDirectory = Directory.Path;
        else
        {
          try
          {
            AbsPath p = new AbsPath(dlg.FileName);
            dlg.InitialDirectory = p.ParentDir.Path;
          }
          catch { }
        }

        if (EFPApp.ShowDialog(dlg) == DialogResult.OK)
        {
          MainProvider.Text = dlg.FileName;
          AbsPath p = new AbsPath(dlg.FileName);
          Directory = p.ParentDir;
          base.OnClick();
        }
      }
    }

    #endregion

    #region Drag-and-drop

    private void InitDragDrop(Control control)
    {
      control.AllowDrop = true;
      control.DragEnter += Control_DragEnter;
      control.DragDrop += Control_DragDrop;
    }

    private void Control_DragEnter(object sender, DragEventArgs args)
    {
      try
      {
        if (args.Data.GetDataPresent(DataFormats.FileDrop))
          args.Effect = DragDropEffects.Link;
      }
      catch { }
    }

    private void Control_DragDrop(object sender, DragEventArgs args)
    {
      try
      {
        DoControl_DragDrop(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPFileDialogButton_Err_DragDrop, e.Message));
      }
    }

    private void DoControl_DragDrop(DragEventArgs args)
    {
      if (!args.Data.GetDataPresent(DataFormats.FileDrop))
        return;

      string[] a = args.Data.GetData(DataFormats.FileDrop) as string[];
      if (a == null)
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_NoFileDrop);
        return;
      }
      if (a.Length != 1)
      {
        EFPApp.ShowTempMessage(Res.EFPFileDialogButton_Err_DragDropMulti);
        return;
      }

      AbsPath path = new AbsPath(a[0]);
      string errorText;
      if (!EFPFileTools.TestFilePath(path.Path,
        TestPathMode.FileExists, // всегда проверяем, что это файл, а не каталог, независимо от свойства PathValidateMode
        out errorText))
      {
        EFPApp.ShowTempMessage(errorText);
        return;
      }

      MainProvider.Text = path.Path;
    }

    #endregion

    #region Буфер обмена

    private void FmtFileDrop_TestFormat(object sender, EFPTestDataObjectEventArgs args)
    {
      args.DataImageKey = "WindowsExplorer";
      if (args.Data.GetDataPresent(DataFormats.FileDrop, false)) // 04.12.2023
        args.Result = EFPTestDataObjectResult.Ok;
    }

    private void FmtFileDrop_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      string[] a = (string[])(args.Data.GetData(DataFormats.FileDrop));
      if (a.Length != 1)
      {
        EFPApp.ShowTempMessage(Res.EFPFileBrowserButton_Err_PasteMulti);
        return;
      }

      AbsPath path = new AbsPath(a[0]);
      string errorText;
      if (!EFPFileTools.TestFilePath(path.Path,
        TestPathMode.FileExists, // всегда проверяем, что это файл, а не каталог, независимо от свойства PathValidateMode
        out errorText))
      {
        EFPApp.ShowTempMessage(errorText);
        return;
      }

      MainProvider.Text = path.Path;
    }

    #endregion
  }

  /// <summary>
  /// Кнопка "Проводник Windows" для просмотра каталогов с помощью Windows
  /// Имя каталога или файла вводится в присоединенном поле ввода или комбоблоке с историей.
  /// Кнопка проводника может (и, обычно, должна, если только поле ввода не используется
  /// исключительно в режиме просмотра (ReadOnly=true)) использоваться совместно с кнопкой "Обзор".
  /// В отличие от <see cref="EFPFolderBrowserButton"/> и <see cref="EFPFileDialogButton"/>, не добавляет к полю вводу обработчики для проверки значения.
  /// </summary>
  public class EFPWindowsExplorerButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="mainProvider">Основной провайдер для поля ввода каталога.
    /// Обычно это <see cref="EFPTextBox"/> или <see cref="EFPHistComboBox"/></param>
    /// <param name="control">Управляющий элемент кнопки</param>
    public EFPWindowsExplorerButton(IEFPTextBox mainProvider, Button control)
      : base(mainProvider.BaseProvider, control)
    {
      _MainProvider = mainProvider;
      SetMainImageKey("WindowsExplorer");
      control.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      VisibleEx = mainProvider.VisibleEx;
      if (EFPApp.IsWindowsExplorerSupported)
      {
        EnabledEx = new DepExpr2<bool, bool, string>(mainProvider.EnabledEx, mainProvider.TextEx,
          new DepFunction2<bool, bool, string>(CalcEnabled));
        base.ToolTipText = String.Format(Res.EFPWindowsExplorerButton_ToolTip_Default, EFPApp.WindowsExplorerDisplayName);
      }
      else
      {
        Enabled = false;
        base.ToolTipText = Res.EFPWindowsExplorerButton_ToolTip_Unsupported;
      }
    }

    #endregion

    #region Свойство Path

    /// <summary>
    /// Текущий выбранный файл.
    /// Если свойство <see cref="MainProvider"/>.Text не определяет корректный путь, свойство возвращает <see cref="AbsPath.Empty"/>.
    /// Свойство PathValidateMode не влияет на это свойство.
    /// </summary>
    public AbsPath Path
    {
      get { return new AbsPath(MainProvider.Text); }
      set
      {
        MainProvider.Text = value.SlashedPath;
        if (_PathEx != null)
          _PathEx.Value = value;
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Path"/>
    /// </summary>
    public DepValue<AbsPath> PathEx
    {
      get
      {
        InitPathEx();
        return _PathEx;
      }
      set
      {
        InitPathEx();
        _PathEx.Source = value;
      }
    }
    private DepInput<AbsPath> _PathEx;

    private void InitPathEx()
    {
      if (_PathEx == null)
      {
        _PathEx = new DepInput<AbsPath>(Path, PathEx_ValueChanged);
        _PathEx.OwnerInfo = new DepOwnerInfo(this, "PathEx");
        MainProvider.TextEx.ValueChanged += MainProvider_TextChanged;
      }
    }

    /// <summary>
    /// При изменениях свойства Text, свойство Path может оставаться без изменений (Empty).
    /// Обратная установка свойства Text приводила бы пользователя в замешательство.
    /// </summary>
    private bool _InsideTextChanged;

    private void MainProvider_TextChanged(object sender, EventArgs args)
    {
      _InsideTextChanged = true;
      try
      {
        _PathEx.Value = Path;
      }
      finally
      {
        _InsideTextChanged = false;
      }
    }

    private void PathEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideTextChanged)
        Path = _PathEx.Value; // Свойство изменено из прикладного кода.
    }

    #endregion

    #region Свойство IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, которое возвращает true, если <see cref="Path"/>.IsEmpty=false.
    /// В отличие от <see cref="MainProvider"/>.IsEmpty, свойство будет возвращать false, если введенный текст задает путь в неправильном формате.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepExpr1<bool, AbsPath>(PathEx, CalcIsNotEmpty);
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(AbsPath path)
    {
      return !path.IsEmpty;
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Провайдер основного поля ввода или комбоблока (задается в конструкторе)
    /// </summary>
    public IEFPTextBox MainProvider { get { return _MainProvider; } }
    private IEFPTextBox _MainProvider;

    /// <summary>
    /// Что вводится в строке: имя файла или имя каталога.
    /// Если свойство не установлено, то при нажатии кнопки определяется автоматически, исходя из
    /// существования файла или каталога и наличия расширении в имени.
    /// Если введенное имя заканчивается на <see cref="System.IO.Path.DirectorySeparatorChar"/>, то свойство игнорируется и
    /// предполагается каталог.
    /// </summary>
    public bool? IsFileName { get { return _IsFileName; } set { _IsFileName = value; } }
    private bool? _IsFileName;

    #endregion

    #region Обработчики

    private static bool CalcEnabled(bool arg1, string arg2)
    {
      if (arg1)
        return (!String.IsNullOrEmpty(arg2));
      else
        return false;
    }

    /// <summary>
    /// Сначала вызывается обработчик события <see cref="EFPButton.Click"/>, если он установлен.
    /// Затем вызывается метод <see cref="EFPApp.ShowWindowsExplorer(AbsPath)"/> для открытия окна проводника
    /// </summary>
    protected override void OnClick()
    {
      string s = MainProvider.Text;
      if (String.IsNullOrEmpty(s))
      {
        EFPApp.ShowTempMessage(Res.EFPWindowsExplorerButton_Err_PathIsEmpty);
        return;
      }

      bool IsSlashed = s[s.Length - 1] == System.IO.Path.DirectorySeparatorChar; // Папка или файл?
      AbsPath path = new AbsPath(s);
      if (!IsSlashed)
      {
        if (IsFileName.HasValue)
        {
          if (IsFileName.Value)
            path = path.ParentDir;
        }
        else
        {
          if (!Directory.Exists(path.Path))
          {
            if (File.Exists(path.Path) || Directory.Exists(path.ParentDir.Path))
              path = path.ParentDir;
            else
            {
              if (!String.IsNullOrEmpty(path.Extension))
                path = path.ParentDir;  // раз есть расширение, значит введено, наверное, имя файла
            }
          }
        }
      }

      if (EFPApp.ShowWindowsExplorer(new AbsPath(path.Path)))
        base.OnClick();
    }

    #endregion
  }
}
