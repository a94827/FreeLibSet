// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;
using FreeLibSet.Controls;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма для IntRangeDialog, SingleRangeDialog и др. диалогов.
  /// В пользовательском коде следует использовать классы диалогов , а не NumRangeForm
  /// </summary>
  internal partial class NumRangeForm : Form
  {
    #region Конструктор

    /// <summary>
    /// Конструктор формы
    /// </summary>
    public NumRangeForm()
    {
      InitializeComponent();

      FormProvider = new EFPFormProvider(this);

      btn2eq1.Image = EFPApp.MainImages.Images["SignEqual"];
      btn2eq1.ImageAlign = ContentAlignment.MiddleCenter;
      efp2eq1 = new EFPButton(FormProvider, btn2eq1);
      efp2eq1.DisplayName = Res.NumRangeInputDialog_Name_2eq1;
      efp2eq1.ToolTipText = Res.NumRangeInputDialog_ToolTip_2eq1;

      btnNo.Visible = false;
    }

    #endregion

    #region Поля

    public EFPFormProvider FormProvider;

    public bool NoButtonVisible
    {
      get { return btnNo.Visible; }
      set { btnNo.Visible = value; }
    }

    public EFPButton efp2eq1;

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона целых чисел.
  /// Базовый класс для <see cref="Int32RangeDialog"/>, <see cref="SingleRangeDialog"/>, <see cref="DoubleRangeDialog"/> и <see cref="DecimalRangeDialog"/>.
  /// </summary>
  public abstract class BaseNumRangeDialog<T> : BaseInputDialog, IMinMaxSource<T?>
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    internal BaseNumRangeDialog()
    {
      _Format = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога по умолчанию
    /// </summary>
    protected override string DefaultTitle { get { return Res.NumRangeInputDialog_Msg_Title; } }

    /// <summary>
    /// Подсказка по умолчанию
    /// </summary>
    protected override string DefaultPrompt { get { return Res.NumRangeInputDialog_Msg_Prompt; } }

    #region N/First/LastValue

    /// <summary>
    /// Вход и выход: Первое значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public T? NFirstValue
    {
      get { return _NFirstValue; }
      set
      {
        _NFirstValue = value;
        if (_NFirstValueEx != null)
          _NFirstValueEx.OwnerSetValue(NFirstValue);
        if (_FirstValueEx != null)
          _FirstValueEx.OwnerSetValue(FirstValue);
      }
    }
    private T? _NFirstValue;

    /// <summary>
    /// Управляемое свойство для <see cref="NFirstValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<T?> NFirstValueEx
    {
      get
      {
        if (_NFirstValueEx == null)
        {
          _NFirstValueEx = new DepOutput<T?>(NFirstValue);
          _NFirstValueEx.OwnerInfo = new DepOwnerInfo(this, "NFirstValueEx");
        }
        return _NFirstValueEx;
      }
    }
    private DepOutput<T?> _NFirstValueEx;

    /// <summary>
    /// Вход и выход: Первое значение диапазона для закрытого интервала.
    /// Если <see cref="NFirstValue"/>=null, то возвращается значение 0.
    /// </summary>
    public T FirstValue
    {
      get { return NFirstValue ?? default(T); }
      set { NFirstValue = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="FirstValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<T> FirstValueEx
    {
      get
      {
        if (_FirstValueEx == null)
        {
          _FirstValueEx = new DepOutput<T>(FirstValue);
          _FirstValueEx.OwnerInfo = new DepOwnerInfo(this, "FirstValueEx");
        }
        return _FirstValueEx;
      }
    }
    private DepOutput<T> _FirstValueEx;


    /// <summary>
    /// Вход и выход: Последнее значение диапазона с поддержкой полуоткрытых диапазонов.
    /// </summary>
    public T? NLastValue
    {
      get { return _NLastValue; }
      set
      {
        _NLastValue = value;
        if (_NLastValueEx != null)
          _NLastValueEx.OwnerSetValue(NFirstValue);
        if (_LastValueEx != null)
          _LastValueEx.OwnerSetValue(FirstValue);
      }
    }
    private T? _NLastValue;

    /// <summary>
    /// Управляемое свойство для <see cref="NLastValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<T?> NLastValueEx
    {
      get
      {
        if (_NLastValueEx == null)
        {
          _NLastValueEx = new DepOutput<T?>(NLastValue);
          _NLastValueEx.OwnerInfo = new DepOwnerInfo(this, "NLastValueEx");
        }
        return _NLastValueEx;
      }
    }
    private DepOutput<T?> _NLastValueEx;

    /// <summary>
    /// Вход и выход: Последнее значение диапазона для закрытого интервала.
    /// Если <see cref="NLastValue"/>=null, возвращается нулевое значение.
    /// </summary>
    public T LastValue
    {
      get { return NLastValue ?? default(T); }
      set { NLastValue = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="NLastValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<T> LastValueEx
    {
      get
      {
        if (_LastValueEx == null)
        {
          _LastValueEx = new DepOutput<T>(LastValue);
          _LastValueEx.OwnerInfo = new DepOwnerInfo(this, "LastValueEx");
        }
        return _LastValueEx;
      }
    }
    private DepOutput<T> _LastValueEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство возвращает true, если обе даты диапазона заполнены (<see cref="NFirstValue"/>.HasValue=true и <see cref="NLastValue"/>.HasValue=true).
    /// Может использоваться в валидаторах.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepExpr2<bool, T?, T?>(NFirstValueEx, NLastValueEx, CalcIsNotEmptyEx);
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmptyEx(T? firstDate, T? lastDate)
    {
      return firstDate.HasValue && lastDate.HasValue;
    }

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>, при этом полуоткрытые интервалы также запрещены.
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

    #region Диапазон допустимых значений

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
          throw ExceptionFactory.ArgOutOfRange("value", value, default(T), null);

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
    /// <returns><see cref="DialogResult.OK"/>, если пользователь ввел корректные значения</returns>
    public override DialogResult ShowDialog()
    {
      NumRangeForm form = new NumRangeForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.TheGroup.Text = Prompt;
      form.NoButtonVisible = CanBeEmptyMode == UIValidateState.Ok;

      EFPPack p = new EFPPack();
      p.Owner = this;
      p.lblRange = form.lblRange;

      #region Создание управляющих элементов

      p.efpFirstValue = CreateControlProvider(form.FormProvider);
      p.efpFirstValue.Control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      p.efpFirstValue.Control.Location = new Point(form.lblMinimum.Left, form.lblMinimum.Bottom);
      p.efpFirstValue.Control.Size = new Size(form.lblMinimum.Width, form.lblMinimum.Height);
      p.efpFirstValue.Control.Format = Format;
      p.efpFirstValue.Control.UpDownHandler = UpDownHandler;
      p.efpFirstValue.Control.TabIndex = 1;
      form.lblMinimum.Parent.Controls.Add(p.efpFirstValue.Control);

      p.efpFirstValue.Label = form.lblMinimum;
      p.efpFirstValue.CanBeEmpty = CanBeEmpty;
      p.efpFirstValue.Minimum = Minimum;
      p.efpFirstValue.Maximum = Maximum;
      p.efpFirstValue.Validating += new UIValidatingEventHandler(p.efpAnyValue_Validating);

      p.efpLastValue = CreateControlProvider(form.FormProvider);
      p.efpLastValue.Control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      p.efpLastValue.Control.Location = new Point(form.lblMaximum.Left, form.lblMaximum.Bottom);
      p.efpLastValue.Control.Size = new Size(form.lblMaximum.Width, form.lblMaximum.Height);
      p.efpLastValue.Control.Format = Format;
      p.efpLastValue.Control.UpDownHandler = UpDownHandler;
      p.efpLastValue.Control.TabIndex = 3;
      form.lblMaximum.Parent.Controls.Add(p.efpLastValue.Control);

      p.efpLastValue.Label = form.lblMaximum;
      p.efpLastValue.CanBeEmpty = CanBeEmpty;
      p.efpLastValue.Minimum = Minimum;
      p.efpLastValue.Maximum = Maximum;
      p.efpLastValue.Validating += new UIValidatingEventHandler(p.efpAnyValue_Validating);

      p.efpFirstValue.NValueEx.ValueChanged += p.efpLastValue.Validate;
      p.efpLastValue.NValueEx.ValueChanged += p.efpFirstValue.Validate;

      #endregion

      if (HasConfig)
      {
        T? cfgValue1 = ReadConfigValue(ConfigName + "-First");
        if (CanBeEmpty || cfgValue1.HasValue)
          NFirstValue = cfgValue1;
        T? cfgValue2 = ReadConfigValue(ConfigName + "-Last");
        if (CanBeEmpty || cfgValue2.HasValue)
          NLastValue = cfgValue2;
      }
      p.efpFirstValue.NValue = NFirstValue;
      p.efpLastValue.NValue = NLastValue;

      form.efp2eq1.Click += p.efp2eq1_Click;

      switch (EFPApp.ShowDialog(form, true, DialogPosition))
      {
        case DialogResult.OK:
          NFirstValue = p.efpFirstValue.NValue;
          NLastValue = p.efpLastValue.NValue;
          break;

        case DialogResult.No:
          NFirstValue = null;
          NLastValue = null;
          break;

        default:
          return DialogResult.Cancel;
      }

      if (HasConfig)
      {
        WriteConfigValue(ConfigName + "-First", NFirstValue);
        WriteConfigValue(ConfigName + "-Last", NLastValue);
      }

      return DialogResult.OK;
    }

    private class EFPPack/*<T>
        where T : struct, IFormattable, IComparable<T>*/
    {
      #region Поля

      public BaseNumRangeDialog<T> Owner;

      public EFPNumEditBoxBase<T> efpFirstValue;

      public EFPNumEditBoxBase<T> efpLastValue;

      public Label lblRange;

      #endregion

      #region Проверка

      public void efpAnyValue_Validating(object sender, UIValidatingEventArgs args)
      {
        Owner.NFirstValue = efpFirstValue.NValue;
        Owner.NLastValue = efpLastValue.NValue;

        DoValidating(args);
        InitLblRange(args);
      }

      private void DoValidating(UIValidatingEventArgs args)
      {
        if (args.ValidateState == UIValidateState.Error)
          return;

        if (efpFirstValue.NValue.HasValue && efpLastValue.NValue.HasValue)
        {
          if (efpFirstValue.Value.CompareTo(efpLastValue.Value) > 0)
          {
            args.SetError(Res.NumRangeInputDialog_Err_Inverted);
            return;
          }
        }

        if (Owner.HasValidators)
          Owner.Validators.Validate(args);
      }

      private void InitLblRange(UIValidatingEventArgs args)
      {
        if (efpFirstValue.ValidateState == UIValidateState.Error || efpLastValue.ValidateState == UIValidateState.Error)
          lblRange.Text = "Ошибка";
        else
        {
          T? v1 = efpFirstValue.NValue;
          T? v2 = efpLastValue.NValue;
          if (v1.HasValue || v2.HasValue)
            lblRange.Text = FormatStringTools.RangeToString<T>(v1, v2, efpFirstValue.Control.Format, efpFirstValue.Control.FormatProvider);
          else
            lblRange.Text = Res.NumRangeInputDialog_Msg_RangeIsEmpty;
        }
      }

      #endregion

      #region Обработчик для кнопки "="

      public void efp2eq1_Click(object sender, EventArgs args)
      {
        efpLastValue.NValue = efpFirstValue.NValue;
      }

      #endregion
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
    /// <param name="name">Имя для параметра</param>
    /// <param name="value">Значение</param>
    protected abstract void WriteConfigValue(string name, T? value);

    /// <summary>
    /// Читает значение из секции конфигурации
    /// </summary>
    /// <param name="name">Имя для параметра</param>
    /// <returns>Значение</returns>
    protected abstract T? ReadConfigValue(string name);

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона целых чисел
  /// </summary>
  public sealed class Int32RangeDialog : BaseNumRangeDialog<Int32>
  {
    #region Переопределенные методы

    /// <summary>
    /// Создает управляющий элемент и провайдер для него
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPNumEditBoxBase<int> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPIntEditBox(baseProvider, new Int32EditBox());
    }

    /// <summary>
    /// Записывает значение в секцию конфигурации
    /// </summary>
    /// <param name="name">Имя для параметра</param>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(string name, int? value)
    {
      ConfigPart.SetNullableInt32(name, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// <param name="name">Имя для параметра</param>
    /// </summary>
    /// <returns>Значение</returns>
    protected override int? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableInt32(name);
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
  /// Диалог для ввода диапазона чисел с плавающей точкой
  /// </summary>
  public sealed class SingleRangeDialog : BaseNumRangeDialog<Single>
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
    /// <param name="name">Имя для параметра</param>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(string name, float? value)
    {
      ConfigPart.SetNullableSingle(name, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// <param name="name">Имя для параметра</param>
    /// </summary>
    /// <returns>Значение</returns>
    protected override float? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableSingle(name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона чисел с плавающей точкой
  /// </summary>
  public sealed class DoubleRangeDialog : BaseNumRangeDialog<Double>
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
    /// <param name="name">Имя для параметра</param>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(string name, double? value)
    {
      ConfigPart.SetNullableDouble(name, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// <param name="name">Имя для параметра</param>
    /// </summary>
    /// <returns>Значение</returns>
    protected override double? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableDouble(name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона чисел с плавающей точкой
  /// </summary>
  public sealed class DecimalRangeDialog : BaseNumRangeDialog<Decimal>
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
    /// <param name="name">Имя для параметра</param>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(string name, decimal? value)
    {
      ConfigPart.SetNullableDecimal(name, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// <param name="name">Имя для параметра</param>
    /// </summary>
    /// <returns>Значение</returns>
    protected override decimal? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableDecimal(name);
    }

    #endregion
  }
}
