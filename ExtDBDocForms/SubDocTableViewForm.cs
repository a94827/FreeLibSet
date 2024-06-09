// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Data.Docs;
using FreeLibSet.Logging;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Форма для просмотра или выбора поддокументов.
  /// Содержит табличный просмотр, иерархический просмотр (если поддерживается видом поддокументов)
  /// и кнопки (в режиме выбора).
  /// Для выбора поддокументов в прикладном коде следует использовать диалог <see cref="SubDocSelectDialog"/> или комбоблоки <see cref="EFPSubDocComboBox"/>, <see cref="EFPMultiSubDocComboBox"/>, <see cref="EFPInsideSubDocComboBox"/>.
  /// Если требуется разместить просмотр в собственной форме, следует использовать <see cref="EFPSubDocTableView"/>.
  /// </summary>
  public partial class SubDocTableViewForm : Form
  {
    #region Конструктор и Disposing

    /// <summary>
    /// Конструктор используется в EFPSubDocTableView, когда требуется перетащить контроли на родительскую панель
    /// </summary>
    internal SubDocTableViewForm()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Создает форму
    /// </summary>
    /// <param name="subDocTypeUI">Интерфейс пользователя для доступа к поддокументам. Не может быть null.</param>
    /// <param name="mode">Режим просмотра или выбора</param>
    /// <param name="subDocs">Список поддокументов. Не может быть null.</param>
    public SubDocTableViewForm(SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
    {
      InitializeComponent();

      base.Text = subDocTypeUI.SubDocType.PluralTitle;
      base.Icon = EFPApp.MainImages.Icons[subDocTypeUI.TableImageKey];


      _FormProvider = new EFPFormProvider(this);

      _ViewProvider = new EFPSubDocTableView(this, subDocTypeUI, mode, subDocs);


      TheButtonPanel.Visible = (mode != DocTableViewMode.Browse);

      _FormProvider.ConfigSectionName = SubDocTypeName;

      _ViewProvider.SaveFormConfig = false; // сами делаем, т.к. требуется запись от корневого тега формы
      _FormProvider.ConfigHandler.Sources.Add(new FormConfigurable(this));
    }

    /// <summary>
    /// Обработчик закрытия формы
    /// </summary>
    /// <param name="args">Аргументы события</param>
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
    public EFPSubDocTableView ViewProvider { get { return _ViewProvider; } }
    private readonly EFPSubDocTableView _ViewProvider;

    /// <summary>
    /// Обработчик формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private readonly EFPFormProvider _FormProvider;

    /// <summary>
    /// Иерархический просмотр. Если для вида поддокумента не определено поле "ParentId", 
    /// содержит значение null.
    /// </summary>
    public EFPSubDocTreeView SubDocTreeView { get { return ViewProvider.SubDocTreeView; } }

    /// <summary>
    /// Основной табличный просмотр документов
    /// </summary>
    public EFPSubDocGridView SubDocGridView { get { return ViewProvider.SubDocGridView; } }

    /// <summary>
    /// Табличка фильтров (управляется основным просмотром.
    /// </summary>
    public EFPGridFilterGridView FilterView { get { return ViewProvider.FilterView; } }

    /// <summary>
    /// Интерфейс пользователя для доступа к поддокументам
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return ViewProvider.SubDocTypeUI; } }

    /// <summary>
    /// Интерфейс пользователя для доступа к поддокументам
    /// </summary>
    public string SubDocTypeName { get { return ViewProvider.SubDocTypeName; } }

    /// <summary>
    /// Режим работы формы.
    /// Не все режимы реализованы.
    /// </summary>
    public DocTableViewMode Mode { get { return ViewProvider.Mode; } }

    /// <summary>
    /// Действительно в режиме выбора.
    /// Если установлено в true, то доступна кнопка "Нет".
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
    /// будут использованы эти фильтры. Пользователь не может их редактировать.
    /// </summary>
    public GridFilters ExternalFilters
    {
      get { return ViewProvider.ExternalFilters; }
      set { ViewProvider.ExternalFilters = value; }
    }

    /// <summary>
    /// Выбор активной вкладки формы, когда вид поддокумента поддерживает иерархический просмотр в дереве.
    /// </summary>
    public DocViewFormActiveTab ActiveTab
    {
      get { return ViewProvider.ActiveTab; }
      set { ViewProvider.ActiveTab = value; }
    }


    /// <summary>
    /// Идентификатор текущего поддокумента
    /// </summary>
    public Int32 CurrentSubDocId
    {
      get { return ViewProvider.CurrentSubDocId; }
      set { ViewProvider.CurrentSubDocId = value; }
    }


    /// <summary>
    /// Идентификаторы выбранных поддокументов
    /// </summary>
    public Int32[] SelectedSubDocIds
    {
      get { return ViewProvider.SelectedSubDocIds; }
      set { ViewProvider.SelectedSubDocIds = value; }
    }
    #endregion

    #region Обработчики кнопок

#if XXX
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
#endif

    #endregion

    #region Обработчики формы

    /// <summary>
    /// Обработчик события активации формы
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnActivated(EventArgs args)
    {
      base.OnActivated(args);

      try
      {
        // Тут у меня несчастье. Если засунуть вызовы ActiveControl и Select() в
        // другое место, то появляются глюки. Например, если сделать в VisibleChanged
        // (при VisibleEx=true), то первый раз нормально, а после повторной активации
        // формы фокус сбрасывается на дерево иерархии.
        // Если сделать в конструкторе (как было бы логично), то еще хуже: при активации
        // формы после закрытия другой, она "пружинит" и делает активной другую форму
        // Юзер просто одуреет от такого интерфейса.
        //
        // Так, конечно, коряво, но работает. Все равно из дерева и кнопок никаких 
        // других MDI-форм не появляется, поэтому лишней активации не происходит и
        // фокус не прыгает

        ViewProvider.ActiveTab = ActiveTab;
      }
      catch (Exception e)
      {
        // Не стоит вызывать EFPApp.ShowException(), чтобы не провоцировать 
        // повторную активацию
        LogoutTools.LogoutException(e, "SubDocTableViewForm.OnActivated()");
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

      public FormConfigurable(SubDocTableViewForm form)
      {
        _Form = form;
      }

      private SubDocTableViewForm _Form;

      #endregion

      #region IEFPConfigurable Members

      public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
      {
        categories.Add(EFPConfigCategories.Form);
      }

      public void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
      {
        if (category == EFPConfigCategories.Form)
        {
          _Form.ViewProvider.WriteFormConfigPart(cfg);
        }
      }

      public void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
      {
        if (category == EFPConfigCategories.Form)
        {
          _Form.ViewProvider.ReadFormConfigPart(cfg);
        }
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Провайдер составного управляющего элемента для табличного просмотра или выбора поддокументов.
  /// Кроме основной таблицы, может содержать табличку фильтров и просмотр в виде дерева.
  /// Реализует основную функциональность формы <see cref="SubDocTableViewForm"/>, но может использоваться и в формах, созданных в прикладном коде.
  /// </summary>
  public class EFPSubDocTableView : EFPControl<Control>
  {
    #region Конструкторы

    internal EFPSubDocTableView(SubDocTableViewForm form, SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
      : base(form.FormProvider, form.ControlPanel, false)
    {
      Init(form, form.FormProvider, subDocTypeUI, mode, subDocs);
    }

    /// <summary>
    /// Создает провайдер для составного элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="parentControl">Пустая панель для добавления составного элемента</param>
    /// <param name="subDocTypeUI">Интерфейс доступа к поддокументам</param>
    /// <param name="mode">Режим просмотра или выбора поддокументов</param>
    /// <param name="subDocs">Список поддокументов</param>
    public EFPSubDocTableView(EFPBaseProvider baseProvider, Control parentControl, SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
      : base(baseProvider, parentControl, false)
    {
      if (parentControl.HasChildren)
        throw new ArgumentException("В панели не должно быть управляющих элементов", "parentControl");

      SubDocTableViewForm dummyForm = new SubDocTableViewForm();

      WinFormsTools.MoveControls(dummyForm.ControlPanel, parentControl);

      Init(dummyForm, baseProvider, subDocTypeUI, mode, subDocs);

      base.InitConfigHandler();
      base.ConfigSectionName = SubDocTypeName; // после InitConfigHandler
    }


    private void Init(SubDocTableViewForm form, EFPBaseProvider baseProvider, SubDocTypeUI subDocTypeUI, DocTableViewMode mode, DBxMultiSubDocs subDocs)
    {
      if (subDocTypeUI == null)
        throw new ArgumentNullException("subDocTypeUI");
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");
      
      // Убрано 28.04.2022
      // В процессе показа редактора документа может быть заменен DocTypeUI.DocProvider.
      // После этого subDocTypeUI.SubDocType будет указывать на новый объект, полученный от сервера,
      // а subDocs.SubDocType - на старый.

      //if (!Object.ReferenceEquals(subDocTypeUI.SubDocType, subDocs.SubDocType))
      //  throw new ArgumentException("SubDocTypeUI и SubDocs относятся к разным объектам SubDocType", "subDocs");

      _Form = form;


      if (!String.IsNullOrEmpty(subDocTypeUI.SubDocType.TreeParentColumnName))
      {
        _TheTabControl = new TabControl();
        _TheTabControl.Dock = DockStyle.Fill;
        _TheTabControl.ImageList = EFPApp.MainImages.ImageList;
        form.MainPanel.Controls.Add(_TheTabControl);

        TabPage tpTree = new TabPage("Дерево");
        _TheTabControl.Controls.Add(tpTree);
        tpTree.ImageKey = "TreeView";

        _SubDocTree = new TreeViewAdv();
        _SubDocTree.Dock = DockStyle.Fill;
        _SubDocTree.Name = "SubDocTree";
        tpTree.Controls.Add(_SubDocTree);

        _SubDocTreeSpeedPanel = new Panel();
        _SubDocTreeSpeedPanel.Dock = DockStyle.Top;
        tpTree.Controls.Add(_SubDocTreeSpeedPanel);

        TabPage tpTable = new TabPage("Таблица");
        _TheTabControl.Controls.Add(tpTable);
        tpTable.ImageKey = "Table";

        _SubDocGrid = new DataGridView();
        _SubDocGrid.Dock = DockStyle.Fill;
        _SubDocGrid.Name = "SubDocGrid";
        tpTable.Controls.Add(_SubDocGrid);

        _SubDocGridSpeedPanel = new Panel();
        _SubDocGridSpeedPanel.Dock = DockStyle.Top;
        tpTable.Controls.Add(_SubDocGridSpeedPanel);
      }
      else
      {
        _SubDocGrid = new DataGridView();
        _SubDocGrid.Dock = DockStyle.Fill;
        _SubDocGrid.Name = "SubDocGrid";
        form.MainPanel.Controls.Add(_SubDocGrid);

        _SubDocGridSpeedPanel = new Panel();
        _SubDocGridSpeedPanel.Dock = DockStyle.Top;
        form.MainPanel.Controls.Add(_SubDocGridSpeedPanel);
      }


      if (!String.IsNullOrEmpty(subDocTypeUI.SubDocType.TreeParentColumnName))
      {
        _SubDocTreeView = new EFPSubDocTreeView(baseProvider, _SubDocTree, subDocs, subDocTypeUI.UI);
        _SubDocTreeView.CommandItems.EnterAsOk = (mode != DocTableViewMode.Browse);
        _SubDocTreeView.ToolBarPanel = _SubDocTreeSpeedPanel;
      }

      _SubDocGridView = new EFPSubDocGridView(baseProvider, _SubDocGrid, subDocs, subDocTypeUI.UI);
      _SubDocGridView.CommandItems.EnterAsOk = (mode != DocTableViewMode.Browse);
      _SubDocGridView.ToolBarPanel = _SubDocGridSpeedPanel;

      if (mode == DocTableViewMode.SelectSingle)
      {
        _SubDocGridView.Control.MultiSelect = false;
        _SubDocGridView.CanMultiEdit = false;
        if (_SubDocTreeView != null)
        {
          _SubDocTreeView.Control.SelectionMode = TreeViewAdvSelectionMode.Single;
          _SubDocTreeView.CanMultiEdit = false;
        }
      }

      if (_SubDocTreeView != null)
        // Синхронизатор
        new EFPDBxViewSync(_SubDocGridView, _SubDocTreeView);


      _FilterView = new EFPGridFilterGridView(_SubDocGridView, form.FilterGrid);
      _Mode = mode;

      _SaveFormConfig = true;
    }

    #endregion

    #region Свойства

    private SubDocTableViewForm _Form;

    /// <summary>
    /// Существует, если есть древовидный просмотр
    /// </summary>
    private TabControl _TheTabControl;

    private FreeLibSet.Controls.TreeViewAdv _SubDocTree;
    private Panel _SubDocTreeSpeedPanel;

    private DataGridView _SubDocGrid;
    private Panel _SubDocGridSpeedPanel;

    /// <summary>
    /// Иерархический просмотр. Если для вида поддокумента не определено поле "ParentId", 
    /// содержит значение null.
    /// </summary>
    public EFPSubDocTreeView SubDocTreeView { get { return _SubDocTreeView; } }
    private EFPSubDocTreeView _SubDocTreeView;

    /// <summary>
    /// Основной табличный просмотр поддокументов
    /// </summary>
    public EFPSubDocGridView SubDocGridView { get { return _SubDocGridView; } }
    private EFPSubDocGridView _SubDocGridView;

    /// <summary>
    /// Табличка фильтров.
    /// </summary>
    public EFPGridFilterGridView FilterView { get { return _FilterView; } }
    private EFPGridFilterGridView _FilterView;

    /// <summary>
    /// Интерфейс доступа к поддокументам
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocGridView.SubDocTypeUI; } }

    /// <summary>
    /// Имя вида просматриваемых поддокументов
    /// </summary>
    public string SubDocTypeName { get { return _SubDocGridView.SubDocType.Name; } }

    /// <summary>
    /// Режим работы формы.
    /// Не все режимы реализованы.
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
    /// будут использованы эти фильтры. Пользователь не может их редактировать.
    /// </summary>
    public GridFilters ExternalFilters
    {
      get { return _ExternalFilters; }
      set
      {
        _ExternalFilters = value;
        if (value != null)
        {
          _SubDocGridView.Filters = value;
          _SubDocGridView.CommandItems.CanEditFilters = false;
        }
      }
    }
    private GridFilters _ExternalFilters;

    ///// <summary>
    ///// Если это событие установлено, то оно будет вызвано после того, как будут
    ///// прочитаны значения фильтров, сохраненные в конфигурации
    ///// </summary>
    //public event InitEFPDBxViewEventHandler InitFilters;


    /// <summary>
    /// Выбор активной вкладки формы, когда вид поддокумента поддерживает иерархический просмотр в дереве
    /// </summary>
    public DocViewFormActiveTab ActiveTab
    {
      get
      {
        if (_SubDocTreeView == null)
          return DocViewFormActiveTab.Grid;
        else
        {
          if (_TheTabControl.SelectedIndex <= 0)
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
            if (_SubDocTreeView == null)
              throw new InvalidOperationException("Форма не содержит вкладки иерархического просмотра");
            else
              _TheTabControl.SelectedIndex = 0;
            break;
          case DocViewFormActiveTab.Grid:
            if (_SubDocTreeView != null)
              _TheTabControl.SelectedIndex = 1;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }


    /// <summary>
    /// Идентификатор текущего поддокумента
    /// </summary>
    public Int32 CurrentSubDocId
    {
      get
      {
        switch (ActiveTab)
        {
          case DocViewFormActiveTab.Grid: return SubDocGridView.CurrentId;
          case DocViewFormActiveTab.Tree: return SubDocTreeView.CurrentId;
          default:
            throw new BugException();
        }
      }
      set
      {
        // 16.11.2017
        SubDocGridView.CurrentId = value;
        if (SubDocTreeView != null)
          SubDocTreeView.CurrentId = value;
      }
    }


    /// <summary>
    /// Идентификаторы выбранных поддокументов
    /// </summary>
    public Int32[] SelectedSubDocIds
    {
      get
      {
        switch (ActiveTab)
        {
          case DocViewFormActiveTab.Grid: return SubDocGridView.SelectedIds;
          case DocViewFormActiveTab.Tree: return SubDocTreeView.SelectedIds;
          default:
            throw new BugException();
        }
      }
      set
      {
        switch (ActiveTab)
        {
          case DocViewFormActiveTab.Grid: SubDocGridView.SelectedIds = value; break;
          case DocViewFormActiveTab.Tree: SubDocTreeView.SelectedIds = value; break;
        }
      }
    }
    #endregion

    #region Обработчики формы

    /// <summary>
    /// Метод вызывается при первом появлении элемента на экране
    /// </summary>
    protected override void OnCreated()
    {
      if (!String.IsNullOrEmpty(Control.Name))
      {
        if (_SubDocTree != null)
          _SubDocTree.Name = Control.Name + _SubDocTree.Name;
        _SubDocGrid.Name = Control.Name + _SubDocGrid.Name;
      }

      base.OnCreated();
    }

    //protected override void OnActivated(EventArgs Args)
    //{
    //  base.OnActivated(Args);

    //  try
    //  {
    //    // Тут у меня несчастье. Если засунуть вызовы ActiveControl и Select() в
    //    // другое место, то появляются глюки. Например, если сделать в VisibleChanged
    //    // (при VisibleEx=true), то первый раз нормально, а после повторной активации
    //    // формы фокус сбрасывается на дерево иерархии.
    //    // Если сделать в конструкторе (как было бы логично), то еще хуже: при активации
    //    // формы после закрытия другой, она "пружинит" и делает активной другую форму
    //    // Юзер просто одуреет от такого интерфейса.
    //    //
    //    // Так, конечно, коряво, но работает. Все равно из дерева и кнопок никаких 
    //    // других MDI-форм не появляется, поэтому лишней активации не происходит и
    //    // фокус не прыгает

    //    switch (ActiveTab)
    //    {
    //      case DocViewFormActiveTab.Grid: ActiveControl = SubDocGrid; break;
    //      case DocViewFormActiveTab.Tree: ActiveControl = SubDocTree; break;
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    // Не стоит вызывать EFPApp.ShowException(), чтобы не провоцировать 
    //    // повторную активацию
    //    LogoutTools.LogoutException(e, "DocTableViewForm.OnActivated()");
    //  }
    //}

    /// <summary>
    /// Вызывается, когда форма с управляющим элементом закрывается.
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (SaveFormConfig)
        ConfigHandler.Changed[EFPConfigCategories.Form] = true;

      base.OnSaveConfig();
    }

    /// <summary>
    /// Проверяет наличие выбранного поддокумента, если свойство <see cref="CanBeEmpty"/>=false.
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
            if (CurrentSubDocId == 0)
              base.SetError("Поддокумент не выбран");
          }
          break;
      }
    }

    /// <summary>
    /// Предотвращает раскраску панели при отсутствии выбранного поддокумента
    /// </summary>
    protected override void InitControlColors()
    {
      // 07.12.2018 Не надо раскрашивать панель, пугая пользователя красным цветом в табличке фильтров
    }

    #endregion

    #region Сохранение конфигурации

    /// <summary>
    /// Надо ли сохранять конфигурацию в секции "Form".
    /// Записывается параметр "ActiveTab".
    /// Если свойство установлено в true (по умолчанию), то элемент будет сам сохранять свои данные.
    /// Если свойство сбосить в false, то предполагается, что параметры должны записываться на уровне формы.
    /// В этом случае можно использовать методы <see cref="WriteFormConfigPart(CfgPart)"/> и <see cref="ReadFormConfigPart(CfgPart)"/>.
    /// Свойство может устанавливаться только до вывода элемента на экран.
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
    /// Добавляет в список категорию "Form"
    /// </summary>
    /// <param name="categories">Список для добавления категорий</param>
    /// <param name="rwMode">Чтение или запись</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);
      if (SaveFormConfig)
        categories.Add(EFPConfigCategories.Form);
    }

    /// <summary>
    /// Выполняет запись секции конфигурации "Form", если свойство <see cref="SaveFormConfig"/>=true.
    /// Для этого вызывается метод <see cref="WriteFormConfigPart(CfgPart)"/>.
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
    /// Выполняет чтение секции конфигурации "Form", если свойство <see cref="SaveFormConfig"/>=true.
    /// Для этого вызывается метод <see cref="ReadFormConfigPart(CfgPart)"/>.
    /// </summary>
    /// <param name="category">Категория записываемой секции</param>
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
    /// Выполняет запись секции конфигурации "Form".
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteFormConfigPart(CfgPart cfg)
    {
      if (SubDocTreeView != null)
      {
        if (ActiveTab == DocViewFormActiveTab.Grid)
          cfg.SetString("ActiveTab", "Grid");
        else
          cfg.SetString("ActiveTab", "Tree");
      }
    }

    /// <summary>
    /// Выполняет чтение секции конфигурации "Form".
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadFormConfigPart(CfgPart cfg)
    {
      if (SubDocTreeView != null)
      {
        if (cfg.GetString("ActiveTab") == "Tree")
          ActiveTab = DocViewFormActiveTab.Tree;
        else
          ActiveTab = DocViewFormActiveTab.Grid;
      }
    }

    #endregion
  }
}
