using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;

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

/*
 * Переходники между полями ввода чисел NumericUpDown и NumEditBox и числовыми
 * полями данных.
 * Типы данных: decimal, int, float и double.
 * Специальные версии для управляющих эоементов и включающего флажка CheckBox
 */

namespace FreeLibSet.Forms.Docs
{
  #region Decimal

  /// <summary>
  /// Переходник для числового поля
  /// </summary>
  public class DocValueDecimalNumEditBox : DocValueControlBase2<decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueDecimalNumEditBox(DBxDocValue docValue, IEFPNumEditBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DecimalValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new IEFPNumEditBox ControlProvider { get { return (IEFPNumEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsDecimal;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetDecimal(CurrentValueEx.Value);
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

  #region Integer

  /// <summary>
  /// Переходник для числового поля
  /// </summary>
  public class DocValueIntNumEditBox : DocValueControlBase2<int>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueIntNumEditBox(DBxDocValue docValue, IEFPNumEditBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.IntValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new IEFPNumEditBox ControlProvider { get { return (IEFPNumEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetInteger(CurrentValueEx.Value);
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
      return ControlProvider2.IntValue;
    }

    protected override void ValueToControl()
    {
      int v = DocValue.AsInteger;
      CurrentValue.Value = v;
      if (v != 0)
        ControlProvider2.IntValue = v;
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
  public class DocValueSingleNumEditBox : DocValueControlBase2<float>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="docValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public DocValueSingleNumEditBox(DBxDocValue docValue, IEFPNumEditBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.SingleValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new IEFPNumEditBox ControlProvider { get { return (IEFPNumEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsSingle;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetSingle(CurrentValueEx.Value);
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
  public class DocValueDoubleNumEditBox : DocValueControlBase2<double>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="docValue">Объект для доступа к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">True, если допускается групповое редактирование</param>
    public DocValueDoubleNumEditBox(DBxDocValue docValue, IEFPNumEditBox controlProvider, bool canMultiEdit)
      : base(docValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DoubleValueEx);
      DepOr.AttachInput(controlProvider.ReadOnlyEx, DepNot.NotOutput(EnabledEx));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new IEFPNumEditBox ControlProvider { get { return (IEFPNumEditBox)(base.ControlProvider); } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = DocValue.AsDouble;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      DocValue.SetDouble(CurrentValueEx.Value);
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
}
