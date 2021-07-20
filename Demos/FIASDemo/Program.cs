using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
//using AgeyevAV.ExtDB.SQLite;
//using System.Data.SQLite;
using AgeyevAV.IO;
using AgeyevAV;
using AgeyevAV.FIAS;
using AgeyevAV.Caching;
using AgeyevAV.Config;
using System.Data;
using AgeyevAV.ExtDB;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FIASDemo
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

#if XXX
      DataTable table = new DataTable();
      table.Columns.Add("x", typeof(Guid));
      table.Columns.Add("y", typeof(Int32));


      table.Rows.Add(new Guid("{00020833-0000-0000-C000-000000000046}"), 1);
      table.Rows.Add(new Guid("{00000103-0000-0010-8000-00AA006D2EA4}"), 2);
      table.Rows.Add(new Guid("{00000301-A8F2-4877-BA0A-FD2B6645FB94}"), 3);
      table.Rows.Add(new Guid("{0000030B-0000-0000-C000-000000000046}"), 4);

      DataTools.SetPrimaryKey(table, "x");

      string ssss=table.Rows.Find(new Guid("{00000301-A8F2-4877-BA0A-FD2B6645FB94}"))["y"].ToString();
#endif

      EFPApp.InitApp();
      try
      {
        TempDirectory.RootDir = new AbsPath(FileTools.ApplicationBaseDir, "Temp");
        Cache.Params.PersistDir = new AbsPath(FileTools.ApplicationBaseDir, "PersistCache");

        // создаем log-файл для трассировки запросов
        FileTools.ForceDirs(TempDirectory.RootDir);
        AbsPath traceFile = new AbsPath(TempDirectory.RootDir, "trace.log");
        Trace.AutoFlush = true;
        TextWriterTraceListener Listener = new TextWriterTraceListener(new StreamWriter(traceFile.Path, true, Encoding.GetEncoding(1251)));
        Trace.Listeners.Add(Listener);
        Trace.WriteLine("FIASDemo starting...");

        EFPApp.ConfigManager = new EFPRegistryConfigManager(@"HKEY_CURRENT_USER\Software\FreeLibSet\FIASDemo");
        using (RegistryCfg cfg = new RegistryCfg(@"HKEY_CURRENT_USER\Software\FreeLibSet\FIASDemo"))
        {
          FiasDB fiasDB;
          using (SelectDBForm selForm = new SelectDBForm())
          {
            selForm.Cfg = cfg;
            if (EFPApp.ShowDialog(selForm, false) != DialogResult.OK)
              return;

            //AbsPath Path = new AbsPath(FileTools.ApplicationBaseDir, "fias.db");
            //SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder();
            //csb.DataSource = Path.Path;
            //SQLiteDBx DB = new SQLiteDBx(csb);

            //FiasDBSettings settings = new FiasDBSettings();
            //settings.RegionCodes.Add("72");
            //settings.RegionCodes.Add("66");
            //settings.RegionCodes.Add("78");
            //settings.UseDates = true;

            DataRow row = selForm.efpGrid.CurrentDataRow;

            using (Splash spl = new Splash("Подключение к базе данных"))
            {
              string provider = DataTools.GetString(row, "ProviderName");
              string constr = DataTools.GetString(row, "ConnectionString");
              DBx DB = DBxManager.Managers[provider].CreateDBObject(constr);

              string xml = DataTools.GetString(row, "FiasDBSettings");
              FiasDBSettings settings = new FiasDBSettings();
              settings.AsXmlText = xml;

              fiasDB = new FiasDB(DB, settings);
              fiasDB.DB.TraceEnabled = true; // трассировка запросов
            }
          }


          MainForm frm = new MainForm(fiasDB, cfg);

          Application.Run(frm);
        }
        Trace.WriteLine("FIASDemo finished.");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }
  }
}