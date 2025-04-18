﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

// Реализация вычисления произвольных выражений
// Эти классы могут быть сериализуемыми, если обработчик может быть передан, или, если используется производный класс

namespace FreeLibSet.DependedValues
{
  #region Интерфейс IDepExpr

  /// <summary>
  /// Расширяет интерфейс IDepValue списком аргументов
  /// </summary>
  public interface IDepExpr : IDepValue
  {
    /// <summary>
    /// Возвращает массив аргументов, которые используются в вычислении
    /// </summary>
    IDepValue[] Args { get; }
  }

  #endregion

  #region Выражения с одним типизированным аргументом

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
  public class DepExpr1<TResult, T1> : DepValue<TResult>, IDepExpr
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="arg">Источник исходных данных. Не может быть null.</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
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
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если конструктору не был передан делегат Function.
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException(Res.DepExpr_Err_Calculate);
      return _Function(_Arg.Value);
    }

    #endregion

    #region IDepExpr Members

    IDepValue[] IDepExpr.Args
    {
      get { return new IDepValue[1] { _Arg }; }
    }

    #endregion
  }

  #endregion

  #region Выражения с двумя типизированными аргументами

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
  public class DepExpr2<TResult, T1, T2> : DepValue<TResult>, IDepExpr
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор с двумя источниками данных.
    /// </summary>
    /// <param name="arg1">Источник исходных данных аргумента 1. Не может быть null.</param>
    /// <param name="arg2">Источник исходных данных аргумента 2. Не может быть null.</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
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
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
    public DepExpr2(DepValue<T1> arg1, T2 value2, DepFunction2<TResult, T1, T2> function)
      : this(arg1, new DepConst<T2>(value2), function)
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
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используется
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException(Res.DepExpr_Err_Calculate);
      return _Function(_Arg1.Value, _Arg2.Value);
    }

    #endregion

    #region IDepExpr Members

    IDepValue[] IDepExpr.Args
    {
      get { return new IDepValue[2] { _Arg1, _Arg2 }; }
    }

    #endregion
  }

  #endregion

  #region Выражения с тремя типизированными аргументами

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
  public class DepExpr3<TResult, T1, T2, T3> : DepValue<TResult>, IDepExpr
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
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
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
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
    public DepExpr3(DepValue<T1> arg1, DepValue<T2> arg2, T3 value3,
      DepFunction3<TResult, T1, T2, T3> function)
      : this(arg1, arg2, new DepConst<T3>(value3), function)
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
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
    public DepExpr3(DepValue<T1> arg1, T2 value2, T3 value3,
      DepFunction3<TResult, T1, T2, T3> function)
      : this(arg1, new DepConst<T2>(value2), new DepConst<T3>(value3), function)
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
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException(Res.DepExpr_Err_Calculate);
      return _Function(_Arg1.Value, _Arg2.Value, _Arg3.Value);
    }

    #endregion

    #region IDepExpr Members

    IDepValue[] IDepExpr.Args
    {
      get { return new IDepValue[3] { _Arg1, _Arg2, _Arg3 }; }
    }

    #endregion
  }

  #endregion

  #region Выражения со списком однотипных аргументов

  /// <summary>
  /// Прототип вычислителя с массивом однотипных аргументов.
  /// "TA"="Typed Array".
  /// </summary>
  /// <typeparam name="TResult">Тип вычисляемого значения</typeparam>
  /// <typeparam name="TArg">Тип аргумента</typeparam>
  /// <param name="args">Массив аргументов</param>
  /// <returns>Результат вычисления</returns>
  public delegate TResult DepFunctionTA<TResult, TArg>(TArg[] args);

  /// <summary>
  /// Вычислитель выражения с переменным числом однотипных аргументов.
  /// В качестве функции вычислителя можно использовать методы класса DepTools. Это особенно актуально для удаленного пользовательского интерфейса, когда нельзя создавать делегаты на методы в прикладном коде.
  /// Если создается класс-наследник с дополнительными аргументами, не забудьте переопределить свойство IsConst.
  /// "TA"="Typed Array".
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  /// <typeparam name="TArg">Тип исходных данных</typeparam>
  [Serializable]
  public class DepExprTA<TResult, TArg> : DepValue<TResult>, IDepExpr
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="args">Массив источников исходных данных. Не может быть null или содержать элементы null</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
    public DepExprTA(DepValue<TArg>[] args, DepFunctionTA<TResult, TArg> function)
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
      BaseSetValue(Calculate(), false);
    }

    private DepFunctionTA<TResult, TArg> _Function;

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException(Res.DepExpr_Err_Calculate);
      TArg[] a = new TArg[_Args.Length];
      for (int i = 0; i < _Args.Length; i++)
        a[i] = _Args[i].Value;
      return _Function(a);
    }

    #endregion

    #region IDepExpr Members

    IDepValue[] IDepExpr.Args
    {
      get { return _Args; }
    }

    #endregion
  }

  #endregion

  #region Выражения со списком разнотипных аргументов

  /// <summary>
  /// Прототип вычислителя с массивом аргументов типа Object.
  /// "OA"="Object Array".
  /// </summary>
  /// <typeparam name="TResult">Тип вычисляемого значения</typeparam>
  /// <param name="args">Массив аргументов</param>
  /// <returns>Результат вычисления</returns>
  public delegate TResult DepFunctionOA<TResult>(object[] args);

  /// <summary>
  /// Вычислитель выражения с переменным числом аргументов типа Object.
  /// В качестве функции вычислителя можно использовать методы класса DepTools. Это особенно актуально для удаленного пользовательского интерфейса, когда нельзя создавать делегаты на методы в прикладном коде.
  /// Если создается класс-наследник с дополнительными аргументами, не забудьте переопределить свойство IsConst.
  /// "OA"="Object Array".
  /// </summary>
  /// <typeparam name="TResult">Тип результата выражения</typeparam>
  [Serializable]
  public class DepExprOA<TResult> : DepValue<TResult>, IDepExpr
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="args">Массив источников исходных данных. Не может быть null или содержать элементы null</param>
    /// <param name="function">Обработчик для вычисления значения. 
    /// Если null, то предполагается, что класс-наследник переопределяет метод Calculate().
    /// При этом конструктор наследника должен вызвать BaseSetValue(Calculate(), false), чтобы вычислить начальное значение</param>
    public DepExprOA(IDepValue[] args, DepFunctionOA<TResult> function)
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
    public IDepValue[] Args { get { return _Args; } }
    private IDepValue[] _Args;

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
      BaseSetValue(Calculate(), false);
    }

    private DepFunctionOA<TResult> _Function;

    /// <summary>
    /// Вычисление значения.
    /// Этот метод должен быть переопределен в производном классе, если внешний обработчик Function не используетс
    /// </summary>
    /// <returns></returns>
    protected virtual TResult Calculate()
    {
      if (_Function == null)
        throw new NullReferenceException(Res.DepExpr_Err_Calculate);
      object[] a = new object[_Args.Length];
      for (int i = 0; i < _Args.Length; i++)
        a[i] = _Args[i].Value;
      return _Function(a);
    }

    #endregion
  }

  #endregion
}
