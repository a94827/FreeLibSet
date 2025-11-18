// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Collections;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Фиксированный набор идентификаторов с доступом к элементам по индексу
  /// </summary>
  /// <typeparam name="T">Тип идентификатора. См. <see cref="IIdSet{T}"/> для списка доступных типов</typeparam>
  [Serializable]
  public class IdArray<T> : IIndexedIdSet<T>, IList<T>, System.Collections.IList
    where T : struct, IEquatable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает массив из заданной коллекции.
    /// Пустые и повторяющиеся элементы пропускаются.
    /// </summary>
    /// <param name="source">Исходная коллекция. Если null, то будет создан пустой массив.</param>
    public IdArray(IEnumerable<T> source)
    {
      if (source == null)
        _Items = EmptyArray<T>.Empty;
      else if (source is IdArray<T>)
        _Items = ((IdArray<T>)source)._Items; // можно использовать одну и ту же ссылку
      else if (source is IIdSet<T>)
        _Items = ((IIdSet<T>)source).ToArray(); // без проверок
      else
      {
        // Так как исходный набор - неизвестная коллекция, требуется проверять повторы и 0
        SingleScopeList<T> lst = new SingleScopeList<T>();
        foreach (T id in source)
        {
          if (id.Equals(default(T)))
            continue;
          lst.Add(id);
        }
        _Items = lst.ToArray();
      }
    }

    private IdArray(T[] items, bool dummy)
    {
      _Items = items;
    }

    #endregion

    #region Свойства

    private readonly T[] _Items;

    /// <summary>
    /// Возвращает количество идентификаторов в массиве
    /// </summary>
    public int Count { get { return _Items.Length; } }

    /// <summary>
    /// Доступ к идентификатору по индексу
    /// </summary>
    /// <param name="index">Индекс</param>
    /// <returns>Идентификатор</returns>
    public T this[int index] { get { return _Items[index]; } }

    T IList<T>.this[int index]
    {
      get { return this[index]; }
      set
      {
        throw ExceptionFactory.ObjectReadOnly(this);
      }
    }

    object IList.this[int index]
    {
      get { return this[index]; }

      set
      {
        throw ExceptionFactory.ObjectReadOnly(this);
      }
    }

    Type IIdSet.IdType { get { return typeof(T); } }

    IdSetKind IIdSet.Kind { get { return IdSetKind.Array; } }

    /// <summary>
    /// Возвращает идентификатор, если массив содержит единственный элемент.
    /// Иначе возвращается пустое значение.
    /// </summary>
    public T SingleId
    {
      get
      {
        if (Count == 1)
          return _Items[0];
        else
          return default(T);
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Создает копию массива идентификаторов в виде обычного массива.
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
      return (T[])(_Items.Clone());
    }

    /// <summary>
    /// Возвращает true, если указанный идентификатор присутствует в массиве.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
      return Array.IndexOf<T>(_Items, item) >= 0;
    }

    /// <summary>
    /// Копирует все элементы массива в указанный массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Первая позиция заполняемого массива</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="other"/> задает пустой список, то возвращается false.
    /// </summary>
    /// <param name="other">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IEnumerable<T> other)
    {
      T someMatchedId;
      return ContainsAny(other, out someMatchedId);
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="others"/> задает пустой список, то возвращается false.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <param name="someMatchedId">Сюда помещается первый совпадающий идентификатор</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IEnumerable<T> others, out T someMatchedId)
    {
      someMatchedId = default(T);
      if (others == null)
        return false;
      if (Count == 0)
        return false;

      // Если others - это IdCollection или IdList, то выгоднее проверить на их стороне, так как там есть словари
      IIdSet<T> others2 = others as IIdSet<T>;
      if (others2 != null && (!(others2 is IdArray<T>)))
        return others2.ContainsAny(this, out someMatchedId);

      // Так как Contains() - медленный метод, есть смысл создать временный словарь

      if (Count > 3)
      {
        Dictionary<T, object> dict = new Dictionary<T, object>(Count);
        foreach (T item in _Items)
          dict.Add(item, null);

        foreach (T id in others)
        {
          if (dict.ContainsKey(id))
          {
            someMatchedId = id;
            return true;
          }
        }
      }
      else
      {
        foreach (T id in others)
        {
          if (Contains(id))
          {
            someMatchedId = id;
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке есть ВСЕ идентификаторы из второго списка.
    /// Если <paramref name="others"/> задает пустой список, то возвращается true.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии всех идентификаторов</returns>
    public bool ContainsAll(IEnumerable<T> others)
    {
      if (others == null)
        return true;

      // Метод ContainsAll() не является симметричным относительно this и others, поэтому нельзя свалить оптиизацию на IdCollection или IdList

      // Так как Contains() - медленный метод, есть смысл создать временный словарь
      if (Count > 3)
      {
        Dictionary<T, object> dict = new Dictionary<T, object>(Count);
        foreach (T item in _Items)
          dict.Add(item, null);

        foreach (T id in others)
        {
          if (!dict.ContainsKey(id))
            return false;
        }
      }
      else
      {
        foreach (T id in others)
        {
          if (!Contains(id))
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Возвращает индекс идентификатора в массиве или (-1), если идентификатор не найден.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Индекс в массиве</returns>
    public int IndexOf(T id)
    {
      return Array.IndexOf<T>(_Items, id);
    }

    #endregion

    #region Заглушки

    void IList<T>.Insert(int index, T item)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IList<T>.RemoveAt(int index)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

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

    void ICollection<T>.Add(T item)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void ICollection<T>.Clear()
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    bool ICollection<T>.Remove(T item)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    bool ICollection<T>.IsReadOnly { get { return true; } }


    int IList.Add(object value)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    bool IList.Contains(object value)
    {
      if (value is T)
        return Contains((T)value);
      else
        return false;
    }

    void IList.Clear()
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    int IList.IndexOf(object value)
    {
      if (value is T)
        return IndexOf((T)value);
      else
        return -1;
    }

    void IList.Insert(int index, object value)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IList.Remove(object value)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IList.RemoveAt(int index)
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    void ICollection.CopyTo(Array array, int index)
    {
      _Items.CopyTo(array, index);
    }

    bool IList.IsFixedSize { get { return true; } }

    object ICollection.SyncRoot { get { return _Items; } }

    bool ICollection.IsSynchronized { get { return false; } }

    #endregion

    #region Перечислитель

    /// <summary>
    /// Создает перечислитель по идентификаторам
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerable<T>.Enumerator GetEnumerator()
    {
      return new ArrayEnumerable<T>(_Items).GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void IIdSet.SetReadOnly() { }

    #endregion

    #region ReadOnly

    bool IReadOnlyObject.IsReadOnly { get { return true; } }

    bool IList.IsReadOnly { get { return true; } }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion

    #region Клонирование

    IIdSet<T> ICloneableReadOnlyObject<IIdSet<T>>.Clone()
    {
      return new IdList<T>((IIdSet<T>)this);
    }

    //IIdSet ICloneableReadOnlyObject<IIdSet>.Clone() { return this; }

    object ICloneable.Clone() { return this; }

    IIdSet<T> ICloneableReadOnlyObject<IIdSet<T>>.CloneIfReadOnly()
    {
      return new IdList<T>((IIdSet<T>)this);
    }

    //IIdSet ICloneableReadOnlyObject<IIdSet>.CloneIfReadOnly()
    //{
    //  return new IdList<T>((IIdSet<T>)this);
    //}

    #endregion

    #region Статический массив

    /// <summary>
    /// Экземпляр пустого массива идентификаторов
    /// </summary>
    public static readonly IdArray<T> Empty = new IdArray<T>(null);

    /// <summary>
    /// Создание массива из одного идентификатора.
    /// Если <paramref name="id"/> задает пустое значение, возвращается пустой массив <see cref="Empty"/>.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Новый массив или пустой массив</returns>
    public static IdArray<T> FromId(T id)
    {
      if (id.Equals(default(T)))
        return Empty;
      else
        return FromArrayInternal(new T[1] { id });
    }

    /// <summary>
    /// Внутренняя версия для использования готового массива
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    internal static IdArray<T> FromArrayInternal(T[] items)
    {
      return new IdArray<T>(items, false);
    }

    #endregion
  }
}
