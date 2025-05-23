﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма установки фильтров для табличного просмотра
  /// </summary>
  internal partial class GridFilterForm : Form, IEFPConfigParamSetHandler, IEFPConfigParamSetAuxTextHandler
  {
    #region Конструктор

    public GridFilterForm(IEFPControlWithFilters callerControlProvider, TempCfg cfgDefault, CfgPart cfgOriginal)
    {
      _CallerControlProvider = callerControlProvider;


      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["Filter"];
      _FormProvider = new EFPFormProvider(this);
      _FormProvider.ConfigSectionName = "GridFilterForm";

      #region Таблица фильтров

      FilterGridProvider = new EFPGridFilterEditorGridView(_FormProvider, FilterGrid);
      string baseTitle = callerControlProvider.DisplayName;
      IEFPDataView callerControlProvider2 = callerControlProvider as IEFPDataView;
      if (callerControlProvider2 != null)
      {
        if (callerControlProvider2.DefaultOutItem != null)
        {
          if (!String.IsNullOrEmpty(callerControlProvider2.DefaultOutItem.Title))
            baseTitle = callerControlProvider2.DefaultOutItem.Title;
        }
      }
        //if (callerControlProvider.out)
      FilterGridProvider.DefaultOutItem.Title = String.Format(Res.EFPGridFilterEditorGridView_Name_Default, baseTitle);
      FilterGridProvider.CommandItems.ClipboardInToolBar = true;

      FilterGridProvider.ToolBarPanel = panSpb;

      FilterGridProvider.Filters = Filters;

      // Дополнительная инициализация прикладной программы
      callerControlProvider.InitGridFilterEditorGridView(FilterGridProvider);

      if (!EFPApp.ShowListImages)
        FilterGrid.Columns[0].Visible = false; // 14.09.2016

      #endregion

      #region Готовые наборы

      _UseHistory = ((!String.IsNullOrEmpty(callerControlProvider.ConfigHandler.ConfigSectionName)) &&
         EFPConfigTools.IsPersist(_CallerControlProvider.ConfigManager.Persistence));

      if (_UseHistory)
      {
        efpParamSet = new EFPConfigParamSetComboBox(_FormProvider, SetComboBox, this);
        efpParamSet.ConfigSectionName = callerControlProvider.ConfigHandler.ConfigSectionName;
        efpParamSet.ParamsCategory = EFPConfigCategories.Filters;
        efpParamSet.HistoryCategory = EFPConfigCategories.FiltersHistory;

        // 06.12.2024
        // Прогоняем конфигурацию по умолчанию через управляющие элементы, чтобы сделать полноценую настройку
        //cfgDefault = efpParamSet.UpdateDefaultConfig(cfgDefault);

        EFPConfigParamDefaultSet defSet = new EFPConfigParamDefaultSet(cfgDefault);
        efpParamSet.DefaultSets.Add(defSet);

        efpParamSet.AuxTextHandler = this;
      }
      else
        grpSets.Visible = false;

      #endregion

      #region Буфер обмена

      btnCopy.Image = EFPApp.MainImages.Images["Copy"];
      btnCopy.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpCopy = new EFPButton(_FormProvider, btnCopy);
      efpCopy.DisplayName = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_Copy);
      efpCopy.ToolTipText = Res.EFPGridFilterEditorGridView_ToolTip_Copy;
      efpCopy.Click += new EventHandler(efpCopy_Click);

      btnPaste.Image = EFPApp.MainImages.Images["Paste"];
      btnPaste.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpPaste = new EFPButton(_FormProvider, btnPaste);
      efpPaste.DisplayName = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_Paste);
      efpPaste.ToolTipText = Res.EFPGridFilterEditorGridView_ToolTip_Paste;
      efpPaste.Click += new EventHandler(efpPaste_Click);

      #endregion
    }

    #endregion

    #region Свойства

    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Используются ли готовые наборы
    /// </summary>
    private bool _UseHistory;

    /// <summary>
    /// Таблица фильтров
    /// </summary>
    public EFPGridFilterEditorGridView FilterGridProvider;

    /// <summary>
    /// Вызывающий табличный просмотр
    /// </summary>
    IEFPControlWithFilters _CallerControlProvider;

    IEFPGridFilters Filters { get { return _CallerControlProvider.Filters; } }

    /// <summary>
    /// Готовые наборы
    /// </summary>
    private EFPConfigParamSetComboBox efpParamSet;

    #endregion

    #region Строка фильтра, выбираемая по умолчанию

    /// <summary>
    /// Выбор первой выбранной строки в таблице фильтров.
    /// Активируется либо строка первого непустого фильтра,
    /// либо заданного фильтра
    /// </summary>
    /// <param name="startFilter">Имя фильтра, который нужно сделать активным</param>
    public void SetStartFilter(string startFilter)
    {
      try
      {
        int firstNotEmptyIndex = -1;
        for (int i = 0; i < Filters.Count; i++)
        {
          if (firstNotEmptyIndex < 0 && (!Filters[i].IsEmpty))
            firstNotEmptyIndex = i;
        }

        int startIndex = Filters.IndexOf(startFilter);
        if (startIndex < 0)
        {
          if (firstNotEmptyIndex < 0)
            startIndex = 0;
          else
            startIndex = firstNotEmptyIndex;
        }

        FilterGridProvider.CurrentRowIndex = startIndex;
      }
      catch
      {
      }
    }

    #endregion

    #region Буфер обмена - кнопки внизу

    private void efpCopy_Click(object sender, EventArgs args)
    {
      TempCfg sect = new TempCfg();
      Filters.WriteConfig(sect);

      string[] codes = new string[Filters.Count];
      for (int i = 0; i < Filters.Count; i++)
        codes[i] = Filters[i].Code;

      FilterClipboardInfo info = new FilterClipboardInfo(Filters.DBIdentity, codes, sect.PartAsXmlText);

      DataObject dobj = new DataObject();
      dobj.SetData(info);
      Clipboard.SetDataObject(dobj);
    }

    private void efpPaste_Click(object sender, EventArgs args)
    {
      IDataObject dobj = Clipboard.GetDataObject();
      if (dobj == null)
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_Empty);
        return;
      }

      FilterClipboardInfo filterInfo = dobj.GetData(typeof(FilterClipboardInfo)) as FilterClipboardInfo;

      if (filterInfo == null)
      {
        // string txtFormats = String.Join(", ", dobj.GetFormats());
        EFPApp.ShowTempMessage(Res.Clipboard_Err_NoFilters);
        return;
      }

      if ((filterInfo.DBIdentity != Filters.DBIdentity) &&
        (!String.IsNullOrEmpty(filterInfo.DBIdentity)) &&
        (!String.IsNullOrEmpty(Filters.DBIdentity)))
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_FiltersDiffDB);
        return;
      }

      TempCfg sect = new TempCfg();
      sect.PartAsXmlText = filterInfo.XmlText;
      Filters.ClearAllFilters();
      Filters.ReadConfig(sect);
      FilterGridProvider.PerformRefresh();
    }

    #endregion

    #region IEFPConfigParamSetHandler Members

    public void ConfigToControls(CfgPart cfg)
    {
      Filters.ClearAllFilters(); // Если после сохранения набора программа изменилась и появились новые фильтры, то они они
      // должны стать пустыми. Без вызова Clear() они сохранят текущее значение
      Filters.ReadConfig(cfg);

      FilterGridProvider.PerformRefresh();
    }

    public void ConfigFromControls(CfgPart cfg)
    {
      cfg.Clear();
      Filters.WriteConfig(cfg);
    }

    #endregion

    #region IEFPConfigParamSetAuxTextHandler Members

    private TempCfg _AuxTextTempCfg;

    public void BeginGetAuxText()
    {
#if DEBUG
      if (_AuxTextTempCfg != null)
        throw new ReenteranceException();
#endif
      _AuxTextTempCfg = new TempCfg();
      Filters.WriteConfig(_AuxTextTempCfg);
    }

    public string GetAuxText(CfgPart cfg)
    {
      Filters.ClearAllFilters();
      Filters.ReadConfig(cfg);
      if (Filters.IsEmpty)
        return Res.EFPGridFilterEditorGridView_Msg_NoFilters;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < Filters.Count; i++)
      {
        if (!Filters[i].IsEmpty)
        {
          if (sb.Length > 0)
            sb.Append(", ");
          sb.Append(Filters[i].DisplayName);
          sb.Append('=');
          sb.Append(Filters[i].FilterText);
        }
      }
      return sb.ToString();
    }

    public void EndGetAuxText()
    {
#if DEBUG
      if (_AuxTextTempCfg == null)
        throw ExceptionFactory.UnpairedCall(this, "BeginGetAuxText()", "EndGetAuxText()");
#endif
      Filters.ClearAllFilters();
      Filters.ReadConfig(_AuxTextTempCfg);
      _AuxTextTempCfg = null;
    }

    #endregion
  }

  /// <summary>
  /// Провайдер табличного просмотра для списка фильтров (классы <see cref="EFPGridFilters"/>, <see cref="FreeLibSet.Forms.Data.EFPDBxGridFilters"/> или пользовательская реализация <see cref="IEFPGridFilters"/>).
  /// Используется командой "Установить фильтр" в настраиваемых табличных и иерарахических просмотрах.
  /// Также может использоваться в диалогов параметров отчетов для редактирования пользовательских фильтров.
  /// Для работы требуется установить свойство Filters.
  /// Если в процессе показа окна фильтры обновляются снаружи провайдера <see cref="EFPGridFilterEditorGridView"/>,
  /// то должен быть вызван метод этого объекта <see cref="EFPDataGridView.PerformRefresh()"/> для обновления.
  /// Редактирование фильтров возможно независимо от наличия вызовов <see cref="IEFPGridFilters.BeginUpdate()"/> / <see cref="IEFPGridFilters.EndUpdate()"/> и свойства <see cref="IReadOnlyObject.IsReadOnly"/>.
  /// </summary>
  public class EFPGridFilterEditorGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к <see cref="DataGridView"/>.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPGridFilterEditorGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к <see cref="IEFPControlWithToolBar{DataGridView}"/>.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPGridFilterEditorGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      Control.VirtualMode = false;
      Control.AutoGenerateColumns = false;
      Columns.AddImage(); // 0
      Columns.AddText("DisplayName", false, Res.EFPGridFilterEditorGridView_ColTitle_DisplayName, 20, 10); // 1
      Columns.LastAdded.CanIncSearch = true;
      Columns.LastAdded.PrintWidth = 500;
      Columns.AddTextFill("FilterText", false, Res.EFPGridFilterEditorGridView_ColTitle_FilterText, 100, 10); // 2
      DisableOrdering();

      Control.ReadOnly = true;
      ReadOnly = false;
      CanInsert = false;
      CanDelete = true;
      CanView = false;
      CanMultiEdit = false;
      Control.MultiSelect = true;
      Control.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      CommandItems.UseRefresh = false;

      CommandItems.EnterAsOk = false;
      CommandItems.AddCopyFormats += new DataObjectEventHandler(TheGridHandler_AddCopyFormats);
      CommandItems.Cut += new EventHandler(TheGridHandler_Cut);

      EFPPasteFormat fmtFilter = new EFPPasteFormat(typeof(FilterClipboardInfo));
      fmtFilter.DisplayName = "Фильтры";
      fmtFilter.Paste += new EFPPasteDataObjectEventHandler(fmtFilter_Paste);
      CommandItems.PasteHandler.Add(fmtFilter);

      EFPCommandItem ciClearAll = new EFPCommandItem("View", "ClearAllFilters");
      ciClearAll.MenuText = Res.EFPGridFilterEditorGridView_Menu_ClearAllFilters;
      ciClearAll.ImageKey = "No";
      ciClearAll.ShortCut = Keys.F8;
      //ciClearAll.Usage = EFPCommandItemUsage.ShortCut | EFPCommandItemUsage.Menu; // обойдемся без значка
      ciClearAll.Click += new EventHandler(ciClear_Click);
      CommandItems.Add(ciClearAll);


      if (true/*AccDepClientExec.DebugShowIds*/) // TODO:
      {
        EFPCommandItem ciXml = new EFPCommandItem("View", "ViewFiltersXML");
        ciXml.MenuText = Res.EFPGridFilterEditorGridView_Menu_ViewFiltersXML;
        ciXml.Click += new EventHandler(ciXml_Click);
        CommandItems.Add(ciXml);
      }
    }

    #endregion

    #region Свойство Filters

    /// <summary>
    /// Основное свойство - список редактируемых фильтров.
    /// Установка нового значения свойства приводит к обновлению списка строк.
    /// Повторная установка ссылки на тот же набор фильтров приводит к обновлению значений фильтра.
    /// </summary>
    public IEFPGridFilters Filters
    {
      get { return _Filters; }
      set
      {
        if (value == null)
          value = new EFPDummyGridFilters();

        if (!Object.ReferenceEquals(value, _Filters))
        {
          _Filters = value;
          // Заполнение строк фильтров
          Control.RowCount = Filters.Count;

          for (int i = 0; i < Filters.Count; i++)
          {
            Control.Rows[i].Cells[1].Value = Filters[i].DisplayName;
            Control.Rows[i].Tag = Filters[i];
          }
        }

        PerformRefresh();
      }
    }
    private IEFPGridFilters _Filters;

    #endregion

    #region Обновление таблички фильтров

    /// <summary>
    /// Обновление всего списка фильтров
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnRefreshData(EventArgs args)
    {
      for (int i = 0; i < Filters.Count; i++)
        RefreshFilter(Filters[i], Control.Rows[i]);

      base.OnRefreshData(args);
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasRefreshDataHandler { get { return true; } }

    private void RefreshFilter(IEFPGridFilter filter, DataGridViewRow row)
    {
      row.Cells[2].Value = filter.FilterText;

      if (EFPApp.ShowListImages)
      {
        if (filter.IsEmpty)
          row.Cells[0].Value = EFPApp.MainImages.Images[/*"Cancel"*/"EmptyImage"];
        else
        {
          string imageKey = null;
          IEFPGridFilterWithImageKey filter2 = filter as IEFPGridFilterWithImageKey;
          if (filter2 != null)
            imageKey = filter2.FilterImageKey;
          if (String.IsNullOrEmpty(imageKey))
            imageKey = EFPGridFilterTools.DefaultFilterImageKey;
          row.Cells[0].Value = EFPApp.MainImages.Images[imageKey];
        }
      }
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Редактирование или очистка фильтра
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Возвращает true</returns>
    protected override bool OnEditData(EventArgs args)
    {
      IEFPGridFilter filter;
      switch (State)
      {
        case UIDataState.Edit:
          EFPDialogPosition dialogPosition = new EFPDialogPosition();
          Rectangle rc = Control.GetCellDisplayRectangle(2, CurrentRowIndex, false);
          dialogPosition.PopupOwnerBounds = Control.RectangleToScreen(rc);
          filter = Filters[this.CurrentRowIndex];
          if (filter.ShowFilterDialog(dialogPosition))
          {
            // 04.07.2007.    
            // Фильтры могут быть взаимозависимыми, например, фильтр по операции импорта
            // и по источнику данных, поэтому надо обновить всю таблицу фильтров
            PerformRefresh();
            //RefreshFilter(TheGrid.CurrentCell.RowIndex);
          }
          break;

        case UIDataState.Delete:
          int[] filterIndices = this.SelectedRowIndices;
          for (int i = 0; i < filterIndices.Length; i++)
          {
            filter = Filters[filterIndices[i]];
            filter.Clear();
          }
          PerformRefresh();
          break;
      }
      return true;
    }

    #endregion

    #region Буфер обмена в табличном просмотре

    void TheGridHandler_AddCopyFormats(object sender, DataObjectEventArgs args)
    {
      int[] filterIndices = this.SelectedRowIndices;
      if (filterIndices.Length == 0)
        return;
      string[] codes = new string[filterIndices.Length];
      TempCfg sect = new TempCfg();
      for (int i = 0; i < filterIndices.Length; i++)
      {
        IEFPGridFilter item = Filters[filterIndices[i]];
        codes[i] = item.Code;
        if (!item.IsEmpty)
        {
          CfgPart cfg2 = sect.GetChild(item.Code, true);
          item.WriteConfig(cfg2);
        }
      }

      FilterClipboardInfo info = new FilterClipboardInfo(Filters.DBIdentity, codes, sect.PartAsXmlText);
      args.DataObject.SetData(info);
    }

    void fmtFilter_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      FilterClipboardInfo filterInfo = args.GetData() as FilterClipboardInfo;
      if (filterInfo == null)
        return;

      if ((filterInfo.DBIdentity != Filters.DBIdentity) &&
        (!String.IsNullOrEmpty(filterInfo.DBIdentity)) &&
        (!String.IsNullOrEmpty(Filters.DBIdentity)))
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_FiltersDiffDB);
        return;
      }

      TempCfg sect = new TempCfg();
      sect.PartAsXmlText = filterInfo.XmlText;
      bool flag = false;
      for (int i = 0; i < filterInfo.Names.Length; i++)
      {
        int filterIndex = Filters.IndexOf(filterInfo.Names[i]);
        if (filterIndex >= 0)
        {
          IEFPGridFilter item = Filters[filterIndex];
          try
          {
            CfgPart cfgOne = sect.GetChild(item.Code, false);
            if (cfgOne == null)
              item.Clear();
            else
              item.ReadConfig(cfgOne);

            flag = true;
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, String.Format(Res.Clipboard_ErrTitle_PasteFilters, item.DisplayName));
          }
        }
      }

      if (flag)
        PerformRefresh();
      else
        EFPApp.ShowTempMessage(Res.Clipboard_Err_OtherFilters);
    }


    void TheGridHandler_Cut(object sender, EventArgs args)
    {
      try
      {
        DoCut();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private void DoCut()
    {
      CommandItems.PerformCopy();
      int[] filterIndices = SelectedRowIndices;
      bool flag = false;
      for (int i = 0; i < filterIndices.Length; i++)
      {
        IEFPGridFilter item = Filters[filterIndices[i]];
        if (!item.IsEmpty)
        {
          item.Clear();
          flag = true;
        }
      }
      if (flag)
        PerformRefresh();
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Очистка всех фильтров
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ciClear_Click(object sender, EventArgs args)
    {
      for (int i = 0; i < Filters.Count; i++)
        Filters[i].Clear();
      PerformRefresh();
    }

    void ciXml_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      TempCfg sect = new TempCfg();
      Filters.WriteConfig(sect);
      //FreeLibSet.Forms.Diagnostics.DebugTools.DebugXml(sect.Document, "Текущие фильтры");
      EFPApp.ShowXmlView(sect.Document, ci.MenuTextWithoutMnemonic);
    }

    #endregion
  }
}
