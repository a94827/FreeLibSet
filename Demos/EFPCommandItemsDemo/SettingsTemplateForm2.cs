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
  public partial class SettingsTemplateForm2 : Form
  {
    public SettingsTemplateForm2(SettingsDialog dialog)
    {
      InitializeComponent();
      dialog.Data.Add(new TestData2());
      SettingsDialogPage page = dialog.Pages.Add(MainPanel2);
      efpText = new EFPTextBox(page.BaseProvider, edText);
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
      page.Text = "Текст";
      page.ImageKey = "CircleBlue";
    }

    public class TestData2 : SettingsDataItem
    {
      public string Text;

      public override void ReadConfig(CfgPart cfg, SettingsPart part)
      {
        Text = cfg.GetString("Text");
      }

      public override void WriteConfig(CfgPart cfg, SettingsPart part)
      {
        cfg.SetString("Text", Text);
      }
    }

    private EFPTextBox efpText;

    private void DataToControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      TestData2 data2 = page.Owner.Data.GetItem<TestData2>();
      efpText.Text = data2.Text;
    }

    private void DataFromControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      TestData2 data2 = page.Owner.Data.GetItem<TestData2>();
      data2.Text = efpText.Text;
    }
  }
}
