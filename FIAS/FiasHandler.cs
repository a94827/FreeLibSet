using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;

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
  /// �������� ������ ��� ������ � ��������, ������������ � ���������������� ����.
  /// ����� ����������� ������������ ���������� ��������.
  /// ���� ����� �� �������� ����������������
  /// </summary>
  public sealed class FiasHandler
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="source">�������� ������. ������ ���� �����</param>
    public FiasHandler(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
      _AddrObExtractor = new FiasAddrObExtractor(source);
      if (source.DBSettings.UseHouse)
        _HouseExtractor = new FiasHouseExtractor(source);
      if (source.DBSettings.UseRoom)
        _RoomExtractor = new FiasRoomExtractor(source);
      _Today = DateTime.Today;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ������ ��� ��������� ������ ��������������
    /// </summary>
    public IFiasSource Source { get { return _Source; } }
    private IFiasSource _Source;

    private FiasAddrObExtractor _AddrObExtractor;
    private FiasHouseExtractor _HouseExtractor;
    private FiasRoomExtractor _RoomExtractor;

    /// <summary>
    /// ������� ����, ������������ ��� �������� ������������
    /// </summary>
    public DateTime Today { get { return _Today; } }
    private DateTime _Today;

    #endregion

    #region ���������� �������� ������

    /// <summary>
    /// ������������ ����� ������.
    /// ����� ���������� �������� ������ � ������ ����������� ������ FiasAddress.Messages.
    /// ���� ��������� ��������� ��������� �������, ����������� ����� FillAddresses(), 
    /// �.�. �� ����� ������� ��������� ������ ����� �������������� �� ���� ������ � ���.
    /// </summary>
    /// <param name="address">�����</param>
    public void FillAddress(FiasAddress address)
    {
      if (address == null)
        throw new ArgumentNullException("address");

      FillAddresses(new FiasAddress[1] { address });
    }

    /// <summary>
    /// ������������ ����� ���������� �������.
    /// ����� ���������� �������� ������ � ������ ����������� ������ FiasAddress.Messages.
    /// </summary>
    /// <param name="addresses">������</param>
    public void FillAddresses(FiasAddress[] addresses)
    {
      if (addresses == null)
        throw new ArgumentNullException("addresses");

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FillAddresses() started. addresses.Length=" + addresses.Length.ToString());

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "���������� ������ GUID��",
       "�������� �������",
       addresses.Length==1 ? "���������� ������" : "���������� ������� ("+addresses.Length.ToString()+")"
      });
      try
      {
        #region �������� GUID�

        ReplaceUnknownGuids(addresses);

        PageLoader loader = new PageLoader(_Source);
        spl.PercentMax = addresses.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < addresses.Length; i++)
        {
          if (addresses[i] == null)
            throw new ArgumentNullException("addresses[" + i.ToString() + "]");

          loader.AddGuids(addresses[i]);
          spl.IncPercent();
        }

        spl.Complete();

        #endregion

        // ��������� ��������
        loader.Load(spl);
        spl.Complete();

        // ��������� ������
        spl.PercentMax = addresses.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < addresses.Length; i++)
        {
          DoFillAddress(addresses[i], loader);
          spl.IncPercent();
        }
        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FillAddresses() finished.");
    }

    private void ReplaceUnknownGuids(FiasAddress[] addresses)
    {
      SingleScopeList<Guid> UnknownGuids = null;
      for (int i = 0; i < addresses.Length; i++)
      {
        if (addresses[i].UnknownGuid != Guid.Empty)
        {
          if (UnknownGuids == null)
            UnknownGuids = new SingleScopeList<Guid>();
          UnknownGuids.Add(addresses[i].UnknownGuid);
        }
      }
      if (UnknownGuids == null)
        return;

      IDictionary<Guid, FiasGuidInfo> dictGuid = Source.GetGuidInfo(UnknownGuids.ToArray(), FiasTableType.Unknown);

      // 25.02.2021
      // ���� GUID �� ������, �� ��� ����� ���� ������������� ������ RecId.
      // �������� ��� ���� ���������
      List<Guid> UnknownRecIds = null; // �� ����������� ������������ SingleScopeList, GUID� ��� ���������
      foreach (KeyValuePair<Guid, FiasGuidInfo> pair in dictGuid)
      {
        if (pair.Value.Level == FiasLevel.Unknown)
        {
          if (UnknownRecIds == null)
            UnknownRecIds = new List<Guid>();
          UnknownRecIds.Add(pair.Key);
        }
      }
      IDictionary<Guid, FiasGuidInfo> dictRecId = null;
      if (UnknownRecIds != null)
        dictRecId = Source.GetRecIdInfo(UnknownRecIds.ToArray(), FiasTableType.Unknown);


      for (int i = 0; i < addresses.Length; i++)
      {
        if (addresses[i].UnknownGuid != Guid.Empty)
        {
          Guid g = addresses[i].UnknownGuid;
          if (dictGuid[g].Level == FiasLevel.Unknown)
          {
            #region ������ �� RecId

            // 25.02.2021

            if (dictRecId[g].Level == FiasLevel.Unknown)
              addresses[i].AddMessage(ErrorMessageKind.Error, "����������� GUID=" + g.ToString());
            else
            {
              FiasTableType tt = FiasTools.GetTableType(dictRecId[g].Level);
              switch (tt)
              {
                case FiasTableType.AddrOb:
                  addresses[i].AORecId = g;
                  break;
                case FiasTableType.House:
                  addresses[i].SetRecId(FiasLevel.House, g);
                  break;
                case FiasTableType.Room:
                  addresses[i].SetRecId(FiasLevel.Flat, g);
                  break;
                default:
                  throw new BugException("����������� TableType=" + tt.ToString() + " ��� Level=" + dictRecId[g].Level + " ��� ������ ��� RecId=" + g.ToString());
              }
              addresses[i].UnknownGuid = Guid.Empty;
            }

            #endregion
          }
          else
          {
            #region ������ �� AOGuid/HouseGuid/RoomGuid

            FiasTableType tt = FiasTools.GetTableType(dictGuid[g].Level);
            switch (tt)
            {
              case FiasTableType.AddrOb:
                addresses[i].AOGuid = g;
                break;
              case FiasTableType.House:
                addresses[i].SetGuid(FiasLevel.House, g);
                break;
              case FiasTableType.Room:
                addresses[i].SetGuid(FiasLevel.Flat, g);
                break;
              default:
                throw new BugException("����������� TableType=" + tt.ToString() + " ��� Level=" + dictGuid[g].Level + " ��� ������ ��� Guid=" + g.ToString());
            }
            addresses[i].UnknownGuid = Guid.Empty;

            #endregion
          }
        }
      }
    }

    /*
     * ������� ��������� ��������������� AOGuid, HouseGuid, RoomGuid, AORecId, HouseRecId � RoomRecId,
     * �������� � FiasAddress.
     * ��� AOGuid � AORecId ���������� �������������� �������� �������� ������ ������ (������ .. �����).
     * �������������� XXXGuid �������� "�����������", � XXXRecId - �������������� ������� (��������� �����). ���
     * �������, ������������ "����������" ��������������, � �������������� ������� ������������ ������ ��� ������ �������.
     * 
     * 1. ������������ ������� ������ �� �������� "����������" ��������������� AOGuid, HouseGuid, RoomGuid.
     * ���� ����� ���� �� ���� �� ���, �� ����������� ����� ������ �� ���������� �������.
     * �������������� ������ ���� �� ���������������, � ��������� ���� �������. ��������� ��������������,
     * ������������. 
     * - 1. RoomGuid
     * - 2. HouseGuid
     * - 3. AOGuid
     * 
     * 2. ����������� ������� �������������� ������ RoomRecId.
     * ���� �� �����, ��������� ������ � ������� "Room". ����������� ���� "HouseGuid" � "EndDate" (���� UseDates=true).
     * 
     * 3. ����������� ������� �������������� ������ HouseRecId.
     * ���� �� �����, ��������� ������ � ������� "House" �� ��������������. 
     * �����, ���� ��� ����� RoomRecId, �� ����� � "House" ����������� 
     * - �� "HouseGuid" + "StartDate/EndDate", ���� UseDates=true
     * - �� "HouseGuid", ���� UseDates=false (����� ���������� �������)
     * 
     * 4. ����������� ������� �������������� AORecId.
     * ���� �� �����, ��������� ������ � ������� "AddrOb" �� ��������������. 
     * �����, ���� ��� ����� HouseRecId, �� ����� � "AddrOb" ����������� AOGUID(+StartDate/EndDate), ����������� ������ �����.
     * 
     * 5. ����������� ����������� ����� ���������� ������� �������� ��������.
     * 
     * � �.3-5, ��� FiasDBSettings.UseDates=true ����������� ����� � ������ ���. ����������� ��������� ����� �������� ���� � ��������
     * StartDate:EndDate ��� ������ ���������� ������ � �������� "House" � "AddrOb". 
     * �������� ���� ������������ �� ������, ��� ������� ��� ����� ������������� RoomRecId, HouseRecId ��� AORecId:
     * - ������� �������� StartDate:EndDate �� ������.
     * - �����������, ��� ������� ���� DateTime.Today �������� � ���� ��������
     * - ���� ������� ���� �������� � ��������, �� ������������ DateTime.Today 
     * - ����� ������������ EndDate.
     */

    private void DoFillAddress(FiasAddress address, PageLoader loader)
    {
      address.ClearAuxInfo();
      address.ClearMessages();
      if (address.IsEmpty)
        return;

      #region �� Guid'�� � �� ��������������� �������

      if (!DoFillAddressByGuid(address, loader))
      {
        Guid roomRecId = address.GetRecId(FiasLevel.Flat);
        Guid houseRecId = address.GetRecId(FiasLevel.House);
        Guid aoRecId = address.AORecId;
        if (roomRecId != Guid.Empty)
          DoFillAddressByRoomRecId(address, loader);
        else if (houseRecId != Guid.Empty)
          DoFillAddressByHouseRecId(address, loader);
        else if (aoRecId != Guid.Empty)
          DoFillAddressByAORecId(address, loader);
      }

      #endregion

      DoFillAddressByNames(address, loader);

      #region �������� ������� �����

      // ������ ����� DoFillAddressXXX(), ��� ��� �������� ������������ ����� ��������
      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        string name = address.GetName(FiasTools.AllLevels[i]);
        if (!String.IsNullOrEmpty(name))
        {
          string errorText;
          if (!FiasTools.IsValidName(name, FiasTools.AllLevels[i], out errorText))
          {
            string sPrefix;
            if (FiasTools.GetTableType(FiasTools.AllLevels[i]) == FiasTableType.AddrOb)
              sPrefix = "������������ ������������. ";
            else
              sPrefix = "������������ �����. ";
            address.AddMessage(ErrorMessageKind.Error, sPrefix + errorText, FiasTools.AllLevels[i]); // 03.03.2021
          }
        }
      }

      #endregion

      #region ��������� ���������� ����� ����, ��� ��������� �����

      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        FiasLevel level = FiasTools.AllLevels[i];
        string n = address.GetName(level);
        string a = address.GetAOType(level);
        if (n.Length > 0)
        {
          if (a.Length == 0)
            address.AddMessage(ErrorMessageKind.Error, "�� ������ ��� ��������� �������", level);
          else if (!AOTypes.IsValidAOType(level, a))
            address.AddMessage(ErrorMessageKind.Error, "����������� ��� ��������� ������� \"" + a + "\"", level);
        }
        else
        {
          if (a.Length > 0)
            address.AddMessage(ErrorMessageKind.Error, "������ ��� ��������� ������� (" + a + ") ��� ������������", level);
        }
      }

      #endregion

      #region ��������� �������� ������

      if (address.PostalCode.Length > 0)
      {
        string errorText;
        if (!FiasTools.PostalCodeMaskProvider.Test(address.PostalCode, out errorText))
          address.AddMessage(ErrorMessageKind.Error, "������������ �������� ������ \"" + address.PostalCode + "\". " + errorText);
        else if (address.PostalCode[0] == '0')
          address.AddMessage(ErrorMessageKind.Error, "������������ �������� ������ \"" + address.PostalCode + "\". �������� ������ �� ����� ���������� � 0");
        if (address.NameBottomLevel == FiasLevel.Unknown)
          address.AddMessage(ErrorMessageKind.Error, "����� �������� ������, � ����� ������");
      }
      else if (!address.AutoPostalCode)
      {
        if (address.NameBottomLevel == FiasLevel.Unknown)
          address.AddMessage(ErrorMessageKind.Error, "������� ����� ������� ��������� ������� ������� ��� ������ ������"); // 23.10.2020
      }


      #endregion
    }

    #region ���������� �� "����������" GUID'��

    /// <summary>
    /// ���������� ������, ���� ���� ���� �� ���� �������� GUID
    /// </summary>
    /// <param name="address"></param>
    /// <param name="loader"></param>
    /// <returns>true, ���� ��� ������ ���� �� ���� GUID</returns>
    private bool DoFillAddressByGuid(FiasAddress address, PageLoader loader)
    {
      bool res = false;

      address.ClearMessages();

      if (DoFillAddressByRoomGuid(address, loader, !res))
        res = true;
      if (DoFillAddressByHouseGuid(address, loader, !res))
        res = true;
      if (DoFillAddressByAOGuid(address, loader, !res))
        res = true;

      if (res)
        address.Actuality = FiasActuality.Actual;

      return res;
    }

    private bool DoFillAddressByRoomGuid(FiasAddress address, PageLoader loader, bool writeRecInfo)
    {
      bool res = false;

      if (_Source.DBSettings.UseRoom)
      {
        Guid gRoom = address.GetGuid(FiasLevel.Flat);
        if (gRoom != Guid.Empty)
        {
          res = true;

          Guid parentG = loader.RoomGuidInfoDict[gRoom].ParentGuid;
          if (parentG == FiasTools.GuidNotFound)
            address.AddMessage(ErrorMessageKind.Error, "� ����������� ��������� �� ������� ������ ��� ROOMGUID=" + gRoom.ToString(), FiasLevel.Flat);
          else
          {
            FiasCachedPageRoom page = loader.RoomPages[parentG];
            _RoomExtractor.Row = page.FindRowByGuid(gRoom);

            if (_RoomExtractor.Row == null)
              throw new BugException("�� ����� ������ ��� ROOMGUID=" + gRoom.ToString() + " �� �������� " + page.ToString());

            DoFillAddressRoomPart(address, writeRecInfo);

            address.SetGuid(FiasLevel.House, parentG);
          }
        }
      }

      return res;
    }

    private void DoFillAddressRoomPart(FiasAddress address, bool writeRecInfo)
    {
      if (_RoomExtractor.FlatNumber.Length > 0)
      {
        address.SetName(FiasLevel.Flat, _RoomExtractor.FlatNumber);
        address.SetAOType(FiasLevel.Flat, FiasEnumNames.ToString(_RoomExtractor.FlatType));
      }
      else
      {
        address.SetName(FiasLevel.Flat, String.Empty);
        address.SetAOType(FiasLevel.Flat, String.Empty);
      }
      if (_RoomExtractor.RoomNumber.Length > 0)
      {
        address.SetName(FiasLevel.Room, _RoomExtractor.RoomNumber);
        address.SetAOType(FiasLevel.Room, FiasEnumNames.ToString(_RoomExtractor.RoomType));
      }
      else
      {
        address.SetName(FiasLevel.Room, String.Empty);
        address.SetAOType(FiasLevel.Room, String.Empty);
      }
      address.SetGuid(FiasLevel.Flat, _RoomExtractor.ROOMGUID);
      address.SetRecId(FiasLevel.Flat, _RoomExtractor.RecId);
      address.UnknownGuid = Guid.Empty; // 02.11.2020

      if (writeRecInfo || address.FiasPostalCode.Length == 0) // ���� ������ ��� ������ ��� ����� �� �� ���� �������� �� ���������� ������ ��� ������
        address.FiasPostalCode = _RoomExtractor.PostalCode;

      if (writeRecInfo)
      {
        address.StartDate = _RoomExtractor.StartDate;
        address.EndDate = _RoomExtractor.EndDate;
        address.Live = _RoomExtractor.Live;
      }
    }

    private bool DoFillAddressByHouseGuid(FiasAddress address, PageLoader loader, bool writeRecInfo)
    {
      bool res = false;

      if (_Source.DBSettings.UseHouse)
      {
        Guid gHouse = address.GetGuid(FiasLevel.House);
        if (gHouse != Guid.Empty)
        {
          res = true;

          Guid parentG = loader.HouseGuidInfoDict[gHouse].ParentGuid;
          if (parentG == FiasTools.GuidNotFound)
            address.AddMessage(ErrorMessageKind.Error, "� ����������� ������ �� ������� ������ ��� HOUSEGUID=" + gHouse.ToString(), FiasLevel.House);
          else
          {
            FiasLevel parentLevel;
            FiasGuidInfo info;
            if (loader.HouseGuidInfoDict.TryGetValue(parentG, out info)) // ����������� 22.10.2020
              parentLevel = info.Level;
            else
              parentLevel = GetAOGuidLevel(parentG);
            if (!FiasTools.IsInheritableLevel(parentLevel, FiasLevel.House, false))
              address.AddHouseMessage(ErrorMessageKind.Error, "��� �� ����� ���� ����� ��� ��������� ������� � ������� [" + FiasEnumNames.ToString(parentLevel, true) + "]");

            FiasCachedPageHouse page = loader.HousePages[parentG];
            _HouseExtractor.Row = page.FindRowByGuid(gHouse);

            if (_HouseExtractor.Row == null)
              throw new BugException("�� ����� ������ ��� HOUSEGUID=" + gHouse.ToString() + " �� �������� " + page.ToString());

            DoFillAddressHousePart(address, writeRecInfo);

            address.AOGuid = parentG;
          }
        }
      }

      return res;
    }

    private void DoFillAddressHousePart(FiasAddress address, bool writeRecInfo)
    {
      if (_HouseExtractor.HouseNum.Length > 0)
      {
        address.SetName(FiasLevel.House, _HouseExtractor.HouseNum);
        address.SetAOType(FiasLevel.House, FiasEnumNames.ToString(_HouseExtractor.EstStatus));
      }
      else
      {
        address.SetName(FiasLevel.House, String.Empty);
        address.SetAOType(FiasLevel.House, String.Empty);
      }

      if (_HouseExtractor.BuildNum.Length > 0)
      {
        address.SetName(FiasLevel.Building, _HouseExtractor.BuildNum);
        address.SetAOType(FiasLevel.Building, "������");
      }
      else
      {
        address.SetName(FiasLevel.Building, String.Empty);
        address.SetAOType(FiasLevel.Building, String.Empty);
      }

      if (_HouseExtractor.StrucNum.Length > 0)
      {
        address.SetName(FiasLevel.Structure, _HouseExtractor.StrucNum);
        address.SetAOType(FiasLevel.Structure, FiasEnumNames.ToString(_HouseExtractor.StrStatus));
      }
      else
      {
        address.SetName(FiasLevel.Structure, String.Empty);
        address.SetAOType(FiasLevel.Structure, String.Empty);
      }

      address.SetGuid(FiasLevel.House, _HouseExtractor.HOUSEGUID);
      address.SetRecId(FiasLevel.House, _HouseExtractor.RecId);
      address.UnknownGuid = Guid.Empty; // 02.11.2020

      if (writeRecInfo || address.FiasPostalCode.Length == 0) // ���� ������ ��� ������ ��� ����� �� �� ���� �������� �� ���������� ������ ��� ������
        address.FiasPostalCode = _HouseExtractor.PostalCode;

      if (writeRecInfo || address.OKATO.Length == 0)
        address.OKATO = _HouseExtractor.OKATO;
      if (writeRecInfo || address.OKTMO.Length == 0)
        address.OKTMO = _HouseExtractor.OKTMO;
      if (writeRecInfo || address.IFNSFL.Length == 0)
        address.IFNSFL = _HouseExtractor.IFNSFL;
      if (writeRecInfo || address.IFNSUL.Length == 0)
        address.IFNSUL = _HouseExtractor.IFNSUL;

      if (writeRecInfo)
      {
        address.StartDate = _HouseExtractor.StartDate;
        address.EndDate = _HouseExtractor.EndDate;
      }
    }

    private bool DoFillAddressByAOGuid(FiasAddress address, PageLoader loader, bool writeRecInfo)
    {
      bool res = false;
      bool first = true;

      Guid gAO = address.AOGuid;
      while (gAO != Guid.Empty)
      {
        res = true;

        Guid parentG = loader.AOGuidInfoDict[gAO].ParentGuid;
        if (parentG == FiasTools.GuidNotFound)
        {
          address.AddMessage(ErrorMessageKind.Error, "� ����������� �������� �������� �� ������� ������ ��� AOGUID=" + gAO.ToString());
          break;
        }

        _AddrObExtractor.Row = null;
        foreach (KeyValuePair<FiasLevel, FiasCachedPageAddrOb> pair in loader.AOPages[parentG])
        {
          _AddrObExtractor.Row = pair.Value.FindRowByGuid(gAO);
          if (_AddrObExtractor.Row != null)
          {
            FiasLevel parentLevel;
            FiasGuidInfo info;
            if (parentG == Guid.Empty)
              parentLevel = FiasLevel.Unknown;
            else if (loader.AOGuidInfoDict.TryGetValue(parentG, out info)) // ����������� 22.10.2020
              parentLevel = info.Level;
            else
              parentLevel = GetAOGuidLevel(parentG);
            if (!FiasTools.IsInheritableLevel(parentLevel, pair.Key, false))
            {
              if (parentLevel == FiasLevel.Unknown)
                address.AddMessage(ErrorMessageKind.Error, "�������� ������ � ������� [" + FiasEnumNames.ToString(pair.Key, true) + "] �� ����� ���� �������� �������� ������", pair.Key);
              else
                address.AddMessage(ErrorMessageKind.Error, "�������� ������ � ������� [" + FiasEnumNames.ToString(pair.Key, true) + "] �� ����� ����� �������� � ������� [" + FiasEnumNames.ToString(parentLevel, true) + "]", pair.Key);
            }
            break;
          }
        }

        if (_AddrObExtractor.Row == null)
          throw new BugException("�� ����� ������ ��� AOGUID=" + gAO.ToString() + " �� ��������� ��� ������������� ������� " + parentG.ToString());


        if (first)
        {
          // ������� ��� ������������ ��� ������������� �������, �� ������� �� ���� �������������
          // ��������, � ������ ���� ������ "��������� �������", "��������� �����" � 
          // GUID {9ae64229-9f7b-4149-b27a-d1f6ec74b5ce} ��� ������ ������
          // ������ �������� ������ "������" � "�����", � ������� "�����" ��������� ��� ���������

          first = false;
          //address.ClearAOLevels();
        }

        DoFillAddressAOPart(address, writeRecInfo);
        writeRecInfo = false;

        gAO = parentG;
      }

      return res;
    }

    private void DoFillAddressAOPart(FiasAddress address, bool writeRecInfo)
    {

      address.SetName(_AddrObExtractor.Level, _AddrObExtractor.Name);
      address.SetAOType(_AddrObExtractor.Level, AOTypes.GetAOType(_AddrObExtractor.AOTypeId, FiasAOTypeMode.Full));
      address.SetGuid(_AddrObExtractor.Level, _AddrObExtractor.AOGUID);
      address.SetRecId(_AddrObExtractor.Level, _AddrObExtractor.RecId);
      address.AOGuid = Guid.Empty; // 02.11.2020
      address.AORecId = Guid.Empty; // 02.11.2020
      address.UnknownGuid = Guid.Empty; // 02.11.2020

      if (writeRecInfo || address.FiasPostalCode.Length == 0) // ���� ������ ��� ������ ��� ����� �� �� ���� �������� �� ���������� ������ ��� ������
        address.FiasPostalCode = _AddrObExtractor.PostalCode;
      if (writeRecInfo || address.OKATO.Length == 0)
        address.OKATO = _AddrObExtractor.OKATO;
      if (writeRecInfo || address.OKTMO.Length == 0)
        address.OKTMO = _AddrObExtractor.OKTMO;
      if (writeRecInfo || address.IFNSFL.Length == 0)
        address.IFNSFL = _AddrObExtractor.IFNSFL;
      if (writeRecInfo || address.IFNSUL.Length == 0)
        address.IFNSUL = _AddrObExtractor.IFNSUL;

      if (writeRecInfo)
      {
        address.Live = _AddrObExtractor.Live;
        address.StartDate = _AddrObExtractor.StartDate;
        address.EndDate = _AddrObExtractor.EndDate;
      }

      if (_AddrObExtractor.Level == FiasLevel.Region) // ���������� �� ��������� writeRecInfo
        address.RegionCode = _AddrObExtractor.RegionCode; // 09.08.2020
    }

    #endregion

    #region ���������� �� "������������" ��������������� �������

    private void DoFillAddressByAORecId(FiasAddress address, PageLoader loader)
    {
      Guid recId = address.AORecId; // ����� ���� ����� �������� ��� �� �����-���� ������

      Guid parentG = loader.AORecIdInfoDict[recId].ParentGuid;
      if (parentG == FiasTools.GuidNotFound)
      {
        address.AddMessage(ErrorMessageKind.Error, "� ����������� �������� �������� �� ������� ������ ��� AOID=" + recId.ToString());
        return;
      }
      _AddrObExtractor.Row = null;
      foreach (KeyValuePair<FiasLevel, FiasCachedPageAddrOb> pair in loader.AOPages[parentG])
      {
        _AddrObExtractor.Row = pair.Value.FindRowByRecId(recId);
        if (_AddrObExtractor.Row != null)
        {
          FiasLevel parentLevel = GetAOGuidLevel(parentG);
          if (!FiasTools.IsInheritableLevel(parentLevel, pair.Key, false))
          {
            if (parentLevel == FiasLevel.Unknown)
              address.AddMessage(ErrorMessageKind.Error, "�������� ������ � ������� [" + FiasEnumNames.ToString(pair.Key, true) + "] �� ����� ���� �������� �������� ������", pair.Key);
            else
              address.AddMessage(ErrorMessageKind.Error, "�������� ������ � ������� [" + FiasEnumNames.ToString(pair.Key, true) + "] �� ����� ����� �������� � ������� [" + FiasEnumNames.ToString(parentLevel, true) + "]", pair.Key);
          }
          break;
        }
      }

      if (_AddrObExtractor.Row == null)
        throw new BugException("�� ����� ������ ��� AOID=" + recId.ToString() + " �� ��������� ��� ������������� ������� " + parentG.ToString());

      // ���� ������� ��� ������, ������� � ������� � ������ �������.
      // ����������� ������ ������� ������. ��� ����� ���� ��������� ������������, ������� ���� ����� �����

      //address.ClearAOLevels();
      int pLastLevel = FiasTools.AOLevelIndexer.IndexOf(_AddrObExtractor.Level);
      if (pLastLevel < 0)
        throw new BugException("������� ������ � ������������ LEVEL=" + _AddrObExtractor.Level.ToString());
      for (int i = 0; i <= pLastLevel; i++)
        address.ClearLevel(FiasTools.AOLevels[i]);

      DoFillAddressAOPart(address, true);
      FiasActuality actuality = _AddrObExtractor.Actual ? FiasActuality.Actual : FiasActuality.Historical; // ���������� �� ��������� ��������� ������
      if (actuality == FiasActuality.Actual && _Source.DBSettings.UseDates)
      {
        if (!DataTools.DateInRange(_Today, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
          actuality = FiasActuality.Historical;
      }

      if (parentG != Guid.Empty)
      {
        address.AOGuid = parentG;
        DoFillAddressByAOGuid(address, loader, false);
        address.AOGuid = Guid.Empty;
      }
      address.Actuality = actuality;

      //if (_Source.DBSettings.UseDates)
      //{
      //  testDate = DateTime.Today;
      //  if (!DataTools.DateInRange(testDate.Value, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
      //    testDate = _AddrObExtractor.EndDate;
      //}

      //DoFillAddressByParentAOGuid(address, loader, parentG, testDate);
    }

    private void DoFillAddressByHouseRecId(FiasAddress address, PageLoader loader)
    {
      Guid recId = address.GetRecId(FiasLevel.House);
      if (recId == Guid.Empty)
        throw new BugException("HOUSEID is empty");

      Guid parentG = loader.HouseRecIdInfoDict[recId].ParentGuid;
      if (parentG == FiasTools.GuidNotFound)
      {
        address.AddMessage(ErrorMessageKind.Error, "� ����������� ����� �� ������� ������ ��� HOUSEID=" + recId.ToString());
        return;
      }

      FiasLevel parentLevel = GetAOGuidLevel(parentG);
      if (!FiasTools.IsInheritableLevel(parentLevel, FiasLevel.House, false))
        address.AddHouseMessage(ErrorMessageKind.Error, "��� �� ����� ���� ����� ��� ��������� ������� � ������� [" + FiasEnumNames.ToString(parentLevel, true) + "]");

      try
      {
        _HouseExtractor.Row = loader.HousePages[parentG].FindRowByRecId(recId);
      }
      catch (Exception e)
      {
        e.Data["AOGUID"] = parentG;
      }
      if (_HouseExtractor.Row == null)
        throw new BugException("�� ����� ������ ��� HOUSEID=" + recId.ToString() + " �� �������� ��� ������������� ������� " + parentG.ToString());

      address.AOGuid = Guid.Empty; // ������� "��������������" GUID
      address.AORecId = Guid.Empty;

      DoFillAddressHousePart(address, true);
      FiasActuality actuality = FiasActuality.Actual; // ���������� �� ��������� ��������� ������
      if (_Source.DBSettings.UseDates)
      {
        if (!DataTools.DateInRange(_Today, _HouseExtractor.StartDate, _HouseExtractor.EndDate))
          actuality = FiasActuality.Historical;
      }

      if (parentG != Guid.Empty)
      {
        address.AOGuid = parentG;
        DoFillAddressByAOGuid(address, loader, false);
      }
      address.Actuality = actuality;

      //if (_Source.DBSettings.UseDates)
      //{
      //  testDate = DateTime.Today;
      //  if (!DataTools.DateInRange(testDate.Value, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
      //    testDate = _AddrObExtractor.EndDate;
      //}

      //DoFillAddressByParentAOGuid(address, loader, parentG, testDate);
    }

    private void DoFillAddressByRoomRecId(FiasAddress address, PageLoader loader)
    {
      Guid recId = address.GetRecId(FiasLevel.Flat);
      if (recId == Guid.Empty)
        throw new BugException("ROOMID is empty");

      Guid parentG = loader.RoomRecIdInfoDict[recId].ParentGuid;
      if (parentG == FiasTools.GuidNotFound)
      {
        address.AddMessage(ErrorMessageKind.Error, "� ����������� ��������� �� ������� ������ ��� ROOMID=" + recId.ToString());
        return;
      }

      try
      {
        _RoomExtractor.Row = loader.RoomPages[parentG].FindRowByRecId(recId);
      }
      catch (Exception e)
      {
        e.Data["HOUSEGUID"] = parentG;
      }
      if (_RoomExtractor.Row == null)
        throw new BugException("�� ����� ������ ��� ROOMID=" + recId.ToString() + " �� �������� ��� ������������� ������� " + parentG.ToString());

      address.AOGuid = Guid.Empty; // ������� "��������������" GUID
      address.AORecId = Guid.Empty;
      address.SetGuid(FiasLevel.House, Guid.Empty);
      address.SetRecId(FiasLevel.House, Guid.Empty);

      DoFillAddressRoomPart(address, true);
      FiasActuality actuality = FiasActuality.Actual; // ���������� �� ��������� ��������� ������
      if (_Source.DBSettings.UseDates)
      {
        if (!DataTools.DateInRange(_Today, _RoomExtractor.StartDate, _RoomExtractor.EndDate))
          actuality = FiasActuality.Historical;
      }

      if (parentG != Guid.Empty)
      {
        address.SetGuid(FiasLevel.House, parentG);
        DoFillAddressByHouseGuid(address, loader, false);
        DoFillAddressByAOGuid(address, loader, false);
      }
      address.Actuality = actuality;

      //if (_Source.DBSettings.UseDates)
      //{
      //  testDate = DateTime.Today;
      //  if (!DataTools.DateInRange(testDate.Value, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
      //    testDate = _AddrObExtractor.EndDate;
      //}

      //DoFillAddressByParentAOGuid(address, loader, parentG, testDate);
    }

#if XXX
    private void DoFillAddressByParentAOGuid(FiasAddress address, PageLoader loader, Guid gAO, DateTime? testDate)
    {
      while (gAO != Guid.Empty)
      {
        Guid parentG = loader.AOParentGuidDict[gAO];
        if (parentG == FiasTools.GuidNotFound)
        {
          address.AddMessage(ErrorMessageKind.Error, "� ����������� �������� �������� �� ������� ������ ��� AOGUID=" + gAO.ToString());
          break;
        }
        _AddrObExtractor.Row = null;
        foreach (KeyValuePair<FiasLevel, FiasCachedPageAddrOb> pair in loader.AOPages[parentG])
        {
          _AddrObExtractor.Row = pair.Value.FindRowByGuid(gAO);
          if (_AddrObExtractor.Row != null)
            break;
        }

        if (_AddrObExtractor.Row == null)
          throw new BugException("�� ����� ������ ��� AOGUID=" + gAO.ToString() + " �� ��������� ��� ������������� ������� " + parentG.ToString());

        address.AOGuid = Guid.Empty; // ������� "��������������" GUID
        address.AORecId = Guid.Empty;

        DoFillAddressAOPart(address);

        gAO = parentG;
      }
    }
#endif
    #endregion

    #region �� ������������� � �����������

    private void DoFillAddressByNames(FiasAddress address, PageLoader loader)
    {
      bool HasErrors = false;
      FiasLevel BottomLevel = address.GuidBottomLevel;

      int i = FiasTools.AllLevelIndexer.IndexOf(BottomLevel) + 1;
      while (i < FiasTools.AOLevels.Length)
      {
        FiasLevel thisLevel = FiasTools.AOLevels[i];

        string name = address.GetName(thisLevel);
        string aoType = address.GetAOType(thisLevel);
        if (name.Length == 0)
        {
          address.ClearLevel(thisLevel);
          i++;
          continue;
        }

        FiasSearchRowResult searchRes = FiasSearchRowResult.NotFound;
        _AddrObExtractor.Row = null;

        for (int j = i; j < FiasTools.AOLevels.Length; j++)
        {
          if (j > i && address.GetName(FiasTools.AOLevels[j]).Length > 0)
            break; // ��������� ����������� �������

          Guid pageAOGuid = Guid.Empty;
          if (BottomLevel != FiasLevel.Unknown)
            pageAOGuid = address.GetGuid(BottomLevel);

          FiasCachedPageAddrOb page = loader.GetAOPage(pageAOGuid, FiasTools.AOLevels[j]);
          Int32 aoTypeId = AOTypes.FindAOTypeId(FiasTools.AOLevels[j], aoType);
          searchRes = page.FindRow(name, aoTypeId);
          if (searchRes.Count == FiasSearchRowCount.Ok)
          {
            // ������� ������ �� "������������"
            for (int k = i; k < j; k++)
              address.ClearLevel(FiasTools.AOLevels[k]);

            i = j;
            break;
          }
        }

        //FiasLevel testLevel = BottomLevel;
        FiasLevel testLevel = address.GetPrevLevel(thisLevel); // 02.10.2020
        if (!FiasTools.IsInheritableLevel(testLevel, thisLevel, false))
        {
          if (testLevel == FiasLevel.Unknown)
            address.AddMessage(ErrorMessageKind.Error, "�������� ������ � ������� [" + FiasEnumNames.ToString(thisLevel, true) + "] �� ����� ���� �������� �������� ������", thisLevel);
          else
            address.AddMessage(ErrorMessageKind.Error, "�������� ������ � ������� [" + FiasEnumNames.ToString(thisLevel, true) + "] �� ����� ����� �������� � ������� [" + FiasEnumNames.ToString(testLevel, true) + "]", thisLevel);
          HasErrors = true;
        }
        else
        {
          switch (searchRes.Count)
          {
            case FiasSearchRowCount.Ok:
              _AddrObExtractor.Row = searchRes.Row;
              DoFillAddressAOPart(address, true);
              BottomLevel = _AddrObExtractor.Level;
              break;
            case FiasSearchRowCount.NotFound:
              address.AddMessage(ErrorMessageKind.Warning, "�� ������ �������� ������ ��� ������ [" + FiasEnumNames.ToString(thisLevel, false) + "]: \"" + name + " " + aoType + "\"", FiasTools.AOLevels[i]);
              HasErrors = true;
              break;
            case FiasSearchRowCount.Multi:
              address.AddMessage(ErrorMessageKind.Warning, "������� ������ ������ ��������� ������� ��� ������ [" + FiasEnumNames.ToString(thisLevel, false) + "]: \"" + name + " " + aoType + "\"", FiasTools.AOLevels[i]);
              HasErrors = true;
              break;
          }
        }
        i++;
      }

      string houseNum = address.GetName(FiasLevel.House);
      string buildNum = address.GetName(FiasLevel.Building);
      string strNum = address.GetName(FiasLevel.Structure);

      //string errorText;
      //if (!NumValidator.IsValidNum(houseNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "������������ ����� ���� \"" + houseNum + "\". " + errorText, FiasLevel.House);
      //  HasErrors = true;
      //}
      //if (!NumValidator.IsValidNum(buildNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "������������ ����� ������� \"" + buildNum + "\". " + errorText, FiasLevel.Building);
      //  HasErrors = true;
      //}
      //if (!NumValidator.IsValidNum(strNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "������������ ����� �������� \"" + strNum + "\". " + errorText, FiasLevel.Structure);
      //  HasErrors = true;
      //}

      bool HasHouse = (houseNum.Length + buildNum.Length + strNum.Length) > 0;
      if (HasHouse)
      {
        if (address.GetName(FiasLevel.Village).Length +
          address.GetName(FiasLevel.PlanningStructure).Length /* 02.10.2020 */ +
          address.GetName(FiasLevel.Street).Length == 0)
        {
          address.AddHouseMessage(ErrorMessageKind.Error, "������ ������, �� �� ����� �� ���������� �����, �� �����");
          HasErrors = true; // 26.10.2020
        }

        if (!HasErrors)
        {
          if (_Source.DBSettings.UseHouse)
          {
            FiasCachedPageHouse page = loader.GetHousePage(address.AOGuid);

            string houseAOType = GetCorrectedAOType(address, FiasLevel.House);
            FiasEstateStatus estStatus = FiasEnumNames.ParseEstateStatus(houseAOType);

            GetCorrectedAOType(address, FiasLevel.Building);

            string strAOType = GetCorrectedAOType(address, FiasLevel.Structure);
            FiasStructureStatus strStatus = FiasEnumNames.ParseStructureStatus(strAOType);

            _HouseExtractor.Row = page.FindRow(houseNum, estStatus, buildNum, strNum, strStatus);
            if (_HouseExtractor.Row != null)
              DoFillAddressHousePart(address, true);
            else
            {
              FiasLevel parentLevel = address.GuidBottomLevel;
              string rbName = "��� ������ [" + FiasEnumNames.ToString(parentLevel, false) + "] \"" + address[parentLevel] + "\"";
              if (!page.IsEmpty)
                address.AddHouseMessage(ErrorMessageKind.Warning, "�� ������� ������ \"" +
                  FiasCachedPageHouse.GetText(houseNum, estStatus, buildNum, strNum, strStatus) +
                  "\", ���� � ����������� " + rbName + " ���� ������ ������");
              else
              {
                address.AddHouseMessage(ErrorMessageKind.Info, "���������� " + rbName + " �� �������� ������");

                Guid gVillage = address.GetGuid(FiasLevel.Village);
                if (gVillage != Guid.Empty && address.GetName(FiasLevel.Street).Length == 0)
                {
                  // ���������� ����� ������ �� �����������, � ����� �� ������
                  FiasCachedPageAddrOb pageVillage = loader.GetAOPage(gVillage, FiasLevel.Street);
                  if (!pageVillage.IsEmpty)
                    address.AddMessage(ErrorMessageKind.Warning, "�� ������ �����, ���� � ���� ���� ����� ��� ����������� ������ \"" + address[FiasLevel.Village] + "\"", FiasLevel.Street); // 26.10.2020
                }
              }
              HasErrors = true;
            }
          }
        } // HasErrors
      }

      string flatNum = address.GetName(FiasLevel.Flat);
      string roomNum = address.GetName(FiasLevel.Room);

      //if (!NumValidator.IsValidNum(flatNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "������������ ����� �������� \"" + flatNum + "\". " + errorText, FiasLevel.Flat);
      //  HasErrors = true;
      //}
      //if (!NumValidator.IsValidNum(roomNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "������������ ����� ��������� \"" + strNum + "\". " + errorText, FiasLevel.Room);
      //  HasErrors = true;
      //}

      bool HasRoom = (flatNum.Length + roomNum.Length) > 0;
      if (HasRoom)
      {
        if (!HasHouse)
        {
          address.AddRoomMessage(ErrorMessageKind.Error, "������ ���������, �� �� ������ ������");
          HasErrors = true; // 26.10.2020
        }
        if (_Source.DBSettings.UseRoom && (!HasErrors))
        {
          string flatAOType = GetCorrectedAOType(address, FiasLevel.Flat);
          FiasFlatType flatType = FiasEnumNames.ParseFlatType(flatAOType);

          string roomAOType = GetCorrectedAOType(address, FiasLevel.Room);
          FiasRoomType roomType = FiasEnumNames.ParseRoomType(roomAOType);

          FiasCachedPageRoom page = loader.GetRoomPage(address.GetGuid(FiasLevel.House));
          _RoomExtractor.Row = page.FindRow(flatNum, flatType, roomNum, roomType);
          if (_RoomExtractor.Row != null)
          {
            DoFillAddressRoomPart(address, true);
          }
          else
          {
            if (!page.IsEmpty)
              address.AddRoomMessage(ErrorMessageKind.Warning, "�� ������� ��������� \"" +
                FiasCachedPageRoom.GetText(flatNum, flatType, roomNum, roomType) +
                "\" � ����������� ��� ������, ���� ������ ��������� ����");
            else
              address.AddHouseMessage(ErrorMessageKind.Info, "���������� ������ �� �������� ���������");
            HasErrors = true;
          }
        }
      }
    }

    /// <summary>
    /// ���������� �� ������ ��� �������� ��� ��������� ������.
    /// ���� �������� ������, � ��� - ���, �� ��������������� ��� �� ���������, ��������, "���".
    /// </summary>
    /// <param name="address"></param>
    /// <param name="level"></param>
    /// <returns>������������ ������������</returns>
    private static string GetCorrectedAOType(FiasAddress address, FiasLevel level)
    {
      string name = address.GetName(level);
      string aoType = address.GetAOType(level);
      if ((name.Length > 0) != (aoType.Length > 0))
      {
        if (name.Length > 0)
        {
          aoType = FiasTools.GetDefaultAOType(level);
#if DEBUG
          if (aoType.Length == 0)
            throw new BugException("�� ��������� ��� ����������������� �������� �� ���������");
#endif
        }
        else
          aoType = String.Empty;
        address.SetAOType(level, aoType); // ���������
      }
      return aoType;
    }

    #region ������������ �������

#if XXX // ���������� FiasTools.IsValidName()
    private static class NumValidator
    {
    #region ����������� �����������

      static NumValidator()
      {
        string s1 = "�����Ũ�������������������������0123456789";
        string s2 = "-/.";

        _AllValidChars = new CharArrayIndexer(s1 + s2, true);
        _SpecChars = new CharArrayIndexer(s2, false);
      }

      /// <summary>
      /// ��� �������, ������� ����� ���� � ������
      /// </summary>
      private static CharArrayIndexer _AllValidChars;

      /// <summary>
      /// ������� �� ������ _AllValidChars, ������� �� ����� ���� � ������ ��� � �����, � ����� ������������� ������
      /// (���, ����� ���� � ����)
      /// </summary>
      private static CharArrayIndexer _SpecChars;

    #endregion

    #region ��������

      /// <summary>
      /// ��������� ������������ ������ ����/�������/��������/��������/���������.
      /// ������ ������ ��������� ����������.
      /// </summary>
      /// <param name="s">����������� �����</param>
      /// <param name="errorText">���� ������������ ��������� �� ������</param>
      /// <returns>true, ���� ����� ����������</returns>
      public static bool IsValidNum(string s, out string errorText)
      {
        if (String.IsNullOrEmpty(s))
        {
          errorText = null;
          return true;
        }

        for (int i = 0; i < s.Length; i++)
        {
          if (!_AllValidChars.Contains(s[i]))
          {
            errorText = "������������ ������ \"" + s[i] + "\" � ������� " + (i + 1).ToString();
            return false;
          }

          if (_SpecChars.Contains(s[i]))
          {
            if (i == 0 || i == (s.Length - 1))
            {
              errorText = "����� �� ����� ���������� ��� ������������� �� ������ \"" + s[i] + "\"";
              return false;
            }

            if (i > 0)
            {
              if (_SpecChars.Contains(s[i - 1]))
              {
                if (s[i - 1] == s[i])
                  errorText = "��� ������� \"" + s.Substring(i - 1, 2) + "\" ������ � ������� " + i.ToString();
                else
                  errorText = "������������ ��������� �������� \"" + s.Substring(i - 1, 2) + "\" ������ � ������� " + i.ToString();
                return false;
              }
            }
          }
        }
        errorText = null;
        return true;
      }

    #endregion
    }
#endif

    #endregion

    #endregion

    #endregion

    #region �������� �������������� �������

    private class PageLoader
    {
      #region �����������

      internal PageLoader(IFiasSource source)
      {
        _Source = source;
        AOGuids = new SingleScopeList<Guid>();
        AORecIds = new SingleScopeList<Guid>();
        if (source.DBSettings.UseHouse)
        {
          HouseGuids = new SingleScopeList<Guid>();
          HouseRecIds = new SingleScopeList<Guid>();
        }
        if (source.DBSettings.UseRoom)
        {
          RoomGuids = new SingleScopeList<Guid>();
          RoomRecIds = new SingleScopeList<Guid>();
        }
      }

      #endregion

      #region ��������

      private readonly IFiasSource _Source;

      /// <summary>
      /// ���� ����� �������� �������������� �������� ��������, �� ������� ������ ������ � �������
      /// </summary>
      internal readonly SingleScopeList<Guid> AOGuids;
      /// <summary>
      /// ���� ����� �������� �������������� ������, �� ������� ������ ������ � �������
      /// </summary>
      internal readonly SingleScopeList<Guid> HouseGuids;
      /// <summary>
      /// ���� ����� �������� �������������� ���������, �� ������� ������ ������ � �������
      /// </summary>
      internal readonly SingleScopeList<Guid> RoomGuids;

      /// <summary>
      /// ���� ����� �������� �������������� ������� �������� ��������, �� ������� ������ ������ � �������.
      /// </summary>
      internal readonly SingleScopeList<Guid> AORecIds;

      /// <summary>
      /// ���� ����� �������� �������������� ������� ������, �� ������� ������ ������ � �������
      /// </summary>
      internal readonly SingleScopeList<Guid> HouseRecIds;

      /// <summary>
      /// ���� ����� �������� �������������� ������� ���������, �� ������� ������ ������ � �������
      /// </summary>
      internal readonly SingleScopeList<Guid> RoomRecIds;

      /// <summary>
      /// ����� ������ ������ Load() ���� ����� ��������� ���� "��������-��������-������: FiasGuidInfo".
      /// ������ �� ���������, ���� � AOGuids �� ���� ��������� �� ������ ��������������.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> AOGuidInfoDict;

      /// <summary>
      /// ����� ������ ������ Load() ���� ����� ��������� ���� "GUID-������: FiasGuidInfo".
      /// ������ �� ���������, ���� � HouseGuids �� ���� ��������� �� ������ ��������������.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> HouseGuidInfoDict;

      /// <summary>
      /// ����� ������ ������ Load() ���� ����� ��������� ���� "GUID-���������: FiasGuidInfo".
      /// ������ �� ���������, ���� � RoomGuids �� ���� ��������� �� ������ ��������������.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> RoomGuidInfoDict;

      /// <summary>
      /// ����� ������ ������ Load() ���� ����� ��������� ���� "RecId-���������-���������-�������: FiasGuidInfo".
      /// ������ �� ���������, ���� � AORecIds �� ���� ��������� �� ������ ��������������.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> AORecIdInfoDict;

      /// <summary>
      /// ����� ������ ������ Load() ���� ����� ��������� ���� "RecId-������: FiasGuidInfo".
      /// ������ �� ���������, ���� � HouseRecIds �� ���� ��������� �� ������ ��������������.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> HouseRecIdInfoDict;

      /// <summary>
      /// ����� ������ ������ Load() ���� ����� ��������� ���� "RecId-���������: FiasGuidInfo".
      /// ������ �� ���������, ���� � RoomRecIds �� ���� ��������� �� ������ ��������������.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> RoomRecIdInfoDict;

      /// <summary>
      /// ����� ������ ������ Load() ����� ����� ��� ������ �������������� �������� �������� ��������, �������
      /// ������������ ��������, �� ����������� �������� ������������.
      /// ������ ������ �������� GUID ������������� �������
      /// ������ ������ �������� ������� �������� ��������, ������� ���� � ������� ��� ������ �������� (������ - �����)
      /// ��������� �������� �������������� ��������, � ������� �������� Level ��������� �� ������ ������ �������
      /// </summary>
      internal Dictionary<Guid, Dictionary<FiasLevel, FiasCachedPageAddrOb>> AOPages;

      /// <summary>
      /// ����� ������ ������ Load() ����� ����� ��� ������ �������������� �������� ������.
      /// ���� - ������������� ������������� ��������� �������
      /// �������� - �������� �� ������� ������
      /// </summary>
      internal Dictionary<Guid, FiasCachedPageHouse> HousePages;

      /// <summary>
      /// ����� ������ ������ Load() ����� ����� ��� ������ �������������� �������� ���������.
      /// ���� - ������������� ������.
      /// �������� - �������� �� ������� ���������.
      /// </summary>
      internal Dictionary<Guid, FiasCachedPageRoom> RoomPages;

      /// <summary>
      /// ��������������� � true, ���� ������ �������� ������ �����
      /// </summary>
      private bool _HaveHouses;

      /// <summary>
      /// ��������������� � true, ���� ������ �������� ������ ���������
      /// </summary>
      private bool _HaveRooms;

      #endregion

      #region ���������� ������ GUID'��

      /// <summary>
      /// ��������� �� ������ ��� GUID�, ����� UnknownGuid, � ��������� �� � ������ XXGuids.
      /// </summary>
      /// <param name="address"></param>
      public void AddGuids(FiasAddress address)
      {
        if (!DoAddGuids(address))
          DoAddRecIds(address);

        if (!_HaveHouses && address.ContainsHouseNum)
          _HaveHouses = true;
        if (!_HaveRooms && address.ContainsRoomNum)
          _HaveRooms = true;

      }

      private bool DoAddGuids(FiasAddress address)
      {
        bool res = false;

        Guid g;

        g = address.AOGuid;
        if (g != Guid.Empty)
        {
          AOGuids.Add(g);
          res = true;
        }

        if (_Source.DBSettings.UseRoom)
        {
          g = address.GetGuid(FiasLevel.Flat);
          if (g != Guid.Empty)
          {
            RoomGuids.Add(g);
            res = true;
          }
        }
        if (_Source.DBSettings.UseHouse)
        {
          g = address.GetGuid(FiasLevel.House);
          if (g != Guid.Empty)
          {
            HouseGuids.Add(g);
            res = true;
          }
        }

        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          g = address.GetGuid(FiasTools.AOLevels[i]);
          if (g != Guid.Empty)
          {
            AOGuids.Add(g);
            res = true;
          }
        }

        return res;
      }

      /// <summary>
      /// ��������� �� ������ ��� �������������� �������, � ��������� �� � ������ XXRecIds.
      /// </summary>
      /// <param name="address"></param>
      private bool DoAddRecIds(FiasAddress address)
      {
        bool res = false;

        Guid g;

        g = address.AORecId;
        if (g != Guid.Empty)
        {
          AORecIds.Add(g);
          res = true;
        }

        if (_Source.DBSettings.UseRoom)
        {
          g = address.GetRecId(FiasLevel.Flat);
          if (g != Guid.Empty)
          {
            RoomRecIds.Add(g);
            res = true;
          }
        }

        if (_Source.DBSettings.UseHouse)
        {
          g = address.GetRecId(FiasLevel.House);
          if (g != Guid.Empty)
          {
            HouseRecIds.Add(g);
            res = true;
          }
        }

        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          g = address.GetRecId(FiasTools.AOLevels[i]);
          if (g != Guid.Empty)
          {
            AORecIds.Add(g);
            res = true;
          }
        }

        return res;
      }

      #endregion

      #region �������� �������

      /// <summary>
      /// ��������� �������� �������, ��������, ������� GetParentAOGuids() � GetPages() ��������� ��� 
      /// </summary>
      internal void Load(ISplash spl)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.PageLoader.Load() started.");

        #region Room

        if (RoomGuids != null)
        {
#if DEBUG
          if (HouseGuids == null)
            throw new BugException("HouseGuids==null ��� RoomGuids!=null");
#endif

          RoomPages = new Dictionary<Guid, FiasCachedPageRoom>();
          if (RoomGuids.Count > 0 || RoomRecIds.Count > 0)
          {
            #region ����� GetParentGuids()

            spl.PhaseText = "��������� GUID�� ������ ��� ��������� (" + (RoomGuids.Count + RoomRecIds.Count).ToString() + ")";

            SingleScopeList<Guid> PageGuids = new SingleScopeList<Guid>();
            if (RoomGuids.Count > 0)
            {
              RoomGuidInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetGuidInfo(RoomGuids.ToArray(), FiasTableType.Room);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                RoomGuidInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            if (RoomRecIds.Count > 0)
            {
              RoomRecIdInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetRecIdInfo(RoomRecIds.ToArray(), FiasTableType.Room);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                RoomRecIdInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            #endregion

            #region �������� �������

            spl.PhaseText = "�������� ������� ��������� (" + PageGuids.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageRoom> dict2 = _Source.GetRoomPages(PageGuids.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageRoom> pair2 in dict2)
            {
              RoomPages[pair2.Value.PageHouseGuid] = pair2.Value;
              if (pair2.Value.PageHouseGuid != Guid.Empty)
                HouseGuids.Add(pair2.Value.PageHouseGuid); // �� ������� �����
              else
                throw new BugException("��� ��������� � GUID=" + pair2.Key.ToString() + " ��������� ������������� ������ GUID.Empty");
            }

            #endregion
          }
        }

        #endregion

        #region House

        if (HouseGuids != null)
        {
          HousePages = new Dictionary<Guid, FiasCachedPageHouse>();
          if (HouseGuids.Count > 0 || HouseRecIds.Count > 0)
          {
            #region ����� GetParentGuids()

            spl.PhaseText = "��������� GUID�� �������� �������� ��� ������ (" + (HouseGuids.Count + HouseRecIds.Count).ToString() + ")";

            SingleScopeList<Guid> PageGuids = new SingleScopeList<Guid>();

            if (HouseGuids.Count > 0)
            {
              HouseGuidInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetGuidInfo(HouseGuids.ToArray(), FiasTableType.House);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                HouseGuidInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            if (HouseRecIds.Count > 0)
            {
              HouseRecIdInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetRecIdInfo(HouseRecIds.ToArray(), FiasTableType.House);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                HouseRecIdInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            #endregion

            #region �������� �������

            spl.PhaseText = "�������� ������� ������ (" + PageGuids.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageHouse> dict2 = _Source.GetHousePages(PageGuids.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageHouse> pair2 in dict2)
            {
              HousePages[pair2.Value.PageAOGuid] = pair2.Value;
              if (pair2.Value.PageAOGuid != Guid.Empty)
                AOGuids.Add(pair2.Value.PageAOGuid); // �� ������� �����
              else
                throw new BugException("��� ������ � GUID=" + pair2.Key.ToString() + " ��������� ������������� ��������� ������� GUID.Empty");
            }

            #endregion
          }
        }

        #endregion

        #region AddrOb

        AOPages = new Dictionary<Guid, Dictionary<FiasLevel, FiasCachedPageAddrOb>>();

        SingleScopeList<Guid> AuxHousePageGuids = null; // �������� �������������� �������������� �������� ��������, ��� ������� ����� ��������� ������ ������

        if (AOGuids.Count > 0 || AORecIds.Count > 0)
        {
          #region ����� GetParentGuids()

          Dictionary<FiasLevel, SingleScopeList<Guid>> PageGuidDict = new Dictionary<FiasLevel, SingleScopeList<Guid>>();
          SingleScopeList<Guid> ResidualAOGuids = new SingleScopeList<Guid>();

          if (AORecIds.Count > 0)
          {
            spl.PhaseText = "��������� GUID�� �������� �������� �� ��������������� ������� (" + AORecIds.Count.ToString() + ")";

            AORecIdInfoDict = new Dictionary<Guid, FiasGuidInfo>();

            IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetRecIdInfo(AORecIds.ToArray(), FiasTableType.AddrOb);
            foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
            {
              AORecIdInfoDict.Add(pair1.Key, pair1.Value);
              if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                continue;
              if (pair1.Value.ParentGuid != Guid.Empty)
                AOGuids.Add(pair1.Value.ParentGuid);

              if (!PageGuidDict.ContainsKey(pair1.Value.Level))
                PageGuidDict.Add(pair1.Value.Level, new SingleScopeList<Guid>());
              PageGuidDict[pair1.Value.Level].Add(pair1.Value.ParentGuid);
            }
          }

          if (AOGuids.Count > 0)
          {
            AOGuidInfoDict = new Dictionary<Guid, FiasGuidInfo>();
            ResidualAOGuids.AddRange(AOGuids);
            bool IsFirstRound = true;

            while (ResidualAOGuids.Count > 0)
            {
              spl.PhaseText = "��������� GUID�� ������������ �������� �������� (" + ResidualAOGuids.Count.ToString() + ")";

              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetGuidInfo(ResidualAOGuids.ToArray(), FiasTableType.AddrOb);

              if (IsFirstRound && _HaveHouses && _Source.DBSettings.UseHouse)
                AuxHousePageGuids = new SingleScopeList<Guid>();

              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                //if (pair1.Key == new Guid("{54049357-326d-4b8f-b224-3c6dc25d6dd3}"))
                //{
                //}
                AOGuidInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                if (!PageGuidDict.ContainsKey(pair1.Value.Level))
                  PageGuidDict.Add(pair1.Value.Level, new SingleScopeList<Guid>());

                PageGuidDict[pair1.Value.Level].Add(pair1.Value.ParentGuid);

                if (IsFirstRound &&
                  AuxHousePageGuids != null &&
                  FiasTools.IsInheritableLevel(pair1.Value.Level, FiasLevel.House, false)) // ����� �� ��� ����� ������ ���� ����?
                {
                  AuxHousePageGuids.Add(pair1.Key);
                }
              }

              // 21.01.2020
              // ��������� ResidualAOGuids � ��������� �����, ����� ����, ��� AOParentDict ��� ��������

              ResidualAOGuids.Clear();
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                if (pair1.Value.ParentGuid != Guid.Empty && (!AOGuidInfoDict.ContainsKey(pair1.Value.ParentGuid)))
                  ResidualAOGuids.Add(pair1.Value.ParentGuid);
              }
              IsFirstRound = false;
            }
          }

          #endregion

          #region �������� �������

          foreach (KeyValuePair<FiasLevel, SingleScopeList<Guid>> pair2 in PageGuidDict)
          {
            spl.PhaseText = "�������� ������� �������� �������� \"" + FiasEnumNames.ToString(pair2.Key, false) + "\" (" + pair2.Value.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageAddrOb> dict3 = _Source.GetAddrObPages(pair2.Key, pair2.Value.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageAddrOb> pair3 in dict3)
            {
              //if (pair3.Key == new Guid("{54049357-326d-4b8f-b224-3c6dc25d6dd3}"))
              //{ 
              //}

              if (!AOPages.ContainsKey(pair3.Value.PageAOGuid))
                AOPages.Add(pair3.Value.PageAOGuid, new Dictionary<FiasLevel, FiasCachedPageAddrOb>());
              AOPages[pair3.Value.PageAOGuid][pair2.Key] = pair3.Value;
            }
          }

          #endregion
        }

        #endregion

        #region Houses - ������ 2

        // ���� �� ����, ��� ����� �������� ���������
        //SingleScopeList<Guid> AuxRoomPageGuids =null;
        //if (_Source.DBSettings.UseRoom && _HaveRooms)
        //{
        //  AuxRoomPageGuids = new SingleScopeList<Guid>(); // �������� �������������� �������������� ������, ��� ������� ����� ��������� ������ ���������
        //  if (RoomGuids.Count == 0)
        //    AuxRoomPageGuids.AddRange(HouseGuids); // ���� ������ GUID� ������ � ������ ��������� �������.
        //}

        if (AuxHousePageGuids != null)
        {
          SingleScopeList<Guid> PageGuids = new SingleScopeList<Guid>();
          foreach (Guid g in AuxHousePageGuids)
          {
            if (!HousePages.ContainsKey(g))
              PageGuids.Add(g);
          }

          if (PageGuids.Count > 0)
          {
            #region �������� �������

            spl.PhaseText = "�������� ������� ������ (" + PageGuids.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageHouse> dict2 = _Source.GetHousePages(PageGuids.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageHouse> pair2 in dict2)
              HousePages[pair2.Value.PageAOGuid] = pair2.Value;

            #endregion
          }
        }

        #endregion

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.PageLoader.Load() finished.");
      }

      #endregion

      #region �������������� ������ ��������� �������

      public FiasCachedPageAddrOb GetAOPage(Guid pageAOGuid, FiasLevel level)
      {
        Dictionary<FiasLevel, FiasCachedPageAddrOb> dict1;
        if (!AOPages.TryGetValue(pageAOGuid, out dict1))
        {
          dict1 = new Dictionary<FiasLevel, FiasCachedPageAddrOb>();
          AOPages.Add(pageAOGuid, dict1);
        }

        FiasCachedPageAddrOb page;
        if (!dict1.TryGetValue(level, out page))
        {
          // ��������� �������� ��������
          page = _Source.GetAddrObPages(level, new Guid[1] { pageAOGuid })[pageAOGuid];
          dict1.Add(level, page);
        }
        return page;
      }

      public FiasCachedPageHouse GetHousePage(Guid pageAOGuid)
      {
        FiasCachedPageHouse page;
        if (!HousePages.TryGetValue(pageAOGuid, out page))
        {
          page = _Source.GetHousePages(new Guid[1] { pageAOGuid })[pageAOGuid];
          HousePages.Add(pageAOGuid, page);
        }
        return page;
      }

      public FiasCachedPageRoom GetRoomPage(Guid pageHouseGuid)
      {
        FiasCachedPageRoom page;
        if (!RoomPages.TryGetValue(pageHouseGuid, out page))
        {
          page = _Source.GetRoomPages(new Guid[1] { pageHouseGuid })[pageHouseGuid];
          RoomPages.Add(pageHouseGuid, page);
        }
        return page;
      }

      #endregion
    }

    /// <summary>
    /// ������� ����������
    /// </summary>
    public FiasCachedAOTypes AOTypes
    {
      get
      {
        if (_AOTypes == null)
          _AOTypes = _Source.GetAOTypes();
        return _AOTypes;
      }
    }
    private FiasCachedAOTypes _AOTypes;

    #endregion

    #region ������ ��� ���������

    /// <summary>
    /// �� ������������ � ���������� ����
    /// </summary>
    /// <param name="level"></param>
    /// <param name="pageAOGuid"></param>
    /// <returns></returns>
    public FiasCachedPageAddrOb GetAddrObPage(FiasLevel level, Guid pageAOGuid)
    {
      return _Source.GetAddrObPages(level, new Guid[1] { pageAOGuid })[pageAOGuid];
    }

    /// <summary>
    /// �� ������������ � ���������� ����
    /// </summary>
    /// <param name="pageAOGuid"></param>
    /// <returns></returns>
    public FiasCachedPageHouse GetHousePage(Guid pageAOGuid)
    {
      return _Source.GetHousePages(new Guid[1] { pageAOGuid })[pageAOGuid];
    }

    /// <summary>
    /// �� ������������ � ���������� ����
    /// </summary>
    /// <param name="pageHouseGuid"></param>
    /// <returns></returns>
    public FiasCachedPageRoom GetRoomPage(Guid pageHouseGuid)
    {
      return _Source.GetRoomPages(new Guid[1] { pageHouseGuid })[pageHouseGuid];
    }

    #endregion

    #region ����� �������

    /// <summary>
    /// ���������� true, ���� �������������� ����� �������
    /// </summary>
    public bool AddressSearchEnabled { get { return _Source.InternalSettings.FTSMode != FiasFTSMode.None; } }

    /// <summary>
    /// ��������� ����� ������� �� �������� ����������
    /// </summary>
    /// <param name="searchParams">��������� ������</param>
    /// <returns>������ ��������� �������</returns>
    public FiasAddress[] FindAddresses(FiasAddressSearchParams searchParams)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss() started.");

      if (!AddressSearchEnabled)
        throw new InvalidOperationException("����� ������� �� ��������������");

      if (searchParams == null)
        throw new ArgumentNullException("searchParams");
      if (String.IsNullOrEmpty(searchParams.Text))
        throw new ArgumentNullException("searchParams", "searchParams.Text==null");
      if (searchParams.Levels != null)
      {
        if (searchParams.Levels.Length == 0)
          throw new ArgumentException("����� ������ ������ ������� searchParams.Levels.Length==0", "searchParams");
      }
      DataTable table = _Source.FindAddresses(searchParams).Tables[0];

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss(). Table.Rows.Count=" + table.Rows.Count.ToString());

      if (table.Rows.Count == 0)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss() finished.");
        return new FiasAddress[0];
      }

      // ������� ������������� ������
      RemoveRepeatedAOGuids(table);

      FiasAddress[] a = new FiasAddress[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
      {
        a[i] = new FiasAddress();
        //a[i].AOGuid = DataTools.GetGuid(table.Rows[i], "AOGUID");
        a[i].AORecId = DataTools.GetGuid(table.Rows[i], "AOID");
      }

      FillAddresses(a);

      //if (_Source.DBSettings.UseHistory)
      //{
      //  for (int i = 0; i < table.Rows.Count; i++)
      //  {
      //    if (!DataTools.GetBool(table.Rows[i], "Actual"))
      //      a[i].Actuality = FiasActuality.Historical;
      //  }
      //}

      // ��������� ���������� �������
      DataTable table2 = new DataTable();
      table2.Columns.Add("Index", typeof(int));
      table2.Columns.Add("AddressText", typeof(string));
      table2.Columns.Add("Actuality", typeof(int)); // ���������� ����� �������������
      if (_Source.DBSettings.UseDates)
      {
        table2.Columns.Add("STARTDATE", typeof(DateTime));
        table2.Columns.Add("ENDDATE", typeof(DateTime));
      }
      for (int i = 0; i < a.Length; i++)
      {
        // �������� ������ ��������
        string PostalCode = a[i].PostalCode;
        a[i].PostalCode = String.Empty;
        DataRow row2 = table2.Rows.Add(i, a[i].ToString(), (int)(a[i].Actuality));
        a[i].PostalCode = PostalCode;

#if XXX // ���� �������� ����?
        if (_Source.DBSettings.UseDates)
        {
          if (_Source.InternalSettings.UseOADates)
          {
            row2["STARTDATE"] = DateTime.FromOADate(DataTools.GetInt(table.Rows[i], "dStartDate"));
            row2["ENDDATE"] = DateTime.FromOADate(DataTools.GetInt(table.Rows[i], "dEndDate"));
          }
          else
          {
            row2["STARTDATE"] = table.Rows[i]["STARTDATE"];
            row2["ENDDATE"] = table.Rows[i]["ENDDATE"];
          }
        }
#endif
      }

      table2.DefaultView.Sort = "AddressText,Actuality";
      FiasAddress[] a2 = new FiasAddress[a.Length];
      for (int i = 0; i < table2.DefaultView.Count; i++)
      {
        int index = DataTools.GetInt(table2.DefaultView[i].Row, "Index");
        a2[i] = a[index];
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss() finished.");

      return a2;
    }

    private void RemoveRepeatedAOGuids(DataTable table)
    {
      string s = "AOGUID";
      if (_Source.DBSettings.UseHistory)
        s += ",Actual DESC";
      if (_Source.DBSettings.UseDates)
      {
        if (_Source.InternalSettings.UseOADates)
          s += ",dEndDate DESC,dStartDate DESC";
        else
          s += ",ENDDATE DESC,STARTDATE DESC";
      }
      table.DefaultView.Sort = s;
      Guid prevG = Guid.Empty;
      List<DataRow> delRows = null;
      for (int i = 0; i < table.DefaultView.Count; i++)
      {
        Guid g = DataTools.GetGuid(table.DefaultView[i].Row, "AOGUID");
        if (g == prevG)
        {
          if (delRows == null)
            delRows = new List<DataRow>();
          delRows.Add(table.DefaultView[i].Row);
        }
        else
          prevG = g;
      }
      if (delRows != null)
      {
        for (int i = 0; i < delRows.Count; i++)
          delRows[i].Delete();
        table.AcceptChanges();

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.RemoveRepeatedAOGuids() deleted " + delRows.Count.ToString() + " row(s).");
      }
    }

    #endregion

    #region ��������� ������������� ������

    /// <summary>
    /// �������� ��������� ������������� ������, ������� �������� ������
    /// </summary>
    /// <param name="address">����������� �����</param>
    /// <returns>��������� �������������</returns>
    public string GetText(FiasAddress address)
    {
      StringBuilder sb = new StringBuilder();
      GetText(sb, address);
      return sb.ToString();
    }

    /// <summary>
    /// �������� ��������� ������������� ������, ������� �������� ������
    /// </summary>
    /// <param name="sb">����� ��� ������ ������</param>
    /// <param name="address">����������� �����</param>
    public void GetText(StringBuilder sb, FiasAddress address)
    {
#if DEBUG
      if (sb == null)
        throw new ArgumentNullException("sb");
      if (address == null)
        throw new ArgumentNullException("address");
#endif
      if (!String.IsNullOrEmpty(address.PostalCode))
      {
        sb.Append(address.PostalCode);
        if (address.NameBottomLevel != FiasLevel.Unknown)
          sb.Append(", ");
      }
      GetTextWithoutPostalCode(sb, address);
    }


    /// <summary>
    /// �������� ��������� ������������� ������ ��� ��������� �������
    /// </summary>
    /// <param name="address">����������� �����</param>
    /// <returns>��������� �������������</returns>
    public string GetTextWithoutPostalCode(FiasAddress address)
    {
      StringBuilder sb = new StringBuilder();
      GetTextWithoutPostalCode(sb, address);
      return sb.ToString();
    }

    /// <summary>
    /// �������� ��������� ������������� ������ ��� ��������� �������
    /// </summary>
    /// <param name="sb">����� ��� ������ ������</param>
    /// <param name="address">����������� �����</param>
    public void GetTextWithoutPostalCode(StringBuilder sb, FiasAddress address)
    {
#if DEBUG
      if (sb == null)
        throw new ArgumentNullException("sb");
      if (address == null)
        throw new ArgumentNullException("address");
#endif

      // ������ ����� ������������ sb, �.�. �� ����� ���� ��������.
      // ��� ������ ����������� ����������� �������
      if (_InternalSB == null)
        _InternalSB = new StringBuilder();
      _InternalSB.Length = 0;
      GetComponentTextAt(address, FiasLevel.Region);
      sb.Append(_InternalSB);
    }

    #endregion

    #region Format()

    private Dictionary<string, FiasParsedFormatString> _FormatDict;
    private StringBuilder _InternalSB;

    /// <summary>
    /// ��������� ���������� ������������� ������ ��� ��� ����������� � �������������� ��������������.
    /// ��������� ������� ������ �������������� <paramref name="format"/> ������� FiasFormatStringParser.Parse().
    /// � ������ ������ � ������ ������������� ����������. ����������� ����������� ����������� ��������.
    /// </summary>
    /// <param name="address">����������� ������ ������. �� ����� ���� null</param>
    /// <param name="format">������ ��������������</param>
    /// <returns>��������� ������������� � ������������ �� ������� ��������������</returns>
    public string Format(FiasAddress address, string format)
    {
      StringBuilder sb = new StringBuilder();
      Format(sb, address, format);
      return sb.ToString();
    }

    /// <summary>
    /// ��������� ���������� ������������� ������ ��� ��� ����������� � �������������� ��������������.
    /// ��������� ������� ������ �������������� <paramref name="format"/> ������� FiasFormatStringParser.Parse().
    /// � ������ ������ � ������ ������������� ����������. ����������� ����������� ����������� ��������.
    /// </summary>
    /// <param name="sb">�����, ���� ����������� ��������� ������������� � ������������ �� ������� ��������������</param>
    /// <param name="address">����������� ������ ������. �� ����� ���� null</param>
    /// <param name="format">������ ��������������</param>
    public void Format(StringBuilder sb, FiasAddress address, string format)
    {
      if (_FormatDict == null)
        _FormatDict = new Dictionary<string, FiasParsedFormatString>();

      FiasParsedFormatString parsedString;
      if (!_FormatDict.TryGetValue(format, out parsedString))
      {
        parsedString = FiasFormatStringParser.Parse(format);
        _FormatDict.Add(format, parsedString);
      }

      Format(sb, address, parsedString);
    }

    /// <summary>
    /// ��������� ���������� ������������� ������ ��� ��� ����������� � �������������� ��������������.
    /// </summary>
    /// <param name="address">����������� ������ ������. �� ����� ���� null.</param>
    /// <param name="format">��������� �������� ������ ��������������, ����������� ������� FiasFormatStringParser.Parse(). �� ����� ���� null.</param>
    /// <returns>��������� ������������� � ������������ �� ������� ��������������</returns>
    public string Format(FiasAddress address, FiasParsedFormatString format)
    {
      StringBuilder sb = new StringBuilder();
      Format(sb, address, format);
      return sb.ToString();
    }

    /// <summary>
    /// ��������� ���������� ������������� ������ ��� ��� ����������� � �������������� ��������������.
    /// </summary>
    /// <param name="sb">�����, ���� ����������� ��������� ������������� � ������������ �� ������� ��������������</param>
    /// <param name="address">����������� ������ ������. �� ����� ���� null.</param>
    /// <param name="format">��������� �������� ������ ��������������, ����������� ������� FiasFormatStringParser.Parse(). �� ����� ���� null.</param>
    public void Format(StringBuilder sb, FiasAddress address, FiasParsedFormatString format)
    {
      if (sb == null)
        throw new ArgumentNullException("sb");
      if (address == null)
        throw new ArgumentNullException("address");
      if (format == null)
        throw new ArgumentNullException("format");

      if (_InternalSB == null)
        _InternalSB = new StringBuilder();


      int count = 0;
      for (int i = 0; i < format.Items.Count; i++)
      {
        FiasParsedFormatString.FormatItem item = format.Items[i];
        if (item.ItemType != FiasFormatStringParser.TypeFormConst)
        {
          _InternalSB.Length = 0;
          GetComponentText(address, item.ItemType);
          if (_InternalSB.Length == 0)
            continue;
        }

        if (count > 0)
          sb.Append(item.Separator);
        sb.Append(item.Prefix);
        sb.Append(_InternalSB.ToString());
        sb.Append(item.Suffix);
        count++;
      }
    }

    private void GetComponentText(FiasAddress address, int itemType)
    {
      FiasLevel level = (FiasLevel)(itemType & FiasFormatStringParser.TypeLevelMask);
      int form = itemType & FiasFormatStringParser.TypeFormMask;
      switch (form)
      {
        case FiasFormatStringParser.TypeFormText:
          if (!String.IsNullOrEmpty(address.PostalCode))
          {
            _InternalSB.Append(address.PostalCode);
            //_InternalSB.Append(0, ", ");
          }
          GetComponentTextAt(address, FiasLevel.Region);
          break;
        case FiasFormatStringParser.TypePostalCode:
          _InternalSB.Append(address.PostalCode);
          break;

        case FiasFormatStringParser.TypeFormNameAndAbbr:
          switch (level)
          {
            case FiasLevel.House:
              GetComponentNameAndAbbreviation(address, FiasLevel.House);
              GetComponentNameAndAbbreviation(address, FiasLevel.Building);
              GetComponentNameAndAbbreviation(address, FiasLevel.Structure);
              break;
            case FiasLevel.Room:
              GetComponentNameAndAbbreviation(address, FiasLevel.Flat);
              GetComponentNameAndAbbreviation(address, FiasLevel.Room);
              break;
            default:
              GetComponentNameAndAbbreviation(address, level);
              break;
          }
          break;

        case FiasFormatStringParser.TypeFormName:
          switch (level)
          {
            case FiasLevel.House:
              GetComponentNameAndAbbreviation(address.GetName(FiasLevel.House), String.Empty, FiasLevel.House);
              GetComponentNameAndAbbreviation(address, FiasLevel.Building);
              GetComponentNameAndAbbreviation(address, FiasLevel.Structure);
              break;
            case FiasLevel.Room:
              GetComponentNameAndAbbreviation(address.GetName(FiasLevel.Flat), String.Empty, FiasLevel.Flat);
              GetComponentNameAndAbbreviation(address, FiasLevel.Room);
              break;
            default:
              _InternalSB.Append(address.GetName(level));
              break;
          }
          break;

        case FiasFormatStringParser.TypeFormType:
          _InternalSB.Append(address.GetAOType(level));
          break;

        case FiasFormatStringParser.TypeFormAbbr:
          _InternalSB.Append(AOTypes.GetAbbreviation(address.GetAOType(level), level));
          break;

        case FiasFormatStringParser.TypeFormNum:
          switch (level)
          {
            case FiasLevel.House:
            case FiasLevel.Building:
            case FiasLevel.Structure:
            case FiasLevel.Flat:
            case FiasLevel.Room:
              _InternalSB.Append(address.GetName(level));
              break;
            default:
              throw new BugException("������������ Level=" + level.ToString() + " ��� TypeFormNum");
          }
          break;

        case FiasFormatStringParser.TypeFormAt:
          GetComponentTextAt(address, level);
          break;

        case FiasFormatStringParser.TypeRegionCode:
          _InternalSB.Append(address.RegionCode);
          break;

        case FiasFormatStringParser.TypeFormGuid:
          GetGuid(address.GetGuid(level));
          break;
        case FiasFormatStringParser.TypeFormRecId:
          GetGuid(address.GetRecId(level));
          break;
        case FiasFormatStringParser.TypeAOGuid:
          GetGuid(address.AOGuid);
          break;
        case FiasFormatStringParser.TypeAORecId:
          GetGuid(address.AORecId);
          break;
        case FiasFormatStringParser.TypeAnyGuid:
          GetGuid(address.AnyGuid);
          break;
        //case FiasFormatStringParser.TypeAOGuid:
        //  GetGuid(address.AnyRecId);
        //  break;

        case FiasFormatStringParser.TypeOKATO:
          _InternalSB.Append(address.OKATO);
          break;
        case FiasFormatStringParser.TypeOKTMO:
          _InternalSB.Append(address.OKTMO);
          break;

        case FiasFormatStringParser.TypeIFNSFL:
          _InternalSB.Append(address.IFNSFL);
          break;
        //case FiasFormatStringParser.TypeTerrIFNSFL:
        //  _InternalSB.Append(address.TerrIFNSFL);
        //  break;
        case FiasFormatStringParser.TypeIFNSUL:
          _InternalSB.Append(address.IFNSUL);
          break;
        //case FiasFormatStringParser.TypeTerrIFNSUL:
        //  _InternalSB.Append(address.TerrIFNSUL);
        //  break;

        default:
          throw new BugException("����������� ����� " + form.ToString());
      }
    }

    private void GetComponentTextAt(FiasAddress address, FiasLevel topLevel)
    {
      for (int i = FiasTools.AllLevelIndexer.IndexOf(topLevel); i < FiasTools.AllLevels.Length; i++)
        GetComponentNameAndAbbreviation(address, FiasTools.AllLevels[i]);
    }

    private void GetComponentNameAndAbbreviation(FiasAddress address, FiasLevel level)
    {
      string nm = address.GetName(level);
      string aoType = address.GetAOType(level);
      string abbr = AOTypes.GetAbbreviation(aoType, level);

      if (level == FiasLevel.Region)
      {
        if (nm.IndexOf(aoType, StringComparison.OrdinalIgnoreCase) >= 0)
          abbr = String.Empty; // 05.10.2020
      }

      GetComponentNameAndAbbreviation(nm, abbr, level);
    }
    private void GetComponentNameAndAbbreviation(string name, string abbr, FiasLevel level)
    {
      if (String.IsNullOrEmpty(name))
        return;
      if (_InternalSB.Length > 0)
        _InternalSB.Append(", ");

      if (GetComponentNameAndAbbreviationSpecialCases(name, abbr, level))
        return;

      FiasAOTypePlace place = GetAOTypePlace(level, name, abbr, FiasAOTypeMode.Abbreviation);

      if (String.IsNullOrEmpty(abbr))
        place = FiasAOTypePlace.None;

      switch (place)
      {
        case FiasAOTypePlace.AfterName:
          _InternalSB.Append(name);
          _InternalSB.Append(" ");
          _InternalSB.Append(abbr);
          break;
        case FiasAOTypePlace.BeforeName:
          _InternalSB.Append(abbr);
          if (abbr[abbr.Length - 1] != '.' && AOTypes.IsAbbreviationDotRequired(abbr, level))
            _InternalSB.Append('.'); // 02.09.2021
          _InternalSB.Append(" ");
          _InternalSB.Append(name);
          break;
        case FiasAOTypePlace.None:
          _InternalSB.Append(name);
          break;
        default:
          throw new BugException("place=" + place.ToString());
      }
    }

    private bool GetComponentNameAndAbbreviationSpecialCases(string name, string abbr, FiasLevel level)
    {
      int p;

      switch (abbr)
      { 
        case "�����":
          p = name.IndexOf("-� ");
          if (p >= 0)
          {
            // ����� �����-���������, ����� "14-� �.�." -> 14-� ����� �.�.

            _InternalSB.Append(name.Substring(0, p + 2)); // ������� "-�", �� ��� �������
            _InternalSB.Append(" �����");
            _InternalSB.Append(name.Substring(p + 2)); // ������� � �������
            return true;
          }
          if (name.EndsWith("-�"))
          {
            // ����� �����-���������, ����� "��� ����������� 1-�" -> ��� ����������� 1-� �����
            _InternalSB.Append(name);
            _InternalSB.Append(" �����");
            return true;
          }
          // ����� �����-���������, ����� "��������� (�������� ����)" -> ��������� ����� (�������� ����)
          p = name.IndexOf("� (");
          if (p >= 0)
          {
            _InternalSB.Append(name.Substring(0, p + 1)); // ��� �������
            _InternalSB.Append(" �����");
            _InternalSB.Append(name.Substring(p + 1));
            return true;
          }
          break;
      }
      return false;
    }

    /// <summary>
    /// ���������� �����, � ������� ������ ������������� ���������� ��� ������.
    /// ��������, ���������� "�-�" ������ ���� ����� ������������, � "�."- �� ������������
    /// </summary>
    /// <param name="level">������� ����������������� ��������</param>
    /// <param name="name">������� ����� ��������� �������</param>
    /// <param name="aoType">���������� ��� ���� ��������� �������</param>
    /// <param name="aoTypeMode">��� ��� ���������� � <paramref name="aoType"/>?</param>
    /// <returns>��������� ���������� ������������ ������������</returns>
    public FiasAOTypePlace GetAOTypePlace(FiasLevel level, string name, string aoType, FiasAOTypeMode aoTypeMode)
    {
      if (String.IsNullOrEmpty(aoType))
        return FiasAOTypePlace.None;
      switch (level)
      {
        case FiasLevel.Region:
        case FiasLevel.District:
          return FiasAOTypePlace.AfterName;
        default:
          switch (aoType.ToUpperInvariant())
          { 
            case "��":
              return FiasAOTypePlace.AfterName;
          }
          return FiasAOTypePlace.BeforeName;
      }
    }

    private void GetGuid(Guid guid)
    {
      if (guid == Guid.Empty)
        return;
      _InternalSB.Append(guid.ToString("D")); // ��� ������
    }

    #endregion

    #region ��������� ������� �� ������ ��� �������

    /// <summary>
    /// ������� ���������� ����� � ������.
    /// ��������������, ��� ���������� ������ ��������� ��������.
    /// ��� ������ ����� ������������ ������ ������ (FiasAddress.IsEmpty=true).
    /// ������ �������� ������������ � ������ FiasAddress.Messages. 
    /// </summary>
    /// <param name="lines">������. ������� �������� ������� (������) ������������� ���� �����</param>
    /// <param name="parseSettings">��������� ��������</param>
    /// <returns>������ �������. ����� ������� ������������� <paramref name="lines"/>.</returns>
    public FiasAddress[] ParseAddresses(string[] lines, OldFiasParseSettings parseSettings)
    {
      // TODO: ���� ��������� ���������� ��������� � �������, ��������� ��, � �� ������ ����� ��������

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() started. lines.Length=" + lines.Length.ToString());

      FiasAddress[] a = new FiasAddress[lines.Length];

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "���������� ������ GUID��",
       "�������� �������",
       lines.Length==1 ? "����������� ������" : "����������� ������� ("+lines.Length.ToString()+")",
       "��������"
      });
      try
      {
        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        ErrorMessageList[] aParseErrors = new ErrorMessageList[lines.Length]; // �������������� ������ ��������
        spl.PercentMax = lines.Length;
        spl.AllowCancel = true;

        for (int i = 0; i < lines.Length; i++)
        {
          aParseErrors[i] = new ErrorMessageList();
          a[i] = DoParseAddress(lines[i], parseSettings, loader, aParseErrors[i]);
          spl.IncPercent();
        }
        spl.Complete();

        FillAddresses(a);
        for (int i = 0; i < lines.Length; i++)
          a[i].AddMessages(aParseErrors[i]);
        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() finished.");

      return a;
    }

    /// <summary>
    /// ������� ������ � �����.
    /// ��������������, ��� ���������� ������ ��������� ��������.
    /// ��� ������ ������ ������������ ������ ����� (FiasAddress.IsEmpty=true).
    /// ������ �������� ������������ � ������ FiasAddress.Messages. 
    /// </summary>
    /// <param name="s">������ ��� ��������</param>
    /// <param name="parseSettings">��������� ��������</param>
    /// <returns>�����</returns>
    public FiasAddress ParseAddress(string s, OldFiasParseSettings parseSettings)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() started.");

      FiasAddress addr;
      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "���������� ������ GUID��",
       "�������� �������",
       "����������� ������",
       "��������",
      });
      try
      {
        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        ErrorMessageList parseErrors = new ErrorMessageList();
        addr = DoParseAddress(s, parseSettings, loader, parseErrors);
        spl.Complete();

        FillAddress(addr);
        addr.AddMessages(parseErrors);
        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() finished.");

      return addr;
    }

    private FiasAddress DoParseAddress(string s, OldFiasParseSettings parseSettings, PageLoader loader, ErrorMessageList parseErrors)
    {
      // 01.10.2020
      s = s.Replace("--", "-");
      s = s.Replace("- ", "-");
      s = s.Replace(" -", "-");
      s = s.Replace("  ", " ");

      s = s.Trim();
      if (s.Length == 0)
      {
        parseErrors.AddInfo("������ ������");
        return new FiasAddress();
      }
      FiasAddress address = parseSettings.BaseAddress.Clone();

      bool useRB = (address.NameBottomLevel == address.GuidBottomLevel);

      string[] a = s.Split(',');
      for (int i = 0; i < a.Length; i++)
        DoParseAddressPart(a[i].Trim(), address, parseSettings, loader, parseErrors, ref useRB, i == (a.Length - 1));

      return address;
    }

    private static readonly char[] AuxSepChars = new char[] { ' ', '-' };

    private void DoParseAddressPart(string part, FiasAddress address, OldFiasParseSettings parseSettings, PageLoader loader, ErrorMessageList parseErrors, ref bool useRB, bool isLastPart)
    {
      if (part.Length == 0)
        return;

      if (!useRB) // �� ����� ������������ ����������
      {
        AddPartToAddress(parseSettings, address, part, null, parseErrors);
        return;
      }


      while (part.Length > 0)
      {
        if (address.GuidBottomLevel == FiasLevel.Street &&
          parseSettings.EditorLevel == FiasEditorLevel.Room &&
          isLastPart &&
          part[0] >= '0' && part[0] <= '9' &&
          part[part.Length - 1] >= '0' && part[part.Length - 1] <= '9' &&
          part.IndexOf('-') > 0 && part.LastIndexOf('-') == part.IndexOf('-'))
        {
          // 02.10.2020
          // ����� ����� ���� � �������� � ������� "�-�"
          int p = part.IndexOf('-');

          // �������� �������� ���
          if (AddPartToAddress(parseSettings, address, part.Substring(0, p), loader, parseErrors))
          {
            // �������. ������ ��������� ��������. 
            if (!AddPartToAddress(parseSettings, address, part.Substring(p + 1), loader, parseErrors))
            {
              useRB = false;
              AddPartToAddress(parseSettings, address, part.Substring(p + 1), null, parseErrors);
            }
            return;
          }
        }

        // �������� ����� � �����������
        if (AddPartToAddress(parseSettings, address, part, loader, parseErrors))
          return;

        // 01.10.2020
        // ���� � ������ ���� �������, �� ����� ����������� ��������� ������ �� ����� � ��������� ����� ������� �����
#if XXX
        string[] a2 = part.Split(' ');
        bool PartFound = false;
        for (int j = a2.Length - 2; j >= 0; j--)
        {
          string s2 = String.Join(" ", a2, 0, j + 1).Trim();
          if (AddPartToAddress(parseSettings, address, s2, loader, parseErrors))
          {
            PartFound = true;
            part = String.Join(" ", a2, j + 1, a2.Length - j - 1).Trim();
          }
        }
        if (!PartFound)
          break;
#endif

        int cnt = part.Length;
        bool PartFound = false;
        while ((cnt = part.LastIndexOfAny(AuxSepChars, cnt - 1, cnt)) > 0)
        {
          string s2 = part.Substring(0, cnt);
          if (AddPartToAddress(parseSettings, address, s2, loader, parseErrors))
          {
            part = part.Substring(cnt + 1);
            PartFound = true;
            break;
          }
        }
        if (!PartFound)
          break;
      }

      // ������ �� ����� �� ������������
      useRB = false;
      AddPartToAddress(parseSettings, address, part, null, parseErrors);
    }

    /// <summary>
    /// �������� � ������ �������� ������
    /// </summary>
    private bool AddPartToAddress(OldFiasParseSettings parseSettings, FiasAddress address, string part, PageLoader loader, ErrorMessageList parseErrors)
    {
      part = part.Trim();
      part = part.Replace("  ", " ");
      part = part.Replace(" .", ".");

      if (part.Length == 0)
        return true; // ������ ���������

      string nm, aoType;

      #region "�.������"

      int p = part.IndexOf('.');
      if (p > 0 && p < (part.Length - 1) && part.IndexOf(' ', 0, p) < 0)
      {
        // ������� �����, ����� ������� ��� ��������.
        // ��������, "�. ������", "�.22"
        aoType = part.Substring(0, p + 1); // ������� �����
        nm = part.Substring(p + 1);

        if (TryAddPartToAddress(parseSettings, address, nm, aoType, loader))
          return true;
      }

      #endregion

      p = part.IndexOf(' ');
      if (p >= 0)
      {
        #region "����� ������"

        aoType = part.Substring(0, p);
        nm = part.Substring(p + 1);

        if (TryAddPartToAddress(parseSettings, address, nm, aoType, loader))
          return true;

        #endregion

        #region "��������� �����"

        p = part.LastIndexOf(' '); // ������ ������ 0
        nm = part.Substring(0, p);
        aoType = part.Substring(p + 1);


        if (TryAddPartToAddress(parseSettings, address, nm, aoType, loader))
          return true;

        #endregion
      }

      #region ��� ���������� "����� ������" ��� "������"

      if (TryAddPartToAddress(parseSettings, address, part, String.Empty, loader))
        return true;

      if (loader == null)
      {
        int pLevel = FiasTools.ParseLevelIndexer.IndexOf(address.NameBottomLevel) + 1;
        int pBottomLevel = FiasTools.ParseLevelIndexer.IndexOf(parseSettings.InternalBottomLevel);
        if (pLevel <= pBottomLevel)
        {
          FiasLevel level = FiasTools.ParseLevels[pLevel];
          address.SetName(level, part);
        }
        else
          parseErrors.AddError("������ �������� �������� ������ \"" + part + "\", ��� ��� ��� ������ ������ ���������");
        return true;
      }

      #endregion

      return false;
    }

    private bool TryAddPartToAddress(OldFiasParseSettings parseSettings, FiasAddress address, string name, string aoType, PageLoader loader)
    {
      name = name.Trim();
      aoType = aoType.Trim();

      if (name.Length == 0)
        return false;

      FiasLevel PrevLevel = address.NameBottomLevel;
      int pBottomLevel = FiasTools.ParseLevelIndexer.IndexOf(parseSettings.InternalBottomLevel);
      for (int pLevel = FiasTools.ParseLevelIndexer.IndexOf(PrevLevel) + 1; pLevel <= pBottomLevel; pLevel++)
      {
        FiasLevel level = FiasTools.ParseLevels[pLevel];
        if (!FiasTools.IsInheritableLevel(PrevLevel, level, true /* 01.03.2021 */))
          continue;

        switch (FiasTools.GetTableType(level))
        {
          case FiasTableType.AddrOb:
            Int32 aoTypeId;
            if (aoType.Length > 0)
            {
              string fullAOType;
              if (!AOTypes.IsValidAOType(level, aoType, out fullAOType, out aoTypeId))
                continue; // ���� ���������� ������, �� ��� ������������ - �� ��������
            }
            else
              aoTypeId = 0;

            // ��������� �����
            if (loader != null)
            {
              FiasCachedPageAddrOb page1 = loader.GetAOPage(address.AOGuid, level);
              FiasSearchRowResult searchRes = page1.FindRow(name, aoTypeId, true);
              if (searchRes.Count == FiasSearchRowCount.NotFound)
              {
                // ���� ��� ����� �� ������ ����������, �� �������� ������������ "�����"
                // ��� ���������, ���� ���� � ����� � ��-����� � ����� ���������
                // ��������, � �������� �����, �. �������� (AOGUID=2e830a89-4f75-4af1-b6bc-639abf8ed050)
                // ���� ��.������� (AOGUID=eff533bc-06cd-4809-84ca-d687e86b61f3) � ���.������� (AOGUID=fd1f752a-6716-4884-b164-65b511d6277c).
                if (level == FiasLevel.Street && aoType.Length == 0)
                {
                  aoTypeId = AOTypes.FindAOTypeId(FiasLevel.Street, "�����");
                  searchRes = page1.FindRow(name, aoTypeId, true);
                }

                if (searchRes.Count != FiasSearchRowCount.Ok)
                  continue;
              }

              _AddrObExtractor.Row = searchRes.Row;
              address.AOGuid = Guid.Empty; // ������� ������������ �������
              address.SetGuid(level, _AddrObExtractor.AOGUID);
              address.SetName(level, _AddrObExtractor.Name);
              address.SetAOType(level, AOTypes.GetAOType(_AddrObExtractor.AOTypeId, FiasAOTypeMode.Full));
              return true;
            }
            break;

          case FiasTableType.House:
            if (loader != null && Source.DBSettings.UseHouse)
            {
              FiasCachedPageHouse page2 = loader.GetHousePage(address.AOGuid);
              DataRow row = page2.FindRow(name, FiasEstateStatus.Unknown, String.Empty, String.Empty, FiasStructureStatus.Unknown);
              if (row == null)
                return false;

              _HouseExtractor.Row = row;
              address.AOGuid = Guid.Empty; // ������� ������������ �������
              address.SetGuid(FiasLevel.House, _HouseExtractor.HOUSEGUID);
              address.SetName(FiasLevel.House, _HouseExtractor.HouseNum);
              address.SetAOType(FiasLevel.House, FiasEnumNames.ToString(_HouseExtractor.EstStatus));
              return true;
            }
            break;

        } // switch(GetTableType)

        if (aoType.Length > 0)
        {
          if (!AOTypes.IsValidAOType(level, aoType))
            continue;
        }

        address.SetName(level, name);
        if (aoType.Length > 0)
          address.SetAOType(level, aoType);
        else
          address.SetAOType(level, FiasTools.GetDefaultAOType(level));
        return true;
      }
      return false;
    }

    #endregion

    #region ��������� ������� �� ������ � ���������� �� �������

    /// <summary>
    /// ������� ���������� ����� � ������.
    /// ��������������, ��� ���������� ������ ��������� ��������.
    /// ��� ������ ����� ������������ ������ ������ (FiasAddress.IsEmpty=true).
    /// ������ �������� ������������ � ������ FiasAddress.Messages. 
    /// </summary>
    /// <param name="textMatrix">������ ������. ������ ������ ������������� ���� �����, � ����� �������� ������ ���� ����� <paramref name="parseSettings"/>.CellLevels.Length.</param>
    /// <param name="parseSettings">��������� ��������</param>
    /// <returns>������ �������. ����� ������� ������������� ���������� ����� � <paramref name="textMatrix"/>.</returns>
    public FiasAddress[] ParseAddresses(string[,] textMatrix, FiasParseSettings parseSettings)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() started. Cells.Length=" + textMatrix.GetLength(0).ToString());

      FiasAddress[] a = new FiasAddress[textMatrix.GetLength(0)];

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "���������� ������ GUID��",
       "�������� �������",
       a.Length==1 ? "����������� ������" : "����������� ������� ("+a.Length.ToString()+")",
       "��������"
      });
      try
      {
        FillAddress(parseSettings.BaseAddress);


        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        spl.PercentMax = a.Length;
        spl.AllowCancel = true;

        for (int i = 0; i < a.Length; i++)
        {
          a[i] = DoParseAddress(DataTools.GetArray2Row<string>(textMatrix, i), parseSettings, loader);
          spl.IncPercent();
        }
        spl.Complete();

        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() finished.");

      return a;
    }

    /// <summary>
    /// ������� ������ � �����.
    /// ��������������, ��� ���������� ������ ��������� ��������.
    /// ��� ������ ������ ������������ ������ ����� (FiasAddress.IsEmpty=true).
    /// ������ �������� ������������ � ������ FiasAddress.Messages. 
    /// </summary>
    /// <param name="cells">������ ��� ��������</param>
    /// <param name="parseSettings">��������� ��������</param>
    /// <returns>�����</returns>
    public FiasAddress ParseAddress(string[] cells, FiasParseSettings parseSettings)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() started.");

      FiasAddress addr;
      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "���������� ������ GUID��",
       "�������� �������",
       "����������� ������",
       "��������",
      });
      try
      {
        FillAddress(parseSettings.BaseAddress);


        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        addr = DoParseAddress(cells, parseSettings, loader);
        spl.Complete();

        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() finished.");

      return addr;
    }

    private FiasAddress DoParseAddress(string[] cellStrings, FiasParseSettings parseSettings, PageLoader loader)
    {
      ParseHelper2 helper = new ParseHelper2();
      helper.Handler = this;
      helper.ParseSettings = parseSettings;
      helper.CellStrings = (string[])(cellStrings.Clone());

      return helper.Parse();
    }

    private class ParseHelper2
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
        if (_BestAddress == null)
          return true;

        // ��������� ������� ������
        if (s.Length > 0 != _BestTail.Length > 0)
          return s.Length == 0;

        FiasLevel testLevel = address.NameBottomLevel;

        int ex1 = GetRBExistance(address, testLevel);
        int ex2 = GetRBExistance(_BestAddress, testLevel);
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
        bool AOTypeFound = false;

        // ����� �� ��� ��������� ������� ���� �� ��� ����� ������� �����
        bool AOTypeBeforeName, AOTypeAfterName;
        FiasTools.GetAOTypePlace(level, out AOTypeBeforeName, out AOTypeAfterName);

        int[] pSpaces = FindSpacePositions(s2);
        // ������� ������, ����������� ���������, ����� ���� � ���� ��������� �������?
        int MaxAOTypeParts = Math.Min(pSpaces.Length, Handler.AOTypes.GetMaxSpaceCount(level) + 1);

        if (AOTypeBeforeName)
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
              AOTypeFound = true;
              RemoveNumChar(ref nm, level);
              if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
                res = true;
              if (AddHouseNumWithSpace(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
                res = true;
            }
          }
        }

        // ���������� ��������, ����� ���� ��� ��������� �������, � ����� - ������������ ("����� ������")
        if (AOTypeBeforeName)
        {
          for (int i = 0; i < MaxAOTypeParts; i++)
          {
            // ������������, ��� ������������ ���������� � ��������, ��������, "��� 1"
            string aoType = s2.Substring(0, pSpaces[i]);
            string nm = s2.Substring(pSpaces[i] + 1); // ��� ����� ���� ��������� �������
            string fullAOType;
            if (IsValidAOType(level, aoType, out fullAOType))
            {
              // ���������� �������� ��� ������
              AOTypeFound = true;
              RemoveNumChar(ref nm, level);
              if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
                res = true;
              if (AddHouseNumWithSpace(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
                res = true;
            }
          }
        }

        // ���������� ��������, ����� ���� ������������, � ����� - ��� ��������� ������� ("��������� �����")
        if (AOTypeAfterName)
        {
          for (int i = 0; i < MaxAOTypeParts; i++)
          {
            // ������������, ��� ������������ ���������� � ��������, ��������, "��� 1"
            string nm = s2.Substring(0, pSpaces[pSpaces.Length - i - 1]);
            string aoType = s2.Substring(pSpaces[pSpaces.Length - i - 1] + 1); // ��� ����� ���� ��������� �������
            string fullAOType;
            if (IsValidAOType(level, aoType, out fullAOType))
            {
              // ���������� �������� ��� ������
              AOTypeFound = true;
              RemoveNumChar(ref nm, level);
              if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
                res = true;
            }
          }
        }

        if ((!AOTypeFound) ||
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
          if (Handler.AOTypes.IsValidAOType(level, aoType.Substring(0, aoType.Length-1), out fullAOType, out id))
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

        // ���� ������������� �����������
        // ����� � �������� �������
        bool CharSepFound = false;
        for (int p = s.Length - 2; p >= 1; p--)
        {
          if (s[p] == '-' || s[p] == '/' || s[p] == '\\')
          {
            CharSepFound = true;
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
        if (!CharSepFound)
        {
          switch (level)
          {
            case FiasLevel.House:
            case FiasLevel.Building:
            case FiasLevel.Flat:
              // ������ ��� �������, ����� ������� ����� ���� �����������.
              // ��������, ��� "1�" - ��� 1, ����� �
              // �� ������, ���� ���� ������������ ����� �������
              int TransPos = -1;
              int TransCount = 0;
              for (int i = 1; i < s.Length; i++)
              {
                if ((Char.IsDigit(s[i - 1]) && Char.IsLetter(s[i])) ||
                  (Char.IsLetter(s[i - 1]) && Char.IsDigit(s[i])))
                {
                  TransCount++;
                  TransPos = i;
                }
              }

              if (TransCount == 1)
              {
                string leftPart = s.Substring(0, TransPos);
                string rightPart = s.Substring(TransPos);
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

        int DigitStart = -1;
        bool MoreDigitGroups = false;
        int NextLetterStart = -1;
        for (int i = 1; i < s.Length; i++)
        {
          if (Char.IsDigit(s[i]))
          {
            if (DigitStart < 0)
              DigitStart = i;
            else if (!Char.IsDigit(s[i - 1]))
              MoreDigitGroups = true; // ���� ������ �������� ������
          }
          else if (Char.IsLetter(s[i]))
          {
            if (NextLetterStart < 0 && Char.IsDigit(s[i - 1]))
              NextLetterStart = i;
          }
          else
          {
            // �� ����� � �� �����
            return false;
          }
        }

        if (DigitStart < 0)
          return false; // ����� ������� ������ �� ����, �������� "���"

        if (NextLetterStart >= 0)
        {
#if DEBUG
          if (NextLetterStart < 2)
            throw new BugException("NextLetterStart=" + NextLetterStart.ToString());
#endif
          if (MoreDigitGroups)
          {
            // ���� ���� ��������� �������� �����, ��������, "���1���2", �� "���2" ���������� �� ��������� �����
            sOthers = s.Substring(NextLetterStart) + sOthers;
            s = s.Substring(0, NextLetterStart);
          }
          // � ����� - ��� ����� �������� ������, ��������, "���1�"
        }


        string aoType = s.Substring(0, DigitStart);
        string nm = s.Substring(DigitStart);
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

    #endregion


    #region ��������������� �������������� ������

    /// <summary>
    /// ���������� ������� ��������� ������� ��� ��������� AOGUID
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public FiasLevel GetAOGuidLevel(Guid guid)
    {
      if (guid == Guid.Empty)
        return FiasLevel.Unknown;
      if (guid == FiasTools.GuidNotFound)
        throw new ArgumentException("������������ guid=" + guid.ToString(), "guid");

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAOGuidLevel() started.");

      FiasLevel level = Source.GetGuidInfo(new Guid[1] { guid }, FiasTableType.AddrOb)[guid].Level;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAOGuidLevel() finsihed.");

      return level;
    }

    /// <summary>
    /// ���������� ������� ��������� ������� ��� ��������� �������������� ������.
    /// </summary>
    /// <param name="recId">������������� ������</param>
    /// <returns></returns>
    public FiasLevel GetAORecIdLevel(Guid recId)
    {
      if (recId == Guid.Empty)
        return FiasLevel.Unknown;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAORecIdLevel() started.");

      FiasLevel level = Source.GetRecIdInfo(new Guid[1] { recId }, FiasTableType.AddrOb)[recId].Level;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAORecIdLevel() finished.");

      return level;
    }

    /// <summary>
    /// ���� ������������ ��������������
    /// </summary>
    public DateTime ActualDate { get { return _Source.ActualDate; } }

    /// <summary>
    /// ���������� ���������� �� ���� ������ ��������������.
    /// </summary>
    public FiasDBStat DBStat { get { return _Source.DBStat; } }

    #endregion
  }
}
