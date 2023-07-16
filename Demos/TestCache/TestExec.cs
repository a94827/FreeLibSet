using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FreeLibSet.Caching;
using FreeLibSet.Core;
using FreeLibSet.Forms;

namespace TestCache
{
  /// <summary>
  /// Объект тестирования
  /// Создается по одному экземпляру для каждого потока
  /// </summary>
  public class TestExec : ICacheFactory<TestBuf1>, ICacheFactory<TestBuf2>, ICacheFactory<TestBuf3>
  {
    #region Конструктор

    public TestExec(int threadNo)
    {
      _ThreadNo = threadNo;
      _Thread = new Thread(new ThreadStart(this.ThreadExec));
      _Thread.Start();
    }

    #endregion

    #region Управление

    /// <summary>
    /// Поток выполнения теста
    /// </summary>
    public Thread Thread { get { return _Thread; } }
    private Thread _Thread;

    public bool StopSignal
    {
      get { lock (this) { return _StopSignal; } }
      set { lock (this) { _StopSignal = value; } }
    }
    private bool _StopSignal;

    public static ObjTypeSettings Settings1, Settings2, Settings3; // если null, то объект не используется

    #endregion

    #region Выводимая информация

    public int AccessCount
    {
      get
      {
        lock (this)
        {
          return _AccessCount;
        }
      }
    }
    private int _AccessCount;

    public int DelCount
    {
      get
      {
        lock (this)
        {
          return _DelCount;
        }
      }
    }
    private int _DelCount;

    /// <summary>
    /// Номер потока 1-число потоков
    /// </summary>
    private int _ThreadNo;

    public override string ToString()
    {
      //return "Thread=" + _Thread.ManagedThreadId.ToString() + ", AccessCount=" + AccessCount.ToString() + ", DelCount=" + DelCount.ToString();
      return _ThreadNo.ToString();
    }

    #endregion

    #region Для проверки

    private static Dictionary<string, Guid> ObjGuids1 = new Dictionary<string, Guid>();
    private static Dictionary<string, Guid> ObjGuids2 = new Dictionary<string, Guid>();
    private static Dictionary<string, Guid> ObjGuids3 = new Dictionary<string, Guid>();

    #endregion

    #region Выполняемый метод


    public void ThreadExec()
    {
      try
      {
        // В каждом потоке должен быть свой Random, чтобы обойтись без блокировки
        // Если в каждом потоке одновременно создать объекты Random без параметров, они будут все одинаковые
        Random rnd = new Random(((int)(DateTime.Now.Ticks & 0xFFFFL)) | Thread.ManagedThreadId);

        int cnt = 0;

        while (!StopSignal)
        {
          int Mode = rnd.Next(303);
          switch (Mode)
          {
            case 300:
              DoDelObj<TestBuf1>(Settings1, rnd);
              break;
            case 301:
              DoDelObj<TestBuf2>(Settings2, rnd);
              break;
            case 302:
              DoDelObj<TestBuf3>(Settings3, rnd);
              break;
            default:
              switch (Mode % 3)
              {
                case 0:
                  DoAccessObj<TestBuf1>(Settings1, rnd, this, ObjGuids1);
                  break;
                case 1:
                  DoAccessObj<TestBuf2>(Settings2, rnd, this, ObjGuids2);
                  break;
                case 2:
                  DoAccessObj<TestBuf3>(Settings3, rnd, this, ObjGuids3);
                  break;
              }
              break;
          }

          if (cnt > 1000)
          {
            // Делаем случайную паузу, чтобы рассинхронизировать потоки
            Thread.Sleep(rnd.Next(20));
            cnt = 0;
          }
        } // конец цикла
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка выполнения в потоке " + this.ToString());
      }
    }

    private void DoAccessObj<T>(ObjTypeSettings settings, Random rnd, ICacheFactory<T> factory, Dictionary<string, Guid> objGuids)
      where T : TestBufBase
    {
      if (settings == null)
        return;

      TestBufBase obj = Cache.GetItem<T>(GetKeys(settings, rnd), settings.Persistance, factory);
      if ((settings.Persistance & CachePersistance.PersistOnly) != 0)
        objGuids = null; // Если используется каталог, то нельзя проверять совпадение GUID'в

      obj.TestValid(objGuids);
      lock (this) { _AccessCount++; }
    }

    private void DoDelObj<T>(ObjTypeSettings settings, Random rnd)
      where T : TestBufBase
    {
      if (settings == null)
        return;
      if (settings.AllowDelete)
      {
        Cache.Clear<T>(GetKeys(settings, rnd));
        lock (this) { _DelCount++; }
      }
    }

    private string[] GetKeys(ObjTypeSettings settings, Random rnd)
    {
      string[] a = new string[settings.KeyCount];
      for (int i = 0; i < settings.KeyCount; i++)
        a[i] = rnd.Next(settings.ValueCount).ToString();
      return a;
    }

    #endregion

    #region ICacheFactory<TestBuf1> Members

    TestBuf1 ICacheFactory<TestBuf1>.CreateCacheItem(string[] keys)
    {
      TestBuf1 Obj = new TestBuf1();
      Obj.InitGuid(ObjGuids1, keys);
      return Obj;
    }

    #endregion

    #region ICacheFactory<TestBuf2> Members

    TestBuf2 ICacheFactory<TestBuf2>.CreateCacheItem(string[] Keys)
    {
      TestBuf2 Obj = new TestBuf2();
      Obj.InitGuid(ObjGuids2, Keys);
      return Obj;
    }

    #endregion

    #region ICacheFactory<TestBuf3> Members

    TestBuf3 ICacheFactory<TestBuf3>.CreateCacheItem(string[] Keys)
    {
      TestBuf3 Obj = new TestBuf3();
      Obj.InitGuid(ObjGuids3, Keys);
      return Obj;
    }

    #endregion
  }

  [Serializable]
  /// <summary>
  /// Базовый класс для тестирования буфера.
  /// </summary>
  public class TestBufBase
  {
    #region Конструктор

    public TestBufBase(int size)
    {
      Random rnd = new Random();

      _Data = new byte[size];
      rnd.NextBytes(_Data);

      _MD5 = DataTools.MD5Sum(_Data);
    }

    #endregion

    #region Свойства

    private byte[] _Data;

    private string _MD5;

    public void TestValid(Dictionary<string, Guid> objGuids)
    {
      string NewMD5 = DataTools.MD5Sum(_Data);
      if (NewMD5 != _MD5)
        throw new Exception("Объект поврежден. Неправильная контрольная сумма");

      if (objGuids != null)
      {
        lock (objGuids)
        {
          Guid SavedGuid;
          if (!objGuids.TryGetValue(TheKey, out SavedGuid))
            throw new BugException("Не было элемента с ключом " + TheKey);
          if (SavedGuid != this.Guid)
            throw new BugException("Неправильный Guid");
        }
      }
    }

    #endregion

    #region Проверка Guid

    public Guid Guid;

    public string TheKey;

    public void InitGuid(Dictionary<string, Guid> objGuids, string[] keys)
    {
      this.Guid = Guid.NewGuid();
      this.TheKey = String.Join("|", keys);
      lock (objGuids)
      {
        objGuids[this.TheKey] = this.Guid;
      }
    }

    #endregion
  }

  // Несколько различных классов

  /// <summary>
  /// Маленький элемент (10кБ)
  /// </summary>
  [Serializable]
  public class TestBuf1 : TestBufBase
  {
    public TestBuf1()
      : base(TestExec.Settings1.Size)
    {
    }
  }

  /// <summary>
  /// Средний элемент (100кБ)
  /// </summary>
  [Serializable]
  public class TestBuf2 : TestBufBase
  {
    public TestBuf2()
      : base(TestExec.Settings2.Size)
    {
    }
  }

  /// <summary>
  /// Большой элемент (1МБ)
  /// </summary>
  [Serializable]
  public class TestBuf3 : TestBufBase
  {
    public TestBuf3()
      : base(TestExec.Settings3.Size)
    {
    }
  }
}
