using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Reporting;
using FreeLibSet.Models.Tree;
using FreeLibSet.Reporting;

namespace BRReportDemo
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
        Application.Run(new MainForm());
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }
  }
}