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
  /// Список идентификаторов <see cref="Int32"/> с однократным вхождением.
  /// Значение 0 не сохраняется.
  /// Отрицательные значения (в ExtDBDocs.dll трактуются как фиктивные идентификаторы)
  /// обрабатываются как обычные идентификаторы.
  /// Порядок элементов не сохраняется.
  /// Класс переопределяет оператор == и связанные с ним
  /// </summary>
  [Serializable]
  public sealed class IdList : ICollection<Int32>, IEnumerable<Int32>, IReadOnlyObject, ICloneable, ISerializable
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public IdList()
    {
      _Items = new Dictionary<Int32, object>();
    }

    /// <summary>
    /// Создает список и загружает в него идентификаторы из заданной коллекции.
    /// Если коллекция <paramref name="source"/> содержит повторяющиеся или нулевые идентификаторы,
    /// они отбрасываются
    /// </summary>
    /// <param name="source">Источник идентификаторов для заполнения списка</param>
    public IdList(ICollection<Int32> source)
    {
      _Items = new Dictionary<Int32, object>(source.Count);
      foreach (Int32 id in source)
        Add(id);
    }

    /// <summary>
    /// Создает список и загружает в него идентификаторы из заданного перечислителя.
    /// Если коллекция <paramref name="source"/> содержит повторяющиеся или нулевые идентификаторы,
    /// они отбрасываются
    /// </summary>
    /// <param name="source">Источник идентификаторов для заполнения списка</param>
    public IdList(IEnumerable<Int32> source)
      : this()
    {
      foreach (Int32 id in source)
        Add(id);
    }


    /// <summary>
    /// Нет открытого конструктора с аргументом Capacity, чтобы не вводить в заблужение, что это одиночный Id
    /// </summary>
    /// <param name="dummy">Затычка</param>
    /// <param name="capacity">Емкость</param>
    private IdList(bool dummy, int capacity)
    {
      _Items = new Dictionary<Int32, object>(capacity);
    }

    #endregion

    #region ISerializable Members

    private IdList(SerializationInfo info, StreamingContext context)
    {
      Int32[] ids = (Int32[])(info.GetValue("Ids", typeof(Int32[])));
      _Items = new Dictionary<Int32, object>(ids.Length);
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
    private Dictionary<Int32, object> _Items;

    #endregion

    #region ICollection<int> Members

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
    public bool Contains(Int32 id)
    {
      return _Items.ContainsKey(id);
    }

    /// <summary>
    /// Копирует идентификаторы в массив.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Первый индекс в массиве, с которого начать заполнение</param>
    public void CopyTo(Int32[] array, int arrayIndex)
    {
      _Items.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает массив идентификаторов
    /// </summary>
    /// <returns>Массив - копия</returns>
    public Int32[] ToArray()
    {
      Int32[] ids = new Int32[_Items.Count];
      _Items.Keys.CopyTo(ids, 0);
      return ids;
    }

    /// <summary>
    /// Добавляет идентификатор в списке.
    /// Если задано значение 0 или такой идентификатор уже есть в списке, никаких действий не выполняется.
    /// </summary>
    /// <param name="id">Добавляемый идентификатор</param>
    public void Add(Int32 id)
    {
      CheckNotReadOnly();

      if (id == 0)
        return;
      if (!_Items.ContainsKey(id))
        _Items.Add(id, null);
    }

    /// <summary>
    /// Добавляет все идентификаторы из другого списка.
    /// Если в текущем списке уже есть такие идентификаторы, то они пропускаются
    /// </summary>
    /// <param name="source">Добавляемый список</param>
    public void Add(IdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 id in source)
        Add(id);
    }

    /// <summary>
    /// Добавляет все идентификаторы из перечислителя.
    /// Нулевые идентификаторы пропускаются.
    /// Если в текущем списке уже есть такие идентификаторы, то они пропускаются
    /// </summary>
    /// <param name="source">Добавляемый список</param>
    public void Add(IEnumerable<Int32> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 id in source)
        Add(id);
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
    public bool Remove(Int32 id)
    {
      CheckNotReadOnly();

      return _Items.Remove(id);
    }

    /// <summary>
    /// Удаляет из текущего списка все идентификаторы, которые есть в списке <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Список удаляемых идентификаторов</param>
    public void Remove(IdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 id in source)
        Remove(id);
    }

    /// <summary>
    /// Удаляет из текущего списка все идентификаторы, которые есть в списке <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Список удаляемых идентификаторов</param>
    public void Remove(IEnumerable<Int32> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 id in source)
        Remove(id);
    }

    /// <summary>
    /// Удаляет из текущего списка все идентификаторы, которых нет в списке <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Список идентификаторов, которые требуется удалить</param>
    public void RemoveOthers(IdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      Int32[] a = ToArray();
      for (int i = 0; i < a.Length; i++)
      {
        if (!source.Contains(a[i]))
          Remove(a[i]);
      }
    }

    /// <summary>
    /// Удаляет из текущего списка все идентификаторы, которых нет в списке <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Список идентификаторов, которые требуется удалить</param>
    public void RemoveOthers(IEnumerable<Int32> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      // Единственный простой способ - создать временный IdList
      IdList source2 = new IdList();
      foreach (Int32 id in source)
        source2.Add(id);

      RemoveOthers(source2);
    }

    #endregion

    #region IEnumerable<int> Members

    /// <summary>
    /// Возвращает перечислитель по всем идентификаторам в списке
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<Int32, object>.KeyCollection.Enumerator GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    IEnumerator<Int32> IEnumerable<Int32>.GetEnumerator()
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
    /// Если <paramref name="other"/> задает пустой список, то возвращается false.
    /// </summary>
    /// <param name="other">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IdList other)
    {
#if DEBUG
      if (other == null)
        throw new ArgumentNullException("other");
#endif

      if (this.Count == 0 || other.Count == 0)
        return false;

      foreach (Int32 otherId in other)
      {
        if (Contains(otherId))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="other"/> задает пустой список, то возвращается false.
    /// </summary>
    /// <param name="other">Список проверяемых идентификаторов</param>
    /// <param name="firstMatchedId">Сюда помещается первый совпадающий идентификатор</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(IdList other, out Int32 firstMatchedId)
    {
#if DEBUG
      if (other == null)
        throw new ArgumentNullException("other");
#endif

      firstMatchedId = 0;
      if (this.Count == 0 || other.Count == 0)
        return false;

      foreach (Int32 otherId in other)
      {
        if (Contains(otherId))
        {
          firstMatchedId = otherId;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// </summary>
    /// <param name="ids">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(Int32[] ids)
    {
      Int32 firstMatchedId;
      return ContainsAny(ids, out firstMatchedId);
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Значения 0 в списке <paramref name="ids"/> пропускаются.
    /// </summary>
    /// <param name="ids">Список проверяемых идентификаторов</param>
    /// <param name="firstMatchedId">Сюда помещается первый совпадающий идентификатор</param>
    /// <returns>true при наличии пересечения</returns>
    public bool ContainsAny(Int32[] ids, out Int32 firstMatchedId)
    {
#if DEBUG
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      firstMatchedId = 0;

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        if (Contains(ids[i]))
        {
          firstMatchedId = ids[i];
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке есть ВСЕ идентификаторы из второго списка.
    /// Если <paramref name="other"/> задает пустой список, то возвращается true.
    /// </summary>
    /// <param name="other">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии всех идентификаторов</returns>
    public bool Contains(IdList other)
    {
#if DEBUG
      if (other == null)
        throw new ArgumentNullException("other");
#endif

      if (other.Count == 0)
        return true;

      if (this.Count == 0)
        return false;

      foreach (Int32 otherId in other)
      {
        if (!Contains(otherId))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Возвращает true, если в текущем списке есть ВСЕ идентификаторы из второго списка.
    /// Если <paramref name="ids"/> задает пустой список, то возвращается true.
    /// Значения 0 в списке <paramref name="ids"/> пропускаются.
    /// </summary>
    /// <param name="ids">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии всех идентификаторов</returns>
    public bool Contains(Int32[] ids)
    {
#if DEBUG
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;

        if (!Contains(ids[i]))
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
    public IdList Clone()
    {
      return new IdList(this);
    }

    object ICloneable.Clone()
    {
      return new IdList(this);
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Возвращает true, если оба списка содержат одинаковые идентификаторы (без учета порядка следования).
    /// Также возвращает true, если обе ссылки равны null
    /// </summary>
    /// <param name="a">Первый список для сравнения</param>
    /// <param name="b">Второй список для сравнения</param>
    /// <returns>Равенство списков</returns>
    public static bool operator ==(IdList a, IdList b)
    {
      if (Object.ReferenceEquals(a, b))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;

      if (a.Count != b.Count)
        return false;

      return a.Contains(b);
    }

    /// <summary>
    /// Возвращает true, если в списках хотя бы один идентификатор не совападает (без учета порядка следования).
    /// Также возвращает true, если одна из ссылок равна null, а вторая - нет
    /// </summary>
    /// <param name="a">Первый список для сравнения</param>
    /// <param name="b">Второй список для сравнения</param>
    /// <returns>Равенство списков</returns>
    public static bool operator !=(IdList a, IdList b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Возвращает true, если <paramref name="obj"/> ссылается на <see cref="IdList"/> и списки совпадают.
    /// </summary>
    /// <param name="obj">Сравниваемый список</param>
    /// <returns>true, если списки совпадают</returns>
    public override bool Equals(object obj)
    {
      if (obj is IdList)
        return this == (IdList)obj;
      else
        return false;
    }

    /// <summary>
    /// Хэш-код списка. Возвращает свойство <see cref="Count"/>.
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return Count;
    }

    #endregion

    #region Операторы сложения / вычитания

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// Тоже, что и оператор "+"
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Объединенный список</returns>
    public static IdList operator |(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList res = a.Clone();
      foreach (Int32 id in b)
        res.Add(id);
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы, входящие в оба списка.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Список с общими идентификаторами</returns>
    public static IdList operator &(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList res = new IdList();
      foreach (Int32 id in a)
      {
        if (b.Contains(id))
          res.Add(id);
      }
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// Тоже, что и оператор "|"
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Объединенный список</returns>
    public static IdList operator +(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList Res = a.Clone();
      foreach (Int32 id in b)
        Res.Add(id);
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из первого списка, которых нет во втором списке.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <param name="a">Первый список (большой)</param>
    /// <param name="b">Второй список (вычитаемый)</param>
    /// <returns>Разностный список</returns>
    public static IdList operator -(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList res = new IdList();
      foreach (Int32 id in a)
      {
        if (!b.Contains(id))
          res.Add(id);
      }
      return res;
    }

    #endregion

    #region Прочие методы и свойства

    /// <summary>
    /// Возвращает список идентификаторов, разделенных запятыми
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return StdConvert.ToString(ToArray());
    }

    /// <summary>
    /// Если список содержит единственный идентификатор, свойство возвращает его.
    /// Иначе возвращается 0.
    /// Свойство может быть полезно для оптимизации кода, когда список из одного идентификатора обрабатывается особым образом
    /// </summary>
    public Int32 SingleId
    {
      get
      {
        if (_Items.Count == 1)
        {
          // Нет более короткого способа
          Int32[] a1 = new Int32[1];
          _Items.Keys.CopyTo(a1, 0);
          return a1[0];
        }
        else
          return 0;
      }
    }

    #endregion

    #region Статические методы создания из таблиц

    #region FromIds

    /// <summary>
    /// Создает список из поля "Id", которое является первичным ключом таблицы.
    /// Аналогично <see cref="DataTools.GetIds(DataTable)"/>.
    /// </summary>
    /// <param name="table">Таблица с полем "Id"</param>
    /// <returns>Список идентификаторов</returns>
    public static IdList FromIds(DataTable table)
    {
      if (table == null)
        return new IdList(); // 20.08.2019

      int colPos = table.Columns.IndexOf("Id");
      if (colPos < 0)
        throw ExceptionFactory.ArgUnknownColumnName("table", table, "Id");

      IdList res = new IdList(false, table.Rows.Count);
      for (int i = 0; i < table.Rows.Count; i++)
      {
        if (table.Rows[i].RowState == DataRowState.Deleted)
          res._Items.Add((Int32)(table.Rows[i][colPos, DataRowVersion.Original]), null);
        else
          res._Items.Add((Int32)(table.Rows[i][colPos]), null);
      }
      return res;
    }

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице для 
    /// строк, входящих в просмотр <see cref="DataView"/>
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр <see cref="DataView"/></param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static IdList FromIds(DataView dv)
    {
      if (dv == null)
        return new IdList(); // 20.08.2019

      int colPos = dv.Table.Columns.IndexOf("Id");
      if (colPos < 0)
        throw ExceptionFactory.ArgUnknownColumnName("dv", dv.Table, "Id");

      IdList res = new IdList(false, dv.Count);
      for (int i = 0; i < dv.Count; i++)
      {
        DataRow row = dv[i].Row;
        if (row.RowState == DataRowState.Deleted)
          res._Items.Add((Int32)(row[colPos, DataRowVersion.Original]), null);
        else
          res._Items.Add((Int32)(row[colPos]), null);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк. В отличие от <see cref="FromColumn(ICollection{DataRow}, string)"/>,
    /// не проверяет нулевые значения и не проверяет повторы.
    /// </summary>
    /// <param name="rows">Коллекция строк</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromIds(ICollection<DataRow> rows)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      IdList res = new IdList(false, rows.Count);

      int colPos = -1;
      foreach (DataRow row in rows)
      {
        if (colPos < 0)
        {
          colPos = row.Table.Columns.IndexOf("Id");
          if (colPos < 0)
            throw ExceptionFactory.ArgUnknownColumnName("rows", row.Table, "Id");
        }
        if (row.RowState == DataRowState.Deleted)
          res._Items.Add((Int32)(row[colPos, DataRowVersion.Original]), null);
        else
          res._Items.Add((Int32)(row[colPos]), null);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк <see cref="DataRowView"/>. В отличие от <see cref="FromColumn(ICollection{DataRowView}, string)"/>,
    /// не проверяет нулевые значения и не проверяет повторы
    /// </summary>
    /// <param name="rows">Коллекция строк типа <see cref="DataRowView"/></param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromIds(ICollection<DataRowView> rows)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      IdList res = new IdList(false, rows.Count);

      int colPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (colPos < 0)
        {
          colPos = drv.Row.Table.Columns.IndexOf("Id");
          if (colPos < 0)
            throw ExceptionFactory.ArgUnknownColumnName("rows", drv.Row.Table, "Id");
        }
        if (drv.Row.RowState == DataRowState.Deleted)
          res._Items.Add((Int32)(drv.Row[colPos, DataRowVersion.Original]), null);
        else
          res._Items.Add((Int32)(drv.Row[colPos]), null);
      }

      return res;
    }

    /// <summary>
    /// Возвращает список, состоящий из одного идентификатора.
    /// Если передан нулевой идентификатор, то возвращается пустой список, который, однако, можно редактировать.
    /// </summary>
    /// <param name="id">Идентификатор, добавляемый в список</param>
    /// <returns>Список</returns>
    public static IdList FromId(Int32 id)
    {
      IdList list = new IdList();
      if (id != 0)
        list.Add(id);
      return list;
    }

    #endregion

    #region FromColumn

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromColumn(DataTable table, string columnName)
    {
      if (table == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
      int colPos = table.Columns.IndexOf(columnName);
      if (colPos < 0)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      foreach (DataRow row in table.Rows)
      {
        if (row.IsNull(colPos))
          continue;
        Int32 Id = (Int32)(row[colPos]);
        if (Id == 0)
          continue;

        if (!res._Items.ContainsKey(Id))
          res._Items.Add(Id, null);
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
    public static IdList FromColumn(DataView dv, string columnName)
    {
      if (dv == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      int colPos = dv.Table.Columns.IndexOf(columnName);
      if (colPos < 0)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", dv.Table, columnName);

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      for (int i = 0; i < dv.Count; i++)
      {
        if (dv[i].Row.IsNull(colPos))
          continue;
        Int32 id = (Int32)(dv[i].Row[colPos]);
        if (id == 0)
          continue;

        if (!res._Items.ContainsKey(id))
          res._Items.Add(id, null);
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
    public static IdList FromColumn(ICollection<DataRow> rows, string columnName)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений
      int colPos = -1;
      foreach (DataRow row in rows)
      {
        if (colPos < 0)
        {
          colPos = row.Table.Columns.IndexOf(columnName);
          if (colPos < 0)
            throw ExceptionFactory.ArgUnknownColumnName("columnName", row.Table, columnName);
        }

        if (row.IsNull(colPos))
          continue;
        Int32 id = (Int32)(row[colPos]);
        if (id == 0)
          continue;
        if (!res._Items.ContainsKey(id))
          res._Items.Add(id, null);
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
    public static IdList FromColumn(ICollection<DataRowView> rows, string columnName)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений
      int colPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (colPos < 0)
        {
          colPos = drv.Row.Table.Columns.IndexOf(columnName);
          if (colPos < 0)
            throw ExceptionFactory.ArgUnknownColumnName("columnName", drv.Row.Table, columnName);
        }

        if (drv.Row.IsNull(colPos))
          continue;
        Int32 id = (Int32)(drv.Row[colPos]);
        if (id == 0)
          continue;
        if (!res._Items.ContainsKey(id))
          res._Items.Add(id, null);
      }

      return res;
    }

    #endregion

    #region FromArray

    /// <summary>
    /// Получить список идентификаторов из другого массива.
    /// Повторяющиеся и нулевые идентификаторы пропускаются
    /// </summary>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns></returns>
    public static IdList FromArray(Int32[] ids)
    {
      if (ids == null)
        return new IdList();

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        if (!res._Items.ContainsKey(ids[i]))
          res._Items.Add(ids[i], null);
      }
      return res;
    }

    /// <summary>
    /// Получить список идентификаторов из другого массива.
    /// Повторяющиеся и нулевые идентификаторы пропускаются
    /// </summary>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns></returns>
    public static IdList FromArray(Int32[,] ids)
    {
      if (ids == null)
        return new IdList();

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      int n1 = ids.GetLowerBound(0);
      int n2 = ids.GetUpperBound(0);
      int m1 = ids.GetLowerBound(1);
      int m2 = ids.GetUpperBound(1);

      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          if (ids[i, j] == 0)
            continue;

          if (!res._Items.ContainsKey(ids[i, j]))
            res._Items.Add(ids[i, j], null);
        }
      }

      return res;
    }

    /// <summary>
    /// Получить список идентификаторов из jagged-массива.
    /// Повторяющиеся и нулевые идентификаторы пропускаются
    /// </summary>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns></returns>
    public static IdList FromArray(Int32[][] ids)
    {
      if (ids == null)
        return new IdList();

      IdList res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == null)
          continue;
        res.Add(ids[i]);
      }
      return res;
    }

    #endregion

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустой список идентификаторов с установленным значением <see cref="IsReadOnly"/>=true
    /// </summary>
    public static readonly IdList Empty = CreateEmpty();

    private static IdList CreateEmpty()
    {
      IdList res = new IdList();
      res.SetReadOnly();
      return res;
    }

    #endregion
  }

  /// <summary>
  /// Набор списков идентификаторов для нескольких таблиц.
  /// Реализует словарь, в котором ключом является имя таблицы, а значением - список идентификаторов <see cref="IdList"/>.
  /// Класс является потокобезопасным после вызова SetReadOnly(). В процессе заполнения класс не является потокобезопасным.
  /// Можно управлять чувствительностью к регистру для имен таблиц.
  /// </summary>
  [Serializable]
  public sealed class TableAndIdList : IDictionary<string, IdList>, ICloneable, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает набор
    /// </summary>
    /// <param name="ignoreCase">Должен ли игнорироваться регистр символов в именах таблиц</param>
    public TableAndIdList(bool ignoreCase)
    {
      _Tables = new TableDict(ignoreCase);
    }

    /// <summary>
    /// Создает набор.
    /// Имена таблиц будут чувствительны к регистру
    /// </summary>
    public TableAndIdList()
      : this(false)
    {
    }

    private TableAndIdList(int dummy)
      : this(false)
    {
      SetReadOnly();
    }

    #endregion

    #region Словарь таблиц

    [Serializable]
    private class TableDict : TypedStringDictionary<IdList>
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

    private TableDict _Tables;

    /// <summary>
    /// Доступ к списку идентификаторов для таблицы с заданным именем.
    /// Если таблицы еще не было в списке, то, если свойство <see cref="IsReadOnly"/>=false (список в режиме заполнения), создается
    /// новый пустой объект <see cref="IdList"/>, в который можно добавлять идентификаторы. Если <see cref="IsReadOnly"/>=true, 
    /// то возвращается ссылка на <see cref="IdList.Empty"/>.
    /// Установка свойства очищает существующий список идентификаторов для таблицы и заменяет его новым
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Список идентификаторов</returns>
    public IdList this[string tableName]
    {
      get
      {
        IdList list;
        if (!_Tables.TryGetValue(tableName, out list))
        {
          if (String.IsNullOrEmpty(tableName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

          if (_Tables.IsReadOnly)
            return IdList.Empty;
          else
          {
            list = new IdList();
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

        IdList lst = this[tableName];
        lst.Clear();
        lst.Add(value);
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

      IdList lst;
      if (!_Tables.TryGetValue(tableName, out lst))
        return false;

      return lst.Count > 0;
    }

    bool IDictionary<string, IdList>.ContainsKey(string key)
    {
      return Contains(key);
    }


    /// <summary>
    /// Возвращает список идентификаторов для заданной таблицы, если он есть и содержит идентификаторы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="value">Сюда помещается список идентификаторов или null</param>
    /// <returns>Наличие идентификаторов. Эквивалентно Contains(<paramref name="tableName"/>).</returns>
    public bool TryGetValue(string tableName, out IdList value)
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
    public bool this[string tableName, Int32 id]
    {
      get
      {
        IdList list;
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
        foreach (KeyValuePair<string, IdList> pair in _Tables)
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
        foreach (KeyValuePair<string, IdList> pair in _Tables)
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
    public static readonly TableAndIdList Empty = new TableAndIdList(0);

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
        foreach (KeyValuePair<string, IdList> pair in _Tables)
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
    public TableAndIdList Clone()
    {
      TableAndIdList res = new TableAndIdList(_Tables.IgnoreCase);
      foreach (KeyValuePair<string, IdList> pair in _Tables)
        res._Tables.Add(pair.Key, pair.Value.Clone());
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Сложение и вычитание

    #region Методы модификации текущего списка

    /// <summary>
    /// Добавляет все идентификаторы из другого списка.
    /// Если в текущем списке уже есть такие идентификаторы, то они пропускаются
    /// </summary>
    /// <param name="source">Добавляемый список</param>
    public void Add(TableAndIdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (string tableName in source.GetTableNames())
        this[tableName].Add(source[tableName]);
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
      IdList lst;
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
    public void Remove(TableAndIdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (string tableName in source.GetTableNames())
      {
        IdList list;
        if (_Tables.TryGetValue(tableName, out list))
          list.Remove(source[tableName]);
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
    public static TableAndIdList operator |(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList res = a.Clone();
      res.Add(b);
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
    public static TableAndIdList operator +(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList res = a.Clone();
      res.Add(b);
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из первого списка, которых нет во втором списке.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <param name="a">Первый список (большой)</param>
    /// <param name="b">Второй список (вычитаемый)</param>
    /// <returns>Разностный список</returns>
    public static TableAndIdList operator -(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList res = a.Clone();
      res.Remove(b);
      return res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы, входящие в оба списка.
    /// У результирующего списка свойство <see cref="IsReadOnly"/> не установлено.
    /// </summary>
    /// <param name="a">Первый список</param>
    /// <param name="b">Второй список</param>
    /// <returns>Список с общими идентификаторами</returns>
    public static TableAndIdList operator &(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList res = new TableAndIdList();
      foreach (string tableName in a.GetTableNames())
      {
        IdList resList = a[tableName] & b[tableName];
        if (resList.Count > 0)
          res[tableName].Add(resList);
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
    public static bool operator ==(TableAndIdList a, TableAndIdList b)
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
        IdList listA = a[tableName];
        IdList listB = b[tableName];
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
    public static bool operator !=(TableAndIdList a, TableAndIdList b)
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
      TableAndIdList b = obj as TableAndIdList;
      return (this == b);
    }

    /// <summary>
    /// Хэш-код для коллекций.
    /// Так как <see cref="TableAndIdList"/> не предназначен для использования в качестве ключа коллекции,
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
    public struct Enumerator : IEnumerator<KeyValuePair<string, IdList>>
    {
      #region Защищенный конструктор

      internal Enumerator(TableAndIdList owner)
      {
        _Owner = owner;
        _BaseEnumerator = owner._Tables.GetEnumerator();
      }

      #endregion

      #region Поля

      private TableAndIdList _Owner;

      private IEnumerator<KeyValuePair<string, IdList>> _BaseEnumerator;

      #endregion

      #region IEnumerator<KeyValuePair<string,IdList>> Members

      /// <summary>
      /// Возвращает текущую пару
      /// </summary>
      public KeyValuePair<string, IdList> Current { get { return _BaseEnumerator.Current; } }

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

    IEnumerator<KeyValuePair<string, IdList>> IEnumerable<KeyValuePair<string, IdList>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region IDictionary<string,IdList> Members

    void IDictionary<string, IdList>.Add(string key, IdList value)
    {
      this[key].Add(value);
    }

    ICollection<string> IDictionary<string, IdList>.Keys
    {
      get { throw new NotImplementedException(); }
    }

    ICollection<IdList> IDictionary<string, IdList>.Values
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ICollection<KeyValuePair<string,IdList>> Members

    int ICollection<KeyValuePair<string, IdList>>.Count
    {
      get
      {
        int cnt = 0;
        foreach (KeyValuePair<string, IdList> pair in this)
          cnt++;
        return cnt;
      }
    }

    void ICollection<KeyValuePair<string, IdList>>.Add(KeyValuePair<string, IdList> item)
    {
      this[item.Key].Add(item.Value);
    }

    bool ICollection<KeyValuePair<string, IdList>>.Contains(KeyValuePair<string, IdList> item)
    {
      return this[item.Key].Contains(item.Value);
    }

    void ICollection<KeyValuePair<string, IdList>>.CopyTo(KeyValuePair<string, IdList>[] array, int arrayIndex)
    {
      List<KeyValuePair<string, IdList>> lst = new List<KeyValuePair<string, IdList>>();
      foreach (KeyValuePair<string, IdList> pair in this)
        lst.Add(pair);
      lst.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, IdList>>.Remove(KeyValuePair<string, IdList> item)
    {
      IdList lst;
      if (!TryGetValue(item.Key, out lst))
        return false;

      bool res = false;
      foreach (Int32 id in item.Value)
      {
        if (lst.Remove(id))
          res = true;
      }
      return res;
    }

    #endregion
  }
}
