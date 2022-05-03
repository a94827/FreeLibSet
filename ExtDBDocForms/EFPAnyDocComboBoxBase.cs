// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using System.Windows.Forms;
using FreeLibSet.Data;
using FreeLibSet.Controls;
using FreeLibSet.UICore;

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
      _EmptyImageKey = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
      control.ClearButton = false; // 08.11.2021
      control.PopupClick += new EventHandler(Control_PopupClick);
      control.ClearClick += new EventHandler(Control_ClearClick);

      ClearButtonEnabled = false;

      _CanBeDeletedMode = UIValidateState.Error;

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
        _EmptyTextEx = new DepInput<string>(EmptyText,EmptyTextEx_ValueChanged);
        _EmptyTextEx.OwnerInfo = new DepOwnerInfo(this, "EmptyTextEx");
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
        _EmptyImageKeyEx = new DepInput<string>(EmptyImageKey,EmptyImageKeyEx_ValueChanged);
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
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
    /// ����� �������� ������� ��������.
    /// �� ��������� - Error.
    /// ��� �������� ���������������� ��� ������������� ���������, ����������
    /// ������ ������� ������ �� ��������
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Control.ClearButton = (value != UIValidateState.Error);
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// True, ���� �� ������� ��������� ������ �������� (������� ������������� ��� ��������� ������ ������������� ��������, ��� ������ ������� ����� ��� ��������� ������ ���������� ��������).
    /// ��������� CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region �������� CanBeDeleted

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// ���� ����������� � Ok, �� ����������� �������� ��������� ��������� ���
    /// ������������. �� ��������� (Error), ���� ��������� ��������/�����������
    /// ������, �� �������� ������.
    /// ����� ����� ���������� ��������������.
    /// </summary>
    public UIValidateState CanBeDeletedMode
    {
      get { return _CanBeDeletedMode; }
      set
      {
        if (value == _CanBeDeletedMode)
          return;
        _CanBeDeletedMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBeDeletedMode;

    /// <summary>
    /// ������������ ��� �������� ������������ ���������� ��������.
    /// ���� ����������� � true, �� ����������� �������� ��������� ��������� ���
    /// ������������. �� ��������� (false), ���� ��������� ��������/�����������
    /// ������, �� �������� ������.
    /// �������� �������� CanBeDeletedMode.
    /// </summary>
    public bool CanBeDeleted
    {
      get { return CanBeDeletedMode != UIValidateState.Error; }
      set { CanBeDeletedMode = value ? UIValidateState.Ok : UIValidateState.Error; }
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

      EFPAnyDocComboBoxBaseCommandItems Items = new EFPAnyDocComboBoxBaseCommandItems(this);
      Items.InitEnabled();
      return Items;
    }


    void SelectableEx_ValueChanged(object sender, EventArgs args)
    {
      InitTextAndImage(); // ����� ���������� ������ ��������������

      if (CommandItemsAssigned)
      {
        if (CommandItems is EFPAnyDocComboBoxBaseCommandItems)
          ((EFPAnyDocComboBoxBaseCommandItems)CommandItems).InitEnabled();
      }
    }

    #endregion
  }


  /// <summary>
  /// ������� ���������� ����
  /// </summary>
  public class EFPAnyDocComboBoxBaseCommandItems : EFPControlCommandItems
  {
    #region �����������

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="controlProvider">��������� ����������</param>
    public EFPAnyDocComboBoxBaseCommandItems(EFPAnyDocComboBoxBase controlProvider)
      :base(controlProvider)
    {
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

    /// <summary>
    /// ��������� ������������ ��������
    /// </summary>
    protected new EFPAnyDocComboBoxBase ControlProvider { get { return (EFPAnyDocComboBoxBase)(base.ControlProvider); } }

    #endregion

    #region ������������� ����������� ������

    /// <summary>
    /// ��������� ������������ ������, � ����������� �� ������� ��������� ������
    /// </summary>
    public void InitEnabled()
    {
      if (ciCut != null)
        ciCut.Enabled = ControlProvider.IsNotEmpty && ControlProvider.Selectable;
      if (ciCopy != null)
        ciCopy.Enabled = ControlProvider.IsNotEmpty;
      if (ciPaste != null)
        ciPaste.Enabled = ControlProvider.Selectable;

      if (ciShowDocInfo != null)
      {
        // 11.04.2016
        DBxDocSelection docSel;
        try
        {
          docSel = ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
          UserPermissions ups = ControlProvider.UI.DocProvider.UserPermissions;
          if (docSel.IsEmpty || ups == null)
            ciShowDocInfo.Enabled = false;
          else
          {
            string DocTypeName = docSel.TableNames[0];
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
      ControlProvider.Clear();
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (!ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage("�������� �� �������");
        return;
      }
      DBxDocSelection docSel = ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
      DataObject dObj = new DataObject();
      dObj.SetData(docSel);
      ControlProvider.UI.OnAddCopyFormats(dObj, docSel); // 06.02.2021
      dObj.SetText(ControlProvider.Control.Text);
      EFPApp.Clipboard.SetDataObject(dObj, true);
    }

    void ciPaste_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.UI.PasteDocSel();
      if (docSel == null)
      {
        EFPApp.ShowTempMessage("����� ������ �� �������� ������ �� ���������");
        return;
      }
      // ���� �� �����������
      // ��� ����� ������ ����������� ����� SetDocSel()
      ControlProvider.PerformSetDocSel(docSel);
    }

    #endregion

    #region ���������� � ���������

    EFPCommandItem ciShowDocInfo;

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (!ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage("�������� �� �������");
        return;
      }
      DBxDocSelection docSel = ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
      if (docSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("��� ���������� ���������");
        return;
      }

      // ��� ��� ��������� - ������ � ������
      string docTypeName = docSel.TableNames[0];
      Int32 docId = docSel[docTypeName][0];

      DocTypeUI dtui = ControlProvider.UI.DocTypes[docTypeName];
      dtui.ShowDocInfo(docId);
    }

    #endregion
  }
}
