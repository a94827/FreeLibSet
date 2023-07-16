using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class IniFileTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_default()
    {
      IniFile sut = new IniFile();
      CollectionAssert.AreEqual(DataTools.EmptyStrings, sut.GetSectionNames(), "GetSectionNames()");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_default([Values(false, true)]bool isReadOnly)
    {
      IniFile sut = new IniFile(isReadOnly);
      CollectionAssert.AreEqual(DataTools.EmptyStrings, sut.GetSectionNames(), "GetSectionNames()");
      Assert.AreEqual(isReadOnly, sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Load()

    public static Encoding[] TestEncodings
    {
      get
      {
        List<Encoding> lst = new List<Encoding>();
        lst.Add(Encoding.UTF8);
        lst.Add(Encoding.Unicode);
        lst.Add(Encoding.UTF32);
        try { lst.Add(Encoding.GetEncoding(1251)); } catch { }
        try { lst.Add(Encoding.GetEncoding(866)); } catch { }
        return lst.ToArray();
      }
    }

    [TestCaseSource("TestEncodings")]
    public void Load_encoding(Encoding enc)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {

        AbsPath path = new AbsPath(tempDir.Dir, "test.ini");
        System.IO.File.WriteAllLines(path.Path, GetTestIniLines_1(), enc);

        IniFile sut1 = new IniFile(true);
        sut1.Load(path, enc);
        DoTestLoad_1(sut1, "AbsPath, Encoding");

        using (System.IO.FileStream strm = new System.IO.FileStream(path.Path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
          IniFile sut2 = new IniFile(true);
          sut2.Load(strm, enc);
          DoTestLoad_1(sut2, "Stream, Encoding");
        }
      }
    }

    private static string[] GetTestIniLines_1()
    {
      List<string> lst = new List<string>();
      lst.Add("[Секция1]");
      lst.Add("Имя1=Значение1");
      lst.Add("Имя2=Значение2");
      lst.Add("[Секция2]");
      lst.Add("Имя3=Значение3");
      lst.Add("Имя4=Значение4");
      return lst.ToArray();
    }

    private static void DoTestLoad_1(IniFile sut, string messagePrefix)
    {
      CollectionAssert.AreEquivalent(new string[] { "Секция1", "Секция2" }, sut.GetSectionNames(), messagePrefix + ". GetSectionNames()");
      CollectionAssert.AreEquivalent(new string[] { "Имя1", "Имя2" }, sut.GetKeyNames("Секция1"), messagePrefix + ". GetKeyNames(Секция1)");
      CollectionAssert.AreEquivalent(new string[] { "Имя3", "Имя4" }, sut.GetKeyNames("Секция2"), messagePrefix + ". GetKeyNames(Секция2)");
      Assert.AreEqual("Значение1", sut["Секция1", "Имя1"], messagePrefix + ". [1]");
      Assert.AreEqual("Значение2", sut["Секция1", "Имя2"], messagePrefix + ". [2]");
      Assert.AreEqual("Значение3", sut["Секция2", "Имя3"], messagePrefix + ". [3]");
      Assert.AreEqual("Значение4", sut["Секция2", "Имя4"], messagePrefix + ". [4]");
    }

    [Test]
    public void Load_default()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {

        AbsPath path = new AbsPath(tempDir.Dir, "test.ini");
        System.IO.File.WriteAllLines(path.Path, GetTestIniLines_2());

        IniFile sut1 = new IniFile(true);
        sut1.Load(path);
        DoTestLoad_2(sut1, "AbsPath");

        //using (System.IO.FileStream strm = new System.IO.FileStream(path.Path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //{
        //  IniFile sut2 = new IniFile(true);
        //  sut2.Load(strm);
        //  DoTestLoad_2(sut2, "Stream");
        //}
      }
    }

    private static string[] GetTestIniLines_2()
    {
      List<string> lst = new List<string>();
      lst.Add("[S1]");
      lst.Add("N1=V1");
      lst.Add("N2=V2");
      lst.Add("[S2]");
      lst.Add("N3=V3");
      lst.Add("N4=V4");
      return lst.ToArray();
    }

    private static void DoTestLoad_2(IniFile sut, string messagePrefix)
    {
      CollectionAssert.AreEquivalent(new string[] { "S1", "S2" }, sut.GetSectionNames(), messagePrefix + ". GetSectionNames()");
      CollectionAssert.AreEquivalent(new string[] { "N1", "N2" }, sut.GetKeyNames("S1"), messagePrefix + ". GetKeyNames(S1)");
      CollectionAssert.AreEquivalent(new string[] { "N3", "N4" }, sut.GetKeyNames("S2"), messagePrefix + ". GetKeyNames(S2)");
      Assert.AreEqual("V1", sut["S1", "N1"], messagePrefix + ". [1]");
      Assert.AreEqual("V2", sut["S1", "N2"], messagePrefix + ". [2]");
      Assert.AreEqual("V3", sut["S2", "N3"], messagePrefix + ". [3]");
      Assert.AreEqual("V4", sut["S2", "N4"], messagePrefix + ". [4]");
    }

    #endregion

    #region Save()

    // При записи текстируем самый простой вариант файла - с одной секцией и одним ключом, так порядок секций и ключей является неопределенным


    [TestCaseSource("TestEncodings")]
    public void Save_encoding(Encoding enc)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        IniFile sut = new IniFile();
        sut["Секция1", "Имя1"] = "Значение1";

        AbsPath path1 = new AbsPath(tempDir.Dir, "test1.ini");
        sut.Save(path1, enc);
        DoTestSave_1(path1, enc, "AbsPath, Encoding");

        AbsPath path2 = new AbsPath(tempDir.Dir, "test2.ini");
        using (System.IO.FileStream strm = new System.IO.FileStream(path2.Path, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
        {
          sut.Save(strm, enc);
          DoTestSave_1(path2, enc, "Stream, Encoding");
        }
      }
    }

    private void DoTestSave_1(AbsPath path, Encoding enc, string messagePrefix)
    {
      string[] a = System.IO.File.ReadAllLines(path.Path, enc);
      // Могут быть пустые строки
      // Могут быть лишние пробелы
      List<string> lines = new List<string>();
      for (int i = 0; i < a.Length; i++)
      {
        string s = a[i];
        if (s.Length == 0)
          continue;
        s = DataTools.RemoveChars(s, " ");
        lines.Add(s);
      }

      string[] wanted = new string[2] {
        "[Секция1]",
        "Имя1=Значение1"
      };
      CollectionAssert.AreEqual(wanted, lines, messagePrefix);
    }

    public void Save_default()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        IniFile sut = new IniFile();
        sut["S1", "N1"] = "V1";

        AbsPath path1 = new AbsPath(tempDir.Dir, "test1.ini");
        sut.Save(path1);
        DoTestSave_2(path1, "AbsPath");

        // Нет такой перегрузки
        //AbsPath path2 = new AbsPath(tempDir.Dir, "test2.ini");
        //using (System.IO.FileStream strm = new System.IO.FileStream(path2.Path, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
        //{
        //  sut.Save(strm);
        //  DoTestSave_2(path2, "Stream");
        //}
      }
    }

    private void DoTestSave_2(AbsPath path, string messagePrefix)
    {
      string[] a = System.IO.File.ReadAllLines(path.Path);
      // Могут быть пустые строки
      // Могут быть лишние пробелы
      List<string> lines = new List<string>();
      for (int i = 0; i < a.Length; i++)
      {
        string s = a[i];
        if (s.Length == 0)
          continue;
        s = DataTools.RemoveChars(s, " ");
        lines.Add(s);
      }

      string[] wanted = new string[2] {
        "[S1]",
        "N1=V1"
      };
      CollectionAssert.AreEqual(wanted, lines, messagePrefix);
    }

    [Test]
    public void Save_Load_big()
    {
      Random rnd = new Random();
      IniFile sut = new IniFile();
      for (int i = 0; i < 1000; i++)
      {
        string sect = "S" + StdConvert.ToString(rnd.Next(30));
        string key = "N" + StdConvert.ToString(rnd.Next(30));
        string value = StdConvert.ToString(rnd.Next(100));
        sut[sect, key] = value;
      }

      IniFile res;
      using (TempDirectory tempDir = new TempDirectory())
      {
        AbsPath path = new AbsPath(tempDir.Dir, "test.ini");
        sut.Save(path);

        res = new IniFile();
        res.Load(path);
      }

      CollectionAssert.AreEquivalent(sut.GetSectionNames(), res.GetSectionNames(), "GetSectionNames()");
      foreach (string sect in sut.GetSectionNames())
      {
        CollectionAssert.AreEquivalent(sut.GetKeyNames(sect), res.GetKeyNames(sect), "GetKeyNames()");
        foreach (string key in sut.GetKeyNames(sect))
          Assert.AreEqual(sut[sect, key], res[sect, key], "Value");
      }
    }

    #endregion

    #region Item, GetString()

    [Test]
    public void Item()
    {
      IniFile sut = new IniFile();
      Assert.AreEqual("", sut["Секция1", "Имя1"], "#1");

      sut["Секция1", "Имя1"] = "Значение1";
      Assert.AreEqual("Значение1", sut["сЕкЦиЯ1", "иМЯ1"], "#2");

      sut["Секция1", "Имя1"] = "Значение2";
      Assert.AreEqual("Значение2", sut["Секция1", "Имя1"], "#3");

      sut["Секция1", "Имя1"] = "";
      Assert.AreEqual("", sut["Секция1", "Имя1"], "#4");
    }

    [Test]
    public void GetString()
    {
      IniFile sut = new IniFile();
      Assert.AreEqual("", sut.GetString("Секция1", "Имя1", ""), "#1");
      Assert.AreEqual("XXX", sut.GetString("Секция1", "Имя1", "XXX"), "#2");

      sut["Секция1", "Имя1"] = "Значение1";
      Assert.AreEqual("Значение1", sut.GetString("сЕкЦиЯ1", "иМЯ1", "XXX"), "#3");

      sut["Секция1", "Имя1"] = "Значение2";
      Assert.AreEqual("Значение2", sut.GetString("Секция1", "Имя1", ""), "#4");

      sut["Секция1", "Имя1"] = "";
      Assert.AreEqual("", sut.GetString("Секция1", "Имя1", ""), "#5");
      Assert.AreEqual("", sut.GetString("Секция1", "Имя1", "XXX"), "#6"); // Value already exists: "Имя1="
    }

    #endregion

    #region GetSectionNames()

    [Test]
    public void GetSectionNames()
    {
      IniFile sut = new IniFile();
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetSectionNames(), "#1");

      sut["Секция1", "Имя1"] = "";
      CollectionAssert.AreEquivalent(new string[] { "Секция1" }, sut.GetSectionNames(), "#2");

      sut["Секция1", "Имя1"] = "Значение1";
      CollectionAssert.AreEquivalent(new string[] { "Секция1" }, sut.GetSectionNames(), "#3");

      sut["Секция1", "Имя2"] = "Значение2";
      CollectionAssert.AreEquivalent(new string[] { "Секция1" }, sut.GetSectionNames(), "#4");

      sut["Секция2", "Имя3"] = "Значение3";
      CollectionAssert.AreEquivalent(new string[] { "Секция1", "Секция2" }, sut.GetSectionNames(), "#5");

      sut["Секция2", "Имя3"] = "";
      CollectionAssert.AreEquivalent(new string[] { "Секция1", "Секция2" }, sut.GetSectionNames(), "#6");

      // Вызов DeleteKey() не обязан приводить к удалению секции

      sut.DeleteSection("Секция1");
      CollectionAssert.AreEquivalent(new string[] { "Секция2" }, sut.GetSectionNames(), "#7");
    }

    #endregion

    #region GetKeyNames()

    [Test]
    public void GetKeyNames()
    {
      IniFile sut = new IniFile();
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetKeyNames("Секция1"), "#1");

      sut["Секция1", "Имя1"] = "";
      CollectionAssert.AreEquivalent(new string[] { "Имя1" }, sut.GetKeyNames("Секция1"), "#2");
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetKeyNames("Секция2"), "#3");

      sut["Секция1", "Имя1"] = "Значение1";
      CollectionAssert.AreEquivalent(new string[] { "Имя1" }, sut.GetKeyNames("Секция1"), "#4");

      sut["Секция1", "Имя2"] = "Значение2";
      sut["Секция2", "Имя3"] = "Значение3";
      CollectionAssert.AreEquivalent(new string[] { "Имя1", "Имя2" }, sut.GetKeyNames("Секция1"), "#5");
      CollectionAssert.AreEquivalent(new string[] { "Имя3" }, sut.GetKeyNames("Секция2"), "#6");

      sut["Секция1", "Имя1"] = "";
      CollectionAssert.AreEquivalent(new string[] { "Имя1", "Имя2" }, sut.GetKeyNames("Секция1"), "#7");

      sut.DeleteKey("Секция1", "Имя1");
      CollectionAssert.AreEquivalent(new string[] { "Имя2" }, sut.GetKeyNames("Секция1"), "#8");

      sut.DeleteSection("Секция1");
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetKeyNames("Секция1"), "#9");
      CollectionAssert.AreEquivalent(new string[] { "Имя3" }, sut.GetKeyNames("Секция2"), "#10");
    }

    #endregion

    #region DeleteSection()

    [Test]
    public void DeleteSection()
    {
      IniFile sut = new IniFile();
      sut.DeleteSection("Секция1");
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetSectionNames(), "#1");

      sut["Секция1", "Имя1"] = "Значение1";
      sut["Секция2", "Имя2"] = "Значение2";
      sut.DeleteSection("сЕкЦия1");
      CollectionAssert.AreEquivalent(new string[] { "Секция2" }, sut.GetSectionNames(), "#2");

      sut.DeleteSection("Секция666");
      CollectionAssert.AreEquivalent(new string[] { "Секция2" }, sut.GetSectionNames(), "#3");

      sut.DeleteSection("Секция2");
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetSectionNames(), "#4");
    }

    #endregion

    #region DeleteKey()

    [Test]
    public void DeleteKey()
    {
      IniFile sut = new IniFile();
      sut.DeleteKey("Секция1", "Имя1");
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetKeyNames("Секция1"), "#1");

      sut["Секция1", "Имя1"] = "Значение1";
      sut["Секция1", "Имя2"] = "Значение2";
      sut.DeleteKey("сЕкЦиЯ1", "иМя1");
      CollectionAssert.AreEquivalent(new string[] { "Имя2" }, sut.GetKeyNames("Секция1"), "#2");

      sut["Секция1", "Имя1"] = "";
      CollectionAssert.AreEquivalent(new string[] { "Имя1", "Имя2" }, sut.GetKeyNames("Секция1"), "#3");

      sut.DeleteKey("Секция1", "Имя1");
      CollectionAssert.AreEquivalent(new string[] { "Имя2" }, sut.GetKeyNames("Секция1"), "#4");
    }

    #endregion

    #region GetKeyValues()

    [Test]
    public void GetKeyValues()
    {
      IniFile sut = new IniFile();
      sut["Секция1", "Имя1"] = "AAA";
      sut["Секция1", "Имя1"] = "BBB"; // переписали
      sut["Секция1", "Имя2"] = "";
      sut["Секция1", "Имя3"] = "CCC";
      sut["Секция2", "Имя4"] = "DDD";

      Dictionary<string, string> dict = new Dictionary<string, string>();
      foreach (IniKeyValue pair in sut.GetKeyValues("сЕкЦия1"))
        dict.Add(pair.Key, pair.Value);

      CollectionAssert.AreEquivalent(new string[] { "Имя1", "Имя2", "Имя3" }, dict.Keys, "Keys");
      Assert.AreEqual("BBB", dict["Имя1"], "Имя1");
      Assert.AreEqual("", dict["Имя2"], "Имя2");
      Assert.AreEqual("CCC", dict["Имя3"], "Имя3");
    }

    [Test]
    public void GetKeyValues_empty()
    {
      IniFile sut = new IniFile();
      sut["Секция2", "Имя4"] = "DDD";

      Dictionary<string, string> dict = new Dictionary<string, string>();
      foreach (IniKeyValue pair in sut.GetKeyValues("Секция1"))
        dict.Add(pair.Key, pair.Value);

      Assert.AreEqual(0, dict.Count);
    }

    #endregion

    #region IsReadOnly

    public void IsReadOnly()
    {
      IniFile sut = new IniFile(true);
      Assert.IsTrue(sut.IsReadOnly, "IsReadnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut["S1", "N1"] = "V1"; }, "Item set");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.DeleteSection("S1"); }, "DeleteSection");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.DeleteKey("S1", "N1"); }, "DeleteKey");
      System.IO.MemoryStream ms = new System.IO.MemoryStream();
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Save(ms, Encoding.Unicode); }, "Save");
    }

    #endregion
  }
}
