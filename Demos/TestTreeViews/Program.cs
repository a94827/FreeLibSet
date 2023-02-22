using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;

namespace TestTreeViews
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
      EFPApp.InitApp();

      try
      {
        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = EnvironmentTools.ApplicationName;
        dlg.GroupTitle = "Режим тестирования";
        dlg.Items = new string[] {
          "CheckBoxes=false", 
          "CheckBoxes=true" };
        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        switch (dlg.SelectedIndex)
        {
          case 0: Application.Run(new TestCheckBoxesForm(false)); break;
          case 1: Application.Run(new TestCheckBoxesForm(true)); break;
          default:
            throw new BugException();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка в программе");
      }
    }
  }
}
