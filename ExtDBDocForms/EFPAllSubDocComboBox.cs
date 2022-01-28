// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using System.Data;
using FreeLibSet.Core;

// TODO: ����������. � ���������, �� ����������� �������� � ������� ������

namespace FreeLibSet.Forms.Docs
{
  #region ��������

  /// <summary>
  /// ��������� ������� EFPAllSubDocComboBox.TextValueNeeded
  /// </summary>
  public class EFPAllSubDocComboBoxTextValueNeededEventArgs : EFPComboBoxTextValueNeededEventArgs
  {
    #region �����������

    /// <summary>
    /// ��������� �����������
    /// </summary>
    /// <param name="owner">������-��������</param>
    public EFPAllSubDocComboBoxTextValueNeededEventArgs(EFPAllSubDocComboBox owner)
    {
      _Owner = owner;
    }

    #endregion

    #region ��������

    private EFPAllSubDocComboBox _Owner;

    // ���� ��� �������������� �������

    #endregion
  }

  /// <summary>
  /// ������� ������� EFPAllSubDocComboBox.TextValueNeeded
  /// </summary>
  /// <param name="sender">���������</param>
  /// <param name="args">��������� �������</param>
  public delegate void EFPAllSubDocComboBoxTextValueNeededEventHandler(object sender,
    EFPAllSubDocComboBoxTextValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// ��������� ����������, ������� ������������ ��� ������ ������� EFPSubDocGridView � ��������� ���������.
  /// ������������, � ��������, ��� �������������, ��������������� ��� �������� ��������� "������-��-������".
  /// ��������� �� ������������ ��� ������ ����-����, � ������ ��� ������ ��������� �������������� � ��������� �������� �������������.
  /// </summary>
  public class EFPAllSubDocComboBox : EFPUserSelComboBox
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

      _MaxTextItemCount = 1;
      _TextValueNeededArgs = new EFPAllSubDocComboBoxTextValueNeededEventArgs(this);
      _EmptyText = EFPAnyDocComboBoxBase.DefaultEmptyText;
      _EmptyImageKey = String.Empty;
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
    /// ���������� ��� ������� � ����������.
    /// </summary>
    public DBUI UI { get { return _SubDocTypeUI.UI; } }

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

      EFPDialogPosition dlgPos = new EFPDialogPosition(Control);
      EFPApp.ShowDialog(form, true, dlgPos);

      InitTextAndImage();
    }

    #endregion

    #region ������� TextValueNeeded

    /// <summary>
    /// ��� ������� ���������� ����� �������������� ������ ������������� � ��������� �������������� ����� � ����������, ����� ������������� ���������
    /// � �����������. ������� ���������� � ��� ����� � ��� ������ ������ �������������.
    /// ����� ���������� ��� ��������� � �������� TextValue
    /// </summary>
    public event EFPAllSubDocComboBoxTextValueNeededEventHandler TextValueNeeded
    {
      add
      {
        _TextValueNeeded += value;
        InitTextAndImage();
      }
      remove
      {
        _TextValueNeeded -= value;
        InitTextAndImage();
      }
    }
    private EFPAllSubDocComboBoxTextValueNeededEventHandler _TextValueNeeded;

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
    /// ����� �� ��������� ������ ������ ���, ������� ��� � ������������.
    /// ����� ���������� ��� �������� ����������� ����� ������� InitText() �
    /// ��� ������� � ����������
    /// </summary>
    private EFPAllSubDocComboBoxTextValueNeededEventArgs _TextValueNeededArgs;

    /// <summary>
    /// ��������� ������ ��������
    /// EFPDocComboBox ������������ ����� ��� ��������� ����������� ������ Edit
    /// </summary>
    private void InitTextAndImage()
    {
      try
      {
        _TextValueNeededArgs.Clear();
        // ����������� �������� ������, ��������� � �����������
        List<DBxSubDoc> viewedSubDocs = new List<DBxSubDoc>();
        foreach (DBxSubDoc sd in SubDocs)
        {
          if (sd.SubDocState != DBxDocState.Delete)
            viewedSubDocs.Add(sd);
        }
        if (viewedSubDocs.Count == 0)
        {
          _TextValueNeededArgs.TextValue = EmptyText;
          _TextValueNeededArgs.ImageKey = EmptyImageKey;
        }
        else
        {
          if (viewedSubDocs.Count > MaxTextItemCount)
            _TextValueNeededArgs.TextValue = SubDocTypeUI.SubDocType.PluralTitle + " (" + viewedSubDocs.Count.ToString() + ")";
          else
          {
            string[] a = new string[viewedSubDocs.Count];
            for (int i = 0; i < viewedSubDocs.Count; i++)
              a[i] = SubDocTypeUI.UI.TextHandlers.GetTextValue(viewedSubDocs[i]);
            _TextValueNeededArgs.TextValue = String.Join(", ", a);
          }

          if (EFPApp.ShowListImages)
          {
            if (viewedSubDocs.Count == 1)
              _TextValueNeededArgs.ImageKey = UI.ImageHandlers.GetImageKey(viewedSubDocs[0]);
            else
              _TextValueNeededArgs.ImageKey = SubDocTypeUI.TableImageKey;
          }
          else
            _TextValueNeededArgs.ImageKey = String.Empty;

          if (EFPApp.ShowToolTips)
          {
            if (viewedSubDocs.Count == 1)
              _TextValueNeededArgs.ToolTipText = UI.ImageHandlers.GetToolTipText(viewedSubDocs[0]);
            else
              _TextValueNeededArgs.ToolTipText = SubDocTypeUI.SubDocType.PluralTitle + " (" + viewedSubDocs.Count.ToString() + ")";
          }
          else
            _TextValueNeededArgs.ToolTipText = String.Empty;
        }

        // ���������������� ����������
        if (_TextValueNeeded != null)
          _TextValueNeeded(this, _TextValueNeededArgs);

        // ������������� ��������. ����������� ������������ ��������
        Control.Text = _TextValueNeededArgs.TextValue;
        if (EFPApp.ShowListImages)
        {
          if (String.IsNullOrEmpty(_TextValueNeededArgs.ImageKey))
            Control.Image = null;
          else
            Control.Image = EFPApp.MainImages.Images[_TextValueNeededArgs.ImageKey];
        }
        if (EFPApp.ShowToolTips)
          ValueToolTipText = _TextValueNeededArgs.ToolTipText;
      }
      catch (Exception e)
      {
        Control.Text = "!!! ������ !!! " + e.Message;
        if (EFPApp.ShowListImages)
          Control.Image = EFPApp.MainImages.Images["Error"];
        EFPApp.ShowTempMessage("������ ��� ��������� ������: " + e.Message);
      }
    }

    #endregion

    #region �������� MaxTextItemCount

    /// <summary>
    /// ������������ ���������� ���������������, ������� ����� ���� �������, ��� �������
    /// ������������ �������� ���� ��������� ����� �������.
    /// ����� ������� ������ ���������, �� ���������� ��������� � �������.
    /// �� ��������� ����� 1.
    /// </summary>
    public int MaxTextItemCount
    {
      get { return _MaxTextItemCount; }
      set
      {
        if (value == _MaxTextItemCount)
          return;
        _MaxTextItemCount = value;
        InitTextAndImage();
      }
    }
    private int _MaxTextItemCount;

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
        _EmptyTextEx = new DepInput<string>(EmptyText, EmptyTextEx_ValueChanged);
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
        _EmptyImageKeyEx = new DepInput<string>(EmptyImageKey, EmptyImageKeyEx_ValueChanged);
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
      }
    }

    private DepInput<String> _EmptyImageKeyEx;

    void EmptyImageKeyEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyImageKey = _EmptyImageKeyEx.Value;
    }

    #endregion
  }
}