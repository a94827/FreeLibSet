using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtForms;
using System.Windows.Forms;
using AgeyevAV.DependedValues;
using AgeyevAV.ExtDB.Docs;

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

namespace AgeyevAV.ExtForms.Docs
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

      DepInput<bool> CurrentValueInput = new DepInput<bool>();
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>();
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider.ThreeState = docValue.Grayed;
      if (docValue.Grayed)
        controlProvider.CheckStateEx = new DepExpr2<CheckState, bool, bool>(GrayedEx, CurrentValueEx,
        new DepFunction2<CheckState, bool, bool>(CalcCheckState));
      else
        controlProvider.CheckedEx = CurrentValueEx;
      controlProvider.CheckStateEx.ValueChanged += new EventHandler(CheckStateValueChanged);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    void CheckStateValueChanged(object sender, EventArgs args)
    {
      if (DocValue.Grayed)
      {
        GrayedEx.Value = (ControlProvider.CheckState == CheckState.Indeterminate);
        if (!GrayedEx.Value)
          CurrentValueEx.Value = ControlProvider.CheckState == CheckState.Checked;
        else
          CurrentValueEx.Value = false; // иначе останетсч признак измененных данных
      }
      else
        CurrentValueEx.Value = ControlProvider.Checked;

      ControlChanged(sender, args);
    }

    /// <summary>
    /// Вычисление состояния для режима CanGrayed
    /// </summary>
    private CheckState CalcCheckState(bool arg1, bool arg2)
    {
      if (arg1)
        return CheckState.Indeterminate;
      else
      {
        if (arg2)
          return CheckState.Checked;
        else
          return CheckState.Unchecked;
      }
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

      DepInput<TValue> CurrentValueInput = new DepInput<TValue>();
      CurrentValueInput.OwnerInfo = new DepOwnerInfo(this, "CurrentValueInput");
      CurrentValueInput.ValueChanged += new EventHandler(CurrentValueInput_ValueChanged);
      base.SetCurrentValueEx(CurrentValueInput);

      DepInput<bool> GrayedInput = new DepInput<bool>();
      GrayedInput.OwnerInfo = new DepOwnerInfo(this, "GrayedInput");
      base.GrayedEx = GrayedInput;

      controlProvider1.ThreeState = docValue.Grayed;

      controlProvider1.CheckStateEx = new DepExpr2<CheckState, bool, TValue>(base.GrayedEx, base.CurrentValueEx,
        new DepFunction2<CheckState, bool, TValue>(CalcCheckState));

      controlProvider1.CheckStateEx.ValueChanged += new EventHandler(CheckStateValueChanged);
      DepAnd.AttachInput(controlProvider1.EnabledEx, EnabledEx);

      controlProvider2.EnabledEx = new DepAnd(controlProvider1.EnabledEx,
        new DepEqual<CheckState>(controlProvider1.CheckStateEx, CheckState.Checked));

      if (MultiEditDisabled)
        controlProvider2.Visible = false;
    }

    void CheckStateValueChanged(object sender, EventArgs args)
    {
      if (ControlProvider1.ThreeState)
      {
        GrayedEx.Value = (ControlProvider1.CheckState == CheckState.Indeterminate);
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
    private CheckState CalcCheckState(bool grayed, TValue currentValue)
    {
      // Метод не может быть static, т.к. обращается к ZeroValue
      if (grayed)
        return CheckState.Indeterminate;
      else
      {
        if (object.Equals(currentValue, ZeroValue))
          return CheckState.Unchecked;
        else
          return CheckState.Checked;
      }
    }

    void CurrentValueInput_ValueChanged(object sender, EventArgs args)
    {
      if (object.Equals(CurrentValueEx.Value, ZeroValue))
        ControlProvider1.CheckState = CheckState.Unchecked;
      else
      {
        ControlProvider1.CheckState = CheckState.Checked;
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
      CheckStateValueChanged(sender, args); // там все есть
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
