using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using AgeyevAV;

namespace ExtTools.tests
{
  /// <summary>
  /// Object comparer with public properties inspection
  /// </summary>
  public static class PropertyAssert
  {
    #region ProperiesEqual()

    /// <summary>
    /// ���������, ��� ������� ����� ���������� ��� ��� ��� ����������� ������� ��������� �� ���� ���������� �������.
    /// ����� ����������� �������� ���� public-�������.
    /// ����������� �������� �� �����������.
    /// ������������� �� �����������.
    /// </summary>
    /// <param name="expected">��������� ������</param>
    /// <param name="actual">���������� ������</param>
    /// <param name="message">���������</param>
    /// <param name="args">��������� ��� �������������� ���������</param>
    public static void ProperiesEqual(object expected, object actual)
    {
      ProperiesEqual(expected, actual, String.Empty, DataTools.EmptyObjects);
    }

    /// <summary>
    /// ���������, ��� ������� ����� ���������� ��� ��� ��� ����������� ������� ��������� �� ���� ���������� �������.
    /// ����� ����������� �������� ���� public-�������.
    /// ����������� �������� �� �����������.
    /// ������������� �� �����������.
    /// </summary>
    /// <param name="expected">��������� ������</param>
    /// <param name="actual">���������� ������</param>
    /// <param name="message">���������</param>
    public static void ProperiesEqual(object expected, object actual, string message)
    {
      ProperiesEqual(expected, actual, message, DataTools.EmptyObjects);
    }

    /// <summary>
    /// ���������, ��� ������� ����� ���������� ��� ��� ��� ����������� ������� ��������� �� ���� ���������� �������.
    /// ����� ����������� �������� ���� public-�������.
    /// ����������� �������� �� �����������.
    /// ������������� �� �����������.
    /// </summary>
    /// <param name="expected">��������� ������</param>
    /// <param name="actual">���������� ������</param>
    /// <param name="message">���������</param>
    /// <param name="args">��������� ��� �������������� ���������</param>
    public static void ProperiesEqual(object expected, object actual, string message, params object[] args)
    {
      ProperiesEqualExcept(expected, actual, DataTools.EmptyStrings, message, args);
    }

    #endregion

    #region ProperiesEqualExcept()

    /// <summary>
    /// ���������, ��� ������� ����� ���������� ��� ��� ��� ����������� ������� ��������� �� ���� ���������� �������.
    /// ����� ����������� �������� ���� public-�������, �� ����������� ������������.
    /// ����������� �������� �� �����������.
    /// ������������� �� �����������.
    /// </summary>
    /// <param name="expected">��������� ������</param>
    /// <param name="actual">���������� ������</param>
    /// <param name="exceptedProperties">����� �������, ������� �� ���� ����������. ����� ���� null</param>
    /// <param name="message">���������</param>
    /// <param name="args">��������� ��� �������������� ���������</param>
    public static void ProperiesEqualExcept(object expected, object actual, string[] exceptedProperties)
    {
      ProperiesEqualExcept(expected, actual, exceptedProperties, String.Empty, DataTools.EmptyObjects);
    }

    /// <summary>
    /// ���������, ��� ������� ����� ���������� ��� ��� ��� ����������� ������� ��������� �� ���� ���������� �������.
    /// ����� ����������� �������� ���� public-�������, �� ����������� ������������.
    /// ����������� �������� �� �����������.
    /// ������������� �� �����������.
    /// </summary>
    /// <param name="expected">��������� ������</param>
    /// <param name="actual">���������� ������</param>
    /// <param name="exceptedProperties">����� �������, ������� �� ���� ����������. ����� ���� null</param>
    /// <param name="message">���������</param>
    public static void ProperiesEqualExcept(object expected, object actual, string[] exceptedProperties, string message)
    {
      ProperiesEqualExcept(expected, actual, exceptedProperties, message, DataTools.EmptyObjects);
    }

    /// <summary>
    /// ���������, ��� ������� ����� ���������� ��� ��� ��� ����������� ������� ��������� �� ���� ���������� �������.
    /// ����� ����������� �������� ���� public-�������, �� ����������� ������������.
    /// ����������� �������� �� �����������.
    /// ������������� �� �����������.
    /// </summary>
    /// <param name="expected">��������� ������</param>
    /// <param name="actual">���������� ������</param>
    /// <param name="exceptedProperties">����� �������, ������� �� ���� ����������. ����� ���� null</param>
    /// <param name="message">���������</param>
    /// <param name="args">��������� ��� �������������� ���������</param>
    public static void ProperiesEqualExcept(object expected, object actual, string[] exceptedProperties, string message, params object[] args)
    {
      if (Object.ReferenceEquals(expected, actual))
        return;
      if (expected == null || actual == null)
        Assert.AreEqual(expected, actual, message, args); // ����� �������� ����������

      Type t1 = expected.GetType();
      Type t2 = actual.GetType();
      if (!Object.ReferenceEquals(t1, t2))
      {
        if (!t2.IsSubclassOf(t1))
          Assert.AreEqual(t1, t2, message + ". Types are different", args);
      }

      if (exceptedProperties == null)
        exceptedProperties = DataTools.EmptyStrings;
      StringArrayIndexer exPropIndexer = new StringArrayIndexer(exceptedProperties);

      PropertyInfo[] aProps = t1.GetProperties(BindingFlags.Instance | BindingFlags.Public);
      for (int i = 0; i < aProps.Length; i++)
      {
        if (exPropIndexer.Contains(aProps[i].Name))
          continue;

        object v1, v2;
        Exception ex1, ex2;
        GetProperty(aProps[i], expected, out v1, out ex1);
        GetProperty(aProps[i], actual, out v2, out ex2);

        if (ex1 == null && ex2 == null)
          Assert.AreEqual(v1, v2, message + "." + aProps[i].Name, args);
        else
        {
          if (ex1 != null && ex2 != null)
            continue;
          if (ex1 != null)
            Assert.Fail(message + ". Exception for property " + aProps[i].Name + " in expected object: " + ex1.Message, args);
          else
            Assert.Fail(message + ". Exception for property " + aProps[i].Name + " in actual object: " + ex2.Message, args);
        }
      }

    #endregion
    }


    [DebuggerStepThrough]
    private static void GetProperty(PropertyInfo propertyInfo, object obj, out object v, out Exception ex)
    {
#if DEBUG
      if (propertyInfo == null)
        throw new ArgumentNullException("propertyInfo");
      if (obj==null)
        throw new ArgumentNullException("obj");
#endif

      v = null;
      ex = null;
      try
      {
        v = propertyInfo.GetValue(obj, DataTools.EmptyObjects);
      }
      catch (Exception e)
      {
        ex = e;
      }
    }


#if XXX
    public static void AreEqual(object expected, object actual)
    {
      AreEqual(expected, actual, String.Empty);
    }

    public static void AreEqual(object expected, object actual, string message)
    {
      ArrayList testedObjects = new ArrayList();
      DoAreEqual(testedObjects, expected, actual, message);
    }
    private static void DoAreEqual(ArrayList testedObjects, object expected, object actual, string message)
    {
      if (expected == null && actual == null)
        return;
      if (expected == null || actual == null)
        Assert.AreEqual(expected, actual, message);

      if (Object.ReferenceEquals(expected, actual))
        return;

      if (testedObjects.Contains(expected))
        return; // ��� ���������
      testedObjects.Add(expected);

      Type t1 = expected.GetType();
      Type t2 = actual.GetType();
      Assert.AreEqual(t1, t2, message + ". Types are different");

      if (t1.IsPrimitive || expected is Decimal || expected is DateTime || expected is TimeSpan)
      {
        Assert.AreEqual(expected, actual, message);
        return;
      }

      #region ��������

      PropertyInfo[] aProps = t1.GetProperties(BindingFlags.Instance | BindingFlags.Public);
      for (int i = 0; i < aProps.Length; i++)
      {
        object v1, v2;
        Exception ex1, ex2;
        GetProperty(aProps[i], expected, out v1, out ex1);
        GetProperty(aProps[i], actual, out v2, out ex2);

        if (ex1 == null && ex2 == null)
          DoAreEqual(testedObjects, v1, v2, message + "." + aProps[i].Name);
        else if ((ex1 == null) != (ex2 == null))
          Assert.Fail(message + ". Exception for one of compared properties " + aProps[i].Name);
      }

      #endregion

      #region ������������ �������

      IEnumerable en1 = expected as IEnumerable;
      if (en1 != null)
      {
        IEnumerable en2 = (IEnumerable)actual;
        ArrayList lst1 = new ArrayList();
        foreach (object item1 in en1)
          lst1.Add(item1);

        ArrayList lst2 = new ArrayList();
        foreach (object item2 in en2)
          lst2.Add(item2);

        Assert.AreEqual(lst1.Count, lst2.Count, message + ". Enumeration result length");
        for (int i = 0; i < lst1.Count; i++)
          DoAreEqual(testedObjects, lst1[i], lst2[i], message + "[" + i.ToString() + "]"); // ����������� �����
      }

      #endregion
    }
#endif
  }

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
    public void PropertyEqual_ok()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 123;

      Assert.DoesNotThrow(delegate() { PropertyAssert.ProperiesEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void PropertyEqual_diff()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 456;


      Assert.Catch<AssertionException>(delegate() { PropertyAssert.ProperiesEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void PropertyEqual_first_null()
    {

      TestClass1 x1 = null;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 123;

      Assert.Catch<AssertionException>(delegate() { PropertyAssert.ProperiesEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void PropertyEqual_second_null()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = null;

      Assert.Catch<AssertionException>(delegate() { PropertyAssert.ProperiesEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void PropertyEqual_both_nulls()
    {
      TestClass1 x1 = null;
      TestClass1 x2 = null;

      Assert.DoesNotThrow(delegate() { PropertyAssert.ProperiesEqual(x1, x2, String.Empty); });
    }

    [Test]
    public void PropertyEqualExcept_ok()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 456;


      Assert.DoesNotThrow(delegate() { PropertyAssert.ProperiesEqualExcept(x1, x2, new string[]{"Prop2"}, String.Empty); });
    }

    [Test]
    public void PropertyEqualExcept_diff()
    {
      TestClass1 x1 = new TestClass1();
      x1.Prop1 = "AAA";
      x1.Prop2 = 123;

      TestClass1 x2 = new TestClass1();
      x2.Prop1 = "AAA";
      x2.Prop2 = 456;


      Assert.Catch<AssertionException>(delegate() { PropertyAssert.ProperiesEqualExcept(x1, x2, new string[] { "Prop1" }, String.Empty); });
    }
  }
}
