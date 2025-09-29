using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Reporting;

namespace FreeLibSet.Forms.Reporting
{
  #region Перечисления

  /// <summary>
  /// Режим вывода нумерации колонок под заголовками столбцов
  /// (свойство GridPageSetup.ColumnSubHeaderNumbers)
  /// </summary>
  public enum BRDataViewColumnSubHeaderNumbersMode
  {
    /// <summary>
    /// Нумерация колонок не используется (по умолчанию)
    /// </summary>
    None,

    /// <summary>
    /// Нумерация колонок выводится на каждой странице под заголовками столбцов
    /// </summary>
    All,

    /// <summary>
    /// На первой странице выводятся заголовки столбцов и нумерация колонок, на
    /// второй странице и дальше выводятся номера столбцов без заголовков
    /// </summary>
    Replace
  }

  /// <summary>
  /// Оформление границ таблицы при печати
  /// </summary>
  public enum BRDataViewBorderStyle
  {
    /// <summary>
    /// Нет линий
    /// </summary>
    None,

    /// <summary>
    /// Заголовки столбцов обведены рамкой, других линий нет
    /// </summary>
    Headers,

    /// <summary>
    /// Столбцы разделены вертикальными линиями, между строками линий нет.
    /// Заголовки столбцов обведены рамкой, внизу таблицы также граница.
    /// </summary>
    Vertical,

    /// <summary>
    /// Сетка. Рисуются линии между строками и столбцами
    /// </summary>
    All
  }

  /// <summary>
  /// Способ раскраски ячеек таблицы
  /// </summary>
  public enum BRDataViewColorStyle
  {
    /// <summary>
    /// При печати раскраска не используется (весь фон - белый)
    /// </summary>
    NoColors,

    /// <summary>
    /// При печати используются те же цвета, что и на экране
    /// </summary>
    Screen,

    /// <summary>
    /// Используются серые цвета для выделения строк
    /// </summary>
    Gray
  }

  ///// <summary>
  ///// Межстрочный интервал
  ///// </summary>
  //internal enum PrintGridRowSpacing
  //{
  //  /// <summary>
  //  /// Не сжатый (определяется метрикой выбранного шрифта)
  //  /// </summary>
  //  Lead100,


  //  /// <summary>
  //  /// Частично сжатый (оставлено 3/4 Lead space)
  //  /// </summary>
  //  Lead75,

  //  /// <summary>
  //  /// Половинный (половина интервала, определенного метрикой выбранного шрифта)
  //  /// </summary>
  //  Lead50,

  //  /// <summary>
  //  /// Сжатый на 3/4 (оставлена четверть Lead space)
  //  /// </summary>
  //  Lead25,

  //  /// <summary>
  //  /// Сжатый (межстрочный интервал равен высоте шрифта без Lead space)
  //  /// </summary>
  //  Lead0,
  //}

  /// <summary>
  /// Значения свойства EFPDataGridViewExpExcelSettings.BoolMode
  /// </summary>
  public enum BRDataViewBooleanMode
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
    Integer,

    /// <summary>
    /// "[X]"=ИСТИНА, "[ ]"=ЛОЖЬ или другой замещающий текст
    /// </summary>
    Text,
  }


  #endregion

  /// <summary>
  /// Данные параметров страницы для печати/отправки/экспорта в файл просмотров <see cref="EFPDataGridView"/> и <see cref="EFPDataTreeView"/>.
  /// Содержит список выбранных столбцов для печати, их размеры и параметры для оформления.
  /// Дополняет параметры страницы <see cref="BRPageSetup"/> и параметры шрифта <see cref="BRFontSettingsDataItem"/>
  /// </summary>
  public class BRDataViewSettingsDataItem : SettingsDataItem, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает набор настроек по умолчанию
    /// </summary>
    public BRDataViewSettingsDataItem()
    {
      #region Столбцы

      _ColumnDict = new Dictionary<string, ColumnInfo>();
      _SizeGroupDict = new Dictionary<string, SizeGroupInfo>();
      _ColumnSubHeaderNumbers = BRDataViewColumnSubHeaderNumbersMode.None;
      _RepeatedColumnCount = 0;

      #endregion

      #region Оформление

      _UseColorStyle = true;
      _UseBoolMode = true;

      _BooleanMode = BRDataViewBooleanMode.Text;
      _BooleanTextTrue = "[X]";
      _BooleanTextFalse = "[ ]";
      _ColorStyle = BRDataViewColorStyle.NoColors;
      _BorderStyle = BRDataViewBorderStyle.All;

      _CellLeftMargin = BRReport.AppDefaultCellStyle.LeftMargin;
      _CellTopMargin = BRReport.AppDefaultCellStyle.TopMargin;
      _CellRightMargin = BRReport.AppDefaultCellStyle.RightMargin;
      _CellBottomMargin = BRReport.AppDefaultCellStyle.BottomMargin;

      #endregion

      #region Отправить

      _ExpRange = EFPDataViewExpRange.All;
      _UseExpTableHeader = true;
      _ExpTableHeader = true;
      _UseExpTableFilters = true;
      _ExpTableFilters = true;
      _UseExpColumnHeaders = true;
      _ExpColumnHeaders = true;

      #endregion

      #region Экспорт в текстовый файл

      _CodePage = Encoding.UTF8.CodePage;
      //_NewLine = Environment.NewLine;
      _FieldDelimiter = ',';
      _Quote = '\"';
      _SingleLineField = true;
      _RemoveDoubleSpaces = false;

      #endregion

      #region Экспорт в DBF

      _DbfCodePage = Encoding.Default.CodePage;

      #endregion
    }

    #endregion

    #region Вкладка "Столбцы"

    #region Внутренние словари

    const int AutoWidth = 0;

    private class ColumnInfo
    {
      #region Печать

      /// <summary>
      /// True (по умолчанию), если столбец печатается
      /// False вместо константы NonPrintWidth 
      /// </summary>
      public bool Print;

      /// <summary>
      /// Ширина при печати в единицах 0.1мм
      /// Значение AutoWidth=0, если ширина определяется автоматически.
      /// </summary>
      public int PrintWidth;

      /// <summary>
      /// Признак автоматического увеличения ширины столбца при печати для заполнения страницы.
      /// По умолчанию - false
      /// </summary>
      public bool AutoGrow;

      #endregion

      #region DBF

      /// <summary>
      /// Если true (по умолчанию), то столбец выгружается в DBF. 
      /// </summary>
      public bool DbfExported;

      /// <summary>
      /// Имя DBF поля – заданное вручную или полученное автоматически при вызове GetRealDbfColumnNames()
      /// </summary>
      public string DbfFieldName;

      /// <summary>
      /// True, если имя поля было присвоено пользователем вручную (вызов SetDbfFieldName()), false, если имя присвоено автоматически
      /// </summary>
      public bool IsManualDbfName;

      #endregion
    }

    /// <summary>
    /// Информация о столбцах.
    /// Ключом является имя столбца <see cref="IEFPDataViewColumnBase.Name"/>.
    /// </summary>
    private readonly Dictionary<string, ColumnInfo> _ColumnDict;

    private class SizeGroupInfo
    {
      #region Печать

      /// <summary>
      /// Ширина при печати в единицах 0.1мм
      /// Значение AutoWidth=0, если ширина определяется автоматически.
      /// </summary>
      public int PrintWidth;

      /// <summary>
      /// Признак автоматического увеличения ширины столбца при печати для заполнения страницы.
      /// По умолчанию - false
      /// </summary>
      public bool AutoGrow;

      #endregion
    }

    /// <summary>
    /// Размеры столбцов с заданной размерной группой.
    /// Ключ - свойство <see cref="IEFPDataViewColumnBase.SizeGroup"/>.
    /// </summary>
    private readonly Dictionary<string, SizeGroupInfo> _SizeGroupDict;

    private ColumnInfo GetColumnInfo(IEFPDataViewColumnBase column)
    {
#if DEBUG
      if (column == null)
        throw new ArgumentNullException("column");
#endif
      ColumnInfo ci;
      if (!_ColumnDict.TryGetValue(column.Name, out ci))
      {
        ci = new ColumnInfo();
        ci.Print = true;
        ci.AutoGrow = column.AutoGrow;
        ci.DbfExported = true;
        ci.DbfFieldName = String.Empty;

        _ColumnDict.Add(column.Name, ci);
      }

      // Размерной группы может не быть в секции конфигурации
      if (!String.IsNullOrEmpty(column.SizeGroup))
      {
        SizeGroupInfo sgi;
        if (!_SizeGroupDict.TryGetValue(column.SizeGroup, out sgi))
        {
          sgi = new SizeGroupInfo();
          sgi.PrintWidth = ci.PrintWidth;
          sgi.AutoGrow = ci.AutoGrow;
          _SizeGroupDict.Add(column.SizeGroup, sgi);
        }
        else
        {
          ci.PrintWidth = sgi.PrintWidth;
          ci.AutoGrow = sgi.AutoGrow;
        }
      }

      return ci;
    }

    #endregion

    /// <summary>
    /// Получить флажок печати столбца
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <returns>Признак печати</returns>
    public bool GetColumnPrinted(IEFPDataViewColumnBase column)
    {
      if (!column.Printable)
        return false;

      return GetColumnInfo(column).Print;
    }

    /// <summary>
    /// Установить флажок печати столбца
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <param name="value">true, если столбец должен быть напечатан</param>
    public void SetColumnPrinted(IEFPDataViewColumnBase column, bool value)
    {
      if (!column.Printable)
        return;

      GetColumnInfo(column).Print = value;
    }

    /// <summary>
    /// Получить ширину столбца в единицах 0.1мм.
    /// Возвращает 0, если ширина определяется автоматически.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <returns>Ширина или 0</returns>
    public int GetColumnPrintWidth(IEFPDataViewColumnBase column)
    {
      if (!column.Printable)
        return 0;

      ColumnInfo ci = GetColumnInfo(column); // заодно создает SizeGroupInfo
      if (String.IsNullOrEmpty(column.SizeGroup))
        return ci.PrintWidth;
      else
        return _SizeGroupDict[column.SizeGroup].PrintWidth;
    }

    /// <summary>
    /// Получить ширину столбцах в единицах 0.1мм.
    /// Если ширина не задана в явном виде, то возвращается ширина, исходя из размера столбца на экране и параметров шрифта для печати.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <param name="fontSettings">Настройки шрифта для печати</param>
    /// <returns>Ширина</returns>
    public int GetRealColumnWidth(IEFPDataViewColumnBase column, BRFontSettingsDataItem fontSettings)
    {
      int w = GetColumnPrintWidth(column);
      if (w == AutoWidth)
        return GetDefaultWidth(column, fontSettings);
      else
        return w;
    }


    /// <summary>
    /// Установить ширину столбца в единицах 0.1мм.
    /// Нулевое значение соответствует автоматическому размеру столбца.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <param name="value">Ширина или 0</param>
    public void SetColumnPrintWidth(IEFPDataViewColumnBase column, int value)
    {
      if (value < 0)
        throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);
      if (!column.Printable)
        return;

      ColumnInfo ci = GetColumnInfo(column); // заодно создает SizeGroupInfo
      ci.PrintWidth = value;
      if (!String.IsNullOrEmpty(column.SizeGroup))
        _SizeGroupDict[column.SizeGroup].PrintWidth = value;
    }

    private int GetDefaultWidth(IEFPDataViewColumnBase column, BRFontSettingsDataItem fontSettings)
    {
      //return (int)Math.Round(column.WidthPt / 72.0 * 254.0, 0, MidpointRounding.AwayFromZero);

      double tw = column.TextWidth;
      if (column.AutoGrow && column.MinTextWidth > 0)
        tw = column.MinTextWidth;

      int nChars = (int)(Math.Ceiling(tw));
      if (nChars < 1)
        nChars = 1;
      string testString = new string('0', nChars);
      BRCellStyleStorage cellStyle = new BRCellStyleStorage(null);
      fontSettings.InitCellStyle(cellStyle);
      int w, h;
      Drawing.Reporting.BRMeasurer.Default.MeasureString(testString, cellStyle, out w, out h);
      return w + this.CellLeftMargin + this.CellRightMargin;
    }

    /// <summary>
    /// Получить признак автоматического увеличения ширины столбца при печати для заполнения ширины столбца.
    /// Если true, то <see cref="SetColumnPrintWidth(IEFPDataViewColumnBase, int)"/> задает минимальную ширину столбца.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <returns>Признак автоматического увеличения ширины</returns>
    public bool GetColumnAutoGrow(IEFPDataViewColumnBase column)
    {
      if (!GetColumnPrinted(column))
        return false;
      ColumnInfo ci = GetColumnInfo(column); // заодно создает SizeGroupInfo
      if (String.IsNullOrEmpty(column.SizeGroup))
        return ci.AutoGrow;
      else
        return _SizeGroupDict[column.SizeGroup].AutoGrow;
    }

    /// <summary>
    /// Установить признак автоматического увеличения ширины столбца при печати для заполнения ширины столбца.
    /// Если true, то <see cref="SetColumnPrintWidth(IEFPDataViewColumnBase, int)"/> задает минимальную ширину столбца.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <param name="value">Признак автоматического увеличения ширины</param>
    public void SetColumnAutoGrow(IEFPDataViewColumnBase column, bool value)
    {
      if (!GetColumnPrinted(column))
        return;

      ColumnInfo ci = GetColumnInfo(column); // заодно создает SizeGroupInfo
      ci.AutoGrow = value;
      if (!String.IsNullOrEmpty(column.SizeGroup))
        _SizeGroupDict[column.SizeGroup].AutoGrow = value;
    }

    /// <summary>
    /// Количество повторяемых в начале каждой полосы столбцов
    /// </summary>
    public int RepeatedColumnCount { get { return _RepeatedColumnCount; } set { _RepeatedColumnCount = value; } }
    private int _RepeatedColumnCount;

    /// <summary>
    /// Нумерация колонок под заголовками столбцов
    /// (по умолчанию - None)
    /// </summary>
    public BRDataViewColumnSubHeaderNumbersMode ColumnSubHeaderNumbers
    {
      get { return _ColumnSubHeaderNumbers; }
      set { _ColumnSubHeaderNumbers = value; }
    }
    private BRDataViewColumnSubHeaderNumbersMode _ColumnSubHeaderNumbers;


    #endregion

    #region Вкладка "Оформление"

    #region Управляющие свойства

    /// <summary>
    /// Наличие свойства <see cref="ColorStyle"/> (только для <see cref="EFPDataGridView"/>)
    /// </summary>
    public bool UseColorStyle { get { return _UseColorStyle; } set { _UseColorStyle = value; } }
    private bool _UseColorStyle;

    /// <summary>
    /// Наличие логических значений в просмотре
    /// </summary>
    public bool UseBoolMode { get { return _UseBoolMode; } set { _UseBoolMode = value; } }
    private bool _UseBoolMode;

    #endregion

    /// <summary>
    /// Стиль оформления рамок при печати табличного просмотра.
    /// По умолчанию - <see cref="BRDataViewBorderStyle.All"/>
    /// </summary>
    public BRDataViewBorderStyle BorderStyle { get { return _BorderStyle; } set { _BorderStyle = value; } }
    private BRDataViewBorderStyle _BorderStyle;

    /// <summary>
    /// Стиль оформления цвета ячеек и текста при печати.
    /// По умолчанию - <see cref="BRDataViewColorStyle.NoColors"/>
    /// </summary>
    public BRDataViewColorStyle ColorStyle { get { return _ColorStyle; } set { _ColorStyle = value; } }
    private BRDataViewColorStyle _ColorStyle;

    ///// <summary>
    ///// Межстрочный интервал
    ///// </summary>
    //public PrintGridRowSpacing RowSpacing { get { return _RowSpacing; } set { _RowSpacing = value; } }
    //private PrintGridRowSpacing _RowSpacing;

    /// <summary>
    /// Режим вывода логических значений.
    /// По умолчанию - <see cref="BRDataViewBooleanMode.Text"/>.
    /// </summary>
    public BRDataViewBooleanMode BooleanMode { get { return _BooleanMode; } set { _BooleanMode = value; } }
    private BRDataViewBooleanMode _BooleanMode;

    /// <summary>
    /// Замещающий текст для логического значения true при <see cref="BooleanMode"/>=Text.
    /// По умолчанию - "[X]".
    /// </summary>
    public string BooleanTextTrue { get { return _BooleanTextTrue; } set { _BooleanTextTrue = value; } }
    private string _BooleanTextTrue;

    /// <summary>
    /// Замещающий текст для логического значения false при <see cref="BooleanMode"/>=Text.
    /// По умолчанию - "[ ]".
    /// </summary>
    public string BooleanTextFalse { get { return _BooleanTextFalse; } set { _BooleanTextFalse = value; } }
    private string _BooleanTextFalse;

    /// <summary>
    /// Возвращает преобразованное логическое значение в соответствии со значением свойства <see cref="BooleanMode"/>
    /// </summary>
    /// <param name="value">Значение в таблице данных</param>
    /// <returns>Преобразованное значение</returns>
    public object GetBooleanValue(bool value)
    {
      switch (BooleanMode)
      {
        case BRDataViewBooleanMode.Boolean:
          return value;
        case BRDataViewBooleanMode.Text:
          return value ? BooleanTextTrue : BooleanTextFalse;
        case BRDataViewBooleanMode.Integer:
          return value ? 1 : 0;
        default:
          throw new BugException("BooleanMode=" + BooleanMode.ToString());
      }
    }


    /// <summary>
    /// Отступ от левого края ячейки в единицах 0.1мм.
    /// По умолчанию используется значение из <see cref="BRReport.AppDefaultCellStyle"/>.
    /// </summary>
    public int CellLeftMargin { get { return _CellLeftMargin; } set { _CellLeftMargin = value; } }
    private int _CellLeftMargin;

    /// <summary>
    /// Отступ от верхнего края ячейки в единицах 0.1мм.
    /// По умолчанию используется значение из <see cref="BRReport.AppDefaultCellStyle"/>.
    /// </summary>
    public int CellTopMargin { get { return _CellTopMargin; } set { _CellTopMargin = value; } }
    private int _CellTopMargin;

    /// <summary>
    /// Отступ от правого края ячейки в единицах 0.1мм.
    /// По умолчанию используется значение из <see cref="BRReport.AppDefaultCellStyle"/>.
    /// </summary>
    public int CellRightMargin { get { return _CellRightMargin; } set { _CellRightMargin = value; } }
    private int _CellRightMargin;

    /// <summary>
    /// Отступ от нижнего края ячейки в единицах 0.1мм.
    /// По умолчанию используется значение из <see cref="BRReport.AppDefaultCellStyle"/>.
    /// </summary>
    public int CellBottomMargin { get { return _CellBottomMargin; } set { _CellBottomMargin = value; } }
    private int _CellBottomMargin;

    #endregion

    #region Диалог "Отправить"

    #region Управляющие свойства

    /// <summary>
    /// Наличие свойства <see cref="ExpTableHeader"/>.
    /// Устанавливается в true, если просмотр содержит заголовок таблицы
    /// </summary>
    public bool UseExpTableHeader { get { return _UseExpTableHeader; } set { _UseExpTableHeader = value; } }
    private bool _UseExpTableHeader;

    /// <summary>
    /// Наличие свойства <see cref="ExpTableFilters"/>.
    /// Устанавливается в true, если просмотр содержит табличку фильтров
    /// </summary>
    public bool UseExpTableFilters { get { return _UseExpTableFilters; } set { _UseExpTableFilters = value; } }
    private bool _UseExpTableFilters;

    /// <summary>
    /// Наличие свойства <see cref="ExpColumnHeaders"/>.
    /// Устанавливается в true, если просмотр содержит заголовки столбцов
    /// </summary>
    public bool UseExpColumnHeaders { get { return _UseExpColumnHeaders; } set { _UseExpColumnHeaders = value; } }
    private bool _UseExpColumnHeaders;

    #endregion

    /// <summary>
    /// При выполнении команды "Отправить": какой диапазон использовать: весь просмотр (по умолчанию) или только выбранные ячейки
    /// </summary>
    public EFPDataViewExpRange ExpRange { get { return _ExpRange; } set { _ExpRange = value; } }
    private EFPDataViewExpRange _ExpRange;

    /// <summary>
    /// При выполнении команды "Отправить": true (по умолчанию) - выводить заголовок таблицы, если он есть.
    /// </summary>
    public bool ExpTableHeader { get { return _ExpTableHeader; } set { _ExpTableHeader = value; } }
    private bool _ExpTableHeader;

    /// <summary>
    /// При выполнении команды "Отправить": true (по умолчанию) - выводить табличку фильтров, если она есть.
    /// </summary>
    public bool ExpTableFilters { get { return _ExpTableFilters; } set { _ExpTableFilters = value; } }
    private bool _ExpTableFilters;

    /// <summary>
    /// При выполнении команды "Отправить": true (по умолчанию) - выводить заголовки столбцов.
    /// Эта же настройка используется для экспорта в форматы txt и csv.
    /// Редактируется в двух вкладках диалога параметров
    /// </summary>
    public bool ExpColumnHeaders { get { return _ExpColumnHeaders; } set { _ExpColumnHeaders = value; } }
    private bool _ExpColumnHeaders;

    #endregion

    #region Экспорт в текстовый файл

    /// <summary>
    /// Кодировка при экспорте в форматы TXT и CSV.
    /// По умолчанию - UTF8.
    /// Для DBF-формата используется отдельное свойство <see cref="DbfCodePage"/>.
    /// </summary>
    public int CodePage { get { return _CodePage; } set { _CodePage = value; } }
    private int _CodePage;

    ///// <summary>
    ///// Разделитель строк. По умолчанию - <see cref="Environment.NewLine"/>.
    ///// Внимание! В RFC 4180 используется разделитель CR+LF. Для соответствия стандарту на не-Windows платформах
    ///// следует установить свойство вручную.
    ///// </summary>
    //public string NewLine
    //{
    //  get { return _NewLine; }
    //  set
    //  {
    //    _NewLine = value;
    //  }
    //}
    //private string _NewLine;

    /// <summary>
    /// Символ-разделитель полей в пределах строки.
    /// По умолчанию - запятая.
    /// Используется только для формата CSV.
    /// </summary>
    public char FieldDelimiter
    {
      get { return _FieldDelimiter; }
      set { _FieldDelimiter = value; }
    }
    private char _FieldDelimiter;

    private string FieldDelimiterStr
    {
      get { return new string(FieldDelimiter, 1); }
      set { FieldDelimiter = value[0]; }
    }

    /// <summary>
    /// Символ кавычки.
    /// Используется только для формата CSV.
    /// </summary>
    public char Quote
    {
      get { return _Quote; }
      set { _Quote = value; }
    }
    private char _Quote;

    private string QuoteStr
    {
      get { return new string(Quote, 1); }
      set { Quote = value[0]; }
    }



    /// <summary>
    /// Замена в полях символов перевода строки на пробелы. По умолчанию - true.
    /// Используется только для формата CSV. В tabbed-формате всегда выполняется замена
    /// </summary>
    public bool SingleLineField { get { return _SingleLineField; } set { _SingleLineField = value; } }
    private bool _SingleLineField;

    /// <summary>
    /// Если true, то выполняется удаление двойных пробелов и пробелов в начале/конце текста.
    /// По умолчанию - false.
    /// </summary>
    public bool RemoveDoubleSpaces { get { return _RemoveDoubleSpaces; } set { _RemoveDoubleSpaces = value; } }
    private bool _RemoveDoubleSpaces;

    #endregion

    #region Экспорт в DBF

    /// <summary>
    /// Кодировка при экспорте в формат DBF.
    /// По умолчанию - кодировка операционной системы по умолчанию.
    /// </summary>
    public int DbfCodePage { get { return _DbfCodePage; } set { _DbfCodePage = value; } }
    private int _DbfCodePage;

    /// <summary>
    /// Устанавливает для столбца признак экспорта в DBF-формат
    /// </summary>
    /// <param name="column">Столбец просмотра</param>
    /// <param name="value">Признак экспорта</param>
    public void SetDbfExported(IEFPDataViewColumn column, bool value)
    {
      GetColumnInfo(column).DbfExported = value;
    }

    /// <summary>
    /// Возвращает заданный пользователем признак экспорта в DBF-формат.
    /// Если свойство <see cref="IEFPDataViewColumnBase.DbfPreliminaryInfo"/> возвращает null и столбец не может быть экспортирован, то возвращается false.
    /// </summary>
    /// <param name="column">Столбец просмотра</param>
    /// <returns>Признак экспорта</returns>
    public bool GetDbfExported(IEFPDataViewColumn column)
    {
      if (column.DbfPreliminaryInfo == null)
        return false;
      return GetColumnInfo(column).DbfExported;
    }

    /// <summary>
    /// Устанавливает имя поля при экспорте в DBF-формат.
    /// </summary>
    /// <param name="column">Столбец просмотра</param>
    /// <param name="value">Имя поля</param>
    public void SetDbfFieldName(IEFPDataViewColumn column, string value)
    {
      GetColumnInfo(column).DbfFieldName = value;
      GetColumnInfo(column).IsManualDbfName = true;
    }

    /// <summary>
    /// Возвращает имя поля для экспорта в DBF-формат.
    /// Возвращается значение, установленное пользователем, или пустая строка.
    /// Следует использовать метод <see cref="GetRealDbfFieldNames(IEFPDataViewColumn[])"/>, который также возвращает имена из свойств столбца и автоматически генерирует недостающие.
    /// </summary>
    /// <param name="column">Столбец просмотра</param>
    /// <returns>Имя поля</returns>
    public string GetDbfFieldName(IEFPDataViewColumn column)
    {
      return GetColumnInfo(column).DbfFieldName;
    }

    /// <summary>
    /// Возвращает имена полей для экспорта в DBF-формат.
    /// Учитываются настройки пользователя <see cref="GetDbfFieldName(IEFPDataViewColumn)"/>, имена из <see cref="IEFPDataViewColumnBase.DbfInfo"/> и <see cref="IEFPDataViewColumnBase.Name"/>.
    /// Недостающие имена полей генерируются автоматически. Возвращает список, в котором все имена заполнены и нет повторов.
    /// Признаки экспорта <see cref="GetDbfExported(IEFPDataViewColumn)"/> не учитываются.
    /// </summary>
    /// <param name="columns">Список столбцов для экспорта. Следует передавать полный список экспортируемых полей</param>
    /// <returns>Заполненный массив имен</returns>
    public string[] GetRealDbfFieldNames(IEFPDataViewColumn[] columns)
    {
      string[] aNames = new string[columns.Length];
      SingleScopeStringList lstNames = new SingleScopeStringList(true);

      #region 1. Имена из настроек пользователя

      for (int i = 0; i < columns.Length; i++)
      {
        string nm = GetDbfFieldName(columns[i]);
        if (!String.IsNullOrEmpty(nm))
        {
          if (!lstNames.Contains(nm))
          {
            aNames[i] = nm;
            lstNames.Add(nm);
          }
        }
      }

      #endregion

      #region 2. Имена из описания поля

      for (int i = 0; i < columns.Length; i++)
      {
        if (aNames[i] == null)
        {
          string nm = columns[i].DbfInfo.Name;
          if (String.IsNullOrEmpty(nm))
            nm = GetValidDbfNamePart(columns[i].Name);

          if (!lstNames.Contains(nm))
          {
            aNames[i] = nm;
            lstNames.Add(nm);
          }
        }
      }

      #endregion

      #region 3. Недостающие имена

      int cntRnd = 0;

      for (int i = 0; i < columns.Length; i++)
      {
        if (aNames[i] == null)
        {
          string nm = "F_0" + (i + 1).ToString("0000000", StdConvert.NumberFormat);
          while (lstNames.Contains(nm))
          {
            cntRnd++;
            nm = "F_1" + cntRnd.ToString("0000000", StdConvert.NumberFormat);
          }
          aNames[i] = nm;
          lstNames.Add(nm);
        }
      }
      #endregion

      return aNames;
    }

    private string GetValidDbfNamePart(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      s = StringTools.Substring(s.ToUpperInvariant(), 0, 10);
      if (s[0] < 'A' || s[0] > 'Z')
        return String.Empty;

      // Можно что-нибудь вернуть
      const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

      StringBuilder sb = new StringBuilder();
      sb.Append(s[0]);
      for (int i = 1; i < s.Length; i++)
      {
        if (validChars.IndexOf(s[i]) >= 0)
          sb.Append(s[i]);
      }
      return sb.ToString();
    }

    #endregion

    #region ISettingsDataItem

    // Размеры и признаки печати столбцов сохраняются во вложенной секции ColumnSizes.
    // Для каждого столбца задается отдельное значение:
    // Больше 0 - ширина в единицах 0.1мм
    // 0 - столбец не печатается.
    // Если значение отсутствует, то столбец печатается и используется значение по умолчанию. При этом столбец может растягиваться, чтобы заполнить лист
    //
    // Для получения размеров используется имя SizeGroup, а для признака печати - имя столбца

    /// <summary>
    /// Записать данные в секцию конфигурации.
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    /// <param name="part">Вариант хранения данных</param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        #region Столбцы

        CfgPart cfg2 = cfg.GetChild("Columns", true);
        // Нельзя чистить, так как затрутся данные из родительской настройки
        // cfg2.Clear();
        foreach (KeyValuePair<string, ColumnInfo> pair in _ColumnDict)
        {
          CfgPart cfg3 = cfg2.GetChild(pair.Key, true);
          cfg3.Clear();
          if (!pair.Value.Print)
            cfg3.SetBoolean("Print", false);
          else
          {
            cfg3.SetInt32("Width", pair.Value.PrintWidth);
            if (pair.Value.AutoGrow)
              cfg3.SetBoolean("AutoGrow", true);
          }

          if (pair.Value.DbfExported)
          {
            if (!String.IsNullOrEmpty(pair.Value.DbfFieldName))
              cfg3.SetString(pair.Value.IsManualDbfName ? "DbfName" : "AutoDbfName", pair.Value.DbfFieldName);
          }
          else
            cfg3.SetBoolean("Dbf", false);
        }

        if (_SizeGroupDict.Count > 0)
        {
          cfg2 = cfg.GetChild("ColumnSizeGroups", true);
          foreach (KeyValuePair<string, SizeGroupInfo> pair in _SizeGroupDict)
          {
            CfgPart cfg3 = cfg2.GetChild(pair.Key, true);
            cfg3.Clear();
            cfg3.SetInt32("Width", pair.Value.PrintWidth);
            if (pair.Value.AutoGrow)
              cfg3.SetBoolean("AutoGrow", true);
          }
        }
        cfg.SetInt32("RepeatedColumnCount", RepeatedColumnCount);
        cfg.SetEnum<BRDataViewColumnSubHeaderNumbersMode>("ColumnSubHeaderNumbers", ColumnSubHeaderNumbers);

        #endregion

        #region Оформление

        cfg.SetEnum<BRDataViewBorderStyle>("BorderStyle", BorderStyle);
        if (UseColorStyle)
          cfg.SetEnum<BRDataViewColorStyle>("ColorStyle", ColorStyle);
        //cfg.SetEnum<PrintGridRowSpacing>("RowSpacing", RowSpacing);
        if (UseBoolMode)
        {
          cfg.SetEnum<BRDataViewBooleanMode>("BoolMode", BooleanMode);
          cfg.SetString("BoolTextTrue", BooleanTextTrue);
          cfg.SetString("BoolTextFalse", BooleanTextFalse);
        }
        cfg.SetInt32("CellLeftMargin", CellLeftMargin);
        cfg.SetInt32("CellTopMargin", CellTopMargin);
        cfg.SetInt32("CellRightMargin", CellRightMargin);
        cfg.SetInt32("CellBottomMargin", CellBottomMargin);

        #endregion

        #region Отправить

        cfg.SetEnum<EFPDataViewExpRange>("ExpRange", ExpRange);
        // Не используем свойства UseXXX
        cfg.SetBoolean("ExpTableHeader", ExpTableHeader);
        cfg.SetBoolean("ExpTableFilters", ExpTableFilters);
        cfg.SetBoolean("ExpColumnHeaders", ExpColumnHeaders);

        #endregion

        #region Экспорт в текстовый файл

        cfg.SetInt32("CodePage", CodePage);
        cfg.SetString("FieldDelimiter", FieldDelimiterStr);
        cfg.SetString("Quote", QuoteStr);
        cfg.SetBoolean("SingleLineField", SingleLineField);
        cfg.SetBoolean("RemoveDoubleSpaces", RemoveDoubleSpaces);

        #endregion

        #region Экспорт в DBF

        cfg.SetInt32("DbfCodePage", DbfCodePage);
        // Остальные параметры относятся к столбцам

        #endregion
      }
    }

    /// <summary>
    /// Прочитать данные из секции конфигурации.
    /// </summary>
    /// <param name="cfg">Секция с данными</param>
    /// <param name="part">Вариант хранения данных</param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        #region Столбцы

        _ColumnDict.Clear();
        CfgPart cfg2 = cfg.GetChild("Columns", false);
        if (cfg2 != null)
        {
          foreach (string colName in cfg2.GetChildNames())
          {
            CfgPart cfg3 = cfg2.GetChild(colName, false);
            ColumnInfo ci = new ColumnInfo();
            ci.Print = cfg3.GetBooleanDef("Print", true);
            if (ci.Print)
            {
              ci.PrintWidth = cfg3.GetInt32("Width");
              ci.AutoGrow = cfg3.GetBoolean("AutoGrow");
            }

            ci.DbfExported = cfg3.GetBooleanDef("Dbf", true);
            ci.DbfFieldName = cfg3.GetString("DbfName");
            if (String.IsNullOrEmpty(ci.DbfFieldName))
              ci.DbfFieldName = cfg3.GetString("AutoDbfName");
            else
              ci.IsManualDbfName = true;

            _ColumnDict.Add(colName, ci);
          }
        }

        _SizeGroupDict.Clear();
        cfg2 = cfg.GetChild("ColumnSizeGroups", false);
        if (cfg2 != null)
        {
          foreach (string grpName in cfg2.GetChildNames())
          {
            CfgPart cfg3 = cfg2.GetChild(grpName, false);
            SizeGroupInfo szi = new SizeGroupInfo();
            szi.PrintWidth = cfg3.GetInt32("Width");
            szi.AutoGrow = cfg3.GetBoolean("AutoGrow");
          }
        }

        RepeatedColumnCount = cfg.GetInt32("RepeatedColumnCount");
        ColumnSubHeaderNumbers = cfg.GetEnum<BRDataViewColumnSubHeaderNumbersMode>("ColumnSubHeaderNumbers");

        #endregion

        #region Оформление

        BorderStyle = cfg.GetEnumDef<BRDataViewBorderStyle>("BorderStyle", BRDataViewBorderStyle.All);
        if (UseColorStyle)
          ColorStyle = cfg.GetEnumDef<BRDataViewColorStyle>("ColorStyle", BRDataViewColorStyle.NoColors);
        //RowSpacing = cfg.GetEnum<PrintGridRowSpacing>("RowSpacing");
        if (UseBoolMode)
        {
          BooleanMode = cfg.GetEnumDef<BRDataViewBooleanMode>("BoolMode", BRDataViewBooleanMode.Text);
          BooleanTextTrue = cfg.GetStringDef("BoolTextTrue", "[X]");
          BooleanTextFalse = cfg.GetStringDef("BoolTextFalse", "[ ]");
        }
        CellLeftMargin = cfg.GetInt32Def("CellLeftMargin", BRReport.AppDefaultCellStyle.LeftMargin);
        CellTopMargin = cfg.GetInt32Def("CellTopMargin", BRReport.AppDefaultCellStyle.TopMargin);
        CellRightMargin = cfg.GetInt32Def("CellRightMargin", BRReport.AppDefaultCellStyle.RightMargin);
        CellBottomMargin = cfg.GetInt32Def("CellBottomMargin", BRReport.AppDefaultCellStyle.BottomMargin);

        #endregion

        #region Отправить

        ExpRange = cfg.GetEnumDef<EFPDataViewExpRange>("ExpRange", EFPDataViewExpRange.All);
        // Не используем свойства UseXXX
        ExpTableHeader = cfg.GetBooleanDef("ExpTableHeader", true);
        ExpTableFilters = cfg.GetBooleanDef("ExpTableFilters", true);
        ExpColumnHeaders = cfg.GetBooleanDef("ExpColumnHeaders", true);

        #endregion

        #region Экспорт в текстовый файл

        CodePage = cfg.GetInt32Def("CodePage", Encoding.UTF8.CodePage);
        FieldDelimiterStr = cfg.GetStringDef("FieldDelimiter", ",");
        QuoteStr = cfg.GetStringDef("Quote", "\"");
        SingleLineField = cfg.GetBooleanDef("SingleLineField", true);
        RemoveDoubleSpaces = cfg.GetBooleanDef("RemoveDoubleSpaces", false);

        #endregion

        #region Экспорт в DBF

        DbfCodePage = cfg.GetInt32Def("DbfCodePage", Encoding.Default.CodePage);
        // Остальные параметры относятся к столбцам

        #endregion
      }
    }

    #endregion

    #region Clone()

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Новый объект</returns>
    public BRDataViewSettingsDataItem Clone()
    {
      BRDataViewSettingsDataItem res = new BRDataViewSettingsDataItem();
      TempCfg cfg = new TempCfg();
      this.WriteConfig(cfg, Config.SettingsPart.User);
      res.ReadConfig(cfg, Config.SettingsPart.User);
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }
}
