using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV;
using AgeyevAV.Config;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtForms
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
      Icon = EFPApp.MainImageIcon("Filter");
      _FormProvider = new EFPFormProvider(this);
      _FormProvider.ConfigSectionName = "GridFilterForm";

      #region Таблица фильтров

      FilterGridProvider = new EFPGridFilterEditorGridView(_FormProvider, FilterGrid);

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

        EFPConfigParamSetComboBox.DefaultSet DefSet = new EFPConfigParamSetComboBox.DefaultSet(cfgDefault);
        efpParamSet.DefaultSets.Add(DefSet);

        efpParamSet.AuxTextHandler = this;
      }
      else
        grpSets.Visible = false;

      #endregion

      #region Буфер обмена

      btnCopy.Image = EFPApp.MainImages.Images["Copy"];
      btnCopy.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpCopy = new EFPButton(_FormProvider, btnCopy);
      efpCopy.DisplayName = "Копировать";
      efpCopy.ToolTipText = "Копирует все фильтры, заданные в этом окне, в буфер обмена";
      efpCopy.Click += new EventHandler(efpCopy_Click);

      btnPaste.Image = EFPApp.MainImages.Images["Paste"];
      btnPaste.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpPaste = new EFPButton(_FormProvider, btnPaste);
      efpPaste.DisplayName = "Вставить";
      efpPaste.ToolTipText = "Заменяет текущие фильтры, ранее скопированными в буфер обмена." + Environment.NewLine +
        "Если фильтры были скопированы для другого просмотра, то будут вставлены только совместимые фильтры, а остальные - очищены";
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
        int FirstNotEmptyIndex = -1;
        for (int i = 0; i < Filters.Count; i++)
        {
          if (FirstNotEmptyIndex < 0 && (!Filters[i].IsEmpty))
            FirstNotEmptyIndex = i;
        }

        int StartIndex = Filters.IndexOf(startFilter);
        if (StartIndex < 0)
        {
          if (FirstNotEmptyIndex < 0)
            StartIndex = 0;
          else
            StartIndex = FirstNotEmptyIndex;
        }

        FilterGridProvider.CurrentRowIndex = StartIndex;
      }
      catch
      {
      }
    }

    #endregion

    #region Буфер обмена - кнопки внизу

    private void efpCopy_Click(object sender, EventArgs args)
    {
      TempCfg Sect = new TempCfg();
      Filters.WriteConfig(Sect);

      string[] Codes = new string[Filters.Count];
      for (int i = 0; i < Filters.Count; i++)
        Codes[i] = Filters[i].Code;

      FilterClipboardInfo Info = new FilterClipboardInfo(Filters.DBIdentity, Codes, Sect.PartAsXmlText);

      DataObject dobj = new DataObject();
      dobj.SetData(Info);
      Clipboard.SetDataObject(dobj);
    }

    private void efpPaste_Click(object sender, EventArgs args)
    {
      IDataObject dobj = Clipboard.GetDataObject();
      if (dobj == null)
      {
        EFPApp.ShowTempMessage("Буфер обмена пуст");
        return;
      }

      FilterClipboardInfo FilterInfo = dobj.GetData(typeof(FilterClipboardInfo)) as FilterClipboardInfo;

      if (FilterInfo == null)
      {
        // string txtFormats = String.Join(", ", dobj.GetFormats());
        EFPApp.ShowTempMessage("Буфер обмена не содержит фильтров табличного просмотра");
        return;
      }

      if ((FilterInfo.DBIdentity != Filters.DBIdentity) &&
        (!String.IsNullOrEmpty(FilterInfo.DBIdentity)) &&
        (!String.IsNullOrEmpty(Filters.DBIdentity)))
      {
        EFPApp.ShowTempMessage("Фильтры в буфере обмена относятся к другой базе данных");
        return;
      }

      TempCfg Sect = new TempCfg();
      Sect.PartAsXmlText = FilterInfo.XmlText;
      Filters.ClearAllFilters();
      Filters.ReadConfig(Sect);
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
      _AuxTextTempCfg = new TempCfg();
      Filters.WriteConfig(_AuxTextTempCfg);
    }

    public string GetAuxText(CfgPart cfg)
    {
      Filters.ClearAllFilters();
      Filters.ReadConfig(cfg);
      if (Filters.IsEmpty)
        return "Фильтры не установлены";

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
      Filters.ClearAllFilters();
      Filters.ReadConfig(_AuxTextTempCfg);
      _AuxTextTempCfg = null;
    }

    #endregion
  }

  /// <summary>
  /// Провайдер табличного просмотра для списка фильтров.
  /// Используется командой "Установить фильтр" в настраиваемых табличных и иерарахических просмотрах.
  /// Также может использоваться в диалогов параметров отчетов для редактирования пользовательских фильтров.
  /// Для работы требуется установить свойство Filters.
  /// Если в процессе показа окна фильтры обновляются снаружи провайдера EFPGridFilterEditorGridView,
  /// то должен быть вызван метод EFPGridFilterEditorGridView.PerformRefresh() для обновления.
  /// Редактирование фильтров возможно независимо от наличия вызовов IEFPGridFilters.BeginUpdate()/EndUpdate() и свойства IsReadOnly.
  /// </summary>
  public class EFPGridFilterEditorGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к DataGridView
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPGridFilterEditorGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элмент и панель инструментов</param>
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
      Columns.AddText("DisplayName", false, "Фильтр", 20, 10); // 1
      Columns.LastAdded.CanIncSearch = true;
      Columns.AddTextFill("FilterText", false, "Значение", 100, 10); // 2
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

      EFPCommandItem ciClearAll = new EFPCommandItem("View", "ClearFilter");
      ciClearAll.MenuText = "Очистить все фильтры";
      ciClearAll.ImageKey = "No";
      ciClearAll.ShortCut = Keys.F12;
      //ciClearAll.Usage = EFPCommandItemUsage.ShortCut | EFPCommandItemUsage.Menu; // обойдемся без значка
      ciClearAll.Click += new EventHandler(ciClear_Click);
      CommandItems.Add(ciClearAll);


      if (true/*AccDepClientExec.DebugShowIds*/) // TODO:
      {
        EFPCommandItem ciXml = new EFPCommandItem("View", "XML");
        ciXml.MenuText = "Просмотр фильтров в XML";
        ciXml.Click += new EventHandler(ciXml_Click);
        CommandItems.Add(ciXml);
      }
    }

    #endregion

    #region Свойство Filters

    /// <summary>
    /// Основное свойство - список редактируемых фильтров.
    /// Установка нового значения свойства приводит к обновлению списка строк.
    /// Повторная установка ссылки на тот же набор фильтров приводит к обновлению значений фильтра
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
      IEFPGridFilter Filter;
      switch (State)
      {
        case EFPDataGridViewState.Edit:
          EFPDialogPosition dialogPosition = new EFPDialogPosition();
          Rectangle rc = Control.GetCellDisplayRectangle(2, CurrentRowIndex, false);
          dialogPosition.PopupOwnerBounds = Control.RectangleToScreen(rc);
          Filter = Filters[this.CurrentRowIndex];
          if (Filter.ShowFilterDialog(dialogPosition))
          {
            // 04.07.2007.    
            // Фильтры могут быть взаимозависимыми, например, фильтр по операции импорта
            // и по источнику данных, поэтому надо обновить всю таблицу фильтров
            PerformRefresh();
            //RefreshFilter(TheGrid.CurrentCell.RowIndex);
          }
          break;

        case EFPDataGridViewState.Delete:
          int[] FilterIndices = this.SelectedRowIndices;
          for (int i = 0; i < FilterIndices.Length; i++)
          {
            Filter = Filters[FilterIndices[i]];
            Filter.Clear();
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
      int[] FilterIndices = this.SelectedRowIndices;
      if (FilterIndices.Length == 0)
        return;
      string[] Codes = new string[FilterIndices.Length];
      TempCfg Sect = new TempCfg();
      for (int i = 0; i < FilterIndices.Length; i++)
      {
        IEFPGridFilter Item = Filters[FilterIndices[i]];
        Codes[i] = Item.Code;
        if (!Item.IsEmpty)
        {
          CfgPart cfg2 = Sect.GetChild(Item.Code, true);
          Item.WriteConfig(cfg2);
        }
      }

      FilterClipboardInfo Info = new FilterClipboardInfo(Filters.DBIdentity, Codes, Sect.PartAsXmlText);
      args.DataObject.SetData(Info);
    }

    void fmtFilter_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      FilterClipboardInfo FilterInfo = args.GetData() as FilterClipboardInfo;
      if (FilterInfo == null)
        return;

      if ((FilterInfo.DBIdentity != Filters.DBIdentity) &&
        (!String.IsNullOrEmpty(FilterInfo.DBIdentity)) &&
        (!String.IsNullOrEmpty(Filters.DBIdentity)))
      {
        EFPApp.ShowTempMessage("Фильтры в буфере обмена относятся к другой базе данных");
        return;
      }

      TempCfg Sect = new TempCfg();
      Sect.PartAsXmlText = FilterInfo.XmlText;
      bool Flag = false;
      for (int i = 0; i < FilterInfo.Names.Length; i++)
      {
        int FilterIndex = Filters.IndexOf(FilterInfo.Names[i]);
        if (FilterIndex >= 0)
        {
          IEFPGridFilter Item = Filters[FilterIndex];
          try
          {
            CfgPart cfgOne = Sect.GetChild(Item.Code, false);
            if (cfgOne == null)
              Item.Clear();
            else
              Item.ReadConfig(cfgOne);

            Flag = true;
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, "Ошибка вставки фильтра \"" + Item.DisplayName + "\" из буфера обмена");
          }
        }
      }

      if (Flag)
        PerformRefresh();
      else
        EFPApp.ShowTempMessage("Фильтры в буфере обмена отсутствуют в списке возможных фильтров");
    }


    void TheGridHandler_Cut(object sender, EventArgs args)
    {
      try
      {
        DoCut();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Вырезка фильтров в буфер обмена");
      }
    }

    private void DoCut()
    {
      CommandItems.PerformCopy();
      int[] FilterIndices = SelectedRowIndices;
      bool Flag = false;
      for (int i = 0; i < FilterIndices.Length; i++)
      {
        IEFPGridFilter Item = Filters[FilterIndices[i]];
        if (!Item.IsEmpty)
        {
          Item.Clear();
          Flag = true;
        }
      }
      if (Flag)
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
      TempCfg Sect = new TempCfg();
      Filters.WriteConfig(Sect);
      DebugTools.DebugXml(Sect.Document, "Текущие фильтры");
    }

    #endregion
  }
}