using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

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

namespace FreeLibSet.Remoting
{
  /// <summary>
  /// Статические методы для сериализации данных
  /// </summary>
  public static class SerializationTools
  {
    #region Проверка объекта

    /// <summary>
    /// Возвращает true, если значение <paramref name="testValue"/> может быть передано с помощью серилизации или
    /// как объект marshal-by-reference. Также возвращает true для значения null.
    /// Выполняется проверка сериализуемости только основного объекта. Если поля объекта ссылаются на другие
    /// объекты, то они не проверяются.
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <returns>true, если возможна передача через границу AppDomain</returns>
    public static bool IsMarshallable(object testValue)
    {
      if (testValue == null)
        return true;
      Type t = testValue.GetType();
      return t.IsSerializable || t.IsMarshalByRef;
    }

    #endregion

    #region Двоичная сериализация

    /// <summary>
    /// Выполняет двоичную сериализацию объекта в памяти и возвращат массив байт.
    /// </summary>
    /// <param name="value">Сериализуемый объект. Не может быть null</param>
    /// <returns>Сериализованные данные</returns>
    public static byte[] SerializeBinary(object value)
    {
      if (value == null)
        throw new ArgumentNullException("value");

      byte[] res;
      using (MemoryStream ms = new MemoryStream())
      {
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        bf.Serialize(ms, value);
        // не нужен ms.Flush();

        //res = ms.GetBuffer();
        res = ms.ToArray(); // 20.11.2020. GetBuffer() возваращает лишние байты
      }
      return res;
    }

    /// <summary>
    /// Выполняет двоичную десериализацию объекта в памяти
    /// </summary>
    /// <param name="a">Сериализованные данные</param>
    /// <returns>Десериализованный объект</returns>
    public static object DeserializeBinary(byte[] a)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      object res;
      using (MemoryStream ms = new MemoryStream(a))
      {
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        res = bf.Deserialize(ms);
      }
      return res;
    }

    #endregion

    #region Объект DataSet

    /// <summary>
    /// Подготовка набора данных к кэшированию.
    /// Вызывает DataSet.AccepChanges() и устанавливает свойство DataSet.RemotingFormat.
    /// Этот метод рекомендуется вызывать из Вашей реализации ICacheFactory, если данные содержат объект(ы) DataSet
    /// </summary>
    /// <param name="ds">Набор данных, который будет входить в кэшируемые данные.
    /// Если null, то никаких действий не выполняется</param>
    public static void PrepareDataSet(System.Data.DataSet ds)
    {
      if (ds == null)
        return;

      ds.AcceptChanges();

      ds.RemotingFormat = GetPreferredRemotingFormat(ds);

    }

    /// <summary>
    /// Возвращает формат сериализации (Xml или Binary), предпочтительный для заданного набора DataSet.
    /// Большие таблицы выгодно, с точки зрения размера передаваемых байтов, сериализовывать в двоичном формате, а маленькие - в старом (XML) формате.
    /// </summary>
    /// <param name="ds">Проверяемый набор данных</param>
    /// <returns>Предпочтительный формат</returns>
    public static SerializationFormat GetPreferredRemotingFormat(DataSet ds)
    {
      int cntRows = 0;
      for (int i = 0; i < ds.Tables.Count; i++)
        cntRows += ds.Tables[i].Rows.Count;

      if (cntRows > 100)
        return System.Data.SerializationFormat.Binary;
      else
        return System.Data.SerializationFormat.Xml;
    }

    /// <summary>
    /// Возвращает формат сериализации (Xml или Binary), предпочтительный для заданной таблицы DataTable.
    /// Большие таблицы выгодно, с точки зрения размера передаваемых байтов, сериализовывать в двоичном формате, а маленькие - в старом (XML) формате.
    /// </summary>
    /// <param name="table">Проверяемая таблица данных</param>
    /// <returns>Предпочтительный формат</returns>
    public static SerializationFormat GetPreferredRemotingFormat(DataTable table)
    {
      if (table.Rows.Count > 100)
        return System.Data.SerializationFormat.Binary;
      else
        return System.Data.SerializationFormat.Xml;
    }

    #endregion
  }
}
