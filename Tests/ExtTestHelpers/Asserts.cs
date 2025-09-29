using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using NUnit.Framework;

namespace FreeLibSet.Tests
{
  /// <summary>
  /// Object comparer with public properties inspection
  /// </summary>
  public static class PropertyAssert
  {
    #region ProperiesEqual()

    /// <summary>
    /// Проверяет, что объекты имеют одинаковый тип или тип актуального объекта выводится из типа ожидаемого объекта.
    /// Далее проверяются значения всех public-свойств.
    /// Рекурсивная проверка не выполняется.
    /// Перечислитель не учитывается.
    /// </summary>
    /// <param name="expected">Ожидаемый объект</param>
    /// <param name="actual">Актуальный объект</param>
    /// <param name="message">Сообщение</param>
    /// <param name="args">Аргументы для форматирования сообщения</param>
    public static void AreEqual(object expected, object actual)
    {
      AreEqual(expected, actual, String.Empty, EmptyArray<Object>.Empty);
    }

    /// <summary>
    /// Проверяет, что объекты имеют одинаковый тип или тип актуального объекта выводится из типа ожидаемого объекта.
    /// Далее проверяются значения всех public-свойств.
    /// Рекурсивная проверка не выполняется.
    /// Перечислитель не учитывается.
    /// </summary>
    /// <param name="expected">Ожидаемый объект</param>
    /// <param name="actual">Актуальный объект</param>
    /// <param name="message">Сообщение</param>
    public static void AreEqual(object expected, object actual, string message)
    {
      AreEqual(expected, actual, message, EmptyArray<Object>.Empty);
    }

    /// <summary>
    /// Проверяет, что объекты имеют одинаковый тип или тип актуального объекта выводится из типа ожидаемого объекта.
    /// Далее проверяются значения всех public-свойств.
    /// Рекурсивная проверка не выполняется.
    /// Перечислитель не учитывается.
    /// </summary>
    /// <param name="expected">Ожидаемый объект</param>
    /// <param name="actual">Актуальный объект</param>
    /// <param name="message">Сообщение</param>
    /// <param name="args">Аргументы для форматирования сообщения</param>
    public static void AreEqual(object expected, object actual, string message, params object[] args)
    {
      AreEqualExcept(expected, actual, EmptyArray<string>.Empty, message, args);
    }

    #endregion

    #region ProperiesEqualExcept()

    /// <summary>
    /// Проверяет, что объекты имеют одинаковый тип или тип актуального объекта выводится из типа ожидаемого объекта.
    /// Далее проверяются значения всех public-свойств, за исключением пропускаемых.
    /// Рекурсивная проверка не выполняется.
    /// Перечислитель не учитывается.
    /// </summary>
    /// <param name="expected">Ожидаемый объект</param>
    /// <param name="actual">Актуальный объект</param>
    /// <param name="exceptedProperties">Имена свойств, которые не надо сравнивать. Может быть null</param>
    /// <param name="message">Сообщение</param>
    /// <param name="args">Аргументы для форматирования сообщения</param>
    public static void AreEqualExcept(object expected, object actual, string[] exceptedProperties)
    {
      AreEqualExcept(expected, actual, exceptedProperties, String.Empty, EmptyArray<Object>.Empty);
    }

    /// <summary>
    /// Проверяет, что объекты имеют одинаковый тип или тип актуального объекта выводится из типа ожидаемого объекта.
    /// Далее проверяются значения всех public-свойств, за исключением пропускаемых.
    /// Рекурсивная проверка не выполняется.
    /// Перечислитель не учитывается.
    /// </summary>
    /// <param name="expected">Ожидаемый объект</param>
    /// <param name="actual">Актуальный объект</param>
    /// <param name="exceptedProperties">Имена свойств, которые не надо сравнивать. Может быть null</param>
    /// <param name="message">Сообщение</param>
    public static void AreEqualExcept(object expected, object actual, string[] exceptedProperties, string message)
    {
      AreEqualExcept(expected, actual, exceptedProperties, message, EmptyArray<Object>.Empty);
    }

    /// <summary>
    /// Проверяет, что объекты имеют одинаковый тип или тип актуального объекта выводится из типа ожидаемого объекта.
    /// Далее проверяются значения всех public-свойств, за исключением пропускаемых.
    /// Рекурсивная проверка не выполняется.
    /// Перечислитель не учитывается.
    /// </summary>
    /// <param name="expected">Ожидаемый объект</param>
    /// <param name="actual">Актуальный объект</param>
    /// <param name="exceptedProperties">Имена свойств, которые не надо сравнивать. Может быть null</param>
    /// <param name="message">Сообщение</param>
    /// <param name="args">Аргументы для форматирования сообщения</param>
    public static void AreEqualExcept(object expected, object actual, string[] exceptedProperties, string message, params object[] args)
    {
      if (Object.ReferenceEquals(expected, actual))
        return;
      if (expected == null || actual == null)
        Assert.AreEqual(expected, actual, message, args); // здесь проверка закончится

      Type t1 = expected.GetType();
      Type t2 = actual.GetType();
      if (!Object.ReferenceEquals(t1, t2))
      {
        if (!t2.IsSubclassOf(t1))
          Assert.AreEqual(t1, t2, message + ". Types are different", args);
      }

      if (exceptedProperties == null)
        exceptedProperties = EmptyArray<string>.Empty;
      StringArrayIndexer exPropIndexer = new StringArrayIndexer(exceptedProperties);

      PropertyInfo[] aProps = t1.GetProperties(BindingFlags.Instance | BindingFlags.Public);
      for (int i = 0; i < aProps.Length; i++)
      {
        if (exPropIndexer.Contains(aProps[i].Name))
          continue;

        object v1, v2;
        Exception ex1, ex2;
        GetProperty(aProps[i], expected, out v1, out ex1);
        GetProperty(aProps[i], actual, out v2, out ex2);

        if (ex1 == null && ex2 == null)
          Assert.AreEqual(v1, v2, message + "." + aProps[i].Name, args);
        else
        {
          if (ex1 != null && ex2 != null)
            continue;
          if (ex1 != null)
            Assert.Fail(message + ". Exception for property " + aProps[i].Name + " in expected object: " + ex1.Message, args);
          else
            Assert.Fail(message + ". Exception for property " + aProps[i].Name + " in actual object: " + ex2.Message, args);
        }
      }
    }

    #endregion


    [DebuggerStepThrough]
    private static void GetProperty(PropertyInfo propertyInfo, object obj, out object v, out Exception ex)
    {
#if DEBUG
      if (propertyInfo == null)
        throw new ArgumentNullException("propertyInfo");
      if (obj == null)
        throw new ArgumentNullException("obj");
#endif

      v = null;
      ex = null;
      try
      {
        v = propertyInfo.GetValue(obj, EmptyArray<Object>.Empty);
      }
      catch (Exception e)
      {
        ex = e;
      }
    }


#if XXX
    public static void AreEqual(object expected, object actual)
    {
      AreEqual(expected, actual, String.Empty);
    }

    public static void AreEqual(object expected, object actual, string message)
    {
      ArrayList testedObjects = new ArrayList();
      DoAreEqual(testedObjects, expected, actual, message);
    }
    private static void DoAreEqual(ArrayList testedObjects, object expected, object actual, string message)
    {
      if (expected == null && actual == null)
        return;
      if (expected == null || actual == null)
        Assert.AreEqual(expected, actual, message);

      if (Object.ReferenceEquals(expected, actual))
        return;

      if (testedObjects.Contains(expected))
        return; // уже проверяли
      testedObjects.Add(expected);

      Type t1 = expected.GetType();
      Type t2 = actual.GetType();
      Assert.AreEqual(t1, t2, message + ". Types are different");

      if (t1.IsPrimitive || expected is Decimal || expected is DateTime || expected is TimeSpan)
      {
        Assert.AreEqual(expected, actual, message);
        return;
      }

    #region Свойства

      PropertyInfo[] aProps = t1.GetProperties(BindingFlags.Instance | BindingFlags.Public);
      for (int i = 0; i < aProps.Length; i++)
      {
        object v1, v2;
        Exception ex1, ex2;
        GetProperty(aProps[i], expected, out v1, out ex1);
        GetProperty(aProps[i], actual, out v2, out ex2);

        if (ex1 == null && ex2 == null)
          DoAreEqual(testedObjects, v1, v2, message + "." + aProps[i].Name);
        else if ((ex1 == null) != (ex2 == null))
          Assert.Fail(message + ". Exception for one of compared properties " + aProps[i].Name);
      }

    #endregion

    #region Перечислимые объекты

      IEnumerable en1 = expected as IEnumerable;
      if (en1 != null)
      {
        IEnumerable en2 = (IEnumerable)actual;
        ArrayList lst1 = new ArrayList();
        foreach (object item1 in en1)
          lst1.Add(item1);

        ArrayList lst2 = new ArrayList();
        foreach (object item2 in en2)
          lst2.Add(item2);

        Assert.AreEqual(lst1.Count, lst2.Count, message + ". Enumeration result length");
        for (int i = 0; i < lst1.Count; i++)
          DoAreEqual(testedObjects, lst1[i], lst2[i], message + "[" + i.ToString() + "]"); // рекурсивный вызов
      }

    #endregion
    }
#endif
  }

  /// <summary>
  /// DateTime equality with delta
  /// </summary>
  public static class DateTimeAssert
  {
    // Наверное, можно лучше сделать

    public static void AreEqual(DateTime expected, DateTime actual, TimeSpan delta)
    {
      if (IsError(expected, actual, delta))
        Assert.AreEqual(expected, actual);
    }

    public static void AreEqual(DateTime expected, DateTime actual, TimeSpan delta, string message)
    {
      if (IsError(expected, actual, delta))
        Assert.AreEqual(expected, actual, message);
    }

    public static void AreEqual(DateTime expected, DateTime actual, TimeSpan delta, string message, params object[] args)
    {
      if (IsError(expected, actual, delta))
        Assert.AreEqual(expected, actual, message, args);
    }

    private static bool IsError(DateTime expected, DateTime actual, TimeSpan delta)
    {
      if (delta.Ticks < 0L)
        throw new ArgumentOutOfRangeException("delta");

      long delta1 = Math.Abs(expected.Ticks - actual.Ticks);
      return delta1 > delta.Ticks;
    }


    public static void AreEqual(DateTime? expected, DateTime? actual, TimeSpan delta)
    {
      if (IsError(expected, actual, delta))
        Assert.AreEqual(expected, actual);
    }

    public static void AreEqual(DateTime? expected, DateTime? actual, TimeSpan delta, string message)
    {
      if (IsError(expected, actual, delta))
        Assert.AreEqual(expected, actual, message);
    }

    public static void AreEqual(DateTime? expected, DateTime? actual, TimeSpan delta, string message, params object[] args)
    {
      if (IsError(expected, actual, delta))
        Assert.AreEqual(expected, actual, message, args);
    }

    private static bool IsError(DateTime? expected, DateTime? actual, TimeSpan delta)
    {
      if (expected.HasValue && actual.HasValue)
        return IsError(expected.Value, actual.Value, delta);
      if (expected.HasValue || actual.HasValue)
        return true;
      return false;
    }
  }


  /// <summary>
  /// Обход предупреждения Obsolete для методов <see cref="NUnit.Framework.Assert.IsNullOrEmpty(string)"/> и<see cref="NUnit.Framework.Assert.IsNotNullOrEmpty(string)"/>.
  /// </summary>
  public static class ExtStringAssert
  {
#pragma warning disable 0618

    /// <summary>
    /// Assert that a string is not null or empty
    /// </summary>
    /// <param name="aString">The string to be tested</param>
    public static void IsNotNullOrEmpty(string aString)
    {
      Assert.IsNotNullOrEmpty(aString);
    }

    public static void IsNotNullOrEmpty(string aString, string message)
    {
      Assert.IsNotNullOrEmpty(aString, message);
    }

    /// <summary>
    /// Assert that a string is not null or empty
    /// </summary>
    /// <param name="aString">The string to be tested</param>
    /// <param name="message">The message to display in case of failure</param>
    /// <param name="args">Array of objects to be used in formatting the message</param>
    public static void IsNotNullOrEmpty(string aString, string message, params object[] args)
    {
      Assert.IsNotNullOrEmpty(aString, message, args);
    }
    //
    // Summary:
    //     /// Assert that a string is either null or equal to string.Empty ///
    //
    // Parameters:
    //   aString:
    //     The string to be tested
    /// <summary>
    /// Assert that a string is either null or equal to <see cref="String.Empty"/>
    /// </summary>
    /// <param name="aString">The string to be tested</param>
    public static void IsNullOrEmpty(string aString)
    {
      Assert.IsNullOrEmpty(aString);
    }

    /// <summary>
    /// Assert that a string is either null or equal to <see cref="String.Empty"/>
    /// </summary>
    /// <param name="aString">The string to be tested</param>
    /// <param name="message">The message to display in case of failure</param>
    public static void IsNullOrEmpty(string aString, string message)
    {
      Assert.IsNullOrEmpty(aString, message);
    }

    /// <summary>
    /// Assert that a string is either null or equal to <see cref="String.Empty"/>
    /// </summary>
    /// <param name="aString">The string to be tested</param>
    /// <param name="message">The message to display in case of failure</param>
    /// <param name="args">Array of objects to be used in formatting the message</param>
    public static void IsNullOrEmpty(string aString, string message, params object[] args)
    {
      Assert.IsNullOrEmpty(aString, message, args);
    }

#pragma warning restore 0618
  }

#if XXX // Не знаю, нужно ли. Одного делегата мало. Хочется еще, например, использовать объект Random, а он нужен для каждого потока

  /// <summary>
  /// Parallel executing code in some number of threads
  /// </summary>
  public static class MultithreadAssert
  {
    /// <summary>
    /// Executes the <paramref name="code"/> in a the default number of threads.
    /// Test failed if any of <see cref="code"/> call throws an exception.
    /// </summary>
    /// <param name="code">Executing test code</param>
    /// <param name="circles">How many time the code should run</param>
    public static void DoesNotThrow(TestDelegate code, int circles)
    {
      if (code == null)
        throw new ArgumentNullException("code");

      TestHelper helper = new TestHelper();
      helper.Code = code;
      helper.Circles = circles;


      helper.Infos = new ThreadInfo[DefaultThreadCount];
      for (int i = 0; i < helper.Infos.Length; i++)
      {
        helper.Infos[i] = new ThreadInfo();
        helper.Infos[i].Helper = helper;
        helper.Infos[i].Thread = new Thread(helper.Infos[i].RunForCircles);
      }

      // Start only after the whole initialization
      for (int i = 0; i < helper.Infos.Length; i++)
        helper.Infos[i].Thread.Start();

      // Wait for all
      for (int i = 0; i < helper.Infos.Length; i++)
        helper.Infos[i].Thread.Join();

      // Check for an exception
      for (int i = 0; i < helper.Infos.Length; i++)
      {
        if (helper.Infos[i].Exception != null)
          Assert.DoesNotThrow(delegate () { throw helper.Infos[i].Exception; });
      }
    }

    /// <summary>
    /// Executes the <paramref name="code"/> in a the default number of threads.
    /// Test failed if any of <see cref="code"/> call throws an exception.
    /// </summary>
    /// <param name="code">Executing test code</param>
    /// <param name="timeout">Execution time</param>
    public static void DoesNotThrow(TestDelegate code, TimeSpan timeout)
    {
      if (code == null)
        throw new ArgumentNullException("code");
      if (timeout.Ticks < 1L)
        throw new ArgumentOutOfRangeException("timeout");

      TestHelper helper = new TestHelper();
      helper.Code = code;
      helper.Timeout = timeout;


      helper.Infos = new ThreadInfo[DefaultThreadCount];
      for (int i = 0; i < helper.Infos.Length; i++)
      {
        helper.Infos[i] = new ThreadInfo();
        helper.Infos[i].Helper = helper;
        helper.Infos[i].Thread = new Thread(helper.Infos[i].RunForTime);
      }

      // Start only after the whole initialization
      for (int i = 0; i < helper.Infos.Length; i++)
        helper.Infos[i].Thread.Start();

      // Wait for all
      for (int i = 0; i < helper.Infos.Length; i++)
        helper.Infos[i].Thread.Join();

      // Check for an exception
      for (int i = 0; i < helper.Infos.Length; i++)
      {
        if (helper.Infos[i].Exception != null)
          Assert.DoesNotThrow(delegate () { throw helper.Infos[i].Exception; });
      }
    }

    private static int DefaultThreadCount { get { return Environment.ProcessorCount; } }

    private class TestHelper
    {
      public TestDelegate Code;
      public int Circles;
      public TimeSpan Timeout;
      public ThreadInfo[] Infos;

      public bool AreAllStarted()
      {
        for (int i = 0; i < Infos.Length; i++)
        {
          if (!Infos[i].IsStarted)
            return false;
        }
        return true;
      }
    }

    private class ThreadInfo
    {
      public TestHelper Helper;
      public Thread Thread;
      public Exception Exception;
      public volatile bool IsStarted;

      private void WaitForAllStarted()
      {
        // Ожидаем, пока не запустятся все потоки
        IsStarted = true;

        while (!Helper.AreAllStarted())
          Thread.Sleep(0);
      }

      public void RunForCircles()
      {
        try
        {
          WaitForAllStarted();

          for (int i = 0; i < Helper.Circles; i++)
            Helper.Code();
        }
        catch (Exception e)
        {
          Exception = e;
        }
      }

      public void RunForTime()
      {
        try
        {
          WaitForAllStarted();

          DateTime finishTime = DateTime.Now + Helper.Timeout;

          while (DateTime.Now <= finishTime)
            Helper.Code();
        }
        catch (Exception e)
        {
          Exception = e;
        }
      }
    }
  }

#endif

  //public abstract class ThreadTester
  //{
  //  abstract void Test;
  //}
}
