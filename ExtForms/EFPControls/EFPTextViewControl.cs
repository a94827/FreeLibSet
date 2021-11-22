// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;

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
