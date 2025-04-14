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
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;
using FreeLibSet.Core;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Базовый класс для всех отображаемых элементов просмотра <see cref="TreeViewAdv"/>.
  /// Наследуется классами <see cref="BindableControl"/> (основные элементы, отображающие даннные модели), <see cref="NodePlusMinus"/>, <see cref="NodeStateIcon"/> (не напрямую).
  /// При <see cref="TreeViewAdv.UseColumns"/>=true элементы отображаются внутри столбцов <see cref="TreeColumn"/>. Обычно в столбце располагается один элемент <see cref="BindableControl"/>,
  /// а в первом столбце - также <see cref="NodeStateIcon"/>, <see cref="NodePlusMinus"/> и <see cref="NodeCheckBox"/> (флажки для отметки строк).
  /// При <see cref="TreeViewAdv.UseColumns"/>=false элементы отображаются непосредственно в строке, без использования <see cref="TreeColumn"/>.
  /// 
  /// Для разных строк элементы могут иметь разные размеры. За прорисовку и определение размеров элемента отвечают классы-наследники.
  /// </summary>
  [DesignTimeVisible(false), ToolboxItem(false)]
  public abstract class NodeControl : Component
  {
    #region Properties

    /// <summary>
    /// Просмотр, к которому добавлен элемент.
    /// Установка свойства присоединяет элемент к коллекции <see cref="TreeViewAdv.NodeControls"/>.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TreeViewAdv Parent
    {
      get { return _parent; }
      set
      {
        if (value != _parent)
        {
          if (_parent != null)
            _parent.NodeControls.Remove(this);

          if (value != null)
            value.NodeControls.Add(this);
        }
      }
    }
    private TreeViewAdv _parent;

    /// <summary>
    /// Генератор всплывающих подсказок для элемента.
    /// Если null (по умолчанию), то подсказки не показываются.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ITreeViewAdvToolTipProvider ToolTipProvider
    {
      get { return _toolTipProvider; }
      set { _toolTipProvider = value; }
    }
    private ITreeViewAdvToolTipProvider _toolTipProvider;

    /// <summary>
    /// Столбец, к которому присоединен элемент в режиме <see cref="TreeViewAdv.UseColumns"/>=true. В этом режиме свойство должно быть обязательно установлено,
    /// иначе элемент отображаться не будет.
    /// В пределах столбца элементы, если их несколько, отображаются в том порядке, в котором они расположены в коллекции <see cref="TreeViewAdv.NodeControls"/>.
    /// </summary>
    [DefaultValue(null)]
    public TreeColumn ParentColumn
    {
      get { return _parentColumn; }
      set
      {
        _parentColumn = value;
        if (_parent != null)
          _parent.FullUpdate();
      }
    }
    private TreeColumn _parentColumn;

    /// <summary>
    /// Вертикальное выравнивание для элемента в пределах высоты строки данных <see cref="TreeNodeAdv.Height"/>. По умолчанию - по центру.
    /// </summary>
    [DefaultValue(VerticalAlignment.Center)]
    public VerticalAlignment VerticalAlign
    {
      get { return _verticalAlign; }
      set
      {
        _verticalAlign = value;
        if (_parent != null)
          _parent.FullUpdate();
      }
    }
    private VerticalAlignment _verticalAlign = VerticalAlignment.Center;

    /// <summary>
    /// Отступ от предыдущего элемента в пикселях. По умолчанию - 0.
    /// </summary>
    public int LeftMargin
    {
      get { return _leftMargin; }
      set
      {
        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        _leftMargin = value;
        if (_parent != null)
          _parent.FullUpdate();
      }
    }
    private int _leftMargin = 0;

    #endregion

    #region Методы

    internal virtual void AssignParent(TreeViewAdv parent)
    {
      _parent = parent;
    }

    protected virtual Rectangle GetBounds(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Rectangle r = context.Bounds;
      Size s = GetActualSize(node, context);
      Size bs = new Size(r.Width - LeftMargin, Math.Min(r.Height, s.Height));
      switch (VerticalAlign)
      {
        case VerticalAlignment.Top:
          return new Rectangle(new Point(r.X + LeftMargin, r.Y), bs);
        case VerticalAlignment.Bottom:
          return new Rectangle(new Point(r.X + LeftMargin, r.Bottom - s.Height), bs);
        default:
          return new Rectangle(new Point(r.X + LeftMargin, r.Y + (r.Height - s.Height) / 2), bs);
      }
    }

    protected void CheckThread()
    {
      if (Parent != null && Control.CheckForIllegalCrossThreadCalls)
        if (Parent.InvokeRequired)
          throw new InvalidOperationException("Cross-thread calls are not allowed");
    }

    /// <summary>
    /// Возвращает true, если элемент должен быть отображен для текущей строки данных. Если false, то элементы, находящиеся справа от этого элемента (в пределах
    /// того же <see cref="TreeColumn"/> при <see cref="TreeViewAdv.UseColumns"/>=true) сдвигаются влево.
    /// Не влияет на оторажения столбца, которое задается свойством <see cref="TreeColumn.IsVisible"/>. Если столбец скрыт, то входящие в него элементы не отображаются.
    /// 
    /// Для получения значения используется событие <see cref="IsVisibleValueNeeded"/>.
    /// </summary>
    /// <param name="node">Узел дерева, к которому относится строка</param>
    /// <returns>Признак видимости</returns>
    public bool IsVisible(TreeNodeAdv node)
    {
      NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
      args.Value = true;
      OnIsVisibleValueNeeded(args);
      return Convert.ToBoolean(args.Value);
    }

    internal Size GetActualSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      if (IsVisible(node))
      {
        Size s = MeasureSize(node, context);
        return new Size(s.Width + LeftMargin, s.Height);
      }
      else
        return Size.Empty;
    }

    public abstract Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context);

    public abstract void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context);

    /// <summary>
    /// Возвращает строку всплывающе подсказки, используя <see cref="ToolTipProvider"/>.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual string GetToolTip(TreeNodeAdv node)
    {
      if (ToolTipProvider != null)
        return ToolTipProvider.GetToolTip(node, this);
      else
        return string.Empty;
    }

    public virtual void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
    }

    public virtual void MouseUp(TreeNodeAdvMouseEventArgs args)
    {
    }

    public virtual void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
    }

    public virtual void KeyDown(KeyEventArgs args)
    {
    }

    public virtual void KeyUp(KeyEventArgs args)
    {
    }

    /// <summary>
    /// Определение видимости элемента для текущей строки.
    /// Событие вызывается методом <see cref="IsVisible(TreeNodeAdv)"/>.
    /// </summary>
    public event EventHandler<NodeControlValueEventArgs> IsVisibleValueNeeded;

    /// <summary>
    /// Вызывает событие <see cref="IsVisibleValueNeeded"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnIsVisibleValueNeeded(NodeControlValueEventArgs args)
    {
      if (IsVisibleValueNeeded != null)
        IsVisibleValueNeeded(this, args);
    }

    #endregion
  }
}
