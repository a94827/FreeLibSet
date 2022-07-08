using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace TrayIconDemo
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

        EFPApp.TrayIcon.CommandItems.Add(EFPAppStdCommandItems.About).Click += new EventHandler(ciAbout_Click);
        EFPApp.TrayIcon.CommandItems.AddSeparator();
        EFPApp.TrayIcon.CommandItems.Add(EFPAppStdCommandItems.Exit).Click += new EventHandler(ciExit_Click);
        EFPApp.TrayIcon.Show();

        Application.Run();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска приложения");
      }
    }

    static void ciAbout_Click(object Sender, EventArgs Args)
    {
      EFPApp.ShowAboutDialog();
    }

    static void ciExit_Click(object Sender, EventArgs Args)
    {
      EFPApp.Exit();
    }
  }
}