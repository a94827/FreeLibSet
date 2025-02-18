// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

// Методы работы с масствами
namespace FreeLibSet.Core
{
  partial class DataTools
  {
    #region Пустые массивы

    /// <summary>
    /// Пустой масив объектов
    /// </summary>
    public static readonly object[] EmptyObjects = new object[0];

    /// <summary>
    /// Массив идентификаторов нулевой длины (<see cref="System.Int32"/>[0])
    /// </summary>
    public static readonly Int32[] EmptyIds = new Int32[0];

    /// <summary>
    /// Пустой массив строк
    /// </summary>
    public static readonly string[] EmptyStrings = new string[0];

    /// <summary>
    /// Пустой массив <see cref="System.Int32"/>
    /// </summary>
    public static readonly int[] EmptyInts = new int[0];

    /// <summary>
    /// Пустой массив <see cref="System.Int64"/>
    /// </summary>
    public static readonly long[] EmptyInt64s = new long[0];

    /// <summary>
    /// Пустой массив <see cref="System.Boolean"/>
    /// </summary>
    public static readonly bool[] EmptyBools = new bool[0];

    /// <summary>
    /// Пустой массив <see cref="System.Single"/>
    /// </summary>
    public static readonly float[] EmptySingles = new float[0];

    /// <summary>
    /// Пустой массив <see cref="System.Double"/>
    /// </summary>
    public static readonly double[] EmptyDoubles = new double[0];

    /// <summary>
    /// Пустой массив <see cref="System.Decimal"/>
    /// </summary>
    public static readonly decimal[] EmptyDecimals = new decimal[0];

    /// <summary>
    /// Пустой массив объектов <see cref="System.DateTime"/>
    /// </summary>
    public static readonly DateTime[] EmptyDateTimes = new DateTime[0];

    /// <summary>
    /// Пустой массив объектов <see cref="System.TimeSpan"/>
    /// </summary>
    public static readonly TimeSpan[] EmptyTimeSpans = new TimeSpan[0];

    /// <summary>
    /// Пустой массив объектов <see cref="System.Guid"/>
    /// </summary>
    public static readonly Guid[] EmptyGuids = new Guid[0];

    /// <summary>
    /// Пустой массив байт
    /// </summary>
    public static readonly byte[] EmptyBytes = new byte[0];

    /// <summary>
    /// Пустой массив направлений сортировки
    /// </summary>
    public static readonly ListSortDirection[] EmptySortDirections = new ListSortDirection[0];

    #endregion

    #region Создание массивов

    /// <summary>
    /// Заполнение всех элементов одномерного массива одинаковыми значениями
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="a">Массив</param>
    /// <param name="value">Значение, которое получит каждый элемент массива</param>
    public static void FillArray<T>(T[] a, T value)
    {
      for (int i = 0; i < a.Length; i++)
        a[i] = value;
    }

    /// <summary>
    /// Заполнение всех элементов двумерного массива одинаковыми значениями
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="a">Массив</param>
    /// <param name="value">Значение, которое получит каждый элемент массива</param>
    public static void FillArray2<T>(T[,] a, T value)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      int m1 = a.GetLowerBound(1);
      int m2 = a.GetUpperBound(1);
      for (int i = n1; i <= n2; i++)
        for (int j = m1; j <= m2; j++)
          a[i, j] = value;
    }

    /// <summary>
    /// Создание массива произвольного типа, заполненного одинаковыми значениями
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="n">Количество элементов, которые будут в массиве</param>
    /// <param name="value">Значение, которое будет записано во все элементы массива</param>
    /// <returns>Созданный массив</returns>
    public static T[] CreateArray<T>(int n, T value)
    {
      T[] a = new T[n];
      for (int i = 0; i < a.Length; i++)
        a[i] = value;
      return a;
    }

    // Убрано 26.07.2019
    ///// <summary>
    ///// Создание массива произвольного типа из интерфейса ICollection
    ///// </summary>
    ///// <typeparam name="T">Тип данных</typeparam>
    ///// <returns>Созданный массив</returns>
    //public static T[] CreateArray<T>(ICollection<T> Source)
    //{
    //  T[] a = new T[Source.Count];
    //  int cnt = 0;
    //  foreach (T Item in Source)
    //  {
    //    a[cnt] = Item;
    //    cnt++;
    //  }
    //  return a;
    //}

    /// <summary>
    /// Создание массива произвольного типа из интерфейса <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <returns>Созданный массив</returns>
    public static T[] CreateArray<T>(IEnumerable<T> source)
    {
      //int cnt = 0;
      //foreach (T Item in Source)
      //  cnt++;

      //T[] a = new T[cnt];
      //cnt = 0;
      //foreach (T Item in Source)
      //{
      //  a[cnt] = Item;
      //  cnt++;
      //}
      //return a;

      // 26.07.2019

      //List<T> lst = new List<T>();
      // 27.05.2022 - ленивое создание списка
      List<T> lst = null;
      foreach (T item in source)
      {
        if (lst == null)
          lst = new List<T>();
        lst.Add(item);
      }

      if (lst == null)
        return new T[0];
      else
        return lst.ToArray();
    }

    /// <summary>
    /// Создает массив <see cref="System.Object"/> из нетипизированного перечислителя
    /// <see cref="System.Collections.IEnumerable"/>
    /// </summary>
    /// <param name="source">Перечислимый объект</param>
    /// <returns>Массив</returns>
    public static object[] CreateObjectArray(System.Collections.IEnumerable source)
    {
      List<object> lst = null;
      foreach (object item in source)
      {
        if (lst == null)
          lst = new List<object>();
        lst.Add(item);
      }

      if (lst == null)
        return DataTools.EmptyObjects;
      else
        return lst.ToArray();
    }


    /// <summary>
    /// Получение массива, заполненного значенияи <see cref="DBNull"/>
    /// </summary>
    /// <param name="n">Требуемая длина массива</param>
    /// <returns>Массив объектов</returns>
    public static object[] CreateDBNullArray(int n)
    {
      object[] res = new object[n];
      for (int i = 0; i < n; i++)
        res[i] = DBNull.Value;
      return res;
    }

    /// <summary>
    /// Создание массива, содержащего элементы массива a, для которых установлен
    /// соответствующий флаг в массиве <paramref name="flags"/>.
    /// Применяется для извлечения выбранных идентификаторов из первоначального
    /// списка.
    /// </summary>
    /// <typeparam name="T">Тип данных исходного и конечного массивов</typeparam>
    /// <param name="a">Исходный массив, из которого берется значение</param>
    /// <param name="flags">Массив флагов. Должен иметь ту же длину, что и массив <paramref name="a"/>.</param>
    /// <returns>Массив, содержащий выбранные элементы из <paramref name="a"/></returns>
    public static T[] CreateSelectedArray<T>(T[] a, bool[] flags)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (flags == null)
        throw new ArgumentNullException("flags");
#endif
      if (flags.Length != a.Length)
        throw ExceptionFactory.ArgWrongCollectionCount("flags", flags, a.Length);

      // Подсчитываем число установленных флагов
      int i;
      int cnt = 0;
      for (i = 0; i < flags.Length; i++)
      {
        if (flags[i])
          cnt++;
      }

      T[] a2 = new T[cnt];
      cnt = 0;
      for (i = 0; i < flags.Length; i++)
      {
        if (flags[i])
        {
          a2[cnt] = a[i];
          cnt++;
        }
      }
      return a2;
    }

    #endregion

    #region Сравнение массивов

    /// <summary>
    /// Сравнение двух произвольных одномерных массивов
    /// Возвращает true, если массивы имеют одинаковую длину и для каждой
    /// пары элементов Equals() возвращает true.
    /// Если обе ссылки на массивы равны null, возвращается true.
    /// Если только одна из ссылок равна null, возвращается false.
    /// </summary>
    /// <param name="a">Первый сравниваемый массив</param>
    /// <param name="b">Второй сравниваемый массив</param>
    /// <returns>true, если массивы совпадают.</returns>
    public static bool AreArraysEqual(Array a, Array b)
    {
      if (a == null && b == null)
        return true;
      if (a == null || b == null)
        return false; // 13.12.2016

      /*
      if (a.Length != b.Length)
        return false;
      for (int i = 0; i < a.Length; i++)
      {
        if (!Object.Equals(a.GetValue(i), b.GetValue(i)))
          return false;
      }
      return true;
       * */

      // Проверяем размерности
      if (a.Rank != b.Rank)
        return false;
      for (int i = 0; i < a.Rank; i++)
      {
        if (a.GetLowerBound(i) != b.GetLowerBound(i) ||
          a.GetUpperBound(i) != b.GetUpperBound(i))
          return false;
      }

      IEnumerator en1 = a.GetEnumerator();
      IEnumerator en2 = b.GetEnumerator();

      while (en1.MoveNext())
      {
        en2.MoveNext();
        if (!Object.Equals(en1.Current, en2.Current))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Сравнение двух произвольных одномерных массивов
    /// Возвращает true, если массивы имеют одинаковую длину и для каждой
    /// пары элементов Equals() возвращает true.
    /// Если обе ссылки на массивы равны null, возвращается true.
    /// Если только одна из ссылок равна null, возвращается false.
    /// Для строковых массивов можно использовать метод AreStringArrayEqual()
    /// </summary>
    /// <typeparam name="T">Тип значений, хранящихся в массивах</typeparam>
    /// <param name="a">Первый сравниваемый массив</param>
    /// <param name="b">Второй сравниваемый массив</param>
    /// <returns>true, если массивы совпадают.</returns>
    public static bool AreArraysEqual<T>(T[] a, T[] b)
    {
      if (a == null && b == null)
        return true;
      if (a == null || b == null)
        return false; // 13.12.2016
      if (a.Length != b.Length)
        return false;
      for (int i = 0; i < a.Length; i++)
      {
        if (!Object.Equals(a[i], b[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Сравнение двух одномерных массивов строк
    /// Возвращает true, если массивы имеют одинаковую длину и для каждой
    /// пары элементов значения совпадают.
    /// Если обе ссылки на массивы равны null, возвращается true.
    /// Если только одна из ссылок равна null, возвращается false.
    /// </summary>
    /// <param name="a">Первый сравниваемый массив</param>
    /// <param name="b">Второй сравниваемый массив</param>
    /// <param name="comparisonType">Режим сравнения строк</param>
    /// <returns>true, если массивы совпадают.</returns>
    public static bool AreArraysEqual(string[] a, string[] b, StringComparison comparisonType)
    {
      if (a == null && b == null)
        return true;
      if (a == null || b == null)
        return false;
      if (a.Length != b.Length)
        return false;
      for (int i = 0; i < a.Length; i++)
      {
        if (!String.Equals(a[i], b[i], comparisonType)) // испр. 16.12.2021
          return false;
      }
      return true;
    }

    #endregion

    #region Объединение массивов

    /// <summary>
    /// Объединение нескольких одномерных массивов произвольного типа в один.
    /// Значения никак не обрабатываются. Получается одномерный массив с длиной,
    /// равной суммарной длине исходных массивов.
    /// Этот метод также может преобразовать jagged-массив в одномерный, если проигнорировать слово params.
    /// Некоторые аргументы (элементы jagged-массива) могут иметь значение null. Они игнорируются.
    /// Если же в самих массивах есть элементы, равные null, то они добавляются в конечный массив.
    /// </summary>
    /// <param name="arrays">Массивы, подлежащие объединению в один</param>
    /// <returns>Одномерный массив</returns>
    public static T[] MergeArrays<T>(params T[][] arrays)
    {
      // Длина массива
      int n = 0;
      for (int i = 0; i < arrays.Length; i++)
      {
        if (arrays[i] == null)
          continue;
        n += arrays[i].Length;
      }

      // Результат
      T[] res = new T[n];

      // Копирование элементов
      n = 0;
      for (int i = 0; i < arrays.Length; i++)
      {
        if (arrays[i] == null)
          continue;
        Array.Copy(arrays[i], 0, res, n, arrays[i].Length);
        n += arrays[i].Length;
      }
      return res;
    }

    /// <summary>
    /// Объединить два массива, отбрасывая повторы элементов.
    /// Предполагается, что исходные массивы <paramref name="array1"/> и <paramref name="array2"/> повторов не содержат.
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="array1">Первый массив</param>
    /// <param name="array2">Второй массив</param>
    /// <returns>Результирующий массив</returns>
    public static T[] MergeArraysOnce<T>(T[] array1, T[] array2)
    {
      if (array1 == null)
        array1 = new T[0];
      if (array2 == null)
        array2 = new T[0];

      if (array2.Length == 0)
        return array1;
      if (array1.Length == 0)
        return array2;

      // Оба массива заполнены
      ArrayIndexer<T> indexer1 = null;
      if (array1.Length > 3)
        indexer1 = new ArrayIndexer<T>(array1);

      List<T> lstRes = null;
      for (int i = 0; i < array2.Length; i++)
      {
        bool mustAdd;
        if (indexer1 == null)
          mustAdd = Array.IndexOf<T>(array1, array2[i]) < 0;
        else
          mustAdd = !indexer1.Contains(array2[i]);

        if (mustAdd)
        {
          // Требуется добавить элемент из массива Array2
          if (lstRes == null)
          {
            lstRes = new List<T>(array1.Length + 1);
            lstRes.AddRange(array1);
          }
          lstRes.Add(array2[i]);
        }
      }
      if (lstRes == null) // были только повторы
        return array1;
      else
        return lstRes.ToArray();
    }

    /// <summary>
    /// Создать массив с элементами, которые присутствуют и в массиве <paramref name="array1"/> и в <paramref name="array2"/> .
    /// Предполагается, что исходные массивы <paramref name="array1"/> и <paramref name="array2"/>  повторов не содержат.
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="array1">Первый массив</param>
    /// <param name="array2">Второй массив</param>
    /// <returns>Результирующий массив</returns>
    public static T[] MergeArraysBoth<T>(T[] array1, T[] array2)
    {
      if (array1 == null)
        array1 = new T[0];
      if (array2 == null)
        array2 = new T[0];
      if (array1.Length == 0)
        return array1;
      if (array2.Length == 0)
        return array2;

      List<T> lstRes = null;
      ArrayIndexer<T> indexer2 = null;
      if (array2.Length > 3)
        indexer2 = new ArrayIndexer<T>(array2);

      for (int i = 0; i < array1.Length; i++)
      {
        bool mustAdd;
        if (indexer2 == null)
          mustAdd = Array.IndexOf<T>(array2, array1[i]) >= 0;
        else
          mustAdd = indexer2.Contains(array1[i]);

        if (mustAdd)
        {
          if (lstRes == null)
            lstRes = new List<T>();
          lstRes.Add(array1[i]);
        }
      }
      if (lstRes == null)
        return new T[0];
      else
        return lstRes.ToArray();
    }

    #endregion

    #region Прочие методы работы с массивами

    /// <summary>
    /// Удалить из массива <paramref name="a"/> элементы, содержащиеся в массиве <paramref name="removingArray"/>.
    /// Если в исходном массиве <paramref name="a"/> не найдено ни одного элемента, никаких действий не выполняется
    /// и ссылка на существующий массив не меняется.
    /// Таким образом, по ссылке может быть возвращена как копия массива, так и оригинал.
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="a">Массив (по ссылке) из которого удаляются элементы. Не может быть null</param>
    /// <param name="removingArray">Массив элементов, которые надо удалить</param>
    /// <returns>Массив, меньший или равный <paramref name="a"/></returns>
    public static void RemoveFromArray<T>(ref T[] a, T[] removingArray)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      if (removingArray == null)
        return;
      if (removingArray.Length == 0)
        return;

      List<T> resArray = null;
      for (int i = 0; i < removingArray.Length; i++)
      {
        if (Array.IndexOf<T>(a, removingArray[i]) >= 0)
        {
          // Требуется удалить элемент из массива SrcArray
          if (resArray == null)
          {
            resArray = new List<T>(a.Length);
            resArray.AddRange(a);
          }
          resArray.Remove(removingArray[i]);
        }
      }
      if (resArray != null)
        a = resArray.ToArray();
    }

    /// <summary>
    /// Удалить из массива <paramref name="a"/> <paramref name="count"/> элементов, начиная с позиции <paramref name="startPosition"/>.
    /// <paramref name="startPosition"/> и <paramref name="count"/> должны находиться в пределах массива.
    /// Если <paramref name="count"/>=0, никаких действий не выполняется и ссылка <paramref name="a"/> не меняется.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве</typeparam>
    /// <param name="a">Изменяемый массив (по ссылке). Не может быть null</param>
    /// <param name="startPosition">Позиция первого удаляемого элемента.</param>
    /// <param name="count">Количество удаляемых элементов</param>
    public static void DeleteFromArray<T>(ref T[] a, int startPosition, int count)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      if (startPosition < 0 || startPosition > a.Length)
        throw ExceptionFactory.ArgOutOfRange("startPosition", startPosition, 0, a.Length);
      if (count < 0 || (startPosition + count) > a.Length)
        throw ExceptionFactory.ArgOutOfRange("count", count, 0, a.Length-startPosition);

      if (count == 0)
        return; // не надо ничего делать.

      T[] b = new T[a.Length - count];
      if (startPosition > 0)
        Array.Copy(a, 0, b, 0, startPosition); // часть до удаления
      if ((startPosition + count) < a.Length)
        Array.Copy(a, startPosition + count, b, startPosition, a.Length - startPosition - count); // часть после удаления

      a = b;
    }


    /// <summary>
    /// Вставить в массив <paramref name="a"/>, начиная с позиции <paramref name="startPosition"/>,
    /// все элементы из массива <paramref name="insertingArray"/>.
    /// <paramref name="startPosition"/> должна находиться в пределах массива <paramref name="a"/> или
    /// быть равна <paramref name="a"/>.Length для добавления в конец массива.
    /// Если <paramref name="insertingArray"/>=null или пустой массива, никаких действий не выполняется и ссылка <paramref name="a"/> не меняется.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве</typeparam>
    /// <param name="a">Изменяемый массив (по ссылке). Не может быть null</param>
    /// <param name="startPosition">Позиция первого удаляемого элемента.</param>
    /// <param name="insertingArray">Добавляемые элементы</param>
    public static void InsertIntoArray<T>(ref T[] a, int startPosition, T[] insertingArray)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      if (startPosition < 0 || startPosition > a.Length)
        throw ExceptionFactory.ArgOutOfRange("startPosition", startPosition, 0, a.Length);

      if (insertingArray == null)
        return;
      if (insertingArray.Length == 0)
        return;

      T[] b = new T[a.Length + insertingArray.Length];
      if (startPosition > 0)
        Array.Copy(a, 0, b, 0, startPosition); // начало массива
      Array.Copy(insertingArray, 0, b, startPosition, insertingArray.Length); // вставка
      if (startPosition < a.Length)
        Array.Copy(a, startPosition, b, startPosition + insertingArray.Length, a.Length - startPosition); // конец массива

      a = b;
    }

    /// <summary>
    /// Возвращает первый элемент одномерного массива или null (значение по умолчанию для структур), если массив пустой
    /// </summary>
    /// <typeparam name="T">Тип элементов в массиве</typeparam>
    /// <param name="a">Массив</param>
    /// <returns>Первый элемент</returns>
    public static T FirstItem<T>(T[] a)
    {
      if (a == null)
        return default(T);
      if (a.Length == 0)
        return default(T);
      return a[0];
    }

    /// <summary>
    /// Возвращает последний элемент одномерного массива или null (значение по умолчанию для структур), если массив пустой
    /// </summary>
    /// <typeparam name="T">Тип элементов в массиве</typeparam>
    /// <param name="a">Массив</param>
    /// <returns>Последний элемент</returns>
    public static T LastItem<T>(T[] a)
    {
      if (a == null)
        return default(T);
      if (a.Length == 0)
        return default(T);
      return a[a.Length - 1];
    }

    /// <summary>
    /// Возвращает true, если массив <paramref name="a1"/> содержит первые элементы, совпадающие с <paramref name="a2"/> (по аналогии с методом String.StartsWith).
    /// Для сравнения используется метод Object.Equals()
    /// </summary>
    /// <typeparam name="T">Тип значений, хранящихся в массиве</typeparam>
    /// <param name="a1">Проверяемый массив</param>
    /// <param name="a2">Начальные элементы массива</param>
    /// <returns>true, если начало <paramref name="a1"/> совпадает с <paramref name="a2"/></returns>
    public static bool ArrayStartsWith<T>(T[] a1, T[] a2)
    {
      if (a2.Length > a1.Length)
        return false;

      for (int i = 0; i < a2.Length; i++)
      {
        if (!(a1[i].Equals(a2[i])))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если массив <paramref name="a1"/> содержит последние элементы, совпадающие с <paramref name="a2"/> (по аналогии с методом String.EndsWith).
    /// Для сравнения используется метод Object.Equals()
    /// </summary>
    /// <typeparam name="T">Тип значений, хранящихся в массиве</typeparam>
    /// <param name="a1">Проверяемый массив</param>
    /// <param name="a2">Последние элементы массива</param>
    /// <returns>true, если конец <paramref name="a1"/> совпадает с <paramref name="a2"/></returns>
    public static bool ArrayEndsWith<T>(T[] a1, T[] a2)
    {
      if (a2.Length > a1.Length)
        return false;

      int off1 = a1.Length - a2.Length;

      for (int i = 0; i < a2.Length; i++)
      {
        if (!(a1[i + off1].Equals(a2[i])))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Получение одномерного массива (строки) из двумерного массива.
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/>. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве (произвольный)</typeparam>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="rowIndex">Индекс строки</param>
    /// <returns>Одномерный массив</returns>
    public static T[] GetArray2Row<T>(T[,] a, int rowIndex)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (rowIndex < a.GetLowerBound(0) || rowIndex > a.GetUpperBound(0))
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, a.GetLowerBound(0), a.GetUpperBound(0));
#endif

      int n1 = a.GetLowerBound(1);
      int n2 = a.GetUpperBound(1);
      T[] res = new T[n2 - n1 + 1];
      for (int i = n1; i <= n2; i++)
        res[i - n1] = a[rowIndex, i];
      return res;
    }

    /// <summary>
    /// Получение одномерного массива (столбца) из двумерного массива.
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/>. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Тип данных в массиве (произвольный)</typeparam>
    /// <param name="a">Исходный двумерный массив</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>Одномерный массив</returns>
    public static T[] GetArray2Column<T>(T[,] a, int columnIndex)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (columnIndex < a.GetLowerBound(1) || columnIndex > a.GetUpperBound(1))
        throw ExceptionFactory.ArgOutOfRange("columnIndex", columnIndex, a.GetLowerBound(1), a.GetUpperBound(1));
#endif

      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      T[] res = new T[n2 - n1 + 1];
      for (int i = n1; i <= n2; i++)
        res[i - n1] = a[i, columnIndex];
      return res;
    }

    /// <summary>
    /// Получение одномерного массива из двумерного путем его построчного "разворачивания".
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/>. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Двумерный массив</param>
    /// <returns>Одномерный массив</returns>
    public static T[] ToArray1<T>(T[,] a)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      int m1 = a.GetLowerBound(1);
      int m2 = a.GetUpperBound(1);

      T[] res = new T[(n2 - n1 + 1) * (m2 - m1 + 1)];

      int pos = 0;
      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          res[pos] = a[i, j];
          pos++;
        }
      }
      return res;
    }


    /// <summary>
    /// Получение одномерного массива из jagged-массива путем его построчного "разворачивания".
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/> и вложенных массивов. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Двумерный массив</param>
    /// <returns>Одномерный массив</returns>
    public static T[] ToArray1<T>(T[][] a)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);

      int len = 0;
      for (int i = n1; i <= n2; i++)
      {
        if (a[i] == null)
          continue;
        len += a[i].Length;
      }

      T[] res = new T[len];

      len = 0;
      for (int i = n1; i <= n2; i++)
      {
        if (a[i] == null)
          continue;
        Array.Copy(a[i], a[i].GetLowerBound(0), res, len, a[i].Length);
        len += a[i].Length;
      }

      return res;
    }

    /// <summary>
    /// Получение одномерного массива из двумерного jagged-массива путем его построчного "разворачивания".
    /// Учитываются значения, возвращаемые Array.GetLowerBound() и GetUpperBound() для исходного массива <paramref name="a"/> и вложенных массивов. 
    /// При этом возвращаемый одномерный массив является простым 0-базированным массивом, независимо от базы исходного массива.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Двумерный массив</param>
    /// <returns>Одномерный массив</returns>
    public static T[] ToArray1<T>(T[,][] a)
    {
      int n1 = a.GetLowerBound(0);
      int n2 = a.GetUpperBound(0);
      int m1 = a.GetLowerBound(1);
      int m2 = a.GetUpperBound(1);

      int len = 0;
      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          if (a[i, j] == null)
            continue;
          len += a[i, j].Length;
        }
      }

      T[] res = new T[len];

      len = 0;
      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          if (a[i, j] == null)
            continue;
          Array.Copy(a[i, j], a[i, j].GetLowerBound(0), res, len, a[i, j].Length);
          len += a[i, j].Length;
        }
      }

      return res;
    }

    /// <summary>
    /// Преобразование Jagged-массива в прямоугольный двумерный массив.
    /// Первая размерность результирующего массива (количество строк) равна длине массива <paramref name="a"/>.
    /// Вторая размерность результирующего массива (количество столбцов) определяется как максимальная длина среди вложенных массивов.
    /// Если длина вложенных массивов различается, то недостающие элементы результирующего массива будут иметь пустое значение.
    /// Ссылка на массив <paramref name="a"/> не может быть равна null, то может быть пустым массивом или содержать ссылки null вместо вложенных массивов
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="a">Jagged массив</param>
    /// <returns>Двумерный массив</returns>
    public static T[,] ToArray2<T>(T[][] a)
    {
      int m = 0;
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
          m = Math.Max(m, a[i].Length);
      }

      T[,] res = new T[a.Length, m];
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i] != null)
        {
          for (int j = 0; j < a[i].Length; j++)
            res[i, j] = a[i][j];
        }
      }
      return res;
    }


    /// <summary>
    /// Получить двумерный массив из одномерного.
    /// Порядок элементов в полученных массивах порядку строк в исходном массиве
    /// Возвращается двумерный jagged-массив, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// Предполагается, что полученный массив не будет меняться. Если он содержит единственный подмассив (при <paramref name="a"/>.Length меньше или равном <paramref name="n"/> разбиение не требуется),
    /// то ссылка на подмассив совпадает с <paramref name="a"/>. В этом случае изменения в подмассиве затронут исходный массив.
    /// Если разбиение требуется только для однократного перебора элементов массива, используйте класс ArraySegmentEnumerator.
    /// Он работает быстрее, так как не выполняет копирование элементов.
    /// Если же при переборе требуется создавать подмассивы как отдельные объекты, то выгодно использовать ArrayBlockEnumerator.
    /// </summary>
    /// <typeparam name="T">Произвольный тип данных</typeparam>
    /// <param name="a">Исходный одномерный массив</param>
    /// <param name="n">Длина подмассивов</param>
    /// <returns>Jagged-массив</returns>
    public static T[][] GetBlockedArray<T>(T[] a, int n)
    {
      if (a == null)
        throw new ArgumentNullException("a");

      if (n < 1)
        throw ExceptionFactory.ArgOutOfRange("n", n, 1, null);

      if (a.Length <= n)
        return new T[1][] { a };

      int nn = ((a.Length + (n - 1))) / n;

      T[][] res = new T[nn][];

      int cnt = 0;
      for (int i = 0; i < nn; i++)
      {
        if (i == (nn - 1))
          res[i] = new T[a.Length - cnt];
        else
          res[i] = new T[n];
        Array.Copy(a, cnt, res[i], 0, res[i].Length);
        cnt += n;
      }

      return res;
    }


    /// <summary>
    /// Возвращает количество элементов произвольного массива, не равных null.
    /// Может рекурсивно обрабатывать Jagged-массивы.
    /// Предполагается, что массив содержит объекты а не ValueType, иначе метод просто вернет общее количество элементов.
    /// </summary>
    /// <param name="a">Массив с произвольным числом измерений</param>
    /// <param name="proceedJaggedArrays">Если true, то при переборе элементов проверяется,
    /// не является ли элемент сам массивом. Если это вложенный (jagged) массив, то учитываются все его элементы.
    /// Если false, то вложенные массивы не определяются и считаются за один элемент</param>
    /// <returns>Количество элементов</returns>
    public static int GetArrayNotNullCount(Array a, bool proceedJaggedArrays)
    {
      int cnt = 0;
      foreach (object item in a)
      {
        if (!Object.ReferenceEquals(item, null))
        {
          if (proceedJaggedArrays)
          {
            Array a2 = item as Array;
            if (a2 != null)
              cnt += GetArrayNotNullCount(a2, true);
            else
              cnt++;
          }
          else
            cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Возвращает количество элементов произвольного массива, равных null.
    /// Может рекурсивно обрабатывать Jagged-массивы.
    /// Предполагается, что массив содержит объекты а не ValueType, иначе метод вернет 0.
    /// </summary>
    /// <param name="a">Массив с произвольным числом измерений</param>
    /// <param name="proceedJaggedArrays">Если true, то при переборе элементов проверяется,
    /// не является ли элемент сам массивом. Если это вложенный (jagged) массив, то учитываются все его элементы.
    /// Если false, то вложенные массивы считаются обычными элементами и пропускаются</param>
    /// <returns>Количество элементов</returns>
    public static int GetArrayNullCount(Array a, bool proceedJaggedArrays)
    {
      int cnt = 0;
      foreach (object item in a)
      {
        if (Object.ReferenceEquals(item, null))
          cnt++;
        else if (proceedJaggedArrays)
        {
          Array a2 = item as Array;
          if (a2 != null)
            cnt += GetArrayNullCount(a2, true);
        }
      }
      return cnt;
    }

    /// <summary>
    /// Копирование произвольной коллекции в одномерный массив.
    /// Для записи значения вызывается метод Array.SetValue(), который может выполнять преобразование значений к нужному типу.
    /// Этот метод, в основном, предназначен для реализации метода ICollection.CopyTo() в коллекциях.
    /// Все значения <paramref name="source"/> должны поместиться в массиве <paramref name="array"/>.
    /// См. справку для метода System.Collections.ICollection.CopyTo(Array, Int32).
    /// В случае, если источник не помещается в массив, то после выброса исключения, массив <paramref name="array"/> может оказаться заполненным частично
    /// </summary>
    /// <param name="source">Перечислимый источник данных. Не может быть null</param>
    /// <param name="array">Заполняемый массив. Должен быть одномерным и начинаться с 0.</param>
    /// <param name="arrayIndex">Индекс первой заполняемой позиции в массиве <paramref name="array"/>.
    /// Должен быть в диапазоне от 0 до <paramref name="array"/>.Length - 1</param>
    public static void CopyToArray(IEnumerable source, Array array, int arrayIndex)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (array==null)
        throw new ArgumentNullException("array");
      foreach (object item in source)
      {
        array.SetValue(item, arrayIndex);
        arrayIndex++;
      }
    }

#if XXX // Есть CreateArray()
    /// <summary>
    /// Создает массив из объекта ICollection, используя CopyTo().
    /// Удобно использовать, например, для получения списка ключей и значений словарей, когда
    /// свойства Keys и Values возвращают только типизированный интерфейс ICollection.
    /// Не используйте метод для асинхронных словарей.
    /// </summary>
    /// <typeparam name="T">Тип значений, хранщихся в коллекции</typeparam>
    /// <param name="source">Исходная коллекция</param>
    public static T[] ToArray<T>(ICollection<T> source)
    {
      T[] a = new T[source.Count];
      source.CopyTo(a, 0);
      return a;
    }

    /// <summary>
    /// Создает массив из перечислителя 
    /// </summary>
    /// <typeparam name="T">Тип значений, хранщихся в коллекции</typeparam>
    /// <param name="source">Исходная коллекция</param>
    /// <returns>Массив значений</returns>
    public static T[] ToArray<T>(IEnumerable<T> source)
    {
      List<T> lst = new List<T>();
      foreach (T item in source)
        lst.Add(item);
      return lst.ToArray();
    }
#endif

    #endregion
  }
}
