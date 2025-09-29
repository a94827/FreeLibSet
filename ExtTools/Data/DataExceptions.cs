// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Исключение, когда у таблицы нет первичного ключа, или он имеет неподходящий тип
  /// </summary>
  [Serializable]
  public class PrimaryKeyException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение с сообщением "Неправильный первичный ключ"
    /// </summary>
    public PrimaryKeyException()
      : base(Res.PrimaryKeyException_Err_Message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public PrimaryKeyException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public PrimaryKeyException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Используется при десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected PrimaryKeyException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение выбрасывается, если задан нулевой идентификатор строки
  /// </summary>
  [Serializable]
  public class NoIdArgumentException : ArgumentException
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение с сообщением "Не задан идентификатор"
    /// </summary>
    public NoIdArgumentException()
      : base(Res.NoIdArgumentException_Err_Message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public NoIdArgumentException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="paramName">Имя параметра</param>
    public NoIdArgumentException(string message, string paramName)
      : base(message, paramName)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="paramName">Имя параметра</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public NoIdArgumentException(string message, string paramName, Exception innerException)
      : base(message, paramName, innerException)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public NoIdArgumentException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Используется при десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected NoIdArgumentException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение генерируется, если обнаружена ошибка в заполнении объекта <see cref="DBxStruct"/>
  /// </summary>
  [Serializable]
  public class DBxStructException : Exception
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект исключения
    /// </summary>
    /// <param name="table">Описание структуры таблицы, содержащее ошибку</param>
    /// <param name="message">Текст сообщения</param>
    public DBxStructException(DBxTableStruct table, string message)
      : base(String.Format(Res.DBxStructException_Err_Message,
        table.TableName, message))
    {
      _Table = table;
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DBxStructException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _Table = (DBxTableStruct)(info.GetValue("Table", typeof(DBxTableStruct)));
    }

    /// <summary>
    /// Используется при сериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Table", _Table);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описание структуры таблицы, содержащее ошибку
    /// </summary>
    public DBxTableStruct Table { get { return _Table; } }
    private DBxTableStruct _Table;

    #endregion
  }
}
