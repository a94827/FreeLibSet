using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#pragma warning disable 1591

namespace AgeyevAV.ExtForms
{

  /// <summary>
  /// Замена для NumericUpDowm:
  /// Ограничения Minimum и Maximum действуют только на кнопки прокрутки. Можно
  /// вводить любой текст (не число), который преобразуется в значение только при
  /// выходе из элемента.
  /// Шестнадцатиричный просмотр и разделитель тысяч не поддерживаются
  /// </summary>
  [Description("Поле ввода числового значения с прокруткой")]
  [ToolboxBitmap(typeof(ExtNumericUpDown), "ExtNumericUpDown.bmp")]
  [ToolboxItem(true)]
  public class ExtNumericUpDown : UpDownBase
  {
    #region Конструктор

    public ExtNumericUpDown()
    {
      _DecimalPlaces = 0;
      _Minimum = 0;
      _Maximum = 100;
      _Increment = 1;
      base.Text = "0";
    }

    #endregion

    #region Общие свойства

    [DefaultValue("0")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public override string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }

    [Bindable(true)]
    [DefaultValue(typeof(decimal), "0")]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Текущее значение. Если задано DecimalPlace=0, то выполняется преобразование в целое число")]
    [Category("Appearance")]
    public decimal Value
    {
      get
      {
        bool textIsValid;
        return GetValue(out textIsValid);

      }
      set
      {
        if (value == Value)
          return;
        base.Text = DoFormat(value);
      }
    }

    private decimal GetValue(out bool textIsValid)
    {
      textIsValid = true;
      string s = base.Text;
      if (String.IsNullOrEmpty(s))
        return 0m;
      if (_DecimalPlaces == 0)
      {
        long x;
        if (long.TryParse(s, out x))
          return (decimal)x;
        else
        {
          textIsValid = false;
          return 0m;
        }
      }
      else
      {
        WinFormsTools.CorrectNumberString(ref s); // замена точки и запятой

        decimal x;
        if (Decimal.TryParse(s, out x))
          return x;
        else
        {
          textIsValid = false;
          return 0m;
        }
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Число десятичных разрядов после точки. По умолчанию - 0, что означает " +
       "преобразование в целое число")]
    [Category("Appearance")]
    [DefaultValue(0)]
    public int DecimalPlaces
    {
      get
      {
        return _DecimalPlaces;
      }
      set
      {
        if (value == _DecimalPlaces)
          return;
        if (value < 0 || value > 10)
          throw new ArgumentOutOfRangeException("value", value, "Число десятичных знаков после запятой должно быть в диапазоне от 0 до 10");
        _DecimalPlaces = value;
        base.Text = DoFormat(Value);
      }
    }
    private int _DecimalPlaces;

    /// <summary>
    /// Вспомогательное свойство. Маска для ввода числа "0", "0.0", "0.00", ..., в зависимости от свойства
    /// DecimalPlaces
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public string Format
    {
      get
      {
        if (DecimalPlaces == 0)
          return "0";
        return "0." + new string('0', DecimalPlaces);
      }
    }

    /// <summary>
    /// Свойство возвращает true, если текущий введенный текст может быть преобразован в число
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    //    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public bool TextIsValid 
    { 
      get 
      {
        bool textIsValid;
        GetValue(out textIsValid);
        return textIsValid; 
      } 
    }

    #endregion

    #region События

    [Description("Посылается после изменения свойства Value")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    #endregion

    #region Minimum и Maximum

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Минимальное значение (по умолчанию - 0)")]
    [Category("Appearance")]
    [DefaultValue(0)]
    public decimal Minimum
    {
      get { return _Minimum; }
      set
      {
        _Minimum = value;
        // Не меняем другие свойства
      }
    }
    private decimal _Minimum;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Минимальное значение (по умолчанию - 100)")]
    [Category("Appearance")]
    [DefaultValue(100)]
    public decimal Maximum
    {
      get { return _Maximum; }
      set
      {
        _Maximum = value;
        // Не меняем другие свойства
      }
    }
    private decimal _Maximum;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Description("Инкремент (по умолчанию - 1)")]
    [Category("Appearance")]
    [DefaultValue(1)]
    public decimal Increment
    {
      get { return _Increment; }
      set
      {
        if (value <= 0m)
          throw new ArgumentOutOfRangeException("value", value, "Значение должно быть больше 0");
        _Increment = value;
        // Не меняем другие свойства
      }
    }
    private decimal _Increment;

    #endregion

    #region Внутренняя реализация

    protected override void OnEnter(EventArgs args)
    {
      base.OnEnter(args);
      if (!String.IsNullOrEmpty(base.Text))
        base.Select(0, base.Text.Length);
    }

    protected override void OnLeave(EventArgs args)
    {
      base.OnLeave(args);
      base.Text = DoFormat(Value);
    }

    protected override void OnTextBoxTextChanged(object sender, EventArgs args)
    {
      base.OnTextBoxTextChanged(sender, args);

      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
    }

    protected override void UpdateEditText()
    {
      Text = DoFormat(Value);
    }

    private string DoFormat(decimal value)
    {
      return value.ToString("F" + _DecimalPlaces.ToString());
    }

    public override void UpButton()
    {
      if (ReadOnly)
        return;
      if (TextIsValid)
        Value = Math.Min(Value + Increment, Maximum);
    }

    public override void DownButton()
    {
      if (ReadOnly)
        return;
      if (TextIsValid)
        Value = Math.Max(Value - Increment, Minimum);
    }

    #endregion

    #region Исправление прокрутки колесиком мыши

    /// <summary>
    /// Событие MouseWheel не вызывается
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
    /// Упрощенная реализация прокрутки колесиком мыши.
    /// В классе UpDownBase выполнется прокрутка, зависящая от системных настроек.
    /// Обычно, за один щелчок колесика выполняется прокрутка на 3 единицы, а не 1, как при нажатии на стрелочки.
    /// Реализуем прокрутку на 1 единицу.
    /// 
    /// К сожалению, событие MouseWheel больше не удается вызвать
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnMouseWheel(MouseEventArgs args)
    {
      // base.OnMouseWheel(e);

      // Взято из UpDownBase.OnMouseWheel()
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
}
