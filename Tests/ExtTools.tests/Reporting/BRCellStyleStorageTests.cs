using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;
using System.Globalization;
using FreeLibSet.Tests;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRCellStyleStorageTests
  {
    #region Конструктор

    [Test]
    public void Constructor_null()
    {
      BRCellStyleStorage sut = new BRCellStyleStorage(null);
      ValidateProps(sut);
      CompareProps(BRReport.AppDefaultCellStyle, sut);
    }


    [Test]
    public void Constructor_withRef()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontHeightPt = 12;
      arg.LineHeightPt = 16;
      arg.Bold = true;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      ValidateProps(sut);
      CompareProps(arg, sut);
    }

    internal static void ValidateProps(BRCellStyle sut)
    {

      ExtStringAssert.IsNotNullOrEmpty(sut.FontName, "FontName");
      Assert.Greater(sut.FontHeightTwip, 0, "FontHeightTwip");
      Assert.Greater(sut.FontHeightPt, 0f, "FontHeightPt");
      Assert.GreaterOrEqual(sut.LineHeightTwip, 0, "LineHeightTwip");
      Assert.GreaterOrEqual(sut.LineHeightPt, 0f, "LineHeightPt");
      Assert.GreaterOrEqual(sut.FontWidthTwip, 0, "FontWidthTwip");
      Assert.GreaterOrEqual(sut.FontWidthPt, 0f, "FontWidthPt");
      Assert.GreaterOrEqual(sut.FontWidthPercent, BRReport.MinFontWidthPercent, "FontWidthPercent min");
      Assert.LessOrEqual(sut.FontWidthPercent, BRReport.MaxFontWidthPercent, "FontWidthPercent max");
      Assert.GreaterOrEqual(sut.MaxEnlargePercent, BRReport.MinFontEnlargePercent, "MaxEnlargePercent min");
      Assert.LessOrEqual(sut.MaxEnlargePercent, BRReport.MaxFontEnlargePercent, "MaxEnlargePercent max");

      Assert.GreaterOrEqual(sut.LeftMargin, 0, "LeftMargin");
      Assert.GreaterOrEqual(sut.TopMargin, 0, "TopMargin");
      Assert.GreaterOrEqual(sut.RightMargin, 0, "RightMargin");
      Assert.GreaterOrEqual(sut.BottomMargin, 0, "BottomMargin");

      Assert.GreaterOrEqual(sut.IndentLevel, BRReport.MinIndentLevel, "IndentLevel min");
      Assert.LessOrEqual(sut.IndentLevel, BRReport.MaxIndentLevel, "IndentLevel max");
    }

    internal static void CompareProps(BRCellStyle wanted, BRCellStyle sut)
    {
      Assert.AreEqual(wanted.FontName, sut.FontName, "FontName");
      Assert.AreEqual(wanted.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(wanted.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(wanted.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(wanted.LineHeightPt, sut.LineHeightPt, "LineHeightPt");
      Assert.AreEqual(wanted.FontWidthTwip, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(wanted.FontWidthPt, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(wanted.FontWidthPercent, sut.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(wanted.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(wanted.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");

      Assert.AreEqual(wanted.Bold, sut.Bold, "Bold");
      Assert.AreEqual(wanted.Italic, sut.Italic, "Italic");
      Assert.AreEqual(wanted.Underline, sut.Underline, "Underline");
      Assert.AreEqual(wanted.Strikeout, sut.Strikeout, "Strikeout");

      Assert.AreEqual(wanted.BackColor, sut.BackColor, "BackColor");
      Assert.AreEqual(wanted.ForeColor, sut.ForeColor, "ForeColor");

      Assert.AreEqual(wanted.HAlign, sut.HAlign, "HAlign");
      Assert.AreEqual(wanted.VAlign, sut.VAlign, "VAlign");
      Assert.AreEqual(wanted.WrapMode, sut.WrapMode, "WrapMode");

      Assert.AreEqual(wanted.LeftMargin, sut.LeftMargin, "LeftMargin");
      Assert.AreEqual(wanted.TopMargin, sut.TopMargin, "TopMargin");
      Assert.AreEqual(wanted.RightMargin, sut.RightMargin, "RightMargin");
      Assert.AreEqual(wanted.BottomMargin, sut.BottomMargin, "BottomMargin");

      Assert.AreEqual(wanted.IndentLevel, sut.IndentLevel, "IndentLevel");

      Assert.AreEqual(wanted.Format, sut.Format, "Format");
      Assert.AreSame(wanted.FormatProvider, sut.FormatProvider, "FormatProvider");

      Assert.AreEqual(wanted.TextFiller, sut.TextFiller, "TextFiller");

      Assert.AreEqual(wanted.LeftBorder, sut.LeftBorder, "LeftBorder");
      Assert.AreEqual(wanted.TopBorder, sut.TopBorder, "TopBorder");
      Assert.AreEqual(wanted.RightBorder, sut.RightBorder, "RightBorder");
      Assert.AreEqual(wanted.BottomBorder, sut.BottomBorder, "BottomBorder");
      Assert.AreEqual(wanted.DiagonalUp, sut.DiagonalUp, "DiagonalUp");
      Assert.AreEqual(wanted.DiagonalDown, sut.DiagonalDown, "DiagonalDown");

      Assert.AreEqual(wanted.ParentStyleName, sut.ParentStyleName, "ParentStyleName");
      Assert.AreSame(wanted.ParentStyle, sut.ParentStyle, "ParentStyle");
    }

    #endregion

    #region Типизированные свойства

    #region Шрифт

    [Test]
    public void FontName()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontName = "Jabberwokey";
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual("Jabberwokey", sut.FontName);
    }

    [Test]
    public void FontHeightPt()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontHeightPt = 16f;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(16f, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(16 * 20, sut.FontHeightTwip, "FontHeightTwip");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightPt, sut.LineHeightPt, "LineHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthTwip, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPt, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPercent, sut.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }


    [Test]
    public void FontHeightTwip()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontHeightTwip = 16 * 20;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(16f, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(16 * 20, sut.FontHeightTwip, "FontHeightTwip");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightPt, sut.LineHeightPt, "LineHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthTwip, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPt, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPercent, sut.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }

    [Test]
    public void LineHeightPt()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.LineHeightPt = 16f;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(16f, sut.LineHeightPt, "LineHeightPt");
      Assert.AreEqual(16 * 20, sut.LineHeightTwip, "LineHeightTwip");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthTwip, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPt, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPercent, sut.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }

    [Test]
    public void LineHeightTwip()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.LineHeightTwip = 16 * 20;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(16f, sut.LineHeightPt, "LineHeightPt");
      Assert.AreEqual(16 * 20, sut.LineHeightTwip, "LineHeightTwip");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthTwip, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPt, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontWidthPercent, sut.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }

    [Test]
    public void FontWidthPt()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontWidthPercent = 120; // will be reset to 100%
      arg.FontWidthPt = 6f;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(6f, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(6 * 20, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(100, sut.FontWidthPercent, "FontWidthPercent");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }

    [Test]
    public void FontWidthTwip()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontWidthPercent = 120; // will be reset to 100%
      arg.FontWidthTwip = 6 * 20;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(6f, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(6 * 20, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(100, sut.FontWidthPercent, "FontWidthPercent");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }

    [Test]
    public void FontWidthPercent()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontWidthPt = 6f; // will be reset to zero
      arg.FontWidthPercent = 120;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(0f, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(0, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(120, sut.FontWidthPercent, "FontWidthPercent");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.MaxEnlargePercent, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.AlwaysEnlarge, sut.AlwaysEnlarge, "AlwaysEnlarge");
    }

    [Test]
    public void MaxEnlargePercent()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FontWidthPt = 6f; // will be reset to zero
      arg.MaxEnlargePercent = 150;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(6f, sut.FontWidthPt, "FontWidthPt");
      Assert.AreEqual(6 * 20, sut.FontWidthTwip, "FontWidthTwip");
      Assert.AreEqual(100, sut.FontWidthPercent, "FontWidthPercent");
      Assert.AreEqual(150, sut.MaxEnlargePercent, "MaxEnlargePercent");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
    }

    [Test]
    public void AlwaysEnlargePercent([Values(false, true)]bool value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.MaxEnlargePercent = 150;
      arg.AlwaysEnlarge = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(150, sut.MaxEnlargePercent, "MaxEnlargePercent");
      Assert.AreEqual(value, sut.AlwaysEnlarge, "AlwaysEnlarge");

      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightPt, sut.FontHeightPt, "FontHeightPt");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.FontHeightTwip, sut.FontHeightTwip, "FontHeightTwip");
      Assert.AreEqual(BRReport.AppDefaultCellStyle.LineHeightTwip, sut.LineHeightTwip, "LineHeightTwip");
    }

    [Test]
    public void Bold([Values(false, true)]bool value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.Bold = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.Bold);
    }

    [Test]
    public void Italic([Values(false, true)]bool value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.Italic = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.Italic);
    }

    [Test]
    public void Underline([Values(false, true)]bool value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.Underline = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.Underline);
    }

    [Test]
    public void Strikeout([Values(false, true)]bool value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.Strikeout = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.Strikeout);
    }

    #endregion

    #region Цвета

    [Test]
    public void BackColor()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.BackColor = BRColor.Green;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(BRColor.Green, sut.BackColor);
    }

    [Test]
    public void ForeColor()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.ForeColor = BRColor.Yellow;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(BRColor.Yellow, sut.ForeColor);
    }

    #endregion

    #region Выравнивание

    [Test]
    public void HAlign([Values(BRHAlign.Auto, BRHAlign.Left, BRHAlign.Center, BRHAlign.Right)]BRHAlign value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.HAlign = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.HAlign);
    }

    [Test]
    public void VAlign([Values(BRVAlign.Top, BRVAlign.Center, BRVAlign.Bottom)]BRVAlign value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.VAlign = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.VAlign);
    }

    [Test]
    public void WrapMode([Values(BRWrapMode.NoWrap, BRWrapMode.WordWrap)]BRWrapMode value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.WrapMode = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.WrapMode);
    }

    #endregion

    #region Отступы

    [Test]
    public void LeftMargin([Values(0, 20)]int value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.LeftMargin = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.LeftMargin);
    }

    [Test]
    public void TopMargin([Values(0, 20)]int value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.TopMargin = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.TopMargin);
    }

    [Test]
    public void RightMargin([Values(0, 20)]int value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.RightMargin = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.RightMargin);
    }

    [Test]
    public void BottomMargin([Values(0, 20)]int value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.BottomMargin = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.BottomMargin);
    }


    [Test]
    public void AllMargins()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.LeftMargin = 10;
      arg.RightMargin = 20;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.Less(0, sut.AllMargins, "Different");

      arg.LeftMargin = 10;
      arg.TopMargin = 10;
      arg.RightMargin = 10;
      arg.BottomMargin = 10;
      Assert.AreEqual(10, sut.AllMargins, "Manual border set");

      arg.AllMargins = 20;
      Assert.AreEqual(20, sut.AllMargins, "AllMargins set");
    }

    [Test]
    public void IndentLevel([Values(0, 3)]int value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.IndentLevel = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.IndentLevel);
    }

    #endregion

    #region Формат

    [Test]
    public void Format([Values("", "yy/MM/dd")]string value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.Format = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.Format);
    }

    [Test]
    public void FormatProvider([Values(null, "en-US", "ru-RU")]string culture)
    {
      CultureInfo ci;
      if (culture == null)
        ci = CultureInfo.CurrentCulture;
      else
        ci = CultureInfo.GetCultureInfo(culture);

      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.FormatProvider = ci;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreSame(ci, sut.FormatProvider, "FormatProvider");

      Assert.AreSame(ci.NumberFormat, sut.NumberFormat, "NumberFormat");
      Assert.AreSame(ci.DateTimeFormat, sut.DateTimeFormat, "DateTimeFormat");
    }

    #endregion

    #region Заполнитель

    [Test]
    public void TextFiller([Values(BRTextFiller.None, BRTextFiller.Thin, BRTextFiller.Medium, BRTextFiller.Thick, BRTextFiller.TwoLines)]BRTextFiller value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.TextFiller = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.TextFiller);
    }

    #endregion

    #region Границы

    public static BRLine[] BorderTestSource { get { return new BRLine[] { BRLine.None, BRLine.Thin }; } }

    [Test]
    public void LeftBorder([ValueSource("BorderTestSource")]BRLine value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.LeftBorder = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.LeftBorder);
    }

    [Test]
    public void TopBorder([ValueSource("BorderTestSource")]BRLine value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.TopBorder = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.TopBorder);
    }

    [Test]
    public void RightBorder([ValueSource("BorderTestSource")]BRLine value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.RightBorder = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.RightBorder);
    }

    [Test]
    public void BottomBorder([ValueSource("BorderTestSource")]BRLine value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.BottomBorder = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.BottomBorder);
    }

    [Test]
    public void DiagonalUp([ValueSource("BorderTestSource")]BRLine value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.DiagonalUp = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.DiagonalUp);
    }

    [Test]
    public void DiagonalDown([ValueSource("BorderTestSource")]BRLine value)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.DiagonalDown = value;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(value, sut.DiagonalDown);
    }

    [Test]
    public void AllBorders()
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.LeftBorder = BRLine.Thin;
      arg.TopBorder = BRLine.Thick;
      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.AreEqual(BRLine.Thick, sut.AllBorders, "Different"); // возвращается самая толстая линия

      arg.LeftBorder = BRLine.Medium;
      arg.TopBorder = BRLine.Medium;
      arg.RightBorder = BRLine.Medium;
      arg.BottomBorder = BRLine.Medium;
      arg.DiagonalUp = BRLine.Thick; // no matter
      arg.DiagonalDown = BRLine.Thick; // no matter
      Assert.AreEqual(BRLine.Medium, sut.AllBorders, "Manual set");

      arg.AllBorders = BRLine.Thin;
      Assert.AreEqual(BRLine.Thin, sut.AllBorders, "AllBorders set");
      Assert.AreEqual(BRLine.Thick, sut.DiagonalUp, "DiagonalUp");
      Assert.AreEqual(BRLine.Thick, sut.DiagonalDown, "DiagonalDown");
    }

    [Test]
    public void AreaAllBordersSame([Values(1, 2, 3, 4)]int test)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.AllBorders = BRLine.Thin;
      arg.DiagonalUp = BRLine.Medium;
      arg.DiagonalDown = BRLine.Medium;

      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.IsTrue(sut.AreaAllBordersSame, "#1");

      switch (test)
      {
        case 1: arg.LeftBorder = BRLine.Thick; break;
        case 2: arg.TopBorder = BRLine.Thick; break;
        case 3: arg.RightBorder = BRLine.Thick; break;
        case 4: arg.BottomBorder = BRLine.Thick; break;
      }
      Assert.IsFalse(sut.AreaAllBordersSame, "#2");
    }


    [Test]
    public void HasBorders([Values(1, 2, 3, 4)]int test)
    {
      BRCellStyleStorage arg = new BRCellStyleStorage(null);
      arg.AllBorders = BRLine.None;
      arg.DiagonalUp = BRLine.Medium;
      arg.DiagonalDown = BRLine.Medium;

      BRCellStyleStorage sut = new BRCellStyleStorage(arg);
      Assert.IsFalse(sut.HasBorders, "#1");

      switch (test)
      {
        case 1: arg.LeftBorder = BRLine.Thick; break;
        case 2: arg.TopBorder = BRLine.Thick; break;
        case 3: arg.RightBorder = BRLine.Thick; break;
        case 4: arg.BottomBorder = BRLine.Thick; break;
      }
      Assert.IsTrue(sut.HasBorders, "#2");
    }

    #endregion

    #region Родительский стиль

    [Test]
    public void ParentStyle()
    {
      BRReport report = new BRReport();
      report.DefaultCellStyle.FontHeightPt = 12f;
      BRNamedCellStyle named1 = report.NamedCellStyles.Add("Style1");
      Assert.AreEqual(12 * 20, named1.FontHeightTwip, "FontHeightTwip #1.1");
      named1.FontHeightPt = 16f;
      BRNamedCellStyle named2 = report.NamedCellStyles.Add("Style2");
      Assert.AreEqual(12 * 20, named2.FontHeightTwip, "FontHeightTwip #1.2");
      named2.FontHeightPt = 18f;

      BRCellStyleStorage sut = new BRCellStyleStorage(report.DefaultCellStyle);
      Assert.IsNull(sut.ParentStyle, "ParentStyle #2");
      ExtStringAssert.IsNullOrEmpty(sut.ParentStyleName, "ParentStyleName #2");
      Assert.AreEqual(12 * 20, sut.FontHeightTwip, "FontHeightTwip #2");

      sut.ParentStyleName = "Style1";
      Assert.AreSame(named1, sut.ParentStyle, "ParentStyle #3");
      Assert.AreEqual("Style1", sut.ParentStyleName, "ParentStyleName #3");
      Assert.AreEqual(16 * 20, sut.FontHeightTwip, "FontHeightTwip #3");

      sut.ParentStyle = named2;
      Assert.AreSame(named2, sut.ParentStyle, "ParentStyle #4");
      Assert.AreEqual("Style2", sut.ParentStyleName, "ParentStyleName #4");
      Assert.AreEqual(18 * 20, sut.FontHeightTwip, "FontHeightTwip #4");
    }

    #endregion

    #endregion
  }
}
