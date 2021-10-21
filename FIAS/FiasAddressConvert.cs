using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Parsing;
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
  #region ������������ FiasAddressConvertGuidMode

  /// <summary>
  /// ��������� �������� ��� ��������
  /// </summary>
  public enum FiasAddressConvertGuidMode
  {
    /// <summary>
    /// GUID� �� ������������. ������������ ����� ��� ���� �������
    /// </summary>
    Text = 0x20,

    /// <summary>
    /// ������������ AOGUID. ������ � ��������� ������������ ��� �����
    /// </summary>
    AOGuid = 0x1,

    /// <summary>
    /// ������������ HOUSEGUID. ��������� ������������ ��� �����
    /// </summary>
    HouseGuid = 0x2,

    /// <summary>
    /// ������������ ROOMGUID. ������ � ��������� ������������ ��� �����
    /// </summary>
    RoomGuid = 0x4,

    /// <summary>
    /// ������������ AOGUID � ����� ��� ���� �������
    /// </summary>
    AOGuidWithText = 0x21,

    /// <summary>
    /// ������������ HOUSEGUID � ����� ��� ���� �������
    /// </summary>
    HouseGuidWithText = 0x22,

    /// <summary>
    /// ������������ ROOMGUID � ����� ��� ���� �������
    /// </summary>
    RoomGuidWithText = 0x24,

    /// <summary>
    /// ������������ �� ������������ ������, � "�����" ������������� ��������� �������, ������ ��� ���������.
    /// ���� ��� ����������, ��������, ������ ������� �� �� �����������, ������������� ����������.
    /// ����� ������������� ����������, ���� � ������ ����� �������� ������.
    /// ������ �� ������� ������������ ���� �����.
    /// </summary>
    GuidOnly = 0x40,

    /// <summary>
    /// ���� ��������, �� ������������ "�����" ������������� ��������� �������, ������ ��� ���������.
    /// ����� ������������ ������������ ������.
    /// </summary>
    GuidPreferred = 0x47,
  }

  #endregion

  /// <summary>
  /// �������������� ������ � ������ ��� �������� � ���� ������ ��� ������.
  /// ������ ������� �� ��� ���-�������� � ����� ������: N1="v1",N2="v2"
  /// ������������� ������:
  /// 1. ������� ��������� FiasAddressConvert.
  /// 2. ���������� ��������.
  /// 3. ������� ToString() ��� ������ ��� Parse()/TryParse() ��� ������.
  /// 
  /// ���� ����� �� �������� ���������������� � ����� ��������� �������. ������ ������� �������� �����������������.
  /// </summary>
  public sealed class FiasAddressConvert : ICloneable
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="source">�������� ������ ����. �� ����� ���� null.</param>
    public FiasAddressConvert(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _GuidMode = FiasAddressConvertGuidMode.AOGuid;
      _FillAddress = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ � ��������������.
    /// �������� � ������������
    /// </summary>
    public IFiasSource Source { get { return _Source; } }
    private readonly IFiasSource _Source;

    // ������ ������� ������ FiasHandler ��� ����, �.�. �� �� �������� ����������������

    /// <summary>
    /// ������ ������ ������.
    /// �� ��������� ������������ ����� AOGuid.
    /// </summary>
    public FiasAddressConvertGuidMode GuidMode
    {
      get { return _GuidMode; }
      set { _GuidMode = value; }
    }
    private FiasAddressConvertGuidMode _GuidMode;

    /// <summary>
    /// ����� �� �������� ����� FiasHandler.FillAddress()?
    /// �� ��������� - true.
    /// 
    /// ��� ������ Parse()/TryParse(), ���� �������� ����� false, � FiasAddress() ����� ��������� ������ �� ����,
    /// ������� ���� � ������������ ������. ��������, ����� ��������� ������������ ������ AOGuid � ����� ����.
    /// ��������������, ��� ���������� ��� ������� FiasHandler.FillAddress() ����� ���������� ����������� ������.
    /// 
    /// ��� ������ ToString(), ���� �������� ����� false, ��������������, ��� � ������ ��� ��������� ��� ����������,
    /// ����������� ��� �������� ������������ ������. ��������, ��� ������ ��� ��� ������ ����� FillAddress().
    /// �����, � ��������� ������� ����� �������� ��� ������ FillAddress(), ���� � FiasAddress ������������� �����������
    /// ��� ����������� ���������� ������.
    /// </summary>
    public bool FillAddress { get { return _FillAddress; } set { _FillAddress = value; } }
    private bool _FillAddress;

    /// <summary>
    /// ������� GuidMode ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return GuidMode.ToString();
    }

    #endregion

    #region �������

    static FiasAddressConvert()
    {
      _NameDict = new BidirectionalDictionary<FiasLevel, string>();
      _NameDict.Add(FiasLevel.Region, "REGION");
      _NameDict.Add(FiasLevel.District, "DISTRICT");
      _NameDict.Add(FiasLevel.City, "CITY");
      _NameDict.Add(FiasLevel.Village, "VILLAGE");
      _NameDict.Add(FiasLevel.PlanningStructure, "PLANSTR");
      _NameDict.Add(FiasLevel.Street, "STREET");
      _NameDict.Add(FiasLevel.House, "HOUSE");
      _NameDict.Add(FiasLevel.Building, "BUILDING");
      _NameDict.Add(FiasLevel.Structure, "STRUCTURE");
      _NameDict.Add(FiasLevel.Flat, "FLAT");
      _NameDict.Add(FiasLevel.Room, "ROOM");
    }

    private static readonly BidirectionalDictionary<FiasLevel, string> _NameDict;

    /// <summary>
    /// ���������� ������ ��������� ������� ��� �������� ������.
    /// ����� GuidOnly �� ������������, ��� ��� �� ����� ��������� � ����������� ��� �������������� ������
    /// </summary>
    public FiasAddressConvertGuidMode[] AvailableGuidModes
    {
      get
      {
        List<FiasAddressConvertGuidMode> lst = new List<FiasAddressConvertGuidMode>();
        lst.Add(FiasAddressConvertGuidMode.Text);
        lst.Add(FiasAddressConvertGuidMode.AOGuid);
        lst.Add(FiasAddressConvertGuidMode.AOGuidWithText);
        if (_Source.DBSettings.UseHouse)
        {
          lst.Add(FiasAddressConvertGuidMode.HouseGuid);
          lst.Add(FiasAddressConvertGuidMode.HouseGuidWithText);
        }
        if (_Source.DBSettings.UseRoom)
        {
          lst.Add(FiasAddressConvertGuidMode.RoomGuid);
          lst.Add(FiasAddressConvertGuidMode.RoomGuidWithText);
        }
        lst.Add(FiasAddressConvertGuidMode.GuidPreferred);
        return lst.ToArray();
      }
    }


    #endregion

    #region �������� ������

    /// <summary>
    /// ����������� ����� � ��������� ������.
    /// ������� ����� ������������ ��������� GuidMode.
    /// ��������������, ��� ����� ��� �������� ���������, �� ���� ��� ����� ������ FiasHandler.FillAddress().
    /// 
    /// ���� �������� FillAddress=true, �� ����� ������� ����� ������ � ������ ����� FiasHandler.FillAddress().
    /// ���� ����� �������� ����������������.
    /// </summary>
    /// <param name="address">�����, ������� ����� �������������. �� ����� ���� null</param>
    /// <returns>������������ ������ ������</returns>
    public string ToString(FiasAddress address)
    {
#if DEBUG
      if (address == null)
        throw new ArgumentNullException("address");
#endif

      if (address.IsEmpty)
        return String.Empty;

      if (FillAddress)
      {
        address = address.Clone();
        new FiasHandler(_Source).FillAddress(address);
      }

      if (GuidMode == FiasAddressConvertGuidMode.GuidOnly)
      {
        if (!address.AutoPostalCode)
          throw new InvalidCastException("������ ������������� ����� � GUID, ��� ��� ����� �������� ������");
        if (address.GuidBottomLevel != FiasTools.ReplaceToHouseOrFlat(address.NameBottomLevel))
          throw new InvalidCastException("������ ������������� ����� � GUID, ��� ��� ��������� �������� ������� �� �� �����������. ����� ����� ��� ������ \"" +
            FiasEnumNames.ToString(address.NameBottomLevel, true) + "\", � �� �����, ��� �� ����������� ������� ������ �� ������ \"" + FiasEnumNames.ToString(address.GuidBottomLevel, true) + "\"");

        return address.AnyGuid.ToString();
      }

      FiasAddressConvertGuidMode ThisGuidMode = _GuidMode & (FiasAddressConvertGuidMode.AOGuid | FiasAddressConvertGuidMode.HouseGuid | FiasAddressConvertGuidMode.RoomGuid);

      if (GuidMode == FiasAddressConvertGuidMode.GuidPreferred)
      {
        if (address.AutoPostalCode &&
        address.GuidBottomLevel == FiasTools.ReplaceToHouseOrFlat(address.NameBottomLevel))
        {
          return address.AnyGuid.ToString();
        }
        else
          ThisGuidMode = FiasAddressConvertGuidMode.RoomGuid;
      }

      List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();

      if (!address.AutoPostalCode)
        lst.Add(new KeyValuePair<string, string>("POSTALCODE", address.PostalCode));

      // � ������ ������ ������������ �����. Unknown - �� ������, Region - ������ ���
      FiasLevel FirstTextLevel = FiasLevel.Unknown;
      if (ThisGuidMode == FiasAddressConvertGuidMode.RoomGuid && address.GetGuid(FiasLevel.Flat) == Guid.Empty)
        ThisGuidMode = FiasAddressConvertGuidMode.HouseGuid;
      if (ThisGuidMode == FiasAddressConvertGuidMode.HouseGuid && address.GetGuid(FiasLevel.House) == Guid.Empty)
        ThisGuidMode = FiasAddressConvertGuidMode.AOGuid;

      if (ThisGuidMode == FiasAddressConvertGuidMode.AOGuid && address.AOGuid != Guid.Empty)
      {
        lst.Add(new KeyValuePair<string, string>("AOGUID", address.AOGuid.ToString()));
        FirstTextLevel = FiasLevel.House;
        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          if (address.GetName(FiasTools.AOLevels[i]).Length > 0 && address.GetGuid(FiasTools.AOLevels[i]) == Guid.Empty)
          {
            FirstTextLevel = FiasTools.AOLevels[i];
            break;
          }
        }
      }
      else if (ThisGuidMode == FiasAddressConvertGuidMode.HouseGuid /*&& address.GetGuid(FiasLevel.House) != Guid.Empty*/)
      {
        lst.Add(new KeyValuePair<string, string>("HOUSEGUID", address.GetGuid(FiasLevel.House).ToString()));
        FirstTextLevel = FiasLevel.Flat;
      }
      else if (ThisGuidMode == FiasAddressConvertGuidMode.RoomGuid /*&& address.GetGuid(FiasLevel.Room) != Guid.Empty*/)
      {
        lst.Add(new KeyValuePair<string, string>("ROOMGUID", address.GetGuid(FiasLevel.Flat).ToString()));
        FirstTextLevel = FiasLevel.Unknown;
      }
      else
        FirstTextLevel = FiasLevel.Region;

      if ((_GuidMode & FiasAddressConvertGuidMode.Text) != 0)
        FirstTextLevel = FiasLevel.Region; // �������������� ������ ���� �������

      if (FirstTextLevel != FiasLevel.Unknown)
      {
        for (int i = FiasTools.AllLevelIndexer.IndexOf(FirstTextLevel); i < FiasTools.AllLevels.Length; i++)
          AddNameAndTypePairs(lst, address, FiasTools.AllLevels[i]);
      }
      return StringKeyValueParser.ToString(lst.ToArray());
    }

    private static void AddNameAndTypePairs(List<KeyValuePair<string, string>> lst, FiasAddress address, FiasLevel level)
    {
      string n = address.GetName(level);
      if (n.Length == 0)
        return;

      lst.Add(new KeyValuePair<string, string>(_NameDict[level], n));
      string a = address.GetAOType(level);
      if (a != FiasTools.GetDefaultAOType(level))
        lst.Add(new KeyValuePair<string, string>(_NameDict[level] + ".TYPE", a));
    }

    /// <summary>
    /// ��������� �������������� �������.
    /// ��� FillHandler=true ����� �������� ���������� ���������������, �� ��������� � ������� ToString() � �����
    /// </summary>
    /// <param name="addresses">������ ������������� �������</param>
    /// <returns>������ ������������ �����</returns>
    public string[] ToStringArray(FiasAddress[] addresses)
    {
#if DEBUG
      if (addresses == null)
        throw new ArgumentNullException("addresses");
#endif
      if (addresses.Length == 0)
        return DataTools.EmptyStrings;

      string[] strs = new string[addresses.Length];
      if (FillAddress)
      {
        FiasAddress[] a2 = new FiasAddress[addresses.Length];
        for (int i = 0; i < a2.Length; i++)
          a2[i] = addresses[i].Clone();
        new FiasHandler(_Source).FillAddresses(a2);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        for (int i = 0; i < a2.Length; i++)
          strs[i] = conv2.ToString(a2[i]);
      }
      else
      {
        // ������� ����� � �����
        for (int i = 0; i < addresses.Length; i++)
          strs[i] = ToString(addresses[i]);
      }
      return strs;
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// �������������� �� ������ � �����.
    /// ��� ������� ������ � ������ ������������� ���������� InvalidCastException.
    /// ���� ���������� ����� �������� ������, �� ��� ������������ � ������ FiasAddress.Messages, � �� �������� ����������.
    /// �������������, ������ ����� ��������� ������������ GUID ������ ��� "���-��������". � ���� ������ �� ������������ � FiasAddress.UnknownGuid.
    /// ��� ������ ������ ������������ ������ FiasAddress.
    /// �������� GuidMode �� ������������.
    /// ���� ����� �������� ����������������.
    /// </summary>
    /// <param name="s">������ � ������</param>
    /// <returns>�����</returns>
    public FiasAddress Parse(string s)
    {
      FiasAddress address;
      if (TryParse(s, out address))
        return address;
      else
        throw new InvalidCastException("������ \"" + s + "\" ������ ������������� � �����");
    }

    /// <summary>
    /// �������������� �� ������ � �����.
    /// ��� ������� ������ � ������, ���������� �� �������������.
    /// ���� ���������� ����� �������� ������, �� ��� ������������ � ������ FiasAddress.Messages, ��� ���� ������������ true.
    /// �������������, ������ ����� ��������� ������������ GUID ������ ��� "���-��������". � ���� ������ �� ������������ � FiasAddress.UnknownGuid.
    /// ��� ������ ������ ������������ ������ FiasAddress.
    /// �������� GuidMode �� ������������.
    /// 
    /// ���� �������� FillAddress=true, �� ����� ��������� ���������� ���� ����������� ������.
    /// ���� FillAddress=false, �� ������ ���� �������� �������� ����� FiasHandler.FillAddress(), ���� ����� �� �������� ������.
    /// ������ ������������ ��� �������� �������������� �������. ������ ����������� � ������, � ����� ���������� ����� FiasHandler.FillAddresses() 
    /// ��� �������� ������������ �������.
    /// 
    /// ���� ����� �������� ����������������.
    /// </summary>
    /// <param name="s">������ � ������</param>
    /// <param name="address">���� ���������� �����.
    /// � ������ ������ ��� �������� ������, ��������� ������ ������ (FiasAddress.IsEmpty=true)</param>
    /// <returns>��������� ��������</returns>
    public bool TryParse(string s, out FiasAddress address)
    {
      address = new FiasAddress();
      if (String.IsNullOrEmpty(s))
        return true;

      Guid g;
      if (FiasTools.TryParseGuid(s, out g))
      {
        // 21.08.2020
        address.UnknownGuid = g;
        if (FillAddress)
          new FiasHandler(_Source).FillAddress(address);
        return true;
      }

      KeyValuePair<string, string>[] pairs;
      try
      {
        pairs = StringKeyValueParser.Parse(s);
      }
      catch (Exception e)
      {
        address.AddMessage(ErrorMessageKind.Error, "������ �������� ������ ������. " + e.Message);
        return false;
      }

      try
      {
        // 30.07.2020
        // ����� ��������� ����������� "��������������" ���� ��������, ��������, "��������"
        SingleScopeList<FiasLevel> MissedAOTypeLevels = null; // ��������, ����� �����������. 

        foreach (KeyValuePair<string, string> pair in pairs)
        {
          try
          {
            switch (pair.Key)
            {
              case "AOGUID":
                address.AOGuid = new Guid(pair.Value);
                break;
              case "HOUSEGUID":
                address.SetGuid(FiasLevel.House, new Guid(pair.Value));
                break;
              case "ROOMGUID":
                address.SetGuid(FiasLevel.Flat, new Guid(pair.Value));
                break;
              case "POSTALCODE":
                address.AutoPostalCode = false;
                address.PostalCode = pair.Value;
                break;
              default:
                bool IsAOType = false;
                string key = pair.Key;
                if (key.EndsWith(".TYPE"))
                {
                  key = key.Substring(0, key.Length - 5); // ��� ".TYPE"
                  IsAOType = true;
                }

                FiasLevel level;
                if (!_NameDict.TryGetKey(key, out level))
                {
                  address.AddMessage(ErrorMessageKind.Error, "����������� ��� \"" + pair.Key + "\" � ������ ������");
                  return false; // ����������� ���
                }

                if (MissedAOTypeLevels == null)
                  MissedAOTypeLevels = new SingleScopeList<FiasLevel>();

                if (IsAOType)
                {
                  address.SetAOType(level, pair.Value);
                  MissedAOTypeLevels.Remove(level);
                }
                else
                {
                  address.SetName(level, pair.Value);
                  MissedAOTypeLevels.Add(level);
                }
                break;
            }
          }
          catch (Exception e)
          {
            address.AddMessage(ErrorMessageKind.Error, "������ ��������� ���� \"" + pair.Key + "\" ������ ������. " + e.Message);
          }
        } // ���� �� �����

        // 30.07.2020
        // ��������� �������������� ����������
        if (MissedAOTypeLevels != null)
        {
          for (int i = 0; i < MissedAOTypeLevels.Count; i++)
          {
            string aoType = FiasTools.GetDefaultAOType(MissedAOTypeLevels[i]);
            if (aoType.Length > 0)
              address.SetAOType(MissedAOTypeLevels[i], aoType);
          }
        }

        if (address.Messages.Count == 0 && FillAddress)
          new FiasHandler(_Source).FillAddress(address);
      }
      catch (Exception e)
      {
        address.AddMessage(ErrorMessageKind.Error, "������ ��������� ����������� ������ ������. " + e.Message);
        return false;
      }

      return true;
    }

    /// <summary>
    /// ��������� ���������� �������� �����.
    /// ��� FillHandler=true ����� �������� ���������� ���������������, �� ��������� � ������� TryParse() � �����
    /// </summary>
    /// <param name="strs">������ ������������ �����. �� ����� ���� null</param>
    /// <param name="addresses">���� ���������� ������.
    /// ���� �����-���� �� ����� ����� ������������ ������, �� ���������� ������ ������ FiasAddress � ������������ �����������.
    /// ������ ������� ������������� ������ ������� FiasAddress.</param>
    /// <returns>true, ���� ��� ������ ����� ���������� ������</returns>
    public bool TryParseArray(string[] strs, out FiasAddress[] addresses)
    {
#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      addresses = new FiasAddress[strs.Length];
      if (strs.Length == 0)
        return true;

      FiasAddress addr;
      bool res = true;
      if (FillAddress)
      {
        // ������, ��� ������� ����� ����� ������� FiasHandler=FillAddresses().
        // �������������� ������� ������ � ��������� �������� ��� ���� ���������.
        List<FiasAddress> lstToFill = new List<FiasAddress>(strs.Length);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        for (int i = 0; i < strs.Length; i++)
        {
          if (conv2.TryParse(strs[i], out addr))
          {
            if (addr.Messages.Count == 0)
              lstToFill.Add(addr);
          }
          else
            res = false;
          addresses[i] = addr;
        }

        // ��������� ������
        if (lstToFill.Count > 0)
        {
          new FiasHandler(_Source).FillAddresses(lstToFill.ToArray());
        }
      }
      else
      {
        // ������� ����� �����
        for (int i = 0; i < strs.Length; i++)
        {
          if (!TryParse(strs[i], out addr))
            res = false;
          addresses[i] = addr;
        }
      }
      return res;
    }

    /// <summary>
    /// ��������� ���������� �������� �����.
    /// ���� ���� �� ���� ������ ����� ������������ ������, ������������� ����������
    /// </summary>
    /// <param name="strs">������ ������������� �����</param>
    /// <returns>������ �������</returns>
    public FiasAddress[] ParseArray(string[] strs)
    {
      // ����� ���� �� ������� TryParseArray(), ��������� ��������� � ��������� ����������.
      // �� �����, ���� ���� �� ����� ������������, ���������� ������ ����������������� �� ��������.

#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      FiasAddress[] addresses = new FiasAddress[strs.Length];
      if (strs.Length == 0)
        return addresses;

      FiasAddress addr;
      if (FillAddress)
      {
        // ������, ��� ������� ����� ����� ������� FiasHandler=FillAddresses().
        // �������������� ������� ������ � �������� �������� ��� ���� ���������.
        List<FiasAddress> lstToFill = new List<FiasAddress>(strs.Length);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        for (int i = 0; i < strs.Length; i++)
        {
          if (conv2.TryParse(strs[i], out addr))
          {
            if (addr.Messages.Count == 0)
              lstToFill.Add(addr);
          }
          else
            throw new InvalidCastException("������ \"" + strs[i] + "\" ������ ������������� � �����");

          addresses[i] = addr;
        }

        // ��������� ������
        if (lstToFill.Count > 0)
        {
          new FiasHandler(_Source).FillAddresses(lstToFill.ToArray());
        }
      }
      else
      {
        // ������� ����� �����
        for (int i = 0; i < strs.Length; i++)
          addresses[i] = Parse(strs[i]);
      }

      return addresses;
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ���������� ������� Parse() � ToString() ��� ������������ ������ ������.
    /// ��� ���� ������������ ������ � ������� GuidMode.
    /// ���� �������� ������ ����� ������������ ������, ������������� ����������.
    /// 
    /// ��������������, ��� �������� FillAddress �����������.
    /// ����� ���������� ��������� ����� �������� ������ ��� �������� �������������� GuidMode, �������� �� AOGuidWithText � Text.
    /// </summary>
    /// <param name="s">�������� ������������ ������</param>
    /// <returns>�������������� ������ � ��������� �������</returns>
    public string ConvertString(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      if (FillAddress)
      {
        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        FiasAddress addr = conv2.Parse(s);
        s = this.ToString(addr); // ������ FillAddress()
      }
      else
        s = ToString(Parse(s));

      return s;
    }

    /// <summary>
    /// ���������� ������� TryParse() � ToString() ��� ������������ ������ ������.
    /// ��� ���� ������������ ������ � ������� GuidMode.
    /// ���� �������� ������ ����� ������������ ������, ������������ false.
    /// 
    /// ��������������, ��� �������� FillAddress �����������.
    /// ����� ���������� ��������� ����� �������� ������ ��� �������� �������������� GuidMode, �������� �� AOGuidWithText � Text.
    /// </summary>
    /// <param name="s">������������ ������ (���� � �����)</param>
    /// <returns>��������� ������ TryParse</returns>
    public bool TryConvertString(ref string s)
    {
      if (String.IsNullOrEmpty(s))
        return true;

      FiasAddress addr;
      bool res;
      if (FillAddress)
      {
        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        res = conv2.TryParse(s, out addr);

        if (res)
          s = this.ToString(addr); // ������ FillAddress()
      }
      else
      {
        res = TryParse(s, out addr);
        if (res)
          s = ToString(addr);
      }

      return res;
    }

    /// <summary>
    /// ���������� ������� ParseArray() � ToStringArray() ��� ������������ ����� ������.
    /// ��� ���� ������������ ������ � ������� GuidMode.
    /// ���� �����-���� �� �������� ����� ����� ������������ ������, ������������� ����������.
    /// 
    /// ��������������, ��� �������� FillAddress �����������.
    /// ����� ���������� ��������� ����� �������� ������ ��� �������� �������������� GuidMode, �������� �� AOGuidWithText � Text.
    /// </summary>
    /// <param name="strs">�������� ������������ ������. ���� ������ �� �������� � �������� ������, � ��� ����� � ��� ������������� ������</param>
    /// <returns>�������������� ������ � ��������� �������</returns>
    public string[] ConvertStringArray(string[] strs)
    {
#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      if (strs.Length == 0)
        return DataTools.EmptyStrings;

      if (FillAddress)
      {
        // �� ������ �������� ��� �������������� ������� �������.
        // ���� �������������� ������ �� ������, �� ����� ���������� ������ FiasHandler.FillAddresses().
        // ������� � ������� ������ �� ������������ ������� ��������������

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;
        FiasAddress[] addrs = conv2.ParseArray(strs);
        string[] a2 = this.ToStringArray(addrs); // ������ FillAddresses()
        return a2;
      }
      else
      {
        string[] a2 = new string[strs.Length];
        for (int i = 0; i < strs.Length; i++)
          a2[i] = ToString(Parse(strs[i]));
        return a2;
      }
    }

    /// <summary>
    /// ���������� ������� TryParseArray() � ToStringArray() ��� ������������ ����� ������.
    /// ��� ���� ������������ ������ � ������� GuidMode.
    /// ���� �����-���� �� �������� ����� ����� ������������ ������, ����� ���������� false.
    /// 
    /// ��������������, ��� �������� FillAddress �����������.
    /// ����� ���������� ��������� ����� �������� ������ ��� �������� �������������� GuidMode, �������� �� AOGuidWithText � Text.
    /// </summary>
    /// <param name="strs">������������ ������. � �������� ������ �������� ������� ����������. ���� �����-���� ������ �� �������
    /// �������������, ��� �������� ��� ���������, � ���������� ������ ���������� ����������</param>
    /// <returns>�������������� ������ � ��������� �������</returns>
    public bool TryConvertStringArray(string[] strs)
    {
#if DEBUG
      if (strs == null)
        throw new ArgumentNullException("strs");
#endif

      if (strs.Length == 0)
        return true;

      FiasAddress addr;
      bool res = true;
      if (FillAddress)
      {
        // �������� �������� ��������� ���������� ������ TryParseArray(), �.�. ���� ����� �� ���������� ���������,
        // ����� ������ ������ ��������� �������������.

        List<FiasAddress> lstToFill = new List<FiasAddress>(strs.Length);

        FiasAddressConvert conv2 = this.Clone();
        conv2.FillAddress = false;

        FiasAddress[] addrs = new FiasAddress[strs.Length];
        for (int i = 0; i < strs.Length; i++)
        {
          if (conv2.TryParse(strs[i], out addr))
          {
            if (addr.Messages.Count == 0)
              lstToFill.Add(addr);
          }
          else
            res = false;
          addrs[i] = addr;
        }

        // ��������� ������
        if (lstToFill.Count > 0)
        {
          new FiasHandler(_Source).FillAddresses(lstToFill.ToArray());
        }


        string[] strs2 = conv2.ToStringArray(addrs); // ����� ��� ������ FillAddress()
        strs2.CopyTo(strs, 0);
      }
      else
      {
        for (int i = 0; i < strs.Length; i++)
        {
          if (TryParse(strs[i], out addr))
            strs[i] = ToString(addr);
          else
            res = false;
        }
      }
      return res;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// ������� ����� ������� FiasAddressConvert � ������ �� ���������� �������
    /// </summary>
    /// <returns></returns>
    public FiasAddressConvert Clone()
    {
      FiasAddressConvert res = new FiasAddressConvert(_Source);
      res.GuidMode = this.GuidMode;
      res.FillAddress = this.FillAddress;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  // TODO: �������� �� ������� � ExtTools.dll

  /// <summary>
  /// ������ ��� ��� ���=��������
  /// </summary>
  internal static class StringKeyValueParser
  {
    #region ����������� ������

    public static string ToString(KeyValuePair<string, string>[] a)
    {
      if (a.Length == 0)
        return String.Empty;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");

        if (String.IsNullOrEmpty(a[i].Key))
          throw new ArgumentNullException("a[" + i.ToString() + "]");

        sb.Append(a[i].Key);
        sb.Append('=');
        if (String.IsNullOrEmpty(a[i].Value))
          sb.Append("\"\"");
        else
        {
          sb.Append('\"');
          for (int j = 0; j < a[i].Value.Length; j++)
          {
            char c = a[i].Value[j];
            if (c == '\"')
              sb.Append("\"\"");
            else
              sb.Append(c);
          }
          sb.Append('\"');
        }
      }
      return sb.ToString();
    }

    public static KeyValuePair<string, string>[] Parse(string s)
    {
      ParsingData pd = new ParsingData(s);
      _TheParserList.Parse(pd);
      if (pd.FirstErrorToken != null)
        throw new InvalidCastException("������ �������� ������");
      List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
      string CurrName = null;
      bool HasEq = false;
      for (int i = 0; i < pd.Tokens.Count; i++)
      {
        switch (pd.Tokens[i].TokenType)
        {
          case "Key":
            if (CurrName != null)
              throw new InvalidCastException("��� ����� ������");
            if (HasEq)
              throw new InvalidCastException("���� ����� ����� \"=\"");
            CurrName = pd.Tokens[i].Text;
            break;
          case "=":
            if (HasEq)
              throw new InvalidCastException("��� ����� \"=\" ������");
            if (CurrName == null)
              throw new InvalidCastException("�� ���� ����� ����� ������ \"=\"");
            HasEq = true;
            break;
          case "String":
            if (!HasEq)
              throw new InvalidCastException("�� ���� ����� \"=\"");
            lst.Add(new KeyValuePair<string, string>(CurrName, pd.Tokens[i].AuxData.ToString()));
            CurrName = null;
            HasEq = false;
            break;
          case "Space":
            break;
          default:
            throw new BugException("����������� ������� " + pd.Tokens[i].TokenType);
        }
      }

      if (HasEq)
        throw new InvalidCastException("����� ����� \"=\" �� ���� ��������");
      if (CurrName != null)
        throw new InvalidCastException("����� ����� �� ���� ��������");

      return lst.ToArray();
    }

    #endregion

    #region �������

    private static readonly ParserList _TheParserList = CreateParserList();

    private static ParserList CreateParserList()
    {
      ParserList lst = new ParserList();
      lst.Add(new KeyParser()); // �����
      lst.Add(new EqParser()); // ���� "="
      lst.Add(new StrConstParser()); // ��������
      lst.Add(new SpaceParser(' ', '\t', ','));
      return lst;
    }

    /// <summary>
    /// ������, ������������ �����
    /// </summary>
    private class KeyParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        if (!(Char.IsLetter(data.GetChar(data.CurrPos)) || data.GetChar(data.CurrPos) == '_'))
          return;
        int cnt = 1;
        while (Char.IsLetterOrDigit(data.GetChar(data.CurrPos + cnt)) || data.GetChar(data.CurrPos + cnt) == '_' || data.GetChar(data.CurrPos + cnt) == '.')
          cnt++;

        data.Tokens.Add(new Token(data, this, "Key", data.CurrPos, cnt));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        return null;
      }

      #endregion
    }

    /// <summary>
    /// ������ ��� ����� "="
    /// </summary>
    private class EqParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        if (data.GetChar(data.CurrPos) == '=')
          data.Tokens.Add(new Token(data, this, "=", data.CurrPos, 1));
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        return null;
      }

      #endregion
    }

    #endregion
  }
}
