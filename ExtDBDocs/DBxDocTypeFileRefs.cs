#define USE_REFS // Ссылки или числовые поля

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

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

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Описание одного поля, хранящего ссылку на файл.
  /// </summary>
  [Serializable]
  public class DBxDocTypeFileRef:IObjectWithCode
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
    private DBxColumnStruct _Column;

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
  public class DBxDocTypeFileRefs : NamedList<DBxDocTypeFileRef>
  {
    #region Конструктор

    internal DBxDocTypeFileRefs(DBxTableStruct tableStruct)
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
      DBxColumnStruct Column = _TableStruct.Columns.AddReference(columnName, "FileNames", true);
#else
      DBxColumnStruct Column = _TableStruct.Columns.AddInt(columnName);
#endif
      DBxDocTypeFileRef Ref = new DBxDocTypeFileRef(Column);
      base.Add(Ref);
      return Ref;
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
  /// Описание одного поля, хранящего двоичные данные в виде ссылки на таблицу BinData
  /// </summary>
  [Serializable]
  public class DBxDocTypeBinDataRef : IObjectWithCode
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
    /// Добавляет ссылку на двоичные даннные.
    /// При этом в структуру таблицы документа или поддокумента добавляется числовое поле,
    /// хранящее идентификаторы двоичных данных
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Объект новой ссылки</returns>
    public DBxDocTypeBinDataRef Add(string columnName)
    {
#if USE_REFS
      // 19.07.2021. Добавляем ссылку, а не числовое поле
      DBxColumnStruct Column = _TableStruct.Columns.AddReference(columnName, "BinData", true);
#else
      DBxColumnStruct Column = _TableStruct.Columns.AddInt(columnName);
#endif
      DBxDocTypeBinDataRef Ref = new DBxDocTypeBinDataRef(Column);
      base.Add(Ref);
      return Ref;
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
