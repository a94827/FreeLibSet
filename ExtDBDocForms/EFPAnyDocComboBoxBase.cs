using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using System.Windows.Forms;
using FreeLibSet.Data;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ������� ����� ��� EFPDocComboBoxBase � EFPMultiDocComboBoxBase.
  /// �� �������� ������ �� �������������� Id ��� Ids
  /// </summary>
  public abstract class EFPAnyDocComboBoxBase : EFPUserSelComboBox
  {
    #region ���������

    /// <summary>
    /// ����� ���������� �� ���������, ����� �� ������ ��������
    /// </summary>
    public const string DefaultEmptyText = "[ ��� ]";

    #endregion

    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="ui">������ � ���������� ������������</param>
    protected EFPAnyDocComboBoxBase(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _EmptyText = DefaultEmptyText;
      control.ClearButton = true;
      control.PopupClick += new EventHandler(Control_PopupClick);
      control.ClearClick += new EventHandler(Control_ClearClick);

      ClearButtonEnabled = false;

      _CanBeDeleted = false;

      SelectableEx.ValueChanged += SelectableEx_ValueChanged;
    }

    #endregion

    #region �������� UI

    /// <summary>
    /// ������ � ���������� ������������
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region InitTextAndImage

    /// <summary>
    /// �������� InitTextAndImage()
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();
      InitTextAndImage();
    }


    /// <summary>
    /// �������������� ��������� ������������� (�������� TextValue) � ������
    /// </summary>
    protected abstract void InitTextAndImage();

    /// <summary>
    /// �������������� ���������� ������ � ����������� � ���������� ������
    /// </summary>
    public void UpdateText()
    {
      InitTextAndImage();
    }

    #endregion

    #region ����� �������� Popup

    private void Control_PopupClick(object sender, EventArgs args)
    {
      try
      {
        PerformPopup();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ ��� ������ �� ���������� \"" + DisplayName + "\"");
        //EFPApp.MessageBox(e.Message, "������ ��� ������ �� ������",
        //  MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// ���� ����� ������ ������� ���� ������� � ���������� �������� RefId.ValueEx
    /// </summary>
    protected abstract void DoPopup();

    /// <summary>
    /// ��������� ��������, ����������� ������� ��������� ����������� ������
    /// </summary>
    public void PerformPopup()
    {
      if (Popup == null)
        DoPopup();
      else
        Popup(this, EventArgs.Empty);
    }

    /// <summary>
    /// ���� ���������� ������� ����������, �� �� ���������� ������ ������
    /// ������������ �������. ���������� ������ ������� ������������ �����������
    /// ������ ������ ��������� � ���������� ��������� �������� ����� ������
    /// </summary>
    public event EventHandler Popup;

    #endregion

    #region ������� �������� Clear

    private void Control_ClearClick(object sender, EventArgs args)
    {
      try
      {
        Clear(); // ����� ���� ����� ���������� ���������
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ ������� ��������");
      }
    }

    /// <summary>
    /// ������� ��������� ��������.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// �������� ���������� true, ���� � ���������� ������� ��������
    /// </summary>
    public abstract bool IsNotEmpty { get;}

    #endregion

    #region �������� EmptyText

    /// <summary>
    /// �����, ��������� � ����������, ����� ��� ���������� ��������.
    /// (�� ��������� "[ ��� ]")
    /// </summary>
    public string EmptyText
    {
      get { return _EmptyText; }
      set
      {
        if (value == null)
          value = String.Empty; // 13.06.2019. ����� ��� ���������� ������ ����������� DocValueAnyDocComboBoxBase

        if (value == _EmptyText)
          return;
        _EmptyText = value;
        if (_EmptyTextEx != null)
          _EmptyTextEx.Value = value;
        InitTextAndImage();
      }
    }
    private string _EmptyText;

    /// <summary>
    /// �����, ��������� � ����������, ����� ��� ���������� ��������.
    /// ����������� �������� ��� EmptyText.
    /// </summary>
    public DepValue<String> EmptyTextEx
    {
      get
      {
        InitEmptyTextEx();
        return _EmptyTextEx;
      }
      set
      {
        InitEmptyTextEx();
        _EmptyTextEx.Source = value;
      }
    }

    private void InitEmptyTextEx()
    {
      if (_EmptyTextEx == null)
      {
        _EmptyTextEx = new DepInput<string>();
        _EmptyTextEx.OwnerInfo = new DepOwnerInfo(this, "EmptyTextEx");
        _EmptyTextEx.Value = EmptyText;
        _EmptyTextEx.ValueChanged += new EventHandler(EmptyTextEx_ValueChanged);
      }
    }

    private DepInput<String> _EmptyTextEx;

    void EmptyTextEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyText = _EmptyTextEx.Value;
    }

    #endregion

    #region �������� EmptyImageKey

    /// <summary>
    /// ������, ��������� � ����������, ����� ��� ���������� ��������.
    /// ����������� ������ ���� � ��������� EFPApp.MainImages.
    /// �� ��������� "" - ��� ������.
    /// </summary>
    public string EmptyImageKey
    {
      get { return _EmptyImageKey; }
      set
      {
        if (value == _EmptyImageKey)
          return;
        _EmptyImageKey = value;
        if (_EmptyImageKeyEx != null)
          _EmptyImageKeyEx.Value = value;
        InitTextAndImage();
      }
    }
    private string _EmptyImageKey;

    /// <summary>
    /// ������, ��������� � ����������, ����� ��� ���������� ��������.
    /// ����������� �������� ��� EmptyImageKey.
    /// </summary>
    public DepValue<String> EmptyImageKeyEx
    {
      get
      {
        InitEmptyImageKeyEx();
        return _EmptyImageKeyEx;
      }
      set
      {
        InitEmptyImageKeyEx();
        _EmptyImageKeyEx.Source = value;
      }
    }

    private void InitEmptyImageKeyEx()
    {
      if (_EmptyImageKeyEx == null)
      {
        _EmptyImageKeyEx = new DepInput<string>();
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
        _EmptyImageKeyEx.Value = EmptyImageKey;
        _EmptyImageKeyEx.ValueChanged += new EventHandler(EmptyImageKeyEx_ValueChanged);
      }
    }

    private DepInput<String> _EmptyImageKeyEx;

    void EmptyImageKeyEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyImageKey = _EmptyImageKeyEx.Value;
    }

    #endregion

    #region �������� CanBeEmpty

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// True, ���� ����� �������� ������ ��������. �� ��������� - true.
    /// ���������� ��������� ������ "���" ��� ������ �� �����������, ������� ��������
    /// "X" ����� � �����������. ��� �������� false ����������� �������� ������.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return Control.ClearButton; }
      set
      {
        if (value == Control.ClearButton)
          return;
        Control.ClearButton = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// True, ���� ����� �������� ������ ��������. 
    /// ����������� �������� ��� CanBeEmpty.
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region �������� WarningIfEmpty

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// �������� ��������������, ���� �������� �� �������, ��� �������, ��� CanBeEmpty=true.
    /// ����� �������� WarningIfEmpty ������������.
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// �������� ��������������, ���� �������� �� �������, ��� �������, ��� CanBeEmpty=true.
    /// ����������� �������� ��� WarningIfEmpty
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion

    #region �������� CanBeDeleted � WarningIfDeleted

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// ���� ����������� � true, �� ����������� �������� ��������� ��������� ���
    /// ������������. �� ��������� (false), ���� ��������� ��������/�����������
    /// ������, �� �������� ������ ��� �������������� � ����������� �� ��������
    /// WarningIfDeleted
    /// </summary>
    public bool CanBeDeleted
    {
      get { return _CanBeDeleted; }
      set
      {
        if (value == _CanBeDeleted)
          return;
        _CanBeDeleted = value;
        if (_CanBeDeletedEx != null)
          _CanBeDeletedEx.Value = value;
        Validate();
      }
    }
    private bool _CanBeDeleted;

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// ���� ����������� � true, �� ����������� �������� ��������� ��������� ���
    /// ������������. 
    /// ����������� �������� ��� CanBeDeleted
    /// </summary>
    public DepValue<Boolean> CanBeDeletedEx
    {
      get
      {
        InitCanBeDeletedEx();
        return _CanBeDeletedEx;
      }
      set
      {
        InitCanBeDeletedEx();
        _CanBeDeletedEx.Source = value;
      }
    }

    private void InitCanBeDeletedEx()
    {
      if (_CanBeDeletedEx == null)
      {
        _CanBeDeletedEx = new DepInput<bool>();
        _CanBeDeletedEx.OwnerInfo = new DepOwnerInfo(this, "CanBeDeletedEx");
        _CanBeDeletedEx.Value = CanBeDeleted;
        _CanBeDeletedEx.ValueChanged += new EventHandler(CanDeletedEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeDeletedEx;

    void CanDeletedEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeDeleted = _CanBeDeletedEx.Value;
    }


    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// ���� �������� CanBeDeleted �� �����������, � ��������� ��������/����������� 
    /// ������, �� �������� ������ (��� �������� false) ��� �������������� (���
    /// �������� true). �������� �� ��������� - false (������).
    /// ��� CanBeDeleted=true, �������� WarningIfDeleted ������������.
    /// </summary>
    public bool WarningIfDeleted
    {
      get { return _WarningIfDeleted; }
      set
      {
        if (value == _WarningIfDeleted)
          return;
        _WarningIfDeleted = value;
        if (_WarningIfDeletedEx != null)
          _WarningIfDeletedEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfDeleted;


    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// ���� �������� CanBeDeleted �� �����������, � ��������� ��������/����������� 
    /// ������, �� �������� ������ (��� �������� false) ��� �������������� (���
    /// �������� true). 
    /// ����������� �������� ��� WarningIfDeleted.
    /// </summary>
    public DepValue<Boolean> WarningIfDeletedEx
    {
      get
      {
        InitWarningIfDeletedEx();
        return _WarningIfDeletedEx;
      }
      set
      {
        InitWarningIfDeletedEx();
        _WarningIfDeletedEx.Source = value;
      }
    }

    private void InitWarningIfDeletedEx()
    {
      if (_WarningIfDeletedEx == null)
      {
        _WarningIfDeletedEx = new DepInput<bool>();
        _WarningIfDeletedEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfDeletedEx");
        _WarningIfDeletedEx.Value = WarningIfDeleted;
        _WarningIfDeletedEx.ValueChanged += new EventHandler(WarningIfDeletedEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _WarningIfDeletedEx;

    void WarningIfDeletedEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfDeleted = _WarningIfDeletedEx.Value;
    }

    #endregion

    #region ������� ����������

    /// <summary>
    /// �������� ���������� true, ���� ������ ������������ ��������� ������� ����������.
    /// ��� - ����������� ��������. ������������ �������� �� ������� �� �������� ���������� ��������.
    /// </summary>
    public virtual bool GetDocSelSupported { get { return false; } }

    /// <summary>
    /// �������� ���������� true, ���� ������ ������������ ���������� ������� ����������
    /// ��� - ����������� ��������. ������������ �������� �� ������� �� �������� ���������� ��������.
    /// </summary>
    public virtual bool SetDocSelSupported { get { return false; } }

    /// <summary>
    /// ������������� ����� ��� ��������� ������� ����������.
    /// �������� ����������� ����� OnGetDocSel().
    /// ��� ����������� ������ ���������� ���������� �������, ���������� ��������� ��������
    /// (��� ���������) �, ��������, ��������� ���������.
    /// ��� ����������� ������ ������������� ���������� ��������-�������� �, ��������, ��������� ���������.
    /// ���� ��� ��������� ���������� � ����������, �� ���������� null.
    /// </summary>
    /// <param name="reason">������� ��������� �������</param>
    /// <returns>������� ����������</returns>
    public DBxDocSelection PerformGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      return OnGetDocSel(reason);
    }

    /// <summary>
    /// ��������� ������� ����������.
    /// ������������������ ����� ���������� null.
    /// </summary>
    /// <param name="reason">������� ��������� �������</param>
    /// <returns>������� ����������</returns>
    protected virtual DBxDocSelection OnGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      return null;
    }

    /// <summary>
    /// ������������� ����� ���������� ������� ����������.
    /// �������� ����������� ����� OnSetDocSel().
    /// </summary>
    /// <param name="docSel">������� ����������</param>
    public void PerformSetDocSel(DBxDocSelection docSel)
    {
      OnSetDocSel(docSel);
    }

    /// <summary>
    /// ���������� ������� ����������.
    /// ������������������ ����� �������� ����������
    /// </summary>
    /// <param name="docSel">������� ����������</param>
    protected virtual void OnSetDocSel(DBxDocSelection docSel)
    {
      throw new NotSupportedException("���������� ������� ���������� �� �����������");
    }

    /// <summary>
    /// ���������� true, ���� ��������� ���������� � ��������� ������ ���� ��������.
    /// ������������ EFPDocComboBox.
    /// ��� - ����������� ��������. ������������ �������� �� ������� �� �������� ���������� ��������.
    /// </summary>
    public virtual bool DocInfoSupported { get { return false; } }

    #endregion

    #region ��������� ����

    /// <summary>
    /// ���������� ����� ������ EFPAnyDocComboBoxBaseControlItems.
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      if (!GetDocSelSupported)
        return base.GetCommandItems();

      EFPAnyDocComboBoxBaseControlItems Items = new EFPAnyDocComboBoxBaseControlItems(this);
      Items.InitEnabled();
      return Items;
    }


    void SelectableEx_ValueChanged(object sender, EventArgs args)
    {
      InitTextAndImage(); // ����� ���������� ������ ��������������

      if (CommandItemsAssigned)
      {
        if (CommandItems is EFPAnyDocComboBoxBaseControlItems)
          ((EFPAnyDocComboBoxBaseControlItems)CommandItems).InitEnabled();
      }
    }

    #endregion
  }


  /// <summary>
  /// ������� ���������� ����
  /// </summary>
  public class EFPAnyDocComboBoxBaseControlItems : EFPControlCommandItems
  {
    #region �����������

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="controlProvider">��������� ����������</param>
    public EFPAnyDocComboBoxBaseControlItems(EFPAnyDocComboBoxBase controlProvider)
    {
      _ControlProvider = controlProvider;

      if (controlProvider.SetDocSelSupported)
      {
        ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
        ciCut.GroupBegin = true;
        ciCut.Click += new EventHandler(ciCut_Click);
        Add(ciCut);
      }

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Click += new EventHandler(ciCopy_Click);
      Add(ciCopy);

      if (controlProvider.SetDocSelSupported)
      {
        ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
        ciPaste.GroupEnd = true;
        ciPaste.Click += new EventHandler(ciPaste_Click);
        Add(ciPaste);
      }

      if (controlProvider.DocInfoSupported)
      {
        ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
        ciShowDocInfo.MenuText = "���������� � ���������";
        ciShowDocInfo.ShortCut = Keys.F12;
        ciShowDocInfo.ImageKey = "Information";
        ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
        ciShowDocInfo.GroupBegin = true;
        ciShowDocInfo.GroupEnd = true;
        Add(ciShowDocInfo);
      }
    }

    #endregion

    #region ��������

    private EFPAnyDocComboBoxBase _ControlProvider;

    #endregion

    #region ������������� ����������� ������

    /// <summary>
    /// ��������� ������������ ������, � ����������� �� ������� ��������� ������
    /// </summary>
    public void InitEnabled()
    {
      if (ciCut != null)
        ciCut.Enabled = _ControlProvider.IsNotEmpty && _ControlProvider.Selectable;
      if (ciCopy != null)
        ciCopy.Enabled = _ControlProvider.IsNotEmpty;
      if (ciPaste != null)
        ciPaste.Enabled = _ControlProvider.Selectable;

      if (ciShowDocInfo != null)
      {
        // 11.04.2016
        DBxDocSelection DocSel;
        try
        {
          DocSel = _ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
          UserPermissions ups = _ControlProvider.UI.DocProvider.UserPermissions;
          if (DocSel.IsEmpty || ups == null)
            ciShowDocInfo.Enabled = false;
          else
          {
            string DocTypeName = DocSel.TableNames[0];
            ciShowDocInfo.Enabled = DocTypeViewHistoryPermission.GetAllowed(ups, DocTypeName);
          }
        }
        catch
        {
          ciShowDocInfo.Enabled = false;
        }
      }
    }

    #endregion

    #region ������� ������ ������

    private EFPCommandItem ciCut, ciCopy, ciPaste;

    void ciCut_Click(object sender, EventArgs args)
    {
      ciCopy_Click(null, null);
      _ControlProvider.Clear();
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (!_ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage("�������� �� �������");
        return;
      }
      DBxDocSelection DocSel = _ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
      DataObject DObj = new DataObject();
      DObj.SetData(DocSel);
      _ControlProvider.UI.OnAddCopyFormats(DObj, DocSel); // 06.02.2021
      DObj.SetText(_ControlProvider.Control.Text);
      EFPApp.Clipboard.SetDataObject(DObj, true);
    }

    void ciPaste_Click(object sender, EventArgs args)
    {
      DBxDocSelection DocSel = _ControlProvider.UI.PasteDocSel();
      if (DocSel == null)
      {
        EFPApp.ShowTempMessage("����� ������ �� �������� ������ �� ���������");
        return;
      }
      // ���� �� �����������
      // ��� ����� ������ ����������� ����� SetDocSel()
      _ControlProvider.PerformSetDocSel(DocSel);
    }

    #endregion

    #region ���������� � ���������

    EFPCommandItem ciShowDocInfo;

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (!_ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage("�������� �� �������");
        return;
      }
      DBxDocSelection DocSel = _ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
      if (DocSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("��� ���������� ���������");
        return;
      }

      // ��� ��� ��������� - ������ � ������
      string DocTypeName = DocSel.TableNames[0];
      Int32 DocId = DocSel[DocTypeName][0];

      DocTypeUI dtui = _ControlProvider.UI.DocTypes[DocTypeName];
      dtui.ShowDocInfo(DocId);
    }

    #endregion
  }
}
