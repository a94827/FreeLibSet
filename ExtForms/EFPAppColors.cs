using System;
using System.Collections.Generic;
using System.Text;
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
  /// Дополнительные цвета для раскрашивания табличных просмотров и других
  /// управляющих элементов.
  /// Цвета, которых не хватает в SystemColors
  /// Реализация свойства EFPApp.Colors
  /// 
  /// В текущей реализации не предусмотрена настройка цветов
  /// </summary>
  public class EFPAppColors
  {
    #region Конструктор

    internal EFPAppColors()
      :this(false)
    { 
    }

    internal EFPAppColors(bool ignoreSysColors)
    {
      #region Метки

      LabelErrorForeColor = Color.Red;
      LabelWarningForeColor = Color.Magenta;

      #endregion

      #region Табличный просмотр

      GridHeaderForeColor = Color.Navy;
      GridHeaderBackColor = Color.White;

      GridAlterBackColor = Color.FromArgb(240, 240, 255);
      GridSpecialBackColor = Color.FromArgb(255, 255, 192);

      GridTotal1BackColor = Color.FromArgb(192, 255, 192);
      GridTotal2BackColor = Color.FromArgb(255, 192, 255);

      if (!ignoreSysColors)
      {
        GridAlterBackColor = FromWhite(SystemColors.Window, GridAlterBackColor);
        GridSpecialBackColor = FromWhite(SystemColors.Window, GridSpecialBackColor);

        GridTotal1BackColor = FromWhite(SystemColors.Window, GridTotal1BackColor);
        GridTotal2BackColor = FromWhite(SystemColors.Window, GridTotal2BackColor);
      }

      GridTotalRowForeColor = Color.Black;
      GridTotalRowBackColor = Color.FromArgb(212, 212, 212);
      //GridTotalRowForeColor = SystemColors.ControlText;
      //GridTotalRowBackColor = SystemColors.ControlLight;

      GridErrorForeColor = Color.White;
      GridErrorBackColor = Color.Red;

      GridWarningForeColor = Color.Maroon;
      GridWarningBackColor = Color.Yellow;

      #endregion

      #region Цвета для элементов списка

      if (ignoreSysColors)
      {
        ListStateOk = new ListItemColors(Color.White, Color.Black, Color.Black, Color.White);

        ListStateError = new ListItemColors(Color.White, Color.Red, Color.Black, Color.LightPink);

        ListStateWarning = new ListItemColors(Color.White, Color.Magenta, Color.Black, Color.LightPink);
      }
      else
      {
        ListStateOk = new ListItemColors(SystemColors.Window, SystemColors.WindowText, SystemColors.Highlight, SystemColors.HighlightText);

        ListStateError = new ListItemColors(SystemColors.Window, Color.Red, SystemColors.Highlight, FromWhite(SystemColors.HighlightText, Color.LightPink));

        ListStateWarning = new ListItemColors(SystemColors.Window, Color.Magenta, SystemColors.Highlight, FromWhite(SystemColors.HighlightText, Color.Magenta));
      }

      ListAlter = new ListItemColors(GridAlterBackColor, ListStateOk.ForeColor, ListStateOk.SelectedBackColor, ListStateOk.SelectedForeColor);

      #endregion
    }

    private static Color FromWhite(Color sysColor, Color diffColor)
    {
      #region 1. Степень затемнения

      int dR = 255 - diffColor.R;
      int dG = 255 - diffColor.G;
      int dB = 255 - diffColor.B;
      int dMax = Math.Max(dR, Math.Max(dG, dB));

      #endregion

      #region 2. Корректировка темного фона

      int SysMax = Math.Max(sysColor.R, Math.Max(sysColor.G, sysColor.B));
      int k = 256;
      if (SysMax < 32)
        k = 512;
      if (SysMax < 16)
        k = 1024;
      if (dMax * k / 256 > 64)
        k = 64 * 256 / dMax;

      dR *= k / 256;
      dG *= k / 256;
      dB *= k / 256;

      #endregion

      #region 3. Проверяем, что применение затемнения не сделает цвет отрицательным

      int R = sysColor.R;
      int G = sysColor.G;
      int B = sysColor.B;

      int Off = 0;
      if (R - dR < 0)
        Off = Math.Max(Off, dR - R);
      if (G - dG < 0)
        Off = Math.Max(Off, dG - G);
      if (B - dB < 0)
        Off = Math.Max(Off, dB - B);

      #endregion

      #region 4. Сдвигаем системный цвет в сторону белого

      R += Off;
      G += Off;
      B += Off;

      #endregion

      #region 5. Применяем затемнение

      R -= dR;
      G -= dG;
      B -= dB;

      #endregion

      #region 6. Корректировка 0-255

      if (R < 0)
        R = 0;
      if (R > 255)
        R = 255;

      if (G < 0)
        G = 0;
      if (G > 255)
        G = 255;

      if (B < 0)
        B = 0;
      if (B > 255)
        B = 255;

      #endregion

      return Color.FromArgb(R, G, B);
    }

    #endregion

    #region Цвета для меток в форме

    /// <summary>
    /// Цвет шрифта управляющего элемента (обычно метки), для которого показывается ошибка (красный)
    /// </summary>
    public readonly Color LabelErrorForeColor;

    /// <summary>
    /// Цвет шрифта управляющего элемента (обычно метки), для которого показывается предупреждение (сиреневый)
    /// </summary>
    public readonly Color LabelWarningForeColor;

    #endregion

    #region Цвета для табличного просмотра

    /// <summary>
    /// Цвет шрифта строк подзаголовков в табличном просмотре (синий)
    /// </summary>
    public readonly Color GridHeaderForeColor;

    /// <summary>
    /// Цвет фона строк подзаголовков в табличном просмотре (белый)
    /// </summary>
    public readonly Color GridHeaderBackColor;

    /// <summary>
    /// Цвет фона строк при полосатой раскраске табличного просмотра (светло-голубой).
    /// Используется основной цвет шрифта.
    /// </summary>
    public readonly Color GridAlterBackColor;

    /// <summary>
    /// Цвет фона строк для выделенных строк табличного просмотра (серо-желтый).
    /// Используется основной цвет шрифта.
    /// </summary>
    public readonly Color GridSpecialBackColor;

    /// <summary>
    /// Цвет фона строк подытогов первого уровня табличного просмотра (светло-зеленый).
    /// Используется основной цвет шрифта.
    /// </summary>
    public readonly Color GridTotal1BackColor;

    /// <summary>
    /// Цвет фона строк подытогов второго уровня табличного просмотра (светло-сиреневый).
    /// Используется основной цвет шрифта.
    /// </summary>
    public readonly Color GridTotal2BackColor;

    /// <summary>
    /// Цвет шрифта строки итогов табличного просмотра (черный).
    /// </summary>
    public readonly Color GridTotalRowForeColor;

    /// <summary>
    /// Цвет фона строки итогов табличного просмотра (серый).
    /// </summary>
    public readonly Color GridTotalRowBackColor;

    /// <summary>
    /// Цвет шрифта ячеек с ошибками в табличном просмотре (белый).
    /// </summary>
    public readonly Color GridErrorForeColor;

    /// <summary>
    /// Цвет фона ячеек с ошибками в табличном просмотре (красный).
    /// </summary>
    public readonly Color GridErrorBackColor;

    /// <summary>
    /// Цвет шрифта ячеек с предупреждениями в табличном просмотре (черный).
    /// </summary>
    public readonly Color GridWarningForeColor;

    /// <summary>
    /// Цвет фона ячеек с предупреждениями в табличном просмотре (желтый).
    /// </summary>
    public readonly Color GridWarningBackColor;

    #endregion

    #region Цвета для элементов списка

    /// <summary>
    /// Использовать стандартные цвета для EFPValidateState.Ok.
    /// Отличается от Empty, т.к. игнорирует цвета Control.ForeColor и Control.BackColor
    /// </summary>
    public readonly ListItemColors ListStateOk;

    /// <summary>
    /// Элемент с ошибкой EFPValidateState.Error
    /// </summary>
    public readonly ListItemColors ListStateError;

    /// <summary>
    /// Элемент с предупреждением EFPValidateState.Warning
    /// </summary>
    public readonly ListItemColors ListStateWarning;

    /// <summary>
    /// Элемент альтернативного цвета
    /// </summary>
    public readonly ListItemColors ListAlter;

    #endregion
  }
}
