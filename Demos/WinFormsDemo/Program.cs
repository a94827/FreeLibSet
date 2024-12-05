using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;

namespace WinFormsDemo
{
  static class Program
  {
    public const string RegKeyName = @"HKEY_CURRENT_USER\Software\FreeLibSet\WinFormsDemo";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      try
      {
        EFPApp.InitApp();
        EFPApp.ConfigManager = new EFPRegistryConfigManager(RegKeyName);
        EFPApp.ShowHelpNeeded += EFPApp_ShowHelpNeeded;

        ListSelectDialog dlg = new ListSelectDialog();
        dlg.Title = "WinFormsDemo";
        dlg.ListTitle = "Режим тестирования";
        dlg.Items = new string[] {
          "EFPFolderBrowserButton и EFPFileDialogButton",
          "EFPTabControl",
          "EFPTreeView и EFPTreeViewAdv",
          "EFPDBxGridFilters",
          "ManualOrderColumn",
          "Wizard",
          "CultureInfo",
          "EFPApp.ShowAboutDialog()",
        };

        string[] codes = new string[] {
          "FileControls",
          "EFPTabControl",
          "TreeView",
          "EFPDBxGridFilters",
          "ManualOrderColumn",
          "Wizard",
          "Culture",
          "About",
        };

        while (dlg.ShowDialog() == DialogResult.OK)
        {
          switch (codes[dlg.SelectedIndex])
          {
            case "Culture":
              EFPApp.ShowDialog(new CultureDemo.CultureSelForm(), true);
              break;
            case "FileControls":
              EFPApp.ShowDialog(new FileControlsDemo.FileControlsForm(), true);
              break;
            case "EFPTabControl":
              EFPApp.ShowDialog(new EFPTabControlDemo.EFPTabControlForm(), true);
              break;
            case "TreeView":
              EFPApp.ShowDialog(new TreeViewDemo.TreeViewParamsForm(), true);
              break;
            case "EFPDBxGridFilters":
              EFPDBxGridFiltersDemo.TestEFPDBxGridFilters.PerfotmTest();
              break;
              case "ManualOrderColumn":
              ManualOrderColumnDemo.ManualOrderColumnParamForm.PerformTest();
              break;
            case "Wizard":
              WizardDemo.WizardParamForm.PerformTest();
              break;
            case "About":
              EFPApp.ShowAboutDialog();
              break;
            default:
              throw new BugException("Неизвестный режим");
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    private static void EFPApp_ShowHelpNeeded(object sender, EFPHelpContextEventArgs args)
    {
      EFPApp.MessageBox(args.HelpContext, "Help called");
    }
  }
}
