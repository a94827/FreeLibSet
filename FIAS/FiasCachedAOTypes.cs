using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Core;

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

namespace FreeLibSet.FIAS
{

  /// <summary>
  /// �������������� ������� ����� �������� ��������
  /// ���� ����� �� ������������ � ���������������� ����
  /// </summary>
  [Serializable]
  public sealed class FiasCachedAOTypes
  {
    #region ���������� �����������

    internal FiasCachedAOTypes(DataSet ds)
    {
      _DS = ds;

      OnDeserializedMethod(new System.Runtime.Serialization.StreamingContext());
    }

    #endregion

    #region ����

    /// <summary>
    /// ���������� ������� ������.
    /// �������� ���� "Id", "Level", "SOCRNAME", "SCNAME".
    /// </summary>
    private readonly DataSet _DS;

    /// <summary>
    /// ���������� ����� � ������� �� �������� (��� ���������� �����)
    /// </summary>
    public int RowCount
    {
      get
      {
        lock (_DS)
        {
          return _DS.Tables[0].Rows.Count;
        }
      }

    }

    #endregion

    #region ������������

    [System.Runtime.Serialization.OnDeserialized]
    private void OnDeserializedMethod(System.Runtime.Serialization.StreamingContext context)
    {
      DataTools.SetPrimaryKey(_DS.Tables[0], "Id");
    }

    #endregion

    #region ������

    /// <summary>
    /// ���������� ��� ��������� ������� �� ��������������.
    /// ���� ����� �������� ������ ��� ������� �������� �������� �� ������ ����� ������������, �� �� ��� ������ � ���������.
    /// </summary>
    /// <param name="id">������������� ���� ��������� �������</param>
    /// <param name="typeMode">������ ������������ ���� ��� ����������</param>
    /// <returns></returns>
    public string GetAOType(Int32 id, FiasAOTypeMode typeMode)
    {
      if (id == 0)
        return String.Empty;

      lock (_DS)
      {
        DataRow row = _DS.Tables[0].Rows.Find(id);
        if (row == null)
          return "?? " + id.ToString();
        else
          return DataTools.GetString(row, typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME");
      }
    }

    #region ����������� ������� ����

    [NonSerialized]
    private string[] _EstateStatusAOTypes;

    [NonSerialized]
    private string[] _EstateStatusAbbrs;


    [NonSerialized]
    private string[] _StructureStatusAOTypes;

    [NonSerialized]
    private string[] _StructureStatusAbbrs;


    [NonSerialized]
    private string[] _FlatTypeAOTypes;

    [NonSerialized]
    private string[] _FlatTypeAbbrs;


    [NonSerialized]
    private string[] _RoomTypeAOTypes;

    [NonSerialized]
    private string[] _RoomTypeAbbrs;


    private string[] PrepareSpecialName(ref string[] ourNames, string[] source)
    {
      if (ourNames != null)
        return ourNames;

      // ����� ���� ������, ����� �������� � �������� 0
      string[] a = new string[source.Length - 1];
      Array.Copy(source, 1, a, 0, a.Length);
      ourNames = a;
      return a;
    }


    #endregion

    /// <summary>
    /// �������� ������ ��������� ����� ���������������� ��������� ��� ���������� ��� ��������� ������
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="typeMode">��� ��� ����������</param>
    /// <returns>������ ����</returns>
    public string[] GetAOTypes(FiasLevel level, FiasAOTypeMode typeMode)
    {
      switch (level)
      {
        case FiasLevel.House:
          // 10.03.2021
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _EstateStatusAOTypes, FiasEnumNames.EstateStatusAOTypes);
          else
            return PrepareSpecialName(ref _EstateStatusAbbrs, FiasEnumNames.EstateStatusAbbrs);
        case FiasLevel.Building:
          return new string[1] { typeMode == FiasAOTypeMode.Full ? "������" : "����." };
        case FiasLevel.Structure:
          //return new string[1] { isLong ? "��������" : "���." };
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _StructureStatusAOTypes, FiasEnumNames.StructureStatusAOTypes);
          else
            return PrepareSpecialName(ref _StructureStatusAbbrs, FiasEnumNames.StructureStatusAbbrs);
        case FiasLevel.Flat:
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _FlatTypeAOTypes, FiasEnumNames.FlatTypeAOTypes);
          else
            return PrepareSpecialName(ref _FlatTypeAbbrs, FiasEnumNames.FlatTypeAbbrs);
        case FiasLevel.Room:
          if (typeMode == FiasAOTypeMode.Full)
            return PrepareSpecialName(ref _RoomTypeAOTypes, FiasEnumNames.RoomTypeAOTypes);
          else
            return PrepareSpecialName(ref _RoomTypeAbbrs, FiasEnumNames.RoomTypeAbbrs);
      }

#if DEBUG
      CheckLevel(level, true);
#endif

      string[] a;
      lock (_DS)
      {
        DataView dv = _DS.Tables[0].DefaultView;
        dv.Sort = typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME";
        dv.RowFilter = "LEVEL=" + (int)level;

        // � ������� ����� ���� �������������� ����������
        SingleScopeList<string> lst = new SingleScopeList<string>(dv.Count);
        for (int i = 0; i < dv.Count; i++)
          lst.Add(dv[i].Row[typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME"].ToString());

        a = lst.ToArray();
      }
      return a;
    }

    private void CheckLevel(FiasLevel level, bool includeHouseAndFlat)
    {
      if (FiasTools.GetTableType(level) == FiasTableType.AddrOb)
        return;

      if (includeHouseAndFlat)
      {
        if (level == FiasLevel.House || level == FiasLevel.Flat)
          return;
      }

      throw new ArgumentException("������������ ������� " + level.ToString() + " ��� ������ � ������� ����������. ����������� ������ ������, ����������� � ������� �������� ��������", "level");
    }

    /// <summary>
    /// ����� �������������� � ������� SOCRBASE �� �������� ��� �������� ������������ ���� ��������� �������
    /// </summary>
    /// <param name="level">������� ����������������� ��������</param>
    /// <param name="aoType">������� ��� ��������</param>
    /// <returns>��������� ������������� ��� 0</returns>
    public Int32 FindAOTypeId(FiasLevel level, string aoType)
    {
      if (level != FiasLevel.House)
        CheckLevel(level, false);
      if (String.IsNullOrEmpty(aoType))
        return 0;

      Int32 AOTypeId;
      lock (_DS)
      {
        DataView dv = _DS.Tables[0].DefaultView;
        dv.RowFilter = "LEVEL=" + (int)level;
        dv.Sort = "SOCRNAME";
        int p = dv.Find(aoType);
        if (p < 0)
        {
          dv.Sort = "SCNAME";
          p = dv.Find(aoType);

          // ���� ������ ���������� � ������, ���� ���������� ��� �����
          if (p < 0 &&
            aoType[aoType.Length - 1] == '.' &&
            aoType.Length > 1 &&
            aoType.IndexOf('-') < 0 /* ��� ������� */)

            p = dv.Find(aoType.Substring(0, aoType.Length - 1));
        }
        if (p < 0)
          AOTypeId = 0;
        else
          AOTypeId = DataTools.GetInt(dv[p].Row, "Id");
      }

      return AOTypeId;
    }

    /// <summary>
    /// ���������� true, ���� ��� ����������������� �������� ��� ���������� �������� ��� ��������� ������.
    /// ����������� ����������� �����.
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="aoType">����������� ���</param>
    /// <returns>������������</returns>
    public bool IsValidAOType(FiasLevel level, string aoType)
    {
      string longName;
      Int32 id;
      return IsValidAOType(level, aoType, out longName, out id);
    }

    /// <summary>
    /// ���������� true, ���� ��� ����������������� �������� ��� ���������� �������� ��� ��������� ������.
    /// ����������� ����������� �����.
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="aoType">����������� ���</param>
    /// <param name="fullAOType">���� ���������� ���������� ������ ��� ����������������� ��������.</param>
    /// <param name="id">���� ���������� ������������� � ������� "AOType".
    /// ����� ���� 0, ���� ������ ����������� ���</param>
    /// <returns>������������</returns>
    public bool IsValidAOType(FiasLevel level, string aoType, out string fullAOType, out Int32 id)
    {
      // ��� ���������� ������������� ���������� �� ������ ��� ����� CreateAOTypeLevelDict()

      fullAOType = String.Empty;
      id = 0;

      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          CheckLevel(level, false);
          break;
      }
      if (String.IsNullOrEmpty(aoType))
        return false;

      // ����������� �������
      switch (level)
      {
        case FiasLevel.House: // ���

          // 27.10.2020
          // ��� ����� ������� ��������� ����������� ��������.
          id = FindAOTypeId(level, aoType);
          if (id != 0)
          {
            fullAOType = GetAOType(id, FiasAOTypeMode.Full);
            return true;
          }

          // ����� �������� ��������� ������������� ����������, ��� ����
          FiasEstateStatus EstStatus = FiasEnumNames.ParseEstateStatus(aoType);
          if (EstStatus == FiasEstateStatus.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(EstStatus);
            return true;
          }

        //break;
        case FiasLevel.Building: // ������
          switch (aoType.ToLowerInvariant())
          {
            case "������":
            case "����.":
            case "�.":
              fullAOType = "������";
              return true;
            default:
              return false;
          }
        case FiasLevel.Structure: // ��������
          FiasStructureStatus StrStatus = FiasEnumNames.ParseStructureStatus(aoType);
          if (StrStatus == FiasStructureStatus.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(StrStatus);
            return true;
          }

        case FiasLevel.Flat:
          FiasFlatType FlatType = FiasEnumNames.ParseFlatType(aoType);
          if (FlatType == FiasFlatType.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(FlatType);
            return true;
          }

        case FiasLevel.Room:
          FiasRoomType RoomType = FiasEnumNames.ParseRoomType(aoType);
          if (RoomType == FiasRoomType.Unknown)
            return false;
          else
          {
            fullAOType = FiasEnumNames.ToString(RoomType);
            return true;
          }

        // �������������� ����������:
        case FiasLevel.Village:
          switch (aoType.ToLowerInvariant())
          {
            case "��":
            case "�/�":
            case "�.�.":
              fullAOType = "������� �������";
              return true;
          }
          break;
        case FiasLevel.PlanningStructure:
        case FiasLevel.Street:
          switch (aoType.ToLowerInvariant())
          {
            case "�-�": // 03.11.2020
              fullAOType = "����������";
              return true;
          }
          break;
      }

      // ����������� ��������
      id = FindAOTypeId(level, aoType);

      if (id != 0)
        fullAOType = GetAOType(id, FiasAOTypeMode.Full);
      return id != 0;
    }

    /// <summary>
    /// ���������� ����������, ��������������� ��������� ���� ����������������� ��������
    /// </summary>
    /// <param name="AOType">��� ����������������� ��������</param>
    /// <param name="level">�������</param>
    /// <returns>����������</returns>
    public string GetAbbreviation(string AOType, FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House: // 01.09.2021
          FiasEstateStatus estStatus = FiasEnumNames.ParseEstateStatus(AOType);
          if (estStatus != FiasEstateStatus.Unknown)
            return FiasEnumNames.EstateStatusAbbrs[(int)estStatus];
          else
            break; // ����� ���� � �����������
        case FiasLevel.Building:
          if (AOType == "������")
            return "����.";
          else
            return AOType;
        case FiasLevel.Structure:
          FiasStructureStatus strStatus = FiasEnumNames.ParseStructureStatus(AOType);
          if (strStatus == FiasStructureStatus.Unknown)
            return AOType;
          else
            return FiasEnumNames.StructureStatusAbbrs[(int)strStatus];
        case FiasLevel.Flat:
          FiasFlatType flatType = FiasEnumNames.ParseFlatType(AOType);
          if (flatType == FiasFlatType.Unknown)
            return AOType;
          else
            return FiasEnumNames.FlatTypeAbbrs[(int)flatType];
        case FiasLevel.Room:
          FiasRoomType roomType = FiasEnumNames.ParseRoomType(AOType);
          if (roomType == FiasRoomType.Unknown)
            return AOType;
          else
            return FiasEnumNames.RoomTypeAbbrs[(int)roomType];
      }

      // ����� ������. ���������� ����������
      Int32 id = FindAOTypeId(level, AOType);
      if (id >= 0)
        return GetAOType(id, FiasAOTypeMode.Abbreviation);
      else
        return AOType; // ���� �� �����, ��������� ��� ����������
    }

    #endregion

    #region GetAOTypeLevels()

    /// <summary>
    /// ���������� ������, ��� ������� �������� �������� ��� ����������������� ��������.
    /// ����������� � ���������� � ������ ������������.
    /// ���� ������ <paramref name="aoType"/> �� �������� �����-���� �����, ������������ Empty
    /// </summary>
    /// <param name="aoType">��� ����������������� ��������</param>
    /// <returns>������, ��� ������� �������� ������ ���</returns>
    public FiasLevelSet GetAOTypeLevels(string aoType)
    {
      return GetAOTypeLevels(aoType, FiasAOTypeMode.Full) | GetAOTypeLevels(aoType, FiasAOTypeMode.Abbreviation);
    }

    /// <summary>
    /// ���������� ������, ��� ������� �������� �������� ��� ����������������� ��������.
    /// ����������� � ���������� � ������ ������������.
    /// ���� ������ <paramref name="aoType"/> �� �������� �����-���� �����, ������������ Empty
    /// </summary>
    /// <param name="aoType">��� ����������������� ��������. ������� �������� ������������</param>
    /// <param name="typeMode">���: ������ ������������ ��� ����������</param>
    /// <returns>������, ��� ������� �������� ������ ���</returns>
    public FiasLevelSet GetAOTypeLevels(string aoType, FiasAOTypeMode typeMode)
    {
      if (String.IsNullOrEmpty(aoType))
        return FiasLevelSet.Empty;

      // ��������� �������� ���������
      TypedStringDictionary<FiasLevelSet> dict = GetAOTypeLevelDict(typeMode);
      FiasLevelSet res;
      if (dict.TryGetValue(aoType, out res))
        return res;
      else
        return FiasLevelSet.Empty;
    }

    private volatile TypedStringDictionary<FiasLevelSet> _FullAOTypeLevels;

    private volatile TypedStringDictionary<FiasLevelSet> _AbbrAOTypeLevels;

    private TypedStringDictionary<FiasLevelSet> GetAOTypeLevelDict(FiasAOTypeMode typeMode)
    {
      switch (typeMode)
      {
        case FiasAOTypeMode.Full:
          if (_FullAOTypeLevels == null)
          {
            lock (_DS)
            {
              if (_FullAOTypeLevels == null)
                _FullAOTypeLevels = CreateAOTypeLevelDict(typeMode);
            }
          }
          return _FullAOTypeLevels;

        case FiasAOTypeMode.Abbreviation:
          if (_AbbrAOTypeLevels == null)
          {
            lock (_DS)
            {
              if (_AbbrAOTypeLevels == null)
                _AbbrAOTypeLevels = CreateAOTypeLevelDict(typeMode);
            }
          }
          return _AbbrAOTypeLevels;
        default:
          throw new ArgumentOutOfRangeException("typeMode");
      }
    }

    private TypedStringDictionary<FiasLevelSet> CreateAOTypeLevelDict(FiasAOTypeMode typeMode)
    {
      TypedStringDictionary<FiasLevelSet> dict = new TypedStringDictionary<FiasLevelSet>(true);

      #region ������ �� �����������

      foreach (DataRow row in _DS.Tables[0].Rows)
      {
        FiasLevel level = (FiasLevel)DataTools.GetInt(row, "Level");
        string s = DataTools.GetString(row, typeMode == FiasAOTypeMode.Full ? "SOCRNAME" : "SCNAME");

        FiasLevelSet ls = FiasLevelSet.FromLevel(level);
        AddToAOTypeLevelDict(dict, s, ls);
      }

      #endregion

      #region ����������� �������

      #region ���������� �����

      if (typeMode == FiasAOTypeMode.Abbreviation)
      {
        AddToAOTypeLevelDict(dict, "��", FiasLevelSet.FromLevel(FiasLevel.Village));
        AddToAOTypeLevelDict(dict, "�/�", FiasLevelSet.FromLevel(FiasLevel.Village));
        AddToAOTypeLevelDict(dict, "�.�.", FiasLevelSet.FromLevel(FiasLevel.Village));
      }

      #endregion

      #region ������������� ���������

      if (typeMode == FiasAOTypeMode.Abbreviation)
      {
        AddToAOTypeLevelDict(dict, "�-�", FiasLevelSet.FromLevel(FiasLevel.PlanningStructure));
      }

      #endregion

      #region �����

      if (typeMode == FiasAOTypeMode.Abbreviation)
      {
        AddToAOTypeLevelDict(dict, "�-�", FiasLevelSet.FromLevel(FiasLevel.Street));
      }

      #endregion

      #region ���

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.EstateStatusAOTypes.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.EstateStatusAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.House));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.EstateStatusAbbrs.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.EstateStatusAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.House));

        AddToAOTypeLevelDict(dict, "�.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "���.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "��.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "���.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "����.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "�����.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "���.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "�-�", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "��", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "��.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "����.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "���.", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "��", FiasLevelSet.FromLevel(FiasLevel.House));
        AddToAOTypeLevelDict(dict, "���", FiasLevelSet.FromLevel(FiasLevel.House));
      }

      #endregion

      #region ������

      if (typeMode == FiasAOTypeMode.Full)
        AddToAOTypeLevelDict(dict, "������", FiasLevelSet.FromLevel(FiasLevel.Building));
      else
      {
        AddToAOTypeLevelDict(dict, "������", FiasLevelSet.FromLevel(FiasLevel.Building));
        AddToAOTypeLevelDict(dict, "����.", FiasLevelSet.FromLevel(FiasLevel.Building));
        AddToAOTypeLevelDict(dict, "�.", FiasLevelSet.FromLevel(FiasLevel.Building));
      }

      #endregion

      #region ��������

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.StructureStatusAOTypes.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.StructureStatusAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.Structure));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.StructureStatusAbbrs.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.StructureStatusAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "��.", FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "������.", FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "����.", FiasLevelSet.FromLevel(FiasLevel.Structure));
        AddToAOTypeLevelDict(dict, "���.", FiasLevelSet.FromLevel(FiasLevel.Structure));
      }

      #endregion

      #region ��������

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.FlatTypeAOTypes.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.FlatTypeAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.Flat));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.FlatTypeAbbrs.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.FlatTypeAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.Flat));
      }

      #endregion

      #region �������

      if (typeMode == FiasAOTypeMode.Full)
      {
        for (int i = 1; i < FiasEnumNames.RoomTypeAOTypes.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.RoomTypeAOTypes[i], FiasLevelSet.FromLevel(FiasLevel.Room));
      }
      else
      {
        for (int i = 1; i < FiasEnumNames.RoomTypeAbbrs.Length; i++) // [0] - "�� ����������"
          AddToAOTypeLevelDict(dict, FiasEnumNames.RoomTypeAbbrs[i], FiasLevelSet.FromLevel(FiasLevel.Flat));
      }

      #endregion

      #endregion

      return dict;
    }

    private static void AddToAOTypeLevelDict(TypedStringDictionary<FiasLevelSet> dict, string s, FiasLevelSet ls)
    {
      if (ls.IsEmpty)
        return;

      FiasLevelSet oldVal;
      if (dict.TryGetValue(s, out oldVal))
        dict[s] = oldVal | ls;
      else
        dict.Add(s, ls);
    }


    #endregion

    #region GetMaxSpaceCount()

    private volatile int[] _MaxSpaceCounts;

    /// <summary>
    /// ���������� ������������ ���������� �������� ���� ��������� ������� ��������� ������
    /// </summary>
    /// <param name="level">������� ��������� ������� �� ������ FiasTools.AllLevels</param>
    /// <returns></returns>
    public int GetMaxSpaceCount(FiasLevel level)
    {
      if (_MaxSpaceCounts == null)
      {
        lock (_DS)
        {
          if (_MaxSpaceCounts == null)
          {
            int[] a = new int[FiasTools.AllLevels.Length];
            TypedStringDictionary<FiasLevelSet> dict = GetAOTypeLevelDict(FiasAOTypeMode.Full);
            foreach (KeyValuePair<string, FiasLevelSet> pair in dict)
            {
              int n = 0;
              for (int j = 0; j < pair.Key.Length; j++)
              {
                if (pair.Key[j] == ' ')
                  n++;
              }
              if (n == 0)
                continue;

              foreach (FiasLevel lvl in pair.Value)
              {
                int pLvl = FiasTools.AllLevelIndexer.IndexOf(lvl);
                a[pLvl] = Math.Max(a[pLvl], n);
              }
            }

            _MaxSpaceCounts = a; // ���������� � ����� �����, ����� ��� ������
          }
        }
      }

      int pLevel = FiasTools.AllLevelIndexer.IndexOf(level);
      if (pLevel < 0)
        throw new ArgumentOutOfRangeException("level", level, "������� ������ ���� �� ������ FiasTools.AllLevels");

      return _MaxSpaceCounts[pLevel];
    }

    #endregion

    #region ����� ����� ����������

    // ����� �������� ���� SCNAME �� ������� AOType. 
    // ��������� ���������� ��� ����� � ��������� �� FiasEnumNames

    private static readonly StringArrayIndexer _AbbrWithDotIndexer = new StringArrayIndexer(new string[] { 
      "��", // �����
      "���", // �����
      "���", // �������� 
      "�",
      "�", // ������� ��� ���
      "����", // ������������
      "���", // ������
      "�/� ������",
      "�/� �����",
      "�/� ���",
      "�/� ��",
      "��",
      "���", // �����
      "���", // �������
      "��",
      "���", // ������
      "���", // �������
      "����",
      "���", // ���������
      "��", // �����
      "�", // ��������
      "�����", // ����������
      "������.���", // ����������� ����������
      "���������",
      "���",
      "��", // �������������
      "���", // ����������
      "�", // ������
      "���",
      "��", // ����
      "�", // ���������, �������
      "�. �/� ��", // ������� ��� ��������������� �������
      "�. ��", // ������� ��� ������� (������� �������)
      "���", // ��������
      "��", // ������
      "���",
      "��", // �������
      "�����",
      "����", // ������
      "���", // ���������
      "���",
      "�����", // �������
      "���.��",
      "���", // �������
      "�",
      "���", // �����
      "���", // �����
      "��", // �������
      "c���", // ����������
      "������",
      "��", // �������
      "���", // ��������
      "���", 
      "���", // �����
      "�", // ����
      "��",
      "��", // �������
      "�", // �����
      "�", // �����
      "�" // ����
     }, true);

    /// <summary>
    /// ���������� true, ���� ����� ���������� ������ ���� �����.
    /// ��������, ��� "��" ������������ true, � ��� "���" - false.
    /// ���� ���������� ��� ������������� �� �����, ������������ false.
    /// </summary>
    /// <param name="abbr">����������� ����������</param>
    /// <param name="level">������� ��������� ������� (� ������� ���������� �� �����������)</param>
    /// <returns>������������� ���������� �����</returns>
    internal bool IsAbbreviationDotRequired(string abbr, FiasLevel level)
    {
      if (String.IsNullOrEmpty(abbr))
        return false;
      if (abbr[abbr.Length - 1] == '.')
        return false;

      // ����� ���������� ������ ���� ����� � ��������
      // http://new.gramota.ru/spravka/buro/search-answer?s=238880 (������ � 238880)
      // ����� ��������� ���������� "���", "��", ... ����� �� �����
      // http://new.gramota.ru/spravka/buro/search-answer?s=261380 (������ 261380)


      abbr = abbr.Replace('_', ' '); // ������ � ���������������

      // �� ���� ��������� �������, ������ ���� ����������.
      // ��� ��� ����� ���� �� �����, ��� �����, ���������� ��, ��� ����� �����.
      return _AbbrWithDotIndexer.Contains(abbr);
    }

    #endregion
  }
}
