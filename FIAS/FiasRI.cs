// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.RI;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.DependedValues;

// Удаленный пользовательский интерфейс

namespace FreeLibSet.FIAS.RI
{
  /// <summary>
  /// Удаленный интерфейс для панели ввода компонентов адреса.
  /// Обычно удобнее использовать комбоблок FiasAddressComboBox
  /// </summary>
  [Serializable]
  public class FiasAddressPanel : Control
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует начальные значения свойств
    /// </summary>
    public FiasAddressPanel(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _AddressString = String.Empty;
      _OldAddressString = String.Empty;
    }

    #endregion

    #region Свойства

    [NonSerialized]
    private IFiasSource _Source;

    /// <summary>
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="source">Источник адресов после десериализации на стороне клиента</param>
    public void InternalSetSource(IFiasSource source)
    {
      _Source = source;
    }

    /// <summary>
    /// Уровень, до которого можно задавать адрес.
    /// Значение по умолчанию: Room
    /// Свойство можно задавать только до вывода диалога на экран.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        CheckNotFixed();
        _EditorLevel = value;
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// Можно ли редактировать почтовый индекс?
    /// По умолчанию - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set
      {
        CheckNotFixed();
        _PostalCodeEditable = value;
      }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// Задает минимальный уровень адреса, который должен быть выбран из справочника, а не задан вручную.
    /// По умолчанию - FiasLevel.City, то есть регион, район и город должны быть в справочнике ФИАС, а населенный пункт,
    /// при необходимости - введен вручную, если его нет в справочнике.
    /// Значение Unknown отключает все проверки. 
    /// Допускаются любые значения, включая House и Room, если они не выходят за пределы FiasEditorLevel.
    /// Свойство можно устанавливать только до вывода элемента на экран. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        CheckNotFixed();
        _MinRefBookLevel = value;
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// Текущий адрес
    /// </summary>
    public FiasAddress Address
    {
      get
      {
        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        return convert.Parse(_AddressString);
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        _AddressString = convert.ToString(value);

      }
    }
    private string _AddressString;
    private string _OldAddressString;

    #endregion

    #region Свойства CanBeEmpty и CanBePartial

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


    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию - Error адрес должен быть заполнен согласно свойству EditorLevel.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        CheckNotFixed();
        _CanBePartialMode = value;
      }
    }
    private UIValidateState _CanBePartialMode;


    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// По умолчанию - false - адрес должен быть заполнен согласно свойству EditorLevel.
    /// Например, если EditorLevel=Room, то должен быть задан, как минимум, дом.
    /// Это свойство дублирует CanBePartialMode, но не позволяет установить режим предупреждения.
    /// При CanBePartialMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBePartialMode=Ok, а false - CanBePartialMode=Error.
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
        return _AddressString != _OldAddressString;
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
      part.SetString("AddressString", _AddressString);
      _OldAddressString = _AddressString;
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
      _AddressString = part.GetString("AddressString");
      _OldAddressString = _AddressString;
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
      part.SetString(Name, _AddressString);
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
        _AddressString = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Удаленный интерфейс для комбоблока выбора адреса
  /// </summary>
  [Serializable]
  public class FiasAddressComboBox : Control
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует начальные значения свойств
    /// </summary>
    public FiasAddressComboBox(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _TextFormat = FiasFormatStringParser.DefaultFormat;
      _AddressString = String.Empty;
      _OldAddressString = String.Empty;
    }

    #endregion

    #region Свойства

    [NonSerialized]
    private IFiasSource _Source;

    /// <summary>
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="source">Источник адресов после десериализации на стороне клиента</param>
    public void InternalSetSource(IFiasSource source)
    {
      _Source = source;
    }

    /// <summary>
    /// Уровень, до которого можно задавать адрес.
    /// Значение по умолчанию: Room
    /// Свойство можно задавать только до вывода диалога на экран.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        CheckNotFixed();
        _EditorLevel = value;
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// Можно ли редактировать почтовый индекс?
    /// По умолчанию - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set
      {
        CheckNotFixed();
        _PostalCodeEditable = value;
      }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// Задает минимальный уровень адреса, который должен быть выбран из справочника, а не задан вручную.
    /// По умолчанию - FiasLevel.City, то есть регион, район и город должны быть в справочнике ФИАС, а населенный пункт,
    /// при необходимости - введен вручную, если его нет в справочнике.
    /// Значение Unknown отключает все проверки. 
    /// Допускаются любые значения, включая House и Room, если они не выходят за пределы FiasEditorLevel.
    /// Свойство можно устанавливать только до вывода элемента на экран. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        CheckNotFixed();
        _MinRefBookLevel = value;
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// Формат текстового представления адреса в поле.
    /// По умолчанию - "TEXT".
    /// </summary>
    public string TextFormat
    {
      get { return _TextFormat; }
      set
      {
        CheckNotFixed();
        string errorText;
        if (!FiasFormatStringParser.IsValidFormat(value, out errorText))
          throw new ArgumentException(errorText);
        _TextFormat = value; // испр. 23.11.2021
      }
    }
    private string _TextFormat;

    /// <summary>
    /// Текущий адрес
    /// </summary>
    public FiasAddress Address
    {
      get
      {
        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        return convert.Parse(_AddressString);
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        _AddressString = convert.ToString(value);

      }
    }
    private string _AddressString;
    private string _OldAddressString;

    #endregion

    #region Свойства CanBeEmpty и CanBePartial

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


    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию - Error адрес должен быть заполнен согласно свойству EditorLevel.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        CheckNotFixed();
        _CanBePartialMode = value;
      }
    }
    private UIValidateState _CanBePartialMode;


    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// По умолчанию - false - адрес должен быть заполнен согласно свойству EditorLevel.
    /// Например, если EditorLevel=Room, то должен быть задан, как минимум, дом.
    /// Это свойство дублирует CanBePartialMode, но не позволяет установить режим предупреждения.
    /// При CanBePartialMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBePartialMode=Ok, а false - CanBePartialMode=Error.
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
        return _AddressString != _OldAddressString;
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
      part.SetString("AddressString", _AddressString);
      _OldAddressString = _AddressString;
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
      _AddressString = part.GetString("AddressString");
      _OldAddressString = _AddressString;
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
      part.SetString(Name, _AddressString);
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
        _AddressString = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Удаленный интерфейс для диалога ввода или просмотра адреса.
  /// Обычно удобнее использовать комбоблок FiasAddressComboBox
  /// </summary>
  [Serializable]
  public class FiasAddressDialog : StandardDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует начальные значения свойств
    /// </summary>
    public FiasAddressDialog(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _AddressString = String.Empty;
      _OldAddressString = String.Empty;
    }

    #endregion

    #region Свойства

    [NonSerialized]
    private IFiasSource _Source;

    /// <summary>
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="source">Источник адресов после десериализации на стороне клиента</param>
    public void InternalSetSource(IFiasSource source)
    {
      _Source = source;
    }

    /// <summary>
    /// Уровень, до которого можно задавать адрес.
    /// Значение по умолчанию: Room
    /// Свойство можно задавать только до вывода диалога на экран.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        CheckNotFixed();
        _EditorLevel = value;
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// Если установить в true, то диалог будет предназначен только для просмотра адреса, а не для редактирования
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
    /// Можно ли редактировать почтовый индекс?
    /// По умолчанию - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set
      {
        CheckNotFixed();
        _PostalCodeEditable = value;
      }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// Задает минимальный уровень адреса, который должен быть выбран из справочника, а не задан вручную.
    /// По умолчанию - FiasLevel.City, то есть регион, район и город должны быть в справочнике ФИАС, а населенный пункт,
    /// при необходимости - введен вручную, если его нет в справочнике.
    /// Значение Unknown отключает все проверки. 
    /// Допускаются любые значения, включая House и Room, если они не выходят за пределы FiasEditorLevel.
    /// Свойство можно устанавливать только до вывода элемента на экран. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        CheckNotFixed();
        _MinRefBookLevel = value;
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// Текущий адрес
    /// </summary>
    public FiasAddress Address
    {
      get
      {
        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        return convert.Parse(_AddressString);
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        _AddressString = convert.ToString(value);

      }
    }
    private string _AddressString;
    private string _OldAddressString;

    #endregion

    #region Свойства CanBeEmpty и CanBePartial

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


    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// Свойство может устанавливаться только до передачи диалога вызываемой стороне.
    /// Значение по умолчанию - Error адрес должен быть заполнен согласно свойству EditorLevel.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        CheckNotFixed();
        _CanBePartialMode = value;
      }
    }
    private UIValidateState _CanBePartialMode;


    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// По умолчанию - false - адрес должен быть заполнен согласно свойству EditorLevel.
    /// Например, если EditorLevel=Room, то должен быть задан, как минимум, дом.
    /// Это свойство дублирует CanBePartialMode, но не позволяет установить режим предупреждения.
    /// При CanBePartialMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBePartialMode=Ok, а false - CanBePartialMode=Error.
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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
        return _AddressString != _OldAddressString;
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
      part.SetString("AddressString", _AddressString);
      _OldAddressString = _AddressString;
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
      _AddressString = part.GetString("AddressString");
      _OldAddressString = _AddressString;
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
      part.SetString(Name, _AddressString);
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
        _AddressString = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// Статические методы, которые можно использовать для поддержки удаленного пользовательского интерфейса,
  /// связанного с адресами ФИАС.
  /// </summary>
  public static class FiasRITools
  {
    #region Строка форматирования адреса

    /// <summary>
    /// Вычисляемое выражение, которое можно использовать в валидаторе текстового поля ввода формата текстового представления адреса.
    /// 
    /// Возвращает UIValidateResult c IsValid=true, если переданная строка является правильным форматом для текстового представления адреса.
    /// Вызывается метод FiasFormatStringParser.IsValidFormat().
    /// </summary>
    /// <param name="s">Управляемое свойство проверяемой строки</param>
    /// <returns>true, если формат правильный</returns>
    public static DepValue<UIValidateResult> FormatStringValidateResultEx(DepValue<string> s)
    {
      return new DepExpr1<UIValidateResult, string>(s, FormatStringValidateResult);
    }

    private static UIValidateResult FormatStringValidateResult(string s)
    {
      string errorMessage;
      bool isValid = FiasFormatStringParser.IsValidFormat(s, out errorMessage);
      return new UIValidateResult(isValid, errorMessage);
    }

    /// <summary>
    /// Создает управляющий элемент-комболок для ввода строки форматирования адреса.
    /// Выпадающий список содержит готовые варианты, но можно ввести формат вручную.
    /// Добавлена проверка корректности формата строки.
    /// </summary>
    /// <returns>Управляющий элемент</returns>
    public static TextComboBox CreateFormatStringTextComboBox()
    {
      TextComboBox control = new TextComboBox(FiasFormatStringParser.ComponentTypes);
      control.Validators.AddError(FormatStringValidateResultEx(control.TextEx),
        control.IsNotEmptyEx);
      return control;
    }

    #endregion
  }
}
