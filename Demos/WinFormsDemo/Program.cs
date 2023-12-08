using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;

namespace WinFormsDemo
{
  static class Program
  {
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
        EFPApp.ConfigManager = new EFPRegistryConfigManager(@"HKEY_CURRENT_USER\Software\FreeLibSet\WinFormsDemo");
        ListSelectDialog dlg = new ListSelectDialog();
        dlg.Title = "WinFormsDemo";
        dlg.ListTitle = "Режим тестирования";
        dlg.Items = new string[] {
          "EFPFolderBrowserButton и EFPFileDialogButton",
        };

        string[] codes = new string[] {
          "FileControls",
        };

        while (dlg.ShowDialog() == DialogResult.OK)
        {
          switch (codes[dlg.SelectedIndex])
          {
            case "FileControls":
              EFPApp.ShowDialog(new FileControlsForm(), true);
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
  }
}
