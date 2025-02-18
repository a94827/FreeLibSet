// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Специальный тип исключения, генерируемого, если пользователь прерывает
  /// длительный процесс.
  /// </summary>
  [Serializable]
  public class UserCancelException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию
    /// </summary>
    public UserCancelException()
      : base(Res.Common_Err_UserCancel)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected UserCancelException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Специальный тип исключения для указания ошибки в программе. 
  /// В конструктор передается описание ошибки. В текст исключения <see cref="Exception.Message"/> добавляется
  /// информация о необходимости обратиться к разработчику программы.
  /// </summary>
  [Serializable]
  public class BugException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение. К нему будет добавлен дополнительный текст</param>
    public BugException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с заданным вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение. К нему будет добавлен дополнительный текст</param>
    /// <param name="innerException">Вложенное исключение</param>
    public BugException(string message, Exception innerException)
      : base(String.Format(Res.Common_Err_BugWithMessage, message), innerException)
    {
    }

    /// <summary>
    /// Создает новый объект исключения со стандартным сообщением
    /// </summary>
    public BugException()
      : base(Res.Common_Err_Bug)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected BugException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, когда объект находится в режиме "Только для чтения"
  /// </summary>
  [Serializable]
  public class ObjectReadOnlyException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public ObjectReadOnlyException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public ObjectReadOnlyException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает исключение с текстом сообщения по умолчанию
    /// </summary>
    public ObjectReadOnlyException()
      : base(Res.Common_Err_ObjectReadOnly)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected ObjectReadOnlyException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, когда выполняется попытка вызова метода из чужого потока
  /// </summary>
  [Serializable]
  public class DifferentThreadException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public DifferentThreadException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public DifferentThreadException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию
    /// </summary>
    public DifferentThreadException()
      : base(String.Format(Res.Common_Err_DifferentThread, ToString(Thread.CurrentThread)))
    {
    }

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию.
    /// </summary>
    /// <param name="wantedThread">Поток, вызов из которого ожидался</param>
    public DifferentThreadException(Thread wantedThread)
      : base(String.Format(Res.Common_Err_DifferentThreadWithWanted, ToString(Thread.CurrentThread), ToString(wantedThread)))
    {
    }

    private static string ToString(Thread thread)
    {
      return "{ManagedThreadId=" + thread.ManagedThreadId + ", Name=" + thread.Name + "}";
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DifferentThreadException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, когда выполняется попытка вызова метода из чужого потока
  /// </summary>
  [Serializable]
  public class ReenteranceException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public ReenteranceException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public ReenteranceException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию
    /// </summary>
    public ReenteranceException()
      : base(Res.Common_Err_Reentrance)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected ReenteranceException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Класс исключения с присоединенным к нему списком ошибок
  /// </summary>
  [Serializable]
  public class ErrorMessageListException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создать объект исключения
    /// </summary>
    /// <param name="errors">Присоединяемый список сообщений. Список переводится в режим только для чтения. 
    /// Если null, то используется пустой список <see cref="ErrorMessageList.Empty"/>.</param>
    /// <param name="message">Основной текст исключения</param>
    public ErrorMessageListException(ErrorMessageList errors, string message)
      : base(message)
    {
      if (errors == null)
        _Errors = ErrorMessageList.Empty;
      else
      {
        errors.SetReadOnly();
        _Errors = errors;
      }
    }

    /// <summary>
    /// Создать объект исключения. Текст исключения генерируется автоматически
    /// </summary>
    /// <param name="errors">Присоединяемый список сообщений. Список переводится в режим только для чтения. 
    /// Если null, то используется пустой список <see cref="ErrorMessageList.Empty"/>.</param>
    public ErrorMessageListException(ErrorMessageList errors)
      : base(GetMessageText(errors))
    {
      if (errors == null)
        _Errors = ErrorMessageList.Empty;
      else
      {
        errors.SetReadOnly();
        _Errors = errors;
      }
    }

    private static string GetMessageText(ErrorMessageList errors)
    {
      if (errors == null)
        return Res.Common_Err_EmptyErrorMessageList;
      else
        return errors.ToString();
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected ErrorMessageListException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _Errors = (ErrorMessageList)(info.GetValue("Errors", typeof(ErrorMessageList)));
    }

    /// <summary>
    /// Используется при сериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Errors", _Errors);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список сообщений об ошибках. 
    /// Задается в конструкторе и находится в режиме только для чтения.
    /// </summary>
    public ErrorMessageList Errors { get { return _Errors; } }
    private readonly ErrorMessageList _Errors;

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, когда система занята
  /// </summary>
  [Serializable]
  public class BusyException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public BusyException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public BusyException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает новый объект исключения со стандартным сообщением
    /// </summary>
    public BusyException()
      : base(Res.Common_Err_Busy)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected BusyException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Исключение, выбрасываемое при попытке добавления в коллекцию элемента, когда достигнут лимит MaxCount
  /// </summary>
  [Serializable]
  public class MaxCountException : InvalidOperationException
  {
  #region Конструктор

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public MaxCountException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public MaxCountException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected MaxCountException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

  #endregion
  }
#endif

  /// <summary>
  /// Исключение, выбрасываемое, когда не удалось установить блокировку объекта за определенное время
  /// </summary>
  [Serializable]
  public class LockTimeoutException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public LockTimeoutException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public LockTimeoutException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected LockTimeoutException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, когда не удалось выполнить разбор строки.
  /// Кроме подсистемы парсинга, используется при разборе CSV-строк.
  /// </summary>
  [Serializable]
  public class ParsingException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public ParsingException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public ParsingException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected ParsingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

#if DEBUG

  /// <summary>
  /// Специальный тип исключения для тестовых целей. 
  /// Класс существует только в отладочной версии библиотеки.
  /// </summary>
  [Serializable]
  public class TestException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public TestException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с заданным вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public TestException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает новый объект исключения со стандартным сообщением
    /// </summary>
    public TestException()
      : base(Res.Common_Err_Test)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected TestException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

#endif
}
