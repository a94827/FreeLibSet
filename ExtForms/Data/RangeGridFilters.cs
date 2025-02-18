// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;
using FreeLibSet.Data;

// Фильтры табличного просмотра по одному полю. В фильтрах можно задавать диапаон значений для поля.

namespace FreeLibSet.Forms.Data
{
  #region RangeGridFilter

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего целочисленное значение.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// Для установки фильтра используется <see cref="IntRangeDialog"/>.
  /// </summary>
  public class IntRangeGridFilter : IntRangeCommonFilter, IEFPGridFilter, IMinMaxSource<int?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя числового поля</param>
    public IntRangeGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public int? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private int? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public int? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private int? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<int?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<int?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public int Increment
    {
      get
      {
        IncrementUpDownHandler<int> incObj = UpDownHandler as IncrementUpDownHandler<int>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new IntUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<Int32>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="IntRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      IntRangeDialog dlg = new IntRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего число с плавающей точкой.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// Для установки фильтра используется <see cref="SingleRangeDialog"/>.
  /// </summary>
  public class SingleRangeGridFilter : SingleRangeCommonFilter, IEFPGridFilter, IMinMaxSource<float?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public SingleRangeGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public float? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private float? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public float? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private float? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве <see cref="Format"/>.
    /// Установка значения свойства создает формат.
    /// </summary>
    public int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<float?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<float?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public float Increment
    {
      get
      {
        IncrementUpDownHandler<float> incObj = UpDownHandler as IncrementUpDownHandler<float>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0f)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0f)
          UpDownHandler = null;
        else
          UpDownHandler = new SingleUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<Single>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="SingleRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      SingleRangeDialog dlg = new SingleRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего число с плавающей точкой.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// Для установки фильтра используется <see cref="DoubleRangeDialog"/>.
  /// </summary>
  public class DoubleRangeGridFilter : DoubleRangeCommonFilter, IEFPGridFilter, IMinMaxSource<double?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public DoubleRangeGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения
    /// </summary>
    public double? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private double? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения
    /// </summary>
    public double? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private double? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве <see cref="Format"/>.
    /// Установка значения свойства создает формат.
    /// </summary>
    public int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<double?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<double?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public double Increment
    {
      get
      {
        IncrementUpDownHandler<double> incObj = UpDownHandler as IncrementUpDownHandler<double>;
        if (incObj == null)
          return 0.0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0.0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0.0)
          UpDownHandler = null;
        else
          UpDownHandler = new DoubleUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<Double>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="DoubleRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DoubleRangeDialog dlg = new DoubleRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего число с плавающей точкой.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// Для установки фильтра используется <see cref="DecimalRangeDialog"/>.
  /// </summary>
  public class DecimalRangeGridFilter : DecimalRangeCommonFilter, IEFPGridFilter, IMinMaxSource<decimal?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public DecimalRangeGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения
    /// </summary>
    public decimal? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private decimal? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения
    /// </summary>
    public decimal? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private decimal? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве <see cref="Format"/>.
    /// Установка значения свойства создает формат.
    /// </summary>
    public int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<decimal?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<decimal?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public decimal Increment
    {
      get
      {
        IncrementUpDownHandler<decimal> incObj = UpDownHandler as IncrementUpDownHandler<decimal>;
        if (incObj == null)
          return 0m;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0m)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0m)
          UpDownHandler = null;
        else
          UpDownHandler = new DecimalUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<Decimal>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="DecimalRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DecimalRangeDialog dlg = new DecimalRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.Increment = Increment;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по диапазону дат для одного поля.
  /// Для установки фильтра используется <see cref="DateRangeDialog"/>.
  /// </summary>
  public class DateRangeGridFilter : DateRangeCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца типа "Дата"</param>
    public DateRangeGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public DateTime? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private DateTime? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public DateTime? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private DateTime? _Maximum;

    #endregion

    #region Переопределяемые свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FirstValue.HasValue || LastValue.HasValue)
          return DateRangeFormatter.Default.ToString(FirstValue, LastValue, true);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DateRangeDialog dlg = new DateRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;

      dlg.NFirstDate = FirstValue;
      dlg.NLastDate = LastValue;

      dlg.DialogPosition = dialogPosition;

      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return false;

      FirstValue = dlg.NFirstDate;
      LastValue = dlg.NLastDate;

      return true;
    }

    /// <summary>
    /// Использует <see cref="DateRangeFormatter"/> для преобразования в строку значения поля.
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      return new string[] { DateRangeFormatter.Default.ToString(DataTools.GetNullableDateTime(columnValues[0]), true) };
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр задает интервал, для которого можно определить предыдущий интервал.
    /// </summary>
    public bool CanScrollUp
    {
      get { return DoScroll(false, true); }
    }

    /// <summary>
    /// Возвращает true, если фильтр задает интервал, для которого можно определить следующий интервал.
    /// </summary>
    public bool CanScrollDown
    {
      get { return DoScroll(true, true); }
    }

    /// <summary>
    /// Вызывает метод <see cref="UITools.ShiftDateRange(ref DateTime?, ref DateTime?, bool)"/> для перехода к предыдущему периоду
    /// </summary>
    public void ScrollUp()
    {
      DoScroll(false, false);
    }

    /// <summary>
    /// Вызывает метод <see cref="UITools.ShiftDateRange(ref DateTime?, ref DateTime?, bool)"/> для перехода к следующему периоду
    /// </summary>
    public void ScrollDown()
    {
      DoScroll(true, false);
    }

    private bool DoScroll(bool forward, bool testOnly)
    {
      DateTime? dt1 = FirstValue;
      DateTime? dt2 = LastValue;
      if (!UITools.ShiftDateRange(ref dt1, ref dt2, forward))
        return false;
      if (!testOnly)
      {
        FirstValue = dt1;
        LastValue = dt2;
      }
      return true;
    }

    #endregion
  }

  #endregion

  #region RangeInclusionGridFilter

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает выбранное значение.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="IntInputDialog"/>.
  /// </summary>
  public class IntRangeInclusionGridFilter : IntRangeInclusionCommonFilter, IEFPGridFilter, IMinMaxSource<int?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public IntRangeInclusionGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public int? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private int? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public int? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private int? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<int?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<int?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public int Increment
    {
      get
      {
        IncrementUpDownHandler<int> incObj = UpDownHandler as IncrementUpDownHandler<int>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new IntUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.Value.ToString(Format, FormatProvider);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="IntInputDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      IntInputDialog dlg = new IntInputDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NValue = Value;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          Value = dlg.NValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          Value = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает выбранное значение.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="SingleInputDialog"/>.
  /// </summary>
  public class SingleRangeInclusionGridFilter : SingleRangeInclusionCommonFilter, IEFPGridFilter, IMinMaxSource<float?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public SingleRangeInclusionGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public float? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private float? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public float? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private float? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<float?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<float?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public float Increment
    {
      get
      {
        IncrementUpDownHandler<float> incObj = UpDownHandler as IncrementUpDownHandler<float>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new SingleUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.Value.ToString(Format, FormatProvider);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="SingleInputDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      SingleInputDialog dlg = new SingleInputDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NValue = Value;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          Value = dlg.NValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          Value = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает выбранное значение.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="DoubleInputDialog"/>.
  /// </summary>
  public class DoubleRangeInclusionGridFilter : DoubleRangeInclusionCommonFilter, IEFPGridFilter, IMinMaxSource<double?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public DoubleRangeInclusionGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public double? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private double? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public double? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private double? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<double?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<double?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public double Increment
    {
      get
      {
        IncrementUpDownHandler<double> incObj = UpDownHandler as IncrementUpDownHandler<double>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new DoubleUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.Value.ToString(Format, FormatProvider);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="DoubleInputDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DoubleInputDialog dlg = new DoubleInputDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NValue = Value;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          Value = dlg.NValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          Value = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает выбранное значение.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="DecimalInputDialog"/>.
  /// </summary>
  public class DecimalRangeInclusionGridFilter : DecimalRangeInclusionCommonFilter, IEFPGridFilter, IMinMaxSource<decimal?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public DecimalRangeInclusionGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public decimal? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private decimal? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public decimal? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private decimal? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<decimal?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<decimal?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public decimal Increment
    {
      get
      {
        IncrementUpDownHandler<decimal> incObj = UpDownHandler as IncrementUpDownHandler<decimal>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new DecimalUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.Value.ToString(Format, FormatProvider);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="DecimalInputDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DecimalInputDialog dlg = new DecimalInputDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NValue = Value;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          Value = dlg.NValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          Value = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  // Класс DateRangeInclusionGridFilter объявлен в отдельном файле.
  // Использует собственную форму редактирования фильтра, которая позволяет выбирать "рабочую дату".

  #endregion

  #region RangeCrossGridFilter

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает любое из значений в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="IntRangeDialog"/>.
  /// </summary>
  public class IntRangeCrossGridFilter : IntRangeCrossCommonFilter, IEFPGridFilter, IMinMaxSource<int?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public IntRangeCrossGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public int? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private int? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public int? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private int? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<int?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<int?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public int Increment
    {
      get
      {
        IncrementUpDownHandler<int> incObj = UpDownHandler as IncrementUpDownHandler<int>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new IntUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<Int32>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="IntRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      IntRangeDialog dlg = new IntRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает любое из значений в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="SingleRangeDialog"/>.
  /// </summary>
  public class SingleRangeCrossGridFilter : SingleRangeCrossCommonFilter, IEFPGridFilter, IMinMaxSource<float?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public SingleRangeCrossGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public float? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private float? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public float? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private float? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<float?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<float?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public float Increment
    {
      get
      {
        IncrementUpDownHandler<float> incObj = UpDownHandler as IncrementUpDownHandler<float>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new SingleUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<float>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="SingleRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      SingleRangeDialog dlg = new SingleRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает любое из значений в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="DoubleRangeDialog"/>.
  /// </summary>
  public class DoubleRangeCrossGridFilter : DoubleRangeCrossCommonFilter, IEFPGridFilter, IMinMaxSource<double?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public DoubleRangeCrossGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public double? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private double? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public double? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private double? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<double?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<double?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public double Increment
    {
      get
      {
        IncrementUpDownHandler<double> incObj = UpDownHandler as IncrementUpDownHandler<double>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new DoubleUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<double>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="DoubleRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DoubleRangeDialog dlg = new DoubleRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра по двум полям, содержащих числовые значения.
  /// В фильтр входят строки, в диапазон значений которых попадает любое из значений в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Для установки фильтра используется <see cref="DecimalRangeDialog"/>.
  /// </summary>
  public class DecimalRangeCrossGridFilter : DecimalRangeCrossCommonFilter, IEFPGridFilter, IMinMaxSource<decimal?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля, задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public DecimalRangeCrossGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public decimal? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private decimal? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public decimal? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private decimal? _Maximum;

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа.
    /// По умолчанию - пустая строка.
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
    /// Форматировщик для числового значения.
    /// По умолчанию используется форматировщик для текущей культуры.
    /// </summary>
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return System.Globalization.CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    #endregion

    #region Increment

    /// <summary>
    /// Специальная реализация прокрутки значения стрелочками вверх и вниз.
    /// Если null, то прокрутки нет.
    /// Обычно следует использовать свойство <see cref="Increment"/>, если не требуется специальная реализация прокрутки.
    /// </summary>
    public IUpDownHandler<decimal?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set { _UpDownHandler = value; }
    }
    private IUpDownHandler<decimal?> _UpDownHandler;

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную.
    /// Это свойство дублирует <see cref="UpDownHandler"/>.
    /// </summary>
    public decimal Increment
    {
      get
      {
        IncrementUpDownHandler<decimal> incObj = UpDownHandler as IncrementUpDownHandler<decimal>;
        if (incObj == null)
          return 0;
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value == 0)
          UpDownHandler = null;
        else
          UpDownHandler = new DecimalUpDownHandler(value, this);
      }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        return FormatStringTools.RangeToString<Decimal>(FirstValue, LastValue, Format, FormatProvider);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется <see cref="DecimalRangeDialog"/>.
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DecimalRangeDialog dlg = new DecimalRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.ImageKey = "Filter";
      dlg.NFirstValue = FirstValue;
      dlg.NLastValue = LastValue;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Format = Format;
      dlg.FormatProvider = FormatProvider;
      dlg.UpDownHandler = UpDownHandler;
      dlg.DialogPosition = dialogPosition;

      switch (dlg.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = dlg.NFirstValue;
          LastValue = dlg.NLastValue;
          return true;
        case System.Windows.Forms.DialogResult.No:
          FirstValue = null;
          LastValue = null;
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон дат.
  /// В фильтр входят строки, в диапазон дат которых попадает любая из дат в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// </summary>
  public class DateRangeCrossGridFilter : DateRangeCrossCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter, IMinMaxSource<DateTime?>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeCrossGridFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public DateTime? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private DateTime? _Minimum;

    /// <summary>
    /// Мaксимальное значение, которое можно задавать в блоке диалога.
    /// По умолчанию - null - нет ограничения.
    /// </summary>
    public DateTime? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private DateTime? _Maximum;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (IsEmpty)
          return String.Empty;
        else
          return DateRangeFormatter.Default.ToString(FirstValue, LastValue, true);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DateRangeDialog dlg = new DateRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmpty = true;
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;

      dlg.NFirstDate = FirstValue;
      dlg.NLastDate = LastValue;

      dlg.DialogPosition = dialogPosition;

      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return false;

      FirstValue = dlg.NFirstDate;
      LastValue = dlg.NLastDate;

      return true;
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр задает интервал, для которого можно определить предыдущий интервал.
    /// </summary>
    public bool CanScrollUp
    {
      get { return DoScroll(false, true); }
    }

    /// <summary>
    /// Возвращает true, если фильтр задает интервал, для которого можно определить следующий интервал.
    /// </summary>
    public bool CanScrollDown
    {
      get { return DoScroll(true, true); }
    }

    /// <summary>
    /// Вызывает метод <see cref="UITools.ShiftDateRange(ref DateTime?, ref DateTime?, bool)"/> для перехода к предыдущему периоду
    /// </summary>
    public void ScrollUp()
    {
      DoScroll(false, false);
    }

    /// <summary>
    /// Вызывает метод <see cref="UITools.ShiftDateRange(ref DateTime?, ref DateTime?, bool)"/> для перехода к следующему периоду
    /// </summary>
    public void ScrollDown()
    {
      DoScroll(true, false);
    }

    private bool DoScroll(bool forward, bool testOnly)
    {
      DateTime? dt1 = FirstValue;
      DateTime? dt2 = LastValue;
      if (!UITools.ShiftDateRange(ref dt1, ref dt2, forward))
        return false;
      if (!testOnly)
      {
        FirstValue = dt1;
        LastValue = dt2;
      }
      return true;
    }

    #endregion
  }

  #endregion
}
