using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        int value2 = StdConvert.ToInt32(s);
        Assert.AreEqual(wantedValue, value2, "ToInt32()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToInt32(s); }, "ToInt32()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        long value2 = StdConvert.ToInt64(s);
        Assert.AreEqual(wantedValue, value2, "ToInt64()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToInt64(s); }, "ToInt64()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        float value2 = StdConvert.ToSingle(s);
        Assert.AreEqual(wantedValue, value2, "ToSingle()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToSingle(s); }, "ToSingle()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        double value2 = StdConvert.ToDouble(s);
        Assert.AreEqual(wantedValue, value2, "ToDouble()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToDouble(s); }, "ToDouble()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual((decimal)wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        decimal value2 = StdConvert.ToDecimal(s);
        Assert.AreEqual((decimal)wantedValue, value2, "ToDecimal()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToDecimal(s); }, "ToDecimal()");
      }
    }

    #endregion

    #region DateTime

    [TestCase("20211223", false, "2021-12-23")]
    [TestCase("20211223", true, "2021-12-23T00:00:00")]
    public void ToString_DateTime(string sValue, bool useTime, string wanted)
    {
      DateTime value = Creators.CreateDate(sValue);
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
        wantedValue = Creators.CreateDate(sWantedValue);

      DateTime value1;
      bool res1 = StdConvert.TryParse(s, out value1, useTime);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        DateTime value2 = StdConvert.ToDateTime(s, useTime);
        Assert.AreEqual(wantedValue, value2, "ToDateTime()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToDateTime(s, useTime); }, "ToDateTime()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        TimeSpan value2 = StdConvert.ToTimeSpan(s);
        Assert.AreEqual(wantedValue, value2, "ToTimeSpan()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToTimeSpan(s); }, "ToTimeSpan()");
      }
    }

    #endregion

    #region Enum

    [TestCase(Creators.TestEnum.Zero, "Zero")]
    [TestCase(Creators.TestEnum.One, "One")]
    public void EnumToString(Creators.TestEnum value, string wanted)
    {
      Assert.AreEqual(wanted, StdConvert.EnumToString<Creators.TestEnum>(value));
    }

    [TestCase("One", true, Creators.TestEnum.One)]
    [TestCase("", false, Creators.TestEnum.Zero)]
    [TestCase("Hello", false, Creators.TestEnum.Zero)]
    public void ToEnum(string s, bool wantedIsOk, Creators.TestEnum wantedValue)
    {
      Creators.TestEnum value1;
      bool res1 = StdConvert.TryParseEnum<Creators.TestEnum>(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParseEnum() result");
      Assert.AreEqual(wantedValue, value1, "TryParseEnum() value");

      if (wantedIsOk)
      {
        Creators.TestEnum value2 = StdConvert.ToEnum<Creators.TestEnum>(s);
        Assert.AreEqual(wantedValue, value2, "ToEnum()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToEnum<Creators.TestEnum>(s); }, "ToEnum()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.AreEqual(wantedIsOk, res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      if (wantedIsOk)
      {
        Guid value2 = StdConvert.ToGuid(s);
        Assert.AreEqual(wantedValue, value2, "ToGuid()");
      }
      else
      {
        Assert.Catch(delegate() { StdConvert.ToGuid(s); }, "ToGuid()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToInt32Array(s); }, "ToInt32Array()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToInt64Array(s); }, "ToInt64Array()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToSingleArray(s); }, "ToSingleArray()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToDoubleArray(s); }, "ToDoubleArray()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToDecimalArray(s); }, "ToDecimalArray()");
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
      bool res1 = StdConvert.TryParse(s, out value1, false);
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
      bool res1 = StdConvert.TryParse(s, out value1, true);
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
      bool res1 = StdConvert.TryParse(s, out value1, false);
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
      bool res1 = StdConvert.TryParse(s, out value1, false);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToDateTimeArray(s, false); }, "ToDateTimeArray()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToTimeSpanArray(s); }, "ToTimeSpanArray()");
    }

    #endregion

    #region Enum

    [Test]
    public void EnumToString_Array_array()
    {
      Assert.AreEqual("One,Two", StdConvert.EnumToString<Creators.TestEnum>(new Creators.TestEnum[] { Creators.TestEnum.One, Creators.TestEnum.Two }));
    }

    [Test]
    public void EnumToString_Array_null()
    {
      Assert.AreEqual("", StdConvert.EnumToString<Creators.TestEnum>((Creators.TestEnum[])null));
    }

    [Test]
    public void ToEnumArray_ok()
    {
      string s = "One,Two";
      Creators.TestEnum[] wantedValue = new Creators.TestEnum[] { Creators.TestEnum.One, Creators.TestEnum.Two };

      Creators.TestEnum[] value1;
      bool res1 = StdConvert.TryParseEnum<Creators.TestEnum>(s, out value1);
      Assert.IsTrue(res1, "TryParseEnum() result");
      Assert.AreEqual(wantedValue, value1, "TryParseEnum() value");

      Creators.TestEnum[] value2 = StdConvert.ToEnumArray<Creators.TestEnum>(s);
      Assert.AreEqual(wantedValue, value2, "ToEnumArray()");
    }

    [Test]
    public void ToEnumArray_empty()
    {
      string s = "";
      Creators.TestEnum[] wantedValue = new Creators.TestEnum[0];

      Creators.TestEnum[] value1;
      bool res1 = StdConvert.TryParseEnum<Creators.TestEnum>(s, out value1);
      Assert.IsTrue(res1, "TryParse() result");
      Assert.AreEqual(wantedValue, value1, "TryParse() value");

      Creators.TestEnum[] value2 = StdConvert.ToEnumArray<Creators.TestEnum>(s);
      Assert.AreEqual(wantedValue, value2, "ToEnumArray()");
    }

    [TestCase("One,Bad")]
    [TestCase("One,")]
    [TestCase("One,,Two")]
    public void ToEnumArray_error(string s)
    {
      Creators.TestEnum[] value1;
      bool res1 = StdConvert.TryParseEnum<Creators.TestEnum>(s, out value1);
      Assert.IsFalse(res1, "TryParseEnum() result");
      Assert.Catch(delegate() { StdConvert.ToEnumArray<Creators.TestEnum>(s); }, "ToEnumArray()");
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
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
      bool res1 = StdConvert.TryParse(s, out value1);
      Assert.IsFalse(res1, "TryParse() result");
      Assert.Catch(delegate() { StdConvert.ToGuidArray(s); }, "ToGuidArray()");
    }

    #endregion

    #endregion
  }
}
