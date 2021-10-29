using FreeLibSet.Config;
using FreeLibSet.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.UICore;
using FreeLibSet.Calendar;
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
      Text = String.Empty;
      MaxLength = 0;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// По умолчанию - пустая строка.
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;
    private string _OldText;

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

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
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
        return _OldText != Text;
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
  /// Диалог ввода произвольной строки текста с возможностью выбора из списка возможных значений.
  /// </summary>
  [Serializable]
  public class TextComboInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    /// <param name="items">Список строк, из которых можно выбрать значение.</param>
    public TextComboInputDialog(string[] items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      _Items = items;

      Title = "Ввод текста";
      Prompt = "Значение";
      Text = String.Empty;
      MaxLength = 0;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// По умолчанию - пустая строка.
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;
    private string _OldText;

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
    /// Задается в конструкторе
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;

    #endregion

    #region Свойства для проверки значения

    /// <summary>
    /// Может ли поле быть пустым.
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
        return _OldText != Text;
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
  /// Базовый класс для IntInputBox, SingleInputBox, DoubleInputBox и DecimalInputBox
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  [Serializable]
  public abstract class BaseNumInputDialog<T> : BaseInputDialog
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public BaseNumInputDialog()
    {
      Title = "Ввод числа";
      Prompt = "Значение";
      Format = String.Empty;
    }

    #endregion

    #region Свойства

    #region CanBeEmpty

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

    #endregion

    #region Value/NValue

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// Используйте это свойство, если CanBeEmpty=false.
    /// Пустое значение трактуется как 0.
    /// </summary>
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }


    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// </summary>
    public T? NValue { get { return _NValue; } set { _NValue = value; } }
    private T? _NValue;

    /// <summary>
    /// Используется при чтении/записи изменений
    /// </summary>
    protected T? OldNValue;

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
  /// Диалог ввода целого числа
  /// </summary>
  [Serializable]
  public class IntInputDialog : BaseNumInputDialog<Int32>
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
  /// Диалог ввода числа одинарной точности
  /// </summary>
  [Serializable]
  public class SingleInputDialog : BaseNumInputDialog<float>
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
  /// Диалог ввода числа двойной точности
  /// </summary>
  [Serializable]
  public class DoubleInputDialog : BaseNumInputDialog<double>
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
  /// Диалог ввода числа типа Decimal
  /// </summary>
  [Serializable]
  public class DecimalInputDialog : BaseNumInputDialog<decimal>
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
  /// Диалог ввода текста с возможностью выбора из списка возможных значений
  /// </summary>
  [Serializable]
  public class DateTimeInputDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public DateTimeInputDialog()
    {
      Title = "Ввод текста";
      Prompt = "Значение";
      _NValue = null;
      _Kind = EditableDateTimeFormatterKind.Date;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

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

    /// <summary>
    /// Вход и выход: редактируемое значение с поддержкой значения null
    /// </summary>
    public DateTime? NValue { get { return _NValue; } set { _NValue = value; } }
    private DateTime? _NValue;
    private DateTime? _OldNValue;

    /// <summary>
    /// Вход и выход: редактируемое значение. Для пустого значения возвращается DateTime.MinValue.
    /// </summary>
    public DateTime Value
    {
      get { return NValue ?? DateTime.MinValue; }
      set { NValue = value; }
    }


    /// <summary>
    /// Минимальное значение. По умолчанию ограничение не задано
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
    /// Максимальное значение. По умолчанию ограничение не задано
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
        return _OldNValue != NValue;
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
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вход и выход: редактируемое значение.
    /// Разделителем строк является Environment.NewLine.
    /// При передаче между компьютерами с разными операционными системами разделитель заменяется.
    /// Дублирует свойство Lines
    /// </summary>
    public string Text
    {
      get { return UITools.LinesToText(_Lines); }
      set { _Lines = UITools.TextToLines(value); }
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
      base.WriteChangeLines(part, "Value", Lines);
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
      Lines = ReadChangeLines(part, "Value");
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

  #endregion

  #region Ввод диапазонов

  /// <summary>
  /// Базовый класс для SingleRangeBox, DoubleRangeBox и DecimalRangeBox
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  [Serializable]
  public abstract class BaseNumRangeDialog<T> : BaseInputDialog
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public BaseNumRangeDialog()
    {
      Title = "Ввод диапазона чисел";
      Prompt = "Диапазон";
      _Format = String.Empty;
    }

    #endregion

    #region Свойства

    #region CanBempty

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

    #endregion

    #region First/LastValue/NValue

    /// <summary>
    /// Вход и выход: первое редактируемое значение с поддержкой null
    /// </summary>
    public T? NFirstValue { get { return _NFirstValue; } set { _NFirstValue = value; } }
    private T? _NFirstValue;

    /// <summary>
    /// Используется при чтении и записи значений
    /// </summary>
    protected T? OldNFirstValue;

    /// <summary>
    /// Вход и выход: второе редактируемое значение с поддержкой null
    /// </summary>
    public T? NLastValue { get { return _NLastValue; } set { _NLastValue = value; } }
    private T? _NLastValue;

    /// <summary>
    /// Используется при чтении и записи значений
    /// </summary>
    protected T? OldNLastValue;

    /// <summary>
    /// Вход и выход: первое редактируемое значение без поддержки null
    /// </summary>
    public T FirstValue
    {
      get { return NFirstValue ?? default(T); }
      set { NFirstValue = value; }
    }

    /// <summary>
    /// Вход и выход: второе редактируемое значение без поддержки null
    /// </summary>
    public T LastValue
    {
      get { return NLastValue ?? default(T); }
      set { NLastValue = value; }
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
    /// Минимальное значение. 
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
    /// Максимальное значение.
    /// </summary>
    public T? Maximum
    {
      get { return _Maximum; }
      set
      {
        CheckNotFixed();
        _Maximum = value;
      }
    }
    private T? _Maximum;

    #endregion

    #region Increment

    /// <summary>
    /// Если задано положительное значение (обычно, 1), то значения в полях можно прокручивать с помощью
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
    /// <param name="cfgType">Тип секции конфигурации, определяющий место ее хранения</param>
    /// <returns>true, если элемент может хранить данные</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода диапазона целых чисел
  /// </summary>
  [Serializable]
  public class IntRangeDialog : BaseNumRangeDialog<Int32>
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
        return OldNFirstValue != NFirstValue || OldNLastValue != NLastValue;
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
      part.SetNullableInt("FirstValue", NFirstValue);
      part.SetNullableInt("LastValue", NLastValue);
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
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
      NFirstValue = part.GetNullableInt("FirstValue");
      NLastValue = part.GetNullableInt("LastValue");
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableInt(Name + "-FirstValue", NFirstValue);
      part.SetNullableInt(Name + "-LastValue", NLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NFirstValue = part.GetNullableInt(Name + "-FirstValue");
      NLastValue = part.GetNullableInt(Name + "-LastValue");
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
  /// Диалог ввода числа одинарной точности
  /// </summary>
  [Serializable]
  public class SingleRangeDialog : BaseNumRangeDialog<float>
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
        return OldNFirstValue != NFirstValue || OldNLastValue != NLastValue;
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
      part.SetNullableSingle("FirstValue", NFirstValue);
      part.SetNullableSingle("LastValue", NLastValue);
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
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
      NFirstValue = part.GetNullableSingle("FirstValue");
      NLastValue = part.GetNullableSingle("LastValue");
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableSingle(Name + "-FirstValue", NFirstValue);
      part.SetNullableSingle(Name + "-LastValue", NLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NFirstValue = part.GetNullableSingle(Name + "-FirstValue");
      NLastValue = part.GetNullableSingle(Name + "-LastValue");
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа двойной точности
  /// </summary>
  [Serializable]
  public class DoubleRangeDialog : BaseNumRangeDialog<double>
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
        return OldNFirstValue != NFirstValue || OldNLastValue != NLastValue;
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
      part.SetNullableDouble("FirstValue", NFirstValue);
      part.SetNullableDouble("LastValue", NLastValue);
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
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
      NFirstValue = part.GetNullableDouble("FirstValue");
      NLastValue = part.GetNullableDouble("LastValue");
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDouble(Name + "-FirstValue", NFirstValue);
      part.SetNullableDouble(Name + "-LastValue", NLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NFirstValue = part.GetNullableDouble(Name + "-FirstValue");
      NLastValue = part.GetNullableDouble(Name + "-LastValue");
    }

    #endregion
  }

  /// <summary>
  /// Диалог ввода числа decimal
  /// </summary>
  [Serializable]
  public class DecimalRangeDialog : BaseNumRangeDialog<decimal>
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
        return OldNFirstValue != NFirstValue || OldNLastValue != NLastValue;
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
      part.SetNullableDecimal("FirstValue", NFirstValue);
      part.SetNullableDecimal("LastValue", NLastValue);
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
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
      NFirstValue = part.GetNullableDecimal("FirstValue");
      NLastValue = part.GetNullableDecimal("LastValue");
      OldNFirstValue = NFirstValue;
      OldNLastValue = NLastValue;
    }

    /// <summary>
    /// Записывает значения, сохраняемые между сеансами работы, в заданную секцию конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для записи</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetNullableDecimal(Name + "-FirstValue", NFirstValue);
      part.SetNullableDecimal(Name + "-LastValue", NLastValue);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NFirstValue = part.GetNullableDecimal(Name + "-FirstValue");
      NLastValue = part.GetNullableDecimal(Name + "-LastValue");
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
      NFirstDate = null;
      NLastDate = null;
      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Могут ли поля быть пустыми.
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
    /// Могут ли поля быть пустыми.
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
    /// Вход и выход - первое редактируемое значение с поддержкой null
    /// </summary>
    public DateTime? NFirstDate { get { return _NFirstDate; } set { _NFirstDate = value; } }
    private DateTime? _NFirstDate;
    private DateTime? _OldNFirstDate;

    /// <summary>
    /// Вход и выход - второе редактируемое значение с поддержкой null
    /// </summary>
    public DateTime? NLastDate { get { return _NLastDate; } set { _NLastDate = value; } }
    private DateTime? _NLastDate;
    private DateTime? _OldNLastDate;

    /// <summary>
    /// Первое редактируемое значение. Пустое значение заменяется на минимально возможную дату
    /// </summary>
    public DateTime FirstDate
    {
      get { return NFirstDate ?? DateRange.Whole.FirstDate; }
      set { NFirstDate = value; }
    }

    /// <summary>
    /// Второе редактируемое значение. Пустое значение заменяется на максимально возможную дату
    /// </summary>
    public DateTime LastDate
    {
      get { return NLastDate ?? DateRange.Whole.LastDate; }
      set { NLastDate = value; }
    }

    /// <summary>
    /// Минимальное значение. По умолчанию: null
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
    /// Максимальное значение. По умолчанию: null
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
        return _OldNFirstDate != NFirstDate || _OldNLastDate != NLastDate;
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
      part.SetNullableDate("FirstValue", NFirstDate);
      part.SetNullableDate("LastValue", NLastDate);
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
      NFirstDate = part.GetNullableDate("FirstValue");
      NLastDate = part.GetNullableDate("LastValue");
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
      part.SetNullableDate(Name + "-FirstValue", NFirstDate);
      part.SetNullableDate(Name + "-LastValue", NLastDate);
    }

    /// <summary>
    /// Считывает значения, сохраняемые между сеансами работы, из заданной секции конфигурации.
    /// Метод вызывается, только если OnSupportsCfgType() вернул true для заданного типа секции.
    /// </summary>
    /// <param name="part">Секция конфигурации для чтения значений</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      NFirstDate = part.GetNullableDate(Name + "-FirstValue");
      NLastDate = part.GetNullableDate(Name + "-LastValue");
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
