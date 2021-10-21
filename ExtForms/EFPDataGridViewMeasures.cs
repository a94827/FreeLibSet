using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Drawing;

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
        _CharWidthDots = ExtTextRenderer.CalcDefaultFontWidth(gr, controlProvider.Control.DefaultCellStyle.Font) * DpiX / 72.0;
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
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private EFPDataGridView _ControlProvider;

    /// <summary>
    /// Горизонтальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiX { get { return _DpiX; } }
    private float _DpiX;

    /// <summary>
    /// Вертикальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiY { get { return _DpiY; } }
    private float _DpiY;

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
      int FH = ControlProvider.Control.DefaultCellStyle.Font.Height;
      int RC = (height - 9) / FH;
      if (RC < 1)
        return 1;
      else
        return RC;
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

      double w1 = _CharWidthDots * textWidth;
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
      return w1 / _CharWidthDots;
    }

    /// <summary>
    /// Средняя ширина одного символа шрифта ячейки по умолчанию в пикселях,
    /// соответствующая текущему разрешению экрана
    /// </summary>
    public double CharWidthDots { get { return _CharWidthDots; } }
    private double _CharWidthDots;

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
        //FCharWidthDots = ExtTextRenderer.CalcDefaultFontWidth(gr, ControlProvider.Control.DefaultCellStyle.Font) * DpiX / 72.0;
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
    private EFPDataTreeView _ControlProvider;

    /// <summary>
    /// Горизонтальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiX { get { return _DpiX; } }
    private float _DpiX;

    /// <summary>
    /// Вертикальное разрешение экрана, точек на дюйм
    /// (нормальное: 96)
    /// </summary>
    public float DpiY { get { return _DpiY; } }
    private float _DpiY;

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
      throw new NotImplementedException();
                                          /*
      // Так было до 05.11.2009
      // return TextWidth * 8 + 10;

      double w1 = FCharWidthDots * TextWidth;
      double w2 = GetLeftRightGaps();

      return (int)Math.Ceiling(w1 + w2);    */
    }

#if XXX
    /// <summary>
    /// Суммарный зазор слева и справа в пикселях, который надо добавить к
    /// ширине столбца в пискелях, чтобы поместился текст
    /// (отступы ячейки и рамки)
    /// </summary>
    /// <returns></returns>
    private double GetLeftRightGaps()
    {
      int w = FControlProvider.Control.DefaultCellStyle.Padding.Left +
        FControlProvider.Control.DefaultCellStyle.Padding.Right;
      w += 2; // Ширина рамки ячейки по умолчанию
      switch (FControlProvider.Control.AdvancedCellBorderStyle.Left)
      {
        case DataGridViewAdvancedCellBorderStyle.InsetDouble:
        case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
          w++;
          break;
      }
      switch (FControlProvider.Control.AdvancedCellBorderStyle.Right)
      {
        case DataGridViewAdvancedCellBorderStyle.InsetDouble:
        case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
          w++;
          break;
      }
      w += 4; // не знаю почему, но иначе не входит

      return (double)w * DpiX / 96.0; // ???
    }
#endif

    /// <summary>
    /// Определить число символов, которые можно отобразить в поле заданной ширины
    /// </summary>
    /// <param name="width">Ширина в пикселях</param>
    /// <returns></returns>
    public double GetColumnWidthChars(int width)
    {
      throw new NotImplementedException();
      /*
      double w1 = (double)Width - GetLeftRightGaps();
      return w1 / FCharWidthDots;*/
    }

#if XXX
    /// <summary>
    /// Средняя ширина одного символа шрифта ячейки по умолчанию в пикселях,
    /// соответствующая текущему разрешению экрана
    /// </summary>
    public double CharWidthDots { get { return FCharWidthDots; } }
    private double FCharWidthDots;

#endif
    #endregion
  }
}
