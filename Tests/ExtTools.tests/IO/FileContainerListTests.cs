using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class FileContainerListTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      FileContainerList sut = new FileContainerList();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");

      int cnt = 0;
      foreach (FileContainer item in sut)
        cnt++;
      Assert.AreEqual(0, cnt, "GetEnumerator()");
    }

    #endregion

    #region Тестовый объект


    private static FileContainerList CreateTestObject()
    {
      FileContainerList sut = new FileContainerList();

      FileContainer item1 = new FileContainer("Test1.bin", GetContent(1000), "");
      sut.Add(item1);

      FileContainer item2 = new FileContainer("Test2.bin", GetContent(2000), "aaa");
      sut.Add(item2);

      FileContainer item3 = new FileContainer("Test3.bin", GetContent(3000), "aaa" + Path.DirectorySeparatorChar + "bbb");
      sut.Add(item3);

      return sut;
    }

    private static byte[] GetContent(int len)
    {
      byte[] b = new byte[len];
      for (int i = 0; i < len; i++)
        b[i] = (byte)(i % 100);
      return b;
    }

    #endregion

    #region Item

    [TestCase("Test1.bin", true)]
    [TestCase("Test2.bin", true)]
    [TestCase("Test3.bin", true)]
    [TestCase("xxx.bin", false)]
    [TestCase("", false)]
    public void Item_byName_get(string fileName, bool wantedNotNull)
    {
      FileContainerList sut = CreateTestObject();

      FileContainer res = sut[fileName];
      Assert.AreEqual(wantedNotNull, res != null, "Found");
      if (wantedNotNull)
        Assert.AreEqual(fileName, res.FileInfo.Name, "Name");
    }

    [Test]
    public void Item_byName_get_caseSensitivity()
    {
      FileContainerList sut = CreateTestObject();
      FileContainer res = sut["test2.bin"];
      Assert.AreEqual(FileTools.CaseSensitive, res == null, "Found");
    }

    [Test]
    public void Item_byIndex_get()
    {
      FileContainerList sut = CreateTestObject();

      Assert.AreEqual("Test1.bin", sut[0].FileInfo.Name, "[0]");
      Assert.AreEqual("Test2.bin", sut[1].FileInfo.Name, "[1]");
      Assert.AreEqual("Test3.bin", sut[2].FileInfo.Name, "[2]");
      FileContainer dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[-1]; }, "[-1]");
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[3]; }, "[3]");
    }

    #endregion

    #region Добавление/удаление элементов

    [Test]
    public void Add_new()
    {
      FileContainerList sut = CreateTestObject();

      FileContainer item4 = new FileContainer("Test4.bin", GetContent(4000), "");
      sut.Add(item4);

      Assert.AreEqual(4, sut.Count, "Count");
      Assert.AreEqual("Test4.bin", sut[3].FileInfo.Name, "Name");
    }

    [Test]
    public void Add_replace()
    {
      FileContainerList sut = CreateTestObject();

      FileContainer item4 = new FileContainer("Test2.bin", GetContent(4000), "");
      sut.Add(item4);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(4000, sut[1].FileInfo.Length, "Length");
      Assert.AreEqual("", sut[1].SubDir, "SubDir");
    }

    [Test]
    public void Add_null()
    {
      FileContainerList sut = CreateTestObject();
      FileContainer item4 = null;
      Assert.Catch<ArgumentNullException>(delegate () { sut.Add(item4); });
    }

    [Test]
    public void Remove_ok()
    {
      FileContainerList sut = CreateTestObject();
      FileContainer item = sut[1];
      bool res = sut.Remove(item);
      Assert.IsTrue(res, "Result");
      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual("Test1.bin", sut[0].FileInfo.Name, "Name[0]");
      Assert.AreEqual("Test3.bin", sut[1].FileInfo.Name, "Name[1]");
    }

    [Test]
    public void Remove_notFound()
    {
      FileContainerList sut = CreateTestObject();
      FileContainer item = new FileContainer("Test4.bin", GetContent(4000), "");
      bool res = sut.Remove(item);
      Assert.IsFalse(res, "Result");
      Assert.AreEqual(3, sut.Count, "Count");
    }

    [Test]
    public void Clear()
    {
      FileContainerList sut = CreateTestObject();
      sut.Clear();
      Assert.AreEqual(0, sut.Count, "Count");
    }

    #endregion

    #region Поиск элементов по имени файлов

    [TestCase("Test1.bin", 0)]
    [TestCase("Test2.bin", 1)]
    [TestCase("Test3.bin", 2)]
    [TestCase("xxx.bin", -1)]
    [TestCase("", -1)]
    public void IndexOf(string fileName, int wantedRes)
    {
      DoTest(fileName, wantedRes);
    }

    [Test]
    public void IndexOf_caseSensitivity()
    {
      DoTest("test2.bin", FileTools.CaseSensitive ? -1 : 1);
    }

    private void DoTest(string fileName, int wantedRes)
    {
      FileContainerList sut = CreateTestObject();
      int res = sut.IndexOf(fileName);
      Assert.AreEqual(wantedRes, res, "IndexOf");
    }

    #endregion

    #region Contains()

    [Test]
    public void Contains()
    {
      FileContainerList sut = CreateTestObject();

      FileContainer item2 = sut[1];
      Assert.IsTrue(sut.Contains(item2), "Test2.bin");

      FileContainer item4 = new FileContainer("Test4.bin", GetContent(4000), "");
      Assert.IsFalse(sut.Contains(item4), "Test4.bin");

      FileContainer item0 = null;
      Assert.IsFalse(sut.Contains(item0), "null");
    }

    #endregion

    #region CopyTo(), ToArray()

    [Test]
    public void CopyTo()
    {
      FileContainerList sut = CreateTestObject();

      FileContainer[] a1 = new FileContainer[3];
      sut.CopyTo(a1);
      CollectionAssert.AreEqual(new FileContainer[] { sut[0], sut[1], sut[2] }, a1, "1 arg");

      FileContainer[] a2 = new FileContainer[5];
      sut.CopyTo(a2, 1);
      CollectionAssert.AreEqual(new FileContainer[] { null, sut[0], sut[1], sut[2], null }, a2, "2 args");

      FileContainer[] a3 = new FileContainer[3];
      sut.CopyTo(1, a3, 2, 1);
      CollectionAssert.AreEqual(new FileContainer[] { null, null, sut[1] }, a3, "3 args");
    }


    [Test]
    public void ToArray()
    {
      FileContainerList sut = CreateTestObject();
      FileContainer[] res = sut.ToArray();
      CollectionAssert.AreEqual(new FileContainer[] { sut[0], sut[1], sut[2] }, res);
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      FileContainerList sut = CreateTestObject();
      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      FileContainer item = new FileContainer("Test4.bin", GetContent(4000), "");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Add(item); }, "Add()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Remove(item); }, "Remove()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Clear(); }, "Clear()");
    }

    #endregion

    #region Save()

    [Test]
    public void Save()
    {
      FileContainerList sut = CreateTestObject();
      using (TempDirectory dir = new TempDirectory())
      {
        sut.Save(dir.Dir);

        AbsPath file1 = new AbsPath(dir.Dir, "Test1.bin");
        AbsPath file2 = new AbsPath(dir.Dir, "aaa", "Test2.bin");
        AbsPath file3 = new AbsPath(dir.Dir, "aaa", "bbb", "Test3.bin");
        Assert.IsTrue(File.Exists(file1.Path), "Test1 exists");
        Assert.IsTrue(File.Exists(file2.Path), "Test2 exists");
        Assert.IsTrue(File.Exists(file3.Path), "Test3 exists");
        Assert.AreEqual(1000L, new FileInfo(file1.Path).Length, "Test1 length");
        Assert.AreEqual(2000L, new FileInfo(file2.Path).Length, "Test2 length");
        Assert.AreEqual(3000L, new FileInfo(file3.Path).Length, "Test3 length");
      }
    }

    #endregion

    #region GetFileNames()

    [Test]
    public void GetFileNames()
    {
      FileContainerList sut = CreateTestObject();
      string[] res = sut.GetFileNames();

      string[] wantedRes = new string[] {
        "Test1.bin",
        "aaa" + Path.DirectorySeparatorChar + "Test2.bin",
        "aaa" + Path.DirectorySeparatorChar + "bbb" + Path.DirectorySeparatorChar + "Test3.bin"  };
      CollectionAssert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone([Values(false, true)]bool setReadOnly)
    {
      FileContainerList sut = CreateTestObject();
      if (setReadOnly)
        sut.SetReadOnly();

      FileContainerList res = sut.Clone();
      Assert.AreNotSame(sut, res, "Not same");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");

      CollectionAssert.AreEqual(sut.GetFileNames(), res.GetFileNames(), "GetFileNames()");
      foreach (FileContainer fcSrc in sut)
      {
        FileContainer fcRes = res[fcSrc.FileInfo.Name];
        CollectionAssert.AreEqual(fcSrc.Content, fcRes.Content, "Content " + fcSrc.FileInfo.Name);
      }
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      FileContainerList sut = CreateTestObject();

      List<string> lst = new List<string>();
      foreach (FileContainer fc in sut)
        lst.Add(fc.SubDir + fc.FileInfo.Name);

      CollectionAssert.AreEqual(sut.GetFileNames(), lst);
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      FileContainerList sut = FileContainerList.Empty;
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      FileContainerList sut = CreateTestObject();
      byte[] b = SerializationTools.SerializeBinary(sut);

      FileContainerList res = (FileContainerList)(SerializationTools.DeserializeBinary(b));

      CollectionAssert.AreEqual(sut.GetFileNames(), res.GetFileNames(), "GetFileNames()");
      foreach (FileContainer fcSrc in sut)
      {
        FileContainer fcRes = res[fcSrc.FileInfo.Name];
        CollectionAssert.AreEqual(fcSrc.Content, fcRes.Content, "Content " + fcSrc.FileInfo.Name);
      }
    }

    #endregion
  }
}
