using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Remoting;
using AgeyevAV;
using AgeyevAV.ExtForms;

namespace TestExecProc
{
  public enum TestProcType 
  {
    SimpleWait,
    Fast,
    WaitAndCrash,
    FastCrash,
  }

  public enum TestStartMode 
  { 
    ExecProcCallListExecuteSync,
    ExecProcCallListExecuteAsync,
    ExecProcCallListExecuteAsyncAndWait,
    WaitHandler,
    AsyncCallback,
    EndExecute,
    NoResults,
    DistributedProcCallItemAsync,
    DistributedProcCallItemAsyncAndWait,
    RemoteDistributedProc,
  }

  public enum TestFromThread
  { 
    MainThread,
    WorkThreads,
  }

  public enum TestExecutionPlace
  { 
    MainModule,
    AnotherAppDomain
  }

  /// <summary>
  /// Тестирование 
  /// </summary>
  public class Test
  {
    #region Параметры

    public TestProcType ProcType;

    public TestStartMode StartMode;

    public TestFromThread FromThread;

    public TestExecutionPlace ExecutionPlace;

    public int Copies;

    public int SyncTime;

    #endregion

    #region Генераторы процедур

    /// <summary>
    /// Создатель процедур в основном домене приложения
    /// </summary>
    public ProcCreator LocalProcCreator;

    /// <summary>
    /// Создатель удаленных процедур в отдельном домене
    /// </summary>
    public ProcCreator RemoteProcCreator;

    #endregion

    #region Выполнение теста

    #region Run

    public void Run()
    {
      for (int i = 0; i < Copies; i++)
      {
        switch (FromThread)
        { 
          case TestFromThread.MainThread:
            DoRun();
            break;
          case TestFromThread.WorkThreads:
            System.Threading.Thread trd = new System.Threading.Thread(DoRun);
            trd.Start();
            break;
          default:
            throw new BugException();
        }
      }
    }

    private void DoRun()
    {
      try
      {
        switch (StartMode)
        { 
          case TestStartMode.DistributedProcCallItemAsync:
          case TestStartMode.DistributedProcCallItemAsyncAndWait:
          case TestStartMode.RemoteDistributedProc:
            DoRunDistributed();
            break;
          default:
            DoRunExecProc();
            break;
        }

      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска теста");
      }
    }

    private void DoRunExecProc()
    {
      IExecProc Proc;
      switch (ExecutionPlace)
      {
        case TestExecutionPlace.MainModule:
          Proc = LocalProcCreator.CreateExecProc(ProcType, SyncTime);
          break;
        case TestExecutionPlace.AnotherAppDomain:
          Proc = new RemoteExecProc(RemoteProcCreator.CreateExecProcProxy(ProcType, SyncTime));
          LocalProcCreator.DebugRefs.Add(Proc);
          break;
        default:
          throw new BugException();
      }

      ExecProcCallItem Item;
      switch (StartMode)
      {
        case TestStartMode.ExecProcCallListExecuteSync:
          Item = new ExecProcCallItem(Proc);
          Item.Finished += ProcFinished;
          LocalProcCreator.DebugRefs.Add(Item);
          EFPApp.ExecProcList.ExecuteSync(Item, NamedValues.Empty);
          break;
        case TestStartMode.ExecProcCallListExecuteAsync:
          Item = new ExecProcCallItem(Proc);
          Item.Finished += ProcFinished;
          LocalProcCreator.DebugRefs.Add(Item);
          EFPApp.ExecProcList.ExecuteAsync(Item, NamedValues.Empty);
          break;
        case TestStartMode.ExecProcCallListExecuteAsyncAndWait:
          Item = new ExecProcCallItem(Proc);
          Item.Finished += ProcFinished;
          LocalProcCreator.DebugRefs.Add(Item);
          EFPApp.ExecProcList.ExecuteAsyncAndWait(Item, NamedValues.Empty);
          break;
        case TestStartMode.WaitHandler:
          StartWithWaitHandler(Proc);
          break;
        case TestStartMode.AsyncCallback:
          StartWithAsyncCallback(Proc);
          break;
        case TestStartMode.EndExecute:
          StartWithEndExecute(Proc);
          break;
        case TestStartMode.NoResults:
          Proc.ExecuteNoResults(NamedValues.Empty);
          break;
        default:
          throw new BugException();
      }
    }

    private void DoRunDistributed()
    {
      DistributedCallData StartData;
      switch (ExecutionPlace)
      {
        case TestExecutionPlace.MainModule:
          StartData = LocalProcCreator.StartDistributed(ProcType, SyncTime);
          break;
        case TestExecutionPlace.AnotherAppDomain:
          StartData = RemoteProcCreator.StartDistributed(ProcType, SyncTime);
          break;
        default:
          throw new BugException();
      }

      DistributedProcCallItem Item;
      switch (StartMode)
      {
        case TestStartMode.DistributedProcCallItemAsync:
          Item = new DistributedProcCallItem(StartData);
          Item.Finished += ProcFinished;
          LocalProcCreator.DebugRefs.Add(Item);
          EFPApp.ExecProcList.ExecuteAsync(Item);
          break;
        case TestStartMode.DistributedProcCallItemAsyncAndWait:
          Item = new DistributedProcCallItem(StartData);
          Item.Finished += ProcFinished;
          LocalProcCreator.DebugRefs.Add(Item);
          EFPApp.ExecProcList.ExecuteAsyncAndWait(Item);
          break;
        case TestStartMode.RemoteDistributedProc:
          RemoteDistributedProc Proc = new RemoteDistributedProc(StartData);
          StartWithWaitHandler(Proc);
          break;
        default:
          throw new BugException();
      }
    }

    #endregion

    #region Обработка завершения

    public static void ProcFinished(object sender, ExecProcCallEventArgs args)
    {
      string prefix = GetPrefix(args.Item.ExecProc);
      Console.WriteLine(prefix + "Успешное завершение (ProcFinished)");
    }


    public static void ProcFinished(object sender, DistributedProcCallEventArgs args)
    {
      string prefix= GetPrefix(args.Item.DistributedProc);
      Console.WriteLine(prefix + "Успешное завершение (ProcFinished)");
    }

    private static string GetPrefix(IExecProc Proc)
    {
      return Proc.DisplayName + ", GUID=" + Proc.Guid.ToString() + " ";
    }

    private static string GetPrefix(RemoteDistributedProc Proc)
    {
      return Proc.StartData.DisplayName + ", GUID=" + Proc.StartData.Guid.ToString() + " ";
    }

    #endregion

    #region Запуск с WaitHandler

    private void StartWithWaitHandler(IExecProc Proc)
    {
      IAsyncResult ar = Proc.BeginExecute(NamedValues.Empty, null, null);
      LocalProcCreator.DebugRefs.Add(ar.AsyncWaitHandle);
      using (Splash spl = new Splash("Ожидание срабатывания WaitHandle"))
      {
        ar.AsyncWaitHandle.WaitOne();
      }
      try
      {
        Proc.EndExecute(ar);
        Console.WriteLine(GetPrefix(Proc) + "Сигнал от WaitHandle получен");
      }
      catch (Exception e)
      {
        Console.WriteLine(GetPrefix(Proc) + "Ошибка EndExecute(). " + e.Message);
      }
    }

    private void StartWithWaitHandler(RemoteDistributedProc Proc)
    {
      IAsyncResult ar = Proc.BeginExecute(null, null);
      LocalProcCreator.DebugRefs.Add(ar.AsyncWaitHandle);
      using (Splash spl = new Splash("Ожидание срабатывания WaitHandle"))
      {
        ar.AsyncWaitHandle.WaitOne();
      }
      try
      {
        Proc.EndExecute(ar);
        Console.WriteLine(GetPrefix(Proc) + "Сигнал от WaitHandle получен");
      }
      catch (Exception e)
      {
        Console.WriteLine(GetPrefix(Proc) + "Ошибка EndExecute(). " + e.Message);
      }
    }

    #endregion

    #region Запуск с AsyncCallback

    private void StartWithAsyncCallback(IExecProc Proc)
    {
      IAsyncResult ar = Proc.BeginExecute(NamedValues.Empty, new AsyncCallback(AsyncCallbackFinished), Proc);
      Console.WriteLine(GetPrefix(Proc) + "Процедура запущена, ждем уведомление");
    }

    /// <summary>
    /// Этот метод вызывается асинхронно
    /// </summary>
    /// <param name="ar"></param>
    private static void AsyncCallbackFinished(IAsyncResult ar)
    {
      IExecProc Proc = (IExecProc)(ar.AsyncState);

      try
      {
        Proc.EndExecute(ar);
        Console.WriteLine(GetPrefix(Proc) + "Сигнал от AsyncCallback получен");
      }
      catch (Exception e)
      {
        // Нельзя использовать EFPApp.ShowException
        Console.WriteLine(GetPrefix(Proc) + "Ошибка EndExecute() в AsyncCallback. " + e.Message);
      }
    }

    #endregion

    #region Немедленный вызов EndExecute

    private static void StartWithEndExecute(IExecProc Proc)
    {
      IAsyncResult ar = Proc.BeginExecute(NamedValues.Empty, null, null);
      Console.WriteLine(GetPrefix(Proc) + "Процедура запущена, сразу вызываем EndExecute()");
      try
      {
        try
        {
          Proc.EndExecute(ar);
        }
        finally
        {
          Console.WriteLine(GetPrefix(Proc) + "EndExecute() выполнен");
        }
      }
      catch (Exception e)
      {
        // Нельзя использовать EFPApp.ShowException
        Console.WriteLine(GetPrefix(Proc) + "Ошибка EndExecute(). " + e.Message);
      }
    }


    #endregion

    #endregion
  }
}
