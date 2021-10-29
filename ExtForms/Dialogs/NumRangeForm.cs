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

      efpFirstValue = new EFPNumEditBox(FormProvider, edFirstValue);
      efpLastValue = new EFPNumEditBox(FormProvider, edLastValue);

      efpFirstValue.Tag = this; // для доступа из обработчиков события Validating
      efpLastValue.Tag = this;

      efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      efpFirstValue.DecimalValueEx.ValueChanged += new EventHandler(efpLastValue.Validate);
      efpLastValue.DecimalValueEx.ValueChanged += new EventHandler(efpFirstValue.Validate);

      btn2eq1.Image = EFPApp.MainImages.Images["SignEqual"];
      btn2eq1.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efp2eq1 = new EFPButton(FormProvider, btn2eq1);
      efp2eq1.DisplayName = "Максимальное значение равно минимальному";
      efp2eq1.ToolTipText = "Присваивает полю \"Максимум\" значение из поля \"Минимум\"";
      efp2eq1.Click += new EventHandler(efp2eq1_Click);

      btnNo.Visible = false;
      efpAnyValue_Validating(null, null);
    }

    //void Other_ValueChanged(object sender, EventArgs args)
    //{
    //  efpLastValue.Validate();
    //}

    void efpAnyValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (sender != null)
      {
        if (args.ValidateState != UIValidateState.Error)
        {
          if (efpFirstValue.NDecimalValue.HasValue && efpLastValue.NDecimalValue.HasValue)
          {
            decimal v1 = efpFirstValue.DecimalValue;
            decimal v2 = efpLastValue.DecimalValue;
            if (v1 > v2)
              args.SetError("Минимальное значение больше максимального");
          }
        }
      }

      if (efpFirstValue.ValidateState == UIValidateState.Error || efpLastValue.ValidateState == UIValidateState.Error)
        lblRange.Text = "Ошибка";
      else
      {
        decimal? v1 = efpFirstValue.NDecimalValue;
        decimal? v2 = efpLastValue.NDecimalValue;
        if (v1.HasValue && v2.HasValue)
        {
          if (v1.Value == v2.Value)
            lblRange.Text = v1.Value.ToString(efpFirstValue.Control.Format);
          else
            lblRange.Text = v1.Value.ToString(efpFirstValue.Control.Format) + " - " + v2.Value.ToString(efpLastValue.Control.Format);
        }
        else if (v1.HasValue)
          lblRange.Text = "От " + v1.Value.ToString(efpFirstValue.Control.Format);
        else if (v2.HasValue)
          lblRange.Text = "До " + v2.Value.ToString(efpLastValue.Control.Format);
        else
          lblRange.Text = "Диапазон не задан";
      }
    }

    void efp2eq1_Click(object sender, EventArgs args)
    {
      if (efpLastValue.Control.CanBeEmpty)
        efpLastValue.NDecimalValue = efpFirstValue.NDecimalValue;
      else
        efpLastValue.DecimalValue = efpFirstValue.DecimalValue;
    }

    #endregion

    #region Поля

    public EFPFormProvider FormProvider;

    public EFPNumEditBox efpFirstValue, efpLastValue;

    public bool NoButtonVisible
    {
      get { return btnNo.Visible; }
      set { btnNo.Visible = value; }
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона целых чисел
  /// </summary>
  public sealed class IntRangeDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога с параметрами по умолчанию
    /// </summary>
    public IntRangeDialog()
    {
      Title = "Ввод диапазона чисел";
      Prompt = "Диапазон";
      _CanBeEmpty = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: Первое значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public int? NFirstValue { get { return _NFirstValue; } set { _NFirstValue = value; } }
    private int? _NFirstValue;

    /// <summary>
    /// Вход и выход: Последнее значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public int? NLastValue { get { return _NLastValue; } set { _NLastValue = value; } }
    private int? _NLastValue;

    /// <summary>
    /// Вход и выход: Первое значение диапазона для закрытого интервала
    /// </summary>
    public int FirstValue
    {
      get { return NFirstValue ?? 0; }
      set { NFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: Последнее значение диапазона для закрытого интервала
    /// </summary>
    public int LastValue
    {
      get { return NLastValue ?? 0; }
      set { NLastValue = value; }
    }

    /// <summary>
    /// Минимальное значение.
    /// </summary>
    public int? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private int? _Minimum;

    /// <summary>
    /// Максимальное значение. 
    /// </summary>
    public int? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private int? _Maximum;

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
    public event EFPValidatingTwoValuesEventHandler<int?, int?> Validating;

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

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел корректные значения</returns>
    public override DialogResult ShowDialog()
    {
      DialogResult Res;
      NumRangeForm TheForm = new NumRangeForm();
      try
      {
        InitFormTitle(TheForm);
        TheForm.FormProvider.HelpContext = HelpContext;
        TheForm.TheGroup.Text = Prompt;
        TheForm.NoButtonVisible = ShowNoButton;
        TheForm.efpFirstValue.Control.CanBeEmpty = CanBeEmpty;
        TheForm.efpLastValue.Control.CanBeEmpty = CanBeEmpty;

        if (HasConfig)
        {
          if (CanBeEmpty)
          {
            NFirstValue = ConfigPart.GetNullableInt(ConfigName + "-First");
            NLastValue = ConfigPart.GetNullableInt(ConfigName + "-Last");
          }
          else
          {
            FirstValue = ConfigPart.GetIntDef(ConfigName + "-First", FirstValue);
            LastValue = ConfigPart.GetIntDef(ConfigName + "-Last", LastValue);
          }
        }

        if (CanBeEmpty)
        {
          TheForm.efpFirstValue.NIntValue = NFirstValue;
          TheForm.efpLastValue.NIntValue = NLastValue;
        }
        else
        {
          TheForm.efpFirstValue.IntValue = FirstValue;
          TheForm.efpLastValue.IntValue = LastValue;
        }

        if (Minimum.HasValue)
        {
          TheForm.efpFirstValue.Minimum = (decimal)(Minimum.Value);
          TheForm.efpLastValue.Minimum = (decimal)(Minimum.Value);
        }
        if (Maximum.HasValue)
        {
          TheForm.efpFirstValue.Maximum = (decimal)(Maximum.Value);
          TheForm.efpLastValue.Maximum = (decimal)(Maximum.Value);
        }
        TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
        TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

        Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

        if (Res == DialogResult.OK)
        {
          if (CanBeEmpty)
          {
            NFirstValue = TheForm.efpFirstValue.NIntValue;
            NLastValue = TheForm.efpLastValue.NIntValue;
          }
          else
          {
            FirstValue = TheForm.efpFirstValue.IntValue;
            LastValue = TheForm.efpLastValue.IntValue;
          }
          if (HasConfig)
          {
            if (CanBeEmpty)
            {
              ConfigPart.SetNullableInt(ConfigName + "-First", NFirstValue);
              ConfigPart.SetNullableInt(ConfigName + "-Last", NLastValue);
            }
            else
            {
              ConfigPart.SetInt(ConfigName + "-First", FirstValue);
              ConfigPart.SetInt(ConfigName + "-Last", LastValue);
            }
          }
        }
      }
      finally
      {
        TheForm.Dispose();
        TheForm = null;
      }

      return Res;
    }

    void efpAnyValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      if (Validating == null)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);

      int? v1 = TheForm.efpFirstValue.NIntValue;
      int? v2 = TheForm.efpLastValue.NIntValue;

      EFPValidatingTwoValuesEventArgs<int?, int?> Args2 = new EFPValidatingTwoValuesEventArgs<int?, int?>(args.Validator,
        v1, v2);

      Validating(this, Args2);
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона целых чисел.
  /// Базовый класс для SingleRangeDialog, DoubleRangeDialog и DecimalRangeDialog.
  /// В пользовательском коде нельзя создавать собственные производные классы.
  /// </summary>
  public abstract class BaseFloatRangeDialog<T> : BaseInputDialog
    where T : struct
  {
    #region Конструктор

    internal BaseFloatRangeDialog()
    {
      Title = "Ввод диапазона чисел";
      Prompt = "Диапазон";
      _NFirstValue = null;
      _NLastValue = null;
      _CanBeEmpty = false;
      _DecimalPlaces = -1;
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
        return FormatStringTools.DecimalPlacesToNumberFormat(DecimalPlaces);
      }
      set
      {
        DecimalPlaces = FormatStringTools.DecimalPlacesFromNumberFormat(value);
      }
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

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Показ блока диалога.
    /// </summary>
    /// <returns>Ok, если пользователь ввел корректные значения</returns>
    public override DialogResult ShowDialog()
    {
      DialogResult Res;
      NumRangeForm TheForm = new NumRangeForm();
      try
      {
        InitFormTitle(TheForm);
        TheForm.FormProvider.HelpContext = HelpContext;
        TheForm.TheGroup.Text = Prompt;
        TheForm.efpFirstValue.Control.DecimalPlaces = DecimalPlaces;
        TheForm.efpLastValue.Control.DecimalPlaces = DecimalPlaces;
        TheForm.efpFirstValue.Control.CanBeEmpty = CanBeEmpty;
        TheForm.efpLastValue.Control.CanBeEmpty = CanBeEmpty;

        TheForm.NoButtonVisible = ShowNoButton;

        Res = DoShowDialog(TheForm);

      }
      finally
      {
        TheForm.Dispose();
      }

      return Res;
    }

    internal abstract DialogResult DoShowDialog(object formObj);

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона чисел с плавающей точкой
  /// </summary>
  public sealed class SingleRangeDialog : BaseFloatRangeDialog<Single>
  {
    #region Запуск диалога

    internal override DialogResult DoShowDialog(object formObj)
    {
      NumRangeForm TheForm = (NumRangeForm)formObj;

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          NFirstValue = ConfigPart.GetNullableSingle(ConfigName + "-First");
          NLastValue = ConfigPart.GetNullableSingle(ConfigName + "-Last");
        }
        else
        {
          FirstValue = ConfigPart.GetSingleDef(ConfigName + "-First", FirstValue);
          LastValue = ConfigPart.GetSingleDef(ConfigName + "-Last", LastValue);
        }
      }

      if (CanBeEmpty)
      {
        TheForm.efpFirstValue.NSingleValue = NFirstValue;
        TheForm.efpLastValue.NSingleValue = NLastValue;
      }
      else
      {
        TheForm.efpFirstValue.SingleValue = FirstValue;
        TheForm.efpLastValue.SingleValue = LastValue;
      }

      if (Minimum.HasValue)
      {
        TheForm.efpFirstValue.Minimum = (decimal)(Minimum.Value);
        TheForm.efpLastValue.Minimum = (decimal)(Minimum.Value);
      }
      if (Maximum.HasValue)
      {
        TheForm.efpFirstValue.Maximum = (decimal)(Maximum.Value);
        TheForm.efpLastValue.Maximum = (decimal)(Maximum.Value);
      }

      TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

      DialogResult Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

      if (Res == DialogResult.OK)
      {
        if (CanBeEmpty)
        {
          NFirstValue = TheForm.efpFirstValue.NSingleValue;
          NLastValue = TheForm.efpLastValue.NSingleValue;
        }
        else
        {
          FirstValue = TheForm.efpFirstValue.SingleValue;
          LastValue = TheForm.efpLastValue.SingleValue;
        }
        if (HasConfig)
        {
          if (CanBeEmpty)
          {
            ConfigPart.SetNullableSingle(ConfigName + "-First", NFirstValue);
            ConfigPart.SetNullableSingle(ConfigName + "-Last", NLastValue);
          }
          else
          {
            ConfigPart.SetSingle(ConfigName + "-First", FirstValue);
            ConfigPart.SetSingle(ConfigName + "-Last", LastValue);
          }
        }
      }

      return Res;
    }

    void efpAnyValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);

      if (!HasValidatingHandler)
        return;

      float? v1 = TheForm.efpFirstValue.NSingleValue;
      float? v2 = TheForm.efpLastValue.NSingleValue;

      EFPValidatingTwoValuesEventArgs<float?, float?> Args2 = new EFPValidatingTwoValuesEventArgs<float?, float?>(args.Validator,
        v1, v2);

      OnValidating(Args2);
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона чисел с плавающей точкой
  /// </summary>
  public sealed class DoubleRangeDialog : BaseFloatRangeDialog<Double>
  {
    #region Запуск диалога

    internal override DialogResult DoShowDialog(object formObj)
    {
      NumRangeForm TheForm = (NumRangeForm)formObj;

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          NFirstValue = ConfigPart.GetNullableDouble(ConfigName + "-First");
          NLastValue = ConfigPart.GetNullableDouble(ConfigName + "-Last");
        }
        else
        {
          FirstValue = ConfigPart.GetDoubleDef(ConfigName + "-First", FirstValue);
          LastValue = ConfigPart.GetDoubleDef(ConfigName + "-Last", LastValue);
        }
      }

      if (CanBeEmpty)
      {
        TheForm.efpFirstValue.NDoubleValue = NFirstValue;
        TheForm.efpLastValue.NDoubleValue = NLastValue;
      }
      else
      {
        TheForm.efpFirstValue.DoubleValue = FirstValue;
        TheForm.efpLastValue.DoubleValue = LastValue;
      }

      if (Minimum.HasValue)
      {
        TheForm.efpFirstValue.Minimum = (decimal)(Minimum.Value);
        TheForm.efpLastValue.Minimum = (decimal)(Minimum.Value);
      }
      if (Maximum.HasValue)
      {
        TheForm.efpFirstValue.Maximum = (decimal)(Maximum.Value);
        TheForm.efpLastValue.Maximum = (decimal)(Maximum.Value);
      }

      TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

      DialogResult Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

      if (Res == DialogResult.OK)
      {
        if (CanBeEmpty)
        {
          NFirstValue = TheForm.efpFirstValue.NDoubleValue;
          NLastValue = TheForm.efpLastValue.NDoubleValue;
        }
        else
        {
          FirstValue = TheForm.efpFirstValue.DoubleValue;
          LastValue = TheForm.efpLastValue.DoubleValue;
        }
        if (HasConfig)
        {
          if (CanBeEmpty)
          {
            ConfigPart.SetNullableDouble(ConfigName + "-First", NFirstValue);
            ConfigPart.SetNullableDouble(ConfigName + "-Last", NLastValue);
          }
          else
          {
            ConfigPart.SetDouble(ConfigName + "-First", FirstValue);
            ConfigPart.SetDouble(ConfigName + "-Last", LastValue);
          }
        }
      }

      return Res;
    }

    void efpAnyValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      if (!HasValidatingHandler)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);


      double? v1 = TheForm.efpFirstValue.NDoubleValue;
      double? v2 = TheForm.efpLastValue.NDoubleValue;

      EFPValidatingTwoValuesEventArgs<double?, double?> Args2 = new EFPValidatingTwoValuesEventArgs<double?, double?>(args.Validator,
        v1, v2);

      OnValidating(Args2);
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода диапазона чисел с плавающей точкой
  /// </summary>
  public sealed class DecimalRangeDialog : BaseFloatRangeDialog<Decimal>
  {
    #region Запуск диалога

    internal override DialogResult DoShowDialog(object formObj)
    {
      NumRangeForm TheForm = (NumRangeForm)formObj;

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          NFirstValue = ConfigPart.GetNullableDecimal(ConfigName + "-First");
          NLastValue = ConfigPart.GetNullableDecimal(ConfigName + "-Last");
        }
        else
        {
          FirstValue = ConfigPart.GetDecimalDef(ConfigName + "-First", FirstValue);
          LastValue = ConfigPart.GetDecimalDef(ConfigName + "-Last", LastValue);
        }
      }

      if (CanBeEmpty)
      {
        //TheForm.efpFirstValue.NullableDecimalValue = NullableFirstValue.Value;
        //TheForm.efpLastValue.NullableDecimalValue = NullableLastValue.Value;
        TheForm.efpFirstValue.NDecimalValue = NFirstValue; // 27.10.2020
        TheForm.efpLastValue.NDecimalValue = NLastValue;
      }
      else
      {
        TheForm.efpFirstValue.DecimalValue = FirstValue;
        TheForm.efpLastValue.DecimalValue = LastValue;
      }

      if (Minimum.HasValue)
      {
        TheForm.efpFirstValue.Minimum = (decimal)(Minimum.Value);
        TheForm.efpLastValue.Minimum = (decimal)(Minimum.Value);
      }
      if (Maximum.HasValue)
      {
        TheForm.efpFirstValue.Maximum = (decimal)(Maximum.Value);
        TheForm.efpLastValue.Maximum = (decimal)(Maximum.Value);
      }

      TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

      DialogResult Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

      if (Res == DialogResult.OK)
      {
        if (CanBeEmpty)
        {
          NFirstValue = TheForm.efpFirstValue.NDecimalValue;
          NLastValue = TheForm.efpLastValue.NDecimalValue;
        }
        else
        {
          FirstValue = TheForm.efpFirstValue.DecimalValue;
          LastValue = TheForm.efpLastValue.DecimalValue;
        }
        if (HasConfig)
        {
          if (CanBeEmpty)
          {
            ConfigPart.SetNullableDecimal(ConfigName + "-First", NFirstValue);
            ConfigPart.SetNullableDecimal(ConfigName + "-Last", NLastValue);
          }
          else
          {
            ConfigPart.SetDecimal(ConfigName + "-First", FirstValue);
            ConfigPart.SetDecimal(ConfigName + "-Last", LastValue);
          }
        }
      }

      return Res;
    }

    void efpAnyValue_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      if (!HasValidatingHandler)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);

      decimal? v1 = TheForm.efpFirstValue.NDecimalValue;
      decimal? v2 = TheForm.efpLastValue.NDecimalValue;

      EFPValidatingTwoValuesEventArgs<decimal?, decimal?> Args2 = new EFPValidatingTwoValuesEventArgs<decimal?, decimal?>(args.Validator,
        v1, v2);

      OnValidating(Args2);
    }

    #endregion
  }
}