using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace ExtTools_tests.Core
{
  [TestFixture]
  class DataToolsTests_Math
  {
    #region IsXXXType()

    [TestCase(typeof(Byte), true)]
    [TestCase(typeof(SByte), true)]
    [TestCase(typeof(Int16), true)]
    [TestCase(typeof(UInt16), true)]
    [TestCase(typeof(Int32), true)]
    [TestCase(typeof(UInt32), true)]
    [TestCase(typeof(Int64), true)]
    [TestCase(typeof(UInt64), true)]
    [TestCase(typeof(Single), false)]
    [TestCase(typeof(Double), false)]
    [TestCase(typeof(Decimal), false)]
    [TestCase(typeof(Boolean), false)]
    [TestCase(typeof(String), false)]
    [TestCase(typeof(DateTime), false)]
    public void IsIntegerType(Type t, bool wanted)
    {
      Assert.AreEqual(wanted, DataTools.IsIntegerType(t));
    }

    [TestCase(typeof(Byte), false)]
    [TestCase(typeof(SByte), false)]
    [TestCase(typeof(Int16), false)]
    [TestCase(typeof(UInt16), false)]
    [TestCase(typeof(Int32), false)]
    [TestCase(typeof(UInt32), false)]
    [TestCase(typeof(Int64), false)]
    [TestCase(typeof(UInt64), false)]
    [TestCase(typeof(Single), true)]
    [TestCase(typeof(Double), true)]
    [TestCase(typeof(Decimal), true)]
    [TestCase(typeof(Boolean), false)]
    [TestCase(typeof(String), false)]
    [TestCase(typeof(DateTime), false)]
    public void IsFloatType(Type t, bool wanted)
    {
      Assert.AreEqual(wanted, DataTools.IsFloatType(t));
    }

    [TestCase(typeof(Byte), true)]
    [TestCase(typeof(SByte), true)]
    [TestCase(typeof(Int16), true)]
    [TestCase(typeof(UInt16), true)]
    [TestCase(typeof(Int32), true)]
    [TestCase(typeof(UInt32), true)]
    [TestCase(typeof(Int64), true)]
    [TestCase(typeof(UInt64), true)]
    [TestCase(typeof(Single), true)]
    [TestCase(typeof(Double), true)]
    [TestCase(typeof(Decimal), true)]
    [TestCase(typeof(Boolean), false)]
    [TestCase(typeof(String), false)]
    [TestCase(typeof(DateTime), false)]
    public void IsNumericType(Type t, bool wanted)
    {
      Assert.AreEqual(wanted, DataTools.IsNumericType(t));
    }

    #endregion

    #region DivideWithRounding

    [TestCase(3, 2, Result = 2)]
    [TestCase(-10, 3, Result = -3)]
    [TestCase(11, -3, Result = -4)]
    [TestCase(-2000000001, -1000000000, Result = 2)]
    public int DivideWithRounding_int(int x, int y)
    {
      return DataTools.DivideWithRounding(x, y);
    }

    [Test]
    //[ExpectedException(typeof(DivideByZeroException))]
    public void DivideWithRounding_int_divide_by_zero()
    {
      Assert.Throws(typeof(DivideByZeroException), delegate()
      { DataTools.DivideWithRounding(1, 0); });
    }

    [TestCase(3000000000000000000L, 2000000000000000000L, Result = 2L)]
    [TestCase(-9000000000000000002L, 3L, Result = -3000000000000000001L)]
    [TestCase(11L, -3L, Result = -4L)]
    [TestCase(-2L, -1L, Result = 2L)]
    public long DivideWithRounding_long(long x, long y)
    {
      return DataTools.DivideWithRounding(x, y);
    }

    [Test]
    //[ExpectedException(typeof(DivideByZeroException))]
    public void DivideWithRounding_long_divide_by_zero()
    {
      Assert.Throws(typeof(DivideByZeroException), delegate()
      { DataTools.DivideWithRounding(1L, 0L); });
    }

    #endregion

    #region Математические функции двух аргументов

    [TestCase(1, 2, 3.0)]
    [TestCase(1f, 2f, 3.0)]
    [TestCase(1.0, 2.0, 3.0)]
    [TestCase("M1", "M2", "M3")]
    [TestCase(1, null, 1)]
    [TestCase(1, 2f, 3f)]
    [TestCase(null, 2f, 2f)]
    [TestCase(1, 2.0, 3.0)]
    [TestCase(1f, 2.0, 3.0)]
    [TestCase(1, "M2", "M3")]
    [TestCase(1, 2L, 3L)]
    [TestCase(1f, 2L, 3f)]
    [TestCase(1L, 2.0, 3.0)]
    [TestCase(1L, "M2", "M3")]
    [TestCase("T1:2:3", null, "T1:2:3")]
    [TestCase(null, "T1:2:3", "T1:2:3")]
    [TestCase("T1:2:3", "T3:4:5", "T4:6:8")]
    [TestCase("D20211215", "T2:0:0:0", "D20211217")]
    public void SumValues(object a, object b, object wanted)
    {
      ConvertMathArg(ref a);
      ConvertMathArg(ref b);
      ConvertMathArg(ref wanted);

      object res = DataTools.SumValues(a, b);
      Assert.AreEqual(wanted, res);
    }

    [TestCase(5, 3, 2)]
    [TestCase(5L, 3L, 2L)]
    [TestCase(5f, 3f, 2f)]
    [TestCase(5.0, 3.0, 2.0)]
    [TestCase("M5", "M3", "M2")]
    [TestCase(5, null, 5)]
    [TestCase(5L, null, 5L)]
    [TestCase(5f, null, 5f)]
    [TestCase(5.0, null, 5.0)]
    [TestCase("M5", null, "M5")]
    [TestCase(5, 3L, 2L)]
    [TestCase(5f, 3, 2f)]
    [TestCase(5L, 3.0, 2.0)]
    [TestCase("M5", 3L, "M2")]
    [TestCase("T1:2:3", "T1:2:2", "T0:0:1")]
    [TestCase("D20211215", "T2:0:0:0", "D20211213")]
    [TestCase("D20211215", "D20211213", "T2:0:0:0")]
    public void SubstractValues(object a, object b, object wanted)
    {
      ConvertMathArg(ref a);
      ConvertMathArg(ref b);
      ConvertMathArg(ref wanted);

      object res = DataTools.SubstractValues(a, b);
      Assert.AreEqual(wanted, res);
    }

    [TestCase(1, -1)]
    [TestCase(1L, -1L)]
    [TestCase(-2f, 2f)]
    [TestCase(3.0, -3.0)]
    [TestCase("M-2", "M2")]
    [TestCase("T1:2:3", "T-1:2:3")]
    public void NegValue(object a, object wanted)
    {
      ConvertMathArg(ref a);
      ConvertMathArg(ref wanted);

      object res = DataTools.NegValue(a);
      Assert.AreEqual(wanted, res);
    }

    [TestCase(3, 4, 12)]
    [TestCase(3L, 4L, 12L)]
    [TestCase(3f, 4f, 12f)]
    [TestCase(3.0, 4.0, 12.0)]
    [TestCase("M3", "M4", "M12")]
    [TestCase(1, null, null)]
    [TestCase(null, 1f, null)]
    [TestCase(3, 4L, 12L)]
    [TestCase(3L, 4f, 12f)]
    [TestCase(3.0, 4, 12.0)]
    [TestCase(3, "M4", "M12")]
    [TestCase("T1:2:3", 3, "T3:6:9")]
    [TestCase("T1:2:3", 3L, "T3:6:9")]
    [TestCase("T1:2:3", 3f, "T3:6:9")]
    [TestCase("T1:2:3", 3.0, "T3:6:9")]
    [TestCase("T1:2:3", "M3", "T3:6:9")]
    [TestCase(3, "T1:2:3", "T3:6:9")]
    public void MultiplyValues(object a, object b, object wanted)
    {
      ConvertMathArg(ref a);
      ConvertMathArg(ref b);
      ConvertMathArg(ref wanted);

      object res = DataTools.MultiplyValues(a, b);
      Assert.AreEqual(wanted, res);
    }

    [TestCase(12, 4, 3)]
    [TestCase(11, 4, 2.75)]
    [TestCase(12L, 4L, 3L)]
    [TestCase(11L, 4L, 2.75)]
    [TestCase(12f, 4f, 3f)]
    [TestCase(12.0, 4.0, 3.0)]
    [TestCase("M12", "M4", "M3")]
    [TestCase(null, 4, null)]
    [TestCase("T3:6:9", 3, "T1:2:3")]
    [TestCase("T3:6:9", 3L, "T1:2:3")]
    [TestCase("T3:6:9", 3f, "T1:2:3")]
    [TestCase("T3:6:9", 3.0, "T1:2:3")]
    [TestCase("T3:6:9", "M3", "T1:2:3")]
    [TestCase("T3:6:9", "T1:2:3", 3L)] // хрупкий тест, т.к. тип данных может измениться
    public void DivideValues(object a, object b, object wanted)
    {
      ConvertMathArg(ref a);
      ConvertMathArg(ref b);
      ConvertMathArg(ref wanted);

      object res = DataTools.DivideValues(a, b);
      Assert.AreEqual(wanted, res);
    }

    [TestCase(1, 1)]
    [TestCase(-1, 1)]
    [TestCase(1L, 1L)]
    [TestCase(-1L, 1L)]
    [TestCase(2f, 2f)]
    [TestCase(-2f, 2f)]
    [TestCase(3.0, 3.0)]
    [TestCase(-3.0, 3.0)]
    [TestCase("M2", "M2")]
    [TestCase("M-2", "M2")]
    [TestCase("T1:2:3", "T1:2:3")]
    [TestCase("T-1:2:3", "T1:2:3")]
    [TestCase(null, null)]
    public void AbsValue(object a, object wanted)
    {
      ConvertMathArg(ref a);
      ConvertMathArg(ref wanted);

      object res = DataTools.AbsValue(a);
      Assert.AreEqual(wanted, res);
    }

    /// <summary>
    /// Аргументы Decimal, DateTime и TimeSpan нельзя в TestCase задавать как константы.
    /// Они задаются как строки. Тип определяется по первой букве
    /// </summary>
    /// <param name="arg"></param>
    private static void ConvertMathArg(ref object arg)
    {
      string s = arg as string;
      if (s == null)
        return;

      switch (s[0])
      {
        case 'D': arg = Creators.CreateDate(s.Substring(1)); break;
        case 'T': arg = StdConvert.ToTimeSpan(s.Substring(1)); break;
        case 'M': arg = StdConvert.ToDecimal(s.Substring(1)); break;
        default: throw new ArgumentException();
      }
    }

    #endregion

    #region IsInRange

    [TestCase(1, 2, 4, false)]
    [TestCase(2, 2, 4, true)]
    [TestCase(3, 2, 4, true)]
    [TestCase(4, 2, 4, true)]
    [TestCase(5, 2, 4, false)]
    [TestCase(1, null, 4, true)]
    [TestCase(4, null, 4, true)]
    [TestCase(5, null, 4, false)]
    [TestCase(1, 2, null, false)]
    [TestCase(2, 2, null, true)]
    [TestCase(3, 2, null, true)]
    [TestCase(1, null, null, true)]
    public void IsInRange(int testValue, int? firstValue, int? lastValue, bool wanted)
    {
      bool res = DataTools.IsInRange<int>(testValue, firstValue, lastValue);
      Assert.AreEqual(wanted, res);
    }

    #endregion
  }
}
