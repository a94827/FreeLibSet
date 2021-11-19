using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
{
  /// <summary>
  /// Генератор строки результатов для проверки вывода DepValue.Value.
  /// В момент вызова добавляет к строке текущее значение. Затем, при получении ValueChanged, добавляет к строке новое значение, отделяя его вертикальной чертой "|".
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal class DepResultProducer<T>
  {
    #region Конструктор

    public DepResultProducer(DepValue<T> tested)
    {
      _Tested = tested;
      _SB = new StringBuilder();
      OutValue();

      _Tested.ValueChanged += new EventHandler(Tested_ValueChanged);
    }

    private void OutValue()
    {
      if (_Tested.Value == null)
        return;

      if (_Tested.Value is IFormattable)
      {
        IFormattable f = (IFormattable)_Tested.Value;
        _SB.Append(f.ToString(String.Empty, System.Globalization.CultureInfo.InvariantCulture));
      }
      else
        _SB.Append(_Tested.Value.ToString());
    }

    void Tested_ValueChanged(object sender, EventArgs args)
    {
      _SB.Append('|');
      OutValue();
    }

    #endregion

    #region Свойства

    private DepValue<T> _Tested;

    private StringBuilder _SB;

    /// <summary>
    /// Собранная строка, используемая для проверки результатов теста.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _SB.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Компаратор, возвращающий результат сравнения в обратном порядке
  /// </summary>
  internal class InvertedTestComparer : IComparer<int>
  {
    #region IComparer<int> Members

    public int Compare(int x, int y)
    {
      if (x > y)
        return -1;
      if (x < y)
        return 1;
      return 0;
    }

    #endregion
  }


  [TestFixture]
  public class DepComparerTests
  {
    [TestCase(2, 1, DepCompareKind.Equal, false)]
    [TestCase(2, 2, DepCompareKind.Equal, true)]
    [TestCase(2, 3, DepCompareKind.Equal, false)]

    [TestCase(2, 1, DepCompareKind.NotEqual, true)]
    [TestCase(2, 2, DepCompareKind.NotEqual, false)]
    [TestCase(2, 3, DepCompareKind.NotEqual, true)]

    [TestCase(2, 1, DepCompareKind.LessThan, false)]
    [TestCase(2, 2, DepCompareKind.LessThan, false)]
    [TestCase(2, 3, DepCompareKind.LessThan, true)]

    [TestCase(2, 1, DepCompareKind.LessOrEqualThan, false)]
    [TestCase(2, 2, DepCompareKind.LessOrEqualThan, true)]
    [TestCase(2, 3, DepCompareKind.LessOrEqualThan, true)]

    [TestCase(2, 1, DepCompareKind.GreaterThan, true)]
    [TestCase(2, 2, DepCompareKind.GreaterThan, false)]
    [TestCase(2, 3, DepCompareKind.GreaterThan, false)]

    [TestCase(2, 1, DepCompareKind.GreaterOrEqualThan, true)]
    [TestCase(2, 2, DepCompareKind.GreaterOrEqualThan, true)]
    [TestCase(2, 3, DepCompareKind.GreaterOrEqualThan, false)]
    public void ConstTest(int v1, int v2, DepCompareKind kind, bool wanted)
    {
      DepComparer<int> sut = new DepComparer<int>(new DepConst<int>(v1), v2, kind);
      Assert.AreEqual(wanted, sut.Value);
    }

    [Test]
    public void DynamicTest()
    {
      DepInput<int> v1 = new DepInput<int>(1, null);
      DepInput<int> v2 = new DepInput<int>(2, null);

      DepComparer<int> sut = new DepComparer<int>(v1, v2, DepCompareKind.LessThan);
      DepResultProducer<bool> resprod = new DepResultProducer<bool>(sut);
      Assert.AreEqual("True", resprod.ToString(), "Original");

      v1.Value = 2;
      Assert.AreEqual("True|False", resprod.ToString(), "First value changed");

      v2.Value = 3;
      Assert.AreEqual("True|False|True", resprod.ToString(), "Second value changed");
    }

    [Test]
    public void Comparer()
    {
      DepComparer<int> sut1 = new DepComparer<int>(new DepConst<int>(1), new DepConst<int>(2), DepCompareKind.LessThan);
      Assert.AreEqual(true, sut1.Value, "Default comparer");

      DepComparer<int> sut2 = new DepComparer<int>(new DepConst<int>(1), new DepConst<int>(2), DepCompareKind.LessThan, new InvertedTestComparer());
      Assert.AreEqual(false, sut2.Value, "Inverted comparer");
    }
  }
}
