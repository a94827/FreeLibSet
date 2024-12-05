using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;

namespace LogWatcher
{
  class DirWatcher : SimpleDisposableObject
  {
    #region Конструктор и Dispose()

    public DirWatcher()
    {
      Update();
      _Timer = new Timer();
      _Timer.Interval = Properties.Settings.Default.UpdateInterval * 1000;
      _Timer.Tick += Timer_Tick;
      _Timer.Enabled = true;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    private Timer _Timer;

    private void Timer_Tick(object sender, EventArgs args)
    {
      Update();
    }

    #endregion

    #region Обновление

    public static readonly EFPConfigSectionInfo CfgInfo = new EFPConfigSectionInfo("PathDictionary", EFPConfigCategories.UserParams);

    private bool _InsideUpdate;

    public void Update()
    {
      if (_InsideUpdate)
        return;
      _InsideUpdate = true;
      try
      {
        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons["HourGlass"];
        DataTable tbl = CreateTable();
        Program.TheForm.SetData(tbl);

        UpdateTrayIcon(tbl);
      }
      catch (Exception e)
      {
        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons["Error"];
        EFPApp.ShowException(e, "Ошибка обновления списка");
      }
      finally
      {
        _InsideUpdate = false;
      }
    }

    private static void UpdateTrayIcon(DataTable tbl)
    {
      int cntError = 0;
      int cntWarning = 0;
      int cntInfo = 0;
      foreach (DataRow row in tbl.Rows)
      {
        int n = DataTools.GetInt(row, "NewCount");
        switch ((ErrorMessageKind)(DataTools.GetInt(row, "Kind")))
        {
          case ErrorMessageKind.Error: cntError += n; break;
          case ErrorMessageKind.Warning: cntWarning += n; break;
          case ErrorMessageKind.Info: cntInfo += n; break;
        }
      }
      if (cntError > 0)
        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons["Error"];
      else if (cntWarning > 0)
        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons["Warning"];
      else if (cntInfo > 0)
        EFPApp.TrayIcon.Icon = EFPApp.MainImages.Icons["Info"];
      else
        EFPApp.TrayIcon.Icon = WinFormsTools.AppIcon;
    }

    private static DataTable CreateTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Path", typeof(string));
      tbl.Columns.Add("LastTime", typeof(DateTime));
      tbl.Columns.Add("RegTime", typeof(DateTime));
      tbl.Columns.Add("Kind", typeof(int));
      tbl.Columns.Add("NewCount", typeof(int));
      tbl.Columns.Add("OldCount", typeof(int));
      DataTools.SetPrimaryKey(tbl, "Path");

      AddRows(tbl, Properties.Settings.Default.ErrorDirs, ErrorMessageKind.Error);
      AddRows(tbl, Properties.Settings.Default.WarningDirs, ErrorMessageKind.Warning);
      AddRows(tbl, Properties.Settings.Default.InfoDirs, ErrorMessageKind.Info);
      if (tbl.Rows.Count == 0)
      {
        StringCollection dummyColl = new StringCollection();
        dummyColl.Add(FreeLibSet.Logging.LogoutTools.LogBaseDirectory.ParentDir.Path);
        AddRows(tbl, dummyColl, ErrorMessageKind.Error);
      }

      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(CfgInfo, EFPConfigMode.Read, out cfg))
      {
        foreach (string name in cfg.GetChildNames())
        {
          CfgPart child = cfg.GetChild(name, false);
          string path = child.GetString("Path");
          DateTime? regTime = child.GetNullableDateTime("RegTime");
          DataRow row = tbl.Rows.Find(path);
          if (row != null)
            DataTools.SetNullableDateTime(row, "RegTime", regTime);
        }
      }

      foreach (DataRow row in tbl.Rows)
      {
        AbsPath dir = new AbsPath(DataTools.GetString(row, "Path"));
        DateTime regTime = DataTools.GetNullableDateTime(row, "RegTime") ?? DateTime.MinValue;
        regTime = RoundTime(regTime);

        if (System.IO.Directory.Exists(dir.Path))
        {
          string[] files = System.IO.Directory.GetFiles(dir.Path, "*.*", System.IO.SearchOption.AllDirectories);
          DateTime lastTime = DateTime.MinValue;
          int cntNew = 0;
          int cntOld = 0;
          foreach (string file in files)
          {
            DateTime tm = System.IO.File.GetLastWriteTime(file);
            tm = RoundTime(tm);
            lastTime = DataTools.Max(lastTime, tm);

            if (tm > regTime)
              cntNew++;
            else
              cntOld++;
          }
          if ((cntNew + cntOld) > 0)
          {
            row["LastTime"] = lastTime;
            DataTools.SetInt(row, "NewCount", cntNew);
            DataTools.SetInt(row, "OldCount", cntOld);
          }
        }
      }

      return tbl;
    }

    private static readonly long _RoundingTicks = TimeSpan.FromSeconds(2.0).Ticks;

    /// <summary>
    /// Округляем время с точностью до 2 секунд
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    private static DateTime RoundTime(DateTime tm)
    {
      long secs2 = tm.Ticks / _RoundingTicks;
      return new DateTime(secs2 * _RoundingTicks);
    }

    private static void AddRows(DataTable tbl, StringCollection dirs, ErrorMessageKind kind)
    {
      if (dirs == null)
        return;
      foreach (string dir in dirs)
      {
        if (String.IsNullOrEmpty(dir) || dir == "-" || dir == "?")
          continue; // dummy template in config file

        DataRow row = tbl.NewRow();
        row["Path"] = dir;
        row["Kind"] = (int)kind;
        tbl.Rows.Add(row);
      }
    }

    #endregion

    #region Запись

    public void WriteRegDates(DataTable tbl)
    {
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(CfgInfo, EFPConfigMode.Write, out cfg))
      {
        cfg.Clear();
        int cnt = 0;
        foreach (DataRow row in tbl.Rows)
        {
          string path = DataTools.GetString(row, "Path");
          DateTime? regTime = DataTools.GetNullableDateTime(row, "RegTime");
          if (regTime.HasValue)
          {
            cnt++;
            CfgPart child = cfg.GetChild("Item" + StdConvert.ToString(cnt), true);
            child.SetString("Path", path);
            child.SetDateTime("RegTime", regTime.Value);
          }
        }
      }

      UpdateTrayIcon(tbl);
    }

    #endregion
  }
}
