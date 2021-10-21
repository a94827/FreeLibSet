using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Data;

using FreeLibSet.Data.Docs;
using FreeLibSet.Config;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  internal partial class YearGridFilterForm : Form
  {
    #region Конструктор

    public YearGridFilterForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("Filter");

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpMode = new EFPRadioButtons(efpForm, rbNoFilter);
      efpYear = new EFPExtNumericUpDown(efpForm, edYear);
      efpYear.EnabledEx = efpMode[1].CheckedEx;
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpMode;

    public EFPExtNumericUpDown efpYear;

    #endregion
  }

  /// <summary>
  /// Фильтр по году для числового поля или поля типа "Дата"
  /// </summary>
  public class YearGridFilter : YearCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для числового поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public YearGridFilter(string columnName)
      : base(columnName)
    {
    }

    /// <summary>
    /// Конструктор для поля типа "Дата" или числового поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="isDateColumn">Если true, то поле имеет тип "Дата".
    /// Если false, то поле является числовым</param>
    public YearGridFilter(string columnName, bool isDateColumn)
      : base(columnName, isDateColumn)
    {
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.ToString();
        else
          return string.Empty;
      }
    }

    /// <summary>
    /// Сохраняем год между вызовами
    /// </summary>
    private static int _PrevYear = DateTime.Today.Year;

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      YearGridFilterForm Form = new YearGridFilterForm();
      Form.Text = DisplayName;

      Form.efpYear.IntValue = _PrevYear;

      if (Value.HasValue)
      {
        Form.efpMode.SelectedIndex = 1;
        Form.efpYear.IntValue = Value.Value;
      }
      else
        Form.efpMode.SelectedIndex = 0;

      if (EFPApp.ShowDialog(Form, true, dialogPosition) != DialogResult.OK)
        return false;

      if (Form.efpMode.SelectedIndex == 0)
        Value = null;
      else
        Value = Form.efpYear.IntValue;

      _PrevYear = Form.efpYear.IntValue;

      return true;
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollUp
    {
      get { return Value != 0 && Value > 1; }
    }

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollDown
    {
      get { return Value != 0 && Value < 9999; }
    }

    /// <summary>
    /// Уменьшает год на 1
    /// </summary>
    public void ScrollUp()
    {
      Value--;
    }

    /// <summary>
    /// Увеличивает год на 1
    /// </summary>
    public void ScrollDown()
    {
      Value++;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр на попадание года в интервал.
  /// В таблице должно быть два числовых поля, задающих первый и последний год.
  /// Строка проходит фильтр, если заданный в фильтре год (Value) попадает в диапазон.
  /// Обрабатываются значения типа NULL, задающие открытые интервалы
  /// </summary>
  public class YearRangeInclusionGridFilter : YearRangeInclusionCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для числового поля
    /// </summary>
    /// <param name="firstYearFieldName"></param>
    /// <param name="lastYearFieldName"></param>
    public YearRangeInclusionGridFilter(string firstYearFieldName, string lastYearFieldName)
      : base(firstYearFieldName, lastYearFieldName)
    {
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value == 0)
          return string.Empty;
        else
          return Value.ToString();
      }
    }

    /// <summary>
    /// Сохраняем год между вызовами
    /// </summary>
    private static int _PrevYear = DateTime.Today.Year;

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      YearGridFilterForm Form = new YearGridFilterForm();
      Form.Text = DisplayName;
      Form.efpYear.IntValue = _PrevYear;

      if (Value == 0)
        Form.efpMode.SelectedIndex = 0;
      else
      {
        Form.efpMode.SelectedIndex = 1;
        Form.efpYear.IntValue = Value;
      }

      if (EFPApp.ShowDialog(Form, true, dialogPosition) != DialogResult.OK)
        return false;

      if (Form.efpMode.SelectedIndex == 0)
        Value = 0;
      else
        Value = Form.efpYear.IntValue;
      _PrevYear = Form.efpYear.IntValue;
      return true;
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollUp
    {
      get { return Value != 0 && Value > 1; }
    }

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollDown
    {
      get { return Value != 0 && Value < 9999; }
    }

    /// <summary>
    /// Уменьшает год на 1
    /// </summary>
    public void ScrollUp()
    {
      Value--;
    }

    /// <summary>
    /// Увеличивает год на 1
    /// </summary>
    public void ScrollDown()
    {
      Value++;
    }

    #endregion
  }
}