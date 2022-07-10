using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Diagnostics;

namespace Bmp2Png
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
        ParamForm.PerformConvert();
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Ошибка запуска программы");
      }
    }
  }
}