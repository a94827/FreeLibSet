// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class NodeStateIcon : NodeIcon
  {
    private Image _leaf;
    private Image _opened;
    private Image _closed;

    public NodeStateIcon()
    {
      _leaf = MakeTransparent(TreeViewAdvRes.TreeViewAdvResources.Item);
      _opened = MakeTransparent(TreeViewAdvRes.TreeViewAdvResources.TreeViewOpenFolder);
      _closed = MakeTransparent(TreeViewAdvRes.TreeViewAdvResources.TreeViewClosedFolder);
    }

    private static Image MakeTransparent(Bitmap bitmap)
    {
      bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
      return bitmap;
    }

    protected override Image GetIcon(TreeNodeAdv node)
    {
      Image icon = base.GetIcon(node);
      if (icon != null)
        return icon;
      else if (node.IsLeaf)
        return _leaf;
      else if (node.CanExpand && node.IsExpanded)
        return _opened;
      else
        return _closed;
    }
  }
}
