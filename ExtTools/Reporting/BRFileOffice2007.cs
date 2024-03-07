using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Models.SpreadsheetBase;
using FreeLibSet.Shell;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Базовый класс для <see cref="BRFileDOCX"/> и <see cref="BRFileXLSX"/>
  /// </summary>
  public abstract class BRFileOffice2007Base : BRFileCreator
  {
    #region Статическое свойство

    /// <summary>
    /// Возвращает true, если можно создать .ODS/ODT файл, то есть доступна библиотека для создания zip-архива.
    /// Наличие установленного OpenOffice не требуется.
    /// </summary>
    public static bool IsSupported { get { return ZipFileTools.ZipLibAvailable; } }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Если true, то в xml-файле будут использоваться отступы
    /// </summary>
    public bool DebugXml;

    #endregion

    #region Пространства имен

    internal const string nmspcXSI = "http://www.w3.org/2001/XMLSchema-instance";

    internal const string nmspcDCTerms = "http://purl.org/dc/terms/";
    internal const string nmspcDCElems = "http://purl.org/dc/elements/1.1/";
    internal const string nmspcDcmiType = "http://purl.org/dc/dcmitype/";

    internal const string nmspcContentTypes = "http://schemas.openxmlformats.org/package/2006/content-types";
    internal const string nmspcCoreProps = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
    internal const string nmspcPackageRels = "http://schemas.openxmlformats.org/package/2006/relationships";
    internal const string nmspcExtProps = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
    internal const string nmspcDocPropsVTypes = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
    internal const string nmspcOfficeRels = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    #endregion

    #region Вспомогательные файлы

    internal static XmlDocument CreateDocPropsCoreXml(BRDocumentProperties props)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("cp:coreProperties", nmspcCoreProps);
      SetAttr(elRoot, "xmlns:cp", nmspcCoreProps, String.Empty);
      SetAttr(elRoot, "xmlns:dc", nmspcDCElems, String.Empty);
      SetAttr(elRoot, "xmlns:dcterms", nmspcDCTerms, String.Empty);
      SetAttr(elRoot, "xmlns:dcmitype", nmspcDcmiType, String.Empty);
      SetAttr(elRoot, "xmlns:xsi", nmspcXSI, String.Empty);

      xmlDoc.AppendChild(elRoot);

      SetTextNode(elRoot, "dc:title", props.Title, nmspcDCElems);
      SetTextNode(elRoot, "dc:subject", props.Subject, nmspcDCElems);
      SetTextNode(elRoot, "dc:creator", props.Author, nmspcDCElems);

      return xmlDoc;
    }

    internal static XmlDocument CreateDocPropsAppXml(BRDocumentProperties props)
    {
      XmlDocument XmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      XmlDoc.InsertBefore(xmldecl, XmlDoc.DocumentElement);

      XmlElement elRoot = XmlDoc.CreateElement("Properties", nmspcExtProps);
      SetAttr(elRoot, "xmlns", nmspcExtProps, String.Empty);
      SetAttr(elRoot, "xmlns:vt", nmspcDocPropsVTypes, String.Empty);
      XmlDoc.AppendChild(elRoot);

      SetTextNode(elRoot, "Company", props.Company, nmspcExtProps);

      return XmlDoc;
    }

    #endregion

    #region Вспомогательные методы

    internal static void SetAttr(XmlElement el, string name, string value, string nmspc)
    {
      XmlAttribute attr;
      if (String.IsNullOrEmpty(nmspc))
        attr = el.OwnerDocument.CreateAttribute(name);
      else
        attr = el.OwnerDocument.CreateAttribute(name, nmspc);
      attr.Value = value;
      el.Attributes.Append(attr);
    }

    internal static void SetTextNode(XmlElement elParent, string name, string value, string nmspc)
    {
      if (String.IsNullOrEmpty(value))
        return;

      XmlElement el = elParent.OwnerDocument.CreateElement(name, nmspc);
      XmlText txt = elParent.OwnerDocument.CreateTextNode(value);
      el.AppendChild(txt);
      elParent.AppendChild(el);
    }

    #endregion
  }

  /// <summary>
  /// Создание файлов в формате XLSX (Excel 2007)
  /// </summary>
  public sealed class BRFileXLSX : BRFileOffice2007Base
  {
    #region Конструктор

    /// <summary>
    /// Создает генератор файла с заданным измерителем
    /// </summary>
    /// <param name="measurer">Измеритель</param>
    public BRFileXLSX(IBRMeasurer measurer)
    {
      if (measurer == null)
        throw new ArgumentNullException("measurer");
      _Measurer = measurer;
    }

    private IBRMeasurer _Measurer;

    #endregion

    #region Создание файла

    /// <summary>
    /// Создает файл xlsx
    /// </summary>
    /// <param name="report">Выгружаемый отчет</param>
    /// <param name="filePath">Путь к создаваемому файлу с расширением xlsx</param>
    protected override void DoCreateFile(BRReport report, AbsPath filePath)
    {
      ZipFileTools.CheckZipLibAvailable();
      DoCreateFile2(report, filePath); // выделено в отдельный метод, чтобы выдавалось осмысленное исключение
    }

    private void DoCreateFile2(BRReport report, AbsPath filePath)
    {
      #region Начало

      //SharpZipLibLoader.LoadSharpZipLib();
      ZipFileCreator zf = new ZipFileCreator(filePath.Path);

      #endregion

      #region Заголовок ([Content_Types].xml)

      XmlDocument xmlDocCT = CreateContentTypesXml(report);
      zf.AddXmlFile("[Content_Types].xml", xmlDocCT);

      #endregion

      #region Связи (_rels/.rels)

      XmlDocument xmlDocRels1 = CreateRels1();
      zf.AddXmlFile("_rels/.rels", xmlDocRels1);

      XmlDocument xmlDocRels2 = CreateRels2(report);
      zf.AddXmlFile("xl/_rels/workbook.xml.rels", xmlDocRels2);

      #endregion

      #region Сводка

      XmlDocument xmlDocCore = CreateDocPropsCoreXml(report.DocumentProperties);
      zf.AddXmlFile("docProps/core.xml", xmlDocCore);


      XmlDocument xmlDocApp = CreateDocPropsAppXml(report.DocumentProperties);
      zf.AddXmlFile("docProps/app.xml", xmlDocApp);

      #endregion

      #region Workbook

      XmlDocument xmlDocWorkbook = CreateWorkbookXml(report);
      zf.AddXmlFile("xl/workbook.xml", xmlDocWorkbook);

      #endregion

      #region Worksheet

      List<string> sharedStrings = new List<string>();
      int sharedStringCount = 0;
      StyleTable styles = new StyleTable();
      styles.GetStyle(report.DefaultCellStyle, false); // сразу добавляем основной стиль

      for (int iSect = 0; iSect < report.Sections.Count; iSect++)
      {
        XmlDocument xmlDocSheet = CreateSheetXml(report.Sections[iSect], sharedStrings, ref sharedStringCount, styles);
        zf.AddXmlFile("xl/worksheets/sheet" + StdConvert.ToString(iSect + 1) + ".xml", xmlDocSheet);
      }

      XmlDocument xmlDocStyles = styles.FinishDoc();
      zf.AddXmlFile("xl/styles.xml", xmlDocStyles);

      #endregion

      #region Список строк

      XmlDocument xmlDocSharedStrings = CreateSharedStringsXml(sharedStrings, sharedStringCount);
      zf.AddXmlFile("xl/sharedStrings.xml", xmlDocSharedStrings);

      #endregion

      #region Конец

      zf.Close();

      #endregion
    }

    #endregion

    #region Вспомогательные файлы для OpenXML

    private static XmlDocument CreateContentTypesXml(BRReport report)
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

      for (int iSect = 0; iSect < report.Sections.Count; iSect++)
      {
        elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
        SetAttr(elOverride, "PartName", "/xl/worksheets/sheet" + StdConvert.ToString(iSect + 1) + ".xml", String.Empty);
        SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml", String.Empty);
        elRoot.AppendChild(elOverride);
      }

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/sharedStrings.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/xl/styles.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/docProps/core.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-package.core-properties+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/docProps/app.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.extended-properties+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      return xmlDoc;
    }

    private static XmlDocument CreateRels1()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elRL;

      // Не знаю, почему порядок строк обратный. Excel так делает

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId3", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties", String.Empty);
      SetAttr(elRL, "Target", "docProps/app.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId2", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties", String.Empty);
      SetAttr(elRL, "Target", "docProps/core.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId1", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", String.Empty);
      SetAttr(elRL, "Target", "xl/workbook.xml", String.Empty);
      elRoot.AppendChild(elRL);

      return xmlDoc;
    }

    private static XmlDocument CreateRels2(BRReport report)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      int cntRId = 0;
      XmlElement elRL;
      for (int iSect = 0; iSect < report.Sections.Count; iSect++)
      {
        elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
        SetAttr(elRL, "Id", "rId" + StdConvert.ToString(++cntRId), String.Empty);
        SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", String.Empty);
        SetAttr(elRL, "Target", "worksheets/sheet" + StdConvert.ToString(iSect + 1) + ".xml", String.Empty);
        elRoot.AppendChild(elRL);
      }

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId" + StdConvert.ToString(++cntRId), String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings", String.Empty);
      SetAttr(elRL, "Target", "sharedStrings.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId" + StdConvert.ToString(++cntRId), String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", String.Empty);
      SetAttr(elRL, "Target", "styles.xml", String.Empty);
      elRoot.AppendChild(elRL);

      return xmlDoc;
    }

    private const string nmspcSpreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    private static XmlDocument CreateWorkbookXml(BRReport report)
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

      for (int iSect = 0; iSect < report.Sections.Count; iSect++)
      {
        XmlElement elSheet = xmlDoc.CreateElement("sheet", nmspcSpreadsheet);
        SetAttr(elSheet, "name", StdConvert.ToString(iSect + 1), String.Empty);
        SetAttr(elSheet, "sheetId", StdConvert.ToString(iSect + 1), String.Empty);
        SetAttr(elSheet, "r:id", "rId" + StdConvert.ToString(iSect + 1), nmspcOfficeRels);
        elSheets.AppendChild(elSheet);
      }

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

      public StyleTable()
      {
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

        // Обязательно создаем корневые узды сейчас, иначе они будут не в том порядке
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
        GetFillIdx(BRColor.Auto);

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

      /// <summary>
      /// Добавление стиля
      /// </summary>
      /// <param name="cellStyle">Стиль ячейки</param>
      /// <param name="addWrap">True, если обязательно нужен перенос по словам. 
      /// Перенос нужен, если внутри текста ячейки есть символы переноса строки</param>
      /// <returns></returns>
      public int GetStyle(BRCellStyle cellStyle, bool addWrap)
      {
        #region Числовые форматы

        int numFmtId = 0; // общий формат
        if (!String.IsNullOrEmpty(cellStyle.Format))
        {
          if (!_NumFmtDict.TryGetValue(cellStyle.Format, out numFmtId))
          {
            if (_elNumFmtRoot == null)
            {
              _elNumFmtRoot = _XmlDoc.CreateElement("numFmts", nmspcSpreadsheet);
              _elRoot.AppendChild(_elNumFmtRoot);
            }

            XmlElement elNumFmt = _XmlDoc.CreateElement("numFmt", nmspcSpreadsheet);
            numFmtId = _NumFmtDict.Count + 1001; // Начинаем нумерацию с 1001
            SetAttr(elNumFmt, "numFmtId", numFmtId.ToString(), String.Empty);
            SetAttr(elNumFmt, "formatCode", cellStyle.Format, String.Empty);
            _elNumFmtRoot.AppendChild(elNumFmt);

            _NumFmtDict.Add(cellStyle.Format, numFmtId);
          }
        }

        #endregion

        #region Шрифт

        string fontKey =
          cellStyle.FontName + "|" +
          cellStyle.FontHeightTwip + "|" +
          cellStyle.LineHeightTwip + "|" +
          cellStyle.FontWidthTwip + "|" +
          cellStyle.FontWidthPercent + "|" +
          cellStyle.Bold.ToString() + "|" +
          cellStyle.Italic.ToString() + "|" +
          cellStyle.Underline.ToString() + "|" +
          cellStyle.Strikeout.ToString() + "|" +
          cellStyle.ForeColor.ToString();
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

          XmlElement elFontName = _XmlDoc.CreateElement("name", nmspcSpreadsheet);
          SetAttr(elFontName, "val", cellStyle.FontName, String.Empty);
          elFont.AppendChild(elFontName);
          XmlElement elFontSize = _XmlDoc.CreateElement("sz", nmspcSpreadsheet);
          SetAttr(elFontSize, "val", StdConvert.ToString(cellStyle.FontHeightPt), String.Empty);
          elFont.AppendChild(elFontSize);

          if (cellStyle.Bold)
          {
            XmlElement elBold = _XmlDoc.CreateElement("b", nmspcSpreadsheet);
            elFont.AppendChild(elBold);
          }
          if (cellStyle.Italic)
          {
            XmlElement elBold = _XmlDoc.CreateElement("i", nmspcSpreadsheet);
            elFont.AppendChild(elBold);
          }
          if (cellStyle.Underline)
          {
            XmlElement elBold = _XmlDoc.CreateElement("u", nmspcSpreadsheet);
            elFont.AppendChild(elBold);
          }
          if (cellStyle.Strikeout)
          {
            XmlElement elStrikeout = _XmlDoc.CreateElement("s", nmspcSpreadsheet);
            elFont.AppendChild(elStrikeout);
          }

          if (!cellStyle.ForeColor.IsAuto)
          {
            XmlElement elColor = _XmlDoc.CreateElement("color", nmspcSpreadsheet);
            SetAttr(elColor, "rgb", MyColorStr(cellStyle.ForeColor), String.Empty);
            elFont.AppendChild(elColor);
          }

          fontIdx = _FontDict.Count;
          _FontDict.Add(fontKey, fontIdx);
        }

        #endregion

        #region Заполнение

        int fillIdx = GetFillIdx(cellStyle.BackColor);

        #endregion

        #region Границы

        string borderKey = cellStyle.LeftBorder.ToString() + "|" +
          cellStyle.TopBorder.ToString() + "|" +
          cellStyle.RightBorder.ToString() + "|" +
          cellStyle.BottomBorder.ToString() + "|" +
          cellStyle.DiagonalUp.ToString() + "|" +
          cellStyle.DiagonalDown.ToString();
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

          AddBorder(elBorder, "left", cellStyle.LeftBorder);
          AddBorder(elBorder, "right", cellStyle.RightBorder);
          AddBorder(elBorder, "top", cellStyle.TopBorder);
          AddBorder(elBorder, "bottom", cellStyle.BottomBorder);

          // Диагональные линии хранятся по-другому
          // Используется общая толщина и цвет линии
          if (cellStyle.DiagonalUp.Style != BRLineStyle.None || cellStyle.DiagonalDown.Style != BRLineStyle.None)
          {
            if ((cellStyle.DiagonalUp.Style != BRLineStyle.None) != (cellStyle.DiagonalDown.Style != BRLineStyle.None))
            {
              // Атрибут нужен, если есть только одна диагональ. Если обе диагонали, то не нужен
              if (cellStyle.DiagonalUp.Style != BRLineStyle.None)
                SetAttr(elBorder, "diagonalUp", "1", String.Empty);
              if (cellStyle.DiagonalDown.Style != BRLineStyle.None)
                SetAttr(elBorder, "diagonalDown", "1", String.Empty);
            }

            AddBorder(elBorder, "diagonal", cellStyle.DiagonalUp | cellStyle.DiagonalDown);
          }

          borderIdx = _BordersDict.Count;
          _BordersDict.Add(borderKey, borderIdx);
        }

        #endregion

        #region Стиль ячейки

        string styleKey = numFmtId.ToString() + "|" +
          fontIdx.ToString() + "|" +
          fillIdx.ToString() + "|" +
          borderIdx.ToString() + "|" +
          cellStyle.HAlign.ToString() + "|" +
          cellStyle.VAlign.ToString() + "|" +
          cellStyle.IndentLevel.ToString() + "|" +
          cellStyle.WrapMode.ToString() + "|" +
          (addWrap ? "1" : "0");
        int styleIdx;
        if (!_CellXfsDict.TryGetValue(styleKey, out styleIdx))
        {
          // Узел создан в конструкторе
          //if (_elCellXfsRoot == null)
          //{
          //  _elCellXfsRoot = _XmlDoc.CreateElement("cellXfs", nmspcSpreadsheet);
          //  _elRoot.AppendChild(_elCellXfsRoot);
          //}

          XmlElement elXf = _XmlDoc.CreateElement("xf", nmspcSpreadsheet);

          if (numFmtId > 0)
          {
            SetAttr(elXf, "numFmtId", StdConvert.ToString(numFmtId), String.Empty);
            SetAttr(elXf, "applyNumberFormat", "1", String.Empty);
          }

          if (fontIdx > 0)
          {
            SetAttr(elXf, "fontId", StdConvert.ToString(fontIdx), String.Empty);
            SetAttr(elXf, "applyFont", "1", String.Empty);
          }

          if (!cellStyle.BackColor.IsAuto)
          {
            SetAttr(elXf, "fillId", StdConvert.ToString(fillIdx), String.Empty);
            SetAttr(elXf, "applyFill", "1", String.Empty);
          }

          if (borderIdx > 0)
          {
            SetAttr(elXf, "borderId", StdConvert.ToString(borderIdx), String.Empty);
            SetAttr(elXf, "applyBorder", "1", String.Empty);
          }
          _elCellXfsRoot.AppendChild(elXf);

          XmlElement elAlignment = _XmlDoc.CreateElement("alignment", nmspcSpreadsheet);
          switch (cellStyle.HAlign)
          {
            case BRHAlign.Left: SetAttr(elAlignment, "horizontal", "left", String.Empty); break;
            case BRHAlign.Center: SetAttr(elAlignment, "horizontal", "center", String.Empty); break;
            case BRHAlign.Right: SetAttr(elAlignment, "horizontal", "right", String.Empty); break;
          }
          switch (cellStyle.VAlign)
          {
            case BRVAlign.Top: SetAttr(elAlignment, "vertical", "top", String.Empty); break;
            case BRVAlign.Center: SetAttr(elAlignment, "vertical", "center", String.Empty); break;
            case BRVAlign.Bottom: SetAttr(elAlignment, "vertical", "bottom", String.Empty); break;
          }
          SetAttr(elXf, "applyAlignment", "1", String.Empty);

          if (cellStyle.IndentLevel > 0)
            SetAttr(elAlignment, "indent", StdConvert.ToString(cellStyle.IndentLevel), String.Empty);
          if (cellStyle.WrapMode != BRWrapMode.NoWrap || addWrap)
            SetAttr(elAlignment, "wrapText", "1", String.Empty);
          elXf.AppendChild(elAlignment);

          styleIdx = _CellXfsDict.Count;
          _CellXfsDict.Add(styleKey, styleIdx);
        }

        #endregion

        return styleIdx;
      }

      private int GetFillIdx(BRColor backColor)
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
          if (backColor.IsAuto)
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

      private static void AddBorder(XmlElement elParent, string tagName, BRLine border)
      {
        if (border.Style == BRLineStyle.None)
          return;

        XmlElement elBorder = elParent.OwnerDocument.CreateElement(tagName, nmspcSpreadsheet);
        elParent.AppendChild(elBorder);

        string sStyle;
        switch (border.Style)
        {
          case BRLineStyle.Thin: sStyle = "thin"; break;
          case BRLineStyle.Medium: sStyle = "medium"; break;
          case BRLineStyle.Thick: sStyle = "thick"; break;
          case BRLineStyle.Dot: sStyle = "dotted"; break;
          case BRLineStyle.Dash: sStyle = "dashed"; break;
          case BRLineStyle.DashDot: sStyle = "dashDot"; break;
          case BRLineStyle.DashDotDot: sStyle = "dashDotDot"; break;
          default: throw new BugException("BorderStyle=" + border.Style.ToString());
        }
        SetAttr(elBorder, "style", sStyle, String.Empty);
        if (!border.Color.IsAuto)
        {
          XmlElement elColor = elParent.OwnerDocument.CreateElement("color", nmspcSpreadsheet);
          SetAttr(elColor, "rgb", MyColorStr(border.Color), String.Empty);
          elBorder.AppendChild(elColor);
        }
      }

      private static string MyColorStr(BRColor c)
      {
        Int32 x = (c.R << 16) | (c.G << 8) | c.B;
        if ((c.R & 0x80) != 0)
          x = x | (0xFF << 24);
        return x.ToString("X8");
      }

      public XmlDocument FinishDoc()
      {
        SetAttr(_elNumFmtRoot, "count", StdConvert.ToString(_NumFmtDict.Count), String.Empty);
        SetAttr(_elFontRoot, "count", StdConvert.ToString(_FontDict.Count), String.Empty);
        SetAttr(_elFillRoot, "count", StdConvert.ToString(_FillDict.Count), String.Empty);
        SetAttr(_elBordersRoot, "count", StdConvert.ToString(_BordersDict.Count), String.Empty);
        if (_CellXfsDict.Count > 0)
          SetAttr(_elCellXfsRoot, "count", StdConvert.ToString(_CellXfsDict.Count), String.Empty);
        else
          _elCellXfsRoot.ParentNode.RemoveChild(_elCellXfsRoot);

        return _XmlDoc;
      }

      #endregion
    }

    #endregion

    #region Создание листа

    private static readonly char[] _NewLineChars = new char[] { '\r', '\n' };

    private XmlDocument CreateSheetXml(BRSection section, List<string> sharedStrings, ref int sharedStringCount, StyleTable styles)
    {
      #region Таблица разделяемых строк

      Dictionary<string, int> sharedStringIndices = new Dictionary<string, int>(); // индексы строк в массиве sharedStrings

      #endregion

      #region Начало документа sheetX.xml

      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elWorksheet = xmlDoc.CreateElement("worksheet", nmspcSpreadsheet);
      SetAttr(elWorksheet, "xmlns", nmspcSpreadsheet, String.Empty);
      SetAttr(elWorksheet, "xmlns:r", nmspcOfficeRels, String.Empty);
      xmlDoc.AppendChild(elWorksheet);

      XmlElement elMergeCells = xmlDoc.CreateElement("mergeCells", nmspcSpreadsheet);
      // Потом добавим elSheetRoot.AppendChild(elMergeCells);

      #endregion

      #region Замороженные строки и столбцы

      XmlElement elSheetViews = xmlDoc.CreateElement("sheetViews", nmspcSpreadsheet);
      elWorksheet.AppendChild(elSheetViews);
      XmlElement elsheetView = xmlDoc.CreateElement("sheetView", nmspcSpreadsheet);
      elSheetViews.AppendChild(elsheetView);
      SetAttr(elsheetView, "workbookViewId", "0", String.Empty);

      // TODO: Замороженные строки и столбцы

      #endregion

      #region Размеры столбцов

      BRColumnBoundList colBounds = new BRColumnBoundList(section, 10, 1);

      XmlElement elCols = xmlDoc.CreateElement("cols", nmspcSpreadsheet);
      elWorksheet.AppendChild(elCols);

      double stdCharWidthPt = GetStdCharWidthPt(section.Report.DefaultCellStyle);

      for (int l = 0; l < colBounds.WidthArray.Length; l++)
      {
        XmlElement elCol = xmlDoc.CreateElement("col", nmspcSpreadsheet);
        SetAttr(elCol, "min", (l + 1).ToString(), String.Empty);
        SetAttr(elCol, "max", (l + 1).ToString(), String.Empty);
        // Ширина задается в условных единицах, соответствующих ширине символа основного шрифта
        double widthPt = colBounds.WidthArray[l] / 254.0 * 72.0;
        SetAttr(elCol, "width", (widthPt / stdCharWidthPt).ToString("0.##", StdConvert.NumberFormat), String.Empty);
        elCols.AppendChild(elCol);
      }

      #endregion

      #region Перебор полос

      XmlElement elSheetData = xmlDoc.CreateElement("sheetData", nmspcSpreadsheet);
      elWorksheet.AppendChild(elSheetData);
      int rowCount = 0;

      for (int j = 0; j < section.Bands.Count; j++)
      {
        #region Зазор между полосами

        int gap = 0;
        if (j > 0)
          gap = Math.Max(section.Bands[j - 1].BottomMargin, section.Bands[j].TopMargin);
        if (gap >= 10)
        {
          rowCount++;
          XmlElement elGapRow = xmlDoc.CreateElement("row", nmspcSpreadsheet);
          elSheetData.AppendChild(elGapRow);
          SetAttr(elGapRow, "r", StdConvert.ToString(rowCount), String.Empty);
          SetAttr(elGapRow, "customHeight", "1", String.Empty);
          SetAttr(elGapRow, "ht", StdConvert.ToString((int)(gap / 254.0 * 72.0)), String.Empty);
        }

        #endregion

        BRBand band = section.Bands[j];

        int[] leftCols = colBounds.LeftColumns[j];
        int[] rightCols = colBounds.RightColumns[j];

        BRSelector sel = band.CreateSelector();
        for (int k = 0; k < band.RowCount; k++)
        {
          sel.RowIndex = k;
          rowCount++;
          XmlElement elRow = xmlDoc.CreateElement("row", nmspcSpreadsheet);
          elSheetData.AppendChild(elRow);
          SetAttr(elRow, "r", StdConvert.ToString(rowCount), String.Empty);
          if (sel.RowInfo.Height != BRReport.AutoRowHeight)
          {
            SetAttr(elRow, "customHeight", "1", String.Empty);
            SetAttr(elRow, "ht", StdConvert.ToString((int)(sel.RowInfo.Height / 254.0 * 72.0)), String.Empty);
          }

          for (int l = 0; l < band.ColumnCount; l++)
          {
            sel.ColumnIndex = l;

            BRRange merge = sel.MergeInfo;

            for (int m = leftCols[l]; m <= rightCols[l]; m++)
            {
              bool isFirstCell = merge.FirstRowIndex == sel.RowIndex &&
                merge.FirstColumnIndex == sel.ColumnIndex &&
                m == leftCols[l]; // первая ячейка объединения или обычная ячейка

              XmlElement elC = xmlDoc.CreateElement("c", nmspcSpreadsheet);
              elRow.AppendChild(elC);

              bool hasNewLine = false;

              if (isFirstCell)
              {
                #region Значение

                string valueText, typeText, formatText;
                GetCellValue(sel.Value, out valueText, out typeText, out formatText);

                if (!String.IsNullOrEmpty(typeText))
                {
                  if (typeText == "s")
                  {
                    WriteCellString(elC, valueText, sharedStrings, sharedStringIndices, ref sharedStringCount);
                    if (valueText.IndexOfAny(_NewLineChars) >= 0)
                      hasNewLine = true;
                  }
                  else
                  {
                    if (!String.IsNullOrEmpty(typeText))
                      SetAttr(elC, "t", typeText, String.Empty);

                    XmlElement elV = elC.OwnerDocument.CreateElement("v", nmspcSpreadsheet);
                    elV.InnerText = valueText;
                    elC.AppendChild(elV);
                  }
                }

                #endregion
              }

              string cellAddr1 = SpreadsheetTools.GetColumnName(m) + StdConvert.ToString(rowCount);
              SetAttr(elC, "r", cellAddr1, String.Empty);
              int styleIdx = styles.GetStyle(sel.CellStyle, hasNewLine);
              SetAttr(elC, "s", StdConvert.ToString(styleIdx), String.Empty);

              if (isFirstCell)
              {
                #region Объединение ячеек

                int xlsColumn1 = leftCols[merge.FirstColumnIndex];
                int xlsColumn2 = rightCols[merge.LastColumnIndex];

                if (merge.RowCount > 1 || xlsColumn2 > xlsColumn1)
                {
                  string cellAddr2 = SpreadsheetTools.GetColumnName(xlsColumn2) + StdConvert.ToString(rowCount + merge.RowCount - 1);
                  XmlElement elMergeCell = xmlDoc.CreateElement("mergeCell", nmspcSpreadsheet);
                  SetAttr(elMergeCell, "ref", cellAddr1 + ":" + cellAddr2, String.Empty);
                  elMergeCells.AppendChild(elMergeCell);
                }

                #endregion
              }
            }
          } // цикл по столбцам
        } // цикл по строкам
      } // цикл по полосам


      #endregion

      // Узел "mergeCells" добавляется, только если есть объединенные ячейки
      if (elMergeCells.ChildNodes.Count > 0)
      {
        SetAttr(elMergeCells, "count", elMergeCells.ChildNodes.Count.ToString(), String.Empty); // число объединений
        elWorksheet.AppendChild(elMergeCells);
      }

      #region Параметры страницы

      XmlElement elPageMargins = xmlDoc.CreateElement("pageMargins", nmspcSpreadsheet);
      SetAttr(elPageMargins, "left", GetSizeText(section.PageSetup.LeftMargin), String.Empty);
      SetAttr(elPageMargins, "right", GetSizeText(section.PageSetup.RightMargin), String.Empty);
      SetAttr(elPageMargins, "top", GetSizeText(section.PageSetup.TopMargin), String.Empty);
      SetAttr(elPageMargins, "bottom", GetSizeText(section.PageSetup.BottomMargin), String.Empty);
      SetAttr(elPageMargins, "header", GetSizeText(section.PageSetup.TopMargin), String.Empty);
      SetAttr(elPageMargins, "footer", GetSizeText(section.PageSetup.BottomMargin), String.Empty);
      elWorksheet.AppendChild(elPageMargins);

      if (section.PageSetup.CenterHorizontal || section.PageSetup.CenterVertical)
      {
        XmlElement elPrintIOptions = xmlDoc.CreateElement("printOptions", nmspcSpreadsheet);
        if (section.PageSetup.CenterHorizontal)
          SetAttr(elPrintIOptions, "horizontalCentered", "1", String.Empty);
        if (section.PageSetup.CenterVertical)
          SetAttr(elPrintIOptions, "verticalCentered", "1", String.Empty);
        elWorksheet.AppendChild(elPrintIOptions);
      }

      XmlElement elPageSetup = xmlDoc.CreateElement("pageSetup", nmspcSpreadsheet);
      OLE.Excel.XlPaperSize paperSize = BRFileExcelOLE.GetPageSize(section.PageSetup);
      SetAttr(elPageSetup, "paperSize", StdConvert.ToString((int)paperSize), String.Empty);
      SetAttr(elPageSetup, "orientation", section.PageSetup.Orientation == BROrientation.Landscape ? "landscape" : "portrait", String.Empty);
      //SetAttr(elPageSetup, "id", "rId1", nmspcPackageRels);
      elWorksheet.AppendChild(elPageSetup);

      #endregion

      return xmlDoc;
    }

    private string GetSizeText(int sz01mm)
    {
      double szPt = sz01mm / 254.0;
      return szPt.ToString("0.0###", StdConvert.NumberFormat);
    }

    /// <summary>
    /// Возвращает ширину одного символа стандартного шрифта в пунктах
    /// </summary>
    /// <returns></returns>
    private double GetStdCharWidthPt(BRCellStyle cellStyle)
    {
      int w, h; // в единицах 0.1мм
      _Measurer.MeasureString("0000000000", cellStyle, out w, out h);
      double wPt = (double)w / 254.0 * 72.0;
      return wPt / 10;
    }

    private static void GetCellValue(object value, out string valueText, out string typeText, out string formatText)
    {
      valueText = null;
      typeText = null;
      formatText = null;

      if (value == null)
        return;
      if (value is DBNull)
        return;

      switch (value.GetType().Name)
      {
        case "String":
        case "Guid": // 03.02.2020
          if (value.ToString().Trim().Length == 0) // 08.08.2012
            return;
          valueText = value.ToString();
          valueText = DataTools.ReplaceAny(valueText, BRFileTools.BadValueChars, ' ');
          typeText = "s";
          break;
        case "Boolean":
          valueText = (bool)value ? "1" : "0";
          typeText = "b";
          break;
        case "DateTime":
          //ValueText = ((DateTime)v).ToString("s");
          // 05.04.2016. Дата/время должно задаваться как число
          try
          {
            double v2 = ((DateTime)value).ToOADate();
            valueText = Convert.ToString(v2, StdConvert.NumberFormat);
          }
          catch
          {
            valueText = "*** Дата, выходящая за допустимые пределы: " + value.ToString() + " ***";
            typeText = "s";
            return;
          }

          // 01.09.2015
          // Если формат данных в DataGridViewCellStyle не задан, используем формат по умолчанию
          // Если оставить формат "Общий", то даты будут отображаться в Excel как числа
          if (((DateTime)value).TimeOfDay.Ticks != 0L)
            formatText = MicrosoftOfficeTools.DefaultShortDateTimeFormat;
          else
            formatText = MicrosoftOfficeTools.DefaultShortDateFormat;
          typeText = "t";
          break;
        default:
          if (DataTools.IsIntegerType(value.GetType()) || DataTools.IsFloatType(value.GetType()))
          {
            valueText = Convert.ToString(value, StdConvert.NumberFormat);
            typeText = "n";
          }
          break;
      }
    }

    #endregion

    #region Запись значения

    private static void WriteCellString(XmlElement elC, string s, List<string> sharedStrings, Dictionary<string, int> sharedStringIndices,
      ref int sharedStringCount)
    {
      if (String.IsNullOrEmpty(s))
        return;

      s = s.Replace(Environment.NewLine, "\n"); // только символ 0x0A
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
      elV.InnerText = StdConvert.ToString(ssi);
      elC.AppendChild(elV);
      sharedStringCount++;
    }

    #endregion

    #region SharedStrings

    private static XmlDocument CreateSharedStringsXml(List<string> sharedStrings, int sharedStringCount)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("sst", nmspcSpreadsheet);
      SetAttr(elRoot, "count", StdConvert.ToString(sharedStringCount), String.Empty);
      SetAttr(elRoot, "uniqueCount", StdConvert.ToString(sharedStrings.Count), String.Empty);
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
  }

  /// <summary>
  /// Создание файлов в формате DOCX (Word 2007)
  /// </summary>
  public sealed class BRFileDOCX : BRFileOffice2007Base
  {
    #region Константы

    private int MinFontWidthPercent = 1;
    private int MaxFontWidthPercent = 600;

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает генератор файла с заданным измерителем
    /// </summary>
    /// <param name="measurer">Измеритель</param>
    public BRFileDOCX(IBRMeasurer measurer)
    {
      if (measurer == null)
        throw new ArgumentNullException("measurer");
      _Measurer = measurer;
    }

    private IBRMeasurer _Measurer;

    #endregion

    #region Создание файла

    /// <summary>
    /// Создает файл docx
    /// </summary>
    /// <param name="report">Выгружаемый отчет</param>
    /// <param name="filePath">Путь к создаваемому файлу с расширением docx</param>
    protected override void DoCreateFile(BRReport report, AbsPath filePath)
    {
      ZipFileTools.CheckZipLibAvailable();
      DoCreateFile2(report, filePath); // выделено в отдельный метод, чтобы выдавалось осмысленное исключение
    }

    private void DoCreateFile2(BRReport report, AbsPath filePath)
    {
      #region Начало

      //SharpZipLibLoader.LoadSharpZipLib();
      ZipFileCreator zf = new ZipFileCreator(filePath.Path);

      #endregion

      #region Заголовок ([Content_Types].xml)

      XmlDocument xmlDocCT = CreateContentTypesXml(report);
      zf.AddXmlFile("[Content_Types].xml", xmlDocCT);

      #endregion

      #region Связи (_rels/.rels)

      XmlDocument xmlDocRels1 = CreateRels1();
      zf.AddXmlFile("_rels/.rels", xmlDocRels1);

      XmlDocument xmlDocRels2 = CreateRels2(report);
      zf.AddXmlFile("word/_rels/document.xml.rels", xmlDocRels2);

      #endregion

      #region Сводка

      XmlDocument xmlDocCore = CreateDocPropsCoreXml(report.DocumentProperties);
      zf.AddXmlFile("docProps/core.xml", xmlDocCore);


      XmlDocument xmlDocApp = CreateDocPropsAppXml(report.DocumentProperties);
      zf.AddXmlFile("docProps/app.xml", xmlDocApp);

      #endregion

      #region Стили

      XmlDocument xmlStyles = CreateStylesXml(report);
      zf.AddXmlFile("word/styles.xml", xmlStyles);

      #endregion

      #region Document

      XmlDocument xmlDocument = CreateDocumentXml(report);
      zf.AddXmlFile("word/document.xml", xmlDocument);

      #endregion

      #region Конец

      zf.Close();

      #endregion
    }

    #endregion

    #region Вспомогательные файлы для OpenXML

    private static XmlDocument CreateContentTypesXml(BRReport report)
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
      SetAttr(elOverride, "PartName", "/word/document.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/word/styles.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/docProps/core.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-package.core-properties+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      elOverride = xmlDoc.CreateElement("Override", nmspcContentTypes);
      SetAttr(elOverride, "PartName", "/docProps/app.xml", String.Empty);
      SetAttr(elOverride, "ContentType", "application/vnd.openxmlformats-officedocument.extended-properties+xml", String.Empty);
      elRoot.AppendChild(elOverride);

      return xmlDoc;
    }

    private static XmlDocument CreateRels1()
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elRL;

      // Не знаю, почему порядок строк обратный. Excel так делает

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId3", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties", String.Empty);
      SetAttr(elRL, "Target", "docProps/app.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId2", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties", String.Empty);
      SetAttr(elRL, "Target", "docProps/core.xml", String.Empty);
      elRoot.AppendChild(elRL);

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId1", String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", String.Empty);
      SetAttr(elRL, "Target", "word/document.xml", String.Empty);
      elRoot.AppendChild(elRL);

      return xmlDoc;
    }

    private static XmlDocument CreateRels2(BRReport report)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("Relationships", nmspcPackageRels);
      SetAttr(elRoot, "xmlns", nmspcPackageRels, String.Empty);
      xmlDoc.AppendChild(elRoot);

      int cntRId = 0;
      XmlElement elRL;

      elRL = xmlDoc.CreateElement("Relationship", nmspcPackageRels);
      SetAttr(elRL, "Id", "rId" + StdConvert.ToString(++cntRId), String.Empty);
      SetAttr(elRL, "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", String.Empty);
      SetAttr(elRL, "Target", "styles.xml", String.Empty);
      elRoot.AppendChild(elRL);

      return xmlDoc;
    }

    #endregion

    #region styles.xml

    private const string nmspcMC = "http://schemas.openxmlformats.org/markup-compatibility/2006";
    private const string nmspcW = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    private XmlDocument CreateStylesXml(BRReport report)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("w:styles", nmspcW);
      SetAttr(elRoot, "xmlns:w", nmspcW, String.Empty);
      SetAttr(elRoot, "xmlns:mc", nmspcMC, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elDocDefaults = xmlDoc.CreateElement("w:docDefaults", nmspcW);
      elRoot.AppendChild(elDocDefaults);

      XmlElement elRPrDefault = xmlDoc.CreateElement("w:rPrDefault", nmspcW);
      elDocDefaults.AppendChild(elRPrDefault);

      XmlElement elRPr = xmlDoc.CreateElement("w:rPr", nmspcW);
      elRPrDefault.AppendChild(elRPr);

      XmlElement elRFonts = xmlDoc.CreateElement("w:rFonts", nmspcW);
      elRPr.AppendChild(elRFonts);
      SetAttr(elRFonts, "w:ascii", report.DefaultCellStyle.FontName, nmspcW);
      SetAttr(elRFonts, "w:eastAsia", report.DefaultCellStyle.FontName, nmspcW); // отличается от WordXML
      SetAttr(elRFonts, "w:hAnsi", report.DefaultCellStyle.FontName, nmspcW); // отличается от WordXML
      SetAttr(elRFonts, "w:cs", report.DefaultCellStyle.FontName, nmspcW);

      XmlElement elPPrDefault = xmlDoc.CreateElement("w:pPrDefault", nmspcW);
      elDocDefaults.AppendChild(elPPrDefault);
      XmlElement elPPr = xmlDoc.CreateElement("w:pPr", nmspcW);
      elPPrDefault.AppendChild(elPPr);
      XmlElement elSpacing = xmlDoc.CreateElement("w:spacing", nmspcW);
      elPPr.AppendChild(elSpacing);
      SetAttr(elSpacing, "w:before", "0", nmspcW);
      SetAttr(elSpacing, "w:after", "0", nmspcW);
      SetAttr(elSpacing, "w:lineRule", "auto", nmspcW);


      // Таблица стилей
      // Задаем только стили "Normal" и "Default Paragrapf Font ". Остальное форматирование стили не использует

      XmlElement elLatentStyles = xmlDoc.CreateElement("w:latentStyles", nmspcW);
      elRoot.AppendChild(elLatentStyles);
      SetAttr(elLatentStyles, "w:defLockedState", "0", nmspcW);
      SetAttr(elLatentStyles, "w:defUIPriority", "99", nmspcW);
      SetAttr(elLatentStyles, "w:defSemiHidden", "0", nmspcW);
      SetAttr(elLatentStyles, "w:defUnhideWhenUsed", "0", nmspcW);
      SetAttr(elLatentStyles, "w:defQFormat", "0", nmspcW);
      SetAttr(elLatentStyles, "w:count", "2", nmspcW);

      XmlElement elEx;
      elEx = xmlDoc.CreateElement("w:lsdException", nmspcW);
      elLatentStyles.AppendChild(elEx);
      SetAttr(elEx, "w:name", "Normal", nmspcW);
      SetAttr(elEx, "w:uiPriority", "0", nmspcW);
      SetAttr(elEx, "w:qFormat", "1", nmspcW);

      elEx = xmlDoc.CreateElement("w:lsdException", nmspcW);
      elLatentStyles.AppendChild(elEx);
      SetAttr(elEx, "w:name", "Default Paragraph Font", nmspcW);
      SetAttr(elEx, "w:semiHidden", "1", nmspcW);
      SetAttr(elEx, "w:uiPriority", "1", nmspcW);
      SetAttr(elEx, "w:unhideWhenUsed", "1", nmspcW);


      InitDefaultStyle(elRoot, report.DefaultCellStyle);

      return xmlDoc;
    }

    private static void InitDefaultStyle(XmlElement elStyles, BRCellStyle cellStyle)
    {
      XmlDocument xmlDoc = elStyles.OwnerDocument;

      #region Normal

      XmlElement elStyle1 = xmlDoc.CreateElement("w:style", nmspcW);
      elStyles.AppendChild(elStyle1);
      SetAttr(elStyle1, "w:type", "paragraph", nmspcW);
      SetAttr(elStyle1, "w:default", "1", nmspcW); // в WordXML - "on"
      SetAttr(elStyle1, "w:styleId", "a", nmspcW); // имя для ссылки на стиль. Чем короче, тем лучше

      XmlElement elName1 = xmlDoc.CreateElement("w:name", nmspcW);
      elStyle1.AppendChild(elName1);
      SetAttr(elName1, "w:val", "Normal", nmspcW); // имя для выбора из списка

      XmlElement elQFormat = xmlDoc.CreateElement("w:qFormat", nmspcW);
      elStyle1.AppendChild(elQFormat);

      XmlElement elPPr = xmlDoc.CreateElement("w:pPr", nmspcW);
      elStyle1.AppendChild(elPPr);

      XmlElement elSpacing = xmlDoc.CreateElement("w:spacing", nmspcW);
      elPPr.AppendChild(elSpacing);
      SetAttr(elSpacing, "w:before", "0", nmspcW);
      SetAttr(elSpacing, "w:after", "0", nmspcW);
      SetAttr(elSpacing, "w:lineRule", "auto", nmspcW);

      XmlElement elInd = xmlDoc.CreateElement("w:ind", nmspcW);
      elPPr.AppendChild(elInd);
      SetAttr(elInd, "w:left", "0", nmspcW);
      SetAttr(elInd, "w:right", "0", nmspcW);
      SetAttr(elInd, "w:firstLine", "0", nmspcW); // нет красной строки - отличается от WordXML

      XmlElement elRPr = xmlDoc.CreateElement("w:rPr", nmspcW);
      elStyle1.AppendChild(elRPr);

      XmlElement elRFonts = xmlDoc.CreateElement("w:rFonts", nmspcW);
      elRPr.AppendChild(elRFonts);
      SetAttr(elRFonts, "w:ascii", cellStyle.FontName, nmspcW);
      SetAttr(elRFonts, "w:eastAsia", cellStyle.FontName, nmspcW);
      SetAttr(elRFonts, "w:hAnsi", cellStyle.FontName, nmspcW);
      SetAttr(elRFonts, "w:cs", cellStyle.FontName, nmspcW);

      XmlElement elSz = xmlDoc.CreateElement("w:sz", nmspcW);
      elRPr.AppendChild(elSz);
      SetAttr(elSz, "w:val", StdConvert.ToString((int)(cellStyle.FontHeightTwip / 10)), nmspcW); // в полупунктах

      #endregion

      #region Default Paragraph Font

      XmlElement elStyle2 = xmlDoc.CreateElement("w:style", nmspcW);
      elStyles.AppendChild(elStyle2);
      SetAttr(elStyle2, "w:type", "character", nmspcW);
      SetAttr(elStyle2, "w:default", "1", nmspcW);
      SetAttr(elStyle2, "w:styleId", "a0", nmspcW);

      XmlElement elName2 = xmlDoc.CreateElement("w:name", nmspcW);
      elStyle2.AppendChild(elName2);
      SetAttr(elName2, "w:val", "Default Paragraph Font", nmspcW);

      XmlElement elUIPriority2 = xmlDoc.CreateElement("w:uiPriority", nmspcW);
      elStyle2.AppendChild(elUIPriority2);
      SetAttr(elUIPriority2, "w:val", "1", nmspcW);

      #endregion
    }

    #endregion

    #region document.xml

    private XmlDocument CreateDocumentXml(BRReport report)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);

      XmlElement elRoot = xmlDoc.CreateElement("w:document", nmspcW);
      SetAttr(elRoot, "xmlns:w", nmspcW, String.Empty);
      SetAttr(elRoot, "xmlns:mc", nmspcMC, String.Empty);
      xmlDoc.AppendChild(elRoot);

      XmlElement elBody = xmlDoc.CreateElement("w:body", nmspcW);
      elRoot.AppendChild(elBody);

      for (int i = 0; i < report.Sections.Count; i++)
      {
        BRSection section = report.Sections[i];

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
            elBody.AppendChild(elP);
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
              elBody.AppendChild(elP);
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
              WriteTextValue(sel, elBody, ref elP, section.PageSetup.PrintAreaWidth, true, bandKeepWithNext);
              elBody.AppendChild(elP);
            }
            else
            {
              XmlElement elTbl = xmlDoc.CreateElement("w:tbl", nmspcW);
              elBody.AppendChild(elTbl);

              XmlElement elTblPr = xmlDoc.CreateElement("w:tblPr", nmspcW);
              elTbl.AppendChild(elTblPr);

              XmlElement elTblW = xmlDoc.CreateElement("w:tblW", nmspcW);
              elTblPr.AppendChild(elTblW);
              SetAttr(elTblW, "w:w", "0", nmspcW);
              SetAttr(elTblW, "w:type", "auto", nmspcW);

              XmlElement elTblLayout = xmlDoc.CreateElement("w:tblLayout", nmspcW);
              elTblPr.AppendChild(elTblLayout);
              SetAttr(elTblLayout, "w:type", "fixed", nmspcW); // с маленькой буквы, в отличие от WordXML

              // Отступы по умолчанию
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
                  SetAttr(elTrHeight, "w:hRule", "exact", nmspcW); // отличается от wordxml
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
                      XmlElement elHmerge = xmlDoc.CreateElement("w:hMerge", nmspcW);
                      elTcPr.AppendChild(elHmerge);
                      SetAttr(elHmerge, "w:val", "restart", nmspcW);
                    }
                    if (merge.RowCount > 1)
                    {
                      XmlElement elVmerge = xmlDoc.CreateElement("w:vMerge", nmspcW);
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
                      XmlElement elHmerge = xmlDoc.CreateElement("w:hMerge", nmspcW);
                      elTcPr.AppendChild(elHmerge);
                    }
                    if (merge.RowCount > 1)
                    {
                      XmlElement elVmerge = xmlDoc.CreateElement("w:vMerge", nmspcW);
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

        // Для последнего раздела структура тегов: <w:body><w:sectPr>
        // Для предыдущих разделов создается параграф, а в нем уже свойства. То есть: <w:body><w:p><w:pPr><w:sectPr>
        XmlElement elSectPr = xmlDoc.CreateElement("w:sectPr", nmspcW);
        if (i == (report.Sections.Count - 1))
          elBody.AppendChild(elSectPr);
        else
        {
          XmlElement elP2 = xmlDoc.CreateElement("w:p", nmspcW);
          elBody.AppendChild(elP2);
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

      if (sel.CellStyle.TextFiller != BRTextFiller.None)
        return false;

      return
      sel.CellStyle.LeftBorder == BRLine.None &&
      sel.CellStyle.TopBorder == BRLine.None &&
      sel.CellStyle.RightBorder == BRLine.None &&
      sel.CellStyle.BottomBorder == BRLine.None &&
      sel.CellStyle.DiagonalUp == BRLine.None &&
      sel.CellStyle.DiagonalDown == BRLine.None;
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

        if (sel.CellStyle.TextFiller != BRTextFiller.None)
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
            DoWriteText(elP, aa[j], sel, columnWidth);
          }
        }
        else
        {
          DoWriteText(elP, a[i], sel, columnWidth);
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

      if (sel.CellStyle.TextFiller != BRTextFiller.None)
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

      if (sel.ActualHAlign != BRHAlign.Left && sel.CellStyle.TextFiller == BRTextFiller.None)
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
        SetAttr(elSpacing, "w:lineRule", "exact", nmspcW); // отличается от WordXml
      }

      #endregion

      #region Табуляции для TextFiller

      if (sel.CellStyle.TextFiller != BRTextFiller.None)
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
          SetAttr(elOne, "w:val", "dotDash", nmspcW); // отличается от WordXML
          break;
        case BRLineStyle.DashDotDot:
          SetAttr(elOne, "w:val", "dotDotDash", nmspcW); // отличается от WordXML
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

    #endregion
  }
}