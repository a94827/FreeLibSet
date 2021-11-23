// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  [Designer(typeof(FreeLibSet.Controls.Design.NumEditBoxBaseDesigner))]
  public abstract class NumEditBoxBase<T> : UserControl, IMinMaxSource<T?>
    where T : struct, IFormattable, IComparable<T>
  {
    #region �����������

    public NumEditBoxBase()
    {
      _Format = String.Empty;
      SetStyle(ControlStyles.FixedHeight, true);
      base.ForeColor = SystemColors.WindowText;
      base.BackColor = SystemColors.Window;
      base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

      InitMainControl(false);
      _MainControl.Text = String.Empty;
      _TextIsValid = true;
    }

    #endregion

    #region �������� ����������� �������

    private class InternalUpDown : UpDownBase
    {
      #region �����������

      internal InternalUpDown(NumEditBoxBase<T> owner)
      {
        _Owner = owner;
        foreach (Control c in base.Controls)
        {
          if (c is TextBox)
          {
            _MainPart = (TextBox)c;
            break;
          }
        }
        if (_MainPart == null)
          throw new BugException("�� ������ TextBox");
      }

      private NumEditBoxBase<T> _Owner;

      #endregion

      #region ��������

      /// <summary>
      /// �������� ������� - TextBox.
      /// ��. �������� ����� ������ net framework UpDownBase.
      /// </summary>
      public TextBox MainPart { get { return _MainPart; } }
      private TextBox _MainPart;

      #endregion

      #region ���������������� ������ UpDownBase

      public override void UpButton()
      {
        _Owner.PerformIncrement(+1);
      }

      public override void DownButton()
      {
        _Owner.PerformIncrement(-1);
      }

      protected override void UpdateEditText()
      {
        // ����� �� ����
        //_Owner.InitControlText();
      }

      #endregion

      #region ����������� ��������� ��������� ����

      /// <summary>
      /// ������� MouseWheel �� ����������
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Bindable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [Browsable(false)]
      public new event MouseEventHandler MouseWheel
      {
        add { base.MouseWheel += value; }
        remove { base.MouseWheel -= value; }
      }

      /// <summary>
      /// ���������� ���������� ��������� ��������� ����.
      /// � ������ UpDownBase ���������� ���������, ��������� �� ��������� ��������.
      /// ������, �� ���� ������ �������� ����������� ��������� �� 3 �������, � �� 1, ��� ��� ������� �� ���������.
      /// ��������� ��������� �� 1 �������.
      /// 
      /// � ���������, ������� MouseWheel ������ �� ������� �������
      /// </summary>
      /// <param name="args">��������� �������</param>
      protected override void OnMouseWheel(MouseEventArgs args)
      {
        // base.OnMouseWheel(args);

        // ����� �� UpDownBase.OnMouseWheel()
        if ((ModifierKeys & (Keys.Shift | Keys.Alt)) != 0 || MouseButtons != MouseButtons.None)
          return; // Do not scroll when Shift or Alt key is down, or when a mouse button is down.

        if (args.Delta == 0)
          return;

        if (args.Delta > 0)
          UpButton();
        else
          DownButton();
      }

      #endregion
    }

    private Control _MainControl;

    private bool IsUpDown { get { return _MainControl is InternalUpDown; } }

    private bool _InsideInitMainControl;

    private void InitMainControl(bool upDown)
    {
      if (_InsideInitMainControl)
        throw new ReenteranceException();

      _InsideInitMainControl = true;
      try
      {
        // ���������� ��������
        HorizontalAlignment oldTextAlign = this.TextAlign;
        string oldText = this.Text;
        bool oldReadOnly = this.ReadOnly;

        bool hasOldControl = false;
        // ������� ������ �������
        if (_MainControl != null)
        {
          hasOldControl = true;
          base.Controls.Remove(_MainControl);
          _MainControl.Dispose();
        }

        // ������� ����� �������
        if (upDown)
          _MainControl = new InternalUpDown(this);
        else
          _MainControl = new TextBox();
        _MainControl.Dock = DockStyle.Fill;

        // ��������������� ��������
        this.TextAlign = oldTextAlign;
        this.Text = oldText;
        this.ReadOnly = oldReadOnly;

        // ������� �������� �������
        if (!this.ReadOnly)
        {
          _MainControl.BackColor = this.BackColor;
          _MainControl.ForeColor = this.ForeColor;
        }
        _MainControl.Font = this.Font;
        //_MainControl.ContextMenu = this.ContextMenu;
        //_MainControl.ContextMenuStrip = this.ContextMenuStrip;

        base.Controls.Add(_MainControl);

        // ����� ����� ������������ �����������
        _MainControl.TextChanged += MainControl_TextChanged;

        _MainControl.KeyDown += new System.Windows.Forms.KeyEventHandler(MainControl_KeyDown);
        _MainControl.KeyUp += new KeyEventHandler(MainControl_KeyUp);
        _MainControl.KeyPress += new KeyPressEventHandler(MainControl_KeyPress);

        _MainControl.MouseDown += new MouseEventHandler(MainControl_MouseDown);
        _MainControl.MouseUp += new MouseEventHandler(MainControl_MouseUp);
        _MainControl.MouseClick += new MouseEventHandler(MainControl_MouseClick);
        _MainControl.MouseDoubleClick += new MouseEventHandler(MainControl_MouseDoubleClick);
        _MainControl.MouseWheel += new MouseEventHandler(MainControl_MouseWheel);
        _MainControl.Click += new EventHandler(MainControl_Click);
        _MainControl.DoubleClick += new EventHandler(MainControl_DoubleClick);

        _MainControl.SizeChanged += MainControl_SizeChanged;

        if (hasOldControl)
          Invalidate();
      }
      finally
      {
        _InsideInitMainControl = false;
      }
    }

    void MainControl_SizeChanged(object sender, EventArgs args)
    {
      this.Height = _MainControl.Height;
    }

    #endregion

    #region �������� Value/NValue

    private bool _InsideValueChanged;

    [Bindable(true)]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� � ���������� ������� ��������")]
    [Category("Appearance")]
    public T? NValue
    {
      get { return _NValue; }
      set
      {
        if (value.Equals(_NValue))
          return;

        if (_InsideValueChanged)
          return;
        _InsideValueChanged = true;
        try
        {
          if (value.HasValue)
            _NValue = GetRoundedValue(value.Value);
          else
            _NValue = null; // 23.11.2021
          if (!_InsideMainControl_TextChanged)
          {
            InitControlText();
            _TextIsValid = true;
          }
          OnValueChanged(EventArgs.Empty);
        }
        finally
        {
          _InsideValueChanged = false;
        }
      }
    }
    private T? _NValue; // ������� ��������

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� ��� ��������� null")]
    [Category("Appearance")]
    [Browsable(false)]
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }

    protected virtual void OnValueChanged(EventArgs args)
    {
      if (ValueChanged != null)
        ValueChanged(this, args);
    }

    [Description("���������� ����� ��������� ������� NValue/Value")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    /// <summary>
    /// ���������������� ����� ������ ��������� ���������� � ������ ���������� ���������� ������ � �������
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract T GetRoundedValue(T value);

    #endregion

    #region �������� Increment

    [Browsable(false)]
    public IUpDownHandler<T?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set
      {
        if (Object.ReferenceEquals(value, _UpDownHandler))
          return;
        _UpDownHandler = value;

        bool newUpDown = (value != null);
        if (newUpDown != IsUpDown)
          InitMainControl(newUpDown);
      }
    }
    private IUpDownHandler<T?> _UpDownHandler;

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("���������. ���� ����� 0, �� ���� ������ ���� �����. ������������� �������� �������� � ��������� ��������� ��� ��������� ��������")]
    [Category("Appearance")]
    [DefaultValue(0.0)]
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
          throw new ArgumentOutOfRangeException("value", value, "�������� ������ ���� ������ ��� ����� 0");

        if (value.CompareTo(default(T)) == 0)
          UpDownHandler = null;
        else
          UpDownHandler = IncrementUpDownHandler<T>.Create(value, this);
      }
    }

    internal void PerformIncrement(int sign)
    {
      if (ReadOnly)
        return;
      if (!TextIsValid)
        return;
      try
      {
        bool hasNext, hasPrev;
        T? nextValue, prevValue;
        UpDownHandler.GetUpDown(NValue, out hasNext, out nextValue, out hasPrev, out prevValue);

        bool has = sign > 0 ? hasNext : hasPrev;
        T? value = sign > 0 ? nextValue : prevValue;

        if (has)
          NValue = value;
      }
      catch { } // �������� OvertflowException

    }

    #endregion

    #region �������� Minimum � Maximum

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("����������� ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public T? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private T? _Minimum;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("������������ ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public T? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private T? _Maximum;

    #endregion

    #region �������� Format

    [Bindable(true)]
    [DefaultValue("")]
    [Description("�������������� ���������� ������")]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    public string Format
    {
      get { return _Format; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (String.Equals(value, _Format, StringComparison.Ordinal))
          return;

        // ��������� ������������ �������
        // ���������� InvariantCulture �� ��������� �������������� �� ������������ ��������
        default(T).ToString(value, CultureInfo.InvariantCulture); // ����� ��������� FormatException

        _Format = value;
        OnFormatChanged();
      }
    }
    private string _Format;

    private void OnFormatChanged()
    {
      InitControlText();
    }

    /// <summary>
    /// ������������� ��� ��������� ��������
    /// </summary>
    [Browsable(false)]
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return CultureInfo.CurrentCulture;
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
    /// ��������������� ��������.
    /// ���������� ���������� ���������� �������� ��� ����� � ��������� ������, ������� ���������� � �������� Format
    /// </summary>
    [DefaultValue("")]
    [Description("���������� ������ ����� �������. �������������� ��������� ��� �������� Format")]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region �������� ReadOnly

    [Description("����� \"������ ��� ���������\"")]
    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ReadOnly
    {
      get
      {
        if (_MainControl == null)
          return false;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).ReadOnly;
        else
          return ((TextBox)_MainControl).ReadOnly;
      }
      set
      {
        if (value == this.ReadOnly)
          return;

        if (IsUpDown)
          ((InternalUpDown)_MainControl).ReadOnly = value;
        else
          ((TextBox)_MainControl).ReadOnly = value;

        if (!_InsideInitMainControl)
          OnReadOnlyChanged(EventArgs.Empty);

        CopyColors();
      }
    }

    [Description("���������� �������� ReadOnly")]
    [Category("PropertyChanged")]
    public event EventHandler ReadOnlyChanged;

    protected virtual void OnReadOnlyChanged(EventArgs args)
    {
      if (ReadOnlyChanged != null)
        ReadOnlyChanged(this, args);
    }

    #endregion

    #region ��������� �������������

    private void InitControlText()
    {
      if (_NValue.HasValue)
      {
        {
          try
          {
            _MainControl.Text = _NValue.Value.ToString(Format, FormatProvider);
          }
          catch
          {
            _MainControl.ToString();
          }
        }
      }
      else
        _MainControl.Text = String.Empty;
    }

    /// <summary>
    /// �������� ���������� true, ���� ������� ��������� ����� ����� ���� ������������ � �����
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [Browsable(false)]
    public bool TextIsValid { get { return _TextIsValid; } }
    private bool _TextIsValid;

    private bool _InsideMainControl_TextChanged;

    void MainControl_TextChanged(object sender, EventArgs args)
    {
      if ((!_InsideValueChanged) && (!_InsideMainControl_TextChanged))
      {
        _InsideMainControl_TextChanged = true;
        try
        {
          if (String.IsNullOrEmpty(_MainControl.Text))
          {
            NValue = null;
            _TextIsValid = true; // 23.11.2021
          }
          else
          {
            string s = _MainControl.Text;
            FreeLibSet.Forms.WinFormsTools.CorrectNumberString(ref s, this.FormatProvider); // ������ ����� � �������

            T value;
            _TextIsValid = TryParseText(s, out value);
            if (_TextIsValid)
              NValue = value;
          }
        }
        finally
        {
          _InsideMainControl_TextChanged = false;
        }
      }

      OnTextChanged(EventArgs.Empty); // 23.11.2021
    }

    protected abstract bool TryParseText(string s, out T value);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public override string Text
    {
      get
      {
        if (_MainControl == null)
          return String.Empty;
        else
          return _MainControl.Text;
      }
      set { _MainControl.Text = value; }
    }

    [Description("�������������� ������������ (�� ��������� - �� ������� ����)")]
    [Localizable(true)]
    [DefaultValue(HorizontalAlignment.Right)]
    [Category("Appearance")]
    public HorizontalAlignment TextAlign
    {
      get
      {
        if (_MainControl == null)
          return HorizontalAlignment.Right;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).TextAlign;
        else
          return ((TextBox)_MainControl).TextAlign;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).TextAlign = value;
        else
          ((TextBox)_MainControl).TextAlign = value;
      }
    }

    #endregion

    #region ����� � ����

    protected override void OnFontChanged(EventArgs args)
    {
      base.OnFontChanged(args);
      if (_MainControl != null)
        _MainControl.Font = this.Font;
    }

    protected override void OnForeColorChanged(EventArgs args)
    {
      base.OnForeColorChanged(args);
      CopyColors();
    }

    protected override void OnBackColorChanged(EventArgs args)
    {
      base.OnBackColorChanged(args);
      CopyColors();
    }

    private void CopyColors()
    {
      if (_MainControl == null)
        return;

      if (Enabled)
      {
        if (ReadOnly)
        {
          _MainControl.ForeColor = SystemColors.ControlText;
          _MainControl.BackColor = SystemColors.Control;
        }
        else
        {
          _MainControl.ForeColor = this.ForeColor;
          _MainControl.BackColor = this.BackColor;
        }
      }
      else
      {
        _MainControl.ForeColor = SystemColors.GrayText;
        _MainControl.BackColor = SystemColors.Control;
      }
    }

    protected override void OnEnabledChanged(EventArgs args)
    {
      base.OnEnabledChanged(args);
      if (_MainControl != null)
        _MainControl.Enabled = this.Enabled;
      CopyColors();
    }

    #endregion

    #region �������� TextBox

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public int SelectionStart
    {
      get
      {
        if (_MainControl == null)
          return 0;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).MainPart.SelectionStart;
        else
          return ((TextBox)_MainControl).SelectionStart;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).MainPart.SelectionStart = value;
        else
          ((TextBox)_MainControl).SelectionStart = value;
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public int SelectionLength
    {
      get
      {
        if (_MainControl == null)
          return 0;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).MainPart.SelectionLength;
        else
          return ((TextBox)_MainControl).SelectionLength;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).MainPart.SelectionLength = value;
        else
          ((TextBox)_MainControl).SelectionLength = value;
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public string SelectedText
    {
      get
      {
        if (_MainControl == null)
          return String.Empty;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).MainPart.SelectedText;
        else
          return ((TextBox)_MainControl).SelectedText;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).MainPart.SelectedText = value;
        else
          ((TextBox)_MainControl).SelectedText = value;
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public void Select(int start, int length)
    {
      if (IsUpDown)
        ((InternalUpDown)_MainControl).Select(start, length);
      else
        ((TextBox)_MainControl).Select(start, length);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public void SelectAll()
    {
      if (IsUpDown)
        ((InternalUpDown)_MainControl).MainPart.SelectAll();
      else
        ((TextBox)_MainControl).SelectAll();
    }

    #endregion

    #region �������� ������ ������� �� ��������� ��������

    void MainControl_Click(object sender, EventArgs args)
    {
      OnClick(args);
    }

    void MainControl_DoubleClick(object sender, EventArgs args)
    {
      OnDoubleClick(args);
    }

    void MainControl_MouseClick(object sender, MouseEventArgs args)
    {
      OnMouseClick(args);
    }

    void MainControl_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      OnMouseDoubleClick(args);
    }

    void MainControl_MouseUp(object sender, MouseEventArgs args)
    {
      OnMouseUp(args);
    }

    void MainControl_MouseDown(object sender, MouseEventArgs args)
    {
      OnMouseDown(args);
    }

    void MainControl_MouseWheel(object sender, MouseEventArgs args)
    {
      OnMouseWheel(args);
    }

    void MainControl_KeyPress(object sender, KeyPressEventArgs args)
    {
      OnKeyPress(args);
    }


    private void MainControl_KeyDown(object sender, KeyEventArgs args)
    {
      OnKeyDown(args);
    }


    void MainControl_KeyUp(object sender, KeyEventArgs args)
    {
      OnKeyUp(args);
    }

    #endregion

    #region ������ ���������������� �������� � ������

    protected override Size DefaultSize
    {
      get
      {
        if (_MainControl == null)
          return base.DefaultSize;
        else
          return _MainControl.Size;
      }
    }

    /// <summary>
    /// ��� ������ �� �������� ��������� �������������� ������
    /// </summary>
    /// <param name="args"></param>
    protected override void OnLeave(EventArgs args)
    {
      if (_TextIsValid)
        InitControlText();

      base.OnLeave(args);
    }

    protected override void Select(bool directed, bool forward)
    {
      base.Select(directed, forward);
      _MainControl.Select();
      SelectAll();
    }

    //protected override void OnContextMenuChanged(EventArgs args)
    //{
    //  base.OnContextMenuChanged(args);
    //  if (_MainControl != null)
    //    _MainControl.ContextMenu = this.ContextMenu;
    //}

    //protected override void OnContextMenuStripChanged(EventArgs args)
    //{
    //  base.OnContextMenuStripChanged(args);
    //  if (_MainControl != null)
    //    _MainControl.ContextMenuStrip = this.ContextMenuStrip;
    //}

    #endregion
  }


  /// <summary>
  /// ���� ����� ��������� �������� ���� Double
  /// </summary>
  [Description("���� ����� ������ �����")]
  [ToolboxBitmap(typeof(IntEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class IntEditBox : NumEditBoxBase<Int32>
  {
    #region ���������������� ������

    /// <summary>
    /// �������� ����� Int32.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    protected override bool TryParseText(string s, out int result)
    {
      return Int32.TryParse(s, NumberStyles.Integer | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    protected override int GetRoundedValue(int value)
    {
      return value;
    }

    /// <summary>
    /// ���������� 0
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int DecimalPlaces { get { return 0; } }

    #endregion
  }

  /// <summary>
  /// ���� ����� ��������� �������� ���� Double
  /// </summary>
  [Description("���� ����� ��������� �������� ���� Single")]
  [ToolboxBitmap(typeof(SingleEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class SingleEditBox : NumEditBoxBase<Single>
  {
    #region ���������������� ������

    /// <summary>
    /// �������� ����� Single.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    protected override bool TryParseText(string s, out float result)
    {
      return Single.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ��������� ���������� �� ����� ��������, ������������ ��������� DecimalPlaces
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    protected override float GetRoundedValue(float value)
    {
      int dp = this.DecimalPlaces;
      if (dp >= 0)
        // ��� Math.Round() ��� float.
        return (float)Math.Round((double)value, dp, MidpointRounding.AwayFromZero);
      else
        return value;
    }

    #endregion
  }

  /// <summary>
  /// ���� ����� ��������� �������� ���� Double
  /// </summary>
  [Description("���� ����� ��������� �������� ���� Double")]
  [ToolboxBitmap(typeof(DoubleEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class DoubleEditBox : NumEditBoxBase<Double>
  {
    #region ���������������� ������

    /// <summary>
    /// �������� ����� Double.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    protected override bool TryParseText(string s, out double result)
    {
      return Double.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ��������� ���������� �� ����� ��������, ������������ ��������� DecimalPlaces
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    protected override double GetRoundedValue(double value)
    {
      int dp = this.DecimalPlaces;
      if (dp >= 0)
        return Math.Round(value, dp, MidpointRounding.AwayFromZero);
      else
        return value;
    }

    #endregion
  }

  /// <summary>
  /// ���� ����� ��������� �������� ���� Double
  /// </summary>
  [Description("���� ����� ��������� �������� ���� Decimal")]
  [ToolboxBitmap(typeof(DecimalEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class DecimalEditBox : NumEditBoxBase<Decimal>
  {
    #region ���������������� ������

    /// <summary>
    /// �������� ����� Decimal.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    protected override bool TryParseText(string s, out decimal result)
    {
      return Decimal.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ��������� ���������� �� ����� ��������, ������������ ��������� DecimalPlaces
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    protected override decimal GetRoundedValue(decimal value)
    {
      int dp = this.DecimalPlaces;
      if (dp >= 0)
        return Math.Round(value, dp, MidpointRounding.AwayFromZero);
      else
        return value;
    }

    #endregion
  }
}
