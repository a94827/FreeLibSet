// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using FreeLibSet.IO;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Методы работы с <see cref="XmlDocument"/>
  /// </summary>
  public static class XmlTools
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
        WriteXmlDocument(strm, xmlDoc);
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
      return ReadXmlDocument(strm);
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

    #region Чтение и запись XML-документов

    #region Запись

    /// <summary>
    /// Запись XML-документа в текстовый файл.
    /// Используется кодировка, заданная в декларации XML-документа или unicode, если декларации нет.
    /// Выполняется красивое форматирование документа.
    /// </summary>
    /// <param name="filePath">Имя файла для записи</param>
    /// <param name="xmlDoc">Записываемый документ</param>
    public static void WriteXmlDocument(AbsPath filePath, XmlDocument xmlDoc)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      Encoding enc = GetXmlEncoding(xmlDoc);
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.NewLineChars = "\r\n";
      settings.Encoding = enc;
      settings.Indent = true;
      settings.IndentChars = "  ";
      XmlWriter wrt = XmlWriter.Create(filePath.Path, settings);
      try
      {
        xmlDoc.WriteTo(wrt);
      }
      finally
      {
        wrt.Close();
      }
    }

    /// <summary>
    /// Запись XML-документа в текстовый файл, заданный как поток (например, <see cref="MemoryStream"/>).
    /// Используется кодировка, заданная в декларации XML-документа или unicode,
    /// если декларации нет.
    /// Выполняется красивое форматирование документа.
    /// </summary>
    /// <param name="outStream">Поток для записи</param>
    /// <param name="xmlDoc">Записываемый документ</param>
    public static void WriteXmlDocument(Stream outStream, XmlDocument xmlDoc)
    {
      if (outStream == null)
        throw new ArgumentNullException("outStream");
      if (xmlDoc == null)
        throw new ArgumentNullException("xmlDoc");

      Encoding enc = GetXmlEncoding(xmlDoc);
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.NewLineChars = "\r\n";
      settings.Encoding = enc;
      settings.Indent = true;
      settings.IndentChars = "  ";
      StreamWriter wrt1 = new StreamWriter(outStream, enc);
      XmlWriter wrt2 = XmlWriter.Create(wrt1, settings);
      try
      {
        xmlDoc.WriteTo(wrt2);
      }
      finally
      {
        wrt2.Close();
        wrt1.Close();
      }
    }

    #endregion

    #region Чтение

    /// <summary>
    /// Чтение XML-документа.
    /// Вызывает <see cref="XmlDocument.Load(string)"/>.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>XML-документ</returns>
    public static XmlDocument ReadXmlDocument(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.XmlResolver = null; // 16.01.2024
      xmlDoc.Load(filePath.Path);
      return xmlDoc;
    }

    /// <summary>
    /// Чтение XML-документа.
    /// Вызывает <see cref="XmlDocument.Load(Stream)"/>.
    /// </summary>
    /// <param name="inStream">Поток для загрузки XML-документа</param>
    /// <returns>XML-документ</returns>
    public static XmlDocument ReadXmlDocument(Stream inStream)
    {
      if (inStream == null)
        throw new ArgumentNullException("inStream");

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.XmlResolver = null; // 16.01.2024
      xmlDoc.Load(inStream);
      return xmlDoc;
    }

    #endregion

    #region Проверка начала файла

    /// <summary>
    /// Начало любого XML-файла в однобайтной кодировке (например, 1251 или 866)
    /// </summary>
    private static readonly byte[] _XmlStartAnsiBytes = new byte[] { 0x3c, 0x3f, 0x78, 0x6d, 0x6c, 0x20 };


    /// <summary>
    /// Начало любого XML-файла в кодировке utf-8
    /// </summary>
    private static readonly byte[] _XmlStartUtf8Bytes = new byte[] { 0xEF, 0xBB, 0xBF, 0x3c, 0x3f, 0x78, 0x6d, 0x6c, 0x20 };


    /// <summary>
    /// Начало любого XML-файла в кодировке utf-16
    /// </summary>
    private static readonly byte[] _XmlStartUtf16Bytes = new byte[] { 0xFF, 0xFE, 0x3c, 0x00, 0x3f, 0x00, 0x78, 0x00, 0x6d, 0x00, 0x6c, 0x00, 0x20, 0x00 };

    /// <summary>
    /// Начало любого XML-файла в кодировке utf-16BE
    /// </summary>
    private static readonly byte[] _XmlStartUtf16BEBytes = new byte[] { 0xFE, 0xFF, 0x00, 0x3c, 0x00, 0x3f, 0x00, 0x78, 0x00, 0x6d, 0x00, 0x6c, 0x00, 0x20 };

    /// <summary>
    /// Начало любого XML-файла в кодировке utf-32
    /// </summary>
    private static readonly byte[] _XmlStartUtf32Bytes = new byte[] { 0xFF , 0xFE , 0x00 , 0x00 ,
                                                                     0x3c , 0x00 , 0x00 , 0x00 ,
                                                                     0x3f, 0x00, 0x00, 0x00,
                                                                     0x78, 0x00, 0x00, 0x00,
                                                                     0x6d, 0x00, 0x00, 0x00,
                                                                     0x6c, 0x00, 0x00, 0x00,
                                                                     0x20, 0x00, 0x00, 0x00};
    /// <summary>
    /// Начало любого XML-файла в кодировке utf-32BE
    /// </summary>
    private static readonly byte[] _XmlStartUtf32BEBytes = new byte[] { 0x00, 0x00, 0xFE, 0xFF,
                                                                       0x00, 0x00, 0x00, 0x3c,
                                                                       0x00, 0x00, 0x00, 0x3f,
                                                                       0x00, 0x00, 0x00, 0x78,
                                                                       0x00, 0x00, 0x00, 0x6d,
                                                                       0x00, 0x00, 0x00, 0x6c,
                                                                       0x00, 0x00, 0x00, 0x20};

    /// <summary>
    /// Массив возможных "начал" файлов
    /// </summary>
    private static readonly byte[][] _XmlStartAnyBytes = new byte[][] { _XmlStartAnsiBytes, _XmlStartUtf8Bytes,
      _XmlStartUtf16Bytes, _XmlStartUtf16BEBytes, _XmlStartUtf32Bytes, _XmlStartUtf32BEBytes };

    /// <summary>
    /// Возвращает true, если байты потока, начиная с текущей позиции, соответствуют XML-файлу в любой (определяемой) кодировке
    /// Функция применяется для проверки загруженного в память файла неизвестного
    /// содержимого перед вызовом <see cref="XmlDocument.Load(Stream)"/>, чтобы избежать лишнего вызова
    /// исключения, когда файл может быть не-XML документом.
    /// Проверяется только начало файла, а не корректность всего XML-документа.
    /// Поток должен поддерживать позиционирование, так как используется свойство <see cref="Stream.Position"/>. 
    /// В противном случае будет сгенерировано <see cref="NotSupportedException"/>
    /// </summary>
    /// <param name="inStream">Открытый на чтение поток</param>
    /// <returns>true, если есть смысл попытаться преобразовать файл в XML-формат</returns>
    public static bool IsValidXmlStart(Stream inStream)
    {
      if (inStream == null)
        return false;

      for (int i = 0; i < _XmlStartAnyBytes.Length; i++)
      {
        if (FileTools.StartsWith(inStream, _XmlStartAnyBytes[i]))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Возвращает true, если байты потока, начиная с текущей позиции, соответствуют XML-файлу в любой (определяемой) кодировке
    /// Функция применяется для проверки загруженного в память файла неизвестного
    /// содержимого перед вызовом <see cref="XmlDocument.Load(string)"/>, чтобы избежать лишнего вызова
    /// исключения, когда файл может быть не-XML документом.
    /// Проверяется только начало файла, а не корректность всего XML-документа.
    /// Рекомендуется использовать перегрузку с аргументом <see cref="Stream"/>, во избежание повторного открытия файла, если планируется дальнейшая загрузка документа
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>true, если есть смысл попытаться преобразовать файл в XML-формат</returns>
    public static bool IsValidXmlStart(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        return false;
      if (!File.Exists(filePath.Path))
        return false;

      using (FileStream stream = new FileStream(filePath.Path, FileMode.Open, FileAccess.Read))
      {
        return IsValidXmlStart(stream);
      }
    }

    #endregion

    #endregion
  }
}
