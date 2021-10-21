using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Data
{
  // Классы исключений, используемые в модуле ExtDB

  /// <summary>
  /// Исключение генерируется, если обнаружена ошибка в заполнении объекта DBxStruct
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
      : base("Ошибка в описании структуры таблицы \"" + table.TableName + "\". " + message)
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

  /// <summary>
  /// Исключение, когда у таблицы нет первичного ключа, или он имеет неподходящий тип
  /// </summary>
  [Serializable]
  public class DBxPrimaryKeyException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение с сообщением "Неправильный первичный ключ"
    /// </summary>
    public DBxPrimaryKeyException()
      : base("Неправильный первичный ключ")
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public DBxPrimaryKeyException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public DBxPrimaryKeyException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Используется при десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DBxPrimaryKeyException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

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
    /// <param name="id">Идентификатор не найденной записи, приведенный к типу String</param>
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
    /// <param name="id">Идентификатор не найденной записи, приведенный к типу String</param>
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
    /// <param name="id">Идентификатор не найденной записи, приведенный к типу String</param>
    public DBxRecordNotFoundException(string tableName, string id)
      : base(CreateMessage(tableName, id))
    {
      _TableName = TableName;
      _Id = id;
    }

    private static string CreateMessage(string tableName, string id)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");

      return "Не найдена запись в таблице \"" + tableName + "\" с идентификатором " + id.ToString();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя таблицы, в которой выполнялся поиск
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Идентификатор не найденной записи.
    /// Свойство имеет тип string, а не Int32 на случай будущей поддержки первичных ключей, отличных от Int32
    /// </summary>
    public string Id { get { return _Id; } }
    private string _Id;

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
  /// Исключение выбрасывается, если задан нулевой идентификатор
  /// </summary>
  [Serializable]
  public class DBxNoIdArgumentException : ArgumentException
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение с сообщением "Не задан идентификатор"
    /// </summary>
    public DBxNoIdArgumentException()
      : base("Не задан идентификатор")
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public DBxNoIdArgumentException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="paramName">Имя параметра</param>
    public DBxNoIdArgumentException(string message, string paramName)
      : base(message, paramName)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="paramName">Имя параметра</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public DBxNoIdArgumentException(string message, string paramName, Exception innerException)
      : base(message, paramName, innerException)
    {
    }

    /// <summary>
    /// Создает исключение с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public DBxNoIdArgumentException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Используется при десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DBxNoIdArgumentException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
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
