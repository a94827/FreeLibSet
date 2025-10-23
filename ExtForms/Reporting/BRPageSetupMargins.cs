using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Reporting;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRPageSetupMargins : Form
  {
    public BRPageSetupMargins(SettingsDialog dialog)
    {
      InitializeComponent();
      SettingsDialogPage page = dialog.Pages.Add(panMargins);

      efpLeftMargin = new EFPDecimalEditBox(page.BaseProvider, edLeftMargin);
      efpLeftMargin.Control.Minimum = 0m;
      efpLeftMargin.Control.Increment = 0.1m;

      efpTopMargin = new EFPDecimalEditBox(page.BaseProvider, edTopMargin);
      efpTopMargin.Control.Minimum = 0m;
      efpTopMargin.Control.Increment = 0.1m;

      efpRightMargin = new EFPDecimalEditBox(page.BaseProvider, edRightMargin);
      efpRightMargin.Control.Minimum = 0m;
      efpRightMargin.Control.Increment = 0.1m;

      efpBottomMargin = new EFPDecimalEditBox(page.BaseProvider, edBottomMargin);
      efpBottomMargin.Control.Minimum = 0m;
      efpBottomMargin.Control.Increment = 0.1m;

      page.Text = Res.BRPageSetupMargins_Title_Tab;
      page.ToolTipText = Res.BRPageSetupMargins_ToolTip_Tab;
      page.ImageKey = "PaperMargins";
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
    }

    private void DataToControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      BRPageSetup ps = page.Owner.Data.GetItem<BRPageSettingsDataItem>().PageSetup;
      efpLeftMargin.Value = (decimal)(ps.LeftMargin) / 100m;
      efpTopMargin.Value = (decimal)(ps.TopMargin) / 100m;
      efpRightMargin.Value = (decimal)(ps.RightMargin) / 100m;
      efpBottomMargin.Value = (decimal)(ps.BottomMargin) / 100m;
    }

    private void DataFromControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      BRPageSetup ps = page.Owner.Data.GetItem<BRPageSettingsDataItem>().PageSetup;
      ps.LeftMargin = (int)(Math.Round(efpLeftMargin.Value * 100m, 0, MidpointRounding.AwayFromZero));
      ps.TopMargin = (int)(Math.Round(efpTopMargin.Value * 100m, 0, MidpointRounding.AwayFromZero));
      ps.RightMargin = (int)(Math.Round(efpRightMargin.Value * 100m, 0, MidpointRounding.AwayFromZero));
      ps.BottomMargin = (int)(Math.Round(efpBottomMargin.Value * 100m, 0, MidpointRounding.AwayFromZero));
    }

    private readonly EFPDecimalEditBox efpLeftMargin, efpTopMargin, efpRightMargin, efpBottomMargin;
  }
}
