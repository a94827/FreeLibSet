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
  partial class DataTools
  {
    #region ������ � XmlDocument

    /// <summary>
    /// ���������� ���������, ���������������� ��� XML-���������
    /// ���� ������ ���������� XML, �� ��������� ������������ �� ���.
    /// ����� ������������ ��������� Unicode
    /// </summary>
    /// <param name="xmlDoc">����������� XML-��������</param>
    /// <returns>���������</returns>
    public static Encoding GetXmlEncoding(XmlDocument xmlDoc)
    {
      Encoding encoding;
      GetXmlEncoding(xmlDoc, out encoding);
      return encoding;
    }

    /// <summary>
    /// ���������� ���������, ���������������� ��� XML-���������
    /// ���� ������ ���������� XML, �� ��������� ������������ �� ���.
    /// ����� ������������ ��������� Unicode.
    /// ��� ������ ���������� ���� ������� �������� ��������� � �����
    /// </summary>
    /// <param name="xmlDoc">����������� XML-��������</param>
    /// <param name="encoding">���������. ���� ������������ ������������ ���������</param>
    /// <returns>True, ���� � ��������� ���� ���� XmlDeclaration � ���������� ����������.
    /// False, ���� ��� ���� XmlDeclation ��� ������ ����������� ��������� � �������� Encoding</returns>
    public static bool GetXmlEncoding(XmlDocument xmlDoc, out Encoding encoding)
    {
      encoding = Encoding.Unicode; // �� ���������
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
    /// ��������� ��������� Xml-���������.
    /// ��������� ���� XmlDeclaration, ���� ��� ��� � ���������, ��� �������� ������������.
    /// ���� XmlDeclaration ��� ���������� � ��������� ���������, ������� �������� �� �����������
    /// </summary>
    /// <param name="xmlDoc">���������� XML-��������</param>
    /// <param name="encoding">���������. �� ����� ���� null</param>
    public static void SetXmlEncoding(XmlDocument xmlDoc, Encoding encoding)
    {
      if (xmlDoc == null)
        throw new ArgumentNullException("Doc");
      if (encoding == null)
        throw new ArgumentNullException("Encoding");

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
              return; // ������� �������� ��������� �� ����
          }
          catch { } // 21.12.2021
        }
        decl.Encoding = encoding.WebName;
      }
      else
      {
        // ��������� ����������
        decl = xmlDoc.CreateXmlDeclaration("1.0", encoding.WebName, null);
        if (xmlDoc.DocumentElement == null)
          xmlDoc.AppendChild(decl);
        else
          xmlDoc.InsertBefore(decl, xmlDoc.DocumentElement);
      }
    }

    /// <summary>
    /// �������������� XML-��������� � ������ ����
    /// ������������ ���������, �������� � ���������� XML-��������� ��� unicode,
    /// ���� ���������� ���
    /// ����������� �������� �������������� ���������
    /// ���� �������� �� �����, ������������ null
    /// </summary>
    /// <param name="xmlDoc">������������ ��������</param>
    /// <returns>������ ���� ��� null</returns>
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
    /// �������������� ������� ���� (������������ ���������� �����) � XML-��������
    /// ���� ������ �� ����� ��� ����� ������� �����, ������������ null
    /// </summary>
    /// <param name="bytes">����������� ��������� ����</param>
    /// <returns>XML-��������</returns>
    public static XmlDocument XmlDocumentFromByteArray(Byte[] bytes)
    {
      if (bytes == null || bytes.Length == 0)
        return null;

      MemoryStream strm = new MemoryStream(bytes);
      return FileTools.ReadXmlDocument(strm);
    }

    /// <summary>
    /// �������������� XML-��������� � ������
    /// ����������� �������� �������������� ���������
    /// ���� �������� �� �����, ������������ ������ �����
    /// </summary>
    /// <param name="xmlDoc">������������ ��������</param>
    /// <returns>������</returns>
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
    /// �������������� ������ � XML-��������
    /// ��������� ������� ����� XmlDocumen.LoadXml(), ���� ������ ��������
    /// ���� ������ ������, ���������� null
    /// </summary>
    /// <param name="str">����� � �������� xml</param>
    /// <returns>XML-��������</returns>
    public static XmlDocument XmlDocumentFromString(String str)
    {
      if (String.IsNullOrEmpty(str))
        return null;

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(str);
      return xmlDoc;
    }

    #endregion
  }
}
