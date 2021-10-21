using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

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


namespace FreeLibSet.Controls
{
  internal static class ResourceHelper
  {
    // VSpilt Cursor with Innerline (symbolisize hidden column)
    private static Cursor _dVSplitCursor = GetCursor(TreeViewAdvRes.TreeViewAdvResources.DVSplit);
    public static Cursor DVSplitCursor
    {
      get { return _dVSplitCursor; }
    }

    private static GifDecoder _loadingIcon = GetGifDecoder(TreeViewAdvRes.TreeViewAdvResources.loading_icon);
    public static GifDecoder LoadingIcon
    {
      get { return _loadingIcon; }
    }

    /// <summary>
    /// Help function to convert byte[] from resource into Cursor Type 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static Cursor GetCursor(byte[] data)
    {
      using (MemoryStream s = new MemoryStream(data))
        return new Cursor(s);
    }

    /// <summary>
    /// Help function to convert byte[] from resource into GifDecoder Type 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static GifDecoder GetGifDecoder(byte[] data)
    {
      using (MemoryStream ms = new MemoryStream(data))
        return new GifDecoder(ms, true);
    }

  }
}
