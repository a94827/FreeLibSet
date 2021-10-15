using AgeyevAV.Config;
using AgeyevAV.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

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
  /// Базовый класс для стандартных блоков диалогв
  /// </summary>
  [Serializable]
  public abstract class StandardDialog : RIItem
  {
    #region Свойства

    /// <summary>
    /// Заголовок окна.
    /// Значение по умолчанию зависить от конкруетного диалога.
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    #endregion

    #region Чтение и запись значений

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
  }

  #region Файлы и папки

  /// <summary>
  /// Диалог выбора каталога
  /// </summary>
  [Serializable]
  public class FolderBrowserDialog : StandardDialog
  {
    #region Свойства

    /// <summary>
    /// Выбранный каталог (вход и выход)
    /// </summary>
    public AbsPath Path { get { return _Path; } set { _Path = value; } }
    private AbsPath _Path;

    private AbsPath _OldPath;

    /// <summary>
    /// Текст описания внизу блока диалога.
    /// Дублирует свойство Title, т.к. заголовок не может быть задан для этого блока диалога
    /// </summary>
    public string Description { get { return base.Title; } set { base.Title = value; } }

    /// <summary>
    /// Должна ли отображаться кнопка "Создать папку"
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
  /// Базовый класс для OpenFileDialog и SaveFileDialog
  /// </summary>
  [Serializable]
  public abstract class FileDialog : StandardDialog
  {
    #region Свойства

    /// <summary>
    /// Выбранный файл
    /// </summary>
    public AbsPath Path { get { return _Path; } set { _Path = value; } }
    private AbsPath _Path;

    private AbsPath _OldPath;

    /// <summary>
    /// Расширение по умолчанию
    /// </summary>
    public string DefaultExt
    {
      get { return _DefaultExt; }
      set
      {
        CheckNotFixed();
        _DefaultExt = value;
      }
    }
    private string _DefaultExt;

    /// <summary>
    /// Фильтры (разделитель "|"). Например: "Текстовые файлы|*.txt|Все файлы|*.*".
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
  /// Диалог выбора файла для открытия
  /// </summary>
  [Serializable]
  public class OpenFileDialog : FileDialog
  {
  }

  /// <summary>
  /// Диалог выбора файла для записи
  /// </summary>
  [Serializable]
  public class SaveFileDialog : FileDialog
  {
  }

  #endregion

  #region Ввод значений

  /// <summary>
  /// Базовый класс для диалогов ввода единственного значения, например TextInputDialog
  /// </summary>
  [Serializable]
  public abstract class BaseInputDialog : StandardDialog
  {
    #region Свойства

    /// <summary>
    /// Текст подсказки.
    /// Значение по умолчанию зависит от конкретного диалога.
    /// </summary>
    public string Prompt
    {
      get { return _Prompt; }
      set
      {
        CheckNotFixed();
        _Prompt = value;
      }
    }
    private string _Prompt;

    #endregion
  }

  /// <summary>
  /// Диалог ввода строки текста.
  /// Есть возможность ввода пароля.
  /// Для ввода многострочного текста используйте MultiLineTextInputDialog.
  /// </summary>
  [Serializable]
  public class TextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public TextInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      Value = String.Empty;
      MaxLength = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// По умолчанию - пустая строка.
    /// </summary>
    public string Value { get { return _Value; } set { _Value = value; } }
    private string _Value;
    private string _OldValue;

    /// <summary>
    /// Максимальная длина текста.
    /// По умолчанию: 0 - длина текста ограничена 32767 символами (Int16.MaxValue)
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
    /// Если установлено в true, то поле предназначено для ввода пароля. Вводимые символы не отображаются.
    /// По умолчанию - false - обычный ввод строки.
    /// </summary>
    public bool IsPassword { get { return _IsPassword; } set { _IsPassword = value; } }
    private bool _IsPassword;

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
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
    /// Может ли поле быть пустым.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
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
        return _OldValue != Value;
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
      part.SetString("Value", Value);
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
      Value = part.GetString("Value");
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
      part.SetString(Name, Value);
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
        Value = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода произвольной строки текста с возможностью выбора из списка возможных значений.
  /// </summary>
  [Serializable]
  public class ComboTextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public ComboTextInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      Value = String.Empty;
      MaxLength = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// В отличие от TextBox, в блоке диалога свойство имеет имя "Value", а не "Text".
    /// По умолчанию - пустая строка.
    /// </summary>
    public string Value { get { return _Value; } set { _Value = value; } }
    private string _Value;
    private string _OldValue;

    /// <summary>
    /// Максимальная длина текста.
    /// По умолчанию: 0 - длина текста ограничена 32767 символами (Int16.MaxValue)
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
    /// Список строк, из которых можно выбрать значение.
    /// По умолчанию - null - список для выбора не задан.
    /// </summary>
    public string[] Items
    {
      get { return _Items; }
      set
      {
        CheckNotFixed();
        _Items = value;
      }
    }
    private string[] _Items;

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
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
    /// Может ли поле быть пустым.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
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
        return _OldValue != Value;
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
      part.SetString("Value", Value);
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
      Value = part.GetString("Value");
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
      part.SetString(Name, Value);
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
        Value = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода целого числа
  /// </summary>
  [Serializable]
  public class IntInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public IntInputDialog()
    {
      Title = "Ввод числа";
      Prompt = "Значение";
      NullableValue = null;
      MinValue = Int32.MinValue;
      MaxValue = Int32.MaxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        CheckNotFixed();
        _CanBeEmpty = value;
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// Используйте это свойство, если CanBeEmpty=false.
    /// Пустое значение трактуется как 0.
    /// </summary>
    public int Value
    {
      get { return NullableValue ?? 0; }
      set { NullableValue = value; }
    }

    /// <summary>
    /// Ввод и вывод: Редактируемое значение
    /// </summary>
    public int? NullableValue { get { return _NullableValue; } set { _NullableValue = value; } }
    private int? _NullableValue;
    private int? _OldNullableValue;

    /// <summary>
    /// Минимальное значение. По умолчанию: Int32.MinValue
    /// </summary>
    public int MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotFixed();
        _MinValue = value;
      }
    }
    private int _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию: Int32.MaxValue
    /// </summary>
    public int MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotFixed();
        _MaxValue = value;
      }
    }
    private int _MaxValue;

    /// <summary>
    /// Если свойство установлено в true, то значение можно выбирать с помощью стрелочек.
    /// По умолчанию - false - стрелочки не используются
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
        return _OldNullableValue != NullableValue;
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
      part.SetNullableInt("Value", NullableValue);
      _OldNullableValue = NullableValue;
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
      NullableValue = part.GetNullableInt("Value");
      _OldNullableValue = NullableValue;
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
      part.SetNullableInt(Name, NullableValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableValue = part.GetNullableInt(Name);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для SingleInputBox, DoubleInputBox и DecimalInputBox
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  [Serializable]
  public abstract class BaseFloatInputDialog<T> : BaseInputDialog
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public BaseFloatInputDialog()
    {
      Title = "Ввод числа";
      Prompt = "Значение";
      NullableValue = null;
      DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        CheckNotFixed();
        _CanBeEmpty = value;
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// Используйте это свойство, если CanBeEmpty=false.
    /// Пустое значение трактуется как 0.
    /// </summary>
    public T Value
    {
      get { return NullableValue ?? default(T); }
      set { NullableValue = value; }
    }


    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// </summary>
    public T? NullableValue { get { return _NullableValue; } set { _NullableValue = value; } }
    private T? _NullableValue;

    /// <summary>
    /// Используется при чтении/записи изменений
    /// </summary>
    protected T? OldNullableValue;


    /// <summary>
    /// Число десятичных знаков после запятой. По умолчанию: (-1) - число дейсятичных знаков не установлено
    /// </summary>
    public int DecimalPlaces
    {
      get { return _DecimalPlaces; }
      set
      {
        CheckNotFixed();
        _DecimalPlaces = value;
      }
    }
    private int _DecimalPlaces;

    /// <summary>
    /// Альтернативная установка свойства DecimalPlaces
    /// </summary>
    public string Format
    {
      get
      {
        return DataTools.DecimalPlacesToNumberFormat(DecimalPlaces);
      }
      set
      {
        DecimalPlaces = DataTools.DecimalPlacesFromNumberFormat(value);
      }
    }

    /// <summary>
    /// Минимальное значение. По умолчанию - минимально возможное значение для своего типа
    /// </summary>
    public T MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotFixed();
        _MinValue = value;
      }
    }
    private T _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию - максимально возможно значение для своего типа
    /// </summary>
    public T MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotFixed();
        _MaxValue = value;
      }
    }
    private T _MaxValue;

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
  /// Диалог ввода числа одинарной точности
  /// </summary>
  [Serializable]
  public class SingleInputDialog : BaseFloatInputDialog<float>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public SingleInputDialog()
    {
      MinValue = Single.MinValue;
      MaxValue = Single.MaxValue;
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
        return OldNullableValue != NullableValue;
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
      part.SetNullableSingle("Value", NullableValue);
      OldNullableValue = NullableValue;
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
      NullableValue = part.GetNullableSingle("Value");
      OldNullableValue = NullableValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableSingle(Name, NullableValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableValue = part.GetNullableSingle(Name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа двойной точности
  /// </summary>
  [Serializable]
  public class DoubleInputDialog : BaseFloatInputDialog<double>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DoubleInputDialog()
    {
      MinValue = Double.MinValue;
      MaxValue = Double.MaxValue;
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
        return OldNullableValue != NullableValue;
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
      part.SetNullableDouble("Value", NullableValue);
      OldNullableValue = NullableValue;
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
      NullableValue = part.GetNullableDouble("Value");
      OldNullableValue = NullableValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDouble(Name, NullableValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableValue = part.GetNullableDouble(Name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа типа Decimal
  /// </summary>
  [Serializable]
  public class DecimalInputDialog : BaseFloatInputDialog<decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DecimalInputDialog()
    {
      MinValue = Decimal.MinValue;
      MaxValue = Decimal.MaxValue;
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
        return OldNullableValue != NullableValue;
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
      part.SetNullableDecimal("Value", NullableValue);
      OldNullableValue = NullableValue;
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
      NullableValue = part.GetNullableDecimal("Value");
      OldNullableValue = NullableValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDecimal(Name, NullableValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableValue = part.GetNullableDecimal(Name);
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода текста с возможностью выбора из списка возможных значений
  /// </summary>
  [Serializable]
  public class DateInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DateInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      Value = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение
    /// </summary>
    public DateTime? Value { get { return _Value; } set { _Value = value; } }
    private DateTime? _Value;
    private DateTime? _OldValue;


    /// <summary>
    /// Минимальное значение. По умолчанию ограничение не задано
    /// </summary>
    public DateTime? MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotFixed();
        _MinValue = value;
      }
    }
    private DateTime? _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию ограничение не задано
    /// </summary>
    public DateTime? MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotFixed();
        _MaxValue = value;
      }
    }
    private DateTime? _MaxValue;

    /// <summary>
    /// Если свойство установлено в true, то в диалоге будет не поле ввода даты, а календарик.
    /// Установка свойства несовместима с CanBeEmpty=true
    /// </summary>
    public bool UseCalendar
    {
      get { return _UseCalendar; }
      set
      {
        CheckNotFixed();
        _UseCalendar = value;
      }
    }
    private bool _UseCalendar;

    /// <summary>
    /// Может ли поле быть пустым.
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
    /// Может ли поле быть пустым.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
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
        return _OldValue != Value;
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
  /// Диалог ввода многострочного текста.
  /// </summary>
  [Serializable]
  public class MultiLineTextInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public MultiLineTextInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Текст"; // 17.02.2021
      _Lines = DataTools.EmptyStrings;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// Разделителем строк является Environment.NewLine.
    /// При передаче между компьютерами с разными операционными системами разделитель заменяется.
    /// </summary>
    public string Value
    {
      get
      {
        if (_Lines.Length == 0)
          return String.Empty;
        else
          return String.Join(Environment.NewLine, _Lines);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          _Lines = DataTools.EmptyStrings;
        else
          _Lines = value.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
      }
    }

    /// <summary>
    /// Многострочный текст.
    /// Дублирует свойство Value как массив строк.
    /// </summary>
    public string[] Lines
    {
      get { return _Lines; }
      set
      {
        if (value == null)
          _Lines = DataTools.EmptyStrings;
        else if (value.Length == 0)
          _Lines = DataTools.EmptyStrings;
        else
          _Lines = value;
      }
    }
    private string[] _Lines;
    private string[] _OldLines;


    /// <summary>
    /// Если true, то форма будет предназначена только для просмотра текста, а не для редактирования.
    /// По умолчанию - false - текст можно редактировать
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        CheckNotFixed();
        _ReadOnly = value;
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Если true, то форма будет выведена на весь экран
    /// По умолчанию - false - форма имеет размер по умолчанию
    /// </summary>
    public bool Maximized
    {
      get { return _Maximized; }
      set
      {
        CheckNotFixed();
        _Maximized = value;
      }
    }
    private bool _Maximized;

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
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
    /// Может ли поле быть пустым.
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
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
        return !DataTools.AreArraysEqual<string>(_OldLines, Lines);
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
      part.SetString("Value", Value);
      _OldLines = Lines;
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
      Value = part.GetString("Value");
      _OldLines = Lines;
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
      part.SetString(Name, Value);
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
        Value = part.GetString(Name);
    }

    #endregion
  }

  #endregion

  #region Ввод диапазонов

  /// <summary>
  /// Диалог ввода диапазона целых чисел
  /// </summary>
  [Serializable]
  public class IntRangeDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public IntRangeDialog()
    {
      Title = "Ввод диапазона чисел";
      Prompt = "Диапазон";
      NullableFirstValue = null;
      NullableLastValue = null;
      CanBeEmpty = false;
      MinValue = Int32.MinValue;
      MaxValue = Int32.MaxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        CheckNotFixed();
        _CanBeEmpty = value;
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Вход и выход - первое редактируемое значение с поддержкой null
    /// </summary>
    public int? NullableFirstValue { get { return _NullableFirstValue; } set { _NullableFirstValue = value; } }
    private int? _NullableFirstValue;
    private int? _OldNullableFirstValue;

    /// <summary>
    /// Вход и выход - второе редактируемое значение с поддержкой null
    /// </summary>
    public int? NullableLastValue { get { return _NullableLastValue; } set { _NullableLastValue = value; } }
    private int? _NullableLastValue;
    private int? _OldNullableLastValue;

    /// <summary>
    /// Вход и выход: первое редактируемое значение без поддержки null
    /// </summary>
    public int FirstValue
    {
      get { return NullableFirstValue ?? 0; }
      set { NullableFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: второе редактируемое значение без поддержки null
    /// </summary>
    public int LastValue
    {
      get { return NullableLastValue ?? 0; }
      set { NullableLastValue = value; }
    }

    /// <summary>
    /// Минимальное значение. По умолчанию: Int32.MinValue
    /// </summary>
    public int MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotFixed();
        _MinValue = value;
      }
    }
    private int _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию: Int32.MaxValue
    /// </summary>
    public int MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotFixed();
        _MaxValue = value;
      }
    }
    private int _MaxValue;

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
        return _OldNullableFirstValue != NullableFirstValue || _OldNullableLastValue != NullableLastValue;
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
      part.SetNullableInt("FirstValue", NullableFirstValue);
      part.SetNullableInt("LastValue", NullableLastValue);
      _OldNullableFirstValue = NullableFirstValue;
      _OldNullableLastValue = NullableLastValue;
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
      NullableFirstValue = part.GetNullableInt("FirstValue");
      NullableLastValue = part.GetNullableInt("LastValue");
      _OldNullableFirstValue = NullableFirstValue;
      _OldNullableLastValue = NullableLastValue;
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
      part.SetNullableInt(Name + "-FirstValue", NullableFirstValue);
      part.SetNullableInt(Name + "-LastValue", NullableLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableFirstValue = part.GetNullableInt(Name + "-FirstValue");
      NullableLastValue = part.GetNullableInt(Name + "-LastValue");
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для SingleRangeBox, DoubleRangeBox и DecimalRangeBox
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  [Serializable]
  public abstract class BaseFloatRangeDialog<T> : BaseInputDialog
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public BaseFloatRangeDialog()
    {
      Title = "Ввод диапазона чисел";
      Prompt = "Диапазон";
      NullableFirstValue = null;
      NullableLastValue = null;
      CanBeEmpty = false;
      _DecimalPlaces = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Можно ли вводить пустое значение.
    /// По умолчанию - false
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        CheckNotFixed();
        _CanBeEmpty = value;
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Вход и выход: первое редактируемое значение с поддержкой null
    /// </summary>
    public T? NullableFirstValue { get { return _NullableFirstValue; } set { _NullableFirstValue = value; } }
    private T? _NullableFirstValue;

    /// <summary>
    /// Используется при чтении и записи значений
    /// </summary>
    protected T? OldNullableFirstValue;

    /// <summary>
    /// Вход и выход: второе редактируемое значение с поддержкой null
    /// </summary>
    public T? NullableLastValue { get { return _NullableLastValue; } set { _NullableLastValue = value; } }
    private T? _NullableLastValue;

    /// <summary>
    /// Используется при чтении и записи значений
    /// </summary>
    protected T? OldNullableLastValue;

    /// <summary>
    /// Вход и выход: первое редактируемое значение без поддержки null
    /// </summary>
    public T FirstValue
    {
      get { return NullableFirstValue ?? default(T); }
      set { NullableFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: второе редактируемое значение без поддержки null
    /// </summary>
    public T LastValue
    {
      get { return NullableLastValue ?? default(T); }
      set { NullableLastValue = value; }
    }


    /// <summary>
    /// Число десятичных знаков после запятой. По умолчанию: (-1) - число десятичных знаков не установлено
    /// </summary>
    public int DecimalPlaces
    {
      get { return _DecimalPlaces; }
      set
      {
        CheckNotFixed();
        _DecimalPlaces = value;
      }
    }
    private int _DecimalPlaces;

    /// <summary>
    /// Альтернативная установка свойства DecimalPlaces
    /// </summary>
    public string Format
    {
      get
      {
        if (DecimalPlaces < 0)
          return String.Empty;
        else if (DecimalPlaces == 0)
          return "0";
        else
          return "0." + new string('0', DecimalPlaces);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          DecimalPlaces = -1;
          return;
        }

        int p = value.IndexOf('.');
        if (p < 0)
        {
          DecimalPlaces = 0;
          return;
        }

        DecimalPlaces = value.Length - p - 1;
      }
    }

    /// <summary>
    /// Минимальное значение. По умолчанию - минимально возможное значение для своего типа
    /// </summary>
    public T MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotFixed();
        _MinValue = value;
      }
    }
    private T _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию - максимально возможно значение для своего типа
    /// </summary>
    public T MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotFixed();
        _MaxValue = value;
      }
    }
    private T _MaxValue;

    #endregion

    #region Методы

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

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа одинарной точности
  /// </summary>
  [Serializable]
  public class SingleRangeDialog : BaseFloatRangeDialog<float>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public SingleRangeDialog()
    {
      MinValue = Single.MinValue;
      MaxValue = Single.MaxValue;
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
        return OldNullableFirstValue != NullableFirstValue || OldNullableLastValue != NullableLastValue;
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
      part.SetNullableSingle("FirstValue", NullableFirstValue);
      part.SetNullableSingle("LastValue", NullableLastValue);
      OldNullableFirstValue = NullableFirstValue;
      OldNullableLastValue = NullableLastValue;
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
      NullableFirstValue = part.GetNullableSingle("FirstValue");
      NullableLastValue = part.GetNullableSingle("LastValue");
      OldNullableFirstValue = NullableFirstValue;
      OldNullableLastValue = NullableLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableSingle(Name + "-FirstValue", NullableFirstValue);
      part.SetNullableSingle(Name + "-LastValue", NullableLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableFirstValue = part.GetNullableSingle(Name + "-FirstValue");
      NullableLastValue = part.GetNullableSingle(Name + "-LastValue");
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа двойной точности
  /// </summary>
  [Serializable]
  public class DoubleRangeDialog : BaseFloatRangeDialog<double>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DoubleRangeDialog()
    {
      MinValue = Double.MinValue;
      MaxValue = Double.MaxValue;
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
        return OldNullableFirstValue != NullableFirstValue || OldNullableLastValue != NullableLastValue;
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
      part.SetNullableDouble("FirstValue", NullableFirstValue);
      part.SetNullableDouble("LastValue", NullableLastValue);
      OldNullableFirstValue = NullableFirstValue;
      OldNullableLastValue = NullableLastValue;
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
      NullableFirstValue = part.GetNullableDouble("FirstValue");
      NullableLastValue = part.GetNullableDouble("LastValue");
      OldNullableFirstValue = NullableFirstValue;
      OldNullableLastValue = NullableLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDouble(Name + "-FirstValue", NullableFirstValue);
      part.SetNullableDouble(Name + "-LastValue", NullableLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableFirstValue = part.GetNullableDouble(Name + "-FirstValue");
      NullableLastValue = part.GetNullableDouble(Name + "-LastValue");
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа decimal
  /// </summary>
  [Serializable]
  public class DecimalRangeDialog : BaseFloatRangeDialog<decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DecimalRangeDialog()
    {
      MinValue = Decimal.MinValue;
      MaxValue = Decimal.MaxValue;
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
        return OldNullableFirstValue != NullableFirstValue || OldNullableLastValue != NullableLastValue;
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
      part.SetNullableDecimal("FirstValue", NullableFirstValue);
      part.SetNullableDecimal("LastValue", NullableLastValue);
      OldNullableFirstValue = NullableFirstValue;
      OldNullableLastValue = NullableLastValue;
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
      NullableFirstValue = part.GetNullableDecimal("FirstValue");
      NullableLastValue = part.GetNullableDecimal("LastValue");
      OldNullableFirstValue = NullableFirstValue;
      OldNullableLastValue = NullableLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDecimal(Name + "-FirstValue", NullableFirstValue);
      part.SetNullableDecimal(Name + "-LastValue", NullableLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NullableFirstValue = part.GetNullableDecimal(Name + "-FirstValue");
      NullableLastValue = part.GetNullableDecimal(Name + "-LastValue");
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода диапазона дат
  /// </summary>
  [Serializable]
  public class DateRangeDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DateRangeDialog()
    {
      Title = "Ввод диапазона дат";
      Prompt = "Диапазон";
      FirstValue = null;
      LastValue = null;
      CanBeEmpty = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Могут ли поля быть пустыми.
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
    /// Могут ли поля быть пустыми.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != CanBeEmptyMode.Error; }
      set { CanBeEmptyMode = value ? CanBeEmptyMode.Ok : CanBeEmptyMode.Error; }
    }


    /// <summary>
    /// Вход и выход - первое редактируемое значение с поддержкой null
    /// </summary>
    public DateTime? FirstValue { get { return _FirstValue; } set { _FirstValue = value; } }
    private DateTime? _FirstValue;
    private DateTime? _OldFirstValue;

    /// <summary>
    /// Вход и выход - второе редактируемое значение с поддержкой null
    /// </summary>
    public DateTime? LastValue { get { return _LastValue; } set { _LastValue = value; } }
    private DateTime? _LastValue;
    private DateTime? _OldLastValue;

    /// <summary>
    /// Минимальное значение. По умолчанию: null
    /// </summary>
    public DateTime? MinValue
    {
      get { return _MinValue; }
      set
      {
        CheckNotFixed();
        _MinValue = value;
      }
    }
    private DateTime? _MinValue;

    /// <summary>
    /// Максимальное значение. По умолчанию: null
    /// </summary>
    public DateTime? MaxValue
    {
      get { return _MaxValue; }
      set
      {
        CheckNotFixed();
        _MaxValue = value;
      }
    }
    private DateTime? _MaxValue;

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
        return _OldFirstValue != FirstValue || _OldLastValue != LastValue;
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
      part.SetNullableDate("FirstValue", FirstValue);
      part.SetNullableDate("LastValue", LastValue);
      _OldFirstValue = FirstValue;
      _OldLastValue = LastValue;
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
      FirstValue = part.GetNullableDate("FirstValue");
      LastValue = part.GetNullableDate("LastValue");
      _OldFirstValue = FirstValue;
      _OldLastValue = LastValue;
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
      part.SetNullableDate(Name + "-FirstValue", FirstValue);
      part.SetNullableDate(Name + "-LastValue", LastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      FirstValue = part.GetNullableDate(Name + "-FirstValue");
      LastValue = part.GetNullableDate(Name + "-LastValue");
    }

    #endregion
  }

  #endregion

  #region Выбор из списка

  #region Перечисление ListSelectDialogClipboardMode

  /// <summary>
  /// Режимы использования буфера обмена в диалоге ListSelectDialog
  /// </summary>
  [Serializable]
  public enum ListSelectDialogClipboardMode
  {
    /// <summary>
    /// Буфер обмена не используется
    /// </summary>
    None = 0,

    /// <summary>
    /// Если MultiSelect=true, в буфер обмена копируются отмеченные флажками элементы, разделенные запятыми, в виде одной строки текста.
    /// Дополнительные пробелы не добавляются. В режиме MultiSelect=false копируется текущий элемент.
    /// Дополнительный столбец SubItems не копируется, даже если он есть.
    /// Режим можно использовать, только если в списке ListSelectDialog.Items гарантированно нет запятых.
    /// </summary>
    CommaCodes = 1,
  }

  #endregion

  /// <summary>
  /// Диалог выбора одного или нескольких значений из списка.
  /// </summary>
  [Serializable]
  public class ListSelectDialog : StandardDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог для выбора одного элемента из списка
    /// </summary>
    /// <param name="items">Список элементов для выбора. Не может быть пустым.</param>
    public ListSelectDialog(string[] items)
      : this(items, false)
    {
    }

    /// <summary>
    /// Создает диалог для выбора одного или нескольких элементов из списка.
    /// </summary>
    /// <param name="items">Список элементов для выбора. Не может быть пустым.</param>
    /// <param name="multiSelect">True, если разрешается выбор нескольких элементов</param>
    public ListSelectDialog(string[] items, bool multiSelect)
    {
      CheckNotEmptyStringArray(items, true);
      _Items = items;

      if (items.Length == 0)
      {
        // 25.10.2019
        _SelectedIndex = -1;
        _OldSelectedIndex = -1;
      }
      else
      {
        _SelectedIndex = 0;
        _OldSelectedIndex = 0;
      }

      if (multiSelect)
      {
        _Selections = new bool[items.Length];
        _OldSelections = new bool[items.Length];
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список для выбора. Задается в конструкторе
    /// Строки могут содержать символ "амперсанд" для подчеркивания буквы.
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;

    /// <summary>
    /// True, если разрешено выбирать несколько позиций. Задается в конструкторе.
    /// </summary>
    public bool MultiSelect { get { return _Selections != null; } }

    /// <summary>
    /// Вход и выход: Текущее выбранное значение.
    /// По умолчанию свойство имеет значение 0 или (-1), если Items.Length=0.
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set
      {
        if (value < (-1) || value > Items.Length)
          throw new ArgumentOutOfRangeException();
        _SelectedIndex = value;
      }
    }
    private int _SelectedIndex;
    private int _OldSelectedIndex;


    /// <summary>
    /// Флажки выбора в режиме MultiSelect.
    /// Если MultiSelect=false, то свойство возвращает null.
    /// </summary>
    public bool[] Selections
    {
      get { return _Selections; }
      set
      {
        if (!MultiSelect)
          throw new InvalidOperationException("Список не предназначен для множественного выбора");
        if (value == null)
          throw new ArgumentNullException();
        value.CopyTo(_Selections, 0);
      }
    }
    private bool[] _Selections;
    private bool[] _OldSelections;


    /// <summary>
    /// Можно ли не выбирать ни одной позиции.
    /// По умолчанию - false - выбор является обязательным.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        CheckNotFixed();
        _CanBeEmpty = value;
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Заголовок над списком
    /// </summary>
    public string ListTitle
    {
      get { return _ListTitle; }
      set
      {
        CheckNotFixed();
        _ListTitle = value;
      }
    }
    private string _ListTitle;

    /// <summary>
    /// В режиме MultiSelect возвращает true, если в Selected установлены все флажки
    /// </summary>
    public bool AreAllSelected
    {
      get
      {
        if (_Selections == null)
          return false;
        for (int i = 0; i < _Selections.Length; i++)
        {
          if (!_Selections[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// В режиме MultiSelect возвращает true, если в Selected сброшены все флажки
    /// </summary>
    public bool AreAllUnselected
    {
      get
      {
        if (_Selections == null)
          return false;
        for (int i = 0; i < _Selections.Length; i++)
        {
          if (_Selections[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Индексы выбранных строк.
    /// Если MultiSelect=false значение содержит один или ноль элементов.
    /// Если в режиме MultiSelect=false устанавливается значение с количеством элементов в массиве, большим 1, 
    /// то используется первый элемент переданного массива, а остальные элементы отбрасываются. Если массив пустой, то устанавливается SelectedIndex=-1.
    /// </summary>
    public int[] SelectedIndices
    {
      get
      {
        if (MultiSelect)
        {
          List<int> lst = new List<int>();
          for (int i = 0; i < _Selections.Length; i++)
          {
            if (_Selections[i])
              lst.Add(i);
          }
          return lst.ToArray();
        }
        else
        {
          if (SelectedIndex >= 0)
            return new int[1] { SelectedIndex };
          else
            return DataTools.EmptyInts;
        }
      }
      set
      {
        if (MultiSelect)
        {
          DataTools.FillArray<bool>(_Selections, false);
          if (value != null)
          {
            for (int i = 0; i < value.Length; i++)
            {
              if (value[i] < 0 || value[i] >= _Selections.Length)
                throw new ArgumentOutOfRangeException();
              _Selections[value[i]] = true;
            }
          }
        }
        else
        {
          if (value == null)
            SelectedIndex = -1;
          else if (value.Length == 0)
            SelectedIndex = -1;
          else /*if (value.Length == 1) ограничение убрано 25.10.2019 */
          {
            if (value[0] < 0 || value[0] >= _Items.Length)
              throw new ArgumentOutOfRangeException();
            SelectedIndex = value[0];
          }
        }
      }
    }

    /// <summary>
    /// Коды выбраны строк.
    /// Если MultiSelect=false значение содержит один или ноль элементов.
    /// Свойство действительно только при установленном свойстве Codes.
    /// Если коды не используются, свойство возвращает null.
    /// Если в режиме MultiSelect=false устанавливается значение с количеством элементов в массиве, большим 1, 
    /// то используется первый элемент переданного массива, а остальные элементы отбрасываются. Если массив пустой, то устанавливается SelectedIndex=-1.
    /// </summary>
    public string[] SelectedCodes
    {
      get
      {
        if (Codes == null)
          return null;

        if (MultiSelect)
        {
          List<string> lst = new List<string>();
          for (int i = 0; i < _Selections.Length; i++)
          {
            if (_Selections[i])
              lst.Add(Codes[i]);
          }
          return lst.ToArray();
        }
        else
        {
          if (SelectedIndex >= 0)
            return new string[1] { Codes[SelectedIndex] };
          else
            return DataTools.EmptyStrings;
        }
      }
      set
      {
        if (Codes == null)
          throw new InvalidOperationException("Свойство Codes не установено");

        if (MultiSelect)
        {

          DataTools.FillArray<bool>(_Selections, false);
          if (value != null)
          {
            ArrayIndexer<string> ai = new ArrayIndexer<string>(value);

            for (int i = 0; i < Codes.Length; i++)
            {
              if (ai.Contains(Codes[i]))
                _Selections[i] = true;
            }
          }
        }
        else
        {
          if (value == null)
            SelectedIndex = -1;
          else if (value.Length == 0)
            SelectedIndex = -1;
          else
            SelectedCode = value[0];
        }
      }
    }

    /// <summary>
    /// Список строк для второго столбца.
    /// Если свойство равно null (по умолчанию), то второго столбца нет
    /// </summary>
    public string[] SubItems
    {
      get
      {
        return _SubItems;
      }
      set
      {
        CheckNotFixed();
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw new ArgumentException("Неправильная длина массива");
        }
        _SubItems = value;
      }
    }
    private string[] _SubItems;

    /// <summary>
    /// Коды для элементов.
    /// Если свойство установлено, то для сохранения значения между сеансами работы используется код, а не индекс.
    /// По умолчанию свойство содержит значение null
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

    /// <summary>
    /// Текущая выбранная позиция как код.
    /// Должно быть установлен свойство Codes.
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
    /// Можно ли использовать команды копирования и вставки из буфера обмена.
    /// По умолчанию - None - копирование недоступно.
    /// </summary>
    public ListSelectDialogClipboardMode ClipboardMode
    {
      get { return _ClipboardMode; }
      set
      {
        CheckNotFixed();
        _ClipboardMode = value;
      }
    }
    private ListSelectDialogClipboardMode _ClipboardMode;

    #endregion

    #region Методы

    /// <summary>
    /// Задать выбранные элементы с помощью списка строк.
    /// Для строк Items, которые будут найдены в переданном аргументе, будет 
    /// установлена отметка. Для остальных строк отметка будет снята
    /// </summary>
    /// <param name="selectedItems">Значения, которые нужно выбрать</param>
    public void SetSelectedItems(string[] selectedItems)
    {
      if (!MultiSelect)
        throw new InvalidOperationException("Свойство MultiSelect не установлено");

      Array.Clear(_Selections, 0, _Selections.Length);

      if (selectedItems != null)
      {
        for (int i = 0; i < selectedItems.Length; i++)
        {
          int p = Array.IndexOf<String>(_Items, selectedItems[i]);
          if (p >= 0)
            Selections[p] = true;
        }
      }
    }

    /// <summary>
    /// Получить список отмеченных строк из массива Items
    /// </summary>
    /// <returns></returns>
    public string[] GetSelectedItems()
    {
      if (!MultiSelect)
      {
        if (SelectedIndex >= 0)
          return new string[1] { Items[SelectedIndex] };
        return DataTools.EmptyStrings;
      }

      // Придется делать 2 прохода
      int i;
      int n = 0;
      for (i = 0; i < Selections.Length; i++)
      {
        if (Selections[i])
          n++;
      }
      string[] a = new string[n];
      n = 0;
      for (i = 0; i < Selections.Length; i++)
      {
        if (Selections[i])
        {
          a[n] = Items[i];
          n++;
        }
      }
      return a;
    }

    /// <summary>
    /// Установить отметки для всех позиций
    /// </summary>
    public void SelectAll()
    {
      if (!MultiSelect)
        throw new InvalidOperationException("Свойство MultiSelect не установлено");

      for (int i = 0; i < Selections.Length; i++)
        Selections[i] = true;
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
        if (_OldSelectedIndex != SelectedIndex)
          return true;
        if (MultiSelect)
        {
          for (int i = 0; i < _Selections.Length; i++)
          {
            if (_Selections[i] != _OldSelections[i])
              return true;
          }
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
      base.WriteChanges(part);
      part.SetInt("SelectedIndex", SelectedIndex);
      _OldSelectedIndex = SelectedIndex;
      if (MultiSelect)
      {
        int[] a1 = SelectedIndices;
        string[] a2 = new string[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = a1[i].ToString();
        part.SetString("Selections", String.Join(",", a2));
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
      base.ReadChanges(part);
      SelectedIndex = part.GetInt("SelectedIndex");
      _OldSelectedIndex = SelectedIndex;
      if (MultiSelect)
      {
        string s = part.GetString("Selections");
        string[] a1;
        if (String.IsNullOrEmpty(s))
          a1 = DataTools.EmptyStrings;
        else
          a1 = s.Split(',');
        int[] a2 = new int[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = int.Parse(a1[i]);
        SelectedIndices = a2;

        Array.Copy(_Selections, _OldSelections, _Selections.Length);
      }
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
      if (MultiSelect)
      {
        // добавлено 25.10.2019

        // Записываем коды или индексы в одну строку через запятую
        // При чтении будем учитывать совместимость для MultiSelect=true и false.

        if (Codes == null)
          part.SetIntCommaString(Name, SelectedIndices);
        else
        {
          string[] aSelCodes = this.SelectedCodes;
#if DEBUG
          if (aSelCodes == null)
            throw new BugException("SelectedCodes==null");
#endif
          part.SetString(Name, String.Join(",", aSelCodes));
        }
      }
      else
      {
        if (Codes == null)
          part.SetInt(Name, SelectedIndex);
        else
          part.SetString(Name, SelectedCode);
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
      if (part.HasValue(Name))
      {
        // Можно в любом режиме использовать свойства SelectedIndices и SelectedCodes, так как работают и при MultiSelect=false.

        if (Codes == null)
          SelectedIndices = part.GetIntCommaString(Name);
        else
          SelectedCodes = part.GetString(Name).Split(',');

        if (MultiSelect)
        {
          // Устанавливаем текущую позицию на первый выбранный элемент
          _SelectedIndex = -1;
          for (int i = 0; i < _Selections.Length; i++)
          {
            if (_Selections[i])
            {
              _SelectedIndex = i;
              break;
            }
          }
          if (_SelectedIndex < 0 && _Items.Length > 0)
            _SelectedIndex = 0;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Диалог выбора значения с помощью группы радиокнопок
  /// </summary>
  [Serializable]
  public class RadioSelectDialog : StandardDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    /// <param name="items">Список надписей для радиокнопок. Список не может быть пустым.</param>
    public RadioSelectDialog(string[] items)
    {
      CheckNotEmptyStringArray(items, false);
      _Items = items;
      _SelectedIndex = 0;
      _OldSelectedIndex = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список для выбора. Задается в конструкторе
    /// Строки могут содержать символ "амперсанд" для подчеркивания буквы
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;


    /// <summary>
    /// Вход и выход: Текущее выбранное значение.
    /// По умолчанию свойство имеет значение 0
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set
      {
        if (value < 0 || value > Items.Length)
          throw new ArgumentOutOfRangeException();
        _SelectedIndex = value;
      }
    }
    private int _SelectedIndex;
    private int _OldSelectedIndex;


    /// <summary>
    /// Заголовок над кнопками
    /// </summary>
    public string GroupTitle
    {
      get { return _GroupTitle; }
      set
      {
        CheckNotFixed();
        _GroupTitle = value;
      }
    }
    private string _GroupTitle;

    /// <summary>
    /// Коды для элементов.
    /// Если свойство установлено, то для сохранения значения между сеансами работы используется код, а не индекс.
    /// По умолчанию свойство содержит значение null.
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

    /// <summary>
    /// Текущая выбранная позиция как код.
    /// Должно быть установлен свойство Codes
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
        return _OldSelectedIndex != SelectedIndex;
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

  #endregion

  #region Ввод табличных данных

  /// <summary>
  /// Диалог для ввода табличных данных.
  /// Данные хранятся в виде таблицы DataTable. См описание свойства Table. 
  /// Для столбцов таблицы можно задавать дополнительные параметры, используя коллекцию Columns. 
  /// Можно работать с фиксированным числом строк, или разрешить пользователю добавлять и удалять строки (свойство FixedRows).
  /// Свойство ReadOnly позволяет показать таблицу без возможности редактирования.
  /// </summary>
  [Serializable]
  public class InputGridDataDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует начальные значения диалога
    /// </summary>
    public InputGridDataDialog()
    {
      Title = "Таблица";

      _DS = new DataSet();
      _DS.Tables.Add("Table");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - редактируемая таблица данных.
    /// В таблицу должны быть добавлены столбцы перед показом диалога.
    /// Могут использоваться столбцы для просмотра, если установлено свойство DataColumn.Expression.
    /// Дополнительные параметры для добавленных столбцов, например, формат, задавайте с помощью методов коллекции Columns.
    /// 
    /// Если FixedRows=true, то в таблицу следует добавить строки, иначе пользователь не сможет ничего ввести.
    /// 
    /// После закрытия блока диалога свойство Table должно быть прочитано заново, так как оно содержит ссылку на новую таблицу.
    /// По умолчанию - пустая таблица.
    /// Если значение свойства устанавливается, то создается КОПИЯ таблицы.
    /// </summary>
    public DataTable Table
    {
      get { return _DS.Tables[0]; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        DataTable tbl = value.Copy();
        tbl.TableName = "Table";
        _DS.Tables.RemoveAt(0);
        _DS.Tables.Add(tbl);

        _Columns = null;
      }
    }
    private DataSet _DS;

    /// <summary>
    /// Фиксированные строки.
    /// Если false (по умолчанию), то пользователь может добавлять и удалять строки в таблицу.
    /// Если true, то пользователь может только редактировать существующие строки в таблице.
    /// </summary>
    public bool FixedRows
    {
      get { return _FixedRows; }
      set
      {
        CheckNotFixed();
        _FixedRows = value;
      }
    }
    private bool _FixedRows;

    /// <summary>
    /// Если true, то диалог будет предназначен только для просмотра, но не для редактирования данных.
    /// По умолчанию - false.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        CheckNotFixed();
        _ReadOnly = value;
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Информационный текст, выводимый в нижней части диалога
    /// </summary>
    public string InfoText
    {
      get { return _InfoText; }
      set
      {
        CheckNotFixed();
        _InfoText = value;
      }
    }
    private string _InfoText;

    /// <summary>
    /// Объект для установки расширенных свойств столбцов (форматирования, размеров, выравнивания текста).
    /// Сама коллекция не хранит данные, для этого используются объекты DataColumn.ExtendedProperties.
    /// </summary>
    public InputGridDataColumnProperties Columns
    {
      get
      {
        if (_Columns == null)
          _Columns = new InputGridDataColumnProperties(Table, IsFixed);
        return _Columns;
      }
    }
    [NonSerialized]
    private InputGridDataColumnProperties _Columns;

    #endregion

    #region Чтение и запись значений

    /// <summary>
    /// Пересоздает объект Columns, предназначенный только для чтения свойств
    /// </summary>
    protected override void OnSetFixed()
    {
      base.OnSetFixed();
      _Columns = null; // требуется пересоздание объекта с ReadOnly=true.
    }

    /// <summary>
    /// Возвращает true, т.к. таблица DataSource передается всегда.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public override bool HasChanges { get { return true; } }

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

      using (System.IO.StringWriter sw = new System.IO.StringWriter())
      {
        _DS.WriteXml(sw, XmlWriteMode.WriteSchema);
        sw.Flush();
        string s = sw.ToString();
        part.SetString("Table", s);
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
      base.ReadChanges(part);

      string s = part.GetString("Table");
      using (System.IO.StringReader sr = new System.IO.StringReader(s))
      {
        DataSet ds = new DataSet();
        ds.ReadXml(sr, XmlReadMode.ReadSchema);
        _DS = ds;
      }
    }

    /// <summary>
    /// Возвращает false, т.к. информация не может сохраняться между сеансами работы
    /// </summary>
    /// <param name="сfgType"></param>
    /// <returns></returns>
    protected override bool OnSupportsCfgType(RIValueCfgType сfgType)
    {
      return false;
    }

    #endregion
  }

  /// <summary>
  /// Класс для установки свойств DataColumn.ExtendedProperties для табличного просмотра InputGridDataDialog
  /// </summary>
  public class InputGridDataColumnProperties : IReadOnlyObject
  {
    // Этот класс не сериализуется

    #region Конструкторы

    /// <summary>
    /// Создает объект, присоединенный к таблице, разрешающий изменение свойста
    /// </summary>
    /// <param name="table">Таблица, свойствами столбцов которой нужно управлять</param>
    public InputGridDataColumnProperties(DataTable table)
      : this(table, false)
    {
    }

    /// <summary>
    /// Создает объект, присоединенный к таблице
    /// </summary>
    /// <param name="table">Таблица, свойствами столбцов которой нужно управлять</param>
    /// <param name="isReadOnly">Если true, то объект позволит только читать свойства, но не устанавливать их</param>
    public InputGridDataColumnProperties(DataTable table, bool isReadOnly)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      _Table = table;
      _IsReadOnly = isReadOnly;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Таблица для управления. Задается в конструкторе
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;

    #endregion

    #region Значения для столбцов

    // "Format" - задает формат числового столбца или даты/времени.
    // "TextWidth" - задает ширину столбца в текстовых единицах.
    // "MinTextWidth" - задает минимальную ширину столбца в текстовых единицах.
    // "FillWeight" - задает относительную ширину столбца, если столбец должен заполнять просмотр по ширине.
    // "Align" - задает горизонтальное выравнивание (строковое значение "Left", "Center" или "Right").

    #region Align

    /// <summary>
    /// Установить горизонтальное выравнивание для столбца.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetAlign(string columnName, HorizontalAlignment value)
    {
      CheckNotReadOnly();
      InternalGetColumn(columnName).ExtendedProperties["Align"] = value.ToString();
    }

    /// <summary>
    /// Установить горизонтальное выравнивание для столбца.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetAlign(DataColumn column, HorizontalAlignment value)
    {
      CheckNotReadOnly();
      InternalGetColumn(column).ExtendedProperties["Align"] = value.ToString();
    }

    /// <summary>
    /// Получить горизонтальное выравнивание для столбца.
    /// Если значение не было установлено в явном виде, будет возвращено значение по умолчанию.
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.</param>
    /// <returns>Установленное значение</returns>
    public HorizontalAlignment GetAlign(string columnName)
    {
      return DoGetAlign(InternalGetColumn(columnName));
    }

    /// <summary>
    /// Получить горизонтальное выравнивание для столбца.
    /// Если значение не было установлено в явном виде, будет возвращено значение по умолчанию.
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null или пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <returns>Установленное значение</returns>
    public HorizontalAlignment GetAlign(DataColumn column)
    {
      return DoGetAlign(InternalGetColumn(column));
    }

    private HorizontalAlignment DoGetAlign(DataColumn column)
    {
      string s = DataTools.GetString(column.ExtendedProperties["Align"]);
      if (String.IsNullOrEmpty(s))
      {
        if (DataTools.IsNumericType(column.DataType))
          return HorizontalAlignment.Right;
        if (column.DataType == typeof(DateTime) || column.DataType == typeof(bool))
          return HorizontalAlignment.Center;
        else
          return HorizontalAlignment.Left;
      }
      else
        return StdConvert.ToEnum<HorizontalAlignment>(s);
    }

    #endregion

    #region Format

    /// <summary>
    /// Установить формат для числового столбца или столбца даты/времени.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetFormat(string columnName, string value)
    {
      CheckNotReadOnly();
      InternalGetColumn(columnName).ExtendedProperties["Format"] = value;
    }

    /// <summary>
    /// Установить формат для числового столбца или столбца даты/времени.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetFormat(DataColumn column, string value)
    {
      CheckNotReadOnly();
      InternalGetColumn(column).ExtendedProperties["Format"] = value;
    }

    /// <summary>
    /// Получить формат для числового столбца или столбца даты/времени.
    /// Если значение не было установлено в явном виде, возвращается пустая строка.
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.</param>
    /// <returns>Установленное значение</returns>
    public string GetFormat(string columnName)
    {
      return DataTools.GetString(InternalGetColumn(columnName).ExtendedProperties["Format"]);
    }

    /// <summary>
    /// Получить формат для числового столбца или столбца даты/времени.
    /// Если значение не было установлено в явном виде, возвращается пустая строка.
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <returns>Установленное значение</returns>
    public string GetFormat(DataColumn column)
    {
      return DataTools.GetString(InternalGetColumn(column).ExtendedProperties["Format"]);
    }

    #endregion

    #region TextWidth

    /// <summary>
    /// Установить ширину столбца как количество символов.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetTextWidth(string columnName, int value)
    {
      CheckNotReadOnly();
      InternalGetColumn(columnName).ExtendedProperties["TextWidth"] = StdConvert.ToString(value);
    }

    /// <summary>
    /// Установить ширину столбца как количество символов.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetTextWidth(DataColumn column, int value)
    {
      CheckNotReadOnly();
      InternalGetColumn(column).ExtendedProperties["TextWidth"] = StdConvert.ToString(value);
    }

    /// <summary>
    /// Получить ширину столбца как количество символов.
    /// Если значение не было установлено в явном виде, будет возвращено значение 0.
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.</param>
    /// <returns>Установленное значение</returns>
    public int GetTextWidth(string columnName)
    {
      return StdConvert.ToInt32(DataTools.GetString(InternalGetColumn(columnName).ExtendedProperties["TextWidth"]));
    }

    /// <summary>
    /// Получить ширину столбца как количество символов.
    /// Если значение не было установлено в явном виде, будет возвращено значение 0.
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <returns>Установленное значение</returns>
    public int GetTextWidth(DataColumn column)
    {
      return StdConvert.ToInt32(DataTools.GetString(InternalGetColumn(column).ExtendedProperties["TextWidth"]));
    }

    #endregion

    #region MinTextWidth

    /// <summary>
    /// Установить минимальную ширину столбца как количество символов.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetMinTextWidth(string columnName, int value)
    {
      CheckNotReadOnly();
      InternalGetColumn(columnName).ExtendedProperties["MinTextWidth"] = StdConvert.ToString(value);
    }

    /// <summary>
    /// Установить минимальную ширину столбца как количество символов.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetMinTextWidth(DataColumn column, int value)
    {
      CheckNotReadOnly();
      InternalGetColumn(column).ExtendedProperties["MinTextWidth"] = StdConvert.ToString(value);
    }

    /// <summary>
    /// Получить минимальную ширину столбца как количество символов.
    /// Если значение не было установлено в явном виде, будет возвращено значение 0.
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.</param>
    /// <returns>Установленное значение</returns>
    public int GetMinTextWidth(string columnName)
    {
      return StdConvert.ToInt32(DataTools.GetString(InternalGetColumn(columnName).ExtendedProperties["MinTextWidth"]));
    }

    /// <summary>
    /// Получить минимальную ширину столбца как количество символов.
    /// Если значение не было установлено в явном виде, будет возвращено значение 0.
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <returns>Установленное значение</returns>
    public int GetMinTextWidth(DataColumn column)
    {
      return StdConvert.ToInt32(DataTools.GetString(InternalGetColumn(column).ExtendedProperties["MinTextWidth"]));
    }

    #endregion

    #region FillWeight

    /// <summary>
    /// Установить весовой коэффициент для столбца, который должен заполнять таблицу по ширине.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetFillWeight(string columnName, int value)
    {
      CheckNotReadOnly();
      InternalGetColumn(columnName).ExtendedProperties["FillWeight"] = StdConvert.ToString(value);
    }

    /// <summary>
    /// Установить весовой коэффициент для столбца, который должен заполнять таблицу по ширине.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetFillWeight(DataColumn column, int value)
    {
      CheckNotReadOnly();
      InternalGetColumn(column).ExtendedProperties["FillWeight"] = StdConvert.ToString(value);
    }

    /// <summary>
    /// Получить весовой коэффициент для столбца, который должен заполнять таблицу по ширине.
    /// Если значение не было установлено в явном виде, будет возвращено значение 0 - столбец имеет ширину, определяемую TextWitdh.
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.</param>
    /// <returns>Установленное значение</returns>
    public int GetFillWeight(string columnName)
    {
      return StdConvert.ToInt32(DataTools.GetString(InternalGetColumn(columnName).ExtendedProperties["FillWeight"]));
    }

    /// <summary>
    /// Получить весовой коэффициент для столбца, который должен заполнять таблицу по ширине.
    /// Если значение не было установлено в явном виде, будет возвращено значение 0 - столбец имеет ширину, определяемую TextWitdh.
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <returns>Установленное значение</returns>
    public int GetFillWeight(DataColumn column)
    {
      return StdConvert.ToInt32(DataTools.GetString(InternalGetColumn(column).ExtendedProperties["FillWeight"]));
    }

    #endregion

    #region Внутренние методы

    private DataColumn InternalGetColumn(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
      {
        if (Table.Columns.Count == 0)
          throw new InvalidOperationException("В таблице нет ни одного столбца DataColumn");
        return Table.Columns[Table.Columns.Count - 1];
      }
      else
      {
        DataColumn col = Table.Columns[columnName];
        if (col == null)
          throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");
        return col;
      }
    }

    private DataColumn InternalGetColumn(DataColumn column)
    {
      if (column == null)
      {
        if (Table.Columns.Count == 0)
          throw new InvalidOperationException("В таблице нет ни одного столбца DataColumn");
        return Table.Columns[Table.Columns.Count - 1];
      }
      else
        return column;
    }

    #endregion

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если можно только получать свойства, но не устанавливать их
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;


    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException("Не разрешено изменение свойств столбцов");
    }

    #endregion
  }

  #endregion
}
