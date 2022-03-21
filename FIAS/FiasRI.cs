// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.RI;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.DependedValues;

// ��������� ���������������� ���������

namespace FreeLibSet.FIAS.RI
{
  /// <summary>
  /// ��������� ��������� ��� ������ ����� ����������� ������.
  /// ������ ������� ������������ ��������� FiasAddressComboBox
  /// </summary>
  [Serializable]
  public class FiasAddressPanel : Control
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
    /// ����� �� ���� ���� ������.
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ��������� - Error - ���� ������ ���� ���������, ����� ����� ���������� ������.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// ����� �� ���� ���� ������.
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ���������: false (���� �������� ������������).
    /// ��� �������� ��������� CanBeEmptyMode, �� �� ��������� ���������� ����� ��������������.
    /// ��� CanBeEmptyMode=Warning ��� �������� ���������� true.
    /// ��������� �������� true ������������ ��������� CanBeEmptyMode=Ok, � false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }


    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ��������� - Error ����� ������ ���� �������� �������� �������� EditorLevel.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        CheckNotFixed();
        _CanBePartialMode = value;
      }
    }
    private UIValidateState _CanBePartialMode;


    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - false - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// ��� �������� ��������� CanBePartialMode, �� �� ��������� ���������� ����� ��������������.
    /// ��� CanBePartialMode=Warning ��� �������� ���������� true.
    /// ��������� �������� true ������������ ��������� CanBePartialMode=Ok, � false - CanBePartialMode=Error.
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

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
        _TextFormat = value; // ����. 23.11.2021
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
    /// ����� �� ���� ���� ������.
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ��������� - Error - ���� ������ ���� ���������, ����� ����� ���������� ������.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// ����� �� ���� ���� ������.
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ���������: false (���� �������� ������������).
    /// ��� �������� ��������� CanBeEmptyMode, �� �� ��������� ���������� ����� ��������������.
    /// ��� CanBeEmptyMode=Warning ��� �������� ���������� true.
    /// ��������� �������� true ������������ ��������� CanBeEmptyMode=Ok, � false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }


    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ��������� - Error ����� ������ ���� �������� �������� �������� EditorLevel.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        CheckNotFixed();
        _CanBePartialMode = value;
      }
    }
    private UIValidateState _CanBePartialMode;


    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - false - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// ��� �������� ��������� CanBePartialMode, �� �� ��������� ���������� ����� ��������������.
    /// ��� CanBePartialMode=Warning ��� �������� ���������� true.
    /// ��������� �������� true ������������ ��������� CanBePartialMode=Ok, � false - CanBePartialMode=Error.
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

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
    /// ����� �� ���� ���� ������.
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ��������� - Error - ���� ������ ���� ���������, ����� ����� ���������� ������.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        CheckNotFixed();
        _CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// ����� �� ���� ���� ������.
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ���������: false (���� �������� ������������).
    /// ��� �������� ��������� CanBeEmptyMode, �� �� ��������� ���������� ����� ��������������.
    /// ��� CanBeEmptyMode=Warning ��� �������� ���������� true.
    /// ��������� �������� true ������������ ��������� CanBeEmptyMode=Ok, � false - CanBeEmptyMode=Error.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }


    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �������� ����� ��������������� ������ �� �������� ������� ���������� �������.
    /// �������� �� ��������� - Error ����� ������ ���� �������� �������� �������� EditorLevel.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        CheckNotFixed();
        _CanBePartialMode = value;
      }
    }
    private UIValidateState _CanBePartialMode;


    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - false - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// ��� �������� ��������� CanBePartialMode, �� �� ��������� ���������� ����� ��������������.
    /// ��� CanBePartialMode=Warning ��� �������� ���������� true.
    /// ��������� �������� true ������������ ��������� CanBePartialMode=Ok, � false - CanBePartialMode=Error.
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

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
  /// ����������� ������, ������� ����� ������������ ��� ��������� ���������� ����������������� ����������,
  /// ���������� � �������� ����.
  /// </summary>
  public static class FiasRITools
  {
    #region ������ �������������� ������

    /// <summary>
    /// ����������� ���������, ������� ����� ������������ � ���������� ���������� ���� ����� ������� ���������� ������������� ������.
    /// 
    /// ���������� UIValidateResult c IsValid=true, ���� ���������� ������ �������� ���������� �������� ��� ���������� ������������� ������.
    /// ���������� ����� FiasFormatStringParser.IsValidFormat().
    /// </summary>
    /// <param name="s">����������� �������� ����������� ������</param>
    /// <returns>true, ���� ������ ����������</returns>
    public static DepValue<UIValidateResult> FormatStringValidateResultEx(DepValue<string> s)
    {
      return new DepExpr1<UIValidateResult, string>(s, FormatStringValidateResult);
    }

    private static UIValidateResult FormatStringValidateResult(string s)
    {
      string errorMessage;
      bool isValid = FiasFormatStringParser.IsValidFormat(s, out errorMessage);
      return new UIValidateResult(isValid, errorMessage);
    }

    /// <summary>
    /// ������� ����������� �������-�������� ��� ����� ������ �������������� ������.
    /// ���������� ������ �������� ������� ��������, �� ����� ������ ������ �������.
    /// ��������� �������� ������������ ������� ������.
    /// </summary>
    /// <returns>����������� �������</returns>
    public static TextComboBox CreateFormatStringTextComboBox()
    {
      TextComboBox control = new TextComboBox(FiasFormatStringParser.ComponentTypes);
      control.Validators.AddError(FormatStringValidateResultEx(control.TextEx),
        control.IsNotEmptyEx);
      return control;
    }

    #endregion
  }
}
