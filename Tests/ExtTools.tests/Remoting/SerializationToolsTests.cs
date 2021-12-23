using FreeLibSet.Core;
using FreeLibSet.Remoting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ExtTools_tests.Remoting
{
  [TestFixture]
  public class SerializationToolsTests
  {
    #region IsMarshallable()

    private class TestClassMBR : MarshalByRefObject
    { 
    }

    [Serializable]
    private class TestClassMBV
    { 
    }

    private class TestClassNS
    { 
    }

    [Test]
    public void IsMarshallable()
    {
      Assert.IsTrue(SerializationTools.IsMarshallable(new TestClassMBR()), "MBR");
      Assert.IsTrue(SerializationTools.IsMarshallable(new TestClassMBV()), "MBV");
      Assert.IsFalse(SerializationTools.IsMarshallable(new TestClassNS()), "NS");
    }

    #endregion

    #region De/SerializeBinary()

    [Test]
    public void SerializeBinary()
    {
      string obj1 = "Hello";
      byte[] b = SerializationTools.SerializeBinary(obj1);

      object res2 = SerializationTools.DeserializeBinary(b);

      Assert.AreEqual(obj1, res2);
    }

    #endregion

    #region PrepareDataSet()

    [Test]
    public void PrepareDataSet()
    {
      DataSet ds = new DataSet();
      DataTable tbl = ds.Tables.Add("T1");
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add(1);
      Assert.AreEqual(DataRowState.Added, tbl.Rows[0].RowState, "Before");

      SerializationTools.PrepareDataSet(ds);
      Assert.AreEqual(DataRowState.Unchanged, tbl.Rows[0].RowState, "After");

      // Не проверяем GetPreferredRemotingFormat(), т.к. реализация может измениться
    }

    #endregion

    #region SetUnspecifiedDateTimeMode()

    [Test]
    public void SetUnspecifiedDateTimeMode()
    {
      DataSet ds = new DataSet();
      DataTable tbl = ds.Tables.Add("T1");
      DataColumn col1 = tbl.Columns.Add("Date1", typeof(DateTime));
      DataColumn col2 = tbl.Columns.Add("Date2", typeof(DateTime));
      col2.DateTimeMode = DataSetDateTime.Utc;
      DataColumn col3 = tbl.Columns.Add("Date3", typeof(DateTime));
      col3.DateTimeMode = DataSetDateTime.Local;

      SerializationTools.SetUnspecifiedDateTimeMode(ds);

      Assert.AreEqual(DataSetDateTime.Unspecified, col1.DateTimeMode, "#1");
      Assert.AreEqual(DataSetDateTime.Utc, col2.DateTimeMode, "#2");
      Assert.AreEqual(DataSetDateTime.Local, col3.DateTimeMode, "#3");
    }

    #endregion
  }
}
