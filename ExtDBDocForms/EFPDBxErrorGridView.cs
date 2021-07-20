using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtDB.Docs;

namespace AgeyevAV.ExtForms.Docs
{
  #region EFPDBxErrorGridViewDocSelEventArgs

  /// <summary>
  /// ��������� ������� EFPDBxErrorGridView.GetDocSel
  /// </summary>
  public class EFPDBxErrorGridViewDocSelEventArgs : EventArgs
  {
    #region �����������

    /// <summary>
    /// ��������� � EFPDBxErrorGridView
    /// </summary>
    /// <param name="items"></param>
    /// <param name="ui"></param>
    /// <param name="reason"></param>
    public EFPDBxErrorGridViewDocSelEventArgs(ErrorMessageItem[] items, DBUI ui, EFPDBxGridViewDocSelReason reason)
    {
      _Items = items;
      _UI = ui;
      _DocSel = new DBxDocSelection(ui.DocProvider.DBIdentity);
      _Reason = reason;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �� �������, ��� ������� ��������� ������� ������� ����������
    /// </summary>
    public ErrorMessageItem[] Items { get { return _Items; } }
    private ErrorMessageItem[] _Items;

    /// <summary>
    /// ������ � ���������� ����������
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    /// <summary>
    /// �������, �� ������� ��������� ������� �������
    /// </summary>
    public EFPDBxGridViewDocSelReason Reason { get { return _Reason; } }
    private EFPDBxGridViewDocSelReason _Reason;

    /// <summary>
    /// ���� ������ ���� ��������� ������ �� ���������
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private DBxDocSelection _DocSel;

    #endregion

    #region ������

    /// <summary>
    /// �������� ������ �� �������� � �������.
    /// ����� ����������� ��� ��������� ���������
    /// </summary>
    /// <param name="docTypeName">��� ���������</param>
    /// <param name="docId">������������� ���������</param>
    public void Add(string docTypeName, Int32 docId)
    {
      UI.DocTypes[docTypeName].PerformGetDocSel(DocSel, docId, Reason);
    }

    #endregion
  }

  /// <summary>
  /// ������� ������� EFPDBxErrorGridView.GetDocSel
  /// </summary>
  /// <param name="sender">������ EFPDBxErrorGridView</param>
  /// <param name="args">��������� �������</param>
  public delegate void EFPDBxErrorGridViewDocSelEventHandler(object sender,
    EFPDBxErrorGridViewDocSelEventArgs args);

  #endregion

  /// <summary>
  /// ���������� ���������� ��������� �� ������� ������ ��� ������ �� �������� �� ���������.
  /// ����������� ������� GetDocSel.
  /// ������������ �������� � �������������� ����������, �� ������� ���� ������ � ���������� � ������.
  /// </summary>
  public class EFPDBxErrorGridView : EFPErrorDataGridView
  {
    #region ������������

    /// <summary>
    /// ������� ��������� ���������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">��������� ��������</param>
    /// <param name="ui">��������� ������� � ����������</param>
    public EFPDBxErrorGridView(EFPBaseProvider baseProvider, DataGridView control, DBUI ui)
      : base(baseProvider, control)
    {
      Init(ui);
    }

    /// <summary>
    /// ������� ��������� ���������� ���������
    /// </summary>
    /// <param name="controlWithPreview">��������� �������� � ������ ������������</param>
    /// <param name="ui">��������� ������� � ����������</param>
    public EFPDBxErrorGridView(IEFPControlWithToolBar<DataGridView> controlWithPreview, DBUI ui)
      : base(controlWithPreview)
    {
      Init(ui);
    }

    private void Init(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      base.CanView = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������ � ����������� �� ������� �������.
    /// �������� � ������������
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// �������� ���������� ������� ErrorMessagesChanged, ���� �� �����������.
    /// </summary>
    protected override void OnErrorMessagesChanged()
    {
      base.OnErrorMessagesChanged();
      InitEditCommandItems();
    }

    /// <summary>
    /// ������������� �������� ReadOnly ����� ��������� ������ ��������� � ������������� �����������.
    /// ����������� ������� ����������� ������� GetDocSel
    /// </summary>
    protected override void InitEditCommandItems()
    {
      base.InitEditCommandItems();

      CanView = HasGetDocSelHandler && ErrorMessages != null;
      if (HasGetDocSelHandler)
      {
        base.CanMultiEdit = true;
        ReadOnly = (ErrorMessages == null);
        // if (Control == null)
        if (CommandItems.Control == null) // 05.12.2016
          CommandItems.EnterAsOk = false;
      }
      // ����� ������� ����� ��������� ��������� ��������
    }

    #endregion

    #region ��������������

    /// <summary>
    /// �������������� ��� �������� ����������.
    /// �������� ���������� ������� GetDocSel ��� ��������� ������� ����������.
    /// �����, ���� �������� �������� �������, ��������� ����������� �� �������� ��� ��������������.
    /// </summary>
    /// <param name="args">�� ������������</param>
    /// <returns>���������� true, ���� ������� ���� ����������</returns>
    protected override bool OnEditData(EventArgs args)
    {
      if (State == EFPDataGridViewState.Edit && base.HasEditMessageHandler)
        return base.OnEditData(args);

      switch (State)
      {
        case EFPDataGridViewState.Edit:
        case EFPDataGridViewState.View:
          DBxDocSelection DocSel = CreateDocSel(EFPDBxGridViewDocSelReason.Copy);
          if (DocSel != null)
          {
            if (!DocSel.IsEmpty)
            {
              // ������ ����� ������ ������� ��� ��������������
              string DocTypeName = DocSel.TableNames[0];
              Int32[] DocIds = DocSel[DocTypeName];
              if (DocIds.Length > 1 && (!UI.DocTypes[DocTypeName].CanMultiEdit))
                UI.ShowDocSel(DocSel); // ��������� �������������� ���������
              else
                UI.DocTypes[DocTypeName].PerformEditing(DocIds, State, false);
              return true;
            }
          }

          EFPApp.ShowTempMessage("��� ��������� � ���������� ����������");
          return true;
      }

      return base.OnEditData(args);
    }

    #endregion

    #region ������� ���������� ����

    /// <summary>
    /// ���������� ������� ���������� ����
    /// </summary>
    public new EFPDBxErrorGridViewCommandItems CommandItems { get { return (EFPDBxErrorGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// ������� EFPDBxErrorGridViewCommandItems
    /// </summary>
    /// <returns>��������� ������ ������</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPDBxErrorGridViewCommandItems(this);
    }

    #endregion

    #region ������� GetDocSel

    /// <summary>
    /// ���� ���������� ����������, �� ��� ����������� ����� � ����� ������ �����
    /// �������� ������� ���������� (������ DBxDocSelection).
    /// ����� ����� ��������� ������� "���������" -> "�������"
    /// </summary>
    public event EFPDBxErrorGridViewDocSelEventHandler GetDocSel
    {
      add
      {
        _GetDocSel += value;
        InitEditCommandItems();
      }
      remove
      {
        _GetDocSel -= value;
        InitEditCommandItems();
      }
    }
    private EFPDBxErrorGridViewDocSelEventHandler _GetDocSel;

    /// <summary>
    /// � ������ ��������������� ������ ����� ������ ���� �������������� �������� HasGetDocSelHandler
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnGetDocSel(EFPDBxErrorGridViewDocSelEventArgs args)
    {
      if (_GetDocSel != null)
        _GetDocSel(this, args);
    }

    /// <summary>
    /// ���������� true, ���� ���� ������������� ���������� GetDocSel
    /// </summary>
    public virtual bool HasGetDocSelHandler { get { return _GetDocSel != null; } }

    /// <summary>
    /// �������� ������� ���������� ��� ��������� ����� � ��������� ���������.
    /// </summary>
    /// <param name="reason">������� �������� �������</param>
    /// <returns>������� ���������� ��� null</returns>
    public DBxDocSelection CreateDocSel(EFPDBxGridViewDocSelReason reason)
    {
      return CreateDocSel(reason, null);
    }

    /// <summary>
    /// �������� ������� ���������� ��� ��������� ����� � ��������� ���������.
    /// </summary>
    /// <param name="reason">������� �������� �������</param>
    /// <param name="rowIndices">������� �����</param>
    /// <returns>������� ���������� ��� null</returns>
    public DBxDocSelection CreateDocSel(EFPDBxGridViewDocSelReason reason, int[] rowIndices)
    {
      if (!HasGetDocSelHandler)
        return null;

      if (ErrorMessages == null)
        return null;
      if (ErrorMessages.Count == 0)
        return null;

      if (rowIndices == null)
        rowIndices = base.SelectedRowIndices;

      ErrorMessageItem[] Items = new ErrorMessageItem[rowIndices.Length];
      for (int i = 0; i < rowIndices.Length; i++)
        Items[i] = ErrorMessages[rowIndices[i]];

      DBxDocSelection DocSel = null;
      try
      {
        EFPApp.BeginWait("�������� ������� ����������", "�������");
        try
        {
          EFPDBxErrorGridViewDocSelEventArgs Args = new EFPDBxErrorGridViewDocSelEventArgs(Items, UI, reason);
          OnGetDocSel(Args);
          if (!Args.DocSel.IsEmpty)
            DocSel = Args.DocSel;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ �������� ������� ���������� ��� ���������� ���������");
      }
      return DocSel;
    }

    #endregion
  }


  /// <summary>
  /// ������� ���������� ���� ��� EFPDBxErrorGridView.
  /// ��������� ������� "���������" - "������� ����������".
  /// ������������ ����������� ������� ���������� � ����� ������
  /// </summary>
  public class EFPDBxErrorGridViewCommandItems : EFPDataGridViewCommandItems
  {
    #region �����������

    /// <summary>
    /// ������������ EFPDBxErrorGridView
    /// </summary>
    /// <param name="controlProvider">��������� ���������� ���������</param>
    public EFPDBxErrorGridViewCommandItems(EFPDBxErrorGridView controlProvider)
      : base(controlProvider)
    {
    }

    #endregion

    #region ����� ��������

    /// <summary>
    /// ��������� ���������� ���������
    /// </summary>
    public new EFPDBxErrorGridView Owner { get { return (EFPDBxErrorGridView)(base.Owner); } }

    /// <summary>
    /// ��������� ������� "���������" - "������� ����������"
    /// </summary>
    protected override void BeforeControlAssigned()
    {
      base.BeforeControlAssigned();


      if (Owner.HasGetDocSelHandler /*&& AccDepMainMenu.MenuSendTo != null*/) // ���� ����������
      {
        EFPCommandItem ci = new EFPCommandItem("Send", "DocSel");
        ci.MenuText = "������� ����������";
        ci.ImageKey = "DBxDocSelection";
        ci.Parent = base.MenuSendTo;
        ci.Click += ciSendToDocSel_Click;
        ci.Usage = EFPCommandItemUsage.Menu; // ��� ������
        Add(ci);
      }
    }

    #endregion

    #region ����� ������

    /// <summary>
    /// ��������� ������� ���������� (������ DBxDocSelection) � ����� �������� ��� ������ ������
    /// </summary>
    /// <param name="args"></param>
    protected override void OnAddCopyFormats(DataObjectEventArgs args)
    {
      DBxDocSelection DocSel = Owner.CreateDocSel(EFPDBxGridViewDocSelReason.Copy);
      if (DocSel != null)
        args.DataObject.SetData(DocSel);

      base.OnAddCopyFormats(args);
    }

    #endregion

    #region ���������

    private void ciSendToDocSel_Click(object sender, EventArgs args)
    {
      DBxDocSelection DocSel = Owner.CreateDocSel(EFPDBxGridViewDocSelReason.SendTo);
      if (DocSel == null || DocSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("������� �� �������� ����������");
        return;
      }
      Owner.UI.ShowDocSel(DocSel);
    }

    #endregion
  }

  /// <summary>
  /// �������� ������ �� ������� ������
  /// </summary>
  public class EFPReportDBxErrorMessageListPage : EFPReportErrorMessageListPage
  {
    #region �����������

    /// <summary>
    /// ������� �������� ������
    /// </summary>
    /// <param name="ui">��������� ��� ������� � ����������</param>
    public EFPReportDBxErrorMessageListPage(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ������� � ����������
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region ��������� ��������

    /// <summary>
    /// ������� EFPDBxErrorGridView 
    /// </summary>
    /// <param name="control">��������� ��������</param>
    /// <returns>��������� ������������ ��������</returns>
    protected override EFPErrorDataGridView CreateControlProvider(DataGridView control)
    {
      EFPDBxErrorGridView ControlProvider = new EFPDBxErrorGridView(BaseProvider, control, UI);
      if (GetDocSel != null)
        ControlProvider.GetDocSel += new EFPDBxErrorGridViewDocSelEventHandler(ControlProvider_GetDocSel);
      return ControlProvider;
    }

    #endregion

    #region ������� GetDocSel

    /// <summary>
    /// ������� ��� ��������� ������� ����������
    /// </summary>
    public event EFPDBxErrorGridViewDocSelEventHandler GetDocSel;

    void ControlProvider_GetDocSel(object sender, EFPDBxErrorGridViewDocSelEventArgs args)
    {
      if (GetDocSel != null)
        GetDocSel(this, args);
    }

    #endregion
  }
}
