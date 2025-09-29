using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Core;
using FreeLibSet.Tests;
using System.Globalization;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class StdConvertTests
  {
    #region Методы преобразования одиночных значений

    #region Int32

    [TestCase(0, "0")]
    [TestCase(1234, "1234")]
    [TestCase(-1234567, "-1234567")]
    public void ToString_Int32(int value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.ToString(value));
    }

    [TestCase("1234", true, 1234)]
    [TestCase("-1234", true, -1234)]
    [TestCase("", false, 0)]
    [TestCase("3000000000", false, 0)]
    [TestCase("1234.0", false, 0)]
    public void ToInt32(string s, bool wantedIsOk, int wantedValue)
    {
      int value1;
      bool res1 = StdConvert.TryParseInt32(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        int value2 = StdConvert.ToInt32(s);
        Assert.AreEqual(wantedValue, value2, "ToInt32()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToInt32(s); }, "ToInt32()");
      }
    }

    #endregion

    #region Int64

    [TestCase(0, "0")]
    [TestCase(111222333444L, "111222333444")]
    [TestCase(-111222333444L, "-111222333444")]
    public void ToString_Int64(long value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.ToString(value));
    }

    [TestCase("123412341234", true, 123412341234L)]
    [TestCase("-999999888888777777", true, -999999888888777777L)]
    [TestCase("", false, 0)]
    [TestCase("1234.0", false, 0)]
    public void ToInt64(string s, bool wantedIsOk, long wantedValue)
    {
      long value1;
      bool res1 = StdConvert.TryParseInt64(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        long value2 = StdConvert.ToInt64(s);
        Assert.AreEqual(wantedValue, value2, "ToInt64()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToInt64(s); }, "ToInt64()");
      }
    }

    #endregion

    #region Single

    [TestCase(0, "0")]
    [TestCase(1.23f, "1.23")] // нельзя делать много разрядов, т.к. точность вывода не гарантируется
    [TestCase(-4.56f, "-4.56")]
    public void ToString_Single(float value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.ToString(value));
    }

    [TestCase("1234.56", true, 1234.56f)]
    [TestCase("-1234.56", true, -1234.56f)]
    [TestCase("", false, 0f)]
    [TestCase("1234..56", false, 0f)]
    [TestCase("1234,56", false, 0f)]
    public void ToSingle(string s, bool wantedIsOk, float wantedValue)
    {
      float value1;
      bool res1 = StdConvert.TryParseSingle(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        float value2 = StdConvert.ToSingle(s);
        Assert.AreEqual(wantedValue, value2, "ToSingle()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToSingle(s); }, "ToSingle()");
      }
    }

    #endregion

    #region Double

    [TestCase(0, "0")]
    [TestCase(1.23, "1.23")]
    [TestCase(-4.56, "-4.56")]
    public void ToString_Double(double value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.ToString(value));
    }

    [TestCase("1234.56", true, 1234.56)]
    [TestCase("-1234.56", true, -1234.56)]
    [TestCase("", false, 0.0)]
    [TestCase("1234..56", false, 0.0)]
    [TestCase("1234,56", false, 0.0)]
    public void ToDouble(string s, bool wantedIsOk, double wantedValue)
    {
      double value1;
      bool res1 = StdConvert.TryParseDouble(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        double value2 = StdConvert.ToDouble(s);
        Assert.AreEqual(wantedValue, value2, "ToDouble()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToDouble(s); }, "ToDouble()");
      }
    }

    #endregion

    #region Decimal

    // Нельзя использовать константы типа Decimal.
    // Передаем аргументы как double и выполняем преобразование

    [TestCase(0.0, "0")]
    [TestCase(1.23, "1.23")]
    [TestCase(-4.56, "-4.56")]
    public void ToString_Decimal(double value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.ToString((decimal)value));
    }

    [TestCase("1234.56", true, 1234.56)]
    [TestCase("-1234.56", true, -1234.56)]
    [TestCase("", false, 0.0)]
    [TestCase("1234..56", false, 0.0)]
    [TestCase("1234,56", false, 0.0)]
    public void ToDecimal(string s, bool wantedIsOk, double wantedValue)
    {
      decimal value1;
      bool res1 = StdConvert.TryParseDecimal(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual((decimal)wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        decimal value2 = StdConvert.ToDecimal(s);
        Assert.AreEqual((decimal)wantedValue, value2, "ToDecimal()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToDecimal(s); }, "ToDecimal()");
      }
    }

    #endregion

    #region DateTime

    [TestCase("20211223", false, "2021-12-23")]
    [TestCase("20211223", true, "2021-12-23T00:00:00")]
    public void ToString_DateTime(string sValue, bool useTime, string wanted)
    {
      DateTime value = Creators.DateTime(sValue);
      Assert.AreEqual(wanted, StdConvert.ToString(value, useTime));
    }

    [TestCase("2021-12-23", false, true, "20211223")]
    [TestCase("2021-12-23", true, true, "20211223")]
    //[TestCase("2021-12-23T00:00:00", false, true, "20211223")] Это поведение не определено
    [TestCase("2021-12-23T00:00:00", true, true, "20211223")]
    [TestCase("", false, false, "")]
    [TestCase("2021-12-32", false, false, "")]
    public void ToDateTime(string s, bool useTime, bool wantedIsOk, string sWantedValue)
    {
      DateTime wantedValue;
      if (sWantedValue.Length == 0)
        wantedValue = DateTime.MinValue;
      else
        wantedValue = Creators.DateTime(sWantedValue);

      DateTime value1;
      bool res1 = StdConvert.TryParseDateTime(s, out value1, useTime);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        DateTime value2 = StdConvert.ToDateTime(s, useTime);
        Assert.AreEqual(wantedValue, value2, "ToDateTime()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToDateTime(s, useTime); }, "ToDateTime()");
      }
    }

    #endregion

    #region TimeSpan

    [TestCase("1:2:3", "01:02:03")]
    [TestCase("1.2:3:4", "1.02:03:04")]
    public void ToString_TimeSpan(string sValue, string wanted)
    {
      TimeSpan value = TimeSpan.Parse(sValue);
      Assert.AreEqual(wanted, StdConvert.ToString(value));
    }

    [TestCase("1:2:3", true, "1:2:3")]
    [TestCase("", false, "0:0")]
    [TestCase("1:60:00", false, "0:0")]
    public void ToTimeSpan(string s, bool wantedIsOk, string sWantedValue)
    {
      TimeSpan wantedValue = TimeSpan.Parse(sWantedValue);

      TimeSpan value1;
      bool res1 = StdConvert.TryParseTimeSpan(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        TimeSpan value2 = StdConvert.ToTimeSpan(s);
        Assert.AreEqual(wantedValue, value2, "ToTimeSpan()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToTimeSpan(s); }, "ToTimeSpan()");
      }
    }

    #endregion

    #region Enum

    [TestCase(TestEnum.Zero, "Zero")]
    [TestCase(TestEnum.One, "One")]
    public void EnumToString(TestEnum value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.EnumToString<TestEnum>(value));
    }

    [TestCase("One", true, TestEnum.One)]
    [TestCase("", false, TestEnum.Zero)]
    [TestCase("Hello", false, TestEnum.Zero)]
    public void ToEnum(string s, bool wantedIsOk, TestEnum wantedValue)
    {
      TestEnum value1;
      bool res1 = StdConvert.TryParseEnum<TestEnum>(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParseEnum() result");
      Assert.AreEqual(wantedValue, value1, "TryParseEnum() value");

      if (wantedIsOk)
      {
        TestEnum value2 = StdConvert.ToEnum<TestEnum>(s);
        Assert.AreEqual(wantedValue, value2, "ToEnum()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToEnum<TestEnum>(s); }, "ToEnum()");
      }
    }

    #endregion

    #region Guid

    [TestCase("2c0dfea6832644e4ba55a994e0bedd10", "2c0dfea6-8326-44e4-ba55-a994e0bedd10")]
    public void ToString_Guid(string sValue, string wanted)
    {
      Guid value = new Guid(sValue);
      Assert.AreEqual(wanted, StdConvert.ToString(value));
    }

    [TestCase("2c0dfea6-8326-44e4-ba55-a994e0bedd10", true, "2c0dfea6832644e4ba55a994e0bedd10")]
    [TestCase("", false, "00000000000000000000000000000000")]
    [TestCase("123", false, "00000000000000000000000000000000")]
    public void ToGuid(string s, bool wantedIsOk, string sWantedValue)
    {
      Guid wantedValue = new Guid(sWantedValue);

      Guid value1;
      bool res1 = StdConvert.TryParseGuid(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        Guid value2 = StdConvert.ToGuid(s);
        Assert.AreEqual(wantedValue, value2, "ToGuid()");
      }
      else
      {
        Assert.Catch(delegate () { StdConvert.ToGuid(s); }, "ToGuid()");
      }
    }

    #endregion

    #endregion

    #region NumberFormat и DateTimeFormat

    [Test]
    public void NumberFormat()
    {
      Assert.AreEqual(".", StdConvert.NumberFormat.NumberDecimalSeparator, "NumberDecimalSeparator");
      Assert.AreEqual("", StdConvert.NumberFormat.NumberGroupSeparator, "NumberGroupSeparator");
    }

    [Test]
    public void DateTimeFormat()
    {
      Assert.AreEqual("-", StdConvert.DateTimeFormat.DateSeparator, "DateSeparator");
      Assert.AreEqual(":", StdConvert.DateTimeFormat.TimeSeparator, "TimeSeparator");
    }

    [Test]
    public void FormatProvider()
    {
      object res1 = StdConvert.FormatProvider.GetFormat(typeof(NumberFormatInfo));
      Assert.AreSame(StdConvert.NumberFormat, res1, "NumberFormat");
      object res2 = StdConvert.FormatProvider.GetFormat(typeof(DateTimeFormatInfo));
      Assert.AreSame(StdConvert.DateTimeFormat, res2, "DateTimeFormat");
    }

    #endregion

    #region ChangeType()

    [TestCase(null, null, typeof(String), "")]
    [TestCase(null, null, typeof(SByte), (sbyte)0)]
    [TestCase(null, null, typeof(Byte), (byte)0)]
    [TestCase(null, null, typeof(Int16), (short)0)]
    [TestCase(null, null, typeof(UInt16), (ushort)0)]
    [TestCase(null, null, typeof(Int32), 0)]
    [TestCase(null, null, typeof(UInt32), 0U)]
    [TestCase(null, null, typeof(Int64), 0L)]
    [TestCase(null, null, typeof(UInt64), 0UL)]
    [TestCase(null, null, typeof(Single), 0f)]
    [TestCase(null, null, typeof(Double), 0.0)]
    [TestCase(null, null, typeof(Decimal), "0")]
    [TestCase(null, null, typeof(Boolean), false)]
    [TestCase(null, null, typeof(DateTime), Creators.StrDateMinValue)]
    [TestCase(null, null, typeof(TimeSpan), Creators.StrTimeSpanZeroValue)]
    [TestCase(null, null, typeof(Guid), Creators.StrGuidEmpty)]
    [TestCase(null, null, typeof(TestEnum), TestEnum.Zero)]

    [TestCase(null, "", typeof(String), "")]
    [TestCase(null, "", typeof(SByte), (sbyte)0)]
    [TestCase(null, "", typeof(Byte), (byte)0)]
    [TestCase(null, "", typeof(Int16), (short)0)]
    [TestCase(null, "", typeof(UInt16), (ushort)0)]
    [TestCase(null, "", typeof(Int32), 0)]
    [TestCase(null, "", typeof(UInt32), 0U)]
    [TestCase(null, "", typeof(Int64), 0L)]
    [TestCase(null, "", typeof(UInt64), 0UL)]
    [TestCase(null, "", typeof(Single), 0f)]
    [TestCase(null, "", typeof(Double), 0.0)]
    [TestCase(null, "", typeof(Decimal), "0")]
    [TestCase(null, "", typeof(Boolean), false)]
    [TestCase(null, "", typeof(DateTime), Creators.StrDateMinValue)]
    [TestCase(null, "", typeof(TimeSpan), Creators.StrTimeSpanZeroValue)]
    [TestCase(null, "", typeof(Guid), Creators.StrGuidEmpty)]
    [TestCase(null, "", typeof(TestEnum), TestEnum.Zero)]

    [TestCase(null, "ABC", typeof(String), "ABC")]
    [TestCase(null, "-127", typeof(SByte), (sbyte)-127)]
    [TestCase(null, "255", typeof(Byte), (byte)255)]
    [TestCase(null, "-32768", typeof(Int16), (short)-32768)]
    [TestCase(null, "65535", typeof(UInt16), (ushort)65535)]
    [TestCase(null, "123", typeof(Int32), 123)]
    [TestCase(null, "123", typeof(UInt32), 123U)]
    [TestCase(null, "1000000000000", typeof(Int64), 1000000000000L)]
    [TestCase(null, "1000000000000", typeof(UInt64), 1000000000000UL)]
    [TestCase(null, "1.5", typeof(Single), 1.5f)]
    [TestCase(null, "2.5", typeof(Double), 2.5)]
    [TestCase(null, "3.5", typeof(Decimal), "3.5")]
    [TestCase(null, "0", typeof(Boolean), false)]
    [TestCase(null, "1", typeof(Boolean), true)]
    [TestCase(null, "False", typeof(Boolean), false)]
    [TestCase(null, "True", typeof(Boolean), true)]
    [TestCase(null, "2025-06-25", typeof(DateTime), "20250625")]
    [TestCase(null, "2025-06-25T12:34:56", typeof(DateTime), "20250625123456")]
    [TestCase(null, "12:34:56", typeof(TimeSpan), "12:34:56")]
    [TestCase(null, "{F1F9208E-11BC-4CD4-9E0D-4194EC8762DF}", typeof(Guid), "f1f9208e-11bc-4cd4-9e0d-4194ec8762df")]
    [TestCase(null, "One", typeof(TestEnum), TestEnum.One)]

    [TestCase(null, -123, typeof(String), "-123")]
    [TestCase(null, -127, typeof(SByte), (sbyte)-127)]
    [TestCase(null, 255, typeof(Byte), (byte)255)]
    [TestCase(null, -32768, typeof(Int16), (short)-32768)]
    [TestCase(null, 65535, typeof(UInt16), (ushort)65535)]
    [TestCase(null, 123, typeof(Int32), 123)]
    [TestCase(null, 123, typeof(UInt32), 123U)]
    [TestCase(null, -12345, typeof(Int64), -12345L)]
    [TestCase(null, 12345, typeof(UInt64), 12345UL)]
    [TestCase(null, 1, typeof(Single), 1f)]
    [TestCase(null, 2, typeof(Double), 2.0)]
    [TestCase(null, 3, typeof(Decimal), "3.0")]
    [TestCase(null, 0, typeof(Boolean), false)]
    [TestCase(null, 1, typeof(Boolean), true)]
    [TestCase(null, -1, typeof(Boolean), true)]
    [TestCase(null, 2, typeof(Boolean), true)]
    [TestCase(null, 0, typeof(TestEnum), TestEnum.Zero)]
    [TestCase(null, 1, typeof(TestEnum), TestEnum.One)]

    [TestCase(null, -123L, typeof(String), "-123")]
    [TestCase(null, -127L, typeof(SByte), (sbyte)-127)]
    [TestCase(null, 255L, typeof(Byte), (byte)255)]
    [TestCase(null, -32768L, typeof(Int16), (short)-32768)]
    [TestCase(null, 65535L, typeof(UInt16), (ushort)65535)]
    [TestCase(null, 123L, typeof(Int32), 123)]
    [TestCase(null, 123L, typeof(UInt32), 123U)]
    [TestCase(null, -12345L, typeof(Int64), -12345L)]
    [TestCase(null, 12345L, typeof(UInt64), 12345UL)]
    [TestCase(null, 1L, typeof(Single), 1f)]
    [TestCase(null, 2L, typeof(Double), 2.0)]
    [TestCase(null, 3L, typeof(Decimal), "3.0")]
    [TestCase(null, 0L, typeof(Boolean), false)]
    [TestCase(null, 1L, typeof(Boolean), true)]
    [TestCase(null, -1L, typeof(Boolean), true)]
    [TestCase(null, 2L, typeof(Boolean), true)]
    [TestCase(null, 0L, typeof(TestEnum), TestEnum.Zero)]
    [TestCase(null, 1L, typeof(TestEnum), TestEnum.One)]

    [TestCase(null, -1f, typeof(String), "-1")]
    [TestCase(null, 12.34f, typeof(String), "12.34")]
    [TestCase(null, -127f, typeof(SByte), (sbyte)-127)]
    [TestCase(null, 255f, typeof(Byte), (byte)255)]
    [TestCase(null, 1.5f, typeof(Byte), (byte)2)]
    [TestCase(null, 2.5f, typeof(Int16), (short)3)]
    [TestCase(null, 3.5f, typeof(UInt16), (ushort)4)]
    [TestCase(null, -1.5f, typeof(Int32), -2)]
    [TestCase(null, 123f, typeof(UInt32), 123U)]
    [TestCase(null, -2.5f, typeof(Int64), -3L)]
    [TestCase(null, 12345f, typeof(UInt64), 12345UL)]
    [TestCase(null, 1.5f, typeof(Single), 1.5f)]
    [TestCase(null, 2.5f, typeof(Double), 2.5)]
    [TestCase(null, 3.5f, typeof(Decimal), "3.5")]
    [TestCase(null, 0f, typeof(Boolean), false)]
    [TestCase(null, 1f, typeof(Boolean), true)]
    [TestCase(null, -1f, typeof(Boolean), true)]
    [TestCase(null, 2f, typeof(Boolean), true)]

    [TestCase(null, -1.0, typeof(String), "-1")]
    [TestCase(null, 12.34, typeof(String), "12.34")]
    [TestCase(null, -127.0, typeof(SByte), (sbyte)-127)]
    [TestCase(null, 255.0, typeof(Byte), (byte)255)]
    [TestCase(null, 1.5, typeof(Byte), (byte)2)]
    [TestCase(null, 2.5, typeof(Int16), (short)3)]
    [TestCase(null, 3.5, typeof(UInt16), (ushort)4)]
    [TestCase(null, -1.5, typeof(Int32), -2)]
    [TestCase(null, 123, typeof(UInt32), 123U)]
    [TestCase(null, -2.5, typeof(Int64), -3L)]
    [TestCase(null, 12345, typeof(UInt64), 12345UL)]
    [TestCase(null, 1.5, typeof(Single), 1.5f)]
    [TestCase(null, 2.5, typeof(Double), 2.5)]
    [TestCase(null, 3.5, typeof(Decimal), "3.5")]
    [TestCase(null, 0.0, typeof(Boolean), false)]
    [TestCase(null, 1.0, typeof(Boolean), true)]
    [TestCase(null, -1.5, typeof(Boolean), true)]
    [TestCase(null, 2.5, typeof(Boolean), true)]

    [TestCase(typeof(Decimal), "-1", typeof(String), "-1")]
    [TestCase(typeof(Decimal), "12.34", typeof(String), "12.34")]
    [TestCase(typeof(Decimal), "-127", typeof(SByte), (sbyte)-127)]
    [TestCase(typeof(Decimal), "255", typeof(Byte), (byte)255)]
    [TestCase(typeof(Decimal), "1.5", typeof(Byte), (byte)2)]
    [TestCase(typeof(Decimal), "2.5", typeof(Int16), (short)3)]
    [TestCase(typeof(Decimal), "3.5", typeof(UInt16), (ushort)4)]
    [TestCase(typeof(Decimal), "-1.5", typeof(Int32), -2)]
    [TestCase(typeof(Decimal), "123", typeof(UInt32), 123U)]
    [TestCase(typeof(Decimal), "-2.5", typeof(Int64), -3L)]
    [TestCase(typeof(Decimal), "12345", typeof(UInt64), 12345UL)]
    [TestCase(typeof(Decimal), "1.5", typeof(Single), 1.5f)]
    [TestCase(typeof(Decimal), "2.5", typeof(Double), 2.5)]
    [TestCase(typeof(Decimal), "3.5", typeof(Decimal), "3.5")]
    [TestCase(typeof(Decimal), "0.0", typeof(Boolean), false)]
    [TestCase(typeof(Decimal), "1.0", typeof(Boolean), true)]
    [TestCase(typeof(Decimal), "-1.5", typeof(Boolean), true)]
    [TestCase(typeof(Decimal), "2.5", typeof(Boolean), true)]

    [TestCase(null, false, typeof(String), "False")]
    [TestCase(null, true, typeof(String), "True")]
    [TestCase(null, false, typeof(SByte), (sbyte)0)]
    [TestCase(null, true, typeof(SByte), (sbyte)1)]
    [TestCase(null, false, typeof(Byte), (byte)0)]
    [TestCase(null, true, typeof(Byte), (byte)1)]
    [TestCase(null, false, typeof(Int16), (short)0)]
    [TestCase(null, true, typeof(Int16), (short)1)]
    [TestCase(null, false, typeof(UInt16), (ushort)0)]
    [TestCase(null, true, typeof(UInt16), (ushort)1)]
    [TestCase(null, false, typeof(Int32), 0)]
    [TestCase(null, true, typeof(Int32), 1)]
    [TestCase(null, false, typeof(UInt32), 0U)]
    [TestCase(null, true, typeof(UInt32), 1U)]
    [TestCase(null, false, typeof(Int64), 0L)]
    [TestCase(null, true, typeof(Int64), 1L)]
    [TestCase(null, false, typeof(UInt64), 0UL)]
    [TestCase(null, true, typeof(UInt64), 1UL)]
    [TestCase(null, false, typeof(Single), 0f)]
    [TestCase(null, true, typeof(Single), 1f)]
    [TestCase(null, false, typeof(Double), 0.0)]
    [TestCase(null, true, typeof(Double), 1.0)]
    [TestCase(null, false, typeof(Decimal), "0.0")]
    [TestCase(null, true, typeof(Decimal), "1.0")]
    [TestCase(null, false, typeof(Boolean), false)]
    [TestCase(null, true, typeof(Boolean), true)]

    [TestCase(typeof(DateTime), "20250625", typeof(String), "2025-06-25T00:00:00")]
    [TestCase(typeof(DateTime), "20250625123456", typeof(String), "2025-06-25T12:34:56")]
    [TestCase(typeof(DateTime), "20250625", typeof(DateTime), "20250625")]

    [TestCase(typeof(TimeSpan), "12:34:56", typeof(String), "12:34:56")]
    [TestCase(typeof(TimeSpan), "12:34:56", typeof(TimeSpan), "12:34:56")]

    [TestCase(typeof(Guid), "{B5FE772B-A2B5-4A44-99C7-F0234636FC2E}", typeof(String), "b5fe772b-a2b5-4a44-99c7-f0234636fc2e")]
    [TestCase(typeof(Guid), "{B5FE772B-A2B5-4A44-99C7-F0234636FC2E}", typeof(Guid), "b5fe772b-a2b5-4a44-99c7-f0234636fc2e")]
    [TestCase(typeof(Guid), "{B5FE772B-A2B5-4A44-99C7-F0234636FC2E}", typeof(byte[]), "2b77feb5b5a2444a99c7f0234636fc2e")]
    [TestCase(typeof(byte[]), "2b77feb5b5a2444a99c7f0234636fc2e", typeof(Guid), "{B5FE772B-A2B5-4A44-99C7-F0234636FC2E}")]

    [TestCase(null, TestEnum.Zero, typeof(String), "Zero")]
    [TestCase(null, TestEnum.One, typeof(String), "One")]
    [TestCase(null, TestEnum.One, typeof(SByte), (sbyte)1)]
    [TestCase(null, TestEnum.One, typeof(Byte), (byte)1)]
    [TestCase(null, TestEnum.One, typeof(Int16), (short)1)]
    [TestCase(null, TestEnum.One, typeof(UInt16), (ushort)1)]
    [TestCase(null, TestEnum.One, typeof(Int32), 1)]
    [TestCase(null, TestEnum.One, typeof(UInt32), 1U)]
    [TestCase(null, TestEnum.One, typeof(Int64), 1L)]
    [TestCase(null, TestEnum.One, typeof(UInt64), 1UL)]
    [TestCase(null, TestEnum.One, typeof(Single), 1f)]
    [TestCase(null, TestEnum.One, typeof(Double), 1.0)]
    [TestCase(null, TestEnum.One, typeof(Decimal), "1.0")]
    [TestCase(null, TestEnum.One, typeof(TestEnum), TestEnum.One)]
    public void ChangeType(Type valueType, object value, Type conversionType, object wantedRes)
    {
      value = CorrectChangeType(value, valueType);
      wantedRes = CorrectChangeType(wantedRes, conversionType);

      object res = StdConvert.ChangeType(value, conversionType);

      Assert.AreEqual(wantedRes, res);
    }

    private static object CorrectChangeType(object value, Type valueType)
    {
      if (valueType == null)
        return value;
      if (valueType == typeof(decimal))
        return StdConvert.ToDecimal((string)value);
      if (valueType == typeof(DateTime))
        return Creators.NDateTime((string)value);
      if (valueType == typeof(TimeSpan))
        return Creators.NTimeSpan((string)value);
      if (valueType == typeof(Guid))
        return new Guid((string)value);
      if (valueType == typeof(byte[]))
        return StringTools.HexToBytes((string)value);

      return value;
    }


    #endregion

    #region Методы преобразования значений через запятую

    #region Int32

    [Test]
    public void ToString_Int32Array_array()
    {
      Assert.AreEqual("1,2", StdConvert.ToString(new int[] { 1, 2 }));
    }

    [Test]
    public void ToString_Int32Array_null()
    {
      Assert.AreEqual("", StdConvert.ToString((int[])null));
    }

    [Test]
    public void ToInt32Array_ok()
    {
      string s = "1,2,3";
      int[] wantedValue = new int[] { 1, 2, 3 };

      int[] value1;
      bool res1 = StdConvert.TryParseInt32Array(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      int[] value2 = StdConvert.ToInt32Array(s);
      Assert.AreEqual(wantedValue, value2, "ToInt32Array()");
    }

    [Test]
    public void ToInt32Array_empty()
    {
      string s = "";
      int[] wantedValue = new int[0];

      int[] value1;
      bool res1 = StdConvert.TryParseInt32Array(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      int[] value2 = StdConvert.ToInt32Array(s);
      Assert.AreEqual(wantedValue, value2, "ToInt32Array()");
    }

    [TestCase("1.2,3")]
    [TestCase("1,")]
    [TestCase("1,,2")]
    [TestCase("1,111222333444")]
    public void ToInt32Array_error(string s)
    {
      int[] value1;
      bool res1 = StdConvert.TryParseInt32Array(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToInt32Array(s); }, "ToInt32Array()");
    }

    #endregion

    #region Int64

    [Test]
    public void ToString_Int64Array_array()
    {
      Assert.AreEqual("1,222333444555", StdConvert.ToString(new long[] { 1L, 222333444555L }));
    }

    [Test]
    public void ToString_Int64Array_null()
    {
      Assert.AreEqual("", StdConvert.ToString((long[])null));
    }

    [Test]
    public void ToInt64Array_ok()
    {
      string s = "1,2,3";
      long[] wantedValue = new long[] { 1L, 2L, 3L };

      long[] value1;
      bool res1 = StdConvert.TryParseInt64Array(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      long[] value2 = StdConvert.ToInt64Array(s);
      Assert.AreEqual(wantedValue, value2, "ToInt64Array()");
    }

    [Test]
    public void ToInt64Array_empty()
    {
      string s = "";
      long[] wantedValue = new long[0];

      long[] value1;
      bool res1 = StdConvert.TryParseInt64Array(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      long[] value2 = StdConvert.ToInt64Array(s);
      Assert.AreEqual(wantedValue, value2, "ToInt64Array()");
    }

    [TestCase("1.2,3")]
    [TestCase("1,")]
    [TestCase("1,,2")]
    public void ToInt64Array_error(string s)
    {
      long[] value1;
      bool res1 = StdConvert.TryParseInt64Array(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToInt64Array(s); }, "ToInt64Array()");
    }

    #endregion

    #region Single

    [Test]
    public void ToString_SingleArray_array()
    {
      Assert.AreEqual("1.1,-2.2", StdConvert.ToString(new float[] { 1.1f, -2.2f }));
    }

    [Test]
    public void ToString_SingleArray_null()
    {
      Assert.AreEqual("", StdConvert.ToString((float[])null));
    }

    [Test]
    public void ToSingleArray_ok()
    {
      string s = "1.1,-2.2,3";
      float[] wantedValue = new float[] { 1.1f, -2.2f, 3f };

      float[] value1;
      bool res1 = StdConvert.TryParseSingleArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      float[] value2 = StdConvert.ToSingleArray(s);
      Assert.AreEqual(wantedValue, value2, "ToSingleArray()");
    }

    [Test]
    public void ToSingleArray_empty()
    {
      string s = "";
      float[] wantedValue = new float[0];

      float[] value1;
      bool res1 = StdConvert.TryParseSingleArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      float[] value2 = StdConvert.ToSingleArray(s);
      Assert.AreEqual(wantedValue, value2, "ToSingleArray()");
    }

    [TestCase("AB,3")]
    [TestCase("1,")]
    [TestCase("1,,2")]
    public void ToSingleArray_error(string s)
    {
      float[] value1;
      bool res1 = StdConvert.TryParseSingleArray(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToSingleArray(s); }, "ToSingleArray()");
    }

    #endregion

    #region Double

    [Test]
    public void ToString_DoubleArray_array()
    {
      Assert.AreEqual("1.1,-2.2", StdConvert.ToString(new double[] { 1.1, -2.2 }));
    }

    [Test]
    public void ToString_DoubleArray_null()
    {
      Assert.AreEqual("", StdConvert.ToString((double[])null));
    }

    [Test]
    public void ToDoubleArray_ok()
    {
      string s = "1.1,-2.2,3";
      double[] wantedValue = new double[] { 1.1, -2.2, 3.0 };

      double[] value1;
      bool res1 = StdConvert.TryParseDoubleArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      double[] value2 = StdConvert.ToDoubleArray(s);
      Assert.AreEqual(wantedValue, value2, "ToDoubleArray()");
    }

    [Test]
    public void ToDoubleArray_empty()
    {
      string s = "";
      double[] wantedValue = new double[0];

      double[] value1;
      bool res1 = StdConvert.TryParseDoubleArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      double[] value2 = StdConvert.ToDoubleArray(s);
      Assert.AreEqual(wantedValue, value2, "ToDoubleArray()");
    }

    [TestCase("AB,3")]
    [TestCase("1,")]
    [TestCase("1,,2")]
    public void ToDoubleArray_error(string s)
    {
      double[] value1;
      bool res1 = StdConvert.TryParseDoubleArray(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToDoubleArray(s); }, "ToDoubleArray()");
    }

    #endregion

    #region Decimal

    [Test]
    public void ToString_DecimalArray_array()
    {
      Assert.AreEqual("1,2.3", StdConvert.ToString(new decimal[] { 1m, 2.3m }));
    }

    [Test]
    public void ToString_DecimalArray_null()
    {
      Assert.AreEqual("", StdConvert.ToString((decimal[])null));
    }

    [Test]
    public void ToDecimalArray_ok()
    {
      string s = "1.0,-2,3.3";
      decimal[] wantedValue = new decimal[] { 1m, -2m, 3.3m };

      decimal[] value1;
      bool res1 = StdConvert.TryParseDecimalArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      decimal[] value2 = StdConvert.ToDecimalArray(s);
      Assert.AreEqual(wantedValue, value2, "ToDecimalArray()");
    }

    [Test]
    public void ToDecimalArray_empty()
    {
      string s = "";
      decimal[] wantedValue = new decimal[0];

      decimal[] value1;
      bool res1 = StdConvert.TryParseDecimalArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      decimal[] value2 = StdConvert.ToDecimalArray(s);
      Assert.AreEqual(wantedValue, value2, "ToDecimalArray()");
    }

    [TestCase("XXX,3")]
    [TestCase("1,")]
    [TestCase("1,,2")]
    public void ToDecimalArray_error(string s)
    {
      decimal[] value1;
      bool res1 = StdConvert.TryParseDecimalArray(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToDecimalArray(s); }, "ToDecimalArray()");
    }

    #endregion

    #region DateTime

    [Test]
    public void ToString_DateTimeArray_array()
    {
      DateTime[] values = new DateTime[] { new DateTime(2021, 12, 23), new DateTime(2021, 12, 24, 11, 22, 33) };
      Assert.AreEqual("2021-12-23,2021-12-24", StdConvert.ToString(values, false), "useTime=false");
      Assert.AreEqual("2021-12-23T00:00:00,2021-12-24T11:22:33", StdConvert.ToString(values, true), "useTime=true");
    }

    [Test]
    public void ToString_DateTimeArray_null()
    {
      Assert.AreEqual("", StdConvert.ToString((DateTime[])null, false), "useTime=false");
      Assert.AreEqual("", StdConvert.ToString((DateTime[])null, true), "useTime=true");
    }

    [Test]
    public void ToDateTimeArray_ok_dateonly()
    {
      string s = "2021-12-23,2021-12-24";
      DateTime[] wantedValue = new DateTime[] { new DateTime(2021, 12, 23), new DateTime(2021, 12, 24) };

      DateTime[] value1;
      bool res1 = StdConvert.TryParseDateTimeArray(s, out value1, false);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      DateTime[] value2 = StdConvert.ToDateTimeArray(s, false);
      Assert.AreEqual(wantedValue, value2, "ToDateTimeArray()");
    }

    [Test]
    public void ToDateTimeArray_ok_usetime()
    {
      string s = "2021-12-23T00:00:00,2021-12-24T11:22:33";
      DateTime[] wantedValue = new DateTime[] { new DateTime(2021, 12, 23), new DateTime(2021, 12, 24, 11, 22, 33) };

      DateTime[] value1;
      bool res1 = StdConvert.TryParseDateTimeArray(s, out value1, true);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      DateTime[] value2 = StdConvert.ToDateTimeArray(s, true);
      Assert.AreEqual(wantedValue, value2, "ToDateTimeArray()");
    }

    [Test]
    public void ToDateTimeArray_empty()
    {
      string s = "";
      DateTime[] wantedValue = new DateTime[0];

      DateTime[] value1;
      bool res1 = StdConvert.TryParseDateTimeArray(s, out value1, false);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      DateTime[] value2 = StdConvert.ToDateTimeArray(s, false);
      Assert.AreEqual(wantedValue, value2, "ToDateTimeArray()");
    }

    [TestCase("2021-12-32")]
    [TestCase("1,2021-12-31")]
    [TestCase("2021-12-30,,2021-12-31")]
    public void ToDateTimeArray_error(string s)
    {
      DateTime[] value1;
      bool res1 = StdConvert.TryParseDateTimeArray(s, out value1, false);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToDateTimeArray(s, false); }, "ToDateTimeArray()");
    }

    #endregion

    #region TimeSpan

    [Test]
    public void ToString_TimeSpanArray_array()
    {
      Assert.AreEqual("11:22:33,1.22:33:44", StdConvert.ToString(new TimeSpan[] { new TimeSpan(11, 22, 33), new TimeSpan(1, 22, 33, 44) }));
    }

    [Test]
    public void ToString_TimeSpanArray_null()
    {
      Assert.AreEqual("", StdConvert.ToString((TimeSpan[])null));
    }

    [Test]
    public void ToTimeSpanArray_ok()
    {
      string s = "11:22:33,1.22:33:44";
      TimeSpan[] wantedValue = new TimeSpan[] { new TimeSpan(11, 22, 33), new TimeSpan(1, 22, 33, 44) };

      TimeSpan[] value1;
      bool res1 = StdConvert.TryParseTimeSpanArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      TimeSpan[] value2 = StdConvert.ToTimeSpanArray(s);
      Assert.AreEqual(wantedValue, value2, "ToTimeSpanArray()");
    }

    [Test]
    public void ToTimeSpanArray_empty()
    {
      string s = "";
      TimeSpan[] wantedValue = new TimeSpan[0];

      TimeSpan[] value1;
      bool res1 = StdConvert.TryParseTimeSpanArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      TimeSpan[] value2 = StdConvert.ToTimeSpanArray(s);
      Assert.AreEqual(wantedValue, value2, "ToTimeSpanArray()");
    }

    [TestCase("12:34:60")]
    [TestCase("12:34:56:78")]
    [TestCase("12:34:56,")]
    [TestCase("12:34:56,,12:34:56")]
    public void ToTimeSpanArray_error(string s)
    {
      TimeSpan[] value1;
      bool res1 = StdConvert.TryParseTimeSpanArray(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToTimeSpanArray(s); }, "ToTimeSpanArray()");
    }

    #endregion

    #region Enum

    [Test]
    public void EnumToString_Array_array()
    {
      Assert.AreEqual("One,Two", StdConvert.EnumToString<TestEnum>(new TestEnum[] { TestEnum.One, TestEnum.Two }));
    }

    [Test]
    public void EnumToString_Array_null()
    {
      Assert.AreEqual("", StdConvert.EnumToString<TestEnum>((TestEnum[])null));
    }

    [Test]
    public void ToEnumArray_ok()
    {
      string s = "One,Two";
      TestEnum[] wantedValue = new TestEnum[] { TestEnum.One, TestEnum.Two };

      TestEnum[] value1;
      bool res1 = StdConvert.TryParseEnumArray<TestEnum>(s, out value1);
      Assert.IsTrue(res1, "TryParseEnum() result");
      Assert.AreEqual(wantedValue, value1, "TryParseEnum() value");

      TestEnum[] value2 = StdConvert.ToEnumArray<TestEnum>(s);
      Assert.AreEqual(wantedValue, value2, "ToEnumArray()");
    }

    [Test]
    public void ToEnumArray_empty()
    {
      string s = "";
      TestEnum[] wantedValue = new TestEnum[0];

      TestEnum[] value1;
      bool res1 = StdConvert.TryParseEnumArray<TestEnum>(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      TestEnum[] value2 = StdConvert.ToEnumArray<TestEnum>(s);
      Assert.AreEqual(wantedValue, value2, "ToEnumArray()");
    }

    [TestCase("One,Bad")]
    [TestCase("One,")]
    [TestCase("One,,Two")]
    public void ToEnumArray_error(string s)
    {
      TestEnum[] value1;
      bool res1 = StdConvert.TryParseEnumArray<TestEnum>(s, out value1);
      Assert.IsFalse(res1, "TryParseEnum() result");
      Assert.Catch(delegate () { StdConvert.ToEnumArray<TestEnum>(s); }, "ToEnumArray()");
    }

    #endregion

    #region Guid

    [Test]
    public void ToString_GuidArray_array()
    {
      Assert.AreEqual("2c0dfea6-8326-44e4-ba55-a994e0bedd10,a4ae45d5-42cf-41ec-af62-e44155698f48",
        StdConvert.ToString(new Guid[] {
          new Guid("2c0dfea6-8326-44e4-ba55-a994e0bedd10"),
          new Guid("a4ae45d5-42cf-41ec-af62-e44155698f48") }));
    }

    [Test]
    public void ToString_GuidArray_null()
    {
      Assert.AreEqual("", StdConvert.ToString((Guid[])null));
    }

    [Test]
    public void ToGuidArray_ok()
    {
      string s = "2c0dfea6-8326-44e4-ba55-a994e0bedd10,a4ae45d5-42cf-41ec-af62-e44155698f48";
      Guid[] wantedValue = new Guid[] {
          new Guid("2c0dfea6-8326-44e4-ba55-a994e0bedd10"),
          new Guid("a4ae45d5-42cf-41ec-af62-e44155698f48") };

      Guid[] value1;
      bool res1 = StdConvert.TryParseGuidArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      Guid[] value2 = StdConvert.ToGuidArray(s);
      Assert.AreEqual(wantedValue, value2, "ToGuidArray()");
    }

    [Test]
    public void ToGuidArray_empty()
    {
      string s = "";
      Guid[] wantedValue = new Guid[0];

      Guid[] value1;
      bool res1 = StdConvert.TryParseGuidArray(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      Guid[] value2 = StdConvert.ToGuidArray(s);
      Assert.AreEqual(wantedValue, value2, "ToGuidArray()");
    }

    [TestCase("123")]
    [TestCase("2c0dfea6-8326-44e4-ba55-a994e0bedd10,")]
    [TestCase("2c0dfea6-8326-44e4-ba55-a994e0bedd10,,a4ae45d5-42cf-41ec-af62-e44155698f48")]
    public void ToGuidArray_error(string s)
    {
      Guid[] value1;
      bool res1 = StdConvert.TryParseGuidArray(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate () { StdConvert.ToGuidArray(s); }, "ToGuidArray()");
    }

    #endregion

    #endregion
  }
}
