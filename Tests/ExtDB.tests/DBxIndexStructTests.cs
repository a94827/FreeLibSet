using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using System.Data;
using FreeLibSet.Data.SQLite;
using FreeLibSet.Remoting;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxIndexStructTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxIndexStruct sut = new DBxIndexStruct("Index001", new DBxColumns("F1,F2,F3"));
      Assert.AreEqual("Index001", sut.IndexName, "IndexName");
      Assert.AreEqual(new string[] { "F1", "F2", "F3" }, sut.Columns.AsArray, "Columns");
      Assert.AreEqual("", sut.Comment, "Comment");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
    }

    #endregion

    #region Comment

    [Test]
    public void Comment()
    {
      DBxIndexStruct sut = new DBxIndexStruct("Index1", new DBxColumns("F1"));
      sut.Comment = "XXX";
      Assert.AreEqual("XXX", sut.Comment);
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      DBxIndexStruct sut = new DBxIndexStruct("Index1", new DBxColumns("F1"));

      DBxTableStruct ts = new DBxTableStruct("T1");
      ts.Columns.AddId();
      ts.Columns.AddString("F1", 10, false);
      ts.Indexes.Add(sut);

      ts.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly);
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Comment = "XXX"; }, "Comment");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      DBxIndexStruct sut = new DBxIndexStruct("Index123", new DBxColumns("F1"));
      sut.Comment = "XXX";
      DBxTableStruct ts = new DBxTableStruct("T1");
      ts.Columns.AddId();
      ts.Columns.AddString("F1", 10, false);
      ts.Indexes.Add(sut);
      ts.SetReadOnly();

      DBxIndexStruct res1 = sut.Clone();
      Assert.AreEqual("Index123", res1.IndexName);
      DoTestClone(res1, "#1");

      DBxIndexStruct res2 = sut.Clone("Index456");
      Assert.AreEqual("Index456", res2.IndexName);
      DoTestClone(res2, "#2");
    }

    private static void DoTestClone(DBxIndexStruct res, string messagePrefix)
    {
      Assert.AreEqual("F1", res.Columns.AsString, messagePrefix + "-Columns");
      Assert.AreEqual("XXX", res.Comment, messagePrefix + "-Comment");
      Assert.IsFalse(res.IsReadOnly, messagePrefix + "-IsReadOnly");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxIndexStruct sut = new DBxIndexStruct("Index123", new DBxColumns("F1"));
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxIndexStruct res = (DBxIndexStruct)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.IndexName, res.IndexName);
    }

    #endregion
  }
}
