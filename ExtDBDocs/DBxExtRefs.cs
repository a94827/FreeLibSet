using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

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
  /// Хранение описаний внешних ссылок на строки документов и поддокументов
  /// Используется при проверке возможности удаления. Чтобы не перебирать
  /// описания всех таблиц, используются списки, содержащие возможные ссылочные
  /// поля и переменные ссылки, допускающие хранение документов данного вида
  /// Экземпляр класса содержит коллекцию списков для всех документов / поддокументов и создается
  /// DBxRealDocProvider при первом выполнении удаления
  /// </summary>
  internal class DBxExtRefs
  {
    #region Вложенные классы и структуры

    /// <summary>
    /// Описание одного поля
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct RefColumnInfo
    {
      #region Конструктор

      public RefColumnInfo(DBxDocType detailsDocType, DBxSubDocType detailsSubDocType, DBxColumnStruct columnDef)
      {
#if DEBUG
        if (columnDef.ColumnType != DBxColumnType.Int)
          throw new ArgumentException("Поле должно иметь ссылочный тип", "columnDef");
        if (detailsDocType == null)
          throw new ArgumentNullException("detailsDocType");
        if (detailsSubDocType != null)
        {
          if (detailsSubDocType.DocType != detailsDocType)
            throw new ArgumentException("Чужой поддокумент", "detailsSubDocType");
        }
#endif

        _DetailsDocType = detailsDocType;
        _DetailsSubDocType = detailsSubDocType;
        _ColumnDef = columnDef;
      }

      #endregion

      #region Свойства

      public DBxDocType DetailsDocType { get { return _DetailsDocType; } }
      private DBxDocType _DetailsDocType;

      public DBxSubDocType DetailsSubDocType { get { return _DetailsSubDocType; } }
      private DBxSubDocType _DetailsSubDocType;

      public DBxDocTypeBase DetailsType
      {
        get
        {
          if (DetailsSubDocType == null)
            return DetailsDocType;
          else
            return DetailsSubDocType;
        }
      }

      public bool IsSubDocType { get { return DetailsSubDocType != null; } }

      public string DetailsTableName
      {
        get
        {
          return DetailsSubDocType == null ? DetailsDocType.Name : DetailsSubDocType.Name;
        }
      }

      public string DetailsSingularDescription
      {
        get
        {
          string s = "Документ \"" + DetailsDocType.SingularTitle + "\"";
          if (DetailsSubDocType != null)
            s += ", поддокумент \"" + DetailsSubDocType.SingularTitle + "\"";
          return s;
        }
      }

      public string DetailsPluralDescription
      {
        get
        {
          string s = "Документы \"" + DetailsDocType.PluralTitle + "\"";
          if (DetailsSubDocType != null)
            s += ", поддокументы \"" + DetailsSubDocType.PluralTitle + "\"";
          return s;
        }
      }

      public DBxColumnStruct ColumnDef { get { return _ColumnDef; } }
      private DBxColumnStruct _ColumnDef;

      #endregion
    }

    /// <summary>
    /// Описание одной переменной ссылки
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct VTRefInfo
    {
      #region Конструктор

      public VTRefInfo(DBxDocType detailsDocType, DBxSubDocType detailsSubDocType, DBxVTReference vtRefDef)
      {
#if DEBUG
        if (detailsDocType == null)
          throw new ArgumentNullException("detailsDocType");
        if (detailsSubDocType != null)
        {
          if (detailsSubDocType.DocType != detailsDocType)
            throw new ArgumentException("Чужой поддокумент", "detailsSubDocType");
        }
#endif

        _DetailsDocType = detailsDocType;
        _DetailsSubDocType = detailsSubDocType;
        _VTRef = vtRefDef;
      }

      #endregion

      #region Свойства

      public DBxDocType DetailsDocType { get { return _DetailsDocType; } }
      private DBxDocType _DetailsDocType;

      public DBxSubDocType DetailsSubDocType { get { return _DetailsSubDocType; } }
      private DBxSubDocType _DetailsSubDocType;

      public DBxDocTypeBase DetailsType
      {
        get
        {
          if (DetailsSubDocType == null)
            return DetailsDocType;
          else
            return DetailsSubDocType;
        }
      }

      public bool IsSubDocType { get { return DetailsSubDocType != null; } }

      public string DetailsTableName
      {
        get
        {
          return DetailsSubDocType == null ? DetailsDocType.Name : DetailsSubDocType.Name;
        }
      }

      public string DetailsSingularDescription
      {
        get
        {
          string s = "Документ \"" + DetailsDocType.SingularTitle + "\"";
          if (DetailsSubDocType != null)
            s += ", поддокумент \"" + DetailsSubDocType.SingularTitle + "\"";
          return s;
        }
      }

      public string DetailsPluralDescription
      {
        get
        {
          string s = "Документы \"" + DetailsDocType.PluralTitle + "\"";
          if (DetailsSubDocType != null)
            s += ", поддокументы \"" + DetailsSubDocType.PluralTitle + "\"";
          return s;
        }
      }

      public DBxVTReference VTRef { get { return _VTRef; } }
      private DBxVTReference _VTRef;

      #endregion
    }

    /// <summary>
    /// Список для одного документа или поддокумента
    /// </summary>
    public class TableRefList
    {
      #region Конструктор

      public TableRefList()
      {
        _RefColumns = new List<RefColumnInfo>();
        _VTRefs = new List<VTRefInfo>();
      }

      #endregion

      #region Списки

      /// <summary>
      /// Список ссылочных полей
      /// </summary>
      public List<RefColumnInfo> RefColumns { get { return _RefColumns; } }
      private List<RefColumnInfo> _RefColumns;

      /// <summary>
      /// Список переменных ссылок на документы
      /// </summary>
      public List<VTRefInfo> VTRefs { get { return _VTRefs; } }
      private List<VTRefInfo> _VTRefs;

      /// <summary>
      /// Возвращает true, если в документе или поддокументе нет ссылочных полей
      /// </summary>
      public bool IsEmpty
      {
        get
        {
          return RefColumns.Count == 0 && VTRefs.Count == 0;
        }
      }

      /// <summary>
      /// Для отладки
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return "Ref column count=" + _RefColumns.Count.ToString() + ", VTReference count=" + _VTRefs.Count.ToString();
      }

      #endregion
    }

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает заполненый список
    /// </summary>
    /// <param name="dts">Описание видов документов и поддокументов</param>
    /// <param name="binDataHandler">Обработчик двоичных данных. Может быть null</param>
    public DBxExtRefs(DBxDocTypes dts, DBxBinDataHandler binDataHandler)
    {
      _Items = new Dictionary<string, TableRefList>();
      // Перебираем описания
      for (int i = 0; i < dts.Count; i++)
      {
        AddRefs(dts, binDataHandler, dts[i], null);
        for (int j = 0; j < dts[i].SubDocs.Count; j++)
          AddRefs(dts, binDataHandler, dts[i], dts[i].SubDocs[j]);
      }
    }

    private void AddRefs(DBxDocTypes dts, DBxBinDataHandler binDataHandler, DBxDocType detailDocType, DBxSubDocType detailSubDocType)
    {
      DBxDocTypeBase DetailBase;
      if (detailSubDocType == null)
        DetailBase = detailDocType;
      else
        DetailBase = detailSubDocType;

      // 1. Ссылочные поля
      for (int i = 0; i < DetailBase.Struct.Columns.Count; i++)
      {
        DBxColumnStruct ColumnDef = DetailBase.Struct.Columns[i];
        if (!String.IsNullOrEmpty(ColumnDef.MasterTableName))
        {
          switch (ColumnDef.MasterTableName)
          {
            case "DocTables":
              continue;

            case "BinData":
            case "FileNames":
              if (binDataHandler != null)
                continue; // 20.07.2021
              break;
          }
          DBxDocType dt1;
          DBxSubDocType sdt1;
          if (!(ColumnDef.MasterTableName == "BinData" || ColumnDef.MasterTableName == "FileNames"))
          {
            if (!dts.FindByTableName(ColumnDef.MasterTableName, out dt1, out sdt1))
              throw new BugException("Объявление таблицы \"" + DetailBase.Name +
                "\" содержит описание ссылочного поля \"" + ColumnDef.ColumnName +
                "\", которое ссылается на неизвестную таблицу \"" + ColumnDef.MasterTableName + "\"");
          }

          TableRefList List = InternalGetList(ColumnDef.MasterTableName);

          List.RefColumns.Add(new RefColumnInfo(detailDocType, detailSubDocType, ColumnDef));
        }
      }

      // 2. Переменные ссылки
      for (int i = 0; i < DetailBase.VTRefs.Count; i++)
      {
        DBxVTReference VTRef = DetailBase.VTRefs[i];
        for (int j = 0; j < VTRef.MasterTableNames.Length; j++)
        {
          DBxDocType dt1;
          DBxSubDocType sdt1;
          if (!dts.FindByTableName(VTRef.MasterTableNames[j], out dt1, out sdt1))
            throw new BugException("Объявление таблицы \"" + DetailBase.Name +
              "\" содержит описание полей переменной ссылки \"" + VTRef.Name +
              "\", которое ссылается на неизвестнкю таблицу \"" + VTRef.MasterTableNames[j] + "\"");


          TableRefList List = InternalGetList(VTRef.MasterTableNames[j]);
          List.VTRefs.Add(new VTRefInfo(detailDocType, detailSubDocType, VTRef));
        }
      }

    }

    #endregion

    #region Доступ к спискам по имени таблицы

    /// <summary>
    /// Возвращает списки ссылок по имени таблицы документа или поддокумента.
    /// Возвращает пустой список, если в таблице нет ссылочных полей.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Список ссылок</returns>
    public TableRefList this[string tableName]
    {
      get
      {
        TableRefList Res;
        if (_Items.TryGetValue(tableName, out Res))
          return Res;
        else
          return EmptyRefList;
      }
    }

    /// <summary>
    /// Этот список может содержать не все виды документов и поддокументов
    /// </summary>
    private Dictionary<string, TableRefList> _Items;

    private static readonly TableRefList EmptyRefList = new TableRefList();

    private TableRefList InternalGetList(string tableName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
#endif

      TableRefList Res;
      if (!_Items.TryGetValue(tableName, out Res))
      {
        Res = new TableRefList();
        _Items.Add(tableName, Res);
      }
      return Res;
    }

    #endregion
  }

}
