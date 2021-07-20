using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.RI;
using AgeyevAV.Config;

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

// ��������� ���������������� ���������

namespace AgeyevAV.FIAS.RI
{
  /// <summary>
  /// ��������� ��������� ��� ������ ����� ����������� ������.
  /// ������ ������� ������������ ��������� FiasAddressComboBox
  /// </summary>
  [Serializable]
  public class FiasAddressPanel:Control
  {
    #region �����������

    /// <summary>
    /// �������������� ��������� �������� �������
    /// </summary>
    public FiasAddressPanel(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _AddressString = String.Empty;
      _OldAddressString = String.Empty;
    }

    #endregion

    #region ��������

    [NonSerialized]
    private IFiasSource _Source;

    /// <summary>
    /// ���� ����� �� ������ �������������� � ���������� ����.
    /// </summary>
    /// <param name="source">�������� ������� ����� �������������� �� ������� �������</param>
    public void InternalSetSource(IFiasSource source)
    {
      _Source = source;
    }

    /// <summary>
    /// �������, �� �������� ����� �������� �����.
    /// �������� �� ���������: Room
    /// �������� ����� �������� ������ �� ������ ������� �� �����.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        CheckNotFixed();
        _EditorLevel = value;
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
      set 
      {
        CheckNotFixed();
        _PostalCodeEditable = value; 
      }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// ������ ����������� ������� ������, ������� ������ ���� ������ �� �����������, � �� ����� �������.
    /// �� ��������� - FiasLevel.City, �� ���� ������, ����� � ����� ������ ���� � ����������� ����, � ���������� �����,
    /// ��� ������������� - ������ �������, ���� ��� ��� � �����������.
    /// �������� Unknown ��������� ��� ��������. 
    /// ����������� ����� ��������, ������� House � Room, ���� ��� �� ������� �� ������� FiasEditorLevel.
    /// �������� ����� ������������� ������ �� ������ �������� �� �����. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set 
      {
        CheckNotFixed();
        _MinRefBookLevel = value; 
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// ������� �����
    /// </summary>
    public FiasAddress Address
    {
      get 
      {
        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        return convert.Parse(_AddressString);
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        _AddressString = convert.ToString(value);

      }
    }
    private string _AddressString;
    private string _OldAddressString;

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
        CheckNotFixed();
        _CanBeEmpty = value;
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
        CheckNotFixed();
        _WarningIfEmpty = value;
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
        CheckNotFixed();
        _CanBePartial = value;
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
        CheckNotFixed();
        _WarningIfPartial = value;
      }
    }
    private bool _WarningIfPartial;

    #endregion

    #region ������ � ������

    /// <summary>
    /// �������� ���������� true, ���� ��� �������� ���� ������������ �� ������ ������� ��������� � ��������� �������,
    /// ������� ����� �������� ��� ������ ����� �������.
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// �������� �� ������������ � ���������������� ����.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _AddressString != _OldAddressString;
      }
    }

    /// <summary>
    /// �������� ���������. ����� ���������� ������������ ��������, ������ ���� �������� HasChanges ������� true. 
    /// �� ������������ ������� ����� ����������� �� �������� ������� ������������ <paramref name="part"/>
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// ����� �� ������������ � ���������������� ����.
    /// </summary>
    /// <param name="part">������ ��� ������ ��������</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetString("AddressString", _AddressString);
      _OldAddressString = _AddressString;
    }

    /// <summary>
    /// ��������� ���������, ���������� "� ������ �������".
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// ����� �� ������������ � ���������������� ����.
    /// </summary>
    /// <param name="part">������ ��� ������ ��������</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _AddressString = part.GetString("AddressString");
      _OldAddressString = _AddressString;
    }

    /// <summary>
    /// ���������� true, ���� ������� ������������ ���������� ����� �������� ����� �������� ������
    /// � ������ ������������ ��������� ����.
    /// </summary>
    /// <param name="cfgType">��� ������ ������������, ������������ ����� �� ��������</param>
    /// <returns>true, ���� ������� ����� ������� ������</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// ���������� ��������, ����������� ����� �������� ������, � �������� ������ ������������.
    /// ����� ����������, ������ ���� OnSupportsCfgType() ������ true ��� ��������� ���� ������.
    /// </summary>
    /// <param name="part">������ ������������ ��� ������</param>
    /// <param name="cfgType">��� ������ ������������</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, _AddressString);
    }

    /// <summary>
    /// ��������� ��������, ����������� ����� �������� ������, �� �������� ������ ������������.
    /// ����� ����������, ������ ���� OnSupportsCfgType() ������ true ��� ��������� ���� ������.
    /// </summary>
    /// <param name="part">������ ������������ ��� ������ ��������</param>
    /// <param name="cfgType">��� ������ ������������</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        _AddressString = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��������� ��� ���������� ������ ������
  /// </summary>
  [Serializable]
  public class FiasAddressComboBox : Control
  {
    #region �����������

    /// <summary>
    /// �������������� ��������� �������� �������
    /// </summary>
    public FiasAddressComboBox(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _TextFormat = FiasFormatStringParser.DefaultFormat;
      _AddressString = String.Empty;
      _OldAddressString = String.Empty;
    }

    #endregion

    #region ��������

    [NonSerialized]
    private IFiasSource _Source;

    /// <summary>
    /// ���� ����� �� ������ �������������� � ���������� ����.
    /// </summary>
    /// <param name="source">�������� ������� ����� �������������� �� ������� �������</param>
    public void InternalSetSource(IFiasSource source)
    {
      _Source = source;
    }

    /// <summary>
    /// �������, �� �������� ����� �������� �����.
    /// �������� �� ���������: Room
    /// �������� ����� �������� ������ �� ������ ������� �� �����.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        CheckNotFixed();
        _EditorLevel = value;
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
      set
      {
        CheckNotFixed();
        _PostalCodeEditable = value;
      }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// ������ ����������� ������� ������, ������� ������ ���� ������ �� �����������, � �� ����� �������.
    /// �� ��������� - FiasLevel.City, �� ���� ������, ����� � ����� ������ ���� � ����������� ����, � ���������� �����,
    /// ��� ������������� - ������ �������, ���� ��� ��� � �����������.
    /// �������� Unknown ��������� ��� ��������. 
    /// ����������� ����� ��������, ������� House � Room, ���� ��� �� ������� �� ������� FiasEditorLevel.
    /// �������� ����� ������������� ������ �� ������ �������� �� �����. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        CheckNotFixed();
        _MinRefBookLevel = value;
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// ������ ���������� ������������� ������ � ����.
    /// �� ��������� - "TEXT".
    /// </summary>
    public string TextFormat
    {
      get { return _TextFormat; }
      set
      {
        CheckNotFixed();
        string errorText;
        if (!FiasFormatStringParser.IsValidFormat(value, out errorText))
          throw new ArgumentException(errorText);
      }
    }
    private string _TextFormat;

    /// <summary>
    /// ������� �����
    /// </summary>
    public FiasAddress Address
    {
      get
      {
        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        return convert.Parse(_AddressString);
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        _AddressString = convert.ToString(value);

      }
    }
    private string _AddressString;
    private string _OldAddressString;

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
        CheckNotFixed();
        _CanBeEmpty = value;
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
        CheckNotFixed();
        _WarningIfEmpty = value;
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
        CheckNotFixed();
        _CanBePartial = value;
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
        CheckNotFixed();
        _WarningIfPartial = value;
      }
    }
    private bool _WarningIfPartial;

    #endregion

    #region ������ � ������

    /// <summary>
    /// �������� ���������� true, ���� ��� �������� ���� ������������ �� ������ ������� ��������� � ��������� �������,
    /// ������� ����� �������� ��� ������ ����� �������.
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// �������� �� ������������ � ���������������� ����.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _AddressString != _OldAddressString;
      }
    }

    /// <summary>
    /// �������� ���������. ����� ���������� ������������ ��������, ������ ���� �������� HasChanges ������� true. 
    /// �� ������������ ������� ����� ����������� �� �������� ������� ������������ <paramref name="part"/>
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// ����� �� ������������ � ���������������� ����.
    /// </summary>
    /// <param name="part">������ ��� ������ ��������</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetString("AddressString", _AddressString);
      _OldAddressString = _AddressString;
    }

    /// <summary>
    /// ��������� ���������, ���������� "� ������ �������".
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// ����� �� ������������ � ���������������� ����.
    /// </summary>
    /// <param name="part">������ ��� ������ ��������</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _AddressString = part.GetString("AddressString");
      _OldAddressString = _AddressString;
    }

    /// <summary>
    /// ���������� true, ���� ������� ������������ ���������� ����� �������� ����� �������� ������
    /// � ������ ������������ ��������� ����.
    /// </summary>
    /// <param name="cfgType">��� ������ ������������, ������������ ����� �� ��������</param>
    /// <returns>true, ���� ������� ����� ������� ������</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// ���������� ��������, ����������� ����� �������� ������, � �������� ������ ������������.
    /// ����� ����������, ������ ���� OnSupportsCfgType() ������ true ��� ��������� ���� ������.
    /// </summary>
    /// <param name="part">������ ������������ ��� ������</param>
    /// <param name="cfgType">��� ������ ������������</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, _AddressString);
    }

    /// <summary>
    /// ��������� ��������, ����������� ����� �������� ������, �� �������� ������ ������������.
    /// ����� ����������, ������ ���� OnSupportsCfgType() ������ true ��� ��������� ���� ������.
    /// </summary>
    /// <param name="part">������ ������������ ��� ������ ��������</param>
    /// <param name="cfgType">��� ������ ������������</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        _AddressString = part.GetString(Name);
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��������� ��� ������� ����� ��� ��������� ������.
  /// ������ ������� ������������ ��������� FiasAddressComboBox
  /// </summary>
  [Serializable]
  public class FiasAddressDialog : StandardDialog
  {
    #region �����������

    /// <summary>
    /// �������������� ��������� �������� �������
    /// </summary>
    public FiasAddressDialog(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _AddressString = String.Empty;
      _OldAddressString = String.Empty;
    }

    #endregion

    #region ��������

    [NonSerialized]
    private IFiasSource _Source;

    /// <summary>
    /// ���� ����� �� ������ �������������� � ���������� ����.
    /// </summary>
    /// <param name="source">�������� ������� ����� �������������� �� ������� �������</param>
    public void InternalSetSource(IFiasSource source)
    {
      _Source = source;
    }

    /// <summary>
    /// �������, �� �������� ����� �������� �����.
    /// �������� �� ���������: Room
    /// �������� ����� �������� ������ �� ������ ������� �� �����.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        CheckNotFixed();
        _EditorLevel = value;
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// ���� ���������� � true, �� ������ ����� ������������ ������ ��� ��������� ������, � �� ��� ��������������
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        CheckNotFixed();
        _ReadOnly = value;
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// ����� �� ������������� �������� ������?
    /// �� ��������� - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set
      {
        CheckNotFixed();
        _PostalCodeEditable = value;
      }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// ������ ����������� ������� ������, ������� ������ ���� ������ �� �����������, � �� ����� �������.
    /// �� ��������� - FiasLevel.City, �� ���� ������, ����� � ����� ������ ���� � ����������� ����, � ���������� �����,
    /// ��� ������������� - ������ �������, ���� ��� ��� � �����������.
    /// �������� Unknown ��������� ��� ��������. 
    /// ����������� ����� ��������, ������� House � Room, ���� ��� �� ������� �� ������� FiasEditorLevel.
    /// �������� ����� ������������� ������ �� ������ �������� �� �����. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        CheckNotFixed();
        _MinRefBookLevel = value;
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// ������� �����
    /// </summary>
    public FiasAddress Address
    {
      get
      {
        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        return convert.Parse(_AddressString);
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        if (_Source == null)
          throw new BugException("_Source=null");
        FiasAddressConvert convert = new FiasAddressConvert(_Source);
        _AddressString = convert.ToString(value);

      }
    }
    private string _AddressString;
    private string _OldAddressString;

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
        CheckNotFixed();
        _CanBeEmpty = value;
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
        CheckNotFixed();
        _WarningIfEmpty = value;
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
        CheckNotFixed();
        _CanBePartial = value;
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
        CheckNotFixed();
        _WarningIfPartial = value;
      }
    }
    private bool _WarningIfPartial;

    #endregion

    #region ������ � ������

    /// <summary>
    /// �������� ���������� true, ���� ��� �������� ���� ������������ �� ������ ������� ��������� � ��������� �������,
    /// ������� ����� �������� ��� ������ ����� �������.
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// �������� �� ������������ � ���������������� ����.
    /// </summary>
    public override bool HasChanges
    {
      get
      {
        if (base.HasChanges)
          return true;
        return _AddressString != _OldAddressString;
      }
    }

    /// <summary>
    /// �������� ���������. ����� ���������� ������������ ��������, ������ ���� �������� HasChanges ������� true. 
    /// �� ������������ ������� ����� ����������� �� �������� ������� ������������ <paramref name="part"/>
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// ����� �� ������������ � ���������������� ����.
    /// </summary>
    /// <param name="part">������ ��� ������ ��������</param>
    public override void WriteChanges(CfgPart part)
    {
      base.WriteChanges(part);
      part.SetString("AddressString", _AddressString);
      _OldAddressString = _AddressString;
    }

    /// <summary>
    /// ��������� ���������, ���������� "� ������ �������".
    /// ���������� ���������� ��������, ������� �� ����� �������� ��� ������ �������, �� �����������.
    /// ����� �� ������������ � ���������������� ����.
    /// </summary>
    /// <param name="part">������ ��� ������ ��������</param>
    public override void ReadChanges(CfgPart part)
    {
      base.ReadChanges(part);
      _AddressString = part.GetString("AddressString");
      _OldAddressString = _AddressString;
    }

    /// <summary>
    /// ���������� true, ���� ������� ������������ ���������� ����� �������� ����� �������� ������
    /// � ������ ������������ ��������� ����.
    /// </summary>
    /// <param name="cfgType">��� ������ ������������, ������������ ����� �� ��������</param>
    /// <returns>true, ���� ������� ����� ������� ������</returns>
    protected override bool OnSupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// ���������� ��������, ����������� ����� �������� ������, � �������� ������ ������������.
    /// ����� ����������, ������ ���� OnSupportsCfgType() ������ true ��� ��������� ���� ������.
    /// </summary>
    /// <param name="part">������ ������������ ��� ������</param>
    /// <param name="cfgType">��� ������ ������������</param>
    protected override void OnWriteValues(CfgPart part, RIValueCfgType cfgType)
    {
      part.SetString(Name, _AddressString);
    }

    /// <summary>
    /// ��������� ��������, ����������� ����� �������� ������, �� �������� ������ ������������.
    /// ����� ����������, ������ ���� OnSupportsCfgType() ������ true ��� ��������� ���� ������.
    /// </summary>
    /// <param name="part">������ ������������ ��� ������ ��������</param>
    /// <param name="cfgType">��� ������ ������������</param>
    protected override void OnReadValues(CfgPart part, RIValueCfgType cfgType)
    {
      if (part.HasValue(Name))
        _AddressString = part.GetString(Name);
    }

    #endregion
  }
}
