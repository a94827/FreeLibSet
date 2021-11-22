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
using System.ComponentModel;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
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
