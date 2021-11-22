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
using System.ComponentModel.Design;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  public class StringCollectionEditor : CollectionEditor
  {
    public StringCollectionEditor(Type type)
      : base(type)
    {
    }

    protected override Type CreateCollectionItemType()
    {
      return typeof(string);
    }

    protected override object CreateInstance(Type itemType)
    {
      return "";
    }
  }
}
