// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Data;

namespace FreeLibSet.Models.Tree
{
  /// <summary>
  /// Расширение интерфейса <see cref="ITreeModel"/>, связанного с <see cref="System.Data.DataTable"/> или <see cref="System.Data.DataView"/>.
  /// Объявляет методы для преобразования строки таблицы данных <see cref="System.Data.DataRow"/> в <see cref="TreePath"/> и обратно.
  /// Интерфейс не определяет факт наличия первичного ключа в таблице и типы полей, используемых для построения дерева.
  /// Также этот интерфейс, как и <see cref="ITreeModel"/>, не определяет, что используется в качестве частей <see cref="TreePath"/>.
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
    /// <returns>Объект <see cref="DataRow"/> из таблицы данных</returns>
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
  /// Источник просмотра древовидной структуры из таблицы <see cref="System.Data.DataTable"/>.
  /// Предполагается, что имеется поле (IdColumnName), идентифицирующее строки (обычно поле является первичным ключом,
  /// но это не является обязательным условием). Также имеется ссылочное поле (ParentColumnName), используемое для построения дерева.
  /// Поле IdColumnName может быть числовым, строковым или иметь тип <see cref="Guid"/> или <see cref="DateTime"/>, лишь бы для этого поля можно было вычислить выражение с помощью <see cref="System.Data.DataTable.Select(string)"/>.
  /// Поле ParentColumnName должно иметь тот же тип, но обязательно поддерживать значение NULL, которое идентифицирует строки верхнего уровня.
  /// В процессе работы могут добавляться, изменяться или удаляться строки. 
  /// Значение поля IdColumnName не должно меняться.
  /// Значение поля ParentColumnName может меняться, при этом вызывается событие StructureChanged.
  /// 
  /// В текущей реализации в качестве тегов, входящих в <see cref="TreePath"/>, используются ссылки на строки таблицы <see cref="DataRow"/>.
  /// 
  /// Класс реализует интерфейс <see cref="IDisposable"/>. К таблице <see cref="DataTable"/> присоединяются обработчики событий и могут создаваться внутренние
  /// объекты <see cref="DataView"/>. Чтобы отцепить обработчики, когда модель больше не используется, вызовите <see cref="IDisposable.Dispose()"/> или
  /// используйте экземпляр <see cref="DataTableTreeModel"/> внутри блока using. В обычных сценариях, когда таблица создается специально для модели, вызывать <see cref="IDisposable.Dispose()"/> не нужно.
  /// </summary>
  public class DataTableTreeModel : TreeModelBase, IDataTableTreeModel, IDisposable
  {
    #region Конструктор и Dispose

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
        throw ExceptionFactory.ArgStringIsNullOrEmpty("idColumnName");
      if (String.IsNullOrEmpty(parentColumnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("parentColumnName");
#endif

      _Table = table;
      _IdColumnPosition = table.Columns.IndexOf(idColumnName);
      _ParentColumnPosition = table.Columns.IndexOf(parentColumnName);

#if DEBUG
      if (_IdColumnPosition < 0)
        throw ExceptionFactory.ArgUnknownColumnName("idColumnName", table, idColumnName);
      if (_ParentColumnPosition < 0)
        throw ExceptionFactory.ArgUnknownColumnName("parentColumnName", table, parentColumnName);
      if (_ParentColumnPosition == _IdColumnPosition)
        throw ExceptionFactory.ArgAreSame("idColumnName", "parentColumnName");
#endif

      if (_Table.Columns[_ParentColumnPosition].DataType != _Table.Columns[_IdColumnPosition].DataType)
        throw new ArgumentException(String.Format(Res.DataTableTreeModel_Arg_DiffColumnTypes,
          idColumnName, _Table.Columns[_IdColumnPosition].DataType.ToString(),
          parentColumnName, _Table.Columns[parentColumnName].DataType.ToString()), "parentColumnName");
      if (!_Table.Columns[_ParentColumnPosition].AllowDBNull)
        throw new ArgumentException(String.Format(Res.DataTableTreeModel_Arg_ParentNotNull,
          parentColumnName), "parentColumnName");

      //_Sort = String.Empty;
      //_SortColumnPositions = DataTools.EmptyInts;
      this.Sort = table.DefaultView.Sort; // 28.05.2022

      _IsNullDefaultValue = DataTools.GetEmptyValue(_Table.Columns[idColumnName].DataType);

      if (table.PrimaryKey.Length == 1)
        _UsePrimaryKey = String.Equals(_Table.PrimaryKey[0].ColumnName, IdColumnName, StringComparison.OrdinalIgnoreCase);

      // 30.11.2015
      _Table.Initialized += new EventHandler(Table_Initialized);
      _Table.ColumnChanging += Table_ColumnChanging;
      _Table.RowChanged += new DataRowChangeEventHandler(Table_RowChanged);
      // 09.12.2015 _Table.RowDeleted += new DataRowChangeEventHandler(Table_RowDeleted);
      _Table.RowDeleting += new DataRowChangeEventHandler(Table_RowDeleting);
      _Table.TableCleared += new DataTableClearEventHandler(Table_TableCleared);
    }

    /// <summary>
    /// Отсоединяет обработчики событий DataTable, присоединенные в конструкторе
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
    }

    /// <summary>
    /// Выполняет отсоединение обработчиков событий
    /// </summary>
    /// <param name="disposing">Всегда true</param>
    protected virtual void Dispose(bool disposing)
    {
      _Table.Initialized -= new EventHandler(Table_Initialized);
      _Table.ColumnChanging += Table_ColumnChanging;
      _Table.RowChanged -= new DataRowChangeEventHandler(Table_RowChanged);
      _Table.RowDeleting -= new DataRowChangeEventHandler(Table_RowDeleting);
      _Table.TableCleared -= new DataTableClearEventHandler(Table_TableCleared);
      _PrevParentKeys = null;

      if (_InternalDataViewByIdColumn != null)
      {
        _InternalDataViewByIdColumn.Dispose();
        _InternalDataViewByIdColumn = null;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Таблица данных. Задается в конструкторе.
    /// К таблице присоединяется множество обработчиков событий, поэтому не следует использовать этот объект
    /// DataTable многократно.
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;

    DataView IDataTableTreeModel.DataView { get { return null; } }

    /// <summary>
    /// Имя ключевого поля в таблице
    /// </summary>
    public string IdColumnName { get { return _Table.Columns[_IdColumnPosition].ColumnName; } }

    /// <summary>
    /// Позиция ключевого столбца IdColumnName в таблице Table.
    /// </summary>
    public int IdColumnPosition { get { return _IdColumnPosition; } }
    private int _IdColumnPosition;

    /// <summary>
    /// Имя поля в таблице, по которому строится дерево
    /// </summary>
    public string ParentColumnName { get { return _Table.Columns[ParentColumnPosition].ColumnName; } }

    /// <summary>
    /// Позиция ссылочного столбца PatentColumnName в таблице Table.
    /// </summary>
    public int ParentColumnPosition { get { return _ParentColumnPosition; } }
    private int _ParentColumnPosition;

    /// <summary>
    /// Порядок сортировки строк (в формате аргумента sort метода DataTable.Select()).
    /// По умолчанию используется порядок сортировки в просмотре DataView (Table.DefaultView.Sort).
    /// Дальнейшее изменение в свойстве Table.DefaultView.Sort не отслеживается.
    /// </summary>
    public string Sort
    {
      get { return _Sort; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value.Length == 0)
          _SortColumnPositions = EmptyArray<Int32>.Empty;
        else
        {
          string[] aColNames = DataTools.GetDataViewSortColumnNames(value);
          int[] a = new int[aColNames.Length];
          for (int i = 0; i < aColNames.Length; i++)
          {
            a[i] = Table.Columns.IndexOf(aColNames[i]);
            if (a[i] < 0)
              throw ExceptionFactory.ArgUnknownColumnName("value", Table, aColNames[i]);
          }
          _SortColumnPositions = a; // только, если не возникло исключения
        }
        _Sort = value;
      }
    }
    private string _Sort;

    /// <summary>
    /// Позиции столбцов в таблице, которые используются для сортировки (свойство Sort).
    /// Если Sort-пустая строка, то содержит пустой массив
    /// </summary>
    private int[] _SortColumnPositions; // 28.05.2022

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
        if (value.GetType() != _Table.Columns[IdColumnPosition].DataType)
          throw new ArgumentException(String.Format(Res.DataTableTreeModel_Arg_IsNullDefaultValueType, 
            _Table.Columns[IdColumnPosition].DataType.ToString(), value.GetType().ToString()));
        _IsNullDefaultValue = value;
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
          return EmptyArray<DataRow>.Empty; // 27.12.2020
        object parentValue = parentRow[IdColumnPosition];
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
          throw new ArgumentException(Res.DataTableTreeModel_Arg_TagType, "tag");
      }
      if (row.Table != _Table)
        throw  new ArgumentException(Res.DataTableTreeModel_Arg_TagAnotherTable, "tag");
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
      object value = row[IdColumnPosition];
      DataRow[] rows = _Table.Select(GetEqExpression(ParentColumnName, value));
      return rows.Length == 0;
    }

    #endregion

    #region Begin/EndUpdate()

    /// <summary>
    /// Счетчик BeginUpdate()
    /// </summary>
    private int _UpdateSuspendCount;

    /// <summary>
    /// Для отслеживания необходимости вызова события StructureChanged в EndUpdate()
    /// </summary>
    private bool _TableChanged;

    /// <summary>
    /// Временно отключает генерацию событий модели до вызова EndUpdate().
    /// Вызовы могут быть вложенными
    /// </summary>
    public void BeginUpdate()
    {
      if (_UpdateSuspendCount == 0)
        _TableChanged = false;
      _UpdateSuspendCount++;
    }

    /// <summary>
    /// Возобновить генерацию событий модели, если больше нет вложенных вызовов BeginUpdate().
    /// Если были изменения в таблице, вызывается событие StructureChanged для всего дерева
    /// </summary>
    public void EndUpdate()
    {
      if (_UpdateSuspendCount == 0)
        throw ExceptionFactory.UnpairedCall(this, "BeginUpdate()", "EndUpdate()");
      _UpdateSuspendCount--;
      if (_UpdateSuspendCount == 0 && _TableChanged)
        OnStructureChanged(TreePathEventArgs.Empty);
    }

    #endregion

    #region Обработка событий DataTable

    void Table_Initialized(object sender, EventArgs args)
    {
      _TableChanged = true;
      if (_UpdateSuspendCount > 0)
        return;

      base.OnStructureChanged(TreePathEventArgs.Empty);
      _PrevParentKeys = null;
    }


    /// <summary>
    /// Для одной и той же строки в процессе редактирования может многократно меняться значение поля ParentColumnName.
    /// Для первого изменения можно использовать начальное значение поля строки. 
    /// При повторных изменениях уже нельзя узнать, какое значение было до изменения.
    /// 
    /// Коллекция хранит предыдущее значение для строки.
    /// Ключом является IdColumnName строки (которое не может меняться).
    /// Значением является предыдущее значение ParentColumnName (может быть DBNull).
    /// Значение запоминается в коллекции при вызове события ColumnChanging и используется в RowChanged.
    /// Так как, теоретически, одновременно могут меняться несколько строк (рекурсия), используем коллекцию, а не единственную пару значений
    /// </summary>
    private Hashtable _PrevParentKeys;

    /// <summary>
    /// Отслеживание изменений в полях, участвующих в сортировке
    /// Ключом является IdColumnName строки. Значение не используется (всегда null).
    /// Если изменение происходит с одним из полей, входящих в свойство Sort, идентификатор строки добавляется в коллекцию.
    /// Когда происходит событие RowChanged, если для строки есть запись в коллекции, выполняется обновление структуры родительского узла 
    /// (или всего дерева для строки верхнего уровня), вместо посылки события NodesChanged.
    /// </summary>
    private Hashtable _SortColumnKeys; // 28.05.2022

    void Table_ColumnChanging(object sender, DataColumnChangeEventArgs args)
    {
      if (_UpdateSuspendCount > 0)
        return;

      if (Object.ReferenceEquals(args.Column, Table.Columns[ParentColumnPosition]))
      {
        if (_PrevParentKeys == null)
          _PrevParentKeys = new Hashtable();

        _PrevParentKeys[args.Row[IdColumnPosition]] = args.Row[ParentColumnPosition];
      }
      else
      {
        for (int i = 0; i < _SortColumnPositions.Length; i++)
        {
          if (Object.ReferenceEquals(args.Column, Table.Columns[_SortColumnPositions[i]]))
          {
            if (_SortColumnKeys == null)
              _SortColumnKeys = new Hashtable();
            _SortColumnKeys[args.Row[IdColumnPosition]] = null;
          }
        }
      }
    }

    void Table_RowChanged(object sender, DataRowChangeEventArgs args)
    {
      _TableChanged = true;
      if (_UpdateSuspendCount > 0)
        return;

      object parentKey = args.Row[ParentColumnPosition];
      switch (args.Action)
      {
        case DataRowAction.Add:
          int[] indices = new int[1] { 0 }; // !!!!
          TreeModelEventArgs args3 = new TreeModelEventArgs(TreePathFromKey(parentKey), indices, new object[] { args.Row });
          base.OnNodesInserted(args3);
          break;

        case DataRowAction.Change:
          object prevParentKey = parentKey;
          if (_PrevParentKeys != null)
          {
            if (_PrevParentKeys.ContainsKey(args.Row[IdColumnPosition]))
            {
              prevParentKey = _PrevParentKeys[args.Row[IdColumnPosition]];
              _PrevParentKeys.Remove(args.Row[IdColumnPosition]); // не загромождаем коллекцию
            }
          }

          bool sortColumnChanged = false;
          if (_SortColumnKeys != null)
          {
            if (_SortColumnKeys.ContainsKey(args.Row[IdColumnPosition]))
            {
              sortColumnChanged = true;
              _SortColumnKeys.Remove(args.Row[IdColumnPosition]);
            }
          }

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
          else if (sortColumnChanged) // 28.05.2022
          {
            TreePath treePath = TreePathFromKey(parentKey);
            if (treePath.IsEmpty)
              OnStructureChanged(TreePathEventArgs.Empty);
            else
              OnStructureChanged(new TreePathEventArgs(treePath));
          }
          else
          {
            TreeModelEventArgs args2 = new TreeModelEventArgs(TreePathFromKey(parentKey), new object[] { args.Row });
            base.OnNodesChanged(args2);
          }
          break;
      }
    }

    void Table_RowDeleting(object sender, DataRowChangeEventArgs args)
    {
      _TableChanged = true;
      if (_UpdateSuspendCount > 0)
        return;

      object parentKey = args.Row[ParentColumnPosition];
      TreeModelEventArgs args2 = new TreeModelEventArgs(TreePathFromKey(parentKey), new object[] { args.Row });
      base.OnNodesRemoved(args2);
    }

    void Table_TableCleared(object sender, DataTableClearEventArgs args)
    {
      _TableChanged = true;
      if (_UpdateSuspendCount > 0)
        return;

      base.OnStructureChanged(TreePathEventArgs.Empty);
      _PrevParentKeys = null;
    }

    #endregion

    #region Доступ к DataRow

    /// <summary>
    /// Внутренний DataView, отсортированный по IdColumnName.
    /// Используется при UsePrimaryKey=false.
    /// DataView cоздается при первом обращении и удаляется методом Dispose().
    /// Сделан internal, так как используется также в классе-наследнике DataTableTreeModelWithIds.
    /// </summary>
    internal DataView InternalDataViewByIdColumn
    {
      get
      {
#if DEBUG
        if (UsePrimaryKey)
          throw new BugException("UsePrimaryKey=true");
#endif

        if (_InternalDataViewByIdColumn == null)
        {
          _InternalDataViewByIdColumn = new DataView(Table);
          _InternalDataViewByIdColumn.Sort = IdColumnName;
        }
        return _InternalDataViewByIdColumn;
      }
    }
    private DataView _InternalDataViewByIdColumn;

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
        throw new InvalidCastException(Res.DataTableTreeModel_Arg_TagType);
      if (row.Table != _Table)
        throw new ArgumentException(Res.DataTableTreeModel_Arg_TagAnotherTable, "path");
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
        throw ExceptionFactory.ArgDataRowNotInSameTable("row", row, Table);
      if (row.RowState == DataRowState.Detached)
        throw ExceptionFactory.ArgProperty("row", row, "RowState", row.RowState, null);

      object parentId = row[ParentColumnPosition];
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
            throw ExceptionFactory.DataRowNotFound(_Table, new object[] { parentId} );
          parentId = row[ParentColumnPosition];

          if (lst.Contains(row))
            throw new InvalidOperationException(String.Format(Res.DataTableTreeModel_Err_Loop, row[IdColumnPosition]));
          lst.Insert(0, row);
        }
      }
      else
      {
        while (!(parentId is DBNull))
        {
          int p = InternalDataViewByIdColumn.Find(parentId);
          if (p < 0)
            throw ExceptionFactory.DataRowNotFound(_Table, new object[] { parentId });
          row = InternalDataViewByIdColumn[p].Row;
          parentId = row[ParentColumnPosition];

          if (lst.Contains(row))
            throw new InvalidOperationException(String.Format(Res.DataTableTreeModel_Err_Loop, row[IdColumnPosition]));
          lst.Insert(0, row);
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
        object parentValue = parentRow[IdColumnPosition];
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
        return row[IdColumnPosition];
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
        int p = InternalDataViewByIdColumn.Find(key);
        if (p >= 0)
          row = InternalDataViewByIdColumn[p].Row;
        else
          row = null;
      }

      return TreePathFromDataRow(row);
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Возвращает true, если в таблице установлен первичный ключ по полю IdColumnName
    /// </summary>
    public bool UsePrimaryKey { get { return _UsePrimaryKey; } }
    private bool _UsePrimaryKey;

    /// <summary>
    /// Возвращает массив строк, являющихся рекурсивно дочерними по отношению к заданному элементу.
    /// </summary>
    /// <param name="treeItem"></param>
    /// <returns></returns>
    protected DataRow[] GetRowsWithChildren(object treeItem)
    {
      if (treeItem == null)
        return EmptyArray<DataRow>.Empty;

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
      object parentValue = parentRow[IdColumnPosition];
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
  /// Источник просмотра древовидной структуры из таблицы <see cref="DataTable"/>.
  /// Предполагается, что имеется поле (IdColumnName) типа <typeparamref name="T"/>, идентифицирующее строки (обычно поле является первичным ключом,
  /// но это не является обязательным условием). Также имеется ссылочное поле (ParentColumnName), используемое для построения дерева.
  /// Поле ParentColumnName тоже должно иметь тип <typeparamref name="T"/> и обязательно поддерживать значение NULL, которое идентифицирует строки верхнего уровня.
  /// </summary>
  /// <typeparam name="T">Тип идентификатора (числовой, <see cref="String"/>, <see cref="DateTime"/>, <see cref="Guid"/>)</typeparam>
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
        throw ExceptionFactory.ArgInvalidColumnType("idColumnName", Table.Columns[idColumnName]);
    }

    #endregion

    #region Доступ к строке по идентификатору

    /// <summary>
    /// Нулевой идентификатор. Дублирует свойство <see cref="DataTableTreeModel.IsNullDefaultValue"/>
    /// </summary>
    public T DefaultId
    {
      get { return (T)IsNullDefaultValue; }
      set { IsNullDefaultValue = value; }
    }

    /// <summary>
    /// Возвращает идентификатор (значение поля <see cref="DataTableTreeModel.IdColumnName"/>), соответствующее заданному пути.
    /// Метод не проверяет весь путь, а находит строку, соответствующуую <paramref name="path"/>.LastNode.
    /// Если строка не найдена, возвращается значение по умолчанию (0 ).
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    public T TreePathToId(TreePath path)
    {
      DataRow row = TreePathToDataRow(path);
      if (row == null)
        return DefaultId;
      else
        return (T)(row[IdColumnPosition]);
    }

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// </summary>
    /// <param name="id">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    public TreePath TreePathFromId(T id)
    {
      if (id.Equals(DefaultId))
        return TreePath.Empty;
      else
        return TreePathFromKey(id);
    }

    /// <summary>
    /// Возвращает идентификатор для строки.
    /// Возвращает DefaultId, если <paramref name="row"/>==null.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <returns>Идентификатор</returns>
    public T DataRowToId(DataRow row)
    {
      if (row == null)
        return (T)DefaultId;
      else
        return (T)(row[IdColumnPosition]);
    }

    /// <summary>
    /// Возвращает строку для заданного идентификатора.
    /// Возвращает null при <paramref name="id"/>==DefaultId или если идентификатор не найден.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Строка в таблице или null</returns>
    public DataRow DataRowFromId(T id)
    {
      if (id.Equals(DefaultId))
        return null;
      else
      {
        if (UsePrimaryKey)
          return Table.Rows.Find(id);
        else
        {
          int p = InternalDataViewByIdColumn.Find(id);
          if (p >= 0)
            return InternalDataViewByIdColumn[p].Row;
          else
            return null;
        }
      }
    }

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля <see cref="DataTableTreeModel.IdColumnName"/>) дочерних узлов, для заданного родительского идентификатора нерекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку узлов данного уровня.
    /// Используется метод <see cref="DataTableTreeModel.GetChildRows(DataRow)"/>.
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
    /// Возвращает массив идентификаторов (значений поля <see cref="DataTableTreeModel.IdColumnName"/>), для заданного родительского идентификатора
    /// и всем его вложенным узлам рекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="id">Идентификатор корневого узла. Если 0, возвращаются все идентификаторы в таблице</param>
    /// <returns>Массив идентификаторов</returns>
    public T[] GetIdWithChildren(T id)
    {
      //return DataRowToIdWithChildren(DataRowFromId(Id));
      if (id.Equals(DefaultId))
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
    /// Возвращает массив идентификаторов (значений поля <see cref="DataTableTreeModel.IdColumnName"/>), соответствующее заданному пути
    /// и всем его вложенным узлам рекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// Для <paramref name="path"/>=<see cref="TreePath.Empty"/> возвращает все существующие идентификаторы в таблице.
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
    /// Возвращает массив идентификаторов (значений поля <see cref="DataTableTreeModel.IdColumnName"/>), соответствующее заданному пути
    /// и всем его вложенным узлам рекурсивно.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// Для <paramref name="row"/>=null возвращает все существующие идентификаторы в таблице.
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
        T id = (T)(row[IdColumnPosition]);
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

  /// <summary>
  /// Расширение табличной модели установкой дополнительного фильтра <see cref="DBxFilter"/>.
  /// </summary>
  /// <typeparam name="T">Тип идентификатора (числовой, <see cref="String"/>, <see cref="DateTime"/>, <see cref="Guid"/>)</typeparam>
  public class FilteredDataTableTreeModelWithIds<T> : DataTableTreeModelWithIds<T>
    where T : IEquatable<T>
  {
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
          throw ExceptionFactory.DataColumnNotFound(sourceModel.Table, sourceIntegrityFlagColumnName);
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
        throw ExceptionFactory.DataColumnNotFound(resTable, sourceModel.ParentColumnName);

      bool needsResort = false;

      while (true)
      {
        IdCollection<Int32> missingIds = new IdCollection<Int32>();
        foreach (DataRow row in resTable.Rows)
        {
          Int32 parentId = DataTools.GetInt32(row[pParentId]);
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
            throw ExceptionFactory.DataRowNotFound(sourceModel.Table, new object[] { id });
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
    private readonly DataTableTreeModel _SourceModel;

    /// <summary>
    /// Накладываемый фильтр
    /// </summary>
    public DBxFilter Filter { get { return _Filter; } }
    private readonly DBxFilter _Filter;


    private const string DefIntegrityFlagColumnName = "__Integrity";

    /// <summary>
    /// Имя логического поля, используемого для индикации узлов, которые не прошли условие фильтра,
    /// но были добавлены, чтобы обеспечить целостность дерева.
    /// Пустая строка означает, что все узлы прошли фильтр (или фильтр не задан).
    /// Это свойство не обязано совпадать с аргументом SourceIntegrityFlagColumnName, заданном в конструкторе
    /// </summary>
    public string IntegrityFlagColumnName { get { return _IntegrityFlagColumnName; } }
    private readonly string _IntegrityFlagColumnName;

    #endregion
  }
}
