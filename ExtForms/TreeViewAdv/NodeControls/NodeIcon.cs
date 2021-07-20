using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
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

namespace AgeyevAV.ExtForms.NodeControls
{
  public class NodeIcon : BindableControl
  {
    public NodeIcon()
    {
      LeftMargin = 1;
    }

    public override Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Image image = GetIcon(node);
      if (image != null)
        return image.Size;
      else
        return Size.Empty;
    }


    public override void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Image image = GetIcon(node);
      if (image != null)
      {
        Rectangle r = GetBounds(node, context);
        if (image.Width > 0 && image.Height > 0)
        {
          switch (_scaleMode)
          {
            case TreeViewAdvImageScaleMode.Fit:
              context.Graphics.DrawImage(image, r);
              break;
            case TreeViewAdvImageScaleMode.ScaleDown:
              {
                float factor = Math.Min((float)r.Width / (float)image.Width, (float)r.Height / (float)image.Height);
                if (factor < 1)
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width * factor, image.Height * factor);
                else
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width, image.Height);
              } break;
            case TreeViewAdvImageScaleMode.ScaleUp:
              {
                float factor = Math.Max((float)r.Width / (float)image.Width, (float)r.Height / (float)image.Height);
                if (factor > 1)
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width * factor, image.Height * factor);
                else
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width, image.Height);
              } break;
            case TreeViewAdvImageScaleMode.AlwaysScale:
              {
                float fx = (float)r.Width / (float)image.Width;
                float fy = (float)r.Height / (float)image.Height;
                if (Math.Min(fx, fy) < 1)
                { //scale down
                  float factor = Math.Min(fx, fy);
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width * factor, image.Height * factor);
                }
                else if (Math.Max(fx, fy) > 1)
                {
                  float factor = Math.Max(fx, fy);
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width * factor, image.Height * factor);
                }
                else
                  context.Graphics.DrawImage(image, r.X, r.Y, image.Width, image.Height);
              } break;
            case TreeViewAdvImageScaleMode.Clip:
            default:
              context.Graphics.DrawImage(image, r.X, r.Y, image.Width, image.Height);
              break;
          }
        }

      }
    }

    protected virtual Image GetIcon(TreeNodeAdv node)
    {
      return GetValue(node) as Image;
    }

    private TreeViewAdvImageScaleMode _scaleMode = TreeViewAdvImageScaleMode.Clip;
    [DefaultValue("Clip"), Category("Appearance")]
    public TreeViewAdvImageScaleMode ScaleMode
    {
      get { return _scaleMode; }
      set { _scaleMode = value; }
    }


  }
}
