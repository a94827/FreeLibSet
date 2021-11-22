// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  #region ����������

  #region IEFPGridFilter

  /// <summary>
  /// ��������� ������ ���������� ���������.
  /// �������� Code ������ ��� � ������ ������������ ��� ���������� ��������
  /// </summary>
  public interface IEFPGridFilter : IObjectWithCode
  {
    /// <summary>
    /// ������������ ��� �������
    /// </summary>
    string DisplayName { get;}

    /// <summary>
    /// ��������� ������������� ��� �������������� �������� �������.
    /// ���� ������ �� ����������, ���������� ������ ������.
    /// </summary>
    string FilterText { get;}

    /// <summary>
    /// ������� ���� ������� ��� �������������� �������� �������
    /// </summary>
    /// <param name="dialogPosition">����������� ��������� ��� ����� �������. ����� ���� ������, �� �� null</param>
    /// <returns>True, ���� (��������) ���� ����������� �������� �������.
    /// False, ���� ������������ ����� ������ "������".</returns>
    bool ShowFilterDialog(EFPDialogPosition dialogPosition);

    /// <summary>
    /// ���������� ������.
    /// </summary>
    void Clear();

    /// <summary>
    /// ���������� true, ���� ������ �� ����������.
    /// </summary>
    bool IsEmpty { get;}

    /// <summary>
    /// ������ �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    void ReadConfig(CfgPart cfg);

    /// <summary>
    /// ���������� �������� ������� � ������ ������������
    /// </summary>
    /// <param name="cfg">������������ ������ ������������</param>
    void WriteConfig(CfgPart cfg);
  }

  /// <summary>
  /// ��������� �� ���������� �������� IEFPGridFilter
  /// </summary>
  public interface IEFPGridFilterSet : IEnumerable<IEFPGridFilter>
  { 
  }

  #endregion

  #region IEFPGridFilters

  /// <summary>
  /// ������ ��������� �������� ���������� ���������
  /// </summary>
  // ReSharper disable once PossibleInterfaceMemberAmbiguity
  public interface IEFPGridFilters : IList<IEFPGridFilter>, IReadOnlyObject
  {
    /// <summary>
    /// ������������� ���� ������.
    /// ������������ ��� ������ � ������� ������ ��� �������������� ������� �������� �������,
    /// ���� ��� ���� ����������� � ������ ���������.
    /// ���� ��� ���������� ��������� �������� � ����� ����� ������, �� ����� ������������
    /// ���������� ��������, ����� ����� ���� �������� � ����� ������� ������.
    /// ���� �� ���� ��������� ��������� �������� � ������������ ������, �� ������ ������������
    /// ������ ��������, ��� �������������� "������" �������.
    /// </summary>
    string DBIdentity { get;}

    /// <summary>
    /// ����� ������� �� �����
    /// </summary>
    /// <param name="Code">��� ��� ���������� ������������, � �� DisplayName.
    /// ������������ ��������� IEFPGridFilter.Code.</param>
    /// <returns>������ ������� � ������. (-1), ���� ������ �� ������</returns>
    int IndexOf(string Code);

    /// <summary>
    /// ����� ������� �� �����
    /// </summary>
    /// <param name="code">��� ��� ���������� ������������, � �� DisplayName.
    /// ������������ ��������� IEFPGridFilter.Code.</param>
    /// <returns>��������� ������ ��� null</returns>
    IEFPGridFilter this[string code] { get; }

    /// <summary>
    /// ������� ��� ������� � ������, ������� ����� Clear()
    /// </summary>
    void ClearAllFilters();

    /// <summary>
    /// ���������� true, ���� �� ���� ������ �� ����������.
    /// ���������� �������� IsEmpty � ���� �������� � ������.
    /// </summary>
    bool IsEmpty { get;}

    /// <summary>
    /// ��������� ������ ������������ ��� ���� �������� � ������.
    /// ��������������, ��� �������� ������� ������� �������� � ��������� �������� ������.
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    void ReadConfig(CfgPart cfg);

    /// <summary>
    /// ��������� ������ ������������ ��� ���� �������� � ������.
    /// ��������������, ��� �������� ������� ������� �������� � ��������� �������� ������.
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    void WriteConfig(CfgPart cfg);

    /// <summary>
    /// ��������� ������ � ����� ���������.
    /// ����� ����� ������ ��������� ������� � ������, �� ����� ������������� �������� ��������.
    /// ���� ����� ���������� � EFPConfigurableDataGridView.OnLoadConfig() � ���������������� �����.
    /// </summary>
    void SetReadOnly();

    /// <summary>
    /// ������ ���������� ��������.
    /// ���� ����� ���������� ��� ������ ������� ��������� ��������, ����� ������������� 
    /// ������ ������ ������� Changed
    /// </summary>
    void BeginUpdate();

    /// <summary>
    /// ��������� ���������� ��������.
    /// ���� ����� ���������� ����� ������ ������� ��������� ��������.
    /// </summary>
    void EndUpdate();

    ///// <summary>
    ///// ������� ���������� ��� ��������� �������� ������-���� �������, ���� �� ���� ������ BeginUpdate()
    ///// </summary>
    //event EventHandler Changed;

    /// <summary>
    /// ���������� ������ �� ���������� ��������
    /// </summary>
    /// <param name="filterSet"></param>
    void Add(IEFPGridFilterSet filterSet);
  }

  #endregion

  #region IEFPScrollableGridFilter

  /// <summary>
  /// ��������� ������� ���������� ���������, ������� ����� "������������"
  /// ����� / ���� Alt + ��������� �� ���������.
  /// ����������� �������� �� ����
  /// </summary>
  public interface IEFPScrollableGridFilter : IEFPGridFilter
  {
    /// <summary>
    /// ���������� true, ���� ����� ������� � ����������� �������� �������.
    /// ��� ��������, ���� � ������� ����� �����-������ ������ (� �� ������������ ��������)
    /// � ��� ������ �����������.
    /// ������������ ��� ���������� ������� ���������� ���� ���������� ���������.
    /// </summary>
    bool CanScrollUp { get;}

    /// <summary>
    /// ���������� true, ���� ����� ������� � ���������� �������� �������.
    /// ��� ��������, ���� � ������� ����� �����-������ ������ (� �� ������������ ��������)
    /// � ��� ������ �����������.
    /// ������������ ��� ���������� ������� ���������� ���� ���������� ���������.
    /// </summary>
    bool CanScrollDown { get;}

    /// <summary>
    /// ������� � ����������� �������� �������.
    /// </summary>
    void ScrollUp();

    /// <summary>
    /// ������� � ���������� �������� �������.
    /// </summary>
    void ScrollDown();
  }

  #endregion

  #region IEFPGridFilterWithImageKey

  /// <summary>
  /// ���������� ���������� ������� ���������� ���������, ������� ������������ ������ ������ ��� ��������
  /// </summary>
  public interface IEFPGridFilterWithImageKey : IEFPGridFilter
  {
    /// <summary>
    /// �������� ������ ���������� ��� ����������� �� ������ EFPApp.MainImages.
    /// �������� ������������ ������ ��� �������������� �������. ��� ����, ���� ������������ ������ ������,
    /// ������������ ��������� EFPGridFilterTools.DefaultFilterImageKey
    /// </summary>
    string FilterImageKey { get; }
  }

  #endregion

  #endregion


  /// <summary>
  /// �������� ��� ���������� ���������� IEFPGridFilters.
  /// ��������� ������ ������ ��������.
  /// </summary>
  public class EFPDummyGridFilters : DummyList<IEFPGridFilter>, IEFPGridFilters
  {
    #region IEFPGridFilters Members

    string IEFPGridFilters.DBIdentity { get { return String.Empty; } }

    int IEFPGridFilters.IndexOf(string code)
    {
      return -1;
    }

    IEFPGridFilter IEFPGridFilters.this[string code] { get { return null; } }

    void IEFPGridFilters.ClearAllFilters() { }

    bool IEFPGridFilters.IsEmpty { get { return true; } }

    void IEFPGridFilters.ReadConfig(CfgPart cfg) { }

    void IEFPGridFilters.WriteConfig(CfgPart cfg) { }

    void IEFPGridFilters.SetReadOnly() { }

    void IEFPGridFilters.BeginUpdate() { }

    void IEFPGridFilters.EndUpdate() { }

    void IEFPGridFilters.Add(IEFPGridFilterSet filterSet) { }

    #endregion
  }

  /// <summary>
  /// ����������� ������ ��� ������ � ��������� ���������� ���������
  /// </summary>
  public static class EFPGridFilterTools
  {
    #region ������ ��� ���������� ��������

    /// <summary>
    /// ������ � EFPApp.MainImages ��� �������������� (���������) �������
    /// </summary>
    public const string DefaultFilterImageKey = "Filter";

    /// <summary>
    /// ������ � EFPApp.MainImages ��� ������ �������
    /// </summary>
    public const string NoFilterImageKey = "No";

    #endregion
  }
}
