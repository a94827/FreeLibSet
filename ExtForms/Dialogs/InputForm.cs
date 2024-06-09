// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Formatting;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма для ввода значения в поле
  /// </summary>
  internal partial class InputForm : Form
  {
    #region Конструктор формы

    public InputForm()
    {
      InitializeComponent();
      EFPApp.InitFormImages(this);

      FormProvider = new EFPFormProvider(this);

      base.MinimumSize = base.Size; // 17.03.2016
    }

    #endregion

    #region Поля

    public EFPFormProvider FormProvider;

    /// <summary>
    /// Максимальная длина текста.
    /// Если свойство установлено, то при показе формы ее размеры могут быть увеличены, чтобы помещался весь текст
    /// </summary>
    public int MaxTextLength;

    public bool FixedSize;

    #endregion

    #region Обработчики формы

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);

      //Screen scr = Screen.FromControl(this);

      if (!FixedSize)
      {
        // Увеличиваем размер текстового поля
        int n = MaxTextLength;
        if (n < 10)
          n = 10;
        if (n > 500)
          n = 500;

        int dx = 0;
        Control mainControl = this.MainPanel.Controls[0];
        try
        {
          using (Graphics gr = this.CreateGraphics())
          {
            gr.PageUnit = GraphicsUnit.Pixel;
            string test = new string('0', n);
            SizeF sz = gr.MeasureString(test, mainControl.Font);
            dx = (int)(sz.Width) - mainControl.ClientSize.Width;
            if (dx < 0)
              dx = 0;
          }

          // Проверяем, что и метка помещается
          Size sz2 = WinFormsTools.GetControlExcess(this.MainLabel.Parent); // там своя панель
          dx = Math.Max(dx, sz2.Width);
        }
        catch { }

        if (dx > 0)
          Width += dx;
      }

      //EFPApp.PlaceFormInScreenCenter(this);
      //this.MaximumSize = new Size(Screen.FromControl(this).WorkingArea.Width, this.Size.Height); // 17.03.2016
      WinFormsTools.PlaceFormInScreenCenter(this, true); // 17.01.2019

      MainPanel.SelectNextControl(null, true, true, true, true);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для диалогов ввода единственного значения, например <see cref="TextInputDialog"/>.
  /// </summary>
  public abstract class BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    protected BaseInputDialog()
    {
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Значок формы в <see cref="EFPApp.MainImages"/>.
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Текст метки-подсказки
    /// </summary>
    public string Prompt { get { return _Prompt; } set { _Prompt = value; } }
    private string _Prompt;

    /// <summary>
    /// Секция конфигурации для хранения значения.
    /// Если свойства <see cref="ConfigPart "/> и <see cref="ConfigName"/> установлены, то значение извлекается перед выводом диалога и записывается обратно при нажатии ОК.
    /// Секция должна допускать запись значений.
    /// </summary>
    public CfgPart ConfigPart { get { return _ConfigPart; } set { _ConfigPart = value; } }
    private CfgPart _ConfigPart;

    /// <summary>
    /// Имя параметра в секции конфигурации <see cref="ConfigPart"/> для хранения значения.
    /// Если свойства <see cref="ConfigPart "/> и <see cref="ConfigName"/> установлены, то значение извлекается перед выводом диалога и записывается обратно при нажатии ОК.
    /// </summary>
    public string ConfigName { get { return _ConfigName; } set { _ConfigName = value; } }
    private string _ConfigName;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно <see cref="EFPApp.DefaultScreen"/>.
    /// Можно либо модифицировать свойства существующего объекта, либо присвоить свойству ссылку на новый объект <see cref="EFPDialogPosition"/>.
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;


    /// <summary>
    /// Контекст справки для формы блока диалога.
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext; }
      set { _HelpContext = value; }
    }
    private string _HelpContext;


    /// <summary>
    /// Список валидаторов элемента, основанных на управляемых значениях
    /// </summary>
    public UIValidatorList Validators
    {
      get
      {
        if (_Valifators == null)
          _Valifators = new UIValidatorList();
        return _Valifators;
      }
    }
    private UIValidatorList _Valifators;

    /// <summary>
    /// Возвращает true, если список <see cref="Validators"/> не пустой.
    /// </summary>
    public bool HasValidators
    {
      get
      {
        if (_Valifators == null)
          return false;
        else
          return _Valifators.Count > 0;
      }
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел корректное значение и нажал кнопку "ОК"</returns>
    public abstract DialogResult ShowDialog();

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Инциализация заголовка формы и значка.
    /// Используется при реализации метода <see cref="ShowDialog()"/>.
    /// </summary>
    /// <param name="form">Объект формы</param>
    protected void InitFormTitle(Form form)
    {
      form.Text = Title;
      EFPApp.MainImages.Icons.InitForm(form, ImageKey, true);
    }

    /// <summary>
    /// Возвращает true, если установлены свойства <see cref="ConfigPart"/> и <see cref="ConfigName"/>.
    /// </summary>
    protected bool HasConfig
    {
      get
      {
        return ConfigPart != null && (!String.IsNullOrEmpty(ConfigName));
      }
    }

#if XXX
    internal static bool ValidateInRange<T>(T value, T? minimum, T? maximum, IEFPValidator validator, string format)
      where T : struct, IFormattable
    {
      if (minimum.HasValue)
      {
        if (maximum.HasValue)
        {
          if (Comparer<T>.Default.Compare(value, minimum.Value) < 0 ||
            Comparer<T>.Default.Compare(value, maximum.Value) > 0)
          {
            validator.SetError("Значение должно быть в диапазоне от " + ((IFormattable)(minimum.Value)).ToString(format, null) +
              " до " + ((IFormattable)(maximum.Value)).ToString(format, null));
            return false;
          }
        }
        else
        {
          if (Comparer<T>.Default.Compare(value, minimum.Value) < 0)
          {
            validator.SetError("Значение должно быть не меньше " + ((IFormattable)(minimum.Value)).ToString(format, null));
            return false;
          }
        }
      }
      else if (maximum.HasValue)
      {
        if (Comparer<T>.Default.Compare(value, maximum.Value) > 0)
        {
          validator.SetError("Значение должно быть не больше " + ((IFormattable)(maximum.Value)).ToString(format, null));
          return false;
        }
      }
      return true;
    }
#endif

    #endregion
  }

  /// <summary>
  /// Диалог для ввода строки текста. Также может использоваться для ввода пароля.
  /// Для ввода многострочного текста используйте <see cref="MultiLineTextInputDialog"/>.
  /// </summary>
  public class TextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public TextInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      _Text = String.Empty;
      _MaxLength = 0;
      _CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Text

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public string Text
    {
      get { return _Text; }
      set
      {
        if (value == null)
          value = String.Empty;
        _Text = value;
        if (_TextEx != null)
          _TextEx.OwnerSetValue(value);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(!String.IsNullOrEmpty(value));
      }
    }
    private string _Text;

    /// <summary>
    /// Управляемое свойство для <see cref="Text"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<string> TextEx
    {
      get
      {
        if (_TextEx == null)
        {
          _TextEx = new DepOutput<string>(Text);
          _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        }
        return _TextEx;
      }
    }
    private DepOutput<string> _TextEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, возвращающее true, если введен непустой текст.
    /// Может использоваться в валидаторах (в предусловии).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepOutput<bool>(!String.IsNullOrEmpty(Text));
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepOutput<bool> _IsNotEmptyEx;

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, возвращается true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region MaxLength

    /// <summary>
    /// Максимальная длина текста.
    /// По умолчанию: 0 - длина текста ограничена 32767 символами символами.
    /// </summary>
    public int MaxLength { get { return _MaxLength; } set { _MaxLength = value; } }
    private int _MaxLength;

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Режим преобразования регистра.
    /// По умолчанию регистр не преобразуется (<see cref="CharacterCasing.Normal"/>).
    /// </summary>
    public CharacterCasing CharacterCasing { get { return _CharacterCasing; } set { _CharacterCasing = value; } }
    private CharacterCasing _CharacterCasing;

    /// <summary>
    /// Если установлено в true, то поле предназначено для ввода пароля. Вводимые символы не отображаются.
    /// </summary>
    public bool IsPassword { get { return _IsPassword; } set { _IsPassword = value; } }
    private bool _IsPassword;

    #endregion

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел текст</returns>
    public override DialogResult ShowDialog()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MaxTextLength = MaxLength;
      form.MainLabel.Text = Prompt;

      TextBox control = new TextBox();
      control.Dock = DockStyle.Top;
      form.MainPanel.Controls.Add(control);

      EFPTextBox efpText = new EFPTextBox(form.FormProvider, control);
      efpText.Label = form.MainLabel;
      efpText.Validating += new UIValidatingEventHandler(efpText_Validating);

      if (MaxLength > 0)
        efpText.MaxLength = MaxLength;

      efpText.CanBeEmptyMode = CanBeEmptyMode;
      control.CharacterCasing = CharacterCasing;
      if (IsPassword)
        control.UseSystemPasswordChar = true;

      if (HasConfig)
      {
        string s2 = ConfigPart.GetString(ConfigName);
        if (!String.IsNullOrEmpty(s2))
          Text = s2;
      }
      efpText.Text = Text;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Text = efpText.Text;
      if (HasConfig)
        ConfigPart.SetString(ConfigName, Text);

      return DialogResult.OK;
    }

    void efpText_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPTextBox efpText = (EFPTextBox)sender;
      this.Text = efpText.Text;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода текста с возможностью выбора из списка значений
  /// </summary>
  public class TextComboInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public TextComboInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      _Text = String.Empty;
      _MaxLength = 0;
      _CanBeEmptyMode = UIValidateState.Error;
      _Items = DataTools.EmptyStrings;
    }

    #endregion

    #region Свойства

    #region Text

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public string Text
    {
      get { return _Text; }
      set
      {
        if (value == null)
          value = String.Empty;
        _Text = value;
        if (_TextEx != null)
          _TextEx.OwnerSetValue(value);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(!String.IsNullOrEmpty(value));
      }
    }
    private string _Text;

    /// <summary>
    /// Управляемое свойство для <see cref="Text"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<string> TextEx
    {
      get
      {
        if (_TextEx == null)
        {
          _TextEx = new DepOutput<string>(Text);
          _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        }
        return _TextEx;
      }
    }
    private DepOutput<string> _TextEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, возвращающее true, если введен непустой текст.
    /// Может использоваться в валидаторах (в предусловии).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepOutput<bool>(!String.IsNullOrEmpty(Text));
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepOutput<bool> _IsNotEmptyEx;

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, то свойство возвращает true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    /// <summary>
    /// Максимальная длина текста.
    /// По умолчанию: 0 - длина текста ограничена 32767 символами символами.
    /// </summary>
    public int MaxLength { get { return _MaxLength; } set { _MaxLength = value; } }
    private int _MaxLength;


    /// <summary>
    /// Список строк, из которых можно выбрать значение.
    /// Пользователь может вводить строки не из списка, это не считается ошибкой.
    /// </summary>
    public string[] Items
    {
      get { return _Items; }
      set
      {
        if (value == null)
          _Items = DataTools.EmptyStrings;
        else
          _Items = value;
      }
    }
    private string[] _Items;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел текст или выбрал его из списка</returns>
    public override DialogResult ShowDialog()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MaxTextLength = MaxLength;
      form.MainLabel.Text = Prompt;

      ComboBox control = new ComboBox();
      control.Dock = DockStyle.Top;
      control.DropDownStyle = ComboBoxStyle.DropDown;
      form.MainPanel.Controls.Add(control);
      control.Items.AddRange(Items);

      EFPTextComboBox efpText = new EFPTextComboBox(form.FormProvider, control);
      efpText.Label = form.MainLabel;
      efpText.Validating += new UIValidatingEventHandler(efpText_Validating);

      if (MaxLength > 0)
        efpText.MaxLength = MaxLength;

      efpText.CanBeEmptyMode = CanBeEmptyMode;

      if (HasConfig)
      {
        string s2 = ConfigPart.GetString(ConfigName);
        if (!String.IsNullOrEmpty(s2))
          Text = s2;
      }
      efpText.Text = Text;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Text = efpText.Text;
      if (HasConfig)
        ConfigPart.SetString(ConfigName, Text);

      return DialogResult.OK;
    }

    void efpText_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPTextComboBox efpText = (EFPTextComboBox)sender;
      this.Text = efpText.Text;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода текста с проверкой ввода по маске
  /// </summary>
  public class MaskedTextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public MaskedTextInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      _Text = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Text

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public string Text
    {
      get { return _Text; }
      set
      {
        if (value == null)
          value = String.Empty;
        _Text = value;
        if (_TextEx != null)
          _TextEx.OwnerSetValue(value);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(!String.IsNullOrEmpty(value));
      }
    }
    private string _Text;

    /// <summary>
    /// Управляемое свойство для <see cref="Text"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<string> TextEx
    {
      get
      {
        if (_TextEx == null)
        {
          _TextEx = new DepOutput<string>(Text);
          _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        }
        return _TextEx;
      }
    }
    private DepOutput<string> _TextEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, возвращающее true, если введен непустой текст.
    /// Может использоваться в валидаторах (в предусловии).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepOutput<bool>(!String.IsNullOrEmpty(Text));
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepOutput<bool> _IsNotEmptyEx;

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, то свойство возвращает true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Mask и MaskProvider

    /// <summary>
    /// Провайдер для обработки маски.
    /// См. описание <see cref="EFPMaskedTextBox.MaskProvider"/>.
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } set { _MaskProvider = value; } }
    private IMaskProvider _MaskProvider;

    /// <summary>
    /// Маска ввода для <see cref="MaskedTextBox"/>.
    /// См. описание <see cref="EFPMaskedTextBox.Mask"/>.
    /// </summary>
    public string Mask { get { return _Mask; } set { _Mask = value; } }
    private string _Mask;

    #endregion

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел текст</returns>
    public override DialogResult ShowDialog()
    {
      if ((!String.IsNullOrEmpty(Mask)) && MaskProvider != null)
        throw new InvalidOperationException("Свойства Mask и MaskProvider не могут задаваться одновременно");

      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      MaskedTextBox control = new MaskedTextBox();
      control.Dock = DockStyle.Top;

      form.MainPanel.Controls.Add(control);

      EFPMaskedTextBox efpText = new EFPMaskedTextBox(form.FormProvider, control);
      efpText.Label = form.MainLabel;

      efpText.Mask = Mask;
      efpText.MaskProvider = MaskProvider;
      if (!String.IsNullOrEmpty(efpText.Mask))
        form.MaxTextLength = efpText.Mask.Length;

      efpText.CanBeEmptyMode = CanBeEmptyMode;
      efpText.Validating += new UIValidatingEventHandler(efpText_Validating);


      if (HasConfig)
      {
        string s2 = ConfigPart.GetString(ConfigName);
        if (!String.IsNullOrEmpty(s2))
          Text = s2;
      }
      efpText.Text = Text;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Text = efpText.Text;
      if (HasConfig)
        ConfigPart.SetString(ConfigName, Text);

      return DialogResult.OK;
    }

    void efpText_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPMaskedTextBox efpText = (EFPMaskedTextBox)sender;
      this.Text = efpText.Text;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода одного числа.
  /// Базовый класс для <see cref="IntInputDialog"/>, <see cref="SingleInputDialog"/>, <see cref="DoubleInputDialog"/> и <see cref="DecimalInputDialog"/>.
  /// </summary>
  public abstract class BaseNumInputDialog<T> : BaseInputDialog, IMinMaxSource<T?>
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public BaseNumInputDialog()
    {
      Title = "Ввод числа";
      Prompt = "Значение";
      _Format = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Value/NValue

    /// <summary>
    /// Вход и выход: редактируемое значение с поддержкой null
    /// </summary>
    public T? NValue
    {
      get { return _NValue; }
      set
      {
        _NValue = value;
        if (_NValueEx != null)
          _NValueEx.OwnerSetValue(value);
        if (_ValueEx != null)
          _ValueEx.OwnerSetValue(Value);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(value.HasValue);
      }
    }
    private T? _NValue;

    /// <summary>
    /// Управляемое свойство для <see cref="NValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<T?> NValueEx
    {
      get
      {
        if (_NValueEx == null)
        {
          _NValueEx = new DepOutput<T?>(NValue);
          _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
        }
        return _NValueEx;
      }
    }
    private DepOutput<T?> _NValueEx;

    /// <summary>
    /// Вход и выход: редактируемое значение без null.
    /// Если <see cref="NValue"/>=null, то возвращается 0.
    /// </summary>
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Value"/>
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<T> ValueEx
    {
      get
      {
        if (_ValueEx == null)
        {
          _ValueEx = new DepOutput<T>(Value);
          _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        }
        return _ValueEx;
      }
    }
    private DepOutput<T> _ValueEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, возвращающее true, если введено значение (<see cref="NValue"/>.HasValue=true).
    /// Может использоваться в валидаторах (в предусловии, когда ввод пустого значения является допустимым).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepOutput<bool>(NValue.HasValue);
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepOutput<bool> _IsNotEmptyEx;

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, свойство возвращает true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа
    /// </summary>
    public string Format
    {
      get { return _Format; }
      set
      {
        if (value == null)
          _Format = String.Empty;
        else
          _Format = value;
      }
    }
    private string _Format;

    /// <summary>
    /// Форматировщик для числового значения
    /// </summary>
    [Browsable(false)]
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве <see cref="Format"/>.
    /// Установка значения свойства создает формат с заданным количеством знаков после запятой.
    /// </summary>
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region Minimum и Maximum

    /// <summary>
    /// Минимальное значение. 
    /// </summary>
    public T? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private T? _Minimum;

    /// <summary>
    /// Максимальное значение.
    /// </summary>
    public T? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private T? _Maximum;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<T?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<T?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public T Increment
    {
      get
      {
        IncrementUpDownHandler<T> incObj = UpDownHandler as IncrementUpDownHandler<T>;
        if (incObj == null)
          return default(T);
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value.CompareTo(default(T)) < 0)
          throw new ArgumentOutOfRangeException("value", value, "Значение должно быть больше или равно 0");

        if (value.CompareTo(default(T)) == 0)
          UpDownHandler = null;
        else
          UpDownHandler = IncrementUpDownHandler<T>.Create(value, this);
      }
    }

    #endregion

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел число</returns>
    public override DialogResult ShowDialog()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      #region Создание управляющего элемента

      EFPNumEditBoxBase<T> efpValue = CreateControlProvider(form.FormProvider);
      efpValue.Control.Dock = DockStyle.Top;
      efpValue.Control.Format = Format;
      efpValue.Control.Increment = Increment;
      form.MainPanel.Controls.Add(efpValue.Control);

      efpValue.Label = form.MainLabel;
      efpValue.CanBeEmpty = CanBeEmpty;
      efpValue.Minimum = Minimum;
      efpValue.Maximum = Maximum;
      efpValue.Validating += new UIValidatingEventHandler(efpValue_Validating);

      #endregion

      if (HasConfig)
      {
        T? cfgValue = ReadConfigValue();
        if (CanBeEmpty || cfgValue.HasValue)
          NValue = cfgValue;
      }
      efpValue.NValue = NValue;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      NValue = efpValue.NValue;
      if (HasConfig)
        WriteConfigValue(NValue);

      return DialogResult.OK;
    }

    void efpValue_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPNumEditBoxBase<T> efpValue = (EFPNumEditBoxBase<T>)sender;
      NValue = efpValue.NValue;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Создает управляющий элемент и провайдер для него
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected abstract EFPNumEditBoxBase<T> CreateControlProvider(EFPBaseProvider baseProvider);

    /// <summary>
    /// Записывает значение в секцию конфигурации
    /// </summary>
    /// <param name="value">Значение</param>
    protected abstract void WriteConfigValue(T? value);

    /// <summary>
    /// Читает значение из секции конфигурации
    /// </summary>
    /// <returns>Значение</returns>
    protected abstract T? ReadConfigValue();

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода целого числа.
  /// </summary>
  public sealed class IntInputDialog : BaseNumInputDialog<Int32>
  {
    #region Переопределенные методы

    /// <summary>
    /// Создает управляющий элемент и провайдер для него
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPNumEditBoxBase<int> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPIntEditBox(baseProvider, new IntEditBox());
    }

    /// <summary>
    /// Записывает значение в секцию конфигурации
    /// </summary>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(int? value)
    {
      ConfigPart.SetNullableInt(ConfigName, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// </summary>
    /// <returns>Значение</returns>
    protected override int? ReadConfigValue()
    {
      return ConfigPart.GetNullableInt(ConfigName);
    }

    /// <summary>
    /// Возвращает 0
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int DecimalPlaces
    {
      get { return 0; }
      set { }
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа с десятичной точкой.
  /// </summary>
  public sealed class SingleInputDialog : BaseNumInputDialog<Single>
  {
    #region Переопределенные методы

    /// <summary>
    /// Создает управляющий элемент и провайдер для него
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPNumEditBoxBase<float> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPSingleEditBox(baseProvider, new SingleEditBox());
    }

    /// <summary>
    /// Записывает значение в секцию конфигурации
    /// </summary>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(float? value)
    {
      ConfigPart.SetNullableSingle(ConfigName, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// </summary>
    /// <returns>Значение</returns>
    protected override float? ReadConfigValue()
    {
      return ConfigPart.GetNullableSingle(ConfigName);
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа с десятичной точкой.
  /// </summary>
  public sealed class DoubleInputDialog : BaseNumInputDialog<Double>
  {
    #region Переопределенные методы

    /// <summary>
    /// Создает управляющий элемент и провайдер для него
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPNumEditBoxBase<double> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPDoubleEditBox(baseProvider, new DoubleEditBox());
    }

    /// <summary>
    /// Записывает значение в секцию конфигурации
    /// </summary>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(double? value)
    {
      ConfigPart.SetNullableDouble(ConfigName, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// </summary>
    /// <returns>Значение</returns>
    protected override double? ReadConfigValue()
    {
      return ConfigPart.GetNullableDouble(ConfigName);
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа с десятичной точкой.
  /// </summary>
  public sealed class DecimalInputDialog : BaseNumInputDialog<Decimal>
  {
    #region Переопределенные методы

    /// <summary>
    /// Создает управляющий элемент и провайдер для него
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPNumEditBoxBase<decimal> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPDecimalEditBox(baseProvider, new DecimalEditBox());
    }

    /// <summary>
    /// Записывает значение в секцию конфигурации
    /// </summary>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(decimal? value)
    {
      ConfigPart.SetNullableDecimal(ConfigName, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// </summary>
    /// <returns>Значение</returns>
    protected override decimal? ReadConfigValue()
    {
      return ConfigPart.GetNullableDecimal(ConfigName);
    }

    #endregion
  }


  /// <summary>
  /// Диалог ввода даты и/или времени с помощью <see cref="DateTimeBox"/> или <see cref="MonthCalendar"/>.
  /// </summary>
  public class DateTimeInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public DateTimeInputDialog()
    {
      Title = "Дата";
      Prompt = "Значение";
      _NValue = null;
      _Formatter = EditableDateTimeFormatters.Date;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Kind, Formatter, UseCalendar

    /// <summary>
    /// Форматизатор для даты/времени.
    /// По умолчанию - стандартный форматизатор для даты.
    /// </summary>
    public EditableDateTimeFormatter Formatter
    {
      get { return _Formatter; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Formatter = value;
      }
    }
    private EditableDateTimeFormatter _Formatter;

    /// <summary>
    /// Альтернативный способ установки свойства <see cref="Formatter"/>
    /// </summary>
    public EditableDateTimeFormatterKind Kind
    {
      get { return _Formatter.Kind; }
      set { _Formatter = EditableDateTimeFormatters.Get(value); }
    }

    /// <summary>
    /// Если свойство установлено в true, то в диалоге будет не поле ввода даты (<see cref="DateTimeBox"/>), а календарик (<see cref="MonthCalendar"/>).
    /// По умолчанию - false.
    /// Свойство нельзя устанавливать, если <see cref="Kind"/> отличается от <see cref="EditableDateTimeFormatterKind.Date"/>", так как <see cref="MonthCalendar"/> не поддерживает компонент времени.
    /// </summary>
    public bool UseCalendar { get { return _UseCalendar; } set { _UseCalendar = value; } }
    private bool _UseCalendar;

    #endregion

    #region Текущее значение

    #region NValue

    /// <summary>
    /// Вход и выход: редактируемое значение с поддержкой null
    /// </summary>
    public DateTime? NValue
    {
      get { return _NValue; }
      set
      {
        _NValue = value;

        if (_NValueEx != null)
          _NValueEx.OwnerSetValue(NValue);
        if (_NValueEx != null)
          _ValueEx.OwnerSetValue(Value);
        if (_NTimeEx != null)
          _NTimeEx.OwnerSetValue(NTime);
        if (_TimeEx != null)
          _TimeEx.OwnerSetValue(Time);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(NValue.HasValue);
      }
    }
    private DateTime? _NValue;

    /// <summary>
    /// Управляемое свойство для <see cref="NValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime?> NValueEx
    {
      get
      {
        if (_NValueEx == null)
        {
          _NValueEx = new DepOutput<DateTime?>(NValue);
          _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
        }
        return _NValueEx;
      }
    }
    private DepOutput<DateTime?> _NValueEx;

    #endregion

    #region Value

    /// <summary>
    /// Вход и выход: редактируемое значение без null.
    /// Если <see cref="NValue"/>=null, то возвращается <see cref="DateTime.MinValue"/>.
    /// </summary>
    public DateTime Value
    {
      get { return NValue ?? DateTime.MinValue; }
      set { NValue = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Value"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime> ValueEx
    {
      get
      {
        if (_ValueEx == null)
        {
          _ValueEx = new DepOutput<DateTime>(Value);
          _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        }
        return _ValueEx;
      }
    }
    private DepOutput<DateTime> _ValueEx;

    #endregion

    #region NTime

    /// <summary>
    /// Доступ к компоненту времени (<see cref="DateTime.TimeOfDay"/>).
    /// Если нет введенного значения (<see cref="NValue"/>=null, а не только не задано время), свойство возвращает null.
    /// </summary>
    public TimeSpan? NTime
    {
      get
      {
        if (NValue.HasValue)
          return NValue.Value.TimeOfDay;
        else
          return null;
      }
      set
      {
        if (value.HasValue)
          NValue = Value.Date /* а не NValue */ + value;
        else
          NValue = null;
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="NTime"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<TimeSpan?> NTimeEx
    {
      get
      {
        if (_NTimeEx == null)
        {
          _NTimeEx = new DepOutput<TimeSpan?>(NTime);
          _NTimeEx.OwnerInfo = new DepOwnerInfo(this, "NTimeEx");
        }
        return _NTimeEx;
      }
    }
    private DepOutput<TimeSpan?> _NTimeEx;

    #endregion

    #region Time

    /// <summary>
    /// Доступ к компоненту времени.
    /// В отличие от <see cref="NTime"/>, это свойство не nullable.
    /// Если <see cref="NTime"/>=null, то возвращается <see cref="TimeSpan.Zero"/>.
    /// </summary>
    public TimeSpan Time
    {
      get { return NTime ?? TimeSpan.Zero; }
      set { NTime = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Time"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<TimeSpan> TimeEx
    {
      get
      {
        if (_TimeEx == null)
        {
          _TimeEx = new DepOutput<TimeSpan>(Time);
          _TimeEx.OwnerInfo = new DepOwnerInfo(this, "TimeEx");
        }
        return _TimeEx;
      }
    }
    private DepOutput<TimeSpan> _TimeEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, возвращающее true, если введено непустое значение (<see cref="NValue"/>.HasValue=false).
    /// Может использоваться в валидаторах (в предусловии).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepOutput<bool>(NValue.HasValue);
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepOutput<bool> _IsNotEmptyEx;

    #endregion

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, то свойство возвращает true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение. По умолчанию ограничение не задано.
    /// </summary>
    public DateTime? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальное значение. По умолчанию ограничение не задано.
    /// </summary>
    public DateTime? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private DateTime? _Maximum;

    #endregion

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел дату</returns>
    public override DialogResult ShowDialog()
    {
      if (HasConfig)
        NValue = ConfigPart.GetNullableDate(ConfigName);

      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      if (UseCalendar)
      {
        form.FixedSize = true;
        MonthCalendar control = new MonthCalendar();
        int dw = control.Width - form.MainPanel.ClientSize.Width;
        int dh = control.Height - form.MainPanel.ClientSize.Height;
        control.Dock = DockStyle.Top;
        form.MainPanel.Controls.Add(control);
        form.Width += dw;
        form.MinimumSize = new Size(form.MinimumSize.Width + dw, form.MinimumSize.Height + dh);
        form.btnNo.Visible = CanBeEmpty; // 27.10.2016
        //if (dh > 0)
        //  form.Height += dh;

        EFPMonthCalendarSingleDay efpValue = new EFPMonthCalendarSingleDay(form.FormProvider, control);
        efpValue.Label = form.MainLabel;
        efpValue.Minimum = Minimum;
        efpValue.Maximum = Maximum;

        efpValue.Validating += new UIValidatingEventHandler(efpValueCal_Validating);
        // невозможно efpValue.CanBeEmpty = CanBeEmpty;

        if (NValue.HasValue)
          efpValue.Value = NValue.Value;


        switch (EFPApp.ShowDialog(form, true, DialogPosition))
        {
          case DialogResult.OK:
            NValue = efpValue.Value;
            break;
          case DialogResult.No:
            NValue = null;
            break;
          default:
            return DialogResult.Cancel;
        }
      }
      else
      {
        DateTimeBox control = new DateTimeBox();
        control.Dock = DockStyle.Top;

        form.MainPanel.Controls.Add(control);

        EFPDateTimeBox efpValue = new EFPDateTimeBox(form.FormProvider, control);
        efpValue.Label = form.MainLabel;
        efpValue.Control.Formatter = Formatter;
        efpValue.CanBeEmptyMode = CanBeEmptyMode;
        efpValue.Minimum = Minimum;
        efpValue.Maximum = Maximum;

        efpValue.Validating += new UIValidatingEventHandler(efpValueDateTime_Validating);

        efpValue.NValue = NValue;

        if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
          return DialogResult.Cancel;

        NValue = efpValue.NValue;
      }
      if (HasConfig)
        ConfigPart.SetNullableDate(ConfigName, NValue);

      return DialogResult.OK;
    }

    void efpValueDateTime_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPDateTimeBox efpValue = (EFPDateTimeBox)sender;
      this.NValue = efpValue.Value;

      if (HasValidators)
        Validators.Validate(args);
    }

    void efpValueCal_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPMonthCalendarSingleDay efpValue = (EFPMonthCalendarSingleDay)sender;
      NValue = efpValue.Value;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода ввода многострочного текста.
  /// В отличие от других диалогов ввода, поддерживается режим "Только для чтения".
  /// </summary>
  public class MultiLineTextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога со значеними по умолчанию
    /// </summary>
    public MultiLineTextInputDialog()
    {
      Title = "Ввод текста";
      _Text = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Text

    /// <summary>
    /// Вход и выход: редактируемый текст. Разделитель строк - <see cref="Environment.NewLine"/>.
    /// </summary>
    public string Text
    {
      get { return _Text; }
      set
      {
        if (value == null)
          value = String.Empty;
        _Text = value;
        if (_TextEx != null)
          _TextEx.OwnerSetValue(value);
        if (_LinesEx != null)
          _LinesEx.OwnerSetValue(Lines);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(!String.IsNullOrEmpty(value));
      }
    }
    private string _Text;

    /// <summary>
    /// Управляемое свойство для <see cref="Text"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<string> TextEx
    {
      get
      {
        if (_TextEx == null)
        {
          _TextEx = new DepOutput<string>(Text);
          _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        }
        return _TextEx;
      }
    }
    private DepOutput<string> _TextEx;

    /// <summary>
    /// Альтернативная установка текста в виде массива строк.
    /// </summary>
    public string[] Lines
    {
      get { return UITools.TextToLines(_Text); }
      set { _Text = UITools.LinesToText(value); }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Lines"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<string[]> LinesEx
    {
      get
      {
        if (_LinesEx == null)
        {
          _LinesEx = new DepOutput<string[]>(Lines);
          _LinesEx.OwnerInfo = new DepOwnerInfo(this, "LinesEx");
        }
        return _LinesEx;
      }
    }
    private DepOutput<string[]> _LinesEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, возвращающее true, если введен непустой текст.
    /// Может использоваться в валидаторах (в предусловии).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepOutput<bool>(!String.IsNullOrEmpty(Text));
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepOutput<bool> _IsNotEmptyEx;

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, то свойство возвращает true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Если true, то форма будет предназначена только для просмотра текста, а не для редактирования.
    /// По умолчанию - false - текст можно редактировать.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Если true, то форма будет выведена на весь экран.
    /// По умолчанию - false - форма имеет размер по умолчанию.
    /// </summary>
    public bool Maximized
    {
      get { return _Maximized; }
      set { _Maximized = value; }
    }
    private bool _Maximized;

    #endregion

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показывает блок диалога
    /// </summary>
    /// <returns>Результат выполния диалога. <see cref="DialogResult.OK"/>, если пользователь ввел текст и нажал "ОК".
    /// Имеет смысл, если <see cref="ReadOnly"/>=false</returns>
    public override DialogResult ShowDialog()
    {
      OKCancelSimpleForm<TextBox> form = new OKCancelSimpleForm<TextBox>(!String.IsNullOrEmpty(Prompt));
      if (form.GroupBox != null)
        form.GroupBox.Text = Prompt;

      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      if (ReadOnly)
        WinFormsTools.OkCancelFormToOkOnly(form);
      EFPApp.SetFormSize(form, 50, 50);
      form.StartPosition = FormStartPosition.CenterScreen;
      if (Maximized)
        form.WindowState = FormWindowState.Maximized;

      EFPTextBox efpText = new EFPTextBox(form.ControlWithToolBar);
      efpText.Control.Multiline = true;
      efpText.Control.ScrollBars = ScrollBars.Both;
      efpText.Control.WordWrap = false;
      efpText.Control.ReadOnly = ReadOnly;
      efpText.Control.Font = EFPApp.CreateMonospaceFont();
      efpText.Control.MaxLength = 0; // 20.09.2017 Убираем ограничение
      efpText.Control.Text = Text;
      efpText.Control.AcceptsReturn = true; // 10.04.2018
      if (String.IsNullOrEmpty(Prompt))
        efpText.DisplayName = "Текст";
      else
        efpText.DisplayName = Prompt; // ?
      efpText.CanBeEmptyMode = CanBeEmptyMode;
      efpText.Validating += new UIValidatingEventHandler(efpText_Validating);

      if (HasConfig)
      {
        string s2 = ConfigPart.GetString(ConfigName);
        if (!String.IsNullOrEmpty(s2))
          Text = s2;
      }
      efpText.Text = Text;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Text = efpText.Text;
      if (HasConfig)
        ConfigPart.SetString(ConfigName, Text);

      return DialogResult.OK;
    }

    void efpText_Validating(object sender, UIValidatingEventArgs args)
    {
      EFPTextBox efpText = (EFPTextBox)sender;
      this.Text = efpText.Text;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }
}
