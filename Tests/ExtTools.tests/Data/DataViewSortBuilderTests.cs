using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using NUnit.Framework;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DataViewSortBuilderTests
  {
    [Test]
    public void Constructor()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      Assert.AreEqual("", sut.ToString(), "ToString()");
    }

    [Test]
    public void Add_ok()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      sut.Add("F1");
      sut.Add("F2");
      Assert.AreEqual("F1,F2", sut.ToString());
    }

    [Test]
    public void AddSubName()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      sut.Add("F1");
      sut.AddSubName("F2");
      sut.AddSubName("F3");
      sut.Add("F4");
      Assert.AreEqual("F1.F2.F3,F4", sut.ToString());
    }

    [Test]
    public void AddSubName_exception()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      Assert.Catch(delegate(){ sut.AddSubName("F1");});
    }

    [Test]
    public void SetSort_ok()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      sut.Add("F1");
      sut.SetSort(System.ComponentModel.ListSortDirection.Ascending);
      sut.Add("F2");
      sut.SetSort(System.ComponentModel.ListSortDirection.Descending);

      Assert.AreEqual("F1,F2 DESC", sut.ToString());
    }

    [Test]
    public void SetSort_exception()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      Assert.Catch(delegate() { sut.SetSort(System.ComponentModel.ListSortDirection.Descending); });
    }

    [Test]
    public void Clear()
    {
      DataViewSortBuilder sut = new DataViewSortBuilder();
      sut.Add("F1");
      sut.Clear();
      Assert.AreEqual("", sut.ToString(), "#1");
      sut.Add("F2");
      Assert.AreEqual("F2", sut.ToString(), "#2");
    }
  }
}
