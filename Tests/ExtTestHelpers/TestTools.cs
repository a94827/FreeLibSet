using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Tests
{
  public static class TestTools
  {
    /// <summary>
    /// Вызов GC.Collect()
    /// </summary>
    public static void GCCollect()
    {
      GC.Collect();

      // В Mono этого недостаточно.
      GC.WaitForPendingFinalizers();
      GC.Collect();
    }
  }
}
