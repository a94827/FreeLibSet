﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Хранилище идентификаторов первых строк страниц таблиц, которые должны быть очищены в результате изменения или удаления строк
  /// Этот класс НЕ ЯВЛЯЕТСЯ потокобезопасным в режиме записи (пока <see cref="DBxClearCacheData.IsReadOnly"/>=false).
  /// </summary>
  [Serializable]
  public sealed class DBxClearCacheData : IReadOnlyObject, IEnumerable<KeyValuePair<string, IdList>>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список для очистки
    /// </summary>
    public DBxClearCacheData()
    {
      _AreAllTables = false;
      _Items = new Dictionary<string, IdList>();
    }

    #endregion

    #region Добавление данных
                                                         
    /// <summary>
    /// Добавляет идентификаторы таблицы для очистки кэша.
    /// Нулевые идентификаторы пропускаются.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="ids">Массив идентификаторов</param>
    public void Add(string tableName, Int32[] ids)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

      if (AreAllTables)
        return;

      if (ids.Length == 0)
        return;

      IdList lst;
      if (!_Items.TryGetValue(tableName, out lst))
      {
        lst = new IdList();
        _Items.Add(tableName, lst);
      }
      else if (Object.ReferenceEquals(lst, null))
        return; // вся таблица помечена на удаление

      for (int i = 0; i < ids.Length; i++)
        DoAdd(lst, ids[i]);
    }

    private void DoAdd(IdList lst, Int32 Id)
    {
      if (Id <= 0) // 25.06.2021 - Фиктивные идентификаторы пропускаются
        return;

      Int32 firstId = DBxTableCache.GetFirstPageId(Id);
      lst.Add(firstId);
    }

    /// <summary>
    /// Добавляет идентификатор таблицы для очистки кэша.
    /// Нулевые идентификаторы пропускаются.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    public void Add(string tableName, Int32 id)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

      if (id == 0)
        return;

      if (AreAllTables)
        return;

      IdList lst;
      if (!_Items.TryGetValue(tableName, out lst))
      {
        lst = new IdList();
        _Items.Add(tableName, lst);
      }
      else if (Object.ReferenceEquals(lst, null))
        return; // вся таблица помечена на удаление

      DoAdd(lst, id);
    }

    /// <summary>
    /// Помечает таблицу целиком на обновление
    /// </summary>
    /// <param name="tableName">Имя обновляемой таблицы</param>
    public void Add(string tableName)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

      if (AreAllTables)
        return;

      IdList lst;
      if (!_Items.TryGetValue(tableName, out lst))
        _Items.Add(tableName, null);
      else
        _Items[tableName] = null;
    }

    /// <summary>
    /// Вызов этого метода означает, что все кэшированные данные должны быть удалены
    /// </summary>
    public void AddAllTables()
    {
      CheckNotReadOnly();

      if (_AreAllTables)
        return;

      _AreAllTables = true;
      _Items.Clear(); // коллекция идентифификаторов больше не нужна
    }

    #endregion

    #region Свойства

    private readonly Dictionary<string, IdList> _Items;

    /// <summary>
    /// Возвращает true, ели был вызов AddAllTables()
    /// </summary>
    public bool AreAllTables { get { return _AreAllTables; } }
    private bool _AreAllTables;

    /// <summary>
    /// Возвращает true, если в объект не было добавлено изменений
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return (!_AreAllTables) && _Items.Count == 0;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если набор находится в режиме просмотра.
    /// В этом случае вызовы методов Add() не допускаются
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Проверка режима IsReadOnly
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Установка свойства IsReadOnly=true
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,IdList>> Members

    /// <summary>
    /// Возвращает перечислитель.
    /// Не предназначено для использования в пользовательском коде.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<string, IdList>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, IdList>> IEnumerable<KeyValuePair<string, IdList>>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Возвращает true, если есть данные по сбросу для этой таблицы.
    /// Также возвращает true, если требуется полный сброс. 
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Наличие</returns>
    public bool ContainsTable(string tableName)
    {
      if (AreAllTables)
        return true;
      else
        return _Items.ContainsKey(tableName);
    }

    /// <summary>
    /// Возвращает список идентификаторов первых строк страниц для данной таблицы.
    /// Возвращает null, если требуется полный сброс таблицы.
    /// Возвращает пустой список, если таблицы нет в списке, или для таблицы было только создание новых документов.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Cписок идентификаторов или null</returns>
    public IdList this[string tableName]
    {
      get
      {
        IdList res;
        if (AreAllTables)
          res = null;
        else if (!_Items.TryGetValue(tableName, out res))
          res = IdList.Empty;
        return res;
      }
    }

    /// <summary>
    /// Возвращает true, если указанный идентификатор строки таблицы есть в списке на обновление.
    /// Учитывается, что в списке хранятся только идентификаторы первых страниц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Наличие идентификатора</returns>
    public bool this[string tableName, Int32 id]
    {
      get
      {
        if (id == 0)
          return false;
        IdList lst = this[tableName];
        if (Object.ReferenceEquals(lst, null))
          return true;
        Int32 firstId = DBxTableCache.GetFirstPageId(id);
        return lst.Contains(firstId);
      }
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Empty";
      if (AreAllTables)
        return "All tables";

      StringBuilder sb = new StringBuilder();
      foreach (KeyValuePair<string, IdList> pair in _Items)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(pair.Key);
        sb.Append(": ");
        if (Object.ReferenceEquals(pair.Value, null))
          sb.Append("all rows");
        else
        {
          Int32[] firstIds = pair.Value.ToArray();
          Array.Sort<Int32>(firstIds);
          // Объединяем повторяющиеся блоки
          // Лень!
          for (int i = 0; i < firstIds.Length; i++)
          {
            if (i > 0)
              sb.Append(",");
            Int32 LastId = firstIds[i] + 99;
            sb.Append(firstIds[i]);
            sb.Append("-");
            sb.Append(LastId);
          }
        }
      }

      return sb.ToString();
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Статический экземпляр объекта, требующий очистки буферов всех таблиц
    /// </summary>
    public static readonly DBxClearCacheData AllTables = CreateAllTables();

    private static DBxClearCacheData CreateAllTables()
    {
      DBxClearCacheData data = new DBxClearCacheData();
      data.AddAllTables();
      data.SetReadOnly();
      return data;
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

    /// <summary>
    /// Создает объект
    /// </summary>
    public DBxClearCacheHolder()
    {
      _Current = new DBxClearCacheData();
      _SyncRoot = new object();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущий накапливаемый список изменений
    /// </summary>
    private DBxClearCacheData _Current;

    /// <summary>
    /// Объект синхронизации
    /// </summary>
    private readonly object _SyncRoot;

    #endregion

    #region Методы установки

    /// <summary>
    /// Помечает выбранные идентификаторы таблицы на обновление
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="ids">Массив идентификаторов</param>
    public void Add(string tableName, Int32[] ids)
    {
      lock (_SyncRoot)
      {
        _Current.Add(tableName, ids);
      }
    }

    /// <summary>
    /// Помечает выбранный идентификатор таблицы на обновление
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    public void Add(string tableName, Int32 id)
    {
      lock (_SyncRoot)
      {
        _Current.Add(tableName, id);
      }
    }

    /// <summary>
    /// Помечает выбранную таблицу целиком на обновление
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public void Add(string tableName)
    {
      lock (_SyncRoot)
      {
        _Current.Add(tableName);
      }
    }

    /// <summary>
    /// Помечает все таблицы на обновление
    /// </summary>
    public void AddAllTables()
    {
      lock (_SyncRoot)
      {
        _Current.AddAllTables();
      }
    }

    #endregion

    #region Переключение на новый набор

    /// <summary>
    /// Переключается на новый список изменений.
    /// Возвращает предыдущий список.
    /// Метод не выполняет переключение и возвращает null, если в текущем списке не зарегистрировано изменений.
    /// </summary>
    /// <returns>Предыдуший список</returns>
    public DBxClearCacheData Swap()
    {
      DBxClearCacheData prev;
      lock (_SyncRoot)
      {
        if (_Current.IsEmpty)
          prev = null;
        else
        {
          prev = _Current;
          _Current = new DBxClearCacheData();
          prev.SetReadOnly();
        }
      }
      return prev;
    }

    #endregion
  }

  /// <summary>
  /// Хранит несколько последних <see cref="DBxClearCacheData"/>. Также содержит объект <see cref="DBxClearCacheHolder"/>.
  /// Класс является потокобезопасным.
  /// </summary>
  public sealed class DBxClearCacheBuffer
  {
    #region Конструктор

    /// <summary>
    /// Создает кольцевой буфер, содержащий <paramref name="bufferSize"/> элементов.
    /// </summary>
    /// <param name="bufferSize">Количество элементов в буфере</param>
    public DBxClearCacheBuffer(int bufferSize)
    {
      if (bufferSize < 2 || bufferSize > 1000)
        throw ExceptionFactory.ArgOutOfRange("bufferSize", bufferSize, 2, 1000);

      _Holder = new DBxClearCacheHolder();
      _Items = new RingBuffer<BufferItem>(bufferSize);
      _DisplayName = "DBxClearCacheBuffer";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Держатель текущего накопителя.
    /// Для регистрации запроса очистки данных должны вызываться его методы Add()
    /// </summary>
    public DBxClearCacheHolder Holder { get { return _Holder; } }
    private readonly DBxClearCacheHolder _Holder;

    /// <summary>
    /// Элемент кольцевого буфера
    /// </summary>
    private struct BufferItem
    {
      #region Поля

      /// <summary>
      /// Версия блока информации
      /// </summary>
      public int Version;

      /// <summary>
      /// Данные об обновлении
      /// </summary>
      public DBxClearCacheData Data;

      #endregion
    }

    /// <summary>
    /// Кольцевой буфер для хранения списков.
    /// Этот объект используется также как SycnRoot
    /// </summary>
    private readonly RingBuffer<BufferItem> _Items;

    #endregion

    #region Переключение набора

    /// <summary>
    /// Последняя версия набора в кольцевом буфере
    /// </summary>
    public int LastVersion { get { return _LastVersion; } }
    /// <summary>
    /// Можно было бы извлекать последнюю версию и из списка Items, но так - быстрее.
    /// Блокировку можно, я думаю, не ставить.
    /// Тем более, что это свойство используется только в отладочных целях.
    /// </summary>
    private int _LastVersion;

    /// <summary>
    /// Переключает на новый набор.
    /// Предыдущий элемент помещается в кольцевой буфер.
    /// Если в текущем наборе не зарегистрировано изменений, никаких действий не выполняется.
    /// Этот метод вызывается на стороне сервера по таймеру, например, 1 раз в минуту.
    /// </summary>
    /// <returns>True, если с момента предыдущего вызова метода <see cref="Swap()"/> были изменения и переключение было выполнено.
    /// Возвращает false, если метод не выполнил никаких действий</returns>
    public bool Swap()
    {
      DBxClearCacheData prevData;
      lock (_Items)
      {
        prevData = Holder.Swap();
        if (prevData != null)
        {
          checked { _LastVersion++; } // Переход к отрицательным значениям недопустим
          BufferItem bufItem = new BufferItem();
          bufItem.Version = _LastVersion;
          bufItem.Data = prevData;
          if (prevData.AreAllTables)
            // Можно очистить кольцевой буфер
            _Items.Clear();
          _Items.Add(bufItem);
          if (DBxCache.TraceSwitch.Enabled)
            TraceSwap(prevData);
        }
      }
      return prevData != null;
    }

    /// <summary>
    /// Вывод трассировки в методе Swap()
    /// </summary>
    /// <param name="prevData"></param>
    private void TraceSwap(DBxClearCacheData prevData)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DateTime.Now.ToString("G"));
      sb.Append(" ");
      sb.Append(DisplayName);
      sb.Append(". Item version=");
      sb.Append(_LastVersion);
      sb.Append(" added");
      Trace.WriteLine(sb.ToString());

      Trace.IndentLevel++;

      Trace.WriteLine("Tables:" + prevData.ToString());

      sb.Length = 0;
      sb.Append("Ring buffer items: ");
      for (int i = 0; i < _Items.Count; i++)
      {
        if (i > 0)
          sb.Append(",");
        sb.Append(_Items[i].Version);
      }
      Trace.WriteLine(sb.ToString());

      Trace.IndentLevel--;
    }

    #endregion

    #region Получение информации клиентами

    /// <summary>
    /// Получить информацию о необходимой очистке буферизации.
    /// Если с момента последнего вызова не было никаких сигналов на очистку буфера, или метод <see cref="Swap()"/> не
    /// вызывался, возвращается пустой массив.
    /// Если, наоборот, очень долго не было вызова <see cref="GetData(ref int)"/> и буфер "убежал", возвращается объект, требующий
    /// полной очистки буфера.
    /// </summary>
    /// <param name="version">На входе задает номер последней версии, с которой был вызван этот метод.
    /// На выходе содержит значение <see cref="LastVersion"/>, которое должно быть использовано при следующем вызове</param>
    /// <returns>Массив объектов из кольцевого буфера, содержащих информацию о необходимой очистке</returns>
    public DBxClearCacheData[] GetData(ref int version)
    {
      DBxClearCacheData[] res;
      lock (_Items)
      {
        int firstVersion = 0;
        if (_Items.Count > 0)
          firstVersion = _Items[0].Version;
#if DEBUG
        if (firstVersion > _LastVersion)
          throw new BugException("FirstVersion > LastVersion");
#endif

        if (version < firstVersion)
          // Либо слишком долго не было вызова GetData() и буфер "убежал",
          // либо в буфер попал вызов очистки всех таблиц и предыдущий буфер был очищен за ненадобностью
          res = _AllTablesArray;
        else if (version == _LastVersion)
          res = _EmptyArray; // Не было никаких изменений с последней версии
        else
        {
          if (version > _LastVersion)
            throw new ArgumentException(String.Format(Res.DBxClearCacheBuffer_Arg_VersionTooBig,
              version, _LastVersion), "version");

          // Все в порядке.
          // В кольцевом буфере есть элементы и с номером Version, и более новые
          int startPos = -1;
          for (int i = 0; i < _Items.Count; i++)
          {
            if (_Items[i].Version == version)
            {
              startPos = i;
              break;
            }
          }
          if (startPos < 0)
            throw new BugException("Item with version " + version.ToString()+" not found in the ring buffer");

          res = new DBxClearCacheData[_Items.Count - startPos - 1];
          // Нельзя использовать CopyTo(), т.к. в списке у нас структура BufferItem
          // FItems.CopyTo(StartPos+1, Res, 0, Res.Length);
          for (int i = 0; i < res.Length; i++)
            res[i] = _Items[startPos + i + 1].Data;
        }

        // Номер версии для следующего вызова
        version = _LastVersion;
      }

      return res;
    }

    #region Статические списки

    /// <summary>
    /// Возвращается при отсутствии изменений
    /// </summary>
    private static readonly DBxClearCacheData[] _EmptyArray = new DBxClearCacheData[0];

    /// <summary>
    /// Возвращается при необходимости выполнить полную очистку
    /// </summary>
    private static readonly DBxClearCacheData[] _AllTablesArray = CreateAllTablesArray();

    private static DBxClearCacheData[] CreateAllTablesArray()
    {
      DBxClearCacheData Data = new DBxClearCacheData();
      Data.AddAllTables();
      Data.SetReadOnly();
      return new DBxClearCacheData[1] { Data };
    }

    #endregion

    #endregion

    #region Трассировка

    /// <summary>
    /// Отображаемое имя при трассировке
    /// </summary>
    public string DisplayName
    {
      get { lock (_Items) { return _DisplayName; } }
      set
      {
        lock (_Items)
        {
          if (String.IsNullOrEmpty(value))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("value");
          _DisplayName = value;
        }
      }
    }
    private string _DisplayName;

    #endregion
  }

  #region Делегат

  /// <summary>
  /// Используется в ExtDBDocs
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="clearCacheData"></param>
  public delegate void DBxClearCacheEventHandler(object sender, DBxClearCacheData clearCacheData);

  #endregion
}
