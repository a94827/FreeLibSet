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
  /// Список идентификаторов Int32 с однократным вхождением.
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
      foreach (Int32 Id in source)
        Add(Id);
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
      foreach (Int32 Id in source)
        Add(Id);
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
      Int32[] Ids = (Int32[])(info.GetValue("Ids", typeof(Int32[])));
      _Items = new Dictionary<Int32, object>(Ids.Length);
      for (int i = 0; i < Ids.Length; i++)
        _Items.Add(Ids[i], null);
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
      Int32[] Ids = new Int32[_Items.Count];
      _Items.Keys.CopyTo(Ids, 0);
      return Ids;
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

      foreach (Int32 Id in source)
        Add(Id);
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

      foreach (Int32 Id in source)
        Add(Id);
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

      foreach (Int32 Id in source)
        Remove(Id);
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

      foreach (Int32 Id in source)
        Remove(Id);
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
      IdList Source2 = new IdList();
      foreach (Int32 Id in source)
        Source2.Add(Id);


      RemoveOthers(Source2);
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
        throw new ObjectReadOnlyException("Список идентификатор находится в режиме ReadOnly");
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

      foreach (Int32 OtherId in other)
      {
        if (Contains(OtherId))
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

      foreach (Int32 OtherId in other)
      {
        if (Contains(OtherId))
        {
          firstMatchedId = OtherId;
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
      Int32 FirstMatchedId;
      return ContainsAny(ids, out FirstMatchedId);
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

      foreach (Int32 OtherId in other)
      {
        if (!Contains(OtherId))
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
    /// В новом списке свойство IsReadOnly не установлено.
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
    /// Возвращает true, если <paramref name="obj"/> ссылается на IdList и списки совпадают.
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
    /// Хэш-код списка. Возвращает свойство Count.
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return Count;
    }

    #endregion

    #region Операторы сложения / вычитания

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков
    /// У результирующего списка свойство IsReadOnly не установлено
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

      IdList Res = a.Clone();
      foreach (Int32 Id in b)
        Res.Add(Id);
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы, входящие в оба списка
    /// У результирующего списка свойство IsReadOnly не установлено
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

      IdList Res = new IdList();
      foreach (Int32 Id in a)
      {
        if (b.Contains(Id))
          Res.Add(Id);
      }
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков
    /// У результирующего списка свойство IsReadOnly не установлено
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
      foreach (Int32 Id in b)
        Res.Add(Id);
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из первого списка, которых нет во втором списке
    /// У результирующего списка свойство IsReadOnly не установлено
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

      IdList Res = new IdList();
      foreach (Int32 Id in a)
      {
        if (!b.Contains(Id))
          Res.Add(Id);
      }
      return Res;
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
    /// Создает список из поля Id, которое является первичным ключом таблицы.
    /// Аналогично DataTools.GetIds()
    /// </summary>
    /// <param name="table">Таблица с полем "Id"</param>
    /// <returns>Список идентификаторов</returns>
    public static IdList FromIds(DataTable table)
    {
      if (table == null)
        return new IdList(); // 20.08.2019

      int ColPos = table.Columns.IndexOf("Id");
      if (ColPos < 0)
        throw new ArgumentException("Таблица\"" + table.TableName + "\" не содержит столбца \"Id\"", "table");

      IdList Res = new IdList(false, table.Rows.Count);
      for (int i = 0; i < table.Rows.Count; i++)
      {
        if (table.Rows[i].RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(table.Rows[i][ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(table.Rows[i][ColPos]), null);
      }
      return Res;
    }

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля "Id" в таблице для 
    /// строк, входящих в просмотр DataView
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static IdList FromIds(DataView dv)
    {
      if (dv == null)
        return new IdList(); // 20.08.2019

      int ColPos = dv.Table.Columns.IndexOf("Id");
      if (ColPos < 0)
        throw new ArgumentException("Таблица\"" + dv.Table.TableName + "\" не содержит столбца \"Id\"", "dv");

      IdList Res = new IdList(false, dv.Count);
      for (int i = 0; i < dv.Count; i++)
      {
        DataRow Row = dv[i].Row;
        if (Row.RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(Row[ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(Row[ColPos]), null);
      }
      return Res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк. В отличие от FromColumn()
    /// не проверяет нулевые значения и не проверяет повторы.
    /// </summary>
    /// <param name="rows">Массив строк</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromIds(ICollection<DataRow> rows)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      IdList Res = new IdList(false, rows.Count);

      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (ColPos < 0)
        {
          ColPos = Row.Table.Columns.IndexOf("Id");
          if (ColPos < 0)
            throw new ArgumentException("Таблица\"" + Row.Table.TableName + "\" не содержит столбца \"Id\"", "rows");
        }
        if (Row.RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(Row[ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(Row[ColPos]), null);
      }
      return Res;
    }

    /// <summary>
    /// Получение значений поля "Id" из массива строк DataRowView. В отличие от FromColumn()
    /// не проверяет нулевые значения и не проверяет повторы
    /// </summary>
    /// <param name="rows">Массив строк типа DataRowView</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromIds(ICollection<DataRowView> rows)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      IdList Res = new IdList(false, rows.Count);

      int ColPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (ColPos < 0)
        {
          ColPos = drv.Row.Table.Columns.IndexOf("Id");
          if (ColPos < 0)
            throw new InvalidOperationException("Таблица\"" + drv.Row.Table.TableName + "\" не содержит столбца \"Id\"");
        }
        if (drv.Row.RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(drv.Row[ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(drv.Row[ColPos]), null);
      }

      return Res;
    }

    /// <summary>
    /// Возвращает список, состоящий из одного идентификатора.
    /// Если передан нулевой идентификатор, то возвращается пустой список, который, однако, можно редактировать.
    /// </summary>
    /// <param name="id">Идентификатор, добавляемый в список</param>
    /// <returns>Список</returns>
    public static IdList FromId(Int32 id)
    {
      IdList List = new IdList();
      if (id != 0)
        List.Add(id);
      return List;
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
        throw new ArgumentNullException("columnName");
      int ColPos = table.Columns.IndexOf(columnName);
      if (ColPos < 0)
        throw new ArgumentException("Таблица\"" + table.TableName + "\" не содержит столбца \"" +
          columnName + "\"", "columnName");

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      foreach (DataRow Row in table.Rows)
      {
        if (Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(Row[ColPos]);
        if (Id == 0)
          continue;

        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле в таблице для строк, относящихся к 
    /// объекту DataView.
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// </summary>
    /// <param name="dv">Коллекция строк таблицы данных</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromColumn(DataView dv, string columnName)
    {
      if (dv == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      int ColPos = dv.Table.Columns.IndexOf(columnName);
      if (ColPos < 0)
        throw new ArgumentException("Таблица\"" + dv.Table.TableName + "\" не содержит столбца \"" +
          columnName + "\"", "columnName");

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      for (int i = 0; i < dv.Count; i++)
      {
        if (dv[i].Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(dv[i].Row[ColPos]);
        if (Id == 0)
          continue;

        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
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
        throw new ArgumentNullException("columnName");

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений
      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (ColPos < 0)
        {
          ColPos = Row.Table.Columns.IndexOf(columnName);
          if (ColPos < 0)
            throw new ArgumentException("Таблица\"" + Row.Table.TableName + "\" не содержит столбца \"" +
              columnName + "\"", "columnName");
        }

        if (Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(Row[ColPos]);
        if (Id == 0)
          continue;
        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
    }

    /// <summary>
    /// Получение списка числовых значений поля (идентификаторов), 
    /// которые принимает ссылочное поле для строк таблицы в массиве. 
    /// Нулевые значение отбрасывается и повторы отбрасываются
    /// Строки в массиве должны относиться либо к одной таблице, либо к таблицам,
    /// имеющим одинаковую структуру
    /// </summary>
    /// <param name="rows">Массив однотипных строк как коллекция DataRowView</param>
    /// <param name="columnName">Имя числового ссылочного поля</param>
    /// <returns>Массив идентификаторов</returns>
    public static IdList FromColumn(ICollection<DataRowView> rows, string columnName)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений
      int ColPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (ColPos < 0)
        {
          ColPos = drv.Row.Table.Columns.IndexOf(columnName);
          if (ColPos < 0)
            throw new ArgumentException("Таблица\"" + drv.Row.Table.TableName + "\" не содержит столбца \"" +
              columnName + "\"", "columnName");
        }

        if (drv.Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(drv.Row[ColPos]);
        if (Id == 0)
          continue;
        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
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

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        if (!Res._Items.ContainsKey(ids[i]))
          Res._Items.Add(ids[i], null);
      }
      return Res;
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

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

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

          if (!Res._Items.ContainsKey(ids[i, j]))
            Res._Items.Add(ids[i, j], null);
        }
      }

      return Res;
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

      IdList Res = new IdList(); // не стоит задавать емкость, т.к. реально может быть мало разных значений

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == null)
          continue;
        Res.Add(ids[i]);
      }
      return Res;
    }


    #endregion

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустой список идентификаторов с установленным значением IsReadOnly=true
    /// </summary>
    public static readonly IdList Empty = CreateEmpty();

    private static IdList CreateEmpty()
    {
      IdList Res = new IdList();
      Res.SetReadOnly();
      return Res;
    }

    #endregion
  }

  /// <summary>
  /// Набор списков идентификаторов для нескольких таблиц.
  /// Реализует словарь, в котором ключом является имя таблицы, а значением - список идентификаторов.
  /// Класс является потокобезопасным после вызова SetReadOnly(). В процессе заполнения класс не является потокобезопасным.
  /// Можно управлять чувствительностью к регистру для имен таблиц.
  /// </summary>
  [Serializable]
  public sealed class TableAndIdList : ICloneable, IReadOnlyObject
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
    /// Если таблицы еще не было в списке, то, если свойство IsReadOnly=false (список в режиме заполнения), создается
    /// новый пустой объект IdList, в который можно добавлять идентификаторы. Если IsReadOnly=true, то возвращается ссылка на IdList.Empty
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
            throw new ArgumentNullException("tableName");

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
    /// Создает копию списка, у которого признак IsReadOnly не установлен
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

    #endregion

    #region Операторы

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков
    /// У результирующего списка свойство IsReadOnly не установлено
    /// Тоже, что и оператор "+"
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

      TableAndIdList Res = a.Clone();
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из обоих списков
    /// У результирующего списка свойство IsReadOnly не установлено
    /// Тоже, что и оператор "+"
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

      TableAndIdList Res = a.Clone();
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы из первого списка, которых нет во втором списке
    /// У результирующего списка свойство IsReadOnly не установлено
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

      TableAndIdList Res = a.Clone();
      Res.Remove(b);
      return Res;
    }

    /// <summary>
    /// Возвращает массив, содержащий идентификаторы, входящие в оба списка
    /// У результирующего списка свойство IsReadOnly не установлено
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

      TableAndIdList Res = new TableAndIdList();
      foreach (string tableName in a.GetTableNames())
      {
        IdList resList = a[tableName] & b[tableName];
        if (resList.Count > 0)
          Res[tableName].Add(resList);
      }
      return Res;
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
    /// Так как TableAndNameList не предназначен для использования в качестве ключа коллекции,
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
  }
}
