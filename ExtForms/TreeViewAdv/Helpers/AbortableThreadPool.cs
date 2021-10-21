// Stephen Toub
// stoub@microsoft.com

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
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
 * Original TreeViewAdv component from Aga.Controls.dll
 * http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
 * http://sourceforge.net/projects/treeviewadv/
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FreeLibSet.Controls
{
  internal enum WorkItemStatus
  {
    Completed,
    Queued,
    Executing,
    Aborted
  }

  internal sealed class WorkItem
  {
    private WaitCallback _callback;
    private object _state;
    private ExecutionContext _ctx;

    internal WorkItem(WaitCallback wc, object state, ExecutionContext ctx)
    {
      _callback = wc;
      _state = state;
      _ctx = ctx;
    }

    internal WaitCallback Callback
    {
      get
      {
        return _callback;
      }
    }

    internal object State
    {
      get
      {
        return _state;
      }
    }

    internal ExecutionContext Context
    {
      get
      {
        return _ctx;
      }
    }
  }

  internal class AbortableThreadPool
  {
    private LinkedList<WorkItem> _callbacks = new LinkedList<WorkItem>();
    private Dictionary<WorkItem, Thread> _threads = new Dictionary<WorkItem, Thread>();

    public WorkItem QueueUserWorkItem(WaitCallback callback)
    {
      return QueueUserWorkItem(callback, null);
    }

    public WorkItem QueueUserWorkItem(WaitCallback callback, object state)
    {
      if (callback == null) throw new ArgumentNullException("callback");

      WorkItem item = new WorkItem(callback, state, ExecutionContext.Capture());
      lock (_callbacks)
      {
        _callbacks.AddLast(item);
      }
      ThreadPool.QueueUserWorkItem(new WaitCallback(HandleItem));
      return item;
    }

    private void HandleItem(object ignored)
    {
      WorkItem item = null;
      try
      {
        lock (_callbacks)
        {
          if (_callbacks.Count > 0)
          {
            item = _callbacks.First.Value;
            _callbacks.RemoveFirst();
          }
          if (item == null)
            return;
          _threads.Add(item, Thread.CurrentThread);

        }
        ExecutionContext.Run(item.Context,
          delegate { item.Callback(item.State); }, null);
      }
      finally
      {
        lock (_callbacks)
        {
          if (item != null)
            _threads.Remove(item);
        }
      }
    }

    public bool IsMyThread(Thread thread)
    {
      lock (_callbacks)
      {
        foreach (Thread t in _threads.Values)
        {
          if (t == thread)
            return true;
        }
        return false;
      }
    }

    public WorkItemStatus Cancel(WorkItem item, bool allowAbort)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      lock (_callbacks)
      {
        LinkedListNode<WorkItem> node = _callbacks.Find(item);
        if (node != null)
        {
          _callbacks.Remove(node);
          return WorkItemStatus.Queued;
        }
        else if (_threads.ContainsKey(item))
        {
          if (allowAbort)
          {
            _threads[item].Abort();
            _threads.Remove(item);
            return WorkItemStatus.Aborted;
          }
          else
            return WorkItemStatus.Executing;
        }
        else
          return WorkItemStatus.Completed;
      }
    }

    public void CancelAll(bool allowAbort)
    {
      lock (_callbacks)
      {
        _callbacks.Clear();
        if (allowAbort)
        {
          foreach (Thread t in _threads.Values)
            t.Abort();
        }
      }
    }
  }
}
