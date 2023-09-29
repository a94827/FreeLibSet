using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace WizardDemo
{
  static class Program
  {
    public const string RegKeyName = @"HKEY_CURRENT_USER\Software\FreeLibSet\WizardDemo";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      EFPApp.InitApp();

      try
      {
        EFPApp.ConfigManager = new EFPRegistryConfigManager(RegKeyName);
        EFPApp.ShowHelpNeeded += EFPApp_ShowHelpNeeded;

        ParamForm parForm = new ParamForm();
        if (EFPApp.ShowDialog(parForm, true)==DialogResult.OK)
        {
          TemplateForm frm = new TemplateForm();
          if (parForm.efpTitle.Checked)
            frm.TheWizard.Title = "Тестирование Мастера";
          if (parForm.efpImageKey.Checked)
            frm.TheWizard.ImageKey = "CircleGreen";
          frm.TheWizard.Sizeable=parForm.efpSizeable.Checked;
          if (parForm.efpConfigSectionName.Checked)
            frm.TheWizard.ConfigSectionName = "WizardTestForm";
          if (parForm.efpHelpContext.Checked)
            frm.TheWizard.HelpContext = "TheWizard";

          frm.TheWizard.Finish += TheWizard_Finish;

          frm.TheWizard.Execute();
          EFPApp.MessageBox("Finished=" + frm.TheWizard.Finished.ToString());
        } 
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    private static void TheWizard_Finish(object sender, EventArgs args)
    {
      EFPApp.MessageBox("Wizard.Finish event");
    }

    private static void EFPApp_ShowHelpNeeded(object sender, EFPHelpContextEventArgs args)
    {
      EFPApp.MessageBox(args.HelpContext, "Help called");
    }
  }
}
