using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Collections;

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
    /// <param name="expression">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isError">true-ошибка, false-предупреждение</param>
    public UIValidator(DepValue<bool> expression, string message, bool isError)
      : this(expression, message, isError, null)
    {
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="expression">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isError">true-ошибка, false-предупреждение</param>
    /// <param name="activeEx">Выражение, определяющее необходимость выполнения проверки. Может быть null, если проверка выполняется всегда</param>
    public UIValidator(DepValue<bool> expression, string message, bool isError, DepValue<bool> activeEx)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(message))
        throw new ArgumentNullException("message");

      _Expression = expression;
      _Message = message;
      _IsError = isError;
      _ActiveEx = activeEx;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение для выполнения проверки.
    /// Вычисление должно возвращать true, если условие валидации выполнено.
    /// Если вычисленное значение равно false, то для управляющего элемента будет выдано сообщение об ошибке или предупреждение,
    /// в зависимости от свойства IsError.
    /// </summary>
    public DepValue<bool> Expression { get { return _Expression; } }
    private readonly DepValue<bool> _Expression;

    /// <summary>
    /// Сообщение, которое будет выдано, если результатом вычисления Expression является false.
    /// </summary>
    public string Message { get { return _Message; } }
    private readonly string _Message;

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
    public DepValue<bool> ActiveEx { get { return _ActiveEx; } }
    private readonly DepValue<bool> _ActiveEx;

    /// <summary>
    /// Возвращает свойство Message (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Message;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства RIItem.Validators
  /// </summary>
  [Serializable]
  public sealed class UIValidatorList : ListWithReadOnly<UIValidator>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public UIValidatorList()
    {
    }

    #endregion

    #region Методы

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="expression">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    public UIValidator AddError(DepValue<bool> expression, string message)
    {
      UIValidator item = new UIValidator(expression, message, true);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="expression">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    public UIValidator AddWarning(DepValue<bool> expression, string message)
    {
      UIValidator item = new UIValidator(expression, message, false);
      Add(item);
      return item;
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
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
