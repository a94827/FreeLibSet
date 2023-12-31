using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Reporting;

namespace ExtTools_tests.Reporting
{
  [TestFixture]
  public class BRLineTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_2args()
    {
      BRLineStyle style = BRLineStyle.DashDotDot;
      BRColor color = BRColor.Cyan;
      BRLine sut = new BRLine(style, color);

      Assert.AreEqual(style, sut.Style, "Style");
      Assert.AreEqual(color, sut.Color, "Color");
    }

    [Test]
    public void Constructor_1arg()
    {
      BRLineStyle style = BRLineStyle.DashDot;
      BRLine sut = new BRLine(style);

      Assert.AreEqual(style, sut.Style, "Style");
      Assert.AreEqual(BRColor.Auto, sut.Color, "Color");
    }

    #endregion

    #region Предопределенные значения

    [Test]
    public void None()
    {
      BRLine sut = BRLine.None;

      Assert.AreEqual(BRLineStyle.None, sut.Style, "Style");
      Assert.AreEqual(BRColor.Auto, sut.Color, "Color");
    }

    [Test]
    public void Thin()
    {
      BRLine sut = BRLine.Thin;

      Assert.AreEqual(BRLineStyle.Thin, sut.Style, "Style");
      Assert.AreEqual(BRColor.Auto, sut.Color, "Color");
    }

    [Test]
    public void Medium()
    {
      BRLine sut = BRLine.Medium;

      Assert.AreEqual(BRLineStyle.Medium, sut.Style, "Style");
      Assert.AreEqual(BRColor.Auto, sut.Color, "Color");
    }

    [Test]
    public void Thick()
    {
      BRLine sut = BRLine.Thick;

      Assert.AreEqual(BRLineStyle.Thick, sut.Style, "Style");
      Assert.AreEqual(BRColor.Auto, sut.Color, "Color");
    }

    #endregion

    #region Операторы сравнения

    public class EqualsTestInfo
    {
      #region Конструктор

      public EqualsTestInfo(BRLine arg1, BRLine arg2, bool wantedRes)
      {
        _Arg1 = arg1;
        _Arg2 = arg2;
        _WantedRes = wantedRes;
      }

      #endregion

      #region Свойства

      public BRLine Arg1 { get { return _Arg1; } }
      private readonly BRLine _Arg1;

      public BRLine Arg2 { get { return _Arg2; } }
      private readonly BRLine _Arg2;

      public bool WantedRes { get { return _WantedRes; } }
      private readonly bool _WantedRes;

      #endregion
    }

    public static EqualsTestInfo[] Equals_Source
    {
      get
      {
        List<EqualsTestInfo> lst = new List<EqualsTestInfo>();

        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.Thin, BRColor.Red), new BRLine(BRLineStyle.Thin, BRColor.Red), true));
        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.Thin, BRColor.Auto), new BRLine(BRLineStyle.Thin, BRColor.Auto), true));
        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.Thin, BRColor.Red), new BRLine(BRLineStyle.Thick, BRColor.Red), false));
        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.Thin, BRColor.Red), new BRLine(BRLineStyle.Thin, BRColor.Green), false));
        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.Thin, BRColor.Black), new BRLine(BRLineStyle.Thin, BRColor.Auto), false));
        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.None, BRColor.Red), new BRLine(BRLineStyle.None, BRColor.Green), true)); // Color ignored
        lst.Add(new EqualsTestInfo(new BRLine(BRLineStyle.None, BRColor.Red), new BRLine(BRLineStyle.Thin, BRColor.Red), false));

        return lst.ToArray();
      }
    }

    [TestCaseSource("Equals_Source")]
    public void Equals(EqualsTestInfo info)
    {
      DoEquals(info.Arg1, info.Arg2, info.WantedRes);
      DoEquals(info.Arg2, info.Arg1, info.WantedRes);
    }

    private static void DoEquals(BRLine arg1, BRLine arg2, bool wantedRes)
    {
      bool res1 = (arg1 == arg2);
      Assert.AreEqual(wantedRes, res1, "Operator ==");

      bool res2 = (arg1 != arg2);
      Assert.AreEqual(wantedRes, !res2, "Operator !=");

      bool res3 = arg1.Equals(arg2);
      Assert.AreEqual(wantedRes, res3, "Equals(BRLine)");

      bool res4 = arg1.Equals((object)arg2);
      Assert.AreEqual(wantedRes, res4, "Equals(object)");
    }

    #endregion

    #region Логические операторы

    public class OperatorOrTestInfo
    {
      #region Конструктор

      public OperatorOrTestInfo(BRLine arg1, BRLine arg2, BRLine wantedRes)
      {
        _Arg1 = arg1;
        _Arg2 = arg2;
        _WantedRes = wantedRes;
      }

      #endregion

      #region Свойства

      public BRLine Arg1 { get { return _Arg1; } }
      private readonly BRLine _Arg1;

      public BRLine Arg2 { get { return _Arg2; } }
      private readonly BRLine _Arg2;

      public BRLine WantedRes { get { return _WantedRes; } }
      private readonly BRLine _WantedRes;

      #endregion
    }


    public static OperatorOrTestInfo[] Operator_Or_Source
    {
      get
      {
        List<OperatorOrTestInfo> lst = new List<OperatorOrTestInfo>();
        lst.Add(new OperatorOrTestInfo(BRLine.None, BRLine.None, BRLine.None));
        lst.Add(new OperatorOrTestInfo(BRLine.Thin, BRLine.Thin, BRLine.Thin));
        lst.Add(new OperatorOrTestInfo(BRLine.None, BRLine.Thin, BRLine.Thin));
        lst.Add(new OperatorOrTestInfo(BRLine.Medium, BRLine.Medium, BRLine.Medium));
        lst.Add(new OperatorOrTestInfo(BRLine.None, BRLine.Medium, BRLine.Medium));
        lst.Add(new OperatorOrTestInfo(BRLine.Thin, BRLine.Medium, BRLine.Medium));
        lst.Add(new OperatorOrTestInfo(BRLine.Thick, BRLine.Thick, BRLine.Thick));
        lst.Add(new OperatorOrTestInfo(BRLine.None, BRLine.Thick, BRLine.Thick));
        lst.Add(new OperatorOrTestInfo(BRLine.Thin, BRLine.Thick, BRLine.Thick));
        lst.Add(new OperatorOrTestInfo(BRLine.Medium, BRLine.Thick, BRLine.Thick));

        lst.Add(new OperatorOrTestInfo(BRLine.None, new BRLine(BRLineStyle.Dot), new BRLine(BRLineStyle.Dot)));
        lst.Add(new OperatorOrTestInfo(BRLine.None, new BRLine(BRLineStyle.Dash), new BRLine(BRLineStyle.Dash)));
        lst.Add(new OperatorOrTestInfo(BRLine.None, new BRLine(BRLineStyle.DashDot), new BRLine(BRLineStyle.DashDot)));
        lst.Add(new OperatorOrTestInfo(BRLine.None, new BRLine(BRLineStyle.DashDotDot), new BRLine(BRLineStyle.DashDotDot)));

        lst.Add(new OperatorOrTestInfo(BRLine.Thin, new BRLine(BRLineStyle.Dot), BRLine.Thin));
        lst.Add(new OperatorOrTestInfo(BRLine.Thin, new BRLine(BRLineStyle.Dash), BRLine.Thin));
        lst.Add(new OperatorOrTestInfo(BRLine.Thin, new BRLine(BRLineStyle.DashDot), BRLine.Thin));
        lst.Add(new OperatorOrTestInfo(BRLine.Thin, new BRLine(BRLineStyle.DashDotDot), BRLine.Thin));

        lst.Add(new OperatorOrTestInfo(BRLine.Medium, new BRLine(BRLineStyle.Dot), BRLine.Medium));
        lst.Add(new OperatorOrTestInfo(BRLine.Medium, new BRLine(BRLineStyle.Dash), BRLine.Medium));
        lst.Add(new OperatorOrTestInfo(BRLine.Medium, new BRLine(BRLineStyle.DashDot), BRLine.Medium));
        lst.Add(new OperatorOrTestInfo(BRLine.Medium, new BRLine(BRLineStyle.DashDotDot), BRLine.Medium));

        lst.Add(new OperatorOrTestInfo(BRLine.Thick, new BRLine(BRLineStyle.Dot), BRLine.Thick));
        lst.Add(new OperatorOrTestInfo(BRLine.Thick, new BRLine(BRLineStyle.Dash), BRLine.Thick));
        lst.Add(new OperatorOrTestInfo(BRLine.Thick, new BRLine(BRLineStyle.DashDot), BRLine.Thick));
        lst.Add(new OperatorOrTestInfo(BRLine.Thick, new BRLine(BRLineStyle.DashDotDot), BRLine.Thick));

        lst.Add(new OperatorOrTestInfo(
          new BRLine(BRLineStyle.Thin, BRColor.Auto),
          new BRLine(BRLineStyle.Thick, BRColor.Black),
          new BRLine(BRLineStyle.Thick, BRColor.Black)));

        lst.Add(new OperatorOrTestInfo(
          new BRLine(BRLineStyle.Thin, BRColor.Red), 
          new BRLine(BRLineStyle.Thick, BRColor.Yellow), 
          new BRLine(BRLineStyle.Thick, BRColor.Red)));
        lst.Add(new OperatorOrTestInfo(
          new BRLine(BRLineStyle.Thick, BRColor.Green),
          new BRLine(BRLineStyle.Thin, BRColor.Yellow),
          new BRLine(BRLineStyle.Thick, BRColor.Green)));

        return lst.ToArray();
      }
    }

    [TestCaseSource("Operator_Or_Source")]
    public void Operator_Or(OperatorOrTestInfo info)
    {
      BRLine res1 = info.Arg1 | info.Arg2;
      Assert.AreEqual(info.WantedRes, res1, "#1");

      BRLine res2 = info.Arg2 | info.Arg1;
      Assert.AreEqual(info.WantedRes, res2, "#2");
    }

    #endregion

    #region Толщина линии

    [TestCase(false)]
    [TestCase(true)]
    public void GetLineWidthPt_GetLineWidthPt01mm(bool function01mm)
    {
      foreach (BRLineStyle lineStyle in Enum.GetValues(typeof(BRLineStyle)))
      {
        double res = DoGetLineWidth(lineStyle, function01mm);
        if (lineStyle == BRLineStyle.None)
          Assert.AreEqual(0.0, res, lineStyle.ToString());
        else
          Assert.Greater(res, 0.0, lineStyle.ToString());
      }

      double resThin = DoGetLineWidth(BRLineStyle.Thin, function01mm);
      double resMedium = DoGetLineWidth(BRLineStyle.Medium, function01mm);
      double resThick = DoGetLineWidth(BRLineStyle.Thick, function01mm);

      Assert.Greater(resMedium, resThin, "Medium > Thin");
      Assert.Greater(resThick, resMedium, "Thick > Medium");
    }

    private static double DoGetLineWidth(BRLineStyle lineStyle, bool function01mm)
    {
      if (function01mm)
        return BRLine.GetLineWidthPt01mm(lineStyle);
      else
        return BRLine.GetLineWidthPt(lineStyle);
    }

    #endregion
  }
}
