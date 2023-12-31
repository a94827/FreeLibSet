using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Reporting;
using NUnit.Framework;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRPaginatorTests
  {
    private class DummyMeasurer : IBRMeasurer
    {
      #region IBRMeasurer

      public int GetWantedHeight(BRSelector sel, int columnWidth)
      {
        throw new NotImplementedException();
      }

      public void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    #region Paginate()

    private class PaginatorStat
    {
      public PaginatorStat()
      {
        _SB = new StringBuilder();
      }

      /// <summary>
      /// Содержит информацию о событиях PageHandling.
      /// Для каждого события записывается одна строка описания страницы "P" и переменное количество записей для блоков "B", строк "R" и столбцов "C".
      /// Каждая запись заканчивается символом "|".
      /// "P,ИндексСекции,ЧислоБлоков,IsFirstPage,IsLastPage|"
      /// "B,ИндексПолосы,ЧислоСтрок,ЧислоСтолбцов|"
      /// "R,ИндексыСтрок|"
      /// "C,ИндексыСтолбов|"
      /// Индексы строк и столбцов записываются как диапазоны, например, "0,1-10"
      /// </summary>
      //public StringBuilder SB { get { return _SB; } }
      private readonly StringBuilder _SB;

      public string Result { get { return _SB.ToString(); } }

      internal void HandlePage(object sender, BRPaginatorEventArgs args)
      {
        _SB.Append("P,");
        _SB.Append(StdConvert.ToString(args.Page.Section.SectionIndex));
        _SB.Append(",");
        _SB.Append(StdConvert.ToString(args.Page.Blocks.Length));
        _SB.Append(",");
        _SB.Append(args.IsFirstPage ? "1" : "0");
        _SB.Append(",");
        _SB.Append(args.IsLastPage ? "1" : "0");
        _SB.Append("|");

        foreach (BRPaginatiorBlockInfo blk in args.Page.Blocks)
        {
          _SB.Append("B,");
          _SB.Append(StdConvert.ToString(blk.Band.BandIndex));
          _SB.Append(",");
          _SB.Append(StdConvert.ToString(blk.RowIndexes.Length));
          _SB.Append(",");
          _SB.Append(StdConvert.ToString(blk.ColumnIndexes.Length));
          _SB.Append("|");

          _SB.Append("R");
          AddIntRanges(blk.RowIndexes);
          _SB.Append("|");

          _SB.Append("C");
          AddIntRanges(blk.ColumnIndexes);
          _SB.Append("|");
        }
      }

      #region Вспомогательные методы

      private void AddIntRanges(int[] a)
      {
        if (a.Length == 0)
          return;

        int v1 = -1;
        int v2 = -1;
        for (int i = 0; i < a.Length; i++)
        {
          if (v1 >= 0 && a[i] != v2 + 1)
            FlushRange(ref v1, ref v2);

          if (v1 < 0)
            v1 = v2 = a[i];
          else
            v2 = a[i];
        }
        if (v1 >= 0)
          FlushRange(ref v1, ref v2);
      }

      private void FlushRange(ref int v1, ref int v2)
      {
        if (v1 < 0 || v2 < v1)
          throw new BugException("v1=" + v1.ToString() + ", v2=" + v2.ToString());

        _SB.Append(",");
        _SB.Append(StdConvert.ToString(v1));
        if (v2 > v1)
        {
          _SB.Append("-");
          _SB.Append(StdConvert.ToString(v2));
        }
        v1 = -1;
        v2 = -1;
      }

      #endregion
    }

    [Test]
    public void Paginate_BRSection()
    {
      BRReport rep = CreateTestReport1();
      BRPaginator sut = new BRPaginator(new DummyMeasurer());
      PaginatorStat stat = new PaginatorStat();
      sut.PageHandling += stat.HandlePage;

      sut.Paginate(rep.Sections[1]);

      string wanted =
        "P,1,1,1,0|" +
        "B,0,54,9|" +
        "R,0-53|" +
        "C,0-8|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0-53|" +
        "C,0,9-16|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0-53|" +
        "C,0,17-24|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0,54-106|" +
        "C,0-8|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0,54-106|" +
        "C,0,9-16|" +

        "P,1,1,0,1|" +
        "B,0,54,9|" +
        "R,0,54-106|" +
        "C,0,17-24|";

      Assert.AreEqual(wanted, stat.Result);
    }


    [Test]
    public void Paginate_BRReport()
    {
      BRReport rep = CreateTestReport1();
      BRPaginator sut = new BRPaginator(new DummyMeasurer());
      PaginatorStat stat = new PaginatorStat();
      sut.PageHandling += stat.HandlePage;

      sut.Paginate(rep);

      string wanted =
        "P,0,2,1,0|" +
        "B,0,1,1|" +
        "R,0|" +
        "C,0|" +
        "B,1,20,4|" +
        "R,0-19|" +
        "C,0-3|" +


        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0-53|" +
        "C,0-8|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0-53|" +
        "C,0,9-16|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0-53|" +
        "C,0,17-24|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0,54-106|" +
        "C,0-8|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0,54-106|" +
        "C,0,9-16|" +

        "P,1,1,0,0|" +
        "B,0,54,9|" +
        "R,0,54-106|" +
        "C,0,17-24|" +


        "P,2,1,0,1|" +
        "B,0,36,13|" +
        "R,0-35|" +
        "C,0-12|";

      Assert.AreEqual(wanted, stat.Result);
    }

    /// <summary>
    /// Создаем тестовый отчет из 3 секций.
    /// Первая секция содержит заголовок и таблицу, которая помещается на 1 страницу.
    /// Вторая секция содержит большую таблицу на 3 полосы по ширине и 2 страницы в высоту, с повторяющимися строками и столбцами.
    /// Третья секция содержит 1 таблицу в альбомной ориентации.
    /// Все строки имеют фиксированную высоту.
    /// </summary>
    /// <returns>Тестовый отчет</returns>
    private static BRReport CreateTestReport1()
    {
      BRReport rep = new BRReport();
      BRSection sect1 = rep.Sections.Add();
      InitTestPageSetup(sect1.PageSetup);
      BRTable tbl11 = sect1.Bands.Add(1, 1);
      tbl11.SetValue(0, 0, "Head 1");

      BRTable tbl12 = sect1.Bands.Add(20, 4);
      FillTestValues(tbl12);

      SetRowsHeight50(sect1);

      BRSection sect2 = rep.Sections.Add();
      // На лист помещается 27/0.5 = 54 строки по высоте и 18/2 = 9 столбов по ширине
      //                     Столбцы 0-8     Столбцы 0,9-16  Столбцы 0,17-24   
      // строки 0-53             1                 2                3               
      // строки 0,54-106         4                 5                6
      BRTable tbl21 = sect2.Bands.Add(107, 25);
      FillTestValues(tbl21);
      tbl21.Cells.RowIndex = 0;
      tbl21.Cells.RowInfo.Repeatable = true;
      tbl21.Cells.ColumnIndex = 0;
      tbl21.Cells.ColumnInfo.Repeatable = true;
      SetRowsHeight50(sect2);
      SetColumnsWidth200(tbl21);

      BRSection sect3 = rep.Sections.Add();
      sect3.PageSetup.InvertOrientation();

      BRTable tbl31 = sect3.Bands.Add(36, 13);
      FillTestValues(tbl31);
      SetRowsHeight50(sect3);
      SetColumnsWidth200(tbl31);
      tbl31.Cells.ColumnIndex = 0;
      tbl31.Cells.ColumnInfo.SetWidth(300, false);

      return rep;
    }

    #endregion

    #region CreatePages()

    [Test]
    public void CreatePages()
    {
      BRReport rep = CreateTestReport1();
      BRPaginator sut = new BRPaginator(new DummyMeasurer());
      BRPaginatorPageInfo[] pages = sut.CreatePages(rep);
      Assert.AreEqual(1 + 6 + 1, pages.Length);
    }

    #endregion

    #region Особенности отчета

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

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Задает размер печатной области 27 x 18см
    /// </summary>
    /// <param name="ps"></param>
    private static void InitTestPageSetup(BRPageSetup ps)
    {
      ps.Orientation = BROrientation.Portrait;
      ps.PaperHeight = 2970;
      ps.PaperWidth = 2100;
      ps.LeftMargin = 200;
      ps.TopMargin = 100;
      ps.RightMargin = 100;
      ps.BottomMargin = 170;
      Assert.AreEqual(2700, ps.PrintAreaHeight, "PrintAreaHeight");
      Assert.AreEqual(1800, ps.PrintAreaWidth, "PrintAreaWidth");
    }

    private static void FillTestValues(BRBand band)
    {
      BRSelector sel = band.CreateSelector();
      for (int i = 0; i < band.RowCount; i++)
      {
        sel.RowIndex = i;
        for (int j = 0; j < band.ColumnCount; j++)
        {
          sel.ColumnIndex = j;
          sel.Value = i * band.ColumnCount + j + 1;
        }
      }
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


    private static void SetColumnsWidth200(BRBand band)
    {
      BRSelector sel = band.CreateSelector();
      for (int j = 0; j < band.ColumnCount; j++)
      {
        sel.ColumnIndex = j;
        sel.ColumnInfo.SetWidth(200, false);
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

    #endregion
  }
}
