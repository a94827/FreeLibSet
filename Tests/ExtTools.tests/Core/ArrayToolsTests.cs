using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Core;
using System.Collections;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class ArrayToolsTests
  {
    #region FillArray(), FillMatrix()

    [Test]
    public void FillArray()
    {
      int[] a = new int[3];
      ArrayTools.FillArray<int>(a, 123);

      for (int i = 0; i < a.Length; i++)
        Assert.AreEqual(123, a[i]);
    }

    [Test]
    public void FillMatrix()
    {
      int[,] a = new int[3, 2];
      ArrayTools.FillMatrix<int>(a, 123);

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
      int[] a = ArrayTools.CreateArray<int>(3, 123);
      Assert.AreEqual(3, a.Length, "Length");
      for (int i = 0; i < 3; i++)
        Assert.AreEqual(123, a[i], "Item");
    }

    [Test]
    public void CreateArray_typedenumerable([Values(false, true)]bool isEmpty)
    {
      List<int> lst = new List<int>();
      if (!isEmpty) // Есть специальная оптимизация для пустого перечислителя
      {
        lst.Add(2);
        lst.Add(1);
        lst.Add(3);
      }

      IEnumerable<int> source = lst;
      int[] a = ArrayTools.CreateArray<int>(source);

      Assert.AreEqual(lst.Count, a.Length, "Length");
      for (int i = 0; i < lst.Count; i++)
        Assert.AreEqual(lst[i], a[i], "Item");
    }


    [Test]
    public void CreateArray_untypedenumerable([Values(false, true)]bool isEmpty)
    {
      List<int> lst = new List<int>();
      if (!isEmpty) // Есть специальная оптимизация для пустого перечислителя
      {
        lst.Add(2);
        lst.Add(1);
        lst.Add(3);
      }

      IEnumerable source = lst;
      int[] a = ArrayTools.CreateArray<int>(source);

      Assert.AreEqual(lst.Count, a.Length, "Length");
      for (int i = 0; i < lst.Count; i++)
        Assert.AreEqual(lst[i], a[i], "Item");
    }


    [Test]
    public void CreateArray_untypedenumerable_autoconvert([Values(false, true)]bool isEmpty)
    {
      List<int> lst = new List<int>();
      if (!isEmpty) // Есть специальная оптимизация для пустого перечислителя
      {
        lst.Add(2);
        lst.Add(1);
        lst.Add(3);
      }

      IEnumerable source = lst;
      long[] a = ArrayTools.CreateArray<long>(source);

      Assert.AreEqual(lst.Count, a.Length, "Length");
      for (int i = 0; i < lst.Count; i++)
        Assert.AreEqual((long)(lst[i]), a[i], "Item");
    }


    [Test]
    public void CreateArray_untypedenumerable_typeerror()
    {
      List<int> lst = new List<int>();
      lst.Add(2);
      lst.Add(1);
      lst.Add(3);

      IEnumerable source = lst;
      DateTime[] dummy;
      Assert.Catch(delegate () { dummy = ArrayTools.CreateArray<DateTime>(source); });
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
      object[] a = ArrayTools.CreateObjectArray(source);

      Assert.AreEqual(lst.Count, a.Length, "Length");
      for (int i = 0; i < lst.Count; i++)
        Assert.AreEqual(lst[i], a[i], "Item");
    }

    #endregion

    #region CreateSelectedArray()

    [Test]
    public void CreateSelectedArray()
    {
      int[] a = new int[] { 111, 222, 333 };
      bool[] flags = new bool[] { true, false, true };

      int[] res = ArrayTools.CreateSelectedArray<int>(a, flags);

      Assert.AreEqual(new int[] { 111, 333 }, res);
    }

    #endregion

    #region AreArraysEqual()

    [Test]
    public void AreArraysEqual_untyped()
    {
      int[,] a1 = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
      int[,] a2 = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
      Assert.IsTrue(ArrayTools.AreArraysEqual(a1, a2), "Equal");

      a2[1, 1] = 10;
      Assert.IsFalse(ArrayTools.AreArraysEqual(a1, a2), "Item diff");

      int[,] a3 = new int[2, 2] { { 1, 2 }, { 3, 4 } };
      Assert.IsFalse(ArrayTools.AreArraysEqual(a1, a3), "Length diff 1");
      Assert.IsFalse(ArrayTools.AreArraysEqual(a3, a1), "Length diff 2");

      int[,] a4 = new int[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
      Assert.IsFalse(ArrayTools.AreArraysEqual(a1, a4), "Bounds diff 1");
      Assert.IsFalse(ArrayTools.AreArraysEqual(a4, a1), "Bounds diff 1");

      int[] a5 = new int[6] { 1, 2, 3, 4, 5, 6 };
      Assert.IsFalse(ArrayTools.AreArraysEqual(a1, a5), "Rank diff 1");
      Assert.IsFalse(ArrayTools.AreArraysEqual(a5, a1), "Rank diff 2");
    }

    [Test]
    public void AreArraysEqual_typed()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 1, 2, 3 };
      Assert.IsTrue(ArrayTools.AreArraysEqual<int>(a1, a2), "Equal");

      a2[2] = 10;
      Assert.IsFalse(ArrayTools.AreArraysEqual<int>(a1, a2), "Item diff");

      int[] a3 = new int[] { 1, 2 };
      Assert.IsFalse(ArrayTools.AreArraysEqual<int>(a1, a3), "Length diff");
    }

    [Test]
    public void AreArraysEqual_string()
    {
      string[] a1 = new string[] { "AAA", "BBB", "CCC" };
      string[] a2 = new string[] { "aaa", "bbb", "ccc" };

      Assert.IsTrue(ArrayTools.AreArraysEqual(a1, a2, StringComparison.OrdinalIgnoreCase), "OrdinalIgnoreCase");
      Assert.IsFalse(ArrayTools.AreArraysEqual(a1, a2, StringComparison.Ordinal), "Ordinal");
    }

    #endregion

    #region MergeArrays()

    [Test]
    public void MergeArrays()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 2, 4, 5 };
      int[] a3 = new int[] { 5, 6, 7 };

      int[] res = ArrayTools.MergeArrays<int>(a1, a2, a3);
      Assert.AreEqual(new int[] { 1, 2, 3, 2, 4, 5, 5, 6, 7 }, res); // порядок сохраняется
    }

    #endregion

    #region MergeArraysOnce()

    [Test]
    public void MergeArraysOnce_small()
    {
      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { 2, 4 };

      int[] res1 = ArrayTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 4, }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 4, }, res2, "#2");
    }

    [Test]
    public void MergeArraysOnce_large()
    {
      // Использует индексатор.
      // см. реализацию ArrayTools.MergeArrayOnce()

      int[] a1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[] a2 = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

      int[] res1 = ArrayTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20 }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20 }, res2, "#2");
    }

    [Test]
    public void MergeArraysOnce_within()
    {
      // Особый случай, когда один массив полностью входит в другой
      // см. реализацию ArrayTools.MergeArrayOnce()

      int[] a1 = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { 2, 4 };

      int[] res1 = ArrayTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res2, "#2");

      int[] res3 = ArrayTools.MergeArraysOnce<int>(a1, a1);
      Array.Sort<int>(res2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res2, "same array");
    }

    [Test]
    public void MergeArraysOnce_empty()
    {
      // Особый случай, когда один массив пустой
      // см. реализацию ArrayTools.MergeArrayOnce()

      int[] a1 = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { };

      int[] res1 = ArrayTools.MergeArraysOnce<int>(a1, a2);
      Array.Sort<int>(res1);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysOnce<int>(a2, a1);
      Array.Sort<int>(res2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res2, "#2");

      int[] res3 = ArrayTools.MergeArraysOnce<int>(a2, a2);
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

      int[] res1 = ArrayTools.MergeArraysBoth<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2 }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysBoth<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2 }, res2, "#2");
    }

    [Test]
    public void MergeArraysBoth_large()
    {
      // Разная реализация MergeArraysBoth(), в зависимости от длины массивов

      int[] a1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[] a2 = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

      int[] res1 = ArrayTools.MergeArraysBoth<int>(a1, a2);
      Array.Sort<int>(res1); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2, 4, 6, 8, 10 }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysBoth<int>(a2, a1);
      Array.Sort<int>(res2); // порядок не гарантируется
      Assert.AreEqual(new int[] { 2, 4, 6, 8, 10 }, res2, "#2");
    }

    [Test]
    public void MergeArraysBoth_empty()
    {
      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { };

      int[] res1 = ArrayTools.MergeArraysBoth<int>(a1, a2);
      Assert.AreEqual(new int[] { }, res1, "#1");

      int[] res2 = ArrayTools.MergeArraysBoth<int>(a2, a1);
      Assert.AreEqual(new int[] { }, res2, "#2");

      int[] res3 = ArrayTools.MergeArraysBoth<int>(a2, a2);
      Assert.AreEqual(new int[] { }, res3, "both empty");
    }

    #endregion

    #region RemoveFromArray()

    [Test]
    public void RemoveFromArray_normal()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { 2, 4 };
      ArrayTools.RemoveFromArray<int>(ref a, a2);
      Assert.AreEqual(new int[] { 1, 3, 5 }, a);
    }

    [Test]
    public void RemoveFromArray_empty_1()
    {
      int[] a = new int[] { };
      int[] a2 = new int[] { 2, 4 };
      ArrayTools.RemoveFromArray<int>(ref a, a2);
      Assert.AreEqual(new int[] { }, a);
    }

    [Test]
    public void RemoveFromArray_empty_2()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] a2 = new int[] { };
      ArrayTools.RemoveFromArray<int>(ref a, a2);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, a);
    }

    #endregion

    #region DeleteFromArray()

    [Test]
    public void DeleteFromArray_left()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      ArrayTools.DeleteFromArray<int>(ref a, 0, 2);
      Assert.AreEqual(new int[] { 3, 4, 5 }, a);
    }

    [Test]
    public void DeleteFromArray_middle()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      ArrayTools.DeleteFromArray<int>(ref a, 1, 2);
      Assert.AreEqual(new int[] { 1, 4, 5 }, a);
    }

    [Test]
    public void DeleteFromArray_right()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      ArrayTools.DeleteFromArray<int>(ref a, 3, 2);
      Assert.AreEqual(new int[] { 1, 2, 3 }, a);
    }

    [Test]
    public void DeleteFromArray_none()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      ArrayTools.DeleteFromArray<int>(ref a, 3, 0);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, a);
    }

    [Test]
    public void DeleteFromArray_all()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      ArrayTools.DeleteFromArray<int>(ref a, 0, 5);
      Assert.AreEqual(new int[] { }, a);
    }

    #endregion

    #region InsertIntoArray()

    [Test]
    public void InsertIntoArray_left()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      ArrayTools.InsertIntoArray<int>(ref a, 0, b);
      Assert.AreEqual(new int[] { 6, 7, 1, 2, 3, 4, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_middle()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      ArrayTools.InsertIntoArray<int>(ref a, 2, b);
      Assert.AreEqual(new int[] { 1, 2, 6, 7, 3, 4, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_right()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      ArrayTools.InsertIntoArray<int>(ref a, 4, b);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 6, 7, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_last()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { 6, 7 };
      ArrayTools.InsertIntoArray<int>(ref a, 5, b);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7 }, a);
    }

    [Test]
    public void InsertIntoArray_none()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5 };
      int[] b = new int[] { };
      ArrayTools.InsertIntoArray<int>(ref a, 2, b);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, a);
    }

    [Test]
    public void InsertIntoArray_to_empty()
    {
      int[] a = new int[] { };
      int[] b = new int[] { 6, 7 };
      ArrayTools.InsertIntoArray<int>(ref a, 0, b);
      Assert.AreEqual(new int[] { 6, 7 }, a);
    }

    [Test]
    public void InsertIntoArray_both_empty()
    {
      int[] a = new int[] { };
      int[] b = new int[] { };
      ArrayTools.InsertIntoArray<int>(ref a, 0, b);
      Assert.AreEqual(new int[] { }, a);
    }

    #endregion

    #region FirstItem()/LastItem()

    [Test]
    public void FirstItem_normal()
    {
      int[] a = new int[] { 1, 2, 3 };
      Assert.AreEqual(1, ArrayTools.FirstItem<int>(a));
    }

    [Test]
    public void FirstItem_empty()
    {
      int[] a = new int[] { };
      Assert.AreEqual(0, ArrayTools.FirstItem<int>(a));
    }

    [Test]
    public void LastItem_normal()
    {
      int[] a = new int[] { 1, 2, 3 };
      Assert.AreEqual(3, ArrayTools.LastItem<int>(a));
    }

    [Test]
    public void LastItem_empty()
    {
      int[] a = new int[] { };
      Assert.AreEqual(0, ArrayTools.LastItem<int>(a));
    }

    #endregion

    #region ArrayStartsWith() / ArrayEndsWith()

    [Test]
    public void ArrayStartsWith_yes()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsTrue(ArrayTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_length()
    {
      int[] a1 = new int[] { 1, 2 };
      int[] a2 = new int[] { 1, 2, 3 };

      Assert.IsFalse(ArrayTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_value()
    {
      int[] a1 = new int[] { 1, 4, 3 };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsFalse(ArrayTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_empty1()
    {
      int[] a1 = new int[] { };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsFalse(ArrayTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayStartsWith_empty2()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { };

      Assert.IsTrue(ArrayTools.ArrayStartsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_yes()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { 2, 3 };

      Assert.IsTrue(ArrayTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_length()
    {
      int[] a1 = new int[] { 2, 3 };
      int[] a2 = new int[] { 1, 2, 3 };

      Assert.IsFalse(ArrayTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_value()
    {
      int[] a1 = new int[] { 1, 4, 3 };
      int[] a2 = new int[] { 2, 3 };

      Assert.IsFalse(ArrayTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_empty1()
    {
      int[] a1 = new int[] { };
      int[] a2 = new int[] { 1, 2 };

      Assert.IsFalse(ArrayTools.ArrayEndsWith<int>(a1, a2));
    }

    [Test]
    public void ArrayEndsWith_empty2()
    {
      int[] a1 = new int[] { 1, 2, 3 };
      int[] a2 = new int[] { };

      Assert.IsTrue(ArrayTools.ArrayEndsWith<int>(a1, a2));
    }

    #endregion

    #region GetMatrixRow(), GetMatrixColumn()

    [Test]
    public void GetMatrixRow()
    {
      int[,] a = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
      int[] res = ArrayTools.GetMatrixRow<int>(a, 1);
      Assert.AreEqual(new int[] { 4, 5, 6 }, res);
    }

    [Test]
    public void GetMatrixColumn()
    {
      int[,] a = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
      int[] res = ArrayTools.GetMatrixColumn<int>(a, 1);
      Assert.AreEqual(new int[] { 2, 5, 8 }, res);
    }

    #endregion

    #region ToPlainArray()

    [Test]
    public void ToPlainArray_from_2d()
    {
      int[,] a = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
      int[] res = ArrayTools.ToPlainArray<int>(a);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6 }, res);
    }

    [Test]
    public void ToPlainArray_from_jagged_1d()
    {
      int[][] a = new int[][] { new int[] { 1, 2, 3 }, null, new int[] { 4, 5 } };
      int[] res = ArrayTools.ToPlainArray<int>(a);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, res);
    }

    [Test]
    public void ToPlainArray_from_jagged_2d()
    {
      int[,][] a = new int[2, 2][] { { new int[] { 1, 2, 3 }, new int[] { 4, 5 } }, { null, new int[] { 6 } } };
      int[] res = ArrayTools.ToPlainArray<int>(a);
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6 }, res);
    }

    #endregion

    #region ToArrayFromRows/Columns()

    [Test]
    public void MatrixFromRows()
    {
      int[,] res = ArrayTools.MatrixFromRows<int>(
        new int[] { 1, 2 },
        null,
        new int[] { 3, 4, 5, 6 });

      Assert.AreEqual(new int[3, 4] { { 1, 2, 0, 0 }, { 0, 0, 0, 0 }, { 3, 4, 5, 6 } }, res);
    }


    [Test]
    public void MatrixFromRows_None()
    {
      int[,] res = ArrayTools.MatrixFromRows<int>();

      Assert.AreEqual(new int[0, 0], res);
    }


    [Test]
    public void MatrixFromColumns()
    {
      int[,] res = ArrayTools.MatrixFromColumns<int>(
        new int[] { 1, 2 },
        null,
        new int[] { 3, 4, 5, 6 });

      Assert.AreEqual(new int[4, 3] { { 1, 0, 3 }, { 2, 0, 4 }, { 0, 0, 5 }, { 0, 0, 6 } }, res);
    }

    [Test]
    public void MatrixFromColumns_None()
    {
      int[,] res = ArrayTools.MatrixFromColumns<int>();

      Assert.AreEqual(new int[0, 0], res);
    }

    #endregion

    #region GetBlockedArray()

    [Test]
    public void GetBlockedArray()
    {
      int[] a = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[][] res = ArrayTools.GetBlockedArray<int>(a, 3);
      int[][] wanted = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, new int[] { 10 } };
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region GetArrayNotNullCount()/GetArrayNullCount()

    [Test]
    public void GetArrayNullNotNullCount_1d()
    {
      string[] a = new string[] { "AAA", null, null, "BBB", "CCC", "DDD" };
      Assert.AreEqual(4, ArrayTools.GetArrayNotNullCount(a, false), "GetArrayNotNullCount()");
      Assert.AreEqual(2, ArrayTools.GetArrayNullCount(a, false), "GetArrayNotNullCount()");
    }


    [Test]
    public void GetArrayNullNotNullCount_2d()
    {
      string[,] a = new string[,] { { "AAA", null }, { null, "BBB" }, { "CCC", "DDD" } };
      Assert.AreEqual(4, ArrayTools.GetArrayNotNullCount(a, false), "GetArrayNotNullCount()");
      Assert.AreEqual(2, ArrayTools.GetArrayNullCount(a, false), "GetArrayNotNullCount()");
    }

    [Test]
    public void GetArrayNullNotNullCount_jagged()
    {
      string[][] a = new string[][] { new string[] { "AAA", null }, new string[] { null, "BBB" }, new string[] { "CCC", "DDD" } };
      Assert.AreEqual(4, ArrayTools.GetArrayNotNullCount(a, true), "GetArrayNotNullCount()");
      Assert.AreEqual(2, ArrayTools.GetArrayNullCount(a, true), "GetArrayNotNullCount()");
    }

    #endregion

    #region CopyToArray()

    [Test]
    public void CopyToArray_ok()
    {
      List<int> lst = new List<int>();
      lst.Add(1);
      lst.Add(2);
      lst.Add(3);
      IEnumerable source = lst;

      float[] array = new float[6];

      ArrayTools.CopyToArray(source, array, 2);

      Assert.AreEqual(new float[] { 0f, 0f, 1f, 2f, 3f, 0 }, array);
    }

    [Test]
    public void CopyToArray_zero_length()
    {
      List<int> lst = new List<int>();
      IEnumerable source = lst;

      float[] array = new float[0];

      ArrayTools.CopyToArray(source, array, 0);

      Assert.Pass();
    }

    [Test]
    public void CopyToArray_exceptions()
    {
      List<int> lst = new List<int>();
      lst.Add(1);
      lst.Add(2);
      lst.Add(3);
      IEnumerable source = lst;

      int[] array = new int[6];

      Assert.Catch(delegate () { ArrayTools.CopyToArray(null, array, 0); }, "source is null");
      Assert.Catch(delegate () { ArrayTools.CopyToArray(source, null, 0); }, "array is null");
      Assert.Catch(delegate () { ArrayTools.CopyToArray(source, array, -1); }, "arrayIndex < 0");
      Assert.Catch(delegate () { ArrayTools.CopyToArray(source, array, 4); }, "arrayIndex is too big");
    }

    #endregion
  }
}
