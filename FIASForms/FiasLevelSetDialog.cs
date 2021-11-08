using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.UICore;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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


namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// Диалог для выбора уровней адреса.
  /// Редактируемым значением является структура FiasLevelSet.
  /// </summary>
  public sealed class FiasLevelSetDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект с параметрами по умолчанию
    /// </summary>
    public FiasLevelSetDialog(FiasUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _Title = "Уровни адресных объектов";
      _AvailableLevels = FiasLevelSet.AllLevels;
      _Value = FiasLevelSet.Empty;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Пользовательский интерфейс.
    /// Задается в конструкторе.
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// Заголовок формы
    /// </summary>
    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// Уровени, из которых можно выбирать
    /// По умолчанию - FiasLevelSet.AllLevels
    /// </summary>
    public FiasLevelSet AvailableLevels
    {
      get { return _AvailableLevels; }
      set { _AvailableLevels = value; }
    }
    private FiasLevelSet _AvailableLevels;

    /// <summary>
    /// Может ли набор быть пустым?
    /// По умолчанию - false - должен быть выбран хотя бы один уровень.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set { _CanBeEmpty = value; }
    }
    private bool _CanBeEmpty;


    /// <summary>
    /// Основное свойство - редактируемый набор уровней.
    /// По умолчанию - FiasLevelSet.Empty - не выбрано ни одного уровня
    /// </summary>
    public FiasLevelSet Value
    {
      get { return _Value; }
      set
      {
        _Value = value;
      }
    }
    private FiasLevelSet _Value;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно EFPApp.DefaultScreen.
    /// </summary>
    public EFPDialogPosition DialogPosition 
    { 
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Выводит диалог на экран
    /// </summary>
    /// <returns>Результат работы диалога</returns>
    public DialogResult ShowDialog()
    {
      FiasLevel[] AvailableLevels2 = AvailableLevels.ToArray();
      string[] items = new string[AvailableLevels2.Length];
      for (int i = 0; i < AvailableLevels2.Length; i++)
        items[i] = FiasEnumNames.ToString(AvailableLevels2[i], true);


      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = Title;
      dlg.ListTitle = "Уровни";
      dlg.Items = items;
      dlg.MultiSelect = true;
      dlg.CanBeEmpty = this.CanBeEmpty;
      for (int i = 0; i < AvailableLevels2.Length; i++)
        dlg.Selections[i] = Value[AvailableLevels2[i]];
      this.DialogPosition.CopyTo(dlg.DialogPosition);

      DialogResult res = dlg.ShowDialog();

      if (res == DialogResult.OK)
      {
        _Value = FiasLevelSet.Empty;
        for (int i = 0; i < AvailableLevels2.Length; i++)
        {
          if (dlg.Selections[i])
            _Value |= AvailableLevels2[i];
        }
      }

      return res;
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок для выбора уровней адреса.
  /// Редактируемым значением является структура FiasLevelSet.
  /// </summary>
  public class EFPFiasLevelSetComboBox : EFPUserSelComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер комбоблока
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Ссылка на объект FiasUI</param>
    public EFPFiasLevelSetComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, FiasUI ui)
      : base(baseProvider, control)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentNullException("ui");
#endif
      _UI = ui;

      _AvailableLevels = FiasLevelSet.AllLevels;
      _Value = FiasLevelSet.Empty;

      control.Text = _Value.ToString();
      control.PopupClick += new EventHandler(Control_PopupClick);
      control.ClearClick += new EventHandler(Control_ClearClick);

      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Пользовательский интерфейс.
    /// Задается в конструкторе.
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// Уровени, из которых можно выбирать
    /// По умолчанию - FiasLevelSet.AllLevels
    /// </summary>
    public FiasLevelSet AvailableLevels
    {
      get { return _AvailableLevels; }
      set
      {
        if (value == _AvailableLevels)
          return;
        _AvailableLevels = value;
        Validate();
      }
    }
    private FiasLevelSet _AvailableLevels;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Может ли набор быть пустым?
    /// По умолчанию - Error- должен быть выбран хотя бы один уровень.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Control.ClearButton = _CanBeEmptyMode !=UIValidateState.Error;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли набор быть пустым?
    /// По умолчанию - false - должен быть выбран хотя бы один уровень.
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Текущее значение

    /// <summary>
    /// Основное свойство - редактируемый набор уровней.
    /// По умолчанию - FiasLevelSet.Empty - не выбрано ни одного уровня
    /// </summary>
    public FiasLevelSet Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        Control.Text = _Value.ToString();
        Validate();
      }
    }
    private FiasLevelSet _Value;

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка значения
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return; // формальность

      FiasLevelSet extra = _Value - _AvailableLevels;
      if (!extra.IsEmpty)
      {
        SetError("Выбраны недопустимые уровни: " + extra.ToString());
        return;
      }

      if (_Value.IsEmpty)
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("Уровни не выбраны");
            break;
          case UIValidateState.Warning:
            SetWarning("Уровни не выбраны");
            break;
        }
      }
    }

    #endregion

    #region Выбор из списка

    void Control_PopupClick(object sender, EventArgs args)
    {
      FiasLevelSetDialog dlg = new FiasLevelSetDialog(UI);
      dlg.Title = DisplayName;
      dlg.AvailableLevels = AvailableLevels;
      dlg.Value = Value;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.DialogPosition.PopupOwnerControl = Control;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      Value = dlg.Value;
    }

    void Control_ClearClick(object sender, EventArgs args)
    {
      Value = FiasLevelSet.Empty;
    }

    #endregion
  }
}
