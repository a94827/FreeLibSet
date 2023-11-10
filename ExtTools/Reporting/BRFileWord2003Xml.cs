using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Shell;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Создание файла в формате XML для Word-2003
  /// </summary>
  public class BRFileWord2003Xml
  {
    #region Константы

    private int MinFontWidthPercent = 1;
    private int MaxFontWidthPercent = 600;

    #endregion

    #region Конструктор

    /// <summary>
    /// 
    /// </summary>
    /// <param name="measurer"></param>
    public BRFileWord2003Xml(IBRMeasurer measurer)
    {
      if (measurer == null)
        throw new ArgumentNullException("measurer");
      _Measurer = measurer;
    }

    private IBRMeasurer _Measurer;

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// 
    /// </summary>
    public bool DebugXml;

    #endregion

    #region Создание файла

    /// <summary>
    /// Создает xml-файл
    /// </summary>
    /// <param name="report"></param>
    /// <param name="filePath"></param>
    public void CreateFile(BRReport report, AbsPath filePath)
    {
      DebugXml = true;

      XmlDocument xmlDoc = CreateXml(report);
      //XmlDoc.Save(FileName);

      XmlWriterSettings xmlSettings = new XmlWriterSettings();
      if (DebugXml)
      {
        //xmlSettings.Encoding = Encoding.GetEncoding(1251);
        xmlSettings.Indent = true;
      }
      XmlWriter wrt = XmlWriter.Create(filePath.Path, xmlSettings);
      try
      {
        xmlDoc.WriteTo(wrt);
      }
      finally
      {
        wrt.Close();
      }
    }

    const string nmspcW = "http://schemas.microsoft.com/office/word/2003/wordml";
    const string nmspcWx = "http://schemas.microsoft.com/office/word/2003/auxHint";
    const string nmspcO = "urn:schemas-microsoft-com:office:office";

    private XmlDocument CreateXml(BRReport report)
    {
      #region Создание XmlDocument и корневого узла

      XmlDocument xmlDoc = new XmlDocument();

      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", /*DebugXml ? "Windows-1251" : */"UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlProcessingInstruction xmlPI = xmlDoc.CreateProcessingInstruction("mso-application", "progid=\"Word.Document\"");
      xmlDoc.AppendChild(xmlPI);


      XmlElement elWholeDoc = xmlDoc.CreateElement("w:wordDocument", nmspcW);
      xmlDoc.AppendChild(elWholeDoc);
      SetAttr(elWholeDoc, "xmlns:w", nmspcW, null);  // без namespace
      SetAttr(elWholeDoc, "xmlns:wx", nmspcWx, null);
      SetAttr(elWholeDoc, "xmlns:o", nmspcO, null);
      SetAttr(elWholeDoc, "xml:space", "preserve", null); // без этого, все <w:tab> в тексте будут проигнорированы

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

      #region Таблица шрифтов

      XmlElement elFonts = xmlDoc.CreateElement("w:fonts", nmspcW);
      elWholeDoc.AppendChild(elFonts);

      XmlElement elDefaultFonts = xmlDoc.CreateElement("w:defaultFonts", nmspcW);
      elFonts.AppendChild(elDefaultFonts);
      SetAttr(elDefaultFonts, "w:ascii", report.DefaultCellStyle.FontName, nmspcW);
      SetAttr(elDefaultFonts, "w:fareast", report.DefaultCellStyle.FontName, nmspcW);
      SetAttr(elDefaultFonts, "w:h-ansi", report.DefaultCellStyle.FontName, nmspcW);
      SetAttr(elDefaultFonts, "w:cs", report.DefaultCellStyle.FontName, nmspcW);
      string[] fontNames = report.GetFontNames();
      for (int i = 0; i < fontNames.Length; i++)
      {
        XmlElement elFont = xmlDoc.CreateElement("w:font", nmspcW);
        elFonts.AppendChild(elFont);
        SetAttr(elFont, "w:name", fontNames[i], nmspcW);
      }

      #endregion

      // Таблица стилей
      // Задаем только стиль "Normal". Остальное форматирование стили не использует
      XmlElement elStyles = xmlDoc.CreateElement("w:styles", nmspcW);
      elWholeDoc.AppendChild(elStyles);

      InitDefaultStyle(elStyles, report.DefaultCellStyle);

      XmlElement elBody = xmlDoc.CreateElement("w:body", nmspcW);
      elWholeDoc.AppendChild(elBody);


      for (int i = 0; i < report.Sections.Count; i++)
      {
        BRSection section = report.Sections[i];

        XmlElement elSect = xmlDoc.CreateElement("wx:sect", nmspcWx);
        elBody.AppendChild(elSect);

        #region Перебор полос

        bool prevBandIsSimple = true;

        for (int j = 0; j < section.Bands.Count; j++)
        {
          #region Зазор между полосами

          int gap = 0;
          if (j > 0)
            gap = Math.Max(section.Bands[j - 1].BottomMargin, section.Bands[j].TopMargin);
          if (gap >= 10)
          {
            XmlElement elP = xmlDoc.CreateElement("w:p", nmspcW);
            elSect.AppendChild(elP);
            XmlElement elR = elP.OwnerDocument.CreateElement("w:r", nmspcW);
            elP.AppendChild(elR);

            XmlElement elRPr = xmlDoc.CreateElement("w:rPr", nmspcW);
            elR.AppendChild(elRPr);

            XmlElement elSz = xmlDoc.CreateElement("w:sz", nmspcW);
            elRPr.AppendChild(elSz);
            int gapPt2 = (int)(gap * 72.0 / 254.0 * 2);
            SetAttr(elSz, "w:val", StdConvert.ToString(gapPt2), nmspcW); // в полупунктах

            XmlElement elT = elR.OwnerDocument.CreateElement("w:t", nmspcW);
            elR.AppendChild(elT);
            XmlText dummyText = xmlDoc.CreateTextNode(DataTools.NonBreakSpaceStr); // какой-нибудь текст обязательно нужен, иначе размер строки ни на что не повлияет. Обычный пробел не считается.
            elT.AppendChild(dummyText);

            prevBandIsSimple = true;
          }

          #endregion

          BRBand band = section.Bands[j];
          BRSelector sel = band.CreateSelector();
          BRStripeItem[][] aaCols = BRPaginator.PaginateBandColumns(band);

          for (int m = 0; m < aaCols.Length; m++)
          {
            #region Зазор между вертикальными полосами 

            if (m > 0)
            {
              XmlElement elP = xmlDoc.CreateElement("w:p", nmspcW);
              elSect.AppendChild(elP);
              XmlElement elR = elP.OwnerDocument.CreateElement("w:r", nmspcW);
              elP.AppendChild(elR);
              //XmlElement elT = elR.OwnerDocument.CreateElement("w:t", nmspcW);
              //elR.AppendChild(elT);

              XmlElement elBr = xmlDoc.CreateElement("w:br", nmspcW);
              elR.AppendChild(elBr);
              SetAttr(elBr, "w:type", "page", nmspcW);
              prevBandIsSimple = true; // можно будет использовать строки-заголовки
            }

            #endregion

            BRStripeItem[] stripeCols = aaCols[m];

            bool thisBandIsSimple = IsSimpleBand(sel);
            if (thisBandIsSimple)
            {
              bool bandKeepWithNext = sel.Band.KeepWithNext || GetBandKeepWithPrev(section, j + 1);

              XmlElement elP = xmlDoc.CreateElement("w:p", nmspcW);
              WriteTextValue(sel, elSect, ref elP, section.PageSetup.PrintAreaWidth, true, bandKeepWithNext);
              elSect.AppendChild(elP);
            }
            else
            {
              XmlElement elTbl = xmlDoc.CreateElement("w:tbl", nmspcW);
              elSect.AppendChild(elTbl);

              XmlElement elTblPr = xmlDoc.CreateElement("w:tblPr", nmspcW);
              elTbl.AppendChild(elTblPr);

              XmlElement elTblW = xmlDoc.CreateElement("w:tblW", nmspcW);
              elTblPr.AppendChild(elTblW);
              SetAttr(elTblW, "w:w", "0", nmspcW);
              SetAttr(elTblW, "w:type", "auto", nmspcW);

              XmlElement elTblLayout = xmlDoc.CreateElement("w:tblLayout", nmspcW);
              elTblPr.AppendChild(elTblLayout);
              SetAttr(elTblLayout, "w:type", "Fixed", nmspcW);// с заглавной буквы

              // Отсупы по умолчанию
              WriteMargins(elTblPr, true, report.DefaultCellStyle.LeftMargin, report.DefaultCellStyle.TopMargin, report.DefaultCellStyle.RightMargin, report.DefaultCellStyle.BottomMargin);

              XmlElement elTblGrid = xmlDoc.CreateElement("w:tblGrid", nmspcW);
              elTbl.AppendChild(elTblGrid);

              for (int l = 0; l < stripeCols.Length; l++)
              {
                XmlElement elGridCol = xmlDoc.CreateElement("w:gridCol", nmspcW);
                elTblGrid.AppendChild(elGridCol);
                SetAttr(elGridCol, "w:w", GetTwips01Attr(stripeCols[l].Size), nmspcW);
              }

              // Строки

              bool isRepeatableRow = prevBandIsSimple;
              for (int k = 0; k < band.RowCount; k++)
              {
                sel.RowIndex = k;

                XmlElement elTr = xmlDoc.CreateElement("w:tr", nmspcW);
                elTbl.AppendChild(elTr);

                XmlElement elTrPr = xmlDoc.CreateElement("w:trPr", nmspcW);
                elTr.AppendChild(elTrPr);

                XmlElement elCantSplit = xmlDoc.CreateElement("w:cantSplit", nmspcW);
                elTrPr.AppendChild(elCantSplit);


                if (sel.RowInfo.Height != BRReport.AutoRowHeight)
                {
                  XmlElement elTrHeight = xmlDoc.CreateElement("w:trHeight", nmspcW);
                  elTrPr.AppendChild(elTrHeight);
                  SetAttr(elTrHeight, "w:h-rule", "exact", nmspcW);
                  SetAttr(elTrHeight, "w:val", GetTwips01Attr(sel.RowInfo.Height), nmspcW);
                }

                if (isRepeatableRow)
                {
                  if (sel.RowInfo.Repeatable)
                  {
                    XmlElement elTblHeader = xmlDoc.CreateElement("w:tblHeader", nmspcW);
                    elTrPr.AppendChild(elTblHeader);
                  }
                  else
                    isRepeatableRow = false; // нельзя делать повторяющиеся строки, если они не вверху таблицы
                }

                bool rowKeepWithNext = sel.RowInfo.KeepWithNext || GetNextRowKeepWithPrev(sel);

                // Столбцы
                for (int l = 0; l < stripeCols.Length; l++)
                {
                  sel.ColumnIndex = stripeCols[l].Index;

                  BRRange merge = sel.MergeInfo;
                  XmlElement elTc = xmlDoc.CreateElement("w:tc", nmspcW);
                  elTr.AppendChild(elTc);

                  XmlElement elTcPr = xmlDoc.CreateElement("w:tcPr", nmspcW);
                  elTc.AppendChild(elTcPr);

                  XmlElement elTcW = xmlDoc.CreateElement("w:tcW", nmspcW);
                  elTcPr.AppendChild(elTcW);
                  SetAttr(elTcW, "w:w", GetTwips01Attr(stripeCols[l].Size), nmspcW);
                  SetAttr(elTcW, "w:type", "dxa", nmspcW);

                  XmlElement elP = xmlDoc.CreateElement("w:p", nmspcW);// Добавляем потом


                  if (merge.FirstRowIndex == sel.RowIndex && merge.FirstColumnIndex == sel.ColumnIndex) // первая ячейка объединения или обычная ячейка
                  {
                    // Объединение ячеек
                    if (merge.ColumnCount > 1)
                    {
                      XmlElement elHmerge = xmlDoc.CreateElement("w:hmerge", nmspcW);
                      elTcPr.AppendChild(elHmerge);
                      SetAttr(elHmerge, "w:val", "restart", nmspcW);
                    }
                    if (merge.RowCount > 1)
                    {
                      XmlElement elVmerge = xmlDoc.CreateElement("w:vmerge", nmspcW);
                      elTcPr.AppendChild(elVmerge);
                      SetAttr(elVmerge, "w:val", "restart", nmspcW);
                    }

                    #region Значение

                    WriteTextValue(sel, elTc, ref elP, GetColumnWidth(stripeCols, l, merge.ColumnCount), false, rowKeepWithNext);

                    #endregion

                    #region Форматирование

                    #region Вертикальное выравнивание

                    if (sel.CellStyle.VAlign != BRVAlign.Top)
                    {
                      XmlElement elVAlign = xmlDoc.CreateElement("w:vAlign", nmspcW);
                      elTcPr.AppendChild(elVAlign);
                      switch (sel.CellStyle.VAlign)
                      {
                        case BRVAlign.Center:
                          SetAttr(elVAlign, "w:val", "center", nmspcW);
                          break;
                        case BRVAlign.Bottom:
                          SetAttr(elVAlign, "w:val", "bottom", nmspcW);
                          break;
                      }
                    }

                    #endregion

                    #region Отступы

                    if (BRFileTools.AreMarginsDifferent(sel.CellStyle, report.DefaultCellStyle))
                      WriteMargins(elTcPr, false, sel.CellStyle.LeftMargin, sel.CellStyle.TopMargin, sel.CellStyle.RightMargin, sel.CellStyle.BottomMargin);

                    #endregion

                    #endregion

                  }
                  else
                  {
                    // не первая ячейка
                    sel.RowIndex = merge.FirstRowIndex;
                    sel.ColumnIndex = merge.FirstColumnIndex;

                    if (merge.ColumnCount > 1)
                    {
                      XmlElement elHmerge = xmlDoc.CreateElement("w:hmerge", nmspcW);
                      elTcPr.AppendChild(elHmerge);
                    }
                    if (merge.RowCount > 1)
                    {
                      XmlElement elVmerge = xmlDoc.CreateElement("w:vmerge", nmspcW);
                      elTcPr.AppendChild(elVmerge);
                    }
                  }

                  #region Цвет фона

                  if (!sel.CellStyle.BackColor.IsAuto)
                  {
                    XmlElement elShd = xmlDoc.CreateElement("w:shd", nmspcW);
                    elTcPr.AppendChild(elShd);
                    SetAttr(elShd, "w:val", "clear", nmspcW);
                    SetAttr(elShd, "w:fill", GetColorHex(sel.CellStyle.BackColor), nmspcW);
                  }

                  #endregion

                  #region Рамки

                  if (sel.CellStyle.HasBorders || sel.CellStyle.DiagonalUp.Style != BRLineStyle.None || sel.CellStyle.DiagonalDown.Style != BRLineStyle.None)
                  {
                    XmlElement elTcBorders = xmlDoc.CreateElement("w:tcBorders", nmspcW);
                    elTcPr.AppendChild(elTcBorders);
                    if (merge.FirstRowIndex == k)
                      WriteBorder(elTcBorders, sel.CellStyle.TopBorder, "w:top");
                    if (merge.FirstColumnIndex == stripeCols[l].Index)
                      WriteBorder(elTcBorders, sel.CellStyle.LeftBorder, "w:left");
                    if (merge.LastRowIndex == k)
                      WriteBorder(elTcBorders, sel.CellStyle.BottomBorder, "w:bottom");
                    if (merge.LastColumnIndex == stripeCols[l].Index)
                      WriteBorder(elTcBorders, sel.CellStyle.RightBorder, "w:right");

                    WriteBorder(elTcBorders, sel.CellStyle.DiagonalUp, "w:tr2bl");
                    WriteBorder(elTcBorders, sel.CellStyle.DiagonalDown, "w:tl2br");
                  }

                  #endregion

                  sel.RowIndex = k; // восстановили


                  // Элемент "p" идет последним в tc
                  elTc.AppendChild(elP);

                } // цикл по столбцам
              } // цикл по строкам
            }

            prevBandIsSimple = thisBandIsSimple;
          } // цикл по вертикальным полосам
        } // цикл по таблицам

        #endregion

        #region Параметры страницы

        // Параметры страницы должны идти после заполнения данных

        // Для последнего раздела структура тегов: <wx:sect><w:sectPr>
        // Для предыдущих разделов создается параграф, а в нем уже свойства. То есть: <wx:sect><w:p><w:pPr><w:sectPr>
        XmlElement elSectPr = xmlDoc.CreateElement("w:sectPr", nmspcW);
        if (i == (report.Sections.Count - 1))
          elSect.AppendChild(elSectPr);
        else
        {
          XmlElement elP2 = xmlDoc.CreateElement("w:p", nmspcW);
          elSect.AppendChild(elP2);
          XmlElement elPPr = xmlDoc.CreateElement("w:pPr", nmspcW);
          elP2.AppendChild(elPPr);
          elPPr.AppendChild(elSectPr);
        }

        XmlElement elPgSz = xmlDoc.CreateElement("w:pgSz", nmspcW);
        elSectPr.AppendChild(elPgSz);
        SetAttr(elPgSz, "w:w", GetTwips01Attr(section.PageSetup.PaperWidth), nmspcW);
        SetAttr(elPgSz, "w:h", GetTwips01Attr(section.PageSetup.PaperHeight), nmspcW);
        SetAttr(elPgSz, "w:orient", section.PageSetup.Orientation == BROrientation.Landscape ? "landscape" : "portrait", nmspcW);

        XmlElement elPgMar = xmlDoc.CreateElement("w:pgMar", nmspcW);
        elSectPr.AppendChild(elPgMar);
        SetAttr(elPgMar, "w:top", GetTwips01Attr(section.PageSetup.TopMargin), nmspcW);
        SetAttr(elPgMar, "w:right", GetTwips01Attr(section.PageSetup.RightMargin), nmspcW);
        SetAttr(elPgMar, "w:bottom", GetTwips01Attr(section.PageSetup.BottomMargin), nmspcW);
        SetAttr(elPgMar, "w:left", GetTwips01Attr(section.PageSetup.LeftMargin), nmspcW);

        #endregion
      } // цикл по листам

      return xmlDoc;
    }

    private static bool GetBandKeepWithPrev(BRSection section, int bandIndex)
    {
      if (bandIndex >= section.Bands.Count)
        return false;
      //BRSelector sel = section.Bands[bandIndex].CreateSelector();
      //sel.RowIndex = 0;
      //return sel.RowInfo.KeepWithPrev;
      return section.Bands[bandIndex].KeepWithPrev;
    }

    private bool GetNextRowKeepWithPrev(BRSelector sel)
    {
      if (sel.RowIndex == (sel.Band.RowCount - 1))
        return false;

      sel.RowIndex++;
      bool res = sel.RowInfo.KeepWithPrev;
      sel.RowIndex--;
      return res;
    }


    private void InitDefaultStyle(XmlElement elStyles, BRCellStyle cellStyle)
    {
      XmlDocument xmlDoc = elStyles.OwnerDocument;

      XmlElement elStyle = xmlDoc.CreateElement("w:style", nmspcW);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "w:type", "paragraph", nmspcW);
      SetAttr(elStyle, "w:default", "on", nmspcW);
      SetAttr(elStyle, "w:styleId", "a", nmspcW); // имя для ссылки на стиль. Чем короче, тем лучше

      XmlElement elName = xmlDoc.CreateElement("w:name", nmspcW);
      elStyle.AppendChild(elName);
      SetAttr(elName, "w:val", "Normal", nmspcW); // имя для выбора из списка

      // Для совместимости с Word'ом
      XmlElement elUIName = xmlDoc.CreateElement("wx:uiName", nmspcWx);
      elStyle.AppendChild(elUIName);
      SetAttr(elUIName, "wx:val", "Обычный", nmspcWx); // имя для выбора из списка

      XmlElement elPPr = xmlDoc.CreateElement("w:pPr", nmspcW);
      elStyle.AppendChild(elPPr);

      XmlElement elSpacing = xmlDoc.CreateElement("w:spacing", nmspcW);
      elPPr.AppendChild(elSpacing);
      SetAttr(elSpacing, "w:before", "0", nmspcW);
      SetAttr(elSpacing, "w:after", "0", nmspcW);

      XmlElement elInd = xmlDoc.CreateElement("w:ind", nmspcW);
      elPPr.AppendChild(elInd);
      SetAttr(elInd, "w:left", "0", nmspcW);
      SetAttr(elInd, "w:right", "0", nmspcW);
      SetAttr(elInd, "w:first-line", "0", nmspcW); // нет красной строки

      XmlElement elRPr = xmlDoc.CreateElement("w:rPr", nmspcW);
      elStyle.AppendChild(elRPr);

      //XmlElement elRFonts = xmlDoc.CreateElement("w:rFonts", nmspcW);
      //elRPr.AppendChild(elRFonts);
      //SetAttr(elRFonts, "w:ascii", cellStyle.FontName, nmspcW);

      XmlElement elFont = xmlDoc.CreateElement("wx:font", nmspcWx);
      elRPr.AppendChild(elFont);
      SetAttr(elFont, "wx:val", cellStyle.FontName, nmspcWx);

      XmlElement elSz = xmlDoc.CreateElement("w:sz", nmspcW);
      elRPr.AppendChild(elSz);
      SetAttr(elSz, "w:val", StdConvert.ToString((int)(cellStyle.FontHeightTwip / 10)), nmspcW); // в полупунктах
    }


    private static int GetColumnWidth(BRStripeItem[] aStripeCols, int firstColumn, int columnCount)
    {
      int w = 0;
      for (int i = 0; i < columnCount; i++)
        w += aStripeCols[firstColumn + i].Size;
      return w;
    }

    private void WriteTextValue(BRSelector sel, XmlElement elFutureParent, ref XmlElement elP, int columnWidth, bool isSimpleBand, bool keepWithNext)
    {
      string s = sel.AsString;
      if (s.Length == 0)
        return;
      string[] a = s.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
      for (int i = 0; i < a.Length; i++)
      {
        WritePPr(elP, sel, columnWidth, isSimpleBand, keepWithNext);

        if (i > 0)
        {
          elFutureParent.AppendChild(elP);
          elP = elFutureParent.OwnerDocument.CreateElement("w:p", nmspcW);
          WritePPr(elP, sel, columnWidth, isSimpleBand, keepWithNext);
        }

        if (sel.CellStyle.TextLeader != BRTextLeader.None)
        {
          if (sel.ActualHAlign == BRHAlign.Center || sel.ActualHAlign == BRHAlign.Right)
          {
            XmlElement elR2 = elP.OwnerDocument.CreateElement("w:r", nmspcW);
            elP.AppendChild(elR2);
            WriteRPr(elR2, sel, columnWidth);
            XmlElement elTab1 = elR2.OwnerDocument.CreateElement("w:tab", nmspcW);
            elR2.AppendChild(elTab1);
          }
        }

        if (a[i].IndexOf(DataTools.SoftHyphenChar) >= 0)
        {
          // Обработка мягких переносов
          string[] aa = a[i].Split(DataTools.SoftHyphenChar);
          for (int j = 0; j < aa.Length; j++)
          {
            if (j > 0)
            {
              // Вставляем мягкий перенос
              XmlElement elR2 = elP.OwnerDocument.CreateElement("w:r", nmspcW);
              elP.AppendChild(elR2);
              WriteRPr(elR2, sel, columnWidth);

              XmlElement elSoftHyphen = elR2.OwnerDocument.CreateElement("w:softHyphen", nmspcW);
              elR2.AppendChild(elSoftHyphen);
            }

            // Основная часть строки
            DoWriteText(elP, aa[j],sel, columnWidth);
          }
        }
        else
        {
          DoWriteText(elP, a[i],sel, columnWidth);
        }
      }
    }

    private void DoWriteText(XmlElement elP, string s, BRSelector sel, int columnWidth)
    {
      XmlElement elR = elP.OwnerDocument.CreateElement("w:r", nmspcW);
      elP.AppendChild(elR);
      WriteRPr(elR, sel, columnWidth);
      XmlElement elT = elR.OwnerDocument.CreateElement("w:t", nmspcW);
      elR.AppendChild(elT);
      XmlText text = elT.OwnerDocument.CreateTextNode(s);
      elT.AppendChild(text);

      if (sel.CellStyle.TextLeader != BRTextLeader.None)
      {
        if (sel.ActualHAlign == BRHAlign.Center || sel.ActualHAlign == BRHAlign.Left)
        {
          XmlElement elR2 = elP.OwnerDocument.CreateElement("w:r", nmspcW);
          elP.AppendChild(elR2);
          WriteRPr(elR2, sel, columnWidth);
          XmlElement elTab2 = elR2.OwnerDocument.CreateElement("w:tab", nmspcW);
          elR2.AppendChild(elTab2);
        }
      }
    }

    /// <summary>
    /// Запись атрибутов абзаца
    /// </summary>
    private void WritePPr(XmlElement elP, BRSelector sel, int columnWidth, bool isSimpleBand, bool keepWithNext)
    {
      XmlDocument xmlDoc = elP.OwnerDocument;
      XmlElement elPPr = xmlDoc.CreateElement("w:pPr", nmspcW);

      //XmlElement elSpacing = xmlDoc.CreateElement("w:spacing", nmspcW);
      //elPPr.AppendChild(elSpacing);
      //SetAttr(elSpacing, "w:before", "0", nmspcW);
      //SetAttr(elSpacing, "w:after", "0", nmspcW);

      #region Отступ

      if (sel.CellStyle.IndentLevel > 0)
      {
        int off = BRFileTools.GetIndentWidth(sel.CellStyle, _Measurer);
        if (off > 0)
        {
          XmlElement elInd = xmlDoc.CreateElement("w:ind", nmspcW);
          elPPr.AppendChild(elInd);
          switch (sel.ActualHAlign)
          {
            case BRHAlign.Left:
              SetAttr(elInd, "w:left", GetTwips01Attr(off), nmspcW);
              break;
            case BRHAlign.Right:
              SetAttr(elInd, "w:right", GetTwips01Attr(off), nmspcW);
              break;
          }
        }
      }

      #endregion

      #region Горизонтальное выравнивание

      if (sel.ActualHAlign != BRHAlign.Left && sel.CellStyle.TextLeader == BRTextLeader.None)
      {
        // Горизонтальное выравнивание не применяется при наличии позиций табуляции
        {
          XmlElement elJc = xmlDoc.CreateElement("w:jc", nmspcW);
          elPPr.AppendChild(elJc);
          switch (sel.ActualHAlign)
          {
            case BRHAlign.Center:
              SetAttr(elJc, "w:val", "center", nmspcW);
              break;
            case BRHAlign.Right:
              SetAttr(elJc, "w:val", "right", nmspcW);
              break;
          }
        }
      }

      #endregion

      #region Межстрочный интервал

      if (sel.CellStyle.LineHeightTwip > 0)
      {
        XmlElement elSpacing = xmlDoc.CreateElement("w:spacing", nmspcW);
        elPPr.AppendChild(elSpacing);
        SetAttr(elSpacing, "w:line", StdConvert.ToString(sel.CellStyle.LineHeightTwip), nmspcW);
        SetAttr(elSpacing, "w:line-rule", "exact", nmspcW);
      }

      #endregion

      #region Табуляции для Leader

      if (sel.CellStyle.TextLeader != BRTextLeader.None)
      {
        XmlElement elTabs = xmlDoc.CreateElement("w:tabs", nmspcW);
        elPPr.AppendChild(elTabs);

        string ledWidth = "hyphen"; // других вариантов нет
        //if (Info.Cell.Font.Leader==PaperDocLeader.Thick)
        //  LedWidth = "heavy";

        if (sel.ActualHAlign == BRHAlign.Center)
        {
          XmlElement elTab1 = xmlDoc.CreateElement("w:tab", nmspcW);
          elTabs.AppendChild(elTab1);
          SetAttr(elTab1, "w:val", "center", nmspcW);
          SetAttr(elTab1, "w:leader", ledWidth, nmspcW);
          SetAttr(elTab1, "w:pos", StdConvert.ToString(((int)((columnWidth / 254f * 72f * 20f / 2f)))), nmspcW);
        }

        XmlElement elTab2 = xmlDoc.CreateElement("w:tab", nmspcW);
        elTabs.AppendChild(elTab2);
        SetAttr(elTab2, "w:val", "right", nmspcW);
        SetAttr(elTab2, "w:leader", ledWidth, nmspcW);
        SetAttr(elTab2, "w:pos", StdConvert.ToString((int)((columnWidth / 254f * 72f * 20f))), nmspcW);

      }

      #endregion

      #region Цвет фона

      if (isSimpleBand && (!sel.CellStyle.BackColor.IsAuto))
      {
        XmlElement elShd = xmlDoc.CreateElement("w:shd", nmspcW);
        elPPr.AppendChild(elShd);
        SetAttr(elShd, "w:val", "clear", nmspcW);
        //SetAttr(elShd, "w:color", "auto", nmspcW);
        SetAttr(elShd, "w:fill", GetColorHex(sel.CellStyle.BackColor), nmspcW);
      }

      #endregion

      #region Не отрывать от следующего

      if (keepWithNext)
      {
        XmlElement elKeepNext = xmlDoc.CreateElement("w:keepNext", nmspcW);
        elPPr.AppendChild(elKeepNext);
      }

      if (isSimpleBand)
      {
        XmlElement elKeepLines = xmlDoc.CreateElement("w:keepLines", nmspcW);
        elPPr.AppendChild(elKeepLines);
      }

      #endregion

      if (elPPr.HasChildNodes)
        elP.AppendChild(elPPr);
    }

    /// <summary>
    /// Запись атрибутов текста
    /// </summary>
    private void WriteRPr(XmlElement elR, BRSelector sel, int columnWidth)
    {
      XmlDocument xmlDoc = elR.OwnerDocument;

      XmlElement elRPr = xmlDoc.CreateElement("w:rPr", nmspcW);

      BRCellStyle defStyle = sel.Band.Report.DefaultCellStyle;

      if (sel.CellStyle.FontName != defStyle.FontName)
      {
        XmlElement elRFonts = xmlDoc.CreateElement("w:rFonts", nmspcW);
        elRPr.AppendChild(elRFonts);
        SetAttr(elRFonts, "w:ascii", sel.CellStyle.FontName, nmspcW);
      }

      if (sel.CellStyle.FontHeightTwip != defStyle.FontHeightTwip)
      {
        XmlElement elSz = xmlDoc.CreateElement("w:sz", nmspcW);
        elRPr.AppendChild(elSz);
        SetAttr(elSz, "w:val", (sel.CellStyle.FontHeightTwip / 10).ToString(StdConvert.NumberFormat), nmspcW); // в полупунктах
      }

      if (sel.CellStyle.Bold)
      {
        XmlElement elB = xmlDoc.CreateElement("w:b", nmspcW);
        elRPr.AppendChild(elB);
      }
      if (sel.CellStyle.Italic)
      {
        XmlElement elI = xmlDoc.CreateElement("w:i", nmspcW);
        elRPr.AppendChild(elI);
      }
      if (sel.CellStyle.Underline)
      {
        XmlElement elU = xmlDoc.CreateElement("w:u", nmspcW);
        elRPr.AppendChild(elU);
        SetAttr(elU, "w:val", "single", nmspcW);
      }
      if (sel.CellStyle.Strikeout)
      {
        XmlElement elStrike = xmlDoc.CreateElement("w:strike", nmspcW);
        elRPr.AppendChild(elStrike);
      }

      //if (sel.AsString == "Text 12345678")
      //{
      //}

      int widthPercent = BRFileTools.GetFontWidthPercent(sel, _Measurer, columnWidth);

      widthPercent = Math.Max(widthPercent, MinFontWidthPercent);
      widthPercent = Math.Min(widthPercent, MaxFontWidthPercent);

      if (widthPercent != 100)
      {
        XmlElement elW = xmlDoc.CreateElement("w:w", nmspcW);
        elRPr.AppendChild(elW);
        SetAttr(elW, "w:val", StdConvert.ToString(widthPercent), nmspcW);
      }

      if (!sel.CellStyle.ForeColor.IsAuto)
      {
        XmlElement elColor = xmlDoc.CreateElement("w:color", nmspcW);
        elRPr.AppendChild(elColor);
        SetAttr(elColor, "w:val", GetColorHex(sel.CellStyle.ForeColor), nmspcW);
      }

      if (elRPr.HasChildNodes) // чтобы не добавлять пустышку
        elR.AppendChild(elRPr);
    }

    #endregion

    private static bool IsSimpleBand(BRSelector sel)
    {
      if (sel.Band.RowCount > 1 || sel.Band.ColumnCount > 1)
        return false;

      if (sel.RowInfo.Height != BRReport.AutoRowHeight)
        return false;

      //string s = sel.AsString;
      //if (s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0)
      //  return false;

      //if (!sel.CellStyle.BackColor.IsAuto)
      //  return false; // цвет задается в ячейке, а не в абзаце

      if (sel.CellStyle.TextLeader != BRTextLeader.None)
        return false;

      return
      sel.CellStyle.LeftBorder == BRLine.None &&
      sel.CellStyle.TopBorder == BRLine.None &&
      sel.CellStyle.RightBorder == BRLine.None &&
      sel.CellStyle.BottomBorder == BRLine.None &&
      sel.CellStyle.DiagonalUp == BRLine.None &&
      sel.CellStyle.DiagonalDown == BRLine.None;
    }

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
          valueText = DataTools.ReplaceAny(valueText, BRFileTools.BadValueChars, ' ');
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

    private static void WriteMargins(XmlElement elAnyPr, bool IsTable, int LeftMargin, int TopMargin, int RightMargin, int BottomMargin)
    {
      XmlElement elTcMar = elAnyPr.OwnerDocument.CreateElement(IsTable ? "w:tblCellMar" : "w:tcMar", nmspcW);
      elAnyPr.AppendChild(elTcMar);

      XmlElement elLeft = elAnyPr.OwnerDocument.CreateElement("w:left", nmspcW);
      elTcMar.AppendChild(elLeft);
      SetAttr(elLeft, "w:w", GetTwips01Attr(LeftMargin), nmspcW);
      SetAttr(elLeft, "w:type", "dxa", nmspcW);

      XmlElement elTop = elAnyPr.OwnerDocument.CreateElement("w:top", nmspcW);
      elTcMar.AppendChild(elTop);
      SetAttr(elTop, "w:w", GetTwips01Attr(TopMargin), nmspcW);
      SetAttr(elTop, "w:type", "dxa", nmspcW);

      XmlElement elRight = elAnyPr.OwnerDocument.CreateElement("w:right", nmspcW);
      elTcMar.AppendChild(elRight);
      SetAttr(elRight, "w:w", GetTwips01Attr(RightMargin), nmspcW);
      SetAttr(elRight, "w:type", "dxa", nmspcW);

      XmlElement elBottom = elAnyPr.OwnerDocument.CreateElement("w:bottom", nmspcW);
      elTcMar.AppendChild(elBottom);
      SetAttr(elBottom, "w:w", GetTwips01Attr(BottomMargin), nmspcW);
      SetAttr(elBottom, "w:type", "dxa", nmspcW);
    }

    private static void WriteBorder(XmlElement elTcBorders, BRLine border, string tagName)
    {
      if (border.Style == BRLineStyle.None)
        return;

      XmlElement elOne = elTcBorders.OwnerDocument.CreateElement(tagName, nmspcW);
      elTcBorders.AppendChild(elOne);
      switch (border.Style)
      {
        case BRLineStyle.Thin:
        case BRLineStyle.Medium:
        case BRLineStyle.Thick:
          SetAttr(elOne, "w:val", "single", nmspcW);
          break;
        case BRLineStyle.Dot:
          SetAttr(elOne, "w:val", "dotted", nmspcW);
          break;
        case BRLineStyle.Dash:
          SetAttr(elOne, "w:val", "dashed", nmspcW);
          break;
        case BRLineStyle.DashDot:
          SetAttr(elOne, "w:val", "dot-dash", nmspcW);
          break;
        case BRLineStyle.DashDotDot:
          SetAttr(elOne, "w:val", "dot-dot-dash", nmspcW);
          break;
      }

      // толщина линии в единицах 1/8 пт.
      int w = (int)(BRLine.GetLineWidthPt(border.Style) * 8);
      SetAttr(elOne, "w:sz", StdConvert.ToString(w), nmspcW);

      if (!border.Color.IsAuto)
        SetAttr(elOne, "w:color", GetColorHex(border.Color), nmspcW);
    }

    #endregion

    #region Вспомогательные методы
    private static string GetTwips01Attr(int size01mm)
    {
      return StdConvert.ToString((int)((float)size01mm / 254f * 1440f));
    }

    private static string GetColorHex(BRColor clr)
    {
      return clr.R.ToString("X2") + clr.G.ToString("X2") + clr.B.ToString("X2");
    }

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
