// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// ������ ��� ������ ������� ������.
  /// ������������� ��������� �������� ��������� FiasLevelSet.
  /// </summary>
  public sealed class FiasLevelSetDialog
  {
    #region �����������

    /// <summary>
    /// ������� ������ � ����������� �� ���������
    /// </summary>
    public FiasLevelSetDialog(FiasUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _Title = "������ �������� ��������";
      _AvailableLevels = FiasLevelSet.AllLevels;
      _Value = FiasLevelSet.Empty;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������������� ���������.
    /// �������� � ������������.
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// ��������� �����
    /// </summary>
    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// �������, �� ������� ����� ��������
    /// �� ��������� - FiasLevelSet.AllLevels
    /// </summary>
    public FiasLevelSet AvailableLevels
    {
      get { return _AvailableLevels; }
      set { _AvailableLevels = value; }
    }
    private FiasLevelSet _AvailableLevels;

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// �� ��������� - false - ������ ���� ������ ���� �� ���� �������.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set { _CanBeEmpty = value; }
    }
    private bool _CanBeEmpty;


    /// <summary>
    /// �������� �������� - ������������� ����� �������.
    /// �� ��������� - FiasLevelSet.Empty - �� ������� �� ������ ������
    /// </summary>
    public FiasLevelSet Value
    {
      get { return _Value; }
      set
      {
        _Value = value;
      }
    }
    private FiasLevelSet _Value;

    /// <summary>
    /// ������� ����� ������� �� ������.
    /// �� ��������� ���� ������� ������������ ������������ EFPApp.DefaultScreen.
    /// </summary>
    public EFPDialogPosition DialogPosition 
    { 
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region ����� �������

    /// <summary>
    /// ������� ������ �� �����
    /// </summary>
    /// <returns>��������� ������ �������</returns>
    public DialogResult ShowDialog()
    {
      FiasLevel[] AvailableLevels2 = AvailableLevels.ToArray();
      string[] items = new string[AvailableLevels2.Length];
      for (int i = 0; i < AvailableLevels2.Length; i++)
        items[i] = FiasEnumNames.ToString(AvailableLevels2[i], true);


      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = Title;
      dlg.ListTitle = "������";
      dlg.Items = items;
      dlg.MultiSelect = true;
      dlg.CanBeEmpty = this.CanBeEmpty;
      for (int i = 0; i < AvailableLevels2.Length; i++)
        dlg.Selections[i] = Value[AvailableLevels2[i]];
      this.DialogPosition.CopyTo(dlg.DialogPosition);

      DialogResult res = dlg.ShowDialog();

      if (res == DialogResult.OK)
      {
        _Value = FiasLevelSet.Empty;
        for (int i = 0; i < AvailableLevels2.Length; i++)
        {
          if (dlg.Selections[i])
            _Value |= AvailableLevels2[i];
        }
      }

      return res;
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��� ������ ������� ������.
  /// ������������� ��������� �������� ��������� FiasLevelSet.
  /// </summary>
  public class EFPFiasLevelSetComboBox : EFPUserSelComboBox
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ����������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="ui">������ �� ������ FiasUI</param>
    public EFPFiasLevelSetComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, FiasUI ui)
      : base(baseProvider, control)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentNullException("ui");
#endif
      _UI = ui;

      _AvailableLevels = FiasLevelSet.AllLevels;
      _Value = FiasLevelSet.Empty;

      control.Text = _Value.ToString();
      control.PopupClick += new EventHandler(Control_PopupClick);
      control.ClearClick += new EventHandler(Control_ClearClick);

      _CanBeEmptyMode = UIValidateState.Error;
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// ���������������� ���������.
    /// �������� � ������������.
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// �������, �� ������� ����� ��������
    /// �� ��������� - FiasLevelSet.AllLevels
    /// </summary>
    public FiasLevelSet AvailableLevels
    {
      get { return _AvailableLevels; }
      set
      {
        if (value == _AvailableLevels)
          return;
        _AvailableLevels = value;
        Validate();
      }
    }
    private FiasLevelSet _AvailableLevels;

    #endregion

    #region �������� CanBeEmpty

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// �� ��������� - Error- ������ ���� ������ ���� �� ���� �������.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Control.ClearButton = _CanBeEmptyMode !=UIValidateState.Error;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// �� ��������� - false - ������ ���� ������ ���� �� ���� �������.
    /// ��������� CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region ������� ��������

    /// <summary>
    /// �������� �������� - ������������� ����� �������.
    /// �� ��������� - FiasLevelSet.Empty - �� ������� �� ������ ������
    /// </summary>
    public FiasLevelSet Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        Control.Text = _Value.ToString();
        Validate();
      }
    }
    private FiasLevelSet _Value;

    #endregion

    #region ��������

    /// <summary>
    /// �������� ��������
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return; // ������������

      FiasLevelSet extra = _Value - _AvailableLevels;
      if (!extra.IsEmpty)
      {
        SetError("������� ������������ ������: " + extra.ToString());
        return;
      }

      if (_Value.IsEmpty)
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("������ �� �������");
            break;
          case UIValidateState.Warning:
            SetWarning("������ �� �������");
            break;
        }
      }
    }

    #endregion

    #region ����� �� ������

    void Control_PopupClick(object sender, EventArgs args)
    {
      FiasLevelSetDialog dlg = new FiasLevelSetDialog(UI);
      dlg.Title = DisplayName;
      dlg.AvailableLevels = AvailableLevels;
      dlg.Value = Value;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.DialogPosition.PopupOwnerControl = Control;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      Value = dlg.Value;
    }

    void Control_ClearClick(object sender, EventArgs args)
    {
      Value = FiasLevelSet.Empty;
    }

    #endregion
  }
}
