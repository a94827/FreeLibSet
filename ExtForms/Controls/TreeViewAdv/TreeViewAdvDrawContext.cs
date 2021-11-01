﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

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

namespace FreeLibSet.Controls
{
  public struct TreeViewAdvDrawContext
  {
    private Graphics _graphics;
    public Graphics Graphics
    {
      get { return _graphics; }
      set { _graphics = value; }
    }

    private Rectangle _bounds;
    public Rectangle Bounds
    {
      get { return _bounds; }
      set { _bounds = value; }
    }

    private Font _font;
    public Font Font
    {
      get { return _font; }
      set { _font = value; }
    }

    private TreeViewAdvDrawSelectionMode _drawSelection;
    public TreeViewAdvDrawSelectionMode DrawSelection
    {
      get { return _drawSelection; }
      set { _drawSelection = value; }
    }

    private bool _drawFocus;
    public bool DrawFocus
    {
      get { return _drawFocus; }
      set { _drawFocus = value; }
    }

    private NodeControl _currentEditorOwner;
    public NodeControl CurrentEditorOwner
    {
      get { return _currentEditorOwner; }
      set { _currentEditorOwner = value; }
    }

    private bool _enabled;
    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }
  }
}