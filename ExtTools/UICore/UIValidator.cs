// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Collections;
using System.ComponentModel;

// Общие объявления пользовательского интерфейса.
// Используется классами FreeLibSet.RI.Control (удаленный пользовательский интерфейс)
// и FreeLibSet.Forms.EFPControlBase (провайдеры Windows Forms)
namespace FreeLibSet.UICore
{
  #region Перечисление UIValidateState

  /// <summary>
  /// Результат проверки ошибок
  /// </summary>
  [Serializable]
  public enum UIValidateState
  {
    // Числовые значения совпадают с UIValidateState в ExtForms.dll

    /// <summary>
    /// Ошибок не найдено
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Предупреждение
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Ошибка
    /// </summary>
    Error = 2
  }

  #endregion

  #region Интерфейс объекта, поддерживающего проверку ошибок

  /// <summary>
  /// Интерфейс объекта, поддерживающего проверку ошибок.
  /// Реализуется EFPControlBase, EFPValidatingEventArgs и некоторыми другими классами.
  /// </summary>
  public interface IUIValidableObject
  {
    /// <summary>
    /// Установить ошибку
    /// </summary>
    /// <param name="message">Сообщение</param>
    void SetError(string message);

    /// <summary>
    /// Установить предупреждение.
    /// </summary>
    /// <param name="message">Сообщение</param>
    void SetWarning(string message);

    /// <summary>
    /// Получить текущее состояние проверки
    /// </summary>
    UIValidateState ValidateState { get; }
  }

  /// <summary>
  /// Простой класс для реализации одноразовой проверки
  /// </summary>
  public class UISimpleValidableObject : IUIValidableObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public UISimpleValidableObject()
    {
      Clear();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Устанавливает состояние Ok
    /// </summary>
    public void Clear()
    {
      _ValidateState = UIValidateState.Ok;
      _Message = null;
    }

    /// <summary>
    /// Устанавливает состояние ошибки.
    /// Если уже установлена ошибка, вызов игнорируется.
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public void SetError(string message)
    {
      if (ValidateState != UIValidateState.Error)
      {
        _ValidateState = UIValidateState.Error;
        _Message = message;
      }
    }

    /// <summary>
    /// Устанавливает предупреждение
    /// Если уже установлена ошибка или предупреждение, вызов игнорируется.
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public void SetWarning(string message)
    {
      if (ValidateState == UIValidateState.Ok)
      {
        _ValidateState = UIValidateState.Warning;
        _Message = message;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее состояние
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    /// <summary>
    /// Текст сообщения об ошибке или предупреждения
    /// </summary>
    public string Message { get { return _Message; } }
    private string _Message;

    #endregion
  }

  #endregion

  #region Делегат для проверки ошибок

  /// <summary>
  /// Аргумент для события проверки
  /// </summary>
  public class UIValidatingEventArgs : IUIValidableObject
  {
    /// <summary>
    /// Создание объекта
    /// </summary>
    /// <param name="validableObject">Объект, запросивший проверку. Ему передаются сообщения об ошибке или предупреждении</param>
    public UIValidatingEventArgs(IUIValidableObject validableObject)
    {
      _ValidableObject = validableObject;
    }

    /// <summary>
    /// Объект, запросивший проверку. Ему передаются сообщения об ошибке или предупреждении
    /// </summary>
    public IUIValidableObject ValidableObject { get { return _ValidableObject; } }
    private IUIValidableObject _ValidableObject;

    /// <summary>
    /// Установить сообщение об ошибке
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void SetError(string message)
    {
      _ValidableObject.SetError(message);
    }

    /// <summary>
    /// Установить предупреждение
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void SetWarning(string message)
    {
      _ValidableObject.SetWarning(message);
    }

    /// <summary>
    /// Определить текущее состояние проверки: наличие ошибок или предупреждений
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidableObject.ValidateState; } }

    /// <summary>
    /// Вспомогательный метод, вызывающий SetError() или SetWarning()
    /// </summary>
    /// <param name="state">Состояние</param>
    /// <param name="message">Сообщение</param>
    public void SetState(UIValidateState state, string message)
    {
      if (ValidateState == UIValidateState.Error)
        return;

      switch (state)
      {
        case UIValidateState.Error:
          SetError(message);
          break;
        case UIValidateState.Warning:
          if (ValidateState == UIValidateState.Ok)
            SetWarning(message);
          break;
      }
    }
  }

  /// <summary>
  /// Делегат события проверки
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void UIValidatingEventHandler(object sender, UIValidatingEventArgs args);

  #endregion

  /// <summary>
  /// Структура для хранения результата вычисления проверочного выражения и сообщения об ошибке
  /// </summary>
  [Serializable]
  public struct UIValidateResult : IEquatable<UIValidateResult>
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="isValid">true - правильное значение, false - ошибка</param>
    /// <param name="message">Сообщение об ошибке. При <paramref name="isValid"/>=true игнорируется</param>
    public UIValidateResult(bool isValid, string message)
    {
      if (isValid)
        _Message = null;
      else if (String.IsNullOrEmpty(message))
        _Message = "Текст сообщения об ошибке не задан";
      else
        _Message = message;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Сообщение об ошибке или null.
    /// </summary>
    private string _Message;

    /// <summary>
    /// Результат проверки. True - правильное значение. False - есть ошибка
    /// </summary>
    public bool IsValid { get { return Object.ReferenceEquals(_Message, null); } }

    /// <summary>
    /// Текст сообщения об ошибке при IsValid=false.
    /// </summary>
    public string Message
    {
      get
      {
        if (Object.ReferenceEquals(_Message, null))
          return String.Empty;
        else
          return _Message;
      }
    }

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (IsValid)
        return "Ok";
      else
        return _Message;
    }

    #endregion

    #region Сравнение двух объектов

    /// <summary>
    /// Сравнение двух объектов.
    /// </summary>
    /// <param name="a">Первый сравниваемый объект</param>
    /// <param name="b">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(UIValidateResult a, UIValidateResult b)
    {
      return String.Equals(a._Message, b._Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Сравнение двух объектов.
    /// </summary>
    /// <param name="a">Первый сравниваемый объект</param>
    /// <param name="b">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(UIValidateResult a, UIValidateResult b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Сравнение с другим объектом.
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(UIValidateResult other)
    {
      return String.Equals(this._Message, other._Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Сравнение с другим объектом.
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is UIValidateResult)
        return (this == (UIValidateResult)other);
      else
        return false;
    }

    /// <summary>
    /// Хэш код. Требуется для подавления предупреждения компилятора.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      if (Object.ReferenceEquals(_Message, null))
        return 0;
      else
        return _Message.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// Описание для одного объекта валидации, присоединенного к управляющему элементу.
  /// Валидаторы могут добавляться к списку Control.Validators.
  /// 
  /// Класс однократной записи.
  /// </summary>
  [Serializable]
  public sealed class UIValidator
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="resultEx">Выражение валидации</param>
    /// <param name="isError">true-ошибка, false-предупреждение</param>
    /// <param name="preconditionEx">Выражение, определяющее необходимость выполнения проверки. Может быть null, если проверка выполняется всегда</param>
    public UIValidator(DepValue<UIValidateResult> resultEx, bool isError, DepValue<bool> preconditionEx)
    {
      if (resultEx == null)
        throw new ArgumentNullException("resultEx");

      _ResultEx = resultEx;
      resultEx.ValueChanged += DummyValueChanged; // Чтобы IsConnected возвращало true
      _IsError = isError;
      _PreconditionEx = preconditionEx;
      if (preconditionEx != null)
        preconditionEx.ValueChanged += DummyValueChanged; // Чтобы IsConnected возвращало true
    }

    static void DummyValueChanged(object sender, EventArgs args)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вычисляемое выражение для выполнения проверки.
    /// Если в вычисленном значении IsValid равно false, то для управляющего элемента будет выдано сообщение об ошибке или предупреждение,
    /// в зависимости от свойства IsError.
    /// Не может быть null.
    /// </summary>
    public DepValue<UIValidateResult> ResultEx { get { return _ResultEx; } }
    private readonly DepValue<UIValidateResult> _ResultEx;

    /// <summary>
    /// True, если нарушение условия является ошибкой, false-если предупреждением.
    /// Если хотя бы один из объектов с IsError=true не проходит валидацию, нельзя закрыть блок диалога нажатием кнопки "ОК".
    /// Предупреждения не препятствуют закрытию диалога.
    /// </summary>
    public bool IsError { get { return _IsError; } }
    private readonly bool _IsError;

    /// <summary>
    /// Необязательное предусловие для выполнения проверки. Если выражение возвращает true, то проверка выполняется,
    /// если false - то отключается.
    /// Если свойство не установлено (обычно), то проверка выполняется.
    /// </summary>
    public DepValue<bool> PreconditionEx { get { return _PreconditionEx; } }
    private readonly DepValue<bool> _PreconditionEx;

    /// <summary>
    /// Возвращает свойство UIValidateResult.Message или "Ok" (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ResultEx.Value.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства RIItem.Validators
  /// </summary>
  [Serializable]
  public class UIValidatorList : ListWithReadOnly<UIValidator>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public UIValidatorList()
    {
    }

    #endregion

    #region Методы создания валидаторов

    #region AddError()

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="resultEx">Выражение валидации</param>
    /// <param name="preconditionEx">Выражение предусловия. Может быть null, если проверка выполняется всегда</param>
    public UIValidator AddError(DepValue<UIValidateResult> resultEx, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(resultEx, true, preconditionEx);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="resultEx">Выражение валидации</param>
    public UIValidator AddError(DepValue<UIValidateResult> resultEx)
    {
      UIValidator item = new UIValidator(resultEx, true, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), true, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="preconditionEx">Выражение предусловия. Может быть null, если проверка выполняется всегда</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), true, preconditionEx);
      Add(item);
      return item;
    }

    #endregion

    #region AddWarning()

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="resultEx">Выражение валидации</param>
    /// <param name="preconditionEx">Выражение предусловия. Может быть null, если проверка выполняется всегда</param>
    public UIValidator AddWarning(DepValue<UIValidateResult> resultEx, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(resultEx, false, preconditionEx);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="resultEx">Выражение валидации</param>
    public UIValidator AddWarning(DepValue<UIValidateResult> resultEx)
    {
      UIValidator item = new UIValidator(resultEx, false, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    public UIValidator AddWarning(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), false, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="preconditionEx">Выражение предусловия. Может быть null, если проверка выполняется всегда</param>
    public UIValidator AddWarning(DepValue<bool> expressionEx, string message, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), false, preconditionEx);
      Add(item);
      return item;
    }

    #endregion

    #endregion

    #region Проверка

    /// <summary>
    /// Выполнить проверку валидаторов в списке и поместить сообщение об ошибке или предупреждении в <paramref name="validableObject"/>.
    /// Валидаторы, для которых PreconditionEx возвращает false, пропускается.
    /// Проверка заканчивается, если очережной валидатор установил сообщение об ошибке.
    /// Этот метод не используется в прикладном коде.
    /// </summary>
    /// <param name="validableObject">Реализация интерфейса для добавления сообщений.</param>
    public void Validate(IUIValidableObject validableObject)
    {
#if DEBUG
      if (validableObject == null)
        throw new ArgumentNullException("validableObject");
#endif

      if (validableObject.ValidateState == UIValidateState.Error)
        return;

      for (int i = 0; i < Count; i++)
      {
        if (this[i].PreconditionEx != null)
        {
          if (!this[i].PreconditionEx.Value)
            continue;
        }
        if (!this[i].ResultEx.Value.IsValid)
        {
          if (this[i].IsError)
          {
            validableObject.SetError(this[i].ResultEx.Value.Message);
            break;
          }
          else
            validableObject.SetWarning(this[i].ResultEx.Value.Message);
        }
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Переводит список в режим "Только чтение".
    /// Метод не должен использоваться в прикладном коде
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Пустой список валидаторов.
    /// Свойство списка IsReadOnly=true.
    /// </summary>
    public static readonly UIValidatorList Empty = CreateEmpty();

    private static UIValidatorList CreateEmpty()
    {
      UIValidatorList obj = new UIValidatorList();
      obj.SetReadOnly();
      return obj;
    }

    #endregion
  }

  /// <summary>
  /// Список валидаторов, предназначенных для проверки одного значения из списка.
  /// Расширяет список UIValidatorList свойством ValueEx.
  /// </summary>
  [Serializable]
  public class UIValueValidatorList<T> : UIValidatorList
  {
    #region ValueEx

    /// <summary>
    /// Управляемое свойство, возвращающее текущее проверяемое значение.
    /// </summary>
    public DepValue<T> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
    }
    private DepInput<T> _ValueEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства ValueEx присоединен к другим объектам в качестве входа.
    /// Это свойство не предназначено для использования в пользовательском коде
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool InternalValueExConnected
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
        _ValueEx = new DepInput<T>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
      }
    }

    /// <summary>
    /// Этот метод не предназначен для использования в пользовательском коде
    /// </summary>
    /// <param name="value"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void InternalSetValueEx(DepValue<T> value)
    {
      InitValueEx();
      _ValueEx.Source = value;
    }

    /// <summary>
    /// Этот метод не предназначен для использования в пользовательском коде
    /// </summary>
    /// <param name="value"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void InternalSetValue(T value)
    {
      InitValueEx();
      _ValueEx.Value = value;
    }

    #endregion
  }

  #region Интерфейсы управляющих элементов

#if XXX
  /// <summary>
  /// Интерфейс управляющего элемента
  /// </summary>
  public interface IUIControl
  {
    /// <summary>
    /// Доступность элемента
    /// </summary>
    bool Enabled { get; set;}

    /// <summary>
    /// Управляемое свойство для Enabled
    /// </summary>
    DepValue<bool> EnabledEx { get;set;}

    /// <summary>
    /// Не предназначено для использования в прикладном коде
    /// </summary>
    bool EnabledExConnected { get;}

    /// <summary>
    /// Проверяющие объекты
    /// </summary>
    UIValidatorList Validators { get;}
  }

  /// <summary>
  /// Управляющий элемент, поддерживающий проверку наличия значения
  /// </summary>
  public interface IUIControlCanBeEmpty : IUIControl
  {
    /// <summary>
    /// True, если элемент может содержать пустое значение.
    /// По умолчанию - false.
    /// </summary>
    bool CanBeEmpty { get;set;}

    /// <summary>
    /// Расширение для свойства CanBeEmpty, поддерживающее выдачу предупреждений
    /// </summary>
    UIValidateState CanBeEmptyMode { get;set;}
  }

  /// <summary>
  /// Управляющий элемент, поддерживающий просмотр значений (свойство ReadOnly)
  /// </summary>
  public interface IUIControlWithReadOnly : IUIControl
  {
    /// <summary>
    /// True, если элемент предназначен только для просмотра значений
    /// </summary>
    bool ReadOnly { get;set;}

    /// <summary>
    /// Управляемое свойство для ReadOnly
    /// </summary>
    DepValue<bool> ReadOnlyEx { get;set;}
  }
#endif
  #endregion
}
