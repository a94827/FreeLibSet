// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
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
        if (!CommandItems.IsReadOnly) // 05.12.2016
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
          DBxDocSelection docSel = CreateDocSel(EFPDBxGridViewDocSelReason.Copy);
          if (docSel != null)
          {
            if (!docSel.IsEmpty)
            {
              // ������ ����� ������ ������� ��� ��������������
              string docTypeName = docSel.TableNames[0];
              Int32[] docIds = docSel[docTypeName];
              if (docIds.Length > 1 && (!UI.DocTypes[docTypeName].CanMultiEdit))
                UI.ShowDocSel(docSel); // ��������� �������������� ���������
              else
                UI.DocTypes[docTypeName].PerformEditing(docIds, State, false);
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

      ErrorMessageItem[] items = new ErrorMessageItem[rowIndices.Length];
      for (int i = 0; i < rowIndices.Length; i++)
        items[i] = ErrorMessages[rowIndices[i]];

      DBxDocSelection docSel = null;
      try
      {
        EFPApp.BeginWait("�������� ������� ����������", "�������");
        try
        {
          EFPDBxErrorGridViewDocSelEventArgs args = new EFPDBxErrorGridViewDocSelEventArgs(items, UI, reason);
          OnGetDocSel(args);
          if (!args.DocSel.IsEmpty)
            docSel = args.DocSel;
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
      return docSel;
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
    public new EFPDBxErrorGridView ControlProvider { get { return (EFPDBxErrorGridView)(base.ControlProvider); } }

    /// <summary>
    /// ��������� ������� "���������" - "������� ����������"
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();


      if (ControlProvider.HasGetDocSelHandler /*&& AccDepMainMenu.MenuSendTo != null*/) // ���� ����������
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
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxGridViewDocSelReason.Copy);
      if (docSel != null)
        args.DataObject.SetData(docSel);

      base.OnAddCopyFormats(args);
    }

    #endregion

    #region ���������

    private void ciSendToDocSel_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxGridViewDocSelReason.SendTo);
      if (docSel == null || docSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("������� �� �������� ����������");
        return;
      }
      ControlProvider.UI.ShowDocSel(docSel);
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
      EFPDBxErrorGridView controlProvider = new EFPDBxErrorGridView(BaseProvider, control, UI);
      if (GetDocSel != null)
        controlProvider.GetDocSel += new EFPDBxErrorGridViewDocSelEventHandler(ControlProvider_GetDocSel);
      return controlProvider;
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
