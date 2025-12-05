using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace LogWatcher
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
        EFPApp.ConfigManager = new EFPRegistryConfigManager(@"HKEY_CURRENT_USER\Software\FreeLibSet\LogWatcher");

        TheForm = new MainForm();

        EFPCommandItem ciShow = new EFPCommandItem("View", "Show");
        ciShow.MenuText = "Open";
        ciShow.ImageKey = "Table";
        ciShow.Click += CiShow_Click;
        EFPApp.TrayIcon.CommandItems.Add(ciShow);
        EFPApp.TrayIcon.DefaultCommandItem = ciShow;

        EFPCommandItem ciAbout = EFPAppCommandItems.CreateStdCommand(EFPAppStdCommandItems.About);
        ciAbout.Click += CiAbout_Click;
        EFPApp.TrayIcon.CommandItems.Add(ciAbout);

        EFPCommandItem ciExit = EFPAppCommandItems.CreateStdCommand(EFPAppStdCommandItems.Exit);
        ciExit.Click += CiExit_Click;
        ciExit.GroupBegin = true;
        EFPApp.TrayIcon.CommandItems.Add(ciExit);

        EFPApp.TrayIcon.Show();

        TheWatcher = new DirWatcher();

        Application.Run();

        TheWatcher.Dispose();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    public static MainForm TheForm;

    public static DirWatcher TheWatcher;

    private static void CiShow_Click(object sender, EventArgs args)
    {
      TheForm.Show();
      TheForm.Activate();
    }

    public static void CiAbout_Click(object sender, EventArgs args)
    {
      EFPApp.ShowAboutDialog();
    }

    public static void CiExit_Click(object sender, EventArgs args)
    {
      EFPApp.Exit();
    }
  }
}
