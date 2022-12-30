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
  /// Интерфейс, возвразающий свойства Minimum и Maximum.
  /// Реализуется управляющими элементами, в которых есть свойства Minimum, Maximum и Increment, и
  /// которые реализуют свойство Increment через IncrementUpDownHandler.
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
        throw new ArgumentException("increment должен быть больше 0", "increment");
      if (minMaxSource == null)
        throw new ArgumentNullException("minMaxSource");

      _Increment = increment;
      _MinMaxSource = minMaxSource;
    }

    /// <summary>
    /// Возвращает неабстрактную реализацию для типов Int32, Single, Double и Decimal.
    /// </summary>
    public static IncrementUpDownHandler<T> Create(T increment, IMinMaxSource<T?> minMaxSource)
    {
      Type resType;
      if (typeof(T) == typeof(Int32))
        resType = typeof(IntUpDownHandler);
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
      hasNext = true; // в любом случая
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
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected abstract T GetIncrement(T currValue);

    /// <summary>
    /// Получить предыдущее значение
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

  // Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
  // Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
  // округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
  // то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
  // Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
  // обычное сложение.

  // TODO: Не реализована прокрутка с учетом округления текущего значения

  /// <summary>
  /// Реализация инкремента для числового значения
  /// </summary>
  [Serializable]
  public sealed class IntUpDownHandler : IncrementUpDownHandler<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="increment">Инкремент. Должен быть больше 0</param>
    /// <param name="minMaxSource">Источник диапазона значений</param>
    public IntUpDownHandler(int increment, IMinMaxSource<Int32?> minMaxSource)
      : base(increment, minMaxSource)
    {
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
      return currValue + Increment;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override int GetDecrement(int currValue)
    {
      return currValue - Increment;
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
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override float GetIncrement(float currValue)
    {
      return currValue + Increment;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override float GetDecrement(float currValue)
    {
      return currValue - Increment;
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
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override double GetIncrement(double currValue)
    {
      return currValue + Increment;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override double GetDecrement(double currValue)
    {
      return currValue - Increment;
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
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат инкремента</returns>
    protected override decimal GetIncrement(decimal currValue)
    {
      return currValue + Increment;
    }

    /// <summary>
    /// Получить предыдущее значение
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <returns>Результат декремента</returns>
    protected override decimal GetDecrement(decimal currValue)
    {
      return currValue - Increment;
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
  public class NumArrayUpDownHandler<T> : IUpDownHandler<T?>
    where T : struct, IComparable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект прокрутки на основании массива значений.
    /// </summary>
    /// <param name="items">Массив значений</param>
    /// <param name="performSort">Нужно ли выполнять сортировку массива (true), или массив уже отсортирован (false)</param>
    public NumArrayUpDownHandler(T[] items, bool performSort)
    {
      if (items == null)
        items = new T[0];

      if (performSort)
        System.Array.Sort<T>(items);

      _Items = items; ;
    }

    /// <summary>
    /// Создает объект прокрутки на основании массива значений.
    /// Выполняется сортировка массива.
    /// </summary>
    /// <param name="items">Массив значений</param>
    public NumArrayUpDownHandler(IEnumerable<T> items)
    {
      List<T> lst = new List<T>();
      if (items != null)
        lst.AddRange(items);
      lst.Sort();
      _Items = lst.ToArray();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Отсортированный массив элементов, из которых осуществляется выбор
    /// </summary>
    public T[] Items { get { return _Items; } }
    private T[] _Items;

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
        int p = System.Array.BinarySearch<T>(Items, current.Value);
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
          if (p <= Items.Length)
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
      return "{" + DataTools.ToStringJoin<T>(", ", Items) + "}";
    }

    #endregion
  }
}
