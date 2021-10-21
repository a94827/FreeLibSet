using FreeLibSet.Data.Docs;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.Controls;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

// Фильтры табличного просмотра по одному полю. В фильтрах можно задавать диапаон значений для поля.

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// Фильтр по диапазону дат для одного поля
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

    #region Переопределяемые свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FirstDate.HasValue || LastDate.HasValue)
          return DateRangeFormatter.Default.ToString(FirstDate, LastDate, true);
        else
          return null;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DateRangeDialog Dialog = new DateRangeDialog();
      Dialog.Title = DisplayName;
      Dialog.CanBeEmpty = true;

      Dialog.FirstDate = FirstDate;
      Dialog.LastDate = LastDate;

      Dialog.DialogPosition = dialogPosition;

      if (Dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return false;

      FirstDate = Dialog.FirstDate;
      LastDate = Dialog.LastDate;

      return true;
    }

    /// <summary>
    /// Использует DateRangeFormatter для преобразования в строку значения поля
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
    /// Возвращает true, если фильтр задает закрытый интервал, свойства FirstDate и LastDate установлены
    /// </summary>
    public bool CanScrollUp
    {
      get { return DoScroll(false, true); }
    }

    /// <summary>
    /// Возвращает true, если фильтр задает закрытый интервал, свойства FirstDate и LastDate установлены
    /// </summary>
    public bool CanScrollDown
    {
      get { return DoScroll(true, true); }
    }

    /// <summary>
    /// Вызывает метод DateRangeBox.ShiftDateRange() для перехода к предыдущему периоду
    /// </summary>
    public void ScrollUp()
    {
      DoScroll(false, false);
    }

    /// <summary>
    /// Вызывает метод DateRangeBox.ShiftDateRange() для перехода к следующему периоду
    /// </summary>
    public void ScrollDown()
    {
      DoScroll(true, false);
    }

    private bool DoScroll(bool forward, bool testOnly)
    {
      DateTime? dt1 = FirstDate;
      DateTime? dt2 = LastDate;
      if (!DateRangeBox.ShiftDateRange(ref dt1, ref dt2, forward))
        return false;
      if (!testOnly)
      {
        FirstDate = dt1;
        LastDate = dt2;
      }
      return true;
    }

    #endregion
  }


  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон дат.
  /// В фильтр входят строки, в диапазон дат которых попадает любая из дат в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// </summary>
  public class DateRangeCrossGridFilter : DateRangeCrossCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstDateColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastDateColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeCrossGridFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (IsEmpty)
          return String.Empty;
        else
          return DateRangeFormatter.Default.ToString(FirstDate, LastDate, true);
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DateRangeDialog Dialog = new DateRangeDialog();
      Dialog.Title = DisplayName;
      Dialog.CanBeEmpty = true;

      Dialog.FirstDate = FirstDate;
      Dialog.LastDate = LastDate;

      Dialog.DialogPosition = dialogPosition;

      if (Dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return false;

      FirstDate = Dialog.FirstDate;
      LastDate = Dialog.LastDate;

      return true;
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр задает закрытый интервал, свойства FirstDate и LastDate установлены
    /// </summary>
    public bool CanScrollUp
    {
      get { return DoScroll(false, true); }
    }

    /// <summary>
    /// Возвращает true, если фильтр задает закрытый интервал, свойства FirstDate и LastDate установлены
    /// </summary>
    public bool CanScrollDown
    {
      get { return DoScroll(true, true); }
    }

    /// <summary>
    /// Вызывает метод DateRangeBox.ShiftDateRange() для перехода к предыдущему периоду
    /// </summary>
    public void ScrollUp()
    {
      DoScroll(false, false);
    }

    /// <summary>
    /// Вызывает метод DateRangeBox.ShiftDateRange() для перехода к следующему периоду
    /// </summary>
    public void ScrollDown()
    {
      DoScroll(true, false);
    }

    private bool DoScroll(bool forward, bool testOnly)
    {
      DateTime? dt1 = FirstDate;
      DateTime? dt2 = LastDate;
      if (!DateRangeBox.ShiftDateRange(ref dt1, ref dt2, forward))
        return false;
      if (!testOnly)
      {
        FirstDate = dt1;
        LastDate = dt2;
      }
      return true;
    }

    #endregion
  }


  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего целочисленное значение.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// Для установки фильтра используется IntRangeDialog
  /// </summary>
  public class IntRangeGridFilter : IntRangeCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public IntRangeGridFilter(string columnName)
      : base(columnName)
    {
      _MinValue = Int32.MinValue;
      _MaxValue = Int32.MaxValue;
    }

    #endregion

    #region Диапазон значений для блока диалога

    /// <summary>
    /// Минимальное значение, которое можно задать в диалоге.
    /// По умолчанию - минимально возможное значение для типа даннах
    /// </summary>
    public int MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private int _MinValue;

    /// <summary>
    /// Максимальное значение, которое можно задать в диалоге.
    /// По умолчанию - максимально возможное значение для типа даннах
    /// </summary>
    public int MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private int _MaxValue;

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FirstValue.HasValue && LastValue.HasValue)
        {
          if (FirstValue.Value == LastValue.Value)
            return FirstValue.Value.ToString();
          else
            return FirstValue.Value.ToString() + " - " + LastValue.Value.ToString();
        }
        else if (FirstValue.HasValue)
          return "От " + FirstValue.Value.ToString();
        else if (LastValue.HasValue)
          return "До " + LastValue.Value.ToString();
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется IntRangeDialog
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      IntRangeDialog Dialog = new IntRangeDialog();
      Dialog.Title = DisplayName;
      Dialog.CanBeEmpty = true;
      Dialog.ImageKey = "Filter";
      Dialog.ShowNoButton = true;

      Dialog.NullableFirstValue = FirstValue;
      Dialog.NullableLastValue = LastValue;
      Dialog.CanBeEmpty = true;
      Dialog.MinValue = MinValue;
      Dialog.MaxValue = MaxValue;

      Dialog.DialogPosition = dialogPosition;

      switch (Dialog.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = Dialog.NullableFirstValue;
          LastValue = Dialog.NullableLastValue;
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
  /// Для установки фильтра используется SingleRangeDialog
  /// </summary>
  public class SingleRangeGridFilter : SingleRangeCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public SingleRangeGridFilter(string columnName)
      : base(columnName)
    {
      _MinValue = Single.MinValue;
      _MaxValue = Single.MaxValue;

      _DecimalPlaces = -1;
    }

    #endregion

    #region Диапазон значений для блока диалога

    /// <summary>
    /// Минимальное значение, которое можно задать в диалоге.
    /// По умолчанию - минимально возможное значение для типа данных
    /// </summary>
    public float MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private float _MinValue;

    /// <summary>
    /// Максимальное значение, которое можно задать в диалоге.
    /// По умолчанию - максимально возможное значение для типа данных
    /// </summary>
    public float MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private float _MaxValue;

    #endregion

    #region Форматирование

    /// <summary>
    /// Число десятичных знаков после запятой. По умолчанию: (-1) - число десятичных знаков не установлено
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Альтернативная установка свойства DecimalPlaces
    /// </summary>
    public string Format
    {
      get { return DataTools.DecimalPlacesToNumberFormat(DecimalPlaces); }
      set { DecimalPlaces = DataTools.DecimalPlacesFromNumberFormat(value); }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FirstValue.HasValue && LastValue.HasValue)
        {
          if (FirstValue.Value == LastValue.Value)
            return FirstValue.Value.ToString(Format);
          else
            return FirstValue.Value.ToString(Format) + " - " + LastValue.Value.ToString(Format);
        }
        else if (FirstValue.HasValue)
          return "От " + FirstValue.Value.ToString(Format);
        else if (LastValue.HasValue)
          return "До " + LastValue.Value.ToString(Format);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется SingleRangeDialog
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      SingleRangeDialog Dialog = new SingleRangeDialog();
      Dialog.Title = DisplayName;
      Dialog.CanBeEmpty = true;
      Dialog.ImageKey = "Filter";
      Dialog.DecimalPlaces = DecimalPlaces;

      Dialog.NullableFirstValue = FirstValue;
      Dialog.NullableLastValue = LastValue;
      Dialog.CanBeEmpty = true;
      Dialog.MinValue = MinValue;
      Dialog.MaxValue = MaxValue;
      Dialog.ShowNoButton = true;
      Dialog.DialogPosition = dialogPosition;

      switch (Dialog.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = Dialog.NullableFirstValue;
          LastValue = Dialog.NullableLastValue;
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
  /// Для установки фильтра используется DoubleRangeDialog
  /// </summary>
  public class DoubleRangeGridFilter : DoubleRangeCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public DoubleRangeGridFilter(string columnName)
      : base(columnName)
    {
      _MinValue = Double.MinValue;
      _MaxValue = Double.MaxValue;

      _DecimalPlaces = -1;
    }

    #endregion

    #region Диапазон значений для блока диалога

    /// <summary>
    /// Минимальное значение, которое можно задать в диалоге.
    /// По умолчанию - минимально возможное значение для типа данных
    /// </summary>
    public double MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private double _MinValue;

    /// <summary>
    /// Максимальное значение, которое можно задать в диалоге.
    /// По умолчанию - максимально возможное значение для типа данных
    /// </summary>
    public double MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private double _MaxValue;

    #endregion

    #region Форматирование

    /// <summary>
    /// Число десятичных знаков после запятой. По умолчанию: (-1) - число десятичных знаков не установлено
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Альтернативная установка свойства DecimalPlaces
    /// </summary>
    public string Format
    {
      get { return DataTools.DecimalPlacesToNumberFormat(DecimalPlaces); }
      set { DecimalPlaces = DataTools.DecimalPlacesFromNumberFormat(value); }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FirstValue.HasValue && LastValue.HasValue)
        {
          if (FirstValue.Value == LastValue.Value)
            return FirstValue.Value.ToString(Format);
          else
            return FirstValue.Value.ToString(Format) + " - " + LastValue.Value.ToString(Format);
        }
        else if (FirstValue.HasValue)
          return "От " + FirstValue.Value.ToString(Format);
        else if (LastValue.HasValue)
          return "До " + LastValue.Value.ToString(Format);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется DoubleRangeDialog
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DoubleRangeDialog Dialog = new DoubleRangeDialog();
      Dialog.Title = DisplayName;
      Dialog.CanBeEmpty = true;
      Dialog.ImageKey = "Filter";
      Dialog.DecimalPlaces = DecimalPlaces;

      Dialog.NullableFirstValue = FirstValue;
      Dialog.NullableLastValue = LastValue;
      Dialog.CanBeEmpty = true;
      Dialog.MinValue = MinValue;
      Dialog.MaxValue = MaxValue;
      Dialog.ShowNoButton = true;
      Dialog.DialogPosition = dialogPosition;

      switch (Dialog.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = Dialog.NullableFirstValue;
          LastValue = Dialog.NullableLastValue;
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
  /// Для установки фильтра используется SingleRangeDialog
  /// </summary>
  public class DecimalRangeGridFilter : DecimalRangeCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName"></param>
    public DecimalRangeGridFilter(string columnName)
      : base(columnName)
    {
      _MinValue = Decimal.MinValue;
      _MaxValue = Decimal.MaxValue;

      _DecimalPlaces = -1;
    }

    #endregion

    #region Диапазон значений для блока диалога

    /// <summary>
    /// Минимальное значение, которое можно задать в диалоге.
    /// По умолчанию - минимально возможное значение для типа данных
    /// </summary>
    public decimal MinValue { get { return _MinValue; } set { _MinValue = value; } }
    private decimal _MinValue;

    /// <summary>
    /// Максимальное значение, которое можно задать в диалоге.
    /// По умолчанию - максимально возможное значение для типа данных
    /// </summary>
    public decimal MaxValue { get { return _MaxValue; } set { _MaxValue = value; } }
    private decimal _MaxValue;

    #endregion

    #region Форматирование

    /// <summary>
    /// Число десятичных знаков после запятой. По умолчанию: (-1) - число десятичных знаков не установлено
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Альтернативная установка свойства DecimalPlaces
    /// </summary>
    public string Format
    {
      get { return DataTools.DecimalPlacesToNumberFormat(DecimalPlaces); }
      set { DecimalPlaces = DataTools.DecimalPlacesFromNumberFormat(value); }
    }

    #endregion

    #region Переопределяемые свойства и методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FirstValue.HasValue && LastValue.HasValue)
        {
          if (FirstValue.Value == LastValue.Value)
            return FirstValue.Value.ToString(Format);
          else
            return FirstValue.Value.ToString(Format) + " - " + LastValue.Value.ToString(Format);
        }
        else if (FirstValue.HasValue)
          return "От " + FirstValue.Value.ToString(Format);
        else if (LastValue.HasValue)
          return "До " + LastValue.Value.ToString(Format);
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра.
    /// Используется DecimalRangeDialog
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DecimalRangeDialog Dialog = new DecimalRangeDialog();
      Dialog.Title = DisplayName;
      Dialog.CanBeEmpty = true;
      Dialog.ImageKey = "Filter";
      Dialog.DecimalPlaces = DecimalPlaces;

      Dialog.NullableFirstValue = FirstValue;
      Dialog.NullableLastValue = LastValue;
      Dialog.CanBeEmpty = true;
      Dialog.MinValue = MinValue;
      Dialog.MaxValue = MaxValue;
      Dialog.ShowNoButton = true;
      Dialog.DialogPosition = dialogPosition;

      switch (Dialog.ShowDialog())
      {
        case System.Windows.Forms.DialogResult.OK:
          FirstValue = Dialog.NullableFirstValue;
          LastValue = Dialog.NullableLastValue;
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
}
