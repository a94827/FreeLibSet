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
using FreeLibSet.UICore;
using FreeLibSet.Logging;

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
          throw ExceptionFactory.ArgIsEmpty("indexArray");
        for (int i = 0; i < indexArray.Length; i++)
        {
          if (indexArray[i] < 0)
            throw ExceptionFactory.ArgInvalidEnumerableItem("indexArray", indexArray, indexArray[i]);
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
    private readonly int[] _IndexArray;

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
    /// Соответствует свойству <see cref="TreeViewAdv.SelectedNodes"/>.
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
    /// Этот режим действует только для <see cref="TreeViewAdv.SelectionMode"/>=<see cref="TreeViewAdvSelectionMode.Multi"/>.
    /// Для режимов выбора узлов <see cref="TreeViewAdvSelectionMode.Single"/> и <see cref="TreeViewAdvSelectionMode.MultiSameParent"/> идентичен <see cref="AllInherited"/>, т.к. у пользователя нет возможности 
    /// выбирать дочерние узлы.
    /// </summary>
    CollapseInherited,
  }

  /// <summary>
  /// Провайдер для расширенного древовидного просмотра <see cref="TreeViewAdv"/> 
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
      _State = UIDataState.View;
      _ReadOnly = false;
      _CanEdit = true;
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

      Control.PreviewKeyDown += new PreviewKeyDownEventHandler(Control_PreviewKeyDown);
    }

    #endregion

    #region Обработчики событий управляющего элемента


    /// <summary>
    /// Если для управляющего элемента не было установлено свойство <see cref="TreeViewAdv.UseColumns"/> и
    /// не было добавлено ни одного <see cref="NodeControl"/>, то для дерева добавляется значок свернутого/развернутого элемента и
    /// текстовый узел, который выводит текстовое представление текущего узла.
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      // 15.08.2023 - всегда добавляем // if ((!Control.UseColumns))
      InitDefaultNodeControls(); // 08.02.2023
    }

    /// <summary>
    /// При отключении элемента отменяет регистрацию событий
    /// </summary>
    protected override void OnDetached()
    {
      base.OnDetached();
    }

    private void InitDefaultNodeControls()
    {
      int posNodeStateIcon = -1;
      int posNodeText = -1;
      for (int i = 0; i < Control.NodeControls.Count; i++)
      {
        NodeControl nc = Control.NodeControls[i];
        if ((nc is NodeStateIcon) && posNodeStateIcon < 0)
          posNodeStateIcon = i;
        if ((nc is BaseTextControl) && posNodeText < 0)
          posNodeText = i;
      }

      if (Control.UseColumns && Control.Columns.Count == 0)
      {
        TreeColumn column = new TreeColumn();
        Control.Columns.Add(column);
      }

      if (posNodeText < 0)
      {
        NodeTextBox tb = new NodeTextBox();
        tb.VirtualMode = true;
        tb.ValueNeeded += new EventHandler<NodeControlValueEventArgs>(DefaultTextBox_ValueNeeded);
        tb.EditEnabled = false;
        if (Control.UseColumns)
          tb.ParentColumn = Control.Columns[0];
        Control.NodeControls.Add(tb);
        posNodeText = Control.NodeControls.Count - 1;
      }

      if (posNodeStateIcon < 0)
      {
        NodeStateIcon ni = new NodeStateIcon();
        if (Control.UseColumns)
          //ni.ParentColumn = Control.NodeControls[posNodeText].ParentColumn;
          ni.ParentColumn = Control.Columns[0];
        Control.NodeControls.Insert(posNodeText, ni); // 15.08.2023
      }
    }

    private void DefaultTextBox_ValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      if (args.Node.Tag == null)
        args.Value = "null";
      else
        args.Value = args.Node.Tag.ToString();
    }

    void Control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs args)
    {
      // 06.10.2025
      // Требуется обработка клавиши Enter и всех ее комбинаций
      // EFPControlBase не добавляет IsInputKey для одиночной <Enter>.
      if (args.KeyCode == Keys.Enter)
      {
        args.IsInputKey = true;
      }
    }



    #endregion

    #region Редактирование

    #region ReadOnly

    /// <summary>
    /// true, если поддерживается только просмотр данных, а не редактирование.
    /// Установка свойства отключает видимость всех команд редактирования. 
    /// Свойства <see cref="CanInsert"/>, <see cref="CanDelete"/> и <see cref="CanInsertCopy"/> перестают действовать.
    /// Значение по умолчанию: false (редактирование разрешено).
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
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство для <see cref="ReadOnly"/>.
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
        _ReadOnlyEx = new DepInput<bool>(ReadOnly, ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
      }
    }

    void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region CanEdit

    /// <summary>
    /// true, если можно редактировать строки (при <see cref="ReadOnly"/>=false).
    /// Значение по умолчанию: true (редактирование разрешено).
    /// Возможность группового редактирования зависит от <see cref="CanMultiEdit"/>.
    /// </summary>
    [DefaultValue(true)]
    public bool CanEdit
    {
      get { return _CanEdit; }
      set
      {
        if (value == _CanEdit)
          return;
        _CanEdit = value;
        if (_CanEditEx != null)
          _CanEditEx.Value = value;
      }
    }
    private bool _CanEdit;

    /// <summary>
    /// Управляемое свойство для <see cref="CanEdit"/>
    /// </summary>
    public DepValue<bool> CanEditEx
    {
      get
      {
        InitCanEditEx();
        return _CanEditEx;
      }
      set
      {
        InitCanEditEx();
        _CanEditEx.Source = value;
      }
    }
    private DepInput<bool> _CanEditEx;

    private void InitCanEditEx()
    {
      if (_CanEditEx == null)
      {
        _CanEditEx = new DepInput<bool>(CanInsert, CanEditEx_ValueChanged);
        _CanEditEx.OwnerInfo = new DepOwnerInfo(this, "CanEditEx");
      }
    }

    void CanEditEx_ValueChanged(object Sender, EventArgs Args)
    {
      CanEdit = _CanEditEx.Value;
    }

    #endregion

    #region CanInsert

    /// <summary>
    /// true, если можно добавлять строки (при <see cref="ReadOnly"/>=false).
    /// Значение по умолчанию: true (добавление строк разрешено).
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
      }
    }
    private bool _CanInsert;

    /// <summary>
    /// Управляемое свойство для <see cref="CanInsert"/>
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
        _CanInsertEx = new DepInput<bool>(CanInsert, CanInsertEx_ValueChanged);
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
    /// (при <see cref="ReadOnly"/>=false и <see cref="CanInsert"/>=true).
    /// Значение по умолчанию: false (копирование запрещено).
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
      }
    }
    private bool _CanInsertCopy;

    /// <summary>
    /// Управляемое свойство для <see cref="CanInsertCopy"/>.
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
        _CanInsertCopyEx = new DepInput<bool>(CanInsertCopy, CanInsertCopyEx_ValueChanged);
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
    /// true, если можно удалять строки (при <see cref="ReadOnly"/>=false).
    /// Значение по умолчанию: true (удаление разрешено).
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
      }
    }
    private bool _CanDelete;

    /// <summary>
    /// Управляемое свойство для <see cref="CanDelete"/>.
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
        _CanDeleteEx = new DepInput<bool>(CanDelete, CanDeleteEx_ValueChanged);
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
    /// true, если можно просмотреть выбранные строки в отдельном окне.
    /// По умолчанию: true.
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
      }
    }
    private bool _CanView;

    /// <summary>
    /// Управляемое свойство для <see cref="CanView"/>.
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
    /// нескольких выбранных строк.
    /// По умолчанию - false.
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
    /// Текущий режим, для которого вызвано событие <see cref="EditData"/>.
    /// </summary>
    public UIDataState State { get { return _State; } }
    private UIDataState _State;

    /// <summary>
    /// Вызывается для редактирования данных, просмотра, вставки, копирования
    /// и удаления строк
    /// </summary>
    public event EventHandler EditData;

    /// <summary>
    /// Вызывает обработчик события <see cref="EditData"/> и возвращает true, если он установлен.
    /// </summary>
    /// <param name="args">Фиктивный аргумент</param>
    /// <returns>Признак наличия обработчика <see cref="EditData"/></returns>
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
    /// Перевод просмотра в один из режимов <see cref="State"/> и вызов <see cref="OnEditData(EventArgs)"/>.
    /// </summary>
    public void PerformEditData(UIDataState state)
    {
      if (_InsideEditData)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_EditNotFinished);
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
      }
      finally
      {
        _State = UIDataState.View;
        _InsideEditData = false;
      }

    }
    /// <summary>
    /// Предотвращение повторного запуска редактора
    /// </summary>
    private bool _InsideEditData;

    /// <summary>
    /// Возвращает true, если управляющий элемент не содержит столбцов, которые можно было бы редактировать.
    /// Перебирает коллекцию <see cref="TreeViewAdv.NodeControls"/> в поисках <see cref="InteractiveControl"/> с <see cref="InteractiveControl.EditEnabled"/>=false.
    /// Если есть хотя бы один такой элемент, свойство возвращает false.
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
    /// Вызывайте этот метод из <see cref="OnRefreshData(EventArgs)"/>, если Вы полностью переопределяете его, не вызывая
    /// метод базового класса.
    /// </summary>
    protected void CallRefreshDataEventHandler(EventArgs args)
    {
      if (RefreshData != null)
        RefreshData(this, args);
    }

    /// <summary>
    /// Непереопределенный метод вызывает событие <see cref="RefreshData"/> с помощью метода <see cref="CallRefreshDataEventHandler(EventArgs)"/>.
    /// Если метод переопределен, также должно быть переопределено свойство <see cref="HasRefreshDataHandler"/>.
    /// </summary>
    /// <param name="args">Фиктивные аргументы события</param>
    protected virtual void OnRefreshData(EventArgs args)
    {
      CallRefreshDataEventHandler(args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик события RefreshData
    /// </summary>
    public virtual bool HasRefreshDataHandler { get { return RefreshData != null; } }

    /// <summary>
    /// Событие вызывается при выполнении метода <see cref="PerformRefresh()"/> после отработки основного события.
    /// В отличие от события <see cref="RefreshData"/>, наличие обработчика этого события не влияет на <see cref="HasRefreshDataHandler"/> и на наличие команды "Обновить".
    /// </summary>
    public event EventHandler AfterRefreshData;

    /// <summary>
    /// Вызывает обработчик события AfterRefreshData
    /// </summary>
    /// <param name="args">Фиктивные аргументы события</param>
    protected virtual void OnAfterRefreshData(EventArgs args)
    {
      if (AfterRefreshData != null)
        AfterRefreshData(this, args);
    }

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

      OnAfterRefreshData(EventArgs.Empty);
    }

    /// <summary>
    /// Вызов метода <see cref="PerformRefresh()"/>, который можно выполнять из обработчика события
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
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
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPTreeViewAdvCommandItems(this);
    }

    ///// <summary>
    ///// </summary>
    //protected override void OnBeforePrepareCommandItems()
    //{
    //  base.OnBeforePrepareCommandItems();
    //}

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

    #region CheckBoxes

    /// <summary>
    /// Включение или выключение флажков для отметки узлов.
    /// Дублирует свойство <see cref="CheckBoxControl"/>.
    /// При установке в true создает новый элемент <see cref="NodeCheckBox"/> для отображения флажков и добавляет его в коллекцию NodeControls.
    /// При установке в false удаляет элемент <see cref="CheckBoxControl"/> из списка.
    /// </summary>
    public bool CheckBoxes
    {
      get { return CheckBoxControl != null; }
      set
      {
        if (value == (CheckBoxControl != null))
          return;
        if (value)
        {
          NodeCheckBox cb = new NodeCheckBox();
          cb.EditEnabled = true;
          Control.NodeControls.Insert(0, cb);
          CheckBoxControl = cb;
          if (Control.UseColumns && Control.Columns.Count > 0)
            cb.ParentColumn = Control.Columns[0];
        }
        else
        {
          Control.NodeControls.Remove(_CheckBoxControl);
          _CheckBoxControl = null;
        }
      }
    }

    /// <summary>
    /// Узел, который используется для установки отметок выбора узлов.
    /// </summary>
    public NodeCheckBox CheckBoxControl
    {
      get { return _CheckBoxControl; }
      set
      {
        if (object.ReferenceEquals(value, _CheckBoxControl))
          return;

        if (_CheckBoxStorage != null && _CheckBoxControl != null)
          _CheckBoxStorage.Detach(_CheckBoxControl);

        if (_CheckBoxControl != null)
          _CheckBoxControl.CheckStateChanged -= CheckBoxControl_CheckStateChanged;

        _CheckBoxControl = value;

        if (_CheckBoxControl != null)
          _CheckBoxControl.CheckStateChanged += CheckBoxControl_CheckStateChanged;

        if (_CheckBoxStorage != null && _CheckBoxControl != null)
          _CheckBoxStorage.Attach(_CheckBoxControl);
      }
    }
    private NodeCheckBox _CheckBoxControl;

    /// <summary>
    /// Установка или снятие отметок со всех узлов.
    /// Должно быть установлено свойство <see cref="CheckBoxes"/>=true.
    /// </summary>
    /// <param name="isChecked"></param>
    public void CheckAll(bool isChecked)
    {
      if (_CheckBoxControl == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "CheckBoxControl");

      foreach (TreeNodeAdv node in Control.AllNodes)
      {
        _CheckBoxControl.SetChecked(node, isChecked);
      }
      Control.Invalidate();
    }

    /// <summary>
    /// Хранилище для значений флажков выбора узлов.
    /// Это свойство должно устанавливаться до показа элемента на экране.
    /// Чтобы включить флажки, установите также свойство <see cref="CheckBoxes"/>.
    /// </summary>
    public EFPTreeViewAdvCheckBoxStorage CheckBoxStorage
    {
      get { return _CheckBoxStorage; }
      set
      {
        if (ProviderState == EFPControlProviderState.Attached)
          throw ExceptionFactory.ObjectProperty(this, "ProviderState", ProviderState, null);

        if (Object.ReferenceEquals(_CheckBoxStorage, value))
          return;

        if (_CheckBoxStorage != null && _CheckBoxControl != null)
          _CheckBoxStorage.Detach(_CheckBoxControl);

        _CheckBoxStorage = value;

        if (_CheckBoxStorage != null && _CheckBoxControl != null)
          _CheckBoxStorage.Attach(_CheckBoxControl);
      }
    }
    private EFPTreeViewAdvCheckBoxStorage _CheckBoxStorage;


    /// <summary>
    /// Режим взаимосвязанной установки флажков, когда свойство CheckBoxes=true.
    /// По умолчанию - None - флажки не влияют друг на друга
    /// </summary>
    public EFPTreeViewAutoCheckMode AutoCheckMode
    {
      get { return _AutoCheckMode; }
      set
      {
        CheckHasNotBeenCreated();
        switch (value)
        {
          case EFPTreeViewAutoCheckMode.None:
          case EFPTreeViewAutoCheckMode.ParentsAndChildren:
            break;
          default:
            throw new ArgumentException();
        }

        _AutoCheckMode = value;
      }
    }
    private EFPTreeViewAutoCheckMode _AutoCheckMode;


    private bool _InsideAfterCheck = false;
    private bool _CheckBoxControl_CheckStateChanged_ExceptionShown = false;

    private void CheckBoxControl_CheckStateChanged(object sender, TreePathEventArgs args)
    {
      if (_AutoCheckMode == EFPTreeViewAutoCheckMode.ParentsAndChildren)
      {
        if (_InsideAfterCheck)
          return;
        _InsideAfterCheck = true;
        try
        {
          try
          {
            TreeNodeAdv node = Control.FindNode(args.Path, true);
            if (node == null)
              throw new BugException();
            DoAfterCheck(node, true);
          }
          catch (Exception e)
          {
            if (!_CheckBoxControl_CheckStateChanged_ExceptionShown)
            {
              _CheckBoxControl_CheckStateChanged_ExceptionShown = true;
              LogoutTools.LogoutException(e);
            }
            EFPApp.ShowTempMessage(e.Message);
          }
        }
        finally
        {
          _InsideAfterCheck = false;
        }
      }
    }

    private void DoAfterCheck(TreeNodeAdv node, bool isRequrse)
    {
      switch (AutoCheckMode)
      {
        case EFPTreeViewAutoCheckMode.ParentsAndChildren:
          if (CheckBoxControl.GetChecked(node))
          {
            for (int i = 0; i < node.Nodes.Count; i++)
              DoSetCheckedChildren(node.Nodes[i], true, true);
            if (node.Parent != null)
            {
              if (AreAllChildNodesChecked(node.Parent))
                DoSetCheckedChildren(node.Parent, true, true);
            }
          }
          else
          {
            if (isRequrse)
            {
              for (int i = 0; i < node.Nodes.Count; i++)
                DoSetCheckedChildren(node.Nodes[i], false, true);
            }
            if (node.Parent != null)
              DoSetCheckedChildren(node.Parent, false, false);
          }
          break;
        case EFPTreeViewAutoCheckMode.None:
          break;
        default:
          throw new BugException("AutoCheckMode=" + AutoCheckMode.ToString());
      }
    }

    private void DoSetCheckedChildren(TreeNodeAdv node, bool isChecked, bool isRequrse)
    {
      if (node.Parent == null) // скрытый корневой узел
        return; // 28.02.2023
      if (CheckBoxControl.GetChecked(node) == isChecked)
        return;
      CheckBoxControl.SetChecked(node, isChecked);
      DoAfterCheck(node, isRequrse);
    }

    /// <summary>
    /// Возвращает true, если у всех дочерних узлов данного узла установлены отметки.
    /// Наличие отметки у самого узла <paramref name="node"/> не проверяется.
    /// Метод НЕ является рекусивным.
    /// </summary>
    /// <param name="node">Родительский узел для проверяемых узлов</param>
    /// <returns>truem </returns>
    public bool AreAllChildNodesChecked(TreeNodeAdv node)
    {
      if (CheckBoxControl == null)
        return false;
      for (int i = 0; i < node.Nodes.Count; i++)
      {
        if (!CheckBoxControl.GetChecked(node.Nodes[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Перебирает все узлы дерева. Если узел отмечен, то проверяется, что все его родительские узлы развернуты.
    /// Сам отмеченный узел не разворачивается.
    /// Свойство <see cref="CheckBoxes"/> должно быть установлено.
    /// Модель дерева должна быть присоединена.
    /// </summary>
    public void ExpandToCheckedNodes()
    {
      if (CheckBoxControl == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "CheckBoxControl");

      foreach (TreeNodeAdv node in Control.AllNodes)
      {
        if (CheckBoxControl.GetChecked(node))
        {
          TreeNodeAdv node2 = node.Parent;
          while (node2 != null)
          {
            node2.IsExpanded = true;
            node2 = node2.Parent;
          }
        }
      }
    }

    /// <summary>
    /// Выбирает первый отмеченный узел.
    /// Свойство <see cref="CheckBoxes"/> должно быть установлено.
    /// Модель дерева должна быть присоединена.
    /// </summary>
    public void SelectFirstCheckedNode()
    {
      if (CheckBoxControl == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "CheckBoxControl");

      foreach (TreeNodeAdv node in Control.AllNodes)
      {
        if (CheckBoxControl.GetChecked(node))
        {
          Control.EnsureVisible(node);
          node.IsSelected = true;
          return;
        }
      }
    }

    #endregion

    #region Получение NodeControl

    /// <summary>
    /// Возвращает первый <see cref="NodeControl"/> заданного типа или производного класса.
    /// Null, если нет ни одного подходящего столбца
    /// </summary>
    /// <typeparam name="T">Тип элемента. Например, <see cref="BaseTextControl"/> для текстового столбца</typeparam>
    public T GetFirstNodeControl<T>()
      where T : NodeControl
    {
      foreach (NodeControl ctrl in Control.NodeControls)
      {
        if (ctrl is T)
          return (T)ctrl;
      }
      return null;
    }

    /// <summary>
    /// Возвращает массив всех элементов <see cref="NodeControl"/> заданного типа или производного класса.
    /// </summary>
    /// <typeparam name="T">Тип элемента. Например, <see cref="BaseTextControl"/> для получения всех текстовых столбцов</typeparam>
    public T[] GetNodeControls<T>()
      where T : NodeControl
    {
      List<T> lst = null;
      foreach (NodeControl ctrl in Control.NodeControls)
      {
        if (lst == null)
          lst = new List<T>();
        if (ctrl is T)
          lst.Add((T)ctrl);
      }
      if (lst == null)
        return new T[0];
      else
        return lst.ToArray();
    }

    /// <summary>
    /// Возвращает массив всех <see cref="NodeControl"/>, относящихся к данному столбцу
    /// </summary>
    /// <param name="column">Столбец, к которому относятся элементы</param>
    /// <returns></returns>
    /// <typeparam name="T">Тип элемента. Например, <see cref="BaseTextControl"/> для получения всех текстовых столбцов. Если равен <see cref="NodeControl"/>, возвращаются все элементы</typeparam>
    public T[] GetNodeControls<T>(TreeColumn column)
      where T : NodeControl
    {
      List<T> lst = null;
      foreach (NodeControl ctrl in Control.NodeControls)
      {
        if (ctrl.ParentColumn != column)
          continue;
        if (lst == null)
          lst = new List<T>();
        if (ctrl is T)
          lst.Add((T)ctrl);
      }
      if (lst == null)
        return new T[0];
      else
        return lst.ToArray();
    }

    // Бесполезное свойство, так как оно устанавливается обычно только при показе формы
    ///// <summary>
    ///// Возвращает объект для управления значками дерева.
    ///// Если в дереве значки не отображаются, возвращается null.
    ///// </summary>
    //public NodeStateIcon NodeStateIcon { get { return GetFirstNodeControl<NodeStateIcon>(); } }


    #endregion

    #region Извлечение текста

    /// <summary>
    /// Возвращает двумерный массив строк, соответствующий выбранным строкам
    /// </summary>
    /// <returns></returns>
    public string[,] GetSelectedNodesTextMatrix()
    {
      TreeNodeAdv[] nodes = new TreeNodeAdv[Control.SelectedNodes.Count];
      Control.SelectedNodes.CopyTo(nodes, 0);
      return GetTextMatrix2(nodes);
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

      try
      {
        // 27.12.2020 лишнее if (Control.Selection.Count > 0)
        Control.EnsureVisible(Control.Selection[Control.Selection.Count - 1]);
        if (Control.Selection.Count > 1) // 25.03.2024
          Control.EnsureVisible(Control.Selection[0]);
      }
      catch { } // 25.03.2024
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
          TreeNodeAdv node = Control.Root;
          while (true)
          {
            if (node.IsLeaf)
              return node;
            if (node.Children.Count == 0)
              return node;
            node = node.Children[node.Children.Count - 1];
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
    public UISelectedRowsState SelectedRowsState
    {
      get
      {
        switch (Control.SelectedNodes.Count)
        {
          case 0: return UISelectedRowsState.NoSelection;
          case 1: return UISelectedRowsState.SingleRow;
          default: return UISelectedRowsState.MultiRows;
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
            throw ExceptionFactory.ArgUnknownValue("mode", mode, null);
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
            throw ExceptionFactory.ArgUnknownValue("mode", mode, null);
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

    bool IEFPTreeView.HasNodes
    {
      get
      {
        return Control.Root.Nodes.Count > 0;
      }
    }

    /// <summary>
    /// Значение свойства <see cref="EFPControlBase.DisplayName"/>, если оно не задано в явном виде
    /// </summary>
    protected override string DefaultDisplayName { get { return Res.EFPTreeViewAdv_Name_Default; } }

    /// <summary>
    /// Возвращает позицию для будущего выпадающего блока диалога, который будет показан для редактирования ячейки.
    /// В возвращаемом объекте устанавливается свойство <see cref="EFPDialogPosition.PopupOwnerBounds"/>.
    /// Если нет текущей ячейки (просмотр пустой) или текущая ячейка прокручены мышью за пределы видимой области просмотра,
    /// возвращается неинициализированный <see cref="EFPDialogPosition.PopupOwnerBounds"/>.
    /// </summary>
    public EFPDialogPosition CurrentPopupPosition
    {
      get
      {
        EFPDialogPosition pos = new EFPDialogPosition();
        if (Control.SelectedNode != null)
        {
          System.Drawing.Rectangle rect = Control.GetNodeBounds(Control.SelectedNode);
          if (!rect.IsEmpty)
          {
            rect = Control.RectangleToScreen(rect);
            pos.PopupOwnerBounds = rect;
          }
        }
        return pos;
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
  /// Хранилище значений флажков для <see cref="EFPTreeViewAdv"/>.
  /// Хранит перечислимые значения System.Windows.Forms.CheckState. Значения хранятся в привязке к свойству <see cref="TreeNodeAdv.Tag"/>,
  /// тип которого зависит от используемой модели.
  /// Чтобы использовать хранилище, создайте объект и установите свойства <see cref="EFPTreeViewAdv.CheckBoxes"/>=true
  /// и <see cref="EFPTreeViewAdv.CheckBoxStorage"/>.
  /// Хранилище можно использовать для нескольких элементов <see cref="EFPTreeViewAdv"/>.
  /// </summary>
  public class EFPTreeViewAdvCheckBoxStorage
  {
    #region Конструктор

    /// <summary>
    /// Создает пустое хранилище
    /// </summary>
    public EFPTreeViewAdvCheckBoxStorage()
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
        if (tag == null)
          return CheckState.Unchecked; // 27.02.2023
        CheckState res;
        if (_Dict.TryGetValue(tag, out res))
          return res;
        else
          return CheckState.Unchecked;
      }
      set
      {
        if (tag == null)
          throw new ArgumentNullException("tag");
        _Dict[tag] = value;
      }
    }

    #endregion

    #region Инициализация элемента

    /// <summary>
    /// Присоединяет хранилище к столбцу древовидного просмотра
    /// </summary>
    /// <param name="control">Столбец с флажками</param>
    internal void Attach(NodeCheckBox control)
    {
      control.VirtualMode = true;
      control.ValueNeeded += Control_ValueNeeded;
      control.ValuePushed += Control_ValuePushed;
    }

    /// <summary>
    /// Отсоединяет хранилище от столбца древовидного просмотра
    /// </summary>
    /// <param name="control">Столбец с флажками</param>
    internal void Detach(NodeCheckBox control)
    {
      control.ValueNeeded -= Control_ValueNeeded;
      control.ValuePushed -= Control_ValuePushed;
    }


    void Control_ValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      args.Value = this[args.Node.Tag];
    }

    void Control_ValuePushed(object sender, NodeControlValueEventArgs args)
    {
      if (args.Value is CheckState)
        this[args.Node.Tag] = (CheckState)(args.Value);
      else
        this[args.Node.Tag] = DataTools.GetBoolean(args.Value) ? CheckState.Checked : CheckState.Unchecked; // 09.02.2023
    }

    #endregion
  }
}
