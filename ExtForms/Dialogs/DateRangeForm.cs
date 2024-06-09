// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.UICore;
using FreeLibSet.Calendar;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms
{
  internal partial class DateRangeForm : Form
  {
    public DateRangeForm()
    {
      InitializeComponent();
      EFPApp.InitFormImages(this);

      FormProvider = new EFPFormProvider(this);
      TheDateRangeBox = new EFPDateRangeBox(FormProvider, edRange);
    }

    public readonly EFPFormProvider FormProvider;

    public readonly EFPDateRangeBox TheDateRangeBox;
  }

  /// <summary>
  /// Диалог редактирования интервала дат с помощью элемента <see cref="EFPDateRangeBox"/>.
  /// </summary>
  public class DateRangeDialog: BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует значения свойств по умолчанию
    /// </summary>
    public DateRangeDialog()
    {
      Title = "Интервал дат";
      Prompt = String.Empty;
      _NFirstDate = null;
      _NLastDate = null;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Текущее значение

    #region NFirstDate

    /// <summary>
    /// Вход и выход: Начальная дата диапазона.
    /// Значение null после завершения диалога может быть только при <see cref="CanBeEmpty"/>=true.
    /// </summary>
    public DateTime? NFirstDate
    {
      get { return _NFirstDate; }
      set
      {
        if (value.HasValue)
          _NFirstDate = value.Value.Date;
        else
          _NFirstDate = null;
        if (_NFirstDateEx != null)
          _NFirstDateEx.OwnerSetValue(NFirstDate);
        if (_FirstDateEx != null)
          _FirstDateEx.OwnerSetValue(FirstDate);
      }
    }
    private DateTime? _NFirstDate;

    /// <summary>
    /// Управляемое свойство для <see cref="NFirstDate"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime?> NFirstDateEx
    {
      get
      {
        if (_NFirstDateEx == null)
        {
          _NFirstDateEx = new DepOutput<DateTime?>(NFirstDate);
          _NFirstDateEx.OwnerInfo = new DepOwnerInfo(this, "NFirstDateEx");
        }
        return _NFirstDateEx;
      }
    }
    private DepOutput<DateTime?> _NFirstDateEx;

    #endregion

    #region NLastDate

    /// <summary>
    /// Вход и выход: Конечная дата диапазона.
    /// Значение null после завершения диалога может быть только при <see cref="CanBeEmpty"/>=true.
    /// </summary>
    public DateTime? NLastDate
    {
      get { return _NLastDate; }
      set
      {
        if (value.HasValue)
          _NLastDate = value.Value.Date;
        else
          _NLastDate = null;

        if (_NLastDateEx != null)
          _NLastDateEx.OwnerSetValue(NLastDate);
        if (_LastDateEx != null)
          _LastDateEx.OwnerSetValue(LastDate);
      }
    }
    private DateTime? _NLastDate;

    /// <summary>
    /// Управляемое свойство для <see cref="NLastDate"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime?> NLastDateEx
    {
      get
      {
        if (_NLastDateEx == null)
        {
          _NLastDateEx = new DepOutput<DateTime?>(NLastDate);
          _NLastDateEx.OwnerInfo = new DepOwnerInfo(this, "NLastDateEx");
        }
        return _NLastDateEx;
      }
    }
    private DepOutput<DateTime?> _NLastDateEx;

    #endregion

    #region FirstDate

    /// <summary>
    /// Первое редактируемое значение. Пустое значение заменяется на минимально возможную дату.
    /// </summary>
    public DateTime FirstDate
    {
      get { return NFirstDate ?? DateRange.Whole.FirstDate; }
      set { NFirstDate = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="FirstDate"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime> FirstDateEx
    {
      get
      {
        if (_FirstDateEx == null)
        {
          _FirstDateEx = new DepOutput<DateTime>(FirstDate);
          _FirstDateEx.OwnerInfo = new DepOwnerInfo(this, "FirstDateEx");
        }
        return _FirstDateEx;
      }
    }
    private DepOutput<DateTime> _FirstDateEx;

    #endregion

    #region LastDate

    /// <summary>
    /// Второе редактируемое значение. Пустое значение заменяется на максимально возможную дату.
    /// </summary>
    public DateTime LastDate
    {
      get { return NLastDate ?? DateRange.Whole.LastDate; }
      set { NLastDate = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="LastDate"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime> LastDateEx
    {
      get
      {
        if (_LastDateEx == null)
        {
          _LastDateEx = new DepOutput<DateTime>(LastDate);
          _LastDateEx.OwnerInfo = new DepOwnerInfo(this, "LastDateEx");
        }
        return _LastDateEx;
      }
    }
    private DepOutput<DateTime> _LastDateEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство возвращает true, если обе даты диапазона заполнены (<see cref="NFirstDate"/>.HasValue=true и <see cref="NLastDate"/>.HasValue=true).
    /// Может использоваться в валидаторах.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepExpr2<bool, DateTime?, DateTime?>(NFirstDateEx, NLastDateEx, CalcIsNotEmptyEx);
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmptyEx(DateTime? firstDate, DateTime? lastDate)
    {
      return firstDate.HasValue && lastDate.HasValue;
    }

    #endregion

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode { get { return _CanBeEmptyMode; } set { _CanBeEmptyMode = value; } }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// Если <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/>, то возвращается true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Диапазон допустимых значений

    /// <summary>
    /// Если задано значение, отличное от null, то не разрешается вводить даты, ранее указанной.
    /// </summary>
    public DateTime? Minimum { get { return _Minimum; } set { _Minimum = value; } }
    private DateTime? _Minimum;

    /// <summary>
    /// Если задано значение, отличное от null, то не разрешается вводить даты, позднее указанной.
    /// </summary>
    public DateTime? Maximum { get { return _Maximum; } set { _Maximum = value; } }
    private DateTime? _Maximum;

    #endregion

    #endregion

    #region Показ диалога

    /// <summary>
    /// Показывает блок диалога.
    /// </summary>
    /// <returns>Результат выполнения</returns>
    public override DialogResult ShowDialog()
    {
      DateRangeForm frm = new DateRangeForm();
      base.InitFormTitle(frm);
      frm.FormProvider.HelpContext = HelpContext;
      frm.MainLabel.Text = Prompt;

      frm.TheDateRangeBox.First.CanBeEmptyMode = CanBeEmptyMode;
      frm.TheDateRangeBox.Last.CanBeEmptyMode = CanBeEmptyMode;
      frm.TheDateRangeBox.First.Minimum = Minimum;
      frm.TheDateRangeBox.Last.Minimum = Minimum;
      frm.TheDateRangeBox.First.Maximum = Maximum;
      frm.TheDateRangeBox.Last.Maximum = Maximum;

      if (HasConfig)
      {
        _NFirstDate = ConfigPart.GetNullableDate(ConfigName + "-FirstDate");
        _NLastDate = ConfigPart.GetNullableDate(ConfigName + "-LastDate");
      }

      frm.TheDateRangeBox.First.NValue = _NFirstDate;
      frm.TheDateRangeBox.Last.NValue = _NLastDate;

      EFPFormCheck fc = new EFPFormCheck(frm.FormProvider);
      fc.Validating += FormCheck;
      fc.Tag = frm;

      if (EFPApp.ShowDialog(frm, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      _NFirstDate = frm.TheDateRangeBox.First.NValue;
      _NLastDate = frm.TheDateRangeBox.Last.NValue;

      if (HasConfig)
      {
        ConfigPart.SetNullableDate(ConfigName + "-FirstDate", _NFirstDate);
        ConfigPart.SetNullableDate(ConfigName + "-LastDate", _NLastDate);
      }

      return DialogResult.OK;
    }

    private void FormCheck(object sender, UIValidatingEventArgs args)
    {
      EFPFormCheck formCheck = (EFPFormCheck)sender;
      DateRangeForm form = (DateRangeForm)(formCheck.Tag);
      NFirstDate = form.TheDateRangeBox.First.NValue;
      NLastDate = form.TheDateRangeBox.Last.NValue;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }
}
