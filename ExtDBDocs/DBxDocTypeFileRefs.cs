// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#define USE_REFS // Ссылки или числовые поля

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Data.Docs
{
  // TODO: Может быть классы коллекций стоит сделать внутренними для DBxDocTypeBase

  /// <summary>
  /// Описание одного поля, хранящего ссылку на файл.
  /// </summary>
  [Serializable]
  public sealed class DBxDocTypeFileRef:IObjectWithCode
  {
    #region Конструктор

    internal DBxDocTypeFileRef(DBxColumnStruct column)
    {
      _Column = column;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описание структуры столбца числового столбца, хранящего идентификаторы во внутренней таблице
    /// </summary>
    public DBxColumnStruct Column { get { return _Column; } }
    private readonly DBxColumnStruct _Column;

    /// <summary>
    /// Возвращает Column.ColumnName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Column.ColumnName;
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code
    {
      get { return Column.ColumnName; }
    }

    #endregion
  }

  /// <summary>
  /// Описание ссылок на файлы, хранящиеся в базе данных
  /// Реализация свойства
  /// </summary>
  [Serializable]
  public sealed class DBxDocTypeFileRefs : NamedList<DBxDocTypeFileRef>
  {
    #region Конструктор

    internal DBxDocTypeFileRefs(DBxTableStruct tableStruct)
    {
      _TableStruct = tableStruct;
    }

    #endregion

    #region Свойства

    private readonly DBxTableStruct _TableStruct;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("TableName=");
      sb.Append(_TableStruct.TableName);
      sb.Append(", Count=");
      sb.Append(Count.ToString());
      if (IsReadOnly)
        sb.Append(" (ReadOnly)");
      return sb.ToString();
    }

    #endregion

    #region Добавление файловой ссылки

    /// <summary>
    /// Добавляет файловую ссылку. 
    /// При этом в описание структуры таблицы документа или поддокумента добавляется ссылочный
    /// столбец для хранения идентификаторов
    /// </summary>
    /// <param name="columnName">Имя добавляемого столбца</param>
    /// <returns>Описание новой ссылки</returns>
    public DBxDocTypeFileRef Add(string columnName)
    {
#if USE_REFS
      // 19.07.2021. Добавляем ссылку, а не числовое поле
      DBxColumnStruct column = _TableStruct.Columns.AddReference(columnName, "FileNames", true);
      column.ColumnType = DBxColumnType.Int32;
#else
      DBxColumnStruct column = _TableStruct.Columns.AddInt32(columnName);
#endif
      DBxDocTypeFileRef fileRef = new DBxDocTypeFileRef(column);
      base.Add(fileRef);
      return fileRef;
    }

    #endregion

    #region SetReadOnly

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Описание одного поля, хранящего двоичные данные в виде ссылки на таблицу "BinData"
  /// </summary>
  [Serializable]
  public sealed class DBxDocTypeBinDataRef : IObjectWithCode
  {
    #region Конструктор

    internal DBxDocTypeBinDataRef(DBxColumnStruct column)
    {
      _Column = column;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описание структуры столбца числового столбца, хранящего идентификаторы во внутренней таблице
    /// </summary>
    public DBxColumnStruct Column { get { return _Column; } }
    private DBxColumnStruct _Column;

    /// <summary>
    /// Возвращает Column.ColumnName
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return _Column.ColumnName;
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code
    {
      get { return Column.ColumnName; }
    }

    #endregion
  }

  /// <summary>
  /// Описание ссылок на файлы, хранящиеся в базе данных
  /// Реализация свойства DBxDocTypeBase.BinDataRefs
  /// </summary>
  [Serializable]
  public class DBxDocTypeBinDataRefs : NamedList<DBxDocTypeBinDataRef>
  {
    #region Конструктор

    internal DBxDocTypeBinDataRefs(DBxTableStruct tableStruct)
    {
      _TableStruct = tableStruct;
    }

    #endregion

    #region Свойства

    private DBxTableStruct _TableStruct;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("TableName=");
      sb.Append(_TableStruct.TableName);
      sb.Append(", Count=");
      sb.Append(Count.ToString());
      if (IsReadOnly)
        sb.Append(" (ReadOnly)");
      return sb.ToString();
    }

    #endregion

    #region Добавление ссылки

    /// <summary>
    /// Добавляет ссылку на двоичные данные.
    /// При этом в структуру таблицы документа или поддокумента добавляется числовое поле,
    /// хранящее идентификаторы двоичных данных
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Объект новой ссылки</returns>
    public DBxDocTypeBinDataRef Add(string columnName)
    {
#if USE_REFS
      // 19.07.2021. Добавляем ссылку, а не числовое поле
      DBxColumnStruct column = _TableStruct.Columns.AddReference(columnName, "BinData", true);
      column.ColumnType = DBxColumnType.Int32;
#else
      DBxColumnStruct column = _TableStruct.Columns.AddInt32(columnName);
#endif
      DBxDocTypeBinDataRef fileRef = new DBxDocTypeBinDataRef(column);
      base.Add(fileRef);
      return fileRef;
    }

    #endregion

    #region SetReadOnly

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion
  }
}
