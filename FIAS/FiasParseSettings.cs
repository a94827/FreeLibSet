using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
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
  /// ��������� �������� ������� �� ������ (��� �������).
  /// </summary>                
  public class OldFiasParseSettings
  {
    #region �����������

    /// <summary>
    /// ������� ����� ����������
    /// </summary>
    /// <param name="source">�������� ������ ����. �� ����� ���� null</param>
    public OldFiasParseSettings(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Handler = new FiasHandler(source);
      _BaseAddress = new FiasAddress();
      _EditorLevel = FiasEditorLevel.Room;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ������ ����. �� ����� ���� null.
    /// </summary>
    public IFiasSource Source { get { return _Handler.Source; } }
    private FiasHandler _Handler;

    /// <summary>
    /// ������� �����, �� �������� ����������� �����.
    /// �� ��������� - ������ ����� - ��� ��
    /// </summary>
    public FiasAddress BaseAddress
    {
      get { return _BaseAddress; }
      set
      {
        if (value == null)
          _BaseAddress = new FiasAddress();
        else
          _BaseAddress = value;
      }
    }
    private FiasAddress _BaseAddress;

    /// <summary>
    /// ��������� �������, �� �������� ����������� �������.
    /// �� ��������� - �� ������ ��������/��������� ������������
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        _EditorLevel = value;
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// �������� ������� ��� ����� � ���������
    /// </summary>
    internal FiasLevel InternalBottomLevel
    {
      get
      {
        switch (_EditorLevel)
        {
          case FiasEditorLevel.Village:
            return FiasLevel.Village;
          case FiasEditorLevel.Street:
            return FiasLevel.Street;
          case FiasEditorLevel.House:
            return FiasLevel.Structure;

          case FiasEditorLevel.Room:
            return FiasLevel.Room;

          default:
            throw new BugException("EditorLevel=" + EditorLevel.ToString());
        }
      }
    }

    ///// <summary>
    ///// ������ ��������� ������� ��� �������� BottomLevel
    ///// </summary>
    //public FiasLevel[] AvailableBottomLevels    {     }
    //private static readonly FiasLevel[]_AvailableBottomLevels    =new FiasLevel[]{FiasLevel.Village, FiasLevel.Street, FiasLevel.Structure, FIAS}

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ��������� ������������� ������ ����������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (_BaseAddress.IsEmpty)
        return FiasTools.RF;
      else
        return _Handler.GetTextWithoutPostalCode(_BaseAddress);
    }

    #endregion

    #region ������ � ������ ��������

    /// <summary>
    /// ������ ������ ���������� � ������ ������������
    /// </summary>
    /// <param name="cfg">������ ��� ������</param>
    public void WriteConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      convert.GuidMode = FiasAddressConvertGuidMode.AOGuid;
      cfg.SetString("BaseAddress", convert.ToString(_BaseAddress));
    }

    /// <summary>
    /// ������ ������ ���������� �� ������ ������������
    /// </summary>
    /// <param name="cfg"></param>
    public void ReadConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      _BaseAddress = convert.Parse(cfg.GetString("BaseAddress"));
    }

    #endregion
  }

  /// <summary>
  /// ��������� �������� ������� �� ������, ��������� �� �������.
  /// </summary>                
  public class FiasParseSettings
  {
    #region �����������

    /// <summary>
    /// ������� ����� ����������
    /// </summary>
    /// <param name="source">�������� ������ ����. �� ����� ���� null</param>
    public FiasParseSettings(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Handler = new FiasHandler(source);
      _BaseAddress = new FiasAddress();
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ������ ����. �� ����� ���� null.
    /// </summary>
    public IFiasSource Source { get { return _Handler.Source; } }
    private FiasHandler _Handler;

    /// <summary>
    /// ������� �����, �� �������� ����������� �����.
    /// �� ��������� - ������ ����� - ��� ��
    /// </summary>
    public FiasAddress BaseAddress
    {
      get { return _BaseAddress; }
      set
      {
        if (value == null)
          _BaseAddress = new FiasAddress();
        else
          _BaseAddress = value;
      }
    }
    private FiasAddress _BaseAddress;

    /// <summary>
    /// ������ ��� �������
    /// </summary>
    public FiasLevelSet[] CellLevels
    {
      get { return _CellLevels; }
      set { _CellLevels = value; }
    }
    private FiasLevelSet[] _CellLevels;

    #endregion

    #region �������������� ������

    /// <summary>
    /// ������������� ������ CellLevels ������ ������ ��������, ������ �� ��� �������������� �������� BaseAddress
    /// </summary>
    /// <param name="editorLevel">������� ���������</param>
    public void SetSingle(FiasEditorLevel editorLevel)
    {
      FiasLevelSet set1 = FiasLevelSet.FromBottomLevel(_BaseAddress.NameBottomLevel, true);
      FiasLevelSet set2 = FiasLevelSet.FromEditorLevel(editorLevel);
      _CellLevels = new FiasLevelSet[1] { set2 - set1 };
    }

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ��������� ������������� ������ ����������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (_BaseAddress.IsEmpty)
        return FiasTools.RF;
      else
        return _Handler.GetTextWithoutPostalCode(_BaseAddress);
    }

    #endregion

    #region ������ � ������ ��������

    /// <summary>
    /// ������ ������ ���������� � ������ ������������
    /// </summary>
    /// <param name="cfg">������ ��� ������</param>
    public void WriteConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      convert.GuidMode = FiasAddressConvertGuidMode.AOGuid;
      cfg.SetString("BaseAddress", convert.ToString(_BaseAddress));
      if (_CellLevels != null)
      {
        cfg.SetInt("CellCount", _CellLevels.Length);
        for (int i = 0; i < _CellLevels.Length; i++)
          cfg.SetString("CellLevels" + (i + 1).ToString(), FiasLevelSetConvert.ToString(_CellLevels[i]));
      }
      else
        cfg.SetInt("CellCount", 0);
    }

    /// <summary>
    /// ������ ������ ���������� �� ������ ������������
    /// </summary>
    /// <param name="cfg"></param>
    public void ReadConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      _BaseAddress = convert.Parse(cfg.GetString("BaseAddress"));
      int n = cfg.GetInt("CellCount");
      _CellLevels = new FiasLevelSet[n];
      for (int i = 0; i < n; i++)
      {
        string s = cfg.GetString("CellLevels" + (i + 1).ToString());
        FiasLevelSet ls;
        FiasLevelSetConvert.TryParse(s, out ls);
        _CellLevels[i] = ls;
      }
    }

    #endregion
  }
}
