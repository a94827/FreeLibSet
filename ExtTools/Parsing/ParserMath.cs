using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Remoting;
using FreeLibSet.Collections;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace FreeLibSet.Parsing
{
  /// <summary>
  /// �����, ����������� �������� ��������
  /// </summary>
  /// <param name="op">���� ��������</param>
  /// <param name="arg1">����� ��������</param>
  /// <param name="arg2">������ ��������</param>
  /// <returns></returns>
  public delegate object BinaryOpDelegate(string op, object arg1, object arg2);

  /// <summary>
  /// �������� �������� ��������
  /// ������� ���������� �������� MathOpDef � ������ MathOpParser ������ ���������������
  /// ��������� ���������� ��������
  /// </summary>
  public class BinaryOpDef : ObjectWithCode
  {
    #region ��������� �����������

    /// <summary>
    /// �� ������������
    /// </summary>
    public const int PriorityPower = 500;

    /// <summary>
    /// �������� ��������� � ������� ����� ������ ���������
    /// </summary>
    public const int PriorityMulDiv = 400;

    /// <summary>
    /// �������� �������� � ��������� ����� ������� ���������
    /// </summary>
    public const int PriorityAddSub = 300;

    /// <summary>
    /// �������� ��������� ����� ������ ��������
    /// </summary>
    public const int PriorityCompare = 200;

    /// <summary>
    /// �� ������������
    /// </summary>
    public const int PriorityLogical = 100;

    #endregion

    #region �����������

    /// <summary>
    /// ������� �������� ��������
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="calcMethod">����� ����������</param>
    /// <param name="priority">��������� ��������. ��� ������ ��������, ��� ���� ���������</param>
    public BinaryOpDef(string op, BinaryOpDelegate calcMethod, int priority)
      : base(op)
    {
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");
      _CalcMethod = calcMethod;
      _Priority = priority;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� ��������.
    /// �������� � ������������.
    /// </summary>
    public string Op { get { return base.Code; } }

    /// <summary>
    /// ����� ���������� ��������.
    /// �������� � ������������
    /// </summary>
    public BinaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private BinaryOpDelegate _CalcMethod;

    /// <summary>
    /// ���������� ��������.
    /// ������������, ����� ��������� �������� ���� ������ ��� ������
    /// ��� ������ ��������, ��� ���� ��������� ��������.
    /// ��������, �������� �������� ��������� ������, ��� �������� ��������.
    /// ��� ����������� �������� ������������ ��������� PriorityXXX
    /// </summary>
    public int Priority { get { return _Priority; } }
    private int _Priority;

    #endregion
  }

  /// <summary>
  /// �����, ����������� ������� ��������
  /// </summary>
  /// <param name="op">���� ��������</param>
  /// <param name="arg">�������� (������ �� ��������)</param>
  /// <returns></returns>
  public delegate object UnaryOpDelegate(string op, object arg);

  /// <summary>
  /// �������� ������� ��������
  /// ������� ���������� �������� MathOpDef � ������ MathOpParser ������ ���������������
  /// ��������� ���������� ��������
  /// </summary>
  public class UnaryOpDef : ObjectWithCode
  {
    #region �����������

    /// <summary>
    /// ������� �������� ��������
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="calcMethod">����� ��� ���������� ��������</param>
    public UnaryOpDef(string op, UnaryOpDelegate calcMethod)
      : base(op)
    {
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");
      _CalcMethod = calcMethod;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ��������.
    /// �������� � ������������.
    /// </summary>
    public string Op { get { return base.Code; } }

    /// <summary>
    /// ����� ���������� ��������.
    /// �������� � �����������.
    /// </summary>
    public UnaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private UnaryOpDelegate _CalcMethod;

    #endregion
  }

  /// <summary>
  /// ������ ��� ������� �������������� ��������.
  /// ������� �������, ����������� � ����������: "+", "-", "*", "/", ����� ���������, ������� ������ "()".
  /// </summary>
  public class MathOpParser : IParser
  {
    #region ������������

    /// <summary>
    /// ������� ������, ������������ ����������� �������������� � ���������� ��������
    /// </summary>
    public MathOpParser()
      : this(true)
    {
    }

    /// <summary>
    /// ������� ������, � ������������ ���������� ��������.
    /// </summary>
    /// <param name="useDefaultOps">���� �� ��������� �������� ����������� �������������� � ���������� ��������.
    /// ���� false, �� �������� ���� �������� ������ ���� ��������� �������</param>
    public MathOpParser(bool useDefaultOps)
    {
      _BinaryOps = new NamedList<BinaryOpDef>();
      _UnaryOps = new NamedList<UnaryOpDef>();

      if (useDefaultOps)
      {
        BinaryOpDelegate BinaryD = new BinaryOpDelegate(BinaryCalc);
        UnaryOpDelegate UnaryD = new UnaryOpDelegate(UnaryCalc);

        // 10.10.2017
        // ������� ������ ���� �������� �� ���� ��������, � ����� - �� ������,
        // �����, ��������, �������� ">=" ����� ���������� ��� ">" � "="

        string[] BinarySigns = new string[] { 
          "<>", ">=", "<=",
          "*", "/", "+", "-", "=", ">", "<" };
        int[] BinaryPriorities = new int[] { 
          BinaryOpDef.PriorityCompare, // "<>"
          BinaryOpDef.PriorityCompare, // ">="
          BinaryOpDef.PriorityCompare, // "<="
          BinaryOpDef.PriorityMulDiv,  // "*"
          BinaryOpDef.PriorityMulDiv,  // "/"
          BinaryOpDef.PriorityAddSub,  // "+"
          BinaryOpDef.PriorityAddSub,  // "-"
          BinaryOpDef.PriorityCompare, // "="
          BinaryOpDef.PriorityCompare, // ">"
          BinaryOpDef.PriorityCompare  // "<"
        };

        for (int i = 0; i < BinarySigns.Length; i++)
          BinaryOps.Add(new BinaryOpDef(BinarySigns[i], BinaryD, BinaryPriorities[i])); // ���� ������� �� ���

        string[] UnarySigns = new string[] { "+", "-" };

        for (int i = 0; i < UnarySigns.Length; i++)
          UnaryOps.Add(new UnaryOpDef(UnarySigns[i], UnaryD)); // ���� ������� �� ���
      }
    }

    #endregion

    #region ������ ��������

    /// <summary>
    /// ������ ���������� �������� ��������
    /// </summary>
    public NamedList<BinaryOpDef> BinaryOps { get { return _BinaryOps; } }
    private NamedList<BinaryOpDef> _BinaryOps;

    /// <summary>
    /// ������ ���������� ������� ��������
    /// </summary>
    public NamedList<UnaryOpDef> UnaryOps { get { return _UnaryOps; } }
    private NamedList<UnaryOpDef> _UnaryOps;

    #endregion

    #region ����������

    #region �������� ��������

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� ��������
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object BinaryCalc(string op, object arg1, object arg2)
    {
      if (arg1 == null)
      {
        if (arg2 == null)
          return CalcNull(op);
        else
          arg1 = DataTools.GetEmptyValue(arg2.GetType());
      }
      else if (arg2 == null)
        arg2 = DataTools.GetEmptyValue(arg1.GetType());

      if (arg1 is DateTime || arg2 is DateTime)
      {
        // ��� ��� �������. ���� ����� ������������ � �������
        if (arg1 is DateTime)
        {
          if (arg2 is DateTime)
            return CalcDateTime(op, (DateTime)arg1, (DateTime)arg2);
          else if (arg2 is TimeSpan)
            return CalcDateTimeAndTimeSpan(op, (DateTime)arg1, (TimeSpan)arg2);
          else
            return CalcDateTimeAndDouble(op, (DateTime)arg1, DataTools.GetDouble(arg2));
        }
        else
        {
          if (arg1 is TimeSpan)
            return CalcTimeSpanAndDateTime(op, (TimeSpan)arg1, (DateTime)arg2);
          else
            return CalcDoubleAndDateTime(op, DataTools.GetDouble(arg1), (DateTime)arg2);
        }
      }

      if (arg1 is TimeSpan || arg2 is TimeSpan)
      {
        if (arg1 is TimeSpan)
        {
          if (arg2 is TimeSpan)
            return CalcTimeSpan(op, (TimeSpan)arg1, (TimeSpan)arg2);
          else
            return CalcTimeSpanAndDouble(op, (TimeSpan)arg1, DataTools.GetDouble(arg2));
        }
        else
          return CalcDouble(op, DataTools.GetDouble(arg1), DataTools.GetDouble(arg2));
      }

      if (arg1 is decimal || arg2 is decimal)
        return CalcDecimal(op, DataTools.GetDecimal(arg1), DataTools.GetDecimal(arg2));

      if (arg1 is double || arg2 is double)
        return CalcDouble(op, DataTools.GetDouble(arg1), DataTools.GetDouble(arg2));

      if (arg1 is float || arg2 is float)
        return CalcSingle(op, DataTools.GetSingle(arg1), DataTools.GetSingle(arg2));

      if (arg1 is int || arg2 is int)
        return CalcInt(op, DataTools.GetInt(arg1), DataTools.GetInt(arg2));

      if (arg1 is string || arg2 is string)
        //throw new NotSupportedException("�������������� �������� ��� �������� �� ��������������. ����������� �������");
        return CalcString(op, DataTools.GetString(arg1), DataTools.GetString(arg2));
      if (arg1 is bool && arg2 is bool)
        return CalcBool(op, DataTools.GetBool(arg1), DataTools.GetBool(arg2));

      throw new NotSupportedException("��������� �� �����������. ��� ������ ��������� 1:" + arg1.GetType().ToString() + ", ��������� 2: " + arg2.GetType().ToString());
    }

    /// <summary>
    /// "���������" ������� �������� ��� ���� ���������� null.
    /// ��� �������� ���������, ���������� ���� ���������, ���������� true.
    /// ��� �������� "������", "������" � "�� �����", ���������� false.
    /// ��� �������������� �������� ���������� null.
    /// </summary>
    /// <param name="op"></param>
    /// <returns></returns>
    public static object CalcNull(string op)
    {
      switch (op)
      {
        case "*":
        case "/":
        case "+":
        case "-":
          return null;
        case "=":
        case ">=":
        case "<=":
          return true;
        case "!=":
        case ">":
        case "<":
          return false;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� ����������� ��� ���������� null");
      }
    }

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� �������� ��� ���� Decimal
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object CalcDecimal(string op, decimal arg1, decimal arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "*": return arg1 * arg2;
        case "/": return arg1 / arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �����������");
      }
    }

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� �������� ��� ���� Double
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object CalcDouble(string op, double arg1, double arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "*": return arg1 * arg2;
        case "/": return arg1 / arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �����������");
      }
    }

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� �������� ��� ���� Single
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object CalcSingle(string op, float arg1, float arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "*": return arg1 * arg2;
        case "/": return arg1 / arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �����������");
      }
    }

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� �������� ��� ���� Int32
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object CalcInt(string op, int arg1, int arg2)
    {
      // 01.03.2017
      // ������� �������� ��������� ��� �������������� ���������
      int res1;
      try
      {
        checked // ����������� � ��������� ������������
        {
          switch (op)
          {
            case "+": return arg1 + arg2;
            case "-": return arg1 - arg2;
            case "*": return arg1 * arg2;
            case "/":
              res1 = arg1 / arg2; // ��� ����� ���� ������������ ������ ��������
              break;

            case "=": return arg1 == arg2;
            case "<>": return arg1 != arg2;
            case ">": return arg1 > arg2;
            case "<": return arg1 < arg2;
            case ">=": return arg1 >= arg2;
            case "<=": return arg1 <= arg2;
            default:
              throw new InvalidOperationException("�������� \"" + op + "\" �� �����������");
          }
        }
      }
      catch
      {
        // � ������ ������������ ��������� ���� ����� ��� ���� Double
        return CalcDouble(op, (double)arg1, (double)arg2);
      }

      // ��� �������� ������� ��������� ���� ����� ��� ���� Double
      double res2 = (double)CalcDouble(op, (double)arg1, (double)arg2);

      if (Math.Abs(res2) > (double)(Int32.MaxValue))
        return res2; // ������������ ������ �����

      if (res2 == (double)res1)
        return res1; // ��� ������ ��������
      else
        return res2; // ���� ������ ��������
    }

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� �������� ��� ���� String
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object CalcString(string op, string arg1, string arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        default:
          throw new InvalidOperationException("��� ����� ��������� ������ �������� \"+\", \"=\" � \"<>\". �������� \"" + op + "\" �� �����������");
      }
    }

    private static object CalcBool(string Op, bool Arg1, bool Arg2)
    {
      switch (Op)
      {
        case "=": return Arg1 == Arg2;
        case "<>": return Arg1 != Arg2;
        default:
          throw new InvalidOperationException("��� ���������� �������� ��������� ������ �������� \"=\" � \"<>\". �������� \"" + Op + "\" �� ���������");
      }
    }

    #region DateTime � TimeSpan

    /// <summary>
    /// ����������� ���������� �������������� � ���������� �������� �������� ��� ���� DateTime
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg1">������ ��������</param>
    /// <param name="arg2">������ ��������</param>
    /// <returns>��������� ��������</returns>
    public static object CalcDateTime(string op, DateTime arg1, DateTime arg2)
    {
      switch (op)
      {
        case "-": return arg1 - arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �������������� ��� ���� ���������� DateTime");
      }
    }

    private static object CalcDateTimeAndDouble(string op, DateTime arg1, double arg2)
    {
      switch (op)
      {
        case "+": return arg1 + TimeSpan.FromDays(arg2);
        case "-": return arg1 + TimeSpan.FromDays(arg2);
      }

      DateTime dt2 = DateTime.FromOADate(arg2);
      return CalcDateTime(op, arg1, dt2);
    }

    private static object CalcDoubleAndDateTime(string op, double arg1, DateTime arg2)
    {
      DateTime dt1 = DateTime.FromOADate(arg1);
      switch (op)
      {
        case "=": return dt1 == arg2;
        case "<>": return dt1 != arg2;
        case ">": return dt1 > arg2;
        case "<": return dt1 < arg2;
        case ">=": return dt1 >= arg2;
        case "<=": return dt1 <= arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �������������� ��� ���� ���������� DateTime");
      }
    }


    private static object CalcTimeSpan(string op, TimeSpan arg1, TimeSpan arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �������������� ��� ���� ���������� TimeSpan");
      }
    }

    private static object CalcTimeSpanAndDateTime(string op, TimeSpan arg1, DateTime arg2)
    {
      throw new InvalidOperationException("�������� � ����������� TimeSpan � DateTime ����������");
    }

    private static object CalcDateTimeAndTimeSpan(string op, DateTime arg1, TimeSpan arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �������������� ��� ���� ���������� TimeSpan");
      }
    }

    private static object CalcTimeSpanAndDouble(string op, TimeSpan arg1, double arg2)
    {
      switch (op)
      {
        case "+": return arg1 + TimeSpan.FromDays(arg2);
        case "-": return arg1 - TimeSpan.FromDays(arg2);
        default:
          throw new InvalidOperationException("�������� \"" + op + "\" �� �������������� ��� ���������� TimeSpan � Double");
      }
    }


    #endregion

    #endregion

    #region ������� ��������

    /// <summary>
    /// ����������� ���������� ������� ��������
    /// </summary>
    /// <param name="op">���� ��������</param>
    /// <param name="arg">��������</param>
    /// <returns>��������� ��������</returns>
    public static object UnaryCalc(string op, object arg)
    {
      if (arg == null)
        return null;

      if (arg is decimal)
        return CalcDecimal(op, (decimal)arg);

      if (arg is double)
        return CalcDouble(op, (double)arg);

      if (arg is float)
        return CalcSingle(op, (float)arg);

      if (arg is int)
        return CalcInt(op, (int)arg);

      if (arg is TimeSpan)
        return CalcTimeSpan(op, (TimeSpan)arg);

      throw new NotSupportedException("������� �������� �� ����������� ��� ���� " + arg.GetType().ToString());
    }

    private static object CalcDecimal(string op, decimal arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("������� �������� \"" + op + "\" �� ����������� ��� Decimal");
      }
    }

    private static object CalcDouble(string op, double arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("������� �������� \"" + op + "\" �� ����������� ��� Double");
      }
    }

    private static object CalcSingle(string op, float arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("������� �������� \"" + op + "\" �� ����������� ��� Single");
      }
    }

    private static object CalcInt(string op, int arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("������� �������� \"" + op + "\" �� ����������� ��� Int32");
      }
    }

    private static object CalcTimeSpan(string op, TimeSpan arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("������� �������� \"" + op + "\" �� ����������� ��� TimeSpan");
      }
    }

    #endregion

    #endregion

    #region ������-����������� IExperession

    /// <summary>
    /// �������� �������� "+", "-", "*", "/", �������� ���������
    /// </summary>
    public class BinaryExpression : IExpression
    {
      #region �����������

      /// <summary>
      /// ������� ��������� ��� �������� ��������
      /// </summary>
      /// <param name="opToken">�������</param>
      /// <param name="leftExpression">��������� ����� �� ��������</param>
      /// <param name="rightExpression">��������� ������ �� ��������</param>
      /// <param name="calcMethod">����������� �����</param>
      public BinaryExpression(Token opToken, IExpression leftExpression, IExpression rightExpression, BinaryOpDelegate calcMethod)
      {
        if (opToken == null)
          throw new ArgumentNullException("opToken");
        if (leftExpression == null)
          throw new ArgumentNullException("leftExpression");
        if (rightExpression == null)
          throw new ArgumentNullException("rightExpression");
        if (calcMethod == null)
          throw new ArgumentNullException("calcMethod");

        _OpToken = opToken;
        _LeftExpression = leftExpression;
        _RightExpression = rightExpression;
        _CalcMethod = calcMethod;
      }

      #endregion

      #region ��������

      /// <summary>
      /// �������
      /// </summary>
      public Token OpToken { get { return _OpToken; } }
      private Token _OpToken;

      /// <summary>
      /// ��������� ����� �� ��������
      /// </summary>
      public IExpression LeftExpression { get { return _LeftExpression; } }
      private IExpression _LeftExpression;

      /// <summary>
      /// ��������� ������ �� ��������
      /// </summary>
      public IExpression RightExpression { get { return _RightExpression; } }
      private IExpression _RightExpression;

      /// <summary>
      /// ����������� �����
      /// </summary>
      public BinaryOpDelegate CalcMethod { get { return _CalcMethod; } }
      private BinaryOpDelegate _CalcMethod;

      /// <summary>
      /// ���� �������� "+", "-", "*" ��� "/"
      /// </summary>
      public string Op { get { return OpToken.TokenType; } }

      #endregion

      #region IExpression Members

      /// <summary>
      /// ��������� ���������� ���������.
      /// ���������� ����� � ������ ���������, ����� ����������� CalcMethod ��� ��������
      /// </summary>
      /// <returns>��������� ��������</returns>
      public object Calc()
      {
        object v1 = _LeftExpression.Calc();
        object v2 = _RightExpression.Calc();
        return _CalcMethod(_OpToken.TokenType, v1, v2);
      }

      /// <summary>
      /// ���������� true, ���� � ����� � ������ ��������� �������� �����������
      /// </summary>
      public bool IsConst
      {
        get
        {
          return _LeftExpression.IsConst && _RightExpression.IsConst;
        }
      }

      /// <summary>
      /// ��������� � ������ OpToken
      /// </summary>
      /// <param name="tokens">������ ��� ����������</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_OpToken);
      }

      /// <summary>
      /// ��������� � ������ ����� � ������ ���������
      /// </summary>
      /// <param name="expressions">������ ��� ����������</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        expressions.Add(_LeftExpression);
        expressions.Add(_RightExpression);
      }


      /// <summary>
      /// ����������� ���������
      /// </summary>
      /// <param name="data">����������� ������</param>
      public void Synthesize(SynthesisData data)
      {
        #region ������������� ����������� ������

        bool LeftExprWithP = false;
        if (_LeftExpression is UnaryExpression)
          LeftExprWithP = true;
        else if (_LeftExpression is BinaryExpression)
        {
          BinaryExpression be = (BinaryExpression)_LeftExpression;
          switch (this.Op)
          {
            case "+":
            case "-": break;
            case "*":
            case "/":
              switch (be.Op)
              {
                case "+":
                case "-":
                  LeftExprWithP = true;
                  break;
              }
              break;
          }
        }

        bool RightExprWithP = false;
        if (_RightExpression is UnaryExpression)
          RightExprWithP = true;
        else if (_RightExpression is BinaryExpression)
        {
          BinaryExpression be = (BinaryExpression)_RightExpression;
          switch (this.Op)
          {
            case "+": break;
            case "-":
              switch (be.Op)
              {
                case "+":
                case "-":
                  RightExprWithP = true;
                  break;
              }
              break;

            case "*":
            case "/":
              RightExprWithP = true;
              break;
          }
        }

        #endregion

        if (LeftExprWithP)
          data.Tokens.Add(new SynthesisToken(data, this, "("));
        _LeftExpression.Synthesize(data);
        if (LeftExprWithP)
          data.Tokens.Add(new SynthesisToken(data, this, ")"));

        if (data.UseSpaces)
          data.Tokens.Add(new SynthesisToken(data, this, "Space", " "));
        data.Tokens.Add(new SynthesisToken(data, this, Op));
        if (data.UseSpaces)
          data.Tokens.Add(new SynthesisToken(data, this, "Space", " "));

        if (RightExprWithP)
          data.Tokens.Add(new SynthesisToken(data, this, "("));
        _RightExpression.Synthesize(data);
        if (RightExprWithP)
          data.Tokens.Add(new SynthesisToken(data, this, ")"));
      }

      #endregion

      #region ��������� �������������

      /// <summary>
      /// ���������� �������� Op
      /// </summary>
      /// <returns>��������� �������������</returns>
      public override string ToString()
      {
        return Op;
#if XXX
        StringBuilder sb = new StringBuilder();
        sb.Append('(');
        sb.Append(FLeftExpression.ToString());
        sb.Append(')');
        sb.Append(Op);
        sb.Append('(');
        sb.Append(FRightExpression.ToString());
        sb.Append(')');
        return sb.ToString();
#endif
      }

      #endregion
    }

    /// <summary>
    /// ������� �������� "+" (������ �� ������) � "-"
    /// </summary>
    public class UnaryExpression : IExpression
    {
      #region �����������

      /// <summary>
      /// ������� ���������
      /// </summary>
      /// <param name="opToken">������� ��������</param>
      /// <param name="rightExpression">��������� ������ �� ��������. �� ����� ���� null</param>
      /// <param name="calcMethod">����������� �����</param>
      public UnaryExpression(Token opToken, IExpression rightExpression, UnaryOpDelegate calcMethod)
      {
        if (opToken == null)
          throw new ArgumentNullException("opToken");
        if (rightExpression == null)
          throw new ArgumentNullException("rightExpression");
        if (calcMethod == null)
          throw new ArgumentNullException("calcMethod");

        _OpToken = opToken;
        _RightExpression = rightExpression;
        _CalcMethod = calcMethod;
      }

      #endregion

      #region ��������

      /// <summary>
      /// ������� ������� ��������
      /// </summary>
      public Token OpToken { get { return _OpToken; } }
      private Token _OpToken;

      /// <summary>
      /// ��������� ������ �� ����� ��������
      /// </summary>
      public IExpression RightExpression { get { return _RightExpression; } }
      private IExpression _RightExpression;

      /// <summary>
      /// ����������� �����
      /// </summary>
      public UnaryOpDelegate CalcMethod { get { return _CalcMethod; } }
      private UnaryOpDelegate _CalcMethod;

      /// <summary>
      /// ���� �������� "+", "-"
      /// </summary>
      public string Op { get { return OpToken.TokenType; } }

      #endregion

      #region IExpression Members

      /// <summary>
      /// ��������� ��������� ������, ����� - ������� ��������
      /// </summary>
      /// <returns>��������� ����������</returns>
      public object Calc()
      {
        object v2 = _RightExpression.Calc();
        return _CalcMethod(_OpToken.TokenType, v2);
      }

      /// <summary>
      /// ���������� true, ���� ��������� ������ �������� ����������
      /// </summary>
      public bool IsConst
      {
        get
        {
          return _RightExpression.IsConst;
        }
      }

      /// <summary>
      /// ��������� ������� � ������
      /// </summary>
      /// <param name="tokens">������ ��� ����������</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_OpToken);
      }

      /// <summary>
      /// ��������� ��������� ������ � ������
      /// </summary>
      /// <param name="expressions">������ ��� ����������</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        expressions.Add(_RightExpression);
      }


      /// <summary>
      /// ����������� ���������
      /// </summary>
      /// <param name="data">������ ��� ����������</param>
      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, Op));
        data.Tokens.Add(new SynthesisToken(data, this, "("));
        _RightExpression.Synthesize(data);
        data.Tokens.Add(new SynthesisToken(data, this, ")"));
      }
      #endregion

      #region ��������� �������������

      /// <summary>
      /// ���������� ���� ��������
      /// </summary>
      /// <returns>��������� �������������</returns>
      public override string ToString()
      {
        return Op;
#if XXX
        StringBuilder sb = new StringBuilder();
        sb.Append(Op);
        sb.Append('(');
        sb.Append(FRightExpression.ToString());
        sb.Append(')');
        return sb.ToString();
#endif
      }

      #endregion
    }

    /// <summary>
    /// ������� ������ "()" ��� ������� ������� ����������
    /// </summary>
    public class ParenthesExpression : IExpression
    {
      #region �����������

      /// <summary>
      /// ������� ��������� "������� ������"
      /// </summary>
      /// <param name="openToken">������� "("</param>
      /// <param name="closeToken">������� ")"</param>
      /// <param name="expression">��������� � �������</param>
      public ParenthesExpression(Token openToken, Token closeToken, IExpression expression)
      {
        if (openToken == null)
          throw new ArgumentNullException("openToken");
        if (closeToken == null)
          throw new ArgumentNullException("closeToken");
        if (expression == null)
          throw new ArgumentNullException("expression");

        _OpenToken = openToken;
        _CloseToken = closeToken;
        _Expression = expression;
      }

      #endregion

      #region ��������

      /// <summary>
      /// ������� ����������� ������
      /// </summary>
      public Token OpenToken { get { return _OpenToken; } }
      private Token _OpenToken;

      /// <summary>
      /// ������� ����������� ������
      /// </summary>
      public Token CloseToken { get { return _CloseToken; } }
      private Token _CloseToken;

      /// <summary>
      /// ��������� � �������
      /// </summary>
      public IExpression Expression { get { return _Expression; } }
      private IExpression _Expression;

      /// <summary>
      /// ���� �������� ���������� � true, �� ��� ��������� ���������� ������������� ����� ��������
      /// ������ ����� ��� Expression, � ������ ��������� �� �����. �� ���� ������ ������ ����� ��������������
      /// </summary>
      public bool IgnoreParenthes { get { return _IgnoreParenthes; } set { _IgnoreParenthes = value; } }
      private bool _IgnoreParenthes;

      #endregion

      #region IExpression Members

      /// <summary>
      /// ��������� ��������� � �������
      /// </summary>
      /// <returns>��������� ���������� ���������</returns>
      public object Calc()
      {
        return _Expression.Calc();
      }

      /// <summary>
      /// ���������� true, ���� ��������� �������� ����������
      /// </summary>
      public bool IsConst
      {
        get
        {
          return _Expression.IsConst;
        }
      }

      /// <summary>
      /// ��������� � ������ ������� "(" � ")"
      /// </summary>
      /// <param name="tokens">������ ��� ����������</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_OpenToken);
        tokens.Add(_CloseToken);
      }

      /// <summary>
      /// ��������� � ������ ��������� � �������
      /// </summary>
      /// <param name="expressions">������ ��� ����������</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        expressions.Add(_Expression);
      }

      /// <summary>
      /// ����������� ���������
      /// </summary>
      /// <param name="data">����������� ������</param>
      public void Synthesize(SynthesisData data)
      {
        if (!IgnoreParenthes)
          data.Tokens.Add(new SynthesisToken(data, this, "("));
        _Expression.Synthesize(data);
        if (!IgnoreParenthes)
          data.Tokens.Add(new SynthesisToken(data, this, ")"));
      }

      #endregion

      #region ��������� �������������

      /// <summary>
      /// ���������� "()"
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return "()";
        //return "(" + FExpression.ToString() + ")";
      }

      #endregion
    }

    #endregion

    #region IParser Members

    #region Parse

    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="data">������ ��������</param>
    public void Parse(ParsingData data)
    {
      if (data.GetChar(data.CurrPos) == '(')
      {
        data.Tokens.Add(new Token(data, this, "(", data.CurrPos, 1));
        return;
      }
      if (data.GetChar(data.CurrPos) == ')')
      {
        data.Tokens.Add(new Token(data, this, ")", data.CurrPos, 1));
        return;
      }

      // �� ������ ����� ������� �� �����, ����� �������� �������� ��� �������

      // �������� �������� 
      foreach (BinaryOpDef Def in BinaryOps)
      {
        if (data.StartsWith(Def.Op, false))
        {
          data.Tokens.Add(new Token(data, this, Def.Op, data.CurrPos, Def.Op.Length));
          return;
        }
      }

      // ������� �������� 
      foreach (UnaryOpDef Def in UnaryOps)
      {
        if (data.StartsWith(Def.Op, false))
        {
          data.Tokens.Add(new Token(data, this, Def.Op, data.CurrPos, Def.Op.Length));
          return;
        }
      }
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// ��������� ����������� ���������
    /// </summary>
    /// <param name="data">������ ��������</param>
    /// <param name="leftExpression">�������������� ���������</param>
    /// <returns>���������� ��������� ��� null � ������ ������</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token OpToken = data.CurrToken; // ����� ������ ������� ��������� ������ ���������
      data.SkipToken(); // ���������� ���� ��������

      if (OpToken.TokenType == "(")
        return CreateParenthesExpression(data, leftExpression);
      if (OpToken.TokenType == ")")
      {
        OpToken.SetError("�������� ����������� ������");
        return null;
      }

      // TODO: 01.03.2017 ������������ ���������� ��� ������������� EndTokens

      // 07.09.2015 �������, ������� ����� ��������� ������ ����� ���������.
      // ��������, ��� ��������� "a+b*c" ������ ���������� ����� "b*c"?
      // � ��� "a+b-c" ����� "b", � "-�" ����������� ��������
      string[] EndTokens;
      switch (OpToken.TokenType)
      {
        case "+":
        case "-":
          EndTokens = new string[] { "+", "-" };
          break;
        case "*":
        case "/":
          EndTokens = new string[] { "*", "/", "+", "-" };
          break;
        case "=":
        case "<>":
        case "<":
        case ">":
        case "<=":
        case ">=":
          EndTokens = new string[] { "=", "<>", "<", ">", "<=", ">=" }; // 22.03.2016 ???
          break;
        default:
          throw new BugException("����������� ���� �������� \"" + OpToken.TokenType + "\"");
      }

      if (data.EndTokens != null)
        EndTokens = DataTools.MergeArrays<string>(EndTokens, data.EndTokens);
      IExpression RightExpession = data.Parsers.CreateSubExpression(data, EndTokens); // ��������� ������� ���������
      if (RightExpession == null)
      {
        OpToken.SetError("�� ������ ������ ������� ��� �������� \"" + OpToken.TokenType + "\"");
        return null;
      }

      if (leftExpression == null)
      {
        // ���� ������ �������� ���, �� ����� ���� ������ ������� ��������
        if (!UnaryOps.Contains(OpToken.TokenType))
        {
          data.CurrToken.SetError("�� ������ ����� ������� ��� �������� \"" + OpToken.TokenType + "\". �������� �� ����� ���� �������");
          return null;
        }
        return new UnaryExpression(OpToken, RightExpession, UnaryOps[OpToken.TokenType].CalcMethod);
      }

      // ������������
      if (!BinaryOps.Contains(OpToken.TokenType))
      {
        data.CurrToken.SetError("�������� \"" + OpToken.TokenType + "\" �� ����� ���� ��������");
        return null;
      }


      BinaryExpression LeftExpression2 = leftExpression as BinaryExpression;
      if (LeftExpression2 != null)
      {
        int LeftPriority = GetPriority(LeftExpression2.Op);
        int CurrPriority = GetPriority(OpToken.TokenType);
        if (CurrPriority > LeftPriority)
        {
          // ������� �������� ("*") ����� ������� ���������, ��� ���������� ("+")
          // ��������� ������

          // ������� ��������
          BinaryExpression Expr2 = new BinaryExpression(OpToken, LeftExpression2.RightExpression, RightExpession, BinaryOps[OpToken.TokenType].CalcMethod);

          return new BinaryExpression(LeftExpression2.OpToken, LeftExpression2.LeftExpression, Expr2, LeftExpression2.CalcMethod);
        }
      }

      // ������� ������� ��������
      return new BinaryExpression(OpToken, leftExpression, RightExpession, BinaryOps[OpToken.TokenType].CalcMethod);
    }

    /// <summary>
    /// �������� ��������� ��� ��������
    /// </summary>
    /// <param name="op"></param>
    /// <returns></returns>
    private int GetPriority(string op)
    {
      int p = BinaryOps.IndexOf(op);
      if (p < 0)
        throw new ArgumentException("�������� \"" + op + "\" ��� � ������ �������� ��������", "op");

      return BinaryOps[p].Priority;
    }


    private IExpression CreateParenthesExpression(ParsingData data, IExpression leftExpression)
    {
      Token OpenToken = data.Tokens[data.CurrTokenIndex - 1];
      //Data.SkipToken(); ���� ��������� � ���������� ������

      if (leftExpression != null)
      {
        OpenToken.SetError("����� ����������� ������� ������ ���� ��������");
        // ? ����� ���������� �����
      }

      IExpression Expr = data.Parsers.CreateSubExpression(data, new string[] { ")" });
      if (Expr == null)
      {
        OpenToken.SetError("��������� � ������� �� ������");
        return null;
      }

      if (data.CurrTokenType == ")")
      {
        Token CloseToken = data.CurrToken;
        data.SkipToken();
        return new ParenthesExpression(OpenToken, CloseToken, Expr);
      }

      OpenToken.SetError("�� ������� ������ ����������� ������");
      return null;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// ������ ��� ������� �������� ��������
  /// ������� ������� (1e3) �� ��������������.
  /// ������� ������� "Const"
  /// </summary>
  public class NumConstParser : IParser
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    public NumConstParser()
    {
      _NumberFormat = CultureInfo.CurrentCulture.NumberFormat;
      _ReplaceChars = new Dictionary<string, string>();
      _ValidChars = null;

      AllowInt = true;
      AllowSingle = true;
      AllowDouble = true;
      AllowDecimal = true;
    }

    #endregion

    #region ��������, ����������� ���������

    /// <summary>
    /// �������������.
    /// �� ��������� ������������ CultureInfo.CurrentCulture.NumberFormat
    /// </summary>
    public NumberFormatInfo NumberFormat
    {
      get { return _NumberFormat; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _NumberFormat = value;
      }
    }
    private NumberFormatInfo _NumberFormat;

    /// <summary>
    /// ������� ������. ��������, ���� ��������� ������ ����������� � �������� ���������� ����� ��������
    /// "," � ".", ����� ������ FormatProvider=StdConvert.NumberFormat � �������� ���� (",":".") � ReplaceChars
    /// �� ���������, ������ ����� ������
    /// </summary>
    public Dictionary<string, string> ReplaceChars { get { return _ReplaceChars; } }
    private Dictionary<string, string> _ReplaceChars;

    /// <summary>
    /// ���� true (�� ���������), ����������� ��������� �������� ���� Int32
    /// </summary>
    public bool AllowInt { get { return _AllowInt; } set { _AllowInt = value; } }
    private bool _AllowInt;

    /// <summary>
    /// ���� true (�� ���������), ����������� ��������� �������� ���� Single
    /// </summary>
    public bool AllowSingle { get { return _AllowSingle; } set { _AllowSingle = value; } }
    private bool _AllowSingle;

    /// <summary>
    /// ���� true (�� ���������), ����������� ��������� �������� ���� Double
    /// </summary>
    public bool AllowDouble { get { return _AllowDouble; } set { _AllowDouble = value; } }
    private bool _AllowDouble;

    /// <summary>
    /// ���� true (�� ���������), ����������� ��������� �������� ���� Decimal
    /// </summary>
    public bool AllowDecimal { get { return _AllowDecimal; } set { _AllowDecimal = value; } }
    private bool _AllowDecimal;

    #endregion

    #region ������ ���������� ��������

    private string _ValidChars;

    private string GetValidChars()
    {
      // ������� ����� ����������� ����������, ������� ���������� ����������

      lock (_ReplaceChars)
      {
        if (_ValidChars == null)
        {
          SingleScopeList<char> Chars = new SingleScopeList<char>();
          for (char ch = '0'; ch <= '9'; ch++)
            Chars.Add(ch);

          foreach (KeyValuePair<string, string> Pair in _ReplaceChars)
          {
            if (Pair.Value.Length != Pair.Key.Length)
              throw new InvalidOperationException("������ ������������ ����������� \"" + Pair.Key + "\" -> \"" + Pair.Value +
                "\". �������� ����� � ������ ������ ����� ���������� �����");

            for (int j = 0; j < Pair.Key.Length; j++)
              Chars.Add(Pair.Key[j]);
          }

          for (int j = 0; j < _NumberFormat.NegativeSign.Length; j++)
            Chars.Add(_NumberFormat.NegativeSign[j]);

          for (int j = 0; j < _NumberFormat.NumberDecimalSeparator.Length; j++)
            Chars.Add(_NumberFormat.NumberDecimalSeparator[j]);

          _ValidChars = new string(Chars.ToArray());
        }
        return _ValidChars;
      }
    }

    #endregion

    #region Parse

    /// <summary>
    /// ��������� �������
    /// </summary>
    /// <param name="data">����������� ������</param>
    public void Parse(ParsingData data)
    {
      #region ������� ��������� ������� ����� �� ���������� ��������

      string ValidChars = GetValidChars();
      int len = 0;
      for (int p = data.CurrPos; p < data.Text.Text.Length; p++)
      {
        if (ValidChars.IndexOf(data.GetChar(p)) >= 0)
          len++;
        else
          break;
      }

      if (len == 0)
        return;

      string s = data.Text.Text.Substring(data.CurrPos, len);

      #endregion

      #region ����������� ��������

      foreach (KeyValuePair<string, string> Pair in _ReplaceChars)
        s = s.Replace(Pair.Key, Pair.Value);

      #endregion

      #region �������� ��������� ��������������

      NumberStyles ns = NumberStyles.AllowLeadingSign /*| ������ ! NumberStyles.AllowTrailingSign */| NumberStyles.AllowDecimalPoint;
      Token NewToken;

      for (int len2 = len; len2 > 0; len2--)
      {
        string s2 = s.Substring(0, len2);
        if (AllowInt)
        {
          int v;
          if (Int32.TryParse(s2, ns, NumberFormat, out v))
          {
            NewToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(NewToken);
            return;
          }
        }
        if (AllowSingle)
        {
          float v;
          if (Single.TryParse(s2, ns, NumberFormat, out v))
          {
            NewToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(NewToken);
            return;
          }
        }
        if (AllowDouble)
        {
          double v;
          if (Double.TryParse(s2, ns, NumberFormat, out v))
          {
            NewToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(NewToken);
            return;
          }
        }
        if (AllowDecimal)
        {
          decimal v;
          if (Decimal.TryParse(s2, ns, NumberFormat, out v))
          {
            NewToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(NewToken);
            return;
          }
        }
      }

      #endregion
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// ����������� ���������
    /// </summary>
    public class ConstExpression : IExpression
    {
      #region �����������

      /// <summary>
      /// ������� ���������
      /// </summary>
      /// <param name="value">�������� ���������</param>
      /// <param name="token">�������</param>
      public ConstExpression(object value, Token token)
      {
        //if (Token == null)
        //  throw new ArgumentNullException("Token");

        _Value = value;
        _Token = token;
      }

      /// <summary>
      /// ������� ��������� ��� �������.
      /// ��� ������ ������������ ����������� ������ ��� ����������� ��������� ��������, ����� ����������
      /// ������� ���������.
      /// </summary>
      /// <param name="value">�������� ���������</param>
      public ConstExpression(object value)
      {
        _Value = value;
        _Token = null;
      }

      #endregion

      #region ��������

      /// <summary>
      /// ���������
      /// </summary>
      public object Value { get { return _Value; } }
      private object _Value;

      /// <summary>
      /// �������, ������ ����� ���������. ����� ���� null
      /// </summary>
      public Token Token { get { return _Token; } }
      private Token _Token;

      #endregion

      #region IExpression Members

      /// <summary>
      /// ���������� Value
      /// </summary>
      /// <returns></returns>
      public object Calc()
      {
        return _Value;
      }

      /// <summary>
      /// ���������� true - ������� ������������ ���������.
      /// </summary>
      public bool IsConst
      {
        get
        {
          return true;
        }
      }

      /// <summary>
      /// ��������� ������� � ������, ���� ��� ������
      /// </summary>
      /// <param name="tokens">������ ��� ����������</param>
      public void GetTokens(IList<Token> tokens)
      {
        if (_Token != null)
          tokens.Add(_Token);
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      /// <param name="expressions">������ ��� ����������</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        // ��� �������� ���������
      }


      /// <summary>
      /// ����������� ���������
      /// </summary>
      /// <param name="data">����������� ������</param>
      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, "Const", data.CreateValueText(_Value), _Value));
      }
      #endregion

      #region ��������� �������������

      /// <summary>
      /// ���������� Value
      /// </summary>
      /// <returns>��������� �������������</returns>
      public override string ToString()
      {
        return _Value.ToString();
      }

      #endregion
    }

    /// <summary>
    /// ���������� ��������� ConstExpression
    /// </summary>
    /// <param name="data">������ ���������</param>
    /// <param name="leftExpression">������ ���� null</param>
    /// <returns>���������</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token CurrToken = data.CurrToken;
      data.SkipToken();
      if (leftExpression != null)
      {
        CurrToken.SetError("��������� �� ������ ���� ��������������� ����� ������� ���������. ��������� ��������");
        // ? ����� ���������� ������
      }

      return new ConstExpression(CurrToken.AuxData, CurrToken);
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ������� ��������� ��������, ����������� � ������� ��� ���������.
  /// ����� ������-������������ ������ � ������, �� ������ ���� �������.
  /// ������� ������� "String"
  /// </summary>
  public class StrConstParser : IParser
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ��� ����� � ������� ��������
    /// </summary>
    public StrConstParser()
      : this('\"')
    {
    }

    /// <summary>
    /// ������� ��������� ��� �����, ������������ ��������� ��������.
    /// </summary>
    /// <param name="separator">������ ������/��������� ������ (������� ��� ��������)</param>
    public StrConstParser(char separator)
    {
      _Separator = separator;
    }

    #endregion

    #region ��������, ����������� ���������

    /// <summary>
    /// ������������ ������.
    /// �������� � ������������
    /// </summary>
    public char Separator { get { return _Separator; } }
    private char _Separator;

    #endregion

    #region Parse

    /// <summary>
    /// ��������� �������
    /// </summary>
    /// <param name="data">������ ��������</param>
    public void Parse(ParsingData data)
    {
      if (data.GetChar(data.CurrPos) != Separator)
        return;

      StringBuilder sb = new StringBuilder();
      int len = 1; // ���� �������
      for (int p = data.CurrPos + 1; p < data.Text.Text.Length; p++)
      {
        len++;
        if (data.GetChar(p) == Separator)
        {
          if (data.GetChar(p + 1) == Separator)
          {
            // ��� ������� ������ - ��� ����� ������
            p++;
            len++;
            sb.Append(Separator);
          }
          else
          {
            // ����� ������
            data.Tokens.Add(new Token(data, this, "String", data.CurrPos, len, sb.ToString()));
            return;
          }
        }
        else
          // ������� ������ ������ ������
          sb.Append(data.GetChar(p));
      }

      // ������ �� ���������
      data.Tokens.Add(new Token(data, this, "String", data.CurrPos, len, sb.ToString(), new ErrorMessageItem(ErrorMessageKind.Error, "�� ������ ������ ���������� ������ (" + Separator + ")")));
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// ��������� ���������
    /// </summary>
    public class StringExpression : IExpression
    {
      #region �����������

      /// <summary>
      /// ������� ����������� ���������
      /// </summary>
      /// <param name="value">���������</param>
      /// <param name="token">�������</param>
      /// <param name="separator">������ �����������, ������, �������</param>
      public StringExpression(string value, Token token, char separator)
      {
        if (token == null)
          throw new ArgumentNullException("token");

        _Value = value;
        _Token = token;
        _Separator = separator;
      }

      #endregion

      #region ��������

      /// <summary>
      /// ��������� ��������� (��� �������)
      /// </summary>
      public string Value { get { return _Value; } }
      private string _Value;

      /// <summary>
      /// �������
      /// </summary>
      public Token Token { get { return _Token; } }
      private Token _Token;

      /// <summary>
      /// �� �����
      /// </summary>
      public char Separator { get { return _Separator; } }
      private char _Separator;

      #endregion

      #region IExpression Members

      /// <summary>
      /// ���������� Value
      /// </summary>
      /// <returns></returns>
      public object Calc()
      {
        return _Value;
      }

      /// <summary>
      /// ���������� true - ������� ������������ ���������
      /// </summary>
      public bool IsConst
      {
        get
        {
          return true;
        }
      }

      /// <summary>
      /// ��������� ������� � ������
      /// </summary>
      /// <param name="tokens">����������� ������</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_Token);
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      /// <param name="expressions">����������� ������</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        // ��� �������� ���������
      }


      /// <summary>
      /// ����������� ���������
      /// </summary>
      /// <param name="data">����������� ������</param>
      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, "String", data.CreateValueText(_Value), _Value));
      }

      #endregion

      #region ��������� �������������

      /// <summary>
      /// ���������� Value (��� �������)
      /// </summary>
      /// <returns>��������� �������������</returns>
      public override string ToString()
      {
        return _Value.ToString();
      }

      #endregion
    }

    /// <summary>
    /// ���������� StringExpression
    /// </summary>
    /// <param name="data">������ ��������</param>
    /// <param name="leftExpression">������ ���� null</param>
    /// <returns>���������</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token CurrToken = data.CurrToken;
      data.SkipToken();
      if (leftExpression != null)
      {
        CurrToken.SetError("��������� �� ������ ���� ��������������� ����� ������� ���������. ��������� ��������");
        // ? ����� ���������� ������
      }

      return new StringExpression((string)(CurrToken.AuxData), CurrToken, Separator);
    }

    #endregion
  }

  #region FunctionArgExpressionsCreatedEventHandler

  /// <summary>
  /// ������� ��� ���������� �������
  /// </summary>
  /// <param name="name">���������������� ��� �������. �������� ��������� ������������ ���� ���������� ��� ��������� �������, 
  /// ���� ��� �������</param>
  /// <param name="args">����������� ���������</param>
  /// <param name="userData">������������ ���������������� ������, ����������� � CreateExpression</param>
  /// <returns>��������� ���������� �������</returns>
  public delegate object FunctionDelegate(string name, object[] args, NamedValues userData);

  /// <summary>
  /// ��������� ������� FunctionDef.ArgExpressionsCreated
  /// </summary>
  public class FunctionArgExpressionsCreatedEventArgs : EventArgs
  {
    #region ��������

    /// <summary>
    /// ��������� ��� ���������� �������
    /// </summary>
    public IExpression[] ArgExpressions { get { return _ArgExpressions; } set { _ArgExpressions = value; } }
    private IExpression[] _ArgExpressions;

    #endregion
  }

  /// <summary>
  /// ������� ������� FunctionDef.ArgExpressionsCreated
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void FunctionArgExpressionsCreatedEventHandler (object sender, FunctionArgExpressionsCreatedEventArgs args);

  #endregion

  /// <summary>
  /// �������� ����� �������
  /// </summary>
  public class FunctionDef : ObjectWithCode
  {
    #region �����������

    /// <summary>
    /// ������� �������� �������
    /// </summary>
    /// <param name="name">��� �������</param>
    /// <param name="calcMethod">����������� �����</param>
    /// <param name="argCount">���������� ����������. ���� ������� ����� ��������� ���������� ����� ����������,
    /// ������� ����� ������������ ����������, � ����� ���������� MinArgCount</param>
    public FunctionDef(string name, FunctionDelegate calcMethod, int argCount)
      : base(name)
    {
      _CalcMethod = calcMethod;
      _MinArgCount = argCount;
      _MaxArgCount = argCount;
      _LocalName = String.Empty;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� �������, ��������, "SIN".
    /// �� ������������. ���� ��������� �����������, �� ������������ �������� LocalName
    /// </summary>
    public string Name { get { return base.Code; } }

    /// <summary>
    /// �������������� ���.
    /// �������� �� �������� ������������.
    /// �� ��������� �������� ������ ������
    /// </summary>
    public string LocalName
    {
      get { return _LocalName; }
      set
      {
        if (value == null)
          value = String.Empty;
        _LocalName = value;
      }
    }
    private string _LocalName;

    /// <summary>
    /// �����, ������������ ��� ���������� �������
    /// </summary>
    public FunctionDelegate CalcMethod { get { return _CalcMethod; } }
    private FunctionDelegate _CalcMethod;

    /// <summary>
    /// ���������� ���������� ����� ����������
    /// </summary>
    public int MinArgCount
    {
      get { return _MinArgCount; }
      set { _MinArgCount = value; }
    }
    private int _MinArgCount;

    /// <summary>
    /// ����������� ���������� ����� ����������
    /// </summary>
    public int MaxArgCount
    {
      get { return _MaxArgCount; }
      set { _MaxArgCount = value; }
    }
    private int _MaxArgCount;

    /// <summary>
    /// �������� �� ������� �������������������.
    /// ���� false (�� ���������), �� ��������� ������� ���������� ������� �� �� ����������. ����������� ���
    /// ����������� �������, ��������, "SIN".
    /// ���� ������� ����� ���������� ��������� �������� ��� ����� � ��� �� ����������, �������� "TODAY", ��
    /// �������� ������ ���� ����������� � true
    /// </summary>
    public bool IsVolatile
    {
      get { return _IsVolatile; }
      set { _IsVolatile = value; }
    }
    private bool _IsVolatile;

    /// <summary>
    /// ���������� LocalName
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(LocalName))
        return Name;
      else
        return LocalName; // 27.12.2020
    }

    #endregion

    #region �������

    /// <summary>
    /// ������� ���������� �� ������ ������ ��������, ����� ��������� ��������� ��� ���� ���������� �������,
    /// �� ��� �� ��������� ���� ��������� FunctionParser.FunctionExpression
    /// </summary>
    public event FunctionArgExpressionsCreatedEventHandler ArgExpressionsCreated;

    internal void OnArgExpressionsCreated(ref IExpression[] argExpressions)
    {
      if (ArgExpressionsCreated != null)
      {
        FunctionArgExpressionsCreatedEventArgs args = new FunctionArgExpressionsCreatedEventArgs();
        args.ArgExpressions = argExpressions;
        ArgExpressionsCreated(this, args);
        argExpressions = args.ArgExpressions;
      }
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ������� �������.
  /// ���������� ��� ������� (�������� ������), ������� ������ � ����������� ���������� ������� 
  /// (������ ������������ ����� ������)
  /// </summary>
  public class FunctionParser : IParser
  {
    #region ���������

    /// <summary>
    /// ������� "��� �������"
    /// </summary>
    public const string TokenName = "FunctionName";

    /// <summary>
    /// ������� "("
    /// </summary>
    public const string TokenOpen = "FunctionOpen";

    /// <summary>
    /// ������� ")"
    /// </summary>
    public const string TokenClose = "FunctionClose";

    /// <summary>
    /// ������� ","
    /// </summary>
    public const string TokenArgSep = "FunctionArgSep";

    #endregion

    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    public FunctionParser()
    {
      _ArgSeparators = new List<string>();
      _ArgSeparators.Add(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
      _Functions = new NamedList<FunctionDef>();
      _UseBothNames = true;

      _ValidNameChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      _InvalidFirstChars = "0123456789";
      _CaseSensitive = false;
    }

    #endregion

    #region ������ �������

    /// <summary>
    /// ������ �������
    /// ����������� ����� ����������� �������. ��� ���������� ������� Excel ����� ������������
    /// ����� ExcelFunctions.AddFunctions()
    /// </summary>
    public NamedList<FunctionDef> Functions { get { return _Functions; } }
    private NamedList<FunctionDef> _Functions;

    /// <summary>
    /// �������� ��������� ����� �������.
    /// ������������� ����������� ������ ������� Functions � ���� ��������������� ���� � ���������
    /// <paramref name="localNames"/>. ���� ���� ������, �� ��� ������� ��������������� �������� LocalName, ������ �������� ���������.
    /// ��������� <paramref name="localNames"/> ����� ��������� �����, ��� ������� ��� ������� � ������ Functions
    /// </summary>
    /// <param name="localNames">�������������� �����. ������ �������� ���������� ���, ��������, "TODAY", � ��������� - ��������������, �������� "�������"</param>
    public void SetLocalNames(IDictionary<string, string> localNames)
    {
      for (int i = 0; i < Functions.Count; i++)
      {
        string LocalName;
        if (localNames.TryGetValue(Functions[i].Name, out LocalName))
          Functions[i].LocalName = LocalName;
      }
    }

    /// <summary>
    /// �������� ��������� ����� �������.
    /// ������������� ������ �������� ���������� ���� <paramref name="names"/> � ������� ��������������� ����������� �������
    /// � ������ Functions. ���� �������, ������������� �������� FunctionDef.LocalName ������ �������� ������� <paramref name="localNames"/>
    /// ������<paramref name="names"/> ����� ��������� ����� �������, ������� ��� � ������ Functions
    /// </summary>
    /// <param name="names">���������� �����</param>
    /// <param name="localNames">�������������� �����</param>
    public void SetLocalNames(string[] names, string[] localNames)
    {
      if (localNames.Length != names.Length)
        throw new ArgumentException("����� �������� �� ���������", "localNames");

      for (int i = 0; i < names.Length; i++)
      {
        FunctionDef fd = Functions[names[i]];
        if (fd != null)
          fd.LocalName = localNames[i];
      }
    }

    #endregion

    #region ��������, ����������� ���������

    /// <summary>
    /// �������, ������� ����� ����������� � ����� �������
    /// �� ��������� �������� ��������� ��������� ����� � �����
    /// ���� ������������ �����������, �� ��������� �������� ������� � ������.
    /// ����� ����� �������� ����������� �������, ��������, ���� "_"
    /// ���� �������� CaseSensitive ����������� � true, �� ��������� ����� �������� ������� �������� ��������
    /// </summary>
    public string ValidNameChars
    {
      get { return _ValidNameChars; }
      set { _ValidNameChars = value; }
    }
    private string _ValidNameChars;

    /// <summary>
    /// ������� �� ValidNameChars, ������� �� ����� ���� ������� � ����� �������/
    /// �� ��������� �������� ����� �� 0 �� 9
    /// </summary>
    public string InvalidFirstChars
    {
      get { return _InvalidFirstChars; }
      set { _InvalidFirstChars = value; }
    }
    private string _InvalidFirstChars;

    /// <summary>
    /// ���� �������� ����������� � true, �� ����� ������� ������������� � ��������.
    /// �� ��������� - false
    /// </summary>
    public bool CaseSensitive
    {
      get { return _CaseSensitive; }
      set { _CaseSensitive = value; }
    }
    private bool _CaseSensitive;


    /// <summary>
    /// ����������� ������ ����������. �� ��������� �������� ���� �����������, �������� �
    /// CultureInfo.CurrentCulture.TextInfo.ListSeparator.
    /// ��� �������������, ������ ����� ���� ������ � �������� ������ ���������
    /// </summary>
    public List<string> ArgSeparators { get { return _ArgSeparators; } }
    private List<string> _ArgSeparators;

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� � ����������� ���������� ��������� ������������
    /// ��� ��������� ����� (LocalName), ��� � ����������� (Name).
    /// ���� �������� �������� � false, �� ����� ������������ ������ ��������� ����� (LocalName), ����� �������, �
    /// ������� LocalName="". ��� ��������� ������������ Name
    /// </summary>
    public bool UseBothNames
    {
      get { return _UseBothNames; }
      set { _UseBothNames = value; }
    }
    private bool _UseBothNames;

    #endregion

    #region ������� ����

    private Dictionary<string, FunctionDef> _NameDict;

    private Dictionary<string, FunctionDef> GetNameDict()
    {
      lock (_Functions)
      {
        if (_NameDict == null)
        {
          _NameDict = new Dictionary<string, FunctionDef>();
          foreach (FunctionDef fd in _Functions)
          {
            if (String.IsNullOrEmpty(fd.LocalName))
              _NameDict.Add(CaseSensitive ? fd.Name : fd.Name.ToUpperInvariant(), fd);
            else
            {
              _NameDict.Add(CaseSensitive ? fd.LocalName : fd.LocalName.ToUpperInvariant(), fd);
              if (UseBothNames)
                _NameDict.Add(CaseSensitive ? fd.Name : fd.Name.ToUpperInvariant(), fd);
            }
          }
        }
        return _NameDict;
      }
    }

    #endregion

    #region Parse

    /// <summary>
    /// ���������� �������:
    /// "Function" - �������� ��� �������. � �������� AuxData �������� ��� �������, ��� ��� ������ � ������.
    /// ������� ������������, ���� ���� �� ���������� ����������� ������
    /// "FunctionOpen" � "FunctionClose" ������ ������ "(" � ")". 
    /// ����������� ������� ������ �� ������������, ���� ��� ������� �� ���� ������.
    /// "FunctionSep" - ����������� ������ ����������. �������� ����������� (�����������), �������� � ������ ArgSeparators
    /// </summary>
    /// <param name="data"></param>
    public void Parse(ParsingData data)
    {
      Token NewToken;

      #region ������

      char ch = data.GetChar(data.CurrPos);

      // ������ ����� ���� ������ �������, � ����� ���� ������ ��������������� ��������� 

      if (ch == '(')
      {
        // ���������, ��� �� ����� ���� ���� �������
        for (int i = data.Tokens.Count - 1; i >= 0; i--)
        {
          switch (data.Tokens[i].TokenType)
          {
            case "Space":
            case "Comment":
              continue;
            case TokenName:
              NewToken = new Token(data, this, TokenOpen, data.CurrPos, 1);
              data.Tokens.Add(NewToken);
              return;
            default:
              return; // 09.02.2017 - �� ���� ������
          }
        }
        return;
      }

      if (ch == ')')
      {
        // ���������, ����������� ������
        int Counter = 1; // ���� ������
        for (int j = data.Tokens.Count - 1; j >= 0; j--)
        {
          switch (data.Tokens[j].TokenType)
          {
            case TokenClose:
              Counter++;
              break;
            case TokenOpen:
              Counter--;
              if (Counter == 0)
              {
                if (data.Tokens[j].Parser == this)
                {
                  // ����������� ������ ����
                  NewToken = new Token(data, this, TokenClose, data.CurrPos, 1);
                  data.Tokens.Add(NewToken);
                }
                return;
              }
              break;
          }
        }
        return;
      }

      #endregion

      #region ����������� ������ ����������

      for (int i = 0; i < ArgSeparators.Count; i++)
      {
        if (data.StartsWith(ArgSeparators[i], false))
        {
          // 17.12.2018. ���������, ����������� ������
          int Counter = 1; // ���� ������
          for (int j = data.Tokens.Count - 1; j >= 0; j--)
          {
            switch (data.Tokens[j].TokenType)
            {
              case TokenClose:
                Counter++;
                break;
              case TokenOpen:
                Counter--;
                if (Counter == 0)
                {
                  if (data.Tokens[j].Parser == this)
                  {
                    // ����������� ������ ����
                    NewToken = new Token(data, this, TokenArgSep, data.CurrPos, ArgSeparators[i].Length);
                    data.Tokens.Add(NewToken);
                  }
                  return;
                }
                break;
            }
          }
        }
      }

      #endregion

      #region ��� �������

      StringComparison sc = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

      string s1 = new string(ch, 1);

      if (ValidNameChars.IndexOf(s1, sc) < 0)
        return;
      if (InvalidFirstChars.IndexOf(s1, sc) >= 0)
        return;

      // ������ ����� ������� �������
      int len = 1;
      for (int p = data.CurrPos + 1; p < data.Text.Text.Length; p++)
      {
        ch = data.GetChar(p);
        s1 = new string(ch, 1);
        if (ValidNameChars.IndexOf(s1, sc) < 0)
          break;
        else
          len++;
      }

      string FuncName = data.Text.Text.Substring(data.CurrPos, len);
      NewToken = new Token(data, this, TokenName, data.CurrPos, len, FuncName);
      data.Tokens.Add(NewToken);

      #endregion
    }

    #endregion

    #region ����������� �������

    /// <summary>
    /// ��������� ��� ���������� �������.
    /// ���� ������� ����� ���������, ������� ����������� �� ������� ��� ��������� ����� �������.
    /// ����� ����������� �������.
    /// </summary>
    public class FunctionExpression : IExpression
    {
      #region �����������

      /// <summary>
      /// ������� ������ ���������
      /// </summary>
      /// <param name="function">�������� �������</param>
      /// <param name="args">��������� ��� ���������� �������</param>
      /// <param name="nameToken">������� ��� ����� �������</param>
      /// <param name="openToken">������� ��� ����������� ������</param>
      /// <param name="closeToken">������� ��� ����������� ������</param>
      /// <param name="argSepTokens">������ ������ ��� ������������ ���������� (����� � �������).
      /// ���������� ��������� � ������� ����� ���� �� ���� ������, ��� � ������� <paramref name="args"/>.</param>
      /// <param name="userData">������������ ���������������� ������, ������� ����� �������� ����������� �������</param>
      public FunctionExpression(FunctionDef function, IExpression[] args, Token nameToken, Token openToken, Token closeToken, Token[] argSepTokens, NamedValues userData)
      {
        if (function == null)
          throw new ArgumentNullException("function");
        if (args == null)
          throw new ArgumentNullException("args");
        for (int i = 0; i < args.Length; i++)
        {
          if (args[i] == null)
            throw new ArgumentNullException("args[" + i.ToString() + "]");
        }
        if (nameToken == null)
          throw new ArgumentNullException("nameToken");
        if (openToken == null)
          throw new ArgumentNullException("openToken");
        if (closeToken == null)
          throw new ArgumentNullException("closeToken");
        if (argSepTokens == null)
          throw new ArgumentNullException("argSepTokens");
        for (int i = 0; i < argSepTokens.Length; i++)
        {
          if (argSepTokens[i] == null)
            throw new ArgumentNullException("argSepTokens[" + i.ToString() + "]");
        }

        _Function = function;
        _Args = args;
        _NameToken = nameToken;
        _OpenToken = openToken;
        _CloseToken = closeToken;
        _ArgSepTokens = argSepTokens;
        _UserData = userData;
      }

      #endregion

      #region ��������

      /// <summary>
      /// ����������� �������
      /// </summary>
      public FunctionDef Function { get { return _Function; } }
      private FunctionDef _Function;


      /// <summary>
      /// ����������� ���������
      /// </summary>
      public IExpression[] Args { get { return _Args; } }
      private IExpression[] _Args;

      /// <summary>
      /// ������� � ������ �������
      /// </summary>
      public Token NameToken { get { return _NameToken; } }
      private Token _NameToken;

      /// <summary>
      /// ������� "("
      /// </summary>
      public Token OpenToken { get { return _OpenToken; } }
      private Token _OpenToken;

      /// <summary>
      /// ������� ")"
      /// </summary>
      public Token CloseToken { get { return _CloseToken; } }
      private Token _CloseToken;

      /// <summary>
      /// ������� ","
      /// </summary>
      public Token[] ArgSepTokens { get { return _ArgSepTokens; } }
      private Token[] _ArgSepTokens;

      /// <summary>
      /// ���������������� ������, ���������� � CreateExpression
      /// </summary>
      public NamedValues UserData { get { return _UserData; } }
      private NamedValues _UserData;

      #endregion

      #region IExpression Members

      /// <summary>
      /// ��������� �������.
      /// </summary>
      /// <returns>��������� ����������</returns>
      public object Calc()
      {
        object[] ArgVals = new object[Args.Length];
        for (int i = 0; i < Args.Length; i++)
          ArgVals[i] = Args[i].Calc();

        return Function.CalcMethod(Function.Name, // �� ��������������
          ArgVals,
          UserData);
      }

      /// <summary>
      /// ���������� false ���� FunctionDef.IsVolatile=true.
      /// ����� ���������� true, ���� ������� �� ����� ���������� ��� ���� ��� ��������� �������� �����������.
      /// </summary>
      public bool IsConst
      {
        get
        {
          if (Function.IsVolatile)
            return false;

          for (int i = 0; i < Args.Length; i++)
          {
            if (!Args[i].IsConst)
              return false;
          }

          return true;
        }
      }

      /// <summary>
      /// ��������� � ������ ��� �������
      /// </summary>
      /// <param name="tokens">������ ��� ����������</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(NameToken);
        tokens.Add(OpenToken);
        for (int i = 0; i < ArgSepTokens.Length; i++)
          tokens.Add(ArgSepTokens[i]);
        tokens.Add(CloseToken);
      }

      /// <summary>
      /// ��������� � ������ ��� ��������� ��� ���������� ����������
      /// </summary>
      /// <param name="expressions">������ ��� ����������</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        for (int i = 0; i < Args.Length; i++)
          expressions.Add(Args[i]);
      }

      /// <summary>
      /// ������ ���������
      /// </summary>
      /// <param name="data"></param>
      public void Synthesize(SynthesisData data)
      {
        if (data.UseFormulas)
        {
          data.Tokens.Add(new SynthesisToken(data, this, TokenName, Function.Name, Function.Name));
          data.Tokens.Add(new SynthesisToken(data, this, TokenOpen, "("));
          for (int i = 0; i < Args.Length; i++)
          {
            if (i > 0)
            {
              data.Tokens.Add(new SynthesisToken(data, this, TokenArgSep, data.ListSeparator));
              if (data.UseSpaces)
                data.Tokens.Add(new SynthesisToken(data, this, "Space", " "));
            }
            Args[i].Synthesize(data);
          }
          data.Tokens.Add(new SynthesisToken(data, this, TokenClose, ")"));
        }
        else
        {
          // ��������� ���������
          data.Tokens.Add(new SynthesisToken(data, this, "Const", data.CreateValueText(Calc())));
        }
      }

      #endregion

      #region ToString()

      /// <summary>
      /// ���������� ��� ������� � ������
      /// </summary>
      /// <returns>��������� �������������</returns>
      public override string ToString()
      {
        return _Function.Name + "()";
      }

      #endregion
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// ������� ��������� FunctionExpression
    /// </summary>
    /// <param name="data">������ ��������</param>
    /// <param name="leftExpression">��������� �����. ������ ���� null</param>
    /// <returns>���������</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      switch (data.CurrTokenType)
      {
        case TokenName:
          Token CurrToken = data.CurrToken;
          data.SkipToken();

          if (leftExpression != null)
          {
            CurrToken.SetError("��� ������� �� ����� ���� ������������ ������� ���������. ��������� ��������");
            // ? ����� ����������
          }

          FunctionDef fd = GetFunction(CurrToken.AuxData.ToString());
          // ���� ������� ����������� �������
          Token OpenToken = null;
          while ((data.CurrTokenIndex < data.Tokens.Count) && (OpenToken == null))
          {
            switch (data.CurrTokenType)
            {
              case TokenOpen:
                OpenToken = data.CurrToken;
                data.SkipToken();
                break;

              case "Space":
              case "Comment":
                data.SkipToken();
                break;

              default:
                data.CurrToken.SetError("��������� ����������� ������ ����� ����� �������");
                return null;
            }
          }
          if (OpenToken == null)
          {
            CurrToken.SetError("�� ������� ����������� ������ ����� ����� �������");
            return null;
          }

          // ���������� �������� �������
          List<IExpression> ArgExprs = new List<IExpression>();
          Token CloseToken = null;
          List<Token> ArgSepTokens = new List<Token>();
          while (true)
          {
            IExpression ArgExpr = data.Parsers.CreateSubExpression(data, new string[] { TokenArgSep, TokenClose });
            if (ArgExpr == null)
            {
              if (data.CurrToken == null)
              {
                CurrToken.SetError("�� ������� ����������� ������ ��� �������");
                return null;
              }

              if (data.CurrTokenType == TokenClose)
              {
                CloseToken = data.CurrToken;
                data.SkipToken();
                break;
              }

              Token ErrorToken;
              if (ArgSepTokens.Count > 0)
                ErrorToken = ArgSepTokens[ArgSepTokens.Count - 1];
              else
                ErrorToken = OpenToken;

              ErrorToken.SetError("�������� ��������");
            }

            ArgExprs.Add(ArgExpr);
            if (data.CurrToken == null)
            {
              CurrToken.SetError("�� ������� ����������� ������ ��� �������");
              return null;
            }
            if (data.CurrTokenType == TokenClose)
            {
              CloseToken = data.CurrToken; // 03.12.2015
              data.SkipToken();
              break;
            }
            if (data.CurrTokenType != TokenArgSep)
            {
              string ErrorText = "��������� ����������� ������ ������ �������";
              if (ArgSepTokens.Count > 0)
              {
                ErrorText += " ��� ����������� ������ ���������� \"" + ArgSepTokens[0] + "\"";
                for (int i = 1; i < ArgSepTokens.Count; i++)
                  ErrorText += " ��� \"" + ArgSepTokens[i] + "\"";
              }
              data.CurrToken.SetError(ErrorText);
              return null;
            }

            ArgSepTokens.Add(data.CurrToken);
            data.SkipToken();
          }

          // ������ ���������� ��������. ������ ��������
          if (fd == null)
          {
            CurrToken.SetError("����������� ��� ������� \"" + CurrToken.AuxData.ToString() + "\"");
            return null;
          }

          IExpression[] ArgExprs2 = ArgExprs.ToArray();
          fd.OnArgExpressionsCreated(ref ArgExprs2);

          if (ArgExprs2.Length < fd.MinArgCount || ArgExprs2.Length > fd.MaxArgCount)
          {
            string ErrorText = "������������ ���������� ���������� ������� \"" + fd.ToString() + "\" (" + ArgExprs2.Length.ToString() + ")";
            if (fd.MaxArgCount == fd.MinArgCount)
              ErrorText += ". ��������� ����������: " + fd.MaxArgCount.ToString();
            else
              ErrorText += ". ��������� ����������: �� " + fd.MinArgCount.ToString() + " �� " + fd.MaxArgCount.ToString();
            CurrToken.SetError(ErrorText);
            return null;
          }


          FunctionExpression FuncExpr = new FunctionExpression(fd, ArgExprs2, CurrToken, OpenToken, CloseToken, ArgSepTokens.ToArray(), data.UserData);
          return FuncExpr;

        default:
          data.CurrToken.SetError("����������� ��������� \"" + data.CurrToken.Text + "\" ��� ������ �������");
          data.SkipToken();
          return null;
      }
    }

    /// <summary>
    /// ���������� �������� ������� �� ��������� ����� (��� ��� ������ � ���������)
    /// ������� ���������� ���������� �������� �� ������� Functions
    /// ���������������� ����� ����� ���������� ����������� ����������� ��������
    /// ����� ���������� null, ���� ��� ������� ����������
    /// </summary>
    /// <param name="name">�������������� ��� ���������������� ��� �������</param>
    /// <returns>�������� �������</returns>
    protected virtual FunctionDef GetFunction(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (!CaseSensitive)
        name = name.ToUpperInvariant();

      Dictionary<string, FunctionDef> Dict = GetNameDict();

      FunctionDef fd;
      if (Dict.TryGetValue(name, out fd))
        return fd;
      else
        return null;
    }

    #endregion
  }
}