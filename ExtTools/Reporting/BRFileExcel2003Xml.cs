using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Shell;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Создание файла в формате XML для Excel-2003
  /// </summary>
  public class BRFileExcel2003Xml : BRFileCreator
  {
    #region Управляющие свойства

    /// <summary>
    /// 
    /// </summary>
    public bool DebugXml;

    #endregion

    #region Создание файла

    // 11.10.2023
    // В формате Excel-2003 символ переноса на новую строку задается в тексте атрибута <Data> как LF ("\r").
    // Символ должен быть задан явно, а не просто вставлен, например:
    //    <Cell ss:StyleID="s63"><Data ss:Type="String">AAA&#10;BBB&#10;CCC</Data></Cell>
    //
    // Для этого есть свойство XmlWriterSettings.NewLineHandling, которое нужно установить в NewLineHandling.Entitize.
    // К сожалению, свойство работает только с символом CR ("\n"). Не знаю, как это победить.
    // 
    // Поэтому, XML-документ создается в памяти. Для новой строки используется "&#xD;".
    // Затем выполняется замена.
    // Потом файл записывается.
    // Фу!

    /// <summary>
    /// Создает xml-файл
    /// </summary>
    /// <param name="report"></param>
    /// <param name="filePath"></param>
    protected override void DoCreateFile(BRReport report, AbsPath filePath)
    {
      XmlDocument xmlDoc = CreateXml(report);

      XmlWriterSettings xmlSettings = new XmlWriterSettings();
      xmlSettings.NewLineHandling = NewLineHandling.Entitize;
      if (DebugXml)
        xmlSettings.Indent = true;
      StringBuilder sb = new StringBuilder();
      using (XmlWriter wrt = XmlWriter.Create(sb, xmlSettings))
      {
        xmlDoc.Save(wrt);
        wrt.Close();
      }

      sb.Replace("&#xD;", "&#10;"); // в 16-ричном формате
      sb.Replace("&#13;", "&#10;"); // в 10-тичном формате - на всякий случай
      System.IO.File.WriteAllText(filePath.Path, sb.ToString(), Encoding.UTF8);
    }

    const string nmspcX = "urn:schemas-microsoft-com:office:excel";
    const string nmspcSS = "urn:schemas-microsoft-com:office:spreadsheet";
    const string nmspcO = "urn:schemas-microsoft-com:office:office";

    private XmlDocument CreateXml(BRReport report)
    {
      #region Создание XmlDocument и корневого узла

      XmlDocument xmlDoc = new XmlDocument();

      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", /*DebugXml ? "Windows-1251" : */"UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlProcessingInstruction xmlPI = xmlDoc.CreateProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
      xmlDoc.AppendChild(xmlPI);


      XmlElement elWholeDoc = xmlDoc.CreateElement("Workbook", nmspcSS);
      xmlDoc.AppendChild(elWholeDoc);
      SetAttr(elWholeDoc, "xmlns", nmspcSS, null);  // без namespace
      SetAttr(elWholeDoc, "xmlns:o", nmspcO, null);
      SetAttr(elWholeDoc, "xmlns:x", nmspcX, null);
      SetAttr(elWholeDoc, "xmlns:ss", nmspcSS, null);

      #endregion

      #region Сводка документа

      XmlElement elDocumentProperties = xmlDoc.CreateElement("DocumentProperties", nmspcO);
      elWholeDoc.AppendChild(elDocumentProperties);

      BRDocumentProperties docProps = report.DocumentProperties;

      if (!String.IsNullOrEmpty(docProps.Title))
      {
        XmlElement elTitle = xmlDoc.CreateElement("Title", nmspcO);
        elDocumentProperties.AppendChild(elTitle);
        elTitle.AppendChild(xmlDoc.CreateTextNode(docProps.Title));
      }
      if (!String.IsNullOrEmpty(docProps.Subject))
      {
        XmlElement elSubject = xmlDoc.CreateElement("Subject", nmspcO);
        elDocumentProperties.AppendChild(elSubject);
        elSubject.AppendChild(xmlDoc.CreateTextNode(docProps.Subject));
      }
      if (!String.IsNullOrEmpty(docProps.Author))
      {
        XmlElement elAuthor = xmlDoc.CreateElement("Author", nmspcO);
        elDocumentProperties.AppendChild(elAuthor);
        elAuthor.AppendChild(xmlDoc.CreateTextNode(docProps.Author));
      }
      if (!String.IsNullOrEmpty(docProps.Company))
      {
        XmlElement elCompany = xmlDoc.CreateElement("Company", nmspcO);
        elDocumentProperties.AppendChild(elCompany);
        elCompany.AppendChild(xmlDoc.CreateTextNode(docProps.Company));
      }

      #endregion

      // Таблица стилей нужна обязательно. 
      XmlElement elStyles = xmlDoc.CreateElement("Styles", nmspcSS);
      elWholeDoc.AppendChild(elStyles);

      XmlElement elStyleDef = xmlDoc.CreateElement("Style", nmspcSS);
      elStyles.AppendChild(elStyleDef);
      SetAttr(elStyleDef, "ss:ID", "Default", nmspcSS);
      SetAttr(elStyleDef, "ss:Name", "Normal", nmspcSS);

      XmlElement elFontDef = xmlDoc.CreateElement("Font", nmspcSS);
      elStyleDef.AppendChild(elFontDef);
      SetAttr(elFontDef, "ss:FontName", report.DefaultCellStyle.FontName, nmspcSS);
      SetAttr(elFontDef, "ss:Size", report.DefaultCellStyle.FontHeightPt.ToString(StdConvert.NumberFormat), nmspcSS);

      StringBuilder sb = new StringBuilder();
      Dictionary<string, int> cellStyleDict = new Dictionary<string, int>(); // ключ-описание стиля, значение N для стиля с именем "sN"

      for (int i = 0; i < report.Sections.Count; i++)
      {
        BRSection section = report.Sections[i];

        XmlElement elWorksheet = xmlDoc.CreateElement("Worksheet", nmspcSS);
        elWholeDoc.AppendChild(elWorksheet);
        SetAttr(elWorksheet, "ss:Name", section.Name, nmspcSS);

        BRColumnBoundList colBounds = new BRColumnBoundList(section, 10, 1);

        // На листе одна таблица
        XmlElement elTable = xmlDoc.CreateElement("Table", nmspcSS);
        elWorksheet.AppendChild(elTable);

        #region Параметры страницы

        XmlElement elWorksheetOptions = xmlDoc.CreateElement("WorksheetOptions", nmspcX);
        elWorksheet.AppendChild(elWorksheetOptions);

        XmlElement elPageSetup = xmlDoc.CreateElement("PageSetup", nmspcX);
        elWorksheetOptions.AppendChild(elPageSetup);

        XmlElement elLayout = xmlDoc.CreateElement("Layout", nmspcX);
        elPageSetup.AppendChild(elLayout);
        SetAttr(elLayout, "x:Orientation", section.PageSetup.Orientation == BROrientation.Landscape ? "Landscape" : "Portrait", nmspcX);
        if (section.PageSetup.CenterHorizontal)
          SetAttr(elLayout, "x:CenterHorizontal", "1", nmspcX);
        if (section.PageSetup.CenterVertical)
          SetAttr(elLayout, "x:CenterVertical", "1", nmspcX);

        XmlElement elPageMargins = xmlDoc.CreateElement("PageMargins", nmspcX);
        elPageSetup.AppendChild(elPageMargins);
        SetAttr(elPageMargins, "x:Left", (section.PageSetup.LeftMargin / 254.0).ToString("", StdConvert.NumberFormat), nmspcX);
        SetAttr(elPageMargins, "x:Top", (section.PageSetup.TopMargin / 254.0).ToString("", StdConvert.NumberFormat), nmspcX);
        SetAttr(elPageMargins, "x:Right", (section.PageSetup.RightMargin / 254.0).ToString("", StdConvert.NumberFormat), nmspcX);
        SetAttr(elPageMargins, "x:Bottom", (section.PageSetup.BottomMargin / 254.0).ToString("", StdConvert.NumberFormat), nmspcX);

        OLE.Excel.XlPaperSize paperSize = BRFileExcelOLE.GetPageSize(section.PageSetup);
        XmlElement elPrint = xmlDoc.CreateElement("Print", nmspcX);
        elWorksheetOptions.AppendChild(elPrint);
        XmlElement elPaperSizeIndex = xmlDoc.CreateElement("PaperSizeIndex", nmspcX);
        elPrint.AppendChild(elPaperSizeIndex);
        XmlText txtPaperSizeIndex = xmlDoc.CreateTextNode(StdConvert.ToString((int)paperSize));
        elPaperSizeIndex.AppendChild(txtPaperSizeIndex);

        #endregion

        #region Размеры столбцов

        for (int l = 0; l < colBounds.WidthArray.Length; l++)
        {
          XmlElement elColumn = xmlDoc.CreateElement("Column", nmspcSS);
          elTable.AppendChild(elColumn);
          SetAttr(elColumn, "ss:Width", StdConvert.ToString((int)(colBounds.WidthArray[l] / 254.0 * 72.0)), nmspcSS);
        }

        #endregion

        #region Перебор полос

        for (int j = 0; j < section.Bands.Count; j++)
        {
          #region Зазор между полосами

          int gap = 0;
          if (j > 0)
            gap = Math.Max(section.Bands[j - 1].BottomMargin, section.Bands[j].TopMargin);
          if (gap >= 10)
          {
            XmlElement elGapRow = xmlDoc.CreateElement("Row", nmspcSS);
            elTable.AppendChild(elGapRow);
            SetAttr(elGapRow, "ss:AutoFitHeight", "0", nmspcSS);
            SetAttr(elGapRow, "ss:Height", StdConvert.ToString((int)(gap / 254.0 * 72.0)), nmspcSS);
          }

          #endregion

          BRBand band = section.Bands[j];

          int[] leftCols = colBounds.LeftColumns[j];
          int[] rightCols = colBounds.RightColumns[j];

          BRSelector sel = band.CreateSelector();
          for (int k = 0; k < band.RowCount; k++)
          {
            sel.RowIndex = k;
            XmlElement elRow = xmlDoc.CreateElement("Row", nmspcSS);
            elTable.AppendChild(elRow);
            if (sel.RowInfo.Height != BRReport.AutoRowHeight)
            {
              SetAttr(elRow, "ss:AutoFitHeight", "0", nmspcSS);
              SetAttr(elRow, "ss:Height", StdConvert.ToString((int)(sel.RowInfo.Height / 254.0 * 72.0)), nmspcSS);
            }

            for (int l = 0; l < band.ColumnCount; l++)
            {
              sel.ColumnIndex = l;

              BRRange merge = sel.MergeInfo;

              if (merge.FirstRowIndex == sel.RowIndex && merge.FirstColumnIndex == sel.ColumnIndex) // первая ячейка объединения или обычная ячейка
              {
                XmlElement elCell = xmlDoc.CreateElement("Cell", nmspcSS);
                elRow.AppendChild(elCell);

                SetAttr(elCell, "ss:Index", StdConvert.ToString(leftCols[l]), nmspcSS);

                #region Значение

                string valueText, typeText, formatText;
                GetCellValue(sel, out valueText, out typeText, out formatText);

                bool hasNewLine = false;
                if (typeText != null)
                {
                  XmlElement elData = xmlDoc.CreateElement("Data", nmspcSS);
                  elCell.AppendChild(elData);
                  //BRFileTools.AppendText(elData, valueText);
                  XmlText textNode = xmlDoc.CreateTextNode(valueText);
                  elData.AppendChild(textNode);

                  SetAttr(elData, "ss:Type", typeText, nmspcSS);
                  if (valueText.IndexOf('\r') > 0)
                    hasNewLine = true;
                }

                #region Форматирование

                BRFileTools.InitCellStyleKey(sb, sel, 0);
                if (hasNewLine)
                  sb.Append("|CR|");

                int styleKeyNum;
                if (!cellStyleDict.TryGetValue(sb.ToString(), out styleKeyNum))
                {
                  #region Добавление стиля

                  styleKeyNum = cellStyleDict.Count + 1;
                  cellStyleDict.Add(sb.ToString(), styleKeyNum);

                  XmlElement elStyle = xmlDoc.CreateElement("Style", nmspcSS);
                  elStyles.AppendChild(elStyle);
                  SetAttr(elStyle, "ss:ID", "s" + StdConvert.ToString(styleKeyNum), nmspcSS);

                  string ha, va;
                  switch (sel.ActualHAlign)
                  {
                    case BRHAlign.Left: ha = "Left"; break;
                    case BRHAlign.Center: ha = "Center"; break;
                    case BRHAlign.Right: ha = "Right"; break;
                    default: throw new BugException("HAlign");
                  }

                  switch (sel.CellStyle.VAlign)
                  {
                    case BRVAlign.Top: va = "Top"; break;
                    case BRVAlign.Center: va = "Center"; break;
                    case BRVAlign.Bottom: va = "Bottom"; break;
                    default: throw new BugException("VAlign");
                  }

                  XmlElement elAlign = xmlDoc.CreateElement("Alignment", nmspcSS);
                  elStyle.AppendChild(elAlign);
                  SetAttr(elAlign, "ss:Horizontal", ha, nmspcSS);
                  SetAttr(elAlign, "ss:Vertical", va, nmspcSS);
                  if (sel.CellStyle.WrapMode != BRWrapMode.NoWrap || hasNewLine)
                    SetAttr(elAlign, "ss:WrapText", "1", nmspcSS);
                  if (sel.CellStyle.IndentLevel > 0)
                    SetAttr(elAlign, "ss:Indent", StdConvert.ToString(sel.CellStyle.IndentLevel), nmspcSS);

                  if (sel.CellStyle.HasBorders || sel.CellStyle.DiagonalUp != BRLine.None || sel.CellStyle.DiagonalDown != BRLine.None)
                  {
                    XmlElement elBorders = xmlDoc.CreateElement("Borders", nmspcSS);
                    elStyle.AppendChild(elBorders);

                    AddBorder(elBorders, "Bottom", sel.CellStyle.BottomBorder);
                    AddBorder(elBorders, "Left", sel.CellStyle.LeftBorder);
                    AddBorder(elBorders, "Right", sel.CellStyle.RightBorder);
                    AddBorder(elBorders, "Top", sel.CellStyle.TopBorder);
                    AddBorder(elBorders, "DiagonalRight", sel.CellStyle.DiagonalUp);
                    AddBorder(elBorders, "DiagonalLeft", sel.CellStyle.DiagonalDown);
                  }

                  XmlElement elFont = xmlDoc.CreateElement("Font", nmspcSS);
                  elStyle.AppendChild(elFont);
                  //if (sel.CellStyle.FontName != report.DefaultCellStyle.FontName)
                    SetAttr(elFont, "ss:FontName", sel.CellStyle.FontName, nmspcSS);
                  //if (sel.CellStyle.FontHeightPt!=report.DefaultCellStyle.FontHeightPt)
                    SetAttr(elFont, "ss:Size", sel.CellStyle.FontHeightPt.ToString(StdConvert.NumberFormat), nmspcSS);

                  if (sel.CellStyle.ForeColor != BRColor.Auto)
                    SetAttr(elFont, "ss:Color", MyColorStr(sel.CellStyle.ForeColor), nmspcSS);

                  if (sel.CellStyle.Bold)
                    SetAttr(elFont, "ss:Bold", "1", nmspcSS);
                  if (sel.CellStyle.Italic)
                    SetAttr(elFont, "ss:Italic", "1", nmspcSS);
                  if (sel.CellStyle.Underline)
                    SetAttr(elFont, "ss:Underline", "Single", nmspcSS);
                  if (sel.CellStyle.Strikeout)
                    SetAttr(elFont, "ss:StrikeThrough", "1", nmspcSS);

                  if (sel.CellStyle.BackColor != BRColor.Auto)
                  {
                    XmlElement elInterior = xmlDoc.CreateElement("Interior", nmspcSS);
                    elStyle.AppendChild(elInterior);
                    SetAttr(elInterior, "ss:Color", MyColorStr(sel.CellStyle.BackColor), nmspcSS);
                    SetAttr(elInterior, "ss:Pattern", "Solid", nmspcSS);
                  }

                  if (!String.IsNullOrEmpty(formatText))
                  {
                    XmlElement elNumberFormat = xmlDoc.CreateElement("NumberFormat", nmspcSS);
                    elStyle.AppendChild(elNumberFormat);
                    SetAttr(elNumberFormat, "ss:Format", formatText, nmspcSS);
                  }
                  #endregion
                }

                SetAttr(elCell, "ss:StyleID", "s" + StdConvert.ToString(styleKeyNum), nmspcSS);

                #endregion


                #endregion

                #region Объединение ячеек

                int xlsColumn1 = leftCols[merge.FirstColumnIndex];
                int xlsColumn2 = rightCols[merge.LastColumnIndex];

                if (merge.RowCount > 1)
                  SetAttr(elCell, "ss:MergeDown", StdConvert.ToString(merge.RowCount - 1), nmspcSS);
                if (xlsColumn2 > xlsColumn1)
                  SetAttr(elCell, "ss:MergeAcross", StdConvert.ToString(xlsColumn2 - xlsColumn1), nmspcSS);

                #endregion
              }
            } // цикл по столбцам
          } // цикл по строкам
        } // цикл по полосам

        #endregion
      } // цикл по листам

      return xmlDoc;
    }

    #endregion

    #region Извлечение значения

    private void GetCellValue(BRSelector sel, out string valueText, out string typeText, out string formatText)
    {
      valueText = null;
      typeText = null;
      formatText = null;

      GetCellValue2(sel, ref valueText, ref typeText, ref formatText);

      if (!String.IsNullOrEmpty(sel.CellStyle.Format))
        formatText = sel.CellStyle.Format;
    }

    private static void GetCellValue2(BRSelector sel, ref string valueText, ref string typeText, ref string formatText)
    {
      if (sel.Value == null)
        return;

      switch (sel.Value.GetType().Name)
      {
        case "String":
        case "Guid":
          if (sel.Value.ToString().Trim().Length == 0)
            return;
          valueText = sel.Value.ToString();
          if (Environment.NewLine != "\r")
            valueText = valueText.Replace(Environment.NewLine, "\r");
          //valueText = DataTools.ReplaceAny(valueText, BRFileTools.BadValueChars, ' ');
          valueText = valueText.Replace(DataTools.SoftHyphenStr, String.Empty);
          typeText = "String";
          break;
        case "Boolean":
          valueText = (bool)(sel.Value) ? "1" : "0";
          typeText = "Boolean";
          break;
        case "DateTime":
          // Предотвращаем ошибку открытия файла из-за неправильной даты
          if ((DateTime)(sel.Value) < MicrosoftOfficeTools.MinExcelDate || ((DateTime)(sel.Value)).Date > MicrosoftOfficeTools.MaxExcelDate)
          {
            valueText = "*** Дата, выходящая за допустимые пределы Excel: " + (sel.Value).ToString() + " ***";
            typeText = "String";
            return;
          }

          valueText = ((DateTime)(sel.Value)).ToString("s", StdConvert.DateTimeFormat);
          typeText = "DateTime";

          // Если формат данных не задан, используем формат по умолчанию
          // Если оставить формат "Общий", то даты будут отображаться в Excel как числа
          if (String.IsNullOrEmpty(sel.CellStyle.Format))
          {
            if (((DateTime)(sel.Value)).TimeOfDay.Ticks != 0L)
              formatText = MicrosoftOfficeTools.DefaultShortDateTimeFormat;
            else
              formatText = MicrosoftOfficeTools.DefaultShortDateFormat;
          }
          break;
        default:
          if (DataTools.IsNumericType(sel.Value.GetType()))
          {
            valueText = Convert.ToString(sel.Value, StdConvert.NumberFormat);
            typeText = "Number";
          }
          break;
      }
    }

    #endregion

    #region Форматирование

    private static void AddBorder(XmlElement elBorders, string pos, BRLine border)
    {
      if (border == BRLine.None)
        return;

      XmlElement elBorder = elBorders.OwnerDocument.CreateElement("Border", nmspcSS);
      elBorders.AppendChild(elBorder);

      SetAttr(elBorder, "ss:Position", pos, nmspcSS);

      string line;
      switch (border.Style)
      {
        case BRLineStyle.Thin:
        case BRLineStyle.Medium:
        case BRLineStyle.Thick: line = "Continuous"; break;
        case BRLineStyle.Dot: line = "Dot"; break;
        case BRLineStyle.Dash: line = "Dash"; break;
        case BRLineStyle.DashDot: line = "DashDot"; break;
        case BRLineStyle.DashDotDot: line = "DashDotDot"; break;
        default:
          throw new BugException("BRLine.Style");
      }

      double w = BRLine.GetLineWidthPt(border.Style);

      SetAttr(elBorder, "ss:LineStyle", line, nmspcSS);
      SetAttr(elBorder, "ss:Weight", w.ToString("0.0", StdConvert.NumberFormat), nmspcSS);
    }

    private static string MyColorStr(BRColor c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    #endregion

    #region Вспомогательные методы

    private static void SetAttr(XmlElement el, string name, string value, string nmspc)
    {
      XmlAttribute attr;
      if (String.IsNullOrEmpty(nmspc))
        attr = el.OwnerDocument.CreateAttribute(name);
      else
        attr = el.OwnerDocument.CreateAttribute(name, nmspc);
      attr.Value = value;
      el.Attributes.Append(attr);
    }

    #endregion
  }
}
