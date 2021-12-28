using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace ExtTools_tests.Core
{
  /// <summary>
  /// Тесты для перечислителей.
  /// Для каждого класса нужно по одному тесту, нет смысла делать отдельные классы
  /// </summary>
  [TestFixture]
  public class EnumerableTests
  {
    [Test]
    public void DummyEnumerable()
    {
      DummyEnumerable<int> sut = new DummyEnumerable<int>();
      int sum = 0;
      foreach (int item in sut)
        sum += item; // никогда не вызовется
      Assert.AreEqual(0, sum);
    }

    [Test]
    public void SingleObjectEnumerable()
    {
      SingleObjectEnumerable<int> sut = new SingleObjectEnumerable<int>(123);
      int sum = 0;
      foreach (int item in sut)
        sum += item; 
      Assert.AreEqual(123, sum);
    }

    [Test]
    public void EnumerableWrapper()
    {
      SingleObjectEnumerable<int>.Enumerator en = new SingleObjectEnumerable<int>.Enumerator(123);
      EnumerableWrapper<int> sut = new EnumerableWrapper<int>(en);

      int sum = 0;
      foreach (int item in sut)
        sum += item;
      Assert.AreEqual(123, sum);
    }

    [Test]
    public void EnumerableWrapper_null()
    {
      EnumerableWrapper<int> sut;
      Assert.Throws<ArgumentNullException>(delegate() { sut = new EnumerableWrapper<int>(null); });
    }

    [Test]
    public void EnumerableWrapper_secondcall()
    {
      SingleObjectEnumerable<int>.Enumerator en = new SingleObjectEnumerable<int>.Enumerator(123);
      EnumerableWrapper<int> sut = new EnumerableWrapper<int>(en);

      Assert.DoesNotThrow(delegate() { EnumerableWrapper_ForeEach_Test(ref sut); }, "#1");
      Assert.Catch(delegate() { EnumerableWrapper_ForeEach_Test(ref sut); }, "#2");
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void EnumerableWrapper_ForeEach_Test(ref EnumerableWrapper<int> sut)
    {
      // Обязательно должно передаваться по ссылке

      int sum = 0;
      foreach (int item in sut)
        sum += item;
    }


    [Test]
    public void GroupArrayEnumerable()
    {
      List<int> lst1 = new List<int>();
      lst1.Add(1);
      lst1.Add(2);
      List<int> lst2 = new List<int>();
      lst1.Add(3);
      lst1.Add(4);

      GroupArrayEnumerable<int>sut=new GroupArrayEnumerable<int>(new IEnumerable<int>[]{lst1, lst2});
      int sum = 0;
      foreach (int item in sut)
        sum += item;
      Assert.AreEqual(10, sum);
    }

    [Test]
    public void ArrayEnumerable()
    {
      int[] a = new int[] { 1, 2, 3 };

      ArrayEnumerable<int> sut = new ArrayEnumerable<int>(a);
      int sum = 0;
      foreach (int item in sut)
        sum += item;

      Assert.AreEqual(6, sum);
    }

    [Test]
    public void ArraySegmentEnumerable()
    {
      int[] a = new int[10];
      int wanted = 0;
      for (int i = 0; i < a.Length; i++)
      {
        a[i] = i + 1;
        wanted += a[i];
      }

      ArraySegmentEnumerable<int> sut = new ArraySegmentEnumerable<int>(a, 3);
      int sum = 0;
      foreach (ArraySegment<int> item in sut)
      {
        for (int j = 0; j < item.Count; j++)
          sum += item.Array[item.Offset + j];
      }

      Assert.AreEqual(wanted, sum);
    }

    [Test]
    public void ArrayBlockEnumerable()
    {
      int[] a = new int[10];
      int wanted = 0;
      for (int i = 0; i < a.Length; i++)
      {
        a[i] = i + 1;
        wanted += a[i];
      }

      ArrayBlockEnumerable<int> sut = new ArrayBlockEnumerable<int>(a, 3);
      int sum = 0;
      foreach (int[] item in sut)
      {
        for (int j = 0; j < item.Length; j++)
          sum += item[j];
      }                                                          

      Assert.AreEqual(wanted, sum);
    }

    [Test]
    public void ConvertEnumerable()
    {
      string[,] a = new string[2, 2] { { "AAA", "BBB" }, { "CCC", "DDD" } };
      System.Collections.IEnumerable en = a; // нетипизированный перечислитель

      ConvertEnumerable<string> sut = new ConvertEnumerable<string>(en);

      string sum = String.Empty;
      foreach (string item in sut)
        sum += item;

      Assert.AreEqual("AAABBBCCCDDD", sum);
    }
  }
}
