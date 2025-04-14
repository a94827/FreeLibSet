using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Core;
using System.Data;
using FreeLibSet.Collections;
using System.Globalization;
using System.Resources;

namespace ExtTools_tests.Core
{
  // Не могу сделать SetCultureAttribute для двух культур.
  // Придется сделать два класса, фу.
  public abstract class ExceptionFactoryTests
  {
    #region Конструктор с культурой

    //protected ExceptionFactoryTests(string cultureStr)
    //{
    //  _ThisCultureStr = cultureStr;
    //}

    //private string _ThisCultureStr;

    //private CultureInfo _OldCulture;

    //[SetUp]
    //public void Setup()
    //{
    //  _OldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
    //  CultureInfo thisCulture = CultureInfo.GetCultureInfo(_ThisCultureStr);
    //  if (thisCulture == null)
    //    throw new BugException();
    //  System.Threading.Thread.CurrentThread.CurrentCulture = thisCulture;
    //}

    //[TearDown]
    //public void TearDown()
    //{
    //  System.Threading.Thread.CurrentThread.CurrentCulture = _OldCulture;
    //}

    #endregion

    const string ParamName1 = "AAA";
    const string ParamName2 = "BBB";
    const string DummyValue1 = "XXX";
    const string DummyValue2 = "YYY";
    const string DummyValue3 = "ZZZ";

    #region Проверка аргументов


    [Test]
    public void ArgIsEmpty()
    {
      ArgumentException res = ExceptionFactory.ArgIsEmpty(ParamName1);
      Assert.AreEqual(ParamName1, res.ParamName);
    }

    [Test]
    public void ArgStringIsNullOrEmpty()
    {
      ArgumentException res = ExceptionFactory.ArgStringIsNullOrEmpty(ParamName1);
      Assert.AreEqual(ParamName1, res.ParamName);
    }

    [Test]
    public void ArgUnknownValue_2args()
    {
      ArgumentException res = ExceptionFactory.ArgUnknownValue(ParamName1, DummyValue1);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(DummyValue1, res.Message);
    }

    [Test]
    public void ArgUnknownValue_3args_list()
    {
      ArgumentException res = ExceptionFactory.ArgUnknownValue(ParamName1, DummyValue1, new object[] { DummyValue2, DummyValue3, null });
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(DummyValue1, res.Message);
      StringAssert.Contains(DummyValue2, res.Message);
      StringAssert.Contains(DummyValue3, res.Message);
      StringAssert.Contains("null", res.Message);
    }

    [Test]
    public void ArgUnknownValue_3args_null()
    {
      ArgumentException res = ExceptionFactory.ArgUnknownValue(ParamName1, DummyValue1, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(DummyValue1, res.Message);
    }

    [Test]
    public void ArgUnknownType_string()
    {
      ArgumentException res = ExceptionFactory.ArgUnknownType(ParamName1, "ABC");
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("String", res.Message);
    }

    [Test]
    public void ArgUnknownType_Int64()
    {
      ArgumentException res = ExceptionFactory.ArgUnknownType(ParamName1, 123L);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("Int64", res.Message);
    }

    [Test]
    public void ArgUnknownType_null()
    {
      ArgumentException res = ExceptionFactory.ArgUnknownType(ParamName1, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("null", res.Message);
    }

    [Test]
    public void ArgNoType_String()
    {
      ArgumentException res = ExceptionFactory.ArgNoType(ParamName1, "ABC", typeof(IComparable<Int32>));
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("String", res.Message);
      StringAssert.Contains("IComparable", res.Message);
    }


    [Test]
    public void ArgNoType_null()
    {
      ArgumentException res = ExceptionFactory.ArgNoType(ParamName1, null, typeof(Int32));
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("null", res.Message);
      StringAssert.Contains("Int32", res.Message);
    }

    [Test]
    public void ArgOutOfRange_both()
    {
      ArgumentOutOfRangeException res = ExceptionFactory.ArgOutOfRange(ParamName1, 123, 456, 789);
      Assert.AreEqual(ParamName1, res.ParamName);
      Assert.AreEqual(123, res.ActualValue);
      StringAssert.Contains("456", res.Message);
      StringAssert.Contains("789", res.Message);
    }

    [Test]
    public void ArgOutOfRange_min()
    {
      ArgumentOutOfRangeException res = ExceptionFactory.ArgOutOfRange(ParamName1, 123, 456, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      Assert.AreEqual(123, res.ActualValue);
      StringAssert.Contains("456", res.Message);
    }

    [Test]
    public void ArgOutOfRange_max()
    {
      ArgumentOutOfRangeException res = ExceptionFactory.ArgOutOfRange(ParamName1, 123, null, -789);
      Assert.AreEqual(ParamName1, res.ParamName);
      Assert.AreEqual(123, res.ActualValue);
      StringAssert.Contains("-789", res.Message);
    }

    [Test]
    public void ArgOutOfRange_none()
    {
      ArgumentOutOfRangeException res = ExceptionFactory.ArgOutOfRange(ParamName1, 123, null, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      Assert.AreEqual(123, res.ActualValue);
    }

    [Test]
    public void ArgOutOfRange_inv()
    {
      ArgumentOutOfRangeException res = ExceptionFactory.ArgOutOfRange(ParamName1, 123, 456, 455);
      Assert.AreEqual(ParamName1, res.ParamName);
      Assert.AreEqual(123, res.ActualValue);
    }

    [Test]
    public void ArgRangeInverted()
    {
      ArgumentException res = ExceptionFactory.ArgRangeInverted(ParamName1, 111, ParamName2, 110);
      Assert.AreEqual(ParamName2, res.ParamName);
      StringAssert.Contains("110", res.Message);
      StringAssert.Contains("111", res.Message);
    }

    [Test]
    public void ArgBadChar_pos()
    {
      ArgumentException res = ExceptionFactory.ArgInvalidChar(ParamName1, "ABCDEF", 3);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("D", res.Message);
      StringAssert.Contains("4", res.Message); // +1 для номера позиции
    }

    [Test]
    public void ArgBadChar_char()
    {
      ArgumentException res = ExceptionFactory.ArgInvalidChar(ParamName1, "ABCDEF", "D");
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("D", res.Message);
      StringAssert.Contains("4", res.Message); // +1 для номера позиции
    }

    [Test]
    public void ArgWrongCollectionCount()
    {
      string[] a = new string[] { "XXX", "YYY", "ZZZ" };
      ArgumentException res = ExceptionFactory.ArgWrongCollectionCount(ParamName1, a, 4);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("3", res.Message);
      StringAssert.Contains("4", res.Message);
    }

    [Test]
    public void ArgProperty_noReq()
    {
      List<string> testObj = new List<string>();
      testObj.Add("XXX");
      testObj.Add("YYY");
      testObj.Add("ZZZ");

      ArgumentException res = ExceptionFactory.ArgProperty(ParamName1, testObj, "Count", 3, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("Count", res.Message);
      StringAssert.Contains("3", res.Message);
    }

    [Test]
    public void ArgProperty_reqs()
    {
      List<string> testObj = new List<string>();
      testObj.Add("XXX");
      testObj.Add("YYY");
      testObj.Add("ZZZ");

      ArgumentException res = ExceptionFactory.ArgProperty(ParamName1, testObj, "Count", 3, new object[] { 666, 777, null });
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("Count", res.Message);
      StringAssert.Contains("3", res.Message);
      StringAssert.Contains("666", res.Message);
      StringAssert.Contains("777", res.Message);
      StringAssert.Contains("null", res.Message); // этого, конечно, в реальности быть не должно
    }

    [Test]
    public void ArgProperty_null()
    {
      List<string> testObj = new List<string>();

      ArgumentException res = ExceptionFactory.ArgProperty(ParamName1, testObj, "FakeProp", null, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("FakeProp", res.Message);
      StringAssert.Contains("null", res.Message);
    }

    [Test]
    public void ArgCollectionSameAsThis()
    {
      ArgumentException res = ExceptionFactory.ArgCollectionSameAsThis(ParamName1);
      Assert.AreEqual(ParamName1, res.ParamName);
    }

    [Test]
    public void ArgAreSame()
    {
      ArgumentException res = ExceptionFactory.ArgAreSame(ParamName1, ParamName2);
      Assert.AreEqual(ParamName2, res.ParamName);
      StringAssert.Contains(ParamName1, res.Message);
    }

    [Test]
    public void ArgIncompatibleTypes()
    {
      ArgumentException res = ExceptionFactory.ArgIncompatibleTypes(ParamName1, 123, ParamName2, "XXX");
      Assert.AreEqual(ParamName2, res.ParamName);
      StringAssert.Contains(ParamName1, res.Message);
      StringAssert.Contains("Int32", res.Message);
      StringAssert.Contains("String", res.Message);
    }

    [Test]
    public void ArgIncompatibleTypes_null()
    {
      ArgumentException res = ExceptionFactory.ArgIncompatibleTypes(ParamName1, 123, ParamName2, null);
      Assert.AreEqual(ParamName2, res.ParamName);
      StringAssert.Contains(ParamName1, res.Message);
      StringAssert.Contains("Int32", res.Message);
    }

    [Test]
    public void ArgStringsWithDifferentLength()
    {
      ArgumentException res = ExceptionFactory.ArgStringsWithDifferentLength(ParamName1, "ABCDEF", ParamName2, "ABCDE");
      Assert.AreEqual(ParamName2, res.ParamName);
      //StringAssert.Contains(ParamName1, res.Message);
      StringAssert.Contains("6", res.Message);
      StringAssert.Contains("5", res.Message);
    }

    [Test]
    public void ArgStringsWithDifferentLength_null()
    {
      ArgumentException res = ExceptionFactory.ArgStringsWithDifferentLength(ParamName1, "ABCDEF", ParamName2, null);
      Assert.AreEqual(ParamName2, res.ParamName);
      //StringAssert.Contains(ParamName1, res.Message);
      StringAssert.Contains("6", res.Message);
      StringAssert.Contains("0", res.Message);
    }

    [Test]
    public void ArgInvalidEnumerableType()
    {
      object[] a = new object[] { null, 123L, 456L };
      ArgumentException res = ExceptionFactory.ArgInvalidEnumerableType(ParamName1, a);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("Int64", res.Message);
    }

    [Test]
    public void ArgInvalidEnumerableType_empty()
    {
      object[] a = new object[0];
      ArgumentException res = ExceptionFactory.ArgInvalidEnumerableType(ParamName1, a);
      Assert.AreEqual(ParamName1, res.ParamName);
    }

    [Test]
    public void ArgInvalidListItem()
    {
      object[] a = new object[] { null, 123L, 456L };
      ArgumentException res = ExceptionFactory.ArgInvalidListItem(ParamName1, a, 2);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("2", res.Message);
      StringAssert.Contains("456", res.Message);
    }

    [Test]
    public void ArgInvalidEnumerableItem()
    {
      object[] a = new object[] { null, 123L, 456L };
      ArgumentException res = ExceptionFactory.ArgInvalidEnumerableItem(ParamName1, a, 456L);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("2", res.Message);
      StringAssert.Contains("456", res.Message);
    }

    [Test]
    public void ArgInvalidEnumerableItem_null()
    {
      object[] a = new object[] { null, 123L, 456L };
      ArgumentException res = ExceptionFactory.ArgInvalidEnumerableItem(ParamName1, a, null);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("0", res.Message);
      StringAssert.Contains("null", res.Message);
    }

    private class DummyObjectWithCollection
    {
      public DummyObjectWithCollection()
      {
        _Items = new ItemCollection();
      }

      private class ItemCollection : List<string>
      {
        public override string ToString()
        {
          return "TestItemCollection";
        }
      }

      public ICollection<string> Items { get { return _Items; } }
      private readonly ItemCollection _Items;

      public override string ToString()
      {
        return "TestObject";
      }
    }

    [Test]
    public void ArgNotInCollection_Prop()
    {
      DummyObjectWithCollection obj = new DummyObjectWithCollection();
      obj.Items.Add("AAA");
      obj.Items.Add("BBB");

      string param1 = "CCC";
      ArgumentException res = ExceptionFactory.ArgNotInCollection(ParamName1, param1, obj, "Items", (System.Collections.ICollection)obj.Items);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("CCC", res.Message);
      StringAssert.Contains(obj.ToString(), res.Message);
      StringAssert.Contains("Items", res.Message);
      StringAssert.Contains(obj.Items.ToString(), res.Message);
    }


    [Test]
    public void ArgNotInCollection_List()
    {
      List<string> obj = new List<string>();
      obj.Add("AAA");
      obj.Add("BBB");

      string param1 = "CCC";
      ArgumentException res = ExceptionFactory.ArgNotInCollection(ParamName1, param1, (System.Collections.ICollection)obj);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains("CCC", res.Message);
      StringAssert.Contains(obj.ToString(), res.Message);
    }

    private class DummyObjectWithCode : FreeLibSet.Collections.IObjectWithCode
    {
      public string Code { get { return _Code ?? String.Empty; } set { _Code = value; } }
      private string _Code;
    }

    [Test]
    public void ArgObjectWithoutCode()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();
      ArgumentException res = ExceptionFactory.ArgObjectWithoutCode(ParamName1, obj);
      Assert.AreEqual(ParamName1, res.ParamName);
    }

    #endregion

    [Test]
    public void Inconvertible()
    {
      Exception res = ExceptionFactory.Inconvertible(123, typeof(Guid));
      StringAssert.Contains("Int32", res.Message);
      StringAssert.Contains("Guid", res.Message);
    }

    #region InvalidOperationException

    private struct DummyStruct
    {
#pragma warning disable 0649
      public int F1;
#pragma warning restore 0649
    }

    [Test]
    public void StructureNotInit()
    {
      InvalidOperationException res = ExceptionFactory.StructureNotInit(typeof(DummyStruct));
      StringAssert.Contains("DummyStruct", res.Message);
    }

    [Test]
    public void ObjectProperty_noReq()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();
      obj.Code = "XXX";

      InvalidOperationException res = ExceptionFactory.ObjectProperty(obj, "Code", "XXX", null);
      StringAssert.Contains("Code", res.Message);
      StringAssert.Contains("XXX", res.Message);
    }

    [Test]
    public void ObjectProperty_req()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();
      obj.Code = "XXX";

      InvalidOperationException res = ExceptionFactory.ObjectProperty(obj, "Code", "XXX", new object[] { "YYY", "ZZZ", null });
      StringAssert.Contains("Code", res.Message);
      StringAssert.Contains("XXX", res.Message);
      StringAssert.Contains("YYY", res.Message);
      StringAssert.Contains("ZZZ", res.Message);
      StringAssert.Contains("null", res.Message);
    }

    [Test]
    public void ObjectPropertyNotSet()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();

      InvalidOperationException res = ExceptionFactory.ObjectPropertyNotSet(obj, "Code");
      StringAssert.Contains("DummyObjectWithCode", res.Message);
      StringAssert.Contains("Code", res.Message);
    }

    [Test]
    public void ObjectPropertyAlreadySet()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();
      obj.Code = "XXX";

      InvalidOperationException res = ExceptionFactory.ObjectPropertyAlreadySet(obj, "Code");
      StringAssert.Contains("DummyObjectWithCode", res.Message);
      StringAssert.Contains("Code", res.Message);
    }

    [Test]
    public void ObjectPropertySwitch()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();
      //obj.Code = "XXX";

      InvalidOperationException res = ExceptionFactory.ObjectPropertySwitch(obj, "Code", "XXX", "YYY");
      StringAssert.Contains("DummyObjectWithCode", res.Message);
      StringAssert.Contains("Code", res.Message);
      StringAssert.Contains("XXX", res.Message);
      StringAssert.Contains("YYY", res.Message);
    }

    [Test]
    public void ObjectMethodNotCalled()
    {
      DummyObjectWithCode obj = new DummyObjectWithCode();

      InvalidOperationException res = ExceptionFactory.ObjectMethodNotCalled(obj, "MyMethod()");
      StringAssert.Contains("DummyObjectWithCode", res.Message);
      StringAssert.Contains("MyMethod", res.Message);
    }

    private class DummyObjectWithEvent
    {
      public event EventHandler Click;

      public bool HasClick { get { return Click != null; } }

      public void PerformClick()
      {
        if (Click != null)
          Click(this, EventArgs.Empty);
      }
    }


    [Test]
    public void ObjectEventHandlerNotSet()
    {
      DummyObjectWithEvent obj = new DummyObjectWithEvent();

      InvalidOperationException res = ExceptionFactory.ObjectEventHandlerNotSet(obj, "Click");
      StringAssert.Contains("DummyObjectWithEvent", res.Message);
      StringAssert.Contains("Click", res.Message);
    }

    private class DummyObjectWithList
    {
      public DummyObjectWithList()
      {
        _Items = new List<string>();
      }

      public List<string> Items { get { return _Items; } }
      private readonly List<string> _Items;
    }

    [Test]
    public void ObjectPropertyCount()
    {
      DummyObjectWithList obj = new DummyObjectWithList();
      obj.Items.Add("AAA");
      obj.Items.Add("BBB");

      InvalidOperationException res = ExceptionFactory.ObjectPropertyCount(obj, "Items", obj.Items, 3);
      StringAssert.Contains("DummyObjectWithList", res.Message);
      StringAssert.Contains("Items", res.Message);
      StringAssert.Contains("2", res.Message);
      StringAssert.Contains("3", res.Message);
    }

    [Test]
    public void TypeNotCloneable()
    {
      InvalidOperationException res = ExceptionFactory.TypeNotCloneable(typeof(DummyObjectWithCode));
      StringAssert.Contains("DummyObjectWithCode", res.Message);
    }

    #endregion

    #region Словари

    [Test]
    public void KeyNotFound_simple()
    {
      object key = "XYZ";
      KeyNotFoundException res = ExceptionFactory.KeyNotFound(key);
      StringAssert.Contains("XYZ", res.Message);
    }


    [Test]
    public void KeyNotFound_array()
    {
      object key = new object[] { "ABC", 456 };
      KeyNotFoundException res = ExceptionFactory.KeyNotFound(key);
      StringAssert.Contains("ABC", res.Message);
      StringAssert.Contains("456", res.Message);
    }

    [Test]
    public void KeyAlreadyExists_simple()
    {
      object key = "XYZ";
      Exception res = ExceptionFactory.KeyAlreadyExists(key);
      StringAssert.Contains("XYZ", res.Message);
    }

    [Test]
    public void KeyAlreadyExists_array()
    {
      object key = new object[] { "ABC", 456 };
      Exception res = ExceptionFactory.KeyAlreadyExists(key);
      StringAssert.Contains("ABC", res.Message);
      StringAssert.Contains("456", res.Message);
    }

    [Test]
    public void CannotAddItemAgain()
    {
      InvalidOperationException res = ExceptionFactory.CannotAddItemAgain(123);
      StringAssert.Contains("123", res.Message);
    }

    [Test]
    public void CannotAddItemAgain_null()
    {
      InvalidOperationException res = ExceptionFactory.CannotAddItemAgain(null);
      StringAssert.Contains("null", res.Message);
    }

    #endregion

    #region Проверка вызова метода

    private class TestBE
    {
      public void BeginSomething()
      {
      }
      public void EndSomething()
      {
      }
    }

    [Test]
    public void UnpairedCall()
    {
      TestBE obj = new TestBE();
      InvalidOperationException res = ExceptionFactory.UnpairedCall(obj, "BeginSomething()", "EndSomething()");
      StringAssert.Contains("BeginSomething()", res.Message);
      StringAssert.Contains("EndSomething()", res.Message);
    }

    [Test]
    public void RepeatedCall()
    {
      TestBE obj = new TestBE();
      InvalidOperationException res = ExceptionFactory.RepeatedCall(obj, "BeginSomething()");
      StringAssert.Contains("BeginSomething()", res.Message);
    }

    [Test]
    public void ConstructorAlreadyCalled()
    {
      InvalidOperationException res = ExceptionFactory.ConstructorAlreadyCalled(typeof(TestBE));
      StringAssert.Contains("TestBE", res.Message);
    }

    #endregion

    #region Путь к файлу

    [Test]
    public void FileNotFound()
    {
      FreeLibSet.IO.AbsPath path = new FreeLibSet.IO.AbsPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) + "aaa.txt";
      System.IO.FileNotFoundException res = ExceptionFactory.FileNotFound(path);
      StringAssert.Contains("aaa.txt", res.FileName);
    }

    [Test]
    public void DirectoryNotFound()
    {
      FreeLibSet.IO.AbsPath path = new FreeLibSet.IO.AbsPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) + "MySubDir";
      System.IO.DirectoryNotFoundException res = ExceptionFactory.DirectoryNotFound(path);
      StringAssert.Contains("MySubDir", res.Message);
    }

    #endregion

    #region DataTable

    private static DataTable CreateTestTable()
    {
      DataTable table = new DataTable();
      table.TableName = "MyTestTable";
      table.Columns.Add("F1", typeof(String));
      table.Columns.Add("F2", typeof(Int32));
      table.Columns.Add("F3", typeof(Boolean));
      return table;
    }

    [Test]
    public void ArgUnknownColumnName()
    {
      DataTable table = CreateTestTable();
      ArgumentException res = ExceptionFactory.ArgUnknownColumnName(ParamName1, table, "F4");
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(table.TableName, res.Message);
      StringAssert.Contains("F4", res.Message);
    }

    [Test]
    public void ArgInvalidColumnType()
    {
      DataTable table = CreateTestTable();
      ArgumentException res = ExceptionFactory.ArgInvalidColumnType(ParamName1, table.Columns["F2"]);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(table.TableName, res.Message);
      StringAssert.Contains("F2", res.Message);
      StringAssert.Contains("Int32", res.Message);
    }

    [Test]
    public void ArgDataTableWithoutPrimaryKey()
    {
      DataTable table = CreateTestTable();
      ArgumentException res = ExceptionFactory.ArgDataTableWithoutPrimaryKey(ParamName1, table);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(table.TableName, res.Message);
    }

    [Test]
    public void ArgDataTableMustHaveSingleColumnPrimaryKey_noPK()
    {
      DataTable table = CreateTestTable();
      DataTools.SetPrimaryKey(table, "");
      ArgumentException res = ExceptionFactory.ArgDataTableMustHaveSingleColumnPrimaryKey(ParamName1, table);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(table.TableName, res.Message);
    }

    [Test]
    public void ArgDataTableMustHaveSingleColumnPrimaryKey_complexPK()
    {
      DataTable table = CreateTestTable();
      DataTools.SetPrimaryKey(table, "F1,F2");
      ArgumentException res = ExceptionFactory.ArgDataTableMustHaveSingleColumnPrimaryKey(ParamName1, table);
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(table.TableName, res.Message);
    }

    [Test]
    public void ArgDataTablePrimaryKeyWrongType()
    {
      DataTable table = CreateTestTable();
      DataTools.SetPrimaryKey(table, "F1");
      ArgumentException res = ExceptionFactory.ArgDataTablePrimaryKeyWrongType(ParamName1, table, typeof(Int32));
      Assert.AreEqual(ParamName1, res.ParamName);
      StringAssert.Contains(table.TableName, res.Message);
      StringAssert.Contains("Int32", res.Message);
      StringAssert.Contains("F1", res.Message);
    }

    [Test]
    public void ArgDataTablesNotSameDataSet()
    {
      DataTable table1 = CreateTestTable();
      table1.TableName = "MyTable1";
      DataSet ds1 = new DataSet();
      ds1.Tables.Add(table1);

      DataTable table2 = CreateTestTable();
      table1.TableName = "MyTable2";
      DataSet ds2 = new DataSet();
      ds2.Tables.Add(table2);

      ArgumentException res = ExceptionFactory.ArgDataTablesNotSameDataSet(ParamName1, table1, ParamName2, table2);
      Assert.AreEqual(ParamName2, res.ParamName);
      //StringAssert.Contains(table1.TableName, res.Message);
      //StringAssert.Contains(table2.TableName, res.Message);
    }

    [Test]
    public void ArgDataRowNotInSameTable()
    {
      DataTable table1 = CreateTestTable();
      table1.TableName = "MyTable1";
      DataRow row1 = table1.Rows.Add("AAA", 123, true);

      DataTable table2 = CreateTestTable();

      ArgumentException res = ExceptionFactory.ArgDataRowNotInSameTable(ParamName1, row1, table2);
      Assert.AreEqual(ParamName1, res.ParamName);
    }

    [Test]
    public void DataRowNotFound()
    {
      DataTable table = CreateTestTable();
      DataTools.SetPrimaryKey(table, "F1");
      Exception res = ExceptionFactory.DataRowNotFound(table, new object[] { "AAA" });
      StringAssert.Contains(table.TableName, res.Message);
      StringAssert.Contains("F1", res.Message);
    }

    [Test]
    public void DataColumnNotFound()
    {
      DataTable table = CreateTestTable();
      Exception res = ExceptionFactory.DataColumnNotFound(table, "F666");
      StringAssert.Contains(table.TableName, res.Message);
      StringAssert.Contains("F666", res.Message);
    }

    #endregion

    #region Прочие исключения

    [Test]
    public void ObjectReadOnly()
    {
      ReadOnlyCollectionWrapper<int> obj = new ReadOnlyCollectionWrapper<int>(new List<int>());
      ObjectReadOnlyException res = ExceptionFactory.ObjectReadOnly(obj);
      StringAssert.Contains(obj.ToString(), res.Message);
    }

    [Test]
    public void MustBeReimplemented()
    {
      object obj = new DummyObjectWithCode();
      NotImplementedException res = ExceptionFactory.MustBeReimplemented(obj, "XXX");
      StringAssert.Contains("XXX", res.Message);
    }

    [Test]
    public void DllNoFound()
    {
      DllNotFoundException res = ExceptionFactory.DllNotFound("XXX");
      StringAssert.Contains("XXX", res.Message);
    }

    #endregion

    #region Вспомогательные методы

    [Test]
    public void MergeInnerException()
    {
      string msg = "AAA";
      Exception e = new Exception("BBB");
      string res = ExceptionFactory.MergeInnerException(msg, e);
      StringAssert.Contains("AAA", res);
      StringAssert.Contains("BBB", res);
    }

    #endregion
  }

  [TestFixture]
  [SetCulture("en-US")]
  [SetUICulture("en-US")]
  public class ExceptionFactoryTests_en_US : ExceptionFactoryTests
  {
    //public ExceptionFactoryTests_en_US()
    //  :base("en-US")
    //{
    //}

    [Test]
    public void InternalTest()
    {
      Assert.AreEqual("en-US", CultureInfo.CurrentCulture.Name, "CurrentCulture");
      Assert.AreEqual("en-US", CultureInfo.CurrentUICulture.Name, "CurrentUICulture");
    }
  }

  [TestFixture]
  [SetCulture("ru-RU")]
  [SetUICulture("ru-RU")]
  public class ExceptionFactoryTests_ru_RU : ExceptionFactoryTests
  {
    //public ExceptionFactoryTests_ru_RU()
    //  :base("ru-RU")
    //{
    //}

    [Test]
    public void InternalTest()
    {
      Assert.AreEqual("ru-RU", CultureInfo.CurrentCulture.Name, "CurrentCulture");
      Assert.AreEqual("ru-RU", CultureInfo.CurrentUICulture.Name, "CurrentUICulture");
    }
  }
}
