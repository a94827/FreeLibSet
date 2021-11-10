using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Drawing;
using FreeLibSet.Controls.TreeViewAdvInternal;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Original TreeViewAdv component from Aga.Controls.dll
 * http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
 * http://sourceforge.net/projects/treeviewadv/
 */

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  partial class TreeViewAdv
  {
    private TreeNodeAdv _editingNode;

    public EditableControl CurrentEditorOwner
    {
      get { return _CurrentEditorOwner; }
      private set { _CurrentEditorOwner = value; }
    }
    private EditableControl _CurrentEditorOwner;

    public Control CurrentEditor
    {
      get { return _CurrentEditor; }
      private set { _CurrentEditor = value; }
    }
    private Control _CurrentEditor;

    public void HideEditor()
    {
      if (CurrentEditorOwner != null)
        CurrentEditorOwner.EndEdit(false);
    }

    internal void DisplayEditor(Control editor, EditableControl owner)
    {
      if (editor == null || owner == null || CurrentNode == null)
        throw new ArgumentNullException();

      HideEditor(false);

      CurrentEditor = editor;
      CurrentEditorOwner = owner;
      _editingNode = CurrentNode;

      editor.Validating += EditorValidating;
      UpdateEditorBounds();
      UpdateView();
      editor.Parent = this;
      editor.Focus();
      owner.UpdateEditor(editor);
    }

    internal bool HideEditor(bool applyChanges)
    {
      if (CurrentEditor != null)
      {
        if (applyChanges)
        {
          if (!ApplyChanges())
            return false;
        }

        //Check once more if editor was closed in ApplyChanges
        if (CurrentEditor != null)
        {
          CurrentEditor.Validating -= EditorValidating;
          CurrentEditorOwner.DoDisposeEditor(CurrentEditor);

          CurrentEditor.Parent = null;
          CurrentEditor.Dispose();

          CurrentEditor = null;
          CurrentEditorOwner = null;
          _editingNode = null;
        }
      }
      return true;
    }

    private bool ApplyChanges()
    {
      try
      {
        CurrentEditorOwner.ApplyChanges(_editingNode, CurrentEditor);
        _errorProvider.Clear();
        return true;
      }
      catch (ArgumentException ex)
      {
        _errorProvider.SetError(CurrentEditor, ex.Message);
        /*CurrentEditor.Validating -= EditorValidating;
        MessageBox.Show(this, ex.Message, "Value is not valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        CurrentEditor.Focus();
        CurrentEditor.Validating += EditorValidating;*/
        return false;
      }
    }

    void EditorValidating(object sender, System.ComponentModel.CancelEventArgs e)
    {
      e.Cancel = !ApplyChanges();
    }

    public void UpdateEditorBounds()
    {
      if (CurrentEditor != null)
      {
        TreeViewAdvEditorContext context = new TreeViewAdvEditorContext();
        context.Owner = CurrentEditorOwner;
        context.CurrentNode = CurrentNode;
        context.Editor = CurrentEditor;
        context.DrawContext = _measureContext;
        SetEditorBounds(context);
      }
    }

    private void SetEditorBounds(TreeViewAdvEditorContext context)
    {
      foreach (NodeControlInfo info in GetNodeControls(context.CurrentNode))
      {
        if (context.Owner == info.Control && info.Control is EditableControl)
        {
          Point p = info.Bounds.Location;
          p.X += info.Control.LeftMargin;
          p.X -= OffsetX;
          p.Y -= (_rowLayout.GetRowBounds(FirstVisibleRow).Y - ColumnHeaderHeight);
          int width = DisplayRectangle.Width - p.X;
          if (UseColumns && info.Control.ParentColumn != null && Columns.Contains(info.Control.ParentColumn))
          {
            Rectangle rect = GetColumnBounds(info.Control.ParentColumn.Index);
            width = rect.Right - OffsetX - p.X;
          }
          context.Bounds = new Rectangle(p.X, p.Y, width, info.Bounds.Height);
          ((EditableControl)info.Control).SetEditorBounds(context);
          return;
        }
      }
    }

    private Rectangle GetColumnBounds(int column)
    {
      int x = 0;
      for (int i = 0; i < Columns.Count; i++)
      {
        if (Columns[i].IsVisible)
        {
          if (i < column)
            x += Columns[i].Width;
          else
            return new Rectangle(x, 0, Columns[i].Width, 0);
        }
      }
      return Rectangle.Empty;
    }
  }
}
