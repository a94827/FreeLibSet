using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace FreeLibSet.OLE.Word
{
  /// <summary>
  /// Статические методы преобразования данных WinForms/Graphics в Excel
  /// </summary>
  public static class WordWinFormsTools
  {
    #region Преобразование цвета

    /// <summary>
    /// Преобразование объекта ColorEx в RGB-представление
    /// Для ColorEx.Empty возвращает автоматический цвет
    /// </summary>
    /// <param name="value">Цвет в .Net framework</param>
    /// <returns>Цвет в Microsoft Excel</returns>
    public static Int32 ColorToRgb(Color value)
    {
      if (value.IsEmpty)
        return WordHelper.wdColorAutomatic;
#if DEBUG
      if (value.IsSystemColor)
        throw new ArgumentException("Системные цвета недопустимы", "value");
#endif
      return (((int)(value.B)) << 16) | (((int)(value.G)) << 8) | ((int)(value.R));
    }

    /// <summary>
    /// Преобразование RGB-цвета в обычный ColorEx
    /// </summary>
    /// <param name="value">Цвет в Microsoft Excel</param>
    /// <returns>Цвет в .Net framework</returns>
    public static Color RgbToColor(Int32 value)
    {
      if (value == WordHelper.wdColorAutomatic)
        return Color.Empty;
      int r = value & 0x000000FF;
      int g = (value & 0x0000FF00) >> 8;
      int b = (value & 0x00FF0000) >> 16;
      return Color.FromArgb(r, g, b);
    }

    #endregion
  }
}
