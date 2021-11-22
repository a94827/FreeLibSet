// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// ��������� ���������� ��� ������ ������.
  /// � ��������� ���� ������������ ��������� �����, � ��� �������� ����������� ������ ������������ ������ �������������� ������ FiasAddressDialog
  /// </summary>
  public class EFPFiasAddressComboBox : EFPUserSelComboBox
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ����������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� ������� - ���������</param>
    /// <param name="ui">��������� ������� � �������</param>
    public EFPFiasAddressComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, FiasUI ui)
      : base(baseProvider, control)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _CanBeEmptyMode = UIValidateState.Error;
      _CanBePartialMode = UIValidateState.Error;

      _TextFormat = FiasFormatStringParser.DefaultFormat;
      _Address = new FiasAddress();
      Control.PopupClick += new EventHandler(Control_PopupClick);
      Control.ClearClick += new EventHandler(Control_ClearClick);
    }

    #endregion

    #region ����� ��������

    /// <summary>
    /// ���������� "�����".
    /// </summary>
    protected override string DefaultDisplayName { get { return "�����"; } }

    /// <summary>
    /// ��������� ������� � �������.
    /// �������� � ������������
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// �������, �� �������� ����� ������� �����.
    /// �� ��������� - FiasLevel.Room.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        _EditorLevel = value;
        this.Address = Address; // ������������� ������
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// ����� �� ������������� �������� ������?
    /// �� ��������� - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set { _PostalCodeEditable = value; }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// ������ ����������� ������� ������, ������� ������ ���� ������ �� �����������, � �� ����� �������.
    /// �� ��������� - FiasLevel.City, �� ���� ������, ����� � ����� ������ ���� � ����������� ����, � ���������� �����,
    /// ��� ������������� - ������ �������, ���� ��� ��� � �����������.
    /// �������� Unknown ��������� ��� ��������. 
    /// ����������� ����� ��������, ������� House � Room, ���� ��� �� ������� �� ������� FiasEditorLevel.
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        if (value == _MinRefBookLevel)
          return;
        _MinRefBookLevel = value;
        Validate();
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// ������ ������ ������ ������.
    /// �� ��������� - "TEXT"
    /// </summary>
    public string TextFormat
    {
      get { return _TextFormat; }
      set
      {
        string errorMessage;
        if (!FiasFormatStringParser.IsValidFormat(value, out errorMessage))
          throw new ArgumentException("������������ ������ ��� ���������� ������ ������. " + errorMessage);
        _TextFormat = value;

        InitTextAndImage();
      }
    }
    private string _TextFormat;

    /// <summary>
    /// ������������� �����
    /// </summary>
    public FiasAddress Address
    {
      get { return _Address; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Address = value;

        InitTextAndImage();

        Validate();
      }
    }
    private FiasAddress _Address;

    private void InitTextAndImage()
    {
      EFPComboBoxTextValueNeededEventArgs args = new EFPComboBoxTextValueNeededEventArgs();
      if (!_Address.IsEmpty)
        args.ImageKey = "FIAS.Address";
      FiasHandler handler = new FiasHandler(_UI.Source);
      args.TextValue = handler.Format(Address, TextFormat);
      base.InitTextAndImage(args);
    }

    #endregion

    #region �������� CanBeEmpty � CanBePartial

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// �� ��������� - Error - ����� ������ ���� ��������.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// ��������� CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - Error - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        if (value == _CanBePartialMode)
          return;
        _CanBePartialMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBePartialMode;

    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - false - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// ��������� �������� CanBePartialMode
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region ����������� ����������

    void Control_PopupClick(object sender, EventArgs args)
    {
      FiasAddressDialog dlg = new FiasAddressDialog(_UI);
      dlg.Title = DisplayName;
      dlg.DialogPosition.PopupOwnerControl = Control;
      dlg.EditorLevel = EditorLevel;
      dlg.PostalCodeEditable = PostalCodeEditable;
      dlg.Address = Address;
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;
      this.Address = dlg.Address;
    }

    void Control_ClearClick(object sender, EventArgs args)
    {
      Address = new FiasAddress();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������
    /// </summary>
    protected override void OnValidate()
    {
      ErrorMessageList errors = Address.Messages.Clone();

      EFPFiasAddressPanel.AddEditorMessages(Address, EditorLevel, errors, CanBeEmptyMode, CanBePartialMode, MinRefBookLevel);

      switch (errors.Severity)
      {
        case ErrorMessageKind.Error:
          SetError(errors.AllText);
          break;
        case ErrorMessageKind.Warning:
          SetWarning(errors.AllText);
          break;
      }
    }

    #endregion
  }
}
