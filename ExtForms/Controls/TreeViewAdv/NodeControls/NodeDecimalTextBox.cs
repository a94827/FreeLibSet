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

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class NodeDecimalTextBox : NodeTextBox
  {
    private bool _allowDecimalSeparator = true;
    [DefaultValue(true)]
    public bool AllowDecimalSeparator
    {
      get { return _allowDecimalSeparator; }
      set { _allowDecimalSeparator = value; }
    }

    private bool _allowNegativeSign = true;
    [DefaultValue(true)]
    public bool AllowNegativeSign
    {
      get { return _allowNegativeSign; }
      set { _allowNegativeSign = value; }
    }

    protected NodeDecimalTextBox()
    {
    }

    protected override TextBox CreateTextBox()
    {
      NumericTextBox textBox = new NumericTextBox();
      textBox.AllowDecimalSeparator = AllowDecimalSeparator;
      textBox.AllowNegativeSign = AllowNegativeSign;
      return textBox;
    }

    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as NumericTextBox).DecimalValue);
    }
  }
}
