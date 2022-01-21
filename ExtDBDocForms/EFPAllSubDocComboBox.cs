using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Data.Docs;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ��������� ����������, ������� ������������ ��� ������ ������� EFPSubDocGridView � ��������� ���������.
  /// ������������, � ��������, ��� �������������, ��������������� ��� �������� ��������� "������-��-������".
  /// ��������� �� ������������ ��� ������ ����-����, � ������ ��� ������ ��������� �������������� � ��������� �������� �������������.
  /// </summary>
  public class EFPAllSubDocComboBox:EFPUserSelComboBox
  {
    #region ������������

    /// <summary>
    /// ������� ��������� ��� ����������� � �������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� ����������</param>
    /// <param name="mainEditor">�������� ��������� ���������, �� ������� �������� ����������� ���������</param>
    /// <param name="subDocs">������ ��������������� �������������</param>
    public EFPAllSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
      : base(baseProvider, control)
    {
      Init(mainEditor, subDocs, mainEditor.UI);
    }

    /// <summary>
    /// ������� ��������� ��� ����������� ��������� ������ ������������� ��� ����������� � �������� ���������.
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� ����������</param>
    /// <param name="subDocs">������ ��������������� �������������</param>
    /// <param name="ui">���������������� ��������� ��� ����������</param>
    public EFPAllSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DBxMultiSubDocs subDocs, DBUI ui)
      : base(baseProvider, control)
    {
      Init(null, subDocs, ui);
    }

    private void Init(DocumentEditor mainEditor, DBxMultiSubDocs subDocs, DBUI ui)
    {
#if DEBUG
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");
      if (ui == null)
        throw new ArgumentNullException("ui");
#endif

      _MainEditor = mainEditor;

      _SubDocs = subDocs;
      _SubDocTypeUI = ui.DocTypes[subDocs.Owner.DocType.Name].SubDocTypes[subDocs.SubDocType.Name]; // ����� �������� �������� ������

      _ValidateBeforeEdit = false;

      _ConfirmDeletion = true;

      Control.PopupClick += new EventHandler(Control_PopupClick);
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� �������� ���������, ������������ ������ ��������.
    /// �������� � ������������.
    /// ����� ���� null, ���� �������� ������������ ��� ���������.
    /// </summary>
    public DocumentEditor MainEditor { get { return _MainEditor; } }
    private DocumentEditor _MainEditor;

    /// <summary>
    /// ��� ������������� �������������. �������� � ������������.
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocTypeUI.SubDocType; } }

    /// <summary>
    /// ������������� ������������ (������ �������). ������������ � ������������.
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// ��������� ��� ������� � ����������
    /// </summary>
    public DBxDocProvider DocProvider { get { return _SubDocs.DocSet.DocProvider; } }

    /// <summary>
    /// ��� �������, ���������������� ��� ������ ���������� ����� ��� null, ����
    /// ������ ����������� �����-�� ������ ��������
    /// </summary>
    public string ManualOrderColumn
    {
      get { return _ManualOrderColumn; }
      set { _ManualOrderColumn = value; }
    }
    private string _ManualOrderColumn;

    /// <summary>
    /// ��������� ������� � �������������
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;


    /// <summary>
    /// ���� ����������� � true, �� ����� ����������� � ��������������� �������
    /// ���������� MainEditor.ValidateDate(). � ���� ������ �������� ������������
    /// ����� ������������ ���������� �������� ����� ��������� ���������
    /// �� ��������� (false) �������� �� �����������. ����������� ��������������
    /// �������������, ���� ���� �� �����-���� ������� ��������� ��������� ���������
    /// ���� ����������� ����������� ����.
    /// </summary>
    public bool ValidateBeforeEdit
    {
      get { return _ValidateBeforeEdit; }
      set
      {
        if (value && _MainEditor == null)
          throw new InvalidOperationException("������ ������������� �������� ValidateBeforeEdit � true, �.�. �������� �� ��������� � DocumentEditor"); // 21.01.2022
        _ValidateBeforeEdit = value;
      }
    }
    private bool _ValidateBeforeEdit;

    /// <summary>
    /// ���� true (�� ���������), �� ����� ��������� ��������� ������������� ������������� �������������.
    /// ���� false, �� �������� ����������� ���������� ��� �������.
    /// </summary>
    public bool ConfirmDeletion { get { return _ConfirmDeletion; } set { _ConfirmDeletion = value; } }
    private bool _ConfirmDeletion;


    #endregion   

    #region ���������� ������

    void Control_PopupClick(object sender, EventArgs args)
    {
      OKCancelGridForm form = new OKCancelGridForm();
      form.Text = this.DisplayName;
      form.Icon = EFPApp.MainImageIcon(this.SubDocTypeUI.TableImageKey);
      WinFormsTools.OkCancelFormToOkOnly(form);
      EFPSubDocGridView efpGrid;
      if (MainEditor == null)
        efpGrid = new EFPSubDocGridView(form.ControlWithToolBar, SubDocs, SubDocTypeUI.UI);
      else
        efpGrid = new EFPSubDocGridView(form.ControlWithToolBar, MainEditor, SubDocs);

      // �������� ��������
      efpGrid.ManualOrderColumn = this.ManualOrderColumn;
      efpGrid.ValidateBeforeEdit = this.ValidateBeforeEdit;
      efpGrid.ConfirmDeletion = this.ConfirmDeletion;

      EFPDialogPosition dlgPos=new EFPDialogPosition(Control);
      EFPApp.ShowDialog(form, true, dlgPos);
    }

    #endregion
  }
}
