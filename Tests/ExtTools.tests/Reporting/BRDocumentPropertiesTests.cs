using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;
using FreeLibSet.Tests;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRDocumentPropertiesTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      BRDocumentProperties sut = new BRDocumentProperties();
      ExtStringAssert.IsNullOrEmpty(sut.Title, "Title");
      ExtStringAssert.IsNullOrEmpty(sut.Subject, "Subject");
      ExtStringAssert.IsNullOrEmpty(sut.Author, "Author");
      ExtStringAssert.IsNullOrEmpty(sut.Company, "Company");
    }

    #endregion

    #region Свойства

    [Test]
    public void Properties()
    {
      BRDocumentProperties sut = new BRDocumentProperties();
      sut.Title = "A";
      sut.Subject = "B";
      sut.Author = "C";
      sut.Company = "D";

      Assert.AreEqual("A", sut.Title, "Title");
      Assert.AreEqual("B", sut.Subject, "Subject");
      Assert.AreEqual("C", sut.Author, "Author");
      Assert.AreEqual("D", sut.Company, "Company");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      BRDocumentProperties sut = new BRDocumentProperties();
      sut.Title = "A";
      sut.Subject = "B";
      sut.Author = "C";
      sut.Company = "D";

      BRDocumentProperties res = sut.Clone();
      Assert.AreEqual("A", res.Title, "Title");
      Assert.AreEqual("B", res.Subject, "Subject");
      Assert.AreEqual("C", res.Author, "Author");
      Assert.AreEqual("D", res.Company, "Company");
    }

    #endregion
  }
}
