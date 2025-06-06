﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Forms;
using FreeLibSet.Config;
using FreeLibSet.Models.Tree;
using FreeLibSet.Data.Docs;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Controls;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Forms.Data;

#pragma warning disable 0169 // TODO: Убрать лишние переменные


namespace FreeLibSet.Forms.Docs
{
  #region EFPDBxTreeViewDocSelEventArgs

  /// <summary>
  /// Аргументы события <see cref="EFPDBxTreeView.GetDocSel"/>
  /// </summary>
  public class EFPDBxTreeViewDocSelEventArgs : EFPDBxViewDocSelEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Конструктор вызывается из <see cref="EFPDBxTreeView"/> 
    /// </summary>
    /// <param name="controlProvider">Доступ провайдеру управляющего элемента</param>
    /// <param name="reason">Причина, по которой требуется создать выборку</param>
    /// <param name="nodes">Узлы, для которых требуется создать выборку документов.
    /// Если null, то берутся выбранные узлы TreeViewAdv.SelectedNodes</param>
    public EFPDBxTreeViewDocSelEventArgs(EFPDBxTreeView controlProvider, EFPDBxViewDocSelReason reason, FreeLibSet.Controls.TreeNodeAdv[] nodes)
      :base(controlProvider, reason)
    {
      _Nodes = nodes;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ провайдеру управляющего элемента
    /// </summary>
    public new EFPDBxTreeView ControlProvider { get { return (EFPDBxTreeView)(base.ControlProvider); } }

    /// <summary>
    /// Узлы, для которых требуется создать выборку документов
    /// </summary>
    public TreeNodeAdv[] Nodes
    {
      get
      {
        if (_Nodes == null)
        {
          _Nodes = new TreeNodeAdv[ControlProvider.Control.SelectedNodes.Count];
          ControlProvider.Control.SelectedNodes.CopyTo(_Nodes, 0);
        }
        return _Nodes;
      }
    }
    private TreeNodeAdv[] _Nodes;

    /// <summary>
    /// Создание списка строк таблицы <see cref="DataTable"/>, соответствующий <see cref="Nodes"/> 
    /// </summary>
    /// <returns>Массив строк</returns>
    protected override DataRow[] CreateDataRows()
    {
      DataRow[] rows = new DataRow[Nodes.Length];
      for (int i = 0; i < rows.Length; i++)
        rows[i] = Nodes[i].Tag as DataRow;
      return rows;
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPDBxTreeView.GetDocSel"/>
  /// </summary>
  /// <param name="sender">Провайдер иерархического просмотра</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDBxTreeViewDocSelEventHandler(object sender,
    EFPDBxTreeViewDocSelEventArgs args);

  #endregion

  /// <summary>
  /// Провайдер иерархического просмотра табличных данных, в которых есть поле "Id" типа Int32.
  /// Поддерживает выборки документов.
  /// </summary>
  public class EFPDBxTreeView : EFPConfigurableDataTreeView, IEFPDBxView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDBxTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, DBUI ui)
      : base(baseProvider, control)
    {
      Init(ui);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDBxTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, DBUI ui)
      : base(controlWithToolBar)
    {
      Init(ui);
    }

    private void Init(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      Control.SelectionMode = TreeViewAdvSelectionMode.Multi;
      Control.FullRowSelect = false;
      Control.SelectionChanged += new EventHandler(Control_SelectionChanged);

      // Определяется автоматически из модели
      // SelectedNodesMode = EFPDataTreeViewSelectedNodesMode.PrimaryKey;

      _SaveCurrentId = true;

      base.InitConfigHandler();
    }

    #endregion

    #region SelectedIds

    /// <summary>
    /// Возвращает <see cref="ITreeModelWithIds{Int32}"/> или выбрасывает исключение
    /// </summary>
    /// <returns>Модель</returns>
    protected ITreeModelWithIds<Int32> GetModelWithIdsWithCheck()
    {
      if (Control.Model == null)
        throw ExceptionFactory.ObjectPropertyNotSet(Control, "Model");

      ITreeModelWithIds<Int32> model = Control.Model as ITreeModelWithIds<Int32>;
      if (model == null)
        throw ExceptionFactory.ObjectProperty(Control, "Model", Control.Model, new object[] { typeof(ITreeModelWithIds<Int32>) });

      return model;
    }

    /// <summary>
    /// Идентификаторы выбранных узлов.
    /// Вложенные узлы не возвращаются.
    /// </summary>
    public virtual Int32[] SelectedIds
    {
      get
      {
        if (_DelayedSelectedIds != null)
          return _DelayedSelectedIds;

        // Может быть и другая модель. Плохо, если чтение свойства будет приводить к исключению
        // ITreeModelWithIds Model = GetModelWithIdsWithCheck();
        ITreeModelWithIds<Int32> model = Control.Model as ITreeModelWithIds<Int32>;
        if (model == null)
          return null;

        DataRow[] rows = SelectedDataRows;
        return DataTools.GetIdsFromColumn(rows, "Id");
      }
      set
      {
        _DelayedSelectedIds = null;

        if (value == null)
          return;
        if (value.Length == 0)
          return;

        if (Control.Model == null)
          _DelayedSelectedIds = value;
        else
        {
          ITreeModelWithIds<Int32> model = GetModelWithIdsWithCheck();
          Control.BeginUpdate();
          try
          {
            Control.ClearSelection();
            for (int i = 0; i < value.Length; i++)
            {
              TreePath path = model.TreePathFromId(value[i]);
              TreeNodeAdv node = Control.FindNode(path, true);
              if (node != null)
                node.IsSelected = true;
            }

            EnsureSelectionVisible();
          }
          finally
          {
            Control.EndUpdate();
          }
        }
      }
    }
    private Int32[] _DelayedSelectedIds;

    /// <summary>
    /// Идентификатор документа, поддокумента или записи в какой-либо другой таблице для текущего выбранного узла
    /// </summary>
    public virtual Int32 CurrentId
    {
      get
      {
        if (_DelayedCurrentId != 0)
          return _DelayedCurrentId;

        // Может быть и другая модель. Плохо, если чтение свойства будет приводить к исключению
        // ITreeModelWithIds Model = GetModelWithIdsWithCheck();
        ITreeModelWithIds<Int32> model = Control.Model as ITreeModelWithIds<Int32>;
        if (model == null)
          return 0;


        if (Control.CurrentNode == null)
          return 0;

        TreePath path = Control.GetPath(Control.CurrentNode);
        return model.TreePathToId(path);
      }
      set
      {
        _DelayedCurrentId = 0;
        if (value == 0)
          return;

        if (Control.Model == null)
          _DelayedCurrentId = value;
        else
        {
          ITreeModelWithIds<Int32> model = GetModelWithIdsWithCheck();

          TreePath path = model.TreePathFromId(value);
          TreeNodeAdv node = Control.FindNode(path, true);
          Control.SelectedNode = node;

          EnsureSelectionVisible();
        }
      }
    }
    private Int32 _DelayedCurrentId;

    /// <summary>
    /// Возвращает идентификаторы выбранных узлов вместе со всеми их дочерними узлами
    /// </summary>
    public Int32[] SelectedIdsWithChildren
    {
      get
      {
        Int32[] ids = SelectedIds;
        ITreeModelWithIds<Int32> model = Control.Model as ITreeModelWithIds<Int32>;
        if (model == null || ids.Length == 0)
          return ids;

        IdList idList = new IdList();
        for (int i = 0; i < ids.Length; i++)
          idList.Add(model.GetIdWithChildren(ids[i]));
        return idList.ToArray();
      }
    }

    #endregion

    #region Чтение / запись конфигурации

    private bool _SelectionChangedFlag;

    void Control_SelectionChanged(object sender, EventArgs args)
    {
      _SelectionChangedFlag = true;
    }

    /// <summary>
    /// Помечает категорию "TreeView" для сохранения текущей позиции
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (_SelectionChangedFlag)
      {
        _SelectionChangedFlag = false;
        if (SaveCurrentId)
          ConfigHandler.Changed[EFPConfigCategories.TreeView] = true; // для сохранения текущей позиции
      }

      base.OnSaveConfig();
    }

    /// <summary>
    /// Добавляет в список категорию "TreeView", если свойство <see cref="SaveCurrentId"/> установлено в true.
    /// </summary>
    /// <param name="categories">Список категорий для заполнения</param>
    /// <param name="rwMode">Режим чтения или записи</param>
    /// <param name="actionInfo">Описание действия</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo); // добавлено 21.02.2019

      if (SaveCurrentId)
        categories.Add(EFPConfigCategories.TreeView);
    }

    /// <summary>
    /// Запись секции конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Заполняемая секция</param>
    /// <param name="actionInfo">Информация о действии</param>
    public override void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      switch (category)
      {
        case EFPConfigCategories.TreeView:
          cfg.SetInt("CurrentId", CurrentId);
          break;
        default:
          base.WriteConfigPart(category, cfg, actionInfo);
          break;
      }
    }

    /// <summary>
    /// Чтение секции конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Считываемая секция</param>
    /// <param name="actionInfo">Информация о действии</param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      switch (category)
      {
        case EFPConfigCategories.TreeView:
          if (/*CurrentId*/_DelayedCurrentId /*23.11.2017*/ == 0)
            CurrentId = cfg.GetInt("CurrentId");
          break;
        default:
          base.ReadConfigPart(category, cfg, actionInfo);
          break;
      }
    }

    #endregion

    #region Восстановление текущей позиции

    /// <summary>
    /// Управляющее свойство.
    /// Если установлено в true (по умолчанию), то при выводе элемента на экран устанавливается значение
    /// <see cref="CurrentId"/>, сохраненное в конфигурационных данных. Установка не выполняется, если свойство <see cref="CurrentId"/>
    /// предварительно было установлено в явном виде. При закрытии просмотра выполняется сохранение значения.
    /// Игнорируется, если свойство <see cref="EFPControlBase.ConfigSectionName"/> не установлено.
    /// Свойство <see cref="SaveCurrentId"/> можно устанавливать только до вывода элемента на экран.
    /// Данные хранятся в секции конфигурации с категорией "GridView" в числовом поле "CurrentId".
    /// </summary>
    public bool SaveCurrentId
    {
      get { return _SaveCurrentId; }
      set
      {
        CheckHasNotBeenCreated();
        _SaveCurrentId = value;
      }
    }
    private bool _SaveCurrentId;

    /// <summary>
    /// Отложенная инициализация текущей позиции
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      if (Control.Model == null)
        OnRefreshData(EventArgs.Empty); // 09.03.2022

      if (Control.Model == null)
        throw new BugException("Control.Model==null");

      // Восстановление сохраненной текущей позиции
      if (_DelayedSelectedIds != null)
        SelectedIds = _DelayedSelectedIds;
      if (_DelayedCurrentId != 0)
      {
        try { CurrentId = _DelayedCurrentId; }
        catch { }
      }
    }

    #endregion

    #region UI

    /// <summary>
    /// Интерфейс доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region Переопределенные методы для работы фильтра

    /// <summary>
    /// Коллекция фильтров для табличного просмотра. Если есть хотя бы один фильтр,
    /// то в локальном меню появляется команда "Фильтр". После установки пользователем
    /// фильтра вызывается обновление просмотра. Ответственность за обработку
    /// фильтров лежит на вызывающей программе.
    /// Чтобы проверить наличие возможных фильтров, следует использовать свойство
    /// <see cref="EFPConfigurableDataTreeView.HasFilters"/>, которое позволяет избежать создания лишних объектов.
    /// </summary>
    public new EFPDBxGridFilters Filters
    {
      get { return (EFPDBxGridFilters)(base.Filters); }
      set
      {
        if (value != null)
          value.SqlFilterRequired = true;
        base.Filters = value;
      }
    }

    /// <summary>
    /// Создает объект <see cref="EFPDBxGridFilters"/>
    /// </summary>
    /// <returns>Список фильтров</returns>
    protected override IEFPGridFilters CreateGridFilters()
    {
      EFPDBxGridFilters filters = new EFPDBxGridFilters();
      filters.SqlFilterRequired = true;
      return filters;
    }

    /// <summary>
    /// Вызывается для дополнительной инициализации табличного просмотра в редакторе фильтров.
    /// Переопределеннный метод может, например, инициализировать дополнительные команды меню.
    /// </summary>
    /// <param name="filterGridProvider">Обработчик таблицы фильтров</param>
    public override void InitGridFilterEditorGridView(EFPGridFilterEditorGridView filterGridProvider)
    {
      base.InitGridFilterEditorGridView(filterGridProvider);

      filterGridProvider.CommandItems.AddCopyFormats += new DataObjectEventHandler(FilterGridProvider_AddCopyFormats);

      DBxDocSelectionPasteFormat fmtDocSel = new DBxDocSelectionPasteFormat(UI);
      fmtDocSel.Tag = filterGridProvider;
      fmtDocSel.Paste += new EFPPasteDataObjectEventHandler(fmtDocSel_Paste);
      filterGridProvider.CommandItems.PasteHandler.Add(fmtDocSel);
    }


    /// <summary>
    /// Копирует в буфер обмена выборку документов, соответствующую выбранным фильтрам
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void FilterGridProvider_AddCopyFormats(object sender, DataObjectEventArgs args)
    {
      EFPDataGridViewCommandItems commandItems = (EFPDataGridViewCommandItems)sender;
      int[] filterIndices = commandItems.ControlProvider.SelectedRowIndices;

      DBxDocSelection docSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      for (int i = 0; i < filterIndices.Length; i++)
      {
        IDBxDocSelectionFilter item2 = Filters[filterIndices[i]] as IDBxDocSelectionFilter;
        if (item2 != null)
          item2.GetDocSel(docSel);
      }
      if (!docSel.IsEmpty)
      {
        args.DataObject.SetData(docSel);
        UI.OnAddCopyFormats(args.DataObject, docSel); // 06.02.2021
      }
    }

    void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      EFPDataGridView efpFilterGrid = (EFPDataGridView)(fmtDocSel.Tag);

      if (!efpFilterGrid.CheckSingleRow())
        return;

      DBxCommonFilter item = Filters[efpFilterGrid.CurrentRowIndex];
      IDBxDocSelectionFilter item2 = item as IDBxDocSelectionFilter;
      if (item2 == null)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPDBxFilterGrid_Err_NotSupportedFilter, item.DisplayName));
        return;
      }

      if (item2.ApplyDocSel(fmtDocSel.DocSel))
        efpFilterGrid.PerformRefresh();
      else
        EFPApp.ShowTempMessage(String.Format(Res.EFPDBxFilterGrid_Err_DocSelMismatch, item.DisplayName));
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// Генератор столбцов таблицы. Если задан, то в локальном меню доступны
    /// команды настройки столбцов таблицы.
    /// </summary>
    public new EFPGridProducer GridProducer
    {
      get { return (EFPGridProducer)(base.GridProducer); }
      set { base.GridProducer = value; }
    }

    #endregion

    #region Выбор и настройка конфигурации просмотра

#if XXX
    /// <summary>
    /// Имя текущей конфигурации
    /// </summary>
    public string CurrentConfigName
    {
      get
      {
        if (_CurrentConfigName == null)
        {
          if (String.IsNullOrEmpty(ConfigSectionName))
            return String.Empty;
          // TODO: // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
          //CfgPart Cfg;
          //using (ConfigManager.GetConfig(this, EFPConfigCategories.TreeView, String.Empty, EFPConfigMode.Read, out Cfg))
          //{
          //  FCurrentConfigName = Cfg.GetString("GridConfig");
          //}
        }
        return _CurrentConfigName;
      }
      set
      {
        if (value == null)
          value = String.Empty;
        _CurrentConfigName = value;
        CommandItems.InitCurrentCfgName();
        // Запоминаем новое имя конфигурации, используемое просмотром
        // TODO: // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //if (!String.IsNullOrEmpty(ConfigSectionName))
        //{
        //  CfgPart Cfg;
        //  using (ConfigManager.GetConfig(this, EFPConfigCategories.TreeView, String.Empty, EFPConfigMode.Write, out Cfg))
        //  {
        //    Cfg.SetString("GridConfig", value);
        //  }
        //}

      }
    }
    private string _CurrentConfigName;
#endif

#if XXX

    /// <summary>
    /// Если true, то текущая конфигурация просмотра была изменена, но не была сохранена
    /// </summary>
    public bool UserConfigModified
    {
      get { return _UserConfigModified; }
      set
      {
        if (value == _UserConfigModified)
          return;
        _UserConfigModified = value;
        _SaveGridConfigAnsweredNo = false;
        //CommandItems.InitCurrentCfgModified();
      }
    }
    private bool _UserConfigModified;

    void Grid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs args)
    {
      try
      {
        if (args.Column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
          UserConfigModified = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Grid_ColumnWidthChanged");
      }
    }

    void Grid_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs args)
    {
      try
      {
        UserConfigModified = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Grid_ColumnDisplayIndexChanged");
      }
    }
#endif

#if XXX // Убрано 28.06.2021. Если это и нужно, то в EFPConfigurableDataTreeView

    /// <summary>
    /// Сохранить текущую конфигурацию просмотра
    /// </summary>
    /// <returns>true, если сохранение выполнено</returns>
    public bool PerformSaveGridConfig()
    {
      return PerformSaveGridConfig(false);
    }

    /// <summary>
    /// Сохранить текущую конфигурацию просмотра
    /// </summary>
    /// <param name="saveAs">Если true, то будет запрошено имя новой конфигурации независимо
    /// от текущего значения свойства CurrentCfgName</param>
    /// <returns>true, если сохранение выполнено</returns>
    public bool PerformSaveGridConfig(bool saveAs)
    {
      throw new NotImplementedException();
#if XXX
      if (GridProducer == null)
        return false;
      if (String.IsNullOrEmpty(ConfigSectionName))
        return false;
      if (CurrentConfig == null)
        return false;
      // Текущая конфигурация с учетом реальных размеров столбцов
      EFPDataGridViewConfig Config2 = CurrentConfig.Clone(this);

      if (String.IsNullOrEmpty(CurrentConfigName) || SaveAs)
      {
        string NewName = String.Empty;
        if (!EFPApp.InputBox(ref NewName, "Сохранение конфигурации просмотра", "Имя новой конфигурации", 30, false))
          return false;

        WriteGridConfig(Config2, NewName);
        UserConfigModified = false;
        CurrentConfigName = NewName;
        return true;
      }
      else
      {
        WriteGridConfig(Config2, CurrentConfigName);
        UserConfigModified = false;
        return true;
      }
#endif
    }

#if XXX
    /// <summary>
    /// Внутренний флаг для предотвращения повторных запросов пользователя, 
    /// когда метод SaveGridConfigIfRequired() вызывается неоднократно и пользователь
    /// уже ответил "нет"
    /// </summary>
    private bool _SaveGridConfigAnsweredNo;

    /// <summary>
    /// Выполнить сохранение конфигурации табличного просмотра, если это необходимо.
    /// Возможно, пользователю будут выдаваться запросы для ввода имени конфигурации
    /// Метод может вызываться в собственных обработчиках FForm_Closing.
    /// Возвращает true, если форма МОЖЕТ закрываться, то есть !Cancel
    /// </summary>
    /// <returns>true, если сохранение конфигурации не требуется или успешно
    /// выполнено, false-в случает ошибки</returns>
    public bool SaveGridConfigIfRequired()
    {
      if (GridProducer == null)
        return true;
      if (String.IsNullOrEmpty(ConfigSectionName))
        return true;
      if (CurrentConfig == null)
        return true;
      if (UserConfigModified)
      {
        if (_SaveGridConfigAnsweredNo)
          return true; // уже спрашивали

        SetFocus(); // 31.07.2013. Передаем фокус ввода

        switch (EFPApp.MessageBox("Настройка табличного просмотра была изменена. Сохранить изменения? " +
          (String.IsNullOrEmpty(CurrentConfigName) ?
          "(Будет запрошено имя новой конфигурации)" :
          "(Имя текущей конфигурации: \"" + CurrentConfigName + "\")"),
          "Закрытие окна", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
        {
          case DialogResult.Yes:
            try
            {
              if (!PerformSaveGridConfig())
                return false;
            }
            catch (Exception e)
            {
              EFPApp.ShowException(e, "Ошибка при сохранении настроек просмотра");
              return false;
            }
            break;
          case DialogResult.No:
            _SaveGridConfigAnsweredNo = true;
            break;
          default:
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Выполнить сохранение конфигурации табличного просмотра, если это необходимо.
    /// Возможно, пользователю будут выдаваться запросы для ввода имени конфигурации
    /// Метод должен вызываться для шага мастера, содержащего табличный просмотр WizardStep.EndStep
    /// (кроме шага WizardStepWithGrid и производных, который сам выполняет нужные действия)
    /// Если при запросе конфигурации пользователь нажимает отмена, устанавливается
    /// свойство Args.Cancel
    /// Возвращает true, если шаг МОЖЕТ закрываться, то есть !Args.Cancel
    /// </summary>
    /// <param name="args">Аргумент обработчика события EndStep</param>
    /// <returns>true, если можно перейти к следующему щагу мастера</returns>
    public bool SaveGridConfigIfRequired(WizardEndStepEventArgs args)
    {
      if (!SaveGridConfigIfRequired())
      {
        args.Cancel = true;
        return false;
      }

      return true;
    }

    private Form _EHFormClosing2Form;
    private FormClosingEventHandler _EHFormClosing2;

    /// <summary>
    /// Обработка закрытия формы для сохранения параметров.
    /// Выполняет вызов SaveGridConfigIfRequired()
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Form_FormClosing2(object sender, FormClosingEventArgs args)
    {
      if (args.Cancel)
        return;
      try
      {
        // TODO: if (!EFPApp.ClosingByServer)
        args.Cancel = !SaveGridConfigIfRequired();
      }
      catch (Exception e)
      {
        Form Form = (Form)sender;
        EFPApp.ShowException(e, "Ошибка при закрытии окна с табличным просмотром");
        args.Cancel = (EFPApp.MessageBox("Закрыть окно \"" + Form.Text + "\" несмотря на возникшую ошибку?",
          "Ошибка при закрытии окна", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK);
      }
    }
#endif

    /*
    public override void AfterControlAssigned()
    {
      base.AfterControlAssigned();

      // Обработчик закрытия формы
      // Сохраняем обработчик и то, к чему его мы прицепи
      EHFormClosing2Form = Control.FindForm();
      EHFormClosing2 = new FormClosingEventHandler(Form_FormClosing2);
      EHFormClosing2Form.FormClosing += EHFormClosing2;
    } */

    internal EFPDataGridViewConfig ReadGridConfig(string configName)
    {
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
        EFPConfigCategories.GridConfig, configName);
      CfgPart cfg;
      using (ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfg))
      {
        if (cfg.IsEmpty)
          return null;
        else
        {
          EFPDataGridViewConfig Config = new EFPDataGridViewConfig();
          Config.ReadConfig(cfg);
          return Config;
        }
      }
    }

    internal void WriteGridConfig(EFPDataGridViewConfig config, string configName)
    {
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
        EFPConfigCategories.GridConfig, configName);
      CfgPart cfg;
      using (ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
      {
        config.WriteConfig(cfg);
      }
    }
#endif

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPDBxTreeViewCommandItems CommandItems { get { return (EFPDBxTreeViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает объект <see cref="EFPDBxTreeViewCommandItems"/>
    /// </summary>
    /// <returns>Список команд</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPDBxTreeViewCommandItems(this);
    }

    #endregion

    #region Событие GetDocSel

    /// <summary>
    /// Если обработчик установлен, то при копировании ячеек в буфер обмена будет
    /// помещена выборка документов (объект <see cref="DBxDocSelection"/>).
    /// Также будет добавлена команда "Отправить" -> "Выборка"
    /// </summary>
    public event EFPDBxTreeViewDocSelEventHandler GetDocSel;

    /// <summary>
    /// В случае пеоеопределения метода также должно быть переопределено свойство <see cref="HasGetDocSelHandler"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnGetDocSel(EFPDBxTreeViewDocSelEventArgs args)
    {
      if (GetDocSel != null)
        GetDocSel(this, args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик <see cref="GetDocSel"/>
    /// </summary>
    public virtual bool HasGetDocSelHandler { get { return GetDocSel != null; } }

    /// <summary>
    /// Создание выборки документов для выбранных узлов <see cref="TreeViewAdv.SelectedNodes"/>
    /// </summary>
    /// <param name="reason">Причина создания выборки</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason)
    {
      return CreateDocSel(reason, null);
    }

    /// <summary>
    /// Создание выборки документов для заданного списка узлов
    /// </summary>
    /// <param name="reason">Причина создания выборки</param>
    /// <param name="nodes">Список узлов.
    /// Если null, то используются выбранные узлы <see cref="TreeViewAdv.SelectedNodes"/></param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason, TreeNodeAdv[] nodes)
    {
      //if (GetDocSel == null)
      if (!HasGetDocSelHandler) // 22.02.2018
        return null;
      DBxDocSelection docSel = null;
      try
      {
        EFPApp.BeginWait(Res.Common_Phase_DocSelCreation, "DBxDocSelection");
        try
        {
          EFPDBxTreeViewDocSelEventArgs args = new EFPDBxTreeViewDocSelEventArgs(this, reason, nodes);
          OnGetDocSel(args);
          if (!args.DocSel.IsEmpty)
            docSel = args.DocSel;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
      return docSel;
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Пометить на обновление строки с заданными идентификаторами
    /// </summary>
    /// <param name="ids">Массив идентификаторов документов</param>
    public void UpdateRowsForIds(Int32[] ids)
    {
      IdList idList = new IdList(ids);
      UpdateRowsForIds(idList);
    }

    /// <summary>
    /// Пометить на обновление строки с заданными идентификаторами
    /// </summary>
    /// <param name="ids">Список идентификаторов документов</param>
    public void UpdateRowsForIds(IdList ids)
    {
      if (ids == null)
        return;

      // TODO: Не реализовано
      /*
      int FirstRow = Control.FirstDisplayedScrollingRowIndex;
      int n = Control.DisplayedRowCount(true);
      for (int i = 0; i < n; i++)
      {
        int Id = GetRowId(FirstRow + i);
        if (Array.IndexOf<int>(Ids, Id) >= 0)
          Control.InvalidateRow(FirstRow + i);
      } */
    }

    event EventHandler IEFPDBxView.SelectionChanged
    {
      add { Control.SelectionChanged += value; }
      remove { Control.SelectionChanged -= value; }
    }

    #endregion

    #region Поиск текста

    /// <summary>
    /// Добавляет кнопку "Выборка документов" в диалог поиска текста
    /// </summary>
    /// <returns>Объект <see cref="EFPDBxTreeViewSearchContext"/></returns>
    protected override IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPDBxTreeViewSearchContext(this);
    }

    #endregion

    #region Оформление просмотра

    // TODO: Возможно, есть смысл перенести в EFPTreeViewAdv

    /// <summary>
    /// Обработчик события <see cref="BindableControl.ValueNeeded"/> для <see cref="NodeStateIcon"/>.
    /// Вызывает виртуальный метод <see cref="GetNodeImageKey(TreeNodeAdv)"/>. Если метод вернул непустую строку,
    /// используется изображение из <see cref="EFPApp.MainImages"/>. Иначе будет использовано изображение по умолчанию.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void NodeStateIconValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      string imageKey = GetNodeImageKey(args.Node);
      if (!String.IsNullOrEmpty(imageKey))
        args.Value = EFPApp.MainImages.Images[imageKey];
    }

    /// <summary>
    /// Переопределенный метод может возвращать изображение для узла.
    /// Изображение должно быть в списке <see cref="EFPApp.MainImages"/>.
    /// Базовый метод возвращает пустую строку. В этом случае используется стандартное изображение для узла.
    /// </summary>
    /// <param name="node">Узел, для которого требуется изображение</param>
    /// <returns>Имя изображения</returns>
    public virtual string GetNodeImageKey(TreeNodeAdv node)
    {
      return String.Empty;
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для иерархического просмотра <see cref="EFPDBxTreeView"/>
  /// </summary>
  public class EFPDBxTreeViewCommandItems : EFPConfigurableDataTreeViewCommandItems
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPDBxTreeViewCommandItems(EFPDBxTreeView controlProvider)
      : base(controlProvider)
    {
      #region Создание команд

      AddSeparator();

      #region Отправить

      ciSendTo = new EFPCommandItem("SendTo", "DocSel");
      ciSendTo.MenuText = Res.Cmd_Menu_SendTo_DocSel;
      ciSendTo.ImageKey = "DBxDocSelection";
      ciSendTo.Parent = base.MenuSendTo;
      ciSendTo.Click += ciSendToDocSel_Click;
      ciSendTo.Usage = EFPCommandItemUsage.Menu; // без кнопки
      Add(ciSendTo);

      #endregion

      #endregion
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPDBxTreeView ControlProvider { get { return (EFPDBxTreeView)(base.ControlProvider); } }

    /// <summary>
    /// Установка свойств <see cref="EFPCommandItem.Usage"/>
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      if (!ControlProvider.HasGetDocSelHandler) // Есть обработчик
        ciSendTo.Usage = EFPCommandItemUsage.None;
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Добавляет к буферу обмена формат выборки документов <see cref="DBxDocSelection"/>, если
    /// метод <see cref="EFPDBxTreeView.CreateDocSel(EFPDBxViewDocSelReason)"/> возвращает значение, отличное от null.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnAddCopyFormats(DataObjectEventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxViewDocSelReason.Copy);
      if (docSel != null)
      {
        args.DataObject.SetData(docSel);
        ControlProvider.UI.OnAddCopyFormats(args.DataObject, docSel);
      }

      base.OnAddCopyFormats(args);
    }

    #endregion

    #region Отправить

    EFPCommandItem ciSendTo;

    private void ciSendToDocSel_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxViewDocSelReason.SendTo);
      if (docSel == null || docSel.IsEmpty)
      {
        EFPApp.ShowTempMessage(Res.DocSel_Msg_IsEmpty);
        return;
      }
      ControlProvider.UI.ShowDocSel(docSel);
    }

    #endregion
  }
}
