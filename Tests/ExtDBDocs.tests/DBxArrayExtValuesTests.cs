using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DBxArrayExtValuesTests
  {
    #region Тестирование конструктора

    [Test]
    public void Constructor()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(new string[] { "F1", "F2" }, sut.Names, "Names");
    }

    #endregion

    #region Item

    [Test]
    public void Item()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });
      DBxExtValue item1 = sut["F1"];
      Assert.AreEqual("F1", item1.Name, "#1");

      DBxExtValue item2 = sut[1];
      Assert.AreEqual("F2", item2.Name, "#2");

      DBxExtValue dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "#3");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[2]; }, "#4");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut["XXX"]; }, "#5");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut[""]; }, "#6");
    }

    [Test]
    public void GetName()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });
      Assert.AreEqual("F2", sut.GetName(1));
    }


    [Test]
    public void IndexOf()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });
      Assert.AreEqual(1, sut.IndexOf("F2"), "#1");
      Assert.AreEqual(-1, sut.IndexOf("XXX"), "#2");
      Assert.AreEqual(-1, sut.IndexOf(""), "#3");
    }

    [Test]
    public void IsReadOnly()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });
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
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });
      Assert.IsNull(sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#1");

      sut.SetValue(0, "123");
      Assert.AreEqual("123", sut.GetValue(0, DBxExtValuePreferredType.Unknown), "as Unknown");
    }

    [Test]
    public void SetValue()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1" });

      sut.SetValue(0, "XXX");
      Assert.AreEqual("XXX", sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#1");

      sut.SetValue(0, null);
      Assert.IsNull(sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#2");
    }

    #endregion

    #region IsNull()

    [Test]
    public void IsNull()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1" });
      Assert.IsTrue(sut.IsNull(0), "#1");
      sut.SetValue(0, "XYZ");
      Assert.IsFalse(sut.IsNull(0), "#2");
    }

    #endregion

    #region GetGrayed()

    [Test]
    public void GetGrayed()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1" });
      Assert.IsFalse(((IDBxExtValues)sut).GetGrayed(0));
    }

    #endregion

    #region AllowDBNull(), MaxLength(), GetValueReadOnly()

    [Test]
    public void AllowDBNull()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1" });
      Assert.IsTrue(((IDBxExtValues)sut).AllowDBNull(0));
    }

    [Test]
    public void MaxLength()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1" });
      Assert.AreEqual(-1, ((IDBxExtValues)sut).MaxLength(0));
    }


    [Test]
    public void GetValueReadOnly()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1" });
      Assert.IsFalse(((IDBxExtValues)sut).GetValueReadOnly(0));
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      DBxArrayExtValues sut = new DBxArrayExtValues(new string[] { "F1", "F2" });

      List<string> lst = new List<string>();
      foreach (DBxExtValue item in sut)
        lst.Add(item.Name);

      Assert.AreEqual(new string[] { "F1", "F2" }, lst.ToArray());
    }

    #endregion
  }
}
