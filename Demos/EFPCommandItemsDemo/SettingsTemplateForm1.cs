using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Forms;

namespace EFPCommandItemsDemo
{
  public partial class SettingsTemplateForm1 : Form
  {
    public SettingsTemplateForm1(SettingsDialog dialog)
    {
      InitializeComponent();

      dialog.Data.Add(new TestData1());

      SettingsDialogPage page = dialog.Pages.Add(MainPanel1);
      efpOption1 = new EFPCheckBox(page.BaseProvider, cbOption1);
      efpOption2 = new EFPCheckBox(page.BaseProvider, cbOption2);
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
      page.Text = "Опции 1 и 2";
      page.ImageKey = "CircleGreen";
    }

    private EFPCheckBox efpOption1, efpOption2;

    public class TestData1 : SettingsDataItem
    {
      public bool Option1, Option2;

      public override void ReadConfig(CfgPart cfg, SettingsPart part)
      {
        Option1 = cfg.GetBool("Option1");
        Option2 = cfg.GetBool("Option2");
      }

      public override void WriteConfig(CfgPart cfg, SettingsPart part)
      {
        cfg.SetBool("Option1", Option1);
        cfg.SetBool("Option2", Option2);
      }
    }

    public void DataToControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      TestData1 data2 = page.Owner.Data.GetItem<TestData1>();
      efpOption1.Checked = data2.Option1;
      efpOption2.Checked = data2.Option2;
    }

    public void DataFromControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      TestData1 data2 = page.Owner.Data.GetItem<TestData1>();
      data2.Option1 = efpOption1.Checked;
      data2.Option2 = efpOption2.Checked;
    }
  }
}
