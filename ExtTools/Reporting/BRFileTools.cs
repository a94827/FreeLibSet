// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.Reporting;
using System.Text.RegularExpressions;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Вспомогательные методы, используемые при экспорте отчетов
  /// </summary>
  internal static class BRFileTools
  {
    #region InitCellStyleKey()

    public static void InitCellStyleKey(StringBuilder sb, BRSelector sel, int fontWidthPercent)
    {
      sb.Length = 0;
      sb.Append(sel.CellStyle.Format);
      sb.Append("|");
      if (sel.CellStyle.HAlign == BRHAlign.Auto)
      {
        sb.Append("A-");
        sb.Append(sel.ActualHAlign.ToString()); // надо учитывать автоматическое выравнивание для типов данных
      }
      else
        sb.Append(sel.CellStyle.HAlign.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.VAlign.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.WrapMode.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.IndentLevel.ToString());
      sb.Append("|");

      sb.Append(sel.CellStyle.FontName);
      sb.Append("|");
      sb.Append(sel.CellStyle.FontHeightTwip.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.LineHeightTwip.ToString());
      sb.Append("|");
      if (fontWidthPercent == 0)
      {
        sb.Append(sel.CellStyle.FontWidthPercent.ToString());
        sb.Append("|");
        sb.Append(sel.CellStyle.FontWidthPt.ToString());
        sb.Append("|");
        sb.Append(sel.CellStyle.MaxEnlargePercent.ToString());
        sb.Append("|");
        sb.Append(sel.CellStyle.AlwaysEnlarge.ToString());
        sb.Append("|");
      }
      else
      {
        sb.Append(fontWidthPercent.ToString());
        sb.Append("||||");
      }
      sb.Append(sel.CellStyle.BackColor.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.ForeColor.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.Bold?'1':'0');
      sb.Append(sel.CellStyle.Italic ? '1' : '0');
      sb.Append(sel.CellStyle.Underline ? '1' : '0');
      sb.Append(sel.CellStyle.Strikeout ? '1' : '0');
      sb.Append("|");

      sb.Append(sel.CellStyle.TextFiller);
      sb.Append("|");

      sb.Append(sel.CellStyle.LeftBorder.Style.ToString());
      sb.Append(sel.CellStyle.LeftBorder.Color.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.TopBorder.Style.ToString());
      sb.Append(sel.CellStyle.TopBorder.Color.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.RightBorder.Style.ToString());
      sb.Append(sel.CellStyle.RightBorder.Color.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.BottomBorder.Style.ToString());
      sb.Append(sel.CellStyle.BottomBorder.Color.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.DiagonalUp.Style.ToString());
      sb.Append(sel.CellStyle.DiagonalUp.Color.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.DiagonalDown.Style.ToString());
      sb.Append(sel.CellStyle.DiagonalDown.Color.ToString());
      sb.Append("|");
      sb.Append(sel.HasLink ? '1' : '0');
    }

    #endregion

    #region Плохие символы

    /// <summary>
    /// Список плохих символов, которые не могут использоваться в значении.
    /// Символы '\r' и '\n' не входят в список и должны обрабатываться отдельно
    /// </summary>
    internal static readonly string BadValueChars = CreateBadValueChars();

    private static string CreateBadValueChars()
    {
      StringBuilder sb = new StringBuilder(32);
      for (int i = 0; i < 32; i++)
      {
        char ch = (char)i;
        if (ch == '\n' || ch == '\r')
          continue;
        sb.Append(ch);
      }

      return sb.ToString();
    }

    #endregion

    #region XML

#if !XXX
    /// <summary>
    /// Добавляет текст к элементу, заменяя управляющие символы 
    /// </summary>
    /// <param name="el"></param>
    /// <param name="text"></param>
    public static void AppendText(XmlElement el, string text)
    {
#if !XXX // Так не работает
      int lastCount = 0;
      for (int i = 0; i < text.Length; i++)
      {
        if (text[i] < ' ')
        {
          if (i > lastCount)
          {
            string substr = text.Substring(lastCount, i - lastCount);
            XmlText txtNode = el.OwnerDocument.CreateTextNode(substr);
            el.AppendChild(txtNode);
          }
          XmlSignificantWhitespace wsp = el.OwnerDocument.CreateSignificantWhitespace(text.Substring(i, 1));
          el.AppendChild(wsp);
          lastCount = i + 1;
        }
      }
      if (lastCount < text.Length)
      {
        string substr;
        if (lastCount > 0)
          substr = text.Substring(lastCount, text.Length - lastCount);
        else
          substr = text;
        XmlText txtNode = el.OwnerDocument.CreateTextNode(substr);
        el.AppendChild(txtNode);
      }
#else

      for (int i = 0; i < text.Length; i++)
      {
        if (text[i] < ' ')
        {
          // Требуется замена
          StringBuilder sb = new StringBuilder();
          if (i > 0)
            sb.Append(text.Substring(0, i));
          for (int j = i; j < text.Length; j++)
          {
            if (text[j] < ' ')
            {
              sb.Append("&#");
              int nChar = (int)(text[j]);
              sb.Append(StdConvert.ToString(nChar));
              sb.Append(";");
            }
            else
              sb.Append(text[j]);
          }
          XmlText txtNode1 = el.OwnerDocument.CreateTextNode(sb.ToString());
          el.AppendChild(txtNode1);
          return;
        }
      }

      // Обычный текст
      XmlText txtNode2 = el.OwnerDocument.CreateTextNode(text);
      el.AppendChild(txtNode2);
#endif
    }
#endif

    #endregion

    #region Отступы

    /// <summary>
    /// Возвращает true, если отличаются отступы
    /// </summary>
    /// <param name="style1"></param>
    /// <param name="style2"></param>
    /// <returns></returns>
    internal static bool AreMarginsDifferent(BRCellStyle style1, BRCellStyle style2)
    {
      return !(style1.LeftMargin == style2.LeftMargin &&
        style1.TopMargin == style2.TopMargin &&
        style1.RightMargin == style2.RightMargin &&
        style1.BottomMargin == style2.BottomMargin);
    }

    #endregion

    #region TextFiller

    public const string TextFillerStrThin = "-"; 

    public const string TextFillerStrMedium = "\x2500"; // горизонтальная линия псевдографики

    public const string TextFillerStrThick = "\x25AC"; // "Black Rectangle"
                                                    // "\x2584"; // "Lower Block"

    public const string TextFillerStrTwoLines = "\x2550"; // двойная горизонтальная линия псевдографики

    #endregion

    #region Проведение дополнительных измерений

    /// <summary>
    /// Псевдоотчет из одной ячейки для выполнения измерений
    /// </summary>
    private static readonly BRReport _DummyReport = CreateDummyReport();

    const string TestString = "0000000000";

    private static BRReport CreateDummyReport()
    {
      BRReport report = new BRReport();
      BRSection sect = report.Sections.Add();
      BRTable table = sect.Bands.Add(1, 1);
      return report;
    }

    /// <summary>
    /// Возвращает ширину символов в процентах с учетом значений <see cref="BRCellStyle.FontWidthTwip"/> и <see cref="BRCellStyle.FontWidthPercent"/>
    /// </summary>
    /// <param name="cellStyle">Отсюда берутся параметры шрифта</param>
    /// <param name="measurer">Измеритель</param>
    /// <returns>Процент ширины шрифта (100% - обычный шрифт)</returns>
    private static int DoGetFontWidthPercent(BRCellStyle cellStyle, IBRMeasurer measurer)
    {
      if (cellStyle.FontWidthPercent != 0 && cellStyle.FontWidthPercent != 100)
        return cellStyle.FontWidthPercent;
      if (cellStyle.FontWidthTwip != 0)
      {
        _DummyReport.DefaultCellStyle.FontName = cellStyle.FontName;
        _DummyReport.DefaultCellStyle.FontHeightTwip = cellStyle.FontHeightTwip;
        _DummyReport.DefaultCellStyle.LineHeightTwip = 0;
        //_DummyReport.DefaultCellStyle.FontWidthTwip = 0;
        _DummyReport.DefaultCellStyle.FontWidthPercent = 100;
        _DummyReport.DefaultCellStyle.Bold = cellStyle.Bold;
        _DummyReport.DefaultCellStyle.Italic = cellStyle.Italic;
        _DummyReport.DefaultCellStyle.Underline = cellStyle.Underline;
        _DummyReport.DefaultCellStyle.Strikeout = cellStyle.Strikeout;

        int w1, w2, h;
        measurer.MeasureString(TestString, _DummyReport.DefaultCellStyle, out w1, out h);
        _DummyReport.DefaultCellStyle.FontWidthTwip = cellStyle.FontWidthTwip;
        measurer.MeasureString(TestString, _DummyReport.DefaultCellStyle, out w2, out h);
        return w2 * 100 / w1;
      }
      return 100;
    }

    /// <summary>
    /// Возвращает ширину символов в процентах с учетом значений <see cref="BRCellStyle.FontWidthTwip"/>, 
    /// <see cref="BRCellStyle.FontWidthPercent"/>, <see cref="BRCellStyle.WrapMode"/>, <see cref="BRCellStyle.MaxEnlargePercent"/> и <see cref="BRCellStyle.AlwaysEnlarge"/>
    /// </summary>
    /// <param name="sel">Отсюда берутся параметры шрифта</param>
    /// <param name="measurer">Измеритель</param>
    /// <param name="columnWidth">Ширина столбца в единицах 0.1мм</param>
    /// <returns>Процент ширины шрифта (100% - обычный шрифт)</returns>
    public static int GetFontWidthPercent(BRSelector sel, IBRMeasurer measurer, int columnWidth)
    {
      int textW, textH;
      MeasureString(sel, measurer, out textW, out textH);
      int availableW = columnWidth - sel.CellStyle.LeftMargin - sel.CellStyle.RightMargin - GetIndentWidth(sel.CellStyle, measurer);
      int widthPercent = DoGetFontWidthPercent(sel.CellStyle, measurer);
      if (sel.CellStyle.WrapMode == BRWrapMode.NoWrap)
      {
        if (textW > availableW)
          widthPercent = widthPercent * availableW / textW;
      }

      if (sel.CellStyle.MaxEnlargePercent > 100 && textW < availableW)
      {
        int enPrc = availableW * 100 / textW;
        if (enPrc > sel.CellStyle.MaxEnlargePercent)
        {
          if (sel.CellStyle.AlwaysEnlarge)
            enPrc = sel.CellStyle.MaxEnlargePercent;
          else
            enPrc = 100;
        }
        widthPercent = widthPercent * enPrc / 100;
      }
      return widthPercent;
    }

    /// <summary>
    /// Измеряется все строки для <see cref="BRSelector.AsString"/>
    /// </summary>
    /// <param name="sel"></param>
    /// <param name="measurer"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    public static void MeasureString(BRSelector sel, IBRMeasurer measurer, out int w, out int h)
    {
      w = 0;
      h = 0;
      string s = sel.AsString;
      if (String.IsNullOrEmpty(s))
        return;

      string[] a = s.Split(StringTools.NewLineSeparators, StringSplitOptions.None);
      for (int i = 0; i < a.Length; i++)
      {
        int w1, h1;
        measurer.MeasureString(BRPaginator.PrepareStringForMeasure(a[i]), sel.CellStyle, out w1, out h1);
        w = Math.Max(w, w1);
        h += h1;
      }
    }

    /// <summary>
    /// Возвращает величину отступа, соответствующую <see cref="BRCellStyle.IndentLevel"/>, в единицах 0.1мм
    /// </summary>
    /// <param name="cellStyle"></param>
    /// <param name="measurer"></param>
    /// <returns></returns>
    public static int GetIndentWidth(BRCellStyle cellStyle, IBRMeasurer measurer)
    {
      if (cellStyle.IndentLevel == 0)
        return 0;
      int w, h;
      measurer.MeasureString("0", cellStyle, out w, out h);
      return w * cellStyle.IndentLevel;
    }

    #endregion

    #region Цвета

    /// <summary>
    /// Цвет для обычной гиперссылок
    /// </summary>
    public static readonly BRColor LinkForeColor = new BRColor(0x05, 0x63, 0xC1); // взято из MS Office 2003

    /// <summary>
    /// Цвет для посещенной гиперссылки
    /// </summary>
    public static readonly BRColor VisitedLinkForeColor = new BRColor(0x95, 0x4F, 0x72); // взято из MS Office 2003

    #endregion

    #region Закладки

    /// <summary>
    /// Возвращает исправленное имя закладки для Excel/Calc.
    /// Если <see cref="BRBookmark.Name"/> задает имя закладки, которое может быть спутано с адресом ячейки, 
    /// например, "A1", то к имени добавляется префикс.
    /// </summary>
    /// <param name="name">Имя закладки <see cref="BRBookmark.Name"/></param>
    /// <returns>Исправленное имя</returns>
    public static string GetExcelBookmarkName(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      if (Regex.IsMatch(name, "^[A-Za-z]+[0-9]+$"))
        return "BM__" + name;
      else
        return name;
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для создания файлов из отчета <see cref="BRReport"/>
  /// </summary>
  public abstract class BRFileCreator
  {
    /// <summary>
    /// Создает файл из отчета.
    /// Создает каталог <paramref name="filePath"/>.ParentDir, в котором будет создан файл, а затем вызывает
    /// абстрактный метод <see cref="DoCreateFile(BRReport, FreeLibSet.IO.AbsPath)"/>, который выполняет создание файла
    /// </summary>
    /// <param name="report">Созданный отчет</param>
    /// <param name="filePath">Путь к записываемому файлу</param>
    public void CreateFile(BRReport report, FreeLibSet.IO.AbsPath filePath)
    {
      if (report == null)
        throw new ArgumentNullException("report");
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");
      FreeLibSet.IO.FileTools.ForceDirs(filePath.ParentDir);
      DoCreateFile(report, filePath);
    }

    /// <summary>
    /// Этот метод отвечает за создание файлов
    /// </summary>
    /// <param name="report"></param>
    /// <param name="filePath"></param>
    protected abstract void DoCreateFile(BRReport report, FreeLibSet.IO.AbsPath filePath);

    /// <summary>
    /// Интерфейс управления заставкой. В частности, позволяет прерывать процесс создания файла.
    /// Устанавливается вызывающим кодом на время вызова <see cref="CreateFile(BRReport, FreeLibSet.IO.AbsPath)"/>.
    /// Если свойство не будет установлено, используется заглушка.
    /// Реализация метода создания файла может игнорировать это свойство.
    /// </summary>
    public ISimpleSplash Splash
    {
      get
      {
        if (_Splash == null)
          _Splash = new DummySimpleSplash();
        return _Splash;
      }
      set { _Splash = value; }
    }
    private ISimpleSplash _Splash;
  }
}
