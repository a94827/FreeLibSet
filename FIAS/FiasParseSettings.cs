// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;

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
