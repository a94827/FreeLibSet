﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// Логическое "И"
  /// </summary>
  [Serializable]
  public sealed class DepAnd : DepExprTA<bool, bool>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, реализующий функцию AND для двух аргументов
    /// </summary>
    /// <param name="a">Первое значение</param>
    /// <param name="b">Второе значение</param>
    public DepAnd(DepValue<Boolean> a, DepValue<Boolean> b)
      : base(new DepValue<bool>[2] { a, b }, null)
    {
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект, реализующий функцию AND для произвольного числа aргументов
    /// </summary>
    /// <param name="args">Массив аргументов</param>
    public DepAnd(params DepValue<Boolean>[] args)
      : base(args, null)
    {
      BaseSetValue(Calculate(), false);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вычисление значения
    /// </summary>
    /// <returns>Вычисленное значение</returns>
    protected override bool Calculate()
    {
      for (int i = 0; i < Args.Length; i++)
      {
        if (!Args[i].Value)
          return false;
      }
      return true;
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "AND", если свойство <see cref="DepValue{Boolean}.OwnerInfo"/> не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "AND";
      else
        return base.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Присоединение ко входу с логикой "И" к существующим источникам.
    /// Если <paramref name="resValue"/> не имеет источника, то <paramref name="srcValue"/> присоединяется в качестве источника
    /// непосредственно. Если источник есть, то создается новый объект <see cref="DepAnd"/>.
    /// </summary>
    /// <param name="resValue">Зависимое значение</param>
    /// <param name="srcValue">Исходное значение</param>
    public static void AttachInput(DepValue<Boolean> resValue, DepValue<Boolean> srcValue)
    {
      DepInput<Boolean> resValue2 = resValue as DepInput<bool>;

#if DEBUG
      if (resValue == null)
        throw new ArgumentNullException("resValue");
      if (resValue2 == null)
        throw new ArgumentException(String.Format(Res.DepValue_Arg_NotDepInput, resValue.ToString()), "resValue");
      if (srcValue == null)
        throw new ArgumentNullException("srcValue");
#endif


      if (resValue2.Source == null)
        resValue2.Source = srcValue;
      else
        // 15.10.2021
        // Больше нельзя добавлять входы к сушествуюшему объекту.
        // Если создать новый DepAnd взамен существующего, то от старого источника останутся привязанные обработчики события ValueChanged.
        resValue2.Source = new DepAnd(resValue2.Source, srcValue);
    }

    #endregion
  }

  /// <summary>
  /// Логическое "ИЛИ"
  /// </summary>
  [Serializable]
  public sealed class DepOr : DepExprTA<bool, bool>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, вычисляющую функцию OR для двух аргументов
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public DepOr(DepValue<Boolean> a, DepValue<Boolean> b)
      : base(new DepValue<bool>[2] { a, b }, null)
    {
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект, реализующий функцию OR для произвольного числа aргументов
    /// </summary>
    /// <param name="args">Массив аргументов</param>
    public DepOr(params DepValue<Boolean>[] args)
      : base(args, null)
    {
      BaseSetValue(Calculate(), false);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вычисление значения
    /// </summary>
    /// <returns>Вычисленное значение OR</returns>
    protected override bool Calculate()
    {
      for (int i = 0; i < Args.Length; i++)
      {
        if (Args[i].Value)
          return true;
      }
      return false;
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "OR", если свойство <see cref="DepValue{Boolean}.OwnerInfo"/> не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "OR";
      else
        return base.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Присоединение ко входу с логикой "ИЛИ" к существующим источникам.
    /// Если <paramref name="resValue"/> не имеет источника, то <paramref name="srcValue"/> присоединяется в качестве источника
    /// непосредственно. Если источник есть, то создается новый объект <see cref="DepOr"/>.
    /// </summary>
    /// <param name="resValue">Зависимое значение</param>
    /// <param name="srcValue">Исходное значение</param>
    public static void AttachInput(DepValue<Boolean> resValue, DepValue<Boolean> srcValue)
    {
      DepInput<Boolean> resValue2 = resValue as DepInput<bool>;

#if DEBUG
      if (resValue == null)
        throw new ArgumentNullException("resValue");
      if (resValue2 == null)
        throw new ArgumentException(String.Format(Res.DepValue_Arg_NotDepInput, resValue.ToString()), "resValue");
      if (srcValue == null)
        throw new ArgumentNullException("srcValue");
#endif


      if (resValue2.Source == null)
        resValue2.Source = srcValue;
      else
        // 15.10.2021
        // Больше нельзя добавлять входы к сушествуюшему объекту.
        // Если создать новый DepAnd взамен существующего, то от старого источника останутся привязанные обработчики события ValueChanged.
        resValue2.Source = new DepOr(resValue2.Source, srcValue);
    }

    #endregion
  }

  /// <summary>
  /// Логическое "НЕ"
  /// </summary>
  [Serializable]
  public sealed class DepNot : DepExpr1<Boolean, Boolean>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, вычисляющий функцию NOT для заданного аргумента
    /// </summary>
    /// <param name="arg">Аргумент</param>
    public DepNot(DepValue<Boolean> arg)
      : base(arg, null)
    {
      BaseSetValue(Calculate(), false);
    }

    #endregion

    #region Вычисление

    /// <summary>
    /// Вычисляет выражение
    /// </summary>
    /// <returns></returns>
    protected override bool Calculate()
    {
      return !Arg.Value;
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "NOT", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "NOT";
      else
        return base.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Добавление логического "НЕ" к выходу <paramref name="srcValue"/>.
    /// Среди выходов <paramref name="srcValue"/> выполняется поиск существующего объекта DepNot. Если
    /// найден, то он возвращается. Иначе создается новый объект DepNot
    /// </summary>
    /// <param name="srcValue"></param>
    /// <returns>Инвертированный выход</returns>
    public static DepNot NotOutput(DepValue<Boolean> srcValue)
    {
      if (srcValue == null)
        throw new ArgumentNullException("srcValue");

      if (srcValue.HasOutputs)
      {
        DepInput<Boolean>[] children = srcValue.Outputs;
        for (int i = 0; i < children.Length; i++)
        {
          if (children[i].OwnerInfo.Owner != null)
          {
            if (children[i].OwnerInfo.Owner is DepNot)
              return (DepNot)(children[i].OwnerInfo.Owner);
          }
        }
      }
      // Нет подходящего объекта
      return new DepNot(srcValue);
    }

    #endregion
  }

  /// <summary>
  /// Сравнение двух значений .
  /// Содержит значение true, когда аргументы <see cref="DepExpr2{Boolean, T, T}.Arg1"/> и <see cref="DepExpr2{Boolean, T, T}.Arg2"/> равны друг другу.
  /// </summary>
  /// <typeparam name="T">Тип сравниваемых значений.</typeparam>
  [Serializable]
  public sealed class DepEqual<T> : DepExpr2<Boolean, T, T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, выполняющий сравнение двух управляемых объектов.
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    public DepEqual(DepValue<T> arg1, DepValue<T> arg2)
      : base(arg1, arg2, null)
    {
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение управляемого значения с константой
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент (константа)</param>
    public DepEqual(DepValue<T> arg1, T arg2)
      : this(arg1, new DepConst<T>(arg2))
    {
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "EQUAL", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "EQUAL";
      else
        return base.ToString();
    }

    #endregion

    #region Вычисление

    /// <summary>
    /// Вычисление
    /// </summary>
    /// <returns></returns>
    protected override bool Calculate()
    {
      return Object.Equals(Arg1.Value, Arg2.Value);
    }

    #endregion
  }


  #region Перечисление DepCompareKind

  /// <summary>
  /// Режимы сравнения поля с константой для фильтра ValueFilter
  /// </summary>
  [Serializable]
  public enum DepCompareKind
  {
    /// <summary>
    /// Сравнение на равенство
    /// </summary>
    Equal,

    /// <summary>
    /// "Меньше"
    /// </summary>
    LessThan,

    /// <summary>
    /// "Меньше или равно"
    /// </summary>
    LessOrEqualThan,

    /// <summary>
    /// "Больше"
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Больше или равно"
    /// </summary>
    GreaterOrEqualThan,

    /// <summary>
    /// "Не равно"
    /// </summary>
    NotEqual,
  }

  #endregion


  /// <summary>
  /// Сравнение двух значений, с поддержкой шести операций сравнения.
  /// Содержит результат сравнения аргументов <see cref="DepExpr2{Boolean, T, T}.Arg1"/> и <see cref="DepExpr2{Boolean, T, T}.Arg2"/>.
  /// В отличие от шаблонного класса <see cref="DepEqual{T}"/>, для <see cref="DepComparer{T}"/> требуется, чтобы тип <typeparamref name="T"/> поддерживал сравнение на "больше/меньше".
  /// Для этого, либо класс должен реализовывать интерфейс IComparable of T, либо должно быть задано свойство Comparer.
  /// </summary>
  /// <typeparam name="T">Тип сравниваемых значений</typeparam>
  [Serializable]
  public sealed class DepComparer<T> : DepExpr2<Boolean, T, T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, выполняющий сравнение двух управляемых объектов с произвольным компаратором.
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <param name="kind">Операция сравнения</param>
    /// <param name="comparer">Компаратор. Если null, то используется стандартный компаратор</param>
    public DepComparer(DepValue<T> arg1, DepValue<T> arg2, DepCompareKind kind, IComparer<T> comparer)
      : base(arg1, arg2, null)
    {
      _Kind = kind;

      if (comparer == null)
        _Comparer = Comparer<T>.Default;
      else
        _Comparer = comparer;

      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение двух управляемых объектов.
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <param name="kind">Операция сравнения</param>
    public DepComparer(DepValue<T> arg1, DepValue<T> arg2, DepCompareKind kind)
      : this(arg1, arg2, kind, null)
    {
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение управляемого значения с константой
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент (константа)</param>
    /// <param name="kind">Операция сравнения</param>
    public DepComparer(DepValue<T> arg1, T arg2, DepCompareKind kind)
      : this(arg1, new DepConst<T>(arg2), kind)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Операция сравнения
    /// </summary>
    public DepCompareKind Kind { get { return _Kind; } }
    private readonly DepCompareKind _Kind;

    /// <summary>
    /// Сравниватель для значений.
    /// Если свойство не было установлено в явном виде, возвращается Comparer of T.Default
    /// </summary>
    public IComparer<T> Comparer { get { return _Comparer; } }
    private readonly IComparer<T> _Comparer;

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "OPERATOR xxx", если свойство <see cref="DepValue{Boolean}"/>.OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "OPERATOR " + Kind.ToString();
      else
        return base.ToString();
    }

    #endregion

    #region Вычисление

    /// <summary>
    /// Выполняет расчет
    /// </summary>
    /// <returns>Результат сравнения</returns>
    protected override bool Calculate()
    {
      return Compare(Arg1.Value, Arg2.Value, Kind, Comparer);
    }

    internal static bool Compare(T value1, T value2, DepCompareKind kind, IComparer<T> comparer)
    {
      switch (kind)
      {
        case DepCompareKind.Equal: return comparer.Compare(value1, value2) == 0;
        case DepCompareKind.LessThan: return comparer.Compare(value1, value2) < 0;
        case DepCompareKind.LessOrEqualThan: return comparer.Compare(value1, value2) <= 0;
        case DepCompareKind.GreaterThan: return comparer.Compare(value1, value2) > 0;
        case DepCompareKind.GreaterOrEqualThan: return comparer.Compare(value1, value2) >= 0;
        case DepCompareKind.NotEqual: return comparer.Compare(value1, value2) != 0;
        default:
          throw ExceptionFactory.ArgUnknownValue("kind", kind);
      }
    }

    #endregion
  }

  /// <summary>
  /// Проверка попадания значения в диапазон.
  /// Тип <typeparamref name="T"/> должен реализовывать интерфейс <see cref="IComparable{T}.CompareTo(T)"/>, либо должно быть задано свойство <see cref="Comparer"/>.
  /// Полуоткрытые интервалы не поддерживаются. Используйте класс <see cref="DepComparer{T}"/>.
  /// </summary>
  /// <typeparam name="T">Тип сравниваемых значений</typeparam>
  [Serializable]
  public sealed class DepInRange<T> : DepExpr3<Boolean, T, T, T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, выполняющий сравнение с управляемыми объектами с произвольным компаратором.
    /// </summary>
    /// <param name="testedValue">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение</param>
    /// <param name="maximum">Максимальное значение</param>
    /// <param name="comparer">Компаратор. Если null, то используется стандартный компаратор</param>
    public DepInRange(DepValue<T> testedValue, DepValue<T> minimum, DepValue<T> maximum, IComparer<T> comparer)
      : base(testedValue, minimum, maximum, null)
    {
      if (comparer == null)
        _Comparer = Comparer<T>.Default;
      else
        _Comparer = comparer;

      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение с управляемыми объектами.
    /// </summary>
    /// <param name="testedValue">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение</param>
    /// <param name="maximum">Максимальное значение</param>
    public DepInRange(DepValue<T> testedValue, DepValue<T> minimum, DepValue<T> maximum)
      : this(testedValue, minimum, maximum, null)
    {
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение с константным диапазоном
    /// </summary>
    /// <param name="testedValue">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение</param>
    /// <param name="maximum">Максимальное значение</param>
    public DepInRange(DepValue<T> testedValue, T minimum, T maximum)
      : this(testedValue, new DepConst<T>(minimum), new DepConst<T>(maximum))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Сравниватель для значений.
    /// Если свойство не было установлено в явном виде, возвращается <see cref="System.Collections.Generic.Comparer{T}.Default"/>  of T.Default
    /// </summary>
    public IComparer<T> Comparer { get { return _Comparer; } }
    private readonly IComparer<T> _Comparer;

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "INRANGE", если свойство <see cref="DepValue{T}.OwnerInfo"/> не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "INRANGE";
      else
        return base.ToString();
    }

    #endregion

    #region Вычисление

    /// <summary>
    /// Выполняет расчет
    /// </summary>
    /// <returns>Результат сравнения</returns>
    protected override bool Calculate()
    {
      return Comparer.Compare(Arg1.Value, Arg2.Value) >= 0 &&
        Comparer.Compare(Arg1.Value, Arg3.Value) <= 0;
    }

    #endregion
  }

  /// <summary>
  /// Определяет наличие элемента в массиве.
  /// Возвращает true, если значение присутствует в массиве.
  /// Массив является фиксированным.
  /// Применяется, например, если требуется делать элемент управления доступным, когда выбрана одна из нескольких радиокнопок (Вместо комбинации DepOr с DepEqual).
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  [Serializable]
  public sealed class DepInArray<T> : DepExpr1<bool, T>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// </summary>
    /// <param name="arg">Проверяемое значение</param>
    /// <param name="items">Массив элементов для поиска. Не может быть null</param>
    public DepInArray(DepValue<T> arg, T[] items)
      : base(arg, null)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      _Items = items;

      // Теперь можно вычислить значение
      BaseSetValue(Calculate(), false);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив, в котором выполняется поиск.
    /// Задается в конструкторе. Не может быть null (хотя элементы могут быть null)
    /// </summary>
    public T[] Items { get { return _Items; } }
    private readonly T[] _Items;

    #endregion

    #region Переопределенный метод

    /// <summary>
    /// Вычисляет выражение, используя Array.IndexOf()
    /// </summary>
    /// <returns>true, если значение находится в массиве</returns>
    protected override bool Calculate()
    {
      if (_Items == null)
        return false;
      return Array.IndexOf<T>(_Items, Arg.Value) >= 0;
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "INARRAY", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "INARRAY";
      else
        return base.ToString();
    }

    #endregion
  }


  /// <summary>
  /// Реализация условного оператора (функция IIF, тернарный оператор в CSharp), который возвращает одно из двух значений, в зависимости от третьего значения.
  /// </summary>
  /// <typeparam name="T">Тип значений</typeparam>
  [Serializable]
  public sealed class DepIf<T> : DepExpr3<T, bool, T, T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, выполняющий выбор из двух управляемых объектов.
    /// </summary>
    /// <param name="conditionArg">Управляющий аргумент</param>
    /// <param name="trueArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает true</param>
    /// <param name="falseArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает false</param>
    public DepIf(DepValue<bool> conditionArg, DepValue<T> trueArg, DepValue<T> falseArg)
      : base(conditionArg, trueArg, falseArg, null)
    {
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект, выполняющий выбор из двух фиксированных значений.
    /// </summary>
    /// <param name="conditionArg">Управляющий аргумент</param>
    /// <param name="trueArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает true</param>
    /// <param name="falseArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает false</param>
    public DepIf(DepValue<bool> conditionArg, T trueArg, T falseArg)
      : this(conditionArg, new DepConst<T>(trueArg), new DepConst<T>(falseArg))
    {
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "EQUAL", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "IF";
      else
        return base.ToString();
    }

    #endregion

    #region Вычисление

    /// <summary>
    /// Выполняет расчет
    /// </summary>
    /// <returns></returns>
    protected override T Calculate()
    {
      if (Arg1.Value)
        return Arg2.Value;
      else
        return Arg3.Value;
    }

    #endregion
  }
}
