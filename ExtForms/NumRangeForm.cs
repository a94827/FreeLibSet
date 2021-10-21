using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

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
        if (args.ValidateState != EFPValidateState.Error)
        {
          if (efpFirstValue.NullableDecimalValue.HasValue && efpLastValue.NullableDecimalValue.HasValue)
          {
            decimal v1 = efpFirstValue.DecimalValue;
            decimal v2 = efpLastValue.DecimalValue;
            if (v1 > v2)
              args.SetError("Минимальное значение больше максимального");
          }
        }
      }

      if (efpFirstValue.ValidateState == EFPValidateState.Error || efpLastValue.ValidateState == EFPValidateState.Error)
        lblRange.Text = "Ошибка";
      else
      {
        decimal? v1 = efpFirstValue.NullableDecimalValue;
        decimal? v2 = efpLastValue.NullableDecimalValue;
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
        efpLastValue.NullableDecimalValue = efpFirstValue.NullableDecimalValue;
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
      _NullableFirstValue = null;
      _NullableLastValue = null;
      _CanBeEmpty = false;
      _MinValue = Int32.MinValue;
      _MaxValue = Int32.MaxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: Первое значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public int? NullableFirstValue { get { return _NullableFirstValue; } set { _NullableFirstValue = value; } }
    private int? _NullableFirstValue;

    /// <summary>
    /// Вход и выход: Последнее значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public int? NullableLastValue { get { return _NullableLastValue; } set { _NullableLastValue = value; } }
    private int? _NullableLastValue;

    /// <summary>
    /// Вход и выход: Первое значение диапазона для закрытого интервала
    /// </summary>
    public int FirstValue
    {
      get { return NullableFirstValue ?? 0; }
      set { NullableFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: Последнее значение диапазона для закрытого интервала
    /// </summary>
    public int LastValue
    {
      get { return NullableLastValue ?? 0; }
      set { NullableLastValue = value; }
    }

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
            NullableFirstValue = ConfigPart.GetNullableInt(ConfigName + "-First");
            NullableLastValue = ConfigPart.GetNullableInt(ConfigName + "-Last");
          }
          else
          {
            FirstValue = ConfigPart.GetIntDef(ConfigName + "-First", FirstValue);
            LastValue = ConfigPart.GetIntDef(ConfigName + "-Last", LastValue);
          }
        }

        if (CanBeEmpty)
        {
          TheForm.efpFirstValue.NullableIntValue = NullableFirstValue;
          TheForm.efpLastValue.NullableIntValue = NullableLastValue;
        }
        else
        {
          TheForm.efpFirstValue.IntValue = FirstValue;
          TheForm.efpLastValue.IntValue = LastValue;
        }

        TheForm.efpFirstValue.Minimum = MinValue;
        TheForm.efpFirstValue.Maximum = MaxValue;
        TheForm.efpLastValue.Minimum = MinValue;
        TheForm.efpLastValue.Maximum = MaxValue;

        TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
        TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

        Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

        if (Res == DialogResult.OK)
        {
          if (CanBeEmpty)
          {
            NullableFirstValue = TheForm.efpFirstValue.NullableIntValue;
            NullableLastValue = TheForm.efpLastValue.NullableIntValue;
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
              ConfigPart.SetNullableInt(ConfigName + "-First", NullableFirstValue);
              ConfigPart.SetNullableInt(ConfigName + "-Last", NullableLastValue);
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
      if (args.ValidateState == EFPValidateState.Error)
        return;

      if (Validating == null)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);

      int? v1 = TheForm.efpFirstValue.NullableIntValue;
      int? v2 = TheForm.efpLastValue.NullableIntValue;

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
      _NullableFirstValue = null;
      _NullableLastValue = null;
      _CanBeEmpty = false;
      _DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: Первое значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public T? NullableFirstValue { get { return _NullableFirstValue; } set { _NullableFirstValue = value; } }
    private T? _NullableFirstValue;

    /// <summary>
    /// Вход и выход: Последнее значение диапазона с поддержкой полуоткрытых диапазонов
    /// </summary>
    public T? NullableLastValue { get { return _NullableLastValue; } set { _NullableLastValue = value; } }
    private T? _NullableLastValue;

    /// <summary>
    /// Вход и выход: Первое значение диапазона для закрытого интервала
    /// </summary>
    public T FirstValue
    {
      get { return NullableFirstValue ?? default(T); }
      set { NullableFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: Последнее значение диапазона для закрытого интервала
    /// </summary>
    public T LastValue
    {
      get { return NullableLastValue ?? default(T); }
      set { NullableLastValue = value; }
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
        return DataTools.DecimalPlacesToNumberFormat(DecimalPlaces);
      }
      set
      {
        DecimalPlaces = DataTools.DecimalPlacesFromNumberFormat(value);
      }
    }

    /// <summary>
    /// Минимальное значение. По умолчанию: Int32.MinValue
    /// </summary>
    public T MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private T _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию: Int32.MaxValue
    /// </summary>
    public T MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private T _MaxValue;

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
    #region Конструктор

    /// <summary>
    /// Конструктор устанавливает свойство CanBeEmpty=false и DecimalPlaces=-1
    /// </summary>
    public SingleRangeDialog()
    {
      MinValue = Single.MinValue;
      MaxValue = Single.MaxValue;
    }

    #endregion

    #region Запуск диалога

    internal override DialogResult DoShowDialog(object formObj)
    {
      NumRangeForm TheForm = (NumRangeForm)formObj;

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          NullableFirstValue = ConfigPart.GetNullableSingle(ConfigName + "-First");
          NullableLastValue = ConfigPart.GetNullableSingle(ConfigName + "-Last");
        }
        else
        {
          FirstValue = ConfigPart.GetSingleDef(ConfigName + "-First", FirstValue);
          LastValue = ConfigPart.GetSingleDef(ConfigName + "-Last", LastValue);
        }
      }

      if (CanBeEmpty)
      {
        TheForm.efpFirstValue.NullableSingleValue = NullableFirstValue;
        TheForm.efpLastValue.NullableSingleValue = NullableLastValue;
      }
      else
      {
        TheForm.efpFirstValue.SingleValue = FirstValue;
        TheForm.efpLastValue.SingleValue = LastValue;
      }

      if (MinValue != Single.MinValue)
      {
        TheForm.efpFirstValue.Minimum = (decimal)MinValue;
        TheForm.efpLastValue.Minimum = (decimal)MinValue;
      }
      if (MaxValue != Single.MaxValue)
      {
        TheForm.efpFirstValue.Maximum = (decimal)MaxValue;
        TheForm.efpLastValue.Maximum = (decimal)MaxValue;
      }

      TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

      DialogResult Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

      if (Res == DialogResult.OK)
      {
        if (CanBeEmpty)
        {
          NullableFirstValue = TheForm.efpFirstValue.NullableSingleValue;
          NullableLastValue = TheForm.efpLastValue.NullableSingleValue;
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
            ConfigPart.SetNullableSingle(ConfigName + "-First", NullableFirstValue);
            ConfigPart.SetNullableSingle(ConfigName + "-Last", NullableLastValue);
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
      if (args.ValidateState == EFPValidateState.Error)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);

      if (!HasValidatingHandler)
        return;

      float? v1 = TheForm.efpFirstValue.NullableSingleValue;
      float? v2 = TheForm.efpLastValue.NullableSingleValue;

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
    #region Конструктор

    /// <summary>
    /// Конструктор устанавливает свойство CanBeEmpty=false и DecimalPlaces=-1
    /// </summary>
    public DoubleRangeDialog()
    {
      MinValue = Double.MinValue;
      MaxValue = Double.MaxValue;
    }

    #endregion

    #region Запуск диалога

    internal override DialogResult DoShowDialog(object formObj)
    {
      NumRangeForm TheForm = (NumRangeForm)formObj;

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          NullableFirstValue = ConfigPart.GetNullableDouble(ConfigName + "-First");
          NullableLastValue = ConfigPart.GetNullableDouble(ConfigName + "-Last");
        }
        else
        {
          FirstValue = ConfigPart.GetDoubleDef(ConfigName + "-First", FirstValue);
          LastValue = ConfigPart.GetDoubleDef(ConfigName + "-Last", LastValue);
        }
      }

      if (CanBeEmpty)
      {
        TheForm.efpFirstValue.NullableDoubleValue = NullableFirstValue;
        TheForm.efpLastValue.NullableDoubleValue = NullableLastValue;
      }
      else
      {
        TheForm.efpFirstValue.DoubleValue = FirstValue;
        TheForm.efpLastValue.DoubleValue = LastValue;
      }

      if (MinValue != Double.MinValue)
      {
        TheForm.efpFirstValue.Minimum = (decimal)MinValue;
        TheForm.efpLastValue.Minimum = (decimal)MinValue;
      }
      if (MaxValue != Double.MaxValue)
      {
        TheForm.efpFirstValue.Maximum = (decimal)MaxValue;
        TheForm.efpLastValue.Maximum = (decimal)MaxValue;
      }

      TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

      DialogResult Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

      if (Res == DialogResult.OK)
      {
        if (CanBeEmpty)
        {
          NullableFirstValue = TheForm.efpFirstValue.NullableDoubleValue;
          NullableLastValue = TheForm.efpLastValue.NullableDoubleValue;
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
            ConfigPart.SetNullableDouble(ConfigName + "-First", NullableFirstValue);
            ConfigPart.SetNullableDouble(ConfigName + "-Last", NullableLastValue);
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
      if (args.ValidateState == EFPValidateState.Error)
        return;

      if (!HasValidatingHandler)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);


      double? v1 = TheForm.efpFirstValue.NullableDoubleValue;
      double? v2 = TheForm.efpLastValue.NullableDoubleValue;

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
    #region Конструктор

    /// <summary>
    /// Конструктор устанавливает свойство CanBeEmpty=false и DecimalPlaces=-1
    /// </summary>
    public DecimalRangeDialog()
    {
      MinValue = Decimal.MinValue;
      MaxValue = Decimal.MaxValue;
    }

    #endregion

    #region Запуск диалога

    internal override DialogResult DoShowDialog(object formObj)
    {
      NumRangeForm TheForm = (NumRangeForm)formObj;

      if (HasConfig)
      {
        if (CanBeEmpty)
        {
          NullableFirstValue = ConfigPart.GetNullableDecimal(ConfigName + "-First");
          NullableLastValue = ConfigPart.GetNullableDecimal(ConfigName + "-Last");
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
        TheForm.efpFirstValue.NullableDecimalValue = NullableFirstValue; // 27.10.2020
        TheForm.efpLastValue.NullableDecimalValue = NullableLastValue;
      }
      else
      {
        TheForm.efpFirstValue.DecimalValue = FirstValue;
        TheForm.efpLastValue.DecimalValue = LastValue;
      }

      if (MinValue != Decimal.MinValue)
      {
        TheForm.efpFirstValue.Minimum = MinValue;
        TheForm.efpLastValue.Minimum = MinValue;
      }
      if (MaxValue != Decimal.MaxValue)
      {
        TheForm.efpFirstValue.Maximum = MaxValue;
        TheForm.efpLastValue.Maximum = MaxValue;
      }

      TheForm.efpFirstValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);
      TheForm.efpLastValue.Validating += new EFPValidatingEventHandler(efpAnyValue_Validating);

      DialogResult Res = EFPApp.ShowDialog(TheForm, false, DialogPosition);

      if (Res == DialogResult.OK)
      {
        if (CanBeEmpty)
        {
          NullableFirstValue = TheForm.efpFirstValue.NullableDecimalValue;
          NullableLastValue = TheForm.efpLastValue.NullableDecimalValue;
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
            ConfigPart.SetNullableDecimal(ConfigName + "-First", NullableFirstValue);
            ConfigPart.SetNullableDecimal(ConfigName + "-Last", NullableLastValue);
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
      if (args.ValidateState == EFPValidateState.Error)
        return;

      if (!HasValidatingHandler)
        return;

      NumRangeForm TheForm = (NumRangeForm)(((EFPNumEditBox)sender).Tag);

      decimal? v1 = TheForm.efpFirstValue.NullableDecimalValue;
      decimal? v2 = TheForm.efpLastValue.NullableDecimalValue;

      EFPValidatingTwoValuesEventArgs<decimal?, decimal?> Args2 = new EFPValidatingTwoValuesEventArgs<decimal?, decimal?>(args.Validator,
        v1, v2);

      OnValidating(Args2);
    }

    #endregion
  }
}