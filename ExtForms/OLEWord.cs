using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Drawing;
using FreeLibSet.IO;
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

#pragma warning disable 1591

namespace FreeLibSet.OLE.Word
{
  #region Перечисления

  /* public enum WdConstants
  {
    wdBackward = -1073741823,
    wdAutoPosition = 0,
    wdFirst = 1,
    wdToggle = 9999998,
    wdUndefined = 9999999,
    wdForward = 1073741823,
    wdCreatorCode = 1297307460,
  }  */

  public enum WdUnderline
  {
    wdUnderlineNone = 0,
    wdUnderlineSingle = 1,
    wdUnderlineWords = 2,
    wdUnderlineDouble = 3,
    wdUnderlineDotted = 4,
    wdUnderlineThick = 6,
    wdUnderlineDash = 7,
    wdUnderlineDotDash = 9,
    wdUnderlineDotDotDash = 10,
    wdUnderlineWavy = 11,
    wdUnderlineDottedHeavy = 20,
    wdUnderlineDashHeavy = 23,
    wdUnderlineDotDashHeavy = 25,
    wdUnderlineDotDotDashHeavy = 26,
    wdUnderlineWavyHeavy = 27,
    wdUnderlineDashLong = 39,
    wdUnderlineWavyDouble = 43,
    wdUnderlineDashLongHeavy = 55,
  }

  public enum WdRowHeightRule
  {
    wdRowHeightAuto = 0,
    wdRowHeightAtLeast = 1,
    wdRowHeightExactly = 2,
  }

  public enum WdPaperSize
  {
    wdPaper10x14 = 0,
    wdPaper11x17 = 1,
    wdPaperLetter = 2,
    wdPaperLetterSmall = 3,
    wdPaperLegal = 4,
    wdPaperExecutive = 5,
    wdPaperA3 = 6,
    wdPaperA4 = 7,
    wdPaperA4Small = 8,
    wdPaperA5 = 9,
    wdPaperB4 = 10,
    wdPaperB5 = 11,
    wdPaperCSheet = 12,
    wdPaperDSheet = 13,
    wdPaperESheet = 14,
    wdPaperFanfoldLegalGerman = 15,
    wdPaperFanfoldStdGerman = 16,
    wdPaperFanfoldUS = 17,
    wdPaperFolio = 18,
    wdPaperLedger = 19,
    wdPaperNote = 20,
    wdPaperQuarto = 21,
    wdPaperStatement = 22,
    wdPaperTabloid = 23,
    wdPaperEnvelope9 = 24,
    wdPaperEnvelope10 = 25,
    wdPaperEnvelope11 = 26,
    wdPaperEnvelope12 = 27,
    wdPaperEnvelope14 = 28,
    wdPaperEnvelopeB4 = 29,
    wdPaperEnvelopeB5 = 30,
    wdPaperEnvelopeB6 = 31,
    wdPaperEnvelopeC3 = 32,
    wdPaperEnvelopeC4 = 33,
    wdPaperEnvelopeC5 = 34,
    wdPaperEnvelopeC6 = 35,
    wdPaperEnvelopeC65 = 36,
    wdPaperEnvelopeDL = 37,
    wdPaperEnvelopeItaly = 38,
    wdPaperEnvelopeMonarch = 39,
    wdPaperEnvelopePersonal = 40,
    wdPaperCustom = 41,
  }

  public enum WdOrientation
  {
    wdOrientPortrait = 0,
    wdOrientLandscape = 1,
  }

  public enum WdBorderType
  {
    wdBorderDiagonalUp = -8,
    wdBorderDiagonalDown = -7,
    wdBorderVertical = -6,
    wdBorderHorizontal = -5,
    wdBorderRight = -4,
    wdBorderBottom = -3,
    wdBorderLeft = -2,
    wdBorderTop = -1,
  }

  public enum WdLineStyle
  {
    wdLineStyleNone = 0,
    wdLineStyleSingle = 1,
    wdLineStyleDot = 2,
    wdLineStyleDashSmallGap = 3,
    wdLineStyleDashLargeGap = 4,
    wdLineStyleDashDot = 5,
    wdLineStyleDashDotDot = 6,
    wdLineStyleDouble = 7,
    wdLineStyleTriple = 8,
    wdLineStyleThinThickSmallGap = 9,
    wdLineStyleThickThinSmallGap = 10,
    wdLineStyleThinThickThinSmallGap = 11,
    wdLineStyleThinThickMedGap = 12,
    wdLineStyleThickThinMedGap = 13,
    wdLineStyleThinThickThinMedGap = 14,
    wdLineStyleThinThickLargeGap = 15,
    wdLineStyleThickThinLargeGap = 16,
    wdLineStyleThinThickThinLargeGap = 17,
    wdLineStyleSingleWavy = 18,
    wdLineStyleDoubleWavy = 19,
    wdLineStyleDashDotStroked = 20,
    wdLineStyleEmboss3D = 21,
    wdLineStyleEngrave3D = 22,
    wdLineStyleOutset = 23,
    wdLineStyleInset = 24,
  }

  public enum WdLineWidth
  {
    wdLineWidth025pt = 2,
    wdLineWidth050pt = 4,
    wdLineWidth075pt = 6,
    wdLineWidth100pt = 8,
    wdLineWidth150pt = 12,
    wdLineWidth225pt = 18,
    wdLineWidth300pt = 24,
    wdLineWidth450pt = 36,
    wdLineWidth600pt = 48,
  }

  public enum WdCellVerticalAlignment
  {
    wdCellAlignVerticalTop = 0,
    wdCellAlignVerticalCenter = 1,
    wdCellAlignVerticalBottom = 3,
  }

  public enum WdVerticalAlignment
  {
    wdAlignVerticalTop = 0,
    wdAlignVerticalCenter = 1,
    wdAlignVerticalJustify = 2,
    wdAlignVerticalBottom = 3,
  }

  public enum WdParagraphAlignment
  {
    wdAlignParagraphLeft = 0,
    wdAlignParagraphCenter = 1,
    wdAlignParagraphRight = 2,
    wdAlignParagraphJustify = 3,
    wdAlignParagraphDistribute = 4,
    wdAlignParagraphJustifyMed = 5,
    wdAlignParagraphJustifyHi = 7,
    wdAlignParagraphJustifyLow = 8,
    wdAlignParagraphThaiJustify = 9,
  }

  public enum WdTableFieldSeparator
  {
    wdSeparateByParagraphs = 0,
    wdSeparateByTabs = 1,
    wdSeparateByCommas = 2,
    wdSeparateByDefaultListSeparator = 3,
  }

  public enum WdReplace
  {
    wdReplaceNone = 0,
    wdReplaceOne = 1,
    wdReplaceAll = 2,
  }

  public enum WdPreferredWidthType
  {
    wdPreferredWidthAuto = 1,
    wdPreferredWidthPercent = 2,
    wdPreferredWidthPoints = 3,
  }

  public enum WdBreakType
  {
    wdSectionBreakNextPage = 2,
    wdSectionBreakContinuous = 3,
    wdSectionBreakEvenPage = 4,
    wdSectionBreakOddPage = 5,
    wdLineBreak = 6,
    wdPageBreak = 7,
    wdColumnBreak = 8,
    wdLineBreakClearLeft = 9,
    wdLineBreakClearRight = 10,
    wdTextWrappingBreak = 11,
  }

  public enum WdInformation
  {
    wdActiveEndAdjustedPageNumber = 1,
    wdActiveEndSectionNumber = 2,
    wdActiveEndPageNumber = 3,
    wdNumberOfPagesInDocument = 4,
    wdHorizontalPositionRelativeToPage = 5,
    wdVerticalPositionRelativeToPage = 6,
    wdHorizontalPositionRelativeToTextBoundary = 7,
    wdVerticalPositionRelativeToTextBoundary = 8,
    wdFirstCharacterColumnNumber = 9,
    wdFirstCharacterLineNumber = 10,
    wdFrameIsSelected = 11,
    wdWithInTable = 12,
    wdStartOfRangeRowNumber = 13,
    wdEndOfRangeRowNumber = 14,
    wdMaximumNumberOfRows = 15,
    wdStartOfRangeColumnNumber = 16,
    wdEndOfRangeColumnNumber = 17,
    wdMaximumNumberOfColumns = 18,
    wdZoomPercentage = 19,
    wdSelectionMode = 20,
    wdCapsLock = 21,
    wdNumLock = 22,
    wdOverType = 23,
    wdRevisionMarking = 24,
    wdInFootnoteEndnotePane = 25,
    wdInCommentPane = 26,
    wdInHeaderFooter = 28,
    wdAtEndOfRowMarker = 31,
    wdReferenceOfType = 32,
    wdHeaderFooterType = 33,
    wdInMasterDocument = 34,
    wdInFootnote = 35,
    wdInEndnote = 36,
    wdInWordMail = 37,
    wdInClipboard = 38,
  }

  #endregion

  #region Основной объект

  public class WordHelper : OLEHelper
  {
    #region Конструктор и Disposing

    public WordHelper()
    {
      _ShowOnEnd = true;
      CreateMainObj("Word.Application");
      _Application = new Application(new ObjBase(MainObj, this));
      MicrosoftOfficeTools.InitOleHelper(this, _Application.Base, "[DispID=24]"); // Свойство "Version"
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (MainObj != null)
        {
          if (ShowOnEnd)
            SetProp(MainObj, "[DispID=23]", true); // VisibleEx
          else
            Call(MainObj, "[DispID=1105]"); // Quit
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Способ завершения

    /// <summary>
    /// Если установлено в true (по умолчанию), то при завершении работы с помощником
    /// приложение Word будет выведено на экран, иначе оно будет завершено
    /// </summary>
    public bool ShowOnEnd { get { return _ShowOnEnd; } set { _ShowOnEnd = value; } }
    private bool _ShowOnEnd;

    #endregion

    #region Основной объект

    public Application Application { get { return _Application; } }
    private Application _Application;

    #endregion

    #region Статические методы и свойства

    /// <summary>
    /// Преобразование объекта ColorEx в RGB-представление
    /// Для ColorEx.Empty возвращает автоматический цвет
    /// </summary>
    /// <param name="value">Цвет в .Net framework</param>
    /// <returns>Цвет в Microsoft Excel</returns>
    public static Int32 ColorToRgb(Color value)
    {
      if (value.IsEmpty)
        return wdColorAutomatic;
#if DEBUG
      if (value.IsSystemColor)
        throw new ArgumentException("Системные цвета недопустимы", "value");
#endif
      return (((int)(value.B)) << 16) | (((int)(value.G)) << 8) | ((int)(value.R));
    }

    /// <summary>
    /// Преобразование RGB-цвета в обычный ColorEx
    /// </summary>
    /// <param name="value">Цвет в Microsoft Excel</param>
    /// <returns>Цвет в .Net framework</returns>
    public static Color RgbToColor(Int32 value)
    {
      if (value == wdColorAutomatic)
        return Color.Empty;
      int r = value & 0x000000FF;
      int g = (value & 0x0000FF00) >> 8;
      int b = (value & 0x00FF0000) >> 16;
      return Color.FromArgb(r, g, b);
    }

    /// <summary>
    /// Преобразования полученного значения свойства вида True/False/Undefined
    /// в формат функции чтения свойства
    /// </summary>
    /// <param name="x">Полученно от Word'а значение</param>
    /// <param name="value">Возрвращаемый по ссылке результат</param>
    /// <returns>true, если значение получено, false-нет значения</returns>
    public static bool GetBoolValue(int x, out bool value)
    {
      switch (x)
      {
        case wdFalse:
          value = false;
          return true;
        case wdTrue:
          value = true;
          return true;
        default:
          value = false;
          return false;
      }
    }

    public static bool GetColorIntValue(Int32 x, out Int32 value)
    { 
      // ?? Не знаю, как обрабатываются смешанные цвета
      value = x;
      return true;
    }

    #endregion

    #region Константы

    // Эти константы заданы в перечислении WdConstants, но их неудобно использовать

    public const int wdBackward = -1073741823;
    public const int wdAutoPosition = 0;
    public const int wdFirst = 1;
    public const int wdToggle = 9999998;
    public const int wdUndefined = 9999999;
    public const int wdForward = 1073741823;
    public const int wdCreatorCode = 1297307460;

    /// <summary>
    /// Автоматический цвет
    /// Константа из перечисления WdColor
    /// </summary>
    public const int wdColorAutomatic = -16777216; // 0xFF000000

    // Признаки часто представлены типом int, а не bool
    public const int wdTrue = -1;
    public const int wdFalse = 0;

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
      _Documents = new Documents();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public Documents Documents
    {
      get
      {
        if (_Documents.Base.IsEmpty)
        {
          _Documents = new Documents(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=6]"), Base.Helper));
        }
        return _Documents;
      }
    }
    private Documents _Documents;


    /// <summary>
    /// Версия программы, например "11.0" для Word-2003
    /// </summary>
    public string VersionStr
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=24]"));
      }
    }

    /// <summary>
    /// Номер построения в виде "11.0.1234"
    /// </summary>
    public string BuildStr
    {
      get
      {
        return DataTools.GetString(Base.Helper.GetProp(Base.Obj, "[DispID=47]"));
      }
    }

    /// <summary>
    /// Получить версию Excel как объект
    /// </summary>
    public Version Version
    {
      get
      {
        string BldStr = BuildStr;
        return FileTools.GetVersionFromStr(BldStr);
      }
    }

    #endregion

    #region Методы

#if XXXX

    // Есть свойство WordHelper.ShowOnEnd
    public void Quit()
    {
      Base.Helper.Call(Base.Obj, "[DispID=1105]");
    }

#endif

    #endregion
  }

  #endregion

  #region Документ

  public struct Document
  {
    #region Конструктор

    public Document(ObjBase theBase)
    {
      _Base = theBase;
      _Tables = new Tables();
      _PageSetup = new PageSetup();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Сохранение документа

    public void Save()
    {
      Base.Helper.Call(Base.Obj, "[DispID=108]");
    }

    public void SaveAs(string fileName)
    {
      Base.Helper.Call(Base.Obj, "[DispID=376]", fileName);
    }

    public void Close()
    {
      Base.Helper.Call(Base.Obj, "[DispID=1105]");
    }

    /// <summary>
    /// Установка свойства в true предотвращает появление сообщение "Документ не сохранен" при закрытии документа
    /// </summary>
    public bool Saved
    {
      get
      {
        return (bool)(Base.Helper.GetProp(Base.Obj, "[DispID=40]"));
      }
      set
      {
        Base.Helper.SetProp(Base.Obj, "[DispID=40]", value);
      }
    }

    #endregion

    #region Таблицы

    public Tables Tables
    {
      get
      {
        if (_Tables.Base.IsEmpty)
          _Tables = new Tables(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=6]"), Base.Helper));
        return _Tables;
      }
    }
    private Tables _Tables;

    #endregion

    #region Параметры страницы

    /// <summary>
    /// Параметры страницы для документа в-целом
    /// </summary>
    public PageSetup PageSetup
    {
      get
      { 
      //global::Word.Document
        if (_PageSetup.Base.IsEmpty)
          _PageSetup = new PageSetup(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1101]"), Base.Helper));
        return _PageSetup;
      }
    }
    private PageSetup _PageSetup;

    #endregion

    #region Диапазоны

    /// <summary>
    /// Получить содержимое всего документа
    /// </summary>
    /// <returns></returns>
    public Range Range()
    {
      return new Range(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=2000]"), Base.Helper));
    }

    /// <summary>
    /// Получить содержимое части документа
    /// </summary>
    /// <param name="start">Начальная позиция в символах</param>
    /// <param name="end">Конечная позиция в символах</param>
    /// <returns></returns>
    public Range Range(int start, int end)
    {
      return new Range(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=2000]", start, end), Base.Helper));
    }

    // ?? не работают
    //public Range Range(Range Start, Range End)
    //{
    //  return new Range(new ObjBase(AnyDocType.Helper.Call(AnyDocType.Obj, "[DispID=2000]", Start.AnyDocType.Obj, End.AnyDocType.Obj), AnyDocType.Helper));
    //}
    //public Range Range(Cell Start, Cell End)
    //{
    //  return new Range(new ObjBase(AnyDocType.Helper.Call(AnyDocType.Obj, "[DispID=2000]", Start.AnyDocType.Obj, End.AnyDocType.Obj), AnyDocType.Helper));
    //}

    /// <summary>
    /// Получить диапазон для вставки текста в начало документа
    /// </summary>
    /// <returns></returns>
    public Range StartDoc()
    {
      Range r = Range();
      return Range(r.Start, r.Start);
    }

    public Range EndDoc()
    {
      Range r = Range();
      return Range(r.End - 1, r.End - 1);
    }

    #endregion

    #region Закладки

    public Bookmarks Bookmarks
    {
      get
      {
        if (_Bookmarks.Base.IsEmpty)
          _Bookmarks = new Bookmarks(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=4]"), Base.Helper));
        return _Bookmarks;
      }
    }
    private Bookmarks _Bookmarks;

    #endregion
  }

  public struct Documents
  {
    #region Конструктор

    public Documents(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Методы

    /// <summary>
    /// Создать пустой новый документ
    /// </summary>
    /// <returns></returns>
    public Document Add()
    {
      //global::Word.Document
      Document doc = new Document(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=14]", Missing.Value, Missing.Value), Base.Helper));
      return doc;
    }

    /// <summary>
    /// Открыть документ Word
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns></returns>
    public Document Open(string fileName)
    {
      return new Document(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=19]", fileName), Base.Helper));
    }

    #endregion
  }

  #endregion

  #region Диапазон

  public struct Range
  {
    #region Конструктор

    public Range(ObjBase theBase)
    {
      //global::Word.Font
      _Base = theBase;
      //FDocument = new Document();
      _Font = new Font();
      _ParagraphFormat = new ParagraphFormat();
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Владелец

    // Свойство Range.Document есть в Word-2003, но нет в Word-2000
    //public Document Document
    //{
    //  get
    //  {
    //    if (FDocument.AnyDocType.IsEmpty)
    //      FDocument = new Document(new ObjBase(AnyDocType.Helper.GetProp(AnyDocType.Obj, "[DispID=409]"), AnyDocType.Helper));
    //    return FDocument;
    //  }
    //}
    //private Document FDocument;

    #endregion

    #region Текст

    public string Text
    {
      get { return GetText(); }
      set { SetText(value); }
    }

    private string GetText()
    {
      return (string)(Base.Helper.GetProp(Base.Obj, "[DispID=0]"));
    }

    private void SetText(string value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=0]", value);
    }

    /// <summary>
    /// Вставка текста в начало диапазлна
    /// </summary>
    /// <param name="text">Вставляемый текст</param>
    public void InsertBefore(string text)
    {
      Base.Helper.Call(Base.Obj, "[DispID=102]", text);
    }

    /// <summary>
    /// Вставка текста в конец диапазона
    /// </summary>
    /// <param name="text">Вставляемый текст</param>
    public void InsertAfter(string text)
    {
      Base.Helper.Call(Base.Obj, "[DispID=104]", text);
    }

    /// <summary>
    /// Вставить разрыв
    /// </summary>
    /// <param name="breakType"></param>
    public void InsertBreak(WdBreakType breakType)
    {
      Base.Helper.Call(Base.Obj, "[DispID=122]", (int)breakType);
    }

    #endregion

    #region Диапазон символов

    /// <summary>
    /// Начальная позиция диапазона в символах
    /// </summary>
    public int Start
    {
      get { return GetStart(); }
      set { SetStart(value); }
    }

    private int GetStart()
    {
      return (int)Base.Helper.GetProp(Base.Obj, "[DispID=3]");
    }

    private void SetStart(int value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=3]", value);
    }

    /// <summary>
    /// Конечная позиция диапазона в символах
    /// </summary>
    public int End
    {
      get { return GetEnd(); }
      set { SetEnd(value); }
    }

    private int GetEnd()
    {
      return (int)Base.Helper.GetProp(Base.Obj, "[DispID=4]");
    }

    private void SetEnd(int value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=4]", value);
    }


    #endregion

    #region Шрифт

    public Font Font
    {
      get
      {
        if (_Font.Base.IsEmpty)
          _Font = new Font(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=5]"), Base.Helper));
        return _Font;
      }
    }
    private Font _Font;

    /*
    #region Атрибуты шрифта

    public bool Bold
    {
      get
      {
        bool ValueEx;
        GetBold(out ValueEx);
        return ValueEx;
      }
      set { SetBold(value); }
    }
    public bool GetBold(out bool ValueEx)
    {
      int x = (int)(AnyDocType.Helper.GetProp(AnyDocType.Obj, "[DispID=130]"));
      switch (x)
      { 
        case 0:
          ValueEx = false;
          return true;
        case -1:
          ValueEx = true;
          return true;
        default:
          ValueEx = false;
          return false;
      }
    }
    public void SetBold(bool ValueEx)
    {
      AnyDocType.Helper.SetProp(AnyDocType.Obj, "[DispID=130]", ValueEx);
    }

    ////public bool Italic
    ////{
    ////  get
    ////  {
    ////    bool ValueEx;
    ////    GetItalic(out ValueEx);
    ////    return ValueEx;
    ////  }
    ////  set { SetItalic(value); }
    ////}
    ////public bool GetItalic(out bool ValueEx)
    ////{
    ////  object x = AnyDocType.Helper.GetProp(AnyDocType.Obj, "[DispID=101]");
    ////  if (x == null)
    ////  {
    ////    ValueEx = false;
    ////    return false;
    ////  }
    ////  else
    ////  {
    ////    ValueEx = (bool)x;
    ////    return true;
    ////  }
    ////}
    ////public void SetItalic(bool ValueEx)
    ////{
    ////  AnyDocType.Helper.SetProp(AnyDocType.Obj, "[DispID=101]", ValueEx);
    ////}

    ////public bool Underline
    ////{
    ////  get
    ////  {
    ////    bool ValueEx;
    ////    GetUnderline(out ValueEx);
    ////    return ValueEx;
    ////  }
    ////  set { SetUnderline(value); }
    ////}
    ////public bool GetUnderline(out bool ValueEx)
    ////{
    ////  object x = AnyDocType.Helper.GetProp(AnyDocType.Obj, "[DispID=106]");
    ////  if (x == null)
    ////  {
    ////    ValueEx = false;
    ////    return false;
    ////  }
    ////  else
    ////  {
    ////    ValueEx = (bool)x;
    ////    return true;
    ////  }
    ////}
    ////public void SetUnderline(bool ValueEx)
    ////{
    ////  AnyDocType.Helper.SetProp(AnyDocType.Obj, "[DispID=106]", ValueEx);
    ////}

    #endregion
  */

    #endregion

    #region Абзац

    public ParagraphFormat ParagraphFormat
    {
      get
      {
        if (_ParagraphFormat.Base.IsEmpty)
          _ParagraphFormat = new ParagraphFormat(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1102]"), Base.Helper));
        return _ParagraphFormat;
      }
    }

    private ParagraphFormat _ParagraphFormat;

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=61]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion

    #region Таблица

    public Tables Tables
    {
      get
      {
        return new Tables(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=50]"), Base.Helper));
      }
    }

    public Rows Rows
    {
      get
      {
        return new Rows(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=303]"), Base.Helper));
      }
    }

    public Columns Columns
    {
      get
      {
        return new Columns(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=302]"), Base.Helper));
      }
    }

    public Cells Cells
    {
      get
      {
        return new Cells(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=57]"), Base.Helper));
      }
    }

    /// <summary>
    /// Преобразование текста в таблицу
    /// </summary>
    /// <param name="separator"></param>
    /// <param name="numRows"></param>
    /// <param name="numColumns"></param>
    public Table ConvertToTable(WdTableFieldSeparator separator, int numRows, int numColumns)
    {
      return new Table(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=498]", (int)separator, numRows, numColumns), Base.Helper));
    }

    #endregion

    #region Закладки

    public Bookmarks Bookmarks
    {
      get
      {
        return new Bookmarks(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=75]"), Base.Helper));
      }
    }

    #endregion

    #region Информация

    public object GetInformation(WdInformation infoType)
    {
      return Base.Helper.GetIndexProp(Base.Obj, "[DispID=313]", (int)infoType);
    }

    #endregion

    #region Поиск и замена

    public Find Find
    {
      get 
      {
        return new Find(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=262]"), Base.Helper));
      }
    }

    #endregion

    #region Вставка файла

    public void InsertFile(string fileName)
    {
      Base.Helper.Call(Base.Obj, "[DispID=123]", fileName);
    }

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
        string Value;
        GetName(out Value);
        return Value;
      }
      set { SetName(value); }
    }

    public bool GetName(out string value)
    {
      object x = Base.Helper.GetProp(Base.Obj, "[DispID=142]");
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
        throw new ArgumentNullException("value", "Имя шрифта не может быть пустым");
      Base.Helper.SetProp(Base.Obj, "[DispID=142]", value);
    }

    #endregion

    #region Размер

    /// <summary>
    /// Высота шрифта в пунктах
    /// </summary>
    public float Size
    {
      get
      {
        float Value;
        GetSize(out Value);
        return Value;
      }
      set { SetSize(value); }
    }

    public bool GetSize(out float value)
    {
      //global::Word.WdConstants
      float x = (float)(Base.Helper.GetProp(Base.Obj, "[DispID=141]"));
      if (x == (float)(WordHelper.wdUndefined))
      {
        value = 0f;
        return false;
      }
      else
      {
        value = x;
        return true;
      }
    }

    public void SetSize(float value)
    {
      if (value <= 0.0)
        throw new ArgumentException("Размер шрифта должен быть больше 0", "value");
      Base.Helper.SetProp(Base.Obj, "[DispID=141]", value);
    }

    #endregion

    #region Атрибуты шрифта

    public bool Bold
    {
      get
      {
        bool Value;
        GetBold(out Value);
        return Value;
      }
      set { SetBold(value); }
    }

    public bool GetBold(out bool value)
    {
      int x = (int)(Base.Helper.GetProp(Base.Obj, "[DispID=130]"));
      return WordHelper.GetBoolValue(x, out value);
    }

    public void SetBold(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=130]", value);
    }

    public bool Italic
    {
      get
      {
        bool Value;
        GetItalic(out Value);
        return Value;
      }
      set { SetItalic(value); }
    }

    public bool GetItalic(out bool value)
    {
      int x = (int)(Base.Helper.GetProp(Base.Obj, "[DispID=131]"));
      return WordHelper.GetBoolValue(x, out value);
    }

    public void SetItalic(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=131]", value);
    }

    public WdUnderline Underline
    {
      get
      {
        WdUnderline Value;
        GetUnderline(out Value);
        return Value;
      }
      set { SetUnderline(value); }
    }

    public bool GetUnderline(out WdUnderline value)
    {
      value = (WdUnderline)(Base.Helper.GetProp(Base.Obj, "[DispID=140]"));
      // ???
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdUnderline.wdUnderlineNone;
        return false;
      }
      return true;
    }

    public void SetUnderline(WdUnderline value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=140]", value);
    }

    public bool Strikeout
    {
      get
      {
        bool Value;
        GetStrikeout(out Value);
        return Value;
      }
      set { SetStrikeout(value); }
    }

    public bool GetStrikeout(out bool value)
    {
      int x = (int)(Base.Helper.GetProp(Base.Obj, "[DispID=135]"));
      return WordHelper.GetBoolValue(x, out value);
    }

    public void SetStrikeout(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=135]", value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Excel
    /// </summary>
    public Int32 ColorInt
    {
      get
      {
        Int32 Value;
        GetColorInt(out Value);
        return Value;
      }
      set
      {
        SetColorInt(value);
      }
    }

    public bool GetColorInt(out Int32 value)
    {
      Int32 x = (Int32)(Base.Helper.GetProp(Base.Obj, "[DispID=159]"));
      return WordHelper.GetColorIntValue(x, out value);
    }

    public void SetColorInt(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=159]", value);
    }

    public Color Color
    {
      get { return WordHelper.RgbToColor(ColorInt); }
      set { ColorInt = WordHelper.ColorToRgb(value); }
    }

    public bool GetColor(out Color value)
    {
      Int32 IntValue;
      bool Res = GetColorInt(out IntValue);
      value = WordHelper.RgbToColor(IntValue);
      return Res;
    }

    public void SetColor(Color value)
    {
      SetColorInt(WordHelper.ColorToRgb(value));
    }

    #endregion

    #endregion
  }

  #endregion

  #region Абзац

  public struct ParagraphFormat
  { 
    #region Конструктор

    public ParagraphFormat(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Выравнивание

    /// <summary>
    /// Вертикальное выравнивание текста на странице
    /// </summary>
    public WdParagraphAlignment Alignment
    {
      get
      {
        WdParagraphAlignment Value;
        GetAlignment(out Value);
        return Value;
      }
      set
      {
        SetAlignment(value);
      }
    }

    public bool GetAlignment(out WdParagraphAlignment value)
    {
      value = (WdParagraphAlignment)(Base.Helper.GetProp(Base.Obj, "[DispID=101]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdParagraphAlignment.wdAlignParagraphLeft;
        return false;
      }
      return true;
    }

    public void SetAlignment(WdParagraphAlignment value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=101]", (int)value);
    }

    #endregion

    #endregion
  }

  #endregion

  #region Таблица

  #region Таблица в-целом

  /// <summary>
  /// Таблица
  /// </summary>
  public struct Table
  {
    #region Конструктор

    public Table(ObjBase theBase)
    {
      _Base = theBase;
      //global::Word.Table
      _Rows = new Rows();
      _Columns = new Columns();
      _Range = new Range();
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Владелец

    //public Document Document
    //{
    //  get { return Range.Document; }
    //}

    #endregion

    #region Доступ к строкам и столбцам

    public Rows Rows
    {
      get
      {
        if (_Rows.Base.IsEmpty)
          _Rows = new Rows(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=101]"), Base.Helper));
        return _Rows;
      }
    }
    private Rows _Rows;

    public Columns Columns
    {
      get
      {
        if (_Columns.Base.IsEmpty)
          _Columns = new Columns(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=100]"), Base.Helper));
        return _Columns;
      }
    }
    private Columns _Columns;

    #endregion

    #region Доступ к ячейкам

    public Cell Cell(int row, int column)
    {
#if DEBUG
      if (row < 1)
        throw new ArgumentOutOfRangeException("row", row, "Неправильный номер строки");
      if (column < 1)
        throw new ArgumentOutOfRangeException("column", column, "Неправильный номер столбца");
#endif
      return new Cell(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=17]", row, column), Base.Helper));
    }

    /// <summary>
    /// Получить массив ячеек в заданном диапазоне
    /// </summary>
    /// <param name="document"></param>
    /// <param name="firstRow"></param>
    /// <param name="firstColumn"></param>
    /// <param name="lastRow"></param>
    /// <param name="lastColumn"></param>
    /// <returns></returns>
    public Cells GetCells(Document document, int firstRow, int firstColumn, int lastRow, int lastColumn)
    {
#if DEBUG
      if (firstRow < 1)
        throw new ArgumentOutOfRangeException("firstRow", firstRow, "Неправильный номер первой строки");
      if (lastRow < firstRow)
        throw new ArgumentOutOfRangeException("lastRow", lastRow, "Неправильный номер последней строки");
      if (firstColumn < 1)
        throw new ArgumentOutOfRangeException("firstColumn", firstColumn, "Неправильный номер первого столбца");
      if (lastColumn < firstColumn)
        throw new ArgumentOutOfRangeException("lastColumn", lastColumn, "Неправильный номер последнего столбца");
#endif
      Range r1=Cell(firstRow, firstColumn).Range;
      Range r2=Cell(lastRow, lastColumn).Range;
      Range r3=document.Range(r1.Start, r2.End);
      return r3.Cells;
    }

    public void MergeCells(int firstRow, int firstColumn, int lastRow, int lastColumn)
    {
#if DEBUG
      if (firstRow < 1)
        throw new ArgumentOutOfRangeException("firstRow", firstRow, "Неправильный номер первой строки");
      if (lastRow < firstRow)
        throw new ArgumentOutOfRangeException("lastRow", lastRow, "Неправильный номер последней строки");
      if (firstColumn < 1)
        throw new ArgumentOutOfRangeException("firstColumn", firstColumn, "Неправильный номер первого столбца");
      if (lastColumn < firstColumn)
        throw new ArgumentOutOfRangeException("lastColumn", lastColumn, "Неправильный номер последнего столбца");
#endif

      Cell c1 = Cell(firstRow, firstColumn);
      Cell c2 = Cell(lastRow, lastColumn);
      c1.Merge(c2);
    }

    #endregion

    #region Содержимое как целое

    public Range Range
    {
      get
      {
        if (_Range.Base.IsEmpty)
          _Range = new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=0]"), Base.Helper));
        return _Range;
      }
    }
    private Range _Range;

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=104]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion

    #region Отступы

    #region Значения в пунктах

    public float LeftPadding
    {
      get { return GetLeftPadding(); }
      set { SetLeftPadding(value); }
    }

    public float GetLeftPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=115]"));
    }

    public void SetLeftPadding(float Value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=115]", Value);
    }

    public float TopPadding
    {
      get { return GetTopPadding(); }
      set { SetTopPadding(value); }
    }

    public float GetTopPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=113]"));
    }

    public void SetTopPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=113]", value);
    }

    public float RightPadding
    {
      get { return GetRightPadding(); }
      set { SetRightPadding(value); }
    }

    public float GetRightPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=116]"));
    }

    public void SetRightPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=116]", value);
    }

    public float BottomPadding
    {
      get { return GetBottomPadding(); }
      set { SetBottomPadding(value); }
    }

    public float GetBottomPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=114]"));
    }

    public void SetBottomPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=114]", value);
    }

    #endregion

    #region Значения в единицах 0.1 мм

    public void SetPaddingLM(int left, int top, int right, int bottom)
    {
      SetLeftPadding((float)left / 254f * 72f);
      SetTopPadding((float)top / 254f * 72f);
      SetRightPadding((float)right / 254f * 72f);
      SetBottomPadding((float)bottom / 254f * 72f);
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Коллекция таблиц в документе
  /// </summary>
  public struct Tables
  {
    #region Конструктор

    public Tables(ObjBase theBase)
    {
      //global::Word.Tables
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public int Count
    {
      get
      {
        return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=2]"));
      }
    }

    public Table this[int index]
    {
      get
      {
        return new Table(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Создание новой таблицы
    /// </summary>
    /// <param name="range">Место для создания таблицы, например, Document.EndDoc()</param>
    /// <param name="numRows">Число строк</param>
    /// <param name="numColumns">Число столбцов</param>
    /// <returns></returns>
    public Table Add(Range range, int numRows, int numColumns)
    {
      return new Table(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=200]", 
        range.Base.Obj, numRows, numColumns), Base.Helper));
    }

    #endregion
  }

  #endregion

  #region Строки таблицы

  /// <summary>
  /// Одна строка таблицы
  /// </summary>
  public struct Row
  {
    #region Конструктор

    public Row(ObjBase theBase)
    {
      _Base = theBase;
      _Cells = new Cells();
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Ячейки

    public Cells Cells
    {
      get
      {
        if (_Cells.Base.IsEmpty)
          _Cells = new Cells(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=100]"), Base.Helper));
        return _Cells;
      }
    }
    private Cells _Cells;

    #endregion

    #region Размеры

    /// <summary>
    /// Высота строки в пунктах.
    /// Для получения корректного значения необходима установка свойства HeightRule
    /// </summary>
    public float Height
    {
      get { return GetHeight(); }
      set { SetHeight(value); }
    }

    public float GetHeight()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=7]"));
    }

    public void SetHeight(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=7]", value);
    }

    /// <summary>
    /// Правила автоматического изменения высоты строки (авто, точно или минимум)
    /// </summary>
    public WdRowHeightRule HeightRule
    {
      get { return GetHeightRule(); }
      set { SetHeightRule(value); }
    }

    public WdRowHeightRule GetHeightRule()
    {
      return (WdRowHeightRule)(Base.Helper.GetProp(Base.Obj, "[DispID=8]"));
    }

    public void SetHeightRule(WdRowHeightRule value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=8]", value);
    }

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=103]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion

    #endregion

    #region Методы

    public void Delete()
    {
      Base.Helper.Call(Base.Obj, "[DispID=200]");
    }

    #endregion
  }

  /// <summary>
  /// Коллекция строк таблицы
  /// </summary>
  public struct Rows
  {
    #region Конструктор

    public Rows(ObjBase theBase)
    {
      _Base = theBase;
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Доступ к отдельным строкам

    public int Count
    {
      get
      {
        return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=2]"));
      }
    }

    /// <summary>
    /// Доступ к строке по индексу
    /// Строки таблицы нумеруются с единицы
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Row this[int index]
    {
      get
      {
        // Item - это метод, а не свойство
        return new Row(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    public Row First
    {
      get
      {
        return new Row(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=10]"), Base.Helper));
      }
    }

    public Row Last
    {
      get
      {
        return new Row(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=11]"), Base.Helper));
      }
    }

    #endregion

    #region Групповая установка размеров

    /// <summary>
    /// Высота строки в пунктах.
    /// Для получения корректного значения необходима установка свойства HeightRule
    /// </summary>
    public float Height
    {
      get 
      {
        float Value;
        GetHeight(out Value);
        return Value;
      }
      set { SetHeight(value); }
    }

    public bool GetHeight(out float value)
    {
      value = (float)(Base.Helper.GetProp(Base.Obj, "[DispID=7]"));
      if (value==WordHelper.wdUndefined)
      {
        value=0f;
        return false;
      }
      return true;
    }

    public void SetHeight(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=7]", value);
    }

    /// <summary>
    /// Правила автоматического изменения высоты строки (авто, точно или минимум)
    /// </summary>
    public WdRowHeightRule HeightRule
    {
      get 
      {
        WdRowHeightRule Value;
        GetHeightRule(out Value);
        return Value;
      }
      set { SetHeightRule(value); }
    }

    public bool GetHeightRule(out WdRowHeightRule value)
    {
      value = (WdRowHeightRule)(Base.Helper.GetProp(Base.Obj, "[DispID=8]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdRowHeightRule.wdRowHeightAuto;
        return false;
      }
      return true;
    }

    public void SetHeightRule(WdRowHeightRule value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=8]", value);
    }

    /// <summary>
    /// Групповая установка высоты строк в единицах 0.1 мм
    /// Устанавливается точная высота строк без автоматического подбора
    /// </summary>
    /// <param name="heights"></param>
    public void SetHeightLM(int[] heights)
    {
      SetHeightRule(WdRowHeightRule.wdRowHeightExactly);
      for (int i = 0; i < heights.Length; i++)
        this[i+1].SetHeight((float)heights[i] / 254f * 72f);
    }

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=102]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion

    #region Другие методы

    public Row Add()
    {
      return new Row(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=100]"), Base.Helper));
    }

    public Row Add(Row beforeRow)
    {
      return new Row(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=100]", beforeRow), Base.Helper));
    }

    #endregion
  }

  #endregion

  #region Столбцы таблицы

  /// <summary>
  /// Один столбец таблицы
  /// </summary>
  public struct Column
  {
    #region Конструктор

    public Column(ObjBase theBase)
    {
      _Base = theBase;
      _Cells = new Cells();
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Ячейки

    public Cells Cells
    {
      get
      {
        if (_Cells.Base.IsEmpty)
          _Cells = new Cells(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=100]"), Base.Helper));
        return _Cells;
      }
    }
    private Cells _Cells;

    #endregion

    #region Размеры

    /// <summary>
    /// Ширина столбца в пунктах
    /// </summary>
    public float Width
    {
      get { return GetWidth(); }
      set { SetWidth(value); }
    }

    public float GetWidth()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=3]"));
    }

    public void SetWidth(float value)
    {
//      if (ValueEx < 20f)
//        ValueEx = 20f;
      Base.Helper.SetProp(Base.Obj, "[DispID=3]", value);
    }

    /// <summary>
    /// Ширина столбца в единицах 0.1 мм
    /// </summary>
    public float WidthLM
    {
      get { return Width / 72f * 254f; }
      set { Width = value * 72f / 254f; }
    }

    public float GetWidthLM()
    {
      return GetWidth() / 72f * 254f; 
    }

    public void SetWidthLM(float value)
    {
      SetWidth(value * 72f / 254f);
    }

    public WdPreferredWidthType PreferredWidthType
    {
      get { return GetPreferredWidthType(); }
      set { SetPreferredWidthType(value); }
    }

    private WdPreferredWidthType GetPreferredWidthType()
    {
      return (WdPreferredWidthType)(Base.Helper.GetProp(Base.Obj, "[DispID=107]"));
    }

    private void SetPreferredWidthType(WdPreferredWidthType value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=107]", (int)value);
    }

    /// <summary>
    /// Предпочитаемая ширина каждого столбца в пунктах или процентах, в зависимости
    /// от свойства PreferredWidthType
    /// </summary>
    public float PreferredWidth
    {
      get { return GetPreferredWidth(); }
      set { SetPreferredWidth(value); }
    }

    public float GetPreferredWidth()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=106]"));
    }

    public void SetPreferredWidth(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=106]", value);
    }

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=102]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion
  }

  /// <summary>
  /// Коллекция столбцов таблицы
  /// </summary>
  public struct Columns
  {
    #region Конструктор

    public Columns(ObjBase theBase)
    {
      _Base = theBase;
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Доступ к отдельным столбцам

    public int Count
    {
      get
      {
        return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=2]"));
      }
    }

    public Column this[int index]
    {
      get
      {
        return new Column(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    public Column First
    {
      get
      {
        return new Column(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=100]"), Base.Helper));
      }
    }

    public Column Last
    {
      get
      {
        return new Column(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=101]"), Base.Helper));
      }
    }

    #endregion

    #region Групповая установка размеров

    /// <summary>
    /// Ширина каждого столбца в пунктах
    /// </summary>
    public float Width
    {
      get 
      {
        float Value;
        GetWidth(out Value);
        return Value;
      }
      set { SetWidth(value); }
    }

    public bool GetWidth(out float value)
    {
      value = (float)(Base.Helper.GetProp(Base.Obj, "[DispID=3]"));
      if (value == (float)(WordHelper.wdUndefined))
      {
        value = 0f;
        return false;
      }
      return true;
    }       

    public void SetWidth(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=3]", value);
    }

    public WdPreferredWidthType PreferredWidthType
    {
      get { return GetPreferredWidthType(); }
      set { SetPreferredWidthType(value); }
    }

    private WdPreferredWidthType GetPreferredWidthType()
    {
      return (WdPreferredWidthType)(Base.Helper.GetProp(Base.Obj, "[DispID=106]"));
    }

    private void SetPreferredWidthType(WdPreferredWidthType value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=106]", (int)value);
    }

    /// <summary>
    /// Предпочитаемая ширина каждого столбца в пунктах или процентах, в зависимости
    /// от свойства PreferredWidthType
    /// </summary>
    public float PreferredWidth
    {
      get
      {
        float Value;
        GetPreferredWidth(out Value);
        return Value;
      }
      set { SetPreferredWidth(value); }
    }

    public bool GetPreferredWidth(out float value)
    {
      value = (float)(Base.Helper.GetProp(Base.Obj, "[DispID=105]"));
      if (value == (float)(WordHelper.wdUndefined))
      {
        value = 0f;
        return false;
      }
      return true;
    }

    public void SetPreferredWidth(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=105]", value);
    }

    /// <summary>
    /// Групповая установка ширины столбцов в единицах 0.1 мм
    /// </summary>
    /// <param name="widthes"></param>
    public void SetWidthLM(int[] widthes)
    {
      SetPreferredWidthType(WdPreferredWidthType.wdPreferredWidthPoints);
      for (int i = 0; i < widthes.Length; i++)
        this[i+1].SetPreferredWidth((float)(widthes[i]) / 254f * 72f);
    }

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=103]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion
  }

  #endregion

  #region Ячейки таблицы

  /// <summary>
  /// Одна ячейка таблицы
  /// </summary>
  public struct Cell
  {
    #region Конструктор

    public Cell(ObjBase theBase)
    {
      _Base = theBase;
      _Range = new Range();
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Содержимое ячейки

    public Range Range
    {
      get
      {
        if (_Range.Base.IsEmpty)
          _Range = new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=0]"), Base.Helper));
        return _Range;
      }
    }
    private Range _Range;

    #endregion

    #region Выравнивание

    /// <summary>
    /// Вертикальное выравнивание текста в ячейке
    /// </summary>
    public WdCellVerticalAlignment VerticalAlignment
    {
      get
      {
        WdCellVerticalAlignment Value;
        GetVerticalAlignment(out Value);
        return Value;
      }
      set
      {
        SetVerticalAlignment(value);
      }
    }

    public bool GetVerticalAlignment(out WdCellVerticalAlignment value)
    {
      value = (WdCellVerticalAlignment)(Base.Helper.GetProp(Base.Obj, "[DispID=1104]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdCellVerticalAlignment.wdCellAlignVerticalTop;
        return false;
      }
      return true;
    }

    public void SetVerticalAlignment(WdCellVerticalAlignment value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1104]", (int)value);
    }


    /// <summary>
    /// Перенос по словам
    /// </summary>
    public bool WordWrap
    {
      get
      {
        bool Value;
        GetWordWrap(out Value);
        return Value;
      }
      set
      {
        SetWordWrap(value);
      }
    }

    public bool GetWordWrap(out bool value)
    {
      int x = (int)(Base.Helper.GetProp(Base.Obj, "[DispID=108]"));
      return WordHelper.GetBoolValue(x, out value);
    }

    public void SetWordWrap(bool value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=108]", value);
    }

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=105]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion

    #region Отступы

    #region Значения в пунктах

    public float LeftPadding
    {
      get { return GetLeftPadding(); }
      set { SetLeftPadding(value); }
    }

    public float GetLeftPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=113]"));
    }

    public void SetLeftPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=113]", value);
    }

    public float TopPadding
    {
      get { return GetTopPadding(); }
      set { SetTopPadding(value); }
    }

    public float GetTopPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=111]"));
    }

    public void SetTopPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=111]", value);
    }

    public float RightPadding
    {
      get { return GetRightPadding(); }
      set { SetRightPadding(value); }
    }

    public float GetRightPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=114]"));
    }

    public void SetRightPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=114]", value);
    }

    public float BottomPadding
    {
      get { return GetBottomPadding(); }
      set { SetBottomPadding(value); }
    }

    public float GetBottomPadding()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=112]"));
    }

    public void SetBottomPadding(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=112]", value);
    }

    #endregion

    #region Значения в единицах 0.1 мм

    public void SetPaddingLM(int left, int top, int right, int bottom)
    {
      SetLeftPadding((float)left / 254f * 72f);
      SetTopPadding((float)top / 254f * 72f);
      SetRightPadding((float)right / 254f * 72f);
      SetBottomPadding((float)bottom / 254f * 72f);
    }

    #endregion

    #endregion

    #region Объединение ячеек

    public void Merge(Cell mergeTo)
    {
      Base.Helper.Call(Base.Obj, "[DispID=204]", mergeTo.Base.Obj);
    }

    #endregion
  }

  /// <summary>
  /// Массив ячеек таблицы
  /// </summary>
  public struct Cells
  {
    #region Конструктор

    public Cells(ObjBase theBase)
    {
      _Base = theBase;
      _Borders = new Borders();
      _Shading = new Shading();
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Доступ к отдельным ячейкам

    /// <summary>
    /// Общее число ячеек в коллекции
    /// </summary>
    public int Count
    {
      get
      {
        return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=2]"));
      }
    }

    /// <summary>
    /// Доступ к ячейке по индексу
    /// Ячейки нумеруются с единицы
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Cell this[int index]
    {
      get
      {
        return new Cell(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    #endregion

    #region Выравнивание

    /// <summary>
    /// Вертикальное выравнивание текста в ячейке
    /// </summary>
    public WdCellVerticalAlignment VerticalAlignment
    {
      get
      {
        WdCellVerticalAlignment Value;
        GetVerticalAlignment(out Value);
        return Value;
      }
      set
      {
        SetVerticalAlignment(value);
      }
    }

    public bool GetVerticalAlignment(out WdCellVerticalAlignment value)
    {
      value = (WdCellVerticalAlignment)(Base.Helper.GetProp(Base.Obj, "[DispID=1104]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdCellVerticalAlignment.wdCellAlignVerticalTop;
        return false;
      }
      return true;
    }

    public void SetVerticalAlignment(WdCellVerticalAlignment value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=1104]", (int)value);
    }

    #endregion

    #region Границы и заливка

    public Borders Borders
    {
      get
      {
        if (_Borders.Base.IsEmpty)
          _Borders = new Borders(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1100]"), Base.Helper));
        return _Borders;
      }
    }
    private Borders _Borders;

    public Shading Shading
    {
      get
      {
        if (_Shading.Base.IsEmpty)
          _Shading = new Shading(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=101]"), Base.Helper));
        return _Shading;
      }
    }
    private Shading _Shading;

    #endregion

    #region Объединение ячеек

    public void Merge()
    {
      Base.Helper.Call(Base.Obj, "[DispID=204]");
    }

    #endregion
  }

  #endregion

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

    #region Стандартные размеры

    /// <summary>
    /// Стандартный размер бумаги.
    /// После установки свойств PaperWidth или PaperHeight размер бумаги становится
    /// WdPaperCustom
    /// </summary>
    public WdPaperSize PaperSize
    {
      get { return GetPaperSize(); }
      set { SetPaperSize(value); }
    }

    public WdPaperSize GetPaperSize()
    {
      return (WdPaperSize)(Base.Helper.GetProp(Base.Obj, "[DispID=120]"));
    }

    public void SetPaperSize(WdPaperSize value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=120]", value);
    }

    #endregion

    #region Размеры в пунктах

    /// <summary>
    /// Ширина листа бумаги в пунктах. Установка значения свойства устанавливает
    /// пользовательский размер бумаги
    /// </summary>
    public float PageWidth
    {
      get { return GetPageWidth(); }
      set { SetPageWidth(value); }
    }

    private float GetPageWidth()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=105]"));
    }

    private void SetPageWidth(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=105]", value);
    }

    /// <summary>
    /// Высота листа бумаги в пунктах. Установка значения свойства устанавливает
    /// пользовательский размер бумаги
    /// </summary>
    public float PageHeight
    {
      get { return GetPageHeight(); }
      set { SetPageHeight(value); }
    }

    private float GetPageHeight()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=106]"));
    }

    private void SetPageHeight(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=106]", value);
    }

    #endregion

    #region Размеры в единицах 0.1 мм

    /// <summary>
    /// Установка размеров бумаги в единицах 0.1 мм
    /// Будут установлен стандартный, а не пользовательский, размер для форматов
    /// A4, A3
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetPaperSizeLM(int width, int height)
    {
      WdPaperSize sz = GetPaperSizeLM(width, height, Orientation);
      if (sz == WdPaperSize.wdPaperCustom)
      {
        SetPageWidth((float)width / 254f * 72f);
        SetPageHeight((float)height / 254f * 72f);
      }
      else
        SetPaperSize(sz);
    }

    public static WdPaperSize GetPaperSizeLM(int width, int height, WdOrientation orientation)
    {
      if (orientation == WdOrientation.wdOrientLandscape)
      {
        int tmp = width;
        width = height;
        height = tmp;
      }
      if (width == 2100 && height == 2970)
        return WdPaperSize.wdPaperA4;
      if (width == 2970 && height == 4200)
        return WdPaperSize.wdPaperA3;
      // Можно добавить другие размеры
      return WdPaperSize.wdPaperCustom;
    }

    #endregion

    #endregion

    #region Ориентация бумаги

    public WdOrientation Orientation
    {
      get { return GetOrientation(); }
      set { SetOrientation(value); }
    }

    public WdOrientation GetOrientation()
    {
      return (WdOrientation)(Base.Helper.GetProp(Base.Obj, "[DispID=107]"));
    }

    public void SetOrientation(WdOrientation value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=107]", value);
    }

    #endregion

    #region Поля

    #region Размеры в пунктах

    public float LeftMargin
    {
      get { return GetLeftMargin(); }
      set { SetLeftMargin(value); }
    }

    public float GetLeftMargin()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=102]"));
    }

    public void SetLeftMargin(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=102]", value);
    }

    public float TopMargin
    {
      get { return GetTopMargin(); }
      set { SetTopMargin(value); }
    }

    public float GetTopMargin()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=100]"));
    }

    public void SetTopMargin(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=100]", value);
    }

    public float RightMargin
    {
      get { return GetRightMargin(); }
      set { SetRightMargin(value); }
    }

    public float GetRightMargin()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=103]"));
    }

    public void SetRightMargin(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=103]", value);
    }

    public float BottomMargin
    {
      get { return GetBottomMargin(); }
      set { SetBottomMargin(value); }
    }

    public float GetBottomMargin()
    {
      return (float)(Base.Helper.GetProp(Base.Obj, "[DispID=101]"));
    }

    public void SetBottomMargin(float value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=101]", value);
    }

    #endregion

    #region Размеры в единицах 0.1 мм

    public void SetMarginsLM(int left, int top, int right, int bottom)
    {
      SetLeftMargin((float)left * 72f / 254f);
      SetTopMargin((float)top * 72f / 254f);
      SetRightMargin((float)right * 72f / 254f);
      SetBottomMargin((float)bottom * 72f / 254f);
    }

    public void GetMarginsLM(out int left, out int top, out int right, out int bottom)
    {
      left = (int)(GetLeftMargin() / 72f * 254f);
      top = (int)(GetTopMargin() / 72f * 254f);
      right = (int)(GetRightMargin() / 72f * 254f);
      bottom = (int)(GetBottomMargin() / 72f * 254f);
    }

    #endregion

    #endregion

    #region Выравнивание

    /// <summary>
    /// Вертикальное выравнивание текста на странице
    /// </summary>
    public WdVerticalAlignment VerticalAlignment
    {
      get
      {
        WdVerticalAlignment Value;
        GetVerticalAlignment(out Value);
        return Value;
      }
      set
      {
        SetVerticalAlignment(value);
      }
    }

    public bool GetVerticalAlignment(out WdVerticalAlignment value)
    {
      value = (WdVerticalAlignment)(Base.Helper.GetProp(Base.Obj, "[DispID=110]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdVerticalAlignment.wdAlignVerticalTop;
        return false;
      }
      return true;
    }

    public void SetVerticalAlignment(WdVerticalAlignment value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=110]", (int)value);
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

    public WdLineStyle LineStyle
    {
      get
      {
        WdLineStyle Value;
        GetLineStyle(out Value);
        return Value;
      }
      set
      {
        SetLineStyle(value);
      }
    }

    public bool GetLineStyle(out WdLineStyle value)
    {
      value = (WdLineStyle)Base.Helper.GetProp(Base.Obj, "[DispID=3]");
      if ((int)value==WordHelper.wdUndefined)
      {
        value = WdLineStyle.wdLineStyleNone;
        return false;
      }
      return true;
    }

    public void SetLineStyle(WdLineStyle value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=3]", (int)value);
    }

    #endregion

    #region Толщина

    /// <summary>
    /// Толщина линии в единицах 1/4 пункта
    /// </summary>
    public WdLineWidth LineWidth
    {
      get
      {
        WdLineWidth Value;
        GetLineWidth(out Value);
        return Value;
      }
      set
      {
        SetLineWidth(value);
      }
    }

    public bool GetLineWidth(out WdLineWidth value)
    {
      value = (WdLineWidth)(Base.Helper.GetProp(Base.Obj, "[DispID=4]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdLineWidth.wdLineWidth025pt;
        return false;
      }
      return true;
    }

    public void SetLineWidth(WdLineWidth value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=4]", (int)value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Word
    /// </summary>
    public Int32 ColorInt
    {
      get
      {
        Int32 Value;
        GetColorInt(out Value);
        return Value;
      }
      set
      {
        SetColorInt(value);
      }
    }

    public bool GetColorInt(out Int32 value)
    {
      Int32 x = (Int32)(Base.Helper.GetProp(Base.Obj, "[DispID=7]"));
      return WordHelper.GetColorIntValue(x, out value);
    }

    public void SetColorInt(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=7]", value);
    }

    public Color Color
    {
      get { return WordHelper.RgbToColor(ColorInt); }
      set { ColorInt = WordHelper.ColorToRgb(value); }
    }

    public bool GetColor(out Color value)
    {
      Int32 IntValue;
      bool Res = GetColorInt(out IntValue);
      value = WordHelper.RgbToColor(IntValue);
      return Res;
    }

    public void SetColor(Color value)
    {
      SetColorInt(WordHelper.ColorToRgb(value));
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

    public Border this[WdBorderType Index]
    {
      get
      {
        return new Border(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", Index), Base.Helper));
      }
    }

    #endregion

    #region Свойства внутренних границ

    #region Стиль линии

    public WdLineStyle InsideLineStyle
    {
      get
      {
        WdLineStyle Value;
        GetInsideLineStyle(out Value);
        return Value;
      }
      set
      {
        SetInsideLineStyle(value);
      }
    }

    public bool GetInsideLineStyle(out WdLineStyle value)
    {
      value = (WdLineStyle)Base.Helper.GetProp(Base.Obj, "[DispID=6]");
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdLineStyle.wdLineStyleNone;
        return false;
      }
      return true;
    }
    public void SetInsideLineStyle(WdLineStyle value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=6]", (int)value);
    }

    #endregion

    #region Толщина

    /// <summary>
    /// Толщина линии в единицах 1/4 пункта
    /// </summary>
    public WdLineWidth InsideLineWidth
    {
      get
      {
        WdLineWidth Value;
        GetInsideLineWidth(out Value);
        return Value;
      }
      set
      {
        SetInsideLineWidth(value);
      }
    }

    public bool GetInsideLineWidth(out WdLineWidth value)
    {
      value = (WdLineWidth)(Base.Helper.GetProp(Base.Obj, "[DispID=8]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdLineWidth.wdLineWidth025pt;
        return false;
      }
      return true;
    }
    public void SetInsideLineWidth(WdLineWidth value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=8]", (int)value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Word
    /// </summary>
    public Int32 InsideColorInt
    {
      get
      {
        Int32 Value;
        GetInsideColorInt(out Value);
        return Value;
      }
      set
      {
        SetInsideColorInt(value);
      }
    }

    public bool GetInsideColorInt(out Int32 value)
    {
      Int32 x = (Int32)(Base.Helper.GetProp(Base.Obj, "[DispID=32]"));
      return WordHelper.GetColorIntValue(x, out value);
    }

    public void SetInsideColorInt(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=32]", value);
    }

    public Color InsideColor
    {
      get { return WordHelper.RgbToColor(InsideColorInt); }
      set { InsideColorInt = WordHelper.ColorToRgb(value); }
    }

    public bool GetInsideColor(out Color value)
    {
      Int32 IntValue;
      bool Res = GetInsideColorInt(out IntValue);
      value = WordHelper.RgbToColor(IntValue);
      return Res;
    }

    public void SetInsideColor(Color Value)
    {
      SetInsideColorInt(WordHelper.ColorToRgb(Value));
    }

    #endregion

    #endregion

    #region Свойства внешних границ

    #region Стиль линии

    public WdLineStyle OutsideLineStyle
    {
      get
      {
        WdLineStyle Value;
        GetOutsideLineStyle(out Value);
        return Value;
      }
      set
      {
        SetOutsideLineStyle(value);
      }
    }

    public bool GetOutsideLineStyle(out WdLineStyle value)
    {
      value = (WdLineStyle)Base.Helper.GetProp(Base.Obj, "[DispID=7]");
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdLineStyle.wdLineStyleNone;
        return false;
      }
      return true;
    }

    public void SetOutsideLineStyle(WdLineStyle value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=7]", (int)value);
    }

    #endregion

    #region Толщина

    /// <summary>
    /// Толщина линии в единицах 1/4 пункта
    /// </summary>
    public WdLineWidth OutsideLineWidth
    {
      get
      {
        WdLineWidth Value;
        GetOutsideLineWidth(out Value);
        return Value;
      }
      set
      {
        SetOutsideLineWidth(value);
      }
    }

    public bool GetOutsideLineWidth(out WdLineWidth value)
    {
      value = (WdLineWidth)(Base.Helper.GetProp(Base.Obj, "[DispID=9]"));
      if ((int)value == WordHelper.wdUndefined)
      {
        value = WdLineWidth.wdLineWidth025pt;
        return false;
      }
      return true;
    }

    public void SetOutsideLineWidth(WdLineWidth value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=9]", (int)value);
    }

    #endregion

    #region Цвет

    /// <summary>
    /// Цвет в представлении Word
    /// </summary>
    public Int32 OutsideColorInt
    {
      get
      {
        Int32 Value;
        GetOutsideColorInt(out Value);
        return Value;
      }
      set
      {
        SetOutsideColorInt(value);
      }
    }

    public bool GetOutsideColorInt(out Int32 value)
    {
      Int32 x = (Int32)(Base.Helper.GetProp(Base.Obj, "[DispID=33]"));
      return WordHelper.GetColorIntValue(x, out value);
    }

    public void SetOutsideColorInt(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=33]", value);
    }

    public Color OutsideColor
    {
      get { return WordHelper.RgbToColor(OutsideColorInt); }
      set { OutsideColorInt = WordHelper.ColorToRgb(value); }
    }

    public bool GetOutsideColor(out Color value)
    {
      Int32 IntValue;
      bool Res = GetOutsideColorInt(out IntValue);
      value = WordHelper.RgbToColor(IntValue);
      return Res;
    }

    public void SetOutsideColor(Color value)
    {
      SetOutsideColorInt(WordHelper.ColorToRgb(value));
    }

    #endregion

    #endregion
  }

  #endregion

  #region Заливка

  public struct Shading
  {
    #region Конструктор

    public Shading(ObjBase theBase)
    {
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    #region Цвет фона

    /// <summary>
    /// Цвет в представлении Excel
    /// </summary>
    public Int32 BackgroundPatternColorInt
    {
      get
      {
        Int32 Value;
        GetBackgroundPatternColorInt(out Value);
        return Value;
      }
      set
      {
        SetBackgroundPatternColorInt(value);
      }
    }

    public bool GetBackgroundPatternColorInt(out Int32 value)
    {
      Int32 x= (Int32)(Base.Helper.GetProp(Base.Obj, "[DispID=5]"));
      return WordHelper.GetColorIntValue(x, out value);
    }

    public void SetBackgroundPatternColorInt(Int32 value)
    {
      Base.Helper.SetProp(Base.Obj, "[DispID=5]", value);
    }

    public Color BackgroundPatternColor
    {
      get { return WordHelper.RgbToColor(BackgroundPatternColorInt); }
      set { BackgroundPatternColorInt = WordHelper.ColorToRgb(value); }
    }

    public bool GetBackgroundPatternColor(out Color value)
    {
      Int32 IntValue;
      bool Res = GetBackgroundPatternColorInt(out IntValue);
      value = WordHelper.RgbToColor(IntValue);
      return Res;
    }

    public void SetBackgroundPatternColor(Color value)
    {
      SetBackgroundPatternColorInt(WordHelper.ColorToRgb(value));
    }

    #endregion

    #endregion
  }

  #endregion

  #region Объект поиска

  public struct Find
  {
    #region Конструктор

    public Find(ObjBase theBase)
    {
      //global::Word.Find

      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Исходный текст

    /*
    public string Text 
    {
      get { return GetText(); }
      set { SetText(value); }
    }

    private string GetText()
    {
      return (string)(AnyDocType.Helper.GetProp(AnyDocType.Obj, "[DispID=22]"));
    }

    private void SetText(string ValueEx)
    {
      AnyDocType.Helper.SetProp(AnyDocType.Obj, "[DispID=22]", ValueEx);
    }

    #endregion

    #region Текст для замены

    public string Text
    {
      get { return GetText(); }
      set { SetText(value); }
    }

    private string GetText()
    {
      return (string)(AnyDocType.Helper.GetProp(AnyDocType.Obj, "[DispID=22]"));
    }

    private void SetText(string ValueEx)
    {
      AnyDocType.Helper.SetProp(AnyDocType.Obj, "[DispID=22]", ValueEx);
    }

    */
    #endregion

    #region Методы

    public void Replace(string oldText, string newText)
    {
      object[] Args = new object[11];
      DataTools.FillArray<object>(Args, Missing.Value);
      Args[0] = oldText;
      Args[9] = newText;
      Args[10] = WdReplace.wdReplaceAll;
      Base.Helper.CallWithArgs(Base.Obj, "[DispID=444]", Args);
    }

    #endregion
  }

  #endregion

  #region Закладка

  /// <summary>
  /// Коллекция закладок в документе
  /// </summary>
  public struct Bookmarks
  {
    #region Конструктор

    public Bookmarks(ObjBase theBase)
    {
      //global::Word.Tables
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public int Count
    {
      get
      {
        return (int)(Base.Helper.GetProp(Base.Obj, "[DispID=2]"));
      }
    }

    public Bookmark this[int index]
    {
      get
      {
        return new Bookmark(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", index), Base.Helper));
      }
    }

    public Bookmark this[string name]
    {
      get
      {
        return new Bookmark(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=0]", name), Base.Helper));
      }
    }

    public bool Exists(string name)
    {
      return (bool)(Base.Helper.Call(Base.Obj, "[DispID=6]", name));
    }

    #endregion
  }

  /// <summary>
  /// Закладка в документе Microsoft Word
  /// </summary>
  public struct Bookmark
  {
    #region Конструктор

    public Bookmark(ObjBase theBase)
    {
      //global::Word.Tables
      _Base = theBase;
    }

    public ObjBase Base { get { return _Base; } }
    private ObjBase _Base;

    #endregion

    #region Свойства

    public string Name
    {
      get
      {
        return (string)(Base.Helper.GetProp(Base.Obj, "[DispID=0]"));
      }
    }

    public Range Range
    {
      get
      {
        return new Range(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=1]"), Base.Helper));
      }
    }

    public bool Empty(string name)
    {
      return (bool)(Base.Helper.GetProp(Base.Obj, "[DispID=2]"));
    }

    #endregion
  }

  #endregion
}
