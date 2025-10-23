// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FreeLibSet.Data
{
  // Классы исключений, используемые в модуле ExtDB

  /// <summary>
  /// Исключение выбрасывается в методах GetValue() и GetValues(), если не найдена строка с заданным идентификатором.
  /// Используется только в перегрузках методов, которые принимают одиночный идентификатор записи
  /// </summary>
  [Serializable]
  public class DBxRecordNotFoundException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение с заданными параметрами и сообщением
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполнялся поиск</param>
    /// <param name="id">Идентификатор не найденной записи, приведенный к типу <see cref="String"/></param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное сообщение</param>
    public DBxRecordNotFoundException(string tableName, string id, string message, Exception innerException)
      : base(message, innerException)
    {
      _TableName = tableName; // 25.12.2020
      _Id = id;
    }

    /// <summary>
    /// Создает исключение с заданными параметрами и сообщением
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполнялся поиск</param>
    /// <param name="id">Идентификатор не найденной записи, приведенный к типу <see cref="String"/></param>
    /// <param name="message">Текст сообщения</param>
    public DBxRecordNotFoundException(string tableName, string id, string message)
      : base(message)
    {
      _TableName = tableName;
      _Id = id;
    }

    /// <summary>
    /// Создает исключение с заданными параметрами.
    /// Текст сообщения
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполнялся поиск</param>
    /// <param name="id">Идентификатор не найденной записи, приведенный к типу <see cref="String"/></param>
    public DBxRecordNotFoundException(string tableName, string id)
      : base(CreateMessage(tableName, id))
    {
      _TableName = TableName;
      _Id = id;
    }

    private static string CreateMessage(string tableName, string id)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      if (String.IsNullOrEmpty(id))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("id");

      return String.Format(Res.DBxRecordNotFoundException_Err_Message, tableName, id);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя таблицы, в которой выполнялся поиск
    /// </summary>
    public string TableName { get { return _TableName; } }
    private readonly string _TableName;

    /// <summary>
    /// Идентификатор не найденной записи.
    /// Свойство имеет тип string, а не Int32 на случай будущей поддержки первичных ключей, отличных от Int32
    /// </summary>
    public string Id { get { return _Id; } }
    private readonly string _Id;

    #endregion

    #region Сериализация

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DBxRecordNotFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _TableName = info.GetString("TableName");
      _Id = info.GetString("Id");
    }

    /// <summary>
    /// Используется при сериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("TableName", _TableName);
      info.AddValue("Id", _Id);
    }

    #endregion
  }


  /// <summary>
  /// Исключение выбрасывается при обнаружении неправильных значений полей в базе данных, например, несоответствие контрольной суммы
  /// </summary>
  [Serializable]
  public class DBxConsistencyException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public DBxConsistencyException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public DBxConsistencyException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Используется при десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DBxConsistencyException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
