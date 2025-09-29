// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using FreeLibSet.IO;
using FreeLibSet.Shell;
using FreeLibSet.Core;
using System.Data;

#pragma warning disable 1591

namespace FreeLibSet.OLE.Excel
{
  #region Перечисления

  public enum XlSheetType
  {
    xlWorksheet = -4167,
    xlDialogSheet = -4116,
    xlChart = -4109,
    xlExcel4MacroSheet = 3,
    xlExcel4IntlMacroSheet = 4,
  }

  public enum XlWBATemplate
  {
    xlWBATWorksheet = -4167,
    xlWBATChart = -4109,
    xlWBATExcel4MacroSheet = 3,
    xlWBATExcel4IntlMacroSheet = 4,
  }

  public enum XlHAlign
  {
    xlHAlignRight = -4152,
    xlHAlignLeft = -4131,
    xlHAlignJustify = -4130,
    xlHAlignDistributed = -4117,
    xlHAlignCenter = -4108,
    xlHAlignGeneral = 1,
    xlHAlignFill = 5,
    xlHAlignCenterAcrossSelection = 7,
  }

  public enum XlVAlign
  {
    xlVAlignTop = -4160,
    xlVAlignJustify = -4130,
    xlVAlignDistributed = -4117,
    xlVAlignCenter = -4108,
    xlVAlignBottom = -4107,
  }

  public enum XlBordersIndex
  {
    xlDiagonalDown = 5,
    xlDiagonalUp = 6,
    xlEdgeLeft = 7,
    xlEdgeTop = 8,
    xlEdgeBottom = 9,
    xlEdgeRight = 10,
    xlInsideVertical = 11,
    xlInsideHorizontal = 12,
  }

  public enum XlLineStyle
  {
    xlLineStyleNone = -4142,
    xlDouble = -4119,
    xlDot = -4118,
    xlDash = -4115,
    xlContinuous = 1,
    xlDashDot = 4,
    xlDashDotDot = 5,
    xlSlantDashDot = 13,
  }

  public enum XlBorderWeight
  {
    xlMedium = -4138,
    xlHairline = 1,
    xlThin = 2,
    xlThick = 4,
  }

  public enum XlPaperSize
  {
    xlPaperLetter = 1,
    xlPaperLetterSmall = 2,
    xlPaperTabloid = 3,
    xlPaperLedger = 4,
    xlPaperLegal = 5,
    xlPaperStatement = 6,
    xlPaperExecutive = 7,
    xlPaperA3 = 8,
    xlPaperA4 = 9,
    xlPaperA4Small = 10,
    xlPaperA5 = 11,
    xlPaperB4 = 12,
    xlPaperB5 = 13,
    xlPaperFolio = 14,
    xlPaperQuarto = 15,
    xlPaper10x14 = 16,
    xlPaper11x17 = 17,
    xlPaperNote = 18,
    xlPaperEnvelope9 = 19,
    xlPaperEnvelope10 = 20,
    xlPaperEnvelope11 = 21,
    xlPaperEnvelope12 = 22,
    xlPaperEnvelope14 = 23,
    xlPaperCsheet = 24,
    xlPaperDsheet = 25,
    xlPaperEsheet = 26,
    xlPaperEnvelopeDL = 27,
    xlPaperEnvelopeC5 = 28,
    xlPaperEnvelopeC3 = 29,
    xlPaperEnvelopeC4 = 30,
    xlPaperEnvelopeC6 = 31,
    xlPaperEnvelopeC65 = 32,
    xlPaperEnvelopeB4 = 33,
    xlPaperEnvelopeB5 = 34,
    xlPaperEnvelopeB6 = 35,
    xlPaperEnvelopeItaly = 36,
    xlPaperEnvelopeMonarch = 37,
    xlPaperEnvelopePersonal = 38,
    xlPaperFanfoldUS = 39,
    xlPaperFanfoldStdGerman = 40,
    xlPaperFanfoldLegalGerman = 41,
    xlPaperUser = 256,
  }

  public enum XlPageOrientation
  {
    xlPortrait = 1,
    xlLandscape = 2,
  }

  public enum XlInsertShiftDirection
  {
    xlShiftDown = -4121,
    xlShiftToRight = -4161
  }

  public enum XlDeleteShiftDirection
  {
    xlShiftToLeft = -4159,
    xlShiftUp = -4162
  }

  public enum XlWindowView
  {
    xlNormalView = 1,
    xlPageBreakPreview = 2
  }

  #endregion

  #region Основной объект

  public class ExcelHelper : OLEHelper
  {
    #region Конструктор и Disposing

    /// <summary>
    /// Запуск нового экземпляра Microsoft Excel или открытие существующего 
    /// (текущего) объекта
    /// </summary>
    /// <param name="isCreate"></param>
    public ExcelHelper(bool isCreate)
    {
      ShowOnEnd = true;
      if (isCreate)
        CreateMainObj("Excel.Application");
      else
        GetActiveMainObj("Excel.Application");
      _Application = new Application(new ObjBase(MainObj, this));

      MicrosoftOfficeTools.InitOleHelper(this, _Application.Base, "[DispID=392]"); // Свойство "Version"

      if (_Application.Version.Major >= MicrosoftOfficeTools.MicrosoftOffice_2007)
      {
        _MaxRowCount = 1048576;
        _MaxColumnCount = 16384;
      }
      else
      {
        _MaxRowCount = 65536;
        _MaxColumnCount = 256;
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (MainObj != null)
        {
          if (ShowOnEnd)
            SetProp(MainObj, "[DispID=558]", true); // VisibleEx
          else
            Call(MainObj, "[DispID=302]"); // Quit
        }
      }
      base.Dispose(disposing);

      // Очистка памяти
      if (disposing)
        GC.GetTotalMemory(true);
    }

    #endregion

    #region Способ завершения

    /// <summary>
    /// Если установлено в true (по умолчанию), то при завершении работы с помощником
    /// приложение Excel будет выведено на экран, иначе оно будет завершено
    /// </summary>
    public bool ShowOnEnd { get { return _ShowOnEnd; } set { _ShowOnEnd = value; } }
    private bool _ShowOnEnd;

    #endregion

    #region Основной объект

    public Application Application { get { return _Application; } }
    private readonly Application _Application;

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Максимальное количество строк на листе, поддерживаемое этой версией Excel
    /// </summary>
    public int MaxRowCount { get { return _MaxRowCount; } }
    private readonly int _MaxRowCount;

    /// <summary>
    /// Максимальное количество столбцов на листе, поддерживаемое этой версией Excel
    /// </summary>
    public int MaxColumnCount { get { return _MaxColumnCount; } }
    private readonly int _MaxColumnCount;

    #endregion
  }

  #endregion

  #region Приложение

  public struct Application
  {
    #region Конструктор

    public Application(ObjBase theBase)
    {
      _Base = theBase;
      _Workbooks = new Workbooks();
    }

    public ObjBase Base { get { return _Base; } }
    private readonly ObjBase _Base;

    #endregion

    #region Свойства

    /// <summary>
    /// Коллекция всех открытых книг
    /// </summary>
    public Workbooks Workbooks
    {
      get
      {
        if (_Workbooks.Base.IsEmpty)
        {
          _Workbooks = new Workbooks(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=572]"), Base.Helper));
        }
        return _Workbooks;
      }
    }
    private Workbooks _Workbooks;

    /// <summary>
    /// Активный лист.
    /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
    /// создается новый объект
    /// </summary>
    public Worksheet ActiveSheet
    {
      get
      {
        return new Worksheet(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=307]"), Base.Helper));
      }
    }

    /// <summary>
    /// Активная книга.
    /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
    /// создается новый объект
    /// </summary>
    public Workbook ActiveWorkbook
    {
      get
      {
        return new Workbook(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=308]"), Base.Helper));
      }
    }

    /// <summary>
    /// Текущая ячейка.
    /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
    /// создается новый объект
    /// </summary>
    public Range ActiveCell
    {
      get
      {
        return new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=305]"), Base.Helper));
      }
    }

    /// <summary>
    /// Версия программы, например "11.0" для Excel-2003
    /// </summary>
    public string VersionStr
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=392]"));
      }
    }

    /// <summary>
    /// Номер построения
    /// </summary>
    public Int32 Build
    {
      get
      {
        return DataTools.GetInt32(Base.Helper.GetProp(Base.Obj, "[DispID=314]"));
      }
    }

    /// <summary>
    /// Получить версию Excel как объект
    /// </summary>
    public Version Version
    {
      get
      {
        string verStr = VersionStr;
        int bld = Build;

        //MessageBox.Show("VerStr=\""+VerStr+"\", Build="+Bld.ToString());

        Version ver = FileTools.GetVersionFromStr(verStr);
        if (ver != null) // 27.12.2020
        {
          if (ver.Build <= 0)
            ver = new Version(ver.Major, ver.Major, bld, 0);
        }
        return ver;
      }
    }

    /// <summary>
    /// Надо ли выдавать подтверждения пользователю
    /// </summary>
    public bool DisplayAlerts
    {
      get
      {
        return DataTools.GetBoolean(Base.Helper.GetProp(Base.Obj, "[DispID=343]"));
      }
    }

    public void SetDisplayAlerts(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=343]", value);
    }

    #endregion
  }

  #endregion

  #region Книга

  public struct Workbook
  {
    #region Конструктор

    public Workbook(ObjBase theBase)
    {
      _Base = theBase;
      _Sheets = new Worksheets();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public Worksheets Sheets
    {
      get
      {
        if (_Sheets.Base.IsEmpty)
          _Sheets = new Worksheets(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=485]"), Base.Helper));
        return _Sheets;
      }
    }
    private Worksheets _Sheets;

    /// <summary>
    /// Активный лист.
    /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
    /// создается новый объект
    /// </summary>
    public Worksheet ActiveSheet
    {
      get
      {
        return new Worksheet(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=307]"), Base.Helper));
      }
    }

    /// <summary>
    /// Установка свойства в true предотвращает появление сообщение "Документ не сохранен" при закрытии документа
    /// </summary>
    public bool Saved
    {
      get
      {
        return (bool)(Base.Helper.GetProp(Base.Obj, "[DispID=298]"));
      }
      set
      {
        Base.Helper.SetProp(Base.Obj, "[DispID=298]", value);
      }
    }


    /// <summary>
    /// Список окон для книги. Обычно есть только одно окно
    /// </summary>
    public Windows Windows
    {
      get
      {
        return new Windows(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=430]"), Base.Helper));
      }
    }

    public Names Names
    {
      get
      {
        return new Names(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=442]"), Base.Helper));
      }
    }

    #endregion

    #region Свойства документа

    public string Title
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=199]"));
      }
    }
    public void SetTitle(string s)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=199]", s);
    }

    public string Subject
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=953]"));
      }
    }
    public void SetSubject(string s)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=953]", s);
    }

    public string Author
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=574]"));
      }
    }
    public void SetAuthor(string s)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=574]", s);
    }
    public string Comments
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=575]"));
      }
    }
    public void SetComments(string s)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=575]", s);
    }

    /// <summary>
    /// Коллекция встроенных свойств
    /// </summary>
    public DocumentProperties BuiltinDocumentProperties
    {
      get
      {
        return new DocumentProperties(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1176]"), Base.Helper));
      }
    }

    public string Company
    {
      get
      {
        return DataTools.GetString(GetBuiltInProperty("Company"));
      }
    }

    public void SetCompany(string s)
    {
      SetBuilInProperty("Company", s);
    }

    private object GetBuiltInProperty(string name)
    {
      return BuiltinDocumentProperties[name].Value;
    }

    private void SetBuilInProperty(string name, string value)
    {
      BuiltinDocumentProperties[name].SetValue(value);
    }

    #endregion

    #region Методы

    public void Close()
    {
      Base.Helper.Call(Base.Obj, "[DispID=277]");
    }

    #endregion
  }

  public struct Workbooks
  {
    #region Конструктор

    public Workbooks(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Методы

    /// <summary>
    /// Создать новую рабочую книгу с заданным количеством листов
    /// </summary>
    /// <param name="sheetCount"></param>
    /// <returns></returns>
    public Workbook Add(int sheetCount)
    {
      Workbook wbk = new Workbook(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=181]", XlWBATemplate.xlWBATWorksheet), Base.Helper));

      int n = wbk.Sheets.Count;
      if (n < sheetCount)
        wbk.Sheets.Add(sheetCount - n);

      // Делаем первый лист активным
      wbk.Sheets[1].Select();

      return wbk;
    }

    /// <summary>
    /// Открыть файл
    /// </summary>
    /// <param name="fileName"></param>
    public void Open(string fileName)
    {
      Base.Helper.Call(Base.Obj, "[DispID=682]", fileName);
    }

    #endregion
  }

  #endregion

  #region Лист

  /// <summary>
  /// Масштабатор для вычисления свойства Range.ColumnWidth, которое необходимо задать для требуемой ширины в пунктах
  /// </summary>
  public struct CellWidthScale
  {
    #region Конструктор

    public CellWidthScale(Worksheet sheet)
    {
      // 13.09.2023
      // Старый вариант неточный
      // Предполагаем, что ячейка состоит из текста и полей.
      //    Wn = n * x + y
      // Здесь: Wn - ширина ячейки в пунктах, когда установлено свойство ColumnWidth=n;
      //        x  - коэффициент пропорциональности между шириной в пункта и шириной в символах;
      //        y  - ширина полей
      // В старом варианте неявно предполагалось, что y=0

      // Делаем две тестовые установки ширины столбца в ColumnWidth=10 и 20, потом возвращаем обратно.
      // Потом решаем систему из двух уравнений

      Range r = sheet.Cells[1, 1];
      double wOld = r.ColumnWidth;
      double w10, w20;
      try
      {
        r.SetColumnWidth(10);
        w10 = r.Width;
        r.SetColumnWidth(20);
        w20 = r.Width;
      }
      finally
      {
        r.SetColumnWidth(wOld);
      }

      x = (w20 - w10) / 10.0;
      y = w10 - x * 10;
    }

    #endregion

    #region Свойства

    private readonly double x;
    private readonly double y;

    #endregion

    #region Методы

    public double ColumnWidthToPt(double columnWidth)
    {
      return columnWidth * x + y;
    }

    public double PtToColumnWidth(double pt)
    {
      if (pt <= y)
        return 0.0;
      return (pt - y) / x;
    }

    #endregion
  }

  public struct Worksheet
  {
    #region Конструктор

    public Worksheet(ObjBase theBase)
    {
      _Base = theBase;
      _Cells = new Range();
      _PageSetup = new PageSetup();
      _Columns = new Range();
      _Rows = new Range();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    /// <summary>
    /// Имя листа, отображаемое на ярлычке
    /// </summary>
    public string Name
    {
      get { return GetName(); }
      set { SetName(value); }
    }

    private string GetName()
    {
      return (string)(Base.Helper.GetProp(Base.Obj, "[DispID=110]"));
    }

    public void SetName(string value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=110]", value);
    }

    /// <summary>
    /// Возвращает диапазон, представляющий весь лист
    /// </summary>
    public Range Cells
    {
      get
      {
        if (_Cells.Base.IsEmpty)
          _Cells = new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=238]"), Base.Helper));
        return _Cells;
      }
    }
    private Range _Cells;

    public Range Rows
    {
      get
      {
        if (_Rows.Base.IsEmpty)
          _Rows = new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=258]"), Base.Helper));
        return _Rows;
      }
    }
    private Range _Rows;

    public Range Columns
    {
      get
      {
        if (_Columns.Base.IsEmpty)
          _Columns = new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=241]"), Base.Helper));
        return _Columns;
      }
    }
    private Range _Columns;

    public PageSetup PageSetup
    {
      get
      {
        if (_PageSetup.Base.IsEmpty)
          _PageSetup = new PageSetup(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=998]"), Base.Helper));
        return _PageSetup;
      }
    }
    private PageSetup _PageSetup;

    public Hyperlinks Hyperlinks
    {
      get { return new Hyperlinks(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1393]"), Base.Helper)); }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получение диапазона для прямоугольной области
    /// </summary>
    /// <param name="rowIndex1">Верхняя строка. Нумерация начинается с 1</param>
    /// <param name="colIndex1">Левый столбец. Нумерация начинается с 1</param>
    /// <param name="rowIndex2">Нижняя строка. Нумерация начинается с 1</param>
    /// <param name="colIndex2">Правый столбец. Нумерация начинается с 1</param>
    /// <returns></returns>
    public Range GetRange(int rowIndex1, int colIndex1, int rowIndex2, int colIndex2)
    {
      //global::Excel.Worksheet
#if DEBUG
      ExcelHelper helper = (ExcelHelper)(_Base.Helper);
      if (rowIndex1 < 1 || rowIndex1 > helper.MaxRowCount)
        throw ExceptionFactory.ArgOutOfRange("rowIndex1", rowIndex1, 1, helper.MaxRowCount);
      if (colIndex1 < 1 || colIndex1 > helper.MaxColumnCount)
        throw ExceptionFactory.ArgOutOfRange("colIndex1", colIndex1, 1, helper.MaxColumnCount);
      if (rowIndex2 < rowIndex1 || rowIndex2 > helper.MaxRowCount)
        throw ExceptionFactory.ArgOutOfRange("rowIndex2", rowIndex2, rowIndex1, helper.MaxRowCount);
      if (colIndex2 < colIndex1 || colIndex2 > helper.MaxColumnCount)
        throw ExceptionFactory.ArgOutOfRange("colIndex2", colIndex2, colIndex1, helper.MaxColumnCount);
#endif
      // Простой диапазон
      if (rowIndex2 == rowIndex1 && colIndex2 == colIndex1)
        return Cells[rowIndex1, colIndex1];
      // Комбинированный диапазон
      Range r1 = Cells[rowIndex1, colIndex1];
      Range r2 = Cells[rowIndex2, colIndex2];

      // Range - это свойство, а не метод
      return new Range(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=197]", r1.Base.Obj, r2.Base.Obj), Base.Helper));
    }

    /// <summary>
    /// Сделать лист текущим
    /// </summary>
    public void Select()
    {
      Base.Helper.Call(Base.Obj, "[DispID=235]");
    }

    /// <summary>
    /// Получить диапазон строк
    /// </summary>
    /// <param name="firstRow">Номер первой строки</param>
    /// <param name="lastRow">Номер последней строки</param>
    /// <returns>Диапазон</returns>
    public Range GetRowsRange(int firstRow, int lastRow)
    {
      ExcelHelper helper = (ExcelHelper)(_Base.Helper);

      if (firstRow < 1 || firstRow > helper.MaxRowCount)
        throw ExceptionFactory.ArgOutOfRange("firstRow", firstRow, 1, helper.MaxRowCount);
      if (lastRow < 1 || firstRow > helper.MaxRowCount)
        throw ExceptionFactory.ArgOutOfRange("lastRow", lastRow, 1, helper.MaxRowCount);
      if (firstRow > lastRow)
        throw ExceptionFactory.ArgRangeInverted("firstRow", firstRow, "lastRow", lastRow);

      Range r1 = Rows[firstRow];
      Range r2 = Rows[lastRow];
      return new Range(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=197]", r1.Base.Obj, r2.Base.Obj), Base.Helper));
    }

    /// <summary>
    /// Получить диапазон столбцов
    /// </summary>
    /// <param name="firstColumn">Номер первого столбца (1-256)</param>
    /// <param name="lastColumn">Номер последнего столбца</param>
    /// <returns>Диапазон</returns>
    public Range GetColumnsRange(int firstColumn, int lastColumn)
    {
      ExcelHelper helper = (ExcelHelper)(_Base.Helper);

      if (firstColumn < 1 || firstColumn > helper.MaxColumnCount)
        throw ExceptionFactory.ArgOutOfRange("firstColumn", firstColumn, 1, helper.MaxColumnCount);
      if (lastColumn < 1 || lastColumn > helper.MaxColumnCount)
        throw ExceptionFactory.ArgOutOfRange("lastColumn", lastColumn, 1, helper.MaxColumnCount);
      if (firstColumn > lastColumn)
        throw ExceptionFactory.ArgRangeInverted("firstColumn", firstColumn, "lastColumn", lastColumn);

      Range r1 = Columns[firstColumn];
      Range r2 = Columns[lastColumn];
      return new Range(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=197]", r1.Base.Obj, r2.Base.Obj), Base.Helper));
    }

    #endregion

    #region Установка размеров строк и столбцов

    /// <summary>
    /// Получить коэффициент для перевода ширины столбцов из пунктов в условные единицы
    /// </summary>
    /// <returns></returns>
    public double GetColumnWidthScale()
    {
      Range r = Cells[1, 1];
      double w1 = r.ColumnWidth; // в символах
      double w2 = r.Width; // в пунктах
      return w1 / w2;
    }

    /// <summary>
    /// Групповая установка высоты строк в единицах 0.1 мм
    /// </summary>
    /// <param name="heights"></param>
    public void SetRowHeightsLM(int[] heights)
    {
      SetRowHeightsLM(heights, 1);
    }

    public void SetRowHeightsLM(int[] heights, int firstRowIndex)
    {
      if (heights == null || heights.Length == 0)
        return;
      int firstPos = 0;
      while (firstPos < heights.Length)
      {
        int h = heights[firstPos];
        int lastPos = firstPos + 1;
        while (lastPos < heights.Length && heights[lastPos] == h)
          lastPos++;
        lastPos--;
        SetRowHeightsLM(h, firstRowIndex + firstPos, lastPos - firstPos + 1);
        firstPos = lastPos + 1;
      }
    }

    private void SetRowHeightsLM(int height, int firstRowIndex, int rowCount)
    {
#if DEBUG
      ExcelHelper helper = (ExcelHelper)(_Base.Helper);

      if (firstRowIndex < 1 || firstRowIndex > helper.MaxRowCount)
        throw ExceptionFactory.ArgOutOfRange("firstRowIndex", firstRowIndex, 1, helper.MaxRowCount);
      if (rowCount < 1 || (firstRowIndex + rowCount - 1) > helper.MaxRowCount)
        throw ExceptionFactory.ArgOutOfRange("rowCount", rowCount, 1, helper.MaxRowCount - firstRowIndex + 1);
#endif

      Range r = GetRange(firstRowIndex, 1, firstRowIndex + rowCount - 1, 1);
      r.SetRowHeight((double)height / 254.0 * 72.0);
    }


    /// <summary>
    /// Установить ширину столбцов в единицах 0.1 мм
    /// </summary>
    /// <param name="widths"></param>
    public void SetColumnWidthsLM(int[] widths)
    {
      SetColumnWidthsLM(widths, 1);
    }

    private void SetColumnWidthsLM(int[] widths, int firstColumnIndex)
    {
      if (widths == null || widths.Length == 0)
        return;
      double WidthScale = GetColumnWidthScale();

      // Свойство ColumnWidth при установке округляется. 
      // Считаем с накоплением

      // На сколько пунктов надо уменьшить ширину каждого столбца
      //double stdsz = Sheet.Application.StandardFontSize;
      double columnSmaller = 0;//1.6; // / stdsz * 10;
      double cs2 = columnSmaller / 72.0 * 254.0; // в единицах 0.1 мм

      double prevErr = 0.0; // ошибка от предыдущих шагов в единицах 0.1 мм
      for (int i = 0; i < widths.Length; i++)
      {
        Range colRange = Cells[1, firstColumnIndex + i];
        double wantedW = (double)widths[i] - prevErr; // в единицах 0.1 мм с учетом ошибки
        if (wantedW < cs2)
          wantedW = cs2;
        double colWPt = DoSetColumnWidth(colRange, WidthScale, wantedW / 254.0 * 72.0);
        double realW = colWPt / 72.0 * 254.0 + cs2; // Реальная ширина в единицах 0.1 мм
        wantedW = (double)widths[i] - prevErr; // еще раз
        prevErr = realW - wantedW;
        if (Math.Abs(prevErr) > 20.0)
          throw new BugException("Cannot set the column width with appropriate precision");
      }
    }

    /// <summary>
    /// Установка ширины столбца методом последовательных приближений
    /// </summary>
    /// <param name="colRange">Диапазон, содержащий один столбец</param>
    /// <param name="scale">Результат GetColumnWidthScale()</param>
    /// <param name="ptW">Желаемая ширина столбца в пунктах</param>
    /// <returns>Реальная ширина столбца в пунктах</returns>
    private double DoSetColumnWidth(Range colRange, double scale, double ptW)
    {
      double w2 = 0; // иначе будет предупреждение
      colRange.ColumnWidth = ptW * scale;
      for (int i = 0; i < 5; i++) // не больше 5 попыток
      {
        w2 = colRange.Width; // в пунктах
        if (Math.Abs(ptW - w2) <= 72.0 / 254.0)
          break;
        double w1 = colRange.ColumnWidth; // не обязательно то, что пытались установить
        w1 += (ptW - w2) * scale;
        colRange.ColumnWidth = w1;
      }
      return w2;
    }

    /// <summary>
    /// Установить ширину столбцов в единицах ширины шрифта
    /// </summary>
    /// <param name="widths"></param>
    public void SetColumnTextWidths(double[] widths)
    {
      SetColumnTextWidths(widths, 1);
    }

    private void SetColumnTextWidths(double[] widths, int firstColumnIndex)
    {
      if (widths == null || widths.Length == 0)
        return;

      for (int i = 0; i < widths.Length; i++)
      {
        Range colRange = Cells[1, firstColumnIndex + i];
        colRange.ColumnWidth = widths[i];
      }
    }

    #endregion

    #region Расширенные методы

    /// <summary>
    /// Установить размер таблицы. При этом могут быть либо добавлены строки,
    /// либо удалены
    /// </summary>
    /// <param name="firstRow">Номер первой строки, начиная с 1</param>
    /// <param name="oldRowCount">Число строк в существующей таблице</param>
    /// <param name="newRowCount">Число строк, которое должно быть</param>
    public void SetTableRowCount(int firstRow, int oldRowCount, int newRowCount)
    {
      if (firstRow < 1)
        throw ExceptionFactory.ArgOutOfRange("firstRow", firstRow, 1, null);
      if (oldRowCount < 0)
        throw ExceptionFactory.ArgOutOfRange("oldRowCount", oldRowCount, 0, null);
      if (newRowCount < 0)
        throw ExceptionFactory.ArgOutOfRange("newRowCount", newRowCount, 0, null);

      if (newRowCount > oldRowCount)
      {
        Range r1 = GetRowsRange(firstRow + oldRowCount, firstRow + newRowCount - 1);
        r1.Insert();
        if (oldRowCount > 0)
        {
          Range r2 = Rows[firstRow];
          Range r3 = GetRowsRange(firstRow + oldRowCount, firstRow + newRowCount - 1);
          r2.Copy(r3);
        }
      }
      else if (newRowCount < oldRowCount)
      {
        Range r1 = GetRowsRange(firstRow + newRowCount, firstRow + oldRowCount - 1);
        r1.Delete();
      }
    }

    #endregion
  }

  public struct Worksheets
  {
    #region Конструктор

    public Worksheets(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public Worksheet this[int sheetIndex]
    {
      get
      {
        return new Worksheet(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]", sheetIndex), Base.Helper));
      }
    }

    public int Count
    {
      get
      {
        return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=118]"));
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавление одного листа
    /// </summary>
    /// <returns></returns>
    public Worksheet Add()
    {
      Add(1);
      return this[Count];
    }

    public void Add(int sheetCount)
    {
      object lastObj;
      int n = this.Count;
      if (n == 0)
        lastObj = Missing.Value;
      else
        lastObj = this[n].Base.Obj;

      // Если добавить листы так, то они будут поименованы задом наперед
      //AnyDocType.Helper.Call(AnyDocType.Obj, "[DispID=181]",
      //  Missing.Value, LastObj, Count, XlSheetType.xlWorksheet);
      for (int i = 0; i < sheetCount; i++)
      {
        Base.Helper.Call(Base.Obj, "[DispID=181]",
          Missing.Value, lastObj, 1, XlSheetType.xlWorksheet);
        lastObj = this[n + i + 1].Base.Obj;
      }
    }

    /// <summary>
    /// Создает новую книгу с копиями листов исходной книгм
    /// </summary>
    public void Copy()
    {
      Base.Helper.Call(Base.Obj, "[DispID=551]");
    }

    #endregion
  }

  #endregion

  #region Диапазон ячеек

  public struct Range
  {
    #region Конструктор

    public Range(ObjBase theBase)
    {
      _Base = theBase;
      _Borders = new Borders();
      _Interior = new Interior();
      _Font = new Font();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Диапазон

    /// <summary>
    /// Номер первой строки, начиная с 1
    /// </summary>
    public int Row
    {
      get { return DataTools.GetInt32(Base.Helper.GetProp(Base.Obj, "[DispID=257]")); }
    }


    /// <summary>
    /// Номер первого столбца, начиная с 1
    /// </summary>
    public int Column
    {
      get { return DataTools.GetInt32(Base.Helper.GetProp(Base.Obj, "[DispID=240]")); }
    }

    /// <summary>
    /// Координаты диапазона.
    /// Использование метода не является оптимальным, так как выполняется 6 вызовов Excel
    /// </summary>
    public Models.SpreadsheetBase.RangeRef RangeRef
    {
      get
      {
        int r1 = Row;
        int c1 = Column;
        int nr = Rows.Count;
        int nc = Columns.Count;
        return new Models.SpreadsheetBase.RangeRef(r1, c1, r1 + nr - 1, c1 + nc - 1);
      }
    }

    public Worksheet Worksheet
    {
      get
      {
        return new Worksheet(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=348]"), Base.Helper));
      }
    }

    #endregion

    #region Поддиапазоны

    /// <summary>
    /// Возвращает диапазон, содержащий одну ячейку
    /// </summary>
    /// <param name="rowIndex">Номер строки в пределах текущего диапазона. Нумерация начинается с 1.</param>
    /// <param name="columnIndex">Номер столбца в пределах текущего диапазона. Нумерация начинается с 1.</param>
    /// <returns></returns>
    public Range this[int rowIndex, int columnIndex]
    {
      get
      {
        return new Range(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]",
          rowIndex, columnIndex), Base.Helper));
      }
    }

    public Range this[int index]
    {
      get
      {
        return new Range(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]",
          index), Base.Helper));
      }
    }

    public Range Rows
    {
      get
      {
        return new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=258]"), Base.Helper));
      }
    }

    public Range Columns
    {
      get
      {
        return new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=241]"), Base.Helper));
      }
    }


    /// <summary>
    /// Возвращает число ячеек в диапазоне
    /// </summary>
    public int Count { get { return GetCount(); } }

    public int GetCount()
    {
      return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=118]"));
    }

    public Range Offset(int rowOffset, int columnOffset)
    {
      return new Range(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=254]",
        rowOffset, columnOffset), Base.Helper));
    }


    #endregion

    #region Значение

    public object Formula
    {
      get { return Base.Helper.GetProp(Base.Obj, "[DispID=261]"); }
      set { SetFormula(value); }
    }

    public void SetFormula(object value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=261]", value);
    }

    /// <summary>
    /// Массив формул. Массив должен быть одномерным
    /// </summary>
    public object[] FormulaArray
    {
      get { return GetFormulaArray(); }
      set { SetFormulaArray(value); }
    }

    private object[] GetFormulaArray()
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=586]");
      if (x == null)
        return null;
      if (x.GetType().IsArray)
        return (object[])x;
      else
        return new object[1] { x };
    }
    private void SetFormulaArray(object[] value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=586]", value);
    }


    public object Value
    {
      get { return Base.Helper.GetProp(Base.Obj, "[DispID=6]"); }
      set { SetValue(value); }
    }
    public void SetValue(object value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=6]", value);
    }

    public object Value2
    {
      get { return Base.Helper.GetProp(Base.Obj, "[DispID=1388]"); }
      set { SetValue2(value); }
    }
    public void SetValue2(object value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1388]", value);
    }


    public string AsString
    {
      get { return GetAsString(); }
      set { SetAsString(value); }
    }

    public string GetAsString()
    {
      object x = Value;
      if (x == null)
        return String.Empty;
      if (x is String)
      {
        if (((string)x).StartsWith("'", StringComparison.Ordinal))
          return ((string)x).Substring(1);
      }
      return x.ToString();
    }

    public void SetAsString(string value)
    {
      if (String.IsNullOrEmpty(value))
        SetValue(null);
      else
      {
        if ("0123456789+-.,/'=".IndexOf(value[0]) >= 0)
          SetValue("'" + value);
      }
    }

    #endregion

    #region Формат

    /// <summary>
    /// Формат ячеек. Возвращает null, если формат ячеек в диапазоне различается.
    /// Формат в международном формате.
    /// </summary>
    public string NumberFormat
    {
      get
      {
        string res;
        GetNumberFormat(out res);
        return res;
      }
      set
      {
        SetNumberFormat(value);
      }
    }

    public bool GetNumberFormat(out string value)
    {
      value = (string)(Base.Helper.GetProp0409(Base.Obj, "[DispID=193]"));
      return value != null;
    }
    public void SetNumberFormat(string value)
    {
      if (value == null)
        value = String.Empty;
      Base.Helper.SetProp0409(Base.Obj, "[DispID=193]", value);
    }

    #endregion

    #region Выравнивание

    public XlHAlign HorizontalAlignment
    {
      get
      {
        XlHAlign Res;
        GetHorizontalAlignment(out Res);
        return Res;
      }
      set
      {
        SetHorizontalAlignment(value);
      }
    }

    public bool GetHorizontalAlignment(out XlHAlign value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=136]");
      if (x == null)
      {
        value = XlHAlign.xlHAlignGeneral;
        return false;
      }
      else
      {
        value = (XlHAlign)x;
        return true;
      }
    }

    public void SetHorizontalAlignment(XlHAlign value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=136]", (int)value);
    }

    public XlVAlign VerticalAlignment
    {
      get
      {
        XlVAlign Res;
        GetVerticalAlignment(out Res);
        return Res;
      }
      set
      {
        SetVerticalAlignment(value);
      }
    }

    public bool GetVerticalAlignment(out XlVAlign value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=137]");
      if (x == null)
      {
        value = XlVAlign.xlVAlignBottom;
        return false;
      }
      else
      {
        value = (XlVAlign)x;
        return true;
      }
    }
    public void SetVerticalAlignment(XlVAlign value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=137]", (int)value);
    }

    /// <summary>
    /// Разрешение переносить текст ячейки по словам
    /// </summary>
    public bool WrapText
    {
      get
      {
        bool res;
        GetWrapText(out res);
        return res;
      }
      set
      {
        SetWrapText(value);
      }
    }

    public bool GetWrapText(out bool value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=276]");
      if (x == null)
      {
        value = false;
        return false;
      }
      else
      {
        value = (bool)x;
        return true;
      }
    }
    public void SetWrapText(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=276]", value);
    }

    /// <summary>
    /// Отступ текста (число символов о 0 до 15)
    /// </summary>
    public int IndentLevel
    {
      get
      {
        int res;
        GetIndentLevel(out res);
        return res;
      }
      set
      {
        SetIndentLevel(value);
      }
    }

    public bool GetIndentLevel(out int value)
    {
      //global::Excel.Range
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=201]");
      if (x == null)
      {
        value = 0;
        return false;
      }
      else
      {
        value = (int)x;
        return true;
      }
    }

    public void SetIndentLevel(int value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=201]", value);
    }

    #endregion

    #region Границы

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=435]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    #endregion

    #region Заполнение

    public Interior Interior
    {
      get
      {
        if (_Interior.Base.IsEmpty)
          _Interior = new Interior(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=129]"), Base.Helper));
        return _Interior;
      }
    }
    private Interior _Interior;

    #endregion

    #region Шрифт

    public Font Font
    {
      get
      {
        if (_Font.Base.IsEmpty)
          _Font = new Font(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=146]"), Base.Helper));
        return _Font;
      }
    }
    private Font _Font;

    #endregion

    #region Высота и ширина строк

    /// <summary>
    /// Высота каждой строки диапазона в пунктах.
    /// Если строки имеют разную высоту, будет возвращено значение 0.
    /// При установке значения все строки принимают заданную высоту
    /// </summary>
    public double RowHeight
    {
      get
      {
        double value;
        GetRowHeight(out value);
        return value;
      }
      set { SetRowHeight(value); }
    }

    public bool GetRowHeight(out double value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=272]");
      if (x == null)
      {
        value = 0.0;
        return false;
      }
      else
      {
        value = (double)x;
        return true;
      }
    }
    public void SetRowHeight(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=272]", value);
    }

    /// <summary>
    /// Ширина каждого столбца диапазона в условных единицах.
    /// Условная единица соответствует ширине символа "0" стандартного шрифта
    /// Для перевода пунктов в единицы используйте свойство Sheet.ColumnWidthScale
    /// Если строки имеют разную ширину, будет возвращено значение 0.
    /// При установке значения все строки принимают заданную ширину.
    /// </summary>
    public double ColumnWidth
    {
      get
      {
        double value;
        GetColumnWidth(out value);
        return value;
      }
      set { SetColumnWidth(value); }
    }

    public bool GetColumnWidth(out double value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=242]");
      if (x == null)
      {
        value = 0.0;
        return false;
      }
      else
      {
        value = (double)x;
        return true;
      }
    }

    public void SetColumnWidth(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=242]", value);
    }

    /// <summary>
    /// Суммарная высота всех строк диапазона в пунктах (DataReadOnly)
    /// </summary>
    public double Height { get { return GetHeight(); } }
    public double GetHeight()
    {
      return (double)(Base.Helper.GetProp(Base.Obj, "[DispID=123]"));
    }

    /// <summary>
    /// Суммарная ширина всех столбцов диапазона в пунктах (DataReadOnly)
    /// </summary>
    public double Width { get { return GetWidth(); } }
    public double GetWidth()
    {
      return (double)(Base.Helper.GetProp(Base.Obj, "[DispID=122]"));
    }

    #endregion

    #endregion

    #region Методы

    #region Объединение ячеек

    public void Merge(bool across)
    {
      Base.Helper.Call(Base.Obj, "[DispID=564]", across);
    }

    public void Merge()
    {
      Merge(false);
    }

    #endregion

    #region Вставка и удаление

    public void Insert(XlInsertShiftDirection shift)
    {
      Base.Helper.Call(Base.Obj, "[DispID=252]", (int)shift);
    }

    public void Insert()
    {
      Base.Helper.Call(Base.Obj, "[DispID=252]");
    }

    public void Delete(XlDeleteShiftDirection shift)
    {
      Base.Helper.Call(Base.Obj, "[DispID=117]", (int)shift);
    }

    public void Delete()
    {
      Base.Helper.Call(Base.Obj, "[DispID=117]");
    }

    #endregion

    /// <summary>
    /// Копирование ячеек в другой диапазон 
    /// </summary>
    /// <param name="dest">Заполняемый диапазон</param>
    public void Copy(Range dest)
    {
      Base.Helper.Call(Base.Obj, "[DispID=551]", dest.Base.Obj);
    }

    #endregion
  }

  #endregion

  #region Границы

  public struct Border
  {
    #region Конструктор

    public Border(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Стиль линии

    public XlLineStyle LineStyle
    {
      get
      {
        XlLineStyle Value;
        GetLineStyle(out Value);
        return Value;
      }
      set
      {
        SetLineStyle(value);
      }
    }

    public bool GetLineStyle(out XlLineStyle value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=119]");
      if (x == null)
      {
        value = XlLineStyle.xlLineStyleNone;
        return false;
      }
      else
      {
        value = (XlLineStyle)x;
        return true;
      }
    }
    public void SetLineStyle(XlLineStyle value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=119]", (int)value);
    }

    #endregion

    #region Толщина

    public XlBorderWeight Weight
    {
      get
      {
        XlBorderWeight value;
        GetWeight(out value);
        return value;
      }
      set
      {
        SetWeight(value);
      }
    }

    public bool GetWeight(out XlBorderWeight value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=129]");
      if (x == null)
      {
        value = XlBorderWeight.xlHairline;
        return false;
      }
      else
      {
        value = (XlBorderWeight)x;
        return true;
      }
    }
    public void SetWeight(XlBorderWeight value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=120]", (int)value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Excel
    /// </summary>
    public Int32 Color
    {
      get
      {
        Int32 value;
        GetColor(out value);
        return value;
      }
      set
      {
        SetColor(value);
      }
    }

    public bool GetColor(out Int32 value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=99]");
      if (x == null)
      {
        value = 0x00FFFFFF;
        return false;
      }
      else
      {
        value = (Int32)x;
        return true;
      }
    }
    public void SetColor(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=99]", value);
    }

    #endregion

    #endregion
  }

  public struct Borders
  {
    #region Конструктор

    public Borders(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Общие свойства

    public Border this[XlBordersIndex index]
    {
      get
      {
        return new Border(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    #endregion

    #region Стиль линии

    public XlLineStyle LineStyle
    {
      get
      {
        XlLineStyle Value;
        GetLineStyle(out Value);
        return Value;
      }
      set
      {
        SetLineStyle(value);
      }
    }

    public bool GetLineStyle(out XlLineStyle value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=119]");
      if (x == null)
      {
        value = XlLineStyle.xlLineStyleNone;
        return false;
      }
      else
      {
        value = (XlLineStyle)x;
        return true;
      }
    }

    /// <summary>
    /// Установить стиль границ диапазона.
    /// Устанавливаются внешние и внутренние границы.
    /// Диагональные линии не устанавливаются.
    /// </summary>
    /// <param name="value">Стиль линии</param>
    public void SetLineStyle(XlLineStyle value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=119]", (int)value);
    }

    #endregion

    #region Толщина

    /// <summary>
    /// Установить толщину границ диапазона.
    /// Устанавливаются внешние и внутренние границы.
    /// Диагональные линии не устанавливаются.
    /// </summary>
    public XlBorderWeight Weight
    {
      get
      {
        XlBorderWeight Value;
        GetWeight(out Value);
        return Value;
      }
      set
      {
        SetWeight(value);
      }
    }

    public bool GetWeight(out XlBorderWeight value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=129]");
      if (x == null)
      {
        value = XlBorderWeight.xlHairline;
        return false;
      }
      else
      {
        value = (XlBorderWeight)x;
        return true;
      }
    }

    public void SetWeight(XlBorderWeight value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=120]", (int)value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Excel
    /// </summary>
    public Int32 Color
    {
      get
      {
        Int32 value;
        GetColor(out value);
        return value;
      }
      set
      {
        SetColor(value);
      }
    }

    public bool GetColor(out Int32 value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=99]");
      if (x == null)
      {
        value = 0x00FFFFFF;
        return false;
      }
      else
      {
        value = (Int32)x;
        return true;
      }
    }
    public void SetColor(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=99]", value);
    }

    #endregion
  }

  #endregion

  #region Заполнение

  public struct Interior
  {
    #region Конструктор

    public Interior(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Цвет

    /// <summary>
    /// Цвет в представлении Excel
    /// </summary>
    public Int32 Color
    {
      get
      {
        Int32 value;
        GetColor(out value);
        return value;
      }
      set
      {
        SetColor(value);
      }
    }

    public bool GetColor(out Int32 value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=99]");
      if (x == null)
      {
        value = 0x00FFFFFF;
        return false;
      }
      else
      {
        value = (Int32)x;
        return true;
      }
    }

    public void SetColor(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=99]", value);
    }

    #endregion

    #endregion
  }

  #endregion

  #region Шрифт

  public struct Font
  {
    #region Конструктор

    public Font(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Гаринитура

    public string Name
    {
      get
      {
        string value;
        GetName(out value);
        return value;
      }
      set { SetName(value); }
    }

    public bool GetName(out string value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=110]");
      if (x == null)
      {
        value = String.Empty;
        return false;
      }
      else
      {
        value = (string)x;
        return true;
      }
    }

    public void SetName(string value)
    {
      if (String.IsNullOrEmpty(value))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("value");
      Base.Helper.SetProp(Base.Obj, "[DispID=110]", value);
    }

    #endregion

    #region Размер

    /// <summary>
    /// Высота шрифта в пунктах
    /// </summary>
    public double Size
    {
      get
      {
        double value;
        GetSize(out value);
        return value;
      }
      set { SetSize(value); }
    }

    public bool GetSize(out double value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=104]");
      if (x == null)
      {
        value = 0.0;
        return false;
      }
      else
      {
        value = (double)x;
        return true;
      }
    }

    public void SetSize(double value)
    {
      if (value <= 0.0)
        throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

      Base.Helper.SetProp(Base.Obj, "[DispID=104]", value);
    }

    #endregion

    #region Атрибуты шрифта

    public bool Bold
    {
      get
      {
        bool value;
        GetBold(out value);
        return value;
      }
      set { SetBold(value); }
    }

    public bool GetBold(out bool value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=96]");
      if (x == null)
      {
        value = false;
        return false;
      }
      else
      {
        value = (bool)x;
        return true;
      }
    }

    public void SetBold(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=96]", value);
    }

    public bool Italic
    {
      get
      {
        bool value;
        GetItalic(out value);
        return value;
      }
      set { SetItalic(value); }
    }

    public bool GetItalic(out bool value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=101]");
      if (x == null)
      {
        value = false;
        return false;
      }
      else
      {
        value = (bool)x;
        return true;
      }
    }

    public void SetItalic(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=101]", value);
    }

    public bool Underline
    {
      get
      {
        bool value;
        GetUnderline(out value);
        return value;
      }
      set { SetUnderline(value); }
    }

    public bool GetUnderline(out bool value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=106]");
      if (x == null)
      {
        value = false;
        return false;
      }
      else
      {
        value = (bool)x;
        return true;
      }
    }

    public void SetUnderline(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=106]", value);
    }

    public bool Strikeout
    {
      get
      {
        bool value;
        GetStrikeout(out value);
        return value;
      }
      set { SetStrikeout(value); }
    }

    public bool GetStrikeout(out bool value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=105]");
      if (x == null)
      {
        value = false;
        return false;
      }
      else
      {
        value = (bool)x;
        return true;
      }
    }

    public void SetStrikeout(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=105]", value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Excel
    /// </summary>
    public Int32 Color
    {
      get
      {
        Int32 value;
        GetColor(out value);
        return value;
      }
      set
      {
        SetColor(value);
      }
    }

    public bool GetColor(out Int32 value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=99]");
      if (x == null)
      {
        value = 0x00FFFFFF;
        return false;
      }
      else
      {
        value = (Int32)x;
        return true;
      }
    }

    public void SetColor(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=99]", value);
    }

    #endregion

    #endregion
  }

  #endregion

  #region Имена ячеек

  public struct Names
  {
    #region Конструктор

    public Names(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Методы

    public Name Add(string name, Range range)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
      if (range.Base.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("range");
      string sheetName = range.Worksheet.Name;
      string refTo = "=" + Models.SpreadsheetBase.SpreadsheetTools.GetQuotedSheetName(sheetName) + "!" + 
        range.RangeRef.ToString(Models.SpreadsheetBase.CellRefFormat.Abs);
      Name nm = new Name(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=181]", name, refTo), Base.Helper));
      return nm;
    }

    #endregion
  }


  public struct Name
  {
    #region Конструктор

    public Name(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    /// <summary>
    /// Имя. Свойство не может иметь такое же имя, как и класс
    /// </summary>
    public string NameStr
    {
      get { return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=110]")); }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("value");

        Base.Helper.SetProp(Base.Obj, "[DispID=110]", value);
      }
    }

    #endregion
  }

  #endregion

  #region Гиперссылки

  public struct Hyperlinks
  {
    #region Конструктор

    public Hyperlinks(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Методы

    public void Add(Range anchor, string address)
    {
      Add(anchor, address, String.Empty);
    }

    public void Add(Range anchor, string address, string subAddress)
    {
      if (anchor.Base.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("anchor");

      // Может быть задан subAddress без address
      if (String.IsNullOrEmpty(address) && String.IsNullOrEmpty(subAddress))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("address");

      Base.Helper.Call(Base.Obj, "[DispID=181]", anchor.Base.Obj, address, subAddress);
    }

    #endregion
  }

  #endregion

  #region Параметры страницы

  public struct PageSetup
  {
    #region Конструктор

    public PageSetup(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Размер бумаги

    public XlPaperSize PaperSize
    {
      get { return GetPaperSize(); }
      set { SetPaperSize(value); }
    }

    public XlPaperSize GetPaperSize()
    {
      return (XlPaperSize)(Base.Helper.GetProp(Base.Obj, "[DispID=1007]"));
    }

    public void SetPaperSize(XlPaperSize value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1007]", value);
    }

    #endregion

    #region Ориентация бумаги

    public XlPageOrientation Orientation
    {
      get { return GetOrientation(); }
      set { SetOrientation(value); }
    }

    public XlPageOrientation GetOrientation()
    {
      return (XlPageOrientation)(Base.Helper.GetProp(Base.Obj, "[DispID=134]"));
    }

    public void SetOrientation(XlPageOrientation value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=134]", value);
    }

    #endregion

    #region Поля

    #region Размеры в пунктах

    public double LeftMargin
    {
      get { return GetLeftMargin(); }
      set { SetLeftMargin(value); }
    }

    public double GetLeftMargin()
    {
      return (double)(Base.Helper.GetProp(Base.Obj, "[DispID=999]"));
    }

    public void SetLeftMargin(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=999]", value);
    }

    public double TopMargin
    {
      get { return GetTopMargin(); }
      set { SetTopMargin(value); }
    }

    public double GetTopMargin()
    {
      return (double)(Base.Helper.GetProp(Base.Obj, "[DispID=1001]"));
    }

    public void SetTopMargin(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1001]", value);
    }

    public double RightMargin
    {
      get { return GetRightMargin(); }
      set { SetRightMargin(value); }
    }

    public double GetRightMargin()
    {
      return (double)(Base.Helper.GetProp(Base.Obj, "[DispID=1000]"));
    }

    public void SetRightMargin(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1000]", value);
    }

    public double BottomMargin
    {
      get { return GetBottomMargin(); }
      set { SetBottomMargin(value); }
    }

    public double GetBottomMargin()
    {
      return (double)(Base.Helper.GetProp(Base.Obj, "[DispID=1002]"));
    }

    public void SetBottomMargin(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1002]", value);
    }

    #endregion

    #region Размеры в единицах 0.1 мм

    public void SetMarginsLM(int left, int top, int right, int bottom)
    {
      SetLeftMargin((double)left * 72.0 / 254.0);
      SetTopMargin((double)top * 72.0 / 254.0);
      SetRightMargin((double)right * 72.0 / 254.0);
      SetBottomMargin((double)bottom * 72.0 / 254.0);
    }

    public void GetMarginsLM(out int left, out int top, out int right, out int bottom)
    {
      left = (int)(Math.Round(GetLeftMargin() / 72.0 * 254.0));
      top = (int)(Math.Round(GetTopMargin() / 72.0 * 254.0));
      right = (int)(Math.Round(GetRightMargin() / 72.0 * 254.0));
      bottom = (int)(Math.Round(GetBottomMargin() / 72.0 * 254.0));
    }

    #endregion

    #endregion

    #region Выравнивание

    public bool CenterVerically
    {
      get { return GetCenterVertically(); }
      set { SetCenterVertically(value); }
    }

    public bool GetCenterVertically()
    {
      return (bool)(Base.Helper.GetProp(Base.Obj, "[DispID=1006]"));
    }

    public void SetCenterVertically(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1006]", value);
    }

    public bool CenterHorizontally
    {
      get { return GetCenterHorizontally(); }
      set { SetCenterHorizontally(value); }
    }

    public bool GetCenterHorizontally()
    {
      return (bool)(Base.Helper.GetProp(Base.Obj, "[DispID=1005]"));
    }

    public void SetCenterHorizontally(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1005]", value);
    }

    #endregion

    #region Масштабирование

    /// <summary>
    /// Масштаб.
    /// Если установлен режим подбора по числу страниц по ширине/высоте, возвращает 0
    /// </summary>
    public double Zoom
    {
      get { return DataTools.GetDouble(Base.Helper.GetProp(Base.Obj, "[DispID=663]")); }
    }

    /// <summary>
    /// Установить масштаб от 10 до 400%.
    /// </summary>
    /// <param name="value"></param>
    public void SetZoom(double value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=663]", value);
    }

    /// <summary>
    /// Размещение по ширине на заданном числе страниц.
    /// Cвойство имеет значение, только если был вызов <see cref="SetFitToPages(int, int)"/>.
    /// </summary>
    public int FitToPagesWide
    {
      get { return DataTools.GetInt32(Base.Helper.GetProp(Base.Obj, "[DispID=1014]")); }
    }
    /// <summary>
    /// Размещение по высоте на заданном числе страниц.
    /// Cвойство имеет значение, только если был вызов <see cref="SetFitToPages(int, int)"/>.
    /// </summary>
    public int FitToPagesTall
    {
      get { return DataTools.GetInt32(Base.Helper.GetProp(Base.Obj, "[DispID=1013]")); }
    }

    /// <summary>
    /// Установить размещение на заданном числе страниц по высоте и ширине.
    /// Если одно из значений равно 0, то для этого параметра используется значение "Авто".
    /// Свойство <see cref="Zoom"/> отключается.
    /// </summary>
    /// <param name="wide">Количество страниц по ширине</param>
    /// <param name="tall">Количество страниц по высоте</param>
    public void SetFitToPages(int wide, int tall)
    {
      if (wide < 0)
        throw ExceptionFactory.ArgOutOfRange("wide", wide, 0, null);
      if (tall < 0)
        throw ExceptionFactory.ArgOutOfRange("tall", tall, 0, null);

      Base.Helper.SetProp(Base.Obj, "[DispID=663]", false);

      if (wide == 0)
        Base.Helper.SetProp(Base.Obj, "[DispID=1014]", false);
      else
        Base.Helper.SetProp(Base.Obj, "[DispID=1014]", wide);

      if (tall == 0)
        Base.Helper.SetProp(Base.Obj, "[DispID=1013]", false);
      else
        Base.Helper.SetProp(Base.Obj, "[DispID=1013]", tall);
    }

    public void SetFitToPagesTall(int value)
    {
      if (value < 0)
        throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

      if (value == 0)
        Base.Helper.SetProp(Base.Obj, "[DispID=1013]", false);
      else
      {
        Base.Helper.SetProp(Base.Obj, "[DispID=663]", false);
        Base.Helper.SetProp(Base.Obj, "[DispID=1013]", value);
      }
    }

    #endregion

    #region Сквозные строки и столбцы

    /// <summary>
    /// Сквозные строки. Задаются в формате "$1:$3".
    /// Пустая строка - нет.
    /// </summary>
    public string PrintTitleRows
    {
      get { return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=1024]")); }
    }

    /// <summary>
    /// Сквозные строки. Задаются в формате "$1:$3"
    /// Пустая строка - нет.
    /// </summary>
    /// <param name="value"></param>
    public void SetPrintTitleRows(string value)
    {
      if (value == null)
        value = String.Empty;

      Base.Helper.SetProp(Base.Obj, "[DispID=1024]", value);
    }


    /// <summary>
    /// Сквозные столбцы. Задаются в формате "$A:$C".
    /// Пустая строка - нет.
    /// </summary>
    public string PrintTitleColumns
    {
      get { return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=1023]")); }
    }

    /// <summary>
    /// Сквозные строки. Задаются в формате "$1:$3"
    /// Пустая строка - нет.
    /// </summary>
    /// <param name="value"></param>
    public void SetPrintTitleColumns(string value)
    {
      if (value == null)
        value = String.Empty;
      Base.Helper.SetProp(Base.Obj, "[DispID=1023]", value);
    }

    #endregion
  }

  #endregion

  #region Свойства документа

  public struct DocumentProperties
  {
    #region Конструктор

    public DocumentProperties(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public DocumentProperty this[string name]
    {
      get
      {
        return new DocumentProperty(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]", name), Base.Helper));
      }
    }

    #endregion
  }

  public struct DocumentProperty
  {
    #region Конструктор

    public DocumentProperty(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public string Name { get { return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=3]")); } }

    public void SetName(string value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=3]", value);
    }

    public object Value { get { return Base.Helper.GetProp(Base.Obj, "[DispID=0]"); } }

    public void SetValue(object value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=0]", value);
    }

    #endregion
  }

  #endregion

  #region Окна

  public struct Windows
  {
    #region Конструктор

    public Windows(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает количество окон (обычно, 1)
    /// </summary>
    public int Count
    {
      get { return DataTools.GetInt32(Base.Helper.GetProp(Base.Obj, "[DispID=118]")); }
    }

    /// <summary>
    /// Выбор окна по индексу. Нумерация начинается с 1.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Window this[int index]
    {
      get
      {
        return new Window(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    #endregion
  }

  public struct Window
  {
    #region Конструктор

    public Window(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    /// <summary>
    /// Режим просмотра: обычный или просмотр разметки
    /// </summary>
    public XlWindowView View { get { return (XlWindowView)Base.Helper.GetProp(Base.Obj, "[DispID=1194]"); } }

    public void SetView(XlWindowView value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1194]", value);
    }

    #endregion
  }


  #endregion
}
