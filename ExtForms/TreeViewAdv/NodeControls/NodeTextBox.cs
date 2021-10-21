using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;

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

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class NodeTextBox : BaseTextControl
  {
    private const int MinTextBoxWidth = 30;

    public NodeTextBox()
    {
    }

    protected override Size CalculateEditorSize(TreeViewAdvEditorContext context)
    {
      if (Parent.UseColumns)
        return context.Bounds.Size;
      else
      {
        Size size = GetLabelSize(context.CurrentNode, context.DrawContext, _label);
        int width = Math.Max(size.Width + Font.Height, MinTextBoxWidth); // reserve a place for new typed character
        return new Size(width, size.Height);
      }
    }

    public override void KeyDown(KeyEventArgs args)
    {
      if (args.KeyCode == Keys.F2 && Parent.CurrentNode != null && EditEnabled)
      {
        args.Handled = true;
        BeginEdit();
      }
    }

    protected override Control CreateEditor(TreeNodeAdv node)
    {
      TextBox textBox = CreateTextBox();
      textBox.TextAlign = TextAlign;
      textBox.Text = GetLabel(node);
      textBox.BorderStyle = BorderStyle.FixedSingle;
      textBox.TextChanged += EditorTextChanged;
      textBox.KeyDown += EditorKeyDown;
      _label = textBox.Text;
      SetEditControlProperties(textBox, node);
      return textBox;
    }

    protected virtual TextBox CreateTextBox()
    {
      return new TextBox();
    }

    protected override void DisposeEditor(Control editor)
    {
      TextBox textBox = editor as TextBox;
      textBox.TextChanged -= EditorTextChanged;
      textBox.KeyDown -= EditorKeyDown;
    }

    private void EditorKeyDown(object sender, KeyEventArgs args)
    {
      if (args.KeyCode == Keys.Escape)
        EndEdit(false);
      else if (args.KeyCode == Keys.Enter)
        EndEdit(true);
    }

    private string _label;

    private void EditorTextChanged(object sender, EventArgs args)
    {
      TextBox textBox = sender as TextBox;
      _label = textBox.Text;
      Parent.UpdateEditorBounds();
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      string label = (editor as TextBox).Text;
      string oldLabel = GetLabel(node);
      if (oldLabel != label)
      {
        SetLabel(node, label);
        OnLabelChanged(node.Tag, oldLabel, label);
      }
    }

    public override void Cut(Control control)
    {
      (control as TextBox).Cut();
    }

    public override void Copy(Control control)
    {
      (control as TextBox).Copy();
    }

    public override void Paste(Control control)
    {
      (control as TextBox).Paste();
    }

    public override void Delete(Control control)
    {
      TextBox textBox = control as TextBox;
      int len = Math.Max(textBox.SelectionLength, 1);
      if (textBox.SelectionStart < textBox.Text.Length)
      {
        int start = textBox.SelectionStart;
        textBox.Text = textBox.Text.Remove(textBox.SelectionStart, len);
        textBox.SelectionStart = start;
      }
    }

    public event EventHandler<LabelEventArgs> LabelChanged;
    protected void OnLabelChanged(object subject, string oldLabel, string newLabel)
    {
      if (LabelChanged != null)
        LabelChanged(this, new LabelEventArgs(subject, oldLabel, newLabel));
    }
  }

  public class LabelEventArgs : EventArgs
  {
    private object _subject;
    public object Subject
    {
      get { return _subject; }
    }

    private string _oldLabel;
    public string OldLabel
    {
      get { return _oldLabel; }
    }

    private string _newLabel;
    public string NewLabel
    {
      get { return _newLabel; }
    }

    public LabelEventArgs(object subject, string oldLabel, string newLabel)
    {
      _subject = subject;
      _oldLabel = oldLabel;
      _newLabel = newLabel;
    }
  }

}
