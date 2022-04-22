using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class DummyListTests
  {
    [Test]
    public void Constructor()
    {
      // ���������� ������ �� ���������, �.�. ������� ������� ������� � ������� ����� ���� ��������

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

      // ��� ���� ������� �� ����������, ������ �� ��� ����������� ����������, ��� ������ �� ������.
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
  }
}
