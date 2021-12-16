using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class MinMaxTests
  {
    [Test]
    public void Constructor_empty()
    {
      MinMax<int> sut = new MinMax<int>();

      Assert.IsFalse(sut.HasValue, "HasValue");
      Assert.IsNull(sut.NullableMinValue, "NullableMinValue");
      Assert.IsNull(sut.NullableMaxValue, "NullableMinValue");
    }

    [Test]
    public void Constructor_single()
    {
      MinMax<int> sut = new MinMax<int>(123);

      Assert.IsTrue(sut.HasValue, "HasValue");
      Assert.AreEqual(123, sut.MinValue, "MinValue");
      Assert.AreEqual(123, sut.MaxValue, "MaxValue");
      Assert.AreEqual(123, sut.NullableMinValue, "NullableMinValue");
      Assert.AreEqual(123, sut.NullableMaxValue, "NullableMaxValue");
    }

    [Test]
    public void Constructor_pair()
    {
      MinMax<int> sut = new MinMax<int>(1, 5);

      Assert.IsTrue(sut.HasValue, "HasValue");
      Assert.AreEqual(1, sut.MinValue, "MinValue");
      Assert.AreEqual(5, sut.MaxValue, "MaxValue");
      Assert.AreEqual(1, sut.NullableMinValue, "NullableMinValue");
      Assert.AreEqual(5, sut.NullableMaxValue, "NullableMaxValue");
    }

    [Test]
    public void Constructor_exception()
    {
      MinMax<int> sut;

      Assert.Catch(delegate() { sut = new MinMax<int>(3, 2); });
    }

    [Test]
    public void Constructor_enumerable()
    {
      int[] a = new int[] { 3, 1, 2, 4 };

      MinMax<int> sut = new MinMax<int>(a);

      Assert.IsTrue(sut.HasValue, "HasValue");
      Assert.AreEqual(1, sut.MinValue, "MinValue");
      Assert.AreEqual(4, sut.MaxValue, "MaxValue");
    }


    [Test]
    public void Constructor_enumerable_empty()
    {
      int[] a = new int[] { };

      MinMax<int> sut = new MinMax<int>(a);
      Assert.IsFalse(sut.HasValue, "HasValue");
    }


    [Test]
    public void operator_addvalue_empty_value()
    {
      MinMax<int> arg1 = new MinMax<int>();
      MinMax<int> res = arg1 + 1;
      Assert.AreEqual(1, res.MinValue, "MinValue");
      Assert.AreEqual(1, res.MaxValue, "MaxValue");
    }

    [TestCase(2, 4, 1, 1, 4)]
    [TestCase(2, 4, 2, 2, 4)]
    [TestCase(2, 4, 3, 2, 4)]
    [TestCase(2, 4, 4, 2, 4)]
    [TestCase(2, 4, 5, 2, 5)]
    public void operator_addvalue(int min, int max, int value, int wantedMin, int wantedMax)
    {
      MinMax<int> arg1 = new MinMax<int>(min, max);
      MinMax<int> res1 = arg1 + value;
      Assert.AreEqual(wantedMin, res1.MinValue, "MinValue-1");
      Assert.AreEqual(wantedMax, res1.MaxValue, "MaxValue-1");

      int? value2 = value;
      MinMax<int> res2 = arg1 + value2;
      Assert.AreEqual(wantedMin, res2.MinValue, "MinValue-2");
      Assert.AreEqual(wantedMax, res2.MaxValue, "MaxValue-2");
    }

    [Test]
    public void operator_addnullvalue_null()
    {
      MinMax<int> arg1 = new MinMax<int>(1);
      int? arg2 = null;
      MinMax<int> res = arg1 + arg2;
      Assert.AreEqual(1, res.MinValue, "MinValue");
      Assert.AreEqual(1, res.MaxValue, "MaxValue");
    }

    [Test]
    public void operator_addminmax_empty_empty()
    {
      MinMax<int> arg1 = new MinMax<int>();
      MinMax<int> arg2 = new MinMax<int>();
      MinMax<int> res = arg1 + arg2;
      Assert.IsFalse(res.HasValue);
    }

    [Test]
    public void operator_addminmax_empty_minmax()
    {
      MinMax<int> arg1 = new MinMax<int>();
      MinMax<int> arg2 = new MinMax<int>(1);
      MinMax<int> res = arg1 + arg2;
      Assert.AreEqual(1, res.MinValue, "MinValue");
      Assert.AreEqual(1, res.MaxValue, "MaxValue");
    }

    [Test]
    public void operator_addminmax_minmax_empty()
    {
      MinMax<int> arg1 = new MinMax<int>(1);
      MinMax<int> arg2 = new MinMax<int>();
      MinMax<int> res = arg1 + arg2;
      Assert.AreEqual(1, res.MinValue, "MinValue");
      Assert.AreEqual(1, res.MaxValue, "MaxValue");
    }

    [TestCase(1, 2, 3, 4, 1, 4)]
    [TestCase(3, 4, 1, 2, 1, 4)]
    [TestCase(1, 2, 2, 3, 1, 3)]
    [TestCase(1, 2, 1, 2, 1, 2)]
    public void operator_addminmax(int min1, int max1, int min2, int max2, int wantedMin, int wantedMax)
    {
      MinMax<int> arg1 = new MinMax<int>(min1, max1);
      MinMax<int> arg2 = new MinMax<int>(min2, max2);

      MinMax<int> res = arg1 + arg2;
      Assert.AreEqual(wantedMin, res.MinValue, "MinValue");
      Assert.AreEqual(wantedMax, res.MaxValue, "MaxValue");
    }

    [Test]
    public void ToType_notempty()
    {
      MinMax<int> sut = new MinMax<int>(2, 4);
      MinMax<double> res = sut.ToType<double>();
      Assert.AreEqual(2.0, res.MinValue, "MinValue");
      Assert.AreEqual(4.0, res.MaxValue, "MaxValue");
    }

    [Test]
    public void ToType_empty()
    {
      MinMax<int> sut = new MinMax<int>();
      MinMax<double> res = sut.ToType<double>();
      Assert.IsFalse(res.HasValue);
    }
  }
}
