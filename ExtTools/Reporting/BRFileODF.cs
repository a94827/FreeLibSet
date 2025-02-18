using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Shell;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Базовый класс для создание файла ODS и ODT
  /// </summary>
  public abstract class BRFileODFBase: BRFileCreator
  {
    #region Статическое свойство

    /// <summary>
    /// Возвращает true, если можно создать .ODS/ODT файл, то есть доступна библиотека для создания zip-архива.
    /// Наличие установленного OpenOffice не требуется.
    /// </summary>
    public static bool IsSupported { get { return ZipFileTools.ZipLibAvailable; } }

    #endregion

    #region Основной метод

    /// <summary>
    /// Создает ods-файл
    /// </summary>
    /// <param name="report"></param>
    /// <param name="filePath"></param>
    protected override void DoCreateFile(BRReport report, AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");
      ZipFileTools.CheckZipLibAvailable();
      CreateFile2(report, filePath);
    }

    private void CreateFile2(BRReport report, AbsPath filePath)
    {
      #region Начало

      //SharpZipLibLoader.LoadSharpZipLib();
      ZipFileCreator zf = new ZipFileCreator(filePath.Path);
      zf.AddMimeType(MimeType);

      #endregion

      #region Манифест (manifest.xml)

      XmlDocument xmlDocManifest = CreateManifest();
      zf.AddXmlFile("META-INF/manifest.xml", xmlDocManifest);

      #endregion

      #region Метаданные

      XmlDocument xmlDocMeta = CreateMeta(report);
      zf.AddXmlFile("meta.xml", xmlDocMeta);

      #endregion

      #region Основной документ и стили

      XmlDocument xmlDocCnt, xmlDocStl;
      CreateContent(report, out xmlDocCnt, out xmlDocStl);
      zf.AddXmlFile("content.xml", xmlDocCnt);
      zf.AddXmlFile("styles.xml", xmlDocStl);

      #endregion

      #region Конец

      zf.Close();

      //ZF.DebugOutFiles();

      #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    protected abstract string MimeType { get; }

    #endregion

    #region Манифест (manifest.xml)

    private XmlDocument CreateManifest()
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
      SetAttr(elFile, "manifest:media-type", MimeType, nmspcManifest);
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

      elFile = xmlDoc.CreateElement("manifest:file-entry", nmspcManifest);
      elRoot.AppendChild(elFile);
      SetAttr(elFile, "manifest:media-type", "text/xml", nmspcManifest);
      SetAttr(elFile, "manifest:full-path", "styles.xml", nmspcManifest);

      return xmlDoc;
    }

    #endregion

    #region Метаданные (meta.xml)

    private XmlDocument CreateMeta(BRReport report)
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

      BRDocumentProperties docProps = report.DocumentProperties;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="report"></param>
    /// <param name="xmlDocCnt"></param>
    /// <param name="xmlDocStl"></param>
    protected abstract void CreateContent(BRReport report, out XmlDocument xmlDocCnt, out XmlDocument xmlDocStl);

    #region Пространства имен

    internal const string nmspcOffice = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    internal const string nmspcMeta = "urn:oasis:names:tc:opendocument:xmlns:meta:1.0";
    internal const string nmspcDc = "http://purl.org/dc/elements/1.1/";
    internal const string nmspcSvg = "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0";
    internal const string nmspcStyle = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    internal const string nmspcText = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    internal const string nmspcTable = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    internal const string nmspcFo = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
    internal const string nmspcNumber = "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0";
    internal const string nmspcLoext = "urn:org:documentfoundation:names:experimental:office:xmlns:loext:1.0";

    #endregion


    #region Границы ячеек

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elCellProps"></param>
    /// <param name="attrName"></param>
    /// <param name="border"></param>
    /// <param name="nmspc"></param>
    protected static void SetBorders(XmlElement elCellProps, string attrName, BRLine border, string nmspc)
    {
      string value2, value3;
      switch (border.Style)
      {
        case BRLineStyle.Thin:
        case BRLineStyle.Medium:
        case BRLineStyle.Thick:
          value2 = "solid";
          break;
        case BRLineStyle.Dot:
          value2 = "dotted";
          break;
        case BRLineStyle.Dash:
          value2 = "dashed";
          break;
        case BRLineStyle.DashDot:
          value2 = "dash-dot";
          break;
        case BRLineStyle.DashDotDot:
          value2 = "dash-dot-dot";
          break;
        default:
          return;
      }

      string value1 = BRLine.GetLineWidthPt(border.Style).ToString("0.0", StdConvert.NumberFormat) + "pt";

      if (border.Color == BRColor.Auto)
        value3 = "#000000";
      else
        value3 = MyColorStr(border.Color);

      SetAttr(elCellProps, attrName, value1 + " " + value2 + " " + value3, nmspc);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    protected static string MyColorStr(BRColor c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
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

    #endregion
  }

  /// <summary>
  /// Создание документов ODS (Calc)
  /// </summary>
  public sealed class BRFileODS : BRFileODFBase
  {
    /// <summary>
    /// 
    /// </summary>
    protected override string MimeType { get { return "application/vnd.oasis.opendocument.spreadsheet"; } }

    #region Содержание и стили(content.xml и styles.xml)

    // В отличие от формата xlsx, все листы хранятся в одном файле
    /// <summary>
    /// 
    /// </summary>
    /// <param name="report"></param>
    /// <param name="xmlDocCnt"></param>
    /// <param name="xmlDocStl"></param>
    protected override void CreateContent(BRReport report, out XmlDocument xmlDocCnt, out XmlDocument xmlDocStl)
    {
      #region Создание XmlDocument

      xmlDocCnt = new XmlDocument();
      XmlDeclaration xmlDeclCnt = xmlDocCnt.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDocCnt.InsertBefore(xmlDeclCnt, xmlDocCnt.DocumentElement);

      xmlDocStl = new XmlDocument();
      XmlDeclaration xmlDeclStl = xmlDocStl.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDocStl.InsertBefore(xmlDeclStl, xmlDocStl.DocumentElement);

      #endregion

      #region Корневой узел

      XmlElement elRootCnt = xmlDocCnt.CreateElement("office:document-content", nmspcOffice);
      xmlDocCnt.AppendChild(elRootCnt);
      SetAttr(elRootCnt, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRootCnt, "xmlns:style", nmspcStyle, String.Empty);
      SetAttr(elRootCnt, "xmlns:text", nmspcText, String.Empty);
      SetAttr(elRootCnt, "xmlns:table", nmspcTable, String.Empty);
      SetAttr(elRootCnt, "xmlns:fo", nmspcFo, String.Empty);
      SetAttr(elRootCnt, "xmlns:number", nmspcNumber, String.Empty);
      SetAttr(elRootCnt, "xmlns:loext", nmspcLoext, String.Empty);
      SetAttr(elRootCnt, "office:version", "1.2", String.Empty);

      XmlElement elRootStl = xmlDocStl.CreateElement("office:document-styles", nmspcOffice);
      xmlDocStl.AppendChild(elRootStl);
      SetAttr(elRootStl, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRootStl, "xmlns:style", nmspcStyle, String.Empty);
      SetAttr(elRootStl, "xmlns:text", nmspcText, String.Empty);
      SetAttr(elRootStl, "xmlns:table", nmspcTable, String.Empty);
      SetAttr(elRootStl, "xmlns:fo", nmspcFo, String.Empty);
      SetAttr(elRootStl, "xmlns:number", nmspcNumber, String.Empty);
      SetAttr(elRootStl, "xmlns:svg", nmspcSvg, String.Empty);
      SetAttr(elRootStl, "office:version", "1.2", nmspcOffice);

      #endregion

      #region Шрифты

      // Шрифты присутствуют и в content.xml и в styles.xml

      XmlElement elFontFaceDeclsCnt = xmlDocCnt.CreateElement("office:font-face-decls", nmspcOffice);
      elRootCnt.AppendChild(elFontFaceDeclsCnt);

      XmlElement elFontFaceDeclsStl = xmlDocStl.CreateElement("office:font-face-decls", nmspcOffice);
      elRootStl.AppendChild(elFontFaceDeclsStl);

      string[] aFontNames = report.GetFontNames();
      for (int i = 0; i < aFontNames.Length; i++)
      {
        XmlElement elFontFaceCnt = xmlDocCnt.CreateElement("style:font-face", nmspcStyle);
        elFontFaceDeclsCnt.AppendChild(elFontFaceCnt);
        SetAttr(elFontFaceCnt, "style:name", aFontNames[i], nmspcStyle);
        SetAttr(elFontFaceCnt, "svg:font-family", "\'" + aFontNames[i] + "\'", nmspcSvg);

        XmlElement elFontFaceStl = xmlDocStl.CreateElement("style:font-face", nmspcStyle);
        elFontFaceDeclsStl.AppendChild(elFontFaceStl);
        SetAttr(elFontFaceStl, "style:name", aFontNames[i], nmspcStyle);
        SetAttr(elFontFaceStl, "svg:font-family", "\'" + aFontNames[i] + "\'", nmspcSvg);
      }

      #endregion

      #region Узлы списков стилей

      XmlElement elAutoStylesCnt = xmlDocCnt.CreateElement("office:automatic-styles", nmspcOffice);
      elRootCnt.AppendChild(elAutoStylesCnt); // будем заполнять по мере необходимости

      //XmlElement elUnusedStylesStl = xmlDocStl.CreateElement("office:styles", nmspcOffice);
      //elRootStl.AppendChild(elUnusedStylesStl);

      XmlElement elAutoStylesStl = xmlDocStl.CreateElement("office:automatic-styles", nmspcOffice);
      elRootStl.AppendChild(elAutoStylesStl);

      XmlElement elMasterStyles = xmlDocStl.CreateElement("office:master-styles", nmspcOffice);
      elRootStl.AppendChild(elMasterStyles);

      // Нумерация стилей является общей для всех листов
      int co_styleCount = 0; // стили столбцов
      SingleScopeList<int> ro_styles = new SingleScopeList<int>(); // стили строк (хранится высота строки)
      Dictionary<string, int> ce_styles = new Dictionary<string, int>(); // стили ячеек. Ключ-коллекции-условная строка-ключ для поиска стиля, значение - номер стиля 1,2,3,...
      StringBuilder sb = new StringBuilder(); // для сбора ключа

      #endregion

      #region Body

      XmlElement elBody = xmlDocCnt.CreateElement("office:body", nmspcOffice);
      elRootCnt.AppendChild(elBody);

      XmlElement elSpreadSheet = xmlDocCnt.CreateElement("office:spreadsheet", nmspcOffice); // только один тег на весь документ
      elBody.AppendChild(elSpreadSheet);

      #endregion

      for (int i = 0; i < report.Sections.Count; i++)
      {
        BRSection section = report.Sections[i];

        #region Стиль страницы

        // Для каждой страницы надо создать отдельный стиль
        XmlElement elPageLayout = xmlDocStl.CreateElement("style:page-layout", nmspcStyle);
        elAutoStylesStl.AppendChild(elPageLayout);
        SetAttr(elPageLayout, "style:name", "Mpm" + StdConvert.ToString(i + 1), nmspcStyle);

        XmlElement elPLP = xmlDocStl.CreateElement("style:page-layout-properties", nmspcStyle);
        elPageLayout.AppendChild(elPLP);

        SetAttr(elPLP, "fo:page-width", (section.PageSetup.PaperWidth / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:page-height", (section.PageSetup.PaperHeight / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        if (section.PageSetup.Orientation == BROrientation.Landscape)
          SetAttr(elPLP, "style:print-orientation", "landscape", nmspcStyle);
        SetAttr(elPLP, "fo:margin-left", (section.PageSetup.LeftMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:margin-top", (section.PageSetup.TopMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:margin-right", (section.PageSetup.RightMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:margin-bottom", (section.PageSetup.BottomMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);

        XmlElement elMasterPage = xmlDocStl.CreateElement("style:master-page", nmspcStyle);
        elMasterStyles.AppendChild(elMasterPage);
        SetAttr(elMasterPage, "style:name", "Page" + StdConvert.ToString(i + 1), nmspcStyle);
        SetAttr(elMasterPage, "style:page-layout-name", "Mpm" + StdConvert.ToString(i + 1), nmspcStyle);

        #endregion

        #region Стиль таблицы

        XmlElement elTableStyle = xmlDocCnt.CreateElement("style:style", nmspcStyle);
        elAutoStylesCnt.AppendChild(elTableStyle);
        SetAttr(elTableStyle, "style:name", "ta" + StdConvert.ToString(i + 1), nmspcStyle);
        SetAttr(elTableStyle, "style:family", "table", nmspcStyle);
        SetAttr(elTableStyle, "style:master-page-name", "Page" + StdConvert.ToString(i + 1), nmspcStyle);

        XmlElement elTableProps = xmlDocCnt.CreateElement("style:table-properties", nmspcStyle);
        elTableStyle.AppendChild(elTableProps);
        //SetAttr(elTableProps, "style:width", (section.PageSetup.PrintAreaWidth / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcStyle);
        SetAttr(elTableProps, "table:display", "true", nmspcTable);

        #endregion

        XmlElement elTable = xmlDocCnt.CreateElement("table:table", nmspcTable);
        elSpreadSheet.AppendChild(elTable);
        SetAttr(elTable, "table:name", section.Name, nmspcTable);
        SetAttr(elTable, "table:style-name", "ta" + StdConvert.ToString(i + 1), nmspcTable);
        BRColumnBoundList colBounds = new BRColumnBoundList(section, 10, 0);

        #region Объявления столбцов

        // В отличие от MS Excel, ширина столбцов задается в стилях
        for (int l = 0; l < colBounds.WidthArray.Length; l++)
        {
          co_styleCount++;
          XmlElement elColStyle = xmlDocCnt.CreateElement("style:style", nmspcStyle);
          elAutoStylesCnt.AppendChild(elColStyle);
          SetAttr(elColStyle, "style:name", "co" + StdConvert.ToString(co_styleCount), nmspcStyle);
          SetAttr(elColStyle, "style:family", "table-column", nmspcStyle);

          XmlElement elColProps = xmlDocCnt.CreateElement("style:table-column-properties", nmspcStyle);
          elColStyle.AppendChild(elColProps);
          double w = colBounds.WidthArray[l] / 100.0;
          SetAttr(elColProps, "style:column-width", w.ToString("0.000", StdConvert.NumberFormat) + "cm", nmspcStyle);


          XmlElement elColumn = xmlDocCnt.CreateElement("table:table-column", nmspcTable);
          elTable.AppendChild(elColumn);
          SetAttr(elColumn, "table:style-name", "co" + StdConvert.ToString(co_styleCount), nmspcTable);
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
            XmlElement elGapRow = xmlDocCnt.CreateElement("table:table-row", nmspcTable);
            elTable.AppendChild(elGapRow);
            SetAttr(elGapRow, "table:style-name", Get_ro_style(ro_styles, elAutoStylesCnt, gap), nmspcTable); // 17.07.2019
          }

          #endregion

          BRBand band = section.Bands[j];

          int[] leftCols = colBounds.LeftColumns[j];
          int[] rightCols = colBounds.RightColumns[j];

          BRSelector sel = band.CreateSelector();
          for (int k = 0; k < band.RowCount; k++)
          {
            sel.RowIndex = k;
            XmlElement elRow = xmlDocCnt.CreateElement("table:table-row", nmspcTable);
            elTable.AppendChild(elRow);
            SetAttr(elRow, "table:style-name", Get_ro_style(ro_styles, elAutoStylesCnt, sel.RowInfo.Height), nmspcTable);

            for (int l = 0; l < band.ColumnCount; l++)
            {
              sel.ColumnIndex = l;

              BRRange merge = sel.MergeInfo;

              if (merge.FirstRowIndex == sel.RowIndex && merge.FirstColumnIndex == sel.ColumnIndex) // первая ячейка объединения или обычная ячейка
              {
                XmlElement elCell = xmlDocCnt.CreateElement("table:table-cell", nmspcTable);
                elRow.AppendChild(elCell);

                #region Форматирование

                BRFileTools.InitCellStyleKey(sb, sel, 0);

                int styleKeyNum;
                if (!ce_styles.TryGetValue(sb.ToString(), out styleKeyNum))
                {
                  #region Добавление стиля

                  styleKeyNum = ce_styles.Count + 1;
                  ce_styles.Add(sb.ToString(), styleKeyNum);

                  // Сначала добавляем формат для чисел
                  string nFormatName = "N" + StdConvert.ToString(styleKeyNum);
                  if (!OpenOfficeTools.ODFAddFormat(elAutoStylesCnt, sel.CellStyle.Format, nFormatName,
                    sel.CellStyle.NumberFormat, sel.CellStyle.DateTimeFormat, String.Empty, String.Empty))
                    nFormatName = null;

                  XmlElement elStyle = xmlDocCnt.CreateElement("style:style", nmspcStyle);
                  elAutoStylesCnt.AppendChild(elStyle);
                  SetAttr(elStyle, "style:name", "ce" + StdConvert.ToString(styleKeyNum), nmspcStyle);
                  SetAttr(elStyle, "style:family", "table-cell", nmspcStyle);

                  if (nFormatName != null)
                    SetAttr(elStyle, "style:data-style-name", nFormatName, nmspcStyle);

                  XmlElement elCellProps = xmlDocCnt.CreateElement("style:table-cell-properties", nmspcStyle);
                  elStyle.AppendChild(elCellProps);

                  XmlElement elParProps = xmlDocCnt.CreateElement("style:paragraph-properties", nmspcStyle);
                  elStyle.AppendChild(elParProps);

                  SetAttr(elParProps, "fo:line-height", sel.CellStyle.LineHeightPt.ToString("0.0", StdConvert.NumberFormat) + "pt", nmspcFo);

                  if (sel.CellStyle.AreaAllBordersSame)
                    // Все границы одинаковые
                    SetBorders(elCellProps, "fo:border", sel.CellStyle.LeftBorder, nmspcFo);
                  else
                  {
                    // Границы разные
                    SetBorders(elCellProps, "fo:border-left", sel.CellStyle.LeftBorder, nmspcFo);
                    SetBorders(elCellProps, "fo:border-top", sel.CellStyle.TopBorder, nmspcFo);
                    SetBorders(elCellProps, "fo:border-right", sel.CellStyle.RightBorder, nmspcFo);
                    SetBorders(elCellProps, "fo:border-bottom", sel.CellStyle.BottomBorder, nmspcFo);
                  }

                  SetBorders(elCellProps, "style:diagonal-bl-tr", sel.CellStyle.DiagonalUp, nmspcStyle);
                  SetBorders(elCellProps, "style:diagonal-tl-br", sel.CellStyle.DiagonalDown, nmspcStyle);

                  string ha, va;
                  switch (sel.ActualHAlign)
                  {
                    //case BRHAlign.Auto: ha = ""; break;
                    case BRHAlign.Left: ha = "start"; break;
                    case BRHAlign.Center: ha = "center"; break;
                    case BRHAlign.Right: ha = "end"; break;
                    default: throw new BugException("Unknown horizontal alignment");
                  }
                  if (ha.Length > 0)
                    SetAttr(elParProps, "fo:text-align", ha, nmspcFo);

                  switch (sel.CellStyle.VAlign)
                  {
                    case BRVAlign.Top: va = "top"; break;
                    case BRVAlign.Center: va = "middle"; break;
                    case BRVAlign.Bottom: va = "bottom"; break;
                    default: throw new BugException("Unknown vertical alignment");
                  }
                  SetAttr(elCellProps, "style:vertical-align", va, nmspcStyle);

                  if (sel.CellStyle.WrapMode != BRWrapMode.NoWrap)
                    SetAttr(elCellProps, "fo:wrap-option", "wrap", nmspcFo);

                  if (sel.CellStyle.IndentLevel > 0)
                  {
                    if (ha == "start")
                      SetAttr(elParProps, "fo:margin-left", (0.3 * sel.CellStyle.IndentLevel).ToString("0.000", StdConvert.NumberFormat) + "cm", nmspcFo);
                    if (ha == "end") // наверное, не будет работать
                      SetAttr(elParProps, "fo:margin-right", (0.3 * sel.CellStyle.IndentLevel).ToString("0.000", StdConvert.NumberFormat) + "cm", nmspcFo);
                  }


                  XmlElement elTextProps = xmlDocCnt.CreateElement("style:text-properties", nmspcStyle);
                  elStyle.AppendChild(elTextProps);

                  SetAttr(elTextProps, "style:font-name", sel.CellStyle.FontName, nmspcStyle);
                  SetAttr(elTextProps, "fo:font-size", sel.CellStyle.FontHeightPt.ToString("0.0", StdConvert.NumberFormat) + "pt", nmspcFo);
                  if (sel.CellStyle.FontWidthPercent != 100)
                    SetAttr(elTextProps, "style:text-scale", StdConvert.ToString(sel.CellStyle.FontWidthPercent) + "%", nmspcStyle);

                  if (sel.CellStyle.Bold)
                    SetAttr(elTextProps, "fo:font-weight", "bold", nmspcFo);
                  if (sel.CellStyle.Italic)
                    SetAttr(elTextProps, "fo:font-style", "italic", nmspcFo);
                  if (sel.CellStyle.Underline)
                    SetAttr(elTextProps, "style:text-underline-style", "solid", nmspcStyle);
                  if (sel.CellStyle.Strikeout)
                    SetAttr(elTextProps, "style:text-line-through-style", "solid", nmspcStyle);
                  if (sel.CellStyle.ForeColor != BRColor.Auto)
                    SetAttr(elTextProps, "fo:color", MyColorStr(sel.CellStyle.ForeColor), nmspcFo);

                  if (sel.CellStyle.BackColor != BRColor.Auto)
                    SetAttr(elCellProps, "fo:background-color", MyColorStr(sel.CellStyle.BackColor), nmspcFo);

                  #endregion
                }

                SetAttr(elCell, "table:style-name", "ce" + StdConvert.ToString(styleKeyNum), nmspcTable);

                #endregion

                #region Значение ячейки

                #region Текстовое представление

                string s = sel.AsString;

                if (s.Length > 0)
                {
                  string[] a = s.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
                  for (int m = 0; m < a.Length; m++)
                  {
                    XmlElement elP = xmlDocCnt.CreateElement("text:p", nmspcText);
                    elCell.AppendChild(elP);
                    s = a[m];
                    s = DataTools.ReplaceAny(s, BRFileTools.BadValueChars, ' ');
                    XmlText txt = xmlDocCnt.CreateTextNode(s);
                    elP.AppendChild(txt);
                  }
                }

                #endregion

                #region Типизированное значение

                object cellValue = sel.Value;
                if (cellValue != null)
                {
                  if (cellValue is string)
                    SetAttr(elCell, "office:value-type", "string", nmspcOffice);
                  else if (cellValue is Boolean)
                  {
                    SetAttr(elCell, "office:value-type", "boolean", nmspcOffice);
                    SetAttr(elCell, "office:boolean-value", (bool)(cellValue) ? "true" : "false", nmspcOffice);
                  }
                  else if (cellValue is DateTime)
                  {
                    // Теоретически существует еще тип time, но реально везде используется date
                    DateTime dt = (DateTime)cellValue;
                    SetAttr(elCell, "office:value-type", "date", nmspcOffice);
                    SetAttr(elCell, "office:date-value", dt.ToString("s"), nmspcOffice);
                  }
                  else if (DataTools.IsIntegerType(cellValue.GetType()) || DataTools.IsFloatType(cellValue.GetType()))
                  {
                    SetAttr(elCell, "office:value-type", "float", nmspcOffice);
                    SetAttr(elCell, "office:value", Convert.ToString(cellValue, StdConvert.NumberFormat), nmspcOffice);
                  }
                }

                #endregion

                #endregion

                #region Объединение ячеек

                int xlsColumn1 = leftCols[merge.FirstColumnIndex];
                int xlsColumn2 = rightCols[merge.LastColumnIndex];


                if (merge.RowCount > 1 || xlsColumn2 > xlsColumn1)
                {
                  SetAttr(elCell, "table:number-columns-spanned", StdConvert.ToString(xlsColumn2 - xlsColumn1 + 1), nmspcTable);
                  SetAttr(elCell, "table:number-rows-spanned", StdConvert.ToString(merge.RowCount), nmspcTable);

                  int xlsColumn3 = rightCols[merge.FirstColumnIndex];
                  for (int m = xlsColumn1 + 1; m <= xlsColumn3; m++)
                  {
                    XmlElement elCoveredCell = xmlDocCnt.CreateElement("table:covered-table-cell", nmspcTable);
                    elRow.AppendChild(elCoveredCell);
                  }
                }

                #endregion
              } // IsMainCell
              else
              {
                // не первая ячейка объединения
                int xlsColumn1 = leftCols[l];
                int xlsColumn2 = rightCols[l];
                for (int m = xlsColumn1; m <= xlsColumn2; m++)
                {
                  XmlElement elCoveredCell = xmlDocCnt.CreateElement("table:covered-table-cell", nmspcTable);
                  elRow.AppendChild(elCoveredCell);
                }
              }
            } // цикл по столбцам
          } // цикл по строкам
        } // цикл по полосам

        #endregion
      }
    }

    private static string Get_ro_style(SingleScopeList<int> ro_styles, XmlElement elAutoStyles, int h)
    {
      int p = ro_styles.IndexOf(h);
      if (p < 0)
      {
        XmlElement elRowStyle = elAutoStyles.OwnerDocument.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elRowStyle);
        SetAttr(elRowStyle, "style:name", "ro" + StdConvert.ToString(ro_styles.Count + 1), nmspcStyle);
        SetAttr(elRowStyle, "style:family", "table-row", nmspcStyle);

        XmlElement elTRP = elAutoStyles.OwnerDocument.CreateElement("style:table-row-properties", nmspcStyle);
        elRowStyle.AppendChild(elTRP);
        if (h == 0)
          SetAttr(elTRP, "style:use-optimal-row-height", "true", nmspcStyle);
        else
          SetAttr(elTRP, "style:row-height", (h / 100.0).ToString("0.000", StdConvert.NumberFormat) + "cm", nmspcStyle);

        ro_styles.Add(h);
        p = ro_styles.Count - 1;
      }
      return "ro" + StdConvert.ToString(p + 1);
    }


    #endregion
  }

  /// <summary>
  /// Создание документов ODT (Writer)
  /// </summary>
  public sealed class BRFileODT : BRFileODFBase
  {
    #region Константы

    private const int MinFontWidthPercent = 1;
    private const int MaxFontWidthPercent = 999;

    #endregion

    #region Конструктор

    /// <summary>
    /// 
    /// </summary>
    /// <param name="measurer"></param>
    public BRFileODT(IBRMeasurer measurer)
    {
      if (measurer == null)
        throw new ArgumentNullException("measurer");
      _Measurer = measurer;
    }

    #endregion

    private IBRMeasurer _Measurer;

    /// <summary>
    /// 
    /// </summary>
    protected override string MimeType { get { return "application/vnd.oasis.opendocument.text"; } }

    #region Содержание и стили(content.xml и styles.xml)

    StringBuilder _SB;

    /// <summary>
    /// Стили параграфов. 
    /// Ключ словаря - условная строка-ключ для поиска стиля.
    /// Значение - имя стиля "P1", "P2, "P3",...
    /// </summary>
    Dictionary<string, string> _P_styles;

    /// <summary>
    /// Стили ячеек. 
    /// Ключ словаря - условная строка-ключ для поиска стиля.
    /// Значение - имя стиля "ce1", "ce2, "ce3",...
    /// </summary>
    Dictionary<string, string> _CE_styles;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="report"></param>
    /// <param name="xmlDocCnt"></param>
    /// <param name="xmlDocStl"></param>
    protected override void CreateContent(BRReport report, out XmlDocument xmlDocCnt, out XmlDocument xmlDocStl)
    {
      #region Создание XmlDocument

      xmlDocCnt = new XmlDocument();
      XmlDeclaration xmlDeclCnt = xmlDocCnt.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDocCnt.InsertBefore(xmlDeclCnt, xmlDocCnt.DocumentElement);

      xmlDocStl = new XmlDocument();
      XmlDeclaration xmlDeclStl = xmlDocStl.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
      xmlDocStl.InsertBefore(xmlDeclStl, xmlDocStl.DocumentElement);

      #endregion

      #region Корневой узел

      XmlElement elRootCnt = xmlDocCnt.CreateElement("office:document-content", nmspcOffice);
      xmlDocCnt.AppendChild(elRootCnt);
      SetAttr(elRootCnt, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRootCnt, "xmlns:style", nmspcStyle, String.Empty);
      SetAttr(elRootCnt, "xmlns:text", nmspcText, String.Empty);
      SetAttr(elRootCnt, "xmlns:table", nmspcTable, String.Empty);
      SetAttr(elRootCnt, "xmlns:fo", nmspcFo, String.Empty);
      SetAttr(elRootCnt, "xmlns:number", nmspcNumber, String.Empty);
      SetAttr(elRootCnt, "xmlns:loext", nmspcLoext, String.Empty);
      SetAttr(elRootCnt, "office:version", "1.2", String.Empty);

      XmlElement elRootStl = xmlDocStl.CreateElement("office:document-styles", nmspcOffice);
      xmlDocStl.AppendChild(elRootStl);
      SetAttr(elRootStl, "xmlns:office", nmspcOffice, String.Empty);
      SetAttr(elRootStl, "xmlns:style", nmspcStyle, String.Empty);
      SetAttr(elRootStl, "xmlns:text", nmspcText, String.Empty);
      SetAttr(elRootStl, "xmlns:table", nmspcTable, String.Empty);
      SetAttr(elRootStl, "xmlns:fo", nmspcFo, String.Empty);
      SetAttr(elRootStl, "xmlns:number", nmspcNumber, String.Empty);
      SetAttr(elRootStl, "xmlns:svg", nmspcSvg, String.Empty);
      SetAttr(elRootStl, "office:version", "1.2", nmspcOffice);

      #endregion

      #region Шрифты

      // Шрифты присутствуют и в content.xml и в styles.xml

      XmlElement elFontFaceDeclsCnt = xmlDocCnt.CreateElement("office:font-face-decls", nmspcOffice);
      elRootCnt.AppendChild(elFontFaceDeclsCnt);

      XmlElement elFontFaceDeclsStl = xmlDocStl.CreateElement("office:font-face-decls", nmspcOffice);
      elRootStl.AppendChild(elFontFaceDeclsStl);

      string[] aFontNames = report.GetFontNames();
      for (int i = 0; i < aFontNames.Length; i++)
      {
        XmlElement elFontFaceCnt = xmlDocCnt.CreateElement("style:font-face", nmspcStyle);
        elFontFaceDeclsCnt.AppendChild(elFontFaceCnt);
        SetAttr(elFontFaceCnt, "style:name", aFontNames[i], nmspcStyle);
        SetAttr(elFontFaceCnt, "svg:font-family", "\'" + aFontNames[i] + "\'", nmspcSvg);

        XmlElement elFontFaceStl = xmlDocStl.CreateElement("style:font-face", nmspcStyle);
        elFontFaceDeclsStl.AppendChild(elFontFaceStl);
        SetAttr(elFontFaceStl, "style:name", aFontNames[i], nmspcStyle);
        SetAttr(elFontFaceStl, "svg:font-family", "\'" + aFontNames[i] + "\'", nmspcSvg);
      }

      #endregion

      #region Узлы списков стилей

      XmlElement elAutoStylesCnt = xmlDocCnt.CreateElement("office:automatic-styles", nmspcOffice);
      elRootCnt.AppendChild(elAutoStylesCnt); // будем заполнять по мере необходимости

      //XmlElement elUnusedStylesStl = xmlDocStl.CreateElement("office:styles", nmspcOffice);
      //elRootStl.AppendChild(elUnusedStylesStl);

      XmlElement elAutoStylesStl = xmlDocStl.CreateElement("office:automatic-styles", nmspcOffice);
      elRootStl.AppendChild(elAutoStylesStl);

      XmlElement elMasterStyles = xmlDocStl.CreateElement("office:master-styles", nmspcOffice);
      elRootStl.AppendChild(elMasterStyles);

      // Нумерация стилей является общей для всех листов
      int tableStyleCount = 0; // стили таблицы
      _P_styles = new Dictionary<string, string>();
      _CE_styles = new Dictionary<string, string>();
      _SB = new StringBuilder();
      Dictionary<int, string> gap_styles = new Dictionary<int, string>(); // стили для зазоров между полосами

      #endregion

      #region Body

      XmlElement elBody = xmlDocCnt.CreateElement("office:body", nmspcOffice);
      elRootCnt.AppendChild(elBody);

      XmlElement elText = xmlDocCnt.CreateElement("office:text", nmspcOffice); // только один тег на весь документ
      elBody.AppendChild(elText);

      #endregion

      for (int i = 0; i < report.Sections.Count; i++)
      {
        BRSection section = report.Sections[i];

        #region Стиль страницы

        // Для каждой страницы надо создать отдельный стиль
        string pageStyleName = "Page" + StdConvert.ToString(i + 1);
        XmlElement elStyle = xmlDocStl.CreateElement("style:page-layout", nmspcStyle);
        elAutoStylesStl.AppendChild(elStyle);
        SetAttr(elStyle, "style:name", "Mpm" + StdConvert.ToString(i + 1), nmspcStyle);

        BRPageSetup ps = section.PageSetup;

        XmlElement elPLP = xmlDocStl.CreateElement("style:page-layout-properties", nmspcStyle);
        elStyle.AppendChild(elPLP);

        SetAttr(elPLP, "fo:page-width", (ps.PaperWidth / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:page-height", (ps.PaperHeight / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "style:print-orientation", ps.Orientation == BROrientation.Landscape ? "landscape" : "portrait", nmspcStyle);
        SetAttr(elPLP, "fo:margin-top", (ps.TopMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:margin-bottom", (ps.BottomMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:margin-left", (ps.LeftMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
        SetAttr(elPLP, "fo:margin-right", (ps.RightMargin / 100f).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);

        XmlElement elMasterPage = xmlDocStl.CreateElement("style:master-page", nmspcStyle);
        elMasterStyles.AppendChild(elMasterPage);

        SetAttr(elMasterPage, "style:name", pageStyleName, nmspcStyle);
        SetAttr(elMasterPage, "style:page-layout-name", "Mpm" + StdConvert.ToString(i + 1), nmspcStyle);

        #endregion

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
            XmlElement elP = xmlDocCnt.CreateElement("text:p", nmspcText);
            elText.AppendChild(elP);

            string gapStyleName;
            if (!gap_styles.TryGetValue(gap, out gapStyleName))
            {
              gapStyleName = "gap_" + StdConvert.ToString(gap);
              gap_styles.Add(gap, gapStyleName);

              XmlElement elGapStyle = elAutoStylesCnt.OwnerDocument.CreateElement("style:style", nmspcStyle);
              elAutoStylesCnt.AppendChild(elGapStyle);
              SetAttr(elGapStyle, "style:name", gapStyleName, nmspcStyle);
              SetAttr(elGapStyle, "style:family", "paragraph", nmspcStyle);

              XmlElement elTextProps = elGapStyle.OwnerDocument.CreateElement("style:text-properties", nmspcStyle);
              elGapStyle.AppendChild(elTextProps);

              int gapPt2 = (int)(gap * 72.0 / 254.0 * 2);
              SetAttr(elTextProps, "fo:font-size", gapPt2.ToString("0.0", StdConvert.NumberFormat) + "pt", nmspcFo);
            }
            SetAttr(elP, "text:style-name", gapStyleName, nmspcText);

            prevBandIsSimple = true;
          }

          #endregion

          BRBand band = section.Bands[j];
          BRSelector sel = band.CreateSelector();
          BRStripeItem[][] aaCols = BRPaginator.PaginateBandColumns(band);

          for (int m = 0; m < aaCols.Length; m++)
          {
            BRStripeItem[] stripeCols = aaCols[m];

            bool thisBandIsSimple = IsSimpleBand(sel);
            if (thisBandIsSimple)
            {
              #region Простой текст

              bool bandKeepWithNext = sel.Band.KeepWithNext || GetBandKeepWithPrev(section, j + 1);

              XmlElement elP = xmlDocCnt.CreateElement("text:p", nmspcText);
              elText.AppendChild(elP);
              WriteTextValue(sel, elP, true);
              InitPStyle(elP, sel, elAutoStylesCnt, section.PageSetup.PrintAreaWidth, pageStyleName, bandKeepWithNext);
              pageStyleName = String.Empty;

              #endregion
            }
            else
            {
              #region Таблица

              #region Стиль таблицы

              tableStyleCount++;
              XmlElement elTableStyle = xmlDocCnt.CreateElement("style:style", nmspcStyle);
              elAutoStylesCnt.AppendChild(elTableStyle);
              SetAttr(elTableStyle, "style:name", "Table" + StdConvert.ToString(tableStyleCount), nmspcStyle);
              SetAttr(elTableStyle, "style:family", "table", nmspcStyle);
              if (!String.IsNullOrEmpty(pageStyleName))
                SetAttr(elTableStyle, "style:master-page-name", pageStyleName, nmspcStyle);
              pageStyleName = String.Empty;


              XmlElement elTableProps = xmlDocCnt.CreateElement("style:table-properties", nmspcStyle);
              elTableStyle.AppendChild(elTableProps);
              SetAttr(elTableProps, "style:width", (BRStripeItem.GetTotalSize(stripeCols) / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcStyle);
              SetAttr(elTableProps, "table:border-model", "collapsing", nmspcTable);
              if (m > 0)
              {
                SetAttr(elTableProps, "fo:break-before", "page", nmspcFo); // разрыв между полосами одной таблицы
                prevBandIsSimple = true; // можно будет использовать строки-заголовки
              }

              #endregion

              #region Таблица

              XmlElement elTable = xmlDocCnt.CreateElement("table:table", nmspcTable);
              elText.AppendChild(elTable);
              SetAttr(elTable, "table:name", "Table " + StdConvert.ToString(tableStyleCount), nmspcTable);
              SetAttr(elTable, "table:style-name", "Table" + StdConvert.ToString(tableStyleCount), nmspcTable);

              XmlElement elTableHeaderRows = null; // Создадим при необходимости

              #endregion

              #region Объявления столбцов

              // У каждого столбца - свой собственный стиль, где задается ширина

              for (int l = 0; l < stripeCols.Length; l++)
              {
                XmlElement elColStyle = xmlDocCnt.CreateElement("style:style", nmspcStyle);
                elAutoStylesCnt.AppendChild(elColStyle);
                string colStyleName = "co" + StdConvert.ToString(tableStyleCount) + "_" + StdConvert.ToString(l + 1);
                SetAttr(elColStyle, "style:name", colStyleName, nmspcStyle);
                SetAttr(elColStyle, "style:family", "table-column", nmspcStyle);

                XmlElement elColProps = xmlDocCnt.CreateElement("style:table-column-properties", nmspcStyle);
                elColStyle.AppendChild(elColProps);
                SetAttr(elColProps, "style:column-width", (stripeCols[l].Size / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcStyle);

                XmlElement elColumn = xmlDocCnt.CreateElement("table:table-column", nmspcTable);
                elTable.AppendChild(elColumn);
                SetAttr(elColumn, "table:style-name", colStyleName, nmspcTable);
              }

              #endregion

              // Строки
              bool isRepeatableRow = prevBandIsSimple;
              for (int k = 0; k < band.RowCount; k++)
              {
                sel.RowIndex = k;

                XmlElement elRowStyle = xmlDocCnt.CreateElement("style:style", nmspcStyle);
                elAutoStylesCnt.AppendChild(elRowStyle);
                string rowStyleName = "ro" + StdConvert.ToString(tableStyleCount) + "_" + StdConvert.ToString(k + 1);
                SetAttr(elRowStyle, "style:name", rowStyleName, nmspcStyle);
                SetAttr(elRowStyle, "style:family", "table-row", nmspcStyle);

                XmlElement elRowProps = xmlDocCnt.CreateElement("style:table-row-properties", nmspcStyle);
                elRowStyle.AppendChild(elRowProps);
                if (sel.RowInfo.Height != BRReport.AutoRowHeight)
                  SetAttr(elRowProps, "style:row-height", (sel.RowInfo.Height / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcStyle);
                SetAttr(elRowProps, "fo:keep-together", "always", nmspcFo);

                XmlElement elRowParentElement = elTable;
                if (isRepeatableRow)
                {
                  if (sel.RowInfo.Repeatable)
                  {
                    if (elTableHeaderRows == null)
                    {
                      elTableHeaderRows = xmlDocCnt.CreateElement("table:table-header-rows", nmspcTable);
                      elTable.AppendChild(elTableHeaderRows);
                    }
                    elRowParentElement = elTableHeaderRows;
                  }
                  else
                    isRepeatableRow = false; // нельзя делать повторяющиеся строки, если они не вверху таблицы
                }


                XmlElement elRow = xmlDocCnt.CreateElement("table:table-row", nmspcTable);
                elRowParentElement.AppendChild(elRow); 
                SetAttr(elRow, "table:style-name", rowStyleName, nmspcTable);


                bool rowKeepWithNext = sel.RowInfo.KeepWithNext || GetNextRowKeepWithPrev(sel);

                //  // Столбцы
                for (int l = 0; l < stripeCols.Length; l++)
                {
                  sel.ColumnIndex = stripeCols[l].Index;

                  BRRange merge = sel.MergeInfo;

                  if (merge.FirstRowIndex == sel.RowIndex && merge.FirstColumnIndex == sel.ColumnIndex) // первая ячейка объединения или обычная ячейка
                  {
                    XmlElement elCell = xmlDocCnt.CreateElement("table:table-cell", nmspcTable);
                    elRow.AppendChild(elCell);

                    if (merge.RowCount > 1 || merge.ColumnCount > 1)
                    {
                      SetAttr(elCell, "table:number-columns-spanned", merge.ColumnCount.ToString(), nmspcTable);
                      SetAttr(elCell, "table:number-rows-spanned", merge.RowCount.ToString(), nmspcTable);
                    }


                    #region Значение

                    XmlElement elP = xmlDocCnt.CreateElement("text:p", nmspcText);
                    elCell.AppendChild(elP);
                    InitCEStyle(elCell, sel, elAutoStylesCnt);

                    WriteTextValue(sel, elP, false);

                    InitPStyle(elP, sel, elAutoStylesCnt, GetColumnWidth(stripeCols, l, merge.ColumnCount), String.Empty, rowKeepWithNext);

                    #endregion
                  }
                  else
                  {
                    // не первая ячейка
                    XmlElement elCoveredCell = xmlDocCnt.CreateElement("table:covered-table-cell", nmspcTable);
                    elRow.AppendChild(elCoveredCell);
                  }

                  sel.RowIndex = k; // восстановили

                } // цикл по столбцам
              } // цикл по строкам

              #endregion
            } // !SimpleBand

            prevBandIsSimple = thisBandIsSimple;
          } // цикл по вертикальным полосам
        } // цикл по таблицам

        #endregion
      } // цикл по листам
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


    private static int GetColumnWidth(BRStripeItem[] aStripeCols, int firstColumn, int columnCount)
    {
      int w = 0;
      for (int i = 0; i < columnCount; i++)
        w += aStripeCols[firstColumn + i].Size;
      return w;
    }

    private void WriteTextValue(BRSelector sel, XmlElement elP, bool isSimpleBand)
    {
      string s = sel.AsString;
      if (s.Length == 0)
        return;
      if (s.IndexOf(Environment.NewLine) < 0 && s.IndexOf("  ") < 0)
      {
        #region Простой режим

        XmlText text = elP.OwnerDocument.CreateTextNode(s);
        elP.AppendChild(text);

        #endregion
      }
      else
      {
        #region Сложный режим

        // Для многострочного текста используется странная конструкция в XML-файле.
        // В теге <text:p> идет текст, разделяемый тегами <text:line-break />:
        // <text:p text:style-name="P1">AAA<text:line-break />BBB<text:line-break />CCC</text:p>

        // Если в тексте есть два или более пробела подряд, тогда также нужен дополнительный тег <text:s>

        string[] a = s.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
        for (int i = 0; i < a.Length; i++)
        {
          if (i > 0)
          {
            XmlElement elLineBreak = elP.OwnerDocument.CreateElement("text:line-break", nmspcText);
            elP.AppendChild(elLineBreak);
          }

          if (a[i].IndexOf("  ") >= 0)
          {
            int currCount = 0;
            while (true)
            {
              int p = a[i].IndexOf("  ", currCount);
              if (p < 0)
                break;

              string s2 = a[i].Substring(currCount, p - currCount + 1); // захватили один пробел
              XmlText text = elP.OwnerDocument.CreateTextNode(s2);
              elP.AppendChild(text);
              currCount = p + 1;
              for (int j = currCount; j < a[i].Length; j++)
              {
                if (a[i][j] == ' ')
                {
                  XmlElement elS = elP.OwnerDocument.CreateElement("text:s", nmspcText);
                  elP.AppendChild(elS);
                  currCount++;
                }
                else
                  break;
              }
            }
            if (currCount < a[i].Length) // остаток строки
            {
              XmlText text = elP.OwnerDocument.CreateTextNode(a[i].Substring(currCount));
              elP.AppendChild(text);
            }
          }
          else
          {
            XmlText text = elP.OwnerDocument.CreateTextNode(a[i]);
            elP.AppendChild(text);
          }
        }

        #endregion
      }
    }

    #endregion

    #region Стили ячеек

    /// <summary>
    /// Вызывается после добавления текста
    /// </summary>
    private void InitPStyle(XmlElement elP, BRSelector sel, XmlElement elAutoStyles, int columnWidth, string pageStyleName, bool keepWithNext)
    {
      int widthPercent = BRFileTools.GetFontWidthPercent(sel, _Measurer, columnWidth);
      widthPercent = Math.Max(widthPercent, MinFontWidthPercent);
      widthPercent = Math.Min(widthPercent, MaxFontWidthPercent);

      BRFileTools.InitCellStyleKey(_SB, sel, widthPercent);
      if (!String.IsNullOrEmpty(pageStyleName))
      {
        // Делаем уникальный ключ
        _SB.Append('|');
        _SB.Append(pageStyleName);
      }

      string styleName;
      if (!_P_styles.TryGetValue(_SB.ToString(), out styleName))
      {
        styleName = "P" + StdConvert.ToString(_P_styles.Count + 1);
        _P_styles.Add(_SB.ToString(), styleName);

        XmlElement elStyle = elAutoStyles.OwnerDocument.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elStyle);
        SetAttr(elStyle, "style:name", styleName, nmspcStyle);
        SetAttr(elStyle, "style:family", "paragraph", nmspcStyle);
        if (!String.IsNullOrEmpty(pageStyleName))
          SetAttr(elStyle, "style:master-page-name", pageStyleName, nmspcStyle);

        XmlElement elParProps = elStyle.OwnerDocument.CreateElement("style:paragraph-properties", nmspcStyle);
        elStyle.AppendChild(elParProps);

        string ha;
        switch (sel.ActualHAlign)
        {
          case BRHAlign.Center: ha = "center"; break;
          case BRHAlign.Right: ha = "end"; break;
          default: ha = "start"; break;
        }
        SetAttr(elParProps, "fo:text-align", ha, nmspcFo);

        switch (sel.CellStyle.WrapMode)
        {
          case BRWrapMode.WordWrap:
            SetAttr(elParProps, "fo:wrap-option", "wrap", nmspcFo);
            break;
        }

        if (sel.CellStyle.IndentLevel > 0)
        {
          int indentOffset = BRFileTools.GetIndentWidth(sel.CellStyle, _Measurer); // в единицах 0.1мм
          switch (sel.ActualHAlign)
          {
            case BRHAlign.Left:
              SetAttr(elParProps, "fo:margin-left", (indentOffset / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
              break;
            case BRHAlign.Right:
              SetAttr(elParProps, "fo:margin-right", (indentOffset / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcFo);
              break;
          }
        }

        if (sel.CellStyle.LineHeightTwip > 0)
          SetAttr(elParProps, "fo:line-height", sel.CellStyle.LineHeightPt.ToString("0.0", StdConvert.NumberFormat) + "pt", nmspcFo);

        if (keepWithNext)
          SetAttr(elParProps, "fo:keep-with-next", "always", nmspcFo);



        XmlElement elTextProps = elStyle.OwnerDocument.CreateElement("style:text-properties", nmspcStyle);
        elStyle.AppendChild(elTextProps);

        SetAttr(elTextProps, "style:font-name", sel.CellStyle.FontName, nmspcStyle);
        SetAttr(elTextProps, "fo:font-size", sel.CellStyle.FontHeightPt.ToString("0.0", StdConvert.NumberFormat) + "pt", nmspcFo);
        if (widthPercent != 100)
          SetAttr(elTextProps, "style:text-scale", StdConvert.ToString(widthPercent) + "%", nmspcStyle);
        if (sel.CellStyle.Bold)
          SetAttr(elTextProps, "fo:font-weight", "bold", nmspcFo);
        if (sel.CellStyle.Italic)
          SetAttr(elTextProps, "fo:font-style", "italic", nmspcFo);
        if (sel.CellStyle.Underline)
          SetAttr(elTextProps, "style:text-underline-style", "solid", nmspcStyle);
        if (sel.CellStyle.Strikeout)
          SetAttr(elTextProps, "style:text-line-through-style", "solid", nmspcStyle);
        if (!sel.CellStyle.ForeColor.IsAuto)
          SetAttr(elTextProps, "fo:color", MyColorStr(sel.CellStyle.ForeColor), nmspcFo);

        if (!sel.CellStyle.BackColor.IsAuto)
          SetAttr(elTextProps, "fo:background-color", MyColorStr(sel.CellStyle.BackColor), nmspcFo); // дублируется для свойств ячейки

        if (sel.CellStyle.TextFiller != BRTextFiller.None)
        {
          // Open Office Writer не понимает атрибуты "style:leader-type" и
          // "style:leader-width". Придется заполнять их с помощью текста

          //string LdrType = "single";
          string leaderStyle = "solid";
          //string LdrWidth = "thin";
          string fillStr;
          switch (sel.CellStyle.TextFiller)
          {
            case BRTextFiller.Medium:
              //  LdrWidth = "medium";
              fillStr = BRFileTools.TextFillerStrMedium;
              break;
            case BRTextFiller.Thick:
              //  LdrWidth = "thick";
              fillStr = BRFileTools.TextFillerStrThick;
              break;
            case BRTextFiller.TwoLines:
              //LdrType = "double";
              fillStr = BRFileTools.TextFillerStrTwoLines;
              break;
            default:
              fillStr = BRFileTools.TextFillerStrThin;
              break;
          }
          // Если выравнивание по центру, то используется две позиции табуляции
          // (перед текстом и после текста), если по левому или правому краю,
          // то достаточно одной позиции (после или перед текстом соответственно)
          XmlElement elTabStops = elParProps.OwnerDocument.CreateElement("style:tab-stops", nmspcStyle);
          elParProps.AppendChild(elTabStops);
          if (sel.ActualHAlign == BRHAlign.Center)
          {
            XmlElement elTabStop1 = elTabStops.OwnerDocument.CreateElement("style:tab-stop", nmspcStyle);
            elTabStops.AppendChild(elTabStop1);
            SetAttr(elTabStop1, "style:position", (columnWidth / 100.0 / 2).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcStyle);
            SetAttr(elTabStop1, "style:type", "center", nmspcStyle);
            //SetAttr(elTabStop1, "style:leader-type", LdrType, nmspcStyle);
            SetAttr(elTabStop1, "style:leader-style", leaderStyle, nmspcStyle);
            //SetAttr(elTabStop1, "style:leader-width", LdrWidth, nmspcStyle);
            SetAttr(elTabStop1, "style:leader-text", fillStr, nmspcStyle);
          }

          XmlElement elTabStop2 = elTabStops.OwnerDocument.CreateElement("style:tab-stop", nmspcStyle);
          elTabStops.AppendChild(elTabStop2);
          SetAttr(elTabStop2, "style:position", (columnWidth / 100.0).ToString("0.00", StdConvert.NumberFormat) + "cm", nmspcStyle);
          SetAttr(elTabStop2, "style:type", "right", nmspcStyle);
          //SetAttr(elTabStop2, "style:leader-type", LdrType, nmspcStyle);
          SetAttr(elTabStop2, "style:leader-style", leaderStyle, nmspcStyle);
          //SetAttr(elTabStop2, "style:leader-width", LdrWidth, nmspcStyle);
          SetAttr(elTabStop2, "style:leader-text", fillStr, nmspcStyle);
        }
      }
      SetAttr(elP, "text:style-name", styleName, nmspcText);

      if (sel.CellStyle.TextFiller != BRTextFiller.None)
      {
        if (sel.ActualHAlign == BRHAlign.Center || sel.ActualHAlign == BRHAlign.Right)
        {
          XmlElement elTab1 = elP.OwnerDocument.CreateElement("text:tab", nmspcText);
          elP.InsertBefore(elTab1, GetFirstTextNode(elP));
        }
        if (sel.ActualHAlign == BRHAlign.Center || sel.ActualHAlign == BRHAlign.Left)
        {
          XmlElement elTab2 = elP.OwnerDocument.CreateElement("text:tab", nmspcText);
          elP.InsertAfter(elTab2, GetLastTextNode(elP));
        }
      }
    }

    private static XmlText GetFirstTextNode(XmlElement el)
    {
      XmlNodeList nodes = el.ChildNodes;
      for (int i = 0; i < nodes.Count; i++)
      {
        XmlText res = nodes[i] as XmlText;
        if (res != null)
          return res;
      }
      return null;
    }

    private static XmlText GetLastTextNode(XmlElement el)
    {
      XmlNodeList nodes = el.ChildNodes;
      for (int i = nodes.Count-1; i >= 0; i++)
      {
        XmlText res = nodes[i] as XmlText;
        if (res != null)
          return res;
      }
      return null;
    }

    private void InitCEStyle(XmlElement elCell, BRSelector sel, XmlElement elAutoStyles)
    {
      BRFileTools.InitCellStyleKey(_SB, sel, 0);
      string styleName;
      if (!_CE_styles.TryGetValue(_SB.ToString(), out styleName))
      {
        styleName = "ce" + StdConvert.ToString(_CE_styles.Count + 1);
        _CE_styles.Add(_SB.ToString(), styleName);

        XmlElement elStyle = elAutoStyles.OwnerDocument.CreateElement("style:style", nmspcStyle);
        elAutoStyles.AppendChild(elStyle);
        SetAttr(elStyle, "style:name", styleName, nmspcStyle);
        SetAttr(elStyle, "style:family", "table-cell", nmspcStyle);

        // Сначала добавляем формат для чисел
        string nFormatName = "N" + StdConvert.ToString(_CE_styles.Count);
        if (OpenOfficeTools.ODFAddFormat(elAutoStyles, sel.CellStyle.Format, nFormatName, sel.CellStyle.NumberFormat, sel.CellStyle.DateTimeFormat, null, null))
          SetAttr(elStyle, "style:data-style-name", nFormatName, nmspcStyle);

        XmlElement elCellProps = elStyle.OwnerDocument.CreateElement("style:table-cell-properties", nmspcStyle);
        elStyle.AppendChild(elCellProps);

        SetBorders(elCellProps, "fo:border-bottom", sel.CellStyle.BottomBorder, nmspcFo);
        SetBorders(elCellProps, "fo:border-left", sel.CellStyle.LeftBorder, nmspcFo);
        SetBorders(elCellProps, "fo:border-right", sel.CellStyle.RightBorder, nmspcFo);
        SetBorders(elCellProps, "fo:border-top", sel.CellStyle.TopBorder, nmspcFo);
        // Диагональных линий нет в Writer

        string va;
        switch (sel.CellStyle.VAlign)
        {
          case BRVAlign.Center: va = "middle"; break;
          case BRVAlign.Bottom: va = "bottom"; break;
          default: va = "top"; break;
        }
        SetAttr(elCellProps, "style:vertical-align", va, nmspcStyle);

        if (!sel.CellStyle.BackColor.IsAuto)
          SetAttr(elCellProps, "fo:background-color", MyColorStr(sel.CellStyle.BackColor), nmspcFo);
      }

      SetAttr(elCell, "table:style-name", styleName, nmspcTable);
    }

    #endregion
  }
}
