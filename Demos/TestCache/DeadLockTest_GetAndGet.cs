using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FreeLibSet.Caching;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Diagnostics;

namespace TestCache
{
  /// <summary>
  /// Этот тест должен, по идее вызывать исключение, т.к. он выполняет неразрешимую 
  /// </summary>
  internal class DeadLockTest_GetAndGet : ICacheFactory<DeadLockTest_GetAndGet.DummyData>
  {
    #region Поля

    private bool UseDelay;

    private string Key1;

    private string Key2;

    int lockTimeout = System.Threading.Timeout.Infinite;

    internal volatile bool Completed;

    #endregion

    [Serializable]
    private class DummyData
    {
    }

    #region Процедура, выполняемая в потоке

    private void Proc(object dummy)
    {
      try
      {
        DummyData data = Cache.GetItem<DummyData>(new string[] { Key1 }, CachePersistance.MemoryOnly, this, lockTimeout);
        EFPApp.MessageBox("Данные для ключа " + Key1 + " успешно получено");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при получении данных для ключа " + Key1);
      }
      Completed = true;
    }

    #endregion

    #region ICacheFactory<DummyData> Members

    DeadLockTest_GetAndGet.DummyData ICacheFactory<DeadLockTest_GetAndGet.DummyData>.CreateCacheItem(string[] keys)
    {
      if (UseDelay)
        Thread.Sleep(2000);

      DeadLockTest_GetAndGet obj2 = new DeadLockTest_GetAndGet();
      obj2.Key1 = Key2;
      obj2.Key2 = Key1;
      DummyData data2 = Cache.GetItem<DummyData>(new string[] { Key2 }, CachePersistance.MemoryOnly, obj2, lockTimeout);
      if (UseDelay)
        Thread.Sleep(2000);
      return new DummyData();
    }

    #endregion

    #region Статический метод запуска

    internal static void PerformTest()
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = "Тест рекурсивного вызова GetItem()";
      dlg.Items = new string[] { 
        "Один объект (ReenteranceException)", 
        "Два объекта без тайм-аута (зависание)",
        "Два объекта с тайм-аутом 30 секунд (LockTimeoutException)" };
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;

      switch (dlg.SelectedIndex)
      {
        case 0:
          DeadLockTest_GetAndGet obj = new DeadLockTest_GetAndGet();
          obj.Key1 = "1";
          obj.Key2 = "2";
          obj.UseDelay = false;
          Thread trd = new Thread(obj.Proc);
          trd.Name = "Test thread";
          trd.Start();

          using (Splash spl = new Splash("Выполняется тест"))
          {
            while (!obj.Completed)
            {
              spl.Sleep(200);
            }
          }

          break;

        case 1:
        case 2:
          DeadLockTest_GetAndGet obj1 = new DeadLockTest_GetAndGet();
          obj1.Key1 = "1";
          obj1.Key2 = "2";
          obj1.UseDelay = true;
          if (dlg.SelectedIndex == 2)
            obj1.lockTimeout = 30000;
          Thread trd1 = new Thread(obj1.Proc);
          trd1.Name = "Test1 thread";
          trd1.Start();

          DeadLockTest_GetAndGet obj2 = new DeadLockTest_GetAndGet();
          obj2.Key1 = "2";
          obj2.Key2 = "1";
          obj2.UseDelay = true;
          if (dlg.SelectedIndex == 2)
            obj2.lockTimeout = 30000;
          Thread trd2 = new Thread(obj2.Proc);
          trd2.Name = "Test2 thread";
          trd2.Start();

          using (Splash spl = new Splash("Выполняется тест"))
          {
            while (!(obj1.Completed && obj2.Completed))
            {
              spl.Sleep(200);
            }
          }
          break;
      }
      DebugTools.ShowDebugInfo("Тест успешно выполнен");
    }

    #endregion
  }
}
