using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Config;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.Tests;

namespace ExtTools_tests.Config
{
  [TestFixture]
  public abstract class CfgPartTests: FixtureWithSetUp
  {
    #region Абстрактные методы

    /// <summary>
    /// Создает тестовый объект
    /// </summary>
    /// <returns></returns>
    protected abstract CfgPart Create();

    #endregion

    #region Доступ к значениям

    #region String

    [Test]
    public void GetString_SetString_2args()
    {
      CfgPart sut = Create();
      Assert.AreEqual("", sut.GetString("ABC"), "#1");
      sut.SetString("ABC", "123");
      Assert.AreEqual("123", sut.GetString("ABC"), "#2");
    }

    [TestCase("123", false, true)]
    [TestCase("123", true, true)]
    [TestCase("", false, true)]
    [TestCase("", true, false)]
    public void SetString_3args(string value, bool removeEmpty, bool wantedHasValue)
    {
      CfgPart sut = Create();
      sut.SetString("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetString("ABC"), "GetString()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Int32

    [Test]
    public void GetInt_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(0, sut.GetInt("ABC"), "#1");
      sut.SetInt("ABC", 123);
      Assert.AreEqual(123, sut.GetInt("ABC"), "#2");
    }

    [Test]
    public void GetInt_2args()
    {
      CfgPart sut = Create();

      int res1 = 444;
      sut.GetInt("ABC", ref res1);
      Assert.AreEqual(444, res1, "#1");

      int res2 = 444;
      sut.SetInt("ABC", 123);
      sut.GetInt("ABC", ref res2);
      Assert.AreEqual(123, res2, "#2");

      int res3 = 444;
      sut.SetInt("ABC", 0);
      sut.GetInt("ABC", ref res3);
      Assert.AreEqual(0, res3, "#3");

      int res4 = 444;
      sut.SetString("ABC", "");
      sut.GetInt("ABC", ref res4);
      Assert.AreEqual(444, res4, "#4");
    }

    [Test]
    public void GetIntDef()
    {
      CfgPart sut = Create();

      int res1 = sut.GetIntDef("ABC", 444);
      Assert.AreEqual(444, res1, "#1");

      sut.SetInt("ABC", 123);
      int res2 = sut.GetIntDef("ABC", 444);
      Assert.AreEqual(123, res2, "#2");

      sut.SetInt("ABC", 0);
      int res3 = sut.GetIntDef("ABC", 444);
      Assert.AreEqual(0, res3, "#3");

      sut.SetString("ABC", "");
      int res4 = sut.GetIntDef("ABC", 444);
      Assert.AreEqual(444, res4, "#4");
    }

    [Test]
    public void SetInt_2args()
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", 0);
      Assert.AreEqual(0, sut.GetInt("ABC"), "#1");
      sut.SetInt("ABC", 123);
      Assert.AreEqual(123, sut.GetInt("ABC"), "#2");
    }

    [TestCase(123, false, true)]
    [TestCase(123, true, true)]
    [TestCase(0, false, true)]
    [TestCase(0, true, false)]
    public void SetInt_3args(int value, bool removeEmpty, bool wantedHasValue)
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetInt("ABC"), "GetInt()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Int64

    [Test]
    public void GetInt64_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(0L, sut.GetInt64("ABC"), "#1");
      sut.SetInt64("ABC", 1000000000000L);
      Assert.AreEqual(1000000000000L, sut.GetInt64("ABC"), "#2");
    }

    [Test]
    public void GetInt64_2args()
    {
      CfgPart sut = Create();

      long res1 = 444L;
      sut.GetInt64("ABC", ref res1);
      Assert.AreEqual(444L, res1, "#1");

      long res2 = 444L;
      sut.SetInt64("ABC", 123L);
      sut.GetInt64("ABC", ref res2);
      Assert.AreEqual(123L, res2, "#2");

      long res3 = 444L;
      sut.SetInt64("ABC", 0L);
      sut.GetInt64("ABC", ref res3);
      Assert.AreEqual(0L, res3, "#3");

      long res4 = 444L;
      sut.SetString("ABC", "");
      sut.GetInt64("ABC", ref res4);
      Assert.AreEqual(444L, res4, "#4");
    }

    [Test]
    public void GetInt64Def()
    {
      CfgPart sut = Create();

      long res1 = sut.GetInt64Def("ABC", 444L);
      Assert.AreEqual(444L, res1, "#1");

      sut.SetInt64("ABC", 123L);
      long res2 = sut.GetInt64Def("ABC", 444L);
      Assert.AreEqual(123L, res2, "#2");

      sut.SetInt64("ABC", 0L);
      long res3 = sut.GetInt64Def("ABC", 444L);
      Assert.AreEqual(0L, res3, "#3");

      sut.SetString("ABC", "");
      long res4 = sut.GetInt64Def("ABC", 444L);
      Assert.AreEqual(444L, res4, "#4");
    }

    [Test]
    public void SetInt64_2args()
    {
      CfgPart sut = Create();
      sut.SetInt64("ABC", 0L);
      Assert.AreEqual(0L, sut.GetInt64("ABC"), "#1");
      sut.SetInt64("ABC", 123L);
      Assert.AreEqual(123L, sut.GetInt64("ABC"), "#2");
    }

    [TestCase(1000000000000L, false, true)]
    [TestCase(1000000000000L, true, true)]
    [TestCase(0L, false, true)]
    [TestCase(0L, true, false)]
    public void SetInt64_3args(long value, bool removeEmpty, bool wantedHasValue)
    {
      CfgPart sut = Create();
      sut.SetInt64("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetInt64("ABC"), "GetInt64()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Single

    [Test]
    public void GetSingle_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(0f, sut.GetSingle("ABC"), "#1");
      sut.SetSingle("ABC", 123.4f);
      Assert.AreEqual(123.4f, sut.GetSingle("ABC"), "#2");
    }

    [Test]
    public void GetSingle_2args()
    {
      CfgPart sut = Create();

      float res1 = 4.4f;
      sut.GetSingle("ABC", ref res1);
      Assert.AreEqual(4.4f, res1, "#1");

      float res2 = 4.4f;
      sut.SetSingle("ABC", 1.2f);
      sut.GetSingle("ABC", ref res2);
      Assert.AreEqual(1.2f, res2, "#2");

      float res3 = 4.4f;
      sut.SetSingle("ABC", 0f);
      sut.GetSingle("ABC", ref res3);
      Assert.AreEqual(0f, res3, "#3");

      float res4 = 4.4f;
      sut.SetString("ABC", "");
      sut.GetSingle("ABC", ref res4);
      Assert.AreEqual(4.4f, res4, "#4");
    }

    [Test]
    public void GetSingleDef()
    {
      CfgPart sut = Create();

      float res1 = sut.GetSingleDef("ABC", 4.4f);
      Assert.AreEqual(4.4f, res1, "#1");

      sut.SetSingle("ABC", 1.2f);
      float res2 = sut.GetSingleDef("ABC", 4.4f);
      Assert.AreEqual(1.2f, res2, "#2");

      sut.SetSingle("ABC", 0f);
      float res3 = sut.GetSingleDef("ABC", 4.4f);
      Assert.AreEqual(0f, res3, "#3");

      sut.SetString("ABC", "");
      float res4 = sut.GetSingleDef("ABC", 4.4f);
      Assert.AreEqual(4.4f, res4, "#4");
    }

    [Test]
    public void SetSingle_2args()
    {
      CfgPart sut = Create();
      sut.SetSingle("ABC", 0f);
      Assert.AreEqual(0f, sut.GetSingle("ABC"), "#1");
      sut.SetSingle("ABC", 1.2f);
      Assert.AreEqual(1.2f, sut.GetSingle("ABC"), "#2");
    }

    [TestCase(1.2f, false, true)]
    [TestCase(1.2f, true, true)]
    [TestCase(0f, false, true)]
    [TestCase(0f, true, false)]
    public void SetSingle_3args(float value, bool removeEmpty, bool wantedHasValue)
    {
      CfgPart sut = Create();
      sut.SetSingle("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetSingle("ABC"), "GetSingle()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Double

    [Test]
    public void GetDouble_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(0.0, sut.GetDouble("ABC"), "#1");
      sut.SetDouble("ABC", 1.2);
      Assert.AreEqual(1.2, sut.GetDouble("ABC"), "#2");
    }

    [Test]
    public void GetDouble_2args()
    {
      CfgPart sut = Create();

      double res1 = 4.4;
      sut.GetDouble("ABC", ref res1);
      Assert.AreEqual(4.4, res1, "#1");

      double res2 = 4.4;
      sut.SetDouble("ABC", 1.2);
      sut.GetDouble("ABC", ref res2);
      Assert.AreEqual(1.2, res2, "#2");

      double res3 = 4.4;
      sut.SetDouble("ABC", 0.0);
      sut.GetDouble("ABC", ref res3);
      Assert.AreEqual(0.0, res3, "#3");

      double res4 = 4.4;
      sut.SetString("ABC", "");
      sut.GetDouble("ABC", ref res4);
      Assert.AreEqual(4.4, res4, "#4");
    }

    [Test]
    public void GetDoubleDef()
    {
      CfgPart sut = Create();

      double res1 = sut.GetDoubleDef("ABC", 4.4);
      Assert.AreEqual(4.4, res1, "#1");

      sut.SetDouble("ABC", 1.2);
      double res2 = sut.GetDoubleDef("ABC", 4.4);
      Assert.AreEqual(1.2, res2, "#2");

      sut.SetDouble("ABC", 0.0);
      double res3 = sut.GetDoubleDef("ABC", 4.4);
      Assert.AreEqual(0.0, res3, "#3");

      sut.SetString("ABC", "");
      double res4 = sut.GetDoubleDef("ABC", 4.4);
      Assert.AreEqual(4.4, res4, "#4");
    }

    [Test]
    public void SetDouble_2args()
    {
      CfgPart sut = Create();
      sut.SetDouble("ABC", 0.0);
      Assert.AreEqual(0.0, sut.GetDouble("ABC"), "#1");
      sut.SetDouble("ABC", 1.2);
      Assert.AreEqual(1.2, sut.GetDouble("ABC"), "#2");
    }

    [TestCase(1.2, false, true)]
    [TestCase(1.2, true, true)]
    [TestCase(0.0, false, true)]
    [TestCase(0.0, true, false)]
    public void SetDouble_3args(double value, bool removeEmpty, bool wantedHasValue)
    {
      CfgPart sut = Create();
      sut.SetDouble("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetDouble("ABC"), "GetDouble()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Decimal

    [Test]
    public void GetDecimal_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(0m, sut.GetDecimal("ABC"), "#1");
      sut.SetDecimal("ABC", 123m);
      Assert.AreEqual(123m, sut.GetDecimal("ABC"), "#2");
    }

    [Test]
    public void GetDecimal_2args()
    {
      CfgPart sut = Create();

      decimal res1 = 4.44m;
      sut.GetDecimal("ABC", ref res1);
      Assert.AreEqual(4.44m, res1, "#1");

      decimal res2 = 4.44m;
      sut.SetDecimal("ABC", 1.23m);
      sut.GetDecimal("ABC", ref res2);
      Assert.AreEqual(1.23m, res2, "#2");

      decimal res3 = 4.44m;
      sut.SetDecimal("ABC", 0m);
      sut.GetDecimal("ABC", ref res3);
      Assert.AreEqual(0m, res3, "#3");

      decimal res4 = 4.44m;
      sut.SetString("ABC", "");
      sut.GetDecimal("ABC", ref res4);
      Assert.AreEqual(4.44m, res4, "#4");
    }

    [Test]
    public void GetDecimalDef()
    {
      CfgPart sut = Create();

      decimal res1 = sut.GetDecimalDef("ABC", 4.44m);
      Assert.AreEqual(4.44m, res1, "#1");

      sut.SetDecimal("ABC", 1.23m);
      decimal res2 = sut.GetDecimalDef("ABC", 4.44m);
      Assert.AreEqual(1.23m, res2, "#2");

      sut.SetDecimal("ABC", 0m);
      decimal res3 = sut.GetDecimalDef("ABC", 4.44m);
      Assert.AreEqual(0m, res3, "#3");

      sut.SetString("ABC", "");
      decimal res4 = sut.GetDecimalDef("ABC", 4.44m);
      Assert.AreEqual(4.44m, res4, "#4");
    }

    [Test]
    public void SetDecimal_2args()
    {
      CfgPart sut = Create();
      sut.SetDecimal("ABC", 0m);
      Assert.AreEqual(0m, sut.GetDecimal("ABC"), "#1");
      sut.SetDecimal("ABC", 1.23m);
      Assert.AreEqual(1.23m, sut.GetDecimal("ABC"), "#2");
    }

    [TestCase(1.2, false, true)]
    [TestCase(1.2, true, true)]
    [TestCase(0.0, false, true)]
    [TestCase(0.0, true, false)]
    public void SetDecimal_3args(double doubleValue, bool removeEmpty, bool wantedHasValue)
    {
      decimal value = (decimal)doubleValue;
      CfgPart sut = Create();
      sut.SetDecimal("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetDecimal("ABC"), "GetDecimal()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Int32-Int64

    [Test]
    public void SetInt_GetInt64()
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", 123);
      Assert.AreEqual(123L, sut.GetInt64("ABC"));
    }

    [Test]
    public void SetInt64_GetInt()
    {
      CfgPart sut = Create();
      sut.SetInt64("ABC", 123L);
      Assert.AreEqual(123, sut.GetInt("ABC"));
    }

    #endregion

    #region Int-Float

    [Test]
    public void SetInt_GetSingle()
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", 123);
      Assert.AreEqual(123f, sut.GetSingle("ABC"));
    }

    [Test]
    public void SetInt_GetDouble()
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", 123);
      Assert.AreEqual(123.0, sut.GetDouble("ABC"));
    }

    [Test]
    public void SetInt_GetDecimal()
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", 123);
      Assert.AreEqual(123m, sut.GetDecimal("ABC"));
    }

    // Обратные преобразования не гарантированы

    #endregion

    #region Boolean

    [Test]
    public void GetBool_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(false, sut.GetBool("ABC"), "#1");

      sut.SetBool("ABC", true);
      Assert.AreEqual(true, sut.GetBool("ABC"), "#2");

      sut.SetBool("ABC", false);
      Assert.AreEqual(false, sut.GetBool("ABC"), "#3");
    }

    [Test]
    public void GetBool_2args()
    {
      CfgPart sut = Create();

      bool res1 = true;
      sut.GetBool("ABC", ref res1);
      Assert.AreEqual(true, res1, "#1");

      bool res2 = true;
      sut.SetBool("ABC", true);
      sut.GetBool("ABC", ref res2);
      Assert.AreEqual(true, res2, "#2");

      bool res3 = true;
      sut.SetBool("ABC", false);
      sut.GetBool("ABC", ref res3);
      Assert.AreEqual(false, res3, "#3");

      bool res4 = true;
      sut.SetString("ABC", "");
      sut.GetBool("ABC", ref res4);
      Assert.AreEqual(true, res4, "#4");
    }

    [Test]
    public void GetBoolDef()
    {
      CfgPart sut = Create();

      bool res1 = sut.GetBoolDef("ABC", true);
      Assert.AreEqual(true, res1, "#1");

      sut.SetBool("ABC", true);
      bool res2 = sut.GetBoolDef("ABC", true);
      Assert.AreEqual(true, res2, "#2");

      sut.SetBool("ABC", false);
      bool res3 = sut.GetBoolDef("ABC", true);
      Assert.AreEqual(false, res3, "#3");

      sut.SetString("ABC", "");
      bool res4 = sut.GetBoolDef("ABC", true);
      Assert.AreEqual(true, res4, "#4");
    }

    [Test]
    public void SetBool_2args()
    {
      CfgPart sut = Create();
      sut.SetBool("ABC", false);
      Assert.AreEqual(false, sut.GetBool("ABC"), "#1");
      sut.SetBool("ABC", true);
      Assert.AreEqual(true, sut.GetBool("ABC"), "#2");
    }

    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    public void SetBool_3args(bool value, bool removeEmpty, bool wantedHasValue)
    {
      CfgPart sut = Create();
      sut.SetBool("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetBool("ABC"), "GetBool()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Int-Boolean

    [TestCase(123, true)]
    [TestCase(-1, true)]
    [TestCase(0, false)]
    public void SetInt_GetBool(int setValue, bool wantedRes)
    {
      CfgPart sut = Create();
      sut.SetInt("ABC", setValue);
      Assert.AreEqual(wantedRes, sut.GetBool("ABC"));
    }

    [TestCase(false, 0)]
    [TestCase(true, 1)]
    public void SetBool_GetInt(bool setValue, int wantedRes)
    {
      CfgPart sut = Create();
      sut.SetBool("ABC", setValue);
      Assert.AreEqual(wantedRes, sut.GetInt("ABC"));
    }

    #endregion

    #region DateTime

    [Test]
    public void GetDateTime_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(DateTime.MinValue, sut.GetDateTime("ABC"), "#1");
      sut.SetDateTime("ABC", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), sut.GetDateTime("ABC"), "#2");
    }

    [Test]
    public void GetDateTime_2args()
    {
      CfgPart sut = Create();

      DateTime res1 = new DateTime(2023, 6, 14, 12, 34, 56);
      sut.GetDateTime("ABC", ref res1);
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), res1, "#1");

      DateTime res2 = new DateTime(2023, 6, 14, 12, 34, 56);
      sut.SetDateTime("ABC", new DateTime(2023, 6, 15, 0, 0, 1));
      sut.GetDateTime("ABC", ref res2);
      Assert.AreEqual(new DateTime(2023, 6, 15, 0, 0, 1), res2, "#2");

      DateTime res3 = new DateTime(2023, 6, 14, 12, 34, 56);
      sut.SetString("ABC", "");
      sut.GetDateTime("ABC", ref res3);
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), res3, "#4");
    }

    [Test]
    public void GetDateTimeDef()
    {
      CfgPart sut = Create();

      DateTime res1 = sut.GetDateTimeDef("ABC", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), res1, "#1");

      sut.SetDateTime("ABC", new DateTime(2023, 6, 15, 0, 0, 1));
      DateTime res2 = sut.GetDateTimeDef("ABC", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 15, 0, 0, 1), res2, "#2");

      sut.SetString("ABC", "");
      DateTime res3 = sut.GetDateTimeDef("ABC", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), res3, "#4");
    }

    [Test]
    public void SetDateTime()
    {
      CfgPart sut = Create();
      sut.SetDateTime("ABC", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), sut.GetDateTime("ABC"));
    }

    #endregion

    #region Date

    [Test]
    public void GetDate_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(DateTime.MinValue, sut.GetDate("ABC"), "#1");
      sut.SetDateTime("ABC", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 14), sut.GetDate("ABC"), "#2");
    }

    [Test]
    public void GetDate_2args()
    {
      CfgPart sut = Create();

      DateTime res1 = new DateTime(2023, 6, 14);
      sut.GetDate("ABC", ref res1);
      Assert.AreEqual(new DateTime(2023, 6, 14), res1, "#1");

      DateTime res2 = new DateTime(2023, 6, 14);
      sut.SetDate("ABC", new DateTime(2023, 6, 15));
      sut.GetDate("ABC", ref res2);
      Assert.AreEqual(new DateTime(2023, 6, 15), res2, "#2");

      DateTime res3 = new DateTime(2023, 6, 14);
      sut.SetString("ABC", "");
      sut.GetDate("ABC", ref res3);
      Assert.AreEqual(new DateTime(2023, 6, 14), res3, "#4");
    }

    [Test]
    public void GetDateDef()
    {
      CfgPart sut = Create();

      DateTime res1 = sut.GetDateTimeDef("ABC", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 14), res1, "#1");

      sut.SetDate("ABC", new DateTime(2023, 6, 15));
      DateTime res2 = sut.GetDateDef("ABC", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 15), res2, "#2");

      sut.SetString("ABC", "");
      DateTime res3 = sut.GetDateDef("ABC", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 14), res3, "#4");
    }

    [Test]
    public void SetDate()
    {
      CfgPart sut = Create();
      sut.SetDate("ABC", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 14), sut.GetDate("ABC"));
    }

    #endregion

    #region Date-DateTime

    [Test]
    public void SetDate_GetDateTime()
    {
      CfgPart sut = Create();
      sut.SetDate("ABC", new DateTime(2023, 6, 19, 12, 34, 56)); // лишний компонент времени
      Assert.AreEqual(new DateTime(2023, 6, 19, 0, 0, 0), sut.GetDateTime("ABC"));
    }


    [Test]
    public void SetDateTime_GetDate()
    {
      CfgPart sut = Create();
      sut.SetDateTime("ABC", new DateTime(2023, 6, 19, 12, 34, 56)); // лишний компонент времени
      Assert.AreEqual(new DateTime(2023, 6, 19), sut.GetDate("ABC"));
    }

    #endregion

    #region TimeSpan

    [Test]
    public void GetTimeSpan_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(TimeSpan.Zero, sut.GetTimeSpan("ABC"), "#1");
      sut.SetTimeSpan("ABC", new TimeSpan(12, 34, 56));
      Assert.AreEqual(new TimeSpan(12, 34, 56), sut.GetTimeSpan("ABC"), "#2");
    }

    [Test]
    public void GetTimeSpan_2args()
    {
      CfgPart sut = Create();

      TimeSpan res1 = new TimeSpan(12, 34, 56);
      sut.GetTimeSpan("ABC", ref res1);
      Assert.AreEqual(new TimeSpan(12, 34, 56), res1, "#1");

      TimeSpan res2 = new TimeSpan(12, 34, 56);
      sut.SetTimeSpan("ABC", new TimeSpan(1, 2, 3));
      sut.GetTimeSpan("ABC", ref res2);
      Assert.AreEqual(new TimeSpan(1, 2, 3), res2, "#2");

      TimeSpan res3 = new TimeSpan(12, 34, 56);
      sut.SetTimeSpan("ABC", TimeSpan.Zero);
      sut.GetTimeSpan("ABC", ref res3);
      Assert.AreEqual(TimeSpan.Zero, res3, "#3");

      TimeSpan res4 = new TimeSpan(12, 34, 56);
      sut.SetString("ABC", "");
      sut.GetTimeSpan("ABC", ref res4);
      Assert.AreEqual(new TimeSpan(12, 34, 56), res4, "#4");
    }

    [Test]
    public void GetTimeSpanDef()
    {
      CfgPart sut = Create();

      TimeSpan res1 = sut.GetTimeSpanDef("ABC", new TimeSpan(12, 34, 56));
      Assert.AreEqual(new TimeSpan(12, 34, 56), res1, "#1");

      sut.SetTimeSpan("ABC", new TimeSpan(1, 2, 3));
      TimeSpan res2 = sut.GetTimeSpanDef("ABC", new TimeSpan(12, 34, 56));
      Assert.AreEqual(new TimeSpan(1, 2, 3), res2, "#2");

      sut.SetTimeSpan("ABC", TimeSpan.Zero);
      TimeSpan res3 = sut.GetTimeSpanDef("ABC", new TimeSpan(12, 34, 56));
      Assert.AreEqual(TimeSpan.Zero, res3, "#3");

      sut.SetString("ABC", "");
      TimeSpan res4 = sut.GetTimeSpanDef("ABC", new TimeSpan(12, 34, 56));
      Assert.AreEqual(new TimeSpan(12, 34, 56), res4, "#4");
    }

    [Test]
    public void SetTimeSpan_2args()
    {
      CfgPart sut = Create();
      sut.SetTimeSpan("ABC", TimeSpan.Zero);
      Assert.AreEqual(TimeSpan.Zero, sut.GetTimeSpan("ABC"), "#1");
      sut.SetTimeSpan("ABC", new TimeSpan(1, 2, 3));
      Assert.AreEqual(new TimeSpan(1, 2, 3), sut.GetTimeSpan("ABC"), "#2");
    }

    [TestCase("12:34:56", false, true)]
    [TestCase("12:34:56", true, true)]
    [TestCase("0:0:0", false, true)]
    [TestCase("0:0:0", true, false)]
    public void SetTimeSpan_3args(string sValue, bool removeEmpty, bool wantedHasValue)
    {
      TimeSpan value = TimeSpan.Parse(sValue);
      CfgPart sut = Create();
      sut.SetTimeSpan("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetTimeSpan("ABC"), "GetTimeSpan()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Guid

    private const string TestGuid0 = "{00000000-0000-0000-0000-000000000000}";
    private const string TestGuid1 = "{E008C99D-523A-4A41-B22C-9094C5DF3199}";
    private const string TestGuid2 = "{8154A7DE-9B40-4595-AB57-1A6A9F4CA458}";

    [Test]
    public void GetGuid_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(Guid.Empty, sut.GetGuid("ABC"), "#1");
      sut.SetGuid("ABC", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid1), sut.GetGuid("ABC"), "#2");
    }

    [Test]
    public void GetGuid_2args()
    {
      CfgPart sut = Create();

      Guid res1 = new Guid(TestGuid1);
      sut.GetGuid("ABC", ref res1);
      Assert.AreEqual(new Guid(TestGuid1), res1, "#1");

      Guid res2 = new Guid(TestGuid1);
      sut.SetGuid("ABC", new Guid(TestGuid2));
      sut.GetGuid("ABC", ref res2);
      Assert.AreEqual(new Guid(TestGuid2), res2, "#2");

      Guid res3 = new Guid(TestGuid1);
      sut.SetGuid("ABC", Guid.Empty);
      sut.GetGuid("ABC", ref res3);
      Assert.AreEqual(Guid.Empty, res3, "#3");

      Guid res4 = new Guid(TestGuid1);
      sut.SetString("ABC", "");
      sut.GetGuid("ABC", ref res4);
      Assert.AreEqual(new Guid(TestGuid1), res4, "#4");
    }

    [Test]
    public void GetGuidDef()
    {
      CfgPart sut = Create();

      Guid res1 = sut.GetGuidDef("ABC", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid1), res1, "#1");

      sut.SetGuid("ABC", new Guid(TestGuid2));
      Guid res2 = sut.GetGuidDef("ABC", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid2), res2, "#2");

      sut.SetGuid("ABC", Guid.Empty);
      Guid res3 = sut.GetGuidDef("ABC", new Guid(TestGuid1));
      Assert.AreEqual(Guid.Empty, res3, "#3");

      sut.SetString("ABC", "");
      Guid res4 = sut.GetGuidDef("ABC", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid1), res4, "#4");
    }

    [Test]
    public void SetGuid_2args()
    {
      CfgPart sut = Create();
      sut.SetGuid("ABC", Guid.Empty);
      Assert.AreEqual(Guid.Empty, sut.GetGuid("ABC"), "#1");
      sut.SetGuid("ABC", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid1), sut.GetGuid("ABC"), "#2");
    }

    [TestCase(TestGuid1, false, true)]
    [TestCase(TestGuid1, true, true)]
    [TestCase(TestGuid0, false, true)]
    [TestCase(TestGuid0, true, false)]
    public void SetGuid_3args(string sValue, bool removeEmpty, bool wantedHasValue)
    {
      Guid value = new Guid(sValue);
      CfgPart sut = Create();
      sut.SetGuid("ABC", value, removeEmpty);
      Assert.AreEqual(value, sut.GetGuid("ABC"), "GetGuid()");
      Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    }

    #endregion

    #region Enum

    public enum TestEnum { Zero, One, Two, Three }

    [Test]
    public void GetEnum_1arg()
    {
      CfgPart sut = Create();
      Assert.AreEqual(TestEnum.Zero, sut.GetEnum<TestEnum>("ABC"), "#1");
      sut.SetEnum<TestEnum>("ABC", TestEnum.One);
      Assert.AreEqual(TestEnum.One, sut.GetEnum<TestEnum>("ABC"), "#2");
    }

    [Test]
    public void GetEnum_2args()
    {
      CfgPart sut = Create();

      TestEnum res1 = TestEnum.One;
      sut.GetEnum<TestEnum>("ABC", ref res1);
      Assert.AreEqual(TestEnum.One, res1, "#1");

      TestEnum res2 = TestEnum.One;
      sut.SetEnum<TestEnum>("ABC", TestEnum.Two);
      sut.GetEnum<TestEnum>("ABC", ref res2);
      Assert.AreEqual(TestEnum.Two, res2, "#2");

      TestEnum res3 = TestEnum.One;
      sut.SetEnum<TestEnum>("ABC", TestEnum.Zero);
      sut.GetEnum<TestEnum>("ABC", ref res3);
      Assert.AreEqual(TestEnum.Zero, res3, "#3");

      TestEnum res4 = TestEnum.One;
      sut.SetString("ABC", "");
      sut.GetEnum<TestEnum>("ABC", ref res4);
      Assert.AreEqual(TestEnum.One, res4, "#4");
    }

    [Test]
    public void GetEnumDef()
    {
      CfgPart sut = Create();

      TestEnum res1 = sut.GetEnumDef<TestEnum>("ABC", TestEnum.One);
      Assert.AreEqual(TestEnum.One, res1, "#1");

      sut.SetEnum<TestEnum>("ABC", TestEnum.Two);
      TestEnum res2 = sut.GetEnumDef<TestEnum>("ABC", TestEnum.One);
      Assert.AreEqual(TestEnum.Two, res2, "#2");

      sut.SetEnum<TestEnum>("ABC", TestEnum.Zero);
      TestEnum res3 = sut.GetEnumDef<TestEnum>("ABC", TestEnum.One);
      Assert.AreEqual(TestEnum.Zero, res3, "#3");

      sut.SetString("ABC", "");
      TestEnum res4 = sut.GetEnumDef<TestEnum>("ABC", TestEnum.One);
      Assert.AreEqual(TestEnum.One, res4, "#4");
    }

    [Test]
    public void SetEnum_2args()
    {
      CfgPart sut = Create();
      sut.SetEnum<TestEnum>("ABC", TestEnum.Zero);
      Assert.AreEqual(TestEnum.Zero, sut.GetEnum<TestEnum>("ABC"), "#1");
      sut.SetEnum<TestEnum>("ABC", TestEnum.One);
      Assert.AreEqual(TestEnum.One, sut.GetEnum<TestEnum>("ABC"), "#2");
    }

    // Нет такой перегрузки
    //[TestCase(TestEnum.One, false, true)]
    //[TestCase(TestEnum.One, true, true)]
    //[TestCase(TestEnum.Zero, false, true)]
    //[TestCase(TestEnum.Zero, true, false)]
    //public void SetEnum_3args(TestEnum value, bool removeEmpty, bool wantedHasValue)
    //{
    //  CfgPart sut = Create();
    //  sut.SetEnum<TestEnum>("ABC", value, removeEmpty);
    //  Assert.AreEqual(value, sut.GetEnum<TestEnum>("ABC"), "GetEnum()");
    //  Assert.AreEqual(wantedHasValue, sut.HasValue("ABC"), "HasValue()");
    //}

    #endregion

    #region Enum-String

    [TestCase("One", TestEnum.One)]
    [TestCase("", TestEnum.Zero)]
    [TestCase("XXX", TestEnum.Zero)]
    public void SetString_GetEnum(string setValue, TestEnum wantedRes)
    {
      CfgPart sut = Create();
      sut.SetString("ABC", setValue);
      Assert.AreEqual(wantedRes, sut.GetEnum<TestEnum>("ABC"));
    }

    [TestCase(TestEnum.Zero, "Zero")]
    [TestCase(TestEnum.One, "One")]
    public void SetEnum_GetString(TestEnum setValue, string wantedRes)
    {
      CfgPart sut = Create();
      sut.SetEnum<TestEnum>("ABC", setValue);
      Assert.AreEqual(wantedRes, sut.GetString("ABC"));
    }

    #endregion

    #endregion

    #region Доступ к Nullable-значениям

    #region Int32

    [Test]
    public void GetNullableInt()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableInt("AAA"), "#1");

      sut.SetInt("AAA", 123);
      Assert.AreEqual(123, sut.GetNullableInt("AAA"), "#2");
    }

    [Test]
    public void SetNullableInt()
    {
      CfgPart sut = Create();
      sut.SetNullableInt("AAA", 123);
      Assert.AreEqual(123, sut.GetInt("AAA"), "#1");

      sut.SetNullableInt("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableInt("AAA", 0);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region Int64

    [Test]
    public void GetNullableInt64()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableInt64("AAA"), "#1");

      sut.SetInt64("AAA", 123L);
      Assert.AreEqual(123L, sut.GetNullableInt64("AAA"), "#2");
    }

    [Test]
    public void SetNullableInt64()
    {
      CfgPart sut = Create();
      sut.SetNullableInt64("AAA", 123L);
      Assert.AreEqual(123L, sut.GetInt64("AAA"), "#1");

      sut.SetNullableInt64("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableInt64("AAA", 0L);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region Single

    [Test]
    public void GetNullableSingle()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableSingle("AAA"), "#1");

      sut.SetSingle("AAA", 1.2f);
      Assert.AreEqual(1.2f, sut.GetNullableSingle("AAA"), "#2");
    }

    [Test]
    public void SetNullableSingle()
    {
      CfgPart sut = Create();
      sut.SetNullableSingle("AAA", 1.2f);
      Assert.AreEqual(1.2f, sut.GetSingle("AAA"), "#1");

      sut.SetNullableSingle("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableSingle("AAA", 0f);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region Double

    [Test]
    public void GetNullableDouble()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableDouble("AAA"), "#1");

      sut.SetDouble("AAA", 1.23);
      Assert.AreEqual(1.23, sut.GetNullableDouble("AAA"), "#2");
    }

    [Test]
    public void SetNullableDouble()
    {
      CfgPart sut = Create();
      sut.SetNullableDouble("AAA", 1.23);
      Assert.AreEqual(1.23, sut.GetDouble("AAA"), "#1");

      sut.SetNullableDouble("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableDouble("AAA", 0.0);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region Decimal

    [Test]
    public void GetNullableDecimal()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableDecimal("AAA"), "#1");

      sut.SetDecimal("AAA", 123.45m);
      Assert.AreEqual(123.45m, sut.GetNullableDecimal("AAA"), "#2");
    }

    [Test]
    public void SetNullableDecimal()
    {
      CfgPart sut = Create();
      sut.SetNullableDecimal("AAA", 123.45m);
      Assert.AreEqual(123.45m, sut.GetDecimal("AAA"), "#1");

      sut.SetNullableDecimal("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableDecimal("AAA", 0m);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region Boolean

    [Test]
    public void GetNullableBool()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableBool("AAA"), "#1");

      sut.SetBool("AAA", true);
      Assert.AreEqual(true, sut.GetNullableBool("AAA"), "#2");
    }

    [Test]
    public void SetNullableBool()
    {
      CfgPart sut = Create();
      sut.SetNullableBool("AAA", true);
      Assert.AreEqual(true, sut.GetBool("AAA"), "#1");

      sut.SetNullableBool("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableBool("AAA", false);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region DateTime

    [Test]
    public void GetNullableDateTime()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableDateTime("AAA"), "#1");

      sut.SetDateTime("AAA", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), sut.GetNullableDateTime("AAA"), "#2");
    }

    [Test]
    public void SetNullableDateTime()
    {
      CfgPart sut = Create();
      sut.SetNullableDateTime("AAA", new DateTime(2023, 6, 14, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 6, 14, 12, 34, 56), sut.GetDateTime("AAA"), "#1");

      sut.SetNullableDateTime("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");
    }

    #endregion

    #region Date

    [Test]
    public void GetNullableDate()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableDate("AAA"), "#1");

      sut.SetDate("AAA", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 14), sut.GetNullableDate("AAA"), "#2");
    }

    [Test]
    public void SetNullableDate()
    {
      CfgPart sut = Create();
      sut.SetNullableDate("AAA", new DateTime(2023, 6, 14));
      Assert.AreEqual(new DateTime(2023, 6, 14), sut.GetDate("AAA"), "#1");

      sut.SetNullableDate("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");
    }

    #endregion

    #region TimeSpan

    [Test]
    public void GetNullableTimeSpan()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableTimeSpan("AAA"), "#1");

      sut.SetTimeSpan("AAA", new TimeSpan(12, 34, 56));
      Assert.AreEqual(new TimeSpan(12, 34, 56), sut.GetNullableTimeSpan("AAA"), "#2");
    }

    [Test]
    public void SetNullableTimeSpan()
    {
      CfgPart sut = Create();
      sut.SetNullableTimeSpan("AAA", new TimeSpan(12, 34, 56));
      Assert.AreEqual(new TimeSpan(12, 34, 56), sut.GetTimeSpan("AAA"), "#1");

      sut.SetNullableTimeSpan("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableTimeSpan("AAA", TimeSpan.Zero);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #region Guid

    [Test]
    public void GetNullableGuid()
    {
      CfgPart sut = Create();
      Assert.IsNull(sut.GetNullableGuid("AAA"), "#1");

      sut.SetGuid("AAA", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid1), sut.GetNullableGuid("AAA"), "#2");
    }

    [Test]
    public void SetNullableGuid()
    {
      CfgPart sut = Create();
      sut.SetNullableGuid("AAA", new Guid(TestGuid1));
      Assert.AreEqual(new Guid(TestGuid1), sut.GetGuid("AAA"), "#1");

      sut.SetNullableGuid("AAA", null);
      Assert.IsFalse(sut.HasValue("AAA"), "#2");

      sut.SetNullableGuid("AAA", Guid.Empty);
      Assert.IsTrue(sut.HasValue("AAA"), "#3");
    }

    #endregion

    #endregion

    #region Доступ к массивам значений

    #region Int32

    [Test]
    public void SetIntCommaString()
    {
      CfgPart sut = Create();

      int[] values1 = new int[] { 10, 20, -30, 10 };
      sut.SetIntCommaString("ABC", values1);

      string resStr1 = sut.GetString("ABC");
      int[] resArray1 = StdConvert.ToInt32Array(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      int[] values2 = new int[] { -30 };
      sut.SetIntCommaString("ABC", values2);
      Assert.AreEqual(-30, sut.GetInt("ABC"), "#2");

      int[] values3 = null;
      sut.SetIntCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      int[] values4 = new int[] { };
      sut.SetIntCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetIntCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetIntCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetIntCommaString("ABC"), "#2");

      sut.SetInt("ABC", 123);
      CollectionAssert.AreEqual(new int[] { 123 }, sut.GetIntCommaString("ABC"), "#3");

      int[] values4 = new int[] { 10, 20, -30, 10 };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetIntCommaString("ABC"), "#4");
    }

    #endregion

    #region Int64

    [Test]
    public void SetInt64CommaString()
    {
      CfgPart sut = Create();

      long[] values1 = new long[] { 10L, 20L, -30L, 10L };
      sut.SetInt64CommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      long[] resArray1 = StdConvert.ToInt64Array(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      long[] values2 = new long[] { -30 };
      sut.SetInt64CommaString("ABC", values2);
      Assert.AreEqual(-30, sut.GetInt("ABC"), "#2");

      long[] values3 = null;
      sut.SetInt64CommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      long[] values4 = new long[] { };
      sut.SetInt64CommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetInt64CommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetInt64CommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetInt64CommaString("ABC"), "#2");

      sut.SetInt64("ABC", 123L);
      CollectionAssert.AreEqual(new long[] { 123L }, sut.GetInt64CommaString("ABC"), "#3");

      long[] values4 = new long[] { 10L, 20L, -30L, 10L };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetInt64CommaString("ABC"), "#4");
    }

    #endregion

    #region Int32-Int64

    [Test]
    public void SetIntCommaString_GetInt64CommaString()
    {
      CfgPart sut = Create();
      sut.SetIntCommaString("ABC", new int[] { 10, 20, -30, 10, 0 });
      long[] res = sut.GetInt64CommaString("ABC");
      Assert.AreEqual(new long[] { 10L, 20L, -30L, 10L, 0L }, res);
    }

    [Test]
    public void SetInt64CommaString_GetIntCommaString()
    {
      CfgPart sut = Create();
      sut.SetInt64CommaString("ABC", new long[] { 10L, 20L, -30L, 10L, 0L });
      int[] res = sut.GetIntCommaString("ABC");
      Assert.AreEqual(new int[] { 10, 20, -30, 10, 0 }, res);
    }

    #endregion

    #region Single

    [Test]
    public void SetSingleCommaString()
    {
      CfgPart sut = Create();

      float[] values1 = new float[] { 1.5f, 2.5f, -3.2f, 10f };
      sut.SetSingleCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      float[] resArray1 = StdConvert.ToSingleArray(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      float[] values2 = new float[] { -30f };
      sut.SetSingleCommaString("ABC", values2);
      Assert.AreEqual(-30f, sut.GetSingle("ABC"), "#2");

      float[] values3 = null;
      sut.SetSingleCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      float[] values4 = new float[] { };
      sut.SetSingleCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetSingleCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetSingleCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetSingleCommaString("ABC"), "#2");

      sut.SetSingle("ABC", 1.2f);
      CollectionAssert.AreEqual(new float[] { 1.2f }, sut.GetSingleCommaString("ABC"), "#3");

      float[] values4 = new float[] { 1.0f, 2.2f, -3.2f, 10f };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetSingleCommaString("ABC"), "#4");
    }

    #endregion

    #region Double

    [Test]
    public void SetDoubleCommaString()
    {
      CfgPart sut = Create();

      double[] values1 = new double[] { 1.7, 2.4, -3.14, 1.15 };
      sut.SetDoubleCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      double[] resArray1 = StdConvert.ToDoubleArray(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      double[] values2 = new double[] { -3.14 };
      sut.SetDoubleCommaString("ABC", values2);
      Assert.AreEqual(-3.14, sut.GetDouble("ABC"), "#2");

      double[] values3 = null;
      sut.SetDoubleCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      double[] values4 = new double[] { };
      sut.SetDoubleCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetDoubleCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetDoubleCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetDoubleCommaString("ABC"), "#2");

      sut.SetDouble("ABC", 1.23);
      CollectionAssert.AreEqual(new double[] { 1.23 }, sut.GetDoubleCommaString("ABC"), "#3");

      double[] values4 = new double[] { 1.15, 2.0, -3.0, 1.23 };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetDoubleCommaString("ABC"), "#4");
    }

    #endregion

    #region Decimal

    [Test]
    public void SetDecimalCommaString()
    {
      CfgPart sut = Create();

      decimal[] values1 = new decimal[] { 1.23m, 2m, -3.14159m, 10m, 1.23m };
      sut.SetDecimalCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      decimal[] resArray1 = StdConvert.ToDecimalArray(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      decimal[] values2 = new decimal[] { -30.5m };
      sut.SetDecimalCommaString("ABC", values2);
      Assert.AreEqual(-30.5m, sut.GetDecimal("ABC"), "#2");

      decimal[] values3 = null;
      sut.SetDecimalCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      decimal[] values4 = new decimal[] { };
      sut.SetDecimalCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetDecimalCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetDecimalCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetDecimalCommaString("ABC"), "#2");

      sut.SetDecimal("ABC", 12.3m);
      CollectionAssert.AreEqual(new decimal[] { 12.3m }, sut.GetDecimalCommaString("ABC"), "#3");

      decimal[] values4 = new decimal[] { 10m, 20.24m, -30.33m, 10m };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetDecimalCommaString("ABC"), "#4");
    }

    #endregion

    #region Int-Float

    [Test]
    public void SetIntCommaString_GetSingleCommaString()
    {
      CfgPart sut = Create();
      sut.SetIntCommaString("ABC", new int[] { 10, 20, -30, 10, 0 });
      float[] res = sut.GetSingleCommaString("ABC");
      Assert.AreEqual(new float[] { 10f, 20f, -30f, 10f, 0f }, res);
    }

    [Test]
    public void SetSingleCommaString_GetIntCommaString()
    {
      CfgPart sut = Create();
      sut.SetSingleCommaString("ABC", new float[] { 10f, 20f, -30f, 10f, 0f });
      int[] res = sut.GetIntCommaString("ABC");
      Assert.AreEqual(new int[] { 10, 20, -30, 10, 0 }, res);
    }


    [Test]
    public void SetIntCommaString_GetDoubleCommaString()
    {
      CfgPart sut = Create();
      sut.SetIntCommaString("ABC", new int[] { 10, 20, -30, 10, 0 });
      double[] res = sut.GetDoubleCommaString("ABC");
      Assert.AreEqual(new double[] { 10.0, 20.0, -30.0, 10.0, 0.0 }, res);
    }

    [Test]
    public void SetDoubleCommaString_GetDoubleCommaString()
    {
      CfgPart sut = Create();
      sut.SetDoubleCommaString("ABC", new double[] { 10.0, 20.0, -30.0, 10.0, 0.0 });
      int[] res = sut.GetIntCommaString("ABC");
      Assert.AreEqual(new int[] { 10, 20, -30, 10, 0 }, res);
    }


    [Test]
    public void SetIntCommaString_GetDecimalCommaString()
    {
      CfgPart sut = Create();
      sut.SetIntCommaString("ABC", new int[] { 10, 20, -30, 10, 0 });
      decimal[] res = sut.GetDecimalCommaString("ABC");
      Assert.AreEqual(new decimal[] { 10m, 20m, -30m, 10m, 0m }, res);
    }

    [Test]
    public void SetDecimalCommaString_GetIntCommaString()
    {
      CfgPart sut = Create();
      sut.SetDecimalCommaString("ABC", new decimal[] { 10m, 20m, -30m, 10m, 0m });
      int[] res = sut.GetIntCommaString("ABC");
      Assert.AreEqual(new int[] { 10, 20, -30, 10, 0 }, res);
    }

    #endregion

    #region Float-Float

    [Test]
    public void SetSingleCommaString_GetDoubleCommaString()
    {
      CfgPart sut = Create();
      sut.SetSingleCommaString("ABC", new float[] { 10.5f, -30.5f });
      Assert.AreEqual(new double[] { 10.5, -30.5 }, sut.GetDoubleCommaString("ABC"));
    }

    [Test]
    public void SetDoubleCommaString_GetSingleCommaString()
    {
      CfgPart sut = Create();
      sut.SetDoubleCommaString("ABC", new double[] { 10.5, -30.5 });
      Assert.AreEqual(new float[] { 10.5f, -30.5f }, sut.GetSingleCommaString("ABC"));
    }


    [Test]
    public void SetSingleCommaString_GetDecimalCommaString()
    {
      CfgPart sut = Create();
      sut.SetSingleCommaString("ABC", new float[] { 10.5f, -30.5f });
      Assert.AreEqual(new decimal[] { 10.5m, -30.5m }, sut.GetDecimalCommaString("ABC"));
    }

    [Test]
    public void SetDecimalCommaString_GetSingleCommaString()
    {
      CfgPart sut = Create();
      sut.SetDecimalCommaString("ABC", new decimal[] { 10.5m, -30.5m });
      Assert.AreEqual(new float[] { 10.5f, -30.5f }, sut.GetSingleCommaString("ABC"));
    }


    [Test]
    public void SetDoubleCommaString_GetDecimalCommaString()
    {
      CfgPart sut = Create();
      sut.SetDoubleCommaString("ABC", new double[] { 10.5, -30.5 });
      Assert.AreEqual(new decimal[] { 10.5m, -30.5m }, sut.GetDecimalCommaString("ABC"));
    }

    [Test]
    public void SetDecimalCommaString_GetDoubleCommaString()
    {
      CfgPart sut = Create();
      sut.SetDecimalCommaString("ABC", new decimal[] { 10.5m, -30.5m });
      Assert.AreEqual(new double[] { 10.5, -30.5 }, sut.GetDoubleCommaString("ABC"));
    }

    #endregion

    #region DateTime

    [Test]
    public void SetDateTimeCommaString()
    {
      CfgPart sut = Create();

      DateTime[] values1 = new DateTime[] {
        new DateTime(2023, 6, 15, 12, 34, 56),
        new DateTime(2023, 1, 1, 0, 0, 1) };

      sut.SetDateTimeCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      DateTime[] resArray1 = StdConvert.ToDateTimeArray(resStr1, true);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      DateTime[] values2 = new DateTime[] { new DateTime(2023, 6, 15, 12, 34, 56) };
      sut.SetDateTimeCommaString("ABC", values2);
      Assert.AreEqual(new DateTime(2023, 6, 15, 12, 34, 56), sut.GetDateTime("ABC"), "#2");

      DateTime[] values3 = null;
      sut.SetDateTimeCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      DateTime[] values4 = new DateTime[] { };
      sut.SetDateTimeCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetDateTimeCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetDateTimeCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetDateTimeCommaString("ABC"), "#2");

      sut.SetDateTime("ABC", new DateTime(2023, 6, 15, 12, 34, 56));
      CollectionAssert.AreEqual(new DateTime[] { new DateTime(2023, 6, 15, 12, 34, 56) },
        sut.GetDateTimeCommaString("ABC"), "#3");

      DateTime[] values4 = new DateTime[] {
        new DateTime(2023, 6, 15, 12, 34, 56),
        new DateTime(2023, 1, 1, 0, 0, 1) };
      sut.SetString("ABC", StdConvert.ToString(values4, true));
      CollectionAssert.AreEqual(values4, sut.GetDateTimeCommaString("ABC"), "#4");
    }


    [Test]
    public void SetDateCommaString()
    {
      CfgPart sut = Create();

      DateTime[] values1 = new DateTime[] {
        new DateTime(2023, 6, 15),
        new DateTime(2023, 1, 1) };

      sut.SetDateCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      DateTime[] resArray1 = StdConvert.ToDateTimeArray(resStr1, false);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      DateTime[] values2 = new DateTime[] { new DateTime(2023, 6, 15) };
      sut.SetDateCommaString("ABC", values2);
      Assert.AreEqual(new DateTime(2023, 6, 15), sut.GetDate("ABC"), "#2");

      DateTime[] values3 = null;
      sut.SetDateCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      DateTime[] values4 = new DateTime[] { };
      sut.SetDateCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetDateCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetDateCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetDateCommaString("ABC"), "#2");

      sut.SetDateTime("ABC", new DateTime(2023, 6, 15));
      CollectionAssert.AreEqual(new DateTime[] { new DateTime(2023, 6, 15) },
        sut.GetDateCommaString("ABC"), "#3");

      DateTime[] values4 = new DateTime[] {
        new DateTime(2023, 6, 15),
        new DateTime(2023, 1, 1) };
      sut.SetString("ABC", StdConvert.ToString(values4, true));
      CollectionAssert.AreEqual(values4, sut.GetDateCommaString("ABC"), "#4");
    }

    #endregion

    #region TimeSpan

    [Test]
    public void SetTimeSpanCommaString()
    {
      CfgPart sut = Create();

      TimeSpan[] values1 = new TimeSpan[] { new TimeSpan(12, 34, 56), new TimeSpan(-1, -2, -3), TimeSpan.Zero };
      sut.SetTimeSpanCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      TimeSpan[] resArray1 = StdConvert.ToTimeSpanArray(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      TimeSpan[] values2 = new TimeSpan[] { new TimeSpan(-12, -34, -56) };
      sut.SetTimeSpanCommaString("ABC", values2);
      Assert.AreEqual(new TimeSpan(-12, -34, -56), sut.GetTimeSpan("ABC"), "#2");

      TimeSpan[] values3 = null;
      sut.SetTimeSpanCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      TimeSpan[] values4 = new TimeSpan[] { };
      sut.SetTimeSpanCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetTimeSpanCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetTimeSpanCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetTimeSpanCommaString("ABC"), "#2");

      sut.SetTimeSpan("ABC", new TimeSpan(-12, -34, -56));
      CollectionAssert.AreEqual(new TimeSpan[] { new TimeSpan(-12, -34, -56) }, sut.GetTimeSpanCommaString("ABC"), "#3");

      TimeSpan[] values4 = new TimeSpan[] { TimeSpan.Zero, new TimeSpan(12, 34, 56), new TimeSpan(-1, -2, -3) };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetTimeSpanCommaString("ABC"), "#4");
    }

    #endregion

    #region Guid

    [Test]
    public void SetGuidCommaString()
    {
      CfgPart sut = Create();

      Guid[] values1 = new Guid[] { new Guid(TestGuid1), new Guid(TestGuid2), new Guid(TestGuid0) };
      sut.SetGuidCommaString("ABC", values1);
      string resStr1 = sut.GetString("ABC");
      Guid[] resArray1 = StdConvert.ToGuidArray(resStr1);
      CollectionAssert.AreEqual(values1, resArray1, "#1");

      Guid[] values2 = new Guid[] { new Guid(TestGuid1) };
      sut.SetGuidCommaString("ABC", values2);
      Assert.AreEqual(new Guid(TestGuid1), sut.GetGuid("ABC"), "#2");

      Guid[] values3 = null;
      sut.SetGuidCommaString("ABC", values3);
      Assert.IsFalse(sut.HasValue("ABC"), "#3");

      Guid[] values4 = new Guid[] { };
      sut.SetGuidCommaString("ABC", values4);
      Assert.IsTrue(sut.HasValue("ABC"), "#4 HasValue()");
      Assert.AreEqual("", sut.GetString("ABC"), "#4 GetString()");
    }

    [Test]
    public void GetGuidCommaString()
    {
      CfgPart sut = Create();

      Assert.IsNull(sut.GetGuidCommaString("ABC"), "#1");

      sut.SetString("ABC", String.Empty, false);
      Assert.IsNull(sut.GetGuidCommaString("ABC"), "#2");

      sut.SetGuid("ABC", new Guid(TestGuid1));
      CollectionAssert.AreEqual(new Guid[] { new Guid(TestGuid1) }, sut.GetGuidCommaString("ABC"), "#3");

      Guid[] values4 = new Guid[] { new Guid(TestGuid1), new Guid(TestGuid2), new Guid(TestGuid0) };
      sut.SetString("ABC", StdConvert.ToString(values4));
      CollectionAssert.AreEqual(values4, sut.GetGuidCommaString("ABC"), "#4");
    }

    #endregion

    #endregion

    #region HistoryList

    [Test]
    public void SetHist_3()
    {
      CfgPart sut = Create();

      HistoryList arg = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      sut.SetHist("ABC", arg);

      Assert.AreEqual("AAA", sut.GetString("ABC"), "MainValue");
      CfgPart histPart = sut.GetChild("ABC_Hist", false);
      Assert.IsNotNull(histPart, "HistPart");
      CollectionAssert.AreEquivalent(new string[] { "H1", "H2" }, histPart.GetValueNames(), "HistPart ValueNames");
      Assert.AreEqual("BBB", histPart.GetString("H1"), "HistPart H1");
      Assert.AreEqual("CCC", histPart.GetString("H2"), "HistPart H2");
      Assert.AreEqual(0, histPart.GetChildNames().Length, "HistPart ChildNames");
    }

    [Test]
    public void SetHist_2()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "ZZZ");
      CfgPart dummyPart = sut.GetChild("ABC_Hist", true);
      dummyPart.SetString("H2", "ZZZ"); // испортил существующие данные. SetHist() должен все исправить

      HistoryList arg = new HistoryList(new string[] { "AAA", "BBB" });
      sut.SetHist("ABC", arg);

      Assert.AreEqual("AAA", sut.GetString("ABC"), "MainValue");
      CfgPart histPart = sut.GetChild("ABC_Hist", false);
      Assert.IsNotNull(histPart, "HistPart");
      CollectionAssert.AreEquivalent(new string[] { "H1" }, histPart.GetValueNames(), "HistPart ValueNames");
      Assert.AreEqual("BBB", histPart.GetString("H1"), "HistPart H1");
      Assert.AreEqual(0, histPart.GetChildNames().Length, "HistPart ChildNames");
    }

    [Test]
    public void SetHist_1()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "ZZZ");
      CfgPart dummyPart = sut.GetChild("ABC_Hist", true);
      dummyPart.SetString("H2", "ZZZ"); // испортил существующие данные. SetHist() должен все исправить

      HistoryList arg = new HistoryList("AAA");
      sut.SetHist("ABC", arg);

      Assert.AreEqual("AAA", sut.GetString("ABC"), "MainValue");
      CfgPart histPart = sut.GetChild("ABC_Hist", false);
      Assert.IsNull(histPart, "HistPart");
    }

    [Test]
    public void SetHist_0()
    {
      CfgPart sut = Create();
      sut.SetString("ABC", "ZZZ");
      CfgPart dummyPart = sut.GetChild("ABC_Hist", true);
      dummyPart.SetString("H2", "ZZZ"); // испортил существующие данные. SetHist() должен все исправить

      HistoryList arg = new HistoryList();
      sut.SetHist("ABC", arg);

      Assert.AreEqual("", sut.GetString("ABC"), "MainValue");
      CfgPart histPart = sut.GetChild("ABC_Hist", false);
      Assert.IsNull(histPart, "HistPart");
    }

    [Test]
    public void GetHist()
    {
      CfgPart sut = Create();
      HistoryList res1 = sut.GetHist("ABC");
      CollectionAssert.AreEqual(new string[] { }, res1, "#1");

      sut.SetString("ABC", "AAA");
      HistoryList res2 = sut.GetHist("ABC");
      CollectionAssert.AreEqual(new string[] { "AAA" }, res2, "#2");

      CfgPart histPart = sut.GetChild("ABC_Hist", true);
      HistoryList res3 = sut.GetHist("ABC");
      CollectionAssert.AreEqual(new string[] { "AAA" }, res3, "#3");

      histPart.SetString("H1", "BBB");
      HistoryList res4 = sut.GetHist("ABC");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB" }, res4, "#4");

      histPart.SetString("H2", "CCC");
      HistoryList res5 = sut.GetHist("ABC");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, res5, "#5");
    }

    [Test]
    public void AddToHist_2args()
    {
      CfgPart sut = Create();

      List<string> lst = new List<string>();

      // Проверка без переполнения
      for (int i = 1; i <= HistoryList.DefaultMaxHistLength; i++)
      {
        string value = "X" + StdConvert.ToString(i);
        sut.AddToHist("ABC", value);
        lst.Insert(0, value);

        HistoryList res = sut.GetHist("ABC");
        CollectionAssert.AreEqual(lst, res, "Add #" + i.ToString());
      }

      // Проверка с переполнением
      for (int i = 1; i <= 3; i++)
      {
        string value = "Y" + StdConvert.ToString(i);
        sut.AddToHist("ABC", value);
        lst.Insert(0, value);
        lst.RemoveAt(lst.Count - 1);

        HistoryList res = sut.GetHist("ABC");
        CollectionAssert.AreEqual(lst, res, "Replace #" + i.ToString());
      }
    }

    [Test]
    public void AddToHist_3args()
    {
      CfgPart sut = Create();

      List<string> lst = new List<string>();

      for (int i = 1; i <= 8; i++)
      {
        string value = "X" + StdConvert.ToString(i);
        sut.AddToHist("ABC", value, 5);

        lst.Insert(0, value);
        if (i > 5)
          lst.RemoveAt(5);

        HistoryList res = sut.GetHist("ABC");
        CollectionAssert.AreEqual(lst, res, "#" + i.ToString());
      }
    }

    #endregion

    #region GetChild()

    public void GetChild()
    {
      CfgPart sut = Create();

      CfgPart res1 = sut.GetChild("AAA", false);
      Assert.IsNull(res1, "#1");
      Assert.IsFalse(sut.HasChild("AAA"), "HasChild() #1");

      CfgPart res2 = sut.GetChild("BBB", true);
      Assert.IsNotNull(res2, "#2");
      Assert.IsTrue(sut.HasChild("BBB"), "HasChild() #2");

      CfgPart res3 = sut.GetChild("BBB", false);
      // Assert.AreSame(res2, res3); // зависит от реализации
      Assert.IsNotNull(res3, "#3");

      CfgPart res4 = sut.GetChild("BBB", true);
      Assert.IsNotNull(res4, "#4");
    }

    #endregion

    #region GetChildNames(), GetValueNames()

    [Test]
    public void GetChildNames()
    {
      CfgPart sut = Create();
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetChildNames(), "#1");
      InitChildrenAndValues(sut);
      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2" }, sut.GetChildNames(), "#2");
    }

    [Test]
    public void GetValueNames()
    {
      CfgPart sut = Create();
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetValueNames(), "#1");
      InitChildrenAndValues(sut);
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value2", "Value3" }, sut.GetValueNames(), "#2");
    }

    [Test]
    public void GetChildAndValueNames()
    {
      CfgPart sut = Create();
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetChildAndValueNames(), "#1");
      InitChildrenAndValues(sut);
      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2", "Value1", "Value2", "Value3" }, sut.GetChildAndValueNames(), "#2");
    }

    protected static void InitChildrenAndValues(CfgPart sut)
    {
      CfgPart p1 = sut.GetChild("Sect1", true);
      CfgPart p2 = sut.GetChild("Sect2", true);
      sut.SetString("Value1", "AAA");
      sut.SetString("Value2", "BBB");
      sut.SetString("Value3", "CCC");

      p1.SetString("Value11", "DDD");
      p1.SetString("Value12", "EEE");
      p2.SetString("Value21", "FFF");
      CfgPart p11 = p1.GetChild("Sect11", true);
      p11.SetString("Value111", "GGG");
    }

    #endregion

    #region HasChild(), HasValue()

    [TestCase("Sect1", true)]
    [TestCase("Value1", false)]
    [TestCase("XXX", false)]
    [TestCase("", false)]
    public void HasChild(string name, bool wantedRes)
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      bool res = sut.HasChild(name);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("Sect1", false)]
    [TestCase("Value1", true)]
    [TestCase("XXX", false)]
    [TestCase("", false)]
    public void HasValue(string name, bool wantedRes)
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      bool res = sut.HasValue(name);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Remove(), Clear()

    [Test]
    public void Remove_child()
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      sut.Remove("Sect1");

      CollectionAssert.AreEquivalent(new string[] { "Sect2" }, sut.GetChildNames(), "ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value2", "Value3" }, sut.GetValueNames(), "ValueNames");
    }

    [Test]
    public void Remove_value()
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      sut.Remove("Value2");

      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2" }, sut.GetChildNames(), "ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value3" }, sut.GetValueNames(), "ValueNames");
    }

    [Test]
    public void Remove_unknown_name()
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      sut.Remove("XXX");

      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2" }, sut.GetChildNames(), "ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value2", "Value3" }, sut.GetValueNames(), "ValueNames");
    }

    [Test]
    public void Clear()
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      sut.Clear();

      CollectionAssert.AreEquivalent(new string[] { }, sut.GetChildNames(), "ChildNames");
      CollectionAssert.AreEquivalent(new string[] { }, sut.GetValueNames(), "ValueNames");
    }

    #endregion

    #region CopyTo()

    [Test]
    public void CopyTo()
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);

      #region Заполнение секции dest

      CfgPart dest = Create();
      dest.SetString("Value3", "VVV"); // Такое имя есть, но со значением "CCC"
      dest.SetString("Value4", "WWW"); // Такого имени нет

      CfgPart p2 = dest.GetChild("Sect2", true); // такая секция есть
      p2.SetString("Value21", "XXX"); // Такое имя есть, но со значением "FFF"
      p2.SetString("Value22", "YYY"); // Такого имени нет
      CfgPart p21 = p2.GetChild("Sect21", true); // такой секции нет
      p21.SetString("Value211", "UUU");

      CfgPart p3 = dest.GetChild("Sect3", true); // такой секции нет
      p3.SetString("Value31", "ZZZ");

      #endregion

      sut.CopyTo(dest);

      #region Проверка dest

      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2", "Sect3" }, dest.GetChildNames(), "root ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value2", "Value3", "Value4" }, dest.GetValueNames(), "root ValueNames");
      Assert.AreEqual("AAA", dest.GetString("Value1"), "Value1");
      Assert.AreEqual("BBB", dest.GetString("Value2"), "Value2");
      Assert.AreEqual("CCC", dest.GetString("Value3"), "Value3");
      Assert.AreEqual("WWW", dest.GetString("Value4"), "Value4");

      CfgPart p1 = dest.GetChild("Sect1", false);
      Assert.IsNotNull(p1, "Sect1");
      CollectionAssert.AreEquivalent(new string[] { "Sect11" }, p1.GetChildNames(), "Sect1 ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value11", "Value12" }, p1.GetValueNames(), "Sect1 ValueNames");
      Assert.AreEqual("DDD", p1.GetString("Value11"), "Value11");
      Assert.AreEqual("EEE", p1.GetString("Value12"), "Value12");

      CfgPart p11 = p1.GetChild("Sect11", false);
      Assert.IsNotNull(p11, "Sect11");
      CollectionAssert.AreEquivalent(new string[] { }, p11.GetChildNames(), "Sect11 ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value111" }, p11.GetValueNames(), "Sect11 ValueNames");
      Assert.AreEqual("GGG", p11.GetString("Value111"), "Value111");

      p2 = dest.GetChild("Sect2", false);
      Assert.IsNotNull(p2, "Sect2");
      CollectionAssert.AreEquivalent(new string[] { "Sect21" }, p2.GetChildNames(), "Sect2 ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value21", "Value22" }, p2.GetValueNames(), "Sect2 ValueNames");
      Assert.AreEqual("FFF", p2.GetString("Value21"), "Value21");
      Assert.AreEqual("YYY", p2.GetString("Value22"), "Value22");

      p21 = p2.GetChild("Sect21", false);
      Assert.IsNotNull(p21, "Sect21");
      CollectionAssert.AreEquivalent(new string[] { }, p21.GetChildNames(), "Sect21 ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value211" }, p21.GetValueNames(), "Sect21 ValueNames");
      Assert.AreEqual("UUU", p21.GetString("Value211"), "Value211");

      p3 = dest.GetChild("Sect3", false);
      Assert.IsNotNull(p3, "Sect3");
      CollectionAssert.AreEquivalent(new string[] { }, p3.GetChildNames(), "Sect3 ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value31" }, p3.GetValueNames(), "Sect3 ValueNames");
      Assert.AreEqual("ZZZ", p3.GetString("Value31"), "Value31");

      #endregion
    }

    #endregion

    #region Прочие методы и свойства

    [Test]
    public void MD5Sum()
    {
      CfgPart sut = Create();
      string res1 = sut.MD5Sum();
      InitChildrenAndValues(sut);
      string res2 = sut.MD5Sum();

      Assert.AreEqual(32, res1.Length, "#1");
      Assert.AreEqual(32, res2.Length, "#2");
      Assert.AreNotEqual(res1, res2, "Difference");
    }

    public void GetXmlText()
    {
      CfgPart sut = Create();
      InitChildrenAndValues(sut);
      string res = sut.GetXmlText();

      TempCfg cfg2 = new TempCfg();
      cfg2.AsXmlText = res;
      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2" }, cfg2.GetChildNames(), "ChildNames");
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value2", "Value3" }, cfg2.GetValueNames(), "ValueNames");
      // Проверяем, что все данные передались, включая вложенные узлы
      Assert.AreEqual("AAA", cfg2.GetString("Value1"));
      Assert.AreEqual("BBB", cfg2.GetString("Value2"));
      Assert.AreEqual("CCC", cfg2.GetString("Value3"));

      CfgPart p1 = cfg2.GetChild("Sect1", false);
      CfgPart p2 = cfg2.GetChild("Sect2", false);
      CfgPart p11 = p1.GetChild("Sect11", false);

      Assert.AreEqual("DDD", p1.GetString("Value11"));
      Assert.AreEqual("EEE", p1.GetString("Value12"));
      Assert.AreEqual("FFF", p2.GetString("Value21"));
      Assert.AreEqual("GGG", p11.GetString("Value111"));
    }

    [Test]
    public void IsEmpty()
    {
      CfgPart sut = Create();
      Assert.IsTrue(sut.IsEmpty, "#1");

      InitChildrenAndValues(sut);
      Assert.IsFalse(sut.IsEmpty, "#1");

      sut.Clear();
      Assert.IsTrue(sut.IsEmpty, "#3");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.IsTrue(CfgPart.Empty.IsEmpty, "IsEmpty");
      CollectionAssert.AreEqual(DataTools.EmptyStrings, CfgPart.Empty.GetChildNames(), "GetChildNames()");
      CollectionAssert.AreEqual(DataTools.EmptyStrings, CfgPart.Empty.GetValueNames(), "GetValueNames()");
      Assert.AreEqual("", CfgPart.Empty.GetString("ABC"), "GetString()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { CfgPart.Empty.SetString("ABC", "XXX"); }, "SetString()");

      Assert.IsNull(CfgPart.Empty.GetChild("Sect1", false), "GetChild(false)");
      Assert.Catch<ObjectReadOnlyException>(delegate () { CfgPart.Empty.GetChild("Sect1", true); }, "GetChild(true)");

      CfgPart source = Create();
      InitChildrenAndValues(source);
      Assert.Catch<ObjectReadOnlyException>(delegate () { source.CopyTo(CfgPart.Empty); }, "CopyTo()");

      Assert.AreSame(CfgConverter.Default, CfgPart.Empty.Converter, "Converter");
    }

    #endregion


    /// <summary>
    /// Исключительно, чтобы был отдельный класс
    /// </summary>
    protected class DummyConverter : CfgConverter
    {
    }
  }

  public class XmlCfgFileTests : CfgPartTests
  {
    #region Временный файл

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();
      _TempDir = new TempDirectory();
      _File = new XmlCfgFile(new AbsPath(_TempDir.Dir, "test.xml"));
    }

    protected override void OnOneTimeTearDown()
    {
      if (_TempDir != null)
        _TempDir.Dispose();
      base.OnOneTimeTearDown();
    }

    private TempDirectory _TempDir;

    private XmlCfgFile _File;

    int _Count;

    protected override CfgPart Create()
    {
      _Count++;
      return _File.GetChild("Test" + StdConvert.ToString(_Count), true);
    }

    #endregion

    #region Конструктор

    private AbsPath GetRandomFilePath()
    {
      string fileName = Guid.NewGuid().ToString("N");
      return new AbsPath(_TempDir.Dir, fileName + ".xml");
    }

    [Test]
    public void Constructor_1arg_noFile()
    {
      AbsPath path = GetRandomFilePath();
      XmlCfgFile sut = new XmlCfgFile(path);

      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(path, sut.FilePath);
      Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
      Assert.AreSame(XmlCfgFile.DefaultEncoding, sut.Encoding, "Encoding");
      Assert.IsNotNull(sut.Document, "Document");
    }

    [Test]
    public void Constructor_1arg_withFile()
    {
      const string CRLF = "\r\n";
      string contents =
        "<?xml version=\"1.0\" encoding=\"windows-1251\"?>" + CRLF +
        "<Config>" + CRLF +
        "  <Value1>абвгд</Value1>" + CRLF +
        "  <Sect1>" + CRLF +
        "     <Value11>еёжзи</Value11>" + CRLF +
        "  </Sect1>" + CRLF +
        "</Config>" + CRLF;

      AbsPath path = GetRandomFilePath();
      System.IO.File.WriteAllText(path.Path, contents, System.Text.Encoding.GetEncoding(1251));

      XmlCfgFile sut = new XmlCfgFile(path);

      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(path, sut.FilePath);
      Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
      Assert.AreEqual(1251, sut.Encoding.CodePage, "Encoding");
      Assert.IsNotNull(sut.Document, "Document");
      Assert.AreEqual("абвгд", sut.GetString("Value1"), "Value1");
      CfgPart p1 = sut.GetChild("Sect1", false);
      Assert.IsNotNull(p1, "Sect1");
      Assert.AreEqual("еёжзи", p1.GetString("Value11"), "Value11");
    }

    [Test]
    public void Constructor_2args()
    {
      AbsPath path = GetRandomFilePath();
      CfgConverter converter = new DummyConverter();
      XmlCfgFile sut = new XmlCfgFile(path, converter);
      Assert.AreSame(converter, sut.Converter, "Converter #1");

      CfgPart p1 = sut.GetChild("Sect1", true);
      Assert.AreSame(converter, p1.Converter, "Converter #2");
    }

    #endregion

    #region DefaultEncoding

    [Test]
    public void DefaultEncoding()
    {
      Assert.AreSame(System.Text.Encoding.UTF8, XmlCfgFile.DefaultEncoding);
      // установка свойства не проверяется
    }

    #endregion

    #region Save()

    [Test]
    public void Save()
    {
      AbsPath path = GetRandomFilePath();
      XmlCfgFile sut = new XmlCfgFile(path);
      sut.SetString("Value1", "AAA");
      CfgPart p1 = sut.GetChild("Sect1", true);
      p1.SetString("Value11", "BBB");

      sut.Save();
      sut = null;
      Assert.IsTrue(System.IO.File.Exists(path.Path), "File exists");

      XmlCfgFile res = new XmlCfgFile(path);
      Assert.AreSame(System.Text.Encoding.UTF8, res.Encoding, "Encoding");
      Assert.AreEqual("AAA", res.GetString("Value1"), "Value1");
      p1 = res.GetChild("Sect1", false);
      Assert.IsNotNull(p1, "Sect1");
      Assert.AreEqual("BBB", p1.GetString("Value11"), "Value11");
    }

    #endregion

    #region Encoding

    [Test]
    public void Encoding()
    {
      AbsPath path = GetRandomFilePath();
      XmlCfgFile sut = new XmlCfgFile(path);
      sut.SetString("Value1", "абвг");
      CfgPart p1 = sut.GetChild("Sect1", true);
      p1.SetString("Value11", "деёж");

      sut.Encoding = System.Text.Encoding.GetEncoding(1251);
      Assert.AreEqual(1251, sut.Encoding.CodePage, "Encoding #1");

      sut.Save();
      sut = null;
      Assert.IsTrue(System.IO.File.Exists(path.Path), "File exists");

      XmlCfgFile res = new XmlCfgFile(path);
      Assert.AreEqual(1251, res.Encoding.CodePage, "Encoding #2");

      Assert.AreEqual("абвг", res.GetString("Value1"), "Value1");
      p1 = res.GetChild("Sect1", false);
      Assert.IsNotNull(p1, "Sect1");
      Assert.AreEqual("деёж", p1.GetString("Value11"), "Value11");
    }

    #endregion

    #region AsXmlText

    [Test]
    public void AsXmlText()
    {
      AbsPath path = GetRandomFilePath();
      XmlCfgFile sut = new XmlCfgFile(path);
      sut.SetString("Value1", "AAA");
      CfgPart p1 = sut.GetChild("Sect1", true);
      p1.SetString("Value11", "BBB");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty #1");
      string s1 = sut.AsXmlText;

      sut.AsXmlText = String.Empty;
      Assert.IsTrue(sut.IsEmpty, "IsEmpty #2");

      // Проверяем работоспособность после очистки
      sut.SetString("Value2", "BBB");
      Assert.AreEqual("BBB", sut.GetString("Value2"), "Value2 #2");

      sut.AsXmlText = s1;
      Assert.IsFalse(sut.IsEmpty, "IsEmpty #3");
      Assert.AreEqual("AAA", sut.GetString("Value1"), "Value1");
      p1 = sut.GetChild("Sect1", false);
      Assert.IsNotNull(p1, "Sect1");
      Assert.AreEqual("BBB", p1.GetString("Value11"), "Value11");
      Assert.AreEqual("", sut.GetString("Value2"), "Value2 #3");
    }

    #endregion

    #region PartAsXmlText

    [Test]
    public void PartAsXmlText()
    {
      AbsPath path = GetRandomFilePath();
      XmlCfgFile sut = new XmlCfgFile(path);
      sut.SetString("Value1", "AAA");
      XmlCfgPart p1 = sut.GetChild("Sect1", true) as XmlCfgPart;
      p1.SetString("Value11", "BBB");
      CfgPart p11 = p1.GetChild("Sect11", true);
      p11.SetString("Value111", "CCC");
      string s1 = p1.PartAsXmlText;

      XmlCfgPart p2 = sut.GetChild("Sect2", true) as XmlCfgPart;
      p2.SetString("Value21", "DUMMY");
      CfgPart dummyP21 = p2.GetChild("Sect21", true);
      dummyP21.SetString("Value211", "DUMMY");


      p2.PartAsXmlText = s1; // Скопировали из другой секции

      PartAsXmlText_testResults(sut);
    }

    private static void PartAsXmlText_testResults(XmlCfgFile sut)
    {
      // выделено в отдельный метод, чтобы имена переменных не мешались

      CollectionAssert.AreEquivalent(new string[] { "Value1" }, sut.GetValueNames(), "root ValueNames");
      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2" }, sut.GetChildNames(), "root ChildNames");
      Assert.AreEqual("AAA", sut.GetString("Value1"), "Value1");

      CfgPart p1 = sut.GetChild("Sect1", false);
      CollectionAssert.AreEquivalent(new string[] { "Value11" }, p1.GetValueNames(), "Sect1 ValueNames");
      CollectionAssert.AreEquivalent(new string[] { "Sect11" }, p1.GetChildNames(), "Sect1 ChildNames");
      Assert.AreEqual("BBB", p1.GetString("Value11"), "Sect1/Value11");

      CfgPart p11 = p1.GetChild("Sect11", false);
      CollectionAssert.AreEquivalent(new string[] { "Value111" }, p11.GetValueNames(), "Sect1/Sect11 ValueNames");
      CollectionAssert.AreEquivalent(new string[] { }, p11.GetChildNames(), "Sect1/Sect11 ChildNames");
      Assert.AreEqual("CCC", p11.GetString("Value111"), "Sect1/Sect11/Value111");

      CfgPart p2 = sut.GetChild("Sect2", false);
      CollectionAssert.AreEquivalent(new string[] { "Value11" }, p2.GetValueNames(), "Sect2 ValueNames");
      CollectionAssert.AreEquivalent(new string[] { "Sect11" }, p2.GetChildNames(), "Sect2 ChildNames");
      Assert.AreEqual("BBB", p2.GetString("Value11"), "Sect2/Value11");

      CfgPart p21 = p2.GetChild("Sect11", false);
      CollectionAssert.AreEquivalent(new string[] { "Value111" }, p21.GetValueNames(), "Sect2/Sect11 ValueNames");
      CollectionAssert.AreEquivalent(new string[] { }, p21.GetChildNames(), "Sect2/Sect11 ChildNames");
      Assert.AreEqual("CCC", p21.GetString("Value111"), "Sect2/Sect11/Value111");
    }

    #endregion

    // Событие OnChanged не тестируем, т.к. м.б. изменено в будущем
  }

  [Platform("Win")]
  public class RegistryCfgTests : CfgPartTests
  {
    #region Объект доступа к реестру

    const string RootPath = @"HKEY_CURRENT_USER\Software\FreeLibSet\Tests";

    private string TestSubKey;

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      TestSubKey = Guid.NewGuid().ToString("B");
      _Object = new RegistryCfg(RootPath + "\\" + TestSubKey);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Object != null)
      {
        _Object.Clear();
        try { _Object.Tree.DeleteTree(_Object.KeyName); }
        catch { }
        _Object.Dispose();
      }

      base.OnOneTimeTearDown();
    }

    private RegistryCfg _Object;

    int _Count;

    protected override CfgPart Create()
    {
      _Count++;
      return _Object.GetChild("Test" + StdConvert.ToString(_Count), true);
    }

    #endregion

    #region Конструкторы

    [Test]
    public void Constructor_1arg_empty()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      using (RegistryCfg sut = new RegistryCfg(keyName))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        sut.Tree.DeleteTree(keyName);
      }
    }

    [Test]
    public void Constructor_1arg_notEmpty()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      Microsoft.Win32.Registry.SetValue(keyName, "Value1", "AAA");
      Microsoft.Win32.Registry.SetValue(keyName + "\\Sect1", "Value11", "BBB");

      using (RegistryCfg sut = new RegistryCfg(keyName))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.IsFalse(sut.IsEmpty, "IsEmpty");

        CollectionAssert.AreEquivalent(new string[] { "Sect1" }, sut.GetChildNames(), "root ChildNames");
        CollectionAssert.AreEquivalent(new string[] { "Value1" }, sut.GetValueNames(), "root ValueNames");
        Assert.AreEqual("AAA", sut.GetString("Value1"), "Value1");

        CfgPart p1 = sut.GetChild("Sect1", false);
        CollectionAssert.AreEquivalent(new string[] { }, p1.GetChildNames(), "root ChildNames");
        CollectionAssert.AreEquivalent(new string[] { "Value11" }, p1.GetValueNames(), "root ValueNames");
        Assert.AreEqual("BBB", p1.GetString("Value11"), "Value11");
        sut.Tree.DeleteTree(keyName);
      }
    }

    [Test]
    public void Consructor_2args_readonly_exist()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      Microsoft.Win32.Registry.SetValue(keyName, "Value1", "AAA");
      Microsoft.Win32.Registry.SetValue(keyName + "\\Sect1", "Value11", "BBB");
      using (RegistryCfg sut = new RegistryCfg(keyName, true))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.IsEmpty, "IsEmpty");
        Assert.IsTrue(sut.Tree.IsReadOnly, "Tree.IsReadOnly");

        // Не проверяем на ObjectReadOnlyException, т.к. исключение выбрасывается кодом Net Framework
        Assert.Catch(delegate () { sut.SetString("Value2", "BBB"); }, "SetString()");
        Assert.Catch(delegate () { sut.GetChild("Sect2", true); }, "GetChild(true)");
        Assert.Catch(delegate () { sut.Remove("Value1"); }, "Remove() value");
        Assert.Catch(delegate () { sut.Remove("Sect1"); }, "Remove() section");
        Assert.Catch(delegate () { sut.Clear(); }, "Clear()");

        // Тут нельзя
        // sut.Tree.DeleteTree(keyName);
      }

      using (FreeLibSet.Win32.RegistryTree tree = new FreeLibSet.Win32.RegistryTree())
      {
        tree.DeleteTree(keyName);
      }
    }


    [Test]
    public void Consructor_2args_readonly_noRegistryKey()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B"); // несуществующий раздел
      using (RegistryCfg sut = new RegistryCfg(keyName, true))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        Assert.IsTrue(sut.Tree.IsReadOnly, "Tree.IsReadOnly");

        Assert.AreEqual("", sut.GetString("ABC"), "GetString()");
      }

      using (FreeLibSet.Win32.RegistryTree tree = new FreeLibSet.Win32.RegistryTree())
      {
        Assert.IsFalse(tree.Exists(keyName), "RegistryKey existance");
      }
    }

    public void Consructor_3args()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      DummyConverter converter = new DummyConverter();
      using (RegistryCfg sut = new RegistryCfg(keyName, false, converter))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(converter, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        sut.Tree.DeleteTree(keyName);
      }
    }

    #endregion
  }

  [Platform("Win")]
  public class RegistryCfg2Tests : CfgPartTests
  {
    #region Объект доступа к реестру

    const string RootPath = @"HKEY_CURRENT_USER\Software\FreeLibSet\Tests";

    private string TestSubKey;

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      TestSubKey = Guid.NewGuid().ToString("B");
      _Object = new RegistryCfg2(RootPath + "\\" + TestSubKey, false, FreeLibSet.Win32.RegistryView2.Default);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Object != null)
      {
        _Object.Clear();
        _Object.Tree.DeleteTree(_Object.KeyName);
        _Object.Dispose();
      }

      base.OnOneTimeTearDown();
    }

    private RegistryCfg2 _Object;

    int _Count;

    protected override CfgPart Create()
    {
      _Count++;
      return _Object.GetChild("Test" + StdConvert.ToString(_Count), true);
    }

    #endregion

    #region Конструкторы

    [Test]
    public void Constructor_1arg_empty()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      using (RegistryCfg2 sut = new RegistryCfg2(keyName))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.AreEqual(GetDefaultView(), sut.Tree.View, "Tree.View");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        sut.Tree.DeleteTree(keyName);
      }
    }

    [Test]
    public void Constructor_1arg_notEmpty()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      Microsoft.Win32.Registry.SetValue(keyName, "Value1", "AAA");
      Microsoft.Win32.Registry.SetValue(keyName + "\\Sect1", "Value11", "BBB");

      using (RegistryCfg2 sut = new RegistryCfg2(keyName))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.AreEqual(GetDefaultView(), sut.Tree.View, "Tree.View");
        Assert.IsFalse(sut.IsEmpty, "IsEmpty");

        CollectionAssert.AreEquivalent(new string[] { "Sect1" }, sut.GetChildNames(), "root ChildNames");
        CollectionAssert.AreEquivalent(new string[] { "Value1" }, sut.GetValueNames(), "root ValueNames");
        Assert.AreEqual("AAA", sut.GetString("Value1"), "Value1");

        CfgPart p1 = sut.GetChild("Sect1", false);
        CollectionAssert.AreEquivalent(new string[] { }, p1.GetChildNames(), "root ChildNames");
        CollectionAssert.AreEquivalent(new string[] { "Value11" }, p1.GetValueNames(), "root ValueNames");
        Assert.AreEqual("BBB", p1.GetString("Value11"), "Value11");
        sut.Tree.DeleteTree(keyName);
      }
    }

    [Test]
    public void Consructor_2args_readonly_exist()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      Microsoft.Win32.Registry.SetValue(keyName, "Value1", "AAA");
      Microsoft.Win32.Registry.SetValue(keyName + "\\Sect1", "Value11", "BBB");
      using (RegistryCfg2 sut = new RegistryCfg2(keyName, true))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.AreEqual(GetDefaultView(), sut.Tree.View, "Tree.View");
        Assert.IsFalse(sut.IsEmpty, "IsEmpty");
        Assert.IsTrue(sut.Tree.IsReadOnly, "Tree.IsReadOnly");

        Assert.Catch(delegate () { sut.SetString("Value2", "BBB"); }, "SetString()");
        Assert.Catch(delegate () { sut.GetChild("Sect2", true); }, "GetChild(true)");
        Assert.Catch(delegate () { sut.Remove("Value1"); }, "Remove() value");
        Assert.Catch(delegate () { sut.Remove("Sect1"); }, "Remove() section");
        Assert.Catch(delegate () { sut.Clear(); }, "Clear()");

        // Тут нельзя
        // sut.Tree.DeleteTree(keyName);
      }

      using (FreeLibSet.Win32.RegistryTree2 tree = new FreeLibSet.Win32.RegistryTree2())
      {
        tree.DeleteTree(keyName);
      }
    }


    [Test]
    public void Consructor_2args_readonly_noRegistryKey()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B"); // несуществующий раздел
      using (RegistryCfg2 sut = new RegistryCfg2(keyName, true))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        Assert.IsTrue(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.AreEqual(GetDefaultView(), sut.Tree.View, "Tree.View");

        Assert.AreEqual("", sut.GetString("ABC"), "GetString()");
      }

      using (FreeLibSet.Win32.RegistryTree2 tree = new FreeLibSet.Win32.RegistryTree2())
      {
        Assert.IsFalse(tree.Exists(keyName), "RegistryKey existance");
      }
    }

    public void Consructor_3args_converter()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      DummyConverter converter = new DummyConverter();
      using (RegistryCfg2 sut = new RegistryCfg2(keyName, false, converter))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(converter, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.AreEqual(GetDefaultView(), sut.Tree.View, "Tree.View");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        sut.Tree.DeleteTree(keyName);
      }
    }


    [Test]
    [Platform("Win32NT")]
    public void Constructor_3args_view()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      using (RegistryCfg2 sut = new RegistryCfg2(keyName, false, FreeLibSet.Win32.RegistryView2.Registry32))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.AreEqual(FreeLibSet.Win32.RegistryView2.Registry32, sut.Tree.View, "Tree.View");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        sut.Tree.DeleteTree(keyName);
      }
    }


    [Test]
    [Platform("Win32NT")]
    public void Constructor_4args()
    {
      string keyName = RootPath + "\\" + Guid.NewGuid().ToString("B");
      DummyConverter converter = new DummyConverter();
      using (RegistryCfg2 sut = new RegistryCfg2(keyName, false, FreeLibSet.Win32.RegistryView2.Registry32, converter))
      {
        Assert.AreEqual(keyName, sut.KeyName, "KeyName");
        Assert.AreSame(converter, sut.Converter, "Converter");
        Assert.IsNotNull(sut.Tree, "Tree");
        Assert.IsFalse(sut.Tree.IsReadOnly, "Tree.IsReadOnly");
        Assert.AreEqual(FreeLibSet.Win32.RegistryView2.Registry32, sut.Tree.View, "Tree.View");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
        sut.Tree.DeleteTree(keyName);
      }
    }

    private static FreeLibSet.Win32.RegistryView2 GetDefaultView()
    {
      if (EnvironmentTools.Is64BitOperatingSystem)
        return FreeLibSet.Win32.RegistryView2.Registry64;
      else
        return FreeLibSet.Win32.RegistryView2.Registry32;
    }

    #endregion
  }

  public class TempCfgTests : CfgPartTests
  {
    #region Создание объекта

    protected override CfgPart Create()
    {
      return new TempCfg();
    }

    #endregion

    #region Конструкторы

    [Test]
    public void Constructor_0args()
    {
      TempCfg sut = new TempCfg();
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
      Assert.IsNotNull(sut.Document, "Document");
      Assert.IsNull(sut.Parent, "Parent");
    }

    [Test]
    public void Constructor_1args()
    {
      DummyConverter converter = new DummyConverter();
      TempCfg sut = new TempCfg(converter);
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreSame(converter, sut.Converter, "Converter");
      Assert.IsNotNull(sut.Document, "Document");
      Assert.IsNull(sut.Parent, "Parent");
    }

    #endregion

    #region AsXmlText

    [Test]
    public void AsXmlText()
    {
      const string CRLF = "\r\n";
      string contents =
        "<?xml version=\"1.0\"?>" + CRLF +
        "<Config>" + CRLF +
        "  <Value1>AAA</Value1>" + CRLF +
        "  <Sect1>" + CRLF +
        "     <Value11>BBB</Value11>" + CRLF +
        "  </Sect1>" + CRLF +
        "</Config>" + CRLF;

      TempCfg sut = new TempCfg();
      sut.AsXmlText = contents;


      Assert.AreEqual("AAA", sut.GetString("Value1"), "Value1");
      CfgPart p1 = sut.GetChild("Sect1", false);
      Assert.IsNotNull(p1, "Sect1");
      Assert.AreEqual("BBB", p1.GetString("Value11"), "Value11");

      sut.AsXmlText = String.Empty;
      Assert.IsTrue(sut.IsEmpty, "IsEmpty #2");
    }

    #endregion

    #region Прочие свойства

    [Test]
    public void Parent()
    {
      TempCfg sut = new TempCfg();
      XmlCfgPart p1 = sut.GetChild("Sect1", true) as XmlCfgPart;
      Assert.AreSame(sut, p1.Parent, "Parent");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      TempCfg sut = new TempCfg();
      InitChildrenAndValues(sut);

      TempCfg res = sut.Clone();
      CollectionAssert.AreEquivalent(new string[] { "Sect1", "Sect2" }, res.GetChildNames(), "GetChildNames()");
      CollectionAssert.AreEquivalent(new string[] { "Value1", "Value2", "Value3" }, res.GetValueNames(), "GetValueNames()");
    }

    #endregion
  }

  public class IniFileCfgTests : CfgPartTests
  {
    #region Файл

    private IniFile _File;

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();
      _File = new IniFile();
    }


    int _Count;

    protected override CfgPart Create()
    {
      _Count++;
      return new IniCfgPart(_File, "Test" + StdConvert.ToString(_Count));
    }

    #endregion

    #region Конструкторы

    [Test]
    public void Constructor_2args()
    {
      IniCfgPart sut = new IniCfgPart(_File, "Sect1\\Sect2");
      Assert.AreSame(_File, sut.File, "File");
      Assert.AreEqual("Sect1\\Sect2", sut.SectionName, "SectionName");
      Assert.AreSame(CfgConverter.Default, sut.Converter, "Converter");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_3args_converter()
    {
      DummyConverter converter = new DummyConverter();
      IniCfgPart sut = new IniCfgPart(_File, "Sect1", converter);
      Assert.AreSame(_File, sut.File, "File");
      Assert.AreEqual("Sect1", sut.SectionName, "SectionName");
      Assert.AreSame(converter, sut.Converter, "Converter");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    #endregion
  }

  [Platform("Win")]
  public class IniFileWindowsCfgTests : CfgPartTests
  {
    #region Файл

    TempDirectory _Dir;
    private FreeLibSet.Win32.IniFileWindows _File;

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      _Dir = new TempDirectory();
      _File = new FreeLibSet.Win32.IniFileWindows(new AbsPath(_Dir.Dir, "test.ini"));
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Dir != null)
        _Dir.Dispose();

      base.OnOneTimeTearDown();
    }

    int _Count;

    protected override CfgPart Create()
    {
      _Count++;
      return new IniCfgPart(_File, "Test" + StdConvert.ToString(_Count));
    }

    #endregion
  }
}
