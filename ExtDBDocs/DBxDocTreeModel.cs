// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Models.Tree;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{

  /// <summary>
  /// Источник данных дерева для просмотра документов
  /// </summary>
  public class DBxDocTreeModel : DataTableTreeModelWithIds<Int32>
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
         docType.TreeParentColumnName)
    {
      base.Sort = docType.DefaultOrder.ToString(); /*23.11.2018 */

      _DocProvider = docProvider;
      _DocType = docType;

      if (Table.Columns.Contains(DefIntegrityFlagColumnName))
        _IntegrityFlagColumnName = DefIntegrityFlagColumnName;
    }

    private static DataTable CreateTable(DBxDocProvider docProvider, DBxDocType docType, DBxColumns columns, DBxFilter filters)
    {
      #region Обязательные столбцы

      if (columns == null)
        columns = new DBxColumns(docType.Struct.AllColumnNames); // 04.02.2022

      DBxColumnList requiredColumns = new DBxColumnList();
      requiredColumns.Add("Id"); // 04.02.2022
      requiredColumns.Add(docType.TreeParentColumnName);
      docType.DefaultOrder.GetColumnNames(requiredColumns); // Тут могут быть столбцы с точками
      columns += new DBxColumns(requiredColumns);

      #endregion

      DataTable table = docProvider.FillSelect(docType.Name,
        columns,
        filters, null /*23.11.2018 DocType.DefaultOrder*/);

      DataTools.SetPrimaryKey(table, "Id");

      // 14.11.2017
      // В фильтр могут попасть дочерние документы, родительские документы для которых не попадут
      // в фильтр. Требуется рекурсивно добавить недостающие документы, чтобы обеспечить целостность
      // модели

      int pParentId = table.Columns.IndexOf(docType.TreeParentColumnName);
      if (pParentId < 0)
        throw new BugException("Таблица \"" + table.TableName + "\" не содержит столбца \"" + docType.TreeParentColumnName + "\"");

      bool needsResort = false;

      while (true)
      {
        IdList missingIds = new IdList();
        foreach (DataRow row in table.Rows)
        {
          Int32 parentId = DataTools.GetInt(row[pParentId]);
          if (parentId == 0)
            continue;
          if (table.Rows.Find(parentId) == null)
            missingIds.Add(parentId);
        }

        if (missingIds.Count == 0)
          break;

        // Требуется догрузить недостающие строки
        DataTable table2 = docProvider.FillSelect(docType.Name,
          columns,
          new IdsFilter(missingIds), docType.DefaultOrder);

        if (table2.Rows.Count != missingIds.Count)
          throw new BugException("При попытке загрузить строки таблицы \"" + docType.Name + "\" для Id=" + missingIds.ToString() + ", получено строк: " + table2.Rows.Count.ToString());

        if (!needsResort)
        {
          // Требуется добавить в таблицу поле флага
          DataTable table3 = table.Clone();
          table3.Columns.Add(DefIntegrityFlagColumnName, typeof(bool));
          foreach (DataRow row1 in table.Rows)
            table3.Rows.Add(row1.ItemArray);
          table = table3;
          DataTools.SetPrimaryKey(table, "Id");
        }

        // Добавляем строки, устанавливая флаг
        foreach (DataRow row2 in table2.Rows)
        {
          DataRow row1 = table.Rows.Add(row2.ItemArray);
          row1[DefIntegrityFlagColumnName] = true;
        }

        needsResort = true;
      }

      if (needsResort)
      {
        table.DefaultView.Sort = docType.DefaultOrder.ToString();
        table = table.DefaultView.ToTable();
        DataTools.SetPrimaryKey(table, "Id");
        table.AcceptChanges();
      }

      return table;
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
  public class DBxSubDocTreeModel : DataTableTreeModelWithIds<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Создает модель
    /// </summary>
    /// <param name="subDocs">Объект для доступа к поддокументам одного вида</param>
    /// <param name="repeaterTable">Ссылка на таблицу-повторитель, если она должна использоваться вместо DBxMultiSubDocs.SubDocsView.
    /// Null, если должна использоваться основная таблица</param>
    public DBxSubDocTreeModel(DBxMultiSubDocs subDocs, DataTable repeaterTable)
      : base(repeaterTable??subDocs.SubDocsView.Table,
         "Id",
         subDocs.SubDocType.TreeParentColumnName)
    {
      base.Sort = subDocs.SubDocsView.Sort;
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

#if XXX // Не используется
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
        DataRow row = SubDocs.SubDocsView.Table.Rows.Find(id);
        if (row == null)
          return DataTools.EmptyIds;

        DataRow[] rows = base.GetRowsWithChildren((object)row);
        return DataTools.GetIds(rows);
      }
    }
#endif

    #endregion
  }

  /// <summary>
  /// Расширение табличной модели установкой дополнительного фильтра
  /// </summary>
  public class FilteredDataTableTreeModelWithIds<T> : DataTableTreeModelWithIds<T>
    where T : IEquatable<T>
  {
    // Кандидат на перенос в ExtTools.dll

    #region Конструкторы

    /// <summary>
    /// Создает модель, основанную на существующей
    /// </summary>
    /// <param name="sourceModel">Исходная модель</param>
    /// <param name="filter">Накладываемый фильтр на таблицу</param>
    public FilteredDataTableTreeModelWithIds(DataTableTreeModel sourceModel, DBxFilter filter)
      : this(sourceModel, filter, String.Empty)
    {
    }

    /// <summary>
    /// Создает модель, основанную на существующей
    /// </summary>
    /// <param name="sourceModel">Исходная модель</param>
    /// <param name="filter">Накладываемый фильтр на таблицу</param>
    /// <param name="sourceIntegrityFlagColumnName">Имя логического поля в исходной таблице, которое определяет узлы, добавленные исключительно из-за необходимости соблюдения целостности дерева.</param>
    public FilteredDataTableTreeModelWithIds(DataTableTreeModel sourceModel, DBxFilter filter, string sourceIntegrityFlagColumnName)
      : base(CreateTable(sourceModel, filter, sourceIntegrityFlagColumnName),
      sourceModel.IdColumnName, sourceModel.ParentColumnName)
    {
      base.Sort = sourceModel.Sort;
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

      string pk = DataTools.GetPrimaryKey(sourceModel.Table);

      DataTable resTable = sourceModel.Table.Clone();
      string integrityFlagColumnName;
      if (String.IsNullOrEmpty(sourceIntegrityFlagColumnName))
      {
        integrityFlagColumnName = DefIntegrityFlagColumnName;
        resTable.Columns.Add(integrityFlagColumnName, typeof(bool));
      }
      else
      {
        if (!sourceModel.Table.Columns.Contains(sourceIntegrityFlagColumnName))
          throw new ArgumentException("Таблица \"" + sourceModel.Table.TableName + "\" не содержит столбца \"" + sourceIntegrityFlagColumnName + "\"", "sourceIntegrityFlagColumnName");
        DataTools.SetPrimaryKey(resTable, pk);

        integrityFlagColumnName = sourceIntegrityFlagColumnName;
        ValueFilter filter2 = new ValueFilter(sourceIntegrityFlagColumnName, false);
        if (filter == null)
          filter = filter2;
        else
          filter = AndFilter.FromArray(new DBxFilter[2] { filter, filter2 });
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
          resTable.Rows.Add(drv.Row.ItemArray);
      }

      #region Добавление недостающих строк

      int pParentId = resTable.Columns.IndexOf(sourceModel.ParentColumnName);
      if (pParentId < 0)
        throw new BugException("Таблица \"" + resTable.TableName + "\" не содержит столбца \"" + sourceModel.ParentColumnName + "\"");

      bool needsResort = false;

      while (true)
      {
        IdList missingIds = new IdList();
        foreach (DataRow row in resTable.Rows)
        {
          Int32 parentId = DataTools.GetInt(row[pParentId]);
          if (parentId == 0)
            continue;
          if (resTable.Rows.Find(parentId) == null)
            missingIds.Add(parentId);
        }

        if (missingIds.Count == 0)
          break;

        // Требуется догрузить недостающие строки
        foreach (Int32 id in missingIds)
        {
          DataRow srcRow = sourceModel.Table.Rows.Find(id);
          if (srcRow == null)
            throw new ArgumentException("Таблица исходной модели не содержит строки с ключом " + id.ToString() + ". Целостность дерева нарушена", "sourceModel");
          DataRow resRow = resTable.Rows.Add(srcRow.ItemArray);
          resRow[integrityFlagColumnName] = true;
        }

        needsResort = true;
      }

      #endregion

      #region Пересортировка

      if (needsResort)
      {
        resTable.DefaultView.Sort = sourceModel.Table.DefaultView.Sort;
        resTable = resTable.DefaultView.ToTable();
        DataTools.SetPrimaryKey(resTable, pk);
        resTable.AcceptChanges();
      }

      #endregion

      return resTable;
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
