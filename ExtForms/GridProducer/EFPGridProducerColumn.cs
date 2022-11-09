// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Collections.Specialized;
using System.Windows.Forms;
using FreeLibSet.Formatting;
using FreeLibSet.DBF;
using System.Data;
using System.Globalization;
using FreeLibSet.Collections;
using FreeLibSet.Controls;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация свойства EFPGridProducer.Columns
  /// </summary>
  public class EFPGridProducerColumns : NamedList<EFPGridProducerColumn>
  {
    #region Свойства

    /// <summary>
    /// Возвращает последний добавленный столбнц
    /// </summary>
    public EFPGridProducerColumn LastAdded
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

    #region Методы добавления столбцов

    #region Текстовые столбцы

    /// <summary>
    /// Добавляет текстовый столбец
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddText(string columnName, string headerText, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Left;
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавляет вычисляемый текстовый столбец
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserText(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, int textWidth, int minTextWidth)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Left;
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      item.ValueNeeded += valueNeeded;
      item.DataType = typeof(string);

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить столбец для отображения числового поля.
    /// Значения должны быть целочисленными.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddInt(string columnName, string headerText, int textWidth)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      item.MinTextWidth = 1;
      item.Format = "0";
      item.DataType = typeof(int);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить вычисляемый столбец для отображения чисел.
    /// Значения должны быть целочисленными.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserInt(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, int textWidth)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      item.MinTextWidth = 1;
      item.Format = "0";
      item.DataType = typeof(int);

      Add(item);
      return item;
    }


    /// <summary>
    /// Добавить столбец для отображения дробных (или целых) чисел.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="decimalPlaces">Количество знаков после десятичной точки. 0 - для отображения целых чисел</param>
    /// <param name="sizeGroup">Группа однотипных столбцов</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddFixedPoint(string columnName, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      if (decimalPlaces < 0)
        throw new ArgumentException("Количество знаков после запятой не может быть отрицательным", "decimalPlaces");

      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      if (decimalPlaces > 0)
      {
        item.Format = "0." + new string('0', decimalPlaces);
        item.MinTextWidth = decimalPlaces + 2; // место для точки
      }
      else
      {
        item.Format = "0";
        item.MinTextWidth = 1;
      }
      item.SizeGroup = sizeGroup; // 25.12.2020
      item.DataType = typeof(double);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить вычисляемый столбец для отображения дробных (или целых) чисел.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="decimalPlaces">Количество знаков после десятичной точки. 0 - для отображения целых чисел</param>
    /// <param name="sizeGroup">Группа однотипных столбцов</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserFixedPoint(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      if (decimalPlaces > 0)
      {
        item.Format = "0." + new string('0', decimalPlaces);
        item.MinTextWidth = decimalPlaces + 2; // место для точки
      }
      else
      {
        item.Format = "0";
        item.MinTextWidth = 1;
      }
      item.DataType = typeof(double);

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить столбец для отображения даты (без компонента времени)
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDate(string columnName, string headerText)
    {
      return AddDateTime(columnName, headerText, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавить столбец для отображения даты (без компонента времени).
    /// Заголовок столбца равен <paramref name="columnName"/>
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDate(string columnName)
    {
      return AddDateTime(columnName, columnName, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавить вычисляемый столбец для отображения даты (без компонента времени)
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserDate(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      return AddUserDateTime(name, sourceColumnNames, valueNeeded, headerText, EditableDateTimeFormatterKind.Date);
    }


    /// <summary>
    /// Добавить столбец для отображения даты и времени
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDateTime(string columnName, string headerText)
    {
      return AddDateTime(columnName, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавить столбец для отображения даты и/или времени
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="kind">Тип даты/времени</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDateTime(string columnName, string headerText, EditableDateTimeFormatterKind kind)
    {
      EditableDateTimeFormatter formatter = EditableDateTimeFormatters.Get(kind);

      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Center;
      item.TextWidth = formatter.TextWidth;
      item.MinTextWidth = formatter.TextWidth;
      item.SizeGroup = kind.ToString();
      item.Format = formatter.Format;
      item.FormatProvider = formatter.FormatProvider;
      item.MaskProvider = formatter.MaskProvider;
      //Item.CanIncSearch = true;
      item.DataType = typeof(DateTime);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить столбец для отображения даты и времени.
    /// Заголовок столбца равен <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDateTime(string columnName)
    {
      return AddDateTime(columnName, columnName, EditableDateTimeFormatterKind.DateTime);
    }


    /// <summary>
    /// Добавить вычисляемый столбец для отображения даты и времени
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserDateTime(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      return AddUserDateTime(name, sourceColumnNames, valueNeeded, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавить вычисляемый столбец для отображения даты и/или времени
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="kind">Формат даты/времени</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserDateTime(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, EditableDateTimeFormatterKind kind)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EditableDateTimeFormatter formatter = EditableDateTimeFormatters.Get(kind);

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      item.TextAlign = HorizontalAlignment.Center;
      item.TextWidth = formatter.TextWidth;
      item.MinTextWidth = formatter.TextWidth;
      item.SizeGroup = kind.ToString();
      item.Format = formatter.Format;
      item.FormatProvider = formatter.FormatProvider;
      item.MaskProvider = formatter.MaskProvider;
      //Item.CanIncSearch = true;
      item.DataType = typeof(DateTime);

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить столбец для отображения денежных сумм
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMoney(string columnName, string headerText)
    {
      return AddMoney(columnName, headerText, false);
    }

    /// <summary>
    /// Добавить столбец для отображения денежных сумм
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="showPlusSign">Если true, то для положительных числовых значений будет
    /// отображаться знак "+". 
    /// Может быть удобно для столбцов, содержащих разности</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMoney(string columnName, string headerText, bool showPlusSign)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = 12;
      item.MinTextWidth = 8;
      if (showPlusSign)
        item.Format = "+0.00;-0.00;0.00";
      else
        item.Format = "0.00";
      item.SizeGroup = "Money";
      item.DataType = typeof(Decimal);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить столбец для отображения денежных сумм.
    /// Заголовок столбца равен <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMoney(string columnName)
    {
      return AddMoney(columnName, columnName);
    }

    /// <summary>
    /// Добавить вычисляемый столбец для отображения денежных сумм.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserMoney(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      return AddUserMoney(name, sourceColumnNames, valueNeeded, headerText, false);
    }

    /// <summary>
    /// Добавить вычисляемый столбец для отображения денежных сумм.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="showPlusSign">Если true, то для положительных числовых значений будет
    /// отображаться знак "+". 
    /// Может быть удобно для столбцов, содержащих разности</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddUserMoney(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, bool showPlusSign)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = 12;
      item.MinTextWidth = 8;
      if (showPlusSign)
        item.Format = "+0.00;-0.00;0.00";
      else
        item.Format = "0.00";
      item.SizeGroup = "Money";
      item.ValueNeeded += valueNeeded;
      item.DataType = typeof(Decimal);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить денежный столбец, являющийся суммой двух или более других столбцов
    /// типа decimal. Если все столбцы в строке имеют значение DBNull, то значение
    /// не выводится
    /// </summary>
    /// <param name="name">Условное имя столбца</param>
    /// <param name="sourceColumnNames">Имена исходных (суммируемых) столбцов</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerSumColumn AddSumMoney(string name, string sourceColumnNames,
      string headerText)
    {
      EFPGridProducerSumColumn item = new EFPGridProducerSumColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = 12;
      item.MinTextWidth = 8;
      item.Format = "0.00";
      item.SizeGroup = "Money";
      item.DataType = typeof(Decimal);
      Add(item);
      return item;
    }

    #endregion

    #region Перечислимые значения

    /// <summary>
    /// Добавляет текстовый столбец для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Добавляемый столбец имеет имя "<paramref name="sourceColumnName"/>_Text".
    /// Для показа текста "нерегулярных" перечислений используйте AddUserText().
    /// </summary>
    /// <param name="sourceColumnName">Имя целочисленного столбца, содержащего исходное значение</param>
    /// <param name="textValues">Список текстовых значений, которые показываются в создаваемом
    /// столбце</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerEnumColumn AddEnumText(string sourceColumnName,
      string[] textValues,
      string headerText, int textWidth, int minTextWidth)
    {
      EFPGridProducerEnumColumn item = new EFPGridProducerEnumColumn(sourceColumnName, textValues);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Left;
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      item.DataType = typeof(string);

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавляет столбец значка для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Добавляемый столбец имеет имя "<paramref name="sourceColumnName"/>_Image".
    /// Для показа текста "нерегулярных" перечислений используйте AddUserImage().
    /// </summary>
    /// <param name="sourceColumnName">Имя целочисленного столбца, содержащего исходное значение</param>
    /// <param name="imageKeys">Список тегов в EFPApp.MainImages, которые показываются в создаваемом
    /// столбце</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerEnumImageColumn AddEnumImage(string sourceColumnName,
      string[] imageKeys,
      string headerText)
    {
      EFPGridProducerEnumImageColumn item = new EFPGridProducerEnumImageColumn(sourceColumnName, imageKeys);
      item.HeaderText = headerText;
      item.CustomOrderSourceColumnName = sourceColumnName; // TODO: 05.07.2021. Порядок строк при сортировке столбцов для AddEnumText() и AddEnumImage() не совпадают

      Add(item);
      return item;
    }
    /// <summary>
    /// Добавляет столбец значка для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Добавляемый столбец имеет имя "<paramref name="sourceColumnName"/>_Image".
    /// Для показа текста "нерегулярных" перечислений используйте AddUserImage().
    /// </summary>
    /// <param name="sourceColumnName">Имя целочисленного столбца, содержащего исходное значение</param>
    /// <param name="imageKeys">Список тегов в EFPApp.MainImages, которые показываются в создаваемом
    /// столбце</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerEnumImageColumn AddEnumImage(string sourceColumnName,
      string[] imageKeys)
    {
      return AddEnumImage(sourceColumnName, imageKeys, String.Empty);
    }

    #endregion

    #region Текстовое представление дат

#if XXX
    /// <summary>
    /// Добавление расчетного столбца для вывода месяца и года в виде текста ("Март 2013 г.")
    /// </summary>
    /// <param name="columnPrefixName">Префикс имени столбца. Таблица должна содержать столбцы "ПрефиксГод" и "ПрефиксМесяц"</param>
    /// <param name="HeaderText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public GridProducerUserColumn AddYearMonth(string columnPrefixName, string HeaderText)
    {
      GridProducerUserColumn Col = AddUserText(columnPrefixName, columnPrefixName + "Год," + columnPrefixName + "Месяц",
        new GridProducerUserColumnValueNeededEventHandler(YearMonthColumnValueNeeded),
        HeaderText,
        16 /* "Сентябрь 2012 г."*/,
        12);
      Col.TextAlign = HorizontalAlignment.Center;
      Col.SizeGroup = "YearMonth";
      return Col;
    }

    private static void YearMonthColumnValueNeeded(object Sender, GridProducerUserColumnValueNeededEventArgs Args)
    {
      // TODO:
      /*
      GridProducerUserColumn Col = (GridProducerUserColumn)Sender;
      int Year = DataTools.GetInt(Args.Row, Col.FieldNames[0]);
      int Month = DataTools.GetInt(Args.Row, Col.FieldNames[1]);
      if (Year != 0)
        Args.Value = DataConv.DateLongStr(Year, Month);
       * */
    }
#endif

    /// <summary>
    /// Добавить вычисляемый текстовый столбец для отображения интервала дат на основании двух
    /// полей с датой.
    /// Для отображения текста используется DateRangeFormatter.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="firstColumnName">Имя поля с начальной датой диапазона</param>
    /// <param name="lastColumnName">Имя поля с конечной датой диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">Использование длинного (true) или короткого (false) формата отображения.
    /// См. класс DateRangeFormatter</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDateRange(string name, string firstColumnName, string lastColumnName,
      string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = AddDateRange(name, firstColumnName, lastColumnName, headerText, longFormat);
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      return item;
    }

    /// <summary>
    /// Добавить вычисляемый текстовый столбец для отображения интервала дат на основании двух
    /// полей с датой.
    /// Для отображения текста используется DateRangeFormatter.
    /// Эта версия вычисляет ширину столбца автоматически.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="firstColumnName">Имя поля с начальной датой диапазона</param>
    /// <param name="lastColumnName">Имя поля с конечной датой диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">Использование длинного (true) или короткого (false) формата отображения.
    /// См. класс DateRangeFormatter</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddDateRange(string name, string firstColumnName, string lastColumnName,
      string headerText, bool longFormat)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(name, new string[] { firstColumnName, lastColumnName });

      item.HeaderText = headerText;
      if (longFormat)
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(DateRangeColumn_LongValueNeeded);
      else
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(DateRangeColumn_ShortValueNeeded);
      // 05.07.2019.
      // Определяем ширину столбца
      if (longFormat)
        item.TextWidth = DateRangeFormatter.Default.DateRangeLongTextLength;
      else
        item.TextWidth = DateRangeFormatter.Default.DateRangeShortTextLength;
      item.MinTextWidth = item.TextWidth;
      item.EmptyValue = DateRangeFormatter.Default.ToString(null, null, longFormat);

      item.SizeGroup = longFormat ? "DateRangeLong" : "DateRangeShort";
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    internal static void DateRangeColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateRangeColumn_ValueNeeded(sender, args, true);
    }

    internal static void DateRangeColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateRangeColumn_ValueNeeded(sender, args, false);
    }
    private static void DateRangeColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
    {
      EFPGridProducerItemBase item = (EFPGridProducerItemBase)sender;
      DateTime? firstDate = args.GetNullableDateTime(0);
      DateTime? lastDate = args.GetNullableDateTime(1);
      if (firstDate.HasValue || lastDate.HasValue)
        args.Value = DateRangeFormatter.Default.ToString(firstDate, lastDate, longFormat);
    }

    /// <summary>
    /// Создает столбец для отображения столбца, содержащего номер дня в диапазоне от 1 до 365 как месяца и дня (структура MonthDay).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца табличного просмотра</param>
    /// <param name="sourceColumnName">Имя числового столбца в базе данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMonthDay(string name, string sourceColumnName, string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = AddMonthDay(name, sourceColumnName, headerText, longFormat);
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      return item;
    }

    /// <summary>
    /// Создает текстовый столбец для отображения числового поля с номером дня в диапазоне от 1 до 365 как месяца и дня (структура MonthDay).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// Эта версия вычисляет ширину столбца автоматически.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца табличного просмотра</param>
    /// <param name="sourceColumnName">Имя числового столбца в базе данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMonthDay(string name, string sourceColumnName, string headerText, bool longFormat)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(name, new string[] { sourceColumnName });
      if (longFormat)
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayColumn_LongValueNeeded);
      else
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayColumn_ShortValueNeeded);
      item.EmptyValue = DateRangeFormatter.Default.ToString(MonthDayRange.Empty, longFormat);
      item.HeaderText = headerText;
      if (longFormat)
        item.TextWidth = DateRangeFormatter.Default.MonthDayLongTextLength;
      else
        item.TextWidth = DateRangeFormatter.Default.MonthDayShortTextLength;
      item.MinTextWidth = item.TextWidth;
      item.SizeGroup = longFormat ? "MonthDayLong" : "MonthDayShort";
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    internal static void MonthDayColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayColumn_ValueNeeded(sender, args, true);
    }

    internal static void MonthDayColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayColumn_ValueNeeded(sender, args, false);
    }

    private static void MonthDayColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
    {
      EFPGridProducerItemBase item = (EFPGridProducerItemBase)sender;
      int v = args.GetInt(item.SourceColumnNames[0]);
      if (v == 0)
        return;
      else if (v < 1 || v > 365)
        args.Value = "?? " + v.ToString();
      else
      {
        MonthDay md = new MonthDay(v);
        args.Value = DateRangeFormatter.Default.ToString(md, longFormat);
      }
    }

    /// <summary>
    /// Создает столбец для отображения двух столбоц, содержащих номер дня в диапазоне от 1 до 365 как диапазона дней в году (структура MonthDayRange).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца табличного просмотра</param>
    /// <param name="firstDayColumnName">Имя числового столбца в базе данных, задающего первый день диапазона</param>
    /// <param name="lastDayColumnName">Имя числового столбца в базе данных, задающего последний день диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMonthDayRange(string name, string firstDayColumnName, string lastDayColumnName, string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = AddMonthDayRange(name, firstDayColumnName, lastDayColumnName, headerText, longFormat);
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      return item;
    }

    /// <summary>
    /// Создает столбец для отображения двух столбоц, содержащих номер дня в диапазоне от 1 до 365 как диапазона дней в году (структура MonthDayRange).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// Эта версия вычисляет ширину столбца автоматически.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца табличного просмотра</param>
    /// <param name="firstDayColumnName">Имя числового столбца в базе данных, задающего первый день диапазона</param>
    /// <param name="lastDayColumnName">Имя числового столбца в базе данных, задающего последний день диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddMonthDayRange(string name, string firstDayColumnName, string lastDayColumnName, string headerText, bool longFormat)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(name, new string[] { firstDayColumnName, lastDayColumnName });
      if (longFormat)
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayRangeColumn_LongValueNeeded);
      else
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayRangeColumn_ShortValueNeeded);
      item.HeaderText = headerText;
      if (longFormat)
        item.TextWidth = DateRangeFormatter.Default.MonthDayRangeLongTextLength;
      else
        item.TextWidth = DateRangeFormatter.Default.MonthDayRangeShortTextLength;
      item.MinTextWidth = item.TextWidth;
      item.SizeGroup = longFormat ? "MonthDayRangeLong" : "MonthDayRangeShort";
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    internal static void MonthDayRangeColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayRangeColumn_ValueNeeded(sender, args, true);
    }

    internal static void MonthDayRangeColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayRangeColumn_ValueNeeded(sender, args, false);
    }

    private static void MonthDayRangeColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
    {
      EFPGridProducerItemBase item = (EFPGridProducerItemBase)sender;
      int v1 = args.GetInt(item.SourceColumnNames[0]);
      int v2 = args.GetInt(item.SourceColumnNames[1]);
      if (v1 == 0 && v2 == 0)
        return;
      else if (v1 < 1 || v1 > 365 || v2 < 1 || v2 > 365)
        args.Value = "?? " + v1.ToString() + "-" + v2.ToString();
      else
      {
        MonthDayRange r = new MonthDayRange(new MonthDay(v1), new MonthDay(v2));
        args.Value = DateRangeFormatter.Default.ToString(r, longFormat);
      }
    }

    #endregion

    #region CheckBox

    /// <summary>
    /// Добавить столбец-флажок для логического поля
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerCheckBoxColumn AddBool(string columnName, string headerText)
    {
      EFPGridProducerCheckBoxColumn item = new EFPGridProducerCheckBoxColumn(columnName);
      item.HeaderText = headerText;
      item.DataType = typeof(bool);
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить вычисляемый столбец-флажок для логического поля
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerCheckBoxColumn AddUserBool(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerCheckBoxColumn item = new EFPGridProducerCheckBoxColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;
      item.DataType = typeof(bool);

      Add(item);
      return item;
    }

    #endregion

    #region Значок

    /// <summary>
    /// Добавить вычисляемый столбец с картинкой
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerImageColumn AddUserImage(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerImageColumn item = new EFPGridProducerImageColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      Add(item);
      return item;
    }

    /// <summary>
    /// Добавить вычисляемый столбец с картинкой
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="sourceColumnNames">Список имен полей, на основании которых вычисляются значения полей</param>
    /// <param name="valueNeeded">Пользовательский обработчик, выполняющий расчет значений.
    /// Вызывается при прорисовке каждой строки просмотра</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerImageColumn AddUserImage(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded)
    {
      return AddUserImage(name, sourceColumnNames, valueNeeded, string.Empty);
    }

    #endregion

    #endregion

    #region Прочие методы

    /// <summary>
    /// Выводит список столбцов (для отладки)
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

    #region IGridProducerColumns Members
#if XXX
    public int IndexOf(IEFPGridProducerColumn ColumnProducer)
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i] == ColumnProducer)
          return i;
      }
      return -1;
    }
#endif

    #endregion
  }

  #region Делегаты

  /// <summary>
  /// Аргументы события EFPGridProducerColumn.CellClick
  /// </summary>
  public class EFPGridProducerCellClickEventArgs : EFPGridProducerBaseEventArgs
  {
    #region Защищенный конструктор

    internal EFPGridProducerCellClickEventArgs(EFPGridProducerItemBase owner)
      : base(owner)
    {
    }

    #endregion


    // Нет новых свойств
  }

  /// <summary>
  /// Делегат события EFPGridProducerColumn.CellClick
  /// </summary>
  /// <param name="sender">Объект EFPGridProducerColumn</param>
  /// <param name="args"></param>
  public delegate void EFPGridProducerCellClickEventHandler(object sender,
    EFPGridProducerCellClickEventArgs args);


  /// <summary>
  /// Аргументы события EFPGridProducerColumn.CellEdit
  /// </summary>
  public class EFPGridProducerCellEditEventArgs : EFPGridProducerBaseEventArgs
  {
    #region Защищенный конструктор

    internal EFPGridProducerCellEditEventArgs(EFPGridProducerItemBase owner)
      : base(owner)
    {
    }

    #endregion


    /// <summary>
    /// Свойство должно быть установлено в true, если редактирование выполнено и дальнейшие действия не требуются
    /// </summary>
    public bool Handled
    {
      get { return _Handled; }
      set { _Handled = value; }
    }
    private bool _Handled;
  }

  /// <summary>
  /// Делегат события EFPGridProducerColumn.CellEdit
  /// </summary>
  /// <param name="sender">Объект EFPGridProducerColumn</param>
  /// <param name="args"></param>
  public delegate void EFPGridProducerCellEditEventHandler(object sender,
    EFPGridProducerCellEditEventArgs args);

  #endregion

  /// <summary>
  /// Описание одного возможного столбца.
  /// Данные извлекаются из источника данных или столбец может быть вычисляемыми.
  /// </summary>
  public class EFPGridProducerColumn : EFPGridProducerItemBase, IEFPGridProducerColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание столбца с заданным именем.
    /// Данные будут извлекаться из источника данных 
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public EFPGridProducerColumn(string columnName)
      : this(columnName, null)
    {
    }

    /// <summary>
    /// Создание вычисляемого столбца.
    /// Если <paramref name="sourceColumnNames"/> задано, то столбец будет вычисляемым.
    /// Если столбец извлекает данные не из источника данных, то следует задать пустой массив <paramref name="sourceColumnNames"/>=DataTools.EmptyStrings.
    /// </summary>
    /// <param name="name">Условное имя этого столбца</param>
    /// <param name="sourceColumnNames">Имена столбцов, на основании которых производится вычисления.
    /// Если null, то столбец является обычным, а невычисляемым.
    /// Для создания вычисляемого столбца, не использующего данные других столбцов (например для нумерации строк),
    /// задайте пустой массив DataTools.EmptyStrings</param>
    public EFPGridProducerColumn(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      TextWidth = 5;
      MinTextWidth = 5;
      CanIncSearch = false;
      TextAlign = HorizontalAlignment.Left;
      if (sourceColumnNames == null)
        _CustomOrderSourceColumnName = name;
      else
        _CustomOrderSourceColumnName = String.Empty;
    }

    #endregion

    #region Общие свойства столбца

    /// <summary>
    /// Заголовок столбца в табличном просмотре
    /// </summary>
    public string HeaderText
    {
      get
      {
        if (_HeaderText == null)
          return Name;
        else
          return _HeaderText;
      }
      set
      {
        _HeaderText = value;
      }
    }
    private string _HeaderText;

    /// <summary>
    /// Используется, когда свойство DisplayName не задано в явном виде
    /// </summary>
    /// <returns></returns>
    protected override string GetDefaultDisplayName()
    {
      if (String.IsNullOrEmpty(HeaderText))
        return base.GetDefaultDisplayName();
      else
        return DataTools.RemoveDoubleChars(DataTools.ReplaceAny(HeaderText, "\r\n", ' '), ' ');
    }

    /// <summary>
    /// Всплывающая подсказка при наведении курсора на заголок столбца.
    /// Если свойство не установлено в явном виде, возвращает DisplayName
    /// </summary>
    public string HeaderToolTipText
    {
      get
      {
        if (String.IsNullOrEmpty(_HeaderToolTipText))
          return DisplayName;
        else
          return _HeaderToolTipText;
      }
      set
      {
        _HeaderToolTipText = value;
      }
    }
    private string _HeaderToolTipText;

    /// <summary>
    /// Ширина столбца в символах
    /// </summary>
    public int TextWidth
    {
      get { return _TextWidth; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", value, "Ширина столбца не может быть меньше 1 символа");
        _TextWidth = value;
      }
    }
    private int _TextWidth;

    /// <summary>
    /// Минимальная ширина столбца в символах
    /// </summary>
    public int MinTextWidth
    {
      get { return _MinTextWidth; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", value, "Минимальная ширина столбца не может быть меньше 1 символа");
        _MinTextWidth = value;
      }
    }
    private int _MinTextWidth;

    private const string NonResizableSizeGroup = "-";

    /// <summary>
    /// Имя группы для синхронного изменения размеров. См. EFPDataGridViewColumn.SizeGroup
    /// Это свойство несовместимо со сбросом свойства Resizable=false
    /// </summary>
    public string SizeGroup
    {
      get
      {
        if (_SizeGroup == NonResizableSizeGroup)
          return String.Empty;
        else
          return _SizeGroup;
      }
      set { _SizeGroup = value; }
    }
    private string _SizeGroup;

    /// <summary>
    /// Если true (по умолчанию), то пользователь может менять гирину столбца.
    /// </summary>
    public bool Resizable
    {
      get { return _SizeGroup != NonResizableSizeGroup; }
      set
      {
        if (value)
        {
          if (_SizeGroup == NonResizableSizeGroup)
            _SizeGroup = String.Empty;
        }
        else
          _SizeGroup = NonResizableSizeGroup;
      }
    }

    /// <summary>
    /// Горизонтальное выравнивание текста в столбце
    /// </summary>
    public HorizontalAlignment TextAlign { get { return _TextAlign; } set { _TextAlign = value; } }
    private HorizontalAlignment _TextAlign;

    /// <summary>
    /// Число строк текста для отображения в ячейке.
    /// Значение по умолчанию: 1. Если задано значение, большее 1, то для столбца 
    /// устанавливается свойство DataGridViewCellStyle.WrapMode=True, а высота
    /// всех строк в просмотре будет увеличена
    /// </summary>
    public int TextRowHeight { get { return _TextRowHeight; } set { _TextRowHeight = value; } }
    private int _TextRowHeight;

    /// <summary>
    /// Форматирование данных при отображении.
    /// Используется для установки свойства DataGridViewCellStyle.Format.
    /// </summary>
    public string Format { get { return _Format; } set { _Format = value; } }
    private string _Format;

    /// <summary>
    /// Форматизатор для столбца.
    /// Используется для установки свойства DataGridViewCellStyle.FormatProvider.
    /// </summary>
    public IFormatProvider FormatProvider { get { return _FormatProvider; } set { _FormatProvider = value; } }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Установка в true предотвращает редактирование столбца "По месту"
    /// </summary>
    public bool ReadOnly { get { return _ReadOnly; } set { _ReadOnly = value; } }
    private bool _ReadOnly;

    /// <summary>
    /// Установка в true разрешает быстрый поиск по первым буквам для этого столбца
    /// </summary>
    public bool CanIncSearch { get { return _CanIncSearch; } set { _CanIncSearch = value; } }
    private bool _CanIncSearch;

    /// <summary>
    /// Используется при поиске по первым буквам
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } set { _MaskProvider = value; } }
    private IMaskProvider _MaskProvider;

    /// <summary>
    /// Цветовое оформление для столбца
    /// </summary>
    public EFPDataGridViewColorType ColorType { get { return _ColorType; } set { _ColorType = value; } }
    private EFPDataGridViewColorType _ColorType;

    /// <summary>
    /// Признак вывода бледным шрифтом
    /// </summary>
    public bool Grayed { get { return _Grayed; } set { _Grayed = value; } }
    private bool _Grayed;

    /// <summary>
    /// Формат столбца при сохранении просмотра в DBF-формате
    /// </summary>
    public DbfFieldInfo DbfInfo { get { return _DbfInfo; } set { _DbfInfo = value; } }
    private DbfFieldInfo _DbfInfo;


    /// <summary>
    /// Имя столбца, используемого для произвольной сортировки.
    /// По умолчанию для невычисляемых столбцов устанавливается равным Name.
    /// Для вычисляемых столбцов - пустая строка
    /// </summary>
    public string CustomOrderSourceColumnName { get { return _CustomOrderSourceColumnName; } set { _CustomOrderSourceColumnName = value; } }
    private string _CustomOrderSourceColumnName;

    /// <summary>
    /// Тип данных для столбца. Используется в таблице-повторителе EFPGridProducerDataTableRepeater
    /// </summary>
    public Type DataType { get { return _DataType; } set { _DataType = value; } }
    private Type _DataType;

    #endregion

    #region Создания столбца для табличного просмотра EFPDataGridView

    /// <summary>
    /// Создает объект столбца для табличного просмотра Windows Forms.
    /// Столбец не добавляется в просмотр.
    /// </summary>
    /// <returns>Объект столбца, производный от DataGridViewColumn</returns>
    public virtual DataGridViewColumn CreateColumn()
    {
      DataGridViewTextBoxColumn gridColumn = new DataGridViewTextBoxColumn();
      InitColumn(gridColumn);
      return gridColumn;
    }

    /// <summary>
    /// Инициализирует свойства DataGridViewColumn.
    /// Используется в методах CreateColumn()
    /// </summary>
    /// <param name="column"></param>
    protected void InitColumn(DataGridViewColumn column)
    {
      column.Name = Name;
      if (!IsCalculated)
        column.DataPropertyName = Name;
      column.HeaderText = HeaderText;
      column.ToolTipText = HeaderToolTipText;
      switch (TextAlign)
      {
        case HorizontalAlignment.Left:
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
          break;
        case HorizontalAlignment.Center:
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
          break;
        case HorizontalAlignment.Right:
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          break;
      }

      if (TextRowHeight > 1)
        column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

      column.DefaultCellStyle.Format = Format;
      if (FormatProvider != null)
        column.DefaultCellStyle.FormatProvider = FormatProvider;
    }

    /// <summary>
    /// Применить настроенную конфигурацию табличного просмотра к столбцу Windows Forms.
    /// Устанавливает свойства DataGridViewColumn.AutoSizeMode, Width, FillWeight и, возможно, другие. 
    /// </summary>
    /// <param name="column">Столбец Windows Forms, </param>
    /// <param name="config">Конфигурация столбца</param>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public virtual void ApplyConfig(DataGridViewColumn column, EFPDataGridViewConfigColumn config, EFPDataGridView controlProvider)
    {
      bool IsImgColumn = column is DataGridViewImageColumn || column is DataGridViewCheckBoxColumn;
      if (config.FillMode)
      {
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        column.FillWeight = config.FillWeight;
      }
      else
      {
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        if (config.Width == 0)
          column.Width = GetWidth(controlProvider.Measures);
        else
          column.Width = config.Width;
        column.FillWeight = 1; // 08.02.2017
      }
      if (IsImgColumn)
        column.MinimumWidth = column.Width;
      else
        column.MinimumWidth = controlProvider.Measures.GetTextColumnWidth(MinTextWidth);

      if (!Resizable)
        column.Resizable = DataGridViewTriState.False;

      if (ReadOnly)
        column.ReadOnly = true;
    }

    #endregion

    #region Создание столбца иерархического просмотра EFPDataTreeView

    /// <summary>
    /// Создает столбец для иерархического просмотра TreeViewAdv
    /// </summary>
    /// <param name="config">Конфигурация столбца</param>
    /// <returns>Столбец TreeViewAdv</returns>
    public virtual TreeColumn CreateTreeColumn(EFPDataGridViewConfigColumn config)
    {
      TreeColumn column = new TreeColumn(DisplayName, TextWidth * 10);
      /*
      if (Config.FillMode)
      {
        Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column.FillWeight = Config.FillWeight;
      } */
      column.TextAlign = this.TextAlign;
      column.TooltipText = HeaderToolTipText;

      return column;
    }

    /// <summary>
    /// Создает присоединяемый элемент для столбца TreeViewAdv.
    /// Непереопределенный метод возвращает NodeTextBox
    /// </summary>
    /// <returns>Присоединяемый элемент</returns>
    public virtual BindableControl CreateNodeControl()
    {
      NodeTextBox tb = new NodeTextBox();
      tb.EditEnabled = !ReadOnly;
      tb.TextAlign = TextAlign; // 15.03.2019
      return tb;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="nodeControl"></param>
    /// <param name="config"></param>
    /// <param name="controlProvider"></param>
    public virtual void ApplyConfig(BindableControl nodeControl, EFPDataGridViewConfigColumn config, EFPDataTreeView controlProvider)
    {
      /*
      bool IsImgColumn = Column is DataGridViewImageColumn || Column is DataGridViewCheckBoxColumn;
      if (Config.FillMode)
      {
        Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column.FillWeight = Config.FillWeight;
      }
      else
      {
        Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        if (Config.Width == 0)
        {
          if (IsImgColumn)
          {
            if (Column is DataGridViewImageColumn)
              Column.Width = ControlProvider.Measures.ImageColumnWidth;
            else
              Column.Width = ControlProvider.Measures.CheckBoxColumnWidth;
          }
          else
            Column.Width = ControlProvider.Measures.GetTextColumnWidth(TextWidth);
        }
        else
          Column.Width = Config.Width;
      }
      if (IsImgColumn)
        Column.MinimumWidth = Column.Width;
      else
        Column.MinimumWidth = ControlProvider.Measures.GetTextColumnWidth(MinTextWidth);
        */

    }

    #endregion

    #region Получение значения

    /// <summary>
    /// Получение значение вычисляемого поля. Вызывает виртуальный метод OnValueNeeded().
    /// Если столбец/подсказка не являются вычисляемым, возвращается значение <paramref name="rowInfo"/>.Values.GetValue(Name).
    /// </summary>
    /// <param name="rowInfo">Информация о строке</param>
    /// <returns>Вычисленное значение</returns>
    public object GetValue(EFPDataViewRowInfo rowInfo)
    {
      object value;
      string toolTipText;
      base.DoGetValue(EFPGridProducerValueReason.Value, rowInfo, out value, out toolTipText);
      return value;
    }


    /// <summary>
    /// Вызывается при необходимости получения всплывающей подсказки для данной ячейки
    /// при наведении курсора. Если возвращается пустая строка, то нет дополнительной подсказки для ячейки
    /// </summary>
    /// <param name="rowInfo">Информация о строке</param>
    /// <param name="columnName">Игнорируется. Сделано для симметрии с остальными обработчиками</param>
    /// <returns>Текст подсказки</returns>
    public string GetCellToolTipText(EFPDataViewRowInfo rowInfo, string columnName)
    {
      object value;
      string toolTipText;
      base.DoGetValue(EFPGridProducerValueReason.ToolTipText, rowInfo, out value, out toolTipText);

      if (toolTipText == null)
        return String.Empty;
      else
        return toolTipText;
    }

    #endregion

    #region Событие GetCellAttributes

    /// <summary>
    /// Событие вызывается при форматировании ячейки с возможностью задания 
    /// цветовых атрибутов и отформатированного значения
    /// </summary>
    public event EFPDataGridViewCellAttributesEventHandler GetCellAttributes;

    ///// <summary>
    ///// Возвращает true, если для столбца заданы форматирующие обработчики
    ///// </summary>
    //public bool HasGetCellAttributes
    //{
    //  get { return GetCellAttributes != null; }
    //}

    /// <summary>
    /// Вызов события GetCellAttributes из DocGridHandler
    /// </summary>
    /// <param name="args">Аргументы, передаваемые обработчику</param>
    internal void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      if (GetCellAttributes != null)
        GetCellAttributes(this, args);
    }

    #endregion

    #region Событие CellClick

    /// <summary>
    /// Событие вызывается при одинарном щелчке левой кнопки мыши по ячейке таблицы.
    /// Используется для столбцов CheckBox.
    /// </summary>
    public event EFPGridProducerCellClickEventHandler CellClick;

    /// <summary>
    /// Вызывается при одинарном щелчке левой кнопки мыши по ячейке таблицы.
    /// Вызывает обработчик события CellClick, если он установлен
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCellClick(EFPGridProducerCellClickEventArgs args)
    {
      if (CellClick != null)
        CellClick(this, args);
    }

    /// <summary>
    /// Вызывается при одинарном щелчке левой кнопки мыши по ячейке таблицы.
    /// Используется для столбцов CheckBox.
    /// Вызывает виртуальный метод OnCellClick()
    /// </summary>
    /// <param name="rowInfo">Информация о строке</param>
    /// <param name="columnName">Игнорируется, т.к. EFPGridProducerColumn и всегда относится к единственному столбцу просмотра</param>
    public void PerformCellClick(EFPDataViewRowInfo rowInfo, string columnName)
    {
      EFPGridProducerCellClickEventArgs args = new EFPGridProducerCellClickEventArgs(this);
      args.RowInfo = rowInfo;
      OnCellClick(args);
    }

    #endregion

    #region Событие CellEdit

    /// <summary>
    /// Событие вызывается при попытке редактирования ячейки, связанной со столбцом.
    /// Метод вызывается до стандартной обработки, например, посылки извещения EFPDataGridView.Editdata
    /// </summary>
    public event EFPGridProducerCellEditEventHandler CellEdit;

    /// <summary>
    /// Вызывает обработчик события CellEdit, если он установлен
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnCellEdit(EFPGridProducerCellEditEventArgs args)
    {
      if (CellEdit != null)
        CellEdit(this, args);
    }

    /// <summary>
    /// Вызывается при попытке редактирования ячейки, связанной со столбцом.
    /// Метод вызывается до стандартной обработки, например, посылки извещения EFPDataGridView.Editdata
    /// </summary>
    /// <param name="rowInfo">Информация о строке просмотра</param>
    /// <param name="columnName">Игнорируется, т.к. EFPGridProducerColumn и всегда относится к единственному столбцу просмотра</param>
    /// <returns>true, если редактирование выполнено и дальнейшее редактирование не должно выполняться</returns>
    public bool PerformCellEdit(EFPDataViewRowInfo rowInfo, string columnName)
    {
      EFPGridProducerCellEditEventArgs args = new EFPGridProducerCellEditEventArgs(this);
      args.RowInfo = rowInfo;
      OnCellEdit(args);
      return args.Handled;
    }

    #endregion

    #region Свойства для печати

    /// <summary>
    /// Многострочные заголовки при печати таблицы
    /// </summary>
    public string[] PrintHeaders { get { return _PrintHeaders; } set { _PrintHeaders = value; } }
    private string[] _PrintHeaders;

    /// <summary>
    /// Многострочные заголовки при печати таблицы (свойство PrintHeaders)
    /// Версия для установки в виде одной строки с заменой символов:
    /// "|" - разделитель многострочного заголовка
    /// "^" - мягкий перенос
    /// "_" - неразрывный пробел
    /// </summary>
    public string PrintHeadersSpec
    {
      get { return DataTools.StrFromSpecCharsArray(PrintHeaders); }
      set { PrintHeaders = DataTools.StrToSpecCharsArray(value); }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Получить имена полей, которые должны быть в наборе данных.
    /// Если столбец является вычисляемым, в список добавляются имена исходных столбцов SourceColumnNames.
    /// Иначе добавляется имя Name для невычисляемого столбца/подсказки/.
    /// Также в список добавляется CustomOrderSourceColumnName.
    /// </summary>
    /// <param name="columns">Список для добавления имен полей</param>
    public override void GetColumnNames(IList<string> columns)
    {
      base.GetColumnNames(columns);
      if (!String.IsNullOrEmpty(CustomOrderSourceColumnName))
        columns.Add(CustomOrderSourceColumnName);
    }

    /// <summary>
    /// Возвращает желаемую ширину столбца в пикселях.
    /// Непереопределенный метод вычисляет ширину исходя из свойства TextWidth.
    /// Переопределяется для столбцов c CheckBox и значком
    /// </summary>
    /// <param name="measures">Объект для вычисления размеров, присоединенный к табличному просмотру.</param>
    /// <returns>Ширина в пикселях</returns>
    public virtual int GetWidth(IEFPGridControlMeasures measures)
    {
      int tw = this.TextWidth;
      if (tw < 1)
        tw = 1; // Если не установлено
      return measures.GetTextColumnWidth(/*TextWidth*/tw /*25.12.2020*/);
    }

    ///// <summary>
    ///// Вызывается при обновлении табличного просмотра. Если столбец использует
    ///// буферизацию, то она должна быть сброшена
    ///// </summary>
    //public virtual void Refresh()
    //{
    //}

    #endregion
  }

  /// <summary>
  /// Расширенный класс столбца типа CheckBox
  /// </summary>
  public class EFPGridProducerCheckBoxColumn : EFPGridProducerColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец, отображающий значение поля
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public EFPGridProducerCheckBoxColumn(string columnName)
      : this(columnName, null)
    {
    }


    /// <summary>
    /// Создает вычисляемый столбец
    /// </summary>
    /// <param name="name">Условное имя этого столбца</param>
    /// <param name="sourceColumnNames">Имена столбцов, на основании которых производится вычисления.
    /// Если null, то столбец является обычным, а не вычисляемым.</param>
    public EFPGridProducerCheckBoxColumn(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      TextWidth = 3;
      MinTextWidth = 3;
      SizeGroup = "CheckBox";
      TextAlign = HorizontalAlignment.Center;
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Создает объект столбца для табличного просмотра Windows Forms.
    /// Столбец не добавляется в просмотр.
    /// </summary>
    /// <returns>Объект столбца</returns>
    public override DataGridViewColumn CreateColumn()
    {
      ExtDataGridViewCheckBoxColumn column = new ExtDataGridViewCheckBoxColumn();
      InitColumn(column);
      return column;
    }

    /// <summary>
    /// Возвращает желаемую ширину столбца в пикселях.
    /// </summary>
    /// <param name="measures">Объект для вычисления размеров, присоединенный к табличному просмотру.</param>
    /// <returns>Ширина в пикселях</returns>
    public override int GetWidth(IEFPGridControlMeasures measures)
    {
      return measures.CheckBoxColumnWidth;
    }

    #endregion
  }


  /// <summary>
  /// Вычисляемый столбец с изображением
  /// </summary>
  public class EFPGridProducerImageColumn : EFPGridProducerColumn
  {
    #region Конструктор

    /// <summary>
    /// Создает описатель столбца
    /// </summary>
    /// <param name="columnName">Имя вычисляемого столбца изображения</param>
    /// <param name="sourceColumnNames">Имена исходных стобцов. Имена разделяются запятыми</param>
    public EFPGridProducerImageColumn(string columnName, string[] sourceColumnNames)
      : base(columnName, sourceColumnNames)
    {
      TextWidth = 3;
      MinTextWidth = 3;
      SizeGroup = "Image";
      TextAlign = HorizontalAlignment.Center;
      CustomOrderSourceColumnName = String.Empty;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает объект столбца для табличного просмотра Windows Forms.
    /// Столбец не добавляется в просмотр.
    /// </summary>
    /// <returns>Объект столбца</returns>
    public override DataGridViewColumn CreateColumn()
    {
      DataGridViewImageColumn col = new DataGridViewImageColumn();
      InitColumn(col);
      return col;
    }

    /// <summary>
    /// Возвращает желаемую ширину столбца в пикселях.
    /// </summary>
    /// <param name="measures">Объект для вычисления размеров, присоединенный к табличному просмотру.</param>
    /// <returns>Ширина в пикселях</returns>
    public override int GetWidth(IEFPGridControlMeasures measures)
    {
      return measures.ImageColumnWidth;
    }

    #endregion
  }


  /// <summary>
  /// Столбец данных, возвращающий вычисляемое значение на
  /// основании одного целочисленного столбца данных, содержащего перечислимое значение 0,1,2,...
  /// </summary>
  public class EFPGridProducerEnumColumn : EFPGridProducerColumn
  {
    #region Конструктор

    /// <summary>
    /// Создание пользовательского столбца.
    /// Столбец получает имя "<paramref name="sourceColumnName"/>_Text".
    /// </summary>
    /// <param name="sourceColumnName">Имя числового столбца в таблице данных</param>
    /// <param name="textValues">Список текстовых значений</param>
    public EFPGridProducerEnumColumn(string sourceColumnName, string[] textValues)
      : this(sourceColumnName + "_Text", new string [] { sourceColumnName }, textValues)
    {
    }


    /// <summary>
    /// Эта версия предназначена для создания классов-наследников. Используется в ExtDBDocForms.dll.
    /// </summary>
    /// <param name="name">Условное имя этого столбца</param>
    /// <param name="sourceColumnNames">Исходные столбцы</param>
    /// <param name="textValues">Список текстовых значений</param>
    protected EFPGridProducerEnumColumn(string name, string[] sourceColumnNames, string[] textValues)
      : base(name, sourceColumnNames)
    {
      if (textValues == null)
        throw new ArgumentNullException("textValues");
      _TextValues = textValues;
      _NullIsZero = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текстовые значения для перечисления
    /// </summary>
    public string[] TextValues { get { return _TextValues; } }
    private string[] _TextValues;

    /// <summary>
    /// Если true (по умолчанию), то значение поля NULL интерпретируется как 0.
    /// Если false, то для значения NULL будет выводиться пустое значение
    /// </summary>
    public bool NullIsZero { get { return _NullIsZero; } set { _NullIsZero = value; } }
    private bool _NullIsZero;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Вычисляет значение
    /// </summary>
    /// <param name="args"></param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      object val = GetSourceValue(args);
      if (val is DBNull && (!NullIsZero))
        args.Value = String.Empty;
      else
      {
        int srcVal = DataTools.GetInt(val);
        if (srcVal < 0 || srcVal >= TextValues.Length)
          args.Value = "?? " + srcVal.ToString();
        else
          args.Value = TextValues[srcVal];
      }

      base.OnValueNeeded(args);
    }

    /// <summary>
    /// Возвращает значение числового поля.
    /// Непереопределенный метод возвращает значение исходного поля.
    /// </summary>
    /// <param name="args">Аргументы события ValueNeeded</param>
    /// <returns>Значение поля</returns>
    protected virtual object GetSourceValue(EFPGridProducerValueNeededEventArgs args)
    {
      return args.Values.GetValue(SourceColumnNames[0]);
    }

    #endregion
  }

  /// <summary>
  /// Столбец данных с изображениями из списка EFPApp.ImageKeys.
  /// Изображение берется на основании одного целочисленного столбца данных, содержащего перечислимое значение.
  /// </summary>
  public class EFPGridProducerEnumImageColumn : EFPGridProducerImageColumn
  {
    #region Конструктор

    /// <summary>
    /// Создание пользовательского столбца.
    /// Столбец получает имя "<paramref name="sourceColumnName"/>_Image".
    /// </summary>
    /// <param name="sourceColumnName">Имя числового столбца, содержащего перечислимое значение</param>
    /// <param name="imageKeys">Список тегов изображений в EFPApp.ImageKeys</param>
    public EFPGridProducerEnumImageColumn(string sourceColumnName, string[] imageKeys)
      : this(sourceColumnName + "_Image", new string [] { sourceColumnName }, imageKeys)
    {
    }

    /// <summary>
    /// Эта версия предназначена для создания классов-наследников. Используется в ExtDBDocForms.dll.
    /// </summary>
    /// <param name="name">Условное имя этого столбца</param>
    /// <param name="sourceColumnNames">Исходные столбцы</param>
    /// <param name="imageKeys">Список тегов изображений в EFPApp.ImageKeys</param>
    protected EFPGridProducerEnumImageColumn(string name, string[] sourceColumnNames, string[] imageKeys)
      : base(name, sourceColumnNames)
    {
      if (imageKeys == null)
        throw new ArgumentNullException("imageKeys");
      _ImageKeys = imageKeys;
      _NullIsZero = true;
      _ErrorImageKey = "Error";
      _EmptyToolTipText = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список тегов изображений в EFPApp.ImageKeys для перечисления.
    /// Задается в конструкторе
    /// </summary>
    public string[] ImageKeys { get { return _ImageKeys; } }
    private string[] _ImageKeys;

    /// <summary>
    /// Если true (по умолчанию), то значение поля NULL интерпретируется как 0.
    /// Если false, то для значения NULL будет выводиться пустое изображение
    /// </summary>
    public bool NullIsZero { get { return _NullIsZero; } set { _NullIsZero = value; } }
    private bool _NullIsZero;

    /// <summary>
    /// Изображение, используемое, если значение поля выходит за пределы списка ImageKeys.
    /// По умолчанию - "Error".
    /// </summary>
    public string ErrorImageKey { get { return _ErrorImageKey; } set { _ErrorImageKey = value; } }
    private string _ErrorImageKey;

    /// <summary>
    /// Тексты всплывающих подсказок, соответствующие массиву ImageKeys.
    /// </summary>
    public string[] ToolTipTexts
    {
      get { return _ToolTipTexts; }
      set
      {
        if (value != null)
        {
          if (value.Length != _ImageKeys.Length)
            throw new ArgumentException("Неправильная длина массива подсказок");
          _ToolTipTexts = value;
        }
      }
    }
    private string[] _ToolTipTexts;

    /// <summary>
    /// Сплывающая подсказка, соответствующая значению NULL, при NullIsZero=false.
    /// По умолчанию - пустая строка
    /// </summary>
    public string EmptyToolTipText
    {
      get { return _EmptyToolTipText; }
      set { _EmptyToolTipText = value; }
    }
    private string _EmptyToolTipText;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Вычисляет значение
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      object val = GetSourceValue(args);
      string imageKey;
      if (val is DBNull && (!NullIsZero))
      {
        imageKey = "EmptyImage";
        args.ToolTipText = EmptyToolTipText;
      }
      else
      {
        int srcVal = DataTools.GetInt(val);
        if (srcVal < 0 || srcVal >= ImageKeys.Length)
        {
          imageKey = ErrorImageKey;
          args.ToolTipText = "Неправильное значение: " + srcVal.ToString();
        }
        else
        {
          imageKey = ImageKeys[srcVal];
          if (ToolTipTexts != null)
            args.ToolTipText = ToolTipTexts[srcVal];
        }
      }
      args.Value = EFPApp.MainImages.Images[imageKey];

      base.OnValueNeeded(args);
    }

    /// <summary>
    /// Возвращает значение числового поля.
    /// Непереопределенный метод возвращает значение исходного поля.
    /// </summary>
    /// <param name="args">Аргументы события ValueNeeded</param>
    /// <returns>Значение поля</returns>
    protected virtual object GetSourceValue(EFPGridProducerValueNeededEventArgs args)
    {
      return args.Values.GetValue(SourceColumnNames[0]);
    }

    #endregion
  }

  /// <summary>
  /// Столбец, содержащий номера строк в табличном просмотре (1,2, ...)
  /// </summary>
  public class EFPGridProducerRowOrderColumn : EFPGridProducerColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает вычисляемый столбец c заданным именем и возможностью пропуска нумерации
    /// </summary>
    public EFPGridProducerRowOrderColumn(string name, string filterColumnName, int filterValue)
      : base(name, GetSourceColumnNames(filterColumnName))
    {
      HeaderText = "№ п/п";
      TextAlign = HorizontalAlignment.Right;
      _FilterColumnName = filterColumnName;
      _FilterValue = filterValue;
    }

    private static string[] GetSourceColumnNames(string filterColumnName)
    {
      if (String.IsNullOrEmpty(filterColumnName))
        return DataTools.EmptyStrings;
      else
        return new string[] { filterColumnName };
    }

    /// <summary>
    /// Создает вычисляемый столбец c заданным именем. Нумеруются все строки
    /// </summary>
    public EFPGridProducerRowOrderColumn(string name)
      : this( name, String.Empty, 0)
    {
    }

    /// <summary>
    /// Создает вычисляемый столбец с именем "RowOrder"
    /// </summary>
    public EFPGridProducerRowOrderColumn()
      : this("RowOrder")
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля для определения строк, подлежащих нумерации
    /// Если свойство не задано, нумеруются все строки
    /// Иначе нумерация выводится только для строк, проходящих фильтр.
    /// Прочие строки содержат пустое поле. Нумерация НЕ СДВИГАЕТСЯ, поэтому
    /// можно использовать только для пропуска итоговой строки.
    /// Задается в конструкторе
    /// </summary>
    public string FilterColumnName { get { return _FilterColumnName; } }
    private string _FilterColumnName;

    /// <summary>
    /// Значение поля для фильтра, заданного FilterColumnName
    /// Задается в конструкторе
    /// </summary>
    public int FilterValue { get { return _FilterValue; } }
    private int _FilterValue;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вычисляет значение
    /// </summary>
    /// <param name="args"></param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      args.Value = args.RowIndex + 1;
      if (!String.IsNullOrEmpty(FilterColumnName))
      {
        int Value = args.GetInt(FilterColumnName);
        if (Value != FilterValue)
          args.Value = null;
      }
      base.OnValueNeeded(args);
    }

    #endregion
  }

#if XXX

  #region Перечисление GridProducerDBFileInfoColumnKind

  /// <summary>
  /// Тип отображаемыз данных для столбца GridProducerDBFileInfoColumn
  /// </summary>
  public enum GridProducerDBFileInfoColumnKind
  {
    /// <summary>
    /// Имя файла
    /// </summary>
    Name,

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    Length,

    /// <summary>
    /// Время создания файла
    /// </summary>
    CreationTime,

    /// <summary>
    /// Время последней записи файла
    /// </summary>
    LastWriteTime
  }

  #endregion

  /// <summary>
  /// Столбец для вывода атрибутов файла, хранящегося в базе данных
  /// Значения извлекается с помощью метода AccDepClientExec.GetDBFileInfo()
  /// </summary>
  public class GridProducerDBFileInfoColumn : GridProducerColumn
  {
  #region Конструкторы

    /// <summary>
    /// Создание столбца.
    /// Полученный объект должен быть добавлен в описание просмотра с помощью
    /// GridProducerColumns.Add()
    /// Версия с возможностью задания имени поля
    /// </summary>
    /// <param name="Kind">Тип столбца (имя файла, размер, время)</param>
    /// <param name="SourceTableName">Имя таблицы в базе данных, которая хранит ссылки на файл</param>
    /// <param name="SourceColumnName">Имя поля в таблице базы данных, которое содержит ссылку на файл</param>
    /// <param name="ColumnName">Имя добавляемого столбца (например, "ИмяФайла"). Не должно совпадать с именем поля в базе данных.
    /// Если не задано, то имя создается автоматически на основе имени исходного столбца и типа</param>
    public GridProducerDBFileInfoColumn(GridProducerDBFileInfoColumnKind Kind, string SourceTableName, string SourceColumnName, string ColumnName)
      : base(GetColumnName(ColumnName, SourceColumnName, Kind))
    {
#if DEBUG
      if (String.IsNullOrEmpty(SourceTableName))
        throw new ArgumentNullException("SourceTableName");
      if (String.IsNullOrEmpty(SourceColumnName))
        throw new ArgumentNullException("SourceColumnName");
      if (ColumnName == SourceColumnName)
        throw new ArgumentException("Имя столбца не должно совпадать с именем исходного столбца \"" +
          SourceColumnName + "\"", "SourceColumnName");
#endif
      FSourceTableName = SourceTableName;
      FSourceColumnName = SourceColumnName;
      FKind = Kind;
      switch (Kind)
      {
        case GridProducerDBFileInfoColumnKind.Name:
          HeaderText = "Имя файла";
          Align = HorizontalAlignment.Left;
          break;

        case GridProducerDBFileInfoColumnKind.Length:
          HeaderText = "Размер, байт";
          Align = HorizontalAlignment.Right;
          SizeGroup = "FileLength";
          TextWidth = 11;
          MinTextWidth = 7;
          Format = "# ### ### ##0";
          FormatProvider = DataConv.DotNumberConvWithGroups;
          break;

        case GridProducerDBFileInfoColumnKind.CreationTime:
          HeaderText = "Время создания";
          Align = HorizontalAlignment.Center;
          TextWidth = 19;
          MinTextWidth = 19;
          SizeGroup = "DateTime";
          Format = "dd/MM/yyyy HH:mm:ss";
          FormatProvider = DataConv.DotDateTimeConv;
          break;

        case GridProducerDBFileInfoColumnKind.LastWriteTime:
          HeaderText = "Время записи";
          Align = HorizontalAlignment.Center;
          TextWidth = 19;
          MinTextWidth = 19;
          SizeGroup = "DateTime";
          Format = "dd/MM/yyyy HH:mm:ss";
          FormatProvider = DataConv.DotDateTimeConv;
          break;

        default:
          throw new ArgumentException("Неизвестный тип столбца: " + Kind.ToString(), "Kind");
      }
    }

    private static string GetColumnName(string ColumnName, string SourceColumnName, GridProducerDBFileInfoColumnKind Kind)
    {
#if DEBUG
      if (String.IsNullOrEmpty(SourceColumnName))
        throw new ArgumentNullException("SourceColumnName");
#endif
      if (String.IsNullOrEmpty(ColumnName))
      {
        string Suffix;
        switch (Kind)
        {
          case GridProducerDBFileInfoColumnKind.Name:
            Suffix = "Имя"; break;
          case GridProducerDBFileInfoColumnKind.Length:
            Suffix = "Размер"; break;
          case GridProducerDBFileInfoColumnKind.CreationTime:
            Suffix = "ВремяСоздания"; break;
          case GridProducerDBFileInfoColumnKind.LastWriteTime:
            Suffix = "ВремяЗаписи"; break;
          default:
            throw new ArgumentException("Неизвестный Kind");
        }
        return SourceColumnName + Suffix;
      }
      else
        return ColumnName;
    }

    /// <summary>
    /// Создание столбца.
    /// Полученный объект должен быть добавлен в описание просмотра с помощью
    /// GridProducerColumns.Add()
    /// Версия для автоматического имени поля
    /// </summary>
    /// <param name="Kind">Тип столбца (имя файла, размер, время)</param>
    /// <param name="SourceTableName">Имя таблицы в базе данных, которая хранит ссылки на файл</param>
    /// <param name="SourceColumnName">Имя поля в таблице базы данных, которое содержит ссылку на файл</param>
    public GridProducerDBFileInfoColumn(GridProducerDBFileInfoColumnKind Kind, string SourceTableName, string SourceColumnName)
      : this(Kind, SourceTableName, SourceColumnName, null)
    {
    }

  #endregion

  #region Свойства

    public string SourceTableName { get { return FSourceTableName; } }
    private string FSourceTableName;

    public string SourceColumnName { get { return FSourceColumnName; } }
    private string FSourceColumnName;

    public GridProducerDBFileInfoColumnKind Kind { get { return FKind; } }
    private GridProducerDBFileInfoColumnKind FKind;

  #endregion

  #region Переопределенные методы

    /// <summary>
    /// Требуемые поля
    /// </summary>
    public override string[] FieldNames { get { return new string[1] { SourceColumnName }; } }

    public override object GetValue(DataGridView Grid, int RowIndex, DataRow Row)
    {
      Int32 FileId = DataTools.GetInt(Row, FSourceColumnName);
      if (FileId == 0)
        return DBNull.Value;
      else
      {
        AccDepFileInfo FileInfo = AccDepClientExec.GetDBFileInfo(FSourceTableName, FSourceColumnName, FileId);
        return GetValue(FileInfo, FKind);
      }
    }

    /// <summary>
    /// Статический метод извлечения значения из структуры атрибутов файла
    /// Может быть использован отдельно из кода прикладного модуля при реализации
    /// табличных просмотров без GridProducer
    /// </summary>
    /// <param name="fi">Структура атрибутов файла</param>
    /// <param name="Kind">Тип извлекаемого значения</param>
    /// <returns>Значение</returns>
    public static object GetValue(AccDepFileInfo FileInfo, GridProducerDBFileInfoColumnKind Kind)
    {
      switch (Kind)
      {
        case GridProducerDBFileInfoColumnKind.Name:
          return FileInfo.Name;
        case GridProducerDBFileInfoColumnKind.Length:
          return FileInfo.Length;
        case GridProducerDBFileInfoColumnKind.CreationTime:
          return FileInfo.CreationTime;
        case GridProducerDBFileInfoColumnKind.LastWriteTime:
          return FileInfo.LastWriteTime;
        default:
          return DBNull.Value;
      }
    }

  #endregion
  }
#endif

  #region Перечисление EFPGridProducerSumColumnMode

  /// <summary>
  /// Вычисляемая агрегатная функция для столбца, значение в котором является вычисляемым из другимх столбцов
  /// </summary>
  public enum EFPGridProducerSumColumnMode
  {
    /// <summary>
    /// Сумма
    /// </summary>
    Sum,

    /// <summary>
    /// Минимальное значение
    /// </summary>
    Min,

    /// <summary>
    /// Максимальное значение
    /// </summary>
    Max,

    /// <summary>
    /// Среднее значение
    /// </summary>
    Average
  }

  #endregion

  /// <summary>
  /// Столбец, содержащий сумму нескольких значений.
  /// Также можно вычислить минимальное, максимальное или среднее значение
  /// Если все столбцы содержат DBNull, то возвращаемое значение также есть DBNull
  /// </summary>
  public class EFPGridProducerSumColumn : EFPGridProducerColumn
  {
    #region Конструктор

    /// <summary>
    /// Создание столбца, содержащего сумму других столбцов
    /// </summary>
    /// <param name="name">Условное имя этого столбца</param>
    /// <param name="sourceColumnNames">Имена суммируемых столбцов</param>
    public EFPGridProducerSumColumn(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      _Mode = EFPGridProducerSumColumnMode.Sum;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вычисляемая функция. По умолчанию - Sum
    /// </summary>
    public EFPGridProducerSumColumnMode Mode { get { return _Mode; } set { _Mode = value; } }
    private EFPGridProducerSumColumnMode _Mode;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Вычисляет значение
    /// </summary>
    /// <param name="args"></param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      object[] a = new object[SourceColumnNames.Length];
      for (int i = 0; i < SourceColumnNames.Length; i++)
        a[i] = args.Values.GetValue(SourceColumnNames[i]);

      switch (Mode)
      {
        case EFPGridProducerSumColumnMode.Sum:
          args.Value = DataTools.SumValue(a);
          break;
        case EFPGridProducerSumColumnMode.Min:
          args.Value = DataTools.MinValue(a);
          break;
        case EFPGridProducerSumColumnMode.Max:
          args.Value = DataTools.MaxValue(a);
          break;
        case EFPGridProducerSumColumnMode.Average:
          args.Value = DataTools.AverageValue(a);
          break;
      }

      base.OnValueNeeded(args);
    }

    #endregion
  }
}
