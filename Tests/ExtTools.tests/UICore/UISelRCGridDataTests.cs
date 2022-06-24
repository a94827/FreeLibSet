using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FreeLibSet.UICore;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using System.Globalization;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UISelRCGridDataTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      string[,] sourceData = new string[,] { { "AAA", "1" }, { "BBB", "2" }, { "CCC", "3" } };
      UISelRCColumn[] availableColumns = new UISelRCColumn[1] { new UISelRCColumn("C1") };

      UISelRCGridData sut = new UISelRCGridData(sourceData, availableColumns);

      Assert.AreEqual(3, sut.RowCount, "RowCount");
      Assert.AreEqual(2, sut.ColumnCount, "ColumnCount");

      Assert.AreEqual(sourceData, sut.SourceData, "SourceData");

      Assert.AreEqual(1, sut.AvailableColumns.Length, "AvailableColumns.Length");
      Assert.AreEqual("C1", sut.AvailableColumns[0].Code);

      Assert.IsTrue(sut.SelRows.IsEmpty, "SelRows.IsEmpty");
      Assert.AreEqual(new bool[3] { false, false, false }, sut.SelRows.AsArray, "SelRows.AsArray");

      Assert.IsTrue(sut.SelColumns.IsEmpty, "SelRows.IsEmpty");
      Assert.AreEqual(new string[2] { String.Empty, String.Empty }, sut.SelColumns.Codes, "SelColumns.Codes");
      Assert.AreEqual(",", sut.SelColumns.AsString, "SelColumns.AsString");
    }

    [Test]
    public void Constructor_exceptions()
    {
      string[,] sourceData = new string[,] { { "AAA", "1" }, { "BBB", "2" }, { "CCC", "3" } };
      UISelRCColumn[] availableColumns = new UISelRCColumn[1] { new UISelRCColumn("C1") };

      Assert.Catch<ArgumentNullException>(delegate { new UISelRCGridData(null, availableColumns); }, "sourceData=null");

      Assert.Catch<ArgumentNullException>(delegate { new UISelRCGridData(sourceData, null); }, "availableColumns=null");

      UISelRCColumn[] badAvailableColumns1 = new UISelRCColumn[2] { new UISelRCColumn("C1"), new UISelRCColumn("C1") };
      Assert.Catch<Exception>(delegate { new UISelRCGridData(sourceData, badAvailableColumns1); }, "availableColumns contains same code");

      UISelRCColumn[] badAvailableColumns2 = new UISelRCColumn[2] { new UISelRCColumn("C1"), null };
      Assert.Catch<Exception>(delegate { new UISelRCGridData(sourceData, badAvailableColumns2); }, "availableColumns contains null");
    }

    #endregion

    #region Тестовые данные

    private static UISelRCGridData CreateTestData()
    {
      string[,] sourceData = new string[5, 3] {
        { "AAA", "", "1" },    // [0] 
        { "BBB", "", "2" },    // [1] 
        { "CCC", "", "XXX" },  // [2] 
        { ""   , "", ""  },    // [3] 
        { "EEE", "", "55" } }; // [4]


      UISelRCColumn col1 = new UISelRCColumn("Col1");
      col1.Validating += TestColumn1_Validating;

      UISelRCColumn col2 = new UISelRCColumn("Col2");

      UISelRCColumn[] availableColumns = new UISelRCColumn[2] { col1, col2 };

      UISelRCGridData sut = new UISelRCGridData(sourceData, availableColumns);
      return sut;
    }

    static void TestColumn1_Validating(object sender, UISelRCValidatingEventArgs args)
    {
      int x;
      if (int.TryParse(args.SourceData, out x))
      {
        args.ResultValue = x;
        if (args.SourceData.Length != 1)
          args.SetWarning("Длина строки должна быть 1 символ");
      }
      else
      {
        args.SetError("Нельзя преобразовать в целое число");
        args.ResultValue = -1; // ошибочное значение все равно можно получить
      }
    }

    #endregion

    #region SelRows

    [Test]
    public void SelRows_Item()
    {
      UISelRCGridData sut = CreateTestData();
      for (int i = 0; i < sut.RowCount; i++)
        Assert.IsFalse(sut.SelRows[i], "#1 [" + i.ToString() + "]");

      sut.SelRows[1] = true;

      for (int i = 0; i < sut.RowCount; i++)
        Assert.AreEqual(i == 1, sut.SelRows[i], "#2 [" + i.ToString() + "]");
    }


    [Test]
    public void SelRows_IsEmpty()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.IsTrue(sut.SelRows.IsEmpty, "#1");

      sut.SelRows[1] = true;
      Assert.IsFalse(sut.SelRows.IsEmpty, "#2");

      sut.SelRows[1] = false;
      Assert.IsTrue(sut.SelRows.IsEmpty, "#3");
    }


    [Test]
    public void SelRows_AsArray_get()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.AreEqual(new bool[] { false, false, false, false, false }, sut.SelRows.AsArray, "#1");

      sut.SelRows[1] = true;
      Assert.AreEqual(new bool[] { false, true, false, false, false }, sut.SelRows.AsArray, "#2");
    }

    [Test]
    public void SelRows_AsArray_set()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.AsArray = new bool[] { true, false, true, false, true };
      Assert.AreEqual(new bool[] { true, false, true, false, true }, sut.SelRows.AsArray, "#1");

      bool[] a2 = new bool[10]; // длинее, чем SourceData
      a2[3] = true;
      sut.SelRows.AsArray = a2;
      Assert.AreEqual(new bool[] { false, false, false, true, false }, sut.SelRows.AsArray, "#2");

      bool[] a3 = new bool[2]; // короче, чем SourceData
      a3[1] = true;
      sut.SelRows.AsArray = a3;
      Assert.AreEqual(new bool[] { false, true, false, false, false }, sut.SelRows.AsArray, "#3");
    }

    [Test]
    public void SelRows_AsArray_set_null()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.AsArray = new bool[] { true, false, true, false, true };
      sut.SelRows.AsArray = null;
      Assert.AreEqual(new bool[] { false, false, false, false, false }, sut.SelRows.AsArray);
    }

    [Test]
    public void SelRows_Clear()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows[1] = true;
      Assert.IsFalse(sut.SelRows.IsEmpty, "#1");

      sut.SelRows.Clear();
      Assert.IsTrue(sut.SelRows.IsEmpty, "#2");
    }

    [Test]
    public void SelRows_Init()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.Init();
      Assert.AreEqual(new bool[] { true, true, true, false, true }, sut.SelRows.AsArray, "#1");

      sut.SelRows.Clear();
      sut.SelRows[1] = true;
      sut.SelRows[3] = true;
      sut.SelRows.Init();
      Assert.AreEqual(new bool[] { true, true, true, true, true }, sut.SelRows.AsArray, "#2");
    }

    #endregion

    #region SelColumns

    [Test]
    public void SelColumns_Item()
    {
      UISelRCGridData sut = CreateTestData();
      for (int i = 0; i < sut.ColumnCount; i++)
        Assert.IsNull(sut.SelColumns[i], "#1[" + i.ToString() + "]");

      sut.SelColumns[1] = sut.AvailableColumns[0];
      Assert.IsNull(sut.SelColumns[0], "#2[0]");
      Assert.AreSame(sut.AvailableColumns[0], sut.SelColumns[1], "#2[1]");
      Assert.IsNull(sut.SelColumns[2], "#2[2]");
    }

    [Test]
    public void SelColumns_IsEmpty()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.IsTrue(sut.SelColumns.IsEmpty, "#1");

      sut.SelColumns[2] = sut.AvailableColumns[0];
      Assert.IsFalse(sut.SelColumns.IsEmpty, "#2");

      sut.SelColumns[2] = null;
      Assert.IsTrue(sut.SelColumns.IsEmpty, "#3");
    }

    [Test]
    public void SelColumns_Codes_get()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.AreEqual(new string[] { "", "", "" }, sut.SelColumns.Codes, "#1");

      sut.SelColumns[1] = sut.AvailableColumns[0];
      Assert.AreEqual(new string[] { "", "Col1", "" }, sut.SelColumns.Codes, "#2");
    }

    [Test]
    public void SelColumns_Codes_set()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.Codes = new string[] { "", "Col1", "" };
      Assert.AreEqual(new string[] { "", "Col1", "" }, sut.SelColumns.Codes, "#1");

      string[] a2 = DataTools.CreateArray<string>(4, ""); // длинее, чем SourceData
      a2[2] = "Col1";
      sut.SelColumns.Codes = a2;
      Assert.AreEqual(new string[] { "", "", "Col1" }, sut.SelColumns.Codes, "#2");

      string[] a3 = DataTools.CreateArray<string>(2, ""); // короче, чем SourceData
      a3[0] = "Col1";
      sut.SelColumns.Codes = a3;
      Assert.AreEqual(new string[] { "Col1", "", "" }, sut.SelColumns.Codes, "#3");
    }

    [Test]
    public void SelColumns_Codes_same()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.Codes = new string[] { "Col1", "Col1" };
      Assert.AreEqual(new string[] { "Col1", "Col1", "" }, sut.SelColumns.Codes);
    }

    [Test]
    public void SelColumns_Codes_set_null()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.Codes = new string[] { "Col1", "", "" };
      sut.SelColumns.Codes = null;
      Assert.AreEqual(new string[] { "", "", "" }, sut.SelColumns.Codes);
    }

    [Test]
    public void SelColumns_AsString_get()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.AreEqual(",,", sut.SelColumns.AsString, "#1");

      sut.SelColumns[1] = sut.AvailableColumns[0];
      Assert.AreEqual(",Col1,", sut.SelColumns.AsString, "#2");
    }

    [Test]
    public void SelColumns_AsString_set()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.AsString = ",,Col1";
      Assert.AreEqual(new string[] { "", "", "Col1" }, sut.SelColumns.Codes);
    }


    [TestCase("Col1", 1)]
    [TestCase("XXX", -1)]
    [TestCase("", 0)]
    public void SelColumns_IndexOf_Contains(string code, int wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns[1] = sut.AvailableColumns[0];

      int res1 = sut.SelColumns.IndexOf(code);
      bool res2 = sut.SelColumns.Contains(code);

      Assert.AreEqual(wantedRes, res1, "IndexOf()");
      Assert.AreEqual(wantedRes >= 0, res2, "Contains()");
    }

    [Test]
    public void SelColumns_IndexOf_repeated()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns[2] = sut.AvailableColumns[0];
      sut.SelColumns[0] = sut.AvailableColumns[0];
      Assert.AreEqual(0, sut.SelColumns.IndexOf("Col1")); // а не 2
    }

    [Test]
    public void SelColumns_IndexOf_dynamic() // нужно тестировать внутреннюю буферизацию позиций
    {
      UISelRCGridData sut = CreateTestData();
      Assert.AreEqual(-1, sut.SelColumns.IndexOf("Col1"), "#1 (null, null, null)");

      sut.SelColumns[2] = sut.AvailableColumns[0];
      Assert.AreEqual(2, sut.SelColumns.IndexOf("Col1"), "#2 (null, null, Col1)");

      sut.SelColumns[0] = sut.AvailableColumns[0];
      Assert.AreEqual(0, sut.SelColumns.IndexOf("Col1"), "#3 (Col1, null, Col1)");

      sut.SelColumns[0] = sut.AvailableColumns[1];
      Assert.AreEqual(2, sut.SelColumns.IndexOf("Col1"), "#4 (Col2 null, Col1)");

      sut.SelColumns.AsString = "Col2";
      Assert.AreEqual(-1, sut.SelColumns.IndexOf("C1"), "#5 (Col2, null, null)");

      sut.SelColumns.Codes = new string[] { "Col1", "Col2" };
      Assert.AreEqual(0, sut.SelColumns.IndexOf("Col1"), "#6 (Col1, Col2, null)");

      sut.SelColumns.Clear();
      Assert.AreEqual(-1, sut.SelColumns.IndexOf("Col1"), "#7 (null, null, null)");

      sut.SelColumns.Init();
      Assert.AreEqual(-1, sut.SelColumns.IndexOf("Col1"), "#8 (null, null, null, no SelRows)");

      sut.SelRows[0] = true;
      sut.SelColumns.Init();
      Assert.AreEqual(2, sut.SelColumns.IndexOf("Col1"), "#9 (Col2, null, Col1)");
    }

    [TestCase("Col1", true)]
    [TestCase("Col2", false)]
    [TestCase("Col1,Col2", true)]
    public void SelColumns_ContainsAny(string codes, bool wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns[1] = sut.AvailableColumns[0];

      bool res = sut.SelColumns.ContainsAny(codes);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("Col1", true)]
    [TestCase("Col2", false)]
    [TestCase("Col1,Col2", false)]
    public void SelColumns_ContainsAll(string codes, bool wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns[1] = sut.AvailableColumns[0];

      bool res = sut.SelColumns.ContainsAll(codes);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void SelColumns_Clear()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns[1] = sut.AvailableColumns[0];
      Assert.IsFalse(sut.SelColumns.IsEmpty, "#1");

      sut.SelColumns.Clear();
      Assert.IsTrue(sut.SelColumns.IsEmpty, "#2");
    }

    [TestCase(",,", "Col2,,Col1")] // автоматическое назначение всех столбцоа
    [TestCase(",,Col2", ",,Col2")] // описатель "Col1" не может быть назначен, т.к. он не подойдет для столбца [0]
    [TestCase(",,CCol", "Col2,,Col1")] // описатель "Col2" может быть назначен
    [TestCase("Col1,,", "Col1,,Col2")] // назначили неподходящий столбец, но описатель "Col2" может быть назначен
    public void SelColumns_Init(string startedCodes, string wantedResCodes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows[0] = true; // иначе не будет. Но не инициализируем все строки, чтобы не было предупреждений для столбца "C1".
      sut.SelColumns.AsString = startedCodes;

      sut.SelColumns.Init();
      Assert.AreEqual(wantedResCodes, sut.SelColumns.AsString);
    }

    [Test]
    public void SelColumns_Init_no_rows()
    {
      UISelRCGridData sut = CreateTestData();
      //sut.SelRows.Clear();

      sut.SelColumns.Init();
      Assert.AreEqual(",,", sut.SelColumns.AsString);
    }

    [TestCase(",,", "", "0,1,2")]
    [TestCase(",Col1,", "", "0,2")]
    [TestCase(",,", "Col1", "")]
    [TestCase(",Col1,", "Col1", "1")]
    [TestCase("Col1,Col2,Col1", "Col1", "0,2")]
    [TestCase("Col1,Col2,Col1", "Col2", "1")] // надо обязательно и этот случай проверить, когда есть повтор в другом коде
    public void SelColumns_GetIndexes(string selColumnCodes, string code, string sWantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.AsString = selColumnCodes;

      int[] res = sut.SelColumns.GetIndexes(code);
      int[] wantedRes = StdConvert.ToInt32Array(sWantedRes);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(",,", false)]
    [TestCase(",Col1,", false)]
    [TestCase("Col1,Col2,", false)]
    [TestCase("Col1,Col2,Col1", true)]
    [TestCase("Col1,Col1,Col1", true)]
    public void SelColumns_HasRepeats(string selColumnCodes, bool wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.AsString = selColumnCodes;

      Assert.AreEqual(wantedRes, sut.SelColumns.HasRepeats);
    }

    [Test]
    public void SelColumns_HasRepeats_Dynamic()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.IsFalse(sut.SelColumns.HasRepeats, "#1 (null,null,null)");

      sut.SelColumns.AsString = "Col1,Col1,Col1";
      Assert.IsTrue(sut.SelColumns.HasRepeats, "#2 (Col1,Col1,Col1)");

      sut.SelColumns[2] = sut.AvailableColumns[1]; // Col2
      Assert.IsTrue(sut.SelColumns.HasRepeats, "#3 (Col1,Col1,Col2)");

      sut.SelColumns[0] = null;
      Assert.IsFalse(sut.SelColumns.HasRepeats, "#4 (null,Col1,Col2)");

      sut.SelColumns[1] = sut.AvailableColumns[1];
      Assert.IsTrue(sut.SelColumns.HasRepeats, "#5 (null,Col2,Col2)");

      sut.SelColumns.Clear();
      Assert.IsFalse(sut.SelColumns.HasRepeats, "#6 (null,null,null)");
    }

    [TestCase(",,", 0, false)]
    [TestCase(",Col1,", 0, false)]
    [TestCase(",Col1,", 1, false)]
    [TestCase(",Col1,Col2", 1, false)]
    [TestCase("Col1,Col2,Col1", 0, true)]
    [TestCase("Col1,Col2,Col1", 1, false)]
    [TestCase("Col1,Col2,Col1", 2, true)]
    [TestCase("Col1,,Col1", 0, true)]
    [TestCase("Col1,,Col1", 1, false)]
    public void SelColumns_IsRepeated(string selColumnCodes, int colIndex, bool wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.AsString = selColumnCodes;

      Assert.AreEqual(wantedRes, sut.SelColumns.IsRepeated(colIndex));
    }

    [Test]
    public void SelColumns_IsRepeated_dynamic() // надо тестировать внутреннюю буферизацию в SelColumns
    {
      UISelRCGridData sut = CreateTestData();
      Assert.IsFalse(sut.SelColumns.IsRepeated(0), "#1");

      sut.SelColumns[0] = sut.AvailableColumns[0];
      Assert.IsFalse(sut.SelColumns.IsRepeated(0), "#2");

      sut.SelColumns[2] = sut.AvailableColumns[0];
      Assert.IsTrue(sut.SelColumns.IsRepeated(0), "#3");

      sut.SelColumns[2] = sut.AvailableColumns[1];
      Assert.IsFalse(sut.SelColumns.IsRepeated(0), "#4");
    }


    [TestCase(",,", "")]
    [TestCase(",Col1,", "")]
    [TestCase("Col1,Col2,", "")]
    [TestCase("Col1,Col2,Col1", "Col1")]
    [TestCase("Col1,Col1,Col1", "Col1")]
    public void SelColumns_RepeatedCodes(string selColumnCodes, string sWantedRes)
    {                        
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.AsString = selColumnCodes;

      Assert.AreEqual(sWantedRes, String.Join(",", sut.SelColumns.RepeatedCodes));
    }

    [Test]
    public void SelColumns_RepeatedCodes_dynamic() // надо тестировать внутреннюю буферизацию в SelColumns
    {
      // Основной тестовый набор CreateTestData() содержит только 3 столбца, поэтому нельзя протестировать больше одного повторяющегося кода
      string[,] sourceData = new string[5, 10];
      UISelRCDateColumn[] availableColumns = new UISelRCDateColumn[3];
      availableColumns[0] = new UISelRCDateColumn("Col1");
      availableColumns[1] = new UISelRCDateColumn("Col2");
      availableColumns[2] = new UISelRCDateColumn("Col3");
      UISelRCGridData sut = new UISelRCGridData(sourceData, availableColumns);
      Assert.AreEqual("", String.Join(",", sut.SelColumns.RepeatedCodes), "#1 ()");

      sut.SelColumns.AsString = "Col2,Col3,Col1";
      Assert.AreEqual("", String.Join(",", sut.SelColumns.RepeatedCodes), "#2 (Col2,Col3,Col1)");

      sut.SelColumns[3] = availableColumns[2];
      Assert.AreEqual("Col3", String.Join(",", sut.SelColumns.RepeatedCodes), "#3 (Col2,Col3,Col1,Col3)");

      sut.SelColumns[4] = availableColumns[1];
      Assert.AreEqual("Col2,Col3", String.Join(",", sut.SelColumns.RepeatedCodes), "#4 (Col2,Col3,Col1,Col3,Col2)");

      sut.SelColumns[5] = availableColumns[0];
      Assert.AreEqual("Col2,Col3,Col1", String.Join(",", sut.SelColumns.RepeatedCodes), "#5 (Col2,Col3,Col1,Col3,Col2,Col1)");

      sut.SelColumns.Clear();
      Assert.AreEqual("", String.Join(",", sut.SelColumns.RepeatedCodes), "#6 ()");

      sut.SelColumns.AsString = "Col2,Col1,Col3,Col3,Col2,Col1,Col2";
      Assert.AreEqual("Col2,Col1,Col3", String.Join(",", sut.SelColumns.RepeatedCodes), "#7 (Col2,Col1,Col3,Col3,Col2,Col1,Col2)");

      sut.SelColumns[0] = null;
      Assert.AreEqual("Col1,Col3,Col2", String.Join(",", sut.SelColumns.RepeatedCodes), "#8 (null,Col1,Col3,Col3,Col2,Col1,Col2)");

      sut.SelColumns[1] = null;
      Assert.AreEqual("Col3,Col2", String.Join(",", sut.SelColumns.RepeatedCodes), "#9 (null,null,Col3,Col3,Col2,Col1,Col2)");
    }

    [TestCase(",,", -1)]
    [TestCase(",Col1,", -1)]
    [TestCase("Col1,Col2,", -1)]
    [TestCase("Col1,Col2,Col1", 0)]
    [TestCase("Col1,Col1,Col1", 0)]
    [TestCase("Col2,Col1,Col1", 1)]
    public void SelColumns_FirstRepeatedColumnIndex(string selColumnCodes, int wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns.AsString = selColumnCodes;

      Assert.AreEqual(wantedRes, sut.SelColumns.FirstRepeatedColumnIndex);
    }

    public void SelColumns_FirstRepeatedColumnIndex_dynamic()
    {
      UISelRCGridData sut = CreateTestData();
      Assert.AreEqual(-1, sut.SelColumns.FirstRepeatedColumnIndex, "#1");

      sut.SelColumns.AsString = ",Col1,Col1";
      Assert.AreEqual(1, sut.SelColumns.FirstRepeatedColumnIndex, "#2");

      sut.SelColumns[0] = sut.AvailableColumns[0];
      Assert.AreEqual(0, sut.SelColumns.FirstRepeatedColumnIndex, "#3");
    }

    #endregion

    #region Событие Changed

    private class ChangeCounter
    {
      public int Count;

      public void Changed(object sender, EventArgs args)
      {
        Count++;
      }
    }

    #region SelRows

    [Test]
    public void Changed_SelRows_Item_set()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelRows[1] = true;
      Assert.AreEqual(1, counter.Count, "#1");

      sut.SelRows[1] = true;
      Assert.AreEqual(1, counter.Count, "#2");

      sut.SelRows[1] = false;
      Assert.AreEqual(2, counter.Count, "#3");
    }

    [Test]
    public void Changed_SelRows_Clear()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows[1] = true;
      sut.SelRows[3] = true;
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelRows.Clear();
      Assert.AreEqual(1, counter.Count);
    }

    [Test]
    public void Changed_SelRows_AsArray_set()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelRows.AsArray = new bool[] { true, false, true, false, true };
      Assert.AreEqual(1, counter.Count); // а не 3
    }

    [Test]
    public void Changed_SelRows_Init()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelRows.Init();
      Assert.AreEqual(1, counter.Count); // а не 4
    }

    #endregion

    #region SelColumns

    [Test]
    public void Changed_SelColumns_Item_set()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelColumns[1] = sut.AvailableColumns[0];
      Assert.AreEqual(1, counter.Count, "#1");

      sut.SelColumns[1] = sut.AvailableColumns[0];
      Assert.AreEqual(1, counter.Count, "#2");

      sut.SelColumns[1] = sut.AvailableColumns[1];
      Assert.AreEqual(2, counter.Count, "#3");
    }

    [Test]
    public void Changed_Columns_Clear()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelColumns[1] = sut.AvailableColumns[0];
      sut.SelColumns[2] = sut.AvailableColumns[1];
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelColumns.Clear();
      Assert.AreEqual(1, counter.Count); // а не 2 
    }

    [Test]
    public void Changed_SelColumns_AsString_set()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelColumns.AsString = "Col1,Col2,";
      Assert.AreEqual(1, counter.Count); // а не 2
    }

    [Test]
    public void Changed_SelColumns_Init()
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.Init(); // иначе не будет инициализации столбцов
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;

      sut.SelColumns.Init();
      Assert.AreEqual(1, counter.Count); // а не 4
    }

    #endregion

    #endregion

    #region Begin/EndUpdate

    [Test]
    public void BeginEndUpdate()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;
      sut.BeginUpdate();
      Assert.AreEqual(0, counter.Count, "#1");

      sut.SelRows[0] = true;
      sut.SelRows[2] = true;
      sut.SelRows[4] = true;
      Assert.AreEqual(0, counter.Count, "#2");

      sut.EndUpdate();
      Assert.AreEqual(1, counter.Count, "#3");
    }

    [Test]
    public void BeginEndUpdate_nested()
    {
      UISelRCGridData sut = CreateTestData();
      ChangeCounter counter = new ChangeCounter();
      sut.Changed += counter.Changed;
      sut.BeginUpdate();
      sut.BeginUpdate();
      Assert.AreEqual(0, counter.Count, "#1");

      sut.SelRows[0] = true;
      sut.SelRows[2] = true;
      Assert.AreEqual(0, counter.Count, "#2");

      sut.EndUpdate();
      Assert.AreEqual(0, counter.Count, "#3");

      sut.EndUpdate();
      Assert.AreEqual(1, counter.Count, "#4");
    }

    #endregion

    #region Validate()

    [TestCase(0, 0, UIValidateState.Ok)]
    [TestCase(0, 1, UIValidateState.Ok)]
    [TestCase(0, 2, UIValidateState.Ok)]
    [TestCase(3, 0, UIValidateState.Ok)]
    [TestCase(2, 2, UIValidateState.Error)]
    [TestCase(4, 2, UIValidateState.Warning)]
    public void Validate(int rowIndex, int colIndex, UIValidateState wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.Init(); // включая строку [4] с предупреждением
      sut.SelColumns.AsString = "Col2,,Col1";

      string errorText;
      UIValidateState res = sut.Validate(rowIndex, colIndex, out errorText);
      Assert.AreEqual(wantedRes, res, "Result");

      Assert.AreEqual(String.IsNullOrEmpty(errorText), res == UIValidateState.Ok, "ErrorText");
    }

    #endregion

    #region Result values

    [TestCase(0, 0, "AAA")]
    [TestCase(3, 0, null)] // пропущенная строка
    [TestCase(0, 1, null)] // Нет назначенного описания столбца
    [TestCase(0, 2, 1)] // преобразованное значение
    [TestCase(2, 2, -1)] // ошибка
    [TestCase(4, 2, 55)] // несмотря на предупреждение, результат возвращается
    public void Item_get_byColIndex(int rowIndex, int colIndex, object wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.Init(); // включая строку [4] с предупреждением
      sut.SelColumns.AsString = "Col2,,Col1";

      object res = sut[rowIndex, colIndex];
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(0, "Col2", "AAA")]
    [TestCase(3, "Col2", null)] // пропущенная строка
    [TestCase(0, "", null)] // Нет назначенного описания столбца
    [TestCase(0, "Col1", 1)] // преобразованное значение
    [TestCase(2, "Col1", -1)] // ошибка
    [TestCase(4, "Col1", 55)] // несмотря на предупреждение, результат возвращается
    public void Item_get_byColCode(int rowIndex, string colCode, object wantedRes)
    {
      UISelRCGridData sut = CreateTestData();
      sut.SelRows.Init(); // включая строку [4] с предупреждением
      sut.SelColumns.AsString = "Col2,,Col1";

      object res = sut[rowIndex, colCode];
      Assert.AreEqual(wantedRes, res);
    }

    #endregion
  }
}
