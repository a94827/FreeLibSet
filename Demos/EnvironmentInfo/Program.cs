using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using FreeLibSet.Core;

namespace EnvironmentInfo
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
        StringBuilder sb = new StringBuilder();
       
        sb.Append("EnvironmentTools:");
        sb.Append(Environment.NewLine);
        sb.Append("OSVersionText=");
        sb.Append(EnvironmentTools.OSVersionText);
        sb.Append(Environment.NewLine);
        sb.Append("IsWine=");
        sb.Append(EnvironmentTools.IsWine.ToString());
        sb.Append(Environment.NewLine);

        sb.Append("NetVersionText=");
        sb.Append(EnvironmentTools.NetVersionText);
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        
        sb.Append("Environment:");
        sb.Append(Environment.NewLine);
        sb.Append("OSVersion.ToString()=");
        sb.Append(Environment.OSVersion.ToString());

        MessageBox.Show(sb.ToString(), "EnvironmentTools Demo", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}
