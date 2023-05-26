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

    #region CompareNumbers()

    public struct CompareNumbersTestInfo
    {
      #region Конструктор

      public CompareNumbersTestInfo(object a, object b, int wantedRes)
      {
        _A = a;
        _B = b;
        _WantedRes = wantedRes;
      }

      #endregion

      #region Свойства

      public object A { get { return _A; } }
      private readonly object _A;

      public object B { get { return _B; } }
      private readonly object _B;

      public int WantedRes { get { return _WantedRes; } }
      private readonly int _WantedRes;

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        if (_A == null)
          sb.Append("null");
        else
        {
          sb.Append(_A);
          sb.Append(" (");
          sb.Append(_A.GetType().Name);
          sb.Append(")");
        }

        sb.Append(" and ");

        if (_B == null)
          sb.Append("null");
        else
        {
          sb.Append(_B);
          sb.Append(" (");
          sb.Append(_B.GetType().Name);
          sb.Append(")");
        }

        sb.Append(" = ");
        sb.Append(_WantedRes.ToString("+0"));
        return sb.ToString();
      }

      #endregion
    }

    public CompareNumbersTestInfo[] CompareNumberTests
    {
      get
      {
        return new CompareNumbersTestInfo[] {

      #region Byte

      new CompareNumbersTestInfo((byte)0, (byte)0, 0),
      new CompareNumbersTestInfo((byte)0, (byte)255, -1),

      new CompareNumbersTestInfo((byte)0, (sbyte)0, 0),
      new CompareNumbersTestInfo((byte)127, (sbyte)0, 1),
      new CompareNumbersTestInfo((byte)0, (sbyte)127, -1),
      new CompareNumbersTestInfo((byte)255, (sbyte)127, 1),
      new CompareNumbersTestInfo((byte)128, (sbyte)-127, 1),

      new CompareNumbersTestInfo((byte)0, (short)0, 0),
      new CompareNumbersTestInfo((byte)127, (short)0, 1),
      new CompareNumbersTestInfo((byte)0, (short)32000, -1),
      new CompareNumbersTestInfo((byte)255, (short)32000, -1),
      new CompareNumbersTestInfo((byte)0, (short)-32000, 1),

      new CompareNumbersTestInfo((byte)0, (ushort)0, 0),
      new CompareNumbersTestInfo((byte)255, (ushort)255, 0),
      new CompareNumbersTestInfo((byte)255, (ushort)65535, -1),
      new CompareNumbersTestInfo((byte)255, (ushort)0, 1),

      new CompareNumbersTestInfo((byte)0, 0, 0),
      new CompareNumbersTestInfo((byte)127, 0, 1),
      new CompareNumbersTestInfo((byte)0, 1000000000, -1),
      new CompareNumbersTestInfo((byte)255, 1000000000, -1),
      new CompareNumbersTestInfo((byte)128, -1000000000, 1),

      new CompareNumbersTestInfo((byte)0, (uint)0, 0),
      new CompareNumbersTestInfo((byte)255, (uint)255, 0),
      new CompareNumbersTestInfo((byte)255, (uint)3000000000, -1),
      new CompareNumbersTestInfo((byte)255, (uint)0, 1),

      new CompareNumbersTestInfo((byte)0, 0L, 0),
      new CompareNumbersTestInfo((byte)127, 0L, 1),
      new CompareNumbersTestInfo((byte)0, 1000000000000L, -1),
      new CompareNumbersTestInfo((byte)255, 1000000000000L, -1),
      new CompareNumbersTestInfo((byte)128, -1000000000000L, 1),

      new CompareNumbersTestInfo((byte)0, (ulong)0, 0),
      new CompareNumbersTestInfo((byte)255, (ulong)255, 0),
      new CompareNumbersTestInfo((byte)255, (ulong)1000000000000, -1),
      new CompareNumbersTestInfo((byte)255, (ulong)0, 1),

      new CompareNumbersTestInfo((byte)0, 0f, 0),
      new CompareNumbersTestInfo((byte)127, 0f, 1),
      new CompareNumbersTestInfo((byte)127, 127.1f, -1),
      new CompareNumbersTestInfo((byte)0, -0.1f, 1),

      new CompareNumbersTestInfo((byte)0, 0.0, 0),
      new CompareNumbersTestInfo((byte)127, 0.0, 1),
      new CompareNumbersTestInfo((byte)127, 127.1, -1),
      new CompareNumbersTestInfo((byte)0, -0.1, 1),

      new CompareNumbersTestInfo((byte)0, 0m, 0),
      new CompareNumbersTestInfo((byte)127, 0m, 1),
      new CompareNumbersTestInfo((byte)127, 127.1m, -1),
      new CompareNumbersTestInfo((byte)0, -0.1m, 1),

      #endregion

      #region SByte

      new CompareNumbersTestInfo((sbyte)0, (sbyte)0, 0),
      new CompareNumbersTestInfo((sbyte)-128, (sbyte)127, -1),

      new CompareNumbersTestInfo((sbyte)0, (short)0, 0),
      new CompareNumbersTestInfo((sbyte)127, (short)127, 0),
      new CompareNumbersTestInfo((sbyte)-128, (short)(-128), 0),
      new CompareNumbersTestInfo((sbyte)127, (short)128, -1),
      new CompareNumbersTestInfo((sbyte)-128, (short)(-129), 1),

      new CompareNumbersTestInfo((sbyte)0, (ushort)0, 0),
      new CompareNumbersTestInfo((sbyte)127, (ushort)127, 0),
      new CompareNumbersTestInfo((sbyte)-128, (ushort)0, -1),
      new CompareNumbersTestInfo((sbyte)127, (ushort)128, -1),

      new CompareNumbersTestInfo((sbyte)0, 0, 0),
      new CompareNumbersTestInfo((sbyte)127, 127, 0),
      new CompareNumbersTestInfo((sbyte)-128, -128, 0),
      new CompareNumbersTestInfo((sbyte)127, 128, -1),
      new CompareNumbersTestInfo((sbyte)-128, -129, 1),

      new CompareNumbersTestInfo((sbyte)0, (uint)0, 0),
      new CompareNumbersTestInfo((sbyte)127, (uint)127, 0),
      new CompareNumbersTestInfo((sbyte)-128, (uint)0, -1),
      new CompareNumbersTestInfo((sbyte)127, (uint)128, -1),

      new CompareNumbersTestInfo((sbyte)0, 0L, 0),
      new CompareNumbersTestInfo((sbyte)127, 127L, 0),
      new CompareNumbersTestInfo((sbyte)-128, -128L, 0),
      new CompareNumbersTestInfo((sbyte)127, 128L, -1),
      new CompareNumbersTestInfo((sbyte)-128, -129L, 1),

      new CompareNumbersTestInfo((sbyte)0, (ulong)0, 0),
      new CompareNumbersTestInfo((sbyte)127, (ulong)127, 0),
      new CompareNumbersTestInfo((sbyte)-128, (ulong)0, -1),
      new CompareNumbersTestInfo((sbyte)127, (ulong)128, -1),

      new CompareNumbersTestInfo((sbyte)0, 0f, 0),
      new CompareNumbersTestInfo((sbyte)127, 127f, 0),
      new CompareNumbersTestInfo((sbyte)-128, -128f, 0),
      new CompareNumbersTestInfo((sbyte)127, 127.1f, -1),
      new CompareNumbersTestInfo((sbyte)-128, -128.1f, 1),

      new CompareNumbersTestInfo((sbyte)0, 0.0, 0),
      new CompareNumbersTestInfo((sbyte)127, 127.0, 0),
      new CompareNumbersTestInfo((sbyte)-128, -128.0, 0),
      new CompareNumbersTestInfo((sbyte)127, 127.1, -1),
      new CompareNumbersTestInfo((sbyte)-128, -128.1, 1),

      new CompareNumbersTestInfo((sbyte)0, 0m, 0),
      new CompareNumbersTestInfo((sbyte)127, 127m, 0),
      new CompareNumbersTestInfo((sbyte)-128, -128m, 0),
      new CompareNumbersTestInfo((sbyte)127, 127.1m, -1),
      new CompareNumbersTestInfo((sbyte)-128, -128.1m, 1),

      #endregion

      #region Int16

      new CompareNumbersTestInfo((short)0, (short)0, 0),
      new CompareNumbersTestInfo(Int16.MinValue, Int16.MaxValue, -1),

      new CompareNumbersTestInfo((short)0, (ushort)0, 0),
      new CompareNumbersTestInfo((short)32767, (ushort)32767, 0),
      new CompareNumbersTestInfo((short)-32768, (ushort)0, -1),
      new CompareNumbersTestInfo((short)32767, (ushort)32768, -1),

      new CompareNumbersTestInfo((short)0, 0, 0),
      new CompareNumbersTestInfo((short)32767, 32767, 0),
      new CompareNumbersTestInfo((short)-32768, -32768, 0),
      new CompareNumbersTestInfo((short)32767, 32768, -1),
      new CompareNumbersTestInfo((short)-32768, -32769, 1),

      new CompareNumbersTestInfo((short)0, (uint)0, 0),
      new CompareNumbersTestInfo((short)32767, (uint)32767, 0),
      new CompareNumbersTestInfo((short)-32768, (uint)0, -1),
      new CompareNumbersTestInfo((short)32767, (uint)32768, -1),

      new CompareNumbersTestInfo((short)0, 0L, 0),
      new CompareNumbersTestInfo((short)32767, 32767L, 0),
      new CompareNumbersTestInfo((short)-32768, -32768L, 0),
      new CompareNumbersTestInfo((short)32767, 32768L, -1),
      new CompareNumbersTestInfo((short)-32768, -32769L, 1),

      new CompareNumbersTestInfo((short)0, (ulong)0, 0),
      new CompareNumbersTestInfo((short)32767, (ulong)32767, 0),
      new CompareNumbersTestInfo((short)-32768, (ulong)0, -1),
      new CompareNumbersTestInfo((short)32767, (ulong)32768, -1),

      new CompareNumbersTestInfo((short)0, 0f, 0),
      new CompareNumbersTestInfo((short)32767, 32767f, 0),
      new CompareNumbersTestInfo((short)-32768, -32768f, 0),
      new CompareNumbersTestInfo((short)32767, 32768f, -1),
      new CompareNumbersTestInfo((short)-32768, -32769f, 1),

      new CompareNumbersTestInfo((short)0, 0.0, 0),
      new CompareNumbersTestInfo((short)32767, 32767.0, 0),
      new CompareNumbersTestInfo((short)-32768, -32768.0, 0),
      new CompareNumbersTestInfo((short)32767, 32767.1, -1),
      new CompareNumbersTestInfo((short)-32768, -32768.1, 1),

      new CompareNumbersTestInfo((short)0, 0m, 0),
      new CompareNumbersTestInfo((short)32767, 32767m, 0),
      new CompareNumbersTestInfo((short)-32768, -32768m, 0),
      new CompareNumbersTestInfo((short)32767, 32767.1m, -1),
      new CompareNumbersTestInfo((short)-32768, -32768.1m, 1),

      #endregion

      #region UInt16

      new CompareNumbersTestInfo((ushort)0, (ushort)0, 0),
      new CompareNumbersTestInfo((ushort)0, (ushort)65535, -1),

      new CompareNumbersTestInfo((ushort)0, 0, 0),
      new CompareNumbersTestInfo((ushort)65535, 0, 1),
      new CompareNumbersTestInfo((ushort)0, 1000000000, -1),
      new CompareNumbersTestInfo((ushort)65535, 1000000000, -1),
      new CompareNumbersTestInfo((ushort)0, -1000000000, 1),

      new CompareNumbersTestInfo((ushort)0, (uint)0, 0),
      new CompareNumbersTestInfo((ushort)65535, (uint)65535, 0),
      new CompareNumbersTestInfo((ushort)65535, (uint)3000000000, -1),
      new CompareNumbersTestInfo((ushort)65535, (uint)0, 1),

      new CompareNumbersTestInfo((ushort)0, 0L, 0),
      new CompareNumbersTestInfo((ushort)65535, 0L, 1),
      new CompareNumbersTestInfo((ushort)0, 1000000000000L, -1),
      new CompareNumbersTestInfo((ushort)65535, 1000000000000L, -1),
      new CompareNumbersTestInfo((ushort)65535, -1000000000000L, 1),

      new CompareNumbersTestInfo((ushort)0, (ulong)0, 0),
      new CompareNumbersTestInfo((ushort)65535, (ulong)65535, 0),
      new CompareNumbersTestInfo((ushort)65535, (ulong)1000000000000, -1),
      new CompareNumbersTestInfo((ushort)65535, (ulong)0, 1),

      new CompareNumbersTestInfo((ushort)0, 0f, 0),
      new CompareNumbersTestInfo((ushort)65535, 0f, 1),
      new CompareNumbersTestInfo((ushort)65535, 65536f, -1),
      new CompareNumbersTestInfo((ushort)0, -0.1f, 1),

      new CompareNumbersTestInfo((ushort)0, 0.0, 0),
      new CompareNumbersTestInfo((ushort)65535, 0.0, 1),
      new CompareNumbersTestInfo((ushort)65535, 65536.0, -1),
      new CompareNumbersTestInfo((ushort)0, -0.1, 1),

      new CompareNumbersTestInfo((ushort)0, 0m, 0),
      new CompareNumbersTestInfo((ushort)65535, 0m, 1),
      new CompareNumbersTestInfo((ushort)65335, 65535.1m, -1),
      new CompareNumbersTestInfo((ushort)0, -0.1m, 1),

      #endregion

      #region Int32

      new CompareNumbersTestInfo(0, 0, 0),
      new CompareNumbersTestInfo(Int32.MinValue, Int32.MaxValue, -1),

      new CompareNumbersTestInfo(0, (uint)0, 0),
      new CompareNumbersTestInfo(2000000000, (uint)2000000000, 0),
      new CompareNumbersTestInfo(-2000000000, (uint)0, -1),
      new CompareNumbersTestInfo(2000000000, (uint)2000000001, -1),

      new CompareNumbersTestInfo(0, 0L, 0),
      new CompareNumbersTestInfo(1000000000, 1000000000L, 0),
      new CompareNumbersTestInfo(-1000000000, -1000000000L, 0),
      new CompareNumbersTestInfo(1000000000, 1000000001L, -1),
      new CompareNumbersTestInfo(-1000000000, -1000000001L, 1),

      new CompareNumbersTestInfo(0, (ulong)0, 0),
      new CompareNumbersTestInfo(1000000000, (ulong)1000000000, 0),
      new CompareNumbersTestInfo(-1000000000, (ulong)0, -1),
      new CompareNumbersTestInfo(1000000000, (ulong)1000000001, -1),

      new CompareNumbersTestInfo(0, 0f, 0),
      new CompareNumbersTestInfo(10000, 10000f, 0),
      new CompareNumbersTestInfo(-10000, -10000f, 0),
      new CompareNumbersTestInfo(100, 100.1f, -1),
      new CompareNumbersTestInfo(-100, -100.1f, 1),

      new CompareNumbersTestInfo(0, 0.0, 0),
      new CompareNumbersTestInfo(10000, 10000.0, 0),
      new CompareNumbersTestInfo(-10000, -10000.0, 0),
      new CompareNumbersTestInfo(100, 100.1, -1),
      new CompareNumbersTestInfo(-100, -100.1, 1),

      new CompareNumbersTestInfo(0, 0m, 0),
      new CompareNumbersTestInfo(10000, 10000m, 0),
      new CompareNumbersTestInfo(-10000, -10000m, 0),
      new CompareNumbersTestInfo(100, 100.1m, -1),
      new CompareNumbersTestInfo(-100, -100.1m, 1),

      #endregion

      #region UInt32

      new CompareNumbersTestInfo((uint)0, (uint)0, 0),
      new CompareNumbersTestInfo((uint)0, UInt32.MaxValue, -1),

      new CompareNumbersTestInfo((uint)0, 0L, 0),
      new CompareNumbersTestInfo((uint)1000, 0L, 1),
      new CompareNumbersTestInfo((uint)0, 1000000000000L, -1),
      new CompareNumbersTestInfo((uint)1000, 1000000000000L, -1),
      new CompareNumbersTestInfo((uint)1000, -1000000000000L, 1),

      new CompareNumbersTestInfo((uint)0, (ulong)0, 0),
      new CompareNumbersTestInfo((uint)4000000000, (ulong)4000000000, 0),
      new CompareNumbersTestInfo((uint)4000000000, (ulong)4000000001, -1),
      new CompareNumbersTestInfo((uint)4000000000, (ulong)0, 1),

      new CompareNumbersTestInfo((uint)0, 0f, 0),
      new CompareNumbersTestInfo((uint)100, 0f, 1),
      new CompareNumbersTestInfo((uint)100, 101f, -1),
      new CompareNumbersTestInfo((uint)0, -0.1f, 1),

      new CompareNumbersTestInfo((uint)0, 0.0, 0),
      new CompareNumbersTestInfo((uint)100, 0.0, 1),
      new CompareNumbersTestInfo((uint)100, 100.1, -1),
      new CompareNumbersTestInfo((uint)0, -0.1, 1),

      new CompareNumbersTestInfo((uint)0, 0m, 0),
      new CompareNumbersTestInfo((uint)100, 0m, 1),
      new CompareNumbersTestInfo((uint)100, 100.1m, -1),
      new CompareNumbersTestInfo((uint)0, -0.1m, 1),

      #endregion

      #region Int32

      new CompareNumbersTestInfo(0L, 0L, 0),
      new CompareNumbersTestInfo(Int64.MinValue, Int64.MaxValue, -1),

      new CompareNumbersTestInfo(0L, (ulong)0, 0),
      new CompareNumbersTestInfo(1000000000L, (ulong)1000000000, 0),
      new CompareNumbersTestInfo(-1000000000L, (ulong)0, -1),
      new CompareNumbersTestInfo(1000000000L, (ulong)1000000001, -1),

      new CompareNumbersTestInfo(0L, 0f, 0),
      new CompareNumbersTestInfo(10000L, 10000f, 0),
      new CompareNumbersTestInfo(-10000L, -10000f, 0),
      new CompareNumbersTestInfo(100L, 100.1f, -1),
      new CompareNumbersTestInfo(-100L, -100.1f, 1),

      new CompareNumbersTestInfo(0L, 0.0, 0),
      new CompareNumbersTestInfo(10000L, 10000.0, 0),
      new CompareNumbersTestInfo(-10000L, -10000.0, 0),
      new CompareNumbersTestInfo(100L, 100.1, -1),
      new CompareNumbersTestInfo(-100L, -100.1, 1),

      new CompareNumbersTestInfo(0L, 0m, 0),
      new CompareNumbersTestInfo(10000L, 10000m, 0),
      new CompareNumbersTestInfo(-10000L, -10000m, 0),
      new CompareNumbersTestInfo(100L, 100.1m, -1),
      new CompareNumbersTestInfo(-100L, -100.1m, 1),

      #endregion

      #region UInt64

      new CompareNumbersTestInfo((ulong)0, (ulong)0, 0),
      new CompareNumbersTestInfo((ulong)0, UInt64.MaxValue, -1),

      new CompareNumbersTestInfo((ulong)0, 0f, 0),
      new CompareNumbersTestInfo((ulong)100, 0f, 1),
      new CompareNumbersTestInfo((ulong)100, 101f, -1),
      new CompareNumbersTestInfo((ulong)0, -0.1f, 1),

      new CompareNumbersTestInfo((ulong)0, 0.0, 0),
      new CompareNumbersTestInfo((ulong)100, 0.0, 1),
      new CompareNumbersTestInfo((ulong)100, 100.1, -1),
      new CompareNumbersTestInfo((ulong)0, -0.1, 1),

      new CompareNumbersTestInfo((ulong)0, 0m, 0),
      new CompareNumbersTestInfo((ulong)100, 0m, 1),
      new CompareNumbersTestInfo((ulong)100, 100.1m, -1),
      new CompareNumbersTestInfo((ulong)0, -0.1m, 1),

      #endregion

      #region Single

      new CompareNumbersTestInfo(0f, 0f, 0),
      new CompareNumbersTestInfo(Single.MinValue, Single.MaxValue, -1),

      new CompareNumbersTestInfo(0f, 0.0, 0),
      new CompareNumbersTestInfo(10000f, 10000.0, 0),
      new CompareNumbersTestInfo(-10000f, -10000.0, 0),
      new CompareNumbersTestInfo(100f, 100.1, -1),
      new CompareNumbersTestInfo(-100f, -100.1, 1),

      new CompareNumbersTestInfo(0f, 0m, 0),
      new CompareNumbersTestInfo(10000f, 10000m, 0),
      new CompareNumbersTestInfo(-10000f, -10000m, 0),
      new CompareNumbersTestInfo(100f, 100.1m, -1),
      new CompareNumbersTestInfo(-100f, -100.1m, 1),

      #endregion

      #region Double

      new CompareNumbersTestInfo(0.0, 0.0, 0),
      new CompareNumbersTestInfo(Double.MinValue, Double.MaxValue, -1),

      new CompareNumbersTestInfo(0.0, 0m, 0),
      new CompareNumbersTestInfo(10000.0, 10000m, 0),
      new CompareNumbersTestInfo(-10000.0, -10000m, 0),
      new CompareNumbersTestInfo(100.0, 100.1m, -1),
      new CompareNumbersTestInfo(-100.0, -100.1m, 1),

      #endregion

      #region Decimal

      new CompareNumbersTestInfo(0m, 0m, 0),
      new CompareNumbersTestInfo(Decimal.MinValue, Decimal.MaxValue, -1),

      #endregion
      // TODO: Остальные пары
    };
      }
    }

    [TestCaseSource("CompareNumberTests")]
    public void CompareNumbers(CompareNumbersTestInfo info)
    {
      int res1 = DataTools.CompareNumbers(info.A, info.B);
      Assert.AreEqual(info.WantedRes, Math.Sign(res1), "#1");

      int res2 = DataTools.CompareNumbers(info.B, info.A);
      Assert.AreEqual(-info.WantedRes, Math.Sign(res2), "#2");
    }

    #endregion

    #region Округление чисел с плавающей точкой

    [TestCase(0.0, 0, 0.0)]
    [TestCase(1.5, 0, 2.0)]
    [TestCase(2.5, 0, 3.0)]
    [TestCase(-1.5, 0, -2.0)]
    [TestCase(-2.5, 0, -3.0)]
    [TestCase(1234.56, 1, 1234.6)]
    // Нельзя использовать 1.5 в качестве базового значения.
    // Для типа Double есть проблемы с окрулением:
    // Math.Round(0.00015, 4, AlwaysFromZero)=0.0001, а не 0.0002, как ожидалось
    [TestCase(1.7, 0, 2)]
    [TestCase(1.7e-1, 1, 2e-1)]
    [TestCase(1.7e-2, 2, 2e-2)]
    [TestCase(1.7e-3, 3, 2e-3)]
    [TestCase(1.7e-4, 4, 2e-4)]
    [TestCase(1.7e-5, 5, 2e-5)]
    [TestCase(1.7e-6, 6, 2e-6)]
    [TestCase(1.7e-7, 7, 2e-7)]
    [TestCase(1.7e+1, -1, 2e+1)]
    [TestCase(1.7e+2, -2, 2e+2)]
    [TestCase(1.7e+3, -3, 2e+3)]
    [TestCase(1.7e+4, -4, 2e+4)]
    [TestCase(1.7e+5, -5, 2e+5)]
    [TestCase(1.7e+6, -6, 2e+6)]
    [TestCase(1.7e+7, -7, 2e+7)]
    // Граничные значения для аргумента decimals в методе Math.Round():
    [TestCase(1.7e-15, 15, 2e-15)]
    [TestCase(1.7e-16, 16, 2e-16)]
    [TestCase(1.7e-28, 28, 2e-28)]
    //непредставимо для типа Decimal [TestCase(1.7e-29, 29, 2e-29)]
    public void Round(double value, int decimals, double wantedRes)
    {
      double res1 = DataTools.Round(value, decimals);
      double delta = Math.Abs(wantedRes) / 1000.0;
      Assert.AreEqual(wantedRes, res1, delta, "Double");

      decimal res2 = DataTools.Round((decimal)value, decimals);
      Assert.AreEqual((decimal)wantedRes, res2, "Decimal");
    }

    [TestCase(1.4, 0, 1)]
    [TestCase(1.6, 0, 1)]
    [TestCase(-1.4, 0, -2)]
    [TestCase(-1.6, 0, -2)]
    [TestCase(1.24, 1, 1.2)]
    [TestCase(-1.24, 1, -1.3)]
    [TestCase(12.34, -1, 10.0)]
    [TestCase(-12.34, -1, -20.0)]
    [TestCase(-12345600.0, -6, -13000000.0)]
    [TestCase(2.7, 0, 2)]
    [TestCase(2.7e-1, 1, 2e-1)]
    [TestCase(2.7e-2, 2, 2e-2)]
    [TestCase(2.7e-3, 3, 2e-3)]
    [TestCase(2.7e-4, 4, 2e-4)]
    [TestCase(2.7e-5, 5, 2e-5)]
    [TestCase(2.7e-6, 6, 2e-6)]
    [TestCase(2.7e-7, 7, 2e-7)]
    [TestCase(2.7e+1, -1, 2e+1)]
    [TestCase(2.7e+2, -2, 2e+2)]
    [TestCase(2.7e+3, -3, 2e+3)]
    [TestCase(2.7e+4, -4, 2e+4)]
    [TestCase(2.7e+5, -5, 2e+5)]
    [TestCase(2.7e+6, -6, 2e+6)]
    [TestCase(2.7e+7, -7, 2e+7)]
    public void Floor(double value, int decimals, double wantedRes)
    {
      double res1 = DataTools.Floor(value, decimals);
      double delta = Math.Abs(wantedRes) / 1000.0;
      Assert.AreEqual(wantedRes, res1, delta, "Double");

      decimal res2 = DataTools.Floor((decimal)value, decimals);
      Assert.AreEqual((decimal)wantedRes, res2, "Decimal");
    }

    [TestCase(1.4, 0, 2)]
    [TestCase(1.6, 0, 2)]
    [TestCase(-1.4, 0, -1)]
    [TestCase(-1.6, 0, -1)]
    [TestCase(1.24, 1, 1.3)]
    [TestCase(-1.24, 1, -1.2)]
    [TestCase(12.34, -1, 20.0)]
    [TestCase(-12.34, -1, -10.0)]
    [TestCase(-12345600.0, -6, -12000000.0)]
    [TestCase(2.7, 0, 3)]
    [TestCase(2.7e-1, 1, 3e-1)]
    [TestCase(2.7e-2, 2, 3e-2)]
    [TestCase(2.7e-3, 3, 3e-3)]
    [TestCase(2.7e-4, 4, 3e-4)]
    [TestCase(2.7e-5, 5, 3e-5)]
    [TestCase(2.7e-6, 6, 3e-6)]
    [TestCase(2.7e-7, 7, 3e-7)]
    [TestCase(2.7e+1, -1, 3e+1)]
    [TestCase(2.7e+2, -2, 3e+2)]
    [TestCase(2.7e+3, -3, 3e+3)]
    [TestCase(2.7e+4, -4, 3e+4)]
    [TestCase(2.7e+5, -5, 3e+5)]
    [TestCase(2.7e+6, -6, 3e+6)]
    [TestCase(2.7e+7, -7, 3e+7)]
    public void Ceiling(double value, int decimals, double wantedRes)
    {
      double res1 = DataTools.Ceiling(value, decimals);
      double delta = Math.Abs(wantedRes) / 1000.0;
      Assert.AreEqual(wantedRes, res1, delta, "Double");

      decimal res2 = DataTools.Ceiling((decimal)value, decimals);
      Assert.AreEqual((decimal)wantedRes, res2, "Decimal");
    }

    [TestCase(1.4, 0, 1)]
    [TestCase(1.6, 0, 1)]
    [TestCase(-1.4, 0, -1)]
    [TestCase(-1.6, 0, -1)]
    [TestCase(1.24, 1, 1.2)]
    [TestCase(-1.24, 1, -1.2)]
    [TestCase(12.34, -1, 10.0)]
    [TestCase(-12.34, -1, -10.0)]
    [TestCase(-12345600.0, -6, -12000000.0)]
    [TestCase(2.7, 0, 2)]
    [TestCase(2.7e-1, 1, 2e-1)]
    [TestCase(2.7e-2, 2, 2e-2)]
    [TestCase(2.7e-3, 3, 2e-3)]
    [TestCase(2.7e-4, 4, 2e-4)]
    [TestCase(2.7e-5, 5, 2e-5)]
    [TestCase(2.7e-6, 6, 2e-6)]
    [TestCase(2.7e-7, 7, 2e-7)]
    [TestCase(2.7e+1, -1, 2e+1)]
    [TestCase(2.7e+2, -2, 2e+2)]
    [TestCase(2.7e+3, -3, 2e+3)]
    [TestCase(2.7e+4, -4, 2e+4)]
    [TestCase(2.7e+5, -5, 2e+5)]
    [TestCase(2.7e+6, -6, 2e+6)]
    [TestCase(2.7e+7, -7, 2e+7)]
    public void Truncate(double value, int decimals, double wantedRes)
    {
      double res1 = DataTools.Truncate(value, decimals);
      double delta = Math.Abs(wantedRes) / 1000.0;
      Assert.AreEqual(wantedRes, res1, delta, "Double");

      decimal res2 = DataTools.Truncate((decimal)value, decimals);
      Assert.AreEqual((decimal)wantedRes, res2, "Decimal");
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
      Assert.Throws(typeof(DivideByZeroException), delegate ()
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
      Assert.Throws(typeof(DivideByZeroException), delegate ()
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
    [TestCase("D20211215", "T2.0:0:0", "D20211217")]
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
    [TestCase("D20211215", "T2.0:0:0", "D20211213")]
    [TestCase("D20211215", "D20211213", "T2.0:0:0")]
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
