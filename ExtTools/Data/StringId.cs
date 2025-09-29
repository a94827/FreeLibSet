// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Строковый идентификатор.
  /// Классы, реализующие <see cref="IIdSet"/>, не могут использовать <see cref="System.String"/> в качестве идентификаторов,
  /// так как требуют <see cref="System.ValueType"/>.
  /// </summary>
  public struct StringId : IEquatable<StringId>
  {
    #region Конструктор

    /// <summary>
    /// Создает строковый идентификатор.
    /// Создает пустой идентификатор, если <paramref name="value"/> - пустая строка или null
    /// </summary>
    /// <param name="value">Значение поля</param>
    public StringId(string value)
    {
      if (String.IsNullOrEmpty(value))
        _Value = null;
      else
        _Value = value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение.
    /// Возвращает <see cref="String.Empty"/> если идентификатор пустой.
    /// </summary>
    public string Value { get { return _Value ?? String.Empty; } }
    private readonly string _Value;

    /// <summary>
    /// Возвращает <see cref="Value"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Value;
    }

    /// <summary>
    /// Преобразование в строку. Возврашает свойство <see cref="Value"/>
    /// </summary>
    /// <param name="value">Идентификатор</param>
    /// <returns>Строковое значение</returns>
    public static implicit operator String(StringId value)
    {
      return value.Value;
    }

    /// <summary>
    /// Преобразование строки в идентификатор
    /// </summary>
    /// <param name="value">Строковое значение поля</param>
    /// <returns>Идентификатор</returns>
    public static implicit operator StringId(string value)
    {
      return new StringId(value);
    }

    #endregion

    #region IEquatable<StringId>

    /// <summary>
    /// Сравнение двух идентификаторов
    /// </summary>
    /// <param name="a">Первый сравниваемый идентификатор</param>
    /// <param name="b">Второй сравниваемый идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(StringId a, StringId b)
    {
      return String.Equals(a.Value, b.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Сравнение двух идентификаторов
    /// </summary>
    /// <param name="a">Первый сравниваемый идентификатор</param>
    /// <param name="b">Второй сравниваемый идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(StringId a, StringId b)
    {
      return !String.Equals(a.Value, b.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Сравнение с другим строковым идентификатором
    /// </summary>
    /// <param name="other">Второй сравниваемый идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(StringId other)
    {
      return String.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Сравнение с другим строковым идентификатором
    /// </summary>
    /// <param name="obj">Второй сравниваемый идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is StringId)
        return (StringId)obj == this;
      else
        return false;
    }

    /// <summary>
    /// Возвращает хэш-код для использования в коллекциях
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      if (_Value == null)
        return 0;
      else
        return _Value.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс составного идентификатора, который образуется из нескольких полей.
  /// Реализуется структурами <see cref="ComplexId{T1, T2}"/> и <see cref="ComplexId{T1, T2, T3}"/>.
  /// </summary>
  public interface IComplexId
  {
    /// <summary>
    /// Возвращает true, если идентификатор не задан
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Возвращает количество частей составного идентификатора
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Возвращает одну из частей идентификатора
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    object this[int index] { get; }
  }

  /// <summary>
  /// Составной идентификатор из двух полей
  /// </summary>
  /// <typeparam name="T1">Тип первого идентификатора</typeparam>
  /// <typeparam name="T2">Тип второго идентификатора</typeparam>
  public struct ComplexId<T1, T2> : IComplexId, IEquatable<ComplexId<T1, T2>>
    where T1 : struct, IEquatable<T1>
    where T2 : struct, IEquatable<T2>
  {
    #region Конструктор

    /// <summary>
    /// Создает составное значение
    /// </summary>
    /// <param name="value1">Первое значение</param>
    /// <param name="value2">Второе значение</param>
    public ComplexId(T1 value1, T2 value2)
    {
      _Value1 = value1;
      _Value2 = value2;

      if (value1.Equals(default(T1)) != value2.Equals(default(T2)))
        throw new ArgumentException(String.Format(Res.ComplexId_Arg_PartialEmpty, ToString()));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение первого поля составного идентификатора
    /// </summary>
    public T1 Value1 { get { return _Value1; } }
    private readonly T1 _Value1;

    /// <summary>
    /// Значение второго поля составного идентификатора
    /// </summary>
    public T2 Value2 { get { return _Value2; } }
    private readonly T2 _Value2;

    /// <summary>
    /// Возвращает true, если идентификатор не задан
    /// </summary>
    public bool IsEmpty { get { return _Value1.Equals(default(T1)); } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Value1.Equals(default(T1)))
        return Res.ComplexId_Msg_ToStringEmpty;
      else
        return String.Format(Res.ComplexId_Msg_ToString2, _Value1, _Value2);
    }

    int IComplexId.Count { get { return 2; } }

    object IComplexId.this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return _Value1;
          case 1: return _Value2;
          default: throw ExceptionFactory.ArgOutOfRange("index", index, 0, 1);
        }
      }
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение двух составных идентификаторов
    /// </summary>
    /// <param name="a">Первый идентификатор</param>
    /// <param name="b">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(ComplexId<T1, T2> a, ComplexId<T1, T2> b)
    {
      return a.Value1.Equals(b.Value1) && a.Value2.Equals(b.Value2);
    }

    /// <summary>
    /// Сравнение двух составных идентификаторов
    /// </summary>
    /// <param name="a">Первый идентификатор</param>
    /// <param name="b">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(ComplexId<T1, T2> a, ComplexId<T1, T2> b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Сравнение с другим составным идентификатором
    /// </summary>
    /// <param name="other">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(ComplexId<T1, T2> other)
    {
      return Value1.Equals(other.Value1) && Value2.Equals(other.Value2);
    }

    /// <summary>
    /// Сравнение с другим составным идентификатором
    /// </summary>
    /// <param name="other">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is ComplexId<T1, T2>)
        return Equals((ComplexId<T1, T2>)other);
      else
        return false;
    }

    /// <summary>
    /// Хэш-код для использования в коллекциях
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return Value1.GetHashCode() ^ Value2.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// Составной идентификатор из трех полей
  /// </summary>
  /// <typeparam name="T1">Тип первого идентификатора</typeparam>
  /// <typeparam name="T2">Тип второго идентификатора</typeparam>
  /// <typeparam name="T3">Тип третьего идентификатора</typeparam>
  public struct ComplexId<T1, T2, T3> : IComplexId, IEquatable<ComplexId<T1, T2, T3>>
    where T1 : struct, IEquatable<T1>
    where T2 : struct, IEquatable<T2>
    where T3 : struct, IEquatable<T3>
  {
    #region Конструктор

    /// <summary>
    /// Создает составное значение
    /// </summary>
    /// <param name="value1">Первое значение</param>
    /// <param name="value2">Второе значение</param>
    /// <param name="value3">Третье значение</param>
    public ComplexId(T1 value1, T2 value2, T3 value3)
    {
      _Value1 = value1;
      _Value2 = value2;
      _Value3 = value3;

      if (value1.Equals(default(T1)) != value2.Equals(default(T2)) ||
        value1.Equals(default(T1)) != value3.Equals(default(T3)))
        throw new ArgumentException(String.Format(Res.ComplexId_Arg_PartialEmpty, ToString()));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение первого поля составного идентификатора
    /// </summary>
    public T1 Value1 { get { return _Value1; } }
    private readonly T1 _Value1;

    /// <summary>
    /// Значение второго поля составного идентификатора
    /// </summary>
    public T2 Value2 { get { return _Value2; } }
    private readonly T2 _Value2;

    /// <summary>
    /// Значение третьего поля составного идентификатора
    /// </summary>
    public T3 Value3 { get { return _Value3; } }
    private readonly T3 _Value3;

    /// <summary>
    /// Возвращает true, если идентификатор не задан
    /// </summary>
    public bool IsEmpty { get { return _Value1.Equals(default(T1)); } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Value1.Equals(default(T1)))
        return Res.ComplexId_Msg_ToStringEmpty;
      else
        return String.Format(Res.ComplexId_Msg_ToString3, _Value1, _Value2, _Value3);
    }

    int IComplexId.Count { get { return 3; } }

    object IComplexId.this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return _Value1;
          case 1: return _Value2;
          case 2: return _Value3;
          default: throw ExceptionFactory.ArgOutOfRange("index", index, 0, 2);
        }
      }
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение двух составных идентификаторов
    /// </summary>
    /// <param name="a">Первый идентификатор</param>
    /// <param name="b">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(ComplexId<T1, T2, T3> a, ComplexId<T1, T2, T3> b)
    {
      return a.Value1.Equals(b.Value1) && a.Value2.Equals(b.Value2) && a.Value3.Equals(b.Value3);
    }

    /// <summary>
    /// Сравнение двух составных идентификаторов
    /// </summary>
    /// <param name="a">Первый идентификатор</param>
    /// <param name="b">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(ComplexId<T1, T2, T3> a, ComplexId<T1, T2, T3> b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Сравнение с другим составным идентификатором
    /// </summary>
    /// <param name="other">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(ComplexId<T1, T2, T3> other)
    {
      return Value1.Equals(other.Value1) && Value2.Equals(other.Value2) && Value3.Equals(other.Value3);
    }


    /// <summary>
    /// Сравнение с другим составным идентификатором
    /// </summary>
    /// <param name="other">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (other is ComplexId<T1, T2, T3>)
        return Equals((ComplexId<T1, T2, T3>)other);
      else
        return false;
    }

    /// <summary>
    /// Хэш-код для использования в коллекциях
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return Value1.GetHashCode() ^ Value2.GetHashCode() ^ Value3.GetHashCode();
    }

    #endregion
  }
}
