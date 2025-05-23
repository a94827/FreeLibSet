﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Reporting;

// TODO: Используется в EFPDataGridViewCommandItems.PerformCopy() для сборки HTML-формата

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Параметры для экспорта табличного просмотра в HTML-файл
  /// </summary>
  internal class EFPDataGridViewExpHtmlSettings
  {
    #region Конструктор

    /// <summary>
    /// Инициализация параметров по умолчанию
    /// </summary>
    public EFPDataGridViewExpHtmlSettings()
    {
      _RangeMode = EFPDataViewExpRange.All;
      _ShowColumnHeaders = true;
      _Encoding = Encoding.UTF8;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Диапазон ячеек для экспорта (по умолчанию - All)
    /// </summary>
    public EFPDataViewExpRange RangeMode { get { return _RangeMode; } set { _RangeMode = value; } }
    private EFPDataViewExpRange _RangeMode;

    /// <summary>
    /// true (по умолчанию), если заголовки столбцов должны быть добавлены в таблицу
    /// </summary>
    public bool ShowColumnHeaders { get { return _ShowColumnHeaders; } set { _ShowColumnHeaders = value; } }
    private bool _ShowColumnHeaders;

    /// <summary>
    /// Кодировка для записи файла (по умолчанию - UTF8)
    /// </summary>
    public Encoding Encoding { get { return _Encoding; } set { _Encoding = value; } }
    private Encoding _Encoding;

    #endregion
  }

  /// <summary>
  /// Вспомогательный класс для экспорта табличного просмотра в формате HTML
  /// </summary>
  internal static class EFPDataGridViewExpHtml
  {
    #region Экспорт в HTML

    /// <summary>
    /// Запись в HTML-файл на диске
    /// </summary>
    /// <param name="controlProvider"></param>
    /// <param name="fileName"></param>
    /// <param name="settings"></param>
    public static void SaveFile(EFPDataGridView controlProvider, string fileName, EFPDataGridViewExpHtmlSettings settings)
    {
      StreamWriter wrt = new StreamWriter(fileName, false, settings.Encoding);
      try
      {
        WriteHtml(controlProvider, wrt, wrt.BaseStream, settings, false);
        wrt.Close();
      }
      finally
      {
        wrt.Dispose();
      }
    }

#if XXXXXXXXXXXXX

    /// <summary>
    /// Получить HTML-представление в виде строки текста
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public string GetHtmlText()
    {
      Encoding Encoding = Encoding.UTF8;
      MemoryStream strm = new MemoryStream();
      StreamWriter wrt = new StreamWriter(strm, Encoding);
      string res;
      try
      {
        WriteHtml(wrt, strm, true);
        strm.Seek(0, SeekOrigin.Begin);
        StreamReader rdr = new StreamReader(strm, Encoding);
        try
        {
          res = rdr.ReadToEnd();
        }
        finally
        {
          rdr.Dispose();
        }
      }
      finally
      {
        wrt.Dispose();
        strm.Dispose();
      }
      return res;
    }
#endif

    public static byte[] GetHtmlBytes(EFPDataGridView controlProvider, EFPDataGridViewExpHtmlSettings settings, bool useFragment)
    {
      MemoryStream strm = new MemoryStream();
      StreamWriter wrt = new StreamWriter(strm, settings.Encoding);
      byte[] buffer = null;
      try
      {
        if (useFragment)
        {
          wrt.Write("");
          wrt.Flush();
          strm.Flush();
          strm.SetLength(0);
        }
        WriteHtml(controlProvider, wrt, strm, settings, useFragment);
        buffer = strm.ToArray();
      }
      finally
      {
        wrt.Dispose();
        strm.Dispose();
      }
      return buffer;
    }

    //public string GetHtmlText(Encoding Encoding, bool UseFragment)
    //{
    //  // Через StringWriter не удобно, т.к. WriteHtml хочет объект Stream
    //  // !! не оптимально
    //  byte[] bytes=GetHtmlBytes(Encoding, UseFragment);
    //  string s = Encoding.GetString(bytes);
    //  if (s.Length != bytes.Length)
    //  {
    //  }
    //  return s;
    //}

    private static void WriteHtml(EFPDataGridView controlProvider, TextWriter wrt, Stream strm, EFPDataGridViewExpHtmlSettings settings, bool useFragment)
    {
      const int PosWrStartHtml = 23;
      const int PosWrEndHtml = 43;
      const int PosWrStartFragment = 70;
      const int PosWrEndFragment = 93;

      EFPDataGridViewRectArea area = controlProvider.GetRectArea(settings.RangeMode);

      wrt.Flush();
      strm.Flush();
      strm.Seek(0, SeekOrigin.End);
      int posOff = (int)(strm.Position);

      if (useFragment)
      {
        wrt.WriteLine("Version:1.0");
        wrt.WriteLine("StartHTML:0000000000");
        wrt.WriteLine("EndHTML:0000000000");
        wrt.WriteLine("StartFragment:0000000000");
        wrt.WriteLine("EndFragment:0000000000");
        wrt.WriteLine();
      }

      BRDocumentProperties docProps = controlProvider.DocumentProperties;

      int offStartHtml = GetHtmlOff(wrt, strm);
      wrt.WriteLine("<HTML>");
      wrt.WriteLine("<HEAD>");
      if (!String.IsNullOrEmpty(docProps.Title))
        wrt.WriteLine("<title>" + MakeHtmlSpc(docProps.Title) + "</title>");
      // Если есть строка, то IE показывает бяку при кодировке DOS и KOI8
      // (Word и Excel работают)
      //Writer.WriteLine("  <META NAME=GENERATOR CONTENT=\"CS1 by Ageyev A.V.\"");
      string CharSetText = wrt.Encoding.WebName;
      wrt.WriteLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=" + CharSetText + "\">");
      wrt.WriteLine("</HEAD>");
      wrt.WriteLine("<BODY lang=RU>");

      if (!useFragment) // 01.10.2020
      {
        if (!String.IsNullOrEmpty(docProps.Title))
        {
          wrt.WriteLine("<P NOWARP ALIGN=CENTER><TT>" + MakeHtmlSpc(docProps.Title));
        }
      }

      // all centered (including table) from here
      wrt.WriteLine("<CENTER>");

      // define table display format (border and cell look)
      // and structure (number of columns)
      wrt.WriteLine("<TABLE BORDER=1 CellPadding=4 CellSpacing=2 COLS=" + area.ColumnCount.ToString() + ">");

      if (useFragment)
        wrt.WriteLine("<!--StartFragment-->");
      int offStartFragment = GetHtmlOff(wrt, strm);

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

      wrt.WriteLine("</BODY>");
      wrt.WriteLine("</HTML>");
      int offEndHtml = GetHtmlOff(wrt, strm) - 2; // CRLF

      wrt.Flush();
      strm.Flush();
      if (useFragment)
      {
        WriteHtmlOff(strm, PosWrStartHtml + posOff, offStartHtml - posOff);
        WriteHtmlOff(strm, PosWrEndHtml + posOff, offEndHtml - posOff);
        WriteHtmlOff(strm, PosWrStartFragment + posOff, offStartFragment - posOff);
        WriteHtmlOff(strm, PosWrEndFragment + posOff, offEndFragment - posOff);
      }
    }

    /// <summary>
    /// Запись значения смещения в указанную позицию файла
    /// Позиция записывается в виде десятичного числа в формате 0000000000 (10 разрядов)
    /// </summary>
    /// <param name="strm"></param>
    /// <param name="posWr">Позиция начала заглушки 0000000000</param>
    /// <param name="value">Записываемое значение</param>
    private static void WriteHtmlOff(Stream strm, int posWr, int value)
    {
      if (!strm.CanSeek)
        throw new NotSupportedException("Stream does not support Seek()");
      string text = value.ToString("d10");
      strm.Seek(posWr, SeekOrigin.Begin);
      for (int i = 0; i < text.Length; i++)
      {
        byte b = (byte)(text[i]);
        strm.WriteByte(b);
      }
    }

    private static int GetHtmlOff(TextWriter wrt, Stream strm)
    {
      wrt.Flush();
      strm.Flush();
      strm.Seek(0, SeekOrigin.End);
      return (int)(strm.Position);
    }

    private static void WriteHtmlRow(TextWriter wrt, int rowIndex, EFPDataGridView controlProvider, EFPDataGridViewRectArea area/*, EFPDataGridViewExpHtmlSettings settings*/)
    {
      wrt.WriteLine("<TR>");
      controlProvider.DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.View);
      for (int j = 0; j < area.ColumnCount; j++)
      {
        int columnIndex = area.ColumnIndices[j];
        EFPDataGridViewCellAttributesEventArgs cellArgs = controlProvider.DoGetCellAttributes(columnIndex);

        // 06.01.2014 - убрано
        // if (Selection && !GridHandler.IsCellSelected(RowIndex, ColumnIndex))
        // {
        //   // Для невыбранных ячеек оставляем пустое место
        //  wrt.WriteLine("<TD>&nbsp</TD>");
        //   continue;
        // }

        string sFormat = String.Empty;
        string sNum = String.Empty;
        if (!String.IsNullOrEmpty(cellArgs.CellStyle.Format))
          sFormat = " STYLE=\"vnd.ms-excel.numberformat:" + cellArgs.CellStyle.Format + "\"";

        string txt;
        if (cellArgs.Value == null ||
          cellArgs.Value is Image ||
          (!cellArgs.ContentVisible)) // 24.08.2015
          txt = "&nbsp";
        else
        {
          if (cellArgs.Value is Boolean)
          {
            if ((bool)(cellArgs.Value))
              txt = "[x]";
            else
              txt = "[&nbsp]";
          }
          else
          {
            txt = cellArgs.Value.ToString().Trim();
            if (String.IsNullOrEmpty(txt))
              txt = "&nbsp";
            else
            {
              txt = MakeHtmlSpc(txt);
              if (cellArgs.OriginalValue != null)
              {
                if (cellArgs.OriginalValue is String)
                  sFormat = " STYLE=\"vnd.ms-excel.numberformat:@\"";
                if (DataTools.IsIntegerType(cellArgs.OriginalValue.GetType()) ||
                  DataTools.IsFloatType(cellArgs.OriginalValue.GetType()))
                  sNum = " x:num=\"" + Convert.ToString(cellArgs.OriginalValue, StdConvert.NumberFormat) + "\"";
              }
            }
          }
        }
        string sHAlign;
        string sStyle = String.Empty;
        switch (cellArgs.CellStyle.Alignment)
        {
          case DataGridViewContentAlignment.TopCenter:
          case DataGridViewContentAlignment.MiddleCenter:
          case DataGridViewContentAlignment.BottomCenter:
            sHAlign = " align=center";
            break;
          case DataGridViewContentAlignment.TopRight:
          case DataGridViewContentAlignment.MiddleRight:
          case DataGridViewContentAlignment.BottomRight:
            sHAlign = " align=right";
            break;
          default:
            sHAlign = String.Empty;
            break;
        }
        string sVAlign;
        switch (cellArgs.CellStyle.Alignment)
        {
          case DataGridViewContentAlignment.TopLeft:
          case DataGridViewContentAlignment.TopCenter:
          case DataGridViewContentAlignment.TopRight:
            sVAlign = " valign=top";
            sStyle += "vertical-align:top;";
            break;
          case DataGridViewContentAlignment.MiddleLeft:
          case DataGridViewContentAlignment.MiddleCenter:
          case DataGridViewContentAlignment.MiddleRight:
            sVAlign = " valign=middle";
            sStyle += "vertical-align:middle;";
            break;
          default:
            sVAlign = " valign=bottom";
            sStyle += "vertical-align:bottom;";
            break;
        }

        if (cellArgs.IndentLevel > 0)
        {
          if (cellArgs.CellStyle.Alignment == DataGridViewContentAlignment.TopRight ||
            cellArgs.CellStyle.Alignment == DataGridViewContentAlignment.MiddleRight ||
            cellArgs.CellStyle.Alignment == DataGridViewContentAlignment.BottomRight)
          {
            //sStyle += "padding-left:" + (12 * CellArgs.IndentLevel).ToString() + "px;";
            sStyle += "padding-right:" + (12 * cellArgs.IndentLevel).ToString() + "px;"; // 27.12.2020. Хотя вряд ли выравнивание по правому краю с отступом встретится
          }
          else
          {
            sStyle += "padding-left:" + (12 * cellArgs.IndentLevel).ToString() + "px;";
            sStyle += "mso-char-indent-count:" + cellArgs.IndentLevel.ToString() + ";";
          }
        }
        if (!String.IsNullOrEmpty(sStyle))
          sStyle = " style='" + sStyle + "'";

        Color backColor, foreColor;
        EFPDataGridView.SetCellAttr(cellArgs.ColorType, cellArgs.Grayed, cellArgs.ContentVisible, out backColor, out foreColor);
        string cBkColor;
        if (backColor.IsEmpty)
          cBkColor = String.Empty;
        else
          cBkColor = " BGCOLOR=\"" + GetHtmlColor(backColor) + "\"";

        string cFont1, cFont2;
        if (foreColor.IsEmpty)
        {
          cFont1 = String.Empty;
          cFont2 = String.Empty;
        }
        else
        {
          cFont1 = "<FONT COLOR=\"" + GetHtmlColor(foreColor) + "\">";
          cFont2 = "</FONT>";
        }

        wrt.WriteLine("<TD" + sVAlign + sHAlign + cBkColor + sStyle + sFormat + sNum + ">" + cFont1 + "<TT>" + txt + cFont2 + "</TD>");
      }
      wrt.WriteLine("</TR>");
    }

    /// <summary>
    /// Преобразование в HTML-цвет в формате "#RRGGBB"
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static string GetHtmlColor(Color color)
    {
      StringBuilder sb = new StringBuilder(7);
      sb.Append('#');
      sb.Append(color.R.ToString("x2"));
      sb.Append(color.G.ToString("x2"));
      sb.Append(color.B.ToString("x2"));
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
