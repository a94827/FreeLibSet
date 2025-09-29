// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FreeLibSet.UICore
{
  /// <summary>
  /// Интерфейс объекта, который выполняет перебор значений с помощью стрелочек вверх и вниз.
  /// </summary>
  /// <typeparam name="T">Тип редактируемых данных. Если элемент управления, например, поле ввода числа, поддерживает пустые значения,
  /// то тип должен быть Nullable-типом</typeparam>
  public interface IUpDownHandler<T>
  {
    /// <summary>
    /// Получить следующее и предыдущее значение для прокрутки
    /// </summary>
    /// <param name="current">Текущеее значение</param>
    /// <param name="hasNext">Сюда записывается true, если можно нажимать стрелку вверх</param>
    /// <param name="nextValue">Значение, которое будет получено при нажатии стрелки вверх</param>
    /// <param name="hasPrev">Сюда записывается true, если можно нажимать стрелку вниз</param>
    /// <param name="prevValue">Значение, которое будет получено при нажатии стрелки вниз</param>
    void GetUpDown(T current, out bool hasNext, out T nextValue, out bool hasPrev, out T prevValue);
  }

  /// <summary>
  /// Интерфейс, возвращающий свойства Minimum и Maximum.
  /// Реализуется управляющими элементами, в которых есть свойства Minimum, Maximum и Increment, и
  /// которые реализуют свойство Increment через <see cref="IncrementUpDownHandler{T}"/>.
  /// </summary>
  /// <typeparam name="T">Тип свойств Minimum и Maximum. Для поля ввода числа - это Nullable-тип</typeparam>
  public interface IMinMaxSource<T>
  {
    /// <summary>
    /// Возвращает минимальное значение.
    /// Пустое значение означает отсутствие ограничения
    /// </summary>
    T Minimum { get; }

    /// <summary>
    /// Возвращает максимальное значение.
    /// Пустое значение означает отсутствие ограничения
    /// </summary>
    T Maximum { get; }
  }

  #region IncrementUpDownHandler

  /// <summary>
  /// Реализация прокрутки числовых значений с помощью свойства Increment
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public abstract class IncrementUpDownHandler<T> : IUpDownHandler<T?>
    where T : struct, IComparable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="increment">Инкремент. Должен быть больше 0</param>
    /// <param name="minMaxSource">Источник диапазона значений</param>
    public IncrementUpDownHandler(T increment, IMinMaxSource<T?> minMaxSource)
    {
      if (increment.CompareTo(default(T)) <= 0)
        throw ExceptionFactory.ArgOutOfRange("increment", increment, null, null);
      if (minMaxSource == null)
        throw new ArgumentNullException("minMaxSource");

      _Increment = increment;
      _MinMaxSource = minMaxSource;
    }

    /// <summary>
    /// Возвращает неабстрактную реализацию для типов <see cref="Int32"/>, <see cref="Single"/>, <see cref="Double"/> и <see cref="Decimal"/>.
    /// </summary>
    public static IncrementUpDownHandler<T> Create(T increment, IMinMaxSource<T?> minMaxSource)
    {
      Type resType;
      if (typeof(T) == typeof(Int32))
        resType = typeof(Int32UpDownHandler);
      else if (typeof(T) == typeof(Single))
        resType = typeof(SingleUpDownHandler);
      else if (typeof(T) == typeof(Double))
        resType = typeof(DoubleUpDownHandler);
      else if (typeof(T) == typeof(Decimal))
        resType = typeof(DecimalUpDownHandler);
      else
        throw new NotImplementedException();

      ConstructorInfo ci = resType.GetConstructors()[0];
      object obj = ci.Invoke(new object[2] { increment, minMaxSource });
      return (IncrementUpDownHandler<T>)obj;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение инкремента.
    /// Всегда больше 0.
    /// </summary>
    public T Increment { get { return _Increment; } }
    private T _Increment;

    private IMinMaxSource<T?> _MinMaxSource;

    /// <summary>
    /// Возврашает минимальное значение или null.
    /// </summary>
    public T? Minimum { get { return _MinMaxSource.Minimum; } }

    /// <summary>
    /// Возвращает максимальное значение или null.
    /// </summary>
    public T? Maximum { get { return _MinMaxSource.Maximum; } }

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее и предыдущее значение для прокрутки
    /// </summary>
    /// <param name="current">Текущеее значение</param>
    /// <param name="hasNext">Сюда записывается true, если можно нажимать стрелку вверх</param>
    /// <param name="nextValue">Значение, которое будет получено при нажатии стрелки вверх</param>
    /// <param name="hasPrev">Сюда записывается true, если можно нажимать стрелку вниз</param>
    /// <param name="prevValue">Значение, которое будет получено при нажатии стрелки вниз</param>
    public void GetUpDown(T? current, out bool hasNext, out T? nextValue, out bool hasPrev, out T? prevValue)
    {
      if (current.HasValue)
      {
        try { DoGetNext(current.Value, out hasNext, out nextValue); }
        catch
        {
          nextValue = null;
          hasNext = false;
        }
        try { DoGetPrev(current.Value, out hasPrev, out prevValue); }
        catch
        {
          prevValue = null;
          hasPrev = false;
        }
      }
      else
      {
        // Если текущее значение - null, то следующим значением является Minimum, а предыдущим - Maximum
        nextValue = Minimum;
        hasNext = Minimum.HasValue;
        prevValue = Maximum;
        hasPrev = Maximum.HasValue;
      }
    }

    private void DoGetNext(T currValue, out bool hasNext, out T? nextValue)
    {
      hasNext = false;
      nextValue = null;

      if (Maximum.HasValue)
      {
        if (currValue.CompareTo(Maximum.Value) >= 0)
          return;
      }

      if (Minimum.HasValue)
      {
        if (currValue.CompareTo(Minimum.Value) < 0)
        {
          hasNext = true;
          nextValue = Minimum;
          return;
        }
      }

      nextValue = GetIncrement(currValue);
      if (Maximum.HasValue)
      {
        if (nextValue.Value.CompareTo(Maximum.Value) > 0)
          nextValue = Maximum;
      }
      hasNext = true; // в любом случае
    }

    private void DoGetPrev(T currValue, out bool hasPrev, out T? prevValue)
    {
      hasPrev = false;
      prevValue = null;

      if (Minimum.HasValue)
      {
        if (currValue.CompareTo(Minimum.Value) <= 0)
          return;
      }

      if (Maximum.HasValue)
      {
        if (currValue.CompareTo(Maximum.Value) > 0)
        {
          hasPrev = true;
          prevValue = Maximum;
          return;
        }
      }

      prevValue = GetDecrement(currValue);
      if (Minimum.HasValue)
      {
        if (prevValue.Value.CompareTo(Minimum.Value) < 0)
          prevValue = Minimum;
      }
      hasPrev = true; // в любом случае
    }

    /// <summary>
    /// Получить следующее значение.
    /// Метод не должен проверять попадание значения в диапазон {<see cref="Minimum"/>-<see cref="Maximum"/>}.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected abstract T GetIncrement(T currValue);

    /// <summary>
    /// Получить предыдущее значение.
    /// Метод не должен проверять попадание значения в диапазон {<see cref="Minimum"/>-<see cref="Maximum"/>}.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected abstract T GetDecrement(T currValue);

    /// <summary>
    /// Выводит "Increment=XXX" (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Increment=" + Increment.ToString();
    }

    #endregion
  }


  /// <summary>
  /// Реализация инкремента для числового значения
  /// </summary>
  [Serializable]
  public sealed class Int32UpDownHandler : IncrementUpDownHandler<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="increment">Инкремент. Должен быть больше 0</param>
    /// <param name="minMaxSource">Источник диапазона значений</param>
    public Int32UpDownHandler(int increment, IMinMaxSource<Int32?> minMaxSource)
      : base(increment, minMaxSource)
    {
      if (increment < 1)
        throw ExceptionFactory.ArgOutOfRange("increment", increment, 1, null);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override int GetIncrement(int currValue)
    {
      if ((currValue % Increment) == 0)
        return currValue + Increment;
      if (currValue > 0)
        return currValue + Increment - (currValue % Increment);
      else
        return currValue - (currValue % Increment);
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override int GetDecrement(int currValue)
    {
      if ((currValue % Increment) == 0)
        return currValue - Increment;
      if (currValue < 0)
        return currValue - Increment - (currValue % Increment);
      else
        return currValue - (currValue % Increment);
    }

    #endregion
  }

  /// <summary>
  /// Функции для вычисления целочисленного инкремента
  /// </summary>
  internal static class NumericUpDownHandlerFunctions
  {
    #region Целочисленная реализация


    // Для инкремента, отличного от 1, выполняется прокрутка к ближайшему значения.
    // Например, если инкремент равен 5:
    // prev - current - next
    //  -5        0      5
    //   0        1      5
    //   0        2      5
    //   0        3      5
    //   0        4      5
    //   0        5     10
    //   5        6     10

    // Особенности для нецелого инкремента
    // Например, если инкремент равен 0.35:
    // -0.35      0     0.35 
    //  0.00   0.10     0.35 
    //  0.00   0.20     0.35 
    //  0.00   0.30     0.35 
    //  0.00   0.35     0.70 
    //  0.35   0.40     0.70 
    // Так как в поле ввода используется десятичная система записи числа, она неявно используется при округлении.
    // Предыдущий пример можно представить в виде расчета для целочисленного инкремента, равного 350, и дополнительного множителя m, равного 1000.00
    // Методы GetIncrement(), GetDecrement() реализуются следующим образом:
    // 1. currValue2=currValue * m, округление до целого
    // 2. Вызов целочисленного инкремента для currValue2 и Increment*m: 
    //    result2 = IntGetIncrement(currValue2, Increment*m)
    // 3. return result2 / m
    //
    // Множитель m зависит только от инкремента и может быть вычислен в конструкторе
    // Он должен быть таким, чтобы произведение Increment*m было целым числом.
    // При этом необходим некоторый запас, так как при расчете currValue может оказаться меньшим, чем число разарядов в Increment
    // Increment  m
    //     0.25   1000
    //     0.3    1000
    //     0.35   1000
    //     0.5    1000
    //     1      100
    //     2      100
    //    10      10 
    //    11      10 
    // Расчет m:
    // 1. Взять десятичный логарифм от Increment
    // 2. Округлить его до целого числа вниз.
    // 3. Возвести 10 в целую степень (значение m2):
    // 4. Разделить 1 на m2 и умножить на 1000
    // Increment Log10  m2 = 10^Log10 m=1/m2*1000
    //     0.25  -1       0.1         10000.0
    //     0.3   -1       0.1         10000.0  
    //     0.35  -1       0.1         10000.0 
    //     0.5   -1       0.1         10000.0  
    //     1      0       1.0          1000.0
    //     2      0       1.0          1000.0
    //    10      1      10.0           100.0  
    //    11      1      10.0           100.0  

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент</param>
    /// <returns>Результат инкремента</returns>
    public static long GetIncrement(long currValue, long increment)
    {
#if DEBUG
      if (increment < 1L)
        throw ExceptionFactory.ArgOutOfRange("increment", increment, 1L, null);
#endif

      if ((currValue % increment) == 0)
        return currValue + increment;
      if (currValue > 0)
        return currValue + increment - (currValue % increment);
      else
        return currValue - (currValue % increment);
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент</param>
    /// <returns>Результат декремента</returns>
    public static long GetDecrement(long currValue, long increment)
    {
#if DEBUG
      if (increment < 1L)
        throw ExceptionFactory.ArgOutOfRange("increment", increment, 1L, null);
#endif

      if ((currValue % increment) == 0)
        return currValue - increment;
      if (currValue < 0)
        return currValue - increment - (currValue % increment);
      else
        return currValue - (currValue % increment);
    }

    /// <summary>
    /// Возвращает множитель для вычисления инкремента с плавающей точкой через целочисленное приведение
    /// </summary>
    /// <param name="increment">Заданный инкремент</param>
    /// <returns>Множитель</returns>
    public static double CalcM(double increment)
    {
      double log10 = Math.Floor(Math.Log10(increment));
      double m2 = Math.Pow(10, log10);
      return 1 / m2 * 1000;
    }

    #endregion
  }

  /// <summary>
  /// Реализация инкремента для числового значения
  /// </summary>
  [Serializable]
  public sealed class SingleUpDownHandler : IncrementUpDownHandler<Single>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="increment">Инкремент. Должен быть больше 0</param>
    /// <param name="minMaxSource">Источник диапазона значений</param>
    public SingleUpDownHandler(float increment, IMinMaxSource<Single?> minMaxSource)
      : base(increment, minMaxSource)
    {
      if (increment <= 0f)
        throw new ArgumentOutOfRangeException("increment");

      _M = (float)NumericUpDownHandlerFunctions.CalcM((double)increment);
      _Increment2 = (long)Math.Round(increment * _M, 0, MidpointRounding.AwayFromZero);
#if DEBUG
      if (_Increment2 < 1L)
        throw new BugException("Increment");
#endif
    }

    private readonly float _M;
    private long _Increment2;

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override float GetIncrement(float currValue)
    {
      long curr2 = (long)Math.Round(currValue * _M, 0, MidpointRounding.AwayFromZero);
      long res2 = NumericUpDownHandlerFunctions.GetIncrement(curr2, _Increment2);
      return (float)res2 / _M;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override float GetDecrement(float currValue)
    {
      long curr2 = (long)Math.Round(currValue * _M, 0, MidpointRounding.AwayFromZero);
      long res2 = NumericUpDownHandlerFunctions.GetDecrement(curr2, _Increment2);
      return (float)res2 / _M;
    }

    #endregion
  }

  /// <summary>
  /// Реализация инкремента для числового значения
  /// </summary>
  [Serializable]
  public sealed class DoubleUpDownHandler : IncrementUpDownHandler<Double>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="increment">Инкремент. Должен быть больше 0</param>
    /// <param name="minMaxSource">Источник диапазона значений</param>
    public DoubleUpDownHandler(double increment, IMinMaxSource<Double?> minMaxSource)
      : base(increment, minMaxSource)
    {
      if (increment <= 0.0)
        throw new ArgumentOutOfRangeException("increment");

      _M = NumericUpDownHandlerFunctions.CalcM(increment);
      _Increment2 = (long)Math.Round(increment * _M, 0, MidpointRounding.AwayFromZero);
#if DEBUG
      if (_Increment2 < 1L)
        throw new BugException("Increment"); 
#endif
    }

    private readonly double _M;
    private long _Increment2;

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override double GetIncrement(double currValue)
    {
      long curr2 = (long)Math.Round(currValue * _M, 0, MidpointRounding.AwayFromZero);
      long res2 = NumericUpDownHandlerFunctions.GetIncrement(curr2, _Increment2);
      return (double)res2 / _M;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override double GetDecrement(double currValue)
    {
      long curr2 = (long)Math.Round(currValue * _M, 0, MidpointRounding.AwayFromZero);
      long res2 = NumericUpDownHandlerFunctions.GetDecrement(curr2, _Increment2);
      return (double)res2 / _M;
    }

    #endregion
  }

  /// <summary>
  /// Реализация инкремента для числового значения
  /// </summary>
  [Serializable]
  public sealed class DecimalUpDownHandler : IncrementUpDownHandler<Decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="increment">Инкремент. Должен быть больше 0</param>
    /// <param name="minMaxSource">Источник диапазона значений</param>
    public DecimalUpDownHandler(decimal increment, IMinMaxSource<Decimal?> minMaxSource)
      : base(increment, minMaxSource)
    {
      if (increment <= 0m)
        throw new ArgumentOutOfRangeException("increment");

      _M = (decimal)NumericUpDownHandlerFunctions.CalcM((double)increment);
      _Increment2 = (long)Math.Round(increment * _M, 0, MidpointRounding.AwayFromZero);
#if DEBUG
      if (_Increment2 < 1L)
        throw new BugException("Increment");
#endif
    }

    private readonly decimal _M;
    private long _Increment2;

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override decimal GetIncrement(decimal currValue)
    {
      long curr2 = (long)Math.Round(currValue * _M, 0, MidpointRounding.AwayFromZero);
      long res2 = NumericUpDownHandlerFunctions.GetIncrement(curr2, _Increment2);
      return (decimal)res2 / _M;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override decimal GetDecrement(decimal currValue)
    {
      long curr2 = (long)Math.Round(currValue * _M, 0, MidpointRounding.AwayFromZero);
      long res2 = NumericUpDownHandlerFunctions.GetDecrement(curr2, _Increment2);
      return (decimal)res2 / _M;
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// "Прокрутка" числового значения путем выбора из массива.
  /// Значения не обязаны быть числовыми, поддерживаются любые типы данных, для которых определима операция сравнения "больше-меньше".
  /// </summary>
  /// <typeparam name="T">Тип данных</typeparam>
  [Serializable]
  public class NumericArrayUpDownHandler<T> : IUpDownHandler<T?>
    where T : struct
  {
    #region Конструкторы

    // Не пытаемся в конструкторе присваивать значение полю _Comparer, если оно не задано.
    // Это бы усложнило сериализацию объекта

    /// <summary>
    /// Создает объект прокрутки на основании массива значений.
    /// Объект использует ссылку на переданный массив, а не создает копию. Конструктор может выполнить сортировку массива.
    /// Для сравнения элементов используется компаратор по умолчанию <see cref="System.Collections.Generic.Comparer{T}.Default"/>.
    /// </summary>
    /// <param name="items">Массив значений</param>
    /// <param name="performSort">Нужно ли выполнять сортировку массива (true), или массив уже отсортирован (false)</param>
    public NumericArrayUpDownHandler(T[] items, bool performSort)
      :this(items, performSort, null)
    {
    }

    /// <summary>
    /// Создает объект прокрутки на основании массива значений.
    /// Объект использует ссылку на переданный массив, а не создает копию. Конструктор может выполнить сортировку массива.
    /// </summary>
    /// <param name="items">Массив значений</param>
    /// <param name="performSort">Нужно ли выполнять сортировку массива (true), или массив уже отсортирован (false)</param>
    /// <param name="comparer">Компаратор для сравнения элементов. Если не задан, используется компаратор по умолчанию</param>
    public NumericArrayUpDownHandler(T[] items, bool performSort, IComparer<T> comparer)
    {
      if (items == null)
        items = new T[0];

      _Comparer = comparer;

      if (performSort)
        System.Array.Sort<T>(items, this.Comparer);

      _Items = items;
    }

    /// <summary>
    /// Создает объект прокрутки на перечислимого списка значений.
    /// Создается внутренний отсортированный массив элементов.
    /// Для сравнения элементов используется компаратор по умолчанию <see cref="System.Collections.Generic.Comparer{T}.Default"/>.
    /// </summary>
    /// <param name="items">Массив значений</param>
    public NumericArrayUpDownHandler(IEnumerable<T> items)
      :this(items, null)
    {
    }

    /// <summary>
    /// Создает объект прокрутки на перечислимого списка значений.
    /// Создается внутренний отсортированный массив элементов.
    /// </summary>
    /// <param name="items">Массив значений</param>
    /// <param name="comparer">Компаратор для сравнения элементов. Если не задан, используется компаратор по умолчанию</param>
    public NumericArrayUpDownHandler(IEnumerable<T> items, IComparer<T>comparer)
    {
      _Comparer = comparer;

      List<T> lst = new List<T>();
      if (items != null)
        lst.AddRange(items);
      lst.Sort(this.Comparer);

      _Items = lst.ToArray();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Отсортированный массив элементов, из которых осуществляется выбор
    /// </summary>
    public T[] Items { get { return _Items; } }
    private readonly T[] _Items;

    /// <summary>
    /// Компаратор для сравнения элементов.
    /// Если не был задан в конструкторе в явном виде, возвращает <see cref="System.Collections.Generic.Comparer{T}.Default"/>.
    /// </summary>
    public IComparer<T> Comparer
    {
      get
      {
        if (_Comparer == null)
          return Comparer<T>.Default;
        else
          return _Comparer;
      }
    }
    private readonly IComparer<T> _Comparer;

    #endregion

    #region Реализация IUpDownHandler

    /// <summary>
    /// Получить следующее и предыдущее значение для прокрутки
    /// </summary>
    /// <param name="current">Текущеее значение</param>
    /// <param name="hasNext">Сюда записывается true, если можно нажимать стрелку вверх</param>
    /// <param name="nextValue">Значение, которое будет получено при нажатии стрелки вверх</param>
    /// <param name="hasPrev">Сюда записывается true, если можно нажимать стрелку вниз</param>
    /// <param name="prevValue">Значение, которое будет получено при нажатии стрелки вниз</param>
    public void GetUpDown(T? current, out bool hasNext, out T? nextValue, out bool hasPrev, out T? prevValue)
    {
      hasNext = false;
      nextValue = null;
      hasPrev = false;
      prevValue = null;
      if (_Items.Length == 0)
        return;

      if (current.HasValue)
      {
        int p = System.Array.BinarySearch<T>(Items, current.Value, Comparer);
        if (p >= 0)
        {
          // Найдена позиция в массиве
          if (p < (Items.Length - 1))
          {
            hasNext = true;
            nextValue = Items[p + 1];
          }
          if (p > 0)
          {
            hasPrev = true;
            prevValue = Items[p - 1];
          }
        }
        else
        {
          // Найдена позиция между элементами массива или за его пределами
          p = ~p;
          if (p < Items.Length) // испр. 20.07.2023
          {
            hasNext = true;
            nextValue = Items[p];
          }
          if (p > 0)
          {
            hasPrev = true;
            prevValue = Items[p - 1];
          }
        }
      }
      else
      {
        hasNext = true;
        nextValue = Items[0];
        hasPrev = true;
        prevValue = Items[Items.Length - 1];
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "{" + StringTools.ToStringJoin<T>(", ", Items) + "}";
    }

    #endregion
  }
}
