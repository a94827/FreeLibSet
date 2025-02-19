﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.DependedValues;
using FreeLibSet.UICore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер для <see cref="CheckBox"/>.
  /// Поддерживаются переключатели на два положения и на три положения (при <see cref="CanBeEmpty"/>=true)
  /// </summary>
  public class EFPCheckBox : EFPControl<CheckBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPCheckBox(EFPBaseProvider baseProvider, CheckBox control)
      : base(baseProvider, control, false)
    {
      if (control.CheckState == CheckState.Indeterminate)
        _SavedNChecked = null;
      else
        _SavedNChecked = control.Checked;
      _AllowDisabledChecked = false;
      _DisabledNChecked = false;
      _CanBeEmptyMode = UIValidateState.Error;

      if (!DesignMode)
        control.CheckStateChanged += new EventHandler(Control_CheckStateChanged);
    }

    #endregion

    #region Enabled

    /// <summary>
    /// Инициализация "серого" значения
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledChecked && EnabledState)
        _HasSavedNChecked = true;
      InitControlCheckState();
    }

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Определяет, может ли элемент находиться в промежуточном состоянии.
    /// По умолчанию - Error - обычный переключатель на два положения.
    /// При CanBeEmptyMode=Ok или Warning - разрешено промежуточное состояние.
    /// В режиме Warning для него выдается предупреждение.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        this.Control.ThreeState = value != UIValidateState.Error;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// True, если ли элемент может находиться в промежуточном состоянии.
    /// По умолчанию - false.
    /// Дублирует <see cref="CanBeEmptyMode"/>.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Свойство NChecked

    /// <summary>
    /// Возвращает актуальное состояние элемента <see cref="CheckBox.CheckState"/>.
    /// При установке свойства <see cref="CheckBox.CheckState"/> не устанавливается, если <see cref="Control.Enabled"/>=false
    /// и <see cref="AllowDisabledChecked"/>=true. В этом случае значение запоминается и будет 
    /// использовано при переходе в разрешенное состояние.
    /// </summary>
    public bool? NChecked
    {
      get
      {
        if (Control.CheckState == CheckState.Indeterminate)
          return null;
        else
          return Control.Checked;
      }
      set
      {
        _HasSavedNChecked = true;
        _SavedNChecked = value;
        InitControlCheckState();
      }
    }

    private bool? _SavedNChecked;
    private bool _HasSavedNChecked;

    private void InitControlCheckState()
    {
      // Не нужно, иначе может не обновляться
      // if (InsideCheckStateChanged)
      //   return;
      if (AllowDisabledChecked && (!EnabledState))
      {
        if (DisabledNChecked.HasValue)
          Control.Checked = DisabledNChecked.Value;
        else
          Control.CheckState = CheckState.Indeterminate;
      }
      else if (_HasSavedNChecked)
      {
        _HasSavedNChecked = false;
        if (_SavedNChecked.HasValue)
          Control.Checked = _SavedNChecked.Value;
        else
          Control.CheckState = CheckState.Indeterminate;
      }
    }

    /// <summary>
    /// Управляемое свойство <see cref="NChecked"/>
    /// </summary>
    public DepValue<bool?> NCheckedEx
    {
      get
      {
        InitNCheckedEx();
        return _NCheckedEx;
      }
      set
      {
        InitNCheckedEx();
        _NCheckedEx.Source = value;
      }
    }

    private void InitNCheckedEx()
    {
      if (_NCheckedEx == null)
      {
        _NCheckedEx = new DepInput<bool?>(NChecked, NCheckedEx_ValueChanged);
        _NCheckedEx.OwnerInfo = new DepOwnerInfo(this, "NCheckedEx");
      }
    }

    private DepInput<bool?> _NCheckedEx;

    private void Control_CheckStateChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideCheckStateChanged)
        {
          _InsideCheckStateChanged = true;
          try
          {
            OnCheckStateChanged();
          }
          finally
          {
            _InsideCheckStateChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private bool _InsideCheckStateChanged;

    /// <summary>
    /// Метод вызывается при изменении состояния флажка в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод.
    /// </summary>
    protected virtual void OnCheckStateChanged()
    {
      if (_NCheckedEx != null)
        _NCheckedEx.Value = NChecked;
      if (_CheckedEx != null)
        _CheckedEx.Value = Checked;

      if (AllowDisabledChecked && EnabledState)
      {
        if (Control.CheckState == CheckState.Indeterminate)
          _SavedNChecked = null;
        else
          _SavedNChecked = Control.Checked;
      }

      Validate();
      //DoSyncValueChanged();
    }

    void NCheckedEx_ValueChanged(object sender, EventArgs args)
    {
      NChecked = _NCheckedEx.Value;
    }

    #endregion

    #region Свойство Checked

    /// <summary>
    /// Наличие флажка.
    /// Для промежуточного состояния возвращает false.
    /// </summary>
    public bool Checked
    {
      get { return NChecked ?? false; }
      set
      {
        NChecked = value;
      }
    }

    /// <summary>
    /// Управляемое свойство <see cref="Checked"/>
    /// </summary>
    public DepValue<Boolean> CheckedEx
    {
      get
      {
        InitCheckedEx();
        return _CheckedEx;
      }
      set
      {
        InitCheckedEx();
        _CheckedEx.Source = value;
      }
    }

    private void InitCheckedEx()
    {
      if (_CheckedEx == null)
      {
        _CheckedEx = new DepInput<bool>(Checked, CheckedEx_ValueChanged);
        _CheckedEx.OwnerInfo = new DepOwnerInfo(this, "CheckedEx");
      }
    }

    private DepInput<bool> _CheckedEx;

    void CheckedEx_ValueChanged(object sender, EventArgs args)
    {
      Checked = _CheckedEx.Value;
    }

    #endregion

    #region Свойство DisabledNChecked

    /// <summary>
    /// Значение, используемое при <see cref="Control.Enabled"/>=false
    /// </summary>
    public bool? DisabledNChecked
    {
      get { return _DisabledNChecked; }
      set
      {
        if (value == _DisabledNChecked)
          return;
        _DisabledNChecked = value;
        if (_DisabledNCheckedEx != null)
          _DisabledNCheckedEx.Value = value;
        if (_DisabledCheckedEx != null)
          _DisabledCheckedEx.Value = DisabledChecked;
        InitControlCheckState();
      }
    }
    private bool? _DisabledNChecked;

    /// <summary>
    /// Управляемое свойство для <see cref="DisabledNChecked"/>.
    /// Свойство действует при установленном свойстве <see cref="AllowDisabledChecked"/>.
    /// </summary>
    public DepValue<bool?> DisabledNCheckedEx
    {
      get
      {
        InitDisabledNCheckedEx();
        return _DisabledNCheckedEx;
      }
      set
      {
        InitDisabledNCheckedEx();
        _DisabledNCheckedEx.Source = value;
      }
    }

    private void InitDisabledNCheckedEx()
    {
      if (_DisabledNCheckedEx == null)
      {
        _DisabledNCheckedEx = new DepInput<bool?>(DisabledNChecked, DisabledNCheckedEx_ValueChanged);
        _DisabledNCheckedEx.OwnerInfo = new DepOwnerInfo(this, "DisabledNCheckedEx");
      }
    }
    private DepInput<bool?> _DisabledNCheckedEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckStateEx
    /// </summary>
    private void DisabledNCheckedEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledNChecked = _DisabledNCheckedEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства <see cref="DisabledChecked"/> и <see cref="DisabledNChecked"/>
    /// </summary>
    public bool AllowDisabledChecked
    {
      get { return _AllowDisabledChecked; }
      set
      {
        if (value == _AllowDisabledChecked)
          return;
        _AllowDisabledChecked = value;
        InitControlCheckState();
      }
    }
    private bool _AllowDisabledChecked;

    #endregion

    #region Свойство DisabledChecked

    /// <summary>
    /// Дублирует свойство <see cref="DisabledNChecked"/>.
    /// По умолчанию - false.
    /// </summary>
    public bool DisabledChecked
    {
      get { return DisabledNChecked ?? false; }
      set { DisabledNChecked = value; }
    }

    /// <summary>
    /// Этот текст замещает свойство <see cref="CheckedEx"/>, когда <see cref="EFPControlBase.EnabledEx"/>=false.
    /// Свойство действует при установленном свойстве <see cref="AllowDisabledChecked"/>.
    /// </summary>
    public DepValue<Boolean> DisabledCheckedEx
    {
      get
      {
        InitDisabledCheckedEx();
        return _DisabledCheckedEx;
      }
      set
      {
        InitDisabledCheckedEx();
        _DisabledCheckedEx.Source = value;
      }
    }

    private void InitDisabledCheckedEx()
    {
      if (_DisabledCheckedEx == null)
      {
        _DisabledCheckedEx = new DepInput<Boolean>(DisabledChecked, DisabledCheckedEx_ValueChanged);
        _DisabledCheckedEx.OwnerInfo = new DepOwnerInfo(this, "DisabledCheckedEx");
      }
    }
    private DepInput<Boolean> _DisabledCheckedEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckedEx
    /// </summary>
    private void DisabledCheckedEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledChecked = _DisabledCheckedEx.Value;
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка промежуточного состояния при <see cref="CanBeEmptyMode"/>=Warning
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Ok)
      {
        if (CanBeEmptyMode == UIValidateState.Warning && Control.CheckState == CheckState.Indeterminate)
          SetWarning(Res.EFPCheckBox_Err_IndeterminateState);
      }
    }

    #endregion

    #region Проверка группы переключателей

    private class GroupValidator
    {
      #region Поля

      public EFPCheckBox[] ControlProviders;

      #endregion

      #region Методы

      public void ContrtolProvider_Validating(object sender, UIValidatingEventArgs args)
      {
        bool hasChecked = false;
        for (int i = 0; i < ControlProviders.Length; i++)
        {
          if (ControlProviders[i].Checked)
          {
            hasChecked = true;
            break;
          }
        }

        if (!hasChecked)
          args.SetError(Res.EFPCheckBox_Err_GroupNoneChecked);
      }

      public void CheckedChanged(object sender, EventArgs args)
      {
        for (int i = 0; i < ControlProviders.Length; i++)
          ControlProviders[i].Validate();
      }

      #endregion
    }

    /// <summary>
    /// Добавляет проверку, что хотя бы один из переключателей включен.
    /// Все переключатели должны быть на 2 положения (<see cref="CheckBox.ThreeState"/>=false).
    /// Свойства <see cref="Control.Visible"/> и <see cref="Control.Enabled"/> не учитываются.
    /// </summary>
    /// <param name="controlProviders">Список переключателей</param>
    public static void AddGroupAtLeastOneCheck(params EFPCheckBox[] controlProviders)
    {
      if (controlProviders.Length < 1)
        throw ExceptionFactory.ArgIsEmpty("controlProviders");
      GroupValidator gv = new GroupValidator();
      gv.ControlProviders = controlProviders;
      for (int i = 0; i < controlProviders.Length; i++)
      {
        if (controlProviders[i] == null)
          throw ExceptionFactory.ArgInvalidEnumerableItem("controlProviders", controlProviders, controlProviders[i]);
        if (controlProviders[i].CanBeEmpty)
          throw new ArgumentException(String.Format(Res.EFPCheckBox_Arg_ThreeStateInGroup, i), "controlProviders");
        controlProviders[i].Validating += new UIValidatingEventHandler(gv.ContrtolProvider_Validating);
        controlProviders[i].CheckedEx.ValueChanged += new EventHandler(gv.CheckedChanged);
      }
    }

    #endregion
  }
}
