using System;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace RegeditDemo
{
  internal static class Program
  {
    const string ConfigRegKey = @"HKEY_CURRENT_USER\Software\FreeLibSet\RegEditDemo";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      //// To customize application configuration such as set high DPI settings or default font,
      //// see https://aka.ms/applicationconfiguration.
      //ApplicationConfiguration.Initialize();

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