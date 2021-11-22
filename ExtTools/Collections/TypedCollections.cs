// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

// Типизированные коллекции общего назначения

namespace FreeLibSet.Core
{

  #region Интерфейс IReadOnlyObject

  /// <summary>
  /// Интерфейс объекта, имеющего свойство IsReadOnly, которое может быть однократно установлено в true
  /// </summary>
  public interface IReadOnlyObject
  {
    /// <summary>
    /// Возвращает true, если объект был переведен в режим "Только для чтения"
    /// </summary>
    bool IsReadOnly { get; }

    /*
     * Этот метод может быть protected
    /// <summary>
    /// Переводит объект в режим "Только для чтения". Допускается повторный вызов метода
    /// </summary>
    void SetReadOnly();
     * */

    /// <summary>
    /// Этот метод должен выбрасывать ObjectReadOnlyException, если IsReadOnly=true
    /// </summary>
    void CheckNotReadOnly();
  }

  #endregion
}

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Список пар "Код"-"Значение", отсортированный не по ключу, как SortedList, а по порядку добавления элементов
  /// Порядок пар, возвращаемых GetEnumerator(), также соответствует порядку добавления
  /// </summary>                                                
  /// <typeparam name="TKey">Тип ключа</typeparam>
  /// <typeparam name="TValue">Тип значения</typeparam>
  [Serializable]
  public class OrderSortedList<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию с параметрами по умолчанию
    /// </summary>
    public OrderSortedList()
    {
      _List = new List<TKey>();
      _Dict = new Dictionary<TKey, TValue>();
      _Keys = new KeyCollection(this);
      _Values = new ValueCollection(this);
    }

    /// <summary>
    /// Создает пустую коллекцию с заданным объектом сравнения
    /// </summary>
    public OrderSortedList(IEqualityComparer<TKey> keyComparer)
    {
      _List = new List<TKey>();
      _Dict = new Dictionary<TKey, TValue>(keyComparer);
      _KeyComparer = keyComparer;
      _Keys = new KeyCollection(this);
      _Values = new ValueCollection(this);
    }

    /// <summary>
    /// Создает коллекцию с заданной начальной емкостью.
    /// Используйте эту версию конструктора, если заранее известно число элементов
    /// </summary>
    /// <param name="capacity">Начальная емкость в коллекции</param>
    public OrderSortedList(int capacity)
    {
      _List = new List<TKey>(capacity);
      _Dict = new Dictionary<TKey, TValue>(capacity);
      _Keys = new KeyCollection(this);
      _Values = new ValueCollection(this);
    }

    /// <summary>
    /// Создает коллекцию с заданной начальной емкостью.
    /// Используйте эту версию конструктора, если заранее известно число элементов.
    /// </summary>
    /// <param name="capacity">Начальная емкость в коллекции</param>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    public OrderSortedList(int capacity, IEqualityComparer<TKey> keyComparer)
    {
      _List = new List<TKey>(capacity);
      _Dict = new Dictionary<TKey, TValue>(capacity, keyComparer);
      _KeyComparer = keyComparer;
      _Keys = new KeyCollection(this);
      _Values = new ValueCollection(this);
    }

    /// <summary>
    /// Конструктор копирования
    /// </summary>
    /// <param name="dictionary">Источник данных</param>
    public OrderSortedList(IDictionary<TKey, TValue> dictionary)
      : this(dictionary.Count)
    {
      foreach (KeyValuePair<TKey, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    /// <summary>
    /// Конструктор копирования
    /// </summary>
    /// <param name="dictionary">Источник данных</param>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    public OrderSortedList(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer)
      : this(dictionary.Count, keyComparer)
    {
      foreach (KeyValuePair<TKey, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    // Конструктор сериализации внизу

    #endregion

    #region Коллекция ключей

    /// <summary>
    /// Реализация свойства Keys
    /// </summary>
    public sealed class KeyCollection : IList<TKey>
    {
      #region Защишенный конструктор

      internal KeyCollection(OrderSortedList<TKey, TValue> owner)
      {
        _Owner = owner;
      }

      private OrderSortedList<TKey, TValue> _Owner;

      #endregion

      #region IList<TKey> Members

      /// <summary>
      /// Возвращает индекс ключа. 
      /// Если такого ключа нет, возвращает (-1).
      /// Быстродействие метода: O(1)
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public int IndexOf(TKey item)
      {
        return _Owner._List.IndexOf(item);
      }

      void IList<TKey>.Insert(int index, TKey item)
      {
        throw new ObjectReadOnlyException();
      }

      void IList<TKey>.RemoveAt(int index)
      {
        throw new ObjectReadOnlyException();
      }

      /// <summary>
      /// Возвращает ключ в заданной позиции.
      /// Индекс должен быть в диапазоне от 0 до Count-1
      /// </summary>
      /// <param name="index">Индекс ключа</param>
      /// <returns>Ключ</returns>
      public TKey this[int index]
      {
        get
        {
          return _Owner._List[index];
        }
        set
        {
          throw new ObjectReadOnlyException();
        }
      }

      #endregion

      #region ICollection<TKey> Members

      void ICollection<TKey>.Add(TKey item)
      {
        throw new ObjectReadOnlyException();
      }

      void ICollection<TKey>.Clear()
      {
        throw new ObjectReadOnlyException();
      }

      bool ICollection<TKey>.Remove(TKey item)
      {
        throw new ObjectReadOnlyException();
      }

      /// <summary>
      /// Возвращает true, если указанный ключ существует.
      /// При поиске используется индексированный просмотр, поэтому использование этого метода предпочтительнее, чем IndexOf()
      /// </summary>
      /// <param name="item">Искомый ключ</param>
      /// <returns>true, если ключ существует</returns>
      public bool Contains(TKey item)
      {
        return _Owner._Dict.ContainsKey(item);
      }

      /// <summary>
      /// Копирует ключи в массив.
      /// Порядок ключей соответствует порядку добавления элементов в список, а не порядку ключей.
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      public void CopyTo(TKey[] array)
      {
        _Owner._List.CopyTo(array);
      }

      /// <summary>
      /// Копирует ключи в массив.
      /// Порядок ключей соответствует порядку добавления элементов в список, а не порядку ключей.
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Индекс первого элемента заполняемого массива</param>
      public void CopyTo(TKey[] array, int arrayIndex)
      {
        _Owner._List.CopyTo(array, arrayIndex);
      }

      /// <summary>
      /// Копирует часть ключей в массив.
      /// Порядок ключей соответствует порядку добавления элементов в список, а не порядку ключей.
      /// </summary>
      /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
      /// <param name="count">Количество элементов, которые нужно скопировать</param>
      public void CopyTo(int index, TKey[] array, int arrayIndex, int count)
      {
        _Owner._List.CopyTo(index, array, arrayIndex, count);
      }

      /// <summary>
      /// Возвращает количество элементов в списке
      /// </summary>
      public int Count
      {
        get { return _Owner._List.Count; }
      }

      bool ICollection<TKey>.IsReadOnly { get { return true; } }

      #endregion

      #region IEnumerable<TKey> Members

      /// <summary>
      /// Возвращает перечислитель по ключам.
      /// Ключи перебираются в порядке добавления элементов в список, а не по возрастанию ключа.
      /// 
      /// Тип возвращаемого значения может измениться в будущем, 
      /// гарантируется только реализация интерфейса перечислителя.
      /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
      /// </summary>
      /// <returns>Перечислитель</returns>
      public List<TKey>.Enumerator GetEnumerator()
      {
        return _Owner._List.GetEnumerator();
      }

      IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
      {
        return _Owner._List.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _Owner._List.GetEnumerator();
      }

      #endregion

      #region Дополнительные методы

      /// <summary>
      /// Возвращает "Count=XXX"
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return _Owner.ToString();
      }

      /// <summary>
      /// Возвращает массив ключей в порядке добавления элементов в список.
      /// </summary>
      /// <returns>Массив ключей</returns>
      public TKey[] ToArray()
      {
        return _Owner._List.ToArray();
      }

      #endregion
    }

    #endregion

    #region Коллекция значений

    /// <summary>
    /// Реализация свойства Values
    /// </summary>
    public sealed class ValueCollection : IList<TValue>
    {
      #region Защишенный конструктор

      internal ValueCollection(OrderSortedList<TKey, TValue> owner)
      {
        _Owner = owner;
      }

      private OrderSortedList<TKey, TValue> _Owner;

      #endregion

      #region IList<TValue> Members

      /// <summary>
      /// Выполняет медленный поиск значения.
      /// Если требуется только проверить наличие элемента, используйте метод Contains().
      /// </summary>
      /// <param name="item">Значение для поиска</param>
      /// <returns>Индекс элемента в списке или (-1), если значение не найдено</returns>
      public int IndexOf(TValue item)
      {
        for (int i = 0; i < _Owner._List.Count; i++)
        {
          if (object.Equals(_Owner._Dict[_Owner._List[i]], item))
            return i;
        }
        return -1;
      }

      void IList<TValue>.Insert(int index, TValue item)
      {
        throw new ObjectReadOnlyException();
      }

      void IList<TValue>.RemoveAt(int index)
      {
        throw new ObjectReadOnlyException();
      }

      /// <summary>
      /// Получить значение по индексу.
      /// Порядок элементов соответствует порядку добавления элементов в список.
      /// Установка свойства не поддерживается
      /// </summary>
      /// <param name="index">Индекс элемента от 0 до Count-1</param>
      /// <returns>Значение</returns>
      public TValue this[int index]
      {
        get
        {
          return _Owner[_Owner._List[index]];
        }
        set
        {
          throw new ObjectReadOnlyException();
        }
      }

      #endregion

      #region ICollection<TValue> Members

      void ICollection<TValue>.Add(TValue item)
      {
        throw new ObjectReadOnlyException();
      }

      void ICollection<TValue>.Clear()
      {
        throw new ObjectReadOnlyException();
      }

      bool ICollection<TValue>.Remove(TValue item)
      {
        throw new ObjectReadOnlyException();
      }

      /// <summary>
      /// Определяет наличие значения в списке.
      /// Метод работает медленно.
      /// </summary>
      /// <param name="item">искомое значение</param>
      /// <returns>Наличие значения в списке</returns>
      public bool Contains(TValue item)
      {
        return _Owner._Dict.ContainsValue(item);
      }

      /// <summary>
      /// Копирует все значения в массив.
      /// Порядок элементов соответствует порядку добавления элементов в список
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      public void CopyTo(TValue[] array)
      {
        for (int i = 0; i < _Owner._List.Count; i++)
          array[i] = _Owner[_Owner._List[i]];
      }

      /// <summary>
      /// Копирует все значения в массив.
      /// Порядок элементов соответствует порядку добавления элементов в список
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Начальный индекс в заполняемом массиве</param>
      public void CopyTo(TValue[] array, int arrayIndex)
      {
        for (int i = 0; i < _Owner._List.Count; i++)
          array[arrayIndex + i] = _Owner[_Owner._List[i]];
      }

      /// <summary>
      /// Копирует часть значений в массив.
      /// Порядок элементов соответствует порядку добавления элементов в список
      /// </summary>
      /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
      /// <param name="count">Количество элементов, которые нужно скопировать</param>
      public void CopyTo(int index, TValue[] array, int arrayIndex, int count)
      {
        for (int i = 0; i < count; i++)
          array[arrayIndex + i] = _Owner[_Owner._List[index + i]];
      }

      /// <summary>
      /// Возвращает количество элементов в списке
      /// </summary>
      public int Count
      {
        get { return _Owner._List.Count; }
      }

      bool ICollection<TValue>.IsReadOnly { get { return true; } }

      #endregion

      #region IEnumerable<TValue> Members

      /// <summary>
      /// Реализация перечислителя по значениям.
      /// Порядок перебора соответствует порядку добавления элементов в список
      /// </summary>
      [Serializable]
      [StructLayout(LayoutKind.Auto)]
      public struct Enumerator : IEnumerator<TValue>
      {
        #region Конструктор

        internal Enumerator(OrderSortedList<TKey, TValue> owner)
        {
          _Owner = owner;
          _CurrIndex = -1;
        }

        #endregion

        #region Поля

        private OrderSortedList<TKey, TValue> _Owner;

        private int _CurrIndex;

        #endregion

        #region IEnumerator<TValue> Members

        /// <summary>
        /// Текущий элемент списка
        /// </summary>
        public TValue Current
        {
          get { return _Owner._Dict[_Owner._List[_CurrIndex]]; }
        }

        /// <summary>
        /// Ничего не делает
        /// </summary>
        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
          get { return Current; }
        }

        /// <summary>
        /// Переход к следующему элементу списка
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
          _CurrIndex++;
          return _CurrIndex < _Owner._List.Count;
        }

        void IEnumerator.Reset()
        {
          _CurrIndex = -1;
        }

        #endregion
      }

      /// <summary>
      /// Возвращает перечислитель по значениям.
      /// Порядок перебора соответствует порядку добавления элементов в список.
      /// </summary>
      /// <returns>Перечислитель</returns>
      public Enumerator GetEnumerator()
      {
        return new Enumerator(_Owner);
      }

      IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
      {
        return new Enumerator(_Owner);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return new Enumerator(_Owner);
      }

      #endregion

      #region Дополнительные методы

      /// <summary>
      /// Возвращает "Count=XXX"
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return _Owner.ToString();
      }

      /// <summary>
      /// Возвращает массив значений.
      /// Порядок элементов соответствует порядку добавления элементов в список
      /// </summary>
      /// <returns>Массив значений</returns>
      public TValue[] ToArray()
      {
        TValue[] a = new TValue[_Owner.Count];
        CopyTo(a, 0);
        return a;
      }

      #endregion
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Линейный список, определяющий порядок ключей
    /// </summary>
    public KeyCollection Keys { get { return _Keys; } }
    [NonSerialized]
    private KeyCollection _Keys;

    /// <summary>
    /// Линейный список, определяющий порядок ключей
    /// </summary>
    public ValueCollection Values { get { return _Values; } }
    [NonSerialized]
    private ValueCollection _Values;

    /// <summary>
    /// Список ключей в порядке добавления
    /// </summary>
    [NonSerialized]
    private List<TKey> _List;

    /// <summary>
    /// Коллекция по кодам.
    /// </summary>
    [NonSerialized]
    private Dictionary<TKey, TValue> _Dict;

    /// <summary>
    /// Для восстановления поля _Dict после десериализации
    /// </summary>
    private IEqualityComparer<TKey> _KeyComparer;

    /// <summary>
    /// Доступ по ключу
    /// Если запрошен несуществуюший код, генерируется исключение.
    /// Установка значения с несуществующим кодом выполняет добавление элемента
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        return _Dict[key];
      }
      set
      {
        CheckNotReadOnly();

        Remove(key);
        Add(key, value);
      }
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список был переведен в режим "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит список в режим "Только чтение"
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добавляет элемент в список
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      CheckNotReadOnly();
      _Dict.Add(key, value);
      try
      {
        _List.Add(key);
      }
      catch
      {
        _Dict.Remove(key);
        throw;
      }
    }

    /// <summary>
    /// Возвращает true, если список содержит ключ
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Наличие элемента</returns>
    public bool ContainsKey(TKey key)
    {
      return _Dict.ContainsKey(key);
    }

    /// <summary>
    /// Удаляет из списка элемент с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если элемент был в коллекции</returns>
    public bool Remove(TKey key)
    {
      CheckNotReadOnly();

      if (_Dict.Remove(key))
      {
        _List.Remove(key);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Пытается получить элемент с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается элемент, если он есть в списке</param>
    /// <returns>Наличие ключа в списке</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _Dict.TryGetValue(key, out value);
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return Keys; } }

    ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return Values; } }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();
      _Dict.Clear();
      _List.Clear();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      return ContainsKey(item.Key); // ??
    }

    /// <summary>
    /// Копирует пары "Ключ-Значение" в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array)
    {
      for (int i = 0; i < _List.Count; i++)
        array[i] = new KeyValuePair<TKey, TValue>(_List[i], _Dict[_List[i]]);
    }

    /// <summary>
    /// Копирует пары "Ключ-Значение" в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Индекс первого элемента в массиве</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      for (int i = 0; i < _List.Count; i++)
        array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(_List[i], _Dict[_List[i]]);
    }

    /// <summary>
    /// Копирует пары "Ключ-Значение" в массив
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, KeyValuePair<TKey, TValue>[] array, int arrayIndex, int count)
    {
      for (int i = 0; i < count; i++)
        array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(_List[index + i], _Dict[_List[index + i]]);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key); // ??
    }

    /// <summary>
    /// Возвращает массив пар "Ключ-Значение"
    /// </summary>
    /// <returns>Массив</returns>
    public KeyValuePair<TKey, TValue>[] ToArray()
    {
      KeyValuePair<TKey, TValue>[] a = new KeyValuePair<TKey, TValue>[_List.Count];
      CopyTo(a);
      return a;
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Реализация перечислителя по парам "Ключ-Значение"
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
      #region Конструктор

      internal Enumerator(OrderSortedList<TKey, TValue> owner)
      {
        _Owner = owner;
        _CurrIndex = -1;
      }

      #endregion

      #region Поля

      private OrderSortedList<TKey, TValue> _Owner;

      private int _CurrIndex;

      #endregion

      #region IEnumerator<KeyValuePair<TKey, TValue>> Members

      /// <summary>
      /// Текущая пара
      /// </summary>
      public KeyValuePair<TKey, TValue> Current
      {
        get { return new KeyValuePair<TKey, TValue>(_Owner._List[_CurrIndex], _Owner._Dict[_Owner._List[_CurrIndex]]); }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      /// <summary>
      /// Переход к следующей паре
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _CurrIndex++;
        return _CurrIndex < _Owner._List.Count;
      }

      void IEnumerator.Reset()
      {
        _CurrIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по KeyValuePair,
    /// Порядок перебора соответствует порядку добавления элементов в список методом Add(), 
    /// а не порядку ключей
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region Сериализация

    /// <summary>
    /// Используется исключительно для сериализации
    /// </summary>
    private TKey[] InternalKeys;

    /// <summary>
    /// Используется исключительно для сериализации
    /// </summary>
    private TValue[] InternalValues;

    [OnSerializing]
    private void OnSerializingMethod(StreamingContext context)
    {
      InternalKeys = Keys.ToArray();
      InternalValues = Values.ToArray();
    }

    [OnSerialized]
    private void OnSerializedMethod(StreamingContext context)
    {
      // После сериализации освобождаем память
      InternalKeys = null;
      InternalValues = null;
    }

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
#if DEBUG
      if (InternalKeys == null || InternalValues == null)
        throw new BugException("Массивы не десериализованы");
#endif

      _List = new List<TKey>(InternalKeys.Length);
      _Dict = new Dictionary<TKey, TValue>(InternalKeys.Length, _KeyComparer);
      _List.AddRange(InternalKeys);
      for (int i = 0; i < InternalKeys.Length; i++)
        _Dict.Add(InternalKeys[i], InternalValues[i]);

      _Keys = new KeyCollection(this);
      _Values = new ValueCollection(this);
    }

    #endregion
  }

  /// <summary>
  /// Потокобезопасная коллекция объектов, реализующая перечислитель по копии списка.
  /// Этот класс не является сериализуемым.
  /// </summary>
  /// <typeparam name="T">Тип даннных в коллекции</typeparam>
  public class SyncCollection<T> : ICollection<T>, ICollection, IEnumerable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создание класса-оберетки для внешней коллекции
    /// </summary>
    /// <param name="source"></param>
    public SyncCollection(ICollection<T> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
    }

    /// <summary>
    /// Создание потокобезопасного списка на основе List
    /// </summary>
    public SyncCollection()
      : this(new List<T>())
    {
    }

    /// <summary>
    /// Создание потокобезопасного списка на основе List с заданной начальной емкостью
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    public SyncCollection(int capacity)
      : this(new List<T>(capacity))
    {
    }

    #endregion

    #region Свойства и методы

    /// <summary>
    /// Основная коллекция
    /// </summary>
    // ReSharper disable once InconsistentlySynchronizedField
    protected ICollection<T> Source { get { return _Source; } }
    private ICollection<T> _Source;

    /// <summary>
    /// Возвращает копию массива элементов.
    /// </summary>
    /// <returns>Новый массив</returns>
    public T[] ToArray()
    {
      T[] Res;
      lock (SyncRoot)
      {
        Res = new T[_Source.Count];
        _Source.CopyTo(Res, 0);
      }
      return Res;
    }

    /// <summary>
    /// Строковое представления "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      lock (SyncRoot)
      {
        return "Count=" + _Source.Count.ToString();
      }
    }

    /// <summary>
    /// Добавляет в список элементы из другого списка
    /// </summary>
    /// <param name="collection">Список, откуда добавляются элементы</param>
    public void AddRange(IEnumerable<T> collection)
    {
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");
      if (Object.ReferenceEquals(collection, Source))
        throw new ArgumentException("Нельзя добавить элементы из базового списка", "collection");

      lock (SyncRoot)
      {
        ResetCopyArray(); // 11.11.2016

        // Оптимиpованная загрузка
        if (_Source is List<T>)
        {
          ((List<T>)_Source).AddRange(collection);
          return;
        }

        // Поэлементная загрузка
        foreach (T Item in collection)
          _Source.Add(Item);
      }
    }

    #endregion

    #region Копия в виде массива

    /// <summary>
    /// Копия коллекции для реализации перечислителя
    /// </summary>
    private T[] _CopyArray;

    /// <summary>
    /// Получение копии массива значений
    /// На момент вызова SyncRoot должен быть заблокирован
    /// </summary>
    /// <returns></returns>
    protected T[] GetCopyArray()
    {
      if (_CopyArray == null)
      {
        // ReSharper disable once InconsistentlySynchronizedField
        _CopyArray = new T[_Source.Count];
        // ReSharper disable once InconsistentlySynchronizedField
        _Source.CopyTo(_CopyArray, 0);
      }
      return _CopyArray;
    }

    /// <summary>
    /// Сброс внутренней копии массива.
    /// Должен вызываться при внесении любых изменений в исходную коллекцию
    /// На момент вызова SyncRoot должен быть заблокирован
    /// </summary>
    protected void ResetCopyArray()
    {
      _CopyArray = null;
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в коллецию
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      lock (SyncRoot)
      {
        ResetCopyArray();
        _Source.Add(item);
      }
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      lock (SyncRoot)
      {
        ResetCopyArray();
        _Source.Clear();
      }
    }

    /// <summary>
    /// Возвращает true, если коллекция содержит элемент
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>Наличие элемента в коллекции</returns>
    public bool Contains(T item)
    {
      bool Res;
      lock (SyncRoot)
      {
        Res = _Source.Contains(item);
      }
      return Res;
    }

    /// <summary>
    /// Копирование элементов в массив.
    /// Метод может быть ненадежным, если вызов Count и CopyTo выполняется вне блокировки SyncRoot
    /// Используйте метод ToArray()
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      lock (SyncRoot)
      {
        _Source.CopyTo(array, arrayIndex);
      }
    }

    /// <summary>
    /// Копирование элементов в массив.
    /// Метод может быть ненадежным, если вызов Count и CopyTo выполняется вне блокировки SyncRoot
    /// Используйте метод ToArray()
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    void ICollection.CopyTo(Array array, int index)
    {
      lock (SyncRoot)
      {
        ((ICollection)_Source).CopyTo(array, index);
      }
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count
    {
      get
      {
        int Res;
        lock (SyncRoot)
        {
          Res = _Source.Count;
        }
        return Res;
      }
    }

    /// <summary>
    /// Возвращает true, если список находится в режиме "только чтение"
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        bool Res;
        lock (SyncRoot)
        {
          Res = _Source.IsReadOnly;
        }
        return Res;
      }
    }

    /// <summary>
    /// Удавляет элемент из списка
    /// </summary>
    /// <param name="item">Элемент, подлежащий удалению</param>
    /// <returns>true, если элемент был найден и удален из списка</returns>
    public bool Remove(T item)
    {
      bool Res;
      lock (SyncRoot)
      {
        ResetCopyArray();
        Res = _Source.Remove(item);
      }
      return Res;
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по копии списка.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerator<T> GetEnumerator()
    {
      lock (SyncRoot)
      {
        return new ArrayEnumerator<T>(GetCopyArray());
      }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region ICollection Members

    bool ICollection.IsSynchronized { get { return true; } }

    /// <summary>
    /// Объект, по которому выполняется блокировка
    /// </summary>
    public object SyncRoot
    {
      // ReSharper disable once InconsistentlySynchronizedField
      get { return _Source; }
    }

    #endregion
  }

  /// <summary>
  /// Потокобезопасный словарь объектов, реализующая перечислитель по копии словаря
  /// Этот класс не является сериализуемым.
  /// </summary>
  /// <typeparam name="TKey">Тип ключа</typeparam>
  /// <typeparam name="TValue">Тип значения</typeparam>
  public class SyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>
  {
    #region Конструкторы

    /// <summary>
    /// Создание класса-оберетки для внешней коллекции
    /// </summary>
    /// <param name="source">Основная (несинхронизированная) коллекция</param>
    public SyncDictionary(IDictionary<TKey, TValue> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
    }

    /// <summary>
    /// Создание потокобезопасного списка на основе Dictionary
    /// </summary>
    public SyncDictionary()
      : this(new Dictionary<TKey, TValue>())
    {
    }

    /// <summary>
    /// Создание потокобезопасного списка на основе Dictionary
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public SyncDictionary(int capacity)
      : this(new Dictionary<TKey, TValue>(capacity))
    {
    }

    #endregion

    #region Свойства и методы

    /// <summary>
    /// Основная коллекция
    /// </summary>
    // ReSharper disable once InconsistentlySynchronizedField
    protected IDictionary<TKey, TValue> Source { get { return _Source; } }
    private IDictionary<TKey, TValue> _Source;

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>ЯТекстовое представление</returns>
    public override string ToString()
    {
      lock (SyncRoot)
      {
        return "Count=" + _Source.Count.ToString();
      }
    }

    /// <summary>
    /// Объект синхронизации
    /// </summary>
    public object SyncRoot
    {
      get
      {
        // ReSharper disable once InconsistentlySynchronizedField
        return _Source;
      }
    }

    /// <summary>
    /// Получить массив ключей
    /// </summary>
    /// <returns></returns>
    public TKey[] KeysToArray()
    {
      lock (SyncRoot)
      {
        TKey[] a = new TKey[_Source.Count];
        _Source.Keys.CopyTo(a, 0);
        return a;
      }
    }

    /// <summary>
    /// Получить массив значений
    /// </summary>
    /// <returns></returns>
    public TValue[] ValuesToArray()
    {
      lock (SyncRoot)
      {
        TValue[] a = new TValue[_Source.Count];
        _Source.Values.CopyTo(a, 0);
        return a;
      }
    }

    /// <summary>
    /// Получить массив значений
    /// </summary>
    /// <returns></returns>
    public KeyValuePair<TKey, TValue>[] ToArray()
    {
      lock (SyncRoot)
      {
        KeyValuePair<TKey, TValue>[] a = new KeyValuePair<TKey, TValue>[_Source.Count];
        _Source.CopyTo(a, 0);
        return a;
      }
    }

    #endregion

    #region Копия в виде Dictionary

    /// <summary>
    /// Копия коллекции для реализации перечислителя
    /// </summary>
    private Dictionary<TKey, TValue> _CopyDict;

    /// <summary>
    /// Получение копии коллекции
    /// На момент вызова SyncRoot должен быть заблокирован
    /// </summary>
    /// <returns></returns>
    protected Dictionary<TKey, TValue> GetCopyDictionary()
    {
      if (_CopyDict == null)
      {
        // ReSharper disable once InconsistentlySynchronizedField
        _CopyDict = new Dictionary<TKey, TValue>(_Source.Count);
        // ReSharper disable once InconsistentlySynchronizedField
        foreach (KeyValuePair<TKey, TValue> Pair in _Source)
          _CopyDict.Add(Pair.Key, Pair.Value);
      }
      return _CopyDict;
    }

    /// <summary>
    /// Сброс внутренней копии коллекции.
    /// Должен вызываться при внесении любых изменений в исходную коллекцию
    /// На момент вызова SyncRoot должен быть заблокирован
    /// </summary>
    protected void ResetCopyDictionary()
    {
      _CopyDict = null;
    }

    #endregion

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добавляет запись в словарь
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      lock (SyncRoot)
      {
        ResetCopyDictionary();
        _Source.Add(key, value);
      }
    }

    /// <summary>
    /// Возвращает true, если словарь содержит ключ
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Наличие записи</returns>
    public bool ContainsKey(TKey key)
    {
      lock (SyncRoot)
      {
        return _Source.ContainsKey(key);
      }
    }

    /// <summary>
    /// Возвращает коллекцию ключей из копии словаря.
    /// Предупреждение. Полученная коллекция ключей может разойтись с реально существуюшей, если в процессе
    /// работы с полученной коллекцией выполняется модификация основного словаря. 
    /// Также свойство может возвращать разные коллекции при последовательных обращениях. Поэтому следует
    /// сохранять полученную ссылку, если она используется несколько раз.
    /// Можно использовать метод KeysToArray()
    /// </summary>
    public ICollection<TKey> Keys
    {
      get
      {
        lock (SyncRoot)
        {
          return GetCopyDictionary().Keys;
        }
      }
    }

    /// <summary>
    /// Удаляет запись из словаря
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если запись была в словаре</returns>
    public bool Remove(TKey key)
    {
      lock (SyncRoot)
      {
        ResetCopyDictionary();
        return _Source.Remove(key);
      }
    }

    /// <summary>
    /// Пытается получить значение из словаря
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение, если запись найдена</param>
    /// <returns>true, если ключ есть в словаре</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      lock (SyncRoot)
      {
        return _Source.TryGetValue(key, out value);
      }
    }

    /// <summary>
    /// Возвращает коллекцию значений из копии словаря.
    /// Предупреждение. Полученная коллекция значений может разойтись с реально существуюшей, если в процессе
    /// работы с полученной коллекцией выполняется модификация основного словаря. 
    /// Также свойство может возвращать разные коллекции при последовательных обращениях. Поэтому следует
    /// сохранять полученную ссылку, если она используется несколько раз.
    /// Можно использовать метод ValuesToArray()
    /// </summary>
    public ICollection<TValue> Values
    {
      get
      {
        lock (SyncRoot)
        {
          return GetCopyDictionary().Values;
        }
      }
    }

    /// <summary>
    /// Чтение или установка значения по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        lock (SyncRoot)
        {
          return _Source[key];
        }
      }
      set
      {
        lock (SyncRoot)
        {
          ResetCopyDictionary();
          _Source[key] = value;
        }
      }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      lock (SyncRoot)
      {
        ResetCopyDictionary();
        _Source.Add(item);
      }
    }

    /// <summary>
    /// Очищает словарь
    /// </summary>
    public void Clear()
    {
      lock (SyncRoot)
      {
        ResetCopyDictionary();
        _Source.Clear();
      }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      lock (SyncRoot)
      {
        return _Source.Contains(item);
      }
    }

    /// <summary>
    /// Копирует пары "Ключ-Значение" в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      lock (SyncRoot)
      {
        _Source.CopyTo(array, arrayIndex);
      }
    }

    /// <summary>
    /// Возвращает количество записей в словаре
    /// </summary>
    public int Count
    {
      get
      {
        lock (SyncRoot)
        {
          return _Source.Count;
        }
      }
    }

    /// <summary>
    /// Возвращает true, если словарь находится в режиме "Только чтение"
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        lock (SyncRoot)
        {
          return _Source.IsReadOnly;
        }
      }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      lock (SyncRoot)
      {
        ResetCopyDictionary();
        return _Source.Remove(item);
      }
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель по копии словаря.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
      lock (SyncRoot)
      {
        return GetCopyDictionary().GetEnumerator();
      }
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      lock (SyncRoot)
      {
        return GetCopyDictionary().GetEnumerator();
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      lock (SyncRoot)
      {
        return GetCopyDictionary().GetEnumerator();
      }
    }

    #endregion
  }

  /// <summary>
  /// Потокобезопасная очередь.
  /// В отличие от основного класса System.Queue, не реализует интерфейс ICollection.
  /// Этот класс не является сериализуемым.
  /// </summary>
  /// <typeparam name="T">Тип хранимых значений</typeparam>
  public class SyncQueue<T> : IEnumerable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую очередь
    /// </summary>
    public SyncQueue()
    {
      _Source = new Queue<T>();
    }

    /// <summary>
    /// Создает пустую очередь заданной начальной емкости
    /// </summary>
    /// <param name="сapacity">Начальная емкость</param>
    public SyncQueue(int сapacity)
    {
      _Source = new Queue<T>(сapacity);
    }

    /// <summary>
    /// Создает обертку для существующей очереди
    /// </summary>
    /// <param name="source">Основная (непотокобезопасная) очередь</param>
    public SyncQueue(Queue<T> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Основная коллекция
    /// </summary>
    // ReSharper disable once InconsistentlySynchronizedField
    protected Queue<T> Source { get { return _Source; } }
    private Queue<T> _Source;

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      lock (SyncRoot)
      {
        return "Count=" + _Source.Count.ToString();
      }
    }

    /// <summary>
    /// Объект синхронизации
    /// </summary>
    public object SyncRoot
    {
      get
      {
        // ReSharper disable once InconsistentlySynchronizedField
        return _Source;
      }
    }

    #endregion

    #region Методы и свойства очереди

    /// <summary>
    /// Возвращает количество элементов в очереди
    /// </summary>
    public int Count
    {
      get
      {
        lock (SyncRoot)
        {
          return _Source.Count;
        }
      }
    }

    /// <summary>
    /// Оичщает очередь
    /// </summary>
    public void Clear()
    {
      lock (SyncRoot)
      {
        _Source.Clear();
      }
    }

    /// <summary>
    /// Возвращает true, если элемент есть в очереди
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      lock (SyncRoot)
      {
        return _Source.Contains(item);
      }
    }

    ///// <summary>
    ///// Копирует очередь в массив.
    ///// Элементом в позиции <paramref name="arrayIndex"/> будет голова очереди
    ///// </summary>
    ///// <param name="array">Заполняемый массив</param>
    ///// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    //void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    //{
    //  lock (SyncRoot)
    //  {
    //    FSource.CopyTo(array, arrayIndex);
    //  }
    //}

    /// <summary>
    /// Добавить элемент в очередь.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Enqueue(T item)
    {
      lock (SyncRoot)
      {
        _Source.Enqueue(item);
      }
    }

    /// <summary>
    /// Добавить элемент в очередь.
    /// Если на момент вызова в очереди уже находится <paramref name="maxCount"/>> элементов, 
    /// элемент не добавляется.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    /// <param name="maxCount">Максимальное количество элементов в очереди. Не влияет на уже существующие элементы в очереди</param>
    /// <returns>true, если элемент был добавлен</returns>
    public bool Enqueue(T item, int maxCount)
    {
      bool res;
      lock (SyncRoot)
      {
        if (_Source.Count >= maxCount)
          res = false;
        else
        {
          _Source.Enqueue(item);
          res = true;
        }
      }
      return res;
    }

    /// <summary>
    /// Извлечь элемент из очереди.
    /// Возникает исключение, если очередь пуста
    /// </summary>
    /// <returns>Извлеченный элемент</returns>
    public T Dequeue()
    {
      lock (SyncRoot)
      {
        return _Source.Dequeue();
      }
    }

    /// <summary>
    /// Выполняет попытку извлечь значение из очереди.
    /// Этого метода нет в основном объекте Queue
    /// </summary>
    /// <param name="item">Сюда помещается элемент, если его удалось извлечь</param>
    /// <returns>true, если очередь не была пуста</returns>
    public bool TryDequeue(out T item)
    {
      lock (SyncRoot)
      {
        if (_Source.Count > 0)
        {
          item = Source.Dequeue();
          return true;
        }
        else
        {
          item = default(T);
          return false;
        }
      }
    }

    /// <summary>
    /// Получить элемент из головы очереди без удаления.
    /// Возникает исключение, если очередь пуста
    /// </summary>
    /// <returns>Первый элемент в очереди</returns>
    public T Peek()
    {
      lock (SyncRoot)
      {
        return _Source.Peek();
      }
    }


    /// <summary>
    /// Выполняет попытку получить значение из головы очереди без его извлечения
    /// Этого метода нет в основном объекте Queue
    /// </summary>
    /// <param name="item">Сюда помещается элемент, если его удалось извлечь</param>
    /// <returns>true, если очередь не была пуста</returns>
    public bool TryPeek(out T item)
    {
      lock (SyncRoot)
      {
        if (_Source.Count > 0)
        {
          item = Source.Peek();
          return true;
        }
        else
        {
          item = default(T);
          return false;
        }
      }
    }

    /// <summary>
    /// Возвращает массив всех элементов в очереди.
    /// Нулевым элементом массива будет голова очереди, а последним - хвост
    /// </summary>
    /// <returns>Массив элементов</returns>
    public T[] ToArray()
    {
      lock (SyncRoot)
      {
        return _Source.ToArray();
      }
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по копии очереди.
    /// Первым будет перечислена голова очереди, а в последнюю очередь - хвост.
    /// </summary>
    /// <returns></returns>
    public ArrayEnumerator<T> GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    #endregion
  }

  /// <summary>
  /// Потокобезопасный стек.
  /// В отличие от основного класса System.Stack, не реализует интерфейс ICollection.
  /// Этот класс не является сериализуемым.
  /// </summary>
  /// <typeparam name="T">Тип хранимых значений</typeparam>
  public class SyncStack<T> : IEnumerable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой стек
    /// </summary>
    public SyncStack()
    {
      _Source = new Stack<T>();
    }

    /// <summary>
    /// Создает пустой стек заданной начальной емкости
    /// </summary>
    /// <param name="Capacity">Начальная емкость</param>
    public SyncStack(int Capacity)
    {
      _Source = new Stack<T>(Capacity);
    }

    /// <summary>
    /// Создает обертку для существующего стека.
    /// </summary>
    /// <param name="source">Базовый несиннхронизированный объект стека</param>
    public SyncStack(Stack<T> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Основная коллекция
    /// </summary>
    // ReSharper disable once InconsistentlySynchronizedField
    protected Stack<T> Source { get { return _Source; } }
    private Stack<T> _Source;

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      lock (SyncRoot)
      {
        return "Count=" + _Source.Count.ToString();
      }
    }

    /// <summary>
    /// Объект синхронизации
    /// </summary>
    public object SyncRoot
    {
      get
      {
        // ReSharper disable once InconsistentlySynchronizedField
        return _Source;
      }
    }

    #endregion

    #region Методы и свойства стека

    /// <summary>
    /// Возвращает количество элементов в стеке.
    /// </summary>
    public int Count
    {
      get
      {
        lock (SyncRoot)
        {
          return _Source.Count;
        }
      }
    }

    /// <summary>
    /// Очищает стек
    /// </summary>
    public void Clear()
    {
      lock (SyncRoot)
      {
        _Source.Clear();
      }
    }

    /// <summary>
    /// Возвращает true, если элемент есть в стеке
    /// </summary>
    /// <param name="item">Проверяемый элемент</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      lock (SyncRoot)
      {
        return _Source.Contains(item);
      }
    }

    ///// <summary>
    ///// Копирует стек в массив.
    ///// Порядок элементов соответствует порядку добавления элементов в стек. Первым элементом в массиве
    ///// будет тот, который будет извлечен последним.
    ///// </summary>
    ///// <param name="array">Заполняемый массив</param>
    ///// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    //void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    //{
    //  lock (SyncRoot)
    //  {
    //    FSource.CopyTo(array, arrayIndex);
    //  }
    //}

    /// <summary>
    /// Добавить элемент в стек.
    /// При следующем вызове Pop() или Peek() именно он будет получен.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Push(T item)
    {
      lock (SyncRoot)
      {
        _Source.Push(item);
      }
    }

    /// <summary>
    /// Добавить элемент в стек.
    /// При следующем вызове Pop() или Peek() именно он будет получен.
    /// Если в стеке уже находится <paramref name="maxCount"/> элементов, элемент не добавляется и возвращается false.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    /// <param name="maxCount">Максимальное количество элементов в стеке. Не влияет на уже существующие элементы в стеке</param>
    /// <returns>true, если элемент был добавлен</returns>
    public bool Push(T item, int maxCount)
    {
      bool res;
      lock (SyncRoot)
      {
        if (_Source.Count >= maxCount)
          res = false;
        else
        {
          _Source.Push(item);
          res = true;
        }
      }
      return res;
    }

    /// <summary>
    /// Извлечь элемент из стека.
    /// Возникает исключение, если стек пустой.
    /// Возвращается элемент, добавленный последним вызовом Push().
    /// </summary>
    /// <returns></returns>
    public T Pop()
    {
      lock (SyncRoot)
      {
        return _Source.Pop();
      }
    }

    /// <summary>
    /// Выполняет попытку извлечь значение из стека.
    /// Этого метода нет в основном объекте Stack.
    /// Возвращается элемент, добавленный последним вызовом Push().
    /// </summary>
    /// <param name="item">Сюда помещается элемент, если его удалось извлечь</param>
    /// <returns>true, если стек не был пустым</returns>
    public bool TryPop(out T item)
    {
      lock (SyncRoot)
      {
        if (_Source.Count > 0)
        {
          item = Source.Pop();
          return true;
        }
        else
        {
          item = default(T);
          return false;
        }
      }
    }

    /// <summary>
    /// Получить элемент без удаления.
    /// Возникает исключение, если стек пустой.
    /// Возвращается элемент, добавленный последним вызовом Push().
    /// </summary>
    /// <returns>Элемент, который будет извлечен следующим вызовом Pop()</returns>
    public T Peek()
    {
      lock (SyncRoot)
      {
        return _Source.Peek();
      }
    }

    /// <summary>
    /// Выполняет попытку получить значение из стека без извлечения.
    /// Этого метода нет в основном объекте Stack.
    /// </summary>
    /// <param name="item">Сюда помещается элемент, если его удалось извлечь</param>
    /// <returns>true, если очередь не была пуста</returns>
    public bool TryPeek(out T item)
    {
      lock (SyncRoot)
      {
        if (_Source.Count > 0)
        {
          item = Source.Peek();
          return true;
        }
        else
        {
          item = default(T);
          return false;
        }
      }
    }

    /// <summary>
    /// Возвращает массив, содержащий копию стека.
    /// Первым элементом массива будет тот, который будет извлечен в последнюю очередь.
    /// </summary>
    /// <returns>Массив</returns>
    public T[] ToArray()
    {
      lock (SyncRoot)
      {
        return _Source.ToArray();
      }
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по копии стека.
    /// При перечислении, первым будет получен элемент, который был первым добавлен, и который
    /// будет извлечен Pop() в последнюю очередь.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerator<T> GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    #endregion
  }


  /// <summary>
  /// Оболочка для интерфейса ICollection, реализующая доступ только для чтения.
  /// Вызов Add(), Remove() и Clear() вызывает исключение
  /// Стандартный класс ReadOnlyCollection реализует интерфейс IList, а не ICollection.
  /// Этот класс не является сериализуемым.
  /// </summary>
  /// <typeparam name="T">Тип значений в коллекции</typeparam>
  public class ReadOnlyCollectionWrapper<T> : ICollection<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает обертку для коллекции.
    /// </summary>
    /// <param name="source">Исходная коллекция. Не может быть null</param>
    public ReadOnlyCollectionWrapper(ICollection<T> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основная коллекция
    /// </summary>
    protected ICollection<T> Source { get { return _Source; } }
    private ICollection<T> _Source;

    /// <summary>
    /// Возвращает текстовое представление исходной коллекции (например, "Count=XXX") и " (ReadOnly)".
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Source.ToString() + " (ReadOnly)";
    }

    #endregion

    #region ICollection<T> Members

    void ICollection<T>.Add(T item)
    {
      throw new ObjectReadOnlyException();
    }

    void ICollection<T>.Clear()
    {
      throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Возвращает true, если элемент есть в коллекции
    /// </summary>
    /// <param name="item">Проверяемый элемент</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      return _Source.Contains(item);
    }

    /// <summary>
    /// Копирует коллекцию в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _Source.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в коллекции
    /// </summary>
    public int Count
    {
      get { return _Source.Count; }
    }

    bool ICollection<T>.IsReadOnly
    {
      get { return true; }
    }

    bool ICollection<T>.Remove(T item)
    {
      throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по коллекции
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    #endregion
  }


  // Реализация синхронизированного интерфейса IList не имеет смысла
#if XXXX

  /// <summary>
  /// Расширение SyncCollection для поддержки интерфейса IList
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SyncList<T> : SyncCollection<T>, IList<T>
  { 
  #region Конструкторы

    /// <summary>
    /// Создание класса-оберетки для внешней коллекции
    /// </summary>
    /// <param name="Source"></param>
    public SyncList(IList<T> Source)
      :base(Source)
    {
    }

    /// <summary>
    /// Создание потокобезопасного списка на основе List
    /// </summary>
    public SyncList()
      :base()
    { 
    }

    /// <summary>
    /// Создание потокобезопасного списка на основе List
    /// </summary>
    public SyncList(int Capacity)
      : base(Capacity)
    {
    }

  #endregion


  #region IList<T> Members

    public int IndexOf(T item)
    {
      lock (SyncRoot)
      { 
      }
    }

    public void Insert(int index, T item)
    {
      throw new ObjectReadOnlyException();
    }

    public void RemoveAt(int index)
    {
      throw new ObjectReadOnlyException();
    }

    public T this[int index]
    {
      get
      {
        throw new NotImplementedException("The method or operation is not implemented.");
      }
      set
      {
      throw new ObjectReadOnlyException();
      }
    }

  #endregion
  }
#endif

#if XXX // 17.04.2020 Бесполезный класс, всегда можно использовать DictionaryWithReadOnly
  /// <summary>
  /// Оболочка для интерфейса IDictionary, реализующая доступ только для чтения.
  /// Вызов Add(), Remove() и Clear() вызывает исключение
  /// Этот класс не является сериализуемым.
  /// </summary>
  /// <typeparam name="TKey">Тип ключа</typeparam>
  /// <typeparam name="TValue">Тип значения</typeparam>
  public class ReadOnlyDictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>
  {
  #region Конструктор

    /// <summary>
    /// Создает обертку для словаря
    /// </summary>
    /// <param name="source">Словарь. Не может быть null</param>
    public ReadOnlyDictionaryWrapper(IDictionary<TKey, TValue> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Основной словарь
    /// </summary>
    protected IDictionary<TKey, TValue> Source { get { return _Source; } }
    private IDictionary<TKey, TValue> _Source;

    /// <summary>
    /// Возвращает текстовое представление колллекции плюс " (ReadOnly)"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Source.ToString() + " (ReadOnly)";
    }

  #endregion

  #region IDictionary<TKey,TValue> Members

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
      throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Возвращает true, если в словаре есть запись с ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Наличие записи</returns>
    public bool ContainsKey(TKey key)
    {
      return _Source.ContainsKey(key);
    }

    /// <summary>
    /// Возвращает коллекцию ключей
    /// </summary>
    public ICollection<TKey> Keys
    {
      get { return _Source.Keys; }
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
      throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Пытается получить значение из словаря
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение, если в словаре есть запись с таким ключом</param>
    /// <returns>true, если ключ найден</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _Source.TryGetValue(key, out value);
    }

    /// <summary>
    /// Возвращает коллекцию значений
    /// </summary>
    public ICollection<TValue> Values
    {
      get { return _Source.Values; }
    }

    /// <summary>
    /// Получить значение по ключу.
    /// Установка свойства вызывает исключение, т.к. коллекция находится в режиме чтения
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        return _Source[key];
      }
      set
      {
        throw new ObjectReadOnlyException();
      }
    }

  #endregion

  #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      throw new ObjectReadOnlyException();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
      throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Бесполезный метод
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      return _Source.Contains(item);
    }

    /// <summary>
    /// Копирует пары "Ключ-Значение" в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      _Source.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество записей в словаре
    /// </summary>
    public int Count
    {
      get { return _Source.Count; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
      get { return true; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      throw new ObjectReadOnlyException();
    }

  #endregion

  #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель для пар "Ключ-Значения" словаря.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

  #endregion
  }
#endif

  /// <summary>
  /// Список, который можно переводить в режим "Только для чтения"
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке</typeparam>
  [Serializable]
  public class ListWithReadOnly<T> : IList<T>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает обертку для существующего списка
    /// </summary>
    /// <param name="source">Исходный список</param>
    /// <param name="isReadOnly">Если true, то обертка сразу переводится в режим "только чтение"</param>
    public ListWithReadOnly(IList<T> source, bool isReadOnly)
    {
      if (source == null)
        throw new ArgumentNullException();

      _Source = source;
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// Создает обертку для существующего списка.
    /// Список остается доступным для записи до вызова метода SetReadOnly().
    /// </summary>
    /// <param name="Source"></param>
    protected ListWithReadOnly(IList<T> Source)
      : this(Source, false)
    {
    }

    /// <summary>
    /// Создает пустой объект List и обертку для него.
    /// Список остается доступным для записи до вызова метода SetReadOnly().
    /// Этот конструктор хотя и объявлен как public, имеет смысл только при наличии производного класса.
    /// Используется, если в производном классе не определен собственный конструктор.
    /// </summary>
    public ListWithReadOnly()
    {
      _Source = new List<T>();
    }

    /// <summary>
    /// Создает пустой объект List и обертку для него
    /// Список остается доступным для записи до вызова метода SetReadOnly().
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    protected ListWithReadOnly(int capacity)
    {
      _Source = new List<T>(capacity);
    }

    /// <summary>
    /// Создает список List, копирует в него элементы из коллекции и создает обертку для списка.
    /// Связь между внутренним списком и переданной коллекцией отсутствует
    /// </summary>
    /// <param name="collection">Коллекция, откуда берутся начальные значения для списка</param>
    /// <param name="isReadOnly">Если true, то обертка сразу переводится в режим "только чтение"</param>
    public ListWithReadOnly(ICollection<T> collection, bool isReadOnly)
    {
      _Source = new List<T>(collection);
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// Создает список List, копирует в него элементы из коллекции и создает обертку для списка.
    /// Связь между внутренним списком и переданной коллекцией отсутствует.
    /// Список остается доступным для записи до вызова метода SetReadOnly().
    /// </summary>
    /// <param name="collection">Коллекция, откуда берутся начальные значения для списка</param>
    protected ListWithReadOnly(ICollection<T> collection)
      : this(collection, false)
    {
    }

    /// <summary>
    /// Создает список List, копирует в него элементы из коллекции и создает обертку для списка.
    /// Связь между внутренним списком и переданной коллекцией отсутствует
    /// </summary>
    /// <param name="collection">Коллекция, откуда берутся начальные значения для списка</param>
    /// <param name="isReadOnly">Если true, то обертка сразу переводится в режим "только чтение"</param>
    public ListWithReadOnly(IEnumerable<T> collection, bool isReadOnly)
    {
      _Source = new List<T>(collection);
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// Создает список List, копирует в него элементы из коллекции и создает обертку для списка.
    /// Связь между внутренним списком и переданной коллекцией отсутствует
    /// Список остается доступным для записи до вызова метода SetReadOnly().
    /// </summary>
    /// <param name="collection">Коллекция, откуда берутся начальные значения для списка</param>
    protected ListWithReadOnly(IEnumerable<T> collection)
      : this(collection, false)
    {
    }

    /// <summary>
    /// Версия конструктора, предназначенная для создания фиктивных списков.
    /// </summary>
    /// <param name="isReadOnly">Если true, то обертка сразу переводится в режим "только чтение"</param>
    public ListWithReadOnly(bool isReadOnly)
      : this()
    {
      _IsReadOnly = isReadOnly;
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Основной объект коллекции
    /// </summary>
    protected IList<T> Source { get { return _Source; } }
    private IList<T> _Source;

    #endregion

    #region IList Members

    /// <summary>
    /// Возвращает индекс объект в списке или (-1), если объект не найден
    /// </summary>
    /// <param name="item">Искомый объект</param>
    /// <returns>Индекс элемента</returns>
    public int IndexOf(T item)
    {
      return _Source.IndexOf(item);
    }

    /// <summary>
    /// Добавляет элемент в заданную позицию
    /// </summary>
    /// <param name="index">Позиция элемента</param>
    /// <param name="item">Добавляемый элемент</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();
      _Source.Insert(index, item);
    }

    /// <summary>
    /// Удаляет элемент в заданной позиции
    /// </summary>
    /// <param name="index">Индекс удаляемого элемента</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();
      _Source.RemoveAt(index);
    }

    /// <summary>
    /// Получение элемента списка по индексу и замена существующего элемента
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    /// <returns>Элемент в заданной позиции</returns>
    public T this[int index]
    {
      get
      {
        return _Source[index];
      }
      set
      {
        CheckNotReadOnly();
        _Source[index] = value;
      }
    }

    /// <summary>
    /// Добавляет элемент в конец списка
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();
      _Source.Add(item);
    }

    /// <summary>
    /// Очищает списка
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();
      _Source.Clear();
    }

    /// <summary>
    /// Возвращает true, если элемент есть в списке.
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      return _Source.Contains(item);
    }

    /// <summary>
    /// Копирует элементы списка в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Позиция в заполняемом массиве</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _Source.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в массиве
    /// </summary>
    public int Count
    {
      get { return _Source.Count; }
    }

    /// <summary>
    /// Удаляет заданный элемент
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>true, если элемент был найден в списке и удален</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();
      return _Source.Remove(item);
    }

    /// <summary>
    /// Возвращает перечислитель элементов списка
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если список находится в режиме "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;


    /// <summary>
    /// Генерирует исключение, если список находится в режиме "только чтение"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Защищенный метод, который переводит список в режим "только чтения"
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Создает копию массива всех элементов в списке
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
      T[] Res = new T[_Source.Count];
      _Source.CopyTo(Res, 0);
      return Res;
    }

    /// <summary>
    /// Добавляет множество элементов в конец списка
    /// </summary>
    /// <param name="collection">Добавляемые элементы</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");
      if (Object.ReferenceEquals(collection, Source))
        throw new ArgumentException("Нельзя добавить элементы из базового списка", "collection");

      // Оптимизированная загрузка для списков
      if (Source is List<T>)
      {
        ((List<T>)Source).AddRange(collection);
        return;
      }

      // Поэлементная загрузка
      foreach (T Item in collection)
        _Source.Add(Item);
    }

    /// <summary>
    /// Текстовое предтавление "Count=XXX (ReadOnly)"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    internal void RemoveRange(int value, int v)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Коллекция, которую можно переводить в режим "Только для чтения"
  /// После установки свойства IsReadOnly, коллекция является потокобезопасной.
  /// Так как метод SetReadOnly() является защищенным, обычно требуется создавать класс-наследник.
  /// </summary>
  /// <typeparam name="TKey">Тип ключей</typeparam>
  /// <typeparam name="TValue">Тип значений</typeparam>
  [Serializable]
  public class DictionaryWithReadOnly<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает обертку вокруг существующей коллекции.
    /// </summary>
    /// <param name="source">Основная коллекция</param>
    /// <param name="isReadOnly">true, если созданная обертка сразу переводится в режим "только чтение"</param>
    public DictionaryWithReadOnly(IDictionary<TKey, TValue> source, bool isReadOnly)
    {
      if (source == null)
        throw new ArgumentNullException();

      _Source = source;
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// Создает обертку вокруг существующей коллекции.
    /// Обертка находится в режиме записи до вызова SetReadOnly().
    /// Эта версия конструктора бесполезна, если не создан производный класс, вызывающий SetReadOnly().
    /// </summary>
    /// <param name="source">Основная коллекция</param>
    protected DictionaryWithReadOnly(IDictionary<TKey, TValue> source)
      : this(source, false)
    {
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// Этот конструктор хотя и объявлен как public, имеет смысл только при наличии производного класса.
    /// Используется, если в производном классе не определен собственный конструктор.
    /// </summary>
    public DictionaryWithReadOnly()
    {
      _Source = new Dictionary<TKey, TValue>();
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// Эта версия конструктора бесполезна, если не создан производный класс, вызывающий SetReadOnly().
    /// </summary>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    public DictionaryWithReadOnly(IEqualityComparer<TKey> keyComparer)
    {
      _Source = new Dictionary<TKey, TValue>(keyComparer);
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// Эта версия конструктора бесполезна, если не создан производный класс, вызывающий SetReadOnly().
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    protected DictionaryWithReadOnly(int capacity)
    {
      _Source = new Dictionary<TKey, TValue>(capacity);
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// Эта версия конструктора бесполезна, если не создан производный класс, вызывающий SetReadOnly().
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    protected DictionaryWithReadOnly(int capacity, IEqualityComparer<TKey> keyComparer)
    {
      _Source = new Dictionary<TKey, TValue>(capacity, keyComparer);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной объект коллекции
    /// </summary>
    protected IDictionary<TKey, TValue> Source { get { return _Source; } }
    private IDictionary<TKey, TValue> _Source;

    #endregion

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добаляет запись в коллекцию
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      CheckNotReadOnly();
      _Source.Add(key, value);
    }

    /// <summary>
    /// Возвращает true, если в коллекции есть такой ключ
    /// </summary>
    /// <param name="key">Проверяемый ключ</param>
    /// <returns>Наличие записи</returns>
    public bool ContainsKey(TKey key)
    {
      return _Source.ContainsKey(key);
    }

    /// <summary>
    /// Возвращает коллекцию ключей
    /// </summary>
    public ICollection<TKey> Keys
    {
      get
      {
        return _Source.Keys;
      }
    }

    /// <summary>
    /// Удаляет запись с ключом.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если запись была в коллекции</returns>
    public bool Remove(TKey key)
    {
      CheckNotReadOnly();
      return _Source.Remove(key);
    }

    /// <summary>
    /// Пытается получить элемент коллекции
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда помещается значение, если запись найдена</param>
    /// <returns>true, если запись найдена</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _Source.TryGetValue(key, out value);
    }

    /// <summary>
    /// Возвращает коллекцию значений (без ключей)
    /// </summary>
    public ICollection<TValue> Values
    {
      get
      {
        return _Source.Values;
      }
    }

    /// <summary>
    /// Чтение и запись значения для ключа.
    /// При чтении, если ключ не найден, генерируется исключение.
    /// При записи, наличие ключа в коллекции не имеет значение
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        return _Source[key];
      }
      set
      {
        CheckNotReadOnly();
        _Source[key] = value;
      }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Добавление в виде пары Ключ-Значение
    /// </summary>
    /// <param name="item">Добавляемая пара</param>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      CheckNotReadOnly();
      _Source.Add(item);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();
      _Source.Clear();
    }

    /// <summary>
    /// Возвращает true, если в коллекции есть такая пара.
    /// Бесполезный метод.
    /// </summary>
    /// <param name="item">Проверяемая пара</param>
    /// <returns>Наличие</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return _Source.Contains(item);
    }

    /// <summary>
    /// Копирование пар Ключ-Значение в массив
    /// </summary>
    /// <param name="array">Массив для заполнения</param>
    /// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      _Source.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в коллекции
    /// </summary>
    public int Count
    {
      get { return _Source.Count; }
    }

    /// <summary>
    /// Удаляет пару.
    /// Бесполезный метод, используйте перегрузку с аргументом Key.
    /// </summary>
    /// <param name="item">Удаляемая пара</param>
    /// <returns>true, если пара была в коллекции</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      CheckNotReadOnly();
      return _Source.Remove(item);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель по парам Ключ-Значение
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Source.GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, 
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;


    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит коллекцию в режим "только чтение".
    /// Повторные вызовы игнорируются.
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }

  /// <summary>
  /// Список значений с однократным вхождением
  /// Значения null не допускаются
  /// После установки свойства ReadOnly=true, список становится потокобезопасным
  /// </summary>
  [Serializable]
  public class SingleScopeList<T> : IList<T>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public SingleScopeList()
    {
      _List = new List<T>();
      _Dict = new Dictionary<T, object>();
    }

    /// <summary>
    /// Конструктор с интерфейсом сравнения значений
    /// <param name="comparer">Реализация интерфейса для сравнения значений</param>
    /// </summary>
    public SingleScopeList(IEqualityComparer<T> comparer)
    {
      _List = new List<T>();
      _Dict = new Dictionary<T, object>(comparer);
      _Comparer = comparer;
    }

    /// <summary>
    /// Конструктор пустого списка с заданной емкостью.
    /// Его следует использовать, если число элементов списка известно заранее.
    /// Несмотря на заданный размер, можно будет добавить больше элементов
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    public SingleScopeList(int capacity)
    {
      _List = new List<T>(capacity);
      _Dict = new Dictionary<T, object>(capacity);
    }

    /// <summary>
    /// Конструктор пустого списка с заданной емкостью.
    /// Его следует использовать, если число элементов списка известно заранее.
    /// Несмотря на заданный размер, можно будет добавить больше элементов
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    /// <param name="comparer">Реализация интерфейса для сравнения значений</param>
    public SingleScopeList(int capacity, IEqualityComparer<T> comparer)
    {
      _List = new List<T>(capacity);
      _Dict = new Dictionary<T, object>(capacity, comparer);
      _Comparer = comparer;
    }

    /// <summary>
    /// Конструктор списка, заполненного элементами из другого списка.
    /// Если в исходном списке есть повторяющиеся элементы, то они отбрасываются.
    /// </summary>
    /// <param name="collection">Исходный список. Не может быть null</param>
    public SingleScopeList(ICollection<T> collection)
      : this(collection.Count)
    {
      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Конструктор списка, заполненного элементами из другого списка.
    /// Если в исходном списке есть повторяющиеся элементы, то они отбрасываются.
    /// </summary>
    /// <param name="collection">Исходный список. Не может быть null</param>
    /// <param name="comparer">Реализация интерфейса для сравнения значений</param>
    public SingleScopeList(ICollection<T> collection, IEqualityComparer<T> comparer)
      : this(collection.Count, comparer)
    {
      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Конструктор списка, заполненного элементами из другого списка.
    /// Если в исходном списке есть повторяющиеся элементы, то они отбрасываются.
    /// </summary>
    /// <param name="collection">Исходный список. Не может быть null</param>
    public SingleScopeList(IEnumerable<T> collection)
      : this()
    {
      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Конструктор списка, заполненного элементами из другого списка.
    /// Если в исходном списке есть повторяющиеся элементы, то они отбрасываются.
    /// </summary>
    /// <param name="collection">Исходный список. Не может быть null</param>
    /// <param name="comparer">Реализация интерфейса для сравнения значений</param>
    public SingleScopeList(IEnumerable<T> collection, IEqualityComparer<T> comparer)
      : this(comparer)
    {
      foreach (T Item in collection)
        Add(Item);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной список
    /// </summary>
    private List<T> _List;

    /// <summary>
    /// Коллекция для проверки наличия элементов
    /// Ключ - элемент, значение не используется
    /// </summary>
    [NonSerialized]
    private Dictionary<T, object> _Dict;

    /// <summary>
    /// Для восстановления поля _Dict после десериализации
    /// </summary>
    private IEqualityComparer<T> _Comparer;

    /// <summary>
    /// Доступ по индексу. 
    /// Индекс должен быть в диапазоне от 0 до (Count-1).
    /// Установка значения элемента выполняет замену элемента на новый. При этом может возникнуть
    /// исключение, если новый элемент отличается от старого и такой элемент уже есть в списке
    /// </summary>                              
    /// <param name="index">Индекс элемента</param>
    /// <returns>Элемент</returns>
    public T this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();

        if (_Dict.ContainsKey(value))
          throw new InvalidOperationException("Значение " + value.ToString() + " уже есть в списке");

        T OldItem = _List[index];
        _Dict.Remove(OldItem);
        try
        {
          _Dict.Add(value, null);
        }
        catch
        {
          _Dict.Add(OldItem, null);
          throw;
        }
        _List[index] = value;
      }
    }

    /// <summary>
    /// Возвращает число элементов в списке
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns>Строка</returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список переведен в режим "Только для чтения"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Защищенный метод перевода в режим "Только для чтения"
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Вызов исключения ObjectReadOnlyException, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Получить перечислитель.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<T>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }


    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// Возвращает индекс элемента в массиве.
    /// Если требуется только проверить наличие элемента, следует использовать Contains().
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>Индекс</returns>
    public int IndexOf(T item)
    {
      return _List.IndexOf(item);
    }

    /// <summary>
    /// Вставить элемент в середину списка.
    /// Если элемент уже есть в списке, никаких действий не выполняется и текущая позиция не изменяется
    /// </summary>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="item">Элемент</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();

      if (_Dict.ContainsKey(item))
        return;

      _Dict.Add(item, null);
      try
      {
        _List.Insert(index, item);
      }
      catch
      {
        _Dict.Remove(item);
        throw;
      }
    }

    /// <summary>
    /// Удалить элемент в заданной позиции
    /// </summary>
    /// <param name="index">Индекс элемента дял удаления</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      T item = _List[index];
      _List.RemoveAt(index);
      _Dict.Remove(item);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списке, если его еще нет в списке
    /// </summary>
    /// <param name="item">добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      if (_Dict.ContainsKey(item))
        return;

      _Dict.Add(item, null);
      try
      {
        _List.Add(item);
      }
      catch
      {
        _Dict.Remove(item);
      }
    }

    /// <summary>
    /// Очистить список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
      _Dict.Clear();
    }

    /// <summary>
    /// Возвращает true, если элемент есть в списке.
    /// Для определения позиции элемента в списке, используйте IndexOf()
    /// </summary>
    /// <param name="item">проверяемый элемент</param>
    /// <returns>true, если элемент есть в списке</returns>
    public bool Contains(T item)
    {
      return _Dict.ContainsKey(item);
    }

    /// <summary>
    /// Удаляет элемент, если он есть в списке
    /// </summary>
    /// <param name="item">Удаляемый элемент</param>
    /// <returns>true, если элемент был в списке</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      if (_List.Remove(item))
      {
        _Dict.Remove(item);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Методы, адресуемые внутреннему списку

    /// <summary>
    /// Копирование всех элементов списка в массив, начиная с начала массива
    /// </summary>
    /// <param name="array">Заполняемый массив элементов</param>
    public void CopyTo(T[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// Копирование всех элементов списка в массив, начиная с указанного индекса
    /// </summary>
    /// <param name="array">Заполняемый массив элементов</param>
    /// <param name="arrayIndex">Индекс в массиве <paramref name="array"/>, куда записывается первый элемент списка</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирование всех элементов списка в массив, начиная с указанного индекса
    /// </summary>
    /// <param name="index">Начальная позиция в текущем списке SingleScopeList, начиная с которой начинать копирование.</param>
    /// <param name="array">Заполняемый массив элементов</param>
    /// <param name="arrayIndex">Индекс в массиве <paramref name="array"/>, куда записывается первый элемент списка</param>
    /// <param name="count">Количество копируемых элементов</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }


    /// <summary>
    /// Сортировка списка с использованием правил сравнения по умолчанию.
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      _List.Sort();
    }

    /// <summary>
    /// Сортировка списка с использованием заданного компаратора
    /// </summary>
    /// <param name="comparer">Компаратор. Если null, то используются правила сравнения по умолчанию</param>
    public void Sort(IComparer<T> comparer)
    {
      CheckNotReadOnly();

      _List.Sort(comparer);
    }

    /// <summary>
    /// Сортировка списка с использованием заданного метода сравнение
    /// </summary>
    /// <param name="comparison">Делегат, выполняющий сравнение</param>
    public void Sort(Comparison<T> comparison)
    {
      CheckNotReadOnly();

      _List.Sort(comparison);
    }

    /// <summary>
    /// Заменяет порядок элементов на обратный
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _List.Reverse();
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Возвращает массив элементов в списке
    /// </summary>
    /// <returns>Массив</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// Групповое добавление элементов списка
    /// В исходной коллекции могут быть одинаковые элементы, которые пропускаются
    /// </summary>
    /// <param name="collection"></param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      foreach (T Item in collection)
        Add(Item);
    }

    #endregion

    #region Десериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<T, object>(_List.Count, _Comparer);
      for (int i = 0; i < _List.Count; i++)
        _Dict.Add(_List[i], null);
    }

    #endregion
  }

  /// <summary>
  /// Список значением с однократным вхождением и сортировкой.
  /// Значения null не допускаются.
  /// После установки свойства ReadOnly=true, список становится потокобезопасным.
  /// Является надстройкой над SortedList, используя только ключи.
  /// </summary>
  [Serializable]
  public class SingleScopeSortedList<T> : IList<T>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public SingleScopeSortedList()
    {
      _List = new SortedList<T, object>();
    }

    /// <summary>
    /// Создает пустой список
    /// <param name="comparer">Реализация интерфейса для сравнения элементов списка на больше/меньше для сортировки</param>
    /// </summary>
    public SingleScopeSortedList(IComparer<T> comparer)
    {
      _List = new SortedList<T, object>(comparer);
    }

    /// <summary>
    /// Создает пустой список заданной начальной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость</param>
    public SingleScopeSortedList(int capacity)
    {
      _List = new SortedList<T, object>(capacity);
    }

    /// <summary>
    /// Создает пустой список заданной начальной емкости
    /// <param name="comparer">Реализация интерфейса для сравнения элементов списка на больше/меньше для сортировки</param>
    /// </summary>
    /// <param name="capacity">Начальная емкость</param>
    public SingleScopeSortedList(int capacity, IComparer<T> comparer)
    {
      _List = new SortedList<T, object>(capacity, comparer);
    }

    /// <summary>
    /// Создает список и копирует в него элементы из коллекции.
    /// Если в коллекции есть повторяеющиеся элементы, они отбрасываются.
    /// Если в коллекции есть значения null, возникает исключение.
    /// </summary>
    /// <param name="collection">Исходная коллекция</param>
    public SingleScopeSortedList(ICollection<T> collection)
      : this(collection.Count)
    {
      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Создает список и копирует в него элементы из коллекции.
    /// Если в коллекции есть повторяеющиеся элементы, они отбрасываются.
    /// Если в коллекции есть значения null, возникает исключение.
    /// </summary>
    /// <param name="collection">Исходная коллекция</param>
    /// <param name="comparer">Реализация интерфейса для сравнения элементов списка на больше/меньше для сортировки</param>
    public SingleScopeSortedList(ICollection<T> collection, IComparer<T> comparer)
      : this(collection.Count, comparer)
    {
      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Создает список и копирует в него элементы из коллекции.
    /// Если в коллекции есть повторяеющиеся элементы, они отбрасываются.
    /// Если в коллекции есть значения null, возникает исключение.
    /// </summary>
    /// <param name="collection">Исходная коллекция</param>
    public SingleScopeSortedList(IEnumerable<T> collection)
      : this()
    {
      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Создает список и копирует в него элементы из коллекции.
    /// Если в коллекции есть повторяеющиеся элементы, они отбрасываются.
    /// Если в коллекции есть значения null, возникает исключение.
    /// </summary>
    /// <param name="collection">Исходная коллекция</param>
    /// <param name="comparer">Реализация интерфейса для сравнения элементов списка на больше/меньше для сортировки</param>
    public SingleScopeSortedList(IEnumerable<T> collection, IComparer<T> comparer)
      : this(comparer)
    {
      foreach (T Item in collection)
        Add(Item);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной список
    /// </summary>
    private SortedList<T, object> _List;

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// Доступ по индексу.
    /// </summary>
    /// <param name="index">Индекс в диапазоне от 0 до (Count-1) </param>
    /// <returns>Элемент списка</returns>
    public T this[int index] { get { return _List.Keys[index]; } }

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список находится в режиме "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит список в режим "только чтение".
    /// Повторные вызовы метода игнорируются.
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Выбрасывает исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по списку
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return _List.Keys.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.Keys.GetEnumerator();
    }

    #endregion

    #region IList<T> Members


    T IList<T>.this[int index]
    {
      get { return _List.Keys[index]; }
      set
      {
        throw new NotSupportedException("Допускается только чтение значения по индексу");
      }
    }

    /// <summary>
    /// Возвращает индекс элемента в списке.
    /// Если требуется только проверить наличие элемента, используйте Contains()
    /// </summary>
    /// <param name="item">Проверяемый элемент</param>
    /// <returns>Индекс элемента в списке</returns>
    public int IndexOf(T item)
    {
      return _List.Keys.IndexOf(item);
    }

    /// <summary>
    /// Вставляет элемент в указанную позицию
    /// </summary>
    /// <param name="index">Индекс для вставки</param>
    /// <param name="item">Добавляемый элемент</param>
    void IList<T>.Insert(int index, T item)
    {
      throw new NotSupportedException("Используйте метод Add()");
    }

    /// <summary>
    /// Удаляет элемент в указанной позиции
    /// </summary>
    /// <param name="index">Позиция от 0 до Count-1</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      _List.RemoveAt(index);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списка.
    /// Если в списке уже есть такой элемент, он не добавляется.
    /// Если <paramref name="item"/>=null, выбрасывается исключение.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      if (_List.ContainsKey(item))
        return;

      _List.Add(item, null);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
    }

    /// <summary>
    /// Проверяет наличие элемента в списке.
    /// В отличие от метода IndexOf() и метода Contains() в обычных списках,
    /// этот метод работает быстро.
    /// </summary>
    /// <param name="item">Проверяемый элемент</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      return _List.ContainsKey(item);
    }

    /// <summary>
    /// Копирует элементы в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(T[] array)
    {
      _List.Keys.CopyTo(array, 0);
    }

    /// <summary>
    /// Копирует элементы в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирует элементы в массив
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      // Нет готового метода
      for (int i = 0; i < count; i++)
        array[arrayIndex + i] = _List.Keys[index + i];
    }

    /// <summary>
    /// Удаляет элемент из списка
    /// </summary>
    /// <param name="item">Удаляемый элемент</param>
    /// <returns>true, если элемент был в списке</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      return _List.Remove(item);
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Создает массив со всеми элементами списка
    /// </summary>
    /// <returns>Массив</returns>
    public T[] ToArray()
    {
      T[] a = new T[_List.Count];
      _List.Keys.CopyTo(a, 0);
      return a;
    }

    /// <summary>
    /// Групповое добавление элементов списка
    /// В исходной коллекции могут быть одинаковые элементы, которые пропускаются
    /// </summary>
    /// <param name="collection"></param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Возвращает true, если в текущем списке есть хотя бы один элемент из другого списка
    /// </summary>
    /// <param name="collection">Список, содержащий проверяемые значения</param>
    /// <returns>Результат проверки</returns>
    public bool ContainsAny(IEnumerable<T> collection)
    {
      if (collection == null)
        return false;
      foreach (T x in collection)
      {
        if (_List.ContainsKey(x))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке есть все элементы из другого списка.
    /// </summary>
    /// <param name="collection">Список, содержащий проверяемые значения</param>
    /// <returns>Результат проверки</returns>
    public bool ContainsAll(IEnumerable<T> collection)
    {
      if (collection == null)
        return true;
      foreach (T x in collection)
      {
        if (!_List.ContainsKey(x))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке содержаться точно те же элементы, что и другом спискн.
    /// Порядок элементов значения не имеет
    /// </summary>
    /// <param name="collection">Список, содержащий проверяемые значения</param>
    /// <returns>Результат проверки</returns>
    public bool ContainsSame(IEnumerable<T> collection)
    {
      if (collection == null)
        return _List.Count == 0;

      int cnt = 0;
      foreach (T x in collection)
        cnt++;

      if (cnt != _List.Count)
        return false;

      foreach (T x in collection)
      {
        if (!_List.ContainsKey(x))
          return false;
      }

      return true;
    }

    #endregion
  }

  /// <summary>
  /// Реализация типизированной Hashtable
  /// Простая надстройка над словарем Dictionary с переопределенным доступом по ключу
  /// Класс не является потокобезопасным
  /// </summary>
  /// <typeparam name="TKey">Ключ</typeparam>
  /// <typeparam name="TValue">Значение</typeparam>
  [Serializable]
  public class Hashtable<TKey, TValue> : Dictionary<TKey, TValue>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую таблицу
    /// </summary>
    public Hashtable()
    {
    }

    /// <summary>
    /// Создает таблицу и заполняет ее значениями из словаря
    /// </summary>
    /// <param name="dictionary">Словарь</param>
    public Hashtable(IDictionary<TKey, TValue> dictionary)
      : base(dictionary)
    {
    }

    /// <summary>
    /// Создает пустую таблицу с заданным сравнивателем
    /// </summary>
    /// <param name="comparer">Сравниватель элементов</param>
    public Hashtable(IEqualityComparer<TKey> comparer)
      : base(comparer)
    {
    }

    /// <summary>
    /// Создает пустую таблицу заданной начальной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость</param>
    public Hashtable(int capacity)
      : base(capacity)
    {
    }

    /// <summary>
    /// Создает таблицу и заполняет ее значениями из словаря
    /// </summary>
    /// <param name="dictionary">Словарь</param>
    /// <param name="comparer">Сравниватель элементов</param>
    public Hashtable(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
      : base(dictionary, comparer)
    {
    }

    /// <summary>
    /// Создает пустую таблицу заданной начальной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость</param>
    /// <param name="comparer">Сравниватель элементов</param>
    public Hashtable(int capacity, IEqualityComparer<TKey> comparer)
      : base(capacity, comparer)
    {
    }

    /// <summary>
    /// Нужен для десериализации коллекции
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected Hashtable(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Доступ к значению
    /// При считывании значения для несуществующего ключа, возвращается значение по умолчанию
    /// При записи значения для несуществующего ключа, добавляется пара ключ+значение. При записи
    /// для существующего ключа выполняется перезапись значения
    /// </summary>
    /// <param name="Key"></param>
    /// <returns></returns>
    public new TValue this[TKey Key]
    {
      get
      {
        TValue Res;
        if (base.TryGetValue(Key, out Res))
          return Res;
        else
          return default(TValue);
      }
      set
      {
        if (base.ContainsKey(Key))
          base[Key] = value;
        else
          base.Add(Key, value);
      }
    }

    #endregion
  }

  /// <summary>
  /// Реализация типизированной Hashtable с возможностью перевода в режим просмотра
  /// Простая надстройка над словарем Dictionary с переопределенным доступом по ключу
  /// После перевода в режим ReadOnly, коллекция становится потокобезопасной
  /// </summary>
  /// <typeparam name="TKey">Ключ</typeparam>
  /// <typeparam name="TValue">Значение</typeparam>
  [Serializable]
  public class HashtableWithReadOnly<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую таблицу
    /// </summary>
    protected HashtableWithReadOnly()
    {
      _Items = new Dictionary<TKey, TValue>();
    }

    /// <summary>
    /// Создает таблицу и заполняет ее элементами из словаря
    /// </summary>
    /// <param name="dictionary">Исходный словарь</param>
    protected HashtableWithReadOnly(IDictionary<TKey, TValue> dictionary)
    {
      _Items = new Dictionary<TKey, TValue>(dictionary);
    }

    /// <summary>
    /// Создает таблицу и заполняет ее элементами из словаря
    /// </summary>
    /// <param name="dictionary">Исходный словарь</param>
    /// <param name="isReadOnly">Позволяет сразу перевести таблицу в режим "только чтение"</param>
    public HashtableWithReadOnly(IDictionary<TKey, TValue> dictionary, bool isReadOnly)
    {
      _Items = new Dictionary<TKey, TValue>(dictionary);
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// Создает пустую таблицу
    /// </summary>
    /// <param name="comparer">Сравниватель</param>
    protected HashtableWithReadOnly(IEqualityComparer<TKey> comparer)
    {
      _Items = new Dictionary<TKey, TValue>(comparer);
    }

    /// <summary>
    /// Создает пустую таблицу заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость</param>
    protected HashtableWithReadOnly(int capacity)
    {
      _Items = new Dictionary<TKey, TValue>(capacity);
    }


    /// <summary>
    /// Создает таблицу и заполняет ее элементами из словаря
    /// </summary>
    /// <param name="dictionary">Исходный словарь</param>
    /// <param name="comparer">Cравниватель</param>
    protected HashtableWithReadOnly(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
      _Items = new Dictionary<TKey, TValue>(dictionary, comparer);
    }

    /// <summary>
    /// Создает таблицу и заполняет ее элементами из словаря
    /// </summary>
    /// <param name="dictionary">Исходный словарь</param>
    /// <param name="comparer">Cравниватель</param>
    /// <param name="IsReadOnly">Позволяет сразу перевести таблицу в режим "только чтение"</param>
    public HashtableWithReadOnly(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer, bool IsReadOnly)
    {
      _Items = new Dictionary<TKey, TValue>(dictionary, comparer);
      _IsReadOnly = IsReadOnly;
    }

    /// <summary>
    /// Создает пустую таблицу заданной начальной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость</param>
    /// <param name="comparer">Cравниватель</param>
    protected HashtableWithReadOnly(int capacity, IEqualityComparer<TKey> comparer)
    {
      _Items = new Dictionary<TKey, TValue>(capacity, comparer);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной объект
    /// </summary>
    private Dictionary<TKey, TValue> _Items;

    /// <summary>
    /// Доступ к значению
    /// При считывании значения для несуществующего ключа, возвращается значение по умолчанию
    /// При записи значения для несуществующего ключа, добавляется пара ключ+значение. При записи
    /// для существующего ключа выполняется перезапись значения
    /// </summary>
    /// <param name="Key">Ключ</param>
    /// <returns>Элемент коллекции</returns>
    public TValue this[TKey Key]
    {
      get
      {
        TValue Res;
        if (_Items.TryGetValue(Key, out Res))
          return Res;
        else
          return default(TValue);
      }
      set
      {
        CheckNotReadOnly();
        if (_Items.ContainsKey(Key))
          _Items[Key] = value;
        else
          _Items.Add(Key, value);
      }
    }

    #endregion

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добавляет элемент в таблицу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      CheckNotReadOnly();
      _Items.Add(key, value);
    }

    /// <summary>
    /// Определяет наличие ключа в таблице
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если запись есть в таблице</returns>
    public bool ContainsKey(TKey key)
    {
      return _Items.ContainsKey(key);
    }

    /// <summary>
    /// Коллекция ключей
    /// </summary>
    public ICollection<TKey> Keys
    {
      get { return _Items.Keys; }
    }

    /// <summary>
    /// Удаляет запись с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если запись была удалена</returns>
    public bool Remove(TKey key)
    {
      CheckNotReadOnly();
      return _Items.Remove(key);
    }

    /// <summary>
    /// Пытается получить значение с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение, если ключ найден</param>
    /// <returns>true, если ключ есть в таблице</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _Items.TryGetValue(key, out value);
    }

    /// <summary>
    /// Коллекция значений
    /// </summary>
    public ICollection<TValue> Values
    {
      get { return _Items.Values; }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Добавляет элемент в таблицу
    /// </summary>
    /// <param name="item">Пара Ключ-Значение</param>
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      CheckNotReadOnly();
      ((ICollection<KeyValuePair<TKey, TValue>>)_Items).Add(item);
    }

    /// <summary>
    /// Очищает таблицу
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();
      _Items.Clear();
    }

    /// <summary>
    /// Определяет наличие пары в таблице.
    /// Бесполезный метод.
    /// </summary>
    /// <param name="item">Проверяемая пара</param>
    /// <returns>Наличие пары</returns>
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      return ((ICollection<KeyValuePair<TKey, TValue>>)_Items).Contains(item);
    }

    /// <summary>
    /// Копирует коллекцию в массив пар.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальная позиция в заполняемом массиве</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<TKey, TValue>>)_Items).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество записей в таблице
    /// </summary>
    public int Count
    {
      get { return _Items.Count; }
    }

    /// <summary>
    /// Удаляет пару из коллекции
    /// </summary>
    /// <param name="item">Пара</param>
    /// <returns>true, если пара была найдена и удалена</returns>
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      CheckNotReadOnly();
      return ((ICollection<KeyValuePair<TKey, TValue>>)_Items).Remove(item);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель по парам "Ключ-Значение".
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если таблица переведена в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Выбрасывает исключение, если таблица переведена в режим "только чтение"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит таблицу в режим "Только чтение".
    /// Потворные вызовы метода игнорируются.
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion
  }



  /// <summary>
  /// Двусторонняя коллекция, в которой можно получить не только значение для ключа, но и ключ для значения
  /// Содержит два объекта Dictionary
  /// </summary>
  /// <typeparam name="TKey">Тип ключа</typeparam>
  /// <typeparam name="TValue">Тип значения</typeparam>
  [Serializable]
  public class BidirectionalDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    public BidirectionalDictionary()
    {
      _MainDict = new Dictionary<TKey, TValue>();
    }

    /// <summary>
    /// Создает пустую коллекцию с заданной емкостью
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public BidirectionalDictionary(int capacity)
    {
      _MainDict = new Dictionary<TKey, TValue>(capacity);
    }

    /// <summary>
    /// Создает коллекцию на основании другой коллекции
    /// </summary>
    /// <param name="dictionary">Исходная коллекция. Не может быть null</param>
    public BidirectionalDictionary(IDictionary<TKey, TValue> dictionary)
    {
#if DEBUG
      if (dictionary == null)
        throw new ArgumentNullException("dictionary");
#endif

      _MainDict = new Dictionary<TKey, TValue>(dictionary);
      PrepareReversed(); // 19.11.2021 - пусть исключение сразу вылезет, а ни когда-нибудь потом
    }

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    /// <param name="keyComparer">Сравнитель для ключей</param>
    /// <param name="valueComparer">Сравнитель для значений</param>
    public BidirectionalDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      _MainDict = new Dictionary<TKey, TValue>(keyComparer);
      _ValueComparer = valueComparer;
    }

    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="keyComparer">Сравнитель для ключей</param>
    /// <param name="valueComparer">Сравнитель для значений</param>
    public BidirectionalDictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      _MainDict = new Dictionary<TKey, TValue>(capacity, keyComparer);
      _ValueComparer = valueComparer;
    }

    /// <summary>
    /// Создает коллекцию на основании другой коллекции
    /// </summary>
    /// <param name="dictionary">Исходная коллекция. Не может быть null</param>
    /// <param name="keyComparer">Сравнитель для ключей</param>
    /// <param name="valueComparer">Сравнитель для значений</param>
    public BidirectionalDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      _MainDict = new Dictionary<TKey, TValue>(dictionary, keyComparer);
      _ValueComparer = valueComparer;
      PrepareReversed(); // 19.11.2021 - пусть исключение сразу вылезет, а ни когда-нибудь потом
    }

    #endregion

    #region Основная коллекция

    /// <summary>
    /// Основная коллекция
    /// </summary>
    private Dictionary<TKey, TValue> _MainDict;

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добавить элемент в коллекцию.
    /// И ключ и значение должны быть уникальными и не могут быть null.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      CheckNotReadOnly();

      _MainDict.Add(key, value);
      try
      {
        if (_ReversedDict != null)
          _ReversedDict.Add(value, key);
      }
      catch
      {
        _MainDict.Remove(key);
        _ReversedDict = null;
        throw;
      }
    }

    /// <summary>
    /// Определяет наличие ключа в коллекции
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если в коллекции есть запись с заданным ключом</returns>
    public bool ContainsKey(TKey key)
    {
      return _MainDict.ContainsKey(key);
    }

    /// <summary>
    /// Возвращает коллекцию ключей
    /// </summary>
    public ICollection<TKey> Keys
    {
      get { return _MainDict.Keys; }
    }

    /// <summary>
    /// Удаляет запись с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если запись найдена и была удалена</returns>
    public bool Remove(TKey key)
    {
      CheckNotReadOnly();

      TValue Value;
      if (_MainDict.TryGetValue(key, out Value))
      {
        if (_ReversedDict != null)
          _ReversedDict.Remove(Value);
        _MainDict.Remove(key);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Поиск ключа в коллекции.
    /// Если ключ найден, возвращается значение, соответствующее ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается найденное значение или значение default соответствующего типа</param>
    /// <returns>true, если ключ найден</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _MainDict.TryGetValue(key, out value);
    }

    /// <summary>
    /// Коллекция значений
    /// </summary>
    public ICollection<TValue> Values
    {
      get { return _MainDict.Values; }
    }

    /// <summary>
    /// Чтение или запись значения, соответствующего ключу.
    /// При попытке чтения для ключа, которого нет в коллекции, генерируется исключение.
    /// При записи свойства для несуществующего ключа выполняется добавление записи.
    /// При записи для существующего ключа сначала удаляется существующаяя запись
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        return _MainDict[key];
      }
      set
      {
        CheckNotReadOnly();

        TValue OldValue;
        if (_MainDict.TryGetValue(key, out OldValue))
        {
          Remove(key);
          try
          {
            Add(key, value);
          }
          catch
          {
            Add(key, OldValue);
            throw;
          }
        }
        else
        {
          // Просто добавляем пару
          Add(key, value);
        }
      }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Очищает коллекцию
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _MainDict.Clear();
      _ReversedDict = null;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      return _MainDict.ContainsKey(item.Key);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<TKey, TValue>>)_MainDict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в коллекции
    /// </summary>
    public int Count
    {
      get { return _MainDict.Count; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель для объектов KeyValuePair.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns></returns>
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    #endregion

    #endregion

    #region Обратная коллекция

    /// <summary>
    /// Обратная коллекция.
    /// Создается только по необходимости
    /// </summary>
    [NonSerialized]
    private Dictionary<TValue, TKey> _ReversedDict;

    /// <summary>
    /// Хранение сравнивателя значений между конструктором и вызовом метода PrepareReversed.
    /// </summary>
    private IEqualityComparer<TValue> _ValueComparer;

    private void PrepareReversed()
    {
      if (_ReversedDict != null)
        return;

      Dictionary<TValue, TKey> r2;
      if (_ValueComparer == null)
        r2 = new Dictionary<TValue, TKey>(_MainDict.Count);
      else
        r2 = new Dictionary<TValue, TKey>(_MainDict.Count, _ValueComparer); // 28.08.2019

      foreach (KeyValuePair<TKey, TValue> Pair in _MainDict)
        r2.Add(Pair.Value, Pair.Key);
      _ReversedDict = r2;
    }

    /// <summary>
    /// Возвращает true, если в коллекции содержится указанное значение
    /// </summary>
    /// <param name="value">значение для поиска в обратной коллекции</param>
    /// <returns>true, если значение существует</returns>
    public bool ContainsValue(TValue value)
    {
      PrepareReversed();
      return _ReversedDict.ContainsKey(value);
    }

    /// <summary>
    /// Попытка получить ключ по значению.
    /// Если значение <paramref name="value"/> существует, возвращает true и по ссылке <paramref name="key"/>
    /// записывается полученное значение.
    /// Если значения <paramref name="value"/> не существует, возвращается false, а а апо ссылке записывается
    /// пустое значение
    /// </summary>
    /// <param name="value">значение для поиска в обратной коллекции</param>
    /// <param name="key">ключ, соответствующий значению</param>
    /// <returns>true, если значение существует</returns>
    public bool TryGetKey(TValue value, out TKey key)
    {
      PrepareReversed();
      return _ReversedDict.TryGetValue(value, out key);
    }

    /// <summary>
    /// Удалить значение из коллекции
    /// </summary>
    /// <param name="value">значение для поиска и удаления</param>
    /// <returns>true, если значение было найдено в обратной коллекции</returns>
    public bool RemoveValue(TValue value)
    {
      CheckNotReadOnly();
      PrepareReversed();
      TKey key;
      if (_ReversedDict.TryGetValue(value, out key))
      {
        _MainDict.Remove(key);
        _ReversedDict.Remove(value);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если коллекция переведена в режим просмотра
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если коллекция находится в режиме просмотра
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит коллекцию в режим просмотра
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Возвращает строку "Count=XXX" для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Кольцевой буфер.
  /// Класс не является потокобезопасным.
  /// Буфер может содержать от 0 до Capacity элементов. 
  /// При добавлении "лишних" элементов, удаляются элемент из начала списка
  /// </summary>
  /// <typeparam name="T">Тип хранимых значений</typeparam>
  [Serializable]
  public class RingBuffer<T> : List<T>,
    IList<T> // интерфейс должен быть объявлен заново, чтобы учесть новые методы
  {
    #region Конструктор

    /// <summary>
    /// Создает кольцевой буфер заданной емкости
    /// </summary>
    /// <param name="capacity">Емкость</param>
    public RingBuffer(int capacity)
      : base(capacity)
    {
    }

    #endregion

    #region Методы добавления

    /// <summary>
    /// Добавляет элемент
    /// </summary>
    /// <param name="item">Элемент для добавления</param>
    public new void Add(T item)
    {
      while (Count >= Capacity)
        base.RemoveAt(0);

      base.Add(item);
    }

    /// <summary>
    /// Добавить несколько элементов.
    /// Если коллекция содержит больше элементов, чем Capacity, будут добавлены только последние элементы
    /// </summary>
    /// <param name="collection">Коллекция для добавления</param>
    public new void AddRange(IEnumerable<T> collection)
    {
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      // Подсчет числа добавляемых элементов
      int AddedCount = 0;
      foreach (T item in collection)
        AddedCount++;

      // Удаляем лишние элементы
      int DelCount = Count + AddedCount - Capacity;
      if (DelCount > 0)
      {
        if (DelCount >= Count)
          Clear();
        else
          base.RemoveRange(0, DelCount);
      }

      // Сколько элементов пропустить
      int SkipCount = AddedCount - Capacity;
      if (SkipCount > 0)
      {
        // Добавляем руками
#if DEBUG
        if (Count > 0)
          throw new BugException("Список должен быть пустым на момент вызова");
#endif
        int cnt = 0;
        foreach (T item in collection)
        {
          cnt++;
          if (cnt > SkipCount)
            base.Add(item);
        }
      }
      else
        base.AddRange(collection);

#if DEBUG
      if (Count > Capacity)
        throw new BugException("Неправильное число элементов после обновления");
#endif
    }

    #endregion
  }

  /// <summary>
  /// Коллекция "слабых" ссылок на объекты.
  /// Реализует перечисление по списку "живых" на момент создания перечислителя объектов.
  /// Этот класс является потокобезопасным.
  /// Коллекция не является сериализуемой.
  /// Обычно используются только методы Add() без парных вызовов Remove().
  /// Методы ToArray(), GetEnumerator() автоматически выполняют удаление "мертвых" ссылок и уменьшают размер объекта.
  /// Коллекция не может содержать ссылки null.
  /// </summary>
  /// <typeparam name="T">Тип объектов, на которые хранятся ссылки</typeparam>
  public class WeakReferenceCollection<T> : ICollection<T>
    where T : class
  {
    #region Конструктор

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    public WeakReferenceCollection()
    {
      _Refs = new List<WeakReference>();
    }

    /// <summary>
    /// Внутренний список ссылок.
    /// Этот объект используется для блокировок
    /// </summary>
    private List<WeakReference> _Refs;

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Счетчик добавлений.
    /// Когда счетчик превышает определенное значение, выполняется очистка списка от пустых ссылок.
    /// Обращение к полю выполняется из блокировки ссылки _Refs.
    /// </summary>
    private int _AddCount;

    /// <summary>
    /// После добавления указанного количества ссылок, выполняется удаление пустых ссылок в списке, чтобы он не разрастался
    /// </summary>
    private const int _MaxAddCount = 1000;

    /// <summary>
    /// Добавляет объект в коллекцию.
    /// </summary>
    /// <param name="item">Добавляемый объект. Не может быть null</param>
    public void Add(T item)
    {
      if (object.ReferenceEquals(item, null))
        throw new ArgumentNullException();

      WeakReference Ref = new WeakReference(item);

      lock (_Refs)
      {
        _AddCount++;
        if (_AddCount > _MaxAddCount)
        {
          for (int i = _Refs.Count - 1; i >= 0; i--)
          {
            if (_Refs[i].Target == null)
              _Refs.RemoveAt(i);
          }
          _AddCount = 0;
        }

        _Refs.Add(Ref);
      }
    }

    /// <summary>
    /// Очищает список ссылок
    /// </summary>
    public void Clear()
    {
      lock (_Refs)
      {
        _Refs.Clear();
        _AddCount = 0;
      }
    }

    /// <summary>
    /// Возвращает true, если есть ссылка на объект.
    /// Обычно применение этого метода бесполезно, так как даже ссылка на объект есть сейчас,
    /// она может исчезнуть в любой момент.
    /// При поиске используется сравнение ссылок, даже если для класса определен оператор сравнения или
    /// метод Equals()
    /// </summary>
    /// <param name="item">Проверяемый объект</param>
    /// <returns>true, если ссылка на объект есть в списке.</returns>
    public bool Contains(T item)
    {
      if (object.ReferenceEquals(item, null))
        return false;

      lock (_Refs)
      {
        for (int i = 0; i < _Refs.Count; i++)
        {
          object x = _Refs[i].Target;
          if (object.ReferenceEquals(x, item))
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Копирует ссылки на объекты в массив.
    /// Этот метод не должен вызываться, т.к. нельзя создать массив подходящего размера, используя
    /// свойство Count.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      lock (_Refs)
      {
        int pos = arrayIndex;
        for (int i = 0; i < _Refs.Count; i++)
        {
          object x = _Refs[i].Target;
          if (object.ReferenceEquals(x, null))
            continue;

          array[pos] = (T)x;
          pos++;
        }
      }
    }

    /// <summary>
    /// Это свойство возвращает количество "живых" ссылок.
    /// При обращении выполняется сжатие списка.
    /// Обычно не следует использовать это свойство, т.к. оно может уменьшиться в любой момент при сборке мусора.
    /// </summary>
    public int Count
    {
      get
      {
        lock (_Refs)
        {
          for (int i = _Refs.Count - 1; i >= 0; i--)
          {
            if (_Refs[i].Target == null)
              _Refs.RemoveAt(i);
          }
          _AddCount = 0;
          return _Refs.Count;
        }
      }
    }

    bool ICollection<T>.IsReadOnly { get { return false; } }

    /// <summary>
    /// Удалить элемент из коллекции.
    /// При поиске используется сравнение ссылок, даже если для класса определен оператор сравнения или метод Equals()
    /// </summary>
    /// <param name="item">Удаляемый элемент</param>
    /// <returns>true, если элемент был удален</returns>
    public bool Remove(T item)
    {
      if (object.ReferenceEquals(item, null))
        return false;

      lock (_Refs)
      {
        //for (int i = 0; i < _Refs.Count; i++)
        for (int i = _Refs.Count - 1; i >= 0; i--) // 18.11.2021 - нужно в обратном порядке
        {
          object x = _Refs[i].Target;
          if (object.ReferenceEquals(x, null))
            _Refs.RemoveAt(i); // 27.01.2021. Убираем сразу ненужные элементы
          else if (object.ReferenceEquals(x, item))
          {
            _Refs.RemoveAt(i);
            return true;
          }
        }

        // Здесь не обнуляем _AddCount, т.к. не весь список просмотрен

        return false;
      }
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Создать перечислитель.
    /// Перечисление будет выполняться по копии массива ссылок.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns></returns>
    public ArrayEnumerator<T> GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<T>(ToArray());
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает массив "живых" ссылок.
    /// Выполняется сжатие списка
    /// </summary>
    /// <returns>Массив ссылок</returns>
    public T[] ToArray()
    {
      List<T> lst = new List<T>(_Refs.Count);

      lock (_Refs)
      {
        List<int> nullRefIndices = null;

        for (int i = 0; i < _Refs.Count; i++)
        {
          T x = (T)(_Refs[i].Target);

          if (object.ReferenceEquals(x, null))
          {
            //_Refs.RemoveAt(i); // 27.01.2021
            // 18.11.2021
            // Так нельзя. Будет пропущена позиция. Удалять можно только в обратном порядке перебора. Делаем это позже
            if (nullRefIndices == null)
              nullRefIndices = new List<int>();
            nullRefIndices.Add(i);
          }
          else
            lst.Add(x);
        }

        if (nullRefIndices != null)
        {
          // Обязательно в обратном порядке
          for (int i = nullRefIndices.Count - 1; i >= 0; i--)
            _Refs.RemoveAt(nullRefIndices[i]);
        }

        _AddCount = 0;
      } // lock

      return lst.ToArray();
    }

    #endregion
  }

  #region Перечисление UnknownItemPosition

  /// <summary>
  /// Положение ненайденных элементов при вызове метода ArrayIndexer.Compare()
  /// </summary>
  public enum UnknownItemPosition
  {
    /// <summary>
    /// Ненайденные элементы располагаются в начале отсортированного списка
    /// </summary>
    First,

    /// <summary>
    /// Ненайденные элементы располагаются в конце отсортированного списка
    /// </summary>
    Last,
  }

  #endregion

  /// <summary>
  /// Простой класс, реализующий быстрый поиск элементов в массиве.
  /// Содержит методы Contains() и IndexOf().
  /// Исходный массив должен подходить в качестве ключа коллекции: элементы должны быть уникальными,
  /// значения null недопустимы.
  /// Не содержит исходного массива.
  /// Этот класс не является сериализуемым, т.к. легко может быть воссоздан.
  /// Интерфейс реализует интерфейс IComparer для сортировки других массивов и списков (метод Compare()).
  /// Класс является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Произвольный тип</typeparam>
  public class ArrayIndexer<T> : IComparer<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает индексатор для массива
    /// </summary>
    /// <param name="collection">Индексируемый массив. Не может быть null</param>
    public ArrayIndexer(T[] collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      _Dict = new Dictionary<T, int>(collection.Length);
      for (int i = 0; i < collection.Length; i++)
        _Dict.Add(collection[i], i);
    }

    /// <summary>
    /// Создает индексатор для массива
    /// </summary>
    /// <param name="collection">Индексируемый массив. Не может быть null</param>
    /// <param name="comparer">Интерфейс для сравнения элементов</param>
    public ArrayIndexer(T[] collection, IEqualityComparer<T> comparer)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      _Dict = new Dictionary<T, int>(collection.Length, comparer);
      for (int i = 0; i < collection.Length; i++)
        _Dict.Add(collection[i], i);
    }

    /// <summary>
    /// Создает индексатор для объекта, поддерживающего интерфейс ICollection 
    /// </summary>
    /// <param name="collection">Индексируемая коллекция</param>
    public ArrayIndexer(ICollection<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      _Dict = new Dictionary<T, int>(collection.Count);
      int cnt = 0;
      foreach (T Item in collection)
      {
        _Dict.Add(Item, cnt);
        cnt++;
      }
    }

    /// <summary>
    /// Создает индексатор для объекта, поддерживающего интерфейс ICollection 
    /// </summary>
    /// <param name="collection">Индексируемая коллекция</param>
    /// <param name="comparer">Интерфейс для сравнения элементов</param>
    public ArrayIndexer(ICollection<T> collection, IEqualityComparer<T> comparer)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      _Dict = new Dictionary<T, int>(collection.Count, comparer);
      int cnt = 0;
      foreach (T Item in collection)
      {
        _Dict.Add(Item, cnt);
        cnt++;
      }
    }

    #endregion

    #region Свойства

    private Dictionary<T, int> _Dict;

    /// <summary>
    /// Количество элементов в массиве
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    #endregion

    #region Методы

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Строка вида "Count=XXX"</returns>
    public override string ToString()
    {
      return "Count=" + _Dict.Count.ToString();
    }

    /// <summary>
    /// Возвращает индекс элемента в массиве.
    /// В отличие от Array.IndexOf(), выполняется быстро
    /// </summary>
    /// <param name="item">Элемент, который требуется найти</param>
    /// <returns>Индекс элемента</returns>
    public int IndexOf(T item)
    {
      int p;
      if (_Dict.TryGetValue(item, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// Возвращает true, если элемент есть в исходном массиве
    /// </summary>
    /// <param name="item">Элемент, который требуется найти</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      return _Dict.ContainsKey(item);
    }

    /// <summary>
    /// Возвращает true, если в списке содержатся все элементы, то есть если Contains() возвращает true для каждого элемента.
    /// Если проверяемый список пустой, возвращает true.
    /// </summary>
    /// <param name="items">Проверяемый список элементов</param>
    /// <returns>Наличие элементов</returns>
    public bool ContainsAll(IEnumerable<T> items)
    {
      foreach (T item in items)
      {
        if (!_Dict.ContainsKey(item))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если в списке есть хотя бы один элемент, то есть если Contains() возвращает true для какого-либо элемента
    /// Если проверяемый список пустой, возвращает false.
    /// </summary>
    /// <param name="items">Проверяемый список элементов</param>
    /// <returns>Наличие элементов</returns>
    public bool ContainsAny(IEnumerable<T> items)
    {
      foreach (T item in items)
      {
        if (_Dict.ContainsKey(item))
          return true;
      }
      return false;
    }

    #endregion

    #region IComparer<T> members

    /// <summary>
    /// Положение ненайденных элементов при сортировке с помощью метода Compare().
    /// По умолчанию - First - ненайденные элементы располагаются в начале списка.
    /// </summary>
    public UnknownItemPosition UnknownItemPosition
    {
      get { return _UnknownItemPosition; }
      set
      {
        switch (value)
        {
          case FreeLibSet.Collections.UnknownItemPosition.First:
          case FreeLibSet.Collections.UnknownItemPosition.Last:
            _UnknownItemPosition = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private UnknownItemPosition _UnknownItemPosition;

    /// <summary>
    /// Сравнение положения двух элементов.
    /// Метод может быть использован для сортировки произвольных списков и массивов, чтобы
    /// отсортировать их в соответствии с порядком элементов в текущем объекте ArrayIndexer.
    /// Сравнивается положение элементов в текущем объекте, а не значения элементов.
    /// Если какое-либо значение отсутствует в текущем объекте, то оно будет расположено в
    /// начале или в конце списка, в зависимости от свойства UnknownItemPosition.
    /// 
    /// Метод возвращает отрицательное значение, если <paramref name="x"/> располагается ближе
    /// к началу списка, чем <paramref name="y"/>. Положительное значение возвращается, если
    /// <paramref name="x"/> располагается ближе к концу списка, чем <paramref name="y"/>. 
    /// Если обоих значений нет в текущем списке, то возвращается результат сравнения значений,
    /// если тип <typeparamref name="T"/> реализует интерфейс IComparable. Если интерфейс
    /// сравнения не реализован, возвращается 0.
    /// /// </summary>
    /// <param name="x">Первое сравниваемое значение</param>
    /// <param name="y">Второе сравниваемое значение</param>
    /// <returns>Результат сравнение позиций</returns>
    public int Compare(T x, T y)
    {
      int px = IndexOf(x);
      int py = IndexOf(y);

      if (px < 0 && py < 0)
      {
        IComparable<T> intf1 = x as IComparable<T>;
        if (intf1 != null)
          return intf1.CompareTo(y);
        IComparable intf2 = x as IComparable;
        if (intf2 != null)
          return intf2.CompareTo(y);
      }

      if (UnknownItemPosition == FreeLibSet.Collections.UnknownItemPosition.Last)
      {
        if (px < 0)
          px = int.MaxValue;
        if (py < 0)
          py = int.MaxValue;
      }

      return px.CompareTo(py);
    }

    #endregion
  }

  /// <summary>
  /// Фиктивная реализация типизированного списка, не содержащего элементов.
  /// Класс предназначен только для реализации интефрейса IList, ICollection, IEnumerable.
  /// При попытке добавления элементов генерируется исключение
  /// </summary>
  /// <typeparam name="T">Произвольный тип элементов списка</typeparam>
  public class DummyList<T> : IList<T>, IReadOnlyObject
  {
    #region IList<T> Members

    /// <summary>
    /// Возвращает (-1)
    /// </summary>
    /// <param name="item">Игнорируется</param>
    /// <returns>Фиксированное значение</returns>
    public int IndexOf(T item)
    {
      return -1;
    }

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="index">Не используется</param>
    /// <param name="item">Не используется</param>
    public void Insert(int index, T item)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="index">Не используется</param>
    public void RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="index">Не используется</param>
    /// <returns>Не используется</returns>
    public T this[int index]
    {
      get { throw new NotSupportedException(); }
      set { throw new NotSupportedException(); }
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="item">Не используется</param>
    public void Add(T item)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Clear()
    {
    }

    /// <summary>
    /// Возвращает false
    /// </summary>
    /// <param name="item">Не используется</param>
    /// <returns>Фиксированное значение</returns>
    public bool Contains(T item)
    {
      return false;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="array">Не используется</param>
    /// <param name="arrayIndex">Не используется</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
    }

    /// <summary>
    /// Возвращает 0
    /// </summary>
    public int Count { get { return 0; } }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public bool IsReadOnly { get { return true; } }

    /// <summary>
    /// Возвращает false
    /// </summary>
    /// <param name="item">Не используется</param>
    /// <returns>Фиксированное значение</returns>
    public bool Remove(T item)
    {
      return false;
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает фиктивный перечислитель DummyEnumerator
    /// </summary>
    /// <returns>DummyEnumerator</returns>
    public DummyEnumerator<T> GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    public void CheckNotReadOnly()
    {
      throw new ObjectReadOnlyException();
    }

    #endregion
  }
}
