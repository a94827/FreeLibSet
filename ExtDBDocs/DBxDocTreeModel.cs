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
        throw new BugException("Table \"" + table.TableName + "\" does not contain column \"" + docType.TreeParentColumnName + "\"");

      bool needsResort = false;

      while (true)
      {
        IdCollection<Int32> missingIds = new IdCollection<Int32>();
        foreach (DataRow row in table.Rows)
        {
          Int32 parentId = DataTools.GetInt32(row[pParentId]);
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
          new ValueInListFilter("Id", missingIds), docType.DefaultOrder);

        if (table2.Rows.Count != missingIds.Count)
          throw new BugException("When loading rows of table \"" + docType.Name + "\" for Id=" + missingIds.ToString() + " has been taken rows: " + table2.Rows.Count.ToString());

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
    private readonly DBxDocProvider _DocProvider;

    /// <summary>
    /// Описание вида документа
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private readonly DBxDocType _DocType;

    private const string DefIntegrityFlagColumnName = "__Integrity";

    /// <summary>
    /// Имя логического поля, используемого для индикации узлов, которые не прошли условие фильтра,
    /// но были добавлены, чтобы обеспечить целостность дерева.
    /// Пустая строка означает, что все узлы прошли фильтр (или фильтр не задан).
    /// </summary>
    public string IntegrityFlagColumnName { get { return _IntegrityFlagColumnName; } }
    private readonly string _IntegrityFlagColumnName;

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
    /// <param name="repeaterTable">Ссылка на таблицу-повторитель, если она должна использоваться вместо <see cref="DBxMultiSubDocs.SubDocsView"/>.
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
    private readonly DBxMultiSubDocs _SubDocs;

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
}
