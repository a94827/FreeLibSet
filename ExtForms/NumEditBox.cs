using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using FreeLibSet.Forms;

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

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Расширение поля ввода для ввода числовых значений (по типу
  /// NumericUpDown, но без стрелочек)
  /// </summary>
  [Description("Поле ввода числового значения")]
  [ToolboxBitmap(typeof(NumEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class NumEditBox : TextBox, ISupportInitialize
  {
    #region Конструктор

    public NumEditBox()
    {
      _DecimalPlaces = -1;
      base.TextAlign = HorizontalAlignment.Right;
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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new string[] Lines
    {
      get { return base.Lines; }
      set { }
    }

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // 14.06.2019. Перенесено от свойства NullableValue
    [DefaultValue(typeof(decimal), "0")]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Текущее значение. Если задано DecimalPlace=0, то выполняется преобразование в целое число")]
    [Category("Appearance")]
    public decimal Value
    {
      get
      {
        _TextIsValid = true;
        string s = base.Text;
        if (s == null)
          s = String.Empty; // на всякий случай
        s = s.Trim(); // 28.12.2014
        if (s.Length == 0)
          return 0m;
        if (_DecimalPlaces == 0)
        {
          long x;
          if (long.TryParse(s, out x))
            return (decimal)x;
          else
          {
            _TextIsValid = false;
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
            _TextIsValid = false;
            return 0m;
          }
        }
      }
      set
      {
        if (value == Value)
          return;
        base.Text = DoFormat(value);
      }
    }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Число десятичных разрядов после точки. По умолчанию - (-1), что означает " +
       "отсутствие контроля за числом разрядов.")]
    [Category("Appearance")]
    [DefaultValue(-1)]
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
        if (value < -1 || value > 10)
          throw new ArgumentOutOfRangeException("value", value, "Число десятичных знаков после запятой должно быть в диапазоне от -1 до 10");
        _DecimalPlaces = value;
        base.Text = DoFormat(NullableValue);
      }
    }
    private int _DecimalPlaces;

    public string Format
    {
      get
      {
        if (DecimalPlaces < 0)
          return String.Empty;
        if (DecimalPlaces == 0)
          return "0";
        return "0." + new string('0', DecimalPlaces);
      }
    }

    [Description("Представление нулевого значения как пустой строки (по умолчанию-false)")]
    [DefaultValue(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    public bool ZeroAsEmptyText
    {
      get { return _ZeroAsEmptyText; }
      set
      {
        if (value == _ZeroAsEmptyText)
          return;
        _ZeroAsEmptyText = value;
        if (value && CanBeEmpty)
          CanBeEmpty = false;
        else
          base.Text = DoFormat(NullableValue);
      }
    }
    private bool _ZeroAsEmptyText;


    [Description("Представление нулевого значения как пустой строки (по умолчанию-false)")]
    [DefaultValue(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        if (value == _CanBeEmpty)
          return;
        _CanBeEmpty = value;
        if (value && ZeroAsEmptyText)
          ZeroAsEmptyText = false;
        else
          base.Text = DoFormat(NullableValue);
      }
    }
    private bool _CanBeEmpty;

    // Свойство NullableValue должно идти после CanBeEmpty, для правильной сериализации

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [DefaultValue(typeof(decimal?), "null")]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Текущее значение с выделением пустого значения. Если задано DecimalPlace=0, то выполняется преобразование в целое число")]
    [Category("Appearance")]
    public decimal? NullableValue
    {
      get
      {
        _TextIsValid = true;
        string s = base.Text;
        if (String.IsNullOrEmpty(s))
          return null;
        if (_DecimalPlaces == 0)
        {
          long x;
          if (long.TryParse(s, out x))
            return (decimal)x;
          else
          {
            _TextIsValid = false;
            return null;
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
            _TextIsValid = false;
            return null;
          }
        }
      }
      set
      {
        //if (value == Value)
        //  return;
        if (value.HasValue)
          base.Text = DoFormat(value);
        else
          base.Text = String.Empty;
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new int MaxLength
    { get { return base.MaxLength; } }

    [Description("Горизонтальное выравнивание (по умолчанию - по правому краю)")]
    [Localizable(true)]
    [DefaultValue(HorizontalAlignment.Right)]
    [Category("Appearance")]
    public new HorizontalAlignment TextAlign
    {
      get { return base.TextAlign; }
      set { base.TextAlign = value; }
    }

    /// <summary>
    /// Свойство возвращает true, если текущий введенный текст может быть преобразован в число
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    //    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public bool TextIsValid { get { return _TextIsValid; } }
    private bool _TextIsValid;

    #endregion

    #region События

    [Description("Посылается после изменения свойства Value")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    #endregion

    #region Внутренняя реализация

    protected override void OnTextChanged(EventArgs args)
    {
      base.OnTextChanged(args);

      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
    }

    protected override void OnLeave(EventArgs args)
    {
      base.OnLeave(args);

      decimal Dummy = Value;
      if (_TextIsValid) // 27.12.2014
        base.Text = DoFormat(NullableValue);
    }



    private string DoFormat(decimal? value)
    {
      if (!value.HasValue)
      {
        if (CanBeEmpty)
          return String.Empty;
        else
          value = 0m;
      }

      if (_ZeroAsEmptyText && value.Value == 0m)
        return String.Empty;
      if (_DecimalPlaces < 0)
        return value.Value.ToString();
      else
        return value.Value.ToString("F" + _DecimalPlaces.ToString());
    }

    #endregion

    #region Скрытие свойств

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AcceptsReturn { get { return base.AcceptsReturn; } set { base.AcceptsReturn = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AcceptsTab { get { return base.AcceptsTab; } set { base.AcceptsTab = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new CharacterCasing CharacterCasing { get { return base.CharacterCasing; } set { base.CharacterCasing = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool Multiline { get { return base.Multiline; } set { base.Multiline = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new char PasswordChar { get { return base.PasswordChar; } set { base.PasswordChar = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool ShortcutsEnabled { get { return base.ShortcutsEnabled; } set { base.ShortcutsEnabled = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool UseSystemPasswordChar { get { return base.UseSystemPasswordChar; } set { base.UseSystemPasswordChar = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool WordWrap { get { return base.WordWrap; } set { base.WordWrap = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new AutoCompleteStringCollection AutoCompleteCustomSource { get { return base.AutoCompleteCustomSource; } set { base.AutoCompleteCustomSource = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new AutoCompleteMode AutoCompleteMode { get { return base.AutoCompleteMode; } set { base.AutoCompleteMode = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new AutoCompleteSource AutoCompleteSource { get { return base.AutoCompleteSource; } set { base.AutoCompleteSource = value; } }

    #endregion

    #region ISupportInitialize Members

    public void BeginInit()
    {
    }

    public void EndInit()
    {
      if (CanBeEmpty && Value == 0m)
        NullableValue = null; // 14.06.2019
    }

    #endregion
  }
}
