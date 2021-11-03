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
        Control MainC = this.MainPanel.Controls[0];
        try
        {
          using (Graphics gr = this.CreateGraphics())
          {
            gr.PageUnit = GraphicsUnit.Pixel;
            string test = new string('0', n);
            SizeF sz = gr.MeasureString(test, MainC.Font);
            dx = (int)(sz.Width) - MainC.ClientSize.Width;
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
  /// Базовый класс для диалогов ввода единственного значения, например TextInputDialog
  /// </summary>
  public abstract class BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Защимщенный конструктор
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
    /// Значок формы в EFPApp.MainImages
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Текст метки-подсказки
    /// </summary>
    public string Prompt { get { return _Prompt; } set { _Prompt = value; } }
    private string _Prompt;

    /// <summary>
    /// Если свойства ConfigPart и ConfigName установлены, то значение извлекается перед выводом диалога и записывается обратно при нажатии ОК
    /// </summary>
    public CfgPart ConfigPart { get { return _ConfigPart; } set { _ConfigPart = value; } }
    private CfgPart _ConfigPart;

    /// <summary>
    /// Если свойства ConfigPart и ConfigName установлены, то значение извлекается перед выводом диалога и записывается обратно при нажатии ОК
    /// </summary>
    public string ConfigName { get { return _ConfigName; } set { _ConfigName = value; } }
    private string _ConfigName;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно EFPApp.DefaultScreen.
    /// Можно либо модифицировать свойства существующего объекта, либо присвоить свойству ссылку на новый объект EFPDialogPosition.
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
    /// Возвращает true, если список Validators не пустой.
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
    /// <returns>Ok, если пользователь ввел корректное значение</returns>
    public abstract DialogResult ShowDialog();

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Инциализация заголовка формы и значка.
    /// Используется при реализации метода ShowDialog().
    /// </summary>
    /// <param name="form">Объект формы</param>
    protected void InitFormTitle(Form form)
    {
      form.Text = Title;
      EFPApp.InitMainImageIcon(form, ImageKey, true);
    }

    /// <summary>
    /// Возвращает true, если установлены свойства ConfigPart и ConfigName.
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
  /// Диалог для ввода строки текста.
  /// Для ввода многострочного текста используйте MultiLineTextInputDialog
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
      Text = String.Empty;
      MaxLength = 0;
      CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
    }

    #endregion

    #region Свойства

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
    /// Управляемое свойство для Text
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
    /// Управляемое свойство, возвращающее true, если введен непустой текст.
    /// Может использоваться в валидаторах.
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



    /// <summary>
    /// Максимальная длина текста.
    /// По умолчанию: 0 - длина текста ограничена 32767 символами сим
    /// </summary>
    public int MaxLength { get { return _MaxLength; } set { _MaxLength = value; } }
    private int _MaxLength;

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Надо ли выводить предупреждение, если значение не введено.
    /// По умолчанию - false.
    /// Свойство действует, только если свойство CanBeEmpty=true.
    /// </summary>
    public bool WarningIfEmpty { get { return _WarningIfEmpty; } set { _WarningIfEmpty = value; } }
    private bool _WarningIfEmpty;


    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, пока в поле ввода находится пустая строка    
    /// </summary>
    public event EFPValidatingValueEventHandler<string> Validating;

    /// <summary>
    /// Режим преобразования регистра.
    /// По умолчанию регистр не преобразуется (CharacterCasing.Normal)
    /// </summary>
    public CharacterCasing CharacterCasing { get { return _CharacterCasing; } set { _CharacterCasing = value; } }
    private CharacterCasing _CharacterCasing;

    /// <summary>
    /// Если установлено в true, то поле предназначено для ввода пароля. Вводимые символы не отображаются
    /// </summary>
    public bool IsPassword { get { return _IsPassword; } set { _IsPassword = value; } }
    private bool _IsPassword;

    #endregion

    #region Свойства ErrorRegEx и WarningRegEx

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается сообщение об ошибке, определяемое свойством ErrorRegExMessage.
    /// </summary>
    public string ErrorRegExPattern { get { return _ErrorRegExPattern; } set { _ErrorRegExPattern = value; } }
    private string _ErrorRegExPattern;

    /// <summary>
    /// Текст сообщения об ошибке, которое выводится, если введенное значение не соответствует регулярному выражению ErrorRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string ErrorRegExMessage { get { return _ErrorRegExMessage; } set { _ErrorRegExMessage = value; } }
    private string _ErrorRegExMessage;

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается предупреждение, определяемое свойством WarningRegExMessage.
    /// Проверка не выполняется, если обнаружена ошибка при проверке значения с помощью свойства ErrorRegEx.
    /// </summary>
    public string WarningRegExPattern { get { return _WarningRegExPattern; } set { _WarningRegExPattern = value; } }
    private string _WarningRegExPattern;

    /// <summary>
    /// Текст предупреждения, которое выводится, если введенное значение не соответствует регулярному выражению WarningRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string WarningRegExMessage { get { return _WarningRegExMessage; } set { _WarningRegExMessage = value; } }
    private string _WarningRegExMessage;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел текст</returns>
    public override DialogResult ShowDialog()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MaxTextLength = MaxLength;
      form.MainLabel.Text = Prompt;

      TextBox Control = new TextBox();
      Control.Dock = DockStyle.Top;
      form.MainPanel.Controls.Add(Control);

      EFPTextBox efpText = new EFPTextBox(form.FormProvider, Control);
      efpText.Label = form.MainLabel;
      efpText.Validating += new EFPValidatingEventHandler(efpText_Validating);

      if (MaxLength > 0)
        efpText.MaxLength = MaxLength;

      efpText.CanBeEmpty = CanBeEmpty;
      efpText.WarningIfEmpty = WarningIfEmpty;
      Control.CharacterCasing = CharacterCasing;
      if (IsPassword)
        Control.UseSystemPasswordChar = true;

      efpText.ErrorRegExPattern = this.ErrorRegExPattern;
      efpText.ErrorRegExMessage = this.ErrorRegExMessage;
      efpText.WarningRegExPattern = this.WarningRegExPattern;
      efpText.WarningRegExMessage = this.WarningRegExMessage;

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

    void efpText_Validating(object sender, EFPValidatingEventArgs args)
    {
      EFPTextBox efpText = (EFPTextBox)sender;
      this.Text = efpText.Text;

      if (args.ValidateState == UIValidateState.Error)
        return;

      if (HasValidators)
        EFPControlBase.Validate(Validators, args);

      if (String.IsNullOrEmpty(efpText.Text))
        return;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<string> Args2 = new EFPValidatingValueEventArgs<string>(args.Validator,
        efpText.Text);

      Validating(this, Args2);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода текста с возможностью выбора из списка возможных значений
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
      Value = String.Empty;
      MaxLength = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public string Value { get { return _Value; } set { _Value = value; } }
    private string _Value;

    /// <summary>
    /// Максимальная длина текста.
    /// По умолчанию: 0 - длина текста ограничена 32767 символами сим
    /// </summary>
    public int MaxLength { get { return _MaxLength; } set { _MaxLength = value; } }
    private int _MaxLength;

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Надо ли выводить предупреждение, если значение не введено.
    /// По умолчанию - false.
    /// Свойство действует, только если свойство CanBeEmpty=true.
    /// </summary>
    public bool WarningIfEmpty { get { return _WarningIfEmpty; } set { _WarningIfEmpty = value; } }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Список строк, из которых можно выбрать значение.
    /// </summary>
    public string[] Items { get { return _Items; } set { _Items = value; } }
    private string[] _Items;

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, пока в поле ввода находится пустая строка    
    /// </summary>
    public event EFPValidatingValueEventHandler<string> Validating;

    #endregion

    #region Свойства ErrorRegEx и WarningRegEx

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается сообщение об ошибке, определяемое свойством ErrorRegExMessage.
    /// </summary>
    public string ErrorRegExPattern { get { return _ErrorRegExPattern; } set { _ErrorRegExPattern = value; } }
    private string _ErrorRegExPattern;

    /// <summary>
    /// Текст сообщения об ошибке, которое выводится, если введенное значение не соответствует регулярному выражению ErrorRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string ErrorRegExMessage { get { return _ErrorRegExMessage; } set { _ErrorRegExMessage = value; } }
    private string _ErrorRegExMessage;

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается предупреждение, определяемое свойством WarningRegExMessage.
    /// Проверка не выполняется, если обнаружена ошибка при проверке значения с помощью свойства ErrorRegEx.
    /// </summary>
    public string WarningRegExPattern { get { return _WarningRegExPattern; } set { _WarningRegExPattern = value; } }
    private string _WarningRegExPattern;

    /// <summary>
    /// Текст предупреждения, которое выводится, если введенное значение не соответствует регулярному выражению WarningRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string WarningRegExMessage { get { return _WarningRegExMessage; } set { _WarningRegExMessage = value; } }
    private string _WarningRegExMessage;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел текст или выбрал его из списка</returns>
    public override DialogResult ShowDialog()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MaxTextLength = MaxLength;
      form.MainLabel.Text = Prompt;

      ComboBox Control = new ComboBox();
      Control.Dock = DockStyle.Top;
      Control.DropDownStyle = ComboBoxStyle.DropDown;
      form.MainPanel.Controls.Add(Control);
      if (Items != null)
        Control.Items.AddRange(Items);

      EFPTextComboBox efpText = new EFPTextComboBox(form.FormProvider, Control);
      efpText.Label = form.MainLabel;
      efpText.Validating += new EFPValidatingEventHandler(efpText_Validating);

      if (MaxLength > 0)
        efpText.MaxLength = MaxLength;

      efpText.CanBeEmpty = CanBeEmpty;
      efpText.WarningIfEmpty = WarningIfEmpty;

      efpText.ErrorRegExPattern = this.ErrorRegExPattern;
      efpText.ErrorRegExMessage = this.ErrorRegExMessage;
      efpText.WarningRegExPattern = this.WarningRegExPattern;
      efpText.WarningRegExMessage = this.WarningRegExMessage;

      if (HasConfig)
      {
        string s2 = ConfigPart.GetString(ConfigName);
        if (!String.IsNullOrEmpty(s2))
          Value = s2;
      }
      efpText.Text = Value;


      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Value = efpText.Text;
      if (HasConfig)
        ConfigPart.SetString(ConfigName, Value);

      return DialogResult.OK;
    }

    void efpText_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      EFPTextComboBox efpText = (EFPTextComboBox)sender;

      if (String.IsNullOrEmpty(efpText.Text))
        return;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<string> Args2 = new EFPValidatingValueEventArgs<string>(args.Validator,
        efpText.Text);

      Validating(this, Args2);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода текста с возможностью выбора из списка возможных значений
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
      Value = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public string Value { get { return _Value; } set { _Value = value; } }
    private string _Value;

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Провайдер для обработки маски
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } set { _MaskProvider = value; } }
    private IMaskProvider _MaskProvider;

    /// <summary>
    /// Маска ввода для MaskedTextBox
    /// </summary>
    public string Mask { get { return _Mask; } set { _Mask = value; } }
    private string _Mask;

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, пока в поле ввода находится пустая строка или она не соответствует маске
    /// </summary>
    public event EFPValidatingValueEventHandler<string> Validating;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел текст</returns>
    public override DialogResult ShowDialog()
    {
      if ((!String.IsNullOrEmpty(Mask)) && MaskProvider != null)
        throw new InvalidOperationException("Свойства Mask и MaskProvider не могут задаваться одновременно");

      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      MaskedTextBox Control = new MaskedTextBox();
      Control.Dock = DockStyle.Top;

      form.MainPanel.Controls.Add(Control);

      EFPMaskedTextBox efpText = new EFPMaskedTextBox(form.FormProvider, Control);
      efpText.Label = form.MainLabel;

      efpText.Mask = Mask;
      efpText.MaskProvider = MaskProvider;
      if (!String.IsNullOrEmpty(efpText.Mask))
        form.MaxTextLength = efpText.Mask.Length;

      efpText.Validating += new EFPValidatingEventHandler(efpText_Validating);

      efpText.CanBeEmpty = CanBeEmpty;

      if (HasConfig)
      {
        string s2 = ConfigPart.GetString(ConfigName);
        if (!String.IsNullOrEmpty(s2))
          Value = s2;
      }
      efpText.Text = Value;


      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Value = efpText.Text;
      if (HasConfig)
        ConfigPart.SetString(ConfigName, Value);

      return DialogResult.OK;
    }

    void efpText_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      EFPMaskedTextBox efpText = (EFPMaskedTextBox)sender;

      if (String.IsNullOrEmpty(efpText.Text))
        return;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<string> Args2 = new EFPValidatingValueEventArgs<string>(args.Validator,
        efpText.Text);

      Validating(this, Args2);
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода одного числа.
  /// Базовый класс для IntInputDialog, SingleInputDialog, DoubleInputDialog и DecimalInputDialog
  /// </summary>
  public abstract class BaseNumInputDialog<T> : BaseInputDialog
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
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение с поддержкой null
    /// </summary>
    public T? NValue { get { return _NValue; } set { _NValue = value; } }
    private T? _NValue;

    /// <summary>
    /// Вход и выход: редактируемое значение без null.
    /// </summary>
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }


    /// <summary>
    /// Если true, то разрешено использовать Nullable-значение, если false (по умолчанию), то значение должно быть введено
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

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
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве Format.
    /// Установка значения свойства создает формат.
    /// </summary>
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

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

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, если число находится вне диапазона
    /// </summary>
    public event EFPValidatingValueEventHandler<T> Validating;

    /// <summary>
    /// Вызывает событие Validating
    /// </summary>
    /// <param name="args"></param>
    protected void OnValidating(EFPValidatingValueEventArgs<T> args)
    {
      if (Validating != null)
        Validating(this, args);
    }

    /// <summary>
    /// Возвращает true, если обработчик события Validating установлен.
    /// </summary>
    protected bool HasValidatingHandler { get { return Validating != null; } }

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную
    /// </summary>
    public T Increment
    {
      get { return _Increment; }
      set
      {
        if (value.CompareTo(default(T)) < 0)
          throw new ArgumentOutOfRangeException("value", value, "Значение должно быть больше или равно 0");
        _Increment = value;
      }
    }
    private T _Increment;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел число</returns>
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
      efpValue.Validating += new EFPValidatingEventHandler(efpValue_Validating);

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


    void efpValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      EFPNumEditBoxBase<T> efpValue = (EFPNumEditBoxBase<T>)sender;

      if (HasValidatingHandler && efpValue.NValue.HasValue)
      {
        EFPValidatingValueEventArgs<T> args2 = new EFPValidatingValueEventArgs<T>(args.Validator,
        efpValue.Value);

        OnValidating(args2);
      }
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
  /// Диалог ввода даты.
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
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Форматизатор для даты/времени.
    /// По умолчанию - стандартный форматизатор для даты
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
    /// Альтернативный способ установки свойства Formatter
    /// </summary>
    public EditableDateTimeFormatterKind Kind
    {
      get { return _Formatter.Kind; }
      set { _Formatter = EditableDateTimeFormatters.Get(value); }
    }

    /// <summary>
    /// Вход и выход: редактируемое значение с поддержкой null
    /// </summary>
    public DateTime? NValue { get { return _NValue; } set { _NValue = value; } }
    private DateTime? _NValue;

    /// <summary>
    /// Вход и выход: редактируемое значение без null
    /// </summary>
    public DateTime Value
    {
      get { return NValue ?? DateTime.MinValue; }
      set { NValue = value; }
    }

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false.
    /// Свойство несовместимо с UseCalendar=true.
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Надо ли выводить предупреждение, если значение не введено.
    /// По умолчанию - false.
    /// Свойство действует, только если свойство CanBeEmpty=true.
    /// Свойство несовместимо с UseCalendar=true.
    /// </summary>
    public bool WarningIfEmpty { get { return _WarningIfEmpty; } set { _WarningIfEmpty = value; } }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Минимальное значение. По умолчанию ограничение не задано
    /// </summary>
    public DateTime? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальное значение. По умолчанию ограничение не задано
    /// </summary>
    public DateTime? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private DateTime? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, то в диалоге будет не поле ввода даты (DateBox), а календарик (MonthCalendar).
    /// По умолчанию - false.
    /// Свойство нельзя устанавливать, если Kind отличается от Date.
    /// </summary>
    public bool UseCalendar { get { return _UseCalendar; } set { _UseCalendar = value; } }
    private bool _UseCalendar;

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, пока в поле ввода находится пустая строка
    /// </summary>
    public event EFPValidatingValueEventHandler<DateTime> Validating;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел дату</returns>
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
        MonthCalendar Control = new MonthCalendar();
        int dw = Control.Width - form.MainPanel.ClientSize.Width;
        int dh = Control.Height - form.MainPanel.ClientSize.Height;
        Control.Dock = DockStyle.Top;
        form.MainPanel.Controls.Add(Control);
        form.Width += dw;
        form.MinimumSize = new Size(form.MinimumSize.Width + dw, form.MinimumSize.Height + dh);
        form.btnNo.Visible = CanBeEmpty; // 27.10.2016
        //if (dh > 0)
        //  form.Height += dh;

        EFPMonthCalendarSingleDay efpValue = new EFPMonthCalendarSingleDay(form.FormProvider, Control);
        efpValue.Label = form.MainLabel;
        efpValue.Minimum = Minimum;
        efpValue.Maximum = Maximum;

        efpValue.Validating += new EFPValidatingEventHandler(efpValueCal_Validating);
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
        DateTimeBox Control = new DateTimeBox();
        Control.Dock = DockStyle.Top;

        form.MainPanel.Controls.Add(Control);

        EFPDateTimeBox efpValue = new EFPDateTimeBox(form.FormProvider, Control);
        efpValue.Label = form.MainLabel;
        efpValue.Control.Formatter = Formatter;
        efpValue.Minimum = Minimum;
        efpValue.Maximum = Maximum;

        efpValue.Validating += new EFPValidatingEventHandler(efpValueDate_Validating);
        efpValue.CanBeEmpty = CanBeEmpty;
        efpValue.WarningIfEmpty = WarningIfEmpty;

        efpValue.NValue = NValue;


        if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
          return DialogResult.Cancel;

        NValue = efpValue.NValue;
      }
      if (HasConfig)
        ConfigPart.SetNullableDate(ConfigName, NValue);

      return DialogResult.OK;
    }

    void efpValueDate_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      EFPDateTimeBox efpValue = (EFPDateTimeBox)sender;

      if (!efpValue.NValue.HasValue)
        return;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<DateTime> Args2 = new EFPValidatingValueEventArgs<DateTime>(args.Validator,
        efpValue.NValue.Value);

      Validating(this, Args2);
    }

    void efpValueCal_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      EFPMonthCalendarSingleDay efpValue = (EFPMonthCalendarSingleDay)sender;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<DateTime> Args2 = new EFPValidatingValueEventArgs<DateTime>(args.Validator,
        efpValue.Value);

      Validating(this, Args2);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода ввода многострочного текста.
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
      Text = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемый текст. Разделитель - Environment.NewLine
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;

    /// <summary>
    /// Альтернативная установка текста
    /// </summary>
    public string[] Lines
    {
      get { return UITools.TextToLines(_Text); }
      set { _Text = UITools.LinesToText(value); }
    }

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Надо ли выводить предупреждение, если значение не введено.
    /// По умолчанию - false.
    /// Свойство действует, только если свойство CanBeEmpty=true.
    /// Свойство правктически бесполезно, если не установлено свойство Prompt, так как не будет рамки и не будет видно подсветки
    /// </summary>
    public bool WarningIfEmpty { get { return _WarningIfEmpty; } set { _WarningIfEmpty = value; } }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, пока в поле ввода находится пустая строка    
    /// </summary>
    public event EFPValidatingValueEventHandler<string> Validating;

    /// <summary>
    /// Если true, то форма будет предназначена только для просмотра текста, а не для редактирования.
    /// По умолчанию - false - текст можно редактировать
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Если true, то форма будет выведена на весь экран
    /// По умолчанию - false - форма имеет размер по умолчанию
    /// </summary>
    public bool Maximized
    {
      get { return _Maximized; }
      set { _Maximized = value; }
    }
    private bool _Maximized;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показывает блок диалога
    /// </summary>
    /// <returns>Результат выполния диалога. Ok, если пользователь ввел значение</returns>
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
      efpText.Validating += new EFPValidatingEventHandler(efpText_Validating);

      efpText.CanBeEmpty = CanBeEmpty;
      efpText.WarningIfEmpty = WarningIfEmpty;

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

    void efpText_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      EFPTextBox efpText = (EFPTextBox)sender;

      if (String.IsNullOrEmpty(efpText.Text))
        return;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<string> Args2 = new EFPValidatingValueEventArgs<string>(args.Validator,
        efpText.Text);

      Validating(this, Args2);
    }

    #endregion
  }
}