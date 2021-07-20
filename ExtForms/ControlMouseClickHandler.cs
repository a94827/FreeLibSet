using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Обработчик событий от мыши для управляющего элемента, в котором не установлены
  /// биты StandardClick и/или StandardDoubleClick, например RadioButton. Такой
  /// Control не посылает события MouseDoubleClick, а в событиях MouseUp и MouseDown
  /// свойство Clicks всегда равно 1.
  /// Объект ControlMouseClickHandler создается для управляющего элемента и 
  /// обрабатывает события мыши, вычисляя условия для двойного щелчка мыши.
  /// Генерируются собственные событий MouseDown, MouseClick, MouseDoubleClick и
  /// MouseUp
  /// Также позволяет отслеживать тройные щелчки, используя свойство Clicks
  /// </summary>
  public class ControlMouseClickHandler
  {
    #region Конструктор

    /// <summary>
    /// Присоединяет обработчики событий мыши
    /// </summary>
    /// <param name="сontrol">Управляющий элемент</param>
    public ControlMouseClickHandler(Control сontrol)
    {
#if DEBUG
      if (сontrol == null)
        throw new ArgumentNullException("сontrol");
#endif

      _Control = сontrol;

      сontrol.MouseDown += new MouseEventHandler(Control_MouseDown);
      сontrol.MouseUp += new MouseEventHandler(Control_MouseUp);
      сontrol.MouseLeave += new EventHandler(Control_MouseLeave);
    }

    #endregion

    #region Свойства и методы

    /// <summary>
    /// Управляющий элемент, к которому присоединены обработчики.
    /// Задается в конструкторе
    /// </summary>
    public Control Control { get { return _Control; } }
    private readonly Control _Control;

    /// <summary>
    /// Вызов этого метода предотвращает вызов следующего события MouseDoubleClick,
    /// заменяя его на обычный MouseClick
    /// </summary>
    public void ResetClicks()
    {
      _ClickCount = 0;
    }

    #endregion

    #region События

    /// <summary>
    /// Сюда может быть присоединен обработчик события нажатия кнопки мыши
    /// </summary>
    public event MouseEventHandler MouseDown;

    /// <summary>
    /// Сюда может быть присоединен обработчик события щелчка мыши
    /// </summary>
    public event MouseEventHandler MouseClick;

    /// <summary>
    /// Сюда может быть присоединен обработчик события двойного щелчка мыши
    /// </summary>
    public event MouseEventHandler MouseDoubleClick;

    /// <summary>
    /// Сюда может быть присоединен обработчик события отпускания кнопки мыши
    /// </summary>
    public event MouseEventHandler MouseUp;

    #endregion

    #region Виртуальные методы

    /// <summary>
    /// Вызывает событие
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseDown(MouseEventArgs args)
    {
      if (MouseDown != null)
        MouseDown(this, args);
    }

    /// <summary>
    /// Вызывает событие
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseClick(MouseEventArgs args)
    {
      if (MouseClick != null)
        MouseClick(this, args);
    }

    /// <summary>
    /// Вызывает событие
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseDoubleClick(MouseEventArgs args)
    {
      if (MouseDoubleClick != null)
        MouseDoubleClick(this, args);
    }

    /// <summary>
    /// Вызывает событие
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseUp(MouseEventArgs args)
    {
      if (MouseUp != null)
        MouseUp(this, args);
    }

    #endregion

    #region Внутренняя реализация

    private DateTime _LastClickTime;

    private MouseButtons _LastClickButton;

    private Point _LastClickLocation;

    private int _ClickCount;

    void Control_MouseDown(object sender, MouseEventArgs args)
    {
      try
      {
        DateTime ThisTime = DateTime.Now;
        if (_ClickCount > 0)
        {
          TimeSpan Span = ThisTime - _LastClickTime;
          int dx = Math.Abs(args.Location.X - _LastClickLocation.X);
          int dy = Math.Abs(args.Location.Y - _LastClickLocation.Y);
          if (args.Button != _LastClickButton || Span.TotalMilliseconds > SystemInformation.DoubleClickTime ||
            dx > SystemInformation.DoubleClickSize.Width || dy > SystemInformation.DoubleClickSize.Height)

            _ClickCount = 0;
        }
        _ClickCount++;
        _LastClickTime = ThisTime;
        _LastClickButton = args.Button;
        _LastClickLocation = args.Location;

        MouseEventArgs Args2 = new MouseEventArgs(args.Button, _ClickCount, args.X, args.Y, args.Delta);
        OnMouseDown(Args2);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика Control.MouseDown");
      }
    }

    void Control_MouseUp(object sender, MouseEventArgs args)
    {
      try
      {
        //if (args.Button != args.Button)
        if (args.Button != _LastClickButton) // 27.12.2020
          _ClickCount = 0;

        if (_ClickCount > 0)
        {
          MouseEventArgs Args2 = new MouseEventArgs(args.Button, _ClickCount, args.X, args.Y, args.Delta);
          if (_ClickCount == 1)
            OnMouseClick(Args2);
          else
            OnMouseDoubleClick(Args2);
        }

        // В событии Up не меняется зарегистрированное число Click'ов, сколько бы не прошло времени
        MouseEventArgs Args3 = new MouseEventArgs(args.Button, _ClickCount, args.X, args.Y, args.Delta);
        OnMouseUp(Args3);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика Control.MouseUp");
      }
    }

    void Control_MouseLeave(object sender, EventArgs args)
    {
      _ClickCount = 0;
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает Control.ToString()
    /// </summary>
    /// <returns>Текстовое представдение</returns>
    public override string ToString()
    {
      return _Control.ToString();
    }

    #endregion
  }
}
