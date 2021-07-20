using System;
using System.Collections.Generic;
using System.Text;

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// Хранилище идентификаторов строк таблиц, которые должны быть очищены в результате изменения или удаления строк
  /// Этот класс НЕ ЯВЛЯЕТСЯ потокобезопасным в режиме записи (пока IsReadOnly=false).
  /// </summary>
  [Serializable]
  public sealed class DBxClearCacheInfo : IReadOnlyObject, IEnumerable<KeyValuePair<string, IdList>>
  {
    #region Конструктор

    public DBxClearCacheInfo()
    {
      FAreAllTables = false;
      FItems = new Dictionary<string, IdList>();
    }

    #endregion

    #region Добавление данных

    public void Add(string TableName, Int32[] Ids)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");

      if (AreAllTables)
        return;

      if (Ids.Length == 0)
        return;

      IdList lst;
      if (!FItems.TryGetValue(TableName, out lst))
      {
        lst = new IdList();
        FItems.Add(TableName, lst);
      }
      lst.Add(Ids);
    }

    public void Add(string TableName, Int32 Id)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");

      if (Id == 0)
        throw new ArgumentException("Id=0", "Id");

      if (AreAllTables)
        return;

      IdList lst;
      if (!FItems.TryGetValue(TableName, out lst))
      {
        lst = new IdList();
        FItems.Add(TableName, lst);
      }
      lst.Add(Id);
    }

    /// <summary>
    /// Вызов этого метода означает, что все кэшированные данные должны быть удалены
    /// </summary>
    public void AddAllTables()
    {
      CheckNotReadOnly();

      if (FAreAllTables)
        return;

      FAreAllTables = true;
      FItems.Clear(); // коллекция идентифификаторов больше не нужна
    }

    #endregion

    #region Свойства

    private Dictionary<string, IdList> FItems;

    /// <summary>
    /// Возвращает true, ели был вызов AddAllTables()
    /// </summary>
    public bool AreAllTables { get { return FAreAllTables; } }
    private bool FAreAllTables;

    /// <summary>
    /// Возвращает true, если в объект не было добавлено изменений
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return (!FAreAllTables) && FItems.Count == 0;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если набор находится в режиме просмотра.
    /// В этом случае вызовы методов Add() не допускаются
    /// </summary>
    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    /// <summary>
    /// Проверка режима IsReadOnly
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (FIsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Установка свойства IsReadOnly=true
    /// </summary>
    public void SetReadOnly()
    {
      FIsReadOnly = true;
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,IdList>> Members

    public IEnumerator<KeyValuePair<string, IdList>> GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Держатель списка на очистку кэша.
  /// Этот класс является потокобезопасным
  /// </summary>
  public sealed class DBxClearCacheHolder
  {
    #region Конструктор

    public DBxClearCacheHolder()
    {
      FCurrent = new DBxClearCacheInfo();
      SyncRoot = new object();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущий накапливаемый список изменений
    /// </summary>
    private DBxClearCacheInfo FCurrent;

    /// <summary>
    /// Объект синхронизации
    /// </summary>
    private object SyncRoot;

    #endregion

    #region Методы установки

    public void Add(string TableName, Int32[] Ids)
    {
      lock (SyncRoot)
      {
        FCurrent.Add(TableName, Ids);
      }
    }

    public void Add(string TableName, Int32 Id)
    {
      lock (SyncRoot)
      {
        FCurrent.Add(TableName, Id);
      }
    }

    public void AddAllTables()
    {
      lock (SyncRoot)
      {
        FCurrent.AddAllTables();
      }
    }

    #endregion

    #region Переключение на новый набор

    /// <summary>
    /// Переключается на новый список изменений.
    /// Возращает предыдущий список.
    /// Метод не выполняет переключение и возращает null, если в текущем списке не зарегистрировано изменений
    /// </summary>
    /// <returns>Предыдуший список</returns>
    public DBxClearCacheInfo Swap()
    {
      DBxClearCacheInfo Prev;
      lock (SyncRoot)
      {
        if (FCurrent.IsEmpty)
          Prev = null;
        else
        {
          Prev = FCurrent;
          FCurrent = new DBxClearCacheInfo();
          Prev.SetReadOnly();
        }
      }
      return Prev;
    }

    #endregion
  }

  /// <summary>
  /// Хранит несколько последних DBxClearCacheInfo. Также содержит объект DBxClearCacheHolder.
  /// Класс является потокобезопасным
  /// </summary>
  public sealed class DBxClearCacheBuffer
  {
    #region Конструктор

    /// <summary>
    /// Создает кольцевой буфер, содержащий <paramref name="BufferSize"/> элементов.
    /// </summary>
    /// <param name="BufferSize">Количество элементов в буфере</param>
    public DBxClearCacheBuffer(int BufferSize)
    {
      if (BufferSize < 2 || BufferSize > 1000)
        throw new ArgumentOutOfRangeException();

      FHolder = Holder;
      FItems = new RingBuffer<BufferItem>(BufferSize);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Держатель текущего накопителя.
    /// Для регистрации запроса очистки данных должны вызываться его методы Add()
    /// </summary>
    public DBxClearCacheHolder Holder { get { return FHolder; } }
    private DBxClearCacheHolder FHolder;

    private struct BufferItem
    {
      #region Поля

      public int Version;
      public DBxClearCacheInfo Info;

      #endregion
    }

    /// <summary>
    /// Кольцевой буфер для хранения списков
    /// </summary>
    private RingBuffer<BufferItem> FItems;

    #endregion
  }
}
