using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Caching;
using FreeLibSet.Core;
using FreeLibSet.Diagnostics;
using FreeLibSet.Forms;
using FreeLibSet.IO;

namespace TestCache
{
  public partial class ParamForm : Form
  {
    #region Конструктор формы

    public ParamForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpCapacity = new EFPIntEditBox(efpForm, edCapacity);
      efpLowMemorySize = new EFPIntEditBox(efpForm, edLowMemorySize);
      efpLowMemorySize.Minimum = 1;
      efpLowMemorySize.Maximum = 1000;
      efpCriticalMemoryLoad = new EFPIntEditBox(efpForm, edCriticalMemoryLoad);
      efpCriticalMemoryLoad.Minimum = 0;
      efpCriticalMemoryLoad.Maximum = 100;

      efpCheckMemoryInterval = new EFPIntEditBox(efpForm, eCheckMemoryInterval);
      efpCheckMemoryInterval.Minimum = 1;
      efpCheckMemoryInterval.Maximum = 600;

      efpThreads = new EFPIntEditBox(efpForm, edThreads);

      ghObjs = new EFPDataGridView(efpForm, grObjs);
      ghObjs.ReadOnly = true;
      ghObjs.Control.ReadOnly = false;
      ghObjs.Control.Columns[0].ReadOnly = true;
      colPersistance.Items.Add("[ Нет ]");
      CachePersistance[] a = (CachePersistance[])(Enum.GetValues(typeof(CachePersistance)));
      for (int i = 0; i < a.Length; i++)
        colPersistance.Items.Add(GetPersistanceText(a[i]));

      base.Text += " - " + EnvironmentTools.NetVersionText;
    }

    private static string GetPersistanceText(CachePersistance p)
    {
      return ((int)p).ToString() + " " + p.ToString();
    }

    #endregion

    #region Поля

    public EFPIntEditBox efpCapacity;

    public EFPIntEditBox efpLowMemorySize;

    public EFPIntEditBox efpCriticalMemoryLoad;

    public EFPIntEditBox efpCheckMemoryInterval;

    public EFPIntEditBox efpThreads;

    public EFPDataGridView ghObjs;

    #endregion

    #region Статический метод

    public static bool PerformSetup(out int ThreadCount)
    {
      ThreadCount = 1;
      ParamForm Form = new ParamForm();
      Form.efpCapacity.Value = Cache.Params.Capacity;
      Form.efpLowMemorySize.Value = MemoryTools.LowMemorySizeMB;
      Form.efpCriticalMemoryLoad.Value = Cache.Params.CriticalMemoryLoad;
      Form.efpCheckMemoryInterval.Value = Cache.Params.CheckMemoryInterval / 1000;
      Form.efpThreads.Value = ThreadCount;

      DataTable TableObjs = new DataTable();
      TableObjs.Columns.Add("ObjType", typeof(string));
      TableObjs.Columns.Add("Persistance", typeof(string));
      TableObjs.Columns.Add("AllowDelete", typeof(bool));
      TableObjs.Columns.Add("Size", typeof(int));
      TableObjs.Columns.Add("KeyCount", typeof(int));
      TableObjs.Columns.Add("ValueCount", typeof(int));
      TableObjs.Rows.Add("BufItem1", GetPersistanceText(CachePersistance.MemoryOnly), false, 10000, 3, 10);
      TableObjs.Rows.Add("BufItem2", GetPersistanceText(CachePersistance.MemoryAndTempDir), false, 100000, 2, 10);
      TableObjs.Rows.Add("BufItem3", GetPersistanceText(CachePersistance.MemoryAndPersist), false, 1000000, 1, 10);
      Form.ghObjs.Control.DataSource = TableObjs;

      if (EFPApp.ShowDialog(Form, true) != DialogResult.OK)
        return false;

      Cache.Params.Capacity = Form.efpCapacity.Value;

      MemoryTools.LowMemorySizeMB = Form.efpLowMemorySize.Value;
      Cache.Params.CriticalMemoryLoad = Form.efpCriticalMemoryLoad.Value;
      Cache.Params.CheckMemoryInterval = Form.efpCheckMemoryInterval.Value * 1000;

      ThreadCount = Form.efpThreads.Value;

      Cache.Params.PersistDir = AbsPath.Empty;
      TestExec.Settings1 = GetSetting(TableObjs.Rows[0]);
      TestExec.Settings2 = GetSetting(TableObjs.Rows[1]);
      TestExec.Settings3 = GetSetting(TableObjs.Rows[2]);

      return true;
    }

    private static ObjTypeSettings GetSetting(DataRow Row)
    {
      string s = DataTools.GetString(Row, "Persistance");
      if (s.StartsWith("["))
        return null;

      int p = s.IndexOf(' ');
      s = s.Substring(0, p);

      ObjTypeSettings Settings = new ObjTypeSettings();
      Settings.Persistance = (CachePersistance)(int.Parse(s));
      Settings.AllowDelete = DataTools.GetBoolean(Row, "AllowDelete");

      Settings.Size = DataTools.GetInt32(Row, "Size");
      Settings.KeyCount = DataTools.GetInt32(Row, "KeyCount");

      Settings.ValueCount = DataTools.GetInt32(Row, "ValueCount");


      if (Cache.Params.PersistDir.IsEmpty)
      {
        switch (Settings.Persistance)
        {
          case CachePersistance.MemoryAndPersist:
          case CachePersistance.PersistOnly:
            Cache.Params.PersistDir = new AbsPath(FileTools.ApplicationBaseDir, "PersistCache");
            break;
        }
      }

      return Settings;
    }

    #endregion
  }

  /// <summary>
  /// Настройки для одного типа объектов
  /// </summary>
  public class ObjTypeSettings
  {
    #region Поля

    /// <summary>
    /// Размер блоков в байтах
    /// </summary>
    public int Size;

    /// <summary>
    /// Длина массива ключей
    /// </summary>
    public int KeyCount;

    /// <summary>
    /// Количество значений (для каждой размерности ключа)
    /// </summary>
    public int ValueCount;

    /// <summary>
    /// Режим сохранения
    /// </summary>
    public CachePersistance Persistance;

    /// <summary>
    /// Тестировать удаление объектов из кэша
    /// </summary>
    public bool AllowDelete;

    #endregion
  }
}
