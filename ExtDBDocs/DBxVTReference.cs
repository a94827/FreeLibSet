using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtDB.Docs
{
  /*
   * Реализация "переменных" ссылок между таблицами
   * Реализуется с помощью двух числовых полей типа Int32. Первое поле имеет имя
   * "ХххТаблица", где "Ххх" - имя ссылки. В нем хранится идентификатор
   * таблицы документа или поддокумента TableId (из таблицы DocTables).
   * Второе поле имеет имя "ХххИдентификатор", в нем храниться идентификатор Id
   * в таблице, на которую выполняется ссылка.
   * Ссылка считается пустой, если оба поля имеют значение Null. В противном 
   * случае ссылка должна быть корректной. Допустимость пустой ссылки определяется
   * свойством Column.CanBeEmpty.
   * Описатель ссылки имеет имя ("Ххх"), указатель на оба поля и список имен таблиц,
   * на которые могут выполняться ссылки. Ссылка недействительна, если ссылается на
   * таблицу, которой нет в списке
   *
   * Переменные ссылки могут быть объявлены для документов и поддокументов. При этом в структуру 
   * таблицы автоматически добавляется пара числовых полей
   */


  /// <summary>
  /// Ссылка на одну из нескольких таблиц.
  /// </summary>
  [Serializable]
  public class DBxVTReference
  {
    #region Защищенный конструктор

    internal DBxVTReference(string name, DBxTableStruct table, DBxColumnStruct tableColumn, DBxColumnStruct idColumn)
    {
      _Name = name;
      _Table = table;
      _TableColumn = tableColumn;
      _IdColumn = idColumn;
      _MasterTableNamesArray = new List<string>();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя ссылки
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    /// <summary>
    /// Таблица, в которой объявлена переменная ссылка
    /// </summary>
    public DBxTableStruct Table { get { return _Table; } }
    private DBxTableStruct _Table;

    /// <summary>
    /// Числовое поле, содержащее идентификатор таблицы
    /// </summary>
    public DBxColumnStruct TableColumn { get { return _TableColumn; } }
    private DBxColumnStruct _TableColumn;

    /// <summary>
    /// Числовое поле, содержащее идентификатор строки в таблице, на которую
    /// выполняется ссылка
    /// </summary>
    public DBxColumnStruct IdColumn { get { return _IdColumn; } }
    private DBxColumnStruct _IdColumn;

    /// <summary>
    /// Имена таблиц, на которые возможна ссылка. Свойство становиться доступно
    /// после инициализации БД
    /// </summary>
    public string[] MasterTableNames { get { return _MasterTableNames; } }
    private string[] _MasterTableNames;

    /// <summary>
    /// Идентификаторы таблиц, на которые возможна ссылка, то есть список возможных
    /// значений для поля TableColumn. Свойство становиться доступно
    /// после инициализации БД
    /// </summary>
    public Int32[] MasterTableIds { get { return _MasterTableIds; } }
    private Int32[] _MasterTableIds;

    /// <summary>
    /// Добавление таблицы, на которую возможна ссылка. Метод доступен, пока не
    /// выполнена инициализация БД.
    /// Возможен повторный вызов метода для той же мастер-таблицы, таблица 
    /// добавляется однократно.
    /// </summary>
    /// <param name="masterTableName">Имя мастер-таблицы</param>
    public void AddMasterTableName(string masterTableName)
    {
#if DEBUG
      Table.CheckNotReadOnly();
      if (_MasterTableNamesArray == null)
        throw new InvalidOperationException("Нельзя добавлять Master-таблицу после инициализации БД");
      if (string.IsNullOrEmpty(masterTableName))
        throw new ArgumentNullException("masterTableName");
#endif
      if (!_MasterTableNamesArray.Contains(masterTableName))
        _MasterTableNamesArray.Add(masterTableName);
    }

    /// <summary>
    /// Внутренний список, куда добавляются имена таблиц до инициализации
    /// </summary>
    private List<string> _MasterTableNamesArray;

    #endregion

    #region Методы инициализации

    /// <summary>
    /// Преобразование списка в массив
    /// </summary>
    internal void InternalInitPhase1()
    {
      _MasterTableNames = _MasterTableNamesArray.ToArray();
      _MasterTableNamesArray = null;
    }

    /// <summary>
    /// Присоединение массива идентификаторов таблиц
    /// </summary>
    /// <param name="masterTableIds"></param>
    internal void InternalInitPhase2(Int32[] masterTableIds)
    {
#if DEBUG
      if (masterTableIds.Length != MasterTableNames.Length)
        throw new ArgumentException("Неправильная длина массива", "masterTableIds");
#endif
      _MasterTableIds = masterTableIds;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства Table.VTReferenceCollection
  /// </summary>
  [Serializable]
  public class DBxVTReferenceList : IEnumerable<DBxVTReference>
  {
    #region Защищенный конструктор

    internal DBxVTReferenceList(DBxDocTypeBase owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описание документа или поддокумента, в котором располагается ссылка
    /// </summary>
    public DBxDocTypeBase Owner { get { return _Owner; } }
    private DBxDocTypeBase _Owner;

    /// <summary>
    /// Возвращает количество ссылок в списке
    /// </summary>
    public int Count
    {
      get
      {
        if (_Items == null)
          return 0;
        else
          return _Items.Count;
      }
    }

    /// <summary>
    /// Доступ к ссылке по индексу
    /// </summary>
    /// <param name="Index">Индекс от 0 до (Count-1)</param>
    /// <returns>Описание ссылки</returns>
    public DBxVTReference this[int Index]
    {
      get { return _Items[Index]; }
    }

    /// <summary>
    /// Доступ к ссылке по имени.
    /// Если нет ссылки с таким именем, возвращает null
    /// </summary>
    /// <param name="name">Имя ссылки</param>
    /// <returns>Описание ссылки или null</returns>
    public DBxVTReference this[string name]
    {
      get
      {
        int p = IndexOf(name);
        if (p < 0)
          return null;
        else
          return _Items[p];
      }
    }

    private List<DBxVTReference> _Items;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить ссылку. Создается два числовых поля к описанию полей
    /// </summary>
    /// <param name="name">Имя ссылки</param>
    /// <returns>Созданный объект ссылки</returns>
    public DBxVTReference Add(string name)
    {
#if DEBUG
      Owner.CheckNotReadOnly();
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
#endif
      DBxColumnStruct TableColumn = Owner.Struct.Columns.AddInt(name + "Таблица"); // TODO: Имена суффиксов
      DBxColumnStruct IdColumn = Owner.Struct.Columns.AddInt(name + "Идентификатор");
      DBxVTReference Item = new DBxVTReference(name, Owner.Struct, TableColumn, IdColumn);
      if (_Items == null)
        _Items = new List<DBxVTReference>();
      _Items.Add(Item);
      return Item;
    }

    /// <summary>
    /// Возвращает индекс ссылки с заданным именем
    /// </summary>
    /// <param name="name">Имя ссылки</param>
    /// <returns>Индекс или (-1)</returns>
    public int IndexOf(string name)
    {
      if (String.IsNullOrEmpty(name))
        return -1;
      if (_Items == null)
        return -1;
      for (int i = 0; i < _Items.Count; i++)
      {
        if (_Items[i].Name == name)
          return i;
      }
      return -1;
    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Используется для создания фиктивного перечислителя
    /// </summary>
    private static readonly List<DBxVTReference> _DummyItems = new List<DBxVTReference>();

    /// <summary>
    /// Возвращает перечислитель по DBxVTReference.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<DBxVTReference>.Enumerator GetEnumerator()
    {
      if (_Items == null)
        return _DummyItems.GetEnumerator();
      else
        return _Items.GetEnumerator();
    }

    IEnumerator<DBxVTReference> IEnumerable<DBxVTReference>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Простая структура, содержащая идентификатор таблицы документа и идентификатор документа
  /// Удобно использовать в качестве поля ErrorMessageItem.Tag для перехода к ошибочному документу
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct DBxVTValue : IEquatable<DBxVTValue>
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="tableId">Идентификатор таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    public DBxVTValue(Int32 tableId, Int32 docId)
    {
      _TableId = tableId;
      _DocId = docId;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор таблицы документа
    /// </summary>
    public Int32 TableId { get { return _TableId; } }
    private readonly Int32 _TableId;

    /// <summary>
    /// Идентификатор документа
    /// </summary>
    public Int32 DocId { get { return _DocId; } }
    private readonly Int32 _DocId;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _DocId == 0; } }

    /// <summary>
    /// Неинициализированная структура
    /// </summary>
    public static readonly DBxVTValue Empty = new DBxVTValue();

    /// <summary>
    /// Для отладки.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_DocId == 0)
        return "Empty";
      else
        return _TableId.ToString() + ":" + _DocId.ToString();
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение двух ссылок
    /// </summary>
    /// <param name="v1">Первая ссылка</param>
    /// <param name="v2">Вторая ссылка</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(DBxVTValue v1, DBxVTValue v2)
    {
      return v1.TableId == v2.TableId && v1.DocId == v2.DocId;
    }

    /// <summary>
    /// Сравнение двух ссылок
    /// </summary>
    /// <param name="v1">Первая ссылка</param>
    /// <param name="v2">Вторая ссылка</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(DBxVTValue v1, DBxVTValue v2)
    {
      return !(v1 == v2);
    }

    /// <summary>
    /// Сравнение двух ссылок
    /// </summary>
    /// <param name="obj">Вторая ссылка</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is DBxVTValue)
        return this == (DBxVTValue)obj;
      else
        return false;
    }

    /// <summary>
    /// Сравнение двух ссылок
    /// </summary>
    /// <param name="obj">Вторая ссылка</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(DBxVTValue obj)
    {
      return this == obj;
    }

    /// <summary>
    /// Возвращает DocId
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _DocId;
    }

    #endregion
  }
}
