using FreeLibSet.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Data
{
  internal partial class DateRangeGridFilterForm : Form
  {
    #region Конструктор формы

    public DateRangeGridFilterForm()
    {
      InitializeComponent();
      FormProvider = new EFPFormProvider(this);

      ModeButtons = new EFPRadioButtons(FormProvider, rbRange, rbNotNull, rbNull);
      TheDateRangeBox = new EFPDateRangeBox(FormProvider, edRange);
      TheDateRangeBox.First.CanBeEmpty = true;
      TheDateRangeBox.Last.CanBeEmpty = true;
      TheDateRangeBox.CanBeHalfEmpty = true;
      TheDateRangeBox.First.EnabledEx = ModeButtons[0].CheckedEx;
      TheDateRangeBox.Last.EnabledEx = ModeButtons[0].CheckedEx;
    }

    #endregion

    #region Поля


    public readonly EFPFormProvider FormProvider;

    public readonly EFPRadioButtons ModeButtons;

    public readonly EFPDateRangeBox TheDateRangeBox;

    #endregion
  }
}
