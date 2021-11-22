// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ��������� ��� ������ ������ ���������� IEFPCormCreator.CreateForm()
  /// </summary>
  public sealed class EFPFormCreatorParams
  {
    #region ���������� �����������

    internal EFPFormCreatorParams(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      _Config = cfg;
      _ClassName = cfg.GetString("Class");
      _ConfigSectionName = cfg.GetString("ConfigSectionName");
      _Title = cfg.GetString("Title");
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ���� Class
    /// </summary>
    public string ClassName { get { return _ClassName; } }
    private string _ClassName;

    /// <summary>
    /// �������� ���� ConfigName
    /// </summary>
    public string ConfigSectionName { get { return _ConfigSectionName; } }
    private string _ConfigSectionName;

    /// <summary>
    /// ��� ������ ��� ����������� ����� �� ������ ������������� "Composition"-"UI"
    /// </summary>
    public CfgPart Config { get { return _Config; } }
    private CfgPart _Config;

    /// <summary>
    /// ��������� �����
    /// </summary>
    public string Title { get { return _Title; } }
    private string _Title;

    /// <summary>
    /// ��������� ������������� ���������� ��������� ����
    /// ��� "[ ��� ��������� ]"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(Title))
        return "[ ��� ��������� ]";
      else
        return Title;
    }

    #endregion
  }

  /// <summary>
  /// �������, ����������� ���������, ������ ���� ��������� � ������ EFPApp.FormCreators
  /// </summary>
  public interface IEFPFormCreator
  {
    /// <summary>
    /// ������� ����� �� �������� ����������.
    /// ����� ���������� ��������� ����� ��� null, ���� ���������� ClassName �� �������������� ���� ��������.
    /// ���� ����� �� ����� ���� ������� �� ������ ��������, ���������, ��-�� ���������� � ������������ ����,
    /// ������ ���� ��������� ����������� ����������.
    /// </summary>
    /// <param name="creatorParams">������ ���������� ��� �������� �����</param>
    /// <returns>��������� ����� ��� null</returns>
    Form CreateForm(EFPFormCreatorParams creatorParams);
  }
}
