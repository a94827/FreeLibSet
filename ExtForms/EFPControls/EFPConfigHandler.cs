// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  #region ������������ EFPConfigPurpose

  /// <summary>
  /// ���������� ��������, ������������ EFPConfigHandler
  /// </summary>
  public enum EFPConfigPurpose
  {
    /// <summary>
    /// ������/������ ������� ������ ������������ � ������, �������� ��������� ConfigSectionName
    /// </summary>
    Config,

    /// <summary>
    /// ������/���������� ���������� �������� ���� ����������������� ����������
    /// </summary>
    Composition,

    /// <summary>
    /// ���� ���� ������� ������ ������������ ��� ��������������� �������� ��� ������ �����
    /// </summary>
    Preload,
  }

  #endregion

  /// <summary>
  /// �������������� ����������, ������������ ������� ���������� IEFPConfigurable.
  /// � ������� ���������� �������� ������ ���������� �������� (������/������ ������� ������ ������������
  /// ��� ���������� ����)
  /// </summary>
  public class EFPConfigActionInfo : IReadOnlyObject
  {
    #region ��������

    /// <summary>
    /// ���������� ������������ ��������
    /// </summary>
    public EFPConfigPurpose Purpose
    {
      get { return _Purpose; }
      set
      {
        CheckNotReadOnly();
        _Purpose = value;
      }
    }
    private EFPConfigPurpose _Purpose;

    /// <summary>
    /// ���������� Purpose
    /// </summary>
    /// <returns>�������� �������������</returns>
    public override string ToString()
    {
      return Purpose.ToString();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ������ ��� ��������� � ����� "������ ������" ������� SetReadOnly().
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ����������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// ��������� ������ � ����� "������ ������".
    /// ���������� �� EFPConfigHandler
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }

  /// <summary>
  /// ���������, ����������� EFPControlBase, EFPFormProvider � ������� ��������, ������������� EFPControlHandler.
  /// ���������� ������ ������ / ������ ������ ������������ � ��������� ������ ���������
  /// </summary>
  public interface IEFPConfigurable
  {
    #region ������

    /// <summary>
    /// �������� ������ ���������, ������� ������ ���� ����������
    /// </summary>
    /// <param name="categories">������ ��� ���������� ���������</param>
    /// <param name="rwMode">������ ��� ������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo);

    /// <summary>
    /// ��������� ������ ����� ������ ������������
    /// </summary>
    /// <param name="category">��������� ������������ ������</param>
    /// <param name="cfg">������ ��� �����b ��������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo);

    /// <summary>
    /// ��������� ������ ����� ������ ������������
    /// </summary>
    /// <param name="category">��������� ����������� ������</param>
    /// <param name="cfg">������ ��� ������ ��������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo);

    #endregion
  }

  /// <summary>
  /// �������� ��� ������ ������������, ������ ���������, ������ ��������� � ������� ������/������. 
  /// ������������ � EFPFormProvider � EFPControlBase.
  /// �� ����������� ��� ���������� ������� � ���������������� �������, �.� �� �������� ������� UserSetName.
  /// 
  /// ���������� ���������� IEFPConfigurable ��������� ����� �������������� ����������
  /// </summary>
  public sealed class EFPConfigHandler : IEFPConfigurable
  {
    #region �����������

    /// <summary>
    /// ������� ����� ������ ��� ��������� ConfigSectionName.
    /// ������ ��������� ������.
    /// </summary>
    public EFPConfigHandler()
    {
      _Sources = new List<IEFPConfigurable>();
      _Changed = new ChangeFlags(this);
      _ConfigSectionName = String.Empty;
      _Categories = new SingleScopeStringList(false);
    }

    #endregion

    #region �������� ConfigSectionName

    /// <summary>
    /// ��� ������ ������������.
    /// �� ��������� � ������ ������.
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName; }
      set
      {
        if (value == null)
          _ConfigSectionName = String.Empty;
        else
          _ConfigSectionName = value;
      }
    }
    private string _ConfigSectionName;

    #endregion

    #region ��������� ������

    /// <summary>
    /// ������ ���������� ������.
    /// EFPControlBase � EFPFormProvider ��������� ���� � ���� ������ ����� ����� �������� EFPConfigHandler
    /// </summary>
    public IList<IEFPConfigurable> Sources { get { return _Sources; } }
    private List<IEFPConfigurable> _Sources;

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// ���������� ������ ���������.
    /// ���������� ��� ��������� ������ � ������ Sources.
    /// </summary>
    /// <param name="categories">����������� ������ ���������</param>
    /// <param name="rwMode">�����: ������ ��� ������ ������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      for (int i = 0; i < _Sources.Count; i++)
        _Sources[i].GetConfigCategories(categories, rwMode, actionInfo);
    }

    /// <summary>
    /// ���������� ������ ������������.
    /// �������� ������ ���� �������� �� ������ Sources
    /// </summary>
    /// <param name="category">���������</param>
    /// <param name="cfg">������������ ������ ������������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    public void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      for (int i = 0; i < _Sources.Count; i++)
        _Sources[i].WriteConfigPart(category, cfg, actionInfo);
    }

    /// <summary>
    /// ��������� ������ ������������.
    /// �������� ������ ���� �������� �� ������ Sources
    /// </summary>
    /// <param name="category">���������</param>
    /// <param name="cfg">����������� ������ ������������</param>
    /// <param name="actionInfo">���������� � ����������� ��������</param>
    public void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      for (int i = 0; i < _Sources.Count; i++)
        _Sources[i].ReadConfigPart(category, cfg, actionInfo);
    }

    #endregion

    #region ������

    /// <summary>
    /// ��� �������� �������� ���������������� �������� Changed.
    /// </summary>
    public sealed class ChangeFlags
    {
      #region ���������� �����������

      internal ChangeFlags(EFPConfigHandler owner)
      {
        _Owner = owner;
        _Flags = new Dictionary<string, object>();
      }

      #endregion

      #region ��������

      private EFPConfigHandler _Owner;

      /// <summary>
      /// ��������� ������������� �������
      /// ���� - ���������
      /// �������� - �� ������������
      /// </summary>
      private Dictionary<string, object> _Flags;

      /// <summary>
      /// ������ ��� ��������� �������� ��������� ��� ���������
      /// ��� �������� ���������������, ��������, ����� ��������� ��������.
      /// ����� ��������� �����, �� ������� ����� ������ ����� ReadConfig().
      /// ����, ��������, ���� CurrentId ����� �������� � ������ "GridView" ������ ��� �������� �����, 
      /// ������� ���������� ���� � ����������� OnHidden() ����� ������� ������ �������� ������.
      /// ������ ����� �������� � ��� �������������� ���������. ��� ������������.
      /// �����, �.�. ������� ��� ����� ������ ��������� �� ������, ������ Remove().
      /// </summary>
      /// <param name="category">���������</param>
      /// <returns>������� ���������</returns>
      public bool this[string category]
      {
        get { return _Flags.ContainsKey(category); }
        set
        {
          if (value)
            _Flags[category] = null;
          else
            _Flags.Remove(category);
        }
      }

      #endregion

      #region ������

      internal void Clear()
      {
        _Flags.Clear();
      }

      internal bool IsEmpty { get { return _Flags.Count == 0; } }

      #endregion
    }

    /// <summary>
    /// ������ ��������� �� ����������
    /// </summary>
    public ChangeFlags Changed { get { return _Changed; } }
    private ChangeFlags _Changed;

    #endregion

    #region ������� EFPConfigActionInfo

    static EFPConfigHandler()
    {
      _ConfigInfoObject = new EFPConfigActionInfo();
      _ConfigInfoObject.Purpose = EFPConfigPurpose.Config;
      _ConfigInfoObject.SetReadOnly();

      _PreloadInfoObject = new EFPConfigActionInfo();
      _PreloadInfoObject.Purpose = EFPConfigPurpose.Preload;
      _PreloadInfoObject.SetReadOnly();

      _CompositionInfoObject = new EFPConfigActionInfo();
      _CompositionInfoObject.Purpose = EFPConfigPurpose.Composition;
      _CompositionInfoObject.SetReadOnly();
    }

    private static readonly EFPConfigActionInfo _ConfigInfoObject;
    private static readonly EFPConfigActionInfo _CompositionInfoObject;
    private static readonly EFPConfigActionInfo _PreloadInfoObject;

    #endregion

    #region ������ � ������

    /// <summary>
    /// ������������ ������ ��� ����� ������ ���������
    /// </summary>
    private SingleScopeStringList _Categories;

    /// <summary>
    /// ���������� ��� ������, ���������� �� �������.
    /// ������ ������������.
    /// ���� �������� ConfigSectionName �� �����������, ������� �������� �� �����������.
    /// ����� �������������� ����� ������ ������������� �������� IEFPConfigManager.Preload()
    /// </summary>
    /// <param name="configManager">�������� ������ ������������ EFPApp.ConfigManager</param>
    public void WriteConfig(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new NullReferenceException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        CfgPart cfg;
        using (configManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
        {
          WriteConfigPart(_Categories[i], cfg, _ConfigInfoObject);
        }
      }

      Changed.Clear();
    }

    /// <summary>
    /// ���������� ������ ���������� ������ � ���������� ������   
    /// ���� �������� ConfigSectionName �� �����������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="configManager">�������� ������ ������������ EFPApp.ConfigManager</param>
    public void WriteConfigChanges(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new NullReferenceException("ConfigManager");

      if (ConfigSectionName.Length == 0)
        return;

      if (Changed.IsEmpty)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        if (Changed[_Categories[i]])
        {
          EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
          CfgPart cfg;
          using (configManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
          {
            WriteConfigPart(_Categories[i], cfg, _ConfigInfoObject);
          }
        }
      }

      Changed.Clear();
    }


    /// <summary>
    /// ��������� ������ ��� ���� ���������, ���������� ������
    /// ���� �������� ConfigSectionName �� �����������, ������� �������� �� �����������.
    /// ����� �������������� ����� ������ ������������� �������� IEFPConfigManager.Preload()
    /// </summary>
    /// <param name="configManager">�������� ������ ������������ EFPApp.ConfigManager</param>
    public void ReadConfig(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new NullReferenceException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Read, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        CfgPart cfg;
        using (configManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
        {
          ReadConfigPart(_Categories[i], cfg, _ConfigInfoObject);
        }
      }

      Changed.Clear();
    }


    /// <summary>
    /// �������� Write ��� ���� ���������, �������� �� ������. ������ �� ������������.
    /// ��� ������ ��������� ��������� ��������� ������ � ������, ������ ���������.
    /// ������������ ��� ���������� ���������� �������� �����.
    /// ������ ������ �� ������� �� �������� ConfigSectionName.
    /// </summary>
    /// <param name="cfg">������ ��� �������� �������� ���������</param>
    public void WriteComposition(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");


      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _CompositionInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        CfgPart cfgChild = cfg.GetChild(_Categories[i], true);

        WriteConfigPart(_Categories[i], cfgChild, _CompositionInfoObject);
      }
    }


    /// <summary>
    /// �������� Read ��� ���� ���������.
    /// ��� ������ ��������� ������������ ��������� ������ � ������, ������ ���������.
    /// ���� �����-���� ������ ���, ������� Read �� ����������
    /// ������������ ��� �������������� ���������� �������� �����. ������ ������������.
    /// ������ ������ �� ������� �� �������� ConfigSectionName.
    /// </summary>
    /// <param name="cfg">������ ��� �������� �������� ���������</param>
    public void ReadComposition(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Read, _CompositionInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        CfgPart cfgChild = cfg.GetChild(_Categories[i], false);
        if (cfgChild == null)
          cfgChild = CfgPart.Empty; // �������� ReadConfigPart() ��� ����� �����, ����� �������� �������� � ������������� ���������
        ReadConfigPart(_Categories[i], cfgChild, _CompositionInfoObject);
      }
    }

    #endregion

    #region ��������������� ������

    /// <summary>
    /// ��������� � ������ ������ ������������ ��� ����������� ��������������� ��������
    /// </summary>
    /// <param name="configInfos">����������� ������</param>
    /// <param name="rwMode">��������������� ����� ������ - ������ ��� ������ ������</param>
    public void GetPreloadConfigSections(ICollection<EFPConfigSectionInfo> configInfos, EFPConfigMode rwMode)
    {
      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, rwMode, _PreloadInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        configInfos.Add(configInfo);
      }
    }

    #endregion
  }

  /// <summary>
  /// ��������� ������/������ ������ ������������ ����� ���������.
  /// ���� ������ ����� ���� �������� � ������ EFPConfigHandler.Sources
  /// </summary>
  public sealed class EFPConfigCategorySuppressor : IEFPConfigurable
  {
    #region �����������

    /// <summary>
    /// ������� ����������
    /// </summary>
    /// <param name="suppressedCategory">���������, ������/������ ������� ����� ���������</param>
    public EFPConfigCategorySuppressor(string suppressedCategory)
    {
      if (String.IsNullOrEmpty(suppressedCategory))
        throw new ArgumentNullException("suppressedCategory");
      _SuppressedCategory = suppressedCategory;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����������� ���������
    /// </summary>
    public string SuppressedCategory { get { return _SuppressedCategory; } }
    private string _SuppressedCategory;

    /// <summary>
    /// ���������� ���������� ����������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return "SuppressedCategory=" + SuppressedCategory;
    }

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// ������� �� ������ ��������� SuppressedCategory
    /// </summary>
    /// <param name="categories">������������ ������</param>
    /// <param name="rwMode">������������</param>
    /// <param name="actionInfo">������������</param>
    public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      categories.Remove(SuppressedCategory);
    }

    void IEFPConfigurable.WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    void IEFPConfigurable.ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    #endregion
  }
}
