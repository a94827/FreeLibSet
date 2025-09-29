// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
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
  /// Описание переменной ссылки.
  /// Переменная ссылка - это два числовых поля, первое из которых задает идентификатор таблицы документа, 
  /// а второе - идентификатор документа.
  /// Числовые поля сами по себе не являются ссылочными. СУБД не поддерживают ссылки на разные таблицы из одного поля.
  /// Функционал переменной ссылки, включая обеспечение целостности, реализуется на уровне <see cref="DBxRealDocProvider"/>.
  /// </summary>
  [Serializable]
  public sealed class DBxVTReference:ObjectWithCode, IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает описание ссылки.
    /// В структуре таблицы <paramref name="table"/> должны быть созданы описания двух числовых полей <paramref name="tableIdColumn"/> и <paramref name="docIdColumn"/>.
    /// </summary>
    /// <param name="vtName">Условное имя ссылки в списке</param>
    /// <param name="table">Описание структуры таблицы, в которой находятся ссылочные поля</param>
    /// <param name="tableIdColumn">Описание числового поля, который содержит идентификатор таблицы</param>
    /// <param name="docIdColumn">Описание числового поля, который содержит идентификатор документа</param>
    public DBxVTReference(string vtName, DBxTableStruct table, DBxColumnStruct tableIdColumn, DBxColumnStruct docIdColumn)
      :base(vtName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (tableIdColumn == null)
        throw new ArgumentNullException("tableIdColumn");
      if (docIdColumn == null)
        throw new ArgumentNullException("docIdColumn");
#endif
      if (!table.Columns.Contains(tableIdColumn))
        throw new ArgumentException(String.Format(Res.DBxVTReference_Arg_NoColumn, table.TableName, tableIdColumn.ColumnName), "tableIdColumn");
      if (!table.Columns.Contains(docIdColumn))
        throw new ArgumentException(String.Format(Res.DBxVTReference_Arg_NoColumn, table.TableName, docIdColumn.ColumnName), "docIdColumn");
      if (Object.ReferenceEquals(tableIdColumn, docIdColumn))
        throw ExceptionFactory.ArgAreSame("tableIdColumn", "docIdColumn");

      if (tableIdColumn.ColumnType != DBxColumnType.Int32)
        throw new ArgumentException(String.Format(Res.DBxVTReference_Arg_ColumnNoInt, tableIdColumn.ColumnName), "tableIdColumn");
      if (docIdColumn.ColumnType != DBxColumnType.Int32)
        throw new ArgumentException(String.Format(Res.DBxVTReference_Arg_ColumnNoInt, docIdColumn.ColumnName), "docIdColumn");
      if (tableIdColumn.Nullable != docIdColumn.Nullable)
        throw new ArgumentException(Res.DBxVTReference_Arg_NullableDiff);
      if (!String.IsNullOrEmpty(tableIdColumn.MasterTableName))
        throw new ArgumentException(String.Format(Res.DBxVTReference_Arg_ColumnIsRef, tableIdColumn.ColumnName), "tableIdColumn");
      if (!String.IsNullOrEmpty(docIdColumn.MasterTableName))
        throw new ArgumentException(String.Format(Res.DBxVTReference_Arg_ColumnIsRef, docIdColumn.ColumnName), "docIdColumn");

      _Table = table;
      _TableIdColumn = tableIdColumn;
      _DocIdColumn = docIdColumn;
      _MasterTableNames=new MasterTableNameList();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Условное имя ссылки
    /// </summary>
    public string Name { get { return base.Code; } }

    /// <summary>
    /// Таблица, в которой объявлена переменная ссылка
    /// </summary>
    public DBxTableStruct Table { get { return _Table; } }
    private readonly DBxTableStruct _Table;

    /// <summary>
    /// Числовое поле, содержащее идентификатор таблицы
    /// </summary>
    public DBxColumnStruct TableIdColumn { get { return _TableIdColumn; } }
    private readonly DBxColumnStruct _TableIdColumn;

    /// <summary>
    /// Числовое поле, содержащее идентификатор строки в таблице, на которую
    /// выполняется ссылка
    /// </summary>
    public DBxColumnStruct DocIdColumn { get { return _DocIdColumn; } }
    private readonly DBxColumnStruct _DocIdColumn;

    [Serializable]
    private class MasterTableNameList : SingleScopeList<string>
    {
      internal new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    /// <summary>
    /// Имена таблиц документов, на которые возможна ссылка.
    /// Добавлять имена можно только до инициализации базы данных.
    /// Список основан на SingleScopeList. Повторное добавление имени таблицы отбрасывается.
    /// Поддерживаются только ссылки на документы, а не поддокументы.
    /// Если список таблиц пустой, то поля добавляются в базу данных, но их нельзя будет использовать. Они всегда будут содержать значения NULL.
    /// </summary>
    public IList<string> MasterTableNames { get { return _MasterTableNames; } }
    private readonly MasterTableNameList _MasterTableNames;

    /// <summary>
    /// Идентификаторы таблиц, на которые возможна ссылка, то есть список возможных
    /// значений для поля TableColumn. Свойство становится доступно
    /// после инициализации БД
    /// </summary>
    internal IIdSet<Int32> MasterTableIds { get { return _MasterTableIds; } /*internal*/ set { _MasterTableIds = value; } }
    private IIdSet<Int32> _MasterTableIds;

    #endregion

    #region IReadOnlyObject

    bool IReadOnlyObject.IsReadOnly { get { return _MasterTableNames.IsReadOnly; } }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      if (_MasterTableNames.IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      _MasterTableNames.SetReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства <see cref="DBxDocTypeBase.VTRefs"/>
  /// </summary>
  [Serializable]
  public sealed class DBxVTReferenceList : NamedList<DBxVTReference>
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
    private readonly DBxDocTypeBase _Owner;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить ссылку. В структуре таблицы создается два числовых поля с именами <paramref name="vtName"/>TableId и <paramref name="vtName"/>DocId.
    /// </summary>
    /// <param name="vtName">Имя ссылки</param>
    /// <returns>Созданный объект ссылки</returns>
    public DBxVTReference Add(string vtName)
    {
      return Add(vtName, vtName + "TableId", vtName + "DocId");
    }

    /// <summary>
    /// Добавить ссылку. В структуре таблицы создается два числовых поля с именами.
    /// </summary>
    /// <param name="vtName">Имя ссылки</param>
    /// <param name="tableIdColumnName">Имя поля, содержащего идентификатор таблицы</param>
    /// <param name="docIdColumnName">Имя поля, содержащего идентификатор</param>
    /// <returns>Созданный объект ссылки</returns>
    public DBxVTReference Add(string vtName, string tableIdColumnName, string docIdColumnName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(vtName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("vtName");
      Owner.CheckNotReadOnly();
#endif
      DBxColumnStruct tableIdColumn = Owner.Struct.Columns.AddInt32(tableIdColumnName, true); 
      DBxColumnStruct idColumn = Owner.Struct.Columns.AddInt32(docIdColumnName, true);
      DBxVTReference item = new DBxVTReference(vtName, Owner.Struct, tableIdColumn, idColumn);
      base.Add(item);
      return item;
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
      for (int i = 0; i < Count; i++)
        this[i].SetReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Простая структура, содержащая идентификатор таблицы документа и идентификатор документа.
  /// Удобно использовать в качестве свойства <see cref="ErrorMessageItem.Tag"/> для перехода к ошибочному документу.
  /// </summary>
  [Serializable]
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
