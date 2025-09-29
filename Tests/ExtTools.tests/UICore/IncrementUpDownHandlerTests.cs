using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;

namespace ExtTools_tests.UICore
{
  internal class TestMinMaxSource<T> : IMinMaxSource<T>
  {
    #region Конструктор

    public TestMinMaxSource(T minimum, T maximum)
    {
      _Minimum = minimum;
      _Maximum = maximum;
    }

    #endregion

    #region Свойства

    public T Minimum { get { return _Minimum; } }
    private T _Minimum;

    public T Maximum { get { return _Maximum; } }
    private T _Maximum;

    #endregion
  }

  [TestFixture]
  public class IncrementUpDownHandlerTests
  {
    #region Create()

    [Test]
    public void Create_Int32()
    {
      TestMinMaxSource<Int32?> minmax = new TestMinMaxSource<Int32?>(-30, 30);
      IncrementUpDownHandler<Int32> res = IncrementUpDownHandler<Int32>.Create(3, minmax);
      Assert.IsInstanceOf<Int32UpDownHandler>(res, "ResultType");
      Assert.AreEqual(3, res.Increment, "Increment");
      Assert.AreEqual(-30, res.Minimum, "Minimum");
      Assert.AreEqual(30, res.Maximum, "Maximum");
    }

    [Test]
    public void Create_Single()
    {
      TestMinMaxSource<float?> minmax = new TestMinMaxSource<float?>(-2.5f, 2.5f);
      IncrementUpDownHandler<float> res = IncrementUpDownHandler<float>.Create(0.5f, minmax);
      Assert.IsInstanceOf<SingleUpDownHandler>(res, "ResultType");
      Assert.AreEqual(0.5f, res.Increment, "Increment");
      Assert.AreEqual(-2.5f, res.Minimum, "Minimum");
      Assert.AreEqual(2.5f, res.Maximum, "Maximum");
    }

    [Test]
    public void Create_Double()
    {
      TestMinMaxSource<double?> minmax = new TestMinMaxSource<double?>(-1.7, 1.5);
      IncrementUpDownHandler<double> res = IncrementUpDownHandler<double>.Create(0.1, minmax);
      Assert.IsInstanceOf<DoubleUpDownHandler>(res, "ResultType");
      Assert.AreEqual(0.1, res.Increment, "Increment");
      Assert.AreEqual(-1.7, res.Minimum, "Minimum");
      Assert.AreEqual(1.5, res.Maximum, "Maximum");
    }

    [Test]
    public void Create_Decimal()
    {
      TestMinMaxSource<decimal?> minmax = new TestMinMaxSource<decimal?>(1.25m, 3.75m);
      IncrementUpDownHandler<decimal> res = IncrementUpDownHandler<decimal>.Create(0.25m, minmax);
      Assert.IsInstanceOf<DecimalUpDownHandler>(res, "ResultType");
      Assert.AreEqual(0.25m, res.Increment, "Increment");
      Assert.AreEqual(1.25m, res.Minimum, "Minimum");
      Assert.AreEqual(3.75m, res.Maximum, "Maximum");
    }

    #endregion
  }

  [TestFixture]
  public class Int32UpDownHandlerTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      TestMinMaxSource<int?> minmax = new TestMinMaxSource<int?>(-5, 10);
      Int32UpDownHandler sut = new Int32UpDownHandler(5, minmax);
      Assert.AreEqual(5, sut.Increment, "Increment");
      Assert.AreEqual(-5, sut.Minimum, "Minimum");
      Assert.AreEqual(10, sut.Maximum, "Maximum");
    }

    #endregion

    #region GetUpDown()

    [TestCase(0, 10, 1, 5, 4, 6)]

    [TestCase(0, 10, 1, 1, 0, 2)]
    [TestCase(0, 10, 1, 0, null, 1)]
    [TestCase(0, 10, 1, -1, null, 0)]
    [TestCase(0, 10, 1, -100, null, 0)]

    [TestCase(0, 10, 1, 9, 8, 10)]
    [TestCase(0, 10, 1, 10, 9, null)]
    [TestCase(0, 10, 1, 11, 10, null)]
    [TestCase(0, 10, 1, 100, 10, null)]

    // Increment отличается от 1
    [TestCase(-12, 13, 5, -12, null, -10)]
    [TestCase(-12, 13, 5, -11, -12, -10)]
    [TestCase(-12, 13, 5, -10, -12, -5)]
    [TestCase(-12, 13, 5, -5, -10, 0)]
    [TestCase(-12, 13, 5, -4, -5, -0)]
    [TestCase(-12, 13, 5, -1, -5, 0)]
    [TestCase(-12, 13, 5, 0, -5, 5)]
    [TestCase(-12, 13, 5, 1, 0, 5)]
    [TestCase(-12, 13, 5, 4, 0, 5)]
    [TestCase(-12, 13, 5, 5, 0, 10)]
    [TestCase(-12, 13, 5, 6, 5, 10)]
    [TestCase(-12, 13, 5, 9, 5, 10)]
    [TestCase(-12, 13, 5, 10, 5, 13)]
    [TestCase(-12, 13, 5, 12, 10, 13)]
    [TestCase(-12, 13, 5, 13, 10, null)]
    public void GetUpDown(int? minimum, int? maximum, int increment, int? current, int? wantedPrev, int? wantedNext)
    {
      if (minimum.HasValue && maximum.HasValue)
      {
        if (minimum.Value > maximum.Value)
          throw new ArgumentException("Invalid minmax");
      }
      if (wantedNext.HasValue && wantedPrev.HasValue)
      {
        if (wantedPrev.Value > wantedNext.Value)
          throw new ArgumentException("Invalid wanted values");
      }

      TestMinMaxSource<int?> minmax = new TestMinMaxSource<int?>(minimum, maximum);
      Int32UpDownHandler sut = new Int32UpDownHandler(increment, minmax);

      bool hasNext, hasPrev;
      int? nextValue, prevValue;
      sut.GetUpDown(current, out hasNext, out nextValue, out hasPrev, out prevValue);

      Assert.AreEqual(wantedNext, nextValue, "NextValue");
      Assert.AreEqual(wantedPrev, prevValue, "PrevValue");
      Assert.AreEqual(wantedNext.HasValue, hasNext, "HasNext");
      Assert.AreEqual(wantedPrev.HasValue, hasPrev, "HasPrev");
    }

    #endregion
  }

  [TestFixture]
  public abstract class NumericUpDownHandlerTestsBase
  {
    #region GetUpDown()

    [TestCase(0.0, 10.0, 1.0, 5.0, 4.0, 6.0)]

    [TestCase(0.0, 10.0, 1.0, 1.0, 0.0, 2.0)]
    [TestCase(0.0, 10.0, 1.0, 0.0, null, 1.0)]
    [TestCase(0.0, 10.0, 1.0, -1.0, null, 0.0)]
    [TestCase(0.0, 10.0, 1.0, -100.0, null, 0.0)]

    [TestCase(0.0, 10.0, 1.0, 9.0, 8.0, 10.0)]
    [TestCase(0.0, 10.0, 1.0, 10.0, 9.0, null)]
    [TestCase(0.0, 10.0, 1.0, 11.0, 10.0, null)]
    [TestCase(0.0, 10.0, 1.0, 100.0, 10.0, null)]

    // Целый инкремент
    [TestCase(null, null, 100.0, -1.0, -100.0, 0.0)]
    [TestCase(null, null, 100.0, -0.0, -100.0, 100.0)]
    [TestCase(null, null, 100.0, 1.0, 0.0, 100.0)]

    [TestCase(null, null, 125.0, 200.0, 125.0, 250.0)]

    [TestCase(null, null, 0.0002, -0.0010, -0.0012, -0.0008)]

    // Нестандартный Increment
    [TestCase(-0.4, 10.0, 0.35, -0.4, null, -0.35)]
    [TestCase(-0.4, 10.0, 0.35, -0.35, -0.4, 0.0)]
    [TestCase(-0.4, 10.0, 0.35, -0.3, -0.35, 0.0)]
    [TestCase(-0.4, 10.0, 0.35, -0.2, -0.35, 0.0)]
    [TestCase(-0.4, 10.0, 0.35, 0.0, -0.35, 0.35)]
    [TestCase(-0.4, 10.0, 0.35, 0.1, 0.0, 0.35)]
    [TestCase(-0.4, 10.0, 0.35, 0.3, 0.0, 0.35)]
    [TestCase(-0.4, 10.0, 0.35, 0.35, 0.0, 0.7)]
    public void GetUpDown(double? minimum, double? maximum, double increment, double? current, double? wantedPrev, double? wantedNext)
    {
      if (minimum.HasValue && maximum.HasValue)
      {
        if (minimum.Value > maximum.Value)
          throw new ArgumentException("Invalid minmax");
      }
      if (wantedNext.HasValue && wantedPrev.HasValue)
      {
        if (wantedPrev.Value > wantedNext.Value)
          throw new ArgumentException("Invalid wanted values");
      }

      DoGetUpDown(minimum, maximum, increment, current, wantedPrev, wantedNext);
    }

    protected abstract void DoGetUpDown(double? minimum, double? maximum, double increment, double? current, double? wantedPrev, double? wantedNext);

    #endregion
  }

  public class SingleUpDownHandlerTests : NumericUpDownHandlerTestsBase
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      TestMinMaxSource<float?> minmax = new TestMinMaxSource<float?>(-1.5f, 2.5f);
      SingleUpDownHandler sut = new SingleUpDownHandler(0.5f, minmax);
      Assert.AreEqual(0.5f, sut.Increment, "Increment");
      Assert.AreEqual(-1.5f, sut.Minimum, "Minimum");
      Assert.AreEqual(2.5f, sut.Maximum, "Maximum");
    }

    #endregion

    #region GetUpDown()

    protected override void DoGetUpDown(double? minimum, double? maximum, double increment, double? current, double? wantedPrev, double? wantedNext)
    {
      float? minimum2 = ConvertArg(minimum);
      float? maximum2 = ConvertArg(maximum);
      float increment2 = (float)increment;
      float? current2 = ConvertArg(current);
      float? wantedPrev2 = ConvertArg(wantedPrev);
      float? wantedNext2 = ConvertArg(wantedNext);

      TestMinMaxSource<float?> minmax = new TestMinMaxSource<float?>(minimum2, maximum2);
      SingleUpDownHandler sut = new SingleUpDownHandler(increment2, minmax);

      bool hasNext, hasPrev;
      float? nextValue, prevValue;
      sut.GetUpDown(current2, out hasNext, out nextValue, out hasPrev, out prevValue);

      Assert.AreEqual(wantedNext2, nextValue, "NextValue");
      Assert.AreEqual(wantedPrev2, prevValue, "PrevValue");
      Assert.AreEqual(wantedNext2.HasValue, hasNext, "HasNext");
      Assert.AreEqual(wantedPrev2.HasValue, hasPrev, "HasPrev");
    }

    private static float? ConvertArg(double? v)
    {
      if (v.HasValue)
        return (float)(v.Value);
      else
        return null;
    }

    #endregion
  }

  public class DoubleUpDownHandlerTests : NumericUpDownHandlerTestsBase
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      TestMinMaxSource<double?> minmax = new TestMinMaxSource<double?>(-5, 10);
      DoubleUpDownHandler sut = new DoubleUpDownHandler(5, minmax);
      Assert.AreEqual(5, sut.Increment, "Increment");
      Assert.AreEqual(-5, sut.Minimum, "Minimum");
      Assert.AreEqual(10, sut.Maximum, "Maximum");
    }

    #endregion

    #region GetUpDown()

    protected override void DoGetUpDown(double? minimum, double? maximum, double increment, double? current, double? wantedPrev, double? wantedNext)
    {
      TestMinMaxSource<double?> minmax = new TestMinMaxSource<double?>(minimum, maximum);
      DoubleUpDownHandler sut = new DoubleUpDownHandler(increment, minmax);

      bool hasNext, hasPrev;
      double? nextValue, prevValue;
      sut.GetUpDown(current, out hasNext, out nextValue, out hasPrev, out prevValue);

      Assert.AreEqual(wantedNext, nextValue, "NextValue");
      Assert.AreEqual(wantedPrev, prevValue, "PrevValue");
      Assert.AreEqual(wantedNext.HasValue, hasNext, "HasNext");
      Assert.AreEqual(wantedPrev.HasValue, hasPrev, "HasPrev");
    }

    #endregion
  }

  public class DecimalUpDownHandlerTests : NumericUpDownHandlerTestsBase
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      TestMinMaxSource<decimal?> minmax = new TestMinMaxSource<decimal?>(-1.5m, 2.5m);
      DecimalUpDownHandler sut = new DecimalUpDownHandler(0.5m, minmax);
      Assert.AreEqual(0.5m, sut.Increment, "Increment");
      Assert.AreEqual(-1.5m, sut.Minimum, "Minimum");
      Assert.AreEqual(2.5m, sut.Maximum, "Maximum");
    }

    #endregion

    #region GetUpDown()

    protected override void DoGetUpDown(double? minimum, double? maximum, double increment, double? current, double? wantedPrev, double? wantedNext)
    {
      decimal? minimum2 = ConvertArg(minimum);
      decimal? maximum2 = ConvertArg(maximum);
      decimal increment2 = (decimal)increment;
      decimal? current2 = ConvertArg(current);
      decimal? wantedPrev2 = ConvertArg(wantedPrev);
      decimal? wantedNext2 = ConvertArg(wantedNext);

      TestMinMaxSource<decimal?> minmax = new TestMinMaxSource<decimal?>(minimum2, maximum2);
      DecimalUpDownHandler sut = new DecimalUpDownHandler(increment2, minmax);

      bool hasNext, hasPrev;
      decimal? nextValue, prevValue;
      sut.GetUpDown(current2, out hasNext, out nextValue, out hasPrev, out prevValue);

      Assert.AreEqual(wantedNext2, nextValue, "NextValue");
      Assert.AreEqual(wantedPrev2, prevValue, "PrevValue");
      Assert.AreEqual(wantedNext2.HasValue, hasNext, "HasNext");
      Assert.AreEqual(wantedPrev2.HasValue, hasPrev, "HasPrev");
    }

    private static decimal? ConvertArg(double? v)
    {
      if (v.HasValue)
        return (decimal)(v.Value);
      else
        return null;
    }

    #endregion
  }
}
