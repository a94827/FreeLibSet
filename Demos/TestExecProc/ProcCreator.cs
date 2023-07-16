using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using FreeLibSet.Collections;

namespace TestExecProc
{
  /// <summary>
  /// Выделенный класс исключения для удобства отладки
  /// </summary>
  [Serializable]
  public class ProcTestException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public ProcTestException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected ProcTestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }


  /// <summary>
  /// Объект для создания процедур ExecProc.
  /// Можно использовать из основного домена приложения или из отдельного домена
  /// </summary>
  public class ProcCreator : MarshalByRefSponsoredObject
  {
    #region Конструктор

    public ProcCreator()
    {
      // Добавляем трассировку
      Trace.Listeners.Add(new ConsoleTraceListener());

      Trace.WriteLine("Включена трассировка для домена " + AppDomain.CurrentDomain.FriendlyName);

      base.EternalLife = true; // существует вечно

      DebugRefs = new WeakReferenceCollection<object>();
    }

    #endregion

    #region Создание процедур

    /// <summary>
    /// Этот метод используется для основного домена приложения, когда запускается непосредственно ExecProc
    /// </summary>
    /// <param name="ProcType"></param>
    /// <returns></returns>
    public ExecProc CreateExecProc(TestProcType ProcType, int syncTime)
    {
      ExecProc Proc;
      switch (ProcType)
      {
        case TestProcType.SimpleWait:
          Proc = new SimpleWaitProc(10);
          break;
        case TestProcType.Fast:
          Proc = new FastProc();
          break;
        case TestProcType.WaitAndCrash:
          Proc = new WaitAndCrashProc();
          break;
        case TestProcType.FastCrash:
          Proc = new FastCrashProc();
          break;
        default:
          throw new BugException();
      }

      Proc.SetContext(NamedValues.Empty);
      Proc.SyncTime = syncTime;

      DebugRefs.Add(Proc);
      //DebugRefs.Add(Proc.Proxy);

      return Proc;
    }

    /// <summary>
    /// Этот метод используется для отдельного домена, когда используется RemoteExecProc.
    /// Возвращает прокси, который реализует marshal-by-reference
    /// </summary>
    /// <param name="procType"></param>
    /// <returns></returns>
    public ExecProcProxy CreateExecProcProxy(TestProcType procType, int syncTime)
    {
      ExecProc Proc = CreateExecProc(procType, syncTime);
      return Proc.CreateProxy();
    }

    public DistributedCallData StartDistributed(TestProcType procType, int syncTime)
    {
      ExecProc proc = CreateExecProc(procType, syncTime);
      return proc.StartDistributedCall(NamedValues.Empty);
    }

    #endregion

    #region Объекты ExecProc

    private abstract class ExecProcExt : ExecProc
    {
      protected override void OnBeforeExecute(NamedValues Args)
      {
        base.OnBeforeExecute(Args);
        Trace.WriteLine(GetPrefix() + "OnBeforeExecute()");
        // base.AutoDispose = true;
      }

      protected override void OnAfterExecute(NamedValues Args, NamedValues Results, Exception Exception)
      {
        base.OnAfterExecute(Args, Results, Exception);
        Trace.WriteLine(GetPrefix() + "OnAfterExecute()");
      }

      protected override void Dispose(bool Disposing)
      {
        Trace.WriteLine(GetPrefix() + "Dispose(" + Disposing.ToString() + ")");
        base.Dispose(Disposing);
      }

      private string GetPrefix()
      {
        return /*DateTime.Now.ToString("G") + */"ExecProc " + Guid.ToString() + ". ";
      }
    }

    private class SimpleWaitProc : ExecProcExt
    {
      #region Конструктор

      public SimpleWaitProc(int Time)
      {
        this.Time = Time;
        this.DisplayName = "Тест выполнения в течение " + Time.ToString() + " секунд";
      }

      #endregion

      #region Поля

      /// <summary>
      /// Время выполнения в секундах
      /// </summary>
      public int Time;

      #endregion

      #region Выполнение

      protected override NamedValues OnExecute(NamedValues Args)
      {
        ISplash spl = BeginSplash("Тест времени");
        spl.PercentMax = Time;
        spl.AllowCancel = true;
        for (int i = 0; i < Time; i++)
        {
          System.Threading.Thread.Sleep(1000);
          Trace.Write(" .. " + (i + 1).ToString());
          spl.IncPercent();
        }

        EndSplash();
        return NamedValues.Empty;
      }

      #endregion
    }

    private class FastProc : ExecProcExt
    {
      #region Конструктор

      public FastProc()
      {
        this.DisplayName = "Быстрое выполнение 0.1 секунды";
      }

      #endregion

      #region Поля

      #endregion

      #region Выполнение

      protected override NamedValues OnExecute(NamedValues Args)
      {
        System.Threading.Thread.Sleep(100);
        return NamedValues.Empty;
      }

      #endregion
    }


    private class WaitAndCrashProc : ExecProcExt
    {
      #region Конструктор

      public WaitAndCrashProc()
      {
        this.DisplayName = "Пять секунд и ошибка";
      }

      #endregion

      #region Поля

      #endregion

      #region Выполнение

      [DebuggerStepThrough]
      protected override NamedValues OnExecute(NamedValues Args)
      {
        for (int i = 0; i < 5; i++)
        {
          System.Threading.Thread.Sleep(1000);
          Trace.Write(" .. " + (i + 1).ToString());
        }
        throw new ProcTestException("Тестовое исключение из процедуры " + Guid.ToString());
      }

      #endregion
    }

    private class FastCrashProc : ExecProcExt
    {
      #region Конструктор

      public FastCrashProc()
      {
        this.DisplayName = "Ошибка сразу";
      }

      #endregion

      #region Поля

      #endregion

      #region Выполнение

      [DebuggerStepThrough]
      protected override NamedValues OnExecute(NamedValues Args)
      {
        throw new ProcTestException("Тестовое исключение из процедуры " + Guid.ToString());
      }

      #endregion
    }

    #endregion

    #region Отладочная информация

    /// <summary>
    /// Отлаживаем реальное удаление объектов
    /// </summary>
    public WeakReferenceCollection<object> DebugRefs;

    public void TraceInfo()
    {
      Trace.WriteLine("=== Домен " + AppDomain.CurrentDomain.FriendlyName + " ===");
      // Trace.WriteLine("ExecProc      .AllProcCount      =" + ExecProc.AllProcCount.ToString());
      // Trace.WriteLine("ExecProc      .ExecutingProcCount=" + ExecProc.ExecutingProcCount.ToString());
      // Trace.WriteLine("RemoteExecProc.AllProcCount      =" + RemoteExecProc.AllProcCount.ToString());
      // Trace.WriteLine("RemoteExecProc.ExecutingProcCount=" + RemoteExecProc.ExecutingProcCount.ToString());
      // //Trace.WriteLine("======");
      object[] a = DebugRefs.ToArray();
      Trace.WriteLine("References (" + a.Length.ToString() + "):");
      for (int i = 0; i < a.Length; i++)
      {
        string s1 = a[i].GetType().ToString();
        string s2 = a[i].ToString();
        string s;
        if (s2 == s1)
          // ToString() ничего полезного не вернул
          s = s1;
        else
          s = "[" + s1 + "] " + s2;
        if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(a[i]))
          s = "[ Transparent Proxy ] "+s;

        Trace.WriteLine("  " + s);
      }
    }

    #endregion
  }
}
