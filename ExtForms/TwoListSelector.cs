using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.UICore;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Компонент для выбора элементов из списка доступных с возможностью сортировки
  /// </summary>
  public partial class TwoListSelector : UserControl
  {
    public TwoListSelector()
    {
      InitializeComponent();
    }
  }
}

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Аргументы события EFPTwoListSelector.ItemInfoNeeded
  /// </summary>
  public sealed class EFPTwoListSelectorItemInfoNeededEventArgs : EventArgs
  {
    #region Защищенный конструктор

    internal EFPTwoListSelectorItemInfoNeededEventArgs()
    {
    }

    internal void Init(object item, bool isSelected)
    {
      _Item = item;
      _IsSelected = isSelected;

      _TextValue = item.ToString();
      _ImageKey = "Item";
      _ToolTipText = String.Empty;
    }

    #endregion

    #region Свойства

    public object Item { get { return _Item; } }
    private object _Item;

    public bool IsSelected { get { return _IsSelected; } }
    private bool _IsSelected;


    /// <summary>
    /// Возвращает true, если всплывающая подсказка используется
    /// </summary>
    public bool ToolTipTextNeeded { get { return EFPApp.ShowToolTips; } }

    /// <summary>
    /// Возвращает true, если изображение используется
    /// </summary>
    public bool ImageNeeded { get { return EFPApp.ShowListImages; } }

    /// <summary>
    /// Сюда может быть помещен текст для вывода в комбоблоке
    /// </summary>
    public string TextValue { get { return _TextValue; } set { _TextValue = value; } }
    private string _TextValue;

    /// <summary>
    /// Сюда может быть помещен текст всплывающей подсказки
    /// </summary>
    public string ToolTipText { get { return _ToolTipText; } set { _ToolTipText = value; } }
    private string _ToolTipText;

    /// <summary>
    /// Сюда может быть помещено изображение
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPTwoListSelector.ItemInfoNeeded
  /// </summary>
  public delegate void EFPTwoListSelectorItemInfoNeededEventHandler(object sender,
    EFPTwoListSelectorItemInfoNeededEventArgs args);

  /// <summary>
  /// Аргументы события EFPTwoListSelector.EditItem
  /// </summary>
  public sealed class EFPTwoListSelectorEditItemEventArgs : EventArgs
  {
    #region Защищенный конструктор

    internal EFPTwoListSelectorEditItemEventArgs()
    {
    }

    #endregion

    #region Свойства

    public object Item
    {
      get { return _Item; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Item = value;
      }
    }
    private object _Item;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPTwoListSelector.EditItem
  /// </summary>
  public delegate void EFPTwoListSelectorEditItemEventHandler(object sender,
     EFPTwoListSelectorEditItemEventArgs args);

  public class EFPTwoListSelector : EFPControlBase
  {
    #region Конструкторы

    public EFPTwoListSelector(EFPBaseProvider baseProvider, TwoListSelector control)
      : base(baseProvider, control, false)
    {
      _AvailableGridViewProvider = new EFPDataGridView(baseProvider, control.AvailableGrid);

      control.AddButton.Image = EFPApp.MainImages.Images["RightRight"];
      control.AddButton.ImageAlign = ContentAlignment.MiddleCenter;
      _AddButtonProvider = new EFPButton(baseProvider, control.AddButton);
      _AddButtonProvider.DisplayName = "Добавить";

      control.RemoveButton.Image = EFPApp.MainImages.Images["LeftLeft"];
      control.RemoveButton.ImageAlign = ContentAlignment.MiddleCenter;
      _RemoveButtonProvider = new EFPButton(baseProvider, control.RemoveButton);
      _RemoveButtonProvider.DisplayName = "Удалить";

      _SelectedGridViewProvider = new EFPDataGridView(baseProvider, control.SelectedGrid);
      _SelectedGridViewProvider.ToolBarPanel = control.SelectedToolBarPanel;

      Init();
    }

    public EFPTwoListSelector(EFPDataGridView availableGridViewProvider, EFPButton addButtonProvider, EFPButton removeButtonProvider, EFPDataGridView selectedGridViewProvider)
      : base(availableGridViewProvider.BaseProvider, availableGridViewProvider.Control, false)
    {
      _AvailableGridViewProvider = availableGridViewProvider;
      _AddButtonProvider = addButtonProvider;
      _RemoveButtonProvider = removeButtonProvider;
      _SelectedGridViewProvider = selectedGridViewProvider;

      Init();
    }

    private void Init()
    {
      _AllItems = DataTools.EmptyObjects;
      _SelectedItemsInternal = null;
      _ItemInfoNeededArgs = new EFPTwoListSelectorItemInfoNeededEventArgs();
      _CanBeEmpty = true;
      _WarningIfEmpty = false;

      if (EFPApp.ShowListImages)
        _AvailableGridViewProvider.Columns.AddImage("Image");
      _AvailableGridViewProvider.Columns.AddTextFill("Text");
      _AvailableGridViewProvider.DisableOrdering();
      _AvailableGridViewProvider.Control.RowHeadersVisible = false;
      _AvailableGridViewProvider.Control.ColumnHeadersVisible = false;
      _AvailableGridViewProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(AvailableGridViewProvider_GetCellAttributes);
      _AvailableGridViewProvider.ReadOnly = true;
      _AvailableGridViewProvider.Control.ReadOnly = true;
      _AvailableGridViewProvider.CanView = false;
      _AvailableGridViewProvider.Control.MultiSelect = true;

      _AddButtonProvider.Enabled = false;
      _AddButtonProvider.Click += new EventHandler(AddButtonProvider_Click);

      _RemoveButtonProvider.Enabled = false;
      _RemoveButtonProvider.Click += new EventHandler(RemoveButtonProvider_Click);
     
      if (EFPApp.ShowListImages)
        _SelectedGridViewProvider.Columns.AddImage("Image");
      _SelectedGridViewProvider.Columns.AddTextFill("Text");
      _SelectedGridViewProvider.DisableOrdering();
      _SelectedGridViewProvider.Control.RowHeadersVisible = false;
      _SelectedGridViewProvider.Control.ColumnHeadersVisible = false;
      _SelectedGridViewProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(_SelectedGridViewProvider_GetCellAttributes);
      _SelectedGridViewProvider.ReadOnly = true;
      _SelectedGridViewProvider.CanInsert = false;
      _SelectedGridViewProvider.CanDelete = false;
      _SelectedGridViewProvider.Control.ReadOnly = true;
      _SelectedGridViewProvider.CanView = false;
      _SelectedGridViewProvider.EditData += new EventHandler(SelectedGridViewProvider_EditData);
      _SelectedGridViewProvider.CanMultiEdit = false;
      _SelectedGridViewProvider.Control.MultiSelect = true;
      _SelectedGridViewProvider.CommandItems.ManualOrderRows = true;
      _SelectedGridViewProvider.CommandItems.ManualOrderChanged += new EventHandler(SelectedGridViewProvider_ManualOrderChanged);

      // Убираем все лишние кнопки из панели инструментов
      foreach (EFPCommandItem ci in _SelectedGridViewProvider.CommandItems)
      {
        bool inTooBar = false;
        if (ci.Category == "Edit")
        {
          switch (ci.Name)
          {
            case "Edit":
            case "MoveUp":
            case "MoveDown":
              inTooBar = true;
              break;
          }
        }

        if (!inTooBar)
          ci.Usage &= ~EFPCommandItemUsage.ToolBar;
      }
      _SelectedGridViewProvider.Validating += new EFPValidatingEventHandler(SelectedGridViewProvider_Validating);
}

    protected override void OnCreated()
    {
      base.OnCreated();
      InitAvailableItemsInternal(); // ??
      _SelectedGridViewProvider.ReadOnly = !HasEditItemHandler;
    }

    #endregion

    #region Провайдеры дочерних элементов

    private EFPDataGridView _AvailableGridViewProvider;
    private EFPButton _AddButtonProvider;
    private EFPButton _RemoveButtonProvider;
    private EFPDataGridView _SelectedGridViewProvider;

    #endregion

    #region Переопределенные методы

    protected override bool ControlEnabled
    {
      get
      {
        return _SelectedGridViewProvider.Enabled;
      }
      set
      {
        _SelectedGridViewProvider.Enabled = value;
      }
    }

    protected override bool ControlVisible
    {
      get
      {
        return _SelectedGridViewProvider.Visible;
      }
      set
      {
        _SelectedGridViewProvider.Visible = value;

      }
    }

    #endregion

    #region Режим сортировки

    /// <summary>
    /// Можно ли переставлять местами элементы (true) или списки являются отсортированными (false).
    /// По умолчанию - true.
    /// Если списки являются отсортированными, то требуется, чтобы элементы должны реализовывать интерфейс IComparable,
    /// или должно быть установлено свойство Comparer.
    /// Свойство можно устанавливать только до вызова события Created.
    /// </summary>
    public bool AllowOrder
    {
      get { return _SelectedGridViewProvider.CommandItems.ManualOrderRows; }
      set
      {
        CheckHasNotBeenCreated();
        _SelectedGridViewProvider.CommandItems.ManualOrderRows = value;
      }
    }

    /// <summary>
    /// Внешний интерфейс для сравнения элементов.
    /// Используется при AllowOrder=false для сортировки элементов в просмотре.
    /// По умолчанию - null.
    /// Свойство можно устанавливать только до вызова события Created.
    /// </summary>
    public System.Collections.IComparer Comparer
    {
      get { return _Comparer; }
      set
      {
        CheckHasNotBeenCreated();
        _Comparer = value;
      }
    }
    private System.Collections.IComparer _Comparer;

    #endregion

    #region Список доступных элементов

    /// <summary>
    /// Полный список доступных элементов, включая выбранные
    /// </summary>
    public object[] AllItems
    {
      get { return _AllItems; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _AllItems = value;

        if (!AllowOrder)
        {
          System.Collections.ArrayList lst = new System.Collections.ArrayList(_AllItems);
          if (_Comparer == null)
            lst.Sort();
          else
            lst.Sort(_Comparer);
          _AllItems = lst.ToArray();
        }

        if (HasBeenCreated)
        {
          InitAvailableItemsInternal();
        }
      }
    }
    private object[] _AllItems;

    #endregion

    #region Список выбранных элементов

    public object[] SelectedItems
    {
      get
      {
        if (_SelectedItemsInternal == null)
        {
          _SelectedItemsInternal = new object[_SelectedGridViewProvider.Control.Rows.Count];
          for (int i = 0; i < _SelectedItemsInternal.Length; i++)
            _SelectedItemsInternal[i] = _SelectedGridViewProvider.Control.Rows[i].Tag;
        }
        return _SelectedItemsInternal;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _SelectedItemsInternal = value;
        if (!AllowOrder)
        {
          System.Collections.ArrayList lst = new System.Collections.ArrayList(value);
          if (_Comparer == null)
            lst.Sort();
          else
            lst.Sort(_Comparer);
          _SelectedItemsInternal = lst.ToArray();
        }
        _SelectedGridViewProvider.Control.RowCount = _SelectedItemsInternal.Length;
        for (int i = 0; i < _SelectedItemsInternal.Length; i++)
          _SelectedGridViewProvider.Control.Rows[i].Tag = _SelectedItemsInternal[i];
        _SelectedGridViewProvider.Control.Invalidate();
        _SelectedGridViewProvider.Validate();

        _RemoveButtonProvider.Enabled = value.Length > 0;

        InitAvailableItemsInternal();
      }
    }
    private object[] _SelectedItemsInternal;

    #endregion

    #region Список доступных элементов

    private void InitAvailableItemsInternal()
    {
      object[] a;
      if (SelectedItems.Length == 0)
        a = _AllItems;
      else
      {
        ArrayIndexer<object> selIndexer = new ArrayIndexer<object>(SelectedItems);
        // Нельзя использовать сразу массив, т.к. в массиве выбранных элементов могут быть посторонние элементы, которых нет в AllItems.
        System.Collections.ArrayList lst = new System.Collections.ArrayList(Math.Max(1, _AllItems.Length - SelectedItems.Length));
        for (int i = 0; i < _AllItems.Length; i++)
        {
          if (!selIndexer.Contains(_AllItems[i]))
            lst.Add(_AllItems[i]);
        }
        a = lst.ToArray();
      }

      _AvailableGridViewProvider.Control.RowCount = a.Length;
      for (int i = 0; i < a.Length; i++)
        _AvailableGridViewProvider.Control.Rows[i].Tag = a[i];
      _AvailableGridViewProvider.Control.Invalidate();

      _AddButtonProvider.Enabled = a.Length > 0;
    }

    #endregion

    #region Получение текста и значка для элемента

    public event EFPTwoListSelectorItemInfoNeededEventHandler ItemInfoNeeded;

    protected virtual void OnItemInfoNeeded(EFPTwoListSelectorItemInfoNeededEventArgs args)
    {
      if (ItemInfoNeeded != null)
        ItemInfoNeeded(this, args);
    }

    private EFPTwoListSelectorItemInfoNeededEventArgs _ItemInfoNeededArgs;

    private EFPTwoListSelectorItemInfoNeededEventArgs GetItemInfo(object item, bool isSelected)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      _ItemInfoNeededArgs.Init(item, isSelected);
      OnItemInfoNeeded(_ItemInfoNeededArgs);
      return _ItemInfoNeededArgs;
    }

    #endregion

    #region Редактирование элемента

    /// <summary>
    /// Событие обработчика редактирования элемента из списка выбранных.
    /// Вызывается, когда пользователь выполняет команду "Редактировать" для строки таблицы.
    /// Групповое редактирование нескольких строк не поддерживается.
    /// Обработчик получает ссылку на редактируемый элемент.
    /// Обработчик события не должен устанавливаться, если "редактирование" не имеет смысла. При этом
    /// команда редактирования не будет доступна.
    /// </summary>
    public event EFPTwoListSelectorEditItemEventHandler EditItem;

    /// <summary>
    /// Возвращает true, если установлен обработчик события EditItem
    /// </summary>
    public bool HasEditItemHandler { get { return EditItem != null; } }

    #endregion

    #region Обработчики элементов

    void AvailableGridViewProvider_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.RowIndex < 0 || args.RowIndex >= _AvailableGridViewProvider.Control.Rows.Count)
        return;
      object item = _AvailableGridViewProvider.Control.Rows[args.RowIndex].Tag;
      EFPTwoListSelectorItemInfoNeededEventArgs info = GetItemInfo(item, false);
      switch (args.ColumnName)
      {
        case "Image":
          args.Value = EFPApp.MainImages.Images[info.ImageKey];
          break;
        case "Text":
          args.Value = info.TextValue;
          break;
      }
    }

    void AddButtonProvider_Click(object sender, EventArgs args)
    {
      DataGridViewRow[] rows1 = _AvailableGridViewProvider.SelectedGridRows;
      if (rows1.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет элементов в списке доступных");
        return;
      }

      int n = _SelectedGridViewProvider.Control.Rows.Count;
      _SelectedGridViewProvider.Control.Rows.Add(rows1.Length);
      int[] SelectedRowIndexes = new int[rows1.Length];
      for (int i = 0; i < rows1.Length; i++)
      {
        _SelectedGridViewProvider.Control.Rows[n + i].Tag = rows1[i].Tag;
        SelectedRowIndexes[i] = n + i;
      }
      _SelectedItemsInternal = null;
      _SelectedGridViewProvider.SelectedRowIndices = SelectedRowIndexes;
      SelectedItems = SelectedItems;
    }

    void RemoveButtonProvider_Click(object sender, EventArgs args)
    {
      DataGridViewRow[] rows1 = _SelectedGridViewProvider.SelectedGridRows;
      if (rows1.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет элементов в списке выбранных");
        return;
      }

      for (int i = 0; i < rows1.Length; i++)
        _SelectedGridViewProvider.Control.Rows.Remove(rows1[i]);
      _SelectedItemsInternal = null;
      SelectedItems = SelectedItems;
    }

    void _SelectedGridViewProvider_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.RowIndex < 0 || args.RowIndex >= _SelectedGridViewProvider.Control.Rows.Count)
        return;
      object item = _SelectedGridViewProvider.Control.Rows[args.RowIndex].Tag;
      EFPTwoListSelectorItemInfoNeededEventArgs info = GetItemInfo(item, true);
      switch (args.ColumnName)
      {
        case "Image":
          args.Value = EFPApp.MainImages.Images[info.ImageKey];
          break;
        case "Text":
          args.Value = info.TextValue;
          break;
      }
    }

    void SelectedGridViewProvider_ManualOrderChanged(object sender, EventArgs args)
    {
      _SelectedItemsInternal = null;
    }

    void SelectedGridViewProvider_EditData(object sender, EventArgs args)
    {
      if (EditItem == null)
        return;

      EFPTwoListSelectorEditItemEventArgs args2 = new EFPTwoListSelectorEditItemEventArgs();
      args2.Item = _SelectedGridViewProvider.CurrentGridRow.Tag;
      EditItem(this, args2);
      _SelectedGridViewProvider.CurrentGridRow.Tag = args2.Item; // На случай, если Item является структурой, а не классом
      _SelectedItemsInternal = null;
      SelectedItems = SelectedItems; // список может быть переупорядочен
    }

    void SelectedGridViewProvider_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;

      if (!CanBeEmpty)
      {
        if (_SelectedGridViewProvider.Control.RowCount == 0)
        {
          args.SetError("Список не может быть пустым");
          return;
        }
      }
      else if (WarningIfEmpty && args.ValidateState==UIValidateState.Ok)
      {
        if (_SelectedGridViewProvider.Control.RowCount == 0)
          args.SetWarning("Список не должен быть пустым");
      }
    }

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// True, если ли список выбранных элементов может быть пустым.
    /// Значение по умолчанию - true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        if (value == _CanBeEmpty)
          return;
        _CanBeEmpty = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// True, если ли элемент содержать пустой текст.
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Выдавать предупреждение, если текст не введен (при условии, что CanBeEmpty=true)
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Если True и свойство CanBeEmpty=True, то при проверке состояния выдается
    /// предупреждение, если свойство Text содержит пустую строку
    /// По умолчанию - False
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion
  }
}
