using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.UICore;

namespace WinFormsDemo.EFPDateRangeBoxDemo
{
  public partial class DateRangeBoxForm : Form
  {
    public DateRangeBoxForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpDateRange = new EFPDateRangeBox(efpForm, edDateRange);
      efpDateOrRange = new EFPDateOrRangeBox(efpForm, edDateOrRange);

      efpCanBeEmptyMode = new EFPListComboBox(efpForm, cbCanBeEmptyMode);
      efpCanBeEmptyMode.Control.Items.AddRange(Enum.GetNames(typeof(UIValidateState)));

      efpCanBeHalfEmpty = new EFPCheckBox(efpForm, cbCanBeHalfEmpty);

      efpMinimum = new EFPDateTimeBox(efpForm, edMinimum);
      efpMinimum.CanBeEmpty = true;

      efpMaximum = new EFPDateTimeBox(efpForm, edMaximum);
      efpMaximum.CanBeEmpty = true;

      efpCanBeEmptyMode.SelectedIndex = (int)(efpDateRange.First.CanBeEmptyMode);
      efpCanBeHalfEmpty.Checked = efpDateRange.CanBeHalfEmpty;
      efpMinimum.NValue = efpDateRange.Minimum;
      efpMaximum.NValue = efpDateRange.Maximum;

      efpCanBeEmptyMode.SelectedIndexEx.ValueChanged += efpCanBeEmptyMode_ValueChanged;
      efpCanBeHalfEmpty.CheckedEx.ValueChanged += efpCanBeHalfEmpty_ValueChanged;
      efpMinimum.NValueEx.ValueChanged += efpMinimum_ValueChanged;
      efpMaximum.NValueEx.ValueChanged += efpMaximum_ValueChanged;
    }

    private void efpCanBeEmptyMode_ValueChanged(object sender, EventArgs args)
    {
      UIValidateState value = (UIValidateState)(efpCanBeEmptyMode.SelectedIndex);
      efpDateRange.First.CanBeEmptyMode = value;
      efpDateRange.Last.CanBeEmptyMode = value;
      efpDateOrRange.CanBeEmptyMode = value;
    }

    private void efpCanBeHalfEmpty_ValueChanged(object sender, EventArgs args)
    {
      efpDateRange.CanBeHalfEmpty = efpCanBeHalfEmpty.Checked;
    }

    private void efpMinimum_ValueChanged(object sender, EventArgs args)
    {
      efpDateRange.Minimum = efpMinimum.NValue;
    }

    private void efpMaximum_ValueChanged(object sender, EventArgs args)
    {
      efpDateRange.Maximum = efpMaximum.NValue;
    }

    EFPDateRangeBox efpDateRange;
    EFPDateOrRangeBox efpDateOrRange;
    EFPListComboBox efpCanBeEmptyMode;
    EFPCheckBox efpCanBeHalfEmpty;
    EFPDateTimeBox efpMinimum, efpMaximum;
  }
}
