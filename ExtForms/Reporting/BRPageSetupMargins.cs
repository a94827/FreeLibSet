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
      efpLeftMargin.ToolTipText = "Расстояние от левого края листа до начала области печати (в сантиметрах)" +Environment.NewLine+
        "Обычно принтер не может печатать ближе 0.5 см от края." + Environment.NewLine +
        "Если задать меньшее поле, то часть изображения будет обрезана";
      efpLeftMargin.Control.Minimum = 0m;
      efpLeftMargin.Control.Increment = 0.1m;

      efpTopMargin = new EFPDecimalEditBox(page.BaseProvider, edTopMargin);
      efpTopMargin.ToolTipText = "Расстояние от верхнего края листа до начала области печати (в сантиметрах)" + Environment.NewLine +
        "Обычно принтер не может печатать ближе 0.5 см от края." + Environment.NewLine +
        "Если задать меньшее поле, то часть изображения будет обрезана";
      efpTopMargin.Control.Minimum = 0m;
      efpTopMargin.Control.Increment = 0.1m;

      efpRightMargin = new EFPDecimalEditBox(page.BaseProvider, edRightMargin);
      efpRightMargin.ToolTipText = "Расстояние от правого края листа до начала области печати (в сантиметрах)" + Environment.NewLine +
        "Обычно принтер не может печатать ближе 0.5 см от края." + Environment.NewLine +
        "Если задать меньшее поле, то часть изображения будет обрезана";
      efpRightMargin.Control.Minimum = 0m;
      efpRightMargin.Control.Increment = 0.1m;

      efpBottomMargin = new EFPDecimalEditBox(page.BaseProvider, edBottomMargin);
      efpBottomMargin.ToolTipText = "Расстояние от нижнего края листа до начала области печати (в сантиметрах)" + Environment.NewLine +
        "Обычно принтер не может печатать ближе 0.5 см от края." + Environment.NewLine +
        "Если задать меньшее поле, то часть изображения будет обрезана";
      efpBottomMargin.Control.Minimum = 0m;
      efpBottomMargin.Control.Increment = 0.1m;

      page.Text ="Поля";
      page.ToolTipText="Размеры полей по краям бумаги"; 
      page.ImageKey="PaperMargins";
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

    EFPDecimalEditBox efpLeftMargin, efpTopMargin, efpRightMargin, efpBottomMargin;
  }
}
