// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using FreeLibSet.Core;
using System.Diagnostics;

/*
 * Расширенное средство рисования текста.
 * Стандартный класс System.Drawing.Font не позволяет задавать ширину символов
 * а, также, рисовать наклонный текст.
 * Объект ExtTextRenderer манипулирует преобразованиями координат для достижения
 * эффекта.
 * Порядок работы.
 * 1. Создать объект ExtTextRenderer.
 * 2. Получить контекст вывода и задать свойство Graphics.
 * 3. Создать объект Font и задать свойство Font.
 * 4. Установить свойства FontHeight и FontWidth - размеры шрифта в пунктах
 * 5. Установить выравнивание и другие параметры свойства StringFormat
 * 6. Вызывать метод DrawString нужное число раз.
 * 7. Освободить объект с помощью Dispose()
 * 
 * !!! Вращение не реализовано !!!
 */

namespace FreeLibSet.Drawing
{
  /// <summary>
  /// Расширенный класс рисования текста. Пригоден как для рисования в форме, так 
  /// и для печати на принтере
  /// </summary>
  public class ExtTextRenderer : SimpleDisposableObject
  {
    // 03.01.2021
    // Можно использовать базовый класс без деструктора

    #region Конструктор и Dispose

    private const string DefaultFontName = "Arial";

    /// <summary>
    /// Создает объект и инициализирует его шрифтом Arial 10пт.
    /// </summary>
    public ExtTextRenderer()
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
      _Color = Color.Black;
      _StringFormat = (StringFormat)(StringFormat.GenericTypographic.Clone());
      _StringFormat.FormatFlags &= ~StringFormatFlags.LineLimit;
    }

    /// <summary>
    /// Освобождает занятые объекты GDI-plus, если <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Brush != null)
        {
          _Brush.Dispose();
          _Brush = null;
        }
        if (_CoordMatrix != null)
        {
          _CoordMatrix.Dispose();
          _CoordMatrix = null;
        }
        ResetFont();

        if (_StringFormat != null)
        {
          _StringFormat.Dispose();
          _StringFormat = null;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    #region Контекст устройства для рисования

    /// <summary>
    /// Контекст для рисования
    /// </summary>
    public Graphics Graphics
    {
      get
      {
        return _Graphics;
      }
      set
      {
#if DEBUG
        CheckNotDisposed();
#endif
        if (value == _Graphics)
          return;
        _Graphics = value;
        if (_CurrentMatrix != null)
        {
          _CurrentMatrix.Dispose();
          _CurrentMatrix = null;
        }
      }
    }
    private Graphics _Graphics;

    /// <summary>
    /// Генерирует исключение, если Graphics=null.
    /// </summary>
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
#if DEBUG
        CheckNotDisposed();
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException("value", "Имя шрифта не задано");
#endif
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
    public float FontHeight
    {
      get { return _FontHeight; }
      set
      {
#if DEBUG
        CheckNotDisposed();
        if (value < 0.1f)
          throw new ArgumentException("Слишком маленький шрифт", "value");
#endif
        if (value == _FontHeight)
          return;
        _FontHeight = value;
        ResetFont();
      }
    }

    private float _FontHeight;

    /// <summary>
    /// Требуемая ширина шрифта в пунктах.
    /// Установка свойства в 0 задает ширину шрифта по умолчанию
    /// </summary>
    public float FontWidth
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
#if DEBUG
        CheckNotDisposed();
#endif
        if (value == _FontWidth)
          return;
        _FontWidth = value;
        ResetFont();
      }
    }
    private float _FontWidth;

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
    public float LineHeight
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
#if DEBUG
        CheckNotDisposed();
#endif
        if (value == _LineHeight)
          return;
        _LineHeight = value;
        ResetFont();
      }
    }
    private float _LineHeight;

    /// <summary>
    /// Альтернативное чтение или установка свойства LineHeight.
    /// Значение 1 устанавливает междстрочный интервал по умолчанию (DefaultLineHeight),
    /// значение 0 задает запись строк вплотную, значения от 0 до 1 задают уплотненное
    /// расположение строк, больше 1 - разреженное
    /// </summary>
    public float LineSpacingScale
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
#if DEBUG
        CheckNotDisposed();
#endif
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
#if DEBUG
        CheckNotDisposed();
#endif
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
#if DEBUG
        CheckNotDisposed();
#endif
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
#if DEBUG
        CheckNotDisposed();
#endif
        if (value == _Strikeout)
          return;
        _Strikeout = value;
        ResetFont();
      }
    }
    private bool _Strikeout;

    #endregion

    #region Объект шрифта

    /// <summary>
    /// Возвращает текущий шрифт, созданный на основе свойств FontName, FontHeight,
    /// Bold, Italic, Underline, Strikeout. Так как конкретный шрифт может не поддерживать
    /// заданный набор стилей, возможно будет создан шрифт с другими стилями.
    /// </summary>
    public Font Font
    {
      get
      {
        if (_Font == null)
        {
          FontStyle st = FontStyle.Regular;
          if (_Bold)
            st |= FontStyle.Bold;
          if (_Italic)
            st |= FontStyle.Italic;
          if (_Underline)
            st |= FontStyle.Underline;
          if (_Strikeout)
            st |= FontStyle.Strikeout;

          FontStyle st1 = st;
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
                st = InvStyle(st, FontStyle.Italic);
              if ((key & 0x2) != 0)
                st = InvStyle(st, FontStyle.Bold);
              if ((key & 0x4) != 0)
                st = InvStyle(st, FontStyle.Underline);
              if ((key & 0x8) != 0)
                st = InvStyle(st, FontStyle.Strikeout);

              if (TryCreateFont(st))
                break;
            }

            //if (_Font == null)
            //  throw new BugException("Не удалось применить для шрифта ни \"" + FontName + "\" ни одной комбинации атрибутов");

            if (_Font == null)
              CreateSubstFont(st1);
          }
        }
        return _Font;
      }
    }
    private Font _Font;

    [DebuggerStepThrough]
    private bool TryCreateFont(FontStyle st)
    {
      try
      {
        _Font = new Font(_FontName, _FontHeight, st);
        return true;
      }
      catch
      {
        return false;
      }
    }

    [DebuggerStepThrough]
    private void CreateSubstFont(FontStyle st)
    {
      try
      {
        // Пытаемся обойтись без стиля
        _Font = new Font(_FontName, _FontHeight);
      }
      catch
      {
        // Используем стандартный шрифт
        _Font = new Font(DefaultFontName, _FontHeight, st);
      }
    }

    /// <summary>
    /// Инверсия стиля шрифта
    /// </summary>
    /// <param name="orgStyle">Исходный стиль</param>
    /// <param name="inv">Какие стили инвертировать</param>
    /// <returns>Новый стиль</returns>
    private static FontStyle InvStyle(FontStyle orgStyle, FontStyle inv)
    {
      return orgStyle ^ inv;
    }

    /// <summary>
    /// Сброс объекта шрифта
    /// </summary>
    private void ResetFont()
    {
      if (_CurrentMatrix != null)
      {
        _CurrentMatrix.Dispose();
        _CurrentMatrix = null;
      }
      if (_Font != null)
      {
        _Font.Dispose();
        _Font = null;
      }
      _DefaultFontWidth = 0;
      _DefaultLineHeight = 0;
    }

    #endregion

    /// <summary>
    /// Реальная высота шрифта в пунктах. В отличие от свойства FontHeight, которое
    /// может принимать любое значение, после создания шрифта его размеры не могут
    /// быть произвольными
    /// </summary>
    public float DefaultFontHeight
    {
      get
      {
        return Font.Size;
      }
    }

    /// <summary>
    /// Ширина шрифта в пунктах по умолчанию
    /// </summary>
    public float DefaultFontWidth
    {
      get
      {
        if (_DefaultFontWidth == 0)
        {
          if (IsDisposed)
            return _FontHeight / 2f; // заглушка
          // Вычисляем ширину шрифта по умолчанию
          if (Graphics == null)
            _DefaultFontWidth = CalcDefaultFontWidth(MeasureGraphics, Font);
          else
            _DefaultFontWidth = CalcDefaultFontWidth(Graphics, Font);
        }
        return _DefaultFontWidth;
      }
    }
    private float _DefaultFontWidth;

    /// <summary>
    /// Высота строк для текущего шрифта по умолчанию.
    /// </summary>
    public float DefaultLineHeight
    {
      get
      {
        if (_DefaultLineHeight == 0)
        {
          if (IsDisposed)
            return FontHeight * 1.5f;
          _DefaultLineHeight = CalcDefaultLineHeight(MeasureGraphics, Font);
        }
        return _DefaultLineHeight;
      }
    }
    private float _DefaultLineHeight;


    /// <summary>
    /// Угол наклона текста по часовой стрелке
    /// </summary>
    public int Angle
    {
      get
      {
        return _Angle;
      }
      set
      {
#if DEBUG
        CheckNotDisposed();
#endif
        if (value == _Angle)
          return;
        _Angle = value;
        if (_CurrentMatrix != null)
        {
          _CurrentMatrix.Dispose();
          _CurrentMatrix = null;
        }
      }
    }
    private int _Angle;

    /// <summary>
    /// Флаги форматирования текста
    /// </summary>
    public StringFormat StringFormat { get { return _StringFormat; } }
    private StringFormat _StringFormat;

    /// <summary>
    /// Цвет для вывода текста
    /// </summary>
    public Color Color
    {
      get
      {
        return _Color;
      }
      set
      {
#if DEBUG
        CheckNotDisposed();
#endif
        if (value == _Color)
          return;
        _Color = value;
        if (_Brush != null)
        {
          _Brush.Dispose();
          _Brush = null;
        }
      }
    }
    private Color _Color;

    /// <summary>
    /// Кисть для вывода текста (не фона)
    /// </summary>
    public Brush Brush
    {
      get
      {
        if (_Brush == null)
          _Brush = new SolidBrush(_Color);
        return _Brush;
      }
    }
    private Brush _Brush;

    #endregion

    #region Дополнительные свойства для удобства доступа к StringFormat

    /// <summary>
    /// Разрешен ли перенос по словам
    /// </summary>
    public bool WordWrap
    {
      get
      {
        return (StringFormat.FormatFlags & StringFormatFlags.NoWrap) == 0;
      }
      set
      {
#if DEBUG
        CheckNotDisposed();
#endif
        if (value)
          StringFormat.FormatFlags &= (~StringFormatFlags.NoWrap);
        else
          StringFormat.FormatFlags |= StringFormatFlags.NoWrap;
      }
    }

    #endregion

    #region Методы рисования

    /// <summary>
    /// Выводит строку в заданной области
    /// </summary>
    /// <param name="text">Выводимая строка</param>
    /// <param name="rc">Область</param>
    public void DrawString(string text, RectangleF rc)
    {
      ReadyMatrix();
      Matrix orgMatrix = _Graphics.Transform;
      _Graphics.Transform = _CurrentMatrix;
      try
      {
        rc = BackTransform(rc);
        try
        {
          _Graphics.DrawString(text, Font, Brush, rc, StringFormat);
        }
        catch /*(Exception e)*/
        {
        }
        //FGraphics.DrawString(Text, Font, Brush, rc.Left, rc.Top);
      }
      finally
      {
        _Graphics.Transform = orgMatrix;
      }
    }

    /// <summary>
    /// Выводит строку, начиная с заданной позиции
    /// </summary>
    /// <param name="text">Выводимая строка</param>
    /// <param name="x">Начальная позиция по горизонтали</param>
    /// <param name="y">Начальная позиция по вертикали</param>
    public void DrawString(string text, float x, float y)
    {
      DrawString(text, new PointF(x, y));
    }

    /// <summary>
    /// Выводит строку, начиная с заданной позиции
    /// </summary>
    /// <param name="text">Выводимая строка</param>
    /// <param name="pt">Начальная позиция</param>
    public void DrawString(string text, PointF pt)
    {
      ReadyMatrix();
      Matrix orgMatrix = _Graphics.Transform;
      _Graphics.Transform = _CurrentMatrix;
      try
      {
        pt = BackTransform(pt);
        _Graphics.DrawString(text, Font, Brush, pt);
      }
      finally
      {
        _Graphics.Transform = orgMatrix;
      }
    }

    /// <summary>
    /// Рисование массива строк текста с принудительным вписыванием их в прямоугольник
    /// </summary>
    /// <param name="lines">Массив строк текста</param>
    /// <param name="rc">Область</param>
    public void DrawLines(string[] lines, RectangleF rc)
    {
      if (lines.Length == 0)
        return;
      if (rc.Width <= 0f || rc.Height <= 0f)
        return; // негде рисовать

      float maxW = 0f;
      // Размеры всех строк по вертикали
      SizeF sz1;
      float lh;
      sz1 = new SizeF(0, LineHeight);
      sz1 = PointsToPageUnits(sz1);
      lh = sz1.Height;
      //System.Windows.Forms.MessageBox.Show("LineHeight="+LineHeight.ToString()+"пт., lh="+ lh.ToString(), "Measures");

      float wholeH = lines.Length * lh;

      int i;
      for (i = 0; i < lines.Length; i++)
      {
        SizeF sz = MeasureString(lines[i]);
        //System.Windows.Forms.MessageBox.Show(sz.ToString(), lines[i]);
        maxW = Math.Max(maxW, sz.Width);
      }
      // Прежде, чем уменьшать размер шрифта, пытаемся уменьшить межстрочный интервал
      if (wholeH > rc.Height)
      {
        float scaleY1 = rc.Height / wholeH;
        if (LineHeight * scaleY1 < FontHeight)
          scaleY1 = FontHeight / LineHeight;
        lh = lh * scaleY1;
        wholeH = lines.Length * lh;
      }
      //System.Windows.Forms.MessageBox.Show(lh.ToString(), "lh");

      float orgFontHeight = FontHeight;
      float orgFontWidth = FontWidth;
      try
      {
        // Дополнительные размерные множители, если текст не помещается
        float scaleY2 = 1f;
        if (wholeH > rc.Height)
        {
          scaleY2 = rc.Height / wholeH;
          FontHeight = FontHeight * scaleY2;
        }
        float scaleX2 = 1f;
        if (maxW > rc.Width)
        {
          scaleX2 = rc.Width / maxW;
          FontWidth = FontWidth * scaleX2;
        }


        // По вертикали исходный прямоугольник требуется пересчитывать, т.к.
        // вертикальное выравнивание работать не будет
        float yOff;
        switch (StringFormat.LineAlignment)
        {
          case StringAlignment.Far:
            yOff = rc.Height - wholeH * scaleY2;
            break;
          case StringAlignment.Center:
            yOff = (rc.Height - wholeH * scaleY2) / 2f;
            break;
          default:
            yOff = 0f;
            break;
        }
        for (i = 0; i < lines.Length; i++)
        {
          RectangleF rc1 = rc;
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

    private SizeF PointsToPageUnits(SizeF sz1)
    {
      PointF pt1 = new PointF(sz1.Width, sz1.Height);
      pt1 = PointsToPageUnits(pt1);
      SizeF sz2 = new SizeF(Math.Abs(pt1.X), Math.Abs(pt1.Y));
      if (sz1.Width < 0)
        sz2.Width = -sz2.Width;
      if (sz1.Height < 0)
        sz2.Height = -sz2.Height;
      return sz2;
    }

    private PointF[] _Point1Array = new PointF[1];

    /// <summary>
    /// Преобразование координат из пунктов в текущие единицы PageUnit
    /// </summary>
    /// <param name="pt"></param>
    /// <returns></returns>
    private PointF PointsToPageUnits(PointF pt)
    {
      // 28.08.2023:
      // Для стандартных единиц выполняем преобразование самостоятельно, для лучшей совместимости с Mono  
      switch (Graphics.PageUnit)
      {
        case GraphicsUnit.Point:
          return pt;
        case GraphicsUnit.Inch:
          return new PointF(pt.X / 72f, pt.Y / 72f);
        case GraphicsUnit.Millimeter:
          return new PointF(pt.X / 72f * 25.4f, pt.Y / 72f * 25.4f);
        default:
          _Point1Array[0] = pt;
          GraphicsUnit oldPU = Graphics.PageUnit;
          try
          {
            Graphics.PageUnit = GraphicsUnit.Point;
            Graphics.TransformPoints(CoordinateSpace.Device, CoordinateSpace.Page, _Point1Array);
          }
          finally
          {
            Graphics.PageUnit = oldPU;
          }
          Graphics.TransformPoints(CoordinateSpace.Page, CoordinateSpace.Device, _Point1Array);
          return _Point1Array[0];
      }
    }

    /// <summary>
    /// Рисование текста со вписыванием в прямоугольник
    /// Разбиение на строки должно быть выполнено заранее с разделителем \r\n
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="rc"></param>
    public void DrawLines(string lines, RectangleF rc)
    {
      if (String.IsNullOrEmpty(lines))
        return;
      string[] a = lines.Split(DataTools.CRLFSeparators, StringSplitOptions.None);
      DrawLines(a, rc);
    }

    #endregion

    #region Измерение текста

    /// <summary>
    /// Статический контекст устройства для измерения строк
    /// </summary>
    public static Graphics MeasureGraphics
    {
      get
      {
        if (_MeasureGraphics == null)
        {
          _MeasureImage = new Bitmap(10, 10);
          _MeasureGraphics = Graphics.FromImage(_MeasureImage);
          _MeasureGraphics.PageUnit = GraphicsUnit.Millimeter;
        }
        return _MeasureGraphics;
      }
    }
    private static Bitmap _MeasureImage;
    private static Graphics _MeasureGraphics;

    /// <summary>
    /// Измерение строки текста. 
    /// Перенос по словам игнорируется.
    /// </summary>
    /// <param name="text">Строка текста</param>
    /// <returns>Размеры в единицах, установленных в Graphics</returns>
    public SizeF MeasureString(string text)
    {
#if DEBUG
      CheckNotDisposed();
#endif
      Graphics gr = Graphics;
      if (gr == null)
        gr = MeasureGraphics;
      text = DataTools.RemoveSoftHyphens(text);
      return MeasureScale(gr.MeasureString(text, Font, new SizeF(float.MaxValue, float.MaxValue), CalcDefaultFontWidthStringFormat));
    }


#if XXXXX
    /// <summary>
    /// Измерение строки текста.
    /// Перенос по словам, если установлена соответствующая опция в StringFormat
    /// Width - ширина области текста  
    /// </summary>
    /// <param name="Text">Строка текста</param>
    /// <param name="Width">Ширина области, в которую надо вписать текст</param>
    /// <returns>Размеры в единицах, установленных в Graphics</returns>
    public SizeF MeasureString(string Text, float Width)
    {
#if DEBUG
      CheckNotDisposed();
      CheckGraphics();
#endif
      SizeF sz = new SizeF(Width, float.MaxValue);
      return MeasureScale(Graphics.MeasureString(Text, Font, sz, FStringFormat));
    }
#endif

    /// <summary>
    /// Измерение строки
    /// </summary>
    /// <param name="text">Текст</param>
    /// <param name="sz">Размер области для предполагаемого размещения текста.
    /// См. описание System.Drawing.Graphics.MeasureString()</param>
    /// <returns>Размер требуемой области</returns>
    public SizeF MeasureString(string text, SizeF sz)
    {
#if DEBUG
      CheckNotDisposed();
#endif
      Graphics gr = Graphics;
      if (gr == null)
        gr = MeasureGraphics;
      return MeasureScale(gr.MeasureString(text, Font, BackMeasureScale(sz), _StringFormat));
    }

    /// <summary>
    /// Учет масштабирования при вычислении размеров строки
    /// </summary>
    /// <param name="sz">Размер, вычисленный Graphics.MeasureString</param>
    /// <returns>Размер с учетом масштабирования</returns>
    private SizeF MeasureScale(SizeF sz)
    {
      return new SizeF(sz.Width * FontWidth / DefaultFontWidth,
        sz.Height * FontHeight / DefaultFontHeight);
    }

    private SizeF BackMeasureScale(SizeF sz)
    {
      return new SizeF(sz.Width * DefaultFontWidth / FontWidth,
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
      SizeF sz = MeasureString(text);
      sz = new SizeF(sz.Width * 10f, sz.Height * 10f);
      return sz.ToSize();
    }

    /// <summary>
    /// Измерение строки текста в единицах 0.1 мм
    /// Перенос по словам, если установлена соответствующая опция в StringFormat
    /// </summary>
    /// <param name="text">Строка текста</param>
    /// <param name="width">Ширина области, в которую надо вписать текст в единицах 0.1 мм</param>
    /// <returns>Размеры в единицах 0.1 мм</returns>
    public Size MeasureStringLM(string text, int width)
    {
      SizeF sz = new SizeF((float)width / 10f, 1000f);
      sz = MeasureString(text, sz);
      sz = new SizeF(sz.Width * 10f, sz.Height * 10f);
      return sz.ToSize();
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Текущая матрица, которая подсовывается при вызове методов рисования
    /// </summary>
    private Matrix _CurrentMatrix;

    /// <summary>
    /// Матрица преобразований координат, которые должны быть в исходной системе 
    /// координат
    /// </summary>
    private Matrix _CoordMatrix;

    /// <summary>
    /// Получение матриц преобразования
    /// </summary>
    private void ReadyMatrix()
    {
#if DEBUG
      CheckNotDisposed();
#endif
      if (_CurrentMatrix != null)
        return;

#if DEBUG
      CheckGraphics();
#endif
      float scaleX = FontWidth / DefaultFontWidth;
      float scaleY = FontHeight / DefaultFontHeight;

      _CurrentMatrix = Graphics.Transform.Clone();
      _CurrentMatrix.Scale(scaleX, scaleY);
      if (_CoordMatrix != null)
        _CoordMatrix.Dispose();
      _CoordMatrix = new Matrix();
      _CoordMatrix.Scale(1.0f / scaleX, 1.0f / scaleY);
    }

    private PointF[] _TrPoint = new PointF[2]; // чтобы не создавать каждый раз

    private RectangleF BackTransform(RectangleF rc)
    {
      _TrPoint[0].X = rc.Left;
      _TrPoint[0].Y = rc.Top;
      _TrPoint[1].X = rc.Right;
      _TrPoint[1].Y = rc.Bottom;
      _CoordMatrix.TransformPoints(_TrPoint);
      return new RectangleF(_TrPoint[0].X, _TrPoint[0].Y,
        _TrPoint[1].X - _TrPoint[0].X, _TrPoint[1].Y - _TrPoint[0].Y);
    }

    private PointF BackTransform(PointF pt)
    {
      _TrPoint[0] = pt;
      _TrPoint[1] = pt; // на всякий случай
      _CoordMatrix.TransformPoints(_TrPoint);
      return _TrPoint[0];
    }

    //private RectangleF CurrentTransform(RectangleF rc)
    //{
    //  _TrPoint[0].X = rc.Left;
    //  _TrPoint[0].Y = rc.Top;
    //  _TrPoint[1].X = rc.Right;
    //  _TrPoint[1].Y = rc.Bottom;
    //  _CurrentMatrix.TransformPoints(_TrPoint);
    //  return new RectangleF(_TrPoint[0].X, _TrPoint[0].Y,
    //    _TrPoint[1].X - _TrPoint[0].X, _TrPoint[1].Y - _TrPoint[0].Y);
    //}

    #endregion

    #region Прочие методы

    /// <summary>
    /// Копирование всех атрибутов, кроме контекста устройства, в другой объект
    /// </summary>
    /// <param name="dest">Заполняемый объект</param>
    public void CopyTo(ExtTextRenderer dest)
    {
      dest.FontName = FontName;
      dest.FontHeight = FontHeight;
      dest.FontWidth = _FontWidth; // используя ширину по умолчанию
      dest.Bold = Bold;
      dest.Italic = Italic;
      dest.Underline = Underline;
      dest.Strikeout = Strikeout;
      dest.Color = Color;
      dest.Angle = Angle;
      dest._StringFormat = (StringFormat)(StringFormat.Clone());
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Экземпляр StringFormat для измерения строк, когда текущие настройки не должны учитываться
    /// </summary>
    private static StringFormat CalcDefaultFontWidthStringFormat
    {
      get
      {
        // 27.12.2020
        // Больше не используем блокировку

        if (_CalcDefaultFontWidthStringFormat == null)
        {
          StringFormat sf = (StringFormat)(StringFormat.GenericTypographic.Clone());
          sf.FormatFlags &= ~StringFormatFlags.LineLimit;
          sf.FormatFlags &= ~StringFormatFlags.NoWrap;
          _CalcDefaultFontWidthStringFormat = sf;
        }
        return _CalcDefaultFontWidthStringFormat;
      }
    }
    private static volatile StringFormat _CalcDefaultFontWidthStringFormat;

    /// <summary>
    /// Вычисление ширины шрифта по умолчанию
    /// </summary>
    /// <param name="Graphics">Контекст рисования</param>
    /// <param name="Font">Шрифт</param>
    /// <returns>Ширина шрифта </returns>
    public static float CalcDefaultFontWidth(Graphics Graphics, Font Font)
    {
      const string TemplateStr = "XXXXXXXXXX0000000000XXXXXXXXXX0000000000";
      float res;
      GraphicsUnit oldPU = Graphics.PageUnit;
      try
      {
        Graphics.PageUnit = GraphicsUnit.Point;
        SizeF sz = Graphics.MeasureString(TemplateStr, Font,
          new SizeF(float.MaxValue, float.MaxValue), CalcDefaultFontWidthStringFormat);
        if (sz.Width <= 0f)
          return Font.Height / 2f; // 09.08.2023 - Заглушка на случай невозможности вычисления
        res = sz.Width / (float)(TemplateStr.Length);
      }
      finally
      {
        Graphics.PageUnit = oldPU;
      }
      return res;
    }

    //public static float CalcDefaultFontWidth(Font Font)
    //{
    //  float res = 0;
    //  Graphics gr = Graphics.FromHwnd(new IntPtr(0));
    //  try
    //  {
    //    res=CalcDefaultFontWidth(gr, Font);
    //  }
    //  finally
    //  {
    //    gr.Dispose();
    //  }
    //  return res;
    //}

    //public static float CalcDefaultFontWidth(string FontName, float emSize)
    //{
    //  float res = 0;
    //  Font fnt = new Font(FontName, emSize);
    //  try
    //  {
    //    res=CalcDefaultFontWidth(fnt);
    //  }
    //  finally
    //  {
    //    fnt.Dispose();
    //  }
    //  return res;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Graphics"></param>
    /// <param name="Font"></param>
    /// <returns></returns>
    public static float CalcDefaultLineHeight(Graphics Graphics, Font Font)
    {
      return Font.Height; // ???
      //float res = 0f;
      //GraphicsUnit oldPU = Graphics.PageUnit;
      //try
      //{
      //  Graphics.PageUnit = GraphicsUnit.Point;
      //  res = Font.GetHeight(Graphics);
      //}
      //finally
      //{
      //  Graphics.PageUnit = oldPU;
      //}
      //return res;
    }

    #endregion
  }
}
