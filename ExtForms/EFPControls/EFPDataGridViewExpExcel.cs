// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Models.SpreadsheetBase;
using FreeLibSet.Shell;
using FreeLibSet.Reporting;
using FreeLibSet.Forms.Reporting;

namespace FreeLibSet.Forms
{
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
      _RangeMode = EFPDataViewExpRange.All;
      _ShowColumnHeaders = true;
      _UseInterior = true;
      _UseBorders = true;
      _BoolMode = BRDataViewBoolMode.Boolean;
    }

    #endregion

    #region Параметры

    /// <summary>
    /// Диапазон ячеек для экспорта (по умолчанию - All)
    /// </summary>
    public EFPDataViewExpRange RangeMode { get { return _RangeMode; } set { _RangeMode = value; } }
    private EFPDataViewExpRange _RangeMode;

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
    public BRDataViewBoolMode BoolMode { get { return _BoolMode; } set { _BoolMode = value; } }
    private BRDataViewBoolMode _BoolMode;

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
        RangeMode = EFPDataViewExpRange.Selected;
      else
        RangeMode = EFPDataViewExpRange.All;
      ShowColumnHeaders = !cfg.GetBool("NoColumnHeaders");
      UseInterior = !cfg.GetBool("NoInterior");
      UseBorders = !cfg.GetBool("NoBorders");
      switch (cfg.GetString("BoolMode"))
      {
        case "Digit": BoolMode = BRDataViewBoolMode.Integer; break;
        case "Brackets": BoolMode = BRDataViewBoolMode.Text; break;
        default: BoolMode = BRDataViewBoolMode.Boolean; break;
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
      XmlDocument xmlDoc = CreateXml(controlProvider, settings);
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
        xmlDoc.WriteTo(wrt);
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
      EFPDataGridViewRectArea area = controlProvider.GetRectArea(settings.RangeMode);

      XmlDocument xmlDoc = new XmlDocument();

      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", settings.DebugXml ? "Windows-1251" : "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlProcessingInstruction xmlPI = xmlDoc.CreateProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
      xmlDoc.AppendChild(xmlPI);


      XmlAttribute attr;
      XmlElement elWholeDoc = xmlDoc.CreateElement("Workbook", nmspcSS);
      xmlDoc.AppendChild(elWholeDoc);
      attr = xmlDoc.CreateAttribute("xmlns");  // без namespace
      attr.Value = nmspcSS;
      elWholeDoc.Attributes.Append(attr);
      attr = xmlDoc.CreateAttribute("xmlns:o");
      attr.Value = nmspcO;
      elWholeDoc.Attributes.Append(attr);
      attr = xmlDoc.CreateAttribute("xmlns:x");
      attr.Value = nmspcX;
      elWholeDoc.Attributes.Append(attr);
      attr = xmlDoc.CreateAttribute("xmlns:ss");
      attr.Value = nmspcSS;
      elWholeDoc.Attributes.Append(attr);

      // Сводка
      XmlElement elDocumentProperties = xmlDoc.CreateElement("DocumentProperties", nmspcO);
      elWholeDoc.AppendChild(elDocumentProperties);

      BRDocumentProperties docProps = controlProvider.DocumentProperties;

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

      // Таблица стилей нужна обязательно. 
      // Первый стиль "ColHdr" используется для заголовков,
      // Остальные стили добавляются в таблицу по мере необходимости, чтобы
      // не делать для каждой ячейки отдельный стиль
      XmlElement elStyles = xmlDoc.CreateElement("Styles", nmspcSS);
      elWholeDoc.AppendChild(elStyles);
      // М.б. нужен стиль "Default"?

      XmlElement elAlign, elFont;

      // Атрибуты заголовков столбцов
      if (settings.ShowColumnHeaders)
      {
        XmlElement elStyleHead = xmlDoc.CreateElement("Style", nmspcSS);
        attr = xmlDoc.CreateAttribute("ss:ID", nmspcSS);
        attr.Value = "ColHdr";
        elStyleHead.Attributes.Append(attr);
        elStyles.AppendChild(elStyleHead);

        elAlign = xmlDoc.CreateElement("Alignment", nmspcSS);
        elStyleHead.AppendChild(elAlign);
        attr = xmlDoc.CreateAttribute("ss:Horizontal", nmspcSS);
        attr.Value = "Center";
        elAlign.Attributes.Append(attr);
        attr = xmlDoc.CreateAttribute("ss:Vertical", nmspcSS);
        attr.Value = "Center";
        elAlign.Attributes.Append(attr);
        attr = xmlDoc.CreateAttribute("ss:WrapText", nmspcSS);
        attr.Value = "1";
        elAlign.Attributes.Append(attr);

        if (settings.UseBorders)
        {
          XmlElement elBorders = xmlDoc.CreateElement("Borders", nmspcSS);
          elStyleHead.AppendChild(elBorders);

          AddBorder(elBorders, "Bottom", EFPDataGridViewBorderStyle.Thin);
          AddBorder(elBorders, "Left", EFPDataGridViewBorderStyle.Thin);
          AddBorder(elBorders, "Right", EFPDataGridViewBorderStyle.Thin);
          AddBorder(elBorders, "Top", EFPDataGridViewBorderStyle.Thin);
        }

        elFont = xmlDoc.CreateElement("Font", nmspcSS);
        elStyleHead.AppendChild(elFont);
        attr = xmlDoc.CreateAttribute("ss:Bold", nmspcSS);
        attr.Value = "1";
        elFont.Attributes.Append(attr);
      }


      // Один лист в книге
      XmlElement elWorksheet = xmlDoc.CreateElement("Worksheet", nmspcSS);
      elWholeDoc.AppendChild(elWorksheet);
      attr = xmlDoc.CreateAttribute("ss:Name", nmspcSS);
      attr.Value = "Таблица";
      elWorksheet.Attributes.Append(attr);

      // На листе одна таблица
      XmlElement elTable = xmlDoc.CreateElement("Table", nmspcSS);
      elWorksheet.AppendChild(elTable);

      // Размеры столбцов
      for (int j = 0; j < area.ColumnCount; j++)
      {
        XmlElement elColumn = xmlDoc.CreateElement("Column", nmspcSS);
        attr = xmlDoc.CreateAttribute("ss:Width", nmspcSS);
        EFPDataGridViewColumn Col = controlProvider.Columns[area.ColumnIndices[j]];
        attr.Value = Col.WidthPt.ToString(); // в пунктах
        elColumn.Attributes.Append(attr);
        elTable.AppendChild(elColumn);
      }

      // Заголовки столбцов
      if (settings.ShowColumnHeaders)
      {
        BRColumnHeaderArray headerArray = controlProvider.GetColumnHeaderArray(area);

        for (int i = 0; i < headerArray.RowCount; i++)
        {
          XmlElement elHeadRow = xmlDoc.CreateElement("Row", nmspcSS);
          elTable.AppendChild(elHeadRow);
          for (int j = 0; j < headerArray.ColumnCount; j++)
          {
            if (headerArray.RowSpan[i, j] == 0)
              continue; // не первая строка объединения

            XmlElement elCell = xmlDoc.CreateElement("Cell", nmspcSS);
            elHeadRow.AppendChild(elCell);

            attr = xmlDoc.CreateAttribute("ss:StyleID", nmspcSS);
            attr.Value = "ColHdr";
            elCell.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("ss:Index", nmspcSS);
            attr.Value = (j + 1).ToString();
            elCell.Attributes.Append(attr);

            if (headerArray.RowSpan[i, j] > 1)
            {
              attr = xmlDoc.CreateAttribute("ss:MergeDown", nmspcSS);
              attr.Value = (headerArray.RowSpan[i, j] - 1).ToString();
              elCell.Attributes.Append(attr);
            }

            if (headerArray.ColumnSpan[i, j] > 1)
            {
              attr = xmlDoc.CreateAttribute("ss:MergeAcross", nmspcSS);
              attr.Value = (headerArray.ColumnSpan[i, j] - 1).ToString();
              elCell.Attributes.Append(attr);
            }

            string s = headerArray.Text[i, j];
            if (!String.IsNullOrEmpty(s))
            {
              s = s.Replace("\r\n", "\n"); // только символ 0x0A
              s = s.Replace(DataTools.SoftHyphenStr, ""); // не знаю, как записать мягкий перенос
              // неразрывный пробел так и остается
              s = DataTools.ReplaceAny(s, BadValueChars, ' '); // непечатные символы

              XmlElement elData = xmlDoc.CreateElement("Data", nmspcSS);
              elCell.AppendChild(elData);
              elData.InnerText = s;

              attr = xmlDoc.CreateAttribute("ss:Type", nmspcSS);
              attr.Value = "String";
              elData.Attributes.Append(attr);
            }
          }
        }
      }

      StringBuilder sb = new StringBuilder();

      // Сюда запоминаем существующие стили ячеек
      List<string> cellStyleKeys = new List<string>();

      // Хранилище для границ ячеек
      EFPDataGridViewBorderStyle[,] borderStyles = null;
      if (settings.UseBorders)
        borderStyles = new EFPDataGridViewBorderStyle[area.ColumnCount, 6];

      // Перебираем строки таблицы
      for (int i = 0; i < area.RowCount; i++)
      {
        // Границы надо запрашивать отдельно, в режиме печати
        if (settings.UseBorders)
        {
          controlProvider.DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.Print);
          for (int j = 0; j < area.ColumnCount; j++)
          {
            int ColumnIndex = area.ColumnIndices[j];
            EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
            borderStyles[j, 0] = CellArgs.BottomBorder;
            borderStyles[j, 1] = CellArgs.LeftBorder;
            borderStyles[j, 2] = CellArgs.RightBorder;
            borderStyles[j, 3] = CellArgs.TopBorder;
            if (CellArgs.DiagonalUpBorder == EFPDataGridViewBorderStyle.Default)
              borderStyles[j, 4] = EFPDataGridViewBorderStyle.None;
            else
              borderStyles[j, 4] = CellArgs.DiagonalUpBorder;
            if (CellArgs.DiagonalDownBorder == EFPDataGridViewBorderStyle.Default)
              borderStyles[j, 5] = EFPDataGridViewBorderStyle.None;
            else
              borderStyles[j, 5] = CellArgs.DiagonalDownBorder;
          }
        }

        controlProvider.DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.View);

        XmlElement elRow = xmlDoc.CreateElement("Row", nmspcSS);
        elTable.AppendChild(elRow);

        for (int j = 0; j < area.ColumnCount; j++)
        {
          int ColumnIndex = area.ColumnIndices[j];
          EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
          EFPDataGridViewExcelCellAttributes ExcelAttr = EFPDataGridView.GetExcelCellAttr(CellArgs);

          XmlElement elCell = xmlDoc.CreateElement("Cell", nmspcSS);
          elRow.AppendChild(elCell);


          string valueText, typeText, formatText;
          GetCellValue(CellArgs, out valueText, out typeText, out formatText, settings);

          bool wordWrap = CellArgs.CellStyle.WrapMode == DataGridViewTriState.True;
          if (CellArgs.Column.GridColumn is DataGridViewTextBoxColumn)
          {
            Type valueType = ((DataGridViewTextBoxColumn)(CellArgs.Column.GridColumn)).ValueType;
            if (valueType == typeof(string) || valueType == null)
              wordWrap = true;
          }

          sb.Length = 0;
          sb.Append(formatText);
          sb.Append("|");
          sb.Append(CellArgs.CellStyle.Alignment.ToString());
          sb.Append("|");
          sb.Append(wordWrap.ToString());
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
            for (int k = 0; k < borderStyles.GetLength(1); k++)
            {
              sb.Append(borderStyles[j, k]);
              sb.Append("|");
            }
          }
          int styleKeyIndex = cellStyleKeys.IndexOf(sb.ToString());
          if (styleKeyIndex < 0)
          {
            // Требуется добавить стиль
            cellStyleKeys.Add(sb.ToString());
            styleKeyIndex = cellStyleKeys.Count - 1;

            XmlElement elStyle = xmlDoc.CreateElement("Style", nmspcSS);
            attr = xmlDoc.CreateAttribute("ss:ID", nmspcSS);
            attr.Value = "s" + (styleKeyIndex + 100).ToString();
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

            elAlign = xmlDoc.CreateElement("Alignment", nmspcSS);
            elStyle.AppendChild(elAlign);
            attr = xmlDoc.CreateAttribute("ss:Horizontal", nmspcSS);
            attr.Value = ha;
            elAlign.Attributes.Append(attr);
            attr = xmlDoc.CreateAttribute("ss:Vertical", nmspcSS);
            attr.Value = va;
            elAlign.Attributes.Append(attr);
            if (wordWrap)
            {
              attr = xmlDoc.CreateAttribute("ss:WrapText", nmspcSS);
              attr.Value = "1";
              elAlign.Attributes.Append(attr);
            }
            if (CellArgs.IndentLevel > 0)
            {
              attr = xmlDoc.CreateAttribute("ss:Indent", nmspcSS);
              attr.Value = CellArgs.IndentLevel.ToString();
              elAlign.Attributes.Append(attr);
            }

            if (settings.UseBorders)
            {
              XmlElement elBorders = xmlDoc.CreateElement("Borders", nmspcSS);
              elStyle.AppendChild(elBorders);

              AddBorder(elBorders, "Bottom", borderStyles[j, 0]);
              AddBorder(elBorders, "Left", borderStyles[j, 1]);
              AddBorder(elBorders, "Right", borderStyles[j, 2]);
              AddBorder(elBorders, "Top", borderStyles[j, 3]);
              AddBorder(elBorders, "DiagonalRight", borderStyles[j, 4]);
              AddBorder(elBorders, "DiagonalLeft", borderStyles[j, 5]);
            }

            if (settings.UseInterior)
            {
              elFont = xmlDoc.CreateElement("Font", nmspcSS);
              elStyle.AppendChild(elFont);

              if (ExcelAttr.Bold)
              {
                attr = xmlDoc.CreateAttribute("ss:Bold", nmspcSS);
                attr.Value = "1";
                elFont.Attributes.Append(attr);
              }
              if (!ExcelAttr.ForeColor.IsEmpty)
              {
                attr = xmlDoc.CreateAttribute("ss:Color", nmspcSS);
                attr.Value = MyColorStr(ExcelAttr.ForeColor);
                elFont.Attributes.Append(attr);
              }
              if (ExcelAttr.Italic)
              {
                attr = xmlDoc.CreateAttribute("ss:Italic", nmspcSS);
                attr.Value = "1";
                elFont.Attributes.Append(attr);
              }
              if (ExcelAttr.Underline)
              {
                attr = xmlDoc.CreateAttribute("ss:Underline", nmspcSS);
                attr.Value = "Single";
                elFont.Attributes.Append(attr);
              }

              if (!ExcelAttr.BackColor.IsEmpty)
              {
                XmlElement elInterior = xmlDoc.CreateElement("Interior", nmspcSS);
                elStyle.AppendChild(elInterior);
                attr = xmlDoc.CreateAttribute("ss:Color", nmspcSS);
                attr.Value = MyColorStr(ExcelAttr.BackColor);
                elInterior.Attributes.Append(attr);
                attr = xmlDoc.CreateAttribute("ss:Pattern", nmspcSS);
                attr.Value = "Solid";
                elInterior.Attributes.Append(attr);
              }
            } // UseFill

            if (!String.IsNullOrEmpty(formatText))
            {
              XmlElement elNumberFormat = xmlDoc.CreateElement("NumberFormat", nmspcSS);
              elStyle.AppendChild(elNumberFormat);
              attr = xmlDoc.CreateAttribute("ss:Format", nmspcSS);
              attr.Value = formatText;
              elNumberFormat.Attributes.Append(attr);
            }
          }

          attr = xmlDoc.CreateAttribute("ss:StyleID", nmspcSS);
          attr.Value = "s" + (styleKeyIndex + 100).ToString();
          elCell.Attributes.Append(attr);

          if (typeText != null)
          {
            XmlElement elData = xmlDoc.CreateElement("Data", nmspcSS);
            elCell.AppendChild(elData);
            elData.InnerText = valueText;

            attr = xmlDoc.CreateAttribute("ss:Type", nmspcSS);
            attr.Value = typeText;
            elData.Attributes.Append(attr);
          }
        }
      }
      return xmlDoc;
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
            case BRDataViewBoolMode.Boolean:
              valueText = (bool)v ? "1" : "0";
              typeText = "Boolean";
              break;
            case BRDataViewBoolMode.Integer:
              valueText = (bool)v ? "1" : "0";
              typeText = "Number";
              break;
            case BRDataViewBoolMode.Text:
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
            valueText = Convert.ToString(v, StdConvert.NumberFormat);
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
      ZipFileCreator zf = new ZipFileCreator(fileName);

      #endregion

      #region Заголовок ([Content_Types].xml)

      XmlDocument xmlDocCT = CreateContentTypes();
      zf.AddXmlFile("[Content_Types].xml", xmlDocCT);

      #endregion

      #region Связи (_rels/.rels)

      XmlDocument xmlDocRels1 = CreateRels1();
      zf.AddXmlFile("_rels/.rels", xmlDocRels1);

      XmlDocument xmlDocRels2 = CreateRels2();
      zf.AddXmlFile("xl/_rels/workbook.xml.rels", xmlDocRels2);

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

      XmlDocument xmlDocWorkbook = CreateWorkbook();
      zf.AddXmlFile("xl/workbook.xml", xmlDocWorkbook);

      #endregion

      #region Worksheet

      List<string> sharedStrings;
      int sharedStringCount;
      StyleTable styles = new StyleTable(controlProvider, settings);
      XmlDocument xmlDocSheet = CreateSheet(controlProvider, settings, out sharedStrings, out sharedStringCount, styles);
      zf.AddXmlFile("xl/worksheets/sheet1.xml", xmlDocSheet);

      XmlDocument xmlDocStyles = styles.FinishDoc();
      zf.AddXmlFile("xl/styles.xml", xmlDocStyles);

      #endregion

      #region Список строк

      XmlDocument xmlDocSharedStrings = CreateSharedStrings(sharedStrings, sharedStringCount);
      zf.AddXmlFile("xl/sharedStrings.xml", xmlDocSharedStrings);

      #endregion

      #region Конец

      zf.Close();

      #endregion
    }

    #endregion

    #region Вспомогательные файлы для OpenXML

    private const string nmspcContentTypes = "http://schemas.openxmlformats.org/package/2006/content-types";

    private static XmlDocument CreateContentTypes()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Types", nmspcContentTypes);
      SetAttr(elRoot, "xmlns", nmspcContentTypes, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elDefault = xmlDoc.CreateElement("Default", nmspcContentTypes);
      SetAttr(elDefault, "Extension", "rels", String.Empty);
      SetAttr(elDefault, "ContentType", "application/vnd.openxmlformats-package.relationships+xml", String.Empty);
      elRoot.AppendChild(elDefault);

      elDefault = xmlDoc.CreateElement("Default", nmspcContentTypes);
      SetAttr(elDefault, "Extension", "xml", String.Empty);
      SetAttr(elDefault, "ContentType", "application/xml", String.Empty);
      elRoot.AppendChild(elDefault);

      XmlElement elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/workbook.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/worksheets/sheet1.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/sharedStrings.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/styles.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      return xmlDoc;
    }

    private const string nmspcPackageRels = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string nmspcOfficeRels = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static XmlDocument CreateRels1()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
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

      return xmlDoc;
    }

    private static XmlDocument CreateRels2()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId1", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", String.Empty);
      SetAttr(elRL, "Target", "worksheets/sheet1.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId2", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings", String.Empty);
      SetAttr(elRL, "Target", "sharedStrings.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId3", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", String.Empty);
      SetAttr(elRL, "Target", "styles.xml", String.Empty);
      elRoot.AppendChild(elRL);

      return xmlDoc;
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
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("workbook", nmspcSpreadsheet);
      SetAttr(elRoot, "xmlns", nmspcSpreadsheet, String.Empty);
      SetAttr(elRoot, "xmlns:r", nmspcOfficeRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elSheets = xmlDoc.CreateElement("sheets", nmspcSpreadsheet);
      elRoot.AppendChild(elSheets);

      XmlElement elSheet = xmlDoc.CreateElement("sheet", nmspcSpreadsheet);
      SetAttr(elSheet, "name", "1", String.Empty);
      SetAttr(elSheet, "sheetId", "1", String.Empty);
      SetAttr(elSheet, "r:id", "rId1", nmspcOfficeRels);
      elSheets.AppendChild(elSheet);

      return xmlDoc;
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

        int numFmtId = 0; // общий формат
        if (!String.IsNullOrEmpty(numberFormat))
        {
          if (!_NumFmtDict.TryGetValue(numberFormat, out numFmtId))
          {
            if (_elNumFmtRoot == null)
            {
              _elNumFmtRoot = _XmlDoc.CreateElement("numFmts", nmspcSpreadsheet);
              _elRoot.AppendChild(_elNumFmtRoot);
            }

            XmlElement elNumFmt = _XmlDoc.CreateElement("numFmt", nmspcSpreadsheet);
            numFmtId = _NumFmtDict.Count + 1001; // Начинаем нумерацию с 1001
            SetAttr(elNumFmt, "numFmtId", numFmtId.ToString(), String.Empty);
            SetAttr(elNumFmt, "formatCode", numberFormat, String.Empty);
            _elNumFmtRoot.AppendChild(elNumFmt);

            _NumFmtDict.Add(numberFormat, numFmtId);
          }
        }

        #endregion

        #region Шрифт

        if (!_Settings.UseInterior)
          foreColor = Color.Empty;

        string fontKey = isBold.ToString() + "|" + isItalic.ToString() + "|" + isUnderline.ToString() + "|" + foreColor.ToString();
        int fontIdx;
        if (!_FontDict.TryGetValue(fontKey, out fontIdx))
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

          fontIdx = _FontDict.Count;
          _FontDict.Add(fontKey, fontIdx);
        }

        #endregion

        #region Заполнение

        if (!_Settings.UseInterior)
          backColor = Color.Empty;

        int fillIdx = GetFillIdx(backColor);

        #endregion

        #region Границы

        string borderKey = leftBorder.ToString() + "|" + topBorder.ToString() + "|" + rightBorder.ToString() + "|" + bottomBorder.ToString()+
          "|" + diagonalUpBorder.ToString()+"|" + diagonalDownBorder.ToString();
        int borderIdx;
        if (!_BordersDict.TryGetValue(borderKey, out borderIdx))
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

          borderIdx = _BordersDict.Count;
          _BordersDict.Add(borderKey, borderIdx);
        }

        #endregion

        #region Стиль ячейки

        string styleKey = numFmtId.ToString() + "|" + fontIdx.ToString() + "|" + fillIdx.ToString() + "|" + borderIdx.ToString() + "|" + align.ToString() + "|" + offset.ToString() + "|" + wordWrap.ToString();
        int styleIdx;
        if (!_CellXfsDict.TryGetValue(styleKey, out styleIdx))
        {
          if (_elCellXfsRoot == null)
          {
            _elCellXfsRoot = _XmlDoc.CreateElement("cellXfs", nmspcSpreadsheet);
            _elRoot.AppendChild(_elCellXfsRoot);
          }

          XmlElement elXf = _XmlDoc.CreateElement("xf", nmspcSpreadsheet);

          if (numFmtId > 0)
          {
            SetAttr(elXf, "numFmtId", numFmtId.ToString(), String.Empty);
            SetAttr(elXf, "applyNumberFormat", "1", String.Empty);
          }

          if (fontIdx > 0)
          {
            SetAttr(elXf, "fontId", fontIdx.ToString(), String.Empty);
            SetAttr(elXf, "applyFont", "1", String.Empty);
          }

          if (!backColor.IsEmpty)
          {
            SetAttr(elXf, "fillId", fillIdx.ToString(), String.Empty);
            SetAttr(elXf, "applyFill", "1", String.Empty);
          }

          if (borderIdx > 0)
          {
            SetAttr(elXf, "borderId", borderIdx.ToString(), String.Empty);
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

          styleIdx = _CellXfsDict.Count;
          _CellXfsDict.Add(styleKey, styleIdx);
        }

        #endregion

        return styleIdx;
      }

      private int GetFillIdx(Color backColor)
      {
        string fillKey = backColor.ToString();
        int fillIdx;
        if (!_FillDict.TryGetValue(fillKey, out fillIdx))
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

          fillIdx = _FillDict.Count;
          _FillDict.Add(fillKey, fillIdx);
        }
        return fillIdx;
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

      EFPDataGridViewRectArea area = controlProvider.GetRectArea(settings.RangeMode);

      #endregion

      #region Таблица разделяемых строк

      sharedStrings = new List<string>();
      Dictionary<string, int> sharedStringIndices = new Dictionary<string, int>(); // индексы строк в массиве SharedStrings
      sharedStringCount = 0;

      #endregion

      #region Начало документа sheet1.xml

      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elSheetRoot = xmlDoc.CreateElement("worksheet", nmspcSpreadsheet);
      SetAttr(elSheetRoot, "xmlns", nmspcSpreadsheet, String.Empty);
      SetAttr(elSheetRoot, "xmlns:r", nmspcOfficeRels, String.Empty);
      xmlDoc.AppendChild(elSheetRoot);

      #endregion

      #region Размеры столбцов

      XmlElement elCols = xmlDoc.CreateElement("cols", nmspcSpreadsheet);
      elSheetRoot.AppendChild(elCols);

      for (int j = 0; j < area.ColumnCount; j++)
      {
        EFPDataGridViewColumn column = controlProvider.Columns[area.ColumnIndices[j]];
        XmlElement elCol = xmlDoc.CreateElement("col", nmspcSpreadsheet);
        SetAttr(elCol, "min", (j + 1).ToString(), String.Empty);
        SetAttr(elCol, "max", (j + 1).ToString(), String.Empty);
        SetAttr(elCol, "width", column.TextWidth.ToString("0.##", StdConvert.NumberFormat), String.Empty);
        elCols.AppendChild(elCol);
      }

      #endregion

      /*int DefaultStyleIdx = */styles.GetStyle(false, false, false, Color.Empty, Color.Empty,
        EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None,
        EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None,
        DataGridViewContentAlignment.MiddleCenter, 0, false, String.Empty); // основной шрифт

      #region Заголовки столбцов

      XmlElement elSheetData = xmlDoc.CreateElement("sheetData", nmspcSpreadsheet);
      elSheetRoot.AppendChild(elSheetData);

      XmlElement elMergeCells = xmlDoc.CreateElement("mergeCells", nmspcSpreadsheet);
      // Потом добавим elSheetRoot.AppendChild(elMergeCells);

      int rowCount = 0; // счетчик строк

      // Заголовки столбцов
      if (settings.ShowColumnHeaders)
      {
        int headerStyleIdx = styles.GetStyle(true, false, false, Color.Empty, Color.Empty,
          EFPDataGridViewBorderStyle.Thin, EFPDataGridViewBorderStyle.Thin, EFPDataGridViewBorderStyle.Thin, EFPDataGridViewBorderStyle.Thin,
          EFPDataGridViewBorderStyle.None, EFPDataGridViewBorderStyle.None,
          DataGridViewContentAlignment.MiddleCenter, 0, true, String.Empty);

        BRColumnHeaderArray headerArray = controlProvider.GetColumnHeaderArray(area);

        for (int i = 0; i < headerArray.RowCount; i++)
        {
          rowCount++;

          XmlElement elRow = xmlDoc.CreateElement("row", nmspcSpreadsheet);
          SetAttr(elRow, "r", rowCount.ToString(), String.Empty);
          elSheetData.AppendChild(elRow);

          for (int j = 0; j < headerArray.ColumnCount; j++)
          {
            if (headerArray.RowSpan[i, j] == 0)
              continue; // не первая строка объединения

            XmlElement elC = xmlDoc.CreateElement("c", nmspcSpreadsheet);

            string cellAddr1 = SpreadsheetTools.GetColumnName(j + 1) + rowCount.ToString();
            SetAttr(elC, "r", cellAddr1, String.Empty);
            SetAttr(elC, "s", headerStyleIdx.ToString(), String.Empty);
            elRow.AppendChild(elC);
            string s = headerArray.Text[i, j];
            WriteCellString(elC, s, sharedStrings, sharedStringIndices, ref sharedStringCount);

            if (headerArray.RowSpan[i, j] > 1 || headerArray.ColumnSpan[i, j] > 1)
            {
              string cellAddr2 = SpreadsheetTools.GetColumnName(j + headerArray.ColumnSpan[i, j]) + (rowCount + headerArray.RowSpan[i, j] - 1).ToString();
              XmlElement elMergeCell = xmlDoc.CreateElement("mergeCell", nmspcSpreadsheet);
              SetAttr(elMergeCell, "ref", cellAddr1 + ":" + cellAddr2, String.Empty);
              elMergeCells.AppendChild(elMergeCell);
            }
          }
        }
      }

      #endregion

      #region Цикл по строкам

      // Перебираем строки таблицы
      for (int i = 0; i < area.RowCount; i++)
      {
        rowCount++;

        XmlElement elRow = xmlDoc.CreateElement("row", nmspcSpreadsheet);
        SetAttr(elRow, "r", rowCount.ToString(), String.Empty);
        elSheetData.AppendChild(elRow);

        controlProvider.DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.View);

        // Перебираем столбцы
        for (int j = 0; j < area.ColumnCount; j++)
        {
          int columnIndex = area.ColumnIndices[j];

          EFPDataGridViewCellAttributesEventArgs cellArgs = controlProvider.DoGetCellAttributes(columnIndex);
          EFPDataGridViewExcelCellAttributes excelAttr = EFPDataGridView.GetExcelCellAttr(cellArgs);

          XmlElement elC = xmlDoc.CreateElement("c", nmspcSpreadsheet);
          SetAttr(elC, "r", SpreadsheetTools.GetColumnName(j + 1) + rowCount.ToString(), String.Empty);
          int styleIdx = styles.GetStyle(excelAttr.Bold, excelAttr.Italic, excelAttr.Underline,
            excelAttr.BackColor, excelAttr.ForeColor,
            cellArgs.LeftBorder, cellArgs.TopBorder, cellArgs.RightBorder, cellArgs.BottomBorder,
            cellArgs.DiagonalUpBorder, cellArgs.DiagonalDownBorder,
            cellArgs.CellStyle.Alignment, cellArgs.IndentLevel, cellArgs.CellStyle.WrapMode == DataGridViewTriState.True, cellArgs.CellStyle.Format);
          SetAttr(elC, "s", styleIdx.ToString(), String.Empty);
          elRow.AppendChild(elC);

          string valueText, typeText, formatText;
          GetCellValue(cellArgs, out valueText, out typeText, out formatText, settings);

          if (!String.IsNullOrEmpty(typeText))
          {
            if (typeText == "s")
              WriteCellString(elC, valueText, sharedStrings, sharedStringIndices, ref sharedStringCount);
            else
            {
              if (!String.IsNullOrEmpty(typeText))
                SetAttr(elC, "t", typeText, String.Empty);

              XmlElement elV = elC.OwnerDocument.CreateElement("v", nmspcSpreadsheet);
              elV.InnerText = valueText;
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

      return xmlDoc;
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
            case BRDataViewBoolMode.Boolean:
              valueText = (bool)v ? "1" : "0";
              typeText = "b";
              break;
            case BRDataViewBoolMode.Integer:
              valueText = (bool)v ? "1" : "0";
              typeText = "n";
              break;
            case BRDataViewBoolMode.Text:
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
            valueText = Convert.ToString(v2, StdConvert.NumberFormat);
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
            valueText = Convert.ToString(v, StdConvert.NumberFormat);
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
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("sst", nmspcSpreadsheet);
      SetAttr(elRoot, "count", sharedStringCount.ToString(), String.Empty);
      SetAttr(elRoot, "uniqueCount", sharedStrings.Count.ToString(), String.Empty);
      SetAttr(elRoot, "xmlns", nmspcSpreadsheet, String.Empty);
      xmlDoc.AppendChild(elRoot);

      for (int i = 0; i < sharedStrings.Count; i++)
      {
        XmlElement elSI = xmlDoc.CreateElement("si", nmspcSpreadsheet);
        elRoot.AppendChild(elSI);
        //XmlElement elR = XmlDoc.CreateElement("r", nmspcSpreadsheet);
        //elSI.AppendChild(elR);
        XmlElement elT = xmlDoc.CreateElement("t", nmspcSpreadsheet);
        elT.InnerText = sharedStrings[i];
        elSI.AppendChild(elT);
      }
      return xmlDoc;
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
      ZipFileCreator zf = new ZipFileCreator(fileName);
      zf.AddMimeType("application/vnd.oasis.opendocument.spreadsheet");

      #endregion

      #region Манифест (manifest.xml)

      XmlDocument xmlDoc = CreateManifest();
      zf.AddXmlFile("META-INF/manifest.xml", xmlDoc);

      #endregion

      #region Метаданные

      xmlDoc = CreateMeta(controlProvider/*, settings*/);
      zf.AddXmlFile("meta.xml", xmlDoc);

      #endregion

      #region Основной документ

      xmlDoc = CreateContent(controlProvider, settings);
      zf.AddXmlFile("content.xml", xmlDoc);

      #endregion

      #region Конец

      zf.Close();

      //ZF.DebugOutFiles();

      #endregion
    }

    #endregion

    #region Манифест

    private static XmlDocument CreateManifest()
    {
      const string nmspcManifest = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";

      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDoc.InsertBefore(xmlDecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("manifest:manifest", nmspcManifest);
      xmlDoc.AppendChild(elRoot);

      SetAttr(elRoot, "xmlns:manifest", nmspcManifest, String.Empty);
      SetAttr(elRoot, "manifest:version", "1.2", nmspcManifest);

      XmlElement elFile;

      elFile = xmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "application/vnd.oasis.opendocument.spreadsheet", nmspcManifest);
      SetAttr(elFile, "manifest:version", "1.2", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "/", nmspcManifest);

      elFile = xmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "text/xml", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "meta.xml", nmspcManifest);

      elFile = xmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "text/xml", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "content.xml", nmspcManifest);

      return xmlDoc;
    }

    #endregion

    #region Метаданные (meta.xml)

    const string nmspcOffice = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    const string nmspcMeta = "urn:oasis:names:tc:opendocument:xmlns:meta:1.0";
    const string nmspcDc = "http://purl.org/dc/elements/1.1/";

    private static XmlDocument CreateMeta(EFPDataGridView controlProvider/*, EFPDataGridViewExpExcelSettings settings*/)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDoc.InsertBefore(xmlDecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("office:document-meta", nmspcOffice);
      xmlDoc.AppendChild(elRoot);
      SetAttr(elRoot, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRoot, "xmlns:meta", nmspcMeta, String.Empty);
      SetAttr(elRoot, "xmlns:dc", nmspcDc, String.Empty);

      XmlElement elMeta = xmlDoc.CreateElement("office:meta", nmspcOffice);
      elRoot.AppendChild(elMeta);

      BRDocumentProperties docProps = controlProvider.DocumentProperties;

      // Требуется соответствие с определением useragent
      // в протоколе HTTP, описанного в разделе 14.43 [RFC2616]
      //AddTextNode(elMeta, "meta:generator", "Бухгалтерия ИП Агеев А.В., v." +
      //  AccDepDataTools.AccDepModuleVersion, nmspcMeta);
      if (!String.IsNullOrEmpty(docProps.Author))
      {
        AddTextNode(elMeta, "meta:initial-creator", docProps.Author, nmspcMeta);
        AddTextNode(elMeta, "meta:creator", docProps.Author, nmspcMeta);
      }
      AddTextNode(elMeta, "meta:creation-date", DateTime.Now.ToString("s"), nmspcMeta);

      // Некуда засунуть название организации
      //if (AccDepClientExec.OurOrgId != 0)
      //  AddTextNode(elMeta, "meta:company", AccDepClientExec.OurOrg.ShortName, nmspcMeta);

      return xmlDoc;
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
      EFPDataGridViewRectArea area = controlProvider.GetRectArea(settings.RangeMode);

      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDoc.InsertBefore(xmlDecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("office:document-content", nmspcOffice);
      xmlDoc.AppendChild(elRoot);
      SetAttr(elRoot, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRoot, "xmlns:style", nmspcStyle, String.Empty);
      SetAttr(elRoot, "xmlns:text", nmspcText, String.Empty);
      SetAttr(elRoot, "xmlns:table", nmspcTable, String.Empty);
      SetAttr(elRoot, "xmlns:fo", nmspcFo, String.Empty);
      SetAttr(elRoot, "xmlns:number", nmspcNumber, String.Empty);
      SetAttr(elRoot, "xmlns:loext", nmspcLoext, String.Empty);
      SetAttr(elRoot, "office:version", "1.2", String.Empty);

      XmlElement elAutoStyles = xmlDoc.CreateElement("office:automatic-styles", nmspcOffice);
      elRoot.AppendChild(elAutoStyles); // будем заполнять по мере необходимости

      XmlElement elBody = xmlDoc.CreateElement("office:body", nmspcOffice);
      elRoot.AppendChild(elBody);

      XmlElement elSpreadSheet = xmlDoc.CreateElement("office:spreadsheet", nmspcOffice);
      elBody.AppendChild(elSpreadSheet);

      XmlElement elTable = xmlDoc.CreateElement("table:table", nmspcTable);
      elSpreadSheet.AppendChild(elTable);
      SetAttr(elTable, "table:name", "Таблица", nmspcTable);

      #region Объявления столбцов

      // В отличие от MS Excel, ширина столбцов задается в стилях
      for (int j = 0; j < area.ColumnCount; j++)
      {
        EFPDataGridViewColumn col = controlProvider.Columns[area.ColumnIndices[j]];

        XmlElement elColStyle = xmlDoc.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elColStyle);
        SetAttr(elColStyle, "style:name", "co" + (j + 1).ToString(), nmspcStyle);
        SetAttr(elColStyle, "style:family", "table-column", nmspcStyle);

        XmlElement elColProps = xmlDoc.CreateElement("style:table-column-properties", nmspcStyle);
        elColStyle.AppendChild(elColProps);
        float w = (float)(col.WidthPt) / 72f * 2.54f;
        SetAttr(elColProps, "style:column-width", w.ToString("0.000", StdConvert.NumberFormat) + "cm", nmspcStyle);


        XmlElement elColumn = xmlDoc.CreateElement("table:table-column", nmspcTable);
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

      XmlElement elRowStyle = xmlDoc.CreateElement("style:style", nmspcStyle);
      elAutoStyles.AppendChild(elRowStyle);
      SetAttr(elRowStyle, "style:name", "ro1", nmspcStyle);
      SetAttr(elRowStyle, "style:family", "table-row", nmspcStyle);

      XmlElement elTRP = xmlDoc.CreateElement("style:table-row-properties", nmspcStyle);
      elRowStyle.AppendChild(elTRP);
      // Вот бы знать ... SetAttr(elTRP, "style:row-height", "70mm", nmspcStyle); 
      SetAttr(elTRP, "style:use-optimal-row-height", "true", nmspcStyle);

      #endregion

      #region "ro2"

      elRowStyle = xmlDoc.CreateElement("style:style", nmspcStyle);
      elAutoStyles.AppendChild(elRowStyle);
      SetAttr(elRowStyle, "style:name", "ro2", nmspcStyle);
      SetAttr(elRowStyle, "style:family", "table-row", nmspcStyle);

      elTRP = xmlDoc.CreateElement("style:table-row-properties", nmspcStyle);
      elRowStyle.AppendChild(elTRP);
      //SetAttr(elTRP, "style:row-height", "40mm", nmspcStyle);
      SetAttr(elTRP, "style:use-optimal-row-height", "true", nmspcStyle);

      #endregion

      #endregion

      #region Заголовки столбцов

      if (settings.ShowColumnHeaders)
      {
        XmlElement elHeadStyle = xmlDoc.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elHeadStyle);
        SetAttr(elHeadStyle, "style:name", "ColHdr", nmspcStyle);
        SetAttr(elHeadStyle, "style:family", "table-cell", nmspcStyle);

        XmlElement elCellProps = xmlDoc.CreateElement("style:table-cell-properties", nmspcStyle);
        elHeadStyle.AppendChild(elCellProps);
        SetBorders(elCellProps, "fo:border", EFPDataGridViewBorderStyle.Thin, nmspcFo);
        SetAttr(elCellProps, "style:vertical-align", "middle", nmspcStyle);
        SetAttr(elCellProps, "fo:wrap-option", "wrap", nmspcFo);

        XmlElement elParProps = xmlDoc.CreateElement("style:paragraph-properties", nmspcStyle);
        elHeadStyle.AppendChild(elParProps);
        SetAttr(elParProps, "fo:text-align", "center", nmspcFo);

        BRColumnHeaderArray headerArray = controlProvider.GetColumnHeaderArray(area);

        for (int i = 0; i < headerArray.RowCount; i++)
        {
          XmlElement elHeadRow = xmlDoc.CreateElement("table:table-row", nmspcTable);
          elTable.AppendChild(elHeadRow);
          SetAttr(elHeadRow, "table:style-name", "ro1", nmspcTable); // 17.07.2019

          for (int j = 0; j < headerArray.ColumnCount; j++)
          {
            if (headerArray.RowSpan[i, j] == 0)
            {
              // не первая строка объединения
              XmlElement elCoveredCell = xmlDoc.CreateElement("table:covered-table-cell", nmspcTable);
              elHeadRow.AppendChild(elCoveredCell);

              continue;
            }

            XmlElement elCell = xmlDoc.CreateElement("table:table-cell", nmspcTable);
            elHeadRow.AppendChild(elCell);
            SetAttr(elCell, "table:style-name", "ColHdr", nmspcTable);

            // один заголовок также может содержать несколько абзацев
            if (!String.IsNullOrEmpty(headerArray.Text[i, j]))
            {
              SetAttr(elCell, "office:value-type", "string", nmspcOffice);

              string[] a = headerArray.Text[i, j].Split(DataTools.NewLineSeparators, StringSplitOptions.None);
              for (int k = 0; k < a.Length; k++)
              {
                string s = a[k];
                s = s.Replace(DataTools.SoftHyphenStr, ""); // не знаю, как записать мягкий перенос
                s = DataTools.ReplaceAny(s, EFPDataGridViewExpExcel2003.BadValueChars, ' ');

                // неразрывный пробел так и остается
                XmlElement elP = xmlDoc.CreateElement("text:p", nmspcText);
                elCell.AppendChild(elP);
                elP.InnerText = s;
              }
            }

            //attr = XmlDoc.CreateAttribute("ss:StyleID", nmspcSS);
            //attr.Value = "ColHdr";
            //elCell.Attributes.Append(attr);

            if (headerArray.RowSpan[i, j] > 1 || headerArray.ColumnSpan[i, j] > 1)
            {
              SetAttr(elCell, "table:number-columns-spanned", headerArray.ColumnSpan[i, j].ToString(), nmspcTable);
              SetAttr(elCell, "table:number-rows-spanned", headerArray.RowSpan[i, j].ToString(), nmspcTable);
            }

          } // по столбцам
        } // по строкам заголовка
      } // есть заголовки

      #endregion

      #region Основные строки

      StringBuilder sb = new StringBuilder();

      // Сюда запоминаем существующие стили ячеек
      List<string> cellStyleKeys = new List<string>();

      // Хранилище для границ ячеек
      EFPDataGridViewBorderStyle[,] borderStyles = null;
      if (settings.UseBorders)
        borderStyles = new EFPDataGridViewBorderStyle[area.ColumnCount, 6];

      // Перебираем строки таблицы
      for (int i = 0; i < area.RowCount; i++)
      {
        // Границы надо запрашивать отдельно, в режиме печати
        if (settings.UseBorders)
        {
          controlProvider.DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.Print);
          for (int j = 0; j < area.ColumnCount; j++)
          {
            int columnIndex = area.ColumnIndices[j];
            EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(columnIndex);
            borderStyles[j, 0] = CellArgs.BottomBorder;
            borderStyles[j, 1] = CellArgs.LeftBorder;
            borderStyles[j, 2] = CellArgs.RightBorder;
            borderStyles[j, 3] = CellArgs.TopBorder;
            if (CellArgs.DiagonalUpBorder == EFPDataGridViewBorderStyle.Default)
              borderStyles[j, 4] = EFPDataGridViewBorderStyle.None;
            else
              borderStyles[j, 4] = CellArgs.DiagonalUpBorder;
            if (CellArgs.DiagonalDownBorder == EFPDataGridViewBorderStyle.Default)
              borderStyles[j, 5] = EFPDataGridViewBorderStyle.None;
            else
              borderStyles[j, 5] = CellArgs.DiagonalDownBorder;
          }
        }

        controlProvider.DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.View);

        XmlElement elRow = xmlDoc.CreateElement("table:table-row", nmspcTable);
        elTable.AppendChild(elRow);
        SetAttr(elRow, "table:style-name", "ro2", nmspcTable); // 17.07.2019

        for (int j = 0; j < area.ColumnCount; j++)
        {
          int columnIndex = area.ColumnIndices[j];

          XmlElement elCell = xmlDoc.CreateElement("table:table-cell", nmspcTable);
          elRow.AppendChild(elCell);

          #region Определение формата

          EFPDataGridViewCellAttributesEventArgs cellArgs = controlProvider.DoGetCellAttributes(columnIndex);
          EFPDataGridViewExcelCellAttributes excelAttr = EFPDataGridView.GetExcelCellAttr(cellArgs);


          bool wordWrap = cellArgs.CellStyle.WrapMode == DataGridViewTriState.True;
          if (cellArgs.Column.GridColumn is DataGridViewTextBoxColumn)
          {
            Type valueType = ((DataGridViewTextBoxColumn)(cellArgs.Column.GridColumn)).ValueType;
            if (valueType == typeof(string) || valueType == null)
              wordWrap = true;
          }

          String formatText = String.Empty;
          if (!String.IsNullOrEmpty(cellArgs.CellStyle.Format))
            formatText = cellArgs.CellStyle.Format;


          #endregion

          #region Поиск существующего стиля ячейки

          sb.Length = 0;
          sb.Append(formatText);
          sb.Append("|");
          sb.Append(cellArgs.CellStyle.Alignment.ToString());
          sb.Append("|");
          sb.Append(wordWrap.ToString());
          sb.Append("|");
          sb.Append(cellArgs.IndentLevel.ToString());
          sb.Append("|");
          if (settings.UseInterior)
          {
            sb.Append(excelAttr.BackColor.ToString());
            sb.Append("|");
            sb.Append(excelAttr.ForeColor.ToString());
            sb.Append("|");
            sb.Append(excelAttr.Bold);
            sb.Append("|");
            sb.Append(excelAttr.Italic);
            sb.Append("|");
            sb.Append(excelAttr.Underline);
            sb.Append("|");
          }
          if (settings.UseBorders)
          {
            for (int k = 0; k < borderStyles.GetLength(1); k++)
            {
              sb.Append(borderStyles[j, k]);
              sb.Append("|");
            }
          }
          int styleKeyIndex = cellStyleKeys.IndexOf(sb.ToString());

          #endregion

          if (styleKeyIndex < 0)
          {
            #region Добавление стиля

            cellStyleKeys.Add(sb.ToString());
            styleKeyIndex = cellStyleKeys.Count - 1;

            // Сначала добавляем формат для чисел
            string nFormatName = "N" + (styleKeyIndex + 1).ToString();
            if (!OpenOfficeTools.ODFAddFormat(elAutoStyles, formatText, nFormatName))
              nFormatName = null;

            XmlElement elStyle = xmlDoc.CreateElement("style:style", nmspcStyle);
            elAutoStyles.AppendChild(elStyle);
            SetAttr(elStyle, "style:name", "ce" + (styleKeyIndex + 1).ToString(), nmspcStyle);
            SetAttr(elStyle, "style:family", "table-cell", nmspcStyle);

            if (nFormatName != null)
              SetAttr(elStyle, "style:data-style-name", nFormatName, nmspcStyle);

            XmlElement elCellProps = xmlDoc.CreateElement("style:table-cell-properties", nmspcStyle);
            elStyle.AppendChild(elCellProps);

            XmlElement elParProps = xmlDoc.CreateElement("style:paragraph-properties", nmspcStyle);
            elStyle.AppendChild(elParProps);

            if (settings.UseBorders)
            {
              if (borderStyles[j, 0] == borderStyles[j, 1] &&
                borderStyles[j, 0] == borderStyles[j, 2] &&
                borderStyles[j, 0] == borderStyles[j, 3] &&
                borderStyles[j, 4] == EFPDataGridViewBorderStyle.None && borderStyles[j, 5] == EFPDataGridViewBorderStyle.None)

                // Все границы одинаковые
                SetBorders(elCellProps, "fo:border", borderStyles[j, 0], nmspcFo);
              else
              {
                // Границы разные
                SetBorders(elCellProps, "fo:border-bottom", borderStyles[j, 0], nmspcFo);
                SetBorders(elCellProps, "fo:border-left", borderStyles[j, 1], nmspcFo);
                SetBorders(elCellProps, "fo:border-right", borderStyles[j, 2], nmspcFo);
                SetBorders(elCellProps, "fo:border-top", borderStyles[j, 3], nmspcFo);
                if (borderStyles[j, 4] != EFPDataGridViewBorderStyle.None || borderStyles[j, 5] != EFPDataGridViewBorderStyle.None)
                {
                  SetBorders(elCellProps, "style:diagonal-bl-tr", borderStyles[j, 4], nmspcStyle);
                  SetBorders(elCellProps, "style:diagonal-tl-br", borderStyles[j, 5], nmspcStyle);
                }
              }
            }

            string ha, va;
            switch (cellArgs.CellStyle.Alignment)
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
            if (wordWrap)
              SetAttr(elCellProps, "fo:wrap-option", "wrap", nmspcFo);

            if (cellArgs.IndentLevel > 0)
            {
              if (ha == "start")
                SetAttr(elParProps, "fo:margin-left", (0.3 * cellArgs.IndentLevel) + "cm", nmspcFo);
              if (ha == "end") // наверное, не будет работать
                SetAttr(elParProps, "fo:margin-right", (0.3 * cellArgs.IndentLevel) + "cm", nmspcFo);
            }


            if (settings.UseInterior)
            {
              if (excelAttr.Bold || excelAttr.Italic || excelAttr.Underline || (!excelAttr.ForeColor.IsEmpty))
              {
                XmlElement elTextProps = xmlDoc.CreateElement("style:text-properties", nmspcStyle);
                elStyle.AppendChild(elTextProps);

                if (excelAttr.Bold)
                  SetAttr(elTextProps, "fo:font-weight", "bold", nmspcFo);
                if (excelAttr.Italic)
                  SetAttr(elTextProps, "fo:font-style", "italic", nmspcFo);
                if (excelAttr.Underline)
                  SetAttr(elTextProps, "style:text-underline-style", "solid", nmspcStyle);
                if (!excelAttr.ForeColor.IsEmpty)
                  SetAttr(elTextProps, "fo:color", MyColorStr(excelAttr.ForeColor), nmspcFo);
              }

              if (!excelAttr.BackColor.IsEmpty)
                SetAttr(elCellProps, "fo:background-color", MyColorStr(excelAttr.BackColor), nmspcFo);

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

          SetAttr(elCell, "table:style-name", "ce" + (styleKeyIndex + 1).ToString(), nmspcTable);

          #region Значение ячейки

          if (cellArgs.ContentVisible) // 24.08.2015
          {
            object cellValue = cellArgs.FormattedValue;
            if (cellValue is DBNull)
              cellValue = null;
            if (cellValue is Image)
              cellValue = null; // 14.01.2014
            if (cellValue is Boolean)
            {
              switch (settings.BoolMode)
              {
                case BRDataViewBoolMode.Boolean:
                  break; // как есть
                case BRDataViewBoolMode.Integer:
                  cellValue = ((bool)cellValue) ? 1 : 0;
                  break;
                case BRDataViewBoolMode.Text:
                  cellValue = ((bool)cellValue) ? "[X]" : "[ ]";
                  break;
              }
            }
            if (cellValue != null)
            {
              #region Текстовое представление

              string s = cellValue.ToString();
              if (s.Length > 0) // 17.07.2019
              {
                string[] a = s.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
                for (int k = 0; k < a.Length; k++)
                {
                  XmlElement elP = xmlDoc.CreateElement("text:p", nmspcText);
                  elCell.AppendChild(elP);
                  s = a[k];
                  s = DataTools.ReplaceAny(s, EFPDataGridViewExpExcel2003.BadValueChars, ' ');
                  //elP.InnerText = s; // это - текст. В числах запятая на точку не заменяется
                  XmlText txt = xmlDoc.CreateTextNode(s); // 17.07.2019
                  elP.AppendChild(txt);
                }
              }

              #endregion

              #region Типизированное значение

              switch (cellValue.GetType().Name)
              {
                case "String":
                  SetAttr(elCell, "office:value-type", "string", nmspcOffice);
                  break;
                case "Boolean":
                  SetAttr(elCell, "office:value-type", "boolean", nmspcOffice);
                  SetAttr(elCell, "office:boolean-value", (bool)cellValue ? "true" : "false", nmspcOffice);
                  break;
                case "DateTime":
                  // Теоретически существует еще тип time, но реально везде используется date
                  DateTime dt = (DateTime)cellValue;
                  SetAttr(elCell, "office:value-type", "date", nmspcOffice);
                  SetAttr(elCell, "office:date-value", dt.ToString("s"), nmspcOffice);
                  break;
                default:
                  if (DataTools.IsIntegerType(cellValue.GetType()) || DataTools.IsFloatType(cellValue.GetType()))
                  {
                    SetAttr(elCell, "office:value-type", "float", nmspcOffice);
                    SetAttr(elCell, "office:value", Convert.ToString(cellValue, StdConvert.NumberFormat), nmspcOffice);
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

      return xmlDoc;
    }

    private static string MyColorStr(Color c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    private static void SetBorders(XmlElement elCellProps, string attrName, EFPDataGridViewBorderStyle borderStyle, string nmspc)
    {
      string value;
      switch (borderStyle)
      {
        case EFPDataGridViewBorderStyle.Default:
          value = "0.1pt solid #000000";
          break;
        case EFPDataGridViewBorderStyle.Thin:
          value = "0.5pt solid #000000";
          break;
        case EFPDataGridViewBorderStyle.Thick:
          value = "1pt solid #000000";
          break;
        default:
          return;
      }

      SetAttr(elCellProps, attrName, value, nmspc);
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

    #endregion
  }
}
