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

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Редактируемая часть иерахического просмотра.
  /// Наследуется классом <see cref="BaseTextControl"/>.
  /// </summary>
  public abstract class EditableControl : InteractiveControl
  {
    #region Защищенный конструктор и Dispose()

    /// <summary>
    /// Создает элемент
    /// </summary>
    protected EditableControl()
    {
      _timer = new Timer();
      _timer.Interval = 500;
      _timer.Tick += new EventHandler(TimerTick);
    }

    /// <summary>
    /// Удаляет используемые объекты
    /// </summary>
    /// <param name="disposing">true, если вызван <see cref="IDisposable.Dispose()"/>, а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
        _timer.Dispose();
    }

    #endregion

    #region Properties

    private readonly Timer _timer;
    private bool _editFlag;

    /// <summary>
    /// Если true, то редактирование начинается по одинарному щелчку левой кнопки мыши
    /// </summary>
    [DefaultValue(false)]
    public bool EditOnClick
    {
      get { return _editOnClick; }
      set { _editOnClick = value; }
    }
    private bool _editOnClick = false;

    #endregion

    private void TimerTick(object sender, EventArgs args)
    {
      _timer.Stop();
      if (_editFlag)
        BeginEdit();
      _editFlag = false;
    }
    
    /// <summary>
    /// Вычисляет границы редактора текста
    /// </summary>
    /// <param name="context">Контекст для редактора</param>
    public void SetEditorBounds(TreeViewAdvEditorContext context)
    {
      Size size = CalculateEditorSize(context);
      context.Editor.Bounds = new Rectangle(context.Bounds.X, context.Bounds.Y,
        Math.Min(size.Width, context.Bounds.Width),
        Math.Min(size.Height, Parent.ClientSize.Height - context.Bounds.Y)
      );
    }

    /// <summary>
    /// Возвращает желаемые размеры редактора
    /// </summary>
    /// <param name="context">Контекст для редактора</param>
    /// <returns>Размеры</returns>
    protected abstract Size CalculateEditorSize(TreeViewAdvEditorContext context);

    /// <summary>
    /// Возвращает true, если редактирование доступно для узла дерева
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Возможность редактирования</returns>
    protected virtual bool CanEdit(TreeNodeAdv node)
    {
      return (node.Tag != null) && IsEditEnabled(node);
    }

    /// <summary>
    /// Начать редактирование узла <see cref="TreeViewAdv.CurrentNode"/>
    /// </summary>
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

    /// <summary>
    /// Закончить редактирование
    /// </summary>
    /// <param name="applyChanges">true, если изменения должны быть записаны</param>
    public void EndEdit(bool applyChanges)
    {
      if (Parent != null)
        if (Parent.HideEditor(applyChanges))
          OnEditorHided();
    }

    /// <summary>
    /// Вызывается методом <see cref="DisposeEditor(Control)"/> перед показом элемента редактора
    /// </summary>
    /// <param name="control">Управляющий элемент редактора</param>
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

    /// <summary>
    /// Перенос данных из управляющего элемента редактора в узел дерева
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <param name="editor">Управляющий элемент редактора</param>
    protected abstract void DoApplyChanges(TreeNodeAdv node, Control editor);

    /// <summary>
    /// Создать управляющий элемент редактора
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Новый <see cref="System.Windows.Forms.Control"/></returns>
    protected abstract Control CreateEditor(TreeNodeAdv node);

    /// <summary>
    /// Удалить управляющий элемент редактора
    /// </summary>
    /// <param name="editor">Управляющий элемент редактора</param>
    protected abstract void DisposeEditor(Control editor);

    /// <summary>
    /// Выполнить команду "Вырезать"
    /// </summary>
    /// <param name="control">Управляющий элемент редактора</param>
    public virtual void Cut(Control control)
    {
    }

    /// <summary>
    /// Выполнить команду "Копировать"
    /// </summary>
    /// <param name="control">Управляющий элемент редактора</param>
    public virtual void Copy(Control control)
    {
    }

    /// <summary>
    /// Выполнить команду "Вставить"
    /// </summary>
    /// <param name="control">Управляющий элемент редактора</param>
    public virtual void Paste(Control control)
    {
    }

    /// <summary>
    /// Удалить выделенный текст в элементе.
    /// Не используется.
    /// </summary>
    /// <param name="control">Управляющий элемент редактора</param>
    public virtual void Delete(Control control)
    {
    }

    /// <summary>
    /// Обработка события мыши
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public override void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
      _editFlag = (!EditOnClick && args.Button == MouseButtons.Left
        && args.ModifierKeys == Keys.None && args.Node.IsSelected);
    }

    /// <summary>
    /// Обработка события мыши.
    /// Начинает редактирование ячейки, если свойство <see cref="EditOnClick"/> установлено.
    /// </summary>
    /// <param name="args">Аргументы события</param>
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

    /// <summary>
    /// Обработка события мыши
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public override void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
      _editFlag = false;
      _timer.Stop();
    }

    #region Events

    /// <summary>
    /// Вызывается перед началом редактирования ячейки.
    /// Обработчик может отменить редактирование, установив <see cref="CancelEventArgs.Cancel"/>=true.
    /// </summary>
    public event CancelEventHandler EditorShowing;

    /// <summary>
    /// Вызывает событие <see cref="EditorShowing"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected void OnEditorShowing(CancelEventArgs args)
    {
      if (EditorShowing != null)
        EditorShowing(this, args);
    }

    /// <summary>
    /// Вызывается при окончании редактирования ячейки
    /// </summary>
    public event EventHandler EditorHided;

    /// <summary>
    /// Вызывает событие <see cref="EditorHided"/>
    /// </summary>
    protected void OnEditorHided()
    {
      if (EditorHided != null)
        EditorHided(this, EventArgs.Empty);
    }

    /// <summary>
    /// Вызывается при переносе данных из управляющего элемента в узел дерева
    /// </summary>
    public event EventHandler ChangesApplied;

    /// <summary>
    /// Вызывает событие <see cref="ChangesApplied"/>
    /// </summary>
    protected void OnChangesApplied()
    {
      if (ChangesApplied != null)
        ChangesApplied(this, EventArgs.Empty);
    }

    #endregion
  }
}
