// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Core
{
  partial class DataTools
  {
    #region IsXXXType()

    private static readonly ArrayIndexer<Type> _IntegerTypeArrayIndexer =
      new ArrayIndexer<Type>(new Type[] {
          typeof(byte), typeof(sbyte), typeof(Int16), typeof(UInt16),
          typeof(Int32), typeof(UInt32), typeof(Int64), typeof(UInt64) });

    /// <summary>
    /// Функция возвращает true, если тип данных относится к одному из 8 
    /// целочисленных типов
    /// </summary>
    /// <param name="type">Проверяемый тип</param>
    /// <returns>Соответствие проверяемого типа</returns>
    public static bool IsIntegerType(Type type)
    {
      return _IntegerTypeArrayIndexer.Contains(type);
    }

    private static readonly ArrayIndexer<Type> _FloatTypeArrayIndexer =
      new ArrayIndexer<Type>(new Type[] {
          typeof(Single), typeof(Double), typeof(Decimal)});

    /// <summary>
    /// Функция возвращает true, если тип данных относится к одному из трех
    /// типов с плавающей точкой
    /// </summary>
    /// <param name="type">Проверяемый тип</param>
    /// <returns>Соответствие проверяемого типа</returns>
    public static bool IsFloatType(Type type)
    {
      return _FloatTypeArrayIndexer.Contains(type);
    }


    /// <summary>
    /// Функция возвращает true, если тип данных относится к одному из 11 числовых типов, 
    /// то есть IsIntegerType() или IsFloatType()
    /// </summary>
    /// <param name="type">Проверяемый тип</param>
    /// <returns>Соответствие проверяемого типа</returns>
    public static bool IsNumericType(Type type)
    {
      return IsIntegerType(type) || IsFloatType(type);
    }

    #endregion

    #region Деление чисел с округлением

    /// <summary>
    /// Деление двух целых чисел.
    /// В отличие от обычного деления, округление выполняется не в сторону 0, а к ближайшему числу.
    /// </summary>
    /// <param name="x">Делимое</param>
    /// <param name="y">Делитель</param>
    /// <returns>Частное</returns>
    /// <remarks>
    /// 8/3=2
    /// DivideWithRounding(8,3)=3
    /// </remarks>
    public static int DivideWithRounding(int x, int y)
    {
      int r; // остаток (всегда имеет тот же знак, что и делимое)
      int q = Math.DivRem(x, y, out r); // q-частное

      if (r == 0)
        return q; // включает также случаи, когда x=0 или y=1

      // Нужно определить условие |r/y|>=0.5, и тогда "отодвинуть" от 0 значение q на единицу
      // Это тоже самое, что и |r|*2>=|y|, что позволяет вычислять без перехода к числам с плавающей точкой
      // Проблема: Вычисление r*2 может привести к переполнению
      // Лучше использовать условие |r|>=|y|-|r|

      if (Math.Abs(r) >= (Math.Abs(y) - Math.Abs(r)))
      {
        // Требуется изменить остаток
        if ((r > 0) ^ (y > 0)) // исключающее или
          return q - 1;
        else
          return q + 1;
      }
      else
        // Округление правильное
        return q;
    }

    /// <summary>
    /// Деление двух целых чисел типа Int64.
    /// В отличие от обычного деления, округление выполняется не в сторону 0, а к ближайшему числу.
    /// </summary>
    /// <param name="x">Делимое</param>
    /// <param name="y">Делитель</param>
    /// <returns>Частное</returns>
    public static long DivideWithRounding(long x, long y)
    {
      long r; // остаток (всегда имеет тот же знак, что и делимое)
      long q = Math.DivRem(x, y, out r); // q-частное

      if (r == 0L)
        return q; // включает также случаи, когда x=0 или y=1

      // Нужно определить условие |r/y|>=0.5, и тогда "отодвинуть" от 0 значение q на единицу
      // Это тоже самое, что и |r|*2>=|y|, что позволяет вычислять без перехода к числам с плавающей точкой
      // Проблема: Вычисление r*2 может привести к переполнению
      // Лучше использовать условие |r|>=|y|-|r|

      if (Math.Abs(r) >= (Math.Abs(y) - Math.Abs(r)))
      {
        // Требуется изменить остаток
        if ((r > 0L) ^ (y > 0L)) // исключающее или
          return q - 1L;
        else
          return q + 1L;
      }
      else
        // Округление правильное
        return q;
    }

    #endregion

    #region Математические функции двух аргументов

    /// <summary>
    /// Выполнить суммирование двух аргументов
    /// Выполняется преобразование к самому большому типу.
    /// Если один из аргументов равен null, возвращается другой аргумент.
    /// Если оба аргумента равны null, возвращается null.
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат сложения</returns>
    public static object SumValues(object a, object b)
    {
      if (a == null)
        return b;
      if (b == null)
        return a;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());

      if (st1 == SumType.DateTime && st2 == SumType.TimeSpan)
      {
        return (DateTime)a + (TimeSpan)b; // 16.12.2021
      }

      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return (int)v1 + (int)v2;
          case SumType.Int64: return (long)v1 + (long)v2;
          case SumType.Single: return (float)v1 + (float)v2;
          case SumType.Double: return (double)v1 + (double)v2;
          case SumType.Decimal: return (decimal)v1 + (decimal)v2;
          case SumType.TimeSpan: return (TimeSpan)v1 + (TimeSpan)v2;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть просуммированы");
        }
      }
    }


    /// <summary>
    /// Выполнить вычитание второго аргумента из первого
    /// Выполняется преобразование к самому большому типу.
    /// Если первый аргумент равен null, возвращается null.
    /// Если второй аргумент равен null, возвращается первый аргумент.
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат вычитания</returns>
    public static object SubstractValues(object a, object b)
    {
      if (a == null)
        return null;
      if (b == null)
        return a;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());
      if (st1 == SumType.DateTime && st2 == SumType.TimeSpan)
      {
        return (DateTime)a - (TimeSpan)b; // 16.12.2021
      }

      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return (int)v1 - (int)v2;
          case SumType.Int64: return (long)v1 - (long)v2;
          case SumType.Single: return (float)v1 - (float)v2;
          case SumType.Double: return (double)v1 - (double)v2;
          case SumType.Decimal: return (decimal)v1 - (decimal)v2;
          case SumType.TimeSpan: return (TimeSpan)v1 - (TimeSpan)v2;
          case SumType.DateTime: return (DateTime)v1 - (DateTime)v2;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут вычитаться");
        }
      }
    }


    /// <summary>
    /// Получить отрицательное значение аргумента.
    /// Если аргумент равен null, возвращается null.
    /// </summary>
    /// <param name="a">Аргумент</param>
    /// <returns>Отрицательное значение</returns>
    public static object NegValue(object a)
    {
      if (a == null)
        return null;

      SumType st = GetSumType(a.GetType());
      object v = ConvertValue(a, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return -(int)v;
          case SumType.Int64: return -(long)v;
          case SumType.Single: return -(float)v;
          case SumType.Double: return -(double)v;
          case SumType.Decimal: return -(decimal)v;
          case SumType.TimeSpan: return -(TimeSpan)v;
          default:
            throw new ArgumentException("Для значения типа " + a.GetType() + " не может быть получено отрицательное значение");
        }
      }
    }

    /// <summary>
    /// Выполнить умножение первого аргумента на второй
    /// Выполняется преобразование к самому большому типу.
    /// Если один из аргументов равен null, возвращается null.
    /// Если при перемножении целых чисел возникает переполнение, результат преобразуется к типу Double
    /// </summary>
    /// <param name="a">Первый аргумент</param>
    /// <param name="b">Второй аргумент</param>
    /// <returns>Результат умножения</returns>
    public static object MultiplyValues(object a, object b)
    {
      if (a == null || b == null)
        return null;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());

      if (st2 == SumType.TimeSpan && st1 != SumType.TimeSpan)
        return MultiplyValues(b, a); // рекурсивный вызов

      if (st1 == SumType.TimeSpan)
      {

        switch (st2)
        {
          case SumType.Int:
          case SumType.Int64:
            return new TimeSpan(((TimeSpan)a).Ticks * (long)ConvertValue(b, SumType.Int64));
          case SumType.Single:
          case SumType.Double:
          case SumType.Decimal:
            double dbl_1 = (double)(((TimeSpan)a).Ticks);
            double dbl_2 = (double)ConvertValue(b, SumType.Double);
            return new TimeSpan((long)(dbl_1 * dbl_2));
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть перемножены");
        }
      }

      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int:
            try
            {
              return (int)v1 * (int)v2;
            }
            catch (OverflowException)
            {
              return (double)v1 * (double)v2;
            }
          case SumType.Int64:
            try
            {
              return (long)v1 * (long)v2;
            }
            catch (OverflowException)
            {
              return (double)v1 * (double)v2;
            }
          case SumType.Single: return (float)v1 * (float)v2;
          case SumType.Double: return (double)v1 * (double)v2;
          case SumType.Decimal: return (decimal)v1 * (decimal)v2;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть перемножены");
        }
      }
    }


    /// <summary>
    /// Выполнить деление первого аргумента на второй
    /// Выполняется преобразование к самому большому типу.
    /// Если один из аргументов равен null, возвращается null.
    /// При делении целых чисел, если результат не является целым, выполняется преобразование к типу Double
    /// </summary>
    /// <param name="a">Делимое</param>
    /// <param name="b">Делитель</param>
    /// <returns>Частное</returns>
    public static object DivideValues(object a, object b)
    {
      if (a == null || b == null)
        return null;

      SumType st1 = GetSumType(a.GetType());
      SumType st2 = GetSumType(b.GetType());

      if (st1 == SumType.TimeSpan)
      {

        switch (st2)
        { 
          case SumType.Int:
          case SumType.Int64:
            return new TimeSpan( ((TimeSpan)a).Ticks / (long)ConvertValue(b, SumType.Int64));
          case SumType.Single:
          case SumType.Double:
          case SumType.Decimal:
            double dbl_1 = (double)(((TimeSpan)a).Ticks);
            double dbl_2 = (double)ConvertValue(b, SumType.Double);
            return new TimeSpan((long)(dbl_1 / dbl_2));
          case SumType.TimeSpan:
            return DivideValues(((TimeSpan)a).Ticks, ((TimeSpan)b).Ticks); // рекурсивный вызов
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть поделены");
        }
      }

      SumType st = GetLargestSumType(st1, st2);
      object v1 = ConvertValue(a, st);
      object v2 = ConvertValue(b, st);
      checked
      {
        switch (st)
        {
          case SumType.Int:
            int ai = (int)v1;
            int bi = (int)v2;
            int di = ai / bi;
            if (di * bi == ai)
              return di;
            else
              return (double)ai / (double)bi;
          case SumType.Int64:
            long al = (long)v1;
            long bl = (long)v2;
            long dl = al / bl;
            if (dl * bl == al)
              return dl;
            else
              return (double)al / (double)bl;
          case SumType.Single: return (float)v1 / (float)v2;
          case SumType.Double: return (double)v1 / (double)v2;
          case SumType.Decimal: return (decimal)v1 / (decimal)v2;
          default:
            throw new ArgumentException("Значения типа " + a.GetType() + " и " + b.GetType() + " не могут быть поделены");
        }
      }
    }


    /// <summary>
    /// Получить модуль (абсолютное значение аргумента. Используется метод Math.Abs()
    /// Если аргумент равен null, возвращается null.
    /// </summary>
    /// <param name="a">Аргумент</param>
    /// <returns>Отрицательное значение</returns>
    public static object AbsValue(object a)
    {
      if (a == null)
        return null;

      SumType st = GetSumType(a.GetType());
      object v = ConvertValue(a, st);
      checked
      {
        switch (st)
        {
          case SumType.Int: return Math.Abs((int)v);
          case SumType.Int64: return Math.Abs((long)v);
          case SumType.Single: return Math.Abs((float)v);
          case SumType.Double: return Math.Abs((double)v);
          case SumType.Decimal: return Math.Abs((decimal)v);
          case SumType.TimeSpan:
            TimeSpan ts = (TimeSpan)v;
            if (ts.Ticks < 0L)
              return -ts;
            else
              return ts;
          default:
            throw new ArgumentException("Для значения типа " + a.GetType() + " не может быть получено абсолютное значение");
        }
      }
    }

    #endregion

    #region IsInRange

    /// <summary>
    /// Возвращает true, если целое число находится в диапазоне значений.
    /// Поддерживаются полуоткрытые диапазоны
    /// </summary>
    /// <param name="testValue">Проверяемое значение</param>
    /// <param name="firstValue">Минимальное значение диапазона (включительно) или null</param>
    /// <param name="lastValue">Максимальное значение диапазона (включительно) или null</param>
    /// <returns>true, если значение входит в диапазон</returns>
    public static bool IsInRange<T>(T testValue, T? firstValue, T? lastValue)
      where T : struct, IComparable<T>
    {
      if (firstValue.HasValue)
      {
        if (testValue.CompareTo(firstValue.Value) < 0)
          return false;
      }
      if (lastValue.HasValue)
      {
        if (testValue.CompareTo(lastValue.Value) > 0)
          return false;
      }
      return true;
    }

    #endregion
  }
}
