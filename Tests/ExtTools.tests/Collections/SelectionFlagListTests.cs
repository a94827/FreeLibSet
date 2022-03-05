using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  class SelectionFlagListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Count()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      Assert.AreEqual(3, sut.Count);
      for (int i = 0; i < sut.Count; i++)
        Assert.IsFalse(sut[i]);
      Assert.AreEqual(0, sut.SelectedCount);
    }

    [Test]
    public void Constructor_Count_Zero()
    {
      SelectionFlagList sut = new SelectionFlagList(0);
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void Constructor_Collection()
    {
      ICollection<bool> src = new bool[3] { true, false, true };
      SelectionFlagList sut = new SelectionFlagList(src);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsTrue(sut[0], "[0]");
      Assert.IsFalse(sut[1], "[1]");
      Assert.IsTrue(sut[2], "[2]");
    }

    #endregion

    #region Item

    [Test]
    public void Item()
    {
      SelectionFlagList sut = new SelectionFlagList(2);
      sut[0] = true;
      Assert.IsTrue(sut[0], "#1");

      sut[0] = false;
      Assert.IsFalse(sut[0], "#2");
    }

    #endregion

    #region SelectedCount

    [Test]
    public void SelectedCount()
    {
      SelectionFlagList sut = new SelectionFlagList(2);
      Assert.AreEqual(0, sut.SelectedCount, "#1");

      sut[0] = true;
      Assert.AreEqual(1, sut.SelectedCount, "#2");

      sut[1] = true;
      Assert.AreEqual(2, sut.SelectedCount, "#3");

      sut[0] = false;
      Assert.AreEqual(1, sut.SelectedCount, "#4");
    }

    #endregion

    #region Событие Changed

    private class ChangedTester
    {
      /// <summary>
      /// Счетчик вызова события
      /// </summary>
      public int EventCount;

      public void Changed(object sender, EventArgs args)
      {
        EventCount++; // больше нет никакой полезной информации
      }
    }

    [Test]
    public void Changed()
    {
      SelectionFlagList sut = new SelectionFlagList(2);
      ChangedTester tester = new ChangedTester();
      sut.Changed += tester.Changed;

      sut[0] = true;
      Assert.AreEqual(1, tester.EventCount, "#1");

      sut[0] = true;
      Assert.AreEqual(1, tester.EventCount, "#2"); // нет вызова события

      sut[1] = true;
      Assert.AreEqual(2, tester.EventCount, "#3"); // нет вызова события

      sut[0] = false;
      Assert.AreEqual(3, tester.EventCount, "#4");
    }

    #endregion

    #region ToArray/FromArray

    [Test]
    public void ToArray()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      sut[1] = true;

      bool[] a = sut.ToArray();
      Assert.AreEqual(new bool[] { false, true, false }, a, "#1");

      // Полученный массив должен являться копией
      sut[0] = true;
      Assert.AreEqual(new bool[] { false, true, false }, a, "#2");
    }

    [Test]
    public void FromArray()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      sut.FromArray(new bool[] { false, true, false });
      Assert.IsFalse(sut[0], "[0]");
      Assert.IsTrue(sut[1], "[1]");
      Assert.IsFalse(sut[2], "[2]");

      Assert.Catch<ArgumentException>(delegate() { sut.FromArray(new bool[] { false, true }); }, "short");
      Assert.Catch<ArgumentException>(delegate() { sut.FromArray(new bool[] { false, true, false, true }); }, "long");
    }

    #endregion

    #region Select/Unselect/InvertAll()

    [Test]
    public void SelectAll()
    {
      SelectionFlagList sut = new SelectionFlagList(new bool[] { true, false, true });
      sut.SelectAll();
      Assert.AreEqual(new bool[] { true, true, true }, sut.ToArray());
    }

    [Test]
    public void UnselectAll()
    {
      SelectionFlagList sut = new SelectionFlagList(new bool[] { true, false, true });
      sut.UnselectAll();
      Assert.AreEqual(new bool[] { false, false, false }, sut.ToArray());
    }

    [Test]
    public void InvertAll()
    {
      SelectionFlagList sut = new SelectionFlagList(new bool[] { true, false, true });
      sut.InvertAll();
      Assert.AreEqual(new bool[] { false, true, false }, sut.ToArray());
    }

    #endregion

    #region AreAllSelected/Unselected

    [Test]
    public void AreAllSelected()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      Assert.IsFalse(sut.AreAllSelected, "#1");
      sut[0] = true;
      Assert.IsFalse(sut.AreAllSelected, "#2");
      sut[1] = true;
      sut[2] = true;
      Assert.IsTrue(sut.AreAllSelected, "#3");
    }

    [Test]
    public void AreAllUnselected()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      Assert.IsTrue(sut.AreAllUnselected, "#1");
      sut[0] = true;
      Assert.IsFalse(sut.AreAllUnselected, "#2");
      sut[1] = true;
      sut[2] = true;
      Assert.IsFalse(sut.AreAllUnselected, "#3");
    }

    #endregion

    #region SelectedIndices

    [Test]
    public void SelectedIndices_get()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      Assert.AreEqual(new int[0], sut.SelectedIndices, "#1");

      sut[1] = true;
      Assert.AreEqual(new int[] { 1 }, sut.SelectedIndices, "#2");

      sut[0] = true;
      Assert.AreEqual(new int[] { 0, 1 }, sut.SelectedIndices, "#3");
    }

    [Test]
    public void SelectedIndices_set()
    {
      SelectionFlagList sut = new SelectionFlagList(3);
      sut.SelectedIndices = new int[] { 0, 2 };
      Assert.AreEqual(new bool[] { true, false, true }, sut.ToArray(), "#1");

      sut.SelectedIndices = new int[] { 1 };
      Assert.AreEqual(new bool[] { false, true, false }, sut.ToArray(), "#2");

      sut.SelectedIndices = new int[0];
      Assert.AreEqual(new bool[] { false, false, false }, sut.ToArray(), "#3");

      // в текущей реализации - допускается
      //Assert.Catch<ArgumentException>(delegate() { sut.SelectedIndices = new int[3]; }, "out of range");
    }

    #endregion
  }
}
