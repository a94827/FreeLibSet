﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Models.Tree
{
  /// <summary>
  /// Расширение интерфейса ITreeModel, связанного с DataTable или DataView.
  /// Объявляет методы для преобразования строки таблицы данных DataRow в TreePath и обратно.
  /// Интерфейс не определяет факт наличия первичного ключа в таблице и типы полей, используемых для построения дерева.
  /// Также этот интерфейс, как и ITreeModel, не определяет, что используется в качестве частей TreePath.
  /// </summary>
  public interface IDataTableTreeModel : ITreeModel
  {
    #region Свойства

    /// <summary>
    /// Объект DataTable, реализующий модель
    /// </summary>
    DataTable Table { get; }

    /// <summary>
    /// Объект DataView.
    /// Может возвращать null.
    /// </summary>
    DataView DataView { get; }

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает строку данных, соответствующую заданному пути
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Объект DataRow из таблицы данных</returns>
    DataRow TreePathToDataRow(TreePath path);

    /// <summary>
    /// Возвращает путь в дереве, соответствующий строке таблицы данных
    /// </summary>
    /// <param name="row">Строка присоединенной таблицы данных</param>
    /// <returns>Путь в дереве</returns>
    TreePath TreePathFromDataRow(DataRow row);

    #endregion
  }

  /// <summary>
  /// Модель данных дерева, реализующая доступ по идентификаторам произвольного типа (числовым, строкам, Guid, ...).
  /// Нулевое значение идентификатора (или пустая строка) соответствует пустому узлу.
  /// </summary>
  /// <typeparam name="T">Тип идентификатора</typeparam>
  public interface ITreeModelWithIds<T> : ITreeModel
    where T : IEquatable<T>
  {
    #region Методы

    /// <summary>
    /// Возвращает идентификатор (значение поля IdColumnNames), соответствующее заданному пути
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    T TreePathToId(TreePath path);

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// </summary>
    /// <param name="id">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    TreePath TreePathFromId(T id);

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), для заданного родительского идентификатора
    /// и всем его вложенным узлам рекурсивно.
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="id">Идентификатор корневого узла. Если 0, возвращаются все идентификаторы в таблице</param>
    /// <returns>Массив идентификаторов</returns>
    T[] GetIdWithChildren(T id);

    #endregion
  }

  /// <summary>
  /// Источник просмотра древовидной структуры из таблицы DataTable.
  /// Предполагается, что имеется поле (IdColumnName), идентифицирующее строки (обычно поле является первичным ключом,
  /// но это не является обязательным условием). Также имеется ссылочное поле (ParentColumnName), используемое для построения дерева.
  /// Поле IdColumnName может быть числовым, строковым или иметь тип Guid или DateTime, лишь бы для этого поля можно было вычислить выражение с помощью DataTable.Select().
  /// Поле ParentColumnName должно иметь тот же тип, но обязательно поддерживать значение NULL, которое идентифицирует строки верхнего уровня.
  /// В процессе работы могут добавляться, изменяться или удаляться строки. 
  /// Значение поля IdColumnName не должно меняться.
  /// Значение поля ParentColumnName может меняться, при этом вызывается событие StructureChanged
  /// 
  /// В текущей реализации в качестве тегов, входящих в TreePath, используются ссылки на строки таблицы DataRow.
  /// </summary>
  public class DataTableTreeModel : TreeModelBase, IDataTableTreeModel
  {
    #region Конструкторы

    /// <summary>
    /// Создает модель на основе DataTable
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="idColumnName">Имя ключевого столбца, например, "Id"</param>
    /// <param name="parentColumnName">Имя столбца родительского идентификатора, который образует древовидную структуру, например, "ParentId"</param>
    public DataTableTreeModel(DataTable table, string idColumnName, string parentColumnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(idColumnName))
        throw new ArgumentNullException("idColumnName");
      if (String.IsNullOrEmpty(parentColumnName))
        throw new ArgumentNullException("parentColumnName");

      int p1 = table.Columns.IndexOf(idColumnName);
      if (p1 < 0)
        throw new ArgumentException("Столбец \"" + idColumnName + "\" не принадлежит таблице \"" + table.TableName + "\"", "idColumnName");
      int p2 = table.Columns.IndexOf(parentColumnName);
      if (p2 < 0)
        throw new ArgumentException("Столбец \"" + parentColumnName + "\" не принадлежит таблице \"" + table.TableName + "\"", "parentColumnName");
      if (p2 == p1)
        throw new ArgumentException("Столбцы idColumnName и parentColumnName не могут совпадать");
#endif

      _Table = table;
      _IdColumnName = idColumnName;
      _ParentColumnName = parentColumnName;

      if (_Table.Columns[parentColumnName].DataType != _Table.Columns[idColumnName].DataType)
        throw new ArgumentException("Столбец \"" + idColumnName + "\" имеет тип данных " + _Table.Columns[idColumnName].DataType.ToString() + ", а \"" + parentColumnName + "\" - " +
          _Table.Columns[parentColumnName].DataType.ToString(), "parentColumnName");
      if (!_Table.Columns[parentColumnName].AllowDBNull)
        throw new ArgumentNullException("Столбец \"" + parentColumnName + "\" имеет свойство AllowDBNull=false", "parentColumnName");

      _Sort = String.Empty;
      _IsNullDefaultValue = DataTools.GetEmptyValue(_Table.Columns[idColumnName].DataType);

      // 30.11.2015
      _Table.RowChanged += new DataRowChangeEventHandler(Table_RowChanged);
      // 09.12.2015 _Table.RowDeleted += new DataRowChangeEventHandler(Table_RowDeleted);
      _Table.RowDeleting += new DataRowChangeEventHandler(Table_RowDeleting);
      _Table.Initialized += new EventHandler(Table_Initialized);
      _Table.TableCleared += new DataTableClearEventHandler(Table_TableCleared);

      // Для отслеживания изменений в столбце parentColumnName надо запомнить текущие значения, если в таблице есть измененные строки.
      // Нельзя будет просто использовать RowVersion.Original, т.к. таблица уже может содержать строки в состоянии Modified на момент вызова конструктора.
      // А для строк с DataRowState=Added нет исходного значения.
      // Когда будет вызвано событие RowChanged, уже нельзя будет узнать предыдущее значение
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState != DataRowState.Unchanged)
        {
          if (_PrevParentKeys == null)
            _PrevParentKeys = new Hashtable();
          _PrevParentKeys[row[IdColumnName]] = row[ParentColumnName];
        }
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Таблица данных. Задается в конструкторе.
    /// К таблице присоединяется множество обработчиков событий, поэтому не следует использовать этот объект
    /// DataTable многократно
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;

    DataView IDataTableTreeModel.DataView { get { return null; } }

    /// <summary>
    /// Имя ключевого поля в таблице
    /// </summary>
    public string IdColumnName { get { return _IdColumnName; } }
    private string _IdColumnName;

    /// <summary>
    /// Имя поля в таблице, по которому строится дерево
    /// </summary>
    public string ParentColumnName { get { return _ParentColumnName; } }
    private string _ParentColumnName;

    /// <summary>
    /// Порядок сортировки строк (в формате аргумента sort метода DataTable.Select()).
    /// По умолчанию узлы дерева одного уровня идут в порядке следования строк в таблице.
    /// </summary>
    public string Sort
    {
      get { return _Sort; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Sort = value;
      }
    }
    private string _Sort;

    /// <summary>
    /// Значение, используемое в качестве второго аргумента при вызове функции ISNULL() (см. справку к свойству DataColumn.Expression).
    /// Не может быть null, должно иметь тот же тип, что и DataColumn.DataType.
    /// По умолчанию используется нулевое значение или "" (для строковых полей).
    /// </summary>
    public object IsNullDefaultValue
    {
      get { return _IsNullDefaultValue; }
      set
      {
#if DEBUG
        if (value == null)
          throw new ArgumentNullException();
#endif
        if (value.GetType() != _Table.Columns[_IdColumnName].DataType)
          throw new ArgumentException("Значение должно иметь тип " + _Table.Columns[_IdColumnName].DataType.ToString());
      }
    }
    private object _IsNullDefaultValue;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает перечислитель дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к родительскому узлу</param>
    /// <returns>Перечислитель</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      if (treePath.IsEmpty)
      {
        DataRow[] rows = _Table.Select(GetIsNullExpression(ParentColumnName), Sort);
        return rows;
      }
      else
      {
        DataRow parentRow = TreePathToDataRow(treePath);
        if (parentRow == null)
          return new DataRow[0]; // 27.12.2020
        object parentValue = parentRow[IdColumnName];
        DataRow[] rows = _Table.Select(GetEqExpression(ParentColumnName, parentValue), Sort);
        return rows;
      }
    }

    private string GetEqExpression(string columnName, object value)
    {
      if (value == null || value is DBNull)
        return GetIsNullExpression(columnName);
      else
        return columnName + "=" + DataTools.FormatDataValue(value);
    }

    private string GetIsNullExpression(string columnName)
    {
      return "ISNULL(" + columnName + "," + DataTools.FormatDataValue(_IsNullDefaultValue) + ")=" + DataTools.FormatDataValue(_IsNullDefaultValue) + "";
    }

    private DataRow GetDataRowWithCheck(object tag)
    {
      DataRow row = tag as DataRow;
      if (row == null)
      {
        if (tag == null)
          throw new ArgumentNullException("tag");
        else
          throw new ArgumentException("Аргумент tag не является DataRow", "tag");
      }
      if (row.Table != _Table)
        throw new ArgumentException("Строка относится к другой таблице", "tag");
      return row;
    }

    /// <summary>
    /// Возвращает true, если заданный узел не имеет дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к проверяемому узлу</param>
    /// <returns>Признак наличия дочерних узлов</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      // TODO: Будет медленно работать
      // Надо бы загружать полный список дочерних элементов второго уровня и запоминать флажки в словаре
      DataRow row = TreePathToDataRow(treePath);
      if (row == null)
        return true; // 27.12.2020
      object value = row[IdColumnName];
      DataRow[] rows = _Table.Select(GetEqExpression(ParentColumnName, value));
      return rows.Length == 0;
    }

    #endregion

    #region Обработка событий DataTable

    void Table_TableCleared(object sender, DataTableClearEventArgs args)
    {
      base.OnStructureChanged(TreePathEventArgs.Empty);
      _PrevParentKeys = null;
    }

    void Table_Initialized(object sender, EventArgs args)
    {
      base.OnStructureChanged(TreePathEventArgs.Empty);
      _PrevParentKeys = null;
    }

    void Table_RowDeleting(object sender, DataRowChangeEventArgs args)
    {
      object parentKey = args.Row[ParentColumnName];
      TreeModelEventArgs args2 = new TreeModelEventArgs(TreePathFromKey(parentKey), new object[] { args.Row });
      base.OnNodesRemoved(args2);
    }

    /// <summary>
    /// Для одной и той же строки в процессе редактирования может многократно меняться значение поля ParentColumnName.
    /// Для первого изменения можно использовать начальное значение поля строки. 
    /// При повторных изменениях уже нельзя узнать, какое значение было до изменения.
    /// 
    /// Коллекция хранит предыдущее значение для строки.
    /// Ключом является IdColumnName строки (которое не может меняться).
    /// Значением является предыдущее значение ParentColumnName (может быть DBNull)
    /// </summary>
    private Hashtable _PrevParentKeys;

    void Table_RowChanged(object sender, DataRowChangeEventArgs args)
    {
      object parentKey = args.Row[ParentColumnName];
      switch (args.Action)
      {
        case DataRowAction.Change:
          if (_PrevParentKeys == null)
            _PrevParentKeys = new Hashtable();
          object prevParentKey = parentKey;
          if (_PrevParentKeys.ContainsKey(args.Row[IdColumnName]))
            prevParentKey = _PrevParentKeys[args.Row[IdColumnName]];
          else
          {
            switch (args.Row.RowState)
            {
              case DataRowState.Modified:
                prevParentKey = args.Row[IdColumnName, DataRowVersion.Original];
                break;
            }
          }
          _PrevParentKeys[args.Row[IdColumnName]] = parentKey; // запоминаем до следующего вызова

          if (!parentKey.Equals(prevParentKey))
          {
            if ((prevParentKey is DBNull) || (parentKey is DBNull))
            {
              // Если узел пришел или ушел из корня дерева, надо выполнить полное обновление
              OnStructureChanged(TreePathEventArgs.Empty);
            }
            else
            {
              TreePath treePath1 = TreePathFromKey(parentKey);
              TreePath treePath2 = TreePathFromKey(prevParentKey);
              // Можно определить, не является ли один родительский узел дочерним по отношению к другому.
              // В этом случае достаточно одного обновления, а не двух
              base.CallStructureChanged(treePath1, treePath2);
            }
          }

          TreeModelEventArgs args2 = new TreeModelEventArgs(TreePathFromKey(parentKey), new object[] { args.Row });
          base.OnNodesChanged(args2);
          break;

        case DataRowAction.Add:
          int[] indices = new int[1] { 0 }; // !!!!
          TreeModelEventArgs args3 = new TreeModelEventArgs(TreePathFromKey(parentKey), indices, new object[] { args.Row });
          base.OnNodesInserted(args3);
          break;
      }
    }

    #endregion

    #region Доступ к DataRow

    /// <summary>
    /// Возвращает строку данных, соответствующую заданному пути
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Объект DataRow из таблицы данных</returns>
    public DataRow TreePathToDataRow(TreePath path)
    {
      if (path.LastNode == null)
        return null;
      DataRow row = path.LastNode as DataRow;
      if (row == null)
        throw new InvalidCastException("Аргумент treePath.LastNode не является DataRow");
      if (row.Table != _Table)
        throw new ArgumentException("Строка относится к другой таблице");
      return row;
    }

    /// <summary>
    /// Возвращает путь в дереве, соответствующий строке таблицы данных
    /// </summary>
    /// <param name="row">Строка присоединенной таблицы данных</param>
    /// <returns>Путь в дереве</returns>
    public TreePath TreePathFromDataRow(DataRow row)
    {
      if (row == null)
        return TreePath.Empty;

      if (row.Table != _Table)
        throw new ArgumentException("Строка не принадлежит таблице данных " + Table.TableName, "row");
      if (row.RowState == DataRowState.Detached)
        throw new ArgumentException("Строка отсоединена от таблицы данных" + Table.TableName, "row");

      object parentId = row[ParentColumnName];
      if (parentId is DBNull)
        // строка верхнего уровня
        return new TreePath(new object[] { row });
      ArrayList lst = new ArrayList();
      lst.Insert(0, row);

      if (UsePrimaryKey)
      {
        while (!(parentId is DBNull))
        {
          row = _Table.Rows.Find(parentId);
          if (row == null)
            throw new InvalidOperationException("В таблице " + _Table.TableName + " не найдена строка с идентификатором " + DataTools.GetString(parentId));
          parentId = row[ParentColumnName];

          if (lst.Contains(row))
            throw new InvalidOperationException("Дерево зациклено для строки с идентификатором " + row[IdColumnName].ToString());
          lst.Insert(0, row);
        }
      }
      else
      {
        using (DataView dv = new DataView(_Table))
        {
          dv.Sort = IdColumnName;
          while (!(parentId is DBNull))
          {
            int p = dv.Find(parentId);
            if (p < 0)
              throw new InvalidOperationException("В таблице " + _Table.TableName + " не найдена строка с идентификатором " + DataTools.GetString(parentId));
            row = dv[p].Row;
            parentId = row[ParentColumnName];

            if (lst.Contains(row))
              throw new InvalidOperationException("Дерево зациклено для строки с идентификатором " + row[IdColumnName].ToString());
            lst.Insert(0, row);
          }
        }
      }
      return new TreePath(lst.ToArray());
    }

    /// <summary>
    /// Возвращает массив строк, являющихся дочерними по отношении к заданной строке (нерекурсивно).
    /// </summary>
    /// <param name="parentRow">Родительская строка. Если null, то возвращаются строки верхнего уровня</param>
    /// <returns>Массив строк</returns>
    public DataRow[] GetChildRows(DataRow parentRow)
    {
      if (parentRow == null)
      {
        DataRow[] rows = _Table.Select(GetIsNullExpression(ParentColumnName), Sort);
        return rows;
      }
      else
      {
        object parentValue = parentRow[IdColumnName];
        DataRow[] rows = _Table.Select(GetEqExpression(ParentColumnName, parentValue), Sort);
        return rows;
      }
    }


    #endregion

    #region Доступ к строке по ключевому полю

    /// <summary>
    /// Возвращает идентификатор (значение поля IdColumnNames), соответствующее заданному пути.
    /// Возвращает DBNuill, если строка в таблице не найдена, или <paramref name="path"/>.IsEmpty=true.
    /// Этот метод можно применять для идентификаторов любых типов.
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    public object TreePathToKey(TreePath path)
    {
      DataRow row = TreePathToDataRow(path);
      if (row == null)
        return DBNull.Value;
      else
        return row[IdColumnName];
    }

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// Этот метод можно применять для идентификаторов любых типов.
    /// </summary>
    /// <param name="key">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    public TreePath TreePathFromKey(object key)
    {
      if (key == null || key is DBNull)
        return TreePath.Empty;

      DataRow row;
      if (UsePrimaryKey)
        row = _Table.Rows.Find(key);
      else
      {
        using (DataView dv = new DataView(_Table))
        {
          dv.Sort = IdColumnName;
          int p = dv.Find(key);
          if (p >= 0)
            row = dv[p].Row;
          else
            row = null;
        }
      }

      return TreePathFromDataRow(row);
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Возвращает true, если в таблице установлен первичный ключ по полю IdColumnName
    /// </summary>
    public bool UsePrimaryKey
    {
      get
      {
        if (_Table.PrimaryKey.Length != 1)
          return false;
        return String.Equals(_Table.PrimaryKey[0].ColumnName, IdColumnName, StringComparison.OrdinalIgnoreCase);
      }
    }

    /// <summary>
    /// Возвращает массив строк, являющихся рекурсивно дочерними по отношению к заданному элементу.
    /// </summary>
    /// <param name="treeItem"></param>
    /// <returns></returns>
    protected DataRow[] GetRowsWithChildren(object treeItem)
    {
      if (treeItem == null)
        return new DataRow[0];

      List<DataRow> lst = new List<DataRow>();
      lst.Add(GetDataRowWithCheck(treeItem));
      DoAddChildRows(lst, lst[0]);
      return lst.ToArray();
    }

    /// <summary>
    /// Рекурсивная процедура
    /// </summary>
    /// <param name="lst"></param>
    /// <param name="parentRow"></param>
    private void DoAddChildRows(List<DataRow> lst, DataRow parentRow)
    {
      object parentValue = parentRow[IdColumnName];
      DataRow[] rows = _Table.Select(GetEqExpression(ParentColumnName, parentValue));
      for (int i = 0; i < rows.Length; i++)
      {
        lst.Add(rows[i]);
        DoAddChildRows(lst, rows[i]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Источник просмотра древовидной структуры из таблицы DataTable.
  /// Предполагается, что имеется поле (IdColumnName) типа <typeparamref name="T"/>, идентифицирующее строки (обычно поле является первичным ключом,
  /// но это не является обязательным условием). Также имеется ссылочное поле (ParentColumnName), используемое для построения дерева.
  /// Поле ParentColumnName тоже должно иметь тип <typeparamref name="T"/> и обязательно поддерживать значение NULL, которое идентифицирует строки верхнего уровня.
  /// </summary>
  /// <typeparam name="T">Тип идентификатора (числовой, String, DateTime, Guid)</typeparam>
  public class DataTableTreeModelWithIds<T> : DataTableTreeModel, ITreeModelWithIds<T>
    where T : IEquatable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает модель на основе DataTable
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="idColumnName">Имя ключевого столбца, например, "Id"</param>
    /// <param name="parentColumnName">Имя столбца родительского идентификатора, который образует древовидную структуру, например, "ParentId"</param>
    public DataTableTreeModelWithIds(DataTable table, string idColumnName, string parentColumnName)
      : base(table, idColumnName, parentColumnName)
    {
      if (Table.Columns[idColumnName].DataType != typeof(T))
        throw new ArgumentException("Столбец идентификатора должен иметь тип " + typeof(T).ToString());
    }

    #endregion

    #region Доступ к строке по идентификатору

    /// <summary>
    /// Возвращает идентификатор (значение поля IdColumnName), соответствующее заданному пути.
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    public T TreePathToId(TreePath path)
    {
      DataRow row = TreePathToDataRow(path);
      if (row == null)
        return (T)IsNullDefaultValue;
      else
        return (T)(row[IdColumnName]);
    }

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// </summary>
    /// <param name="id">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    public TreePath TreePathFromId(T id)
    {
      if (id.Equals(IsNullDefaultValue))
        return TreePath.Empty;

      DataRow row;
      if (UsePrimaryKey)
        row = Table.Rows.Find(id);
      else
      {
        using (DataView dv = new DataView(Table))
        {
          dv.Sort = IdColumnName;
          int p = dv.Find(id);
          if (p >= 0)
            row = dv[p].Row;
          else
            row = null;
        }
      }

      return TreePathFromDataRow(row);
    }

    /// <summary>
    /// Возвращает идентификатор для строки.
    /// Возвращает IsNullDefaultValue, если <paramref name="row"/>==null.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <returns>Идентификатор</returns>
    public T DataRowToId(DataRow row)
    {
      if (row == null)
        return (T)IsNullDefaultValue;
      else
        return (T)(row[IdColumnName]);
    }

    /// <summary>
    /// Возвращает строку для заданного идентификатора.
    /// Возвращает null при <paramref name="id"/>==IsNullDefaultValue или если идентификатор не найден.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Строка в таблице или null</returns>
    public DataRow DataRowFromId(T id)
    {
      if (id.Equals(IsNullDefaultValue))
        return null;
      else
      {
        DataRow Row;
        if (UsePrimaryKey)
          Row = Table.Rows.Find(id);
        else
        {
          using (DataView dv = new DataView(Table))
          {
            dv.Sort = IdColumnName;
            int p = dv.Find(id);
            if (p >= 0)
              Row = dv[p].Row;
            else
              Row = null;
          }
        }
        return Row;
      }
    }

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName) дочерних узлов, для заданного родительского идентификатора нерекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку узлов данного уровня.
    /// Используется метод GetChildRows().
    /// </summary>
    /// <param name="id">Идентификатор родительского узла.
    /// Если 0, то будут возвращены идентификаторы строк верхнего уровня</param>
    /// <returns>Массив идентификаторов</returns>
    public T[] GetChildIds(T id)
    {
      DataRow[] rows = GetChildRows(DataRowFromId(id));
      T[] ids = new T[rows.Length];
      for (int i = 0; i < rows.Length; i++)
        ids[i] = DataRowToId(rows[i]);
      return ids;
    }

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), для заданного родительского идентификатора
    /// и всем его вложенным узлам рекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="id">Идентификатор корневого узла. Если 0, возвращаются все идентификаторы в таблице</param>
    /// <returns>Массив идентификаторов</returns>
    public T[] GetIdWithChildren(T id)
    {
      //return DataRowToIdWithChildren(DataRowFromId(Id));
      if (id.Equals(IsNullDefaultValue))
        return DataRowToIdWithChildren(null);
      else
      {
        DataRow row = DataRowFromId(id);
        if (row == null)
          return new T[1] { id }; // 10.06.2019
        else
          return DataRowToIdWithChildren(row);
      }
    }


    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), соответствующее заданному пути
    /// и всем его вложенным узлам рекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="path">Путь к узлу дерева. Если путь пустой, возвращаются все идентификаторы в таблице</param>
    /// <returns>Массив идентификаторов</returns>
    public T[] TreePathToIdWithChildren(TreePath path)
    {
      SingleScopeList<T> ids = new SingleScopeList<T>();
      DataRow row = TreePathToDataRow(path);
      DoAddIdWithChildren(ids, row);
      return ids.ToArray();
    }

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), соответствующее заданному пути
    /// и всем его вложенным узлам рекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="row">Строка в таблице. Если задана пустая строка, возвращаются все идентификаторы</param>
    /// <returns>Массив идентификаторов</returns>
    public T[] DataRowToIdWithChildren(DataRow row)
    {
      SingleScopeList<T> ids = new SingleScopeList<T>();
      DoAddIdWithChildren(ids, row);
      return ids.ToArray();
    }

    private void DoAddIdWithChildren(SingleScopeList<T> ids, DataRow row)
    {
      if (row != null)
      {
        T id = (T)(row[IdColumnName]);
        if (ids.Contains(id))
          return; // Ошибка - дерево зациклено
        ids.Add(id);
      }

      DataRow[] childRows = GetChildRows(row);
      for (int i = 0; i < childRows.Length; i++)
        DoAddIdWithChildren(ids, childRows[i]);
    }

    #endregion
  }
}