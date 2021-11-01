using FreeLibSet.Config;
using FreeLibSet.DependedValues;
using FreeLibSet.IO;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;

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

namespace FreeLibSet.RI
{
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

        if (_EnabledEx != null)
          _EnabledEx.Value = value;
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
    /// Возвращает true, если обработчик свойства EnabledEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool EnabledExConnected
    {
      get
      {
        if (_EnabledEx == null)
          return false;
        else
          return _EnabledEx.IsConnected;
      }
    }


    private void InitEnabledEx()
    {
      if (_EnabledEx == null)
      {
        _EnabledEx = new DepInput<bool>();
        _EnabledEx.OwnerInfo = new DepOwnerInfo(this, "EnabledEx");
        _EnabledEx.Value = true;
        _EnabledEx.ValueChanged += new EventHandler(EnabledEx_ValueChanged);
      }
    }

    void EnabledEx_ValueChanged(object sender, EventArgs args)
    {
      Enabled = _EnabledEx.Value;
    }

    #endregion

    #region Проверка элемента

    /// <summary>
    /// Список объектов для проверки корректности значения элемента (валидаторов).
    /// Поддерживаются ошибки и предупреждения с подсветкой и выдачей всплывающей подсказки.
    /// При нажатии кнопки "ОК" в диалоге устанавливается фокус на первый управляющий элемент, для которого валидатор вернул ошибку, и закрытие диалога предотвращается.
    /// Валидатор считается "сработавшим", если вычисляемое выражение (Validator.Expression) возвращает false. 
    /// Как правило, в качестве аргументов выражения выступают управляемые свойства текущего элемента управления, но могут использоваться и свойства других элементов.
    /// Если у управляющего элемента есть проверки, задаваемые свойствами (например, CanBeEmpty=false для TextBox), то такие проверки выполняются до опроса объектов из списка Validators.
    /// 
    /// Если проверку корректности введенного значения нельзя реализовать с помощью объектов Validator, используйте обработчик события Dialog.Validating,
    /// которое вызывается на стороне сервера.
    /// </summary>
    public UIValidatorList Validators
    {
      get
      {
        if (_Validators == null)
          _Validators = new UIValidatorList();
        return _Validators;
      }
    }
    private UIValidatorList _Validators;

    /// <summary>
    /// Возвращает true, если список Validators не пустой.
    /// Используется для оптимизации, вместо обращения к Validators.Count, позволяя обойтись без создания объекта списка, когда у управляющего элемента нет валидаторов.
    /// </summary>
    public bool HasValidators
    {
      get
      {
        if (_Validators == null)
          return false;
        else
          return _Validators.Count > 0;
      }
    }

    /// <summary>
    /// Блокирует список Validators от изменений
    /// </summary>
    protected override void OnSetFixed()
    {
      base.OnSetFixed();

      if (_Validators != null)
        _Validators.SetReadOnly();
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
      _CanBeEmptyMode = UIValidateState.Error;
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
    /// Возвращает true, если обработчик свойства TextEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool TextExConnected
    {
      get
      {
        if (_TextEx == null)
          return false;
        else
          return _TextEx.IsConnected;
      }
    }

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

        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
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
    /// Возвращает true, если обработчик свойства ReadOnlyEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool ReadOnlyExConnected
    {
      get
      {
        if (_ReadOnlyEx == null)
          return false;
        else
          return _ReadOnlyEx.IsConnected;
      }
    }

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
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
  /// Поле ввода числа. Базовый класс для IntEditBox, SingleEditBox, DoubleEditBox и DecimalEditBox
  /// </summary>
  [Serializable]
  public class BaseNumEditBox<T> : Control
      where T : struct, IFormattable, IComparable<T>
{
    #region Конструктор

    /// <summary>
    /// Создает поле ввода
    /// </summary>
    public BaseNumEditBox()
    {
      _Format = String.Empty;
    }

    #endregion

    #region Свойства

    #region Value/NValue

    #region NValue

    /// <summary>
    /// Введенное значение.
    /// Может быть null, если нет введенного значения.
    /// </summary>
    public T? NValue
    {
      get { return _NValue; }
      set
      {
        _NValue = value;
        if (_NValueEx != null)
          _NValueEx.Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = this.Value;
      }
    }
    private T? _NValue;

    protected T? OldNValue;

    /// <summary>
    /// Управляемое значение для NValue.
    /// </summary>
    public DepValue<T?> NValueEx
    {
      get
      {
        InitNValueEx();
        return _NValueEx;
      }
      set
      {
        InitNValueEx();
        _NValueEx.Source = value;
      }
    }
    private DepInput<T?> _NValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства NValueEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool NValueExConnected
    {
      get
      {
        if (_NValueEx == null)
          return false;
        else
          return _NValueEx.IsConnected;
      }
    }

    private void InitNValueEx()
    {
      if (_NValueEx == null)
      {
        _NValueEx = new DepInput<T?>();
        _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
        _NValueEx.Value = NValue;
        _NValueEx.ValueChanged += new EventHandler(NValueEx_ValueChanged);
      }
    }

    private void NValueEx_ValueChanged(object sender, EventArgs args)
    {
      NValue = _NValueEx.Value;
    }

    #endregion

    #region Value

    /// <summary>
    /// Введенное значение.
    /// Если нет введенного значения, свойство возвращает 0.
    /// Это свойство следует использовать при CanBeEmpty=false.
    /// </summary>
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<T> ValueEx
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
    private DepInputWithCheck<T> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool ValueExConnected
    {
      get
      {
        if (_ValueEx == null)
          return false;
        else
          return _ValueEx.IsConnected;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInputWithCheck<T>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.CheckValue += ValueEx_CheckValue;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<T> args)
    {
      if ((!NValue.HasValue) && args.CurrValue.Equals(default(T)))
        args.Forced = true;
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    #endregion

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Format

    /// <summary>
    /// Строка формата для числа
    /// </summary>
    public string Format
    {
      get { return _Format; }
      set
      {
        CheckNotFixed();
        if (value == null)
          _Format = String.Empty;
        else
          _Format = value;
      }
    }
    private string _Format;

    /// <summary>
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве Format.
    /// Установка значения свойства создает формат.
    /// </summary>
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region Minimum/Maximum

    /// <summary>
    /// Минимальное значение. По умолчанию - не задано
    /// </summary>
    public T? Minimum
    {
      get { return _Minimum; }
      set
      {
        CheckNotFixed();
        _Minimum = value;
      }
    }
    private T? _Minimum;

    /// <summary>
    /// Максимальное значение. По умолчанию - не задано
    /// </summary>
    public T Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private T _Maximum;

    #endregion

    #region Increment

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значение в поле можно прокручивать с помощью
    /// стрелочек вверх/вниз или колесиком мыши.
    /// Если свойство равно 0 (по умолчанию), то число можно вводить только вручную
    /// </summary>
    public T Increment
    {
      get { return _Increment; }
      set
      {
        CheckNotFixed();
        if (value.CompareTo(default(T)) < 0)
          throw new ArgumentOutOfRangeException("value", value, "Значение должно быть больше или равно 0");
        _Increment = value;
      }
    }
    private T _Increment;

    #endregion

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает true, если элемент поддерживает сохранение своих значений между сеансами работы
    /// в секции конфигурации заданного типа.
    /// </summary>
    /// <param name="сfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType сfgType)
    {
      return сfgType == RIValueCfgType.Default;
    }

    #endregion
}

  /// <summary>
  /// Поле ввода целого числа
  /// </summary>
  [Serializable]
  public class IntEditBox : BaseNumEditBox<Int32>
  {
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
        return OldNValue != NValue;
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
      part.SetNullableInt("Value", NValue);
      OldNValue = NValue;
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
      NValue = part.GetNullableInt("Value");
      OldNValue = NValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableInt(Name, NValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NValue = part.GetNullableInt(Name);
    }

    #endregion

    #region Заглушка

    /// <summary>
    /// Возвращает 0
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public override int DecimalPlaces
    {
      get { return 0; }
      set { }
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числа одинарной точности
  /// </summary>
  [Serializable]
  public class SingleEditBox : BaseNumEditBox<Single>
  {
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
        return OldNValue != NValue;
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
      part.SetNullableSingle("Value", NValue);
      OldNValue = NValue;
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
      NValue = part.GetNullableSingle("Value");
      OldNValue = NValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableSingle(Name, NValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NValue = part.GetNullableSingle(Name);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числа двойной точности
  /// </summary>
  [Serializable]
  public class DoubleEditBox : BaseNumEditBox<Double>
  {
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
        return OldNValue != NValue;
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
      part.SetNullableDouble("Value", NValue);
      OldNValue = NValue;
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
      NValue = part.GetNullableDouble("Value");
      OldNValue = NValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDouble(Name, NValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NValue = part.GetNullableDouble(Name);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числа типа decimal
  /// </summary>
  [Serializable]
  public class DecimalEditBox : BaseNumEditBox<Decimal>
  {
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
        return OldNValue != NValue;
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
      part.SetNullableDecimal("Value", NValue);
      OldNValue = NValue;
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
      NValue = part.GetNullableDecimal("Value");
      OldNValue = NValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDecimal(Name, NValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NValue = part.GetNullableDecimal(Name);
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
    /// Возвращает true, если обработчик свойства CheckStateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool CheckStateExConnected
    {
      get
      {
        if (_CheckStateEx == null)
          return false;
        else
          return _CheckStateEx.IsConnected;
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
    /// Возвращает true, если обработчик свойства CheckedEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool CheckedExConnected
    {
      get
      {
        if (_CheckedEx == null)
          return false;
        else
          return _CheckedEx.IsConnected;
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
    /// Возвращает true, если обработчик свойства SelectedIndexEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool SelectedIndexExConnected
    {
      get
      {
        if (_SelectedIndexEx == null)
          return false;
        else
          return _SelectedIndexEx.IsConnected;
      }
    }

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
    /// Возвращает true, если обработчик свойства SelectedCodeEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool SelectedCodeExConnected
    {
      get
      {
        if (_SelectedCodeEx == null)
          return false;
        else
          return _SelectedCodeEx.IsConnected;
      }
    }

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
  /// Поле ввода даты и/или времени
  /// </summary>
  [Serializable]
  public class DateTimeBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает управляющий элемент
    /// </summary>
    public DateTimeBox()
    {
      _CanBeEmptyMode = UIValidateState.Error;
      _Kind = EditableDateTimeFormatterKind.Date;
    }

    #endregion

    #region Свойства

    #region Kind

    /// <summary>
    /// Формат вводимого значения.
    /// По умолчанию - Date
    /// </summary>
    public EditableDateTimeFormatterKind Kind 
    {
      get { return _Kind; }
      set
      {
        CheckNotFixed();
        _Kind = value;
      }
    }
    private EditableDateTimeFormatterKind _Kind;

    #endregion

    #region Value/NValue

    #region NValue

    /// <summary>
    /// Введенное значение.
    /// Может быть null, если нет введенного значения.
    /// </summary>
    public DateTime? NValue
    {
      get { return _NValue; }
      set
      {
        _NValue = value;
        if (_NValueEx != null)
          _NValueEx.Value = value;
        if (_ValueEx != null)
          _ValueEx.Value = this.Value;
      }
    }
    private DateTime? _NValue;

    private DateTime? _OldNValue;

    /// <summary>
    /// Управляемое значение для NValue.
    /// </summary>
    public DepValue<DateTime?> NValueEx
    {
      get
      {
        InitNValueEx();
        return _NValueEx;
      }
      set
      {
        InitNValueEx();
        _NValueEx.Source = value;
      }
    }
    private DepInput<DateTime?> _NValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства NValueEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool NValueExConnected
    {
      get
      {
        if (_NValueEx == null)
          return false;
        else
          return _NValueEx.IsConnected;
      }
    }

    private void InitNValueEx()
    {
      if (_NValueEx == null)
      {
        _NValueEx = new DepInput<DateTime?>();
        _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
        _NValueEx.Value = NValue;
        _NValueEx.ValueChanged += new EventHandler(NValueEx_ValueChanged);
      }
    }

    private void NValueEx_ValueChanged(object sender, EventArgs args)
    {
      NValue = _NValueEx.Value;
    }

    #endregion

    #region Value

    /// <summary>
    /// Введенное значение.
    /// Если нет введенного значения, свойство возвращает DateTime.MinValue.
    /// Это свойство следует использовать при CanBeEmpty=false.
    /// </summary>
    public DateTime Value
    {
      get { return NValue ?? DateTime.MinValue; }
      set { NValue = value; }
    }

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<DateTime> ValueEx
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
    private DepInputWithCheck<DateTime> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool ValueExConnected
    {
      get
      {
        if (_ValueEx == null)
          return false;
        else
          return _ValueEx.IsConnected;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInputWithCheck<DateTime>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.CheckValue += ValueEx_CheckValue;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
    }

    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<DateTime> args)
    {
      if ((!NValue.HasValue) && args.CurrValue.Equals(default(DateTime)))
        args.Forced = true;
    }

    private void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    #endregion

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Minimum/Maximum

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
        return NValue != _OldNValue;
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
      part.SetNullableDate("Value", NValue);
      _OldNValue = NValue;
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
      NValue = part.GetNullableDate("Value");
      _OldNValue = NValue;
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
      if (NValue.HasValue)
        part.SetDate(Name, NValue.Value);
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
        NValue = part.GetNullableDate(Name);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода интервала дат. Содержит два отдельных поля даты, связанных проверкой корректности диапазона.
  /// Начальная дата диапазона доступна через свойства FirstDate,NFirstDate,FirstDateEx,NFirstDateEx, а конечная -
  /// через LasttDate,NLastDate,LastDateEx,NLastDateEx
  /// Поддерживается пустой интервал и полуоткрытие интервалы.
  /// </summary>
  [Serializable]
  public class DateRangeBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает управляющий элемент
    /// </summary>
    public DateRangeBox()
    {
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Редактируемое значение

    #region NFirstDate

    /// <summary>
    /// Начальная дата диапазона с поддержкой значения null
    /// </summary>
    public DateTime? NFirstDate
    {
      get { return _NFirstDate; }
      set
      {
        _NFirstDate = value;
        if (_NFirstDateEx != null)
          _NFirstDateEx.Value = value;
        if (_FirstDateEx != null)
          _FirstDateEx.Value = FirstDate;
      }
    }
    private DateTime? _NFirstDate;

    private DateTime? _OldNFirstDate;

    /// <summary>
    /// Управляемое значение для NFirstDate.
    /// </summary>
    public DepValue<DateTime?> NFirstDateEx
    {
      get
      {
        InitNFirstDateEx();
        return _NFirstDateEx;
      }
      set
      {
        InitNFirstDateEx();
        _NFirstDateEx.Source = value;
      }
    }
    private DepInput<DateTime?> _NFirstDateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства NFirstDateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool NFirstDateExConnected
    {
      get
      {
        if (_NFirstDateEx == null)
          return false;
        else
          return _NFirstDateEx.IsConnected;
      }
    }

    private void InitNFirstDateEx()
    {
      if (_NFirstDateEx == null)
      {
        _NFirstDateEx = new DepInput<DateTime?>();
        _NFirstDateEx.OwnerInfo = new DepOwnerInfo(this, "NFirstDateEx");
        _NFirstDateEx.Value = NFirstDate;
        _NFirstDateEx.ValueChanged += new EventHandler(NFirstDateEx_ValueChanged);
      }
    }

    private void NFirstDateEx_ValueChanged(object sender, EventArgs args)
    {
      NFirstDate = _NFirstDateEx.Value;
    }

    #endregion

    #region FirstDate

    /// <summary>
    /// Начальная дата диапазона без значения null
    /// </summary>
    public DateTime FirstDate
    {
      get { return NFirstDate ?? DateRange.Whole.FirstDate; }
      set { NFirstDate = value; }
    }

    /// <summary>
    /// Управляемое значение для FirstDate.
    /// </summary>
    public DepValue<DateTime> FirstDateEx
    {
      get
      {
        InitFirstDateEx();
        return _FirstDateEx;
      }
      set
      {
        InitFirstDateEx();
        _FirstDateEx.Source = value;
      }
    }
    private DepInput<DateTime> _FirstDateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства FirstDateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool FirstDateExConnected
    {
      get
      {
        if (_FirstDateEx == null)
          return false;
        else
          return _FirstDateEx.IsConnected;
      }
    }

    private void InitFirstDateEx()
    {
      if (_FirstDateEx == null)
      {
        _FirstDateEx = new DepInput<DateTime>();
        _FirstDateEx.OwnerInfo = new DepOwnerInfo(this, "FirstDateEx");
        _FirstDateEx.Value = FirstDate;
        _FirstDateEx.ValueChanged += new EventHandler(FirstDateEx_ValueChanged);
      }
    }

    private void FirstDateEx_ValueChanged(object sender, EventArgs args)
    {
      FirstDate = _FirstDateEx.Value;
    }

    #endregion

    #region NLastDate

    /// <summary>
    /// Конечная дата диапазона с поддержкой значения null
    /// </summary>
    public DateTime? NLastDate
    {
      get { return _NLastDate; }
      set
      {
        _NLastDate = value;
        if (_NLastDateEx != null)
          _NLastDateEx.Value = value;
        if (_LastDateEx != null)
          _LastDateEx.Value = LastDate;
      }
    }
    private DateTime? _NLastDate;

    private DateTime? _OldNLastDate;

    /// <summary>
    /// Управляемое значение для NLastDate.
    /// </summary>
    public DepValue<DateTime?> NLastDateEx
    {
      get
      {
        InitNLastDateEx();
        return _NLastDateEx;
      }
      set
      {
        InitNLastDateEx();
        _NLastDateEx.Source = value;
      }
    }
    private DepInput<DateTime?> _NLastDateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства NLastDateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool NLastDateExConnected
    {
      get
      {
        if (_NLastDateEx == null)
          return false;
        else
          return _NLastDateEx.IsConnected;
      }
    }

    private void InitNLastDateEx()
    {
      if (_NLastDateEx == null)
      {
        _NLastDateEx = new DepInput<DateTime?>();
        _NLastDateEx.OwnerInfo = new DepOwnerInfo(this, "NLastDateEx");
        _NLastDateEx.Value = NLastDate;
        _NLastDateEx.ValueChanged += new EventHandler(NLastDateEx_ValueChanged);
      }
    }

    private void NLastDateEx_ValueChanged(object sender, EventArgs args)
    {
      NLastDate = _NLastDateEx.Value;
    }

    #endregion

    #region LastDate

    /// <summary>
    /// Конечная дата диапазона без значения null
    /// </summary>
    public DateTime LastDate
    {
      get { return NLastDate ?? DateRange.Whole.LastDate; }
      set { NLastDate = value; }
    }

    /// <summary>
    /// Управляемое значение для LastDate.
    /// </summary>
    public DepValue<DateTime> LastDateEx
    {
      get
      {
        InitLastDateEx();
        return _LastDateEx;
      }
      set
      {
        InitLastDateEx();
        _LastDateEx.Source = value;
      }
    }
    private DepInput<DateTime> _LastDateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства LastDateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool LastDateExConnected
    {
      get
      {
        if (_LastDateEx == null)
          return false;
        else
          return _LastDateEx.IsConnected;
      }
    }

    private void InitLastDateEx()
    {
      if (_LastDateEx == null)
      {
        _LastDateEx = new DepInput<DateTime>();
        _LastDateEx.OwnerInfo = new DepOwnerInfo(this, "LastDateEx");
        _LastDateEx.Value = LastDate;
        _LastDateEx.ValueChanged += new EventHandler(LastDateEx_ValueChanged);
      }
    }

    private void LastDateEx_ValueChanged(object sender, EventArgs args)
    {
      LastDate = _LastDateEx.Value;
    }

    #endregion

    #region DateRange

    /// <summary>
    /// Вспомогательное свойство для доступа к интервалу дат.
    /// Если начальная и/или конечная дата не установлена, свойство возвращает DateRange.Empty.
    /// Установка значения свойства в DateRange.Empty очищает обе даты
    /// </summary>
    public DateRange DateRange
    {
      get
      {
        if (NFirstDate.HasValue && NLastDate.HasValue)
          return new DateRange(NFirstDate.Value, NLastDate.Value);
        else
          return DateRange.Empty;
      }
      set
      {
        if (value.IsEmpty)
        {
          NFirstDate = null;
          NLastDate = null;
        }
        else
        {
          NFirstDate = value.FirstDate;
          NLastDate = value.LastDate;
        }
      }
    }

    #endregion

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Могут ли поля быть пустыми.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Могут ли поля быть пустыми.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
        return NFirstDate != _OldNFirstDate || NLastDate != _OldNLastDate;
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
      part.SetNullableDate("FirstDate", NFirstDate);
      part.SetNullableDate("LastDate", NLastDate);
      _OldNFirstDate = NFirstDate;
      _OldNLastDate = NLastDate;
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
      NFirstDate = part.GetNullableDate("FirstDate");
      NLastDate = part.GetNullableDate("LastDate");
      _OldNFirstDate = NFirstDate;
      _OldNLastDate = NLastDate;
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
      if (NFirstDate.HasValue)
        part.SetDate(Name + "-First", NFirstDate.Value);
      else
        part.SetString(Name + "-First", String.Empty);

      if (NLastDate.HasValue)
        part.SetDate(Name + "-Last", NLastDate.Value);
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
        NFirstDate = part.GetNullableDate(Name + "-First");
      if (part.HasValue(Name + "-Last"))
        NLastDate = part.GetNullableDate(Name + "-Last");
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода одиночной даты или интервала дат.
  /// Пустой интервал (DateRange.Empty) поддерживается. Полуоткрытые интервалы не поддерживаются.
  /// </summary>
  [Serializable]
  public class DateOrRangeBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Создает управляющий элемент
    /// </summary>
    public DateOrRangeBox()
    {
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Вводимое значение FirstDate/LastDate/DateRange

    #region DateRange

    /// <summary>
    /// Интервал дат.
    /// Пустой интервал задается как DateRange.Empty
    /// </summary>
    public DateRange DateRange
    {
      get { return _DateRange; }
      set
      {
        _DateRange = value;
        if (_FirstDateEx != null)
          _FirstDateEx.Value = FirstDate;
        if (_LastDateEx != null)
          _LastDateEx.Value = FirstDate;
      }
    }
    private DateRange _DateRange;

    private DateRange _OldDateRange;

    #endregion

    #region FirstDate

    /// <summary>
    /// Начальная дата диапазона.
    /// Пустой диапазон не поддерживается. Если DateRange.IsEmpty=true, возвращается минимально возможная дата.
    /// Установка значения свойства может привести к изменению конечной даты, если задаваемое значение больше, чем текущая конечная дата.
    /// </summary>
    public DateTime FirstDate
    {
      get
      {
        if (DateRange.IsEmpty)
          return DateRange.Whole.FirstDate;
        else
          return DateRange.FirstDate;
      }
      set
      {
        if (DateRange.IsEmpty)
          DateRange = new DateRange(value, value);
        else
          DateRange = new DateRange(value, DataTools.Max(DateRange.LastDate, value.Date));
      }
    }

    /// <summary>
    /// Управляемое значение для FirstDate.
    /// </summary>
    public DepValue<DateTime> FirstDateEx
    {
      get
      {
        InitFirstDateEx();
        return _FirstDateEx;
      }
      set
      {
        InitFirstDateEx();
        _FirstDateEx.Source = value;
      }
    }
    private DepInput<DateTime> _FirstDateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства FirstDateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool FirstDateExConnected
    {
      get
      {
        if (_FirstDateEx == null)
          return false;
        else
          return _FirstDateEx.IsConnected;
      }
    }

    private void InitFirstDateEx()
    {
      if (_FirstDateEx == null)
      {
        _FirstDateEx = new DepInput<DateTime>();
        _FirstDateEx.OwnerInfo = new DepOwnerInfo(this, "FirstDateEx");
        _FirstDateEx.Value = FirstDate;
        _FirstDateEx.ValueChanged += new EventHandler(FirstDateEx_ValueChanged);
      }
    }

    private void FirstDateEx_ValueChanged(object sender, EventArgs args)
    {
      FirstDate = _FirstDateEx.Value;
    }

    #endregion

    #region LastDate

    /// <summary>
    /// Конечная дата диапазона.
    /// Пустой диапазон не поддерживается. Если DateRange.IsEmpty=true, возвращается максимально возможная дата.
    /// Установка значения свойства может привести к изменению начальной даты, если задаваемое значение меньше, чем текущая начальная дата.
    /// </summary>
    public DateTime LastDate
    {
      get
      {
        if (DateRange.IsEmpty)
          return DateRange.Whole.LastDate;
        else
          return DateRange.LastDate;
      }
      set
      {
        if (DateRange.IsEmpty)
          DateRange = new DateRange(value, value);
        else
          DateRange = new DateRange(DataTools.Min(DateRange.FirstDate, value.Date), value);
      }
    }

    /// <summary>
    /// Управляемое значение для LastDate.
    /// </summary>
    public DepValue<DateTime> LastDateEx
    {
      get
      {
        InitLastDateEx();
        return _LastDateEx;
      }
      set
      {
        InitLastDateEx();
        _LastDateEx.Source = value;
      }
    }
    private DepInput<DateTime> _LastDateEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства LastDateEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool LastDateExConnected
    {
      get
      {
        if (_LastDateEx == null)
          return false;
        else
          return _LastDateEx.IsConnected;
      }
    }

    private void InitLastDateEx()
    {
      if (_LastDateEx == null)
      {
        _LastDateEx = new DepInput<DateTime>();
        _LastDateEx.OwnerInfo = new DepOwnerInfo(this, "LastDateEx");
        _LastDateEx.Value = LastDate;
        _LastDateEx.ValueChanged += new EventHandler(LastDateEx_ValueChanged);
      }
    }

    private void LastDateEx_ValueChanged(object sender, EventArgs args)
    {
      LastDate = _LastDateEx.Value;
    }

    #endregion

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Допускается ли пустой интервал дат.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Допускается ли пустой интервал дат.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// Нет возможности задать режим проверки только для поля начальной или конечной даты.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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

    #region Year

    /// <summary>
    /// Текущее значение - выбранный год
    /// </summary>
    public int Year
    {
      get { return _Year; }
      set
      {
        _Year = value;
        if (_YearEx != null)
          _YearEx.Value = value;
        if (_YMEx != null)
          _YMEx.Value = YM;
      }
    }
    private int _Year;

    private int _OldYear;


    /// <summary>
    /// Управляемое значение для Year.
    /// </summary>
    public DepValue<int> YearEx
    {
      get
      {
        InitYearEx();
        return _YearEx;
      }
      set
      {
        InitYearEx();
        _YearEx.Source = value;
      }
    }
    private DepInput<int> _YearEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства YearEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool YearExConnected
    {
      get
      {
        if (_YearEx == null)
          return false;
        else
          return _YearEx.IsConnected;
      }
    }

    private void InitYearEx()
    {
      if (_YearEx == null)
      {
        _YearEx = new DepInput<int>();
        _YearEx.OwnerInfo = new DepOwnerInfo(this, "YearEx");
        _YearEx.Value = Year;
        _YearEx.ValueChanged += new EventHandler(YearEx_ValueChanged);
      }
    }

    private void YearEx_ValueChanged(object sender, EventArgs args)
    {
      Year = _YearEx.Value;
    }

    #endregion

    #region Month

    /// <summary>
    /// Текущее значение - выбранный месяц (1-12)
    /// </summary>
    public int Month
    {
      get { return _Month; }
      set
      {
        _Month = value;
        if (_MonthEx != null)
          _MonthEx.Value = value;
        if (_YMEx != null)
          _YMEx.Value = YM;
      }
    }
    private int _Month;

    private int _OldMonth;

    /// <summary>
    /// Управляемое значение для Month.
    /// </summary>
    public DepValue<int> MonthEx
    {
      get
      {
        InitMonthEx();
        return _MonthEx;
      }
      set
      {
        InitMonthEx();
        _MonthEx.Source = value;
      }
    }
    private DepInput<int> _MonthEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства MonthEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool MonthExConnected
    {
      get
      {
        if (_MonthEx == null)
          return false;
        else
          return _MonthEx.IsConnected;
      }
    }

    private void InitMonthEx()
    {
      if (_MonthEx == null)
      {
        _MonthEx = new DepInput<int>();
        _MonthEx.OwnerInfo = new DepOwnerInfo(this, "MonthEx");
        _MonthEx.Value = Month;
        _MonthEx.ValueChanged += new EventHandler(MonthEx_ValueChanged);
      }
    }

    private void MonthEx_ValueChanged(object sender, EventArgs args)
    {
      Month = _MonthEx.Value;
    }

    #endregion

    #region YM

    /// <summary>
    /// Текущее значение - год и месяц вместе
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

        // YMEx.ValueChanged() был вызван
      }
    }

    /// <summary>
    /// Управляемое значение для Value.
    /// </summary>
    public DepValue<YearMonth> YMEx
    {
      get
      {
        InitYMEx();
        return _YMEx;
      }
      set
      {
        InitYMEx();
        _YMEx.Source = value;
      }
    }
    private DepInput<YearMonth> _YMEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства YMEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool YMExConnected
    {
      get
      {
        if (_YMEx == null)
          return false;
        else
          return _YMEx.IsConnected;
      }
    }

    private void InitYMEx()
    {
      if (_YMEx == null)
      {
        _YMEx = new DepInput<YearMonth>();
        _YMEx.OwnerInfo = new DepOwnerInfo(this, "YMEx");
        _YMEx.Value = YM;
        _YMEx.ValueChanged += new EventHandler(YMEx_ValueChanged);
      }
    }

    private void YMEx_ValueChanged(object sender, EventArgs args)
    {
      YM = _YMEx.Value;
    }

    #endregion

    #region Minumum и Maximum

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

    #region DateRange

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

    #region Year

    /// <summary>
    /// Выбранный год
    /// </summary>
    public int Year
    {
      get { return _Year; }
      set
      {
        _Year = value;
        if (_YearEx != null)
          _YearEx.Value = value;
        if (_FirstYMEx != null)
          _FirstYMEx.Value = this.FirstYM;
        if (_LastYMEx != null)
          _LastYMEx.Value = this.LastYM;
      }
    }
    private int _Year;

    private int _OldYear;

    /// <summary>
    /// Управляемое значение для Year.
    /// </summary>
    public DepValue<int> YearEx
    {
      get
      {
        InitYearEx();
        return _YearEx;
      }
      set
      {
        InitYearEx();
        _YearEx.Source = value;
      }
    }
    private DepInput<int> _YearEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства YearEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool YearExConnected
    {
      get
      {
        if (_YearEx == null)
          return false;
        else
          return _YearEx.IsConnected;
      }
    }

    private void InitYearEx()
    {
      if (_YearEx == null)
      {
        _YearEx = new DepInput<int>();
        _YearEx.OwnerInfo = new DepOwnerInfo(this, "YearEx");
        _YearEx.Value = Year;
        _YearEx.ValueChanged += new EventHandler(YearEx_ValueChanged);
      }
    }

    private void YearEx_ValueChanged(object sender, EventArgs args)
    {
      Year = _YearEx.Value;
    }

    #endregion

    #region FirstMonth

    /// <summary>
    /// Выбранный первый месяц (1 .. 12)
    /// </summary>
    public int FirstMonth
    {
      get { return _FirstMonth; }
      set
      {
        _FirstMonth = value;
        if (_FirstMonthEx != null)
          _FirstMonthEx.Value = value;
        if (_FirstYMEx != null)
          _FirstYMEx.Value = this.FirstYM;
      }
    }
    private int _FirstMonth;

    private int _OldFirstMonth;

    /// <summary>
    /// Управляемое значение для FirstMonth.
    /// </summary>
    public DepValue<int> FirstMonthEx
    {
      get
      {
        InitFirstMonthEx();
        return _FirstMonthEx;
      }
      set
      {
        InitFirstMonthEx();
        _FirstMonthEx.Source = value;
      }
    }
    private DepInput<int> _FirstMonthEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства FirstMonthEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool FirstMonthExConnected
    {
      get
      {
        if (_FirstMonthEx == null)
          return false;
        else
          return _FirstMonthEx != null;
      }
    }

    private void InitFirstMonthEx()
    {
      if (_FirstMonthEx == null)
      {
        _FirstMonthEx = new DepInput<int>();
        _FirstMonthEx.OwnerInfo = new DepOwnerInfo(this, "FirstMonthEx");
        _FirstMonthEx.Value = FirstMonth;
        _FirstMonthEx.ValueChanged += new EventHandler(FirstMonthEx_ValueChanged);
      }
    }

    private void FirstMonthEx_ValueChanged(object sender, EventArgs args)
    {
      FirstMonth = _FirstMonthEx.Value;
    }

    #endregion

    #region LastMonth

    /// <summary>
    /// Выбранный последний месяц (1 .. 12)
    /// </summary>
    public int LastMonth
    {
      get { return _LastMonth; }
      set
      {
        _LastMonth = value;
        if (_LastMonthEx != null)
          _LastMonthEx.Value = value;
        if (_LastYMEx != null)
          _LastYMEx.Value = this.LastYM;
      }
    }
    private int _LastMonth;

    private int _OldLastMonth;

    /// <summary>
    /// Управляемое значение для LastMonth.
    /// </summary>
    public DepValue<int> LastMonthEx
    {
      get
      {
        InitLastMonthEx();
        return _LastMonthEx;
      }
      set
      {
        InitLastMonthEx();
        _LastMonthEx.Source = value;
      }
    }
    private DepInput<int> _LastMonthEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства LastMonthEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool LastMonthExConnected
    {
      get
      {
        if (_LastMonthEx == null)
          return false;
        else
          return _LastMonthEx.IsConnected;
      }
    }

    private void InitLastMonthEx()
    {
      if (_LastMonthEx == null)
      {
        _LastMonthEx = new DepInput<int>();
        _LastMonthEx.OwnerInfo = new DepOwnerInfo(this, "LastMonthEx");
        _LastMonthEx.Value = LastMonth;
        _LastMonthEx.ValueChanged += new EventHandler(LastMonthEx_ValueChanged);
      }
    }

    private void LastMonthEx_ValueChanged(object sender, EventArgs args)
    {
      LastMonth = _LastMonthEx.Value;
    }

    #endregion

    #region FirstYM

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

        // Управляемые свойства установлены
      }
    }

    /// <summary>
    /// Управляемое значение для FirstYM.
    /// </summary>
    public DepValue<YearMonth> FirstYMEx
    {
      get
      {
        InitFirstYMEx();
        return _FirstYMEx;
      }
      set
      {
        InitFirstYMEx();
        _FirstYMEx.Source = value;
      }
    }
    private DepInput<YearMonth> _FirstYMEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства FirstYMEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool FirstYMExConnected
    {
      get
      {
        if (_FirstYMEx == null)
          return false;
        else
          return _FirstYMEx.IsConnected;
      }
    }

    private void InitFirstYMEx()
    {
      if (_FirstYMEx == null)
      {
        _FirstYMEx = new DepInput<YearMonth>();
        _FirstYMEx.OwnerInfo = new DepOwnerInfo(this, "FirstYMEx");
        _FirstYMEx.Value = FirstYM;
        _FirstYMEx.ValueChanged += new EventHandler(FirstYMEx_ValueChanged);
      }
    }

    private void FirstYMEx_ValueChanged(object sender, EventArgs args)
    {
      FirstYM = _FirstYMEx.Value;
    }

    #endregion

    #region LastYM

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
        // Управляемые свойства установлены
      }
    }

    /// <summary>
    /// Управляемое значение для LastYM.
    /// </summary>
    public DepValue<YearMonth> LastYMEx
    {
      get
      {
        InitLastYMEx();
        return _LastYMEx;
      }
      set
      {
        InitLastYMEx();
        _LastYMEx.Source = value;
      }
    }
    private DepInput<YearMonth> _LastYMEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства LastYMEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool LastYMExConnected
    {
      get
      {
        if (_LastYMEx == null)
          return false;
        else
          return _LastYMEx.IsConnected;
      }
    }

    private void InitLastYMEx()
    {
      if (_LastYMEx == null)
      {
        _LastYMEx = new DepInput<YearMonth>();
        _LastYMEx.OwnerInfo = new DepOwnerInfo(this, "LastYMEx");
        _LastYMEx.Value = LastYM;
        _LastYMEx.ValueChanged += new EventHandler(LastYMEx_ValueChanged);
      }
    }

    private void LastYMEx_ValueChanged(object sender, EventArgs args)
    {
      LastYM = _LastYMEx.Value;
    }

    #endregion

    #region DateRange

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

    #endregion

    #region YMRange

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

    #endregion

    #region Minimum и Maximum

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
    /// Текущее значение (индекс в массиве Items).
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
    /// Возвращает true, если обработчик свойства SelectedIndexEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool SelectedIndexExConnected
    {
      get
      {
        if (_SelectedIndexEx == null)
          return false;
        else
          return _SelectedIndexEx.IsConnected;
      }
    }

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
    /// Возвращает true, если обработчик свойства SelectedCodeEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool SelectedCodeExConnected
    {
      get
      {
        if (_SelectedCodeEx == null)
          return false;
        else
          return _SelectedCodeEx != null;
      }
    }

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
      _CanBeEmptyMode = UIValidateState.Error;
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
    /// Возвращает true, если обработчик свойства TextEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool TextExConnected
    {
      get
      {
        if (_TextEx == null)
          return false;
        else
          return _TextEx != null;
      }
    }

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
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
  /// Текущий выбор пользователя задает свойство SelectedCodes
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
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    #region Codes и Names

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

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region SelectedCodes

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
        if (_SelectedCodesEx != null)
          _SelectedCodesEx.Value = value;
      }
    }
    private string[] _SelectedCodes;
    private string[] _OldSelectedCodes;

    /// <summary>
    /// Управляемое значение для SelectedCodes.
    /// </summary>
    public DepValue<string[]> SelectedCodesEx
    {
      get
      {
        InitSelectedCodesEx();
        return _SelectedCodesEx;
      }
      set
      {
        InitSelectedCodesEx();
        _SelectedCodesEx.Source = value;
      }
    }
    private DepInput<string[]> _SelectedCodesEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства SelectedCodesEx присоединен к другим объектам в качестве входа или выхода.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    public bool SelectedCodesExConnected
    {
      get
      {
        if (_SelectedCodesEx == null)
          return false;
        else
          return _SelectedCodesEx != null;
      }
    }

    private void InitSelectedCodesEx()
    {
      if (_SelectedCodesEx == null)
      {
        _SelectedCodesEx = new DepInput<string[]>();
        _SelectedCodesEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodesEx");
        _SelectedCodesEx.Value = SelectedCodes;
        _SelectedCodesEx.ValueChanged += new EventHandler(SelectedCodesEx_ValueChanged);
      }
    }

    private void SelectedCodesEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCodes = _SelectedCodesEx.Value;
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
      if (part.HasValue(Name)) // 14.10.2021
      {
        string s = part.GetString(Name);
        if (s.Length == 0)
          _SelectedCodes = DataTools.EmptyStrings;
        else
          _SelectedCodes = s.Split(',');
      }
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
    #region Конструкторы

    /// <summary>
    /// Создает окно с текстом
    /// </summary>
    /// <param name="text">Текст сообщения. Не может быть пустой строкой</param>
    public InfoLabel(string text)
      :this(UITools.TextToLines(text))
    {
    }

    /// <summary>
    /// Создает окно с текстом
    /// </summary>
    /// <param name="lines">Строки сообщения. Должна быть хотя бы одна строка</param>
    public InfoLabel(string[] lines)
    {
      if (lines == null)
        throw new ArgumentNullException("lines");
      if(lines.Length<1)
        throw new ArgumentException("Текст должен быть задан", "lines");
      _Lines = lines;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текст сообщения.
    /// Символ "амперсанд" отображается как есть, а не используется для подчеркивания буквы.
    /// Задается в конструкторе.
    /// </summary>
    public string Text { get { return UITools.LinesToText(_Lines); } }

    /// <summary>
    /// Текст сообщения.
    /// Задается в конструкторе.
    /// </summary>
    public string[] Lines { get { return _Lines; } }
    private string[] _Lines;

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
    #region Конструктор

    /// <summary>
    /// Создает управляющий элемент
    /// </summary>
    public FolderBrowserTextBox()
    {
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выбранный каталог (вход и выход).
    /// В текущей реализации нет управляемого свойства PathEx.
    /// </summary>
    public AbsPath Path { get { return _Path; } set { _Path = value; } }
    private AbsPath _Path;

    private AbsPath _OldPath;


    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
    #region Конструктор

    /// <summary>
    /// Создает управляющий элемент
    /// </summary>
    public FileTextBox()
    {
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выбранный файл.
    /// Выбрать можно только один файл, множественный выбор не поддерживается.
    /// В текущей реализации нет управляемого свойства PathEx.
    /// </summary>
    public AbsPath Path { get { return _Path; } set { _Path = value; } }
    private AbsPath _Path;

    private AbsPath _OldPath;


    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
    /// Событие вызывается на вызывающей стороне для проверки корректности введенных значений при нажатии кнопки "ОК".
    /// Обработчик должен проверить значения, введенные пользователем и вызвать Control.SetError() для индикации ошибки.
    /// 
    /// Рекомендуется по возможности использовать списки Control.Validators для реализации проверок. 
    /// Такие проверки выполняются динамически при каждом изменении пользовательского ввода, а не только при нажатии кнопки "ОК".
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
