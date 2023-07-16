using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FreeLibSet.Caching;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Diagnostics;

namespace TestCache
{
  internal class DeadLockTest_GetAndClear : ICacheFactory<DeadLockTest_GetAndClear.DummyData>
  {
    #region Поля

    private string Key1;

    private string Key2;

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
        DummyData data = Cache.GetItem<DummyData>(new string[] { Key1 }, CachePersistance.MemoryOnly, this);
        EFPApp.MessageBox("Данные для ключа " + Key1 + " успешно получены");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при получении данных для ключа " + Key1);
      }
      Completed = true;
    }

    #endregion

    #region ICacheFactory<DummyData> Members

    DeadLockTest_GetAndClear.DummyData ICacheFactory<DeadLockTest_GetAndClear.DummyData>.CreateCacheItem(string[] keys)
    {
      Thread.Sleep(2000);
      Cache.Clear<DummyData>(new string[] { Key2 });
      Thread.Sleep(2000);
      return new DummyData();
    }

    #endregion

    #region Статический метод запуска

    internal static void PerformTest()
    {
      DeadLockTest_GetAndClear obj1 = new DeadLockTest_GetAndClear();
      obj1.Key1 = "1";
      obj1.Key2 = "2";
      Thread trd1 = new Thread(obj1.Proc);
      trd1.Name = "Test1 thread";
      trd1.Start();

      DeadLockTest_GetAndClear obj2 = new DeadLockTest_GetAndClear();
      obj2.Key1 = "2";
      obj2.Key2 = "1";
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

      DebugTools.ShowDebugInfo("Тест успешно выполнен");
    }

    #endregion
  }
}
