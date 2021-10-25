using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.UICore;

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

namespace FreeLibSet.Forms
{
  internal partial class DateRangeForm : Form
  {
    public DateRangeForm()
    {
      InitializeComponent();
      EFPApp.InitFormImages(this);

      FormProvider = new EFPFormProvider(this);
      TheDateRangeBox = new EFPDateRangeBox(FormProvider, edRange);
    }

    public readonly EFPFormProvider FormProvider;

    public readonly EFPDateRangeBox TheDateRangeBox;
  }

  /// <summary>
  /// Диалог редактирования интервала дат
  /// </summary>
  public class DateRangeDialog: BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует значения свойств по умолчанию
    /// </summary>
    public DateRangeDialog()
    {
      Title = "Интервал дат";
      Prompt = String.Empty;
      _FirstDate = null;
      _LastDate = null;
      CanBeEmpty = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: Начальная дата диапазона.
    /// Значение null после завершения диалога может быть только при CanBeEmpty=true.
    /// </summary>
    public DateTime? FirstDate
    {
      get { return _FirstDate; }
      set
      {
        if (value.HasValue)
          _FirstDate = value.Value.Date;
        else
          _FirstDate = null;
      }
    }
    private DateTime? _FirstDate;

    /// <summary>
    /// Вход и выход: Конечная дата диапазона.
    /// Значение null после завершения диалога может быть только при CanBeEmpty=true.
    /// </summary>
    public DateTime? LastDate
    {
      get { return _LastDate; }
      set
      {
        if (value.HasValue)
          _LastDate = value.Value.Date;
        else
          _LastDate = null;
      }
    }
    private DateTime? _LastDate;

    /// <summary>
    /// Если свойство установлено в true, то разрешается ввод полу- или полностью открытых интервалов.
    /// Если false (по умолчанию), то обе даты должны быть введены
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Надо ли выводить предупреждение, если значения не введены.
    /// По умолчанию - false.
    /// Свойство действует, только если свойство CanBeEmpty=true.
    /// </summary>
    public bool WarningIfEmpty { get { return _WarningIfEmpty; } set { _WarningIfEmpty = value; } }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Если задано значение, отличное от null, то не разрешается вводить даты, ранее указанной.
    /// </summary>
    public DateTime? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private DateTime? _Minimum;

    /// <summary>
    /// Если задано значение, отличное от null, то не разрешается вводить даты, позднее указанной.
    /// </summary>
    public DateTime? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private DateTime? _Maximum;

    /// <summary>
    /// Обработчик для проверки корректности значений при вводе.
    /// Обработчик не вызывается, если даты находятся вне диапазона
    /// </summary>
    public event EFPValidatingTwoValuesEventHandler<DateTime?, DateTime?> Validating;

    #endregion

    #region Методы

    /// <summary>
    /// Показывает блок диалога.
    /// </summary>
    /// <returns>Результат выполнения</returns>
    public override DialogResult ShowDialog()
    {
      DateRangeForm form = new DateRangeForm();
      base.InitFormTitle(form);
      form.FormProvider.HelpContext = HelpContext;
      form.MainLabel.Text = Prompt;

      form.TheDateRangeBox.FirstDate.CanBeEmpty = CanBeEmpty;
      form.TheDateRangeBox.LastDate.CanBeEmpty = CanBeEmpty;
      form.TheDateRangeBox.FirstDate.WarningIfEmpty = WarningIfEmpty;
      form.TheDateRangeBox.LastDate.WarningIfEmpty = WarningIfEmpty;
      form.TheDateRangeBox.FirstDate.Minimum = Minimum;
      form.TheDateRangeBox.LastDate.Minimum = Minimum;
      form.TheDateRangeBox.FirstDate.Maximum = Maximum;
      form.TheDateRangeBox.LastDate.Maximum = Maximum;

      if (HasConfig)
      {
        _FirstDate = ConfigPart.GetNullableDate(ConfigName + "-FirstDate");
        _LastDate = ConfigPart.GetNullableDate(ConfigName + "-LastDate");
      }

      form.TheDateRangeBox.FirstDate.NValue = _FirstDate;
      form.TheDateRangeBox.LastDate.NValue = _LastDate;

      EFPFormCheck fc = new EFPFormCheck(form.FormProvider);
      fc.Validating += FormCheck;
      fc.Tag = form;

      if (EFPApp.ShowDialog(form, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      _FirstDate = form.TheDateRangeBox.FirstDate.NValue;
      _LastDate = form.TheDateRangeBox.LastDate.NValue;

      if (HasConfig)
      {
        ConfigPart.SetNullableDate(ConfigName + "-FirstDate", _FirstDate);
        ConfigPart.SetNullableDate(ConfigName + "-LastDate", _LastDate);
      }

      return DialogResult.OK;
    }

    private void FormCheck(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      if (Validating != null)
      { 
        EFPFormCheck FormCheck=(EFPFormCheck)sender;
        DateRangeForm Form = (DateRangeForm )(FormCheck.Tag);

        EFPValidatingTwoValuesEventArgs<DateTime?, DateTime?> Args2 =
          new EFPValidatingTwoValuesEventArgs<DateTime?, DateTime?>(args,
          Form.TheDateRangeBox.FirstDate.NValue,
          Form.TheDateRangeBox.LastDate.NValue);

        Validating(this, Args2);
      }
    }

    #endregion
  }
}