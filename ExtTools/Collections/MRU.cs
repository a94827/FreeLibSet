// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

// Списки с автоматическим удалением последних элементов
// (Most-Recently-Used)

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Коллекция объектов с автоматическим удалением элементов, обращение к которым выполнялось давно
  /// (используется двусвязный список объектов. При каждом обращении к элементу, он перемещается к голове списка)
  /// При обращению по ключу к элементу коллекции, если элемента с таким ключом не существует, то вызывается
  /// метод CreateItem() для создания элемента. Производный класс должен реализовать действия по созданию объекта. 
  /// Также он может переопределить метод, DestroyItem() (например, для сбоса данных в файл на диске)
  /// Коллекция реализует интерфейс IDisposable. При удалении объекта выполняется очиска списка с вызовом методов
  /// DestroyItem() для каждого элемента.
  ///                                                                        
  /// Следует рассмотреть возможность использования системы кэширования (класс Cache) вместо этой коллекции,
  /// если требуется хранить данные, которые занимают много памяти. Данная коллекция не реализует автоматической
  /// очистки элементов в случае нехватки памяти
  /// 
  /// Класс не является потокобезопасным, но является реентрабельным.
  /// 
  /// Используйте класс DictionaryWithMRU, если хранимые значения <typeparamref name="TValue"/> являются простыми.
  /// </summary>
  /// <typeparam name="TKey">Тип ключа для доступа к элементам. Обычно, String</typeparam>
  /// <typeparam name="TValue">Тип хранящихся значений</typeparam>
  public abstract class MRUObjectDictionary<TKey, TValue> : SimpleDisposableObject, IDictionary<TKey, TValue>
  {
    #region Статический конструктор

    static MRUObjectDictionary()
    {
      _IsValueDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TValue)); // 05.01.2020
    }

    /// <summary>
    /// Содержит True, если TValue реализует интерфейс IDisposable
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly bool _IsValueDisposable;

    #endregion

    #region Конструктор и Dispose

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    public MRUObjectDictionary()
    {
      _MaxCapacity = 10;
      _MRU = new LinkedList<KeyValuePair<TKey, TValue>>();
      _Dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
    }

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    public MRUObjectDictionary(IEqualityComparer<TKey> keyComparer)
    {
      _MaxCapacity = 10;
      _MRU = new LinkedList<KeyValuePair<TKey, TValue>>();
      _Dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(keyComparer);
    }

    /// <summary>
    /// Очищает коллекцию
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        Clear();
      base.Dispose(disposing);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Список ключей, отсортированный по последнему обращению. Ключ, к которому обращались последним,
    /// находится в начале списка
    /// </summary>
    private LinkedList<KeyValuePair<TKey, TValue>> _MRU;

    /// <summary>
    /// Словарь объектов по ключам для поиска
    /// В качестве значения используется LinkedListNode, чтобы быстрее перемещать запись в связанном списке
    /// </summary>
    private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _Dict;

    /// <summary>
    /// Получение объекта по ключу.
    /// Если объекта с таким ключом еще нет в списке, он создается с помощью CreateItem().
    /// Если число элементов превышает Capacity, то самый старый элемент удаляется с помощью DestroyItem.
    /// 
    /// При установке значения свойства вызов CreateItem не используется. Значение помещается в начало MRU-списка
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        LinkedListNode<KeyValuePair<TKey, TValue>> Node;
        if (_Dict.TryGetValue(key, out Node))
        {
          // Объект уже есть в списке. Перемещаем его в начало MRU
          if (!Object.ReferenceEquals(Node, _MRU.First))
          {
            _MRU.Remove(Node);
            _MRU.AddFirst(Node);
          }
          return Node.Value.Value;
        }

        // Проверяем, не пора ли удалять старые элементы
        LimitCount(MaxCapacity - 1);

        // Создаем новый элемент

        TValue NewItem = CreateItem(key); // Во время вызова виртуального метода могут быть вложенные вызовы

        if (!_Dict.ContainsKey(key)) // 07.04.2018 - Обеспечиваем реентрабельность
        {
          Node = _MRU.AddFirst(new KeyValuePair<TKey, TValue>(key, NewItem));
          _Dict.Add(key, Node);
        }

        return NewItem;
      }
      set // 04.10.2018
      {
        this.Remove(key);
        LinkedListNode<KeyValuePair<TKey, TValue>> Node;
        Node = _MRU.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
        _Dict.Add(key, Node);
      }
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Этот метод должен быть переопределен для создания элементов
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение для ключа</returns>
    protected abstract TValue CreateItem(TKey key);

    /// <summary>
    /// Этот метод вызывается при удалении элемента.
    /// Вызывает Value.Dispose(), если <typeparamref name="TValue"/> реализует интерефейс IDisposaable.
    /// Переопределенный метод может выполнить действия, связанные  с удалением элемента,
    /// а затем вызвать базовый метод.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    protected virtual void DestroyItem(TKey key, TValue value)
    {
      if (_IsValueDisposable)
      {
        IDisposable idisp = (IDisposable)value;
        if (idisp != null)
          idisp.Dispose();
      }
    }

    #endregion

    #region Емкость списка

    /// <summary>
    /// Емкость списка.
    /// Значение по умолчанию: 10.
    /// Установка свойства может привести к удалению части элементов списка.
    /// </summary>
    public int MaxCapacity
    {
      get { return _MaxCapacity; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _MaxCapacity = value;
        LimitCount(value);
      }
    }
    private int _MaxCapacity;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Очистка списка. Для каждого элемента вызывается DestroyItem()
    /// </summary>
    public void Clear()
    {
      LimitCount(0);
    }

    private void LimitCount(int value)
    {
      while (_Dict.Count > value)
      {
        // Удаляем последний элемент из списка
        DestroyItem(_MRU.Last.Value.Key, _MRU.Last.Value.Value);
        _Dict.Remove(_MRU.Last.Value.Key);
        _MRU.RemoveLast();
      }
    }

    #endregion

    #region IDictionary<TKey,TValue> Members

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
      throw new InvalidOperationException("Эта коллекция реализует автоматическое создание элементов при обращении. Добавление существующих элементов не предусмотрено");
    }

    /// <summary>
    /// Возвращает true, если коллекция содержит значение с таким ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Наличие значения</returns>
    public bool ContainsKey(TKey key)
    {
      return _Dict.ContainsKey(key);
    }

    /// <summary>
    /// Возвращает коллекцию ключей
    /// </summary>
    public ICollection<TKey> Keys
    {
      get { return _Dict.Keys; }
    }

    /// <summary>
    /// Удаляет элемент с заданным ключом.
    /// Вызывает DestroyItem() перед удалением.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если запись была в коллекйии</returns>
    public bool Remove(TKey key)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> Node;
      if (!_Dict.TryGetValue(key, out Node))
        return false;

      DestroyItem(key, Node.Value.Value);
      _MRU.Remove(Node);
      try
      {
        _Dict.Remove(key);
      }
      catch
      {
        _MRU.AddLast(Node);
        throw;
      }

      return true;
    }

    /// <summary>
    /// Пытается найти запись с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение, если оно найдено</param>
    /// <returns>true, если значение найдено</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> Node;
      if (!_Dict.TryGetValue(key, out Node))
      {
        value = default(TValue);
        return false;
      }

      value = Node.Value.Value;
      return true;
    }

    /// <summary>
    /// Возвращает коллекцию значений
    /// </summary>
    public ICollection<TValue> Values
    {
      get
      {
        // Придется создавать собстенную коллекцию
        // Использовать следует список значений коллекции, а не MRUList,
        // чтобы порядок совпадал с массивом Keys
        LinkedListNode<KeyValuePair<TKey, TValue>>[] Nodes = new LinkedListNode<KeyValuePair<TKey, TValue>>[_Dict.Count];
        _Dict.Values.CopyTo(Nodes, 0);

        TValue[] a = new TValue[Nodes.Length];
        for (int i = 0; i < Nodes.Length; i++)
          a[i] = Nodes[i].Value.Value;

        return a;
      }
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
      get
      {
        return this[key];
      }
      set
      {
        throw new InvalidOperationException("Эта коллекция не поддерживает замену элементов");
      }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      throw new InvalidOperationException("Коллекция не поддерживает добавление элементов");
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> Node;
      if (!_Dict.TryGetValue(item.Key, out Node))
        return false;

      return Object.Equals(item.Value, Node.Value.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      _MRU.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count
    {
      get { return _MRU.Count; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
      get { return false; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель по парам "Ключ-Значение"
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public LinkedList<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator()
    {
      return _MRU.GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return _MRU.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _MRU.GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Список с возможностью перебора как по списку, так и по последним "затронутым" элементам.
  /// Содержит метод Touch() который перемещает элемент в начало списка MRU (в основном списке
  /// элемент не перемещается). Простое обращение к объекту по индексу не перемещает объект в MRU.
  /// Может использоваться, например, для хранения списка окон с Z-order.
  /// "Старые" элементы не удаляются из списка автоматически, если свойство MaxCapacity не установлено в явном виде.
  /// Автоматическое удаление элементов работает медленно, так как используется вызов List.Remove() для удаления.
  /// Класс не является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Тип данных, хранимых в списке</typeparam>
  public class ListWithMRU<T> : IList<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public ListWithMRU()
    {
      _List = new List<T>();
      _LinkedList = new LinkedList<T>();
      _MaxCapacity = Int32.MaxValue;
    }

#if XXX // Этот конструктор только всех запутает

    /// <summary>
    /// Создает пустой список заданной начальной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость списка. Не путать с MaxCapacity</param>
    public ListWithMRU(int capacity)
    {
      FList = new List<T>(capacity);
      FLinkedList = new LinkedList<T>();
      FMaxCapacity = Int32.MaxValue;
    }
#endif

    /// <summary>
    /// Создает список и заполняет его элементами из коллекции
    /// </summary>
    /// <param name="collection">Исходная коллекция</param>
    public ListWithMRU(IEnumerable<T> collection)
    {
      _List = new List<T>(collection);
      _LinkedList = new LinkedList<T>(collection);
      _MaxCapacity = Int32.MaxValue;
    }

    #endregion

    #region Поля

    /// <summary>
    /// Основной список
    /// </summary>
    private List<T> _List;

    /// <summary>
    /// Список для ведения MRU.
    /// При добавлении элементов в список и при вызове Touch(),
    /// ссылка на элемент перемещается в начало связанного списка.
    /// </summary>
    private LinkedList<T> _LinkedList;

    #endregion

    #region Емкость списка

    /// <summary>
    /// Емкость списка.
    /// Значением по умолчанию - Int32.MaxValue, то есть емкость не ограничена
    /// и автоматическое удаление не выполняется.
    /// Установка свойства может привести к удалению части элементов списка.
    /// </summary>
    public int MaxCapacity
    {
      get { return _MaxCapacity; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _MaxCapacity = value;
        LimitCount();
      }
    }
    private int _MaxCapacity;

    private void LimitCount()
    {
      while (Count > MaxCapacity)
      {
        if (!Remove(MRU.Last))
          throw new BugException("Ошибка удаления последнего элемента списка");
      }
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// Возвращает индекс элемента в основном списке.
    /// Возвращает (-1), если элемента нет в списке
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>Индекс элемента</returns>
    public int IndexOf(T item)
    {
      return _List.IndexOf(item);
    }

    /// <summary>
    /// Вставляет элемент в указанную позицию основного списка.
    /// Элемент попадает в начало MRU-списка
    /// </summary>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="item">Элемент</param>
    public void Insert(int index, T item)
    {
      _List.Insert(index, item);
      _LinkedList.AddFirst(item);
      LimitCount();
    }

    /// <summary>
    /// Удаляет элемент в заданной позиции основного списка.
    /// Элемент также удаляется из списка MRU
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    public void RemoveAt(int index)
    {
      T OldItem = _List[index];
      _List.RemoveAt(index);
      _LinkedList.Remove(OldItem);
    }

    /// <summary>
    /// Получить или установить элемент в заданной позиции.
    /// Получение элемента не влияет на его положение в MRU-списке.
    /// Используйте метод Touch() для перемещения в начало списка.
    /// Установка элемента, напротив, помещает новый элемент в начало MRU-списка.
    /// </summary>
    /// <param name="index">Индекс элемента в основном списке</param>
    /// <returns>Элемент списка</returns>
    public T this[int index]
    {
      get
      {
        return _List[index];
      }
      set
      {
        T OldValue = _List[index];
        _List[index] = value;

        _LinkedList.Remove(OldValue);
        _LinkedList.AddFirst(value);
        // здесь LimitCount() не вызываем, т.к. количество элементов не поменялось
      }
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец основного списка и в начало MRU-списка
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      _List.Add(item);
      _LinkedList.AddFirst(item);
      LimitCount();
    }

    /// <summary>
    /// Очищает основной список и MRU-список
    /// </summary>
    public void Clear()
    {
      _List.Clear();
      _LinkedList.Clear();
    }

    /// <summary>
    /// Возвращает true, если элемент есть в списке
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(T item)
    {
      return _List.Contains(item);
    }

    /// <summary>
    /// Копирует все элементы из основного списка.
    /// Порядок элементов соответствует основному списку, а не MRU
    /// </summary>
    /// <param name="array">Массив для заполнения</param>
    public void CopyTo(T[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// Копирует все элементы из основного списка.
    /// Порядок элементов соответствует основному списку, а не MRU
    /// </summary>
    /// <param name="array">Массив для заполнения</param>
    /// <param name="arrayIndex">Начальная позиция</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирует все элементы из основного списка.
    /// Порядок элементов соответствует основному списку, а не MRU
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _List.Count; } }

    bool ICollection<T>.IsReadOnly { get { return false; } }

    /// <summary>
    /// Удаляет элемент из основного списка и из MRU-списка
    /// </summary>
    /// <param name="item">Удаляемый элемент</param>
    /// <returns>true, если элемент был удален</returns>
    public bool Remove(T item)
    {
      if (_List.Remove(item))
      {
        _LinkedList.Remove(item);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по объектам в списке.
    /// Порядок соответствует основному списку, а не MRU.
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

    #region Прочие методы основного списка

    /// <summary>
    /// Возвращает массив элементов в порядке, соответствующем основному списке
    /// </summary>
    /// <returns>Массив элементов</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// Возвращает "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region Методы для списка MRU

    /// <summary>
    /// Перемещает заданный элемент в начало списке MRU.
    /// Позиция элемента в основном списке не меняется.
    /// Если элемента нет в списке, никаких действий не выполняется
    /// </summary>
    /// <param name="item">Элемент, который переносится в начало списка</param>
    /// <returns>true, если элемент был найден в списке.
    /// False, если элемента нет в списке</returns>
    public bool Touch(T item)
    {
      if (_LinkedList.Remove(item))
      {
        _LinkedList.AddFirst(item);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Перемещает элемент в указанной позиции основного списка в начало списка MRU.
    /// Позиция элемента в основном списке не меняется.
    /// </summary>
    /// <param name="index">Индекс элемента в основном списке</param>
    public void TouchAt(int index)
    {
      _LinkedList.Remove(_List[index]);
      _LinkedList.AddFirst(_List[index]);
    }

    /// <summary>
    /// Реализация свойства MRU
    /// </summary>
    public struct MRUCollection : IEnumerable<T>
    {
      #region Конструктор

      internal MRUCollection(ListWithMRU<T> owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private ListWithMRU<T> _Owner;

      /// <summary>
      /// Возвращает первый объект в списке MRU.
      /// Если список пуст, возвращает default(T)
      /// </summary>
      public T First
      {
        get
        {
          LinkedListNode<T> node = _Owner._LinkedList.First;
          if (node == null)
            return default(T);
          else
            return node.Value;
        }
      }

      /// <summary>
      /// Возвращает последний объект в списке MRU.
      /// Если список пуст, возвращает default(T)
      /// </summary>
      public T Last
      {
        get
        {
          LinkedListNode<T> node = _Owner._LinkedList.Last;
          if (node == null)
            return default(T);
          else
            return node.Value;
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает список MRU в виде массива.
      /// Нулевым элементом идет элемент, к которому "прикасались" последним.
      /// В конце списка идут "менее нужные" элементы
      /// </summary>
      /// <returns>Массив элементов</returns>
      public T[] ToArray()
      {
        _Owner.CheckCount();

        T[] a = new T[_Owner._List.Count];
        int cnt = 0;
        foreach (T item in _Owner._LinkedList)
        {
          a[cnt] = item;
          cnt++;
        }
        return a;
      }

      #endregion

      #region IEnumerable

      /// <summary>
      /// Возвращает перечислитель по объектам в MRU-списке
      /// </summary>
      /// <returns>Перечислитель</returns>
      public LinkedList<T>.Enumerator GetEnumerator()
      {
        return _Owner._LinkedList.GetEnumerator();
      }

      IEnumerator<T> IEnumerable<T>.GetEnumerator()
      {
        return _Owner._LinkedList.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _Owner._LinkedList.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Возвращает псевдообъект для доступа к списку MRU
    /// </summary>
    public MRUCollection MRU { get { return new MRUCollection(this); } }


    private void CheckCount()
    {
      if (_List.Count != _LinkedList.Count)
        throw new BugException("Длина внутренних списков различается");
    }

    #endregion
  }

#if OLD
  /// <summary>
  /// Коллекция "Ключ-Значение" с возможностью перебора по последним "затронутым" элементам.
  /// Содержит метод Touch() который перемещает пару "Ключ-Значение" в начало списка MRU. 
  /// Простое обращение к значению по ключу не перемещает объект в MRU.
  /// "Старые" пары не удаляются из списка автоматически, если свойство MaxCapacity не установлено в явном виде.
  /// Класс не является потокобезопасным.
  /// </summary>
  public class DictionaryWithMRU<TKey, TValue> : IDictionary<TKey, TValue>
  {
  #region Конструкторы

    /// <summary>
    /// Создает пустой словарь
    /// </summary>
    public DictionaryWithMRU()
    {
      _Dict = new Dictionary<TKey, TValue>();
      _LinkedList = new LinkedList<TKey>();
      _MaxCapacity = Int32.MaxValue;
    }

    /// <summary>
    /// Создает словарь и заполняет его значениями из другого словаря.
    /// После вызова конструктора связь между словарями не сохраняется.
    /// </summary>
    /// <param name="source">Словарь, откуда нужно скопировать пары "Ключ-Значение"</param>
    public DictionaryWithMRU(IDictionary<TKey, TValue> source)
      : this()
    {
      foreach (KeyValuePair<TKey, TValue> Pair in source)
        Add(Pair.Key, Pair.Value);
    }

  #endregion

  #region Поля

    /// <summary>
    /// Основная коллекция.
    /// Используется для поиска значений по ключу
    /// </summary>
    private Dictionary<TKey, TValue> _Dict;

    /// <summary>
    /// Список для ведения MRU.
    /// При добавлении элементов в список и при вызове Touch(),
    /// ссылка на элемент перемещается в начало связанного списка.
    /// </summary>
    private LinkedList<TKey> _LinkedList;

  #endregion

  #region Емкость списка

    /// <summary>
    /// Емкость списка.
    /// Значением по умолчанию - Int32.MaxValue, то есть емкость не ограничена
    /// и автоматическое удаление не выполняется.
    /// Установка свойства может привести к удалению части элементов списка.
    /// </summary>
    public int MaxCapacity
    {
      get { return _MaxCapacity; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _MaxCapacity = value;
        LimitCount();
      }
    }
    private int _MaxCapacity;

    private void LimitCount()
    {
      while (Count > MaxCapacity)
      {
        if (!Remove(MRULastKey))
          throw new BugException("Ошибка удаления последнего элемента списка");
      }
    }

  #endregion

  #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добавляет пару "Код-Значение" в словарь.
    /// Ключ добавляется в начало MRU-списка
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      _Dict.Add(key, value);
      _LinkedList.AddFirst(key);
      LimitCount();

#if DEBUG
      CheckCount();
#endif
    }

    /// <summary>
    /// Возвращает true, если ключ есть в словаре
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Наличие элемента</returns>
    public bool ContainsKey(TKey key)
    {
      return _Dict.ContainsKey(key);
    }

    /// <summary>
    /// Возвращаемая коллекция ключей имеет порядок, отличный от порядка MRU
    /// </summary>
    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
      get { return _Dict.Keys; }
    }

    /// <summary>
    /// Удаляет запись с данным ключом из словаря
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если элемент был найден в словаре и удален</returns>
    public bool Remove(TKey key)
    {
      if (_Dict.Remove(key))
      {
        _LinkedList.Remove(key);
#if DEBUG
        CheckCount();
#endif
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Получение значения для заданного ключа.
    /// Доступ не вызывает перемещение элемента к началу MRU-списка. 
    /// Используйте метод Touch() в явном виде.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение</param>
    /// <returns>true, если ключ найден в словаре</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return _Dict.TryGetValue(key, out value);
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
      get { return _Dict.Values; }
    }

    /// <summary>
    /// Доступ к значению по ключу.
    /// При чтении свойства, если заданного ключа нет в словаре, генерируется исключение.
    /// Доступ не вызывает перемещение элемента к началу MRU-списка. 
    /// Используйте метод Touch() в явном виде.
    /// При записи выполняется замена значения или добавление записи в словарь.
    /// Ключ перемещается в начало MRU-списка.
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
#if DEBUG
        CheckCount();
#endif
        // Реализация изменена 20.10.2020

        bool IsNew = !_Dict.ContainsKey(key);
        if (IsNew)
          _Dict.Add(key, value);
        else
        {
          _Dict[key] = value;
          if (!_LinkedList.Remove(key))
            throw new BugException("Не найден ключ " + key.ToString() + " в LinkedList");
        }
        _LinkedList.AddFirst(key); 
        LimitCount();
#if DEBUG
        CheckCount();
#endif
      }
    }

  #endregion

  #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Очищает словарь
    /// </summary>
    public void Clear()
    {
      _Dict.Clear();
      _LinkedList.Clear();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      return ((ICollection<KeyValuePair<TKey, TValue>>)_Dict).Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      // Можно было бы и в правильном порядке сделать
      ((ICollection<KeyValuePair<TKey, TValue>>)_Dict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество записей в словаре
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get { return false; } }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

  #endregion

  #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Перечислитель, расширяющий LinkedList.GetEnumerator().
    /// Перечислимым элементом является пара "Код-Значение", а не один только код
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
  #region Конструктор

      internal Enumerator(DictionaryWithMRU<TKey, TValue> owner)
      {
#if DEBUG
        owner.CheckCount();
#endif
        _Owner = owner;
        _En = _Owner._LinkedList.GetEnumerator();
      }

  #endregion

  #region Поля

      private DictionaryWithMRU<TKey, TValue> _Owner;

      private LinkedList<TKey>.Enumerator _En;

  #endregion

  #region IEnumerator<KeyValuePair<TKey,TValue>> Members

      /// <summary>
      /// Текущая пара
      /// </summary>
      public KeyValuePair<TKey, TValue> Current
      {
        get
        {
          TKey key = _En.Current;
          TValue value = _Owner._Dict[key];
          return new KeyValuePair<TKey, TValue>(key, value);
        }
      }

      /// <summary>
      /// Закрывает связанный перечислитель LinkedList.Enumerator
      /// </summary>
      public void Dispose()
      {
        _En.Dispose();
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      /// <summary>
      /// Переход к следующему элементу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        return _En.MoveNext();
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        // Так как вложенный перечислитель - структура,
        // нельзя вызывать метод интерфейса Reset(), т.к. из-за boxing'а метод будет вызван фактически для другого экземпляра объекта
        _En = _Owner._LinkedList.GetEnumerator();
      }

  #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по парам "Код-Значение".
    /// Элементы словаря перебираются в порядке MRU-списка. 
    /// Сначала перечисляются, начиная с самых новых, или для которых был вызван Touch().
    /// В последнюю очередь возвращаются самые старые элементы.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

  #endregion

  #region Методы для списка MRU

    /// <summary>
    /// Перемещает элемент с заданным ключом в начало списке MRU.
    /// Позиция элемента в основном списке не меняется.
    /// Если элемента нет в словаре, никаких действий не выполняется
    /// </summary>
    /// <param name="key">Ключ элемента, который переносится в начало списка</param>
    /// <returns>true, если ключ был найден в списке.
    /// False, если элемента нет в списке</returns>
    public bool Touch(TKey key)
    {
      if (_LinkedList.Remove(key))
      {
        _LinkedList.AddFirst(key);
        return true;
      }
      else
        return false;
    }

    private void CheckCount()
    {
      if (_Dict.Count != _LinkedList.Count)
        throw new BugException("Длина внутренних списков различается");
    }

    /// <summary>
    /// Возвращает первый объект в списке MRU.
    /// Если список пуст, возвращает default(TKey)
    /// </summary>
    public TKey MRUFirstKey
    {
      get
      {
        LinkedListNode<TKey> node = _LinkedList.First;
        if (node == null)
          return default(TKey);
        else
          return node.Value;
      }
    }

    /// <summary>
    /// Возвращает последний объект в списке MRU.
    /// Если список пуст, возвращает default(T)
    /// </summary>
    public TKey MRULastKey
    {
      get
      {
        LinkedListNode<TKey> node = _LinkedList.Last;
        if (node == null)
          return default(TKey);
        else
          return node.Value;
      }
    }

  #endregion
  }

#else

  /// <summary>
  /// Коллекция "Ключ-Значение" с возможностью перебора по последним "затронутым" элементам.
  /// Содержит метод Touch() который перемещает пару "Ключ-Значение" в начало списка MRU. 
  /// Простое обращение к значению по ключу не перемещает объект в MRU.
  /// "Старые" пары не удаляются из списка автоматически, если свойство MaxCapacity не установлено в явном виде.
  /// Класс не является потокобезопасным.
  /// </summary>
  public class DictionaryWithMRU<TKey, TValue> : IDictionary<TKey, TValue>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой словарь
    /// </summary>
    public DictionaryWithMRU()
    {
      _Dict = new Dictionary<TKey, NodeAndValue>();
      _LinkedList = new LinkedList<TKey>();
      _MaxCapacity = Int32.MaxValue;
    }

    /// <summary>
    /// Создает пустой словарь
    /// </summary>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    public DictionaryWithMRU(IEqualityComparer<TKey> keyComparer)
    {
      _Dict = new Dictionary<TKey, NodeAndValue>(keyComparer);
      _LinkedList = new LinkedList<TKey>();
      _MaxCapacity = Int32.MaxValue;
    }

    /// <summary>
    /// Создает словарь и заполняет его значениями из другого словаря.
    /// После вызова конструктора связь между словарями не сохраняется.
    /// </summary>
    /// <param name="source">Словарь, откуда нужно скопировать пары "Ключ-Значение"</param>
    public DictionaryWithMRU(IDictionary<TKey, TValue> source)
      : this()
    {
      foreach (KeyValuePair<TKey, TValue> Pair in source)
        Add(Pair.Key, Pair.Value);

#if DEBUG
      DebugCheckCount();
#endif
    }

    /// <summary>
    /// Создает словарь и заполняет его значениями из другого словаря.
    /// После вызова конструктора связь между словарями не сохраняется.
    /// </summary>
    /// <param name="source">Словарь, откуда нужно скопировать пары "Ключ-Значение"</param>
    /// <param name="keyComparer">Интерфейс для сравнения ключей</param>
    public DictionaryWithMRU(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> keyComparer)
      : this(keyComparer)
    {
      foreach (KeyValuePair<TKey, TValue> Pair in source)
        Add(Pair.Key, Pair.Value);

#if DEBUG
      DebugCheckCount();
#endif
    }

    #endregion

    #region Поля

    /// <summary>
    /// Значения коллекции Dict.
    /// В коллекции хранится, кроме основного значения, еще и ссылка на LinkedListNode для списка LinkedList.
    /// Это позволяет использовать метод LinkedList.Remove(), принимающий не ключ, а узел. 
    /// У этих методов производительность разная: O(1) и O(Count) соответственно.
    /// </summary>
    private struct NodeAndValue
    {
      #region Конструктор

      public NodeAndValue(LinkedListNode<TKey> node, TValue value)
      {
        _Node = node;
        _Value = value;
      }

      #endregion

      #region Свойства

      public LinkedListNode<TKey> Node { get { return _Node; } }
      private LinkedListNode<TKey> _Node;

      public TValue Value { get { return _Value; } }
      private TValue _Value;

      #endregion
    }

    /// <summary>
    /// Основная коллекция.
    /// Используется для поиска значений по ключу
    /// </summary>
    private Dictionary<TKey, NodeAndValue> _Dict;

    /// <summary>
    /// Список для ведения MRU.
    /// При добавлении элементов в список и при вызове Touch(),
    /// ссылка на элемент перемещается в начало связанного списка.
    /// </summary>
    private LinkedList<TKey> _LinkedList;

    #endregion

    #region Емкость списка

    /// <summary>
    /// Емкость списка.
    /// Значением по умолчанию - Int32.MaxValue, то есть емкость не ограничена
    /// и автоматическое удаление не выполняется.
    /// Установка свойства может привести к удалению части элементов списка.
    /// </summary>
    public int MaxCapacity
    {
      get { return _MaxCapacity; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _MaxCapacity = value;
        LimitCount();
      }
    }
    private int _MaxCapacity;

    private void LimitCount()
    {
      while (Count > MaxCapacity)
      {
        if (!Remove(MRULastKey))
          throw new BugException("Ошибка удаления последнего элемента списка");
      }

#if DEBUG
      DebugCheckCount();
#endif
    }

    #endregion

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Добавляет пару "Код-Значение" в словарь.
    /// Ключ добавляется в начало MRU-списка
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(TKey key, TValue value)
    {
      LinkedListNode<TKey> node = _LinkedList.AddFirst(key);
      try
      {
        _Dict.Add(key, new NodeAndValue(node, value));

#if DEBUG
        DebugCheckCount();
#endif
      }
      catch
      {
        _LinkedList.Remove(node);

#if DEBUG
        DebugCheckCount();
#endif
        throw;
      }

      LimitCount();
    }

    /// <summary>
    /// Возвращает true, если ключ есть в словаре
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Наличие элемента</returns>
    public bool ContainsKey(TKey key)
    {
      return _Dict.ContainsKey(key);
    }

    /// <summary>
    /// Удаляет запись с данным ключом из словаря
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если элемент был найден в словаре и удален</returns>
    public bool Remove(TKey key)
    {

      NodeAndValue nv;
      if (_Dict.TryGetValue(key, out nv))
      {
        _Dict.Remove(key);
        _LinkedList.Remove(nv.Node);
#if DEBUG
        DebugCheckCount();
#endif
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Получение значения для заданного ключа.
    /// Доступ не вызывает перемещение элемента к началу MRU-списка. 
    /// Используйте метод Touch() в явном виде.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение</param>
    /// <returns>true, если ключ найден в словаре</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      NodeAndValue nv;
      bool res = _Dict.TryGetValue(key, out nv);
      value = nv.Value;
      return res;
    }

    /// <summary>
    /// Доступ к значению по ключу.
    /// При чтении свойства, если заданного ключа нет в словаре, генерируется исключение.
    /// Доступ не вызывает перемещение элемента к началу MRU-списка. 
    /// Используйте метод Touch() в явном виде.
    /// При записи выполняется замена значения или добавление записи в словарь.
    /// Ключ перемещается в начало MRU-списка.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[TKey key]
    {
      get
      {
        return _Dict[key].Value;
      }
      set
      {
#if DEBUG
        DebugCheckCount();
#endif

        NodeAndValue nv;
        if (_Dict.TryGetValue(key, out nv))
        {
          // Замена

          _LinkedList.Remove(nv.Node);
          _LinkedList.AddFirst(nv.Node);
          nv = new NodeAndValue(nv.Node, value);
          _Dict[key] = nv;
        }
        else
        {
          // Добавление

          LinkedListNode<TKey> node = _LinkedList.AddFirst(key);
          try
          {
            _Dict.Add(key, new NodeAndValue(node, value));
          }
          catch
          {
            _LinkedList.Remove(node);
            throw;
          }
        }

        LimitCount();
#if DEBUG
        DebugCheckCount();
#endif
      }
    }

    #endregion

    #region Коллекции Keys and Values

    /// <summary>
    /// Перечислитель ключей словаря.
    /// Дублирует структуру LinkedList.Enumerator.
    /// </summary>
    public struct KeyEnumerator : IEnumerator<TKey>
    {
      #region Конструктор

      internal KeyEnumerator(DictionaryWithMRU<TKey, TValue> owner)
      {
        _En = owner._LinkedList.GetEnumerator();
      }

      #endregion

      #region Поля

      private LinkedList<TKey>.Enumerator _En;

      #endregion

      #region IEnumerator<T> Members

      /// <summary>
      /// Возвращает текущий элемент перечислителя
      /// </summary>
      public TKey Current { get { return _En.Current; } }

      /// <summary>
      /// Очищает ресурсы перечислителя
      /// </summary>
      public void Dispose()
      {
        _En.Dispose();
      }

      object System.Collections.IEnumerator.Current { get { return _En.Current; } }

      /// <summary>
      /// Переходит к следующему 
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        return _En.MoveNext();
      }

      void System.Collections.IEnumerator.Reset()
      {
        ((System.Collections.IEnumerator)_En).Reset();
      }

      #endregion
    }

    /// <summary>
    /// Реализация свойства Keys.
    /// Коллекция ключей словаря. Коллекция доступна в режиме только для чтения.
    /// Порядок элементов коллекции при перечислении соответствует MRU-списку.
    /// </summary>
    public struct KeyCollection : ICollection<TKey>
    {
      #region Конструктор

      internal KeyCollection(DictionaryWithMRU<TKey, TValue> owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Поля

      private DictionaryWithMRU<TKey, TValue> _Owner;

      #endregion

      #region ICollection<TKey> Members

      void ICollection<TKey>.Add(TKey item)
      {
        throw new NotImplementedException();
      }

      void ICollection<TKey>.Clear()
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Возвращает true, если в словаре есть такой ключ
      /// </summary>
      /// <param name="item">Ключ</param>
      /// <returns></returns>
      public bool Contains(TKey item)
      {
        return _Owner.ContainsKey(item);
      }

      /// <summary>
      /// Копирует все ключи словаря в массив.
      /// Порядок элементов соответствует MRU--списку
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Первый индекс в массиве</param>
      public void CopyTo(TKey[] array, int arrayIndex)
      {
        int cnt = 0;
        foreach (TKey key in _Owner._LinkedList)
        {
          array[arrayIndex + cnt] = key;
          cnt++;
        }
      }

      /// <summary>
      /// Возвращает количество элементов в MRU-списке
      /// </summary>
      public int Count { get { return _Owner.Count; } }

      bool ICollection<TKey>.IsReadOnly { get { return true; } }

      bool ICollection<TKey>.Remove(TKey item)
      {
        throw new NotImplementedException();
      }

      #endregion

      #region IEnumerable<TKey> Members

      /// <summary>
      /// Создает перечислитель по ключам.
      /// Порядок перебора ключей соответствует MRU-списку
      /// </summary>
      /// <returns>Перечислитель</returns>
      public KeyEnumerator GetEnumerator()
      {
        return new KeyEnumerator(_Owner);
      }

      IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
      {
        return new KeyEnumerator(_Owner);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new KeyEnumerator(_Owner);
      }

      #endregion

      #region Дополнительные методы

      /// <summary>
      /// Возвращает массив всех ключей.
      /// Порядок элементов массива соответствует MRU-списку
      /// </summary>
      /// <returns>Массив ключей</returns>
      public TKey[] ToArray()
      {
        TKey[] a = new TKey[_Owner.Count];
        CopyTo(a, 0);
        return a;
      }

      #endregion
    }

    /// <summary>
    /// Перечислитель по значениям словаря.
    /// Порядок перечисления совпадает со списком MRU
    /// </summary>
    public struct ValueEnumerator : IEnumerator<TValue>
    {
      #region Конструктор

      internal ValueEnumerator(DictionaryWithMRU<TKey, TValue> owner)
      {
        _Owner = owner;
        _En = owner._LinkedList.GetEnumerator();
      }

      #endregion

      #region Поля

      private DictionaryWithMRU<TKey, TValue> _Owner;

      private LinkedList<TKey>.Enumerator _En;

      #endregion

      #region IEnumerator<TValue> Members

      /// <summary>
      /// Возвращает текущее значение перечислителя
      /// </summary>
      public TValue Current { get { return _Owner._Dict[_En.Current].Value; } }

      /// <summary>
      /// Удаляет перечислитель
      /// </summary>
      public void Dispose()
      {
        _En.Dispose();
      }

      #endregion

      #region IEnumerator Members

      object System.Collections.IEnumerator.Current { get { return _Owner._Dict[_En.Current]; } }

      /// <summary>
      /// Переходит к следующему элементу перечислителя
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        return _En.MoveNext();
      }

      void System.Collections.IEnumerator.Reset()
      {
        ((System.Collections.IEnumerator)_En).Reset();
      }

      #endregion
    }

    /// <summary>
    /// Реализация свойства Values.
    /// Коллекция значений словаря. Коллекция доступна в режиме только для чтения.
    /// Порядок элементов коллекции при перечислении соответствует MRU-списку.
    /// </summary>
    public struct ValueCollection : ICollection<TValue>
    {
      #region Конструктор

      internal ValueCollection(DictionaryWithMRU<TKey, TValue> owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Поля

      private DictionaryWithMRU<TKey, TValue> _Owner;

      #endregion

      #region ICollection<TValue> Members

      void ICollection<TValue>.Add(TValue item)
      {
        throw new NotImplementedException();
      }

      void ICollection<TValue>.Clear()
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Возвращает true, если значение есть в MRU-списке.
      /// Этот метод выполняется медленно, т.к. требует перебора всех элементов (O(n))
      /// </summary>
      /// <param name="item">Значение</param>
      /// <returns>Наличие в коллекции</returns>
      public bool Contains(TValue item)
      {
        foreach (KeyValuePair<TKey, NodeAndValue> pair in _Owner._Dict)
        {
          if (pair.Value.Value.Equals(item))
            return true;
        }
        return false;
      }

      /// <summary>
      /// Копирует значения в массив.
      /// Порядок значений соответствует MRU-списку
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Первый индекс в массиве</param>
      public void CopyTo(TValue[] array, int arrayIndex)
      {
        int cnt = 0;
        foreach (TKey key in _Owner._LinkedList)
        {
          array[cnt] = _Owner._Dict[key].Value;
          cnt++;
        }
      }

      /// <summary>
      /// Возвращает количество элементов в словаре
      /// </summary>
      public int Count { get { return _Owner.Count; } }

      bool ICollection<TValue>.IsReadOnly { get { return true; } }

      bool ICollection<TValue>.Remove(TValue item)
      {
        throw new NotImplementedException();
      }

      #endregion

      #region IEnumerable<TValue> Members

      /// <summary>
      /// Создает перечислитель по значениям словаря.
      /// Порядок перечисления соответствует MRU-списку
      /// </summary>
      /// <returns></returns>
      public ValueEnumerator GetEnumerator()
      {
        return new ValueEnumerator(_Owner);
      }

      IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
      {
        return new ValueEnumerator(_Owner);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new ValueEnumerator(_Owner);
      }

      #endregion

      #region Дополнительные методы

      /// <summary>
      /// Возвращает массив всех значений.
      /// Порядок элементов массива соответствует MRU-списку
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

    /// <summary>
    /// Коллекция ключей.
    /// Порядок перебора соответствует MRU-списку
    /// </summary>
    public KeyCollection Keys { get { return new KeyCollection(this); } }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return new KeyCollection(this); } }

    /// <summary>
    /// Коллекция значений.
    /// Порядок перебора соответствует MRU-списку
    /// </summary>
    public ValueCollection Values { get { return new ValueCollection(this); } }

    ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return new ValueCollection(this); } }


    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Очищает словарь
    /// </summary>
    public void Clear()
    {
      _Dict.Clear();
      _LinkedList.Clear();

#if DEBUG
      DebugCheckCount();
#endif
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      //return ((ICollection<KeyValuePair<TKey, TValue>>)_Dict).Contains(item);
      return ContainsKey(item.Key); // Сравнивать значение нечем
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      foreach (KeyValuePair<TKey, TValue> pair in this)
      {
        array[arrayIndex] = pair;
        arrayIndex++;
      }
    }

    /// <summary>
    /// Возвращает количество записей в словаре
    /// </summary>
    public int Count
    {
      get
      {
#if DEBUG
        DebugCheckCount();
#endif

        return _Dict.Count;
      }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get { return false; } }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Перечислитель, расширяющий LinkedList.GetEnumerator().
    /// Перечислимым элементом является пара "Код-Значение", а не один только код
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
      #region Конструктор

      internal Enumerator(DictionaryWithMRU<TKey, TValue> owner)
      {
#if DEBUG
        owner.DebugCheckCount();
#endif
        _Owner = owner;
        _En = _Owner._LinkedList.GetEnumerator();
      }

      #endregion

      #region Поля

      private DictionaryWithMRU<TKey, TValue> _Owner;

      private LinkedList<TKey>.Enumerator _En;

      #endregion

      #region IEnumerator<KeyValuePair<TKey,TValue>> Members

      /// <summary>
      /// Текущая пара
      /// </summary>
      public KeyValuePair<TKey, TValue> Current
      {
        get
        {
          TKey key = _En.Current;
          TValue value = _Owner._Dict[key].Value;
          return new KeyValuePair<TKey, TValue>(key, value);
        }
      }

      /// <summary>
      /// Закрывает связанный перечислитель LinkedList.Enumerator
      /// </summary>
      public void Dispose()
      {
        _En.Dispose();
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      /// <summary>
      /// Переход к следующему элементу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        return _En.MoveNext();
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        // Так как вложенный перечислитель - структура,
        // нельзя вызывать метод интерфейса Reset(), т.к. из-за boxing'а метод будет вызван фактически для другого экземпляра объекта
        _En = _Owner._LinkedList.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по парам "Код-Значение".
    /// Элементы словаря перебираются в порядке MRU-списка. 
    /// Сначала перечисляются, начиная с самых новых, или для которых был вызван Touch().
    /// В последнюю очередь возвращаются самые старые элементы.
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region Методы для списка MRU

    /// <summary>
    /// Перемещает элемент с заданным ключом в начало списке MRU.
    /// Позиция элемента в основном списке не меняется.
    /// Если элемента нет в словаре, никаких действий не выполняется
    /// </summary>
    /// <param name="key">Ключ элемента, который переносится в начало списка</param>
    /// <returns>true, если ключ был найден в списке.
    /// False, если элемента нет в списке</returns>
    public bool Touch(TKey key)
    {
      NodeAndValue nv;
      if (_Dict.TryGetValue(key, out nv))
      {
        _LinkedList.Remove(nv.Node);
        _LinkedList.AddFirst(nv.Node);

#if DEBUG
        DebugCheckCount();
#endif
        return true;
      }
      else
        return false;
    }

#if DEBUG
    private void DebugCheckCount()
    {
      if (_Dict.Count != _LinkedList.Count)
        throw new BugException("Длина внутренних списков различается");
    }
#endif

    /// <summary>
    /// Возвращает первый объект в списке MRU.
    /// Если список пуст, возвращает default(TKey)
    /// </summary>
    public TKey MRUFirstKey
    {
      get
      {
#if DEBUG
        DebugCheckCount();
#endif

        LinkedListNode<TKey> node = _LinkedList.First;
        if (node == null)
          return default(TKey);
        else
          return node.Value;
      }
    }

    /// <summary>
    /// Возвращает последний объект в списке MRU.
    /// Если список пуст, возвращает default(T)
    /// </summary>
    public TKey MRULastKey
    {
      get
      {
#if DEBUG
        DebugCheckCount();
#endif
        LinkedListNode<TKey> node = _LinkedList.Last;
        if (node == null)
          return default(TKey);
        else
          return node.Value;
      }
    }

    #endregion
  }
#endif
}
