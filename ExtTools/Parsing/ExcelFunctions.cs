// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Calendar;

namespace FreeLibSet.Parsing
{
  /// <summary>
  /// Коллекция функций Excel, которые можно добавить в список FunctionParser.Functions
  /// </summary>
  public static class ExcelFunctions
  {
    #region Общие методы и свойства

    /// <summary>
    /// Список доступных имен функций
    /// Имена не локализованы
    /// </summary>
    public static readonly string[] Names = new string[] { 

      #region Математические функции

      "SUM",
      "SIGN",
      "ABS",
      "FLOOR",
      "CEILING",
      "ROUND",
      "TRUNC",
      "POWER",
      "SQRT",

      #endregion

      #region Статистические функции

      "MIN",
      "MAX",

      #endregion

      #region Тригонометроические функции

      "PI",
      "DEGREES",
      "RADIANS",
      "COS",
      "ACOS",
      "COSH",
      "ACOSH",
      "SIN",
      "ASIN",
      "SINH",
      "ASINH",
      "TAN",
      "ATAN",
      "ATAN2",
      "TANH",
      "ATANH",

      #endregion

      #region Экспоненты и логарифмы

      "EXP",
      "LN",
      "LOG",
      "LOG10",

      #endregion

      #region Строки

      "LEN",
      "LEFT",
      "RIGHT",
      "LOWER",
      "UPPER",
      "CONCATENATE",
      "REPLACE",
      "SUBSTITUTE",

      #endregion

      #region Дата и время

      "DATE",
      "TIME",
      "NOW",
      "TODAY",
      "YEAR",
      "MONTH",
      "DAY",
      "HOUR",
      "MINUTE",
      "SECOND",
      "WEEKDAY",
      "DATEDIF",
      "DAYS",

      #endregion

      #region Логические функции

      "IF",
      "AND",
      "OR",
      "NOT",
      "TRUE",
      "FALSE",

      #endregion

      #region Выбор

      "CHOOSE",

      #endregion
    };

    /// <summary>
    /// Возвращает определение для определенной функции с заданным именем
    /// Свойство LocalName не устанавливается
    /// </summary>
    /// <param name="name">Нелокализованное имя</param>
    /// <returns>Определение функции или null</returns>
    public static FunctionDef GetFunction(string name)
    {
      FunctionDef fd;
      switch (name)
      {
        #region Математические функции (разнотипные аргументы)

        case "SUM":
          fd = new FunctionDef(name, new FunctionDelegate(CalcSum), 255);
          fd.MinArgCount = 1;
          return fd;
        case "SIGN":
          fd = new FunctionDef(name, new FunctionDelegate(CalcSign), 1);
          return fd;
        case "ABS":
          fd = new FunctionDef(name, new FunctionDelegate(CalcAbs), 1);
          return fd;
        case "FLOOR":
        case "CEILING":
        case "ROUND":
        case "TRUNC":
          fd = new FunctionDef(name, new FunctionDelegate(CalcRound), 2);
          fd.MinArgCount = 1; // Excel требует второй аргумент для ROUND(), а для TRUNC() - не требует
          return fd;

        #endregion

        #region Статистические функции (разнотипные аргументы)

        case "MIN":
        case "MAX":
          fd = new FunctionDef(name, new FunctionDelegate(CalcMinMax), 255);
          fd.MinArgCount = 1;
          return fd;

        #endregion

        #region Математические функции типа Double

        // Без аргументов
        case "PI":
          fd = new FunctionDef(name, new FunctionDelegate(CalcMath0Arg), 0);
          return fd;

        // С одним аргументом
        case "DEGREES":
        case "RADIANS":
        case "COS":
        case "ACOS":
        case "COSH":
        case "ACOSH":
        case "SIN":
        case "ASIN":
        case "SINH":
        case "ASINH":
        case "TAN":
        case "ATAN":
        case "TANH":
        case "ATANH":
        case "EXP":
        case "LN":
        case "LOG10":
        case "SQRT":
          fd = new FunctionDef(name, new FunctionDelegate(CalcMath1Arg), 1);
          return fd;

        // С двумя аргументами
        case "POWER":
        case "ATAN2":
          fd = new FunctionDef(name, new FunctionDelegate(CalcMath2Arg), 2);
          return fd;

        // С одним или двумя аргументами
        case "LOG":
          fd = new FunctionDef(name, new FunctionDelegate(CalcMath21Arg), 2);
          fd.MinArgCount = 1;
          return fd;

        #endregion

        #region Строковые функции

        case "LEN":
          return new FunctionDef(name, new FunctionDelegate(CalcLen), 1);
        case "LEFT":
        case "RIGHT":
          fd = new FunctionDef(name, new FunctionDelegate(CalcLeftRight), 2);
          fd.MinArgCount = 1;
          return fd;
        case "MID":
          return new FunctionDef(name, new FunctionDelegate(CalcMid), 3);
        case "UPPER":
        case "LOWER":
          return new FunctionDef(name, new FunctionDelegate(CalcUpperLower), 1);

        case "CONCATENATE":
          fd = new FunctionDef(name, new FunctionDelegate(CalcConcatenate), 255);
          fd.MinArgCount = 0;
          return fd;

        // Функции поиска должны поддерживать шаблонные символы - лень делать

        case "REPLACE":
          return new FunctionDef(name, new FunctionDelegate(CalcReplace), 4);
        case "SUBSTITUTE":
          fd = new FunctionDef(name, new FunctionDelegate(CalcSubstitute), 4);
          fd.MinArgCount = 3;
          return fd;


        #endregion

        #region Дата и время

        case "DATE":
          return new FunctionDef(name, _FDDateTime, 3);
        case "TIME":
          return new FunctionDef(name, _FDDateTime, 3);
        case "NOW":
          fd = new FunctionDef(name, _FDDateTime, 0);
          fd.IsVolatile = true;
          return fd;
        case "TODAY":
          fd = new FunctionDef(name, _FDDateTime, 0);
          fd.IsVolatile = true;
          return fd;

        case "YEAR":
        case "MONTH":
        case "DAY":
        case "HOUR":
        case "MINUTE":
        case "SECOND":
        case "WEEKDAY":
          fd = new FunctionDef(name, _FDDateTimePart, 1);
          return fd;

        case "DATEDIF":
          return new FunctionDef(name, new FunctionDelegate(CalcDateDif), 3);
        case "DAYS":
          return new FunctionDef(name, new FunctionDelegate(CalcDays), 2);

        #endregion

        #region Логические функции

        case "IF":
          fd = new FunctionDef(name, new FunctionDelegate(CalcIf), 3);
          fd.MinArgCount = 3; // не поддерживаем другое число аргументов
          return fd;
        case "AND":
          fd = new FunctionDef(name, new FunctionDelegate(CalcAnd), 255);
          fd.MinArgCount = 0;
          return fd;
        case "OR":
          fd = new FunctionDef(name, new FunctionDelegate(CalcOr), 255);
          fd.MinArgCount = 0;
          return fd;
        case "NOT":
          fd = new FunctionDef(name, new FunctionDelegate(CalcNot), 1);
          return fd;
        case "TRUE":
        case "FALSE":
          fd = new FunctionDef(name, new FunctionDelegate(CalcTrueFalse), 0);
          return fd;

        #endregion

        #region Выбор

        case "CHOOSE":
          fd = new FunctionDef(name, new FunctionDelegate(CalcChoose), 255);
          fd.MinArgCount = 2;
          return fd;

        #endregion

        default:
          return null;
      }
    }

    /// <summary>
    /// Добавляет все функции в парсер
    /// Имена функций не локализованы.
    /// Для локализации следует вызвать FunctionParser.SetLocalNames() после добавления функций
    /// </summary>
    /// <param name="parser"></param>
    public static void AddFunctions(FunctionParser parser)
    {
      for (int i = 0; i < Names.Length; i++)
      {
        FunctionDef fd = GetFunction(Names[i]);
        if (fd == null)
          throw new BugException("Не найдена функция \"" + Names[i] + "\"");
        parser.Functions.Add(fd);
      }
    }

    #endregion

    #region Математические функции

    private static object CalcSum(FunctionExpression expression, object[] args)
    {
      return CalcSum(args);
    }

    /// <summary>
    /// Общедоступный метод вычисления суммы от произвольного списка аргументов.
    /// Среди аргументов могут встречаться вложенные массивы произвольной размерности.
    /// Значения null, Boolean и String пропускаются, как это определено для Excel.
    /// Обрабатываются типы Int32, Single, Double и Decimal.
    /// Результат имеет самый "большой" тип.
    /// Если нет ни одного числового аргумента, возвращается null
    /// </summary>
    /// <param name="args">Список аргументов</param>
    /// <returns></returns>
    public static object CalcSum(object args)
    {
      object resValue = null;
      DoCalcSum(args, ref resValue); // рекурсивный метод
      return resValue;
    }

    private static void DoCalcSum(object item, ref object resValue)
    {
      if (item == null)
        return;
      if (item is string)
        return; // ???
      if (item is Boolean)
        return; // так определено в Excel


      Array a = item as Array;
      if (a != null)
      {
        foreach (object item2 in a)
          DoCalcSum(item2, ref resValue); // Рекурсия
        return;
      }

      // Одиночное згачение

      if (resValue == null)
        resValue = item;
      else
      {
        if (item is Int32 && resValue is Int32)
          resValue = DataTools.GetInt(item) + DataTools.GetInt(resValue);
        else if (item is Single && resValue is Single)
          resValue = DataTools.GetSingle(item) + DataTools.GetSingle(resValue);
        else if (item is Double && resValue is Double)
          resValue = DataTools.GetDouble(item) + DataTools.GetDouble(resValue);
        else
          resValue = DataTools.GetDecimal(item) + DataTools.GetDecimal(resValue);
      }
    }

    private static object CalcSign(FunctionExpression expression, object[] args)
    {
      if (args[0] is decimal)
        return Math.Sign((decimal)args[0]);
      if (args[0] is double)
        return Math.Sign((double)args[0]);
      if (args[0] is float)
        return Math.Sign((float)args[0]);
      if (args[0] is DateTime)
        return Math.Sign(((DateTime)args[0]).ToOADate());
      return Math.Sign(DataTools.GetInt(args[0]));
    }

    private static object CalcAbs(FunctionExpression expression, object[] args)
    {
      if (args[0] is decimal)
        return Math.Abs((decimal)args[0]);
      if (args[0] is double)
        return Math.Abs((double)args[0]);
      if (args[0] is float)
        return Math.Abs((float)args[0]);
      return Math.Abs(DataTools.GetInt(args[0]));
    }

    private static object CalcRound(FunctionExpression expression, object[] args)
    {
      object value = args[0];
      int decimals = 0;
      if (args.Length >= 2)
        decimals = DataTools.GetInt(args[1]);

      /*
      switch (name)
      {
        case "FLOOR":
        case "CEILING":
        case "TRUNC":
          if (digits != 0)
            throw new NotImplementedException("Для функций FLOOR, CEILING и TRUNC аргумент \"Число знаков\", отличный от 0, не реализован. Задано: " + digits.ToString());
          break;
        case "ROUND":
          if (digits < 0)
            throw new NotImplementedException("Для функции ROUND аргумент \"Число знаков\", должен быть больше или равен 0. Отрицательное число знаков не реализовано. Задано: " + digits.ToString());
          break;
      }
      */


      if (value is decimal)
      {
        decimal v = (decimal)value;
        switch (expression.Function.Name)
        {
          case "FLOOR": return DataTools.Floor(v, decimals);
          case "CEILING": return DataTools.Ceiling(v, decimals);
          case "ROUND": return DataTools.Round(v, decimals);
          case "TRUNC": return DataTools.Truncate(v, decimals);
          default: throw new BugException();
        }
      }
      else
      {
        double v = Convert.ToDouble(value);
        double m = Math.Pow(10, decimals);
        double res;
        switch (expression.Function.Name)
        {
          case "FLOOR": res = DataTools.Floor(v, decimals); break;
          case "CEILING": res = DataTools.Ceiling(v, decimals); break;
          case "ROUND": res = DataTools.Round(v, decimals); break;
          case "TRUNC": res = DataTools.Truncate(v, decimals); break;
          default: throw new BugException();
        }


        if (args[0] is double)
          return res;
        if (args[0] is float)
          return Convert.ToSingle(res);
        return Convert.ToInt32(res);
      }
    }

    #endregion

    #region Статистические функции

    private static object CalcMinMax(FunctionExpression expression, object[] args)
    {
      bool isMax = (expression.Function.Name == "MAX");

      object resValue = null;

      for (int i = 0; i < args.Length; i++)
      {
        Array a = args[i] as Array;
        if (a == null)
          DoCalcMinMax(args[i], ref resValue, isMax);
        else
        {
          foreach (object Item in a)
            DoCalcMinMax(Item, ref resValue, isMax);
        }
      }

      return resValue;
    }

    private static void DoCalcMinMax(object item, ref object resValue, bool isMax)
    {
      if (item == null)
        return;
      if (item is string)
        return; // так определено в Excel
      if (item is Boolean)
        return; // так определено в Excel

      if (resValue == null)
        resValue = item;
      else
      {
        int cmp = Compare(item, resValue);
        if (isMax)
        {
          if (cmp > 0)
            resValue = item;
        }
        else
        {
          if (cmp < 0)
            resValue = item;
        }
      }
    }

    /// <summary>
    /// Функция сравнения чисел в смысле Excel
    /// Допускаемые типы данных: int, float, double, decimal и DateTime
    /// Больше 0, если a больше b
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат сравнения</returns>
    public static int Compare(object a, object b)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");

      if (a is DateTime)
      {
        if (b is DateTime)
        {
          return (DateTime.SpecifyKind((DateTime)a, DateTimeKind.Unspecified)).CompareTo(DateTime.SpecifyKind((DateTime)b, DateTimeKind.Unspecified));
        }

        a = ((DateTime)a).ToOADate();
      }

      if (b is DateTime)
        b = ((DateTime)b).ToOADate();

      //return System.Collections.Comparer.Default.Compare(a, b);

      // 01.03.2017
      // Так не работает для разных типов
      // Приводим все к "максимальному" типу
      if (a is decimal || b is decimal)
        return System.Collections.Comparer.Default.Compare(DataTools.GetDecimal(a), DataTools.GetDecimal(b));
      if (a is double || b is double)
        return System.Collections.Comparer.Default.Compare(DataTools.GetDouble(a), DataTools.GetDouble(b));
      if (a is float || b is float)
        return System.Collections.Comparer.Default.Compare(DataTools.GetSingle(a), DataTools.GetSingle(b));
      else
        return System.Collections.Comparer.Default.Compare(DataTools.GetInt(a), DataTools.GetInt(b));
    }

#if XXX
    private static readonly Type[] ComparableTypes = new Type[] { typeof(int), typeof(float), typeof(double), typeof(decimal), typeof(DateTime) };
    private enum ComparableType
    { 
      Int32=0,
      Single=1,
      Double=2,
      Decimal=3,
      DateTime=4,
      Unknown=-1
    }

    /// <summary>
    /// Функция сравнения чисел в смысле Excel
    /// Допускаемые типы данных: int, float, double, decimal и DateTime
    /// Больше 0, если a больше b
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат сравнения</returns>
    public static int Compare(object a, object b)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");

      ComparableType cta = GetComparableType(a);
      ComparableType ctb = GetComparableType(b);
      switch (cta)
      { 
        case ComparableType.Int32:
          int ia = (Int32)a;
          switch (ctb)
          { 
            case ComparableType.Int32:
              return ia.CompareTo((Int32)b);
            case ComparableType.Single:
              return ((float)ia).CompareTo((float)b);
            case ComparableType.Double:
              return ((double)ia).CompareTo((double)b);
            case ComparableType.Decimal:
              return ((decimal)ia).CompareTo((decimal)b);
            case ComparableType.DateTime:
              return ((double)ia).CompareTo(((DateTime)b).ToOADate());
            default:
              throw NewCompareException(b, "b");
          }

        case ComparableType.Single:
          float fa = (float)a;
          switch (ctb)
          {
            case ComparableType.Int32:
              return fa.CompareTo((float)(Int32)b);
            case ComparableType.Single:
              return fa.CompareTo((float)b);
            case ComparableType.Double:
              return ((double)ia).CompareTo((double)b);
            case ComparableType.Decimal:
              return ((decimal)ia).CompareTo((decimal)b);
            case ComparableType.DateTime:
              return ((double)ia).CompareTo(((DateTime)b).ToOADate());
            default:
              throw NewCompareException(b, "b");
          }

        case ComparableType.Double:
          int ia = (Int32)a;
          switch (ctb)
          {
            case ComparableType.Int32:
              return ia.CompareTo((Int32)b);
            case ComparableType.Single:
              return ((float)ia).CompareTo((float)b);
            case ComparableType.Double:
              return ((double)ia).CompareTo((double)b);
            case ComparableType.Decimal:
              return ((decimal)ia).CompareTo((decimal)b);
            case ComparableType.DateTime:
              return ((double)ia).CompareTo(((DateTime)b).ToOADate());
            default:
              throw NewCompareException(b, "b");
          }

        case ComparableType.Decimal:
          int ia = (Int32)a;
          switch (ctb)
          {
            case ComparableType.Int32:
              return ia.CompareTo((Int32)b);
            case ComparableType.Single:
              return ((float)ia).CompareTo((float)b);
            case ComparableType.Double:
              return ((double)ia).CompareTo((double)b);
            case ComparableType.Decimal:
              return ((decimal)ia).CompareTo((decimal)b);
            case ComparableType.DateTime:
              return ((double)ia).CompareTo(((DateTime)b).ToOADate());
            default:
              throw NewCompareException(b, "b");
          }

        case ComparableType.DateTime:
          int ia = (Int32)a;
          switch (ctb)
          {
            case ComparableType.Int32:
              return ia.CompareTo((Int32)b);
            case ComparableType.Single:
              return ((float)ia).CompareTo((float)b);
            case ComparableType.Double:
              return ((double)ia).CompareTo((double)b);
            case ComparableType.Decimal:
              return ((decimal)ia).CompareTo((decimal)b);
            case ComparableType.DateTime:
              return ((double)ia).CompareTo(((DateTime)b).ToOADate());
            default:
              throw NewCompareException(b, "b");
          }

        default:
          throw NewCompareException(a, "a");
      }
    }

    private static ComparableType GetComparableType(object Value)
    {
      int p = Array.IndexOf<Type>(ComparableTypes, Value.GetType());
      return (ComparableType)p;
    }

    private static Exception NewCompareException(object Value, string ArgName)
    {
      return new ArgumentException("Недопустимый тип аргумента: " + Value.GetType().ToString() +
        ". Допускаются типы: Int32, Single, Double, Decimal и DateTime");
    }
#endif

    #endregion

    #region Математические функции типа Double

    private static object CalcMath0Arg(FunctionExpression expression, object[] args)
    {
      switch (expression.Function.Name)
      {
        // Без аргументов
        case "PI":
          return Math.PI;
        default:
          throw new BugException();
      }
    }

    private static object CalcMath1Arg(FunctionExpression expression, object[] args)
    {
      double a1 = DataTools.GetDouble(args[0]);

      // Имена функций логарифма не совпадают в Excel и .Net Framework
      // Функция               Excel              .Net Framework
      // Натуральный логарифм  LN(x)              Log(x)
      // Десятичный логарифм   LOG10(x), LOG(x)   Log10(x)
      // По произвольной базе  LOG(x, b)          Log(x, b)


      // Формулы для функций AxxH() взяты из https://ru.wikipedia.org/wiki/%D0%9E%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D1%8B%D0%B5_%D0%B3%D0%B8%D0%BF%D0%B5%D1%80%D0%B1%D0%BE%D0%BB%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B5_%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8

      switch (expression.Function.Name)
      {
        // С одним аргументом
        case "DEGREES": return a1 / Math.PI * 180.0;
        case "RADIANS": return a1 * Math.PI / 180.0;
        case "COS": return Math.Cos(a1);
        case "ACOS": return Math.Acos(a1);
        case "COSH": return Math.Cosh(a1);
        case "ACOSH": return Math.Log(a1 + Math.Sqrt(a1 + 1.0) * Math.Sqrt(a1 - 1.0));
        case "SIN": return Math.Sin(a1);
        case "ASIN": return Math.Asin(a1);
        case "SINH": return Math.Sinh(a1);
        case "ASINH": return Math.Log(a1 + Math.Sqrt(a1 * a1 + 1.0));
        case "TAN": return Math.Tan(a1);
        case "ATAN": return Math.Atan(a1);
        case "TANH": return Math.Tanh(a1);
        case "ATANH": return Math.Log((1.0 + a1) / (1.0 - a1)) / 2.0;
        case "EXP": return Math.Exp(a1);
        case "LN": return Math.Log(a1);
        case "LOG10": return Math.Log10(a1);
        case "SQRT": return Math.Sqrt(a1);
        default:
          throw new BugException();
      }
    }

    private static object CalcMath2Arg(FunctionExpression expression, object[] args)
    {
      double a1 = DataTools.GetDouble(args[0]);
      double a2 = DataTools.GetDouble(args[1]);

      switch (expression.Function.Name)
      {
        // С двумя аргументами
        case "POWER": return Math.Pow(a1, a2);
        case "ATAN2": 
          //return Math.Atan2(a1, a2);
          return Math.Atan2(a2, a1); // 08.11.2022. В Net Framework аргументы идут в обратном порядке по отношению к Excel
        default:
          throw new BugException();
      }
    }

    private static object CalcMath21Arg(FunctionExpression expression, object[] args)
    {
      double a1 = DataTools.GetDouble(args[0]);
      switch (expression.Function.Name)
      {
        case "LOG":
          if (args.Length == 0)
            return Math.Log10(a1);
          else
          {
            double a2 = DataTools.GetDouble(args[1]);
            return Math.Log(a1, a2);
          }
        default:
          throw new BugException();
      }
    }
    #endregion

    #region Строковые функции

    /// <summary>
    /// Возвращает строку в понятиях Excel
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static string GetStr(object v)
    {
      if (v == null)
        return String.Empty;
      if (v is DateTime)
        v = ((DateTime)v).ToOADate();
      return v.ToString();
    }

    private static object CalcLen(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 1);
      return GetStr(args[0]).Length;
    }

    private static object CalcLeftRight(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 1, 2);
      int len = 1;
      if (args.Length >= 2)
        len = DataTools.GetInt(args[1]);
      if (len < 0)
        throw new ArgumentException("Число знаков должно быть больше или равно 0");

      if (expression.Function.Name == "LEFT")
        return DataTools.StrLeft(GetStr(args[0]), len);
      else
        return DataTools.StrRight(GetStr(args[0]), len);
    }

    private static object CalcMid(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 3);
      int start = DataTools.GetInt(args[1]); // Помним, что нумерация символов с 1, а не с 0 
      int len = DataTools.GetInt(args[2]);
      if (start < 1)
        throw new ArgumentException("Начальная позиция должна быть больше 0");
      if (len < 0)
        throw new ArgumentException("Начальная позиция должна быть больше 0");

      // В остальных случаях ошибка не возвращается

      string s = GetStr(args[0]);
      if (start > s.Length)
        return String.Empty;
      if ((start + len) > s.Length)
        return s.Substring(start - 1);
      else
        return s.Substring(start - 1, len);
    }

    private static object CalcUpperLower(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 1);
      if (expression.Function.Name == "UPPER")
        return (GetStr(args[0])).ToUpper();
      else
        return (GetStr(args[0])).ToLower();
    }

    private static object CalcConcatenate(FunctionExpression expression, object[] args)
    {
      if (args.Length == 0)
        return String.Empty;

      string[] a = new string[args.Length];
      for (int i = 0; i < args.Length; i++)
        a[i] = DataTools.GetString(args[i]);
      return String.Join(String.Empty, a);
    }


    private static object CalcReplace(FunctionExpression expression, object[] args)
    {
      string s1 = DataTools.GetString(args[0]);
      int start = DataTools.GetInt(args[1]); // нумерация начинается с 1
      int n = DataTools.GetInt(args[2]);
      string s2 = DataTools.GetString(args[3]);

      StringBuilder sb = new StringBuilder();
      if (start > 1)
        sb.Append(s1.Substring(0, Math.Min(start - 1, s1.Length)));
      sb.Append(s2);
      if ((start + n - 1) <= s1.Length)
        sb.Append(s1.Substring(start + n - 1));
      return sb.ToString();
    }

    private static object CalcSubstitute(FunctionExpression expression, object[] args)
    {
      string s1 = DataTools.GetString(args[0]);
      string s2 = DataTools.GetString(args[1]);
      string s3 = DataTools.GetString(args[2]);
      if (args.Length == 3)
        return s1.Replace(s2, s3);

      // Замена только для заданного вхождения
      int nReplace = DataTools.GetInt(args[3]);

      int startIndex = 0;
      for (int i = 1; i <= nReplace; i++)
      {
        int p = s1.IndexOf(s2, startIndex, s1.Length - startIndex, StringComparison.Ordinal);
        if (p < 0)
          return s1;

        if (i == nReplace)
          return s1.Substring(0, startIndex) + s3 + s1.Substring(startIndex + s2.Length);
        else
          startIndex += s2.Length;
      }
      throw new BugException("Ошибка выполнения цикла");
    }

    #endregion

    #region Функции даты и времени

    #region DATE, TIME, NOW, TODAY

    private static FunctionDelegate _FDDateTime = new FunctionDelegate(CalcDateTime);

    private static object CalcDateTime(FunctionExpression expression, object[] args)
    {
      switch (expression.Function.Name)
      {
        case "DATE":
          CheckArgCount(args, 3);
          return new DateTime(DataTools.GetInt(args[0]), DataTools.GetInt(args[1]), DataTools.GetInt(args[2]),0,0,0,0, DateTimeKind.Unspecified);
        
        case "TIME":
          CheckArgCount(args, 3);
          return new TimeSpan(DataTools.GetInt(args[0]), DataTools.GetInt(args[1]), DataTools.GetInt(args[2]));

        case "NOW":
          CheckArgCount(args, 0);
          return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        case "TODAY":
          CheckArgCount(args, 0);
          return DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);

        default:
          throw new ArgumentException("Неизвестное имя функции \"" + expression.Function.Name + "\"", "name");
      }
    }

    #endregion

    #region Компоненты даты (YEAR .. SECOND), WEEKDAY

    private static FunctionDelegate _FDDateTimePart = new FunctionDelegate(CalcDateTimePart);

    private static object CalcDateTimePart(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 1);
      if (args[0] == null)
        return 0; // В Excel функция DAY() возвращает 0, а остальные - непонятные значения
      // Пока буду возвращать 0
      DateTime dt = GetDateTime(args[0]);
      switch (expression.Function.Name)
      {
        case "YEAR": return dt.Year;
        case "MONTH": return dt.Month;
        case "DAY": return dt.Day;
        case "HOUR": return dt.Hour;
        case "MINUTE": return dt.Minute;
        case "SECOND": return dt.Second;
        case "WEEKDAY": return ((int)dt.DayOfWeek) + 1;
        default:
          throw new ArgumentException("Неизвестное имя функции \"" + expression.Function.Name + "\"", "name");
      }
    }

    private static DateTime GetDateTime(object arg)
    {
      if (arg == null)
        return DateTime.SpecifyKind(DateTime.FromOADate(0), DateTimeKind.Unspecified);

      if (arg is DateTime)
        return DateTime.SpecifyKind((DateTime)arg, DateTimeKind.Unspecified);
      else if (DataTools.IsIntegerType(arg.GetType()) || DataTools.IsFloatType(arg.GetType()))
      {
        double v = DataTools.GetDouble(arg);
        return DateTime.SpecifyKind(DateTime.FromOADate(v), DateTimeKind.Unspecified);
      }
      else if (arg is TimeSpan)
      {
        TimeSpan ts = (TimeSpan)arg;
        if (ts.Ticks >= 0L)
          return new DateTime(ts.Ticks, DateTimeKind.Unspecified);
        else
          return DateTime.SpecifyKind(DateTime.MaxValue.Date.AddTicks(ts.Ticks), DateTimeKind.Unspecified);
      }

      throw new InvalidCastException("Аргумент типа " + arg.GetType().ToString() + " нельзя преобразовать в DateTime");
    }

    #endregion

    #region DATEDIF и Days

    private static object CalcDateDif(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 3);
      DateTime dt1 = GetDateTime(args[0]).Date; // начальная дата
      DateTime dt2 = GetDateTime(args[1]).Date; // конечная дата
      if (dt2 < dt1)
        return null; // неправильный интервал дат. Excel возвращает #Число!
      if (dt2 == dt1)
        return 0;

      DateRange dtr = new DateRange(dt1, dt2.AddDays(-1)); 

      string mode = DataTools.GetString(args[2]);
      switch (mode.ToLowerInvariant())
      {
        case "d": return dtr.Days;
        case "m":
          return dtr.SimpleMonths; 
        case "y": 
          return dtr.SimpleYears;
        case "ym":
        case "md":
        case "yd":
          throw new NotImplementedException("Режимы расчета ym, md и yd не реализованы");
        default:
          throw new ArgumentException("Задан неправильный режим вычисления \"" + mode + "\"");
      }
    }

    private static object CalcDays(FunctionExpression expression, object[] args)
    {
      CheckArgCount(args, 2);
      DateTime dt1 = GetDateTime(args[0]); // начальная дата
      DateTime dt2 = GetDateTime(args[1]); // конечная дата
      TimeSpan ts = dt1 - dt2; // 09.11.2022 - не отрезаем компоненты времени
      if ((ts.Ticks % TimeSpan.TicksPerDay) == 0L)
        return (int)(ts.Ticks / TimeSpan.TicksPerDay);
      else
        return ts.TotalDays;
    }

    #endregion

    #endregion

    #region Логические функции

    private static object CalcIf(FunctionExpression expression, object[] args)
    {
      if (GetBool(args[0]))
        return args[1];
      else
        return args[2];
    }

    private static object CalcAnd(FunctionExpression expression, object[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (!GetBool(args[i]))
          return false;
      }
      return true;
    }

    private static object CalcOr(FunctionExpression expression, object[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (GetBool(args[i]))
          return true;
      }
      return false;
    }

    private static object CalcNot(FunctionExpression expression, object[] args)
    {
      return !GetBool(args[0]);
    }

    private static object CalcTrueFalse(FunctionExpression expression, object[] args)
    {
      switch (expression.Function.Name)
      {
        case "TRUE": return true;
        case "FALSE": return false;
        default:
          throw new BugException();
      }
    }

    private static bool GetBool(object value)
    {
      if (value is DateTime)
        return true;
      else
        return DataTools.GetBool(value);
    }

    #endregion

    #region Выбор

    private static object CalcChoose(FunctionExpression expression, object[] args)
    {
      int index = DataTools.GetInt(args[0]);
      if (index < 1 || index >= args.Length)
        throw new ArgumentOutOfRangeException("args", index, "Индекс должен быть в диапазоне от 1 до " + (args.Length - 1).ToString());
      //return args[index - 1];
      return args[index]; // испр.08.11.2022
    }

    #endregion

    #region Вспомогательные методы

    private static void CheckArgCount(object[] args, int count)
    {
      if (args.Length != count)
        throw new ArgumentException("Неправильное число аргументов (" + args.Length.ToString() + "). Ожидалось аргументов: " + count.ToString(), "args");
    }

    private static void CheckArgCount(object[] args, int minCount, int maxCount)
    {
      if (args.Length < minCount || args.Length > maxCount)
        throw new ArgumentException("Неправильное число аргументов (" + args.Length.ToString() + "). Должно быть аргументов от " + minCount.ToString() + " до " + maxCount.ToString(), "args");
    }

    #endregion
  }
}
