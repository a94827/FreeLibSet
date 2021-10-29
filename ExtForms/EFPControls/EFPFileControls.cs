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

namespace FreeLibSet.Forms
{
  /*
   * Провайдеры управляющих элементов для полей выбора файлов и каталогов
   */

  /// <summary>
  /// Провайдер комбоблока с историей, предназначенного для выбора каталога на диске.
  /// Обычно этот комбоблок сопровождается кнопкой "Обзор", к которой присоединяется EFPFolderBrowserButton.
  /// Не проверяет наличие каталога на диске.
  /// </summary>
  [Obsolete("Используйте класс EFPHistComboBox", false)]
  public class EFPFolderHistComboBox : EFPHistComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPFolderHistComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка введенного значения.
    /// Кроме проверки пустого значения, выполняемой базовым классом, проверяет наличие недопустимых символов.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return;

      if (String.IsNullOrEmpty(Text))
        return;

      int p = Text.IndexOfAny(Path.GetInvalidPathChars());
      if (p >= 0)
        SetError("Недопустимый символ \"" + Text.Substring(p, 1) + "\" в позиции " + (p + 1).ToString());
    }

    #endregion
  }

  /// <summary>
  /// Провайдер комбоблока с историей, предназначенного для выбора файла на диске.
  /// Обычно этот комбоблок сопровождается кнопкой "Обзор", к которой присоединяется EFPFileDialogButton.
  /// Не проверяет наличие файла на диске.
  /// </summary>
  [Obsolete("Используйте класс EFPHistComboBox", false)]
  public class EFPFileHistComboBox : EFPHistComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPFileHistComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка введенного значения.
    /// Кроме проверки пустого значения, выполняемой базовым классом, проверяет наличие недопустимых символов.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return;

      if (String.IsNullOrEmpty(Text))
        return;

      string DirName = String.Empty;
      string FileName = Text;
      int p1 = Text.LastIndexOf('\\');
      if (p1 >= 0)
      {
        DirName = Text.Substring(0, p1);
        FileName = Text.Substring(p1 + 1);
      }
      int p = DirName.IndexOfAny(Path.GetInvalidPathChars());
      if (p >= 0)
        SetError("Недопустимый символ \"" + Text.Substring(p, 1) + "\" в позиции " + (p + 1).ToString());
      else
      {
        p = FileName.IndexOfAny(Path.GetInvalidFileNameChars());
        if (p >= 0)
          SetError("Недопустимый символ \"" + Text.Substring(p, 1) + "\" в позиции " + (DirName.Length + p + 1).ToString());

      }
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для EFPFilePathValidator и EFPFolderPathValidator
  /// </summary>
  public abstract class EFPPathValidatorBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта
    /// </summary>
    /// <param name="controlProvider">Провайдер основного управляющего элемента (текстового поля или комбоблока). Не может быть</param>
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
    /// Если свойство не установлено в явном виде, возвращает значение свойства AmbientPathValidateMode.
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
    /// Режим проверки по умолчанию, используемый, если свойство PathValidateMode не установлено в явном виде.
    /// Провайдеры кнопки "Обзор" EFPFileDialogMode и EFPFolderBrowseButton устанавливают это свойство, в зависимости
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
    /// Сбрасывает свойство PathValidateMode в исходное состояние, равное AmbientPathValidateMode.
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
    /// Выполняет проверку при установке свойств PathValidateMode и AmbientPathValidateMode.
    /// Должен выбрасывать исключение, если устанавливается недопустимое значение
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

    void ControlProvider_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (_InsideControlProvider_Validating)
        args.SetError("Предыдущая проверка еще не закончена");
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

      // Регион полная проверка
      _LastValidatedText = _ControlProvider.Text;
      if (Validate(_LastValidatedText, PathValidateMode, out _LastValidateMessage))
        _LastValidateMessage = String.Empty; // а не null;
      else
        _ControlProvider.SetError(_LastValidateMessage);
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
  /// Обычно используется провайдер кнопки "Обзор" EFPFileDIalogButton, который использует внутреннюю копию валидатора.
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
    /// Для поля добавляется обработчик события Validate и устанавливается свойство ValidateWhenFocusChanged=true.
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
    /// Вызывает EFPFileTools.TestFilePath() для проверки
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
  /// Обычно используется провайдер кнопки "Обзор" EFPFileDIalogButton, который использует внутреннюю копию валидатора.
  /// Прикладной код может использовать этот объект, если имеется поле ввода, но нет кнопки "Обзор".
  /// Может задаваться как абсолютный, так и относительный путь.
  /// Имя каталога в Windows должно заканчиваться обратной косой чертой.
  /// Проверка формата пути выполняется при каждом изменении текста. 
  /// Проверка существования пути или его части выполняется только при открытии/закрытии формы или при получении/потере
  /// фокуса ввода элементом. 
  /// </summary>
  public class EFPFolderPathValidator : EFPPathValidatorBase
  {
    #region Конструктор

    /// <summary>
    /// Создает экземпляр валидатора и присоединяет его к текстовому полю.
    /// Для поля добавляется обработчик события Validate и устанавливается свойство ValidateWhenFocusChanged=true.
    /// </summary>
    /// <param name="controlProvider">Провайдер основного управляющего элемента (текстового поля или комбоблока). 
    /// Не может быть null</param>
    public EFPFolderPathValidator(IEFPSimpleTextBox controlProvider)
      : base(controlProvider)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выбрасывает исключение для <paramref name="value"/>=FileExists.
    /// </summary>
    /// <param name="value">Проверяемый режим</param>
    protected override void CheckValidateMode(TestPathMode value)
    {
      base.CheckValidateMode(value);
      if (value == TestPathMode.FileExists)
        throw new ArgumentException("Режим проверки " + value.ToString() + " не допускается для проверки каталога");
    }

    /// <summary>
    /// Вызывает EFPFileTools.TestDirSlashedPath() для проверки
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
  /// Кнопка "Обзор" для выбора каталога, имя которого вводится в поле ввода
  /// или комбоблоке с историей.
  /// Кнопка также может использоваться совместно с текстовым полем только для чтения, когда обработка каталога должна выполнятся 
  /// после выбора каталога, а не при закрытии формы.
  /// Событие EFPButton.Click вызывается только после того, как пользователь выбрал файл.
  /// </summary>
  public class EFPFolderBrowserButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента кнопки
    /// </summary>
    /// <param name="mainProvider">Провайдер основного управляющего элемента,
    /// предназначенного для ввода пути. Обычно это EFPTextBox или EFPFolderHistComboBox</param>
    /// <param name="control">Управляющий элемент - кнопка "Обзор"</param>
    public EFPFolderBrowserButton(IEFPTextBox mainProvider, Button control)
      : base(mainProvider.BaseProvider, control)
    {
      _MainProvider = mainProvider;
      SetMainImageKey("Open");
      VisibleEx = mainProvider.VisibleEx;
      EnabledEx = mainProvider.EnabledEx;
      base.ToolTipText = "Выбор папки с помощью стандартного блока диалога Windows";

      _PathValidator = new EFPFolderPathValidator(mainProvider);
      _PathValidator.AmbientPathValidateMode = TestPathMode.DirectoryExists;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер основного поля ввода иди комбоблока (задается в конструкторе)
    /// </summary>
    public IEFPTextBox MainProvider { get { return _MainProvider; } }
    private IEFPTextBox _MainProvider;

    /// <summary>
    /// Описание, появляющееся в блоке диалога выбора папки при нажатии кнопки
    /// Если свойство не задано, то в качестве пояснения будет использовано
    /// MainProvider.DisplayName
    /// </summary>
    public string Description { get { return _Description; } set { _Description = value; } }
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

    #endregion

    #region Проверка введенного текста

    private EFPFolderPathValidator _PathValidator;

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства ShowNewFolderButton. При ShowNewFolderButton=true возвращает RootExists,
    /// а при false - DirectoryExists
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get { return _PathValidator.PathValidateMode; }
      set { _PathValidator.PathValidateMode = value; }
    }

    /// <summary>
    /// Сбрасывает свойство PathValidateMode в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidator.ResetPathValidateMode();
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Выводит стандартный блок диалога выбора каталога FolderBrowserDialog.
    /// Если пользователь закрыл диалог кнопкой "ОК", устанавливается свойство MainProvider.Text.
    /// Затем вызывается обработчик события EFPButton.Click.
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
        AbsPath dir = new AbsPath(MainProvider.Text);
        dlg.SelectedPath = dir.Path;
        if (EFPApp.ShowDialog(dlg) == DialogResult.OK)
        {
          AbsPath Path = new AbsPath(dlg.SelectedPath);
          MainProvider.Text = Path.SlashedPath;
          base.OnClick();
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Кнопка "Обзор" для выбора файла, имя которого вводится в поле ввода
  /// или комбоблоке с историей.
  /// Кнопка также может использоваться совместно с текстовым полем только для чтения, когда обработка файла должна выполнятся 
  /// после выбора файла, а не при закрытии формы.
  /// Событие EFPButton.Click вызывается только после того, как пользователь выбрал файл.
  /// </summary>
  public class EFPFileDialogButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента кнопки
    /// </summary>
    /// <param name="mainProvider">Провайдер основного управляющего элемента,
    /// предназначенного для ввода пути. Обычно это EFPTextBox или EFPFileHistComboBox</param>
    /// <param name="control">Управляющий элемент - кнопка "Обзор"</param>
    public EFPFileDialogButton(IEFPTextBox mainProvider, Button control)
      : base(mainProvider.BaseProvider, control)
    {
      _MainProvider = mainProvider;
      SetMainImageKey("Open");
      VisibleEx = mainProvider.VisibleEx;
      EnabledEx = mainProvider.EnabledEx;
      base.ToolTipText = "Выбор файла с помощью стандартного блока диалога Windows";

      _PathValidator = new EFPFilePathValidator(mainProvider);
      Mode = FileDialogMode.Read;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер основного поля ввода иди комбоблока (задается в конструкторе)
    /// </summary>
    public IEFPTextBox MainProvider { get { return _MainProvider; } }
    private IEFPTextBox _MainProvider;

    /// <summary>
    /// Описание, появляющееся в блоке диалога выбора папки при нажатии кнопки
    /// Если свойство не задано, то в качестве пояснения будет использовано
    /// MainProvider.DisplayName
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Значение для свойства Filter (с разделителями "|")
    /// </summary>
    public string Filter { get { return _Filter; } set { _Filter = value; } }
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
    /// При открытии диалога используется в качестве свойства FileDialog.InitialDirectory,
    /// если в поле ввода не задано имя файла.
    /// После того, как файл выбран в диалоге, содержит каталог, соответствующий FileDialog.FileName
    /// </summary>
    public AbsPath Directory
    {
      get { return _Directory; }
      set { _Directory = value; }
    }
    private AbsPath _Directory;

    #endregion

    #region Проверка введенного текста

    private EFPFilePathValidator _PathValidator;

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства Mode. При Mode=Write возвращает RootExists,
    /// а при Read - FileExists
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get { return _PathValidator.PathValidateMode; }
      set { _PathValidator.PathValidateMode = value; }
    }

    /// <summary>
    /// Сбрасывает свойство PathValidateMode в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidator.ResetPathValidateMode();
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Выводит стандартный блок диалога выбора файла OpenFileDialog или SaveFileDialog,
    /// в зависимости от свойства Mode. Свойство FileName устанавливается равным текущему значению в поле ввода.
    /// Если диалог закрыт нажатием кнопки "ОК" (файл выбран пользователем), устанавливается свойство MainProvider.Text.
    /// Затем вызывается обработчик события EFPButton.Click.
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
  }

  /// <summary>
  /// Кнопка "Проводник Windows" для просмотра каталогов с помощью Windows
  /// Имя каталога или файла вводится в присоединенном поле ввода
  /// или комбоблоке с историей.
  /// Кнопка проводника может (и, обычно, должна, если только поле ввода не используется
  /// исключительно в режиме просмотра (ReadOnly=true)) использоваться совместно с кнопкой "Обзор"
  /// и провайдером EFPFolderBrowserButton.
  /// </summary>
  public class EFPWindowsExplorerButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="mainProvider">Основной провайдер для поля ввода каталога.
    /// Обычно это EFPTextBox или EFPFolderHistComboBox</param>
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
        base.ToolTipText = "Открытие выбранной папки с помощью Проводника Windows";
      }
      else
      {
        Enabled = false;
        base.ToolTipText = "Открытие выбранной папки не поддерживается операционной системой";
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер основного поля ввода иди комбоблока (задается в конструкторе)
    /// </summary>
    public IEFPTextBox MainProvider { get { return _MainProvider; } }
    private IEFPTextBox _MainProvider;

    /// <summary>
    /// Что вводится в строке: имя файла или имя каталога.
    /// Если свойство не установлено, то определяется автоматически, исходя из
    /// существования файла или каталога и наличия расширении в имени.
    /// Если введенное имя заканчивается на "\", то свойство игнорируется и
    /// предполагается каталог
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
    /// Сначала вызывается обработчик события EFPButton.Click, если он установлен.
    /// Затем вызывается метод EFPApp.ShowWindowsExplorer() для открытия окна проводника
    /// </summary>
    protected override void OnClick()
    {
      string s = MainProvider.Text;
      if (String.IsNullOrEmpty(s))
      {
        EFPApp.ShowTempMessage("Имя не задано");
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
