// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Обработчик событий от мыши для управляющего элемента, в котором не установлены
  /// биты StandardClick и/или StandardDoubleClick, например <see cref="RadioButton"/>. Такой
  /// <see cref="System.Windows.Forms.Control"/> не посылает события <see cref="System.Windows.Forms.Control.MouseDoubleClick"/>, а в событиях <see cref="System.Windows.Forms.Control.MouseUp"/> и <see cref="System.Windows.Forms.Control.MouseDown"/>
  /// свойство <see cref="MouseEventArgs.Clicks"/> всегда равно 1.
  /// Объект <see cref="ControlMouseClickHandler"/> создается для управляющего элемента и 
  /// обрабатывает события мыши, вычисляя условия для двойного щелчка мыши.
  /// Генерируются собственные события <see cref="MouseDown"/>, <see cref="MouseClick"/>, <see cref="MouseDoubleClick"/> и
  /// <see cref="MouseUp"/>, так как события класса <see cref="System.Windows.Forms.Control"/> не могут быть вызваны.
  /// Также позволяет отслеживать тройные щелчки, используя свойство <see cref="MouseEventArgs.Clicks"/> в обработчике событий.
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
    /// Вызов этого метода предотвращает вызов следующего события <see cref="MouseDoubleClick"/>,
    /// заменяя его на обычный <see cref="MouseClick"/>.
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
    /// Вызывает событие <see cref="MouseDown"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseDown(MouseEventArgs args)
    {
      if (MouseDown != null)
        MouseDown(this, args);
    }

    /// <summary>
    /// Вызывает событие <see cref="MouseClick"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseClick(MouseEventArgs args)
    {
      if (MouseClick != null)
        MouseClick(this, args);
    }

    /// <summary>
    /// Вызывает событие <see cref="MouseDoubleClick"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnMouseDoubleClick(MouseEventArgs args)
    {
      if (MouseDoubleClick != null)
        MouseDoubleClick(this, args);
    }

    /// <summary>
    /// Вызывает событие <see cref="MouseUp"/>
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
        DateTime thisTime = DateTime.Now;
        if (_ClickCount > 0)
        {
          TimeSpan span = thisTime - _LastClickTime;
          int dx = Math.Abs(args.Location.X - _LastClickLocation.X);
          int dy = Math.Abs(args.Location.Y - _LastClickLocation.Y);
          if (args.Button != _LastClickButton || span.TotalMilliseconds > SystemInformation.DoubleClickTime ||
            dx > SystemInformation.DoubleClickSize.Width || dy > SystemInformation.DoubleClickSize.Height)

            _ClickCount = 0;
        }
        _ClickCount++;
        _LastClickTime = thisTime;
        _LastClickButton = args.Button;
        _LastClickLocation = args.Location;

        MouseEventArgs args2 = new MouseEventArgs(args.Button, _ClickCount, args.X, args.Y, args.Delta);
        OnMouseDown(args2);
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
          MouseEventArgs args2 = new MouseEventArgs(args.Button, _ClickCount, args.X, args.Y, args.Delta);
          if (_ClickCount == 1)
            OnMouseClick(args2);
          else
            OnMouseDoubleClick(args2);
        }

        // В событии Up не меняется зарегистрированное число Click'ов, сколько бы не прошло времени
        MouseEventArgs args3 = new MouseEventArgs(args.Button, _ClickCount, args.X, args.Y, args.Delta);
        OnMouseUp(args3);
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
    /// Возвращает <see cref="Control"/>.ToString()
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Control.ToString();
    }

    #endregion
  }
}
