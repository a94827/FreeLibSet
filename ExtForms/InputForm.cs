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
      Value = String.Empty;
      MaxLength = 0;
      CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
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
      if (args.ValidateState == EFPValidateState.Error)
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

  /// <summary>
  /// Диалог ввода текста с возможностью выбора из списка возможных значений
  /// </summary>
  public class ComboTextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public ComboTextInputDialog()
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
      if (args.ValidateState == EFPValidateState.Error)
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
      if (args.ValidateState == EFPValidateState.Error)
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
  /// Блок диалога для ввода целого числа.
  /// </summary>
  public class IntInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public IntInputDialog()
    {
      Title = "Ввод числа";
      Prompt = "Значение";
      NullableValue = null;
      CanBeEmpty = false;
      MinValue = Int32.MinValue;
      MaxValue = Int32.MaxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public int Value
    {
      get { return NullableValue ?? 0; }
      set { NullableValue = value; }
    }

    /// <summary>
    /// Вход и выход: редактируемое значение с возможностью ввода значения null.
    /// Использование null невозможно, если ShowUpDown=true 
    /// </summary>
    public int? NullableValue { get { return _NullableValue; } set { _NullableValue = value; } }
    private int? _NullableValue;

    /// <summary>
    /// Минимальное значение. По умолчанию: Int32.MinValue
    /// </summary>
    public int MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private int _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию: Int32.MaxValue
    /// </summary>
    public int MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private int _MaxValue;

    /// <summary>
    /// Если true, то разрешено использовать Nullable-значение, если false (по умолчанию), то значение должно быть введено
    /// Режим не совместим с установкой свойство ShowUpDown
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        _CanBeEmpty = value;
        if (value)
          _ShowUpDown = false;
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, если число находится вне диапазона
    /// </summary>
    public event EFPValidatingValueEventHandler<int> Validating;

    /// <summary>
    /// Если свойство установлено в true, то значение можно выбирать с помощью стрелочек.
    /// По умолчанию - false - стрелочки не используются.
    /// Реким несовместим с установкой свойства CanBeEmpty
    /// </summary>
    public bool ShowUpDown
    {
      get { return _ShowUpDown; }
      set
      {
        _ShowUpDown = value;
        if (value)
          _CanBeEmpty = false;
      }
    }
    private bool _ShowUpDown;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел число</returns>
    public override DialogResult ShowDialog()
    {
      if (ShowUpDown)
        return ShowDialog2();
      else
        return ShowDialog1();
    }

    /// <summary>
    /// Вывод диалога с NumEditBox
    /// </summary>
    /// <returns></returns>
    private DialogResult ShowDialog1()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      NumEditBox Control = new NumEditBox();
      Control.Dock = DockStyle.Top;
      Control.DecimalPlaces = 0; // только целые числа
      form.MainPanel.Controls.Add(Control);

      EFPNumEditBox efpValue = new EFPNumEditBox(form.FormProvider, Control);
      efpValue.Label = form.MainLabel;
      efpValue.Validating += new EFPValidatingEventHandler(efpValue_Validating);

      if (HasConfig)
      {
        if (CanBeEmpty)
          NullableValue = ConfigPart.GetNullableInt(ConfigName);
        else
          Value = ConfigPart.GetIntDef(ConfigName, Value);
      }
      if (CanBeEmpty)
      {
        efpValue.Control.CanBeEmpty = true;
        efpValue.NullableIntValue = NullableValue;
      }
      else
        efpValue.IntValue = Value;


      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      if (CanBeEmpty)
        NullableValue = efpValue.NullableIntValue;
      else
        Value = efpValue.IntValue;
      if (HasConfig)
      {
        if (CanBeEmpty)
          ConfigPart.SetNullableInt(ConfigName, NullableValue);
        else
          ConfigPart.SetInt(ConfigName, Value);
      }

      return DialogResult.OK;
    }

    /// <summary>
    /// Вывод диалога с ExtNumericUpDown
    /// </summary>
    /// <returns></returns>
    private DialogResult ShowDialog2()
    {
      InputForm form = new InputForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      ExtNumericUpDown Control = new ExtNumericUpDown();
      Control.Dock = DockStyle.Top;
      Control.DecimalPlaces = 0; // только целые числа
      Control.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Right;
      Control.TextAlign = HorizontalAlignment.Right;
      form.MainPanel.Controls.Add(Control);

      EFPExtNumericUpDown efpValue = new EFPExtNumericUpDown(form.FormProvider, Control);
      efpValue.Label = form.MainLabel;
      efpValue.Validating += new EFPValidatingEventHandler(efpValue_Validating);

      if (HasConfig)
      {
        if (CanBeEmpty)
          NullableValue = ConfigPart.GetNullableInt(ConfigName);
        else
          Value = ConfigPart.GetIntDef(ConfigName, Value);
      }

      efpValue.Minimum = MinValue;
      efpValue.Maximum = MaxValue;
      efpValue.IntValue = Value;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      Value = efpValue.IntValue;

      if (HasConfig)
      {
        if (CanBeEmpty)
          ConfigPart.SetNullableInt(ConfigName, NullableValue);
        else
          ConfigPart.SetInt(ConfigName, Value);
      }

      return DialogResult.OK;
    }

    void efpValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
        return;

      if (sender is EFPNumEditBox)
      {
        EFPNumEditBox efpValue = (EFPNumEditBox)sender;

        if (efpValue.NullableIntValue.HasValue)
        {
          if (efpValue.IntValue < MinValue || efpValue.IntValue > MaxValue)
          {
            args.SetError("Число должно быть в диапазоне от " + MinValue.ToString() + " до " + MaxValue.ToString());
            return;
          }
        }
        else
        {
          if (!CanBeEmpty)
            args.SetError("Значение должно быть введено");
          return;
        }

        if (Validating == null)
          return;

        EFPValidatingValueEventArgs<int> Args2 = new EFPValidatingValueEventArgs<int>(args.Validator,
          efpValue.IntValue);

        Validating(this, Args2);
      }
      else
      {
        EFPExtNumericUpDown efpValue = (EFPExtNumericUpDown)sender;

        if (efpValue.IntValue < MinValue || efpValue.IntValue > MaxValue)
        {
          args.SetError("Число должно быть в диапазоне от " + MinValue.ToString() + " до " + MaxValue.ToString());
          return;
        }

        if (Validating == null)
          return;

        EFPValidatingValueEventArgs<int> Args2 = new EFPValidatingValueEventArgs<int>(args.Validator,
          efpValue.IntValue);

        Validating(this, Args2);
      }
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа.
  /// Базовый класс для SingleInputDialog, DoubleInputDialog и DecimalInputDialog
  /// </summary>
  public abstract class BaseFloatInputDialog<T> : BaseInputDialog
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public BaseFloatInputDialog()
    {
      Title = "Ввод числа";
      Prompt = "Значение";
      NullableValue = null;
      CanBeEmpty = false;
      DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public T Value
    {
      get { return NullableValue ?? default(T); }
      set { NullableValue = value; }
    }

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public T? NullableValue { get { return _NullableValue; } set { _NullableValue = value; } }
    private T? _NullableValue;


    /// <summary>
    /// Если true, то разрешено использовать Nullable-значение, если false (по умолчанию), то значение должно быть введено
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Число десятичных знаков после запятой. По умолчанию: (-1) - число десятичных знаков не установлено
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Альтернативная установка свойства DecimalPlaces
    /// </summary>
    public string Format
    {
      get
      {
        return DataTools.DecimalPlacesToNumberFormat(DecimalPlaces);
      }
      set
      {
        DecimalPlaces = DataTools.DecimalPlacesFromNumberFormat(value);
      }
    }

    /// <summary>
    /// Минимальное значение. По умолчанию - минимально возможное значение для своего типа
    /// </summary>
    public T MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private T _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию - максимально возможно значение для своего типа
    /// </summary>
    public T MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private T _MaxValue;

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

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа с десятичной точкой.
  /// </summary>
  public class SingleInputDialog : BaseFloatInputDialog<float>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public SingleInputDialog()
    {
      MinValue = Single.MinValue;
      MaxValue = Single.MaxValue;
    }

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

      NumEditBox Control = new NumEditBox();
      Control.Dock = DockStyle.Top;
      Control.DecimalPlaces = DecimalPlaces;
      form.MainPanel.Controls.Add(Control);

      EFPNumEditBox efpValue = new EFPNumEditBox(form.FormProvider, Control);
      efpValue.Label = form.MainLabel;
      efpValue.Validating += new EFPValidatingEventHandler(efpValue_Validating);

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          efpValue.Control.CanBeEmpty = true;
          NullableValue = ConfigPart.GetNullableSingle(ConfigName);
        }
        else
          Value = ConfigPart.GetSingleDef(ConfigName, Value);
      }
      if (CanBeEmpty)
        efpValue.NullableSingleValue = NullableValue;
      else
        efpValue.SingleValue = Value;


      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      if (CanBeEmpty)
        NullableValue = efpValue.NullableSingleValue;
      else
        Value = efpValue.SingleValue;
      if (HasConfig)
      {
        if (CanBeEmpty)
          ConfigPart.SetNullableSingle(ConfigName, NullableValue);
        else
          ConfigPart.SetSingle(ConfigName, Value);
      }

      return DialogResult.OK;
    }

    void efpValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
        return;

      EFPNumEditBox efpValue = (EFPNumEditBox)sender;

      if (efpValue.NullableSingleValue.HasValue)
      {
        if (efpValue.SingleValue < MinValue || efpValue.SingleValue > MaxValue)
        {
          args.SetError("Число должно быть в диапазоне от " + MinValue.ToString(Format) + " до " + MaxValue.ToString(Format));
          return;
        }
      }
      else
      {
        if (!CanBeEmpty)
          args.SetError("Значение должно быть введено");
        return;
      }

      if (HasValidatingHandler)
      {
        EFPValidatingValueEventArgs<float> Args2 = new EFPValidatingValueEventArgs<float>(args.Validator,
        efpValue.SingleValue);

        OnValidating(Args2);
      }
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа с десятичной точкой.
  /// </summary>
  public class DoubleInputDialog : BaseFloatInputDialog<double>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public DoubleInputDialog()
    {
      MinValue = Double.MinValue;
      MaxValue = Double.MaxValue;
    }

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
      form.MainLabel.Text = Prompt;

      NumEditBox Control = new NumEditBox();
      Control.Dock = DockStyle.Top;
      Control.DecimalPlaces = DecimalPlaces;
      form.MainPanel.Controls.Add(Control);

      EFPNumEditBox efpValue = new EFPNumEditBox(form.FormProvider, Control);
      efpValue.Label = form.MainLabel;
      efpValue.Validating += new EFPValidatingEventHandler(efpValue_Validating);

      if (HasConfig)
      {
        if (CanBeEmpty)
          NullableValue = ConfigPart.GetNullableDouble(ConfigName);
        else
          Value = ConfigPart.GetDoubleDef(ConfigName, Value);
      }
      if (CanBeEmpty)
      {
        efpValue.Control.CanBeEmpty = true;
        efpValue.NullableDoubleValue = NullableValue;
      }
      else
        efpValue.DoubleValue = Value;


      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      if (CanBeEmpty)
        NullableValue = efpValue.NullableDoubleValue;
      else
        Value = efpValue.DoubleValue;
      if (HasConfig)
      {
        if (CanBeEmpty)
          ConfigPart.SetNullableDouble(ConfigName, NullableValue);
        else
          ConfigPart.SetDouble(ConfigName, Value);
      }

      return DialogResult.OK;
    }

    void efpValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
        return;

      EFPNumEditBox efpValue = (EFPNumEditBox)sender;

      if (efpValue.NullableDoubleValue.HasValue)
      {
        if (efpValue.DoubleValue < MinValue || efpValue.DoubleValue > MaxValue)
        {
          args.SetError("Число должно быть в диапазоне от " + MinValue.ToString(Format) + " до " + MaxValue.ToString(Format));
          return;
        }
      }
      else
      {
        if (!CanBeEmpty)
          args.SetError("Значение должно быть введено");
        return;
      }

      if (HasValidatingHandler)
      {
        EFPValidatingValueEventArgs<double> Args2 = new EFPValidatingValueEventArgs<double>(args.Validator,
          efpValue.DoubleValue);

        OnValidating(Args2);
      }
    }

    #endregion
  }

  /// <summary>
  /// Блок диалога для ввода числа с десятичной точкой.
  /// </summary>
  public class DecimalInputDialog : BaseFloatInputDialog<decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public DecimalInputDialog()
    {
      MinValue = Decimal.MinValue;
      MaxValue = Decimal.MaxValue;
    }

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

      NumEditBox Control = new NumEditBox();
      Control.Dock = DockStyle.Top;
      Control.DecimalPlaces = DecimalPlaces;
      form.MainPanel.Controls.Add(Control);

      EFPNumEditBox efpValue = new EFPNumEditBox(form.FormProvider, Control);
      efpValue.Label = form.MainLabel;
      efpValue.Validating += new EFPValidatingEventHandler(efpValue_Validating);

      if (HasConfig)
      {
        if (CanBeEmpty)
          NullableValue = ConfigPart.GetNullableDecimal(ConfigName);
        else
          Value = ConfigPart.GetDecimalDef(ConfigName, Value);
      }
      if (CanBeEmpty)
      {
        efpValue.Control.CanBeEmpty = true;
        efpValue.NullableDecimalValue = NullableValue;
      }
      else
        efpValue.DecimalValue = Value;


      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      if (CanBeEmpty)
        NullableValue = efpValue.NullableDecimalValue;
      else
        Value = efpValue.DecimalValue;
      if (HasConfig)
      {
        if (CanBeEmpty)
          ConfigPart.SetNullableDecimal(ConfigName, NullableValue);
        else
          ConfigPart.SetDecimal(ConfigName, Value);
      }

      return DialogResult.OK;
    }

    void efpValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
        return;

      EFPNumEditBox efpValue = (EFPNumEditBox)sender;

      if (efpValue.NullableDecimalValue.HasValue)
      {
        if (efpValue.DecimalValue < MinValue || efpValue.DecimalValue > MaxValue)
        {
          args.SetError("Число должно быть в диапазоне от " + MinValue.ToString(Format) + " до " + MaxValue.ToString(Format));
          return;
        }
      }
      else
      {
        if (!CanBeEmpty)
          args.SetError("Значение должно быть введено");
        return;
      }

      if (HasValidatingHandler)
      {
        EFPValidatingValueEventArgs<decimal> Args2 = new EFPValidatingValueEventArgs<decimal>(args.Validator,
          efpValue.DecimalValue);

        OnValidating(Args2);
      }
    }

    #endregion
  }


  /// <summary>
  /// Диалог ввода даты.
  /// </summary>
  public class DateInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public DateInputDialog()
    {
      Title = "Дата";
      Prompt = "Значение";
      Value = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public DateTime? Value { get { return _Value; } set { _Value = value; } }
    private DateTime? _Value;

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
    public DateTime? MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private DateTime? _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию ограничение не задано
    /// </summary>
    public DateTime? MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private DateTime? _MaxValue;

    /// <summary>
    /// Если свойство установлено в true, то в диалоге будет не поле ввода даты (DateBox), а календарик (MonthCalendar).
    /// По умолчанию - false.
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
        Value = ConfigPart.GetNullableDate(ConfigName);

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
        efpValue.Minimum = MinValue;
        efpValue.Maximum = MaxValue;

        efpValue.Validating += new EFPValidatingEventHandler(efpValueCal_Validating);
        // невозможно efpValue.CanBeEmpty = CanBeEmpty;

        if (Value.HasValue)
          efpValue.Value = Value.Value;


        switch (EFPApp.ShowDialog(form, true, DialogPosition))
        {
          case DialogResult.OK:
            Value = efpValue.Value;
            break;
          case DialogResult.No:
            Value = null;
            break;
          default:
            return DialogResult.Cancel;
        }
      }
      else
      {
        DateBox Control = new DateBox();
        Control.Dock = DockStyle.Top;

        form.MainPanel.Controls.Add(Control);

        EFPDateBox efpValue = new EFPDateBox(form.FormProvider, Control);
        efpValue.Label = form.MainLabel;
        efpValue.Minimum = MinValue;
        efpValue.Maximum = MaxValue;

        efpValue.Validating += new EFPValidatingEventHandler(efpValueDate_Validating);
        efpValue.CanBeEmpty = CanBeEmpty;
        efpValue.WarningIfEmpty = WarningIfEmpty;

        efpValue.Value = Value;


        if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
          return DialogResult.Cancel;

        Value = efpValue.Value;
      }
      if (HasConfig)
        ConfigPart.SetNullableDate(ConfigName, Value);

      return DialogResult.OK;
    }

    void efpValueDate_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
        return;

      EFPDateBox efpValue = (EFPDateBox)sender;

      if (!efpValue.Value.HasValue)
        return;

      if (Validating == null)
        return;

      EFPValidatingValueEventArgs<DateTime> Args2 = new EFPValidatingValueEventArgs<DateTime>(args.Validator,
        efpValue.Value.Value);

      Validating(this, Args2);
    }

    void efpValueCal_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
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
      efpText.Control.Text = Value;
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
      if (args.ValidateState == EFPValidateState.Error)
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