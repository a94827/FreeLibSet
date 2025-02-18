using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Forms;

namespace WinFormsDemo.WizardDemo
{
  public partial class WizardParamForm : Form
  {
    public WizardParamForm()
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpTitle = new EFPCheckBox(efpForm, cbTitle);
      efpImageKey = new EFPCheckBox(efpForm, cbImageKey);
      efpSizeable = new EFPCheckBox(efpForm, cbSizeable);
      efpConfigSectionName = new EFPCheckBox(efpForm, cbConfigSectionName);
      efpHelpContext = new EFPCheckBox(efpForm, cbHelpContext);

      efpForm.FormClosing += EfpForm_FormClosing;

      try
      {
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo("TestParams", EFPConfigCategories.UserParams), EFPConfigMode.Read, out cfg))
        {
          efpTitle.Checked = cfg.GetBoolDef("UseTitle", true);
          efpImageKey.Checked = cfg.GetBoolDef("UseImageKey", true);
          efpSizeable.Checked = cfg.GetBoolDef("Sizeable", true);
          efpConfigSectionName.Checked = cfg.GetBoolDef("UseConfigSectionName", true);
          efpHelpContext.Checked = cfg.GetBoolDef("UseHelpContext", false);
        }
      }
      catch { }
    }

    private void EfpForm_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (DialogResult == DialogResult.OK)
      {
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo("TestParams", EFPConfigCategories.UserParams), EFPConfigMode.Write, out cfg))
        {
          cfg.SetBool("UseTitle", efpTitle.Checked);
          cfg.SetBool("UseImageKey", efpImageKey.Checked);
          cfg.SetBool("Sizeable", efpSizeable.Checked);
          cfg.SetBool("UseConfigSectionName", efpConfigSectionName.Checked);
          cfg.SetBool("UseHelpContext", efpHelpContext.Checked);
        }
      }
    }

    public EFPCheckBox efpTitle, efpImageKey, efpSizeable, efpConfigSectionName, efpHelpContext;

    #region Статический метод запуска теста

    internal static void PerformTest()
    {
      WizardParamForm parForm = new WizardParamForm();
      if (EFPApp.ShowDialog(parForm, true) == DialogResult.OK)
      {
        WizardTemplateForm frm = new WizardTemplateForm();
        if (parForm.efpTitle.Checked)
          frm.TheWizard.Title = "Wizard test";
        if (parForm.efpImageKey.Checked)
          frm.TheWizard.ImageKey = "CircleGreen";
        frm.TheWizard.Sizeable = parForm.efpSizeable.Checked;
        if (parForm.efpConfigSectionName.Checked)
          frm.TheWizard.ConfigSectionName = "WizardTestForm";
        if (parForm.efpHelpContext.Checked)
          frm.TheWizard.HelpContext = "TheWizard";

        frm.TheWizard.Finish += TheWizard_Finish;

        frm.TheWizard.Execute();
        EFPApp.MessageBox("Finished=" + frm.TheWizard.Finished.ToString(), "Wizard test finished");
      }
    }

    private static void TheWizard_Finish(object sender, EventArgs args)
    {
      EFPApp.MessageBox("Wizard.Finish event handler");
    }

    #endregion
  }
}
