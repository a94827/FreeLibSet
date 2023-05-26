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
using FreeLibSet.DependedValues;
using FreeLibSet.Config;
using FreeLibSet.Data.Docs;
using FreeLibSet.Calendar;

namespace FreeLibSet.Forms.Docs
{
  internal partial class DateRangeInclusionGridFilterForm : Form
  {
    #region Конструктор

    public DateRangeInclusionGridFilterForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["Filter"];
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpMode = new EFPRadioButtons(efpForm, rbNone);

      efpDate = new EFPDateTimeBox(efpForm, edDate);
      efpDate.CanBeEmpty = false;
      efpDate.DisabledValue = DateTime.Today;
      efpDate.AllowDisabledValue = true;
      efpDate.EnabledEx = efpMode[2].CheckedEx;
      efpDate.VisibleEx = new DepNot(efpMode[0].CheckedEx);
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpMode;

    public EFPDateTimeBox efpDate;

    #endregion
  }

  /// <summary>
  /// Фильтр по интервалу дат
  /// В таблице должно быть два поля типа даты, которые составляют интервал дат.
  /// В фильтре задается дата. В просмотр попадают строки, в которых интервал дат
  /// включает в себя эту дату. Обрабатываются открытые и полуоткрытые интервалы,
  /// когда одно или оба поля содержат NULL
  /// Эта версия фильтра поддерживает только текущую системную дату
  /// </summary>
  public class DateRangeInclusionGridFilter : DateRangeInclusionCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstDateColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastDateColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeInclusionGridFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
    }

    #endregion

    #region Свойства, которые можно переопределить для использования рабочей даты

    /// <summary>
    /// Непереопределенный метод возвращает "Текущая дата".
    /// Если производный класс переопределяет свойство WorkData, то он, вероятно,
    /// будет переопределять и это свойство
    /// </summary>
    public virtual string WorkDateText { get { return "Текущая дата"; } }

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
        if (Date.HasValue)
        {
          string s = DateRangeFormatter.Default.ToString(Date.Value, true);
          if (UseWorkDate)
            s += " (" + WorkDateText + ")";
          return s;
        }
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      DateRangeInclusionGridFilterForm form = new DateRangeInclusionGridFilterForm();
      form.Text = DisplayName;
      form.efpMode[1].Control.Text = WorkDateText;
      form.efpDate.DisabledValue = WorkDate;


      if (Date.HasValue)
      {
        if (UseWorkDate)
          form.efpMode.SelectedIndex = 1;
        else
        {
          form.efpMode.SelectedIndex = 2;
          form.efpDate.NValue = Date;
        }
      }
      else
        form.efpMode.SelectedIndex = 0;

      if (EFPApp.ShowDialog(form, true, dialogPosition) != DialogResult.OK)
        return false;
      switch (form.efpMode.SelectedIndex)
      {
        case 0:
          Date = null;
          break;
        case 1:
          UseWorkDate = true;
          Date = WorkDate;
          break;
        case 2:
          UseWorkDate = false;
          Date = form.efpDate.NValue;
          break;
      }
      return true;
    }

    #endregion
  }
}
