// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Drawing;
using System.Diagnostics;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Интерфейс измерителя размеров элементов табличного просмотра.
  /// Не предназначен для использования в прикладном коде.
  /// </summary>
  public interface IEFPGridControlMeasures
  {

    /// <summary>
    /// Получить ширину столбца в пискелях для текстового поля
    /// </summary>
    /// <param name="textWidth"></param>
    /// <returns></returns>
    int GetTextColumnWidth(double textWidth);

    /// <summary>
    /// Определить число символов, которые можно отобразить в поле заданной ширины
    /// </summary>
    /// <param name="width">Ширина в пикселях</param>
    /// <returns></returns>
    double GetColumnWidthChars(int width);

    /// <summary>
    /// Ширина по умолчанию для столбца CheckBoxColumn
    /// </summary>
    int CheckBoxColumnWidth { get;}

    /// <summary>
    /// Ширина по умолчанию для столбца ImageColumn
    /// </summary>
    int ImageColumnWidth { get;}
  }

  /// <summary>
  /// Свойство EFPDataGridView.Measures
  /// Свойства этого класса предназначены для правильного
  /// определения размера создаваемых столбцов.
  /// Не предназначен для использования в прикладном коде.
  /// </summary>
  public sealed class EFPDataGridViewMeasures : IEFPGridControlMeasures
  {
    #region Конструктор

    internal EFPDataGridViewMeasures(EFPDataGridView controlProvider)
    {
      _ControlProvider = controlProvider;
      Graphics gr = controlProvider.Control.CreateGraphics();
      try
      {
        _DpiX = gr.DpiX;
        _DpiY = gr.DpiY;
        _CharWidthPixel = ExtTextRenderer.CalcDefaultFontWidth(gr, controlProvider.Control.DefaultCellStyle.Font) * DpiX / 72.0;
      }
      finally
      {
        gr.Dispose();
      }

#if DEBUG
      Debug.Assert(_DpiX > 0.0);
      Debug.Assert(_DpiY > 0.0);
      Debug.Assert(_CharWidthPixel > 0.0);
#endif
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private readonly EFPDataGridView _ControlProvider;

    /// <summary>
    /// Горизонтальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiX { get { return _DpiX; } }
    private readonly float _DpiX;

    /// <summary>
    /// Вертикальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiY { get { return _DpiY; } }
    private readonly float _DpiY;

    #endregion

    #region Высота строк

    /// <summary>
    /// Возвращает число пикселей для высоты строки (свойство DataGridViewRow.Height), которая должна отображать заданное
    /// число строк текста
    /// Если запрошена высота меньше, чем для одной строки, возвращается высота для
    /// одной строки
    /// </summary>
    /// <param name="textRowCount">Количество строк текста, которые должны помещаться</param>
    /// <returns>Высота строки в пикселах</returns>
    public int SetTextRowHeight(int textRowCount)
    {
      if (textRowCount < 1)
        textRowCount = 1;

      int FH=ControlProvider.Control.DefaultCellStyle.Font.Height;
      return FH * textRowCount + 9;
    }

    /// <summary>
    /// Возвращает число строк текста, которые можно отобразить в строке заданной высоты
    /// Если высота строки недостаточна для отображения одной строки, возвращается 1
    /// </summary>
    /// <param name="height">Высота строки в пикселах (свойство DataGridViewRow.Height)</param>
    /// <returns>Число строк</returns>
    public int GetTextRowHeight(int height)
    {
      int fh = ControlProvider.Control.DefaultCellStyle.Font.Height;
      int rc = (height - 9) / fh;
      if (rc < 1)
        return 1;
      else
        return rc;
    }

    #endregion

    #region Размеры столбцов

    /// <summary>
    /// Ширина по умолчанию для столбца CheckBoxColumn
    /// </summary>
    public int CheckBoxColumnWidth
    {
      get
      {
        return (int)Math.Ceiling(20.0 * DpiX / 96.0);
      }
    }

    /// <summary>
    /// Ширина по умолчанию для столбца ImageColumn
    /// </summary>
    public int ImageColumnWidth { get { return 20; } }

    /// <summary>
    /// Получить ширину столбца в пискелях для текстового поля
    /// </summary>
    /// <param name="textWidth"></param>
    /// <returns></returns>
    public int GetTextColumnWidth(double textWidth)
    {
      // Так было до 05.11.2009
      // return TextWidth * 8 + 10;

      double w1 = _CharWidthPixel * textWidth;
      double w2 = GetLeftRightGaps();

      return (int)Math.Ceiling(w1 + w2);
    }

    /// <summary>
    /// Суммарный зазор слева и справа в пикселях, который надо добавить к
    /// ширине столбца в пискелях, чтобы поместился текст
    /// (отступы ячейки и рамки)
    /// </summary>
    /// <returns></returns>
    private double GetLeftRightGaps()
    {
      int w = _ControlProvider.Control.DefaultCellStyle.Padding.Left +
        _ControlProvider.Control.DefaultCellStyle.Padding.Right;
      w += 2; // Ширина рамки ячейки по умолчанию
      switch (_ControlProvider.Control.AdvancedCellBorderStyle.Left)
      {
        case DataGridViewAdvancedCellBorderStyle.InsetDouble:
        case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
          w++;
          break;
      }
      switch (_ControlProvider.Control.AdvancedCellBorderStyle.Right)
      {
        case DataGridViewAdvancedCellBorderStyle.InsetDouble:
        case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
          w++;
          break;
      }
      w += 4; // не знаю почему, но иначе не входит

      return (double)w * DpiX / 96.0; // ???
    }

    /// <summary>
    /// Определить число символов, которые можно отобразить в поле заданной ширины
    /// </summary>
    /// <param name="width">Ширина в пикселях</param>
    /// <returns></returns>
    public double GetColumnWidthChars(int width)
    {
      double w1 = (double)width - GetLeftRightGaps();
      return w1 / _CharWidthPixel;
    }

    /// <summary>
    /// Средняя ширина одного символа шрифта ячейки по умолчанию в пикселях,
    /// соответствующая текущему разрешению экрана
    /// </summary>
    public double CharWidthPixel { get { return _CharWidthPixel; } }
    private readonly double _CharWidthPixel;

    #endregion
  }

  /// <summary>
  /// Свойство EFPDataTreeView.Measures
  /// Свойства этого класса предназначены для правильного
  /// определения размера создаваемых столбцов.
  /// Не предназначен для использования в прикладном коде
  /// </summary>
  public sealed class EFPDataTreeViewMeasures : IEFPGridControlMeasures
  {
    #region Конструктор

    internal EFPDataTreeViewMeasures(EFPDataTreeView controlProvider)
    {
      _ControlProvider = controlProvider;
      Graphics gr = controlProvider.Control.CreateGraphics();
      try
      {
        _DpiX = gr.DpiX;
        _DpiY = gr.DpiY;
        _CharWidthPixel = ExtTextRenderer.CalcDefaultFontWidth(gr, controlProvider.Control.Font) * DpiX / 72.0;
      }
      finally
      {
        gr.Dispose();
      }
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
    private readonly EFPDataTreeView _ControlProvider;

    /// <summary>
    /// Горизонтальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiX { get { return _DpiX; } }
    private readonly float _DpiX;

    /// <summary>
    /// Вертикальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiY { get { return _DpiY; } }
    private readonly float _DpiY;

    #endregion

#if XXX
    #region Высота строк

    /// <summary>
    /// Возвращает число пикселей для высоты строки (свойство DataGridViewRow.Height), которая должна отображать заданное
    /// число строк текста
    /// Если запрошена высота меньше, чем для одной строки, возвращается высота для
    /// одной строки
    /// </summary>
    /// <param name="TextRowCount">Количество строк текста, которые должны помещаться</param>
    /// <returns>Высота строки в пикселах</returns>
    public int SetTextRowHeight(int TextRowCount)
    {
      if (TextRowCount < 1)
        TextRowCount = 1;

      int FH = ControlProvider.Control.DefaultCellStyle.Font.Height;
      return FH * TextRowCount + 9;
    }

    /// <summary>
    /// Возвращает число строк текста, которые можно отобразить в строке заданной высоты
    /// Если высота строки недостаточна для отображения одной строки, возвращается 1
    /// </summary>
    /// <param name="Height">Высота строки в пикселах (свойство DataGridViewRow.Height)</param>
    /// <returns>Число строк</returns>
    public int GetTextRowHeight(int Height)
    {
      int FH = ControlProvider.Control.DefaultCellStyle.Font.Height;
      int RC = (Height - 9) / FH;
      if (RC < 1)
        return 1;
      else
        return RC;
    }

    #endregion
#endif

    #region Размеры столбцов

    /// <summary>
    /// Ширина по умолчанию для столбца CheckBoxColumn
    /// </summary>
    public int CheckBoxColumnWidth
    {
      get
      {
        return (int)Math.Ceiling(20.0 * DpiX / 96.0);
      }
    }

    /// <summary>
    /// Ширина по умолчанию для столбца ImageColumn
    /// </summary>
    public int ImageColumnWidth { get { return 20; } }


    /// <summary>
    /// Получить ширину столбца в пискелях для текстового поля
    /// </summary>
    /// <param name="textWidth"></param>
    /// <returns></returns>
    public int GetTextColumnWidth(double textWidth)
    {
      double w1 = _CharWidthPixel * textWidth;
      double w2 = GetLeftRightGaps();

      return (int)Math.Ceiling(w1 + w2);
    }

    /// <summary>
    /// Суммарный зазор слева и справа в пикселях, который надо добавить к
    /// ширине столбца в пискелях, чтобы поместился текст
    /// (отступы ячейки и рамки)
    /// </summary>
    /// <returns></returns>
    private double GetLeftRightGaps()
    {
      return 2 * SystemInformation.BorderSize.Width;
    }

    /// <summary>
    /// Определить число символов, которые можно отобразить в поле заданной ширины
    /// </summary>
    /// <param name="width">Ширина в пикселях</param>
    /// <returns></returns>
    public double GetColumnWidthChars(int width)
    {
      double w1 = (double)width - GetLeftRightGaps();
      return w1 / _CharWidthPixel;
    }

    /// <summary>
    /// Средняя ширина одного символа шрифта ячейки по умолчанию в пикселях,
    /// соответствующая текущему разрешению экрана
    /// </summary>
    public double CharWidthPixel { get { return _CharWidthPixel; } }
    private readonly double _CharWidthPixel;

    #endregion
  }
}
