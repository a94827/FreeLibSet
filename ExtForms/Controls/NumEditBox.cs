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
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  [Designer(typeof(FreeLibSet.Controls.Design.NumEditBoxBaseDesigner))]
  public abstract class NumEditBoxBase : UserControl
  {
    #region ��������� �����

    internal abstract class UntypedInfo
    {
      internal NumEditBoxBase Control { get { return _Control; } set { _Control = value; } }
      private NumEditBoxBase _Control;

      internal abstract void InitControlText();

      internal abstract void MainControl_TextChanged(object sender, EventArgs args);

      internal abstract void PerformIncrement(int sign);

      internal abstract IFormattable DefaultValue { get;}
    }

    internal class TypedInfo<T> : UntypedInfo, IMinMaxSource<T?>
      where T : struct, IFormattable, IComparable<T>
    {
      private INumEditBox<T> Control2 { get { return (INumEditBox<T>)Control; } }

      internal override IFormattable DefaultValue { get { return default(T); } }

      #region �������� Value/NValue

      public T? NValue
      {
        get { return _NValue; }
        set
        {
          if (value.Equals(_NValue))
            return;

          if (Control._InsideValueChanged)
            return;
          Control._InsideValueChanged = true;
          try
          {
            if (value.HasValue)
              _NValue = Control2.GetRoundedValue(value.Value);
            else
              _NValue = null; // 23.11.2021
            if (!Control._InsideMainControl_TextChanged)
            {
              InitControlText();
              Control._TextIsValid = true;
            }
            Control.OnValueChanged(EventArgs.Empty);
          }
          finally
          {
            Control._InsideValueChanged = false;
          }
        }
      }
      private T? _NValue; // ������� ��������

      public T Value
      {
        get { return NValue ?? default(T); }
        set { NValue = value; }
      }


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
          if (newUpDown != Control.IsUpDown)
            Control.InitMainControl(newUpDown);
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

      internal override void PerformIncrement(int sign)
      {
        if (Control.ReadOnly)
          return;
        if (!Control.TextIsValid)
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

      #region ��������� �������������

      internal override void InitControlText()
      {
        if (_NValue.HasValue)
        {
          {
            try
            {
              Control._MainControl.Text = _NValue.Value.ToString(Control.Format, Control.FormatProvider);
            }
            catch
            {
              Control._MainControl.ToString();
            }
          }
        }
        else
          Control._MainControl.Text = String.Empty;
      }

      internal override void MainControl_TextChanged(object sender, EventArgs args)
      {
        if ((!Control._InsideValueChanged) && (!Control._InsideMainControl_TextChanged))
        {
          Control._InsideMainControl_TextChanged = true;
          try
          {
            if (String.IsNullOrEmpty(Control._MainControl.Text))
            {
              NValue = null;
              Control._TextIsValid = true; // 23.11.2021
            }
            else
            {
              string s = Control._MainControl.Text;
              FreeLibSet.Forms.WinFormsTools.CorrectNumberString(ref s, Control.FormatProvider); // ������ ����� � �������

              T value;
              Control._TextIsValid = Control2.TryParseText(s, out value);
              if (Control._TextIsValid)
                NValue = value;
            }
          }
          finally
          {
            Control._InsideMainControl_TextChanged = false;
          }
        }

        Control.OnTextChanged(EventArgs.Empty); // 23.11.2021
      }

      #endregion
    }

    #endregion

    #region �����������

    internal NumEditBoxBase(UntypedInfo untypedInfo)
    {
      _Format = String.Empty;
      SetStyle(ControlStyles.FixedHeight, true);
      base.ForeColor = SystemColors.WindowText;
      base.BackColor = SystemColors.Window;
      base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

      _UntypedInfo = untypedInfo;
      _UntypedInfo.Control = this;

      InitMainControl(false);
      _MainControl.Text = String.Empty;
      _TextIsValid = true;
    }

    internal UntypedInfo UTI 
    {
      [MethodImpl(MethodImplOptions.NoInlining)]
      get { return _UntypedInfo; } 
    }
    private UntypedInfo _UntypedInfo;

    #endregion

    #region �������� ����������� �������

    private class InternalUpDown : UpDownBase
    {
      #region �����������

      internal InternalUpDown(NumEditBoxBase owner)
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

      private NumEditBoxBase _Owner;

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
        _Owner._UntypedInfo.PerformIncrement(+1);
      }

      public override void DownButton()
      {
        _Owner._UntypedInfo.PerformIncrement(-1);
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
        _MainControl.TextChanged += _UntypedInfo.MainControl_TextChanged;

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


    protected virtual void OnValueChanged(EventArgs args)
    {
      if (ValueChanged != null)
        ValueChanged(this, args);
    }

    [Description("���������� ����� ��������� ������� NValue/Value")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

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
        _UntypedInfo.DefaultValue.ToString(value, CultureInfo.InvariantCulture); // ����� ��������� FormatException

        _Format = value;
        OnFormatChanged();
      }
    }
    private string _Format;

    private void OnFormatChanged()
    {
      _UntypedInfo.InitControlText();
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

    /// <summary>
    /// �������� ���������� true, ���� ������� ��������� ����� ����� ���� ������������ � �����
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [Browsable(false)]
    public bool TextIsValid { get { return _TextIsValid; } }
    private bool _TextIsValid;

    private bool _InsideMainControl_TextChanged;

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
        _UntypedInfo.InitControlText();

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
  /// �������������� ��������, ������� ������ ��������������� � Int/Single/Double/DecimalEditBox.
  /// ���� ��������� ������������ ������ ExtForms.dll � �� ������������ ��� ����������� ����.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface INumEditBox<T>
    where T : struct, IFormattable, IComparable<T>
  {
    #region ��������

    /// <summary>
    /// ������� �������� � ���������� null
    /// </summary>
    T? NValue { get;set;}

    /// <summary>
    /// ������� �������� ��� ��������� null.
    /// </summary>
    T Value { get;set;}

    /// <summary>
    /// ��������� ��� "���������" ��������
    /// </summary>
    IUpDownHandler<T?> UpDownHandler { get;set;}

    /// <summary>
    /// ��������� (���� ������ ������������� ��������)
    /// </summary>
    T Increment { get;set;}

    /// <summary>
    /// ���������� ���������� ��������, ���� �� null. ������������ ��� "���������".
    /// </summary>
    T? Minimum { get; set;}

    /// <summary>
    /// ����������� ���������� ��������, ���� �� null. ������������ ��� "���������".
    /// </summary>
    T? Maximum { get; set;}

    #endregion

    #region ������


    /// <summary>
    /// ����� ������ ��������� ���������� � ������ ���������� ���������� ������ � �������
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    T GetRoundedValue(T value);

    /// <summary>
    /// ������ ������� <typeparamref name="T"/>.TryParse().
    /// </summary>
    /// <param name="s"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryParseText(string s, out T value);

    #endregion
  }

  /// <summary>
  /// ���� ����� ��������� �������� ���� Double
  /// </summary>
  [Description("���� ����� ������ �����")]
  [ToolboxBitmap(typeof(IntEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  [DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
  public class IntEditBox : NumEditBoxBase, INumEditBox<Int32>
  {
    #region �����������

    public IntEditBox()
      : base(new TypedInfo<int>())
    {
    }

    #endregion

    #region INumEditBox<Int32> implementation

    private TypedInfo<Int32> TI 
    {
      [MethodImpl(MethodImplOptions.NoInlining)]
      get { return (TypedInfo<Int32>)(base.UTI); } 
    }

    [Bindable(true)]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� � ���������� ������� ��������")]
    [Category("Appearance")]
    public int? NValue { get { return TI.NValue; } set { TI.NValue = value; } }

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� ��� ��������� null")]
    [Category("Appearance")]
    [Browsable(false)]
    public int Value { get { return TI.Value; } set { TI.Value = value; } }

    [Browsable(false)]
    public IUpDownHandler<int?> UpDownHandler { get { return TI.UpDownHandler; } set { TI.UpDownHandler = value; } }

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("���������. ���� ����� 0, �� ���� ������ ���� �����. ������������� �������� �������� � ��������� ��������� ��� ��������� ��������")]
    [Category("Appearance")]
    [DefaultValue(0)]
    public int Increment { get { return TI.Increment; } set { TI.Increment = value; } }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("����������� ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public int? Minimum { get { return TI.Minimum; } set { TI.Minimum = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("������������ ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public int? Maximum { get { return TI.Maximum; } set { TI.Maximum = value; } }

    /// <summary>
    /// �������� ����� Int32.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    bool INumEditBox<int>.TryParseText(string s, out int result)
    {
      return Int32.TryParse(s, NumberStyles.Integer | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    int INumEditBox<int>.GetRoundedValue(int value)
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
  public class SingleEditBox : NumEditBoxBase, INumEditBox<Single>
  {
    #region �����������

    public SingleEditBox()
      :base(new TypedInfo<float>())
    {
    }

    #endregion

    #region INumEditBox<Single> implementation

    private TypedInfo<Single> TI { get { return (TypedInfo<Single>)UTI; } }

    [Bindable(true)]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� � ���������� ������� ��������")]
    [Category("Appearance")]
    public float? NValue { get { return TI.NValue; } set { TI.NValue = value; } }

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� ��� ��������� null")]
    [Category("Appearance")]
    [Browsable(false)]
    public float Value { get { return TI.Value; } set { TI.Value = value; } }

    [Browsable(false)]
    public IUpDownHandler<float?> UpDownHandler { get { return TI.UpDownHandler; } set { TI.UpDownHandler = value; } }

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("���������. ���� ����� 0, �� ���� ������ ���� �����. ������������� �������� �������� � ��������� ��������� ��� ��������� ��������")]
    [Category("Appearance")]
    [DefaultValue(0f)]
    public float Increment { get { return TI.Increment; } set { TI.Increment = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("����������� ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public float? Minimum { get { return TI.Minimum; } set { TI.Minimum = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("������������ ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public float? Maximum { get { return TI.Maximum; } set { TI.Maximum = value; } }

    /// <summary>
    /// �������� ����� Single.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    bool INumEditBox<float>.TryParseText(string s, out float result)
    {
      return Single.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ��������� ���������� �� ����� ��������, ������������ ��������� DecimalPlaces
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    float INumEditBox<float>.GetRoundedValue(float value)
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
  public class DoubleEditBox : NumEditBoxBase, INumEditBox<Double>
  {
    #region �����������

    public DoubleEditBox()
      :base(new TypedInfo<double>())
    {
    }

    #endregion

    #region INumEditBox<Double> implementation

    private TypedInfo<Double> TI { get { return (TypedInfo<Double>)UTI; } }

    [Bindable(true)]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� � ���������� ������� ��������")]
    [Category("Appearance")]
    public double? NValue { get { return TI.NValue; } set { TI.NValue = value; } }

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� ��� ��������� null")]
    [Category("Appearance")]
    [Browsable(false)]
    public double Value { get { return TI.Value; } set { TI.Value = value; } }

    [Browsable(false)]
    public IUpDownHandler<double?> UpDownHandler { get { return TI.UpDownHandler; } set { TI.UpDownHandler = value; } }

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("���������. ���� ����� 0, �� ���� ������ ���� �����. ������������� �������� �������� � ��������� ��������� ��� ��������� ��������")]
    [Category("Appearance")]
    [DefaultValue(0.0)]
    public double Increment { get { return TI.Increment; } set { TI.Increment = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("����������� ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public double? Minimum { get { return TI.Minimum; } set { TI.Minimum = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("������������ ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public double? Maximum { get { return TI.Maximum; } set { TI.Maximum = value; } }

    /// <summary>
    /// �������� ����� Double.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    bool INumEditBox<double>.TryParseText(string s, out double result)
    {
      return Double.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ��������� ���������� �� ����� ��������, ������������ ��������� DecimalPlaces
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    double INumEditBox<double>.GetRoundedValue(double value)
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
  public class DecimalEditBox : NumEditBoxBase, INumEditBox<Decimal>
  {
    #region �����������

    public DecimalEditBox()
      :base(new TypedInfo<decimal>())
    {
    }

    #endregion

    #region INumEditBox<Decimal> implementation

    private TypedInfo<Decimal> TI { get { return (TypedInfo<Decimal>)UTI; } }

    [Bindable(true)]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� � ���������� ������� ��������")]
    [Category("Appearance")]
    public decimal? NValue { get { return TI.NValue; } set { TI.NValue = value; } }

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("������� �������� ��� ��������� null")]
    [Category("Appearance")]
    [Browsable(false)]
    public decimal Value { get { return TI.Value; } set { TI.Value = value; } }

    [Browsable(false)]
    public IUpDownHandler<decimal?> UpDownHandler { get { return TI.UpDownHandler; } set { TI.UpDownHandler = value; } }

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("���������. ���� ����� 0, �� ���� ������ ���� �����. ������������� �������� �������� � ��������� ��������� ��� ��������� ��������")]
    [Category("Appearance")]
    [DefaultValue(0.0)]
    public decimal Increment { get { return TI.Increment; } set { TI.Increment = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("����������� ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public decimal? Minimum { get { return TI.Minimum; } set { TI.Minimum = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("������������ ��������, ������������ ��� ���������")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public decimal? Maximum { get { return TI.Maximum; } set { TI.Maximum = value; } }

    /// <summary>
    /// �������� ����� Decimal.TryParse() � ���������������� �������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="result">�������� ��������</param>
    /// <returns>true, ���� �������������� ���������</returns>
    bool INumEditBox<decimal>.TryParseText(string s, out decimal result)
    {
      return Decimal.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// ��������� ���������� �� ����� ��������, ������������ ��������� DecimalPlaces
    /// </summary>
    /// <param name="value">�������� �� ����������</param>
    /// <returns>����������� ��������</returns>
    decimal INumEditBox<decimal>.GetRoundedValue(decimal value)
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
