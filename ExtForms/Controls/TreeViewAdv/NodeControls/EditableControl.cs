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

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Редактируемая часть иерахического просмотра.
  /// Наследуется классом <see cref="BaseTextControl"/>.
  /// </summary>
  public abstract class EditableControl : InteractiveControl
  {
    private Timer _timer;
    private bool _editFlag;

    #region Properties

    private bool _editOnClick = false;
    [DefaultValue(false)]
    public bool EditOnClick
    {
      get { return _editOnClick; }
      set { _editOnClick = value; }
    }

    #endregion

    protected EditableControl()
    {
      _timer = new Timer();
      _timer.Interval = 500;
      _timer.Tick += new EventHandler(TimerTick);
    }

    private void TimerTick(object sender, EventArgs args)
    {
      _timer.Stop();
      if (_editFlag)
        BeginEdit();
      _editFlag = false;
    }

    public void SetEditorBounds(TreeViewAdvEditorContext context)
    {
      Size size = CalculateEditorSize(context);
      context.Editor.Bounds = new Rectangle(context.Bounds.X, context.Bounds.Y,
        Math.Min(size.Width, context.Bounds.Width),
        Math.Min(size.Height, Parent.ClientSize.Height - context.Bounds.Y)
      );
    }

    protected abstract Size CalculateEditorSize(TreeViewAdvEditorContext context);

    protected virtual bool CanEdit(TreeNodeAdv node)
    {
      return (node.Tag != null) && IsEditEnabled(node);
    }

    public void BeginEdit()
    {
      if (Parent != null && Parent.CurrentNode != null && CanEdit(Parent.CurrentNode))
      {
        CancelEventArgs args = new CancelEventArgs();
        OnEditorShowing(args);
        if (!args.Cancel)
        {
          Control editor = CreateEditor(Parent.CurrentNode);
          Parent.DisplayEditor(editor, this);
        }
      }
    }

    public void EndEdit(bool applyChanges)
    {
      if (Parent != null)
        if (Parent.HideEditor(applyChanges))
          OnEditorHided();
    }

    public virtual void UpdateEditor(Control control)
    {
    }

    internal void ApplyChanges(TreeNodeAdv node, Control editor)
    {
      DoApplyChanges(node, editor);
      OnChangesApplied();
    }

    internal void DoDisposeEditor(Control editor)
    {
      DisposeEditor(editor);
    }

    protected abstract void DoApplyChanges(TreeNodeAdv node, Control editor);

    protected abstract Control CreateEditor(TreeNodeAdv node);

    protected abstract void DisposeEditor(Control editor);

    public virtual void Cut(Control control)
    {
    }

    public virtual void Copy(Control control)
    {
    }

    public virtual void Paste(Control control)
    {
    }

    public virtual void Delete(Control control)
    {
    }

    public override void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
      _editFlag = (!EditOnClick && args.Button == MouseButtons.Left
        && args.ModifierKeys == Keys.None && args.Node.IsSelected);
    }

    public override void MouseUp(TreeNodeAdvMouseEventArgs args)
    {
      if (args.Node.IsSelected)
      {
        if (EditOnClick && args.Button == MouseButtons.Left && args.ModifierKeys == Keys.None)
        {
          Parent.ItemDragMode = false;
          BeginEdit();
          args.Handled = true;
        }
        else if (_editFlag)// && args.Node.IsSelected)
          _timer.Start();
      }
    }

    public override void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
      _editFlag = false;
      _timer.Stop();
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
        _timer.Dispose();
    }

    #region Events

    public event CancelEventHandler EditorShowing;
    protected void OnEditorShowing(CancelEventArgs args)
    {
      if (EditorShowing != null)
        EditorShowing(this, args);
    }

    public event EventHandler EditorHided;
    protected void OnEditorHided()
    {
      if (EditorHided != null)
        EditorHided(this, EventArgs.Empty);
    }

    public event EventHandler ChangesApplied;
    protected void OnChangesApplied()
    {
      if (ChangesApplied != null)
        ChangesApplied(this, EventArgs.Empty);
    }

    #endregion
  }
}
