using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{
                             
  /// <summary>
  /// ������� ���������� ���� ��� TreeView � TreeViewAdv.
  /// ������� ����� ��� EFPTreeViewCommandItems � EFPTreeViewAdvCommandItemsBase
  /// </summary>
  public class EFPTreeViewCommandItemsBase : EFPControlCommandItems, IEFPClipboardCommandItems
  {
    #region �����������

    /// <summary>
    /// �������������
    /// </summary>
    /// <param name="owner">��������� ������������ ��������</param>
    protected EFPTreeViewCommandItemsBase(IEFPTreeView owner)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");
      _Owner = owner;
    }

    /// <summary>
    /// ���������� ������ � ������
    /// </summary>
    protected void AddCommands()
    {
      #region ����� ������

      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.Click += new EventHandler(DoCut);
      ciCut.GroupBegin = true;
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.MenuText = "���������� ������";
      ciCopy.Enabled = true;
      ciCopy.Click += new EventHandler(DoCopy);
      Add(ciCopy);

      /*
      if (EFPApp.ShowToolTips)
      {
        ciCopyToolTip = new EFPCommandItem("������", "�������������������");
        ciCopyToolTip.MenuText = "���������� ����������� ���������";
        ciCopyToolTip.Click += new EventHandler(DoCopyToolTip);
        Add(ciCopyToolTip);
      } */
      AddSeparator();

      _PasteHandler = new EFPPasteHandler(this);

      #endregion

      #region �����

      ciFind = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Find);
      ciFind.Click += new EventHandler(Find);
      ciFind.GroupBegin = true;
      Add(ciFind);

      ciIncSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.IncSearch);
      ciIncSearch.Click += new EventHandler(IncSearch);
      ciIncSearch.StatusBarText = "??????????????????????";
      Add(ciIncSearch);

      ciFindNext = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FindNext);
      ciFindNext.Click += new EventHandler(FindNext);
      ciFindNext.GroupEnd = true;
      Add(ciFindNext);


      #endregion

      #region ��������� �������

      ciCheckAll = new EFPCommandItem("Edit", "SetAllCheckMarks");
      //ciCheckAll.Parent = MenuCheck;
      ciCheckAll.MenuText = "���������� ������� ��� ���� �����";
      ciCheckAll.ImageKey = "CheckListAll";
      ciCheckAll.ShortCut = Keys.Control | Keys.A;
      ciCheckAll.Click += new EventHandler(ciCheckAll_Click);
      Add(ciCheckAll);

      ciUncheckAll = new EFPCommandItem("Edit", "DeleteAllCheckmarks");
      //ciUncheckAll.Parent = MenuCheck;
      ciUncheckAll.GroupEnd = true;
      ciUncheckAll.MenuText = "����� ������� ��� ���� �����";
      ciUncheckAll.ImageKey = "CheckListNone";
      ciUncheckAll.ShortCut = Keys.Control | Keys.Shift | Keys.A;
      ciUncheckAll.Click += new EventHandler(ciUncheckAll_Click);
      Add(ciUncheckAll);

      #endregion
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ������������ ��������
    /// </summary>
    public IEFPTreeView Owner { get { return _Owner; } }
    private IEFPTreeView _Owner;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������������� ������� EFPCommandItem.Usage
    /// </summary>
    protected override void BeforeControlAssigned()
    {
      base.BeforeControlAssigned();

      EFPCommandItemUsage ClipboardUsage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        ClipboardUsage |= EFPCommandItemUsage.ToolBar;

      if (Cut == null)
      {
        ciCut.Enabled = false;
        ciCut.Usage = EFPCommandItemUsage.None;
      }
      else
        ciCut.Usage = ClipboardUsage;

      ciCopy.Usage = ClipboardUsage;


      // ��������� ������� ������� ������ ����� ���������������� ��������
      // (���� ��� �� ���� ��������� ����)
      //AddTextPasteFormats();

      _PasteHandler.InitCommandUsage(ClipboardInToolBar);
      _PasteHandler.PasteApplied += new EventHandler(FPasteHandler_PasteApplied);


      ciIncSearch.Usage = EFPCommandItemUsage.None; // TODO: ������� ������ �� ������ ������
      if (Owner.TextSearchContext == null)
      {
        ciFind.Usage = EFPCommandItemUsage.None;
        ciFindNext.Usage = EFPCommandItemUsage.None;
      }
    }

    /// <summary>
    /// ������������� ��������� ������
    /// </summary>
    protected override void AfterControlAssigned()
    {
      base.AfterControlAssigned();

      ciCheckAll.Visible = _Owner.CheckBoxes;
      ciUncheckAll.Visible = _Owner.CheckBoxes;


      RefreshSearchItems();
    }

    #endregion

    #region ����� ������

    /// <summary>
    /// ����� �� ���������� ������ "��������", "����������" � "��������" � ������
    /// ������������ (���� ��� ����).
    /// �� ��������� - false (������ � ���� � ������� �������)
    /// </summary>
    public bool ClipboardInToolBar 
    { 
      get { return _ClipboardInToolBar; } 
      set 
      {
        CheckNotAssigned();
        _ClipboardInToolBar = value; 
      } 
    }
    private bool _ClipboardInToolBar;

    #region ��������

    private EFPCommandItem ciCut;

    /// <summary>
    /// ���� ���������� ����������, �� � ��������� ���� ����������� ������� "��������"
    /// ���� ���������� �� ����������, �� �������������� ������� ������ �����.
    /// ��� ������������� ���������� Cut ����� �������� ����� PerformCutText() ���
    /// TryPerformCutText()
    /// </summary>
    public event EventHandler Cut;

    private void DoCut(object sender, EventArgs args)
    {
      if (Cut != null)
      {
        Cut(this, EventArgs.Empty);

        PerformRefreshItems();
      }
    }

    #endregion

    #region ����������

    private EFPCommandItem ciCopy;

    private void DoCopy(object sender, EventArgs args)
    {
      //Owner.CurrentIncSearchColumn = null;
      PerformCopy();
    }

    /// <summary>
    /// ���������� ����� �������� ��� ����������� � ����� ������ �������������� �������
    /// </summary>
    public event DataObjectEventHandler AddCopyFormats;

    /// <summary>
    /// �������� ������� AddCopyFormats.
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected virtual void OnAddCopyFormats(DataObjectEventArgs args)
    {
      if (AddCopyFormats != null)
        AddCopyFormats(this, args);
    }

    /// <summary>
    /// ��������� ����������� ���������� ����� ���������� ��������� � ����� ������.
    /// � ������ ������ ������ ��������� �� ����� � ���������� false
    /// ����� ����� �������������� ������ ���������� ����������� Cut
    /// </summary>
    /// <returns>true, ���� ����������� ������� ���������. false - � ������ ������</returns>
    public bool PerformCopy()
    {
      try
      {
        EFPApp.BeginWait("����������� ����� � ����� ������", "Copy");
        try
        {
          DataObject dobj2 = new DataObject();
          DataObjectEventArgs Args = new DataObjectEventArgs(dobj2);
          OnAddCopyFormats(Args);

          EFPApp.Clipboard.SetDataObject(dobj2, true);
        }
        finally
        {
          EFPApp.EndWait();
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.MessageBox(e.Message, "������ ��� ����������� � ����� ������",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� ��� ������ "�������" � "����������� �������"
    /// </summary>
    public EFPPasteHandler PasteHandler { get { return _PasteHandler; } }
    private EFPPasteHandler _PasteHandler;

    void FPasteHandler_PasteApplied(object sender, EventArgs args)
    {
      //Owner.CurrentIncSearchColumn = null;
      PerformRefreshItems();
    }

    #endregion

    #endregion

    #region ������� ������

    EFPCommandItem ciIncSearch, ciFind, ciFindNext;

    private void IncSearch(object sender, EventArgs args)
    {
      /*
      // ������ / ��������� ����� �� ������ ������
      if (Owner.CurrentIncSearchColumn == null)
      {
        if (Owner.CurrentColumn == null)
        {
          EFPApp.ShowTempMessage("������� �� ������");
          return;
        }
        if (!Owner.CurrentColumn.CanIncSearch)
        {
          EFPApp.ShowTempMessage("������� ������� �� ������������ ����� �� ������ ������");
          return;
        }
        Owner.CurrentIncSearchColumn = Owner.CurrentColumn;
      }
      else
      {
        Owner.CurrentIncSearchColumn = null;
      }
       * */
    }

    private void Find(object sender, EventArgs args)
    {
      Owner.TextSearchContext.StartSearch();
      RefreshSearchItems();
    }

    private void FindNext(object sender, EventArgs args)
    {

      //if (Owner.CurrentIncSearchColumn == null)
      //{
        if (Owner.TextSearchContext != null)
          Owner.TextSearchContext.ContinueSearch();
      //}
      //else
      //  if (!Owner.CurrentIncSearchColumn.PerformIncSearch(Owner.CurrentIncSearchChars.ToUpper(), true))
      //    EFPApp.ShowTempMessage("��� ������ �����, � ������� �������� ���� ���������� � \"" +
      //      Owner.CurrentIncSearchChars + "\"");
    }


    internal void RefreshSearchItems()
    {
      if (Owner.TextSearchContext!=null)
        ciFindNext.Enabled = Owner.TextSearchContext.ContinueEnabled;

      /*
      if (ciIncSearch == null)
        return;

      bool Enabled;
      string MenuText;
      string StatusBarText;
      bool Checked = false;

      if (Owner.CurrentIncSearchColumn != null)
      {
        // ����� �� ������ �����������
        Enabled = true;
        MenuText = "��������� ����� �� ������";
        string s = Owner.CurrentIncSearchChars;
        s = s.Replace(' ', (char)(0x00B7));
        s = s.PadRight(20);
        StatusBarText = s.ToUpper();
        Checked = true;
      }
      else
      {
        // ����� �� ������ �� �����������
        if (Owner.CanIncSearch)
        {
          MenuText = "������ ����� �� ������";
          if (Owner.CurrentColumn == null)
          {
            Enabled = false;
            StatusBarText = "<������� �� ������>";
          }
          else
          {
            Enabled = Owner.CurrentColumn.CanIncSearch;
            if (Enabled)
              StatusBarText = "<����� �� �����>";
            else
              StatusBarText = "<������������ �������>";
          }
        }
        else
        {
          MenuText = "����� �� ������";
          Enabled = false;
          StatusBarText = "<����� ����������>";
        }
      }

      ciIncSearch.MenuText = MenuText;
      ciIncSearch.Enabled = Enabled;
      ciIncSearch.StatusBarText = StatusBarText;
      ciIncSearch.Checked = Checked;
      ciIncSearch.ToolTipText = MenuText;

      if (Owner.CurrentIncSearchColumn == null)
        ciFindNext.Enabled = Owner.TextSearchContext.ContinueEnabled;
      else
        ciFindNext.Enabled = true;
      * */
    }

    #endregion

    #region ������� ��������� �������

    private EFPCommandItem ciCheckAll, ciUncheckAll;

    private void ciCheckAll_Click(object sender, EventArgs args)
    {
      _Owner.CheckAll(true);
    }

    private void ciUncheckAll_Click(object sender, EventArgs args)
    {
      _Owner.CheckAll(false);
    }

    #endregion

    #region ���������� ��������� ������

    /// <summary>
    /// ���������� ��� ��������� ������� ������� � ����������� �������� ���
    /// ��� ������ PerformRefreshItems()
    /// </summary>
    public event EventHandler RefreshItems;

    /// <summary>
    /// ���������� ����������� ������ ���������� ���� ����� �������� ���������
    /// ��������� ����� ���������
    /// </summary>
    public void PerformRefreshItems()
    {
      if (Owner == null)
        return;

      // �������� ����������� �����
      DoRefreshItems();
      // �������� ���������
      if (RefreshItems != null)
        RefreshItems(this, EventArgs.Empty);
    }

    /// <summary>
    /// ���������� ��������� � ����������� ������
    /// </summary>
    protected virtual void DoRefreshItems()
    {
      RefreshSearchItems();
    }

    #endregion
  }

  /// <summary>
  /// ������� ��� ��������� EFPTreeView
  /// </summary>
  public class EFPTreeViewCommandItems : EFPTreeViewCommandItemsBase
  {
    #region �����������

    /// <summary>
    /// �������������
    /// </summary>
    /// <param name="owner">��������� ������������ ��������</param>
    public EFPTreeViewCommandItems(EFPTreeView owner)
      : base(owner)
    {
      AddCommands();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ������������ ��������
    /// </summary>
    public new EFPTreeView Owner { get { return (EFPTreeView)(base.Owner); } }

    #endregion

    #region ����� ������

    /// <summary>
    /// ��������� � ����� ������ ������� ������������� ��� �������� ����.
    /// ����� ���������� ������� AddCopyFormats.
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected override void OnAddCopyFormats(DataObjectEventArgs args)
    {
      if (Owner.Control.SelectedNode != null)
        args.DataObject.SetData(Owner.Control.SelectedNode.Text);

      base.OnAddCopyFormats(args);
    }

    #endregion
  }
}