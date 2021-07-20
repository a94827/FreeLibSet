using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;

namespace FileEnumeratorDemo
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
        Application.Run(new ParamForm());
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }
  }
}