﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

#pragma warning disable 1591

namespace FreeLibSet.Forms
{
  #region Перечисление ZoomDirection

  /// <summary>
  /// Режим увеличения или уменьшения масштаба
  /// </summary>
  public enum ZoomDirection
  {
    /// <summary>
    /// Увеличение масштаба ("приблизить")
    /// </summary>
    ZoomIn,

    /// <summary>
    /// Уменьшение масштаба ("отдалить")
    /// </summary>
    ZoomOut
  }

  #endregion

  #region Делегаты

  public class ZoomDirectionEventArgs : EventArgs
  {
    #region Конструктор

    public ZoomDirectionEventArgs(ZoomDirection direction)
    {
      _Direction = direction;
    }

    #endregion

    #region Свойства

    public ZoomDirection Direction { get { return _Direction; } }
    private ZoomDirection _Direction;

    #endregion
  }

  public delegate void ZoomDirectionEventHandler(object sender,
    ZoomDirectionEventArgs args);

  #endregion

  /// <summary>
  /// Расширение класса управляющего элемента просмотра изображения
  /// Добавляет прокрутку изображения с помощью колесика мыши и сдвиг при нажатой
  /// левой кнопке
  /// </summary>
  [Description("Расширение класса просмотра изображения с расширенной обработкой мыши")]
  [ToolboxBitmap(typeof(ExtPrintPreviewControl), "ExtPrintPreviewControl.bmp")]
  [ToolboxItem(true)]
  public class ExtPrintPreviewControl : PrintPreviewControl
  {
    #region Константы из WinUser.h

    const int WM_VSCROLL = 0x0115;
    const int WM_HSCROLL = 0x0114;
    const int WM_MOUSEWHEEL = 0x020A;

    const int WHEEL_DELTA = 120;

    const int SB_LINEUP = 0;
    const int SB_LINELEFT = 0;
    const int SB_LINEDOWN = 1;
    const int SB_LINERIGHT = 1;
    const int SB_PAGEUP = 2;
    const int SB_PAGEDOWN = 3;

    #endregion

    #region Конструктор

    public ExtPrintPreviewControl()
    {
    }

    #endregion

    #region Внутренние поля

    /// <summary>
    /// Аккумулятор вращения колесика мыши
    /// </summary>
    private int _WheelAcc = 0;

    /// <summary>
    /// Признак режима прокрутки изображения при нажатой левой кнопки мыши
    /// </summary>
    private bool _InMouseMove = false;

    /// <summary>
    /// Предыдущая позиция в режиме InMouseMove
    /// </summary>
    private Point _StartMouse;

    #endregion

    #region Курсорные клавиши

    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
      // Чтобы работали горячие клавиши локального меню, необходимо обрабатывать 
      // нажатие клавиш здесь, т.к. событие KeyDown почему-то не вызывается
      if (!(args.KeyCode == Keys.ControlKey || args.KeyCode == Keys.ShiftKey ||
        args.KeyCode == Keys.Menu))
      {
        if (EFPCommandItems.PerformShortCutKey(args.KeyData))
        {
          args.IsInputKey = false;
          return;
        }
      }
      if (args.KeyCode == Keys.Up || args.KeyCode == Keys.Down ||
        args.KeyCode == Keys.Left || args.KeyCode == Keys.Right)
      {
        // Нажатие с Shift'ом обрабатываем для ускорения прокрутки
        if (args.Modifiers == Keys.Shift)
        {
          Message msg;
          switch (args.KeyCode)
          {
            case Keys.Up:
              msg = Message.Create(Handle, WM_VSCROLL,
                (IntPtr)(SB_PAGEUP), (IntPtr)0);
              break;
            case Keys.Down:
              msg = Message.Create(Handle, WM_VSCROLL,
                (IntPtr)(SB_PAGEDOWN), (IntPtr)0);
              break;
            case Keys.Left:
              msg = Message.Create(Handle, WM_HSCROLL,
                //(IntPtr)(SB_PAGEDOWN), (IntPtr)0);
                (IntPtr)(SB_PAGEUP), (IntPtr)0); // 27.12.2020
              break;
            case Keys.Right:
              msg = Message.Create(Handle, WM_HSCROLL,
                (IntPtr)(SB_PAGEDOWN), (IntPtr)0);
              break;
            default:
              throw new BugException("Неизвестный KeyCode=" + args.KeyCode.ToString());
          }
          WndProc(ref msg);
          return;
        }
        args.IsInputKey = true;
      }
      else
        args.IsInputKey = false;

      base.OnPreviewKeyDown(args);
    }

    protected override void OnKeyDown(KeyEventArgs args)
    {
      //// Предотвращаем двойное срабатывание
      //if (Args.KeyCode == Keys.PageUp || Args.KeyCode == Keys.PageDown)
      //{
      //  Args.Handled = true;
      //  return;
      //}
      base.OnKeyDown(args);
    }

    #endregion

    #region Нажатие кнопки и перемещение мыши

    protected override void OnMouseDown(MouseEventArgs args)
    {
      if (!Focused)
        Focus();

      //base.OnMouseDown(Args);

      if (args.Button == MouseButtons.Left)
      {
        _InMouseMove = true;
        _StartMouse = Control.MousePosition;
      }
    }

    protected override void OnMouseUp(MouseEventArgs args)
    {
      base.OnMouseUp(args);
      _InMouseMove = false;
    }

    protected override void OnMouseMove(MouseEventArgs args)
    {
      base.OnMouseMove(args);
      if (_InMouseMove)
      {
        int dx = (Control.MousePosition.X - _StartMouse.X) / 4;
        int dy = (Control.MousePosition.Y - _StartMouse.Y) / 4;
        if (dx > 0)
        {
          Message msg = Message.Create(Handle, WM_HSCROLL, (IntPtr)SB_LINELEFT, (IntPtr)0);
          for (int i = 0; i < dx; i++)
            WndProc(ref msg);
        }
        if (dx < 0)
        {
          Message msg = Message.Create(Handle, WM_HSCROLL, (IntPtr)SB_LINERIGHT, (IntPtr)0);
          for (int i = 0; i < -dx; i++)
            WndProc(ref msg);
        }
        if (dy > 0)
        {
          Message msg = Message.Create(Handle, WM_VSCROLL, (IntPtr)SB_LINEUP, (IntPtr)0);
          for (int i = 0; i < dy; i++)
            WndProc(ref msg);
        }
        if (dy < 0)
        {
          Message msg = Message.Create(Handle, WM_VSCROLL, (IntPtr)SB_LINEDOWN, (IntPtr)0);
          for (int i = 0; i < -dy; i++)
            WndProc(ref msg);
        }
        _StartMouse = Control.MousePosition;
      }
    }

    #endregion

    #region Колесико мыши

    protected override void OnMouseWheel(MouseEventArgs args)
    {
      //      base.OnMouseWheel(Args);
      _WheelAcc += args.Delta;

      bool IsControl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
      bool IsShift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

      while (_WheelAcc > WHEEL_DELTA)
      {
        if (IsControl)
          OnWheelZoom(ZoomDirection.ZoomIn);
        else
          DoWheelScroll(true, IsShift);
        _WheelAcc -= WHEEL_DELTA;
      }
      while (_WheelAcc < -WHEEL_DELTA)
      {
        if (IsControl)
          OnWheelZoom(ZoomDirection.ZoomOut);
        else
          DoWheelScroll(false, IsShift);
        _WheelAcc += WHEEL_DELTA;
      }
    }


    private void DoWheelScroll(bool down, bool isPage)
    {
      Message msg = Message.Create(Handle, WM_VSCROLL, (IntPtr)(
        isPage ? (down ? SB_PAGEUP : SB_PAGEDOWN) : (down ? SB_LINEUP : SB_LINEDOWN)), (IntPtr)0);
      WndProc(ref msg);
      if (!isPage)
        WndProc(ref msg); // чтобы быстрее крутилось
    }

    #endregion

    #region Событие WheelZoom

    /// <summary>
    /// Событие вызывается при прокрутке колесика мыши вверх (увеличить масштаб)
    /// или вниз (уменьшить масштаб).
    /// Сам управляющий элемент не выполняет масштабирования
    /// </summary>
    public event ZoomDirectionEventHandler WheelZoom;

    protected void OnWheelZoom(ZoomDirection direction)
    {
      if (WheelZoom != null)
      {
        ZoomDirectionEventArgs Args = new ZoomDirectionEventArgs(direction);
        WheelZoom(this, Args);
      }
    }

    #endregion
  }

  public class EFPExtPrintPreviewControl : EFPControl<ExtPrintPreviewControl>
  {
    #region Конструктор

    public EFPExtPrintPreviewControl(EFPBaseProvider baseProvider, ExtPrintPreviewControl control )
      : base(baseProvider, control, true)
    {
    }

    #endregion
  }
}
