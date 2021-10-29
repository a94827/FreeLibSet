using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace FreeLibSet.Core
{
  /// <summary>
  /// Структура для подсчета минимального и максимального значений целого числа.
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxInt() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxInt
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxInt(int value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxInt(int minValue, int maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public int MinValue { get { return _MinValue; } }
    private int _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public int MaxValue { get { return _MaxValue; } }
    private int _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxInt operator +(MinMaxInt minMax, int value)
    {
      if (minMax.HasValue)
        return new MinMaxInt(Math.Min(minMax.MinValue, value), Math.Max(minMax.MaxValue, value));
      else
        return new MinMaxInt(value);
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
    public static MinMaxInt operator +(MinMaxInt minMax, int? value)
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
    public static MinMaxInt operator +(MinMaxInt value1, MinMaxInt value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
          return new MinMaxInt(Math.Min(value1.MinValue, value2.MinValue),
            Math.Max(value1.MaxValue, value2.MaxValue));
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Операторы преобразования

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxInt64(MinMaxInt src)
    {
      if (src.HasValue)
        return new MinMaxInt64(src.MinValue, src.MaxValue);
      else
        return new MinMaxInt64();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxSingle(MinMaxInt src)
    {
      if (src.HasValue)
        return new MinMaxSingle(src.MinValue, src.MaxValue);
      else
        return new MinMaxSingle();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxDouble(MinMaxInt src)
    {
      if (src.HasValue)
        return new MinMaxDouble(src.MinValue, src.MaxValue);
      else
        return new MinMaxDouble();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxDecimal(MinMaxInt src)
    {
      if (src.HasValue)
        return new MinMaxDecimal(src.MinValue, src.MaxValue);
      else
        return new MinMaxDecimal();
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public int? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public int? NullableMaxValue
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

  /// <summary>
  /// Структура для подсчета минимального и максимального значений целого числа.
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxInt64() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxInt64
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxInt64(long value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxInt64(long minValue, long maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public long MinValue { get { return _MinValue; } }
    private long _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public long MaxValue { get { return _MaxValue; } }
    private long _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxInt64 operator +(MinMaxInt64 minMax, long value)
    {
      if (minMax.HasValue)
        return new MinMaxInt64(Math.Min(minMax.MinValue, value), Math.Max(minMax.MaxValue, value));
      else
        return new MinMaxInt64(value);
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
    public static MinMaxInt64 operator +(MinMaxInt64 minMax, long? value)
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
    public static MinMaxInt64 operator +(MinMaxInt64 value1, MinMaxInt64 value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
          return new MinMaxInt64(Math.Min(value1.MinValue, value2.MinValue),
            Math.Max(value1.MaxValue, value2.MaxValue));
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Операторы преобразования

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt(MinMaxInt64 src)
    {
      if (src.HasValue)
        return new MinMaxInt(Convert.ToInt32(src.MinValue), Convert.ToInt32(src.MaxValue));
      else
        return new MinMaxInt();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxSingle(MinMaxInt64 src)
    {
      if (src.HasValue)
        return new MinMaxSingle(src.MinValue, src.MaxValue);
      else
        return new MinMaxSingle();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxDouble(MinMaxInt64 src)
    {
      if (src.HasValue)
        return new MinMaxDouble(src.MinValue, src.MaxValue);
      else
        return new MinMaxDouble();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxDecimal(MinMaxInt64 src)
    {
      if (src.HasValue)
        return new MinMaxDecimal(src.MinValue, src.MaxValue);
      else
        return new MinMaxDecimal();
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public long? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public long? NullableMaxValue
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

  /// <summary>
  /// Структура для подсчета минимального и максимального значений числа с плавающей точкой.
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxSingle() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxSingle
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxSingle(float value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxSingle(float minValue, float maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public float MinValue { get { return _MinValue; } }
    private float _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public float MaxValue { get { return _MaxValue; } }
    private float _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxSingle operator +(MinMaxSingle minMax, float value)
    {
      if (minMax.HasValue)
        return new MinMaxSingle(Math.Min(minMax.MinValue, value), Math.Max(minMax.MaxValue, value));
      else
        return new MinMaxSingle(value);
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
    public static MinMaxSingle operator +(MinMaxSingle minMax, float? value)
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
    public static MinMaxSingle operator +(MinMaxSingle value1, MinMaxSingle value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
          return new MinMaxSingle(Math.Min(value1.MinValue, value2.MinValue),
            Math.Max(value1.MaxValue, value2.MaxValue));
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Операторы преобразования

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt(MinMaxSingle src)
    {
      if (src.HasValue)
        return new MinMaxInt(Convert.ToInt32(src.MinValue), Convert.ToInt32(src.MaxValue));
      else
        return new MinMaxInt();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt64(MinMaxSingle src)
    {
      if (src.HasValue)
        return new MinMaxInt64(Convert.ToInt64(src.MinValue), Convert.ToInt64(src.MaxValue));
      else
        return new MinMaxInt64();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static implicit operator MinMaxDouble(MinMaxSingle src)
    {
      if (src.HasValue)
        return new MinMaxDouble(src.MinValue, src.MaxValue);
      else
        return new MinMaxDouble();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxDecimal(MinMaxSingle src)
    {
      if (src.HasValue)
        return new MinMaxDecimal(Convert.ToDecimal(src.MinValue), Convert.ToDecimal(src.MaxValue));
      else
        return new MinMaxDecimal();
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public float? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public float? NullableMaxValue
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

  /// <summary>
  /// Структура для подсчета минимального и максимального значений числа с плавающей точкой.
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxDouble() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxDouble
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxDouble(double value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxDouble(double minValue, double maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public double MinValue { get { return _MinValue; } }
    private double _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public double MaxValue { get { return _MaxValue; } }
    private double _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxDouble operator +(MinMaxDouble minMax, double value)
    {
      if (minMax.HasValue)
        return new MinMaxDouble(Math.Min(minMax.MinValue, value), Math.Max(minMax.MaxValue, value));
      else
        return new MinMaxDouble(value);
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
    public static MinMaxDouble operator +(MinMaxDouble minMax, double? value)
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
    public static MinMaxDouble operator +(MinMaxDouble value1, MinMaxDouble value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
          return new MinMaxDouble(Math.Min(value1.MinValue, value2.MinValue),
            Math.Max(value1.MaxValue, value2.MaxValue));
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Операторы преобразования

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt(MinMaxDouble src)
    {
      if (src.HasValue)
        return new MinMaxInt(Convert.ToInt32(src.MinValue), Convert.ToInt32(src.MaxValue));
      else
        return new MinMaxInt();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt64(MinMaxDouble src)
    {
      if (src.HasValue)
        return new MinMaxInt64(Convert.ToInt64(src.MinValue), Convert.ToInt64(src.MaxValue));
      else
        return new MinMaxInt64();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxSingle(MinMaxDouble src)
    {
      if (src.HasValue)
        return new MinMaxSingle(Convert.ToSingle(src.MinValue), Convert.ToSingle(src.MaxValue));
      else
        return new MinMaxSingle();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxDecimal(MinMaxDouble src)
    {
      if (src.HasValue)
        return new MinMaxDecimal(Convert.ToDecimal(src.MinValue), Convert.ToDecimal(src.MaxValue));
      else
        return new MinMaxDecimal();
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public double? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public double? NullableMaxValue
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

  /// <summary>
  /// Структура для подсчета минимального и максимального значений числа с плавающей точкой.
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxDecimal() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxDecimal
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxDecimal(decimal value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxDecimal(decimal minValue, decimal maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public decimal MinValue { get { return _MinValue; } }
    private decimal _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public decimal MaxValue { get { return _MaxValue; } }
    private decimal _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxDecimal operator +(MinMaxDecimal minMax, decimal value)
    {
      if (minMax.HasValue)
        return new MinMaxDecimal(Math.Min(minMax.MinValue, value), Math.Max(minMax.MaxValue, value));
      else
        return new MinMaxDecimal(value);
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
    public static MinMaxDecimal operator +(MinMaxDecimal minMax, decimal? value)
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
    public static MinMaxDecimal operator +(MinMaxDecimal value1, MinMaxDecimal value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
          return new MinMaxDecimal(Math.Min(value1.MinValue, value2.MinValue),
            Math.Max(value1.MaxValue, value2.MaxValue));
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Операторы преобразования

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt(MinMaxDecimal src)
    {
      if (src.HasValue)
        return new MinMaxInt(Convert.ToInt32(src.MinValue), Convert.ToInt32(src.MaxValue));
      else
        return new MinMaxInt();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxInt64(MinMaxDecimal src)
    {
      if (src.HasValue)
        return new MinMaxInt64(Convert.ToInt64(src.MinValue), Convert.ToInt64(src.MaxValue));
      else
        return new MinMaxInt64();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxSingle(MinMaxDecimal src)
    {
      if (src.HasValue)
        return new MinMaxSingle(Convert.ToSingle(src.MinValue), Convert.ToSingle(src.MaxValue));
      else
        return new MinMaxSingle();
    }

    /// <summary>
    /// Преобразование типа структуры MinMax.
    /// </summary>
    /// <param name="src">Исходная структура</param>
    /// <returns>Структура преобразованного типа</returns>
    public static explicit operator MinMaxDouble(MinMaxDecimal src)
    {
      if (src.HasValue)
        return new MinMaxDouble(Convert.ToDouble(src.MinValue), Convert.ToDouble(src.MaxValue));
      else
        return new MinMaxDouble();
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public decimal? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public decimal? NullableMaxValue
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

  /// <summary>
  /// Структура для подсчета минимального максимального значений типа "Дата / время".
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxDateTime() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxDateTime
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxDateTime(DateTime value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxDateTime(DateTime minValue, DateTime maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    /// <summary>
    /// Создает диапазон дат, соответствующий диапазону лет.
    /// Если <paramref name="minMax"/> пустой, создается пустой диапазон.
    /// </summary>
    /// <param name="minMax">Диапазон лет</param>
    public MinMaxDateTime(MinMaxInt minMax)
    {
      if (minMax.HasValue)
      {
        _HasValue = true;
        _MinValue = DataTools.BottomOfYear(minMax.MinValue);
        _MaxValue = DataTools.EndOfYear(minMax.MaxValue);
      }
      else
      {
        _HasValue = false;
        _MinValue = new DateTime();
        _MaxValue = new DateTime();
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public DateTime MinValue { get { return _MinValue; } }
    private DateTime _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public DateTime MaxValue { get { return _MaxValue; } }
    private DateTime _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxDateTime operator +(MinMaxDateTime minMax, DateTime value)
    {
      if (minMax.HasValue)
      {
        DateTime dt1 = minMax.MinValue;
        if (value < dt1)
          dt1 = value;
        DateTime dt2 = minMax.MaxValue;
        if (value > dt2)
          dt2 = value;
        return new MinMaxDateTime(dt1, dt2);
      }
      else
        return new MinMaxDateTime(value);
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
    public static MinMaxDateTime operator +(MinMaxDateTime minMax, DateTime? value)
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
    public static MinMaxDateTime operator +(MinMaxDateTime value1, MinMaxDateTime value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
        {
          DateTime dt1 = value1.MinValue;
          if (value1.MinValue < dt1)
            dt1 = value1.MinValue;
          DateTime dt2 = value1.MaxValue;
          if (value2.MaxValue > dt2)
            dt2 = value2.MaxValue;
          return new MinMaxDateTime(dt1, dt2);
        }
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public DateTime? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public DateTime? NullableMaxValue
    {
      get
      {
        if (HasValue)
          return MaxValue;
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает диапазон целых чисел, соответствующий годам в текущем диапазоне дат.
    /// Возвращает пустой MinMaxInt, если текущий диапазон пустой.
    /// </summary>
    public MinMaxInt MinMaxYear
    {
      get
      {
        if (HasValue)
          return new MinMaxInt(MinValue.Year, MaxValue.Year);
        else
          return new MinMaxInt();
      }
    }

    #endregion
  }

  /// <summary>
  /// Структура для подсчета минимального и максимального значений типа "Интервал времени".
  /// Используйте оператор "+" для добавления значений.
  /// Используйте методы DataTools.MinMaxTimeSpan() для получения диапазонов значений из поля таблицы и массивов.
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct MinMaxTimeSpan
  {
    #region Конструкторы

    /// <summary>
    /// Устанавливает MinValue и MaxValue равными заданному значению
    /// </summary>
    /// <param name="value">Значение</param>
    public MinMaxTimeSpan(TimeSpan value)
    {
      _HasValue = true;
      _MinValue = value;
      _MaxValue = value;
    }

    /// <summary>
    /// Устанавливает значения свойств MinValue и MaxValue.
    /// </summary>
    /// <param name="minValue">Минимальное значение диапазона</param>
    /// <param name="maxValue">Максимальное значение диапазона</param>
    public MinMaxTimeSpan(TimeSpan minValue, TimeSpan maxValue)
    {
      if (maxValue < minValue)
        throw new ArgumentException("minValue больше maxValue");

      _HasValue = true;
      _MinValue = minValue;
      _MaxValue = maxValue;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если диапазон был инициализирован.
    /// Возвращает false, если для структуры использовался конструктор по умолчанию.
    /// </summary>
    public bool HasValue { get { return _HasValue; } }
    private bool _HasValue;

    /// <summary>
    /// Возвращает минимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public TimeSpan MinValue { get { return _MinValue; } }
    private TimeSpan _MinValue;

    /// <summary>
    /// Возвращает максимальное значение диапазона.
    /// Свойство имеет смысл, только если HasValue=true.
    /// </summary>
    public TimeSpan MaxValue { get { return _MaxValue; } }
    private TimeSpan _MaxValue;

    /// <summary>
    /// Возвращает строку вида "(MinValue:MaxValue)".
    /// Возвращает "()", если HasValue=false.
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
    public static MinMaxTimeSpan operator +(MinMaxTimeSpan minMax, TimeSpan value)
    {
      if (minMax.HasValue)
      {
        TimeSpan v1 = minMax.MinValue;
        if (value < v1)
          v1 = value;
        TimeSpan v2 = minMax.MaxValue;
        if (value > v2)
          v2 = value;
        return new MinMaxTimeSpan(v1, v2);
      }
      else
        return new MinMaxTimeSpan(value);
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
    public static MinMaxTimeSpan operator +(MinMaxTimeSpan minMax, TimeSpan? value)
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
    public static MinMaxTimeSpan operator +(MinMaxTimeSpan value1, MinMaxTimeSpan value2)
    {
      if (value1.HasValue)
      {
        if (value2.HasValue)
        {
          TimeSpan v1 = value1.MinValue;
          if (value1.MinValue < v1)
            v1 = value1.MinValue;
          TimeSpan v2 = value1.MaxValue;
          if (value2.MaxValue > v2)
            v2 = value2.MaxValue;
          return new MinMaxTimeSpan(v1, v2);
        }
        else
          return value1;
      }
      else
        return value2;
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает MinValue или null, если диапазон пустой
    /// </summary>
    public TimeSpan? NullableMinValue
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
    /// Возвращает MaxValue или null, если диапазон пустой
    /// </summary>
    public TimeSpan? NullableMaxValue
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
