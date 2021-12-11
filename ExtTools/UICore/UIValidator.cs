// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Collections;

// ����� ���������� ����������������� ����������.
// ������������ �������� FreeLibSet.RI.Control (��������� ���������������� ���������)
// � FreeLibSet.Forms.EFPControlBase (���������� Windows Forms)
namespace FreeLibSet.UICore
{
  #region ������������ UIValidateState

  /// <summary>
  /// ��������� �������� ������
  /// </summary>
  [Serializable]
  public enum UIValidateState
  {
    // �������� �������� ��������� � UIValidateState � ExtForms.dll

    /// <summary>
    /// ������ �� �������
    /// </summary>
    Ok = 0,

    /// <summary>
    /// ��������������
    /// </summary>
    Warning = 1,

    /// <summary>
    /// ������
    /// </summary>
    Error = 2
  }

  #endregion

  #region ��������� �������, ��������������� �������� ������

  /// <summary>
  /// ��������� �������, ��������������� �������� ������.
  /// ����������� EFPControlBase, EFPValidatingEventArgs � ���������� ������� ��������.
  /// </summary>
  public interface IUIValidableObject
  {
    /// <summary>
    /// ���������� ������
    /// </summary>
    /// <param name="message">���������</param>
    void SetError(string message);

    /// <summary>
    /// ���������� ��������������.
    /// </summary>
    /// <param name="message">���������</param>
    void SetWarning(string message);

    /// <summary>
    /// �������� ������� ��������� ��������
    /// </summary>
    UIValidateState ValidateState { get; }
  }

  /// <summary>
  /// ������� ����� ��� ���������� ����������� ��������
  /// </summary>
  public class UISimpleValidableObject : IUIValidableObject
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    public UISimpleValidableObject()
    {
      Clear();
    }

    #endregion

    #region ������

    /// <summary>
    /// ������������� ��������� Ok
    /// </summary>
    public void Clear()
    {
      _ValidateState = UIValidateState.Ok;
      _Message = null;
    }

    /// <summary>
    /// ������������� ��������� ������.
    /// ���� ��� ����������� ������, ����� ������������.
    /// </summary>
    /// <param name="message">����� ���������</param>
    public void SetError(string message)
    {
      if (ValidateState != UIValidateState.Error)
      {
        _ValidateState = UIValidateState.Error;
        _Message = message;
      }
    }

    /// <summary>
    /// ������������� ��������������
    /// ���� ��� ����������� ������ ��� ��������������, ����� ������������.
    /// </summary>
    /// <param name="message">����� ���������</param>
    public void SetWarning(string message)
    {
      if (ValidateState == UIValidateState.Ok)
      {
        _ValidateState = UIValidateState.Warning;
        _Message = message;
      }
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ���������
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    /// <summary>
    /// ����� ��������� �� ������ ��� ��������������
    /// </summary>
    public string Message { get { return _Message; } }
    private string _Message;

    #endregion
  }

  #endregion

  #region ������� ��� �������� ������

  /// <summary>
  /// �������� ��� ������� ��������
  /// </summary>
  public class UIValidatingEventArgs : IUIValidableObject
  {
    /// <summary>
    /// �������� �������
    /// </summary>
    /// <param name="validableObject">������, ����������� ��������. ��� ���������� ��������� �� ������ ��� ��������������</param>
    public UIValidatingEventArgs(IUIValidableObject validableObject)
    {
      _ValidableObject = validableObject;
    }

    /// <summary>
    /// ������, ����������� ��������. ��� ���������� ��������� �� ������ ��� ��������������
    /// </summary>
    public IUIValidableObject ValidableObject { get { return _ValidableObject; } }
    private IUIValidableObject _ValidableObject;

    /// <summary>
    /// ���������� ��������� �� ������
    /// </summary>
    /// <param name="message">���������</param>
    public void SetError(string message)
    {
      _ValidableObject.SetError(message);
    }

    /// <summary>
    /// ���������� ��������������
    /// </summary>
    /// <param name="message">���������</param>
    public void SetWarning(string message)
    {
      _ValidableObject.SetWarning(message);
    }

    /// <summary>
    /// ���������� ������� ��������� ��������: ������� ������ ��� ��������������
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidableObject.ValidateState; } }

    /// <summary>
    /// ��������������� �����, ���������� SetError() ��� SetWarning()
    /// </summary>
    /// <param name="state">���������</param>
    /// <param name="message">���������</param>
    public void SetState(UIValidateState state, string message)
    {
      if (ValidateState == UIValidateState.Error)
        return;

      switch (state)
      {
        case UIValidateState.Error:
          SetError(message);
          break;
        case UIValidateState.Warning:
          if (ValidateState == UIValidateState.Ok)
            SetWarning(message);
          break;
      }
    }
  }

  /// <summary>
  /// ������� ������� ��������
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void UIValidatingEventHandler(object sender, UIValidatingEventArgs args);

  #endregion

  /// <summary>
  /// ��������� ��� �������� ���������� ���������� ������������ ��������� � ��������� �� ������
  /// </summary>
  [Serializable]
  public struct UIValidateResult : IEquatable<UIValidateResult>
  {
    #region �����������

    /// <summary>
    /// ������������� ���������
    /// </summary>
    /// <param name="isValid">true - ���������� ��������, false - ������</param>
    /// <param name="message">��������� �� ������. ��� <paramref name="isValid"/>=true ������������</param>
    public UIValidateResult(bool isValid, string message)
    {
      if (isValid)
        _Message = null;
      else if (String.IsNullOrEmpty(message))
        _Message = "����� ��������� �� ������ �� �����";
      else
        _Message = message;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �� ������ ��� null.
    /// </summary>
    private string _Message;

    /// <summary>
    /// ��������� ��������. True - ���������� ��������. False - ���� ������
    /// </summary>
    public bool IsValid { get { return Object.ReferenceEquals(_Message, null); } }

    /// <summary>
    /// ����� ��������� �� ������ ��� IsValid=false.
    /// </summary>
    public string Message
    {
      get
      {
        if (Object.ReferenceEquals(_Message, null))
          return String.Empty;
        else
          return _Message;
      }
    }

    /// <summary>
    /// ��������� �������������
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (IsValid)
        return "Ok";
      else
        return _Message;
    }

    #endregion

    #region ��������� ���� ��������

    /// <summary>
    ///  ��������� ���� ��������.
    /// </summary>
    /// <param name="a">������ ������������ ������</param>
    /// <param name="b">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator ==(UIValidateResult a, UIValidateResult b)
    {
      return String.Equals(a._Message, b._Message, StringComparison.Ordinal);
    }

    /// <summary>
    ///  ��������� ���� ��������.
    /// </summary>
    /// <param name="a">������ ������������ ������</param>
    /// <param name="b">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator !=(UIValidateResult a, UIValidateResult b)
    {
      return !(a == b);
    }

    /// <summary>
    ///  ��������� � ������ ��������.
    /// </summary>
    /// <param name="other">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public bool Equals(UIValidateResult other)
    {
      return String.Equals(this._Message, other._Message, StringComparison.Ordinal);
    }

    /// <summary>
    ///  ��������� � ������ ��������.
    /// </summary>
    /// <param name="other">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object other)
    {
      if (other is UIValidateResult)
        return (this == (UIValidateResult)other);
      else
        return false;
    }

    /// <summary>
    /// ��� ���. ��������� ��� ���������� �������������� �����������.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      if (Object.ReferenceEquals(_Message, null))
        return 0;
      else
        return _Message.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// �������� ��� ������ ������� ���������, ��������������� � ������������ ��������.
  /// ���������� ����� ����������� � ������ Control.Validators.
  /// 
  /// ����� ����������� ������.
  /// </summary>
  [Serializable]
  public sealed class UIValidator
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="resultEx">��������� ���������</param>
    /// <param name="isError">true-������, false-��������������</param>
    /// <param name="preconditionEx">���������, ������������ ������������� ���������� ��������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator(DepValue<UIValidateResult> resultEx, bool isError, DepValue<bool> preconditionEx)
    {
      if (resultEx == null)
        throw new ArgumentNullException("resultEx");

      _ResultEx = resultEx;
      resultEx.ValueChanged += DummyValueChanged; // ����� IsConnected ���������� true
      _IsError = isError;
      _PreconditionEx = preconditionEx;
      if (preconditionEx != null)
        preconditionEx.ValueChanged += DummyValueChanged; // ����� IsConnected ���������� true
    }

    static void DummyValueChanged(object sender, EventArgs args)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����������� ��������� ��� ���������� ��������.
    /// ���� � ����������� �������� IsValid ����� false, �� ��� ������������ �������� ����� ������ ��������� �� ������ ��� ��������������,
    /// � ����������� �� �������� IsError.
    /// �� ����� ���� null.
    /// </summary>
    public DepValue<UIValidateResult> ResultEx { get { return _ResultEx; } }
    private readonly DepValue<UIValidateResult> _ResultEx;

    /// <summary>
    /// True, ���� ��������� ������� �������� �������, false-���� ���������������.
    /// ���� ���� �� ���� �� �������� � IsError=true �� �������� ���������, ������ ������� ���� ������� �������� ������ "��".
    /// �������������� �� ������������ �������� �������.
    /// </summary>
    public bool IsError { get { return _IsError; } }
    private readonly bool _IsError;

    /// <summary>
    /// �������������� ����������� ��� ���������� ��������. ���� ��������� ���������� true, �� �������� �����������,
    /// ���� false - �� �����������.
    /// ���� �������� �� ����������� (������), �� �������� �����������.
    /// </summary>
    public DepValue<bool> PreconditionEx { get { return _PreconditionEx; } }
    private readonly DepValue<bool> _PreconditionEx;

    /// <summary>
    /// ���������� �������� UIValidateResult.Message ��� "Ok" (��� �������)
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return _ResultEx.Value.ToString();
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� RIItem.Validators
  /// </summary>
  [Serializable]
  public class UIValidatorList : ListWithReadOnly<UIValidator>
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    public UIValidatorList()
    {
    }

    #endregion

    #region ������ �������� �����������

    #region AddError()

    /// <summary>
    /// ������� ������ Validator � IsError=true � ��������� ��� � ������
    /// </summary>
    /// <param name="resultEx">��������� ���������</param>
    /// <param name="preconditionEx">��������� �����������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator AddError(DepValue<UIValidateResult> resultEx, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(resultEx, true, preconditionEx);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=true � ��������� ��� � ������
    /// </summary>
    /// <param name="resultEx">��������� ���������</param>
    public UIValidator AddError(DepValue<UIValidateResult> resultEx)
    {
      UIValidator item = new UIValidator(resultEx, true, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=true � ��������� ��� � ������
    /// </summary>
    /// <param name="expressionEx">��������� ���������</param>
    /// <param name="message">���������</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), true, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=true � ��������� ��� � ������
    /// </summary>
    /// <param name="expressionEx">��������� ���������</param>
    /// <param name="message">���������</param>
    /// <param name="preconditionEx">��������� �����������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), true, preconditionEx);
      Add(item);
      return item;
    }

    #endregion

    #region AddWarning()

    /// <summary>
    /// ������� ������ Validator � IsError=false � ��������� ��� � ������
    /// </summary>
    /// <param name="resultEx">��������� ���������</param>
    /// <param name="preconditionEx">��������� �����������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator AddWarning(DepValue<UIValidateResult> resultEx, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(resultEx, false, preconditionEx);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=false � ��������� ��� � ������
    /// </summary>
    /// <param name="resultEx">��������� ���������</param>
    public UIValidator AddWarning(DepValue<UIValidateResult> resultEx)
    {
      UIValidator item = new UIValidator(resultEx, false, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=false � ��������� ��� � ������
    /// </summary>
    /// <param name="expressionEx">��������� ���������</param>
    /// <param name="message">���������</param>
    public UIValidator AddWarning(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), false, null);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=false � ��������� ��� � ������
    /// </summary>
    /// <param name="expressionEx">��������� ���������</param>
    /// <param name="message">���������</param>
    /// <param name="preconditionEx">��������� �����������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator AddWarning(DepValue<bool> expressionEx, string message, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(UITools.CreateValidateResultEx(expressionEx, message), false, preconditionEx);
      Add(item);
      return item;
    }

    #endregion

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �������� ����������� � ������ � ��������� ��������� �� ������ ��� �������������� � <paramref name="validableObject"/>.
    /// ����������, ��� ������� PreconditionEx ���������� false, ������������.
    /// �������� �������������, ���� ��������� ��������� ��������� ��������� �� ������.
    /// ���� ����� �� ������������ � ���������� ����.
    /// </summary>
    /// <param name="validableObject">���������� ���������� ��� ���������� ���������.</param>
    public void Validate(IUIValidableObject validableObject)
    {
#if DEBUG
      if (validableObject == null)
        throw new ArgumentNullException("validableObject");
#endif

      if (validableObject.ValidateState == UIValidateState.Error)
        return;

      for (int i = 0; i < Count; i++)
      {
        if (this[i].PreconditionEx != null)
        {
          if (!this[i].PreconditionEx.Value)
            continue;
        }
        if (!this[i].ResultEx.Value.IsValid)
        {
          if (this[i].IsError)
          {
            validableObject.SetError(this[i].ResultEx.Value.Message);
            break;
          }
          else
            validableObject.SetWarning(this[i].ResultEx.Value.Message);
        }
      }
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ��������� ������ � ����� "������ ������".
    /// ����� �� ������ �������������� � ���������� ����
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������ ������ �����������.
    /// �������� ������ IsReadOnly=true.
    /// </summary>
    public static readonly UIValidatorList Empty = CreateEmpty();

    private static UIValidatorList CreateEmpty()
    {
      UIValidatorList obj = new UIValidatorList();
      obj.SetReadOnly();
      return obj;
    }

    #endregion
  }

  /// <summary>
  /// ������ �����������, ��������������� ��� �������� ������ �������� �� ������.
  /// ��������� ������ UIValidatorList ��������� ValueEx.
  /// </summary>
  [Serializable]
  public class UIValueValidatorList<T> : UIValidatorList
  {
    #region ValueEx

    /// <summary>
    /// ����������� ��������, ������������ ������� ����������� ��������.
    /// </summary>
    public DepValue<T> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
    }
    private DepInput<T> _ValueEx;

    /// <summary>
    /// ���������� true, ���� ���������� �������� ValueEx ����������� � ������ �������� � �������� �����.
    /// ��� �������� �� ������������� ��� ������������� � ���������������� ����
    /// </summary>
    public bool InternalValueExConnected
    {
      get
      {
        if (_ValueEx == null)
          return false;
        else
          return _ValueEx.IsConnected;
      }
    }

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<T>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
      }
    }

    /// <summary>
    /// ���� ����� �� ������������ ��� ������������� � ���������������� ����
    /// </summary>
    /// <param name="value"></param>
    public void InternalSetValueEx(DepValue<T> value)
    {
      InitValueEx();
      _ValueEx.Source = value;
    }

    /// <summary>
    /// ���� ����� �� ������������ ��� ������������� � ���������������� ����
    /// </summary>
    /// <param name="value"></param>
    public void InternalSetValue(T value)
    {
      InitValueEx();
      _ValueEx.Value = value;
    }

    #endregion
  }


  #region ���������� ����������� ���������

#if XXX
  /// <summary>
  /// ��������� ������������ ��������
  /// </summary>
  public interface IUIControl
  {
    /// <summary>
    /// ����������� ��������
    /// </summary>
    bool Enabled { get; set;}

    /// <summary>
    /// ����������� �������� ��� Enabled
    /// </summary>
    DepValue<bool> EnabledEx { get;set;}

    /// <summary>
    /// �� ������������� ��� ������������� � ���������� ����
    /// </summary>
    bool EnabledExConnected { get;}

    /// <summary>
    /// ����������� �������
    /// </summary>
    UIValidatorList Validators { get;}
  }

  /// <summary>
  /// ����������� �������, �������������� �������� ������� ��������
  /// </summary>
  public interface IUIControlCanBeEmpty : IUIControl
  {
    /// <summary>
    /// True, ���� ������� ����� ��������� ������ ��������.
    /// �� ��������� - false.
    /// </summary>
    bool CanBeEmpty { get;set;}

    /// <summary>
    /// ���������� ��� �������� CanBeEmpty, �������������� ������ ��������������
    /// </summary>
    UIValidateState CanBeEmptyMode { get;set;}
  }

  /// <summary>
  /// ����������� �������, �������������� �������� �������� (�������� ReadOnly)
  /// </summary>
  public interface IUIControlWithReadOnly : IUIControl
  {
    /// <summary>
    /// True, ���� ������� ������������ ������ ��� ��������� ��������
    /// </summary>
    bool ReadOnly { get;set;}

    /// <summary>
    /// ����������� �������� ��� ReadOnly
    /// </summary>
    DepValue<bool> ReadOnlyEx { get;set;}
  }
#endif
  #endregion
}
