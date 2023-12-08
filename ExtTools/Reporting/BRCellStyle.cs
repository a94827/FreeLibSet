using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Задание цвета фона ячейки или текста в формате RGB.
  /// Поддерживает значение Auto. Альфа-канал не поддерживается.
  /// </summary>
  public struct BRColor : IEquatable<BRColor>
  {
    #region Создание

    private BRColor(int value)
    {
      _Value = value;
    }

    /// <summary>
    /// Задание цвета в формате RGB
    /// </summary>
    /// <param name="r">Красный (0-255)</param>
    /// <param name="g">Зеленый (0-255)</param>
    /// <param name="b">Синий (0-255)</param>
    public BRColor(int r, int g, int b)
    {
      if (r < 0 || r > 255)
        throw new ArgumentOutOfRangeException("r");
      if (g < 0 || g > 255)
        throw new ArgumentOutOfRangeException("g");
      if (b < 0 || b > 255)
        throw new ArgumentOutOfRangeException("b");
      _Value = (1 << 24 | (r & 0xFF) << 16 | (g & 0xFF) << 8 | (b & 0xFF));
    }

    /// <summary>
    /// Специальное значение цвета - "автоматический" (обычно, белый цвет фона и черный текст и линии границ)
    /// </summary>
    public static BRColor Auto { get { return new BRColor(); } }

    /// <summary>
    /// Черный
    /// </summary>
    public static BRColor Black { get { return new BRColor(0, 0, 0); } }

    /// <summary>
    /// Синий
    /// </summary>
    public static BRColor Blue { get { return new BRColor(0, 0, 255); } }
    
    /// <summary>
    /// Зеленый
    /// </summary>
    public static BRColor Green { get { return new BRColor(0, 255, 0); } }

    /// <summary>
    /// Красный
    /// </summary>
    public static BRColor Red { get { return new BRColor(255, 0, 0); } }

    /// <summary>
    /// Сине-зеленый
    /// </summary>
    public static BRColor Cyan { get { return new BRColor(0, 255, 255); } }

    /// <summary>
    /// Сиреневый
    /// </summary>
    public static BRColor Purple { get { return new BRColor(255, 0, 255); } }

    /// <summary>
    /// Желтый
    /// </summary>
    public static BRColor Yellow { get { return new BRColor(255, 255, 0); } }

    /// <summary>
    /// Белый
    /// </summary>
    public static BRColor White { get { return new BRColor(255, 255, 255); } }

    #endregion

    #region Свойства

    /// <summary>
    /// Представление в виде целого числа.
    /// Не предназначено для использования в прикладном коде.
    /// </summary>
    public int IntValue { get { return _Value; } }
    private readonly Int32 _Value;

    /// <summary>
    /// Красный компонент (0-255)
    /// </summary>
    public int R { get { return (_Value >> 16) & 0xFF; } }

    /// <summary>
    /// Зеленый компонент (0-255)
    /// </summary>
    public int G { get { return (_Value >> 8) & 0xFF; } }

    /// <summary>
    /// Синий компонент (0-255)
    /// </summary>
    public int B { get { return _Value & 0xFF; } }

    /// <summary>
    /// Возвращает true для автоматически определяемого цвета
    /// </summary>
    public bool IsAuto { get { return _Value == 0; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Value == 0)
        return "Auto";
      else
        return "R=" + R.ToString() + ", G=" + G.ToString() + ", B=" + B.ToString();
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение с другим цветом
    /// </summary>
    /// <param name="other">Второй цвет</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(BRColor other)
    {
      return this._Value == other._Value;
    }

    /// <summary>
    /// Сравнение с другим цветом
    /// </summary>
    /// <param name="obj">Второй цвет</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is BRColor)
        return (this._Value == ((BRColor)obj)._Value);
      return false;
    }

    /// <summary>
    /// Хэш-код для создания коллекций
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return _Value;
    }

    /// <summary>
    /// Сравнение двух цветов
    /// </summary>
    /// <param name="a">Первый цвет</param>
    /// <param name="b">Второй цвет</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(BRColor a, BRColor b)
    {
      return a._Value == b._Value;
    }

    /// <summary>
    /// Сравнение двух цветов
    /// </summary>
    /// <param name="a">Первый цвет</param>
    /// <param name="b">Второй цвет</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(BRColor a, BRColor b)
    {
      return a._Value != b._Value;
    }

    #endregion
  }

  #region Перечисление BRHAlign

  /// <summary>
  /// Горизонтальное выравнивание текста ячейки
  /// </summary>
  public enum BRHAlign
  {
    /// <summary>
    /// Автоматическое выравнивание, в зависимости от типа значения в <see cref="BRSelector.Value"/>.
    /// Для определения реального выравнивания используйте свойство <see cref="BRSelector.ActualHAlign"/>.
    /// </summary>
    Auto,

    /// <summary>
    /// По левому краю
    /// </summary>
    Left,

    /// <summary>
    /// По центру
    /// </summary>
    Center,

    /// <summary>
    /// По правому краю
    /// </summary>
    Right
  }

  #endregion

  #region Перечисление BRVAlign

  /// <summary>
  /// Вертикальное выравнивание текста ячейки
  /// </summary>
  public enum BRVAlign
  {
    /// <summary>
    /// По верхнему краю
    /// </summary>
    Top,

    /// <summary>
    /// По центру
    /// </summary>
    Center,

    /// <summary>
    /// По нижнему краю
    /// </summary>
    Bottom
  }

  #endregion

  #region Перечисление BRWrapMode

  /// <summary>
  /// Режим переноса текста ячейки
  /// </summary>
  public enum BRWrapMode
  {
    /// <summary>
    /// Нет переноса. Если текст не помещается, будет уменьшена ширина шрифта
    /// </summary>
    NoWrap,

    /// <summary>
    /// Перенос по словам
    /// </summary>
    WordWrap
  }

  #endregion

  #region Перечисление BRTextFiller

  /// <summary>
  /// Заполнитель свободного от текста пространства ячейки
  /// </summary>
  public enum BRTextFiller
  {
    /// <summary>
    /// Нет заполнения (по умолчанию)
    /// </summary>
    None,

    /// <summary>
    /// Одинарная линия
    /// </summary>
    Thin,

    /// <summary>
    /// Одинарная линия средней толщины
    /// </summary>
    Medium,

    /// <summary>
    /// Толстая линия
    /// </summary>
    Thick,

    /// <summary>
    /// Две линии
    /// </summary>
    TwoLines,
  }

  #endregion

  #region Перечисление BRLineStyle

  /// <summary>
  /// Стиль границы ячейки
  /// </summary>
  public enum BRLineStyle
  {
    /*
     * Числовые значения не фиксированы
     * Порядок определяется "поглощением" для границ соседних ячеек
     */

      /// <summary>
      /// Нет линий
      /// </summary>
    None,

    /// <summary>
    /// Точки
    /// </summary>
    Dot,

    /// <summary>
    /// Штрихи
    /// </summary>
    Dash,

    /// <summary>
    /// Точки и штрихи
    /// </summary>
    DashDot,

    /// <summary>
    /// Штихи и две точки
    /// </summary>
    DashDotDot,
    /// <summary>
    /// Одинарная тонкая линия
    /// </summary>
    Thin,

    /// <summary>
    /// Одинарная линия средней толщины
    /// </summary>
    Medium,

    /// <summary>
    /// Одинарная толстая линия
    /// </summary>
    Thick
  }

  #endregion

  /// <summary>
  /// Описание границы ячейки. Задается цвет и стиль.
  /// Структура однократной записи
  /// </summary>
  public struct BRLine : IEquatable<BRLine>
  {
    #region Конструкторы

    /// <summary>
    /// Создает линию заданного стиля и цвета
    /// </summary>
    /// <param name="style">Стиль</param>
    /// <param name="color">Цвет</param>
    public BRLine(BRLineStyle style, BRColor color)
    {
      _Style = style;
      _Color = color;
    }

    /// <summary>
    /// Создает линию заданного стиля и автоматически определяемого цвета
    /// </summary>
    /// <param name="style">Стиль</param>
    public BRLine(BRLineStyle style)
    {
      _Style = style;
      _Color = BRColor.Auto;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Стиль линии
    /// </summary>
    public BRLineStyle Style { get { return _Style; } }
    private readonly BRLineStyle _Style;

    /// <summary>
    /// Цвет линии
    /// </summary>
    public BRColor Color { get { return _Color; } }
    private readonly BRColor _Color;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Style=" + Style.ToString() + ", Color=" + Color.ToString();
    }

    #endregion

    #region Логические операторы

    /// <summary>
    /// Объединение двух линий.
    /// Из двух стилей выбирается более "толстый".
    /// Если оба цвета <see cref="BRColor.Auto"/>, то результирующий цвет будет <see cref="BRColor.Auto"/>.
    /// Если один из цветов <see cref="BRColor.Auto"/>, то берется цвет другой линии.
    /// Если оба цвета заданы, то результирующий цвет определяется по каждому компонента отдельно. Для каждого компонента берется минимальное из двух значений.
    /// </summary>
    /// <param name="line1">Описание первой линии</param>
    /// <param name="line2">Описание второй линии</param>
    /// <returns>Объединенное описание</returns>
    public static BRLine operator |(BRLine line1, BRLine line2)
    {
      BRLineStyle style = (BRLineStyle)(Math.Max((int)(line1.Style), (int)(line2.Style)));
      BRColor color;
      if (line1.Color == BRColor.Auto)
        color = line2.Color;
      else if (line2.Color == BRColor.Auto)
        color = line1.Color;
      else
      {
        int r = Math.Min(line1.Color.R, line2.Color.R);
        int g = Math.Min(line1.Color.G, line2.Color.G);
        int b = Math.Min(line1.Color.B, line2.Color.B);
        color = new BRColor(r, g, b);
      }
      return new BRLine(style, color);
    }

    #endregion

    #region Операторы сравнения

    /// <summary>
    /// Сравнение описаний двух линий.
    /// Описания считаются одинаковыми, если совпадает стиль и цвет линий.
    /// </summary>
    /// <param name="line1">Описание первой линии</param>
    /// <param name="line2">Описание второй линии</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(BRLine line1, BRLine line2)
    {
      if (line1.Style == BRLineStyle.None)
        return line2.Style == BRLineStyle.None;
      else
        return line1.Style == line2.Style && line1.Color == line2.Color;
    }

    /// <summary>
    /// Сравнение описаний двух линий.
    /// Описания считаются одинаковыми, если совпадает стиль и цвет линий.
    /// </summary>
    /// <param name="line1">Описание первой линии</param>
    /// <param name="line2">Описание второй линии</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(BRLine line1, BRLine line2)
    {
      return !(line1 == line2);
    }

    /// <summary>
    /// Сравнение с другим описанием линии.
    /// Описания считаются одинаковыми, если совпадает стиль и цвет линий.
    /// </summary>
    /// <param name="other">Описание второй линии</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(BRLine other)
    {
      return this == other;
    }

    /// <summary>
    /// Сравнение с другим описанием линии.
    /// Описания считаются одинаковыми, если совпадает стиль и цвет линий.
    /// </summary>
    /// <param name="obj">Описание второй линии</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      if (!(obj is BRLine))
        return false;
      return this == (BRLine)obj;
    }

    /// <summary>
    /// Возвращает хэш-код для организации словарей
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return (int)Style;
    }

    #endregion

    #region Предопределенные значения

    /// <summary>
    /// Описание "Нет линии"
    /// </summary>
    public static BRLine None { get { return new BRLine(); } }

    /// <summary>
    /// Описание "Тонкая линия с автоматически определяемым цветом"
    /// </summary>
    public static BRLine Thin { get { return new BRLine(BRLineStyle.Thin); } }

    /// <summary>
    /// Описание "Линия средней толщины с автоматически определяемым цветом"
    /// </summary>
    public static BRLine Medium { get { return new BRLine(BRLineStyle.Medium); } }

    /// <summary>
    /// Описание "Толстая линия с автоматически определяемым цветом"
    /// </summary>
    public static BRLine Thick { get { return new BRLine(BRLineStyle.Thick); } }

    #endregion

    #region Толщина линий

    /// <summary>
    /// Толщина пера для тонких линий в единицах 0.1 мм
    /// </summary>
    public const double ThinLineWidth01mm = 2.0;

    /// <summary>
    /// Толщина пера для средних линий в единицах 0.1 мм
    /// </summary>
    public const double MediumLineWidth01mm = 5.0;

    /// <summary>
    /// Толщина пера для толстых линий в единицах 0.1 мм
    /// </summary>
    public const double ThickLineWidth01mm = 10.0;

    /// <summary>
    /// Толщина пера для тонких линий в единицах 1/20 пункта
    /// </summary>
    public const int ThinLineWidthTwip = 11;

    /// <summary>
    /// Толщина пера для средних линий в единицах 1/20 пункта
    /// </summary>
    public const int MediumLineWidthTwip = 28;

    /// <summary>
    /// Толщина пера для толстых линий в единицах 1/20 пункта
    /// </summary>
    public const int ThickLineWidthTwip = 57;


    /// <summary>
    /// Толщина пера для тонких линий в пунктах
    /// </summary>
    public const double ThinLineWidthPt = ThinLineWidthTwip / 20.0;

    /// <summary>
    /// Толщина пера для средних линий в пунктах
    /// </summary>
    public const double MediumLineWidthPt = MediumLineWidthTwip / 20.0;

    /// <summary>
    /// Толщина пера для толстых линий в пунктах
    /// </summary>
    public const double ThickLineWidthPt = ThickLineWidthTwip / 20.0;


    /// <summary>
    /// Возвращает желаемую толщину линии в пунктах
    /// </summary>
    /// <param name="lineStyle">Стиль линии</param>
    /// <returns>Толщина</returns>
    public static double GetLineWidthPt(BRLineStyle lineStyle)
    {
      switch (lineStyle)
      {
        case BRLineStyle.None: return 0;
        case BRLineStyle.Medium: return MediumLineWidthPt;
        case BRLineStyle.Thick: return ThickLineWidthPt;
        default: return ThinLineWidthPt;
      }
    }

    /// <summary>
    /// Возвращает желаемую толщину линии в единицах 0.1мм
    /// </summary>
    /// <param name="lineStyle">Стиль линии</param>
    /// <returns>Толщина</returns>
    public static double GetLineWidthPt01mm(BRLineStyle lineStyle)
    {
      switch (lineStyle)
      {
        case BRLineStyle.None: return 0;
        case BRLineStyle.Medium: return MediumLineWidth01mm;
        case BRLineStyle.Thick: return ThickLineWidth01mm;
        default: return ThinLineWidth01mm;
      }
    }

    #endregion
  }

  /// <summary>
  /// Стили могут задаваться:
  /// - для отчета в-целом
  /// - для полосы (таблицы)
  /// - для столбца
  /// - для строки
  /// - для ячейки
  /// </summary>
  public abstract class BRCellStyle
  {
    #region Константы

    const int Index_FontName = 0;
    const int Index_FontSize = 1;
    const int Index_Bold = 2;
    const int Index_Italic = 3;
    const int Index_Underline = 4;
    const int Index_Strikeout = 5;
    const int Index_BackColor = 6;
    const int Index_ForeColor = 7;
    const int Index_LeftMargin = 8;
    const int Index_TopMargin = 9;
    const int Index_RightMargin = 10;
    const int Index_BottomMargin = 11;
    const int Index_IndentLevel = 12;
    const int Index_HAlign = 13;
    const int Index_VAlign = 14;
    const int Index_WrapMode = 15;
    const int Index_LeftBorder = 16;
    const int Index_TopBorder = 17;
    const int Index_RightBorder = 18;
    const int Index_BottomBorder = 19;
    const int Index_DiagonalUp = 20;
    const int Index_DiagonalDown = 21;
    const int Index_Format = 22;
    const int Index_FormatProvider = 23;
    const int Index_TextFiller = 24;
    internal const int Index_ParentStyleName = 25;
    internal const int Array_Size = 26;

    #endregion

    #region Нетипизированное чтение/запись значений 

    internal abstract object GetValue(BRCellStyle caller, int index);
    internal abstract void SetValue(int index, object value);

    internal abstract BRReport Report { get; }

    #endregion

    #region Типизированный доступ

    #region Шрифт

    /// <summary>
    /// Имя шрифта, например, "Arial"
    /// </summary>
    public string FontName
    {
      get { return (string)GetValue(this, Index_FontName); }
      set
      {
        SetValue(Index_FontName, value);
      }
    }

    /// <summary>
    /// Размеры шрифта.
    /// Размеры должны задаваться синхронно. Нельзя, например, задавать высоту шрифта на уровне ячейки, а ширину - для BRReport.
    /// Класс однократной записи
    /// </summary>
    private class BRFontSize
    {
      #region Конструктор

      public BRFontSize(int heightTwip)
      {
        if (heightTwip < BRReport.MinFontHeightTwip || heightTwip > BRReport.MaxFontHeightTwip)
          throw new ArgumentOutOfRangeException();
        _HeightTwip = heightTwip;
        _WidthPercent = 100;
        _MaxEnlargePercent = 100;
      }

      private BRFontSize(int heightTwip, int lineHeightTwip, int widthTwip, int widthPercent, int maxEnlargePercent, bool alwaysEnlarge)
      {
        _HeightTwip = heightTwip;
        _LineHeightTwip = lineHeightTwip;
        _WidthTwip = widthTwip;
        _WidthPercent = widthPercent;
        _MaxEnlargePercent = maxEnlargePercent;
        _AlwaysEnlarge = alwaysEnlarge;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Высота шрифта в единицах 1/20 пункта (твипсах).
      /// Например, для шрифта 10 пунктов имеет значение 200
      /// </summary>
      public int HeightTwip { get { return _HeightTwip; } }
      private readonly int _HeightTwip;

      /// <summary>
      /// Высота строки в твипсах. 0 - использовать значение по умолчанию
      /// </summary>
      public int LineHeightTwip { get { return _LineHeightTwip; } }
      private readonly int _LineHeightTwip;

      /// <summary>
      /// Средняя ширина символов в твипсах. 0 - использовать значение по умолчанию.
      /// </summary>
      public int WidthTwip { get { return _WidthTwip; } }
      private readonly int _WidthTwip;

      /// <summary>
      /// Ширина шрифта в процентах от стандартной. 
      /// </summary>
      public int WidthPercent { get { return _WidthPercent; } }
      private readonly int _WidthPercent;

      /// <summary>
      /// Максимальное разрешенное расширение шрифта для заполнения по ширине.
      /// </summary>
      public int MaxEnlargePercent { get { return _MaxEnlargePercent; } }
      private readonly int _MaxEnlargePercent;

      public bool AlwaysEnlarge { get { return _AlwaysEnlarge; } }
      private readonly bool _AlwaysEnlarge;

      #endregion

      #region Модификация свойств

      public BRFontSize SetLineHeightTwip(int lineHeightTwip)
      {
        if (lineHeightTwip == this.LineHeightTwip)
          return this;
        if (lineHeightTwip != 0)
        {
          if (lineHeightTwip < BRReport.MinFontHeightTwip || lineHeightTwip > BRReport.MaxFontHeightTwip)
            throw new ArgumentOutOfRangeException();
        }

        return new BRFontSize(this.HeightTwip, lineHeightTwip, this.WidthTwip, this.WidthPercent, this.MaxEnlargePercent, this.AlwaysEnlarge);
      }

      public BRFontSize SetWidthTwip(int widthTwips)
      {
        if (widthTwips == this.WidthTwip && this.WidthPercent == 100)
          return this;
        if (widthTwips < BRReport.MinFontWidthTwip || widthTwips > BRReport.MaxFontWidthTwip)
          throw new ArgumentOutOfRangeException();
        return new BRFontSize(this.HeightTwip, this.LineHeightTwip, widthTwips, 0, this.MaxEnlargePercent, this.AlwaysEnlarge);
      }

      public BRFontSize SetWidthPercent(int widthPercent)
      {
        if (widthPercent == this.WidthPercent && this.WidthTwip == 0)
          return this;
        if (widthPercent < BRReport.MinFontWidthPercent || widthPercent > BRReport.MaxFontWidthPercent)
          throw new ArgumentOutOfRangeException();
        return new BRFontSize(this.HeightTwip, this.LineHeightTwip, 0, widthPercent, this.MaxEnlargePercent, this.AlwaysEnlarge);
      }

      public BRFontSize SetMaxEnlargePercent(int maxEnlargePercent)
      {
        if (maxEnlargePercent < BRReport.MinFontEnlargePercent || maxEnlargePercent > BRReport.MaxFontEnlargePercent)
          throw new ArgumentOutOfRangeException();
        return new BRFontSize(this.HeightTwip, this.LineHeightTwip, this.WidthTwip, this.WidthPercent, maxEnlargePercent, this.AlwaysEnlarge);
      }

      public BRFontSize SetAlwaysEnlarge(bool alwaysEnlarge)
      {
        return new BRFontSize(this.HeightTwip, this.LineHeightTwip, this.WidthTwip, this.WidthPercent, this.MaxEnlargePercent, alwaysEnlarge);
      }

      #endregion
    }

    private BRFontSize FontSizeObj { get { return (BRFontSize)GetValue(this, Index_FontSize); } }

    #region Высота

    /// <summary>
    /// Высота шрифта в единица 1/20 пункта
    /// </summary>
    public int FontHeightTwip
    {
      get { return FontSizeObj.HeightTwip; }
      set { SetValue(Index_FontSize, new BRFontSize(value)); }
    }

    /// <summary>
    /// Высота строки в единицах 1/20 пункта.
    /// Высота строки состоит из высоты шрифта <see cref="FontHeightTwip"/> и межстрочного интервала.
    /// Нулевое значение (по умолчанию) указывает автоматически определяемый межстрочный интервал.
    /// </summary>
    public int LineHeightTwip
    {
      get { return FontSizeObj.LineHeightTwip; }
      set { SetValue(Index_FontSize, FontSizeObj.SetLineHeightTwip(value)); }
    }

    /// <summary>
    /// Высота шрифта в пунктах
    /// </summary>
    public float FontHeightPt
    {
      get { return FontHeightTwip / 20f; }
      set { FontHeightTwip = (int)Math.Round(value * 20f); }
    }

    /// <summary>
    /// Высота строки в пунктах.
    /// Высота строки состоит из высоты шрифта <see cref="FontHeightPt"/> и межстрочного интервала.
    /// Нулевое значение (по умолчанию) указывает автоматически определяемый межстрочный интервал.
    /// </summary>
    public float LineHeightPt
    {
      get { return LineHeightTwip / 20f; }
      set { LineHeightTwip = (int)Math.Round(value * 20f); }
    }

    #endregion

    #region Ширина

    /// <summary>
    /// Ширина шрифта в единицах 1/20 пункта.
    /// Нулевое значение (по умолчанию) задает автоматическое определение ширины, определяемое самим шрифтом.
    /// Свойства <see cref="FontWidthPercent"/> и <see cref="FontWidthTwip"/> являются взаимоисключающими.
    /// </summary>
    public int FontWidthTwip
    {
      get { return FontSizeObj.WidthTwip; }
      set { SetValue(Index_FontSize, FontSizeObj.SetWidthTwip(value)); }
    }

    /// <summary>
    /// Ширина шрифта в процентах от нормальной.
    /// По умолчанию - 100%. Значения, большие 100, задаются растянутый шрифт, меньше 100 - сжатый.
    /// Если текст не помещается в ячейку, то для него может быть применено автоматическое сжатие.
    /// Также, может быть выполнено растягивание шрифта при установленном свойстве <see cref="MaxEnlargePercent"/>.
    /// Свойства <see cref="FontWidthPercent"/> и <see cref="FontWidthTwip"/> являются взаимоисключающими.
    /// </summary>
    public int FontWidthPercent
    {
      get { return FontSizeObj.WidthPercent; }
      set { SetValue(Index_FontSize, FontSizeObj.SetWidthPercent(value)); }
    }

    /// <summary>
    /// Ширина шрифта в пунктах.
    /// Нулевое значение (по умолчанию) задает автоматическое определение ширины, определяемое самим шрифтом.
    /// </summary>
    public float FontWidthPt
    {
      get { return FontWidthTwip / 20f; }
      set { FontWidthTwip = (int)Math.Round(value * 20f); }
    }

    #endregion

    #region Увеличение ширины

    /// <summary>
    /// Максимальный коэффициент увеличения ширины шрифта для заполнения ячейки.
    /// Значение 100 (по умолчанию) означает отсутствие расширения (то есть если текст
    /// заполняет не всю ячейку, то свободное место не заполняется). Значение больше
    /// 100% задает возможность расширения.
    /// 
    /// Если даже при применении заданного максимального коэффициента не удается
    /// заполнить ширину ячейки, то поведение зависит от свойства <see cref="AlwaysEnlarge"/>.
    /// 
    /// Например, если задана ширина шрифта 6пт (автоматически или в явном виде);
    /// ширина ячейки минус отсупы слева и справа равна 4.0 см; ширина текста при
    /// ширина 6.0 пт равна 3.0 см; значение <see cref="MaxEnlargePercent"/>=120 (максимальное 
    /// расширение 20%). В этом случае, если <see cref="AlwaysEnlarge"/>=true, будет использован 
    /// шрифт шириной 6*1.2=7.2пт и текст займет 3,6см. При <see cref="AlwaysEnlarge"/>=false 
    /// расширения не будет.
    /// Если же текст занимает 3.5 см, то будет применен меньший
    /// коэффициент расширения, равный 4.0см/3.5см=1.143, ширина шрифта будет равна
    /// 6.0*1.143=6.86пт и текст займет все отведенные 4 см.
    /// 
    /// Коэффициент не влияет на сжатие шрифта для размещения текста в ячейке, если
    /// он там не помещается. Также он не влияет на перенос по словам, т.к. перенос
    /// расчитывается до учета этого коэффициента.
    /// 
    /// Допустимый диапазон значений: от 100 до 500%. Реально не следует использовать
    /// значения, большие 1.5
    /// </summary>
    public int MaxEnlargePercent
    {
      get { return FontSizeObj.MaxEnlargePercent; }
      set { SetValue(Index_FontSize, FontSizeObj.SetMaxEnlargePercent(value)); }
    }

    /// <summary>
    /// Определяет поведение, когда расширение текста даже с помощью коэффициента,
    /// равного <see cref="MaxEnlargePercent"/>, не позволяет заполнить ширину ячейки (слишком узкий текст). 
    /// При значении false (по умолчанию) расширение отменяется (будет использована ширина шрифта <see cref="FontWidthPt"/>, <see cref="FontWidthPercent"/> или оптимальная). 
    /// Если задано значение true, то будет использован максимальный коэффициент расширения <see cref="MaxEnlargePercent"/>.
    /// </summary>
    public bool AlwaysEnlarge
    {
      get { return FontSizeObj.AlwaysEnlarge; }
      set { SetValue(Index_FontSize, FontSizeObj.SetAlwaysEnlarge(value)); }
    }

    #endregion

    #region Атрибуты шрифта

    /// <summary>
    /// True, если шрифт жирный
    /// </summary>
    public bool Bold
    {
      get { return (bool)GetValue(this, Index_Bold); }
      set { SetValue(Index_Bold, value); }
    }

    /// <summary>
    /// True, если шрифт наклонный
    /// </summary>
    public bool Italic
    {
      get { return (bool)GetValue(this, Index_Italic); }
      set { SetValue(Index_Italic, value); }
    }

    /// <summary>
    /// True, если шрифт подчеркнутый
    /// </summary>
    public bool Underline
    {
      get { return (bool)GetValue(this, Index_Underline); }
      set { SetValue(Index_Underline, value); }
    }

    /// <summary>
    /// True, если шрифт зачеркнутый
    /// </summary>
    public bool Strikeout
    {
      get { return (bool)GetValue(this, Index_Strikeout); }
      set { SetValue(Index_Strikeout, value); }
    }

    #endregion

    #endregion

    #region Цвета

    /// <summary>
    /// Цвет фона ячейки. По умолчанию - <see cref="BRColor.Auto"/> - обычно белый
    /// </summary>
    public BRColor BackColor
    {
      get { return (BRColor)GetValue(this, Index_BackColor); }
      set { SetValue(Index_BackColor, value); }
    }

    /// <summary>
    /// Цвет текста. По умолчанию - <see cref="BRColor.Auto"/> - обычно черный
    /// </summary>
    public BRColor ForeColor
    {
      get { return (BRColor)GetValue(this, Index_ForeColor); }
      set { SetValue(Index_ForeColor, value); }
    }

    #endregion

    #region Выравнивание

    /// <summary>
    /// Горизонтальное выравнивание. По умолчанию - <see cref="BRHAlign.Auto"/>  - определяется значением ячейки <see cref="BRSelector.Value"/>.
    /// </summary>
    public BRHAlign HAlign
    {
      get { return (BRHAlign)GetValue(this, Index_HAlign); }
      set { SetValue(Index_HAlign, value); }
    }

    /// <summary>
    /// Вертикальное выравнивание. По умолчанию - <see cref="BRVAlign.Center"/> (а не по нижнему краю, как в Excel).
    /// </summary>
    public BRVAlign VAlign
    {
      get { return (BRVAlign)GetValue(this, Index_VAlign); }
      set { SetValue(Index_VAlign, value); }
    }

    /// <summary>
    /// Режим переноса текста. По умолчанию - <see cref="BRWrapMode.NoWrap"/> - не используется.
    /// </summary>
    public BRWrapMode WrapMode
    {
      get { return (BRWrapMode)GetValue(this, Index_WrapMode); }
      set { SetValue(Index_WrapMode, value); }
    }

    #endregion

    #region Отступы

    /// <summary>
    /// Отступ от левой границы ячейки в единицах 0.1мм.
    /// По умолчанию - 1мм.
    /// </summary>
    public int LeftMargin
    {
      get { return (int)GetValue(this, Index_LeftMargin); }
      set { SetValue(Index_LeftMargin, value); }
    }

    /// <summary>
    /// Отступ от верхней границы ячейки в единицах 0.1мм.
    /// По умолчанию - 1мм.
    /// </summary>
    public int TopMargin
    {
      get { return (int)GetValue(this, Index_TopMargin); }
      set { SetValue(Index_TopMargin, value); }
    }

    /// <summary>
    /// Отступ от правой границы ячейки в единицах 0.1мм.
    /// По умолчанию - 1мм.
    /// </summary>
    public int RightMargin
    {
      get { return (int)GetValue(this, Index_RightMargin); }
      set { SetValue(Index_RightMargin, value); }
    }

    /// <summary>
    /// Отступ от нижней границы ячейки в единицах 0.1мм.
    /// По умолчанию - 1мм.
    /// </summary>
    public int BottomMargin
    {
      get { return (int)GetValue(this, Index_BottomMargin); }
      set { SetValue(Index_BottomMargin, value); }
    }

    /// <summary>
    /// Одновременное задание всех отступов в единицах 0.1мм.
    /// Если свойства <see cref="LeftMargin"/>, <see cref="TopMargin"/>, <see cref="RightMargin"/> и <see cref="BottomMargin"/>
    /// имеют разные значения, возвращается минимальное из них.
    /// </summary>
    public int AllMargins
    {
      get
      {
        return Math.Min(Math.Min(LeftMargin, TopMargin), Math.Min(RightMargin, BottomMargin));
      }
      set
      {
        LeftMargin = value;
        TopMargin = value;
        RightMargin = value;
        BottomMargin = value;
      }
    }

    /// <summary>
    /// Дополнительный отступ от края ячейки в единицах средней ширины шрифта (условно количество символов).
    /// Если <see cref="BRSelector.ActualHAlign"/> возвращает <see cref="BRHAlign.Left"/>, то отступ считается от левого края ячейки.
    /// Если <see cref="BRHAlign.Right"/> - то от правого. Для <see cref="BRHAlign.Center"/> свойство игнорируется.
    /// (аналогично свойствам ячейки Excel)
    /// </summary>
    public int IndentLevel
    {
      get { return (int)GetValue(this, Index_IndentLevel); }
      set
      {
        if (value < BRReport.MinIndentLevel || value > BRReport.MaxIndentLevel)
          throw new ArgumentOutOfRangeException();
        SetValue(Index_IndentLevel, value);
      }
    }

    #endregion

    #region Числовой формат

    /// <summary>
    /// Формат ячейки.
    /// Способы форматирования определяются типом значения ячейки в соответствии с <see cref="IFormattable.ToString(string, IFormatProvider)"/>.
    /// </summary>
    public string Format
    {
      get { return (string)GetValue(this, Index_Format); }
      set { SetValue(Index_Format, value); }
    }

    /// <summary>
    /// Провайдер для форматирования ячейки.
    /// Используется при вызове <see cref="IFormattable.ToString(string, IFormatProvider)"/>.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get { return (IFormatProvider)GetValue(this, Index_FormatProvider); }
      set { SetValue(Index_FormatProvider, value); }
    }

    /// <summary>
    /// Возвращает числовой формат из <see cref="FormatProvider"/>
    /// </summary>
    public System.Globalization.NumberFormatInfo NumberFormat
    {
      get { return (System.Globalization.NumberFormatInfo)(FormatProvider.GetFormat(typeof(System.Globalization.NumberFormatInfo))); }
    }

    /// <summary>
    /// Возвращает формат даты/времени из <see cref="FormatProvider"/>
    /// </summary>
    public System.Globalization.DateTimeFormatInfo DateTimeFormat
    {
      get { return (System.Globalization.DateTimeFormatInfo)(FormatProvider.GetFormat(typeof(System.Globalization.DateTimeFormatInfo))); }
    }

    #endregion

    #region Заполнитель

    /// <summary>
    /// Заполнение свободного от текста пространства ячейки.
    /// Используется для организации "прочеркивания".
    /// </summary>
    public BRTextFiller TextFiller
    {
      get { return (BRTextFiller)GetValue(this, Index_TextFiller); }
      set { SetValue(Index_TextFiller, value); }
    }

    #endregion

    #region Рамки

    /// <summary>
    /// Левая граница ячейки
    /// </summary>
    public BRLine LeftBorder
    {
      get { return (BRLine)GetValue(this, Index_LeftBorder); }
      set { SetValue(Index_LeftBorder, value); }
    }


    /// <summary>
    /// Верхняя граница ячейки
    /// </summary>
    public BRLine TopBorder
    {
      get { return (BRLine)GetValue(this, Index_TopBorder); }
      set { SetValue(Index_TopBorder, value); }
    }

    /// <summary>
    /// Правая граница ячейки
    /// </summary>
    public BRLine RightBorder
    {
      get { return (BRLine)GetValue(this, Index_RightBorder); }
      set { SetValue(Index_RightBorder, value); }
    }

    /// <summary>
    /// Нижняя граница ячейки
    /// </summary>
    public BRLine BottomBorder
    {
      get { return (BRLine)GetValue(this, Index_BottomBorder); }
      set { SetValue(Index_BottomBorder, value); }
    }

    /// <summary>
    /// Одновременная установка четырех границ ячейки.
    /// Если свойства <see cref="LeftBorder"/>, <see cref="TopBorder"/>, <see cref="RightBorder"/> и <see cref="BottomBorder"/> имеют
    /// разные значения, возвращается условный результат объединения стилей.
    /// Свойства <see cref="DiagonalUp"/> и <see cref="DiagonalDown"/> не используются.
    /// </summary>
    public BRLine AllBorders
    {
      get
      {
        return LeftBorder | TopBorder | RightBorder | BottomBorder;
      }
      set
      {
        LeftBorder = value;
        TopBorder = value;
        RightBorder = value;
        BottomBorder = value;
      }
    }

    /// <summary>
    /// Возвращает true, если свойства <see cref="LeftBorder"/>, <see cref="TopBorder"/>, <see cref="RightBorder"/> и <see cref="BottomBorder"/>
    /// совпадают (возможно, все не установлены).
    /// Свойства <see cref="DiagonalUp"/> и <see cref="DiagonalDown"/> не используются.
    /// </summary>
    public bool AreaAllBordersSame
    {
      get
      {
        BRLine l = LeftBorder;
        return TopBorder == l && RightBorder == l && BottomBorder == l;
      }
    }

    /// <summary>
    /// Возвращает true, если задана хотя бы одна из четырех границ
    /// Свойства <see cref="DiagonalUp"/> и <see cref="DiagonalDown"/> не используются.
    /// </summary>
    public bool HasBorders
    {
      get { return LeftBorder != BRLine.None || TopBorder != BRLine.None || RightBorder != BRLine.None || BottomBorder != BRLine.None; }
    }

    /// <summary>
    /// Линия внутри ячейки из левого нижнего угла в правый верхний
    /// </summary>
    public BRLine DiagonalUp
    {
      get { return (BRLine)GetValue(this, Index_DiagonalUp); }
      set { SetValue(Index_DiagonalUp, value); }
    }

    /// <summary>
    /// Линия внутри ячейки из левого верхнего угла в правый нижний
    /// </summary>
    public BRLine DiagonalDown
    {
      get { return (BRLine)GetValue(this, Index_DiagonalDown); }
      set { SetValue(Index_DiagonalDown, value); }
    }

    #endregion

    #region Родительский стиль

    /// <summary>
    /// Если свойство установлено, то при извлечении данных на уровне <see cref="BRReport"/>, данные берутся из указанного именованного стиля.
    /// Если они не определены, то в родителе именованного стиля, если они образуют иерархию. Если именованный стиль и его родители не содержат данных,
    /// то берутся данные из <see cref="BRReport.DefaultCellStyle"/>.
    /// Если свойство не установлено, то данные сразу извлекаются из <see cref="BRReport.DefaultCellStyle"/>
    /// </summary>
    public string ParentStyleName
    {
      get { return (string)GetValue(null /* не передаем ссылку */, Index_ParentStyleName); }
      set
      {
        if (Report == null)
          throw new InvalidOperationException();
        if (value == null)
          value = String.Empty;
        if (value.Length > 0)
        {
          if (Report.NamedCellStyles[value] == null)
            throw new ArgumentException("Имя стиля не найдено: " + value);
        }
        SetValue(Index_ParentStyleName, value);
      }
    }

    /// <summary>
    /// Альтернативная установка свойства <see cref="ParentStyleName"/> как объекта именованного стиля.
    /// </summary>
    public BRNamedCellStyle ParentStyle
    {
      get
      {
        if (Report == null)
          return null;
        return Report.NamedCellStyles[ParentStyleName];
      }
      set
      {
        if (Report == null)
          throw new InvalidOperationException();
        if (value == null)
          SetValue(Index_ParentStyleName, String.Empty);
        else
        {
          if (!Object.ReferenceEquals(value.Report, this.Report))
            throw new ArgumentException("Стиль относится к другому отчету");
          SetValue(Index_ParentStyleName, value.Name);
        }
      }
    }

    #endregion

    #region Значения по умолчанию

    #region Избегание боксинга

    private static readonly object BorderNoneObject = BRLine.None;

    private static readonly object BRFontSizeObject = new BRFontSize(BRReport.DefaultFontHeightTwip);

    private static readonly object FalseObject = false;

    private static readonly object AutoColorObject = BRColor.Auto;

    private static readonly object AutoHAlignObject = BRHAlign.Auto;

    private static readonly object CenterVAlignObject = BRVAlign.Center;

    private static readonly object NoWrapObject = BRWrapMode.NoWrap;

    private static readonly object IntObject_10 = 10;

    private static readonly object IntObject_0 = 0;

    private static readonly object NoTextFillerObject = BRTextFiller.None;

    #endregion

    internal static object GetDefaultValue(int index)
    {
      switch (index)
      {
        case Index_FontName: return BRReport.DefaultFontName;
        case Index_FontSize: return BRFontSizeObject;
        case Index_Bold:
        case Index_Italic:
        case Index_Underline:
        case Index_Strikeout:
          return FalseObject;
        case Index_BackColor:
        case Index_ForeColor:
          return AutoColorObject;
        case Index_HAlign:
          return AutoHAlignObject;
        case Index_VAlign:
          return CenterVAlignObject;
        case Index_WrapMode:
          return NoWrapObject;
        case Index_LeftMargin:
        case Index_TopMargin:
        case Index_RightMargin:
        case Index_BottomMargin:
          return IntObject_10;
        case Index_IndentLevel:
          return IntObject_0;
        case Index_Format:
          return String.Empty;
        case Index_FormatProvider:
          return System.Globalization.CultureInfo.CurrentCulture;
        case Index_LeftBorder:
        case Index_TopBorder:
        case Index_RightBorder:
        case Index_BottomBorder:
        case Index_DiagonalUp:
        case Index_DiagonalDown:
          return BorderNoneObject;
        case Index_TextFiller:
          return NoTextFillerObject;
        case Index_ParentStyleName:
          return String.Empty;
        default:
          throw new ArgumentOutOfRangeException("index");
      }
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Неабстрактная реализация <see cref="BRCellStyle"/> с внутренним хранением значений стиля.
  /// Используется в виртуальных реализациях <see cref="BRSelector"/> и как хранилище стилей в <see cref="BRReport"/> и <see cref="BRBand"/>.
  /// </summary>
  public class BRCellStyleStorage : BRCellStyle
  {
    #region Конструктор

    /// <summary>
    /// Создает хранилище
    /// </summary>
    /// <param name="parent">Родительское хранилище</param>
    public BRCellStyleStorage(BRCellStyle parent)
    {
      _Parent = parent;
    }

    private BRCellStyle _Parent;

    private object[] _Data;

    internal override object GetValue(BRCellStyle caller, int index)
    {
      if (_Data != null)
      {
        if (_Data[index] != null)
          return _Data[index];
      }

      if (_Parent == null)
        return GetDefaultValue(index);
      else
        return _Parent.GetValue(caller, index);
    }

    internal override void SetValue(int index, object value)
    {
      if (_Data == null)
      {
        if (value == null)
          return;
        _Data = new object[Array_Size];
      }
      _Data[index] = value;
    }

    internal override BRReport Report
    {
      get
      {
        if (_Parent == null)
          throw new NullReferenceException("Свойство Parent не установлено");
        else
          return _Parent.Report;
      }
    }

    internal void Clear()
    {
      if (_Data != null)
        Array.Clear(_Data, 0, Array_Size);
    }

    #endregion
  }

  /// <summary>
  /// Компактное 4-байтное хранилище высоты/ширины строки/столбца и набора флагов
  /// </summary>
  internal struct BRRowColumnData
  {
    #region Значение

    private int _Value;

    private const int SizeMask = 0x00FFFFFF;

    public int GetSize()
    {
      return _Value & SizeMask;
    }

    public void SetSize(int value)
    {
      if ((value & (~SizeMask)) != 0)
        throw new ArgumentOutOfRangeException();

      _Value = (_Value & (~SizeMask)) | value;
    }

    public bool GetFlag(int flag)
    {
      return (_Value & flag) != 0;
    }

    public void SetFlag(int flag, bool value)
    {
      if (value)
        _Value = _Value | flag;
      else
        _Value = _Value & (~flag);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для <see cref="BRRowInfo"/> и <see cref="BRColumnInfo"/>
  /// </summary>
  public abstract class BRRowColumnInfoBase
  {
    #region Абстрактные методы

    internal abstract int GetSize();

    internal abstract void SetSize(int value);

    internal abstract bool GetFlag(int flag);

    internal abstract void SetFlag(int flag, bool value);

    internal abstract BRReport Report { get; }

    #endregion
  }

  /// <summary>
  /// Описание строки в <see cref="BRBand"/>. Для доступа к объекту используйте <see cref="BRSelector.RowInfo"/>.
  /// </summary>
  public abstract class BRRowInfo : BRRowColumnInfoBase
  {
    #region Пользовательские свойства

    /// <summary>
    /// Высота строки в единицах 0.1мм.
    /// Значение <see cref="BRReport.AutoRowHeight"/> (по умолчанию) - автоматический подбор высоты по содержимому.
    /// Установка свойства сбрасывает также <see cref="AutoGrow"/> в false
    /// </summary>
    public int Height
    {
      get { return GetSize(); }
      set
      {
        if (value != BRReport.AutoRowHeight)
        {
          if (value < BRReport.MinRowHeight || value > BRReport.MaxRowHeight)
            throw new ArgumentOutOfRangeException();
        }
        SetSize(value);
      }
    }

    const int Hidden_Flag = 0x01000000;
    const int Repeatable_Flag = 0x02000000;
    const int KeepWithNext_Flag = 0x04000000;
    const int KeepWithPrev_Flag = 0x08000000;
    const int AutoGrow_Flag = 0x10000000;

    /// <summary>
    /// True (по умолчанию) - строка является видимой
    /// </summary>
    public bool Visible
    {
      get { return !GetFlag(Hidden_Flag); }
      set { SetFlag(Hidden_Flag, !value); }
    }

    /// <summary>
    /// Признак повторения строки большой таблицы, которая размещается на нескольких листах по высоте.
    /// По умолчанию - false.
    /// Используйте для строк шапки таблицы или для строки с номерами граф.
    /// На первой странице строки размещаются по порядку, без учета повтора. На второй и следующей странице сначала выводятся строки с признаком <see cref="Repeatable"/>=true,
    /// а потом - обычные строки.
    /// Если строка обнаружена не на первой странице, повторение начинается со следующей страницы.
    /// Если суммарная высота повторяемых строк не позволяет разместить на странице хотя бы одну обычную строку, то повторение отключается для этой страницы. 
    /// При этом повторение отключается для всех строк. На следующей странице повторение может быть восстановлено, если очередная обычная строка имеет меньший размер и помещается на странице.
    /// Свойство <see cref="Repeatable"/> имеет приоритет над <see cref="KeepWithNext"/> и <see cref="KeepWithPrev"/>. Если повторяющиеся строки имеют большую высоту, которая не позволяет
    /// разместить две простые "сцепленные" строки, то строки выводятся на разных страницах. Также, если одна из сцепленных строк имеет признак <see cref="Repeatable"/>, а другая - нет, то повторяться будет только одна из них.
    /// </summary>
    public bool Repeatable
    {
      get { return GetFlag(Repeatable_Flag); }
      set { SetFlag(Repeatable_Flag, value); }
    }

    /// <summary>
    /// Если установлено в true, то строка будет размещена на странице вместе со следующей.
    /// По умолчанию - false.
    /// Используйте для шапки таблицы.
    /// Игнорируется для последней строки полосы. Чтобы запретить разрыв со следующей полосой, используйте свойство <see cref="BRBand.KeepWithNext"/>.
    /// Свойство игнорируется, пока остается установленным свойство <see cref="BRBand.KeepWhole"/>, которое запрещает разрыв между любыми строками таблицы.
    /// Установка свойства <see cref="KeepWithNext"/> для строки эквивалентно установке <see cref="KeepWithPrev"/> для следующей строки.
    /// 
    /// Если свойство <see cref="KeepWithNext"/> установлено, а у следующей строки задано <see cref="Visible"/>=false, то проверяется свойство <see cref="KeepWithNext"/> у скрытой строки.
    /// Сцепление действует, если оно установлено. Аналогично, если скрыто несколько строк, то проверяется наличие сцепления у каждой из них, в противном случае, сцепка разрывается.
    /// </summary>
    public bool KeepWithNext
    {
      get { return GetFlag(KeepWithNext_Flag); }
      set { SetFlag(KeepWithNext_Flag, value); }
    }

    /// <summary>
    /// Если установлено в true, то строка будет размещена на странице вместе с предыдущей.
    /// По умолчанию - false.
    /// Используйте для строки итогов.
    /// Игнорируется для первой строки полосы. Чтобы запретить разрыв с предыдущей полосой, используйте свойство <see cref="BRBand.KeepWithPrev"/>.
    /// 
    /// О взаимодействии с друними свойствами см. в пояснениях к свойству <see cref="KeepWithNext"/>.
    /// </summary>
    public bool KeepWithPrev
    {
      get { return GetFlag(KeepWithPrev_Flag); }
      set { SetFlag(KeepWithPrev_Flag, value); }
    }

    /// <summary>
    /// Если установлено в true, то строка будет участвовать в заполнении листа страницы по высоте. Все строки на странице, имеющие установленное свойство, будут увеличены пропорционально своим значениям <see cref="Height"/>, чтобы заполнить страницу.
    /// Если false (по умолчанию), то строка будет иметь точную высоту, задаваемую <see cref="Height"/> или высота будет подбираться автоматически по содержимому
    /// </summary>
    public bool AutoGrow
    {
      get { return GetFlag(AutoGrow_Flag); }
      set { SetFlag(AutoGrow_Flag, value); }
    }

    /// <summary>
    /// Одновременная установка высоты строки и признака заполнения по высоте
    /// </summary>
    /// <param name="height">Высота в единицах 0.1мм</param>
    /// <param name="autoGrow">Признак расширения</param>
    public void SetHeight(int height, bool autoGrow)
    {
      Height = height;
      AutoGrow = AutoGrow;
    }

    #endregion
  }

  /// <summary>
  /// Неабстрактная реализация <see cref="BRRowInfo "/> с внутренним хранением данных.
  /// Используется для виртуальных таблиц
  /// </summary>
  internal sealed class BRRowInfoStorage : BRRowInfo
  {
    #region Конструктор

    public BRRowInfoStorage(BRReport report)
    {
      _Report = report;
    }

    #endregion

    #region Доступ к данным

    private BRRowColumnData _Data;

    internal override int GetSize()
    {
      return _Data.GetSize();
    }

    internal override void SetSize(int value)
    {
      _Data.SetSize(value);
    }

    internal override bool GetFlag(int flag)
    {
      return _Data.GetFlag(flag);
    }

    internal override void SetFlag(int flag, bool value)
    {
      _Data.SetFlag(flag, value);
    }

    internal void Clear()
    {
      _Data = new BRRowColumnData();
    }

    internal override BRReport Report { get { return _Report; } }
    private BRReport _Report;

    #endregion
  }

  /// <summary>
  /// Описание столбца в <see cref="BRBand"/>. Для доступа к объекту используйте <see cref="BRSelector.ColumnInfo"/>.
  /// </summary>
  public abstract class BRColumnInfo : BRRowColumnInfoBase
  {
    #region Пользовательские свойства

    /// <summary>
    /// Ширина столбца в единицах 0.1мм.
    /// По умолчанию - <see cref="BRReport.DefaultColumnWidth"/>
    /// </summary>
    public int Width
    {
      get
      {
        int w = GetSize();
        if (w == 0)
          return Report.DefaultColumnWidth;
        else
          return w;
      }
      set
      {
        if (value < BRReport.MinColumnWidth || value > BRReport.MaxColumnWidth)
          throw new ArgumentOutOfRangeException();
        SetSize(value);
        SetFlag(NoAutoGrow_Flag, true);
      }
    }

    const int Hidden_Flag = 0x01000000;
    const int Repeatable_Flag = 0x02000000;
    const int NoAutoGrow_Flag = 0x10000000;

    /// <summary>
    /// Если true (по умолчанию), то столбец является видимым
    /// </summary>
    public bool Visible
    {
      get { return !GetFlag(Hidden_Flag); }
      set { SetFlag(Hidden_Flag, !value); }
    }

    /// <summary>
    /// Если true, то столбец будет повторяться на каждой странице при печати таблицы, если таблица не помещается на одну страницу по ширине.
    /// </summary>
    public bool Repeatable
    {
      get { return GetFlag(Repeatable_Flag); }
      set { SetFlag(Repeatable_Flag, value); }
    }

    /// <summary>
    /// Если установлено в true (по умолчанию), то столбец будет участвовать в заполнении листа страницы по ширине. Все столбцы на странице, имеющие установленное свойство, будут увеличены пропорционально своим значениям <see cref="Width"/>, чтобы заполнить страницу.
    /// Если false, то столбец будет иметь точную ширину, задаваемую <see cref="Width"/>.
    /// </summary>
    public bool AutoGrow
    {
      get { return !GetFlag(NoAutoGrow_Flag); }
      set { SetFlag(NoAutoGrow_Flag, !value); }
    }

    /// <summary>
    /// Одновременная установка свойств <see cref="Width"/> и <see cref="AutoGrow"/>.
    /// </summary>
    /// <param name="width">Ширина столбца в единицах 0.1мм</param>
    /// <param name="autoGrow">Автоматическое увеличение ширины столбца</param>
    public void SetWidth(int width, bool autoGrow)
    {
      Width = width;
      AutoGrow = autoGrow;
    }

    #endregion
  }

  /// <summary>
  /// Неабстрактная реализация <see cref="BRColumnInfo"/> с внутренним хранением данных.
  /// Используется для виртуальных таблиц
  /// </summary>
  internal sealed class BRColumnInfoStorage : BRColumnInfo
  {
    #region Конструктор

    public BRColumnInfoStorage(BRReport report)
    {
      _Report = report;
    }

    #endregion

    #region Доступ к данным

    private BRRowColumnData _Data;

    internal override int GetSize()
    {
      return _Data.GetSize();
    }

    internal override void SetSize(int value)
    {
      _Data.SetSize(value);
    }

    internal override bool GetFlag(int flag)
    {
      return _Data.GetFlag(flag);
    }

    internal override void SetFlag(int flag, bool value)
    {
      _Data.SetFlag(flag, value);
    }

    internal void Clear()
    {
      _Data = new BRRowColumnData();
    }

    internal override BRReport Report { get { return _Report; } }
    private BRReport _Report;

    #endregion
  }

  /// <summary>
  /// Именованный стиль ячеек
  /// </summary>
  public sealed class BRNamedCellStyle : BRCellStyleStorage, IObjectWithCode
  {
    #region Защищенный конструктор

    internal BRNamedCellStyle(string name, BRCellStyle parent)
      : base(parent)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      _Name = name;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя стиля. Задается в конструкторе
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    string IObjectWithCode.Code { get { return _Name; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Name;
    }

    #endregion

    #region Обработка значений

    internal override void SetValue(int index, object value)
    {
      if (index == Index_ParentStyleName)
        throw new InvalidOperationException("Нельзя устанавливать это свойство");
      base.SetValue(index, value);
    }

    #endregion
  }
}
