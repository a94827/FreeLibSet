using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using FreeLibSet.Core;

namespace FreeLibSet.Drawing.Reporting
{
  /// <summary>
  /// Калька с <see cref="FreeLibSet.Drawing.ExtTextRenderer"/> для рисования текста в PDF
  /// Упрощенный вариант без лишних функций.
  /// Вместо матрицы преобразования Matrix используется метод TransormScale().
  /// Так как классы XBrush и прочие, в отличие от Brush, не являются IDisposable,
  /// этот объект также не нуждается в удалении
  /// </summary>
  /// <summary>
  /// Вспомогательный класс для рисования текста в PDF-файле
  /// </summary>
  internal class PdfTextRenderer
  {
    #region Конструктор

    private const string DefaultFontName = "Arial";

    public PdfTextRenderer()
    {
      _FontName = DefaultFontName;
      _FontHeight = 10;
      _FontWidth = 0; // признак необходимости вычислить ширину
      _Bold = false;
      _Italic = false;
      _Underline = false;
      _Strikeout = false;
      _DefaultFontWidth = 0;
      _DefaultLineHeight = 0;
      _Color = XColor.FromArgb(System.Drawing.Color.Black);
      _StringFormat = new XStringFormat();
      _FontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always); //?? может быть не надо
    }

    #endregion

    #region Свойства

    #region Контекст устройства для рисования

    public XGraphics Graphics
    {
      get
      {
        return _Graphics;
      }
      set
      {
        if (value == _Graphics)
          return;
        _Graphics = value;
      }
    }
    private XGraphics _Graphics;

    protected void CheckGraphics()
    {
      if (_Graphics == null)
        throw new NullReferenceException("Свойство Graphics не было установлено");
    }

    #endregion

    #region Свойства шрифта

    /// <summary>
    /// Имя гарниутры шрифта
    /// </summary>
    public string FontName
    {
      get { return _FontName; }
      set
      {
        if (value == _FontName)
          return;
        _FontName = value;
        ResetFont();
      }
    }
    private string _FontName;

    /// <summary>
    /// Требуемая высота шрифта в пунктах
    /// </summary>
    public double FontHeight
    {
      get { return _FontHeight; }
      set
      {
#if DEBUG
        if (value < 0.1f)
          throw new ArgumentException("Слишком маленький шрифт", "value");
#endif
        if (value == _FontHeight)
          return;
        _FontHeight = value;
        ResetFont();
      }
    }

    private double _FontHeight;

    /// <summary>
    /// Требуемая ширина шрифта в пунктах.
    /// Установка свойства в 0 задает ширину шрифта по умолчанию
    /// </summary>
    public double FontWidth
    {
      get
      {
        if (_FontWidth == 0)
          return DefaultFontWidth;
        else
          return _FontWidth;
      }
      set
      {
        if (value == _FontWidth)
          return;
        _FontWidth = value;
        ResetFont();
      }
    }
    private double _FontWidth;

    /// <summary>
    /// Возвращает true, если задан режим автоматического определения ширины шрифта
    /// </summary>
    public bool IsDefaultFontWidth
    {
      get { return _FontWidth == 0; }
    }

    /// <summary>
    /// Расстояние между двумя строками в пунктах. Превышает высоту шрифта
    /// FontHeight на величину дополнительного расстояния между строками
    /// По умолчанию совпадает с DefaultLineHeight. Установка значения в 0 сбрасывает
    /// интервал в DefaultLineHeight. Задание значения LineHeight=FontHeight устанавливает
    /// запись вывод строк вплотную, без дополнительных интервалов
    /// </summary>
    public double LineHeight
    {
      get
      {
        if (_LineHeight == 0)
          return DefaultLineHeight;
        else
          return _LineHeight;
      }
      set
      {
        if (value == _LineHeight)
          return;
        _LineHeight = value;
        ResetFont();
      }
    }
    private double _LineHeight;

    /// <summary>
    /// Альтернативное чтение или установка свойства LineHeight.
    /// Значение 1 устанавливает междстрочный интервал по умолчанию (DefaultLineHeight),
    /// значение 0 задает запись строк вплотную, значения от 0 до 1 задают уплотненное
    /// расположение строк, больше 1 - разреженное
    /// </summary>
    public double LineSpacingScale
    {
      get
      {
        return (LineHeight - FontHeight) / (DefaultLineHeight - FontHeight);
      }
      set
      {
        LineHeight = FontHeight + value * (DefaultLineHeight - FontHeight);
      }
    }

    /// <summary>
    /// Признак жирного шрифта
    /// </summary>
    public bool Bold
    {
      get { return _Bold; }
      set
      {
        if (value == _Bold)
          return;
        _Bold = value;
        ResetFont();
      }
    }
    private bool _Bold;

    /// <summary>
    /// Признак наклонного шрифта
    /// </summary>
    public bool Italic
    {
      get { return _Italic; }
      set
      {
        if (value == _Italic)
          return;
        _Italic = value;
        ResetFont();
      }
    }
    private bool _Italic;

    /// <summary>
    /// Признак подчеркнутого шрифта
    /// </summary>
    public bool Underline
    {
      get { return _Underline; }
      set
      {
        if (value == _Underline)
          return;
        _Underline = value;
        ResetFont();
      }
    }
    private bool _Underline;

    /// <summary>
    /// Признак перечеркнутого шрифта
    /// </summary>
    public bool Strikeout
    {
      get { return _Strikeout; }
      set
      {
        if (value == _Strikeout)
          return;
        _Strikeout = value;
        ResetFont();
      }
    }
    private bool _Strikeout;

    #endregion

    #region Объект шрифта

    public XFont Font
    {
      get
      {
        if (_Font == null)
        {
          XFontStyle st = XFontStyle.Regular;
          if (_Bold)
            st |= XFontStyle.Bold;
          if (_Italic)
            st |= XFontStyle.Italic;
          if (_Underline)
            st |= XFontStyle.Underline;
          if (_Strikeout)
            st |= XFontStyle.Strikeout;

          XFontStyle st1 = st;
          if (!TryCreateFont(st))
          {
            // Пытаемся перебрать все варианты стилей
            // Сначала меняем по одному атрибуту, потом по два, потом - три, потом - все четыре
            int[] keys = new int[15]{
              0x1, 0x2, 0x4, 0x8,
              0x3, 0x5, 0x6, 0x9, 0xA, 0xC,
              0x7, 0xB, 0xD, 0xE,
              0xF
            };

            for (int i = 0; i < 15; i++)
            {
              st = st1;
              int key = keys[i];
              if ((key & 0x1) != 0)
                st = InvStyle(st, XFontStyle.Italic);
              if ((key & 0x2) != 0)
                st = InvStyle(st, XFontStyle.Bold);
              if ((key & 0x4) != 0)
                st = InvStyle(st, XFontStyle.Underline);
              if ((key & 0x8) != 0)
                st = InvStyle(st, XFontStyle.Strikeout);

              if (TryCreateFont(st))
                break;
            }
            if (_Font == null)
              CreateSubstFont(st1);
          }
        }
        return _Font;
      }
    }
    private XFont _Font;

    [DebuggerStepThrough]
    private bool TryCreateFont(XFontStyle st)
    {
      try
      {
        _Font = new XFont(_FontName, _FontHeight, st, _FontOptions);
        return true;
      }
      catch
      {
        return false;
      }
    }

    private XPdfFontOptions _FontOptions;


    [DebuggerStepThrough]
    private void CreateSubstFont(XFontStyle st)
    {
      try
      {
        // Пытаемся обойтись без стиля
        _Font = new XFont(_FontName, _FontHeight);
      }
      catch
      {
        // Используем стандартный шрифт
        _Font = new XFont(DefaultFontName, _FontHeight, st);
      }
    }

    /// <summary>
    /// Инверсия стиля шрифта
    /// </summary>
    /// <param name="orgStyle">Исходный стиль</param>
    /// <param name="inv">Какие стили инвертировать</param>
    /// <returns>Новый стиль</returns>
    private static XFontStyle InvStyle(XFontStyle orgStyle, XFontStyle inv)
    {
      return orgStyle ^ inv;
    }

    /// <summary>
    /// Сброс объекта шрифта
    /// </summary>
    private void ResetFont()
    {
      _Font = null;
      _DefaultFontWidth = 0;
      _DefaultLineHeight = 0;
    }
    #endregion

    /// <summary>
    /// Реальная высота шрифта в пунктах. В отличие от свойства FontHeight, которое
    /// может принимать любое значение, после создания шрифта его размеры не могут
    /// быть произвольными
    /// </summary>
    public double DefaultFontHeight
    {
      get
      {
        return Font.Size;
      }
    }

    /// <summary>
    /// Ширина шрифта в пунктах по умолчанию
    /// </summary>
    public double DefaultFontWidth
    {
      get
      {
        if (_DefaultFontWidth == 0)
        {
          // Вычисляем ширину шрифта по умолчанию
          _DefaultFontWidth = CalcDefaultFontWidth(Graphics, Font);
        }
        return _DefaultFontWidth;
      }
    }
    private double _DefaultFontWidth;

    /// <summary>
    /// Высота строк для текущего шрифта по умолчанию.
    /// </summary>
    public double DefaultLineHeight
    {
      get
      {
        if (_DefaultLineHeight == 0)
        {
          _DefaultLineHeight = CalcDefaultLineHeight(Graphics, Font);
        }
        return _DefaultLineHeight;
      }
    }
    private double _DefaultLineHeight;

    public XStringFormat StringFormat { get { return _StringFormat; } }
    private XStringFormat _StringFormat;

    public XColor Color
    {
      get
      {
        return _Color;
      }
      set
      {
        if (value == _Color)
          return;
        _Color = value;
        _Brush = null;
      }
    }
    private XColor _Color;

    public XBrush Brush
    {
      get
      {
        if (_Brush == null)
          _Brush = new XSolidBrush(_Color);
        return _Brush;
      }
    }
    private XBrush _Brush;

    #endregion

    #region Дополнительные свойства для удобства доступа к StringFormat

    /// <summary>
    /// Разрешен ли перенос по словам
    /// </summary>
    public bool WordWrap
    {
      //get
      //{
      //  return (XStringFormat.FormatFlags & XStringFormatFlags.NoWrap) == 0;
      //}
      //set
      //{
      //  if (value)
      //    StringFormat.FormatFlags &= (~StringFormatFlags.NoWrap);
      //  else
      //    StringFormat.FormatFlags |= StringFormatFlags.NoWrap;
      //}
      get
      {
        return false;
      }
      set
      {
      }
    }

    #endregion

    #region Методы рисования

    public void DrawString(string text, XRect rc)
    {
      ReadyMatrix();
      _Graphics.Save();
      try
      {
        _Graphics.ScaleTransform(_ScaleX, _ScaleY);
        rc = BackTransform(rc);
        _Graphics.DrawString(text, Font, Brush, rc, StringFormat);
      }
      finally
      {
        _Graphics.Restore();
      }
    }

    /// <summary>
    /// Рисование массива строк текста с принудительным вписыванием их в прямоугольник
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="rc"></param>
    public void DrawLines(string[] lines, XRect rc)
    {
      if (lines.Length == 0)
        return;
      if (rc.Width <= 0f || rc.Height <= 0f)
        return; // негде рисовать

      double maxW = 0f;
      // Размеры всех строк по вертикали
      XSize sz1;
      double lh;
      sz1 = new XSize(0, LineHeight);
      lh = sz1.Height;

      double wholeH = lines.Length * lh;

      int i;
      for (i = 0; i < lines.Length; i++)
      {
        XSize sz = MeasureString(lines[i]);
        maxW = Math.Max(maxW, sz.Width);
      }
      // Прежде, чем уменьшать размер шрифта, пытаемся уменьшить межстрочный интервал
      if (wholeH > rc.Height)
      {
        double scaleY1 = rc.Height / wholeH;
        if (LineHeight * scaleY1 < FontHeight)
          scaleY1 = FontHeight / LineHeight;
        lh = lh * scaleY1;
        wholeH = lines.Length * lh;
      }

      // Дополнительные размерные множители, если текст не помещается
      double orgFontHeight = FontHeight;
      double orgFontWidth = FontWidth;
      try
      {
        double scaleY2 = 1f;
        if (wholeH > rc.Height)
        {
          scaleY2 = rc.Height / wholeH;
          FontHeight = FontHeight * scaleY2;
        }
        double scaleX2 = 1f;
        if (maxW > rc.Width)
        {
          scaleX2 = rc.Width / maxW;
          FontWidth = FontWidth * scaleX2;
        }


        // По вертикали исходный прямоугольник требуется пересчитывать, т.к.
        // вертикальное выравнивание работать не будет
        double yOff;
        switch (StringFormat.LineAlignment)
        {
          case XLineAlignment.Far:
            yOff = rc.Height - wholeH * scaleY2;
            break;
          case XLineAlignment.Center:
            yOff = (rc.Height - wholeH * scaleY2) / 2f;
            break;
          default:
            yOff = 0f;
            break;
        }
        for (i = 0; i < lines.Length; i++)
        {
          XRect rc1 = rc;
          rc1.Y += yOff;
          rc1.Y += lh * scaleY2 * i;
          rc1.Height = lh * scaleY2;
          DrawString(lines[i], rc1);
        }
      }
      finally
      {
        FontHeight = orgFontHeight;
        FontWidth = orgFontWidth;
      }
    }

    /// <summary>
    /// Рисование текста со вписыванием в прямоугольник
    /// Разбиение на строки должно быть выполнено заранее с разделителем \r\n
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="rc"></param>
    public void DrawLines(string lines, XRect rc)
    {
      if (String.IsNullOrEmpty(lines))
        return;
      string[] a = lines.Split(DataTools.CRLFSeparators, StringSplitOptions.None);
      DrawLines(a, rc);
    }

    #endregion

    #region Измерение текста

    /// <summary>
    /// Измерение строки текста. 
    /// Перенос по словам игнорируется.
    /// </summary>
    /// <param name="text">Строка текста</param>
    /// <returns>Размеры в единицах, установленных в Graphics</returns>
    public XSize MeasureString(string text)
    {
      text = DataTools.RemoveSoftHyphens(text);
      return MeasureScale(Graphics.MeasureString(text, Font));
    }

    /// <summary>
    /// Измерение строки
    /// </summary>
    /// <param name="text">Текст</param>
    /// <param name="sz">Размер области для предполагаемого размещения текста.
    /// См. описание System.Drawing.Graphics.MeasureString()</param>
    /// <returns>Размер требуемой области</returns>
    public XSize MeasureString(string text, XSize sz)
    {
      return MeasureScale(Graphics.MeasureString(text, Font, /*BackMeasureScale(sz), */_StringFormat)); // TODO: !!!
    }


    /// <summary>
    /// Учет масштабирования при вычислении размеров строки
    /// </summary>
    /// <param name="sz">Размер, вычисленный Graphics.MeasureString</param>
    /// <returns>Размер с учетом масштабирования</returns>
    private XSize MeasureScale(XSize sz)
    {
      return new XSize(sz.Width * FontWidth / DefaultFontWidth,
        sz.Height * FontHeight / DefaultFontHeight);
    }

    private XSize BackMeasureScale(XSize sz)
    {
      return new XSize(sz.Width * DefaultFontWidth / FontWidth,
        sz.Height * DefaultFontHeight / FontHeight);
    }

    /// <summary>
    /// Измерение текста в единицах 0.1 мм
    /// Перенос по словам игнорируется.
    /// </summary>
    /// <param name="text">Строка текста</param>
    /// <returns>Размеры области в единицах 0.1 мм</returns>
    public Size MeasureStringLM(string text)
    {
      XSize sz = MeasureString(text);
      switch (Graphics.PageUnit)
      {
        case XGraphicsUnit.Millimeter:
          sz = new XSize(sz.Width * 10.0, sz.Height * 10.0);
          break;
        case XGraphicsUnit.Point:
          sz = new XSize(sz.Width / 72.0 * 254.0, sz.Height / 72.0 * 254.0);
          break;
        default:
          throw new NotImplementedException();
      }
      return new Size((int)(sz.Width), (int)(sz.Height));
    }

    #endregion

    #region Внутренняя реализация

    /*
    /// <summary>
    /// Текущая матрица, которая подсовывается при вызове методов рисования
    /// </summary>
    private XMatrix CurrentMatrix;

    /// <summary>
    /// Матрица преобразований координат, которые должны быть в исходной системе 
    /// координат
    /// </summary>
    private XMatrix CoordMatrix;
     * */

    private double _ScaleX, _ScaleY;

    /// <summary>
    /// Получение матриц преобразования
    /// </summary>
    private void ReadyMatrix()
    {
#if DEBUG
      CheckGraphics();
#endif
      _ScaleX = FontWidth / DefaultFontWidth;
      _ScaleY = FontHeight / DefaultFontHeight;

      /*
      CurrentMatrix = Graphics.Transform.Clone();
      CurrentMatrix.Scale(ScaleX, ScaleY);
      if (CoordMatrix != null)
        CoordMatrix.Dispose();
      CoordMatrix = new Matrix();
      CoordMatrix.Scale(1.0f / ScaleX, 1.0f / ScaleY);
       * */
    }

    //private PointF[] trPoint = new PointF[2]; // чтобы не создавать каждый раз

    private XRect BackTransform(XRect rc)
    {
      return new XRect(rc.Left / _ScaleX, rc.Top / _ScaleY, rc.Width / _ScaleX, rc.Height / _ScaleY);
    }

    /*
    private PointF BackTransform(PointF pt)
    {
      TrPoint[0] = pt;
      TrPoint[1] = pt; // на всякий случай
      CoordMatrix.TransformPoints(TrPoint);
      return TrPoint[0];
    }

    private RectangleF CurrentTransform(RectangleF rc)
    {
      TrPoint[0].X = rc.Left;
      TrPoint[0].Y = rc.Top;
      TrPoint[1].X = rc.Right;
      TrPoint[1].Y = rc.Bottom;
      CurrentMatrix.TransformPoints(TrPoint);
      return new RectangleF(TrPoint[0].X, TrPoint[0].Y,
        TrPoint[1].X - TrPoint[0].X, TrPoint[1].Y - TrPoint[0].Y);
    }
     * */

    #endregion

    #region Прочие методы
    /*
    /// <summary>
    /// Копирование всех атрибутов, кроме контекста устройства, в другой объект
    /// </summary>
    /// <param name="Dest"></param>
    public void CopyTo(PdfTextRenderer Dest)
    {
      Dest.FontName = FontName;
      Dest.FontHeight = FontHeight;
      Dest.FontWidth = FFontWidth; // используя ширину по умолчанию
      Dest.Bold = Bold;
      Dest.Italic = Italic;
      Dest.Underline = Underline;
      Dest.Strikeout = Strikeout;
      Dest.Color = Color;
      //Dest.Angle = Angle;
      Dest.FStringFormat = (StringFormat)(StringFormat.Clone());
    }
  */
    #endregion

    #region Статические методы

    /// <summary>
    /// Вычисление ширины шрифта по умолчанию
    /// </summary>
    /// <param name="graphics">Контекст рисования</param>
    /// <param name="font">Шрифт</param>
    /// <returns>Ширина шрифта </returns>
    public static double CalcDefaultFontWidth(XGraphics graphics, XFont font)
    {
      const string templateStr = "XXXXXXXXXX0000000000XXXXXXXXXX0000000000";
      double res;
      XSize sz = graphics.MeasureString(templateStr, font);
      res = sz.Width / (double)(templateStr.Length);
      return res;
    }

    public static double CalcDefaultLineHeight(XGraphics graphics, XFont font)
    {
      return font.Height; // ???
    }

    #endregion
  }
}
