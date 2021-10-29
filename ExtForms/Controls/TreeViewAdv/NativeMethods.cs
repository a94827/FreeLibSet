using System;
using System.Drawing;
using System.Runtime.InteropServices;

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



namespace FreeLibSet.Controls
{
  internal static class NativeMethods
  {
    public const int DCX_WINDOW = 0x01;
    public const int DCX_CACHE = 0x02;
    public const int DCX_NORESETATTRS = 0x04;
    public const int DCX_CLIPCHILDREN = 0x08;
    public const int DCX_CLIPSIBLINGS = 0x10;
    public const int DCX_PARENTCLIP = 0x20;
    public const int DCX_EXCLUDERGN = 0x40;
    public const int DCX_INTERSECTRGN = 0x80;
    public const int DCX_EXCLUDEUPDATE = 0x100;
    public const int DCX_INTERSECTUPDATE = 0x200;
    public const int DCX_LOCKWINDOWUPDATE = 0x400;
    public const int DCX_VALIDATE = 0x200000;

    public const int WM_THEMECHANGED = 0x031A;
    public const int WM_NCPAINT = 0x85;
    public const int WM_NCCALCSIZE = 0x83;

    public const int WS_BORDER = 0x800000;
    public const int WS_EX_CLIENTEDGE = 0x200;

    public const int WVR_HREDRAW = 0x100;
    public const int WVR_VREDRAW = 0x200;
    public const int WVR_REDRAW = (WVR_HREDRAW | WVR_VREDRAW);

    [StructLayout(LayoutKind.Sequential)]
    public struct NCCALCSIZE_PARAMS
    {
      public RECT rgrc0, rgrc1, rgrc2;
      public IntPtr lppos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;

      public static RECT FromRectangle(Rectangle rectangle)
      {
        RECT result = new RECT();
        result.Left = rectangle.Left;
        result.Top = rectangle.Top;
        result.Right = rectangle.Right;
        result.Bottom = rectangle.Bottom;
        return result;
      }

      public Rectangle ToRectangle()
      {
        return new Rectangle(Left, Top, Right - Left, Bottom - Top);
      }
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  }
}
