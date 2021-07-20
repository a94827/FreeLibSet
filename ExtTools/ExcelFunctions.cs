using System;
using System.Collections.Generic;
using System.Text;
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

/*
 * Синтаксический разбор текста
 * Класс Parser содержит коллекцию "правил" ParserPart. Класс является "многоразовым"
 * Parser и ParserPart не содержат никаких ссылок на разбиваемый текст
 * Основной метод Parse() выполняет разбор переданных данных
 * 
 * Класс TextWithRows предназначен для хранения исходного текста для разбора. Получая в конструкторе объект
 * string, он выполняет разбиение на строки и позоволяет преобразовывать абсолютную позицию в номер строки и столбца
 * и обратно
 * 
 * Класс ParsingData содержит данные для разбора. Объект создается каждый раз, когда необходимо выполнить разбор
 * - Свойство Text содержит исходный текст в виде TextWithRows
 * - Свойство Tokens содержит результат разбора
 * 
 * Основным результатом синтаксического разбора является массив объектов Token. Token является структурой 
 * фиксированного размера. Тип лексемы определяется строковым полем Token.TokenType, например,
 * "TableRef", "Operation", "Comment", "Space", "Error" и т.д.
 * Объект Token содержит диапазон позиций в исходном тексте, чтобы можно было выполнить подсветку синтаксиса
 * Также он может содержать дополнительные данные, если работы с лексемой недостаточно только текста. 
 * Например, для лексемы "TableRef" задается имя ссылки на таблицы
 * 
 * В процессе разбора, метод Parser.Parse() предлагает каждому объекту ParserPart распознать очередную позицию
 * текста. Класс, производный от ParserPart, пытается сравнить фрагмент текста, начиная с текущей позиции, со
 * "своим" синтаксисом. Например, класс OperationParser проверяет, что очередная позиция содержит "+", "-", "*"
 * или "/". Если условие выполняется, создается лексема "Operation". 
 * 
 * Цикл по ParserPart прекращается, когда очередной элемент смог выполнить разбор.
 * Отсюда следует, что порядок размещения элементов ParserPart имеет значение. Например, если используется 
 * комментарий "//", то CommentParser должен идти до OperationParser. В противном случае, вместо лексемы "Comment"
 * будет добавлено две лексемы "Operation" для операций "/", а затем возникнет ошибка
 * 
 * Если ни один ParserPart не смог распознать текст, добавляется ErrorToken, содержащий один символ, после чего
 * выполняется переход к следующей позиции. Если и следующая позиция содержит ошибку, то новый ErrorToken
 * не добавляется, а к предыдцщему добавляется один символ
 * 
 * Пробельные символы образуют Token типа "Space", также группирующие идущие подряд пробелы
 * Объект Token может содержать ошибку или предупреждение. Метод ParsingData.GetErrorMessages() позволяет
 * собрать список ошибок.
 * 
 * Список объектов Token еще не образуют дерево вычислений, т.к. список является "линейным". Кроме того, 
 * возможно бессмысленное расположение лексем. Например, могут быть обнаружены лексемы:
 * 1. "Operation"
 * 2. "Space"
 * 3. "Operation"
 * 
 * Следующий шаг - это построение дерева вычислений. Он может быть выполнен, только если нет ни одной ErrorToken,
 * то есть когда первый шаг завершился без ошибок.
 * 
 * В общем случае, целью парсинга может быть не получение дерева вычислений, а, например, получение диапазона
 * к которому применимо выражение
 * 
 * Также, из одного исходного условия может быть получено несколько вычисляемых выражений
 * Таким образом, задача парсинга, результатом которой являются объекты Token, отделяется от задачи получения
 * вычисляемого выражения Expression.
 * Для этого используется второй метод IParser.CreateExpression(), получающий на входе ParsingData и
 * возвращающий объект IExpression. 
 * На момент вызова метода все Token уже распознаны.
 * В отличие от первого шага, метод IParser.CreateExpression() вызывается для того IParser, который создал
 * очередную лексему, перебор объектов IParser не нужен.
 * Метод может вернуть null, если лексема не имеет значения, например, это пробел или комментарий. В этом
 * случае разбирается следующая лексема. 
 * 
 * Сериализация
 * ------------
 * Объекты ParserList и реализации IParser, ParsingData и Token не являются сериализуемыми
 * Для передачи ошибок от сервера к клиенту можно использовать метод ParsingData.GetErrorMessages(),
 * который возвращает сериализуемые данные. К каждому ErrorMessageItem к полю Tag присоединяется сериализуемый 
 * объект ParsingErrorItemData, который содержит некоторую часть данных из объекта Token
 */

namespace AgeyevAV.Parsing
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
          return new FunctionDef(name, fdDateTime, 3);
        case "NOW":
          fd = new FunctionDef(name, fdDateTime, 0);
          fd.IsVolatile = true;
          return fd;
        case "TODAY":
          fd = new FunctionDef(name, fdDateTime, 0);
          fd.IsVolatile = true;
          return fd;

        case "YEAR":
        case "MONTH":
        case "DAY":
        case "HOUR":
        case "MINUTE":
        case "SECOND":
        case "WEEKDAY":
          fd = new FunctionDef(name, fdDateTimePart, 1);
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

    private static object CalcSum(string name, object[] args, NamedValues userData)
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
      object ResValue = null;
      DoCalcSum(args, ref ResValue); // рекурсивный метод
      return ResValue;
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
        foreach (object Item2 in a)
          DoCalcSum(Item2, ref resValue); // Рекурсия
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

    private static object CalcSign(string name, object[] args, NamedValues userData)
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

    private static object CalcAbs(string name, object[] args, NamedValues userData)
    {
      if (args[0] is decimal)
        return Math.Abs((decimal)args[0]);
      if (args[0] is double)
        return Math.Abs((double)args[0]);
      if (args[0] is float)
        return Math.Abs((float)args[0]);
      return Math.Abs(DataTools.GetInt(args[0]));
    }

    private static object CalcRound(string name, object[] args, NamedValues userData)
    {
      object Value = args[0];
      int Digits = 0;
      if (args.Length >= 2)
        Digits = DataTools.GetInt(args[1]);

      switch (name)
      {
        case "FLOOR":
        case "CEILING":
        case "TRUNC":
          if (Digits != 0)
            throw new NotImplementedException("Для функций FLOOR, CEILING и TRUNC аргумент \"Число знаков\", отличный от 0, не реализован. Задано: " + Digits.ToString());
          break;
        case "ROUND":
          if (Digits < 0)
            throw new NotImplementedException("Для функции ROUND аргумент \"Число знаков\", должен быть больше или равен 0. Отрицательное число знаков не реализовано. Задано: " + Digits.ToString());
          break;
      }

      if (Value is decimal)
      {
        decimal v = (decimal)Value;
        switch (name)
        {
          case "FLOOR": return Math.Floor(v);
          case "CEILING": return Math.Ceiling(v);
          case "ROUND": return Math.Round(v, Digits, MidpointRounding.AwayFromZero);
          case "TRUNC": return Math.Truncate(v);
          default: throw new BugException();
        }
      }
      else
      {
        double v = Convert.ToDouble(Value);
        double res;
        switch (name)
        {
          case "FLOOR": res = Math.Floor(v); break;
          case "CEILING": res = Math.Ceiling(v); break;
          case "ROUND": res = Math.Round(v, Digits, MidpointRounding.AwayFromZero); break;
          case "TRUNC": res = Math.Truncate(v); break;
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

    private static object CalcMinMax(string name, object[] args, NamedValues userData)
    {
      bool IsMax = (name == "MAX");

      object ResValue = null;

      for (int i = 0; i < args.Length; i++)
      {
        Array a = args[i] as Array;
        if (a == null)
          DoCalcMinMax(args[i], ref ResValue, IsMax);
        else
        {
          foreach (object Item in a)
            DoCalcMinMax(Item, ref ResValue, IsMax);
        }
      }

      return ResValue;
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
          return ((DateTime)a).CompareTo((DateTime)b);
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

    private static object CalcMath0Arg(string name, object[] args, NamedValues userData)
    {
      switch (name)
      {
        // Без аргументов
        case "PI":
          return Math.PI;
        default:
          throw new BugException();
      }
    }

    private static object CalcMath1Arg(string name, object[] args, NamedValues userData)
    {
      double a1 = DataTools.GetDouble(args[0]);

      // Имена функций логарифма не совпадают в Excel и .Net Framework
      // Функция               Excel              .Net Framework
      // Натуральный логарифм  LN(x)              Log(x)
      // Десятичный логарифм   LOG10(x), LOG(x)   Log10(x)
      // По произвольной базе  LOG(x, b)          Log(x, b)

      // Версяи функции Excel Log(x) в смысле десятичного логарифма пока не


      // Формулы для функций AxxH() взяты из https://ru.wikipedia.org/wiki/%D0%9E%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D1%8B%D0%B5_%D0%B3%D0%B8%D0%BF%D0%B5%D1%80%D0%B1%D0%BE%D0%BB%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B5_%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8

      switch (name)
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

    private static object CalcMath2Arg(string name, object[] args, NamedValues userData)
    {
      double a1 = DataTools.GetDouble(args[0]);
      double a2 = DataTools.GetDouble(args[1]);

      switch (name)
      {
        // С двумя аргументами
        case "POWER": return Math.Pow(a1, a2);
        case "ATAN2": return Math.Atan2(a1, a2);
        default:
          throw new BugException();
      }
    }

    private static object CalcMath21Arg(string name, object[] args, NamedValues userData)
    {
      double a1 = DataTools.GetDouble(args[0]);
      switch (name)
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

    private static object CalcLen(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 1);
      return GetStr(args[0]).Length;
    }

    private static object CalcLeftRight(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 1, 2);
      int Len = 1;
      if (args.Length >= 2)
        Len = DataTools.GetInt(args[1]);
      if (Len < 0)
        throw new ArgumentException("Число знаков должно быть больше или равно 0");

      if (name == "LEFT")
        return DataTools.StrLeft(GetStr(args[0]), Len);
      else
        return DataTools.StrRight(GetStr(args[0]), Len);
    }

    private static object CalcMid(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 3);
      int Start = DataTools.GetInt(args[1]); // Помним, что нумерация символов с 1, а не с 0 
      int Len = DataTools.GetInt(args[2]);
      if (Start < 1)
        throw new ArgumentException("Начальная позиция должна быть больше 0");
      if (Len < 0)
        throw new ArgumentException("Начальная позиция должна быть больше 0");

      // В остальных случаях ошибка не возвращается

      string s = GetStr(args[0]);
      if (Start > s.Length)
        return String.Empty;
      if ((Start + Len) > s.Length)
        return s.Substring(Start - 1);
      else
        return s.Substring(Start - 1, Len);
    }

    private static object CalcUpperLower(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 1);
      if (name == "UPPER")
        return (GetStr(args[0])).ToUpper();
      else
        return (GetStr(args[0])).ToLower();
    }

    private static object CalcConcatenate(string name, object[] args, NamedValues userData)
    {
      if (args.Length == 0)
        return String.Empty;

      string[] a = new string[args.Length];
      for (int i = 0; i < args.Length; i++)
        a[i] = DataTools.GetString(args[i]);
      return String.Join(String.Empty, a);
    }


    private static object CalcReplace(string name, object[] args, NamedValues userData)
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

    private static object CalcSubstitute(string name, object[] args, NamedValues userData)
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

    #region DATE, NOW, TODAY

    private static FunctionDelegate fdDateTime = new FunctionDelegate(CalcDateTime);

    private static object CalcDateTime(string name, object[] args, NamedValues userData)
    {
      switch (name)
      {
        case "DATE":
          CheckArgCount(args, 3);
          return new DateTime(DataTools.GetInt(args[0]), DataTools.GetInt(args[1]), DataTools.GetInt(args[2]));

        case "NOW":
          CheckArgCount(args, 0);
          return DateTime.Now;

        case "TODAY":
          CheckArgCount(args, 0);
          return DateTime.Today;

        default:
          throw new ArgumentException("Неизвестное имя функции \"" + name + "\"", "name");
      }
    }

    #endregion

    #region Компоненты даты (YEAR .. SECOND), WEEKDAY

    private static FunctionDelegate fdDateTimePart = new FunctionDelegate(CalcDateTimePart);

    private static object CalcDateTimePart(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 1);
      if (args[0] == null)
        return 0; // В Excel функция DAY() возвращает 0, а остальные - непонятные значения
      // Пока буду возвращать 0
      DateTime dt;
      dt = GetDate(args[0]);
      switch (name)
      {
        case "YEAR": return dt.Year;
        case "MONTH": return dt.Month;
        case "DAY": return dt.Day;
        case "HOUR": return dt.Hour;
        case "MINUTE": return dt.Minute;
        case "SECOND": return dt.Second;
        case "WEEKDAY": return ((int)dt.DayOfWeek) + 1;
        default:
          throw new ArgumentException("Неизвестное имя функции \"" + name + "\"", "name");
      }
    }

    private static DateTime GetDate(object arg)
    {
      if (arg == null)
        return DateTime.FromOADate(0);

      if (arg is DateTime)
        return (DateTime)arg;
      else if (DataTools.IsIntegerType(arg.GetType()) || DataTools.IsFloatType(arg.GetType()))
      {
        double v = DataTools.GetDouble(arg);
        return DateTime.FromOADate(v);
      }

      throw new InvalidCastException("Аргумент типа " + arg.GetType().ToString() + " нельзя преобразовать в DateTime");
    }

    #endregion

    #region DATEDIF и Days

    private static object CalcDateDif(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 3);
      DateTime dt1 = GetDate(args[0]).Date; // начальная дата
      DateTime dt2 = GetDate(args[1]).Date; // конечная дата
      if (dt2 < dt1)
        return null; // неправильный интервал дат. Excel возвращает #Число!
      if (dt2 == dt1)
        return 0;

      DateRange dtr = new DateRange(dt1, dt2);

      string Mode = DataTools.GetString(args[2]);
      switch (Mode.ToLowerInvariant())
      {
        case "d": return dtr.Days - 1;
        case "m": return dtr.Months;
        case "y": return dtr.Years;
        case "ym":
        case "md":
        case "yd":
          throw new NotImplementedException("Режимы расчета ym, md и yd не реализованы");
        default:
          throw new ArgumentException("Задан неправильный режим вычисления \"" + Mode + "\"");
      }
    }

    private static object CalcDays(string name, object[] args, NamedValues userData)
    {
      CheckArgCount(args, 2);
      DateTime dt1 = GetDate(args[0]).Date; // начальная дата
      DateTime dt2 = GetDate(args[1]).Date; // конечная дата

      return dt1 - dt2;
    }

    #endregion

    #endregion

    #region Логические функции

    private static object CalcIf(string name, object[] args, NamedValues userData)
    {
      if (GetBool(args[0]))
        return args[1];
      else
        return args[2];
    }

    private static object CalcAnd(string name, object[] args, NamedValues userData)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (!GetBool(args[i]))
          return false;
      }
      return true;
    }

    private static object CalcOr(string name, object[] args, NamedValues userData)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (GetBool(args[i]))
          return true;
      }
      return false;
    }

    private static object CalcNot(string name, object[] args, NamedValues userData)
    {
      return !GetBool(args[0]);
    }

    private static object CalcTrueFalse(string name, object[] args, NamedValues userData)
    {
      switch (name)
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

    private static object CalcChoose(string name, object[] args, NamedValues userData)
    {
      int Index = DataTools.GetInt(args[0]);
      if (Index < 1 || Index >= args.Length)
        throw new ArgumentOutOfRangeException("args", Index, "Индекс должен быть в диапазоне от 1 до " + (args.Length - 1).ToString());
      return args[Index - 1];
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
