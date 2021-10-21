using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using FreeLibSet.Controls;
using FreeLibSet.Core;

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
    /// �� ��������� - false - ����� ������ ���� ��������.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        if (value == _CanBeEmpty)
          return;
        _CanBeEmpty = value;
        Validate();
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// �������� ��������������, ���� ����� �� ��������
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
        if (_CanBeEmpty)
          Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - false - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// </summary>
    public bool CanBePartial
    {
      get { return _CanBePartial; }
      set
      {
        if (value == _CanBePartial)
          return;
        _CanBePartial = value;
        Validate();
      }
    }
    private bool _CanBePartial;

    /// <summary>
    /// �������� ��������������, ���� ����� �������� �������� (��������, ������ ������ ������).
    /// �� ��������� - false - �� ��������.
    /// ��������� ������ ��� ��������� �������� CanBePartial=true, ����� ����� ���������� ������, � �� ��������������.
    /// </summary>
    public bool WarningIfPartial
    {
      get { return _WarningIfPartial; }
      set
      {
        if (value == _WarningIfPartial)
          return;
        _WarningIfPartial = value;
        if (CanBePartial)
          Validate();
      }
    }
    private bool _WarningIfPartial;

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

      EFPFiasAddressPanel.AddEditorMessages(Address, EditorLevel, errors, CanBeEmpty, WarningIfEmpty, CanBePartial, WarningIfPartial, MinRefBookLevel);

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
