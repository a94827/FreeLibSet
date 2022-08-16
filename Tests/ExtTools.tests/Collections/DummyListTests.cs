using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using System.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class DummyListTests
  {
    [Test]
    public void Constructor()
    {
      // Используем ссылку на интерфейс, т.к. наличие видимых свойств и методов может быть изменено

      IList<int> sut = new DummyList<int>();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Modification_fails()
    {
      IList<int> sut = new DummyList<int>();

      Assert.Catch(delegate() { sut.Add(1); }, "Add()");
      Assert.Catch(delegate() { sut.Insert(0, 1); }, "Insert()");

      // Для этих методов не определено, должны ли они выбрасывать исключение, или ничего не делать.
      // Assert.Catch(delegate() { sut.Clear(); }, "Clear()");
      // Assert.Catch(delegate() { sut.Remove(123); }, "Remove()");
      // Assert.Catch(delegate() { sut.RemoveAt(0); }, "RemoveAt()");
    }

    [Test]
    public void CopyTo()
    {
      IList<int> sut = new DummyList<int>();
      int[] a = new int[0];
      sut.CopyTo(a, 0);
      Assert.Pass();
    }

    [Test]
    public void ICollection_CopyTo()
    {
      IList<int> sut = new DummyList<int>();
      float[] a = new float[0];
      ((ICollection)sut).CopyTo(a, 0);
      Assert.Pass();
    }

    [Test]
    public void Contains()
    {
      IList<int> sut = new DummyList<int>();
      Assert.IsFalse(sut.Contains(0));
    }

    [Test]
    public void IndexOf()
    {
      IList<int> sut = new DummyList<int>();
      Assert.AreEqual(-1, sut.IndexOf(0));
    }

    [Test]
    public void GetEnumerator()
    {
      IList<int> sut = new DummyList<int>();

      List<int> lst = new List<int>();
      foreach (int item in sut)
        lst.Add(item);

      Assert.AreEqual(0, lst.Count);
    }

    [Test]
    public void ICollection_GetEnumerator()
    {
      IList<int> sut = new DummyList<int>();

      List<object> lst = new List<object>();
      foreach (object item in (ICollection)sut)
        lst.Add(item);

      Assert.AreEqual(0, lst.Count);
    }
  }
}
