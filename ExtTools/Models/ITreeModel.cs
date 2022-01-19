// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

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
  #region Интерфейс ITreeModel взят из Aga.Controls

  /// <summary>
  /// Интерфейс модели иерархической структуры для построения деревьев.
  /// Содержит методы для работы с путем и события, возникающие при изменении стуктуры
  /// </summary>
  public interface ITreeModel
  {
    #region Методы

    /// <summary>
    /// Возвращает перечислитель для узлов, являющихся дочерними для заданного родителького узла.
    /// Если treePath.IsEmpty=true, возвращает список узлов верхнего уровня.
    /// Перечислимыми объектами являются теги узлов (TreeNodeAdv.Tag), а не объекты TreePath.
    /// Для получения дочерних объектов TreePath используйте конструктор TreePath, принимающий родительский узел и дочерний Tag.
    /// </summary>
    /// <param name="treePath">Путь к родительскому узлу</param>
    /// <returns>Перечислитель</returns>
    IEnumerable GetChildren(TreePath treePath);

    /// <summary>
    /// Возвращает признак, что данный узел дерева является конечным, то есть не может содержать дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к проверяемому узлу</param>
    /// <returns>true, если узел является "листом"</returns>
    bool IsLeaf(TreePath treePath);

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при изменении узлов
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesChanged;

    /// <summary>
    /// Событие вызывается при добавлении узлов
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesInserted;

    /// <summary>
    /// Событие вызывается при удалении узлов
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesRemoved;

    /// <summary>
    /// Событие вызывается при "глобальном" изменении иерархии, начиная с заданного узла,
    /// задаваемого свойством TreePathEventArgs.Path. Если задан пустой путь, то дерево изменено полностью.
    /// </summary>
    event EventHandler<TreePathEventArgs> StructureChanged;

    #endregion
  }

  /// <summary>
  /// Путь в модели дерева.
  /// Содержит массив объектов, образующих иерархию.
  /// Тип объектов определяется моделью.
  /// Ссылки на саму модель ITreeModel в объект TreePath нет.
  /// Является объектом "однократной записи"
  /// </summary>
  public class TreePath
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой путь.
    /// Используйте статическое свойство Empty.
    /// </summary>
    public TreePath()
    {
      _path = new object[0];
    }

    /// <summary>
    /// Создает путь с единственным узлом
    /// </summary>
    /// <param name="node">Объект узла. Тип объекта определяется моделью</param>
    public TreePath(object node)
    {
      _path = new object[] { node };
    }

    /// <summary>
    /// Создает пусть из массива узлов.
    /// </summary>
    /// <param name="path">Объекты узлов. Тип объектов определяется моделью</param>
    public TreePath(object[] path)
    {
      _path = path;
    }

    /// <summary>
    /// Создает путь, расширяющий существующий на один уровень
    /// </summary>
    /// <param name="parent">Родительский путь</param>
    /// <param name="node">Добавляемый узел. Тип объектов определяется моделью</param>
    public TreePath(TreePath parent, object node)
    {
      _path = new object[parent.FullPath.Length + 1];
      for (int i = 0; i < _path.Length - 1; i++)
        _path[i] = parent.FullPath[i];
      _path[_path.Length - 1] = node;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает путь в виде массива объектов.
    /// Тип объектов зависит от модели.
    /// </summary>
    public object[] FullPath { get { return _path; } }
    private readonly object[] _path;

    /// <summary>
    /// Возвращает первый узел пути (объект верхнего уровня)
    /// Тип объекта зависит от модели.
    /// Возвращает null, если путь пустой.
    /// </summary>
    public object FirstNode
    {
      get
      {
        if (_path.Length > 0)
          return _path[0];
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает последний узел пути (объект нижнего уровня)
    /// Тип объекта зависит от модели.
    /// Возвращает null, если путь пустой.
    /// </summary>
    public object LastNode
    {
      get
      {
        if (_path.Length > 0)
          return _path[_path.Length - 1];
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает путь, содержащий на один уровень меньше, чем текущий.
    /// Если текущий путь пустой, то также возвращается пустой путь, 
    /// чтобы не генерировать исключение
    /// </summary>
    public TreePath Parent
    {
      // Этого свойства не было в оригинале в TreeViewAdv

      get
      {
        if (_path.Length < 2)
          return Empty;
        else
        {
          object[] a = new object[_path.Length - 1];
          Array.Copy(_path, a, a.Length);
          return new TreePath(a);
        }
      }
    }

    /// <summary>
    /// Возвращает true, если путь пустой (не содержит объектов)
    /// </summary>
    /// <returns>FullPath.Length=0</returns>
    public bool IsEmpty()
    {
      return (_path.Length == 0);
    }

    #endregion

    #region Статические свойства

    /// <summary>
    /// Пустой путь.
    /// </summary>
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly TreePath Empty = new TreePath();

    #endregion
  }

  /// <summary>
  /// Аргументы события ITreeModel.StructureChanged.
  /// Также является базовым классом для TreeModelEventArgs
  /// </summary>
  public class TreePathEventArgs : EventArgs
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект с пустым путем
    /// </summary>
    public TreePathEventArgs()
    {
      _path = new TreePath();
    }

    /// <summary>
    /// Создает объект с заданным путем
    /// </summary>
    /// <param name="path">Путь</param>
    public TreePathEventArgs(TreePath path)
    {
      if (path == null)
        throw new ArgumentNullException("path");

      _path = path;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к узлу дерева.
    /// Для события ITreeModel.StructureChanged задает узел, дочерние узлы которого полностью изменились.
    /// Если при этом задан пустой путь, предполагается полное перестроение дерева.
    /// Для событий NodesChanged, NodesInserted и NodesRemoved задает корневой узел, дочерние узлы
    /// которого поменялись. Пустой путь означает изменение узлов верхнего уровня
    /// </summary>
    public TreePath Path { get { return _path; } }
    private TreePath _path;

    /// <summary>
    /// Статический экземпляр объекта с пустым TreePath
    /// </summary>
    public static readonly new TreePathEventArgs Empty = new TreePathEventArgs();

    #endregion
  }

  /// <summary>
  /// Аргумент событий ITreeModel.NodesChanged, NodesInserted и NodesRemoved, 
  /// генерируемых моделью иерархической структуры, реализующей интерфейс ITreeModel
  /// </summary>
  public class TreeModelEventArgs : TreePathEventArgs
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый объект без индексов узлов
    /// </summary>
    /// <param name="parent">Path to a parent node</param>
    /// <param name="children">Child nodes</param>
    public TreeModelEventArgs(TreePath parent, object[] children)
      : this(parent, null, children)
    {
    }

    /// <summary>
    /// Создает новый объект с индексами узлов
    /// </summary>
    /// <param name="parent">Path to a parent node</param>
    /// <param name="indices">Indices of children in parent nodes collection</param>
    /// <param name="children">Child nodes</param>
    public TreeModelEventArgs(TreePath parent, int[] indices, object[] children)
      : base(parent)
    {
      if (children == null)
        throw new ArgumentNullException("children");

      if (indices != null && indices.Length != children.Length)
        throw new ArgumentException("indices and children arrays must have the same length");

      _Indices = indices;
      _children = children;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объекты дочерних узлов.
    /// Тип объектов зависит от модели.
    /// Дочерние узлы задаются относительно родительского узла, задаваемого свойством TreePathEventArgs.Path
    /// </summary>
    public object[] Children { get { return _children; } }
    private object[] _children;

    /// <summary>
    /// Индексы дочерних узлов в дереве.
    /// Массив соответствует свойству Children.
    /// Может быть null, если индексы узлов не определены.
    /// </summary>
    public int[] Indices { get { return _Indices; } }
    private int[] _Indices;

    #endregion
  }

  /// <summary>
  /// Абстрактный класс, реализующий интерфейс ITreeModel
  /// </summary>
  public abstract class TreeModelBase : ITreeModel
  {
    #region Методы

    /// <summary>
    /// Возвращает перечислитель для узлов, являющихся дочерними для заданного родителького узла.
    /// Если treePath.IsEmpty=true, возвращает список узлов верхнего уровня
    /// </summary>
    /// <param name="treePath">Путь к родительскому узлу</param>
    /// <returns>Перечислитель</returns>
    public abstract System.Collections.IEnumerable GetChildren(TreePath treePath);

    /// <summary>
    /// Возвращает признак, что данный узел дерева является конечным, то есть не может содержать дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к проверяемому узлу</param>
    /// <returns>true, если узел является "листом"</returns>
    public abstract bool IsLeaf(TreePath treePath);

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при изменении узлов
    /// </summary>
    public event EventHandler<TreeModelEventArgs> NodesChanged;

    /// <summary>
    /// Вызов обработчика события NodesChanged
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesChanged(TreeModelEventArgs args)
    {
      if (NodesChanged != null)
        NodesChanged(this, args);
    }

    /// <summary>
    /// Событие вызывается при "глобальном" изменении дерева, начиная с узла TreePathEventArgs.Path
    /// </summary>
    public event EventHandler<TreePathEventArgs> StructureChanged;

    /// <summary>
    /// Вызов обработчика события StructureChanged
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnStructureChanged(TreePathEventArgs args)
    {
      if (StructureChanged != null)
        StructureChanged(this, args);
    }

    /// <summary>
    /// Событие вызывается при добавлении узлов
    /// </summary>
    public event EventHandler<TreeModelEventArgs> NodesInserted;

    /// <summary>
    /// Вызов обработчика события NodesInserted
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesInserted(TreeModelEventArgs args)
    {
      if (NodesInserted != null)
        NodesInserted(this, args);
    }

    /// <summary>
    /// Событие вызывается при удалении узлов
    /// </summary>
    public event EventHandler<TreeModelEventArgs> NodesRemoved;

    /// <summary>
    /// Вызов обработчика события NodesRemoved
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesRemoved(TreeModelEventArgs args)
    {
      if (NodesRemoved != null)
        NodesRemoved(this, args);
    }

    /// <summary>
    /// Полное обновление модели
    /// </summary>
    public virtual void Refresh()
    {
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    /// <summary>
    /// Вызывает событие NodesChanged для заданного узла.
    /// Дочерние узлы не обновляются.
    /// </summary>
    /// <param name="path">Путь к узлу, который требуется обновить. Не может быть пустым</param>
    public void RefreshNode(TreePath path)
    {
      if (path == null)
        throw new ArgumentNullException("path");
      if (path.IsEmpty())
        throw new ArgumentException("Узел должен быть задан", "path");
      TreeModelEventArgs Args = new TreeModelEventArgs(path.Parent, new object[] { path.LastNode });
      OnNodesChanged(Args);
    }

    #endregion
  }

  /// <summary>
  /// Простейшая модель "дерева", содержащая узлы только одного уровня.
  /// Узлы задаются списком IList.
  /// Содержит методы для добавления, удаления и изменения узлов
  /// </summary>
  public class ListModel : TreeModelBase
  {
    #region Конструторы

    /// <summary>
    /// Создает модель, первоначально не содержащую ни одного узла
    /// </summary>
    public ListModel()
    {
      _list = new List<object>();
    }

    /// <summary>
    /// Создает модель с заданным списком узлов.
    /// Передаваемый список <paramref name="list"/> является "рабочим".
    /// Методы изменения узлов в ListModel будут вносить изменения в этот список.
    /// </summary>
    /// <param name="list">Список</param>
    public ListModel(IList list)
    {
      _list = list;
    }
    private IList _list;

    #endregion

    #region Методы ITreeModel

    /// <summary>
    /// Возвращает рабочий список в качестве перечислителя
    /// </summary>
    /// <param name="treePath">Игнорируется</param>
    /// <returns>Список</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      return _list;
    }

    /// <summary>
    /// Возвращает true, так как "дерево" не содержит узлов, кроме верхнего уровня.
    /// </summary>
    /// <param name="treePath">Игнорируется</param>
    /// <returns>true</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return true;
    }

    #endregion

    #region Методы ICollection

    /// <summary>
    /// Возвращает число узлов в списке
    /// </summary>
    public int Count
    {
      get { return _list.Count; }
    }

    /// <summary>
    /// Добавляет несколько узлов в конец списка
    /// </summary>
    /// <param name="items">Добавляемые узлы</param>
    public void AddRange(IEnumerable items)
    {
      foreach (object obj in items)
        _list.Add(obj);
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    /// <summary>
    /// Возвращает один узел в конец списка
    /// </summary>
    /// <param name="item">Добавляемый узел</param>
    public void Add(object item)
    {
      _list.Add(item);
      OnNodesInserted(new TreeModelEventArgs(TreePath.Empty, new int[] { _list.Count - 1 }, new object[] { item }));
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      _list.Clear();
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    #endregion
  }

  /// <summary>
  /// Добавление сортировки узлов дерева в пределах уровня иерархии для произвольной модели.
  /// Определяет метод GetChildren(), остальные вызовы переадресуются базовой модели.
  /// </summary>
  public class SortedTreeModel : TreeModelBase
  {
    #region Конструктор

    /// <summary>
    /// Создает модель
    /// </summary>
    /// <param name="innerModel">Базовая модель, которой переадресуются все вызовы</param>
    public SortedTreeModel(ITreeModel innerModel)
    {
      _innerModel = innerModel;
      _innerModel.NodesChanged += new EventHandler<TreeModelEventArgs>(_innerModel_NodesChanged);
      _innerModel.NodesInserted += new EventHandler<TreeModelEventArgs>(_innerModel_NodesInserted);
      _innerModel.NodesRemoved += new EventHandler<TreeModelEventArgs>(_innerModel_NodesRemoved);
      _innerModel.StructureChanged += new EventHandler<TreePathEventArgs>(_innerModel_StructureChanged);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Базовая модель, которой переадресуются все вызовы.
    /// Задается в конструкторе.
    /// </summary>
    public ITreeModel InnerModel { get { return _innerModel; } }
    private ITreeModel _innerModel;

    /// <summary>
    /// Интерфейс сортировки узлов.
    /// Пока не задан (по умолчанию), узлы не сортируются и возвращаются в том порядке,
    /// в котором определены в базовой модели.
    /// Свойство должно быть установлено, иначе использование класса SortedTreeModel не имеет смысла
    /// </summary>
    public IComparer Comparer
    {
      get { return _comparer; }
      set
      {
        _comparer = value;
        OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
      }
    }
    private IComparer _comparer;

    #endregion

    #region Методы ITreeModel

    /// <summary>
    /// Возвращает перечислимый объект для дочерних узлов.
    /// Если свойство Comparer установлено, список узлов извлекается из базовой модели и сортируются
    /// с помощью интерфейса IComparer. Отсортированный список возвращается.
    /// Если свойство Comparer не установлено, то вызов передается базовой модели для получения перечислимого объекта.
    /// </summary>
    /// <param name="treePath">Путь к узлу, для которого извлекаются дочерние узлы</param>
    /// <returns>Перечислимый объект</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      if (Comparer != null)
      {
        ArrayList list = new ArrayList();
        IEnumerable res = InnerModel.GetChildren(treePath);
        if (res != null)
        {
          foreach (object obj in res)
            list.Add(obj);
          list.Sort(Comparer);
          return list;
        }
        else
          return null;
      }
      else
        return InnerModel.GetChildren(treePath);
    }

    /// <summary>
    /// Вызов передается базовой модели.
    /// </summary>
    /// <param name="treePath">Путь к узлу</param>
    /// <returns>true, если узел является "листом" и не содержит дочерних узлов</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return InnerModel.IsLeaf(treePath);
    }

    #endregion

    #region Передача событий от базовой модели

    void _innerModel_StructureChanged(object sender, TreePathEventArgs args)
    {
      OnStructureChanged(args);
    }

    void _innerModel_NodesRemoved(object sender, TreeModelEventArgs args)
    {
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    void _innerModel_NodesInserted(object sender, TreeModelEventArgs args)
    {
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    void _innerModel_NodesChanged(object sender, TreeModelEventArgs args)
    {
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// Расширение интерфейса ITreeModel, связанного с DataTable или DataView
  /// </summary>
  public interface IDataTableTreeModel : ITreeModel
  {
    /// <summary>
    /// Объект DataTable, реализующий момент
    /// </summary>
    DataTable Table { get; }

    /// <summary>
    /// Объект DataView
    /// </summary>
    DataView DataView { get; }

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

    /// <summary>
    /// Вызывает событие NodesChanged для заданного узла.
    /// Дочерние узлы не обновляются.
    /// </summary>
    /// <param name="path">Путь к узлу, который требуется обновить. Не может быть пустым</param>
    void RefreshNode(TreePath path);
    // Может быть, этот метод должен быть в ITreeModel?
    // Или должен быть какой-нибудь интерфейс IRefreshableTreeModel?

    /// <summary>
    /// Обновление всей структуры
    /// </summary>
    void Refresh();
  }

  /// <summary>
  /// Модель данных дерева, реализующая доступ по числовым идентификатором
  /// Нулевое значение идентификатора соответствует пустому узлу
  /// </summary>
  public interface ITreeModelWithIds : ITreeModel
  {
    /// <summary>
    /// Возвращает идентификатор (значение поля IdColumnNames), соответствующее заданному пути
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    Int32 TreePathToId(TreePath path);

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// </summary>
    /// <param name="id">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    TreePath TreePathFromId(Int32 id);
  }

  /// <summary>
  /// Источник просмотра древовидной структуры из таблицы DataTable.
  /// Реализация интерфейса ITreeModelWithIds работает правильно, если тип ключевого поля -  Int32.
  /// DataTableTreeModel поддерживает числовые и строковые ключевые поля (в том числе Guid)
  /// </summary>
  public class DataTableTreeModel : TreeModelBase, IDataTableTreeModel, ITreeModelWithIds
  {
    #region Конструктор

    /// <summary>
    /// Создает модель на основе DataTable.
    /// Узлы дерева одного уровня идут в порядке следования строк в таблице.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="idColumnName">Имя ключевого столбца, например, "Id"</param>
    /// <param name="parentColumnName">Имя столбца родительского идентификатора, который образует древовидную структуру, например, "ParentId"</param>
    public DataTableTreeModel(DataTable table, string idColumnName, string parentColumnName)
      : this(table, idColumnName, parentColumnName, String.Empty)
    {
    }

    /// <summary>
    /// Создает модель на основе DataTable
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="idColumnName">Имя ключевого столбца, например, "Id"</param>
    /// <param name="parentColumnName">Имя столбца родительского идентификатора, который образует древовидную структуру, например, "ParentId"</param>
    /// <param name="sort">Ключ сортировки узлов одного уровня как выражение DataView.Sort</param>
    public DataTableTreeModel(DataTable table, string idColumnName, string parentColumnName, string sort)
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

      Type typ = _Table.Columns[idColumnName].DataType;
      if (typ == typeof(Int32) || typ == typeof(UInt32) ||
        typ == typeof(Int64) || typ == typeof(UInt64) ||
        typ == typeof(Int16) || typ == typeof(UInt16))
        FKeyType = KeyDataType.Int;
      else if (typ == typeof(String))
        FKeyType = KeyDataType.String;
      else
        throw new ArgumentException("Столбец \"" + idColumnName + "\" имеет неподходящий тип данных " + typ.ToString(), "idColumnName");

      if (_Table.Columns[parentColumnName].DataType != typ)
        throw new ArgumentException("Столбец \"" + idColumnName + "\" имеет тип данных " + typ.ToString() + ", а \"" + parentColumnName + "\" - " +
          _Table.Columns[parentColumnName].DataType.ToString(), "parentColumnName");

      if (sort == null)
        _Sort = String.Empty;
      else
        _Sort = sort;

      // 30.11.2015
      _Table.TableNewRow += new DataTableNewRowEventHandler(FTable_TableNewRow);
      _Table.RowChanged += new DataRowChangeEventHandler(FTable_RowChanged);
      // 09.12.2015 FTable.RowDeleted += new DataRowChangeEventHandler(FTable_RowDeleted);
      _Table.RowDeleting += new DataRowChangeEventHandler(FTable_RowDeleting);
      _Table.Initialized += new EventHandler(FTable_Initialized);
      _Table.TableCleared += new DataTableClearEventHandler(FTable_TableCleared);
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
    /// Тип ключевого поля IdColumnName и ParentColumnName (числовой или строковый)
    /// </summary>
    protected enum KeyDataType
    {
      /// <summary>
      /// Числовое ключевое поле
      /// </summary>
      Int,

      /// <summary>
      /// Строковое ключевое поле
      /// </summary>
      String
    }

    /// <summary>
    /// Тип ключевого поля (определяется в конструкторе)
    /// </summary>
    protected KeyDataType KeyType { get { return FKeyType; } }
    private KeyDataType FKeyType;

    /// <summary>
    /// Порядок сортировки строк (в формате аргумента sort метода DataTable.Select())
    /// </summary>
    public string Sort { get { return _Sort; } }
    private string _Sort;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает перечислитель дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к родительскому узлу</param>
    /// <returns>Перечислитель</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      if (treePath.IsEmpty())
      {
        DataRow[] Rows = _Table.Select(GetIsNullExpression(ParentColumnName), Sort);
        return Rows;
      }
      else
      {
        DataRow ParentRow = TreePathToDataRow(treePath);
        if (ParentRow == null)
          return new DataRow[0]; // 27.12.2020
        object ParentValue = ParentRow[IdColumnName];
        DataRow[] Rows = _Table.Select(GetEqExpression(ParentColumnName, ParentValue), Sort);
        return Rows;
      }
    }

    private string GetEqExpression(string columnName, object value)
    {
      if (value == null || value is DBNull)
        return GetIsNullExpression(columnName);

      switch (KeyType)
      {
        case KeyDataType.Int:
          return columnName + "=" + value.ToString();
        case KeyDataType.String:
          return columnName + "=\'" + value.ToString() + "\'";
        default:
          throw new BugException("Неизвестное значение KeyType=" + KeyType.ToString());
      }
    }

    private string GetIsNullExpression(string columnName)
    {
      switch (KeyType)
      {
        case KeyDataType.Int:
          return "ISNULL(" + columnName + ",0)=0";
        case KeyDataType.String:
          return "ISNULL(" + columnName + ",\"\")=\"\"";
        default:
          throw new BugException("Неизвестное значение KeyType=" + KeyType.ToString());
      }
    }

    private DataRow GetDataRowWithCheck(object tag)
    {
      DataRow Row = tag as DataRow;
      if (Row == null)
      {
        if (tag == null)
          throw new ArgumentNullException("tag");
        else
          throw new ArgumentException("Аргумент tag не является DataRow", "tag");
      }
      if (Row.Table != _Table)
        throw new ArgumentException("Строка относится к другой таблице", "tag");
      return Row;
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
      DataRow Row = TreePathToDataRow(treePath);
      if (Row == null)
        return true; // 27.12.2020
      object Value = Row[IdColumnName];
      DataRow[] Rows = _Table.Select(GetEqExpression(ParentColumnName, Value));
      return Rows.Length == 0;
    }

    #endregion

    #region Обработка событий DataTable

    void FTable_TableCleared(object sender, DataTableClearEventArgs args)
    {
      base.OnStructureChanged(TreePathEventArgs.Empty);
    }

    void FTable_Initialized(object Sender, EventArgs Args)
    {
      base.OnStructureChanged(TreePathEventArgs.Empty);
    }

    void FTable_RowDeleting(object sender, DataRowChangeEventArgs args)
    {
      int ParentId = DataTools.GetInt(args.Row, ParentColumnName);
      TreeModelEventArgs Args2 = new TreeModelEventArgs(TreePathFromId(ParentId), new object[] { args.Row });
      base.OnNodesRemoved(Args2);
    }

    void FTable_RowChanged(object sender, DataRowChangeEventArgs args)
    {
      int ParentId = DataTools.GetInt(args.Row[ParentColumnName]);
      switch (args.Action)
      {
        case DataRowAction.Change:
          ParentId = DataTools.GetInt(args.Row[ParentColumnName]);
          TreeModelEventArgs Args2 = new TreeModelEventArgs(TreePathFromId(ParentId), new object[] { args.Row });
          base.OnNodesChanged(Args2);
          break;
        case DataRowAction.Add:
          ParentId = DataTools.GetInt(args.Row[ParentColumnName]);
          int[] indices = new int[1] { 0 }; // !!!!
          TreeModelEventArgs Args3 = new TreeModelEventArgs(TreePathFromId(ParentId), indices, new object[] { args.Row });
          base.OnNodesInserted(Args3);
          break;
      }
    }

    void FTable_TableNewRow(object sender, DataTableNewRowEventArgs args)
    {
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
      DataRow Row = path.LastNode as DataRow;
      if (Row == null)
      {
        throw new InvalidCastException("Аргумент treePath.LastNode не является DataRow");
      }
      if (Row.Table != _Table)
        throw new ArgumentException("Строка относится к другой таблице");
      return Row;
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

      object ParentId = row[ParentColumnName];
      if (ParentId is DBNull)
        // строка верхнего уровня
        return new TreePath(new object[] { row });
      ArrayList lst = new ArrayList();
      lst.Insert(0, row);

      if (UsePrimaryKey)
      {
        while (!(ParentId is DBNull))
        {
          row = _Table.Rows.Find(ParentId);
          if (row == null)
            throw new InvalidOperationException("В таблице " + _Table.TableName + " не найдена строка с идентификатором " + DataTools.GetString(ParentId));
          ParentId = row[ParentColumnName];

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
          while (!(ParentId is DBNull))
          {
            int p = dv.Find(ParentId);
            if (p < 0)
              throw new InvalidOperationException("В таблице " + _Table.TableName + " не найдена строка с идентификатором " + DataTools.GetString(ParentId));
            row = dv[p].Row;
            ParentId = row[ParentColumnName];

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
        DataRow[] Rows = _Table.Select(GetIsNullExpression(ParentColumnName), Sort);
        return Rows;
      }
      else
      {
        object ParentValue = parentRow[IdColumnName];
        DataRow[] Rows = _Table.Select(GetEqExpression(ParentColumnName, ParentValue), Sort);
        return Rows;
      }
    }


    #endregion

    #region Доступ к строке по идентификатору

    /// <summary>
    /// Возвращает идентификатор (значение поля IdColumnName), соответствующее заданному пути
    /// Этот метод можно применять только для числовых идентификаторов
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    public Int32 TreePathToId(TreePath path)
    {
      DataRow Row = TreePathToDataRow(path);
      if (Row == null)
        return 0;
      else
        return DataTools.GetInt(Row, IdColumnName);
    }

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// Этот метод можно применять только для числовых идентификаторов
    /// </summary>
    /// <param name="id">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    public TreePath TreePathFromId(Int32 id)
    {
      if (id == 0)
        return TreePath.Empty;

      DataRow Row;
      if (UsePrimaryKey)
        Row = _Table.Rows.Find(id);
      else
      {
        using (DataView dv = new DataView(_Table))
        {
          dv.Sort = IdColumnName;
          int p = dv.Find(id);
          if (p >= 0)
            Row = dv[p].Row;
          else
            Row = null;
        }
      }

      return TreePathFromDataRow(Row);
    }

    /// <summary>
    /// Возвращает идентификатор для строки.
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Возвращает 0, если <paramref name="row"/>==null.
    /// </summary>
    /// <param name="row">Строка</param>
    /// <returns>Идентификатор</returns>
    public Int32 DataRowToId(DataRow row)
    {
      if (row == null)
        return 0;
      else
        return DataTools.GetInt(row, IdColumnName);
    }

    /// <summary>
    /// Возвращает строку для заданного идентификатора.
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Возвращает null при <paramref name="id"/>==0 или идентификатор не найден.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Строка в таблице или null</returns>
    public DataRow DataRowFromId(Int32 id)
    {
      if (id == 0)
        return null;
      else
      {
        DataRow Row;
        if (UsePrimaryKey)
          Row = _Table.Rows.Find(id);
        else
        {
          using (DataView dv = new DataView(_Table))
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
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Порядок идентификаторов в массиве соответствует порядку узлов данного уровня.
    /// Используется метод GetChildRows().
    /// </summary>
    /// <param name="id">Идентификатор родительского узла.
    /// Если 0, то будут возвращены идентификаторы строк верхнего уровня</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] GetChildIds(Int32 id)
    {
      DataRow[] Rows = GetChildRows(DataRowFromId(id));
      Int32[] Ids = new Int32[Rows.Length];
      for (int i = 0; i < Rows.Length; i++)
        Ids[i] = DataRowToId(Rows[i]);
      return Ids;
    }

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), для заданного родительского идентификатора
    /// и всем его вложенным узлам рекурсивно.
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="id">Идентификатор корневого узла. Если 0, возвращаются все идентификаторы в таблице</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] GetIdWithChildren(Int32 id)
    {
      //return DataRowToIdWithChildren(DataRowFromId(Id));
      if (id == 0)
        return DataRowToIdWithChildren(null);
      else
      {
        DataRow Row = DataRowFromId(id);
        if (Row == null)
          return new Int32[1] { id }; // 10.06.2019
        else
          return DataRowToIdWithChildren(Row);
      }
    }


    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), соответствующее заданному пути
    /// и всем его вложенным узлам рекурсивно.
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="path">Путь к узлу дерева. Если путь пустой, возвращаются все идентификаторы в таблице</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] TreePathToIdWithChildren(TreePath path)
    {
      SingleScopeList<Int32> Ids = new SingleScopeList<Int32>();
      DataRow Row = TreePathToDataRow(path);
      DoAddIdWithChildren(Ids, Row);
      return Ids.ToArray();
    }

    /// <summary>
    /// Возвращает массив идентификаторов (значений поля IdColumnName), соответствующее заданному пути
    /// и всем его вложенным узлам рекурсивно.
    /// Этот метод можно применять только для числовых идентификаторов.
    /// Порядок идентификаторов в массиве соответствует порядку обхода узлов в дереве.
    /// </summary>
    /// <param name="row">Строка в таблице. Если задана пустая строка, возвращаются все идентификаторы</param>
    /// <returns>Массив идентификаторов</returns>
    public Int32[] DataRowToIdWithChildren(DataRow row)
    {
      SingleScopeList<Int32> Ids = new SingleScopeList<Int32>();
      DoAddIdWithChildren(Ids, row);
      return Ids.ToArray();
    }

    private void DoAddIdWithChildren(SingleScopeList<Int32> ids, DataRow row)
    {
      if (row != null)
      {
        Int32 Id = DataTools.GetInt(row, IdColumnName);
        if (ids.Contains(Id))
          return; // Ошибка - дерево зациклено
        ids.Add(Id);
      }

      DataRow[] ChildRows = GetChildRows(row);
      for (int i = 0; i < ChildRows.Length; i++)
        DoAddIdWithChildren(ids, ChildRows[i]);
    }

    #endregion

    #region Доступ к строке по ключевому полю

    /// <summary>
    /// Возвращает идентификатор (значение поля IdColumnNames), соответствующее заданному пути.
    /// Возвращает DBNuill, если строка в таблице не найдена
    /// Этот метод можно применять для любых идентификаторов
    /// </summary>
    /// <param name="path">Путь к узлу дерева</param>
    /// <returns>Идентификатор в строке таблицы данных</returns>
    public object TreePathToKey(TreePath path)
    {
      DataRow Row = TreePathToDataRow(path);
      if (Row == null)
        return DBNull.Value;
      else
        return Row[IdColumnName];
    }

    /// <summary>
    /// Возвращает путь в дереве, соответствующий заданному идентификатору
    /// Этот метод можно применять для любых идентификаторов
    /// </summary>
    /// <param name="key">Идентификатор строки</param>
    /// <returns>Путь в дереве</returns>
    public TreePath TreePathFromKey(object key)
    {
      if (key == null || key is DBNull)
        return TreePath.Empty;

      DataRow Row;
      if (UsePrimaryKey)
        Row = _Table.Rows.Find(key);
      else
      {
        using (DataView dv = new DataView(_Table))
        {
          dv.Sort = IdColumnName;
          int p = dv.Find(key);
          if (p >= 0)
            Row = dv[p].Row;
          else
            Row = null;
        }
      }

      return TreePathFromDataRow(Row);
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
      object ParentValue = parentRow[IdColumnName];
      DataRow[] Rows = _Table.Select(GetEqExpression(ParentColumnName, ParentValue));
      for (int i = 0; i < Rows.Length; i++)
      {
        lst.Add(Rows[i]);
        DoAddChildRows(lst, Rows[i]);
      }
    }

    #endregion
  }
}
