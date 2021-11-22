// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Data;
using System.Windows.Forms;

namespace FreeLibSet.Forms.Docs
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
