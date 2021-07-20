using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using AgeyevAV;
using AgeyevAV.Config;

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
  /// <summary>
  /// Выборка документов. В одной выборке могут присутствовать документы разных
  /// видов. Документ не может входить в выборку дважды.
  /// Класс не является потокобезопасным.
  /// </summary>
  [Serializable]
  public sealed class DBxDocSelection: ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создать пустую выборку документов
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных (см. описание свойства)</param>
    public DBxDocSelection(string dbIdentity)
    {
      if (String.IsNullOrEmpty(dbIdentity))
        throw new ArgumentNullException("dbIdentity");

      _Data = new DataSet();
      _Data.RemotingFormat = SerializationFormat.Binary;
      _DBIdentity = dbIdentity;
    }

    /// <summary>
    /// Создает выборку из одного документа
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных (см. описание свойства)</param>
    /// <param name="tableName">Имя таблицы документа DBxDocType.Name</param>
    /// <param name="id">Идентификатор документа</param>
    public DBxDocSelection(string dbIdentity, string tableName, Int32 id)
      : this(dbIdentity)
    {
      Add(tableName, id);
    }

    /// <summary>
    /// Создает выборку из нескольких документа
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных (см. описание свойства)</param>
    /// <param name="tableName">Имя таблицы документа DBxDocType.Name</param>
    /// <param name="ids">Идентификаторы документов</param>
    public DBxDocSelection(string dbIdentity, string tableName, Int32[] ids)
      : this(dbIdentity)
    {
      Add(tableName, ids);
    }

    /// <summary>
    /// Создает копию выборки документов
    /// </summary>
    /// <param name="source">Исходная выборка документов</param>
    public DBxDocSelection(DBxDocSelection source)
      :this(source.DBIdentity)
    {
      Add(source);
    }

    /// <summary>
    /// Создает выборку документов, куда помещает идентификаторы только одного вида документов из исходной выборки
    /// </summary>
    /// <param name="source">Исходная выборка документов</param>
    /// <param name="tableName">Имя таблицы</param>
    public DBxDocSelection(DBxDocSelection source, string tableName)
      : this(source.DBIdentity)
    {
      Add(tableName, source[tableName]);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор базы данных (задается в конструкторе)
    /// Используется при работе с буфером обмена, чтобы не допустить вставку данных из программы,
    /// работающей с другой базой данных
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private string _DBIdentity;

    /// <summary>
    /// Основные данные. В каждой таблице имеется единственное поле Id
    /// </summary>
    private DataSet _Data;

    /// <summary>
    /// Список имен таблиц, входящих в выборку
    /// </summary>
    public string[] TableNames
    {
      get
      {
        if (_TableNames == null)
        {
          _TableNames = new string[_Data.Tables.Count];
          for (int i = 0; i < _TableNames.Length; i++)
            _TableNames[i] = _Data.Tables[i].TableName;
        }
        return _TableNames;
      }
    }
    [NonSerialized]
    [XmlIgnore]
    private string[] _TableNames;

    /// <summary>
    /// Получить массив идентификаторов заданной таблицы, входящих в выборку.
    /// Если нет идентификаторов для таблицы <paramref name="tableName"/>, то возвращается пустой
    /// массив.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] this[string tableName]
    {
      get
      {
        int p = _Data.Tables.IndexOf(tableName);
        if (p < 0)
          return DataTools.EmptyIds;
        else
          return DataTools.GetIds(_Data.Tables[p]);
      }
    }

    /// <summary>
    /// Получить число записей заданного вида.
    /// Если таблица не входит в выборку, возвращается 0.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Количество идентификаторов</returns>
    public int GetCount(string tableName)
    {
      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return 0;
      else
        return _Data.Tables[p].Rows.Count;
    }

    /// <summary>
    /// Общее число ссылок в выборке
    /// </summary>
    public int TotalCount
    {
      get
      {
        int cnt = 0;
        foreach (DataTable Table in _Data.Tables)
          cnt += Table.Rows.Count;
        return cnt;
      }
    }

    /// <summary>
    /// Возвращает true, если выборка не содержит ни одного документа
    /// </summary>
    public bool IsEmpty
    {
      get { return TotalCount == 0; }
    }

    #endregion

    #region Добавление / удаление документов

    ///// <summary>
    ///// Внутренний метод доступа к таблице с идентификаторами
    ///// </summary>
    ///// <param name="TableName"></param>
    ///// <returns></returns>
    //public DataTable GetTable(string TableName)
    //{
    //  return ReadyTable(TableName, true);
    //}

    private DataTable ReadyTable(string tableName, bool create)
    {
#if DEBUG
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
#endif
      int p = _Data.Tables.IndexOf(tableName);
      if (p >= 0)
        return _Data.Tables[p];
      if (!create)
        return null;

      _TableNames = null; // список имен таблиц больше недействителен

      // Добавляем таблицу
      DataTable Table = _Data.Tables.Add(tableName);
      Table.Columns.Add("Id", typeof(Int32));
      DataTools.SetPrimaryKey(Table, "Id");
      return Table;
    }

    /// <summary>
    /// Добавить документ в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="id">Идентификатор документа</param>
    /// <returns>1, если документ был добавлен, 0, если нет (уже есть в выборке или <paramref name="id"/>=0</returns>
    public int Add(string tableName, Int32 id)
    {
      if (id == 0)
        return 0;
      DataTable Table = ReadyTable(tableName, true);
      DataRow DummyRow;
      return DataTools.FindOrAddPrimaryKeyRow(Table, id, out DummyRow) ? 1 : 0;
    }

    /// <summary>
    /// Добавить несколько документов в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для добавления</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(string tableName, Int32[] ids)
    {
      if (ids == null)
        return 0;
      if (ids.Length == 0)
        return 0;
      DataTable Table = ReadyTable(tableName, true);
      int cnt = 0;
      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        DataRow DummyRow;
        if (DataTools.FindOrAddPrimaryKeyRow(Table, ids[i], out DummyRow))
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Добавить несколько документов в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для добавления</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(string tableName, IdList ids)
    {
      if (Object.ReferenceEquals(ids, null))
        return 0;
      if (ids.Count == 0)
        return 0;
      DataTable Table = ReadyTable(tableName, true);
      int cnt = 0;
      foreach (Int32 Id in ids)
      {
        DataRow DummyRow;
        if (DataTools.FindOrAddPrimaryKeyRow(Table, Id, out DummyRow))
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Добавить документы из другой выборки
    /// </summary>
    /// <param name="source">Исходная выборка</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(DBxDocSelection source)
    {
      if (source == null)
        return 0;

      int cnt = 0;
      foreach (DataTable SrcTable in source._Data.Tables)
      {
        DataTable ResTable = ReadyTable(SrcTable.TableName, true);
        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          // Можно не проверять удаленные строки
          Int32 Id = (Int32)(SrcRow["Id"]);
          DataRow DummyRow;
          if (DataTools.FindOrAddPrimaryKeyRow(ResTable, Id, out DummyRow))
            cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Добавить документы в выборку
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="table">Таблица, содержащая столбец "Id" с идентификаторами добавляемых документов</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(string tableName, DataTable table)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      DataTable ResTable = ReadyTable(tableName, true);
      int cnt = 0;
      foreach (DataRow SrcRow in table.Rows)
      {
        if (SrcRow.RowState == DataRowState.Deleted)
          continue; // 27.09.2017

        Int32 Id = (Int32)(SrcRow["Id"]);
        DataRow DummyRow;
        if (DataTools.FindOrAddPrimaryKeyRow(ResTable, Id, out DummyRow))
          cnt++;
      }

      return cnt;
    }

    /// <summary>
    /// Добавить документы в выборку.
    /// Свойство <paramref name="table"/>.TableName определяет вид документов.
    /// </summary>
    /// <param name="table">Таблица, содержащая столбец "Id" с идентификаторами добавляемых документов</param>
    /// <returns>Количество добавленных документов</returns>
    public int Add(DataTable table)
    {
      if (table == null)
        return 0;
      return Add(table.TableName, table);
    }

    /// <summary>
    /// Удалить документ из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="id">Идентификатор удаляемого документа</param>
    /// <returns>1, если документ был удален, 0, если такого документа нет в выборке</returns>
    public int Remove(string tableName, Int32 id)
    {
      if (id == 0)
        return 0;
      DataTable Table = ReadyTable(tableName, false);
      if (Table == null)
        return 0;
      DataRow Row = Table.Rows.Find(id);
      if (Row != null)
      {
        Table.Rows.Remove(Row);
        return 1;
      }
      else
        return 0;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName, Int32[] ids)
    {
      if (ids == null)
        return 0;
      if (ids.Length == 0)
        return 0;
      DataTable Table = ReadyTable(tableName, false);
      if (Table == null)
        return 0;

      int cnt = 0;
      for (int i = 0; i < ids.Length; i++)
      {
        DataRow Row = Table.Rows.Find(ids[i]);
        if (Row != null)
        {
          Table.Rows.Remove(Row);
          cnt++;
        }
      }

      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName, IdList ids)
    {
      if (ids == null)
        return 0;
      if (ids.Count == 0)
        return 0;
      DataTable Table = ReadyTable(tableName, false);
      if (Table == null)
        return 0;

      int cnt = 0;
      foreach (Int32 Id in ids)
      {
        DataRow Row = Table.Rows.Find(Id);
        if (Row != null)
        {
          Table.Rows.Remove(Row);
          cnt++;
        }
      }

      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="source">Выборка, содержащая документы, которые надо удалить из текущей выборки</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(DBxDocSelection source)
    {
      if (source == null)
        return 0;

      int cnt = 0;
      foreach (DataTable SrcTable in source._Data.Tables)
      {
        DataTable ResTable = ReadyTable(SrcTable.TableName, false);
        if (ResTable == null)
          continue;
        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          Int32 Id = (Int32)(SrcRow["Id"]);
          DataRow Row = ResTable.Rows.Find(Id);
          if (Row != null)
          {
            ResTable.Rows.Remove(Row);
            cnt++;
          }
        }
      }

      return cnt;
    }

    /// <summary>
    /// Удалить все ссылки заданного вида
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName)
    {
      DataTable Table = ReadyTable(tableName, false);
      if (Table == null)
        return 0;
      int cnt = Table.Rows.Count;
      _Data.Tables.Remove(Table);
      _TableNames = null;
      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="table">Таблица, в которой есть столбец "Id", содержащий идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(string tableName, DataTable table)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      DataTable ResTable = ReadyTable(tableName, false);
      if (ResTable == null)
        return 0;

      int cnt = 0;
      foreach (DataRow SrcRow in table.Rows)
      {
        if (SrcRow.RowState == DataRowState.Deleted)
          continue; // 27.09.2017
        Int32 Id = (Int32)(SrcRow["Id"]);
        DataRow Row = ResTable.Rows.Find(Id);
        if (Row != null)
        {
          ResTable.Rows.Remove(Row);
          cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Удалить документы из выборки.
    /// Свойство <paramref name="table"/>.TableName задает вид удаляемых документов.
    /// </summary>
    /// <param name="table">Таблица, в которой есть столбец "Id", содержащий идентификаторы удаляемых документов</param>
    /// <returns>Количество удаленных документов</returns>
    public int Remove(DataTable table)
    {
      if (table == null)
        return 0;
      return Remove(table.TableName, table);
    }

    /// <summary>
    /// Удаляет из текущей выборки все ссылки, которых нет в выборке <paramref name="source"/>.
    /// Если <paramref name="source"/> - пустая выборка, то будут удалены все документы.
    /// Таким образом, текущая выборка становится пересечением текущей выборки и <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Выборка для сравнения (не изменяется)</param>
    /// <returns>Число удаленных ссылок</returns>
    public int RemoveNeg(DBxDocSelection source)
    {
      int cnt = 0;
      if (source == null)
      {
        cnt = TotalCount;
        Clear();
        return cnt;
      }
      if (source.IsEmpty)
      {
        cnt = TotalCount;
        Clear();
        return cnt;
      }

      for (int i = _Data.Tables.Count - 1; i >= 0; i--)
      {
        DataTable ResTable = _Data.Tables[i];
        DataTable SrcTable = source.ReadyTable(ResTable.TableName, false);
        if (SrcTable == null)
        {
          cnt += ResTable.Rows.Count;
          _Data.Tables.RemoveAt(i);
          _TableNames = null;
          continue;
        }

        for (int j = ResTable.Rows.Count - 1; j >= 0; j--)
        {
          DataRow ResRow = ResTable.Rows[j];
          Int32 Id = (Int32)(ResRow["Id"]);
          if (SrcTable.Rows.Find(Id) == null)
          {
            ResTable.Rows.RemoveAt(j);
            cnt++;
          }
        }
      }

      return cnt;
    }

    /// <summary>
    /// Очистка всей выборки
    /// </summary>
    public void Clear()
    {
      _Data.Tables.Clear();
      _TableNames = null;
    }

    #endregion

    #region Поиск

    /// <summary>
    /// Получить первый идентификатор заданного документа или 0, если выборка не
    /// содержит таких документов
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <returns>Идентификатор первого документа таблицы или 0</returns>
    public Int32 GetSingleId(string tableName)
    {
      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return 0;
      else
      {
        if (_Data.Tables[p].Rows.Count == 0)
          return 0;
        else
          return DataTools.GetInt(_Data.Tables[p].Rows[0], "Id");
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть заданный документ
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="id">Идентификатор документа для поиска</param>
    /// <returns>true, если идентификатор есть в выборке</returns>
    public bool Contains(string tableName, Int32 id)
    {
      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
        return _Data.Tables[p].Rows.Find(id) != null;
    }

    /// <summary>
    /// Возвращает true, если в выборке есть хотя бы один документ из списка.
    /// Если список идентификаторов пуст, возвращает false.
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для поиска</param>
    /// <returns>true, если документ найден</returns>
    public bool ContainsAny(string tableName, Int32[] ids)
    {
      if (ids == null)
        return false; // 19.01.2021
      if (ids.Length == 0)
        return false; // 19.01.2021

      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
      {
        for (int i = 0; i < ids.Length; i++)
        {
          if (_Data.Tables[p].Rows.Find(ids[i]) != null)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть хотя бы один документ из списка.
    /// Если список идентификаторов пуст, возвращает false.
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для поиска</param>
    /// <returns>true, если документ найден</returns>
    public bool ContainsAny(string tableName, IdList ids)
    {
      if (ids == null)
        return false; // 19.01.2021
      if (ids.Count == 0)
        return false; // 19.01.2021

      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
      {
        foreach (Int32 Id in ids)
        {
          if (_Data.Tables[p].Rows.Find(Id) != null)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть хотя бы один документ из списка.
    /// Если выборка <paramref name="source"/> - пустая, возвращается false.
    /// </summary>
    /// <param name="source">Выборка документов, которые надо найти</param>
    /// <returns>true, если документ найден</returns>
    public bool ContainsAny(DBxDocSelection source)
    {
      if (source == null)
        return true; // 19.01.2021
      if (source.IsEmpty)
        return true; // 19.01.2021

      foreach (DataTable SrcTable in source._Data.Tables)
      {
        DataTable ResTable = ReadyTable(SrcTable.TableName, false);
        if (ResTable == null)
          continue;
        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          // Можно не проверять удаленные строки
          Int32 Id = (Int32)(SrcRow["Id"]);
          if (ResTable.Rows.Find(Id) != null)
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в выборке есть все документы из списка.
    /// Если список идентификаторов пустой, возвращается true.
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для поиска</param>
    /// <returns>true, если документы найдены</returns>
    public bool ContainsAll(string tableName, Int32[] ids)
    {
      if (ids == null)
        return true;
      if (ids.Length == 0)
        return true;

      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
      {
        for (int i = 0; i < ids.Length; i++)
        {
          if (_Data.Tables[p].Rows.Find(ids[i]) == null)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть все документы из списка.
    /// Если список идентификаторов пустой, возвращается true.
    /// </summary>
    /// <param name="tableName">Имя таблицы документов</param>
    /// <param name="ids">Идентификаторы документов для поиска</param>
    /// <returns>true, если документы найдены</returns>
    public bool ContainsAll(string tableName, IdList ids)
    {
      if (ids == null)
        return true;
      if (ids.Count == 0)
        return true;

      int p = _Data.Tables.IndexOf(tableName);
      if (p < 0)
        return false;
      else
      {
        foreach (Int32 Id in ids)
        {
          if (_Data.Tables[p].Rows.Find(Id) == null)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если в выборке есть все документы из другой выборки.
    /// Если выборка <paramref name="source"/> пустая, возвращается true.
    /// </summary>
    /// <param name="source">Выборка документов, которые надо найти</param>
    /// <returns>true, если документы найдены</returns>
    public bool ContainsAll(DBxDocSelection source)
    {
      if (source == null)
        return true;
      if (source.IsEmpty)
        return true;

      foreach (DataTable SrcTable in source._Data.Tables)
      {
        if (SrcTable.Rows.Count == 0)
          continue;

        DataTable ResTable = ReadyTable(SrcTable.TableName, false);
        if (ResTable == null)
          return false;

        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          // Можно не проверять удаленные строки
          Int32 Id = (Int32)(SrcRow["Id"]);
          if (ResTable.Rows.Find(Id) == null)
            return false;
        }
      }
      return true;
    }

    #endregion

    #region Сериализация

    [OnSerializing()]
    internal void OnSerializingMethod(StreamingContext context)
    {
      // Перед сериализацией сохраняем все изменения
      _Data.AcceptChanges();
    }

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Запись в секцию конфигурацию
    /// В переданном узле создается полдузел "Tables" с вложенными подузлами, соответствующими
    /// именам таблиц в списке TableNames. Для каждого подузла создается строковый параметр "Ids".
    /// В строку записываются идентификаторы документов, разделенные запятыми.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (IsEmpty)
        cfg.Remove("Tables");
      else
      {
        CfgPart Part2 = cfg.GetChild("Tables", true);
        Part2.Clear();
        for (int i = 0; i < TableNames.Length; i++)
        {
          Int32[] Ids = this[TableNames[i]];
          if (Ids.Length == 0)
            continue;
          CfgPart Part3 = Part2.GetChild(TableNames[i], true);
          string s = DataTools.CommaStringFromIds(Ids, false);
          Part3.SetString("Ids", s);
        }
      }
    }

    /// <summary>
    /// Чтение выборки, которая была записана вызовом WriteConfig()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      Clear();
      CfgPart Part2 = cfg.GetChild("Tables", false);
      if (Part2 == null)
        return;
      string[] TableNames = Part2.GetChildNames();
      for (int i = 0; i < TableNames.Length; i++)
      {
        CfgPart Part3 = Part2.GetChild(TableNames[i], false);
        if (Part3 == null)
          continue; // ошибка
        string s = Part3.GetString("Ids");
        Int32[] Ids = DataTools.CommaStringToIds(s);
        Add(TableNames[i], Ids);
      }
    }

    #endregion

    // TODO: Нет доступа к сохраненным документам выборки

#if XXXX

    #region Нормализация

    /// <summary>
    /// Возвращает true, если выборка содержит ссылки на другие выборки и
    /// нуждается в нормализации
    /// </summary>
    public bool NormalizeNeeded
    {
      get { return GetCount("Выборки") > 0; }
    }

    /// <summary>
    /// Нормализация выборки.
    /// Заменяет ссылки на другие выборки их содержимым
    /// Возвращает true, если нормализация выполнена
    /// </summary>
    /// <param name="BufTables">Доступ к буферизации таблиц</param>
    /// <returns>true, если нормализация выполнена</returns>
    public bool Normalize(BufTables BufTables)
    {
      Int32[] Ids = this["Выборки"];
      if (Ids.Length == 0)
        return false;
      List<Int32> ReadyIds = new List<Int32>();
      for (int i = 0; i < Ids.Length; i++)
        NormalizeOneDocSel(Ids[i], ReadyIds, BufTables);
      Remove("Выборки");
      return true;
    }

    /// <summary>
    /// Рекурсивная процедура нормализации одной выборки
    /// </summary>
    /// <param name="DocSelId">Идентификатор текущей выборки</param>
    /// <param name="ReadyIds">Список уже обработанных выборок</param>
    /// <param name="BufTables">Доступ к данным</param>
    private void NormalizeOneDocSel(Int32 DocSelId, List<Int32> ReadyIds, BufTables BufTables)
    {
      if (ReadyIds.Contains(DocSelId))
        return; // уже обработана

      // Предотвращаем повторную обработку
      ReadyIds.Add(DocSelId);

      // Загружаем идентификаторы выборки
      string XmlText = DataTools.GetString(BufTables.GetValue("Выборки", DocSelId, "Данные"));
      ConfigSection Sect = new ConfigSection();
      Sect.AsXmlText = XmlText;
      DBxDocSelection DocSel2 = new DBxDocSelection(DBIdentity);
      DocSel2.ReadConfig(Sect);

      // Добавляем ссылки из другой выборки
      for (int i = 0; i < DocSel2.TableNames.Length; i++)
      {
        Int32[] Ids2 = DocSel2[DocSel2.TableNames[i]];
        if (DocSel2.TableNames[i] == "Выборки")
        {
          // Рекурсивный вызов
          for (int j = 0; j < Ids2.Length; j++)
            NormalizeOneDocSel(Ids2[j], ReadyIds, BufTables);
        }
        else
          // Обычное добавление
          Add(DocSel2.TableNames[i], Ids2);
      }
    }

    #endregion

#endif

    #region Текстовое представление

    /// <summary>
    /// Для отладки.
    /// Для вывода содержимого выборки пользователю используйте перегрузку метода с аргументом DBxDocTextHandlers.
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      //sb.Append("TotalCount=");
      //sb.Append(TotalCount);
      for (int i = 0; i < _Data.Tables.Count; i++)
      {
        if (i > 0)
          sb.Append(", ");
        else
          sb.Append(": ");

        sb.Append(_Data.Tables[i].TableName);
        sb.Append(" (");
        sb.Append(_Data.Tables[i].Rows.Count);
        sb.Append(")");
      }
      return sb.ToString();
    }

    /// <summary>
    /// Улучшенный метод получения текстового представления для выборки документов
    /// </summary>
    /// <param name="textHandlers">Обработчики текстового представления документов</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(DBxDocTextHandlers textHandlers)
    {
#if DEBUG
      if (textHandlers == null)
        throw new ArgumentNullException("textHandlers");
#endif

      if (IsEmpty)
        return "Нет документов";
      if (TotalCount == 1)
      {
        DBxDocType DocType = textHandlers.DocTypes[_Data.Tables[0].TableName];
        string sDTN = DocType.SingularTitle;
        Int32 DocId = this[DocType.Name][0];
        return sDTN + " " + textHandlers.GetTextValue(DocType.Name, DocId);
      }

      StringBuilder sb = new StringBuilder();
      //sb.Append("TotalCount=");
      //sb.Append(TotalCount);
      for (int i = 0; i < _Data.Tables.Count; i++)
      {
        if (i > 0)
          sb.Append(", ");
        else
          sb.Append(": ");

        DBxDocType DocType = textHandlers.DocTypes[_Data.Tables[i].TableName];
        int n = _Data.Tables[i].Rows.Count;
        if (n == 1)
          sb.Append(DocType.SingularTitle);
        else
          sb.Append(DocType.PluralTitle);

        sb.Append(" (");
        sb.Append(n);
        sb.Append(")");
      }
      return sb.ToString();
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию выборки
    /// </summary>
    /// <returns></returns>
    public DBxDocSelection Clone()
    {
      return new DBxDocSelection(this);
    }

    object ICloneable.Clone()
    {
      return new DBxDocSelection(this);
    }

    #endregion
  }
}
