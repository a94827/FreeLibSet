using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using AgeyevAV.Remoting;

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

namespace AgeyevAV.Parsing
{
  /// <summary>
  /// Метод, выполняющий бинарную операцию
  /// </summary>
  /// <param name="op">Знак операции</param>
  /// <param name="arg1">Левый аргумент</param>
  /// <param name="arg2">Правый аргумент</param>
  /// <returns></returns>
  public delegate object BinaryOpDelegate(string op, object arg1, object arg2);

  /// <summary>
  /// Описание бинарной операции
  /// Порядок добавления объектов MathOpDef в списки MathOpParser должен соответствовать
  /// понижению приоритета операций
  /// </summary>
  public class BinaryOpDef : ObjectWithCode
  {
    #region Константы приоритетов

    /// <summary>
    /// Не используется
    /// </summary>
    public const int PriorityPower = 500;

    /// <summary>
    /// Операции умножения и деления имеют высший приоритет
    /// </summary>
    public const int PriorityMulDiv = 400;

    /// <summary>
    /// Операции сложения и вычитания имеют меньший приоритет
    /// </summary>
    public const int PriorityAddSub = 300;

    /// <summary>
    /// Операции сравнения имеют низкий приритет
    /// </summary>
    public const int PriorityCompare = 200;

    /// <summary>
    /// Не используется
    /// </summary>
    public const int PriorityLogical = 100;

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает описание операции
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="calcMethod">Метод вычисления</param>
    /// <param name="priority">Приоритет операции. Чем больше значение, тем выше приоритет</param>
    public BinaryOpDef(string op, BinaryOpDelegate calcMethod, int priority)
      : base(op)
    {
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");
      _CalcMethod = calcMethod;
      _Priority = priority;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Знак операции.
    /// Задается в конструкторе.
    /// </summary>
    public string Op { get { return base.Code; } }

    /// <summary>
    /// Метод вычисления операции.
    /// Задается в конструкторе
    /// </summary>
    public BinaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private BinaryOpDelegate _CalcMethod;

    /// <summary>
    /// Приоритеты операций.
    /// Используются, когда несколько операций идут подряд без скобок
    /// Чем больше значение, тем выше приоритет операции.
    /// Например, приритет операции умножения больше, чем операции сложения.
    /// Для стандартных операций используются константы PriorityXXX
    /// </summary>
    public int Priority { get { return _Priority; } }
    private int _Priority;

    #endregion
  }

  /// <summary>
  /// Метод, выполняющий унарную операцию
  /// </summary>
  /// <param name="op">Знак операции</param>
  /// <param name="arg">Аргумент (справа от операции)</param>
  /// <returns></returns>
  public delegate object UnaryOpDelegate(string op, object arg);

  /// <summary>
  /// Описание унарной операции
  /// Порядок добавления объектов MathOpDef в списки MathOpParser должен соответствовать
  /// понижению приоритета операций
  /// </summary>
  public class UnaryOpDef : ObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описание операции
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="calcMethod">Метод для вычисления операции</param>
    public UnaryOpDef(string op, UnaryOpDelegate calcMethod)
      : base(op)
    {
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");
      _CalcMethod = calcMethod;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Знако операции.
    /// Задается в конструкторе.
    /// </summary>
    public string Op { get { return base.Code; } }

    /// <summary>
    /// Метод вычисления операции.
    /// Задается в конструторе.
    /// </summary>
    public UnaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private UnaryOpDelegate _CalcMethod;

    #endregion
  }

  /// <summary>
  /// Парсер для разбора арифметических операций.
  /// Создает лексемы, совпадающие с операциями: "+", "-", "*", "/", знаки сравнения, круглые скобки "()".
  /// </summary>
  public class MathOpParser : IParser
  {
    #region Конструкторы

    /// <summary>
    /// Создает парсер, распознающий стандартные арифметические и логические операции
    /// </summary>
    public MathOpParser()
      : this(true)
    {
    }

    /// <summary>
    /// Создает парсер, с возможностью добавления операций.
    /// </summary>
    /// <param name="useDefaultOps">Надо ли добавлять описания стандартных арифметических и логических операций.
    /// Если false, то описания всех операций должны быть добавлены вручную</param>
    public MathOpParser(bool useDefaultOps)
    {
      _BinaryOps = new NamedList<BinaryOpDef>();
      _UnaryOps = new NamedList<UnaryOpDef>();

      if (useDefaultOps)
      {
        BinaryOpDelegate BinaryD = new BinaryOpDelegate(BinaryCalc);
        UnaryOpDelegate UnaryD = new UnaryOpDelegate(UnaryCalc);

        // 10.10.2017
        // Сначала должны идти операции из двух символов, а затем - из одного,
        // иначе, например, операция ">=" будет распознана как ">" и "="

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
          BinaryOps.Add(new BinaryOpDef(BinarySigns[i], BinaryD, BinaryPriorities[i])); // один делегат на все

        string[] UnarySigns = new string[] { "+", "-" };

        for (int i = 0; i < UnarySigns.Length; i++)
          UnaryOps.Add(new UnaryOpDef(UnarySigns[i], UnaryD)); // один делегат на все
      }
    }

    #endregion

    #region Списки операций

    /// <summary>
    /// Список допустимых бинарных операций
    /// </summary>
    public NamedList<BinaryOpDef> BinaryOps { get { return _BinaryOps; } }
    private NamedList<BinaryOpDef> _BinaryOps;

    /// <summary>
    /// Список допустимых унарных операций
    /// </summary>
    public NamedList<UnaryOpDef> UnaryOps { get { return _UnaryOps; } }
    private NamedList<UnaryOpDef> _UnaryOps;

    #endregion

    #region Вычисление

    #region Бинарные операции

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
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
        // Тут все сложнее. Даты могут складываться с числами
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
        //throw new NotSupportedException("Математические операции над строками не поддерживаются. Используйте функции");
        return CalcString(op, DataTools.GetString(arg1), DataTools.GetString(arg2));
      if (arg1 is bool && arg2 is bool)
        return CalcBool(op, DataTools.GetBool(arg1), DataTools.GetBool(arg2));

      throw new NotSupportedException("Остальное не реализовано. Тип данных аргумента 1:" + arg1.GetType().ToString() + ", аргумента 2: " + arg2.GetType().ToString());
    }

    /// <summary>
    /// "Вычисляет" унарную операцию для двух аргументов null.
    /// Для операций сравнение, содержажих знак равенства, возвращает true.
    /// Для операций "больше", "меньше" и "не равно", возвращает false.
    /// Для арифметических операций возвращает null.
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
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована для аргументов null");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Decimal
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
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
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Double
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
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
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Single
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
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
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Int32
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcInt(string op, int arg1, int arg2)
    {
      // 01.03.2017
      // Сначала пытаемся вычислить для целочисленного аргумента
      int res1;
      try
      {
        checked // обязательно с контролем переполнения
        {
          switch (op)
          {
            case "+": return arg1 + arg2;
            case "-": return arg1 - arg2;
            case "*": return arg1 * arg2;
            case "/":
              res1 = arg1 / arg2; // тут может быть существенная потеря точности
              break;

            case "=": return arg1 == arg2;
            case "<>": return arg1 != arg2;
            case ">": return arg1 > arg2;
            case "<": return arg1 < arg2;
            case ">=": return arg1 >= arg2;
            case "<=": return arg1 <= arg2;
            default:
              throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
          }
        }
      }
      catch
      {
        // В случае переполнения вычисляем тоже самое для типа Double
        return CalcDouble(op, (double)arg1, (double)arg2);
      }

      // Для операции деления вычисляем тоже самое для типа Double
      double res2 = (double)CalcDouble(op, (double)arg1, (double)arg2);

      if (Math.Abs(res2) > (double)(Int32.MaxValue))
        return res2; // переполнение целого числа

      if (res2 == (double)res1)
        return res1; // нет потери точности
      else
        return res2; // есть потеря точности
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа String
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcString(string op, string arg1, string arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        default:
          throw new InvalidOperationException("Для строк применимы только операции \"+\", \"=\" и \"<>\". Операция \"" + op + "\" не реализована");
      }
    }

    private static object CalcBool(string Op, bool Arg1, bool Arg2)
    {
      switch (Op)
      {
        case "=": return Arg1 == Arg2;
        case "<>": return Arg1 != Arg2;
        default:
          throw new InvalidOperationException("Для логических значений применимы только операции \"=\" и \"<>\". Операция \"" + Op + "\" не применима");
      }
    }

    #region DateTime и TimeSpan

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа DateTime
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
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
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов DateTime");
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
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов DateTime");
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
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов TimeSpan");
      }
    }

    private static object CalcTimeSpanAndDateTime(string op, TimeSpan arg1, DateTime arg2)
    {
      throw new InvalidOperationException("Операции с аргументами TimeSpan и DateTime невозможны");
    }

    private static object CalcDateTimeAndTimeSpan(string op, DateTime arg1, TimeSpan arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов TimeSpan");
      }
    }

    private static object CalcTimeSpanAndDouble(string op, TimeSpan arg1, double arg2)
    {
      switch (op)
      {
        case "+": return arg1 + TimeSpan.FromDays(arg2);
        case "-": return arg1 - TimeSpan.FromDays(arg2);
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для аргументов TimeSpan и Double");
      }
    }


    #endregion

    #endregion

    #region Унарные операции

    /// <summary>
    /// Стандартное вычисление унарной операции
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg">Аргумент</param>
    /// <returns>Результат операции</returns>
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

      throw new NotSupportedException("Унарные операции не реализованы для типа " + arg.GetType().ToString());
    }

    private static object CalcDecimal(string op, decimal arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Decimal");
      }
    }

    private static object CalcDouble(string op, double arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Double");
      }
    }

    private static object CalcSingle(string op, float arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Single");
      }
    }

    private static object CalcInt(string op, int arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Int32");
      }
    }

    private static object CalcTimeSpan(string op, TimeSpan arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для TimeSpan");
      }
    }

    #endregion

    #endregion

    #region Классы-вычислители IExperession

    /// <summary>
    /// Бинарная операция "+", "-", "*", "/", операции сравнения
    /// </summary>
    public class BinaryExpression : IExpression
    {
      #region Конструктор

      /// <summary>
      /// Создает выражение для бинарной операции
      /// </summary>
      /// <param name="opToken">Лексема</param>
      /// <param name="leftExpression">Выражение слева от операции</param>
      /// <param name="rightExpression">Выражение справа от операции</param>
      /// <param name="calcMethod">Вычисляющий метод</param>
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

      #region Свойства

      /// <summary>
      /// Лексема
      /// </summary>
      public Token OpToken { get { return _OpToken; } }
      private Token _OpToken;

      /// <summary>
      /// Выражение слева от операции
      /// </summary>
      public IExpression LeftExpression { get { return _LeftExpression; } }
      private IExpression _LeftExpression;

      /// <summary>
      /// Выражение справа от операции
      /// </summary>
      public IExpression RightExpression { get { return _RightExpression; } }
      private IExpression _RightExpression;

      /// <summary>
      /// Вычисляющий метод
      /// </summary>
      public BinaryOpDelegate CalcMethod { get { return _CalcMethod; } }
      private BinaryOpDelegate _CalcMethod;

      /// <summary>
      /// Знак операции "+", "-", "*" или "/"
      /// </summary>
      public string Op { get { return OpToken.TokenType; } }

      #endregion

      #region IExpression Members

      /// <summary>
      /// Выполнить вычисление выражения.
      /// Вычисляюся левое и правое выражение, затем вычисляется CalcMethod для операции
      /// </summary>
      /// <returns>Результат вычислей</returns>
      public object Calc()
      {
        object v1 = _LeftExpression.Calc();
        object v2 = _RightExpression.Calc();
        return _CalcMethod(_OpToken.TokenType, v1, v2);
      }

      /// <summary>
      /// Возвращает true, если и левое и правое выражение являются константами
      /// </summary>
      public bool IsConst
      {
        get
        {
          return _LeftExpression.IsConst && _RightExpression.IsConst;
        }
      }

      /// <summary>
      /// Добавляет в список OpToken
      /// </summary>
      /// <param name="tokens">Список для заполнения</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_OpToken);
      }

      /// <summary>
      /// Добавляет в список левое и правое выражение
      /// </summary>
      /// <param name="expressions">Список для заполнения</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        expressions.Add(_LeftExpression);
        expressions.Add(_RightExpression);
      }


      /// <summary>
      /// Синтезирует выражение
      /// </summary>
      /// <param name="data">Заполняемый объект</param>
      public void Synthesize(SynthesisData data)
      {
        #region Необходимость определения скобок

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

      #region Текстовое представление

      /// <summary>
      /// Возвращает свойство Op
      /// </summary>
      /// <returns>Текстовое представление</returns>
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
    /// Унарная операция "+" (ничего не делает) и "-"
    /// </summary>
    public class UnaryExpression : IExpression
    {
      #region Конструктор

      /// <summary>
      /// Создает выражение
      /// </summary>
      /// <param name="opToken">Лексема операции</param>
      /// <param name="rightExpression">Выражение справа от операции. Не может быть null</param>
      /// <param name="calcMethod">Вычисляющий метод</param>
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

      #region Свойства

      /// <summary>
      /// Лексема унарной операции
      /// </summary>
      public Token OpToken { get { return _OpToken; } }
      private Token _OpToken;

      /// <summary>
      /// Выражение справа от знака операции
      /// </summary>
      public IExpression RightExpression { get { return _RightExpression; } }
      private IExpression _RightExpression;

      /// <summary>
      /// Вычисляющий метод
      /// </summary>
      public UnaryOpDelegate CalcMethod { get { return _CalcMethod; } }
      private UnaryOpDelegate _CalcMethod;

      /// <summary>
      /// Знак операции "+", "-"
      /// </summary>
      public string Op { get { return OpToken.TokenType; } }

      #endregion

      #region IExpression Members

      /// <summary>
      /// Вычисляет выражение справа, затем - унарную операцию
      /// </summary>
      /// <returns>Результат вычисления</returns>
      public object Calc()
      {
        object v2 = _RightExpression.Calc();
        return _CalcMethod(_OpToken.TokenType, v2);
      }

      /// <summary>
      /// Возвращает true, если выражение справа является константой
      /// </summary>
      public bool IsConst
      {
        get
        {
          return _RightExpression.IsConst;
        }
      }

      /// <summary>
      /// Добавляет лексему в список
      /// </summary>
      /// <param name="tokens">Список для заполнения</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_OpToken);
      }

      /// <summary>
      /// Добавляет выражение справа в список
      /// </summary>
      /// <param name="expressions">Список для заполнения</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        expressions.Add(_RightExpression);
      }


      /// <summary>
      /// Синтезирует выражение
      /// </summary>
      /// <param name="data">Объект для заполнения</param>
      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, Op));
        data.Tokens.Add(new SynthesisToken(data, this, "("));
        _RightExpression.Synthesize(data);
        data.Tokens.Add(new SynthesisToken(data, this, ")"));
      }
      #endregion

      #region Текстовое представление

      /// <summary>
      /// Возвращает знак операции
      /// </summary>
      /// <returns>Текстовое представление</returns>
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
    /// Круглые скобки "()" для задания порядка вычислений
    /// </summary>
    public class ParenthesExpression : IExpression
    {
      #region Конструктор

      /// <summary>
      /// Создает выражение "круглые скобки"
      /// </summary>
      /// <param name="openToken">Лексема "("</param>
      /// <param name="closeToken">Лексема ")"</param>
      /// <param name="expression">Выражение в скобках</param>
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

      #region Свойства

      /// <summary>
      /// Лексема открывающей скобки
      /// </summary>
      public Token OpenToken { get { return _OpenToken; } }
      private Token _OpenToken;

      /// <summary>
      /// Лексема закрывающей скобки
      /// </summary>
      public Token CloseToken { get { return _CloseToken; } }
      private Token _CloseToken;

      /// <summary>
      /// Выражение в скобках
      /// </summary>
      public IExpression Expression { get { return _Expression; } }
      private IExpression _Expression;

      /// <summary>
      /// Если свойство установить в true, то при получении текстового представления будет выведено
      /// только текст для Expression, а скобки выводится не будут. То есть данный объект будет бездействующим
      /// </summary>
      public bool IgnoreParenthes { get { return _IgnoreParenthes; } set { _IgnoreParenthes = value; } }
      private bool _IgnoreParenthes;

      #endregion

      #region IExpression Members

      /// <summary>
      /// Вычисляет выражение в скобках
      /// </summary>
      /// <returns>Результат вычисления выражения</returns>
      public object Calc()
      {
        return _Expression.Calc();
      }

      /// <summary>
      /// Возвращает true, если выражение является константой
      /// </summary>
      public bool IsConst
      {
        get
        {
          return _Expression.IsConst;
        }
      }

      /// <summary>
      /// Добавляет в список лексемы "(" и ")"
      /// </summary>
      /// <param name="tokens">Список для заполнения</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_OpenToken);
        tokens.Add(_CloseToken);
      }

      /// <summary>
      /// Добавляет в список выражение в скобках
      /// </summary>
      /// <param name="expressions">Список для заполнения</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        expressions.Add(_Expression);
      }

      /// <summary>
      /// Синтезирует выражение
      /// </summary>
      /// <param name="data">Заполняемый объект</param>
      public void Synthesize(SynthesisData data)
      {
        if (!IgnoreParenthes)
          data.Tokens.Add(new SynthesisToken(data, this, "("));
        _Expression.Synthesize(data);
        if (!IgnoreParenthes)
          data.Tokens.Add(new SynthesisToken(data, this, ")"));
      }

      #endregion

      #region Текстовое представление

      /// <summary>
      /// Возвращает "()"
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
    /// Распознание лексемы
    /// </summary>
    /// <param name="data">Данные парсинга</param>
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

      // На первом этапе разбора не важно, будет операция бинарной или унарной

      // Бинарные операции 
      foreach (BinaryOpDef Def in BinaryOps)
      {
        if (data.StartsWith(Def.Op, false))
        {
          data.Tokens.Add(new Token(data, this, Def.Op, data.CurrPos, Def.Op.Length));
          return;
        }
      }

      // Унарные операции 
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
    /// Получение вычислимого выражения
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Предшествуюшее выражение</param>
    /// <returns>Вычислимое выражение или null в случае ошибки</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token OpToken = data.CurrToken; // после поиска правого выражения ссылка изменится
      data.SkipToken(); // Пропускаем знак операции

      if (OpToken.TokenType == "(")
        return CreateParenthesExpression(data, leftExpression);
      if (OpToken.TokenType == ")")
      {
        OpToken.SetError("Непарная закрывающая скобка");
        return null;
      }

      // TODO: 01.03.2017 использовать приоритеты для разграничения EndTokens

      // 07.09.2015 Лексемы, которые могут завершать правую часть выражение.
      // Например, для выражения "a+b*c" правым выражением будет "b*c"?
      // а для "a+b-c" будет "b", а "-с" вычисляется отдельно
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
          throw new BugException("Неизвестный знак операции \"" + OpToken.TokenType + "\"");
      }

      if (data.EndTokens != null)
        EndTokens = DataTools.MergeArrays<string>(EndTokens, data.EndTokens);
      IExpression RightExpession = data.Parsers.CreateSubExpression(data, EndTokens); // получение правого выражения
      if (RightExpession == null)
      {
        OpToken.SetError("Не найден правый операнд для операции \"" + OpToken.TokenType + "\"");
        return null;
      }

      if (leftExpression == null)
      {
        // Если левого операнда нет, то может быть только унарная операция
        if (!UnaryOps.Contains(OpToken.TokenType))
        {
          data.CurrToken.SetError("Не найден левый операнд для операции \"" + OpToken.TokenType + "\". Операция не может быть унарной");
          return null;
        }
        return new UnaryExpression(OpToken, RightExpession, UnaryOps[OpToken.TokenType].CalcMethod);
      }

      // Формальность
      if (!BinaryOps.Contains(OpToken.TokenType))
      {
        data.CurrToken.SetError("Операция \"" + OpToken.TokenType + "\" не может быть бинарной");
        return null;
      }


      BinaryExpression LeftExpression2 = leftExpression as BinaryExpression;
      if (LeftExpression2 != null)
      {
        int LeftPriority = GetPriority(LeftExpression2.Op);
        int CurrPriority = GetPriority(OpToken.TokenType);
        if (CurrPriority > LeftPriority)
        {
          // Текущая операция ("*") имеет больший приоритет, чем предыдущая ("+")
          // Выполняем замену

          // Текущая операция
          BinaryExpression Expr2 = new BinaryExpression(OpToken, LeftExpression2.RightExpression, RightExpession, BinaryOps[OpToken.TokenType].CalcMethod);

          return new BinaryExpression(LeftExpression2.OpToken, LeftExpression2.LeftExpression, Expr2, LeftExpression2.CalcMethod);
        }
      }

      // Обычный порядок операции
      return new BinaryExpression(OpToken, leftExpression, RightExpession, BinaryOps[OpToken.TokenType].CalcMethod);
    }

    /// <summary>
    /// Получить приоритет для операции
    /// </summary>
    /// <param name="op"></param>
    /// <returns></returns>
    private int GetPriority(string op)
    {
      int p = BinaryOps.IndexOf(op);
      if (p < 0)
        throw new ArgumentException("Операции \"" + op + "\" нет в списке бинарных операций", "op");

      return BinaryOps[p].Priority;
    }


    private IExpression CreateParenthesExpression(ParsingData data, IExpression leftExpression)
    {
      Token OpenToken = data.Tokens[data.CurrTokenIndex - 1];
      //Data.SkipToken(); было пропущено в вызывающем методе

      if (leftExpression != null)
      {
        OpenToken.SetError("Перед открывающей скобкой должна идти операция");
        // ? можно продолжить обзор
      }

      IExpression Expr = data.Parsers.CreateSubExpression(data, new string[] { ")" });
      if (Expr == null)
      {
        OpenToken.SetError("Выражение в скобках не задано");
        return null;
      }

      if (data.CurrTokenType == ")")
      {
        Token CloseToken = data.CurrToken;
        data.SkipToken();
        return new ParenthesExpression(OpenToken, CloseToken, Expr);
      }

      OpenToken.SetError("Не найдена парная закрывающая скобка");
      return null;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Парсер для разбора числовых констант
  /// Научная нотация (1e3) не поддерживается.
  /// Создает лексему "Const"
  /// </summary>
  public class NumConstParser : IParser
  {
    #region Конструктор

    /// <summary>
    /// Создает парсер
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

    #region Свойства, управляющие парсингом

    /// <summary>
    /// Форматировщик.
    /// По умолчанию используется CultureInfo.CurrentCulture.NumberFormat
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
    /// Символы замены. Например, если требуется задать распознание в качестве десятичной точки символов
    /// "," и ".", можно задать FormatProvider=StdConvert.NumberFormat и добавить пару (",":".") в ReplaceChars
    /// По умолчанию, список замен пустой
    /// </summary>
    public Dictionary<string, string> ReplaceChars { get { return _ReplaceChars; } }
    private Dictionary<string, string> _ReplaceChars;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Int32
    /// </summary>
    public bool AllowInt { get { return _AllowInt; } set { _AllowInt = value; } }
    private bool _AllowInt;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Single
    /// </summary>
    public bool AllowSingle { get { return _AllowSingle; } set { _AllowSingle = value; } }
    private bool _AllowSingle;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Double
    /// </summary>
    public bool AllowDouble { get { return _AllowDouble; } set { _AllowDouble = value; } }
    private bool _AllowDouble;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Decimal
    /// </summary>
    public bool AllowDecimal { get { return _AllowDecimal; } set { _AllowDecimal = value; } }
    private bool _AllowDecimal;

    #endregion

    #region Список допустимых символов

    private string _ValidChars;

    private string GetValidChars()
    {
      // Парсинг может выполняться асинхронно, поэтому необходима блокировка

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
              throw new InvalidOperationException("Задана недопустимая подстановка \"" + Pair.Key + "\" -> \"" + Pair.Value +
                "\". Исходный текст и замена должны иметь одинаковую длину");

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
    /// Выполняет парсинг
    /// </summary>
    /// <param name="data">Заполняемый объект</param>
    public void Parse(ParsingData data)
    {
      #region Сначала выполняем быстрый поиск по подходяшим символам

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

      #region Подстановки символов

      foreach (KeyValuePair<string, string> Pair in _ReplaceChars)
        s = s.Replace(Pair.Key, Pair.Value);

      #endregion

      #region Пытаемся выполнить преобразование

      NumberStyles ns = NumberStyles.AllowLeadingSign /*| Нельзя ! NumberStyles.AllowTrailingSign */| NumberStyles.AllowDecimalPoint;
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
    /// Константное выражение
    /// </summary>
    public class ConstExpression : IExpression
    {
      #region Конструктор

      /// <summary>
      /// Создает выражение
      /// </summary>
      /// <param name="value">Значение константы</param>
      /// <param name="token">Лексема</param>
      public ConstExpression(object value, Token token)
      {
        //if (Token == null)
        //  throw new ArgumentNullException("Token");

        _Value = value;
        _Token = token;
      }

      /// <summary>
      /// Создает выражение без лексемы.
      /// Эта версия конструктора применяется только при специальных вариантах парсинга, когда существуют
      /// неявные выражения.
      /// </summary>
      /// <param name="value">Значение константы</param>
      public ConstExpression(object value)
      {
        _Value = value;
        _Token = null;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Константа
      /// </summary>
      public object Value { get { return _Value; } }
      private object _Value;

      /// <summary>
      /// Лексема, откуда взята константа. Может быть null
      /// </summary>
      public Token Token { get { return _Token; } }
      private Token _Token;

      #endregion

      #region IExpression Members

      /// <summary>
      /// Возвращает Value
      /// </summary>
      /// <returns></returns>
      public object Calc()
      {
        return _Value;
      }

      /// <summary>
      /// Возвращает true - признак константного выражения.
      /// </summary>
      public bool IsConst
      {
        get
        {
          return true;
        }
      }

      /// <summary>
      /// Добавляет лексему в список, если она задана
      /// </summary>
      /// <param name="tokens">Список для заполнения</param>
      public void GetTokens(IList<Token> tokens)
      {
        if (_Token != null)
          tokens.Add(_Token);
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      /// <param name="expressions">Список для заполнения</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        // Нет дочерних выражений
      }


      /// <summary>
      /// Синтезирует выражение
      /// </summary>
      /// <param name="data">Заполняемый объект</param>
      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, "Const", data.CreateValueText(_Value), _Value));
      }
      #endregion

      #region Текстовое представление

      /// <summary>
      /// Возвращает Value
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return _Value.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Возвращает выражение ConstExpression
    /// </summary>
    /// <param name="data">Данные парисинга</param>
    /// <param name="leftExpression">Должно быть null</param>
    /// <returns>Выражение</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token CurrToken = data.CurrToken;
      data.SkipToken();
      if (leftExpression != null)
      {
        CurrToken.SetError("Константа не должна идти непосредственно после другого выражения. Ожидалась операция");
        // ? можно продолжить разбор
      }

      return new ConstExpression(CurrToken.AuxData, CurrToken);
    }

    #endregion
  }

  /// <summary>
  /// Парсер для разбора строковых констант, заключенных в кавычки или апострофы.
  /// Чтобы символ-ограничитель входил в строку, он должен быть задвоен.
  /// Создает лексему "String"
  /// </summary>
  public class StrConstParser : IParser
  {
    #region Конструктор

    /// <summary>
    /// Создает сепаратор для строк в двойных кавычках
    /// </summary>
    public StrConstParser()
      : this('\"')
    {
    }

    /// <summary>
    /// Создает сепаратор для строк, ограниченных указанным символом.
    /// </summary>
    /// <param name="separator">Маркер начала/окончания строки (кавычки или апостроф)</param>
    public StrConstParser(char separator)
    {
      _Separator = separator;
    }

    #endregion

    #region Свойства, управляющие парсингом

    /// <summary>
    /// Ограничитель строки.
    /// Задается в конструкторе
    /// </summary>
    public char Separator { get { return _Separator; } }
    private char _Separator;

    #endregion

    #region Parse

    /// <summary>
    /// Выполняет парсинг
    /// </summary>
    /// <param name="data">Объект парсинга</param>
    public void Parse(ParsingData data)
    {
      if (data.GetChar(data.CurrPos) != Separator)
        return;

      StringBuilder sb = new StringBuilder();
      int len = 1; // сама кавычка
      for (int p = data.CurrPos + 1; p < data.Text.Text.Length; p++)
      {
        len++;
        if (data.GetChar(p) == Separator)
        {
          if (data.GetChar(p + 1) == Separator)
          {
            // Две кавычки подряд - это часть строки
            p++;
            len++;
            sb.Append(Separator);
          }
          else
          {
            // Конец строки
            data.Tokens.Add(new Token(data, this, "String", data.CurrPos, len, sb.ToString()));
            return;
          }
        }
        else
          // Обычный символ внутри строки
          sb.Append(data.GetChar(p));
      }

      // Строка не закончена
      data.Tokens.Add(new Token(data, this, "String", data.CurrPos, len, sb.ToString(), new ErrorMessageItem(ErrorMessageKind.Error, "Не найден символ завершения строки (" + Separator + ")")));
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// Строковая константа
    /// </summary>
    public class StringExpression : IExpression
    {
      #region Конструктор

      /// <summary>
      /// Создает константное выражение
      /// </summary>
      /// <param name="value">Константа</param>
      /// <param name="token">Лексема</param>
      /// <param name="separator">Символ разделителя, обычно, кавычки</param>
      public StringExpression(string value, Token token, char separator)
      {
        if (token == null)
          throw new ArgumentNullException("token");

        _Value = value;
        _Token = token;
        _Separator = separator;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Строковая константа (без кавычек)
      /// </summary>
      public string Value { get { return _Value; } }
      private string _Value;

      /// <summary>
      /// Лексема
      /// </summary>
      public Token Token { get { return _Token; } }
      private Token _Token;

      /// <summary>
      /// Не нужен
      /// </summary>
      public char Separator { get { return _Separator; } }
      private char _Separator;

      #endregion

      #region IExpression Members

      /// <summary>
      /// Возвращает Value
      /// </summary>
      /// <returns></returns>
      public object Calc()
      {
        return _Value;
      }

      /// <summary>
      /// Возвращает true - признак константного выражения
      /// </summary>
      public bool IsConst
      {
        get
        {
          return true;
        }
      }

      /// <summary>
      /// Добавляет лексему в список
      /// </summary>
      /// <param name="tokens">Заполняемый список</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_Token);
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      /// <param name="expressions">Заполняемый список</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        // Нет дочерних выражений
      }


      /// <summary>
      /// Синтезирует выражение
      /// </summary>
      /// <param name="data">Заполняемый объект</param>
      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, "String", data.CreateValueText(_Value), _Value));
      }

      #endregion

      #region Текстовое представление

      /// <summary>
      /// Возвращает Value (без кавычек)
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return _Value.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Возвращает StringExpression
    /// </summary>
    /// <param name="data">Объект парсинга</param>
    /// <param name="leftExpression">Должно быть null</param>
    /// <returns>Выражение</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token CurrToken = data.CurrToken;
      data.SkipToken();
      if (leftExpression != null)
      {
        CurrToken.SetError("Константа не должна идти непосредственно после другого выражения. Ожидалась операция");
        // ? можно продолжить разбор
      }

      return new StringExpression((string)(CurrToken.AuxData), CurrToken, Separator);
    }

    #endregion
  }

  #region FunctionArgExpressionsCreatedEventHandler

  /// <summary>
  /// Делегат для вычисления функции
  /// </summary>
  /// <param name="name">Нелокализованное имя функции. Аргумент позволяет использовать один обработчик для множества функций, 
  /// если так удобнее</param>
  /// <param name="args">Вычисленные аргументы</param>
  /// <param name="userData">Произвольные пользовательские данные, передданные в CreateExpression</param>
  /// <returns>Результат вычисления функции</returns>
  public delegate object FunctionDelegate(string name, object[] args, NamedValues userData);

  /// <summary>
  /// Аргументы события FunctionDef.ArgExpressionsCreated
  /// </summary>
  public class FunctionArgExpressionsCreatedEventArgs : EventArgs
  {
    #region Свойства

    /// <summary>
    /// Выражения для аргументов функции
    /// </summary>
    public IExpression[] ArgExpressions { get { return _ArgExpressions; } set { _ArgExpressions = value; } }
    private IExpression[] _ArgExpressions;

    #endregion
  }

  /// <summary>
  /// Делегат события FunctionDef.ArgExpressionsCreated
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void FunctionArgExpressionsCreatedEventHandler (object sender, FunctionArgExpressionsCreatedEventArgs args);

  #endregion

  /// <summary>
  /// Описание одной функции
  /// </summary>
  public class FunctionDef : ObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описание функции
    /// </summary>
    /// <param name="name">Имя функции</param>
    /// <param name="calcMethod">Вычисляющий метод</param>
    /// <param name="argCount">Количество аргументов. Если функция может содержать переменное число аргументов,
    /// задайте здесь максимальное количество, а затем установите MinArgCount</param>
    public FunctionDef(string name, FunctionDelegate calcMethod, int argCount)
      : base(name)
    {
      _CalcMethod = calcMethod;
      _MinArgCount = argCount;
      _MaxArgCount = argCount;
      _LocalName = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя функции, например, "SIN".
    /// НЕ локализуется. Если требуется локализация, то используется свойство LocalName
    /// </summary>
    public string Name { get { return base.Code; } }

    /// <summary>
    /// Локализованное имя.
    /// Свойство не является обязательным.
    /// По умолчанию содержит пустую строку
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
    /// Метод, используемый для вычисления функции
    /// </summary>
    public FunctionDelegate CalcMethod { get { return _CalcMethod; } }
    private FunctionDelegate _CalcMethod;

    /// <summary>
    /// Минимально допустимое число аргументов
    /// </summary>
    public int MinArgCount
    {
      get { return _MinArgCount; }
      set { _MinArgCount = value; }
    }
    private int _MinArgCount;

    /// <summary>
    /// Максимально допустимое число аргументов
    /// </summary>
    public int MaxArgCount
    {
      get { return _MaxArgCount; }
      set { _MaxArgCount = value; }
    }
    private int _MaxArgCount;

    /// <summary>
    /// Является ли функция недетерминированной.
    /// Если false (по умолчанию), то результат функции однозначно зависит от ее аргументов. Справедливо для
    /// большинства функций, например, "SIN".
    /// Если функция может возвращать различные значения при одних и тех же аргументов, например "TODAY", то
    /// свойство должно быть установлено в true
    /// </summary>
    public bool IsVolatile
    {
      get { return _IsVolatile; }
      set { _IsVolatile = value; }
    }
    private bool _IsVolatile;

    /// <summary>
    /// Возвращает LocalName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(LocalName))
        return Name;
      else
        return LocalName; // 27.12.2020
    }

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается на второй стадии парсинга, когда построены выражения для всех аргументов функции,
    /// но еще не построено само выражение FunctionParser.FunctionExpression
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
  /// Парсер для разбора функций.
  /// Распознает имя функции (задается список), круглые скобки и разделители аргументов функции 
  /// (список разделителей можно задать)
  /// </summary>
  public class FunctionParser : IParser
  {
    #region Константы

    /// <summary>
    /// Лексема "Имя функции"
    /// </summary>
    public const string TokenName = "FunctionName";

    /// <summary>
    /// Лексема "("
    /// </summary>
    public const string TokenOpen = "FunctionOpen";

    /// <summary>
    /// Лексема ")"
    /// </summary>
    public const string TokenClose = "FunctionClose";

    /// <summary>
    /// Лексема ","
    /// </summary>
    public const string TokenArgSep = "FunctionArgSep";

    #endregion

    #region Конструктор

    /// <summary>
    /// Создать парсер
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

    #region Список функций

    /// <summary>
    /// Список функций
    /// Определения могут добавляться вручную. Для добавления функций Excel можно использовать
    /// метод ExcelFunctions.AddFunctions()
    /// </summary>
    public NamedList<FunctionDef> Functions { get { return _Functions; } }
    private NamedList<FunctionDef> _Functions;

    /// <summary>
    /// Добавить локальные имена функций.
    /// Просматривает заполненный список функций Functions и ищет соответствующий ключ в коллекции
    /// <paramref name="localNames"/>. Если ключ найден, то для функции устанавливается свойство LocalName, равным значению коллекции.
    /// Коллекция <paramref name="localNames"/> может содержать ключи, для которых нет функций в списке Functions
    /// </summary>
    /// <param name="localNames">Локализованные имена. Ключом является глобальное имя, например, "TODAY", а значением - локализованное, например "СЕГОДНЯ"</param>
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
    /// Добавить локальные имена функций.
    /// Просматривает список заданных глобальных имен <paramref name="names"/> и находит соответствующее определение функции
    /// в списке Functions. Если найдено, устанавливает свойство FunctionDef.LocalName равным элементу массива <paramref name="localNames"/>
    /// Список<paramref name="names"/> может содержать имена функций, которых нет в списке Functions
    /// </summary>
    /// <param name="names">Глобальные имена</param>
    /// <param name="localNames">Локализованные имена</param>
    public void SetLocalNames(string[] names, string[] localNames)
    {
      if (localNames.Length != names.Length)
        throw new ArgumentException("Длина массивов не совпадает", "localNames");

      for (int i = 0; i < names.Length; i++)
      {
        FunctionDef fd = Functions[names[i]];
        if (fd != null)
          fd.LocalName = localNames[i];
      }
    }

    #endregion

    #region Свойства, управляющие парсингом

    /// <summary>
    /// Символы, которые могут содержаться в имени функции
    /// По умолчанию содержит заглавные латинские буквы и цифры
    /// Если используется локализация, то требуется добавить символы в строку.
    /// Также можно добавить управляющие символы, например, знак "_"
    /// Если свойство CaseSensitive установлено в true, то требуется также добавить символы неижнего регистра
    /// </summary>
    public string ValidNameChars
    {
      get { return _ValidNameChars; }
      set { _ValidNameChars = value; }
    }
    private string _ValidNameChars;

    /// <summary>
    /// Символы из ValidNameChars, которые не могут быть первыми в имени функции/
    /// По умолчанию содержит цифры от 0 до 9
    /// </summary>
    public string InvalidFirstChars
    {
      get { return _InvalidFirstChars; }
      set { _InvalidFirstChars = value; }
    }
    private string _InvalidFirstChars;

    /// <summary>
    /// Если свойство установлено в true, то имена функций чувстсительны к регистру.
    /// По умолчанию - false
    /// </summary>
    public bool CaseSensitive
    {
      get { return _CaseSensitive; }
      set { _CaseSensitive = value; }
    }
    private bool _CaseSensitive;


    /// <summary>
    /// Разделители списка аргументов. По умолчанию содержит один разделитель, заданный в
    /// CultureInfo.CurrentCulture.TextInfo.ListSeparator.
    /// При необходимости, список может быть очищен и заполнен нужным значением
    /// </summary>
    public List<string> ArgSeparators { get { return _ArgSeparators; } }
    private List<string> _ArgSeparators;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то в разбираемых выражениях разрешено использовать
    /// как локальные имена (LocalName), так и глобавльные (Name).
    /// Если свойства сброшено в false, то можно использовать только локальные имена (LocalName), кроме функций, у
    /// которых LocalName="". Для последних используется Name
    /// </summary>
    public bool UseBothNames
    {
      get { return _UseBothNames; }
      set { _UseBothNames = value; }
    }
    private bool _UseBothNames;

    #endregion

    #region Словарь имен

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
    /// Определяет лексемы:
    /// "Function" - содержит имя функции. В качестве AuxData содержит имя функции, как оно задано в тексте.
    /// Лексема определяется, даже если не обнаружена открывающая скобка
    /// "FunctionOpen" и "FunctionClose" задают скобки "(" и ")". 
    /// Открывающая круглая скобка не распознается, если имя функции не было задано.
    /// "FunctionSep" - разделитель списка аргументов. Реальный разделитель (разделители), задаются в списке ArgSeparators
    /// </summary>
    /// <param name="data"></param>
    public void Parse(ParsingData data)
    {
      Token NewToken;

      #region Скобки

      char ch = data.GetChar(data.CurrPos);

      // Скобки могут быть частью функции, а могут быть частью математического выражения 

      if (ch == '(')
      {
        // Проверяем, что до этого была наша функция
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
              return; // 09.02.2017 - не наша скобка
          }
        }
        return;
      }

      if (ch == ')')
      {
        // Проверяем, подсчитывая скобки
        int Counter = 1; // наша скобка
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
                  // Открывающая скобка наша
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

      #region Разделитель списка аргументов

      for (int i = 0; i < ArgSeparators.Count; i++)
      {
        if (data.StartsWith(ArgSeparators[i], false))
        {
          // 17.12.2018. Проверяем, подсчитывая скобки
          int Counter = 1; // наша скобка
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
                    // Открывающая скобка наша
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

      #region Имя функции

      StringComparison sc = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

      string s1 = new string(ch, 1);

      if (ValidNameChars.IndexOf(s1, sc) < 0)
        return;
      if (InvalidFirstChars.IndexOf(s1, sc) >= 0)
        return;

      // Начало имени функции найдено
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

    #region Вычислитель функции

    /// <summary>
    /// Выражение для вычисления функции.
    /// Если функция имеет аргументы, сначала вычисляются по очереди все аргументы слева направо.
    /// Затем вычисляется функция.
    /// </summary>
    public class FunctionExpression : IExpression
    {
      #region Конструктор

      /// <summary>
      /// Создает объект выражения
      /// </summary>
      /// <param name="function">Описание функции</param>
      /// <param name="args">Выражения для аргументов функции</param>
      /// <param name="nameToken">Лексема для имени функции</param>
      /// <param name="openToken">Лексема для открывающей скобки</param>
      /// <param name="closeToken">Лексема для закрывающей скобки</param>
      /// <param name="argSepTokens">Массив лексем для разделителей аргументов (точки с запятой).
      /// Количество элементов в массиве дожно быть на одно меньше, чем в массиве <paramref name="args"/>.</param>
      /// <param name="userData">Произвольные пользовательские данные, которые будут переданы вычислителю функции</param>
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

      #region Свойства

      /// <summary>
      /// Определение функции
      /// </summary>
      public FunctionDef Function { get { return _Function; } }
      private FunctionDef _Function;


      /// <summary>
      /// Вычисляемые аргументы
      /// </summary>
      public IExpression[] Args { get { return _Args; } }
      private IExpression[] _Args;

      /// <summary>
      /// Лексема с именем функции
      /// </summary>
      public Token NameToken { get { return _NameToken; } }
      private Token _NameToken;

      /// <summary>
      /// Лексема "("
      /// </summary>
      public Token OpenToken { get { return _OpenToken; } }
      private Token _OpenToken;

      /// <summary>
      /// Лексема ")"
      /// </summary>
      public Token CloseToken { get { return _CloseToken; } }
      private Token _CloseToken;

      /// <summary>
      /// Лексемы ","
      /// </summary>
      public Token[] ArgSepTokens { get { return _ArgSepTokens; } }
      private Token[] _ArgSepTokens;

      /// <summary>
      /// Пользовательские данные, переданные в CreateExpression
      /// </summary>
      public NamedValues UserData { get { return _UserData; } }
      private NamedValues _UserData;

      #endregion

      #region IExpression Members

      /// <summary>
      /// Вычисляет функцию.
      /// </summary>
      /// <returns>Результат вычислений</returns>
      public object Calc()
      {
        object[] ArgVals = new object[Args.Length];
        for (int i = 0; i < Args.Length; i++)
          ArgVals[i] = Args[i].Calc();

        return Function.CalcMethod(Function.Name, // не локализованное
          ArgVals,
          UserData);
      }

      /// <summary>
      /// Возвращает false если FunctionDef.IsVolatile=true.
      /// Иначе возвращает true, если функция не имеет аргументов или если все аргументы являются константами.
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
      /// Добавляет в список все лексемы
      /// </summary>
      /// <param name="tokens">Список для заполнения</param>
      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(NameToken);
        tokens.Add(OpenToken);
        for (int i = 0; i < ArgSepTokens.Length; i++)
          tokens.Add(ArgSepTokens[i]);
        tokens.Add(CloseToken);
      }

      /// <summary>
      /// Добавляет в список все выражения для вычисления аргументов
      /// </summary>
      /// <param name="expressions">Список для заполнения</param>
      public void GetChildExpressions(IList<IExpression> expressions)
      {
        for (int i = 0; i < Args.Length; i++)
          expressions.Add(Args[i]);
      }

      /// <summary>
      /// Синтез выражения
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
          // Добавляем константу
          data.Tokens.Add(new SynthesisToken(data, this, "Const", data.CreateValueText(Calc())));
        }
      }

      #endregion

      #region ToString()

      /// <summary>
      /// Возвращает имя функции и скобки
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return _Function.Name + "()";
      }

      #endregion
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// Создает выражение FunctionExpression
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Выражение слева. Должно быть null</param>
    /// <returns>Выражение</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      switch (data.CurrTokenType)
      {
        case TokenName:
          Token CurrToken = data.CurrToken;
          data.SkipToken();

          if (leftExpression != null)
          {
            CurrToken.SetError("Имя функции не может быть продолжением другого выражения. Ожидалась операция");
            // ? можно продолжить
          }

          FunctionDef fd = GetFunction(CurrToken.AuxData.ToString());
          // Ищем лексему открывающей функции
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
                data.CurrToken.SetError("Ожидалась открывающая скобка после имени функции");
                return null;
            }
          }
          if (OpenToken == null)
          {
            CurrToken.SetError("Не найдена открывающая скобка после имени функции");
            return null;
          }

          // Перебираем аргуметы функции
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
                CurrToken.SetError("Не найдена закрывающая скобка для функции");
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

              ErrorToken.SetError("Ожидался аргумент");
            }

            ArgExprs.Add(ArgExpr);
            if (data.CurrToken == null)
            {
              CurrToken.SetError("Не найдена закрывающая скобка для функции");
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
              string ErrorText = "Ожидалась закрывающая скобка вызова функции";
              if (ArgSepTokens.Count > 0)
              {
                ErrorText += " или разделитель списка аргументов \"" + ArgSepTokens[0] + "\"";
                for (int i = 1; i < ArgSepTokens.Count; i++)
                  ErrorText += " или \"" + ArgSepTokens[i] + "\"";
              }
              data.CurrToken.SetError(ErrorText);
              return null;
            }

            ArgSepTokens.Add(data.CurrToken);
            data.SkipToken();
          }

          // Список аргументов загружен. Скобка получена
          if (fd == null)
          {
            CurrToken.SetError("Неизвестное имя функции \"" + CurrToken.AuxData.ToString() + "\"");
            return null;
          }

          IExpression[] ArgExprs2 = ArgExprs.ToArray();
          fd.OnArgExpressionsCreated(ref ArgExprs2);

          if (ArgExprs2.Length < fd.MinArgCount || ArgExprs2.Length > fd.MaxArgCount)
          {
            string ErrorText = "Неправильное количество аргументов функции \"" + fd.ToString() + "\" (" + ArgExprs2.Length.ToString() + ")";
            if (fd.MaxArgCount == fd.MinArgCount)
              ErrorText += ". Ожидалось аргументов: " + fd.MaxArgCount.ToString();
            else
              ErrorText += ". Ожидалось аргументов: от " + fd.MinArgCount.ToString() + " до " + fd.MaxArgCount.ToString();
            CurrToken.SetError(ErrorText);
            return null;
          }


          FunctionExpression FuncExpr = new FunctionExpression(fd, ArgExprs2, CurrToken, OpenToken, CloseToken, ArgSepTokens.ToArray(), data.UserData);
          return FuncExpr;

        default:
          data.CurrToken.SetError("Неожиданное вхождение \"" + data.CurrToken.Text + "\" вне вызова функции");
          data.SkipToken();
          return null;
      }
    }

    /// <summary>
    /// Возвращает описание функции по заданному имени (как оно задано в выражении)
    /// Базовая реализация возвращает описание из массива Functions
    /// Переопределенный метод может возвращать динамически создаваемое описание
    /// Метод возвращает null, если имя функции неизвестно
    /// </summary>
    /// <param name="name">Локализованное или нелокализованное имя функции</param>
    /// <returns>Описание функции</returns>
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
