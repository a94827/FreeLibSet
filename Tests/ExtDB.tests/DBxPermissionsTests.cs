using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxPermissionsTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxPermissions sut = new DBxPermissions();
      Assert.AreEqual(DBxAccessMode.Full, sut.DBMode, "DBMode");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
    }

    #endregion

    #region DBMode

    [Test]
    public void DBMode()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.DBMode = DBxAccessMode.None;
      sut.TableModes["T1"] = DBxAccessMode.Full;
      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      Assert.AreEqual(DBxAccessMode.None, sut.DBMode, "DBMode #1");
      Assert.AreEqual(DBxAccessMode.Full, sut.TableModes["T1"], "TableModes #1");
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T1", "F1"], "ColumnModes #1");

      sut.DBMode = DBxAccessMode.ReadOnly;
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.DBMode, "DBMode #2");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.TableModes["T1"], "TableModes #2");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.ColumnModes["T1", "F1"], "ColumnModes #2");
    }

    #endregion

    #region TableModes

    [Test]
    public void TableModes()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.TableModes["T1"] = DBxAccessMode.ReadOnly;
      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      sut.TableModes["T2"] = DBxAccessMode.ReadOnly;
      sut.ColumnModes["T2", "F1"] = DBxAccessMode.None;
      Assert.AreEqual(DBxAccessMode.Full, sut.DBMode, "DBMode #1");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.TableModes["T1"], "TableModes T1 #1");
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T1", "F1"], "ColumnModes T1 F1 #1");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.TableModes["T2"], "TableModes T2 #1");
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T2", "F1"], "ColumnModes T2 F1 #1");
      Assert.AreEqual(DBxAccessMode.Full, sut.TableModes["T3"], "T3", "TableModes T3 #1");
      Assert.AreEqual(DBxAccessMode.Full, sut.ColumnModes["T3", "F1"], "ColumnModes T3 F1 #1");

      sut.TableModes["T1"] = DBxAccessMode.Full;
      Assert.AreEqual(DBxAccessMode.Full, sut.DBMode, "DBMode #2");
      Assert.AreEqual(DBxAccessMode.Full, sut.TableModes["T1"], "TableModes T1 #2");
      Assert.AreEqual(DBxAccessMode.Full, sut.ColumnModes["T1", "F1"], "ColumnModes T1 F1 #2");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.TableModes["T2"], "TableModes T2 #2");
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T2", "F1"], "ColumnModes T2 F1 #2");
      Assert.AreEqual(DBxAccessMode.Full, sut.TableModes["T3"], "T3", "TableModes T3 #2");
      Assert.AreEqual(DBxAccessMode.Full, sut.ColumnModes["T3", "F1"], "ColumnModes T3 F1 #2");
    }

    #endregion

    #region ContainsTableModes()

    [Test]
    public void ContainsTableModes_0args()
    {
      DBxPermissions sut = new DBxPermissions();
      Assert.IsFalse(sut.ContainsTableModes(), "#1");
      sut.DBMode = DBxAccessMode.ReadOnly;
      Assert.IsFalse(sut.ContainsTableModes(), "#2");
      sut.TableModes["T1"] = DBxAccessMode.Full;
      Assert.IsTrue(sut.ContainsTableModes(), "#3");
      // Зависит от реализации
      //sut.TableModes["T2"] = DBxAccessMode.ReadOnly;
      //Assert.IsFalse(sut.ContainsTableModes(), "#4");
    }


    [Test]
    public void ContainsTableModes_1arg()
    {
      DBxPermissions sut = new DBxPermissions();
      Assert.IsTrue(sut.ContainsTableModes(DBxAccessMode.Full), "Full #1");
      Assert.IsFalse(sut.ContainsTableModes(DBxAccessMode.ReadOnly), "ReadOnly #1");
      Assert.IsFalse(sut.ContainsTableModes(DBxAccessMode.None), "None #1");

      sut.TableModes["T1"] = DBxAccessMode.ReadOnly;
      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      Assert.IsTrue(sut.ContainsTableModes(DBxAccessMode.Full), "Full #2");
      Assert.IsTrue(sut.ContainsTableModes(DBxAccessMode.ReadOnly), "ReadOnly #2");
      Assert.IsFalse(sut.ContainsTableModes(DBxAccessMode.None), "None #2");

      sut.TableModes["T1"] = DBxAccessMode.None;
      Assert.IsTrue(sut.ContainsTableModes(DBxAccessMode.Full), "Full #3");
      Assert.IsFalse(sut.ContainsTableModes(DBxAccessMode.ReadOnly), "ReadOnly #3");
      Assert.IsTrue(sut.ContainsTableModes(DBxAccessMode.None), "None #3");

      sut.DBMode = DBxAccessMode.ReadOnly;
      Assert.IsFalse(sut.ContainsTableModes(DBxAccessMode.Full), "Full #4");
      Assert.IsTrue(sut.ContainsTableModes(DBxAccessMode.ReadOnly), "ReadOnly #4");
      Assert.IsFalse(sut.ContainsTableModes(DBxAccessMode.None), "None #4");
    }

    #endregion

    #region ColumnModes

    [Test]
    public void ColumnModes()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      sut.ColumnModes["T1", "F2"] = DBxAccessMode.ReadOnly;
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T1", "F1"], "F1 #1");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.ColumnModes["T1", "F2"], "F2 #1");
      Assert.AreEqual(DBxAccessMode.Full, sut.ColumnModes["T1", "F3"], "F3 #1");

      sut.ColumnModes["T1", "F1"] = DBxAccessMode.Full;
      Assert.AreEqual(DBxAccessMode.Full, sut.ColumnModes["T1", "F1"], "F1 #2");
      Assert.AreEqual(DBxAccessMode.ReadOnly, sut.ColumnModes["T1", "F2"], "F2 #2");
      Assert.AreEqual(DBxAccessMode.Full, sut.ColumnModes["T1", "F3"], "F3 #2");

      sut.TableModes["T1"] = DBxAccessMode.None;
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T1", "F1"], "F1 #3");
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T1", "F2"], "F2 #3");
      Assert.AreEqual(DBxAccessMode.None, sut.ColumnModes["T1", "F3"], "F3 #3");
    }

    #endregion

    #region ContainsColumnModes

    [Test]
    public void ContainsColumnModes_1arg()
    {
      DBxPermissions sut = new DBxPermissions();
      Assert.IsFalse(sut.ContainsColumnModes("T1"), "#1");

      sut.TableModes["T1"] = DBxAccessMode.ReadOnly;
      Assert.IsFalse(sut.ContainsColumnModes("T1"), "#2");

      sut.ColumnModes["T1", "F1"] = DBxAccessMode.ReadOnly;
      Assert.IsFalse(sut.ContainsColumnModes("T1"), "#3");

      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      Assert.IsTrue(sut.ContainsColumnModes("T1"), "#4");

      // Зависит от реализации
      //sut.TableModes["T1"] = DBxAccessMode.Full;
      //Assert.IsFalse(sut.ContainsColumnModes("T1"), "#5");
    }


    [Test]
    public void ContainsColumnModes_2arg()
    {
      DBxPermissions sut = new DBxPermissions();
      Assert.IsTrue(sut.ContainsColumnModes("T1", DBxAccessMode.Full), "Full #1");
      Assert.IsFalse(sut.ContainsColumnModes("T1", DBxAccessMode.ReadOnly), "ReadOnly #1");
      Assert.IsFalse(sut.ContainsColumnModes("T1", DBxAccessMode.None), "None #1");

      sut.TableModes["T1"] = DBxAccessMode.ReadOnly;
      Assert.IsFalse(sut.ContainsColumnModes("T1", DBxAccessMode.Full), "Full #2");
      Assert.IsTrue(sut.ContainsColumnModes("T1", DBxAccessMode.ReadOnly), "ReadOnly #2");
      Assert.IsFalse(sut.ContainsColumnModes("T1", DBxAccessMode.None), "None #2");

      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      Assert.IsFalse(sut.ContainsColumnModes("T1", DBxAccessMode.Full), "Full #3");
      Assert.IsTrue(sut.ContainsColumnModes("T1", DBxAccessMode.ReadOnly), "ReadOnly #3");
      Assert.IsTrue(sut.ContainsColumnModes("T1", DBxAccessMode.None), "None #3");
    }

    #endregion

    #region Min()/Max()

    [TestCase(DBxAccessMode.Full, DBxAccessMode.Full, DBxAccessMode.Full)]
    [TestCase(DBxAccessMode.Full, DBxAccessMode.ReadOnly, DBxAccessMode.ReadOnly)]
    [TestCase(DBxAccessMode.Full, DBxAccessMode.None, DBxAccessMode.None)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.ReadOnly, DBxAccessMode.ReadOnly)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.None, DBxAccessMode.None)]
    [TestCase(DBxAccessMode.None, DBxAccessMode.None, DBxAccessMode.None)]
    public void Min(DBxAccessMode a, DBxAccessMode b, DBxAccessMode wantedRes)
    {
      Assert.AreEqual(wantedRes, DBxPermissions.Min(a, b), "#1");
      Assert.AreEqual(wantedRes, DBxPermissions.Min(b, a), "#2");
    }

    [TestCase(DBxAccessMode.Full, DBxAccessMode.Full, DBxAccessMode.Full)]
    [TestCase(DBxAccessMode.Full, DBxAccessMode.ReadOnly, DBxAccessMode.Full)]
    [TestCase(DBxAccessMode.Full, DBxAccessMode.None, DBxAccessMode.Full)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.ReadOnly, DBxAccessMode.ReadOnly)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.None, DBxAccessMode.ReadOnly)]
    [TestCase(DBxAccessMode.None, DBxAccessMode.None, DBxAccessMode.None)]
    public void Max(DBxAccessMode a, DBxAccessMode b, DBxAccessMode wantedRes)
    {
      Assert.AreEqual(wantedRes, DBxPermissions.Max(a, b), "#1");
      Assert.AreEqual(wantedRes, DBxPermissions.Max(b, a), "#2");
    }

    #endregion

    #region Compare()

    [TestCase(DBxAccessMode.Full, DBxAccessMode.Full, 0)]
    [TestCase(DBxAccessMode.Full, DBxAccessMode.ReadOnly, 1)]
    [TestCase(DBxAccessMode.Full, DBxAccessMode.None, 1)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.Full, -1)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.ReadOnly, 0)]
    [TestCase(DBxAccessMode.ReadOnly, DBxAccessMode.None, 1)]
    [TestCase(DBxAccessMode.None, DBxAccessMode.Full, -1)]
    [TestCase(DBxAccessMode.None, DBxAccessMode.ReadOnly, -1)]
    [TestCase(DBxAccessMode.None, DBxAccessMode.None, 0)]
    public void Compare(DBxAccessMode a, DBxAccessMode b, int wantedRes)
    {
      Assert.AreEqual(wantedRes, Math.Sign(DBxPermissions.Compare(a, b)));
    }

    #endregion

    #region Статические экземпляры

    [Test]
    public void FullAccess()
    {
      Assert.AreEqual(DBxAccessMode.Full, DBxPermissions.FullAccess.DBMode, "DBMode");
      Assert.IsFalse(DBxPermissions.FullAccess.ContainsTableModes(), "ContainsTableModes()");
      Assert.IsTrue(DBxPermissions.FullAccess.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void ReadOnlyAccess()
    {
      Assert.AreEqual(DBxAccessMode.ReadOnly, DBxPermissions.ReadOnlyAccess.DBMode, "DBMode");
      Assert.IsFalse(DBxPermissions.ReadOnlyAccess.ContainsTableModes(), "ContainsTableModes()");
      Assert.IsTrue(DBxPermissions.ReadOnlyAccess.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.DBMode = DBxAccessMode.ReadOnly; }, "DBMode");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.TableModes["T1"] = DBxAccessMode.ReadOnly; }, "TableModes");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.ColumnModes["T1", "F1"] = DBxAccessMode.ReadOnly; }, "ColumnModes");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone_noTables()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.DBMode = DBxAccessMode.ReadOnly;
      sut.SetReadOnly();

      DBxPermissions res = sut.Clone();
      Assert.AreEqual(DBxAccessMode.ReadOnly, res.DBMode, "DBMode");
      Assert.IsFalse(res.ContainsTableModes(), "ContainsTableModes()");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Clone()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.TableModes["T1"] = DBxAccessMode.ReadOnly;
      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;
      sut.SetReadOnly();

      DBxPermissions res = sut.Clone();
      Assert.AreEqual(DBxAccessMode.Full, res.DBMode, "DBMode");
      Assert.AreEqual(DBxAccessMode.ReadOnly, res.TableModes["T1"], "TableModes");
      Assert.AreEqual(DBxAccessMode.None, res.ColumnModes["T1", "F1"], "ColumnModes");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxPermissions sut = new DBxPermissions();
      sut.TableModes["T1"] = DBxAccessMode.ReadOnly;
      sut.ColumnModes["T1", "F1"] = DBxAccessMode.None;

      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxPermissions res = (DBxPermissions)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(DBxAccessMode.Full, res.DBMode, "DBMode");
      Assert.AreEqual(DBxAccessMode.ReadOnly, res.TableModes["T1"], "TableModes");
      Assert.AreEqual(DBxAccessMode.None, res.ColumnModes["T1", "F1"], "ColumnModes");
    }

    #endregion
  }
}
