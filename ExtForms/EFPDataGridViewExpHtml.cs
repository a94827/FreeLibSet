using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

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
  #region Перечисление EFPDataGridViewExpRange

  /// <summary>
  /// Диапазон ячеек табличного просмотра для экспорта
  /// </summary>
  public enum EFPDataGridViewExpRange
  {
    // Члены не переименовывать!
    // Имена используются при сохранении конфигурации

    /// <summary>
    /// Все ячейки табличного просмотра (значение по умолчанию)
    /// </summary>
    All,

    /// <summary>
    /// Выбранные ячейки табличного просмотра
    /// </summary>
    Selected
  }

  #endregion

  /// <summary>
  /// Поля сводки документа, используемые при сохранении табличного просмотра
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct EFPDocumentProperties
  {
    #region Поля для сводки

    /// <summary>
    /// Сводка документа - название
    /// </summary>
    public string Title { get { return GetText(_Title); } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Сводка документа - тема
    /// </summary>
    public string Subject { get { return GetText(_Subject); } set { _Subject = value; } }
    private string _Subject;

    /// <summary>
    /// Сводка документа - автор
    /// </summary>
    public string Author { get { return GetText(_Author); } set { _Author = value; } }
    private string _Author;

    /// <summary>
    /// Сводка документа - учреждение
    /// </summary>
    public string Company { get { return GetText(_Company); } set { _Company = value; } }
    private string _Company;

    private static string GetText(string s)
    {
      if (s == null)
        return String.Empty;
      s = s.Replace(Environment.NewLine, " ");
      s = DataTools.ReplaceCharRange(s, (char)0, (char)31, ' ');
      s = s.Replace(DataTools.SoftHyphenStr, "");
      s = s.Replace(DataTools.NonBreakSpaceChar, ' ');
      return s;
    }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Title: ");
      sb.Append(Title);
      sb.Append(", Subject: ");
      sb.Append(Subject);
      sb.Append(", Author: ");
      sb.Append(Author);
      sb.Append(", Company: ");
      sb.Append(Company);
      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Параметры для экспорта табличного просмотра в HTML-файл
  /// </summary>
  public class EFPDataGridViewExpHtmlSettings
  {
    #region Конструктор

    /// <summary>
    /// Инициализация параметров по умолчанию
    /// </summary>
    public EFPDataGridViewExpHtmlSettings()
    {
      _RangeMode = EFPDataGridViewExpRange.All;
      _ShowColumnHeaders = true;
      _Encoding = Encoding.UTF8;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Диапазон ячеек для экспорта (по умолчанию - All)
    /// </summary>
    public EFPDataGridViewExpRange RangeMode { get { return _RangeMode; } set { _RangeMode = value; } }
    private EFPDataGridViewExpRange _RangeMode;

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
      byte[] Buffer = null;
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
        Buffer = strm.ToArray();
      }
      finally
      {
        wrt.Dispose();
        strm.Dispose();
      }
      return Buffer;
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

      EFPDataGridViewRectArea Area = controlProvider.GetRectArea(settings.RangeMode);

      wrt.Flush();
      strm.Flush();
      strm.Seek(0, SeekOrigin.End);
      int PosOff = (int)(strm.Position);

      if (useFragment)
      {
        wrt.WriteLine("Version:1.0");
        wrt.WriteLine("StartHTML:0000000000");
        wrt.WriteLine("EndHTML:0000000000");
        wrt.WriteLine("StartFragment:0000000000");
        wrt.WriteLine("EndFragment:0000000000");
        wrt.WriteLine();
      }

      EFPDocumentProperties Props = controlProvider.DocumentProperties;

      int OffStartHtml = GetHtmlOff(wrt, strm);
      wrt.WriteLine("<HTML>");
      wrt.WriteLine("<HEAD>");
      if (!String.IsNullOrEmpty(Props.Title))
        wrt.WriteLine("<title>" + MakeHtmlSpc(Props.Title) + "</title>");
      // Если есть строка, то IE показывает бяку при кодировке DOS и KOI8
      // (Word и Excel работают)
      //Writer.WriteLine("  <META NAME=GENERATOR CONTENT=\"CS1 by Ageyev A.V.\"");
      string CharSetText = wrt.Encoding.WebName;
      wrt.WriteLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=" + CharSetText + "\">");
      wrt.WriteLine("</HEAD>");
      wrt.WriteLine("<BODY lang=RU>");

      if (!useFragment) // 01.10.2020
      {
        if (!String.IsNullOrEmpty(Props.Title))
        {
          wrt.WriteLine("<P NOWARP ALIGN=CENTER><TT>" + MakeHtmlSpc(Props.Title));
        }
      }

      // all centered (including table) from here
      wrt.WriteLine("<CENTER>");

      // define table display format (border and cell look)
      // and structure (number of columns)
      wrt.WriteLine("<TABLE BORDER=1 CellPadding=4 CellSpacing=2 COLS=" + Area.ColumnCount.ToString() + ">");

      if (useFragment)
        wrt.WriteLine("<!--StartFragment-->");
      int OffStartFragment = GetHtmlOff(wrt, strm);

      // Ширина столбцов
      for (int j = 0; j < Area.ColumnCount; j++)
      {
        DataGridViewColumn Column = Area.Columns[j];
        wrt.WriteLine("<col width=" + Column.Width.ToString() + ">");
      }
      wrt.WriteLine();

      // Многострочные заголовки
      if (settings.ShowColumnHeaders)
      {
        EFPDataGridViewColumnHeaderArray HeaderArray = controlProvider.GetColumnHeaderArray(Area);

        for (int i = 0; i < HeaderArray.RowCount; i++)
        {
          wrt.WriteLine("<TR>");
          for (int j = 0; j < HeaderArray.ColumnCount; j++)
          {
            if (HeaderArray.RowSpan[i, j] == 0)
              continue; // не первая строка объединения
            string txt = HeaderArray.Text[i, j];
            if (String.IsNullOrEmpty(txt))
              txt = "&nbsp";
            else
              txt = MakeHtmlSpc(txt);
            txt = "<TT>" + txt;
            //string cBkColor=" BGCOLOR=\""+GetHtmlColor(Column.HeaderCell.InheritedStyle.BackColor)+"\"";
            wrt.Write("<TH");
            if (HeaderArray.RowSpan[i, j] > 1)
            {
              wrt.Write(" ROWSPAN=");
              wrt.Write(HeaderArray.RowSpan[i, j].ToString());
            }
            if (HeaderArray.ColumnSpan[i, j] > 1)
            {
              wrt.Write(" COLSPAN=");
              wrt.Write(HeaderArray.ColumnSpan[i, j].ToString());
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
      for (int i = 0; i < Area.RowCount; i++)
        WriteHtmlRow(wrt, Area.RowIndices[i], controlProvider, Area/*, settings*/);

      int OffEndFragment = GetHtmlOff(wrt, strm);
      if (useFragment)
        wrt.WriteLine("<!--EndFragment-->");

      wrt.WriteLine("</TABLE>");
      wrt.WriteLine("</CENTER>");

      wrt.WriteLine("</BODY>");
      wrt.WriteLine("</HTML>");
      int OffEndHtml = GetHtmlOff(wrt, strm) - 2; // CRLF

      wrt.Flush();
      strm.Flush();
      if (useFragment)
      {
        WriteHtmlOff(strm, PosWrStartHtml + PosOff, OffStartHtml - PosOff);
        WriteHtmlOff(strm, PosWrEndHtml + PosOff, OffEndHtml - PosOff);
        WriteHtmlOff(strm, PosWrStartFragment + PosOff, OffStartFragment - PosOff);
        WriteHtmlOff(strm, PosWrEndFragment + PosOff, OffEndFragment - PosOff);
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
        throw new NotSupportedException("Поток не поддерживает Seek");
      string Text = value.ToString("d10");
      strm.Seek(posWr, SeekOrigin.Begin);
      for (int i = 0; i < Text.Length; i++)
      {
        byte b = (byte)(Text[i]);
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
        int ColumnIndex = area.ColumnIndices[j];
        EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);

        // 06.01.2014 - убрано
        // if (Selection && !GridHandler.IsCellSelected(RowIndex, ColumnIndex))
        // {
        //   // Для невыбранных ячеек оставляем пустое место
        //  wrt.WriteLine("<TD>&nbsp</TD>");
        //   continue;
        // }

        string sFormat = String.Empty;
        string sNum = String.Empty;
        if (!String.IsNullOrEmpty(CellArgs.CellStyle.Format))
          sFormat = " STYLE=\"vnd.ms-excel.numberformat:" + CellArgs.CellStyle.Format + "\"";

        string txt;
        if (CellArgs.Value == null ||
          CellArgs.Value is Image ||
          (!CellArgs.ContentVisible)) // 24.08.2015
          txt = "&nbsp";
        else
        {
          if (CellArgs.Value is Boolean)
          {
            if ((bool)(CellArgs.Value))
              txt = "[x]";
            else
              txt = "[&nbsp]";
          }
          else
          {
            txt = CellArgs.Value.ToString().Trim();
            if (String.IsNullOrEmpty(txt))
              txt = "&nbsp";
            else
            {
              txt = MakeHtmlSpc(txt);
              if (CellArgs.OriginalValue != null)
              {
                if (CellArgs.OriginalValue is String)
                  sFormat = " STYLE=\"vnd.ms-excel.numberformat:@\"";
                if (DataTools.IsIntegerType(CellArgs.OriginalValue.GetType()) ||
                  DataTools.IsFloatType(CellArgs.OriginalValue.GetType()))
                  sNum = " x:num=\"" + Convert.ToString(CellArgs.OriginalValue, DataTools.DotNumberConv) + "\"";
              }
            }
          }
        }
        string sHAlign;
        string sStyle = String.Empty;
        switch (CellArgs.CellStyle.Alignment)
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
        switch (CellArgs.CellStyle.Alignment)
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

        if (CellArgs.IndentLevel > 0)
        {
          if (CellArgs.CellStyle.Alignment == DataGridViewContentAlignment.TopRight ||
            CellArgs.CellStyle.Alignment == DataGridViewContentAlignment.MiddleRight ||
            CellArgs.CellStyle.Alignment == DataGridViewContentAlignment.BottomRight)
          {
            //sStyle += "padding-left:" + (12 * CellArgs.IndentLevel).ToString() + "px;";
            sStyle += "padding-right:" + (12 * CellArgs.IndentLevel).ToString() + "px;"; // 27.12.2020. Хотя вряд ли выравнивание по правому краю с отступом встретится
          }
          else
          {
            sStyle += "padding-left:" + (12 * CellArgs.IndentLevel).ToString() + "px;";
            sStyle += "mso-char-indent-count:" + CellArgs.IndentLevel.ToString() + ";";
          }
        }
        if (!String.IsNullOrEmpty(sStyle))
          sStyle = " style='" + sStyle + "'";

        Color BackColor, ForeColor;
        EFPDataGridView.SetCellAttr(CellArgs.ColorType, CellArgs.Grayed, CellArgs.ContentVisible, out BackColor, out ForeColor);
        string cBkColor;
        if (BackColor.IsEmpty)
          cBkColor = String.Empty;
        else
          cBkColor = " BGCOLOR=\"" + GetHtmlColor(BackColor) + "\"";

        string cFont1, cFont2;
        if (ForeColor.IsEmpty)
        {
          cFont1 = String.Empty;
          cFont2 = String.Empty;
        }
        else
        {
          cFont1 = "<FONT COLOR=\"" + GetHtmlColor(ForeColor) + "\">";
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
