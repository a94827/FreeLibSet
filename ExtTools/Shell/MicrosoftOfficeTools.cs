// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;
using FreeLibSet.OLE;
using FreeLibSet.IO;
using System.Threading;
using System.Runtime.InteropServices;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Shell
{
  /// <summary>
  /// Поддержка для Microsoft Office
  /// </summary>
  public static class MicrosoftOfficeTools
  {
    #region Номера версий офиса

    /// <summary>
    /// Старший номер версии для Microsoft Office 95
    /// </summary>
    public const int MicrosoftOffice_95 = 7;

    /// <summary>
    /// Старший номер версии для Microsoft Office 97
    /// </summary>
    public const int MicrosoftOffice_97 = 8;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2000
    /// </summary>
    public const int MicrosoftOffice_2000 = 9;

    /// <summary>
    /// Старший номер версии для Microsoft Office XP
    /// </summary>
    public const int MicrosoftOffice_XP = 10;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2003
    /// </summary>
    public const int MicrosoftOffice_2003 = 11;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2007
    /// </summary>
    public const int MicrosoftOffice_2007 = 12;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2010
    /// </summary>
    public const int MicrosoftOffice_2010 = 14;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2013
    /// </summary>
    public const int MicrosoftOffice_2013 = 15;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2016
    /// </summary>
    public const int MicrosoftOffice_2016 = 16;

    /// <summary>
    /// Старший номер версии для Microsoft Office 2019.
    /// Совпадает с Microsoft Office 2016
    /// </summary>
    public const int MicrosoftOffice_2019 = 16;

    private static string GetOfficeTradeVersion(Version Ver)
    {
      switch (Ver.Major)
      {
        case MicrosoftOffice_95: return "95";
        case MicrosoftOffice_97: return "97";
        case MicrosoftOffice_2000: return "2000";
        case MicrosoftOffice_XP: return "XP";
        case MicrosoftOffice_2003: return "2003";
        case MicrosoftOffice_2007: return "2007";
        case MicrosoftOffice_2010: return "2010";
        case MicrosoftOffice_2013: return "2013";
        case MicrosoftOffice_2016: return "2016/2019";

        default: return String.Empty;
      }
    }


    #endregion

    #region Константы даты

    /*
     * Возможности различных версий Microsoft Office:
     * - Начиная с Word/Excel-97 открываются HTML-файлы, но OLE глючит
     * - Начиная с Word/Excel-2000 работает передача через OLE (объекты WordHelper и ExcelHelper)
     * - Начиная с Excel-XP читаются XML-файлы "Книга Microsoft Excel XP-2003"
     * - Начиная с Word-2003 читаются XML-файлы "Документ Microsoft Word 2003"
     * - Начиная с Word/Excel-2007 читаются сжатые файлы docx/xlsx
     */

    /// <summary>
    /// Минимальная дата, которую можно использовать в Excel 
    /// </summary>
    public static readonly DateTime MinExcelDate = new DateTime(1900, 1, 1);

    /// <summary>
    /// Максимальная дата, которую можно использовать в Excel (без компонента времени)
    /// </summary>
    public static readonly DateTime MaxExcelDate = new DateTime(9999, 12, 31);

    #endregion

    #region Установленная версия

    /// <summary>
    /// Путь к программе winword.exe
    /// </summary>
    public static AbsPath WordPath
    {
      get
      {
        return OLEHelper.GetLocalServer32Path("Word.Application");
      }
    }

    /// <summary>
    /// Получить установленную версию Microsoft Word
    /// Если Word не установлен, то возвращается null
    /// </summary>
    public static Version WordVersion
    {
      get
      {
        using (new FileRedirectionSupressor())
        {
          return OLEHelper.GetLocalServer32Version("Word.Application");
        }
      }
    }

    /// <summary>
    /// Возвращает "Microsoft Word"
    /// </summary>
    public static string WordDisplayName { get { return "Microsoft Word"; } }

    /// <summary>
    /// Возвращает "Microsoft Excel"
    /// </summary>
    public static string ExcelDisplayName { get { return "Microsoft Excel"; } }

    /// <summary>
    /// Путь к программе excel.exe
    /// </summary>
    public static AbsPath ExcelPath
    {
      get
      {
        return OLEHelper.GetLocalServer32Path("Excel.Application");
      }
    }

    /// <summary>
    /// Получить установленную версию Microsoft Excel
    /// Если Excel не установлен, то возвращается null
    /// </summary>
    public static Version ExcelVersion
    {
      get
      {
        using (new FileRedirectionSupressor())
        {
          return OLEHelper.GetLocalServer32Version("Excel.Application");
        }
      }
    }


    /// <summary>
    /// Возвращает версию Word в читаемом виде, например, "Microsoft Word 2003 (11.0.8411.0)".
    /// Если Word не установлен, возвращается пустая строка
    /// </summary>
    public static string WordVersionString
    {
      get
      {
        if (WordVersion == null)
          return String.Empty;
        else
        {
          string s = GetOfficeTradeVersion(WordVersion);
          if (String.IsNullOrEmpty(s)) // не смогли определить
            s = WordDisplayName + " " + WordVersion.ToString();
          else
            s = WordDisplayName + " " + s + " (" + WordVersion.ToString() + ")";
          bool? Is64bit = FileTools.Is64bitPE(WordPath);
          if (Is64bit.HasValue)
            s += Is64bit.Value ? " (64-bit)" : " (32-bit)";
          return s;
        }
      }
    }

    /// <summary>
    /// Возвращает версию Excel в читаемом виде, например, "Microsoft Excel 2003 (11.0.8404.0)".
    /// Если Excel не установлен, возвращается пустая строка
    /// </summary>
    public static string ExcelVersionString
    {
      get
      {
        if (ExcelVersion == null)
          return String.Empty;
        else
        {
          string s = GetOfficeTradeVersion(ExcelVersion);
          if (String.IsNullOrEmpty(s)) // не смогли определить
            s = ExcelDisplayName + " " + ExcelVersion.ToString();
          else
            s = ExcelDisplayName + " " + s + " (" + ExcelVersion.ToString() + ")";

          bool? Is64bit = FileTools.Is64bitPE(ExcelPath);
          if (Is64bit.HasValue)
            s += Is64bit.Value ? " (64-bit)" : " (32-bit)";
          return s;
        }
      }
    }

    #endregion

    #region Номера столбцов Microsoft Excel

    /// <summary>
    /// Преобразование номера столбца (1 - 65535) в обозначение столбца Microsoft Excel ("A", "B" .., "Z", "AA", "AB..", "ZZ", "AAA", ...).
    /// </summary>
    /// <param name="columnNumber">Номер столбца, начиная с 1</param>
    /// <returns>Буквенное обозначение столбца</returns>
    /// <remarks>    
    /// Для совместного преобразования номера строки и столбца используйте структуру ExcelRCNumber
    /// </remarks>
    public static string GetExcelColumnName(int columnNumber)
    {
      if (columnNumber < 1)
        throw new ArgumentOutOfRangeException("columnNumber", columnNumber, "Номер столбца должен быть больше 0");

      string s = String.Empty;
      do
      {
        int v1 = (columnNumber - 1) % 26;
        columnNumber = (columnNumber - 1) / 26;
        char c = (char)('A' + v1);
        s = c + s;
      }
      while (columnNumber > 0);

      return s;
    }

    /// <summary>
    /// Преобразование обозначения столбца Microsoft Excel в номер столбца: "A"=1, "B=2", "Z"=26, ..., "AA"=27, ...
    /// Если строка пустая или null, вызывается ArgumentNullException. Если строка содержит символы, отличные от "A"-"Z", генерируется
    /// FormatException. Если число больше int.MaxValue, генерируется OverflowException
    /// </summary>
    /// <param name="columnName">Буквенное обозначение столбца</param>
    /// <returns>Номер столбца, начиная с 1</returns>
    /// <remarks>    
    /// Для совместного преобразования номера строки и столбца используйте структуру ExcelRCNumber
    /// </remarks>
    public static int GetExcelColumnNumber(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      int ColumnNumber = 0;

      for (int i = 0; i < columnName.Length; i++)
      {
        char ch = columnName[i];
        if (ch < 'A' || ch > 'Z')
          throw new FormatException();
        int v = (ch - 'A') + 1;
        checked
        {
          ColumnNumber = ColumnNumber * 26 + v;
        }
      }

      return ColumnNumber;
    }

    /// <summary>
    /// Преобразование обозначения столбца Microsoft Excel в номер столбца: "A"=1, "B=2", "Z"=26, ..., "AA"=27, ...
    /// Если строка пустая или не может быть преобразована, возвращается false
    /// </summary>
    /// <param name="columnName">Буквенное обозначение столбца</param>
    /// <param name="columnNumber">Результат преобразования (номер столбца) или 0 в случае ошибки</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    /// <remarks>    
    /// Для совместного преобразования номера строки и столбца используйте структуру ExcelRCNumber
    /// </remarks>
    public static bool TryGetExcelColumnNumber(string columnName, out int columnNumber)
    {
      columnNumber = 0;
      if (String.IsNullOrEmpty(columnName))
        return false;

      for (int i = 0; i < columnName.Length; i++)
      {
        char ch = columnName[i];
        if (ch < 'A' || ch > 'Z')
        {
          columnNumber = 0;
          return false;
        }
        int v = (ch - 'A') + 1;
        try
        {
          checked
          {
            columnNumber = columnNumber * 26 + v;
          }
        }
        catch (OverflowException)
        {
          columnNumber = 0;
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Запуск приложений

    /// <summary>
    /// Инициализация объекта OLEHelper для работы с Word или Excel
    /// </summary>
    /// <param name="helper">Созданный OLEHelper</param>
    /// <param name="testObj">Объект приложения</param>
    /// <param name="testPropName">Имя свойства, например, "[DispID=392]" (Application.Verion) для приложения Excel</param>
    [DebuggerStepThrough]
    public static void InitOleHelper(OLEHelper helper, ObjBase testObj, string testPropName)
    {
      const int TYPE_E_INVDATAREAD = unchecked((int)0x80028018);

      try
      {
        // Используем текущие настройки
        helper.GetProp(testObj.Obj, testPropName);
      }
      catch (Exception e)
      {
        COMException ex = LogoutTools.GetException<COMException>(e);
        if (ex != null)
        {
          if (ex.ErrorCode == TYPE_E_INVDATAREAD)
          {
            helper.GetProp0409(testObj.Obj, testPropName);
            helper.LCID = 0x0409;
            return;
          }
        }
        throw;
      }
    }

    /// <summary>
    /// Возвращает true, если при OLE-взаимодействии нужна английская локаль
    /// </summary>
    /// <returns></returns>
    internal static bool LCID0409Needed()
    {
      return Environment.OSVersion.Platform == PlatformID.Win32NT;
    }

    /*
     * Основная объектная модель находится в ExtForms.dll, т.к. в ней используются ссылки на System.Drawings.Color и перечисления System.Windows.Forms
     * Здесь - урезанная копия
     */

    #region Программная модель Word

    #region Основной объект

    private class WordHelper : OLEHelper
    {
      #region Конструктор и Disposing

      public WordHelper()
      {
        ShowOnEnd = true;
        CreateMainObj("Word.Application");
        _Application = new WordApplication(new ObjBase(MainObj, this));
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

      public WordApplication Application { get { return _Application; } }
      private WordApplication _Application;

      #endregion
    }

    #endregion

    #region Приложение

    /// <summary>
    /// Уровень выдачи диагностических сообщений в Microsoft Word
    /// </summary>
    public enum WdAlertLevel
    {
      /// <summary>
      /// Only message boxes are displayed; errors are trapped and returned to the macro
      /// </summary>
      wdAlertsMessageBox = -2,

      /// <summary>
      /// All message boxes and alerts are displayed; errors are returned to the macro.
      /// </summary>
      wdAlertsAll = -1,

      /// <summary>
      /// No alerts or message boxes are displayed. If a macro encounters a message box, the default value is chosen and the macro continues
      /// </summary>
      wdAlertsNone = 0,
    }


    private struct WordApplication
    {
      #region Конструктор

      public WordApplication(ObjBase objBase)
      {
        _Base = objBase;
        _Documents = new WordDocuments();
      }

      public ObjBase Base { get { return _Base; } }
      private ObjBase _Base;

      #endregion

      #region Свойства

      public WordDocuments Documents
      {
        get
        {
          if (_Documents.Base.IsEmpty)
          {
            _Documents = new WordDocuments(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=6]"), Base.Helper));
          }
          return _Documents;
        }
      }
      private WordDocuments _Documents;


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


      /// <summary>
      /// Надо ли выдавать подтверждения пользователю
      /// </summary>
      public WdAlertLevel DisplayAlerts
      {
        get
        {
          return (WdAlertLevel)(DataTools.GetInt(Base.Helper.GetProp(Base.Obj, "[DispID=94]")));
        }
      }

      public void SetDisplayAlerts(WdAlertLevel Value)
      {
        Base.Helper.SetProp(Base.Obj, "[DispID=94]", (int)Value);
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

    private struct WordDocument
    {
      #region Конструктор

      public WordDocument(ObjBase baseObj)
      {
        _Base = baseObj;
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

      #region Диапазоны

      /// <summary>
      /// Получить содержимое всего документа
      /// </summary>
      /// <returns></returns>
      public WordRange Range()
      {
        return new WordRange(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=2000]"), Base.Helper));
      }

      /// <summary>
      /// Получить содержимое части документа
      /// </summary>
      /// <param name="start">Начальная позиция в символах</param>
      /// <param name="end">Конечная позиция в символах</param>
      /// <returns></returns>
      public WordRange Range(int start, int end)
      {
        return new WordRange(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=2000]", start, end), Base.Helper));
      }

      #endregion
    }

    private struct WordDocuments
    {
      #region Конструктор

      public WordDocuments(ObjBase baseObj)
      {
        _Base = baseObj;
      }

      public ObjBase Base { get { return _Base; } }
      private ObjBase _Base;

      #endregion

      #region Методы

      /// <summary>
      /// Создать пустой новый документ
      /// </summary>
      /// <returns></returns>
      public WordDocument Add()
      {
        WordDocument doc = new WordDocument(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=14]", Missing.Value, Missing.Value), Base.Helper));
        return doc;
      }

      /// <summary>
      /// Открыть документ Word
      /// </summary>
      /// <param name="fileName">Имя файла</param>
      /// <returns></returns>
      public WordDocument Open(string fileName)
      {
        return new WordDocument(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=19]", fileName), Base.Helper));
      }

      #endregion
    }

    #endregion

    #region Диапазон

    private struct WordRange
    {
      #region Конструктор

      public WordRange(ObjBase baseObj)
      {
        _Base = baseObj;
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

      #region Вставка файла

      public void InsertFile(string fileName)
      {
        Base.Helper.Call(Base.Obj, "[DispID=123]", fileName);
      }

      #endregion
    }


    #endregion

    #endregion

    #region Программная модель Excel

    #region Перечисления

    private enum XlSheetType
    {
      xlWorksheet = -4167,
      xlDialogSheet = -4116,
      xlChart = -4109,
      xlExcel4MacroSheet = 3,
      xlExcel4IntlMacroSheet = 4,
    }

    private enum XlWBATemplate
    {
      xlWBATWorksheet = -4167,
      xlWBATChart = -4109,
      xlWBATExcel4MacroSheet = 3,
      xlWBATExcel4IntlMacroSheet = 4,
    }

    #endregion

    #region Основной объект

    private class ExcelHelper : OLEHelper
    {
      #region Конструктор и Disposing

      /// <summary>
      /// Запуск нового экземпляра Microsoft Excel или открытие существующего 
      /// (текущего) объекта
      /// </summary>
      /// <param name="createApp">Если true, то будет создан новый экземпляр приложения.
      /// Если false, то будет использован существующий экземпляр, если он есть</param>
      public ExcelHelper(bool createApp)
      {
        ShowOnEnd = true;
        if (createApp)
          CreateMainObj("Excel.Application");
        else
          GetActiveMainObj("Excel.Application");
        _Application = new ExcelApplication(new ObjBase(MainObj, this));
        MicrosoftOfficeTools.InitOleHelper(this, _Application.Base, "[DispID=392]"); // Свойство "Version"
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

      public ExcelApplication Application { get { return _Application; } }
      private ExcelApplication _Application;

      #endregion
    }

    #endregion

    #region Приложение

    private struct ExcelApplication
    {
      #region Конструктор

      public ExcelApplication(ObjBase baseObj)
      {
        _Base = baseObj;
        _Workbooks = new ExcelWorkbooks();
      }

      public ObjBase Base { get { return _Base; } }
      private ObjBase _Base;

      #endregion

      #region Свойства

      /// <summary>
      /// Коллекция всех открытых книг
      /// </summary>
      public ExcelWorkbooks Workbooks
      {
        get
        {
          if (_Workbooks.Base.IsEmpty)
          {
            _Workbooks = new ExcelWorkbooks(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=572]"), Base.Helper));
          }
          return _Workbooks;
        }
      }
      private ExcelWorkbooks _Workbooks;

      /// <summary>
      /// Активный лист.
      /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
      /// создается новый объект
      /// </summary>
      public ExcelWorksheet ActiveSheet
      {
        get
        {
          return new ExcelWorksheet(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=307]"), Base.Helper));
        }
      }

      /// <summary>
      /// Активная книга.
      /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
      /// создается новый объект
      /// </summary>
      public ExcelWorkbook ActiveWorkbook
      {
        get
        {
          return new ExcelWorkbook(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=308]"), Base.Helper));
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
          return DataTools.GetInt(Base.Helper.GetProp(Base.Obj, "[DispID=314]"));
        }
      }

      /// <summary>
      /// Получить версию Excel как объект
      /// </summary>
      public Version Version
      {
        get
        {
          string VerStr = VersionStr;
          int Bld = Build;

          //MessageBox.Show("VerStr=\""+VerStr+"\", Build="+Bld.ToString());

          Version Ver = FileTools.GetVersionFromStr(VerStr);
          if (Ver != null)
          {
            if (Ver.Build <= 0)
              Ver = new Version(Ver.Major, Ver.Major, Bld, 0);
          }
          return Ver;
        }
      }

      /// <summary>
      /// Надо ли выдавать подтверждения пользователю
      /// </summary>
      public bool DisplayAlerts
      {
        get
        {
          return DataTools.GetBool(Base.Helper.GetProp(Base.Obj, "[DispID=343]"));
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

    private struct ExcelWorkbook
    {
      #region Конструктор

      public ExcelWorkbook(ObjBase baseObj)
      {
        _Base = baseObj;
        _Sheets = new ExcelWorksheets();
      }

      public ObjBase Base { get { return _Base; } }
      private ObjBase _Base;

      #endregion

      #region Свойства

      public ExcelWorksheets Sheets
      {
        get
        {
          if (_Sheets.Base.IsEmpty)
            _Sheets = new ExcelWorksheets(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=485]"), Base.Helper));
          return _Sheets;
        }
      }
      private ExcelWorksheets _Sheets;

      /// <summary>
      /// Активный лист.
      /// Полученный объект рекомендуется сохранять, т.к. при каждом обращении к свойству
      /// создается новый объект
      /// </summary>
      public ExcelWorksheet ActiveSheet
      {
        get
        {
          return new ExcelWorksheet(new ObjBase(Base.Helper.GetProp(Base.Obj, "[DispID=307]"), Base.Helper));
        }
      }

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

      #endregion

      #region Методы

      public void Close()
      {
        Base.Helper.Call(Base.Obj, "[DispID=277]");
      }

      #endregion
    }

    private struct ExcelWorkbooks
    {
      #region Конструктор

      public ExcelWorkbooks(ObjBase baseObj)
      {
        _Base = baseObj;
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
      public ExcelWorkbook Add(int sheetCount)
      {
        ExcelWorkbook wbk = new ExcelWorkbook(new ObjBase(Base.Helper.Call(Base.Obj, "[DispID=181]", XlWBATemplate.xlWBATWorksheet), Base.Helper));

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

    private struct ExcelWorksheet
    {
      #region Конструктор

      public ExcelWorksheet(ObjBase baseObj)
      {
        _Base = baseObj;
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

      private void SetName(string value)
      {
        Base.Helper.SetProp(Base.Obj, "[DispID=110]", value);
      }

      #endregion

      #region Методы

      /// <summary>
      /// Сделать лист текущим
      /// </summary>
      public void Select()
      {
        Base.Helper.Call(Base.Obj, "[DispID=235]");
      }

      #endregion
    }

    private struct ExcelWorksheets
    {
      #region Конструктор

      public ExcelWorksheets(ObjBase baseObj)
      {
        _Base = baseObj;
      }

      public ObjBase Base { get { return _Base; } }
      private ObjBase _Base;

      #endregion

      #region Свойства

      public ExcelWorksheet this[int sheetIndex]
      {
        get
        {
          ExcelWorksheet Sheet = new ExcelWorksheet(new ObjBase(Base.Helper.GetIndexProp(Base.Obj, "[DispID=0]", sheetIndex), Base.Helper));
          return Sheet;
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
      public ExcelWorksheet Add()
      {
        Add(1);
        return this[Count];
      }

      public void Add(int count)
      {
        object LastObj;
        int n = this.Count;
        if (n == 0)
          LastObj = Missing.Value;
        else
          LastObj = this[n].Base.Obj;

        // Если добавить листы так, то они будут поименованы задом наперед
        //AnyDocType.Helper.Call(AnyDocType.Obj, "[DispID=181]",
        //  Missing.ValueEx, LastObj, Count, XlSheetType.xlWorksheet);
        for (int i = 0; i < count; i++)
        {
          Base.Helper.Call(Base.Obj, "[DispID=181]",
            Missing.Value, LastObj, 1, XlSheetType.xlWorksheet);
          LastObj = this[n + i + 1].Base.Obj;
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

    #endregion

    /// <summary>
    /// Открыть файл текстового документа в редакторе Microsoft Office Word
    /// </summary>
    /// <param name="fileName">Полный путь к doc-, rtf- или html- файлу</param>
    /// <param name="asTemplate">Если true, то файл используется как шаблон.
    /// В заголовке не будет показано имя файла, а команда "Сохранить" предложит выбрать имя файла.
    /// Используется для реализации команд "Отправить"</param>
    public static void OpenWithWord(AbsPath fileName, bool asTemplate)
    {
      if (!File.Exists(fileName.Path))
        throw new FileNotFoundException("Файл не найден: \"" + fileName.Path + "\"", fileName.Path);

      try
      {
        using (WordHelper Helper = new WordHelper())
        {
          WdAlertLevel OldDisplayAlerts = Helper.Application.DisplayAlerts;
          Helper.Application.SetDisplayAlerts(WdAlertLevel.wdAlertsNone);
          try
          {
            if (asTemplate)
            {
              WordDocument doc = Helper.Application.Documents.Add();
              doc.Range().InsertFile(fileName.Path);
              doc.Saved = true;
            }
            else
              Helper.Application.Documents.Open(fileName.Path);
          }
          finally
          {
            Helper.Application.SetDisplayAlerts(OldDisplayAlerts);
          }
        }
      }
      catch (Exception e)
      {
        e.Data["OpenWithWord() - FileName"] = fileName;
        e.Data["OpenWithWord() - AsTemplate"] = asTemplate;
        throw;
      }
    }

    /// <summary>
    /// Открыть файл электронной таблицы в Microsoft Office Excel
    /// </summary>
    /// <param name="fileName">Полный путь к xls-файлу</param>
    /// <param name="asTemplate">Если true, то файл используется как шаблон.
    /// В заголовке не будет показано имя файла, а команда "Сохранить" предложит выбрать имя файла.
    /// Используется для реализации команд "Отправить"</param>
    public static void OpenWithExcel(AbsPath fileName, bool asTemplate)
    {
      if (!File.Exists(fileName.Path))
        throw new FileNotFoundException("Файл не найден: \"" + fileName.Path + "\"", fileName.Path);
      try
      {
        using (ExcelHelper Helper = new ExcelHelper(true))
        {
          bool OldDisplayAlerts = Helper.Application.DisplayAlerts; // по идее, всегда возвращает true
          Helper.Application.SetDisplayAlerts(false);
          try
          {
            if (asTemplate)
            {
              Helper.Application.Workbooks.Open(fileName.Path);
              ExcelWorkbook wbk1 = Helper.Application.ActiveWorkbook;
              string Title = wbk1.Title;
              string Subject = wbk1.Subject;
              string Author = wbk1.Author;
              string Comments = wbk1.Comments;
              wbk1.Sheets.Copy();
              wbk1.Close();

              ExcelWorkbook wbk2 = Helper.Application.ActiveWorkbook;
              wbk2.SetTitle(Title);
              wbk2.SetSubject(Subject);
              wbk2.SetAuthor(Author);
              wbk2.SetComments(Comments);
              wbk2.Saved = true;
            }
            else
            {
              Helper.Application.Workbooks.Open(fileName.Path);
            }
          }
          finally
          {
            Helper.Application.SetDisplayAlerts(OldDisplayAlerts);
          }
        }
      }
      catch (Exception e)
      {
        e.Data["OpenWithExcel() - FileName"] = fileName;
        e.Data["OpenWithExcel() - AsTemplate"] = asTemplate;
        throw;
      }
    }

    #endregion

    #region Форматы ячеек по умолчанию

    /// <summary>
    /// Формат даты в Excel по умолчанию.
    /// Возвращаемое значение зависит от CultureInfo.CurrentCulture
    /// Для русской Windows возвращает "dd/MM/yyyy"
    /// </summary>
    public static string DefaultShortDateFormat
    {
      get
      {
        return GetDefaultShortDateFormat(CultureInfo.CurrentCulture);
      }
    }

    /// <summary>
    /// Получить короткий формат даты для заданной культуры
    /// </summary>
    /// <param name="culture">Культура</param>
    /// <returns>Формат</returns>
    public static string GetDefaultShortDateFormat(CultureInfo culture)
    {
      string s = culture.DateTimeFormat.ShortDatePattern;
      // строка содержит разделители компонентов даты как литералы ".", а не стандартные разделители "/"
      string StdSep = culture.DateTimeFormat.DateSeparator;
      if ((!String.IsNullOrEmpty(StdSep)) && s.IndexOf('/') < 0)
      {
        s = s.Replace(StdSep, "/");
      }

      return s;
    }

    /// <summary>
    /// Формат времени в Excel по умолчанию.
    /// Возвращаемое значение зависит от CultureInfo.CurrentCulture
    /// Для русской Windows возвращает "чч:мм:cc"
    /// </summary>
    public static string DefaultShortTimeFormat
    {
      get
      {
        return GetDefaultShortTimeFormat(CultureInfo.CurrentCulture);
      }
    }

    /// <summary>
    /// Получить короткий формат времени для заданной культуры
    /// </summary>
    /// <param name="culture">Культура</param>
    /// <returns>Формат</returns>
    public static string GetDefaultShortTimeFormat(CultureInfo culture)
    {
      string s = culture.DateTimeFormat.ShortTimePattern;
      // строка может содержать разделители компонентов времени как литералы, а не стандартные разделители ":"
      string StdSep = culture.DateTimeFormat.TimeSeparator;
      if ((!String.IsNullOrEmpty(StdSep)) && s.IndexOf(':') < 0)
      {
        s = s.Replace(StdSep, ":");
      }

      return s;
    }

    /// <summary>
    /// Формат даты и времени в Excel по умолчанию.
    /// Возвращаемое значение зависит от CultureInfo.CurrentCulture
    /// Для русской Windows возвращает "dd/MM/yyyy"
    /// </summary>
    public static string DefaultShortDateTimeFormat
    {
      get
      {
        return GetDefaultShortDateTimeFormat(CultureInfo.CurrentCulture);
      }
    }

    /// <summary>
    /// Получить короткий формат даты и времени для заданной культуры
    /// </summary>
    /// <param name="culture">Культура</param>
    /// <returns>Формат</returns>
    public static string GetDefaultShortDateTimeFormat(CultureInfo culture)
    {
      return GetDefaultShortDateFormat(culture) + " " + GetDefaultShortTimeFormat(culture);
    }

    /// <summary>
    /// Длинный формат даты в Excel по умолчанию.
    /// Возвращаемое значение зависит от CultureInfo.CurrentCulture
    /// </summary>
    public static string DefaultLongDateFormat
    {
      get
      {
        return GetDefaultLongDateFormat(CultureInfo.CurrentCulture);
      }
    }

    /// <summary>
    /// Получить длинный формат даты для заданной культуры
    /// </summary>
    /// <param name="culture">Культура</param>
    /// <returns>Формат</returns>
    public static string GetDefaultLongDateFormat(CultureInfo culture)
    {
      return culture.DateTimeFormat.LongDatePattern;
    }

    /// <summary>
    /// Длинный формат даты и времени в Excel по умолчанию.
    /// Возвращаемое значение зависит от CultureInfo.CurrentCulture
    /// </summary>
    public static string DefaultLongDateTimeFormat
    {
      get
      {
        return GetDefaultLongDateTimeFormat(CultureInfo.CurrentCulture);
      }
    }

    /// <summary>
    /// Получить длинный формат даты и времени для заданной культуры
    /// </summary>
    /// <param name="culture">Культура</param>
    /// <returns>Формат</returns>
    public static string GetDefaultLongDateTimeFormat(CultureInfo culture)
    {
      return GetDefaultLongDateFormat(culture) + " " + GetDefaultShortDateFormat(culture);
    }


    #endregion
  }


  /// <summary>
  /// Хранение адреса ячейки Excel (строки и столбца).
  /// Нумерация начинается с 1.
  /// Проверка верхней границы (65535 строк, 255 столбцов) не выполняется
  /// </summary>
  [Serializable]
  public struct ExcelRCNumber : IEquatable<ExcelRCNumber>
  {
    #region Конструктор

    /// <summary>
    /// Создает заполненный адрес.
    /// </summary>
    /// <param name="rowNumber">Номер строки. Нумерация начинается с 1</param>
    /// <param name="columnNumber">Номер столбца. Нумерация начинается с 1</param>
    public ExcelRCNumber(int rowNumber, int columnNumber)
    {
      if (rowNumber < 1)
        throw new ArgumentOutOfRangeException("rowNumber", rowNumber, "Номер строки должен быть больше 0");
      if (columnNumber < 1)
        throw new ArgumentOutOfRangeException("columnNumber", columnNumber, "Номер столбца должен быть больше 0");

      _RowNumber = rowNumber;
      _ColumnNumber = columnNumber;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Номер строки. Нумерация начинается с 1
    /// </summary>
    public int RowNumber { get { return _RowNumber; } }
    private int _RowNumber;

    /// <summary>
    /// Номер столбца. Нумерация начинается с 1
    /// </summary>
    public int ColumnNumber { get { return _ColumnNumber; } }
    private int _ColumnNumber;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _RowNumber == 0; } }

    #endregion

    #region Сравнение

    /// <summary>
    /// Возвращает true, если адреса одинаковые
    /// </summary>
    /// <param name="a">Первый сравниваемый адрес</param>
    /// <param name="b">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(ExcelRCNumber a, ExcelRCNumber b)
    {
      return a.RowNumber == b.RowNumber && a.ColumnNumber == b.ColumnNumber;
    }

    /// <summary>
    /// Возвращает true, если адреса разные
    /// </summary>
    /// <param name="a">Первый сравниваемый адрес</param>
    /// <param name="b">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(ExcelRCNumber a, ExcelRCNumber b)
    {
      return a.RowNumber != b.RowNumber || a.ColumnNumber != b.ColumnNumber;
    }

    /// <summary>
    /// Возвращает true, если адреса одинаковые
    /// </summary>
    /// <param name="other">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(ExcelRCNumber other)
    {
      return this == other;
    }

    /// <summary>
    /// Возвращает true, если адреса одинаковые
    /// </summary>
    /// <param name="other">Второй сравниваемый адрес</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is ExcelRCNumber)
        return this == (ExcelRCNumber)other;
      else
        return false;
    }

    /// <summary>
    /// Хэш-код для коллекций
    /// </summary>
    /// <returns>Числовой код</returns>
    public override int GetHashCode()
    {
      return ((RowNumber & 0xFFFF) << 8) | (ColumnNumber & 0xFFFF);
    }

    #endregion

    #region Преобразование в строку / из строки

    /// <summary>
    /// Возвращает текстовое представление в формате "A1"
    /// </summary>
    /// <returns>Тестовое представление</returns>
    public override string ToString()
    {
      if (_RowNumber == 0)
        return "Empty";
      return MicrosoftOfficeTools.GetExcelColumnName(ColumnNumber) + RowNumber.ToString();
    }

    /// <summary>
    /// Преобразование строки в формате "A1"
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <returns>Структура</returns>
    public static ExcelRCNumber Parse(string s)
    {
      if (String.IsNullOrEmpty(s))
        throw new ArgumentNullException(s);

      ExcelRCNumber res;
      if (TryParse(s, out res))
        return res;
      else
        throw new FormatException();
    }

    /// <summary>
    /// Попытка преобразования строки в формате "A1" в адрес ячейки
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="res">Результат преобразования или неинициализированная структура, в случае ошибки</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out ExcelRCNumber res)
    {
      if (String.IsNullOrEmpty(s))
      {
        res = new ExcelRCNumber();
        return false;
      }

      s = s.ToUpperInvariant();

      int nChars = 0;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] >= 'A' && s[i] <= 'Z')
          nChars++;
        else
          break;
      }

      if (nChars < 1 || nChars == s.Length)
      {
        res = new ExcelRCNumber();
        return false;
      }

      int ColumnNumber;
      if (!MicrosoftOfficeTools.TryGetExcelColumnNumber(s.Substring(0, nChars), out ColumnNumber))
      {
        res = new ExcelRCNumber();
        return false;
      }

      // Надо проверить, что идут только цифры.
      // Первый символ не может быть 0.

      if (s[nChars] < '1' || s[nChars] > '9')
      {
        res = new ExcelRCNumber();
        return false;
      }

      // Дальше можно использовать стандартное преобразование
      int RowNumber;
      if (!int.TryParse(s.Substring(nChars), System.Globalization.NumberStyles.None,
        CultureInfo.InvariantCulture, out RowNumber))
      {
        res = new ExcelRCNumber();
        return false;
      }


      res = new ExcelRCNumber(RowNumber, ColumnNumber);
      return true;
    }

    #endregion
  }
}
