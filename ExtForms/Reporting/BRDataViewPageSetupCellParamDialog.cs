using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupCellParamDialog : Form
  {
    #region Конструктор

    public BRDataViewPageSetupCellParamDialog()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["Table"];
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpLeftMargin = new EFPDecimalEditBox(efpForm, edLeftMargin);
      efpLeftMargin.Control.Minimum = 0m;
      efpLeftMargin.Control.Increment = 0.1m;

      efpTopMargin = new EFPDecimalEditBox(efpForm, edTopMargin);
      efpTopMargin.Control.Minimum = 0m;
      efpTopMargin.Control.Increment = 0.1m;

      efpRightMargin = new EFPDecimalEditBox(efpForm, edRightMargin);
      efpRightMargin.Control.Minimum = 0m;
      efpRightMargin.Control.Increment = 0.1m;

      efpBottomMargin = new EFPDecimalEditBox(efpForm, edBottomMargin);
      efpBottomMargin.Control.Minimum = 0m;
      efpBottomMargin.Control.Increment = 0.1m;
    }

    #endregion

    #region Поля

    public EFPDecimalEditBox efpLeftMargin, efpTopMargin, efpRightMargin, efpBottomMargin;

    #endregion
  }
}
