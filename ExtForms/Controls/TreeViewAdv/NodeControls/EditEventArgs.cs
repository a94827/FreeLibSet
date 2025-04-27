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

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Аргументы события <see cref="NodeComboBox.CreatingEditor"/>
  /// </summary>
  public class EditEventArgs : NodeEventArgs
  {
    /// <summary>
    /// Используется <see cref="TreeViewAdv"/>
    /// </summary>
    /// <param name="node"></param>
    /// <param name="control"></param>
    public EditEventArgs(TreeNodeAdv node, Control control)
      : base(node)
    {
      _control = control;
    }

    /// <summary>
    /// Управляющий элемент редактора
    /// </summary>
    public Control Control
    {
      get { return _control; }
    }
    private readonly Control _control;
  }
}
