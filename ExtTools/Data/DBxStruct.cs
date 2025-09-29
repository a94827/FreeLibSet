// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /*
   * Структура базы данных
   * ---------------------
   * 
   * Библиотека реализует следующие сценарии получения описаний структуры базы данных
   * 1. Извлекается реальная структура на основании вызовов DBConnection.GetSchema()
   * 2. Задается желаемая структура. При этом в запросах можно использовать только те таблицы и поля,
   *    которые описаны в желаемой структуре. На основании желаемой структуры можно выполнить создание / обновление
   *    реальной структуры БД (Создание / обновление структуры БД не является обязательным)
   * 3. Комбинированный метод. Извлекается реальная структура, в нее вносятся необходимые дополнения (например,
   *    описания ссылочных полей, которые нельзя определить из реальной структуры), затем измененная структура
   *    замещает исходную. При этом создание / обновление структуры таблиц БД не выполняется
   * 
   * Сценарий 1
   * По умолчанию используется автоматический способ получения реальной структуры. Никаких действий для этого
   * выполнять не требуется. При обращении к свойству DBx.Struct[ИмяТаблицы], если необходимо, выполняется
   * вызов DBConnection.GetSchema() и возвращается объект DBxTableStruct с доступом только для чтения
   * 
   * Полная реальная структура базы данных может быть очень большой, если база данных содержит множество
   * таблиц. Поэтому нецелесообразно загружать ее за один раз, особенно, если запросы выполняются к небольшому
   * числу таблиц. Загрузка структуры выполняется по одной таблице.
   * 
   * Сценарий 2
   * Для ручного описания структуры требуется создать объект DBxStruct и добавить в него описания таблиц.
   * После этого должно быть установлено свойство DBx.Struct. В результате, объект DBxStruct переводится
   * в режим "только для просмотра"
   * 
   * Чтобы выполнить создание / обовление структуры таблицы на основании DBx.Struct, должен быть явно вызван 
   * метод DBx.UpdateDBStruct()
   * 
   * Сценарий 3
   * Сначала берется реальная структура таблицы (читается свойство DBx.Struct) и выполняется вызов метод
   * Clone(), который создает копию объекта DBxStruct, который можно модифицировать (ReadOnly=false).
   * После этого, в копию вносятся изменения. Затем, свойство DBx.Struct устанавливается
   * 
   * Сериализация
   * ------------
   * Описания являются сериализуемыми, т.к. должны уметь передаваться от сервера к клиенту. Если сделать класс 
   * DBxStruct сериализуемым, то перед передачей должны быть загружены описания для всех таблиц.
   * Если сделать объект DBxStruct передаваемым по ссылке (Marshal-by-Reference), то при каждом обращении
   * клиента к описанию таблицы, оно будет передаваться заново.
   *
   * Решение.
   * Класс DBxStruct не является не сериализуемым, ни Marshal-by-reference. Вместо этого, объект ссылается на
   * промежуточный объект, производный от DBxStructSource, который является Marshal-by-reference, и обеспечивает 
   * загрузку описаний (DBxRealStructSource извлекает описание из объекта DBx, а DBxRemoteStructSource - из 
   * другого объекта DBxStruct). Объект DBxStruct содержит буферизуемый список описаний. Если объект содержит
   * структуру, созданную вручную (сценарии 2 и 3), то ссылки на DBxStructSource нет. Для передачи структуры
   * клиенту, на сервере создается объект DBxRemoteStructSource, который передается по сслыке клиенту. 
   * Клиент создает собственный объект DBxStruct, который ссылается на полученный DBxRemoteStructSource)
   */

  /// <summary>
  /// Базовый интерфейс для DBxRealStructSource (ExtDB.dll) и <see cref="DBxRemoteStructSource"/>.
  /// Используется в <see cref="DBxStruct"/>.
  /// </summary>
  public interface IDBxStructSource
  {
    #region Методы и свойства

    /// <summary>
    /// Возвращает true, если объект описывает реальную структуру базы данных.
    /// Возвращает false, если структура задана в коде приложения.
    /// </summary>
    bool IsRealStruct { get; }

    /// <summary>
    /// Получить полный список имен таблиц в виде массива (без структуры)
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    string[] GetAllTableNames();

    /// <summary>
    /// Получить описание структуры одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненное описание структуры таблицы</returns>
    DBxTableStruct GetTableStruct(string tableName);

    #endregion
  }

  /// <summary>
  /// Переходник для передачи описаний структуры таблиц от сервера к клиенту.
  /// Экземпляр объекта, присоединенный к объекту, возвращаемому свойством DBx.Struct (ExtDB.dll), передается по ссылке
  /// клиенту. На стороне клиента создается собственный объект <see cref="DBxStruct"/>, присоединенный к этому объекту.
  /// </summary>
  public sealed class DBxRemoteStructSource : MarshalByRefObject, IDBxStructSource
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="source">Структура базы данных. Не может быть null.
    /// Ссылка сохраняется в private-поле, чтобы к ней нельзя было получить неавторизованный доступ</param>
    public DBxRemoteStructSource(DBxStruct source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
      _IsRealStruct = source.IsRealStruct;
    }

    #endregion

    #region Поля

    /// <summary>
    /// Эта ссылка напрямую клиенту не видна
    /// </summary>
    private readonly DBxStruct _Source;


    /// <summary>
    /// Возвращает true, если объект описывает реальную структуру базы данных.
    /// Возвращает false, если структура задана в коде приложения.
    /// </summary>
    public bool IsRealStruct { get { return _IsRealStruct; } }
    private readonly bool _IsRealStruct;


    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Получить полный список имен таблиц в виде массива (без структуры)
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    public string[] GetAllTableNames()
    {
      return _Source.AllTableNames;
    }

    /// <summary>
    /// Получить описание структуры одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненное описание структуры таблицы</returns>
    public DBxTableStruct GetTableStruct(string tableName)
    {
      return _Source.Tables[tableName];
    }

    #endregion
  }

  /// <summary>
  /// Описание структуры базы данных.
  /// Используется для получения реальной структуры существующей базы данных (при заданном свойстве <see cref="DBxStruct.Source"/>)
  /// и для задания необходимой структуры базы данных.
  /// Все обращения к присоединенной к базе данных структуре являются потокобезопасными. 
  /// В процессе ручного заполнения обращения не являются потокобезопасными.
  /// </summary>
  public sealed class DBxStruct : ICloneableReadOnlyObject<DBxStruct>
  {
    #region Вложенный класс

    /// <summary>
    /// Список описаний структуры таблиц для реализации свойства <see cref="DBxStruct.Tables"/>
    /// </summary>
    public sealed class TableCollection : IList<DBxTableStruct>
    {
      // Работает по-разному, в зависимости от установки свойства DBxStruct.Source

      #region Защищенные конструкторы

      /// <summary>
      /// Основной конструктор
      /// </summary>
      /// <param name="owner">Создаваемый объект</param>
      internal TableCollection(DBxStruct owner)
      {
        _Owner = owner;
        _List = new NamedList<DBxTableStruct>(true);
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
      }

      /// <summary>
      /// Конструктор копирования, извлекающий описания всех таблиц из источника
      /// </summary>
      /// <param name="owner">Создаваемый объект</param>
      /// <param name="source">Источник у исходного объекта</param>
      internal TableCollection(DBxStruct owner, TableCollection source)
      {
        _Owner = owner;
        _List = new NamedList<DBxTableStruct>(source.AllTableNames.Length, true);
        _AllTableNames = source.AllTableNames;
        for (int i = 0; i < source.AllTableNames.Length; i++)
        {
          DBxTableStruct orgTable = source[source.AllTableNames[i]];
          _List.Add(orgTable.Clone());
        }
        _AllTableNamesIndexer = null;
      }

      #endregion

      #region Поля

      private readonly DBxStruct _Owner;

      private readonly NamedList<DBxTableStruct> _List;

      private string[] _AllTableNames; // not readonly

      private StringArrayIndexer _AllTableNamesIndexer;

      #endregion

      #region Полный список имен таблиц

      internal string[] AllTableNames
      {
        get
        {
          lock (_List)
          {
            if (_AllTableNames == null)
            {
              if (_Owner.Source == null)
              {
                // Собираем список таблиц из созданных структур
                _AllTableNames = new string[_List.Count];
                for (int i = 0; i < _List.Count; i++)
                  _AllTableNames[i] = _List[i].TableName;
              }
              else
              {
                // Извлекаем список таблиц из источника
                _AllTableNames = _Owner.Source.GetAllTableNames();
              }
            }
            return _AllTableNames;
          }
        }
      }

      #endregion

      #region Поиск таблицы по имени

      // При обращениях, для синхронизации используется блокировка объекта _List.
      // Блокировка требуется только для "читающих" методов,
      // т.к. "записывающие" методы могут вызываться только в процессе ручного создания (всегда синхронного),
      // а затем - блокируются IsReadOInly.

      /// <summary>
      /// Возвращает структуру таблицы по имени.
      /// Если нет такой таблицы, возвращается null.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Структура таблицы</returns>
      public DBxTableStruct this[string tableName]
      {
        get
        {
          if (String.IsNullOrEmpty(tableName))
            return null;

          lock (_List)
          {
            DBxTableStruct obj = _List[tableName];
            if (obj == null && _Owner.Source != null)
            {
              if (_AllTableNamesIndexer == null)
                _AllTableNamesIndexer = new StringArrayIndexer(AllTableNames, _List.IgnoreCase);

              if (_AllTableNamesIndexer.Contains(tableName))
              {
                obj = _Owner.Source.GetTableStruct(tableName);
                if (obj == null)
                  throw new NullReferenceException(String.Format(Res.DBxStruct_Err_GetTableStructTableMissed, tableName));
                _List.Add(obj);
              }
            }
            return obj;
          }
        }
      }

      /// <summary>
      /// Возвращает структуру таблицы по имени.
      /// Если нет такой таблицы, выбрасывается исключение
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Структура таблицы</returns>
      public DBxTableStruct GetRequired(string tableName)
      {
        DBxTableStruct tstr = this[tableName];
        if (tstr == null)
        {
          if (String.IsNullOrEmpty(tableName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
          else
            throw new KeyNotFoundException(String.Format(Res.DBxStruct_Err_TableNotFound, tableName));
        }
        return tstr;
      }

      /// <summary>
      /// Получить индекс таблицы по имени
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Индекс описания таблицы или (-1), если таблица не найдена</returns>
      public int IndexOf(string tableName)
      {
        if (String.IsNullOrEmpty(tableName))
          return -1;
        lock (_List)
        {
          if (_Owner.Source == null)
            return _List.IndexOf(tableName);
          else
          {
            if (_AllTableNamesIndexer == null)
              _AllTableNamesIndexer = new StringArrayIndexer(AllTableNames, _List.IgnoreCase);
            return _AllTableNamesIndexer.IndexOf(tableName);
          }
        }
      }

      #endregion

      #region IList<DBxTableStruct> Members

      /// <summary>
      /// Реализация интерфейса IList
      /// </summary>
      /// <param name="item"></param>
      /// <returns>Индекс описания таблицы</returns>
      public int IndexOf(DBxTableStruct item)
      {
        if (item == null)
          return -1;

        lock (_List)
        {
          if (_Owner.Source == null)
            return _List.IndexOf(item);
          else
          {
            if (_AllTableNamesIndexer == null)
              _AllTableNamesIndexer = new StringArrayIndexer(AllTableNames, _List.IgnoreCase);
            int p = _AllTableNamesIndexer.IndexOf(item.TableName);
            if (p >= 0)
            {
              DBxTableStruct item2 = this[item.TableName];
              if (!Object.ReferenceEquals(item2, item))
                return -1;
            }
            return p;
          }
        }
      }

      /// <summary>
      /// Реализация интерфейса IList
      /// </summary>
      /// <param name="index"></param>
      /// <param name="item"></param>
      public void Insert(int index, DBxTableStruct item)
      {
        _Owner.CheckNotReadOnly();
        _List.Insert(index, item);
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
      }

      /// <summary>
      /// Реализация интерфейса IList
      /// </summary>
      /// <param name="index"></param>
      public void RemoveAt(int index)
      {
        _Owner.CheckNotReadOnly();
        _List.RemoveAt(index);
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
      }

      /// <summary>
      /// Возвращает или присоединяет структуру таблицы по индексу
      /// </summary>
      /// <param name="index">Индекс таблицы в списке</param>
      /// <returns>Структура таблицы</returns>
      public DBxTableStruct this[int index]
      {
        get
        {
          lock (_List)
          {
            if (_Owner.Source == null)
              return _List[index];
            else
            {
              string tableName = AllTableNames[index];
              return this[tableName];
            }
          }
        }
        set
        {
          _Owner.CheckNotReadOnly();
          _List[index] = value;
          _AllTableNames = null;
        }
      }

      #endregion

      #region ICollection<DBxTableStruct> Members

      /// <summary>
      /// Добавляет описание таблицы в описание базы данных
      /// </summary>
      /// <param name="item">Описание таблицы</param>
      public void Add(DBxTableStruct item)
      {
        if (item == null)
          throw new ArgumentNullException("item");
        _Owner.CheckNotReadOnly();
        _List.Add(item);
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
      }

      /// <summary>
      /// Добавляет пустое описание таблицы в описание базы данных
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Описание структуры таблицы, которое следует заполнить</returns>
      public DBxTableStruct Add(string tableName)
      {
        DBxTableStruct item = new DBxTableStruct(tableName);
        Add(item);
        return item;
      }

      /// <summary>
      /// Удаляет описания всех таблиц
      /// </summary>
      public void Clear()
      {
        _Owner.CheckNotReadOnly();
        _List.Clear();
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
      }

      /// <summary>
      /// Реализация интерфейса ICollection
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public bool Contains(DBxTableStruct item)
      {
        return IndexOf(item) >= 0;
      }

      /// <summary>
      /// Возвращает true, если в списке есть описание структуры таблицы с заданным именем
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Наличие описания</returns>
      public bool Contains(string tableName)
      {
        lock (_List)
        {
          if (_Owner.Source == null)
            return _List.Contains(tableName);
          else
          {
            if (_AllTableNamesIndexer == null)
              _AllTableNamesIndexer = new StringArrayIndexer(AllTableNames, _List.IgnoreCase);
            return _AllTableNamesIndexer.Contains(tableName);
          }
        }
      }

      /// <summary>
      /// Реализация интерфейса <see cref="ICollection"/>
      /// </summary>
      /// <param name="array"></param>
      /// <param name="arrayIndex"></param>
      public void CopyTo(DBxTableStruct[] array, int arrayIndex)
      {
        lock (_List)
        {
          if (_Owner.Source == null)
            _List.CopyTo(array, arrayIndex);
          else
          {
            // Так будет медленно из-за пересоздания AllTableNames в циклен
            //int n = AllTableNames.Length;
            //for (int i = 0; i < n; i++)
            //  array[arrayIndex + i] = this[AllTableNames[i]];

            string[] aNames = new string[AllTableNames.Length];
            Array.Copy(AllTableNames, aNames, aNames.Length);
            for (int i = 0; i < aNames.Length; i++)
              array[arrayIndex + i] = this[aNames[i]];
          }
        }
      }

      /// <summary>
      /// Возвращает количество описаний таблиц, добавленных в структуру при <see cref="DBxStruct.IsRealStruct"/>=false.
      /// Возвращает количество таблиц в базе данных при <see cref="DBxStruct.IsRealStruct"/>=true.
      /// Реализация интерфейса <see cref="ICollection"/>
      /// </summary>
      public int Count
      {
        get
        {
          lock (_List)
          {
            if (_Owner.Source == null)
              return _List.Count;
            else
              return AllTableNames.Length;
          }
        }
      }

      /// <summary>
      /// Реализация интерфейса ICollection
      /// </summary>
      public bool IsReadOnly
      {
        get { return _Owner.IsReadOnly; }
      }

      /// <summary>
      /// Удаление описания таблицы.
      /// Таблица из базы данных не удаляется.
      /// </summary>
      /// <param name="item">Описание таблицы</param>
      /// <returns>true, если описание было удалено</returns>
      public bool Remove(DBxTableStruct item)
      {
        _Owner.CheckNotReadOnly();
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
        return _List.Remove(item);
      }

      /// <summary>
      /// Удаление описания таблицы.
      /// Таблица из базы данных не удаляется.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>true, если описание было удалено</returns>
      public bool Remove(string tableName)
      {
        _Owner.CheckNotReadOnly();
        _AllTableNames = null;
        _AllTableNamesIndexer = null;
        return _List.Remove(tableName);
      }

      #endregion

      #region IEnumerable<DBxTableStruct> Members

      #region Собственный класс перебора

      /// <summary>
      /// Создает перечислитель по таблицам
      /// </summary>
      public struct Enumerator : IEnumerator<DBxTableStruct>
      {
        #region Конструктор

        internal Enumerator(DBxStruct owner, string[] allTableNames)
        {
          _Owner = owner;
          _AllTableNames = allTableNames;
          _CurrIndex = -1;
        }

        #endregion

        #region Поля

        private readonly DBxStruct _Owner;

        /// <summary>
        /// Используем собственную копию списка таблиц для перебора, чтобы не отслеживать возможные изменения
        /// </summary>
        private readonly string[] _AllTableNames;

        private int _CurrIndex;

        #endregion

        #region IEnumerator<DBxTableStruct> Members

        /// <summary>
        /// Текущая таблица
        /// </summary>
        public DBxTableStruct Current
        {
          get
          {
            string tableName = _AllTableNames[_CurrIndex];
            return _Owner.Tables[tableName];
          }
        }

        /// <summary>
        /// Ничего не делает
        /// </summary>
        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
          get
          {
            return Current;
          }
        }

        /// <summary>
        /// Переход к следующей таблице
        /// </summary>
        /// <returns>true, если есть еще таблица для перебора</returns>
        public bool MoveNext()
        {
          _CurrIndex++;
          return _CurrIndex < _AllTableNames.Length;
        }

        void IEnumerator.Reset()
        {
          _CurrIndex = -1;
        }

        #endregion
      }

      #endregion

      /// <summary>
      /// Возвращает перечислитель по описаниям структур <see cref="DBxTableStruct"/> для всех таблиц (свойство <see cref="AllTableNames"/>)
      /// </summary>
      /// <returns>Перечислитель</returns>
      public Enumerator GetEnumerator()
      {
        return new Enumerator(_Owner, AllTableNames);
      }

      IEnumerator<DBxTableStruct> IEnumerable<DBxTableStruct>.GetEnumerator()
      {
        return new Enumerator(_Owner, AllTableNames);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return new Enumerator(_Owner, AllTableNames);
      }

      #endregion

      #region Дополнительно

      /// <summary>
      /// Возвращает описание последней добавленной таблицы.
      /// Обычно использование свойства не имеет смысла.
      /// </summary>
      public DBxTableStruct LastAdded
      {
        get
        {
          if (Count == 0)
            return null;
          else
            return this[Count - 1];
        }
      }

      internal void SetReadOnly()
      {
        foreach (DBxTableStruct ts in _List)
          ts.SetReadOnly();
      }

      /// <summary>
      /// Возвращает свойство <see cref="Count"/>
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return "Count=" + Count.ToString();
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор создает описание структуры для ручного заполнения.
    /// После заполнения структуры должно быть установлено свойство DBx.Struct (ExtDB.dll).
    /// Чтобы создать или обновить реальную структуру БД, также должен быть вызван метод DBx.UpdateStruct().
    /// </summary>
    public DBxStruct()
      : this((IDBxStructSource)null)
    {
    }

    /// <summary>
    /// Этот конструктор должен вызываться на стороне клиента.
    /// Если <paramref name="source"/> не null, то структура будет находиться в режиме "только для чтения".
    /// </summary>
    /// <param name="source">Источник данных для получения структуры, реализующий передачу marshal-by-Reference. 
    /// Может быть null.</param>
    public DBxStruct(IDBxStructSource source)
    {
      _Tables = new TableCollection(this);
      _Source = source;
      if (source != null)
        _IsReadOnly = true;

      _DefaultIdColumnType = DBxColumnType.Int32;
    }

    ///// <summary>
    ///// Этот конструктор должен вызываться на стороне клиента.
    ///// Структура будет находиться в режиме "только для чтения".
    ///// </summary>
    ///// <param name="con">Полученное клиентское соединение с базой данных. Не может быть null</param>
    //public DBxStruct(DBxCon con)
    //  : this(con.StructSource)
    //{
    //}

    /// <summary>
    /// Конструктор для копирования
    /// </summary>
    /// <param name="source">Оригинальная структура</param>
    private DBxStruct(DBxStruct source)
    {
      _Tables = new TableCollection(this, source.Tables);
      _Source = null;
      _IsReadOnly = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Источник для загрузки описаний таблиц, если список предназначен для получения реальной структуры
    /// базы данных или передан клиенту по сети
    /// </summary>
    public IDBxStructSource Source { get { return _Source; } }
    private readonly IDBxStructSource _Source;

    /// <summary>
    /// Полный список таблиц
    /// </summary>
    public string[] AllTableNames { get { return _Tables.AllTableNames; } }

    /// <summary>
    /// Доступ к структуре таблиц
    /// </summary>
    public TableCollection Tables { get { return _Tables; } }
    private readonly TableCollection _Tables;


    /// <summary>
    /// Возвращает true, если объект описывает реальную структуру базы данных.
    /// Возвращает false, если структура задана в коде приложения.
    /// </summary>
    public bool IsRealStruct
    {
      get
      {
        if (_Source == null)
          return false;
        else
          return _Source.IsRealStruct;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание структуры было переведено в режим просмотра
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    /// <summary>
    /// Переводит описание структуры в режим просмотра.
    /// Повторные вызовы метода игнорируются.
    /// Вызывается метод <see cref="UpdateColumnTypes()"/>.
    /// </summary>
    public void SetReadOnly()
    {
      if (!_IsReadOnly)
      {
        UpdateColumnTypes();
        _Tables.SetReadOnly(); // 07.06.2023
      }
      _IsReadOnly = true;
    }

    #endregion

    #region UpdateColumnTypes()

    /// <summary>
    /// Тип ключевых полей таблиц по умолчанию.
    /// Используется в <see cref="UpdateColumnTypes()"/>, если в таблицу добавлено ключевое поле с помощью метода <see cref="DBxTableStruct.ColumnCollection.AddId(string)"/>,
    /// который не определяет тип поля.
    /// По умолчанию - <see cref="DBxColumnType.Int32"/>.
    /// </summary>
    public DBxColumnType DefaultIdColumnType
    {
      get { return _DefaultIdColumnType; }
      set
      {
        CheckNotReadOnly();
        if (!DBxTools.IsValidIdColumnType(value))
          throw ExceptionFactory.ArgUnknownValue("value", value);
        _DefaultIdColumnType = value;
      }
    }
    private DBxColumnType _DefaultIdColumnType;

    /// <summary>
    /// Устанавливает <see cref="DBxColumnStruct.ColumnType"/> для столбцов первичного ключа и ссылочных полей,
    /// если тип данных не был задан в явном виде при объявлении столбцов.
    /// </summary>
    public void UpdateColumnTypes()
    {
      CheckNotReadOnly();

      #region Поля первичного ключа

      foreach (DBxTableStruct ts in Tables)
      {
        if (ts.PrimaryKey.Count == 1)
        {
          DBxColumnStruct cs = ts.Columns[ts.PrimaryKey[0]];
          if (cs.ColumnType == DBxColumnType.Unknown)
            cs.ColumnType = DefaultIdColumnType;
        }
      }

      #endregion

      #region Ссылочные поля

      foreach (DBxTableStruct ts in Tables)
      {
        foreach (DBxColumnStruct cs in ts.Columns)
        {
          if ((!String.IsNullOrEmpty(cs.MasterTableName)) && cs.ColumnType == DBxColumnType.Unknown)
          {
            DBxTableStruct ts2 = Tables[cs.MasterTableName];
            if (ts2.PrimaryKey.Count == 1)
            {
              DBxColumnStruct cs2 = ts2.Columns[ts2.PrimaryKey[0]];
              cs.ColumnType = cs2.ColumnType;
            }
          }
        }
      }

      #endregion
    }

    #endregion

    #region ICloneable Members

    /// <summary> 
    /// Возвращает копию объекта, доступную для редактирования (<see cref="IsReadOnly"/>=false). 
    /// Копия не ссылается на источник описаний таблиц <see cref="Source"/>. 
    /// В процессе копирования реальной структуры данных извлекаются описания для всех таблиц.
    /// </summary>
    public DBxStruct Clone()
    {
      return new DBxStruct(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию структуры базы данных, если текущий объект предназначен только для чтения.
    /// Если свойство <see cref="IsReadOnly"/>=false, возвращается ссылка на текущий объект.
    /// </summary>
    /// <returns>Новый или текущий объект</returns>
    public DBxStruct CloneIfReadOnly()
    {
      if (IsReadOnly)
        return Clone();
      else
        return this;
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Создает объект <see cref="DataSet"/>, содержащий пустые <see cref="DataTable"/> для всех таблиц в текущем объекте <see cref="DBxStruct"/>.
    /// См. описание метода <see cref="FreeLibSet.Data.DBxTableStruct.CreateDataTable()"/>.
    /// Объекты <see cref="System.Data.DataRelation"/> не создаются.
    /// </summary>
    /// <returns><see cref="DataSet"/> с таблицами, но без строк</returns>
    public DataSet CreateDataSet()
    {
      DataSet ds = new DataSet();
      string[] a = AllTableNames; // этот вызов может быть медленным
      for (int i = 0; i < a.Length; i++)
      {
        DBxTableStruct ts = Tables[AllTableNames[i]];
        ds.Tables.Add(ts.CreateDataTable());
      }
      return ds;
    }

    /// <summary>
    /// Создает пустую таблицу с заданным списком столбцов.
    /// Можно использовать ссылочные столбцы, содержащие "."
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов. Если null, то возвращаются все столбцы таблицы (<see cref="DBxTableStruct.CreateDataTable()"/>)</param>
    /// <returns>Пустая таблица данных</returns>
    public DataTable CreateDataTable(string tableName, DBxColumns columnNames)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      DBxTableStruct ts = Tables[tableName];
      if (ts == null)
        throw ExceptionFactory.ArgUnknownValue("tableName", tableName);

      if (columnNames == null)
        return ts.CreateDataTable();

      DataTable table = new DataTable(ts.TableName);
      for (int i = 0; i < columnNames.Count; i++)
      {
        DBxColumnStruct colDef = this.FindColumn(tableName, columnNames[i]);
        if (colDef == null)
          throw new ArgumentException(String.Format(Res.DBxStruct_Arg_ColumnNotFound, tableName, columnNames[i]), "columnNames");
        table.Columns.Add(colDef.CreateDataColumn(columnNames[i]));
      }
      return table;
    }

    /// <summary>
    /// Поиск столбца таблиц по имени.
    /// Позволяет использовать в качестве имени столбца ссылочное поле, содержащее точки.
    /// Возвращает null, если таблица или столбец не найдены
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Описание столбца</returns>
    public DBxColumnStruct FindColumn(string tableName, string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        return null;
      DBxTableStruct ts = Tables[tableName];
      while (ts != null)
      {
        int p = columnName.IndexOf('.');
        if (p >= 0)
        {
          // Ссылочное поле
          string refColName = columnName.Substring(0, p);
          columnName = columnName.Substring(p + 1);
          DBxColumnStruct refCol = ts.Columns[refColName];
          if (refCol == null)
            return null;
          if (String.IsNullOrEmpty(refCol.MasterTableName))
            return null;
          ts = Tables[refCol.MasterTableName];
        }
        else
        {
          // Обычно поле
          return ts.Columns[columnName];
        }
      }
      return null;
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Source == null)
        return "Manual, Tables.Count=" + _Tables.Count.ToString();
      else
        return "Real";
    }

    /// <summary>
    /// Возвращает true, если есть комментарий хотя бы к одной из таблиц, столбцу или индексу
    /// </summary>
    public bool HasComments
    {
      get
      {
        for (int i = 0; i < Tables.Count; i++)
        {
          if (Tables[i].HasComments)
            return true;
        }
        return false;
      }
    }

    #endregion
  }
}
