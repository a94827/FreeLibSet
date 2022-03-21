// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Russian.PhoneticAlgorithms
{
  /// <summary>
  /// Daitch�Mokotoff Soundex
  /// </summary>
  public static class DMSoundex
  {
    private static readonly string[] _EmptyStrings = new string[0];
    private static readonly int[] _EmptyInts = new int[0];


    // �������� �������� ����������� �����:
    // http://www.jewishgen.org/InfoFiles/soundex.html

    // ���������� ��� ������������� �������� ����� �����:
    // https://github.com/iourinski/DMSoundex/blob/master/DMSoundex.pm

    #region ����������� �������

    /// <summary>
    /// ������ ������ ���������� ��������
    /// </summary>
    private static Dictionary<char, object> _ValidChars;

    /// <summary>
    /// ����� ����.
    /// ����: ������ ��� ���������� ��������, ���������� �����������.
    /// ��������: ������ �����, ��� �������, �� ������ ��������.
    /// ������� ������� �������� ������ ��, ��� �������, ������ �������, �������� ����.
    /// ���� ������ �������� ������ ������, ������������������ �� ����������.
    /// ���� ������ �������� ��������� ���������, ���������� "���������", � � ���������� 
    /// ����� ��������� �����.
    /// </summary>
    private static Dictionary<string, string[]> _Generic;

    /// <summary>
    /// ����, ����� ���������� ��������� � ������ �����
    /// </summary>
    private static Dictionary<string, string[]> _Beginning;

    /// <summary>
    /// ����, ���� ��������� ������ �������� �������
    /// </summary>
    private static Dictionary<string, string[]> _BeforeVowel;

    /// <summary>
    /// ������������ ����� ���������� ������������������ �������� � �������� ������
    /// </summary>
    private static int _MaxSeqLength;

    /// <summary>
    /// ������� �����.
    /// ���� - ������ �����
    /// �������� - null (�� ������������)
    /// </summary>
    private static Dictionary<char, object> _Vowels;

    /// <summary>
    /// ����������� ����������� �������������� �������
    /// </summary>
    static DMSoundex()
    {
      FillCodes();
    }

    private static void FillCodes()
    {
      #region ���������� �������

      string aValidChars = "abcdefghijklmnopqrstuvwxyz" + "��������������������������������";
      _ValidChars = new Dictionary<char, object>(aValidChars.Length);
      for (int i = 0; i < aValidChars.Length; i++)
        _ValidChars.Add(aValidChars[i], null);

      #endregion

      #region ������������ �������

      _Beginning = new Dictionary<string, string[]>();
      _BeforeVowel = new Dictionary<string, string[]>();
      _Generic = new Dictionary<string, string[]>();

      _Codes2Dict = new Dictionary<string, string[]>();

      // ������� ����� ��:
      // http://www.jewishgen.org/InfoFiles/soundex.html
      // ������ �������� � ��������� ����� � �����������

      // 23.08.2017
      // ����� ��� ����������, ������������ � A, E, I, O, U, J, and Y ��� ��� ������ �����

      AddCodes("AI,AJ,AY", /*"0"*/null, "1", "NC");
      AddCodes("AU", /*"0"*/null, "7", "NC");
      AddCodes("A", "0", "NC", "NC");
      AddCodes("B", "7");
      AddCodes("CHS", "5", "54", "54");
      AddCodes("CH", "5/4");
      AddCodes("CK", "5/45");
      AddCodes("CZ,CS,CSZ,CZS", "4");
      AddCodes("C", "5/4");
      AddCodes("DRZ,DRS", "4");
      AddCodes("DS,DSH,DSZ", "4");
      AddCodes("DZ,DZH,DZS", "4");
      AddCodes("D,DT", "3");
      AddCodes("EI,EJ,EY", /*"0"*/null, "1", "NC");
      AddCodes("EU", /*"1"*/null, "1", "NC");
      AddCodes("E", "0", "NC", "NC");
      AddCodes("FB", "7");
      AddCodes("F", "7");
      AddCodes("G", "5");
      AddCodes("H", "5", "5", "NC");
      AddCodes("IA,IE,IO,IU", /*"1"*/null, "NC", "NC");
      AddCodes("I", "0", "NC", "NC");
      AddCodes("J", "1/4");
      AddCodes("KS", "5", "54", "54");
      AddCodes("KH", "5");
      AddCodes("K", "5");
      AddCodes("L", "8");
      AddCodes("MN", null, "66", "66");
      AddCodes("M", "6");
      AddCodes("NM", null, "66", "66");
      AddCodes("N", "6");
      AddCodes("OI,OJ,OY", /*"0"*/null, "1", "NC");
      AddCodes("O", "0", "NC", "NC");
      AddCodes("P,PF,PH", "7");
      AddCodes("Q", "5");
      AddCodes("RZ,RS", "94/4");
      AddCodes("R", "9");
      AddCodes("SCHTSCH,SCHTSH,SCHTCH", "2", "4", "4");
      AddCodes("SCH", "4");
      AddCodes("SHTCH,SHCH,SHTSH", "2", "4", "4");
      AddCodes("SHT,SCHT,SCHD", "2", "43", "43");
      AddCodes("SH", "4");
      AddCodes("STCH,STSCH,SC", "2", "4", "4");
      AddCodes("STRZ,STRS,STSH", "2", "4", "4");
      AddCodes("ST", "2", "43", "43");
      AddCodes("SZCZ,SZCS", "2", "4", "4");
      AddCodes("SZT,SHD,SZD,SD", "2", "43", "43");
      AddCodes("SZ", "4");
      AddCodes("S", "4");
      AddCodes("TCH,TTCH,TTSCH", "4");
      AddCodes("TH", "3");
      AddCodes("TRZ,TRS", "4");
      AddCodes("TSCH,TSH", "4");
      AddCodes("TS,TTS,TTSZ,TC", "4");
      AddCodes("TZ,TTZ,TZS,TSZ", "4");
      AddCodes("T", "3");
      AddCodes("UI,UJ,UY", /*"0"*/null, "1", "NC");
      AddCodes("UE", /*"0"*/null, "NC", "NC");
      AddCodes("U", "0", "NC", "NC");
      AddCodes("V", "7");
      AddCodes("W", "7");
      AddCodes("X", "5", "54", "54");
      AddCodes("Y", "1", "NC", "NC");
      AddCodes("ZDZ,ZDZH,ZHDZH", "2", "4", "4");
      AddCodes("ZD,ZHD", "2", "43", "43");
      AddCodes("ZH,ZS,ZSCH,ZSH", "4");
      AddCodes("Z", "4");

      AddCodes("��,��", "0", "1", "NC");
      AddCodes("�", "0", "NC", "NC");
      AddCodes("�", "7");
      AddCodes("�", "7");
      AddCodes("�", "5");
      AddCodes("��", "4");
      AddCodes("���", "4");
      AddCodes("��,��", "4");
      AddCodes("�,��", "3");
      AddCodes("��,��", "0", "1", "NC");
      AddCodes("�", "0/1", "NC", "NC"); // ����� ���� ����� "�"
      AddCodes("�", "4");
      AddCodes("���,���,���", "2", "4", "4");
      AddCodes("�", "4");
      AddCodes("��,��,��", "1", "NC", "NC");
      AddCodes("�", "1", "NC", "NC");
      AddCodes("�", "1", "NC", "NC");
      AddCodes("��", "5", "54", "54");
      AddCodes("�", "5");
      AddCodes("�", "8");
      AddCodes("��", null, "66", "66");
      AddCodes("�", "6");
      AddCodes("��", null, "66", "66");
      AddCodes("�", "6");
      AddCodes("��", "0", "1", "NC");
      AddCodes("�", "0", "NC", "NC");
      AddCodes("��", "7");
      AddCodes("�", "7");
      AddCodes("��,��", "4");
      AddCodes("�", "9");
      AddCodes("���", "4");
      AddCodes("���,����", "2", "4", "4"); // ?
      AddCodes("��", "2", "43", "43"); // ?
      AddCodes("�", "4");
      AddCodes("��,��,��,���,���,��", "4");
      AddCodes("��,��", "4");
      AddCodes("�", "3");
      AddCodes("��", "0", "1", "NC");
      AddCodes("�,��", "0", "NC", "NC");
      AddCodes("��", "7");
      AddCodes("�", "7");
      AddCodes("�", "5");
      AddCodes("�", "4");
      AddCodes("��", "2", "43", "43");
      AddCodes("�,��", "4");
      AddCodes("��", "2", "43", "43");
      AddCodes("��", "2", "43", "43");
      AddCodes("��", "4");
      AddCodes("��,��", "4");
      AddCodes("�", "4");
      AddCodes("��", "2", "43", "43");
      AddCodes("�", "4");
      AddCodes("�,�", "NC");
      AddCodes("�", "0", "NC", "NC");
      AddCodes("�", "1", "NC", "NC");
      AddCodes("�", "1", "NC", "NC");
      AddCodes("�", "1", "NC", "NC");

#if DEBUG
      CheckDict(_Beginning);
      CheckDict(_BeforeVowel);
      CheckDict(_Generic);
#endif

      _Codes2Dict = null; // ������� �������� ������

      #endregion

      #region �������

      string aVowels = "aouiey" + "��������"/*�*/;

      _Vowels = new Dictionary<char, object>(aVowels.Length);
      for (int i = 0; i < aVowels.Length; i++)
        _Vowels.Add(aVowels[i], null);

      #endregion
    }

    private static void AddCodes(string keys, string xGeneric)
    {
      AddCodes(keys, xGeneric, xGeneric, xGeneric);
    }

    private static void AddCodes(string keys, string xBegining, string xBeforeVowel, string xGeneric)
    {
      keys = keys.ToLowerInvariant();
      string[] aKeys = keys.Split(',');
      for (int i = 0; i < aKeys.Length; i++)
      {
        string key = aKeys[i];
#if DEBUG
        for (int j = 0; j < key.Length; j++)
        {
          if (!_ValidChars.ContainsKey(key[j]))
            throw new Exception("���� \"" + key + "\" �������� ������������ ������ \"" + key[j] + "\"");
        }
#endif

        AddCodes2(_Beginning, key, xBegining);
        AddCodes2(_BeforeVowel, key, xBeforeVowel);
        AddCodes2(_Generic, key, xGeneric);
        _MaxSeqLength = Math.Max(_MaxSeqLength, key.Length);
      }
    }

    /// <summary>
    /// ������������ ������ � �������� �������������.
    /// �� ����� �������� �������� ��������� ��������, ���� {"1"}.
    /// ���� �� ����� ������������ ��������� ���������� ��� ���������� ��������
    /// </summary>
    private static Dictionary<string, string[]> _Codes2Dict;

    /// <summary>
    /// ���� ��� ������������ ������������������ �������� "NC"
    /// </summary>
    private static readonly string[] _NonCodedCodes = new string[1] { String.Empty };

    private static void AddCodes2(Dictionary<string, string[]> dict, string key, string xValue)
    {
      if (String.IsNullOrEmpty(xValue))
        return; // ���������� �� ������������. ��� �� ���� �����, ��� "�� ����������"

      string[] aValues;
      if (!_Codes2Dict.TryGetValue(xValue, out aValues))
      {
        if (xValue == "NC") // �� ����������
          aValues = _NonCodedCodes;
        else
        {
          aValues = xValue.Split('/');
#if DEBUG
          if (aValues.Length == 0)
            throw new Exception("��� ����� ��� ����� \"" + key + "\"");
          for (int i = 0; i < aValues.Length; i++)
          {
            if (String.IsNullOrEmpty(aValues[i]))
              throw new Exception("������ �������� ��� ��������� \"" + xValue + "\"");
            for (int j = 0; j < aValues[i].Length; j++)
            {
              char ch = aValues[i][j];
              if (ch < '0' || ch > '9')
                throw new Exception("������������ ������ \"" + ch + "\" � �������� \"" + xValue + "\"");
            }
          }
#endif
        }
        _Codes2Dict.Add(xValue, aValues);
      }

      dict.Add(key, aValues);
    }


#if DEBUG

    private static void CheckDict(Dictionary<string, string[]> dict)
    {
      #region �������� �������� � �������������������

      foreach (KeyValuePair<string, string[]> pair in dict)
      {
        string s = pair.Key;
        for (int i = 0; i < s.Length; i++)
        {
          if (!_ValidChars.ContainsKey(s[i]))
            throw new Exception("������������������ \"" + s + "\" �������� ������������ ������ \"" + s[i] + "\"");
        }
      }

      #endregion

      #region �������� ������� �������������������

      foreach (KeyValuePair<char, object> pair in _ValidChars)
      {
        string s = new string(pair.Key, 1);
        if (!dict.ContainsKey(s))
          throw new Exception("������� �� �������� ���������� ������� \"" + s + "\" (0x" + ((int)pair.Key).ToString("x") + ")");
      }

      #endregion
    }

#endif

    #endregion

    #region ���������� �����

    // ����� ���� ��, �������, ������������ ������� String ��� StringBuilder, �� ���� �����������
    // ������������ �������� ��������, � �� �������� ������

    private struct CodeAccumulator
    {
      #region ��������

      /// <summary>
      /// ����� � ��������� �� 0 �� 999999 � �������� ����������
      /// </summary>
      public int Value { get { return _Value; } }
      private int _Value;

      /// <summary>
      /// ���������� ����������� �������� � ��������� �� 0 �� 6
      /// </summary>
      public int Count { get { return _Count; } }
      private int _Count;

      #endregion

      #region ���������� ����

      private static readonly int[] _Muls = new int[6] { 100000, 10000, 1000, 100, 10, 1 };

      /// <summary>
      /// ���������� ������ ����.
      /// ���������� �����, ��� ������� �������, ��������������
      /// </summary>
      /// <param name="curr">������� ����������� ��������</param>
      /// <param name="ch">������ � ��������� �� '0' �� '9' </param>
      /// <returns>����� ����������� ��������</returns>
      public static CodeAccumulator operator +(CodeAccumulator curr, char ch)
      {
#if DEBUG
        if (ch < '0' || ch > '9')
          throw new ArgumentException("ch");
#endif

        int nCh = ch - '0'; // � ��������� �� 0 �� 9

        if (curr._Count >= 6)
          return curr; // �����������

        int v2 = _Muls[curr._Count] * nCh;

        CodeAccumulator next = new CodeAccumulator();
        next._Count = curr._Count + 1;
        next._Value = curr._Value + v2;
        return next;
      }

      /// <summary>
      /// ���������� ���������� ����.
      /// ���������� �����, ��� ������� �������, ��������������
      /// </summary>
      /// <param name="curr">������� ����������� ��������</param>
      /// <param name="chars">������� � ��������� �� '0' �� '9' </param>
      /// <returns>����� ����������� ��������</returns>
      public static CodeAccumulator operator +(CodeAccumulator curr, string chars)
      {
        CodeAccumulator res = curr;
        for (int i = 0; i < chars.Length; i++)
          res += chars[i];
        return res;
      }

      #endregion

      #region ��������� �������������

      public override string ToString()
      {
        return Value.ToString("000000");
      }

      #endregion

      #region ����������� ��������

      public static readonly CodeAccumulator[] EmptyArray = new CodeAccumulator[0];

      #endregion
    }

    #endregion

    #region �������� ������ ����������

    /// <summary>
    /// ��������� ������� Daitch�Mokotoff Soundex.
    /// ���������� ������ 6-���������� �����, �������������� � ��������� �������.
    /// ������ ������������ ������������ ���, �� ��������� ���������� �������� �����
    /// ��������� ��������� ���������
    /// </summary>
    /// <param name="s">������������� ������ </param>
    /// <returns>������ ����� DM-Soundex</returns>
    public static string[] Calculate(string s)
    {
      CodeAccumulator[] a = DoCalculate(s, null);
      if (a.Length == 0)
        return _EmptyStrings;

      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = a[i].ToString();
      return a2;
    }

    /// <summary>
    /// ��������� ������� Daitch�Mokotoff Soundex.
    /// ���������� ������ 6-���������� �����, �������������� � ��������� �������.
    /// ������ ������������ ������������ ���, �� ��������� ���������� �������� �����
    /// ��������� ��������� ���������.
    /// ������, �������� ���������� ����������
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <param name="debugInfo">���� ���������� ���������� ����������</param>
    /// <returns>������ ����� DM-Soundex</returns>
    public static string[] Calculate(string s, out DMSoundexCodingPart[] debugInfo)
    {
      List<DMSoundexCodingPart> debugList = new List<DMSoundexCodingPart>();
      CodeAccumulator[] a = DoCalculate(s, debugList);
      debugInfo = debugList.ToArray();

      if (a.Length == 0)
        return _EmptyStrings;

      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = a[i].ToString();
      return a2;
    }

    /// <summary>
    /// ��������� ������� Daitch�Mokotoff Soundex.
    /// ���������� ������ �������� ����� � ��������� �� 0 �� 999999
    /// ������ ������������ ������������ ���, �� ��������� ���������� �������� �����
    /// ��������� ��������� ���������
    /// </summary>
    /// <param name="s">������������� ������ </param>
    /// <returns>������ ����� DM-Soundex</returns>
    public static int[] CalculateInt(string s)
    {
      CodeAccumulator[] a = DoCalculate(s, null);
      if (a.Length == 0)
        return _EmptyInts;

      int[] a2 = new int[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = a[i].Value;
      return a2;
    }


    #endregion

    #region ���������� ����������

    private static CodeAccumulator[] DoCalculate(string s, List<DMSoundexCodingPart> debugList)
    {
      s = PrepareString(s);

      if (s.Length == 0)
        return CodeAccumulator.EmptyArray;

      // ���� ����� ������ ���� ���, ���� ������ ���������� "���������"
      CodeAccumulator singleCode = new CodeAccumulator(); // ������������ ���������
      List<CodeAccumulator> multiCodes = null; // ������������� ���������

      int pos = 0;
      while (pos < s.Length)
      {
        // ����� ����� ������� ������������������ ����� �����
        int n = Math.Min(_MaxSeqLength, s.Length - pos);

        // �������� ����� ������������������ � ������� ������������
        string[] codes = null;
        string seq = null;
        for (int len = n; len > 0; len--)
        {
          seq = s.Substring(pos, len); // ����������� ������������������
          if (pos == 0)
            _Beginning.TryGetValue(seq, out codes);
          else
          {
            bool IsVowel = false;
            if ((pos + len) < s.Length)
            {
              char NextChar = s[pos + len];
              IsVowel = _Vowels.ContainsKey(NextChar);
            }
            if (IsVowel)
              _BeforeVowel.TryGetValue(seq, out codes);
            else
              _Generic.TryGetValue(seq, out codes);
          }
          if (codes != null)
            break;
        }

        // 23.08.2017
        // ����������� ������ �����
        if (seq.Length == 1 && pos > 0)
        {
          if (s[pos - 1] == seq[0])
            codes = _NonCodedCodes;
        }

#if DEBUG
        if (codes == null)
          throw new Exception("��������� ������. �� ������� ������������������");
        if (codes.Length == 0)
          throw new Exception("��������� ������. ������� ������ ���� ��� ������������������ \"" + seq + "\"");
#endif

        if (debugList != null)
        {
          DMSoundexCodingPart di = new DMSoundexCodingPart(seq, codes);
          debugList.Add(di);
        }

        if (codes.Length > 1)
        {
          // ��������� � ������������� �����
          if (multiCodes == null)
          {
            multiCodes = new List<CodeAccumulator>();
            multiCodes.Add(singleCode);
          }
          // ��������� ���������
          int n2 = multiCodes.Count;
          for (int i = 0; i < n2; i++)
          {
            CodeAccumulator baseCode = multiCodes[i];
            multiCodes[i] = baseCode + codes[0];
            for (int j = 1; j < codes.Length; j++)
              multiCodes.Add(baseCode + codes[j]);
          }
        }
        else // Codes.Length=1
        {
          // ������� ���������� �����
          if (multiCodes == null)
            singleCode += codes[0];
          else
          {
            for (int i = 0; i < multiCodes.Count; i++)
              multiCodes[i] += codes[0];
          }
        }


        pos += seq.Length;
      }

      if (multiCodes == null)
        return new CodeAccumulator[1] { singleCode };
      else
      {
        // ������� �������� ������������� ����
        for (int i = multiCodes.Count - 1; i >= 1; i--)
        {
          bool remove = false;
          for (int j = 0; j < i; j++)
          {
            if (multiCodes[i].Value == multiCodes[j].Value)
            {
              remove = true;
              break;
            }
          }
          if (remove)
            multiCodes.RemoveAt(i);
        }

        return multiCodes.ToArray();
      }
    }

    #endregion

    #region ���������� ������

    /// <summary>
    /// �������� ���� ��������, ����� ValidChars.
    /// ���������� � ������� ��������.
    /// ������ "�"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string PrepareString(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      s = s.ToLowerInvariant();
      s = s.Replace('�', '�');

      for (int i = 0; i < s.Length; i++)
      {
        if (!_ValidChars.ContainsKey(s[i]))
        {
          // ���� ������ ������
          if (i == (s.Length - 1))
          {
            // ������ �������� ��������� ������
            return s.Substring(0, s.Length - 1);
          }

          StringBuilder sb = new StringBuilder(s.Length - 1);
          sb.Append(s, 0, i);
          for (int j = i + 1; j < s.Length; j++)
          {
            if (_ValidChars.ContainsKey(s[j]))
              sb.Append(s[j]);
          }
          return sb.ToString();
        }
      }

      // ��� ������� �������
      return s;
    }

    #endregion
  }

  /// <summary>
  /// ���������� ���������� � ��������� ������ �� ���� DM-Soundex.
  /// ��� ������ ����� ���� ������� ������ ����� ��������
  /// </summary>
  [Serializable]
  public struct DMSoundexCodingPart
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="sequence">������������������ �������� � �������� ������</param>
    /// <param name="codes">����������� ����</param>
    public DMSoundexCodingPart(string sequence, string[] codes)
    {
      _Sequence = sequence;
      _Codes = codes;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������������������ �������� � �������� ������
    /// </summary>
    public string Sequence { get { return _Sequence; } }
    private string _Sequence;

    /// <summary>
    /// ����������� ����.
    /// </summary>
    public string[] Codes { get { return _Codes; } }
    private string[] _Codes;

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ���������� ��������� ������������� � ���� "Sequence"=Codes
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb);
      return sb.ToString();
    }

    /// <summary>
    /// ���������� ��������� ������������� � ���� "Sequence"=Codes
    /// </summary>
    /// <param name="sb">����� ��� ����������</param>
    public void ToString(StringBuilder sb)
    {
      sb.Append("\"");
      sb.Append(Sequence);
      sb.Append("\"=");
      CodesToString(sb, Codes);
    }

    private static void CodesToString(StringBuilder sb, string[] codes)
    {
      if (codes == null)
        sb.Append("null");
      if (codes.Length == 1)
      {
        if (codes[0].Length == 0)
          sb.Append("NC");
        else
          sb.Append(codes[0]);
      }
      else
      {
        sb.Append("[");
        for (int i = 0; i < codes.Length; i++)
        {
          if (i > 0)
            sb.Append(",");
          sb.Append(codes[i]);
        }
        sb.Append("]");
      }
    }

    /// <summary>
    /// ������� ����� ����� �������
    /// </summary>
    /// <param name="info">������ ������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(DMSoundexCodingPart[] info)
    {
      StringBuilder sb = new StringBuilder();
      ToString(info, sb);
      return sb.ToString();
    }

    private static void ToString(DMSoundexCodingPart[] info, StringBuilder sb)
    {
      for (int i = 0; i < info.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        info[i].ToString(sb);
      }
    }

    #endregion
  }
}
