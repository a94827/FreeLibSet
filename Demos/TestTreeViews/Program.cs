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
        EFPApp.ShowDialog(new MainForm(), true);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка в программе");
      }
    }
  }
}
