using FreeLibSet.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataToolsTests_Xml
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
      bool res = DataTools.GetXmlEncoding(xmlDoc, out enc1);
      Assert.AreEqual(wantedRes, res, "Found");
      Assert.AreEqual(wantedCP, enc1.CodePage, "#1");

      Encoding enc2 = DataTools.GetXmlEncoding(xmlDoc);
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
      DataTools.SetXmlEncoding(xmlDoc, enc1);

      Encoding enc2 = DataTools.GetXmlEncoding(xmlDoc);
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

      byte[] b = DataTools.XmlDocumentToByteArray(xmlDoc1);

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

      XmlDocument xmlDoc = DataTools.XmlDocumentFromByteArray(b);

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

      string res = DataTools.XmlDocumentToString(xmlDoc);
      Assert.IsTrue(res.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"), "First line");
    }


    [Test]
    public void XmlDocumentFromString()
    {
      string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><x></x>";

      XmlDocument xmlDoc = DataTools.XmlDocumentFromString(xml);

      Assert.AreEqual("x", xmlDoc.DocumentElement.Name);
    }

    #endregion
  }
}
