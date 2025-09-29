using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class ErrorMessageListTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      ErrorMessageList sut = new ErrorMessageList();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(0, sut.ErrorCount, "ErrorCount");
      Assert.AreEqual(0, sut.WarningCount, "WarningCount");
      Assert.AreEqual(0, sut.InfoCount, "InfoCount");

      Assert.AreEqual(0, sut.AllLines.Length, "AllLines");
      Assert.AreEqual(String.Empty, sut.AllText, "AllText");

      Assert.AreEqual(ErrorMessageKind.Info, sut.Severity, "Severity");
      Assert.IsNull(sut.NullableSeverity, "NullableSeverity");
      Assert.IsNull(sut.FirstSevereItem, "FirstSevereItem");

      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Свойства

    [Test]
    public void Empty()
    {
      ErrorMessageList sut = ErrorMessageList.Empty;
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.Catch(delegate() { sut.AddError("Test"); }, "AddError()");
    }

    [Test]
    public void AllLines_AllText()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");

      string[] wanted1 = new string[] { "Text1", "Text2", "Text3" };
      string wanted2 = "Text1" + Environment.NewLine + "Text2" + Environment.NewLine + "Text3";

      Assert.AreEqual(wanted1, sut.AllLines, "AllLines");
      Assert.AreEqual(wanted2, sut.AllText, "AllText");
    }

    #endregion

    #region Add()

    [Test]
    public void AddError()
    {
      ErrorMessageList sut = new ErrorMessageList();

      sut.AddError("Text1");
      Assert.AreEqual(1, sut.Count, "Count 1");
      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind 1");
      Assert.AreEqual("Text1", sut[0].Text, "Text 1");

      sut.AddError("Text2", "C2");
      Assert.AreEqual(2, sut.Count, "Count 2");
      Assert.AreEqual(ErrorMessageKind.Error, sut[1].Kind, "Kind 2");
      Assert.AreEqual("Text2", sut[1].Text, "Text 2");
      Assert.AreEqual("C2", sut[1].Code, "Code 2");

      sut.AddError("Text3", "C3", "Tag3");
      Assert.AreEqual(3, sut.Count, "Count 3");
      Assert.AreEqual(ErrorMessageKind.Error, sut[2].Kind, "Kind 3");
      Assert.AreEqual("Text3", sut[2].Text, "Text 3");
      Assert.AreEqual("C3", sut[2].Code, "Code 3");
      Assert.AreEqual("Tag3", sut[2].Tag, "Tag 3");

      ErrorMessageItem item4 = new ErrorMessageItem(ErrorMessageKind.Info, "Text4", "C4", "Tag4");
      sut.AddError(item4);
      Assert.AreEqual(4, sut.Count, "Count 4");
      Assert.AreEqual(ErrorMessageKind.Error, sut[3].Kind, "Kind 4");
      Assert.AreEqual("Text4", sut[3].Text, "Text 4");
      Assert.AreEqual("C4", sut[3].Code, "Code 4");
      Assert.AreEqual("Tag4", sut[3].Tag, "Tag 4");
    }

    [Test]
    public void AddWarning()
    {
      ErrorMessageList sut = new ErrorMessageList();

      sut.AddWarning("Text1");
      Assert.AreEqual(1, sut.Count, "Count 1");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[0].Kind, "Kind 1");
      Assert.AreEqual("Text1", sut[0].Text, "Text 1");

      sut.AddWarning("Text2", "C2");
      Assert.AreEqual(2, sut.Count, "Count 2");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[1].Kind, "Kind 2");
      Assert.AreEqual("Text2", sut[1].Text, "Text 2");
      Assert.AreEqual("C2", sut[1].Code, "Code 2");

      sut.AddWarning("Text3", "C3", "Tag3");
      Assert.AreEqual(3, sut.Count, "Count 3");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[2].Kind, "Kind 3");
      Assert.AreEqual("Text3", sut[2].Text, "Text 3");
      Assert.AreEqual("C3", sut[2].Code, "Code 3");
      Assert.AreEqual("Tag3", sut[2].Tag, "Tag 3");

      ErrorMessageItem item4 = new ErrorMessageItem(ErrorMessageKind.Info, "Text4", "C4", "Tag4");
      sut.AddWarning(item4);
      Assert.AreEqual(4, sut.Count, "Count 4");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[3].Kind, "Kind 4");
      Assert.AreEqual("Text4", sut[3].Text, "Text 4");
      Assert.AreEqual("C4", sut[3].Code, "Code 4");
      Assert.AreEqual("Tag4", sut[3].Tag, "Tag 4");
    }

    [Test]
    public void AddInfo()
    {
      ErrorMessageList sut = new ErrorMessageList();

      sut.AddInfo("Text1");
      Assert.AreEqual(1, sut.Count, "Count 1");
      Assert.AreEqual(ErrorMessageKind.Info, sut[0].Kind, "Kind 1");
      Assert.AreEqual("Text1", sut[0].Text, "Text 1");

      sut.AddInfo("Text2", "C2");
      Assert.AreEqual(2, sut.Count, "Count 2");
      Assert.AreEqual(ErrorMessageKind.Info, sut[1].Kind, "Kind 2");
      Assert.AreEqual("Text2", sut[1].Text, "Text 2");
      Assert.AreEqual("C2", sut[1].Code, "Code 2");

      sut.AddInfo("Text3", "C3", "Tag3");
      Assert.AreEqual(3, sut.Count, "Count 3");
      Assert.AreEqual(ErrorMessageKind.Info, sut[2].Kind, "Kind 3");
      Assert.AreEqual("Text3", sut[2].Text, "Text 3");
      Assert.AreEqual("C3", sut[2].Code, "Code 3");
      Assert.AreEqual("Tag3", sut[2].Tag, "Tag 3");

      ErrorMessageItem item4 = new ErrorMessageItem(ErrorMessageKind.Error, "Text4", "C4", "Tag4");
      sut.AddInfo(item4);
      Assert.AreEqual(4, sut.Count, "Count 4");
      Assert.AreEqual(ErrorMessageKind.Info, sut[3].Kind, "Kind 4");
      Assert.AreEqual("Text4", sut[3].Text, "Text 4");
      Assert.AreEqual("C4", sut[3].Code, "Code 4");
      Assert.AreEqual("Tag4", sut[3].Tag, "Tag 4");
    }

    [Test]
    public void Add_item_with_suffix_and_prefix()
    {
      ErrorMessageList sut = new ErrorMessageList();

      ErrorMessageItem item = new ErrorMessageItem(ErrorMessageKind.Warning, "Text1", "Code1", "Tag1");
      sut.Add(item, "P1", "S1");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[0].Kind, "Kind");
      Assert.AreEqual("P1Text1S1", sut[0].Text, "Text");
      Assert.AreEqual("Code1", sut[0].Code, "Code");
      Assert.AreEqual("Tag1", sut[0].Tag, "Tag");
    }

    [Test]
    public void AddRange_list_with_suffix_and_prefix()
    {
      ErrorMessageList sut = new ErrorMessageList();

      ErrorMessageList list = new ErrorMessageList();
      list.AddError("Text1", "C1", "Tag1");
      list.AddWarning("Text2", "C2", "Tag2");
      list.AddInfo("Text3", "C3", "Tag3");
      list.SetReadOnly();

      sut.AddRange(list, "P1", "S1");

      Assert.AreEqual(3, sut.Count, "Count");

      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind 1");
      Assert.AreEqual("P1Text1S1", sut[0].Text, "Text ");
      Assert.AreEqual("C1", sut[0].Code, "Code 1");
      Assert.AreEqual("Tag1", sut[0].Tag, "Tag 1");

      Assert.AreEqual(ErrorMessageKind.Warning, sut[1].Kind, "Kind 2");
      Assert.AreEqual("P1Text2S1", sut[1].Text, "Text 2");
      Assert.AreEqual("C2", sut[1].Code, "Code 2");
      Assert.AreEqual("Tag2", sut[1].Tag, "Tag 2");

      Assert.AreEqual(ErrorMessageKind.Info, sut[2].Kind, "Kind 3");
      Assert.AreEqual("P1Text3S1", sut[2].Text, "Text 3");
      Assert.AreEqual("C3", sut[2].Code, "Code 3");
      Assert.AreEqual("Tag3", sut[2].Tag, "Tag 3");
    }

    [Test]
    public void Add_exception_simple()
    {
      InvalidOperationException ex = new InvalidOperationException("Text1");
      ErrorMessageList sut = new ErrorMessageList();
      sut.Add(ex);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("Text1", sut[0].Text, "Text");
      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind");
    }

    [Test]
    public void Add_exception_ErrorMessageListException()
    {
      ErrorMessageList list = new ErrorMessageList();
      list.AddError("Text1");
      list.AddWarning("Text2");
      ErrorMessageListException ex = new ErrorMessageListException(list, "Text3");
      ErrorMessageList sut = new ErrorMessageList();
      sut.Add(ex);

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual("Text1", sut[0].Text, "Text 1");
      Assert.AreEqual("Text2", sut[1].Text, "Text 2");
      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind 1");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[1].Kind, "Kind 2");
    }

    [Test]
    public void Add_exception_with_prefix_and_suffix_simple()
    {
      InvalidOperationException ex = new InvalidOperationException("Text1");
      ErrorMessageList sut = new ErrorMessageList();
      sut.Add(ex, "P1", "S1");

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("P1Text1S1", sut[0].Text, "Text");
      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind");
    }

    [Test]
    public void Add_exception_with_prefix_and_suffix_ErrorMessageListException()
    {
      ErrorMessageList list = new ErrorMessageList();
      list.AddError("Text1");
      list.AddWarning("Text2");
      ErrorMessageListException ex = new ErrorMessageListException(list, "Text3");
      ErrorMessageList sut = new ErrorMessageList();
      sut.Add(ex, "P1", "S1");

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual("P1Text1S1", sut[0].Text, "Text 1");
      Assert.AreEqual("P1Text2S1", sut[1].Text, "Text 2");
      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind 1");
      Assert.AreEqual(ErrorMessageKind.Warning, sut[1].Kind, "Kind 2");
    }

    #endregion

    #region Set()

    [Test]
    public void SetTag()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");

      sut.SetTag("Tag4", 1);
      Assert.AreEqual("Tag1", sut[0].Tag, "Tag 1 #1");
      Assert.AreEqual("Tag4", sut[1].Tag, "Tag 2 #1");
      Assert.AreEqual("Tag4", sut[2].Tag, "Tag 3 #1");

      sut.SetTag("Tag5");
      Assert.AreEqual("Tag5", sut[0].Tag, "Tag 1 #2");
      Assert.AreEqual("Tag5", sut[1].Tag, "Tag 2 #2");
      Assert.AreEqual("Tag5", sut[2].Tag, "Tag 3 #2");
    }

    [Test]
    public void SetCode()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");

      sut.SetCode("C4", 1);
      Assert.AreEqual("C1", sut[0].Code, "Code 1 #1");
      Assert.AreEqual("C4", sut[1].Code, "Code 2 #1");
      Assert.AreEqual("C4", sut[2].Code, "Code 3 #1");

      sut.SetCode("C5");
      Assert.AreEqual("C5", sut[0].Code, "Code 1 #2");
      Assert.AreEqual("C5", sut[1].Code, "Code 2 #2");
      Assert.AreEqual("C5", sut[2].Code, "Code 3 #2");
    }

    [Test]
    public void SetPrefix()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");

      sut.SetPrefix("P4", 1);
      Assert.AreEqual("Text1", sut[0].Text, "Text 1 #1");
      Assert.AreEqual("P4Text2", sut[1].Text, "Text 2 #1");
      Assert.AreEqual("P4Text3", sut[2].Text, "Text 3 #1");

      sut.SetPrefix("P5");
      Assert.AreEqual("P5Text1", sut[0].Text, "Text 1 #2");
      Assert.AreEqual("P5P4Text2", sut[1].Text, "Text 2 #2");
      Assert.AreEqual("P5P4Text3", sut[2].Text, "Text 3 #2");
    }

    [Test]
    public void SetSuffix()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");

      sut.SetSuffix("S4", 1);
      Assert.AreEqual("Text1", sut[0].Text, "Text 1 #1");
      Assert.AreEqual("Text2S4", sut[1].Text, "Text 2 #1");
      Assert.AreEqual("Text3S4", sut[2].Text, "Text 3 #1");

      sut.SetSuffix("S5");
      Assert.AreEqual("Text1S5", sut[0].Text, "Text 1 #2");
      Assert.AreEqual("Text2S4S5", sut[1].Text, "Text 2 #2");
      Assert.AreEqual("Text3S4S5", sut[2].Text, "Text 3 #2");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone_simple()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");
      sut.SetReadOnly();

      ErrorMessageList res = sut.Clone();

      Assert.AreEqual(3, res.Count, "Count");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");

      Assert.AreEqual(ErrorMessageKind.Error, res[0].Kind, "Kind 1");
      Assert.AreEqual("Text1", res[0].Text, "Text 1");
      Assert.AreEqual("C1", res[0].Code, "Code 1");
      Assert.AreEqual("Tag1", res[0].Tag, "Tag 1");

      Assert.AreEqual(ErrorMessageKind.Warning, res[1].Kind, "Kind 2");
      Assert.AreEqual("Text2", res[1].Text, "Text 2");
      Assert.AreEqual("C2", res[1].Code, "Code 2");
      Assert.AreEqual("Tag2", res[1].Tag, "Tag 2");

      Assert.AreEqual(ErrorMessageKind.Info, res[2].Kind, "Kind 3");
      Assert.AreEqual("Text3", res[2].Text, "Text 3");
      Assert.AreEqual("C3", res[2].Code, "Code 3");
      Assert.AreEqual("Tag3", res[2].Tag, "Tag 3");
    }
    [Test]
    public void Clone_filter_by_kind()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");
      sut.SetReadOnly();

      ErrorMessageList res = sut.Clone(ErrorMessageKind.Warning);

      Assert.AreEqual(1, res.Count, "Count");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");

      Assert.AreEqual(ErrorMessageKind.Warning, res[0].Kind, "Kind 2");
      Assert.AreEqual("Text2", res[0].Text, "Text 2");
      Assert.AreEqual("C2", res[0].Code, "Code 2");
      Assert.AreEqual("Tag2", res[0].Tag, "Tag 2");
    }

    [Test]
    public void CloneIfReadOnly()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");

      ErrorMessageList res1 = sut.CloneIfReadOnly();
      Assert.AreSame(sut, res1, "Same list");

      sut.SetReadOnly();
      ErrorMessageList res2 = sut.CloneIfReadOnly();
      Assert.AreNotSame(sut, res2, "Cloned list");

      Assert.AreEqual(1, res2.Count, "Count 2");
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly 2");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void SetReadOnly()
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly before");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly after SetReadOnly()");
      Assert.Catch(delegate() { sut.AddError("Test 2"); }, "AddError()");
      Assert.AreEqual(1, sut.Count, "Count not changed");
      Assert.Catch(delegate() { sut.SetCode("Code"); }, "SetCode()");
      Assert.Catch(delegate() { sut.SetTag("Tag"); }, "SetTag()");
      Assert.Catch(delegate() { sut.SetPrefix("P"); }, "SetPrefix()");
      Assert.Catch(delegate() { sut.SetSuffix("S"); }, "SetSuffix()");
      Assert.Catch(delegate () { sut.SetMaxSeverity(ErrorMessageKind.Warning); }, "SetMaxSeverity()");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly after SetReadOnly() again");

      // Проверяем, что ничего не испортилось
      Assert.AreEqual(1, sut.Count, "Count");

      Assert.AreEqual(ErrorMessageKind.Error, sut[0].Kind, "Kind 1");
      Assert.AreEqual("Text1", sut[0].Text, "Text 1");
      Assert.AreEqual("C1", sut[0].Code, "Code 1");
      Assert.AreEqual("Tag1", sut[0].Tag, "Tag 1");
    }

    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Error, ErrorMessageKind.Warning, ErrorMessageKind.Info)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Warning, ErrorMessageKind.Warning, ErrorMessageKind.Info)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Info, ErrorMessageKind.Info, ErrorMessageKind.Info)]
    public void SetMaxSeverity(ErrorMessageKind maxKind, ErrorMessageKind wanted1, ErrorMessageKind wanted2, ErrorMessageKind wanted3)
    {
      ErrorMessageList sut = new ErrorMessageList();
      sut.AddError("Text1", "C1", "Tag1");
      sut.AddWarning("Text2", "C2", "Tag2");
      sut.AddInfo("Text3", "C3", "Tag3");
      string s1 = sut.AllText;

      sut.SetMaxSeverity(maxKind);

      Assert.AreEqual(s1, sut.AllText, "Original is not changed");

      Assert.AreEqual(3, sut.Count, "Count");

      Assert.AreEqual(wanted1, sut[0].Kind, "Kind 1");
      Assert.AreEqual("Text1", sut[0].Text, "Text 1");
      Assert.AreEqual("C1", sut[0].Code, "Code 1");
      Assert.AreEqual("Tag1", sut[0].Tag, "Tag 1");

      Assert.AreEqual(wanted2, sut[1].Kind, "Kind 2");
      Assert.AreEqual("Text2", sut[1].Text, "Text 2");
      Assert.AreEqual("C2", sut[1].Code, "Code 2");
      Assert.AreEqual("Tag2", sut[1].Tag, "Tag 2");

      Assert.AreEqual(wanted3, sut[2].Kind, "Kind 3");
      Assert.AreEqual("Text3", sut[2].Text, "Text 3");
      Assert.AreEqual("C3", sut[2].Code, "Code 3");
      Assert.AreEqual("Tag3", sut[2].Tag, "Tag 3");
    }

    #endregion

    #region Выбрасывание исключений

    [Test]
    public void ThrowIfErrors()
    {
      ErrorMessageList sut1 = new ErrorMessageList();
      sut1.AddError("Text1");
      Assert.Throws<ErrorMessageListException>(delegate() { sut1.ThrowIfErrors(); });

      ErrorMessageList sut2 = new ErrorMessageList();
      sut2.AddWarning("Text2");
      Assert.DoesNotThrow(delegate() { sut2.ThrowIfErrors(); });

      ErrorMessageList sut3 = new ErrorMessageList();
      Assert.DoesNotThrow(delegate() { sut3.ThrowIfErrors(); });
    }

    #endregion

    #region Статические методы

    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Error, ErrorMessageKind.Error)]
    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Warning, ErrorMessageKind.Error)]
    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Info, ErrorMessageKind.Error)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Error, ErrorMessageKind.Error)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Warning, ErrorMessageKind.Warning)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Info, ErrorMessageKind.Warning)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Error, ErrorMessageKind.Error)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Warning, ErrorMessageKind.Warning)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Info, ErrorMessageKind.Info)]
    public void MaxSeverity(ErrorMessageKind kind1, ErrorMessageKind kind2, ErrorMessageKind wanted)
    {
      Assert.AreEqual(wanted, ErrorMessageList.MaxSeverity(kind1, kind2), "Kind-Kind");

      ErrorMessageList list1 = new ErrorMessageList();
      list1.Add(new ErrorMessageItem(kind1, "Text1"));
      ErrorMessageList list2 = new ErrorMessageList();
      list2.Add(new ErrorMessageItem(kind2, "Text2"));

      Assert.AreEqual(wanted, ErrorMessageList.MaxSeverity(list1, list2), "List-List");

      Assert.AreEqual(wanted, ErrorMessageList.MaxSeverity(kind1, list2), "Kind-List");
    }

    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Error, 0)]
    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Warning, +1)]
    [TestCase(ErrorMessageKind.Error, ErrorMessageKind.Info, +1)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Error, -1)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Warning, 0)]
    [TestCase(ErrorMessageKind.Warning, ErrorMessageKind.Info, 1)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Error, -1)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Warning, -1)]
    [TestCase(ErrorMessageKind.Info, ErrorMessageKind.Info, 0)]
    public void Compare(ErrorMessageKind kind1, ErrorMessageKind kind2, int wanted)
    {
      int res = ErrorMessageList.Compare(kind1, kind2);
      Assert.AreEqual(wanted, Math.Sign(res));
    }

    #endregion
  }
}
