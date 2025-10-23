// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Drawing;

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  internal static class ResourceHelper
  {
    // VSpilt Cursor with Innerline (symbolisize hidden column)
    private static readonly Cursor _dVSplitCursor = GetCursor(TreeViewAdvRes.TreeViewAdvResources.DVSplit);
    public static Cursor DVSplitCursor
    {
      get { return _dVSplitCursor; }
    }

    private static readonly GifDecoder _loadingIcon = GetGifDecoder(TreeViewAdvRes.TreeViewAdvResources.loading_icon);
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
