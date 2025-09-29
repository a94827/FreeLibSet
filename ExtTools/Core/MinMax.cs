// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Структура для подсчета минимального и максимального значений целого числа.
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы <see cref="o:DataTools.MinMaxInt32()"/> для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  public struct MinMax<T>
    where T : struct, IComparable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает <see cref="MinValue"/> и <see cref="MaxValue"/> равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMax(T value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств <see cref="MinValue"/> и <see cref="MaxValue"/>.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMax(T minValue, T maxValue)
    {
      if (minValue.CompareTo(maxValue) > 0)
        throw ExceptionFactory.ArgRangeInverted("minValue", minValue, "maxValue", maxValue);

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    /// <summary>
    /// Если задана непустая коллекция значений, то устанавливает минимальное и максимальное значения.
    /// Иначе структура будет пустой.
    /// </summary>
    /// <param name="items"></param>
    public MinMax(IEnumerable<T> items)
    {
      // Нужна инициализация, иначе будет ошибка компиляции
      _HasValue = false;
      _MinValue = default(T);
      _MaxValue = default(T);

      if (items == null)
        return;

      foreach (T item in items)
      {
        if (_HasValue)
        {
          if (item.CompareTo(_MinValue) < 0)
            _MinValue = item;
          if (item.CompareTo(_MaxValue) > 0)
            _MaxValue = item;
        }
        else
        {
          _HasValue = true;
          _MinValue = item;
          _MaxValue = item;
        }
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private readonly bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если <see cref="HasValue"/>=true.
    /// </summary>
    public T MinValue { get { return _MinValue; } }
    private readonly T _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если <see cref="HasValue"/>=true.
    /// </summary>
    public T MaxValue { get { return _MaxValue; } }
    private readonly T _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(<see cref="MinValue"/>:<see cref="MaxValue"/>)".
    /// Возвращает "()", если <see cref="HasValue"/>=false.
    /// </summary>
    /// <returns>Текстовое представление диапазона</returns>
    public override string ToString()
    {
      if (HasValue)
        return "{" + MinValue.ToString() + ":" + MaxValue.ToString() + "}";
      else
        return "{}";
    }

    #endregion

    #region Операторы

    /// <summary>
    /// Расширяет диапазон <paramref name="minMax"/> так, чтобы новый диапазон включал значение <paramref name="value"/>.
    /// Если даиапазон <paramref name="minMax"/> пустой, то возвращается диапазон, содержащий единственное значение.
    /// </summary>
    /// <param name="minMax">Исходный диапазон, возможно пустой</param>
    /// <param name="value">Добавляемое значение</param>
    /// <returns>Расширенный диапазон</returns>
    public static MinMax<T> operator +(MinMax<T> minMax, T value)
    {
      if (minMax.HasValue)
      {
        T v1 = minMax.MinValue;
        if (value.CompareTo(v1) < 0)
          v1 = value;
        T v2 = minMax.MaxValue;
        if (value.CompareTo(v2) > 0)
          v2 = value;
        return new MinMax<T>(v1, v2);
      }
      else
        return new MinMax<T>(value);
    }

    /// <summary>
    /// Расширяет диапазон <paramref name="minMax"/> так, чтобы новый диапазон включал значение <paramref name="value"/>,
    /// если оно задано.
    /// Если даиапазон <paramref name="minMax"/> пустой, то возвращается диапазон, содержащий единственное значение.
    /// Если значение <paramref name="value"/> равно null, то возвращается неизменный диапазон <paramref name="minMax"/>.
    /// </summary>
    /// <param name="minMax">Исходный диапазон, возможно пустой</param>
    /// <param name="value">Добавляемое значение или null</param>
    /// <returns>Расширенный диапазон или <paramref name="minMax"/></returns>
    public static MinMax<T> operator +(MinMax<T> minMax, T? value)
    {
      if (value.HasValue)
        return minMax + value.Value;
      else
        return minMax;
    }

    /// <summary>
    /// Возвращает объединенный диапазон, куда входят все значения из <paramref name="value1"/> и <paramref name="value2"/>.
    /// Если один из диапазонов пустой, возвращается другой диапазон.
    /// Если оба диапазона пусты, возвращается пустой диапазон.
    /// </summary>
    /// <param name="value1">Первый объединяемый диапазон</param>
    /// <param name="value2">Второй объединяемый диапазон</param>
    /// <returns>Объединенный диапазон</returns>
    public static MinMax<T> operator +(MinMax<T> value1, MinMax<T> value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
        {
          T v1 = value1.MinValue;
          if (value2.MinValue.CompareTo(v1) < 0)
            v1 = value2.MinValue;
          T v2 = value1.MaxValue;
          if (value2.MaxValue.CompareTo(v2) > 0)
            v2 = value2.MaxValue;

          return new MinMax<T>(v1, v2);
        }
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Операторы преобразования

    /// <summary>
    /// Преобразует в диапазон другого типа.
    /// Преобразование выполняется методом <see cref="System.Convert.ChangeType(object, Type)"/>.
    /// </summary>
    /// <typeparam name="TRes">Тип данных для нового диапазона</typeparam>
    /// <returns>Новый диапазон</returns>
    public MinMax<TRes> ToType<TRes>()
      where TRes : struct, IComparable<TRes>
    {
      if (HasValue)
        return new MinMax<TRes>((TRes)Convert.ChangeType(MinValue, typeof(TRes)),
          (TRes)Convert.ChangeType(MaxValue, typeof(TRes)));
      else
        return new MinMax<TRes>();
    }


    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает <see cref="MinValue"/> или null, если диапазон пустой
    /// </summary>
    public T? NullableMinValue
    {
      get
      {
        if (HasValue)
          return MinValue;
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает <see cref="MaxValue"/> или null, если диапазон пустой
    /// </summary>
    public T? NullableMaxValue
    {
      get
      {
        if (HasValue)
          return MaxValue;
        else
          return null;
      }
    }

    #endregion
  }
}
