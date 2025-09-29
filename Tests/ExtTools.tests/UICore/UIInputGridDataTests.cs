using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;
using System.Data;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UIInputGridDataTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_default()
    {
      UIInputGridData sut = new UIInputGridData();

      Assert.IsNotNull(sut.Table, "Table");
      Assert.IsFalse(sut.CanBeEmpty, "CanBeEmpty");
      Assert.AreEqual(UIValidateState.Error, sut.CanBeEmptyMode, "CanBeEmptyMode");
    }

    [Test]
    public void Constructor_DataTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(DateTime));
      UIInputGridData sut = new UIInputGridData(tbl);

      Assert.AreSame(tbl, sut.Table, "Table");
      Assert.IsFalse(sut.CanBeEmpty, "CanBeEmpty");
      Assert.AreEqual(UIValidateState.Error, sut.CanBeEmptyMode, "CanBeEmptyMode");

      Assert.AreEqual("F1", sut.Columns[0].ColumnName, "Columns[0].ColumnName");
      Assert.AreSame(tbl.Columns[0], sut.Columns[0].Column, "Columns[0].Column");
      Assert.AreSame(tbl.Columns[0], sut.Columns["F1"].Column, "Columns[F1].Column");
      Assert.AreEqual(UIHorizontalAlignment.Left, sut.Columns[0].TextAlign, "Columns[0].TextAlign");

      Assert.AreEqual("F2", sut.Columns[1].ColumnName, "Columns[1].ColumnName");
      Assert.AreSame(tbl.Columns[1], sut.Columns[1].Column, "Columns[1].Column");
      Assert.AreSame(tbl.Columns[1], sut.Columns["F2"].Column, "Columns[F2].Column");
      Assert.AreEqual(UIHorizontalAlignment.Right, sut.Columns[1].TextAlign, "Columns[1].TextAlign");

      Assert.AreEqual("F3", sut.Columns[2].ColumnName, "Columns[2].ColumnName");
      Assert.AreSame(tbl.Columns[2], sut.Columns[2].Column, "Columns[2].Column");
      Assert.AreSame(tbl.Columns[2], sut.Columns["F3"].Column, "Columns[F3].Column");
      Assert.AreEqual(UIHorizontalAlignment.Center, sut.Columns[2].TextAlign, "Columns[2].TextAlign");
    }


    #endregion

    #region CanBeEmptyMode и CanBeEmpty

    [TestCase(UIValidateState.Error, false)]
    [TestCase(UIValidateState.Warning, true)]
    [TestCase(UIValidateState.Ok, true)]
    public void CanBeEmptyMode(UIValidateState canBeEmptyMode, bool wantedCanBeEmpty)
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(DateTime));
      tbl.Columns.Add("F4", typeof(int));
      UIInputGridData sut = new UIInputGridData(tbl);

      // не устанавливаем sut.Columns[0].CanBeEmptyMode = UIValidateState.Error;
      sut.Columns[1].CanBeEmptyMode = UIValidateState.Error;
      sut.Columns[2].CanBeEmptyMode = UIValidateState.Warning;
      sut.Columns[3].CanBeEmptyMode = UIValidateState.Ok;

      Assert.AreEqual(UIValidateState.Error, sut.CanBeEmptyMode, "CanBeEmptyMode #1");
      Assert.IsFalse(sut.CanBeEmpty, "CanBeEmpty #1");

      sut.CanBeEmptyMode = canBeEmptyMode;

      Assert.AreEqual(canBeEmptyMode, sut.CanBeEmptyMode, "CanBeEmptyMode #2");
      Assert.AreEqual(wantedCanBeEmpty, sut.CanBeEmpty, "CanBeEmpty #2");
    }

    [TestCase(false, UIValidateState.Error)]
    [TestCase(true, UIValidateState.Ok)]
    public void CanBeEmpty(bool canBeEmpty, UIValidateState wantedCanBeEmptyMode)
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(DateTime));
      tbl.Columns.Add("F4", typeof(int));
      UIInputGridData sut = new UIInputGridData(tbl);

      // не устанавливаем sut.Columns[0].CanBeEmptyMode = UIValidateState.Error;
      sut.Columns[1].CanBeEmptyMode = UIValidateState.Error;
      sut.Columns[2].CanBeEmptyMode = UIValidateState.Warning;
      sut.Columns[3].CanBeEmptyMode = UIValidateState.Ok;

      Assert.AreEqual(UIValidateState.Error, sut.CanBeEmptyMode, "CanBeEmptyMode #1");
      Assert.IsFalse(sut.CanBeEmpty, "CanBeEmpty #1");

      sut.CanBeEmpty = canBeEmpty;

      Assert.AreEqual(canBeEmpty, sut.CanBeEmpty, "CanBeEmpty #2");
      Assert.AreEqual(wantedCanBeEmptyMode, sut.CanBeEmptyMode, "CanBeEmptyMode #2");

      for (int i = 0; i < tbl.Columns.Count; i++)
      {
        Assert.AreEqual(canBeEmpty, sut.Columns[i].CanBeEmpty, "Columns[].CanBeEmpty #2");
        Assert.AreEqual(wantedCanBeEmptyMode, sut.Columns[i].CanBeEmptyMode, "Columns[].CanBeEmptyMode #2");
      }
    }

    #endregion

    #region ColumnInfo

    [Test]
    public void ColumnInfo_Constructor()
    {
      UIInputGridData data = new UIInputGridData();
      data.Table.Columns.Add("F1", typeof(string));
      UIInputGridData.ColumnInfo sut = data.Columns[0];

      Assert.AreEqual(UIHorizontalAlignment.Left, sut.TextAlign, "TextAlign");
      Assert.AreEqual("F1", sut.ColumnName, "ColumnName");
      Assert.AreEqual("", sut.Format, "Format");
      Assert.AreEqual(UIValidateState.Error, sut.CanBeEmptyMode, "CanBeEmptyMode");
      Assert.IsFalse(sut.HasValidators, "HasValidators");
      Assert.AreEqual(0, sut.FillWeight, "FillWeight");
    }

    [TestCase(typeof(String), UIHorizontalAlignment.Left)]
    [TestCase(typeof(Int32), UIHorizontalAlignment.Right)]
    [TestCase(typeof(Single), UIHorizontalAlignment.Right)]
    [TestCase(typeof(Double), UIHorizontalAlignment.Right)]
    [TestCase(typeof(Decimal), UIHorizontalAlignment.Right)]
    [TestCase(typeof(DateTime), UIHorizontalAlignment.Center)]
    [TestCase(typeof(bool), UIHorizontalAlignment.Center)]
    public void ColumnInfo_TextAlign(Type colType, UIHorizontalAlignment wantedAlign)
    {
      UIInputGridData data = new UIInputGridData();
      data.Table.Columns.Add("F1", colType);
      UIInputGridData.ColumnInfo sut = data.Columns[0];
      Assert.AreEqual(wantedAlign, sut.TextAlign, "TextAlign #1");

      sut.TextAlign = UIHorizontalAlignment.Center;
      Assert.AreEqual(UIHorizontalAlignment.Center, sut.TextAlign, "TextAlign #2");
    }

    #endregion

    #region Validators

    [Test]
    public void Validators()
    {
      UIInputGridData data = new UIInputGridData();
      data.Table.Columns.Add("F1", typeof(int));

      UIInputGridData.ColumnInfo sut = data.Columns[0];
      sut.Validators.AddError(new DepComparer<int>(sut.Validators.AsIntEx, 2, DepCompareKind.LessThan), "Value should be less than 2");
      Assert.IsTrue(sut.HasValidators, "HasValidators");

      data.Table.Rows.Add(1);
      data.Table.Rows.Add(2);

      UISimpleValidableObject vo;
      data.InternalSetValidatingRow(data.Table.Rows[0]); // F1=1
      vo = new UISimpleValidableObject();
      sut.Validators.Validate(vo);
      Assert.AreEqual(UIValidateState.Ok, vo.ValidateState, "#1");

      data.InternalSetValidatingRow(data.Table.Rows[1]); // F1=2
      vo = new UISimpleValidableObject();
      sut.Validators.Validate(vo);
      Assert.AreEqual(UIValidateState.Error, vo.ValidateState, "#1");
    }

    #endregion
  }
}
