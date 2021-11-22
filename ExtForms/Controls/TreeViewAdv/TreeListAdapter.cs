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
using FreeLibSet.Models.Tree;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
	/// <summary>
	/// Converts IEnumerable interface to ITreeModel. 
	/// Allows to display a plain list in the TreeView
	/// </summary>
	public class TreeListAdapter : ITreeModel
	{
		private System.Collections.IEnumerable _list;

		public TreeListAdapter(System.Collections.IEnumerable list)
		{
			_list = list;
		}

		#region ITreeModel Members

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			if (treePath.IsEmpty())
				return _list;
			else
				return null;
		}

		public bool IsLeaf(TreePath treePath)
		{
			return true;
		}

		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public void OnNodesChanged(TreeModelEventArgs args)
		{
			if (NodesChanged != null)
				NodesChanged(this, args);
		}

		public event EventHandler<TreePathEventArgs> StructureChanged;
		public void OnStructureChanged(TreePathEventArgs args)
		{
			if (StructureChanged != null)
				StructureChanged(this, args);
		}

		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public void OnNodeInserted(TreeModelEventArgs args)
		{
			if (NodesInserted != null)
				NodesInserted(this, args);
		}

		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public void OnNodeRemoved(TreeModelEventArgs args)
		{
			if (NodesRemoved != null)
				NodesRemoved(this, args);
		}

		#endregion
	}
}
