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
      efp2eq1.DisplayName = "Максимальное значение равно минимальному";
      efp2eq1.ToolTipText = "Присваивает полю \"Максимум\" значение из поля \"Минимум\"";

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
  /// Базовый класс для IntRangeDialog, SingleRangeDialog, DoubleRangeDialog и DecimalRangeDialog.
  /// </summary>
  public abstract class BaseNumRangeDialog<T> : BaseInputDialog
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    internal BaseNumRangeDialog()
    {
      Title = "Ввод диапазона чисел";
      Prompt = "Диапазон";
      _Format = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: Первое значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public T? NFirstValue { get { return _NFirstValue; } set { _NFirstValue = value; } }
    private T? _NFirstValue;

    /// <summary>
    /// Вход и выход: Последнее значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public T? NLastValue { get { return _NLastValue; } set { _NLastValue = value; } }
    private T? _NLastValue;

    /// <summary>
    /// Вход и выход: Первое значение диапазона для закрытого интервала
    /// </summary>
    public T FirstValue
    {
      get { return NFirstValue ?? default(T); }
      set { NFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: Последнее значение диапазона для закрытого интервала
    /// </summary>
    public T LastValue
    {
      get { return NLastValue ?? default(T); }
      set { NLastValue = value; }
    }

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
    /// Если true, то разрешено использовать Nullable-значение, если false (по умолчанию), то значение должно быть введено
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set { _CanBeEmpty = value; }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Обработчик для проверки корректности значения при вводе.
    /// Обработчик не вызывается, если число находится вне диапазона
    /// </summary>
    public event EFPValidatingTwoValuesEventHandler<T?, T?> Validating;

    internal void OnValidating(EFPValidatingTwoValuesEventArgs<T?, T?> args)
    {
      Validating(this, args);
    }

    internal bool HasValidatingHandler { get { return Validating != null; } }

    /// <summary>
    /// Если свойство установлено, то в диалоге появляется кнопка "Нет".
    /// При нажатии этой кнопки значения не меняются, а ShowDialog() возвращает DialogResult.No.
    /// По умолчанию - false - диалог не содержит кнопки.
    /// </summary>
    public bool ShowNoButton
    {
      get { return _ShowNoButton; }
      set { _ShowNoButton = value; }
    }
    private bool _ShowNoButton;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значения в полях можно прокручивать с помощью
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
    /// <returns>Ok, если пользователь ввел корректные значения</returns>
    public override DialogResult ShowDialog()
    {
      NumRangeForm form = new NumRangeForm();
      InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.TheGroup.Text = Prompt;
      form.NoButtonVisible = ShowNoButton;

      EFPPack p = new EFPPack();
      p.Owner = this;
      p.lblRange = form.lblRange;

      #region Создание управляющих элементов

      p.efpFirstValue = CreateControlProvider(form.FormProvider);
      p.efpFirstValue.Control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      p.efpFirstValue.Control.Location = new Point(form.lblMinimum.Left, form.lblMinimum.Bottom);
      p.efpFirstValue.Control.Size = new Size(form.lblMinimum.Width, form.lblMinimum.Height);
      p.efpFirstValue.Control.Format = Format;
      p.efpFirstValue.Control.Increment = Increment;
      p.efpFirstValue.Control.TabIndex = 1;
      form.lblMinimum.Parent.Controls.Add(p.efpFirstValue.Control);

      p.efpFirstValue.Label = form.lblMinimum;
      p.efpFirstValue.CanBeEmpty = CanBeEmpty;
      p.efpFirstValue.Minimum = Minimum;
      p.efpFirstValue.Maximum = Maximum;
      p.efpFirstValue.Validating += new EFPValidatingEventHandler(p.efpAnyValue_Validating);

      p.efpLastValue = CreateControlProvider(form.FormProvider);
      p.efpLastValue.Control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      p.efpLastValue.Control.Location = new Point(form.lblMaximum.Left, form.lblMaximum.Bottom);
      p.efpLastValue.Control.Size = new Size(form.lblMaximum.Width, form.lblMaximum.Height);
      p.efpLastValue.Control.Format = Format;
      p.efpLastValue.Control.Increment = Increment;
      p.efpLastValue.Control.TabIndex = 3;
      form.lblMaximum.Parent.Controls.Add(p.efpLastValue.Control);

      p.efpLastValue.Label = form.lblMaximum;
      p.efpLastValue.CanBeEmpty = CanBeEmpty;
      p.efpLastValue.Minimum = Minimum;
      p.efpLastValue.Maximum = Maximum;
      p.efpLastValue.Validating += new EFPValidatingEventHandler(p.efpAnyValue_Validating);

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

      form.efp2eq1.Click+=p.efp2eq1_Click; 

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      NFirstValue = p.efpFirstValue.NValue;
      NLastValue = p.efpLastValue.NValue;
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

      public void efpAnyValue_Validating(object sender, EFPValidatingEventArgs args)
      {
        DoValidating(args);
        InitLblRange(args);
      }

      private void DoValidating(EFPValidatingEventArgs args)
      {
        if (args.ValidateState == UIValidateState.Error)
          return;

          if (efpFirstValue.NValue.HasValue && efpLastValue.NValue.HasValue)
          {
            if (efpFirstValue.Value.CompareTo(efpLastValue.Value) > 0)
            {
              args.SetError("Минимальное значение больше максимального");
              return;
            }
          }

        if (!Owner.HasValidatingHandler)
          return;

        EFPValidatingTwoValuesEventArgs<T?, T?> args2 = new EFPValidatingTwoValuesEventArgs<T?, T?>(args.Validator,
          efpFirstValue.NValue, efpLastValue.NValue);

        Owner.OnValidating(args2);
      }

      private void InitLblRange(EFPValidatingEventArgs args)
      {
        if (efpFirstValue.ValidateState == UIValidateState.Error || efpLastValue.ValidateState == UIValidateState.Error)
          lblRange.Text = "Ошибка";
        else
        {
          T? v1 = efpFirstValue.NValue;
          T? v2 = efpLastValue.NValue;
          if (v1.HasValue && v2.HasValue)
          {
            if (v1.Value.Equals(v2.Value))
              lblRange.Text = v1.Value.ToString(efpFirstValue.Control.Format, efpFirstValue.Control.FormatProvider);
            else
              lblRange.Text = v1.Value.ToString(efpFirstValue.Control.Format, efpFirstValue.Control.FormatProvider) + " - " + v2.Value.ToString(efpLastValue.Control.Format, efpLastValue.Control.FormatProvider);
          }
          else if (v1.HasValue)
            lblRange.Text = "От " + v1.Value.ToString(efpFirstValue.Control.Format, efpFirstValue.Control.FormatProvider);
          else if (v2.HasValue)
            lblRange.Text = "До " + v2.Value.ToString(efpLastValue.Control.Format, efpLastValue.Control.FormatProvider);
          else
            lblRange.Text = "Диапазон не задан";
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
  public sealed class IntRangeDialog : BaseNumRangeDialog<Int32>
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
    /// <param name="name">Имя для параметра</param>
    /// <param name="value">Значение</param>
    protected override void WriteConfigValue(string name, int? value)
    {
      ConfigPart.SetNullableInt(name, value);
    }

    /// <summary>
    /// Читает значение из секции конфигурации
    /// <param name="name">Имя для параметра</param>
    /// </summary>
    /// <returns>Значение</returns>
    protected override int? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableInt(name);
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