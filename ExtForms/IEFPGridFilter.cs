using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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
