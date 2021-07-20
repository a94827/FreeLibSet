using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Remoting;
using AgeyevAV;
using System.Diagnostics;

namespace TestExecProc
{
  /// <summary>
  /// ���������� ����� ���������� ��� �������� �������
  /// </summary>
  [Serializable]
  public class ProcTestException : ApplicationException
  {
    #region ������������

    /// <summary>
    /// ������� ����� ������ ����������
    /// </summary>
    /// <param name="message">���������</param>
    public ProcTestException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// ��� ������ ������������ ����� ��� ���������� ��������������
    /// </summary>
    protected ProcTestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }


  /// <summary>
  /// ������ ��� �������� �������� ExecProc.
  /// ����� ������������ �� ��������� ������ ���������� ��� �� ���������� ������
  /// </summary>
  public class ProcCreator : MarshalByRefSponsoredObject
  {
    #region �����������

    public ProcCreator()
    {
      // ��������� �����������
      Trace.Listeners.Add(new ConsoleTraceListener());

      Trace.WriteLine("�������� ����������� ��� ������ " + AppDomain.CurrentDomain.FriendlyName);

      base.EternalLife = true; // ���������� �����

      DebugRefs = new WeakReferenceCollection<object>();
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// ���� ����� ������������ ��� ��������� ������ ����������, ����� ����������� ��������������� ExecProc
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
    /// ���� ����� ������������ ��� ���������� ������, ����� ������������ RemoteExecProc.
    /// ���������� ������, ������� ��������� marshal-by-reference
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

    #region ������� ExecProc

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
      #region �����������

      public SimpleWaitProc(int Time)
      {
        this.Time = Time;
        this.DisplayName = "���� ���������� � ������� " + Time.ToString() + " ������";
      }

      #endregion

      #region ����

      /// <summary>
      /// ����� ���������� � ��������
      /// </summary>
      public int Time;

      #endregion

      #region ����������

      protected override NamedValues OnExecute(NamedValues Args)
      {
        ISplash spl = BeginSplash("���� �������");
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
      #region �����������

      public FastProc()
      {
        this.DisplayName = "������� ���������� 0.1 �������";
      }

      #endregion

      #region ����

      #endregion

      #region ����������

      protected override NamedValues OnExecute(NamedValues Args)
      {
        System.Threading.Thread.Sleep(100);
        return NamedValues.Empty;
      }

      #endregion
    }


    private class WaitAndCrashProc : ExecProcExt
    {
      #region �����������

      public WaitAndCrashProc()
      {
        this.DisplayName = "���� ������ � ������";
      }

      #endregion

      #region ����

      #endregion

      #region ����������

      [DebuggerStepThrough]
      protected override NamedValues OnExecute(NamedValues Args)
      {
        for (int i = 0; i < 5; i++)
        {
          System.Threading.Thread.Sleep(1000);
          Trace.Write(" .. " + (i + 1).ToString());
        }
        throw new ProcTestException("�������� ���������� �� ��������� " + Guid.ToString());
      }

      #endregion
    }

    private class FastCrashProc : ExecProcExt
    {
      #region �����������

      public FastCrashProc()
      {
        this.DisplayName = "������ �����";
      }

      #endregion

      #region ����

      #endregion

      #region ����������

      [DebuggerStepThrough]
      protected override NamedValues OnExecute(NamedValues Args)
      {
        throw new ProcTestException("�������� ���������� �� ��������� " + Guid.ToString());
      }

      #endregion
    }

    #endregion

    #region ���������� ����������

    /// <summary>
    /// ���������� �������� �������� ��������
    /// </summary>
    public WeakReferenceCollection<object> DebugRefs;

    public void TraceInfo()
    {
      Trace.WriteLine("=== ����� " + AppDomain.CurrentDomain.FriendlyName + " ===");
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
          // ToString() ������ ��������� �� ������
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
