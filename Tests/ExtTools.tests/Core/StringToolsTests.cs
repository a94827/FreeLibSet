using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace ExtTools_tests.Core
{
  [TestFixture]
  class StringToolsTests
  {
    #region AddStrIfNotEmpty()

    [TestCase("ABC", "DEF", ",", "ABC,DEF")]
    [TestCase("ABC", "", ",", "ABC")]
    [TestCase("", "DEF", ",", "DEF")]
    [TestCase("", "", ",", "")]
    public void AddStrIfNotEmpty(string sBase, string addedStr, string separator, string wanted)
    {
      string resStr1 = sBase;
      StringTools.AddStrIfNotEmpty(ref resStr1, addedStr, separator);
      Assert.AreEqual(wanted, resStr1, "String overload");

      StringBuilder sb2 = new StringBuilder();
      sb2.Append(sBase);
      StringTools.AddStrIfNotEmpty(sb2, addedStr, separator);
      Assert.AreEqual(wanted, sb2.ToString(), "StringBuilder overload");
    }

    #endregion

    #region BytesToHex() / HexToBytes()

    [TestCase("0|200|255", false, "00c8ff")]
    [TestCase("0|200|255", true, "00C8FF")]
    public void BytesToHex(string sBytes, bool upperCase, string wanted)
    {
      string[] a1 = sBytes.Split('|');
      byte[] b = new byte[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        b[i] = (byte)StdConvert.ToInt32(a1[i]);

      string res1 = StringTools.BytesToHex(b, upperCase);
      Assert.AreEqual(wanted, res1, "String overload");

      StringBuilder sb = new StringBuilder();
      StringTools.BytesToHex(sb, b, upperCase);
      Assert.AreEqual(wanted, sb.ToString(), "StringBuilder overload");
    }

    [Test]
    public void BytesToHex_empty()
    {
      string s1 = StringTools.BytesToHex(new Byte[] { }, false);
      Assert.AreEqual("", s1);

      string s2 = StringTools.BytesToHex(null, false);
      Assert.AreEqual("", s2);
    }

    [TestCase("abCD00", Result = "171|205|0")]
    [TestCase("", Result = "")]
    [TestCase(null, Result = "")]
    public string HexToBytes(string s)
    {
      byte[] a1 = StringTools.HexToBytes(s);
      string[] a2 = new string[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = StdConvert.ToString(a1[i]);
      return String.Join("|", a2);
    }

    #endregion

    #region ChangeUpperLowerInvariant()

    [TestCase("ABC", Result = "abc")]
    [TestCase("abc", Result = "ABC")]
    [TestCase("AbCd", Result = "aBcD")]
    [TestCase("..ABC", Result = "..abc")]
    public string ChangeUpperLowerInvariant(string s)
    {
      return StringTools.ChangeUpperLowerInvariant(s);
    }

    #endregion

    #region New Line Separators

    [Test]
    public void LineSeparators()
    {
      Assert.AreEqual(1, StringTools.CRLFSeparators.Length, "CRLFSeparators.Length");
      Assert.AreEqual("\r\n", StringTools.CRLFSeparators[0], "CRLFSeparators");

      Assert.AreEqual(1, StringTools.NewLineSeparators.Length, "NewLineSeparators.Length");
      Assert.AreEqual(Environment.NewLine, StringTools.NewLineSeparators[0], "NewLineSeparators");

      Assert.GreaterOrEqual(StringTools.AllPossibleLineSeparators.Length, 2, "AllPossibleLineSeparators.Length");
      Assert.IsTrue(Array.IndexOf<string>(StringTools.AllPossibleLineSeparators, "\r\n") >= 0, "AllPossibleLineSeparators contains Windows");
      Assert.IsTrue(Array.IndexOf<string>(StringTools.AllPossibleLineSeparators, "\n") >= 0, "AllPossibleLineSeparators contains Linux");
      Assert.IsTrue(Array.IndexOf<string>(StringTools.AllPossibleLineSeparators, Environment.NewLine) >= 0, "AllPossibleLineSeparators contains Environment.NewLine");
    }

    #endregion

    #region GetNewLineSeparators()

    [TestCase("ABC\r\nDEF", Result = "\r\n")]
    [TestCase("ABC\r\n\r\nDEF", Result = "\r\n")]
    [TestCase("ABC\r\n", Result = "\r\n")]
    [TestCase("\r\nDEF", Result = "\r\n")]
    [TestCase("\r\n", Result = "\r\n")]
    [TestCase("ABC", Result = "")]
    [TestCase("", Result = "")]
    [TestCase(null, Result = "")]
    [TestCase("ABC\nDEF", Result = "\n")]
    [TestCase("ABC\n\nDEF", Result = "\n")]
    [TestCase("ABC\rDEF", Result = "\r")]
    [TestCase("ABC\r\rDEF", Result = "\r")]
    public string GetNewLineSeparators(string s)
    {
      return StringTools.GetNewLineSeparators(s);
    }

    #endregion

    #region GetCharCount()

    [TestCase("ABCABCA", 'A', Result = 3)]
    [TestCase("BCD", 'A', Result = 0)]
    [TestCase("", 'A', Result = 0)]
    [TestCase(null, 'A', Result = 0)]
    public int GetCharCount(string s, char searchChar)
    {
      return StringTools.GetCharCount(s, searchChar);
    }

    #endregion

    #region Last/IndexOfAny() / IndexOfAnyOther()

    [TestCase("ABCD", "DB", 1)]
    [TestCase("ABCD", "YZ", -1)]
    [TestCase("ABCD", "", -1)]
    [TestCase("ABCD", null, -1)]
    [TestCase("", "ABC", -1)]
    [TestCase(null, "ABC", -1)]
    [TestCase("ABCDEFGHIJ", "1234567890", -1)] // используется CharArrayIndexer
    [TestCase("ABCDEFGHIJ6", "1234567890", 10)] // используется CharArrayIndexer
    public void IndexOfAny(string str, string searchChars, int wanted)
    {
      int res1 = StringTools.IndexOfAny(str, searchChars);
      Assert.AreEqual(wanted, res1, "String overload");

      if (searchChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(searchChars);
        int res2 = StringTools.IndexOfAny(str, ix);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    [TestCase("ABCD", "DB", 0)]
    [TestCase("ABCD", "AC", 1)]
    [TestCase("ABCD", "", 0)]
    [TestCase("ABCD", null, 0)]
    [TestCase("", "ABC", -1)]
    [TestCase(null, "ABC", -1)]
    public void IndexOfAnyOther(string str, string searchChars, int wanted)
    {
      int res1 = StringTools.IndexOfAnyOther(str, searchChars);
      Assert.AreEqual(wanted, res1, "String overload");

      if (searchChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(searchChars);
        int res2 = StringTools.IndexOfAnyOther(str, ix);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    [TestCase("ABCD", "DB", 3)]
    [TestCase("ABCD", "YZ", -1)]
    [TestCase("ABCD", "", -1)]
    [TestCase("ABCD", null, -1)]
    [TestCase("", "ABC", -1)]
    [TestCase(null, "ABC", -1)]
    [TestCase("ABCDEFGHIJ", "1234567890", -1)] // используется CharArrayIndexer
    [TestCase("ABCDEFGHIJ6", "1234567890", 10)] // используется CharArrayIndexer
    public void LastIndexOfAny(string str, string searchChars, int wanted)
    {
      int res1 = StringTools.LastIndexOfAny(str, searchChars);
      Assert.AreEqual(wanted, res1, "String overload");

      if (searchChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(searchChars);
        int res2 = StringTools.LastIndexOfAny(str, ix);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    [TestCase("ABCD", "DB", 2)]
    [TestCase("ABCD", "AC", 3)]
    [TestCase("ABCD", "", 3)]
    [TestCase("ABCD", null, 3)]
    [TestCase("", "ABC", -1)]
    [TestCase(null, "ABC", -1)]
    public void LastIndexOfAnyOther(string str, string searchChars, int wanted)
    {
      int res1 = StringTools.LastIndexOfAnyOther(str, searchChars);
      Assert.AreEqual(wanted, res1, "String overload");

      if (searchChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(searchChars);
        int res2 = StringTools.LastIndexOfAnyOther(str, ix);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    #endregion

    #region IndexOfOccurence(), LastIndexOfOccurence()

    [TestCase("ABABABA", "ABA", 0, StringComparison.Ordinal, 0)]
    [TestCase("ABABABA", "ABA", 1, StringComparison.Ordinal, 4)]
    [TestCase("ABABABA", "ABA", 2, StringComparison.Ordinal, -1)]
    [TestCase("ABABABA", "aba", 1, StringComparison.Ordinal, -1)]
    [TestCase("ABABABA", "aba", 1, StringComparison.OrdinalIgnoreCase, 4)]
    [TestCase("ABABABA", "", 0, StringComparison.Ordinal, 0)]
    [TestCase("", "ABA", 0, StringComparison.Ordinal, -1)]
    [TestCase(null, "ABA", 0, StringComparison.Ordinal, -1)]
    // Специальный случай просмотра конца строки
    [TestCase("AAA", "A", 2, StringComparison.Ordinal, 2)]
    [TestCase("AAA", "A", 3, StringComparison.Ordinal, -1)]
    public void IndexOfOccurence_string(string str, string value, int occurence, StringComparison comparisonType, int wantedRes)
    {
      int res = StringTools.IndexOfOccurence(str, value, occurence, comparisonType);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("ABABABA", "ABA", 0, StringComparison.Ordinal, 4)]
    [TestCase("ABABABA", "ABA", 1, StringComparison.Ordinal, 0)]
    [TestCase("ABABABA", "ABA", 2, StringComparison.Ordinal, -1)]
    [TestCase("ABABABA", "aba", 1, StringComparison.Ordinal, -1)]
    [TestCase("ABABABA", "aba", 1, StringComparison.OrdinalIgnoreCase, 0)]
    [TestCase("ABABABA", "", 0, StringComparison.Ordinal, 0)]
    [TestCase("", "ABA", 0, StringComparison.Ordinal, -1)]
    [TestCase(null, "ABA", 0, StringComparison.Ordinal, -1)]
    // Специальный случай просмотра конца строки
    [TestCase("AAA", "A", 2, StringComparison.Ordinal, 0)]
    [TestCase("AAA", "A", 3, StringComparison.Ordinal, -1)]
    public void LastIndexOfOccurence_string(string str, string value, int occurence, StringComparison comparisonType, int wantedRes)
    {
      int res = StringTools.LastIndexOfOccurence(str, value, occurence, comparisonType);
      Assert.AreEqual(wantedRes, res);
    }


    [TestCase("ABABA", 'A', 0, 0)]
    [TestCase("ABABA", 'A', 1, 2)]
    [TestCase("ABABA", 'A', 2, 4)]
    [TestCase("ABABA", 'A', 3, -1)]
    [TestCase("ABABA", 'X', 0, -1)]
    [TestCase("", 'A', 0, -1)]
    [TestCase(null, 'A', 0, -1)]
    public void IndexOfOccurence_char(string str, char value, int occurence, int wantedRes)
    {
      int res = StringTools.IndexOfOccurence(str, value, occurence);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("ABABA", 'A', 0, 4)]
    [TestCase("ABABA", 'A', 1, 2)]
    [TestCase("ABABA", 'A', 2, 0)]
    [TestCase("ABABA", 'A', 3, -1)]
    [TestCase("ABABA", 'X', 0, -1)]
    [TestCase("", 'A', 0, -1)]
    [TestCase(null, 'A', 0, -1)]
    public void LastIndexOfOccurence_char(string str, char value, int occurence, int wantedRes)
    {
      int res = StringTools.LastIndexOfOccurence(str, value, occurence);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Substring()

    [TestCase("ABCDEF", 2, 3, "CDE")]
    [TestCase("ABCDEF", 2, 4, "CDEF")]
    [TestCase("ABCDEF", 2, 5, "CDEF")]
    [TestCase("ABCDEF", 0, 3, "ABC")]
    [TestCase("ABCDEF", -1, 3, "AB")]
    [TestCase("ABCDEF", -2, 3, "A")]
    [TestCase("ABCDEF", -3, 3, "")]
    [TestCase("ABCDEF", 6, 1, "")]
    [TestCase("ABCDEF", 2, 0, "")]
    [TestCase("ABCDEF", 2, -1, "")]
    [TestCase("", 0, 1, "")]
    [TestCase(null, 0, 1, "")]
    public void Substring(string s, int start, int length, string wantedRes)
    {
      string res = StringTools.Substring(s, start, length);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region IsSubstring()

    [TestCase("ABCDEF", 2, "cde", StringComparison.OrdinalIgnoreCase, Result = true)]
    [TestCase("ABCDEF", 2, "cde", StringComparison.Ordinal, Result = false)]
    [TestCase("ABCDEF", 4, "zzz", StringComparison.OrdinalIgnoreCase, Result = false, Description = "no exception throws")]
    [TestCase("ABCDEF", -1, "zzz", StringComparison.OrdinalIgnoreCase, Result = false, Description = "no exception throws")]
    [TestCase("", 2, "zzz", StringComparison.OrdinalIgnoreCase, Result = false)]
    [TestCase(null, 2, "zzz", StringComparison.OrdinalIgnoreCase, Result = false)]
    [TestCase("ABCDEF", 2, "", StringComparison.OrdinalIgnoreCase, Result = true)]
    [TestCase("ABCDEF", 2, null, StringComparison.OrdinalIgnoreCase, Result = true)]
    public bool IsSubstring(string s, int startPos, string substring, StringComparison comparisionType)
    {
      return StringTools.IsSubstring(s, startPos, substring, comparisionType);
    }

    #endregion

    #region JoinNotEmptyStrings()

    [TestCase(",", "ABC", "DEF", "GHI", Result = "ABC,DEF,GHI")]
    [TestCase(",", "", "DEF", "GHI", Result = "DEF,GHI")]
    [TestCase(",", null, "DEF", "GHI", Result = "DEF,GHI")]
    [TestCase(",", "ABC", "DEF", "", Result = "ABC,DEF")]
    [TestCase(",", "ABC", "", "GHI", Result = "ABC,GHI")]
    [TestCase(",", "ABC", "", "", "GHI", Result = "ABC,GHI")]
    [TestCase(null, "ABC", "DEF", "GHI", Result = "ABCDEFGHI")]
    [TestCase(",", "", Result = "")]
    [TestCase(",", Result = "")]
    public string JoinNotEmptyStrings(string separator, params string[] values)
    {
      // Надо тестировать все перегрузки

      string s1 = StringTools.JoinNotEmptyStrings(separator, values);

      List<string> lst2 = new List<string>();
      lst2.AddRange(values);
      string s2 = StringTools.JoinNotEmptyStrings(separator, lst2);
      string s3 = StringTools.JoinNotEmptyStrings(separator, (IEnumerable<string>)lst2);

      Assert.AreEqual(s1, s2, "Overloads with string[] and IList returns different results");
      Assert.AreEqual(s1, s3, "Overloads with string[] and IEnumerable returns different results");
      return s1;
    }

    #endregion

    #region PadXXX()

    [TestCase("AB", 5, "#AB##")]
    [TestCase("AB", 2, "AB")]
    [TestCase("AB", 1, "A")]
    [TestCase("", 1, "#")]
    [TestCase(null, 1, "#")]
    public void PadCenter(string s, int length, string wanted)
    {
      string res1 = StringTools.PadCenter(s, length, '#');
      Assert.AreEqual(wanted, res1, "overload with padding char");

      string res2 = StringTools.PadCenter(s, length);
      Assert.AreEqual(wanted.Replace('#', ' '), res2, "overload with 2 args");
    }

    [TestCase("AB", 5, "###AB")]
    [TestCase("AB", 2, "AB")]
    [TestCase("AB", 1, "B")]
    [TestCase("", 1, "#")]
    [TestCase(null, 1, "#")]
    public void PadLeft(string s, int length, string wanted)
    {
      string res1 = StringTools.PadLeft(s, length, '#');
      Assert.AreEqual(wanted, res1, "overload with padding char");

      string res2 = StringTools.PadLeft(s, length);
      Assert.AreEqual(wanted.Replace('#', ' '), res2, "overload with 2 args");
    }

    [TestCase("AB", 5, "AB###")]
    [TestCase("AB", 2, "AB")]
    [TestCase("AB", 1, "A")]
    [TestCase("", 1, "#")]
    public void PadRight(string s, int length, string wanted)
    {
      string res1 = StringTools.PadRight(s, length, '#');
      Assert.AreEqual(wanted, res1, "overload with padding char");

      string res2 = StringTools.PadRight(s, length);
      Assert.AreEqual(wanted.Replace('#', ' '), res2, "overload with 2 args");
    }

    #endregion

    #region RemoveChars() RemoveOtherChars()

    [TestCase("ABCDEF", "DCBZ", "AEF")]
    [TestCase("ABCDEF", "XYZ", "ABCDEF")]
    [TestCase("", "XYZ", "")]
    [TestCase(null, "XYZ", "")]
    [TestCase("ABCDEF", "", "ABCDEF")]
    [TestCase("ABCDEF", null, "ABCDEF")]
    public void RemoveChars(string str, string removedChars, string wanted)
    {
      string res1 = StringTools.RemoveChars(str, removedChars);
      Assert.AreEqual(wanted, res1, "string overload");

      if (removedChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(removedChars);
        string res2 = StringTools.RemoveChars(str, ix);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    [TestCase("ABCDEF", "DCBZ", "BCD")]
    [TestCase("ABCDEF", "XYZ", "")]
    [TestCase("", "XYZ", "")]
    [TestCase(null, "XYZ", "")]
    [TestCase("ABCDEF", "", "")]
    [TestCase("ABCDEF", null, "")]
    public void RemoveOtherChars(string str, string validChars, string wanted)
    {
      string res1 = StringTools.RemoveOtherChars(str, validChars);
      Assert.AreEqual(wanted, res1, "string overload");
      if (validChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(validChars);
        string res2 = StringTools.RemoveOtherChars(str, ix);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    #endregion

    #region RemoveDoubleChars()

    [TestCase("ABCCD", 'C', Result = "ABCD")]
    [TestCase("AABCAA", 'A', Result = "ABCA")]
    [TestCase("AAABC", 'A', Result = "ABC")]
    [TestCase("AAABC", 'Z', Result = "AAABC")]
    [TestCase("", 'Z', Result = "")]
    [TestCase(null, 'Z', Result = "")]
    public string RemoveDoubleChars(string str, char searchChar)
    {
      return StringTools.RemoveDoubleChars(str, searchChar);
    }

    #endregion

    #region ReplaceAny() / ReplaceAnyOther()

    [TestCase("ABCDEF", "DBZ", '0', "A0C0EF")]
    [TestCase("", "DB", '0', "")]
    [TestCase(null, "DB", '0', "")]
    [TestCase("ABCDEF", "", '0', "ABCDEF")]
    [TestCase("ABCDEF", null, '0', "ABCDEF")]
    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEVWXYZ", '0', "00000FGHIJKLMNOPQRSTU00000")] // используется индексатор
    public void ReplaceAny(string str, string searchChars, char replaceChar, string wanted)
    {
      string res1 = StringTools.ReplaceAny(str, searchChars, replaceChar);
      Assert.AreEqual(wanted, res1, "string overload");

      if (searchChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(searchChars);
        string res2 = StringTools.ReplaceAny(str, ix, replaceChar);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    [TestCase("ABCDEF", "DBZ", '0', "0B0D00")]
    [TestCase("", "DB", '0', "")]
    [TestCase(null, "DB", '0', "")]
    [TestCase("ABCDEF", "", '0', "000000")]
    [TestCase("ABCDEF", null, '0', "000000")]
    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEVWXYZ", '0', "ABCDE0000000000000000VWXYZ")] // используется индексатор
    public void ReplaceAnyOther(string str, string searchChars, char replaceChar, string wanted)
    {
      string res1 = StringTools.ReplaceAnyOther(str, searchChars, replaceChar);
      Assert.AreEqual(wanted, res1, "string overload");

      if (searchChars != null)
      {
        CharArrayIndexer ix = new CharArrayIndexer(searchChars);
        string res2 = StringTools.ReplaceAnyOther(str, ix, replaceChar);
        Assert.AreEqual(wanted, res2, "CharArrayIndexer overload");
      }
    }

    #endregion

    #region ReplaceChars()

    [TestCase("ABCDEF", "DFA", "123", "3BC1E2")]
    [TestCase("", "DF", "12", "")]
    [TestCase(null, "DF", "12", "")]
    [TestCase("ABCDEF", "", "", "ABCDEF")]
    public void ReplaceChars(string str, string searchChars, string replaceChars, string wanted)
    {
      string res1 = StringTools.ReplaceChars(str, searchChars, replaceChars);
      Assert.AreEqual(wanted, res1, "string overload");

      Dictionary<char, char> dict = new Dictionary<char, char>();
      for (int i = 0; i < searchChars.Length; i++)
        dict.Add(searchChars[i], replaceChars[i]);
      string res2 = StringTools.ReplaceChars(str, dict);
      Assert.AreEqual(wanted, res2, "Dictionary overload");
    }

    [Test]
    public void ReplaceChars_exception()
    {
      Assert.Catch<ArgumentException>(delegate() { StringTools.ReplaceChars("ABCDEF", "ABC", "12"); });
    }

    #endregion

    #region ReplaceCharRange()

    [TestCase("AHBCEF", 'B', 'C', '0', "AH00EF")]
    [TestCase("", 'B', 'C', '0', "")]
    [TestCase(null, 'B', 'C', '0', "")]
    [TestCase("AHBCEF", 'C', 'B', '0', "AHBCEF")]
    public void ReplaceCharRange(string str, char firstChar, char lastChar, char replaceChar, string wanted)
    {
      string res = StringTools.ReplaceCharRange(str, firstChar, lastChar, replaceChar);
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region ReplaceDigits()

    [TestCase("1AB", '0', Result = "0AB")]
    [TestCase("AB0", '0', Result = "AB0")]
    [TestCase("ABC", '0', Result = "ABC")]
    [TestCase("", '0', Result = "")]
    [TestCase(null, '0', Result = "")]
    public string ReplaceDigits(string str, char c)
    {
      string res = StringTools.ReplaceDigits(str, c);

      if (str != null)
        Assert.AreEqual(str.Length, res.Length);
      return res;
    }

    #endregion

    #region StrXXX()

    [TestCase("AB", 5, Result = "AB")]
    [TestCase("AB", 2, Result = "AB")]
    [TestCase("AB", 1, Result = "A")]
    [TestCase("", 1, Result = "")]
    [TestCase(null, 1, Result = "")]
    public string StrLeft(string s, int length)
    {
      return StringTools.StrLeft(s, length);
    }

    [TestCase("AB", 5, Result = "AB")]
    [TestCase("AB", 2, Result = "AB")]
    [TestCase("AB", 1, Result = "B")]
    [TestCase("", 1, Result = "")]
    [TestCase(null, 1, Result = "")]
    public string StrRight(string s, int length)
    {
      return StringTools.StrRight(s, length);
    }

    #endregion

    #region ToUpperInvariant() / ToLowerInvariant() / ToUpperFirstInvariant() / ToUpperWordsInvariant()

    [Test]
    public void ToUpperInvariant_for_array()
    {
      string[] a = new string[] { "abc", null, "DEF" };

      string[] res = StringTools.ToUpperInvariant(a);

      Assert.AreEqual(a.Length, res.Length);
      Assert.AreEqual(res[0], "ABC");
      Assert.IsNull(res[1]);
      Assert.AreEqual(res[2], "DEF");
    }

    [Test]
    public void ToUpperInvariant_for_array_null()
    {
      string[] a = null;

      string[] res = StringTools.ToUpperInvariant(a);

      Assert.IsNull(res);
    }


    [Test]
    public void ToLowerInvariant_for_array()
    {
      string[] a = new string[] { "abc", null, "DEF" };

      string[] res = StringTools.ToLowerInvariant(a);

      Assert.AreEqual(a.Length, res.Length);
      Assert.AreEqual(res[0], "abc");
      Assert.IsNull(res[1]);
      Assert.AreEqual(res[2], "def");
    }

    [Test]
    public void ToLowerInvariant_for_array_null()
    {
      string[] a = null;

      string[] res = StringTools.ToLowerInvariant(a);

      Assert.IsNull(res);
    }

    [TestCase("ABC", Result = "Abc")]
    [TestCase("abc", Result = "Abc")]
    [TestCase("..ABC", Result = "..abc")]
    [TestCase("", Result = "")]
    [TestCase(null, Result = "")]
    public string ToUpperFirstInvariant(string s)
    {
      return StringTools.ToUpperFirstInvariant(s);
    }

    [TestCase("ABC DEF", Result = "Abc Def")]
    [TestCase("_ABC DEF", Result = "_Abc Def")]
    [TestCase("abc def", Result = "Abc Def")]
    [TestCase("abc  def", Result = "Abc  Def")] // два разделителя подряд
    [TestCase("abc123def", Result = "Abc123Def")]
    [TestCase("", Result = "")]
    [TestCase(null, Result = "")]
    public string ToUpperWordsInvariant(string s)
    {
      return StringTools.ToUpperWordsInvariant(s);
    }

    #endregion

    #region ToStringArray()

    [TestCase("1|2|3", "1,2,3")]
    [TestCase("1", "1")]
    [TestCase("", "")]
    public void ToStringArray(string sValues, string wanted)
    {
      // Arrange
      Int32[] a2;
      if (sValues.Length > 0)
      {
        string[] a1 = sValues.Split('|');
        a2 = new Int32[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = StdConvert.ToInt32(a1[i]);
      }
      else
        a2 = new Int32[0];
      List<Int32> lst2 = new List<Int32>();
      lst2.AddRange(a2);

      // Act
      string s1 = String.Join(",", StringTools.ToStringArray<Int32>(a2));
      string s2 = String.Join(",", StringTools.ToStringArray<Int32>(lst2));
      string s3 = String.Join(",", StringTools.ToStringArray<Int32>((IEnumerable<Int32>)lst2));
      string s4 = String.Join(",", StringTools.ToStringArray((System.Collections.IList)lst2));
      string s5 = String.Join(",", StringTools.ToStringArray((System.Collections.IEnumerable)lst2));

      // Assert
      Assert.AreEqual(wanted, s1, "string[]");
      Assert.AreEqual(wanted, s2, "IList<Int32>");
      Assert.AreEqual(wanted, s3, "IEnumerable<Int32>");
      Assert.AreEqual(wanted, s4, "System.Collections.IList");
      Assert.AreEqual(wanted, s5, "System.Collections.IEnumerable");
    }

    #endregion

    #region ToStringJoin()

    [TestCase(",", "1|2|3", "1,2,3")]
    [TestCase(",", "1", "1")]
    [TestCase(",", "", "")]
    public void ToStringJoin(string separator, string sValues, string wanted)
    {
      // Arrange
      Int32[] a2;
      if (sValues.Length > 0)
      {
        string[] a1 = sValues.Split('|');
        a2 = new Int32[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = StdConvert.ToInt32(a1[i]);
      }
      else
        a2 = new Int32[0];
      List<Int32> lst2 = new List<Int32>();
      lst2.AddRange(a2);

      // Act
      string s1 = StringTools.ToStringJoin<Int32>(separator, a2);
      string s2 = StringTools.ToStringJoin<Int32>(separator, lst2);
      string s3 = StringTools.ToStringJoin<Int32>(separator, (IEnumerable<Int32>)lst2);
      string s4 = StringTools.ToStringJoin(separator, (System.Collections.IList)lst2);
      string s5 = StringTools.ToStringJoin(separator, (System.Collections.IEnumerable)lst2);

      // Assert
      Assert.AreEqual(wanted, s1, "string[] and IList<Int32>");
      Assert.AreEqual(wanted, s2, "IList<Int32>");
      Assert.AreEqual(wanted, s3, "IEnumerable<Int32>");
      Assert.AreEqual(wanted, s4, "System.Collections.IList");
      Assert.AreEqual(wanted, s5, "System.Collections.IEnumerable");
    }

    #endregion

    #region TrimStart/EndNewLineSeparators()

    [TestCase("\r\n\r\nABC\r\n\r\n", true, Result = "\r\n\r\nABC")]
    [TestCase("\r\n\r\nABC\r\n\r\n", false, Result = "\r\n\r\nABC\r\n")]
    [TestCase("\n\nABC\n\n", true, Result = "\n\nABC")]
    [TestCase("\n\n", true, Result = "")]
    [TestCase("\n\n", false, Result = "\n")]
    [TestCase("ABC", true, Result = "ABC")]
    [TestCase("", true, Result = "")]
    [TestCase(null, true, Result = "")]
    public string TrimEndNewLineSeparators_with_autodetection(string s, bool trimAll)
    {
      return StringTools.TrimEndNewLineSeparators(s, trimAll);
    }

    [TestCase("\r\n\r\nABC\r\n\r\n", true, Result = "\r\n\r\nABC\r\n\r")]
    [TestCase("\n\nABC\n\n", true, Result = "\n\nABC")]
    [TestCase("\n\n", true, Result = "")]
    [TestCase("\n\n", false, Result = "\n")]
    [TestCase("ABC", true, Result = "ABC")]
    [TestCase("", true, Result = "")]
    [TestCase(null, true, Result = "")]
    public string TrimEndNewLineSeparators_LF(string s, bool trimAll)
    {
      return StringTools.TrimEndNewLineSeparators(s, trimAll, "\n");
    }

    [TestCase("\r\n\r\nABC\r\n\r\n", true, Result = "ABC\r\n\r\n")]
    [TestCase("\r\n\r\nABC\r\n\r\n", false, Result = "\r\nABC\r\n\r\n")]
    [TestCase("\n\nABC\n\n", true, Result = "ABC\n\n")]
    [TestCase("\n\n", true, Result = "")]
    [TestCase("\n\n", false, Result = "\n")]
    [TestCase("ABC", true, Result = "ABC")]
    [TestCase("", true, Result = "")]
    [TestCase(null, true, Result = "")]
    public string TrimStartNewLineSeparators_with_autodetection(string s, bool trimAll)
    {
      return StringTools.TrimStartNewLineSeparators(s, trimAll);
    }

    [TestCase("\r\n\r\nABC\r\n\r\n", true, Result = "\r\n\r\nABC\r\n\r\n")]
    [TestCase("\n\nABC\n\n", true, Result = "ABC\n\n")]
    [TestCase("\n\n", true, Result = "")]
    [TestCase("\n\n", false, Result = "\n")]
    [TestCase("ABC", true, Result = "ABC")]
    [TestCase("", true, Result = "")]
    [TestCase(null, true, Result = "")]
    public string TrimStartNewLineSeparators_LF(string s, bool trimAll)
    {
      return StringTools.TrimStartNewLineSeparators(s, trimAll, "\n");
    }

    #endregion

    #region IndexOf()

    [TestCase("AAAAA", StringComparison.Ordinal, -1)]
    [TestCase("AAAAA", StringComparison.OrdinalIgnoreCase, 0)]
    [TestCase("", StringComparison.Ordinal, -1)]
    public void IndexOf(string searchStr, StringComparison comparisonType, int wanted)
    {
      string[] a = new string[] { "AaAaA", "BbBbB" };

      int res = StringTools.IndexOf(a, searchStr, comparisonType);
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region StrToCSharpString()

    [TestCase("Hello\r\n\"world\"", "\"Hello\\r\\n\\\"world\\\"\"")]
    [TestCase("", "\"\"")]
    public void StrToCSharpString(string s, string wanted)
    {
      Assert.AreEqual(wanted, StringTools.StrToCSharpString(s));
    }

    #endregion

    #region RemoveEmptyRows() / RemoveEmptyColumns()

    [Test]
    public void RemoveEmptyRows()
    {
      string[,] a = RemoveEmptyRowsAndColumns_CreateTestArray();
      string[,] res = StringTools.RemoveEmptyRows(a);
      Assert.AreEqual(new string[2, 3] { { "AAA", "BBB", "" }, { "CCC", "", null } }, res);
    }

    [Test]
    public void RemoveEmptyColumns()
    {
      string[,] a = RemoveEmptyRowsAndColumns_CreateTestArray();
      string[,] res = StringTools.RemoveEmptyColumns(a);
      Assert.AreEqual(new string[3, 2] { { "AAA", "BBB"}, { "CCC", ""}, {"", null} }, res);
    }

    [Test]
    public void RemoveEmptyRowsAndColumns()
    {
      string[,] a = RemoveEmptyRowsAndColumns_CreateTestArray();
      string[,] res = StringTools.RemoveEmptyRowsAndColumns(a);
      Assert.AreEqual(new string[2, 2] { { "AAA", "BBB"}, { "CCC", ""} }, res);
    }

    private static string[,] RemoveEmptyRowsAndColumns_CreateTestArray()
    {
      return new string[3, 3] { { "AAA", "BBB", "" }, { "CCC", "", null }, { "", null, "" } };
    }

    #endregion

    #region RemoveSoftHyphens()

    [Test]
    public void RemoveSoftHyphens()
    {
      string s = "event-dri^ven ap^p^li^ca^ti^on".Replace('^', '\u00AD');

      string res = StringTools.RemoveSoftHyphens(s);
      Assert.AreEqual("event-driven application", res);
    }

    #endregion

    #region StrTo/FromSpecCharsStr/Array()

    [Test]
    public void StrFromSpecCharsStr()
    {
      string s = "A" + '\u00AD' + "B" + '\u00A0'+"C";
      string wanted="A^B_C";
      Assert.AreEqual(wanted, StringTools.StrFromSpecCharsStr(s));
    }

    [Test]
    public void StrToSpecCharsStr()
    {
      string s = "A^B_C";
      string wanted = "A" + '\u00AD' + "B" + '\u00A0' + "C";
      Assert.AreEqual(wanted, StringTools.StrToSpecCharsStr(s));
    }


    [Test]
    public void StrFromSpecCharsArray()
    {
      string[] a = { "A" + '\u00AD' + "B" + '\u00A0' + "C", "D" + '\u00AD' +"E"};
      string wanted="A^B_C|D^E";
      Assert.AreEqual(wanted, StringTools.StrFromSpecCharsArray(a));
    }

    [Test]
    public void StrToSpecCharsArray()
    {
      string s = "A^B_C|D^E";
      string[] wanted = { "A" + '\u00AD' + "B" + '\u00A0' + "C", "D" + '\u00AD' + "E" };
      Assert.AreEqual(wanted, StringTools.StrToSpecCharsArray(s));
    }

    #endregion

    #region Разбиение на две части

    [TestCase("ABC-DEF", '-', 0, true, "ABC", "DEF")]
    [TestCase("ABC-DEF", '-', 1, false, "ABC-DEF", "")]
    [TestCase("ABC-DEF-", '-', 0, true, "ABC", "DEF-")]
    [TestCase("ABC-DEF-", '-', 1, true, "ABC-DEF", "")]
    [TestCase("ABC-DEF-", '-', 2, false, "ABC-DEF-", "")]
    [TestCase("-ABC-DEF", '-', 0, true, "", "ABC-DEF")]
    [TestCase("-ABC-DEF", '-', 1, true, "-ABC", "DEF")]
    [TestCase("-ABC-DEF", '-', 2, false, "-ABC-DEF", "")]
    [TestCase("-", '-', 0, true, "", "")]
    [TestCase("-", '-', 1, false, "-", "")]
    [TestCase("ABC-DEF", '*', 0, false, "ABC-DEF", "")]
    [TestCase("**", '*', 0, true, "", "*")]
    [TestCase("**", '*', 1, true, "*", "")]
    [TestCase("**", '*', 2, false, "**", "")]
    [TestCase("", '-', 0, false, "", "")]
    [TestCase(null, '-', 0, false, "", "")]
    public void SplitBySeparator(string s, char separator, int occurence, bool wantedRes, string wantedMainPart, string wantedAuxPart)
    {
      string mainPart, auxPart;
      bool res = StringTools.SplitBySeparator(s, separator, occurence, out mainPart, out auxPart);

      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedMainPart, mainPart, "mainPart");
      Assert.AreEqual(wantedAuxPart, auxPart, "auxPart");
    }

#if XXX
    [TestCase("ABC-DEF", '-', true, "ABC", "DEF")]
    [TestCase("ABC-DEF-", '-', true, "ABC-DEF", "")]
    [TestCase("-ABC-DEF", '-', true, "-ABC", "DEF")]
    [TestCase("ABC-DEF-GHI", '-', true, "ABC-DEF", "GHI")]
    [TestCase("-", '-', true, "", "")]
    [TestCase("ABC-DEF", '*', false, "ABC-DEF", "")]
    [TestCase("", '-', false, "", "")]
    [TestCase(null, '-', false, "", "")]
    public void SplitLastSeparator(string s, char separator, bool wantedRes, string wantedMainPart, string wantedAuxPart)
    {
      string mainPart, auxPart;
      bool res = StringTools.SplitByLastSeparator(s, separator, out mainPart, out auxPart);

      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedMainPart, mainPart, "mainPart");
      Assert.AreEqual(wantedAuxPart, auxPart, "auxPart");
    }
#endif

#endregion
  }
}
