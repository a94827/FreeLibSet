// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Remoting;
using FreeLibSet.IO;
using FreeLibSet.Formatting;
using System.Runtime.Serialization;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;
using FreeLibSet.UICore;

/*
 * Удаленный диалоговый интерфейс.
 * 
 * Обычное использование терминов "клиент" и "сервер" является неудобным, т.к. вводит в заблуждение.
 * Используются термины "вызывающая сторона" - та, где ожидаются результаты ввода данных и 
 * "вызываемая строна", которая взаимодействует с пользователем
 * 
 * Вызывающая сторона создает объект RIDialog и добавляет в него элементы, производные от RIItem,
 * например RILabel, RITextBox, ... Устанавливаются начальные значения свойств, например RITextBox.Text.
 * Объекты RIDialog и RIItem являются сериализуемыми. Объект RIDialog передается вызываемой стороне.
 * 
 * На вызываемой стороне выводится блок диалога. Для этого используются классы, реализованные в модуле ExtForms.dll
 * (если используется интерфейс Windows Forms). Для каждого RIItem имеется эквивалентный класс в ExtForms.
 * Эти классы отвечают за наполнение блока диалога реальным содержимым, получение и отправку реальных значений.
 * Класс RIItem содержит свойство HasChanges, которое возвращает true,
 * если есть несохраненные изменения. Класс реализует абстрактные методы ReadChanges() и WriteChanges(). 
 * Они получают в качестве аргумента объект CfgPart. Передаются только те свойства, которые изменились
 * с предыдущего вызова. Если свойство HasChanges возвращает false, метод WriteChanges() не вызывается и
 * "лишние" данные не передаются.
 * 
 * Когда пользователь нажимает "ОК" в блоке диалога, вызывается метод RIItem.GetValues() для всех элементов,
 * имеющих имя. Заполненный CfgPart передается вызывающей стороне. Там вызываются методы RIItem.WriteChanges(),
 * после чего на вызывающей стороне объект RIDialog становится синхронизированным с введенными значениями.
 * Пользовательский обработчик (событие RIDialog.Validating) может установить свойство RIItem.Error. 
 * Если есть ошибки, то вызывающая сторона собирает список ошибок и передает его вызываемой стороне. 
 * Для передачи ошибок используется те же методы ReadChanges() и WriteChanges(), что и для вводимых значений
 * В блоке диалога на экране выполняется подсветка соответствующих элементов и ожидается повторное нажатие
 * кнопки ОК.
 * В отличие от ExtForms, где EFPControl также поддерживает метод SetWarning(), для удаленного интерфейса
 * установка предупреждений не реализована. Предупреждения обычно используются для динамической подсветки
 * управляющих элементов. Удаленный интерфейс не поддерживает динамическую подсветку элементов, т.к. это
 * потребовало бы пересылки значений между сторонами, например, в процессе редактирования текста, что замедлило
 * бы работу
 * 
 * Идентификация элементов
 * Однократная передача диалога вызываемой стороне использует механизм сериализации. При этом RIDialog
 * восстанавливается в полном объеме без использования имен. При использовании WriteChanges() обязанность
 * по созданию секции CfgPart для элемента ложится на элемент-родитель. Он же отвечает за именование дочерних
 * элементов. Например, RIBand использует имена секций "C0", "C1", ..., соответствующие индексам дочерних
 * элементов
 * 
 * Пользовательский код может использовать свойство RIItem.Name и метод Find() для поиска управляющих элементов.
 * Также можно использовать обычные ссылки на элементы. Например
 * 
 * RIDialog dlg=new RIDialog("Тестовый диалог");
 * RIDateBox dt1=new RIDateBox();
 * dt1.Value=DateTime.Today; // исходное значение поля ввода
 * dlg.Controls.Add(new RILabelWithControl("Дата", dt1));
 * 
 * IRemoteInterface UI=...; // интерфейс обработки
 * if (UI.ShowDialog(dlg)!=RIDialogResult.Ok)
 *   return;
 * 
 * xxx=dt1.Value; // введенное пользователем значение
 * 
 * 
 * 
 * Запуска диалога на выполнение на сервере
 * -----------------------------
 * Основной задачей RI является показ диалога из процедуры ExecProc, выполняемой на сервере
 * Предполагается, что свойство ExecProc.Context["UIProcProxy"] возвращает ссылку на callback-процедуру
 * обработки интерфейса клиентом
 * Интерфейс IRemoteInterface объявляет методы MessageBox(), ShowDialog()
 */

namespace FreeLibSet.RI
{
  #region Перечисления для MessageBox

  /// <summary>
  /// Specifies constants defining which buttons to display on a System.Windows.Forms.MessageBox.
  /// </summary>
  [Serializable]
  public enum MessageBoxButtons
  {
    /// <summary>
    /// The message box contains an OK button.
    /// </summary>
    OK = 0,

    /// <summary>
    /// The message box contains OK and Cancel buttons.
    /// </summary>
    OKCancel = 1,

    /// <summary>
    /// The message box contains Abort, Retry, and Ignore buttons.
    /// </summary>
    AbortRetryIgnore = 2,

    /// <summary>
    /// The message box contains Yes, No, and Cancel buttons.
    /// </summary>
    YesNoCancel = 3,

    /// <summary>
    /// The message box contains Yes and No buttons.
    /// </summary>
    YesNo = 4,

    /// <summary>
    /// The message box contains Retry and Cancel buttons.
    /// </summary>
    RetryCancel = 5,
  }

  /// <summary>
  /// Specifies constants defining which information to display.
  /// </summary>
  [Serializable]
  public enum MessageBoxIcon
  {
    /// <summary>
    /// The message box contain no symbols.
    /// </summary>
    None = 0,

    /// <summary>
    /// The message box contains a symbol consisting of white X in a circle with a red background.
    /// </summary>
    Error = 16,

    /// <summary>
    /// The message box contains a symbol consisting of a white X in a circle with a red background.
    /// </summary>
    Hand = 16,

    /// <summary>
    /// The message box contains a symbol consisting of white X in a circle with a red background.
    /// </summary>
    Stop = 16,

    /// <summary>
    /// The message box contains a symbol consisting of a question mark in a circle.
    /// The question-mark message icon is no longer recommended because it does not
    /// clearly represent a specific type of message and because the phrasing of
    /// a message as a question could apply to any message type. In addition, users
    /// can confuse the message symbol question mark with Help information. Therefore,
    /// do not use this question mark message symbol in your message boxes. The system
    /// continues to support its inclusion only for backward compatibility.
    /// </summary>
    Question = 32,

    /// <summary>
    /// The message box contains a symbol consisting of an exclamation point in a
    /// triangle with a yellow background.
    /// </summary>
    Exclamation = 48,

    /// <summary>
    /// The message box contains a symbol consisting of an exclamation point in a
    /// triangle with a yellow background.
    /// </summary>
    Warning = 48,

    /// <summary>
    /// The message box contains a symbol consisting of a lowercase letter i in a circle.
    /// </summary>
    Information = 64,

    /// <summary>
    /// The message box contains a symbol consisting of a lowercase letter i in a circle.
    /// </summary>
    Asterisk = 64,
  }

  /// <summary>
  /// Specifies constants defining the default button on a System.Windows.Forms.MessageBox.
  /// </summary>
  [Serializable]
  public enum MessageBoxDefaultButton
  {
    /// <summary>
    /// The first button on the message box is the default button.
    /// </summary>
    Button1 = 0,

    /// <summary>
    /// The second button on the message box is the default button.
    /// </summary>
    Button2 = 256,

    /// <summary>
    /// The third button on the message box is the default button.
    /// </summary>
    Button3 = 512,
  }

  /// <summary>
  /// Результат вызова блока диалога
  /// </summary>
  [Serializable]
  public enum DialogResult
  {
    /// <summary>
    /// Nothing is returned from the dialog box. 
    /// This means that the modal dialog continues running.
    /// Не используется.
    /// </summary>
    None = 0,

    /// <summary>
    /// The dialog box return value is OK (usually sent from a button labeled OK).
    /// </summary>
    OK = 1,

    /// <summary>
    /// The dialog box return value is Cancel (usually sent from a button labeled Cancel).
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// The dialog box return value is Abort (usually sent from a button labeled Abort).
    /// </summary>
    Abort = 3,

    /// <summary>
    /// The dialog box return value is Retry (usually sent from a button labeled Retry).
    /// </summary>
    Retry = 4,

    /// <summary>
    /// The dialog box return value is Ignore (usually sent from a button labeled Ignore).
    /// </summary>
    Ignore = 5,

    /// <summary>
    /// The dialog box return value is Yes (usually sent from a button labeled Yes).
    /// </summary>
    Yes = 6,

    /// <summary>
    /// The dialog box return value is No (usually sent from a button labeled No).
    /// </summary>
    No = 7,
  }

  #endregion

  #region Перечисления для управляющих элементов

  /// <summary>
  /// Specifies how an object or text in a control is horizontally aligned relative
  /// to an element of the control.
  /// </summary>
  public enum HorizontalAlignment
  {
    /// <summary>
    /// The object or text is aligned on the left of the control element.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The object or text is aligned on the right of the control element.
    /// </summary>
    Right = 1,

    /// <summary>
    /// The object or text is aligned in the center of the control element.
    /// </summary>
    Center = 2,
  }

  #endregion

  /// <summary>
  /// Базовый класс для передаваемых элементов пользовательского интерфейса
  /// </summary>
  [Serializable]
  public abstract class RIItem : IRIValueSaveable
  {
    #region Конструктор

    /// <summary>
    /// Конструктор базового класса
    /// </summary>
    public RIItem()
    {
      _Name = null;
      _ErrorMessage = null;
    }

    #endregion

    #region Имя

    /// <summary>
    /// Имя управляющего элемента для поиска.
    /// Использование свойства не является обязательным.
    /// Корректность имени проверяется с помощью CfgPart.IsValidName().
    /// Имя должно начинаться с буквы. Допускаются только буквы, цифры и символы "-", "_" и ".".
    /// </summary>
    public string Name
    {
      get { return _Name; }
      set
      {
        CheckNotFixed();
        if (String.IsNullOrEmpty(value))
          _Name = null;
        else
        {
          CfgPart.ValidateName(value);
          _Name = value;
        }
      }
    }
    private string _Name;

    /*
    /// <summary>
    /// Проверяет наличие установленного значения для свойства Name.
    /// Если свойство не установлено, выбрасывает NullReferenceException
    /// </summary>
    protected void CheckHasName()
    {
      if (String.IsNullOrEmpty(Name))
        throw new NullReferenceException("Свойство Name не установлено для элемента");
    }
     */

    /// <summary>
    /// Поиск управляющего элемента по имени.
    /// Непереопределенный метод проверяет соответствие <paramref name="name"/> свойству Name текукущего элемента.
    /// Для контейнеров, переопределенный метод выполняет поиск среди дочерних элементов.
    /// Имена элементов являются чувствительными к регистру
    /// </summary>
    /// <param name="name">Искомое имя</param>
    /// <returns>Ссылка на найденный элемент или null.</returns>
    public virtual RIItem Find(string name)
    {
      if (String.IsNullOrEmpty(name))
        return null;
      if (name == _Name)
        return this;
      else
        return null;
    }

    /// <summary>
    /// Получение полного списка элементов.
    /// Непереопределенный метод просто добавляет текущий объект к списку.
    /// Для контейнеров, переопределенный метод добавляет также все дочерние элементы
    /// </summary>
    /// <param name="Items">Список для заполнения</param>
    public virtual void GetItems(ICollection<RIItem> Items)
    {
      Items.Add(this);
    }

    #endregion

    #region Чтение и запись значений

    #region Передача изменений между сервером и клиентом при сериализации

    /// <summary>
    /// Свойство возвращает true, если для элемента есть непереданные на другую сторону изменения в значениях свойств,
    /// которые могут меняться при показе блока диалога.
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Свойство не используется в пользовательском коде.
    /// </summary>
    public virtual bool HasChanges { get { return ErrorMessage != OldErrorMessage; } }

    /// <summary>
    /// Записать изменения. Метод вызывается родительским объектом, только если свойство HasChanges вернуло true. 
    /// На родительском объекте лежит обязанность по созданию раздела конфигурации <paramref name="part"/>
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для записи значений</param>
    public virtual void WriteChanges(CfgPart part)
    {
      if (ErrorMessage != OldErrorMessage)
      {
        part.SetString("Error", ErrorMessage);
        _OldErrorMessage = _ErrorMessage;
      }
    }

    /// <summary>
    /// Прочитать изменения, переданные "с другой стороны".
    /// Однократно задаваемые свойства, которые не могут меняться при работе диалога, не учитываются.
    /// Метод не используется в пользовательском коде.
    /// </summary>
    /// <param name="part">Секция для чтения значений</param>
    public virtual void ReadChanges(CfgPart part)
    {
      _ErrorMessage = part.GetString("Error");
      if (_ErrorMessage == String.Empty)
        _ErrorMessage = null;
      _OldErrorMessage = _ErrorMessage;
    }

    #endregion

    #region Сохранение значений между сеансами работы

    /// <summary>
    /// Если класс поддерживает сохранение значений между вызовами, он должен вернуть true для той секции конфигурации (или нескольких секций), 
    /// которая используется для хранения значения.
    /// Метод должен вернуть false, если свойство Name не установлено.
    /// Метод переопределяется для составных элементов, например RIBand.
    /// Вместо этого метода обычно следует переопределить OnSupportsCfgType.
    /// </summary>
    /// <param name="cfgType">Запрашиваемый тип секции конфигурации</param>
    /// <returns>true, если секция используется</returns>
    public virtual bool SupportsCfgType(RIValueCfgType cfgType)
    {
      if (String.IsNullOrEmpty(Name))
        return false;
      else
        return OnSupportsCfgType(cfgType);
    }

    /// <summary>
    /// Должно переопределяться для управляющих элементов и стандартных блоков диалога, которые поддерживают
    /// сохранение введенного значения между вызовами.
    /// Обычно, переопределенный метод должен вернуть true, если <paramref name="сfgType"/>=Default.
    /// Метод не вызывается, если свойство Name не устрановлено
    /// </summary>
    /// <param name="сfgType">Запрашиваемый тип секции конфигурации</param>
    /// <returns>true, если секция используется</returns>
    protected virtual bool OnSupportsCfgType(RIValueCfgType сfgType)
    {
      return false;
    }

    /// <summary>
    /// Записать значения в секцию конфигурации для сохранения между сеансами работы.
    /// В отличие от WriteChanges(), отдельная секция для каждого элемента не создается, т.к. это было бы не экономно.
    /// Метод вызывает SupportsCfgType() и, если true, вызывает OnWriteValues(), который и должен быть
    /// переопределен для большинства элементов.
    /// Метод переопределяется для составных элементов, типа RIItem
    /// </summary>
    /// <param name="part">Записываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации. Должен проверяться перед записью</param>
    public virtual void WriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      try
      {
        if (SupportsCfgType(cfgType))
          OnWriteValues(part, cfgType);
      }
      catch { }
    }

    /// <summary>
    /// Записать значения в секцию конфигурации для сохранения между сеансами работы.
    /// В отличие от WriteChanges(), отдельная секция для каждого элемента не создается, т.к. это было бы не экономно.
    /// В качестве имени записываемого значения должно использоваться свойство Name. Если требуется
    /// записать несколько значений, следует использовать имя Name+"."+Суффикс.
    /// Метод вызывается, только если SupportsCfgType() возвращает true для данной секции.
    /// Поэтому, обычно нет необходимости проверять свойство Name и аргумент CfgType.
    /// </summary>
    /// <param name="part">Записываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected virtual void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      throw ExceptionFactory.MustBeReimplemented(this, "OnWriteValues()");
    }

    /// <summary>
    /// Прочитать значения из секции конфигурации, сохраненные между сеансами работы
    /// В отличие от ReadChanges(), отдельная секция для каждого элемента не создается, т.к. это было бы не экономно.
    /// Метод вызывает SupportsCfgType() и, если true, вызывает OnWriteValues(), который и должен быть
    /// переопределен для большинства элементов.
    /// Метод переопределяется для составных элементов, типа RIItem
    /// </summary>
    /// <param name="part">Считываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    public virtual void ReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      try
      {
        if (SupportsCfgType(cfgType))
          OnReadValues(part, cfgType);
      }
      catch { }
    }

    /// <summary>
    /// Прочитать значения из секцию конфигурации для сохранения между сеансами работы.
    /// В отличие от ReadChanges(), отдельная секция для каждого элемента не создается, т.к. это было бы не экономно.
    /// В качестве имени записываемого значения должно использоваться свойство Name. Если требуется
    /// записать несколько значений, следует использовать имя Name+"."+Суффикс.
    /// Метод вызывается, только если SupportsCfgType() возвращает true для данной секции.
    /// Поэтому, обычно нет необходимости проверять свойство Name и аргумент CfgType.
    /// </summary>
    /// <param name="part">Считываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    protected virtual void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      throw ExceptionFactory.MustBeReimplemented(this, "OnReadValues()");
    }

    #endregion

    #endregion

    #region Фиксация изменений

    /// <summary>
    /// Свойство возвращает true, если диалог уже был передан клиенту.
    /// Когда свойство установлено, разрешается только изменение вводимых значений (например, свойства TextBox.Text),
    /// но не управляющих свойств (TextBox.CanBeEmpty). Также запрещается добавление дочерних элементов
    /// </summary>
    public bool IsFixed { get { return _IsFixed; } }

    [NonSerialized] // после передачи вызываемой стороне, свойство будет установлено заново
    private bool _IsFixed;

    /// <summary>
    /// Установить свойство IsFixed у текущего элемента и всех дочерних элементов.
    /// Метод может вызываться многократно
    /// </summary>
    public void SetFixed()
    {
      if (!_IsFixed)
        OnSetFixed();
    }

    /// <summary>
    /// Устанавливает свойство IsFixed в true.
    /// Переопределяется составными элементами, такими, как RIBand, для установки свойства дочерних элементов
    /// </summary>
    protected virtual void OnSetFixed()
    {
      _IsFixed = true;
    }

    /// <summary>
    /// Если свойство IsFixed=true, генерируется исключение InvalidOperationException
    /// </summary>
    public void CheckNotFixed()
    {
      if (_IsFixed)
        throw new InvalidOperationException(Res.RI_Err_IsFixed);
    }

    #endregion

    #region Проверка элемента

    /// <summary>
    /// Установить состояние ошибки для элемента.
    /// Этот метод может вызываться из обработчика события Dialog.Validating.
    /// Очищать сообщение об ошибке в обработчике Validating (вызовом ClearErrors()) обычно нет необходимости.
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке. Не может быть пустой строкой</param>
    public void SetError(string message)
    {
      if (String.IsNullOrEmpty(message))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("message");
      _ErrorMessage = message;
    }

    /// <summary>
    /// Очищает список ошибок.
    /// Для контейнера также выполняет очистку для всех дочерних элементов.
    /// Этот метод не используется в прикладном коде.
    /// </summary>
    public virtual void ClearErrors()
    {
      _ErrorMessage = null;
    }

    /// <summary>
    /// Текущее сообщение об ошибке.
    /// Возвращает пустую строку, если нет ошибки.
    /// </summary>
    public string ErrorMessage
    {
      get
      {
        if (_ErrorMessage == null)
          return String.Empty;
        else
          return _ErrorMessage;
      }
    }
    /// <summary>
    /// Храним null вместо "", чтобы сэкономить на сериализации
    /// </summary>
    private string _ErrorMessage;

    private string OldErrorMessage
    {
      get
      {
        if (_OldErrorMessage == null)
          return String.Empty;
        else
          return _OldErrorMessage;
      }
    }
    private string _OldErrorMessage;

    /// <summary>
    /// Рекурсивный поиск элемента с ошибкой.
    /// Возвращает первый элемент, у которого установлено свойство RIItem.ErrorMessage.
    /// Возвращает null, если ошибок нет
    /// </summary>
    /// <returns>Найденный элемент или null</returns>
    public RIItem FindError()
    {
      List<RIItem> items = new List<RIItem>();
      GetItems(items);
      for (int i = 0; i < items.Count; i++)
      {
        if (!String.IsNullOrEmpty(items[i].ErrorMessage))
          return items[i];
      }
      return null;
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Проверяет, что массив не null, его длина больше нуля, и все строки имеют ненулевую длину
    /// </summary>
    /// <param name="items">Проверяемый массив</param>
    /// <param name="zeroLengthAllowed">Если true, то допускается массив нулевой длины</param>
    internal static void CheckNotEmptyStringArray(string[] items, bool zeroLengthAllowed)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      if (!zeroLengthAllowed)
      {
        if (items.Length == 0)
          throw new ArgumentException("Items.Length=0", "items");
      }

      for (int i = 0; i < items.Length; i++)
      {
        if (String.IsNullOrEmpty(items[i]))
          throw new ArgumentNullException("Items[" + i.ToString() + "]");
      }
    }


    /// <summary>
    /// Запись свойства Lines. Используется в методах WriteChanges() для многострочного текста
    /// </summary>
    /// <param name="part">Секция для записи</param>
    /// <param name="name">Имя свойства</param>
    /// <param name="lines">Записываемык строки</param>
    protected void WriteChangeLines(CfgPart part, string name, string[] lines)
    {
      if (lines == null)
        lines = DataTools.EmptyStrings;
      part.SetInt(name + "_Count", lines.Length);
      for (int i = 0; i < lines.Length; i++)
        part.SetString(name + "_" + i.ToString(), lines[i]);
    }

    /// <summary>
    /// Чтение свойства Lines. Используется в методах ReadChanges() для многострочного текста
    /// </summary>
    /// <param name="part">Секция для чтения</param>
    /// <param name="name">Имя свойства</param>
    /// <returns>Массив строк</returns>
    protected string[] ReadChangeLines(CfgPart part, string name)
    {
      int n = part.GetInt(name + "_Count");
      if (n == 0)
        return DataTools.EmptyStrings;
      string[] lines = new string[n];
      for (int i = 0; i < n; i++)
        lines[i] = part.GetString(name + "_" + i.ToString());
      return lines;
    }


    #endregion
  }

  /// <summary>
  /// Интерфейс обработки удаленного пользовательского интерфейса
  /// </summary>
  public interface IRemoteInterface
  {
    #region Методы показа диалогов

    /// <summary>
    /// Вывести блок диалога, созданный в пользовательском коде и дождаться его завершения.
    /// После завершения следует проверить результат на равенство Ok
    /// </summary>
    /// <param name="dialog">Созданный блок диалога</param>
    /// <returns>Результат завершения (Ok или Cancel)</returns>
    DialogResult ShowDialog(Dialog dialog);

    /// <summary>
    /// Показать стандартный блок диалога
    /// После завершения следует проверить результат на равенство Ok
    /// </summary>
    /// <param name="dialog">Блок диалога</param>
    /// <returns>Результат завершения (Ok или Cancel)</returns>
    DialogResult ShowDialog(StandardDialog dialog);

    /// <summary>
    /// Вывести информационное сообщение без заголовка
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    void MessageBox(string text);

    /// <summary>
    /// Вывести информационное сообщение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Текст заголовка</param>
    void MessageBox(string text, string caption);

    /// <summary>
    /// Вывести сообщение с заданным набором кнопок
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Текст заголовка</param>
    /// <param name="buttons">Какой набор кнопок использовать</param>
    /// <returns>Результат завершения блока диалога</returns>
    DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons);

    /// <summary>
    /// Вывести сообщение с заданным набором кнопок и значком
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Текст заголовка</param>
    /// <param name="buttons">Какой набор кнопок использовать</param>
    /// <param name="icon">Значок</param>
    /// <returns>Результат завершения блока диалога</returns>
    DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

    /// <summary>
    /// Вывести сообщение с заданным набором кнопок и значком.
    /// Можно указать, какая кнопка будет активна по умолчанию
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Текст заголовка</param>
    /// <param name="buttons">Какой набор кнопок использовать</param>
    /// <param name="icon">Значок</param>
    /// <param name="defaultButton">Номер кнопки, выбранной по умолчанию</param>
    /// <returns>Результат завершения блока диалога</returns>
    DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton);

    /// <summary>
    /// Вывести сообщение об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    void ErrorMessageBox(string text);

    /// <summary>
    /// Вывести сообщение об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Текст заголовка</param>
    void ErrorMessageBox(string text, string caption);

    /// <summary>
    /// Вывести сообщение об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    void WarningMessageBox(string text);

    /// <summary>
    /// Вывести сообщение об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Текст заголовка</param>
    void WarningMessageBox(string text, string caption);

    #endregion
  }

  /// <summary>
  /// Переходник для процедуры, используемый на вызывающей стороне
  /// </summary>
  public class RIExecProcCaller : MarshalByRefObject, IRemoteInterface
  {
    #region Конструктор

    /// <summary>
    /// Создать переходник без поддержки сохранения значений между вызовами
    /// </summary>
    /// <param name="execProc">Выполняемая процедура</param>
    public RIExecProcCaller(IExecProc execProc)
      : this(execProc, null)
    {
    }

    /// <summary>
    /// Создать переходник с возможностью поддержки сохранения значений между вызовами
    /// </summary>
    /// <param name="execProc">Выполняемая процедура</param>
    /// <param name="saver">Поставщик секций конфигурации для сохранения значений. Может быть null</param>
    public RIExecProcCaller(IExecProc execProc, IRIValueSaver saver)
    {
      if (execProc == null)
        throw new ArgumentNullException("execProc");
      _ExecProc = execProc;
      _Saver = saver;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылка на процедуру, выполняющую вызов. 
    /// </summary>
    public IExecProc ExecProc { get { return _ExecProc; } }
    private IExecProc _ExecProc;

    /// <summary>
    /// Интерфейс, используемый для чтения / записи значений между вызовами.
    /// Задается в конструкторе. Может быть null
    /// </summary>
    public IRIValueSaver Saver { get { return _Saver; } }
    private IRIValueSaver _Saver;

    #endregion

    #region IRemoteInterface Members

    /// <summary>
    /// Показывает пользовательский блок диалога.
    /// Проверяйте, что результат выполнения равен DialogResult=Ok после вызова метода.
    /// </summary>
    /// <param name="dialog">Диалог</param>
    /// <returns>Результат выполнения блока диалога</returns>
    public DialogResult ShowDialog(Dialog dialog)
    {
      if (dialog == null)
        throw new ArgumentNullException("dialog");

      dialog.SetFixed(); // блокируем добавление элементов
      dialog.ReadValues(Saver); // прочитали предыдущие значения

      while (true)
      {
        TempCfg cfg = new TempCfg();
        dialog.WriteChanges(cfg); // 21.10.2021 - для сброса RIItem.ErrorMessage

        NamedValues dispArgs = new NamedValues();
        dispArgs["Action"] = "ShowDialog";
        dispArgs["Dialog"] = dialog;
        NamedValues dispRes = _ExecProc.Execute(dispArgs);
        DialogResult dlgRes = (DialogResult)(dispRes["DialogResult"]);
        if (dlgRes == DialogResult.OK || dlgRes == DialogResult.Yes)
        {
          cfg = new TempCfg();
          cfg.AsXmlText = dispRes.GetString("Changes");
          dialog.ReadChanges(cfg);

          if (!dialog.Validate())
            continue;

          dialog.WriteValues(Saver);
        }

        return dlgRes;
      }
    }

    /// <summary>
    /// Показ одного из стандартный блоков диалога.
    /// Проверяйте, что результат выполнения равен DialogResult=Ok после вызова метода.
    /// </summary>
    /// <param name="dialog">Диалог</param>
    /// <returns>Результат выполнения блока диалога</returns>
    public DialogResult ShowDialog(StandardDialog dialog)
    {
      if (dialog == null)
        throw new ArgumentNullException("dialog");

      dialog.SetFixed(); // блокируем внесение изменений
      dialog.ReadValues(Saver);

      while (true)
      {
        TempCfg cfg = new TempCfg();
        dialog.WriteChanges(cfg); // 21.10.2021 - для сброса RIItem.ErrorMessage

        NamedValues dispArgs = new NamedValues();
        dispArgs["Action"] = "ShowStandardDialog";
        dispArgs["Dialog"] = dialog;
        NamedValues dispRes = _ExecProc.Execute(dispArgs);

        DialogResult dlgRes = (DialogResult)(dispRes["DialogResult"]);
        if (dlgRes == DialogResult.OK || dlgRes == DialogResult.Yes)
        {
          cfg = new TempCfg();
          cfg.AsXmlText = dispRes.GetString("Changes");
          dialog.ReadChanges(cfg);

          if (!dialog.Validate())
            continue;

          dialog.WriteValues(Saver);
        }
        return dlgRes;
      }
    }

    /// <summary>
    /// Вывод окна с сообщением, без заголовка, и кнопкой ОК.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void MessageBox(string text)
    {
      MessageBox(text, String.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывод окна с сообщением.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок блока диалога</param>
    public void MessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывод окна с сообщением.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок блока диалога</param>
    /// <param name="buttons">Кнопки для выбора</param>
    /// <returns>Результат выполнения диалога</returns>
    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons)
    {
      return MessageBox(text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывод окна с сообщением.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок блока диалога</param>
    /// <param name="buttons">Кнопки для выбора</param>
    /// <param name="icon">Значок</param>
    /// <returns>Результат выполнения диалога</returns>
    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
      return MessageBox(text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывод окна с сообщением.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок блока диалога</param>
    /// <param name="buttons">Кнопки для выбора</param>
    /// <param name="icon">Значок</param>
    /// <param name="defaultButton">Какая кнопка из <paramref name="buttons"/> должна быть активирована по умолчанию</param>
    /// <returns>Результат выполнения диалога</returns>
    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
    {
      NamedValues dispArgs = new NamedValues();
      dispArgs["Action"] = "MessageBox";

      //Args["Text"] = text;
      // 27.08.2019
      dispArgs["Lines"] = UITools.TextToLines(text);
      dispArgs["Caption"] = caption;
      dispArgs["Buttons"] = buttons;
      dispArgs["Icon"] = icon;
      dispArgs["DefaultButton"] = defaultButton;

      NamedValues dispRes = _ExecProc.Execute(dispArgs);
      return (DialogResult)(dispRes["DialogResult"]);
    }

    /// <summary>
    /// Вывод сообщения об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void ErrorMessageBox(string text)
    {
      MessageBox(text, Res.RI_Msg_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Вывод сообщения об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок блока диалога</param>
    public void ErrorMessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Вывод предупреждающего сообщения
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void WarningMessageBox(string text)
    {
      MessageBox(text, Res.RI_Msg_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Вывод предупреждающего сообщения.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок блока диалога</param>
    public void WarningMessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    #endregion
  }

  /// <summary>
  /// Заглушка реализации IRemoteInterface.
  /// Может либо выбрасывать исключения при всех вызовах, либо "нажимать кнопку Отмена" для всех вызовов
  /// </summary>
  public class RemoteInterfaceStub : IRemoteInterface
  {
    #region Конструктор

    /// <summary>
    /// Создает заглушку.
    /// </summary>
    /// <param name="throwExceptions">Если true, то будут выбрасываться исключения, если false - будет отправляться результат "Отмена"</param>
    public RemoteInterfaceStub(bool throwExceptions)
    {
      _ThrowExceptions = throwExceptions;
    }

    /// <summary>
    /// Создает заглушку, выбрасывающую исключения с заданным сообщением
    /// </summary>
    /// <param name="exceptionMessage">Сообщение об ошибке</param>
    public RemoteInterfaceStub(string exceptionMessage)
    {
      _ThrowExceptions = true;
      _ExceptionMessage = exceptionMessage;
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Если true, то будут выбрасываться исключения, если false - будет отправляться результат "Отмена"
    /// </summary>
    public bool ThrowExceptions { get { return _ThrowExceptions; } }
    private bool _ThrowExceptions;

    /// <summary>
    /// Текст сообщения в исключении.
    /// Если свойство не установлено, будет использовано сообщение по умолчанию.
    /// </summary>
    public string ExceptionMessage { get { return _ExceptionMessage; } set { _ExceptionMessage = value; } }
    private string _ExceptionMessage;

    #endregion

    #region IRemoteInterface Members

    private void DoThrowException()
    {
      if (ThrowExceptions)
      {
        if (String.IsNullOrEmpty(ExceptionMessage))
          throw new RemoteInterfaceNotsopportedException();
        else
          throw new RemoteInterfaceNotsopportedException(ExceptionMessage);
      }
    }

    DialogResult IRemoteInterface.ShowDialog(Dialog dialog)
    {
      DoThrowException();
      return DialogResult.Cancel;
    }

    DialogResult IRemoteInterface.ShowDialog(StandardDialog dialog)
    {
      DoThrowException();
      return DialogResult.Cancel;
    }

    void IRemoteInterface.MessageBox(string text)
    {
      DoThrowException();
    }

    void IRemoteInterface.MessageBox(string text, string caption)
    {
      DoThrowException();
    }

    DialogResult IRemoteInterface.MessageBox(string text, string caption, MessageBoxButtons buttons)
    {
      DoThrowException();
      return DialogResult.Cancel;
    }

    DialogResult IRemoteInterface.MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
      DoThrowException();
      return DialogResult.Cancel;
    }

    DialogResult IRemoteInterface.MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
    {
      DoThrowException();
      return DialogResult.Cancel;
    }

    void IRemoteInterface.ErrorMessageBox(string text)
    {
      DoThrowException();
    }

    void IRemoteInterface.ErrorMessageBox(string text, string caption)
    {
      DoThrowException();
    }

    void IRemoteInterface.WarningMessageBox(string text)
    {
      DoThrowException();
    }

    void IRemoteInterface.WarningMessageBox(string text, string caption)
    {
      DoThrowException();
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, если удаленный интерфейс не поддерживается
  /// </summary>
  [Serializable]
  public class RemoteInterfaceNotsopportedException : NotSupportedException
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public RemoteInterfaceNotsopportedException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с заданным вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public RemoteInterfaceNotsopportedException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает новый объект исключения со стандартным сообщением
    /// </summary>
    public RemoteInterfaceNotsopportedException()
      : base(Res.RemoteInterfaceNotsopportedException_Err_Default)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected RemoteInterfaceNotsopportedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Выполняемая процедура, используемая на вызываемой стороне.
  /// Объекты этого класса не должны использоваться в прикладном коде.
  /// Процедура является "многоразовой" (AutoDispose=false) и позволяет получать для нее ExecProcProxy многократно.
  /// Используется в ExtForms для реализации свойства EFPApp.RemoteUIProc
  /// </summary>
  public class RIExecProc : ExecProc
  {
    #region Конструкторы

    /// <summary>
    /// Создает процедуру со ссылкой на интерфейс
    /// </summary>
    /// <param name="ui">Интерфейс удаленного доступа</param>
    public RIExecProc(IRemoteInterface ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");

      base.AutoDispose = false;

      NamedValues ctx = new NamedValues();
      ctx.Add("UI", ui);
      base.SetContext(ctx);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к удаленному интерфейсу
    /// </summary>
    public IRemoteInterface UI
    {
      get
      {
        return base.Context["UI"] as IRemoteInterface;
      }
    }

    /// <summary>
    /// Если установлено, то выполняется callback-вызов для проверки диалога
    /// </summary>
    public IExecProcCallBack CallBackProc { get { return _CallBackProc; } set { _CallBackProc = value; } }
    private IExecProcCallBack _CallBackProc;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполнение процедуры
    /// </summary>
    /// <param name="args">Аргументы</param>
    /// <returns>Результат выполнения</returns>
    protected override NamedValues OnExecute(NamedValues args)
    {
      if (UI == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "UI");
      NamedValues res = ExecuteInterfaceCall(args, UI);
      if (res == null)
        throw new InvalidOperationException(String.Format(Res.RI_Err_UnknownAction, args.GetString("Action")));
      return res;
    }

    /// <summary>
    /// Этот метод можно использовать, если команды интерфейса передаются не через выделенную процедуру.
    /// Обрабатывается строковый аргумент "Action" для определения необходимого действия.
    /// Если действие не может быть обработано, метод возвращает null. Это значение следует обрабатывать,
    /// генерируя исключение
    /// </summary>
    /// <param name="dispArgs"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    private static NamedValues ExecuteInterfaceCall(NamedValues dispArgs, IRemoteInterface ui)
    {
      NamedValues dispRes;
      DialogResult dlgRes;
      switch (dispArgs.GetString("Action"))
      {
        case "ShowDialog":
          Dialog dlg1 = dispArgs["Dialog"] as Dialog;
          if (dlg1 == null)
            throw new NullReferenceException(String.Format(Res.RI_Err_ArgumentNotSet, "Dialog"));
          dlg1.SetFixed();

          dispRes = new NamedValues();
          dlgRes = ui.ShowDialog(dlg1);
          dispRes["DialogResult"] = dlgRes;
          if (dlgRes == DialogResult.OK || dlgRes == DialogResult.Yes) // 06.12.2021
          {
            TempCfg cfg1 = new TempCfg();
            dlg1.WriteChanges(cfg1);
            dispRes["Changes"] = cfg1.AsXmlText;
          }
          return dispRes;
        case "ShowStandardDialog":
          StandardDialog dlg2 = dispArgs["Dialog"] as StandardDialog;
          if (dlg2 == null)
            throw new NullReferenceException(String.Format(Res.RI_Err_ArgumentNotSet, "Dialog"));
          dlg2.SetFixed();

          dispRes = new NamedValues();
          dlgRes = ui.ShowDialog(dlg2);
          dispRes["DialogResult"] = dlgRes;
          if (dlgRes == DialogResult.OK || dlgRes == DialogResult.Yes) // 06.12.2021
          {
            TempCfg cfg2 = new TempCfg();
            dlg2.WriteChanges(cfg2);
            dispRes["Changes"] = cfg2.AsXmlText;
          }
          return dispRes;
        case "MessageBox":
          dispRes = new NamedValues();

          string text = UITools.LinesToText((string[])dispArgs["Lines"]); // 27.08.2019
          dispRes["DialogResult"] = ui.MessageBox(text,
            dispArgs.GetString("Caption"),
            (MessageBoxButtons)(dispArgs["Buttons"]),
            (MessageBoxIcon)(dispArgs["Icon"]),
            (MessageBoxDefaultButton)(dispArgs["DefaultButton"]));
          return dispRes;
        default:
          return null; // не наше сообщение
      }
    }

    #endregion
  }
}
