// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Дополнительные цвета для раскрашивания табличных просмотров и других
  /// управляющих элементов.
  /// Цвета, которых не хватает в <see cref="SystemColors"/>.
  /// Реализация свойства <see cref="EFPApp.Colors"/>.
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

      int sysMax = Math.Max(sysColor.R, Math.Max(sysColor.G, sysColor.B));
      int k = 256;
      if (sysMax < 32)
        k = 512;
      if (sysMax < 16)
        k = 1024;
      if (dMax * k / 256 > 64)
        k = 64 * 256 / dMax;

      dR *= k / 256;
      dG *= k / 256;
      dB *= k / 256;

      #endregion

      #region 3. Проверяем, что применение затемнения не сделает цвет отрицательным

      int r = sysColor.R;
      int g = sysColor.G;
      int b = sysColor.B;

      int off = 0;
      if (r - dR < 0)
        off = Math.Max(off, dR - r);
      if (g - dG < 0)
        off = Math.Max(off, dG - g);
      if (b - dB < 0)
        off = Math.Max(off, dB - b);

      #endregion

      #region 4. Сдвигаем системный цвет в сторону белого

      r += off;
      g += off;
      b += off;

      #endregion

      #region 5. Применяем затемнение

      r -= dR;
      g -= dG;
      b -= dB;

      #endregion

      #region 6. Корректировка 0-255

      if (r < 0)
        r = 0;
      if (r > 255)
        r = 255;

      if (g < 0)
        g = 0;
      if (g > 255)
        g = 255;

      if (b < 0)
        b = 0;
      if (b > 255)
        b = 255;

      #endregion

      return Color.FromArgb(r, g, b);
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
    /// Стандартные цвета элемента списка для <see cref="UIValidateState.Ok"/>.
    /// Используется <see cref="ListControlImagePainter"/>.
    /// </summary>
    public readonly ListItemColors ListStateOk;

    /// <summary>
    /// Элемент списка с ошибкой <see cref="UIValidateState.Error"/>.
    /// Используется <see cref="ListControlImagePainter"/>.
    /// </summary>
    public readonly ListItemColors ListStateError;

    /// <summary>
    /// Элемент списка с предупреждением <see cref="UIValidateState.Warning"/>.
    /// Используется <see cref="ListControlImagePainter"/>.
    /// </summary>
    public readonly ListItemColors ListStateWarning;

    /// <summary>
    /// Элемент списка альтернативного цвета.
    /// Используется <see cref="ListControlImagePainter"/>.
    /// </summary>
    public readonly ListItemColors ListAlter;

    #endregion
  }
}
