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
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using FreeLibSet.Models.Tree;

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Элемент просмотра "флажок". Поддерживает флажки с двумя и тремя состояниями.
  /// </summary>
  public class NodeCheckBox : InteractiveControl
  {
    /// <summary>
    /// Высота и ширина значка
    /// </summary>
    public const int ImageSize = 13;

    private Bitmap _check;
    private Bitmap _uncheck;
    private Bitmap _unknown;

    #region Properties

    /// <summary>
    /// Если установлено в true, то разрешается выбор "промежуточного" состояния флажка.
    /// </summary>
    [DefaultValue(false)]
    public bool ThreeState
    {
      get { return _threeState; }
      set { _threeState = value; }
    }
    private bool _threeState;

    #endregion

    /// <summary>
    /// Создает элемент, не привязанный к свойству в наборе данных
    /// </summary>
    public NodeCheckBox()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// Создает элемент с возможностью привязки к свойству в наборе данных
    /// </summary>
    /// <param name="propertyName">Имя свойства</param>
    public NodeCheckBox(string propertyName)
    {
      _check = TreeViewAdvRes.TreeViewAdvResources.check;
      _uncheck = TreeViewAdvRes.TreeViewAdvResources.uncheck;
      _unknown = TreeViewAdvRes.TreeViewAdvResources.unknown;
      DataPropertyName = propertyName;
      LeftMargin = 0;
    }

    /// <summary>
    /// Возвращает константный размер элемента <see cref="ImageSize"/>.
    /// </summary>
    /// <param name="node">Не используется</param>
    /// <param name="context">Не используется</param>
    /// <returns>Размер</returns>
    public override Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      return new Size(ImageSize, ImageSize);
    }

    /// <summary>
    /// Выполняет рисование флажка
    /// </summary>
    /// <param name="node">Текущий узел</param>
    /// <param name="context">Контекст рисования</param>
    public override void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Rectangle bounds = GetBounds(node, context);
      int dx = (bounds.Width - ImageSize) / 2;
      int dy = (bounds.Height - ImageSize) / 2;
      Rectangle bounds2 = new Rectangle(bounds.X + dx, bounds.Y + dy, ImageSize, ImageSize);
      CheckState state = GetCheckState(node);

      if (Application.RenderWithVisualStyles)
      {
        VisualStyleRenderer renderer;
        if (state == CheckState.Indeterminate)
          renderer = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.MixedNormal);
        else if (state == CheckState.Checked)
          renderer = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.CheckedNormal);
        else
          renderer = new VisualStyleRenderer(VisualStyleElement.Button.CheckBox.UncheckedNormal);
        renderer.DrawBackground(context.Graphics, new Rectangle(bounds.X, bounds.Y, ImageSize, ImageSize));
      }
      else
      {
        Image img;
        if (state == CheckState.Indeterminate)
          img = _unknown;
        else if (state == CheckState.Checked)
          img = _check;
        else
          img = _uncheck;
        //context.Graphics.DrawImage(img, bounds.Location);
        context.Graphics.DrawImage(img, bounds2); // 09.02.2023
      }
    }

    /// <summary>
    /// Возвращает состояние флажка для узла с помощью вызова GetValue().
    /// </summary>
    /// <param name="node">Узел</param>
    /// <returns>Состояние флажка</returns>
    protected virtual CheckState GetCheckState(TreeNodeAdv node)
    {
      object obj = GetValue(node);
      if (obj is CheckState)
        return (CheckState)obj;
      else if (obj is bool)
        return (bool)obj ? CheckState.Checked : CheckState.Unchecked;
      else
        return CheckState.Unchecked;
    }

    /// <summary>
    /// Устанавливает состояние флажка для узла с помощью вызова SetValue().
    /// </summary>
    /// <param name="node">Узел</param>
    /// <param name="value">Состояние флажка</param>
    protected virtual void SetCheckState(TreeNodeAdv node, CheckState value)
    {
      if (VirtualMode)
      {
        SetValue(node, value);
        OnCheckStateChanged(node);
      }
      else
      {
        Type type = GetPropertyType(node);
        if (type == typeof(CheckState))
        {
          SetValue(node, value);
          OnCheckStateChanged(node);
        }
        else if (type == typeof(bool))
        {
          SetValue(node, value != CheckState.Unchecked);
          OnCheckStateChanged(node);
        }
      }
    }

    /// <summary>
    /// Обработка нажатия кнопки мыши
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public override void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
      if (args.Button == MouseButtons.Left && IsEditEnabled(args.Node))
      {
        TreeViewAdvDrawContext context = new TreeViewAdvDrawContext();
        context.Bounds = args.ControlBounds;
        Rectangle rect = GetBounds(args.Node, context);
        if (rect.Contains(args.ViewLocation))
        {
          CheckState state = GetCheckState(args.Node);
          state = GetNewState(state);
          SetCheckState(args.Node, state);
          Parent.UpdateView();
          args.Handled = true;
        }
      }
    }

    /// <summary>
    /// Обработка двойного нажатия кнопки мыши
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public override void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
      args.Handled = true;
    }

    private CheckState GetNewState(CheckState state)
    {
      if (state == CheckState.Indeterminate)
        return CheckState.Unchecked;
      else if (state == CheckState.Unchecked)
        return CheckState.Checked;
      else
        return ThreeState ? CheckState.Indeterminate : CheckState.Unchecked;
    }

    /// <summary>
    /// Обработка нажатия клавиши
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public override void KeyDown(KeyEventArgs args)
    {
      if (args.KeyCode == Keys.Space && EditEnabled)
      {
        Parent.BeginUpdate();
        try
        {
          if (Parent.CurrentNode != null)
          {
            CheckState value = GetNewState(GetCheckState(Parent.CurrentNode));
            foreach (TreeNodeAdv node in Parent.Selection)
              if (IsEditEnabled(node))
                SetCheckState(node, value);
          }
        }
        finally
        {
          Parent.EndUpdate();
        }
        args.Handled = true;
      }
    }

    /// <summary>
    /// Событие вызывается после изменения состояния флажка
    /// </summary>
    public event EventHandler<TreePathEventArgs> CheckStateChanged;

    /// <summary>
    /// Вызывает обработчик события <see cref="CheckStateChanged"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected void OnCheckStateChanged(TreePathEventArgs args)
    {
      if (CheckStateChanged != null)
        CheckStateChanged(this, args);
    }

    /// <summary>
    /// Вызывает обработчик события <see cref="CheckStateChanged"/>
    /// </summary>
    /// <param name="node">Узел дерева</param>
    protected void OnCheckStateChanged(TreeNodeAdv node)
    {
      TreePath path = this.Parent.GetPath(node);
      OnCheckStateChanged(new TreePathEventArgs(path));
    }
  }
}
