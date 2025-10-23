using FreeLibSet.Collections;
using FreeLibSet.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Набор списков идентификаторов для нескольких таблиц.
  /// Реализует словарь, в котором ключом является имя таблицы, а значением - список идентификаторов <see cref="IdCollection{T}"/>.
  /// Класс является потокобезопасным после вызова SetReadOnly(). В процессе заполнения класс не является потокобезопасным.
  /// Можно управлять чувствительностью к регистру для имен таблиц.
  /// Порядок идентификаторов и таблиц не сохраняется.
  /// </summary>
  /// <typeparam name="T">Тип поля идентификатора (<see cref="System.Int32"/>, <see cref="System.Int64"/>, <see cref="System.Guid"/>)</typeparam>
  [Serializable]
  public sealed class TableIdCollection<T> : IDictionary<string, IdCollection<T>>, 
    ITableIdSet<T>,
    ICloneableReadOnlyObject<TableIdCollection<T>>
    where T : struct, IEquatable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает набор
    /// </summary>
    /// <param name="ignoreCase">Должен ли игнорироваться регистр символов в именах таблиц</param>
    public TableIdCollection(bool ignoreCase)
    {
      _Tables = new TableDict(ignoreCase);
    }

    /// <summary>
    /// Создает набор.
    /// Имена таблиц будут чувствительны к регистру
    /// </summary>
    public TableIdCollection()
      : this(false)
    {
    }

    private TableIdCollection(int dummy)
      : this(false)
    {
      SetReadOnly();
    }

    #endregion

    #region Словарь таблиц

    [Serializable]
    private class TableDict : TypedStringDictionary<IdCollection<T>>
    {
      #region Конструктор

      public TableDict(bool ignoreCase)
        : base(ignoreCase)
      {
      }

      #endregion

      #region SetReadOnly

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    private readonly TableDict _Tables;

    /// <summary>
    /// Доступ к списку идентификаторов для таблицы с заданным именем.
    /// Если таблицы еще не было в списке, то, если свойство <see cref="IsReadOnly"/>=false (список в режиме заполнения), создается
    /// новый пустой объект <see cref="IdCollection{T}"/>, в который можно добавлять идентификаторы. Если <see cref="IsReadOnly"/>=true, 
    /// то возвращается ссылка на <see cref="IdCollection{T}.Empty"/>.
    /// Установка свойства очищает существующий список идентификаторов для таблицы и заменяет его новым
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Список идентификаторов</returns>
    public IdCollection<T> this[string tableName]
    {
      get
      {
        IdCollection<T> list;
        if (!_Tables.TryGetValue(tableName, out list))
        {
          if (String.IsNullOrEmpty(tableName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

          if (_Tables.IsReadOnly)
            return IdCollection<T>.Empty;
          else
          {
            list = new IdCollection<T>();
            _Tables.Add(tableName, list);
          }
        }
        return list;
      }
      set
      {
        CheckNotReadOnly();
        if (String.IsNullOrEmpty(tableName))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

        if (value.Count == 0)
        {
          if (!_Tables.ContainsKey(tableName))
            return;
        }

        IdCollection<T> lst = this[tableName];
        lst.Clear();
        lst.AddRange(value);
      }
    }

    /// <summary>
    /// Возвращает true, если есть идентификаторы для указанной таблицы.
    /// Эквивалентно проверке this[tableName].Count больше 0.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Наличие идентификаторов</returns>
    public bool Contains(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        return false;

      IdCollection<T> lst;
      if (!_Tables.TryGetValue(tableName, out lst))
        return false;

      return lst.Count > 0;
    }

    bool IDictionary<string, IdCollection<T>>.ContainsKey(string key)
    {
      return Contains(key);
    }

    /// <summary>
    /// Возвращает список идентификаторов для заданной таблицы, если он есть и содержит идентификаторы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="value">Сюда помещается список идентификаторов или null</param>
    /// <returns>Наличие идентификаторов. Эквивалентно Contains(<paramref name="tableName"/>).</returns>
    public bool TryGetValue(string tableName, out IdCollection<T> value)
    {
      if (!_Tables.TryGetValue(tableName, out value))
        return false;

      if (value.Count == 0)
      {
        value = null;
        return false;
      }

      return true;
    }

    #endregion

    #region Альтернативный доступ

    /// <summary>
    /// Возвращает наличие идентификатора для заданной таблицы в наборе.
    /// Установка свойства добавляет или удаляет идентификатор из набора, в зависимости от значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Наличие идентификатора для набора</returns>
    public bool this[string tableName, T id]
    {
      get
      {
        IdCollection<T> list;
        if (_Tables.TryGetValue(tableName, out list))
          return list.Contains(id);
        else
          return false;
      }
      set
      {
        if (value)
          this[tableName].Add(id);
        else
          this[tableName].Remove(id);
      }
    }

    #endregion

    #region Общая информация

    /// <summary>
    /// Возвращает общее количество идентификаторов для всех таблиц
    /// </summary>
    public int Count
    {
      get
      {
        int cnt = 0;
        foreach (KeyValuePair<string, IdCollection<T>> pair in _Tables)
          cnt += pair.Value.Count;
        return cnt;
      }
    }

    /// <summary>
    /// Возвращает true, если в списке нет ни одного идентификатора.
    /// Таблицы без идентификаторов не учитываются
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        foreach (KeyValuePair<string, IdCollection<T>> pair in _Tables)
        {
          if (pair.Value.Count > 0)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Пустой список без возможности изменения
    /// </summary>
    public static readonly TableIdCollection<T> Empty = new TableIdCollection<T>(0);

    /// <summary>
    /// Возвращает список имен таблиц, у которых есть идентификаторы
    /// </summary>
    /// <returns></returns>
    public string[] GetTableNames()
    {
      if (IsReadOnly)
      {
        // В режиме "только чтение" не может быть пустых списков IdList

        string[] a = new string[_Tables.Count];
        _Tables.Keys.CopyTo(a, 0);
        return a;
      }
      else
      {
        List<string> lst = new List<string>();
        foreach (KeyValuePair<string, IdCollection<T>> pair in _Tables)
        {
          if (pair.Value.Count > 0)
            lst.Add(pair.Key);
        }
        return lst.ToArray();
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если набор находится в режиме просмотра
    /// </summary>
    public bool IsReadOnly { get { return _Tables.IsReadOnly; } }

    /// <summary>
    /// Выбрасывает исключение, если набор находится в режиме просмотра
    /// </summary>
    public void CheckNotReadOnly()
    {
      _Tables.CheckNotReadOnly();
    }

    /// <summary>
    /// Переводит список в режим "Только чтение"
    /// </summary>
    public void SetReadOnly()
    {
      // Так как операция сложная, предотвращаем асинхронный вызов метода
      lock (_Tables)
      {
        if (!_Tables.IsReadOnly)
        {
          #region Вызываем IdList.SetReadOnly() и удаляем таблицы без идентификаторов

          if (_Tables.Count > 0)
          {
            string[] allNames = new string[_Tables.Count];
            _Tables.Keys.CopyTo(allNames, 0);
            for (int i = 0; i < allNames.Length; i++)
            {
              if (_Tables[allNames[i]].Count == 0)
                _Tables.Remove(allNames[i]);
              else
                _Tables[allNames[i]].SetReadOnly();
            }
          }

          #endregion

          _Tables.SetReadOnly(); // основной признак
        }
      }
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка, у которого признак <see cref="IsReadOnly"/> не установлен
    /// </summary>
    /// <returns></returns>
    public TableIdCollection<T> Clone()
    {
      TableIdCollection<T> res = new TableIdCollection<T>(_Tables.IgnoreCase);
      foreach (KeyValuePair<string, IdCollection<T>> pair in _Tables)
        res._Tables.Add(pair.Key, pair.Value.Clone());
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию объекта, если установлен признак "Только чтение".
    /// Иначе возвращает ссылку на текущий объект
    /// </summary>
    /// <returns>Ссылка на копию или текущий объект</returns>
    public TableIdCollection<T> CloneIfReadOnly()
    {
      if (IsReadOnly)
        return Clone();
      else
        return this;
    }

    #endregion

    #region Сложение и вычитание

    #region Методы модификации текущего списка

    /// <summary>
    /// Добавляет все идентификаторы из другого списка.
    /// Если в текущем списке уже есть такие идентификаторы, то они пропускаются
    /// </summary>
    /// <param name="source">Добавляемый список</param>
    public void AddRange(TableIdCollection<T> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (string tableName in source.GetTableNames())
        this[tableName].AddRange(source[tableName]);
    }


    /// <summary>
    /// Удаляет все идентификаторы для заданной таблицы.
    /// Эквивалентно this[<paramref name="tableName"/>].Clear().
    /// </summary>
    /// <param name="tableName">Имя очищаемой таблицы</param>
    /// <returns></returns>
    public bool Remove(string tableName)
    {
      CheckNotReadOnly();
      IdCollection<T> lst;
      if (!_Tables.TryGetValue(tableName, out lst))
        return false;
      lst.CheckNotReadOnly(); // на всякий случай
      _Tables.Remove(tableName);
      return lst.Count > 0; // а не true
    }

    /// <summary>
    /// Удаляет из текущего списка идентификаторы, которые есть в другом списке.
    /// Если в текущем списке нет некоторых идентификаторов, то они пропускаются.
    /// </summary>
    /// <param name="source">Вычитаемый список</param>
    public void RemoveRange(TableIdCollection<T> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (string tableName in source.GetTableNames())
      {
        IdCollection<T> list;
        if (_Tables.TryGetValue(tableName, out list))
          list.RemoveRange(source[tableName]);
      }
    }

    /// <summary>
    /// Очищает коллекцию
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _Tables.Clear();
    }

    #endregion

    #region Операторы

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// Тоже, что и оператор "+".
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Объединенный список</returns>
    public static TableIdCollection<T> operator |(TableIdCollection<T> a, TableIdCollection<T> b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableIdCollection<T> res = a.Clone();
      res.AddRange(b);
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// Тоже, что и оператор "+".
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Объединенный список</returns>
    public static TableIdCollection<T> operator +(TableIdCollection<T> a, TableIdCollection<T> b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableIdCollection<T> res = a.Clone();
      res.AddRange(b);
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из первого списка, которых нет во втором списке.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <param name="a">Первый список (большой)</param>
    /// <param name="b">Второй список (вычитаемый)</param>
    /// <returns>Разностный список</returns>
    public static TableIdCollection<T> operator -(TableIdCollection<T> a, TableIdCollection<T> b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableIdCollection<T> res = a.Clone();
      res.RemoveRange(b);
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы, входящие в оба списка.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Список с общими идентификаторами</returns>
    public static TableIdCollection<T> operator &(TableIdCollection<T> a, TableIdCollection<T> b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableIdCollection<T> res = new TableIdCollection<T>();
      foreach (string tableName in a.GetTableNames())
      {
        if (a[tableName].Count == 0)
          continue;
        IdCollection<T> resList = a[tableName].Clone();
        resList.RemoveOthers(b[tableName]);
        if (resList.Count > 0)
          res[tableName].AddRange(resList);
      }
      return res;
    }

    #endregion

    #endregion

    #region Сравнение

    /// <summary>
    /// Возвращает true, если два списка полностью совпадают
    /// </summary>
    /// <param name="a">Первый сравниваемый список</param>
    /// <param name="b">Второй сравниваемый список</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(TableIdCollection<T> a, TableIdCollection<T> b)
    {
      if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;

      string[] tableNamesA = a.GetTableNames();
      string[] tableNamesB = b.GetTableNames();
      if (tableNamesA.Length != tableNamesB.Length)
        return false; // Проверка нужна, чтобы не оказалось в списке b таблиц, которых нет в списке a.

      foreach (string tableName in tableNamesA)
      {
        IdCollection<T> listA = a[tableName];
        IdCollection<T> listB = b[tableName];
        if (listA != listB)
          return false;
      }

      return true;
    }

    /// <summary>
    /// Возвращает true, если два списка отличаются
    /// </summary>
    /// <param name="a">Первый сравниваемый список</param>
    /// <param name="b">Второй сравниваемый список</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(TableIdCollection<T> a, TableIdCollection<T> b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Возвращает true, если текущий список полностью совпадает с <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Второй сравниваемый список</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      TableIdCollection<T> b = obj as TableIdCollection<T>;
      return (this == b);
    }

    /// <summary>
    /// Хэш-код для коллекций.
    /// Так как <see cref="TableIdCollection{T}"/> не предназначен для использования в качестве ключа коллекции,
    /// метод возвращает 0.
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return 0;
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString() + (IsReadOnly ? " (ReadOnly)" : String.Empty);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,IdList>> Members

    /// <summary>
    /// Перечислитель по именам таблиц и их идентификаторам
    /// </summary>
    public struct Enumerator : 
      IEnumerator<KeyValuePair<string, IdCollection<T>>>,
      IEnumerator<KeyValuePair<string, IIdSet<T>>>
    {
      #region Защищенный конструктор

      internal Enumerator(TableIdCollection<T> owner)
      {
        _Owner = owner;
        _BaseEnumerator = owner._Tables.GetEnumerator();
      }

      #endregion

      #region Поля

      private readonly TableIdCollection<T> _Owner;

      private IEnumerator<KeyValuePair<string, IdCollection<T>>> _BaseEnumerator;

      #endregion

      #region IEnumerator<KeyValuePair<string,IdList>> Members

      /// <summary>
      /// Возвращает текущую пару
      /// </summary>
      public KeyValuePair<string, IdCollection<T>> Current { get { return _BaseEnumerator.Current; } }

      KeyValuePair<string, IIdSet<T>> IEnumerator<KeyValuePair<string, IIdSet<T>>>.Current
      {
        get
        {
          return new KeyValuePair<string, IIdSet<T>>(Current.Key, Current.Value);
        }
      }

      /// <summary>
      /// Завершает перечисление
      /// </summary>
      public void Dispose()
      {
        _BaseEnumerator.Dispose();
      }

      object IEnumerator.Current { get { return _BaseEnumerator.Current; } }

      /// <summary>
      /// Переходит к следующей таблице, у которой есть идентификаторы
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        do
        {
          if (!_BaseEnumerator.MoveNext())
            return false;
        } while (_BaseEnumerator.Current.Value.Count == 0);

        return true;
      }

      void IEnumerator.Reset()
      {
        _BaseEnumerator.Dispose();
        _BaseEnumerator = _Owner._Tables.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель по парам "Имя таблицы - список идентификаторов".
    /// Порядок перечисления таблиц не гарантируется.
    /// Перечисляются только таблицы, по которым есть идентификаторы.
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<KeyValuePair<string, IdCollection<T>>> IEnumerable<KeyValuePair<string, IdCollection<T>>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region IDictionary<string,IdList> Members

    void IDictionary<string, IdCollection<T>>.Add(string key, IdCollection<T> value)
    {
      this[key].AddRange(value);
    }

    ICollection<string> IDictionary<string, IdCollection<T>>.Keys
    {
      get { throw new NotImplementedException(); }
    }

    ICollection<IdCollection<T>> IDictionary<string, IdCollection<T>>.Values
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ICollection<KeyValuePair<string,IdList>> Members

    int ICollection<KeyValuePair<string, IdCollection<T>>>.Count
    {
      get
      {
        int cnt = 0;
        foreach (KeyValuePair<string, IdCollection<T>> pair in this)
          cnt++;
        return cnt;
      }
    }

    void ICollection<KeyValuePair<string, IdCollection<T>>>.Add(KeyValuePair<string, IdCollection<T>> item)
    {
      this[item.Key].AddRange(item.Value);
    }

    bool ICollection<KeyValuePair<string, IdCollection<T>>>.Contains(KeyValuePair<string, IdCollection<T>> item)
    {
      return this[item.Key].ContainsAll(item.Value);
    }

    void ICollection<KeyValuePair<string, IdCollection<T>>>.CopyTo(KeyValuePair<string, IdCollection<T>>[] array, int arrayIndex)
    {
      List<KeyValuePair<string, IdCollection<T>>> lst = new List<KeyValuePair<string, IdCollection<T>>>();
      foreach (KeyValuePair<string, IdCollection<T>> pair in this)
        lst.Add(pair);
      lst.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, IdCollection<T>>>.Remove(KeyValuePair<string, IdCollection<T>> item)
    {
      IdCollection<T> lst;
      if (!TryGetValue(item.Key, out lst))
        return false;

      bool res = false;
      foreach (T id in item.Value)
      {
        if (lst.Remove(id))
          res = true;
      }
      return res;
    }

    #endregion

    Type ITableIdSet.IdType
    {
      get { return typeof(T); }
    }

    IdSetKind ITableIdSet.Kind
    {
      get { return IdSetKind.Collection; }
    }

    /// <summary>
    /// Возвращает true, если имена таблиц в коллекции не учитываются
    /// </summary>
    public bool IgnoreCase { get { return _Tables.IgnoreCase; } }

    void IDictionary<string, IIdSet<T>>.Add(string tableName, IIdSet<T> ids)
    {
      this[tableName].AddRange(ids);
    }

    bool IDictionary<string, IIdSet<T>>.ContainsKey(string tableName)
    {
      return Contains(tableName);
    }

    ICollection<string> IDictionary<string, IIdSet<T>>.Keys
    {
      get { return _Tables.Keys; }
    }

    bool IDictionary<string, IIdSet<T>>.TryGetValue(string tableName, out IIdSet<T> ids)
    {
      IdCollection<T> ids2;
      bool res = TryGetValue(tableName, out ids2);
      ids = ids2;
      return res;
    }

    ICollection<IIdSet<T>> IDictionary<string, IIdSet<T>>.Values
    {
      get { return (ICollection<IIdSet<T>>)(_Tables.Values); }
    }

    IIdSet<T> IDictionary<string, IIdSet<T>>.this[string tableName]
    {
      get
      {
        return this[tableName];
      }
      set
      {
        this[tableName].Clear();
        this[tableName].AddRange(value);
      }
    }

    void ICollection<KeyValuePair<string, IIdSet<T>>>.Add(KeyValuePair<string, IIdSet<T>> item)
    {
      this[item.Key].AddRange(item.Value);
    }

    bool ICollection<KeyValuePair<string, IIdSet<T>>>.Contains(KeyValuePair<string, IIdSet<T>> item)
    {
      throw new NotImplementedException();
    }

    void ICollection<KeyValuePair<string, IIdSet<T>>>.CopyTo(KeyValuePair<string, IIdSet<T>>[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    int ICollection<KeyValuePair<string, IIdSet<T>>>.Count
    {
      get { throw new NotImplementedException(); }
    }

    bool ICollection<KeyValuePair<string, IIdSet<T>>>.Remove(KeyValuePair<string, IIdSet<T>> item)
    {
      if (_Tables.ContainsKey(item.Key))
      {
        int cnt = this[item.Key].Count;
        this[item.Key].RemoveRange(item.Value);
        return cnt > this[item.Key].Count;
      }
      else
        return false;
    }

    IEnumerator<KeyValuePair<string, IIdSet<T>>> IEnumerable<KeyValuePair<string, IIdSet<T>>>.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
