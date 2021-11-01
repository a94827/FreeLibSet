﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.IO;
using System.Globalization;
using FreeLibSet.IO;
using System.Runtime.InteropServices;
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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Набор статических методов для работы с классами System.Windows.Forms и System.Drawing.
  /// Методы не связаны с EFPApp, EFPFormProvider и другими классами библиотеки ExtForms.
  /// Все методы класса являются потокобезопасными.
  /// </summary>
  public static class WinFormsTools
  {
    /// <summary>
    /// Используется для блокировки на уровне приложения в целом
    /// </summary>
    internal static readonly object InternalSyncRoot = new object();

    #region Картинки

    /// <summary>
    /// Возвращает один из объектов SystemIcons.
    /// Для None возвращает null
    /// </summary>
    /// <param name="icon">Перечисление MessageBoxIcon</param>
    /// <returns>Иконка</returns>
    public static Icon GetSystemIcon(MessageBoxIcon icon)
    {
      switch (icon)
      {
        case MessageBoxIcon.Error: return SystemIcons.Error;
        case MessageBoxIcon.Warning: return SystemIcons.Warning;
        case MessageBoxIcon.Information: return SystemIcons.Information;
        case MessageBoxIcon.Question: return SystemIcons.Question;
        case MessageBoxIcon.None: return null;
        default:
          throw new ArgumentException("Неизвествное значение " + icon.ToString(), "icon");
      }
    }

    /// <summary>
    /// Определяет необходимость уменьшения изображения.
    /// </summary>
    /// <param name="image">Исходное изображение</param>
    /// <param name="maxSize">Ограничение на размер изображения</param>
    /// <param name="newSize">Сюда помещается новый размер изображения, меньший или равный исходному.
    /// Пропорции сохраняются</param>
    /// <returns>true, если изображение должно быть уменьшено</returns>
    public static bool IsImageShrinkNeeded(Image image, Size maxSize, out Size newSize)
    {
      return IsImageShrinkNeeded(image.Size, maxSize, out newSize);
    }

    /// <summary>
    /// Определяет необходимость уменьшить размер изображения, чтобы вписать его в заданную область.
    /// При уменьшении сохраняются существующие пропорции.
    /// Если изображение меньше, чем <paramref name="maxSize"/>, то увеличение не выполняется.
    /// </summary>
    /// <param name="srcImageSize">Существующий размер изображение</param>
    /// <param name="maxSize">Размер, в который нужно вписать</param>
    /// <param name="newSize">Сюда записываются уменьшенные размеры.
    /// Если уменьшение не требуется, возвращается <paramref name="srcImageSize"/></param>
    /// <returns>true, если требуется уменьшение размеров</returns>
    public static bool IsImageShrinkNeeded(Size srcImageSize, Size maxSize, out Size newSize)
    {
#if DEBUG
      if (srcImageSize.Width < 0 || srcImageSize.Height < 0)
        throw new ArgumentException("Неправильный исходный размер: " + srcImageSize.ToString(), "maxSize");
      if (maxSize.Width < 1 || maxSize.Height < 1)
        throw new ArgumentException("Неправильный максимальный размер: " + maxSize.ToString(), "maxSize");
#endif

      if (srcImageSize.Width > maxSize.Width || srcImageSize.Height > maxSize.Height)
      {
        double s1 = 1.0;
        double s2 = 1.0;
        if (srcImageSize.Width > 0) // 27.12.2020
          s1 = (double)(maxSize.Width) / (double)(srcImageSize.Width);
        if (srcImageSize.Height > 0) // 27.12.2020
          s2 = (double)(maxSize.Height) / (double)(srcImageSize.Height);
        double s = Math.Min(s1, s2);

#if DEBUG
        if (s <= 0.0 || s > 1.0)
          throw new BugException("Неправильный коэффициент масштабирования: " + s.ToString());
#endif

        newSize = new Size((int)(Math.Round(srcImageSize.Width * s)),
        (int)(Math.Round(srcImageSize.Height * s)));
        return true;
      }
      else
      {
        newSize = srcImageSize;
        return false;
      }
    }

    /// <summary>
    /// Создает миниатюру для заданного изображения.
    /// Всегда создается новый объект Bitmap, даже если исходное изображение не требуется масштабировать.
    /// Если <paramref name="mainImage"/>=null, возвращается пустое изображение
    /// </summary>
    /// <param name="mainImage">Исходное изображенние</param>
    /// <param name="maxThumbnailSize">Максимальный размер миниатюры</param>
    /// <returns>Новый объект Bitmap</returns>
    public static Bitmap CreateThumbnailImage(Image mainImage, Size maxThumbnailSize)
    {
      if (mainImage == null)
        mainImage = EFPApp.MainImages.Images["EmptyImage"];

      Size NewSize;
      WinFormsTools.IsImageShrinkNeeded(mainImage, maxThumbnailSize, out NewSize);
      return new Bitmap(mainImage, NewSize);
    }

    #endregion

    #region Масштабирование Size и SizeF

    /// <summary>
    /// Увеличивает или уменьшает размеры, задаваемые <paramref name="size"/> 
    /// на масштабные коэффициенты, задаваемые <paramref name="factor"/>
    /// </summary>
    /// <param name="size">Исходные размеры</param>
    /// <param name="factor">Масштабные коэффициенты</param>
    /// <returns>Измененные размеры</returns>
    public static Size Scale(Size size, SizeF factor)
    {
      if (size.IsEmpty)
        return size;
      if (factor.IsEmpty)
        throw new ArgumentException("Не задан масштабный фактор", "factor");

      return new Size(Scale(size.Width, factor.Width),
        Scale(size.Height, factor.Height));
    }

    /// <summary>
    /// Умножает <paramref name="size"/> на <paramref name="factor"/> и округляет до целого
    /// </summary>
    /// <param name="size">Исходный размер</param>
    /// <param name="factor">Масштабный коэффициент</param>
    /// <returns>Новый размер</returns>
    public static int Scale(int size, float factor)
    {
      return (int)(Math.Round((float)size * factor, 0, MidpointRounding.AwayFromZero));
    }

    #endregion

    #region Координаты прямоугольников

    /// <summary>
    /// Вписать прямоугольник в родительскую область, чтобы он не высовывался.
    /// Эта перегрузка запрещает менять размеры прямоугольника <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">Вписываемый прямоугольник</param>
    /// <param name="area">Область, в которую надо вписать <paramref name="rect"/></param>
    /// <returns>Измененный прямоугольник</returns>
    public static Rectangle PlaceRectangle(Rectangle rect, Rectangle area)
    {
      return PlaceRectangle(rect, area, rect.Size);
    }

    /// <summary>
    /// Вписать прямоугольник в родительскую область, чтобы он не высовывался.
    /// Если прямоугольник <paramref name="rect"/> по размеру больше, чем <paramref name="area"/>,
    /// то он будет уменьшен, если <paramref name="allowShrink"/>=true. Иначе прямоугольник будет
    /// высоываться вправо и/или вниз.
    /// При уменьшении пропорции не сохраняются.
    /// </summary>
    /// <param name="rect">Вписываемый прямоугольник</param>
    /// <param name="area">Область, в которую надо вписать <paramref name="rect"/></param>
    /// <param name="allowShrink">Можно ли уменьшить размеры <paramref name="rect"/>, если он не входит</param>
    /// <returns>Измененный прямоугольник</returns>
    public static Rectangle PlaceRectangle(Rectangle rect, Rectangle area, bool allowShrink)
    {
      if (allowShrink)
        return PlaceRectangle(rect, area, Size.Empty);
      else
        return PlaceRectangle(rect, area, rect.Size);
    }

    /// <summary>
    /// Вписать прямоугольник в родительскую область, чтобы он не высовывался.
    /// Если прямоугольник <paramref name="rect"/> по размеру больше, чем <paramref name="area"/>,
    /// то он будет уменьшен.
    /// При уменьшении пропорции не сохраняются.
    /// </summary>
    /// <param name="rect">Вписываемый прямоугольник</param>
    /// <param name="area">Область, в которую надо вписать <paramref name="rect"/></param>
    /// <param name="minSize">До какого размера можно уменьшить <paramref name="rect"/>, если он не входит.
    /// Чтобы запретить уменьшение размера, задайте свойство равным <paramref name="rect"/>.Size</param>
    /// <returns>Измененный прямоугольник</returns>
    public static Rectangle PlaceRectangle(Rectangle rect, Rectangle area, Size minSize)
    {
      #region Размеры

      if (rect.Width > area.Width)
        //Rect.Width = Math.Max(Rect.Width - (Rect.Right - Area.Right), MinSize.Width);
        rect.Width = Math.Max(area.Width, minSize.Width); // 16.10.2018. Что-то перемудрил

      if (rect.Height > area.Height)
        //Rect.Height = Math.Max(Rect.Height - (Rect.Bottom - Area.Bottom), MinSize.Height);
        rect.Height = Math.Max(area.Height, minSize.Height); // 16.10.2018

      #endregion

      #region Сдвиг

      int NewX;
      if (area.Width > 0) // 15.05.2019
      {
        int dx = 0;
        if (rect.Right > area.Right)
          dx = area.Right - rect.Right; // отрицательное
        else if (rect.Left < area.Left)
          dx = area.Left - rect.Left; // положительное
        NewX = rect.Left + dx;
      }
      else
        NewX = area.Left;

      int NewY;
      if (area.Height >= 0) // 15.05.2019
      {
        int dy = 0;
        if (rect.Bottom > area.Bottom)
          dy = area.Bottom - rect.Bottom; // отрицательное
        else if (rect.Top < area.Top)
          dy = area.Top - rect.Top; // положительное
        NewY = rect.Top + dy;
      }
      else
        NewY = area.Top;

      rect.Location = new Point(NewX, NewY);

      #endregion

      return rect;
    }

    #endregion

    #region Min() и Max()

    /// <summary>
    /// Возвращает размер, в котором и для длины и для ширины содержатся максимальные значения
    /// исходных размеров
    /// </summary>
    /// <param name="a">Первый размер</param>
    /// <param name="b">Второй размер</param>
    /// <returns>Максимальный размер</returns>
    public static Size Max(Size a, Size b)
    {
      return new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));
    }

    /// <summary>
    /// Возвращает размер, в котором и для длины и для ширины содержатся минимальные значения
    /// исходных размеров
    /// </summary>
    /// <param name="a">Первый размер</param>
    /// <param name="b">Второй размер</param>
    /// <returns>Минимальный размер</returns>
    public static Size Min(Size a, Size b)
    {
      return new Size(Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height));
    }

    /// <summary>
    /// Возвращает размер, в котором и для длины и для ширины содержатся максимальные значения
    /// исходных размеров
    /// </summary>
    /// <param name="a">Первый размер</param>
    /// <param name="b">Второй размер</param>
    /// <returns>Максимальный размер</returns>
    public static SizeF Max(SizeF a, SizeF b)
    {
      return new SizeF(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));
    }

    /// <summary>
    /// Возвращает размер, в котором и для длины и для ширины содержатся минимальные значения
    /// исходных размеров
    /// </summary>
    /// <param name="a">Первый размер</param>
    /// <param name="b">Второй размер</param>
    /// <returns>Минимальный размер</returns>
    public static SizeF Min(SizeF a, SizeF b)
    {
      return new SizeF(Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height));
    }

    #endregion

    #region Для Control

    /// <summary>
    /// Размещение управляющего элемента в центре родительского элемента
    /// Этот метод может вызываться из события Resize.
    /// </summary>
    /// <param name="control">Управляющий элемент, который нужно центрировать 
    /// в его родительском элементе</param>
    public static void PlaceControlInCenter(Control control)
    {
      Size sz1 = control.Size;
      Size sz2 = control.Parent.ClientSize;
      control.Location = new Point((sz2.Width - sz1.Width) / 2, (sz2.Height - sz1.Height) / 2);
    }

    /// <summary>
    /// Замена одного управляющего элемента на другой с сохранением
    /// текущего положения и порядка обхода элементов. Тип элементов совпадать
    /// не обязан
    /// </summary>
    /// <param name="oldControl">Существующий управляющий элемент, который имеет родителя</param>
    /// <param name="newControl">Новый неприсоединенный управляющий элемент с Parent=null</param>
    public static void ReplaceControl(Control oldControl, Control newControl)
    {
      if (oldControl == null)
        throw new ArgumentNullException("oldControl");
      if (newControl == null)
        throw new ArgumentNullException("newControl");
      if (oldControl.Parent == null)
        throw new ArgumentException("Существующий элемент не имеет родителя", "oldControl");
      if (newControl.Parent != null)
        throw new ArgumentException("Добавляемый элемент уже имеет родителя", "newControl");

      // Копируем свойства
      newControl.Dock = oldControl.Dock;
      newControl.Anchor = oldControl.Anchor;
      newControl.SetBounds(oldControl.Left, oldControl.Top,
         oldControl.Width, oldControl.Height);
      newControl.TabIndex = oldControl.TabIndex;

      Control Parent = oldControl.Parent;
      int p = Parent.Controls.GetChildIndex(oldControl);

      if (Parent is TableLayoutPanel)
        DoReplaceTLP((TableLayoutPanel)Parent, oldControl, newControl);
      else
      {
        Parent.Controls.Remove(oldControl);
        Parent.Controls.Add(newControl);
      }
      Parent.Controls.SetChildIndex(newControl, p);
    }

    private static void DoReplaceTLP(TableLayoutPanel parent, Control oldControl, Control newControl)
    {
      TableLayoutPanelCellPosition tlp = parent.GetPositionFromControl(oldControl);

      parent.Controls.Remove(oldControl);
      parent.Controls.Add(newControl, tlp.Column, tlp.Row);
    }

    /// <summary>
    /// Отсоединяет от управляющего элемента все дочерние элементы и вызывает для
    /// них Dispose()
    /// </summary>
    /// <param name="control">Родительский элемент</param>
    public static void DisposeChildren(Control control)
    {
      if (control == null)
        return;
      if (control.Controls.Count == 0)
        return;

      Control[] a = new Control[control.Controls.Count];
      control.Controls.CopyTo(a, 0);
      control.Controls.Clear();
      for (int i = 0; i < a.Length; i++)
        a[i].Dispose();
    }

    /// <summary>
    /// Перенос всех управляющих элементов из одного родительского элемента (формы) в другой.
    /// </summary>
    /// <param name="srcParentControl">Родительский элемент, содержащий элементы, которые нужно перенести</param>
    /// <param name="resParentControl">Пустой управляющий элемент</param>
    public static void MoveControls(Control srcParentControl, Control resParentControl)
    {
#if DEBUG
      if (srcParentControl == null)
        throw new ArgumentNullException("srcParentControl");
      if (resParentControl == null)
        throw new ArgumentNullException("resParentControl");
#endif
      Control[] Controls = new Control[srcParentControl.Controls.Count];
      srcParentControl.Controls.CopyTo(Controls, 0);
      for (int i = 0; i < Controls.Length; i++)
        Controls[i].Parent = resParentControl;
    }


    /// <summary>
    /// Установка фокуса ввода на управляющий элемент с обработкой закладок TabControl
    /// </summary>
    /// <param name="control">Элемент, который получит фокус</param>
    /// <returns>True, если удалось установить управляющий элемент</returns>
    public static bool FocusToControl(Control control)
    {
      if (control == null)
        return false;

      //FocusToControl(Control.Parent); // 11.12.2018
      //if (Control.Parent is ContainerControl)
      //  ((ContainerControl)(Control.Parent)).ActiveControl = Control;

      if (!DoSetControlPage(control)) // Рекурсивная функция
        return false;
      if (!control.Enabled)
        return false;
      if (!control.CanSelect)
        return false;
      if (control is RadioButton)
      {
        // Для радиокнопки установка методом Select приводит к появлению точки
        // на этом элементе
        bool OldAutoCheck = ((RadioButton)control).AutoCheck;
        ((RadioButton)control).AutoCheck = false;
        control.Select();
        ((RadioButton)control).AutoCheck = OldAutoCheck;
      }
      else
        control.Select();
      return true;
    }

    private static bool DoSetControlPage(Control control)
    {
      if (control == null)
        return true;
      if (!DoSetControlPage(control.Parent))
        return false;

      if (control is TabPage)
      {
        TabControl tc = (TabControl)(((TabPage)control).Parent);
        tc.SelectedTab = (TabPage)control;
      }
      return true;
    }

    /// <summary>
    /// Возвращает <paramref name="control"/>, если для переданного элемента установлено свойство CanSelect.
    /// Иначе берется родительский элемент, и.т.д.
    /// Если очередной Control.Parent вернул null, а свойство CanSelect не было 
    /// установлено (куча вложенных Panel без формы), возвращается null
    /// </summary>
    /// <param name="control">Проверяемый управляющий элемент. Может быть null</param>
    /// <returns><paramref name="control"/> или один из его родителей или null</returns>
    public static Control GetSelectableControl(Control control)
    {
      while (control != null)
      {
        if (control.CanSelect)
          return control;
        control = control.Parent;
      }

      return null;
    }

    /// <summary>
    /// Возвращает true, если управляющий элемент расположен на форме и вся цепочка
    /// родителей имеет свойство Visible=true.
    /// Возвращает false, если для одного из элементов в цепочке IsDisposed=true.
    /// Если <paramref name="control"/>=null, возвращает false/
    /// </summary>
    /// <param name="control">Проверяемый управляющий элемент</param>
    /// <returns>Видимость цепочки элементов</returns>
    public static bool AreControlAndFormVisible(Control control)
    {
      while (control != null)
      {
        if (!control.Visible)
          return false;

        if (control.Parent == null)
        {
          if (control is Form)
            return true;
        }
        control = control.Parent;
      }

      return false;
    }

    /// <summary>
    /// Расширенная функция добавления управляющего элемента в коллекцию
    /// Используется, когда форма уже выведена на экран и требуется добавить
    /// элемент из другой формы-"заготовки", которая сама на экран никогда
    /// не выводится (например, при переходе к очередному шагу мастера).
    /// В этом случае стандартные механизмы Windows не могут выполнить 
    /// масштабирование, если текущее разрешение экрана не совпадает с тем,
    /// что использовалось при разработке формы-заготовки
    /// 
    /// Метод отсоединяет добавляемый элемент от родителя (формы-заготовки или ее
    /// дочернего элемента), если он есть. Затем проверяется, что раньше элемент
    /// располагался на форме-"заготовке" (а не создан динамически) и выполняется
    /// масштабирование с помощью Control.Scale(). Затем, отмасщтабированный
    /// элемент добавляется в коллекцию Controls нового хозяина
    /// </summary>
    /// <param name="newParent">Новая панель-хозяин, куда будет добавлен элемент</param>
    /// <param name="child">Добавляемый управляющий элемент</param>
    public static void AddControlAndScale(Control newParent, Control child)
    {
      Form OldForm = child.FindForm();
      Form NewForm = newParent.FindForm();
      child.Parent = null;
      if (OldForm != null && OldForm != NewForm && (!OldForm.Visible) && NewForm.Visible)
      {
        // Выполняем масштабирование
        SizeF sz1 = OldForm.AutoScaleDimensions;
        SizeF sz2 = NewForm.CurrentAutoScaleDimensions;
        if (sz1.Width > 0 && sz1.Height > 0 && sz2.Width > 0 && sz2.Height > 0) // перестраховка
        {
          SizeF Factor = new SizeF(sz2.Width / sz1.Width, sz2.Height / sz1.Height);
          child.Scale(Factor);
        }
      }
      newParent.Controls.Add(child);
    }

    /// <summary>
    /// Возвращает текст в управляющем элементе (свойство Control.Text). 
    /// Если элемент не содержит текста, возвращает рекурсивно текст родительского
    /// элемента
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Свойство Control.Text</returns>
    public static string GetControlText(Control control)
    {
      while (control != null)
      {
        if (!String.IsNullOrEmpty(control.Text))
          return control.Text;
        control = control.Parent;
      }
      return String.Empty;
    }

    /// <summary>
    /// Удаление мнемонических символов из текста команд меню, меток, кнопок
    /// Одиночные символы "амперсанд" удаляются
    /// Два идущих подряд символа "амперсанд" заменяются на один
    /// </summary>
    /// <param name="s">Строка, возможно содержащая мнемонический символ "амперсанд"</param>
    /// <returns>Строка без мнемонических символов</returns>
    public static string RemoveMnemonic(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      if (s.IndexOf('&') < 0)
        return s; // не надо заменять

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == '&')
        {
          if (i < (s.Length - 1))
          {
            if (s[i + 1] == '&')
            {
              sb.Append('&');
              i++;
            }
          }
        }
        else
          sb.Append(s[i]);
      }
      return sb.ToString();
    }


    /// <summary>
    /// Возвращает часть области Control.ClientArea, из которой исключены части, занятые
    /// дочерними управляющими элементами с установленным свойством Doc=Top, Left, Right и Bottom.
    /// Дочерние управляющие элементы с Dock=None и Fill не учитываются.
    /// Скрытые элементы не учитываются.
    /// Для определения области в MDI-контейнере используйте метод GetMdiContainerArea()
    /// </summary>
    /// <param name="control">Управляющий элемент или форма MDI Container</param>
    /// <returns>"Свободная" область</returns>
    public static Rectangle GetControlDockFillArea(Control control)
    {
      if (control == null)
        throw new ArgumentNullException("control");
      int Left = control.ClientRectangle.Left + control.Padding.Left;
      int Right = control.ClientRectangle.Right - control.Padding.Right; // 09.06.2021
      int Top = control.ClientRectangle.Top + control.Padding.Top;
      int Bottom = control.ClientRectangle.Bottom - control.Padding.Bottom; // 09.06.2021
      foreach (Control Child in control.Controls)
      {
        if (!Child.Visible)
          continue;
        switch (Child.Dock)
        {
          case DockStyle.Left:
            Left = Math.Max(Left, Child.Right + Child.Margin.Right);
            break;
          case DockStyle.Right:
            Right = Math.Min(Right, Child.Left - Child.Margin.Left);
            break;
          case DockStyle.Top:
            Top = Math.Max(Top, Child.Bottom + Child.Margin.Bottom);
            break;
          case DockStyle.Bottom:
            Bottom = Math.Min(Bottom, Child.Top - Child.Margin.Top);
            break;
        }
      }

      return new Rectangle(Left, Top, Right - Left, Bottom - Top);
    }

    /// <summary>
    /// Возвращает true, если управляющий элемент <paramref name="parent"/> прямо или косвенно является родителем для 
    /// <paramref name="child"/>.
    /// Метод рекурсивно просматривает свойство Control.Parent для дочернего элемента.
    /// Если один из аргументов равен null, возвращает false
    /// </summary>
    /// <param name="parent">Родительский элемент</param>
    /// <param name="child">Дочерний элемент</param>
    /// <returns>Наличие дочернего элемента у родителя</returns>
    public static bool ContainsControl(Control parent, Control child)
    {
      if (parent == null || child == null)
        return false;

      while (child != null)
      {
        if (child == parent)
          return true;
        child = child.Parent;
      }
      return false;
    }

    #region FindControl()

    /// <summary>
    /// Поиск управляющего элемента заданного вида (например, статусной строки).
    /// </summary>
    /// <typeparam name="T">Тип управляющего элемента, который требуется найти.
    /// Будет также найден элемент производного класса</typeparam>
    /// <param name="control">Управляющий элемент, с которого требуется начать поиск.
    /// Может быть формой Form.
    /// Будет проверен сам этот элемент, его дочерние элементы Controls и рекурсивно все вложенные элементы.</param>
    /// <returns>Первый найденный элемент. Если не найдено ни одного управляющего элемента, возвращается null</returns>
    public static T FindControl<T>(Control control)
      where T : Control
    {
      return FindControl<T>(control, true);
    }

    /// <summary>
    /// Поиск управляющего элемента заданного вида (например, статусной строки).
    /// </summary>
    /// <typeparam name="T">Тип управляющего элемента, который требуется найти.
    /// Будет также найден элемент производного класса</typeparam>
    /// <param name="control">Управляющий элемент, с которого требуется начать поиск.
    /// Может быть формой Form.
    /// Если <paramref name="recurse"/> равен true, будут проверены рекурсивно все элементы</param>
    /// <param name="recurse">true, если требуется рекурсивный просмотр элементов.
    /// Если false, то будет просмотрен только Control и, его непосредственные дочерние элементы</param>
    /// <returns>Первый найденный элемент. Если не найдено ни одного управляющего элемента, возвращается null</returns>
    public static T FindControl<T>(Control control, bool recurse)
      where T : Control
    {
      if (control == null)
        return null;

      if (control is T)
        return (T)control;

      return DoFindControlWithinChildren<T>(control, recurse);
    }

    private static T DoFindControlWithinChildren<T>(Control control, bool recurse)
      where T : Control
    {
      if (!control.HasChildren)
        return null;

      for (int i = 0; i < control.Controls.Count; i++)
      {
        T Child = control.Controls[i] as T;
        if (Child != null)
          return Child;
        if (recurse)
        {
          T Res = DoFindControlWithinChildren<T>(control.Controls[i], true);
          if (Res != null)
            return Res;
        }
      }

      return null;
    }

    #endregion

    #region GetControls()

    /// <summary>
    /// Поиск управляющих элементов заданного вида.
    /// Можно использовать, например, для поиска всех кнопок на форме.
    /// </summary>
    /// <typeparam name="T">Тип управляющих элементов, которые требуется найти.
    /// Используйте тип Control, чтобы найти все элементы.
    /// Будут также найдены все элементы производных классов</typeparam>
    /// <param name="control">Управляющий элемент, с которого требуется начать поиск.
    /// Может быть формой Form.
    /// Будет проверен сам этот элемент, его дочерние элементы Controls и рекурсивно все вложенные элементы.</param>
    /// <returns>Массив найденных элементов. Если не найдено ни одного управляющего элемента, возвращается пустой массив</returns>
    public static T[] GetControls<T>(Control control)
      where T : Control
    {
      return GetControls<T>(control, true);
    }

    /// <summary>
    /// Поиск управляющих элементов заданного вида.
    /// Можно использовать, например, для поиска всех кнопок на форме.
    /// </summary>
    /// <typeparam name="T">Тип управляющих элементов, которые требуется найти.
    /// Используйте тип Control, чтобы найти все элементы.
    /// Будут также найдены все элементы производных классов</typeparam>
    /// <param name="control">Управляющий элемент, с которого требуется начать поиск.
    /// Может быть формой Form.
    /// Будет проверен сам этот элемент и его дочерние элементы Controls.
    /// Если <paramref name="recurse"/> равен true, будут проверены рекурсивно все элементы</param>
    /// <param name="recurse">true, если требуется рекурсивный просмотр элементов.
    /// Если false, то будет просмотрен только Control и, его непосредственные дочерние элементы</param>
    /// <returns>Массив найденных элементов. Если не найдено ни одного управляющего элемента, возвращается пустой массив</returns>
    public static T[] GetControls<T>(Control control, bool recurse)
      where T : Control
    {
      if (control == null)
        return new T[0];

      List<T> lst = null;
      if (control is T)
      {
        lst = new List<T>();
        lst.Add((T)control);
      }

      DoGetChildControls<T>(ref lst, control, recurse);

      if (lst == null)
        return new T[0];
      else
        return lst.ToArray();
    }

    private static void DoGetChildControls<T>(ref List<T> lst, Control control, bool recurse)
      where T : Control
    {
      if (!control.HasChildren)
        return;

      for (int i = 0; i < control.Controls.Count; i++)
      {
        T Child = control.Controls[i] as T;
        if (Child != null)
        {
          if (lst == null)
            lst = new List<T>();
          lst.Add(Child);
        }
        if (recurse)
          DoGetChildControls<T>(ref lst, control.Controls[i], true);
      }
    }

    #endregion

    #region FindButton()

    /// <summary>
    /// Поиск в форме закрывающей кнопки с заданным кодом DialogResult.
    /// Если кнопка не найдена, возвращает null
    /// </summary>
    /// <param name="control">Управляющий элемент, с которого начинается поиск. Обычно это форма</param>
    /// <param name="dialogResult">Значение свойства Button.DialogResult</param>
    /// <returns>Объект кнопки или null</returns>
    public static Button FindButton(Control control, DialogResult dialogResult)
    {
      if (control == null)
        return null;

      Button btn = control as Button;
      if (btn != null)
      {
        if (btn.DialogResult == dialogResult)
          return btn;
      }

      if (!control.HasChildren)
        return null;

      for (int i = 0; i < control.Controls.Count; i++)
      {
        btn = FindButton(control.Controls[i], dialogResult); // рекурсивный вызов
        if (btn != null)
          return btn;
      }
      return null;
    }

    #endregion

    #region GetControlExcess()

    /// <summary>
    /// Проверяет, что дочерние управляющие элементы помещаются в пределах клиентской области
    /// родительского управляющего элемента.
    /// Для каждого элемента проверяется только правая и нижняя граница. Верхняя и левая не проверяются.
    /// Свойства Dock и Anchor дочерних элементов не учитываются, предполагается, что элементы
    /// располагаются от верхнего левого угла родительского элемента.
    /// Допустимой считается область ClientRectangle за вычетом полей Padding.
    /// Проверяются только непосредственные дочерние элементы, рекурсивный обход элементов не выполняется.
    /// Возвращается размер, на который следует увеличить родительский элемент, чтобы в него
    /// поместились дочерние элементы.
    /// Возвращается Size.Empty, если все элементы поместились.
    /// </summary>
    /// <param name="parent">Родителский элемент</param>
    /// <returns>Недостающий размер, на который надо увеличить родительский элемент</returns>
    public static Size GetControlExcess(Control parent)
    {
      if (parent == null)
        throw new ArgumentNullException("parent");

      if (!parent.HasChildren)
        return Size.Empty;

      Size sz = Size.Empty;
      int R = parent.ClientRectangle.Right - parent.Padding.Right;
      int B = parent.ClientRectangle.Bottom - parent.Padding.Bottom;

      for (int i = 0; i < parent.Controls.Count; i++)
      {
        Rectangle rc2 = parent.Controls[i].Bounds;
        int dx = rc2.Right - R;
        int dy = rc2.Bottom - B;
        if (dx < 0)
          dx = 0;
        if (dy < 0)
          dy = 0;
        sz = Max(sz, new Size(dx, dy));
      }
      return sz;
    }

    #endregion

    /// <summary>
    /// Возвращает true, если <paramref name="control"/>=null или <paramref name="control"/>.IsDisposed=true
    /// </summary>
    /// <param name="control">Проверяемый элемент</param>
    /// <returns>True, если элемент не пригоден для использования</returns>
    public static bool IsNullOrDisposed(Control control)
    {
      if (control == null)
        return true;
      else
        return control.IsDisposed;
    }

    #region GetChildAtPont()

    /// <summary>
    /// Поиск дочернего управляющего элемента в заданной позиции.
    /// Расширяет стандартный метод Control.GetChildAtPoint() рекурсивным вызовом.
    /// Сначала вызывается <paramref name="parentControl"/>.GetChildAtPoint(). Если он возвращает
    /// дочерний элемент, вызов повторяется для него (с преобразованием координат).
    /// Таким образом, будет найден элемент самого вложенного уровня.
    /// Эта версия выполняет поиск всех элементов, включая скрытые.
    /// Обычно следует использовать перегрузку, принимающую аргумент skipValue.
    /// </summary>
    /// <param name="parentControl">Управляющий элемент, в котором выполняется поиск, например, форма. Не может быть null</param>
    /// <param name="pt">Координаты, в которых ищется элемент. Задаются относительно клиентской области <paramref name="parentControl"/>.</param>
    /// <returns>Найденный дочерний, внучатый, ... элемент или null, если нет элементов в указанной позиции</returns>
    public static Control GetChildAtPointRecursive(Control parentControl, Point pt)
    {
      return GetChildAtPointRecursive(parentControl, pt, GetChildAtPointSkip.None);
    }

    /// <summary>
    /// Поиск дочернего управляющего элемента в заданной позиции.
    /// Расширяет стандартный метод Control.GetChildAtPoint() рекурсивным вызовом.
    /// Сначала вызывается <paramref name="parentControl"/>.GetChildAtPoint(). Если он возвращает
    /// дочерний элемент, вызов повторяется для него (с преобразованием координат).
    /// Таким образом, будет найден элемент самого вложенного уровня.
    /// </summary>
    /// <param name="parentControl">Управляющий элемент, в котором выполняется поиск, например, форма. Не может быть null</param>
    /// <param name="pt">Координаты, в которых ищется элемент. Задаются относительно клиентской области <paramref name="parentControl"/>.</param>
    /// <param name="skipValue">Определяет, какие элементы следует игнорировать. 
    /// Обычно следует задавать значение GetChildAtPointSkip.Invisible.</param>
    /// <returns>Найденный дочерний, внучатый, ... элемент или null, если нет элементов в указанной позиции</returns>
    public static Control GetChildAtPointRecursive(Control parentControl, Point pt, GetChildAtPointSkip skipValue)
    {
#if DEBUG
      if (parentControl == null)
        throw new ArgumentNullException("parentControl");
#endif

      // Первый такт
      Control ctrl2 = parentControl.GetChildAtPoint(pt, skipValue);
      if (ctrl2 == null)
        return null;

      // Запускаем циклический поиск
      Control ctrl1 = parentControl;
      do
      {
        pt = ctrl1.PointToScreen(pt);
        pt = ctrl2.PointToClient(pt);
        ctrl1 = ctrl2;
        ctrl2 = ctrl1.GetChildAtPoint(pt, skipValue);
      }
      while (ctrl2 != null);

      return ctrl1;
    }

    #endregion

    #endregion

    #region Для SplitContainer

    /// <summary>
    /// Установка положения разделителя в SplitContainer (свойство Distance) с
    /// выполнением проверки допустимости значения.
    /// Обычная установка свойства может привести в вызову исключения, если
    /// установлены ограничения на минимальные размеры панелей
    /// </summary>
    /// <param name="control">Контейнер с двумя панелями</param>
    /// <param name="value">Желаемое значение для свойства Distance</param>
    public static void SetSplitContainerDistance(SplitContainer control, int value)
    {
      int sz;
      if (control.Orientation == Orientation.Horizontal)
        sz = control.Panel1.Height + control.Panel2.Height;
      else
        sz = control.Panel1.Width + control.Panel2.Width;
      int min1 = control.Panel1MinSize;
      int min2 = control.Panel2MinSize;

      if (sz <= (min1 + min2))
        return; // нельзя установить

      if (value < min1)
        value = min1;
      if (value > (sz - min2))
        value = sz - min2;

      try
      {
        control.SplitterDistance = value;
      }
      catch { }
    }

    /// <summary>
    /// Установка положения разделителя в SplitContainer в процентном соотношении
    /// к размерам объекта. На момент вызова должно быть установлено свойство
    /// Orientation
    /// </summary>
    /// <param name="control">Контейнер с двумя панелями</param>
    /// <param name="percent">Процентное значение размера верхней или левой панели (от 1 до 99,
    /// если будет вне диапазона, то будет изменено)</param>
    public static void SetSplitContainerDistancePercent(SplitContainer control, int percent)
    {
      if (percent < 1)
        percent = 1;
      if (percent > 99)
        percent = 99;

      control.FixedPanel = FixedPanel.None;

      int WholeSize;
      if (control.Orientation == Orientation.Horizontal)
        // верхняя и нижняя панели
        WholeSize = control.ClientSize.Height - control.SplitterWidth;
      else
        WholeSize = control.ClientSize.Width - control.SplitterWidth;

      SetSplitContainerDistance(control, WholeSize * percent / 100);
    }


    /// <summary>
    /// Определение положения разделителя в SplitContainer в процентном соотношении
    /// к размерам объекта. На момент вызова должно быть установлено свойство
    /// Orientation
    /// </summary>
    /// <param name="control">Контейнер с двумя панелями</param>
    /// <returns>Процентное значение размера верхней или левой панели (от 0 до 100)</returns>
    public static int GetSplitContainerDistancePercent(SplitContainer control)
    {
      int WholeSize;
      if (control.Orientation == Orientation.Horizontal)
        // верхняя и нижняя панели
        WholeSize = control.ClientSize.Height - control.SplitterWidth;
      else
        WholeSize = control.ClientSize.Width - control.SplitterWidth;

      if (WholeSize < 1)
        return 0;

      return control.SplitterDistance * 100 / WholeSize;
    }


    /// <summary>
    /// Установка положения разделителя в SplitContainer точно посередине
    /// объекта. На момент вызова должно быть установлено свойство
    /// Orientation
    /// </summary>
    /// <param name="control">Контейнер с двумя панелями</param>
    public static void SetSplitContainerDistance(SplitContainer control)
    {
      SetSplitContainerDistancePercent(control, 50);
    }

    #endregion

    #region Для TabControl

    internal static void CorrectTabControlActivation(TabControl control)
    {
      Form TheForm = control.FindForm();
      if (TheForm == null)
        return;
      control.Focus(); // 24.08.2016

      while (true)
      {
        TabPage FirstControl = control.SelectedTab;
        if (FirstControl == null)
        {
          if (control.TabPages.Count > 0)
            FirstControl = control.TabPages[0]; // 24.08.2016
          else
            return; // окно не имеет ни одной закладки
        }
        FirstControl.SelectNextControl(null, true, true, true, false);
        Control NewControl = TheForm.ActiveControl;
        if (NewControl is TabControl && NewControl != control)
          control = (TabControl)NewControl;
        else
          break;
      }

      //DebugTools.DebugObject(TheForm.ActiveControl, "TheForm.ActiveControl");
    }

    #endregion

    #region Для DataGridView

    /// <summary>
    /// Если просмотр присоединен к DataSet'у DataView или DataTable,
    /// возвращаем DataTable, иначе возвращаем null
    /// </summary>
    /// <param name="control">Табличный просмотр</param>
    public static DataTable GetTableDataSource(DataGridView control)
    {
      if (control.DataSource == null)
        return null;
      if (control.DataSource is DataTable)
        return (DataTable)(control.DataSource);
      if (control.DataSource is DataView)
        return ((DataView)(control.DataSource)).Table;
      if (control.DataSource is DataSet)
        return ((DataSet)(control.DataSource)).Tables[control.DataMember];
      return null;
    }

    /// <summary>
    /// Получить горизонтальное выравнивание текста ячейки 
    /// (преобразование из DataGridViewContentAlignment в HorizontalAlignment )
    /// </summary>
    /// <param name="cellAlign">Выравнивание ячейки</param>
    /// <returns>Горизонтальное выравнивание текста</returns>
    public static HorizontalAlignment GetTextAlign(DataGridViewContentAlignment cellAlign)
    {
      switch (cellAlign)
      {
        case DataGridViewContentAlignment.TopLeft:
        case DataGridViewContentAlignment.MiddleLeft:
        case DataGridViewContentAlignment.BottomLeft:
          return HorizontalAlignment.Left;

        case DataGridViewContentAlignment.TopCenter:
        case DataGridViewContentAlignment.MiddleCenter:
        case DataGridViewContentAlignment.BottomCenter:
          return HorizontalAlignment.Center;

        case DataGridViewContentAlignment.TopRight:
        case DataGridViewContentAlignment.MiddleRight:
        case DataGridViewContentAlignment.BottomRight:
          return HorizontalAlignment.Right;

        default:
          throw new ArgumentException("Неизвестное значение " + cellAlign.ToString(), "cellAlign");
      }
    }

    /// <summary>
    /// Получить выравнивание ячейки исходя из существующего выравнивания и 
    /// желательного выравнивая текста
    /// </summary>
    /// <param name="textAlign">Желаемое выравнивание текста</param>
    /// <param name="cellAlign">Существующее выравнивание ячейки</param>
    /// <returns>Выравнивание ячейки</returns>
    public static DataGridViewContentAlignment GetCellAlign(HorizontalAlignment textAlign, DataGridViewContentAlignment cellAlign)
    {
      switch (textAlign)
      {
        case HorizontalAlignment.Left:
          switch (cellAlign)
          {
            case DataGridViewContentAlignment.TopLeft:
            case DataGridViewContentAlignment.TopCenter:
            case DataGridViewContentAlignment.TopRight:
              return DataGridViewContentAlignment.TopLeft;

            case DataGridViewContentAlignment.MiddleLeft:
            case DataGridViewContentAlignment.MiddleCenter:
            case DataGridViewContentAlignment.MiddleRight:
              return DataGridViewContentAlignment.MiddleLeft;

            case DataGridViewContentAlignment.BottomLeft:
            case DataGridViewContentAlignment.BottomCenter:
            case DataGridViewContentAlignment.BottomRight:
              return DataGridViewContentAlignment.BottomLeft;

            default:
              throw new ArgumentException("Неизвестный cellAlign=" + cellAlign.ToString(), "cellAlign");
          }
        //break;

        case HorizontalAlignment.Center:
          switch (cellAlign)
          {
            case DataGridViewContentAlignment.TopLeft:
            case DataGridViewContentAlignment.TopCenter:
            case DataGridViewContentAlignment.TopRight:
              return DataGridViewContentAlignment.TopCenter;

            case DataGridViewContentAlignment.MiddleLeft:
            case DataGridViewContentAlignment.MiddleCenter:
            case DataGridViewContentAlignment.MiddleRight:
              return DataGridViewContentAlignment.MiddleCenter;

            case DataGridViewContentAlignment.BottomLeft:
            case DataGridViewContentAlignment.BottomCenter:
            case DataGridViewContentAlignment.BottomRight:
              return DataGridViewContentAlignment.BottomCenter;

            default:
              throw new ArgumentException("Неизвестный cellAlign=" + cellAlign.ToString(), "cellAlign");
          }
        //break;

        case HorizontalAlignment.Right:
          switch (cellAlign)
          {
            case DataGridViewContentAlignment.TopLeft:
            case DataGridViewContentAlignment.TopCenter:
            case DataGridViewContentAlignment.TopRight:
              return DataGridViewContentAlignment.TopRight;

            case DataGridViewContentAlignment.MiddleLeft:
            case DataGridViewContentAlignment.MiddleCenter:
            case DataGridViewContentAlignment.MiddleRight:
              return DataGridViewContentAlignment.MiddleRight;

            case DataGridViewContentAlignment.BottomLeft:
            case DataGridViewContentAlignment.BottomCenter:
            case DataGridViewContentAlignment.BottomRight:
              return DataGridViewContentAlignment.BottomRight;

            default:
              throw new ArgumentException("Неизвестный cellAlign=" + cellAlign.ToString(), "cellAlign");
          }
        //break;

        default:
          throw new ArgumentException("Неизвестный textAlign=" + textAlign.ToString(), "textAlign");
      }
    }

    /// <summary>
    /// Получить выравнивание ячейки исходя из горизонтального выравнивания текста
    /// (преобразование из HorizontalAlignment в DataGridViewContentAlignment)
    /// Вертикальное выравнивание ячейки предполагается по центру
    /// </summary>
    /// <param name="textAlign">Горизонтальное выравнивание текста</param>
    /// <returns>Выравнивание ячейки</returns>
    public static DataGridViewContentAlignment GetCellAlign(HorizontalAlignment textAlign)
    {
      switch (textAlign)
      {
        case HorizontalAlignment.Left:
          return DataGridViewContentAlignment.BottomLeft;

        case HorizontalAlignment.Center:
          return DataGridViewContentAlignment.MiddleCenter;

        case HorizontalAlignment.Right:
          return DataGridViewContentAlignment.MiddleRight;

        default:
          throw new ArgumentException("Неизвестный textAlign=" + textAlign.ToString(), "textAlign");
      }
    }

    /// <summary>
    /// Возвращает отсортированный в порядке размещения массив объектов DataGridViewColumn
    /// </summary>
    /// <param name="control">Управляющий элемент DataGridView</param>
    /// <returns>Массив столбцов</returns>
    public static DataGridViewColumn[] GetOrderedAllColumns(DataGridView control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      DataGridViewColumn[] a = new DataGridViewColumn[control.Columns.Count];
      control.Columns.CopyTo(a, 0);
      Array.Sort<DataGridViewColumn>(a, new Comparison<DataGridViewColumn>(DataGridViewComparer));
      return a;
    }

    private static int DataGridViewComparer(DataGridViewColumn x, DataGridViewColumn y)
    {
      return x.DisplayIndex - y.DisplayIndex;
    }

    /// <summary>
    /// Возвращает отсортированный в порядке размещения массив объектов DataGridViewColumn, 
    /// у которых свойство Visible=true
    /// </summary>
    /// <param name="control">Управляющий элемент DataGridView</param>
    /// <returns>Массив столбцов</returns>
    public static DataGridViewColumn[] GetOrderedVisibleColumns(DataGridView control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      // 17.05.2016
      // Под mono не реализованы методы DataGridViewColumnCollection.GetFirstColumn() и GetLastColumn()
      // Выполняем перебор вручную

      List<DataGridViewColumn> lst = new List<DataGridViewColumn>(control.Columns.Count);
      for (int i = 0; i < control.Columns.Count; i++)
      {
        if (control.Columns[i].Visible)
          lst.Add(control.Columns[i]);
      }
      lst.Sort(new Comparison<DataGridViewColumn>(DataGridViewComparer));
      return lst.ToArray();
    }

    /// <summary>
    /// Возвращает отсортированный в порядке размещения массив объектов DataGridViewColumn, 
    /// у которых свойство Visible=true и Selected=true
    /// </summary>
    /// <param name="control">Управляющий элемент DataGridView</param>
    /// <returns>Массив столбцов</returns>
    public static DataGridViewColumn[] GetOrderedSelectedColumns(DataGridView control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      // 17.05.2016
      // Под mono не реализованы методы DataGridViewColumnCollection.GetFirstColumn() и GetLastColumn()
      // Выполняем перебор вручную

      List<DataGridViewColumn> lst = new List<DataGridViewColumn>(control.Columns.Count);
      for (int i = 0; i < control.Columns.Count; i++)
      {
        if (control.Columns[i].Visible && control.Columns[i].Selected)
          lst.Add(control.Columns[i]);
      }
      lst.Sort(new Comparison<DataGridViewColumn>(DataGridViewComparer));
      return lst.ToArray();
    }

    /// <summary>
    /// Преобразует массив столбцов DataGridViewColumn в массив индексов столбцоы
    /// </summary>
    /// <param name="columns">Массив столбцов</param>
    /// <returns>Массив индексов</returns>
    public static int[] GetColumnIndices(DataGridViewColumn[] columns)
    {
#if DEBUG
      if (columns == null)
        throw new ArgumentNullException("columns");
#endif
      int[] a = new int[columns.Length];
      for (int i = 0; i < columns.Length; i++)
        a[i] = columns[i].Index;
      return a;
    }

    /// <summary>
    /// Преобразует массив индексов столбцов в массив объектов DataGridViewColumn
    /// </summary>
    /// <param name="control">Табличный просмотр</param>
    /// <param name="columnIndices">Массив индексов столбцов</param>
    /// <returns>Массив столбцов</returns>
    public static DataGridViewColumn[] GetColumns(DataGridView control, int[] columnIndices)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
      if (columnIndices == null)
        throw new ArgumentNullException("columnIndices");
#endif
      DataGridViewColumn[] a = new DataGridViewColumn[columnIndices.Length];
      for (int i = 0; i < columnIndices.Length; i++)
        a[i] = control.Columns[columnIndices[i]];
      return a;
    }

    #endregion

    #region Для ComboBox

    /// <summary>
    /// Устанавливает свойство ComboBox.Size в соответствии с самой длинной строкой в списке Items.
    /// </summary>
    /// <param name="control">Комбоблок</param>
    internal static void SetComboBoxWidth(ComboBox control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      if (control.Items.Count == 0)
        return;

      try
      {
        int w = 0;
        using (Graphics gr = control.CreateGraphics())
        {
          foreach (object item in control.Items)
          {
            string s = item.ToString();
            w = Math.Max(w, (int)gr.MeasureString(s, control.Font).Width);
          }
        }

        w += SystemInformation.Border3DSize.Width * 4 + SystemInformation.VerticalScrollBarWidth;
        w = Math.Max(w, control.Width); // уменьшать не будем

        control.Size = new Size(w, control.Size.Height);
      }
      catch { }
    }

    #endregion

    #region Для Form

    /// <summary>
    /// Размещение формы в указанной прямоугольной области.
    /// Если форма имеет изменяемые размеры, то она может быть уменьшена, чтобы поместиться в <paramref name="area"/>.
    /// Этот метод не меняет и не учитывает состояние формы WindowState.
    /// </summary>
    /// <param name="form">Разменщаемая форма</param>
    /// <param name="area">Область для размещения формы</param>
    public static void PlaceFormInRectangle(Form form, Rectangle area)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      Size MinSize;
      switch (form.FormBorderStyle)
      {
        case FormBorderStyle.Sizable:
        case FormBorderStyle.SizableToolWindow:
          MinSize = WinFormsTools.Max(form.MinimumSize, SystemInformation.MinimumWindowSize);
          break;
        default:
          MinSize = form.Size;
          break;
      }

      form.Bounds = PlaceRectangle(form.Bounds, area, MinSize);
    }

    /// <summary>
    /// Если форма выходит за пределы экрана, выполняется ее перемещение так, чтобы она помещалась
    /// Размеры формы не меняются
    /// Только для форм верхнего уровня
    /// </summary>
    /// <param name="form"></param>
    public static void PlaceFormInScreen(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      // Область для размещения окна
      Rectangle Area = Screen.FromControl(form).WorkingArea;
      form.Bounds = PlaceRectangle(form.Bounds, Area);
    }


    /// <summary>
    /// Установка формы по центру экрана, в котором расположена форма <paramref name="parentForm"/>. Меняется свойство Location
    /// Размер формы не изменяется.
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <param name="parentForm">Форма, экран которой используется для размещения формы. Если форма не задана, используется первичный дисплей</param>
    public static void PlaceFormInScreenCenter(Form form, Control parentForm)
    {
      PlaceFormInScreenCenter(form, parentForm, false);
    }

    /// <summary>
    /// Установка формы по центру экрана, в котором расположена форма <paramref name="parentForm"/>. Меняется свойство Location
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <param name="parentForm">Форма, экран которой используется для размещения формы. Может быть null. Если форма не задана, используется первичный дисплей</param>
    /// <param name="limitSize">Нужно ли уменьшить размеры формы, если она она не помещается на экране</param>
    public static void PlaceFormInScreenCenter(Form form, Control parentForm, bool limitSize)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      Screen Scr;
      if (parentForm == null)
        Scr = Screen.PrimaryScreen;
      else if (parentForm.IsHandleCreated)
        Scr = Screen.FromHandle(parentForm.Handle);
      else
        Scr = Screen.PrimaryScreen;

      PlaceFormInScreenCenter(form, Scr, limitSize);  // испр. 17.01.2019
    }

    /// <summary>
    /// Установка формы по центру экрана, в котором расположена форма <paramref name="parentForm"/>. Меняется свойство Location.
    /// Размер формы не изменяется.
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <param name="parentForm">Форма, экран которой используется для размещения формы. Может быть null. Если форма не задана, используется первичный дисплей</param>
    public static void PlaceFormInScreenCenter(Form form, IWin32Window parentForm)
    {
      Screen Scr;
      if (parentForm == null)
        Scr = Screen.PrimaryScreen;
      else
        Scr = Screen.FromHandle(parentForm.Handle);

      PlaceFormInScreenCenter(form, Scr);
    }

    /// <summary>
    /// Разместить форму в центре экрана, на котором она располагается сейчас.
    /// Размеры формы не меняются
    /// </summary>
    /// <param name="form">Форма, положение которой задается</param>
    public static void PlaceFormInScreenCenter(Form form)
    {
      PlaceFormInScreenCenter(form, Screen.FromControl(form), false);
    }

    /// <summary>
    /// Разместить форму в центре экрана, на котором она располагается сейчас.
    /// Размеры формы могут быть уменьшены, если <paramref name="limitSize"/>=true
    /// </summary>
    /// <param name="form">Форма, положение которой задается</param>
    /// <param name="limitSize">Нужно ли уменьшить размеры формы, если она она не помещается на экране</param>
    public static void PlaceFormInScreenCenter(Form form, bool limitSize)
    {
      PlaceFormInScreenCenter(form, Screen.FromControl(form), limitSize);
    }


    /// <summary>
    /// Установка формы по центру заданного экрана. Меняется свойство Location.
    /// Размер формы не изменяется
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <param name="screen">Экран, на котором будет размещена форма</param>
    public static void PlaceFormInScreenCenter(Form form, Screen screen)
    {
      PlaceFormInScreenCenter(form, screen, false);
    }

    /// <summary>
    /// Установка формы по центру заданного экрана. Меняется свойство Location.
    /// Размеры формы могут быть уменьшены, если <paramref name="limitSize"/>=true
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <param name="screen">Экран, на котором будет размещена форма</param>
    /// <param name="limitSize">Нужно ли уменьшить размеры формы, если она она не помещается на экране</param>
    public static void PlaceFormInScreenCenter(Form form, Screen screen, bool limitSize)
    {
      Rectangle rcScr = screen.WorkingArea;
      if (limitSize)
      {
        if (form.Width > rcScr.Width)
          form.Width = rcScr.Width;
        if (form.Height > rcScr.Height)
          form.Height = rcScr.Height;
      }
      Size sz1 = form.Size;
      int Left = rcScr.Left + (rcScr.Width - sz1.Width) / 2;
      int Top = rcScr.Top + (rcScr.Height - sz1.Height) / 2;
      form.StartPosition = FormStartPosition.Manual;
      form.Location = new Point(Left, Top);

      PlaceFormInScreen(form);
    }

    /// <summary>
    /// Показывает форму снизу (если возможно) или сверху от заданной прямоугольной области.
    /// </summary>
    /// <param name="form">Форма, координаты которой надо задать</param>
    /// <param name="ownerControl">Управляющий элемент, из которого выпадает форма</param>
    public static void PlacePopupForm(Form form, Control ownerControl)
    {
      if (ownerControl == null)
        throw new ArgumentNullException("ownerControl");
      Rectangle rc = new Rectangle(0, 0, ownerControl.Width, ownerControl.Height);
      Rectangle OwnerRect = ownerControl.RectangleToScreen(rc);
      PlacePopupForm(form, OwnerRect);
    }

    /// <summary>
    /// Показывает форму снизу (если возможно) или сверху от заданной прямоугольной области.
    /// </summary>
    /// <param name="form">Форма, координаты которой надо задать</param>
    /// <param name="ownerRect">Прямоугольная область (экранные координаты) элемента, из которого выпадает форма</param>
    public static void PlacePopupForm(Form form, Rectangle ownerRect)
    {
      Screen scr = Screen.FromRectangle(ownerRect);
      PlacePopupForm(form, ownerRect, scr.WorkingArea);
    }

    /// <summary>
    /// Размещает форму, которая должна быть показана под определенным окном, 
    /// похоже на выпадающий список комбоблока.
    /// Если форма не может быть размещена под основным элементом, то выполняется попытка
    /// разместить ее над элементом.
    /// Размеры формы не меняются.
    /// Если форма не помещается не над, не под элементом, то она может перекрыть элемент.
    /// </summary>
    /// <param name="form">Созданная всплывающая форма, которую требуется разместить</param>
    /// <param name="ownerRect">Область, занимаемая основным элементом (полем ввода комблока
    /// или кнопкой) в экранных координатах</param>
    /// <param name="wholeRect">Рабочая область экрана, в которую следует вписать форму.</param>
    public static void PlacePopupForm(Form form, Rectangle ownerRect, Rectangle wholeRect)
    {
      if (form == null)
        throw new ArgumentNullException("form");

      #region Снизу или сверху?

      int DH1 = ownerRect.Top - wholeRect.Top; // место над элементом
      int DH2 = wholeRect.Bottom - ownerRect.Bottom; // место под элементов

      bool Below;
      if (DH2 >= form.Height)
        Below = true;
      else if (DH1 >= form.Height)
        Below = false;
      else
        Below = true;

      #endregion

      form.StartPosition = FormStartPosition.Manual;
      if (Below)
        form.Top = ownerRect.Bottom;
      else
        form.Top = ownerRect.Top - form.Height;
      form.Left = ownerRect.Left;

      PlaceFormInScreen(form);
    }

    /// <summary>
    /// Преобразование формы, содержащей кнопки "ОК" и "Отмена" в форму, содержащую
    /// только кнопку "ОК", которая выполняет закрытие формы
    /// </summary>
    /// <param name="form">Форма с кнопками "ОК" и "Отмена"</param>
    public static void OkCancelFormToOkOnly(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.AcceptButton == null)
        throw new ArgumentException("У формы не установлено свойство AcceptButton", "form");
      if (form.CancelButton == null)
        throw new ArgumentException("У формы не установлено свойство CancelButton", "form");
#endif
      Button btnOk = form.AcceptButton as Button;
      Button btnCancel = form.CancelButton as Button;
      btnOk.DialogResult = DialogResult.Cancel;
      form.CancelButton = btnOk;
      btnCancel.Visible = false;
    }

    /// <summary>
    /// Попытка закрытия формы с требуемым результатом.
    /// Если обработчик TheForm.Closing не разрешает закрыть форму, то восстанавливается
    /// исходное состояние TheForm.DialogResult
    /// Применяется внутри обработчиков управляющих элементов формы (например, DoubleClick)
    /// </summary>
    /// <param name="form">Форма, которую надо закрыть</param>
    /// <param name="dialogResult">Значение TheForm.DialogResult, которое будет установлено</param>
    /// <returns>true, если форму удалось закрыть, и false, если Closing запретил закрытие</returns>
    public static bool TryCloseForm(Form form, DialogResult dialogResult)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      DialogResult OldRes = form.DialogResult;
      form.DialogResult = dialogResult;
      bool Success = false;
      try
      {
        form.Close();
        Success = (!form.Visible) || (dialogResult != DialogResult.None);
      }
      finally
      {
        if (!Success)
          form.DialogResult = OldRes;
      }
      return Success;
    }

    /// <summary>
    /// Получить доступную область для размещения форм в MDI-контейнере
    /// </summary>
    /// <param name="mdiContainer">Форма с установленным свойством IsMdiContainer=true</param>
    /// <returns>Область для размещения дочерних окон</returns>
    public static Rectangle GetMdiContainerArea(Form mdiContainer)
    {
#if DEBUG
      if (mdiContainer == null)
        throw new ArgumentNullException("mdiContainer");
#endif

      if (!mdiContainer.IsMdiContainer)
        throw new ArgumentException("Свойство Form.IsMdiContainer не установлено", "mdiContainer");
      Rectangle Area;
      FormWindowState OldState = mdiContainer.WindowState;
      try
      {
        if (mdiContainer.WindowState == FormWindowState.Minimized)
          mdiContainer.WindowState = FormWindowState.Normal; // 15.05.2019
        Area = GetControlDockFillArea(mdiContainer);
        Area.Location = new Point(0, 0); // при размещении в MDI-контейнере координаты считаются от свободной области
        Area.Width -= SystemInformation.FrameBorderSize.Width; // почему нужен такой большой зазор - не знаю. Исправлено 09.06.2021
        Area.Height -= SystemInformation.FrameBorderSize.Height;
      }
      finally
      {
        mdiContainer.WindowState = OldState;
      }
      return Area;
    }

    /// <summary>
    /// Получить значение свойства <paramref name="form"/>.MiminumSize в координатах клиентной области
    /// </summary>
    /// <param name="form">Форма для извлечения свойства</param>
    /// <returns>Минимальный размер формы в координатах клиентной области</returns>
    public static Size GetFormClientMinimumSize(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (form.MinimumSize.IsEmpty)
        return Size.Empty;

      int w = Math.Max(form.MinimumSize.Width - (form.Width - form.ClientSize.Width), 0);
      int h = Math.Max(form.MinimumSize.Height - (form.Height - form.ClientSize.Height), 0);
      return new Size(w, h);
    }

    /// <summary>
    /// Установка свойства <paramref name="form"/>.MinimumSize в координатах клиентной области
    /// </summary>
    /// <param name="form">Форма для установки свойства</param>
    /// <param name="value">Минимальный размер формы в координатах клиентной области</param>
    public static void SetFormClientMinimumSize(Form form, Size value)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (value.IsEmpty)
        form.MinimumSize = Size.Empty;
      else
      {
        if (value.Width < 0 || value.Height < 0)
          throw new ArgumentOutOfRangeException("value");

        int w = value.Width + (form.Width - form.ClientSize.Width);
        int h = value.Height + (form.Height - form.ClientSize.Height);
        form.MinimumSize = new Size(w, h);
      }
    }

    /// <summary>
    /// Получить значение свойства <paramref name="form"/>.MaxinumSize в координатах клиентной области
    /// </summary>
    /// <param name="form">Форма для извлечения свойства</param>
    /// <returns>Максимальный размер формы в координатах клиентной области</returns>
    public static Size GetFormClientMaximumSize(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (form.MaximumSize.IsEmpty)
        return Size.Empty;

      int w = Math.Max(form.MaximumSize.Width - (form.Width - form.ClientSize.Width), 0);
      int h = Math.Max(form.MaximumSize.Height - (form.Height - form.ClientSize.Height), 0);
      return new Size(w, h);
    }

    /// <summary>
    /// Установка свойства <paramref name="form"/>.MaximumSize в координатах клиентной области
    /// </summary>
    /// <param name="form">Форма для установки свойства</param>
    /// <param name="value">Максимальный размер формы в координатах клиентной области</param>
    public static void SetFormClientMaximumSize(Form form, Size value)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (value.IsEmpty)
        form.MaximumSize = Size.Empty;
      else
      {
        if (value.Width < 0 || value.Height < 0)
          throw new ArgumentOutOfRangeException("value");

        int w = value.Width + (form.Width - form.ClientSize.Width);
        int h = value.Height + (form.Height - form.ClientSize.Height);
        form.MaximumSize = new Size(w, h);
      }
    }

    #endregion

    #region Для ToolStrip

    // Эксперимент
#if XXX
    private enum KnownColors
    {
      msocbvcrCBBdrOuterDocked,
      msocbvcrCBBdrOuterFloating,
      msocbvcrCBBkgd,
      msocbvcrCBCtlBdrMouseDown,
      msocbvcrCBCtlBdrMouseOver,
      msocbvcrCBCtlBdrSelected,
      msocbvcrCBCtlBdrSelectedMouseOver,
      msocbvcrCBCtlBkgd,
      msocbvcrCBCtlBkgdLight,
      msocbvcrCBCtlBkgdMouseDown,
      msocbvcrCBCtlBkgdMouseOver,
      msocbvcrCBCtlBkgdSelected,
      msocbvcrCBCtlBkgdSelectedMouseOver,
      msocbvcrCBCtlText,
      msocbvcrCBCtlTextDisabled,
      msocbvcrCBCtlTextLight,
      msocbvcrCBCtlTextMouseDown,
      msocbvcrCBCtlTextMouseOver,
      msocbvcrCBDockSeparatorLine,
      msocbvcrCBDragHandle,
      msocbvcrCBDragHandleShadow,
      msocbvcrCBDropDownArrow,
      msocbvcrCBGradMainMenuHorzBegin,
      msocbvcrCBGradMainMenuHorzEnd,
      msocbvcrCBGradMenuIconBkgdDroppedBegin,
      msocbvcrCBGradMenuIconBkgdDroppedEnd,
      msocbvcrCBGradMenuIconBkgdDroppedMiddle,
      msocbvcrCBGradMenuTitleBkgdBegin,
      msocbvcrCBGradMenuTitleBkgdEnd,
      msocbvcrCBGradMouseDownBegin,
      msocbvcrCBGradMouseDownEnd,
      msocbvcrCBGradMouseDownMiddle,
      msocbvcrCBGradMouseOverBegin,
      msocbvcrCBGradMouseOverEnd,
      msocbvcrCBGradMouseOverMiddle,
      msocbvcrCBGradOptionsBegin,
      msocbvcrCBGradOptionsEnd,
      msocbvcrCBGradOptionsMiddle,
      msocbvcrCBGradOptionsMouseOverBegin,
      msocbvcrCBGradOptionsMouseOverEnd,
      msocbvcrCBGradOptionsMouseOverMiddle,
      msocbvcrCBGradOptionsSelectedBegin,
      msocbvcrCBGradOptionsSelectedEnd,
      msocbvcrCBGradOptionsSelectedMiddle,
      msocbvcrCBGradSelectedBegin,
      msocbvcrCBGradSelectedEnd,
      msocbvcrCBGradSelectedMiddle,
      msocbvcrCBGradVertBegin,
      msocbvcrCBGradVertEnd,
      msocbvcrCBGradVertMiddle,
      msocbvcrCBIconDisabledDark,
      msocbvcrCBIconDisabledLight,
      msocbvcrCBLabelBkgnd,
      msocbvcrCBLowColorIconDisabled,
      msocbvcrCBMainMenuBkgd,
      msocbvcrCBMenuBdrOuter,
      msocbvcrCBMenuBkgd,
      msocbvcrCBMenuCtlText,
      msocbvcrCBMenuCtlTextDisabled,
      msocbvcrCBMenuIconBkgd,
      msocbvcrCBMenuIconBkgdDropped,
      msocbvcrCBMenuShadow,
      msocbvcrCBMenuSplitArrow,
      msocbvcrCBOptionsButtonShadow,
      msocbvcrCBShadow,
      msocbvcrCBSplitterLine,
      msocbvcrCBSplitterLineLight,
      msocbvcrCBTearOffHandle,
      msocbvcrCBTearOffHandleMouseOver,
      msocbvcrCBTitleBkgd,
      msocbvcrCBTitleText,
      msocbvcrDisabledFocuslessHighlightedText,
      msocbvcrDisabledHighlightedText,
      msocbvcrDlgGroupBoxText,
      msocbvcrDocTabBdr,
      msocbvcrDocTabBdrDark,
      msocbvcrDocTabBdrDarkMouseDown,
      msocbvcrDocTabBdrDarkMouseOver,
      msocbvcrDocTabBdrLight,
      msocbvcrDocTabBdrLightMouseDown,
      msocbvcrDocTabBdrLightMouseOver,
      msocbvcrDocTabBdrMouseDown,
      msocbvcrDocTabBdrMouseOver,
      msocbvcrDocTabBdrSelected,
      msocbvcrDocTabBkgd,
      msocbvcrDocTabBkgdMouseDown,
      msocbvcrDocTabBkgdMouseOver,
      msocbvcrDocTabBkgdSelected,
      msocbvcrDocTabText,
      msocbvcrDocTabTextMouseDown,
      msocbvcrDocTabTextMouseOver,
      msocbvcrDocTabTextSelected,
      msocbvcrDWActiveTabBkgd,
      msocbvcrDWActiveTabText,
      msocbvcrDWActiveTabTextDisabled,
      msocbvcrDWInactiveTabBkgd,
      msocbvcrDWInactiveTabText,
      msocbvcrDWTabBkgdMouseDown,
      msocbvcrDWTabBkgdMouseOver,
      msocbvcrDWTabTextMouseDown,
      msocbvcrDWTabTextMouseOver,
      msocbvcrFocuslessHighlightedBkgd,
      msocbvcrFocuslessHighlightedText,
      msocbvcrGDHeaderBdr,
      msocbvcrGDHeaderBkgd,
      msocbvcrGDHeaderCellBdr,
      msocbvcrGDHeaderCellBkgd,
      msocbvcrGDHeaderCellBkgdSelected,
      msocbvcrGDHeaderSeeThroughSelection,
      msocbvcrGSPDarkBkgd,
      msocbvcrGSPGroupContentDarkBkgd,
      msocbvcrGSPGroupContentLightBkgd,
      msocbvcrGSPGroupContentText,
      msocbvcrGSPGroupContentTextDisabled,
      msocbvcrGSPGroupHeaderDarkBkgd,
      msocbvcrGSPGroupHeaderLightBkgd,
      msocbvcrGSPGroupHeaderText,
      msocbvcrGSPGroupline,
      msocbvcrGSPHyperlink,
      msocbvcrGSPLightBkgd,
      msocbvcrHyperlink,
      msocbvcrHyperlinkFollowed,
      msocbvcrJotNavUIBdr,
      msocbvcrJotNavUIGradBegin,
      msocbvcrJotNavUIGradEnd,
      msocbvcrJotNavUIGradMiddle,
      msocbvcrJotNavUIText,
      msocbvcrListHeaderArrow,
      msocbvcrNetLookBkgnd,
      msocbvcrOABBkgd,
      msocbvcrOBBkgdBdr,
      msocbvcrOBBkgdBdrContrast,
      msocbvcrOGMDIParentWorkspaceBkgd,
      msocbvcrOGRulerActiveBkgd,
      msocbvcrOGRulerBdr,
      msocbvcrOGRulerBkgd,
      msocbvcrOGRulerInactiveBkgd,
      msocbvcrOGRulerTabBoxBdr,
      msocbvcrOGRulerTabBoxBdrHighlight,
      msocbvcrOGRulerTabStopTicks,
      msocbvcrOGRulerText,
      msocbvcrOGTaskPaneGroupBoxHeaderBkgd,
      msocbvcrOGWorkspaceBkgd,
      msocbvcrOLKFlagNone,
      msocbvcrOLKFolderbarDark,
      msocbvcrOLKFolderbarLight,
      msocbvcrOLKFolderbarText,
      msocbvcrOLKGridlines,
      msocbvcrOLKGroupLine,
      msocbvcrOLKGroupNested,
      msocbvcrOLKGroupShaded,
      msocbvcrOLKGroupText,
      msocbvcrOLKIconBar,
      msocbvcrOLKInfoBarBkgd,
      msocbvcrOLKInfoBarText,
      msocbvcrOLKPreviewPaneLabelText,
      msocbvcrOLKTodayIndicatorDark,
      msocbvcrOLKTodayIndicatorLight,
      msocbvcrOLKWBActionDividerLine,
      msocbvcrOLKWBButtonDark,
      msocbvcrOLKWBButtonLight,
      msocbvcrOLKWBDarkOutline,
      msocbvcrOLKWBFoldersBackground,
      msocbvcrOLKWBHoverButtonDark,
      msocbvcrOLKWBHoverButtonLight,
      msocbvcrOLKWBLabelText,
      msocbvcrOLKWBPressedButtonDark,
      msocbvcrOLKWBPressedButtonLight,
      msocbvcrOLKWBSelectedButtonDark,
      msocbvcrOLKWBSelectedButtonLight,
      msocbvcrOLKWBSplitterDark,
      msocbvcrOLKWBSplitterLight,
      msocbvcrPlacesBarBkgd,
      msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd,
      msocbvcrPPOutlineThumbnailsPaneTabBdr,
      msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd,
      msocbvcrPPOutlineThumbnailsPaneTabText,
      msocbvcrPPSlideBdrActiveSelected,
      msocbvcrPPSlideBdrActiveSelectedMouseOver,
      msocbvcrPPSlideBdrInactiveSelected,
      msocbvcrPPSlideBdrMouseOver,
      msocbvcrPubPrintDocScratchPageBkgd,
      msocbvcrPubWebDocScratchPageBkgd,
      msocbvcrSBBdr,
      msocbvcrScrollbarBkgd,
      msocbvcrToastGradBegin,
      msocbvcrToastGradEnd,
      msocbvcrWPBdrInnerDocked,
      msocbvcrWPBdrOuterDocked,
      msocbvcrWPBdrOuterFloating,
      msocbvcrWPBkgd,
      msocbvcrWPCtlBdr,
      msocbvcrWPCtlBdrDefault,
      msocbvcrWPCtlBdrDisabled,
      msocbvcrWPCtlBkgd,
      msocbvcrWPCtlBkgdDisabled,
      msocbvcrWPCtlText,
      msocbvcrWPCtlTextDisabled,
      msocbvcrWPCtlTextMouseDown,
      msocbvcrWPGroupline,
      msocbvcrWPInfoTipBkgd,
      msocbvcrWPInfoTipText,
      msocbvcrWPNavBarBkgnd,
      msocbvcrWPText,
      msocbvcrWPTextDisabled,
      msocbvcrWPTitleBkgdActive,
      msocbvcrWPTitleBkgdInactive,
      msocbvcrWPTitleTextActive,
      msocbvcrWPTitleTextInactive,
      msocbvcrXLFormulaBarBkgd,
      lastKnownColor = msocbvcrXLFormulaBarBkgd
    }

    /// <summary>
    /// Таблица цветов стиля оформления Microsoft Office 2003 ("голубая")
    /// </summary>
    /// <remarks>
    /// Чтобы использовать "голубое" меню, добавьте вызов в начале программы:
    /// ToolStripManager.Renderer = new ToolStripProfessionalRenderer(WinFormsTools.BlueLunaColorTable);
    /// </remarks>
    public static ProfessionalColorTable BlueLunaColorTable
    {
      get
      {
        if (FBlueLunaColorTable == null)
          FBlueLunaColorTable = new FixedProfessionalColorTable(new Color[]{
            /* msocbvcrCBBdrOuterDocked */ Color.FromArgb(196, 205, 218),
            /* msocbvcrCBBdrOuterFloating */ Color.FromArgb(42, 102, 201),
            /* msocbvcrCBBkgd */ Color.FromArgb(196, 219, 249),
            /* msocbvcrCBCtlBdrMouseDown */ Color.FromArgb(0, 0, 128),
            /* msocbvcrCBCtlBdrMouseOver */ Color.FromArgb(0, 0, 128),
            /* msocbvcrCBCtlBdrSelected */ Color.FromArgb(0, 0, 128),
            /* msocbvcrCBCtlBdrSelectedMouseOver */ Color.FromArgb(0, 0, 128),
            /* msocbvcrCBCtlBkgd */ Color.FromArgb(196, 219, 249),
            /* msocbvcrCBCtlBkgdLight */ Color.FromArgb(255, 255, 255),
            /* msocbvcrCBCtlBkgdMouseDown */ Color.FromArgb(254, 128, 62),
            /* msocbvcrCBCtlBkgdMouseOver */ Color.FromArgb(255, 238, 194),
            /* msocbvcrCBCtlBkgdSelected */ Color.FromArgb(255, 192, 111),
            /* msocbvcrCBCtlBkgdSelectedMouseOver */ Color.FromArgb(254, 128, 62),
            /* msocbvcrCBCtlText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrCBCtlTextDisabled */ Color.FromArgb(141, 141, 141),
            /* msocbvcrCBCtlTextLight */ Color.FromArgb(128, 128, 128),
            /* msocbvcrCBCtlTextMouseDown */ Color.FromArgb(0, 0, 0),
            /* msocbvcrCBCtlTextMouseOver */ Color.FromArgb(0, 0, 0),
            /* msocbvcrCBDockSeparatorLine */ Color.FromArgb(0, 53, 145),
            /* msocbvcrCBDragHandle */ Color.FromArgb(39, 65, 118),
            /* msocbvcrCBDragHandleShadow */ Color.FromArgb(255, 255, 255),
            /* msocbvcrCBDropDownArrow */ Color.FromArgb(236, 233, 216),
            /* msocbvcrCBGradMainMenuHorzBegin */ Color.FromArgb(158, 190, 245),
            /* msocbvcrCBGradMainMenuHorzEnd */ Color.FromArgb(196, 218, 250),
            /* msocbvcrCBGradMenuIconBkgdDroppedBegin */ Color.FromArgb(203, 221, 246),
            /* msocbvcrCBGradMenuIconBkgdDroppedEnd */ Color.FromArgb(114, 155, 215),
            /* msocbvcrCBGradMenuIconBkgdDroppedMiddle */ Color.FromArgb(161, 197, 249),
            /* msocbvcrCBGradMenuTitleBkgdBegin */ Color.FromArgb(227, 239, 255),
            /* msocbvcrCBGradMenuTitleBkgdEnd */ Color.FromArgb(123, 164, 224),
            /* msocbvcrCBGradMouseDownBegin */ Color.FromArgb(254, 128, 62),
            /* msocbvcrCBGradMouseDownEnd */ Color.FromArgb(255, 223, 154),
            /* msocbvcrCBGradMouseDownMiddle */ Color.FromArgb(255, 177, 109),
            /* msocbvcrCBGradMouseOverBegin */ Color.FromArgb(255, 255, 222),
            /* msocbvcrCBGradMouseOverEnd */ Color.FromArgb(255, 203, 136),
            /* msocbvcrCBGradMouseOverMiddle */ Color.FromArgb(255, 225, 172),
            /* msocbvcrCBGradOptionsBegin */ Color.FromArgb(127, 177, 250),
            /* msocbvcrCBGradOptionsEnd */ Color.FromArgb(0, 53, 145),
            /* msocbvcrCBGradOptionsMiddle */ Color.FromArgb(82, 127, 208),
            /* msocbvcrCBGradOptionsMouseOverBegin */ Color.FromArgb(255, 255, 222),
            /* msocbvcrCBGradOptionsMouseOverEnd */ Color.FromArgb(255, 193, 118),
            /* msocbvcrCBGradOptionsMouseOverMiddle */ Color.FromArgb(255, 225, 172),
            /* msocbvcrCBGradOptionsSelectedBegin */ Color.FromArgb(254, 140, 73),
            /* msocbvcrCBGradOptionsSelectedEnd */ Color.FromArgb(255, 221, 152),
            /* msocbvcrCBGradOptionsSelectedMiddle */ Color.FromArgb(255, 184, 116),
            /* msocbvcrCBGradSelectedBegin */ Color.FromArgb(255, 223, 154),
            /* msocbvcrCBGradSelectedEnd */ Color.FromArgb(255, 166, 76),
            /* msocbvcrCBGradSelectedMiddle */ Color.FromArgb(255, 195, 116),
            /* msocbvcrCBGradVertBegin */ Color.FromArgb(227, 239, 255),
            /* msocbvcrCBGradVertEnd */ Color.FromArgb(123, 164, 224),
            /* msocbvcrCBGradVertMiddle */ Color.FromArgb(203, 225, 252),
            /* msocbvcrCBIconDisabledDark */ Color.FromArgb(97, 122, 172),
            /* msocbvcrCBIconDisabledLight */ Color.FromArgb(233, 236, 242),
            /* msocbvcrCBLabelBkgnd */ Color.FromArgb(186, 211, 245),
            /* msocbvcrCBLowColorIconDisabled */ Color.FromArgb(109, 150, 208),
            /* msocbvcrCBMainMenuBkgd */ Color.FromArgb(153, 204, 255),
            /* msocbvcrCBMenuBdrOuter */ Color.FromArgb(0, 45, 150),
            /* msocbvcrCBMenuBkgd */ Color.FromArgb(246, 246, 246),
            /* msocbvcrCBMenuCtlText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrCBMenuCtlTextDisabled */ Color.FromArgb(141, 141, 141),
            /* msocbvcrCBMenuIconBkgd */ Color.FromArgb(203, 225, 252),
            /* msocbvcrCBMenuIconBkgdDropped */ Color.FromArgb(172, 183, 201),
            /* msocbvcrCBMenuShadow */ Color.FromArgb(95, 130, 234),
            /* msocbvcrCBMenuSplitArrow */ Color.FromArgb(128, 128, 128),
            /* msocbvcrCBOptionsButtonShadow */ Color.FromArgb(255, 255, 255),
            /* msocbvcrCBShadow */ Color.FromArgb(59, 97, 156),
            /* msocbvcrCBSplitterLine */ Color.FromArgb(106, 140, 203),
            /* msocbvcrCBSplitterLineLight */ Color.FromArgb(241, 249, 255),
            /* msocbvcrCBTearOffHandle */ Color.FromArgb(169, 199, 240),
            /* msocbvcrCBTearOffHandleMouseOver */ Color.FromArgb(255, 238, 194),
            /* msocbvcrCBTitleBkgd */ Color.FromArgb(42, 102, 201),
            /* msocbvcrCBTitleText */ Color.FromArgb(255, 255, 255),
            /* msocbvcrDisabledFocuslessHighlightedText */ Color.FromArgb(172, 168, 153),
            /* msocbvcrDisabledHighlightedText */ Color.FromArgb(187, 206, 236),
            /* msocbvcrDlgGroupBoxText */ Color.FromArgb(0, 70, 213),
            /* msocbvcrDocTabBdr */ Color.FromArgb(0, 53, 154),
            /* msocbvcrDocTabBdrDark */ Color.FromArgb(117, 166, 241),
            /* msocbvcrDocTabBdrDarkMouseDown */ Color.FromArgb(0, 0, 128),
            /* msocbvcrDocTabBdrDarkMouseOver */ Color.FromArgb(0, 0, 128),
            /* msocbvcrDocTabBdrLight */ Color.FromArgb(255, 255, 255),
            /* msocbvcrDocTabBdrLightMouseDown */ Color.FromArgb(0, 0, 128),
            /* msocbvcrDocTabBdrLightMouseOver */ Color.FromArgb(0, 0, 128),
            /* msocbvcrDocTabBdrMouseDown */ Color.FromArgb(0, 0, 128),
            /* msocbvcrDocTabBdrMouseOver */ Color.FromArgb(0, 0, 128),
            /* msocbvcrDocTabBdrSelected */ Color.FromArgb(59, 97, 156),
            /* msocbvcrDocTabBkgd */ Color.FromArgb(186, 211, 245),
            /* msocbvcrDocTabBkgdMouseDown */ Color.FromArgb(254, 128, 62),
            /* msocbvcrDocTabBkgdMouseOver */ Color.FromArgb(255, 238, 194),
            /* msocbvcrDocTabBkgdSelected */ Color.FromArgb(255, 255, 255),
            /* msocbvcrDocTabText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrDocTabTextMouseDown */ Color.FromArgb(0, 0, 0),
            /* msocbvcrDocTabTextMouseOver */ Color.FromArgb(0, 0, 0),
            /* msocbvcrDocTabTextSelected */ Color.FromArgb(0, 0, 0),
            /* msocbvcrDWActiveTabBkgd */ Color.FromArgb(186, 211, 245),
            /* msocbvcrDWActiveTabText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrDWActiveTabTextDisabled */ Color.FromArgb(94, 94, 94),
            /* msocbvcrDWInactiveTabBkgd */ Color.FromArgb(129, 169, 226),
            /* msocbvcrDWInactiveTabText */ Color.FromArgb(255, 255, 255),
            /* msocbvcrDWTabBkgdMouseDown */ Color.FromArgb(254, 128, 62),
            /* msocbvcrDWTabBkgdMouseOver */ Color.FromArgb(255, 238, 194),
            /* msocbvcrDWTabTextMouseDown */ Color.FromArgb(0, 0, 0),
            /* msocbvcrDWTabTextMouseOver */ Color.FromArgb(0, 0, 0),
            /* msocbvcrFocuslessHighlightedBkgd */ Color.FromArgb(236, 233, 216),
            /* msocbvcrFocuslessHighlightedText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrGDHeaderBdr */ Color.FromArgb(89, 89, 172),
            /* msocbvcrGDHeaderBkgd */ Color.FromArgb(239, 235, 222),
            /* msocbvcrGDHeaderCellBdr */ Color.FromArgb(126, 125, 104),
            /* msocbvcrGDHeaderCellBkgd */ Color.FromArgb(239, 235, 222),
            /* msocbvcrGDHeaderCellBkgdSelected */ Color.FromArgb(255, 192, 111),
            /* msocbvcrGDHeaderSeeThroughSelection */ Color.FromArgb(191, 191, 223),
            /* msocbvcrGSPDarkBkgd */ Color.FromArgb(74, 122, 201),
            /* msocbvcrGSPGroupContentDarkBkgd */ Color.FromArgb(185, 208, 241),
            /* msocbvcrGSPGroupContentLightBkgd */ Color.FromArgb(221, 236, 254),
            /* msocbvcrGSPGroupContentText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrGSPGroupContentTextDisabled */ Color.FromArgb(150, 145, 133),
            /* msocbvcrGSPGroupHeaderDarkBkgd */ Color.FromArgb(101, 143, 224),
            /* msocbvcrGSPGroupHeaderLightBkgd */ Color.FromArgb(196, 219, 249),
            /* msocbvcrGSPGroupHeaderText */ Color.FromArgb(0, 45, 134),
            /* msocbvcrGSPGroupline */ Color.FromArgb(255, 255, 255),
            /* msocbvcrGSPHyperlink */ Color.FromArgb(0, 61, 178),
            /* msocbvcrGSPLightBkgd */ Color.FromArgb(221, 236, 254),
            /* msocbvcrHyperlink */ Color.FromArgb(0, 61, 178),
            /* msocbvcrHyperlinkFollowed */ Color.FromArgb(170, 0, 170),
            /* msocbvcrJotNavUIBdr */ Color.FromArgb(59, 97, 156),
            /* msocbvcrJotNavUIGradBegin */ Color.FromArgb(158, 190, 245),
            /* msocbvcrJotNavUIGradEnd */ Color.FromArgb(255, 255, 255),
            /* msocbvcrJotNavUIGradMiddle */ Color.FromArgb(196, 218, 250),
            /* msocbvcrJotNavUIText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrListHeaderArrow */ Color.FromArgb(172, 168, 153),
            /* msocbvcrNetLookBkgnd */ Color.FromArgb(227, 239, 255),
            /* msocbvcrOABBkgd */ Color.FromArgb(128, 128, 128),
            /* msocbvcrOBBkgdBdr */ Color.FromArgb(128, 128, 128),
            /* msocbvcrOBBkgdBdrContrast */ Color.FromArgb(255, 255, 255),
            /* msocbvcrOGMDIParentWorkspaceBkgd */ Color.FromArgb(144, 153, 174),
            /* msocbvcrOGRulerActiveBkgd */ Color.FromArgb(255, 255, 255),
            /* msocbvcrOGRulerBdr */ Color.FromArgb(0, 0, 0),
            /* msocbvcrOGRulerBkgd */ Color.FromArgb(216, 231, 252),
            /* msocbvcrOGRulerInactiveBkgd */ Color.FromArgb(158, 190, 245),
            /* msocbvcrOGRulerTabBoxBdr */ Color.FromArgb(75, 120, 202),
            /* msocbvcrOGRulerTabBoxBdrHighlight */ Color.FromArgb(255, 255, 255),
            /* msocbvcrOGRulerTabStopTicks */ Color.FromArgb(128, 128, 128),
            /* msocbvcrOGRulerText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrOGTaskPaneGroupBoxHeaderBkgd */ Color.FromArgb(186, 211, 245),
            /* msocbvcrOGWorkspaceBkgd */ Color.FromArgb(144, 153, 174),
            /* msocbvcrOLKFlagNone */ Color.FromArgb(242, 240, 228),
            /* msocbvcrOLKFolderbarDark */ Color.FromArgb(0, 53, 145),
            /* msocbvcrOLKFolderbarLight */ Color.FromArgb(89, 135, 214),
            /* msocbvcrOLKFolderbarText */ Color.FromArgb(255, 255, 255),
            /* msocbvcrOLKGridlines */ Color.FromArgb(234, 233, 225),
            /* msocbvcrOLKGroupLine */ Color.FromArgb(123, 164, 224),
            /* msocbvcrOLKGroupNested */ Color.FromArgb(253, 238, 201),
            /* msocbvcrOLKGroupShaded */ Color.FromArgb(190, 218, 251),
            /* msocbvcrOLKGroupText */ Color.FromArgb(55, 104, 185),
            /* msocbvcrOLKIconBar */ Color.FromArgb(253, 247, 233),
            /* msocbvcrOLKInfoBarBkgd */ Color.FromArgb(144, 153, 174),
            /* msocbvcrOLKInfoBarText */ Color.FromArgb(255, 255, 255),
            /* msocbvcrOLKPreviewPaneLabelText */ Color.FromArgb(144, 153, 174),
            /* msocbvcrOLKTodayIndicatorDark */ Color.FromArgb(187, 85, 3),
            /* msocbvcrOLKTodayIndicatorLight */ Color.FromArgb(251, 200, 79),
            /* msocbvcrOLKWBActionDividerLine */ Color.FromArgb(215, 228, 251),
            /* msocbvcrOLKWBButtonDark */ Color.FromArgb(123, 164, 224),
            /* msocbvcrOLKWBButtonLight */ Color.FromArgb(203, 225, 252),
            /* msocbvcrOLKWBDarkOutline */ Color.FromArgb(0, 45, 150),
            /* msocbvcrOLKWBFoldersBackground */ Color.FromArgb(255, 255, 255),
            /* msocbvcrOLKWBHoverButtonDark */ Color.FromArgb(247, 190, 87),
            /* msocbvcrOLKWBHoverButtonLight */ Color.FromArgb(255, 255, 220),
            /* msocbvcrOLKWBLabelText */ Color.FromArgb(50, 69, 105),
            /* msocbvcrOLKWBPressedButtonDark */ Color.FromArgb(248, 222, 128),
            /* msocbvcrOLKWBPressedButtonLight */ Color.FromArgb(232, 127, 8),
            /* msocbvcrOLKWBSelectedButtonDark */ Color.FromArgb(238, 147, 17),
            /* msocbvcrOLKWBSelectedButtonLight */ Color.FromArgb(251, 230, 148),
            /* msocbvcrOLKWBSplitterDark */ Color.FromArgb(0, 53, 145),
            /* msocbvcrOLKWBSplitterLight */ Color.FromArgb(89, 135, 214),
            /* msocbvcrPlacesBarBkgd */ Color.FromArgb(236, 233, 216),
            /* msocbvcrPPOutlineThumbnailsPaneTabAreaBkgd */ Color.FromArgb(195, 218, 249),
            /* msocbvcrPPOutlineThumbnailsPaneTabBdr */ Color.FromArgb(59, 97, 156),
            /* msocbvcrPPOutlineThumbnailsPaneTabInactiveBkgd */ Color.FromArgb(158, 190, 245),
            /* msocbvcrPPOutlineThumbnailsPaneTabText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrPPSlideBdrActiveSelected */ Color.FromArgb(61, 108, 192),
            /* msocbvcrPPSlideBdrActiveSelectedMouseOver */ Color.FromArgb(61, 108, 192),
            /* msocbvcrPPSlideBdrInactiveSelected */ Color.FromArgb(128, 128, 128),
            /* msocbvcrPPSlideBdrMouseOver */ Color.FromArgb(61, 108, 192),
            /* msocbvcrPubPrintDocScratchPageBkgd */ Color.FromArgb(144, 153, 174),
            /* msocbvcrPubWebDocScratchPageBkgd */ Color.FromArgb(189, 194, 207),
            /* msocbvcrSBBdr */ Color.FromArgb(211, 211, 211),
            /* msocbvcrScrollbarBkgd */ Color.FromArgb(251, 251, 248),
            /* msocbvcrToastGradBegin */ Color.FromArgb(220, 236, 254),
            /* msocbvcrToastGradEnd */ Color.FromArgb(167, 197, 238),
            /* msocbvcrWPBdrInnerDocked */ Color.FromArgb(185, 212, 249),
            /* msocbvcrWPBdrOuterDocked */ Color.FromArgb(196, 218, 250),
            /* msocbvcrWPBdrOuterFloating */ Color.FromArgb(42, 102, 201),
            /* msocbvcrWPBkgd */ Color.FromArgb(221, 236, 254),
            /* msocbvcrWPCtlBdr */ Color.FromArgb(127, 157, 185),
            /* msocbvcrWPCtlBdrDefault */ Color.FromArgb(0, 0, 0),
            /* msocbvcrWPCtlBdrDisabled */ Color.FromArgb(128, 128, 128),
            /* msocbvcrWPCtlBkgd */ Color.FromArgb(169, 199, 240),
            /* msocbvcrWPCtlBkgdDisabled */ Color.FromArgb(222, 222, 222),
            /* msocbvcrWPCtlText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrWPCtlTextDisabled */ Color.FromArgb(172, 168, 153),
            /* msocbvcrWPCtlTextMouseDown */ Color.FromArgb(0, 0, 0),
            /* msocbvcrWPGroupline */ Color.FromArgb(123, 164, 224),
            /* msocbvcrWPInfoTipBkgd */ Color.FromArgb(255, 255, 204),
            /* msocbvcrWPInfoTipText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrWPNavBarBkgnd */ Color.FromArgb(74, 122, 201),
            /* msocbvcrWPText */ Color.FromArgb(0, 0, 0),
            /* msocbvcrWPTextDisabled */ Color.FromArgb(172, 168, 153),
            /* msocbvcrWPTitleBkgdActive */ Color.FromArgb(123, 164, 224),
            /* msocbvcrWPTitleBkgdInactive */ Color.FromArgb(148, 187, 239),
            /* msocbvcrWPTitleTextActive */ Color.FromArgb(0, 0, 0),
            /* msocbvcrWPTitleTextInactive */ Color.FromArgb(0, 0, 0),
            /* msocbvcrXLFormulaBarBkgd */ Color.FromArgb(158, 190, 245)
         });

        return FBlueLunaColorTable;
      }
    }
    private static ProfessionalColorTable FBlueLunaColorTable = null;


    /// <summary>
    /// Таблица фиксированных цыетов
    /// </summary>
    private sealed class FixedProfessionalColorTable : ProfessionalColorTable
    {
      public FixedProfessionalColorTable(Color[] Colors)
      {
#if DEBUG
        if (Colors.Length!=(int)(KnownColors.lastKnownColor+1))
          throw new ArgumentException();
#endif
        FColors = Colors;
      }

      private Color[] FColors;

    #region Colors

      //public override Color ButtonSelectedHighlight { get { return FColors[(int)KnownColors.ButtonSelectedHighlight]; } }

      //public override Color ButtonPressedHighlight { get { return FColors[(int)KnownColors.ButtonPressedHighlight]; } }

      //public override Color ButtonCheckedHighlight { get { return FColors[(int)KnownColors.ButtonCheckedHighlight]; } }

      public override Color ButtonPressedBorder { get { return FColors[(int)KnownColors.msocbvcrCBCtlBdrMouseOver]; } }

      public override Color ButtonSelectedBorder { get { return FColors[(int)KnownColors.msocbvcrCBCtlBdrMouseOver]; } }

      public override Color ButtonCheckedGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradSelectedBegin]; } }

      public override Color ButtonCheckedGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradSelectedMiddle]; } }

      public override Color ButtonCheckedGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradSelectedEnd]; } }

      public override Color ButtonSelectedGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseOverBegin]; } }

      public override Color ButtonSelectedGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseOverMiddle]; } }

      public override Color ButtonSelectedGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseOverEnd]; } }

      public override Color ButtonPressedGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseDownBegin]; } }

      public override Color ButtonPressedGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseDownMiddle]; } }

      public override Color ButtonPressedGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseDownEnd]; } }

      public override Color CheckBackground { get { return FColors[(int)KnownColors.msocbvcrCBCtlBkgdSelected]; } }

      public override Color CheckSelectedBackground { get { return FColors[(int)KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver]; } }

      public override Color CheckPressedBackground { get { return FColors[(int)KnownColors.msocbvcrCBCtlBkgdSelectedMouseOver]; } }

      public override Color GripDark { get { return FColors[(int)KnownColors.msocbvcrCBDragHandle]; } }

      public override Color GripLight { get { return FColors[(int)KnownColors.msocbvcrCBDragHandleShadow]; } }

      public override Color ImageMarginGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradVertBegin]; } }

      public override Color ImageMarginGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradVertMiddle]; } }

      public override Color ImageMarginGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradVertEnd]; } }

      public override Color ImageMarginRevealedGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMenuIconBkgdDroppedBegin]; } }

      public override Color ImageMarginRevealedGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle]; } }

      public override Color ImageMarginRevealedGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMenuIconBkgdDroppedEnd]; } }

      public override Color MenuStripGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzBegin]; } }

      public override Color MenuStripGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzEnd]; } }

      public override Color MenuItemSelected { get { return FColors[(int)KnownColors.msocbvcrCBCtlBkgdMouseOver]; } }

      public override Color MenuItemBorder { get { return FColors[(int)KnownColors.msocbvcrCBCtlBdrSelected]; } }

      public override Color MenuBorder { get { return FColors[(int)KnownColors.msocbvcrCBMenuBdrOuter]; } }

      public override Color MenuItemSelectedGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseOverBegin]; } }

      public override Color MenuItemSelectedGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMouseOverEnd]; } }

      public override Color MenuItemPressedGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMenuTitleBkgdBegin]; } }

      public override Color MenuItemPressedGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradMenuIconBkgdDroppedMiddle]; } }

      public override Color MenuItemPressedGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMenuTitleBkgdEnd]; } }

      public override Color RaftingContainerGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzBegin]; } }

      public override Color RaftingContainerGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzEnd]; } }

      public override Color SeparatorDark { get { return FColors[(int)KnownColors.msocbvcrCBSplitterLine]; } }

      public override Color SeparatorLight { get { return FColors[(int)KnownColors.msocbvcrCBSplitterLineLight]; } }

      public override Color StatusStripGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzBegin]; } }

      public override Color StatusStripGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzEnd]; } }

      public override Color ToolStripBorder { get { return FColors[(int)KnownColors.msocbvcrCBShadow]; } }

      public override Color ToolStripDropDownBackground { get { return FColors[(int)KnownColors.msocbvcrCBMenuBkgd]; } }

      public override Color ToolStripGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradVertBegin]; } }

      public override Color ToolStripGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradVertMiddle]; } }

      public override Color ToolStripGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradVertEnd]; } }

      public override Color ToolStripContentPanelGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzBegin]; } }

      public override Color ToolStripContentPanelGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzEnd]; } }

      public override Color ToolStripPanelGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzBegin]; } }

      public override Color ToolStripPanelGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradMainMenuHorzEnd]; }      }

      public override Color OverflowButtonGradientBegin { get { return FColors[(int)KnownColors.msocbvcrCBGradOptionsBegin]; } }

      public override Color OverflowButtonGradientMiddle { get { return FColors[(int)KnownColors.msocbvcrCBGradOptionsMiddle]; } }

      public override Color OverflowButtonGradientEnd { get { return FColors[(int)KnownColors.msocbvcrCBGradOptionsEnd]; } }

    #endregion Colors
    }
#endif
    #endregion

    #region Для ImageList

    /// <summary>
    /// Копирование изображений
    /// </summary>
    /// <param name="srcImages">Исходный список</param>
    /// <param name="dstImages">Заполняемый список</param>
    /// <param name="replace">Определяет действие, когда есть изображения с совпадающими ключами.
    /// Если true, то исходное изображение будет заменено новым.
    /// Если false, то в <paramref name="dstImages"/> будет сохранено существующее изображение</param>
    public static void CopyImages(ImageList srcImages, ImageList dstImages, bool replace)
    {
#if DEBUG
      if (srcImages == null)
        throw new ArgumentNullException("srcImages");
      if (dstImages == null)
        throw new ArgumentNullException("dstImages");
#endif
      if (Object.ReferenceEquals(srcImages, dstImages))
        throw new ArgumentException("Списки не могут совпадать");

      for (int i = 0; i < srcImages.Images.Count; i++)
      {
        string Key = srcImages.Images.Keys[i];
        if (dstImages.Images.ContainsKey(Key))
        {
          if (replace)
            dstImages.Images.RemoveByKey(Key);
          else
            continue;
        }
        dstImages.Images.Add(Key, srcImages.Images[i]);
      }
    }

    #endregion

    #region Значок приложения

    /// <summary>
    /// Возвращает значок текущего приложения.
    /// Использует Icon.ExtractAssociatedIcon() для извлечения значка exe-файла.
    /// Путь к приложению возвращается свойством FileTools.ApplicationPath.
    /// Загруженный значок буферизуется.
    /// При отсутствии значка приложения или в случае ошибки загрузки, свойство возвращает null.
    /// </summary>
    public static Icon AppIcon
    {
      get
      {
        Icon Res;
        lock (WinFormsTools.InternalSyncRoot)
        {
          if (!_AppIconDefined)
          {
            _AppIconDefined = true;
            try
            {
              //FAppIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
              if (!FileTools.ApplicationPath.IsEmpty)
                _AppIcon = Icon.ExtractAssociatedIcon(FileTools.ApplicationPath.Path); // 10.04.2015
            }
            catch
            {
            }
          }
          Res = _AppIcon;
        }
        return Res;
      }
    }
    private static Icon _AppIcon = null;
    private static bool _AppIconDefined = false;

    /// <summary>
    /// Инициализирует значок формы равным значку приложения.
    /// Если свойство AppIcon возвращает null, устанавливается свойство <paramref name="form"/>.ShowIcon=false
    /// для скрытия значка.
    /// </summary>
    /// <param name="form">Форма, для которой устанавливается значок</param>
    public static void InitAppIcon(Form form)
    {
      if (AppIcon == null)
        form.ShowIcon = false;
      else
      {
        form.Icon = AppIcon;
        form.ShowIcon = true;
      }
    }

    #endregion

    #region Значки других приложений

    /// <summary>
    /// Получить значок требуемого размера из ресурсов файла.
    /// Если для значка нет требуемого размера, возвращается значок другого размера.
    /// Если файл не найден или в файле нет значка с заданным индексом, возвращается null.
    /// Для платформ, отличных от Windows, всегда возвращает null.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="iconIndex">Индекс значка в файле. 
    /// См. описание функции Windows ExtractIcon или ExtractIconEx()</param>
    /// <param name="smallIcon">true - извлечь маленький значок (16x16), false - больщой (32x32)</param>
    /// <returns>Значок или null</returns>
    public static Icon ExtractIcon(AbsPath filePath, int iconIndex, bool smallIcon)
    {
      if (!System.IO.File.Exists(filePath.Path))
        return null;

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          break;
        default:
          return null;
      }

      IntPtr HIcon;
      if (smallIcon)
      {
        HIcon = DoExtractSmallIcon(filePath, iconIndex);
        if (HIcon == IntPtr.Zero)
          HIcon = DoExtractLargeIcon(filePath, iconIndex);
      }
      else
      {
        HIcon = DoExtractLargeIcon(filePath, iconIndex);
        if (HIcon == IntPtr.Zero)
          HIcon = DoExtractSmallIcon(filePath, iconIndex);
      }
      if (HIcon == IntPtr.Zero)
        return null;

      // К сожалению, нельзя вернуть значок, чтобы Net Framework отвечал за вызов DestroyIcon,
      // т.к. соответствующий конструктор защищенный.
      // Надо клонировать

      Icon Icon1 = Icon.FromHandle(HIcon);
      Icon Icon2 = Icon1.Clone() as Icon;
      Icon1.Dispose();
      NativeMethods.DestroyIcon(HIcon);
      return Icon2;
    }

    private static IntPtr DoExtractLargeIcon(AbsPath filePath, int iconIndex)
    {
      return NativeMethods.ExtractIcon(IntPtr.Zero, filePath.Path, iconIndex);
    }

    private static IntPtr DoExtractSmallIcon(AbsPath filePath, int iconIndex)
    {
      IntPtr[] SmallIcons = new IntPtr[1] { IntPtr.Zero };
      NativeMethods.ExtractIconEx(filePath.Path, iconIndex, null, SmallIcons, 1);
      return SmallIcons[0];
    }

    private static class NativeMethods
    {

      [DllImport("shell32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr ExtractIcon(IntPtr hInst, string szFileName, int nIconIndex);

      [DllImport("shell32.dll", CharSet = CharSet.Auto)]
      public static extern uint ExtractIconEx(string szFileName, int nIconIndex,
         IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool DestroyIcon(IntPtr hIcon);
    }


    /// <summary>
    /// Получить значок требуемого размера из ресурсов файла для объекта FileAssociationItem.
    /// Если файл не найден или в файле нет значка с заданным индексом, возвращается null.
    /// В отличие от ExtractIcon(), выполняет также загрузку значков из файлов изображений,
    /// и выполняет уменьшение размера
    /// Если для значка нет требуемого размера, возвращается значок меньшего размера.
    /// </summary>
    /// <param name="faItem">Ассоциация файла</param>
    /// <param name="smallIcon">true - извлечь маленький значок (16x16), false - больщой (32x32)</param>
    /// <returns>Значок или null</returns>
    public static Image ExtractIconImage(FreeLibSet.Shell.FileAssociationItem faItem, bool smallIcon)
    {
      if (faItem == null)
        return null;
      return ExtractIconImage(faItem.IconPath, faItem.IconIndex, smallIcon);
    }

    /// <summary>
    /// Получить значок требуемого размера из ресурсов файла.
    /// Если файл не найден или в файле нет значка с заданным индексом, возвращается null.
    /// В отличие от ExtractIcon(), выполняет также загрузку значков из файлов изображений,
    /// и выполняет уменьшение размера
    /// Если для значка нет требуемого размера, возвращается значок меньшего размера.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="iconIndex">Индекс значка в файле. 
    /// См. описание функции Windows ExtractIcon или ExtractIconEx()</param>
    /// <param name="smallIcon">true - извлечь маленький значок (16x16), false - больщой (32x32)</param>
    /// <returns>Значок или null</returns>
    public static Image ExtractIconImage(AbsPath filePath, int iconIndex, bool smallIcon)
    {
      if (filePath.IsEmpty)
        return null;

      Size MaxSize = smallIcon ? new Size(16, 16) : new Size(32, 32);

      switch (filePath.Extension.ToUpperInvariant())
      {
        case ".PNG":
        case ".BMP":
        case ".JPG":
        case ".JPEG":
        case ".TIF":
        case ".TIFF":
        case ".GIF":
          // ?? другие типы файлов
          Image img = Image.FromFile(filePath.Path);
          return CreateThumbnailImage(img, MaxSize);
      }

      Icon Icon = ExtractIcon(filePath, iconIndex, smallIcon);
      if (Icon == null)
        return null;

      Image img2 = CreateThumbnailImage(Icon.ToBitmap(), MaxSize);
      Icon.Dispose();
      return img2;
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Получить текст в виде массива строк и столбцов.
    /// Извлекается текст в формате CSV или обычный текст
    /// </summary>
    /// <returns></returns>
    public static string[,] GetClipboardStringArray2()
    {
      string s;

      // 10.09.2012
      // Сначала - текст, затем - csv

      s = EFPApp.Clipboard.GetText();
      if (EFPApp.Clipboard.HasError)
        return null;
      if (!String.IsNullOrEmpty(s))
      {
        try
        {
          return DataTools.TabbedStringToArray2(s);
        }
        catch (Exception e)
        {
          throw new ParsingException("Ошибка преобразования текста из буфера обмена. " + e.Message, e);
        }
      }

      s = EFPApp.Clipboard.GetData("Csv") as String;
      if (EFPApp.Clipboard.HasError)
        return null;
      if (!String.IsNullOrEmpty(s))
      {
        try
        {
          try
          {
            // Формат RFC 4180
            return DataTools.CommaStringToArray2(s);
          }
          catch
          {
            // Формат Excel
            return DataTools.CommaStringToArray2(s, ';');
          }
        }
        catch (Exception e)
        {
          throw new ParsingException("Ошибка преобразования текста CSV из буфера обмена. " + e.Message);
        }
      }
      return null;
    }

    #endregion

    #region Преобразование чисел

    /// <summary>
    /// Корректировка строки, содержащей число с плавающей точкой, перед преобразованием
    /// float/double/decimal.TryParse().
    /// Выполняет замену символов "." и "," в зависимости от значения DecimalSeparator
    /// Также убираются пробелы
    /// </summary>
    /// <param name="s">Строка, которая будет преобразовываться</param>
    /// <param name="nfi">Объект, содержщий параметры форматирования</param>
    public static void CorrectNumberString(ref string s, NumberFormatInfo nfi)
    {
      if (String.IsNullOrEmpty(s))
        return;

      if (nfi == null)
        nfi = NumberFormatInfo.CurrentInfo;
      switch (nfi.NumberDecimalSeparator)
      {
        case ",":
          s = s.Replace('.', ',');
          break;
        case ".":
          s = s.Replace(',', '.');
          break;
      }

      s = s.Replace(" ", "");
      s = s.Replace(DataTools.NonBreakSpaceStr, "");
    }

    /// <summary>
    /// Корректировка строки, содержащей число с плавающей точкой, перед преобразованием
    /// float/double/decimal.TryParse().
    /// Выполняет замену символов "." и "," в зависимости от значения DecimalSeparator
    /// Также убираются пробелы.
    /// </summary>
    /// <param name="s">Строка, которая будет преобразовываться</param>
    public static void CorrectNumberString(ref string s)
    {
      CorrectNumberString(ref s, NumberFormatInfo.CurrentInfo);
    }

    /// <summary>
    /// Корректировка строки, содержащей число с плавающей точкой, перед преобразованием
    /// float/double/decimal.TryParse().
    /// Выполняет замену символов "." и "," в зависимости от значения DecimalSeparator
    /// Также убираются пробелы
    /// </summary>
    /// <param name="s">Строка, которая будет преобразовываться</param>
    /// <param name="formatProvider">Форматизатор</param>
    public static void CorrectNumberString(ref string s, IFormatProvider formatProvider)
    {
      if (formatProvider == null)
        CorrectNumberString(ref s);
      else
      {
        NumberFormatInfo nfi = formatProvider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
        CorrectNumberString(ref s, nfi);
      }

    }

    #endregion

    #region Преобразование строк

    /// <summary>
    /// Преобразование ошибочно введенных латинских символов в русские и наоборот
    /// </summary>
    /// <param name="s">Исходная строка</param>
    /// <returns>Преобразованная строка</returns>
    public static string ChangeRusLat(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      //                <   Ряд 1  ><  Ряд 2  >< Ряд 3  >
      string ConvS1 = "`qwertyuiop[]asdfghjkl;'zxcvbnm,./" +
                      "~QWERTYUIOP{}ASDFGHJKL:\"ZXCVBNM<>" +
                      "ёйцукенгшщзхъфывапролджэячсмитьбю." +
                      "ЁЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,";

      string ConvS2 = "ёйцукенгшщзхъфывапролджэячсмитьбю." +
                      "ЁЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ," +
                      "`qwertyuiop[]asdfghjkl;'zxcvbnm,./" +
                      "~QWERTYUIOP{}ASDFGHJKL:\"ZXCVBNM<>";

      char[] a = new char[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        int p = ConvS1.IndexOf(s[i]);
        if (p >= 0)
          a[i] = ConvS2[p];
        else
          a[i] = s[i];
      }
      return new string(a);
    }

    #endregion

    #region Поддержка Windows-98

    /// <summary>
    /// Корректировка кода символа, полученного в событии KeyPress.
    /// Под Windows-98 возвращаются неправильные коды для русских букв
    /// (используется ANSI-кодировка, а не UNICODE)
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    internal static char CorrectCharWin98(char c)
    {
      int n = (int)c;
      if (n >= 192 && n <= 223)
        return (char)((int)'А' + n - 192);
      if (n >= 224 && n <= 255)
        return (char)((int)'а' + n - 224);
      switch (n)
      {
        case 168:
          return 'Ё';
        case 184:
          return 'ё';
        case 185:
          return '№';
      }
      return c;
    }

    #endregion
  }
}