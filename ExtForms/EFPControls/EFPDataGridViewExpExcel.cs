using FreeLibSet.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FreeLibSet.Config;
using System.Globalization;
using FreeLibSet.Core;
using FreeLibSet.Shell;

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
  #region Перечисление EFPDataGridViewExpExcelBoolMode

  /// <summary>
  /// Значения свойства EFPDataGridViewExpExcelSettings.BoolMode
  /// </summary>
  public enum EFPDataGridViewExpExcelBoolMode
  {
    // Члены не переименовывать!
    // Используются при записи конфигурации

    /// <summary>
    /// Не заменять ("ИСТИНА", "ЛОЖЬ")
    /// </summary>
    Boolean,

    /// <summary>
    /// 1=ИСТИНА, 0=ЛОЖЬ
    /// </summary>
    Digit,

    /// <summary>
    /// [X]=ИСТИНА, [0]=ЛОЖЬ
    /// </summary>
    Brackets,
  }

  #endregion

  /// <summary>
  /// Настройки для копирования таблицы в Microsoft Excel
  /// Доступны через свойство GridPageSetupBase.SendToExcel
  /// </summary>
  public class EFPDataGridViewExpExcelSettings
  {
    #region Конструктор

    /// <summary>
    /// Конструктор по умолчанию задает режим "Таблица как на экране",
    /// "Все ячейки" и все включенные опции
    /// </summary>
    public EFPDataGridViewExpExcelSettings()
    {
      _RangeMode = EFPDataGridViewExpRange.All;
      _ShowColumnHeaders = true;
      _UseInterior = true;
      _UseBorders = true;
      _BoolMode = EFPDataGridViewExpExcelBoolMode.Boolean;
    }

    #endregion

    #region Параметры

    /// <summary>
    /// Диапазон ячеек для экспорта (по умолчанию - All)
    /// </summary>
    public EFPDataGridViewExpRange RangeMode { get { return _RangeMode; } set { _RangeMode = value; } }
    private EFPDataGridViewExpRange _RangeMode;

    /// <summary>
    /// true (по умолчанию) - выводить заголовки столбцов
    /// </summary>
    public bool ShowColumnHeaders { get { return _ShowColumnHeaders; } set { _ShowColumnHeaders = value; } }
    private bool _ShowColumnHeaders;

    /// <summary>
    /// Раскрашивать ячейки
    /// </summary>
    public bool UseInterior { get { return _UseInterior; } set { _UseInterior = value; } }
    private bool _UseInterior;

    /// <summary>
    /// Границы ячеек
    /// </summary>
    public bool UseBorders { get { return _UseBorders; } set { _UseBorders = value; } }
    private bool _UseBorders;

    /// <summary>
    /// Режим вывода логических значений
    /// </summary>
    public EFPDataGridViewExpExcelBoolMode BoolMode { get { return _BoolMode; } set { _BoolMode = value; } }
    private EFPDataGridViewExpExcelBoolMode _BoolMode;

    /// <summary>
    /// Использовать "неэкономное" форматирование для повышения читаемости XML-файла
    /// Используется для отладки
    /// </summary>
    public bool DebugXml { get { return _DebugXml; } set { _DebugXml = value; } }
    private bool _DebugXml;

    #endregion

    #region Чтение и запись значений

    /// <summary>
    /// Запись настроек в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      cfg.SetString("Range", RangeMode.ToString());
      cfg.SetBool("NoColumnHeaders", !ShowColumnHeaders);
      cfg.SetBool("NoInterior", !UseInterior);
      cfg.SetBool("NoBorders", !UseBorders);
      cfg.SetString("BoolMode", BoolMode.ToString());
    }

    /// <summary>
    /// Чтение настроек из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (cfg.GetString("Range") == "Selected")
        RangeMode = EFPDataGridViewExpRange.Selected;
      else
        RangeMode = EFPDataGridViewExpRange.All;
      ShowColumnHeaders = !cfg.GetBool("NoColumnHeaders");
      UseInterior = !cfg.GetBool("NoInterior");
      UseBorders = !cfg.GetBool("NoBorders");
      switch (cfg.GetString("BoolMode"))
      {
        case "Digit": BoolMode = EFPDataGridViewExpExcelBoolMode.Digit; break;
        case "Brackets": BoolMode = EFPDataGridViewExpExcelBoolMode.Brackets; break;
        default: BoolMode = EFPDataGridViewExpExcelBoolMode.Boolean; break;
      }
    }

    #endregion
  }

  /// <summary>
  /// Сохранение табличного просмотра в XML-формате Microsoft Excel XP и 2003
  /// Файл не сжимается и имеет расширение XML
  /// </summary>
  internal static class EFPDataGridViewExpExcel2003
  {
    #region Основной метод

    public static void SaveFile(EFPDataGridView controlProvider, string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      XmlDocument XmlDoc = CreateXml(controlProvider, settings);
      //XmlDoc.Save(FileName);

      XmlWriterSettings XmlSettings = new XmlWriterSettings();
      if (settings.DebugXml)
      {
        XmlSettings.Encoding = Encoding.GetEncoding(1251);
        XmlSettings.Indent = true;
      }
      XmlWriter wrt = XmlWriter.Create(fileName, XmlSettings);
      try
      {
        XmlDoc.WriteTo(wrt);
      }
      finally
      {
        wrt.Close();
      }
    }

    const string nmspcX = "urn:schemas-microsoft-com:office:excel";

    const string nmspcSS = "urn:schemas-microsoft-com:office:spreadsheet";

    const string nmspcO = "urn:schemas-microsoft-com:office:office";

    /// <summary>
    /// Список плохих символов, которые не могут использоваться в значении
    /// </summary>
    public static readonly string BadValueChars = CreateBadValueChars();

    private static string CreateBadValueChars()
    {
      StringBuilder sb = new StringBuilder(32);
      for (int i = 0; i < 32; i++)
      {
        char ch = (char)i;
        if (ch == '\n')
          continue;
        sb.Append(ch);
      }

      return sb.ToString();
    }

    public static XmlDocument CreateXml(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings)
    {
      // 1. Собираем массив печатаемых столбцов и строк
      EFPDataGridViewRectArea Area = controlProvider.GetRectArea(settings.RangeMode);

      XmlDocument XmlDoc = new XmlDocument();

      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", settings.DebugXml ? "Windows-1251" : "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlProcessingInstruction XmlPI = XmlDoc.CreateProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
      XmlDoc.AppendChild(XmlPI);


      XmlAttribute attr;
      XmlElement elWholeDoc = XmlDoc.CreateElement("Workbook", nmspcSS);
      XmlDoc.AppendChild(elWholeDoc);
      attr = XmlDoc.CreateAttribute("xmlns");  // без namespace
      attr.Value = nmspcSS;
      elWholeDoc.Attributes.Append(attr);
      attr = XmlDoc.CreateAttribute("xmlns:o");
      attr.Value = nmspcO;
      elWholeDoc.Attributes.Append(attr);
      attr = XmlDoc.CreateAttribute("xmlns:x");
      attr.Value = nmspcX;
      elWholeDoc.Attributes.Append(attr);
      attr = XmlDoc.CreateAttribute("xmlns:ss");
      attr.Value = nmspcSS;
      elWholeDoc.Attributes.Append(attr);

      // Сводка
      XmlElement elDocumentProperties = XmlDoc.CreateElement("DocumentProperties", nmspcO);
      elWholeDoc.AppendChild(elDocumentProperties);

      EFPDocumentProperties Props = controlProvider.DocumentProperties;

      if (!String.IsNullOrEmpty(Props.Title))
      {
        XmlElement elTitle = XmlDoc.CreateElement("Title", nmspcO);
        elDocumentProperties.AppendChild(elTitle);
        elTitle.AppendChild(XmlDoc.CreateTextNode(Props.Title));
      }
      if (!String.IsNullOrEmpty(Props.Subject))
      {
        XmlElement elSubject = XmlDoc.CreateElement("Subject", nmspcO);
        elDocumentProperties.AppendChild(elSubject);
        elSubject.AppendChild(XmlDoc.CreateTextNode(Props.Subject));
      }
      if (!String.IsNullOrEmpty(Props.Author))
      {
        XmlElement elAuthor = XmlDoc.CreateElement("Author", nmspcO);
        elDocumentProperties.AppendChild(elAuthor);
        elAuthor.AppendChild(XmlDoc.CreateTextNode(Props.Author));
      }
      if (!String.IsNullOrEmpty(Props.Company))
      {
        XmlElement elCompany = XmlDoc.CreateElement("Company", nmspcO);
        elDocumentProperties.AppendChild(elCompany);
        elCompany.AppendChild(XmlDoc.CreateTextNode(Props.Company));
      }

      // Таблица стилей нужна обязательно. 
      // Первый стиль "ColHdr" используется для заголовков,
      // Остальные стили добавляются в таблицу по мере необходимости, чтобы
      // не делать для каждой ячейки отдельный стили
      XmlElement elStyles = XmlDoc.CreateElement("Styles", nmspcSS);
      elWholeDoc.AppendChild(elStyles);
      // М.б. нужен стиль "Default"?

      XmlElement elAlign, elFont;

      // Атрибуты заголовков столбцов
      if (settings.ShowColumnHeaders)
      {
        XmlElement elStyleHead = XmlDoc.CreateElement("Style", nmspcSS);
        attr = XmlDoc.CreateAttribute("ss:ID", nmspcSS);
        attr.Value = "ColHdr";
        elStyleHead.Attributes.Append(attr);
        elStyles.AppendChild(elStyleHead);

        elAlign = XmlDoc.CreateElement("Alignment", nmspcSS);
        elStyleHead.AppendChild(elAlign);
        attr = XmlDoc.CreateAttribute("ss:Horizontal", nmspcSS);
        attr.Value = "Center";
        elAlign.Attributes.Append(attr);
        attr = XmlDoc.CreateAttribute("ss:Vertical", nmspcSS);
        attr.Value = "Center";
        elAlign.Attributes.Append(attr);
        attr = XmlDoc.CreateAttribute("ss:WrapText", nmspcSS);
        attr.Value = "1";
        elAlign.Attributes.Append(attr);

        if (settings.UseBorders)
        {
          XmlElement elBorders = XmlDoc.CreateElement("Borders", nmspcSS);
          elStyleHead.AppendChild(elBorders);

          AddBorder(elBorders, "Bottom", EFPDataGridViewBorderStyle.Thin);
          AddBorder(elBorders, "Left", EFPDataGridViewBorderStyle.Thin);
          AddBorder(elBorders, "Right", EFPDataGridViewBorderStyle.Thin);
          AddBorder(elBorders, "Top", EFPDataGridViewBorderStyle.Thin);
        }

        elFont = XmlDoc.CreateElement("Font", nmspcSS);
        elStyleHead.AppendChild(elFont);
        attr = XmlDoc.CreateAttribute("ss:Bold", nmspcSS);
        attr.Value = "1";
        elFont.Attributes.Append(attr);
      }


      // Один лист в книге
      XmlElement elWorksheet = XmlDoc.CreateElement("Worksheet", nmspcSS);
      elWholeDoc.AppendChild(elWorksheet);
      attr = XmlDoc.CreateAttribute("ss:Name", nmspcSS);
      attr.Value = "Таблица";
      elWorksheet.Attributes.Append(attr);

      // На листе одна таблица
      XmlElement elTable = XmlDoc.CreateElement("Table", nmspcSS);
      elWorksheet.AppendChild(elTable);

      // Размеры столбцов
      for (int j = 0; j < Area.ColumnCount; j++)
      {
        XmlElement elColumn = XmlDoc.CreateElement("Column", nmspcSS);
        attr = XmlDoc.CreateAttribute("ss:Width", nmspcSS);
        EFPDataGridViewColumn Col = controlProvider.Columns[Area.ColumnIndices[j]];
        attr.Value = Col.WidthPt.ToString(); // в пунктах
        elColumn.Attributes.Append(attr);
        elTable.AppendChild(elColumn);
      }

      // Заголовки столбцов
      if (settings.ShowColumnHeaders)
      {
        EFPDataGridViewColumnHeaderArray HeaderArray = controlProvider.GetColumnHeaderArray(Area);

        for (int i = 0; i < HeaderArray.RowCount; i++)
        {
          XmlElement elHeadRow = XmlDoc.CreateElement("Row", nmspcSS);
          elTable.AppendChild(elHeadRow);
          for (int j = 0; j < HeaderArray.ColumnCount; j++)
          {
            if (HeaderArray.RowSpan[i, j] == 0)
              continue; // не первая строка объединения

            XmlElement elCell = XmlDoc.CreateElement("Cell", nmspcSS);
            elHeadRow.AppendChild(elCell);

            attr = XmlDoc.CreateAttribute("ss:StyleID", nmspcSS);
            attr.Value = "ColHdr";
            elCell.Attributes.Append(attr);

            attr = XmlDoc.CreateAttribute("ss:Index", nmspcSS);
            attr.Value = (j + 1).ToString();
            elCell.Attributes.Append(attr);

            if (HeaderArray.RowSpan[i, j] > 1)
            {
              attr = XmlDoc.CreateAttribute("ss:MergeDown", nmspcSS);
              attr.Value = (HeaderArray.RowSpan[i, j] - 1).ToString();
              elCell.Attributes.Append(attr);
            }

            if (HeaderArray.ColumnSpan[i, j] > 1)
            {
              attr = XmlDoc.CreateAttribute("ss:MergeAcross", nmspcSS);
              attr.Value = (HeaderArray.ColumnSpan[i, j] - 1).ToString();
              elCell.Attributes.Append(attr);
            }

            string s = HeaderArray.Text[i, j];
            if (!String.IsNullOrEmpty(s))
            {
              s = s.Replace("\r\n", "\n"); // только символ 0x0A
              s = s.Replace(DataTools.SoftHyphenStr, ""); // не знаю, как записать мягкий перенос
              // неразрывный пробел так и остается
              s = DataTools.ReplaceAny(s, BadValueChars, ' '); // непечатные символы

              XmlElement elData = XmlDoc.CreateElement("Data", nmspcSS);
              elCell.AppendChild(elData);
              elData.InnerText = s;

              attr = XmlDoc.CreateAttribute("ss:Type", nmspcSS);
              attr.Value = "String";
              elData.Attributes.Append(attr);
            }
          }
        }
      }

      StringBuilder sb = new StringBuilder();

      // Сюда запоминаем существующие стили ячеек
      List<string> CellStyleKeys = new List<string>();

      // Хранилище для границ ячеек
      EFPDataGridViewBorderStyle[,] BorderStyles = null;
      if (settings.UseBorders)
        BorderStyles = new EFPDataGridViewBorderStyle[Area.ColumnCount, 6];

      // Перебираем строки таблицы
      for (int i = 0; i < Area.RowCount; i++)
      {
        // Границы надо запрашивать отдельно, в режиме печати
        if (settings.UseBorders)
        {
          controlProvider.DoGetRowAttributes(Area.RowIndices[i], EFPDataGridViewAttributesReason.Print);
          for (int j = 0; j < Area.ColumnCount; j++)
          {
            int ColumnIndex = Area.ColumnIndices[j];
            EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
            BorderStyles[j, 0] = CellArgs.BottomBorder;
            BorderStyles[j, 1] = CellArgs.LeftBorder;
            BorderStyles[j, 2] = CellArgs.RightBorder;
            BorderStyles[j, 3] = CellArgs.TopBorder;
            if (CellArgs.DiagonalUpBorder == EFPDataGridViewBorderStyle.Default)
              BorderStyles[j, 4] = EFPDataGridViewBorderStyle.None;
            else
              BorderStyles[j, 4] = CellArgs.DiagonalUpBorder;
            if (CellArgs.DiagonalDownBorder == EFPDataGridViewBorderStyle.Default)
              BorderStyles[j, 5] = EFPDataGridViewBorderStyle.None;
            else
              BorderStyles[j, 5] = CellArgs.DiagonalDownBorder;
          }
        }

        controlProvider.DoGetRowAttributes(Area.RowIndices[i], EFPDataGridViewAttributesReason.View);

        XmlElement elRow = XmlDoc.CreateElement("Row", nmspcSS);
        elTable.AppendChild(elRow);

        for (int j = 0; j < Area.ColumnCount; j++)
        {
          int ColumnIndex = Area.ColumnIndices[j];
          EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
          EFPDataGridViewExcelCellAttributes ExcelAttr = EFPDataGridView.GetExcelCellAttr(CellArgs);

          XmlElement elCell = XmlDoc.CreateElement("Cell", nmspcSS);
          elRow.AppendChild(elCell);


          string ValueText, TypeText, FormatText;
          GetCellValue(CellArgs, out ValueText, out TypeText, out FormatText, settings);

          bool WordWrap = CellArgs.CellStyle.WrapMode == DataGridViewTriState.True;
          if (CellArgs.Column.GridColumn is DataGridViewTextBoxColumn)
          {
            Type ValueType = ((DataGridViewTextBoxColumn)(CellArgs.Column.GridColumn)).ValueType;
            if (ValueType == typeof(string) || ValueType == null)
              WordWrap = true;
          }

          sb.Length = 0;
          sb.Append(FormatText);
          sb.Append("|");
          sb.Append(CellArgs.CellStyle.Alignment.ToString());
          sb.Append("|");
          sb.Append(WordWrap.ToString());
          sb.Append("|");
          sb.Append(CellArgs.IndentLevel.ToString());
          sb.Append("|");
          if (settings.UseInterior)
          {
            sb.Append(ExcelAttr.BackColor.ToString());
            sb.Append("|");
            sb.Append(ExcelAttr.ForeColor.ToString());
            sb.Append("|");
            sb.Append(ExcelAttr.Bold);
            sb.Append("|");
            sb.Append(ExcelAttr.Italic);
            sb.Append("|");
            sb.Append(ExcelAttr.Underline);
            sb.Append("|");
          }
          if (settings.UseBorders)
          {
            for (int k = 0; k < BorderStyles.GetLength(1); k++)
            {
              sb.Append(BorderStyles[j, k]);
              sb.Append("|");
            }
          }
          int StyleKeyIndex = CellStyleKeys.IndexOf(sb.ToString());
          if (StyleKeyIndex < 0)
          {
            // Требуется добавить стиль
            CellStyleKeys.Add(sb.ToString());
            StyleKeyIndex = CellStyleKeys.Count - 1;

            XmlElement elStyle = XmlDoc.CreateElement("Style", nmspcSS);
            attr = XmlDoc.CreateAttribute("ss:ID", nmspcSS);
            attr.Value = "s" + (StyleKeyIndex + 100).ToString();
            elStyle.Attributes.Append(attr);
            elStyles.AppendChild(elStyle);

            string ha, va;
            switch (CellArgs.CellStyle.Alignment)
            {
              case DataGridViewContentAlignment.TopLeft: ha = "Left"; va = "Top"; break;
              case DataGridViewContentAlignment.TopCenter: ha = "Center"; va = "Top"; break;
              case DataGridViewContentAlignment.TopRight: ha = "Right"; va = "Top"; break;
              case DataGridViewContentAlignment.MiddleLeft: ha = "Left"; va = "Center"; break;
              case DataGridViewContentAlignment.MiddleCenter: ha = "Center"; va = "Center"; break;
              case DataGridViewContentAlignment.MiddleRight: ha = "Right"; va = "Center"; break;
              case DataGridViewContentAlignment.BottomLeft: ha = "Left"; va = "Bottom"; break;
              case DataGridViewContentAlignment.BottomCenter: ha = "Center"; va = "Bottom"; break;
              case DataGridViewContentAlignment.BottomRight: ha = "Right"; va = "Bottom"; break;
              default: throw new BugException("Неизвестное выравнивание");
            }

            elAlign = XmlDoc.CreateElement("Alignment", nmspcSS);
            elStyle.AppendChild(elAlign);
            attr = XmlDoc.CreateAttribute("ss:Horizontal", nmspcSS);
            attr.Value = ha;
            elAlign.Attributes.Append(attr);
            attr = XmlDoc.CreateAttribute("ss:Vertical", nmspcSS);
            attr.Value = va;
            elAlign.Attributes.Append(attr);
            if (WordWrap)
            {
              attr = XmlDoc.CreateAttribute("ss:WrapText", nmspcSS);
              attr.Value = "1";
              elAlign.Attributes.Append(attr);
            }
            if (CellArgs.IndentLevel > 0)
            {
              attr = XmlDoc.CreateAttribute("ss:Indent", nmspcSS);
              attr.Value = CellArgs.IndentLevel.ToString();
              elAlign.Attributes.Append(attr);
            }

            if (settings.UseBorders)
            {
              XmlElement elBorders = XmlDoc.CreateElement("Borders", nmspcSS);
              elStyle.AppendChild(elBorders);

              AddBorder(elBorders, "Bottom", BorderStyles[j, 0]);
              AddBorder(elBorders, "Left", BorderStyles[j, 1]);
              AddBorder(elBorders, "Right", BorderStyles[j, 2]);
              AddBorder(elBorders, "Top", BorderStyles[j, 3]);
              AddBorder(elBorders, "DiagonalRight", BorderStyles[j, 4]);
              AddBorder(elBorders, "DiagonalLeft", BorderStyles[j, 5]);
            }

            if (settings.UseInterior)
            {
              elFont = XmlDoc.CreateElement("Font", nmspcSS);
              elStyle.AppendChild(elFont);

              if (ExcelAttr.Bold)
              {
                attr = XmlDoc.CreateAttribute("ss:Bold", nmspcSS);
                attr.Value = "1";
                elFont.Attributes.Append(attr);
              }
              if (!ExcelAttr.ForeColor.IsEmpty)
              {
                attr = XmlDoc.CreateAttribute("ss:Color", nmspcSS);
                attr.Value = MyColorStr(ExcelAttr.ForeColor);
                elFont.Attributes.Append(attr);
              }
              if (ExcelAttr.Italic)
              {
                attr = XmlDoc.CreateAttribute("ss:Italic", nmspcSS);
                attr.Value = "1";
                elFont.Attributes.Append(attr);
              }
              if (ExcelAttr.Underline)
              {
                attr = XmlDoc.CreateAttribute("ss:Underline", nmspcSS);
                attr.Value = "Single";
                elFont.Attributes.Append(attr);
              }

              if (!ExcelAttr.BackColor.IsEmpty)
              {
                XmlElement elInterior = XmlDoc.CreateElement("Interior", nmspcSS);
                elStyle.AppendChild(elInterior);
                attr = XmlDoc.CreateAttribute("ss:Color", nmspcSS);
                attr.Value = MyColorStr(ExcelAttr.BackColor);
                elInterior.Attributes.Append(attr);
                attr = XmlDoc.CreateAttribute("ss:Pattern", nmspcSS);
                attr.Value = "Solid";
                elInterior.Attributes.Append(attr);
              }
            } // UseFill

            if (!String.IsNullOrEmpty(FormatText))
            {
              XmlElement elNumberFormat = XmlDoc.CreateElement("NumberFormat", nmspcSS);
              elStyle.AppendChild(elNumberFormat);
              attr = XmlDoc.CreateAttribute("ss:Format", nmspcSS);
              attr.Value = FormatText;
              elNumberFormat.Attributes.Append(attr);
            }
          }

          attr = XmlDoc.CreateAttribute("ss:StyleID", nmspcSS);
          attr.Value = "s" + (StyleKeyIndex + 100).ToString();
          elCell.Attributes.Append(attr);

          if (TypeText != null)
          {
            XmlElement elData = XmlDoc.CreateElement("Data", nmspcSS);
            elCell.AppendChild(elData);
            elData.InnerText = ValueText;

            attr = XmlDoc.CreateAttribute("ss:Type", nmspcSS);
            attr.Value = TypeText;
            elData.Attributes.Append(attr);
          }
        }
      }
      return XmlDoc;
    }

    private static void AddBorder(XmlElement elBorders, string pos, EFPDataGridViewBorderStyle borderStyle)
    {
      if (borderStyle == EFPDataGridViewBorderStyle.None)
        return;

      XmlElement elBorder = elBorders.OwnerDocument.CreateElement("Border", nmspcSS);
      elBorders.AppendChild(elBorder);

      XmlAttribute attr;
      attr = elBorder.OwnerDocument.CreateAttribute("ss:Position", nmspcSS);
      attr.Value = pos;
      elBorder.Attributes.Append(attr);

      attr = elBorder.OwnerDocument.CreateAttribute("ss:LineStyle", nmspcSS);
      attr.Value = "Continuous";
      elBorder.Attributes.Append(attr);

      attr = elBorder.OwnerDocument.CreateAttribute("ss:Weight", nmspcSS);
      switch (borderStyle)
      {
        case EFPDataGridViewBorderStyle.Default:
          attr.Value = "0.5";
          break;
        case EFPDataGridViewBorderStyle.Thin:
          attr.Value = "1";
          break;
        case EFPDataGridViewBorderStyle.Thick:
          attr.Value = "2";
          break;
      }
      elBorder.Attributes.Append(attr);
    }

    private static string MyColorStr(Color c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    private static void GetCellValue(EFPDataGridViewCellAttributesEventArgs cellArgs, out string valueText, out string typeText, out string formatText, EFPDataGridViewExpExcelSettings settings)
    {
      valueText = null;
      typeText = null;
      formatText = null;

      if (!cellArgs.ContentVisible)
        return; // 24.08.2015

      GetCellValue2(cellArgs.FormattedValue, ref valueText, ref typeText, ref formatText, settings);

      if (!String.IsNullOrEmpty(cellArgs.CellStyle.Format))
        formatText = cellArgs.CellStyle.Format;
    }

    private static void GetCellValue2(object v, ref string valueText, ref string typeText, ref string formatText, EFPDataGridViewExpExcelSettings settings)
    {
      if (v == null)
        return;
      if (v is DBNull)
        return;

      if (v is Image) // 14.01.2014
        return;

      switch (v.GetType().Name)
      {
        case "String":
        case "Guid": // 03.02.2020
          if (v.ToString().Trim().Length == 0) // 08.08.2012
            return;
          valueText = v.ToString();
          valueText = DataTools.ReplaceAny(valueText, BadValueChars, ' ');
          typeText = "String";
          break;
        case "Boolean":
          switch (settings.BoolMode)
          {
            case EFPDataGridViewExpExcelBoolMode.Boolean:
              valueText = (bool)v ? "1" : "0";
              typeText = "Boolean";
              break;
            case EFPDataGridViewExpExcelBoolMode.Digit:
              valueText = (bool)v ? "1" : "0";
              typeText = "Number";
              break;
            case EFPDataGridViewExpExcelBoolMode.Brackets:
              valueText = (bool)v ? "[X]" : "[ ]";
              typeText = "String";
              break;
            default:
              throw new InvalidOperationException("Неизвестный режим преобразования логчиеских значений: " + settings.BoolMode.ToString());
          }
          break;
        case "DateTime":
          // 05.04.2016. Предотвращаем ошибку открытия файла из-за неправильной даты
          if ((DateTime)v < MicrosoftOfficeTools.MinExcelDate || ((DateTime)v).Date > MicrosoftOfficeTools.MaxExcelDate)
          {
            valueText = "*** Дата, выходящая за допустимые пределы Excel: " + v.ToString() + " ***";
            typeText = "String";
            return;
          }

          valueText = ((DateTime)v).ToString("s");
          typeText = "DateTime";

          // 01.09.2015
          // Если формат данных в DataGridViewCellStyle не задан, используем формат по умолчанию
          // Если оставить формат "Общий", то даты будут отображаться в Excel как числа
          if (((DateTime)v).TimeOfDay.Ticks != 0L)
            formatText = MicrosoftOfficeTools.DefaultShortDateTimeFormat;
          else
            formatText = MicrosoftOfficeTools.DefaultShortDateFormat;
          break;
        default:
          if (DataTools.IsIntegerType(v.GetType()) || DataTools.IsFloatType(v.GetType()))
          {
            valueText = Convert.ToString(v, DataTools.DotNumberConv);
            typeText = "Number";
          }
          break;
      }
    }

    #endregion
  }


  /// <summary>
  /// Сохранение табличного просмотра в XML-формате Microsoft Excel 2007 и 2010
  /// Файл является ZIP-архивом, содержащим несколько вложенных XML-Файлов и имеет расширение .xlsx
  /// </summary>
  internal static class EFPDataGridViewExpExcel2007
  {
    #region Основной метод

    public static void SaveFile(EFPDataGridView controlProvider, string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      ZipFileTools.CheckZipLibAvailable();
      SaveFile2(controlProvider, fileName, settings);
    }
    
    private static void SaveFile2(EFPDataGridView controlProvider, string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      #region Начало

      //SharpZipLibLoader.LoadSharpZipLib();
      ZipFileCreator ZF = new ZipFileCreator(fileName);

      #endregion

      #region Заголовок ([Content_Types].xml)

      XmlDocument XmlDocCT = CreateContentTypes();
      ZF.AddXmlFile("[Content_Types].xml", XmlDocCT);

      #endregion

      #region Связи (_rels/.rels)

      XmlDocument XmlDocRels1 = CreateRels1();
      ZF.AddXmlFile("_rels/.rels", XmlDocRels1);

      XmlDocument XmlDocRels2 = CreateRels2();
      ZF.AddXmlFile("xl/_rels/workbook.xml.rels", XmlDocRels2);

      #endregion

      #region Сводка

#if XXXX
      // !!!!!
      // Что-то формируется неправильно. Excel не читает, LibreOffice читает

      XmlDocument XmlDocCore = CreateCore();
      ZF.AddXmlFile("docProps/core.xml", XmlDocCore);

      XmlDocument XmlDocApp = CreateApp();
      ZF.AddXmlFile("docProps/app.xml", XmlDocApp);
#endif

      #endregion

      #region Workbook

      XmlDocument XmlDocWorkbook = CreateWorkbook();
      ZF.AddXmlFile("xl/workbook.xml", XmlDocWorkbook);

      #endregion

      #region Worksheet

      List<string> SharedStrings;
      int SharedStringCount;
      StyleTable Styles = new StyleTable(controlProvider, settings);
      XmlDocument XmlDocSheet = CreateSheet(controlProvider, settings, out SharedStrings, out SharedStringCount, Styles);
      ZF.AddXmlFile("xl/worksheets/sheet1.xml", XmlDocSheet);

      XmlDocument XmlDocStyles = Styles.FinishDoc();
      ZF.AddXmlFile("xl/styles.xml", XmlDocStyles);

      #endregion

      #region Список строк

      XmlDocument XmlDocSharedStrings = CreateSharedStrings(SharedStrings, SharedStringCount);
      ZF.AddXmlFile("xl/sharedStrings.xml", XmlDocSharedStrings);

      #endregion

      #region Конец

      ZF.Close();

      #endregion
    }

    #endregion

    #region Вспомогательные файлы для OpenXML

    private const string nmspcContentTypes = "http://schemas.openxmlformats.org/package/2006/content-types";

    private static XmlDocument CreateContentTypes()
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("Types", nmspcContentTypes);
      SetAttr(elRoot, "xmlns", nmspcContentTypes, String.Empty);
      XmlDoc.AppendChild(elRoot);

      XmlElement elDefault = XmlDoc.CreateElement("Default", nmspcContentTypes);
      SetAttr(elDefault, "Extension", "rels", String.Empty);
      SetAttr(elDefault, "ContentType", "application/vnd.openxmlformats-package.relationships+xml", String.Empty);
      elRoot.AppendChild(elDefault);

      elDefault = XmlDoc.CreateElement("Default", nmspcContentTypes);
      SetAttr(elDefault, "Extension", "xml", String.Empty);
      SetAttr(elDefault, "ContentType", "application/xml", String.Empty);
      elRoot.AppendChild(elDefault);

      XmlElement elOverride = XmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/workbook.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = XmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/worksheets/sheet1.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = XmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/sharedStrings.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = XmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/styles.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      return XmlDoc;
    }

    private const string nmspcPackageRels = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string nmspcOfficeRels = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static XmlDocument CreateRels1()
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      XmlDoc.AppendChild(elRoot);

      XmlElement elRL = XmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId1", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", String.Empty);
      SetAttr(elRL, "Target", "xl/workbook.xml", String.Empty);
      elRoot.AppendChild(elRL);

#if XXX
      elRL = XmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId2", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties", String.Empty);
      SetAttr(elRL, "Target", "docProps/core.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = XmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId3", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties", String.Empty);
      SetAttr(elRL, "Target", "docProps/app.xml", String.Empty);
      elRoot.AppendChild(elRL);
#endif

      return XmlDoc;
    }

    private static XmlDocument CreateRels2()
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      XmlDoc.AppendChild(elRoot);

      XmlElement elRL = XmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId1", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", String.Empty);
      SetAttr(elRL, "Target", "worksheets/sheet1.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = XmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId2", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings", String.Empty);
      SetAttr(elRL, "Target", "sharedStrings.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = XmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId3", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", String.Empty);
      SetAttr(elRL, "Target", "styles.xml", String.Empty);
      elRoot.AppendChild(elRL);

      return XmlDoc;
    }


#if XXX

    private const string nmspcCoreProps = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";

    private const string nmspcDCElems = "http://purl.org/dc/elements/1.1/";

    private const string nmspcExtProps = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";

    private static XmlDocument CreateCore()
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("cp:coreProperties", nmspcCoreProps);
      SetAttr(elRoot, "xmlns:cp", nmspcCoreProps, String.Empty);
      SetAttr(elRoot, "xmlns:dc", nmspcDCElems, String.Empty);
      XmlDoc.AppendChild(elRoot);

      XmlElement elCreator = XmlDoc.CreateElement("dc:creator", nmspcDCElems);
      elCreator.InnerText = AccDepClientExec.UserName;
      elRoot.AppendChild(elCreator);

      return XmlDoc;
    }

    private static XmlDocument CreateApp()
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("Properties", nmspcExtProps);
      SetAttr(elRoot, "xmlns", nmspcExtProps, String.Empty);
      XmlDoc.AppendChild(elRoot);

      XmlElement elCompany = XmlDoc.CreateElement("Company", nmspcExtProps);
      elCompany.InnerText = AccDepClientExec.OurOrg.ShortName;
      elRoot.AppendChild(elCompany);

      return XmlDoc;
    }
#endif

    private const string nmspcSpreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    private static XmlDocument CreateWorkbook()
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("workbook", nmspcSpreadsheet);
      SetAttr(elRoot, "xmlns", nmspcSpreadsheet, String.Empty);
      SetAttr(elRoot, "xmlns:r", nmspcOfficeRels, String.Empty);
      XmlDoc.AppendChild(elRoot);

      XmlElement elSheets = XmlDoc.CreateElement("sheets", nmspcSpreadsheet);
      elRoot.AppendChild(elSheets);

      XmlElement elSheet = XmlDoc.CreateElement("sheet", nmspcSpreadsheet);
      SetAttr(elSheet, "name", "1", String.Empty);
      SetAttr(elSheet, "sheetId", "1", String.Empty);
      SetAttr(elSheet, "r:id", "rId1", nmspcOfficeRels);
      elSheets.AppendChild(elSheet);

      return XmlDoc;
    }

    #endregion

    #region Таблица стилей

    /// <summary>
    /// Заполнитель для файла styles.xml
    /// </summary>
    private class StyleTable
    {
      #region Конструктор

      public StyleTable(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings)
      {
        _ControlProvider = controlProvider;
        _Settings = settings;

        _NumFmtDict = new Dictionary<string, int>();
        _FontDict = new Dictionary<string, int>();
        _FillDict = new Dictionary<string, int>();
        _BordersDict = new Dictionary<string, int>();
        _CellXfsDict = new Dictionary<string, int>();

        _XmlDoc = new XmlDocument();
        XmlDeclaration xmldecl = _XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        _XmlDoc.InsertBefore(xmldecl, _XmlDoc.DocumentElement);

        _elRoot = _XmlDoc.CreateElement("styleSheet", nmspcSpreadsheet);
        SetAttr(_elRoot, "xmlns", nmspcSpreadsheet, String.Empty);
        _XmlDoc.AppendChild(_elRoot);

        // Обязаетельно создаем корневые узды сейчас, иначе они будут не в том порядке
        _elNumFmtRoot = _XmlDoc.CreateElement("numFmts", nmspcSpreadsheet);
        _elRoot.AppendChild(_elNumFmtRoot);
        _elFontRoot = _XmlDoc.CreateElement("fonts", nmspcSpreadsheet);
        _elRoot.AppendChild(_elFontRoot);

        _elFillRoot = _XmlDoc.CreateElement("fills", nmspcSpreadsheet);
        _elRoot.AppendChild(_elFillRoot);

        // Тут бяка в Excel
        // Должно быть как минимум 2 входа заполнения.
        // Первое - пустое
        // второе - штриховка
        GetFillIdx(Color.Empty);

        XmlElement elFill1 = _XmlDoc.CreateElement("fill", nmspcSpreadsheet);
        _elFillRoot.AppendChild(elFill1);

        XmlElement elPatternFill = _XmlDoc.CreateElement("patternFill", nmspcSpreadsheet);
        SetAttr(elPatternFill, "patternType", "gray125", String.Empty);
        elFill1.AppendChild(elPatternFill);
        _FillDict.Add("Dummy1", 1);

        _elBordersRoot = _XmlDoc.CreateElement("borders", nmspcSpreadsheet);
        _elRoot.AppendChild(_elBordersRoot);
        _elCellXfsRoot = _XmlDoc.CreateElement("cellXfs", nmspcSpreadsheet);
        _elRoot.AppendChild(_elCellXfsRoot);

      }

      #endregion

      #region Свойства

      private EFPDataGridView _ControlProvider;

      private EFPDataGridViewExpExcelSettings _Settings;

      /// <summary>
      /// Документ styles.xml
      /// </summary>
      private XmlDocument _XmlDoc;

      private XmlElement _elRoot;
      private XmlElement _elNumFmtRoot, _elFontRoot, _elFillRoot, _elBordersRoot, _elCellXfsRoot;

      Dictionary<string, int> _NumFmtDict;
      Dictionary<string, int> _FontDict;
      Dictionary<string, int> _FillDict;
      Dictionary<string, int> _BordersDict;
      Dictionary<string, int> _CellXfsDict;

      #endregion

      #region Методы

      public int GetStyle(bool isBold, bool isItalic, bool isUnderline, Color backColor, Color foreColor, 
        EFPDataGridViewBorderStyle leftBorder, EFPDataGridViewBorderStyle topBorder, EFPDataGridViewBorderStyle rightBorder, EFPDataGridViewBorderStyle bottomBorder,
        EFPDataGridViewBorderStyle diagonalUpBorder, EFPDataGridViewBorderStyle diagonalDownBorder,
        DataGridViewContentAlignment align, int offset, bool wordWrap, string numberFormat)
      {
        #region Числовые форматы

        int NumFmtId = 0; // общий формат
        if (!String.IsNullOrEmpty(numberFormat))
        {
          if (!_NumFmtDict.TryGetValue(numberFormat, out NumFmtId))
          {
            if (_elNumFmtRoot == null)
            {
              _elNumFmtRoot = _XmlDoc.CreateElement("numFmts", nmspcSpreadsheet);
              _elRoot.AppendChild(_elNumFmtRoot);
            }

            XmlElement elNumFmt = _XmlDoc.CreateElement("numFmt", nmspcSpreadsheet);
            NumFmtId = _NumFmtDict.Count + 1001; // Начинаем нумерацию с 1001
            SetAttr(elNumFmt, "numFmtId", NumFmtId.ToString(), String.Empty);
            SetAttr(elNumFmt, "formatCode", numberFormat, String.Empty);
            _elNumFmtRoot.AppendChild(elNumFmt);

            _NumFmtDict.Add(numberFormat, NumFmtId);
          }
        }

        #endregion

        #region Шрифт

        if (!_Settings.UseInterior)
          foreColor = Color.Empty;

        string FontKey = isBold.ToString() + "|" + isItalic.ToString() + "|" + isUnderline.ToString() + "|" + foreColor.ToString();
        int FontIdx;
        if (!_FontDict.TryGetValue(FontKey, out FontIdx))
        {
          if (_elFontRoot == null)
          {
            _elFontRoot = _XmlDoc.CreateElement("fonts", nmspcSpreadsheet);
            _elRoot.AppendChild(_elFontRoot);
          }


          XmlElement elFont = _XmlDoc.CreateElement("font", nmspcSpreadsheet);
          _elFontRoot.AppendChild(elFont);
          /*
          XmlElement elFontName = XmlDoc.CreateElement("name", nmspcSpreadsheet);
          SetAttr(elFontName, "val", FHandler.Control.Font.Name, String.Empty);
          elFont.AppendChild(elFontName);
          XmlElement elFontSize = XmlDoc.CreateElement("sz", nmspcSpreadsheet);
          SetAttr(elFontSize, "val", DataConv.FloatToStr(FHandler.Control.Font.Size), String.Empty);
          elFont.AppendChild(elFontSize);
          */
          if (isBold)
          {
            XmlElement elBold = _XmlDoc.CreateElement("b", nmspcSpreadsheet);
            elFont.AppendChild(elBold);
          }
          if (isItalic)
          {
            XmlElement elBold = _XmlDoc.CreateElement("i", nmspcSpreadsheet);
            elFont.AppendChild(elBold);
          }
          if (isUnderline)
          {
            XmlElement elBold = _XmlDoc.CreateElement("u", nmspcSpreadsheet);
            elFont.AppendChild(elBold);
          }

          if (!foreColor.IsEmpty)
          {
            XmlElement elColor = _XmlDoc.CreateElement("color", nmspcSpreadsheet);
            SetAttr(elColor, "rgb", MyColorStr(foreColor), String.Empty);
            elFont.AppendChild(elColor);
          }

          FontIdx = _FontDict.Count;
          _FontDict.Add(FontKey, FontIdx);
        }

        #endregion

        #region Заполнение

        if (!_Settings.UseInterior)
          backColor = Color.Empty;

        int FillIdx = GetFillIdx(backColor);

        #endregion

        #region Границы

        string BorderKey = leftBorder.ToString() + "|" + topBorder.ToString() + "|" + rightBorder.ToString() + "|" + bottomBorder.ToString()+
          "|" + diagonalUpBorder.ToString()+"|" + diagonalDownBorder.ToString();
        int BorderIdx;
        if (!_BordersDict.TryGetValue(BorderKey, out BorderIdx))
        {
          if (_elBordersRoot == null)
          {
            _elBordersRoot = _XmlDoc.CreateElement("borders", nmspcSpreadsheet);
            _elRoot.AppendChild(_elBordersRoot);
          }

          XmlElement elBorder = _XmlDoc.CreateElement("border", nmspcSpreadsheet);
          _elBordersRoot.AppendChild(elBorder);

          AddBorder(elBorder, "left", leftBorder);
          AddBorder(elBorder, "right", rightBorder);
          AddBorder(elBorder, "top", topBorder);
          AddBorder(elBorder, "bottom", bottomBorder);

          // Диагональные линии хранятся по-другому
          // Используется общая толщина линии
          if (diagonalUpBorder >= EFPDataGridViewBorderStyle.Thin || diagonalDownBorder >= EFPDataGridViewBorderStyle.Thin)
          {
            if (diagonalUpBorder >= EFPDataGridViewBorderStyle.Thin)
              SetAttr(elBorder, "diagonalUp", "1", nmspcSpreadsheet);
            if (diagonalDownBorder >= EFPDataGridViewBorderStyle.Thin)
              SetAttr(elBorder, "diagonalDown", "1", nmspcSpreadsheet);

            //AddBorder(elBorder, "diagonal", diagonalUpBorder | diagonalDownBorder);
            AddBorder(elBorder, "diagonal", (EFPDataGridViewBorderStyle)Math.Max((int)diagonalUpBorder, (int)diagonalDownBorder));
          }

          BorderIdx = _BordersDict.Count;
          _BordersDict.Add(BorderKey, BorderIdx);
        }

        #endregion

        #region Стиль ячейки

        string StyleKey = NumFmtId.ToString() + "|" + FontIdx.ToString() + "|" + FillIdx.ToString() + "|" + BorderIdx.ToString() + "|" + align.ToString() + "|" + offset.ToString() + "|" + wordWrap.ToString();
        int StyleIdx;
        if (!_CellXfsDict.TryGetValue(StyleKey, out StyleIdx))
        {
          if (_elCellXfsRoot == null)
          {
            _elCellXfsRoot = _XmlDoc.CreateElement("cellXfs", nmspcSpreadsheet);
            _elRoot.AppendChild(_elCellXfsRoot);
          }

          XmlElement elXf = _XmlDoc.CreateElement("xf", nmspcSpreadsheet);

          if (NumFmtId > 0)
          {
            SetAttr(elXf, "numFmtId", NumFmtId.ToString(), String.Empty);
            SetAttr(elXf, "applyNumberFormat", "1", String.Empty);
          }

          if (FontIdx > 0)
          {
            SetAttr(elXf, "fontId", FontIdx.ToString(), String.Empty);
            SetAttr(elXf, "applyFont", "1", String.Empty);
          }

          if (!backColor.IsEmpty)
          {
            SetAttr(elXf, "fillId", FillIdx.ToString(), String.Empty);
            SetAttr(elXf, "applyFill", "1", String.Empty);
          }

          if (BorderIdx > 0)
          {
            SetAttr(elXf, "borderId", BorderIdx.ToString(), String.Empty);
            SetAttr(elXf, "applyBorder", "1", String.Empty);
          }
          _elCellXfsRoot.AppendChild(elXf);

          string ha, va;
          switch (align)
          {
            case DataGridViewContentAlignment.TopLeft: ha = "left"; va = "top"; break;
            case DataGridViewContentAlignment.TopCenter: ha = "center"; va = "top"; break;
            case DataGridViewContentAlignment.TopRight: ha = "right"; va = "top"; break;
            case DataGridViewContentAlignment.MiddleLeft: ha = "left"; va = "center"; break;
            case DataGridViewContentAlignment.MiddleCenter: ha = "center"; va = "center"; break;
            case DataGridViewContentAlignment.MiddleRight: ha = "right"; va = "center"; break;
            case DataGridViewContentAlignment.BottomLeft: ha = "left"; va = "bottom"; break;
            case DataGridViewContentAlignment.BottomCenter: ha = "center"; va = "bottom"; break;
            case DataGridViewContentAlignment.BottomRight: ha = "right"; va = "bottom"; break;
            default: throw new BugException("Неизвестное выравнивание");
          }

          XmlElement elAlignment = _XmlDoc.CreateElement("alignment", nmspcSpreadsheet);
          SetAttr(elAlignment, "horizontal", ha, String.Empty);
          SetAttr(elAlignment, "vertical", va, String.Empty);
          SetAttr(elXf, "applyAlignment", "1", String.Empty);

          if (offset > 0)
            SetAttr(elAlignment, "indent", offset.ToString(), String.Empty);
          if (wordWrap)
            SetAttr(elAlignment, "wrapText", "1", String.Empty);
          elXf.AppendChild(elAlignment);

          StyleIdx = _CellXfsDict.Count;
          _CellXfsDict.Add(StyleKey, StyleIdx);
        }

        #endregion

        return StyleIdx;
      }

      private int GetFillIdx(Color backColor)
      {
        string FillKey = backColor.ToString();
        int FillIdx;
        if (!_FillDict.TryGetValue(FillKey, out FillIdx))
        {
          if (_elFillRoot == null)
          {
            _elFillRoot = _XmlDoc.CreateElement("fills", nmspcSpreadsheet);
            _elRoot.AppendChild(_elFillRoot);
          }


          XmlElement elFill = _XmlDoc.CreateElement("fill", nmspcSpreadsheet);
          _elFillRoot.AppendChild(elFill);

          XmlElement elPatternFill = _XmlDoc.CreateElement("patternFill", nmspcSpreadsheet);
          if (backColor.IsEmpty)
            SetAttr(elPatternFill, "patternType", "none", String.Empty);
          else
          {
            SetAttr(elPatternFill, "patternType", "solid", String.Empty);
            XmlElement elFgColor = _XmlDoc.CreateElement("fgColor", nmspcSpreadsheet);
            SetAttr(elFgColor, "rgb", MyColorStr(backColor), String.Empty);
            elPatternFill.AppendChild(elFgColor);
          }

          elFill.AppendChild(elPatternFill);

          FillIdx = _FillDict.Count;
          _FillDict.Add(FillKey, FillIdx);
        }
        return FillIdx;
      }

      private static void AddBorder(XmlElement elParent, string tagName, EFPDataGridViewBorderStyle borderStyle)
      {
        XmlElement elBorder = elParent.OwnerDocument.CreateElement(tagName, nmspcSpreadsheet);
        elParent.AppendChild(elBorder);

        switch (borderStyle)
        {
          case EFPDataGridViewBorderStyle.Default:
            SetAttr(elBorder, "style", "thin", String.Empty);
            break;
          case EFPDataGridViewBorderStyle.Thin:
            SetAttr(elBorder, "style", "medium", String.Empty);
            break;
          case EFPDataGridViewBorderStyle.Thick:
            SetAttr(elBorder, "style", "thick", String.Empty);
            break;
        }
      }

      private static string MyColorStr(Color c)
      {
        Int32 x = (c.R << 16) | (c.G << 8) | c.B;
        if ((c.R & 0x80) != 0)
          x = x | (0xFF << 24);
        return x.ToString("X8");
      }

      public XmlDocument FinishDoc()
      {
        if (_elNumFmtRoot != null)
          SetAttr(_elNumFmtRoot, "count", _NumFmtDict.Count.ToString(), String.Empty);
        if (_elFontRoot != null)
          SetAttr(_elFontRoot, "count", _FontDict.Count.ToString(), String.Empty);
        if (_elFillRoot != null)
          SetAttr(_elFillRoot, "count", _FillDict.Count.ToString(), String.Empty);
        if (_elBordersRoot != null)
          SetAttr(_elBordersRoot, "count", _BordersDict.Count.ToString(), String.Empty);
        if (_elCellXfsRoot != null)
          SetAttr(_elCellXfsRoot, "count", _CellXfsDict.Count.ToString(), String.Empty);

        return _XmlDoc;
      }

      #endregion
    }

    #endregion

    #region Создание листа и таблицы стилей

    private static XmlDocument CreateSheet(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings, out List<string> sharedStrings, out int sharedStringCount, StyleTable styles)
    {
      #region Собираем массив печатаемых столбцов и строк

      EFPDataGridViewRectArea Area = controlProvider.GetRectArea(settings.RangeMode);

      #endregion

      #region Таблица разделяемых строк

      sharedStrings = new List<string>();
      Dictionary<string, int> SharedStringIndices = new Dictionary<string, int>(); // индексы строк в массиве SharedStrings
      sharedStringCount = 0;

      #endregion

      #region Начало документа sheet1.xml

      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elSheetRoot = XmlDoc.CreateElement("worksheet", nmspcSpreadsheet);
      SetAttr(elSheetRoot, "xmlns", nmspcSpreadsheet, String.Empty);
      SetAttr(elSheetRoot, "xmlns:r", nmspcOfficeRels, String.Empty);
      XmlDoc.AppendChild(elSheetRoot);

      #endregion

      #region Размеры столбцов

      XmlElement elCols = XmlDoc.CreateElement("cols", nmspcSpreadsheet);
      elSheetRoot.AppendChild(elCols);

      for (int j = 0; j < Area.ColumnCount; j++)
      {
        EFPDataGridViewColumn Column = controlProvider.Columns[Area.ColumnIndices[j]];
        XmlElement elCol = XmlDoc.CreateElement("col", nmspcSpreadsheet);
        SetAttr(elCol, "min", (j + 1).ToString(), String.Empty);
        SetAttr(elCol, "max", (j + 1).ToString(), String.Empty);
        SetAttr(elCol, "width", Column.TextWidth.ToString("0.##", DataTools.DotNumberConv), String.Empty);
        elCols.AppendChild(elCol);
      }

      #endregion

      /*int DefaultStyleIdx = */styles.GetStyle(false, false, false, Color.Empty, Color.Empty,
        EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None,
        EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None,
        DataGridViewContentAlignment.MiddleCenter, 0, false, String.Empty); // основной шрифт

      #region Заголовки столбцов

      XmlElement elSheetData = XmlDoc.CreateElement("sheetData", nmspcSpreadsheet);
      elSheetRoot.AppendChild(elSheetData);

      XmlElement elMergeCells = XmlDoc.CreateElement("mergeCells", nmspcSpreadsheet);
      // Потом добавим elSheetRoot.AppendChild(elMergeCells);

      int RowCount = 0; // счетчик строк

      // Заголовки столбцов
      if (settings.ShowColumnHeaders)
      {
        int HeaderStyleIdx = styles.GetStyle(true, false, false, Color.Empty, Color.Empty,
          EFPDataGridViewBorderStyle.Thin, EFPDataGridViewBorderStyle.Thin, EFPDataGridViewBorderStyle.Thin, EFPDataGridViewBorderStyle.Thin,
          EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None,
          DataGridViewContentAlignment.MiddleCenter, 0, true, String.Empty);

        EFPDataGridViewColumnHeaderArray HeaderArray = controlProvider.GetColumnHeaderArray(Area);

        for (int i = 0; i < HeaderArray.RowCount; i++)
        {
          RowCount++;

          XmlElement elRow = XmlDoc.CreateElement("row", nmspcSpreadsheet);
          SetAttr(elRow, "r", RowCount.ToString(), String.Empty);
          elSheetData.AppendChild(elRow);

          for (int j = 0; j < HeaderArray.ColumnCount; j++)
          {
            if (HeaderArray.RowSpan[i, j] == 0)
              continue; // не первая строка объединения

            XmlElement elC = XmlDoc.CreateElement("c", nmspcSpreadsheet);

            string CellAddr1 = MicrosoftOfficeTools.GetExcelColumnName(j + 1) + RowCount.ToString();
            SetAttr(elC, "r", CellAddr1, String.Empty);
            SetAttr(elC, "s", HeaderStyleIdx.ToString(), String.Empty);
            elRow.AppendChild(elC);
            string s = HeaderArray.Text[i, j];
            WriteCellString(elC, s, sharedStrings, SharedStringIndices, ref sharedStringCount);

            if (HeaderArray.RowSpan[i, j] > 1 || HeaderArray.ColumnSpan[i, j] > 1)
            {
              string CellAddr2 = MicrosoftOfficeTools.GetExcelColumnName(j + HeaderArray.ColumnSpan[i, j]) + (RowCount + HeaderArray.RowSpan[i, j] - 1).ToString();
              XmlElement elMergeCell = XmlDoc.CreateElement("mergeCell", nmspcSpreadsheet);
              SetAttr(elMergeCell, "ref", CellAddr1 + ":" + CellAddr2, String.Empty);
              elMergeCells.AppendChild(elMergeCell);
            }
          }
        }
      }


      #endregion

      #region Цикл по строкам

      // Перебираем строки таблицы
      for (int i = 0; i < Area.RowCount; i++)
      {
        RowCount++;

        XmlElement elRow = XmlDoc.CreateElement("row", nmspcSpreadsheet);
        SetAttr(elRow, "r", RowCount.ToString(), String.Empty);
        elSheetData.AppendChild(elRow);

        controlProvider.DoGetRowAttributes(Area.RowIndices[i], EFPDataGridViewAttributesReason.View);

        // Перебираем столбцы
        for (int j = 0; j < Area.ColumnCount; j++)
        {
          int ColumnIndex = Area.ColumnIndices[j];

          EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
          EFPDataGridViewExcelCellAttributes ExcelAttr = EFPDataGridView.GetExcelCellAttr(CellArgs);

          XmlElement elC = XmlDoc.CreateElement("c", nmspcSpreadsheet);
          SetAttr(elC, "r", MicrosoftOfficeTools.GetExcelColumnName(j + 1) + RowCount.ToString(), String.Empty);
          int StyleIdx = styles.GetStyle(ExcelAttr.Bold, ExcelAttr.Italic, ExcelAttr.Underline,
            ExcelAttr.BackColor, ExcelAttr.ForeColor,
            CellArgs.LeftBorder, CellArgs.TopBorder, CellArgs.RightBorder, CellArgs.BottomBorder,
            CellArgs.DiagonalUpBorder, CellArgs.DiagonalDownBorder,
            CellArgs.CellStyle.Alignment, CellArgs.IndentLevel, CellArgs.CellStyle.WrapMode == DataGridViewTriState.True, CellArgs.CellStyle.Format);
          SetAttr(elC, "s", StyleIdx.ToString(), String.Empty);
          elRow.AppendChild(elC);

          string ValueText, TypeText, FormatText;
          GetCellValue(CellArgs, out ValueText, out TypeText, out FormatText, settings);

          if (!String.IsNullOrEmpty(TypeText))
          {
            if (TypeText == "s")
              WriteCellString(elC, ValueText, sharedStrings, SharedStringIndices, ref sharedStringCount);
            else
            {
              if (!String.IsNullOrEmpty(TypeText))
                SetAttr(elC, "t", TypeText, String.Empty);

              XmlElement elV = elC.OwnerDocument.CreateElement("v", nmspcSpreadsheet);
              elV.InnerText = ValueText;
              elC.AppendChild(elV);
            }
          }
        }
      }


      #endregion

      // 06.04.2016
      // Узел "mergeCells" добавляется, только если есть объединенные ячейки
      if (elMergeCells.ChildNodes.Count > 0)
      {
        SetAttr(elMergeCells, "count", elMergeCells.ChildNodes.Count.ToString(), String.Empty); // число объединений
        elSheetRoot.AppendChild(elMergeCells);
      }

      return XmlDoc;
    }

    private static void GetCellValue(EFPDataGridViewCellAttributesEventArgs cellArgs, out string valueText, out string typeText, out string formatText, EFPDataGridViewExpExcelSettings settings)
    {
      valueText = null;
      typeText = null;
      formatText = null;

      if (!cellArgs.ContentVisible)
        return; // 24.08.2015

      GetCellValue2(cellArgs.FormattedValue, ref valueText, ref typeText, ref formatText, settings);

      if (!String.IsNullOrEmpty(cellArgs.CellStyle.Format))
        formatText = cellArgs.CellStyle.Format;
    }

    private static void GetCellValue2(object v, ref string valueText, ref string typeText, ref string formatText, EFPDataGridViewExpExcelSettings settings)
    {
      if (v == null)
        return;
      if (v is DBNull)
        return;

      switch (v.GetType().Name)
      {
        case "String":
        case "Guid": // 03.02.2020
          if (v.ToString().Trim().Length == 0) // 08.08.2012
            return;
          valueText = v.ToString();
          valueText = DataTools.ReplaceAny(valueText, EFPDataGridViewExpExcel2003.BadValueChars, ' ');
          typeText = "s";
          break;
        case "Boolean":
          switch (settings.BoolMode)
          {
            case EFPDataGridViewExpExcelBoolMode.Boolean:
              valueText = (bool)v ? "1" : "0";
              typeText = "b";
              break;
            case EFPDataGridViewExpExcelBoolMode.Digit:
              valueText = (bool)v ? "1" : "0";
              typeText = "n";
              break;
            case EFPDataGridViewExpExcelBoolMode.Brackets:
              valueText = (bool)v ? "[X]" : "[ ]";
              typeText = "s";
              break;
            default:
              throw new InvalidOperationException("Неизвестный режим преобразования логических значений: " + settings.BoolMode.ToString());
          }
          break;
        case "DateTime":
          //ValueText = ((DateTime)v).ToString("s");
          // 05.04.2016. Дата/время должно задаваться как число
          try
          {
            double v2 = ((DateTime)v).ToOADate();
            valueText = Convert.ToString(v2, DataTools.DotNumberConv);
          }
          catch
          {
            valueText = "*** Дата, выходящая за допустимые пределы: " + v.ToString() + " ***";
            typeText = "s";
            return;
          }

          // 01.09.2015
          // Если формат данных в DataGridViewCellStyle не задан, используем формат по умолчанию
          // Если оставить формат "Общий", то даты будут отображаться в Excel как числа
          if (((DateTime)v).TimeOfDay.Ticks != 0L)
            formatText = MicrosoftOfficeTools.DefaultShortDateTimeFormat;
          else
            formatText = MicrosoftOfficeTools.DefaultShortDateFormat;
          typeText = "t";
          break;
        default:
          if (DataTools.IsIntegerType(v.GetType()) || DataTools.IsFloatType(v.GetType()))
          {
            valueText = Convert.ToString(v, DataTools.DotNumberConv);
            typeText = "n";
          }
          break;
      }
    }

    #region Запись значения

    private static void WriteCellString(XmlElement elC, string s, List<string> sharedStrings, Dictionary<string, int> sharedStringIndices,
      ref int sharedStringCount)
    {
      if (String.IsNullOrEmpty(s))
        return;

      s = s.Replace("\r\n", "\n"); // только символ 0x0A
      s = s.Replace(DataTools.SoftHyphenStr, ""); // не знаю, как записать мягкий перенос
      // неразрывный пробел так и остается

      int ssi;
      if (!sharedStringIndices.TryGetValue(s, out ssi))
      {
        // Добавляем строку в справочник разделяемых строк
        sharedStrings.Add(s);
        ssi = sharedStrings.Count - 1;
        sharedStringIndices.Add(s, ssi);
      }

      SetAttr(elC, "t", "s", String.Empty);

      XmlElement elV = elC.OwnerDocument.CreateElement("v", nmspcSpreadsheet);
      elV.InnerText = ssi.ToString();
      elC.AppendChild(elV);
      sharedStringCount++;
    }

    #endregion

    #endregion

    #region SharedStrings

    private static XmlDocument CreateSharedStrings(List<string> sharedStrings, int sharedStringCount)
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("sst", nmspcSpreadsheet);
      SetAttr(elRoot, "count", sharedStringCount.ToString(), String.Empty);
      SetAttr(elRoot, "uniqueCount", sharedStrings.Count.ToString(), String.Empty);
      SetAttr(elRoot, "xmlns", nmspcSpreadsheet, String.Empty);
      XmlDoc.AppendChild(elRoot);

      for (int i = 0; i < sharedStrings.Count; i++)
      {
        XmlElement elSI = XmlDoc.CreateElement("si", nmspcSpreadsheet);
        elRoot.AppendChild(elSI);
        //XmlElement elR = XmlDoc.CreateElement("r", nmspcSpreadsheet);
        //elSI.AppendChild(elR);
        XmlElement elT = XmlDoc.CreateElement("t", nmspcSpreadsheet);
        elT.InnerText = sharedStrings[i];
        elSI.AppendChild(elT);
      }
      return XmlDoc;
    }

    #endregion

    #region Вспомогательные методы

    private static void SetAttr(XmlElement el, string name, string value, string nmspc)
    {
      XmlAttribute Attr;
      if (String.IsNullOrEmpty(nmspc))
        Attr = el.OwnerDocument.CreateAttribute(name);
      else
        Attr = el.OwnerDocument.CreateAttribute(name, nmspc);
      Attr.Value = value;
      el.Attributes.Append(Attr);
    }

    #endregion
  }

  internal static class EFPDataGridViewExpOpenOfficeCalc
  {
    #region Создание ODS-файла

    #region Основной метод

    public static void SaveFile(EFPDataGridView controlProvider, string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      ZipFileTools.CheckZipLibAvailable();
      SaveFile2(controlProvider, fileName, settings);
    }

    private static void SaveFile2(EFPDataGridView controlProvider, string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      #region Начало

      //SharpZipLibLoader.LoadSharpZipLib();
      ZipFileCreator ZF = new ZipFileCreator(fileName);
      ZF.AddMimeType("application/vnd.oasis.opendocument.spreadsheet");

      #endregion

      #region Манифест (manifest.xml)

      XmlDocument XmlDoc = CreateManifest();
      ZF.AddXmlFile("META-INF/manifest.xml", XmlDoc);

      #endregion

      #region Метаданные

      XmlDoc = CreateMeta(controlProvider/*, settings*/);
      ZF.AddXmlFile("meta.xml", XmlDoc);

      #endregion

      #region Основной документ

      XmlDoc = CreateContent(controlProvider, settings);
      ZF.AddXmlFile("content.xml", XmlDoc);

      #endregion

      #region Конец

      ZF.Close();

      //ZF.DebugOutFiles();

      #endregion
    }

    #endregion

    #region Манифест

    private static XmlDocument CreateManifest()
    {
      const string nmspcManifest = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";

      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmlDecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      XmlDoc.InsertBefore(xmlDecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("manifest:manifest", nmspcManifest);
      XmlDoc.AppendChild(elRoot);

      SetAttr(elRoot, "xmlns:manifest", nmspcManifest, String.Empty);
      SetAttr(elRoot, "manifest:version", "1.2", nmspcManifest);

      XmlElement elFile;

      elFile = XmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "application/vnd.oasis.opendocument.spreadsheet", nmspcManifest);
      SetAttr(elFile, "manifest:version", "1.2", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "/", nmspcManifest);

      elFile = XmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "text/xml", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "meta.xml", nmspcManifest);

      elFile = XmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "text/xml", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "content.xml", nmspcManifest);

      return XmlDoc;
    }

    #endregion

    #region Метаданные (meta.xml)

    const string nmspcOffice = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    const string nmspcMeta = "urn:oasis:names:tc:opendocument:xmlns:meta:1.0";
    const string nmspcDc = "http://purl.org/dc/elements/1.1/";

    private static XmlDocument CreateMeta(EFPDataGridView controlProvider/*, EFPDataGridViewExpExcelSettings settings*/)
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmlDecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      XmlDoc.InsertBefore(xmlDecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("office:document-meta", nmspcOffice);
      XmlDoc.AppendChild(elRoot);
      SetAttr(elRoot, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRoot, "xmlns:meta", nmspcMeta, String.Empty);
      SetAttr(elRoot, "xmlns:dc", nmspcDc, String.Empty);

      XmlElement elMeta = XmlDoc.CreateElement("office:meta", nmspcOffice);
      elRoot.AppendChild(elMeta);

      EFPDocumentProperties Props = controlProvider.DocumentProperties;

      // Требуется соответствие с определением useragent
      // в протоколе HTTP, описанного в разделе 14.43 [RFC2616]
      //AddTextNode(elMeta, "meta:generator", "Бухгалтерия ИП Агеев А.В., v." +
      //  AccDepDataTools.AccDepModuleVersion, nmspcMeta);
      if (!String.IsNullOrEmpty(Props.Author))
      {
        AddTextNode(elMeta, "meta:initial-creator", Props.Author, nmspcMeta);
        AddTextNode(elMeta, "meta:creator", Props.Author, nmspcMeta);
      }
      AddTextNode(elMeta, "meta:creation-date", DateTime.Now.ToString("s"), nmspcMeta);

      // Некуда засунуть название организации
      //if (AccDepClientExec.OurOrgId != 0)
      //  AddTextNode(elMeta, "meta:company", AccDepClientExec.OurOrg.ShortName, nmspcMeta);

      return XmlDoc;
    }

    private static void AddTextNode(XmlElement elParent, string tagName, string text, string nmspc)
    {
      if (String.IsNullOrEmpty(text))
        return;

      XmlElement el = elParent.OwnerDocument.CreateElement(tagName, nmspc);
      el.InnerText = text;
      elParent.AppendChild(el);
    }

    #endregion

    #region Содержание (content.xml)

    const string nmspcStyle = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    const string nmspcText = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    const string nmspcTable = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    const string nmspcFo = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
    const string nmspcNumber = "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0";
    const string nmspcLoext = "urn:org:documentfoundation:names:experimental:office:xmlns:loext:1.0"; // 18.11.2016

    private static XmlDocument CreateContent(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings)
    {
      // 1. Собираем массив печатаемых столбцов и строк
      EFPDataGridViewRectArea Area = controlProvider.GetRectArea(settings.RangeMode);

      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmlDecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      XmlDoc.InsertBefore(xmlDecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("office:document-content", nmspcOffice);
      XmlDoc.AppendChild(elRoot);
      SetAttr(elRoot, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRoot, "xmlns:style", nmspcStyle, String.Empty);
      SetAttr(elRoot, "xmlns:text", nmspcText, String.Empty);
      SetAttr(elRoot, "xmlns:table", nmspcTable, String.Empty);
      SetAttr(elRoot, "xmlns:fo", nmspcFo, String.Empty);
      SetAttr(elRoot, "xmlns:number", nmspcNumber, String.Empty);
      SetAttr(elRoot, "xmlns:loext", nmspcLoext, String.Empty);
      SetAttr(elRoot, "office:version", "1.2", String.Empty);

      XmlElement elAutoStyles = XmlDoc.CreateElement("office:automatic-styles", nmspcOffice);
      elRoot.AppendChild(elAutoStyles); // будем заполнять по мере необходимости

      XmlElement elBody = XmlDoc.CreateElement("office:body", nmspcOffice);
      elRoot.AppendChild(elBody);

      XmlElement elSpreadSheet = XmlDoc.CreateElement("office:spreadsheet", nmspcOffice);
      elBody.AppendChild(elSpreadSheet);

      XmlElement elTable = XmlDoc.CreateElement("table:table", nmspcTable);
      elSpreadSheet.AppendChild(elTable);
      SetAttr(elTable, "table:name", "Таблица", nmspcTable);

      #region Объявления столбцов

      // В отличие от MS Excel, ширина столбцов задается в стилях
      for (int j = 0; j < Area.ColumnCount; j++)
      {
        EFPDataGridViewColumn Col = controlProvider.Columns[Area.ColumnIndices[j]];

        XmlElement elColStyle = XmlDoc.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elColStyle);
        SetAttr(elColStyle, "style:name", "co" + (j + 1).ToString(), nmspcStyle);
        SetAttr(elColStyle, "style:family", "table-column", nmspcStyle);

        XmlElement elColProps = XmlDoc.CreateElement("style:table-column-properties", nmspcStyle);
        elColStyle.AppendChild(elColProps);
        float w = (float)(Col.WidthPt) / 72f * 2.54f;
        SetAttr(elColProps, "style:column-width", w.ToString("0.000", DataTools.DotNumberConv) + "cm", nmspcStyle);


        XmlElement elColumn = XmlDoc.CreateElement("table:table-column", nmspcTable);
        elTable.AppendChild(elColumn);
        SetAttr(elColumn, "table:style-name", "co" + (j + 1).ToString(), nmspcTable);
      }

      #endregion

      #region Стили для строк

      // 17.07.2019
      // Объявляем стиль "ro1" для строк заголовков и
      // "ro2" для данных
      // Стили одинаковые, с автоподбором высоты

      // TODO: Автоподбор высоты почему-то не работает сразу.
      // Вот если в уже открытой книге поменять текст в строке, то будет выполнен автоматический подбор.
      // То есть, признак передается, но Calc сам высоту не вычисляет. Требуется явно задавать высоту. 
      // А я ее не знаю :(

      #region "ro1"

      XmlElement elRowStyle = XmlDoc.CreateElement("style:style", nmspcStyle);
      elAutoStyles.AppendChild(elRowStyle);
      SetAttr(elRowStyle, "style:name", "ro1", nmspcStyle);
      SetAttr(elRowStyle, "style:family", "table-row", nmspcStyle);

      XmlElement elTRP = XmlDoc.CreateElement("style:table-row-properties", nmspcStyle);
      elRowStyle.AppendChild(elTRP);
      // Вот бы знать ... SetAttr(elTRP, "style:row-height", "70mm", nmspcStyle); 
      SetAttr(elTRP, "style:use-optimal-row-height", "true", nmspcStyle);

      #endregion

      #region "ro2"

      elRowStyle = XmlDoc.CreateElement("style:style", nmspcStyle);
      elAutoStyles.AppendChild(elRowStyle);
      SetAttr(elRowStyle, "style:name", "ro2", nmspcStyle);
      SetAttr(elRowStyle, "style:family", "table-row", nmspcStyle);

      elTRP = XmlDoc.CreateElement("style:table-row-properties", nmspcStyle);
      elRowStyle.AppendChild(elTRP);
      //SetAttr(elTRP, "style:row-height", "40mm", nmspcStyle);
      SetAttr(elTRP, "style:use-optimal-row-height", "true", nmspcStyle);

      #endregion

      #endregion

      #region Заголовки столбцов

      if (settings.ShowColumnHeaders)
      {
        XmlElement elHeadStyle = XmlDoc.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elHeadStyle);
        SetAttr(elHeadStyle, "style:name", "ColHdr", nmspcStyle);
        SetAttr(elHeadStyle, "style:family", "table-cell", nmspcStyle);

        XmlElement elCellProps = XmlDoc.CreateElement("style:table-cell-properties", nmspcStyle);
        elHeadStyle.AppendChild(elCellProps);
        SetBorders(elCellProps, "fo:border", EFPDataGridViewBorderStyle.Thin, nmspcFo);
        SetAttr(elCellProps, "style:vertical-align", "middle", nmspcStyle);
        SetAttr(elCellProps, "fo:wrap-option", "wrap", nmspcFo);

        XmlElement elParProps = XmlDoc.CreateElement("style:paragraph-properties", nmspcStyle);
        elHeadStyle.AppendChild(elParProps);
        SetAttr(elParProps, "fo:text-align", "center", nmspcFo);

        EFPDataGridViewColumnHeaderArray HeaderArray = controlProvider.GetColumnHeaderArray(Area);

        for (int i = 0; i < HeaderArray.RowCount; i++)
        {
          XmlElement elHeadRow = XmlDoc.CreateElement("table:table-row", nmspcTable);
          elTable.AppendChild(elHeadRow);
          SetAttr(elHeadRow, "table:style-name", "ro1", nmspcTable); // 17.07.2019

          for (int j = 0; j < HeaderArray.ColumnCount; j++)
          {
            if (HeaderArray.RowSpan[i, j] == 0)
            {
              // не первая строка объединения
              XmlElement elCoveredCell = XmlDoc.CreateElement("table:covered-table-cell", nmspcTable);
              elHeadRow.AppendChild(elCoveredCell);

              continue;
            }

            XmlElement elCell = XmlDoc.CreateElement("table:table-cell", nmspcTable);
            elHeadRow.AppendChild(elCell);
            SetAttr(elCell, "table:style-name", "ColHdr", nmspcTable);

            // один заголовок также может содержать несколько абзацев
            if (!String.IsNullOrEmpty(HeaderArray.Text[i, j]))
            {
              SetAttr(elCell, "office:value-type", "string", nmspcOffice);

              string[] a = HeaderArray.Text[i, j].Split(DataTools.NewLineSeparators, StringSplitOptions.None);
              for (int k = 0; k < a.Length; k++)
              {
                string s = a[k];
                s = s.Replace(DataTools.SoftHyphenStr, ""); // не знаю, как записать мягкий перенос
                s = DataTools.ReplaceAny(s, EFPDataGridViewExpExcel2003.BadValueChars, ' ');

                // неразрывный пробел так и остается
                XmlElement elP = XmlDoc.CreateElement("text:p", nmspcText);
                elCell.AppendChild(elP);
                elP.InnerText = s;
              }
            }

            //attr = XmlDoc.CreateAttribute("ss:StyleID", nmspcSS);
            //attr.Value = "ColHdr";
            //elCell.Attributes.Append(attr);

            if (HeaderArray.RowSpan[i, j] > 1 || HeaderArray.ColumnSpan[i, j] > 1)
            {
              SetAttr(elCell, "table:number-columns-spanned", HeaderArray.ColumnSpan[i, j].ToString(), nmspcTable);
              SetAttr(elCell, "table:number-rows-spanned", HeaderArray.RowSpan[i, j].ToString(), nmspcTable);
            }

          } // по столбцам
        } // по строкам заголовка
      } // есть заголовки

      #endregion

      #region Основные строки

      StringBuilder sb = new StringBuilder();

      // Сюда запоминаем существующие стили ячеек
      List<string> CellStyleKeys = new List<string>();

      // Хранилище для границ ячеек
      EFPDataGridViewBorderStyle[,] BorderStyles = null;
      if (settings.UseBorders)
        BorderStyles = new EFPDataGridViewBorderStyle[Area.ColumnCount, 6];

      // Перебираем строки таблицы
      for (int i = 0; i < Area.RowCount; i++)
      {
        // Границы надо запрашивать отдельно, в режиме печати
        if (settings.UseBorders)
        {
          controlProvider.DoGetRowAttributes(Area.RowIndices[i], EFPDataGridViewAttributesReason.Print);
          for (int j = 0; j < Area.ColumnCount; j++)
          {
            int ColumnIndex = Area.ColumnIndices[j];
            EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
            BorderStyles[j, 0] = CellArgs.BottomBorder;
            BorderStyles[j, 1] = CellArgs.LeftBorder;
            BorderStyles[j, 2] = CellArgs.RightBorder;
            BorderStyles[j, 3] = CellArgs.TopBorder;
            if (CellArgs.DiagonalUpBorder == EFPDataGridViewBorderStyle.Default)
              BorderStyles[j, 4] = EFPDataGridViewBorderStyle.None;
            else
              BorderStyles[j, 4] = CellArgs.DiagonalUpBorder;
            if (CellArgs.DiagonalDownBorder == EFPDataGridViewBorderStyle.Default)
              BorderStyles[j, 5] = EFPDataGridViewBorderStyle.None;
            else
              BorderStyles[j, 5] = CellArgs.DiagonalDownBorder;
          }
        }

        controlProvider.DoGetRowAttributes(Area.RowIndices[i], EFPDataGridViewAttributesReason.View);

        XmlElement elRow = XmlDoc.CreateElement("table:table-row", nmspcTable);
        elTable.AppendChild(elRow);
        SetAttr(elRow, "table:style-name", "ro2", nmspcTable); // 17.07.2019

        for (int j = 0; j < Area.ColumnCount; j++)
        {
          int ColumnIndex = Area.ColumnIndices[j];

          XmlElement elCell = XmlDoc.CreateElement("table:table-cell", nmspcTable);
          elRow.AppendChild(elCell);

          #region Определение формата

          EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
          EFPDataGridViewExcelCellAttributes ExcelAttr = EFPDataGridView.GetExcelCellAttr(CellArgs);


          bool WordWrap = CellArgs.CellStyle.WrapMode == DataGridViewTriState.True;
          if (CellArgs.Column.GridColumn is DataGridViewTextBoxColumn)
          {
            Type ValueType = ((DataGridViewTextBoxColumn)(CellArgs.Column.GridColumn)).ValueType;
            if (ValueType == typeof(string) || ValueType == null)
              WordWrap = true;
          }

          String FormatText = String.Empty;
          if (!String.IsNullOrEmpty(CellArgs.CellStyle.Format))
            FormatText = CellArgs.CellStyle.Format;


          #endregion

          #region Поиск существующего стиля ячейки

          sb.Length = 0;
          sb.Append(FormatText);
          sb.Append("|");
          sb.Append(CellArgs.CellStyle.Alignment.ToString());
          sb.Append("|");
          sb.Append(WordWrap.ToString());
          sb.Append("|");
          sb.Append(CellArgs.IndentLevel.ToString());
          sb.Append("|");
          if (settings.UseInterior)
          {
            sb.Append(ExcelAttr.BackColor.ToString());
            sb.Append("|");
            sb.Append(ExcelAttr.ForeColor.ToString());
            sb.Append("|");
            sb.Append(ExcelAttr.Bold);
            sb.Append("|");
            sb.Append(ExcelAttr.Italic);
            sb.Append("|");
            sb.Append(ExcelAttr.Underline);
            sb.Append("|");
          }
          if (settings.UseBorders)
          {
            for (int k = 0; k < BorderStyles.GetLength(1); k++)
            {
              sb.Append(BorderStyles[j, k]);
              sb.Append("|");
            }
          }
          int StyleKeyIndex = CellStyleKeys.IndexOf(sb.ToString());

          #endregion

          if (StyleKeyIndex < 0)
          {
            #region Добавление стиля

            CellStyleKeys.Add(sb.ToString());
            StyleKeyIndex = CellStyleKeys.Count - 1;

            // Сначала добавляем формат для чисел
            string NFormatName = "N" + (StyleKeyIndex + 1).ToString();
            if (!OpenOfficeTools.ODFAddFormat(elAutoStyles, FormatText, NFormatName))
              NFormatName = null;

            XmlElement elStyle = XmlDoc.CreateElement("style:style", nmspcStyle);
            elAutoStyles.AppendChild(elStyle);
            SetAttr(elStyle, "style:name", "ce" + (StyleKeyIndex + 1).ToString(), nmspcStyle);
            SetAttr(elStyle, "style:family", "table-cell", nmspcStyle);

            if (NFormatName != null)
              SetAttr(elStyle, "style:data-style-name", NFormatName, nmspcStyle);

            XmlElement elCellProps = XmlDoc.CreateElement("style:table-cell-properties", nmspcStyle);
            elStyle.AppendChild(elCellProps);

            XmlElement elParProps = XmlDoc.CreateElement("style:paragraph-properties", nmspcStyle);
            elStyle.AppendChild(elParProps);

            if (settings.UseBorders)
            {
              if (BorderStyles[j, 0] == BorderStyles[j, 1] &&
                BorderStyles[j, 0] == BorderStyles[j, 2] &&
                BorderStyles[j, 0] == BorderStyles[j, 3] &&
                BorderStyles[j, 4] == EFPDataGridViewBorderStyle.None && BorderStyles[j, 5] == EFPDataGridViewBorderStyle.None)

                // Все границы одинаковые
                SetBorders(elCellProps, "fo:border", BorderStyles[j, 0], nmspcFo);
              else
              {
                // Границы разные
                SetBorders(elCellProps, "fo:border-bottom", BorderStyles[j, 0], nmspcFo);
                SetBorders(elCellProps, "fo:border-left", BorderStyles[j, 1], nmspcFo);
                SetBorders(elCellProps, "fo:border-right", BorderStyles[j, 2], nmspcFo);
                SetBorders(elCellProps, "fo:border-top", BorderStyles[j, 3], nmspcFo);
                if (BorderStyles[j, 4] != EFPDataGridViewBorderStyle.None || BorderStyles[j, 5] != EFPDataGridViewBorderStyle.None)
                {
                  SetBorders(elCellProps, "style:diagonal-bl-tr", BorderStyles[j, 4], nmspcStyle);
                  SetBorders(elCellProps, "style:diagonal-tl-br", BorderStyles[j, 5], nmspcStyle);
                }
              }
            }

            string ha, va;
            switch (CellArgs.CellStyle.Alignment)
            {
              case DataGridViewContentAlignment.TopLeft: ha = "start"; va = "top"; break;
              case DataGridViewContentAlignment.TopCenter: ha = "center"; va = "top"; break;
              case DataGridViewContentAlignment.TopRight: ha = "end"; va = "top"; break;
              case DataGridViewContentAlignment.MiddleLeft: ha = "start"; va = "middle"; break;
              case DataGridViewContentAlignment.MiddleCenter: ha = "center"; va = "middle"; break;
              case DataGridViewContentAlignment.MiddleRight: ha = "end"; va = "middle"; break;
              case DataGridViewContentAlignment.BottomLeft: ha = "start"; va = "bottom"; break;
              case DataGridViewContentAlignment.BottomCenter: ha = "center"; va = "bottom"; break;
              case DataGridViewContentAlignment.BottomRight: ha = "end"; va = "bottom"; break;
              default: throw new BugException("Неизвестное выравнивание");
            }

            SetAttr(elCellProps, "style:vertical-align", va, nmspcStyle);
            SetAttr(elParProps, "fo:text-align", ha, nmspcFo);
            if (WordWrap)
              SetAttr(elCellProps, "fo:wrap-option", "wrap", nmspcFo);

            if (CellArgs.IndentLevel > 0)
            {
              if (ha == "start")
                SetAttr(elParProps, "fo:margin-left", (0.3 * CellArgs.IndentLevel) + "cm", nmspcFo);
              if (ha == "end") // наверное, не будет работать
                SetAttr(elParProps, "fo:margin-right", (0.3 * CellArgs.IndentLevel) + "cm", nmspcFo);
            }


            if (settings.UseInterior)
            {
              if (ExcelAttr.Bold || ExcelAttr.Italic || ExcelAttr.Underline || (!ExcelAttr.ForeColor.IsEmpty))
              {
                XmlElement elTextProps = XmlDoc.CreateElement("style:text-properties", nmspcStyle);
                elStyle.AppendChild(elTextProps);

                if (ExcelAttr.Bold)
                  SetAttr(elTextProps, "fo:font-weight", "bold", nmspcFo);
                if (ExcelAttr.Italic)
                  SetAttr(elTextProps, "fo:font-style", "italic", nmspcFo);
                if (ExcelAttr.Underline)
                  SetAttr(elTextProps, "style:text-underline-style", "solid", nmspcStyle);
                if (!ExcelAttr.ForeColor.IsEmpty)
                  SetAttr(elTextProps, "fo:color", MyColorStr(ExcelAttr.ForeColor), nmspcFo);
              }

              if (!ExcelAttr.BackColor.IsEmpty)
                SetAttr(elCellProps, "fo:background-color", MyColorStr(ExcelAttr.BackColor), nmspcFo);

            } // UseFill

            //if (!String.IsNullOrEmpty(FormatText))
            //{
            //  XmlElement elNumberFormat = XmlDoc.CreateElement("NumberFormat", nmspcSS);
            //  elStyle.AppendChild(elNumberFormat);
            //  attr = XmlDoc.CreateAttribute("ss:Format", nmspcSS);
            //  attr.Value = FormatText;
            //  elNumberFormat.Attributes.Append(attr);
            //}

            #endregion
          }

          SetAttr(elCell, "table:style-name", "ce" + (StyleKeyIndex + 1).ToString(), nmspcTable);

          #region Значение ячейки

          if (CellArgs.ContentVisible) // 24.08.2015
          {
            object CellValue = CellArgs.FormattedValue;
            if (CellValue is DBNull)
              CellValue = null;
            if (CellValue is Image)
              CellValue = null; // 14.01.2014
            if (CellValue is Boolean)
            {
              switch (settings.BoolMode)
              {
                case EFPDataGridViewExpExcelBoolMode.Boolean:
                  break; // как есть
                case EFPDataGridViewExpExcelBoolMode.Digit:
                  CellValue = ((bool)CellValue) ? 1 : 0;
                  break;
                case EFPDataGridViewExpExcelBoolMode.Brackets:
                  CellValue = ((bool)CellValue) ? "[X]" : "[ ]";
                  break;
              }
            }
            if (CellValue != null)
            {
              #region Текстовое представление

              string s = CellValue.ToString();
              if (s.Length > 0) // 17.07.2019
              {
                string[] a = s.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
                for (int k = 0; k < a.Length; k++)
                {
                  XmlElement elP = XmlDoc.CreateElement("text:p", nmspcText);
                  elCell.AppendChild(elP);
                  s = a[k];
                  s = DataTools.ReplaceAny(s, EFPDataGridViewExpExcel2003.BadValueChars, ' ');
                  //elP.InnerText = s; // это - текст. В числах запятая на точку не заменяется
                  XmlText txt = XmlDoc.CreateTextNode(s); // 17.07.2019
                  elP.AppendChild(txt);
                }
              }

              #endregion

              #region Типизированное значение

              switch (CellValue.GetType().Name)
              {
                case "String":
                  SetAttr(elCell, "office:value-type", "string", nmspcOffice);
                  break;
                case "Boolean":
                  SetAttr(elCell, "office:value-type", "boolean", nmspcOffice);
                  SetAttr(elCell, "office:boolean-value", (bool)CellValue ? "true" : "false", nmspcOffice);
                  break;
                case "DateTime":
                  // Теоретически существует еще тип time, но реально везде используется date
                  DateTime dt = (DateTime)CellValue;
                  SetAttr(elCell, "office:value-type", "date", nmspcOffice);
                  SetAttr(elCell, "office:date-value", dt.ToString("s"), nmspcOffice);
                  break;
                default:
                  if (DataTools.IsIntegerType(CellValue.GetType()) || DataTools.IsFloatType(CellValue.GetType()))
                  {
                    SetAttr(elCell, "office:value-type", "float", nmspcOffice);
                    SetAttr(elCell, "office:value", Convert.ToString(CellValue, DataTools.DotNumberConv), nmspcOffice);
                  }
                  break;
              }

              #endregion
            }
          }

          #endregion

        } // цикл по столбцам
      } // цикл по строкам

      #endregion

      //DebugTools.DebugXml(XmlDoc, "Contents");

      return XmlDoc;
    }

    private static string MyColorStr(Color c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    private static void SetBorders(XmlElement elCellProps, string attrName, EFPDataGridViewBorderStyle borderStyle, string nmspc)
    {
      string Value;
      switch (borderStyle)
      {
        case EFPDataGridViewBorderStyle.Default:
          Value = "0.1pt solid #000000";
          break;
        case EFPDataGridViewBorderStyle.Thin:
          Value = "0.5pt solid #000000";
          break;
        case EFPDataGridViewBorderStyle.Thick:
          Value = "1pt solid #000000";
          break;
        default:
          return;
      }

      SetAttr(elCellProps, attrName, Value, nmspc);
    }

    #endregion

    #region Вспомогательные методы

    private static void SetAttr(XmlElement el, string name, string value, string nmspc)
    {
      XmlAttribute Attr;
      if (String.IsNullOrEmpty(nmspc))
        Attr = el.OwnerDocument.CreateAttribute(name);
      else
        Attr = el.OwnerDocument.CreateAttribute(name, nmspc);
      Attr.Value = value;
      el.Attributes.Append(Attr);
    }

    #endregion

    #endregion
  }
}
