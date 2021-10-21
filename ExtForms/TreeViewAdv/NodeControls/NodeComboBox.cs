﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;

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
  public class NodeComboBox : BaseTextControl
  {
    #region Properties

    private int _editorWidth = 100;
    [DefaultValue(100)]
    public int EditorWidth
    {
      get { return _editorWidth; }
      set { _editorWidth = value; }
    }

    private int _editorHeight = 100;
    [DefaultValue(100)]
    public int EditorHeight
    {
      get { return _editorHeight; }
      set { _editorHeight = value; }
    }

    private List<object> _dropDownItems;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [Editor(typeof(StringCollectionEditor), typeof(UITypeEditor)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<object> DropDownItems
    {
      get { return _dropDownItems; }
    }

    #endregion

    public event EventHandler<EditEventArgs> CreatingEditor;

    public NodeComboBox()
    {
      _dropDownItems = new List<object>();
    }

    protected override Size CalculateEditorSize(TreeViewAdvEditorContext context)
    {
      if (Parent.UseColumns)
      {
        if (context.Editor is CheckedListBox)
          return new Size(context.Bounds.Size.Width, EditorHeight);
        else
          return context.Bounds.Size;
      }
      else
      {
        if (context.Editor is CheckedListBox)
          return new Size(EditorWidth, EditorHeight);
        else
          return new Size(EditorWidth, context.Bounds.Height);
      }
    }

    protected override Control CreateEditor(TreeNodeAdv node)
    {
      Control c;
      object value = GetValue(node);
      if (IsCheckedListBoxRequired(node))
        c = CreateCheckedListBox(node);
      else
        c = CreateCombo(node);
      OnCreatingEditor(new EditEventArgs(node, c));
      return c;
    }

    protected override void DisposeEditor(Control editor)
    {
    }

    protected virtual void OnCreatingEditor(EditEventArgs args)
    {
      if (CreatingEditor != null)
        CreatingEditor(this, args);
    }

    protected virtual bool IsCheckedListBoxRequired(TreeNodeAdv node)
    {
      object value = GetValue(node);
      if (value != null)
      {
        Type t = value.GetType();
        object[] arr = t.GetCustomAttributes(typeof(FlagsAttribute), false);
        return (t.IsEnum && arr.Length == 1);
      }
      return false;
    }

    private Control CreateCombo(TreeNodeAdv node)
    {
      ComboBox comboBox = new ComboBox();
      if (DropDownItems != null)
        comboBox.Items.AddRange(DropDownItems.ToArray());
      comboBox.SelectedItem = GetValue(node);
      comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBox.DropDownClosed += new EventHandler(EditorDropDownClosed);
      SetEditControlProperties(comboBox, node);
      return comboBox;
    }

    private Control CreateCheckedListBox(TreeNodeAdv node)
    {
      CheckedListBox listBox = new CheckedListBox();
      listBox.CheckOnClick = true;

      object value = GetValue(node);
      Type enumType = GetEnumType(node);
      foreach (object obj in Enum.GetValues(enumType))
      {
        object[] attributes = enumType.GetField(obj.ToString()).GetCustomAttributes(typeof(BrowsableAttribute), false);
        if (attributes.Length == 0 || ((BrowsableAttribute)attributes[0]).Browsable)
          listBox.Items.Add(obj, IsContain(value, obj));
      }

      SetEditControlProperties(listBox, node);
      if (CreatingEditor != null)
        CreatingEditor(this, new EditEventArgs(node, listBox));
      return listBox;
    }

    protected virtual Type GetEnumType(TreeNodeAdv node)
    {
      object value = GetValue(node);
      return value.GetType();
    }

    private bool IsContain(object value, object enumElement)
    {
      if (value == null || enumElement == null)
        return false;
      if (value.GetType().IsEnum)
      {
        int i1 = (int)value;
        int i2 = (int)enumElement;
        return (i1 & i2) == i2;
      }
      else
      {
        object[] arr = value as object[];
        foreach (object obj in arr)
          if ((int)obj == (int)enumElement)
            return true;
        return false;
      }
    }

    protected override string FormatLabel(object obj)
    {
      object[] arr = obj as object[];
      if (arr != null)
      {
        StringBuilder sb = new StringBuilder();
        foreach (object t in arr)
        {
          if (sb.Length > 0)
            sb.Append(", ");
          sb.Append(t);
        }
        return sb.ToString();
      }
      else
        return base.FormatLabel(obj);
    }

    void EditorDropDownClosed(object sender, EventArgs args)
    {
      EndEdit(true);
    }

    public override void UpdateEditor(Control control)
    {
      if (control is ComboBox)
        (control as ComboBox).DroppedDown = true;
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      ComboBox combo = editor as ComboBox;
      if (combo != null)
      {
        if (combo.DropDownStyle == ComboBoxStyle.DropDown)
          SetValue(node, combo.Text);
        else
          SetValue(node, combo.SelectedItem);
      }
      else
      {
        CheckedListBox listBox = editor as CheckedListBox;
        Type type = GetEnumType(node);
        if (IsFlags(type))
        {
          int res = 0;
          foreach (object obj in listBox.CheckedItems)
            res |= (int)obj;
          object val = Enum.ToObject(type, res);
          SetValue(node, val);
        }
        else
        {
          List<object> list = new List<object>();
          foreach (object obj in listBox.CheckedItems)
            list.Add(obj);
          SetValue(node, list.ToArray());
        }
      }
    }

    private bool IsFlags(Type type)
    {
      object[] atr = type.GetCustomAttributes(typeof(FlagsAttribute), false);
      return atr.Length == 1;
    }

    public override void MouseUp(TreeNodeAdvMouseEventArgs args)
    {
      if (args.Node != null && args.Node.IsSelected) //Workaround of specific ComboBox control behavior
        base.MouseUp(args);
    }
  }
}
