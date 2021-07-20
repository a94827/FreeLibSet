#define DEBUG_THREADS // ���� ���������, �� ������ ����� ����������� � � Release-������
#if DEBUG
#define DEBUG_THREADS
#endif

using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Logging;
using System.Threading;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * ����� �������� ExecProc � ���������������� ����������
 */

namespace AgeyevAV.Remoting
{
  #region �������� ������� Finished

  /// <summary>
  /// ��������� ������� ExecProcCallItem.Finished
  /// </summary>
  public class ExecProcCallEventArgs : EventArgs
  {
    #region ������������

    internal ExecProcCallEventArgs(ExecProcCallItem item, NamedValues results, Exception exception)
    {
      _Item = item;
      _Results = results;
      _Exception = exception;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������, ��� �������� ������� �������
    /// </summary>
    public ExecProcCallItem Item { get { return _Item; } }
    private readonly ExecProcCallItem _Item;

    /// <summary>
    /// ���������� ���������� ���������, ���� ��� ��������� �������
    /// </summary>
    public NamedValues Results { get { return _Results; } }
    private readonly NamedValues _Results;

    /// <summary>
    /// ������ ����������, ���� ������� Finished ������� �� ������� ���������� ���������� ���������
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private readonly Exception _Exception;

    #endregion
  }

  /// <summary>
  /// ������� ������� ExecProcCallItem.Finished
  /// </summary>
  /// <param name="sender">������ ��� �������� ������� �������</param>
  /// <param name="args">��������� �������</param>
  public delegate void ExecProcCallEventHandler(object sender, ExecProcCallEventArgs args);

  /// <summary>
  /// ��������� ������� DistributedProcCallItem.Finished
  /// </summary>
  public class DistributedProcCallEventArgs : EventArgs
  {
    #region ������������

    internal DistributedProcCallEventArgs(DistributedProcCallItem item, NamedValues results, Exception exception)
    {
      _Item = item;
      _Results = results;
      _Exception = exception;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������, ��� �������� ������� �������
    /// </summary>
    public DistributedProcCallItem Item { get { return _Item; } }
    private readonly DistributedProcCallItem _Item;

    /// <summary>
    /// ���������� ���������� ���������, ���� ��� ��������� �������
    /// </summary>
    public NamedValues Results { get { return _Results; } }
    private readonly NamedValues _Results;

    /// <summary>
    /// ������ ����������, ���� ������� Finished ������� �� ������� ���������� ���������� ���������
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private readonly Exception _Exception;

    #endregion
  }

  /// <summary>
  /// ������� ������� DistributedProcCallItem.Finished
  /// </summary>
  /// <param name="sender">������ ��� �������� ������� �������</param>
  /// <param name="args">��������� �������</param>
  public delegate void DistributedProcCallEventHandler(object sender, DistributedProcCallEventArgs args);

  #endregion

  #region ������������ ExecProcCallItemState

  /// <summary>
  /// ������� ��������� ������� ExecProcCallItem
  /// </summary>
  public enum ExecProcCallItemState
  {
    /// <summary>
    /// ������ ExecProcCallItem ������, �� ��� �� ������� �� ����������
    /// </summary>
    NotStarted,

    /// <summary>
    /// ��������� �����������
    /// </summary>
    Executing,

    /// <summary>
    /// ���������� ��������� �������
    /// </summary>
    Finished,

    /// <summary>
    /// ���������� ��������� � �������
    /// </summary>
    Failed
  }

  #endregion

  /// <summary>
  /// ������� ����� ��� ExecProcCallItem � DistributedProcCallItem
  /// </summary>
  public abstract class ExecProcCallItemBase : SimpleDisposableObject
  {
    #region ����������� � Dispose

    internal ExecProcCallItemBase()
    {
      _UserData = new NamedValues();
      _State = ExecProcCallItemState.NotStarted;

#if DEBUG_THREADS
      _CallerThread = Thread.CurrentThread;
#endif
    }

    /// <summary>
    /// ������� ���������� ������ � ����� ������� Disposed
    /// </summary>
    /// <param name="disposing">True, ���� ������ ����� Dispose(), � �� ����������</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
#if DEBUG_THREADS
        if (!IsAsync)
          CheckCallerThread();
#endif

        AsyncResult = null;
        AsyncResultHandler = null;
        if (SplashWatcher != null)
        {
          try
          {
            SplashWatcher.UseDefaultSplash = false;
            SplashWatcher.ClearClientStack();
          }
          catch (Exception e)
          {
            AddExceptionInfo(e);
            LogoutTools.LogoutException(e, "������ ������ ClientSplashWatcher.ClearClientStack()"); // ��� �� ������ �������� ���������
            _State = ExecProcCallItemState.Failed; // 16.01.2020
          }
          SplashWatcher = null;
        }
      }

      base.Dispose(disposing);

      EventHandler ehDisposed = Disposed; // 12.01.2021. ��������� ����������� ������������ ������������� � ������������ ������������ �������
      if (ehDisposed != null) 
      {
        try
        {
          ehDisposed(this, EventArgs.Empty);
        }
        catch (Exception e)// 16.01.2020
        {
          AddExceptionInfo(e);
          LogoutTools.LogoutException(e, "������ ������ ����������� ������� ExecProcCallItem.Disposed"); // ��� �� ������ �������� ���������
          _State = ExecProcCallItemState.Failed;
        }
      }

      if (_State == ExecProcCallItemState.Executing)
        _State = ExecProcCallItemState.Failed; // 05.12.2019
    }

    /// <summary>
    /// ������� ���������� ����� ���������� ������ � ���������� � ExecProcCallList.
    /// �� ������ ������ ������� Disposed, ������� Finished � ����� ExecProc.Dispose() ��� �������.
    /// ��� ����������� ������ �������������, ��� ����� ����� �������� �� ����� �� ������, � ������� ���� �������� ���������.
    /// ��� ������������ ������ ����� ����� ���� �����.
    /// </summary>
    public event EventHandler Disposed;

    #endregion

    #region �����

#if DEBUG_THREADS

    /// <summary>
    /// �����, � ������� ��� ������ ������ ExecProcCallItem
    /// </summary>
    private Thread _CallerThread;

    internal void CheckCallerThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _CallerThread))
        throw new DifferentThreadException(_CallerThread);
    }

#endif

    #endregion

    #region ��������

    /// <summary>
    /// ������� ��������� ���������
    /// </summary>
    public ExecProcCallItemState State { get { return _State; } internal set { _State = value; } }
    private ExecProcCallItemState _State;

    /// <summary>
    /// ������������ ���������������� ������, ������� ����� ������������� ����������� ������� Finished.
    /// </summary>
    public NamedValues UserData { get { return _UserData; } }
    private readonly NamedValues _UserData;

    /// <summary>
    /// ������������� ��������� ExecProc
    /// </summary>
    public abstract Guid Guid { get;}

    /// <summary>
    /// ������������ �������� � ������ ��������.
    /// ���� �������� �� ������ � ����� ����, ������������ IExecProc.DisplayName
    /// </summary>
    public string DisplayName
    {
      get { return _DisplayName; }
      set
      {
        CheckNotStarted();
#if DEBUG_THREADS
        CheckCallerThread();
#endif
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// ���� �������� ����������� � true, �� ������� Finished ����� ������� � � ������ ����������
    /// ���������� ���������� ���������.
    /// �� ��������� (false), ��� ������������� ������, ��� �������������� ��� ������� ������� Finished
    /// </summary>
    public bool UseFinishedWhenFailed
    {
      get { return _UseFinishedWhenFailed; }
      set
      {
        CheckNotStarted();
#if DEBUG_THREADS
        CheckCallerThread();
#endif
        _UseFinishedWhenFailed = value;
      }
    }
    private bool _UseFinishedWhenFailed;

    /// <summary>
    /// �������� ���������� true, ���� ��������� ���� �������� ����������.
    /// �������� ������������� ������ ����� ������� ��������� �� ����������
    /// </summary>
    public bool IsAsync { get { return _IsAsync; } }
    private bool _IsAsync;

    /// <summary>
    /// ������ ��������� ExecProc ��� RemoteExecProc
    /// </summary>
    internal abstract IExecProc InternalExecProc { get; }

    #endregion

    #region ������� Finished

    internal abstract bool HasFinishedEventHander { get; }

    internal abstract void OnFinished(NamedValues results, Exception exception);

    #endregion

    #region �������� �������� � ������

    /// <summary>
    /// ��������� ���������� ����������� �������.
    /// ������������ ���������� �� ������� SplashWatcher
    /// </summary>
    internal IAsyncResult AsyncResult;

    /// <summary>
    /// ���� �������� �� ������� �������, ������������ ExecProcCallList.CreateSplashStack() 
    /// </summary>
    internal ClientSplashWatcher SplashWatcher;

    /// <summary>
    /// ��������� ���������� ����������� �������.
    /// ������������, ���� ExecProcCallList.CreateSplashStack() ������ ���� ��������, ������� ����� ���������������
    /// </summary>
    internal AsyncResultWithSplashHandler AsyncResultHandler;

    internal abstract NamedValues EndExecute(IAsyncResult asyncResult);

    #endregion

    #region IReadOnlyObject Members

    /*
    bool IReadOnlyObject.IsReadOnly { get { return FUserData.IsReadOnly; } }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      if (FUserData.IsReadOnly)
        throw new ObjectReadOnlyException();
    }
    */

    private void CheckNotStarted()
    {
      if (State != ExecProcCallItemState.NotStarted)
        throw new InvalidOperationException("��������� " + DisplayName + " ��� ��������");
      // 19.08.2020
      // �������� ������, ��� ��� ��� ����������� ������ ������� �����
      //if (!AgeyevAV.Remoting.ExecProc.GetCanStart(ExecProc.State))
      //  throw new InvalidOperationException(" ��������� " + ExecProc.DisplayName + " �� ������ � �������, ��� ��� ��������� � ��������� " + ExecProc.State.ToString());
    }

    internal void SetStarted(bool isAsync)
    {
      CheckNotDisposed();
      //if (FState != ExecProcCallItemState.NotStarted)
      //  throw new InvalidOperationException("��������� ���������� ��� ������ � ���� �� ������� ExecProcCallItem �� �����������");
      CheckNotStarted();


      // ������ 28.09.2017 FUserData.SetReadOnly();
      _State = ExecProcCallItemState.Executing;
      _IsAsync = isAsync;
    }

    #endregion

    #region �������

    internal virtual void AddExceptionInfo(Exception e)
    {
      try
      {
        e.Data["ExecProcCallItem.DisplayName"] = this.DisplayName;
        e.Data["ExecProcCallItem.State"] = this.State;
        e.Data["ExecProcCallItem.IsAsync"] = this.IsAsync;
        e.Data["ExecProcCallItem.UseFinishedWhenFailed"] = this.UseFinishedWhenFailed;
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��� ������ � ���������� ����������.
  /// ������ ExecProcCallItem (� ������� �� ExecProc) �������� "�����������". 
  /// ���� ��������� ��������� ��������� ��� ���, �� ������� ������� ����� ������ (�� ������� �������������
  /// �������� ExecProcCallItem.DisposeExecProc=false).
  /// �������� �� ���������� ���������� IDisposable, ��� ������������� �������� ����� Dispose()
  /// �� ����������������� ����. ����������� ������� ����������� ExecProcCallList.
  /// </summary>
  public sealed class ExecProcCallItem : ExecProcCallItemBase
  {
    #region ����������� � Dispose

    /// <summary>
    /// ������� ������� ��� �������� ���������.
    /// </summary>
    /// <param name="execProc"></param>
    public ExecProcCallItem(IExecProc execProc)
    {
      if (execProc == null)
        throw new ArgumentNullException("execProc");
      _ExecProc = execProc;
      base.DisplayName = execProc.DisplayName;
    }

    /// <summary>
    /// �������� IExecProc.Dispose(), ���� <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">True, ���� ������ ����� Dispose(), � �� ����������</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (InternalExecProc.AutoDispose)
        {
          try
          {
            InternalExecProc.Dispose();
          }
          catch (Exception e) // 16.01.2020
          {
            AddExceptionInfo(e);
            LogoutTools.LogoutException(e, "������ ������ IExecProc.Dispose()"); // ��� �� ������ �������� ���������
            base.State = ExecProcCallItemState.Failed;
          }
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� IExecProc.Guid
    /// </summary>
    public override Guid Guid { get { return _ExecProc.Guid; } }

    /// <summary>
    /// ������ ��������� ExecProc ��� RemoteExecProc
    /// </summary>
    public IExecProc ExecProc { get { return _ExecProc; } }
    internal override IExecProc InternalExecProc { get { return _ExecProc; } }
    private readonly IExecProc _ExecProc;

    internal override NamedValues EndExecute(IAsyncResult asyncResult)
    {
      return _ExecProc.EndExecute(asyncResult);
    }

    #endregion

    #region ������� Finished

    /// <summary>
    /// �������, ���������� ��� �������� ���������� ���������.
    /// ���� ����������� �������� UseFinishedWhenFailed, �� ������� ����� ������� ����� ��� ���������
    /// ���������� ���������.
    /// ���� ���������� �� ����������, ����������, ������������ ���������� ����� ���������������.
    /// ��� ����������� ������ �������������, ��� ����� ����� �������� �� ����� �� ������, � ������� ���� �������� ���������.
    /// ��� ������������ ������ ����� ����� ���� �����.
    /// </summary>
    public event ExecProcCallEventHandler Finished;

    internal override bool HasFinishedEventHander { get { return Finished != null; } }

    internal override void OnFinished(NamedValues results, Exception exception)
    {
#if DEBUG_THREADS
      if (!IsAsync)
        CheckCallerThread();
#endif

      ExecProcCallEventHandler ehFinished = Finished; // 12.01.2021. ��������� ����������� ������������ ������������� � ������������ ������������ �������
      if (ehFinished != null)
      {
        ExecProcCallEventArgs args = new ExecProcCallEventArgs(this, results, exception);
        ehFinished(this, args);
      }
    }

    #endregion

    #region �������

    internal override void AddExceptionInfo(Exception e)
    {
      base.AddExceptionInfo(e);

      try
      {
        e.Data["ExecProcCallItem.ExecProc.Info"] = this.InternalExecProc.GetInfo();
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��� ������ � ���������� ����������.
  /// ������ ExecProcCallItem (� ������� �� ExecProc) �������� "�����������". 
  /// ���� ��������� ��������� ��������� ��� ���, �� ������� ������� ����� ������ (�� ������� �������������
  /// �������� ExecProcCallItem.DisposeExecProc=false).
  /// �������� �� ���������� ���������� IDisposable, ��� ������������� �������� ����� Dispose()
  /// �� ����������������� ����. ����������� ������� ����������� ExecProcCallList.
  /// </summary>
  public sealed class DistributedProcCallItem : ExecProcCallItemBase
  {
    #region ����������� � Dispose

    /// <summary>
    /// ������� ������� ��� ���������, ������� �� ������� �������.
    /// </summary>
    /// <param name="startData">������ ������ ExecProc.StartDistributedCall() �� ������� ������� � ���������� ������� ��� ����������</param>
    public DistributedProcCallItem(DistributedCallData startData)
    {
      if (startData == null)
        throw new ArgumentNullException("startData");
      base.DisplayName = startData.DisplayName;
      _DistributedProc = new RemoteDistributedProc(startData);
    }

    internal DistributedProcCallItem(RemoteDistributedProc distributed)
    {
      base.DisplayName = distributed.StartData.DisplayName;
      _DistributedProc = distributed;
    }

    /// <summary>
    /// �������� IExecProc.Dispose(), ���� <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">True, ���� ������ ����� Dispose(), � �� ����������</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          _DistributedProc.Dispose();
        }
        catch (Exception e)
        {
          AddExceptionInfo(e);
          LogoutTools.LogoutException(e, "������ ������ RemoteSingleCallExecProc.Dispose()"); // ��� �� ������ �������� ���������
          base.State = ExecProcCallItemState.Failed;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� DistributedCallData.Guid
    /// </summary>
    public override Guid Guid { get { return _DistributedProc.StartData.Guid; } }

    /// <summary>
    /// ������ ��������� RemoteExecProc ��� null
    /// </summary>
    internal override IExecProc InternalExecProc { get { return _DistributedProc.MainProc; } }

    /// <summary>
    /// ��������� � ����� APM.
    /// </summary>
    public RemoteDistributedProc DistributedProc { get { return _DistributedProc; } }
    private RemoteDistributedProc _DistributedProc;

    internal override NamedValues EndExecute(IAsyncResult asyncResult)
    {
      return _DistributedProc.EndExecute(asyncResult);
    }

    #endregion

    #region ������� Finished

    /// <summary>
    /// �������, ���������� ��� �������� ���������� ���������.
    /// ���� ����������� �������� UseFinishedWhenFailed, �� ������� ����� ������� ����� ��� ���������
    /// ���������� ���������.
    /// ���� ���������� �� ����������, ����������, ������������ ���������� ����� ���������������.
    /// ��� ����������� ������ �������������, ��� ����� ����� �������� �� ����� �� ������, � ������� ���� �������� ���������.
    /// ��� ������������ ������ ����� ����� ���� �����.
    /// </summary>
    public event DistributedProcCallEventHandler Finished;

    internal override bool HasFinishedEventHander { get { return Finished != null; } }

    internal override void OnFinished(NamedValues results, Exception exception)
    {
#if DEBUG_THREADS
      if (!IsAsync)
        CheckCallerThread();
#endif

      DistributedProcCallEventHandler ehFinished = Finished; // 12.01.2021. ��������� ����������� ������������ ������������� � ������������ ������������ �������
      if (ehFinished != null)
      {
        DistributedProcCallEventArgs args = new DistributedProcCallEventArgs(this, results, exception);
        ehFinished(this, args);
      }
    }

    #endregion

    #region �������

    internal override void AddExceptionInfo(Exception e)
    {
      base.AddExceptionInfo(e);

      try
      {
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// ������ �������� ���������� ������������ ������ ��������
  /// ������������ ��������� ������ ��������� � ���������� �������
  /// (���� ������������ ExtForms, �� ����� ������������ �������� EFPApp.ExecProcList) 
  /// ��� ������� ��������� ������� ��������� ����� ������ Add(), �������� ���������� �������� Args � ����������
  /// Finished. ��� ���� ����������� ExecuteAsync()
  /// ����� �� ������� ���������� Process(). ���� ���� �� ����������� �������� ��������� ������, ����������
  /// ���������� (� �������� ������). ���� ���������� ��������� � �������, ����������� ���������� �������������
  /// ������ ����� �� �������� ����������������. ��������� ������ ������ ���������� �� ��������� ������ ����������.
  /// </summary>
  public class ExecProcCallList
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    public ExecProcCallList()
    {
      _Items = new List<ExecProcCallItemBase>();

#if DEBUG_THREADS
      _MainThread = Thread.CurrentThread;
#endif
    }

    #endregion

    #region �����

#if DEBUG_THREADS

    /// <summary>
    /// �������� ����� (����������) � ������� ������ ExecProcCallList
    /// </summary>
    private Thread _MainThread;

    internal void CheckMainThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _MainThread))
        throw new DifferentThreadException(_MainThread);
    }

#endif

    #endregion

    #region ������

    /// <summary>
    /// ������ ��������������� ��������.
    /// ��� ��������� � ������ �� �����������
    /// </summary>
    private readonly List<ExecProcCallItemBase> _Items;

    /// <summary>
    /// ���������� true, ���� ��������� ExecProc ��� �� ��������� (��� ���������� ��� �� ����������).
    /// ���� ����� ����� ���������� �� ������ ������.
    /// </summary>
    /// <param name="execProc">������� ���������</param>
    /// <returns></returns>
    public bool Contains(IExecProc execProc)
    {
      lock (_Items)
      {
        for (int i = 0; i < _Items.Count; i++)
        {
          if (_Items[i].InternalExecProc == execProc)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// ���������� ����� ������� �������� � ������.
    /// ���� ����� ����� ���������� �� ������ ������
    /// </summary>
    /// <returns>����� ������</returns>
    public IExecProc[] ToArray()
    {
      List<IExecProc> lst = new List<IExecProc>();
      lock (_Items)
        for (int i = 0; i < _Items.Count; i++)
        {
          IExecProc p = _Items[i].InternalExecProc;
          if (p != null)
            lst.Add(p);
        }
      return lst.ToArray();
    }

    /// <summary>
    /// ���������� ���������� �������� � ������.
    /// � �������� ����� ���������� �� ������ ������.
    /// �������� ����� ������������ ������ � ���������� �����, ��� ��� ������ ����� �������� ����������.
    /// </summary>
    public int Count
    {
      get
      {
        lock (_Items)
        {
          return _Items.Count;
        }
      }
    }

    ///// <summary>
    ///// ���������� ��������� �� �������
    ///// </summary>
    ///// <param name="index">������ � ������</param>
    ///// <returns>������ IExecProc</returns>
    //public IExecProc this[int index]
    //{
    //  get { return _Items[index].ExecProc; }
    //}

    ///// <summary>
    ///// ���������� ������������ ��� ����������� ���������.
    ///// </summary>
    ///// <param name="index">������ ��������� � ������ ��������</param>
    ///// <returns>�������� IExecProc.DisplayName, ���� ��� �� ���� �������������� ��� ������ ������ ExecuteAsync()</returns>
    //public string GetDisplayName(int index)
    //{
    //  return _Items[index].DisplayName;
    //}


    #endregion

    #region �������� �������� SplashStack

    /// <summary>
    /// ������� ���� ��������, ������� ����� ��������� � �������� ���������� ���������.
    /// ������������������ ����� ���������� null, ��� ���� �������� �� ������������.
    /// ���� ����� ����� ���������� �� ������ ������, �� ����������� �� ���������.
    /// </summary>
    /// <returns></returns>
    protected virtual ISplashStack CreateSplashStack()
    {
      //CheckMainThread();
      return null;
    }

    private bool? _SplashStackSupportedFlag;

    /// <summary>
    /// ���������� true, ���� ������ ������ ������������ �������� ��������.
    /// �������� �� ������ ������
    /// </summary>
    /// <returns></returns>
    private bool IsSplashStackSupported()
    {
      if (!_SplashStackSupportedFlag.HasValue)
        _SplashStackSupportedFlag = CreateSplashStack() != null;
      return _SplashStackSupportedFlag.Value;
    }

    #endregion

    #region ������ ����������

    #region ��� ExecProcCallItem

    #region ���������� ���������� ExecuteSync()

    /// <summary>
    /// ���������� ����� �������.
    /// �� ������������� �������� ����� ��� ��������� ���������, ��� ��� ����� ���������� ������ ����-����.
    /// ����������� ����� ExecuteAsyncAndWait().
    /// ���� ����� ����� ���������� �� ������ ������, ��� ���������, ��� � ��������.
    /// </summary>
    /// <param name="item">���������� ���������</param>
    /// <param name="args">���������, ������������ �������</param>
    /// <returns>���� ��������� ������� �������, �� ���������� ���������� ����������.
    /// ���� � ��������� �������� ����������, �� ��� ���� ���������� � ����������� Finished, �� ������������ null</returns>
    public virtual NamedValues ExecuteSync(ExecProcCallItem item, NamedValues args)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      NamedValues Results;

      try
      {
        try
        {
          item.SetStarted(false);
          try
          {
            if (IsSplashStackSupported())
            {
              if (item.InternalExecProc is ExecProc)
                Results = DoExecuteSyncWithSyncSplash(item.InternalExecProc as ExecProc, args);
              else
                Results = DoExecuteSyncWithAsyncSplash(item, args);
            }
            else
            {
              // �������� �� ������������
              Results = item.InternalExecProc.Execute(args);
            }
          }
          catch (Exception e2)
          {
            item.AddExceptionInfo(e2);
            OnFailed(item, e2);
            Results = null;
          }

          if (Results != null)
            OnFinished(item, Results);
        }
        catch (Exception e1)
        {
          item.AddExceptionInfo(e1);
          throw;
        }
      }
      finally
      {
        try
        {
          item.Dispose();
        }
        catch { }
      }

      return Results;
    }

    /// <summary>
    /// ���������� � ��������� �������� � ������� ������
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private NamedValues DoExecuteSyncWithSyncSplash(ExecProc proc, NamedValues args)
    {
      NamedValues res;
      ISplashStack sstack = CreateSplashStack();
#if DEBUG
      if (sstack == null)
        throw new NullReferenceException("CreateSplashStack() ������ null");
#endif

      ISplashStack oldss = proc.SplashStack;
      try
      {
        proc.SplashStack = sstack;
        // ������� SplashWatcher'�� �� �����. ��������� ���� ��������� ��������� ��������.

        res = proc.Execute(args);
      }
      finally
      {
        proc.SplashStack = oldss; // ���������������, ����� ��������� ����� ��� ��� ������� ��� ����� ExecProcCallList � ��� ��������

        // ������� ����
        while (sstack.Splash != null)
          sstack.EndSplash();
      }

      return res;
    }


    private class AsyncSplashExecutor
    {
      #region ����

      /// <summary>
      /// ���������, ������� ���������� � �������� ������
      /// </summary>
      public ExecProcCallItem Item;

      /// <summary>
      /// ������, ��������������� ����� ����, ��� ����������  ���������
      /// </summary>
      public bool Finished;

      #endregion
    }

    /// <summary>
    /// ���������� � ��������� �������� � ��������� ������
    /// </summary>
    /// <param name="item"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private NamedValues DoExecuteSyncWithAsyncSplash(ExecProcCallItem item, NamedValues args)
    {
      NamedValues res;

      AsyncSplashExecutor ase = new AsyncSplashExecutor();
      try
      {
        ase.Item = item;
        ThreadPool.QueueUserWorkItem(RunAsyncSplash, ase);
        res = item.InternalExecProc.Execute(args);
      }
      finally
      {
        ase.Finished = true;
      }

      return res;
    }

    private void RunAsyncSplash(object state)
    {
      AsyncSplashExecutor ase = (AsyncSplashExecutor)state;

      try
      {
        // ������ Finished ����� ���� ���������� � ����� ������.
        // ����� ����, ��� � ������ ����������. 
        //Thread.Sleep(100); // �����
        // this.Sleep(100); // 10.12.2019 ����� ���������� DoEvents()
        Thread.Sleep(100); // 20.08.2020 ���������� ������� ��� ������ DoEvents


        if (ase.Finished)
          return;

        IServerSplashWatcher ssw = ase.Item.InternalExecProc.CreateSplashWatcher();
        ISplashStack sstack = CreateSplashStack();
#if DEBUG
        if (sstack == null)
          throw new NullReferenceException("CreateSplashStack() ������ null");
#endif

        ClientSplashWatcher csw = new ClientSplashWatcher(ssw, sstack);
        try
        {
          csw.DefaultSplashPhaseText = "�����������: " + ase.Item.DisplayName;
          csw.UseDefaultSplash = this.UseDefaultSplash;

          while (!ase.Finished)
          {
            try
            {
              for (int i = 0; i < 9; i++)
              {
                //Thread.Sleep(100);
                this.Sleep(100); // 21.01.2021. ����� Sleep ��������� �������� ����������
                csw.UseDefaultSplash = this.UseDefaultSplash;
              }
              //Thread.Sleep(100);// 16.12.2019
              this.Sleep(100); // 21.01.2021

              csw.ProcessSplash();
            }
            catch { } // ������������ ������, ��������, ����-���� ����

          }
        }
        finally
        {
          // ������� ����
          csw.UseDefaultSplash = false;
          csw.ClearClientStack();
        }
      }
      catch (Exception e)
      {
        // ������ ��������� ���������� �� ����� ������
        ase.Item.AddExceptionInfo(e);
        OnUnhandledException(e, "������ � RunAsyncSplash");
      }
    }

    /// <summary>
    /// ���������� ����� �������.
    /// �� ������������� �������� ����� ��� ��������� ���������, ��� ��� ����� ���������� ������ ����-����.
    /// ����������� ����� ExecuteAsyncAndWait().
    /// ���� ����� ����� ���������� �� ������ ������, ��� ���������, ��� � ��������.
    /// </summary>
    /// <param name="execProc">���������� ���������</param>
    /// <param name="args">���������, ������������ �������</param>
    /// <returns>���������� ����������.</returns>
    public NamedValues ExecuteSync(IExecProc execProc, NamedValues args)
    {
      ExecProcCallItem item = new ExecProcCallItem(execProc);
      return ExecuteSync(item, args);
    }

    #endregion

    #region ����������� ���������� ExecuteAsync()

    /// <summary>
    /// �������� ��������� ���������� � ��������� ������� ��������� � ������, ������ � ������������ ����������.
    /// ���� ����� ����� ���������� �� ������ ������.
    /// </summary>
    /// <param name="item">��������� � �����������</param>
    /// <param name="args">���������, ������������ �������</param>
    public virtual void ExecuteAsync(ExecProcCallItem item, NamedValues args)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      try
      {
        item.SetStarted(true);

        ISplashStack splStack = CreateSplashStack();
        if (splStack == null)
          item.AsyncResult = item.InternalExecProc.BeginExecute(args, null, null); // ����� ����� ���������� ����������, ���� ��������� ������ ���������
        else
        {
          IAsyncResultWithSplash ar2 = item.InternalExecProc.BeginExecuteWithSplash(args, null, null);
          item.AsyncResult = ar2;
          item.AsyncResultHandler = new AsyncResultWithSplashHandler(ar2);
          item.SplashWatcher = new ClientSplashWatcher(item.AsyncResultHandler, splStack);
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.DefaultSplashPhaseText = "�����������: " + item.DisplayName;
        }
        lock (_Items)
        {
          _Items.Add(item);
        }
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);

        try
        {
          item.Dispose();
        }
        catch { }

        throw;
      }
    }

    #endregion

    #region ������������� ����� Execute()

    /// <summary>
    /// ��������� ���������� ��� ����������� �����, � ����������� �� <paramref name="isAsync"/>.
    /// ������, ���� ���� ��������� ����� ���������� � ��� � ���.
    /// ���� ����� ����� ���������� �� ������ ������.
    /// </summary>
    /// <param name="isAsync">true - ����������, false </param>
    /// <param name="item">��������� � �����������</param>
    /// <param name="args">���������, ������������ �������</param>
    public void Execute(bool isAsync, ExecProcCallItem item, NamedValues args)
    {
      if (isAsync)
        ExecuteAsync(item, args);
      else
        ExecuteSync(item, args);
    }

    #endregion

    #region ExecuteAsyncAndWait()

    /// <summary>
    /// ������������ ������� ExecuteAsyncAndWait, ����������� ExecProcCallItem.
    /// ������ ����������, ����������� IExecProc, ������� ����������� ExecProcCallItem � ����� ������������ ��� ��� ������������� ���������.
    /// </summary>
    private class ExecProcExecuteAsyncAndWaitHandler
    {
      #region ����

      public NamedValues Results;

      public Exception Exception;

      #endregion

      #region �����

      public void Item_Finished(object Sender, ExecProcCallEventArgs Args)
      {
        Results = Args.Results;
        Exception = Args.Exception;
      }

      #endregion
    }

    /// <summary>
    /// ����������� ����� � ���������.
    /// � ������� �� ������ ExecuteAsync(), ������� ����� ���������� ����������,
    /// ����� ����� ������ �� �����������, ���� ���������� �� ����� ���������.
    /// ����� ����� ���������� � ����� ������.
    /// ����� ������ �������������� � ��������� ����������, ������� ����������� ����� � ����� ������� ������ ����-���� ��� ������������� ExecuteSync(),
    /// � ������������ ExecuteAsync() ������, ��� ��� ��������� �������� ���������� � ������� ������.
    /// ������������� � ExecProc ����� �����������, ���� � ��������� ������� ����� ���� �������� ������������ ExecuteSync().
    /// ����������, ���� ����� �������� ExecuteAsync() � ���������� ���������� ���������. ����� ���������� ������������ � �������� ������.
    /// ���� ��� ���������� ��������� �������� ����������, �� ��� ����������������� � �������� ������. ��������������, ��� � <paramref name="item"/>
    /// ��� ����� ��������� ������.
    /// </summary>
    /// <param name="item">������ ���������</param>
    /// <param name="args">��������� ���������</param>
    /// <returns>���������� ���������� ���������</returns>
    public virtual NamedValues ExecuteAsyncAndWait(ExecProcCallItem item, NamedValues args)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      // 09.12.2019
      // ������� ����� ExecProcCallItem.
      // � ��������� ����� ���� �� ����������� �������� UseFinishedWhenFailed, �� ���� ���������� ������� Finished. ����� ���������� ������� �����
      // ������ ��� ��������� ������, � ���� ���������� �� �����
      ExecProcCallItem item2 = new ExecProcCallItem(item.InternalExecProc);
      item2.DisplayName = item.DisplayName;

      ExecProcExecuteAsyncAndWaitHandler TempHandler = new ExecProcExecuteAsyncAndWaitHandler();
      item2.UseFinishedWhenFailed = true;
      item2.Finished += TempHandler.Item_Finished;
      try
      {
        ExecuteAsync(item2, args);
        Thread.Sleep(0); // 19.08.2020
        // ������! ProcessItem(item2); // 20.08.2020

        while (item2.State == ExecProcCallItemState.Executing)
        {
          //_ProcessItemCalled = false;
          Sleep(250);
          //if (!_ProcessItemCalled)
          // ������!  Process(); // 20.08.2020. ������ ����, ��� ���������������� ����� �� ������
        }
      }
      finally
      {
        item2.Dispose();
        item.Dispose();
      }

      if (TempHandler.Exception != null)
      {
        TempHandler.Exception.Data["AsyncStackTrace"] = TempHandler.Exception.StackTrace;
        //OnFailed(item, TempHandler.Exception);
        // 19.12.2019
        // �� ����, ����� ��������� ����������� �����, ������� ���������� ����������.
        // ���� ����������� ���������� ������ �� ���������� ��� ExecProcCallItem, �� ����� ���������� �������������.
        DoOnFailed(item, TempHandler.Exception);
      }
      else
        OnFinished(item, TempHandler.Results);

      return TempHandler.Results;
    }

    /// <summary>
    /// ������������������ ����� �������� Thread.Sleep().
    /// ���������������� ����� ����� ������������� ������������ ������� ��������� ����������, ����� �� "�������������" ���������������� ���������.
    /// ���� ����� ����� ���������� ����������.
    /// </summary>                                                                                       
    /// <param name="milliseconds">�������� �������� � �������������</param>
    protected virtual void Sleep(int milliseconds)
    {
      // 21.01.2021.
      // ������ ��������. ����� ����� ���������� ����������

// #if DEBUG_THREADS
      //CheckMainThread();
// #endif
      Thread.Sleep(milliseconds);
    }

    /// <summary>
    /// ����������� ����� � ���������.
    /// � ������� �� ������ ExecuteAsync(), ������� ����� ���������� ����������,
    /// ����� ����� ������ �� �����������, ���� ���������� �� ����� ���������.
    /// ����� ����� ������� �� ������ ������.
    /// ����� ������ �������������� � ��������� ����������, ������� ����������� ����� � ����� ������� ������ ����-���� ��� ������������� ExecuteSync(),
    /// � ������������ ExecuteAsync() ������, ��� ��� ��������� �������� ���������� � ������� ������.
    /// ������������� � ExecProc ����� �����������.
    /// </summary>
    /// <param name="proc">����������� ���������</param>
    /// <param name="args">��������� ������</param>
    /// <returns>���������� ������</returns>
    public virtual NamedValues ExecuteAsyncAndWait(IExecProc proc, NamedValues args)
    {
      ExecProcCallItem Item = new ExecProcCallItem(proc);
      Item.UseFinishedWhenFailed = true;
      Item.Finished += new ExecProcCallEventHandler(Item_Finished);
      ExecuteAsyncAndWait(Item, args);
      NamedValues Results = Item.UserData["Results"] as NamedValues;
      if (Results != null)
        return Results;
      Exception Exception = Item.UserData["Exception"] as Exception;
      if (Exception != null)
        throw Exception;
      throw new BugException("����� ���������� ��������� ��������� ������������ ���������");
    }

    #endregion

    #endregion

    #region ��� DistributedProcCallItem

    #region ����������� ���������� ExecuteAsync()

    /// <summary>
    /// �������� ��������� ���������� � ��������� ������� ��������� � ������, ������ � ������������ ����������.
    /// ���� ����� ����� ���������� �� ������ ������.
    /// </summary>
    /// <param name="item">��������� � �����������</param>
    public virtual void ExecuteAsync(DistributedProcCallItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      try
      {
        item.SetStarted(true);

        ISplashStack splStack = CreateSplashStack();
        if (splStack == null)
          item.AsyncResult = item.DistributedProc.BeginExecute(null, null); // ����� ����� ���������� ����������, ���� ��������� ������ ���������
        else
        {
          IAsyncResultWithSplash ar2 = item.DistributedProc.BeginExecuteWithSplash(null, null);
          item.AsyncResult = ar2;
          item.AsyncResultHandler = new AsyncResultWithSplashHandler(ar2);
          item.SplashWatcher = new ClientSplashWatcher(item.AsyncResultHandler, splStack);
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.DefaultSplashPhaseText = "�����������: " + item.DisplayName;
        }

        if (item.DistributedProc.StartData.IsCompleted)
        {
          NamedValues res=item.EndExecute(item.AsyncResult);
          OnFinished(item, res);
          item.Dispose();
        }
        else
        {
          lock (_Items)
          {
            _Items.Add(item);
          }
        }
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);

        try
        {
          item.Dispose();
        }
        catch { }

        throw;
      }
    }

    #endregion

    #region ExecuteAsyncAndWait()

    /// <summary>
    /// ������������ ������� ExecuteAsyncAndWait, ����������� ExecProcCallItem.
    /// ������ ����������, ����������� IExecProc, ������� ����������� ExecProcCallItem � ����� ������������ ��� ��� ������������� ���������.
    /// </summary>
    private class DistributedProcExecuteAsyncAndWaitHandler
    {
      #region ����

      public NamedValues Results;

      public Exception Exception;

      #endregion

      #region �����

      public void Item_Finished(object Sender, DistributedProcCallEventArgs Args)
      {
        Results = Args.Results;
        Exception = Args.Exception;
      }

      #endregion
    }

    /// <summary>
    /// ����������� ����� � ���������.
    /// � ������� �� ������ ExecuteAsync(), ������� ����� ���������� ����������,
    /// ����� ����� ������ �� �����������, ���� ���������� �� ����� ���������.
    /// ����� ����� ���������� � ����� ������.
    /// ����� ������ �������������� � ��������� ����������, ������� ����������� ����� � ����� ������� ������ ����-���� ��� ������������� ExecuteSync(),
    /// � ������������ ExecuteAsync() ������, ��� ��� ��������� �������� ���������� � ������� ������.
    /// ����������, ���� ����� �������� ExecuteAsync() � ���������� ���������� ���������. ����� ���������� ������������ � �������� ������.
    /// ���� ��� ���������� ��������� �������� ����������, �� ��� ����������������� � �������� ������. ��������������, ��� � <paramref name="item"/>
    /// ��� ����� ��������� ������.
    /// </summary>
    /// <param name="item">������ ���������</param>
    /// <returns>���������� ���������� ���������</returns>
    public virtual NamedValues ExecuteAsyncAndWait(DistributedProcCallItem item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      // ������� ����� ExecProcCallItem.
      // � ��������� ����� ���� �� ����������� �������� UseFinishedWhenFailed, �� ���� ���������� ������� Finished. ����� ���������� ������� �����
      // ������ ��� ��������� ������, � ���� ���������� �� �����
      DistributedProcCallItem item2 = new DistributedProcCallItem(item.DistributedProc);
      item2.DisplayName = item.DisplayName;

      DistributedProcExecuteAsyncAndWaitHandler TempHandler = new DistributedProcExecuteAsyncAndWaitHandler();
      item2.UseFinishedWhenFailed = true;
      item2.Finished += TempHandler.Item_Finished;
      try
      {
        ExecuteAsync(item2);
        Thread.Sleep(0);

        while (item2.State == ExecProcCallItemState.Executing)
        {
#if DEBUG
          if (item2.DistributedProc.StartData.IsCompleted)
            throw new BugException("�������� ���������, ������� ��� ���� ��������� �� �������");
#endif
          Sleep(250);
        }
      }
      finally
      {
        item2.Dispose();
        item.Dispose();
      }

      if (TempHandler.Exception != null)
      {
        TempHandler.Exception.Data["AsyncStackTrace"] = TempHandler.Exception.StackTrace;
        //OnFailed(item, TempHandler.Exception);
        // 19.12.2019
        // �� ����, ����� ��������� ����������� �����, ������� ���������� ����������.
        // ���� ����������� ���������� ������ �� ���������� ��� ExecProcCallItem, �� ����� ���������� �������������.
        DoOnFailed(item, TempHandler.Exception);
      }
      else
        OnFinished(item, TempHandler.Results);

      return TempHandler.Results;
    }

    #endregion

    #endregion

    #region ���������� ����������

    void Item_Finished(object sender, ExecProcCallEventArgs args)
    {
      if (args.Exception == null)
        args.Item.UserData["Results"] = args.Results;
      else
        args.Item.UserData["Exception"] = args.Exception;
    }

    /// <summary>
    /// ���� ����� ���������� � ������ ��������� ���������� ���������.
    /// ���� ����� ����� ���� ������ ���������� � ����� ������: � ������ ������� ���������, � �������� ������ ��� � ��������������.
    /// </summary>
    /// <param name="item">��������� ��������</param>
    /// <param name="results">���������� ���������� ���������</param>
    protected virtual void OnFinished(ExecProcCallItemBase item, NamedValues results)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (results == null)
        throw new ArgumentNullException("results");

      item.State = ExecProcCallItemState.Finished;
      
      item.OnFinished(results, null);
      item.Dispose();
    }

    /// <summary>
    /// ���� ����� ���������� � ������ ���������� ���������� ���������.
    /// 1. ������������� ��������� Failed
    /// 2. �������� ���������� OnFinished(), ���� UseFinishedWhenFailed = true, ����� ��������������� ��������� <paramref name="exception"/>.
    /// 3. �������� <paramref name="item"/>.Dispose() ���������� �� ������� ��������� ������.
    /// ����� ���������� � ��� ������, ������� ������������� ��� ������ ���������.
    /// </summary>
    /// <param name="item">��������� ���������, ��� ������� �������� ����������</param>
    /// <param name="exception">������ ����������</param>
    protected virtual void OnFailed(ExecProcCallItemBase item, Exception exception)
    {
      DoOnFailed(item, exception);
    }

    private static void DoOnFailed(ExecProcCallItemBase item, Exception exception)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (exception == null)
        throw new ArgumentNullException("exception");

#if DEBUG_THREADS
      item.CheckCallerThread();
#endif

      item.State = ExecProcCallItemState.Failed;
      try
      {
        if (item.UseFinishedWhenFailed)
          item.OnFinished(null, exception);
        else
          throw exception;
      }
      finally
      {
        item.Dispose();
      }
    }

    #endregion

    #endregion

    #region ��������� ������� �������

    /// <summary>
    /// ������ ������� ��������� � ������ FItems.
    /// �������� �� 0 �� _Items.Count-1 �� ����� ��� ������ ������ ������ Process()
    /// </summary>
    private int _CurrIndex;

    /// <summary>
    /// �������������� ��������� �������
    /// </summary>
    private bool InsideProcess;

    /// <summary>
    /// ���� ����� ������ ���������� ����������� ������� �� �������.
    /// ��������� ���������� ��������� ��������� �� ������. �� ���� ����� �������������� ������ ���� ���������, ����� ����� ��� ����������� ������.
    /// ���� � �������� ������ EndExecute() ��������� ���������, �� ���������� ����� OnFailed() � ���� ��������� �� ������.
    /// �� ������� ������� ������ ���������������� ����� OnFailed(). �� ����� ����������� ���������� ��� ������ �������� ������ ExecProcCallList.OnFailed()
    /// � ������ ��������� �� ������. ���� ����� OnFailed() �� �������������, �� ���������� ����� �������� � log-����.
    /// ���� ����� ������� �� ����������� ����������.
    /// </summary>
    public void Process()
    {
#if DEBUG_THREADS
      CheckMainThread();
#endif

      if (InsideProcess)
        return;
      InsideProcess = true;
      try
      {
        ExecProcCallItemBase Item = null;
        lock (_Items)
        {
          if (_Items.Count > 0)
          {
            if (_CurrIndex >= _Items.Count)
              _CurrIndex = 0;

            Item = _Items[_CurrIndex];

            _CurrIndex++;
          }
        }
        if (Item != null)
          ProcessItem(Item);
      }
      catch (Exception e)
      {
        OnUnhandledException(e, "���������� ������ � ExecProcCallList.Process()");
      }
      InsideProcess = false;
    }


    ///// <summary>
    ///// ���������� ������ ��������������� ProcessItem().
    ///// ������������ ��� ����������� ������, ����� ���������, ��� ����� Sleep() �������� ����� Process(),
    ///// � �� ������ ������-������.
    ///// � ���������� �� ����� �����������, �� ��������� �����, � �����
    ///// </summary>
    //private bool _ProcessItemCalled; // 20.08.2020

    private void ProcessItem(ExecProcCallItemBase item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif

      if (item.IsDisposed)
        return;

      #region �������� IsCompleted

      bool IsCompleted;

      try
      {
        if (item.AsyncResultHandler == null)
          IsCompleted = item.AsyncResult.IsCompleted; // ����������� �����
        else
        {
          IsCompleted = item.AsyncResultHandler.GetIsCompleted();
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.ProcessSplash();
        }
      }
      catch (Exception e)
      {
        try
        {
          item.AddExceptionInfo(e);
          OnFailed(item, e);
        }
        catch (Exception e2)
        {
          OnUnhandledException(e2, "��������������� ������ ������ OnFailed() ��� ��������� ���������� ������ GetIsCompleted()");
        }
        lock (_Items)
        {
          _Items.Remove(item);
        }
        return;
      }

      #endregion

      if (!IsCompleted)
        return;

      // 19.08.2020
      // ������� ������� ������� �� ������, � ����� ������ ��� ���������. � �� ��������.
      // ���������� ������� Finished ����� ����������� ����������� �����. ���� �� �������� ������,
      // �� � ExtForms ����� ������������ ExecProcCallBack, ��� ��� ���������� �������, ��� ��������� ��������� ��� 
      // ��� ����������� �� �������.

      lock (_Items)
      {
        _Items.Remove(item);
      }

      #region ����� EndExecute()

      try
      {
        DoEndExecute(item);
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);
        try
        {
          OnFailed(item, e);
        }
        catch (Exception e2)
        {
          OnUnhandledException(e2, "��������������� ������ ������ OnFailed() ��� ��������� ���������� ������ EndExecute()");
        }
      }

      #endregion

      item.Dispose(); // 05.12.2019
    }

#if XXX
    private void ProcessItem(ExecProcCallItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");

      CheckMainThread();
#endif

      //_ProcessItemCalled = true;

      if (item.IsDisposed)
        return;

    #region �������� IsCompleted

      bool IsCompleted;

      try
      {
        if (item.AsyncResultHandler == null)
          IsCompleted = item.AsyncResult.IsCompleted; // ����������� �����
        else
        {
          IsCompleted = item.AsyncResultHandler.GetIsCompleted();
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.ProcessSplash();
        }
      }
      catch (Exception e)
      {
        lock (_Items)
        {
          _Items.Remove(item);
        }

        item.AddExceptionInfo(e);
        // ������ ��������! ����� ������ �� ������ item. OnFailed(item, e);
        OnUnhandledException(e, "��������������� ������ ��� ������ GetIsCompleted()");
        return;
      }

    #endregion

      if (!IsCompleted)
        return;

      // 19.08.2020
      // ������� ������� ������� �� ������, � ����� ������ ��� ���������. � �� ��������.
      // ���������� ������� Finished ����� ����������� ����������� �����. ���� �� �������� ������,
      // �� � ExtForms ����� ������������ ExecProcCallBack, ��� ��� ���������� �������, ��� ��������� ��������� ��� 
      // ��� ����������� �� �������.

      lock (_Items)
      {
        _Items.Remove(item);
      }

    #region ����� EndExecute()

      try
      {
        DoEndExecute(item);    !!!!!
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);
        // ������ ��������!  OnFailed(item, e);
        LogoutTools.LogoutException(e, "��������������� ������ ������ EndExecute() ��� ��������� ���������� ������ EndExecute()");
      }

    #endregion

      item.Dispose(); // 05.12.2019
    }

#endif

    private void DoEndExecute(ExecProcCallItemBase item)
    {
      NamedValues Results;
      try
      {
        Results = item.EndExecute(item.AsyncResult);
      }
      catch (Exception e)
      {
        OnFailed(item, e);
        return;
      }

      OnFinished(item, Results);
    }


    /// <summary>
    /// ���� ����� ����� ���������� ����������� �������, ��������, ��� ������ ������ �������.
    /// ��������� ���������� ���� �������� �� ������. 
    /// ����� ����� �������� ������ � �������� ������.
    /// ���� � �������� ������ EndExecute() ��������� ���������, �� ���������� ����� OnFailed() � ���� ��������� �� ������.
    /// �� ������� ������� ������ ���������������� ����� OnFailed(). �� ����� ����������� ���������� ��� ������ �������� ������ ExecProcCallList.OnFailed()
    /// � ������ ��������� �� ������. ���� ����� OnFailed() �� �������������, �� ���������� ����� �������� � log-����.
    /// ���� ����� ������� �� ����������� ���������� � �������� ������.
    /// </summary>
    public void ProcessAll()
    {
#if DEBUG_THREADS
      CheckMainThread();
#endif

      if (InsideProcess)
        return;
      InsideProcess = true;
      try
      {
        ExecProcCallItemBase[] a;
        lock (_Items)
        {
          a = _Items.ToArray(); // ��������� �����, ��� ��� �������� ����� ��������� �� ������ � �������� ��������
        }

        for (int i = 0; i < a.Length; i++)
          ProcessItem(a[i]);
      }
      catch (Exception e)
      {
        OnUnhandledException(e, "���������� ������ � ExecProcCallList.ProcessAll()");
      }
      InsideProcess = false;
    }


    #endregion

    #region ���������� ���������� ��������

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� ��� ���������� ��������� ����� ���������� �������� �� ���������
    /// </summary>
    public bool UseDefaultSplash
    {
      get { return _UseDefaultSplash; }
      set
      {
        if (value == _UseDefaultSplash)
          return;

        _UseDefaultSplash = value;
      }
    }
    private bool _UseDefaultSplash;

    #endregion

    #region ��������� ������

    /// <summary>
    /// ����� ���������� ��� ������������� ������, ������� ������ �����������.
    /// ���� ����� ����� ���������� �� ������ ������, ��������� ��� ��������.
    /// ������������������ ����� �������� LogoutTools.LogoutException().
    /// ���������������� ����� ����� ����������, ������� �� ����� ����������, � ������, ��������, ��������� �� ������.
    /// </summary>
    /// <param name="e">������ ����������</param>
    /// <param name="title">���������</param>
    protected virtual void OnUnhandledException(Exception e, string title)
    {
      LogoutTools.LogoutException(e, title);
    }

    #endregion
  }
}
