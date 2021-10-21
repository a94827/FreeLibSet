using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Data;

using FreeLibSet.DependedValues;
using FreeLibSet.Config;
using FreeLibSet.Data.Docs;
using FreeLibSet.Calendar;

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
  internal partial class DateRangeInclusionGridFilterForm : Form
  {
    #region Конструктор

    public DateRangeInclusionGridFilterForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("Filter");
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpMode = new EFPRadioButtons(efpForm, rbNone);

      efpDate = new EFPDateBox(efpForm, edDate);
      efpDate.CanBeEmpty = false;
      efpDate.DisabledValue = DateTime.Today;
      efpDate.AllowDisabledValue = true;
      efpDate.EnabledEx = efpMode[2].CheckedEx;
      efpDate.VisibleEx = new DepNot(efpMode[0].CheckedEx);
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpMode;

    public EFPDateBox efpDate;

    #endregion
  }

  /// <summary>
  /// Фильтр по интервалу дат
  /// В таблице должно быть два поля типа даты, которые составляют интервал дат.
  /// В фильтре задается дата. В просмотр попадают строки, в которых интервал дат
  /// включает в себя эту дату. Обрабатываются открытые и полуоткрытые интервалы,
  /// когда одно или оба поля содержат NULL
  /// Эта версия фильтра поддерживает только текущую системную дату
  /// </summary>
  public class DateRangeInclusionGridFilter : DateRangeInclusionCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstDateColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastDateColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeInclusionGridFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
    }

    #endregion

    #region Свойства, которые можно переопределить для использования рабочей даты

    /// <summary>
    /// Непереопределенный метод возвращает "Текущая дата".
    /// Если производный класс переопределяет свойство WorkData, то он, вероятно,
    /// будет переопределять и это свойство
    /// </summary>
    public virtual string WorkDateText { get { return "Текущая дата"; } }

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
        if (Date.HasValue)
        {
          string s = DateRangeFormatter.Default.ToString(Date.Value, true);
          if (UseWorkDate)
            s += " (" + WorkDateText + ")";
          return s;
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DateRangeInclusionGridFilterForm Form = new DateRangeInclusionGridFilterForm();
      Form.Text = DisplayName;
      Form.efpMode[1].Control.Text = WorkDateText;
      Form.efpDate.DisabledValue = WorkDate;


      if (Date.HasValue)
      {
        if (UseWorkDate)
          Form.efpMode.SelectedIndex = 1;
        else
        {
          Form.efpMode.SelectedIndex = 2;
          Form.efpDate.Value = Date;
        }
      }
      else
        Form.efpMode.SelectedIndex = 0;

      if (EFPApp.ShowDialog(Form, true, dialogPosition) != DialogResult.OK)
        return false;
      switch (Form.efpMode.SelectedIndex)
      {
        case 0:
          Date = null;
          break;
        case 1:
          UseWorkDate = true;
          Date = WorkDate;
          break;
        case 2:
          UseWorkDate = false;
          Date = Form.efpDate.Value;
          break;
      }
      return true;
    }

    #endregion
  }
}