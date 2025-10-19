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
        dlg.ListTitle = "Test mode";
        dlg.Items = new string[] {
          "EFPDateRangeBox && EFPDateOrRangeBox",
          "EFPFolderBrowserButton & EFPFileDialogButton",
          "EFPTabControl",
          "EFPTreeView & EFPTreeViewAdv",
          "EFPDBxGridFilters",
          "ManualOrderColumn",
          "EFPDataGridView & EFPDataTreeView",
          "EFPInputDataGridView",
          "Wizard",
          "DataTableEditDialog",
          "CultureInfo",
          "EFPApp.ShowAboutDialog()",
          "EFPClipboard",
          "EFPFormProvider.CloseForm()",
        };

        string[] codes = new string[] {
          "EFPDateRangeBox",
          "FileControls",
          "EFPTabControl",
          "TreeView",
          "EFPDBxGridFilters",
          "ManualOrderColumn",
          "EFPDataView",
          "EFPInputDataGridView",
          "Wizard",
          "DataTableEditDialog",
          "Culture",
          "About",
          "EFPClipboard",
          "CloseForm",
        };

        while (dlg.ShowDialog() == DialogResult.OK)
        {
          switch (codes[dlg.SelectedIndex])
          {
            case "EFPDateRangeBox":
              EFPApp.ShowDialog(new EFPDateRangeBoxDemo.DateRangeBoxForm(), true);
              break;
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
            case "EFPDataView":
              EFPDataViewDemo.EFPDataViewParamsForm.PerformTest();
              break;
            case "EFPInputDataGridView":
              EFPInputDataGridViewDemo.TestEFPInputDataGridView.PerformTest();
              break;
            case "Wizard":
              WizardDemo.WizardParamForm.PerformTest();
              break;
            case "DataTableEditDialog":
              DataTableEditDialogDemo.TestDataTableEditDialogForm.PerformTest();
              break;
            case "About":
              EFPApp.ShowAboutDialog();
              break;
            case "EFPClipboard":
              EFPApp.ShowDialog(new EFPClipboardDemo.EFPClipboardForm(), true);
              break;
            case "CloseForm":
              EFPApp.ShowDialog(new CloseFormDemo.TestCloseForm(), true);
              break;
            default:
              throw new BugException("Unknown mode");
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "App start error");
      }
    }

    private static void EFPApp_ShowHelpNeeded(object sender, EFPHelpContextEventArgs args)
    {
      EFPApp.MessageBox(args.HelpContext, "Help called");
    }
  }
}
