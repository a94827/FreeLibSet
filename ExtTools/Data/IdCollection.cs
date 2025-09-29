// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Список идентификаторов (например, типа <see cref="Int32"/>) с однократным вхождением.
  /// Значение 0 не сохраняется.
  /// Отрицательные значения (в ExtDBDocs.dll трактуются как фиктивные идентификаторы)
  /// обрабатываются как обычные идентификаторы.
  /// Порядок элементов не сохраняется.
  /// Класс переопределяет оператор == и связанные с ним.
  /// </summary>
  /// <typeparam name="T">Тип поля идентификатора (<see cref="System.Int32"/>, <see cref="System.Int64"/>, <see cref="System.Guid"/>)</typeparam>
  [Serializable]
  public sealed class IdCollection<T> : IIdSet<T>, ICollection<T>, IEnumerable<T>, ICloneableReadOnlyObject<IdCollection<T>>, ISerializable
    where T : struct, IEquatable<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public IdCollection()
    {
      _Items = new Dictionary<T, object>();
    }

    /// <summary>
    /// Создает список и загружает в него идентификаторы из заданного перечислителя.
    /// Если коллекция <paramref name="source"/> содержит повторяющиеся или нулевые идентификаторы,
    /// они отбрасываются.
    /// </summary>
    /// <param name="source">Источник идентификаторов для заполнения списка. Не может</param>
    public IdCollection(IEnumerable<T> source)
    {
      ICollection<T> source2 = source as ICollection<T>;
      if (source2 == null)
        _Items = new Dictionary<T, object>();
      else
        _Items = new Dictionary<T, object>(source2.Count);

      if (source == null)
        return;

      IIdSet<T> source3 = source as IIdSet<T>;
      if (source3 == null)
      {
        // Обычное добавление с проверкой повторов и 0
        foreach (T id in source)
          Add(id);
      }
      else
      {
        // Оптимизированное добавление без проверок
        foreach (T id in source3)
          _Items.Add(id, null);
      }
    }

    ///// <summary>
    ///// Создает список и загружает в него идентификаторы из заданного нетипизированного перечислителя.
    ///// Если перечислитель возвращает элементы типа, отличного от <typeparamref name="T"/>, 
    ///// то для преобразования используется <see cref="StdConvert.ChangeType(object, Type)"/>.
    ///// </summary>
    ///// <param name="source">Источник идентификаторов для заполнения списка</param>
    //public IdCollection(IEnumerable source)
    //  :this()
    //{
    //  IEnumerable<T> source2 = source as IEnumerable<T>;
    //  if (source2 == null)
    //  {
    //    foreach (object id in source)
    //      Add((T)(StdConvert.ChangeType(id, typeof(T))));
    //  }
    //  else
    //  {
    //    foreach (T id in source2)
    //      Add(id);
    //  }
    //}

    ///// <summary>
    ///// Нет открытого конструктора с аргументом Capacity, чтобы не вводить в заблужение, что это одиночный Id
    ///// </summary>
    ///// <param name="dummy">Затычка</param>
    ///// <param name="capacity">Емкость</param>
    //private IdCollection(bool dummy, int capacity)
    //{
    //  _Items = new Dictionary<T, object>(capacity);
    //}

    #endregion

    #region ISerializable Members

    private IdCollection(SerializationInfo info, StreamingContext context)
    {
      T[] ids = (T[])(info.GetValue("Ids", typeof(Int32[])));
      _Items = new Dictionary<T, object>(ids.Length);
      for (int i = 0; i < ids.Length; i++)
        _Items.Add(ids[i], null);
      _IsReadOnly = info.GetBoolean("IsReadOnly");
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Ids", ToArray());
      info.AddValue("IsReadOnly", IsReadOnly);
    }

    #endregion

    #region Список идентификаторов

    /// <summary>
    /// Используем псевдоколлекцию, в которой в качестве значения используется null
    /// </summary>
    private Dictionary<T, object> _Items;

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Возвращает количество идентификаторов в списке
    /// </summary>
    public int Count
    {
      get { return _Items.Count; }
    }

    /// <summary>
    /// Возвращает true, если идентификатор есть в списке
    /// </summary>
    /// <param name="id">Проверяемый идентификатор</param>
    /// <returns>true, если идентификатор есть в списке</returns>
    public bool Contains(T id)
    {
      return _Items.ContainsKey(id);
    }

    /// <summary>
    /// Копирует идентификаторы в массив.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Первый индекс в массиве, с которого начать заполнение</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _Items.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает массив идентификаторов
    /// </summary>
    /// <returns>Массив - копия</returns>
    public T[] ToArray()
    {
      T[] ids = new T[_Items.Count];
      _Items.Keys.CopyTo(ids, 0);
      return ids;
    }

    /// <summary>
    /// Добавляет идентификатор в списке.
    /// Если задано значение 0 или такой идентификатор уже есть в списке, никаких действий не выполняется.
    /// </summary>
    /// <param name="id">Добавляемый идентификатор</param>
    public void Add(T id)
    {
      CheckNotReadOnly();

      if (id.Equals(default(T)))
        return;
      if (!_Items.ContainsKey(id))
        _Items.Add(id, null);
    }

    /// <summary>
    /// Добавляет все идентификаторы из перечислителя.
    /// Нулевые идентификаторы пропускаются.
    /// Если в текущем списке уже есть такие идентификаторы, то они пропускаются
    /// </summary>
    /// <param name="source">Добавляемый список</param>
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
        foreach (T id in source2)
          _Items.Add(id, source2);
      }
      else
      {
        foreach (T id in source)
          Add(id);
      }
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _Items.Clear();
    }

    /// <summary>
    /// Удаляет идентификатор из списка.
    /// Если идентификатора нет в списке, никаих действий не выполняется.
    /// </summary>
    /// <param name="id">Удаляемый идентификатор.</param>
    /// <returns>true, если идентификатор найден и удален</returns>
    public bool Remove(T id)
    {
      CheckNotReadOnly();

      return _Items.Remove(id);
    }

    /// <summary>
    /// Удаляет из текущего списка все идентификаторы, которые есть в списке <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Список удаляемых идентификаторов</param>
    public void RemoveRange(IEnumerable<T> source)
    {
      CheckNotReadOnly();

      if (source == null)
        return;

      if (_Items.Count == 0)
        return;

      foreach (T id in source)
        Remove(id);
    }

    /// <summary>
    /// Удаляет из текущего списка все идентификаторы, которых нет в списке <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Список идентификаторов, которые требуется удалить</param>
    public void RemoveOthers(IEnumerable<T> source)
    {
      CheckNotReadOnly();

      if (source == null)
      {
        Clear();
        return;
      }

      if (_Items.Count == 0)
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

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по всем идентификаторам в списке
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<T, object>.KeyCollection.Enumerator GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если список находися в режиме "только чтение".
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;


    /// <summary>
    /// Генерирует исключение, если список находися в режиме "только чтение".
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    /// <summary>
    /// Переводит список в режим "только чтение".
    /// Повторный вызов метода не выполняет никаких действий.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
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

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка.
    /// В новом списке свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <returns>Копия списка</returns>
    public IdCollection<T> Clone()
    {
      return new IdCollection<T>(this);
    }

    IIdSet<T> ICloneableReadOnlyObject<IIdSet<T>>.Clone()
    {
      return Clone();
    }

    //IIdSet ICloneableReadOnlyObject<IIdSet>.Clone()
    //{
    //  return Clone();
    //}

    object ICloneable.Clone()
    {
      return new IdCollection<T>(this);
    }

    /// <summary>
    /// Создает копию объекта, если <see cref="IsReadOnly"/>=true.
    /// Иначе возвращает ссылку на текущий объект
    /// </summary>
    /// <returns>Ссылка на копию или текущий объект</returns>
    public IdCollection<T> CloneIfReadOnly()
    {
      if (IsReadOnly)
        return Clone();
      else
        return this;
    }

    IIdSet<T> ICloneableReadOnlyObject<IIdSet<T>>.CloneIfReadOnly()
    {
      return CloneIfReadOnly();
    }

    //IIdSet ICloneableReadOnlyObject<IIdSet>.CloneIfReadOnly()
    //{
    //  return CloneIfReadOnly();
    //}

    #endregion

    #region Прочие методы и свойства

    IdSetKind IIdSet.Kind { get { return IdSetKind.Collection; } }

    /// <summary>
    /// Возвращает список идентификаторов, разделенных запятыми
    /// </summary>
    /// <returns>Список идентификаторов</returns>
    public override string ToString()
    {
      return StdConvert.ToString(ToArray());
    }

    /// <summary>
    /// Если список содержит единственный идентификатор, свойство возвращает его.
    /// Иначе возвращается 0.
    /// Свойство может быть полезно для оптимизации кода, когда список из одного идентификатора обрабатывается особым образом
    /// </summary>
    public T SingleId
    {
      get
      {
        if (_Items.Count == 1)
        {
          // Нет более короткого способа
          T[] a1 = new T[1];
          _Items.Keys.CopyTo(a1, 0);
          return a1[0];
        }
        else
          return default(T);
      }
    }

    Type IIdSet.IdType { get { return typeof(T); } }

    #endregion

    #region Статические методы создания из таблиц

    #region FromIds
#if XXX

    /// <summary>
    /// Создает список из поля, которое является первичным ключом таблицы или первого поля таблицы.
    /// </summary>
    /// <param name="table">Таблица с полем "Id"</param>
    /// <returns>Список идентификаторов</returns>
    public static IdCollection<T> FromIds(DataTable table)
    {
      if (table == null)
        return new IdCollection<T>(); // 20.08.2019

      return FromColumn(table, GetPKColumnName(table, "table"));
    }

    private static string GetPKColumnName(DataTable table, string paramName)
    {
      DataColumn col;
      switch (table.PrimaryKey.Length)
      { 
        case 0:
          if (table.Columns.Count==0)
            throw ExceptionFactory.ArgIsEmpty(paramName);
          col = table.Columns[0];
          break;
        case 1:
          col = table.PrimaryKey[0];
          break;
        default:
          throw ExceptionFactory.ArgDataTableMustHaveSingleColumnPrimaryKey(paramName, table);
      }

      if (col.DataType != typeof(T))
        throw ExceptionFactory.ArgDataTablePrimaryKeyWrongType(paramName, table, typeof(T));
      return col.ColumnName;
    }

    /// <summary>
    /// Создает список из поля, которое является первичным ключом таблицы или первого поля таблицы
    /// для строк, входящих в просмотр <see cref="DataView"/>
    /// </summary>
    /// <param name="dv">Просмотр <see cref="DataView"/></param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static IdCollection<T> FromIds(DataView dv)
    {
      if (dv == null)
        return new IdCollection<T>(); // 20.08.2019

      return FromColumn(dv, GetPKColumnName(dv.Table, "dv"));
    }
#endif

    /// <summary>
    /// Возвращает список, состоящий из одного идентификатора.
    /// Если передан нулевой идентификатор, то возвращается пустой список, который, однако, можно редактировать.
    /// </summary>
    /// <param name="id">Идентификатор, добавляемый в список</param>
    /// <returns>Список</returns>
    public static IdCollection<T> FromId(T id)
    {
      if (id.Equals(default(T)))
        return Empty;
      else
      {
        IdCollection<T> list = new IdCollection<T>();
        list.Add(id);
        return list;
      }
    }

    #endregion

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
    public static IdCollection<T> FromColumn(DataTable table, string columnName)
    {
      if (table == null)
        return new IdCollection<T>(); // 20.08.2019

      DataTableValues src = new DataTableValues(table);
      DataColumnValue cv = src[columnName];

      IdCollection<T> res = new IdCollection<T>(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

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
    /// которые принимает ссылочное поле в таблице для строк, относящихся к 
    /// объекту <see cref="DataView"/>.
    /// Нулевые значение отбрасывается и повторы отбрасываются.
    /// </summary>
    /// <param name="dv">Коллекция строк таблицы данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdCollection<T> FromColumn(DataView dv, string columnName)
    {
      if (dv == null)
        return new IdCollection<T>(); // 20.08.2019

      DataViewValues src = new DataViewValues(dv);
      DataColumnValue cv = src[columnName];

      IdCollection<T> res = new IdCollection<T>(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

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
    public static IdCollection<T> FromColumn(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return new IdCollection<T>(); // 20.08.2019

      DataRowArrayValues src = new DataRowArrayValues(rows, null);
      DataColumnValue cv = null;

      IdCollection<T> res = new IdCollection<T>();
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
    public static IdCollection<T> FromColumn(ICollection<DataRowView> rows, string columnName)
    {
      if (rows == null)
        return new IdCollection<T>(); // 20.08.2019

      DataRowArrayValues src = new DataRowArrayValues(rows, null);
      DataColumnValue cv = null;

      IdCollection<T> res = new IdCollection<T>(); // не стоит задавать емкость, т.к. реально может быть мало разных значений
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

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустой список идентификаторов с установленным значением <see cref="IsReadOnly"/>=true
    /// </summary>
    public static readonly IdCollection<T> Empty = CreateEmpty();

    private static IdCollection<T> CreateEmpty()
    {
      IdCollection<T> res = new IdCollection<T>();
      res.SetReadOnly();
      return res;
    }

    #endregion
  }
}
