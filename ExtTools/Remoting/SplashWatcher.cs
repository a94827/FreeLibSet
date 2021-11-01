// ��� ����� ���� ���������� � ��������� �������
// #define DEBUG_SPLASHWATCHERS // ���� ����������, �� ����� ����������� ����������� ServerSplashWatcher � ClientSplashWatcher

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using FreeLibSet.Core;

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

namespace FreeLibSet.Remoting
{

  /// <summary>
  /// ������ ������, ������������ �� ������� � �������.
  /// ��� - ������ ������� �����.
  /// ������ ���������� � ������� ����������� ������� 
  /// </summary>
  [Serializable]
  public abstract class SplashInfoPack
  {
    #region ���������� �����������

    internal SplashInfoPack()
    {
    }

    #endregion

#if DEBUG_SPLASHWATCHERS
    internal Guid ServerWatcherGuid;
    internal int ServerCallId;
#endif
  }

  [Serializable]
  internal class SplashInfoPackCurrentPhase : SplashInfoPack
  {
    #region ����

    public string PhaseText;

    public int Percent;

    public int PercentMax;

    public bool AllowCancel;

    #endregion
  }

  [Serializable]
  internal class SplashInfoPackSplash : SplashInfoPackCurrentPhase
  {
    #region ����

    public SplashPhaseState[] PhaseStates;

    #endregion

    #region ������

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append("PhaseStates={");
      for (int i = 0; i < PhaseStates.Length; i++)
      {
        if (i > 0)
          sb.Append("|");
        sb.Append(PhaseStates[i].ToString());
      }
      sb.Append("}");
      return sb.ToString();
    }

    #endregion
  }

  [Serializable]
  internal class SplashInfoPackStackItem : SplashInfoPackSplash
  {
    #region ����

    public string[] Phases;

    public int StackSerialId;

    #endregion

    #region ������

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append(", StackSerialId=");
      sb.Append(StackSerialId);
      sb.Append(", Phases={");
      sb.Append(String.Join("|", Phases));
      sb.Append("}");

      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// ���������� � ������ ����� ��������
  /// </summary>
  [Serializable]
  internal class SplashInfoPackStack : SplashInfoPack
  {
    #region ����

    /// <summary>
    /// �������� �������� � �����.
    /// ������� �������� ������������� ��������� �  Stack.ToArray().
    /// ������� �������� ����� ������ 0.
    /// </summary>
    public SplashInfoPackStackItem[] Stack;

    #endregion

    #region ������

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append(", SplashCount=");
      sb.Append(Stack.Length);
      sb.Append(" {");
      for (int i = 0; i < Stack.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        sb.Append("{StackSerialId=");
        sb.Append(Stack[i].StackSerialId);
        sb.Append(", Phases={");
        sb.Append(String.Join("|", Stack[i].Phases));
        sb.Append("}");
      }
      sb.Append("}");

      return sb.ToString();
    }

    #endregion
  }

#if DEBUG_SPLASHWATCHERS

  /// <summary>
  /// ��� ������� �������� �� null, � ������-��������, ����� � �������� ��� ������� ���������
  /// </summary>
  [Serializable]
  internal class SplashInfoPackNone : SplashInfoPack
  {
  }

#endif


#if XXX
  /// <summary>
  /// ������� ��� �������� �������� SplashInfoPack.
  /// ������������ ��� ����������� ��������.
  /// ���� ����� �� �������� ����������������.
  /// </summary>
  public sealed class SplashInfoPackQueue
  {
  #region �����������

    public SplashInfoPackQueue()
    {
      _Queue = new Queue<SplashInfoPack>();
    }

  #endregion

  #region �������

    public void Enqueue(SplashInfoPack pack)
    { 
#if DEBUG_SPLASHWATCHERS
      if (pack == null)
        throw new ArgumentNullException("pack");
#endif

      if (pack == null)
        return;

      // TODO: ����� ����� ���������������� �������� ������������ ��������

      _Queue.Enqueue(pack);
    }

    public SplashInfoPack Dequeue()
    {
      if (_Queue.Count == null)
      {
#if DEBUG_SPLASHWATCHERS
        return new SplashInfoPackNone();
#else
        return null;
#endif
      }
      else
        return _Queue.Dequeue();
    }

    private Queue<SplashInfoPack> _Queue;

  #endregion
  }
#endif

  /// <summary>
  /// ���������, ����������� ServerSplashWatcher
  /// </summary>
  public interface IServerSplashWatcher
  {
    #region ������

    /// <summary>
    /// �������� ��������� ������ ������ �� ���������� � ����� ��������.
    /// ��� ������ ������ ������ ��� ����� ResetSplashIOfo() ���������� ������ ���������� � �����.
    /// ��� ��������� ���������� ���������� �������� ������ ��� ���� null, ���� ��������� �� ����.
    /// ��������� ������������ ������ SplashInfoPack �������� �������� � ������������ ������������� ��� �������� ������
    /// �� ServerSplashWatcher � ClientSplashWatcher.
    /// </summary>
    /// <returns>������ ������</returns>
    SplashInfoPack GetSplashInfoPack();

    /// <summary>
    /// ����� ������.
    /// ����� ������ ����� ������, ��� ��������� ������ GetSplashInfoPack() ����� ���������� ������ ���������� � ����� ��������.
    /// ������������ ��� ����������� ������ ����� ������� ����������.
    /// </summary>
    void ResetSplashInfo();

    /// <summary>
    /// �������� ���������� ��������.
    /// ����� ������������� �������� ISplash.Cancelled=true, ���� � ����� ���� ������� ��������
    /// </summary>
    void Cancel();

    #endregion
  }

  /// <summary>
  /// �������������� ����� �������� �� ������� �������
  /// </summary>
  public sealed class ServerSplashWatcher : MarshalByRefObject, IServerSplashWatcher
  {
    #region �����������

    /// <summary>
    /// ������� �������������� ��� ��������� ����� ��������
    /// </summary>
    /// <param name="source">���� ��������</param>
    public ServerSplashWatcher(ISplashStack source)
    {
      if (source == null)
        throw new ArgumentNullException();

      _Source = source;

#if DEBUG_SPLASHWATCHERS

      _WatcherGuid = Guid.NewGuid();
      Trace.WriteLine("ServerSplashWatcher " + _WatcherGuid.ToString() + " created");
#endif
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������������� ����
    /// </summary>
    public ISplashStack Source { get { return _Source; } }
    private ISplashStack _Source;

#if DEBUG_SPLASHWATCHERS

    /// <summary>
    /// ��� ������� - ������������� �������.
    /// �������� ������������ �������� ��������������, ��� ������� ��������� � ������ AppDomain � ������ ���������
    /// </summary>
    private Guid _WatcherGuid;

    /// <summary>
    /// ��� ������� - ������� ������� GetSplashInfoPack()
    /// </summary>
    private int GetSplashInfoPackCount;

#endif

    #endregion

    #region IServerSplashWatcher Members

    /// <summary>
    /// ������������ ��� ������ ������ GetSplashStack()
    /// </summary>
    private int _PrevStackVersion;

    /// <summary>
    /// ����� ������� ���� ��� ���������� ������
    /// </summary>
    private int _PrevPhase;

    /// <summary>
    /// ���������� � ������� ����
    /// </summary>
    private SplashInfoPackCurrentPhase _PrevCurrentPhase;

    /// <summary>
    /// ������ �� ������� ��������.
    /// ������ ����������� �������� ISplashStack.Splash, �.�. ����� ������ GetSplashStack() ����� ���� ����������� ��������� � �����
    /// </summary>
    private ISplash _PrevSplash;

    /// <summary>
    /// �������� ����� - ��������� ������ ������
    /// </summary>
    /// <returns></returns>
    public SplashInfoPack GetSplashInfoPack()
    {
      SplashInfoPack Pack = DoGetSplashInfoPack();

#if DEBUG_SPLASHWATCHERS

      string sPack;
      if (Pack == null)
        sPack = "null";
      else
        sPack = Pack.ToString();

      unchecked { GetSplashInfoPackCount++; }

      if (Pack != null)
      {
        Pack.ServerWatcherGuid = _WatcherGuid;
        Pack.ServerCallId = GetSplashInfoPackCount;
      }

      Trace.WriteLine("ServerSplashWatcher " + _WatcherGuid.ToString() + ". GetSplashInfoPack() # " + GetSplashInfoPackCount.ToString() + ". Returning " + sPack);
#endif

      return Pack;
    }

    private SplashInfoPack DoGetSplashInfoPack()
    {
      ISplash[] stack = _Source.GetSplashStack(ref _PrevStackVersion);
      if (stack != null)
      {
        #region 1. ���������� ������ ���������� � ��������� �����

        SplashInfoPackStack res1 = new SplashInfoPackStack();

        res1.Stack = new SplashInfoPackStackItem[stack.Length];
        for (int i = 0; i < stack.Length; i++)
        {
          res1.Stack[i] = new SplashInfoPackStackItem();
          res1.Stack[i].Phases = stack[i].Phases;
          res1.Stack[i].StackSerialId = stack[i].StackSerialId;
          res1.Stack[i].PhaseStates = stack[i].GetPhaseStates();
          InitCurrentPhase(stack[i], res1.Stack[i]);
        }

        if (stack.Length > 0)
        {
          _PrevPhase = stack[0].Phase;
          _PrevCurrentPhase = res1.Stack[0];
          _PrevSplash = stack[0];
        }
        else
        {
          _PrevPhase = -1;
          _PrevCurrentPhase = null;
          _PrevSplash = null;
        }

        return res1;

        #endregion
      }

      // �� ���� ���������/������ �������� � ����������� ������
      if (_PrevSplash == null)
      {
        // �������� �� ���� � ���. ���������� ������
#if DEBUG_SPLASHWATCHERS
        return new SplashInfoPackNone();
#else
        return null;
#endif
      }

      if (_PrevSplash.Phase != _PrevPhase)
      {
        #region 2. ���������� ���������� � ������� ��������

        SplashInfoPackSplash res2 = new SplashInfoPackSplash();
        res2.PhaseStates = _PrevSplash.GetPhaseStates();
        InitCurrentPhase(_PrevSplash, res2);
        _PrevPhase = _PrevSplash.Phase;

        return res2;

        #endregion
      }

      #region 3. ���������� ��������� � ������� ���� ��� null

      SplashInfoPackCurrentPhase res3 = new SplashInfoPackCurrentPhase();
      InitCurrentPhase(_PrevSplash, res3);

      if (res3.PercentMax == _PrevCurrentPhase.PercentMax &&
        res3.Percent == _PrevCurrentPhase.Percent &&
        res3.AllowCancel == _PrevCurrentPhase.AllowCancel &&
        res3.PhaseText == _PrevCurrentPhase.PhaseText)

        // ������ �� ����������
#if DEBUG_SPLASHWATCHERS
        return new SplashInfoPackNone();
#else
        return null;
#endif
      else
      {
        // ���-�� ����������
        _PrevCurrentPhase = res3;
        return res3;
      }

      #endregion
    }

    private void InitCurrentPhase(ISplash src, SplashInfoPackCurrentPhase res)
    {
      res.PercentMax = src.PercentMax;
      res.Percent = src.Percent;
      res.AllowCancel = src.AllowCancel;
      res.PhaseText = src.PhaseText;
    }

    /// <summary>
    /// ����� ������ ����� ������, ��������� ����� GetSplashInfoPack() ������ ������ ���������� � ����� ��������.
    /// ����� ������������ ��� �������������� ����� ������ ���������� ������� � ��������.
    /// </summary>
    public void ResetSplashInfo()
    {
      _PrevStackVersion = 0;
    }

    /// <summary>
    /// ������������� �������� ISplash.Cancelled=true, ���� ���� �������� �� ������
    /// </summary>
    public void Cancel()
    {
      ISplash spl = _Source.Splash;
      if (spl != null)
        spl.Cancelled = true;
    }

    #endregion
  }

  /// <summary>
  /// ������ ������ ��� ServerSplashWatcher.
  /// ������������� ��������� �������� �������� �� �����, ���� ServerSplashWatcher �� ������������� ��������
  /// ���� ������ �� �������� ����������������. ��� ������ ������ ���������� �� ������ ������, � ������� 
  /// ��� ������ ����������� �������
  /// </summary>
  public sealed class ClientSplashWatcher
  {
    #region �����������

    /// <summary>
    /// ������� ������, ������� �������������� � ServerSplashWatcher ��� ��������� �� ���� ������ � ����������� ������ �������� �� ������� �������
    /// </summary>
    /// <param name="serverWatcher">��������� �����, ������������ ������</param>
    /// <param name="clientStack">���� �������� �� ������� �������, �������� ����� ���������</param>
    public ClientSplashWatcher(IServerSplashWatcher serverWatcher, ISplashStack clientStack)
    {
      if (serverWatcher == null)
        throw new ArgumentNullException("serverWatcher");
      if (clientStack == null)
        throw new ArgumentNullException("clientStack");
      _ServerWatcher = serverWatcher;
      _ClientStack = clientStack;

      _ClientSplashes = new Stack<ISplash>();

#if DEBUG_SPLASHWATCHERS
      _WatcherGuid = Guid.NewGuid();
      Trace.WriteLine("ClientSplashWatcher " + _WatcherGuid.ToString() + " created");
#endif

      _UseDefaultSplash = false;
      _DefaultSplashPhaseText = SplashTools.DefaultSplashText;

#if DEBUG
      _TheThread = Thread.CurrentThread;
#endif
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �����, ������������ ������
    /// </summary>
    public IServerSplashWatcher ServerWatcher { get { return _ServerWatcher; } }
    private IServerSplashWatcher _ServerWatcher;

    /// <summary>
    /// ���� �������� �� ������� �������, �������� ����� ���������
    /// </summary>
    public ISplashStack ClientStack { get { return _ClientStack; } }
    private ISplashStack _ClientStack;

#if DEBUG_SPLASHWATCHERS

    /// <summary>
    /// ��� ������� - ������������� �������
    /// </summary>
    private Guid _WatcherGuid;

    /// <summary>
    /// ������� ������� ProcessUpdateStack()
    /// </summary>
    private int ProcessUpdateStackCount;

#endif

    #endregion

    #region ����� ProcessSplash

    /// <summary>
    /// ���� ����� ������ ������������ ���������� �� �������.
    /// 1. �������� ServerSplashWatcher.GetSplashPackInfo() � ��������� ��������� �������� � ������� ISplashStack
    /// 2. ��������� ISplashStack.Cancelled �, ��� �������������, �������� ServerSplashWatcher.Cancel()
    /// </summary>
    public void ProcessSplash()
    {
#if DEBUG
      CheckThread();
#endif

      try
      {
        ProcessUpdateStack();
      }
      catch
      {
        ResetSplashInfo();
      }
      ShowDefaultSplash();
      ProcessCheckCancelled();
    }

    [DebuggerStepThrough]
    private void ResetSplashInfo()
    {
      try
      {
        _ServerWatcher.ResetSplashInfo();
      }
      catch { }
    }

    /// <summary>
    /// ������ ����, ����� ����������� �������� ������� ClientStack.GetSplashStack(), ������ ����� ������.
    /// ����� ��� �������� "���� �������".
    /// �������� �������� ��������� �� �������
    /// </summary>
    private Stack<ISplash> _ClientSplashes;

    private void ProcessUpdateStack()
    {
      SplashInfoPack pack = GetSplashInfoPack(); // ������ ������ �� ����

#if DEBUG_SPLASHWATCHERS

      if (pack == null)
        throw new NullReferenceException("� ���������� ������ SplashInfoPack �� ����� ���� null");

      string sPack = "From " + pack.ServerWatcherGuid + ", CallId=" + pack.ServerCallId + ". " + pack.ToString();

      unchecked { ProcessUpdateStackCount++; }

      // ����� ���� ������ ��������� � �������
      //if (ProcessUpdateStackCount != pack.ServerCallId)
      //  throw new BugException("SplashWatcher call missmatch. ServerCallId=" + pack.ServerCallId.ToString() + ", ClientCallId=" + ProcessUpdateStackCount.ToString());

      Trace.WriteLine("ClientSplashWatcher " + _WatcherGuid.ToString() + ". ProcessUpdateStack() # " + ProcessUpdateStackCount.ToString() + ". Recieved " + sPack);
#endif

      if (pack == null)
        return; // ��� ���������

      SplashInfoPackStack pack1 = pack as SplashInfoPackStack;
      if (pack1 != null)
      {
        // �������� ������ ���� �������� 

        // ������� ���� ��������
        if (pack1.Stack.Length > 0 && _DefaultSplash != null)
        {
          _ClientStack.EndSplash();
          _DefaultSplash = null;
        }

        // ������� �������� ��������, ������� ��� � �����
        // - �� ����������
        while (_ClientSplashes.Count > pack1.Stack.Length)
        {
          _ClientStack.EndSplash();
          _ClientSplashes.Pop();
        }

        // - �� ������������
        for (int i = (pack1.Stack.Length - _ClientSplashes.Count); i < pack1.Stack.Length; i++)
        {
          if (_ClientSplashes.Peek().StackSerialId == pack1.Stack[i].StackSerialId)
            break;
          else
          {
            _ClientStack.EndSplash();
            _ClientSplashes.Pop();
          }
        }

        // ��������� ����������� �������
        for (int i = pack1.Stack.Length - _ClientSplashes.Count - 1; i >= 0; i--)
        {
          ISplash spl = _ClientStack.BeginSplash(pack1.Stack[i].Phases);
          spl.StackSerialId = pack1.Stack[i].StackSerialId; // ��������������
          _ClientSplashes.Push(spl);
        }

        if (_ClientSplashes.Count != pack1.Stack.Length)
          throw new BugException("����������� ���������� ��������");

        ISplash[] ClientSplashes2 = _ClientSplashes.ToArray();
        // �������������� ��������� ���� ��������
        for (int i = 0; i < pack1.Stack.Length; i++)
          InitSplashStates(pack1.Stack[i], ClientSplashes2[i]);

        return;
      }

      SplashInfoPackSplash pack2 = pack as SplashInfoPackSplash;
      if (pack2 != null)
      {
        // �������� ���������� �� ���������� ����� ��������
        if (_ClientSplashes.Count == 0)
          throw new BugException("��������� SplashInfoPackSplash. � ������ _ClientSplashes ��� ��������");
        InitSplashStates(pack2, _ClientSplashes.Peek());
        return;
      }

      SplashInfoPackCurrentPhase pack3 = pack as SplashInfoPackCurrentPhase;
      if (pack3 != null)
      {
        // �������� ���������� �� ���������� ����������� ����������
        if (_ClientSplashes.Count == 0)
          throw new BugException("��������� SplashInfoPackCurrentPhase. � ������ _ClientSplashes ��� ��������");
        InitSplashCurrentPhase(pack3, _ClientSplashes.Peek());
        return;
      }

#if DEBUG_SPLASHWATCHERS
      if (pack is SplashInfoPackNone) // �������
        return;
#endif

      throw new BugException("������� ������ ������������ ���� " + pack.GetType().ToString());
    }

    /// <summary>
    /// ����� IServerSplashWatcher.GetSplashInfoPack().
    /// ����� ���������� ����������, ��������� � �����.
    /// � ���� ������, ���� ���������� �������� ���������� � ���������
    /// </summary>
    /// <returns></returns>
    [DebuggerStepThrough]
    private SplashInfoPack GetSplashInfoPack()
    {
      try
      {
        return _ServerWatcher.GetSplashInfoPack();
      }
      catch
      {
        try
        {
          _ServerWatcher.ResetSplashInfo();
        }
        catch { }
        return null;
      }
    }

    /// <summary>
    /// �������������� ������� ���� �������� � �������, ����������� � �������
    /// </summary>
    /// <param name="src">���������� � ������ ������� ���� ��������</param>
    /// <param name="dst">��������� ��������, ������� ��������� ����������������</param>
    private void InitSplashStates(SplashInfoPackSplash src, ISplash dst)
    {
      SplashPhaseState[] dstStates = dst.GetPhaseStates();
      if (dstStates.Length != src.PhaseStates.Length)
        throw new BugException("������ ���������� ��� � ��������");
      for (int i = 0; i < dst.PhaseCount; i++)
      {
        if (dstStates[i] != src.PhaseStates[i])
        {
          if (src.PhaseStates[i] == SplashPhaseState.Complete || src.PhaseStates[i] == SplashPhaseState.Skipped)
          {
            if (dstStates[i] == SplashPhaseState.Current || dstStates[i] == SplashPhaseState.None)
            {
              // ��������� ������� �� ��������� ����
              if (src.PhaseStates[i] == SplashPhaseState.Complete)
                dst.Complete();
              else
                dst.Skip();
            }
            else
              break;
          }
        }
      }
      InitSplashCurrentPhase(src, dst);
    }


    /// <summary>
    /// �������������� ��������� ���������� �������� � �������, ����������� � �������
    /// </summary>
    /// <param name="src">���������� � ������ ������� ���� ��������</param>
    /// <param name="dst">��������� ��������, ������� ��������� ����������������</param>
    private void InitSplashCurrentPhase(SplashInfoPackCurrentPhase src, ISplash dst)
    {
      if (dst.Phase >= dst.PhaseCount)
        return; // ����� ��������� ���� ���������, ��� ������ ����������� ������
      dst.SetPercent(src.Percent, src.PercentMax);
      dst.PhaseText = src.PhaseText;
      dst.AllowCancel = src.AllowCancel;
    }

    #endregion

    #region �������� DefaultSplash

    /// <summary>
    /// ����� �� �������� �������� "���� �������", ���� ServerSplashWatcher �� ���������� �� ����� ��������.
    /// �� ��������� - false - �������� �� ���������.
    /// ���� �������� ��������������� � true, �� ��� ������ ���� ���� �������� � false �� ��������� ������ ��������,
    /// ����� �������� �� ��������� ��������� �� ������.
    /// </summary>
    public bool UseDefaultSplash
    {
      get { return _UseDefaultSplash; }
      set
      {
#if DEBUG
        CheckThread();
#endif

        _UseDefaultSplash = value;
        if (!value) // 16.12.2019
          ShowDefaultSplash();
      }
    }
    private bool _UseDefaultSplash;

    /// <summary>
    /// �������� "���� �������", ���� ��� ���������� ��������
    /// </summary>
    private ISplash _DefaultSplash;

    /// <summary>
    /// ��������� �������� "���� �������"
    /// </summary>
    private void ShowDefaultSplash()
    {
      if (_UseDefaultSplash)
      {
        if (_ClientSplashes.Count == 0 && _DefaultSplash == null)
        {
          _DefaultSplash = _ClientStack.BeginSplash(DefaultSplashPhaseText);

#if DEBUG_SPLASHWATCHERS
        Trace.WriteLine("ClientSplashWatcher " + _WatcherGuid.ToString() + ". Dummy splash created");
#endif
        }
      }
      else
      {
        if (_DefaultSplash != null)
        {
          _ClientStack.EndSplash();
          _DefaultSplash = null;
        }
      }
    }

    /// <summary>
    /// ����� ��������, ��������� �� ��������� ��� UseDefaultSplash=true.
    /// </summary>
    public string DefaultSplashPhaseText
    {
      get { return _DefaultSplashPhaseText; }
      set
      {
#if DEBUG
        CheckThread();
#endif

        if (value == null)
          value = SplashTools.DefaultSplashText;
        if (value == _DefaultSplashPhaseText)
          return;

        _DefaultSplashPhaseText = value;
        if (_DefaultSplash != null)
          _DefaultSplash.PhaseText = value;
      }
    }
    private string _DefaultSplashPhaseText;

    #endregion

    #region ������� ������ "������"

    /// <summary>
    /// ��������������� � true, ���� ��� ������ ������ Cancel
    /// </summary>
    private bool _CancelSignalled;

    /// <summary>
    /// �������� ������� ������ "������"
    /// </summary>
    private void ProcessCheckCancelled()
    {
      ISplash spl = _ClientStack.Splash;
      if (spl == null)
        return;

      if (spl.AllowCancel && spl.Cancelled)
      {
        if (_CancelSignalled)
          return; // �������������� ��������� ������� �������

        // �������� ������
        _ServerWatcher.Cancel();
        _CancelSignalled = true;
      }
      else
        _CancelSignalled = false;
    }

    #endregion

    #region ������� �����

    /// <summary>
    /// ��������� ����� ����������� EndSplash().
    /// ����� ������� ����� ������, ���������, ������� ���������� UseDefaultSplash=false
    /// </summary>
    public void ClearClientStack()
    {
#if DEBUG
      CheckThread();
#endif

      while (_ClientSplashes.Count > 0)
      {
        _ClientStack.EndSplash();
        _ClientSplashes.Pop();
      }
      ShowDefaultSplash();
    }

    #endregion

    #region �������� ������

#if DEBUG

    private Thread _TheThread;

    private void CheckThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _TheThread))
        throw new DifferentThreadException(_TheThread);
    }

#endif

    #endregion
  }
}