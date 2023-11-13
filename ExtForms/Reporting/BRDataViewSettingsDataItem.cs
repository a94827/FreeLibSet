using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Config;
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
    NoBorders,

    /// <summary>
    /// Заголовки столбцов обведены рамкой, других линий нет
    /// </summary>
    OnlyHeaders,

    /// <summary>
    /// Столбцы разделены вертикальными линиями, между строками линий нет
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
  public enum BRDataViewBoolMode
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
  /// Данные параметров страницы для печати EFPDataGridView и EFPDataTreeView.
  /// Содержит список выбранных столбцов для печати, их размеры и параметры для оформления.
  /// Дополняет параметры страницы BRPageSetup и параметры шрифта BRFontSettings
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

      _ColumnWidthDict = new Dictionary<string, int>();
      _GroupWidthDict = new Dictionary<string, int>();
      _ColumnSubHeaderNumbers = BRDataViewColumnSubHeaderNumbersMode.None;
      _RepeatedColumnCount = 0;

      #endregion

      #region Оформление

      _UseColorStyle = true;
      _UseBoolMode = true;

      _BoolMode = BRDataViewBoolMode.Text;
      _BoolTextTrue = "[X]";
      _BoolTextFalse = "[ ]";
      _ColorStyle = BRDataViewColorStyle.NoColors;
      _BorderStyle = BRDataViewBorderStyle.All;

      _CellLeftMargin = BRReport.AppDefaultCellStyle.LeftMargin;
      _CellTopMargin = BRReport.AppDefaultCellStyle.TopMargin;
      _CellRightMargin = BRReport.AppDefaultCellStyle.RightMargin;
      _CellBottomMargin = BRReport.AppDefaultCellStyle.BottomMargin;

      #endregion

      #region Отправить

      _ExpRange = EFPDataViewExpRange.All;
      _UseExpColumnHeaders = true;
      _ExpColumnHeaders = true;

      #endregion
    }

    #endregion

    #region Вкладка "Столбцы"

    const int AutoWidth = 0;

    const int NotPrintedWidth = Int32.MinValue;

    /// <summary>
    /// Ширина столбцов или константы AutoWidth и NonPrintWidth
    /// Ключом является имя столбца.
    /// </summary>
    private readonly Dictionary<string, int> _ColumnWidthDict;

    /// <summary>
    /// Ширина для SizeGroup.
    /// Ключ - код группы. Значение - ширина или константа AutoWidth
    /// </summary>
    private readonly Dictionary<string, int> _GroupWidthDict;

    /// <summary>
    /// Получить флажок печати столбца
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <returns>Признак печати</returns>
    public bool GetColumnPrinted(IEFPDataViewColumnBase column)
    {
      if (!column.Printable)
        return false;

      int w;
      _ColumnWidthDict.TryGetValue(column.Name, out w);
      return w != NotPrintedWidth;
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

      if (value)
      {
        int w;
        _ColumnWidthDict.TryGetValue(column.Name, out w);
        if (w == NotPrintedWidth)
          w = AutoWidth;
        _ColumnWidthDict[column.Name] = w;
      }
      else
        _ColumnWidthDict[column.Name] = NotPrintedWidth;
    }

    /// <summary>
    /// Получить ширину столбца в единицах 0.1мм.
    /// Возвращает 0, если ширина определяется автоматически.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <returns>Ширина или 0</returns>
    public int GetColumnWidth(IEFPDataViewColumnBase column)
    {
      if (!column.Printable)
        return 0;
      int w;
      if (String.IsNullOrEmpty(column.SizeGroup))
        _ColumnWidthDict.TryGetValue(column.Name, out w);
      else
        _GroupWidthDict.TryGetValue(column.SizeGroup, out w);

      if (w == NotPrintedWidth)
        w = AutoWidth;
      return Math.Abs(w);
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
      int w = GetColumnWidth(column);
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
    public void SetColumnWidth(IEFPDataViewColumnBase column, int value)
    {
      if (value < 0)
        throw new ArgumentOutOfRangeException("value");
      if (!column.Printable)
        return;

      bool isAutoGrow = GetColumnAutoGrow(column);

      if (isAutoGrow)
        value = -value;

      if (String.IsNullOrEmpty(column.SizeGroup))
        _ColumnWidthDict[column.Name] = value;
      else
        _GroupWidthDict[column.SizeGroup] = value;
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
    /// Если true, то <see cref="SetColumnWidth(IEFPDataViewColumnBase, int)"/> задает минимальную ширину столбца.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <returns>Признак автоматического увеличения ширины</returns>
    public bool GetColumnAutoGrow(IEFPDataViewColumnBase column)
    {
      if (!GetColumnPrinted(column))
        return false;
      int w;
      bool res;
      if (String.IsNullOrEmpty(column.SizeGroup))
        res = _ColumnWidthDict.TryGetValue(column.Name, out w);
      else
        res = _GroupWidthDict.TryGetValue(column.SizeGroup, out w);

      if (res)
        return w < 0;
      else
        return column.AutoGrow;
    }

    /// <summary>
    /// Установить признак автоматического увеличения ширины столбца при печати для заполнения ширины столбца.
    /// Если true, то <see cref="SetColumnWidth(IEFPDataViewColumnBase, int)"/> задает минимальную ширину столбца.
    /// </summary>
    /// <param name="column">Столбец провайдера табличного или иерархического просмотра (<see cref="EFPDataGridViewColumn"/> или <see cref="EFPDataTreeViewColumn"/>)</param>
    /// <param name="value">Признак автоматического увеличения ширины</param>
    public void SetColumnAutoGrow(IEFPDataViewColumnBase column, bool value)
    {
      if (!GetColumnPrinted(column))
        return;
      int w = GetColumnWidth(column);

      if (value)
        w = -w;

      if (String.IsNullOrEmpty(column.SizeGroup))
        _ColumnWidthDict[column.Name] = w;
      else
        _GroupWidthDict[column.SizeGroup] = w;
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
    /// По умолчанию - <see cref="BRDataViewBoolMode.Text"/>.
    /// </summary>
    public BRDataViewBoolMode BoolMode { get { return _BoolMode; } set { _BoolMode = value; } }
    private BRDataViewBoolMode _BoolMode;

    /// <summary>
    /// Замещающий текст для логического значения true при <see cref="BoolMode"/>=Text.
    /// По умолчанию - "[X]".
    /// </summary>
    public string BoolTextTrue { get { return _BoolTextTrue; } set { _BoolTextTrue = value; } }
    private string _BoolTextTrue;

    /// <summary>
    /// Замещающий текст для логического значения false при <see cref="BoolMode"/>=Text.
    /// По умолчанию - "[ ]".
    /// </summary>
    public string BoolTextFalse { get { return _BoolTextFalse; } set { _BoolTextFalse = value; } }
    private string _BoolTextFalse;

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
    /// Наличие свойства <see cref="ExpColumnHeaders"/>.
    /// Устанавливается в true, если просмотр содержит заголовки столбцов
    /// </summary>
    public bool UseExpColumnHeaders { get { return _UseExpColumnHeaders; } set { _UseExpColumnHeaders = value; } }
    private bool _UseExpColumnHeaders;

    #endregion

    /// <summary>
    /// При экспорте в текстовые форматы: какой диапазон использовать: весь просмотр (по умолчанию) или только выбранные ячейки
    /// </summary>
    public EFPDataViewExpRange ExpRange { get { return _ExpRange; } set { _ExpRange = value; } }
    private EFPDataViewExpRange _ExpRange;

    /// <summary>
    /// При экпорте в текстовые форматы: true (по умолчанию) - выводить заголовки столбцов
    /// </summary>
    public bool ExpColumnHeaders { get { return _ExpColumnHeaders; } set { _ExpColumnHeaders = value; } }
    private bool _ExpColumnHeaders;

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
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      #region Столбцы

      CfgPart cfg2 = cfg.GetChild("Columns", true);
      // Нельзя чистить, так как затрутся данные из родительской настройки
      // cfg2.Clear();
      foreach (KeyValuePair<string, int> pair in _ColumnWidthDict)
      {
        CfgPart cfg3 = cfg2.GetChild(pair.Key, true);
        cfg3.Clear();
        if (pair.Value == NotPrintedWidth)
          cfg3.SetBool("Print", false);
        else if (pair.Value > 0)
          cfg3.SetInt("Width", pair.Value);
        else if (pair.Value < 0)
        {
          cfg3.SetInt("Width", -pair.Value);
          cfg3.SetBool("AutoGrow", true);
        }
      }
      if (_GroupWidthDict.Count > 0)
      {
        cfg2 = cfg.GetChild("ColumnSizeGroups", true);
        foreach (KeyValuePair<string, int> pair in _GroupWidthDict)
        {
          CfgPart cfg3 = cfg2.GetChild(pair.Key, true);
          cfg3.Clear();
          if (pair.Value > 0)
            cfg3.SetInt("Width", pair.Value);
          else if (pair.Value < 0)
          {
            cfg3.SetInt("Width", -pair.Value);
            cfg3.SetBool("AutoGrow", true);
          }
        }
      }
      cfg.SetInt("RepeatedColumnCount", RepeatedColumnCount);
      cfg.SetEnum<BRDataViewColumnSubHeaderNumbersMode>("ColumnSubHeaderNumbers", ColumnSubHeaderNumbers);

      #endregion

      #region Оформление

      cfg.SetEnum<BRDataViewBorderStyle>("BorderStyle", BorderStyle);
      if (UseColorStyle)
        cfg.SetEnum<BRDataViewColorStyle>("ColorStyle", ColorStyle);
      //cfg.SetEnum<PrintGridRowSpacing>("RowSpacing", RowSpacing);
      if (UseBoolMode)
      {
        cfg.SetEnum<BRDataViewBoolMode>("BoolMode", BoolMode);
        cfg.SetString("BoolTextTrue", BoolTextTrue);
        cfg.SetString("BoolTextFalse", BoolTextFalse);
      }
      cfg.SetInt("CellLeftMargin", CellLeftMargin);
      cfg.SetInt("CellTopMargin", CellTopMargin);
      cfg.SetInt("CellRightMargin", CellRightMargin);
      cfg.SetInt("CellBottomMargin", CellBottomMargin);

      #endregion

      #region Отправить

      cfg.SetEnum<EFPDataViewExpRange>("ExpRange", ExpRange);
      cfg.SetBool("ExpColumnHeaders", ExpColumnHeaders);

      #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      #region Столбцы

      _ColumnWidthDict.Clear();
      CfgPart cfg2 = cfg.GetChild("Columns", false);
      if (cfg2 != null)
      {
        foreach (string colName in cfg2.GetChildNames())
        {
          CfgPart cfg3 = cfg2.GetChild(colName, false);
          if (cfg3.GetBoolDef("Print", true))
          {
            if (cfg3.GetBool("AutoGrow"))
              _ColumnWidthDict.Add(colName, -cfg3.GetInt("Width"));
            else
              _ColumnWidthDict.Add(colName, cfg3.GetInt("Width"));
          }
          else
            _ColumnWidthDict.Add(colName, NotPrintedWidth);
        }
      }
      _GroupWidthDict.Clear();
      cfg2 = cfg.GetChild("ColumnSizeGroups", false);
      if (cfg2 != null)
      {
        foreach (string grpName in cfg2.GetChildNames())
        {
          CfgPart cfg3 = cfg2.GetChild(grpName, false);
          if (cfg3.GetBool("AutoGrow"))
            _GroupWidthDict.Add(grpName, -cfg3.GetInt("Width"));
          else
            _GroupWidthDict.Add(grpName, cfg3.GetInt("Width"));
        }
      }
      RepeatedColumnCount = cfg.GetInt("RepeatedColumnCount");
      ColumnSubHeaderNumbers = cfg.GetEnum<BRDataViewColumnSubHeaderNumbersMode>("ColumnSubHeaderNumbers");

      #endregion

      #region Оформление

      BorderStyle = cfg.GetEnumDef<BRDataViewBorderStyle>("BorderStyle", BRDataViewBorderStyle.All);
      if (UseColorStyle)
        ColorStyle = cfg.GetEnumDef<BRDataViewColorStyle>("ColorStyle", BRDataViewColorStyle.NoColors);
      //RowSpacing = cfg.GetEnum<PrintGridRowSpacing>("RowSpacing");
      if (UseBoolMode)
      {
        BoolMode = cfg.GetEnumDef<BRDataViewBoolMode>("BoolMode", BRDataViewBoolMode.Text);
        BoolTextTrue = cfg.GetStringDef("BoolTextTrue", "[X]");
        BoolTextFalse = cfg.GetStringDef("BoolTextFalse", "[ ]");
      }
      CellLeftMargin = cfg.GetIntDef("CellLeftMargin", BRReport.AppDefaultCellStyle.LeftMargin);
      CellTopMargin = cfg.GetIntDef("CellTopMargin", BRReport.AppDefaultCellStyle.TopMargin);
      CellRightMargin = cfg.GetIntDef("CellRightMargin", BRReport.AppDefaultCellStyle.RightMargin);
      CellBottomMargin = cfg.GetIntDef("CellBottomMargin", BRReport.AppDefaultCellStyle.BottomMargin);

      #endregion

      #region Отправить

      ExpRange = cfg.GetEnumDef<EFPDataViewExpRange>("ExpRange", EFPDataViewExpRange.All);
      ExpColumnHeaders = cfg.GetBoolDef("ExpColumnHeaders", true);

      #endregion
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
