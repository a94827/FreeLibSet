using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.FIAS;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


namespace AgeyevAV.ExtForms.FIAS
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

    ///// <summary>
    ///// �������� ��������������, ���� �� ������� �� ������ ������
    ///// �� ��������� - false - �� ��������.
    ///// ��������� ������ ��� ��������� �������� CanBeEmpty=true, ����� ����� ���������� ������, � �� ��������������.
    ///// </summary>
    //public bool WarningIfEmpty
    //{
    //  get { return _WarningIfEmpty; }
    //  set { _WarningIfEmpty = value; }
    //}
    //private bool _WarningIfEmpty;

    ///// <summary>
    ///// ���� �������� ���������� � true, �� �������� ����� ������ � ������ ���������, � �� ��������������.
    ///// �� ��������� - false
    ///// </summary>
    //public bool ReadOnly
    //{
    //  get { return _ReadOnly; }
    //  set { _ReadOnly = value; }
    //}
    //private bool _ReadOnly;

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

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// �� ��������� - false - ������ ���� ������ ���� �� ���� �������.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return Control.ClearButton; }
      set
      {
        if (value == Control.ClearButton)
          return;
        Control.ClearButton = value;
        Validate();
      }
    }

    /// <summary>
    /// �������� ��������������, ���� �� ������� �� ������ ������
    /// �� ��������� - false - �� ��������.
    /// ��������� ������ ��� ��������� �������� CanBeEmpty=true, ����� ����� ���������� ������, � �� ��������������.
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    ///// <summary>
    ///// ���� �������� ���������� � true, �� �������� ����� ������ � ������ ���������, � �� ��������������.
    ///// �� ��������� - false
    ///// </summary>
    //public bool ReadOnly
    //{
    //  get { return _ReadOnly; }
    //  set { _ReadOnly = value; }
    //}
    //private bool _ReadOnly;

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
      if (ValidateState == EFPValidateState.Error)
        return; // ������������

      FiasLevelSet extra = _Value - _AvailableLevels;
      if (!extra.IsEmpty)
      {
        SetError("������� ������������ ������: " + extra.ToString());
        return;
      }

      if (_Value.IsEmpty)
      {
        if (!CanBeEmpty)
          SetError("������ �� �������");
        else if (WarningIfEmpty)
          SetWarning("������ �� �������");
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
