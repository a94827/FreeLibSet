using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using System.Threading;
using AgeyevAV.Caching;
using AgeyevAV;
using AgeyevAV.IO;
using AgeyevAV.Diagnostics;
using AgeyevAV.Logging;

namespace TestCache
{
  public partial class TestForm : Form
  {
    #region Конструктор формы

    public TestForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpThreads = new EFPDataGridView(efpForm, grThreads);
      efpThreads.Columns.AddInt("Id", false, "№ потока", 3);
      efpThreads.Columns.AddInt("AccessCount", false, "Число обращений", 8);
      efpThreads.Columns.AddInt("DelCount", false, "Число удалений", 8);
      efpThreads.Columns.AddText("ThreadState", false, "Thread.State", 20, 5);
      efpThreads.ReadOnly = false;
      efpThreads.CanView = false;
      efpThreads.Control.ReadOnly = true;
      efpThreads.DisableOrdering();

      efpStat = new EFPDataGridView(efpForm, grStat);
      efpStat.Columns.AddText("ObjType", false, "Object type", 15, 10);
      efpStat.Columns.AddFixedPoint("PrcMem", false, "% из памяти", 5, 1, "Percent");
      efpStat.Columns.AddFixedPoint("PrcFile", false, "% из файла", 5, 1, "Percent");
      efpStat.Columns.AddFixedPoint("PrcCreate", false, "% создание", 5, 1, "Percent");
      MinMaxInt mmi = DataTools.GetEnumRange(typeof(CacheStatParam));
      for (int i = 0; i <= mmi.MaxValue; i++)
      {
        CacheStatParam sp = ((CacheStatParam)i);
        string Title = sp.ToString();
        efpStat.Columns.AddInt(Title, false, Title, 8);
        efpStat.Columns.LastAdded.Tag = sp;
      }
      efpStat.Columns.AddText("Persistance", false, "Persistance", 15, 5);
      efpStat.Columns.AddBool("AllowDelete", false, "AllowDelete");
      efpStat.Columns.AddInt("MaxCount", false, "Вариантов значений", 7);

      efpStat.ReadOnly = false;
      efpStat.CanView = false;
      efpStat.Control.ReadOnly = true;
      efpStat.DisableOrdering();
      efpStat.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(efpStat_GetRowAttributes);
      //efpStat.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpStat_GetCellAttributes);

      EFPTextBox efpTempDir = new EFPTextBox(efpForm, edTempDir);
      EFPWindowsExplorerButton efpWE = new EFPWindowsExplorerButton(efpTempDir, btnTempDir);

      EFPTextBox efpPersistDir = new EFPTextBox(efpForm, edPersistDir);
      efpWE = new EFPWindowsExplorerButton(efpPersistDir, btnPersistDir);

      btnClear.Image = EFPApp.MainImages.Images["No"];
      btnClear.ImageAlign = ContentAlignment.BottomLeft;
      EFPButton efpClear = new EFPButton(efpForm, btnClear);
      efpClear.Click += new EventHandler(efpClear_Click);

      btnCollect.Image = EFPApp.MainImages.Images["Execute"];
      btnCollect.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpCollect = new EFPButton(efpForm, btnCollect);
      efpCollect.Click += new EventHandler(efpCollect_Click);

      btnInfo.Image = EFPApp.MainImages.Images["Debug"];
      btnInfo.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpInfo = new EFPButton(efpForm, btnInfo);
      efpInfo.Click += new EventHandler(efpInfo_Click);

      base.Text += " - " + EnvironmentTools.NetVersionText;
    }

    void efpCollect_Click(object sender, EventArgs args)
    {
      GC.Collect();
    }

    void efpStat_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      if (args.RowIndex == grStat.RowCount - 1)
        args.ColorType = EFPDataGridViewColorType.TotalRow;
    }

    //void efpStat_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    //{
    //  if (args.RowIndex < grStat.RowCount - 1)
    //  {
    //    EFPDataGridViewColumn col = efpStat.Columns[args.ColumnIndex];
    //    if (col.Tag is CacheStatParam)
    //    {
    //      CacheStatParam sp = (CacheStatParam)(col.Tag);
    //      if (CacheStat.IsTotalOnly(sp))
    //      {
    //        args.Value = "x";
    //        args.FormattingApplied = true;
    //        args.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    //      }
    //    }
    //  }
    //}

    void efpInfo_Click(object sender, EventArgs args)
    {
      DebugTools.ShowDebugInfo("Отладочная информация");
    }

    void efpClear_Click(object sender, EventArgs args)
    {
      Cache.Clear();
    }

    #endregion

    #region Поля

    private EFPDataGridView efpThreads, efpStat;

    /// <summary>
    /// Количество потоков тестирования
    /// </summary>
    public int ThreadCount;

    #endregion

    #region Запуск теста

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      StartTest();

      efpThreads.Control.RowCount = Tests.Length;
      TheTimer.Tick += new EventHandler(TheTimer_Tick);
      TheTimer.Enabled = true;
    }

    private TestExec[] Tests;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
      StopTest();

      base.OnFormClosing(e);
    }

    private void StartTest()
    {
      Tests = new TestExec[ThreadCount];
      for (int i = 0; i < ThreadCount; i++)
      {
        Tests[i] = new TestExec(i+1);
      }
    }

    private void StopTest()
    {
      if (Tests != null)
      {
        for (int i = 0; i < Tests.Length; i++)
          Tests[i].StopSignal = true;
      }
    }

    #endregion

    #region Вывод статистики

    bool InsideTimerTick;

    void TheTimer_Tick(object sender, EventArgs args)
    {
      if (InsideTimerTick)
        return;

      InsideTimerTick = true;
      try
      {
        UpdateStat();
      }
      catch
      {
      }
      InsideTimerTick = false;
    }

    private void UpdateStat()
    {
      edTempDir.Text = Cache.Params.TempDir.SlashedPath;
      edPersistDir.Text = Cache.Params.PersistDir.SlashedPath;

      for (int i = 0; i < Tests.Length; i++)
      {
        grThreads[0, i].Value = i + 1;
        grThreads[1, i].Value = Tests[i].AccessCount;
        grThreads[2, i].Value = Tests[i].DelCount;
        grThreads[3, i].Value = Tests[i].Thread.ThreadState.ToString();
      }

      MinMaxInt mmi = DataTools.GetEnumRange(typeof(CacheStatParam));
      Type[] ObjTypes;
      CacheStat[] Stats;
      Cache.GetStat(out ObjTypes, out Stats);
      CacheStat TotalStat = Cache.GetStat();
      grStat.RowCount = Stats.Length + 1;
      for (int i = 0; i < Stats.Length; i++)
      {
        grStat[0, i].Value = ObjTypes[i].ToString();
        FillStatRow(i, Stats[i], mmi);

        string s = ObjTypes[i].ToString();
        char Suffix = s[s.Length - 1];
        ObjTypeSettings Settings;
        switch (Suffix)
        {
          case '1': Settings = TestExec.Settings1; break;
          case '2': Settings = TestExec.Settings2; break;
          case '3': Settings = TestExec.Settings3; break;
          default: throw new BugException();
        }

        if (Settings != null)
        {
          DataGridViewRow GridRow = grStat.Rows[i];
          GridRow.Cells["Persistance"].Value = Settings.Persistance.ToString();
          GridRow.Cells["AllowDelete"].Value = Settings.AllowDelete;
          GridRow.Cells["MaxCount"].Value = Settings.KeyCount * Settings.ValueCount;
        }
      }

      grStat[0, Stats.Length].Value = "Все объекты";
      FillStatRow(Stats.Length, TotalStat, mmi);

      string[] a = new string[GC.MaxGeneration + 1];
      int MaxCC = 0;
      for (int i = 0; i <= GC.MaxGeneration; i++)
      {
        int ThisCC = GC.CollectionCount(i);
        a[i] = "Gen." + i.ToString() + ": " + ThisCC.ToString();
        MaxCC = Math.Max(MaxCC, ThisCC);
      }

      TimeSpan ts = DateTime.Now - EFPApp.AppStartTime;
      string s2 = String.Empty;
      if (ts.TotalSeconds > 1.0)
        s2 = " (" + ((double)MaxCC / ts.TotalSeconds).ToString("0.0") + " раз в секунду)";

      lblCollectInfo.Text = String.Join(", ", a) + s2;

      lblGCTotalMemory.Text = (GC.GetTotalMemory(false) / FileTools.MByte).ToString() + " MB";

      AvailableMemoryState ams = MemoryTools.AvailableMemoryState;
      lblMemoryState.Text = ams.ToString();
      switch (ams)
      {
        case AvailableMemoryState.Normal: lblMemoryState.BackColor = Color.Green; break;
        case AvailableMemoryState.Swapping: lblMemoryState.BackColor = Color.Yellow; break;
        case AvailableMemoryState.Low: lblMemoryState.BackColor = Color.Red; break;
      }

      int ml = MemoryTools.MemoryLoad;
      if (ml == MemoryTools.UnknownMemoryLoad)
        lblMemoryLoad.Text = "Неизвестно";
      else
        lblMemoryLoad.Text = ml.ToString() + "%";
    }

    private void FillStatRow(int Row, CacheStat Stat, MinMaxInt mmi)
    {
      long N = Stat[CacheStatParam.AccessCount];
      grStat[1, Row].Value = GetPercentValue(Stat[CacheStatParam.FromMemCount], N);
      grStat[2, Row].Value = GetPercentValue(Stat[CacheStatParam.LoadFileCount], N);
      grStat[3, Row].Value = GetPercentValue(Stat[CacheStatParam.CreateCount], N);

      for (int j = 0; j <= mmi.MaxValue; j++)
      {
        if (Stat[(CacheStatParam)j] == 0L)
          grStat[j + 4, Row].Value = null;
        else
          grStat[j + 4, Row].Value = Stat[(CacheStatParam)j];
      }
    }

    private object GetPercentValue(long X, long N)
    {
      if (X == 0L || N == 0L)
        return null;

      return (double)X * 100.0 / (double)N;
    }

    #endregion
  }
}
