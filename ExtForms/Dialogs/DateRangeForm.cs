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
      //EFPApp.InitFormImages(this);

      FormProvider = new EFPFormProvider(this);
      TheDateRangeBox = new EFPDateRangeBox(FormProvider, edRange);
    }

    public readonly EFPFormProvider FormProvider;

    public readonly EFPDateRangeBox TheDateRangeBox;
  }

  /// <summary>
  /// Диалог редактирования интервала дат с помощью элемента <see cref="EFPDateRangeBox"/>.
  /// </summary>
  public class DateRangeDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует значения свойств по умолчанию
    /// </summary>
    public DateRangeDialog()
    {
      _NFirstValue = null;
      _NLastValue = null;
      _CanBeEmptyMode = UIValidateState.Error;
      _CanBeHalfEmpty = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога по умолчанию
    /// </summary>
    protected override string DefaultTitle { get { return Res.DateRangeDialog_Msg_Title; } }

    /// <summary>
    /// Подсказка по умолчанию
    /// </summary>
    protected override string DefaultPrompt { get { return Res.DateRangeDialog_Msg_Prompt; } }

    #region Текущее значение

    #region NFirstValue

    /// <summary>
    /// Вход и выход: Начальная дата диапазона.
    /// Значение null после завершения диалога может быть только при <see cref="CanBeEmpty"/>=true.
    /// </summary>
    public DateTime? NFirstValue
    {
      get { return _NFirstValue; }
      set
      {
        if (value.HasValue)
          _NFirstValue = value.Value.Date;
        else
          _NFirstValue = null;
        if (_NFirstValueEx != null)
          _NFirstValueEx.OwnerSetValue(NFirstValue);
        if (_FirstValueEx != null)
          _FirstValueEx.OwnerSetValue(FirstValue);
      }
    }
    private DateTime? _NFirstValue;

    /// <summary>
    /// Управляемое свойство для <see cref="NFirstValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime?> NFirstValueEx
    {
      get
      {
        if (_NFirstValueEx == null)
        {
          _NFirstValueEx = new DepOutput<DateTime?>(NFirstValue);
          _NFirstValueEx.OwnerInfo = new DepOwnerInfo(this, "NFirstValueEx");
        }
        return _NFirstValueEx;
      }
    }
    private DepOutput<DateTime?> _NFirstValueEx;

    #endregion

    #region NLastValue

    /// <summary>
    /// Вход и выход: Конечная дата диапазона.
    /// Значение null после завершения диалога может быть только при <see cref="CanBeEmpty"/>=true.
    /// </summary>
    public DateTime? NLastValue
    {
      get { return _NLastValue; }
      set
      {
        if (value.HasValue)
          _NLastValue = value.Value.Date;
        else
          _NLastValue = null;

        if (_NLastValueEx != null)
          _NLastValueEx.OwnerSetValue(NLastValue);
        if (_LastValueEx != null)
          _LastValueEx.OwnerSetValue(LastValue);
      }
    }
    private DateTime? _NLastValue;

    /// <summary>
    /// Управляемое свойство для <see cref="NLastValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime?> NLastValueEx
    {
      get
      {
        if (_NLastValueEx == null)
        {
          _NLastValueEx = new DepOutput<DateTime?>(NLastValue);
          _NLastValueEx.OwnerInfo = new DepOwnerInfo(this, "NLastValueEx");
        }
        return _NLastValueEx;
      }
    }
    private DepOutput<DateTime?> _NLastValueEx;

    #endregion

    #region FirstValue

    /// <summary>
    /// Первое редактируемое значение. Пустое значение заменяется на минимально возможную дату.
    /// </summary>
    public DateTime FirstValue
    {
      get { return NFirstValue ?? DateRange.Whole.FirstDate; }
      set { NFirstValue = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="FirstValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime> FirstValueEx
    {
      get
      {
        if (_FirstValueEx == null)
        {
          _FirstValueEx = new DepOutput<DateTime>(FirstValue);
          _FirstValueEx.OwnerInfo = new DepOwnerInfo(this, "FirstValueEx");
        }
        return _FirstValueEx;
      }
    }
    private DepOutput<DateTime> _FirstValueEx;

    #endregion

    #region LastValue

    /// <summary>
    /// Второе редактируемое значение. Пустое значение заменяется на максимально возможную дату.
    /// </summary>
    public DateTime LastValue
    {
      get { return NLastValue ?? DateRange.Whole.LastDate; }
      set { NLastValue = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="LastValue"/>.
    /// Только для чтения. Может использоваться в валидаторах.
    /// </summary>
    public DepValue<DateTime> LastValueEx
    {
      get
      {
        if (_LastValueEx == null)
        {
          _LastValueEx = new DepOutput<DateTime>(LastValue);
          _LastValueEx.OwnerInfo = new DepOwnerInfo(this, "LastValueEx");
        }
        return _LastValueEx;
      }
    }
    private DepOutput<DateTime> _LastValueEx;

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство возвращает true, если обе даты диапазона заполнены (<see cref="NFirstValue"/>.HasValue=true и <see cref="NLastValue"/>.HasValue=true).
    /// Может использоваться в валидаторах.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepExpr2<bool, DateTime?, DateTime?>(NFirstValueEx, NLastValueEx, CalcIsNotEmptyEx);
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
    /// <summary>
    /// Допускаются ли полуоткрытые интервалы.
    /// По умолчанию - true - допускаются.
    /// Свойство имеет смысл при <see cref="EFPDateTimeControl{DateTimeBox}.ControlCanBeEmpty"/>, отличном
    /// от <see cref="UIValidateState.Error"/>, когда допустимы полностью открыты интервалы.
    /// Если false, то выдается сообщение об ошибке, когда одно поле заполнено, а второе - нет
    /// </summary>
    public bool CanBeHalfEmpty
    {
      get { return _CanBeHalfEmpty; }
      set { _CanBeHalfEmpty = value; }
    }
    private bool _CanBeHalfEmpty;

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
      frm.TheDateRangeBox.CanBeHalfEmpty = CanBeHalfEmpty;

      if (HasConfig)
      {
        _NFirstValue = ConfigPart.GetNullableDate(ConfigName + "-FirstValue");
        _NLastValue = ConfigPart.GetNullableDate(ConfigName + "-LastValue");
      }

      frm.TheDateRangeBox.First.NValue = _NFirstValue;
      frm.TheDateRangeBox.Last.NValue = _NLastValue;

      EFPFormCheck fc = new EFPFormCheck();
      frm.FormProvider.FormChecks.Add(fc);
      fc.Validating += FormCheck;
      fc.Tag = frm;

      if (EFPApp.ShowDialog(frm, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      _NFirstValue = frm.TheDateRangeBox.First.NValue;
      _NLastValue = frm.TheDateRangeBox.Last.NValue;

      if (HasConfig)
      {
        ConfigPart.SetNullableDate(ConfigName + "-FirstValue", _NFirstValue);
        ConfigPart.SetNullableDate(ConfigName + "-LastValue", _NLastValue);
      }

      return DialogResult.OK;
    }

    private void FormCheck(object sender, UIValidatingEventArgs args)
    {
      EFPFormCheck formCheck = (EFPFormCheck)sender;
      DateRangeForm form = (DateRangeForm)(formCheck.Tag);
      NFirstValue = form.TheDateRangeBox.First.NValue;
      NLastValue = form.TheDateRangeBox.Last.NValue;

      if (HasValidators)
        Validators.Validate(args);
    }

    #endregion
  }
}
