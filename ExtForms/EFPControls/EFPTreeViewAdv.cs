// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Models.Tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Положение узла дерева относительно корня
  /// Содержит массив индексов. Первый элемент задает индекс узла верхнего уровня, второй - индекс дочернего узла 
  /// внутри узла верхнего уровня, и т.д.
  /// </summary>
  public struct EFPTreeNodePosition
  {
    #region Защишенный конструктор

    internal EFPTreeNodePosition(int[] indexArray)
    {
      if (indexArray != null)
      {
        if (indexArray.Length < 1)
          throw new ArgumentException("Length==0", "indexArray");
        for (int i = 0; i < indexArray.Length; i++)
        {
          if (indexArray[i] < 0)
            throw new ArgumentException("indexArray[" + i.ToString() + "]<0", "indexArray");
        }
      }

      _IndexArray = indexArray;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Позиции узлов каждого уровня в пределах родительского узла.
    /// Первый элемент массива соответствует индексу узла верхнего уровня.
    /// </summary>
    public int[] IndexArray { get { return _IndexArray; } }
    private int[] _IndexArray;

    /// <summary>
    /// Возвращает true, если структура не была не инициализирована.
    /// </summary>
    public bool IsRoot { get { return _IndexArray == null; } }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_IndexArray == null)
        return "{Root}";
      else
        return "{" + StdConvert.ToString(_IndexArray) + "}";
    }

    #endregion
  }

  /// <summary>
  /// Способ получения выделенных узлов в иерархическом просмотре
  /// </summary>
  public enum EFPGetTreeNodesMode
  {
    /// <summary>
    /// Получить только непосредственно выбранные узлы.
    /// Соответствует свойству TreeViewAdv.SelectedNodes.
    /// </summary>
    NoChildren,

    /// <summary>
    /// Для выбранных узлов получить все дочерние узлы любого уровня вложенности, независимо от того, является ли
    /// узел свернутым или развернутым.
    /// </summary>
    AllInherited,

    /// <summary>
    /// Для выбранных узлов вернуть дочерние узлы любого уровня вложенности, если выбранный узел свернут.
    /// Предполагается, что если дочерние узлы показаны на экране, то они должны быть выбраны в явном виде.
    /// Этот режим действует только для TreeViewAdv.SelectionMode=TreeViewAdvSelectionMode.Multi.
    /// Для режимов выбора узлов Single и MultiSameParent идентичен AllInherited, т.к. у пользователя нет возможности 
    /// выбирать дочерние узлы.
    /// </summary>
    CollapseInherited,
  }

  /// <summary>
  /// Провайдер для расширенного древовидного просмотра TreeViewAdv 
  /// </summary>
  public class EFPTreeViewAdv : EFPControl<TreeViewAdv>, IEFPTreeView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPTreeViewAdv(EFPBaseProvider baseProvider, TreeViewAdv control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создаент провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    public EFPTreeViewAdv(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      Init();
    }

    private void Init()
    {
      _State = EFPDataGridViewState.View;
      _ReadOnly = false;
      _CanInsert = true;
      _CanInsertCopy = false;
      _CanDelete = true;
      _CanView = true;
      _CanMultiEdit = false;

      //Control.ImageList = EFPApp.MainImages; // здесь ли ?

      //FCurrentIncSearchChars = String.Empty;
      //FCurrentIncSearchMask = null;
      _TextSearchContext = null;
      _TextSearchEnabled = true;
      if (!DesignMode)
        Control.SelectionChanged += new EventHandler(Control_SelectionChanged);
    }

    #endregion

    #region Обработчики событий управляющего элемента

    void Control_SelectionChanged(object sender, EventArgs args)
    {
      if (HasBeenCreated)
        CommandItems.PerformRefreshItems();
    }


    /// <summary>
    /// Обновляет команды локального меню
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();

      CommandItems.PerformRefreshItems();
    }

    #endregion

    #region Редактирование

    #region ReadOnly

    /// <summary>
    /// true, если поддерживается только просмотр данных, а не редактирование.
    /// Установка свойства отключает видимость всех команд редактирования. 
    /// Свойства CanInsert, CanDelete и CanInsertCopy перестают действовать
    /// Значение по умолчанию: false (редактирование разрешено)
    /// Не влияет на возможность inline-редактирования. Возможно любое сочетание
    /// свойств ReadOnly и Control.ReadOnly
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        if (value == _ReadOnly)
          return;
        _ReadOnly = value;
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;

        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство для ReadOnly.
    /// </summary>
    public DepValue<bool> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyEx.Source = value;
      }
    }
    private DepInput<bool> _ReadOnlyEx;

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<bool>(ReadOnly,ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
      }
    }

    void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region CanInsert

    /// <summary>
    /// true, если можно добаввлять строки (при DataReadOnly=false)
    /// Значение по умолчанию: true (добавление строк разрешено)
    /// </summary>
    public bool CanInsert
    {
      get { return _CanInsert; }
      set
      {
        if (value == _CanInsert)
          return;
        _CanInsert = value;
        if (_CanInsertEx != null)
          _CanInsertEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanInsert;

    /// <summary>
    /// Управляемое свойство для CanInsert
    /// </summary>
    public DepValue<bool> CanInsertEx
    {
      get
      {
        InitCanInsertEx();
        return _CanInsertEx;
      }
      set
      {
        InitCanInsertEx();
        _CanInsertEx.Source = value;
      }
    }
    private DepInput<bool> _CanInsertEx;

    private void InitCanInsertEx()
    {
      if (_CanInsertEx == null)
      {
        _CanInsertEx = new DepInput<bool>(CanInsert,CanInsertEx_ValueChanged);
        _CanInsertEx.OwnerInfo = new DepOwnerInfo(this, "CanInsertEx");
      }
    }

    void CanInsertEx_ValueChanged(object sender, EventArgs args)
    {
      CanInsert = _CanInsertEx.Value;
    }

    #endregion

    #region CanInsertCopy

    /// <summary>
    /// true, если разрешено добавлять строку по образцу существующей
    /// (при DataReadOnly=false и CanInsert=true)
    /// Значение по умолчанию: false (копирование запрещено)
    /// </summary>
    public bool CanInsertCopy
    {
      get { return _CanInsertCopy; }
      set
      {
        if (value == _CanInsertCopy)
          return;
        _CanInsertCopy = value;
        if (_CanInsertCopyEx != null)
          _CanInsertCopyEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanInsertCopy;

    /// <summary>
    /// Управляемое свойство для Checked.
    /// </summary>
    public DepValue<bool> CanInsertCopyEx
    {
      get
      {
        InitCanInsertCopyEx();
        return _CanInsertCopyEx;
      }
      set
      {
        InitCanInsertCopyEx();
        _CanInsertCopyEx.Source = value;
      }
    }
    private DepInput<bool> _CanInsertCopyEx;

    private void InitCanInsertCopyEx()
    {
      if (_CanInsertCopyEx == null)
      {
        _CanInsertCopyEx = new DepInput<bool>(CanInsertCopy,CanInsertCopyEx_ValueChanged);
        _CanInsertCopyEx.OwnerInfo = new DepOwnerInfo(this, "CanInsertCopyEx");
      }
    }

    void CanInsertCopyEx_ValueChanged(object sender, EventArgs args)
    {
      CanInsertCopy = _CanInsertCopyEx.Value;
    }

    #endregion

    #region CanDelete

    /// <summary>
    /// true, если можно удалять строки (при DataReadOnly=false)
    /// Значение по умолчанию: true (удаление разрешено)
    /// </summary>
    public bool CanDelete
    {
      get { return _CanDelete; }
      set
      {
        if (value == _CanDelete)
          return;
        _CanDelete = value;
        if (_CanDeleteEx != null)
          _CanDeleteEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanDelete;

    /// <summary>
    /// Управляемое свойство для CanDelete.
    /// </summary>
    public DepValue<bool> CanDeleteEx
    {
      get
      {
        InitCanDeleteEx();
        return _CanDeleteEx;
      }
      set
      {
        InitCanDeleteEx();
        _CanDeleteEx.Source = value;
      }
    }
    private DepInput<bool> _CanDeleteEx;

    private void InitCanDeleteEx()
    {
      if (_CanDeleteEx == null)
      {
        _CanDeleteEx = new DepInput<bool>(CanDelete,CanDeleteEx_ValueChanged);
        _CanDeleteEx.OwnerInfo = new DepOwnerInfo(this, "CanDeleteEx");
      }
    }

    void CanDeleteEx_ValueChanged(object sender, EventArgs args)
    {
      CanDelete = _CanDeleteEx.Value;
    }

    #endregion

    #region CanView

    /// <summary>
    /// true, если можно просмотреть выбранные строки в отдельном окне
    /// По умолчанию: true
    /// </summary>
    public bool CanView
    {
      get { return _CanView; }
      set
      {
        if (value == _CanView)
          return;
        _CanView = value;
        if (_CanViewEx != null)
          _CanViewEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanView;

    /// <summary>
    /// Управляемое свойство для CanView.
    /// </summary>
    public DepValue<bool> CanViewEx
    {
      get
      {
        InitCanViewEx();
        return _CanViewEx;
      }
      set
      {
        InitCanViewEx();
        _CanViewEx.Source = value;
      }
    }
    private DepInput<bool> _CanViewEx;

    private void InitCanViewEx()
    {
      if (_CanViewEx == null)
      {
        _CanViewEx = new DepInput<bool>(CanView, CanViewEx_ValueChanged);
        _CanViewEx.OwnerInfo = new DepOwnerInfo(this, "CanViewEx");
      }
    }

    void CanViewEx_ValueChanged(object sender, EventArgs args)
    {
      CanView = _CanViewEx.Value;
    }

    #endregion

    /// <summary>
    /// true, если разрешено редактирование и просмотр одновременно 
    /// нескольких выбранных строк
    /// По умолчанию - false
    /// </summary>
    public bool CanMultiEdit
    {
      get
      {
        return _CanMultiEdit;
      }
      set
      {
        _CanMultiEdit = value;
        if (value && Control.SelectionMode == TreeViewAdvSelectionMode.Single)
          Control.SelectionMode = TreeViewAdvSelectionMode.MultiSameParent;
      }
    }
    private bool _CanMultiEdit;

    /// <summary>
    /// Текущий режим, для которого вызвано событие EditData
    /// </summary>
    public EFPDataGridViewState State { get { return _State; } }
    private EFPDataGridViewState _State;

    /// <summary>
    /// Вызывается для редактирования данных, просмотра, вставки, копирования
    /// и удаления строк
    /// </summary>
    public event EventHandler EditData;

    /// <summary>
    /// Вызывает обработчик события EditData и возвращает true, если он установлен
    /// Если метод возвращает false, выполняется редактирование "по месту"
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected virtual bool OnEditData(EventArgs args)
    {
      if (EditData != null)
      {
        EditData(this, args);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Перевод просмотра в один из режимов
    /// </summary>
    public void PerformEditData(EFPDataGridViewState state)
    {
      if (_InsideEditData)
      {
        EFPApp.ShowTempMessage("Предыдущее редактирование еще не выполнено");
        return;
      }
      // Любая операция редактирования останавливает поиск по первым буквам
      //CurrentIncSearchColumn = null;

      // Пытаемся вызвать специальный обработчик в GridProducerColum

      _InsideEditData = true;
      _State = state;
      try
      {
        OnEditData(EventArgs.Empty);
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
      finally
      {
        _State = EFPDataGridViewState.View;
        _InsideEditData = false;
      }

    }
    /// <summary>
    /// Предотвращение повторного запуска редактора
    /// </summary>
    private bool _InsideEditData;

    /// <summary>
    /// Возвращает true, если управляющий элемент не содержит столбцов, которые можно было бы редактировать.
    /// Перебирает коллекцию TreeViewAdv.NodeControls в поисках InteractiveControl с EditEnabled=false.
    /// Если есть хотя бы один такой элемент, свойство возвращает false
    /// </summary>
    public bool ControlIsReadOnly
    {
      get
      {
        foreach (NodeControl c in Control.NodeControls)
        {
          InteractiveControl ic = c as InteractiveControl;
          if (ic == null)
            continue;
          if (ic.EditEnabled)
            return false;
        }
        return true;
      }
    }

    #endregion

    #region Обновление просмотра

    /// <summary>
    /// Вызывается при нажатии кнопки "Обновить", а также при смене
    /// фильтра (когда будет реализовано)
    /// </summary>
    public event EventHandler RefreshData;

    /// <summary>
    /// Вызывает событие RefreshData.
    /// Вызывайте этот метод из OnRefreshData(), если Вы полностью переопределяете его, не вызывая
    /// метод базового класса.
    /// </summary>
    protected void CallRefreshDataEventHandler(EventArgs args)
    {
      if (RefreshData != null)
        RefreshData(this, args);
    }

    /// <summary>
    /// Непереопределенный метод вызывает событие RefreshData
    /// Если метод переопределен, также должно быть переопределно свойство HasRefreshDataHandler
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnRefreshData(EventArgs args)
    {
      CallRefreshDataEventHandler(args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик события RefreshData
    /// </summary>
    public virtual bool HasRefreshDataHandler { get { return RefreshData != null; } }

    /// <summary>
    /// Принудительное обновление просмотра
    /// </summary>
    public virtual void PerformRefresh()
    {
      if (Control.IsDisposed)
        return;

      Control.BeginUpdate();
      try
      {
        // Вызов пользовательского события
        OnRefreshData(EventArgs.Empty);
      }
      finally
      {
        Control.EndUpdate();
      }

      CommandItems.PerformRefreshItems();
    }

    /// <summary>
    /// Вызов метода PerformRefresh(), который можно выполнять из обработчика события
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнеорируется</param>
    public void PerformRefresh(object sender, EventArgs args)
    {
      PerformRefresh();
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPTreeViewAdvCommandItems CommandItems { get { return (EFPTreeViewAdvCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает EFPTreeViewAdvCommandItems.
    /// </summary>
    /// <returns>Список команд</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPTreeViewAdvCommandItems(this);
    }

    /// <summary>
    /// Делает командой, выделенной по умолчанию, команду "Edit" или "View"
    /// </summary>
    protected override void OnBeforePrepareCommandItems()
    {
      base.OnBeforePrepareCommandItems();
      if ((!CommandItems.EnterAsOk) && CommandItems.DefaultCommandItem == null)
      {
        EFPCommandItem ci = CommandItems["Edit", "Edit"];
        if (ci.Visible)
          CommandItems.DefaultCommandItem = ci;
        else
        {
          ci = CommandItems["Edit", "View"];
          if (ci.Visible)
            CommandItems.DefaultCommandItem = ci;
        }
      }
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// Генератор столбцов таблицы. Если задан, то в локальном меню доступны
    /// команды настройки столбцов таблицы
    /// </summary>
    public IEFPGridProducer GridProducer 
    { 
      get { return _GridProducer; } 
      set 
      {
        if (value != null)
          value.SetReadOnly();

        _GridProducer = value; 
      } 
    }
    private IEFPGridProducer _GridProducer;

    #endregion

    #region Свойство CurrentGridConfig

    /// <summary>
    /// Выбранная настройка табличного просмотра.
    /// Если свойство GridProducer не установлено, должен быть задан обработчик CurrentGridConfigChanged,
    /// который выполнит реальную настройку просмотра
    /// </summary>
    public EFPDataGridViewConfig CurrentConfig
    {
      get { return _CurrentConfig; }
      set
      {
        if (value != null)
          value.SetReadOnly();
        _CurrentConfig = value;

        CancelEventArgs Args = new CancelEventArgs();
        Args.Cancel = false;
        OnCurrentGridConfigChanged(Args);
        if ((!Args.Cancel) && (GridProducer != null))
        {
          Control.Columns.Clear(); // 21.01.2012
          // TODO: GridProducer.InitGrid(this, CurrentConfigHasBeenSet);
          //PerformGridProducerPostInit();
        }
        _CurrentConfigHasBeenSet = true;
      }
    }
    private EFPDataGridViewConfig _CurrentConfig;

    /// <summary>
    /// Признак того, что свойство CurrentConfig уже устанавливалось
    /// </summary>
    protected bool CurrentConfigHasBeenSet { get { return _CurrentConfigHasBeenSet; } }
    private bool _CurrentConfigHasBeenSet = false;

    /// <summary>
    /// Вызывается при изменении свойства CurrentGridConfig.
    /// Если аргумент Cancel обработчиком установлен в true, то предполагается,
    /// что инициализация просмотра выполнена в обработчике. В противном случае
    /// (по умолчанию Cancel=false или при отстутствии обработчика) будет вызван
    /// метод GridProducer.InitGrid()
    /// </summary>
    public event CancelEventHandler CurrentConfigChanged;

    /// <summary>
    /// Вызывает событие CurrentConfigChanged.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCurrentGridConfigChanged(CancelEventArgs args)
    {
      if (CurrentConfigChanged != null)
        CurrentConfigChanged(this, args);
    }

    #endregion

    #region Поиск текста

    /// <summary>
    /// Контекст поиска по Ctrl-F / F3
    /// Свойство возвращает null, если TextSearchEnabled=false
    /// </summary>
    public IEFPTextSearchContext TextSearchContext
    {
      get
      {
        if (_TextSearchEnabled)
        {
          if (_TextSearchContext == null)
            _TextSearchContext = CreateTextSearchContext();
          return _TextSearchContext;
        }
        else
          return null;
      }
    }
    private IEFPTextSearchContext _TextSearchContext;

    /// <summary>
    /// Вызывается при первом обращении к свойству TextSearchContext.
    /// Непереопределенный метод создает и возвращает объект EFPTreeViewAdvSearchContext
    /// </summary>
    /// <returns></returns>
    protected virtual IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPTreeViewAdvSearchContext(this);
    }

    /// <summary>
    /// Если true (по умолчанию), то доступна команда "Найти" (Ctrl-F).
    /// Если false, то свойство TextSearchContext возвращает null и поиск недоступен.
    /// Свойство можно устанавливать только до вывода просмотра на экран
    /// </summary>
    public bool TextSearchEnabled
    {
      get { return _TextSearchEnabled; }
      set
      {
        CheckHasNotBeenCreated();
        _TextSearchEnabled = value;
      }
    }
    private bool _TextSearchEnabled;

    #endregion

    #region IEFPTreeView Members

    bool IEFPTreeView.CheckBoxes
    {
      get { return false; }
    }

    void IEFPTreeView.CheckAll(bool isChecked)
    {
      throw new NotImplementedException("The method or operation is not implemented.");
    }

    #endregion

    #region Извлечение текста

    /// <summary>
    /// Возвращает первый текстовый элемент дерева.
    /// Теоретически, свойство может возвращать null
    /// </summary>
    public BaseTextControl FirstTextControl
    {
      get
      {
        foreach (NodeControl ctrl in Control.NodeControls)
        {
          if (ctrl is BaseTextControl)
            return (BaseTextControl)ctrl;
        }
        return null;
      }
    }

    /// <summary>
    /// Возвращает массив всех текстовых элементов дерева
    /// </summary>
    public BaseTextControl[] TextControls
    {
      get
      {
        List<BaseTextControl> lst = new List<BaseTextControl>();
        foreach (NodeControl ctrl in Control.NodeControls)
        {
          if (ctrl is BaseTextControl)
            lst.Add((BaseTextControl)ctrl);
        }
        return lst.ToArray();
      }
    }

    /// <summary>
    /// Возвращает двумерный массив строк, соответствующий выбранным строкам
    /// </summary>
    /// <returns></returns>
    public string[,] GetSelectedNodesTextMatrix()
    {
      TreeNodeAdv[] Nodes = new TreeNodeAdv[Control.SelectedNodes.Count];
      Control.SelectedNodes.CopyTo(Nodes, 0);
      return GetTextMatrix2(Nodes);
    }

    private string[,] GetTextMatrix2(TreeNodeAdv[] nodes)
    {
      List<BaseTextControl> ctrls = new List<BaseTextControl>();
      foreach (NodeControl ctrl in Control.NodeControls) // исправлено 05.01.2021
      {
        if (ctrl is BaseTextControl)
          ctrls.Add((BaseTextControl)ctrl);
      }
      string[,] a = new string[nodes.Length, ctrls.Count];

      for (int i = 0; i < nodes.Length; i++)
      {
        for (int j = 0; j < ctrls.Count; j++)
          a[i, j] = ctrls[j].GetLabel(nodes[i]);
      }
      return a;
    }

    #endregion

    #region Прокрутка узлов для выделения

    /// <summary>
    /// Делает (по возможности) текущие выделенные узлы видимыми
    /// </summary>
    public void EnsureSelectionVisible()
    {
      if (Control.Selection.Count == 0)
        return;

      // 27.12.2020 лишнее if (Control.Selection.Count > 0)
      Control.EnsureVisible(Control.Selection[Control.Selection.Count - 1]);
      Control.EnsureVisible(Control.Selection[0]);
    }

    #endregion

    #region Предыдущий и следующий узлы

    /// <summary>
    /// Возвращает следующий узел относительно текущего, с учетом иерархии
    /// </summary>
    /// <param name="node">Узел, от которого находится следующий узел</param>
    /// <returns>Найденный узел или null</returns>
    public TreeNodeAdv GetNextTreeNode(TreeNodeAdv node)
    {
      if (node == null)
        return null;

      if (!node.IsLeaf)
      {
        if (node.Children.Count > 0)
          return node.Children[0];
      }

      while (node != null)
      {
        if (node.NextNode != null)
          return node.NextNode;
        node = node.Parent;
      }
      return null;
    }

    /// <summary>
    /// Возвращает предыдущий узел относительно текущего, с учетом иерархии
    /// </summary>
    /// <param name="node">Узел, от которого находится следующий узел</param>
    /// <returns>Найденный узел или null</returns>
    public TreeNodeAdv GetPreviousTreeNode(TreeNodeAdv node)
    {
      if (node == null)
        return null;

      if (node.PreviousNode != null)
        node = node.PreviousNode;
      else
      {
        node = node.Parent;
        if (node == Control.Root)
          return null;
        else
          return node;
      }

      while (true)
      {
        if (node.IsLeaf)
          return node;
        if (node.Children.Count == 0)
          return node;
        node = node.Children[node.Children.Count - 1];
      }
    }

    /// <summary>
    /// Возвращает первый дочерний узел корневого узла
    /// </summary>
    public TreeNodeAdv FirstTreeNode
    {
      get
      {
        if (Control.Root.Children.Count > 0)
          return Control.Root.Children[0];
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает последний узел дерева в иерарахии, который был бы внизу при развороте всего дерева
    /// </summary>
    public TreeNodeAdv LastTreeNode
    {
      get
      {
        if (Control.Root.Children.Count > 0)
        {
          TreeNodeAdv Node = Control.Root;
          while (true)
          {
            if (Node.IsLeaf)
              return Node;
            if (Node.Children.Count == 0)
              return Node;
            Node = Node.Children[Node.Children.Count - 1];
          }
        }
        else
          return null;
      }
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Сколько строк выбрано в дереве: одна, несколько или ни одной.
    /// Определяется из TreeViewAdv.SelectedNodes.Count
    /// </summary>
    public EFPDataGridViewSelectedRowsState SelectedRowsState
    {
      get
      {
        switch (Control.SelectedNodes.Count)
        {
          case 0: return EFPDataGridViewSelectedRowsState.NoSelection;
          case 1: return EFPDataGridViewSelectedRowsState.SingleRow;
          default: return EFPDataGridViewSelectedRowsState.MultiRows;
        }
      }
    }

    /// <summary>
    /// Возвращает true, если выбраны все узлы в дереве.
    /// Учитываются только развернутые узлы.
    /// В режиме SelectionMode=TreeViewAdvSelectionMode.Single всегда возвращается false
    /// </summary>
    public bool AreAllNodesSelected
    {
      get
      {
        if (Control.SelectionMode == TreeViewAdvSelectionMode.Single)
          return false;
        if (Control.SelectedNodes.Count == 0)
          return false;
        return DoGetAreAllNodesSelected(Control.Root);
      }
    }

    private bool DoGetAreAllNodesSelected(TreeNodeAdv node)
    {
      for (int i = 0; i < node.Children.Count; i++)
      {
        if (!node.Children[i].IsSelected)
          return false;
        if (node.Children[i].IsExpanded)
        {
          if (!DoGetAreAllNodesSelected(node.Children[i])) // рекурсивный вызов.
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Корректировка неподходящих значений EFPGetTreeNodesMode.
    /// </summary>
    /// <param name="mode">Аргумент, который нужно скорректировать</param>
    protected void CorrectGetTreeNodesMode(ref EFPGetTreeNodesMode mode)
    {
      if (mode == EFPGetTreeNodesMode.CollapseInherited)
      {
        if (Control.SelectionMode != TreeViewAdvSelectionMode.Multi)
          mode = EFPGetTreeNodesMode.AllInherited;
      }
    }

    /// <summary>
    /// Получить значения свойства TreeNodeAdv.Tag для выбранных узлов
    /// </summary>
    /// <param name="mode">Режим доступа к дочерним узлам</param>
    /// <returns>Список тегов узлов</returns>
    public object[] GetSelectedNodeTags(EFPGetTreeNodesMode mode)
    { 
      CorrectGetTreeNodesMode(ref mode);
      SingleScopeList<object> lst = new SingleScopeList<object>();
      foreach (TreeNodeAdv node in Control.SelectedNodes)
      {
        if (node.Tag == null)
          continue;
        if (lst.Contains(node.Tag))
          continue;
        lst.Add(node.Tag);
        switch (mode)
        {
          case EFPGetTreeNodesMode.NoChildren: 
            break;
          case EFPGetTreeNodesMode.AllInherited:
            AddAllInheritedTags(lst, Control.GetPath(node));
            break;
          case EFPGetTreeNodesMode.CollapseInherited:
            if (!node.IsExpanded)
              AddAllInheritedTags(lst, Control.GetPath(node));
            break;
          default:
            throw new ArgumentException("Неизвестный режим " + mode.ToString(), "mode");
        }
      }
      return lst.ToArray();
    }

    private void AddAllInheritedTags(SingleScopeList<object> lst, TreePath treePath)
    {
      if (Control.Model.IsLeaf(treePath))
        return;
      foreach (object childTag in Control.Model.GetChildren(treePath))
      {
        if (lst.Contains(childTag))
          continue;
        lst.Add(childTag);
        AddAllInheritedTags(lst, new TreePath(treePath, childTag)); // рекурсивный вызов
      }
    }

    /// <summary>
    /// Получить массив TreePath для выбранных узлов
    /// </summary>
    /// <param name="mode">Режим доступа к дочерним узлам</param>
    /// <returns>Список путей к узлам</returns>
    public TreePath[] GetSelectedNodePaths(EFPGetTreeNodesMode mode)
    {
      CorrectGetTreeNodesMode(ref mode);
      //SingleScopeList<TreePath> lst = new SingleScopeList<TreePath>();
      List<TreePath> lst = new List<TreePath>(); // Нет возможности проверять повторы путей
      foreach (TreeNodeAdv node in Control.SelectedNodes)
      {
        TreePath path = Control.GetPath(node);
        lst.Add(path);
        switch (mode)
        {
          case EFPGetTreeNodesMode.NoChildren:
            break;
          case EFPGetTreeNodesMode.AllInherited:
            AddAllInheritedPaths(lst, path);
            break;
          case EFPGetTreeNodesMode.CollapseInherited:
            if (!node.IsExpanded)
              AddAllInheritedPaths(lst, path);
            break;
          default:
            throw new ArgumentException("Неизвестный режим " + mode.ToString(), "mode");
        }
      }
      return lst.ToArray();
    }

    private void AddAllInheritedPaths(List<TreePath> lst, TreePath treePath)
    {
      if (Control.Model.IsLeaf(treePath))
        return;
      foreach (object childTag in Control.Model.GetChildren(treePath))
      {
        TreePath childPath = new TreePath(treePath, childTag);
        lst.Add(childPath);
        AddAllInheritedPaths(lst, childPath); // рекурсивный вызов
      }
    }
    #endregion

    #region Использование EFPTreeNodePosition

    /// <summary>
    /// Положение текущего узла (TreeViewAdv.SelectedNode) в виде структуры EFPTreeNodePosition.
    /// </summary>
    public EFPTreeNodePosition CurrentNodePosition
    {
      get
      {
        return GetNodePosition(Control.SelectedNode);
      }
      set
      {
        Control.SelectedNode = GetNodeFromPosition(value);
      }
    }

    /// <summary>
    /// Положения выбранных узлов (TreeViewAdv.SelectedNodes) в виде массива структур EFPTreeNodePosition.
    /// </summary>
    public EFPTreeNodePosition[] SelectedNodePositions
    {
      get
      {
        EFPTreeNodePosition[] a = new EFPTreeNodePosition[Control.SelectedNodes.Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = GetNodePosition(Control.SelectedNodes[i]);
        return a;
      }
      set
      {
        Control.BeginUpdate();
        try
        {
          Control.ClearSelection();
          if (value != null)
          {
            for (int i = 0; i < value.Length; i++)
            {
              TreeNodeAdv node = GetNodeFromPosition(value[i]);
              if (node != null)
              {
                node.IsSelected = true;
                if (Control.SelectionMode == TreeViewAdvSelectionMode.Single)
                  break;
              }
            }
          }
        }
        finally
        {
          Control.EndUpdate();
        }
      }
    }

    /// <summary>
    /// Возвращает путь к узлу как массив позиций (индексов) узлов от корня до текущего узла
    /// </summary>
    /// <param name="node">Узел, положение которого нужно найти. 
    /// Если null, то возвращается пустая структура с IsRoot=true</param>
    /// <returns>Путь к узлу дерева</returns>
    public EFPTreeNodePosition GetNodePosition(TreeNodeAdv node)
    {
      if (node == null)
        return new EFPTreeNodePosition();
      List<int> stack = new List<int>();
      while (node.Parent != null)
      {
        stack.Add(node.Parent.Children.IndexOf(node));
        node = node.Parent;
      }
      stack.Add(Control.Root.Children.IndexOf(node));
      stack.Reverse();
      return new EFPTreeNodePosition(stack.ToArray());
    }

    /// <summary>
    /// Возвращает узел, соответствующий заданному положению.
    /// Если <paramref name="value"/>.IsRoot=true, возвращается null.
    /// Если заданное положение не соответствует ни одному узлу, 
    /// </summary>
    /// <param name="value">Позиция искомого узла</param>
    /// <returns>Узел в дереве</returns>
    public TreeNodeAdv GetNodeFromPosition(EFPTreeNodePosition value)
    {
      if (value.IsRoot)
        return null;

      TreeNodeAdv node = Control.Root;
      for (int i = 0; i < value.IndexArray.Length; i++)
      {
        if (value.IndexArray[i] >= node.Children.Count)
          return null;
        node = node.Children[value.IndexArray[i]];
      }
      return node;
    }

    #endregion
  }

  /// <summary>
  /// Хранилище значений флажков для TreeViewAdv NodeCheckBox.
  /// Хранит перечислимые значения System.Windows.Forms.CheckState.
  /// Хранилище присоединяется к столбцу и делает его виртуальным
  /// </summary>
  public class TreeViewAdvCheckBoxStorage
  {
    #region Конструктор

    /// <summary>
    /// Создает пустое хранилище
    /// </summary>
    public TreeViewAdvCheckBoxStorage()
    {
      _Dict = new Dictionary<object, CheckState>();
    }

    #endregion

    #region Хранилище данных

    private Dictionary<object, CheckState> _Dict;

    /// <summary>
    /// Основное свойство.
    /// Значение флажка, связанного с заданным узлом модели.
    /// </summary>
    /// <param name="tag">Узел модели дерева</param>
    /// <returns></returns>
    public CheckState this[object tag]
    {
      get
      {
        CheckState Res;
        if (_Dict.TryGetValue(tag, out Res))
          return Res;
        else
          return CheckState.Unchecked;
      }
      set
      {
        _Dict[tag] = value;
      }
    }

    #endregion

    #region Инициализация элемента

    /// <summary>
    /// Присоединяет хранилище к столбцу древовидного просмотра
    /// </summary>
    /// <param name="control">Столбец с флажками</param>
    public void InitControl(NodeCheckBox control)
    {
      control.VirtualMode = true;
      control.ValueNeeded += new EventHandler<NodeControlValueEventArgs>(Control_ValueNeeded);
      control.ValuePushed += new EventHandler<NodeControlValueEventArgs>(Control_ValuePushed);
    }

    void Control_ValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      args.Value = this[args.Node.Tag];
    }

    void Control_ValuePushed(object sender, NodeControlValueEventArgs args)
    {
      this[args.Node.Tag] = (CheckState)(args.Value);
    }

    #endregion
  }
}
