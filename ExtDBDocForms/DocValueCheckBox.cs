// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Переходник для CheckBox для логического поля
  /// </summary>
  public class DocValueCheckBox : DocValueControl<bool, EFPCheckBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueCheckBox(DBxDocValue docValue, EFPCheckBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, false, canMultiEdit)
    {
      Inverted = false;

      DepInput<bool> CurrentValueInput = new DepInput<bool>(false, null);
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>(false, null);
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider.CanBeEmpty = docValue.Grayed;
      if (docValue.Grayed)
        controlProvider.NCheckedEx = new DepExpr2<bool?, bool, bool>(GrayedEx, CurrentValueEx,
        new DepFunction2<bool?, bool, bool>(CalcNChecked));
      else
        controlProvider.CheckedEx = CurrentValueEx;
      controlProvider.NCheckedEx.ValueChanged += new EventHandler(NCheckedValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    void NCheckedValueChanged(object sender, EventArgs args)
    {
      if (DocValue.Grayed)
      {
        Grayed = !ControlProvider.NChecked.HasValue;
        if (!GrayedEx.Value)
          CurrentValueEx.Value = ControlProvider.Checked;
        else
          CurrentValueEx.Value = false; // иначе останется признак измененных данных
      }
      else
        CurrentValueEx.Value = ControlProvider.Checked;

      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление состояния для режима CanGrayed
    /// </summary>
    private bool? CalcNChecked(bool arg1, bool arg2)
    {
      if (arg1)
        return null;
      else
        return arg2;
    }

    #endregion

    #region Дополнительное свойство

    /// <summary>
    /// Если свойство установить в true, то записываемое в поле значение будет 
    /// инвертировано, то есть значению false будет соответствовать включенный
    /// крестик, а true - выключенный.
    /// По умолчанию свойство установлено в false.
    /// </summary>
    public bool Inverted { get { return _Inverted; } set { _Inverted = value; } }
    private bool _Inverted;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Кнопки доступны (EnabledEx=False), даже в режиме Grayed.
    /// </summary>
    /// <returns>true, если значение поля можно редактировать</returns>
    protected override bool GetEnabledState()
    {
      return !DocValue.IsReadOnly;
    }

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (Inverted)
        CurrentValueEx.Value = !DocValue.AsBoolean;
      else
        CurrentValueEx.Value = DocValue.AsBoolean;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (Inverted)
        DocValue.SetBoolean(!CurrentValueEx.Value);
      else
        DocValue.SetBoolean(CurrentValueEx.Value);
    }

    #endregion
  }


  /// <summary>
  /// Базовый класс переходника для комбинации управляющих элементов вида CheckBox+Управляющий элемент.
  /// Когда флажок включен, пользователь может редактировать значение в основном управляющем элементе.
  /// Выключенное состояние флажка означает пустое значение.
  /// При редактировании "серых" значений флажок переводится в "промежуточное" состояние.
  /// 
  /// Как правило, конструкция из флажка и элемента используется для управляющих элементов, не допускающих ввода пустого
  /// значения.
  /// </summary>
  /// <typeparam name="TValue">Тип редактируемого значения</typeparam>
  /// <typeparam name="TControlProvider">Класс провайдера основного управляющего элемента, производного от EFPControlBase</typeparam>
  public abstract class DocValueCheckBoxWithControl<TValue, TControlProvider> : DocValueControlBase2<TValue>
    where TControlProvider : EFPControlBase
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="docValue">Редактируемое значение</param>
    /// <param name="controlProvider1">Провайдер переключателя CheckBox</param>
    /// <param name="controlProvider2">Провайдер основного управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирование (разрешаются "серые" значения)</param>
    public DocValueCheckBoxWithControl(DBxDocValue docValue, EFPCheckBox controlProvider1, TControlProvider controlProvider2, bool canMultiEdit)
      : base(docValue, controlProvider1, false, canMultiEdit)
    {
      _ControlProvider2 = controlProvider2;

      DepInput<TValue> CurrentValueInput = new DepInput<TValue>(default(TValue),CurrentValueInput_ValueChanged); // ???
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      base.SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>(false, null);
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider1.CanBeEmpty = docValue.Grayed;

      controlProvider1.NCheckedEx = new DepExpr2<bool?, bool, TValue>(base.GrayedEx, base.CurrentValueEx,
        new DepFunction2<bool?, bool, TValue>(CalcNChecked));

      controlProvider1.NCheckedEx.ValueChanged += new EventHandler(NCheckedValueChanged);
      DepAnd.AttachInput(controlProvider1.EnabledEx, EnabledEx);

      controlProvider2.EnabledEx = new DepAnd(controlProvider1.EnabledEx,
        controlProvider1.CheckedEx);

      if (MultiEditDisabled)
        controlProvider2.Visible = false;
    }

    void NCheckedValueChanged(object sender, EventArgs args)
    {
      if (ControlProvider1.CanBeEmpty)
      {
        Grayed = !ControlProvider1.NChecked.HasValue;
        if (!GrayedEx.Value)
          CurrentValueEx.Value = GetControlValue2();
      }
      else
      {
        if (ControlProvider1.Checked)
          CurrentValueEx.Value = GetControlValue2();
        else
          CurrentValueEx.Value = ZeroValue;
      }

      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление состояния для режима CanGrayed.
    /// </summary>
    private bool? CalcNChecked(bool grayed, TValue currentValue)
    {
      // Метод не может быть static, т.к. обращается к ZeroValue
      if (grayed)
        return null;
      else
        return !object.Equals(currentValue, ZeroValue);
    }

    void CurrentValueInput_ValueChanged(object sender, EventArgs args)
    {
      if (object.Equals(CurrentValueEx.Value, ZeroValue))
        ControlProvider1.Checked = false;
      else
      {
        ControlProvider1.Checked = true;
        SetControlValue2(CurrentValueEx.Value);
      }
    }

    #endregion

    #region Виртуальные методы

    /// <summary>
    /// Метод возвращает пустое значение.
    /// Для строкового типа свойство должно быть переопределено и возвращать "".
    /// </summary>
    protected virtual TValue ZeroValue { get { return default(TValue); } }

    /// <summary>
    /// Метод должен записать (непустое) значение в основной управляющий элемент ControlProvider2.
    /// </summary>
    /// <param name="value">Записываемое значение</param>
    protected abstract void SetControlValue2(TValue value);

    /// <summary>
    /// Метод должен вернуть (непустое) значение из основного управляющего элемента ControlProvider2.
    /// </summary>
    /// <returns></returns>
    protected abstract TValue GetControlValue2();

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Провайдер переключателя CheckBox
    /// </summary>
    public EFPCheckBox ControlProvider1 { get { return (EFPCheckBox)(base.ControlProvider); } }

    /// <summary>
    /// Провайдер основного управляющего элемента
    /// </summary>
    public TControlProvider ControlProvider2 { get { return _ControlProvider2; } }
    private TControlProvider _ControlProvider2;

    /// <summary>
    /// Вызывается производным классом при изменении значения в основном управляющем
    /// элементе.
    /// Обновляет CurrentValue и вызывает базовый метод ControlChanged
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected void ControlChanged2(object sender, EventArgs args)
    {
      NCheckedValueChanged(sender, args); // там все есть
      //ControlChanged(Sender, Args);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Кнопки доступны (EnabledEx=False), даже в режиме Grayed.
    /// </summary>
    /// <returns></returns>
    protected override bool GetEnabledState()
    {
      return !DocValue.IsReadOnly;
    }


    #endregion
  }

  /// <summary>
  /// Переходник для флажка CheckBox и поля выбора месяца и дня MonthDayBox.
  /// В отличие от DocValueIntMonthDayBox, разрешает запись нулевого значения.
  /// </summary>
  public class DocValueIntCheckBoxWithMonthDayBox : DocValueCheckBoxWithControl<int, EFPMonthDayBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="docValue">Редактируемое значение</param>
    /// <param name="controlProvider1">Провайдер переключателя CheckBox</param>
    /// <param name="controlProvider2">Провайдер основного управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирование (разрешаются "серые" значения)</param>
    public DocValueIntCheckBoxWithMonthDayBox(DBxDocValue docValue, EFPCheckBox controlProvider1, EFPMonthDayBox controlProvider2, bool canMultiEdit)
      : base(docValue, controlProvider1, controlProvider2, canMultiEdit)
    {
      controlProvider2.DayOfYearEx.ValueChanged += new EventHandler(base.ControlChanged2);
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Запись значения в CurrentValueEx.
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Запись значения в DocValue
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
    }

    /// <summary>
    /// Получение EFPMonthDayBox.IntValue.
    /// </summary>
    /// <returns>Значение</returns>
    protected override Int32 GetControlValue2()
    {
      return ControlProvider2.DayOfYear;
    }
                         
    /// <summary>
    /// Установка EFPMonthDayBox.IntValue.
    /// </summary>
    /// <param name="value">Значение</param>
    protected override void SetControlValue2(Int32 value)
    {
      ControlProvider2.DayOfYear = value;
    }
                                 
    #endregion
  }


#if XXX
  /// <summary>
  /// Переходник для EFPDocTypeComboBox с использованием в качестве значения идентификатора таблицы
  /// Реализация для дополнительного CheckBox'а, который разрешает выбор из списка
  /// </summary>
  public class DocValueCheckBoxWithDocTypeComboBoxByTableId : DocValueCheckBoxWithControl<int>
  {
  #region Конструктор

    public DocValueCheckBoxWithDocTypeComboBoxByTableId(IDocValue DocValue, EFPCheckBox ControlProvider1, EFPDocTypeComboBox ControlProvider2, bool CanMultiEdit)
      : base(DocValue, ControlProvider1, ControlProvider2, CanMultiEdit)
    {
      ControlProvider2.EnabledEx = new DepAnd(ControlProvider1.EnabledEx,
        new DepEqual<CheckState>(ControlProvider1.CheckStateEx, CheckState.Checked));
      ControlProvider2.DocTableIdEx.ValueChanged += new EventHandler(ControlChanged2);
    }

    #endregion

  #region Переопределенные методы и свойства

    public new EFPDocTypeComboBox ControlProvider2 { get { return (EFPDocTypeComboBox)(base.ControlProvider2); } }

    protected override void ValueToControl()
    {
      Int32 v = DocValue.AsInteger;
      CurrentValue.Value = v;
      if (v != 0)
        ControlProvider2.DocTableId = v;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0)
        DocValue.SetNull();
      else
        DocValue.AsInteger = CurrentValue.Value;
    }

    protected override Int32 ZeroValue { get { return 0; } }

    protected override Int32 GetControlValue2()
    {
      return ControlProvider2.DocTableId;
    }

    protected void SetControlValue2(Int32 Value)
    {
    }

    #endregion
  }

#endif

#if XXX
  public interface IDocValueNullableControl:IDocEditItem
  {
    IEFPControl ControlProvider { get;}
    /// <summary>
    /// True, если редактирование запрещено из-за наличия нескольких документов
    /// </summary>
    bool MultiEditDisabled { get;}
  }

  public abstract class DocValueNullableWithControls : IDocEditItem
  {
    #region Конструктор

    public DocValueNullableWithControls(EFPControlBase ControlProvider1, params IDocValueNullableControl[] MainItems)
    {
      if (MainItems.Length < 0)
        throw new ArgumentException("Должен быть, как минимум, один основной элемент");
      FMainItems = MainItems;
      FMultiEditDisabled = false;
      FChangeInfo = new DepChangeInfoList();
      for (int i = 0; i < MainItems.Length; i++)
      {
        FChangeInfo.Add(MainItems[i].ChangeInfo);
        FMultiEditDisabled |= MainItems[i].MultiEditDisabled;
        MainItems[i].ControlProvider.VisibleEx = ControlProvider1.VisibleEx;
      }
      ControlProvider1.Visible = !FMultiEditDisabled;
    }

    #endregion

    #region Свойства

    public IDocValueNullableControl[] MainItems { get { return FMainItems; } }
    private IDocValueNullableControl[] FMainItems;


    /// <summary>
    /// True, если редактирование запрещено из-за наличия нескольких документов
    /// </summary>
    public bool MultiEditDisabled { get { return FMultiEditDisabled; } }
    private bool FMultiEditDisabled;

    #endregion

    #region IDocEditItem Members

    public void BeforeReadValues()
    {
      for (int i = 0; i < MainItems.Length; i++)
      { 
      }
    }

    public void ReadValues()
    {
      throw new NotImplementedException("The method or operation is not implemented.");
    }

    public void AfterReadValues()
    {
      throw new NotImplementedException("The method or operation is not implemented.");
    }

    public void WriteValues()
    {
      throw new NotImplementedException("The method or operation is not implemented.");
    }

    public DepChangeInfo ChangeInfo
    {
      get { throw new NotImplementedException("The method or operation is not implemented."); }
    }
    private DepChangeInfoList FChangeInfo;

    #endregion
  }

  /// <summary>
  /// Переходник для флажка CheckBox и одного или нескольких переходников
  /// </summary>
  public class DocValueCheckBoxWithControls : DocValueNullableWithControls
  {
  #region Конструктор

    public DocValueCheckBoxWithControls(EFPCheckBox ControlProvider1, params IDocValueNullableControl[] MainItems)
      :base(ControlProvider1, MainItems)
    {
      FControlProvider1 = ControlProvider1;
    }

  #endregion

  #region Свойства

    public EFPCheckBox ControlProvider1 { get { return FControlProvider1; } }
    private EFPCheckBox FControlProvider1;

  #endregion
  }
#endif
}
