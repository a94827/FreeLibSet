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
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  internal static class TextHelper
  {
    public static StringAlignment TranslateAligment(HorizontalAlignment alignment)
    {
      if (alignment == HorizontalAlignment.Left)
        return StringAlignment.Near;
      else if (alignment == HorizontalAlignment.Right)
        return StringAlignment.Far;
      else
        return StringAlignment.Center;
    }

    public static TextFormatFlags TranslateAligmentToFlag(HorizontalAlignment alignment)
    {
      if (alignment == HorizontalAlignment.Left)
        return TextFormatFlags.Left;
      else if (alignment == HorizontalAlignment.Right)
        return TextFormatFlags.Right;
      else
        return TextFormatFlags.HorizontalCenter;
    }

    public static TextFormatFlags TranslateTrimmingToFlag(StringTrimming trimming)
    {
      if (trimming == StringTrimming.EllipsisCharacter)
        return TextFormatFlags.EndEllipsis;
      else if (trimming == StringTrimming.EllipsisPath)
        return TextFormatFlags.PathEllipsis;
      if (trimming == StringTrimming.EllipsisWord)
        return TextFormatFlags.WordEllipsis;
      if (trimming == StringTrimming.Word)
        return TextFormatFlags.WordBreak;
      else
        return TextFormatFlags.Default;
    }
  }
}
