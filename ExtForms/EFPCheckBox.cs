using AgeyevAV.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Обработчик для CheckBox
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
      _SavedCheckState = control.CheckState;
      _AllowDisabledCheckState = false;
      _DisabledCheckState = CheckState.Unchecked;

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
      if (AllowDisabledCheckState && EnabledState)
        _HasSavedCheckState = true;
      InitControlCheckState();
    }


    #endregion

    #region Свойство ThreeState

    /// <summary>
    /// Доступ к CheckBox.ThreeState
    /// </summary>
    public bool ThreeState
    {
      get { return Control.ThreeState; }
      set { Control.ThreeState = value; }
    }

    /// <summary>
    /// Управляемое свойство ThreeState, разрешающее применение состояния CheckState.Grayed
    /// По умолчанию - false (кнопка на два положения)
    /// </summary>
    public DepValue<Boolean> ThreeStateEx
    {
      get
      {
        InitThreeStateEx();
        return _ThreeStateEx;
      }
      set
      {
        InitThreeStateEx();
        _ThreeStateEx.Source = value;
      }
    }

    private void InitThreeStateEx()
    {
      if (_ThreeStateEx == null)
      {
        _ThreeStateEx = new DepInput<Boolean>();
        _ThreeStateEx.OwnerInfo = new DepOwnerInfo(this, "ThreeStateEx");
        _ThreeStateEx.Value = ThreeState;
        _ThreeStateEx.ValueChanged += new EventHandler(ThreeStateEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _ThreeStateEx;

    /// <summary>
    /// Вызывается, когда изменяется значение "снаружи" элемента
    /// </summary>
    private void ThreeStateEx_ValueChanged(object sender, EventArgs args)
    {
      ThreeState = _ThreeStateEx.Value;
    }

    #endregion

    #region Свойство CheckState

    /// <summary>
    /// Возвращает актуальное состояние элемента CheckBox.CheckState.
    /// При установке свойства Control.CheckState не устанавливается, если Enabled=false
    /// и AllowDisabledState=true. В этом случае значение запоминается и будет 
    /// использовано при переходе в разрешенное состояние.
    /// </summary>
    public CheckState CheckState
    {
      get { return Control.CheckState; }
      set
      {
        _HasSavedCheckState = true;
        _SavedCheckState = value;
        InitControlCheckState();
      }
    }

    private CheckState _SavedCheckState;
    private bool _HasSavedCheckState;

    private void InitControlCheckState()
    {
      // Не нужно, иначе может не обновляться
      // if (InsideCheckStateChanged)
      //   return;
      if (AllowDisabledCheckState && (!EnabledState))
        Control.CheckState = DisabledCheckState;
      else if (_HasSavedCheckState)
      {
        _HasSavedCheckState = false;
        Control.CheckState = _SavedCheckState;
      }
    }

    /// <summary>
    /// Управляемое свойство CheckState
    /// </summary>
    public DepValue<CheckState> CheckStateEx
    {
      get
      {
        InitCheckStateEx();
        return _CheckStateEx;
      }
      set
      {
        InitCheckStateEx();
        _CheckStateEx.Source = value;
      }
    }

    private void InitCheckStateEx()
    {
      if (_CheckStateEx == null)
      {
        _CheckStateEx = new DepInput<CheckState>();
        _CheckStateEx.OwnerInfo = new DepOwnerInfo(this, "CheckStateEx");
        _CheckStateEx.Value = CheckState;
        _CheckStateEx.ValueChanged += new EventHandler(CheckStateEx_ValueChanged);
      }
    }

    private DepInput<CheckState> _CheckStateEx;

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
        EFPApp.ShowException(e, "Ошибка обработчика CheckBox.CheckStateChanged");
      }
    }

    private bool _InsideCheckStateChanged;

    /// <summary>
    /// Метод вызывается при изменении состояния флажка в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnCheckStateChanged()
    {
      if (_CheckStateEx != null)
        _CheckStateEx.Value = CheckState;
      if (_CheckedEx != null)
        _CheckedEx.Value = Checked;

      if (AllowDisabledCheckState && EnabledState)
        _SavedCheckState = Control.CheckState;

      Validate();
      //DoSyncValueChanged();
    }

    void CheckStateEx_ValueChanged(object sender, EventArgs args)
    {
      CheckState = _CheckStateEx.Value;
    }

    #endregion

    #region Свойство Checked

    /// <summary>
    /// Доступ к CheckedEx.ValueEx без принудительного создания объекта
    /// </summary>
    public bool Checked
    {
      get { return Control.Checked; }
      set
      {
        CheckState = value ? CheckState.Checked : CheckState.Unchecked;
      }
    }

    /// <summary>
    /// Свойство CheckedEx
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
        _CheckedEx = new DepInput<bool>();
        _CheckedEx.OwnerInfo = new DepOwnerInfo(this, "CheckedEx");
        _CheckedEx.Value = Checked;
        _CheckedEx.ValueChanged += new EventHandler(CheckedEx_ValueChanged);
      }
    }

    private DepInput<bool> _CheckedEx;

    void CheckedEx_ValueChanged(object sender, EventArgs args)
    {
      Checked = _CheckedEx.Value;
    }

    #endregion

    #region Свойство DisabledCheckState

    /// <summary>
    /// Доступ к DisabledCheckStateEx.ValueEx без принудительного создания объекта
    /// </summary>
    public CheckState DisabledCheckState
    {
      get { return _DisabledCheckState; }
      set
      {
        if (value == _DisabledCheckState)
          return;
        _DisabledCheckState = value;
        if (_DisabledCheckStateEx != null)
          _DisabledCheckStateEx.Value = value;
        if (_DisabledCheckedEx != null)
          _DisabledCheckedEx.Value = DisabledChecked;
        InitControlCheckState();
      }
    }
    private CheckState _DisabledCheckState;

    /// <summary>
    /// Этот текст замещает свойство CheckState, когда Enabled=false 
    /// Свойство действует при установленном свойстве AllowDisabledCheckState
    /// </summary>
    public DepValue<CheckState> DisabledCheckStateEx
    {
      get
      {
        InitDisabledCheckStateEx();
        return _DisabledCheckStateEx;
      }
      set
      {
        InitDisabledCheckStateEx();
        _DisabledCheckStateEx.Source = value;
      }
    }

    private void InitDisabledCheckStateEx()
    {
      if (_DisabledCheckStateEx == null)
      {
        _DisabledCheckStateEx = new DepInput<CheckState>();
        _DisabledCheckStateEx.OwnerInfo = new DepOwnerInfo(this, "DisabledCheckStateEx");
        _DisabledCheckStateEx.Value = DisabledCheckState;
        _DisabledCheckStateEx.ValueChanged += new EventHandler(DisabledCheckStateEx_ValueChanged);
      }
    }
    private DepInput<CheckState> _DisabledCheckStateEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckStateEx
    /// </summary>
    private void DisabledCheckStateEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledCheckState = _DisabledCheckStateEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства DisabledCheckStateEx
    /// </summary>
    public bool AllowDisabledCheckState
    {
      get { return _AllowDisabledCheckState; }
      set
      {
        if (value == _AllowDisabledCheckState)
          return;
        _AllowDisabledCheckState = value;
        InitControlCheckState();
      }
    }
    private bool _AllowDisabledCheckState;

    #endregion

    #region Свойство DisabledChecked

    /// <summary>
    /// Обращение к DisabledCheckedEx.ValueEx без принудительного создания объекта
    /// По умолчанию - false
    /// </summary>
    public bool DisabledChecked
    {
      get { return DisabledCheckState != CheckState.Unchecked; }
      set { DisabledCheckState = value ? CheckState.Checked : CheckState.Unchecked; }
    }

    /// <summary>
    /// Этот текст замещает свойство CheckedEx, когда EnabledEx=false 
    /// Свойство действует при установленном свойстве AllowDisabledChecked
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
        _DisabledCheckedEx = new DepInput<Boolean>();
        _DisabledCheckedEx.OwnerInfo = new DepOwnerInfo(this, "DisabledCheckedEx");
        _DisabledCheckedEx.Value = DisabledChecked;
        _DisabledCheckedEx.ValueChanged += new EventHandler(DisabledCheckedEx_ValueChanged);
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

    /// <summary>
    /// Разрешает использование свойства DisabledChecked.
    /// Это свойство совпадает с AllowDisabledCheckState.
    /// </summary>
    public bool AllowDisabledChecked
    {
      get { return AllowDisabledCheckState; }
      set { AllowDisabledCheckState = value; }
    }

    #endregion

    #region Проверка группы переключателей

    private class GroupValidator
    {
      #region Поля

      public EFPCheckBox[] ControlProviders;

      #endregion

      #region Методы

      public void ContrtolProvider_Validating(object sender, EFPValidatingEventArgs args)
      {
        bool HasChecked = false;
        for (int i = 0; i < ControlProviders.Length; i++)
        {
          if (ControlProviders[i].Checked)
          {
            HasChecked = true;
            break;
          }
        }

        if (!HasChecked)
          args.SetError("Должен быть установлен хотя бы один флажок");
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
    /// Все переключатели должны быть на 2 положения (CheckBox.ThreeState=false).
    /// Свойства CheckBox.Visible и Enabled не учитываются
    /// </summary>
    /// <param name="controlProviders">Список переключателей</param>
    public static void AddGroupAtLeastOneCheck(params EFPCheckBox[] controlProviders)
    {
      if (controlProviders.Length < 1)
        throw new ArgumentException("Список пустой", "controlProviders");
      GroupValidator gv = new GroupValidator();
      gv.ControlProviders = controlProviders;
      for (int i = 0; i < controlProviders.Length; i++)
      {
        if (controlProviders[i] == null)
          throw new ArgumentNullException("controlProviders[" + i.ToString() + "]");
        if (controlProviders[i].ThreeState)
          throw new ArgumentException("ThreeState=true", "controlProviders[" + i.ToString() + "]");
        controlProviders[i].Validating += new EFPValidatingEventHandler(gv.ContrtolProvider_Validating);
        controlProviders[i].CheckedEx.ValueChanged += new EventHandler(gv.CheckedChanged);
      }
    }

    #endregion
  }
}
