using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;

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
      Int32[] aIds = new Int32[count];
      for (int i = 0; i < count; i++)
        aIds[i] = i * 3 + 1;
      IdList lstIds = new IdList(aIds);

      IdsFilterGenerator sut1 = new IdsFilterGenerator(aIds);
      DoTestObject(sut1, aIds, "Array");

      IdsFilterGenerator sut2 = new IdsFilterGenerator(lstIds);
      DoTestObject(sut2, aIds, "IdList");
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
      IdList lstIds = new IdList(aIds);

      IdsFilterGenerator sut1 = new IdsFilterGenerator(aIds, maxCount);
      DoTestObject(sut1, aIds, "Array");

      IdsFilterGenerator sut2 = new IdsFilterGenerator(lstIds, maxCount);
      DoTestObject(sut2, aIds, "IdList");
    }

    private static Int32[] CreateIdArray(int count)
    {
      Int32[] aIds = new Int32[count];
      for (int i = 0; i < count; i++)
        aIds[i] = i * 3 + 1;
      return aIds;
    }

    private static void DoTestObject(IdsFilterGenerator sut, Int32[] aIds, string messagePrefix)
    {
      Assert.AreEqual(aIds.Length, sut.AllIdCount, messagePrefix + "AllIdsCount");
      CollectionAssert.AreEquivalent(aIds, sut.GetAllIds(), "GetAllIds()");
      CollectionAssert.AreEquivalent(aIds, sut.GetWholeIdList(), "GetWholeIdList()");

      IdList resList = new IdList();
      for (int i = 0; i < sut.Count; i++)
        resList.Add(sut.GetIds(i));
      CollectionAssert.AreEquivalent(aIds, resList, "GetIds()");
    }

    #endregion

    #region CreateFilters()

    [Test]
    public void CreateFilters_1arg()
    {
      Int32[] aIds = CreateIdArray(299);

      IdsFilterGenerator sut = new IdsFilterGenerator(aIds, 50);
      sut.CreateFilters("F1");

      IdList resList = new IdList();
      int cnt = 0;

      foreach (IdsFilter filter in sut)
      {
        Assert.IsInstanceOf<DBxColumn>(filter.Expression, "Expression.GetType()");
        Assert.AreEqual("F1", ((DBxColumn)(filter.Expression)).ColumnName, "ColumnName");
        resList.Add(filter.Ids);
        cnt++;
      }
      Assert.AreEqual(sut.Count, cnt, "Count");
      CollectionAssert.AreEquivalent(aIds, resList, "Ids");
    }

    [Test]
    public void CreateFilters_0arg()
    {
      Int32[] aIds = CreateIdArray(299);

      IdsFilterGenerator sut = new IdsFilterGenerator(aIds, 50);
      sut.CreateFilters();

      IdList resList = new IdList();
      int cnt = 0;

      foreach (IdsFilter filter in sut)
      {
        Assert.IsInstanceOf<DBxColumn>(filter.Expression, "Expression.GetType()");
        Assert.AreEqual("Id", ((DBxColumn)(filter.Expression)).ColumnName, "ColumnName");
        resList.Add(filter.Ids);
        cnt++;
      }
      Assert.AreEqual(sut.Count, cnt, "Count");
      CollectionAssert.AreEquivalent(aIds, resList, "Ids");
    }


    #endregion
  }
}
