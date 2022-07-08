using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;

namespace TestCalculator
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

      Application.Run(new MainForm());
    }
  }
}