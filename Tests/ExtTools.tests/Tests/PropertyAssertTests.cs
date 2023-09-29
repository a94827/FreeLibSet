using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Tests;

namespace ExtTools_tests.Tests
{
  [TestFixture]
  public class PropertyAssertTests
  {
    private class TestClass1
    {
      public string Prop1 { get { return _Prop1; } set { _Prop1 = value; } }
      private string _Prop1;

      public int Prop2 { get { return _Prop2; } set { _Prop2 = value; } }
      private int _Prop2;
    }

    [Test]
    public void AreEqual_ok()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 123;

      Assert.DoesNotThrow(delegate () { PropertyAssert.AreEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void AreEqual_diff()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 456;


      Assert.Catch<AssertionException>(delegate () { PropertyAssert.AreEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void AreEqual_first_null()
    {

      TestClass1 x1 = null;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 123;

      Assert.Catch<AssertionException>(delegate () { PropertyAssert.AreEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void AreEqual_second_null()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = null;

      Assert.Catch<AssertionException>(delegate () { PropertyAssert.AreEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void AreEqual_both_nulls()
    {
      TestClass1 x1 = null;
      TestClass1 x2 = null;

      Assert.DoesNotThrow(delegate () { PropertyAssert.AreEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void AreEqualExcept_ok()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 456;


      Assert.DoesNotThrow(delegate () { PropertyAssert.AreEqualExcept(x1, x2, new string[] { "Prop2" }, String.Empty); });
    }

    [Test]
    public void AreEqualExcept_diff()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 456;


      Assert.Catch<AssertionException>(delegate () { PropertyAssert.AreEqualExcept(x1, x2, new string[] { "Prop1" }, String.Empty); });
    }
  }
}
