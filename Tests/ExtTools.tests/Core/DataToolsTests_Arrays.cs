using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataToolsTests_Arrays
  {
    #region Empty arrays

    [Test]
    public void EmptyArrays()
    {
      Assert.AreEqual(0, DataTools.EmptyObjects.Length);
      Assert.AreEqual(0, DataTools.EmptyStrings.Length);
      Assert.AreEqual(0, DataTools.EmptyInts.Length);
      Assert.AreEqual(0, DataTools.EmptyInt64s.Length);
      Assert.AreEqual(0, DataTools.EmptySingles.Length);
      Assert.AreEqual(0, DataTools.EmptyDoubles.Length);
      Assert.AreEqual(0, DataTools.EmptyDecimals.Length);
      Assert.AreEqual(0, DataTools.EmptyGuids.Length);
      Assert.AreEqual(0, DataTools.EmptyDateTimes.Length);
      Assert.AreEqual(0, DataTools.EmptyTimeSpans.Length);
      Assert.AreEqual(0, DataTools.EmptyBools.Length);
      Assert.AreEqual(0, DataTools.EmptyBytes.Length);
      Assert.AreEqual(0, DataTools.EmptyGuids.Length);
      Assert.AreEqual(0, DataTools.EmptySortDirections.Length);
      Assert.AreEqual(0, DataTools.EmptyIds.Length);
    }

    #endregion

    #region FillArray(), FillArray2()

    [Test]
    public void FillArray()
    {
      int[] a = new int[3];
      DataTools.FillArray<int>(a, 123);

      for (int i = 0; i < a.Length; i++)
        Assert.AreEqual(123, a[i]);
    }

    [Test]
    public void FillArray2()
    {
      int[,] a = new int[3, 2];
      DataTools.FillArray2<int>(a, 123);

      for (int i = 0; i < 3; i++)
      {
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(123, a[i, j]);
      }
    }

    #endregion

    #region CreateArray()

    [Test]
    public void CreateArray_value()
    {
      int[] a = DataTools.CreateArray<int>(3, 123);
      Assert.AreEqual(3, a.Length, "Length");
      for (int i = 0; i < 3; i++)
        Assert.AreEqual(123, a[i], "Item");
    }

    [Test]
    public void CreateArray_enumerable([Values(false, true)]bool isEmpty)
    {
      List<int> lst = new List<int>();
      if (!isEmpty) // Есть специальная оптимизация для пустого перечислителя
      {
        lst.Add(2);
        lst.Add(1);
        lst.Add(3);
      }

      IEnumerable<int> source = lst;
      int[] a = DataTools.CreateArray<int>(source);

      Assert.AreEqual(lst.Count, a.Length, "Length");
      for (int i = 0; i < lst.Count; i++)
        Assert.AreEqual(lst[i], a[i], "Item");
    }

    #endregion

    #region CreateObjectArray()

    [Test]
    public void CreateObjectArray([Values(false, true)]bool isEmpty)
    {
      List<int> lst = new List<int>();
      if (!isEmpty) // Есть специальная оптимизация для пустого перечислителя
      {
        lst.Add(2);
        lst.Add(1);
        lst.Add(3);
      }

      System.Collections.IEnumerable source = lst;
      object[] a = DataTools.CreateObjectArray(source);

      Assert.AreEqual(lst.Count, a.Length, "Length");
      for (int i = 0; i < lst.Count; i++)
        Assert.AreEqual(lst[i], a[i], "Item");
    }

    #endregion

    #region CreateDBNullArray()

    [Test]
    public void CreateDBNullArray()
    {
      object[] a = DataTools.CreateDBNullArray(3);
      Assert.AreEqual(3, a.Length, "Length");
      for (int i = 0; i < 3; i++)
        Assert.IsTrue(a[i] is DBNull, "Item");
    }

    #endregion

    #region CreateSelectedArray()

    [Test]
    public void CreateSelectedArray()
    {
      int[] a = new int[] { 111, 222, 333 };
      bool[] flags = new bool[] { true, false, true };

      int[] res = DataTools.CreateSelectedArray<int>(a, flags);

      Assert.AreEqual(new int[] { 111, 333 }, res);
    }

    #endregion

    #region AreArraysEqual()

    [Test]
    public void AreArraysEqual_untyped()
    {
      int[,] a1 = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
      int[,] a2 = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
      Assert.IsTrue(DataTools.AreArraysEqual(a1, a2), "Equal");

      a2[1, 1] = 10;
      Assert.IsFalse(DataTools.AreArraysEqual(a1, a2), "Item diff");

      int[,] a3 = new int[2, 2] { { 1, 2 }, { 3, 4 } };
      Assert.IsFalse(DataTools.AreArraysEqual(a1, a3), "Length diff 1");
      Assert.IsFalse(DataTools.AreArraysEqual(a3, a1), "Length diff 2");

      int[,] a4 = new int[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
      Assert.IsFalse(DataTools.AreArraysEqual(a1, a4), "Bounds diff 1");
      Assert.IsFalse(DataTools.AreArraysEqual(a4, a1), "Bounds diff 1");

      int[] a5 = new int[6] { 1, 2, 3, 4, 5, 6 };
      Assert.IsFalse(DataTools.AreArraysEqual(a1, a5), "Rank diff 1");
      Assert.IsFalse(DataTools.AreArraysEqual(a5, a1), "Rank diff 2");
    }

    [Test]
    public void AreArraysEqual_typed()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 1, 2, 3 };
      Assert.IsTrue(DataTools.AreArraysEqual<int>(a1, a2), "Equal");

      a2[2] = 10;
      Assert.IsFalse(DataTools.AreArraysEqual<int>(a1, a2), "Item diff");

      int[] a3 = new int[] { 1, 2 };
      Assert.IsFalse(DataTools.AreArraysEqual<int>(a1, a3), "Length diff");
    }

    [Test]
    public void AreArraysEqual_string()
    {
      string[] a1 = new string[] { "AAA", "BBB", "CCC" };
      string[] a2 = new string[] { "aaa", "bbb", "ccc" };

      Assert.IsTrue(DataTools.AreArraysEqual(a1, a2, StringComparison.OrdinalIgnoreCase), "OrdinalIgnoreCase");
      Assert.IsFalse(DataTools.AreArraysEqual(a1, a2, StringComparison.Ordinal), "Ordinal");
    }

    #endregion

    #region MergeArrays()

    [Test]
    public void MergeArrays()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 2, 4, 5 };
      int[] a3 = new int[] { 5, 6, 7 };

      int[] res = DataTools.MergeArrays<int>(a1, a2, a3);
      Assert.AreEqual(new int[] { 1, 2, 3, 2, 4, 5, 5, 6, 7 }, res); // порядок сохраняется
    }

    #endregion

    #region MergeArraysOnce()

    [Test]
    public void MergeArraysOnce_small()
    {
      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { 2, 4 };

      int[] res1 = DataTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 4, }, res1, "#1");

      int[] res2 = DataTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 4, }, res2, "#2");
    }

    [Test]
    public void MergeArraysOnce_large()
    {
      // Использует индексатор.
      // см. реализацию DataTools.MergeArrayOnce()

      int[] a1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[] a2 = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

      int[] res1 = DataTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20 }, res1, "#1");

      int[] res2 = DataTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20 }, res2, "#2");
    }

    [Test]
    public void MergeArraysOnce_within()
    {
      // Особый случай, когда один массив полностью входит в другой
      // см. реализацию DataTools.MergeArrayOnce()

      int[] a1 = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { 2, 4 };

      int[] res1 = DataTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res1, "#1");

      int[] res2 = DataTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res2, "#2");

      int[] res3 = DataTools.MergeArraysOnce<int>(a1, a1);
      Array.Sort<int>(res2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res2, "same array");
    }

    [Test]
    public void MergeArraysOnce_empty()
    {
      // Особый случай, когда один массив пустой
      // см. реализацию DataTools.MergeArrayOnce()

      int[] a1 = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { };

      int[] res1 = DataTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res1, "#1");

      int[] res2 = DataTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res2, "#2");

      int[] res3 = DataTools.MergeArraysOnce<int>(a2, a2);
      Assert.AreEqual(new int[] { }, res3, "both empty");
    }

    #endregion

    #region MergeArraysBoth()

    [Test]
    public void MergeArraysBoth_small()
    {
      // Разная реализация MergeArraysBoth(), в зависимости от длины массивов

      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { 2, 4 };

      int[] res1 = DataTools.MergeArraysBoth<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2 }, res1, "#1");

      int[] res2 = DataTools.MergeArraysBoth<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2 }, res2, "#2");
    }

    [Test]
    public void MergeArraysBoth_large()
    {
      // Разная реализация MergeArraysBoth(), в зависимости от длины массивов

      int[] a1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[] a2 = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

      int[] res1 = DataTools.MergeArraysBoth<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2, 4, 6, 8, 10 }, res1, "#1");

      int[] res2 = DataTools.MergeArraysBoth<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2, 4, 6, 8, 10 }, res2, "#2");
    }

    [Test]
    public void MergeArraysBoth_empty()
    {
      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { };

      int[] res1 = DataTools.MergeArraysBoth<int>(a1, a2);
      Assert.AreEqual(new int[] { }, res1, "#1");

      int[] res2 = DataTools.MergeArraysBoth<int>(a2, a1);
      Assert.AreEqual(new int[] { }, res2, "#2");

      int[] res3 = DataTools.MergeArraysBoth<int>(a2, a2);
      Assert.AreEqual(new int[] { }, res3, "both empty");
    }

    #endregion

    #region RemoveFromArray()

    [Test]
    public void RemoveFromArray_normal()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { 2, 4 };
      DataTools.RemoveFromArray<int>(ref a, a2);
      Assert.AreEqual(new int[] { 1, 3, 5 }, a);
    }

    [Test]
    public void RemoveFromArray_empty_1()
    {
      int[] a = new int[] { };
      int[] a2 = new int[] { 2, 4 };
      DataTools.RemoveFromArray<int>(ref a, a2);
      Assert.AreEqual(new int[] { }, a);
    }

    [Test]
    public void RemoveFromArray_empty_2()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { };
      DataTools.RemoveFromArray<int>(ref a, a2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, a);
    }

    #endregion

    #region DeleteFromArray()

    [Test]
    public void DeleteFromArray_left()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      DataTools.DeleteFromArray<int>(ref a, 0, 2);
      Assert.AreEqual(new int[] { 3, 4, 5 }, a);
    }

    [Test]
    public void DeleteFromArray_middle()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      DataTools.DeleteFromArray<int>(ref a, 1, 2);
      Assert.AreEqual(new int[] { 1, 4, 5 }, a);
    }

    [Test]
    public void DeleteFromArray_right()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      DataTools.DeleteFromArray<int>(ref a, 3, 2);
      Assert.AreEqual(new int[] { 1, 2, 3 }, a);
    }

    [Test]
    public void DeleteFromArray_none()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      DataTools.DeleteFromArray<int>(ref a, 3, 0);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, a);
    }

    [Test]
    public void DeleteFromArray_all()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      DataTools.DeleteFromArray<int>(ref a, 0, 5);
      Assert.AreEqual(new int[] { }, a);
    }

    #endregion

    #region InsertIntoArray()

    [Test]
    public void InsertIntoArray_left()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      DataTools.InsertIntoArray<int>(ref a, 0, b);
      Assert.AreEqual(new int[] { 6, 7, 1, 2, 3, 4, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_middle()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      DataTools.InsertIntoArray<int>(ref a, 2, b);
      Assert.AreEqual(new int[] { 1, 2, 6, 7, 3, 4, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_right()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      DataTools.InsertIntoArray<int>(ref a, 4, b);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 6, 7, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_last()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      DataTools.InsertIntoArray<int>(ref a, 5, b);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7 }, a);
    }

    [Test]
    public void InsertIntoArray_none()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { };
      DataTools.InsertIntoArray<int>(ref a, 2, b);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_to_empty()
    {
      int[] a = new int[] { };
      int[] b = new int[] { 6, 7 };
      DataTools.InsertIntoArray<int>(ref a, 0, b);
      Assert.AreEqual(new int[] { 6, 7 }, a);
    }

    [Test]
    public void InsertIntoArray_both_empty()
    {
      int[] a = new int[] { };
      int[] b = new int[] { };
      DataTools.InsertIntoArray<int>(ref a, 0, b);
      Assert.AreEqual(new int[] { }, a);
    }

    #endregion

    #region FirstItem()/LastItem()

    [Test]
    public void FirstItem_normal()
    {
      int[] a = new int[] { 1, 2, 3 };
      Assert.AreEqual(1, DataTools.FirstItem<int>(a));
    }

    [Test]
    public void FirstItem_empty()
    {
      int[] a = new int[] { };
      Assert.AreEqual(0, DataTools.FirstItem<int>(a));
    }

    [Test]
    public void LastItem_normal()
    {
      int[] a = new int[] { 1, 2, 3 };
      Assert.AreEqual(3, DataTools.LastItem<int>(a));
    }

    [Test]
    public void LastItem_empty()
    {
      int[] a = new int[] { };
      Assert.AreEqual(0, DataTools.LastItem<int>(a));
    }

    #endregion

    #region ArrayStartsWith() / ArrayEndsWith()

    [Test]
    public void ArrayStartsWith_yes()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsTrue(DataTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_length()
    {
      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { 1, 2, 3 };

      Assert.IsFalse(DataTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_value()
    {
      int[] a1 = new int[] { 1, 4, 3 };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsFalse(DataTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_empty1()
    {
      int[] a1 = new int[] { };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsFalse(DataTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_empty2()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { };

      Assert.IsTrue(DataTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_yes()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 2, 3 };

      Assert.IsTrue(DataTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_length()
    {
      int[] a1 = new int[] { 2, 3 };
      int[] a2 = new int[] { 1, 2, 3 };

      Assert.IsFalse(DataTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_value()
    {
      int[] a1 = new int[] { 1, 4, 3 };
      int[] a2 = new int[] { 2, 3 };

      Assert.IsFalse(DataTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_empty1()
    {
      int[] a1 = new int[] { };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsFalse(DataTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_empty2()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { };

      Assert.IsTrue(DataTools.ArrayEndsWith<int>(a1, a2));
    }

    #endregion

    #region GetArray2Row(), GetArray2Column()

    [Test]
    public void GetArray2Row()
    {
      int[,] a = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
      int[] res = DataTools.GetArray2Row<int>(a, 1);
      Assert.AreEqual(new int[] { 4, 5, 6 }, res);
    }

    [Test]
    public void GetArray2Column()
    {
      int[,] a = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
      int[] res = DataTools.GetArray2Column<int>(a, 1);
      Assert.AreEqual(new int[] { 2, 5, 8 }, res);
    }

    #endregion

    #region ToArray1()

    [Test]
    public void ToArray1_from_2d()
    {
      int[,] a = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
      int[] res = DataTools.ToArray1<int>(a);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6 }, res);
    }

    [Test]
    public void ToArray1_from_jagged_1d()
    {
      int[][] a = new int[][] { new int[] { 1, 2, 3 }, null, new int[] { 4, 5 } };
      int[] res = DataTools.ToArray1<int>(a);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res);
    }

    [Test]
    public void ToArray1_from_jagged_2d()
    {
      int[,][] a = new int[2, 2][] { { new int[] { 1, 2, 3 }, new int[] { 4, 5 } }, { null, new int[] { 6 } } };
      int[] res = DataTools.ToArray1<int>(a);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6 }, res);
    }

    #endregion

    #region ToArray2()

    [Test]
    public void ToArray2()
    {
      int[][] a = new int[3][] { new int[] { 1, 2 }, null, new int[] { 3, 4, 5, 6 } };
      int[,] res = DataTools.ToArray2<int>(a);
      Assert.AreEqual(new int[3, 4] { { 1, 2, 0, 0 }, { 0, 0, 0, 0 }, { 3, 4, 5, 6 } }, res);
    }

    #endregion

    #region GetBlockedArray()

    [Test]
    public void GetBlockedArray()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[][] res = DataTools.GetBlockedArray<int>(a, 3);
      int[][] wanted = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, new int[] { 10 } };
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region GetArrayNotNullCount()/GetArrayNullCount()

    [Test]
    public void GetArrayNullNotNullCount_1d()
    {
      string[] a = new string[] { "AAA", null, null, "BBB", "CCC", "DDD" };
      Assert.AreEqual(4, DataTools.GetArrayNotNullCount(a, false), "GetArrayNotNullCount()");
      Assert.AreEqual(2, DataTools.GetArrayNullCount(a, false), "GetArrayNotNullCount()");
    }


    [Test]
    public void GetArrayNullNotNullCount_2d()
    {
      string[,] a = new string[,] { { "AAA", null }, { null, "BBB" }, { "CCC", "DDD" } };
      Assert.AreEqual(4, DataTools.GetArrayNotNullCount(a, false), "GetArrayNotNullCount()");
      Assert.AreEqual(2, DataTools.GetArrayNullCount(a, false), "GetArrayNotNullCount()");
    }

    [Test]
    public void GetArrayNullNotNullCount_jagged()
    {
      string[][] a = new string[][] { new string[] { "AAA", null }, new string[] { null, "BBB" }, new string[] { "CCC", "DDD" } };
      Assert.AreEqual(4, DataTools.GetArrayNotNullCount(a, true), "GetArrayNotNullCount()");
      Assert.AreEqual(2, DataTools.GetArrayNullCount(a, true), "GetArrayNotNullCount()");
    }

    #endregion
  }
}
