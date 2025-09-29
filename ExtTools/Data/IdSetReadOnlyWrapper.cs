using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Обертка только для чтения для набора идентификаторов
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class IdSetReadOnlyWrapper<T> : IIdSet<T>
    where T : struct, IEquatable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает обертку
    /// </summary>
    /// <param name="source">Исходный набор. Не может быть null.</param>
    public IdSetReadOnlyWrapper(IIdSet<T> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
    }

    private readonly IIdSet<T> _Source;

    #endregion

    #region Свойства и методы, передаваемые источнику

    /// <summary>
    /// Создает простой массив идентификаторов.
    /// Каждый раз создается новый массив.
    /// </summary>
    /// <returns>Массив</returns>
    public T[] ToArray()
    {
      return _Source.ToArray();
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="others"/> задает пустой список или равно null, то возвращается false.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IEnumerable<T> others)
    {
      return _Source.ContainsAny(others);
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="others"/> задает пустой список или равно null, то возвращается false.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <param name="someMatchedId">Сюда помещается первый совпадающий идентификатор.
    /// При этом неизвестно, какой именно идентификатор из набора будет возвращен</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IEnumerable<T> others, out T someMatchedId)
    {
      return _Source.ContainsAny(others, out someMatchedId);
    }

    /// <summary>
    /// Возвращает true, если в текущем списке есть ВСЕ идентификаторы из второго списка.
    /// Если <paramref name="others"/> задает пустой список или равно null, то возвращается true.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии всех идентификаторов</returns>
    public bool ContainsAll(IEnumerable<T> others)
    {
      return _Source.ContainsAll(others);
    }

    Type IIdSet.IdType
    {
      get { return typeof(T); }
    }

    IdSetKind IIdSet.Kind
    {
      get { return _Source.Kind; }
    }

    /// <summary>
    /// Возвращает true, если в наборе есть такой идентификатор
    /// </summary>
    /// <param name="item">Идентификатор</param>
    /// <returns>Наличие идентификатора</returns>
    public bool Contains(T item)
    {
      return _Source.Contains(item);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      _Source.CopyTo(array, arrayIndex);
    }

    int ICollection<T>.Count
    {
      get { return _Source.Count; }
    }

    /// <summary>
    /// Создает перечислитель по идентификаторам
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    /// <summary>
    /// Создает копию набора.
    /// Тип возвращаемого набора зависит от исходного набора.
    /// </summary>
    /// <returns>Новый набор</returns>
    public IIdSet<T> Clone()
    {
      return _Source.Clone();
    }

    IIdSet<T> Core.ICloneableReadOnlyObject<IIdSet<T>>.CloneIfReadOnly()
    {
      return _Source.Clone();
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Возвращает идентификатор, если коллекция содержит единственный элемент.
    /// Иначе возвращается пустое значение.
    /// </summary>
    public T SingleId { get { return _Source.SingleId; } }

    #endregion

    #region Заглушки

    void IIdSet<T>.AddRange(IEnumerable<T> source)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IIdSet<T>.RemoveRange(IEnumerable<T> source)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IIdSet<T>.RemoveOthers(IEnumerable<T> source)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IIdSet.SetReadOnly()
    {
    }

    void ICollection<T>.Add(T item)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void ICollection<T>.Clear()
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    bool ICollection<T>.IsReadOnly { get { return true; } }

    bool ICollection<T>.Remove(T item)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    bool Core.IReadOnlyObject.IsReadOnly { get { return true; } }

    void Core.IReadOnlyObject.CheckNotReadOnly()
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion
  }
}
