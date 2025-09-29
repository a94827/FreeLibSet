using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class IdsFilterGeneratorTests
  {
    #region Конструкторы

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(50)]
    [TestCase(51)]
    [TestCase(100)]
    [TestCase(101)]
    [TestCase(1000)]
    [TestCase(1001)]
    public void Constructors_1arg(int count)
    {
      Int64[] aIds = new Int64[count];
      for (int i = 0; i < count; i++)
        aIds[i] = i * 3 + 1;
      IdList<Int64> lstIds = new IdList<Int64>(aIds);

      IdsFilterGenerator<Int64> sut1 = new IdsFilterGenerator<Int64>(aIds);
      DoTestObject<Int64>(sut1, aIds, "Array");

      IdsFilterGenerator<Int64> sut2 = new IdsFilterGenerator<Int64>(lstIds);
      DoTestObject<Int64>(sut2, aIds, "IdList");
    }

    [TestCase(0, 50)]
    [TestCase(1, 50)]
    [TestCase(50, 50)]
    [TestCase(51, 50)]
    [TestCase(100, 50)]
    [TestCase(1001, 50)]
    [TestCase(1, 1)]
    [TestCase(3, 1)]
    public void Constructors_2arg(int count, int maxCount)
    {
      int[] aIds = CreateIdArray(count);
      IdList<Int32> lstIds = new IdList<Int32>(aIds);

      IdsFilterGenerator<Int32> sut1 = new IdsFilterGenerator<Int32>(aIds, maxCount);
      DoTestObject(sut1, aIds, "Array");

      IdsFilterGenerator<Int32> sut2 = new IdsFilterGenerator<Int32>(lstIds, maxCount);
      DoTestObject(sut2, aIds, "IdList");
    }

    private static Int32[] CreateIdArray(int count)
    {
      Int32[] aIds = new Int32[count];
      for (int i = 0; i < count; i++)
        aIds[i] = i * 3 + 1;
      return aIds;
    }

    private static void DoTestObject<T>(IdsFilterGenerator<T> sut, T[] aIds, string messagePrefix)
      where T : struct, IEquatable<T>
    {
      Assert.AreEqual(aIds.Length, sut.AllIdCount, messagePrefix + "AllIdsCount");
      CollectionAssert.AreEquivalent(aIds, sut.GetAllIds(), "GetAllIds()");
      CollectionAssert.AreEquivalent(aIds, sut.GetWholeIdList(), "GetWholeIdList()");

      IdList<T> resList = new IdList<T>();
      for (int i = 0; i < sut.Count; i++)
        resList.AddRange(sut.GetIds(i));
      CollectionAssert.AreEquivalent(aIds, resList, "GetIds()");
    }

    #endregion

    #region CreateFilters()

    [Test]
    public void CreateFilters_1arg()
    {
      Int32[] aIds = CreateIdArray(299);

      IdsFilterGenerator<Int32> sut = new IdsFilterGenerator<Int32>(aIds, 50);
      sut.CreateFilters("F1");

      IdList<Int32> resList = new IdList<Int32>();
      int cnt = 0;

      foreach (ValueInListFilter filter in sut)
      {
        Assert.IsInstanceOf<DBxColumn>(filter.Expression, "Expression.GetType()");
        Assert.AreEqual("F1", ((DBxColumn)(filter.Expression)).ColumnName, "ColumnName");
        resList.AddRange(ArrayTools.CreateArray<Int32>(filter.Values));
        cnt++;
      }
      Assert.AreEqual(sut.Count, cnt, "Count");
      CollectionAssert.AreEquivalent(aIds, resList, "Ids");
    }

    [Test]
    public void CreateFilters_0arg()
    {
      Int32[] aIds = CreateIdArray(299);

      IdsFilterGenerator<Int32> sut = new IdsFilterGenerator<Int32>(aIds, 50);
      sut.CreateFilters("Id");

      IdList<Int32> resList = new IdList<Int32>();
      int cnt = 0;

      foreach (ValueInListFilter filter in sut)
      {
        Assert.IsInstanceOf<DBxColumn>(filter.Expression, "Expression.GetType()");
        Assert.AreEqual("Id", ((DBxColumn)(filter.Expression)).ColumnName, "ColumnName");
        resList.AddRange(ArrayTools.CreateArray<Int32>(filter.Values));
        cnt++;
      }
      Assert.AreEqual(sut.Count, cnt, "Count");
      CollectionAssert.AreEquivalent(aIds, resList, "Ids");
    }


    #endregion
  }
}
