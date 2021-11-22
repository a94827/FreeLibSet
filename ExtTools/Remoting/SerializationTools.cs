﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

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
