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
  public class BRReportHtmlWriter
  {
    #region Конструктор

    public BRReportHtmlWriter()
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
      wrt.WriteLine("<BODY lang=RU>");

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

    private void WriteSection(BRSection section, StreamWriter wrt)
    {
      foreach (BRBand band in section.Bands)
      {
        BRSelector sel = band.CreateSelector();
        if (IsSimpleBand(sel))
        {
          // Простой абзац текста
          wrt.WriteLine("<P>" + MakeHtmlSpc(sel.AsString) + "</P>");
        }
        else
        {
          wrt.WriteLine("<TABLE COLS=" + band.ColumnCount + ">");
          for (int j = 0; j < band.ColumnCount; j++)
          {
            sel.ColumnIndex = j;
            int wPt = (int)(sel.ColumnInfo.Width / 254.0 * 72.0);
            string txtWidth;
            if (sel.ColumnInfo.AutoGrow)
              txtWidth = StdConvert.ToString(wPt) + "*";
            else
              txtWidth = StdConvert.ToString(wPt) + "pt";
            wrt.WriteLine("  <COL WIDTH=" + txtWidth + "/>");
          }

          for (int i = 0; i < band.RowCount; i++)
          {
            sel.RowIndex = i;
            wrt.WriteLine("  <TR>");
            for (int j = 0; j < band.ColumnCount; j++)
            {
              sel.ColumnIndex = j;
              string txt;
              if (String.IsNullOrEmpty(sel.AsString))
                txt = "&nbsp";
              else
                txt = MakeHtmlSpc(sel.AsString);
              wrt.WriteLine("    <TD>" + txt + "</TD>");
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

      return
        sel.CellStyle.LeftBorder == BRLine.None &&
        sel.CellStyle.TopBorder == BRLine.None &&
        sel.CellStyle.RightBorder == BRLine.None &&
        sel.CellStyle.BottomBorder == BRLine.None &&
        sel.CellStyle.DiagonalUp == BRLine.None &&
        sel.CellStyle.DiagonalDown == BRLine.None;
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
      // ??? sb.Replace("  ", ???);
      //txt:=STRTRAN(txt, '  ', CHR(32)+CHR(160))

      // Заменяем плохие символы на комбинации
      sb.Replace("&", "&amp;");
      sb.Replace("<", "&lt;");
      sb.Replace(">", "&gt;");

      return sb.ToString();
    }

    #endregion
  }
}
