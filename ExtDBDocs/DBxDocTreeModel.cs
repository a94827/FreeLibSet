using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Trees;
using System.Data;

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
  /// Источник данных дерева для просмотра документов
  /// </summary>
  public class DBxDocTreeModel : DataTableTreeModel
  {
    #region Конструкторы

    /// <summary>
    /// Создать модель для всех документов, кроме удаленных
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docType">Вид документа</param>
    /// <param name="columns">Список столбцов таблицы</param>
    public DBxDocTreeModel(DBxDocProvider docProvider, DBxDocType docType, DBxColumns columns)
      : this(docProvider, docType, columns, GetDeletedFalseFilter(docProvider))
    {
    }

    private static DBxFilter GetDeletedFalseFilter(DBxDocProvider docProvider)
    {
      if (docProvider.DocTypes.UseDeleted)
        return DBSDocType.DeletedFalseFilter;
      else
        return null;
    }

    /// <summary>
    /// Создать модель для документов с выбранными фильтрами.
    /// Не забудьте добавить в список фильтр по удаленным документам
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docType">Вид документа</param>
    /// <param name="columns">Список столбцов таблицы</param>
    /// <param name="filters">Фильтр для SQL-запроса</param>
    public DBxDocTreeModel(DBxDocProvider docProvider, DBxDocType docType, DBxColumns columns, DBxFilter filters)
      : base(CreateTable(docProvider, docType, columns, filters),
         DBSDocType.Id,
         docType.TreeParentColumnName, docType.DefaultOrder.ToString() /*23.11.2018 */)
    {
      _DocProvider = docProvider;
      _DocType = docType;

      if (Table.Columns.Contains(DefIntegrityFlagColumnName))
        _IntegrityFlagColumnName = DefIntegrityFlagColumnName;
    }

    private static DataTable CreateTable(DBxDocProvider docProvider, DBxDocType docType, DBxColumns columns, DBxFilter filters)
    {
      #region Обязательные столбцы

      DBxColumnList RequiredColumns = new DBxColumnList();
      RequiredColumns.Add(docType.TreeParentColumnName);
      docType.DefaultOrder.GetColumnNames(RequiredColumns);
      columns += new DBxColumns(RequiredColumns);

      #endregion

      DataTable Table = docProvider.FillSelect(docType.Name,
        columns,
        filters, null /*23.11.2018 DocType.DefaultOrder*/);

      DataTools.SetPrimaryKey(Table, "Id");

      // 14.11.2017
      // В фильтр могут попасть дочерние документы, родительские документы для которых не попадут
      // в фильтр. Требуется рекурсивно добавить недостающие документы, чтобы обеспечить целостность
      // модели

      int pParentId = Table.Columns.IndexOf(docType.TreeParentColumnName);
      if (pParentId < 0)
        throw new BugException("Таблица \"" + Table.TableName + "\" не содержит столбца \"" + docType.TreeParentColumnName + "\"");

      bool NeedsResort = false;

      while (true)
      {
        IdList MissingIds = new IdList();
        foreach (DataRow Row in Table.Rows)
        {
          Int32 ParentId = DataTools.GetInt(Row[pParentId]);
          if (ParentId == 0)
            continue;
          if (Table.Rows.Find(ParentId) == null)
            MissingIds.Add(ParentId);
        }

        if (MissingIds.Count == 0)
          break;

        // Требуется догрузить недостающие строки
        DataTable Table2 = docProvider.FillSelect(docType.Name,
          columns,
          new IdsFilter(MissingIds), docType.DefaultOrder);

        if (Table2.Rows.Count != MissingIds.Count)
          throw new BugException("При попытке загрузить строки таблицы \"" + docType.Name + "\" для Id=" + MissingIds.ToString() + ", получено строк: " + Table2.Rows.Count.ToString());

        if (!NeedsResort)
        {
          // Требуется добавить в таблицу поле флага
          DataTable Table3 = Table.Clone();
          Table3.Columns.Add(DefIntegrityFlagColumnName, typeof(bool));
          foreach (DataRow Row1 in Table.Rows)
            Table3.Rows.Add(Row1.ItemArray);
          Table = Table3;
          DataTools.SetPrimaryKey(Table, "Id");
        }

        // Добавляем строки, устанавливая флаг
        foreach (DataRow Row2 in Table2.Rows)
        {
          DataRow Row1 = Table.Rows.Add(Row2.ItemArray);
          Row1[DefIntegrityFlagColumnName] = true;
        }

        NeedsResort = true;
      }

      if (NeedsResort)
      {
        Table.DefaultView.Sort = docType.DefaultOrder.ToString();
        Table = Table.DefaultView.ToTable();
        DataTools.SetPrimaryKey(Table, "Id");
        Table.AcceptChanges();
      }

      return Table;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private DBxDocProvider _DocProvider;

    /// <summary>
    /// Описание вида документа
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private DBxDocType _DocType;

    private const string DefIntegrityFlagColumnName = "__Integrity";

    /// <summary>
    /// Имя логического поля, используемого для индикации узлов, которые не прошли условие фильтра,
    /// но были добавлены, чтобы обеспечить целостность дерева.
    /// Пустая строка означает, что все узлы прошли фильтр (или фильтр не задан).
    /// </summary>
    public string IntegrityFlagColumnName { get { return _IntegrityFlagColumnName; } }
    private string _IntegrityFlagColumnName;

    #endregion
  }

  /// <summary>
  /// Источник данных дерева для просмотра поддокументов в процессе редактирования одного или нескольких документов
  /// </summary>
  public class DBxSubDocTreeModel : DataTableTreeModel
  {
    #region Конструктор

    /// <summary>
    /// Создает модель
    /// </summary>
    /// <param name="subDocs">Объект для доступа к поддокументам одного вида</param>
    /// <param name="columns">Список столбцов, которые должны быть в таблице</param>
    public DBxSubDocTreeModel(DBxMultiSubDocs subDocs, DBxColumns columns)
      : base(subDocs.SubDocsView.Table,
         "Id",
         subDocs.SubDocType.TreeParentColumnName,
         subDocs.SubDocsView.Sort)
    {
      _SubDocs = subDocs;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект для доступа к поддокументам одного вида
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращае массив, содержащий идентификатор <paramref name="id"/> 
    /// и все дочерние идентификаторы для строк, возвращаемых GetRowsWithChildren().
    /// Если <paramref name="id"/>=0, то возвращается пустой массив
    /// </summary>
    /// <param name="id">Корневой идентификатор</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] GetIdsWithChildren(Int32 id)
    {
      if (id == 0)
        return DataTools.EmptyIds;
      else
      {
        DataRow Row = SubDocs.SubDocsView.Table.Rows.Find(id);
        if (Row == null)
          return DataTools.EmptyIds;

        DataRow[] Rows = base.GetRowsWithChildren((object)Row);
        return DataTools.GetIds(Rows);
      }
    }

    #endregion
  }

  /// <summary>
  /// Расширение табличной модели установкой дополнительного фильтра
  /// </summary>
  public class FilteredDataTableTreeModel : DataTableTreeModel
  {
    // Кандидат на перенос в ExtTools.dll

    #region Конструкторы

    /// <summary>
    /// Создает модель, основанную на существующей
    /// </summary>
    /// <param name="sourceModel">Исходная модель</param>
    /// <param name="filter">Накладываемый фильтр на таблицу</param>
    public FilteredDataTableTreeModel(DataTableTreeModel sourceModel, DBxFilter filter)
      : this(sourceModel, filter, String.Empty)
    {
    }

    /// <summary>
    /// Создает модель, основанную на существующей
    /// </summary>
    /// <param name="sourceModel">Исходная модель</param>
    /// <param name="filter">Накладываемый фильтр на таблицу</param>
    /// <param name="sourceIntegrityFlagColumnName">Имя логического поля в исходной таблице, которое опредедяет узлы, добавленные исключительно из-за необходимости соблюдения целостности дерева.</param>
    public FilteredDataTableTreeModel(DataTableTreeModel sourceModel, DBxFilter filter, string sourceIntegrityFlagColumnName)
      : base(CreateTable(sourceModel, filter, sourceIntegrityFlagColumnName),
      sourceModel.IdColumnName, sourceModel.ParentColumnName, sourceModel.Sort)
    {
      _SourceModel = sourceModel;
      _Filter = filter;
      if (String.IsNullOrEmpty(sourceIntegrityFlagColumnName))
        _IntegrityFlagColumnName = DefIntegrityFlagColumnName;
      else
        _IntegrityFlagColumnName = sourceIntegrityFlagColumnName;
    }

    private static DataTable CreateTable(DataTableTreeModel sourceModel, DBxFilter filter, string sourceIntegrityFlagColumnName)
    {
#if DEBUG
      if (sourceModel == null)
        throw new ArgumentNullException("sourceModel");
#endif

      string PK = DataTools.GetPrimaryKey(sourceModel.Table);

      DataTable ResTable = sourceModel.Table.Clone();
      string IntegrityFlagColumnName;
      if (String.IsNullOrEmpty(sourceIntegrityFlagColumnName))
      {
        IntegrityFlagColumnName = DefIntegrityFlagColumnName;
        ResTable.Columns.Add(IntegrityFlagColumnName, typeof(bool));
      }
      else
      {
        if (!sourceModel.Table.Columns.Contains(sourceIntegrityFlagColumnName))
          throw new ArgumentException("Таблица \"" + sourceModel.Table.TableName + "\" не содержит столбца \"" + sourceIntegrityFlagColumnName + "\"", "sourceIntegrityFlagColumnName");
        DataTools.SetPrimaryKey(ResTable, PK);

        IntegrityFlagColumnName = sourceIntegrityFlagColumnName;
        ValueFilter Filter2 = new ValueFilter(sourceIntegrityFlagColumnName, false);
        if (filter == null)
          filter = Filter2;
        else
          filter = AndFilter.FromArray(new DBxFilter[2] { filter, Filter2 });
      }

      // Лучше сделать отдельный DataView, а не использовать DefaultView. 
      // Вдруг исходная модель сейчас используется.
      using (DataView dv = new DataView(sourceModel.Table))
      {
        if (filter == null)
          dv.RowFilter = sourceModel.Table.DefaultView.RowFilter;
        else
          dv.RowFilter = filter.AddToDataViewRowFilter(sourceModel.Table.DefaultView.RowFilter);
        dv.Sort = sourceModel.Table.DefaultView.Sort;
        //Int32 [] aaa=DataTools.GetIdsFromField(SourceModel.Table, "GroupId");

        foreach (DataRowView drv in dv)
          ResTable.Rows.Add(drv.Row.ItemArray);
      }

      #region Добавление недостающих строк

      int pParentId = ResTable.Columns.IndexOf(sourceModel.ParentColumnName);
      if (pParentId < 0)
        throw new BugException("Таблица \"" + ResTable.TableName + "\" не содержит столбца \"" + sourceModel.ParentColumnName + "\"");

      bool NeedsResort = false;

      while (true)
      {
        IdList MissingIds = new IdList();
        foreach (DataRow Row in ResTable.Rows)
        {
          Int32 ParentId = DataTools.GetInt(Row[pParentId]);
          if (ParentId == 0)
            continue;
          if (ResTable.Rows.Find(ParentId) == null)
            MissingIds.Add(ParentId);
        }

        if (MissingIds.Count == 0)
          break;

        // Требуется догрузить недостающие строки
        foreach (Int32 Id in MissingIds)
        {
          DataRow SrcRow = sourceModel.Table.Rows.Find(Id);
          if (SrcRow == null)
            throw new ArgumentException("Таблица исходной модели не содержит строки с ключом " + Id.ToString() + ". Целостность дерева нарушена", "sourceModel");
          DataRow ResRow = ResTable.Rows.Add(SrcRow.ItemArray);
          ResRow[IntegrityFlagColumnName] = true;
        }

        NeedsResort = true;
      }

      #endregion

      #region Пересортировка

      if (NeedsResort)
      {
        ResTable.DefaultView.Sort = sourceModel.Table.DefaultView.Sort;
        ResTable = ResTable.DefaultView.ToTable();
        DataTools.SetPrimaryKey(ResTable, PK);
        ResTable.AcceptChanges();
      }

      #endregion

      return ResTable;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Базовая модель, на которую накладываются фильтры
    /// </summary>
    public DataTableTreeModel SourceModel { get { return _SourceModel; } }
    private DataTableTreeModel _SourceModel;

    /// <summary>
    /// Накладываемый фильтр
    /// </summary>
    public DBxFilter Filter { get { return _Filter; } }
    private DBxFilter _Filter;


    private const string DefIntegrityFlagColumnName = "__Integrity";

    /// <summary>
    /// Имя логического поля, используемого для индикации узлов, которые не прошли условие фильтра,
    /// но были добавлены, чтобы обеспечить целостность дерева.
    /// Пустая строка означает, что все узлы прошли фильтр (или фильтр не задан).
    /// Это свойство не обязано совпадать с аргументом SourceIntegrityFlagColumnName, заданном в конструкторе
    /// </summary>
    public string IntegrityFlagColumnName { get { return _IntegrityFlagColumnName; } }
    private string _IntegrityFlagColumnName;

    #endregion
  }
}
