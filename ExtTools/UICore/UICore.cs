using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Calendar;
using System.Diagnostics;

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

  /// <summary>
  /// ����������� ������ ��� ���������� ����������������� ����������
  /// </summary>
  public static class UITools
  {
    #region Text <-> Lines

    /// <summary>
    /// �������������� �������� Text � Lines ��� �������������� ������
    /// </summary>
    /// <param name="text">������</param>
    /// <returns>������ �����</returns>
    public static string[] TextToLines(string text)
    {
      if (String.IsNullOrEmpty(text))
        return DataTools.EmptyStrings;
      else
        return text.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
    }

    /// <summary>
    /// �������������� �������� Lines � Text ��� �������������� ������
    /// </summary>
    /// <param name="lines">������ �����</param>
    /// <returns>������</returns>
    public static string LinesToText(string[] lines)
    {
      if (lines == null)
        return String.Empty;
      if (lines.Length == 0)
        return String.Empty;
      else
        return String.Join(Environment.NewLine, lines);
    }

    #endregion

    #region ����� ��������� ���

    #region ShiftDateRange

    /// <summary>
    /// ����� ��������� ��� ������ ��� �����.
    /// ���� ������� ������ ������������ ����� ����� ����� �������, �� �����
    /// ����������� �� ����� �������, ������������ � �������. ����� ����� ����������� �� ����� ���� � �������.
    /// ���� ����� ������ ���������, �� ��������� �������� �� ���������� � ������������ false.
    /// 
    /// ��� ������ ��������� �������� ������������ ���������. ���� ������ ������ ��������� ����, �� �����
    /// ��������� "����� �����", � <paramref name="forward"/>=false. ���� ������ ������ �������� ����,
    /// �� ����� ��������� "����� ������", � <paramref name="forward"/>=true. 
    /// 
    /// ������������ � ��������� ����������������� ����������, ��������������� ��� ������ � ����������� ���.
    /// ��. ����� ��������� ������ ������/����� ��� ���� DateRange.
    /// </summary>
    /// <param name="firstDate">��������� ���� (�� ������)</param>
    /// <param name="lastDate">�������� ���� (�� ������)</param>
    /// <param name="forward">true - ��� ������ ������, false - �����</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRange(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      if (firstDate.HasValue && lastDate.HasValue)
      {
        // ������� �����
        DateRange dtr = new DateRange(firstDate.Value, lastDate.Value);

        dtr = dtr >> (forward ? +1 : -1);

        firstDate = dtr.FirstDate;
        lastDate = dtr.LastDate;
        return true;
      }

      if (firstDate.HasValue)
      {
        if (!forward)
        {
          if (firstDate.Value == DateTime.MinValue)
            return false; // 02.09.2020
          lastDate = firstDate.Value.AddDays(-1);
          firstDate = null;
          return true;
        }
      }
      if (lastDate.HasValue)
      {
        if (forward)
        {
          if (lastDate.Value == DateTime.MaxValue.Date)
            return false; // 02.09.2020
          firstDate = lastDate.Value.AddDays(1);
          lastDate = null;
          return true;
        }
      }
      return false;
    }

    #endregion

    #region ShiftDateRangeYear

    /// <summary>
    /// ����� ��������� ��� ������ ��� ����� �� ���� ���.
    /// ���� ������� ������ ������������ ����� ����� ����� �������, �� �����
    /// ����������� �� 12 �������. ����� ����� ����������� �� ����. ��� �������� ����� �������� ������ ��� ����������
    /// �����, ���� ������ ������������� � �������.
    /// ��������, ������ {01.10.2018-28.02.2019} ���������� ������ �� {01.10.2019-29.02.2020},
    /// � {02.10.2018-28.02.2019} - �� {02.10.2019-28.02.2020}.
    /// ���� ����� ������ ���������, �� ��������� �������� �� ���������� � ������������ false.
    /// ������������ � ��������� ����������������� ����������, ��������������� ��� ������ � ����������� ���.
    /// </summary>
    /// <param name="firstDate">��������� ���� (�� ������)</param>
    /// <param name="lastDate">�������� ���� (�� ������)</param>
    /// <param name="forward">true ��� ������ ������, false - �����</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRangeYear(ref DateTime firstDate, ref DateTime lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1.Value;
        lastDate = dt2.Value;
      }
      return Res;
    }

    /// <summary>
    /// ����� ��������� ��� ������ ��� ����� �� ���� ���.
    /// ���� ������� ������ ������������ ����� ����� ����� �������, �� �����
    /// ����������� �� 12 �������. ����� ����� ����������� �� ����. ��� �������� ����� �������� ������ ��� ����������
    /// �����, ���� ������ ������������� � �������.
    /// ��������, ������ {01.10.2018-28.02.2019} ���������� ������ �� {01.10.2019-29.02.2020},
    /// � {02.10.2018-28.02.2019} - �� {02.10.2019-28.02.2020}.
    /// ���� ����� ������ ���������, �� ��������� �������� �� ���������� � ������������ false.
    /// ��� ������ ��������� �������� ������������ ���������.
    /// ������������ � ��������� ����������������� ����������, ��������������� ��� ������ � ����������� ���.
    /// </summary>
    /// <param name="firstDate">��������� ���� (�� ������)</param>
    /// <param name="lastDate">�������� ���� (�� ������)</param>
    /// <param name="forward">true ��� ������ ������, false - �����</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      bool Res = false;
      bool IsWholeMonth = false;
      if (firstDate.HasValue && lastDate.HasValue)
      {
        if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
          IsWholeMonth = true;
      }

      if (firstDate.HasValue)
      {
        firstDate = DataTools.CreateDateTime(firstDate.Value.Year + (forward ? +1 : -1), firstDate.Value.Month, firstDate.Value.Day);
        Res = true;
      }

      if (lastDate.HasValue)
      {
        lastDate = DataTools.CreateDateTime(lastDate.Value.Year + (forward ? +1 : -1), lastDate.Value.Month,
          IsWholeMonth ? 31 : lastDate.Value.Day);
        Res = true;
      }
      return Res;
    }

    #endregion

    #endregion

    #region ��������� ������������ ��������


    /// <summary>
    /// ��������� ������������� �������� ��� ��������� ��������� ���� �� ����������� ���������.
    /// ������ ���������� <paramref name="currValue"/>+<paramref name="increment"/>, �� ������� ���������
    /// ���������� ��������� �������� � ������ �������. ��������, ���� <paramref name="increment"/>=5,
    /// �� ����� ������������ �������� 0, 5, 10, 15, ... ��, ��� �������� 1 ������������ 5, � �� 2.
    /// </summary>
    /// <param name="currValue">������� ��������</param>
    /// <param name="increment">��������� (������������� ��������) ��� ��������� (�������������)</param>
    /// <returns>����� ��������</returns>
    public static int GetIncrementedValue(int currValue, int increment)
    {
      return currValue + increment;
    }

    /// <summary>
    /// ��������� ������������� �������� ��� ��������� ��������� ���� �� ����������� ���������.
    /// ������ ���������� <paramref name="currValue"/>+<paramref name="increment"/>, �� ������� ���������
    /// ���������� ��������� �������� � ������ �������. ��������, ���� <paramref name="increment"/>=0.2,
    /// �� ����� ������������ �������� 0, 0.2, 0.4, 0.6, ... ��, ��� �������� 0.1 ������������ 0.2, � �� 0.3.
    /// ���� <paramref name="increment"/> �� �������� ������ ���� 1/n, ��� n-����� �����, �� �����������
    /// ������� ��������.
    /// </summary>
    /// <param name="currValue">������� ��������</param>
    /// <param name="increment">��������� (������������� ��������) ��� ��������� (�������������)</param>
    /// <returns>����� ��������</returns>
    public static float GetIncrementedValue(float currValue, float increment)
    {
      // TODO: �� �����������

      return currValue + increment;
    }

    /// <summary>
    /// ��������� ������������� �������� ��� ��������� ��������� ���� �� ����������� ���������.
    /// ������ ���������� <paramref name="currValue"/>+<paramref name="increment"/>, �� ������� ���������
    /// ���������� ��������� �������� � ������ �������. ��������, ���� <paramref name="increment"/>=0.2,
    /// �� ����� ������������ �������� 0, 0.2, 0.4, 0.6, ... ��, ��� �������� 0.1 ������������ 0.2, � �� 0.3.
    /// ���� <paramref name="increment"/> �� �������� ������ ���� 1/n, ��� n-����� �����, �� �����������
    /// ������� ��������.
    /// </summary>
    /// <param name="currValue">������� ��������</param>
    /// <param name="increment">��������� (������������� ��������) ��� ��������� (�������������)</param>
    /// <returns>����� ��������</returns>
    public static double GetIncrementedValue(double currValue, double increment)
    {
      // TODO: �� �����������

      return currValue + increment;

      /*
      if (increment == 0.0)
        return currValue;

      double incMod = Math.Abs(increment);
      if (incMod >= 1.0)
      {
        if (Math.Truncate(incMod) != incMod)
          return currValue + increment;

        if (increment>0)
          currValue=
      }
      else
      { 
      }
       * */
    }

    /// <summary>
    /// ��������� ������������� �������� ��� ��������� ��������� ���� �� ����������� ���������.
    /// ������ ���������� <paramref name="currValue"/>+<paramref name="increment"/>, �� ������� ���������
    /// ���������� ��������� �������� � ������ �������. ��������, ���� <paramref name="increment"/>=0.2,
    /// �� ����� ������������ �������� 0, 0.2, 0.4, 0.6, ... ��, ��� �������� 0.1 ������������ 0.2, � �� 0.3.
    /// ���� <paramref name="increment"/> �� �������� ������ ���� 1/n, ��� n-����� �����, �� �����������
    /// ������� ��������.
    /// </summary>
    /// <param name="currValue">������� ��������</param>
    /// <param name="increment">��������� (������������� ��������) ��� ��������� (�������������)</param>
    /// <returns>����� ��������</returns>
    public static decimal GetIncrementedValue(decimal currValue, decimal increment)
    {
      // TODO: �� �����������

      return currValue + increment;
    }

    #endregion
  }
}
