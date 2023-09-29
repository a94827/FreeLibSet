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
  public abstract class BRReportODFWriterBase
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
    public void CreateFile(BRReport report, AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");
      ZipFileTools.CheckZipLibAvailable();
      CreateFile2(report, filePath);
    }

    private void CreateFile2(BRReport report, AbsPath filePath)
    {
      #region Начало

      //SharpZipLibLoader.LoadSharpZipLib();
      ZipFileCreator zf = new ZipFileCreator(filePath.Path);
      zf.AddMimeType("application/vnd.oasis.opendocument.spreadsheet");

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

    #endregion

    #region Манифест (manifest.xml)

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
  /// Создание документов ODS
  /// </summary>
  public sealed class BRReportODSWriter : BRReportODFWriterBase
  {
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
        if (section.PageSetup.Landscape)
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

                BRReportWriterTools.InitCellStyleKey(sb, sel);

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
                    default: throw new BugException("Неизвестное горизонтальное выравнивание");
                  }
                  if (ha.Length > 0)
                    SetAttr(elParProps, "fo:text-align", ha, nmspcFo);

                  switch (sel.CellStyle.VAlign)
                  {
                    case BRVAlign.Top: va = "top"; break;
                    case BRVAlign.Center: va = "middle"; break;
                    case BRVAlign.Bottom: va = "bottom"; break;
                    default: throw new BugException("Неизвестное вертикальное выравнивание");
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
                  // TODO: FontWidthPt

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
                    s = DataTools.ReplaceAny(s, BRReportWriterTools.BadValueChars, ' ');
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

    private static void SetBorders(XmlElement elCellProps, string attrName, BRLine border, string nmspc)
    {
      string value1, value2;
      switch (border.Style)
      {
        case BRLineStyle.Thin:
          value1 = "0.1pt solid";
          break;
        case BRLineStyle.Medium:
          value1 = "0.5pt solid";
          break;
        case BRLineStyle.Thick:
          value1 = "1pt solid";
          break;
        case BRLineStyle.Dot:
          value1 = "0.1pt dotted";
          break;
        case BRLineStyle.Dash:
          value1 = "0.1pt dashed";
          break;
        case BRLineStyle.DashDot:
          value1 = "0.1pt dash-dot";
          break;
        case BRLineStyle.DashDotDot:
          value1 = "0.1pt dash-dot-dot";
          break;
        default:
          return;
      }

      if (border.Color == BRColor.Auto)
        value2 = "#000000";
      else
        value2 = MyColorStr(border.Color);

      SetAttr(elCellProps, attrName, value1 + " " + value2, nmspc);
    }
    private static string MyColorStr(BRColor c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }


    #endregion
  }
}
