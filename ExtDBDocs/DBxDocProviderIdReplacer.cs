// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Класс для замены фиктивных идентификаторов, используемых для созданных
  /// документов / поддокументов на реальные 
  /// Используется в DBxDocProvider.ApplyChanges()
  /// </summary>
  internal class DBxDocProviderIdReplacer
  {
    #region Конструктор

    public DBxDocProviderIdReplacer(DBxRealDocProvider caller)
    {
      _Caller = caller;
    }

    #endregion

    #region Поля данных

    /// <summary>
    /// Владелец
    /// </summary>
    private DBxRealDocProvider _Caller;

    /// <summary>
    /// Пары "СтарыйId:НовыйId" для одной таблицы документа или поддокумента
    /// Реализует двустороннюю коллекцию
    /// </summary>
    private class TableIdPairs : BidirectionalDictionary<Int32, Int32>
    {
      // Нет дополнительных полей
    }

    /// <summary>
    /// Словари замены идентификаторов для каждой таблицы
    /// </summary>
    private Dictionary<String, TableIdPairs> _SubstIds;

    #endregion

    #region Структура DelayedFieldInfo

    /// <summary>
    /// Информация об отложенной записи значения поля
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal struct DelayedFieldInfo
    {
      public string TableName;
      public Int32 Id;
      public string FieldName;
      public object FieldValue;
    }

    #endregion

    #region Метод, вызываемый из ServerDocProvider.ApplyChanges


    public List<DelayedFieldInfo> DelayedList { get { return _DelayedList; } }
    private List<DelayedFieldInfo> _DelayedList;


    /// <summary>
    /// Выполнить замену идентификаторов
    /// Заполняет список DelayedList
    /// На момент вызова база данных должна быть заблокирована и блокировка должна
    /// сохраняться до записи всех документов.
    /// Метод вызывается из секции lock, поэтому он не потокобезопасен
    /// </summary>
    public void PerformReplace(DBxDocSet docSet, DataSet ds)
    {
      _DelayedList = null;

      //Changes.Caller.Splash.PhaseText = "Поиск новых документов и поддокументов";
      // SubstIds создадим только когда понадобится
      _SubstIds = null; // убираем предыдущее
      foreach (DBxMultiDocs MultiDocs in docSet)
      {
        // Основная таблица
        MakeTableSubstIds(ds.Tables[MultiDocs.DocType.Name]);
        // Поддокументы
        for (int i = 0; i < MultiDocs.DocType.SubDocs.Count; i++)
        {
          DBxSubDocType sdt = MultiDocs.DocType.SubDocs[i];
          if (MultiDocs.SubDocs.ContainsModified(sdt.Name))
            MakeTableSubstIds(ds.Tables[sdt.Name]);
        }
      }

      // 2. Если есть новые документы / поддокументы, перебираем все, чтобы 
      // заменить ссылки
      if (_SubstIds != null)
      {
        SingleScopeList<string> ProceedTableNames = new SingleScopeList<string>();

        //Changes.Caller.Splash.PhaseText = "Замена идентификаторов";
        foreach (DBxMultiDocs MultiDocs in docSet)
        {
          // Основная таблица
          ApplyTableSubstIds(ds.Tables[MultiDocs.DocType.Name],
            MultiDocs.DocType.Struct,
            ProceedTableNames);

          // Поддокументы
          for (int i = 0; i < MultiDocs.DocType.SubDocs.Count; i++)
          {
            DBxSubDocType sdt = MultiDocs.DocType.SubDocs[i];
            if (MultiDocs.SubDocs.ContainsModified(sdt.Name))
            {
              ApplyDocId(ds.Tables[sdt.Name],
                MultiDocs.DocType);

              ApplyTableSubstIds(ds.Tables[sdt.Name],
                sdt.Struct,
                ProceedTableNames);
            }
          }
        }
      }

      // 17.11.2016
      // Надо сбросить буферизацию идентификаторов, иначе не работает копирование документов,
      // содержащих поддокументы
      foreach (DBxMultiDocs MultiDocs in docSet)
        MultiDocs.ResetDocIds();
    }

    #endregion

    #region Внутренние методы

    /// <summary>
    /// Замена фиктивных идентификаторов Id на реальные для всех новых строк таблицы
    /// </summary>
    /// <param name="table">Таблица, в которой выполняется замена</param>
    private void MakeTableSubstIds(DataTable table)
    {
      TableIdPairs Pairs = null;

      DataTools.SetPrimaryKey(table, (string)null);
      table.Columns["Id"].Unique = false;

      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Added)
        {
          Int32 OrgId = (Int32)Row["Id"];
          if (OrgId < 0)
          {
            if (Pairs == null)
            {
              if (_SubstIds == null)
                _SubstIds = new Dictionary<string, TableIdPairs>();

              if (!_SubstIds.TryGetValue(table.TableName, out Pairs))
              {
                // По идее - будет вызываться всегда
                Pairs = new TableIdPairs();
                _SubstIds.Add(table.TableName, Pairs);
              }
            }


            Int32 NewId = _Caller.Source.GlobalData.GetNextId(table.TableName, _Caller.Source.MainDBEntry);
            Pairs.Add(OrgId, NewId);
            Row["Id"] = NewId;
          }
        }
      }
      table.Columns["Id"].Unique = true;
      DataTools.SetPrimaryKey(table, "Id");
    }


    /// <summary>
    /// Замена поля DocId для поддокумента
    /// </summary>
    /// <param name="table"></param>
    /// <param name="docType"></param>
    private void ApplyDocId(DataTable table, DBxDocType docType)
    {
      TableIdPairs Pairs;
      if (!_SubstIds.TryGetValue(docType.Name, out Pairs))
        return;
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted) // 03.11.2015
          continue;
        int NewDocId;
        if (Pairs.TryGetValue((int)Row["DocId"], out NewDocId))
          Row["DocId"] = NewDocId;
      }
    }

    /// <summary>
    /// Замена ссылочных полей
    /// </summary>
    /// <param name="table"></param>
    /// <param name="tableDef"></param>
    /// <param name="proceedTableNames"></param>
    private void ApplyTableSubstIds(DataTable table,
      DBxTableStruct tableDef,
      SingleScopeList<string> proceedTableNames)
    {
      int i;
      for (i = 0; i < tableDef.Columns.Count; i++)
      {
        DBxColumnStruct ColDef = tableDef.Columns[i];
        int ColumnPos = table.Columns.IndexOf(ColDef.ColumnName);
        if (ColumnPos < 0)
          continue;
        if (!String.IsNullOrEmpty(ColDef.MasterTableName))
          ApplyColumnSubstIds(table, ColumnPos, ColDef.MasterTableName,
            proceedTableNames);
      }


      proceedTableNames.Add(tableDef.TableName);

#if XXXX
      // 30.05.2007 
      // Замена переменных ссылочных полей
      if (TableDef.VTReferencesCount > 0)
      {
        foreach (DataRow Row in Table.Rows)
        {
          if (Row.RowState == DataRowState.Deleted)
            continue;
          for (i = 0; i < TableDef.VTReferencesCount; i++)
          {
            DBStruct.VTReference vtr = TableDef.VTReferences[i];
            int IdColumnPos = Table.Columns.IndexOf(vtr.IdField.FieldName);
            if (IdColumnPos < 0)
              continue;
            int TableColumnPos = Table.Columns.IndexOf(vtr.TableField.FieldName);
            if (TableColumnPos < 0)
              throw new InvalidOperationException("При создании строки в таблице \"" + Table.TableName +
                "\" задано переменное ссылочное поле \"" + vtr.IdField.FieldName +
                "\", содержащее идентификатор, но не задано поле \"" + vtr.TableField.FieldName +
                "\" содержащее номер таблицы");

            int OldRefId = DataTools.GetInt(Row[IdColumnPos]);
            if (OldRefId == 0)
              continue;

            int TableId = DataTools.GetInt(Row[TableColumnPos]);
            if (TableId == 0)
              throw new InavlidOperationException("При создании строки в таблице \"" + Table.TableName +
                "\" задано значение для переменного ссылочного поля \"" + vtr.IdField.FieldName +
                "\"-идентификатора, но не задан идентификатор таблицы в поле \"" + vtr.TableField.FieldName +
                "\"");

            string MasterTableName = FCaller.DocTypes.FindTableNameByTableId(TableId);
            if (string.IsNullOrEmpty(MasterTableName))
              throw new InavlidOperationException("При создании строки в таблице \"" + Table.TableName +
                "\" задан идентификатор мастер-таблицы \"" + TableId + "\" в поле \"" +
                vtr.TableField.FieldName + "\", которому не соответствует никакая таблица");

            TableIdPairs Pairs;
            if (!SubstIds.TryGetValue(MasterTableName, out Pairs))
              return;

            int NewRefId;
            if (Pairs.TryGetValue(OldRefId, out NewRefId))
            {
              // Запись значения поля откладывается на будущее
              if (DelayedValues == null)
                DelayedValues = new List<DelayedFieldInfo>();
              DelayedFieldInfo Info = new DelayedFieldInfo();
              Info.TableName = Table.TableName;
              Info.Id = DataTools.GetInt(Row, "Id");
              Info.FieldName = vtr.IdField.FieldName;
              Info.FieldValue = NewRefId;
              DelayedValues.Add(Info);
              Row[IdColumnPos] = DBNull.Value;
            }
          }
        }
      }
#endif
    }

    private void ApplyColumnSubstIds(DataTable table, int columnPos, string masterTableName,
      SingleScopeList<string> proceedTableNames)
    {
      TableIdPairs Pairs;
      if (!_SubstIds.TryGetValue(masterTableName, out Pairs))
        return;

      int pIdColumn = table.Columns.IndexOf("Id");
      foreach (DataRow Row in table.Rows)
      {
        int OldRefId = DataTools.GetInt(DBxDocSet.GetValue(Row, columnPos));
        if (OldRefId == 0)
          continue;
        int NewRefId;
        if (Pairs.TryGetValue(OldRefId, out NewRefId))
        {
          if (proceedTableNames.Contains(masterTableName))
          {
            // Можно просто заменять ссылку, без использования списка отложенных
            Row[columnPos] = NewRefId;
          }
          else
          {
            if (_DelayedList == null)
              _DelayedList = new List<DelayedFieldInfo>();
            DelayedFieldInfo Info = new DelayedFieldInfo();
            Info.TableName = table.TableName;
            Info.Id = DataTools.GetInt(Row[pIdColumn]);
            Info.FieldName = table.Columns[columnPos].ColumnName;
            Info.FieldValue = NewRefId;
            _DelayedList.Add(Info);
            if (Row.Table.Columns[columnPos].AllowDBNull) // 15.01.2018. 
              Row[columnPos] = DBNull.Value;
            // Интересно, а как быть с проверкой ссылочной целостности базы данных?
          }
        }
      }
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Метод возвращает true, если идентификатор <paramref name="id"/> является замененяющим для 
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsAdded(string tableName, Int32 id)
    {
      _Caller.CheckIsRealDocId(id);

      if (_SubstIds == null)
        return false;

      TableIdPairs Pairs;
      if (!_SubstIds.TryGetValue(tableName, out Pairs))
        return false;

      return Pairs.ContainsValue(id);
    }

    #endregion
  }
}
