// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Data;

/*
 * Переходники между полями ввода чисел NumericUpDown и NumEditBox и числовыми
 * полями данных.
 * Типы данных: decimal, int, float и double.
 * Специальные версии для управляющих эоементов и включающего флажка CheckBox
 */

namespace FreeLibSet.Forms.Data
{
  #region Integer

  /// <summary>
  /// Переходник для числового поля
  /// </summary>
  public class ExtValueInt32EditBox : ExtValueControlBase2<int?>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInt32EditBox(DBxExtValue extValue, EFPInt32EditBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPInt32EditBox ControlProvider { get { return (EFPInt32EditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (this.ControlProvider.CanBeEmpty && ExtValue.IsNull)
        CurrentValueEx.Value = null;
      else
        CurrentValueEx.Value = ExtValue.AsInt32;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (this.ControlProvider.CanBeEmpty && (!CurrentValueEx.Value.HasValue))
        ExtValue.SetNull();
      else
        ExtValue.SetInt32(CurrentValueEx.Value ?? 0);
    }

    #endregion
  }

#if XXX
  public class DocValueCheckBoxWithIntNumEditBox : DocValueCheckBoxWithControl<int>
  {
  #region Конструктор

    public DocValueCheckBoxWithIntNumEditBox(IDocValue DocValue, EFPCheckBox ControlProvider1,
      IEFPNumEditBox ControlProvider2, bool CanMultiEdit)
      : base(DocValue, ControlProvider1, ControlProvider2, CanMultiEdit)
    {
      ControlProvider2.EnabledEx = new DepAnd(ControlProvider1.EnabledEx,
        new DepEqual<CheckState>(ControlProvider1.CheckStateEx, CheckState.Checked));
      ControlProvider2.DecimalValueEx.ValueChanged += new EventHandler(ControlChanged2);
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Провайдер основного управляющего элемента для ввода числового значения
    /// </summary>
    public new IEFPNumEditBox ControlProvider2 { get { return (IEFPNumEditBox)(base.ControlProvider2); } }

  #endregion

  #region Переопределенные методы и свойства

    protected override int ZeroValue
    {
      get { return 0; }
    }

    protected override int GetControlValue2()
    {
      return ControlProvider2.Int32Value;
    }

    protected override void ValueToControl()
    {
      int v = DocValue.AsInteger;
      CurrentValue.Value = v;
      if (v != 0)
        ControlProvider2.Int32Value = v;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0)
        DocValue.SetNull();
      else
        DocValue.AsInteger = CurrentValue.Value;
    }

  #endregion
  }
#endif

  #endregion

  #region Single

  /// <summary>
  /// Переходник для числового поля
  /// </summary>
  public class ExtValueSingleEditBox : ExtValueControlBase2<float?>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueSingleEditBox(DBxExtValue extValue, EFPSingleEditBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPSingleEditBox ControlProvider { get { return (EFPSingleEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (this.ControlProvider.CanBeEmpty && ExtValue.IsNull)
        CurrentValueEx.Value = null;
      else
        CurrentValueEx.Value = ExtValue.AsSingle;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (this.ControlProvider.CanBeEmpty && (!CurrentValueEx.Value.HasValue))
        ExtValue.SetNull();
      else
        ExtValue.SetSingle(CurrentValueEx.Value ?? 0f);
    }

    #endregion
  }

#if XXX
  public class DocValueCheckBoxWithSingleNumEditBox : DocValueCheckBoxWithControl<float>
  {
  #region Конструктор

    public DocValueCheckBoxWithSingleNumEditBox(IDocValue DocValue, EFPCheckBox ControlProvider1,
      IEFPNumEditBox ControlProvider2, bool CanMultiEdit)
      : base(DocValue, ControlProvider1, ControlProvider2, CanMultiEdit)
    {
      ControlProvider2.EnabledEx = new DepAnd(ControlProvider1.EnabledEx,
        new DepEqual<CheckState>(ControlProvider1.CheckStateEx, CheckState.Checked));
      ControlProvider2.DecimalValueEx.ValueChanged += new EventHandler(ControlChanged2);
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Провайдер основного управляющего элемента для ввода числового значения
    /// </summary>
    public new IEFPNumEditBox ControlProvider2 { get { return (IEFPNumEditBox)(base.ControlProvider2); } }

  #endregion

  #region Переопределенные методы и свойства

    protected override float ZeroValue
    {
      get { return 0f; }
    }

    protected override float GetControlValue2()
    {
      return ControlProvider2.SingleValue;
    }

    protected override void ValueToControl()
    {
      float v = DocValue.AsSingle;
      CurrentValue.Value = v;
      if (v != 0f)
        ControlProvider2.SingleValue = v;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0f)
        DocValue.SetNull();
      else
        DocValue.AsSingle = CurrentValue.Value;
    }

  #endregion
  }
#endif

  #endregion

  #region Double

  /// <summary>
  /// Переходник для числового поля
  /// </summary>
  public class ExtValueDoubleEditBox : ExtValueControlBase2<double?>
  {
    #region Конструктор

      /// <summary>
      /// Создает переходник
      /// </summary>
      /// <param name="extValue">Объект для доступа к значению поля</param>
      /// <param name="controlProvider">Провайдер управляющего элемента</param>
      /// <param name="canMultiEdit">True, если допускается групповое редактирование</param>
    public ExtValueDoubleEditBox(DBxExtValue extValue, EFPDoubleEditBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPDoubleEditBox ControlProvider { get { return (EFPDoubleEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (this.ControlProvider.CanBeEmpty && ExtValue.IsNull)
        CurrentValueEx.Value = null;
      else
        CurrentValueEx.Value = ExtValue.AsDouble;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (this.ControlProvider .CanBeEmpty && (!CurrentValueEx.Value.HasValue))
        ExtValue.SetNull();
      else
        ExtValue.SetDouble(CurrentValueEx.Value ?? 0.0);
    }

    #endregion
  }

#if XXX
  public class DocValueCheckBoxWithDoubleNumEditBox : DocValueCheckBoxWithControl<double>
  {
  #region Конструктор

    public DocValueCheckBoxWithDoubleNumEditBox(IDocValue DocValue, EFPCheckBox ControlProvider1,
      IEFPNumEditBox ControlProvider2, bool CanMultiEdit)
      : base(DocValue, ControlProvider1, ControlProvider2, CanMultiEdit)
    {
      ControlProvider2.EnabledEx = new DepAnd(ControlProvider1.EnabledEx,
        new DepEqual<CheckState>(ControlProvider1.CheckStateEx, CheckState.Checked));
      ControlProvider2.DecimalValueEx.ValueChanged += new EventHandler(ControlChanged2);
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Провайдер основного управляющего элемента для ввода числового значения
    /// </summary>
    public new IEFPNumEditBox ControlProvider2 { get { return (IEFPNumEditBox)(base.ControlProvider2); } }

  #endregion

  #region Переопределенные методы и свойства

    protected override double ZeroValue
    {
      get { return 0.0; }
    }

    protected override double GetControlValue2()
    {
      return ControlProvider2.DoubleValue;
    }

    protected override void ValueToControl()
    {
      double v = DocValue.AsDouble;
      CurrentValue.Value = v;
      if (v != 0.0)
        ControlProvider2.DoubleValue = v;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0.0)
        DocValue.SetNull();
      else
        DocValue.AsDouble = CurrentValue.Value;
    }

  #endregion
  }
#endif

  #endregion

  #region Decimal

  /// <summary>
  /// Переходник для числового поля
  /// </summary>
  public class ExtValueDecimalEditBox : ExtValueControlBase2<decimal?>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDecimalEditBox(DBxExtValue extValue, EFPDecimalEditBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.NValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPDecimalEditBox ControlProvider { get { return (EFPDecimalEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      if (this.ControlProvider.CanBeEmpty && ExtValue.IsNull)
        CurrentValueEx.Value = null;
      else
        CurrentValueEx.Value = ExtValue.AsDecimal;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (this.ControlProvider.CanBeEmpty && (!CurrentValueEx.Value.HasValue))
        ExtValue.SetNull();
      else
        ExtValue.SetDecimal(CurrentValueEx.Value ?? 0m);
    }

    #endregion
  }

#if XXXX
  public class DocValueCheckBoxWithDecimalNumEditBox : DocValueCheckBoxWithControl<decimal>
  {
  #region Конструктор

    public DocValueCheckBoxWithDecimalNumEditBox(IDocValue DocValue, EFPCheckBox ControlProvider1,
      IEFPNumEditBox ControlProvider2, bool CanMultiEdit)
      : base(DocValue, ControlProvider1, ControlProvider2, CanMultiEdit)
    {
      ControlProvider2.EnabledEx = new DepAnd(ControlProvider1.EnabledEx,
        new DepEqual<CheckState>(ControlProvider1.CheckStateEx, CheckState.Checked));
      ControlProvider2.DecimalValueEx.ValueChanged += new EventHandler(ControlChanged2);
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Провайдер основного управляющего элемента для ввода числового значения
    /// </summary>
    public new IEFPNumEditBox ControlProvider2 { get { return (IEFPNumEditBox)(base.ControlProvider2); } }

  #endregion

  #region Переопределенные методы и свойства

    protected override decimal ZeroValue
    {
      get { return 0; }
    }

    protected override decimal GetControlValue2()
    {
      return ControlProvider2.DecimalValue;
    }

    protected override void ValueToControl()
    {
      decimal v = DocValue.AsDecimal;
      CurrentValue.Value = v;
      if (v != 0m)
        ControlProvider2.DecimalValue = v;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0m)
        DocValue.SetNull();
      else
        DocValue.AsDecimal = CurrentValue.Value;
    }

  #endregion
  }
#endif

  #endregion
}
