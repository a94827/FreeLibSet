// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Config;
using FreeLibSet.Data;
using FreeLibSet.Logging;
using FreeLibSet.Data.Docs;
using FreeLibSet.Models.Tree;
using System.Collections.Generic;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  #region Перечисление DocTableViewMode

  /// <summary>
  /// Режим работы формы DocTableViewForm и SubDocTableViewForm
  /// </summary>
  public enum DocTableViewMode
  {
    /// <summary>
    /// Обычный режим просмотра
    /// </summary>
    Browse,

    /// <summary>
    /// Режим выбора одного документа
    /// </summary>
    SelectSingle,

    /// <summary>
    /// Режим выбора одного или нескольких документов без использования флажков.
    /// При этом выбранные строки являются результатом. Нельзя задать выбранные
    /// строки на входе
    /// </summary>
    SelectMulti,

    /// <summary>
    /// Режим выбора нескольких документов из заданного массива идентификаторов
    /// с использованием флажков для отметки выбранных документов
    /// </summary>
    SelectMultiWithFlags
  }

  #endregion

  #region Перечисление DocViewFormActiveTab

  /// <summary>
  /// Какая вкладка активна в форме просмотра документов или поддокументов: таблица или иерархический просмотр
  /// </summary>
  public enum DocViewFormActiveTab
  {
    /// <summary>
    /// Активен табличный просмотр
    /// </summary>
    Grid,

    /// <summary>
    /// Активен иерархиченский просмотр
    /// </summary>
    Tree
  }

  #endregion

  /// <summary>
  /// Форма табличного просмотра или выбора документов.
  /// Кроме основной таблицы, может содержать кнопки ОК/Отмена, табличку фильтров, и просмотр в виде дерева.
  /// Поддерживается просмотр групп документов.
  /// </summary>
  public partial class DocTableViewForm : Form
  {
    #region Конструктор и Disposing

    internal DocTableViewForm()
    {
      InitializeComponent();
      // так не работает
      //TheSplitter.Panel1.TabIndex = 2;
      //TheSplitter.Panel2.TabIndex = 0;
    }

    /// <summary>
    /// Создает форму
    /// </summary>
    /// <param name="docTypeUI">Интерфейс пользователя для просматриваемых документов</param>
    /// <param name="mode">Режим просмотра или выбора документов</param>
    public DocTableViewForm(DocTypeUI docTypeUI, DocTableViewMode mode)
    {
      InitializeComponent();

      base.Text = docTypeUI.DocType.PluralTitle;
      base.Icon = EFPApp.MainImages.Icons[docTypeUI.TableImageKey];

      _FormProvider = new EFPFormProvider(this);

      _ViewProvider = new EFPDocTableView(this, docTypeUI, mode);

      TheButtonPanel.Visible = (mode != DocTableViewMode.Browse);

      _FormProvider.ConfigSectionName = DocTypeName;
      _FormSearchKey = String.Empty;

      _ViewProvider.SaveFormConfig = false; // сами делаем, т.к. нужно еще записать параметр "FormSearchKey"
      _FormProvider.ConfigHandler.Sources.Add(new FormConfigurable(this));
    }

    /// <summary>
    /// Вызывается при закрытии формы.
    /// </summary>
    /// <param name="args"></param>
    protected override void OnFormClosing(FormClosingEventArgs args)
    {
      _FormProvider.ConfigHandler.Changed[EFPConfigCategories.Form] = true; // может быть, не всегда

      base.OnFormClosing(args);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной управляющий элемент
    /// </summary>
    public EFPDocTableView ViewProvider { get { return _ViewProvider; } }
    private EFPDocTableView _ViewProvider;

    /// <summary>
    /// Обработчик формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Иерархический просмотр. Если для вида документа не определено поле ParentId, 
    /// содержит значение null
    /// </summary>
    public EFPDocTreeView DocTreeView { get { return ViewProvider.DocTreeView; } }

    /// <summary>
    /// Основной табличный просмотр документов
    /// </summary>
    public EFPDocGridView DocGridView { get { return ViewProvider.DocGridView; } }

    /// <summary>
    /// Табличка фильтров (управляется основным просмотром DocView)
    /// </summary>
    public EFPGridFilterGridView FilterView { get { return ViewProvider.FilterView; } }

    /// <summary>
    /// Интерфейс пользователя для просматриваемых документов
    /// </summary>
    public DocTypeUI DocTypeUI { get { return ViewProvider.DocTypeUI; } }

    /// <summary>
    /// Имя таблицы просматриваемых документов
    /// </summary>
    public string DocTypeName { get { return ViewProvider.DocTypeName; } }

    /// <summary>
    /// Режим работы формы
    /// </summary>
    public DocTableViewMode Mode { get { return ViewProvider.Mode; } }

    /// <summary>
    /// Используется для поиска уже открытой формы
    /// </summary>
    public string FormSearchKey
    {
      get { return _FormSearchKey; }
      set
      {
        if (value == null)
          _FormSearchKey = String.Empty;
        else
          _FormSearchKey = value;
      }
    }
    private string _FormSearchKey;

    /// <summary>
    /// Действительно в режиме выбора 
    /// Если установлено в true, то доступна кнопка "Нет"
    /// </summary>
    public bool CanBeEmpty
    {
      get { return ViewProvider.CanBeEmpty; }
      set
      {
        ViewProvider.CanBeEmpty = value;
        TheNoButton.Visible = value;
      }
    }

    /// <summary>
    /// Если это свойство установлено, то вместо фильтров, выбираемых пользователем,
    /// будут использованы эти фильтры. Пользователь не может их редактировать
    /// </summary>
    public GridFilters ExternalFilters
    {
      get { return ViewProvider.ExternalFilters; }
      set { ViewProvider.ExternalFilters = value; }
    }

    /// <summary>
    /// Внешний инициализатор для новых документов
    /// Если свойство установлено, то при создании нового документа в качестве
    /// инициализатора значений полей (аргумент Caller при вызове ClientDocType.PerformEdit()) 
    /// будет использован этот инициализатор вместо текущих фильтров (DocGridHandler.DocFilters)
    /// </summary>
    public DocumentViewHandler ExternalEditorCaller
    {
      get { return ViewProvider.ExternalEditorCaller; }
      set { ViewProvider.ExternalEditorCaller = value; }
    }

    ///// <summary>
    ///// Если это событие установлено, то оно будет вызвано после того, как будут
    ///// прочитаны значения фильтров, сохраненные в конфигурации
    ///// </summary>
    //public event InitEFPDBxViewEventHandler InitFilters;

    /// <summary>
    /// Выбор активная вкладка формы, когда вид документа поддерживает иерархический просмотр в дереве
    /// </summary>
    public DocViewFormActiveTab ActiveTab
    {
      get { return ViewProvider.ActiveTab; }
      set { ViewProvider.ActiveTab = value; }
    }

    /// <summary>
    /// Идентификатор текущего документа
    /// </summary>
    public Int32 CurrentDocId
    {
      get { return ViewProvider.CurrentDocId; }
      set { ViewProvider.CurrentDocId = value; }
    }

    /// <summary>
    /// Идентификаторы выбранных документов
    /// </summary>
    public Int32[] SelectedDocIds
    {
      get { return ViewProvider.SelectedDocIds; }
      set { ViewProvider.SelectedDocIds = value; }
    }

    #endregion

    #region Обработчики кнопок

    private void FormOKButton_Click(object sender, EventArgs args)
    {
      if (!Modal)
      {
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void FormCancelButton_Click(object sender, EventArgs args)
    {
      if (!Modal)
      {
        DialogResult = DialogResult.Cancel;
        Close();
      }
    }

    private void FormNoButton_Click(object sender, EventArgs args)
    {
      if (!Modal)
      {
        DialogResult = DialogResult.No;
        Close();
      }
    }

    #endregion

    #region Обработчики формы

    /// <summary>
    /// Устанавливает фокус на таблицу или дерево документов при показе формы
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);
      if (Visible)
      {
        // 19.05.2021, 17.02.2022
        // Еще бывает дерево и комбоблок выбора группы.
        // Активировать надо таблицу
        if (ActiveTab == DocViewFormActiveTab.Grid)
          ViewProvider.DocGridView.SetFocus();
        else
          ViewProvider.DocTreeView.SetFocus();
      }
    }
    /// <summary>
    /// Вызывается при активации формы
    /// </summary>
    /// <param name="args"></param>
    protected override void OnActivated(EventArgs args)
    {
      base.OnActivated(args);

      try
      {
        // Тут у меня несчастье. Если засунуть вызовы ActiveControl и Select() в
        // другое место, то появляются глюки. Например, если сделать в VisibleChanged
        // (при Visible=true), то первый раз нормально, а после повторной активации
        // формы фокус сбрасывается на дерево иерархии.
        // Если сделать в конструкторе (как было бы логично), то еще хуже: при активации
        // формы после закрытия другой, она "пружинит" и делает активной другую форму
        // Юзер просто одуреет от такого интерфейса.
        //
        // Так, конечно, коряво, но работает. Все равно из дерева и кнопок никаких 
        // других MDI-форм не появляется, поэтому лишней активации не происходит и
        // фокус не прыгает

        ViewProvider.ActiveTab = ViewProvider.ActiveTab;

      }
      catch (Exception e)
      {
        // Не стоит вызывать EFPApp.ShowException(), чтобы не провоцировать 
        // повторную активацию
        LogoutTools.LogoutException(e, "DocTableViewForm.OnActivated()");
      }
    }

    #endregion

    #region Сохранение внешнего вида формы

    /// <summary>
    /// Не хочется реализовывать IEFPConfigurable непосредственно в DocTableViewForm
    /// </summary>
    private class FormConfigurable : IEFPConfigurable
    {
      #region Конструктор

      public FormConfigurable(DocTableViewForm form)
      {
        _Form = form;
      }

      private DocTableViewForm _Form;

      #endregion

      #region IEFPConfigurable Members

      public void GetConfigCategories(System.Collections.Generic.ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
      {
        categories.Add(EFPConfigCategories.Form);
      }

      public void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
      {
        if (category == EFPConfigCategories.Form)
        {
          if (actionInfo.Purpose == EFPConfigPurpose.Composition)
            cfg.SetString("FormSearchKey", _Form.FormSearchKey);

          _Form.ViewProvider.WriteFormConfigPart(cfg);
        }
      }

      public void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
      {
        if (category == EFPConfigCategories.Form)
        {
          if (actionInfo.Purpose == EFPConfigPurpose.Composition)
            _Form.FormSearchKey = cfg.GetString("FormSearchKey");
          _Form.ViewProvider.ReadFormConfigPart(cfg);
        }
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Провайдер составного управляющего элемента для табличного просмотра или выбора документов.
  /// Кроме основной таблицы, может содержать табличку фильтров и просмотр в виде дерева.
  /// Поддерживается просмотр групп документов.
  /// </summary>
  public class EFPDocTableView : EFPControl<Control>
  {
    #region Конструкторы

    internal EFPDocTableView(DocTableViewForm form, DocTypeUI docTypeUI, DocTableViewMode mode)
      : base(form.FormProvider, form.ControlPanel, false)
    {
      Init(form, form.FormProvider, docTypeUI, mode);
    }

    /// <summary>
    /// Создает провайдер составного элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="parent">Пустая панель, куда будет добавлен составной элемент</param>
    /// <param name="docTypeUI">Интерфейс для доступа к просматриваемым документам</param>
    /// <param name="mode">Режим просмотра списка или выбора документов</param>
    public EFPDocTableView(EFPBaseProvider baseProvider, Control parent, DocTypeUI docTypeUI, DocTableViewMode mode)
      : base(baseProvider, parent, false)
    {
      if (parent.HasChildren)
        throw new ArgumentException("В панели не должно быть управляющих элементов", "parent");

      DocTableViewForm dummyForm = new DocTableViewForm();

      WinFormsTools.MoveControls(dummyForm.ControlPanel, parent);

      Init(dummyForm, baseProvider, docTypeUI, mode);

      base.InitConfigHandler();
      base.ConfigSectionName = DocTypeName; // после InitConfigHandler
    }

    private void Init(DocTableViewForm form, EFPBaseProvider baseProvider, DocTypeUI docTypeUI, DocTableViewMode mode)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");

      _Form = form;

      if (!String.IsNullOrEmpty(docTypeUI.DocType.TreeParentColumnName))
      {
        #region Есть древовидный просмотр

        _TheTabControl = new TabControl();
        _TheTabControl.Dock = DockStyle.Fill;
        _TheTabControl.ImageList = EFPApp.MainImages.ImageList;
        form.MainPanel.Controls.Add(_TheTabControl);

        TabPage tpTree = new TabPage("Дерево");
        _TheTabControl.Controls.Add(tpTree);
        tpTree.ImageKey = "TreeView";

        _DocTree = new TreeViewAdv();
        _DocTree.Dock = DockStyle.Fill;
        _DocTree.Name = "DocTree";
        tpTree.Controls.Add(_DocTree);

        _DocTreeSpeedPanel = new Panel();
        _DocTreeSpeedPanel.Dock = DockStyle.Top;
        tpTree.Controls.Add(_DocTreeSpeedPanel);

        TabPage tpTable = new TabPage("Таблица");
        _TheTabControl.Controls.Add(tpTable);
        tpTable.ImageKey = "Table";

        _DocGrid = new DataGridView();
        _DocGrid.Dock = DockStyle.Fill;
        _DocGrid.Name = "DocGrid";
        tpTable.Controls.Add(_DocGrid);

        _DocGridSpeedPanel = new Panel();
        _DocGridSpeedPanel.Dock = DockStyle.Top;
        tpTable.Controls.Add(_DocGridSpeedPanel);

        #endregion
      }
      else
      {
        #region Нет древовидного просмотра

        _DocGrid = new DataGridView();
        _DocGrid.Dock = DockStyle.Fill;
        _DocGrid.Name = "DocGrid";
        form.MainPanel.Controls.Add(_DocGrid);

        _DocGridSpeedPanel = new Panel();
        _DocGridSpeedPanel.Dock = DockStyle.Top;
        form.MainPanel.Controls.Add(_DocGridSpeedPanel);

        #endregion
      }

      if (!String.IsNullOrEmpty(docTypeUI.DocType.TreeParentColumnName))
      {
        _DocTreeView = new EFPDocTreeView(baseProvider, _DocTree, docTypeUI);
        _DocTreeView.DisplayName = "Дерево документов";
        _DocTreeView.CommandItems.EnterAsOk = (mode != DocTableViewMode.Browse);
        _DocTreeView.ToolBarPanel = _DocTreeSpeedPanel;
      }

      _DocGridView = new EFPDocGridView(baseProvider, _DocGrid, docTypeUI);
      _DocGridView.DisplayName = "Таблица документов";
      _DocGridView.CommandItems.EnterAsOk = (mode != DocTableViewMode.Browse);
      _DocGridView.ToolBarPanel = _DocGridSpeedPanel;
      if (mode == DocTableViewMode.SelectSingle)
      {
        _DocGridView.Control.MultiSelect = false;
        _DocGridView.CanMultiEdit = false;
        if (_DocTreeView != null)
        {
          _DocTreeView.Control.SelectionMode = TreeViewAdvSelectionMode.Single;
          _DocTreeView.CanMultiEdit = false;
        }
      }
      else if (mode == DocTableViewMode.SelectMultiWithFlags)
      {
        _DocGridView.MarkRowIds = IdList.Empty; // 13.01.2022
        //if (_DocTreeView!=null)
        // TODO: _DocTreeView.MarkRowIds = IdList.Empty; 
      }


      _DocGridView.LoadConfig(); // табличка фильтров нужна сразу
      if (_DocTreeView != null)
      {
        _DocTreeView.BrowserGuid = _DocGridView.BrowserGuid; // одинаковый идентификатор просмотра
        _DocTreeView.Filters = _DocGridView.Filters; // Общие фильтры
        // Синхронизаторы
        new EFPDBxViewSync(_DocGridView, _DocTreeView);
        new EFPDocViewFilterSync(_DocGridView, _DocTreeView);
      }

      _FilterView = new EFPGridFilterGridView(_DocGridView, form.FilterGrid);
      _FilterView.DisplayName = "Табличка фильтров";
      _Mode = mode;

      #region Дерево групп

      if (GroupAllowed)
      {
        #region GroupTreeView

        _GroupTreeView = new EFPGroupDocTreeView(baseProvider, form.GroupTree, GroupTypeUI);
        _GroupTreeView.DisplayName = "Дерево групп";
        AddOpenGroupWindowCommandItem(_GroupTreeView.CommandItems);
        _GroupTreeView.CommandItems.UseRefresh = false; // обновляется из основного просмотра
        _GroupTreeView.ToolBarPanel = form.GroupSpeedPanel;

        // 18.11.2017. Сохраняем текущую выбранную группу вручную.
        // Нельзя полагаться на автоматическое сохранение свойства EFPGroupTreeView.CurrentId, 
        // так как оно не происходит, если панель дерева ни разу не была включена во время показа
        // формы, а использовался исключительно EFPGroupComboBox
        _GroupTreeView.SaveCurrentId = false;
        _GroupTreeView.BrowserGuid = _DocGridView.BrowserGuid; // одинаковый идентификатор просмотра
        _GroupTreeView.Control.SelectionChanged += new EventHandler(GroupTreeView_SelectionChanged);


        #endregion

        #region GroupComboBox

        _GroupComboBox = new EFPGroupDocComboBox(baseProvider, form.GroupCB, GroupTypeUI);
        _GroupComboBox.DisplayName = "Выпадающий список групп";
        _GroupComboBox.DocIdEx.ValueChanged += new EventHandler(CurrentGroupChanged);
        _GroupComboBox.IncludeNestedEx.ValueChanged += new EventHandler(IncludeNestedChanged);
        AddOpenGroupWindowCommandItem(_GroupComboBox.CommandItems);
        _GroupComboBox.BrowserGuid = _DocGridView.BrowserGuid; // одинаковый идентификатор просмотра

        #endregion

        #region DocGridView и DocTreeView

        GroupCommandItems1 = new GroupCommandItems(this, _DocGridView.CommandItems);
        if (_DocTreeView != null)
          GroupCommandItems2 = new GroupCommandItems(this, _DocTreeView.CommandItems);

        _DocGridView.RefreshData += new EventHandler(RefreshGroupTree);
        if (_DocTreeView != null)
          _DocTreeView.RefreshData += new EventHandler(RefreshGroupTree);

        #endregion

        InitGroupTreeViewRootNode(); // должно быть после инициализации GroupComboBox
        UpdateGroupCommandItems();
      }

      GroupTreeVisible = false;
      GroupCBVisible = false;

      #endregion

      _SaveFormConfig = true;
    }

    #endregion

    #region Общие свойства

    private DocTableViewForm _Form;

    /// <summary>
    /// Существует, если есть древовидный просмотр
    /// </summary>
    private TabControl _TheTabControl;

    private FreeLibSet.Controls.TreeViewAdv _DocTree;
    private Panel _DocTreeSpeedPanel;

    private DataGridView _DocGrid;
    private Panel _DocGridSpeedPanel;

    /// <summary>
    /// Иерархический просмотр. Если для вида документа не определено поле ParentId, 
    /// содержит значение null
    /// </summary>
    public EFPDocTreeView DocTreeView { get { return _DocTreeView; } }
    private EFPDocTreeView _DocTreeView;

    /// <summary>
    /// Основной табличный просмотр документов
    /// </summary>
    public EFPDocGridView DocGridView { get { return _DocGridView; } }
    private EFPDocGridView _DocGridView;

    /// <summary>
    /// Табличка фильтров (управляется основным просмотром DocView)
    /// </summary>
    public EFPGridFilterGridView FilterView { get { return _FilterView; } }
    private EFPGridFilterGridView _FilterView;

    /// <summary>
    /// Интерфейс пользователя для просматриваемых документов
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocGridView.DocTypeUI; } }

    /// <summary>
    /// Имя вида документов
    /// </summary>
    public string DocTypeName { get { return _DocGridView.DocTypeUI.DocType.Name; } }

    /// <summary>
    /// Режим работы формы
    /// </summary>
    public DocTableViewMode Mode { get { return _Mode; } }
    private DocTableViewMode _Mode;


    /// <summary>
    /// Действительно в режиме выбора 
    /// Если установлено в true, то доступна кнопка "Нет"
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set { _CanBeEmpty = value; }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// Если это свойство установлено, то вместо фильтров, выбираемых пользователем,
    /// будут использованы эти фильтры. Пользователь не может их редактировать
    /// </summary>
    public GridFilters ExternalFilters
    {
      get { return _ExternalFilters; }
      set
      {
        _ExternalFilters = value;
        if (value != null)
        {
          _DocGridView.CommandItems.CanEditFilters = false; // до установки фильтра
          if (_DocTreeView != null)
            _DocTreeView.CommandItems.CanEditFilters = false;
          _DocGridView.Filters = value;
          if (_DocTreeView != null)
            _DocTreeView.Filters = value;
        }
      }
    }
    private GridFilters _ExternalFilters;

    /// <summary>
    /// Внешний инициализатор для новых документов
    /// Если свойство установлено, то при создании нового документа в качестве
    /// инициализатора значений полей (аргумент Caller при вызове ClientDocType.PerformEdit()) 
    /// будет использован этот инициализатор вместо текущих фильтров (DocGridHandler.DocFilters)
    /// </summary>
    public DocumentViewHandler ExternalEditorCaller
    {
      get { return _DocGridView.ExternalEditorCaller; }
      set { _DocGridView.ExternalEditorCaller = value; }
    }

    ///// <summary>
    ///// Если это событие установлено, то оно будет вызвано после того, как будут
    ///// прочитаны значения фильтров, сохраненные в конфигурации
    ///// </summary>
    //public event InitEFPDBxViewEventHandler InitFilters;

    /// <summary>
    /// Выбор активной вкладка формы, когда вид документа поддерживает иерархический просмотр в дереве
    /// </summary>
    public DocViewFormActiveTab ActiveTab
    {
      get
      {
        if (_DocTreeView == null)
          return DocViewFormActiveTab.Grid;
        else
        {
          if (_TheTabControl.SelectedIndex <= 0)  // -1, если просмотр еще не выведен
            return DocViewFormActiveTab.Tree;
          else
            return DocViewFormActiveTab.Grid;
        }
      }
      set
      {
        switch (value)
        {
          case DocViewFormActiveTab.Tree:
            if (_DocTreeView == null)
              throw new InvalidOperationException("Форма не содержит вкладки иерархического просмотра");
            else
              _TheTabControl.SelectedIndex = 0;
            break;
          case DocViewFormActiveTab.Grid:
            if (_DocTreeView != null)
              _TheTabControl.SelectedIndex = 1;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }

    /// <summary>
    /// Возвращает название для отладки
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return "Составная панель просмотра документов"; }
    }


    #endregion

    #region Свойства CurrentDocId и SelectedDocIds

    // 13.06.2019
    // Используем отложенную установку свойств, пока форма не выведена на экран.
    // Нужно, чтобы скорректировать список групп

    /// <summary>
    /// Идентификатор текущего документа.
    /// При установке свойства может измениться текущая выбранная группа (свойство CurrentGroupId),
    /// если текущая группа не подходит.
    /// </summary>
    public Int32 CurrentDocId
    {
      get
      {
        if (HasBeenCreated)
        {
          switch (ActiveTab)
          {
            case DocViewFormActiveTab.Grid: return DocGridView.CurrentId;
            case DocViewFormActiveTab.Tree: return DocTreeView.CurrentId;
            default:
              throw new BugException();
          }
        }
        else
          return _DelayedCurrentDocId;
      }
      set
      {
        if (HasBeenCreated)
          SetCurrentDocId(value);
        else
          _DelayedCurrentDocId = value; // откладываем на потом
      }
    }

    /// <summary>
    /// Запоминаем CurrentDocId, если форма еще не была выведена на экран
    /// </summary>
    private Int32 _DelayedCurrentDocId;

    private void SetCurrentDocId(Int32 value)
    {
      if (value != 0)
        CorrectGroupForDocIds(new Int32[1] { value });

      // 16.11.2017
      DocGridView.CurrentId = value;
      if (DocTreeView != null)
        DocTreeView.CurrentId = value;
    }

    /// <summary>
    /// Идентификаторы выбранных документов
    /// </summary>
    public Int32[] SelectedDocIds
    {
      get
      {
        if (HasBeenCreated)
        {
          switch (ActiveTab)
          {
            case DocViewFormActiveTab.Grid:
              if (Mode == DocTableViewMode.SelectMultiWithFlags)
                return DocGridView.MarkRowIds.ToArray();
              else
                return DocGridView.SelectedIds;
            case DocViewFormActiveTab.Tree:
              return DocTreeView.SelectedIds;
            default:
              throw new BugException();
          }
        }
        else
          return _DelayedSelectedDocIds;
      }
      set
      {
        if (HasBeenCreated)
          SetSelectedDocIds(value);
        else
          _DelayedSelectedDocIds = value;
      }
    }

    /// <summary>
    /// Запоминаем SelectedDocIds, если форма еще не была выведена на экран
    /// </summary>
    private Int32[] _DelayedSelectedDocIds;

    private void SetSelectedDocIds(Int32[] value)
    {
      if (value != null)
        CorrectGroupForDocIds(value);

      switch (ActiveTab)
      {
        case DocViewFormActiveTab.Grid:
          if (value == null)
            value = DataTools.EmptyIds;
          if (Mode == DocTableViewMode.SelectMultiWithFlags)
            DocGridView.MarkRowIds = new IdList(value);
          DocGridView.SelectedIds = value;
          break;
        case DocViewFormActiveTab.Tree:
          DocTreeView.SelectedIds = value;
          break;
      }
    }

    private void InitDelayedProperties()
    {
      try
      {
        if (_DelayedSelectedDocIds != null)
        {
          SetSelectedDocIds(_DelayedSelectedDocIds);
          _DelayedSelectedDocIds = null;
        }
        if (_DelayedCurrentDocId != 0)
        {
          SetCurrentDocId(_DelayedCurrentDocId);
          _DelayedCurrentDocId = 0;
        }
      }
      catch { } // 15.04.2022
    }

    #endregion

    #region Работа с группами

    /// <summary>
    /// Возвращает true, если для вида документов поддерживаются группы
    /// </summary>
    public bool GroupAllowed { get { return DocTypeUI.GroupDocType != null; } }

    /// <summary>
    /// Интерфейс для работы с документами группы
    /// </summary>
    public GroupDocTypeUI GroupTypeUI { get { return DocTypeUI.GroupDocType; } }


    /// <summary>
    /// true, если дерево групп отображается
    /// </summary>
    public bool GroupTreeVisible
    {
      get { return !_Form.TheSplitter.Panel1Collapsed; }
      set
      {
        _Form.TheSplitter.Panel1Collapsed = !value;
        UpdateGroupCommandItems();
      }
    }

    /// <summary>
    /// Процент, занимаемый панелью дерева групп (от 0 до 100, но крайние значения не поддерживаются)
    /// </summary>
    public int GroupTreePercent
    {
      get
      {
        return WinFormsTools.GetSplitContainerDistancePercent(_Form.TheSplitter);
      }
      set
      {
        WinFormsTools.SetSplitContainerDistancePercent(_Form.TheSplitter, value);
      }
    }

    /// <summary>
    /// true, если комбоблок выбора группы отображается
    /// </summary>
    public bool GroupCBVisible
    {
      get { return _GroupCBVisible; }
      set
      {
        _GroupCBVisible = value;
        _Form.GroupCBPanel.Visible = value;
        UpdateGroupCommandItems();
      }
    }
    private bool _GroupCBVisible;

    /// <summary>
    /// Провайдер дерева групп.
    /// Null, если GroupAllowed=false.
    /// </summary>
    public EFPGroupDocTreeView GroupTreeView { get { return _GroupTreeView; } }
    private EFPGroupDocTreeView _GroupTreeView;

    /// <summary>
    /// Провайдер комбоблока выбора группы.
    /// Null, если GroupAllowed=false.
    /// </summary>
    public EFPGroupDocComboBox GroupComboBox { get { return _GroupComboBox; } }
    private EFPGroupDocComboBox _GroupComboBox;

    /// <summary>
    /// Возващает true, если работа с группами осуществляется в данный момент
    /// </summary>
    public bool GroupActive
    {
      get
      {
        if (!GroupAllowed)
          return false;
        if (HasBeenCreated)
          return GroupCBVisible || GroupTreeVisible;
        else
          return true; // до показа формы - неизвестно
      }
    }

    /// <summary>
    /// Текущий выбранный идентификатор группы
    /// </summary>
    public Int32 CurrentGroupId
    {
      get
      {
        if (GroupActive)
          return GroupComboBox.DocId;
        else
          return 0;
      }
      set
      {
        if (GroupComboBox != null)
          GroupComboBox.DocId = value;
      }
    }

    /// <summary>
    /// Если true, то показываются документы из вложенных групп.
    /// Свойство возвращает true, если в данный момент работа с деревом не выполняется
    /// </summary>
    public bool IncludeNestedGroups
    {
      get
      {
        if (GroupActive)
          return GroupComboBox.IncludeNested;
        else
          return true;
      }
      set
      {
        if (GroupComboBox != null)
          GroupComboBox.IncludeNested = value;
      }
    }

    /// <summary>
    /// Корректировка текущей выбранной группы перед выбором документа или документов
    /// </summary>
    /// <param name="docIds">Идентификаторы выбираемых документов</param>
    private void CorrectGroupForDocIds(Int32[] docIds)
    {
      if (docIds.Length == 0)
        return;

      // лишняя проверка
      //if (!(GroupTreeVisible || GroupCBVisible))
      //  return;

      if (CurrentGroupId == 0 && IncludeNestedGroups)
        return; // выбраны все группы или работа с группами не выполняется

      SingleScopeList<Int32> wantedGroupIds = new SingleScopeList<Int32>(); // нельзя использовать IdList, т.к. идентификатор группы 0 может встретиться
      for (int i = 0; i < docIds.Length; i++)
        wantedGroupIds.Add(DocTypeUI.TableCache.GetInt(docIds[i], DocTypeUI.DocType.GroupRefColumnName));

      if (wantedGroupIds.Count == 1)
      {
        if (wantedGroupIds[0] == CurrentGroupId)
          return; // выбрана точно совпадающая группа.
      }

      ArrayIndexer<Int32> idxAuxFilterGroupIds = new ArrayIndexer<Int32>(AuxFilterGroupIds);
      if (idxAuxFilterGroupIds.ContainsAll(wantedGroupIds))
        return;

      // Устанавливаем группу

      if (wantedGroupIds.Count == 1)
        CurrentGroupId = wantedGroupIds[0];
      else
      {
        // Если выбраны документы для разных групп, сбрасываем все.
        // По идее, можно было бы поискать общую родительскую группу
        CurrentGroupId = 0;
        IncludeNestedGroups = true;
      }
    }

    #region Свойство AuxFilterGroupIds

    /// <summary>
    /// Возвращает массив идентификаторов отфильтрованных групп документов.
    /// Используется для фильтрации документов в основном просмотре.
    /// Если выбраны "Все документы", возвращает null.
    /// Если выбраны "Документы без групп", возвращает массив нулевой длины.
    /// Если есть выбранная группа, возвращает массив из одного или нескольких элементов,
    /// в зависимости от IncludeNested
    /// </summary>
    public Int32[] AuxFilterGroupIds
    {
      get
      {
        if (CurrentGroupId == 0)
        {
          if (IncludeNestedGroups)
            return null;
          else
            return DataTools.EmptyIds;
        }
        else
        {
          if (IncludeNestedGroups)
          {
            DataTableTreeModelWithIds<Int32> model;
            if (GroupCBVisible)
              model = GroupComboBox.Model;
            else if (GroupTreeVisible)
            {
              if (GroupTreeView.Control.Model == null)
                GroupTreeView.PerformRefresh(); // 17.05.2021
              model = GroupTreeView.Control.Model as DataTableTreeModelWithIds<Int32>;
              if (model == null)
              {
                if (GroupTreeView.Control.Model == null)
                  throw new NullReferenceException("Не установлено свойство GroupTreeView.Control.Model");
                else
                  throw new BugException("Неправильная модель в GroupTreeView");
              }
            }
            else
              throw new BugException("CurrentGroupId!=0 при скрытом дереве и комбоблоке выбора группы");

            return model.GetIdWithChildren(CurrentGroupId);
          }
          else
            return new Int32[1] { CurrentGroupId };
        }
      }
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Команды работы с группами, присоединяемые к табличному просмотру и просмотру дерева
    /// </summary>
    private class GroupCommandItems
    {
      #region Конструктор

      public GroupCommandItems(EFPDocTableView owner, EFPCommandItems commandItems)
      {
        _Owner = owner;

        commandItems.AddSeparator();

        ciGroupTreeVisible = new EFPCommandItem("View", "GroupTreeVisible");
        ciGroupTreeVisible.MenuText = "Дерево групп";
        ciGroupTreeVisible.ImageKey = "GroupDocTreePanel";
        ciGroupTreeVisible.Click += new EventHandler(ciGroupTreeVisible_Click);
        commandItems.Add(ciGroupTreeVisible);

        ciGroupCBVisible = new EFPCommandItem("View", "GroupCBVisible");
        ciGroupCBVisible.MenuText = "Выпадающий список групп";
        ciGroupCBVisible.ImageKey = "GroupDocTreeCB";
        ciGroupCBVisible.Click += new EventHandler(ciGroupCBVisible_Click);
        commandItems.Add(ciGroupCBVisible);

        commandItems.AddSeparator();

        ciHideNested = new EFPCommandItem("View", "HideNestedGroups");
        ciHideNested.MenuText = "Скрыть документы во вложенных группах";
        ciHideNested.ImageKey = "OnlyOneGroupDoc";
        ciHideNested.Click += new EventHandler(ciHideNested_Click);
        commandItems.Add(ciHideNested);

        commandItems.AddSeparator();
      }

      //public EFPDocTableView Owner { get { return _Owner; } }
      private EFPDocTableView _Owner;

      #endregion

      #region Видимость элементов формы

      private EFPCommandItem ciGroupTreeVisible, ciGroupCBVisible;

      void ciGroupTreeVisible_Click(object sender, EventArgs args)
      {
        _Owner.GroupTreeVisible = !_Owner.GroupTreeVisible;
        // Обновление команд выполняется формой
      }

      void ciGroupCBVisible_Click(object sender, EventArgs args)
      {
        _Owner.GroupCBVisible = !_Owner.GroupCBVisible;
        // Обновление команд выполняется формой
      }

      #endregion

      #region "Скрыть вложенные"

      EFPCommandItem ciHideNested;

      void ciHideNested_Click(object sender, EventArgs args)
      {
        _Owner.GroupComboBox.IncludeNested = !_Owner.GroupComboBox.IncludeNested;
      }

      #endregion

      #region Обновление команд

      public void Update()
      {
        ciGroupTreeVisible.Checked = _Owner.GroupTreeVisible;
        ciGroupCBVisible.Checked = _Owner.GroupCBVisible;
        ciHideNested.Visible = _Owner.GroupTreeVisible || _Owner.GroupCBVisible;
        ciHideNested.Checked = !_Owner.GroupComboBox.IncludeNested;
      }

      #endregion
    }

    private GroupCommandItems GroupCommandItems1, GroupCommandItems2;

    private void UpdateGroupCommandItems()
    {
      if (!(GroupTreeVisible || GroupCBVisible))
      {
        if (GroupComboBox != null)
        {
          // Если нет элементов для работы с деревом, то нельзя выбирать группу
          GroupComboBox.DocId = 0;
          GroupComboBox.IncludeNested = true;
        }
      }

      if (GroupCommandItems1 != null)
        GroupCommandItems1.Update();

      if (GroupCommandItems2 != null)
        GroupCommandItems2.Update();
    }


    private void AddOpenGroupWindowCommandItem(EFPCommandItems commandItems)
    {
      EFPCommandItem ciOpenGroupWindow = new EFPCommandItem("Edit", "OpenGroupWindow");
      ciOpenGroupWindow.MenuText = "Открыть просмотр групп в отдельном окне";
      ciOpenGroupWindow.Click += new EventHandler(ciOpenGroupWindow_Click);
      ciOpenGroupWindow.GroupBegin = true;
      ciOpenGroupWindow.GroupEnd = true;
      commandItems.Add(ciOpenGroupWindow);
    }

    void ciOpenGroupWindow_Click(object sender, EventArgs args)
    {
      this.GroupTreeView.DocTypeUI.ShowOrOpen(null);
    }

    #endregion

    void CurrentGroupChanged(object sender, EventArgs args)
    {
      if (!_GroupTreeView_InsideSelectionChanged)
      {
        _GroupTreeView_InsideSelectionChanged = true;
        try
        {
          GroupTreeView.CurrentId = GroupComboBox.DocId;
        }
        finally
        {
          _GroupTreeView_InsideSelectionChanged = false;
        }
      }
      DocGridView.AuxFilterGroupIds = this.AuxFilterGroupIds;
      if (DocTreeView != null)
        DocTreeView.AuxFilterGroupIds = DocGridView.AuxFilterGroupIds;
    }

    void IncludeNestedChanged(object sender, EventArgs args)
    {
      UpdateGroupCommandItems();
      CurrentGroupChanged(sender, args);
      InitGroupTreeViewRootNode();
    }

    private void InitGroupTreeViewRootNode()
    {
      GroupTreeView.IncludeNested = GroupComboBox.IncludeNested;
    }

    private bool _GroupTreeView_InsideSelectionChanged;

    private void GroupTreeView_SelectionChanged(object sender, EventArgs args)
    {
      if (_GroupTreeView_InsideSelectionChanged)
        return;

      _GroupTreeView_InsideSelectionChanged = true;
      try
      {
        GroupComboBox.DocId = GroupTreeView.CurrentId;
      }
      finally
      {
        _GroupTreeView_InsideSelectionChanged = false;
      }
    }

    /// <summary>
    /// Обновляем дерево и комбоблок групп при обновлении основного просмотра
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void RefreshGroupTree(object sender, EventArgs args)
    {
      if (GroupTreeView.HasBeenCreated)
        GroupTreeView.PerformRefresh();
      if (GroupComboBox.HasBeenCreated)
        GroupComboBox.PerformRefresh();
    }

    #endregion

    #region Обработчики формы

    /// <summary>
    /// Метод вызывается при первом открытии формы
    /// </summary>
    protected override void OnCreated()
    {
      if (!String.IsNullOrEmpty(Control.Name))
      {
        if (_DocTree != null)
          _DocTree.Name = Control.Name + _DocTree.Name;
        _DocGrid.Name = Control.Name + _DocGrid.Name;
      }

      base.OnCreated();

      //try
      //{
      //  switch (ActiveTab)
      //  {
      //    case DocViewFormActiveTab.Grid: ActiveControl = DocGrid; break;
      //    case DocViewFormActiveTab.Tree: ActiveControl = DocTree; break;
      //  }
      //}
      //catch (Exception e)
      //{
      //  // Не стоит вызывать EFPApp.ShowException(), чтобы не провоцировать 
      //  // повторную активацию
      //  LogoutTools.LogoutException(e, "EFPDocTableView.OnShown()");
      //}
    }

    /// <summary>
    /// Вызывается при появлении формы на экране
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      // 05.07.2021. Перенесено из OnCreated()
      Form frm = Control.FindForm();
      if (frm != null)
      {
        if (WinFormsTools.ContainsControl(Control, frm.ActiveControl))
        {
          switch (ActiveTab)
          {
            case DocViewFormActiveTab.Tree:
              _DocTreeView.SetFocus(); // 24.01.2022
              break;
            case DocViewFormActiveTab.Grid:
              _DocGridView.SetFocus(); // 11.12.2018
              break;
          }
        }
      }

      InitDelayedProperties();
    }

    /// <summary>
    /// Вызывается перед деактивацией, закрытием формы и в других случаях, когда нужно сохранить настройки
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (SaveFormConfig)
        ConfigHandler.Changed[EFPConfigCategories.Form] = true;
      base.OnSaveConfig();
    }

    /// <summary>
    /// Проверка наличия выбранного документа при CanBeEmpty=false.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return;

      switch (Mode)
      {
        case DocTableViewMode.SelectSingle:
          if (!CanBeEmpty)
          {
            if (CurrentDocId == 0)
              SetError("Документ не выбран");
          }
          break;
      }
    }

    /// <summary>
    /// Предотвращает раскрашивание панели при отсутствии выбранного элемента
    /// </summary>
    protected override void InitControlColors()
    {
      // 07.12.2018 Не надо раскрашивать панель, пугая пользователя красным цветом в табличке фильтров
    }

    #endregion

    #region Сохранение конфигурации

    /// <summary>
    /// Надо ли сохранять конфигурацию в секции "Form".
    /// Записываются параметры "ActiveTab", "GroupTreeVisible", "GroupTreePercent", "GroupCBVisible", 
    /// "CurrentGroupId" и "HideNestedGroups".
    /// Если свойство установлено в true (по умолчанию), то элемент будет сам сохранять свои данные.
    /// Если свойство сбосить в false, то предполагается, что параметры должны записываться на уровне формы.
    /// В этом случае можно использовать методы WriteFormConfigPart() и ReadFormConfigPart().
    /// Свойство может устанавливаться только до вывода элемента на экран
    /// </summary>
    public bool SaveFormConfig
    {
      get { return _SaveFormConfig; }
      set
      {
        CheckHasNotBeenCreated();
        _SaveFormConfig = value;
      }
    }
    private bool _SaveFormConfig;

    /// <summary>
    /// Добавляет категорию "Form"
    /// </summary>
    /// <param name="categories">Список категориф для заполнения</param>
    /// <param name="rwMode">Режим: Чтение или запись</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);
      if (SaveFormConfig)
        categories.Add(EFPConfigCategories.Form);
    }

    /// <summary>
    /// Запись категории "Form".
    /// Вызывает WriteFormConfigPart(), если своство SaveFormConfig=true.
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public override void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.WriteConfigPart(category, cfg, actionInfo);
      switch (category)
      {
        case EFPConfigCategories.Form:
          if (SaveFormConfig)
            WriteFormConfigPart(cfg);
          break;
      }
    }

    /// <summary>
    /// Чтение категории "Form".
    /// Вызывает ReadFormConfigPart(), если своство SaveFormConfig=true.
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.ReadConfigPart(category, cfg, actionInfo);
      switch (category)
      {
        case EFPConfigCategories.Form:
          if (SaveFormConfig)
            ReadFormConfigPart(cfg);
          break;
      }
    }

    /// <summary>
    /// Запись категории "Form".
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteFormConfigPart(CfgPart cfg)
    {
      if (DocTreeView != null)
      {
        if (ActiveTab == DocViewFormActiveTab.Grid)
          cfg.SetString("ActiveTab", "Grid");
        else
          cfg.SetString("ActiveTab", "Tree");
      }

      if (GroupAllowed)
      {
        cfg.SetBool("GroupTreeVisible", GroupTreeVisible);
        cfg.SetInt("GroupTreePercent", GroupTreePercent);
        cfg.SetBool("GroupCBVisible", GroupCBVisible);
        if (GroupTreeVisible || GroupCBVisible)
        {
          cfg.SetInt("CurrentGroupId", GroupComboBox.DocId); // 18.11.2017
          cfg.SetBool("HideNestedGroups", !GroupComboBox.IncludeNested); // 17.11.2017
        }
        else
        {
          cfg.SetInt("CurrentGroupId", 0); // 18.11.2017
          cfg.SetBool("HideNestedGroups", false);
        }
      }

      // Записать можно, а прочитать - нет
      //if (ExternalFilters != null)
      //{
      //  CfgPart cfgExtFilters = cfg.GetChild("ExternalFilters", true);
      //  ExternalFilters.WriteConfig(cfgExtFilters);
      //}
      //else
      //  cfg.Remove("ExternalFilters");
    }

    /// <summary>
    /// Чтение категории "Form".
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadFormConfigPart(CfgPart cfg)
    {
      if (DocTreeView != null)
      {
        if (cfg.GetString("ActiveTab") == "Tree")
          ActiveTab = DocViewFormActiveTab.Tree;
        else
          ActiveTab = DocViewFormActiveTab.Grid;
      }

      if (GroupAllowed)
      {
        GroupTreeVisible = cfg.GetBool("GroupTreeVisible");
        int prc = cfg.GetInt("GroupTreePercent");
        if (prc > 0)
          GroupTreePercent = prc;
        GroupCBVisible = cfg.GetBool("GroupCBVisible");
        if (GroupTreeVisible || GroupCBVisible)
        {
          GroupComboBox.DocId = cfg.GetInt("CurrentGroupId"); // 18.11.2017
          GroupComboBox.IncludeNested = !cfg.GetBool("HideNestedGroups"); // 17.11.2017
        }
      }
      else
      {
        GroupTreeVisible = false;
        GroupCBVisible = false;
      }

      // Нет способа создать внешние фильтры.
      // Они создаются в прикладном коде и затем передаются в DocTypeUI.ShowOrOpen().

      //CfgPart cfgExtFilters = cfg.GetChild("ExternalFilters", false);
      //if (cfgExtFilters != null)
      //{ 
      //  GridFilters filters=new GridFilters();
      //  // Как-то добавить фиоьтры
      //  filters.ReadConfig(cfgExtFilters);
      //  ExternalFilters = filters;
      //}
    }

    #endregion
  }
}
