// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Formatting;
using FreeLibSet.Data;
using FreeLibSet.Calendar;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация свойства EFPGridProducer.ToolTips
  /// </summary>
  public class EFPGridProducerToolTips : NamedList<EFPGridProducerToolTip>
  {
    #region Методы добавления

    /// <summary>
    /// Добавить строку подсказки с содержимым одного поля
    /// </summary>
    /// <param name="columnName">Поле, значение которого выводится</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddText(string columnName, string prefixText)
    {
      EFPGridProducerToolTip item = new EFPGridProducerToolTip(columnName);
      InitPrefixText(item, prefixText);
      Add(item);
      return item;
    }

    private static void InitPrefixText(EFPGridProducerToolTip item, string prefixText)
    {
      if (prefixText != null)
      {
        if (prefixText.Length == 0)
          item.PrefixText = "";
        else
        {
          item.PrefixText = prefixText + ": ";
          item.DisplayName = prefixText;
        }
      }
    }

    /// <summary>
    /// Добавить строку подсказки с содержимым одного поля типа даты
    /// </summary>
    /// <param name="columnName">Поле, значение которого выводится</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddDate(string columnName, string prefixText)
    {
      return AddDateTime(columnName, prefixText, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавить строку подсказки с содержимым одного поля типа даты или даты/времени
    /// </summary>
    /// <param name="columnName">Поле, значение которого выводится</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <param name="format">Формат вывода даты, например, "d" или "G"</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddDate(string columnName, string prefixText, string format)
    {
      EFPGridProducerToolTip item = new EFPGridProducerToolTip(columnName);
      InitPrefixText(item, prefixText);

      if (format != null)
        item.Format = format;

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить строку подсказки с содержимым одного поля типа даты/времени
    /// </summary>
    /// <param name="columnName">Поле, значение которого выводится</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddDateTime(string columnName, string prefixText)
    {
      return AddDateTime(columnName, prefixText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавить строку подсказки с содержимым одного поля типа даты и/или времени
    /// </summary>
    /// <param name="columnName">Поле, значение которого выводится</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <param name="kind">Используемый формат даты/времени</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddDateTime(string columnName, string prefixText, EditableDateTimeFormatterKind kind)
    {
      EditableDateTimeFormatter formatter = EditableDateTimeFormatters.Get(kind);

      EFPGridProducerToolTip item = new EFPGridProducerToolTip(columnName);
      InitPrefixText(item, prefixText);
      item.Format = formatter.Format;
      item.FormatProvider = formatter.FormatProvider;

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить строку подсказки с содержимым двух полей типа "дата" для задания диапазона
    /// </summary>
    /// <param name="firstColumnName">Имя поля начальной даты</param>
    /// <param name="lastColumnName">Имя поля конечной даты</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "PrefixText : Диапазон".
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddDateRange(string firstColumnName, string lastColumnName, string prefixText)
    {
      string name = firstColumnName + "_" + lastColumnName;
      EFPGridProducerToolTip item = new EFPGridProducerToolTip(name, new string[] { firstColumnName, lastColumnName });
      InitPrefixText(item, prefixText);
      item.EmptyValue = DateRangeFormatter.Default.ToString(null, null, true);
      item.ValueNeeded += EFPGridProducerColumns.DateRangeColumn_LongValueNeeded;
      Add(item);
      return item;
    }


    /// <summary>
    /// Добавляет подсказку для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Добавляемая подсказка имеет имя "<paramref name="sourceColumnName"/>_Text".
    /// Для показа текста "нерегулярных" перечислений используйте AddUserItem().
    /// </summary>
    /// <param name="sourceColumnName">Имя целочисленного столбца, содержащего исходное значение</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <param name="textValues">Список текстовых значений, которые показываются в подсказке</param>
    /// <returns>Описание подсказки</returns>
    public EFPGridProducerToolTip AddEnumText(string sourceColumnName, string prefixText,
      string[] textValues)
    {
      if (textValues == null)
        throw new ArgumentNullException("textValues");

      EnumToolTipInfo tti = new EnumToolTipInfo();
      tti.TextValues = textValues;

      return AddUserItem(sourceColumnName + "_Text", sourceColumnName, new EFPGridProducerValueNeededEventHandler(tti.ValueNeeded), prefixText);
    }

    private class EnumToolTipInfo
    {
      #region Поля

      public string[] TextValues;

      #endregion

      #region Обработчик

      public void ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        int SrcVal = args.GetInt(0);
        if (SrcVal < 0 || SrcVal >= TextValues.Length)
          args.Value = "?? " + SrcVal.ToString();
        else
          args.Value = TextValues[SrcVal];
      }

      #endregion
    }


    /// <summary>
    /// Добавление подсказки, формируемой пользовательским обработчиком
    /// </summary>
    /// <param name="name">Имя подсказки (для сохранения конфигурации просмотра)</param>
    /// <param name="sourceColumnNames">Имена полей, значения которых используются для формирования подсказки.
    /// Список имен разделяется запятыми</param>
    /// <param name="handler">Обработчик, формирующий текст подсказки</param>
    /// <returns>Созданный объект подсказки</returns>
    public EFPGridProducerToolTip AddUserItem(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler handler)
    {
      string[] aNames = sourceColumnNames.Split(',');
      EFPGridProducerToolTip Item = new EFPGridProducerToolTip(name, aNames);
      Item.ValueNeeded += handler;
      Add(Item);
      return Item;
    }

    /// <summary>
    /// Добавление подсказки, формируемой пользовательским обработчиком
    /// </summary>
    /// <param name="name">Имя подсказки (для сохранения конфигурации просмотра)</param>
    /// <param name="sourceColumnNames">Имена полей, значения которых используются для формирования подсказки.
    /// Список имен разделяется запятыми</param>
    /// <param name="handler">Обработчик, формирующий текст подсказки</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "PrefixText : Диапазон".
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Созданный объект подсказки</returns>
    public EFPGridProducerToolTip AddUserItem(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler handler, string prefixText)
    {
      EFPGridProducerToolTip Item = AddUserItem(name, sourceColumnNames, handler);
      InitPrefixText(Item, prefixText);
      return Item;
    }


    /// <summary>
    /// Последняя добавленная подсказка.
    /// Вспомогательное свойство, которое можно использовать в процессе добавления подсказок
    /// </summary>
    public EFPGridProducerToolTip LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

    #endregion

    #region Получение текста подсказки

    /// <summary>
    /// Разделитель, который следует выводить между всплывающей подсказкой по ячейке и всплывающей подсказкой по строке таблицы.
    /// Содержит перевод строки, символы "---" и еще один перевод строки
    /// </summary>
    public static readonly string ToolTipTextSeparator = Environment.NewLine + new string('-', 32) + Environment.NewLine;

    /// <summary>
    /// Возвращает текст высплывающей подсказки
    /// </summary>
    /// <param name="gridConfig">Используемая конфигурация табличного просмотра. Используется для определения списка подсказок, выбранных пользователем</param>
    /// <param name="rowInfo">Данные строки табличного просмотра</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(EFPDataGridViewConfig gridConfig, EFPDataViewRowInfo rowInfo)
    {
      if (gridConfig == null)
        throw new ArgumentNullException("grifConfig");

      if (gridConfig.ToolTips.Count == 0)
        return String.Empty;

      List<string> lst = null;
      for (int i = 0; i < gridConfig.ToolTips.Count; i++)
      {
        EFPGridProducerToolTip ToolTip = this[gridConfig.ToolTips[i].ToolTipName];
        if (ToolTip == null)
          continue; // ерунда какая-то

        //// Если в подсказки входит столбец, на который наведена мышь, то пропускаем подсказку
        //List<string> lst1 = new List<string>();
        //ToolTip.GetColumnNames(lst1);
        //if (lst1.Contains(args.ColumnName))
        //  continue;

        string s;
        try
        {
          s = ToolTip.GetToolTipText(rowInfo);
        }
        catch (Exception e)
        {
          s = ToolTip.DisplayName + ": Ошибка! " + e.Message;
        }
        if (!String.IsNullOrEmpty(s))
        {
          if (lst == null)
            lst = new List<string>();
          lst.Add(s);
        }
      }

      if (lst == null)
        return String.Empty;
      else
        return String.Join(Environment.NewLine, lst.ToArray());
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает имена подсказок через запятую
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = "";
      for (int i = 0; i < Count; i++)
      {
        if (i > 0)
          s += ", ";
        s += this[i].Name;
      }
      return "{" + s + "}";
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Описание одного возможного элемента ToolTip.
  /// </summary>
  public class EFPGridProducerToolTip : EFPGridProducerItemBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает подсказку, выводящую текст для заданного поля
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public EFPGridProducerToolTip(string columnName)
      : this(columnName, null)
    {
    }

    /// <summary>
    /// Создает подсказку, выводящую текст на основании нескольких исходных полей с помощью пользовательского обработчика
    /// </summary>
    /// <param name="name">Условное имя подсказки</param>
    /// <param name="sourceColumnNames">Имена исходных столбцов. Если null, то подсказка не использует пользовательский
    /// обработчик, а выводит значение поля <paramref name="name"/>.</param>
    public EFPGridProducerToolTip(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      _Format = String.Empty;
      _PrefixText = String.Empty;
    }

    #endregion

    #region Получение текста

    /// <summary>
    /// Текст, который будет идти перед значением поля.
    /// Текст, если он задан, должен содержать в конце ": ", дополнительные символы не добавляются.
    /// </summary>
    public string PrefixText
    {
      get { return _PrefixText; }
      set
      {
        if (value == null)
          _PrefixText = String.Empty;
        else
          _PrefixText = value;
      }
    }
    private string _PrefixText;

    /// <summary>
    /// Форматирование данных при отображении.
    /// Используется, если значение, возвращаемое методом GetValue(), реализует интерфейс IFormattable
    /// </summary>
    public string Format
    {
      get { return _Format; }
      set
      {
        if (value == null)
          _Format = String.Empty;
        else
          _Format = value;
      }
    }
    private string _Format;

    /// <summary>
    /// Форматирование данных при отображении.
    /// Используется, если значение, возвращаемое методом GetValue(), реализует интерфейс IFormattable
    /// </summary>
    public IFormatProvider FormatProvider { get { return _FormatProvider; } set { _FormatProvider = value; } }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Основной метод - получение текста всплывающей подсказки для строки табличного просмотра.
    /// Вызывается событие ValueNeeded.
    /// Если обработчик установил свойство ToolTipText, то возвращается этот текст.
    /// Иначе проверяется свойство Value. Если значение свойства реализует интерфейс IFormattable, вызывается метод
    /// ToString(), с использование свойств Format и FormatProvider.
    /// К полученному тексту слева добавляется PrefixText.
    /// </summary>
    /// <param name="rowInfo">Информация о строке</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(EFPDataViewRowInfo rowInfo)
    {
      object value;
      string toolTipText;

      base.DoGetValue(EFPGridProducerValueReason.ToolTipText, rowInfo, out value, out toolTipText);
      if (toolTipText != null)
        return toolTipText;

      string s;
      if (value == null)
        s = String.Empty;
      else if (value is IFormattable)
        s = ((IFormattable)value).ToString(Format, FormatProvider);
      else
        s = value.ToString();
      return PrefixText + s;
    }

    #endregion
  }
  /// <summary>
  /// Объект для извлечения всплывающих подсказок для строки, 
  /// отдельно от табличного просмотра
  /// </summary>
  public class GridProducerToolTipExtractor
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="producer">Генератор табличного просмотра</param>
    /// <param name="toolTips">Список подсказок, которые требуется извлекать</param>
    public GridProducerToolTipExtractor(EFPGridProducer producer, IEnumerable<EFPGridProducerToolTip> toolTips)
    {
#if DEBUG
      if (producer == null)
        throw new ArgumentNullException("producer");
      if (toolTips == null)
        throw new ArgumentNullException("toolTips");
#endif

      _Producer = producer;
      _ToolTips = toolTips;

      _RVA = new DataRowValueArray();
      _SB = new StringBuilder();
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="producer">Генератор табличного просмотра</param>
    /// <param name="config">Настройка табличного просмотра</param>
    public GridProducerToolTipExtractor(EFPGridProducer producer, EFPDataGridViewConfig config)
      : this(producer, GetToolTips(producer, config))
    {
    }

    private static IEnumerable<EFPGridProducerToolTip> GetToolTips(EFPGridProducer producer, EFPDataGridViewConfig config)
    {
      List<EFPGridProducerToolTip> ToolTips = new List<EFPGridProducerToolTip>();
      for (int i = 0; i < config.ToolTips.Count; i++)
      {
        EFPGridProducerToolTip ToolTip = producer.ToolTips[config.ToolTips[i].ToolTipName];
        if (ToolTip != null)
          ToolTips.Add(ToolTip);
      }
      return ToolTips;
    }

    /// <summary>
    /// Вызывает GridProducer.LoadConfig() и создает объект
    /// </summary>
    /// <param name="producer">Генератор табличного просмотра</param>
    /// <param name="configSectionName">Имя секции конфигурации, используемое табличным просмотром</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки, используемой табличным просмотром.
    /// Пустая строка (обычно), если используется настройка просмота GridProducer.DefaultConfig</param>
    /// <param name="cfgName">Имя сохраненной секции. В текущей реализации должно быть пустой строкой</param>
    public GridProducerToolTipExtractor(EFPGridProducer producer, string configSectionName, string defaultConfigName, string cfgName)
      : this(producer, producer.LoadConfig(configSectionName, defaultConfigName, cfgName))
    {
    }

    /// <summary>
    /// Вызывает GridProducer.LoadConfig() и создает объект
    /// </summary>
    /// <param name="producer">Генератор табличного просмотра</param>
    /// <param name="configSectionName">Имя секции конфигурации, используемое табличным просмотром</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки, используемой табличным просмотром.
    /// Пустая строка (обычно), если используется настройка просмота GridProducer.DefaultConfig</param>
    public GridProducerToolTipExtractor(EFPGridProducer producer, string configSectionName, string defaultConfigName)
      : this(producer, producer.LoadConfig(configSectionName, defaultConfigName))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-источник
    /// </summary>
    public EFPGridProducer Producer { get { return _Producer; } }
    private EFPGridProducer _Producer;

    /// <summary>
    /// Список используемых компонентов всплывающих подсказок
    /// Задается в конструкторе
    /// </summary>
    public IEnumerable<EFPGridProducerToolTip> ToolTips { get { return _ToolTips; } }
    private IEnumerable<EFPGridProducerToolTip> _ToolTips;

    /// <summary>
    /// Возвращает true, если список подсказок пустой
    /// </summary>
    public bool IsEmpty { get { return _ToolTips == null; } }

    #endregion

    #region Получение подсказки

    private DataRowValueArray _RVA;
    private StringBuilder _SB;

    /// <summary>
    /// Основной метод получения подсказки.
    /// Возвращает строку, содержащую тексты подсказок, разделенные символами CR+LF
    /// Исходные данные должны быть переданы в виде строки данных Row. Строка должна
    /// содержать все поля, необходимые для создания подсказки
    /// </summary>
    /// <param name="row">Строка исходных данных</param>
    /// <returns>Текст подсказки</returns>
    public string GetToolTipText(DataRow row)
    {
      if (row == null)
        throw new ArgumentNullException("row");

      _RVA.CurrentRow = row;
      _SB.Length = 0;

      EFPDataViewRowInfo ri = new EFPDataViewRowInfo(null, row, _RVA, -1);

      foreach (EFPGridProducerToolTip ToolTip in ToolTips)
      {
        try
        {
          string s = ToolTip.GetToolTipText(ri);
          if (String.IsNullOrEmpty(s))
            continue;
          if (_SB.Length > 0)
            _SB.Append(Environment.NewLine);
          _SB.Append(s);
        }
        catch
        {
        }
      }
      return _SB.ToString();
    }

    #endregion
  }
}
