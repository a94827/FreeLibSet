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
  /// В качестве функции вычислителя можно использовать методы класса DepTools. Это особенно актуально для удаленного пользовательского интерфейса, когда нельзя создавать делегаты на методы в прикладном коде.
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="T1">Тип исходных данных</typeparam>
  [Serializable]
  public class DepExpr1<TResult, T1> : DepValueObject<TResult>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="arg">Источник исходных данных. Не может быть null.</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExpr1(DepValue<T1> arg, DepFunction1<TResult, T1> function)
    {
#if DEBUG
      if (arg == null)
        throw new ArgumentNullException("arg");
#endif
      _Arg = arg;
      if (!_Arg.IsConst)
        _Arg.ValueChanged += new EventHandler(SourceValueChanged);
      if (function != null)
      {
        _Function = function;
        SourceValueChanged(null, null);
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Исходные данные для расчета
    /// </summary>
    public DepValue<T1> Arg { get { return _Arg; } }
    private readonly DepValue<T1> _Arg;

    /// <summary>
    /// Делегат для вычисления функции
    /// </summary>
    // public DepFunction1<TResult, T1> Function { get { return FFunction; } }
    private readonly DepFunction1<TResult, T1> _Function;

    /// <summary>
    /// Возвращает true, если аргумент является константой
    /// </summary>
    public override bool IsConst
    {
      get
      {
        return _Arg.IsConst;
      }
    }

    #endregion

    #region Обработчик события

    private void SourceValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Calculate());
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если конструктору не был передан делегат Function.
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException("Метод Calculate должен быть переопределен, если обработчик Function не был задан в конструкторе");
      return _Function(_Arg.Value);
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
  /// Вычислитель выражения с двумя аргументами.
  /// В качестве функции вычислителя можно использовать методы класса DepTools. Это особенно актуально для удаленного пользовательского интерфейса, когда нельзя создавать делегаты на методы в прикладном коде.
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="T1">Тип исходных данных аргумента 1</typeparam>
  /// <typeparam name="T2">Тип исходных данных аргумента 2</typeparam>
  [Serializable]
  public class DepExpr2<TResult, T1, T2> : DepValueObject<TResult>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор с двумя источниками данных.
    /// </summary>
    /// <param name="arg1">Источник исходных данных аргумента 1. Не может быть null.</param>
    /// <param name="arg2">Источник исходных данных аргумента 2. Не может быть null.</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExpr2(DepValue<T1> arg1, DepValue<T2> arg2,
      DepFunction2<TResult, T1, T2> function)
    {
#if DEBUG
      if (arg1 == null)
        throw new ArgumentNullException("arg1");
      if (arg2 == null)
        throw new ArgumentNullException("arg2");
#endif

      _Arg1 = arg1;
      _Arg2 = arg2;
      if (!_Arg1.IsConst)
        _Arg1.ValueChanged += new EventHandler(SourceValueChanged);
      if (!_Arg2.IsConst)
        _Arg2.ValueChanged += new EventHandler(SourceValueChanged);

      if (function != null)
      {
        _Function = function;
        SourceValueChanged(null, null);
      }
    }

    /// <summary>
    /// Конструктор с гибридными аргументами
    /// </summary>
    /// <param name="arg1">Источник исходных данных аргумента 1. Не может быть null.</param>
    /// <param name="value2">Фиксированное значение 2</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExpr2(DepValue<T1> arg1, T2 value2, DepFunction2<TResult, T1, T2> function)
      : this(arg1, new DepConst <T2>(value2), function)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Исходные данные для расчета. Первый аргумент
    /// </summary>
    public DepValue<T1> Arg1 { get { return _Arg1; } }
    private readonly DepValue<T1> _Arg1;

    /// <summary>
    /// Исходные данные для расчета. Второй аргумент
    /// </summary>
    public DepValue<T2> Arg2 { get { return _Arg2; } }
    private readonly DepValue<T2> _Arg2;

    /// <summary>
    /// Делегат для вычисления функции
    /// </summary>
    // public DepFunction2<TResult, T1, T2> Function { get { return FFunction; } }
    private readonly DepFunction2<TResult, T1, T2> _Function;

    /// <summary>
    /// Возвращает true, если все аргументы являются константами
    /// </summary>
    public override bool IsConst
    {
      get
      {
        return _Arg1.IsConst && _Arg2.IsConst;
      }
    }

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
      return _Function(_Arg1.Value, _Arg2.Value);
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
  /// Вычислитель выражения с тремя аргументами.
  /// В качестве функции вычислителя можно использовать методы класса DepTools. Это особенно актуально для удаленного пользовательского интерфейса, когда нельзя создавать делегаты на методы в прикладном коде.
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
    /// Конструктор с двумя источниками данных.
    /// </summary>
    /// <param name="arg1">Источник исходных данных аргумента 1. Не может быть null.</param>
    /// <param name="arg2">Источник исходных данных аргумента 2. Не может быть null.</param>
    /// <param name="arg3">Источник исходных данных аргумента 3. Не может быть null.</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExpr3(DepValue<T1> arg1, DepValue<T2> arg2, DepValue<T3> arg3,
      DepFunction3<TResult, T1, T2, T3> function)
    {
#if DEBUG
      if (arg1 == null)
        throw new ArgumentNullException("arg1");
      if (arg2 == null)
        throw new ArgumentNullException("arg2");
      if (arg3 == null)
        throw new ArgumentNullException("arg3");
#endif

      _Arg1 = arg1;
      _Arg2 = arg2;
      _Arg3 = arg3;
      if (!_Arg1.IsConst)
        _Arg1.ValueChanged += new EventHandler(SourceValueChanged);
      if (!_Arg2.IsConst)
        _Arg2.ValueChanged += new EventHandler(SourceValueChanged);
      if (!_Arg3.IsConst)
        _Arg3.ValueChanged += new EventHandler(SourceValueChanged);

      if (function != null)
      {
        _Function = function;
        SourceValueChanged(null, null);
      }
    }

    /// <summary>
    /// Конструктор с гибридными аргументами
    /// </summary>
    /// <param name="arg1">Источник исходных данных аргумента 1. Не может быть null.</param>
    /// <param name="arg2">Источник исходных данных аргумента 2. Не может быть null.</param>
    /// <param name="value3">Фиксированное значение 3</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExpr3(DepValue<T1> arg1, DepValue<T2> arg2, T3 value3,
      DepFunction3<TResult, T1, T2, T3> function)
      : this(arg1, arg2, new DepConst <T3>(value3), function)
    {
    }

    /// <summary>
    /// Конструктор с гибридными аргументами
    /// </summary>
    /// <param name="arg1">Источник исходных данных аргумента 1. Не может быть null.</param>
    /// <param name="value2">Фиксированное значение 2</param>
    /// <param name="value3">Фиксированное значение 3</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExpr3(DepValue<T1> arg1, T2 value2, T3 value3,
      DepFunction3<TResult, T1, T2, T3> function)
      : this(arg1, new DepConst <T2>(value2), new DepConst <T3>(value3), function)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Исходные данные для расчета. Первый аргумент
    /// </summary>
    public DepValue<T1> Arg1 { get { return _Arg1; } }
    private readonly DepValue<T1> _Arg1;

    /// <summary>
    /// Исходные данные для расчета. Второй аргумент
    /// </summary>
    public DepValue<T2> Arg2 { get { return _Arg2; } }
    private readonly DepValue<T2> _Arg2;

    /// <summary>
    /// Исходные данные для расчета. Третий аргумент
    /// </summary>
    public DepValue<T3> Arg3 { get { return _Arg3; } }
    private readonly DepValue<T3> _Arg3;

    /// <summary>
    /// Делегат для вычисления функции
    /// </summary>
    //public DepFunction3<TResult, T1, T2, T3> Function { get { return FFunction; } }
    private readonly DepFunction3<TResult, T1, T2, T3> _Function;

    /// <summary>
    /// Возвращает true, если все аргументы являются константами
    /// </summary>
    public override bool IsConst
    {
      get
      {
        return _Arg1.IsConst && _Arg2.IsConst && _Arg3.IsConst;
      }
    }


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
      return _Function(_Arg1.Value, _Arg2.Value, _Arg3.Value);
    }

    #endregion
  }

  #endregion

  #region Выражения со списком однотипных аргументов
#if !XXX
  /// <summary>
  /// Прототип вычислителя с массивом однотиных аргументов
  /// </summary>
  /// <typeparam name="TResult">Тип вычисляемого значения</typeparam>
  /// <typeparam name="TArg">Тип аргумента</typeparam>
  /// <param name="args">Массив аргументов</param>
  /// <returns>Результат вычисления</returns>
  public delegate TResult DepFunctionA<TResult, TArg>(TArg[] args);

  /// <summary>
  /// Вычислитель выражения с переменным числом однотипных аргументов
  /// В качестве функции вычислителя можно использовать методы класса DepTools. Это особенно актуально для удаленного пользовательского интерфейса, когда нельзя создавать делегаты на методы в прикладном коде.
  /// Если создается класс-наследник с дополнительными аргументами, не забудьте переопределить свойство IsConst.
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="TArg">Тип исходных данных</typeparam>
  [Serializable]
  public class DepExprA<TResult, TArg> : DepValueObject<TResult>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="args">Массив источников исходных данных. Не может быть null или содержать элементы null</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать OwnerSetValue(Calculate()), чтобы вычислить начальное значение</param>
    public DepExprA(DepValue<TArg>[] args, DepFunctionA<TResult, TArg> function)
    {
#if DEBUG
      if (args == null)
        throw new ArgumentNullException("args");
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == null)
          throw new ArgumentNullException("args");
      }
#endif
      _Args = args;
      for (int i = 0; i < _Args.Length; i++)
      {
        if (!_Args[i].IsConst)
          _Args[i].ValueChanged += SourceValueChanged;
      }

      if (function != null)
      {
        _Function = function;
        SourceValueChanged(null, null);
      }
    }



    #endregion

    #region Список аргументов

    /// <summary>
    /// Исходные данные для расчета
    /// </summary>
    public DepValue<TArg>[] Args { get { return _Args; } }
    private DepValue<TArg>[] _Args;

    /// <summary>
    /// Возвращает true, если все аргументы являются константами
    /// </summary>
    public override bool IsConst
    {
      get
      {
        for (int i = 0; i < Args.Length; i++)
        {
          if (!Args[i].IsConst)
            return false;
        }
        return true;
      }
    }

    #endregion

    #region Вычисление значения

    private void SourceValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Calculate());
    }

    private DepFunctionA<TResult, TArg> _Function;

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException("Метод Calculate должен быть переопределен, если обработчик Function не был задан в конструкторе");
      TArg[] a = new TArg[_Args.Length];
      for (int i = 0; i < _Args.Length; i++)
        a[i] = _Args[i].Value;
      return _Function(a);
    }

    #endregion
  }

#endif
  #endregion
}
