// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Хранение описаний внешних ссылок на строки документов и поддокументов.
  /// Используется при проверке возможности удаления. Чтобы не перебирать
  /// описания всех таблиц, используются списки, содержащие возможные ссылочные
  /// поля и переменные ссылки, допускающие хранение документов данного вида.
  /// Экземпляр класса содержит коллекцию списков для всех документов/поддокументов в <see cref="DBxDocTypes"/>,
  /// включая те, к которым у пользователя может не быть доступа.
  /// Создается в конструкторе <see cref="DBxRealDocProviderGlobal"/>.
  /// </summary>
  internal class DBxExtRefs
  {
    #region Вложенные классы и структуры

    /// <summary>
    /// Описание одного обычного ссылочного поля
    /// </summary>
    public struct RefColumnInfo
    {
      #region Конструктор

      public RefColumnInfo(DBxDocType detailsDocType, DBxSubDocType detailsSubDocType, DBxColumnStruct columnDef)
      {
#if DEBUG
        if (columnDef.ColumnType != DBxColumnType.Int32)
          throw ExceptionFactory.ArgProperty("columnDef", columnDef, "ColumnType", columnDef.ColumnType, new object[] { DBxColumnType.Int32 });
        if (detailsDocType == null)
          throw new ArgumentNullException("detailsDocType");
        if (detailsSubDocType != null)
        {
          if (detailsSubDocType.DocType != detailsDocType)
            throw ExceptionFactory.ArgProperty("detailsSubDocType", detailsSubDocType, "DocType", detailsSubDocType.DocType,
              new object[] { detailsDocType });
        }
#endif

        _DetailsDocType = detailsDocType;
        _DetailsSubDocType = detailsSubDocType;
        _ColumnDef = columnDef;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Описание вида документа, в котором расположено ссылочное под\ле
      /// </summary>
      public DBxDocType DetailsDocType { get { return _DetailsDocType; } }
      private readonly DBxDocType _DetailsDocType;

      /// <summary>
      /// Описание вида поддокумента, в котором расположено ссылочное поле, или null
      /// </summary>
      public DBxSubDocType DetailsSubDocType { get { return _DetailsSubDocType; } }
      private readonly DBxSubDocType _DetailsSubDocType;

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

      /// <summary>
      /// Имя таблицы, в которой располагается ссылочное поле
      /// </summary>
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
          if (DetailsSubDocType == null)
            return String.Format(Res.DBxExtRefs_Name_SingularDoc, DetailsDocType.SingularTitle);
          else
            return String.Format(Res.DBxExtRefs_Name_SingularSubDoc, DetailsDocType.SingularTitle, DetailsSubDocType.SingularTitle);
        }
      }

      public string DetailsPluralDescription
      {
        get
        {
          if (DetailsSubDocType == null)
            return String.Format(Res.DBxExtRefs_Name_PluralDoc, DetailsDocType.PluralTitle);
          else
            return String.Format(Res.DBxExtRefs_Name_PluralSubDoc, DetailsDocType.PluralTitle, DetailsSubDocType.PluralTitle);
        }
      }

      /// <summary>
      /// Описание ссылочного столбца
      /// </summary>
      public DBxColumnStruct ColumnDef { get { return _ColumnDef; } }
      private readonly DBxColumnStruct _ColumnDef;

      /// <summary>
      /// Текстовое представление для отладки
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return "Table \"" + DetailsTableName + "\", RefColumn \"" + _ColumnDef.ColumnName + "\" -> " + _ColumnDef.MasterTableName;
      }

      #endregion
    }

    /// <summary>
    /// Описание одной переменной ссылки
    /// </summary>
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
            throw ExceptionFactory.ArgProperty("detailsSubDocType", detailsSubDocType, "DocType", detailsSubDocType.DocType,
              new object[] { detailsDocType });
        }
#endif

        _DetailsDocType = detailsDocType;
        _DetailsSubDocType = detailsSubDocType;
        _VTRef = vtRefDef;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Описание вида документа, в котором объявлена ссылка
      /// </summary>
      public DBxDocType DetailsDocType { get { return _DetailsDocType; } }
      private readonly DBxDocType _DetailsDocType;

      /// <summary>
      /// Описание вида поддокумента, в котором объявлена ссылка, или null.
      /// </summary>
      public DBxSubDocType DetailsSubDocType { get { return _DetailsSubDocType; } }
      private readonly DBxSubDocType _DetailsSubDocType;

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

      /// <summary>
      /// Имя таблицы, которая содержит ссылочные поля
      /// </summary>
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
          if (DetailsSubDocType == null)
            return String.Format(Res.DBxExtRefs_Name_SingularDoc, DetailsDocType.SingularTitle);
          else
            return String.Format(Res.DBxExtRefs_Name_SingularSubDoc, DetailsDocType.SingularTitle, DetailsSubDocType.SingularTitle);
        }
      }

      public string DetailsPluralDescription
      {
        get
        {
          if (DetailsSubDocType == null)
            return String.Format(Res.DBxExtRefs_Name_PluralDoc, DetailsDocType.PluralTitle);
          else
            return String.Format(Res.DBxExtRefs_Name_PluralSubDoc, DetailsDocType.PluralTitle, DetailsSubDocType.PluralTitle);
        }
      }

      /// <summary>
      /// Объявление переменной ссылки
      /// </summary>
      public DBxVTReference VTRef { get { return _VTRef; } }
      private readonly DBxVTReference _VTRef;

      /// <summary>
      /// Текстовое представление для отдадки
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return "Table \"" + DetailsTableName + "\", RefColumns \"" + _VTRef.TableIdColumn.ColumnName + "\"," + _VTRef.DocIdColumn.ColumnName;
      }

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
      /// Список обычных ссылочных полей
      /// </summary>
      public List<RefColumnInfo> RefColumns { get { return _RefColumns; } }
      private readonly List<RefColumnInfo> _RefColumns;

      /// <summary>
      /// Список переменных ссылок на документы
      /// </summary>
      public List<VTRefInfo> VTRefs { get { return _VTRefs; } }
      private readonly List<VTRefInfo> _VTRefs;

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
      _MasterTableDict = new Dictionary<string, TableRefList>();
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
      DBxDocTypeBase detailBase;
      if (detailSubDocType == null)
        detailBase = detailDocType;
      else
        detailBase = detailSubDocType;

      #region 1. Ссылочные поля

      for (int i = 0; i < detailBase.Struct.Columns.Count; i++)
      {
        DBxColumnStruct colDef = detailBase.Struct.Columns[i];
        if (!String.IsNullOrEmpty(colDef.MasterTableName))
        {
          switch (colDef.MasterTableName)
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
          if (!(colDef.MasterTableName == "BinData" || colDef.MasterTableName == "FileNames"))
          {
            if (!dts.FindByTableName(colDef.MasterTableName, out dt1, out sdt1))
              throw new InvalidOperationException(String.Format(Res.DBxExtRefs_Err_RefToUnknownTable,
                detailBase.Name, colDef.ColumnName, colDef.MasterTableName));
          }

          TableRefList list = InternalGetList(colDef.MasterTableName);

          list.RefColumns.Add(new RefColumnInfo(detailDocType, detailSubDocType, colDef));
        }
      }

      #endregion

      #region 2. Переменные ссылки

      for (int i = 0; i < detailBase.VTRefs.Count; i++)
      {
        DBxVTReference vtRef = detailBase.VTRefs[i];
        for (int j = 0; j < vtRef.MasterTableNames.Count; j++)
        {
          DBxDocType dt1;
          DBxSubDocType sdt1;
          dts.GetByTableName(vtRef.MasterTableNames[j], out dt1, out sdt1);
          TableRefList list = InternalGetList(vtRef.MasterTableNames[j]);
          list.VTRefs.Add(new VTRefInfo(detailDocType, detailSubDocType, vtRef));
        }
      }

      #endregion
    }

    #endregion

    #region Доступ к спискам по имени таблицы

    /// <summary>
    /// Возвращает списки ссылок по имени таблицы документа или поддокумента.
    /// Возвращает пустой список, если в таблице нет ссылочных полей.
    /// </summary>
    /// <param name="masterTableName">Имя мастер-таблицы, на которую выполняются ссылки</param>
    /// <returns>Список ссылок</returns>
    public TableRefList this[string masterTableName]
    {
      get
      {
        TableRefList res;
        if (_MasterTableDict.TryGetValue(masterTableName, out res))
          return res;
        else
          return _EmptyRefList;
      }
    }

    /// <summary>
    /// Этот список может содержать не все виды документов и поддокументов
    /// </summary>
    private readonly Dictionary<string, TableRefList> _MasterTableDict;

    private static readonly TableRefList _EmptyRefList = new TableRefList();

    private TableRefList InternalGetList(string tableName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
#endif

      TableRefList res;
      if (!_MasterTableDict.TryGetValue(tableName, out res))
      {
        res = new TableRefList();
        _MasterTableDict.Add(tableName, res);
      }
      return res;
    }

    #endregion
  }
}
