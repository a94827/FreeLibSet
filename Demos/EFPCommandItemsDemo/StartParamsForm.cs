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
  public partial class StartParamsForm : Form
  {
    #region �����������

    public StartParamsForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpUIType = new EFPRadioButtons(efpForm, rbMDI);
      efpDebugWindow = new EFPCheckBox(efpForm, cbDebugWindow);
    }

    #endregion

    #region ����

    public EFPRadioButtons efpUIType;

    public EFPCheckBox efpDebugWindow;

    #endregion
  }
}