using FreeLibSet.Core;
using FreeLibSet.Data;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ExtTools_tests.Data
{
  [TestFixture]
  class IdToolsTests
  {
    #region Создание коллекций

    #region CreateIdCollection()

    [Test]
    public void CreateIdCollection_Int32()
    {
      IIdSet res = IdTools.CreateIdCollection(typeof(Int32));
      Assert.IsInstanceOf<IdCollection<Int32>>(res, "Type");
      IdCollection<Int32> res2 = (IdCollection<Int32>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void CreateIdCollection_StringId()
    {
      IIdSet res = IdTools.CreateIdCollection(typeof(StringId));
      Assert.IsInstanceOf<IdCollection<StringId>>(res, "Type");
      IdCollection<StringId> res2 = (IdCollection<StringId>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void CreateIdCollection_ComplexId2()
    {
      IIdSet res = IdTools.CreateIdCollection(typeof(ComplexId<Guid, Int64>));
      Assert.IsInstanceOf<IdCollection<ComplexId<Guid, Int64>>>(res, "Type");
      IdCollection<ComplexId<Guid, Int64>> res2 = (IdCollection<ComplexId<Guid, Int64>>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void CreateIdCollection_ComplexId3()
    {
      IIdSet res = IdTools.CreateIdCollection(typeof(ComplexId<Guid, Int64, StringId>));
      Assert.IsInstanceOf<IdCollection<ComplexId<Guid, Int64, StringId>>>(res, "Type");
      IdCollection<ComplexId<Guid, Int64, StringId>> res2 = (IdCollection<ComplexId<Guid, Int64, StringId>>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    #endregion

    #region CreateIdList()

    [Test]
    public void CreateIdList_Int32()
    {
      IIdSet res = IdTools.CreateIdList(typeof(Int32));
      Assert.IsInstanceOf<IdList<Int32>>(res, "Type");
      IdList<Int32> res2 = (IdList<Int32>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void CreateIdList_StringId()
    {
      IIdSet res = IdTools.CreateIdList(typeof(StringId));
      Assert.IsInstanceOf<IdList<StringId>>(res, "Type");
      IdList<StringId> res2 = (IdList<StringId>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void CreateIdList_ComplexId2()
    {
      IIdSet res = IdTools.CreateIdList(typeof(ComplexId<Guid, Int64>));
      Assert.IsInstanceOf<IdList<ComplexId<Guid, Int64>>>(res, "Type");
      IdList<ComplexId<Guid, Int64>> res2 = (IdList<ComplexId<Guid, Int64>>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void CreateIdList_ComplexId3()
    {
      IIdSet res = IdTools.CreateIdList(typeof(ComplexId<Guid, Int64, StringId>));
      Assert.IsInstanceOf<IdList<ComplexId<Guid, Int64, StringId>>>(res, "Type");
      IdList<ComplexId<Guid, Int64, StringId>> res2 = (IdList<ComplexId<Guid, Int64, StringId>>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, res2.Count, "Count");
    }

    #endregion

    #region GetEmptyArray()

    [Test]
    public void GetEmptyArray_Int32()
    {
      IIdSet res = IdTools.GetEmptyArray(typeof(Int32));
      Assert.IsInstanceOf<IdArray<Int32>>(res, "Type");
      IdArray<Int32> res2 = (IdArray<Int32>)res;
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void GetEmptyArray_StringId()
    {
      IIdSet res = IdTools.GetEmptyArray(typeof(StringId));
      Assert.IsInstanceOf<IdArray<StringId>>(res, "Type");
      IdArray<StringId> res2 = (IdArray<StringId>)res;
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void GetEmptyArray_ComplexId2()
    {
      IIdSet res = IdTools.GetEmptyArray(typeof(ComplexId<Guid, Int64>));
      Assert.IsInstanceOf<IdArray<ComplexId<Guid, Int64>>>(res, "Type");
      IdArray<ComplexId<Guid, Int64>> res2 = (IdArray<ComplexId<Guid, Int64>>)res;
      Assert.AreEqual(0, res2.Count, "Count");
    }

    [Test]
    public void GetEmptyArray_ComplexId3()
    {
      IIdSet res = IdTools.GetEmptyArray(typeof(ComplexId<Guid, Int64, StringId>));
      Assert.IsInstanceOf<IdArray<ComplexId<Guid, Int64, StringId>>>(res, "Type");
      IdArray<ComplexId<Guid, Int64, StringId>> res2 = (IdArray<ComplexId<Guid, Int64, StringId>>)res;
      Assert.AreEqual(0, res2.Count, "Count");
    }

    #endregion

    #endregion

    #region Преобразования IIdSet

    #region AsEnumerable()

    [Test]
    public void AsEnumerable_Int32_Int32()
    {
      IIdSet src = CreateInt32IdList();
      IEnumerable<Int32> en = IdTools.AsEnumerable<Int32>(src);
      ValidateEnumerable<Int32>(en, new Int32[] { 1, 3, 2 });
    }

    [Test]
    public void AsEnumerable_Int64_Int64()
    {
      IIdSet src = CreateInt64IdList();
      IEnumerable<Int64> en = IdTools.AsEnumerable<Int64>(src);
      ValidateEnumerable<Int64>(en, new Int64[] { 1L, 3L, 2L });
    }

    [Test]
    public void AsEnumerable_Int32_Int64()
    {
      IIdSet src = CreateInt32IdList();
      IEnumerable<Int64> en = IdTools.AsEnumerable<Int64>(src);
      ValidateEnumerable<Int64>(en, new Int64[] { 1L, 3L, 2L });
    }

    [Test]
    public void AsEnumerable_Int64_Int32()
    {
      IIdSet src = CreateInt64IdList();
      IEnumerable<Int32> en = IdTools.AsEnumerable<Int32>(src);
      ValidateEnumerable<Int32>(en, new Int32[] { 1, 3, 2 });
    }

    [Test]
    public void AsEnumerable_Int64_Int32_overflow()
    {
      IdList<Int64> src = new IdList<Int64>();
      src.Add((long)(Int32.MaxValue) + 1);
      IEnumerable<Int32> en = IdTools.AsEnumerable<Int32>(src);
      Assert.Catch<OverflowException>(delegate () { PerformForEach(en); });
    }

    [Test]
    public void AsEnumerable_Int32_Guid()
    {
      IIdSet src = CreateInt32IdList();
      IEnumerable<Guid> en;
      Assert.Catch<InvalidCastException>(delegate () { en = IdTools.AsEnumerable<Guid>(src); });
    }


    private static IdList<Int32> CreateInt32IdList()
    {
      IdList<Int32> lst = new IdList<Int32>();
      lst.Add(1);
      lst.Add(3);
      lst.Add(2);
      return lst;
    }

    private static IdList<Int64> CreateInt64IdList()
    {
      IdList<Int64> lst = new IdList<Int64>();
      lst.Add(1L);
      lst.Add(3L);
      lst.Add(2L);
      return lst;
    }

    private static void ValidateEnumerable<T>(IEnumerable<T> en, T[] wantedRes)
    {
      List<T> res = new List<T>();
      foreach (T item in en)
        res.Add(item);
      CollectionAssert.AreEqual(wantedRes, res);
    }

    private static void PerformForEach(System.Collections.IEnumerable en)
    {
      foreach (object item in en)
      {
      }
    }

    #endregion

    #region AsIdSet()

    [Test]
    public void AsIdSet_IIdSet_sameType_IdList()
    {
      IdList<ComplexId<Int32, Int64>> arg = new IdList<ComplexId<int, long>>();
      arg.Add(new ComplexId<int, long>(1, 2));

      IIdSet<ComplexId<Int32, Int64>> res = IdTools.AsIdSet<ComplexId<Int32, Int64>>((IIdSet)arg);

      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdSet_IIdSet_sameType_IdCollection()
    {
      IdCollection<StringId> arg = new IdCollection<StringId>();
      arg.Add("ABC");

      IIdSet<StringId> res = IdTools.AsIdSet<StringId>((IIdSet)arg);

      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdSet_IIdSet_sameType_IdArray()
    {
      IdArray<Int64> arg = new IdArray<Int64>(new long[] { 1L, 2L, 3L });

      IIdSet<Int64> res = IdTools.AsIdSet<Int64>((IIdSet)arg);

      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdSet_IIdSet_null()
    {
      IIdSet<ComplexId<Int32, Int64, Guid>> res = IdTools.AsIdSet<ComplexId<Int32, Int64, Guid>>((IIdSet)null);

      // Возвращает IdArray в текущей реализации, но это не является предопределенным

      Assert.IsNotNull(res, "Not null");
      Assert.AreEqual(0, res.Count, "Count");
    }

    [Test]
    public void AsIdSet_IIdSet_diffType_IdList()
    {
      IdList<Int32> arg = new IdList<int>();
      arg.Add(1);
      arg.Add(3);
      arg.Add(2);

      IIdSet<Int64> res = IdTools.AsIdSet<Int64>(arg);

      Assert.IsInstanceOf<IdList<Int64>>(res, "Type");
      IdList<Int64> res2 = (IdList<Int64>)res;
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new Int64[] { 1L, 3L, 2L }, res2.ToArray(), "ToArray()");
    }

    [Test]
    public void AsIdSet_IIdSet_diffType_IdCollection()
    {
      IdCollection<Int64> arg = new IdCollection<Int64>();
      arg.Add(1);
      arg.Add(3);
      arg.Add(2);

      IIdSet<Int32> res = IdTools.AsIdSet<Int32>(arg);

      // В текущей реализации возвращается IdCollection, но может возвращаться и IdList

      Assert.IsFalse(((IReadOnlyObject)res).IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 2, 3 }, res.ToArray(), "ToArray()");
    }

    [Test]
    public void AsIdSet_IIdSet_diffType_IdArray()
    {
      IdArray<Int32> arg = new IdArray<int>(new Int32[] { 1, 3, 2 });

      IIdSet<Int64> res = IdTools.AsIdSet<Int64>(arg);

      // В текущей реализации возвращается IdList, но может быть и IdArray
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res);

      CollectionAssert.AreEqual(new Int64[] { 1L, 3L, 2L }, res.ToArray(), "ToArray()");
    }

    [Test]
    public void AsIdSet_IEn_IdList()
    {
      IdList<Int32> arg = new IdList<int>(new Int32[] { 1, 3, 2 });

      IIdSet<Int32> res = IdTools.AsIdSet<Int32>((IEnumerable<Int32>)arg);
      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdSet_IEn_IdCollection()
    {
      IdCollection<ComplexId<Int64, StringId>> arg = new IdCollection<ComplexId<long, StringId>>();
      arg.Add(new ComplexId<Int64, StringId>(1, "AAA"));
      arg.Add(new ComplexId<Int64, StringId>(2, "BBB"));

      IIdSet<ComplexId<Int64, StringId>> res = IdTools.AsIdSet<ComplexId<Int64, StringId>>((IEnumerable<ComplexId<Int64, StringId>>)arg);
      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdSet_IEn_IdArray()
    {
      IdArray<Int32> arg = new IdArray<int>(new Int32[] { 1, 3, 2 });

      IIdSet<Int32> res = IdTools.AsIdSet<Int32>((IEnumerable<Int32>)arg);
      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdSet_IEn_null()
    {
      IIdSet<Int32> res = IdTools.AsIdSet<Int32>((IEnumerable<Int32>)null);
      Assert.IsNotNull(res, "not null");
      Assert.AreEqual(0, res.Count, "Count");
    }

    public void AsIdSet_IEn_Enumerable()
    {
      List<Int32> arg = new List<int>(new Int32[] { 1, 3, 2 });

      IIdSet<Int32> res = IdTools.AsIdSet<Int32>((IEnumerable<Int32>)arg);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res, "Type");

      CollectionAssert.AreEqual(arg.ToArray(), res.ToArray(), "ToArray()");
    }

    #endregion

    #region AsIdArray()

    [Test]
    public void AsIdArray_IdArray()
    {
      IdArray<Int32> arg = new IdArray<Int32>(new Int32[] { 1, 3, 2 });
      IdArray<Int32> res = IdTools.AsIdArray<Int32>(arg);
      Assert.AreSame(arg, res);
    }

    [Test]
    public void AsIdArray_IdList()
    {
      IdList<Int32> arg = new IdList<Int32>(new Int32[] { 1, 3, 2 });
      IdArray<Int32> res = IdTools.AsIdArray<Int32>(arg);
      CollectionAssert.AreEqual(new Int32[] { 1, 3, 2 }, res.ToArray(), "ToArray()");
    }

    [Test]
    public void AsIdArray_IdCollection()
    {
      IdCollection<Int32> arg = new IdCollection<Int32>(new Int32[] { 1, 3, 2 });
      IdArray<Int32> res = IdTools.AsIdArray<Int32>(arg);
      CollectionAssert.AreNotEqual(new Int32[] { 1, 2, 3 }, res.ToArray(), "ToArray()");
    }

    [Test]
    public void AsIdArray_null()
    {
      IdArray<Int32> res = IdTools.AsIdArray<Int32>(null);
      Assert.IsNotNull(res, "Not null");
      Assert.AreEqual(0, res.Count, "Count");
    }

    [Test]
    public void AsIdArray_Enumerable()
    {
      List<Int32> arg = new List<Int32>(new Int32[] { 1, 3, 2 });
      IdArray<Int32> res = IdTools.AsIdArray<Int32>(arg);
      CollectionAssert.AreEqual(new Int32[] { 1, 3, 2 }, res.ToArray(), "ToArray()");
    }

    #endregion

    #endregion

    #region Сравнение наборов

    #region AreEqual<IIdSet>

    [TestCase("1,2,3", "3,2,1", true)]
    [TestCase("1,2,3", "4,2,1", false)]
    [TestCase("1,2,3", "", false)]
    [TestCase("1,2,3", null, false)]
    [TestCase("", "", true)]
    [TestCase("", null, true)]
    [TestCase(null, null, true)]
    public void AreEqual_IIdSet_Int32(string sA, string sB, bool wantedRes)
    {
      IdSetKind[] allKinds = new IdSetKind[] { IdSetKind.List, IdSetKind.Collection, IdSetKind.Array };

      foreach (IdSetKind kindA in allKinds)
      {
        foreach (IdSetKind kindB in allKinds)
        {
          IIdSet<Int32> a = CreateInt32IdSet(sA, kindA);
          IIdSet<Int32> b = CreateInt32IdSet(sB, kindB);

          bool res1 = IdTools.AreEqual<Int32>(a, b);
          Assert.AreEqual(wantedRes, res1);

          bool res2 = IdTools.AreEqual<Int32>(b, a);
          Assert.AreEqual(wantedRes, res2);
        }
      }
    }

    private IIdSet<Int32> CreateInt32IdSet(string s, IdSetKind kind)
    {
      if (s == null)
        return null;

      Int32[] a = StdConvert.ToInt32Array(s);
      return IdTools.CreateIdSet<Int32>(a, kind);
    }

    [TestCase("ABC,DEF", "DEF,ABC", true)]
    [TestCase("ABC,DEF", "DEF,abc", false)]
    [TestCase("ABC,DEF", "DEF", false)]
    [TestCase("ABC,DEF", "", false)]
    [TestCase("ABC,DEF", null, false)]
    [TestCase("", "", true)]
    [TestCase("", null, true)]
    [TestCase(null, null, true)]
    public void AreEqual_IIdSet_String(string sA, string sB, bool wantedRes)
    {
      IdSetKind[] allKinds = new IdSetKind[] { IdSetKind.List, IdSetKind.Collection, IdSetKind.Array };

      foreach (IdSetKind kindA in allKinds)
      {
        foreach (IdSetKind kindB in allKinds)
        {
          IIdSet<StringId> a = CreateStringIdSet(sA, kindA);
          IIdSet<StringId> b = CreateStringIdSet(sB, kindB);

          bool res1 = IdTools.AreEqual<StringId>(a, b);
          Assert.AreEqual(wantedRes, res1);

          bool res2 = IdTools.AreEqual<StringId>(b, a);
          Assert.AreEqual(wantedRes, res2);
        }
      }
    }

    private IIdSet<StringId> CreateStringIdSet(string s, IdSetKind kind)
    {
      if (s == null)
        return null;

      StringId[] a;
      if (s.Length == 0)
        a = EmptyArray<StringId>.Empty;
      else
      {
        string[] a2 = s.Split(',');
        a = new StringId[a2.Length];
        for (int i = 0; i < a.Length; i++)
          a[i] = new StringId(a2[i]);
      }
      return IdTools.CreateIdSet<StringId>(a, kind);
    }

    #endregion

    #region AreEqual<ITableIdSet>

    [TestCase("AAA:1,2,3|BBB:1,2,3", "AAA:3,2,1|BBB:2,1,3", true)]
    [TestCase("AAA:1,2,3|BBB:1,2,3", "AAA:3,2,1|bbb:2,1,3", false)]
    [TestCase("!AAA:1,2,3|BBB:1,2,3", "!AAA:3,2,1|bbb:2,1,3", true)]
    [TestCase("!AAA:1,2,3|BBB:1,2,3", "AAA:3,2,1|bbb:2,1,3", false)]
    [TestCase("AAA:1,2,3|BBB:1,2,3", "AAA:1,2,3", false)]
    [TestCase("AAA:1,2,3|BBB:1,2,3", "", false)]
    [TestCase("AAA:1,2,3|BBB:1,2,3", null, false)]
    [TestCase("!", "", true)]
    [TestCase("", null, true)]
    [TestCase(null, null, true)]
    public void AreEqual_ITableIdSet_Int32(string sA, string sB, bool wantedRes)
    {
      ITableIdSet<Int32> a = CreateInt32TableIdSet(sA);
      ITableIdSet<Int32> b = CreateInt32TableIdSet(sB);

      bool res1 = IdTools.AreEqual<Int32>(a, b);
      Assert.AreEqual(wantedRes, res1);

      bool res2 = IdTools.AreEqual<Int32>(b, a);
      Assert.AreEqual(wantedRes, res2);
    }

    private ITableIdSet<Int32> CreateInt32TableIdSet(string s)
    {
      if (s == null)
        return null;

      bool ignoreCase = false;
      if (s.StartsWith("!"))
      {
        ignoreCase = true;
        s = s.Substring(1);
      }

      TableIdCollection<Int32> coll = new TableIdCollection<Int32>(ignoreCase);
      if (s.Length == 0)
        return coll;

      string[] a1 = s.Split('|');
      for (int i = 0; i < a1.Length; i++)
      {
        int p = a1[i].IndexOf(':');
        string tableName = a1[i].Substring(0, p);
        string sIds = a1[i].Substring(p + 1);
        IIdSet<Int32> ids = CreateInt32IdSet(sIds, IdSetKind.Collection);
        coll[tableName].AddRange(ids);
      }
      return coll;
    }

    #endregion

    #endregion

    #region Определение типа ключевого поля

    #region IsValidDataType()

    [TestCase(typeof(Int32), true)]
    [TestCase(typeof(Int64), true)]
    [TestCase(typeof(Guid), true)]
    [TestCase(typeof(String), true)]
    [TestCase(typeof(DateTime), false)]
    [TestCase(typeof(StringId), false)]
    [TestCase(typeof(ComplexId<Int32, Int64>), false)]
    [TestCase(null, false)]
    public void IsValidDataType(Type arg, bool wantedRes)
    {
      bool res = IdTools.IsValidDataType(arg);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region IsValidIdType()

    [TestCase(typeof(Int32), true)]
    [TestCase(typeof(Int64), true)]
    [TestCase(typeof(Guid), true)]
    [TestCase(typeof(String), false)]
    [TestCase(typeof(DateTime), false)]
    [TestCase(typeof(StringId), true)]
    [TestCase(typeof(ComplexId<Int32, Int64>), true)]
    [TestCase(typeof(ComplexId<Int32, Int64, StringId>), true)]
    [TestCase(typeof(ComplexId<Int32, Int64, DateTime>), false)]
    [TestCase(typeof(ComplexId<Int32, ComplexId<Int64, Int64>>), false)]
    [TestCase(null, false)]
    public void IsValidIdType(Type arg, bool wantedRes)
    {
      bool res = IdTools.IsValidIdType(arg);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region GetIdType()

    [TestCase(typeof(Int32), typeof(Int32))]
    [TestCase(typeof(Int64), typeof(Int64))]
    [TestCase(typeof(Guid), typeof(Guid))]
    [TestCase(typeof(String), typeof(StringId))]
    public void GetIdType_DataType_1(Type dataType, Type wantedIdType)
    {
      Type idType1 = IdTools.GetIdType(dataType);
      Assert.AreEqual(wantedIdType, idType1);

      Type idType2 = IdTools.GetIdType(new Type[1] { dataType });
      Assert.AreEqual(wantedIdType, idType2);
    }

    [TestCase(typeof(Boolean))]
    [TestCase(typeof(Single))]
    [TestCase(typeof(Double))]
    [TestCase(typeof(Decimal))]
    [TestCase(typeof(DateTime))]
    [TestCase(typeof(TimeSpan))]
    public void GetIdType_DataType_1_Unsupported(Type dataType)
    {
      Assert.Catch<ArgumentException>(delegate () { IdTools.GetIdType(dataType); });
    }

    [TestCase(typeof(String), typeof(Int32), typeof(ComplexId<StringId, Int32>))]
    [TestCase(typeof(Int64), typeof(Guid), typeof(ComplexId<Int64, Guid>))]
    public void GetIdType_DataType_2(Type dataType1, Type dataType2, Type wantedIdType)
    {
      Type idType = IdTools.GetIdType(new Type[2] { dataType1, dataType2 });
      Assert.AreEqual(wantedIdType, idType);
    }

    [TestCase(typeof(Int64), typeof(Boolean))]
    [TestCase(typeof(Boolean), typeof(Int64))]
    [TestCase(typeof(Boolean), typeof(ComplexId<StringId, Int32>))]
    [TestCase(typeof(Boolean), typeof(ComplexId<Int32, Int32, Int64>))]
    public void GetIdType_DataType_2_Unsupported(Type dataType1, Type dataType2)
    {
      Assert.Catch<ArgumentException>(delegate () { IdTools.GetIdType(new Type[2] { dataType1, dataType2 }); });
    }

    [TestCase("FString", typeof(StringId))]
    [TestCase("FInt32", typeof(Int32))]
    [TestCase("FInt32NN", typeof(Int32))]
    [TestCase("FInt64", typeof(Int64))]
    [TestCase("FGuid", typeof(Guid))]
    [TestCase("FInt32,FString", typeof(ComplexId<Int32, StringId>))]
    [TestCase("FString,FInt64,FGuid", typeof(ComplexId<StringId, Int64, Guid>))]
    public void GetIdType_DataTable_DBxColumns(string columnNames, Type wantedIdType)
    {
      DataTable table = TestTable.Create();

      DBxColumns columns = new DBxColumns(columnNames);
      Type idType = IdTools.GetIdType(table, columns);
      Assert.AreEqual(wantedIdType, idType);
    }

    [Test]
    public void GetIdType_DataTable_DBxColumns_Empty()
    {
      DataTable table = TestTable.Create();
      DBxColumns columns = DBxColumns.Empty;
      Assert.Catch<ArgumentException>(delegate () { IdTools.GetIdType(table, columns); });
    }

    [Test]
    public void GetIdType_DataTable_DBxColumns_UnknownColumn()
    {
      DataTable table = TestTable.Create();
      DBxColumns columns = new DBxColumns("XXX");
      Assert.Catch<ArgumentException>(delegate () { IdTools.GetIdType(table, columns); });
    }

    [TestCase("FString", typeof(StringId))]
    [TestCase("FInt32", typeof(Int32))]
    [TestCase("FInt32NN", typeof(Int32))]
    [TestCase("FInt64", typeof(Int64))]
    [TestCase("FGuid", typeof(Guid))]
    public void GetIdType_DbDataReader(string columnName, Type wantedIdType)
    {
      DataTable table = TestTable.Create();
      DbDataReader rdr = table.CreateDataReader();

      Type idType = IdTools.GetIdType(rdr, columnName);

      Assert.AreEqual(wantedIdType, idType);
    }

    #endregion

    #region GetDataTypes()

    [TestCase(typeof(Int32), typeof(Int32))]
    [TestCase(typeof(Int64), typeof(Int64))]
    [TestCase(typeof(Guid), typeof(Guid))]
    [TestCase(typeof(StringId), typeof(string))]
    public void GetDataTypes_1(Type idType, Type wantedRes)
    {
      Type[] res = IdTools.GetDataTypes(idType);

      CollectionAssert.AreEqual(new Type[1] { wantedRes }, res);
    }

    [TestCase(typeof(ComplexId<Int32, StringId>), typeof(Int32), typeof(string))]
    public void GetDataTypes_2(Type idType, Type wantedRes1, Type wantedRes2)
    {
      Type[] res = IdTools.GetDataTypes(idType);

      CollectionAssert.AreEqual(new Type[2] { wantedRes1, wantedRes2 }, res);
    }

    [TestCase(typeof(ComplexId<Int32, StringId, Guid>), typeof(Int32), typeof(string), typeof(Guid))]
    public void GetDataTypes_3(Type idType, Type wantedRes1, Type wantedRes2, Type wantedRes3)
    {
      Type[] res = IdTools.GetDataTypes(idType);

      CollectionAssert.AreEqual(new Type[3] { wantedRes1, wantedRes2, wantedRes3 }, res);
    }

    [TestCase(typeof(double))]
    [TestCase(typeof(string))]
    [TestCase(typeof(ComplexId<Int32, double>))]
    [TestCase(null)]
    public void GetDataTypes_exception(Type idType)
    {
      Type[] res;
      Assert.Catch<ArgumentException>(delegate () { res = IdTools.GetDataTypes(idType); });
    }

    #endregion

    #endregion

    #region Получение IIdExtractor

    [Test]
    public void CreateExtractor_columnName()
    {
      DataTable table = TestTable.Create();
      DataTableValues source = new DataTableValues(table);

      IdExtractor<Int32> res1 = IdTools.CreateExtractor<Int32>(source, "FInt32");
      IIdExtractor res2 = IdTools.CreateExtractor(source, "FInt32", typeof(Int32));

      IdList<Int32> lst1 = new IdList<int>();
      IdList<Int32> lst2 = new IdList<int>();

      while (source.Read())
      {
        Int32 wantedId = DataTools.GetInt32(source.CurrentRow, "FInt32");

        Assert.AreEqual(wantedId, res1.Id, "Id");

        res1.AddToSet(lst1);
        res2.AddToSet(lst2);
      }

      Int32[] wantedIds = new Int32[] { TestTable.Row1.VInt32, TestTable.Row2.VInt32 };
      CollectionAssert.AreEqual(wantedIds, lst1.ToArray(), "#1");
      CollectionAssert.AreEqual(wantedIds, lst2.ToArray(), "#2");
    }

    [Test]
    public void CreateExtractor_DBxColumns()
    {
      DataTable table = TestTable.Create();
      DataTableValues source = new DataTableValues(table);

      DBxColumns columnNames = new DBxColumns("FGuid,FString");
      IdExtractor<ComplexId<Guid, StringId>> res1 = IdTools.CreateExtractor<ComplexId<Guid, StringId>>(source, columnNames);
      IIdExtractor res2 = IdTools.CreateExtractor(source, columnNames, typeof(ComplexId<Guid, StringId>));

      IdList<ComplexId<Guid, StringId>> lst1 = new IdList<ComplexId<Guid, StringId>>();
      IdList<ComplexId<Guid, StringId>> lst2 = new IdList<ComplexId<Guid, StringId>>();

      while (source.Read())
      {
        ComplexId<Guid, StringId> wantedId = new ComplexId<Guid, StringId>(DataTools.GetGuid(source.CurrentRow, "FGuid"),
          DataTools.GetString(source.CurrentRow, "FString"));

        Assert.AreEqual(wantedId, res1.Id, "Id");

        res1.AddToSet(lst1);
        res2.AddToSet(lst2);
      }

      ComplexId<Guid, StringId>[] wantedIds = new ComplexId<Guid, StringId>[] {
        new ComplexId<Guid, StringId>(TestTable.Row1.VGuid, TestTable.Row1.VString),
        new ComplexId<Guid, StringId>(TestTable.Row2.VGuid, TestTable.Row2.VString) };

      CollectionAssert.AreEqual(wantedIds, lst1.ToArray(), "#1");
      CollectionAssert.AreEqual(wantedIds, lst2.ToArray(), "#2");
    }

    #endregion

    #region Извлечение значений из DbDataReader

    public struct ReadTestData
    {
      public ReadTestData(string columnNames, object[] wantedValues)
      {
        _ColumnNames = columnNames;
        _WantedValues = wantedValues;
      }

      public string ColumnNames { get { return _ColumnNames; } }
      private readonly string _ColumnNames;

      public object[] WantedValues { get { return _WantedValues; } }
      private readonly object[] _WantedValues;

      public override string ToString()
      {
        return _ColumnNames;
      }
    }

    public ReadTestData[] GetIdsFromColumns_DbDataReader_NoType_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FInt32", new object[]{TestTable.Row1.VInt32, TestTable.Row2.VInt32}),
          new ReadTestData("FInt32NN", new object[]{TestTable.Row1.VInt32, TestTable.Row2.VInt32}),
          new ReadTestData("FInt64", new object[]{TestTable.Row1.VInt64, TestTable.Row2.VInt64}),
          new ReadTestData("FGuid", new object[]{TestTable.Row1.VGuid, TestTable.Row2.VGuid}),
          new ReadTestData("FString", new object[]{new StringId(TestTable.Row1.VString), new StringId(TestTable.Row2.VString)}),

          new ReadTestData("FInt32,FInt64", new object[]{
            new ComplexId<Int32, Int64>(TestTable.Row1.VInt32, TestTable.Row1.VInt64),
            new ComplexId<Int32, Int64>(TestTable.Row2.VInt32, TestTable.Row2.VInt64)}),

          new ReadTestData("FGuid,FString", new object[]{
            new ComplexId<Guid, StringId>(TestTable.Row1.VGuid, TestTable.Row1.VString),
            new ComplexId<Guid, StringId>(TestTable.Row2.VGuid, TestTable.Row2.VString)}),

            new ReadTestData("FInt32,FInt64,FGuid", new object[]{
            new ComplexId<Int32, Int64, Guid>(TestTable.Row1.VInt32, TestTable.Row1.VInt64, TestTable.Row1.VGuid),
            new ComplexId<Int32, Int64, Guid>(TestTable.Row2.VInt32, TestTable.Row2.VInt64, TestTable.Row2.VGuid)}),

        };
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_NoType_Tests")]
    public void GetIdsFromColumns_DbDataReader_NoType(ReadTestData data)
    {
      DataTable table = TestTable.Create();
      using (DbDataReader rdr = table.CreateDataReader())
      {
        IIdSet res = IdTools.GetIdsFromColumns(rdr, new DBxColumns(data.ColumnNames), true);
        Assert.IsInstanceOf<ICollection>(res);

        ICollection coll = res as ICollection;
        List<object> lstIds = new List<object>();
        foreach (object item in coll)
          lstIds.Add(item);
        CollectionAssert.AreEqual(data.WantedValues, lstIds);
      }
    }

    [Test]
    public void GetIdsFromColumns_DbDataReader_NoType_TooComplexKey()
    {
      DataTable table = TestTable.Create();
      using (DbDataReader rdr = table.CreateDataReader())
      {
        Assert.DoesNotThrow(delegate () { IdTools.GetIdsFromColumns(rdr, new DBxColumns("FInt32,FInt64,FGuid"), true); });
        Assert.Catch<ArgumentException>(delegate () { IdTools.GetIdsFromColumns(rdr, new DBxColumns("FInt32,FInt64,FGuid,FString"), true); });
      }
    }


    public ReadTestData[] GetIdsFromColumns_DbDataReader_Int32_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FInt32", new object[]{TestTable.Row1.VInt32, TestTable.Row2.VInt32}),
          new ReadTestData("FInt32NN", new object[]{TestTable.Row1.VInt32, TestTable.Row2.VInt32}),
          new ReadTestData("FInt64", new object[]{(int)TestTable.Row1.VInt64, (int)TestTable.Row2.VInt64}),
        };
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_Int32_Tests")]
    public void GetIdsFromColumns_DbDataReader_Int32(ReadTestData data)
    {
      GetIdsFromColumns_DbDataReader<Int32>(data);
    }

    public ReadTestData[] GetIdsFromColumns_DbDataReader_Int64_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FInt32", new object[]{(long)TestTable.Row1.VInt32, (long)TestTable.Row2.VInt32}),
          new ReadTestData("FInt32NN", new object[]{(long)TestTable.Row1.VInt32, (long)TestTable.Row2.VInt32}),
          new ReadTestData("FInt64", new object[]{TestTable.Row1.VInt64, TestTable.Row2.VInt64}),
        };
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_Int64_Tests")]
    public void GetIdsFromColumns_DbDataReader_Int64(ReadTestData data)
    {
      GetIdsFromColumns_DbDataReader<Int64>(data);
    }

    public ReadTestData[] GetIdsFromColumns_DbDataReader_Guid_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FGuid", new object[]{TestTable.Row1.VGuid, TestTable.Row2.VGuid}),
        };
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_Guid_Tests")]
    public void GetIdsFromColumns_DbDataReader_Guid(ReadTestData data)
    {
      GetIdsFromColumns_DbDataReader<Guid>(data);
    }

    public ReadTestData[] GetIdsFromColumns_DbDataReader_String_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FInt32", new object[]{new StringId(TestTable.Row1.StrInt32), new StringId(TestTable.Row2.StrInt32)}),
          new ReadTestData("FInt32NN", new object[]{new StringId("0"), new StringId(TestTable.Row1.StrInt32), new StringId(TestTable.Row2.StrInt32)}),
          new ReadTestData("FInt64", new object[]{new StringId(TestTable.Row1.StrInt64), new StringId(TestTable.Row2.StrInt64)}),
          new ReadTestData("FGuid", new object[]{new StringId(TestTable.Row1.StrGuid), new StringId(TestTable.Row2.StrGuid)}),
          new ReadTestData("FString", new object[]{new StringId(TestTable.Row1.VString), new StringId(TestTable.Row2.VString)}),
        };
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_String_Tests")]
    public void GetIdsFromColumns_DbDataReader_String(ReadTestData data)
    {
      GetIdsFromColumns_DbDataReader<StringId>(data);
    }

    public ReadTestData[] GetIdsFromColumns_DbDataReader_Complex2_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FInt32NN,FInt64",
            new object[]{
              new ComplexId<Int32, Int64>(TestTable.Row1.VInt32, TestTable.Row1.VInt64),
              new ComplexId<Int32, Int64>(TestTable.Row2.VInt32, TestTable.Row2.VInt64),
            })};
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_Complex2_Tests")]
    public void GetIdsFromColumns_DbDataReader_Complex2(ReadTestData data)
    {
      GetIdsFromColumns_DbDataReader<ComplexId<Int32, Int64>>(data);
    }

    public ReadTestData[] GetIdsFromColumns_DbDataReader_Complex3_Tests
    {
      get
      {
        return new ReadTestData[] {
          new ReadTestData("FGuid,FInt32NN,FString",
            new object[]{
              new ComplexId<Guid, Int32, StringId>(TestTable.Row1.VGuid, TestTable.Row1.VInt32, new StringId(TestTable.Row1.VString)),
              new ComplexId<Guid, Int32, StringId>(TestTable.Row2.VGuid, TestTable.Row2.VInt32, new StringId(TestTable.Row2.VString)),
            })};
      }
    }
    [TestCaseSource("GetIdsFromColumns_DbDataReader_Complex3_Tests")]
    public void GetIdsFromColumns_DbDataReader_Complex3(ReadTestData data)
    {
      GetIdsFromColumns_DbDataReader<ComplexId<Guid, Int32, StringId>>(data);
    }

    private static void GetIdsFromColumns_DbDataReader<T>(ReadTestData data)
      where T : struct, IEquatable<T>
    {
      DataTable table = TestTable.Create();
      using (DbDataReader rdr = table.CreateDataReader())
      {
        IIdSet<T> res = IdTools.GetIdsFromColumns<T>(rdr, new DBxColumns(data.ColumnNames), true);

        List<T> lstIds = new List<T>();
        foreach (T item in res)
          lstIds.Add(item);
        CollectionAssert.AreEqual(data.WantedValues, lstIds);
      }
    }

    #endregion

    #region Извлечение значений из DataTable

    [Test]
    public void GetIds_DataTable()
    {
      DataTable table = TestTable.Create();
      table.Rows[0]["FInt32"] = 666;
      DataTools.SetPrimaryKey(table, "FInt32");

      IIdSet<Int32> res1 = IdTools.GetIds<Int32>(table);
      IIdSet<Int32> res2 = IdTools.GetIds<Int32>(table, false);
      IIdSet<Int32> res3 = IdTools.GetIds<Int32>(table, true);
      IIdSet res4 = IdTools.GetIds(table);
      IIdSet res5 = IdTools.GetIds(table, false);
      IIdSet res6 = IdTools.GetIds(table, true);

      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res1);
      //Assert.IsInstanceOf<IIdSet<Int32>>(res2);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res3);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res4);
      Assert.IsInstanceOf<IIdSet<Int32>>(res5);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res6);

      Int32[] wantedIds = new Int32[] { 666, TestTable.Row1.VInt32, TestTable.Row2.VInt32 };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEquivalent(wantedIds, res2);
      CollectionAssert.AreEqual(wantedIds, res3);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res4);
      CollectionAssert.AreEquivalent(wantedIds, (IEnumerable)res5);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res6);
    }


    [Test]
    public void GetIds_DataTable_empty()
    {
      DataTable table = TestTable.Create().Clone();
      DataTools.SetPrimaryKey(table, "FInt32");

      IIdSet<Int32> res1 = IdTools.GetIds<Int32>(table);
      IIdSet<Int32> res2 = IdTools.GetIds<Int32>(table, false);
      IIdSet<Int32> res3 = IdTools.GetIds<Int32>(table, true);
      IIdSet res4 = IdTools.GetIds(table);
      IIdSet res5 = IdTools.GetIds(table, false);
      IIdSet res6 = IdTools.GetIds(table, true);

      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res1);
      //Assert.IsInstanceOf<IIdSet<Int32>>(res2);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res3);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res4);
      Assert.IsInstanceOf<IIdSet<Int32>>(res5);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res6);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, res2.Count);
      Assert.AreEqual(0, res3.Count);
      Assert.AreEqual(0, ((ICollection)res4).Count);
      Assert.AreEqual(0, ((ICollection)res5).Count);
      Assert.AreEqual(0, ((ICollection)res6).Count);
    }

    [Test]
    public void GetIdsFromColumn_DataTable()
    {
      DataTable table = TestTable.Create();

      IIdSet<Int64> res1 = IdTools.GetIdsFromColumn<Int64>(table, "FInt64");
      IIdSet<Int64> res2 = IdTools.GetIdsFromColumn<Int64>(table, "FInt64", false);
      IIdSet<Int64> res3 = IdTools.GetIdsFromColumn<Int64>(table, "FInt64", true);
      IIdSet res4 = IdTools.GetIdsFromColumn(table, "FInt64");
      IIdSet res5 = IdTools.GetIdsFromColumn(table, "FInt64", false);
      IIdSet res6 = IdTools.GetIdsFromColumn(table, "FInt64", true);

      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res1);
      //Assert.IsInstanceOf<IIdSet<Int64>>(res2);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res3);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res4);
      Assert.IsInstanceOf<IIdSet<Int64>>(res5);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res6);

      Int64[] wantedIds = new Int64[] { TestTable.Row1.VInt64, TestTable.Row2.VInt64 };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEquivalent(wantedIds, res2);
      CollectionAssert.AreEqual(wantedIds, res3);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res4);
      CollectionAssert.AreEquivalent(wantedIds, (IEnumerable)res5);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res6);
    }

    [Test]
    public void GetIdsFromColumn_DataTable_empty()
    {
      DataTable table = TestTable.Create().Clone();

      IIdSet<Int64> res1 = IdTools.GetIdsFromColumn<Int64>(table, "FInt64");
      IIdSet<Int64> res2 = IdTools.GetIdsFromColumn<Int64>(table, "FInt64", false);
      IIdSet<Int64> res3 = IdTools.GetIdsFromColumn<Int64>(table, "FInt64", true);
      IIdSet res4 = IdTools.GetIdsFromColumn(table, "FInt64");
      IIdSet res5 = IdTools.GetIdsFromColumn(table, "FInt64", false);
      IIdSet res6 = IdTools.GetIdsFromColumn(table, "FInt64", true);

      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res1);
      //Assert.IsInstanceOf<IIdSet<Int64>>(res2);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res3);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res4);
      Assert.IsInstanceOf<IIdSet<Int64>>(res5);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res6);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, res2.Count);
      Assert.AreEqual(0, res3.Count);
      Assert.AreEqual(0, ((ICollection)res4).Count);
      Assert.AreEqual(0, ((ICollection)res5).Count);
      Assert.AreEqual(0, ((ICollection)res6).Count);
    }

    [Test]
    public void GetIdsFromColumns_DataTable()
    {
      DataTable table = TestTable.Create();

      IIdSet<ComplexId<Guid, StringId>> res1 = IdTools.GetIdsFromColumns<ComplexId<Guid, StringId>>(table, new DBxColumns("FGuid,FString"));
      IIdSet<ComplexId<Guid, StringId>> res2 = IdTools.GetIdsFromColumns<ComplexId<Guid, StringId>>(table, new DBxColumns("FGuid,FString"), false);
      IIdSet<ComplexId<Guid, StringId>> res3 = IdTools.GetIdsFromColumns<ComplexId<Guid, StringId>>(table, new DBxColumns("FGuid,FString"), true);
      IIdSet res4 = IdTools.GetIdsFromColumns(table, new DBxColumns("FGuid,FString"));
      IIdSet res5 = IdTools.GetIdsFromColumns(table, new DBxColumns("FGuid,FString"), false);
      IIdSet res6 = IdTools.GetIdsFromColumns(table, new DBxColumns("FGuid,FString"), true);

      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res1);
      //Assert.IsInstanceOf<IIdSet<ComplexId<Guid, StringId>>>(res2);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res3);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res4);
      Assert.IsInstanceOf<IIdSet<ComplexId<Guid, StringId>>>(res5);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res6);
       
      ComplexId<Guid, StringId>[] wantedIds = new ComplexId<Guid, StringId>[]
      {
        new ComplexId<Guid, StringId>(TestTable.Row1.VGuid, TestTable.Row1.VString),
        new ComplexId<Guid, StringId>(TestTable.Row2.VGuid, TestTable.Row2.VString)};

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEquivalent(wantedIds, res2);
      CollectionAssert.AreEqual(wantedIds, res3);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res4);
      CollectionAssert.AreEquivalent(wantedIds, (IEnumerable)res5);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res6);
    }


    [Test]
    public void GetIdsFromColumns_DataTable_empty()
    {
      DataTable table = TestTable.Create().Clone();

      IIdSet<ComplexId<Guid, StringId>> res1 = IdTools.GetIdsFromColumns<ComplexId<Guid, StringId>>(table, new DBxColumns("FGuid,FString"));
      IIdSet<ComplexId<Guid, StringId>> res2 = IdTools.GetIdsFromColumns<ComplexId<Guid, StringId>>(table, new DBxColumns("FGuid,FString"), false);
      IIdSet<ComplexId<Guid, StringId>> res3 = IdTools.GetIdsFromColumns<ComplexId<Guid, StringId>>(table, new DBxColumns("FGuid,FString"), true);
      IIdSet res4 = IdTools.GetIdsFromColumns(table, new DBxColumns("FGuid,FString"));
      IIdSet res5 = IdTools.GetIdsFromColumns(table, new DBxColumns("FGuid,FString"), false);
      IIdSet res6 = IdTools.GetIdsFromColumns(table, new DBxColumns("FGuid,FString"), true);

      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res1);
      //Assert.IsInstanceOf<IIdSet<ComplexId<Guid, StringId>>>(res2);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res3);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res4);
      Assert.IsInstanceOf<IIdSet<ComplexId<Guid, StringId>>>(res5);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Guid, StringId>>>(res6);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, res2.Count);
      Assert.AreEqual(0, res3.Count);
      Assert.AreEqual(0, ((ICollection)res4).Count);
      Assert.AreEqual(0, ((ICollection)res5).Count);
      Assert.AreEqual(0, ((ICollection)res6).Count);
    }

    #endregion

    #region Извлечение значений из DataView

    [Test]
    public void GetIds_DataView()
    {
      DataTable table = TestTable.Create();
      table.Rows[0]["FInt32"] = 666;
      DataTools.SetPrimaryKey(table, "FInt32");
      table.DefaultView.Sort = "FInt64 DESC";

      IIndexedIdSet<Int32> res1 = IdTools.GetIds<Int32>(table.DefaultView);
      IIdSet res2 = IdTools.GetIds(table.DefaultView);

      //Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res2);

      Int32[] wantedIds = new Int32[] { TestTable.Row2.VInt32, TestTable.Row1.VInt32, 666 };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res2);
    }

    [Test]
    public void GetIds_DataView_empty()
    {
      DataTable table = TestTable.Create();
      table.Rows[0]["FInt32"] = 666;
      DataTools.SetPrimaryKey(table, "FInt32");
      table.DefaultView.RowFilter = "FInt64=777";

      IIndexedIdSet<Int32> res1 = IdTools.GetIds<Int32>(table.DefaultView);
      IIdSet res2 = IdTools.GetIds(table.DefaultView);

      //Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res2);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, ((ICollection)res2).Count);
    }

    [Test]
    public void GetIdsFromColumn_DataView()
    {
      DataTable table = TestTable.Create();
      table.DefaultView.Sort = "FInt64 DESC";

      IIndexedIdSet<Guid> res1 = IdTools.GetIdsFromColumn<Guid>(table.DefaultView, "FGuid");
      IIdSet res2 = IdTools.GetIdsFromColumn(table.DefaultView, "FGuid");

      //Assert.IsInstanceOf<IIndexedIdSet<Guid>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Guid>>(res2);

      Guid[] wantedIds = new Guid[] { TestTable.Row2.VGuid, TestTable.Row1.VGuid };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res2);
    }

    [Test]
    public void GetIdsFromColumn_DataView_empty()
    {
      DataTable table = TestTable.Create();
      table.DefaultView.RowFilter = "FInt64=777";

      IIndexedIdSet<Guid> res1 = IdTools.GetIdsFromColumn<Guid>(table.DefaultView, "FGuid");
      IIdSet res2 = IdTools.GetIdsFromColumn(table.DefaultView, "FGuid");

      //Assert.IsInstanceOf<IIndexedIdSet<Guid>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Guid>>(res2);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, ((ICollection)res2).Count);
    }

    [Test]
    public void GetIdsFromColumns_DataView()
    {
      DataTable table = TestTable.Create();
      table.DefaultView.Sort = "FInt64 DESC";

      IIndexedIdSet<ComplexId<StringId, Int64>> res1 = IdTools.GetIdsFromColumns<ComplexId<StringId, Int64>>(table.DefaultView, new DBxColumns("FString,FInt64"));
      IIdSet res2 = IdTools.GetIdsFromColumns(table.DefaultView, new DBxColumns("FString,FInt64"));

      //Assert.IsInstanceOf<IIndexedIdSet<Guid>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<StringId, Int64>>>(res2);

      ComplexId<StringId, Int64>[] wantedIds = new ComplexId<StringId, Int64>[] {
        new ComplexId<StringId, Int64>(TestTable.Row2.VString, TestTable.Row2.VInt64),
        new ComplexId<StringId, Int64>(TestTable.Row1.VString, TestTable.Row1.VInt64)
      };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res2);
    }

    [Test]
    public void GetIdsFromColumns_DataView_empty()
    {
      DataTable table = TestTable.Create();
      table.DefaultView.RowFilter = "FInt64=777";

      IIndexedIdSet<ComplexId<StringId, Int64>> res1 = IdTools.GetIdsFromColumns<ComplexId<StringId, Int64>>(table.DefaultView, new DBxColumns("FString,FInt64"));
      IIdSet res2 = IdTools.GetIdsFromColumns(table.DefaultView, new DBxColumns("FString,FInt64"));

      //Assert.IsInstanceOf<IIndexedIdSet<Guid>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<StringId, Int64>>>(res2);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, ((ICollection)res2).Count);
    }

    #endregion

    #region Извлечение значений из DataRow collection

    [Test]
    public void GetIds_IEn_DataRow()
    {
      DataTable table = TestTable.Create();
      table.Rows[0]["FInt32"] = 666;
      DataTools.SetPrimaryKey(table, "FInt32");
      DataRow[] rows = DataTools.GetDataTableRows(table, 1, 2);

      IIndexedIdSet<Int32> res1 = IdTools.GetIds<Int32>(rows);
      IIdSet res2 = IdTools.GetIds(rows, table);

      //Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res2);

      Int32[] wantedIds = new Int32[] { TestTable.Row1.VInt32, TestTable.Row2.VInt32 };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res2);
    }

    [Test]
    public void GetIds_IEn_DataRow_empty()
    {
      DataTable table = TestTable.Create();
      table.Rows[0]["FInt32"] = 666;
      DataTools.SetPrimaryKey(table, "FInt32");
      DataRow[] rows = EmptyArray<DataRow>.Empty;

      IIndexedIdSet<Int32> res1 = IdTools.GetIds<Int32>(rows);
      IIdSet res2 = IdTools.GetIds(rows, table);

      //Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Int32>>(res2);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, ((ICollection)res2).Count);
    }

    [Test]
    public void GetIdsFromColumn_IEn_DataRow()
    {
      DataTable table = TestTable.Create();
      DataRow[] rows = DataTools.GetDataTableRows(table, 1, 2);

      IIndexedIdSet<Int64> res1 = IdTools.GetIdsFromColumn<Int64>(rows, "FInt64");
      IIdSet res2 = IdTools.GetIdsFromColumn(rows, "FInt64", table);

      //Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res2);

      Int64[] wantedIds = new Int64[] { TestTable.Row1.VInt64, TestTable.Row2.VInt64 };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res2);
    }

    [Test]
    public void GetIdsFromColumn_IEn_DataRow_empty()
    {
      DataTable table = TestTable.Create();
      DataRow[] rows = EmptyArray<DataRow>.Empty;

      IIndexedIdSet<Int64> res1 = IdTools.GetIdsFromColumn<Int64>(rows, "FInt64");
      IIdSet res2 = IdTools.GetIdsFromColumn(rows, "FInt64", table);

      //Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<Int64>>(res2);

      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, ((ICollection)res2).Count);
    }

    [Test]
    public void GetIdsFromColumns_IEn_DataRow()
    {
      DataTable table = TestTable.Create();
      DataRow[] rows = DataTools.GetDataTableRows(table, 1, 2);

      IIndexedIdSet<ComplexId<Int32, Int64, Guid>> res1 = IdTools.GetIdsFromColumns<ComplexId<Int32, Int64, Guid>>(rows, new DBxColumns("FInt32,FInt64,FGuid"));
      IIdSet res2 = IdTools.GetIdsFromColumns(rows, new DBxColumns("FInt32,FInt64,FGuid"), table);

      //Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Int32, Int64, Guid>>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Int32, Int64, Guid>>>(res2);

      ComplexId<Int32, Int64, Guid>[] wantedIds = new ComplexId<Int32, Int64, Guid>[]       {
        new ComplexId<Int32,Int64,Guid>(TestTable.Row1.VInt32, TestTable.Row1.VInt64, TestTable.Row1.VGuid),
        new ComplexId<Int32,Int64,Guid>(TestTable.Row2.VInt32, TestTable.Row2.VInt64, TestTable.Row2.VGuid) };

      CollectionAssert.AreEqual(wantedIds, res1);
      CollectionAssert.AreEqual(wantedIds, (IEnumerable)res2);
    }

    [Test]
    public void GetIdsFromColumns_IEn_DataRow_empty()
    {
      DataTable table = TestTable.Create();
      DataRow[] rows = EmptyArray<DataRow>.Empty;

      IIndexedIdSet<ComplexId<Int32, Int64, Guid>> res1 = IdTools.GetIdsFromColumns<ComplexId<Int32, Int64, Guid>>(rows, new DBxColumns("FInt32,FInt64,FGuid"));
      IIdSet res2 = IdTools.GetIdsFromColumns(rows, new DBxColumns("FInt32,FInt64,FGuid"), table);

      //Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Int32, Int64, Guid>>>(res1);
      Assert.IsInstanceOf<IIndexedIdSet<ComplexId<Int32, Int64, Guid>>>(res2);
      Assert.AreEqual(0, res1.Count);
      Assert.AreEqual(0, ((ICollection)res2).Count);
    }

    #endregion

    #region FindRows()

    [Test]
    public void FindRows_DataTable()
    {
      DataTable table = TestTable.Create();
      // Нельзя задать первичный ключ, если есть значения DBNull
      table.Rows[0]["FInt32"] = 666;
      table.Rows[0]["FInt64"] = 666L;
      DataTools.SetPrimaryKey(table, "FInt32,FInt64");

      IdList<ComplexId<Int32, Int64>> ids = new IdList<ComplexId<int, long>>();
      ids.Add(new ComplexId<int, long>(TestTable.Row2.VInt32, TestTable.Row2.VInt64));
      ids.Add(new ComplexId<int, long>(TestTable.Row1.VInt32, TestTable.Row1.VInt64));

      DataRow[] res1 = IdTools.FindRows<ComplexId<Int32, Int64>>(table, ids);
      DataRow[] res2 = IdTools.FindRows(table, (IIdSet)ids);

      DataRow[] wantedRes = new DataRow[] { table.Rows[2], table.Rows[1] };
      CollectionAssert.AreEqual(wantedRes, res1);
      CollectionAssert.AreEqual(wantedRes, res2);
    }

    [Test]
    public void FindRowsFromColumn_DataTable()
    {
      DataTable table = TestTable.Create();

      IdList<Int32> ids = new IdList<Int32>();
      ids.Add(TestTable.Row2.VInt32);
      ids.Add(TestTable.Row1.VInt32);

      DataRow[] res1 = IdTools.FindRowsFromColumn<Int32>(table, "FInt32", ids);
      DataRow[] res2 = IdTools.FindRowsFromColumn(table, "FInt32", (IIdSet)ids);

      DataRow[] wantedRes = new DataRow[] { table.Rows[2], table.Rows[1] };
      CollectionAssert.AreEqual(wantedRes, res1);
      CollectionAssert.AreEqual(wantedRes, res2);
    }

    [Test]
    public void FindRowsFromColumns_DataTable()
    {
      DataTable table = TestTable.Create();

      IdList<ComplexId<Int32, Int64>> ids = new IdList<ComplexId<int, long>>();
      ids.Add(new ComplexId<int, long>(TestTable.Row2.VInt32, TestTable.Row2.VInt64));
      ids.Add(new ComplexId<int, long>(TestTable.Row1.VInt32, TestTable.Row1.VInt64));

      DataRow[] res1 = IdTools.FindRowsFromColumns<ComplexId<Int32, Int64>>(table, new DBxColumns("FInt32,FInt64"), ids);
      DataRow[] res2 = IdTools.FindRowsFromColumns(table, new DBxColumns("FInt32,FInt64"), (IIdSet)ids);

      DataRow[] wantedRes = new DataRow[] { table.Rows[2], table.Rows[1] };
      CollectionAssert.AreEqual(wantedRes, res1);
      CollectionAssert.AreEqual(wantedRes, res2);
    }

    #endregion

    #region GetBlockedIds()

    [TestCase(0, 100)]
    [TestCase(1, 100)]
    [TestCase(100, 100)]
    [TestCase(101, 100)]
    [TestCase(1000, 100)]
    public void GetBlockedIds(int rowCount, int n)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(Int32));
      DataTools.SetPrimaryKey(table, "Id");
      List<Int32> ids = new List<Int32>();
      for (int i = 1; i <= rowCount; i++)
      {
        table.Rows.Add(i);
        ids.Add(i);
      }

      IIdSet<Int32>[] res1 = IdTools.GetBlockedIds<Int32>(table, n);
      TestGetBlockedIds(rowCount, n, res1, "DataTable");

      IIdSet<Int32>[] res2 = IdTools.GetBlockedIds<Int32>(table.DefaultView, n);
      TestGetBlockedIds(rowCount, n, res2, "DataView");

      IIdSet<Int32>[] res3 = IdTools.GetBlockedIds<Int32>(DataTools.GetDataTableRows(table), n);
      TestGetBlockedIds(rowCount, n, res3, "IEnumerable<DataRow>");

      IIdSet<Int32>[] res4 = IdTools.GetBlockedIds<Int32>(ids, n);
      TestGetBlockedIds(rowCount, n, res3, "IEnumerable<Int32>");
    }

    private static void TestGetBlockedIds(int rowCount, int n, IIdSet<Int32>[] res, string msg)
    {
      int cnt = 0;
      for (int i = 0; i < res.Length; i++)
      {
        Assert.IsNotNull(res[i], String.Format("{0}, res[{1}] is null", msg, i));

        if (i == (res.Length - 1))
          Assert.LessOrEqual(res[i].Count, n, String.Format("{0}, res[{1}].Count", msg, i));
        else
          Assert.AreEqual(n, res[i].Count, String.Format("{0}, res[{1}].Count", msg, i));

        foreach (Int32 id in res[i])
        {
          cnt++;
          Assert.AreEqual(cnt, id, String.Format("{0}, Id in res[{1}]", msg, i));
        }
      }
      Assert.AreEqual(rowCount, cnt, String.Format("{0}, Total count", msg));
    }

    #endregion

    #region GetRowsFromIds()

    [TestCase(10, "1,5,10,11")]
    [TestCase(1, "1")]
    [TestCase(0, "1")]
    public void GetRowsFromIds(int nRows, string sIds)
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      for (int i = 1; i <= nRows; i++)
        tbl.Rows.Add(StdConvert.ToString(i));
      DataTools.SetPrimaryKey(tbl, "Id");

      Int32[] ids = StdConvert.ToInt32Array(sIds);

      DataRow[] rows = IdTools.FindRows<Int32>(tbl, new IdArray<Int32>(ids));
      Assert.AreEqual(ids.Length, rows.Length, "Length");

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] > 0 && ids[i] <= nRows)
        {
          Assert.IsNotNull(rows[i], "HasRow (" + ids[i] + ")");
          Assert.AreEqual(ids[i], rows[i]["Id"], "Id=" + ids[i].ToString());
        }
        else
          Assert.IsNull(rows[i], "Null (" + ids[i] + ")");
      }

      // Проверяем, что первичный ключ таблицы не испортился
      Assert.AreEqual("Id", DataTools.GetPrimaryKey(tbl), "Primary key");
    }

    #endregion

    #region TableFromIds()

    [Test]
    public void TableFromIds()
    {
      DataTable tbl = IdTools.TableFromIds<Int32>(new Int32[] { 3, 1, 2 }, "Id");

      Assert.AreEqual(1, tbl.Columns.Count, "Columns.Count");
      Assert.AreEqual("Id", tbl.Columns[0].ColumnName, "ColumnName");
      Assert.AreEqual(typeof(Int32), tbl.Columns[0].DataType, "DataType");

      Assert.AreEqual(3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(3, tbl.Rows[0]["Id"], "[0]");
      Assert.AreEqual(1, tbl.Rows[1]["Id"], "[1]");
      Assert.AreEqual(2, tbl.Rows[2]["Id"], "[2]");
    }

    #endregion

    #region CloneOrSameTableForSelectedIds()

    [Test]
    public void CloneTableForSelectedIds()
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");
      int[] ids = new int[] { 1, 7 };

      DataTable res = IdTools.CloneTableForSelectedIds<Int32>(tbl, new IdArray<Int32>(ids));
      Assert.AreEqual(2, res.Rows.Count, "Rows.Count");
      Assert.AreEqual(tbl.Rows[0].ItemArray, res.Rows[0].ItemArray, "[0]");
      Assert.AreEqual(tbl.Rows[2].ItemArray, res.Rows[1].ItemArray, "[1]");
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(res), "PrimaryKey");
    }


    [Test]
    public void CloneOrSameTableForSelectedIds()
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");

      int[] ids1 = new int[] { 1, 7 };
      DataTable res1 = IdTools.CloneOrSameTableForSelectedIds<Int32>(tbl, new IdArray<Int32>(ids1));
      Assert.AreNotSame(res1, tbl, "Clone");
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(res1), "PrimaryKey");

      int[] ids2 = new int[] { 1, 4, 7 };
      DataTable res2 = IdTools.CloneOrSameTableForSelectedIds<Int32>(tbl, new IdArray<Int32>(ids2));
      Assert.AreSame(res2, tbl, "Same");
    }

    #endregion

    #region Тестовая таблица

    /// <summary>
    /// Тестовая таблица из 3 строк
    /// </summary>
    /// <returns></returns>
    private static DataTable CreateTestTable123()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));
      tbl.Rows.Add(1, 2, 3);
      tbl.Rows.Add(4, 5, 6);
      tbl.Rows.Add(7, 8, 9);
      return tbl;
    }

    #endregion

    #region GetRandomId()

    [Test]
    public void GetRandomId()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32)); // Не обязательно "Id"
      DataTools.SetPrimaryKey(tbl, "F1");

      for (int i = 0; i < 1000; i++)
      {
        Int32 id = IdTools.GetRandomId<Int32>(tbl);
        tbl.Rows.Add(id);
      }
    }

    #endregion
  }
}
