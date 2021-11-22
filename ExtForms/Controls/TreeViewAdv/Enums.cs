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

#pragma warning disable 1591


namespace FreeLibSet.Controls
{
  public enum TreeViewAdvDrawSelectionMode
  {
    None, Active, Inactive, FullRowSelect
  }

  public enum TreeViewAdvSelectionMode
  {
    Single, Multi, MultiSameParent
  }

  public enum TreeViewAdvNodePosition
  {
    Inside, Before, After
  }

#if XXX
  public enum VerticalAlignment
  {
    Top, Bottom, Center
  }
#endif

  public enum TreeViewAdvIncrementalSearchMode
  {
    None, Standard, Continuous
  }

  [Flags]
  public enum TreeViewAdvGridLineStyle
  {
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    HorizontalAndVertical = 3
  }

  public enum TreeViewAdvImageScaleMode
  {
    /// <summary>
    /// Don't scale
    /// </summary>
    Clip,
    /// <summary>
    /// Scales image to fit the display rectangle, aspect ratio is not fixed.
    /// </summary>
    Fit,
    /// <summary>
    /// Scales image down if it is larger than display rectangle, taking aspect ratio into account
    /// </summary>
    ScaleDown,
    /// <summary>
    /// Scales image up if it is smaller than display rectangle, taking aspect ratio into account
    /// </summary>
    ScaleUp,
    /// <summary>
    /// Scales image to match the display rectangle, taking aspect ratio into account
    /// </summary>
    AlwaysScale,

  }
}
