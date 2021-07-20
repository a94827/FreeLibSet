using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.Caching;
using AgeyevAV.IO;
using System.Threading;

namespace TestCache
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      try
      {
        Main2();
      }
      catch(Exception e)
      {
        DebugTools.ShowException(e, "������ ������� ���������");
      }
    }

    private static void Main2()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      EFPApp.InitApp();

      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = "������������ Cache";
      dlg.Items = new string[] { 
        "�������� ����", 
        "���� �������� ���������� GetItem()/Clear()",
        "���� �������� ���������� GetItem()/GetItem() (������)"};

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      switch (dlg.SelectedIndex)
      {
        case 0:
          int ThreadCount;
          if (!ParamForm.PerformSetup(out ThreadCount))
            return;

          // ���� �������� ��� �������� ����
          if (!Cache.Params.PersistDir.IsEmpty)
            FileTools.ClearDirAsPossible(Cache.Params.PersistDir);

          TestForm frm2 = new TestForm();
          frm2.ThreadCount = ThreadCount;

          Application.Run(frm2);
          break;

        case 1:
          DeadLockTest_GetAndClear.PerformTest();
          break;

        case 2:
          DeadLockTest_GetAndGet.PerformTest();
          break;
      }
    }
  }
}