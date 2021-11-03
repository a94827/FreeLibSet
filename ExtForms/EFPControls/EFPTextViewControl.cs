using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для EFPLabel, EFPButton, EFPGroupBox и других классов,
  /// которые предназначены для отображения текста без возможности его редактирования
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public class EFPTextViewControl<T> : EFPControl<T>
    where T : Control
  {
    #region Конструктор

    /// <summary>
    /// Конструктор базового класса
    /// </summary>
    /// <param name="baseProvider"></param>
    /// <param name="control"></param>
    /// <param name="labelNeeded"></param>
    public EFPTextViewControl(EFPBaseProvider baseProvider, T control, bool labelNeeded)
      : base(baseProvider, control, labelNeeded)
    {
    }

    #endregion

    #region Свойство Text

    /// <summary>
    /// Свойство Control.Text
    /// </summary>
    public string Text
    {
      get { return Control.Text; }
      set
      {
        Control.Text = value;
        if (_TextEx != null)
          _TextEx.Value = value;
      }
    }

    /// <summary>
    /// Управляемое свойство Control.Text
    /// </summary>
    public DepValue<string> TextEx
    {
      get
      {
        InitTextEx();
        return _TextEx;
      }
      set
      {
        InitTextEx();
        _TextEx.Source = value;
      }
    }

    private void InitTextEx()
    {
      if (_TextEx == null)
      {
        _TextEx = new DepInput<string>(Text,TextEx_ValueChanged);
        _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
      }
    }

    private DepInput<string> _TextEx;

    private void TextEx_ValueChanged(object sender, EventArgs args)
    {
      Control.Text = _TextEx.Value;
    }

    #endregion
  }

  // ****************************************************************************
  // Реализации для конкретных управляющих элементов

  /// <summary>
  /// Обработчик для Label.
  /// Основное назначение - реализация управляемого свойства TextEx.
  /// </summary>
  public class EFPLabel : EFPTextViewControl<Label>
  {
    #region Конструктор

    /// <summary>
    /// Конструктор провайдера
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPLabel(EFPBaseProvider baseProvider, Label control)
      : base(baseProvider, control, false)
    {
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для InfoLabel.
  /// Позволяет управлять свойством Text.
  /// </summary>
  public class EFPInfoLabel : EFPTextViewControl<InfoLabel>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPInfoLabel(EFPBaseProvider baseProvider, InfoLabel control)
      : base(baseProvider, control, false)
    {
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для GroupBox.
  /// Позволяет управлять заголовком группы с помощью свойства Text и TextEx.
  /// </summary>
  public class EFPGroupBox : EFPTextViewControl<GroupBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPGroupBox(EFPBaseProvider baseProvider, GroupBox control)
      : base(baseProvider, control, false)
    {
    }

    #endregion
  }
}
