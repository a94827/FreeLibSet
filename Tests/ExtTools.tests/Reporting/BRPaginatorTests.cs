using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Reporting;
using NUnit.Framework;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRPaginatorTests
  {
    private class DummyMeasurer : IBRMeasurer
    {
      public int GetWantedHeight(BRSelector sel, int columnWidth)
      {
        throw new NotImplementedException();
      }

      public void MeasureString(BRSelector sel, string s, out int width, out int height)
      {
        throw new NotImplementedException();
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void BRBand_KeepWhole(bool secondKeepWhole)
    {
      BRReport rep = new BRReport();
      BRSection sect = rep.Sections.Add();
      InitTestPageSetup(sect.PageSetup);

      sect.Bands.Add(40, 1); // 20см. высотой
      sect.Bands[0].BottomMargin = 100; // 1см
      sect.Bands[0].KeepWhole = true;
      sect.Bands.Add(40, 2); // 20см. высотой. Столбцы только, чтобы было виднее в отладчике
      sect.Bands[1].KeepWhole = secondKeepWhole;

      SetRowsHeight50(sect);

      BRPaginator sut = new BRPaginator(new DummyMeasurer());
      BRPaginatorPageInfo[] res = sut.CreatePages(rep);
      Assert.AreEqual(2, res.Length, "PageCount");
      if (secondKeepWhole)
      {
        // Каждая таблица на своей странице
        Assert.AreEqual(1, res[0].Blocks.Length, "BlockLen[0]");
        Assert.AreSame(sect.Bands[0], res[0].Blocks[0].Band, "[0][0].Band");
        Assert.AreEqual(0, res[0].Blocks[0].TopOffset, "[0][0].TopOffset");
        CollectionAssert.AreEqual(CreateSeqIndexes(0, 40), res[0].Blocks[0].RowIndexes, "[0][0].RowIndexes");

        Assert.AreEqual(1, res[1].Blocks.Length, "BlockLen[1]");
        Assert.AreSame(sect.Bands[1], res[1].Blocks[0].Band, "[1][0].Band");
        Assert.AreEqual(0, res[1].Blocks[0].TopOffset, "[1][0].TopOffset");
        CollectionAssert.AreEqual(CreateSeqIndexes(0, 40), res[1].Blocks[0].RowIndexes, "[1][0].RowIndexes");
      }
      else
      {
        // Вторая таблица разрывается
        Assert.AreEqual(2, res[0].Blocks.Length, "BlockLen[0]");
        Assert.AreSame(sect.Bands[0], res[0].Blocks[0].Band, "[0][0].Band");
        Assert.AreEqual(0, res[0].Blocks[0].TopOffset, "[0][0].TopOffset");
        CollectionAssert.AreEqual(CreateSeqIndexes(0, 40), res[0].Blocks[0].RowIndexes, "[0][0].RowIndexes");
        Assert.AreSame(sect.Bands[1], res[0].Blocks[1].Band, "[0][1].Band");
        Assert.AreEqual(2100, res[0].Blocks[1].TopOffset, "[0][1].TopOffset");
        CollectionAssert.AreEqual(CreateSeqIndexes(0, 12), res[0].Blocks[1].RowIndexes, "[0][1].RowIndexes");

        Assert.AreEqual(1, res[1].Blocks.Length, "BlockLen[1]");
        Assert.AreSame(sect.Bands[1], res[1].Blocks[0].Band, "[1][0].Band");
        Assert.AreEqual(0, res[1].Blocks[0].TopOffset, "[1][0].TopOffset");
        CollectionAssert.AreEqual(CreateSeqIndexes(12, 28), res[1].Blocks[0].RowIndexes, "[1][0].RowIndexes");
      }
    }

    /// <summary>
    /// Задает размер печатной области 27 x 18см
    /// </summary>
    /// <param name="ps"></param>
    private static void InitTestPageSetup(BRPageSetup ps)
    {
      ps.Landscape = false;
      ps.PaperHeight = 2970;
      ps.PaperWidth = 2100;
      ps.LeftMargin = 200;
      ps.TopMargin = 100;
      ps.RightMargin = 100;
      ps.BottomMargin = 170;
      Assert.AreEqual(2700, ps.PrintAreaHeight, "PrintAreaHeight");
      Assert.AreEqual(1800, ps.PrintAreaWidth, "PrintAreaWidth");
    }

    /// <summary>
    /// Устанавливает высоту всех строк 0,5 см.
    /// </summary>
    /// <param name="sect"></param>
    private static void SetRowsHeight50(BRSection sect)
    {
      foreach (BRBand band in sect.Bands)
      {
        BRSelector sel = band.CreateSelector();
        for (int i = 0; i < band.RowCount; i++)
        {
          sel.RowIndex = i;
          sel.RowInfo.Height = 50;
        }
      }
    }

    /// <summary>
    /// Возвращает массив последовательных индексов
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private static int[] CreateSeqIndexes(int start, int count)
    {
      int[] a = new int[count];
      for (int i = 0; i < count; i++)
        a[i] = start + i;
      return a;
    }
  }
}
