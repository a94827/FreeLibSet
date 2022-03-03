using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DBxMemoryDocValuesTests
  {
    #region Тестирование конструктора

    [Test]
    public void Constructor()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(new string[] { "F1", "F2" }, sut.Names, "Names");
    }

    #endregion

    #region Item

    [Test]
    public void Item()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });
      DBxDocValue item1 = sut["F1"];
      Assert.AreEqual("F1", item1.Name, "#1");

      DBxDocValue item2 = sut[1];
      Assert.AreEqual("F2", item2.Name, "#2");

      DBxDocValue dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "#3");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[2]; }, "#4");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut["XXX"]; }, "#5");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut[""]; }, "#6");
    }

    [Test]
    public void GetName()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });
      Assert.AreEqual("F2", sut.GetName(1));
    }


    [Test]
    public void IndexOf()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });
      Assert.AreEqual(1, sut.IndexOf("F2"), "#1");
      Assert.AreEqual(-1, sut.IndexOf("XXX"), "#2");
      Assert.AreEqual(-1, sut.IndexOf(""), "#3");
    }

    [Test]
    public void IsReadOnly()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });
      Assert.IsFalse(sut.IsReadOnly, "#1");
      sut.SetValue(1, 123);

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "#2");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SetValue(1, 123); }, "SetValue()");
    }

    #endregion

    #region Get/SetValue()

    [Test]
    public void GetValue()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });
      Assert.IsNull(sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#1");

      sut.SetValue(0, "123");
      Assert.AreEqual("123", sut.GetValue(0, DBxDocValuePreferredType.Unknown), "as Unknown");
    }

    [Test]
    public void SetValue()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1" });

      sut.SetValue(0, "XXX");
      Assert.AreEqual("XXX", sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#1");

      sut.SetValue(0, null);
      Assert.IsNull(sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#2");
    }

    #endregion

    #region IsNull()

    [Test]
    public void IsNull()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1" });
      Assert.IsTrue(sut.IsNull(0), "#1");
      sut.SetValue(0, "XYZ");
      Assert.IsFalse(sut.IsNull(0), "#2");
    }

    #endregion

    #region GetGrayed()

    [Test]
    public void GetGrayed()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1" });
      Assert.IsFalse(((IDBxDocValues)sut).GetGrayed(0));
    }

    #endregion

    #region AllowDBNull(), MaxLength(), GetValueReadOnly()

    [Test]
    public void AllowDBNull()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1" });
      Assert.IsTrue(((IDBxDocValues)sut).AllowDBNull(0));
    }

    [Test]
    public void MaxLength()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1" });
      Assert.AreEqual(-1, ((IDBxDocValues)sut).MaxLength(0));
    }


    [Test]
    public void GetValueReadOnly()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1" });
      Assert.IsFalse(((IDBxDocValues)sut).GetValueReadOnly(0));
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      DBxMemoryDocValues sut = new DBxMemoryDocValues(new string[] { "F1", "F2" });

      List<string> lst = new List<string>();
      foreach (DBxDocValue item in sut)
        lst.Add(item.Name);

      Assert.AreEqual(new string[] { "F1", "F2" }, lst.ToArray());
    }

    #endregion
  }
}
