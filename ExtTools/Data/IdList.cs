using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace FreeLibSet.Data
{

  /// <summary>
  /// Список идентификаторов, в котором порядок идентификаторов является значимым
  /// Значения null не допускаются.
  /// После установки свойства <see cref="IReadOnlyObject.IsReadOnly"/>=true, список становится потокобезопасным.
  /// </summary>
  [Serializable]
  public class IdList<T> : IIndexedIdSet<T>, IList<T>, System.Collections.IList, IReadOnlyObject
    where T : struct, IEquatable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public IdList()
    {
      _List = new List<T>();
      _Dict = new Dictionary<T, object>();
    }

    /// <summary>
    /// Конструктор списка, заполненного элементами из другого списка.
    /// Если в исходном списке есть повторяющиеся или нулевые элементы, то они отбрасываются.
    /// </summary>
    /// <param name="source">Исходный список. Не может быть null</param>
    public IdList(IEnumerable<T> source)
    {
      if (source == null)
      {
        _List = new List<T>();
        _Dict = new Dictionary<T, object>();
      }
      else if (source is IIdSet<T>)
      {
        IIdSet<T> source2 = (IIdSet<T>)source;
        _List = new List<T>(source2.Count);
        _Dict = new Dictionary<T, object>(source2.Count);
        foreach (T id in source2)
        {
          _Dict.Add(id, null);
          _List.Add(id);
        }
      }
      else
      {
        _List = new List<T>();
        _Dict = new Dictionary<T, object>();
        foreach (T id in source)
          Add(id);
      }
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной список
    /// </summary>
    private List<T> _List;

    /// <summary>
    /// Коллекция для проверки наличия элементов.
    /// Ключ - элемент, значение не используется.
    /// </summary>
    [NonSerialized]
    private Dictionary<T, object> _Dict;

    /// <summary>
    /// Доступ по индексу. 
    /// Индекс должен быть в диапазоне от 0 до (<see cref="Count"/>-1).
    /// Установка значения элемента выполняет замену элемента на новый. При этом может возникнуть
    /// исключение, если новый элемент отличается от старого и такой элемент уже есть в списке.
    /// </summary>                              
    /// <param name="index">Индекс элемента</param>
    /// <returns>Элемент</returns>
    public T this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();

        if (value.Equals(default(T)))
          throw ExceptionFactory.ArgIsEmpty("value");

        if (_Dict.ContainsKey(value))
        {
          if (IndexOf(value) != index)
            throw ExceptionFactory.KeyAlreadyExists(value);
        }

        T oldItem = _List[index];
        _Dict.Remove(oldItem);
        try
        {
          _Dict.Add(value, null);
        }
        catch
        {
          _Dict.Add(oldItem, null);
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

    Type IIdSet.IdType { get { return typeof(T); } }

    /// <summary>
    /// Возвращает идентификатор, если массив содержит единственный элемент.
    /// Иначе возвращается пустое значение.
    /// </summary>
    public T SingleId
    {
      get
      {
        if (Count == 1)
          return _List[0];
        else
          return default(T);
      }
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список переведен в режим "Только для чтения"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Перевод списка в режим "Только для чтения".
    /// Повторные вызовы игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Вызов исключения <see cref="ObjectReadOnlyException"/>, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
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
    /// Если требуется только проверить наличие элемента, следует использовать <see cref="Contains(T)"/>.
    /// </summary>
    /// <param name="id">Искомый элемент</param>
    /// <returns>Индекс</returns>
    public int IndexOf(T id)
    {
      // Сначала может иметь смысл убедиться в наличии элемента
      if (_List.Count > 20)
      {
        if (!_Dict.ContainsKey(id))
          return -1;
      }

      return _List.IndexOf(id);
    }

    /// <summary>
    /// Вставить элемент в середину списка.
    /// Если элемент уже есть в списке, никаких действий не выполняется и текущая позиция не изменяется.
    /// </summary>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="id">Элемент</param>
    public void Insert(int index, T id)
    {
      CheckNotReadOnly();

      if (id.Equals(default(T)))
        return;

      if (_Dict.ContainsKey(id))
        return;

      _Dict.Add(id, null);
      try
      {
        _List.Insert(index, id);
      }
      catch
      {
        _Dict.Remove(id);
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

      T id = _List[index];
      _List.RemoveAt(index);
      _Dict.Remove(id);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списке, если его еще нет в списке
    /// </summary>
    /// <param name="id">добавляемый элемент</param>
    public void Add(T id)
    {
      CheckNotReadOnly();

      if (id.Equals(default(T)))
        return;

      if (_Dict.ContainsKey(id))
        return;

      _Dict.Add(id, null);
      try
      {
        _List.Add(id);
      }
      catch
      {
        _Dict.Remove(id);
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
    /// Для определения позиции элемента в списке, используйте <see cref="IndexOf(T)"/>
    /// </summary>
    /// <param name="id">проверяемый элемент</param>
    /// <returns>true, если элемент есть в списке</returns>
    public bool Contains(T id)
    {
      return _Dict.ContainsKey(id);
    }

    /// <summary>
    /// Удаляет элемент, если он есть в списке.
    /// </summary>
    /// <param name="id">Удаляемый элемент</param>
    /// <returns>true, если элемент был в списке</returns>
    public bool Remove(T id)
    {
      CheckNotReadOnly();

      if (_Dict.ContainsKey(id))
      {
        _List.Remove(id);
        _Dict.Remove(id);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Методы, адресуемые внутреннему списку

    ///// <summary>
    ///// Копирование всех элементов списка в массив, начиная с начала массива
    ///// </summary>
    ///// <param name="array">Заполняемый массив элементов</param>
    //public void CopyTo(T[] array)
    //{
    //  _List.CopyTo(array);
    //}

    /// <summary>
    /// Копирование всех элементов списка в массив, начиная с указанного индекса
    /// </summary>
    /// <param name="array">Заполняемый массив элементов</param>
    /// <param name="arrayIndex">Индекс в массиве <paramref name="array"/>, куда записывается первый элемент списка</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    ///// <summary>
    ///// Копирование всех элементов списка в массив, начиная с указанного индекса
    ///// </summary>
    ///// <param name="index">Начальная позиция в текущем списке SingleScopeList, начиная с которой начинать копирование.</param>
    ///// <param name="array">Заполняемый массив элементов</param>
    ///// <param name="arrayIndex">Индекс в массиве <paramref name="array"/>, куда записывается первый элемент списка</param>
    ///// <param name="count">Количество копируемых элементов</param>
    //public void CopyTo(int index, T[] array, int arrayIndex, int count)
    //{
    //  _List.CopyTo(index, array, arrayIndex, count);
    //}

    #endregion

    #region Дополнительные методы

    IdSetKind IIdSet.Kind { get { return IdSetKind.List; } }

    /// <summary>
    /// Возвращает массив элементов в списке.
    /// Всегда создается новый массив.
    /// </summary>
    /// <returns>Массив</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// Групповое добавление элементов списка.
    /// В исходной коллекции могут быть одинаковые и нулевые элементы, которые пропускаются.
    /// </summary>
    /// <param name="source">Исходная коллекция</param>
    public void AddRange(IEnumerable<T> source)
    {
      CheckNotReadOnly();
      if (source == null)
        return;
      if (Object.ReferenceEquals(source, this))
        throw ExceptionFactory.ArgCollectionSameAsThis("collection");

      IIdSet<T> source2 = source as IIdSet<T>;
      if (Count == 0 && source2 != null)
      {
        // Оптимизированное добавление
        try
        {
          foreach (T id in source2)
          {
            _Dict.Add(id, null);
            _List.Add(id);
          }
        }
        catch
        {
          // Нельзя оставлять списки несогласованными
          _Dict.Clear();
          _List.Clear();
          throw;
        }
      }
      else
      {
        foreach (T id in source)
          Add(id);
      }
    }

    /// <summary>
    /// Удаление элементов, которые есть в <paramref name="source"/>
    /// </summary>
    /// <param name="source">Коллекция удаляемых идентификаторов. Может содержать пустые и повторяющиеся элементы.</param>
    public void RemoveRange(IEnumerable<T> source)
    {
      CheckNotReadOnly();
      if (source == null)
        return;

      foreach (T id in source)
      {
        if (_Dict.ContainsKey(id))
        {
          _List.Remove(id);
          _Dict.Remove(id);
        }
      }
    }

    /// <summary>
    /// Удаление всех элементов, которых нет в <paramref name="source"/>
    /// </summary>
    /// <param name="source">Коллекция удаляемых идентификаторов. Может содержать пустые и повторяющиеся элементы.</param>
    public void RemoveOthers(IEnumerable<T> source)
    {
      CheckNotReadOnly();

      if (source == null)
      {
        Clear();
        return;
      }

      if (_List.Count == 0)
        return;

      // Единственный простой способ - создать временный IdList
      IIdSet<T> source2 = source as IIdSet<T>;
      if (source2 == null || (source2 is IdArray<T>))
        source2 = new IdCollection<T>(source);
      if (source2.Count == 0)
      {
        Clear();
        return;
      }

      T[] a = ToArray();
      for (int i = 0; i < a.Length; i++)
      {
        if (!source2.Contains(a[i]))
          Remove(a[i]);
      }
    }

    #endregion

    #region Поиск совпадений

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="others"/> задает пустой список, то возвращается false.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IEnumerable<T> others)
    {
      T someMatchedId;
      return ContainsAny(others, out someMatchedId);
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

      foreach (T otherId in others)
      {
        if (Contains(otherId))
        {
          someMatchedId = otherId;
          return true;
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

      foreach (T otherId in others)
      {
        if (!Contains(otherId))
          return false;
      }

      return true;
    }

    #endregion

    #region Десериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<T, object>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
        _Dict.Add(_List[i], null);
    }

    #endregion

    #region IList Members

    int System.Collections.IList.Add(object value)
    {
      T value2 = (T)value;

      if (_Dict.ContainsKey(value2))
        return IndexOf(value2);

      Add(value2);
      return _List.Count - 1;
    }

    /// <summary>
    /// Возвращает true, если значение <paramref name="value"/> можно приводить к типу <typeparamref name="T"/>.
    /// Взято из List.cs в .Net Framework.
    /// </summary>
    /// <param name="value">Значение, переданное в метод нетипизированного интерфейса IList.</param>
    /// <returns>true - возможность преобразования</returns>
    private static bool IsCompatibleObject(object value)
    {
      // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
      // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
      return (value is T);
    }

    bool System.Collections.IList.Contains(object value)
    {
      if (IsCompatibleObject(value))
        return Contains((T)value);
      else
        return false;
    }

    int System.Collections.IList.IndexOf(object value)
    {
      if (IsCompatibleObject(value))
        return IndexOf((T)value);
      else
        return -1;
    }

    void System.Collections.IList.Insert(int index, object value)
    {
      this.Insert(index, (T)value);
    }

    bool System.Collections.IList.IsFixedSize
    {
      get { return IsReadOnly; }
    }

    void System.Collections.IList.Remove(object value)
    {
      if (IsCompatibleObject(value))
        Remove((T)value);
    }

    object System.Collections.IList.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        this[index] = (T)value;
      }
    }

    #endregion

    #region ICollection Members

    void System.Collections.ICollection.CopyTo(Array array, int index)
    {
      //((ICollection)_List).CopyTo(array, index);
      ArrayTools.CopyToArray(_List, array, index);
    }

    bool System.Collections.ICollection.IsSynchronized
    {
      get { return false; }
    }

    object System.Collections.ICollection.SyncRoot
    {
      get { return _List; }
    }

    #endregion

    #region ICloneable

    /// <summary>
    /// Создает копию списка.
    /// Новый список будет иметь свойство <see cref="IsReadOnly"/>=false.
    /// </summary>
    /// <returns>Копия списка</returns>
    public IdList<T> Clone()
    {
      return new IdList<T>((IIdSet<T>)this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию списка, если свойство <see cref="IsReadOnly"/>=true.
    /// Новый список будет иметь свойство <see cref="IsReadOnly"/>=false.
    /// Если <see cref="IsReadOnly"/>=false, возвращается ссылка на текущий список.
    /// </summary>
    /// <returns></returns>
    public IdList<T> CloneIfReadOnly()
    {
      if (IsReadOnly)
        return Clone();
      else
        return this;
    }

    IIdSet<T> ICloneableReadOnlyObject<IIdSet<T>>.Clone()
    {
      return Clone();
    }

    //IIdSet ICloneableReadOnlyObject<IIdSet>.Clone()
    //{
    //  return Clone();
    //}

    IIdSet<T> ICloneableReadOnlyObject<IIdSet<T>>.CloneIfReadOnly()
    {
      return CloneIfReadOnly();
    }

    //IIdSet ICloneableReadOnlyObject<IIdSet>.CloneIfReadOnly()
    //{
    //  return CloneIfReadOnly();
    //}

    #endregion

    #region Статические методы

    #region FromColumn
#if XXX
    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList<T> FromColumn(DataTable table, string columnName)
    {
      if (table == null)
        return new IdList<T>();

      DataTableValues src = new DataTableValues(table);
      DataColumnValue cv = src[columnName];

      IdList<T> res = new IdList<T>();

      while(src.Read())
      {
        if (cv.IsNull)
          continue;
        T Id = (T)(cv.Value);
        res.Add(Id);
      }

      return res;
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице для строк, относящихся к 
    /// объекту <see cref="DataView"/>.
    /// Нулевые значение отбрасывается и повторы отбрасываются.
    /// </summary>
    /// <param name="dv">Коллекция строк таблицы данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList<T> FromColumn(DataView dv, string columnName)
    {
      if (dv == null)
        return new IdList<T>();

      DataViewValues src = new DataViewValues(dv);
      DataColumnValue cv = src[columnName];

      IdList<T> res = new IdList<T>();

      while (src.Read())
      {
        if (cv.IsNull)
          continue;
        T Id = (T)(cv.Value);
        res.Add(Id);
      }

      return res;
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле для строк таблицы в массиве. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// Строки в массиве должны относиться либо к одной таблице, либо к таблицам,
    /// имеющим одинаковую структуру
    /// </summary>
    /// <param name="rows">Массив однотипных строк</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList<T> FromColumn(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return new IdList<T>();

      DataRowArrayValues src = new DataRowArrayValues(rows, null);
      DataColumnValue cv = null;

      IdList<T> res = new IdList<T>();
      while (src.Read())
      {
        if (cv == null)
          cv = src[columnName];

        if (cv.IsNull)
          continue;
        T Id = (T)(cv.Value);
        res.Add(Id);
      }

      return res;
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле для строк таблицы в массиве. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// Строки в массиве должны относиться либо к одной таблице, либо к таблицам,
    /// имеющим одинаковую структуру
    /// </summary>
    /// <param name="rows">Массив однотипных строк как коллекция <see cref="DataRowView"/></param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList<T> FromColumn(IEnumerable<DataRowView> rows, string columnName)
    {
      if (rows == null)
        return new IdList<T>();

      DataRowArrayValues src = new DataRowArrayValues(rows, null);
      DataColumnValue cv = null;

      IdList<T> res = new IdList<T>();
      while (src.Read())
      {
        if (cv == null)
          cv = src[columnName];

        if (cv.IsNull)
          continue;
        T Id = (T)(cv.Value);
        res.Add(Id);
      }

      return res;
    }
#endif

    #endregion

    #region FromId()

    /// <summary>
    /// Создает список из одного идентификатора.
    /// Если <paramref name="id"/> задает пустое значение, возвращается пустой массив <see cref="Empty"/>.
    /// Применение этого метода не очень удобно, так как возвращаемый список может иметь свойство <see cref="IsReadOnly"/>=true.
    /// Если предполагается дальнейшее изменение списка, удобнее создать пустой список и добавить в него идентификатор.
    /// Если изменение списка не предполагается, следует использовать <see cref="IdArray{T}.FromId(T)"/> как менее ресурсозатратный.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Список из одного элемента или пустой список</returns>
    public static IdList<T> FromId(T id)
    {
      if (id.Equals(default(T)))
        return Empty;
      else
      {
        IdList<T> lst = new IdList<T>();
        lst.Add(id);
        return lst;
      }
    }

    #endregion

    /// <summary>
    /// Список без идентификаторов и установленным признаком <see cref="IsReadOnly"/>=true.
    /// </summary>
    public static readonly IdList<T> Empty = CreateEmpty();

    private static IdList<T> CreateEmpty()
    {
      IdList<T> lst = new IdList<T>();
      lst.SetReadOnly();
      return lst;
    }

    #endregion
  }
}
