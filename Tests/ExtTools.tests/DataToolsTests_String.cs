using AgeyevAV;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_String
  {
    [TestCase("ABC", "DEF", ",", Result = "ABC,DEF")]
    [TestCase("ABC", "", ",", Result = "ABC")]
    [TestCase("", "DEF", ",", Result = "DEF")]
    [TestCase("", "", ",", Result = "")]
    public string AddStrIfNoEmpty(string resStr, string addedStr, string separator)
    {
      DataTools.AddStrIfNoEmpty(ref resStr, addedStr, separator);
      return resStr;
    }

    [TestCase("0|200|255", false, Result = "00c8ff")]
    [TestCase("0|200|255", true, Result = "00C8FF")]
    public string BytesToHex(string sBytes, bool upperCase)
    {
      string[] a1 = sBytes.Split('|');
      byte[] b = new byte[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        b[i] = (byte)StdConvert.ToInt32(a1[i]);

      return DataTools.BytesToHex(b, upperCase);
    }

    [Test]
    public void BytesToHex_empty()
    {
      string s1 = DataTools.BytesToHex(new Byte[] { }, false);
      Assert.AreEqual("", s1);

      string s2 = DataTools.BytesToHex(null, false);
      Assert.AreEqual("", s2);
    }

    [TestCase("ABC", Result = "abc")]
    [TestCase("abc", Result = "ABC")]
    [TestCase("AbCd", Result = "aBcD")]
    [TestCase("..ABC", Result = "..abc")]
    public string ChangeUpperLowerInvariant(string s)
    {
      return DataTools.ChangeUpperLowerInvariant(s);
    }

    [TestCase("ABC,DEF", Result = new string[] { "ABC", "DEF" })]
    [TestCase("ABC,DEF,", Result = new string[] { "ABC", "DEF", "" })]
    [TestCase("ABC,DEF,,", Result = new string[] { "ABC", "DEF", "", "" })]
    [TestCase(",,ABC,DEF", Result = new string[] { "", "", "ABC", "DEF" })]
    [TestCase("ABC,,DEF", Result = new string[] { "ABC", "", "DEF" })]
    [TestCase("ABC,,,DEF", Result = new string[] { "ABC", "", "", "DEF" })]
    [TestCase("\"ABC\",\"DE\"\"FG\"", Result = new string[] { "ABC", "DE\"FG" })]
    [TestCase(" ABC, \"DEF\" ", Result = new string[] { "ABC", "DEF" }, Description = "with spaces")]
    [TestCase("", Result = null)]
    //[TestCase("\"ABC", ExpectedException = typeof(ParsingException), Description="last quote missing")]
    //[TestCase("ABC\"", ExpectedException = typeof(ParsingException), Description = "first quote missing")]
    [TestCase("\"AB\"CD\"", ExpectedException = typeof(ParsingException), Description = "middle quote missing")]
    public string[] CommaStringToArray(string s)
    {
      return DataTools.CommaStringToArray(s);
    }

    [TestCase("abCD00", Result = "171|205|0")]
    [TestCase("", Result = "")]
    [TestCase(null, Result = "")]
    public string HexToBytes(string s)
    {
      byte[] a1 = DataTools.HexToBytes(s);
      string[] a2 = new string[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = StdConvert.ToString(a1[i]);
      return String.Join("|", a2);
    }

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
      return DataTools.GetNewLineSeparators(s);
    }

    [TestCase("ABCABCA", 'A', Result = 3)]
    [TestCase("BCD", 'A', Result = 0)]
    [TestCase("", 'A', Result = 0)]
    [TestCase(null, 'A', Result = 0)]
    public int GetCharCount(string s, char searchChar)
    {
      return DataTools.GetCharCount(s, searchChar);
    }

    [TestCase("ABCD", "DB", Result = 1)]
    [TestCase("ABCD", "YZ", Result = -1)]
    [TestCase("ABCD", "", Result = -1)]
    [TestCase("ABCD", null, Result = -1)]
    [TestCase("", "ABC", Result = -1)]
    [TestCase(null, "ABC", Result = -1)]
    [TestCase("ABCDEFGHIJ", "1234567890", Result = -1)] // используется CharArrayIndexer
    [TestCase("ABCDEFGHIJ6", "1234567890", Result = 10)] // используется CharArrayIndexer
    public int IndexOfAny(string str, string searchChars)
    {
      return DataTools.IndexOfAny(str, searchChars);
    }

    [TestCase("ABCD", "DB", Result = 0)]
    [TestCase("ABCD", "AC", Result = 1)]
    [TestCase("ABCD", "", Result = 0)]
    [TestCase("ABCD", null, Result = 0)]
    [TestCase("", "ABC", Result = -1)]
    [TestCase(null, "ABC", Result = -1)]
    public int IndexOfAnyOther(string str, string searchChars)
    {
      return DataTools.IndexOfAnyOther(str, searchChars);
    }

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
      return DataTools.IsSubstring(s, startPos, substring, comparisionType);
    }

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

      string s1 = DataTools.JoinNotEmptyStrings(separator, values);

      List<string> lst2 = new List<string>();
      lst2.AddRange(values);
      string s2 = DataTools.JoinNotEmptyStrings(separator, lst2);
      string s3 = DataTools.JoinNotEmptyStrings(separator, (IEnumerable<string>)lst2);

      Assert.AreEqual(s1, s2, "Overloads with string[] and IList returns different results");
      Assert.AreEqual(s1, s3, "Overloads with string[] and IEnumerable returns different results");
      return s1;
    }

    [TestCase("AB", 5, '#', Result = "#AB##")]
    [TestCase("AB", 2, '#', Result = "AB")]
    [TestCase("AB", 1, '#', Result = "A")]
    [TestCase("", 1, '#', Result = "#")]
    [TestCase(null, 1, '#', Result = "#")]
    public string PadCenter(string s, int length, char paddingChar)
    {
      return DataTools.PadCenter(s, length, paddingChar);
    }

    [TestCase("AB", 5, '#', Result = "###AB")]
    [TestCase("AB", 2, '#', Result = "AB")]
    [TestCase("AB", 1, '#', Result = "B")]
    [TestCase("", 1, '#', Result = "#")]
    [TestCase(null, 1, '#', Result = "#")]
    public string PadLeft(string s, int length, char paddingChar)
    {
      return DataTools.PadLeft(s, length, paddingChar);
    }

    [TestCase("AB", 5, '#', Result = "AB###")]
    [TestCase("AB", 2, '#', Result = "AB")]
    [TestCase("AB", 1, '#', Result = "A")]
    [TestCase("", 1, '#', Result = "#")]
    public string PadRight(string s, int length, char paddingChar)
    {
      return DataTools.PadRight(s, length, paddingChar);
    }

    [TestCase("ABCDEF", "DCBZ", Result = "AEF")]
    [TestCase("ABCDEF", "XYZ", Result = "ABCDEF")]
    [TestCase("", "XYZ", Result = "")]
    [TestCase(null, "XYZ", Result = "")]
    [TestCase("ABCDEF", "", Result = "ABCDEF")]
    [TestCase("ABCDEF", null, Result = "ABCDEF")]
    public string RemoveChars(string str, string removedChars)
    {
      return DataTools.RemoveChars(str, removedChars);
    }

    [TestCase("ABCCD", 'C', Result = "ABCD")]
    [TestCase("AABCAA", 'A', Result = "ABCA")]
    [TestCase("AAABC", 'A', Result = "ABC")]
    [TestCase("AAABC", 'Z', Result = "AAABC")]
    [TestCase("", 'Z', Result = "")]
    [TestCase(null, 'Z', Result = "")]
    public string RemoveDoubleChars(string str, char searchChar)
    {
      return DataTools.RemoveDoubleChars(str, searchChar);
    }

    [TestCase("ABCDEF", "DCBZ", Result = "BCD")]
    [TestCase("ABCDEF", "XYZ", Result = "")]
    [TestCase("", "XYZ", Result = "")]
    [TestCase(null, "XYZ", Result = "")]
    [TestCase("ABCDEF", "", Result = "")]
    [TestCase("ABCDEF", null, Result = "")]
    public string RemoveOtherChars(string str, string validChars)
    {
      return DataTools.RemoveOtherChars(str, validChars);
    }

    [TestCase("ABCDEF", "DBZ", '0', Result = "A0C0EF")]
    [TestCase("", "DB", '0', Result = "")]
    [TestCase(null, "DB", '0', Result = "")]
    [TestCase("ABCDEF", "", '0', Result = "ABCDEF")]
    [TestCase("ABCDEF", null, '0', Result = "ABCDEF")]
    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEVWXYZ", '0', Result = "00000FGHIJKLMNOPQRSTU00000")] // используется индексатор
    public string ReplaceAny(string str, string searchChars, char replaceChar)
    {
      string res = DataTools.ReplaceAny(str, searchChars, replaceChar);

      if (str != null)
        Assert.AreEqual(str.Length, res.Length);
      return res;
    }

    [TestCase("ABCDEF", "DBZ", '0', Result = "0B0D00")]
    [TestCase("", "DB", '0', Result = "")]
    [TestCase(null, "DB", '0', Result = "")]
    [TestCase("ABCDEF", "", '0', Result = "000000")]
    [TestCase("ABCDEF", null, '0', Result = "000000")]
    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEVWXYZ", '0', Result = "ABCDE0000000000000000VWXYZ")] // используется индексатор
    public string ReplaceAnyOther(string str, string searchChars, char replaceChar)
    {
      string res = DataTools.ReplaceAnyOther(str, searchChars, replaceChar);

      if (str != null)
        Assert.AreEqual(str.Length, res.Length);
      return res;
    }


    [TestCase("ABCDEF", "DFA", "123", Result = "3BC1E2")]
    [TestCase("", "DF", "12", Result = "")]
    [TestCase(null, "DF", "12", Result = "")]
    [TestCase("ABCDEF", "", "", Result = "ABCDEF")]
    [TestCase("ABCDEF", "ABC", "12", ExpectedException = typeof(ArgumentException))]
    public string ReplaceChars(string str, string searchChars, string replaceChars)
    {
      string res = DataTools.ReplaceChars(str, searchChars, replaceChars);

      if (str != null)
        Assert.AreEqual(str.Length, res.Length);
      return res;
    }

    [TestCase("AHBCEF", 'B', 'C', '0', Result = "AH00EF")]
    [TestCase("", 'B', 'C', '0', Result = "")]
    [TestCase(null, 'B', 'C', '0', Result = "")]
    [TestCase("AHBCEF", 'C', 'B', '0', Result = "AHBCEF")]
    public string ReplaceCharRange(string str, char firstChar, char lastChar, char replaceChar)
    {
      string res = DataTools.ReplaceCharRange(str, firstChar, lastChar, replaceChar);

      if (str != null)
        Assert.AreEqual(str.Length, res.Length);
      return res;
    }

    [TestCase("1AB", '0', Result = "0AB")]
    [TestCase("AB0", '0', Result = "AB0")]
    [TestCase("ABC", '0', Result = "ABC")]
    [TestCase("", '0', Result = "")]
    [TestCase(null, '0', Result = "")]
    public string ReplaceDigits(string str, char c)
    {
      string res = DataTools.ReplaceDigits(str, c);

      if (str != null)
        Assert.AreEqual(str.Length, res.Length);
      return res;
    }

    [TestCase("AB", 5, Result = "AB")]
    [TestCase("AB", 2, Result = "AB")]
    [TestCase("AB", 1, Result = "A")]
    [TestCase("", 1, Result = "")]
    [TestCase(null, 1, Result = "")]
    public string StrLeft(string s, int length)
    {
      return DataTools.StrLeft(s, length);
    }

    [TestCase("AB", 5, Result = "AB")]
    [TestCase("AB", 2, Result = "AB")]
    [TestCase("AB", 1, Result = "B")]
    [TestCase("", 1, Result = "")]
    [TestCase(null, 1, Result = "")]
    public string StrRight(string s, int length)
    {
      return DataTools.StrRight(s, length);
    }

    [Test]
    public void ToLowerInvariant_for_array()
    {
      string[] a = new string[] { "abc", null, "DEF" };

      string[] res = DataTools.ToLowerInvariant(a);

      Assert.AreEqual(a.Length, res.Length);
      Assert.AreEqual(res[0], "abc");
      Assert.IsNull(res[1]);
      Assert.AreEqual(res[2], "def");
    }

    [Test]
    public void ToLowerInvariant_for_array_null()
    {
      string[] a = null;

      string[] res = DataTools.ToLowerInvariant(a);

      Assert.IsNull(res);
    }

    [TestCase("ABC", Result = "Abc")]
    [TestCase("abc", Result = "Abc")]
    [TestCase("..ABC", Result = "..abc")]
    [TestCase("", Result = "")]
    [TestCase(null, Result = "")]
    public string ToUpperFirstInvariant(string s)
    {
      return DataTools.ToUpperFirstInvariant(s);
    }

    [TestCase("1|2|3", Result = "1,2,3")]
    [TestCase("1", Result = "1")]
    [TestCase("", Result = "")]
    public static string ToStringArray_int32(string sValues)
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
      string s1 = String.Join(",", DataTools.ToStringArray<Int32>(a2));
      string s2 = String.Join(",", DataTools.ToStringArray<Int32>(lst2));
      string s3 = String.Join(",", DataTools.ToStringArray<Int32>((IEnumerable<Int32>)lst2));
      string s4 = String.Join(",", DataTools.ToStringArray((System.Collections.IList)lst2));
      string s5 = String.Join(",", DataTools.ToStringArray( (System.Collections.IEnumerable)lst2));

      // Assert
      Assert.AreEqual(s1, s2, "Overloads with string[] and IList<Int32> returns different results");
      Assert.AreEqual(s1, s3, "Overloads with string[] and IEnumerable<Int32> returns different results");
      Assert.AreEqual(s1, s4, "Overloads with string[] and System.Collections.IList returns different results");
      Assert.AreEqual(s1, s5, "Overloads with string[] and System.Collections.IEnumerable returns different results");

      // Result
      return s1;
    }


    [TestCase(",", "1|2|3", Result = "1,2,3")]
    [TestCase(",", "1", Result = "1")]
    [TestCase(",", "",Result = "")]
    public static string ToStringJoin_int32(string separator, string sValues)
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
      string s1 = DataTools.ToStringJoin<Int32>(separator, a2);
      string s2 = DataTools.ToStringJoin<Int32>(separator, lst2);
      string s3 = DataTools.ToStringJoin<Int32>(separator, (IEnumerable<Int32>)lst2);
      string s4 = DataTools.ToStringJoin(separator, (System.Collections.IList)lst2);
      string s5 = DataTools.ToStringJoin(separator, (System.Collections.IEnumerable)lst2);

      // Assert
      Assert.AreEqual(s1, s2, "Overloads with string[] and IList<Int32> returns different results");
      Assert.AreEqual(s1, s3, "Overloads with string[] and IEnumerable<Int32> returns different results");
      Assert.AreEqual(s1, s4, "Overloads with string[] and System.Collections.IList returns different results");
      Assert.AreEqual(s1, s5, "Overloads with string[] and System.Collections.IEnumerable returns different results");

      // Result
      return s1;
    }

    [Test]
    public void ToUpperInvariant_for_array()
    {
      string[] a = new string[] { "abc", null, "DEF" };

      string[] res = DataTools.ToUpperInvariant(a);

      Assert.AreEqual(a.Length, res.Length);
      Assert.AreEqual(res[0], "ABC");
      Assert.IsNull(res[1]);
      Assert.AreEqual(res[2], "DEF");
    }

    [Test]
    public void ToUpperInvariant_for_array_null()
    {
      string[] a = null;

      string[] res = DataTools.ToLowerInvariant(a);

      Assert.IsNull(res);
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
      return DataTools.ToUpperWordsInvariant(s);
    }


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
      return DataTools.TrimEndNewLineSeparators(s, trimAll);
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
      return DataTools.TrimEndNewLineSeparators(s, trimAll, "\n");
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
      return DataTools.TrimStartNewLineSeparators(s, trimAll);
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
      return DataTools.TrimStartNewLineSeparators(s, trimAll, "\n");
    }
  }
}
