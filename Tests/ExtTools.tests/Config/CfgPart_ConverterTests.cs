using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Config;
using FreeLibSet.Core;

namespace ExtTools_tests.Config
{
  [TestFixture(Description = "Using CfgConverter ansector")]
  public class CfgPart_ConverterTests
  {
    #region Тестовый конвертер

    /// <summary>
    /// Конвертер добавляет перед всеми значениями признак типа в виде "[XX]значение",
    /// где [XX] - [I4], "[I8]", "[F4]", "[F8]", "[DC]", "[BL]", "[DT]", ["TM"], "[TS]", "[GU]".
    /// В текущей реализации <see cref="CfgConverter"/> нет возможности переопределить режим хранения для перечислений.
    /// </summary>
    private class TestConverter : CfgConverter
    {
      #region Int32

      public override string ToString(int value)
      {
        return "[I4]" + StdConvert.ToString(value);
      }

      public override bool TryParse(string s, out int value)
      {
        value = 0;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value);
      }

      #endregion

      #region Int64

      public override string ToString(long value)
      {
        return "[I8]" + StdConvert.ToString(value);
      }

      public override bool TryParse(string s, out long value)
      {
        value = 0L;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value);
      }

      #endregion

      #region Single

      public override string ToString(float value)
      {
        return "[F4]" + StdConvert.ToString(value);
      }

      public override bool TryParse(string s, out float value)
      {
        value = 0f;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value);
      }

      #endregion

      #region Double

      public override string ToString(double value)
      {
        return "[F8]" + StdConvert.ToString(value);
      }

      public override bool TryParse(string s, out double value)
      {
        value = 0.0;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value);
      }

      #endregion

      #region Decimal

      public override string ToString(decimal value)
      {
        return "[DC]" + StdConvert.ToString(value);
      }

      public override bool TryParse(string s, out decimal value)
      {
        value = 0m;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value);
      }

      #endregion

      #region Boolean

      public override string ToString(bool value)
      {
        return "[BL]" + (value ? "YES" : "NO");
      }

      public override bool TryParse(string s, out bool value)
      {
        value = false;
        if (s.Length < 5)
          return false;
        switch (s)
        {
          case "[BL]YES": value = true; return true;
          case "[BL]NO": value = false; return true;
          default:
            int v;
            if (StdConvert.TryParse(s.Substring(4), out v))
            {
              value = v != 0;
              return true;
            }
            else
              return false;
        }
      }

      #endregion

      #region DateTime

      public override string ToString(DateTime value, bool useTime)
      {
        return (useTime ? "[TM]" : "[DT]") + StdConvert.ToString(value, useTime);
      }

      public override bool TryParse(string s, out DateTime value, bool useTime)
      {
        value = DateTime.MinValue;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value, useTime);
      }

      #endregion

      #region TimeSpan

      public override string ToString(TimeSpan value)
      {
        return "[TS]" + StdConvert.ToString(value);
      }

      public override bool TryParse(string s, out TimeSpan value)
      {
        value = TimeSpan.Zero;
        if (s.Length < 5)
          return false;
        return StdConvert.TryParse(s.Substring(4), out value);
      }

      #endregion

      #region Guid

      public override string ToString(Guid value)
      {
        return "[GU]" + value.ToString("N");
      }

      public override bool TryParse(string s, out Guid value)
      {
        value = Guid.Empty;
        if (s.Length < 5)
          return false;
        try
        {
          value = new Guid(s.Substring(4));
          return true;
        }
        catch { return false; }
      }

      #endregion
    }

    private CfgPart Create()
    {
      TestConverter converter = new TestConverter();
      TempCfg sut = new TempCfg(converter);
      return sut;
    }

    #endregion

    #region Одиночные значения

    #region Int32

    [Test]
    public void SetInt()
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", 123);
      Assert.AreEqual("[I4]123", sut.GetString("ABC"));
    }

    [Test]
    public void GetInt()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[I4]12345");
      Assert.AreEqual(12345, sut.GetInt("ABC"));
    }

    #endregion

    #region Int64

    [Test]
    public void SetInt64()
    {
      CfgPart sut = Create();
      sut.SetInt64("ABC", -123L);
      Assert.AreEqual("[I8]-123", sut.GetString("ABC"));
    }

    [Test]
    public void GetInt64()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[I4]-12345");
      Assert.AreEqual(-12345L, sut.GetInt64("ABC"));
    }

    #endregion

    #region Single

    [Test]
    public void SetSingle()
    {
      CfgPart sut = Create();
      sut.SetSingle("ABC", 1.2f);
      Assert.AreEqual("[F4]1.2", sut.GetString("ABC"));
    }

    [Test]
    public void GetSingle()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[F4]1.2");
      Assert.AreEqual(1.2f, sut.GetSingle("ABC"));
    }

    #endregion

    #region Double

    [Test]
    public void SetDouble()
    {
      CfgPart sut = Create();
      sut.SetDouble("ABC", 1.25);
      Assert.AreEqual("[F8]1.25", sut.GetString("ABC"));
    }

    [Test]
    public void GetDouble()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[F8]1.25");
      Assert.AreEqual(1.25, sut.GetDouble("ABC"));
    }

    #endregion

    #region Decimal

    [Test]
    public void SetDecimal()
    {
      CfgPart sut = Create();
      sut.SetDecimal("ABC", 1.2345m);
      Assert.AreEqual("[DC]1.2345", sut.GetString("ABC"));
    }

    [Test]
    public void GetDecimal()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[DC]1.2345");
      Assert.AreEqual(1.2345m, sut.GetDecimal("ABC"));
    }

    #endregion

    #region Boolean

    [Test]
    public void SetBool()
    {
      CfgPart sut = Create();
      sut.SetBool("ABC", false);
      Assert.AreEqual("[BL]NO", sut.GetString("ABC"));
    }

    [Test]
    public void GetBool()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[BL]YES");
      Assert.AreEqual(true, sut.GetBool("ABC"));
    }

    #endregion

    #region Date

    [Test]
    public void SetDate()
    {
      CfgPart sut = Create();
      sut.SetDate("ABC", new DateTime(2023, 6, 19));
      Assert.AreEqual("[DT]2023-06-19", sut.GetString("ABC"));
    }

    [Test]
    public void GetDate()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[DT]2023-06-19");
      Assert.AreEqual(new DateTime(2023, 6, 19), sut.GetDate("ABC"));
    }

    #endregion

    #region DateTime

    [Test]
    public void SetDateTime()
    {
      CfgPart sut = Create();
      sut.SetDateTime("ABC", new DateTime(2023, 6, 19, 12, 34, 56));
      Assert.AreEqual("[TM]2023-06-19T12:34:56", sut.GetString("ABC"));
    }

    [Test]
    public void GetDateTime()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[TM]2023-06-19T12:34:56");
      Assert.AreEqual(new DateTime(2023, 6, 19, 12, 34, 56), sut.GetDateTime("ABC"));
    }

    #endregion

    #region TimeSpan

    [Test]
    public void SetTimeSpan()
    {
      CfgPart sut = Create();
      sut.SetTimeSpan("ABC", new TimeSpan(12, 34, 56));
      Assert.AreEqual("[TS]12:34:56", sut.GetString("ABC"));
    }

    [Test]
    public void GetTimeSpan()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[TS]12:34:56");
      Assert.AreEqual(new TimeSpan(12, 34, 56), sut.GetTimeSpan("ABC"));
    }

    #endregion

    #region Guid

    const string TestGuid1 = "6a04b7257da24a21ab91f5402102c6f3";

    [Test]
    public void SetGuid()
    {
      CfgPart sut = Create();
      sut.SetGuid("ABC", new Guid(TestGuid1));
      Assert.AreEqual("[GU]"+TestGuid1, sut.GetString("ABC"));
    }

    [Test]
    public void GetGuid()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "[GU]"+TestGuid1);
      Assert.AreEqual(new Guid(TestGuid1), sut.GetGuid("ABC"));
    }

    #endregion

    #endregion

    #region Nullablle-значения

    #region Int32

    [Test]
    public void SetNullableInt()
    {
      CfgPart sut = Create();
      sut.SetNullableInt("ABC", 123);
      Assert.AreEqual("[I4]123", sut.GetString("ABC"), "#1");

      sut.SetNullableInt("ABC", null);
      Assert.IsFalse(sut.HasValue("ABC"), "#2");
    }

    [Test]
    public void GetNullableInt()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "");
      Assert.IsNull(sut.GetNullableInt("ABC"), "#1");

      sut.SetString("ABC", "[I4]123");
      Assert.AreEqual(123, sut.GetNullableInt("ABC"), "#2");
    }

    #endregion

    #region Int64

    [Test]
    public void SetNullableInt64()
    {
      CfgPart sut = Create();
      sut.SetNullableInt64("ABC", 123L);
      Assert.AreEqual("[I8]123", sut.GetString("ABC"), "#1");

      sut.SetNullableInt64("ABC", null);
      Assert.IsFalse(sut.HasValue("ABC"), "#2");
    }

    [Test]
    public void GetNullableInt64()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "");
      Assert.IsNull(sut.GetNullableInt64("ABC"), "#1");

      sut.SetString("ABC", "[I8]123");
      Assert.AreEqual(123L, sut.GetNullableInt64("ABC"), "#2");
    }

    #endregion

    #region Single

    [Test]
    public void SetNullableSingle()
    {
      CfgPart sut = Create();
      sut.SetNullableSingle("ABC", 123f);
      Assert.AreEqual("[F4]123", sut.GetString("ABC"), "#1");

      sut.SetNullableSingle("ABC", null);
      Assert.IsFalse(sut.HasValue("ABC"), "#2");
    }

    [Test]
    public void GetNullableSingle()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "");
      Assert.IsNull(sut.GetNullableSingle("ABC"), "#1");

      sut.SetString("ABC", "[F4]123");
      Assert.AreEqual(123f, sut.GetNullableSingle("ABC"), "#2");
    }

    #endregion

    #region Double

    [Test]
    public void SetNullableDouble()
    {
      CfgPart sut = Create();
      sut.SetNullableDouble("ABC", 123.0);
      Assert.AreEqual("[F8]123", sut.GetString("ABC"), "#1");

      sut.SetNullableDouble("ABC", null);
      Assert.IsFalse(sut.HasValue("ABC"), "#2");
    }

    [Test]
    public void GetNullableDouble()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "");
      Assert.IsNull(sut.GetNullableDouble("ABC"), "#1");

      sut.SetString("ABC", "[F8]123");
      Assert.AreEqual(123.0, sut.GetNullableDouble("ABC"), "#2");
    }

    #endregion

    #region Single

    [Test]
    public void SetNullableDecimal()
    {
      CfgPart sut = Create();
      sut.SetNullableDecimal("ABC", 123m);
      Assert.AreEqual("[DC]123", sut.GetString("ABC"), "#1");

      sut.SetNullableDecimal("ABC", null);
      Assert.IsFalse(sut.HasValue("ABC"), "#2");
    }

    [Test]
    public void GetNullableDecimal()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "");
      Assert.IsNull(sut.GetNullableDecimal("ABC"), "#1");

      sut.SetString("ABC", "[DC]123");
      Assert.AreEqual(123m, sut.GetNullableSingle("ABC"), "#2");
    }

    #endregion

    #endregion
  }
}
