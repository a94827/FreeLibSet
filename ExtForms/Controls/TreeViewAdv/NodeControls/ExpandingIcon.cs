using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

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

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Displays an animated icon for those nodes, who are in expanding state. 
  /// Parent TreeView must have AsyncExpanding property set to true.
  /// </summary>
  public class ExpandingIcon : NodeControl
  {
    private static GifDecoder _gif = ResourceHelper.LoadingIcon;
    private static int _index = 0;
    private static volatile Thread _animatingThread;
    private static object _lock = new object();

    public override Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      return ResourceHelper.LoadingIcon.FrameSize;
    }

    protected override void OnIsVisibleValueNeeded(NodeControlValueEventArgs args)
    {
      args.Value = args.Node.IsExpandingNow;
      base.OnIsVisibleValueNeeded(args);
    }

    public override void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Rectangle rect = GetBounds(node, context);
      Image img = _gif.GetFrame(_index).Image;
      context.Graphics.DrawImage(img, rect.Location);
    }

    public static void Start()
    {
      lock (_lock)
      {
        if (_animatingThread == null)
        {
          _index = 0;
          _animatingThread = new Thread(new ThreadStart(IterateIcons));
          _animatingThread.IsBackground = true;
          _animatingThread.Priority = ThreadPriority.Lowest;
          _animatingThread.Start();
        }
      }
    }

    public static void Stop()
    {
      lock (_lock)
      {
        _index = 0;
        _animatingThread = null;
      }
    }

    private static void IterateIcons()
    {
      while (_animatingThread != null)
      {
        if (_index < _gif.FrameCount - 1)
          _index++;
        else
          _index = 0;

        if (IconChanged != null)
          IconChanged(null, EventArgs.Empty);

        int delay = _gif.GetFrame(_index).Delay;
        Thread.Sleep(delay);
      }
      System.Diagnostics.Debug.WriteLine("IterateIcons Stopped");
    }

    public static event EventHandler IconChanged;
  }
}
