using AgeyevAV;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_Math
  {
    [TestCase(3, 2, Result = 2)]
    [TestCase(-10, 3, Result = -3)]
    [TestCase(11, -3, Result = -4)]
    [TestCase(-2000000001, -1000000000, Result = 2)]
    public int DivideWithRounding_int(int x, int y)
    {
      return DataTools.DivideWithRounding(x, y);
    }

    [Test]
    //[ExpectedException(typeof(DivideByZeroException))]
    public void DivideWithRounding_int_divide_by_zero()
    {
      Assert.Throws(typeof(DivideByZeroException), delegate() 
      { DataTools.DivideWithRounding(1, 0); });
    }

    [TestCase(3000000000000000000L, 2000000000000000000L, Result = 2L)]
    [TestCase(-9000000000000000002L, 3L, Result = -3000000000000000001L)]
    [TestCase(11L, -3L, Result = -4L)]
    [TestCase(-2L, -1L, Result = 2L)]
    public long DivideWithRounding_long(long x, long y)
    {
      return DataTools.DivideWithRounding(x, y);
    }

    [Test]
    //[ExpectedException(typeof(DivideByZeroException))]
    public void DivideWithRounding_long_divide_by_zero()
    {
      Assert.Throws(typeof(DivideByZeroException), delegate()
      { DataTools.DivideWithRounding(1L, 0L); });
    }
  }
}
