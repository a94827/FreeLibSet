using System;
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
      FreeLibSet.Controls.ExtNumericUpDown num = new FreeLibSet.Controls.ExtNumericUpDown();
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
      SetValue(node, (editor as FreeLibSet.Controls.ExtNumericUpDown).Value);
    }
  }
}
