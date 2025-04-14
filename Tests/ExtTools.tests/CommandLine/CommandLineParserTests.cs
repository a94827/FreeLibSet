using System;
using System.Collections.Generic;
using NUnit.Framework;
using FreeLibSet.CommandLine;
using FreeLibSet.Core;

namespace ExtTools_tests.CommandLine
{
  [TestFixture]
  public class CommandLineParserTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      CommandLineParser sut = new CommandLineParser();
      Assert.AreEqual(0, sut.OptionDefs.Count, "OptionDefs.Count");
      Assert.IsFalse(sut.CommonArgsEnabled, "CommonArgsEnabled");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
    }

    #endregion

    #region Действия

    internal static CommandLineParser CreateTestParserWithActions()
    {
      CommandLineParser parser = new CommandLineParser();
      parser.ActionMode = CommandLineActionMode.FirstOnly;
      parser.ActionDefs.Add("add,a");
      parser.ActionDefs.Add("remove,r");
      parser.ActionDefs.Add("list,l");
      parser.OptionDefs.Add("--help,-h");
      return parser;
    }

    [Test]
    public void Parse_Action_And_Option()
    {
      CommandLineParser sut = CreateTestParserWithActions();
      string[] args = new string[] { "list", "--help" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      CollectionAssert.AreEqual(new string[] { "list"}, sut.ActionValues, "ActionValues");
      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.IsTrue(sut.OptionValues.ContainsKey("help"), "OptionValues");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");
    }

    [Test]
    public void Parse_Action_And_Common()
    {
      CommandLineParser sut = CreateTestParserWithActions();
      sut.CommonArgsEnabled = true;
      string[] args = new string[] { "list", "add" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      CollectionAssert.AreEqual(new string[] { "list" }, sut.ActionValues, "ActionValues");
      Assert.AreEqual(0, sut.OptionValues.Count, "OptionValues.Count");
      CollectionAssert.AreEqual(new string[] { "add"}, sut.CommonValues, "CommonValues");
    }

    [Test]
    public void Parse_Action_Twice()
    {
      CommandLineParser sut = CreateTestParserWithActions();
      sut.CommonArgsEnabled = false;
      string[] args = new string[] { "list", "add" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");
    }

    [Test]
    public void ActionDefs_DiasbleAll_EnableAll()
    {
      CommandLineParser sut = CreateTestParserWithActions();
      DoTestActionEnabled(sut, true);

      sut.ActionDefs.DisableAll(null);
      DoTestActionEnabled(sut, false);

      sut.ActionDefs.EnableAll();
      DoTestActionEnabled(sut, true);

      sut.ActionDefs.DisableAll("XXX");
      DoTestActionEnabled(sut, false);
      foreach (CommandLineAction act in sut.ActionDefs)
        Assert.AreEqual("XXX", act.EnabledErrorMessage);
    }

    private static void DoTestActionEnabled(CommandLineParser sut, bool wantedValue)
    {
      foreach (CommandLineAction act in sut.ActionDefs)
        Assert.AreEqual(wantedValue, act.Enabled);
    }

    #endregion

    #region Опции

    internal static CommandLineParser CreateTestParserWithOptions()
    {
      CommandLineParser parser = new CommandLineParser();

      CommandLineOption opt1 = parser.OptionDefs.Add("--aaa,-a");
      CommandLineOption opt2 = parser.OptionDefs.Add("--bbb", CommandLineOptionValueMode.Single);
      CommandLineOption opt3 = parser.OptionDefs.Add("--ccc", CommandLineOptionValueMode.Optional);
      CommandLineOption opt4 = parser.OptionDefs.Add("--ddd", CommandLineOptionValueMode.Multi);

      Assert.AreEqual(CommandLineOptionValueMode.None, opt1.ValueMode, "aaa ValueType");
      Assert.AreEqual(CommandLineOptionValueMode.Single, opt2.ValueMode, "bbb ValueType");
      Assert.AreEqual(CommandLineOptionValueMode.Optional, opt3.ValueMode, "ccc ValueType");
      Assert.AreEqual(CommandLineOptionValueMode.Multi, opt4.ValueMode, "ddd ValueType");

      return parser;
    }

    [Test]
    public void Parse_Option_noargs()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = DataTools.EmptyStrings;
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(0, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");
    }

    [Test]
    public void Parse_Option_Unknown()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--xxx" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("xxx", sut.ErrorMessage, "ErrorMessage");
    }


    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Parse_Option_All(int rotate)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--aaa", "--bbb=123", "--ccc=456", "--ddd=789" };
      RotateArgs(args, rotate);

      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
    }

    private static void RotateArgs(string[] args, int rotate)
    {
      for (int i = 0; i < rotate; i++)
      {
        string[] tmp = new string[args.Length];
        Array.Copy(args, 0, tmp, 1, args.Length - 1);
        tmp[0] = args[args.Length - 1];
        Array.Copy(tmp, args, args.Length);
      }
    }

    [Test]
    public void IgnoreCase([Values(false, true)]bool ignoreCase)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.IgnoreCase = ignoreCase;
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      string[] args = new string[] { "--AAA" };

      bool res = sut.Parse(args, false);
      Assert.AreEqual(ignoreCase, res, "Result");
    }

    [TestCase(false, "--aaa", true)]
    [TestCase(true, "--aaa", true)]
    [TestCase(false, "-a", true)]
    [TestCase(true, "-a", true)]
    [TestCase(false, "/aaa", false)]
    [TestCase(true, "/aaa", true)]
    [TestCase(false, "/a", false)]
    [TestCase(true, "/a", true)]
    public void SlashedOptionsEnabled(bool slashedOptionsEnabled, string arg, bool wantedRes)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.SlashedOptionsEnabled = slashedOptionsEnabled;
      Assert.AreEqual(slashedOptionsEnabled, sut.SlashedOptionsEnabled, "SlashedOptionsEnabled");
      string[] args = new string[] { arg };

      bool res = sut.Parse(args, false);
      Assert.AreEqual(wantedRes, res, "Result");
    }


    #region ValueType=None

    [Test]
    public void Parse_Option_ValueTypeNone_ok()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--aaa" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("", sut.OptionValues["aaa"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeNone_withValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--aaa=123" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("aaa", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeNone_repeated()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--aaa", "--aaa" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("aaa", sut.ErrorMessage, "ErrorMessage");
    }

    #endregion

    #region ValueType=Single

    [Test]
    public void Parse_Option_ValueTypeSingle_ok_EqSign()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb=123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123", sut.OptionValues["bbb"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeSingle_ok_Space()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb", "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123", sut.OptionValues["bbb"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeSingle_NoValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("bbb", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeSingle_EqWithoutValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb=" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("bbb", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeSingle_NextIsOption()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb", "--aaa" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("bbb", sut.ErrorMessage, "ErrorMessage");
    }


    [Test]
    public void Parse_Option_ValueTypeSingle_NextIsSlashed()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.SlashedOptionsEnabled = true;
      string[] args = new string[] { "--bbb", "/aaa" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("bbb", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeSingle_ok_SpaceWithSlashedValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.SlashedOptionsEnabled = false;
      string[] args = new string[] { "--bbb", "/123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("/123", sut.OptionValues["bbb"], "OptionValues");
    }


    [Test]
    public void Parse_Option_ValueTypeSingle_TwoValues()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb", "123", "456" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      //StringAssert.Contains("456", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeSingle_repeated()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--bbb=123", "--bbb=456" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("bbb", sut.ErrorMessage, "ErrorMessage");
    }

    #endregion

    #region ValueType=Optional

    [Test]
    public void Parse_Option_ValueTypeOptional_ok_EqSign()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ccc=123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123", sut.OptionValues["ccc"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeOptional_ok_Space()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ccc", "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123", sut.OptionValues["ccc"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeOptional_ok_NoValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ccc" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("", sut.OptionValues["ccc"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeOptional_TwoValues()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ccc", "123", "456" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      //StringAssert.Contains("456", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeOptional_repeated()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ccc=123", "--ccc=456" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("ccc", sut.ErrorMessage, "ErrorMessage");
    }

    #endregion

    #region ValueType=Multi

    [Test]
    public void Parse_Option_ValueTypeMulti_ok_EqSign()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ddd=123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123", sut.OptionValues["ddd"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeMulti_ok_Space()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ddd", "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123", sut.OptionValues["ddd"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeMulti_ok_Repeated()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ddd", "123", "--ddd=456", "--ddd", "789" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, sut.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, sut.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123,456,789", sut.OptionValues["ddd"], "OptionValues");
    }

    [Test]
    public void Parse_Option_ValueTypeOptional_TwoValuesWithOneOption()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ddd", "123", "456" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      //StringAssert.Contains("456", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeMulti_NoValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ddd" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("ddd", sut.ErrorMessage, "ErrorMessage");
    }

    [Test]
    public void Parse_Option_ValueTypeMulti_EqWithoutValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      string[] args = new string[] { "--ddd=" };
      bool res = sut.Parse(args, false);

      Assert.IsFalse(res, "Result");

      StringAssert.Contains("ddd", sut.ErrorMessage, "ErrorMessage");
    }

    #endregion

    [Test]
    public void OptionDefs_DiasbleAll_EnableAll()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      DoTestOptionEnabled(sut, true);

      sut.OptionDefs.DisableAll(null);
      DoTestOptionEnabled(sut, false);

      sut.OptionDefs.EnableAll();
      DoTestOptionEnabled(sut, true);

      sut.OptionDefs.DisableAll("XXX");
      DoTestOptionEnabled(sut, false);
      foreach (CommandLineOption opt in sut.OptionDefs)
        Assert.AreEqual("XXX", opt.EnabledErrorMessage);
    }

    private static void DoTestOptionEnabled(CommandLineParser sut, bool wantedValue)
    {
      foreach (CommandLineOption opt in sut.OptionDefs)
        Assert.AreEqual(wantedValue, opt.Enabled);
    }

    #endregion

    #region CommonArgs

    [Test]
    public void CommonValues()
    {
      CommandLineParser sut = new CommandLineParser();
      sut.CommonArgsEnabled = true;

      string[] args = new string[] { "aaa", "bbb" };
      bool res = sut.Parse(args, false);
      Assert.IsTrue(res, "Result");
      Assert.AreEqual(0, sut.OptionValues.Count, "OptionValues.Count");
      CollectionAssert.AreEqual(new string[] { "aaa", "bbb" }, sut.CommonValues, "CommonValues");
    }

    public void CommonValues_withOptions()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = true;

      string[] args = new string[] { "--aaa", "aaa", "bbb", "--bbb=123", "--ccc" };
      bool res = sut.Parse(args, false);
      Assert.IsTrue(res, "Result");
      Assert.AreEqual(3, sut.OptionValues.Count, "OptionValues.Count");
      CollectionAssert.AreEqual(new string[] { "aaa", "bbb" }, sut.CommonValues, "CommonValues");
    }

    [Test]
    public void CommonArgsEnabled([Values(false, true)] bool commonArgsEnabled)
    {
      CommandLineParser sut = new CommandLineParser();
      sut.CommonArgsEnabled = commonArgsEnabled;
      Assert.AreEqual(commonArgsEnabled, sut.CommonArgsEnabled, "CommonArgsEnabled");
      string[] args = new string[] { "aaa", "bbb" };
      bool res = sut.Parse(args, false);
      Assert.AreEqual(commonArgsEnabled, res, "Result");
    }

    [Test]
    public void CommonArgsErrorMessage()
    {
      CommandLineParser sut = new CommandLineParser();
      sut.CommonArgsEnabled = false;
      sut.CommonArgsErrorMessage = "XXX";
      Assert.AreEqual("XXX", sut.CommonArgsErrorMessage, "CommonArgsErrorMessage");

      string[] args = new string[] { "aaa", "bbb" };
      bool res = sut.Parse(args, false);
      Assert.IsFalse(res, "Result");
      StringAssert.Contains("XXX", sut.ErrorMessage, "ErrorMessage");
    }

    #endregion

    #region Events

    private class EventHelper
    {
      public EventHelper(CommandLineParser parser)
      {
        parser.OptionFound += this.OptionFound;
        parser.CommonArgFound += this.CommonArgFound;


        _Messages = new List<string>();
      }

      public List<string> Messages { get { return _Messages; } }
      private List<string> _Messages;

      public void OptionFound(object sender, CommandLineOptionEventArgs args)
      {
        _Messages.Add(String.Format("OptionFound,{0},{1},{2}", args.Option.Code, args.OptionString, args.Value));
      }

      public void CommonArgFound(object sender, CommandLineCommonArgEventArgs args)
      {
        _Messages.Add(String.Format("CommonArgFound,{0}", args.Value));
      }
    }

    [Test]
    public void OptionFound_ValueTypeNone()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--aaa" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,aaa,--aaa,", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_AuxOptionCode()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "-a" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,aaa,-a,", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeSingle_withEq()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--bbb=123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,bbb,--bbb,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeSingle_withSpace()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--bbb", "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,bbb,--bbb,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeOptional_NoValue()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--ccc" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,ccc,--ccc,", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeOptional_withEq()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--ccc=123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,ccc,--ccc,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeOptional_withSpace()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--ccc", "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,ccc,--ccc,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeMulti_withEq()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--ddd=123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,ddd,--ddd,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void OptionFound_ValueTypeMulti_withSpace()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "--ddd", "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("OptionFound,ddd,--ddd,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void CommonArgFound()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = true;

      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "123" };
      bool res = sut.Parse(args, false);

      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, helper.Messages.Count, "MessageCount");
      Assert.AreEqual("CommonArgFound,123", helper.Messages[0], "Messages");
    }

    [Test]
    public void Events_mixed()
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = true;

      EventHelper helper = new EventHelper(sut);

      string[] args = new string[] { "-a", "aaa", "bbb", "--bbb=123", "--ccc" };
      bool res = sut.Parse(args, false);
      Assert.IsTrue(res, "Result");

      Assert.AreEqual(
        "OptionFound,aaa,-a,|" +
        "CommonArgFound,aaa|" +
        "CommonArgFound,bbb|" +
        "OptionFound,bbb,--bbb,123|" +
        "OptionFound,ccc,--ccc,",
        String.Join("|", helper.Messages.ToArray()), "Messages");
    }

    #endregion

    #region Dynamic Enabled changes

    [Test]
    public void CommonArgEnabledAfterOption([Values(false, true)]bool setEnabled)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = false;
      if (setEnabled)
        sut.OptionFound += CommonArgEnabledAfterOption_OptionFound;

      string[] args = new string[] { "--aaa", "xxx" };
      bool res = sut.Parse(args, false);
      Assert.AreEqual(setEnabled, res, "Result");
    }

    private static void CommonArgEnabledAfterOption_OptionFound(object sender, CommandLineOptionEventArgs args)
    {
      CommandLineParser sut = (CommandLineParser)sender;
      if (args.Option.Code == "aaa")
        sut.CommonArgsEnabled = true;
    }

    [Test]
    public void CommonArgDisabledAfterOption([Values(false, true)]bool setDisabled)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = true;
      if (setDisabled)
        sut.OptionFound += CommonArgDisabledAfterOption_OptionFound;

      string[] args = new string[] { "--aaa", "xxx" };
      bool res = sut.Parse(args, false);
      Assert.AreEqual(!setDisabled, res, "Result");
    }

    private static void CommonArgDisabledAfterOption_OptionFound(object sender, CommandLineOptionEventArgs args)
    {
      CommandLineParser sut = (CommandLineParser)sender;
      if (args.Option.Code == "aaa")
        sut.CommonArgsEnabled = false;
    }


    [Test]
    public void OptionEnabledAfterCommonArg([Values(false, true)]bool setEnabled)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = true;
      sut.OptionDefs["aaa"].Enabled = false;
      if (setEnabled)
        sut.CommonArgFound += OptionEnabledAfterCommonArg_CommonArgFound;

      string[] args = new string[] { "xxx", "--aaa" };
      bool res = sut.Parse(args, false);
      Assert.AreEqual(setEnabled, res, "Result");
    }

    private static void OptionEnabledAfterCommonArg_CommonArgFound(object sender, CommandLineCommonArgEventArgs args)
    {
      CommandLineParser sut = (CommandLineParser)sender;
      sut.OptionDefs["aaa"].Enabled = true;
    }


    [Test]
    public void OptionDisabledAfterCommonArg([Values(false, true)]bool setDisabled)
    {
      CommandLineParser sut = CreateTestParserWithOptions();
      sut.CommonArgsEnabled = true;
      if (setDisabled)
        sut.CommonArgFound += OptionDisabledAfterCommonArg_CommonArgFound;

      string[] args = new string[] { "xxx", "--aaa" };
      bool res = sut.Parse(args, false);
      Assert.AreEqual(!setDisabled, res, "Result");
    }

    private static void OptionDisabledAfterCommonArg_CommonArgFound(object sender, CommandLineCommonArgEventArgs args)
    {
      CommandLineParser sut = (CommandLineParser)sender;
      sut.OptionDefs["aaa"].Enabled = false;
    }

    #endregion
  }

  [TestFixture]
  public class CommandLineOptionTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      CommandLineOption sut = new CommandLineOption("--list,-l");
      CollectionAssert.AreEqual(new string[] { "--list", "-l" }, sut.Options, "Options");
      Assert.AreEqual("list", sut.Code, "Code");
      Assert.IsTrue(sut.Enabled, "Enabled");
      Assert.AreEqual(',', sut.MultiValueSeparator, "MultiValueSeparator");
      Assert.AreEqual(CommandLineOptionValueMode.None, sut.ValueMode, "ValueType");
    }


    [Test]
    public void Constructor_error()
    {
      CommandLineOption sut;
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption(""); }, "Empty string");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("aaa"); }, "No prefix");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("---aaa"); }, "Extra prefix");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("/aaa"); }, "Must be -");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("--aaa="); }, "Bad char");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("--aaa/"); }, "Bad char");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("--a a"); }, "Space");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("-- a"); }, "Space");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("--a "); }, "Space");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("- -a"); }, "Space");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("-"); }, "No code");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineOption("--"); }, "No code");
    }

    #endregion

    #region Свойства

    [Test]
    public void MultiValueSeparator()
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithOptions();
      CommandLineOption sut = parser.OptionDefs["ddd"];
      sut.MultiValueSeparator = '^';

      string[] args = new string[] { "--ddd=123", "--ddd", "456" };
      bool res = parser.Parse(args, false);

      Assert.IsTrue(res, "Result");

      Assert.AreEqual(1, parser.OptionValues.Count, "OptionValues.Count");
      Assert.AreEqual(0, parser.CommonValues.Count, "CommonValues.Count");

      Assert.AreEqual("123^456", parser.OptionValues["ddd"], "OptionValues");
    }

    [Test]
    public void Enabled([Values(true, false)]bool enabled)
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithOptions();
      CommandLineOption sut = parser.OptionDefs["aaa"];
      sut.Enabled = enabled;

      string[] args = new string[] { "--aaa" };
      bool res = parser.Parse(args, false);
      Assert.AreEqual(enabled, res, "Result");
    }

    [Test]
    public void EnabledErrorMessage()
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithOptions();
      CommandLineOption sut = parser.OptionDefs["aaa"];
      sut.Enabled = false;
      sut.EnabledErrorMessage = "XXX";

      string[] args = new string[] { "--aaa" };
      bool res = parser.Parse(args, false);
      Assert.IsFalse(res, "Result");
      StringAssert.Contains("XXX", parser.ErrorMessage, "ErrorMessage");
    }

    [TestCase("--aaa")]
    [TestCase("-a")]
    public void Options(string option)
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithOptions();
      string[] args = new string[] { option };
      bool res = parser.Parse(args, false);
      Assert.IsTrue(res, "Result");
      Assert.IsTrue(parser.OptionValues.ContainsKey("aaa"), "Contains");
    }

    #endregion

    #region Values

    [TestCase("X1", false, true)]
    [TestCase("X2", false, true)]
    [TestCase("X3", false, false)]
    [TestCase("x2", false, false)]
    [TestCase("x2", true, true)]
    public void Values(string value, bool ignoreCase, bool wantedRes)
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithOptions();
      parser.OptionDefs["bbb"].Values.Add("X1");
      parser.OptionDefs["bbb"].Values.Add("X2");
      parser.IgnoreCase = ignoreCase;

      string[] args = new string[] { "--bbb", value };

      bool res = parser.Parse(args, false);

      Assert.AreEqual(wantedRes, res);
    }

    #endregion
  }

  [TestFixture]
  public class CommandLineActionTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      CommandLineAction sut = new CommandLineAction("list,l");
      CollectionAssert.AreEqual(new string[] { "list", "l" }, sut.Actions, "Actions");
      Assert.AreEqual("list", sut.Code, "Code");
      Assert.IsTrue(sut.Enabled, "Enabled");
    }


    [Test]
    public void Constructor_error()
    {
      CommandLineAction sut;
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction(""); }, "Empty string");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction("-aaa"); }, "Prefix");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction("--aaa"); }, "Prefix");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction("/aaa"); }, "Prefix");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction("a a"); }, "Space");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction(" a"); }, "Space");
      Assert.Catch<ArgumentException>(delegate () { sut = new CommandLineAction("a "); }, "Space");
    }

    #endregion

    #region Свойства

    [Test]
    public void Enabled([Values(true, false)]bool enabled)
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithActions();
      CommandLineAction sut = parser.ActionDefs["add"];
      sut.Enabled = enabled;

      string[] args = new string[] { "add" };
      bool res = parser.Parse(args, false);
      Assert.AreEqual(enabled, res, "Result");
    }

    [Test]
    public void EnabledErrorMessage()
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithActions();
      CommandLineAction sut = parser.ActionDefs["add"];
      sut.Enabled = false;
      sut.EnabledErrorMessage = "XXX";

      string[] args = new string[] { "add" };
      bool res = parser.Parse(args, false);
      Assert.IsFalse(res, "Result");
      StringAssert.Contains("XXX", parser.ErrorMessage, "ErrorMessage");
    }

    [TestCase("add")]
    [TestCase("a")]
    public void Actions(string action)
    {
      CommandLineParser parser = CommandLineParserTests.CreateTestParserWithActions();
      string[] args = new string[] { action };
      bool res = parser.Parse(args, false);
      Assert.IsTrue(res, "Result");
      CollectionAssert.AreEqual(new string[] { "add" }, parser.ActionValues, "ActionValues");
    }

    #endregion
  }
}
