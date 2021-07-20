using AgeyevAV.Config;
using AgeyevAV.DependedValues;
using AgeyevAV.IO;
using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace AgeyevAV.RI
{
  /// <summary>
  /// Значения для свойств CanBeEmptyMode различных управляющих элементов
  /// </summary>
  [Serializable]
  public enum CanBeEmptyMode
  {
    /// <summary>
    /// Значение должно быть введено обязательно, иначе выдается ошибка
    /// </summary>
    Error,

    /// <summary>
    /// Значение может быть пустым, но будет выдано предупреждение
    /// </summary>
    Warning,

    /// <summary>
    /// Значение может быть пустым
    /// </summary>
    Ok
  }

  /// <summary>
  /// Базовый класс для управляющих элементов, которые могут располагаться на полосе RIBand
  /// </summary>
  [Serializable]
  public abstract class Control : RIItem
  {
    #region Конструктор

    /// <summary>
    /// Конструктор базового класса
    /// </summary>
    public Control()
    {
    }

    #endregion

    #region Свойство Enabled

    /// <summary>
    /// Доступность элемента для ввода пользователем.
    /// По умолчанию - true.
    /// Нет смысла устанавливать в false это свойство, так как элемент становится бесполезным.
    /// Вместо этого можно использовать свойство EnabledEx для организации условных блокировок элементов.
    /// Свойство, однако, может опрашиваться в обработчике события Dialog.Validating, чтобы выполнять
    /// проверку корректности введенного эначения только когда элемент доступен для ввода.
    /// </summary>
    public bool Enabled
    {
      // Нет смысла делать свойство Enabled как отдельное поле, т.к. оно нужно редко

      get
      {
        if (_EnabledEx == null)
          return true; // обычный вариант
        else
          return _EnabledEx.Value;
      }
      set
      {
        if (value && (_EnabledEx == null))
          return;

        EnabledEx.Value = value;
      }
    }

    /// <summary>
    /// Управляемое значение для Enabled.
    /// Используется для организации условной блокировки элементов.
    /// Например, элемент может быть доступен, только если включен флажок CheckBox.
    /// </summary>
    public DepValue<bool> EnabledEx
    {
      get
      {
        InitEnabledEx();
        return _EnabledEx;
      }
      set
      {
        InitEnabledEx();
        _EnabledEx.Source = value;
      }
    }
    private DepInput<bool> _EnabledEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства EnabledEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasEnabledExProperty { get { return _EnabledEx != null; } }


    private void InitEnabledEx()
    {
      if (_EnabledEx == null)
      {
        _EnabledEx = new DepInput<bool>();
        _EnabledEx.OwnerInfo = new DepOwnerInfo(this, "EnabledEx");
        _EnabledEx.Value = true;
      }
    }

    #endregion
  }

  /// <summary>
  /// Метка для другого управляющего элемента.
  /// Свойство "Text" является неизменяемым
  /// Не содержит значений, которые можно было бы передавать
  /// </summary>
  [Serializable]
  public class Label : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает метку.
    /// </summary>
    /// <param name="text">Текст метки. Может содержать символ "амперсанд" для выделения "горячей клавиши".
    /// Не может быть пустой строкой</param>
    public Label(string text)
    {
      if (String.IsNullOrEmpty(text))
        throw new ArgumentNullException("text");
      _Text = text;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текст метки. Может содержать символ "амперсанд" для выделения "горячей клавиши"
    /// </summary>
    public string Text { get { return _Text; } }
    private string _Text;

    #endregion
  }


  /// <summary>
  /// Поле ввода однострочного текста
  /// </summary>
  [Serializable]
  public class TextBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле ввода
    /// </summary>
    public TextBox()
    {
      _MaxLength = Int16.MaxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Введенный текст
    /// </summary>
    public string Text
    {
      get
      {
        if (_Text == null)
          return String.Empty;
        else
          return _Text;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          _Text = null;
        else
          _Text = value;
        if (_TextEx != null)
          _TextEx.Value = _Text;
      }
    }
    private string _Text; // храним null вместо пустой строки

    private string _OldText;

    /// <summary>
    /// Управляемое значение для Text.
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
    private DepInput<string> _TextEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства TextEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasTextExProperty { get { return _TextEx != null; } }

    private void InitTextEx()
    {
      if (_TextEx == null)
      {
        _TextEx = new DepInput<string>();
        _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        _TextEx.Value = Text;
        _TextEx.ValueChanged += new EventHandler(TextEx_ValueChanged);
      }
    }

    private void TextEx_ValueChanged(object sender, EventArgs args)
    {
      Text = _TextEx.Value;
    }

    /// <summary>
    /// Максимальная длина текста. По умолчанию: 32767 (Int16.MaxValue) символов
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// </summary>
    public int MaxLength
    {
      get { return _MaxLength; }
      set
      {
        CheckNotFixed();
        _MaxLength = value;
      }
    }
    private int _MaxLength;


    /// <summary>
    /// Режим работы текстового поля для просмотра без возможности редактирования.
    /// По умолчанию - false - пользователь может редактировать текст
    /// В отличие от установки свойства Enabled=false, при RedaOnly=true пользователь может выделять текст и копировать его в буфер обмена.
    /// Контроль введенного значения не выполняется при ReadOnly=true.
    /// </summary>
    public bool ReadOnly
    {
      // Нет смысла делать свойство ReadOnly как отдельное поле, т.к. оно нужно редко
      get
      {
        if (_ReadOnlyEx == null)
          return false; // обычный вариант
        else
          return _ReadOnlyEx.Value;
      }
      set
      {
        if ((!value) && (_ReadOnlyEx == null))
          return;

        ReadOnlyEx.Value = value;
      }
    }

    /// <summary>
    /// Управляемое значение для ReadOnly.
    /// Используется для организации условной блокировки элементов.
    /// Например, элемент может быть заблокирован, если выключен флажок CheckBox.
    /// </summary>
    public DepValue<bool> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyEx.Source = value;
      }
    }
    private DepInput<bool> _ReadOnlyEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ReadOnlyEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasReadOnlyExProperty { get { return _ReadOnlyEx != null; } }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<bool>();
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
        _ReadOnlyEx.Value = false;
      }
    }

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }


    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается сообщение об ошибке, определяемое свойством ErrorRegExMessage.
    /// </summary>
    public string ErrorRegExPattern
    {
      get { return _ErrorRegExPattern; }
      set
      {
        CheckNotFixed();
        _ErrorRegExPattern = value;
      }
    }
    private string _ErrorRegExPattern;

    /// <summary>
    /// Текст сообщения об ошибке, которое выводится, если введенное значение не соответствует регулярному выражению ErrorRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string ErrorRegExMessage
    {
      get { return _ErrorRegExMessage; }
      set
      {
        CheckNotFixed();
        _ErrorRegExMessage = value;
      }
    }
    private string _ErrorRegExMessage;

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается предупреждение, определяемое свойством WarningRegExMessage.
    /// Проверка не выполняется, если обнаружена ошибка при проверке значения с помощью свойства ErrorRegEx.
    /// </summary>
    public string WarningRegExPattern
    {
      get { return _WarningRegExPattern; }
      set
      {
        CheckNotFixed();
        _WarningRegExPattern = value;
      }
    }
    private string _WarningRegExPattern;

    /// <summary>
    /// Текст предупреждения, которое выводится, если введенное значение не соответствует регулярному выражению WarningRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string WarningRegExMessage
    {
      get { return _WarningRegExMessage; }
      set
      {
        CheckNotFixed();
        _WarningRegExMessage = value;
      }
    }
    private string _WarningRegExMessage;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return Text != _OldText;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetString("Text", Text);
      _OldText = Text;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Text = part.GetString("Text");
      _OldText = Text;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, Text);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        Text = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода целого числа
  /// </summary>
  [Serializable]
  public class IntEditBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле ввода
    /// </summary>
    public IntEditBox()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текушее значение
    /// </summary>
    public int Value
    {
      get
      {
        return _Value;
      }
      set
      {
        _Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = value;
      }
    }
    private int _Value;

    private int _OldValue;

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<int> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }
    private DepInput<int> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasValueExProperty { get { return _ValueEx != null; } }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<int>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public int? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private int? _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public int? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private int? _Maximum;

    /// <summary>
    /// Если свойство установлено в true, то значение можно выбирать с помощью стрелочек.
    /// По умолчанию - false - стрелочки не используются.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public bool ShowUpDown
    {
      get { return _ShowUpDown; }
      set
      {
        CheckNotFixed();
        _ShowUpDown = value;
      }
    }
    private bool _ShowUpDown;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _Value != _OldValue;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetInt("Value", _Value);
      _OldValue = _Value;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _Value = part.GetInt("Value");
      _OldValue = Value;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetInt(Name, Value);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      Value = part.GetIntDef(Name, Value);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числа одинарной точности
  /// </summary>
  [Serializable]
  public class SingleEditBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле ввода
    /// </summary>
    public SingleEditBox()
    {
      _DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текушее значение
    /// </summary>
    public float Value
    {
      get
      {
        return _Value;
      }
      set
      {
        _Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = value;
      }
    }
    private float _Value;

    private float _OldValue;


    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<float> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }
    private DepInput<float> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasValueExProperty { get { return _ValueEx != null; } }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<float>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }
    /// <summary>
    /// Количество знаков после запятой. По умолчанию: (-1) - разрешается использование любого числа знаков
    /// Если поле предназначено для ввода целых чисел, следует установить равным 0.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public float? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private float? _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public float? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private float? _Maximum;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _Value != _OldValue;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetSingle("Value", _Value);
      _OldValue = _Value;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _Value = part.GetSingle("Value");
      _OldValue = Value;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetSingle(Name, Value);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      Value = part.GetSingleDef(Name, Value);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числа двойной точности
  /// </summary>
  [Serializable]
  public class DoubleEditBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле ввода
    /// </summary>
    public DoubleEditBox()
    {
      _DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текушее значение
    /// </summary>
    public double Value
    {
      get
      {
        return _Value;
      }
      set
      {
        _Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = value;
      }
    }
    private double _Value;

    private double _OldValue;

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<double> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }
    private DepInput<Double> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasValueExProperty { get { return _ValueEx != null; } }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<double>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    /// <summary>
    /// Количество знаков после запятой. По умолчанию: (-1) - разрешается использование любого числа знаков
    /// Если поле предназначено для ввода целых чисел, следует установить равным 0.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public double? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private double? _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public double? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private double? _Maximum;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _Value != _OldValue;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetDouble("Value", _Value);
      _OldValue = _Value;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _Value = part.GetDouble("Value");
      _OldValue = Value;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetDouble(Name, Value);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      Value = part.GetDoubleDef(Name, Value);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числа типа decimal
  /// </summary>
  [Serializable]
  public class DecimalEditBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле ввода
    /// </summary>
    public DecimalEditBox()
    {
      _DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текушее значение
    /// </summary>
    public decimal Value
    {
      get
      {
        return _Value;
      }
      set
      {
        _Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = value;
      }
    }
    private decimal _Value; // данные всегда хранятся в формате Decimal

    private decimal _OldValue;

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<decimal> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }
    private DepInput<decimal> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasValueExProperty { get { return _ValueEx != null; } }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<decimal>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    /// <summary>
    /// Количество знаков после запятой. По умолчанию: (-1) - разрешается использование любого числа знаков
    /// Если поле предназначено для ввода целых чисел, следует установить равным 0.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public int DecimalPlaces { get { return _DecimalPlaces; } set { _DecimalPlaces = value; } }
    private int _DecimalPlaces;

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public decimal? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private decimal? _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public decimal? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private decimal? _Maximum;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _Value != _OldValue;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetDecimal("Value", _Value);
      _OldValue = _Value;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _Value = part.GetDecimal("Value");
      _OldValue = Value;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetDecimal(Name, Value);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      Value = part.GetDecimalDef(Name, Value);
    }

    #endregion
  }

  /// <summary>
  /// Переключатель CheckBox.
  /// Поддерживаются переключатели на два и на три состояния (свойство ThreeState).
  /// </summary>
  [Serializable]
  public class CheckBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает переключатель
    /// </summary>
    /// <param name="text">Текст кнопки. Должен быть задан</param>
    public CheckBox(string text)
    {
      if (String.IsNullOrEmpty(text))
        throw new ArgumentNullException("text");
      _Text = text;
      _CheckState = CheckState.Unchecked;
      _OldCheckState = CheckState.Unchecked;
    }

    #endregion

    #region Свойства

    #region Text

    /// <summary>
    /// Текст переключателя.
    /// Может содержать амперсанд для подчеркивания буквы.
    /// Задается в конструкторе и не может быть изменено.
    /// </summary>
    public string Text { get { return _Text; } }
    private string _Text;

    #endregion

    #region ThreeState

    /// <summary>
    /// Разрешено ли использовать третье состояние
    /// По умолчанию - false - (два состояния).
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public bool ThreeState
    {
      get { return _ThreeState; }
      set
      {
        CheckNotFixed();
        _ThreeState = value;
      }
    }
    private bool _ThreeState;

    #endregion

    #region CheckState

    /// <summary>
    /// Если есть расширенное свойство CheckedEx и устанавливается значение CheckState=Indeterminate,
    /// то оно исчезнет и заменится на Checked. Чтобы этого не произошло, предотвращаем вложенный вызов
    /// </summary>
    [NonSerialized]
    private bool _InsideSetCheckState;

    /// <summary>
    /// Текущее состояние кнопки.
    /// Обычно свойство используется для кнопок, у которых установлено ThreeState=true.
    /// Для обычных кнопок на два положения удобнее использовать свойство Checked.
    /// </summary>
    public CheckState CheckState
    {
      get { return _CheckState; }
      set
      {
        if (_InsideSetCheckState)
          return;
        _InsideSetCheckState = true;
        try
        {
          _CheckState = value;
          if (_CheckStateEx != null)
            _CheckStateEx.Value = CheckState;
          if (_CheckedEx != null)
            _CheckedEx.Value = Checked;
        }
        finally
        {
          _InsideSetCheckState = false;
        }
      }
    }
    private CheckState _CheckState;

    private CheckState _OldCheckState;

    /// <summary>
    /// Управляемое значение для CheckState.
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
    private DepInput<CheckState> _CheckStateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства CheckStateEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasCheckStateExProperty { get { return _CheckStateEx != null; } }

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

    private void CheckStateEx_ValueChanged(object sender, EventArgs args)
    {
      CheckState = _CheckStateEx.Value;
    }

    #endregion

    #region Checked

    /// <summary>
    /// Состояние кнопки.
    /// Используется для обычных кнопок на два положения.
    /// Если ThreeState=true, используйте свойство CheckState.
    /// Если ThreeState=true, то свойство возвращает true для CheckState=Checked и Indeterminate и false для Unchecked.
    /// Установка свойства в true задает CheckState=Checked, а false - CheckState=Unchecked.
    /// </summary>
    public bool Checked
    {
      get
      {
        return CheckState != CheckState.Unchecked;
      }
      set
      {
        CheckState = value ? CheckState.Checked : CheckState.Unchecked;
      }
    }

    /// <summary>
    /// Управляемое значение для Checked.
    /// </summary>
    public DepValue<bool> CheckedEx
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
    private DepInput<bool> _CheckedEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства CheckedEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasCheckedExProperty { get { return _CheckedEx != null; } }

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

    private void CheckedEx_ValueChanged(object sender, EventArgs args)
    {
      Checked = _CheckedEx.Value;
    }

    #endregion

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return CheckState != _OldCheckState;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetInt("CheckState", (int)CheckState);
      _OldCheckState = CheckState;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      CheckState = (CheckState)part.GetInt("CheckState");
      _OldCheckState = CheckState;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetInt(Name, (int)CheckState); // 0,1 или 2
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (ThreeState)
        CheckState = (CheckState)part.GetIntDef(Name, (int)CheckState);
      else
        Checked = part.GetBoolDef(Name, Checked);
    }

    #endregion
  }

  /// <summary>
  /// Группа радиокнопок
  /// </summary>
  [Serializable]
  public class RadioGroup : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает группу кнопок.
    /// </summary>
    /// <param name="items">Список надписей для кнопок. Массив не может быть пустым</param>
    public RadioGroup(string[] items)
    {
      CheckNotEmptyStringArray(items, false);

      _Items = items;
    }

    #endregion

    #region Свойства

    #region Items

    /// <summary>
    /// Названия радиокнопок.
    /// Названия могут содержать амперсанд, чтобы выделить букву.
    /// Задаются в конструкторе и не могут быть изменены в дальнейшем
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;

    #endregion

    #region SelectedIndex

    /// <summary>
    /// Текущее выбранное значение (индекс в списке Items)
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set
      {
        _SelectedIndex = value;
        if (_SelectedIndexEx != null)
          _SelectedIndexEx.Value = SelectedIndex;
        if (_SelectedCodeEx != null)
          _SelectedCodeEx.Value = SelectedCode;
      }
    }
    private int _SelectedIndex;

    private int _OldSelectedIndex;

    /// <summary>
    /// Управляемое значение для SelectedIndex.
    /// </summary>
    public DepValue<int> SelectedIndexEx
    {
      get
      {
        InitSelectedIndexEx();
        return _SelectedIndexEx;
      }
      set
      {
        InitSelectedIndexEx();
        _SelectedIndexEx.Source = value;
      }
    }
    private DepInput<int> _SelectedIndexEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства SelectedIndexEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasSelectedIndexExProperty { get { return _SelectedIndexEx != null; } }

    private void InitSelectedIndexEx()
    {
      if (_SelectedIndexEx == null)
      {
        _SelectedIndexEx = new DepInput<int>();
        _SelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "SelectedIndexEx");
        _SelectedIndexEx.Value = SelectedIndex;
        _SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexEx_ValueChanged);
      }
    }

    private void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedIndex = _SelectedIndexEx.Value;
    }

    #endregion

    #region Codes

    /// <summary>
    /// Коды для элементов.
    /// Если свойство установлено, то для сохранения значения между сеансами работы используется код, а не индекс.
    /// По умолчанию свойство содержит значение null и свойство SelectedCode нельзя использовать.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// Длина массива должна совпадать с Items.
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
        CheckNotFixed();
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw new ArgumentException("Неправильная длина массива кодов");
          _Codes = value;
        }
      }
    }
    private string[] _Codes;

    #endregion

    #region SelectedCode

    /// <summary>
    /// Текущая выбранная позиция как код.
    /// Должно быть установлено свойство Codes, иначе свойство возвращает пустую строку.
    /// </summary>
    public string SelectedCode
    {
      get
      {
        if (Codes == null)
          return String.Empty;
        else
        {
          if (SelectedIndex < 0)
            return String.Empty; // нет свойства UnselectedCode
          else
            return Codes[SelectedIndex];
        }
      }
      set
      {
        if (Codes == null)
          throw new InvalidOperationException("Свойство Codes не установено");
        SelectedIndex = Array.IndexOf<string>(Codes, value);
      }
    }

    /// <summary>
    /// Управляемое значение для SelectedCode.
    /// </summary>
    public DepValue<string> SelectedCodeEx
    {
      get
      {
        InitSelectedCodeEx();
        return _SelectedCodeEx;
      }
      set
      {
        InitSelectedCodeEx();
        _SelectedCodeEx.Source = value;
      }
    }
    private DepInput<string> _SelectedCodeEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства SelectedCodeEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasSelectedCodeExProperty { get { return _SelectedCodeEx != null; } }

    private void InitSelectedCodeEx()
    {
      if (_SelectedCodeEx == null)
      {
        _SelectedCodeEx = new DepInput<string>();
        _SelectedCodeEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodeEx");
        _SelectedCodeEx.Value = SelectedCode;
        _SelectedCodeEx.ValueChanged += new EventHandler(FSelectedCodeEx_ValueChanged);
      }
    }

    private void FSelectedCodeEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCode = _SelectedCodeEx.Value;
    }

    #endregion

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return SelectedIndex != _OldSelectedIndex;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetInt("SelectedIndex", SelectedIndex);
      _OldSelectedIndex = SelectedIndex;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      SelectedIndex = part.GetInt("SelectedIndex");
      _OldSelectedIndex = SelectedIndex;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (Codes == null)
        part.SetInt(Name, SelectedIndex);
      else
        part.SetString(Name, SelectedCode);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
      {
        if (Codes == null)
          SelectedIndex = part.GetInt(Name);
        else
          SelectedCode = part.GetString(Name);
      }
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода даты
  /// </summary>
  [Serializable]
  public class DateBox : Control
  {
    #region Свойства

    /// <summary>
    /// Введенное значение
    /// </summary>
    public DateTime? Value
    {
      get { return _Value; }
      set
      {
        _Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = value;
      }
    }
    private DateTime? _Value;

    private DateTime? _OldValue;

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<DateTime?> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }
    private DepInput<DateTime?> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasValueExProperty { get { return _ValueEx != null; } }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<DateTime?>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство можно устанавливать только до вывода диалога на экран.
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private DateTime? _Maximum;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return Value != _OldValue;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetNullableDate("Value", Value);
      _OldValue = Value;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Value = part.GetNullableDate("Value");
      _OldValue = Value;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (Value.HasValue)
        part.SetDate(Name, Value.Value);
      else
        part.SetString(Name, String.Empty);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        Value = part.GetNullableDate(Name);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода интервала дат
  /// </summary>
  [Serializable]
  public class DateRangeBox : Control
  {
    #region Свойства

    #region Редактируемое значение

    /// <summary>
    /// Начальная дата диапазона
    /// </summary>
    public DateTime? FirstDate
    {
      get { return _FirstDate; }
      set { _FirstDate = value; }
    }
    private DateTime? _FirstDate;

    private DateTime? _OldFirstDate;

    /// <summary>
    /// Конечная дата диапазона
    /// </summary>
    public DateTime? LastDate
    {
      get { return _LastDate; }
      set { _LastDate = value; }
    }
    private DateTime? _LastDate;

    private DateTime? _OldLastDate;

    /// <summary>
    /// Вспомогательное свойство для доступа к интервалу дат.
    /// Если начальная и/или конечная дата не установлена, свойство возвращает DateRange.Empty.
    /// Установка значения свойства в DateRange.Empty очищает обе даты
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        if (FirstDate.HasValue && LastDate.HasValue)
          return new DateRange(FirstDate.Value, LastDate.Value);
        else
          return DateRange.Empty;
      }
      set
      {
        if (value.IsEmpty)
        {
          FirstDate = null;
          LastDate = null;
        }
        else
        {
          FirstDate = value.FirstDate;
          LastDate = value.LastDate;
        }
      }
    }

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Могут ли поля быть пустыми.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Могут ли поля быть пустыми.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }


    #endregion

    #region Диапазон значений

    #region Отдельно для начальной даты

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? MinimumFirstDate
    {
      get { return _MinimumFirstDate; }
      set
      {
        CheckNotFixed();
        _MinimumFirstDate = value;
      }
    }
    private DateTime? _MinimumFirstDate;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? MaximumFirstDate
    {
      get { return _MaximumFirstDate; }
      set
      {
        CheckNotFixed();
        _MaximumFirstDate = value;
      }
    }
    private DateTime? _MaximumFirstDate;

    #endregion

    #region Отдельно для конечной даты

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? MinimumLastDate
    {
      get { return _MinimumLastDate; }
      set
      {
        CheckNotFixed();
        _MinimumLastDate = value;
      }
    }
    private DateTime? _MinimumLastDate;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? MaximumLastDate
    {
      get { return _MaximumLastDate; }
      set
      {
        CheckNotFixed();
        _MaximumLastDate = value;
      }
    }
    private DateTime? _MaximumLastDate;

    #endregion

    #region Для диапазона в-целом

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Установка этого свойства задает ограничение и для начальной и для конечной даты диапазона (свойства MinimumFirstDate и MinumumLastDate).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? Minimum
    {
      get
      {
        if (MinimumFirstDate.HasValue && MinimumLastDate.HasValue && MinimumFirstDate == MinimumLastDate)
          return MinimumFirstDate;
        else
          return null;
      }
      set
      {
        MinimumFirstDate = value;
        MinimumLastDate = value;
      }
    }

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Установка этого свойства задает ограничение и для начально и для конечной даты диапазона (свойства MaximumFirstDate и MaximumLastDate).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? Maximum
    {
      get
      {
        if (MaximumFirstDate.HasValue && MaximumLastDate.HasValue && MaximumFirstDate == MaximumLastDate)
          return MinimumFirstDate;
        else
          return null;
      }
      set
      {
        MaximumFirstDate = value;
        MaximumLastDate = value;
      }
    }

    #endregion

    #endregion

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return FirstDate != _OldFirstDate || LastDate != _OldLastDate;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetNullableDate("FirstDate", FirstDate);
      part.SetNullableDate("LastDate", LastDate);
      _OldFirstDate = FirstDate;
      _OldLastDate = LastDate;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      FirstDate = part.GetNullableDate("FirstDate");
      LastDate = part.GetNullableDate("LastDate");
      _OldFirstDate = FirstDate;
      _OldLastDate = LastDate;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (FirstDate.HasValue)
        part.SetDate(Name + "-First", FirstDate.Value);
      else
        part.SetString(Name + "-First", String.Empty);

      if (LastDate.HasValue)
        part.SetDate(Name + "-Last", LastDate.Value);
      else
        part.SetString(Name + "-Last", String.Empty);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name + "-First"))
        FirstDate = part.GetNullableDate(Name + "-First");
      if (part.HasValue(Name + "-Last"))
        LastDate = part.GetNullableDate(Name + "-Last");
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода одиночной даты или интервала дат.
  /// Полуоткрытые интервалы недопускаются
  /// </summary>
  [Serializable]
  public class DateOrRangeBox : Control
  {
    #region Свойства

    #region Вводимое значение

    /// <summary>
    /// Интервал дат.
    /// Пустой интервал задается как DateRange.Empty
    /// </summary>
    public DateRange DateRange
    {
      get { return _DateRange; }
      set { _DateRange = value; }
    }
    private DateRange _DateRange;

    private DateRange _OldDateRange;

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Допускается ли пустой интервал дат.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Допускается ли пустой интервал дат.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }

    #endregion

    #region Диапазон значений

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private DateTime? _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public DateTime? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private DateTime? _Maximum;

    #endregion

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return DateRange != _OldDateRange;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      if (DateRange.IsEmpty)
      {
        part.Remove("FirstDate");
        part.Remove("LastDate");
      }
      else
      {
        part.SetNullableDate("FirstDate", DateRange.FirstDate);
        part.SetNullableDate("LastDate", DateRange.LastDate);
      }
      _OldDateRange = DateRange;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      DateTime? FirstDate = part.GetNullableDate("FirstDate");
      DateTime? LastDate = part.GetNullableDate("LastDate");
      if (FirstDate.HasValue && LastDate.HasValue)
        DateRange = new DateRange(FirstDate.Value, LastDate.Value);
      else
        DateRange = DateRange.Empty;
      _OldDateRange = DateRange;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (DateRange.IsEmpty)
      {
        part.SetString(Name + "-First", String.Empty);
        part.SetString(Name + "-Last", String.Empty);
      }
      else
      {
        part.SetDate(Name + "-First", DateRange.FirstDate);
        part.SetDate(Name + "-Last", DateRange.LastDate);
      }
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      DateTime? FirstDate = part.GetNullableDate(Name + "-First");
      DateTime? LastDate = part.GetNullableDate(Name + "-Last");

      if (FirstDate.HasValue && LastDate.HasValue)
        DateRange = new DateRange(FirstDate.Value, LastDate.Value);
      else
        DateRange = DateRange.Empty;
    }

    #endregion
  }

  /// <summary>
  /// Поле выбора месяца и года
  /// </summary>
  [Serializable]
  public class YearMonthBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле.
    /// </summary>
    public YearMonthBox()
    {
      _Year = DateTime.Today.Year;
      _Month = DateTime.Today.Month;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение - выбранный год
    /// </summary>
    public int Year
    {
      get { return _Year; }
      set { _Year = value; }
    }
    private int _Year;

    private int _OldYear;

    /// <summary>
    /// Текущее значение - выбранный месяц (1-12)
    /// </summary>
    public int Month
    {
      get { return _Month; }
      set { _Month = value; }
    }
    private int _Month;

    private int _OldMonth;

    /// <summary>
    /// Текущее значение - год и месяц всесте
    /// </summary>
    public YearMonth YM
    {
      get { return new YearMonth(Year, Month); }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException();
        Year = value.Year;
        Month = value.Month;
      }
    }

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено (YearMonth.Empty).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public YearMonth Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private YearMonth _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено (YearMonth.Empty).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public YearMonth Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private YearMonth _Maximum;


    /// <summary>
    /// Вспомогательное свойство для чтения/записи значений как интервала дат.
    /// При установке свойства интервал дат должен относиться к одному месяцу.
    /// </summary>
    /// <remarks>В основном, для совместимости с YearMonthRangeBox</remarks>
    public DateRange DateRange
    {
      get
      {
        return YM.DateRange;
      }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException("value", "Задан пустой интервал дат");
        if (value.FirstDate.Year != value.LastDate.Year || value.FirstDate.Month != value.LastDate.Month)
          throw new ArgumentException("Интервал дат должен относиться к одному месяцу");

        Year = value.FirstDate.Year;
        Month = value.FirstDate.Month;
      }
    }

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return Year != _OldYear || Month != _OldMonth;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetInt("Year", Year);
      part.SetInt("Month", Month);
      _OldYear = Year;
      _OldMonth = Month;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Year = part.GetInt("Year");
      Month = part.GetInt("Month");
      _OldYear = Year;
      _OldMonth = Month;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetInt(Name + "-Year", Year);
      part.SetInt(Name + "-Month", Month);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name + "-Year"))
      {
        Year = part.GetInt(Name + "-Year");
        Month = part.GetInt(Name + "-Month");
      }
    }

    #endregion
  }

  /// <summary>
  /// Поле выбора диапазона месяцев и года.
  /// Может задаваться только один год.
  /// При размещении на полосе элементов рекомендуется занимать целую полосу, задавая метку на предыдущей полосе
  /// </summary>
  [Serializable]
  public class YearMonthRangeBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает поле
    /// </summary>
    public YearMonthRangeBox()
    {
      Year = DateTime.Today.Year;
      FirstMonth = DateTime.Today.Month;
      LastMonth = FirstMonth;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выбранный год
    /// </summary>
    public int Year
    {
      get { return _Year; }
      set { _Year = value; }
    }
    private int _Year;

    private int _OldYear;

    /// <summary>
    /// Выбранный первый месяц (1 .. 12)
    /// </summary>
    public int FirstMonth
    {
      get { return _FirstMonth; }
      set { _FirstMonth = value; }
    }
    private int _FirstMonth;

    private int _OldFirstMonth;

    /// <summary>
    /// Выбранный последний месяц (1 .. 12)
    /// </summary>
    public int LastMonth
    {
      get { return _LastMonth; }
      set { _LastMonth = value; }
    }
    private int _LastMonth;

    private int _OldLastMonth;

    /// <summary>
    /// Первый месяц и год в виде структуры YearMonth
    /// </summary>
    public YearMonth FirstYM
    {
      get { return new YearMonth(Year, FirstMonth); }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException();
        Year = value.Year;
        FirstMonth = value.Month;
      }
    }

    /// <summary>
    /// Последний месяц и год в виде структуры YearMonth
    /// </summary>
    public YearMonth LastYM
    {
      get { return new YearMonth(Year, LastMonth); }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException();
        Year = value.Year;
        LastMonth = value.Month;
      }
    }

    /// <summary>
    /// Вспомогательное свойство для чтения/записи значений как интервала дат.
    /// При установке свойства интервал дат должен относиться к одному году
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        return new DateRange(FirstYM.DateRange.FirstDate, LastYM.DateRange.LastDate);
      }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException("value", "Задан пустой интервал дат");
        if (value.FirstDate.Year != value.LastDate.Year)
          throw new ArgumentException("Интервал дат должен относиться к одному году");

        Year = value.FirstDate.Year;
        FirstMonth = value.FirstDate.Month;
        LastMonth = value.LastDate.Month;
      }
    }

    /// <summary>
    /// Свойства FirstYM и LastYM вместе
    /// </summary>
    public YearMonthRange YMRange
    {
      get
      {
        return new YearMonthRange(Year, FirstMonth, LastMonth);
      }
      set
      {
        if (value.IsEmpty)
          throw new ArgumentNullException();
        if (value.LastYM.Year != value.FirstYM.Year)
          throw new ArgumentException("Период должен относиться к одному году");
        this.Year = value.FirstYM.Year;
        this.FirstMonth = value.FirstYM.Month;
        this.LastMonth = value.LastYM.Month;
      }
    }

    /// <summary>
    /// Минимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено (YearMonth.Empty).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public YearMonth Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private YearMonth _Minimum;

    /// <summary>
    /// Максимальное значение, которое можно ввести.
    /// По умолчанию ограничение не установлено (YearMonth.Empty).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public YearMonth Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private YearMonth _Maximum;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _Year != _OldYear || _FirstMonth != _OldFirstMonth || _LastMonth != _OldLastMonth;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetInt("Year", Year);
      part.SetInt("FirstMonth", FirstMonth);
      part.SetInt("LastMonth", LastMonth);
      _OldYear = _Year;
      _OldFirstMonth = FirstMonth;
      _OldLastMonth = LastMonth;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Year = part.GetInt("Year");
      FirstMonth = part.GetInt("FirstMonth");
      LastMonth = part.GetInt("LastMonth");
      _OldYear = Year;
      _OldFirstMonth = FirstMonth;
      _OldLastMonth = LastMonth;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetInt(Name + "-Year", Year);
      part.SetInt(Name + "-FirstMonth", FirstMonth);
      part.SetInt(Name + "-LastMonth", LastMonth);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name + "-Year"))
      {
        Year = part.GetInt(Name + "-Year");
        FirstMonth = part.GetInt(Name + "-FirstMonth");
        LastMonth = part.GetInt(Name + "-LastMonth");
      }
    }

    #endregion
  }


  /// <summary>
  /// Комбоблок выбора значения из выпадающего списка.
  /// Текущее значение определяется свойством SelectedIndex.
  /// Список для выбора является фиксированным и задается в конструкторе.
  /// Вариант "отсутствие выбора" не поддерживается.
  /// </summary>
  [Serializable]
  public class ListComboBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает комбоблок
    /// </summary>
    /// <param name="items">Список элементов, из которых можно выбирать</param>
    public ListComboBox(string[] items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      _Items = items;
    }

    #endregion

    #region Свойства

    #region Items

    /// <summary>
    /// Массив строк для выпадающего списка.
    /// Задается в конструкторе.
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;

    #endregion

    #region SelectedIndex

    /// <summary>
    /// Текушее значение (индекс в массиве Items).
    /// По умолчанию - 0 - выбран первый элемент списка
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set
      {
        _SelectedIndex = value;
        if (_SelectedIndexEx != null)
          _SelectedIndexEx.Value = SelectedIndex;
        if (_SelectedCodeEx != null)
          _SelectedCodeEx.Value = SelectedCode;
      }
    }
    private int _SelectedIndex;

    private int _OldSelectedIndex;

    /// <summary>
    /// Управляемое значение для SelectedIndex.
    /// </summary>
    public DepValue<int> SelectedIndexEx
    {
      get
      {
        InitSelectedIndexEx();
        return _SelectedIndexEx;
      }
      set
      {
        InitSelectedIndexEx();
        _SelectedIndexEx.Source = value;
      }
    }
    private DepInput<int> _SelectedIndexEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства SelectedIndexEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasSelectedIndexExProperty { get { return _SelectedIndexEx != null; } }

    private void InitSelectedIndexEx()
    {
      if (_SelectedIndexEx == null)
      {
        _SelectedIndexEx = new DepInput<int>();
        _SelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "SelectedIndexEx");
        _SelectedIndexEx.Value = SelectedIndex;
        _SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndexEx_ValueChanged);
      }
    }

    private void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedIndex = _SelectedIndexEx.Value;
    }

    #endregion

    #region Codes

    /// <summary>
    /// Коды для элементов.
    /// Если свойство установлено, то для сохранения значения между сеансами работы используется код, а не индекс.
    /// По умолчанию свойство содержит значение null (коды не используются).
    /// При установке свойства длина массива должна совпадать с длиной массива Items.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
        CheckNotFixed();
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw new ArgumentException("Неправильная длина массива кодов");
          _Codes = value;
        }
      }
    }
    private string[] _Codes;

    #endregion

    #region SelectedCode

    /// <summary>
    /// Текущая выбранная позиция как код.
    /// Должно быть установлено свойство Codes, иначе возвращается пустая строка.
    /// </summary>
    public string SelectedCode
    {
      get
      {
        if (Codes == null)
          return String.Empty;
        else
          return Codes[SelectedIndex];
      }
      set
      {
        if (Codes == null)
          throw new InvalidOperationException("Свойство Codes не установено");
        SelectedIndex = Array.IndexOf<string>(Codes, value);
      }
    }

    /// <summary>
    /// Управляемое значение для SelectedCode.
    /// </summary>
    public DepValue<string> SelectedCodeEx
    {
      get
      {
        InitSelectedCodeEx();
        return _SelectedCodeEx;
      }
      set
      {
        InitSelectedCodeEx();
        _SelectedCodeEx.Source = value;
      }
    }
    private DepInput<string> _SelectedCodeEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства SelectedCodeEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasSelectedCodeExProperty { get { return _SelectedCodeEx != null; } }

    private void InitSelectedCodeEx()
    {
      if (_SelectedCodeEx == null)
      {
        _SelectedCodeEx = new DepInput<string>();
        _SelectedCodeEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodeEx");
        _SelectedCodeEx.Value = SelectedCode;
        _SelectedCodeEx.ValueChanged += new EventHandler(SelectedCodeEx_ValueChanged);
      }
    }

    private void SelectedCodeEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCode = _SelectedCodeEx.Value;
    }

    #endregion

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _SelectedIndex != _OldSelectedIndex;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetInt("SelectedIndex", _SelectedIndex);
      _OldSelectedIndex = _SelectedIndex;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _SelectedIndex = part.GetInt("SelectedIndex");
      _OldSelectedIndex = SelectedIndex;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (Codes == null)
        part.SetInt(Name, SelectedIndex);
      else
        part.SetString(Name, SelectedCode);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
      {
        if (Codes == null)
          SelectedIndex = part.GetInt(Name);
        else
          SelectedCode = part.GetString(Name);
      }
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок ввода текста с возможностью выбора из выпадающего списка.
  /// Пользователь может как выбрать текст из списка, так и ввести его вручную.
  /// Текущее значение определяется свойством Text.
  /// Список для выбора является фиксированным и задается в конструкторе.
  /// </summary>
  [Serializable]
  public class TextComboBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает комбоблок
    /// </summary>
    /// <param name="items">Список вариантов, из которых можно выбирать готовые значения</param>
    public TextComboBox(string[] items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      _Items = items;
      _MaxLength = Int16.MaxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив строк для выпадающего списка.
    /// Задается в конструкторе.
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;

    /// <summary>
    /// Текущее введенное значение.
    /// По умолчанию - пустая строка
    /// </summary>
    public string Text
    {
      get
      {
        if (_Text == null)
          return String.Empty;
        else
          return _Text;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          _Text = null;
        else
          _Text = value;
      }
    }
    private string _Text; // храним null вместо пустой строки

    private string _OldText;

    /// <summary>
    /// Управляемое значение для Text.
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
    private DepInput<string> _TextEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства TextEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool HasTextExProperty { get { return _TextEx != null; } }

    private void InitTextEx()
    {
      if (_TextEx == null)
      {
        _TextEx = new DepInput<string>();
        _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        _TextEx.Value = Text;
        _TextEx.ValueChanged += new EventHandler(TextEx_ValueChanged);
      }
    }

    private void TextEx_ValueChanged(object sender, EventArgs args)
    {
      Text = _TextEx.Value;
    }

    /// <summary>
    /// Максимальная длина текста. По умолчанию: 32767 символов (Int16.MaxValue).
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public int MaxLength
    {
      get { return _MaxLength; }
      set
      {
        CheckNotFixed();
        _MaxLength = value;
      }
    }
    private int _MaxLength;

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }


    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается сообщение об ошибке, определяемое свойством ErrorRegExMessage.
    /// </summary>
    public string ErrorRegExPattern
    {
      get { return _ErrorRegExPattern; }
      set
      {
        CheckNotFixed();
        _ErrorRegExPattern = value;
      }
    }
    private string _ErrorRegExPattern;

    /// <summary>
    /// Текст сообщения об ошибке, которое выводится, если введенное значение не соответствует регулярному выражению ErrorRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string ErrorRegExMessage
    {
      get { return _ErrorRegExMessage; }
      set
      {
        CheckNotFixed();
        _ErrorRegExMessage = value;
      }
    }
    private string _ErrorRegExMessage;

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается предупреждение, определяемое свойством WarningRegExMessage.
    /// Проверка не выполняется, если обнаружена ошибка при проверке значения с помощью свойства ErrorRegEx.
    /// </summary>
    public string WarningRegExPattern
    {
      get { return _WarningRegExPattern; }
      set
      {
        CheckNotFixed();
        _WarningRegExPattern = value;
      }
    }
    private string _WarningRegExPattern;

    /// <summary>
    /// Текст предупреждения, которое выводится, если введенное значение не соответствует регулярному выражению WarningRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string WarningRegExMessage
    {
      get { return _WarningRegExMessage; }
      set
      {
        CheckNotFixed();
        _WarningRegExMessage = value;
      }
    }
    private string _WarningRegExMessage;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return Text != _OldText;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetString("Text", Text);
      _OldText = Text;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Text = part.GetString("Text");
      _OldText = Text;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, Text);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        Text = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок для выбора одного или нескольких кодов, разделенных запятыми.
  /// </summary>
  [Serializable]
  public class CsvCodesComboBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает комбоблок с заданным списком доступных кодов
    /// </summary>
    /// <param name="codes">Массив кодов, из которых можно выбирать</param>
    public CsvCodesComboBox(string[] codes)
    {
      if (codes == null)
        throw new ArgumentNullException("codes");
      _Codes = codes;
      _SelectedCodes = DataTools.EmptyStrings;
      _OldSelectedCodes = _SelectedCodes;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список доступных кодов. Задается в конструкторе
    /// </summary>
    public string[] Codes { get { return _Codes; } }
    private string[] _Codes;

    /// <summary>
    /// Описания, соответствующие кодам AvailableCodes.
    /// По умолчанию null - нет описаний.
    /// </summary>
    public string[] Names
    {
      get { return _Names; }
      set
      {
        CheckNotFixed();
        if (value != null)
        {
          if (value.Length != _Codes.Length)
            throw new ArgumentException("Неправильная длина массива: " + value.Length.ToString() + ". Ожидалось: " + _Codes.Length.ToString());
          _Names = value;
        }
      }
    }
    private string[] _Names;


    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }

    /// <summary>
    /// Основное свойство - выбранные коды.
    /// По умолчанию - пустой массив кодов
    /// </summary>
    public string[] SelectedCodes
    {
      get { return _SelectedCodes; }
      set
      {
        if (value == null)
          value = DataTools.EmptyStrings;
        _SelectedCodes = value;
      }
    }
    private string[] _SelectedCodes;
    private string[] _OldSelectedCodes;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return !Object.ReferenceEquals(_SelectedCodes, _OldSelectedCodes);
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetString("SelectedCodes", String.Join(",", _SelectedCodes));
      _OldSelectedCodes = _SelectedCodes;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      string s = part.GetString("SelectedCodes");
      if (s.Length == 0)
        _SelectedCodes = DataTools.EmptyStrings;
      else
        _SelectedCodes = s.Split(',');
      _OldSelectedCodes = _SelectedCodes;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, String.Join(",", _SelectedCodes));
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      string s = part.GetString(Name);
      if (s.Length == 0)
        _SelectedCodes = DataTools.EmptyStrings;
      else
        _SelectedCodes = s.Split(',');
    }

    #endregion
  }

  /// <summary>
  /// Цвет фона и текста для информационного сообщения (свойство InfoLabel.ColorType)
  /// </summary>
  [Serializable]
  public enum InfoLabelColorType
  {
    /// <summary>
    /// Сообщение имеет выделенный фон (обычно, желтый)
    /// </summary>
    Info,

    /// <summary>
    /// Сообщение не имеет выделенного фона. Используется цвет, как у остальных элементов блока диалога
    /// </summary>
    Simple
  }

  /// <summary>
  /// Поле с информационным текстом.
  /// Кроме текста, может содержать значок, как MessageBox.
  /// Пользователь не может взаимодействовать с элементом.
  /// Не связано с другими управляющими элементами, как Label.
  /// </summary>
  [Serializable]
  public class InfoLabel : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает окно с текстом
    /// </summary>
    /// <param name="text">Текст сообщения. Не может быть пустой строкой</param>
    public InfoLabel(string text)
    {
      if (String.IsNullOrEmpty(text))
        throw new ArgumentNullException("text");
      _Text = text;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текст сообщения.
    /// Символ "амперсанд" отображается как есть, а не используется для подчеркивания буквы.
    /// Задается в конструкторе.
    /// </summary>
    public string Text { get { return _Text; } }
    private string _Text;

    /// <summary>
    /// Тип цветового оформления.
    /// По умолчанию используется значение Info - сообщение имеет выделенный желтым цветом фон.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public InfoLabelColorType ColorType
    {
      get { return _ColorType; }
      set
      {
        CheckNotFixed();
        _ColorType = value;
      }
    }
    private InfoLabelColorType _ColorType;

    /// <summary>
    /// Значок слева от текста.
    /// По умолчанию - None - значок не отображается.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public MessageBoxIcon Icon
    {
      get { return _Icon; }
      set
      {
        CheckNotFixed();
        _Icon = value;
      }
    }
    private MessageBoxIcon _Icon;

    #endregion
  }

  /// <summary>
  /// Поле ввода имени каталога с кнопкой "Обзор"
  /// </summary>
  [Serializable]
  public class FolderBrowserTextBox : Control
  {
    #region Свойства

    /// <summary>
    /// Выбранный каталог (вход и выход)
    /// </summary>
    public AbsPath Path { get { return _Path; } set { _Path = value; } }
    private AbsPath _Path;

    private AbsPath _OldPath;


    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }

    /// <summary>
    /// Пояснительный текст, который выводится в диалоге выбора каталога при нажатии кнопки "Обзор".
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// По умолчанию текст не задан.
    /// </summary>
    public string Description { get { return _Description; } set { _Description = value; } }
    private string _Description;

    /// <summary>
    /// Должна ли в диалоге выбора отображаться кнопка "Создать папку".
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// По умолчанию - false (когда выбирается папка для выбора существующих файлов).
    /// </summary>
    public bool ShowNewFolderButton
    {
      get { return _ShowNewFolderButton; }
      set
      {
        CheckNotFixed();
        _ShowNewFolderButton = value;
      }
    }
    private bool _ShowNewFolderButton;

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства ShowNewFolderButton. При ShowNewFolderButton=true возвращает RootExists,
    /// а при false - DirectoryExists
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get
      {
        if (_PathValidateMode.HasValue)
          return _PathValidateMode.Value;
        else
          return ShowNewFolderButton ? TestPathMode.RootExists : TestPathMode.DirectoryExists;
      }
      set
      {
        CheckNotFixed();
        if (value == TestPathMode.FileExists)
          throw new ArgumentException();
        _PathValidateMode = value;
      }
    }
    private TestPathMode? _PathValidateMode;

    /// <summary>
    /// Сбрасывает свойство PathValidateMode в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      CheckNotFixed();
      _PathValidateMode = null;
    }

    #endregion

    #region Чтение и запись значений

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _OldPath != Path;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);

      part.SetString("Path", Path.Path);
      _OldPath = Path;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Path = new AbsPath(part.GetString("Path"));
      _OldPath = Path;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.MachineSpecific;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, Path.SlashedPath);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        Path = new AbsPath(part.GetString(Name));
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для OpenFileBox и SaveFileBox
  /// </summary>
  [Serializable]
  public abstract class FileTextBox : Control
  {
    #region Свойства

    /// <summary>
    /// Выбранный файл.
    /// Выбрать можно только один файл, множественный выбор не поддерживается.
    /// </summary>
    public AbsPath Path { get { return _Path; } set { _Path = value; } }
    private AbsPath _Path;

    private AbsPath _OldPath;


    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public CanBeEmptyMode CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private CanBeEmptyMode _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалента установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }

    /// <summary>
    /// Фильтры (разделитель "|"). Например: "Текстовые файлы|*.txt|Все файлы|*.*".
    /// Если свойство не установлено, то предлагается выбирать из полного списка файлов.
    /// Свойство носит рекомендательный характер. Пользователь может ввести путь к файлу вручную без использования диалога выбора.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// </summary>
    public string Filter
    {
      get { return _Filter; }
      set
      {
        CheckNotFixed();
        _Filter = value;
      }
    }
    private string _Filter;

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию - FileExists для OpenFileTextBox и RootExists для SaveFileTextBox
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get { return _PathValidateMode; }
      set
      {
        CheckNotFixed();
        _PathValidateMode = value;
      }
    }
    private TestPathMode _PathValidateMode;

    #endregion

    #region Чтение и запись значений

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _OldPath != Path;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);

      part.SetString("Path", Path.Path);
      _OldPath = Path;
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      Path = new AbsPath(part.GetString("Path"));
      _OldPath = Path;
    }

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.MachineSpecific;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, Path.Path);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        Path = new AbsPath(part.GetString(Name));
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода имени файла с кнопкой "Обзор", показывающей диалог открытия файла
  /// </summary>
  [Serializable]
  public class OpenFileTextBox : FileTextBox
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует свойство PathValidateMode
    /// </summary>
    public OpenFileTextBox()
    {
      PathValidateMode = TestPathMode.FileExists;
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода имени файла с кнопкой "Обзор", показывающей диалог сохранения файла
  /// </summary>
  [Serializable]
  public class SaveFileTextBox : FileTextBox
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует свойство PathValidateMode
    /// </summary>
    public SaveFileTextBox()
    {
      PathValidateMode = TestPathMode.RootExists;
    }

    #endregion
  }


  /// <summary>
  /// Метка (Label) и управляющий элемент (любой Control), располагающиеся рядом на полосе RIBand
  /// </summary>
  [Serializable]
  public class ControlWithLabel : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает элемент
    /// </summary>
    /// <param name="labelText">Текст метки. Должен быть задан. Может содержать амперсанд для подчеркивания символа</param>
    /// <param name="mainControl">Управляюший элемент. Должен быть задан</param>
    public ControlWithLabel(string labelText, Control mainControl)
    {
      _Label = new Label(labelText);
      if (mainControl == null)
        throw new ArgumentNullException("mainControl");
      _MainControl = mainControl;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Метка. Создается в конструкторе.
    /// </summary>
    public Label Label { get { return _Label; } }
    private Label _Label;

    /// <summary>
    /// Основной элемент.
    /// Задается в конструкторе.
    /// </summary>
    public Control MainControl { get { return _MainControl; } }
    private Control _MainControl;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges { get { return _MainControl.HasChanges; } }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      // Не создаем отдельную секцию

      _MainControl.WriteChanges(part);
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      _MainControl.ReadChanges(part);
    }

    /// <summary>
    /// Вызывает соответствующий метод MainControl
    /// </summary>
    /// <param name="cfgType">Запрашиваемый тип секции конфигурации</param>
    /// <returns>true, если секция используется</returns>
    public override bool SupportsCfgType(RIValueCfgType cfgType)
    {
      return _MainControl.SupportsCfgType(cfgType);
    }

    /// <summary>
    /// Вызывает соответствующий метод MainControl
    /// </summary>
    /// <param name="part">Записываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации. Должен проверяться перед записью</param>
    public override void WriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      _MainControl.WriteValues(part, cfgType);
    }

    /// <summary>
    /// Вызывает соответствующий метод MainControl
    /// </summary>
    /// <param name="part">Считываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    public override void ReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      _MainControl.ReadValues(part, cfgType);
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Рекурсивный поиск элемента по имени в MainControl. Также проверяет, не соответствует ли имя Label.Name.
    /// </summary>
    /// <param name="name">Имя элемента</param>
    /// <returns>Найденный элемент или null</returns>
    public override RIItem Find(string name)
    {
      if (String.IsNullOrEmpty(name))
        return null;

      RIItem res = base.Find(name);
      if (res == null && _Label != null)
        res = _Label.Find(name);
      if (res == null)
        res = _MainControl.Find(name);

      return res;
    }

    /// <summary>
    /// Рекурсивное создание списка из всех элементов.
    /// </summary>
    /// <param name="items">Заполняемый список</param>
    public override void GetItems(ICollection<RIItem> items)
    {
      base.GetItems(items);
      if (_Label != null)
        _Label.GetItems(items);
      _MainControl.GetItems(items);
    }

    /// <summary>
    /// Рекурсивная очистка списка сообщений.
    /// Метод не используется в прикладном коде.
    /// </summary>
    public override void ClearErrors()
    {
      base.ClearErrors();
      _Label.ClearErrors();
      _MainControl.ClearErrors();
    }

    /// <summary>
    /// Рекурсивный вызов SetFixed()
    /// </summary>
    protected override void OnSetFixed()
    {
      base.OnSetFixed();
      if (_Label != null)
        _Label.SetFixed();
      _MainControl.SetFixed();
    }

    #endregion
  }

  /// <summary>
  /// Полоса управляющих элементов RIControl.
  /// Каждый элемент располагается на отдельной "строке". Высота, занимаемая "строкой" определяется автоматически.
  /// Интерфейс ICollection реализован только частично. Можно добавлять элементы, но не удалять их.
  /// </summary>
  [Serializable]
  public class Band : RIItem, ICollection<Control>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустую полосу
    /// </summary>
    public Band()
    {
      _Items = new List<Control>();
    }

    #endregion

    #region Список управляющих элементов

    private List<Control> _Items;

    /// <summary>
    /// Доступ к дочерим элементам по индексу
    /// </summary>
    /// <param name="index">Индекс в диапазоне от 0 до (Count-1)</param>
    /// <returns>Элемент</returns>
    public Control this[int index]
    {
      get { return _Items[index]; }
    }

    #endregion

    #region ICollection<RIControl> Members

    /// <summary>
    /// Добавление управляющего элемента на полосу
    /// </summary>
    /// <param name="item">Управляющий элемент или объект ControlWithLabel</param>
    public void Add(Control item)
    {
      CheckNotFixed();
      if (item == null)
        throw new ArgumentNullException("item");
      _Items.Add(item);
    }

    /// <summary>
    /// Добавление управляющего элемента с меткой на полосу.
    /// Создает и добавляет элемент ControlWithLabel.
    /// </summary>
    /// <param name="labelText">Текст метки. Должен быть задан. Может содержать амперсанд для выделения буквы</param>
    /// <param name="control">Основной управляющий элемент. Должен быть задан.</param>
    public void Add(string labelText, Control control)
    {
      Add(new ControlWithLabel(labelText, control));
    }

    void ICollection<Control>.Clear()
    {
      CheckNotFixed();
      throw new NotSupportedException();
    }

    bool ICollection<Control>.Contains(Control item)
    {
      return _Items.Contains(item);
    }

    void ICollection<Control>.CopyTo(Control[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество дочерних элементов на полосе. 
    /// Вложенные элементы не учитываются.
    /// </summary>
    public int Count { get { return _Items.Count; } }

    bool ICollection<Control>.IsReadOnly { get { return false; } }

    bool ICollection<Control>.Remove(Control item)
    {
      CheckNotFixed();
      throw new NotSupportedException();
    }

    #endregion

    #region IEnumerable<RIControl> Members

    /// <summary>
    /// Создает нерекурсивный перечислитель по дочерним элементам.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns></returns>
    public List<Control>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<Control> IEnumerable<Control>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        for (int i = 0; i < _Items.Count; i++)
        {
          if (_Items[i].HasChanges)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        if (_Items[i].HasChanges)
        {
          CfgPart Part2 = part.GetChild("C" + i.ToString(), true);
          _Items[i].WriteChanges(Part2);
        }
      }
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        CfgPart Part2 = part.GetChild("C" + i.ToString(), false);
        if (Part2 != null)
          _Items[i].ReadChanges(Part2);
      }
    }

    /// <summary>
    /// Рекурсивный вызов SupportsCfgType() для дочерних элементов.
    /// Возвращает true, если хотя бы один элемент вернул true.
    /// </summary>
    /// <param name="cfgType">Тип секции</param>
    /// <returns>True, если используется запись в такую секцию</returns>
    public override bool SupportsCfgType(RIValueCfgType cfgType)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        if (_Items[i].SupportsCfgType(cfgType))
          return true;
      }
      return false;
    }


    /// <summary>
    /// Рекурсивная запись значений
    /// </summary>
    /// <param name="part">Секция для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    public override void WriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        try
        {
          _Items[i].WriteValues(part, cfgType);
        }
        catch { }
      }
    }

    /// <summary>
    /// Рекурсивное чтение значений
    /// </summary>
    /// <param name="part">Секция для чтения</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    public override void ReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        try
        {
          _Items[i].ReadValues(part, cfgType);
        }
        catch { }
      }
    }

    #endregion

    #region Другие переопределенные методы

    /// <summary>
    /// Рекурсивный поиск элемента по имени
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Найденный элемент или null</returns>
    public override RIItem Find(string name)
    {
      RIItem res = base.Find(name);
      if (res != null)
        return res;

      for (int i = 0; i < _Items.Count; i++)
      {
        res = _Items[i].Find(name);
        if (res != null)
          return res;
      }

      return null;
    }

    /// <summary>
    /// Рекурсивное построение списка элементов
    /// </summary>
    /// <param name="items">Список для заполнения</param>
    public override void GetItems(ICollection<RIItem> items)
    {
      base.GetItems(items);
      for (int i = 0; i < _Items.Count; i++)
        _Items[i].GetItems(items);
    }

    /// <summary>
    /// Рекурсивная очистка списка ошибок.
    /// Не используется в пользовательском коде.
    /// </summary>
    public override void ClearErrors()
    {
      base.ClearErrors();
      for (int i = 0; i < _Items.Count; i++)
        _Items[i].ClearErrors();
    }

    /// <summary>
    /// Рекурсивный вызов метода
    /// </summary>
    protected override void OnSetFixed()
    {
      base.OnSetFixed();
      for (int i = 0; i < _Items.Count; i++)
        _Items[i].SetFixed();
    }

    #endregion
  }

  /// <summary>
  /// Сериализуемый блок диалога.
  /// Содержит одну "полосу" управляющих элеметов и панель кнопок внизу
  /// </summary>
  [Serializable]
  public class Dialog : RIItem
  {
    #region Конструктор

    /// <summary>
    /// Создает блок диалога
    /// </summary>
    /// <param name="text">Заголовок окна</param>
    public Dialog(string text)
    {
      _Text = text;

      _Controls = new Band();
    }

    #endregion

    #region Заголовок

    /// <summary>
    /// Заголовок блока диалога.
    /// Задается в конструкторе.
    /// </summary>
    public string Text { get { return _Text; } }
    private string _Text;

    #endregion

    #region Полоса элементов

    /// <summary>
    /// Полоса управляющих элементов
    /// </summary>
    public Band Controls { get { return _Controls; } }
    private Band _Controls;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _Controls.HasChanges;
      }
    }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      _Controls.WriteChanges(part);
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _Controls.ReadChanges(part);
    }

    /// <summary>
    /// Вызывает Controls.SupportsCfgType()
    /// </summary>
    /// <param name="cfgType">Тип секции</param>
    /// <returns>True, если используется запись в такую секцию</returns>
    public override bool SupportsCfgType(RIValueCfgType cfgType)
    {
      return _Controls.SupportsCfgType(cfgType);
    }


    /// <summary>
    /// Вызывает Controls.WriteValues()
    /// </summary>
    /// <param name="part">Секция для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    public override void WriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      _Controls.WriteValues(part, cfgType);
    }


    /// <summary>
    /// Вызывает Controls.ReadValues()
    /// </summary>
    /// <param name="part">Секция для чтения</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    public override void ReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      _Controls.ReadValues(part, cfgType);
    }

    /// <summary>
    /// Работа с интерфейсом IRIValueSaver
    /// Выполняет запись значений, если есть сохраняемые данные и <paramref name="saver"/> поддерживает секции конфигурации.
    /// Перехватывает все исключения.
    /// </summary>
    /// <param name="saver">Объект, реализующий IRIValueSaver. Может быть null</param>
    /// <returns>true, если запись выполнена</returns>
    public bool WriteValues(IRIValueSaver saver)
    {
      return RITools.WriteValues(saver, this);
    }

    /// <summary>
    /// Работа с интерфейсом IRIValueSaver
    /// Выполняет чтение значений, если есть сохраняемые данные и <paramref name="saver"/> поддерживает секции конфигурации.
    /// Перехватывает все исключения
    /// </summary>
    /// <param name="saver">Объект, реализующий IRIValueSaver. Может быть null</param>
    /// <returns>true, если чтение выполнено</returns>
    public bool ReadValues(IRIValueSaver saver)
    {
      return RITools.ReadValues(saver, this);
    }

    #endregion

    #region Другие переопределенные методы

    /// <summary>
    /// Рекурсивный поиск управляющего элемента по имени.
    /// Имена элементов являются чувствительными к регистру.
    /// </summary>
    /// <param name="name">Искомое имя</param>
    /// <returns>Ссылка на найденный элемент или null.</returns>
    public override RIItem Find(string name)
    {
      RIItem res = base.Find(name);
      if (res == null)
        res = _Controls.Find(name);

      return res;
    }

    /// <summary>
    /// Создание списка всех элементов блока диалога
    /// </summary>
    /// <param name="items">Заполняемый список</param>
    public override void GetItems(ICollection<RIItem> items)
    {
      base.GetItems(items);
      _Controls.GetItems(items);
    }

    /// <summary>
    /// Рекурсивный вызов.
    /// </summary>
    protected override void OnSetFixed()
    {
      base.OnSetFixed();
      _Controls.SetFixed();
    }

    #endregion

    #region Проверка значений

    /// <summary>
    /// Очистка списка ошибок.
    /// Вызывается из метода Validate().
    /// Не используется в прикладном коде.
    /// </summary>
    public override void ClearErrors()
    {
      base.ClearErrors();
      _Controls.ClearErrors();
    }

    /// <summary>
    /// Выполнить проверку значений в диалоге.
    /// Возвращает false, если хотя бы для одного элемента выставлен признак ошибки.
    /// Не используется в прикладном коде.
    /// </summary>
    /// <returns>Успех проверки</returns>
    public bool Validate()
    {
      ClearErrors();
      if (_Validating != null)
        _Validating(this, EventArgs.Empty);
      RIItem ErrorItem = FindError();
      return ErrorItem == null;
    }

    /// <summary>
    /// Событие вызывается на вызывающей стороне для проверки корректности введенных значений
    /// </summary>
    public event EventHandler Validating
    {
      add { _Validating += value; }
      remove { _Validating -= value; }
    }
    [NonSerialized]
    private EventHandler _Validating;

    #endregion
  }
}
