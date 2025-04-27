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
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Drawing;

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  /// <summary>
  /// Описание положения одной "ячейки" иерархического просмотра <see cref="TreeViewAdv"/>
  /// </summary>
  public struct NodeControlInfo
  {
    /// <summary>
    /// Инициализирует структуру
    /// </summary>
    /// <param name="control">Ссылка на элемент <see cref="NodeControl"/></param>
    /// <param name="bounds">Границы "ячейки"</param>
    /// <param name="node">Узел в иерархии</param>
    public NodeControlInfo(NodeControl control, Rectangle bounds, TreeNodeAdv node)
    {
      _control = control;
      _bounds = bounds;
      _node = node;
    }

    /// <summary>
    /// Ссылка на элемент <see cref="NodeControl"/>
    /// </summary>
    public NodeControl Control { get { return _control; } }
    private readonly NodeControl _control;

    /// <summary>
    /// Границы "ячейки"
    /// </summary>
    public Rectangle Bounds { get { return _bounds; } }
    private readonly Rectangle _bounds;

    /// <summary>
    /// Узел в иерархии
    /// </summary>
    public TreeNodeAdv Node { get { return _node; } }
    private readonly TreeNodeAdv _node;

    /// <summary>
    /// Экзмпляр неинициализированной структуры
    /// </summary>
    public static readonly NodeControlInfo Empty = new NodeControlInfo(null, Rectangle.Empty, null);
  }
}
