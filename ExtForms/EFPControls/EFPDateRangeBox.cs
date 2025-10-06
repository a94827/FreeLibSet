// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Forms;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Calendar;
using System.Diagnostics;
using FreeLibSet.Formatting;
using FreeLibSet.Core;
using System.Drawing;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Проверка правильности интервала дат для двух управляющих элементов
  /// </summary>
  public sealed class EFPDateBoxRangeCheck
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// Далее должны быть установлены свойства <see cref="FirstValueBox"/> и <see cref="LastValueBox"/>.
    /// </summary>
    public EFPDateBoxRangeCheck()
    {
      DoInit();
    }

    /// <summary>
    /// Создает объект с заданными свойствами <see cref="FirstValueBox"/> и <see cref="LastValueBox"/>.
    /// </summary>
    /// <param name="firstValueBox">Провайдер поля ввода начальной даты диапазона</param>
    /// <param name="lastValueBox">Провайдер поля ввода конечной даты диапазона</param>
    public EFPDateBoxRangeCheck(EFPDateTimeBox firstValueBox, EFPDateTimeBox lastValueBox)
    {
      DoInit();
      this.FirstValueBox = firstValueBox;
      this.LastValueBox = lastValueBox;
    }

    internal EFPDateBoxRangeCheck(EFPDateTimeBox firstDateBox, EFPDateTimeBox lastDateBox, EFPDateRangeBox owner)
    {
      DoInit();
      this.FirstValueBox = firstDateBox;
      this.LastValueBox = lastDateBox;
      _Owner = owner;
    }

    private void DoInit()
    {
      _FirstValueInput = new DepInput<DateTime?>(null, FirstValueInput_ValueChanged);
      _FirstValueInput.OwnerInfo = new DepOwnerInfo(this, "FirstValueInput");

      _LastValueInput = new DepInput<DateTime?>(null, LastValueInput_ValueChanged);
      _LastValueInput.OwnerInfo = new DepOwnerInfo(this, "LastValueInput");


      _CanBeHalfEmpty = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для поля ввода начальной даты
    /// </summary>
    public EFPDateTimeBox FirstValueBox
    {
      get { return _FirstValueBox; }
      set
      {
        if (value == _FirstValueBox)
          return;

        if (_FirstValueBox != null)
          _FirstValueBox.Validating -= new UIValidatingEventHandler(DoValidate);
        _FirstValueBox = value;
        if (_FirstValueBox != null)
          _FirstValueBox.Validating += new UIValidatingEventHandler(DoValidate);

        if (value == null)
          _FirstValueInput.Source = null;
        else
          _FirstValueInput.Source = value.NValueEx;
      }
    }

    private EFPDateTimeBox _FirstValueBox;

    /// <summary>
    /// Провайдер для поля ввода конечной даты
    /// </summary>
    public EFPDateTimeBox LastValueBox
    {
      get { return _LastValueBox; }
      set
      {
        if (value == _LastValueBox)
          return;

        if (_LastValueBox != null)
          _LastValueBox.Validating -= new UIValidatingEventHandler(DoValidate);
        _LastValueBox = value;
        if (_LastValueBox != null)
          _LastValueBox.Validating += new UIValidatingEventHandler(DoValidate);

        if (value == null)
          _LastValueInput.Source = null;
        else
          _LastValueInput.Source = value.NValueEx;
      }
    }

    private EFPDateTimeBox _LastValueBox;

    private readonly EFPDateRangeBox _Owner;


    /// <summary>
    /// Допускаются ли полуоткрытые интервалы.
    /// По умолчанию - true - допускаются.
    /// Свойство имеет смысл при <see cref="EFPDateTimeControl{DateTimeBox}.ControlCanBeEmpty"/>, отличном
    /// от <see cref="UIValidateState.Error"/>, когда допустимы полностью открыты интервалы.
    /// Если false, то выдается сообщение об ошибке, когда одно поле заполнено, а второе - нет.
    /// </summary>
    public bool CanBeHalfEmpty
    {
      get { return _CanBeHalfEmpty; }
      set
      {
        if (value == _CanBeHalfEmpty)
          return;
        _CanBeHalfEmpty = value;
        if (FirstValueBox != null && LastValueBox != null)
        {
          FirstValueBox.Validate();
          LastValueBox.Validate();
        }
      }
    }
    private bool _CanBeHalfEmpty;

    #endregion

    #region Внутренняя реализация

    private DepInput<Nullable<DateTime>> _FirstValueInput;
    private DepInput<Nullable<DateTime>> _LastValueInput;

    void FirstValueInput_ValueChanged(object sender, EventArgs args)
    {
      if (LastValueBox != null)
        LastValueBox.Validate();
    }

    void LastValueInput_ValueChanged(object sender, EventArgs args)
    {
      if (FirstValueBox != null)
        FirstValueBox.Validate();
    }


    private void DoValidate(object sender, UIValidatingEventArgs args)
    {
      if (_Owner != null)
        _Owner.Validate();

      if (_FirstValueInput.Value.HasValue && _LastValueInput.Value.HasValue)
      {
        if (FirstValueBox.Enabled && LastValueBox.Enabled)
        {
          if (_FirstValueInput.Value.Value > _LastValueInput.Value.Value)
            args.SetError(Res.DateRangeBox_Err_Inverted);
        }
      }
      else if ((!CanBeHalfEmpty) && args.ValidateState != UIValidateState.Error)
      {
        DepInput<Nullable<DateTime>> thisValueInput, otherValueInput;
        if (object.ReferenceEquals(sender, FirstValueBox))
        {
          thisValueInput = _FirstValueInput;
          otherValueInput = _LastValueInput;
        }
        else if (object.ReferenceEquals(sender, LastValueBox))
        {
          thisValueInput = _LastValueInput;
          otherValueInput = _FirstValueInput;
        }
        else
          return; // вдруг отсоединилось
        if ((!thisValueInput.Value.HasValue) && (otherValueInput.Value.HasValue))
          args.SetError(Res.DateRangeBox_Err_IsHalfEmpty);
      }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для <see cref="DateRangeBox"/>.
  /// Содержит два "вложенных" провайдера <see cref="EFPDateTimeBox"/> для ввода начальной и конечной даты.
  /// </summary>
  public class EFPDateRangeBox : EFPSyncControl<DateRangeBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateRangeBox(EFPBaseProvider baseProvider, DateRangeBox control)
      : base(new EFPDateRangeBoxBaseProvider(), control, true)
    {
      this.BaseProvider.Parent = baseProvider;
      ((EFPDateRangeBoxBaseProvider)(this.BaseProvider)).ControlProvider = this;

      _First = new EFPDateTimeBox(this.BaseProvider, control.First);
      _First.DisplayName = Res.DateRangeBox_Name_First;
      _Last = new EFPDateTimeBox(this.BaseProvider, control.Last);
      _Last.DisplayName = Res.DateRangeBox_Name_Last;

      _RangeCheck = new EFPDateBoxRangeCheck(_First, _Last, this);

      _First.NValueEx.ValueChanged += new EventHandler(FirstLast_NValueChanged);
      _Last.NValueEx.ValueChanged += new EventHandler(FirstLast_NValueChanged);
      _First.EditableEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _Last.EditableEx.ValueChanged += new EventHandler(InitCommandItemsState);


      if (!DesignMode)
        control.MenuButtonClicked += new System.ComponentModel.CancelEventHandler(Control_MenuButtonClicked);
    }

    private bool _Inside_FirstLast_NValueChanged;

    private void FirstLast_NValueChanged(object sender, EventArgs args)
    {
      InitCommandItemsState(null, null);

      if (_Inside_FirstLast_NValueChanged)
        return;
      _Inside_FirstLast_NValueChanged = true;
      try
      {
        if (_DateRangeEx != null)
          _DateRangeEx.Value = this.DateRange;
      }
      finally
      {
        _Inside_FirstLast_NValueChanged = false;
      }
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Провайдер поля ввода начальной даты диапазона.
    /// </summary>
    public EFPDateTimeBox First { get { return _First; } }
    private readonly EFPDateTimeBox _First;

    /// <summary>
    /// Провайдер поля ввода конечной даты диапазона.
    /// </summary>
    public EFPDateTimeBox Last { get { return _Last; } }
    private readonly EFPDateTimeBox _Last;

    #endregion

    #region DateRange

    /// <summary>
    /// Выбранный период.
    /// Свойство возвращает непустое значение, если обе даты заполнены и конечная дата не меньше начальной.
    /// Возвращает <see cref="FreeLibSet.Calendar.DateRange.Empty"/>, если одна или обе даты не заполнены, или начальная дата больше конечной.
    /// Установка значения инициализирует обе даты. Если передано <see cref="FreeLibSet.Calendar.DateRange.Empty"/>, обе даты очищаются.
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        if (First.NValue.HasValue && Last.NValue.HasValue && First.Value <= Last.Value)
          return new DateRange(First.Value, Last.Value);
        else
          return DateRange.Empty;
      }
      set
      {
        if (value.IsEmpty)
        {
          First.NValue = null;
          Last.NValue = null;
        }
        else
        {
          First.NValue = value.FirstDate;
          Last.NValue = value.LastDate;
        }
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DateRange"/>
    /// </summary>
    public DepValue<DateRange> DateRangeEx
    {
      get
      {
        InitDateRangeEx();
        return _DateRangeEx;
      }
      set
      {
        InitDateRangeEx();
        _DateRangeEx.Source = value;
      }
    }
    private DepInput<DateRange> _DateRangeEx;

    private void InitDateRangeEx()
    {
      if (_DateRangeEx == null)
      {
        _DateRangeEx = new DepInput<DateRange>(this.DateRange, DateRangeEx_ValueChanged);
        _DateRangeEx.OwnerInfo = new DepOwnerInfo(this, "DateRangeEx");
      }
    }

    private void DateRangeEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_Inside_FirstLast_NValueChanged)
        DateRange = _DateRangeEx.Value;
    }

    #endregion

    #region Диапазон допустимых значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Дублирует свойство <see cref="EFPDateTimeControl{DateTimeBox}.Minimum"/> для обеих дат <see cref="First"/> и <see cref="Last"/>.
    /// По умолчанию ограничение не установлено.
    /// </summary>
    public DateTime? Minimum
    {
      get { return First.Minimum; }
      set
      {
        First.Minimum = value;
        Last.Minimum = value;
      }
    }

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Дублирует свойство <see cref="EFPDateTimeControl{DateTimeBox}.Maximum"/> для обеих дат <see cref="First"/> и <see cref="Last"/>.
    /// По умолчанию ограничение не установлено.
    /// </summary>
    public DateTime? Maximum
    {
      get { return Last.Maximum; }
      set
      {
        First.Maximum = value;
        Last.Maximum = value;
      }
    }

    private readonly EFPDateBoxRangeCheck _RangeCheck;

    /// <summary>
    /// Допускаются ли полуоткрытые интервалы.
    /// По умолчанию - true - допускаются.
    /// Свойство имеет смысл при <see cref="EFPDateTimeControl{DateTimeBox}.ControlCanBeEmpty"/>, отличном
    /// от <see cref="UIValidateState.Error"/>, когда допустимы полностью открыты интервалы.
    /// </summary>
    public bool CanBeHalfEmpty { get { return _RangeCheck.CanBeHalfEmpty; } set { _RangeCheck.CanBeHalfEmpty = value; } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Доинициализирует всплывающие подсказки для кнопок 
    /// </summary>
    protected override void OnCreated()
    {
      // 05.10.2025
      EFPCommandItem ci1 = CommandItems["Edit", "PrevPeriod"];
      if (ci1.ShortCut != Keys.None)
        Control.TheLeftButton.ToolTipText += " (" + ci1.ShortCutText + ")";
      EFPCommandItem ci2 = CommandItems["Edit", "NextPeriod"];
      if (ci2.ShortCut != Keys.None)
        Control.TheRightButton.ToolTipText += " (" + ci2.ShortCutText + ")";

      base.OnCreated();
    }

    /// <summary>
    /// Всплывающая подсказка.
    /// Для элементов задаются подсказки "ToolTipText - начальная дата" и "ToolTipText - конечная дата".
    /// </summary>
    public override string ToolTipText
    {
      get
      {
        return base.ToolTipText;
      }
      set
      {
        base.ToolTipText = value;
        string s = String.Empty;
        if (!String.IsNullOrEmpty(value))
          s = value + " - ";
        First.ToolTipText = s + Res.DateRangeBox_Name_First;
        Last.ToolTipText = s + Res.DateRangeBox_Name_Last;
      }
    }

    private bool _InsideOnValidate;

    /// <summary>
    /// Проверка корректности.
    /// Вызывает метод проверки для "вложенных" провайдеров.
    /// </summary>
    protected override void OnValidate()
    {
      if (_InsideOnValidate)
        return;
      _InsideOnValidate = true;
      try
      {
        base.OnValidate();
        First.Validate();
        Last.Validate();
      }
      finally
      {
        _InsideOnValidate = false;
      }
    }

    /// <summary>
    /// Синхронизированное значение типа <see cref="DateRange"/>
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return DateRange;
      }
      set
      {
        DateRange = (DateRange)value;
      }
    }

    #endregion

    #region Команды меню

    private class EFPDateRangeBoxBaseProvider : EFPBaseProvider
    {
      #region Доступ к меню

      public EFPDateRangeBox ControlProvider;

      public override void InitCommandItemList(List<EFPCommandItems> list)
      {
        if (ControlProvider != null)
        {
          if (ControlProvider.CommandItems.Count > 0)
            list.Add(ControlProvider.CommandItems);
        }
        base.InitCommandItemList(list);
      }

      #endregion
    }

    /// <summary>
    /// Создает EFPDateRangeBoxCommandItems.
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPDateRangeBoxCommandItems(this);
    }

    void InitCommandItemsState(object sender, EventArgs args)
    {
      EFPDateRangeBoxCommandItems commandItems2 = CommandItemsIfAssigned as EFPDateRangeBoxCommandItems;
      if (commandItems2 != null)
        commandItems2.InitCommandItemsState();
    }

    void Control_MenuButtonClicked(object sender, System.ComponentModel.CancelEventArgs args)
    {
      PrepareContextMenu(); // иначе меню не будет показано
    }

    #endregion
  }

  /// <summary>
  /// Команды локальго меню для <see cref="EFPDateRangeBox"/>.
  /// Содержит команды для выбора предопределенных периодов и сдвига периода.
  /// </summary>
  internal sealed class EFPDateRangeBoxCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    public EFPDateRangeBoxCommandItems(EFPDateRangeBox controlProvider)
      : base(controlProvider)
    {
      ciToday = new EFPCommandItem("Edit", "Today");
      ciToday.MenuText = Res.DateRangeBox_Menu_Edit_Today;
      ciToday.ShortCut = Keys.Control | Keys.T;
      ciToday.Click += ciToday_Click;
      Add(ciToday);

      ci1To2 = new EFPCommandItem("Edit", "SetLastDateAsFirst");
      ci1To2.MenuText = Res.DateRangeBox_Menu_Edit_SetLastDateAsFirst;
      ci1To2.ShortCut = Keys.Control | Keys.OemCloseBrackets;
      ci1To2.Click += ci1To2_Click;
      ci1To2.GroupBegin = true;
      Add(ci1To2);

      ci2To1 = new EFPCommandItem("Правка", "SetFirstDateAsLast");
      ci2To1.MenuText = Res.DateRangeBox_Menu_Edit_SetFirstDateAsLast;
      ci2To1.ShortCut = Keys.Control | Keys.OemOpenBrackets;
      ci2To1.Click += ci2To1_Click;
      ci2To1.GroupEnd = true;
      Add(ci2To1);

      ciMonth = new EFPCommandItem("Edit", "Month");
      ciMonth.MenuText = Res.DateRangeBox_Menu_Edit_Month;
      ciMonth.ShortCut = Keys.Control | Keys.M;
      ciMonth.Click += ciMonth_Click;
      ciMonth.GroupBegin = true;
      Add(ciMonth);

      ciQuarter = new EFPCommandItem("Edit", "Quarter");
      ciQuarter.MenuText = Res.DateRangeBox_Menu_Edit_Quarter;
      ciQuarter.ShortCut = Keys.Control | Keys.Q;
      ciQuarter.Click += ciQuarter_Click;
      Add(ciQuarter);

      ciYear = new EFPCommandItem("Edit", "Year");
      ciYear.MenuText = Res.DateRangeBox_Menu_Edit_Year;
      ciYear.ShortCut = Keys.Control | Keys.Y; // Redo все равно недоступна, можно занимать
      ciYear.Click += ciYear_Click;
      Add(ciYear);

      ciNextPeriod = new EFPCommandItem("Edit", "NextPeriod");
      ciNextPeriod.MenuText = controlProvider.Control.TheRightButton.ToolTipText;
      ciNextPeriod.ShortCut = Keys.Control | Keys.OemPeriod;
      ciNextPeriod.ImageKey = "DateRangeBoxRight";
      ciNextPeriod.Click += new EventHandler(controlProvider.Control.TheRightButton_Click);
      ciNextPeriod.GroupBegin = true;
      Add(ciNextPeriod);

      ciPrevPeriod = new EFPCommandItem("Edit", "PrevPeriod");
      ciPrevPeriod.MenuText = controlProvider.Control.TheLeftButton.ToolTipText;
      ciPrevPeriod.ShortCut = Keys.Control | Keys.Oemcomma;
      ciPrevPeriod.ImageKey = "DateRangeBoxLeft";
      ciPrevPeriod.Click += new EventHandler(controlProvider.Control.TheLeftButton_Click);
      Add(ciPrevPeriod);

      ciNextYear = new EFPCommandItem("Edit", "NextYear");
      ciNextYear.MenuText = Res.DateRangeBox_Menu_Edit_NextYear;
      ciNextYear.ShortCut = Keys.Control | Keys.Shift | Keys.OemPeriod;
      ciNextYear.Click += new EventHandler(ciNextYear_Click);
      Add(ciNextYear);

      ciPrevYear = new EFPCommandItem("Edit", "PrevYear");
      ciPrevYear.MenuText = Res.DateRangeBox_Menu_Edit_PrevYear;
      ciPrevYear.ShortCut = Keys.Control | Keys.Shift | Keys.Oemcomma;
      ciPrevYear.Click += new EventHandler(ciPrevYear_Click);
      ciPrevYear.GroupEnd = true;
      Add(ciPrevYear);

      InitCommandItemsState();
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPDateRangeBox ControlProvider { get { return (EFPDateRangeBox)(base.ControlProvider); } }

    #endregion

    #region Команды установки значений

    EFPCommandItem ciToday, ci1To2, ci2To1;

    private void ciToday_Click(object sender, EventArgs args)
    {
      ControlProvider.First.NValue = DateTime.Today;
      ControlProvider.Last.NValue = ControlProvider.First.NValue;
    }

    private void ci1To2_Click(object sender, EventArgs args)
    {
      ControlProvider.Last.NValue = ControlProvider.First.NValue;
    }

    private void ci2To1_Click(object sender, EventArgs args)
    {
      ControlProvider.First.NValue = ControlProvider.Last.NValue;
    }

    #endregion

    #region Команды установки диапазонов

    EFPCommandItem ciMonth, ciQuarter, ciYear;

    private void ciMonth_Click(object sender, EventArgs args)
    {
      DoPeriod(1);
    }

    private void ciQuarter_Click(object sender, EventArgs args)
    {
      DoPeriod(2);
    }

    private void ciYear_Click(object sender, EventArgs args)
    {
      DoPeriod(3);
    }

    private void DoPeriod(int mode)
    {
      DateTime firstDateValue, lastDateValue;
      GetPeriodValues(mode, out firstDateValue, out lastDateValue);
      ControlProvider.First.NValue = firstDateValue;
      ControlProvider.Last.NValue = lastDateValue;
    }

    private void GetPeriodValues(int mode, out DateTime firstDateValue, out DateTime lastDateValue)
    {
      DateTime date;
      if (ControlProvider.First.NValue.HasValue)
        date = ControlProvider.First.NValue.Value;
      else if (ControlProvider.Last.NValue.HasValue)
        date = ControlProvider.Last.NValue.Value;
      else
        date = DateTime.Today;

      int m1 = date.Month;
      int m2 = date.Month;

      switch (mode)
      {
        case 1:
          // Для месяца ничего не делаем
          break;
        case 2:
          // Квартал
          m1 = ((m1 - 1) / 3) * 3 + 1;
          m2 = ((m2 + 2) / 3) * 3;
          break;
        case 3:
          // Год
          m1 = 1;
          m2 = 12;
          break;
      }
      firstDateValue = new DateTime(date.Year, m1, 1);
      lastDateValue = new DateTime(date.Year, m2, DateTime.DaysInMonth(date.Year, m2));
    }

    #endregion

    #region Следующий / предыдущий период

    EFPCommandItem ciNextPeriod, ciPrevPeriod, ciNextYear, ciPrevYear;

    void ciNextYear_Click(object sender, EventArgs args)
    {
      ShiftYear(true);
    }

    void ciPrevYear_Click(object sender, EventArgs args)
    {
      ShiftYear(false);
    }

    private void ShiftYear(bool forward)
    {
      DateTime? firstDate = ControlProvider.First.NValue;
      DateTime? lastDate = ControlProvider.Last.NValue;
      if (UITools.ShiftDateRangeYear(ref firstDate, ref lastDate, forward))
      {
        ControlProvider.First.NValue = firstDate;
        ControlProvider.Last.NValue = lastDate;
      }
      else
        EFPApp.ShowTempMessage(Res.DateRangeBox_Err_CannotChangeYear);
    }

    #endregion

    #region Управление доступностью команд и текстом справа

    public void InitCommandItemsState()
    {
      DateRangeFormatter formatter = ControlProvider.Control.Formatter;

      SetMenuRightText(ciToday, formatter.ToString(DateTime.Today, false));

      bool rangeEnabled = ControlProvider.First.Editable && ControlProvider.Last.Editable;

      ci1To2.Enabled = rangeEnabled;
      ci2To1.Enabled = rangeEnabled;
      ciMonth.Enabled = rangeEnabled;
      ciQuarter.Enabled = rangeEnabled;
      ciYear.Enabled = rangeEnabled;

      if (rangeEnabled)
      {
        if (ControlProvider.First.NValue.HasValue)
          SetMenuRightText(ci1To2, formatter.ToString(ControlProvider.First.NValue.Value, false));
        else
        {
          SetMenuRightText(ci1To2, String.Empty);
          ci1To2.Enabled = false;
        }
        if (ControlProvider.Last.NValue.HasValue)
          SetMenuRightText(ci2To1, formatter.ToString(ControlProvider.Last.NValue.Value, false));
        else
        {
          SetMenuRightText(ci2To1, String.Empty);
          ci2To1.Enabled = false;
        }

        SetMenuRightText(ciMonth, GetPeriodText(1));
        SetMenuRightText(ciQuarter, GetPeriodText(2));
        SetMenuRightText(ciYear, GetPeriodText(3));

      }
      else
      {
        SetMenuRightText(ci1To2, String.Empty);
        SetMenuRightText(ci2To1, String.Empty);
        SetMenuRightText(ciMonth, String.Empty);
        SetMenuRightText(ciQuarter, String.Empty);
        SetMenuRightText(ciYear, String.Empty);
      }

      DateTime? dt1;
      DateTime? dt2;

      ciNextPeriod.Enabled = ControlProvider.Control.TheRightButton.Enabled;
      if (ciNextPeriod.Enabled)
      {
        dt1 = ControlProvider.First.NValue;
        dt2 = ControlProvider.Last.NValue;
        UITools.ShiftDateRange(ref dt1, ref dt2, true);
        SetMenuRightText(ciNextPeriod, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      }
      else
        SetMenuRightText(ciNextPeriod, String.Empty);

      ciPrevPeriod.Enabled = ControlProvider.Control.TheLeftButton.Enabled;
      if (ciPrevPeriod.Enabled)
      {
        dt1 = ControlProvider.First.NValue;
        dt2 = ControlProvider.Last.NValue;
        UITools.ShiftDateRange(ref dt1, ref dt2, false);
        SetMenuRightText(ciPrevPeriod, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      }
      else
        SetMenuRightText(ciPrevPeriod, String.Empty);

      dt1 = ControlProvider.First.NValue;
      dt2 = ControlProvider.Last.NValue;
      ciNextYear.Enabled = UITools.ShiftDateRangeYear(ref dt1, ref dt2, true);
      if (ciNextYear.Enabled)
        SetMenuRightText(ciNextYear, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      else
        SetMenuRightText(ciNextYear, String.Empty);

      dt1 = ControlProvider.First.NValue;
      dt2 = ControlProvider.Last.NValue;
      ciPrevYear.Enabled = UITools.ShiftDateRangeYear(ref dt1, ref dt2, false);
      if (ciPrevYear.Enabled)
        SetMenuRightText(ciPrevYear, ControlProvider.Control.Formatter.ToString(dt1, dt2, false));
      else
        SetMenuRightText(ciPrevYear, String.Empty);
    }

    private string GetPeriodText(int mode)
    {
      DateTime firstDateValue, lastDateValue;
      GetPeriodValues(mode, out firstDateValue, out lastDateValue);
      return ControlProvider.Control.Formatter.ToString(firstDateValue, lastDateValue, false);
    }

    internal static void SetMenuRightText(EFPCommandItem ci, string text)
    {
#if XXX
      string s2 = EFPCommandItem.GetShortCutText(ci.ShortCut);
      if (String.IsNullOrEmpty(Text))
        ci.MenuRightText = s2;
      else if (s2.Length == 0)
        ci.MenuRightText = Text;
      else
        ci.MenuRightText = Text + " " + s2;
#else
      ci.MenuRightText = text;
#endif
    }

    #endregion
  }

  /// <summary>
  /// Провайдер комбоблока для выбора даты или интервала дат.
  /// Первоначально комбоблок выглядит как <see cref="DateTimeBox"/> и позволяет ввести дату вручную.
  /// При нажатии "стрелочки" открывается диалог выбора интервала дат.
  /// Текущим значением является <see cref="DateRange"/>. Пустое значение допускается (при <see cref="CanBeEmpty"/>=true).
  /// Полуоткрытые интервалы не поддерживаются.
  /// </summary>
  public class EFPDateOrRangeBox : EFPSyncControl<UserMaskedComboBox>, IEFPReadOnlyControl, IEFPSimpleTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDateOrRangeBox(EFPBaseProvider baseProvider, UserMaskedComboBox control)
      : base(baseProvider, control, true)
    {
      control.Mask = EditableDateTimeFormatters.Date.EditMask;
      if (!DesignMode)
      {
        control.TextChanged += new EventHandler(Control_TextChanged);
        control.PopupClick += new EventHandler(Control_PopupClick);
        control.ClearClick += new EventHandler(Control_ClearClick);
      }
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Возвращает true, если установлены свойства <see cref="EFPControlBase.Enabled"/>=true и <see cref="ReadOnly"/>=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Блокировка при синхронизации выполняется не через свойство <see cref="EFPControlBase.EnabledEx"/>, как
    /// у других управляющих элементов, а через свойство <see cref="ReadOnly"/>.
    /// </summary>
    /// <param name="value">True-выключить блокировку, false-включить</param>
    public override void SyncMasterState(bool value)
    {
      InitReadOnlyEx();
      _NotReadOnlySync.Value = value;
    }

    /// <summary>
    /// Проверка попадания обеих дат диапазона в интервал {<see cref="Minimum"/>, <see cref="Maximum"/>}.
    /// </summary>
    protected override void OnValidate()
    {
      if (!DateRange.IsEmpty)
      {
        if ((!TimeTools.DateInRange(DateRange.FirstDate, Minimum, Maximum)) ||
            (!TimeTools.DateInRange(DateRange.LastDate, Minimum, Maximum)))
          SetError(String.Format(Res.DateTimeBox_Err_DateMustBeInRange,
            Formatter.ToString(Minimum, Maximum, true)));
      }
      else
        ValidateCanBeEmptyMode(CanBeEmptyMode);

      base.OnValidate();
    }

    /// <summary>
    /// Синхронизированное значение типа <see cref="DateRange"/>
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return DateRange;
      }
      set
      {
        DateRange = (DateRange)value;
      }
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Создает объект команд локального меню <see cref="EFPDateOrRangeBoxCommandItems"/>.
    /// </summary>
    /// <returns>Список команд</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      //if (EFPApp.EasyInterface)
      //  return base.GetCommandItems();
      //else
      return new EFPDateOrRangeBoxCommandItems(this);
    }

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPDateOrRangeBoxCommandItems CommandItems { get { return (EFPDateOrRangeBoxCommandItems)(base.CommandItems); } }

    #endregion

    #region Выпадающий диалог и кнопка очистки

    void Control_PopupClick(object sender, EventArgs args)
    {
      InitText(); // на случай частично введенной даты, типа "12.03.19"

      if (DateRange.IsEmpty || DateRange.LastDate == DateRange.FirstDate)
        ShowDateDialog();
      else
        ShowRangeDialog();
    }

    #region Для одиночной даты

    #region Класс MyCalendar

    private class MyCalendar : MonthCalendar
    {
      public EFPDateOrRangeBox Owner;

      protected override void OnDateSelected(DateRangeEventArgs drEvent)
      {
        try
        {
          base.OnDateSelected(drEvent);
          Owner.DateRange = new DateRange(SelectionStart);
          //Owner.MainControl.SelectAll();
          FindForm().Hide();
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }

      protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
      {
        base.OnPreviewKeyDown(args);
        if (/*Args.KeyCode == Keys.Tab || */args.KeyCode == Keys.Escape)
        {
          if ((!args.Alt) && (!args.Control))
            args.IsInputKey = true;
        }
      }

      protected override void OnKeyDown(KeyEventArgs args)
      {
        try
        {
          switch (args.KeyCode)
          {
            case Keys.Return:
              Owner.DateRange = new DateRange(SelectionStart);
              //Owner.MainControl.SelectAll();
              FindForm().Hide();
              args.Handled = true;
              return;
            //case Keys.Tab:
            //  FindForm().Hide();
            //  Args.Handled = true;
            //  Owner.Control.FindForm().SelectNextControl(Owner.Control, !Args.Shift, true, true, true);
            //  return;
            case Keys.Escape:
              FindForm().Hide();
              return;
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
        base.OnKeyDown(args);
      }
    }

    #endregion

    #region Класс MyCalendarForm

    private class MyCalendarForm : Form
    {
      #region Конструктор

      public MyCalendarForm(EFPDateOrRangeBox owner)
      {
        this.SuspendLayout();
        // Нужно для правильного масшабирования кнопки
        // Календарик и так всегда правильный
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        //TopMost = false;
        //TopLevel = false;
        //base.Owner = Owner.FindForm();
        FormButtonStub.AssignCancel(this);

        base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        base.AutoSize = true;

        // Если используется тема Windows-XP, то MonthCalendar рисует нормальную рамку
        // Если используется тема Windows-98, то MonthCalendar не содержит никакой рамки
        // и получается некрасиво
        // Пусть лучше будет лишняя рамка
        TableLayoutPanel pan = new TableLayoutPanel();
        pan.SuspendLayout();
        pan.BorderStyle = BorderStyle.Fixed3D;
        pan.Padding = new Padding(0);
        pan.Margin = new Padding(0);
        pan.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //pan.RowStyles.Add(new RowStyle(SizeType.Absolute, 24 + pan.Margin.Vertical));
        pan.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pan.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pan.AutoSize = true;

        Calendar = new MyCalendar();
        Calendar.Owner = owner;
        Calendar.MaxSelectionCount = 1;
        Calendar.Margin = new Padding(3);
        //Calendar.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        Calendar.SizeChanged += new EventHandler(Calendar_SizeChanged);

        _ToRangeButton = new Button();
        _ToRangeButton.Size = new Size(Calendar.Width, 24);
        _ToRangeButton.Text = Res.EFPDateOrRangeBox_Name_ToRangeButton;
        _ToRangeButton.Image = EFPApp.MainImages.Images["DateRange"];
        _ToRangeButton.ImageAlign = ContentAlignment.MiddleLeft;
        //ToRangeButton.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        _ToRangeButton.Margin = Calendar.Margin; // для красоты
        _ToRangeButton.Click += new EventHandler(ToRangeButton_Click);

        pan.Controls.Add(Calendar, 0, 0);
        pan.Controls.Add(_ToRangeButton, 0, 1);

        this.Controls.Add(pan);
        pan.ResumeLayout();
        this.ResumeLayout();
      }

      #endregion

      #region Свойства

      public readonly MyCalendar Calendar;

      private readonly Button _ToRangeButton;

      #endregion

      #region Обработчики

      protected override bool ShowWithoutActivation
      {
        get { return false; }
      }

      protected override void OnDeactivate(EventArgs args)
      {
        base.OnDeactivate(args);

        // Закрытие формы при попытке щелкнуть мимо
        try
        {
          if (Visible)
            Hide();
        }
        catch /*(Exception e)*/
        {
          //MessageBox.Show(e.Message, "Ошибка при деактивации календаря");
        }
      }

      void ToRangeButton_Click(object sender, EventArgs args)
      {
        Hide();
        Calendar.Owner.ShowRangeDialog(); // показываем окно выбора диапазона
      }

      private void Calendar_SizeChanged(object sender, EventArgs args)
      {
        _ToRangeButton.Width = Calendar.Width;
      }

      #endregion
    }

    #endregion

    /// <summary>
    /// Форма календарика. Нельзя уничтожать форму изнутри обработчика календаря,
    /// т.к. будет ошибка обращения к удаленному объекту при нажатии мыши на
    /// кнопке "текущая дата" (видимо, дефект в MonthCalendar).
    /// Поэтому форма не уничтожается, а делается скрытой.
    /// </summary>
    private MyCalendarForm _CalendarForm;

    private void ShowDateDialog()
    {
      try
      {
        if (WinFormsTools.IsNullOrDisposed(_CalendarForm))
        {
          _CalendarForm = new MyCalendarForm(this);
          Control.Disposed += new EventHandler(Control_Disposed);
        }

        Point pt = new Point(3, Control.Height);
        pt = Control.PointToScreen(pt);
        _CalendarForm.Location = pt;
        if (!DateRange.IsEmpty)
        {
          if (DateRange.FirstDate >= _CalendarForm.Calendar.MinDate && DateRange.FirstDate <= _CalendarForm.Calendar.MaxDate)
            _CalendarForm.Calendar.SelectionStart = DateRange.FirstDate;
          else
            _CalendarForm.Calendar.SelectionStart = DateTime.Today; // 24.05.2019
        }
        else
          _CalendarForm.Calendar.SelectionStart = DateTime.Today;
        _CalendarForm.Calendar.SelectionEnd = _CalendarForm.Calendar.SelectionStart;

        EFPApp.SystemMethods.Show(_CalendarForm, null);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, Res.EFPDateOrRangeBox_Err_ShowCalendar);
      }
    }

    void Control_Disposed(object sender, EventArgs args)
    {
      if (_CalendarForm != null)
      {
        _CalendarForm.Dispose();
        _CalendarForm = null;
        Control.Disposed -= new EventHandler(Control_Disposed);
      }
    }

    #endregion

    #region Для диапазона дат

    private void ShowRangeDialog()
    {
      DateRangeDialog dlg = new DateRangeDialog();
      dlg.Title = DisplayName;
      dlg.CanBeEmptyMode = CanBeEmptyMode;
      dlg.DialogPosition.PopupOwnerControl = Control;
      if (!DateRange.IsEmpty)
      {
        dlg.NFirstValue = DateRange.FirstDate;
        dlg.NLastValue = DateRange.LastDate;
      }
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.CanBeHalfEmpty = false;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      if (dlg.NFirstValue.HasValue && dlg.NLastValue.HasValue)
        DateRange = new DateRange(dlg.NFirstValue.Value, dlg.NLastValue.Value);
      else
        DateRange = DateRange.Empty;
    }

    #endregion

    void Control_ClearClick(object sender, EventArgs args)
    {
      DateRange = DateRange.Empty;
    }

    #endregion

    #region Текущее значение DateRange/FirstDate/LastDate

    #region DateRange

    /// <summary>
    /// Доступ к выбранной дате без принудительного создания объекта
    /// </summary>
    public DateRange DateRange
    {
      get { return _DateRange; }
      set
      {
        _DateRange = value;
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            if (_FirstValueEx != null)
              _FirstValueEx.Value = FirstValue;
            if (_LastValueEx != null)
              _LastValueEx.Value = LastValue;
            if (_NFirstValueEx != null)
              _NFirstValueEx.Value = NFirstValue;
            if (_NLastValueEx != null)
              _NLastValueEx.Value = NLastValue;
            if (_DateRangeEx != null)
              _DateRangeEx.Value = DateRange;
            InitText();
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }

        OnValueChanged();
      }
    }

    private DateRange _DateRange;

    /// <summary>
    /// Управляемое свойство для <see cref="DateRange"/>
    /// </summary>
    public DepValue<DateRange> DateRangeEx
    {
      get
      {
        InitDateRangeEx();
        return _DateRangeEx;
      }
      set
      {
        InitDateRangeEx();
        _DateRangeEx.Source = value;
      }
    }
    private DepInput<DateRange> _DateRangeEx;

    private void InitDateRangeEx()
    {
      if (_DateRangeEx == null)
      {
        _DateRangeEx = new DepInput<DateRange>(this.DateRange, DateRangeEx_ValueChanged);
        _DateRangeEx.OwnerInfo = new DepOwnerInfo(this, "DateRangeEx");
      }
    }

    private void DateRangeEx_ValueChanged(object sender, EventArgs args)
    {
      DateRange = _DateRangeEx.Value;
    }


    private void InitText()
    {
      bool currInsideValueChanged = _InsideValueChanged;

      // 18.10.2023. В Mono, при установке свойства UserMaskedTextBox.Mask меняется свойство Text и генерируется событие TextChanged,
      // что неожиданно меняет свойство DateRange.
      // Проявляется только при установке ReadOnly=true.
      // Предотвращаем изменение свойства.
      _InsideValueChanged = true;
      try
      {
        if (DateRange.IsEmpty)
        {
          Control.Mask = EditableDateTimeFormatters.Date.EditMask;
          Control.Text = String.Empty;
          Control.ReadOnly = ReadOnly;
        }
        else if (DateRange.FirstDate == DateRange.LastDate)
        {
          Control.Mask = EditableDateTimeFormatters.Date.EditMask;
          Control.Text = EditableDateTimeFormatters.Date.ToString(DateRange.FirstDate); // 29.04.2021, 19.05.2021
          Control.ReadOnly = ReadOnly;
        }
        else
        {
          Control.Mask = String.Empty;
          Control.Text = Formatter.ToString(DateRange, false);
          Control.ReadOnly = true;
        }
        Control.PopupButtonEnabled = !ReadOnly;
        Control.ClearButtonEnabled = (!DateRange.IsEmpty) && (!ReadOnly);
      }
      finally
      {
        _InsideValueChanged = currInsideValueChanged;
      }
    }

    #endregion

    #region FirstValue

    /// <summary>
    /// Начальная дата диапазона.
    /// Пустой диапазон не поддерживается. Если <see cref="DateRange"/>.IsEmpty=true, возвращается минимально возможная дата.
    /// Установка значения свойства может привести к изменению конечной даты, если задаваемое значение больше, чем текущая конечная дата.
    /// </summary>
    public DateTime FirstValue
    {
      get
      {
        if (DateRange.IsEmpty)
          return DateRange.Whole.FirstDate;
        else
          return DateRange.FirstDate;
      }
      set
      {
        if (DateRange.IsEmpty)
          DateRange = new DateRange(value, value);
        else
          DateRange = new DateRange(value, TimeTools.Max(DateRange.LastDate, value.Date));
      }
    }

    /// <summary>
    /// Управляемое значение для <see cref="FirstValue"/>.
    /// </summary>
    public DepValue<DateTime> FirstValueEx
    {
      get
      {
        InitFirstValueEx();
        return _FirstValueEx;
      }
      set
      {
        InitFirstValueEx();
        _FirstValueEx.Source = value;
      }
    }

    private DepInput<DateTime> _FirstValueEx;

    private void InitFirstValueEx()
    {
      if (_FirstValueEx == null)
      {
        _FirstValueEx = new DepInput<DateTime>(FirstValue, FirstValueEx_ValueChanged);
        _FirstValueEx.OwnerInfo = new DepOwnerInfo(this, "FirstValueEx");
      }
    }

    private void FirstValueEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        FirstValue = _FirstValueEx.Value;
    }

    #endregion

    #region LastValue

    /// <summary>
    /// Конечная дата диапазона.
    /// Пустой диапазон не поддерживается. Если <see cref="DateRange"/>.IsEmpty=true, возвращается максимально возможная дата.
    /// Установка значения свойства может привести к изменению начальной даты, если задаваемое значение меньше, чем текущая начальная дата.
    /// </summary>
    public DateTime LastValue
    {
      get
      {
        if (DateRange.IsEmpty)
          return DateRange.Whole.LastDate;
        else
          return DateRange.LastDate;
      }
      set
      {
        if (DateRange.IsEmpty)
          DateRange = new DateRange(value, value);
        else
          DateRange = new DateRange(TimeTools.Min(DateRange.FirstDate, value.Date), value);
      }
    }

    /// <summary>
    /// Управляемое значение для <see cref="LastValue"/>.
    /// </summary>
    public DepValue<DateTime> LastValueEx
    {
      get
      {
        InitLastValueEx();
        return _LastValueEx;
      }
      set
      {
        InitLastValueEx();
        _LastValueEx.Source = value;
      }
    }

    private DepInput<DateTime> _LastValueEx;


    private void InitLastValueEx()
    {
      if (_LastValueEx == null)
      {
        _LastValueEx = new DepInput<DateTime>(LastValue, LastValueEx_ValueChanged);
        _LastValueEx.OwnerInfo = new DepOwnerInfo(this, "LastValueEx");
      }
    }

    private void LastValueEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        LastValue = _LastValueEx.Value;
    }

    #endregion

    #region NFirstValue

    /// <summary>
    /// Начальная дата диапазона.
    /// Пустой диапазон не поддерживается. Если <see cref="DateRange"/>.IsEmpty=true, возвращается null.
    /// Установка значения свойства может привести к изменению конечной даты, если задаваемое значение больше, чем текущая конечная дата.
    /// </summary>
    public DateTime? NFirstValue
    {
      get
      {
        if (DateRange.IsEmpty)
          return null;
        else
          return DateRange.FirstDate;
      }
      set
      {
        if (value.HasValue)
          FirstValue = value.Value;
        else
          DateRange = DateRange.Empty;
      }
    }

    /// <summary>
    /// Управляемое значение для <see cref="NFirstValue"/>.
    /// </summary>
    public DepValue<DateTime?> NFirstValueEx
    {
      get
      {
        InitNFirstValueEx();
        return _NFirstValueEx;
      }
      set
      {
        InitNFirstValueEx();
        _NFirstValueEx.Source = value;
      }
    }

    private DepInput<DateTime?> _NFirstValueEx;

    private void InitNFirstValueEx()
    {
      if (_NFirstValueEx == null)
      {
        _NFirstValueEx = new DepInput<DateTime?>(NFirstValue, NFirstValueEx_ValueChanged);
        _NFirstValueEx.OwnerInfo = new DepOwnerInfo(this, "NFirstValueEx");
      }
    }

    private void NFirstValueEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        NFirstValue = _NFirstValueEx.Value;
    }

    #endregion

    #region NLastValue

    /// <summary>
    /// Конечная дата диапазона.
    /// Пустой диапазон не поддерживается. Если <see cref="DateRange"/>.IsEmpty=true, возвращается null.
    /// Установка значения свойства может привести к изменению начальной даты, если задаваемое значение меньше, чем текущая начальная дата.
    /// </summary>
    public DateTime? NLastValue
    {
      get
      {
        if (DateRange.IsEmpty)
          return null;
        else
          return DateRange.LastDate;
      }
      set
      {
        if (value.HasValue)
          LastValue = value.Value;
        else
          DateRange = DateRange.Empty;
      }
    }

    /// <summary>
    /// Управляемое значение для <see cref="NLastValue"/>.
    /// </summary>
    public DepValue<DateTime?> NLastValueEx
    {
      get
      {
        InitNLastValueEx();
        return _NLastValueEx;
      }
      set
      {
        InitNLastValueEx();
        _NLastValueEx.Source = value;
      }
    }

    private DepInput<DateTime?> _NLastValueEx;


    private void InitNLastValueEx()
    {
      if (_NLastValueEx == null)
      {
        _NLastValueEx = new DepInput<DateTime?>(NLastValue, NLastValueEx_ValueChanged);
        _NLastValueEx.OwnerInfo = new DepOwnerInfo(this, "NLastValueEx");
      }
    }

    private void NLastValueEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        NLastValue = _NLastValueEx.Value;
    }

    #endregion

    #region OnValueChanged

    void Control_TextChanged(object sender, EventArgs args)
    {
      if (_InsideValueChanged)
        return;
      _InsideValueChanged = true;
      try
      {
        DateTime? dt = EditableDateTimeFormatters.Date.ToNValue(Control.Text, DefaultYear);
        if (dt.HasValue)
          this.DateRange = new DateRange(dt.Value, dt.Value);
        else
          this.DateRange = DateRange.Empty;
      }
      finally
      {
        _InsideValueChanged = false;
      }
    }

    private bool _InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод.
    /// </summary>
    protected virtual void OnValueChanged()
    {
#if XXX
      if (_FirstValueEx != null)
      {
        if (DateRange.IsEmpty)
          _FirstValueEx.OwnerSetValue(null);
        else
          _FirstValueEx.OwnerSetValue(DateRange.FirstDate);
      }
      if (_LastValueEx != null)
      {
        if (DateRange.IsEmpty)
          _LastValueEx.OwnerSetValue(null);
        else
          _LastValueEx.OwnerSetValue(DateRange.LastDate);
      }
#endif
      EFPDateOrRangeBoxCommandItems commandItems2 = CommandItemsIfAssigned as EFPDateOrRangeBoxCommandItems;
      if (commandItems2 != null)
        commandItems2.InitCommandItemsState(); // 03.06.2019

      Validate();
      OnSyncValueChanged();
    }

    #endregion

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, которое возвращает true, если диапазон задан (<see cref="NFirstValue"/>.HasValue=true и <see cref="NLastValue"/>.HasValue=true).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr2<bool, DateTime?, DateTime?>(NFirstValueEx, NLastValueEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }

    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(DateTime? firstDate, DateTime? lastDate)
    {
      return firstDate.HasValue && lastDate.HasValue;
    }

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        if (ProviderState == EFPControlProviderState.Attached)
          Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Можно ли вводить пустое значение. Дублирует свойство <see cref="CanBeEmptyMode"/>.
    /// По умолчанию - false.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Свойство DefaultYear

    /// <summary>
    /// Год по умолчанию.
    /// Если задано ненулевое значение и пользователь ввел только 4 цифры дня и месяца,
    /// дата будет считаться корректной и использовать выбранный год.
    /// </summary>
    public int DefaultYear
    {
      get { return _DefaultYear; }
      set
      {
        _DefaultYear = value;
        if (_DefaultYearEx != null)
          _DefaultYearEx.Value = value;
        Control_TextChanged(null, null);
      }
    }

    private int _DefaultYear;

    /// <summary>
    /// Управляемое свойство для <see cref="DefaultYear"/>
    /// </summary>
    public DepValue<int> DefaultYearEx
    {
      get
      {
        InitDefaultYearEx();
        return _DefaultYearEx;
      }
      set
      {
        InitDefaultYearEx();
        _DefaultYearEx.Source = value;
      }
    }
    private DepInput<int> _DefaultYearEx;

    private void InitDefaultYearEx()
    {
      if (_DefaultYearEx == null)
      {
        _DefaultYearEx = new DepInput<int>(DefaultYear, DefaultYearEx_ValueChanged);
        _DefaultYearEx.OwnerInfo = new DepOwnerInfo(this, "DefaultYearEx");
      }
    }

    void DefaultYearEx_ValueChanged(object sender, EventArgs args)
    {
      DefaultYear = DefaultYearEx.Value;
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Если true, то управляющий элемент разрешает только просмотр диапазона дат, но не его изменение
    /// </summary>
    public bool ReadOnly
    {
      // Нельзя использовать свойство Control.ReadOnly напрямую.
      // Элемент переводится в состояние "Только чтение", если в нем введен интервал дат

      get { return _ReadOnly; }
      set
      {
        if (value == _ReadOnly)
          return;
        _ReadOnly = value;
        InitText(); // переключает Control.ReadOnly
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство <see cref="ReadOnly"/>
    /// </summary>
    public DepValue<Boolean> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyMain.Source = value;
      }
    }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<Boolean>(false, ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");

        _ReadOnlyMain = new DepInput<bool>(false, null);
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");

        _NotReadOnlySync = new DepInput<bool>(true, null);
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");

        DepOr readOnlyOr = new DepOr(_ReadOnlyMain, new DepNot(_NotReadOnlySync));
        _ReadOnlyEx.Source = readOnlyOr;
      }
    }

    /// <summary>
    /// Выходная часть свойства ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyEx;
    /// <summary>
    /// Основной вход для ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyMain;
    /// <summary>
    /// Дополнительный вход для ReadOnly для выполнения синхронизации
    /// </summary>
    private DepInput<Boolean> _NotReadOnlySync;

    private void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region Диапазон допустимых значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство <see cref="FirstValue"/> меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено.
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set
      {
        _Minimum = value;
        if (ProviderState == EFPControlProviderState.Attached)
          Validate();
      }
    }

    private DateTime? _Minimum;

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство <see cref="LastValue"/> больше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено.
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set
      {
        _Maximum = value;
        if (ProviderState == EFPControlProviderState.Attached)
          Validate();
      }
    }

    private DateTime? _Maximum;

    #endregion

    #region Свойство Formatter

    /// <summary>
    /// Форматировщик диапазона дат.
    /// Если свойство не установлено в явном виде, возвращает <see cref="DateRangeFormatter.Default"/>.
    /// </summary>
    public DateRangeFormatter Formatter
    {
      get
      {
        if (_Formatter == null)
          return DateRangeFormatter.Default;
        else
          return _Formatter;
      }
      set
      {
        _Formatter = value;
      }
    }
    private DateRangeFormatter _Formatter;

    #endregion

    #region IEFPSimpleTextBox Members

    string IEFPSimpleTextBox.Text
    {
      get
      {
        return Control.Text;
      }
      set
      {
        Control.Text = value; // ?? может быть, нужна проверка
      }
    }

    int IEFPSimpleTextBox.TextLength { get { return Control.Text.Length; } }

    int IEFPSimpleTextBox.SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    int IEFPSimpleTextBox.SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    string IEFPSimpleTextBox.SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    void IEFPSimpleTextBox.Select(int start, int length)
    {
      Control.Select(start, length);
    }

    void IEFPSimpleTextBox.SelectAll()
    {
      Control.SelectAll();
    }

    #endregion
  }

  /// <summary>
  /// Команды меню для <see cref="EFPDateOrRangeBox"/>
  /// </summary>
  public class EFPDateOrRangeBoxCommandItems : EFPTextBoxCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPDateOrRangeBoxCommandItems(EFPDateOrRangeBox controlProvider)
      : base(controlProvider, false, true)
    {
      ciToday = new EFPCommandItem("Edit", "Today");
      ciToday.MenuText = Res.DateRangeBox_Menu_Edit_Today;
      ciToday.ShortCut = Keys.Control | Keys.T;
      ciToday.Click += ciToday_Click;
      Add(ciToday);

      ciMonth = new EFPCommandItem("Edit", "Month");
      ciMonth.MenuText = Res.DateRangeBox_Menu_Edit_Month;
      ciMonth.ShortCut = Keys.Control | Keys.M;
      ciMonth.Click += ciMonth_Click;
      ciMonth.GroupBegin = true;
      Add(ciMonth);

      ciQuarter = new EFPCommandItem("Edit", "Quarter");
      ciQuarter.MenuText = Res.DateRangeBox_Menu_Edit_Quarter;
      ciQuarter.ShortCut = Keys.Control | Keys.Q;
      ciQuarter.Click += ciQuarter_Click;
      Add(ciQuarter);

      ciYear = new EFPCommandItem("Edit", "Year");
      ciYear.MenuText = Res.DateRangeBox_Menu_Edit_Year;
      ciYear.ShortCut = Keys.Control | Keys.Y;
      ciYear.Click += ciYear_Click;
      Add(ciYear);

      InitCommandItemsState();
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента 
    /// </summary>
    public new EFPDateOrRangeBox ControlProvider { get { return (EFPDateOrRangeBox)(base.ControlProvider); } }

    #endregion

    #region Команды установки значений

    EFPCommandItem ciToday;
    EFPCommandItem ciMonth, ciQuarter, ciYear;

    private void ciToday_Click(object sender, EventArgs args)
    {
      ControlProvider.DateRange = new DateRange(DateTime.Today);
    }

    private void ciMonth_Click(object sender, EventArgs args)
    {
      DoPeriod(1);
    }

    private void ciQuarter_Click(object sender, EventArgs args)
    {
      DoPeriod(2);
    }

    private void ciYear_Click(object sender, EventArgs args)
    {
      DoPeriod(3);
    }

    private void DoPeriod(int mode)
    {
      DateTime firstDateValue, lastDateValue;
      GetPeriodValues(mode, out firstDateValue, out lastDateValue);
      ControlProvider.DateRange = new DateRange(firstDateValue, lastDateValue);
    }

    private void GetPeriodValues(int mode, out DateTime firstDateValue, out DateTime lastDateValue)
    {
      DateTime date;
      if (ControlProvider.DateRange.IsEmpty)
        date = DateTime.Today;
      else
        date = ControlProvider.DateRange.FirstDate;

      int m1 = date.Month;
      int m2 = date.Month;

      switch (mode)
      {
        case 1:
          // Для месяца ничего не делаем
          break;
        case 2:
          // Квартал
          m1 = ((m1 - 1) / 3) * 3 + 1;
          m2 = ((m2 + 2) / 3) * 3;
          break;
        case 3:
          // Год
          m1 = 1;
          m2 = 12;
          break;
      }
      firstDateValue = new DateTime(date.Year, m1, 1);
      lastDateValue = new DateTime(date.Year, m2, DateTime.DaysInMonth(date.Year, m2));
    }

    #endregion

    #region Управление доступностью команд и текстом справа

    /// <summary>
    /// Инициализация текста для команд
    /// </summary>
    public void InitCommandItemsState()
    {
      EFPDateRangeBoxCommandItems.SetMenuRightText(ciToday, ControlProvider.Formatter.ToString(DateTime.Today, false));

      EFPDateRangeBoxCommandItems.SetMenuRightText(ciMonth, GetPeriodText(1));
      EFPDateRangeBoxCommandItems.SetMenuRightText(ciQuarter, GetPeriodText(2));
      EFPDateRangeBoxCommandItems.SetMenuRightText(ciYear, GetPeriodText(3));

    }

    private string GetPeriodText(int mode)
    {
      DateTime firstDateValue, lastDateValue;
      GetPeriodValues(mode, out firstDateValue, out lastDateValue);
      return ControlProvider.Formatter.ToString(firstDateValue, lastDateValue, false);
    }

    #endregion
  }

}
