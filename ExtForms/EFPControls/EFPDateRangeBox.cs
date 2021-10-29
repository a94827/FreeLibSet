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
  public class EFPDateBoxRangeCheck
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// Далее должны быть установлены свойства FirstDateBox и LastDateBox.
    /// </summary>
    public EFPDateBoxRangeCheck()
    {
      DoInit();
    }

    /// <summary>
    /// Создает объект с заданными свойствами FirstDateBox и LastDateBox.
    /// </summary>
    /// <param name="firstDateBox">Провайдер поля ввода начальной даты диапазона</param>
    /// <param name="lastDateBox">Провайдер поля ввода конечной даты диапазона</param>
    public EFPDateBoxRangeCheck(EFPDateTimeBox firstDateBox, EFPDateTimeBox lastDateBox)
    {
      DoInit();
      this.FirstDateBox = firstDateBox;
      this.LastDateBox = lastDateBox;
    }

    internal EFPDateBoxRangeCheck(EFPDateTimeBox firstDateBox, EFPDateTimeBox lastDateBox, EFPDateRangeBox owner)
    {
      DoInit();
      this.FirstDateBox = firstDateBox;
      this.LastDateBox = lastDateBox;
      _Owner = owner;
    }

    private void DoInit()
    {
      _FirstDateInput = new DepInput<DateTime?>();
      _FirstDateInput.OwnerInfo = new DepOwnerInfo(this, "FirstDateInput");
      _FirstDateInput.ValueChanged += new EventHandler(FirstDateInput_ValueChanged);

      _LastDateInput = new DepInput<DateTime?>();
      _LastDateInput.OwnerInfo = new DepOwnerInfo(this, "LastDateInput");
      _LastDateInput.ValueChanged += new EventHandler(LastDateInput_ValueChanged);

      _EHValidatingFirstDate = new EFPValidatingEventHandler(DoValidate);
      _EHValidatingLastDate = new EFPValidatingEventHandler(DoValidate);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для поля ввода начальной даты
    /// </summary>
    public EFPDateTimeBox FirstDateBox
    {
      get { return _FirstDateBox; }
      set
      {
        if (value == _FirstDateBox)
          return;

        if (_FirstDateBox != null)
          _FirstDateBox.Validating -= _EHValidatingFirstDate;
        _FirstDateBox = value;
        if (_FirstDateBox != null)
          _FirstDateBox.Validating += _EHValidatingFirstDate;

        if (value == null)
          _FirstDateInput.Source = null;
        else
          _FirstDateInput.Source = value.NValueEx;
      }
    }
    private EFPDateTimeBox _FirstDateBox;

    /// <summary>
    /// Провайдер для поля ввода конечной даты
    /// </summary>
    public EFPDateTimeBox LastDateBox
    {
      get { return _LastDateBox; }
      set
      {
        if (value == _LastDateBox)
          return;

        if (_LastDateBox != null)
          _LastDateBox.Validating -= _EHValidatingLastDate;
        _LastDateBox = value;
        if (_LastDateBox != null)
          _LastDateBox.Validating += _EHValidatingLastDate;

        if (value == null)
          _LastDateInput.Source = null;
        else
          _LastDateInput.Source = value.NValueEx;
      }
    }
    private EFPDateTimeBox _LastDateBox;

    private EFPDateRangeBox _Owner;

    #endregion

    #region Внутренняя реализация

    private DepInput<Nullable<DateTime>> _FirstDateInput;
    private DepInput<Nullable<DateTime>> _LastDateInput;

    void FirstDateInput_ValueChanged(object sender, EventArgs args)
    {
      if (LastDateBox != null)
        LastDateBox.Validate();
    }

    void LastDateInput_ValueChanged(object sender, EventArgs args)
    {
      if (FirstDateBox != null)
        FirstDateBox.Validate();
    }


    private EFPValidatingEventHandler _EHValidatingFirstDate;
    private EFPValidatingEventHandler _EHValidatingLastDate;

    private void DoValidate(object sender, EFPValidatingEventArgs args)
    {
      if (_Owner != null)
        _Owner.Validate();

      if (_FirstDateInput.Value.HasValue && _LastDateInput.Value.HasValue)
      {
        if (FirstDateBox.Enabled && LastDateBox.Enabled)
        {
          if (_FirstDateInput.Value.Value > _LastDateInput.Value.Value)
            args.SetError("Неправильный интервал дат. Начальная дата больше конечной");
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для DateRangeBox.
  /// Содержит два "вложенных" провайдера EFPDateBox.
  /// </summary>
  public class EFPDateRangeBox : EFPControl<DateRangeBox>
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
      _First.DisplayName = "Начальная дата";
      _Last = new EFPDateTimeBox(this.BaseProvider, control.Last);
      _Last.DisplayName = "Конечная дата";
      new EFPDateBoxRangeCheck(_First, _Last, this);

      _First.NValueEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _Last.NValueEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _First.EditableEx.ValueChanged += new EventHandler(InitCommandItemsState);
      _Last.EditableEx.ValueChanged += new EventHandler(InitCommandItemsState);

      if (!DesignMode)
        control.MenuButtonClicked += new System.ComponentModel.CancelEventHandler(Control_MenuButtonClicked);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер поля ввода начальной даты диапазона.
    /// </summary>
    public EFPDateTimeBox First { get { return _First; } }
    private EFPDateTimeBox _First;

    /// <summary>
    /// Провайдер поля ввода конечной даты диапазона.
    /// </summary>
    public EFPDateTimeBox Last { get { return _Last; } }
    private EFPDateTimeBox _Last;

    #endregion

    #region Переопределяемые методы

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
        First.ToolTipText = s + "Начальная дата";
        Last.ToolTipText = s + "Конечная дата";
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
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPDateRangeBoxCommandItems(this);
    }

    void InitCommandItemsState(object sender, EventArgs args)
    {
      EFPDateRangeBoxCommandItems CommandItems2 = CommandItemsIfAssigned as EFPDateRangeBoxCommandItems;
      if (CommandItems2 != null)
        CommandItems2.InitCommandItemsState();
    }

    void Control_MenuButtonClicked(object sender, System.ComponentModel.CancelEventArgs args)
    {
      PrepareContextMenu(); // иначе меню не будет показано
    }


    #endregion
  }

  /// <summary>
  /// Команды меню для EFPDateRangeBox
  /// </summary>
  internal sealed class EFPDateRangeBoxCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    public EFPDateRangeBoxCommandItems(EFPDateRangeBox controlProvider)
    {
      _ControlProvider = controlProvider;

      ciToday = new EFPCommandItem("Edit", "Today");
      ciToday.MenuText = "Сегодня";
      ciToday.ShortCut = Keys.Control | Keys.T;
      ciToday.Click += ciToday_Click;
      Add(ciToday);

      ci1To2 = new EFPCommandItem("Edit", "LastDateEqualFirst");
      ci1To2.MenuText = "Конечная дата равна начальной";
      ci1To2.ShortCut = Keys.Control | Keys.OemCloseBrackets;
      ci1To2.Click += ci1To2_Click;
      ci1To2.GroupBegin = true;
      Add(ci1To2);

      ci2To1 = new EFPCommandItem("Правка", "FirstDateEqualLast");
      ci2To1.MenuText = "Начальная дата равна конечной";
      ci2To1.ShortCut = Keys.Control | Keys.OemOpenBrackets;
      ci2To1.Click += ci2To1_Click;
      ci2To1.GroupEnd = true;
      Add(ci2To1);

      ciMonth = new EFPCommandItem("Edit", "Month");
      ciMonth.MenuText = "Месяц";
      ciMonth.ShortCut = Keys.Control | Keys.M;
      ciMonth.Click += ciMonth_Click;
      ciMonth.GroupBegin = true;
      Add(ciMonth);

      ciQuarter = new EFPCommandItem("Edit", "Quarter");
      ciQuarter.MenuText = "Квартал";
      ciQuarter.ShortCut = Keys.Control | Keys.Q;
      ciQuarter.Click += ciQuarter_Click;
      Add(ciQuarter);

      ciYear = new EFPCommandItem("Edit", "Year");
      ciYear.MenuText = "Год";
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
      ciNextYear.MenuText = "Следующий год";
      ciNextYear.ShortCut = Keys.Control | Keys.Shift | Keys.OemPeriod;
      ciNextYear.Click += new EventHandler(ciNextYear_Click);
      Add(ciNextYear);

      ciPrevYear = new EFPCommandItem("Edit", "PrevYear");
      ciPrevYear.MenuText = "Предыдущий год";
      ciPrevYear.ShortCut = Keys.Control | Keys.Shift | Keys.Oemcomma;
      ciPrevYear.Click += new EventHandler(ciPrevYear_Click);
      ciPrevYear.GroupEnd = true;
      Add(ciPrevYear);

      InitCommandItemsState();
    }

    #endregion

    #region Общие свойства

    public EFPDateRangeBox ControlProvider { get { return _ControlProvider; } }
    private EFPDateRangeBox _ControlProvider;

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
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      ControlProvider.First.NValue = FirstDateValue;
      ControlProvider.Last.NValue = LastDateValue;
    }

    private void GetPeriodValues(int mode, out DateTime firstDateValue, out DateTime lastDateValue)
    {
      DateTime Date;
      if (ControlProvider.First.NValue.HasValue)
        Date = ControlProvider.First.NValue.Value;
      else if (ControlProvider.Last.NValue.HasValue)
        Date = ControlProvider.Last.NValue.Value;
      else
        Date = DateTime.Today;

      int m1 = Date.Month;
      int m2 = Date.Month;

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
      firstDateValue = new DateTime(Date.Year, m1, 1);
      lastDateValue = new DateTime(Date.Year, m2, DateTime.DaysInMonth(Date.Year, m2));
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
      DateTime? FirstDate = ControlProvider.First.NValue;
      DateTime? LastDate = ControlProvider.Last.NValue;
      if (UITools.ShiftDateRangeYear(ref FirstDate, ref LastDate, forward))
      {
        ControlProvider.First.NValue = FirstDate;
        ControlProvider.Last.NValue = LastDate;
      }
      else
        EFPApp.ShowTempMessage("Нельзя изменить год");
    }

    #endregion

    #region Управление доступностью команд и текстом справа

    public void InitCommandItemsState()
    {
      DateRangeFormatter Formatter = ControlProvider.Control.Formatter;

      SetMenuRightText(ciToday, Formatter.ToString(DateTime.Today, false));

      bool RangeEnabled = ControlProvider.First.Editable && ControlProvider.Last.Editable;

      ci1To2.Enabled = RangeEnabled;
      ci2To1.Enabled = RangeEnabled;
      ciMonth.Enabled = RangeEnabled;
      ciQuarter.Enabled = RangeEnabled;
      ciYear.Enabled = RangeEnabled;

      if (RangeEnabled)
      {
        if (ControlProvider.First.NValue.HasValue)
          SetMenuRightText(ci1To2, Formatter.ToString(ControlProvider.First.NValue.Value, false));
        else
        {
          SetMenuRightText(ci1To2, String.Empty);
          ci1To2.Enabled = false;
        }
        if (ControlProvider.Last.NValue.HasValue)
          SetMenuRightText(ci2To1, Formatter.ToString(ControlProvider.Last.NValue.Value, false));
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
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      return ControlProvider.Control.Formatter.ToString(FirstDateValue, LastDateValue, false);
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
  /// Первоначально комбоблок выглядит как DateBox и позволяет ввести дату вручную.
  /// При нажатии "стрелочки" открывается диалог выбора интервала дат.
  /// Текущим значением является DateRange. Пустое значение допускается (при CanBeEmpty=true).
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
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Блокировка при синхронизации выполняется не через свойство EnabledEx, как
    /// у других управляющих элементов, а через свойство ReadOnly
    /// </summary>
    /// <param name="value">True-выключить блокировку, false-включить</param>
    public override void SyncMasterState(bool value)
    {
      InitReadOnlyEx();
      _NotReadOnlySync.Value = value;
    }

    /// <summary>
    /// Проверка попадания обних дат диапазона в интервал {Minimum, Maximum}
    /// </summary>
    protected override void OnValidate()
    {
      if (!DateRange.IsEmpty)
      {
        if ((!DataTools.DateInRange(DateRange.FirstDate, Minimum, Maximum)) ||
          (!DataTools.DateInRange(DateRange.LastDate, Minimum, Maximum)))
        {
          if (WarningIfOutOfRange)
            SetWarning("Дата должна быть в диапазоне " + Formatter.ToString(Minimum, Maximum, true));
          else
            SetError("Дата должна быть в диапазоне " + Formatter.ToString(Minimum, Maximum, true));
        }
      }
      else
      {
        if (CanBeEmpty)
        {
          if (WarningIfEmpty)
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
        }
        else
          SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
      }

      base.OnValidate();
    }

    /// <summary>
    /// Синхронизированное значение типа DateRange
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
    /// Создает объект команд локального меню EFPDateOrRangeBoxCommandItems.
    /// </summary>
    /// <returns>Список команд</returns>
    protected override EFPControlCommandItems GetCommandItems()
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

      protected override void OnDateSelected(DateRangeEventArgs drevent)
      {
        try
        {
          base.OnDateSelected(drevent);
          Owner.DateRange = new DateRange(SelectionStart);
          //Owner.MainControl.SelectAll();
          FindForm().Hide();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
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
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
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
        _ToRangeButton.Text = "Диапазон";
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

        _CalendarForm.Show();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Ошибка вызова календаря");
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
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.Prompt = "Диапазон значений";
      dlg.DialogPosition.PopupOwnerControl = Control;
      if (!DateRange.IsEmpty)
      {
        dlg.NFirstDate = DateRange.FirstDate;
        dlg.NLastDate = DateRange.LastDate;
      }
      dlg.Minimum = Minimum;
      dlg.Maximum = Maximum;
      dlg.Validating += new EFPValidatingTwoValuesEventHandler<DateTime?, DateTime?>(RangeDialog_Validating);

      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      if (dlg.NFirstDate.HasValue && dlg.NLastDate.HasValue)
        DateRange = new DateRange(dlg.NFirstDate.Value, dlg.NLastDate.Value);
      else
        DateRange = DateRange.Empty;
    }

    void RangeDialog_Validating(object sender, EFPValidatingTwoValuesEventArgs<DateTime?, DateTime?> args)
    {
      if (args.Value1.HasValue != args.Value2.HasValue)
        args.SetError("Полуоткрытые интервалы не допускаются");
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
        if (_FirstDateEx != null)
          _FirstDateEx.Value = FirstDate;
        if (_LastDateEx != null)
          _LastDateEx.Value = LastDate;
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
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

    private void InitText()
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

    #endregion

    #region FirstDate

    /// <summary>
    /// Начальная дата диапазона.
    /// Пустой диапазон не поддерживается. Если DateRange.IsEmpty=true, возвращается минимально возможная дата.
    /// Установка значения свойства может привести к изменению конечной даты, если задаваемое значение больше, чем текущая конечная дата.
    /// </summary>
    public DateTime FirstDate
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
          DateRange = new DateRange(value, DataTools.Max(DateRange.LastDate, value.Date));
      }
    }

    /// <summary>
    /// Управляемое значение для FirstDate.
    /// </summary>
    public DepValue<DateTime> FirstDateEx
    {
      get
      {
        InitFirstDateEx();
        return _FirstDateEx;
      }
      set
      {
        InitFirstDateEx();
        _FirstDateEx.Source = value;
      }
    }
    private DepInput<DateTime> _FirstDateEx;

    private void InitFirstDateEx()
    {
      if (_FirstDateEx == null)
      {
        _FirstDateEx = new DepInput<DateTime>();
        _FirstDateEx.OwnerInfo = new DepOwnerInfo(this, "FirstDateEx");
        _FirstDateEx.Value = FirstDate;
        _FirstDateEx.ValueChanged += new EventHandler(FirstDateEx_ValueChanged);
      }
    }

    private void FirstDateEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        FirstDate = _FirstDateEx.Value;
    }

    #endregion

    #region LastDate

    /// <summary>
    /// Конечная дата диапазона.
    /// Пустой диапазон не поддерживается. Если DateRange.IsEmpty=true, возвращается максимально возможная дата.
    /// Установка значения свойства может привести к изменению начальной даты, если задаваемое значение меньше, чем текущая начальная дата.
    /// </summary>
    public DateTime LastDate
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
          DateRange = new DateRange(DataTools.Min(DateRange.FirstDate, value.Date), value);
      }
    }

    /// <summary>
    /// Управляемое значение для LastDate.
    /// </summary>
    public DepValue<DateTime> LastDateEx
    {
      get
      {
        InitLastDateEx();
        return _LastDateEx;
      }
      set
      {
        InitLastDateEx();
        _LastDateEx.Source = value;
      }
    }
    private DepInput<DateTime> _LastDateEx;


    private void InitLastDateEx()
    {
      if (_LastDateEx == null)
      {
        _LastDateEx = new DepInput<DateTime>();
        _LastDateEx.OwnerInfo = new DepOwnerInfo(this, "LastDateEx");
        _LastDateEx.Value = LastDate;
        _LastDateEx.ValueChanged += new EventHandler(LastDateEx_ValueChanged);
      }
    }

    private void LastDateEx_ValueChanged(object sender, EventArgs args)
    {
      if (!_InsideValueChanged)
        LastDate = _LastDateEx.Value;
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
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
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

      EFPDateOrRangeBoxCommandItems CommandItems2 = CommandItemsIfAssigned as EFPDateOrRangeBoxCommandItems;
      if (CommandItems2 != null)
        CommandItems2.InitCommandItemsState(); // 03.06.2019

      Validate();
      DoSyncValueChanged();
    }

    #endregion

    #endregion

    #region Свойства FirstValueEx и LastValueEx

    /// <summary>
    /// Управляемое значение для начальной даты диапазона Value.FirstDate.
    /// Управляемое свойство возвращает значение null, если интервал не задан.
    /// Свойство предназначено только для чтения, а не для установки значения снаружи.
    /// </summary>
    public DepValue<DateTime?> FirstValueEx
    {
      get
      {
        if (_FirstValueEx == null)
        {
          _FirstValueEx = new DepValueObject<DateTime?>();
          _FirstValueEx.OwnerInfo = new DepOwnerInfo(this, "FirstValueEx");
          if (DateRange.IsEmpty)
            _FirstValueEx.OwnerSetValue(null);
          else
            _FirstValueEx.OwnerSetValue(DateRange.FirstDate);
        }
        return _FirstValueEx;
      }
    }
    private DepValueObject<DateTime?> _FirstValueEx;

    /// <summary>
    /// Управляемое значение для конечной даты диапазона Value.LastDate.
    /// Управляемое свойство возвращает значение null, если интервал не задан.
    /// Свойство предназначено только для чтения, а не для установки значения снаружи.
    /// </summary>
    public DepValue<DateTime?> LastValueEx
    {
      get
      {
        if (_LastValueEx == null)
        {
          _LastValueEx = new DepValueObject<DateTime?>();
          _LastValueEx.OwnerInfo = new DepOwnerInfo(this, "LastValueEx");
          if (DateRange.IsEmpty)
            _LastValueEx.OwnerSetValue(null);
          else
            _LastValueEx.OwnerSetValue(DateRange.LastDate);
        }
        return _LastValueEx;
      }
    }
    private DepValueObject<DateTime?> _LastValueEx;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// True, если можно выбирать значение DateRange.Empty.
    /// Значение совпадает со свойством UserMaskedComboBox.ClearButton, которое по умолчанию имеет значение false.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return Control.ClearButton; }
      set
      {
        if (value == Control.ClearButton)
          return;
        Control.ClearButton = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }

    /// <summary>
    /// Управляемое свойство для CanBeEmpty
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Выдавать предупреждение, если текст не введен (при условии, что CanBeEmpty=true)
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Если True и свойство CanBeEmpty=True, то при проверке состояния выдается
    /// предупреждение, если свойство Value=null.
    /// По умолчанию - False
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion

    #region Свойство DefaultYear

    /// <summary>
    /// Год по умолчанию
    /// Если задано ненулевое значение и пользователь ввел только 4 цифры дня и месяца,
    /// дата будет считаться корректной и использовать выбранный год
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
    /// Свойство DefaultYearEx
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
        _DefaultYearEx = new DepInput<int>();
        _DefaultYearEx.OwnerInfo = new DepOwnerInfo(this, "DefaultYearEx");
        _DefaultYearEx.Value = DefaultYear;
        _DefaultYearEx.ValueChanged += new EventHandler(DefaultYearEx_ValueChanged);
      }
    }

    //void Control_DefaultYearChanged(object sender, EventArgs args)
    //{
    //  _DefaultYearEx.Value = DefaultYear;
    //}

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
    /// Управляемое свойство ReadOnly
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
        _ReadOnlyEx = new DepInput<Boolean>();
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
        _ReadOnlyEx.Value = false;
        _ReadOnlyEx.ValueChanged += new EventHandler(ReadOnlyEx_ValueChanged);

        _ReadOnlyMain = new DepInput<bool>();
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");
        _ReadOnlyMain.Value = false;

        _NotReadOnlySync = new DepInput<bool>();
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");
        _NotReadOnlySync.Value = true;

        DepOr ReadOnlyOr = new DepOr(_ReadOnlyMain, new DepNot(_NotReadOnlySync));
        _ReadOnlyEx.Source = ReadOnlyOr;
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
    /// Если значение свойства установлено и свойство Value меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value больше заданной даты, будет выдана ошибка
    /// при проверке контроля
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private DateTime? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

    #endregion

    #region Свойство Formatter

    /// <summary>
    /// Форматировщик диапазона дат.
    /// Если свойство не установлено в явном виде, возвращает DateRangeFormatter.Default
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
  /// Команды меню для EFPDateRangeBox
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
      _ControlProvider = controlProvider;

      ciToday = new EFPCommandItem("Edit", "Today");
      ciToday.MenuText = "Сегодня";
      ciToday.ShortCut = Keys.Control | Keys.T;
      ciToday.Click += ciToday_Click;
      Add(ciToday);

      ciMonth = new EFPCommandItem("Edit", "Month");
      ciMonth.MenuText = "Месяц";
      ciMonth.ShortCut = Keys.Control | Keys.M;
      ciMonth.Click += ciMonth_Click;
      ciMonth.GroupBegin = true;
      Add(ciMonth);

      ciQuarter = new EFPCommandItem("Edit", "Quarter");
      ciQuarter.MenuText = "Квартал";
      ciQuarter.ShortCut = Keys.Control | Keys.Q;
      ciQuarter.Click += ciQuarter_Click;
      Add(ciQuarter);

      ciYear = new EFPCommandItem("Edit", "Year");
      ciYear.MenuText = "Год";
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
    public EFPDateOrRangeBox ControlProvider { get { return _ControlProvider; } }
    private EFPDateOrRangeBox _ControlProvider;

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
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      ControlProvider.DateRange = new DateRange(FirstDateValue, LastDateValue);
    }

    private void GetPeriodValues(int mode, out DateTime firstDateValue, out DateTime lastDateValue)
    {
      DateTime Date;
      if (ControlProvider.DateRange.IsEmpty)
        Date = DateTime.Today;
      else
        Date = ControlProvider.DateRange.FirstDate;

      int m1 = Date.Month;
      int m2 = Date.Month;

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
      firstDateValue = new DateTime(Date.Year, m1, 1);
      lastDateValue = new DateTime(Date.Year, m2, DateTime.DaysInMonth(Date.Year, m2));
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
      DateTime FirstDateValue, LastDateValue;
      GetPeriodValues(mode, out FirstDateValue, out LastDateValue);
      return ControlProvider.Formatter.ToString(FirstDateValue, LastDateValue, false);
    }

    #endregion
  }

}
