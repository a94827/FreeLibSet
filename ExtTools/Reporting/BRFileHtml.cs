using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.IO;

#pragma warning disable 1591
#pragma warning disable 0219

namespace FreeLibSet.Reporting
{
  public class BRFileHtml
  {
    #region Конструктор

    public BRFileHtml()
    {
      _Encoding = Encoding.UTF8;
    }

    #endregion

    #region Свойства

    public Encoding Encoding { get { return _Encoding; } set { _Encoding = value; } }
    private Encoding _Encoding;

    #endregion

    #region Запись

    public void CreateFile(BRReport report, AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");
      using (FileStream fs = new FileStream(filePath.Path, FileMode.Create))
      {
        Write(report, fs, false);
        fs.Flush();
      }
    }

    public void Write(BRReport report, Stream stream, bool useFragment)
    {
      StreamWriter wrt = new StreamWriter(stream, Encoding);
      const int PosWrStartHtml = 23;
      const int PosWrEndHtml = 43;
      const int PosWrStartFragment = 70;
      const int PosWrEndFragment = 93;

      int posOff = -1;
      int offStartHtml = -1;
      if (useFragment)
      {
        stream.Flush();
        stream.Seek(0, SeekOrigin.End);
        posOff = (int)(stream.Position);

        wrt.WriteLine("Version:1.0");
        wrt.WriteLine("StartHTML:0000000000");
        wrt.WriteLine("EndHTML:0000000000");
        wrt.WriteLine("StartFragment:0000000000");
        wrt.WriteLine("EndFragment:0000000000");
        wrt.WriteLine();

        offStartHtml = GetHtmlOff(wrt, stream);
      }

      BRDocumentProperties docProps = report.DocumentProperties;

      wrt.WriteLine("<HTML>");
      wrt.WriteLine("<HEAD>");
      if (!String.IsNullOrEmpty(docProps.Title))
        wrt.WriteLine("<TITLE>" + MakeHtmlSpc(docProps.Title) + "</TITLE>");
      // Если есть строка, то IE показывает бяку при кодировке DOS и KOI8
      // (Word и Excel работают)
      //Writer.WriteLine("  <META NAME=GENERATOR CONTENT=\"CS1 by Ageyev A.V.\"");
      string charSetText = wrt.Encoding.WebName;
      wrt.WriteLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; CHARSET=" + charSetText + "\"/>");
      wrt.WriteLine("</HEAD>");
      StringBuilder sb = new StringBuilder();
      sb.Append("<BODY STYLE=\"");

      sb.Append("font-family: ");
      sb.Append(report.DefaultCellStyle.FontName);
      sb.Append("; ");

      sb.Append("font-size: ");
      sb.Append(report.DefaultCellStyle.FontHeightPt.ToString("0.0", StdConvert.NumberFormat));
      sb.Append("pt; ");

      sb.Append("padding: ");
      Add01mm(sb, report.DefaultCellStyle.TopMargin);
      sb.Append(' ');
      Add01mm(sb, report.DefaultCellStyle.RightMargin);
      sb.Append(' ');
      Add01mm(sb, report.DefaultCellStyle.BottomMargin);
      sb.Append(' ');
      Add01mm(sb, report.DefaultCellStyle.LeftMargin);
      sb.Append("; ");

      sb.Append("border-collapse: collapse;");

      sb.Append("\">");
      wrt.WriteLine(sb.ToString());

      wrt.WriteLine("<FONT FACE=\"" + report.DefaultCellStyle.FontName + "\">");

      foreach (BRSection section in report.Sections)
        WriteSection(section, wrt);

#if XXX
      // all centered (including table) from here
      wrt.WriteLine("<CENTER>");

      // define table display format (border and cell look)
      // and structure (number of columns)
      wrt.WriteLine("<TABLE BORDER=1 CELLPADDING=4 CELLSPACING=2 COLS=" + area.ColumnCount.ToString() + ">");

      if (useFragment)
        wrt.WriteLine("<!--StartFragment-->");
      int OffStartFragment = GetHtmlOff(wrt, strm);

      // Ширина столбцов
      for (int j = 0; j < area.ColumnCount; j++)
      {
        DataGridViewColumn gridCol = area.Columns[j];
        wrt.WriteLine("<col width=" + gridCol.Width.ToString() + ">");
      }
      wrt.WriteLine();

      // Многострочные заголовки
      if (settings.ShowColumnHeaders)
      {
        BRColumnHeaderArray headerArray = controlProvider.GetColumnHeaderArray(area);

        for (int i = 0; i < headerArray.RowCount; i++)
        {
          wrt.WriteLine("<TR>");
          for (int j = 0; j < headerArray.ColumnCount; j++)
          {
            if (headerArray.RowSpan[i, j] == 0)
              continue; // не первая строка объединения
            string txt = headerArray.Text[i, j];
            if (String.IsNullOrEmpty(txt))
              txt = "&nbsp";
            else
              txt = MakeHtmlSpc(txt);
            txt = "<TT>" + txt;
            //string cBkColor=" BGCOLOR=\""+GetHtmlColor(Column.HeaderCell.InheritedStyle.BackColor)+"\"";
            wrt.Write("<TH");
            if (headerArray.RowSpan[i, j] > 1)
            {
              wrt.Write(" ROWSPAN=");
              wrt.Write(headerArray.RowSpan[i, j].ToString());
            }
            if (headerArray.ColumnSpan[i, j] > 1)
            {
              wrt.Write(" COLSPAN=");
              wrt.Write(headerArray.ColumnSpan[i, j].ToString());
            }
            wrt.Write(">");
            wrt.Write(txt);
            wrt.WriteLine("</TH>");
          }
          wrt.WriteLine("</TR>");
          wrt.WriteLine();
        }
      }

      // Перебираем строки таблицы
      for (int i = 0; i < area.RowCount; i++)
        WriteHtmlRow(wrt, area.RowIndices[i], controlProvider, area/*, settings*/);

      int offEndFragment = GetHtmlOff(wrt, strm);
      if (useFragment)
        wrt.WriteLine("<!--EndFragment-->");

      wrt.WriteLine("</TABLE>");
      wrt.WriteLine("</CENTER>");
#endif
      wrt.WriteLine("</FONT>");
      wrt.WriteLine("</BODY>");
      wrt.WriteLine("</HTML>");

      wrt.Flush();
      //stream.Flush();
      if (useFragment)
      {
        int OffEndHtml = GetHtmlOff(wrt, stream) - 2; // CRLF
        WriteHtmlOff(stream, PosWrStartHtml + posOff, offStartHtml - posOff);
        WriteHtmlOff(stream, PosWrEndHtml + posOff, OffEndHtml - posOff);
        // TODO: WriteHtmlOff(stream, PosWrStartFragment + posOff, OffStartFragment - posOff);
        // TODO: WriteHtmlOff(stream, PosWrEndFragment + posOff, offEndFragment - posOff);
      }
    }

    private static void Add01mm(StringBuilder sb, int value)
    {
      sb.Append((value / 10.0).ToString("0.0", StdConvert.NumberFormat));
      sb.Append("mm");
    }

    private void WriteSection(BRSection section, StreamWriter wrt)
    {
      StringBuilder sb = new StringBuilder(); // Открывающие теги 

      foreach (BRBand band in section.Bands)
      {
        BRSelector sel = band.CreateSelector();

        if (IsSimpleBand(sel))
        {
          sb.Length = 0;
          InitStyles(sel, section.Report.DefaultCellStyle, sb);
          // Простой абзац текста
          wrt.WriteLine("<P " + sb.ToString() + ">" + MakeHtmlSpc(sel.AsString) + "</P>");
        }
        else
        {
          string[] aTxtWidth = new string[band.ColumnCount];
          int totalWidth = 0;
          int totalGrowWidth = 0;
          for (int j = 0; j < band.ColumnCount; j++)
          {
            sel.ColumnIndex = j;
            aTxtWidth[j] = (sel.ColumnInfo.Width/10.0).ToString("0.0", StdConvert.NumberFormat)  + "mm";
            totalWidth += sel.ColumnInfo.Width;
            if (sel.ColumnInfo.AutoGrow)
              totalGrowWidth += sel.ColumnInfo.Width;
          }

          string txtTableWidth = String.Empty;
          if (totalGrowWidth > 0)
          {
            txtTableWidth = " WIDTH=100%";
            if (totalWidth < section.PageSetup.PrintAreaWidth)
            {
              for (int j = 0; j < band.ColumnCount; j++)
              {
                sel.ColumnIndex = j;
                if (sel.ColumnInfo.AutoGrow)
                {
                  int prc = sel.ColumnInfo.Width * 100 / totalGrowWidth;
                  aTxtWidth[j] = StdConvert.ToString(prc) + "%";
                }
              }
            }
          }

          wrt.WriteLine("<TABLE" + txtTableWidth + " COLS=" + band.ColumnCount + " CELLPADDING=0 CELLSPACING=0>");
          for (int j = 0; j < band.ColumnCount; j++)
          {
            wrt.WriteLine("  <COL WIDTH=" + aTxtWidth[j] + "/>");
          }

          for (int i = 0; i < band.RowCount; i++)
          {
            sel.RowIndex = i;
            wrt.WriteLine("  <TR>");
            for (int j = 0; j < band.ColumnCount; j++)
            {
              sel.ColumnIndex = j;

              if (!sel.IsMainCell)
                continue;

              string txt;
              if (String.IsNullOrEmpty(sel.AsString))
                txt = "&nbsp";
              else
              {
                txt = MakeHtmlSpc(sel.AsString);
                if (txt.IndexOf(Environment.NewLine) >= 0)
                {
                  string[] a = txt.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
                  for (int k = 0; k < (a.Length - 1); k++)
                    a[k] += "<BR>";
                  txt = String.Join("", a);
                }
              }

              sb.Length = 0;

              if (sel.RowInfo.Height != BRReport.AutoRowHeight)
              {
                sb.Append("height: ");
                sb.Append((sel.RowInfo.Height / 10.0).ToString("0.0", StdConvert.NumberFormat));
                sb.Append("mm; ");
              }
              InitStyles(sel, section.Report.DefaultCellStyle, sb);

              BRRange r = sel.MergeInfo;
              if (r.RowCount > 1)
              {
                sb.Append(" ROWSPAN=\"");
                sb.Append(StdConvert.ToString(r.RowCount));
                sb.Append("\"");
              }
              if (r.ColumnCount > 1)
              {
                sb.Append(" COLSPAN=\"");
                sb.Append(StdConvert.ToString(r.ColumnCount));
                sb.Append("\"");
              }

              wrt.WriteLine("    <TD" + sb.ToString() + ">" + txt + "</TD>");
            }
            wrt.WriteLine("  </TR>");
          }

          wrt.WriteLine("</TABLE>");
        }
      }
    }

    private static bool IsSimpleBand(BRSelector sel)
    {
      if (sel.Band.RowCount > 1 || sel.Band.ColumnCount > 1)
        return false;

      if (sel.RowInfo.Height != BRReport.AutoRowHeight)
        return false;

      string s = sel.AsString;
      if (s.IndexOf('\n')>=0 || s.IndexOf('\r')>=0)
        return false;

      return
        sel.CellStyle.LeftBorder == BRLine.None &&
        sel.CellStyle.TopBorder == BRLine.None &&
        sel.CellStyle.RightBorder == BRLine.None &&
        sel.CellStyle.BottomBorder == BRLine.None &&
        sel.CellStyle.DiagonalUp == BRLine.None &&
        sel.CellStyle.DiagonalDown == BRLine.None &&
        sel.CellStyle.BackColor == BRColor.Auto;
    }

    private static void InitStyles(BRSelector sel, BRCellStyle defaultStyle, StringBuilder sb)
    {
      BRCellStyle style = sel.CellStyle;

      if (!style.BackColor.IsAuto)
      {
        sb.Append("background-color: ");
        sb.Append(GetHtmlColor(style.BackColor));
        sb.Append("; ");
      }

      if (!style.ForeColor.IsAuto)
      {
        sb.Append("color: ");
        sb.Append(GetHtmlColor(style.ForeColor));
        sb.Append("; ");
      }

      if (style.AreaAllBordersSame)
        InitBorderStyle("border", style.LeftBorder, sb, style.ForeColor);
      else
      {
        InitBorderStyle("border-left", style.LeftBorder, sb, style.ForeColor);
        InitBorderStyle("border-top", style.TopBorder, sb, style.ForeColor);
        InitBorderStyle("border-right", style.RightBorder, sb, style.ForeColor);
        InitBorderStyle("border-bottom", style.BottomBorder, sb, style.ForeColor);
      }

      InitBorderStyle("mso-diagonal-down", style.DiagonalDown, sb, style.ForeColor); // будет видно только в Excel
      InitBorderStyle("mso-diagonal-up", style.DiagonalUp, sb, style.ForeColor);

      if (style.FontName != defaultStyle.FontName)
      {
        sb.Append("font-family: ");
        sb.Append(style.FontName);
        sb.Append("; ");
      }
      if (style.FontHeightTwip != defaultStyle.FontHeightTwip)
      {
        sb.Append("font-size: ");
        sb.Append(style.FontHeightPt.ToString("0.0", StdConvert.NumberFormat));
        sb.Append("pt; ");
      }
      if (style.LineHeightTwip != 0)
      {
        sb.Append("line-height: ");
        sb.Append(style.LineHeightPt.ToString("0.0", StdConvert.NumberFormat));
        sb.Append("pt; ");
      }

      if (style.Bold)
        sb.Append("font-weight: bold; ");
      if (style.Italic)
        sb.Append("font-style: italic; ");
      if (style.Underline || style.Strikeout)
      {
        sb.Append("text-decoration-line:");
        if (style.Underline)
          sb.Append(" underline");
        if (style.Strikeout)
          sb.Append(" line-through");
        sb.Append("; ");
      }

      switch (sel.ActualHAlign)
      {
        case BRHAlign.Center:
          sb.Append("text-align: center; ");
          break;
        case BRHAlign.Right:
          sb.Append("text-align: right; ");
          break;
      }
      switch (style.VAlign)
      {
        case BRVAlign.Top:
          sb.Append("vertical-align: top; ");
          break;
        case BRVAlign.Bottom:
          sb.Append("vertical-align: bottom; ");
          break;
      }

      if (!String.IsNullOrEmpty(style.Format))
      {
        sb.Append("vnd.ms-excel.numberformat: ");
        FormatAttrValue(sb, style.Format);
        sb.Append("; ");
      }
      else if (sel.Value is String)
      {
        sb.Append(" vnd.ms-excel.numberformat:@; ");
      }

      if (style.TopBorder != defaultStyle.TopBorder ||
        style.RightBorder != defaultStyle.RightBorder ||
        style.BottomBorder != defaultStyle.BottomBorder ||
        style.LeftBorder != defaultStyle.LeftBorder ||
        style.IndentLevel!=0)
      {
        int topMargin = style.TopMargin;
        int rightMargin = style.RightMargin;
        int bottomMargin = style.BottomMargin;
        int leftMargin = style.LeftMargin;

        if (style.IndentLevel > 0)
        {
          switch (sel.ActualHAlign)
          {
            case BRHAlign.Left:
              leftMargin += 30 * style.IndentLevel;
              break;
            case BRHAlign.Right:
              rightMargin += 30 * style.IndentLevel;
              break;
            case BRHAlign.Center:
              leftMargin += 15 * style.IndentLevel; 
              rightMargin += 15 * style.IndentLevel;
              break;
          }
        }


        sb.Append("padding: ");
        if (leftMargin == rightMargin)
        {
          if (topMargin == bottomMargin)
          {
            if (leftMargin == topMargin)
            {
              // 1 значение
              Add01mm(sb, topMargin);
            }
            else
            {
              // 2 значения
            }
          }
        }
        else
        {
          // 4 значения
          Add01mm(sb, topMargin);
          sb.Append(' ');
          Add01mm(sb, rightMargin);
          sb.Append(' ');
          Add01mm(sb, bottomMargin);
          sb.Append(' ');
          Add01mm(sb, leftMargin);
        }
        sb.Append(";");
      }

      if (sb.Length > 0)
      {
        if (sb[sb.Length - 2] == ';' && sb[sb.Length - 1] == ' ')
          sb = sb.Remove(sb.Length - 1, 1);
        sb.Insert(0, " STYLE=\"");
        sb.Append("\"");
      }

      // Атрибут x:num - отдельный, а не в составе style
      if (sel.Value != null)
      {
        if (DataTools.IsNumericType(sel.Value.GetType()))
        {
          sb.Append(" x:num=\"");
          sb.Append(((IFormattable)(sel.Value)).ToString("", StdConvert.NumberFormat));
          sb.Append("\"");
        }
      }
    }

    private static void InitBorderStyle(string styleName, BRLine border, StringBuilder sb, BRColor foreColor)
    {
      if (border.Style == BRLineStyle.None)
        return;

      sb.Append(styleName);
      sb.Append(": ");

      switch (border.Style)
      {
        case BRLineStyle.Thin: sb.Append("solid"); break;
        case BRLineStyle.Medium: sb.Append("2px solid"); break;
        case BRLineStyle.Thick: sb.Append("4px solid"); break;
        case BRLineStyle.Dot: sb.Append("dotted"); break;
        case BRLineStyle.Dash: sb.Append("dashed"); break;
        case BRLineStyle.DashDot: sb.Append("dashed"); break;
        case BRLineStyle.DashDotDot: sb.Append("dotted"); break;
        default: sb.Append("solid"); break;
      }
      if (border.Color.IsAuto)
      {
        if (!foreColor.IsAuto)
        {
          // Если не задать цвет рамки, то он будет равен цвету текста
          sb.Append(" ");
          sb.Append(GetHtmlColor(BRColor.Black));
        }
      }
      else
      {
        sb.Append(" ");
        sb.Append(GetHtmlColor(border.Color));
      }
      sb.Append(";");
    }

    /// <summary>
    /// Запись значения смещения в указанную позицию файла
    /// Позиция записывается в виде десятичного числа в формате 0000000000 (10 разрядов)
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="posWr">Позиция начала заглушки 0000000000</param>
    /// <param name="value">Записываемое значение</param>
    private static void WriteHtmlOff(Stream stream, int posWr, int value)
    {
      if (!stream.CanSeek)
        throw new NotSupportedException("Поток не поддерживает Seek");
      string text = value.ToString("d10");
      stream.Seek(posWr, SeekOrigin.Begin);
      for (int i = 0; i < text.Length; i++)
      {
        byte b = (byte)(text[i]);
        stream.WriteByte(b);
      }
    }

    private static int GetHtmlOff(TextWriter wrt, Stream strm)
    {
      wrt.Flush();
      strm.Flush();
      strm.Seek(0, SeekOrigin.End);
      return (int)(strm.Position);
    }

    /// <summary>
    /// Преобразование в HTML-цвет в формате "#RRGGBB"
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static string GetHtmlColor(BRColor color)
    {
      StringBuilder sb = new StringBuilder(7);
      sb.Append('#');
      sb.Append(color.R.ToString("x2", StdConvert.NumberFormat));
      sb.Append(color.G.ToString("x2", StdConvert.NumberFormat));
      sb.Append(color.B.ToString("x2", StdConvert.NumberFormat));
      return sb.ToString();
    }

    /// <summary>
    /// Замена пробелов. 
    /// Если в строке более одного пробела подряд, заменяем второй пробел на
    /// код 160 (неразрывный пробел)
    /// Также заменяем специальные символы "больше", "меньше" и "амперсанд"
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    internal static string MakeHtmlSpc(string txt)
    {
      if (String.IsNullOrEmpty(txt))
        return txt;
      StringBuilder sb = new StringBuilder(txt);
      // Убираем гадкие символы (заменяем их на точки)
      for (int i = 0; i < sb.Length; i++)
      {
        if (sb[i] < ' ' && sb[i] != '\r' && sb[i] != '\n')
          sb[i] = '.';
      }

      // Замена второго и далее пробелов на неразрывный пробел
      bool prevIsSpace = false;
      for (int i = 0; i < sb.Length; i++)
      {
        if (sb[i] == ' ')
        {
          if (prevIsSpace)
            sb[i] = DataTools.NonBreakSpaceChar;
          else
            prevIsSpace = true;
        }
        else
          prevIsSpace = false;
      }

      // Заменяем плохие символы на комбинации
      sb.Replace("&", "&amp;");
      sb.Replace("<", "&lt;");
      sb.Replace(">", "&gt;");

      return sb.ToString();
    }

    private static void FormatAttrValue(StringBuilder sb, string txt)
    {
      for (int i = 0; i < txt.Length; i++)
      {
        if (txt[i] < '0')
        {
          int cn = txt[i];
          sb.Append('\\');
          sb.Append(cn.ToString("x4", StdConvert.NumberFormat));
        }
        else
          sb.Append(txt[i]);
      }
    }

    #endregion
  }
}
