using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace FreeLibSet.OLE.Excel
{
  /// <summary>
  /// Статические методы преобразования данных WinForms/Graphics в Excel
  /// </summary>
  public static class ExcelWinFormsTools
  {
    #region Цветовая палитра Microsoft Excel

    // Реализация (с изменениями) взята из
    // http://www.codeproject.com/Articles/21475/TblProc-OpenOffice-Calc-and-Excel

    // Список цветов палитры может быть взят из
    // http://dmcritchie.mvps.org/excel/colors.htm



    /// <summary>
    /// 56-цветная палитра, используемая Microsoft Excel
    /// </summary>
    public static readonly Color[] ExcelColorPalette = CreateExcelColorPalette();

    private static Color[] CreateExcelColorPalette()
    {
      // Цвета в формате Color.Argb. 
      // В отличие от Excel, здесь старшие разряды - красный, а младший - синий
      int[] RGBs = new int[56] {
        // Цвета из палитры VGA ()
        0x000000, //  0 Black
        0xFFFFFF, //  1 White
        0xFF0000, //  2 Red
        0x00FF00, //  3 Green
        0x0000FF, //  4 Blue
        0xFFFF00, //  5
        0xFF00FF, //  6
        0x00FFFF, //  7
        0x800000, //  8
        0x008000, //  9
        0x000080, // 10
        0x808000, // 11
        0x800080, // 12
        0x008080, // 13
        0xC0C0C0, // 14
        0x808080, // 15

        0x9999FF, // 16
        0x993366, // 17
        0xFFFFCC, // 18
        0xCCFFFF, // 19
        0x660066, // 20
        0xFF8080, // 21
        0x0066CC, // 22
        0xCCCCFF, // 23
        0x000080, // 24
        0xFF00FF, // 25
        0xFFFF00, // 26
        0x00FFFF, // 27
        0x800080, // 28
        0x800000, // 29
        0x008080, // 30
        0x0000FF, // 31
        0x00CCFF, // 32
        0xCCFFFF, // 33  // Чем отличается от [19]?
        0xCCFFCC, // 34
        0xFFFF99, // 35
        0x99CCFF, // 36
        0xFF99CC, // 37
        0xCC99FF, // 38
        0xFFCC99, // 39
        0x3366FF, // 40
        0x33CCCC, // 41
        0x99CC00, // 42
        0xFFCC00, // 43
        0xFF9900, // 44
        0xFF6600, // 45
        0x666699, // 46
        0x969696, // 47
        0x003366, // 48
        0x339966, // 49
        0x003300, // 50
        0x333300, // 51
        0x993300, // 52
        0x993366, // 53
        0x333399, // 54
        0x333333  // 55
      };

      Color[] a = new Color[56];
      for (int i = 0; i < RGBs.Length; i++)
      {
        unchecked
        {
          a[i] = Color.FromArgb((int)0xFF000000 | RGBs[i]);
        }
      }
      return a;
    }

    /// <summary>
    /// Возвращает индекс ближайшего цвета из 56-цветной палитры Excel.
    /// Возвращаемое значение лежит в диапазоне от 0 до 55.
    /// Всегда возвращается какой-нибудь действительный индекс.
    /// </summary>
    /// <param name="color">Исходный цвет</param>
    /// <returns>Индекс ближайшего цвета в палитре <see cref="ExcelColorPalette"/></returns>
    public static int FindExcelClosestColorIndex(Color color)
    {
      int minDistance = int.MaxValue;
      int ret = 0;
      for (int i = 0; i < ExcelColorPalette.Length; i++)
      {
        int dist = ColorDistance(color, ExcelColorPalette[i]);
        if (dist < minDistance)
        {
          minDistance = dist;
          ret = i;
        }
      }
      return ret;
    }

    private static int ColorDistance(Color color1, Color color2)
    {
      return Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B);
    }

    /// <summary>
    /// Возвращает ближайший цвет из 56-цветной палитры Excel
    /// </summary>
    /// <param name="color">Исходный цвет</param>
    /// <returns>Цвет из палитры <see cref="ExcelColorPalette"/></returns>
    public static Color FindExcelClosestColor(Color color)
    {
      return ExcelColorPalette[FindExcelClosestColorIndex(color)];
    }

    #endregion

    #region Преобразование цвета

    /// <summary>
    /// Преобразование объекта <see cref="System.Drawing.Color"/> в RGB-представление
    /// </summary>
    /// <param name="value">Цвет в .Net framework</param>
    /// <returns>Цвет в Microsoft Excel</returns>
    public static Int32 ColorToRgb(Color value)
    {
#if DEBUG
      if (value.IsSystemColor)
        throw new ArgumentException(Res.WinFormsTools_Arg_SystemColor, "value");
#endif
      return (((int)(value.B)) << 16) | (((int)(value.G)) << 8) | ((int)(value.R));
    }

    /// <summary>
    /// Преобразование RGB-цвета Excel в <see cref="System.Drawing.Color"/>
    /// </summary>
    /// <param name="Value">Цвет в Microsoft Excel</param>
    /// <returns>Цвет в .Net framework</returns>
    public static Color RgbToColor(Int32 Value)
    {
      int r = Value & 0x000000FF;
      int g = (Value & 0x0000FF00) >> 8;
      int b = (Value & 0x00FF0000) >> 16;
      return Color.FromArgb(r, g, b);
    }

    #endregion

    #region Преобразование выравнивания

    /// <summary>
    /// Преобразование выравнивания ячейки таблицы <see cref="DataGridView"/> в горизонтальное выравнивание Excel
    /// </summary>
    /// <param name="align">Выравнивание <see cref="DataGridView"/></param>
    /// <returns>Выравнивание Excel</returns>
    public static XlHAlign GridToHAlign(DataGridViewContentAlignment align)
    {
      switch (align)
      {
        case DataGridViewContentAlignment.TopLeft:
        case DataGridViewContentAlignment.MiddleLeft:
        case DataGridViewContentAlignment.BottomLeft:
          return XlHAlign.xlHAlignLeft;

        case DataGridViewContentAlignment.TopCenter:
        case DataGridViewContentAlignment.MiddleCenter:
        case DataGridViewContentAlignment.BottomCenter:
          return XlHAlign.xlHAlignCenter;
        default:
          return XlHAlign.xlHAlignRight;
      }
    }

    /// <summary>
    /// Преобразование выравнивания ячейки таблицы <see cref="DataGridView"/> в вертикальное выравнивание Excel
    /// </summary>
    /// <param name="align">Выравнивание <see cref="DataGridView"/></param>
    /// <returns>Выравнивание Excel</returns>
    public static XlVAlign GridToVAlign(DataGridViewContentAlignment align)
    {
      switch (align)
      {
        case DataGridViewContentAlignment.TopLeft:
        case DataGridViewContentAlignment.TopCenter:
        case DataGridViewContentAlignment.TopRight:
          return XlVAlign.xlVAlignTop;
        case DataGridViewContentAlignment.MiddleLeft:
        case DataGridViewContentAlignment.MiddleCenter:
        case DataGridViewContentAlignment.MiddleRight:
          return XlVAlign.xlVAlignCenter;
        default:
          return XlVAlign.xlVAlignBottom;
      }
    }

    #endregion
  }
}
