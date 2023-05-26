using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace EFPCommandItemsDemo
{
  public partial class NewFormParamForm : Form
  {
    #region Конструктор

    public NewFormParamForm()
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpFormType = new EFPRadioButtons(efpForm, rb1);
      efpFormType.SelectedIndex = _OldFormType;

      efpMode = new EFPRadioButtons(efpForm, rbChild);
      efpMode.SelectedIndex = _OldMode;

      efpForm.FormClosing += new FormClosingEventHandler(efpForm_FormClosing);
    }

    void efpForm_FormClosing(object sender, FormClosingEventArgs args)
    {
      _OldFormType = efpFormType.SelectedIndex;
      _OldMode = efpMode.SelectedIndex;
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpFormType, efpMode;

    private static int _OldFormType = 0;
    private static int _OldMode = 0;

    #endregion
  }
}
