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

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Элемент для прорисовки значка дерева.
  /// По умолчанию показываются изображения для открытой и закрытой папки, а для узлов-"листьев" - значок "Item".
  /// Для изменения значков можно установить свойства объекта или добавить обработчик события <see cref="BindableControl.ValueNeeded"/>,
  /// установив <see cref="BindableControl.VirtualMode"/>.
  /// </summary>
  public class NodeStateIcon : NodeIcon
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public NodeStateIcon()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значок для узлов - "листьев".
    /// По умолчанию - значок "Item", который визуально отличается от обычных узлов.
    /// Чтобы отображать узлы листья как обычные узлы (как в программе Windows Explorer или RegEdit),
    /// установите свойство равным <see cref="ClosedImage"/>.
    /// </summary>
    public Image LeafImage
    {
      get { return _LeafImage ?? TreeViewAdvRes.TreeViewAdvResources.Item; }
      set { _LeafImage = value; }
    }
    private Image _LeafImage;

    /// <summary>
    /// Значок для открытых узлов
    /// </summary>
    public Image OpenedImage
    {
      get { return _OpenedImage ?? TreeViewAdvRes.TreeViewAdvResources.TreeViewOpenFolder; }
      set { _OpenedImage = value; }
    }
    private Image _OpenedImage;

    /// <summary>
    /// Значок для обычных значков, не-листьев
    /// </summary>
    public Image ClosedImage
    {
      get { return _ClosedImage ?? TreeViewAdvRes.TreeViewAdvResources.TreeViewClosedFolder; }
      set { _ClosedImage = value; }
    }
    private Image _ClosedImage;

    ///// <summary>
    ///// Если true (по умолчанию), то для узлов дерева, которые являются "листьями" (<see cref="TreeNodeAdv.IsLeaf"/>=true),
    ///// используется отдельный значок.
    ///// Если установить в false, то будет использоваться значок как для обычных узлов (открытая или закрытая папка), 
    ///// как в дереве папок в Проводнике Windows, или в дереве Редактора Реестра.
    ///// </summary>
    //public bool UseLeafImage
    //{
    //  get { return _UseLeafImage; }
    //  set { _UseLeafImage = value; }
    //}
    //private bool _UseLeafImage;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает изображение <see cref="LeafImage"/>, <see cref="OpenedImage"/> или <see cref="ClosedImage"/>,
    /// если не задано использование пользовательского изображения.
    /// </summary>
    /// <param name="node">Узел дерева, для которого требуется значок</param>
    /// <returns>Значок</returns>
    protected override Image GetIcon(TreeNodeAdv node)
    {
      Image icon = base.GetIcon(node);
      if (icon != null)
        return icon;
      else if (node.IsLeaf)
        return LeafImage;
      else if (node.CanExpand && node.IsExpanded)
        return OpenedImage;
      else
        return ClosedImage;
    }

    #endregion
  }
}
