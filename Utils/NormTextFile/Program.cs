using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace NormTextFile
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
        EFPApp.ConfigManager = new EFPRegistryConfigManager(@"HKEY_CURRENT_USER\Software\FreeLibSet\NormTextFile");
        Application.Run(new MianForm());
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }
  }
}
