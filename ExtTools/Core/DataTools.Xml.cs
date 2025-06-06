﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using FreeLibSet.IO;

namespace FreeLibSet.Core
{
  partial class DataTools
  {
    #region Работа с XmlDocument

    /// <summary>
    /// Возвращает кодировку, предполагающуюся для XML-документа
    /// Если задана декларация XML, то кодировка возвращается из нее.
    /// Иначе возвращается кодировка Unicode
    /// </summary>
    /// <param name="xmlDoc">Проверяемый XML-документ</param>
    /// <returns>Кодировка</returns>
    public static Encoding GetXmlEncoding(XmlDocument xmlDoc)
    {
      Encoding encoding;
      GetXmlEncoding(xmlDoc, out encoding);
      return encoding;
    }

    /// <summary>
    /// Возвращает кодировку, предполагающуюся для XML-документа
    /// Если задана декларация XML, то кодировка возвращается из нее.
    /// Иначе возвращается кодировка Unicode.
    /// Эта версия возвращает факт наличия заданной кодировки в файле
    /// </summary>
    /// <param name="xmlDoc">Проверяемый XML-документ</param>
    /// <param name="encoding">Результат. Сюда записывается обнаруженная кодировка</param>
    /// <returns>True, если в документе есть узел <see cref="XmlDeclaration"/> с корректной кодировкой.
    /// False, если нет узла <see cref="XmlDeclaration"/> или задана неизвестная кодировка в свойстве <see cref="XmlDeclaration.Encoding"/></returns>
    public static bool GetXmlEncoding(XmlDocument xmlDoc, out Encoding encoding)
    {
      encoding = Encoding.Unicode; // по умолчанию
      if (xmlDoc == null)
        return false;
      if (xmlDoc.ChildNodes.Count == 0)
        return false;

      XmlDeclaration decl = xmlDoc.ChildNodes[0] as XmlDeclaration;
      if (decl == null)
        return false;
      if (String.IsNullOrEmpty(decl.Encoding)) // 18.09.2013
        return false;
      try
      {
        encoding = Encoding.GetEncoding(decl.Encoding);
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Установка кодировки Xml-документа.
    /// Добавляет узел XmlDeclaration, если его нет в документе, или заменяет существующий.
    /// Если XmlDeclaration уже существует и кодировка совпадает, никаких действий не выполняется
    /// </summary>
    /// <param name="xmlDoc">Изменяемый XML-документ</param>
    /// <param name="encoding">Кодировка. Не может быть null</param>
    public static void SetXmlEncoding(XmlDocument xmlDoc, Encoding encoding)
    {
      if (xmlDoc == null)
        throw new ArgumentNullException("xmlDoc");
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      XmlDeclaration decl = null;
      if (xmlDoc.ChildNodes.Count > 0)
        decl = xmlDoc.ChildNodes[0] as XmlDeclaration;
      if (decl != null)
      {
        if (!String.IsNullOrEmpty(decl.Encoding)) // 21.12.2021
        {
          try
          {
            Encoding oldEncoding = Encoding.GetEncoding(decl.Encoding);
            if (encoding.WebName == oldEncoding.WebName)
              return; // Никаких действий выполнять не надо
          }
          catch { } // 21.12.2021
        }
        decl.Encoding = encoding.WebName;
      }
      else
      {
        // Добавляем декларацию
        decl = xmlDoc.CreateXmlDeclaration("1.0", encoding.WebName, null);
        if (xmlDoc.DocumentElement == null)
          xmlDoc.AppendChild(decl);
        else
          xmlDoc.InsertBefore(decl, xmlDoc.DocumentElement);
      }
    }

    /// <summary>
    /// Преобразование XML-документа в массив байт
    /// Используется кодировка, заданная в декларации XML-документа или unicode,
    /// если декларации нет
    /// Выполняется красивое форматирование документа
    /// Если документ не задан, возвращается null
    /// </summary>
    /// <param name="xmlDoc">Записываемый документ</param>
    /// <returns>Массив байт или null</returns>
    public static byte[] XmlDocumentToByteArray(XmlDocument xmlDoc)
    {
      if (xmlDoc == null)
        return null;
      byte[] bytes;
      MemoryStream strm = new MemoryStream();
      try
      {
        FileTools.WriteXmlDocument(strm, xmlDoc);
        strm.Flush();
        bytes = strm.ToArray();
      }
      finally
      {
        strm.Close();
      }
      return bytes;
    }

    /// <summary>
    /// Преобразование массива байт (загруженного текстового файла) в XML-документ
    /// Если массив не задан или имеет нулевую длину, возвращается null
    /// </summary>
    /// <param name="bytes">Загруженный текстовый файл</param>
    /// <returns>XML-документ</returns>
    public static XmlDocument XmlDocumentFromByteArray(Byte[] bytes)
    {
      if (bytes == null || bytes.Length == 0)
        return null;

      MemoryStream strm = new MemoryStream(bytes);
      return FileTools.ReadXmlDocument(strm);
    }

    /// <summary>
    /// Преобразование XML-документа в строку
    /// Выполняется красивое форматирование документа
    /// Если документ не задан, возвращается пустая строк
    /// </summary>
    /// <param name="xmlDoc">Записываемый документ</param>
    /// <returns>Строка</returns>
    public static string XmlDocumentToString(XmlDocument xmlDoc)
    {
      if (xmlDoc == null)
        return String.Empty;

      StringBuilder sb = new StringBuilder();

      XmlWriterSettings settings = new XmlWriterSettings();
      settings.NewLineChars = Environment.NewLine;
      settings.Indent = true;
      settings.IndentChars = "  ";
      using (XmlWriter wrt = XmlWriter.Create(sb, settings))
      {
        xmlDoc.WriteTo(wrt);
        wrt.Flush();
      }
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование строки в XML-документ
    /// Выполняет простой вызов XmlDocumen.LoadXml(), если строка непустая
    /// Если строка пустая, возвращает null
    /// </summary>
    /// <param name="str">Текст в фотрмате xml</param>
    /// <returns>XML-документ</returns>
    public static XmlDocument XmlDocumentFromString(String str)
    {
      if (String.IsNullOrEmpty(str))
        return null;

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.XmlResolver = null; // 16.01.2024
      xmlDoc.LoadXml(str);
      return xmlDoc;
    }

    #endregion
  }
}
