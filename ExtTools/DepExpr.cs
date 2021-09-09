using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

// Реализация вычисления произвольных выражений
// Эти классы могут быть сериализуемыми, если обработчик может быть передан, или, если используется производный класс

namespace AgeyevAV.DependedValues
{
  #region Выражения с одним аргументом

  /// <summary>
  /// Прототип вычислителя с одним аргументом
  /// </summary>
  /// <typeparam name="TResult">Тип вычисляемого значения</typeparam>
  /// <typeparam name="T1">Тип аргумента</typeparam>
  /// <param name="arg">Аргумент</param>
  /// <returns>Результат вычисления</returns>
  public delegate TResult DepFunction1<TResult, T1>(T1 arg);

  /// <summary>
  /// Вычислитель выражения с одним аргументом.
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="T1">Тип исходных данных</typeparam>
  [Serializable]
  public class DepExpr1<TResult, T1> : DepValueObject<TResult>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор с ссылкой на исходные данные и обработчиком DepFunction1
    /// </summary>
    /// <param name="source">Источник исходных данных</param>
    /// <param name="function">Обработчик для вычисления значения</param>
    public DepExpr1(DepValue<T1> source, DepFunction1<TResult, T1> function)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
      if (function == null)
        throw new ArgumentNullException("function");
#endif
      _Source = new DepInput<T1>();
      _Source.OwnerInfo = new DepOwnerInfo(this, "Source");
      _Source.ValueChanged += new EventHandler(SourceValueChanged);
      _Function = function;
      _Source.Source = source;
      SourceValueChanged(null, null);
    }

    /// <summary>
    /// Защищенный конструктор без использования обработчика.
    /// Требуется, чтобы в производном классе был переопределен метод Calculate() и вызван метод Init().
    /// Эта версия используется, если метод Calculate() не может вызываться немедленно, т.к. не 
    /// инициализированы поля производного класса
    /// </summary>
    protected DepExpr1()
    {
    }

    /// <summary>
    /// Этот метод может использоваться в конструкторе производного класса, если
    /// был вызван конструктор этого класса без аргументов.
    /// Инициализирует свойство Source.
    /// </summary>
    /// <param name="source">Источник исходных данных</param>
    protected void Init(DepValue<T1> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
      if (_Source != null)
        throw new InvalidOperationException("Повторный вызов Init()");
#endif

      _Source = new DepInput<T1>();
      _Source.OwnerInfo = new DepOwnerInfo(this, "Source");
      _Source.ValueChanged += new EventHandler(SourceValueChanged);
      _Source.Source = source;
      SourceValueChanged(null, null);
    }

    /// <summary>
    /// Защищенный конструктор без использования обработчика.
    /// Требуется, чтобы в производном классе был переопределен метод Calculate()
    /// </summary>
    /// <param name="source">Источник исходных данных</param>
    protected DepExpr1(DepValue<T1> source)
    {
      Init(source);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Исходные данные для расчета
    /// </summary>
    public new DepValue<T1> Source { get { return _Source; } }
    private DepInput<T1> _Source;

    /// <summary>
    /// Делегат для вычисления функции
    /// </summary>
    // public DepFunction1<TResult, T1> Function { get { return FFunction; } }
    private DepFunction1<TResult, T1> _Function;

    #endregion

    #region Обработчик события

    private void SourceValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Calculate());
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException("Метод Calculate должен быть переопределен, если обработчик Function не был задан в конструкторе");
      return _Function(_Source.Value);
    }

    #endregion
  }

  #endregion

  #region Выражения с двумя аргументами

  #region Делегат

  /// <summary>
  /// Прототип вычислителя с двумя аргументами
  /// </summary>
  /// <typeparam name="TResult">Тип вычисляемого значения</typeparam>
  /// <typeparam name="T1">Тип первого аргумента</typeparam>
  /// <typeparam name="T2">Тип второго аргумента</typeparam>
  /// <param name="arg1">Первый аргумент</param>
  /// <param name="arg2">Второй аргумент</param>
  /// <returns>Результат вычисления</returns>
  public delegate TResult DepFunction2<TResult, T1, T2>(T1 arg1, T2 arg2);

  #endregion

  /// <summary>
  /// Вычислитель выражения с двумя аргументами
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="T1">Тип исходных данных аргумента 1</typeparam>
  /// <typeparam name="T2">Тип исходных данных аргумента 2</typeparam>
  [Serializable]
  public class DepExpr2<TResult, T1, T2> : DepValueObject<TResult>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор с ссылкой на исходные данные и обработчиком DepFunction2
    /// </summary>
    /// <param name="source1">Источник исходных данных аргумента 1</param>
    /// <param name="source2">Источник исходных данных аргумента 2</param>
    /// <param name="function">Обработчик для вычисления значения</param>
    public DepExpr2(DepValue<T1> source1, DepValue<T2> source2,
      DepFunction2<TResult, T1, T2> function)
    {
#if DEBUG
      if (source1 == null)
        throw new ArgumentNullException("source1");
      if (source2 == null)
        throw new ArgumentNullException("source2");
      if (function == null)
        throw new ArgumentNullException("function");
#endif

      _Source1 = new DepInput<T1>();
      _Source1.OwnerInfo = new DepOwnerInfo(this, "Source1");
      _Source1.ValueChanged += new EventHandler(SourceValueChanged);

      _Source2 = new DepInput<T2>();
      _Source2.OwnerInfo = new DepOwnerInfo(this, "Source2");
      _Source2.ValueChanged += new EventHandler(SourceValueChanged);

      _Function = function;
      _Source1.Source = source1;
      _Source2.Source = source2;
      SourceValueChanged(null, null);
    }


    /// <summary>
    /// Защищенный конструктор без использования обработчика.
    /// Требуется, чтобы в производном классе был переопределен метод Calculate() и вызван метод Init().
    /// Эта версия используется, если метод Calculate() не может вызываться немедленно, т.к. не 
    /// инициализированы поля производного класса
    /// </summary>
    protected DepExpr2()
    {
    }

    /// <summary>
    /// Защищенный конструктор без использования обработчика.
    /// Требуется, чтобы в производном классе был переопределен метод Calculate
    /// Инициализирует свойства Source1 и Source2
    /// </summary>
    /// <param name="source1">Источник исходных данных аргумента 1</param>
    /// <param name="source2">Источник исходных данных аргумента 2</param>
    protected DepExpr2(DepValue<T1> source1, DepValue<T2> source2)
    {
      Init(source1, source2);
    }

    /// <summary>
    /// Этот метод может использоваться в конструкторе производного класса, если
    /// был вызван конструктор этого класса без аргументов.
    /// Инициализирует свойства Source1 и Source2
    /// </summary>
    /// <param name="source1">Источник исходных данных аргумента 1</param>
    /// <param name="source2">Источник исходных данных аргумента 2</param>
    protected void Init(DepValue<T1> source1, DepValue<T2> source2)
    {
#if DEBUG
      if (source1 == null)
        throw new ArgumentNullException("source1");
      if (source2 == null)
        throw new ArgumentNullException("source2");
      if (_Source1 != null || _Source2 != null)
        throw new InvalidOperationException("Повторный вызов Init()");
#endif

      _Source1 = new DepInput<T1>();
      _Source1.OwnerInfo = new DepOwnerInfo(this, "Source1");
      _Source1.ValueChanged += new EventHandler(SourceValueChanged);

      _Source2 = new DepInput<T2>();
      _Source2.OwnerInfo = new DepOwnerInfo(this, "Source2");
      _Source2.ValueChanged += new EventHandler(SourceValueChanged);

      _Source1.Source = source1;
      _Source2.Source = source2;
      SourceValueChanged(null, null);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Исходные данные для расчета. Первый аргумент
    /// </summary>
    public DepValue<T1> Source1 { get { return _Source1; } }
    private DepInput<T1> _Source1;

    /// <summary>
    /// Исходные данные для расчета. Второй аргумент
    /// </summary>
    public DepValue<T2> Source2 { get { return _Source2; } }
    private DepInput<T2> _Source2;

    /// <summary>
    /// Делегат для вычисления функции
    /// </summary>
    // public DepFunction2<TResult, T1, T2> Function { get { return FFunction; } }
    private DepFunction2<TResult, T1, T2> _Function;


    #endregion

    #region Обработчик события

    private void SourceValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Calculate());
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException("Метод Calculate должен быть переопределен, если обработчик Function не был задан в конструкторе");
      return _Function(_Source1.Value, _Source2.Value);
    }

    #endregion
  }

  #endregion

  #region Выражения с тремя аргументами

  #region Делегат

  /// <summary>
  /// Прототип вычислителя с тремя аргументами
  /// </summary>
  /// <typeparam name="TResult">Тип вычисляемого значения</typeparam>
  /// <typeparam name="T1">Тип первого аргумента</typeparam>
  /// <typeparam name="T2">Тип второго аргумента</typeparam>
  /// <typeparam name="T3">Тип третего аргумента</typeparam>
  /// <param name="arg1">Первый аргумент</param>
  /// <param name="arg2">Второй аргумент</param>
  /// <param name="arg3">Третий аргумент</param>
  /// <returns>Результат вычисления</returns>
  public delegate TResult DepFunction3<TResult, T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

  #endregion

  /// <summary>
  /// Вычислитель выражения с тремя аргументами
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="T1">Тип исходных данных аргумента 1</typeparam>
  /// <typeparam name="T2">Тип исходных данных аргумента 2</typeparam>
  /// <typeparam name="T3">Тип исходных данных аргумента 3</typeparam>
  [Serializable]
  public class DepExpr3<TResult, T1, T2, T3> : DepValueObject<TResult>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор с ссылкой на исходные данные и обработчиком DepFunction3
    /// </summary>
    /// <param name="source1">Источник исходных данных аргумента 1</param>
    /// <param name="source2">Источник исходных данных аргумента 2</param>
    /// <param name="source3">Источник исходных данных аргумента 3</param>
    /// <param name="function">Обработчик для вычисления значения</param>
    public DepExpr3(DepValue<T1> source1, DepValue<T2> source2, DepValue<T3> source3,
      DepFunction3<TResult, T1, T2, T3> function)
    {
#if DEBUG
      if (source1 == null)
        throw new ArgumentNullException("source1");
      if (source2 == null)
        throw new ArgumentNullException("source2");
      if (source3 == null)
        throw new ArgumentNullException("source3");
      if (function == null)
        throw new ArgumentNullException("function");
#endif

      _Source1 = new DepInput<T1>();
      _Source1.OwnerInfo = new DepOwnerInfo(this, "Source1");
      _Source1.ValueChanged += new EventHandler(SourceValueChanged);

      _Source2 = new DepInput<T2>();
      _Source2.OwnerInfo = new DepOwnerInfo(this, "Source2");
      _Source2.ValueChanged += new EventHandler(SourceValueChanged);

      _Source3 = new DepInput<T3>();
      _Source3.OwnerInfo = new DepOwnerInfo(this, "Source3");
      _Source3.ValueChanged += new EventHandler(SourceValueChanged);
      _Function = function;
      _Source1.Source = source1;
      _Source2.Source = source2;
      _Source3.Source = source3;
      SourceValueChanged(null, null);
    }


    /// <summary>
    /// Защищенный конструктор без использования обработчика.
    /// Требуется, чтобы в производном классе был переопределен метод Calculate() и вызван метод Init().
    /// Эта версия используется, если метод Calculate() не может вызываться немедленно, т.к. не 
    /// инициализированы поля производного класса
    /// </summary>
    protected DepExpr3()
    {
    }

    /// <summary>
    /// Защищенный онструктор с ссылкой на исходные данные, без обработчика DepFunction3.
    /// Требуется, чтобы в производном классе был переопределен метод Calculate()
    /// Инициализирует свойства Source1, Source2 и Source3.
    /// </summary>
    /// <param name="source1">Источник исходных данных аргумента 1</param>
    /// <param name="source2">Источник исходных данных аргумента 2</param>
    /// <param name="source3">Источник исходных данных аргумента 3</param>
    protected DepExpr3(DepValue<T1> source1, DepValue<T2> source2, DepValue<T3> source3)
    {
      Init(source1, source2, source3);
    }

    /// <summary>
    /// Этот метод может использоваться в конструкторе производного класса, если
    /// был вызван конструктор этого класса без аргументов.
    /// Инициализирует свойства Source1, Source2 и Source3
    /// </summary>
    /// <param name="source1">Источник исходных данных аргумента 1</param>
    /// <param name="source2">Источник исходных данных аргумента 2</param>
    /// <param name="source3">Источник исходных данных аргумента 3</param>
    protected void Init(DepValue<T1> source1, DepValue<T2> source2, DepValue<T3> source3)
    {
#if DEBUG
      if (source1 == null)
        throw new ArgumentNullException("source1");
      if (source2 == null)
        throw new ArgumentNullException("source2");
      if (source3 == null)
        throw new ArgumentNullException("source3");
      if (_Source1 != null || _Source2 != null || _Source3 != null)
        throw new InvalidOperationException("Повторный вызов Init()");
#endif

      _Source1 = new DepInput<T1>();
      _Source1.OwnerInfo = new DepOwnerInfo(this, "Source1");
      _Source1.ValueChanged += new EventHandler(SourceValueChanged);

      _Source2 = new DepInput<T2>();
      _Source2.OwnerInfo = new DepOwnerInfo(this, "Source2");
      _Source2.ValueChanged += new EventHandler(SourceValueChanged);

      _Source3 = new DepInput<T3>();
      _Source3.OwnerInfo = new DepOwnerInfo(this, "Source3");
      _Source3.ValueChanged += new EventHandler(SourceValueChanged);

      _Source1.Source = source1;
      _Source2.Source = source2;
      _Source3.Source = source3;
      SourceValueChanged(null, null);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Исходные данные для расчета. Первый аргумент
    /// </summary>
    public DepValue<T1> Source1 { get { return _Source1; } }
    private DepInput<T1> _Source1;

    /// <summary>
    /// Исходные данные для расчета. Второй аргумент
    /// </summary>
    public DepValue<T2> Source2 { get { return _Source2; } }
    private DepInput<T2> _Source2;

    /// <summary>
    /// Исходные данные для расчета. Третий аргумент
    /// </summary>
    public DepValue<T3> Source3 { get { return _Source3; } }
    private DepInput<T3> _Source3;

    /// <summary>
    /// Делегат для вычисления функции
    /// </summary>
    //public DepFunction3<TResult, T1, T2, T3> Function { get { return FFunction; } }
    private DepFunction3<TResult, T1, T2, T3> _Function;


    #endregion

    #region Обработчик события

    private void SourceValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Calculate());
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException("Метод Calculate должен быть переопределен, если обработчик Function не был задан в конструкторе");
      return _Function(_Source1.Value, _Source2.Value, _Source3.Value);
    }

    #endregion
  }

  #endregion
}
