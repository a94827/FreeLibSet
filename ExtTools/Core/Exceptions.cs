using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;

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

namespace FreeLibSet.Core
{
  /// <summary>
  /// Специальный тип исключения, генерируемого, если пользователь прерывает
  /// длительный процесс
  /// </summary>
  [Serializable]
  public class UserCancelException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию
    /// </summary>
    public UserCancelException()
      : base("Выполнение прервано пользователем")
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
  /// В конструктор передается описание ошибки. В текст исключения Message добавляется
  /// информация о необходимости обратиться к разработчику программы
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
      : base("Внутренняя ошибка в программе. " + message + ". Обратитесь к разработчику", innerException)
    {
    }

    /// <summary>
    /// Создает новый объект исключения со стандартным сообщением
    /// </summary>
    public BugException()
      : base("Внутренняя ошибка в программе. Обратитесь к разработчику")
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
      : base("Объект заблокирован от внесения изменений (ReadOnly)")
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
      : base("Вызов из чужого потока "+ToString(Thread.CurrentThread))
    {
    }

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию.
    /// </summary>
    /// <param name="wantedThread">Поток, вызов из которого ожидался</param>
    public DifferentThreadException(Thread wantedThread)
      : base("Вызов из чужого потока " + ToString(Thread.CurrentThread) + ". Ожидался вызов из потока " + ToString(wantedThread))
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
      : base("Вложенный вызов процедуры")
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
    /// <param name="errors">Присоединяемый список сообщений. Список переводится в режим ReadOnly. Если null, то используется пустой список</param>
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
    /// <param name="errors">Присоединяемый список сообщений. Список переводится в режим ReadOnly. Если null, то используется пустой список</param>
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
        return "Нет ошибок";
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
    /// Задается в конструкторе и находится в режиме ReadOnly
    /// </summary>
    public ErrorMessageList Errors { get { return _Errors; } }
    private ErrorMessageList _Errors;

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
      : base("Приложение занято")
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
  /// Кроме подсистемы Parsing, используется при разборе CSV-строк.
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
      : base("Тестовое исключение")
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
