using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtDB;
using System.Data;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms.Docs
{
  /// <summary>
  /// �������� ������, ���������� ������� �� ������� ���������� ������ ����
  /// </summary>
  public class EFPReportDocGridPage : EFPReportDBxGridPage /* 06.07.2021 */
  {
    // TODO: 06.07.2021. ���� �� ���� ��� ���� �� ��������� DataSource. ��� ���������, ��������, ����� ������������ � ������������ ������ ������������

    #region �����������

    /// <summary>
    /// ������� �������� ������
    /// </summary>
    /// <param name="docTypeUI">��������� ��� ���� ���������</param>
    public EFPReportDocGridPage(DocTypeUI docTypeUI)
      :base(docTypeUI.UI)
    {
      //if (docTypeUI == null)
      //  throw new ArgumentNullException("docTypeUI");
      _DocTypeUI = docTypeUI;

      base.ShowToolBar = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��� ���� ���������
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// �������������� ����� ������������� ��������� ������.
    /// </summary>
    public IdList FixedDocIds
    {
      get
      {
        return _FixedDocIds;
      }
      set
      {
        _FixedDocIds = value;
        if (ControlProvider != null)
          ControlProvider.FixedDocIds = value;
      }
    }
    private IdList _FixedDocIds;

    #endregion

    #region ��������� ��������

    /// <summary>
    /// ��������� ���������� ���������
    /// </summary>
    public new EFPDocGridView ControlProvider
    {
      get { return (EFPDocGridView)(base.ControlProvider); }
    }

    /// <summary>
    /// ������� EFPDocGridView                 
    /// </summary>
    /// <param name="control">��������� �������� Windows Forms</param>
    /// <returns>��������� ������������ ��������</returns>
    protected override EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      EFPDocGridView Res = new EFPDocGridView(BaseProvider, control, _DocTypeUI);
      Res.Filters = new GridFilters();
      Res.CommandItems.CanEditFilters = false; // 19.10.2015
      // 06.07.2021
      // ����� �������� ������� ����������
      //Res.UseGridProducerOrders = false;
      //Res.CustomOrderAllowed = false; // 05.07.2021
      return Res;
    }

    /// <summary>
    /// ������������� �������� EFPDocGridView.FixedDocIds
    /// </summary>
    protected override void InitData()
    {
      if (FixedDocIds != null)
        ControlProvider.FixedDocIds = FixedDocIds;
      base.InitData();
    }


    #endregion
  }
}
