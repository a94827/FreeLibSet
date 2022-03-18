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
    /// Словари замены идентификаторов для каждой таблицы.
    /// Ключ - имя таблицы документа или поддокумента, на которую выполняется ссылка
    /// Значение - список пар "СтарыйId:НовыйId"
    /// </summary>
    private Dictionary<String, TableIdPairs> _SubstIds;

    #endregion

    #region Структура DelayedFieldInfo

    /// <summary>
    /// Информация об отложенной записи значения поля
    /// </summary>
    internal struct DelayedFieldInfo
    {
      #region Поля

      /// <summary>
      /// Таблица, в которой выполняется замена
      /// </summary>
      public string TableName;

      /// <summary>
      /// Идентификатор документа или поддокумента
      /// </summary>
      public Int32 Id;

      /// <summary>
      /// Имя поля, в которое требуется записать
      /// </summary>
      public string ColumnName;

      /// <summary>
      /// Записываемое значение
      /// </summary>
      public object Value;

      #endregion
    }

    #endregion

    #region Метод, вызываемый из ServerDocProvider.ApplyChanges()


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
      #region Поиск

      _DelayedList = null;

      //Changes.Caller.Splash.PhaseText = "Поиск новых документов и поддокументов";
      // SubstIds создадим только когда понадобится
      _SubstIds = null; // убираем предыдущее
      foreach (DBxMultiDocs multiDocs in docSet)
      {
        // Основная таблица
        FindTableSubstIds(ds.Tables[multiDocs.DocType.Name]);
        // Поддокументы
        for (int i = 0; i < multiDocs.DocType.SubDocs.Count; i++)
        {
          DBxSubDocType sdt = multiDocs.DocType.SubDocs[i];
          if (multiDocs.SubDocs.ContainsModified(sdt.Name))
            FindTableSubstIds(ds.Tables[sdt.Name]);
        }
      }

      #endregion

      #region Замена

      // 2. Если есть новые документы / поддокументы, перебираем все, чтобы 
      // заменить ссылки
      if (_SubstIds != null)
      {
        SingleScopeList<string> proceedTableNames = new SingleScopeList<string>();

        //Changes.Caller.Splash.PhaseText = "Замена идентификаторов";
        foreach (DBxMultiDocs multiDocs in docSet)
        {
          // Основная таблица
          ApplyTableSubstIds(ds.Tables[multiDocs.DocType.Name],
            multiDocs.DocType,
            proceedTableNames);

          // Поддокументы
          for (int i = 0; i < multiDocs.DocType.SubDocs.Count; i++)
          {
            DBxSubDocType sdt = multiDocs.DocType.SubDocs[i];
            if (multiDocs.SubDocs.ContainsModified(sdt.Name))
            {
              ApplyDocId(ds.Tables[sdt.Name],
                multiDocs.DocType);

              ApplyTableSubstIds(ds.Tables[sdt.Name],
                sdt,
                proceedTableNames);
            }
          }
        }
      }

      #endregion

      // 17.11.2016
      // Надо сбросить буферизацию идентификаторов, иначе не работает копирование документов,
      // содержащих поддокументы
      foreach (DBxMultiDocs multiDocs in docSet)
        multiDocs.ResetDocIds();
    }

    #endregion

    #region Внутренние методы

    /// <summary>
    /// Замена фиктивных идентификаторов Id на реальные для всех новых строк таблицы
    /// </summary>
    /// <param name="table">Таблица, в которой выполняется замена</param>
    private void FindTableSubstIds(DataTable table)
    {
      TableIdPairs pairs = null;

      DataTools.SetPrimaryKey(table, (string)null);
      table.Columns["Id"].Unique = false;

      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Added)
        {
          Int32 orgId = (Int32)row["Id"];
          if (orgId < 0)
          {
            if (pairs == null)
            {
              if (_SubstIds == null)
                _SubstIds = new Dictionary<string, TableIdPairs>();

              if (!_SubstIds.TryGetValue(table.TableName, out pairs))
              {
                // По идее - будет вызываться всегда
                pairs = new TableIdPairs();
                _SubstIds.Add(table.TableName, pairs);
              }
            }


            Int32 newId = _Caller.Source.GlobalData.GetNextId(table.TableName, _Caller.Source.MainDBEntry);
            pairs.Add(orgId, newId);
            row["Id"] = newId;
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
      TableIdPairs pairs;
      if (!_SubstIds.TryGetValue(docType.Name, out pairs))
        return;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted) // 03.11.2015
          continue;
        Int32 newDocId;
        if (pairs.TryGetValue((int)row["DocId"], out newDocId))
          row["DocId"] = newDocId;
      }
    }

    /// <summary>
    /// Замена ссылочных полей
    /// </summary>
    /// <param name="table"></param>
    /// <param name="dtb"></param>
    /// <param name="proceedTableNames"></param>
    private void ApplyTableSubstIds(DataTable table,
      DBxDocTypeBase dtb,
      SingleScopeList<string> proceedTableNames)
    {
      #region Обычные ссылки

      for (int i = 0; i < dtb.Struct.Columns.Count; i++)
      {
        DBxColumnStruct colDef = dtb.Struct.Columns[i];
        int colPos = table.Columns.IndexOf(colDef.ColumnName);
        if (colPos < 0)
          continue;
        if (!String.IsNullOrEmpty(colDef.MasterTableName))
          ApplyColumnSubstIds(table, colPos, colDef.MasterTableName,
            proceedTableNames);
      }


      proceedTableNames.Add(dtb.Name);

      #endregion

      #region Замена переменных ссылочных полей

      if (dtb.VTRefs.Count > 0)
      {
        for (int i = 0; i < dtb.VTRefs.Count; i++)
        {
          DBxVTReference vtr = dtb.VTRefs[i];
          int tableIdColumnPos = table.Columns.IndexOf(vtr.TableIdColumn.ColumnName);
          int docIdColumnPos = table.Columns.IndexOf(vtr.DocIdColumn.ColumnName);
          if (tableIdColumnPos <0 && docIdColumnPos < 0)
            continue;
          if (tableIdColumnPos < 0)
            throw new InvalidOperationException("Неправильный набор данных. В таблице \"" + table.TableName +
              "\" существует переменное ссылочное поле \"" + vtr.DocIdColumn.ColumnName +
              "\", содержащее идентификатор документа, но нет поля \"" + vtr.TableIdColumn.ColumnName +
              "\" содержащего идентификатор таблицы");
          if (docIdColumnPos < 0)
            throw new InvalidOperationException("Неправильный набор данных. При создании строки в таблице \"" + table.TableName +
              "\" сущействует переменное ссылочное поле \"" + vtr.TableIdColumn.ColumnName +
              "\", содержащее идентификатор таблицы, но нет поля \"" + vtr.TableIdColumn.ColumnName +
              "\" содержащего идентификатор документа");

          foreach (DataRow row in table.Rows)
          {
            if (row.RowState == DataRowState.Deleted)
              continue;
            Int32 refDocId = DataTools.GetInt(row[docIdColumnPos]);
            Int32 tableId = DataTools.GetInt(row[tableIdColumnPos]);

            if (refDocId == 0 && tableId == 0)
              continue;
#if XXX // проверки в DBxRealDocProvider.ValidateRefs()
            if (tableId == 0)
              throw new InvalidOperationException("При создании строки в таблице \"" + table.TableName +
                "\" задано значение для переменного ссылочного поля \"" + vtr.DocIdColumn.ColumnName +
                "\" - идентификатора документа, но не задан идентификатор таблицы в поле \"" + vtr.TableIdColumn.ColumnName +
                "\" - идентификатора таблицы");
            if (refDocId == 0)
              throw new InvalidOperationException("При создании строки в таблице \"" + table.TableName +
                "\" не задано значение для переменного ссылочного поля \"" + vtr.DocIdColumn.ColumnName +
                "\" - идентификатора документа, но задан идентификатор таблицы в поле \"" + vtr.TableIdColumn.ColumnName +
                "\" - идентификатора таблицы");

#endif
            DBxDocType masterDT = _Caller.DocTypes.FindByTableId(tableId);
            if (masterDT == null)
              throw new InvalidOperationException("При создании строки в таблице \"" + table.TableName +
                "\" задан идентификатор " + tableId + " мастер-таблицы в поле " +
                vtr.TableIdColumn.ColumnName + "\", которому не соответствует никакая таблица");
#if XXX

            if (!vtr.MasterTableNames.Contains(masterDT.Name))
              throw new InvalidOperationException("В таблице \"" + table.TableName +
              "\" задан идентификатор мастер-таблицы \"" + masterDT + "\" в поле \"" +
              vtr.TableIdColumn.ColumnName + "\", которую нельзя использовать. Для ссылки \"" + vtr.Name + "\" допускаются только таблицы: " +
            DataTools.JoinNotEmptyStrings(", ", vtr.MasterTableNames));
#endif

            TableIdPairs pairs;
            if (!_SubstIds.TryGetValue(masterDT.Name, out pairs))
              continue;

            Int32 newRefId;
            if (pairs.TryGetValue(refDocId, out newRefId))
            {
              // Запись значения поля откладывается на будущее
              if (_DelayedList == null)
                _DelayedList = new List<DelayedFieldInfo>();
              DelayedFieldInfo info = new DelayedFieldInfo();
              info.TableName = table.TableName;
              info.Id = DataTools.GetInt(row, "Id");
              info.ColumnName = vtr.DocIdColumn.ColumnName;
              info.Value = newRefId;
              _DelayedList.Add(info);
              row[docIdColumnPos] = DBNull.Value;
            }
          }
        }
      }

      #endregion
    }

    private void ApplyColumnSubstIds(DataTable table, int columnPos, string masterTableName,
      SingleScopeList<string> proceedTableNames)
    {
      TableIdPairs pairs;
      if (!_SubstIds.TryGetValue(masterTableName, out pairs))
        return;

      int pIdColumn = table.Columns.IndexOf("Id");
      foreach (DataRow row in table.Rows)
      {
        int oldRefId = DataTools.GetInt(DBxDocSet.GetValue(row, columnPos));
        if (oldRefId == 0)
          continue;
        int newRefId;
        if (pairs.TryGetValue(oldRefId, out newRefId))
        {
          if (proceedTableNames.Contains(masterTableName))
          {
            // Можно просто заменять ссылку, без использования списка отложенных
            row[columnPos] = newRefId;
          }
          else
          {
            if (_DelayedList == null)
              _DelayedList = new List<DelayedFieldInfo>();
            DelayedFieldInfo info = new DelayedFieldInfo();
            info.TableName = table.TableName;
            info.Id = DataTools.GetInt(row[pIdColumn]);
            info.ColumnName = table.Columns[columnPos].ColumnName;
            info.Value = newRefId;
            _DelayedList.Add(info);
            if (row.Table.Columns[columnPos].AllowDBNull) // 15.01.2018. 
              row[columnPos] = DBNull.Value;
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

      TableIdPairs pairs;
      if (!_SubstIds.TryGetValue(tableName, out pairs))
        return false;

      return pairs.ContainsValue(id);
    }

    #endregion
  }
}
