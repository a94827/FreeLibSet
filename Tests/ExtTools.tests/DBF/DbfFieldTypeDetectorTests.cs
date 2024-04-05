using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DBF;

namespace ExtTools_tests.DBF
{
  [TestFixture]
  class DbfFieldTypeDetectorTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      // Свойство Name не проверяем, так как его исходное состояние не имеет значения

      Assert.AreEqual(' ', sut.PreliminaryInfo.Type, "PreliminaryInfo.Type");
      Assert.AreEqual(0, sut.PreliminaryInfo.Length, "PreliminaryInfo.Length");
      Assert.IsFalse(sut.PreliminaryInfo.LengthIsDefined, "PreliminaryInfo.LengthIsDefined");
      Assert.AreEqual(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage, sut.Encoding.CodePage, "Encoding");
      Assert.AreEqual(DbfFileFormat.dBase3, sut.Format, "Format");
      Assert.IsTrue(sut.UseMemo, "UseMemo");
      Assert.IsFalse(sut.IsCompleted, "IsCompleted");
      Assert.AreEqual("C1", sut.Result.TypeSizeText, "Result");
    }

    #endregion

    #region PreliminaryInfo

    [TestCase('C', 10, false, 0, false, "C10", false)]
    [TestCase('C', 10, true, 0, false, "C10", true)]
    [TestCase('N', 10, false, 2, false, "N10.2", false)]
    [TestCase('N', 10, false, 2, true, "N10.2", false)]
    [TestCase('N', 10, true, 2, false, "N10.2", false)]
    [TestCase('N', 10, true, 2, true, "N10.2", true)]
    [TestCase('D', 0, false, 0, false, "D", true)]
    [TestCase('L', 0, false, 0, false, "L", true)]
    [TestCase('M', 0, false, 0, false, "M", true)]
    public void PreliminaryInfo(char type, int length, bool lengthIsDefined, int precision, bool precisionIsDefined,
      string wantedTypeSize, bool wantedIsCompleted)
    {
      DbfFieldTypePreliminaryInfo pi = new DbfFieldTypePreliminaryInfo();
      pi.Type = type;
      pi.Length = length;
      pi.LengthIsDefined = lengthIsDefined;
      pi.Precision = precision;
      pi.PrecisionIsDefined = precisionIsDefined;

      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.PreliminaryInfo = pi;

      Assert.AreEqual(sut.Result.TypeSizeText, wantedTypeSize, "TypeSizeText");
      Assert.AreEqual(sut.IsCompleted, wantedIsCompleted, "IsCompleted");
    }

    #endregion

    #region ApplyValue()

    #region String

    [Test]
    public void ApplyValue_String()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая
      sut.ApplyValue("ABCD");
      Assert.AreEqual("C4", sut.Result.TypeSizeText);
    }

    [Test]
    public void ApplyValue_C_String()
    {
      DbfFieldTypePreliminaryInfo pi = new DbfFieldTypePreliminaryInfo();
      pi.Type = 'C';
      pi.Length = 3;
      pi.LengthIsDefined = false;
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.PreliminaryInfo = pi;
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая
      Assert.AreEqual("C3", sut.Result.TypeSizeText, "#0");

      sut.ApplyValue("ABCD");
      Assert.AreEqual("C4", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue("EF");
      Assert.AreEqual("C4", sut.Result.TypeSizeText, "#2");
    }

    [TestCase(true, "M")]
    [TestCase(false, "C65535")]
    public void ApplyValue_LongString(bool useMemo, string wantedRes)
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.UseMemo = useMemo;
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая
      sut.ApplyValue("ABCD");
      Assert.AreEqual("C4", sut.Result.TypeSizeText, "#1");
      Assert.IsFalse(sut.IsCompleted, "IsCompleted #1");

      string longStr = new string('A', 65536);
      sut.ApplyValue(longStr);

      Assert.AreEqual(wantedRes, sut.Result.TypeSizeText, "Result");
      Assert.IsTrue(sut.IsCompleted, "IsCompleted #2");
    }

    #endregion

    #region Date

    [Test]
    public void ApplyValue_Date_String()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(new DateTime(2024, 3, 29));
      Assert.AreEqual("D", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue("ABC");
      Assert.AreEqual("C8", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue("1234567890");
      Assert.AreEqual("C10", sut.Result.TypeSizeText, "#3");
    }

    [Test]
    public void ApplyValue_String_Date()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue("ABC");
      Assert.AreEqual("C3", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(new DateTime(2024, 3, 29));
      Assert.AreEqual("C8", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue("1234567890");
      Assert.AreEqual("C10", sut.Result.TypeSizeText, "#3");
    }

    #endregion

    #region DateTime

    [Test]
    public void ApplyValue_Date_DateTime()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(new DateTime(2024, 3, 29));
      Assert.AreEqual("D", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(new DateTime(2024, 3, 29, 12, 34, 56));
      Assert.AreEqual("C15", sut.Result.TypeSizeText, "#2");
    }

    [Test]
    public void ApplyValue_DateTime_Date()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(new DateTime(2024, 3, 29, 12, 34, 56));
      Assert.AreEqual("C15", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(new DateTime(2024, 3, 29));
      Assert.AreEqual("C15", sut.Result.TypeSizeText, "#2");
    }

    #endregion

    #region Bool

    [Test]
    public void ApplyValue_Bool_String()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(true);
      Assert.AreEqual("L", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue("ABC");
      Assert.AreEqual("C3", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue("1234567890");
      Assert.AreEqual("C10", sut.Result.TypeSizeText, "#3");
    }

    [Test]
    public void ApplyValue_String_Bool()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue("ABC");
      Assert.AreEqual("C3", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(true);
      Assert.AreEqual("C3", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue("1234567890");
      Assert.AreEqual("C10", sut.Result.TypeSizeText, "#3");
    }

    #endregion

    #region Int 

    [Test]
    public void ApplyValue_Int32()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(-123);
      Assert.AreEqual("N4.0", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(12345);
      Assert.AreEqual("N5.0", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue(12);
      Assert.AreEqual("N5.0", sut.Result.TypeSizeText, "#3");
    }

    [Test]
    public void ApplyValue_Int32_Int64()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(-123);
      Assert.AreEqual("N4.0", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(12345L);
      Assert.AreEqual("N5.0", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue(12L);
      Assert.AreEqual("N5.0", sut.Result.TypeSizeText, "#3");
    }

    [Test]
    public void ApplyValue_Int_String()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(-123);
      Assert.AreEqual("N4.0", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue("ABC");
      Assert.AreEqual("C4", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue("1234567890");
      Assert.AreEqual("C10", sut.Result.TypeSizeText, "#3");
    }

    [Test]
    public void ApplyValue_String_Int()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue("ABC");
      Assert.AreEqual("C3", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(-123);
      Assert.AreEqual("C4", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue("1234567890");
      Assert.AreEqual("C10", sut.Result.TypeSizeText, "#3");
    }

    #endregion

    #region Single

    public void ApplyValue_Single()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(-123f);
      Assert.AreEqual("N4.0", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(1.5f);
      Assert.AreEqual("N6.1", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue(12345f);
      Assert.AreEqual("N7.1", sut.Result.TypeSizeText, "#3");

      sut.ApplyValue(0.25f);
      Assert.AreEqual("N8.2", sut.Result.TypeSizeText, "#4");
    }

    #endregion

    #region Double

    public void ApplyValue_Double()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(-123.0);
      Assert.AreEqual("N4.0", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(1.5);
      Assert.AreEqual("N6.1", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue(1234567.0);
      Assert.AreEqual("N9.1", sut.Result.TypeSizeText, "#3");

      sut.ApplyValue(0.25);
      Assert.AreEqual("N10.2", sut.Result.TypeSizeText, "#4");
    }

    #endregion

    #region Decimal

    public void ApplyValue_Decimal()
    {
      DbfFieldTypeDetector sut = new DbfFieldTypeDetector();
      sut.Encoding = Encoding.ASCII; // для определенности, что кодировка однобайтовая

      sut.ApplyValue(-123m);
      Assert.AreEqual("N4.0", sut.Result.TypeSizeText, "#1");

      sut.ApplyValue(1.5m);
      Assert.AreEqual("N6.1", sut.Result.TypeSizeText, "#2");

      sut.ApplyValue(1234567m);
      Assert.AreEqual("N9.1", sut.Result.TypeSizeText, "#3");

      sut.ApplyValue(0.25m);
      Assert.AreEqual("N10.2", sut.Result.TypeSizeText, "#4");
    }

    #endregion

    #endregion
  }
}
