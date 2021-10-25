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
    // �������� �������� ��������� � EFPValidateState � ExtForms.dll

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
    /// <param name="expression">��������� ���������</param>
    /// <param name="message">���������</param>
    /// <param name="isError">true-������, false-��������������</param>
    /// <param name="activeEx">���������, ������������ ������������� ���������� ��������. ����� ���� null, ���� �������� ����������� ������</param>
    public UIValidator(DepValue<bool> expression, string message, bool isError, DepValue<bool> activeEx)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(message))
        throw new ArgumentNullException("message");

      _Expression = expression;
      _Message = message;
      _IsError = isError;
      _ActiveEx = activeEx;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��� ���������� ��������.
    /// ���������� ������ ���������� true, ���� ������� ��������� ���������.
    /// ���� ����������� �������� ����� false, �� ��� ������������ �������� ����� ������ ��������� �� ������ ��� ��������������,
    /// � ����������� �� �������� IsError.
    /// </summary>
    public DepValue<bool> Expression { get { return _Expression; } }
    private readonly DepValue<bool> _Expression;

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
    public DepValue<bool> ActiveEx { get { return _ActiveEx; } }
    private readonly DepValue<bool> _ActiveEx;

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

    #region ������

    /// <summary>
    /// ������� ������ Validator � IsError=true � ��������� ��� � ������
    /// </summary>
    /// <param name="expression">��������� ���������</param>
    /// <param name="message">���������</param>
    public UIValidator AddError(DepValue<bool> expression, string message)
    {
      UIValidator item = new UIValidator(expression, message, true);
      Add(item);
      return item;
    }

    /// <summary>
    /// ������� ������ Validator � IsError=false � ��������� ��� � ������
    /// </summary>
    /// <param name="expression">��������� ���������</param>
    /// <param name="message">���������</param>
    public UIValidator AddWarning(DepValue<bool> expression, string message)
    {
      UIValidator item = new UIValidator(expression, message, false);
      Add(item);
      return item;
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
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
