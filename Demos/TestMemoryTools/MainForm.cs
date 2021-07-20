using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.Diagnostics;
using AgeyevAV;
using AgeyevAV.IO;
using AgeyevAV.Logging;

namespace TestMemoryTools
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      InitializeComponent();

      efpForm = new EFPFormProvider(this);

      efpCount = new EFPExtNumericUpDown(efpForm, edCount);

      efpSize = new EFPNumEditBox(efpForm, edSize);

      btnAlloc.Image = EFPApp.MainImages.Images["Insert"];
      btnAlloc.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpAlloc = new EFPButton(efpForm, btnAlloc);
      efpAlloc.Click += new EventHandler(efpAlloc_Click);

      btnFree.Image = EFPApp.MainImages.Images["Delete"];
      btnFree.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpFree = new EFPButton(efpForm, btnFree);
      efpFree.Click +=new EventHandler(efpFree_Click);

      btnInfo.Image = EFPApp.MainImages.Images["Debug"];
      btnInfo.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpInfo = new EFPButton(efpForm, btnInfo);
      efpInfo.Click += new EventHandler(efpInfo_Click);

      if (MemoryTools.TotalPhysicalMemory == MemoryTools.UnknownMemorySize)
        lblTotalPhysicalMemory.Text = "Неизвестно";
      else
        lblTotalPhysicalMemory.Text = (MemoryTools.TotalPhysicalMemory / FileTools.MByte).ToString() + " MB";

      InitTotalSize();

      efpForm.UpdateByTime += new EventHandler(efpForm_UpdateByTime);
    }

    #endregion

    #region Поля

    EFPFormProvider efpForm;
    EFPExtNumericUpDown efpCount;
    EFPNumEditBox efpSize;

    #endregion

    #region Обработчики кнопок

    /// <summary>
    /// Список хранит object, для которых вызывается ToString()
    /// </summary>
    private class MemListItem
    {
      #region Конструктор

      public MemListItem(int SizeMB)
      {
        this.SizeMB = SizeMB;
        Buffer = new byte[SizeMB * FileTools.MByte];
        // Важно заполнить буфер значениями, иначе swapping может не выполняться
        rnd.NextBytes(Buffer);
      }

      private static Random rnd = new Random();

      #endregion

      #region Свойства

      public int SizeMB;

      private byte[] Buffer;

      public override string ToString()
      {
        return SizeMB.ToString() + " МБ";
      }

      #endregion
    }

    void efpAlloc_Click(object Sender, EventArgs Args)
    {
      if (!efpForm.ValidateForm())
        return;

      try
      {
        using (Splash spl = new Splash("Выделяется память"))
        {
          spl.PercentMax = efpCount.IntValue;
          for (int i = 0; i < efpCount.IntValue; i++)
          {
            lb1.Items.Add(new MemListItem(efpSize.IntValue));
            InitTotalSize();
            efpForm_UpdateByTime(null, null);
            spl.IncPercent();
          }
        }
      }
      finally
      {
        InitTotalSize();
        efpForm_UpdateByTime(null, null);
      }
    }

    void efpFree_Click(object Sender, EventArgs Args)
    {
      if (lb1.SelectedItems.Count==0)
      {
        EFPApp.ShowTempMessage("Блоки не выбраны");
        return;
      }
      object[]a=new object[lb1.SelectedItems.Count];
      lb1.SelectedItems.CopyTo(a,0);
      for (int i = 0; i < a.Length; i++)
        lb1.Items.Remove(a[i]);

      GC.Collect();

      InitTotalSize();
    }

    private void InitTotalSize()
    {
      object []a=new object[lb1.Items.Count];
      lb1.Items.CopyTo(a, 0);

      int TotalMB=0;
      for (int i = 0; i < a.Length; i++)
        TotalMB += ((MemListItem)(a[i])).SizeMB;

      lblTotalSize.Text = TotalMB.ToString() + " MB";
    }

    void efpInfo_Click(object Sender, EventArgs Args)
    {
      EFPApp.ShowTextView(LogoutTools.GetDebugInfo(), "Информация о программе");
    }

    #endregion

    #region Обновление информации по таймеру

    void efpForm_UpdateByTime(object Sender, EventArgs Args)
    {
      lblGCGetTotalMemory.Text = (GC.GetTotalMemory(false) / FileTools.MByte).ToString() + " MB";

      AvailableMemoryState ams = MemoryTools.AvailableMemoryState;
      lblMemoryState.Text = ams.ToString();
      switch (ams)
      {
        case AvailableMemoryState.Normal: lblMemoryState.BackColor = Color.Green; break;
        case AvailableMemoryState.Swapping: lblMemoryState.BackColor = Color.Yellow; break;
        case AvailableMemoryState.Low: lblMemoryState.BackColor = Color.Red; break;
      }

      int ml=MemoryTools.MemoryLoad;
      if (ml == MemoryTools.UnknownMemoryLoad)
        lblMemoryLoad.Text = "Неизвестно";
      else
        lblMemoryLoad.Text = ml.ToString() + "%";
    }

    #endregion
  }
}