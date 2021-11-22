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
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class NodeNumericUpDown : BaseTextControl
  {
    #region Properties

    private int _editorWidth = 100;
    [DefaultValue(100)]
    public int EditorWidth
    {
      get { return _editorWidth; }
      set { _editorWidth = value; }
    }

    private int _decimalPlaces = 0;
    [Category("Data"), DefaultValue(0)]
    public int DecimalPlaces
    {
      get
      {
        return this._decimalPlaces;
      }
      set
      {
        this._decimalPlaces = value;
      }
    }

    private decimal _increment = 1;
    [Category("Data"), DefaultValue(1)]
    public decimal Increment
    {
      get
      {
        return this._increment;
      }
      set
      {
        this._increment = value;
      }
    }

    private decimal _minimum = 0;
    [Category("Data"), DefaultValue(0)]
    public decimal Minimum
    {
      get
      {
        return _minimum;
      }
      set
      {
        _minimum = value;
      }
    }

    private decimal _maximum = 100;
    [Category("Data"), DefaultValue(100)]
    public decimal Maximum
    {
      get
      {
        return this._maximum;
      }
      set
      {
        this._maximum = value;
      }
    }

    #endregion

    public NodeNumericUpDown()
    {
    }

    protected override Size CalculateEditorSize(TreeViewAdvEditorContext context)
    {
      if (Parent.UseColumns)
        return context.Bounds.Size;
      else
        return new Size(EditorWidth, context.Bounds.Height);
    }

    protected override Control CreateEditor(TreeNodeAdv node)
    {
      FreeLibSet.Controls.DecimalEditBox num = new FreeLibSet.Controls.DecimalEditBox();
      num.Increment = Increment;
      num.DecimalPlaces = DecimalPlaces;
      num.Minimum = Minimum;
      num.Maximum = Maximum;
      num.Value = (decimal)GetValue(node);
      SetEditControlProperties(num, node);
      return num;
    }

    protected override void DisposeEditor(Control editor)
    {
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as FreeLibSet.Controls.DecimalEditBox).Value);
    }
  }
}
