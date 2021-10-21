using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using FreeLibSet.Models.Tree;

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
  public class NodeCheckBox : InteractiveControl
  {
    public const int ImageSize = 13;

    private Bitmap _check;
    private Bitmap _uncheck;
    private Bitmap _unknown;

    #region Properties

    private bool _threeState;
    [DefaultValue(false)]
    public bool ThreeState
    {
      get { return _threeState; }
      set { _threeState = value; }
    }

    #endregion

    public NodeCheckBox()
      : this(string.Empty)
    {
    }

    public NodeCheckBox(string propertyName)
    {
      _check = TreeViewAdvRes.TreeViewAdvResources.check;
      _uncheck = TreeViewAdvRes.TreeViewAdvResources.uncheck;
      _unknown = TreeViewAdvRes.TreeViewAdvResources.unknown;
      DataPropertyName = propertyName;
      LeftMargin = 0;
    }

    public override Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      return new Size(ImageSize, ImageSize);
    }

    public override void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Rectangle bounds = GetBounds(node, context);
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
        context.Graphics.DrawImage(img, bounds.Location);
      }
    }

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

    public event EventHandler<TreePathEventArgs> CheckStateChanged;
    protected void OnCheckStateChanged(TreePathEventArgs args)
    {
      if (CheckStateChanged != null)
        CheckStateChanged(this, args);
    }

    protected void OnCheckStateChanged(TreeNodeAdv node)
    {
      TreePath path = this.Parent.GetPath(node);
      OnCheckStateChanged(new TreePathEventArgs(path));
    }

  }
}
