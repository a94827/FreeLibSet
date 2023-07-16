using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using FreeLibSet.Forms;

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

        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons["CircleGreen"];
        EFPApp.TrayIcon.CommandItems.Add(EFPAppStdCommandItems.About).Click += new EventHandler(ciAbout_Click);
        EFPApp.TrayIcon.CommandItems.AddSeparator();
        EFPApp.TrayIcon.CommandItems.Add(EFPAppStdCommandItems.Exit).Click += new EventHandler(ciExit_Click);
        EFPApp.Timers.Add(new TestActivity());
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

    private class TestActivity : IEFPAppTimeHandler
    {
      int count;

      private static readonly string[] ImageNames = new string[] { "CircleGreen", "CircleRed", "CircleYellow", "CircleBlue" };
      public void TimerTick()
      {
        count++;
        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons[ImageNames[count % ImageNames.Length]];
      }
    }
  }
}
