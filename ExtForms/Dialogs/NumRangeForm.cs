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
  /// ����� ��� IntRangeDialog, SingleRangeDialog � ��. ��������.
  /// � ���������������� ���� ������� ������������ ������ �������� , � �� NumRangeForm
  /// </summary>
  internal partial class NumRangeForm : Form
  {
    #region �����������

    /// <summary>
    /// ����������� �����
    /// </summary>
    public NumRangeForm()
    {
      InitializeComponent();

      FormProvider = new EFPFormProvider(this);

      btn2eq1.Image = EFPApp.MainImages.Images["SignEqual"];
      btn2eq1.ImageAlign = ContentAlignment.MiddleCenter;
      efp2eq1 = new EFPButton(FormProvider, btn2eq1);
      efp2eq1.DisplayName = "������������ �������� ����� ������������";
      efp2eq1.ToolTipText = "����������� ���� \"��������\" �������� �� ���� \"�������\"";

      btnNo.Visible = false;
    }

    #endregion

    #region ����

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
  /// ������ ��� ����� ��������� ����� �����.
  /// ������� ����� ��� IntRangeDialog, SingleRangeDialog, DoubleRangeDialog � DecimalRangeDialog.
  /// </summary>
  public abstract class BaseNumRangeDialog<T> : BaseInputDialog
    where T : struct, IFormattable, IComparable<T>
  {
    #region �����������

    internal BaseNumRangeDialog()
    {
      Title = "���� ��������� �����";
      Prompt = "��������";
      _Format = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region ��������

    #region N/First/LastValue

    /// <summary>
    /// ���� � �����: ������ �������� ��������� � ���������� ������������ ����������
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
    /// ����������� �������� ��� NFirstValue
    /// ������ ��� ������. ����� �������������� � �����������.
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
    /// ���� � �����: ������ �������� ��������� ��� ��������� ���������
    /// </summary>
    public T FirstValue
    {
      get { return NFirstValue ?? default(T); }
      set { NFirstValue = value; }
    }

    /// <summary>
    /// ����������� �������� ��� FirstValue
    /// ������ ��� ������. ����� �������������� � �����������.
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
    /// ���� � �����: ��������� �������� ��������� � ���������� ������������ ����������
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
    /// ����������� �������� ��� NLastValue
    /// ������ ��� ������. ����� �������������� � �����������.
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
    /// ���� � �����: ��������� �������� ��������� ��� ��������� ���������
    /// </summary>
    public T LastValue
    {
      get { return NLastValue ?? default(T); }
      set { NLastValue = value; }
    }

    /// <summary>
    /// ����������� �������� ��� NLastValue
    /// ������ ��� ������. ����� �������������� � �����������.
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
    /// ����������� �������� ���������� true, ���� ��� ���� ��������� ��������� (NFirstDate.HasValue=true � NLastDate.HasValue=true).
    /// ����� �������������� � �����������.
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
    /// ����� �������� ������� ��������.
    /// �� ��������� - Error
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// ����� �� ������� ������ ��������. ��������� �������� CanBeEmptyMode.
    /// �� ��������� - false
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Format

    /// <summary>
    /// ������ ������� ��� �����
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
    /// ������������� ��� ��������� ��������
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
    /// ���������� ���������� ���������� �������� ��� ����� � ��������� ������, ������� ���������� � �������� Format.
    /// ��������� �������� �������� ������� ������.
    /// </summary>
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region �������� ���������� ��������

    /// <summary>
    /// ����������� ��������. 
    /// </summary>
    public T? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private T? _Minimum;

    /// <summary>
    /// ������������ ��������. 
    /// </summary>
    public T? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private T? _Maximum;

    #endregion

    #region Increment

    /// <summary>
    /// ���� ������ ������������� �������� (������, 1), �� �������� � ����� ����� ������������ � �������
    /// ��������� �����/���� ��� ��������� ����.
    /// ���� �������� ����� 0 (�� ���������), �� ����� ����� ������� ������ �������
    /// </summary>
    public T Increment
    {
      get { return _Increment; }
      set
      {
        if (value.CompareTo(default(T)) < 0)
          throw new ArgumentOutOfRangeException("value", value, "�������� ������ ���� ������ ��� ����� 0");
        _Increment = value;
      }
    }
    private T _Increment;

    #endregion

    #endregion

    #region ����� �������

    /// <summary>
    /// ����� ����� �������.
    /// </summary>
    /// <returns>Ok, ���� ������������ ���� ���������� ��������</returns>
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

      #region �������� ����������� ���������

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
      p.efpFirstValue.Validating += new UIValidatingEventHandler(p.efpAnyValue_Validating);

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
      #region ����

      public BaseNumRangeDialog<T> Owner;

      public EFPNumEditBoxBase<T> efpFirstValue;

      public EFPNumEditBoxBase<T> efpLastValue;

      public Label lblRange;

      #endregion

      #region ��������

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
            args.SetError("����������� �������� ������ �������������");
            return;
          }
        }

        if (Owner.HasValidators)
          Owner.Validators.Validate(args);
      }

      private void InitLblRange(UIValidatingEventArgs args)
      {
        if (efpFirstValue.ValidateState == UIValidateState.Error || efpLastValue.ValidateState == UIValidateState.Error)
          lblRange.Text = "������";
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
            lblRange.Text = "�� " + v1.Value.ToString(efpFirstValue.Control.Format, efpFirstValue.Control.FormatProvider);
          else if (v2.HasValue)
            lblRange.Text = "�� " + v2.Value.ToString(efpLastValue.Control.Format, efpLastValue.Control.FormatProvider);
          else
            lblRange.Text = "�������� �� �����";
        }
      }

      #endregion

      #region ���������� ��� ������ "="

      public void efp2eq1_Click(object sender, EventArgs args)
      {
        efpLastValue.NValue = efpFirstValue.NValue;
      }

      #endregion
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������� ����������� ������� � ��������� ��� ����
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <returns>��������� ������������ ��������</returns>
    protected abstract EFPNumEditBoxBase<T> CreateControlProvider(EFPBaseProvider baseProvider);

    /// <summary>
    /// ���������� �������� � ������ ������������
    /// </summary>
    /// <param name="name">��� ��� ���������</param>
    /// <param name="value">��������</param>
    protected abstract void WriteConfigValue(string name, T? value);

    /// <summary>
    /// ������ �������� �� ������ ������������
    /// </summary>
    /// <param name="name">��� ��� ���������</param>
    /// <returns>��������</returns>
    protected abstract T? ReadConfigValue(string name);

    #endregion
  }

  /// <summary>
  /// ������ ��� ����� ��������� ����� �����
  /// </summary>
  public sealed class IntRangeDialog : BaseNumRangeDialog<Int32>
  {
    #region ���������������� ������

    /// <summary>
    /// ������� ����������� ������� � ��������� ��� ����
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <returns>��������� ������������ ��������</returns>
    protected override EFPNumEditBoxBase<int> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPIntEditBox(baseProvider, new IntEditBox());
    }

    /// <summary>
    /// ���������� �������� � ������ ������������
    /// </summary>
    /// <param name="name">��� ��� ���������</param>
    /// <param name="value">��������</param>
    protected override void WriteConfigValue(string name, int? value)
    {
      ConfigPart.SetNullableInt(name, value);
    }

    /// <summary>
    /// ������ �������� �� ������ ������������
    /// <param name="name">��� ��� ���������</param>
    /// </summary>
    /// <returns>��������</returns>
    protected override int? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableInt(name);
    }

    /// <summary>
    /// ���������� 0
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
  /// ������ ��� ����� ��������� ����� � ��������� ������
  /// </summary>
  public sealed class SingleRangeDialog : BaseNumRangeDialog<Single>
  {
    #region ���������������� ������

    /// <summary>
    /// ������� ����������� ������� � ��������� ��� ����
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <returns>��������� ������������ ��������</returns>
    protected override EFPNumEditBoxBase<float> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPSingleEditBox(baseProvider, new SingleEditBox());
    }

    /// <summary>
    /// ���������� �������� � ������ ������������
    /// </summary>
    /// <param name="name">��� ��� ���������</param>
    /// <param name="value">��������</param>
    protected override void WriteConfigValue(string name, float? value)
    {
      ConfigPart.SetNullableSingle(name, value);
    }

    /// <summary>
    /// ������ �������� �� ������ ������������
    /// <param name="name">��� ��� ���������</param>
    /// </summary>
    /// <returns>��������</returns>
    protected override float? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableSingle(name);
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ����� ��������� ����� � ��������� ������
  /// </summary>
  public sealed class DoubleRangeDialog : BaseNumRangeDialog<Double>
  {
    #region ���������������� ������

    /// <summary>
    /// ������� ����������� ������� � ��������� ��� ����
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <returns>��������� ������������ ��������</returns>
    protected override EFPNumEditBoxBase<double> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPDoubleEditBox(baseProvider, new DoubleEditBox());
    }

    /// <summary>
    /// ���������� �������� � ������ ������������
    /// </summary>
    /// <param name="name">��� ��� ���������</param>
    /// <param name="value">��������</param>
    protected override void WriteConfigValue(string name, double? value)
    {
      ConfigPart.SetNullableDouble(name, value);
    }

    /// <summary>
    /// ������ �������� �� ������ ������������
    /// <param name="name">��� ��� ���������</param>
    /// </summary>
    /// <returns>��������</returns>
    protected override double? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableDouble(name);
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ����� ��������� ����� � ��������� ������
  /// </summary>
  public sealed class DecimalRangeDialog : BaseNumRangeDialog<Decimal>
  {
    #region ���������������� ������

    /// <summary>
    /// ������� ����������� ������� � ��������� ��� ����
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <returns>��������� ������������ ��������</returns>
    protected override EFPNumEditBoxBase<decimal> CreateControlProvider(EFPBaseProvider baseProvider)
    {
      return new EFPDecimalEditBox(baseProvider, new DecimalEditBox());
    }

    /// <summary>
    /// ���������� �������� � ������ ������������
    /// </summary>
    /// <param name="name">��� ��� ���������</param>
    /// <param name="value">��������</param>
    protected override void WriteConfigValue(string name, decimal? value)
    {
      ConfigPart.SetNullableDecimal(name, value);
    }

    /// <summary>
    /// ������ �������� �� ������ ������������
    /// <param name="name">��� ��� ���������</param>
    /// </summary>
    /// <returns>��������</returns>
    protected override decimal? ReadConfigValue(string name)
    {
      return ConfigPart.GetNullableDecimal(name);
    }

    #endregion
  }
}