using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Parsing;
using System.Runtime.InteropServices;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.FIAS
{
  /// <summary>
  /// ��������� ������� ������ ��������������, ����������� ������� FiasHandler.Format()
  /// </summary>
  public static class FiasFormatStringParser
  {
    #region ��������� TypeFormXXX

    /// <summary>
    /// ��������� ��� + ����������
    /// </summary>
    internal const int TypeFormNameAndAbbr = 0x100;

    /// <summary>
    /// ��������� ��� (".NAME")
    /// </summary>
    internal const int TypeFormName = 0x200;

    /// <summary>
    /// ��������� ��� ������� (".TYPE")
    /// </summary>
    internal const int TypeFormType = 0x300;

    /// <summary>
    /// ��������� ���������� (".ABBR")
    /// </summary>
    internal const int TypeFormAbbr = 0x400;

    /// <summary>
    /// ��������� ����� (".NUM")
    /// </summary>
    internal const int TypeFormNum = 0x500;

    /// <summary>
    /// ��������� �����, ������� � ��������� ������
    /// </summary>
    internal const int TypeFormAt = 0x600;

    /// <summary>
    /// ���������� ������������� ��������� �������.
    /// </summary>
    internal const int TypeFormGuid = 0x700;

    /// <summary>
    /// ������������� ������ � ���� ������
    /// </summary>
    internal const int TypeFormRecId = 0x800;

    /// <summary>
    /// ����� ��� ��������� ������ �������
    /// </summary>
    internal const int TypeLevelMask = 0xFF;

    /// <summary>
    /// ����� ��� ��������� ��������� TypeFormXXX
    /// </summary>
    internal const int TypeFormMask = 0xFF00;

    #endregion

    #region ��������� TypeXXX, �� ����������� � ������ �������

    /// <summary>
    /// ���������
    /// </summary>
    internal const int TypeFormConst = 0x1000;

    /// <summary>
    /// "TEXT"
    /// </summary>
    internal const int TypeFormText = 0x2000;

    /// <summary>
    /// "POSTALCODE"
    /// </summary>
    internal const int TypePostalCode = 0x3000;

    /// <summary>
    /// "REGIONCODE"
    /// </summary>
    internal const int TypeRegionCode = 0x4000;

    /// <summary>
    /// "AO.GUID"
    /// </summary>
    internal const int TypeAOGuid = 0x5000;

    /// <summary>
    /// "AO.RECID"
    /// </summary>
    internal const int TypeAORecId = 0x6000;

    /// <summary>
    /// "ANY.GUID"
    /// </summary>
    internal const int TypeAnyGuid = 0x7000;

    ///// <summary>
    ///// "ANY.RECID"
    ///// </summary>
    //internal const int TypeAnyRecId = 0x8000;

    /// <summary>
    /// "OKATO"
    /// </summary>
    internal const int TypeOKATO = 0x9000;

    /// <summary>
    /// "OKTMO"
    /// </summary>
    internal const int TypeOKTMO = 0xA000;

    /// <summary>
    /// "IFNSUL"
    /// </summary>
    internal const int TypeIFNSFL = 0xB000;

    ///// <summary>
    ///// "TERRIFNSUL"
    ///// </summary>
    //internal const int TypeTerrIFNSFL = 0xC000;

    /// <summary>
    /// "IFNSUL"
    /// </summary>
    internal const int TypeIFNSUL = 0xD000;

    ///// <summary>
    ///// "TERRIFNSUL"
    ///// </summary>
    //internal const int TypeTerrIFNSUL = 0xE000;

    #endregion

    #region ����������� �����������

    static FiasFormatStringParser()
    {
      ComponentTypes = new string[] {
        "TEXT",
        "AT.REGION",
        "AT.DISTRICT",
        "AT.CITY",
        "AT.VILLAGE",
        "AT.STREET",
        "AT.HOUSE",

        "POSTALCODE",

        "REGION",
        "REGION.NAME",
        "REGION.TYPE",
        "REGION.ABBR",
        "REGION.GUID",
        "REGION.RECID",
        "REGIONCODE",

        "DISTRICT",
        "DISTRICT.NAME",
        "DISTRICT.TYPE",
        "DISTRICT.ABBR",
        "DISTRICT.GUID",
        "DISTRICT.RECID",
        
        "CITY",
        "CITY.NAME",
        "CITY.TYPE",
        "CITY.ABBR",
        "CITY.GUID",
        "CITY.RECID",
        
        "VILLAGE",
        "VILLAGE.NAME",
        "VILLAGE.TYPE",
        "VILLAGE.ABBR",
        "VILLAGE.GUID",
        "VILLAGE.RECID",
        
        "STREET",
        "STREET.NAME",
        "STREET.TYPE",
        "STREET.ABBR",
        "STREET.GUID",
        "STREET.RECID",
        
        "AO.GUID",
        "AO.RECID",
        
        "HOUSE",
        "HOUSE.NAME",
        "HOUSE.NUM",
        "HOUSE.TYPE",
        "HOUSE.ABBR",
        "BUILD.NUM",
        "BUILD.TYPE",
        "BUILD.ABBR",
        "STR.NUM",
        "STR.TYPE",
        "STR.ABBR",
        "HOUSE.GUID",
        "HOUSE.RECID",
        
        "ROOM",
        "ROOM.NAME",
        "FLAT.NUM",
        "FLAT.TYPE",
        "FLAT.ABBR",
        "ROOM.NUM",
        "ROOM.TYPE",
        "ROOM.ABBR",
        "ROOM.GUID",
        "ROOM.RECID",
        
        "ANY.GUID",
        //"ANY.RECID",
        
        "OKATO",
        "OKTMO",
        "IFNSFL",
        //"TERRIFNSFL",
        "IFNSUL",
        //"TERRIFNSUL",
      };

      _ComponentTypeValues = new int[] {
        TypeFormText,
        TypeFormAt | (int)FiasLevel.Region,
        TypeFormAt | (int)FiasLevel.District,
        TypeFormAt | (int)FiasLevel.City,
        TypeFormAt | (int)FiasLevel.Village,
        TypeFormAt | (int)FiasLevel.Street,
        TypeFormAt | (int)FiasLevel.House,

        TypePostalCode,

        TypeFormNameAndAbbr | (int)FiasLevel.Region,
        TypeFormName | (int)FiasLevel.Region,
        TypeFormType | (int)FiasLevel.Region,
        TypeFormAbbr | (int)FiasLevel.Region,
        TypeFormGuid | (int)FiasLevel.Region,
        TypeFormRecId | (int)FiasLevel.Region,
        TypeRegionCode,

        TypeFormNameAndAbbr | (int)FiasLevel.District,
        TypeFormName | (int)FiasLevel.District,
        TypeFormType | (int)FiasLevel.District,
        TypeFormAbbr | (int)FiasLevel.District,
        TypeFormGuid | (int)FiasLevel.District,
        TypeFormRecId | (int)FiasLevel.District,

        TypeFormNameAndAbbr | (int)FiasLevel.City,
        TypeFormName | (int)FiasLevel.City,
        TypeFormType | (int)FiasLevel.City,
        TypeFormAbbr | (int)FiasLevel.City,
        TypeFormGuid | (int)FiasLevel.City,
        TypeFormRecId | (int)FiasLevel.City,

        TypeFormNameAndAbbr | (int)FiasLevel.Village,
        TypeFormName | (int)FiasLevel.Village,
        TypeFormType | (int)FiasLevel.Village,
        TypeFormAbbr | (int)FiasLevel.Village,
        TypeFormGuid | (int)FiasLevel.Village,
        TypeFormRecId | (int)FiasLevel.Village,

        TypeFormNameAndAbbr | (int)FiasLevel.Street,
        TypeFormName | (int)FiasLevel.Street,
        TypeFormType | (int)FiasLevel.Street,
        TypeFormAbbr | (int)FiasLevel.Street,
        TypeFormGuid | (int)FiasLevel.Street,
        TypeFormRecId | (int)FiasLevel.Street,

        TypeAOGuid,
        TypeAORecId,

        TypeFormNameAndAbbr | (int)FiasLevel.House,
        TypeFormName | (int)FiasLevel.House,
        TypeFormNum | (int)FiasLevel.House,
        TypeFormType | (int)FiasLevel.House,
        TypeFormAbbr | (int)FiasLevel.House,
        TypeFormNum | (int)FiasLevel.Building,
        TypeFormType | (int)FiasLevel.Building,
        TypeFormAbbr | (int)FiasLevel.Building,
        TypeFormNum | (int)FiasLevel.Structure,
        TypeFormType | (int)FiasLevel.Structure,
        TypeFormAbbr | (int)FiasLevel.Structure,
        TypeFormGuid | (int)FiasLevel.House,
        TypeFormRecId | (int)FiasLevel.House,

        TypeFormNameAndAbbr | (int)FiasLevel.Room,
        TypeFormName | (int)FiasLevel.Room,
        TypeFormNum | (int)FiasLevel.Flat,
        TypeFormType | (int)FiasLevel.Flat,
        TypeFormAbbr | (int)FiasLevel.Flat,
        TypeFormNum | (int)FiasLevel.Room,
        TypeFormType | (int)FiasLevel.Room,
        TypeFormAbbr | (int)FiasLevel.Room,
        TypeFormGuid | (int)FiasLevel.Room,
        TypeFormRecId | (int)FiasLevel.Room,
      
        TypeAnyGuid,
        //TypeAnyRecId,

        TypeOKATO,
        TypeOKTMO,
        TypeIFNSFL,
        //TypeTerrIFNSFL,
        TypeIFNSUL,
        //TypeTerrIFNSUL
      };

#if DEBUG
      if (_ComponentTypeValues.Length != ComponentTypes.Length)
        throw new BugException("ComponentTypeValues.Length!=ComponentTypes.Length");
#endif


      _ComponentTypeIndexer = new StringArrayIndexer(ComponentTypes, false);

      _TheParserList1 = new ParserList();
      _TheParserList1.Add(new TypeParser());
      _TheParserList1.Add(new ComplexSepParser());
      _TheParserList1.Add(new SimpleSepParser()); // ����� ComplexSepParser
      _TheParserList1.Add(new BraceParser());

      _TheParserList2 = new ParserList();
      _TheParserList2.Add(new TypeParser());
      _TheParserList2.Add(new StrConstParser());
      _TheParserList2.Add(new SpaceParser());
      _TheParserList2.Add(new BraceParser()); // ��� ����������� �������� ������
    }

    #endregion

    #region ���������

    /// <summary>
    /// ������ ���������� ����� ����������� ������
    /// </summary>
    public static readonly string[] ComponentTypes;

    private static readonly StringArrayIndexer _ComponentTypeIndexer;

    /// <summary>
    /// �������� ��� ���� FiasParsedFormatString.ItemType, ��������������� ������� ComponentTypes
    /// </summary>
    private static readonly int[] _ComponentTypeValues;

    /// <summary>
    /// ������, ������������ �� ���������
    /// </summary>
    public static string DefaultFormat = "TEXT";

    #endregion

    #region �������

    /// <summary>
    /// ��������� ������� ������ �������������� � ���������� ���������� ������ FiasParsedFormatString.
    /// � ������ ������� ������������ ����������
    /// </summary>
    /// <param name="format">������ �������</param>
    /// <returns>��������� �������� - ���������� ������ FiasParsedFormatString</returns>
    public static FiasParsedFormatString Parse(string format)
    {
      FiasParsedFormatString parsedFormat;
      string errorMessage;
      int errorStart, errorLen;
      if (TryParse(format, out parsedFormat, out errorMessage, out errorStart, out errorLen))
        return parsedFormat;
      else
        throw new FormatException(errorMessage);
    }

    /// <summary>
    /// ��������� ������� ������ �������������� � ������� ���������� ������ FiasParsedFormatString � ������ ������.
    /// </summary>
    /// <param name="format">������ �������</param>
    /// <param name="parsedFormat">���� ������������ ��������� �������� - ���������� ������ FiasParsedFormatString � ������ ������</param>
    /// <param name="errorMessage">���� ������������ ����� ��������� �� ������ � ������ �������</param>
    /// <returns>true, ���� ������� ������� ��������</returns>
    public static bool TryParse(string format, out FiasParsedFormatString parsedFormat, out string errorMessage)
    {
      int errorStart;
      int errorLen;
      ParsingData pd;
      return TryParse(format, out parsedFormat, out errorMessage, out errorStart, out errorLen, out pd);
    }

    /// <summary>
    /// ��������� ������� ������ �������������� � ������� ���������� ������ FiasParsedFormatString � ������ ������.
    /// </summary>
    /// <param name="format">������ �������</param>
    /// <param name="parsedFormat">���� ������������ ��������� �������� - ���������� ������ FiasParsedFormatString � ������ ������</param>
    /// <param name="errorMessage">���� ������������ ����� ��������� �� ������ � ������ �������</param>
    /// <param name="errorStart">� ������ ������ ���� ���������� ������ ��������� �������</param>
    /// <param name="errorLen">� ������ ������ ���� ���������� ����� ��������� �������</param>
    /// <returns>true, ���� ������� ������� ��������</returns>
    public static bool TryParse(string format, out FiasParsedFormatString parsedFormat, out string errorMessage, out int errorStart, out int errorLen)
    {
      ParsingData pd;
      return TryParse(format, out parsedFormat, out errorMessage, out errorStart, out errorLen, out pd);
    }

    /// <summary>
    /// ��������� ������� ������ �������������� � ������� ���������� ������ FiasParsedFormatString � ������ ������.
    /// ��� ���������� ������������� ��� ���������� ����� � ���������� ������ �������� ������
    /// </summary>
    /// <param name="format">������ �������</param>
    /// <param name="parsedFormat">���� ������������ ��������� �������� - ���������� ������ FiasParsedFormatString � ������ ������</param>
    /// <param name="errorMessage">���� ������������ ����� ��������� �� ������ � ������ �������</param>
    /// <param name="errorStart">� ������ ������ ���� ���������� ������ ��������� �������</param>
    /// <param name="errorLen">� ������ ������ ���� ���������� ����� ��������� �������</param>
    /// <param name="pd">���� ���������� ������ ��������</param>
    /// <returns>true, ���� ������� ������� ��������</returns>
    public static bool TryParse(string format, out FiasParsedFormatString parsedFormat, out string errorMessage, out int errorStart, out int errorLen, out ParsingData pd)
    {
      pd = new ParsingData(format);
      _TheParserList1.Parse(pd);

      if (pd.FirstErrorToken == null)
        parsedFormat = TryParse2(pd);
      else
        parsedFormat = null;

      if (pd.FirstErrorToken != null)
      {
        errorMessage = pd.FirstErrorToken.ErrorMessage.Value.Text;
        errorStart = pd.FirstErrorToken.Start;
        errorLen = pd.FirstErrorToken.Length;
        return false;
      }
      else
      {
        errorMessage = null;
        errorStart = 0;
        errorLen = 0;
        return true;
      }
    }

    private static FiasParsedFormatString TryParse2(ParsingData pd)
    {
      Token CurrSepToken = null;
      FiasParsedFormatString res = new FiasParsedFormatString();
      FiasParsedFormatString.FormatItem CurrItem = new FiasParsedFormatString.FormatItem();
      Token OpenBracesToken = null;

      for (int i = 0; i < pd.Tokens.Count; i++)
      {
        Token token = pd.Tokens[i];
        switch (token.TokenType)
        {
          case "Space":
            continue;

          case "{":
            if (OpenBracesToken != null)
            {
              token.SetError("��������� �������� ������ �� �����������");
              return null;
            }
            OpenBracesToken = token;
            CurrItem = new FiasParsedFormatString.FormatItem();
            break;

          case "}":
            if (OpenBracesToken == null)
            {
              token.SetError("�� ���� ����������� ������");
              return null;
            }
            OpenBracesToken = null;
            if (CurrItem.ItemType == 0)
            {
              if (CurrItem.Suffix != null)
              {
                token.SetError("���������� ������. ���� �������, �� ��� ���� ����������");
                return null;
              }
              if (CurrItem.Prefix == null)
              {
                token.SetError("������ ���������");
                return null;
              }
              CurrItem.ItemType = TypeFormConst;
            }
            if (CurrSepToken != null)
            {
              CurrItem.Separator = CurrSepToken.AuxData.ToString();
              CurrSepToken = null;
            }
            res.Items.Add(CurrItem);
            break;

          case "Type":
            string sType = token.AuxData.ToString();
            int pType = _ComponentTypeIndexer.IndexOf(sType);
            if (pType < 0)
            {
              if (sType.Length == 0)
                token.SetError("���������� ������. ��� ���������� ����� ������� �����");
              else if (sType[0] == '.' || sType[sType.Length - 1] == '.')
                token.SetError("��� ���������� �� ����� ���������� ��� ������������� �� �����");
              else if (sType.IndexOf("..",StringComparison.Ordinal) >= 0)
                token.SetError("� ���� ���������� �� ����� ���� ��� ����� ������");
              else
                token.SetError("����������� ��� ���������� \"" + sType + "\"");
              return null;
            }
            if (OpenBracesToken != null)
            {
              if (CurrItem.ItemType != 0)
              {
                token.SetError("������ �������� ������ ����� ���� ������ ���� ��� ����������");
                return null;
              }


              CurrItem.ItemType = _ComponentTypeValues[pType];
            }
            else
            {
              // ��������� ��� ��� �������� ������
              CurrItem = new FiasParsedFormatString.FormatItem();
              CurrItem.ItemType = _ComponentTypeValues[pType];
              if (CurrSepToken != null)
              {
                CurrItem.Separator = CurrSepToken.AuxData.ToString();
                CurrSepToken = null;
              }
              res.Items.Add(CurrItem);
            }
            break;

          case "String":
            if (OpenBracesToken == null)
            {
              token.SetError("���������� ������. ������ ��� �������� ������");
              return null;
            }
            if (CurrItem.ItemType == 0)
            {
              if (CurrItem.Prefix != null)
              {
                token.SetError("��� �������� ������ �� �����������");
                return null;
              }
              CurrItem.Prefix = token.AuxData.ToString();
            }
            else
            {
              if (CurrItem.Suffix != null)
              {
                token.SetError("��� �������� ������ �� �����������");
                return null;
              }
              CurrItem.Suffix = token.AuxData.ToString();
            }
            break;

          case "Sep":
            if (CurrSepToken != null)
            {
              token.SetError("�� ����� ���� ��� ����������� ������");
              return null;
            }
            CurrSepToken = token;
            if (OpenBracesToken != null)
            {
              token.SetError("���������� ������. ��������� ����������� ������ �������� ������");
              return null;
            }
            if (res.Items.Count == 0)
            {
              token.SetError("����������� �� ����� ���� �� ������� ����������");
              return null;
            }
            break;

          default:
            token.SetError("���������� ������. ����������� ������� \"" + token.TokenType + "\"");
            return null;
        }
      }

      if (OpenBracesToken != null)
      {
        OpenBracesToken.SetError("�� ������� ������ ����������� ������");
        return null;
      }

      if (CurrSepToken != null)
      {
        CurrSepToken.SetError("����������� �� ����� ���� ����� ���������� ����������");
        return null;
      }

      return res;
    }

    /// <summary>
    /// �������� ������������ ������ �������.
    /// ���������� �� TryParse() ������ ���, ��� �� ���������� ������ FiasParsedFormatString.
    /// </summary>
    /// <param name="format">����������� ������ ��������������</param>
    /// <param name="errorMessage">���� ���������� ��������� �� ������, ���� ������ ������������</param>
    /// <returns>true, ���� ������ �������������� �������� ����������.</returns>
    public static bool IsValidFormat(string format, out string errorMessage)
    {
      FiasParsedFormatString parsedFormat;
      return TryParse(format, out parsedFormat, out errorMessage);
    }

    /// <summary>
    /// �������� ������������ ������ �������.
    /// ���������� �� TryParse() ������ ���, ��� �� ���������� ������ FiasParsedFormatString.
    /// </summary>
    /// <param name="format">����������� ������ ��������������</param>
    /// <returns>true, ���� ������ �������������� �������� ����������.</returns>
    public static bool IsValidFormat(string format)
    {
      FiasParsedFormatString parsedFormat;
      string errorMessage;
      return TryParse(format, out parsedFormat, out errorMessage);
    }

    #endregion

    #region ������ ��������

    /// <summary>
    /// �������� ������ ��������
    /// </summary>
    private static readonly ParserList _TheParserList1;

    /// <summary>
    /// �������������� ������ ��������, ����������� ������ �������� ������
    /// </summary>
    private static readonly ParserList _TheParserList2;

    /// <summary>
    /// ������� ���������� ���� - ������������������ ��������� �������� � �����
    /// </summary>
    private class TypeParser : IParser
    {
      #region IParser Members

      private static readonly CharArrayIndexer ValidCharIndexer = new CharArrayIndexer("ABCDEFGHIJKLMNOPQRSTUVWXYZ.", false);

      public void Parse(ParsingData data)
      {
        int cnt = 0;
        for (int i = data.CurrPos; i < data.Text.Text.Length; i++)
        {
          if (ValidCharIndexer.Contains(data.GetChar(i)))
            cnt++;
          else
            break;
        }
        if (cnt > 0)
          data.Tokens.Add(new Token(data, this, "Type", data.CurrPos, cnt, data.Text.Text.Substring(data.CurrPos, cnt)));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// ������� ����������� � ��������
    /// </summary>
    private class ComplexSepParser : IParser
    {
      #region IParser Members

      void IParser.Parse(ParsingData data)
      {
        #region ������� �������

        // �� ����� ��������������� �� �������� ParsingData.CurrPos, �.�. ����� �� ��� ����������� ��� ������ Tokens.Add(), ������� �� ����������.
        // � ����� ����� ������������� �������� �� ���� �������, � ��� (Space, Sep, Space)
        int CurrPos = data.CurrPos;

        int cntSpace1 = 0;
        for (int i = CurrPos; i < data.Text.Text.Length; i++)
        {
          if (data.GetChar(i) == ' ')
            cntSpace1++;
          else
            break;
        }

        if (data.GetChar(CurrPos + cntSpace1) != '\"')
          return; // ���� �� �������, �� ��� �� ����

        if (cntSpace1 > 0)
        {
          data.Tokens.Add(new Token(data, this, "Space", CurrPos, cntSpace1));
          CurrPos += cntSpace1;
        }

        #endregion

        #region ����� � ��������

        StringBuilder sb = new StringBuilder();
        bool QuoteFound = false;
        int nextpos = CurrPos + 1;
        while (nextpos < data.Text.Text.Length)
        {
          if (data.GetChar(nextpos) == '\"')
          {
            if (data.GetChar(nextpos + 1) == '\"')
            {
              // ���������� �������
              nextpos += 2;
              sb.Append('\"');
            }
            else
            {
              // ����� �����������
              nextpos++;
              QuoteFound = true;
              break;
            }
          }
          else
          {
            // ������� ������
            sb.Append(data.GetChar(nextpos));
            nextpos++;
          }
        }

        Token SepToken = new Token(data, this, "Sep", CurrPos, nextpos - CurrPos, sb.ToString());
        data.Tokens.Add(SepToken);
        CurrPos += SepToken.Length;
        if (!QuoteFound)
        {
          SepToken.SetError("�� ������� ����������� ������� �����������");
          return;
        }

        #endregion

        #region ����������� �������

        int cntSpace2 = 0;
        for (int i = CurrPos; i < data.Text.Text.Length; i++)
        {
          if (data.GetChar(i) == ' ')
            cntSpace2++;
          else
            break;
        }

        if (cntSpace2 > 0)
          data.Tokens.Add(new Token(data, this, "Space", CurrPos, cntSpace2));

        #endregion
      }

      IExpression IParser.CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// ������� �������� ����������� (��� �������)
    /// </summary>
    private class SimpleSepParser : IParser
    {
      #region IParser Members

      private static readonly CharArrayIndexer ValidCharIndexer = new CharArrayIndexer(" .-,;/", false);

      public void Parse(ParsingData data)
      {
        int cnt = 0;
        for (int i = data.CurrPos; i < data.Text.Text.Length; i++)
        {
          if (ValidCharIndexer.Contains(data.GetChar(i)))
            cnt++;
          else
            break;
        }
        if (cnt > 0)
          data.Tokens.Add(new Token(data, this, "Sep", data.CurrPos, cnt, data.Text.Text.Substring(data.CurrPos, cnt)));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    private class BraceParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        switch (data.GetChar(data.CurrPos))
        {
          case '{':
            data.Tokens.Add(new Token(data, this, "{", data.CurrPos, 1));
            _TheParserList2.SubParse(data, new string[] { "}" });
            break;
          case '}':
            data.Tokens.Add(new Token(data, this, "}", data.CurrPos, 1));
            break;
        }
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// ��������� �������� ������ �������������� ������� FiasFormatStringParser.Parse().
  /// ���� ����� �� ����� �������� ������.
  /// ������ ���������� �������� ����� ���������� ��������������� ���������� ������ FiasHandler.Format().
  /// </summary>
  public sealed class FiasParsedFormatString
  {
    #region ���������� �����������

    internal FiasParsedFormatString()
    {
      _Items = new List<FormatItem>();
    }

    #endregion

    #region ����������

    [StructLayout(LayoutKind.Auto)]
    internal struct FormatItem
    {
      #region ����

      /// <summary>
      /// �����������, ��������� ����� ���� �����������
      /// </summary>
      public string Separator;

      /// <summary>
      /// �������.
      /// ��� ����������-��������� ���� ���������� ����������� ��������
      /// </summary>
      public string Prefix;

      ///// <summary>
      ///// ��� ����������.
      ///// ��� ����������-��������� ����� �������� "CONST".
      ///// </summary>
      //public string ItemType;

      /// <summary>
      /// �������� ��� ����������.
      /// ������� �� ���������� TypeFormXXX (���������) � (int)FiasLevel
      /// ��� ����������-��������� ����� �������� TypeConst.
      /// </summary>
      public int ItemType;


      /// <summary>
      /// �������
      /// </summary>
      public string Suffix;

      #endregion
    }

    internal List<FormatItem> Items { get { return _Items; } }
    private List<FormatItem> _Items;

    #endregion
  }
}
