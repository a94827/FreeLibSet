using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.Caching;
using AgeyevAV;
using AgeyevAV.IO;
using AgeyevAV.Diagnostics;

namespace TestCache
{
  public partial class ParamForm : Form
  {
    #region Конструктор формы

    public ParamForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpCapacity = new EFPNumEditBox(efpForm, edCapacity);
      efpLowMemorySize = new EFPNumEditBox(efpForm, edLowMemorySize);
      efpLowMemorySize.Minimum = 1;
      efpLowMemorySize.Maximum = 1000;
      efpCriticalMemoryLoad = new EFPNumEditBox(efpForm, edCriticalMemoryLoad);
      efpCriticalMemoryLoad.Minimum = 0;
      efpCriticalMemoryLoad.Maximum = 100;

      efpCheckMemoryInterval = new EFPNumEditBox(efpForm, eCheckMemoryInterval);
      efpCheckMemoryInterval.Minimum = 1;
      efpCheckMemoryInterval.Maximum = 600;

      efpThreads = new EFPExtNumericUpDown(efpForm, edThreads);

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

    public EFPNumEditBox efpCapacity;

    public EFPNumEditBox efpLowMemorySize;

    public EFPNumEditBox efpCriticalMemoryLoad;

    public EFPNumEditBox efpCheckMemoryInterval;

    public EFPExtNumericUpDown efpThreads;

    public EFPDataGridView ghObjs;

    #endregion

    #region Статический метод

    public static bool PerformSetup(out int ThreadCount)
    {
      ThreadCount = 1;
      ParamForm Form = new ParamForm();
      Form.efpCapacity.IntValue = Cache.Params.Capacity;
      Form.efpLowMemorySize.IntValue = MemoryTools.LowMemorySizeMB;
      Form.efpCriticalMemoryLoad.IntValue = Cache.Params.CriticalMemoryLoad;
      Form.efpCheckMemoryInterval.IntValue = Cache.Params.CheckMemoryInterval / 1000;
      Form.efpThreads.IntValue = ThreadCount;

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

      Cache.Params.Capacity = Form.efpCapacity.IntValue;

      MemoryTools.LowMemorySizeMB = Form.efpLowMemorySize.IntValue;
      Cache.Params.CriticalMemoryLoad = Form.efpCriticalMemoryLoad.IntValue;
      Cache.Params.CheckMemoryInterval = Form.efpCheckMemoryInterval.IntValue * 1000;

      ThreadCount = Form.efpThreads.IntValue;

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
      Settings.AllowDelete = DataTools.GetBool(Row, "AllowDelete");

      Settings.Size = DataTools.GetInt(Row, "Size");
      Settings.KeyCount = DataTools.GetInt(Row, "KeyCount");

      Settings.ValueCount = DataTools.GetInt(Row, "ValueCount");


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