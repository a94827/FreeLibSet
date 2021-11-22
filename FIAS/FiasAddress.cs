// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  #region FiasAbbreviationPlace

  /// <summary>
  /// ��������� ���� ��������� ������� ������������ ������������
  /// </summary>
  [Serializable]
  public enum FiasAOTypePlace
  {
    /// <summary>
    /// ���������� �� �����
    /// </summary>
    None,

    /// <summary>
    /// ���������� ���� �� ������������ ("��. ����������")
    /// </summary>
    BeforeName,

    /// <summary>
    /// ���������� ���� ����� ������������ ("��������� �-�")
    /// </summary>
    AfterName
  }

  #endregion

  /// <summary>
  /// ������ ������. �������� �����, ����������� �� ����������, ������� ����� �������� �� �����������.
  /// ���� ����� �� �������� ����������������.
  /// </summary>
  [Serializable]
  public sealed class FiasAddress : ICloneable
  {
    #region �����������

    /// <summary>
    /// ������� ������ �����
    /// </summary>
    public FiasAddress()
    {
      _Items = new Dictionary<int, string>();
    }

    #endregion

    #region ���������� ���������

    #region ���������

    private const FiasLevel LevelUnknownGuid = (FiasLevel)301;
    private const FiasLevel LevelAnyAOGuid = (FiasLevel)302;
    private const FiasLevel LevelRegionCode = (FiasLevel)311;
    private const FiasLevel LevelPostalCode = (FiasLevel)312;
    private const FiasLevel LevelManualPostalCode = (FiasLevel)313; // ��� ���������������� �������� AutoPostalCode
    private const FiasLevel LevelFiasPostalCode = (FiasLevel)314;
    private const FiasLevel LevelOKATO = (FiasLevel)315;
    private const FiasLevel LevelOKTMO = (FiasLevel)316;
    private const FiasLevel LevelIFNSFL = (FiasLevel)317;
    private const FiasLevel LevelTerrIFNSFL = (FiasLevel)318;
    private const FiasLevel LevelIFNSUL = (FiasLevel)319;
    private const FiasLevel LevelTerrIFNSUL = (FiasLevel)320;
    private const FiasLevel LevelDivType = (FiasLevel)331;
    private const FiasLevel LevelActuality = (FiasLevel)332;
    private const FiasLevel LevelLive = (FiasLevel)333;
    private const FiasLevel LevelStartDate = (FiasLevel)341;
    private const FiasLevel LevelEndDate = (FiasLevel)342;

    #endregion

    #region FiasItemPart

    /// <summary>
    /// ����������, ����� ����� ���������� ������ ��������� ��������/������: ������������, ��� ��������� ������� ��� ������ ����������
    /// </summary>
    [Serializable]
    private enum FiasItemPart
    {
      /// <summary>
      /// ������� ����� ������ ("������")
      /// </summary>
      Name = 0,

      /// <summary>
      /// ������ ������������ ���� ��������� ������� ("�����")
      /// </summary>
      AOType = 1,

      /// <summary>
      /// GUID ��������� �������, ����, ��������
      /// </summary>
      Guid = 2,

      /// <summary>
      /// ���������� ������������� ������ � ������� AddrOb, House ��� Room
      /// </summary>
      RecId = 3,

      /// <summary>
      /// �������� ������, ����, ����
      /// </summary>
      Value = 4,
    }

    #endregion

    /// <summary>
    /// ����� �������� ��� ������, ����� ������ ��������� �� �������
    /// </summary>
    private readonly Dictionary<int, string> _Items;

    private string InternalGetString(FiasLevel level, FiasItemPart part)
    {
      int key = (int)level * 10 + (int)part;
      string s;
      if (_Items.TryGetValue(key, out s))
        return s;
      else
        return String.Empty;
    }

    private void InternalSetString(FiasLevel level, FiasItemPart part, string value)
    {
      int key = (int)level * 10 + (int)part;

      if (String.IsNullOrEmpty(value))
        _Items.Remove(key);
      else
        _Items[key] = value;
    }

    /// <summary>
    /// ���������� true, ���� ������ ������
    /// </summary>
    public bool IsEmpty { get { return _Items.Count == 0; } }

    /// <summary>
    /// ������� ������ ������.
    /// � ��� ����� ������������� AutoPostalCode=true.
    /// </summary>
    public void Clear()
    {
      _Items.Clear();
    }

    /// <summary>
    /// ������� ��� �������������� ����������, ����� ����������� ������, GUID'��
    /// ������� �������� ������, ��� �������, �����, �����, ���� ����.
    /// GUID'� �� ���������
    /// ������� �������� DivType, Actuality, Live, StartDate � EndDate
    /// 
    /// �������� AutoPostalCode �� ��������. ���� AutoPostalCode=false, �� �������� ������ ����� �� ���������.
    /// </summary>
    public void ClearAuxInfo()
    {
      RegionCode = String.Empty;
      if (AutoPostalCode)
        PostalCode = String.Empty;
      OKATO = String.Empty;
      OKTMO = String.Empty;
      IFNSUL = String.Empty;
      TerrIFNSUL = String.Empty;
      IFNSFL = String.Empty;
      TerrIFNSFL = String.Empty;

      DivType = FiasDivType.Unknown;
      Actuality = FiasActuality.Unknown;
      Live = null;
      StartDate = null;
      EndDate = null;
    }

    /// <summary>
    /// ������� ��� ������ ��� ������� �������� ��������, ����� ������ � ���������
    /// </summary>
    internal void ClearAOLevels()
    {
      AOGuid = Guid.Empty; // ������� "��������������" GUID
      AORecId = Guid.Empty;
      for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        ClearLevel(FiasTools.AOLevels[i]);
    }


    #endregion

    #region ������������ � ��� ����������������� ��������

    /// <summary>
    /// �������� ������������ ����������������� �������� (��� ����) ��� ������
    /// </summary>
    /// <param name="level">�������</param>
    ///<returns>������������</returns>
    public string GetName(FiasLevel level)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "������� ������ ���� � ������ FiasTools.AllLevels");
      return InternalGetString(level, FiasItemPart.Name);
    }

    /// <summary>
    /// ���������� ������������ ����������������� �������� (��� ����) ��� ������
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="value">������������</param>
    public void SetName(FiasLevel level, string value)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "������� ������ ���� � ������ FiasTools.AllLevels");
      InternalSetString(level, FiasItemPart.Name, value);
    }

    /// <summary>
    /// �������� ��� ����������������� �������� ��� ������.
    /// ������������ ������ ������������ ����, �� ���� "�����", � �� "��.".
    /// </summary>
    /// <param name="level">�������</param>
    ///<returns>��� ��������� ������� </returns>
    public string GetAOType(FiasLevel level)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "������� ������ ���� � ������ FiasTools.AllLevels");
      return InternalGetString(level, FiasItemPart.AOType);
    }

    /// <summary>
    /// ���������� ��� ����������������� �������� ��� ������.
    /// ������������ ������ ������������ ����, �� ���� "�����", � �� "��.".
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="value">��� ��������� ������� </param>
    public void SetAOType(FiasLevel level, string value)
    {
      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "������� ������ ���� � ������ FiasTools.AllLevels");
      InternalSetString(level, FiasItemPart.AOType, value);
    }

    /// <summary>
    /// ������������� ��� ����������������� �������� �� ���������, ��������� FiasTools.GetDefaultAOType().
    /// ���� ��� ������ �� ��������� ��� �� ���������, ��������������� ������ ������.
    /// ������� ������������ �������� �� �����������.
    /// </summary>
    /// <param name="level">�������</param>
    public void SetDefaultAOType(FiasLevel level)
    {
      SetAOType(level, FiasTools.GetDefaultAOType(level));
    }

    /// <summary>
    /// ������ � ����������� ������ � ������ "��� ���� ��� ����������������� ��������" ������ � ����� �������, �� ���� "������ �����".
    /// ��� �������� ������ �� ������� ������������. ��� � ��� ������� �������� �� �����������.
    /// ��� ���������� ������ ������� ������������ ����� FiasHandler.Format(), ������� ������������ ���������� ������� ����� � ����, 
    /// � ���������� ����������.
    /// </summary>
    /// <param name="level">�������</param>
    /// <returns>����� ������</returns>
    public string this[FiasLevel level]
    {
      get
      {
        if (level == FiasLevel.Unknown)
          return String.Empty; // 29.04.2020

        string s1 = GetName(level);
        string s2 = GetAOType(level);
        if (s1.Length == 0)
          return String.Empty;
        else if (s2.Length == 0)
          return s1;
        else
          return s1 + " " + s2;
      }
      set
      {
        int p = value.LastIndexOf(' ');
        if (p >= 0 && FiasTools.AllLevelIndexer.Contains(level))
        {
          string s1 = value.Substring(0, p);
          string s2 = value.Substring(p + 1);
          SetName(level, s1);
          SetAOType(level, s2);
        }
        else
        {
          SetName(level, value);
          SetAOType(level, String.Empty);
        }
      }
    }

    /// <summary>
    /// ������� ���� �������: ������������, ����������, GUID � RecId.
    /// ������ ������ �� ��������.
    /// ������ House/Building/Structure � Flat/Room �����������.
    /// </summary>
    /// <param name="level">��������� �������</param>
    public void ClearLevel(FiasLevel level)
    {
      InternalSetString(level, FiasItemPart.Name, String.Empty);
      InternalSetString(level, FiasItemPart.AOType, String.Empty);
      InternalSetString(level, FiasItemPart.Guid, String.Empty);
      InternalSetString(level, FiasItemPart.RecId, String.Empty);
    }


    /// <summary>
    /// ������� ��� ������, ������� ��������� ���� ���������.
    /// ������� <paramref name="level"/> � ����������� �� ��������.
    /// ���� <paramref name="level"/>=Unknown, ��������� ��� ������.
    /// ������ House/Building/Structure � Flat/Room �����������.
    /// </summary>
    /// <param name="level">��������� ����������� �������</param>
    public void ClearBelow(FiasLevel level)
    {
      int p = FiasTools.AllLevelIndexer.IndexOf(level);

      for (int i = p + 1; i < FiasTools.AllLevels.Length; i++)
        ClearLevel(FiasTools.AllLevels[i]);
    }

    /// <summary>
    /// ������� ��� ������, ������� � ���������, � ����.
    /// ������� ���� <paramref name="level"/> �� ��������.
    /// ���� <paramref name="level"/>=Unknown, ������������� ����������.
    /// ������ House/Building/Structure � Flat/Room �����������.
    /// </summary>
    /// <param name="level">������ ��������� �������</param>
    public void ClearStartingWith(FiasLevel level)
    {
      int p = FiasTools.AllLevelIndexer.IndexOf(level);
      if (p < 0)
        throw new ArgumentException();

      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        ClearLevel(FiasTools.AllLevels[i]);
    }

    /// <summary>
    /// ���������� ��������� �������, ��� �������� ���� ����������� ������������.
    /// ���� �� ������� �� ��������, ���������� FiasLevel.Unknown.
    /// ������ House/Building/Structure � Flat/Room �����������.
    /// </summary>
    public FiasLevel NameBottomLevel
    {
      get
      {
        if (IsEmpty)
          return FiasLevel.Unknown;
        for (int i = FiasTools.AllLevels.Length - 1; i >= 0; i--)
        {
          if (GetName(FiasTools.AllLevels[i]).Length > 0)
            return FiasTools.AllLevels[i];
        }

        return FiasLevel.Unknown;
      }
    }


    /// <summary>
    /// ���������� ���������� ����������� ������� ������, ������������� ���� ���������.
    /// ���� ���� ��������� ������ ��� ����������� �������, ������������ Unknown.
    /// </summary>
    /// <param name="level">�������, ���� �������� ����������� ��������</param>
    /// <returns>���������� ����������� �������</returns>
    public FiasLevel GetPrevLevel(FiasLevel level)
    {
      level = CorrectLevel(level);

      int p = FiasTools.AOLevelIndexer.IndexOf(level);
      if (p < 0)
        return FiasLevel.Unknown;
      for (int i = p - 1; i >= 0; i--)
      {
        FiasLevel lvl2 = FiasTools.AllLevels[i];
        if (GetName(lvl2).Length > 0)
        {
          switch (lvl2)
          {
            case FiasLevel.Building:
            case FiasLevel.Structure:
              return FiasLevel.House;
            default:
              return lvl2;
          }
        }
      }
      return FiasLevel.Unknown;
    }

    /// <summary>
    /// ���������� true, ���� ��������� ������������ ��������� ������� ������-���� ������ �� ������� �� ����� ������������
    /// </summary>
    public bool ContainsAddrObName
    {
      get
      {
        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          if (GetName(FiasTools.AOLevels[i]).Length > 0)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// ���������� true, ���� ����� ����� ����, ������� ��� ��������
    /// </summary>
    public bool ContainsHouseNum
    {
      get
      {
        return GetName(FiasLevel.House).Length > 0 ||
          GetName(FiasLevel.Building).Length > 0 ||
          GetName(FiasLevel.Structure).Length > 0;
      }
    }

    /// <summary>
    /// ���������� true, ���� ����� ����� �������� ��� ���������
    /// </summary>
    public bool ContainsRoomNum
    {
      get
      {
        return GetName(FiasLevel.Flat).Length > 0 ||
          GetName(FiasLevel.Room).Length > 0;
      }
    }

    /// <summary>
    /// ���������� ������ �������, ��� ������� ��������� ��������.
    /// ������ House/Buiding/Structure � Flat/Room �����������
    /// </summary>
    public FiasLevelSet NameLevels
    {
      get
      {
        int _IntValue = 0;
        for (int i = 0; i < FiasTools.AllLevels.Length; i++)
        {
          if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Name).Length > 0)
            _IntValue |= (1 << i);
        }
        return new FiasLevelSet(_IntValue);
      }
    }

    /// <summary>
    /// ��������� ����������� ������������ � ����� ���������������� ��������� �� �������� ������ � ������.
    /// GUID�, �������������� �������, �������� ������, ��������� �� ������� �� ����������.
    /// </summary>
    /// <param name="dest">����������� �������� ������</param>
    /// <param name="levels">���������� ������. 
    /// ���� � ������� ������ ��� ������ �� ������ ��� ��������, �� ������� ���������.
    /// � ���������� ������ ������, �� ������������� � ������, �� ����������</param>
    public void CopyNamesTo(FiasAddress dest, FiasLevelSet levels)
    { 
#if DEBUG
      if (dest == null)
        throw new ArgumentNullException("dest");
#endif

      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        if (levels[FiasTools.AllLevels[i]])
        {
          dest.SetName(FiasTools.AllLevels[i], this.GetName(FiasTools.AllLevels[i]));
          dest.SetAOType(FiasTools.AllLevels[i], this.GetAOType(FiasTools.AllLevels[i]));
        }
      }
    }

    #endregion

    #region ������� �������� ����������� ������
#if XXX
    /// <summary>
    /// ������������ ������� ("��������� ���")
    /// </summary>    
    public string Region { get { return this[FiasLevel.Region]; } set { this[FiasLevel.Region] = value; } }

    /// <summary>
    /// ���������� ����� (����������)
    /// </summary>
    public string AutonomousArea { get { return this[FiasLevel.AutonomousArea]; } set { this[FiasLevel.AutonomousArea] = value; } }

    /// <summary>
    /// ����� ("��������� �-�")
    /// </summary>
    public string District { get { return this[FiasLevel.District]; } set { this[FiasLevel.District] = value; } }

    /// <summary>
    /// ���������/�������� ���������
    /// </summary>
    public string Settlement { get { return this[FiasLevel.Settlement]; } set { this[FiasLevel.Settlement] = value; } }

    /// <summary>
    /// �����
    /// </summary>
    public string City { get { return this[FiasLevel.City]; } set { this[FiasLevel.City] = value; } }

    /// <summary>
    /// ��������������� ���������� (����������)
    /// </summary>
    public string InnerCityArea { get { return this[FiasLevel.InnerCityArea]; } set { this[FiasLevel.InnerCityArea] = value; } }

    /// <summary>
    /// ���������� �����
    /// </summary>
    public string Village { get { return this[FiasLevel.Village]; } set { this[FiasLevel.Village] = value; } }

    /// <summary>
    /// ������������� ���������
    /// </summary>
    public string PlanningStructure { get { return this[FiasLevel.PlanningStructure]; } set { this[FiasLevel.PlanningStructure] = value; } }

    /// <summary>
    /// �����
    /// </summary>
    public string Street { get { return this[FiasLevel.Street]; } set { this[FiasLevel.Street] = value; } }

    /// <summary>
    /// ��������� �������
    /// </summary>
    public string LandPlot { get { return this[FiasLevel.LandPlot]; } set { this[FiasLevel.LandPlot] = value; } }

    /// <summary>
    /// ������, ����������, ������ �������������� �������������
    /// </summary>
    public string House { get { return this[FiasLevel.House]; } set { this[FiasLevel.House] = value; } }

    /// <summary>
    /// ��������� � �������� ������, ����������
    /// </summary>
    public string Flat { get { return this[FiasLevel.Flat]; } set { this[FiasLevel.Flat] = value; } }

    /// <summary>
    /// �������������� ���������� (����������)
    /// </summary>
    public string AdditionalTerritory { get { return this[FiasLevel.AdditionalTerritory]; } set { this[FiasLevel.AdditionalTerritory] = value; } }

    /// <summary>
    /// ������ �� �������������� ���������� (����������)
    /// </summary>
    public string AdditionalTerritoryObject { get { return this[FiasLevel.AdditionalTerritoryObject]; } set { this[FiasLevel.AdditionalTerritoryObject] = value; } }

    /// <summary>
    /// ������
    /// </summary>
    public string Buiding { get { return this[FiasLevel.Building]; } set { this[FiasLevel.Building] = value; } }

    /// <summary>
    /// ��������
    /// </summary>
    public string Structure { get { return this[FiasLevel.Structure]; } set { this[FiasLevel.Structure] = value; } }

    /// <summary>
    /// �������
    /// </summary>
    public string Room { get { return this[FiasLevel.Room]; } set { this[FiasLevel.Room] = value; } }
#endif
    #endregion

    #region Guid'�

    private void InternalSetGuid(FiasLevel level, FiasItemPart part, Guid value)
    {
      if (value == Guid.Empty)
        InternalSetString(level, part, String.Empty);
      else
        InternalSetString(level, part, value.ToString());
    }

    private Guid InternalGetGuid(FiasLevel level, FiasItemPart part)
    {
      string s = InternalGetString(level, part);
      if (s.Length == 0)
        return Guid.Empty;
      else
        return new Guid(s);
    }

    /// <summary>
    /// ��� ������ � ��������� ���� ��������� ������� ��� ������������, �� GUID� � ��� �����.
    /// ��� ������ ������������ FiasLevel.House, � ��� ��������� - FiasLevel.Flat
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private static FiasLevel CorrectLevel(FiasLevel level)
    {
      if (level == FiasLevel.Unknown)
        return FiasLevel.Unknown;

      if (!FiasTools.AllLevelIndexer.Contains(level))
        throw new ArgumentOutOfRangeException("level", level, "������� ������ ���� � ������ FiasTools.AllLevels");
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          return FiasLevel.House;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          return FiasLevel.Flat;
        default:
          return level;
      }
    }

    /// <summary>
    /// ��������� GUID� ��� ������ ��������� �������, ���� ��� ��������.
    /// ���������� Guid.Empty, ���� ��������������� GUID �� �����
    /// </summary>
    /// <param name="level">�������</param>
    /// <returns>GUID</returns>
    public Guid GetGuid(FiasLevel level)
    {
      return InternalGetGuid(CorrectLevel(level), FiasItemPart.Guid);
    }

    /// <summary>
    /// ��������� GUID� ��� ������ ��������� �������, ���� ��� ��������.
    /// ������� ���������, ��� ��� ������� <paramref name="level"/>=House, Building � Construction
    /// ��������� ������������ GUID, ��� � ��� ������� Flat � Room. ��������� ��� ������ ������ ���������� ��� ���� �������.
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="value">GUID</param>
    public void SetGuid(FiasLevel level, Guid value)
    {
      InternalSetGuid(CorrectLevel(level), FiasItemPart.Guid, value);
    }

    /// <summary>
    /// GUID ��������� �������.
    /// ���������� GUID ������� ������ ���������� ������ (������, �����).
    /// ��������� �������� ������ GUID, �� ����������� � ����������� ������
    /// </summary>
    public Guid AOGuid
    {
      get
      {
        Guid g = InternalGetGuid(LevelAnyAOGuid, FiasItemPart.Guid);
        if (g != Guid.Empty)
          return g;

        for (int i = FiasTools.AOLevels.Length - 1; i >= 0; i--)
        {
          g = InternalGetGuid(FiasTools.AOLevels[i], FiasItemPart.Guid);
          if (g != Guid.Empty)
            return g;
        }
        return Guid.Empty;
      }
      set
      {
        InternalSetGuid(LevelAnyAOGuid, FiasItemPart.Guid, value);
      }
    }
    /// <summary>
    /// GUID ��������� �������, ���� ��� ��������.
    /// ���� ����� �� ���������� ��������������� �������� �������� ������������� ��������,
    /// ����������� ����� �� "������������" ��������������� AORecId, HouseRecId � RoomRecId.
    /// </summary>
    public Guid UnknownGuid
    {
      get
      {
        return InternalGetGuid(LevelUnknownGuid, FiasItemPart.Guid);
      }
      set
      {
        InternalSetGuid(LevelUnknownGuid, FiasItemPart.Guid, value);
      }
    }

    /// <summary>
    /// GUID ������.
    /// ������������ ������ ������� GetGuid()/SetGuid ��� FiasLevel.House
    /// </summary>
    public Guid HouseGuid
    {
      get { return GetGuid(FiasLevel.House); }
      set { SetGuid(FiasLevel.House, value); }
    }

    /// <summary>
    /// GUID ���������.
    /// ������������ ������ ������� GetGuid()/SetGuid ��� FiasLevel.Room
    /// </summary>
    public Guid RoomGuid
    {
      get { return GetGuid(FiasLevel.Room); }
      set { SetGuid(FiasLevel.Room, value); }
    }

    /// <summary>
    /// ���������� ����� ��������� ��������� ������, ��� �������� ����� GUID
    /// (���������, ���, �����, ...).
    /// AOGuid � UnknownGuid �� ���������.
    /// ���� �� ������ GUID'� �� ����������, ������������ FiasLevel.Unknown
    /// ���� ����� Guid ��� ����, �� ������������ FiasLevel.House, � �� FiasLevel.Structure, �������� �� ��,
    /// ��� GetGuid(FiasLevel.Structure) ����� ���������� ��������.
    /// ����������, ��� ��������� ������������ �������� FiasLevel.Flat, � �� Room
    /// </summary>
    public FiasLevel GuidBottomLevel
    {
      get
      {
        if (InternalGetString(FiasLevel.Flat, FiasItemPart.Guid).Length > 0)
          return FiasLevel.Flat;
        if (InternalGetString(FiasLevel.House, FiasItemPart.Guid).Length > 0)
          return FiasLevel.House;
        for (int i = FiasTools.AOLevels.Length - 1; i >= 0; i--)
        {
          if (InternalGetString(FiasTools.AOLevels[i], FiasItemPart.Guid).Length > 0)
            return FiasTools.AOLevels[i];
        }
        return FiasLevel.Unknown;
      }
    }

    /// <summary>
    /// ������� ��� GUID�, ������� UnknownGuid � AOGuid
    /// </summary>
    public void ClearGuids()
    {
      ClearGuidsStartingWith(FiasLevel.Region);
    }

    /// <summary>
    /// ������� GUID�, ������� � ��������� ������ � �� ������ Flat ������������.
    /// ����� ������� UnknownGuid � AOGuid
    /// </summary>
    /// <param name="level">����� ������� �������, ������� ����� ��������. ���� FiasLevel.Region,
    /// �� ����� ������� ��� Guid�</param>
    public void ClearGuidsStartingWith(FiasLevel level)
    {
      int p;
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Flat);
          break;
        default:
          p = FiasTools.AllLevelIndexer.IndexOf(level);
          if (p < 0)
            throw new ArgumentException("level=" + level.ToString(), "level");
          break;
      }

      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        SetGuid(FiasTools.AllLevels[i], Guid.Empty);
      UnknownGuid = Guid.Empty;
      AOGuid = Guid.Empty;
    }

    /// <summary>
    /// ������� GUID�, ������� � ������, ���� ���������, � �� ������ Flat ������������.
    /// ����� ������� UnknownGuid � AOGuid
    /// </summary>
    /// <param name="level">����� ������ �������, ������� �� ����� �������. ���� ����� Guid.Unknown, ����� ������� ��� ������</param>
    public void ClearGuidsBelow(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          ClearGuidsStartingWith(FiasLevel.Flat);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          int p = FiasTools.AllLevelIndexer.IndexOf(level);
          ClearGuidsStartingWith(FiasTools.AllLevels[p + 1]);
          break;
      }
    }

    /// <summary>
    /// ���������� ���������� ������������� �������, ���� ���� ��������, ������� AOGuid � UnknownGuid
    /// </summary>
    public Guid AnyGuid
    {
      get
      {
        Guid g;
        g = UnknownGuid;
        if (g != Guid.Empty)
          return g;
        g = GetGuid(FiasLevel.Room);
        if (g != Guid.Empty)
          return g;
        g = GetGuid(FiasLevel.House);
        if (g != Guid.Empty)
          return g;
        g = AOGuid;
        if (g != Guid.Empty)
          return g;
        return Guid.Empty;
      }
    }

    /// <summary>
    /// ���������� ������ �������, ��� ������� �������� ���������� ������������� ������.
    /// ������ House/Buiding/Structure � Flat/Room ����������� � ��� ������, ��� ��� ��� ������ ���������������,
    /// ������ ���� ��������� ��������������� ������� �����.
    /// � ������������� ���������, �� ������ FillAddress(), ����, ��������, ���������� HouseGuid, �� ����� �� ���������,
    /// ��������������� ��� ������ House, Building � Structure.
    /// ��������������, �� ����������� � ������ (UnknownGuid, AOGuid) �� �����������.
    /// </summary>
    public FiasLevelSet GuidLevels
    {
      get
      {
        bool HasHouseGuid = false;
        bool HasHouseName = false;
        bool HasRoomGuid = false;
        bool HasRoomName = false;

        int _IntValue = 0;
        for (int i = 0; i < FiasTools.AllLevels.Length; i++)
        {
          switch (FiasTools.AllLevels[i])
          {
            case FiasLevel.House:
            case FiasLevel.Building:
            case FiasLevel.Structure:
              if (InternalGetString(FiasLevel.House, FiasItemPart.Guid).Length > 0)
              {
                HasHouseGuid = true;
                if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Name).Length > 0)
                {
                  _IntValue |= (1 << i);
                  HasHouseName = true;
                }
              }
              break;

            case FiasLevel.Flat:
            case FiasLevel.Room:
              if (InternalGetString(FiasLevel.Flat, FiasItemPart.Guid).Length > 0)
              {
                HasRoomGuid = true;
                if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Name).Length > 0)
                {
                  _IntValue |= (1 << i);
                  HasRoomName = true;
                }
              }
              break;
            default:
              if (InternalGetString(FiasTools.AllLevels[i], FiasItemPart.Guid).Length > 0)
                _IntValue |= (1 << i);
              break;
          }
        }

        FiasLevelSet res = new FiasLevelSet(_IntValue);

        if (HasHouseGuid && (!HasHouseName))
          res |= FiasLevelSet.HouseLevels;
        if (HasRoomGuid && (!HasRoomName))
          res |= FiasLevelSet.RoomLevels;

        return res;
      }
    }

    #endregion

    #region �������������� �������

    /// <summary>
    /// ��������� �������������� ������ ��� ������ ��������� ������� (AOID), ���� (HOUSEID) ��� �������� (ROOMID).
    /// ���������� Guid.Empty, ���� ��������������� GUID �� �����.
    /// � ���������� ���� ������ ������������ "����������" GUID� �������� ��������, � �� �������������� �������.
    /// </summary>
    /// <param name="level">�������</param>
    /// <returns>�������������� ������</returns>
    public Guid GetRecId(FiasLevel level)
    {
      return InternalGetGuid(CorrectLevel(level), FiasItemPart.RecId);
    }

    /// <summary>
    /// ��������� �������������� ������ ��� ������ ��������� �������, ���� ��� ��������.
    /// ������� ���������, ��� ��� ������� <paramref name="level"/>=House, Building � Construction
    /// ��������� ������������ �������������, ��� � ��� ������� Flat � Room. ��������� ��� ������ ������ ���������� ��� ���� �������.
    /// � ���������� ���� ������ ������������ "����������" GUID� �������� ��������, � �� �������������� �������.
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="value">������������ ������ ����</param>
    public void SetRecId(FiasLevel level, Guid value)
    {
      InternalSetGuid(CorrectLevel(level), FiasItemPart.RecId, value);
    }

    /// <summary>
    /// ������������� ������ ��������� �������.
    /// ���������� ID ������� ������ ���������� ������ (������, �����).
    /// ��������� �������� ������ �������������, �� ����������� � ����������� ������
    /// </summary>
    public Guid AORecId
    {
      get
      {
        Guid g = InternalGetGuid(LevelAnyAOGuid, FiasItemPart.RecId);
        if (g != Guid.Empty)
          return g;

        for (int i = FiasTools.AOLevels.Length - 1; i >= 0; i--)
        {
          g = InternalGetGuid(FiasTools.AOLevels[i], FiasItemPart.RecId);
          if (g != Guid.Empty)
            return g;
        }
        return Guid.Empty;
      }
      set
      {
        InternalSetGuid(LevelAnyAOGuid, FiasItemPart.RecId, value);
      }
    }

    ///// <summary>
    ///// GUID ��������� �������, ���� ��� ��������.
    ///// </summary>
    //public Guid UnknownGuid
    //{
    //  get
    //  {
    //    string s = this[LevelUnknownGuid, FiasItemPart.Guid];
    //    if (s.Length == 0)
    //      return Guid.Empty;
    //    else
    //      return new Guid(s);
    //  }
    //  set
    //  {
    //    SetGuid(LevelUnknownGuid, value);
    //  }
    //}

    ///// <summary>
    ///// ���������� ����� ��������� ��������� ������, ��� �������� ����� GUID
    ///// (���������, ���, �����, ...).
    ///// AOGuid � UnknownGuid �� ���������.
    ///// ���� �� ������ GUID'� �� ����������, ������������ FiasLevel.Unknown
    ///// </summary>
    //public FiasLevel GuidBottomLevel
    //{
    //  get
    //  {
    //    for (int i = LevelWithGuidArray.Length - 1; i >= 0; i--)
    //    {
    //      if (this[LevelWithGuidArray[i], FiasItemPart.Guid].Length > 0)
    //        return LevelWithGuidArray[i];
    //    }
    //    return FiasLevel.Unknown;
    //  }
    //}

    /// <summary>
    /// ������� ��� �������������� ������.
    /// ����� ������� �������� AORecId.
    /// </summary>
    public void ClearRecIds()
    {
      ClearRecIdsStartingWith(FiasLevel.Region);
    }

    /// <summary>
    /// ������� �������������� �������, ������� � ��������� ������.
    /// ����� ������� �������� AORecId.
    /// </summary>
    /// <param name="level">�������, ������� � �������� ��������� ��������� �������.
    /// ���� ������ �������� Region, ����� ������� ��� ������</param>
    public void ClearRecIdsStartingWith(FiasLevel level)
    {
      int p;
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          p = FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Flat);
          break;
        default:
          p = FiasTools.AllLevelIndexer.IndexOf(level);
          if (p < 0)
            throw new ArgumentException("level=" + level.ToString(), "level");
          break;
      }

      for (int i = p; i < FiasTools.AllLevels.Length; i++)
        SetRecId(FiasTools.AllLevels[i], Guid.Empty);
      AORecId = Guid.Empty;
    }

    /// <summary>
    /// ������� �������������� �������, ������� � ��������� ������.
    /// ����� ������� �������� AORecId.
    /// </summary>
    /// <param name="level">�������, ������� � �������� ��������� ��������� �������.
    /// ���� ������ �������� Region, ����� ������� ��� ������</param>
    public void ClearRecIdsBelow(FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
          ClearRecIdsStartingWith(FiasLevel.Flat);
          break;
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          int p = FiasTools.AllLevelIndexer.IndexOf(level);
          ClearRecIdsStartingWith(FiasTools.AllLevels[p + 1]);
          break;
      }
    }


    #endregion

    #region ����, ��������� � �������

    /// <summary>
    /// ��� �������
    /// </summary>
    public string RegionCode
    {
      get { return InternalGetString(LevelRegionCode, FiasItemPart.Value); }
      internal set { InternalSetString(LevelRegionCode, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// ���� true (�� ���������), �� �������� ������ ������� �� ����������� ����.
    /// ���� false, �� ������ ����� ������� � �� ����� ����������� ��� �������� ������
    /// </summary>
    public bool AutoPostalCode
    {
      get { return InternalGetInt(LevelManualPostalCode) == 0; }
      set { InternalSetInt(LevelManualPostalCode, value ? 0 : 1); }
    }

    /// <summary>
    /// �������� ������.
    /// ��������� �������� ����� ����� ������ ��� AutoPostalCode=false.
    /// </summary>
    public string PostalCode
    {
      get { return InternalGetString(LevelPostalCode, FiasItemPart.Value); }
      set { InternalSetString(LevelPostalCode, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// �������� ������, ������������ � �������������� ����.
    /// ��� �������� ����� ���������� �� PostalCode ��� AutoPostalCode=false
    /// </summary>
    public string FiasPostalCode
    {
      get { return InternalGetString(LevelFiasPostalCode, FiasItemPart.Value); }
      internal set
      {
        InternalSetString(LevelFiasPostalCode, FiasItemPart.Value, value);
        if (AutoPostalCode)
          InternalSetString(LevelPostalCode, FiasItemPart.Value, value);
      }
    }

    /// <summary>
    /// �����
    /// </summary>
    public string OKATO
    {
      get { return InternalGetString(LevelOKATO, FiasItemPart.Value); }
      internal set { InternalSetString(LevelOKATO, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// �����
    /// </summary>
    public string OKTMO
    {
      get { return InternalGetString(LevelOKTMO, FiasItemPart.Value); }
      internal set { InternalSetString(LevelOKTMO, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// ��� ���� ��
    /// </summary>
    public string IFNSFL
    {
      get { return InternalGetString(LevelIFNSFL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelIFNSFL, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// ��� ���������������� ������� ���� ��
    /// </summary>
    public string TerrIFNSFL
    {
      get { return InternalGetString(LevelTerrIFNSFL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelTerrIFNSFL, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// ��� ���� ��
    /// </summary>
    public string IFNSUL
    {
      get { return InternalGetString(LevelIFNSUL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelIFNSUL, FiasItemPart.Value, value); }
    }

    /// <summary>
    /// ��� ���������������� ������� ���� ��
    /// </summary>
    public string TerrIFNSUL
    {
      get { return InternalGetString(LevelTerrIFNSUL, FiasItemPart.Value); }
      internal set { InternalSetString(LevelTerrIFNSUL, FiasItemPart.Value, value); }
    }

    #endregion

    #region ������ �������� ������

    private void InternalSetInt(FiasLevel level, int value)
    {
      if (value == 0)
        InternalSetString(level, FiasItemPart.Value, String.Empty);
      else
        InternalSetString(level, FiasItemPart.Value, StdConvert.ToString(value));
    }

    private int InternalGetInt(FiasLevel level)
    {
      string s = InternalGetString(level, FiasItemPart.Value);
      if (s.Length == 0)
        return 0;
      else
        return StdConvert.ToInt32(s);
    }

    /// <summary>
    /// ��� ��������� �������
    /// </summary>
    public FiasDivType DivType
    {
      get
      {
        return (FiasDivType)InternalGetInt(LevelDivType);
      }
      internal set
      {
        InternalSetInt(LevelDivType, (int)value);
      }
    }

    /// <summary>
    /// ������������ ������.
    /// ������������ ����� �������� ������
    /// </summary>
    public FiasActuality Actuality
    {
      get
      {
        return (FiasActuality)InternalGetInt(LevelActuality);
      }
      internal set
      {
        InternalSetInt(LevelActuality, (int)value);
      }
    }

    private void InternalSetNullableBool(FiasLevel level, bool? value)
    {
      if (value.HasValue)
        InternalSetString(level, FiasItemPart.Value, value.Value ? "1" : "0");
      else
        InternalSetString(level, FiasItemPart.Value, String.Empty);
    }

    private bool? InternalGetNullableBool(FiasLevel level)
    {
      string s = InternalGetString(level, FiasItemPart.Value);
      if (s.Length == 0)
        return null;
      else
        return StdConvert.ToInt32(s) != 0;
    }

    /// <summary>
    /// True, ���� �������� ������ ������� ��� �����������
    /// </summary>
    public bool? Live
    {
      get { return InternalGetNullableBool(LevelLive); }
      internal set
      {
        InternalSetNullableBool(LevelLive, value);
      }
    }

    #endregion

    #region ���� �������� ������

    private void InternalSetNullableDate(FiasLevel level, DateTime? value)
    {
      if (value.HasValue)
        InternalSetString(level, FiasItemPart.Value, StdConvert.ToString(value.Value, false));
      else
        InternalSetString(level, FiasItemPart.Value, String.Empty);
    }

    private DateTime? InternalGetNullableDate(FiasLevel level)
    {
      string s = InternalGetString(level, FiasItemPart.Value);
      if (s.Length == 0)
        return null;
      else
        return StdConvert.ToDateTime(s, false);
    }

    /// <summary>
    /// ���� ������ �������� ������.
    /// ������������ ������ �������� ��� ������ �������������� ������ ���������� ������ (������������ GuidBottomLevel)
    /// �������� ���������� null, ���� FiasDBSettings.UseDates=false.
    /// </summary>
    public DateTime? StartDate
    {
      get { return InternalGetNullableDate(LevelStartDate); }
      internal set
      {
        InternalSetNullableDate(LevelStartDate, value);
      }
    }


    /// <summary>
    /// ���� ��������� �������� ������.
    /// ������������ ������ �������� ��� ������ �������������� ������ ���������� ������ (������������ GuidBottomLevel)
    /// �������� ���������� null, ���� FiasDBSettings.UseDates=false.
    /// </summary>
    public DateTime? EndDate
    {
      get { return InternalGetNullableDate(LevelEndDate); }
      internal set
      {
        InternalSetNullableDate(LevelEndDate, value);
      }
    }

    #endregion

    #region ������

    /// <summary>
    /// ������ ������ ���������.
    /// ������ ����������� ������� FillAddress().
    /// </summary>
    public ErrorMessageList Messages
    {
      get
      {
        if (_Messages == null)
          return ErrorMessageList.Empty;
        else
          return _Messages;
      }
    }
    private ErrorMessageList _Messages;

    internal void ClearMessages()
    {
      _Messages = null;
    }

    /// <summary>
    /// ��������� ��������� �� ������, �� ����������� � ����������� ������ ������
    /// </summary>
    /// <param name="kind">�����������</param>
    /// <param name="message">����� ���������</param>
    internal void AddMessage(ErrorMessageKind kind, string message)
    {
      AddMessage(kind, message, FiasLevel.Unknown);
    }

    /// <summary>
    /// ��������� ��������� �� ������, ����������� � ����������� ������ ������
    /// </summary>
    /// <param name="kind">�����������</param>
    /// <param name="message">����� ���������</param>
    /// <param name="level">������� ������</param>
    internal void AddMessage(ErrorMessageKind kind, string message, FiasLevel level)
    {
      if (_Messages == null)
        _Messages = new ErrorMessageList();
      if (level != FiasLevel.Unknown)
        message = "[" + FiasEnumNames.ToString(level, false) + "] " + message;
      _Messages.Add(new ErrorMessageItem(kind, message, String.Empty, level));
    }

    internal void AddHouseMessage(ErrorMessageKind kind, string message)
    {
      if (GetName(FiasLevel.House).Length > 0)
        AddMessage(kind, message, FiasLevel.House);
      else if (GetName(FiasLevel.Building).Length > 0)
        AddMessage(kind, message, FiasLevel.Building);
      else if (GetName(FiasLevel.Structure).Length > 0)
        AddMessage(kind, message, FiasLevel.Structure);
      else // ��� ���� ������
        AddMessage(kind, message, FiasLevel.House);
    }

    internal void AddRoomMessage(ErrorMessageKind kind, string message)
    {
      if (GetName(FiasLevel.Flat).Length > 0)
        AddMessage(kind, message, FiasLevel.Flat);
      else if (GetName(FiasLevel.Room).Length > 0)
        AddMessage(kind, message, FiasLevel.Room);
      else // ��� ���� ������
        AddMessage(kind, message, FiasLevel.Flat);
    }

    internal void AddMessages(ErrorMessageList messages)
    {
      if (messages.Count == 0)
        return;
      if (_Messages == null)
        _Messages = new ErrorMessageList();
      _Messages.Add(messages);
    }

    /// <summary>
    /// ���������� ������ ���������, ��������� � �������� ������� ��������������
    /// </summary>
    /// <param name="level">������� ������ ���� �� ������� FiasTools.AllLevels.
    /// ���� ������ �������� FiasLevel.Unknown, ������������ ���������, �� ����������� �� � ������ ������</param>
    /// <returns>��������� ��������� �� ������ Messages</returns>
    public ErrorMessageList GetMessages(FiasLevel level)
    {
      if (_Messages == null)
        return ErrorMessageList.Empty;

      ErrorMessageList list2 = null;
      for (int i = 0; i < _Messages.Count; i++)
      {
        FiasLevel thisLevel = FiasLevel.Unknown;
        if (_Messages[i].Tag is FiasLevel)
          thisLevel = (FiasLevel)(_Messages[i].Tag);
        if (thisLevel == level)
        {
          if (list2 == null)
            list2 = new ErrorMessageList();
          list2.Add(_Messages[i]);
        }
      }
      if (list2 == null)
        return ErrorMessageList.Empty;
      else
      {
        list2.SetReadOnly();
        return list2;
      }
    }


    /// <summary>
    /// ���������� ������ ���������, ��������� � ��������� �������� ��������������.
    /// ������ �������� ���������, �� ����������� � ������
    /// </summary>
    /// <param name="levels">������ ������ ���� �� ������� FiasTools.AllLevels.</param>
    /// <returns>��������� ��������� �� ������ Messages</returns>
    public ErrorMessageList GetMessages(FiasLevelSet levels)
    {
      if (_Messages == null)
        return ErrorMessageList.Empty;

      ErrorMessageList list2 = null;
      for (int i = 0; i < _Messages.Count; i++)
      {
        if (_Messages[i].Tag is FiasLevel)
        {
          FiasLevel thisLevel = (FiasLevel)(_Messages[i].Tag);
          if (levels[thisLevel])
          {
            if (list2 == null)
              list2 = new ErrorMessageList();
            list2.Add(_Messages[i]);
          }
        }
      }
      if (list2 == null)
        return ErrorMessageList.Empty;
      else
      {
        list2.SetReadOnly();
        return list2;
      }
    }

    /// <summary>
    /// ���������� ������� ��������� ������� � �������� ��������� ��������� �� ������ Messages.
    /// ��������� ��������� ����� ���� �� ��������� � ������������ ������. ��� ��� ������������ FiasLevel.Unknown
    /// </summary>
    /// <param name="item">���������</param>
    /// <returns>������� ��������� �������</returns>
    public FiasLevel GetMessageFiasLevel(ErrorMessageItem item)
    {
      if (item.Tag is FiasLevel)
        return (FiasLevel)(item.Tag);
      else
        return FiasLevel.Unknown;
    }                       

    #endregion

    #region ToString()

    /// <summary>
    /// ��������� ������������� ��� �������.
    /// �������� ������ ������
    /// ����������� ����� FiasHandler.GetText()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(PostalCode);
      AddPartToString(sb, FiasLevel.Region, FiasAOTypePlace.AfterName);
      AddPartToString(sb, FiasLevel.District, FiasAOTypePlace.AfterName);
      AddPartToString(sb, FiasLevel.City, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Village, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.PlanningStructure, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Street, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.House, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Building, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Structure, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Flat, FiasAOTypePlace.BeforeName);
      AddPartToString(sb, FiasLevel.Room, FiasAOTypePlace.BeforeName);

      if (_Messages != null)
      {
        for (int i = 0; i < _Messages.Count; i++)
        {
          sb.Append(Environment.NewLine);
          sb.Append(_Messages[i].ToString());
        }
      }

      return sb.ToString();
    }

    internal void AddPartToString(StringBuilder sb, FiasLevel level, FiasAOTypePlace place)
    {
      string s1 = GetName(level);
      string s2 = GetAOType(level);

      if (level == FiasLevel.Region)
      {
        if (s1.IndexOf(s2, StringComparison.OrdinalIgnoreCase) >= 0)
          s2 = String.Empty; // 05.10.2020
      }

      if (s1.Length == 0)
        return;
      if (sb.Length > 0)
        sb.Append(", ");

      if (place == FiasAOTypePlace.BeforeName && s2.Length > 0)
      {
        sb.Append(s2);
        sb.Append(" ");
      }
      sb.Append(s1);
      if (place == FiasAOTypePlace.AfterName && s2.Length > 0)
      {
        sb.Append(" ");
        sb.Append(s2);
      }
    }

    #endregion

    #region Clone()

    /// <summary>
    /// ������������ ������.
    /// ���������� ��� ���� ������ � ������ ���������.
    /// </summary>
    /// <returns>����� ������ ������</returns>
    public FiasAddress Clone()
    {
      FiasAddress a = new FiasAddress();
      foreach (KeyValuePair<int, string> pair in _Items)
        a._Items.Add(pair.Key, pair.Value);

      if (_Messages != null)
      {
        a._Messages = new ErrorMessageList();
        a._Messages.AddRange(_Messages);
      }
      return a;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region ������

    /// <summary>
    /// ���������� ������ ����������� ������� � ������� ������ � �������� ������� ���������.
    /// ���������� Equal, ���� �������� ������� ������������� ����������.
    /// ���������� Less, ���� ������� ����� �������� ��������������� ��� ������ ������.
    /// ���������� true, ���� ������� ����� �������� ������ ����������.
    /// </summary>
    /// <param name="editorLevel">������� ������ � ���������</param>
    /// <returns>��������� ���������</returns>
    public FiasLevelCompareResult CompareTo(FiasEditorLevel editorLevel)
    {
      string errorText;
      return CompareTo(editorLevel, out errorText);
    }

    /// <summary>
    /// ���������� ������ ����������� ������� � ������� ������ � �������� ������� ���������.
    /// ���������� Equal, ���� �������� ������� ������������� ����������.
    /// ���������� Less, ���� ������� ����� �������� ��������������� ��� ������ ������.
    /// ���������� true, ���� ������� ����� �������� ������ ����������.
    /// </summary>
    /// <param name="editorLevel">������� ������ � ���������</param>
    /// <param name="errorText">���� ���������� ����� ��������� �� ������</param>
    /// <returns>��������� ���������</returns>
    public FiasLevelCompareResult CompareTo(FiasEditorLevel editorLevel, out string errorText)
    {
      FiasLevel BL = this.NameBottomLevel;
      if (BL == FiasLevel.Unknown)
      {
        errorText = "����� �� ��������";
        return FiasLevelCompareResult.Less;
      }
      int pBL = FiasTools.AllLevelIndexer.IndexOf(BL);
#if DEBUG
      if (pBL < 0)
        throw new BugException("������������ NameBottomLevel=" + BL.ToString());
#endif

      switch (editorLevel)
      {
        case FiasEditorLevel.Village:
          // ������ ���� ����� ����� ��� ���������� �����
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.City))
          {
            errorText = "������ ���� �������� ����� ��� ���������� �����";
            return FiasLevelCompareResult.Less;
          }
          if (pBL > FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Village))
          {
            errorText = "�� ������ ����������� ������ ���� ����������� ������";
            return FiasLevelCompareResult.Greater;
          }

          errorText = null;
          return FiasLevelCompareResult.Equal;

        case FiasEditorLevel.Street:
          // ������ ���� ������ ����� ��� ���������� �����, �� �� � ������
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Village))
          {
            errorText = "������ ���� �������� ���������� ����� ��� �����";
            return FiasLevelCompareResult.Less;
          }
          if (pBL > FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Street))
          {
            errorText = "�� ������ ����������� ������ ���� �����";
            return FiasLevelCompareResult.Greater;
          }
          if (BL == FiasLevel.Village && GetName(FiasLevel.City).Length > 0)
          {
            errorText = "���� �������� ���������� ����� ��� �����, �� ���������� ����� �� ����� ������������� � ������";
            return FiasLevelCompareResult.Less;
          }
          errorText = null;
          return FiasLevelCompareResult.Equal;

        case FiasEditorLevel.House:
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House))
          {
            errorText = "������ ���� �������� ��� �/��� ��������";
            return FiasLevelCompareResult.Less;
          }
          if (pBL >= FiasTools.AllLevelIndexer.IndexOf(FiasLevel.Flat))
          {
            errorText = "�� ������ ����������� ������ ���� ����";
            return FiasLevelCompareResult.Greater;
          }
          errorText = null;
          return FiasLevelCompareResult.Equal;

        case FiasEditorLevel.Room:
          if (pBL < FiasTools.AllLevelIndexer.IndexOf(FiasLevel.House))
          {
            errorText = "������ ���� �������� ��� ��� ���������";
            return FiasLevelCompareResult.Less;
          }
          errorText = null;
          return FiasLevelCompareResult.Equal;
        default:
          throw new ArgumentException("���������� editorLevel=" + editorLevel.ToString(), "editorLevel");
      }
    }

    /// <summary>
    /// ��������� ��������, ��� ������� ������ ������ ������� �� �����������, � �� ������ �������.
    /// </summary>
    /// <param name="minRefBookLevel">�������, ���� � ������� �������, ������ ������������ ����������.
    /// ���� ������ �������� Unknown, �� �������� �� �����������</param>
    /// <returns>true, ���� �������� ���������</returns>
    public bool IsMinRefBookLevel(FiasLevel minRefBookLevel)
    {
      string errorText;
      return IsMinRefBookLevel(minRefBookLevel, out errorText);
    }

    /// <summary>
    /// ��������� ��������, ��� ������� ������ ������ ������� �� �����������, � �� ������ �������.
    /// </summary>
    /// <param name="minRefBookLevel">�������, ���� � ������� �������, ������ ������������ ����������.
    /// ���� ������ �������� Unknown, �� �������� �� �����������</param>
    /// <param name="errorText">���� ���������� ��������� �� ������</param>
    /// <returns>true, ���� �������� ���������</returns>
    public bool IsMinRefBookLevel(FiasLevel minRefBookLevel, out string errorText)
    {
      errorText = null;
      if (minRefBookLevel == FiasLevel.Unknown)
        return true;
      if (IsEmpty)
        return true;

#if XXX
      FiasLevel gl = this.GuidBottomLevel;
      FiasLevel nl = this.NameBottomLevel;

      if (nl == gl)
        return true; // ��� �����������, ��������� �������

      minRefBookLevel = CorrectLevel(minRefBookLevel);
      int pMin = FiasTools.AllLevelIndexer.IndexOf(minRefBookLevel);
      if (pMin < 0)
        throw new ArgumentException("����������� ������� " + minRefBookLevel.ToString(), "minRefBookLevel");
      int pNL = FiasTools.AllLevelIndexer.IndexOf(nl);
      if (pNL < pMin)
      {
        minRefBookLevel = nl; // ��� �������� ������������ ������ �� ���� �������� ������ ���������
        pMin = pNL;
      }

      if (gl == FiasLevel.Unknown)
      {
        errorText = "��� �������� �������� ������� �������. �������� �������� �� ������ \"" + FiasEnumNames.ToString(minRefBookLevel, false) + "\" ������ ���� ������� �� ����������� ����";
        return false;
      }

      int pGL = FiasTools.AllLevelIndexer.IndexOf(gl);
      if (pGL < pMin)
      {
        errorText = "�������� �������� �� ������ \"" + FiasEnumNames.ToString(minRefBookLevel, false) + "\" ������ ���� ������� �� ����������� ����. � ������ �� ����������� ������� ������ ������ �� \"" +
          FiasEnumNames.ToString(gl, false) + "\" ������������";
        return false;
      }

#endif

      minRefBookLevel = CorrectLevel(minRefBookLevel);
      int pMin = FiasTools.AllLevelIndexer.IndexOf(minRefBookLevel);
      if (pMin < 0)
        throw new ArgumentException("����������� ������� " + minRefBookLevel.ToString(), "minRefBookLevel");
      for (int i = 0; i <= pMin; i++)
      {
        FiasLevel lvl = FiasTools.AllLevels[i];
        string nm = GetName(lvl);
        Guid g = GetGuid(lvl);
        if (nm.Length > 0 && g == Guid.Empty)
        {
          errorText = "�������� �������� �� ������ \"" + FiasEnumNames.ToString(minRefBookLevel, false) + "\" ������ ���� ������� �� ����������� ����. ��������� �������� \"" + this[lvl] + "\" ������ \"" + FiasEnumNames.ToString(lvl, false) + "\" ��� � ����";
          return false;
        }
      }
      return true;
    }

    #endregion
  }
}
