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
    /// <param name="expression">��������� ���������</param>
    /// <param name="message">���������</param>
    /// <param name="isError">true-������, false-��������������</param>
    public UIValidator(DepValue<bool> expression, string message, bool isError)
      : this(expression, message, isError, null)
    {
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="expressionEx">��������� ���������</param>
    /// <param name="message">���������</param>
    /// <param name="isError">true-������, false-��������������</param>
    /// <param name="preconditionEx">���������, ������������ ������������� ���������� ��������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator(DepValue<bool> expressionEx, string message, bool isError, DepValue<bool> preconditionEx)
    {
      if (expressionEx == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(message))
        throw new ArgumentNullException("message");

      _ExpressionEx = expressionEx;
      expressionEx.ValueChanged += DummyValueChanged; // ����� IsConnected ���������� true
      _Message = message;
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
    /// ��������� ��� ���������� ��������.
    /// ���������� ������ ���������� true, ���� ������� ��������� ���������.
    /// ���� ����������� �������� ����� false, �� ��� ������������ �������� ����� ������ ��������� �� ������ ��� ��������������,
    /// � ����������� �� �������� IsError.
    /// </summary>
    public DepValue<bool> ExpressionEx { get { return _ExpressionEx; } }
    private readonly DepValue<bool> _ExpressionEx;

    /// <summary>
    /// ���������, ������� ����� ������, ���� ����������� ���������� Expression �������� false.
    /// </summary>
    public string Message { get { return _Message; } }
    private readonly string _Message;

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
    /// ���������� �������� Message (��� �������)
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return Message;
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� RIItem.Validators
  /// </summary>
  [Serializable]
  public sealed class UIValidatorList : ListWithReadOnly<UIValidator>
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

    /// <summary>
    /// ������� ������ Validator � IsError=true � ��������� ��� � ������
    /// </summary>
    /// <param name="expressionEx">��������� ���������</param>
    /// <param name="message">���������</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(expressionEx, message, true);
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
      UIValidator item = new UIValidator(expressionEx, message, true, preconditionEx);
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
      UIValidator item = new UIValidator(expressionEx, message, false);
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
      UIValidator item = new UIValidator(expressionEx, message, false, preconditionEx);
      Add(item);
      return item;
    }

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
        if (!this[i].ExpressionEx.Value)
        {
          if (this[i].IsError)
          {
            validableObject.SetError(this[i].Message);
            break;
          }
          else
            validableObject.SetWarning(this[i].Message);
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