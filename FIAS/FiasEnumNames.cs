// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  #region FiasLevel

  /// <summary>
  /// �������� ���� AOLEVEL (������� ��������� �������)
  /// </summary>
  [Serializable]
  public enum FiasLevel
  {
    #region ��������, ������������ � ����

    /// <summary>
    /// ������� �������
    /// </summary>
    Region = 1,

    /// <summary>
    /// ������� ����������� ������ (����������)
    /// </summary>
    AutonomousArea = 2,

    /// <summary>
    /// ������� ������
    /// </summary>
    District = 3,

    /// <summary>
    /// ������� ��������� � �������� ���������
    /// </summary>
    Settlement = 35,

    /// <summary>
    /// ������� ������
    /// </summary>
    City = 4,

    /// <summary>
    /// ������� ��������������� ���������� (����������)
    /// </summary>
    InnerCityArea = 5,

    /// <summary>
    /// ������� ����������� ������
    /// </summary>
    Village = 6,

    /// <summary>
    /// ������������� ���������
    /// </summary>
    PlanningStructure = 65,

    /// <summary>
    /// ������� �����
    /// </summary>
    Street = 7,

    /// <summary>
    /// ��������� �������
    /// </summary>
    LandPlot = 75,

    /// <summary>
    /// ������, ����������, ������� �������������� �������������
    /// </summary>
    House = 8,

    /// <summary>
    ///  ������� ��������� � �������� ������, ����������
    /// </summary>
    Flat = 9,

    /// <summary>
    /// ������� �������������� ���������� (����������)
    /// </summary>
    AdditionalTerritory = 90,

    /// <summary>
    /// ������� �������� �� �������������� ����������� (����������)
    /// </summary>
    AdditionalTerritoryObject = 91,

    #endregion

    #region �������������� ��������

    /// <summary>
    /// ������.
    /// ��� �������� �� ���������� � ����
    /// </summary>
    Building = 201,

    /// <summary>
    /// ��������.
    /// ��� �������� �� ���������� � ����
    /// </summary>
    Structure = 202,

    /// <summary>
    /// �������
    /// ��� �������� �� ���������� � ����
    /// </summary>
    Room = 203,

    /// <summary>
    /// ������� �� �����
    /// </summary>
    Unknown = 0,

    #endregion
  }

  #endregion

  #region FiasCenterStatus

  /// <summary>
  /// �������� �� ���������� ����� �������� ������ �/��� �������?
  /// �������� ���� AddressObjects.CENTSTATUS
  /// </summary>
  [Serializable]
  public enum FiasCenterStatus
  {
    /// <summary>
    /// ������ �� �������� ������� ���������������-���������������� �����������
    /// </summary>
    None = 0,

    /// <summary>
    /// ������ �������� ������� ������
    /// </summary>
    District = 1,

    /// <summary>
    /// ������ �������� ������� (��������) �������
    /// </summary>
    Region = 2,

    /// <summary>
    /// ������ �������� ������������ � ������� ������ � ������� �������.
    /// </summary>
    RegionAndDistrict = 3
  }

  #endregion

  #region FiasDivType

  /// <summary>
  /// ������� ���������
  /// </summary>
  [Serializable]
  public enum FiasDivType
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// ������������� �������
    /// </summary>
    MD = 1,

    /// <summary>
    /// ���������������-��������������� �������
    /// </summary>
    ATD = 2,
  }

  #endregion

  #region FiasStructureStatus

  /// <summary>
  /// ������� ��������.
  /// �������� ���� "STRSTATUS"
  /// </summary>
  [Serializable]
  public enum FiasStructureStatus
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// ��������
    /// </summary>
    Structure = 1,

    /// <summary>
    /// ����������
    /// </summary>
    Construction = 2,

    /// <summary>
    /// �����
    /// </summary>
    Character = 3,
  }

  #endregion

  #region FiasEstateStatus

  /// <summary>
  /// ������� �������� (���������).
  /// �������� ���� ESTSTATUS
  /// </summary>
  [Serializable]
  public enum FiasEstateStatus
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// ��������
    /// </summary>
    Hold = 1,

    /// <summary>
    /// ���
    /// </summary>
    House = 2,

    /// <summary>
    /// ������������
    /// </summary>
    Household = 3,

    /// <summary>
    /// �����
    /// </summary>
    Garage = 4,

    /// <summary>
    /// ������
    /// </summary>
    Bulding = 5,

    /// <summary>
    /// �����
    /// </summary>
    Mine = 6,

    /// <summary>
    /// ������
    /// </summary>
    Basement = 7,

    /// <summary>
    /// ���������
    /// </summary>
    Steamshop = 8,

    /// <summary>
    /// ������
    /// </summary>
    Cellar = 9,

    /// <summary>
    /// ������ �������������� �������������
    /// </summary>
    UnderConstruction = 10,
  }

  #endregion

  #region FiasFlatType

  /// <summary>
  /// ��� ���������.
  /// �������� ���� FLATTYPE
  /// </summary>
  [Serializable]
  public enum FiasFlatType
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// ���������
    /// </summary>
    Space = 1,

    /// <summary>
    /// ��������
    /// </summary>
    Flat = 2,

    /// <summary>
    /// ����,
    /// </summary>
    Office = 3,

    /// <summary>
    /// �������
    /// </summary>
    Room = 4,

    /// <summary>
    /// ������� �������
    /// </summary>
    WorkingSection = 5,

    /// <summary>
    /// �����
    /// </summary>
    Storage = 6,

    /// <summary>
    /// �������� ���
    /// </summary>
    SalesArea = 7,

    /// <summary>
    /// ���
    /// </summary>
    Workshop = 8,

    /// <summary>
    /// ��������
    /// </summary>
    Pavilion = 9,

    /// <summary>
    /// ������
    /// </summary>
    Basement = 10,

    /// <summary>
    /// ���������
    /// </summary>
    Steamshop = 11,

    /// <summary>
    /// ������
    /// </summary>
    Cellar = 12,

    /// <summary>
    /// �����
    /// </summary>
    Garage = 13,
  };

  #endregion

  #region FiasRoomType

  /// <summary>
  /// ��� �������.
  /// �������� ���� ROOMTYPE
  /// </summary>
  [Serializable]
  public enum FiasRoomType
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// �������
    /// </summary>
    Room = 1,

    /// <summary>
    /// ���������
    /// </summary>
    Space = 2
  }

  #endregion

  #region FiasActuality

  /// <summary>
  /// ����������� ������
  /// </summary>
  [Serializable]
  public enum FiasActuality
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown,

    /// <summary>
    /// ���������� �����
    /// </summary>
    Actual,

    /// <summary>
    /// ������������ ��������
    /// </summary>
    Historical,
  }

  #endregion

  #region FiasTableType

  /// <summary>
  /// ������� � ����������� ����
  /// </summary>
  [Serializable]
  public enum FiasTableType
  {
    /// <summary>
    /// �� ����������
    /// </summary>
    Unknown,

    /// <summary>
    /// �������� �������
    /// </summary>
    AddrOb,

    /// <summary>
    /// ������ � ����������
    /// </summary>
    House,

    /// <summary>
    /// ���������
    /// </summary>
    Room
  }

  #endregion

  #region FiasAOTypeMode

  /// <summary>
  /// ������� ���� ����������������� ��������
  /// </summary>
  [Serializable]
  public enum FiasAOTypeMode
  {
    /// <summary>
    /// ������ ������������ ���� �������� ("�����"). ������������� ���� "SOCRNAME" � ������� "SOCRBASE"
    /// </summary>
    Full,

    /// <summary>
    /// ����������� ������������ ���� �������� ("��."). ������������� ���� "SCNAME" � ������� "SOCRBASE"
    /// </summary>
    Abbreviation
  }

  #endregion

  #region FiasEditorLevel

  /// <summary>
  /// �������, �� �������� ����� ������������� ����� � ���������
  /// </summary>
  [Serializable]
  public enum FiasEditorLevel
  {
    /// <summary>
    /// �� ����������� ������
    /// </summary>
    Village,

    /// <summary>
    /// �� �����
    /// </summary>
    Street,

    /// <summary>
    /// �� ������
    /// </summary>
    House,

    /// <summary>
    /// �� ���������
    /// </summary>
    Room
  }

  #endregion

  #region FiasLevelCompareResult

  /// <summary>
  /// ���������� ��������� ������� ������.
  /// �������� �������� 0, 1 � (-1), ����������� � ������������� ���������� ����������� ������� Compare() � Net Framework
  /// </summary>
  [Serializable]
  public enum FiasLevelCompareResult
  {
    /// <summary>
    /// ������ ���������
    /// </summary>
    Equal = 0,

    /// <summary>
    /// ������ ������������ ������� �������� ����� ���������, ��� ������
    /// </summary>
    Greater = +1,

    /// <summary>
    /// ������ ������������ ������� �������� ����� ���������, ��� ������
    /// </summary>
    Less = -1,
  }

  #endregion

  /// <summary>
  /// ������������ ��� ������������, ������������ ����
  /// </summary>
  public static class FiasEnumNames
  {
    /// <summary>
    /// �������, ��������� �� ���������� ��� �������������� �������������
    /// </summary>
    private static readonly CharArrayIndexer _AbbrRemovedCharIndexer = new CharArrayIndexer(" .-/");

    #region FiasLevel

    private static Dictionary<FiasLevel, string> _LevelNames1 = CreateLevelNames1();
    private static Dictionary<FiasLevel, string> CreateLevelNames1()
    {
      Dictionary<FiasLevel, string> d = new Dictionary<FiasLevel, string>();
      d.Add(FiasLevel.Region, "������");
      d.Add(FiasLevel.AutonomousArea, "��");
      d.Add(FiasLevel.District, "�����");
      d.Add(FiasLevel.Settlement, "�/� ���.");
      d.Add(FiasLevel.City, "�����");
      d.Add(FiasLevel.InnerCityArea, "���������.�.");
      d.Add(FiasLevel.Village, "���. �����");
      d.Add(FiasLevel.PlanningStructure, "����.���");
      d.Add(FiasLevel.Street, "�����");
      d.Add(FiasLevel.LandPlot, "�������");
      d.Add(FiasLevel.AdditionalTerritory, "���.���.");
      d.Add(FiasLevel.AdditionalTerritoryObject, "���.���.��.");
      d.Add(FiasLevel.House, "���");
      d.Add(FiasLevel.Building, "������");
      d.Add(FiasLevel.Structure, "��������");
      d.Add(FiasLevel.Flat, "��������");
      d.Add(FiasLevel.Room, "�������");
      return d;
    }

    private static Dictionary<FiasLevel, string> _LevelNames2 = CreateLevelNames2();
    private static Dictionary<FiasLevel, string> CreateLevelNames2()
    {
      Dictionary<FiasLevel, string> d = new Dictionary<FiasLevel, string>();
      d.Add(FiasLevel.Region, "������� ���������� ���������");
      d.Add(FiasLevel.AutonomousArea, "���������� ����� (����������)");
      d.Add(FiasLevel.District, "�����");
      d.Add(FiasLevel.Settlement, "��������� ��� �������� ���������");
      d.Add(FiasLevel.City, "�����");
      d.Add(FiasLevel.InnerCityArea, "��������������� ���������� (����������)");
      d.Add(FiasLevel.Village, "���������� �����");
      d.Add(FiasLevel.PlanningStructure, "������������� ���������");
      d.Add(FiasLevel.Street, "�����");
      d.Add(FiasLevel.LandPlot, "��������� �������");
      d.Add(FiasLevel.AdditionalTerritory, "�������������� ���������� (����������)");
      d.Add(FiasLevel.AdditionalTerritoryObject, "������ �� �������������� ���������� (����������)");
      d.Add(FiasLevel.House, "������, ����������, ������ �������������� �������������");
      d.Add(FiasLevel.Building, "������");
      d.Add(FiasLevel.Structure, "��������");
      d.Add(FiasLevel.Flat, "��������� � �������� ������, ����������");
      d.Add(FiasLevel.Room, "�������");
      return d;
    }

    /// <summary>
    /// �������� ��������� ������������� ��� ������ ��������� �������
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="isLong">true - ������� ������������� ("���������� �����"),
    /// false - ������� ������������� ("���. �����")</param>
    /// <returns>�����</returns>
    public static string ToString(FiasLevel level, bool isLong)
    {
      if (level == FiasLevel.Unknown)
        return "�� �����"; // 29.04.2020
      Dictionary<FiasLevel, string> d = isLong ? _LevelNames2 : _LevelNames1;
      string s;
      if (d.TryGetValue(level, out s))
        return s;
      else
        return level.ToString();
    }

    #endregion

    #region FiasCenterStatus

    /// <summary>
    /// �������� �� ���������� ����� �������� ������ �/��� �������?
    /// �������� ���� AddressObjects.CENTSTATUS
    /// </summary>
    public static readonly string[] CenterStatusNames = new string[] { 
      "���",
      "�������� �����",
      "����� (�������) �������",
      "����� ������� � ������"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasCenterStatus.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasCenterStatus value)
    {
      return GetName((int)value, CenterStatusNames);
    }

    private static string GetName(int value, string[] names)
    {
      if (value < 0 || value >= names.Length)
        return "?? " + value.ToString();
      else
        return names[value];
    }

    #endregion

    #region FiasDivType

    /// <summary>
    /// �������� ���������
    /// </summary>
    public static readonly string[] DivTypeNames = new string[] { 
      "�� ����������",
      "������������� �������",
      "���������������-��������������� �������"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasDivType.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasDivType value)
    {
      return GetName((int)value, DivTypeNames);
    }

    #endregion

    #region FiasEstateStatus (��� ��������)

    /// <summary>
    /// ������� �������� (���������).
    /// �������� ���� ESTSTATUS
    /// </summary>
    public static readonly string[] EstateStatusAOTypes = new string[] { 
     "�� ����������",
     "��������",
     "���",
     "������������",
     "�����",
     "������",
     "�����",
     "������", // ���� �� 23.03.2020
     "���������", // ���� �� 23.03.2020
     "������", // ���� �� 23.03.2020
     "������ ���-�� ���-��" // ���� �� 23.03.2020
    };

    private static readonly StringArrayIndexer _EstateStatusAOTypeIndexer = new StringArrayIndexer(EstateStatusAOTypes, true);


    /// <summary>
    /// ������� �������� (���������). ����������
    /// �������� ���� ESTSTATUS
    /// </summary>
    public static readonly string[] EstateStatusAbbrs = new string[] { 
     String.Empty,
     "���.",
     "�.",
     "����.",
     "�-�",
     "��.",
     "�����",
     "����.", 
     "���.", 
     "��.", 
     "���"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasEstateStatus.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasEstateStatus value)
    {
      return GetName((int)value, EstateStatusAOTypes);
    }

    /// <summary>
    /// ����������� ������ ����������, ��������, "���" � ������������ FiasEstateStatus.
    /// ������� �� �����������.
    /// ������������� �������������� ����������, ��������, "�".
    /// ���� ������ ������ ������, ������������ �������� Unknown.
    /// ���� ������ ����������� ������, ������������ �������� Unknown. ���������� �� �������������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>�������� ������������</returns>
    public static FiasEstateStatus ParseEstateStatus(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasEstateStatus.Unknown;

      int p = _EstateStatusAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasEstateStatus)p;

      // ��� ���������� �� ������ ��� ����� FiasCachedAOTypes.CreateAOTypeLevelDict()

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "�": return FiasEstateStatus.House;
        case "���": return FiasEstateStatus.Hold;
        case "��":
        case "���":
        case "����":
        case "�����": return FiasEstateStatus.Hold;
        case "���":
        case "�-�":
        case "��": return FiasEstateStatus.Garage;
        case "��": return FiasEstateStatus.Bulding;
        case "����": return FiasEstateStatus.Basement;
        case "���": return FiasEstateStatus.Steamshop;
        case "��": return FiasEstateStatus.Cellar;
        case "���": return FiasEstateStatus.UnderConstruction;
      }

      return FiasEstateStatus.Unknown;
    }

    #endregion

    #region FiasStructureStatus (��� ��������)

    /// <summary>
    /// ������� ��������.
    /// �������� ���� "STRSTATUS"
    /// </summary>
    public static readonly string[] StructureStatusAOTypes = new string[] { 
      "�� ����������",
      "��������",
      "����������",
      "�����"};

    private static readonly StringArrayIndexer _StructureStatusAOTypeIndexer = new StringArrayIndexer(StructureStatusAOTypes, true);

    /// <summary>
    /// ������� ��������. ����������
    /// �������� ���� "STRSTATUS"
    /// </summary>
    public static readonly string[] StructureStatusAbbrs = new string[] { 
      String.Empty,
      "���.",
      "c���.",
      "�."};


    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasStructureStatus.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasStructureStatus value)
    {
      return GetName((int)value, StructureStatusAOTypes);
    }

    /// <summary>
    /// ����������� ������ ����������, ��������, "��������" � ������������ FiasStructureStatus.
    /// ������� �� �����������.
    /// ������������� �������������� ����������, ��������, "���".
    /// ���� ������ ������ ������, ������������ �������� Unknown.
    /// ���� ������ ����������� ������, ������������ �������� Unknown. ���������� �� �������������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>�������� ������������</returns>
    public static FiasStructureStatus ParseStructureStatus(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasStructureStatus.Unknown;

      int p = _StructureStatusAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasStructureStatus)p;

      // ��� ���������� �� ������ ��� ����� FiasCachedAOTypes.CreateAOTypeLevelDict()

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "���":
        case "��": return FiasStructureStatus.Structure;
        case "������":
        case "����": return FiasStructureStatus.Construction;
        case "���":
        case "�": return FiasStructureStatus.Character;
      }

      return FiasStructureStatus.Unknown;
    }

    #endregion

    #region FiasFlatType

    /// <summary>
    /// ��� ���������.
    /// �������� ���� FLATTYPE
    /// </summary>
    public static readonly string[] FlatTypeAOTypes = new string[] { 
      "�� ����������",
      "���������",
      "��������",
      "����",
      "�������",
      "������� �������",
      "�����",
      "�������� ���",
      "���",
      "��������",
      "������", // ���� �� 23.03.2020
      "���������", // ���� �� 23.03.2020
      "������", // ���� �� 23.03.2020
      "�����" // ���� �� 23.03.2020
    };

    private static readonly StringArrayIndexer _FlatTypeAOTypeIndexer = new StringArrayIndexer(FlatTypeAOTypes, true);

    /// <summary>
    /// ��� ���������. ����������
    /// �������� ���� FLATTYPE
    /// </summary>
    public static readonly string[] FlatTypeAbbrs = new string[] { 
      String.Empty,
      "���.",
      "��.",
      "��.",
      "���.",
      "���.��.",
      "���.",
      "����.���",
      "���",
      "���.",
      "����.", // ���� �� 23.03.2020
      "���.", // ���� �� 23.03.2020
      "�-�", // ���� �� 23.03.2020
      "�-�" // ���� �� 23.03.2020
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasFlatType.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasFlatType value)
    {
      return GetName((int)value, FlatTypeAOTypes);
    }

    /// <summary>
    /// ����������� ������ ����������, ��������, "��������" � ������������ FiasFlatType.
    /// ������� �� �����������.
    /// ������������� �������������� ����������, ��������, "��".
    /// ���� ������ ������ ������, ������������ �������� Unknown.
    /// ���� ������ ����������� ������, ������������ �������� Unknown. ���������� �� �������������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>�������� ������������</returns>
    public static FiasFlatType ParseFlatType(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasFlatType.Unknown;

      int p = _FlatTypeAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasFlatType)p;

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "���": return FiasFlatType.Space;
        case "��": return FiasFlatType.Flat;
        case "��": return FiasFlatType.Office;
        case "���":
        case "����": return FiasFlatType.Room;
        case "���": return FiasFlatType.Storage;
        case "��":
        case "����":
        case "�������": return FiasFlatType.SalesArea;
        case "���": return FiasFlatType.Pavilion;
        case "����": return FiasFlatType.Basement;
        case "���": return FiasFlatType.Steamshop;
        case "��": return FiasFlatType.Cellar;
        case "���":
        case "��": return FiasFlatType.Garage;
      }

      return FiasFlatType.Unknown;
    }

    #endregion

    #region FiasRoomType

    /// <summary>
    /// ��� �������.
    /// �������� ���� ROOMTYPE
    /// </summary>
    public static readonly string[] RoomTypeAOTypes = new string[] { 
      "�� ����������",
      "�������",
      "���������"
    };

    private static readonly StringArrayIndexer _RoomTypeAOTypeIndexer = new StringArrayIndexer(RoomTypeAOTypes, true);

    /// <summary>
    /// ��� �������. ����������
    /// �������� ���� ROOMTYPE
    /// </summary>
    public static readonly string[] RoomTypeAbbrs = new string[] { 
      String.Empty,
      "���.",
      "���."
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasRoomType.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasRoomType value)
    {
      return GetName((int)value, RoomTypeAOTypes);
    }

    /// <summary>
    /// ����������� ������ ����������, ��������, "�������" � ������������ FiasRoomType.
    /// ������� �� �����������.
    /// ������������� �������������� ����������, ��������, "���".
    /// ���� ������ ������ ������, ������������ �������� Unknown.
    /// ���� ������ ����������� ������, ������������ �������� Unknown. ���������� �� �������������.
    /// </summary>
    /// <param name="s">������������� ������</param>
    /// <returns>�������� ������������</returns>
    public static FiasRoomType ParseRoomType(string s)
    {
      if (String.IsNullOrEmpty(s))
        return FiasRoomType.Unknown;

      int p = _RoomTypeAOTypeIndexer.IndexOf(s);
      if (p >= 0)
        return (FiasRoomType)p;

      s = DataTools.RemoveChars(s.ToLowerInvariant(), _AbbrRemovedCharIndexer);
      switch (s)
      {
        case "���":
        case "����": return FiasRoomType.Room;
        case "���": return FiasRoomType.Space;
      }

      return FiasRoomType.Unknown;
    }

    #endregion

    #region FiasActuality

    /// <summary>
    /// ����������� ������
    /// </summary>
    public static readonly string[] ActualityNames = new string[] { 
      "�� ����������",
      "����������",
      "������������"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasActuality.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasActuality value)
    {
      return GetName((int)value, ActualityNames);
    }

    #endregion

    #region FiasTableType

    /// <summary>
    /// ��������� ��������, ��������������� ������������ FiasTableType, �� ������������� �����
    /// </summary>
    public static readonly string[] TableTypeNamesPlural = new string[] { 
      "����������",
      "�������� �������",
      "������",
      "���������"
    };

    /// <summary>
    /// ��������� ��������, ��������������� ������������ FiasTableType, � ������������ �����
    /// </summary>
    public static readonly string[] TableTypeNamesSingular = new string[] { 
      "����������",
      "�������� ������",
      "������",
      "���������"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasTableType.
    /// ���� �������� ������������ �������� <paramref name="value"/>, ������������ �������� �������� �� ������� "??" 
    /// </summary>
    /// <param name="value">��������</param>
    /// <param name="plural">true - �� ������������� �����, false - � ������������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasTableType value, bool plural)
    {
      return GetName((int)value, plural ? TableTypeNamesPlural : TableTypeNamesSingular);
    }

    #endregion

    #region FiasTableType

    /// <summary>
    /// �������� ��� ������������ FiasTableType
    /// </summary>
    public static readonly string[] FiasTableTypeNames = new string[] { 
      "�� ����������",
      "�������� �������",
      "������",
      "���������"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasTableType
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasTableType value)
    {
      return GetName((int)value, FiasTableTypeNames);
    }

    #endregion

    #region FiasEditorLebel

    /// <summary>
    /// �������� ��� ������������ FiasEditorLevel
    /// </summary>
    public static readonly string[] FiasEditorLevelNames = new string[] { 
      "���������� �����",
      "�����",
      "������",
      "���������"
    };

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ FiasEditorLevel
    /// </summary>
    /// <param name="value">��������</param>
    /// <returns>��������� �������������</returns>
    public static string ToString(FiasEditorLevel value)
    {
      return GetName((int)value, FiasEditorLevelNames);
    }

    #endregion
  }
}
