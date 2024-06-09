// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Data;

using FreeLibSet.Data.Docs;
using FreeLibSet.Config;

namespace FreeLibSet.Forms.Docs
{
  internal partial class YearGridFilterForm : Form
  {
    #region Конструктор

    public YearGridFilterForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["Filter"];

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpMode = new EFPRadioButtons(efpForm, rbNoFilter);
      efpYear = new EFPIntEditBox(efpForm, edYear);
      efpYear.Control.Increment = 1;
      efpYear.Minimum = 1900;
      efpYear.Maximum = 2099;
      efpYear.EnabledEx = efpMode[1].CheckedEx;
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpMode;

    public EFPIntEditBox efpYear;

    #endregion
  }

  /// <summary>
  /// Фильтр по году для числового поля или поля типа "Дата".
  /// Поддерживает "прокрутку" фильтра.
  /// </summary>
  public class YearGridFilter : YearCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для числового поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public YearGridFilter(string columnName)
      : base(columnName)
    {
    }

    /// <summary>
    /// Конструктор для поля типа "Дата" или числового поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="isDateColumn">Если true, то поле имеет тип "Дата".
    /// Если false, то поле является числовым</param>
    public YearGridFilter(string columnName, bool isDateColumn)
      : base(columnName, isDateColumn)
    {
    }

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
        if (Value.HasValue)
          return Value.ToString();
        else
          return string.Empty;
      }
    }

    /// <summary>
    /// Сохраняем год между вызовами
    /// </summary>
    private static int _PrevYear = DateTime.Today.Year;

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      YearGridFilterForm form = new YearGridFilterForm();
      form.Text = DisplayName;

      form.efpYear.Value = _PrevYear;

      if (Value.HasValue)
      {
        form.efpMode.SelectedIndex = 1;
        form.efpYear.Value = Value.Value;
      }
      else
        form.efpMode.SelectedIndex = 0;

      if (EFPApp.ShowDialog(form, true, dialogPosition) != DialogResult.OK)
        return false;

      if (form.efpMode.SelectedIndex == 0)
        Value = null;
      else
        Value = form.efpYear.Value;

      _PrevYear = form.efpYear.Value;

      return true;
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollUp
    {
      get { return Value != 0 && Value > 1; }
    }

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollDown
    {
      get { return Value != 0 && Value < 9999; }
    }

    /// <summary>
    /// Уменьшает год на 1
    /// </summary>
    public void ScrollUp()
    {
      Value--;
    }

    /// <summary>
    /// Увеличивает год на 1
    /// </summary>
    public void ScrollDown()
    {
      Value++;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр на попадание года в интервал.
  /// В таблице должно быть два числовых поля, задающих первый и последний год.
  /// Строка проходит фильтр, если заданный в фильтре год (Value) попадает в диапазон.
  /// Обрабатываются значения типа NULL, задающие открытые интервалы
  /// </summary>
  public class YearRangeInclusionGridFilter : IntRangeInclusionCommonFilter, IEFPGridFilter, IEFPScrollableGridFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для числового поля
    /// </summary>
    /// <param name="firstYearFieldName"></param>
    /// <param name="lastYearFieldName"></param>
    public YearRangeInclusionGridFilter(string firstYearFieldName, string lastYearFieldName)
      : base(firstYearFieldName, lastYearFieldName)
    {
    }

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
        if (Value.HasValue)
          return Value.Value.ToString();
        else
          return string.Empty;
      }
    }

    /// <summary>
    /// Сохраняем год между вызовами
    /// </summary>
    private static int _PrevYear = DateTime.Today.Year;

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      YearGridFilterForm form = new YearGridFilterForm();
      form.Text = DisplayName;
      form.efpYear.Value = _PrevYear;

      if (Value.HasValue)
      {
        form.efpMode.SelectedIndex = 1;
        form.efpYear.Value = Value.Value;
      }
      else
        form.efpMode.SelectedIndex = 0;

      if (EFPApp.ShowDialog(form, true, dialogPosition) != DialogResult.OK)
        return false;

      if (form.efpMode.SelectedIndex == 0)
        Value = null;
      else
        Value = form.efpYear.Value;
      _PrevYear = form.efpYear.Value;
      return true;
    }

    #endregion

    #region IScrollableGridFilter Members

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollUp
    {
      get { return Value.HasValue && Value.Value > 1; }
    }

    /// <summary>
    /// Возвращает true, если фильтр установлен
    /// </summary>
    public bool CanScrollDown
    {
      get { return Value.HasValue && Value.Value < 9999; }
    }

    /// <summary>
    /// Уменьшает год на 1
    /// </summary>
    public void ScrollUp()
    {
      Value--;
    }

    /// <summary>
    /// Увеличивает год на 1
    /// </summary>
    public void ScrollDown()
    {
      Value++;
    }

    #endregion
  }
}
