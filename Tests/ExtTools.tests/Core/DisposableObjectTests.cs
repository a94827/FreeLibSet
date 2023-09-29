using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Tests;
using NUnit.Framework;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DisposableObjectTests
  {
    private class TestObject : DisposableObject
    {
      public int DisposeCountDisposing;
      public int DisposeCountDestructor;
      public event EventHandler DisposeCalled;

      protected override void Dispose(bool disposing)
      {
        if (disposing)
          DisposeCountDisposing++;
        else
          DisposeCountDestructor++;

        if (DisposeCalled != null)
          DisposeCalled(this, EventArgs.Empty);

        base.Dispose(disposing);
      }
    }

    [Test]
    public void Constructor()
    {
      TestObject sut = new TestObject();
      Assert.IsFalse(sut.IsDisposed, "IsDisposed");
      Assert.DoesNotThrow(delegate() { sut.CheckNotDisposed(); }, "CheckNotDisposed()");
      Assert.AreEqual(0, sut.DisposeCountDisposing, "DisposeCountDisposing");
      Assert.AreEqual(0, sut.DisposeCountDestructor, "DisposeCountDestructor");
    }

    [Test]
    public void Dispose_once()
    {
      TestObject sut = new TestObject();
      sut.Dispose();

      Assert.IsTrue(sut.IsDisposed, "IsDisposed");
      Assert.Catch<ObjectDisposedException>(delegate() { sut.CheckNotDisposed(); }, "CheckNotDisposed()");
      Assert.AreEqual(1, sut.DisposeCountDisposing, "DisposeCountDisposing");
      Assert.AreEqual(0, sut.DisposeCountDestructor, "DisposeCountDestructor");
    }

    [Test]
    public void Dispose_twice()
    {
      TestObject sut = new TestObject();
      sut.Dispose();
      sut.Dispose();

      Assert.IsTrue(sut.IsDisposed, "IsDisposed");
      Assert.Catch<ObjectDisposedException>(delegate() { sut.CheckNotDisposed(); }, "CheckNotDisposed()");
      Assert.AreEqual(1, sut.DisposeCountDisposing, "DisposeCountDisposing"); // должен быть только один вызов, а не два
      Assert.AreEqual(0, sut.DisposeCountDestructor, "DisposeCountDestructor");
    }

    // Тестирование деструктора не работает, так как нельзя из деструктора вызывать события, это ненадежно.
  //  private class FlagObj
  //  {
  //    public bool Flag;

  //    internal void DisposeCalled(object sender, EventArgs e)
  //    {
  //      Flag = true;
  //    }
  //  }

  //  [Test]
  //  public void Destructor()
  //  {
  //    FlagObj xFlag = new FlagObj();
  //    Destructor_CreateObj(xFlag);
  //    TestTools.GCCollect();
  //    Assert.IsTrue(xFlag.Flag);
  //  }

  //  private void Destructor_CreateObj(FlagObj xFlag)
  //  {
  //    TestObject sut = new TestObject();
  //    sut.DisposeCalled += xFlag.DisposeCalled;
  //    // А больше TestObject никто не держит
  //  }

  }

  [TestFixture]
  public class SimpleDisposableObjectTests
  {
    private class TestObject : SimpleDisposableObject
    {
      public int DisposeCountDisposing;
      public int DisposeCountDestructor;

      protected override void Dispose(bool disposing)
      {
        if (disposing)
          DisposeCountDisposing++;
        else
          DisposeCountDestructor++;

        base.Dispose(disposing);
      }
    }

    [Test]
    public void Constructor()
    {
      TestObject sut = new TestObject();
      Assert.IsFalse(sut.IsDisposed, "IsDisposed");
      Assert.DoesNotThrow(delegate() { sut.CheckNotDisposed(); }, "CheckNotDisposed()");
      Assert.AreEqual(0, sut.DisposeCountDisposing, "DisposeCountDisposing");
      Assert.AreEqual(0, sut.DisposeCountDestructor, "DisposeCountDestructor");
    }

    [Test]
    public void Dispose_once()
    {
      TestObject sut = new TestObject();
      sut.Dispose();

      Assert.IsTrue(sut.IsDisposed, "IsDisposed");
      Assert.Catch<ObjectDisposedException>(delegate() { sut.CheckNotDisposed(); }, "CheckNotDisposed()");
      Assert.AreEqual(1, sut.DisposeCountDisposing, "DisposeCountDisposing");
      Assert.AreEqual(0, sut.DisposeCountDestructor, "DisposeCountDestructor");
    }

    [Test]
    public void Dispose_twice()
    {
      TestObject sut = new TestObject();
      sut.Dispose();
      sut.Dispose();

      Assert.IsTrue(sut.IsDisposed, "IsDisposed");
      Assert.Catch<ObjectDisposedException>(delegate() { sut.CheckNotDisposed(); }, "CheckNotDisposed()");
      Assert.AreEqual(1, sut.DisposeCountDisposing, "DisposeCountDisposing"); // должен быть только один вызов, а не два
      Assert.AreEqual(0, sut.DisposeCountDestructor, "DisposeCountDestructor");
    }
  }
}
