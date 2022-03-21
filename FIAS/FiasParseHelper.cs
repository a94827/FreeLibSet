// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// ������� ������ � ���������� �� �������
  /// </summary>
  internal class FiasParseHelper
  {
    #region ������������� ������

    public FiasHandler Handler;

    public FiasParseSettings ParseSettings;

    public string[] CellStrings;

    #endregion

    #region �������� �����

    public FiasAddress Parse()
    {
      if (CellStrings.Length != ParseSettings.CellLevels.Length)
        throw new BugException();

      if (CellStrings.Length == 0)
        throw new ArgumentNullException();

      for (int i = 0; i < CellStrings.Length; i++)
      {
        string s = CellStrings[i].Replace("--", "-");
        s = s.Replace("- ", "-");
        s = s.Replace(" -", "-");
        s = s.Replace("  ", " ");
        s = s.Replace(" .", ".");

        CellStrings[i] = s.Trim();

        if (i > 0)
        {
          // TODO: �������� ������������� �������
        }
      }

      //_Handler.FillAddress(_BaseAddress);
      _BestAddress = null;
      //_BestTail = String.Join(",", CellStrings); // ���� �� ���������� ������ ������
      _BestTail = String.Empty;

      AddPart(0, CellStrings[0], ParseSettings.CellLevels[0], ParseSettings.BaseAddress.Clone());

      if (!String.IsNullOrEmpty(_BestTail))
        _BestAddress.AddMessage(ErrorMessageKind.Error, "������ �������� �������� ������ \"" + _BestTail + "\", ��� ��� ��� ������ ������ ���������");

      if (_BestAddress == null)
        return ParseSettings.BaseAddress; // ������ �� �����
      else
        return _BestAddress;
    }

    #endregion

    #region ������ �����

    private FiasAddress _BestAddress;

    private string _BestTail;

    /// <summary>
    /// ��������� ������-����������� � ������ ������������ �������.
    /// ���� ���������� "�����", ��� ������ �����, ������������ true, � ������ ����� ���������� �� �����
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="s"></param>
    /// <param name="address">�����-����������</param>
    /// <returns></returns>
    private bool CompareWithTheBest(int currentCellIndex, string s, FiasAddress address)
    {
      // 09.03.2021
      // ���� ���� "�����", �� �������� ��������� �����

      if (!String.IsNullOrEmpty(s))
        return false;

      // �������� �����
      for (int i = currentCellIndex + 1; i < ParseSettings.CellLevels.Length; i++)
      {
        if (!String.IsNullOrEmpty(CellStrings[i]))
        {
          if (s.Length > 0)
            return false;
          //  s += ", ";
          //s += CellStrings[i];
        }
      }

      Handler.FillAddress(address);

      if (DoCompareWithTheBest(s, address))
      {
        _BestAddress = address.Clone();
        _BestTail = s;
        return true;
      }
      else
        return false;
    }

    private bool DoCompareWithTheBest(string s, FiasAddress address)
    {
      if (address.NameBottomLevel == FiasLevel.Unknown)
        return false; // ��������

      if (_BestAddress == null)
        return true;

      // 09.09.2021. ��������� ������� ������
      int cmpSeverity = ErrorMessageList.Compare(address.Messages.Severity, _BestAddress.Messages.Severity);
      if (cmpSeverity != 0)
        return cmpSeverity < 0;

      // ��������� ������� ������
      if (s.Length > 0 != _BestTail.Length > 0)
        return s.Length == 0;

      FiasLevel testLevel = address.NameBottomLevel;

      int ex1 = GetRBExistance(address, testLevel); // ����������
      int ex2 = GetRBExistance(_BestAddress, testLevel); // ������� �������
      if (ex1 != ex2)
        return ex1 < ex2;

      return testLevel < _BestAddress.NameBottomLevel; // ???
    }

    private int GetRBExistance(FiasAddress address, FiasLevel testLevel)
    {
      if (address.GetGuid(testLevel) != Guid.Empty)
        return 1;

      switch (FiasTools.GetTableType(testLevel))
      {
        case FiasTableType.House:
          if (!Handler.Source.DBSettings.UseHouse)
            return 2; // ��� �����������
          break;
        case FiasTableType.Room:
          if (!Handler.Source.DBSettings.UseRoom)
            return 2; // ��� �����������
          break;
      }

      if (address.GetMessages(testLevel).Severity != ErrorMessageKind.Info)
        return 3;
      else
        return 2;
    }

    #endregion

    #region ����������� �����

    private bool AddPart(int currentCellIndex, string s, FiasLevelSet levels, FiasAddress address)
    {
      if (!levels.IsEmpty)
        address.ClearStartingWith(levels.TopLevel);


      if (String.IsNullOrEmpty(s))
        return AddNextPart(currentCellIndex, address);

      FiasLevel lastLevel = address.NameBottomLevel; // ��������� ����������� �������

      // ��������� ��� ��������� ������, ��� ������� �������� ������������
      bool res = false;
      if (!levels.IsEmpty)
      {
        foreach (FiasLevel level in levels)
        {
          // ���������� ������
          address.ClearStartingWith(levels.TopLevel);
          FiasLevelSet levels2 = levels.GetBelow(level);

          if (FiasTools.IsInheritableLevel(lastLevel, level, true))
          {
            if (AddPart2(currentCellIndex, s, levels2, address, level))
              res = true;
          }
        }
      }

      if (res)
        return true;


      return CompareWithTheBest(currentCellIndex, s, address);
    }

    private bool AddNextPart(int currentCellIndex, FiasAddress address)
    {
      if (currentCellIndex < (ParseSettings.CellLevels.Length - 1))
        return AddPart(currentCellIndex + 1, CellStrings[currentCellIndex + 1], ParseSettings.CellLevels[currentCellIndex + 1], address); // ����������� ����� ��� ���������� ������
      else
        return CompareWithTheBest(currentCellIndex, string.Empty, address);
    }

    private bool AddPart2(int currentCellIndex, string s, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      string s2 = s;
      string sOthers = String.Empty;
      int pComma = s.IndexOf(',');
      if (pComma >= 0)
      {
        s2 = s.Substring(0, pComma);
        sOthers = s.Substring(pComma); // ������� �������
      }

      bool res = false;
      bool aoTypeFound = false;

      // ����� �� ��� ��������� ������� ���� �� ��� ����� ������� �����
      bool aoTypeBeforeName, aoTypeAfterName;
      FiasTools.GetAOTypePlace(level, out aoTypeBeforeName, out aoTypeAfterName);

      int[] pSpaces = FindSpacePositions(s2);
      // ������� ������, ����������� ���������, ����� ���� � ���� ��������� �������?
      int maxAOTypeParts = Math.Min(pSpaces.Length, Handler.AOTypes.GetMaxSpaceCount(level) + 1);

      if (aoTypeBeforeName)
      {
        int pDot = s2.IndexOf('.');
        if (pDot >= 0 && (pSpaces.Length == 0 || pDot < pSpaces[0]))
        {
          // ������������, ��� ������������ ���������� � ������ ���� ����� ����, ��������, "�. 1"

          string aoType = s2.Substring(0, pDot + 1); // ������� �����
          string nm = s2.Substring(pDot + 1).Trim(); // ��� ����� ���� �������
          string fullAOType;
          if (IsValidAOType(level, aoType, out fullAOType))
          {
            // ���������� �������� ��� ������
            aoTypeFound = true;
            RemoveNumChar(ref nm, level);
            if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
            if (AddHouseNumWithSpace(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
          }
        }
      }

      // ���������� ��������, ����� ���� ��� ��������� �������, � ����� - ������������ ("����� ������")
      if (aoTypeBeforeName)
      {
        for (int i = 0; i < maxAOTypeParts; i++)
        {
          // ������������, ��� ������������ ���������� � ��������, ��������, "��� 1"
          string aoType = s2.Substring(0, pSpaces[i]);
          string nm = s2.Substring(pSpaces[i] + 1); // ��� ����� ���� ��������� �������
          string fullAOType;
          if (IsValidAOType(level, aoType, out fullAOType))
          {
            // ���������� �������� ��� ������
            aoTypeFound = true;
            RemoveNumChar(ref nm, level);
            if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
            if (AddHouseNumWithSpace(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
          }
        }
      }

      // ���������� ��������, ����� ���� ������������, � ����� - ��� ��������� ������� ("��������� �����")
      if (aoTypeAfterName)
      {
        for (int i = 0; i < maxAOTypeParts; i++)
        {
          // ������������, ��� ������������ ���������� � ��������, ��������, "��� 1"
          string nm = s2.Substring(0, pSpaces[pSpaces.Length - i - 1]);
          string aoType = s2.Substring(pSpaces[pSpaces.Length - i - 1] + 1); // ��� ����� ���� ��������� �������
          string fullAOType;
          if (IsValidAOType(level, aoType, out fullAOType))
          {
            // ���������� �������� ��� ������
            aoTypeFound = true;
            RemoveNumChar(ref nm, level);
            if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
          }
        }
      }

      if ((!aoTypeFound) ||
        level == FiasLevel.Region) // � ����� � ���� 2020 ���� ������ ������������ �������� "��������� �������"
      {
        // ������������ ����� ���� ��� ����������, ��������, "1"
        RemoveNumChar(ref s2, level);
        if (AddPartialSubsts(currentCellIndex, String.Empty, s2, sOthers, levels, address, level))
          res = true;
        if (AddHouseNumWithSpace(currentCellIndex, String.Empty, s2, sOthers, levels, address, level))
          res = true;
        if (AddNumWithoutSep(currentCellIndex, s2, sOthers, levels, address, level))
          res = true;
      }

      return res;
    }

    private bool IsValidAOType(FiasLevel level, string aoType, out string fullAOType)
    {
      if (String.IsNullOrEmpty(aoType))
      {
        fullAOType = String.Empty;
        return false;
      }

      if (level == FiasLevel.Structure)
      {
        switch (aoType)
        {
          case "�":
          case "�.":
            // ��� ����� ���� �������� ��� ����������
            fullAOType = String.Empty;
            return true;
        }
      }

      Int32 id;
      if (Handler.AOTypes.IsValidAOType(level, aoType, out fullAOType, out id))
        return true;

      // ���� ���������� � ������ ��� ��� �����
      if (aoType[aoType.Length - 1] != '.')
      {
        if (Handler.AOTypes.IsValidAOType(level, aoType + ".", out fullAOType, out id))
          return true;
      }
      else
      {
        if (Handler.AOTypes.IsValidAOType(level, aoType.Substring(0, aoType.Length - 1), out fullAOType, out id))
          return true;
      }

      return false;
    }

    /// <summary>
    /// ������� ������� ������ "�"
    /// </summary>
    /// <param name="s"></param>
    /// <param name="level"></param>
    private static void RemoveNumChar(ref string s, FiasLevel level)
    {
      if (s[0] == '�' || s[0] == 'N')
      {
        switch (level)
        {
          case FiasLevel.House:
          case FiasLevel.Building:
          case FiasLevel.Structure:
          case FiasLevel.Flat:
          case FiasLevel.Room:
            break;
          default:
            return; // �������� ������ ����� ���������� � ������
        }

        s = s.Substring(1).Trim(); // ����� ���� ������ ����� ����� ������
      }
    }

    /// <summary>
    /// ���������� ������ ������� �������� � ������
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static int[] FindSpacePositions(string s)
    {
      if (String.IsNullOrEmpty(s))
        return DataTools.EmptyInts;

      List<int> lst = null;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == ' ')
        {
          if (lst == null)
            lst = new List<int>();
          lst.Add(i);
        }
      }

      if (lst == null)
        return DataTools.EmptyInts;
      else
        return lst.ToArray();
    }

    /// <summary>
    /// ������� ������� ����� � ������ ��������� ������� ����������
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="fullAOType">����������, ������� ����� �������� ��� ��������� ������</param>
    /// <param name="s">������ � ������� ����. ����� ��������� ������</param>
    /// <param name="sOthers">���������� ����� ������, ������������ � �������</param>
    /// <param name="levels">���������� ������, ������� ����� ������������ ��� ������� ������</param>
    /// <param name="address"></param>
    /// <param name="level">������� �������</param>
    /// <returns>true, ���� ���-������ �������</returns>
    private bool AddPartialSubsts(int currentCellIndex, string fullAOType, string s, string sOthers, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      if (s.Length == 0)
        return false;

      bool res = false;

      // ��������� ������ �������
      if (FiasTools.IsValidName(s, level))
      {
        address.ClearStartingWith(level);
        address.SetName(level, s);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex, sOthers.TrimStart(',', ' '), levels, address)) // ����������� �����
          res = true;
      }

      // ��������� ������ �������
      if (FiasTools.IsValidName(s, level))
      {
        address.ClearStartingWith(level);
        address.SetName(level, s);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex, sOthers.TrimStart(',', ' '), levels, address)) // ����������� �����
          res = true;
      }

      int pSpace = s.IndexOf(' ');
      if (pSpace >= 0)
      {
        sOthers = s.Substring(pSpace) + sOthers; // �������, ������� � �������
        s = s.Substring(0, pSpace); // ����� ������, � ������� ����� ������ 

        if (!Handler.AOTypes.GetAOTypeLevels(s).IsEmpty)
          return false; // ����� �������� �����������
      }

      if (String.IsNullOrEmpty(s))
        return false;

      // ���� ������������� �����������
      // ����� � �������� �������
      bool charSepFound = false;
      for (int p = s.Length - 2; p >= 1; p--)
      {
        if (s[p] == '-' || s[p] == '/' || s[p] == '\\')
        {
          charSepFound = true;
          string leftPart = s.Substring(0, p);
          string rightPart = s.Substring(p + 1);
          if (FiasTools.IsValidName(leftPart, level))
          {
            address.ClearStartingWith(level);
            address.SetName(level, leftPart);
            address.SetAOType(level, fullAOType);
            address.ClearGuidsStartingWith(level);
            address.ClearRecIdsStartingWith(level);
            if (AddPart(currentCellIndex,
              rightPart + sOthers, // ����� �� ���� �������� ������� ����������� �� sOthers
              levels, address)) // ����������� �����
              res = true;
          }
        }
      }

      // ���� �������� �����<-->�����
      if (!charSepFound)
      {
        switch (level)
        {
          case FiasLevel.House:
          case FiasLevel.Building:
          case FiasLevel.Flat:
            // ������ ��� �������, ����� ������� ����� ���� �����������.
            // ��������, ��� "1�" - ��� 1, ����� �
            // �� ������, ���� ���� ������������ ����� �������
            int transPos = -1;
            int transCount = 0;
            for (int i = 1; i < s.Length; i++)
            {
              if ((Char.IsDigit(s[i - 1]) && Char.IsLetter(s[i])) ||
                (Char.IsLetter(s[i - 1]) && Char.IsDigit(s[i])))
              {
                transCount++;
                transPos = i;
              }
            }

            if (transCount == 1)
            {
              string leftPart = s.Substring(0, transPos);
              string rightPart = s.Substring(transPos);
              if (FiasTools.IsValidName(leftPart, level))
              {
                address.ClearStartingWith(level);
                address.SetName(level, leftPart);
                address.SetAOType(level, fullAOType);
                address.ClearGuidsStartingWith(level);
                address.ClearRecIdsStartingWith(level);
                if (AddPart(currentCellIndex,
                  rightPart + sOthers, // ����� �� ���� �������� ������� ����������� �� sOthers
                  levels, address)) // ����������� �����
                  res = true;
              }
            }
            break;
        }
      }

      return res;
    }


    /// <summary>
    /// ���� ������ ���� �� ���� ������, �������� "1 �" ����� ���� "1�"
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="fullAOType"></param>
    /// <param name="s"></param>
    /// <param name="sOthers"></param>
    /// <param name="levels"></param>
    /// <param name="address"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private bool AddHouseNumWithSpace(int currentCellIndex, string fullAOType, string s, string sOthers, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      if (level != FiasLevel.House)
        return false; // ��� ������ ������� �� ������

      if (s.Length == 0)
        return false;

      int pSpace = s.IndexOf(' ');
      if (pSpace < 0)
        return false;

      if (s.LastIndexOf(' ') != pSpace)
        return false; // ������ ������ �������

      // �� ������� ������ ���� �����, � ����� - �����

      string s1 = s.Substring(0, pSpace);
      string s2 = s.Substring(pSpace + 1);
      for (int i = 0; i < s1.Length; i++)
      {
        if (!Char.IsDigit(s1[i]))
          return false;
      }
      for (int i = 0; i < s2.Length; i++)
      {
        if (!Char.IsLetter(s2[i]))
          return false;
      }

      // ����� ���������
      string s3 = s1 + s2; // ��� �������

      if (FiasTools.IsValidName(s3, level))
      {
        address.ClearStartingWith(level);
        address.SetName(level, s3);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex, sOthers.TrimStart(',', ' '), levels, address)) // ����������� �����
          return true;
      }

      return false;
    }

    /// <summary>
    /// ������� ������� �����, � ������� ��� ������������, ��������, "���1������2��������3".
    /// ������� ������� "�����-�����" (�� �� ��������)
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="s"></param>
    /// <param name="sOthers"></param>
    /// <param name="levels"></param>
    /// <param name="address"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private bool AddNumWithoutSep(int currentCellIndex, string s, string sOthers, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          return false;
      }

      if (s.Length == 0)
        return false;

      // ���������, ��� ���� ������ ����� � 
      if (!Char.IsLetter(s[0]))
        return false;

      int digitStart = -1;
      bool moreDigitGroups = false;
      int nextLetterStart = -1;
      for (int i = 1; i < s.Length; i++)
      {
        if (Char.IsDigit(s[i]))
        {
          if (digitStart < 0)
            digitStart = i;
          else if (!Char.IsDigit(s[i - 1]))
            moreDigitGroups = true; // ���� ������ �������� ������
        }
        else if (Char.IsLetter(s[i]))
        {
          if (nextLetterStart < 0 && Char.IsDigit(s[i - 1]))
            nextLetterStart = i;
        }
        else
        {
          // �� ����� � �� �����
          return false;
        }
      }

      if (digitStart < 0)
        return false; // ����� ������� ������ �� ����, �������� "���"

      if (nextLetterStart >= 0)
      {
#if DEBUG
        if (nextLetterStart < 2)
          throw new BugException("NextLetterStart=" + nextLetterStart.ToString());
#endif
        if (moreDigitGroups)
        {
          // ���� ���� ��������� �������� �����, ��������, "���1���2", �� "���2" ���������� �� ��������� �����
          sOthers = s.Substring(nextLetterStart) + sOthers;
          s = s.Substring(0, nextLetterStart);
        }
        // � ����� - ��� ����� �������� ������, ��������, "���1�"
      }


      string aoType = s.Substring(0, digitStart);
      string nm = s.Substring(digitStart);
      string fullAOType;
      if (IsValidAOType(level, aoType, out fullAOType) &&
        FiasTools.IsValidName(nm, level)) // ��� - ��������� �������, ��� ��� ����� ������ ����� (�, ����� ����, ����� � �����)
      {
        address.ClearStartingWith(level);
        address.SetName(level, nm);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex,
          sOthers,
          levels, address)) // ����������� �����
          return true;
      }

      return false;
    }

    #endregion
  }
}
