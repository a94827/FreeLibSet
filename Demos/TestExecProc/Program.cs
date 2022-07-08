using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;

namespace TestExecProc
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
        Application.Run(new MainForm());
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }
  }
}