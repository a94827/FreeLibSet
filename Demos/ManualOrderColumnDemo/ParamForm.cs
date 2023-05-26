using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace ManualOrderColumnDemo
{
  public partial class ParamForm : Form
  {
    #region Конструктор

    public ParamForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpView = new EFPRadioButtons(efpForm, rbEFPDataGridView);
      efpView.SelectedIndex = 0;

      efpManualOrderColumn = new EFPCheckBox(efpForm, cbManualOrderColumn);
      efpManualOrderColumn.Checked = true;

      efpDefaultManualOrderColumn = new EFPCheckBox(efpForm, cbDefaultManualOrderColumn);

      efpReadOnly = new EFPCheckBox(efpForm, cbReadOnly);

      efpMultiSelect = new EFPCheckBox(efpForm, cbMultiSelect);
      efpMultiSelect.Checked = true;

      efpOrderDataType = new EFPListComboBox(efpForm, cbOrderDataType);
      efpOrderDataType.SelectedIndex = 0;

      efpOrderStartMode = new EFPListComboBox(efpForm, cbOrderStartMode);
      efpOrderStartMode.SelectedIndex = 0;
    }

    #endregion

    #region Свойства

    public EFPRadioButtons efpView;

    public EFPCheckBox efpManualOrderColumn, efpDefaultManualOrderColumn, efpReadOnly, efpMultiSelect;

    public EFPListComboBox efpOrderDataType;

    public EFPListComboBox efpOrderStartMode;

    #endregion
  }
}
