using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

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
  /// ����������� ��������� ��� ���������� �������� ������ ��������������.
  /// �������� ������� ����� ������������� �� ������������� � FiasDB. ����� ����� ������ ���������� ����������������
  /// </summary>
  [Serializable]
  public sealed class FiasDBSettings : IReadOnlyObject
  {
    #region �����������

    /// <summary>
    /// ������� ������, � ������� ����� ������ ���������
    /// </summary>
    public FiasDBSettings()
    {
      _UseHouse = true;
      _UseRoom = true;
      _RegionCodes = new RegionCodeList();
      _UseOKATO = true;
      _UseOKTMO = true;
      _UseIFNS = true;
      _UseHistory = false;
    }

    //private FiasDBSettings(bool dummy)
    //  : this()
    //{
    //  SetReadOnly();
    //}

    #endregion

    #region ��������

    /// <summary>
    /// ����� �� �������� � ������������� ������ �����?
    /// �� ��������� - true.
    /// </summary>
    public bool UseHouse
    {
      get { return _UseHouse; }
      set
      {
        CheckNotReadOnly();
        _UseHouse = value;
      }
    }
    private bool _UseHouse;

    /// <summary>
    /// ����� �� �������� � ������������� ������ ��������� (�������)?
    /// �� ��������� - true.
    /// </summary>
    public bool UseRoom
    {
      get { return _UseRoom; }
      set
      {
        CheckNotReadOnly();
        _UseRoom = value;
      }
    }
    bool _UseRoom;

    [Serializable]
    private class RegionCodeList : SingleScopeList<string>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    /// <summary>
    /// ������ ���������� ����� �������.
    /// �� ��������� ������ ������, ��� �������� ��������� � ������������� ���� ��������.
    /// </summary>
    public ICollection<string> RegionCodes { get { return _RegionCodes; } }
    private RegionCodeList _RegionCodes;

    /// <summary>
    /// ����� �� �������� � ������������� ���� � ������ �����?
    /// �� ��������� - true.
    /// </summary>
    public bool UseOKATO
    {
      get { return _UseOKATO; }
      set
      {
        CheckNotReadOnly();
        _UseOKATO = value;
      }
    }
    private bool _UseOKATO;

    /// <summary>
    /// ����� �� �������� � ������������� ���� � ������ �����?
    /// �� ��������� - true.
    /// </summary>
    public bool UseOKTMO
    {
      get { return _UseOKTMO; }
      set
      {
        CheckNotReadOnly();
        _UseOKTMO = value;
      }
    }
    private bool _UseOKTMO;

    /// <summary>
    /// ����� �� �������� � ������������� ���� � ������ ����?
    /// �� ��������� - true.
    /// </summary>
    public bool UseIFNS
    {
      get { return _UseIFNS; }
      set
      {
        CheckNotReadOnly();
        _UseIFNS = value;
      }
    }
    private bool _UseIFNS;

    /// <summary>
    /// ����� �� �������� � ������������� ������������ �������� (true) ��� ������ ���������� (false).
    /// �� ��������� - false - ������ ���������� ��������
    /// </summary>
    public bool UseHistory
    {
      get { return _UseHistory; }
      set
      {
        CheckNotReadOnly();
        _UseHistory = value;
        if (value)
          _UseDates = true;
      }
    }
    private bool _UseHistory;

    /// <summary>
    /// ������� ����� ���� ������ � ��������� �������� �������.
    /// �� ��������� - false.
    /// ��������� �������� UseHistory=true �������� �������������� ��������� � ����� ��������
    /// </summary>
    public bool UseDates
    {
      get { return _UseDates; }
      set
      {
        CheckNotReadOnly();
        _UseDates = value;
      }
    }
    private bool _UseDates;

    #endregion

    #region ������ ��������

    internal void CheckUseHouse()
    {
      if (!_UseHouse)
        throw new FiasDBSettingsException("� ���������� ���� ������ ���� ��������� ������������� ����������� ������ (FiasDBSettings.UseHouse=false)");
    }

    internal void CheckUseRoom()
    {
      if (!_UseRoom)
        throw new FiasDBSettingsException("� ���������� ���� ������ ���� ��������� ������������� ����������� ��������� (FiasDBSettings.UseRoom=false)");
    }

    internal void CheckUseHistory()
    {
      if (!_UseHistory)
        throw new FiasDBSettingsException("� ���������� ���� ������ ���� ��������� ������������� ������������ ������ (FiasDBSettings.UseHistory=false)");
    }

    internal void CheckUseIFNS()
    {
      if (!_UseIFNS)
        throw new FiasDBSettingsException("� ���������� ���� ������ ���� ��������� ������������� ����� ���� (FiasDBSettings.UseIFNS=false)");
    }

    internal void CheckUseOKATO()
    {
      if (!_UseOKATO)
        throw new FiasDBSettingsException("� ���������� ���� ������ ���� ��������� ������������� ����� ����� ����� (FiasDBSettings.UseOKATO=false)");
    }

    internal void CheckUseOKTMO()
    {
      if (!_UseOKTMO)
        throw new FiasDBSettingsException("� ���������� ���� ������ ���� ��������� ������������� ����� ����� (FiasDBSettings.UseOKTMO=false)");
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true ����� ������ ������������ FiasDB.
    /// </summary>
    public bool IsReadOnly
    {
      get { return _RegionCodes.IsReadOnly; }
    }

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      _RegionCodes.CheckNotReadOnly();
    }

    /// <summary>
    /// ��������� ����� �������� � ����� "������ ������".
    /// ���������� ������������� FiasDB.
    /// </summary>
    public void SetReadOnly()
    {
      if (IsReadOnly)
        return;

      if (_UseRoom && (!_UseHouse))
        throw new InvalidOperationException("������������ ���������. �� ����� ���� UseFlats=true ��� UseHouses=false");

      if (_UseHistory && (!_UseDates))
        throw new InvalidOperationException("������������ ���������. �� ����� ���� UseHistory=true ��� UseDates=false");

      _RegionCodes.SetReadOnly();
    }

    #endregion

    #region �������������� � XML � ������ ������������

    /// <summary>
    /// ���������� ��������� � ������ ������������
    /// </summary>
    /// <param name="cfg">����������� ������</param>
    public void WriteConfig(CfgPart cfg)
    {
      cfg.SetBool("UseHouse", UseHouse);
      cfg.SetBool("UseRoom", UseRoom);
      if (RegionCodes.Count == 0)
        cfg.SetString("RegionCodes", String.Empty);
      else
      {
        string[] aRegionCodes = new string[RegionCodes.Count];
        RegionCodes.CopyTo(aRegionCodes, 0);
        cfg.SetString("RegionCodes", String.Join(",", aRegionCodes));
      }
      cfg.SetBool("UseOKATO", UseOKATO);
      cfg.SetBool("UseOKTMO", UseOKTMO);
      cfg.SetBool("UseIFNS", UseIFNS);
      cfg.SetBool("UseHistory", UseHistory);
      cfg.SetBool("UseDates", UseDates);
    }

    /// <summary>
    /// ������ �������� �� ������ ������������.
    /// ���������, ������� ��� � ������, ����������� �������������.
    /// </summary>
    /// <param name="cfg">����������� ������ ������������</param>
    public void ReadConfig(CfgPart cfg)
    {
      CheckNotReadOnly();

      UseHouse = cfg.GetBoolDef("UseHouse", UseHouse);
      if (UseHouse)
        UseRoom = cfg.GetBoolDef("UseRoom", UseRoom);
      else
        UseRoom = false;

      RegionCodes.Clear();
      string sRegionCodes = cfg.GetString("RegionCodes");
      if (!String.IsNullOrEmpty(sRegionCodes))
      {
        string[] aRegionCodes = sRegionCodes.Split(',');
        for (int i = 0; i < aRegionCodes.Length; i++)
          RegionCodes.Add(aRegionCodes[i]);
      }

      UseOKATO = cfg.GetBoolDef("UseOKATO", UseOKATO);
      UseOKTMO = cfg.GetBoolDef("UseOKTMO", UseOKTMO);
      UseIFNS = cfg.GetBoolDef("UseIFNS", UseIFNS);
      UseHistory = cfg.GetBoolDef("UseHistory", UseHistory);
      if (!UseHistory)
        UseDates = cfg.GetBoolDef("UseDates", UseDates);

    }

    /// <summary>
    /// ��������� � ���� ������ XML
    /// </summary>
    public string AsXmlText
    {
      get
      {
        TempCfg cfg = new TempCfg();
        WriteConfig(cfg);
        return cfg.AsXmlText;
      }
      set
      {
        TempCfg cfg = new TempCfg();
        cfg.AsXmlText = value;
        ReadConfig(cfg);
      }
    }

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ��������� ������������� (��� �������)
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("����: ");
      sb.Append(_UseHouse ? "����" : "���");
      if (_UseHouse)
      {
        sb.Append(", ��������: ");
        sb.Append(_UseRoom ? "����" : "���");
      }
      sb.Append(", �������: ");
      if (_RegionCodes.Count == 0)
        sb.Append("���");
      else
      {
        for (int i = 0; i < _RegionCodes.Count; i++)
        {
          if (i > 0)
            sb.Append(',');
          sb.Append(_RegionCodes[i]);
        }
      }

      sb.Append(", ���� �����: ");
      sb.Append(UseOKATO ? "����" : "���");

      sb.Append(", ���� �����: ");
      sb.Append(UseOKTMO ? "����" : "���");

      sb.Append(", ���� ����: ");
      sb.Append(UseIFNS ? "����" : "���");

      sb.Append(", ������������ ��������: ");
      sb.Append(UseHistory ? "����" : "���");

      if (!UseHistory)
      {
        sb.Append(", ���� �������� �������: ");
        sb.Append(UseDates ? "����" : "���");
      }

      return sb.ToString();
    }

    #endregion

    #region ����������� ���������

    /// <summary>
    /// ��������� �� ���������.
    /// � ����� ������� ������ ������ ��������
    /// </summary>
    public static readonly FiasDBSettings DefaultSettings = new FiasDBSettings();

    #endregion
  }

  [Serializable]
  internal enum FiasFTSMode { None, FTS3 }

  /// <summary>
  /// ���������� ��������� ��������������.
  /// �� ������������ � ���������������� ����
  /// </summary>
  [Serializable]
  public sealed class FiasInternalSettings
  {
    #region ��������

    /// <summary>
    /// ��� ���������� ���� ������ - ��������� �� ������ DBxProviderNames
    /// </summary>
    public string ProviderName
    {
      get { return _ProviderName; }
      internal set { _ProviderName = value; }
    }
    private string _ProviderName;

    /// <summary>
    /// ������������� ��������� ������ ��������������� ��� ���������� �������
    /// </summary>
    internal bool UseIdTables;

    /// <summary>
    /// ���������� true ��� ���� ������ SQLite.
    /// ��� ���� ������ ����� "STARTDATE" � "ENDDATE" ���� DATE ������������ ���� "dStartDate" � "dEndDate" ���� INT.
    /// ��� �������������� ���� � ����� ������������ ������� DateTime.ToOADate(), ���� 01.01.1900 ������������� �������� 1.
    /// </summary>
    public bool UseOADates
    {
      get { return _UseOADates; }
      internal set { _UseOADates = value; }
    }
    private bool _UseOADates;

    /// <summary>
    /// ������������� �������������� ������
    /// </summary>
    internal FiasFTSMode FTSMode;

    #endregion
  }
}
