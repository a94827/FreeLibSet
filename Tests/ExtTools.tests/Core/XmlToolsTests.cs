using FreeLibSet.Core;
using FreeLibSet.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class XmlToolsTests
  {
    #region Get/SetXmlEncoding()

    [TestCase("<x></x>", false, 1200)]
    [TestCase("<?xml version=\"1.0\"?><x></x>", false, 1200)]
    [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>", true, 65001)]
    [TestCase("<?xml version=\"1.0\" encoding=\"windows-1251\"?><x></x>", true, 1251)]
    public void GetXmlEncoding(string xml, bool wantedRes, int wantedCP)
    {
      TextReader rdr1 = new StringReader(xml);
      XmlReader rdr2 = XmlReader.Create(rdr1);
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(rdr2);

      Encoding enc1;
      bool res = XmlTools.GetXmlEncoding(xmlDoc, out enc1);
      Assert.AreEqual(wantedRes, res, "Found");
      Assert.AreEqual(wantedCP, enc1.CodePage, "#1");

      Encoding enc2 = XmlTools.GetXmlEncoding(xmlDoc);
      Assert.AreEqual(wantedCP, enc2.CodePage, "#2");
    }

    [TestCase("<x></x>", 65001)]
    [TestCase("<?xml version=\"1.0\"?><x></x>", 65001)]
    [TestCase("<?xml version=\"1.0\" encoding=\"windows-1251\"?><x></x>", 65001)]
    [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>", 1251)]
    public void SetXmlEncoding(string xml, int cp)
    {
      TextReader rdr1 = new StringReader(xml);
      XmlReader rdr2 = XmlReader.Create(rdr1);
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(rdr2);

      Encoding enc1 = Encoding.GetEncoding(cp);
      XmlTools.SetXmlEncoding(xmlDoc, enc1);

      Encoding enc2 = XmlTools.GetXmlEncoding(xmlDoc);
      Assert.AreEqual(cp, enc2.CodePage);
    }

    #endregion

    #region XmlDocumentTo/FromByteArray()

    [Test]
    public void XmlDocumentToByteArray()
    {
      // Бесполезно проверять вывод метода XmlDocumentToByteArray(), т.к. форматирование текста может поменяться
      // и тест будет хрупким.

      string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>";
      TextReader rdr1 = new StringReader(xml);
      XmlReader rdr2 = XmlReader.Create(rdr1);
      XmlDocument xmlDoc1 = new XmlDocument();
      xmlDoc1.Load(rdr2);

      byte[] b = XmlTools.XmlDocumentToByteArray(xmlDoc1);

      // Главное, чтобы прочитать можно было

      MemoryStream strm = new MemoryStream(b);
      XmlDocument xmlDoc2 = new XmlDocument();
      xmlDoc2.Load(strm);

      Assert.AreEqual("x", xmlDoc2.DocumentElement.Name);
    }


    [Test]
    public void XmlDocumentFromByteArray()
    {
      string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>";
      byte[] b = Encoding.UTF8.GetBytes(xml);

      XmlDocument xmlDoc = XmlTools.XmlDocumentFromByteArray(b);

      Assert.AreEqual("x", xmlDoc.DocumentElement.Name);
    }

    #endregion

    #region XmlDocumentTo/FromString()

    [Test]
    public void XmlDocumentToString()
    { 
      // Бесполезно проверять точный формат выводимой строки, т.к. он может измениться и тест будет хрупким.
      string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>";
      TextReader rdr1 = new StringReader(xml);
      XmlReader rdr2 = XmlReader.Create(rdr1);
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(rdr2);

      string res = XmlTools.XmlDocumentToString(xmlDoc);
      Assert.IsTrue(res.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", StringComparison.Ordinal), "First line");
    }


    [Test]
    public void XmlDocumentFromString()
    {
      string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>";

      XmlDocument xmlDoc = XmlTools.XmlDocumentFromString(xml);

      Assert.AreEqual("x", xmlDoc.DocumentElement.Name);
    }

    #endregion

    #region Read/WriteXmlDocument()

    [TestCase("windows-1251", 1251)]
    [TestCase("utf-8", 65001)]
    public void WriteXmlDocument(string encodingStr, int codePage)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlElement elMain = xmlDoc.CreateElement("Эл1");
      xmlDoc.AppendChild(elMain);
      XmlElement el2 = xmlDoc.CreateElement("Эл2");
      elMain.AppendChild(el2);
      XmlAttribute attr = xmlDoc.CreateAttribute("Атр3");
      attr.Value = "ABC";
      el2.Attributes.Append(attr);
      XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", encodingStr, "yes");
      xmlDoc.InsertBefore(decl, elMain);

      // <?xml version="1.0" encoding="xxx" standalone="yes"?>
      // <Эл1>
      //   <Эл2 Атр3="ABC"/>
      // </Эл1>

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file1 = new AbsPath(dir.Dir, "test1.xml");
        XmlTools.WriteXmlDocument(file1, xmlDoc);
        DoTestWriteXmlDocument(file1, codePage, "#1");

        System.IO.MemoryStream ms2 = new System.IO.MemoryStream();
        XmlTools.WriteXmlDocument(ms2, xmlDoc);
        ms2.Flush();
        byte[] b2 = ms2.ToArray();
        AbsPath file2 = new AbsPath(dir.Dir, "test2.xml");
        System.IO.File.WriteAllBytes(file2.Path, b2);
        DoTestWriteXmlDocument(file2, codePage, "#2");
      }
    }

    private static void DoTestWriteXmlDocument(AbsPath file, int codePage, string messagePrefix)
    {
      Assert.IsTrue(System.IO.File.Exists(file.Path), messagePrefix + ". File exists");
      Encoding enc = Encoding.GetEncoding(codePage);
      string[] lines = System.IO.File.ReadAllLines(file.Path, enc);
      Assert.AreEqual(4, lines.Length, messagePrefix + ". Line count");
      Assert.IsTrue(lines[0].StartsWith("<?xml version=\"1.0\"", StringComparison.Ordinal), messagePrefix + ". Line 1");
      // Мало просто использовать Trim(), т.к. пробелы могут быть вставлены в середину строки без определенных правил
      Assert.AreEqual("<Эл1>", StringTools.RemoveChars(lines[1], " "), messagePrefix + ". Line 2");
      Assert.AreEqual("<Эл2Атр3=\"ABC\"/>", StringTools.RemoveChars(lines[2], " "), messagePrefix + ". Line 3");
      Assert.AreEqual("</Эл1>", StringTools.RemoveChars(lines[3], " "), messagePrefix + ". Line 4");
    }

    [Test]
    public void ReadXmlDocument()
    {
      string[] lines = new string[] {
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
        "<Эл1>",
        "<Эл2 Атр3=\"ABC\"/>",
        "</Эл1>" };

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file1 = new AbsPath(dir.Dir, "test1.xml");
        System.IO.File.WriteAllLines(file1.Path, lines, Encoding.UTF8);

        XmlDocument xmlDoc1 = XmlTools.ReadXmlDocument(file1);
        DoTestReadXmlDocument(xmlDoc1, "#1");

        byte[] b2 = System.IO.File.ReadAllBytes(file1.Path);
        System.IO.MemoryStream ms2 = new System.IO.MemoryStream(b2);
        XmlDocument xmlDoc2 = XmlTools.ReadXmlDocument(ms2);
        DoTestReadXmlDocument(xmlDoc2, "#2");
      }
    }

    private static void DoTestReadXmlDocument(XmlDocument xmlDoc, string messagePrefix)
    {
      Assert.AreEqual(2, xmlDoc.ChildNodes.Count, messagePrefix + ". ChildNodeCount");
      Assert.IsInstanceOf<XmlDeclaration>(xmlDoc.ChildNodes[0], messagePrefix + ". Declaration type");
      Assert.IsInstanceOf<XmlElement>(xmlDoc.ChildNodes[1], messagePrefix + ". Root Element type");
      Assert.AreEqual("Эл1", xmlDoc.DocumentElement.Name, messagePrefix + ". Root Element name");
    }

    #endregion

    #region IsValidXmlStart()

    [Test]
    public void IsValidXmlStart_FileName()
    {
      // Условный xml-файл из одной строки
      string sGood = "<?xml version =\"1.0\"?>";
      string sBad = "<XXX>";

      // проверяем только одну кодировку, чтобы не записывать много файлов

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file1 = new AbsPath(dir.Dir, "test1.xml");
        System.IO.File.WriteAllText(file1.Path, sGood, Encoding.UTF8);
        Assert.IsTrue(XmlTools.IsValidXmlStart(file1), "#1");

        AbsPath file2 = new AbsPath(dir.Dir, "test2.xml");
        System.IO.File.WriteAllText(file2.Path, sBad, Encoding.UTF8);
        Assert.IsFalse(XmlTools.IsValidXmlStart(file2), "#2");
      }
    }

    /// <summary>
    /// Расширение класса EncodingInfo методом ToString() для отображения в списке тестов
    /// </summary>
    public struct EncodingInfo2
    {
      public EncodingInfo2(EncodingInfo info)
      {
        _Info = info;
      }

      public EncodingInfo Info { get { return _Info; } }
      private EncodingInfo _Info;

      public override string ToString()
      {
        if (_Info == null)
          return "Empty";
        else
          return _Info.DisplayName + ", CodePage=" + _Info.CodePage;
      }
    }

    public EncodingInfo2[] IsValidXmlStart_Encodings
    {
      get
      {
        List<EncodingInfo2> lst = new List<EncodingInfo2>();
        foreach (EncodingInfo ei in Encoding.GetEncodings())
        {
          if (ei.DisplayName.StartsWith("IBM EBCDIC"))
            continue; // может быть, и нужно делать для этих кодировок

          switch (ei.CodePage)
          {
            //case 20924: // IBM Латиница-1 на Windows-XP
            case 1047: // IBM Латиница-1 на Windows-7
            case 65000: // UTF-7
            case 500: // 03.09.2023: Эти кодировки не проходят тест в Linux
            case 870:
            case 875:
            case 1026:
            case 37:
              continue;
          }
          // 03.09.2023: Пропускаем все кодировки EBCDIC и IBB. Под Linux тесты не проходят
          if (ei.CodePage >= 20000 && ei.CodePage <= 29999)
            continue;
          if (ei.CodePage >= 1140 && ei.CodePage <= 1149)
            continue;


          lst.Add(new EncodingInfo2(ei));
        }
        return lst.ToArray();
      }
    }

    [TestCaseSource("IsValidXmlStart_Encodings")]
    public void IsValidXmlStart_Stream(EncodingInfo2 ei2)
    {
      // Условный xml-файл из одной строки
      string sGood = "<?xml version =\"1.0\"?>";
      string sBad = "<XXX>";
      DoTestIsValidXmlStart(ei2.Info, sGood, true, "#1");
      DoTestIsValidXmlStart(ei2.Info, sBad, false, "#2");
    }

    private static void DoTestIsValidXmlStart(EncodingInfo ei, string s, bool wantedRes, string message)
    {
      Encoding enc = ei.GetEncoding();

      System.IO.MemoryStream ms = new System.IO.MemoryStream();
      System.IO.TextWriter wrt = new System.IO.StreamWriter(ms, enc);
      wrt.Write(s);
      wrt.Flush();
      string sHexBytes = StringTools.BytesToHex(ms.ToArray(), false);
      ms.Position = 0;

      bool res = XmlTools.IsValidXmlStart(ms);
      Assert.AreEqual(wantedRes, res, message /*+ ", Stream="+sHexBytes*/);
    }

    #endregion
  }
}
