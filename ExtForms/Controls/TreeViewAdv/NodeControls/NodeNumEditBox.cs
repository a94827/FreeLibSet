// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public abstract class BaseFormattedTextControl : BaseTextControl
  {
    public BaseFormattedTextControl()
    {
      _Format = String.Empty;
    }

    #region Свойства

    [Bindable(true)]
    [DefaultValue("")]
    [Description("Форматирование текстового вывода")]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    public virtual string Format
    {
      get { return _Format; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (String.Equals(value, _Format, StringComparison.Ordinal))
          return;

        _Format = value;
      }
    }
    private string _Format;

    /// <summary>
    /// Форматировщик для числового значения
    /// </summary>
    [Browsable(false)]
    public virtual IFormatProvider FormatProvider
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

    #endregion

    #region Переопределенные методы

    protected override string FormatLabel(object obj)
    {
      IFormattable fv = obj as IFormattable;
      if (fv != null)
        return fv.ToString(Format, FormatProvider);
      else
        return base.FormatLabel(obj);
    }

    #endregion
  }

  public abstract class NodeNumEditBoxBase<T> : BaseFormattedTextControl, IMinMaxSource<T?>
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    public NodeNumEditBoxBase()
    {
      TextAlign = HorizontalAlignment.Right;
    }

    #endregion

    #region Свойства

    #region Свойство Increment

    [Browsable(false)]
    public IUpDownHandler<T?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<T?> _UpDownHandler;

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Инкремент. Если равно 0, то есть только поле ввода. Положительное значение приводит к появлению стрелочек для прокрутки значения")]
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
          throw new ArgumentOutOfRangeException("value", value, "Значение должно быть больше или равно 0");

        if (value.CompareTo(default(T)) == 0)
          UpDownHandler = null;
        else
          UpDownHandler = IncrementUpDownHandler<T>.Create(value, this);
      }
    }

    #endregion

    #region Свойства Minimum и Maximum

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Минимальное значение, используемое для прокрутки")]
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
    [Description("Максимальное значение, используемое для прокрутки")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public T? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private T? _Maximum;

    #endregion

    #region Свойство Format

    public override string Format
    {
      get { return base.Format; }
      set
      {
        if (value == null)
          value = String.Empty;
        // Проверяем корректность формата
        // Используем InvariantCulture во избежание неожиданностей от национальных настроек
        default(T).ToString(value, CultureInfo.InvariantCulture); // может произойти FormatException

        base.Format = value;
      }
    }

    /// <summary>
    /// Вспомогательное свойство.
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве Format
    /// </summary>
    [DefaultValue("")]
    [Description("Количество знаков после запятой. Альтернативная установка для свойства Format")]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        Format = FormatStringTools.DecimalPlacesToNumberFormat(value);
      }
    }

    #endregion

    #endregion

    #region Переопределенные методы

    protected override Size CalculateEditorSize(TreeViewAdvEditorContext context)
    {
      if (Parent.UseColumns)
        return context.Bounds.Size;
      else
      {
        int n = DecimalPlaces + 10;
        int w = (int)context.DrawContext.Graphics.MeasureString(new string('0', n), Parent.Font).Width + 2 * SystemInformation.BorderSize.Width;
        return new Size(w, context.Bounds.Height);
      }
    }

    protected override Control CreateEditor(TreeNodeAdv node)
    {
      NumEditBoxBase<T> control = DoCreateEditor(node);
      control.UpDownHandler = this.UpDownHandler;
      control.Minimum = this.Minimum;
      control.Maximum = this.Maximum;
      control.Format = this.Format;
      control.FormatProvider = this.FormatProvider;
      SetEditControlProperties(control, node);
      return control;
    }

    protected abstract NumEditBoxBase<T> DoCreateEditor(TreeNodeAdv node);

    protected override void DisposeEditor(Control editor)
    {
    }

    #endregion
  }

  public class NodeIntEditBox : NodeNumEditBoxBase<Int32>
  {
    #region Переопределенные методы

    protected override NumEditBoxBase<int> DoCreateEditor(TreeNodeAdv node)
    {
      IntEditBox control = new IntEditBox();
      control.NValue = (int?)GetValue(node);
      return control;
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as FreeLibSet.Controls.IntEditBox).NValue);
    }

    #endregion
  }

  public class NodeSingleEditBox : NodeNumEditBoxBase<Single>
  {
    #region Properties

    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //public int DecimalPlaces
    //{
    //  get
    //  {
    //    return this._decimalPlaces;
    //  }
    //  set
    //  {
    //    this._decimalPlaces = value;
    //  }
    //}


    #endregion

    #region Переопределенные методы

    protected override NumEditBoxBase<Single> DoCreateEditor(TreeNodeAdv node)
    {
      SingleEditBox control = new SingleEditBox();
      control.NValue = (float?)GetValue(node);
      return control;
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as FreeLibSet.Controls.SingleEditBox).NValue);
    }

    #endregion
  }

  public class NodeDoubleEditBox : NodeNumEditBoxBase<Double>
  {
    #region Properties

    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //public int DecimalPlaces
    //{
    //  get
    //  {
    //    return this._decimalPlaces;
    //  }
    //  set
    //  {
    //    this._decimalPlaces = value;
    //  }
    //}


    #endregion

    #region Переопределенные методы

    protected override NumEditBoxBase<Double> DoCreateEditor(TreeNodeAdv node)
    {
      DoubleEditBox control = new DoubleEditBox();
      control.NValue = (double?)GetValue(node);
      return control;
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as FreeLibSet.Controls.DoubleEditBox).NValue);
    }

    #endregion
  }

  public class NodeDecimalEditBox : NodeNumEditBoxBase<decimal>
  {
    #region Properties

    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //public int DecimalPlaces
    //{
    //  get
    //  {
    //    return this._decimalPlaces;
    //  }
    //  set
    //  {
    //    this._decimalPlaces = value;
    //  }
    //}


    #endregion

    #region Переопределенные методы

    protected override NumEditBoxBase<decimal> DoCreateEditor(TreeNodeAdv node)
    {
      DecimalEditBox control = new DecimalEditBox();
      control.NValue = (decimal?)GetValue(node);
      return control;
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as FreeLibSet.Controls.DecimalEditBox).NValue);
    }

    #endregion
  }
}
