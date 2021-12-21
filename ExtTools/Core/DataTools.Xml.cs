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

      XmlDeclaration Decl = xmlDoc.ChildNodes[0] as XmlDeclaration;
      if (Decl == null)
        return false;
      if (String.IsNullOrEmpty(Decl.Encoding)) // 18.09.2013
        return false;
      try
      {
        encoding = Encoding.GetEncoding(Decl.Encoding);
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

      XmlDeclaration Decl = null;
      if (xmlDoc.ChildNodes.Count > 0)
        Decl = xmlDoc.ChildNodes[0] as XmlDeclaration;
      if (Decl != null)
      {
        if (!String.IsNullOrEmpty(Decl.Encoding)) // 21.12.2021
        {
          try
          {
            Encoding OldEncoding = Encoding.GetEncoding(Decl.Encoding);
            if (encoding.WebName == OldEncoding.WebName)
              return; // ������� �������� ��������� �� ����
          }
          catch { } // 21.12.2021
        }
        Decl.Encoding = encoding.WebName;
      }
      else
      {
        // ��������� ����������
        Decl = xmlDoc.CreateXmlDeclaration("1.0", encoding.WebName, null);
        if (xmlDoc.DocumentElement == null)
          xmlDoc.AppendChild(Decl);
        else
          xmlDoc.InsertBefore(Decl, xmlDoc.DocumentElement);
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
      byte[] Bytes;
      MemoryStream strm = new MemoryStream();
      try
      {
        FileTools.WriteXmlDocument(strm, xmlDoc);
        strm.Flush();
        Bytes = strm.ToArray();
      }
      finally
      {
        strm.Close();
      }
      return Bytes;
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

      XmlWriterSettings Settings = new XmlWriterSettings();
      Settings.NewLineChars = Environment.NewLine;
      Settings.Indent = true;
      Settings.IndentChars = "  ";
      using (XmlWriter wrt = XmlWriter.Create(sb, Settings))
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

      XmlDocument Doc = new XmlDocument();
      Doc.LoadXml(str);
      return Doc;
    }

    #endregion
  }
}
