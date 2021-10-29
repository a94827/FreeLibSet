using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;


/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Управляющие элементы, встраиваемые в редактор настройки табличного просмотра.
  /// Сама форма не используется
  /// </summary>
  internal partial class EFPGridProducerEditor : Form, IEFPGridProducerEditor
  {
    #region Конструктор

    public EFPGridProducerEditor(EFPGridProducer gridProducer, IEFPGridControl controlProvider, EFPBaseProvider baseProvider)
    {
      InitializeComponent();
      this._GridProducer = gridProducer;
      this._TheControlProvider = controlProvider;

      //efpForm.AddFormCheck(new EFPValidatingEventHandler(ValidateForm));
      TheTabControl.ImageList = EFPApp.MainImages;
      tpColumns.ImageKey = "TableColumns";
      tpToolTips.ImageKey = "ToolTip";

      _OrgConfig = controlProvider.CurrentConfig.Clone(controlProvider);

      #region Таблица столбцов

      ghColumns = new EFPDataGridView(baseProvider, grColumns);
      /*
      if (TheControlProvider.UI.DebugShowIds)
      {
        ghColumns.Columns.AddText("ColumnName", false, "ColumnName", 30, 10);
        ghColumns.Columns.AddText("FieldNames", false, "FieldNames", 30, 10);
      } */
      ghColumns.DisableOrdering();
      ghColumns.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ghColumns_GetCellAttributes);
      ghColumns.ReadOnly = true;
      ghColumns.CanInsert = false; // !!!
      ghColumns.CanDelete = false;
      ghColumns.CanView = false;
      ghColumns.Columns[1].CanIncSearch = true;
      grColumns.MultiSelect = false;
      grColumns.ReadOnly = false;
      //InitColumnsGrid();
      ghColumns.CommandItems.ManualOrderRows = true;
      ghColumns.MarkRowsColumnIndex = 0;
      ghColumns.CellFinished += new EFPDataGridViewCellFinishedEventHandler(ghColumns_CellFinished);
      ghColumns.ToolBarPanel = panSpbColumns;

      #endregion

      // Так нельзя. В просмотре могут быть заморожены посторонние столбцы,
      // например, картинка
      //edFrozenColumns.ValueEx = TheHandler.FrozenColumns;


      // Список выбора активного столбца
      //StartColumnName = ControlProvider.CurrentConfig.StartColumnName;
      //InitCBStartCol();

      efpStartColumn = new EFPListComboBox(baseProvider, cbStartColumn);
      efpStartColumn.ToolTipText = "Если указан столбец, то он будет активироваться при каждом открытии табличного просмотра." + Environment.NewLine +
        "Если выбран вариант \"[ Нет ]\", то последний выбранный столбец запоминается между открытиями просмотра";

      #region Таблица подсказок

      if (EFPApp.ShowToolTips)
      {
        ghToolTips = new EFPDataGridView(baseProvider, grToolTips);
        /*
        if (TheControlProvider.UI.DebugShowIds)
        {
          ghToolTips.Columns.AddText("Name", false, "Name", 30, 10);
          ghToolTips.Columns.AddText("FieldNames", false, "FieldNames", 30, 10);
        } */
        ghToolTips.DisableOrdering();
        ghToolTips.ReadOnly = true;
        ghToolTips.CanInsert = false; // !!!
        ghToolTips.CanDelete = false;
        ghToolTips.CanView = false;
        grToolTips.MultiSelect = false;
        grToolTips.ReadOnly = false;
        ghToolTips.Columns[1].CanIncSearch = true;
        //InitToolTipsGrid();
        ghToolTips.CommandItems.ManualOrderRows = true;
        ghToolTips.MarkRowsColumnIndex = 0;
        ghToolTips.ToolBarPanel = panSpbToolTips;
      }
      else
      {
        tpToolTips.Controls.Clear();
        Label lbl = new Label();
        lbl.Dock = DockStyle.Fill;
        lbl.Text = "Всплывающие подсказки отключены";
        lbl.TextAlign = ContentAlignment.MiddleCenter;
        lbl.BackColor = SystemColors.Info;
        lbl.ForeColor = SystemColors.InfoText;
        tpToolTips.Controls.Add(lbl);
      }

      #endregion
    }

    #endregion

    #region Поля

    /// <summary>
    /// Копия конфигурации до начала редактирования
    /// </summary>
    private EFPDataGridViewConfig _OrgConfig;

    private EFPDataGridView ghColumns, ghToolTips;

    private EFPGridProducer _GridProducer;
    private IEFPGridControl _TheControlProvider;

    /// <summary>
    /// Список "Активировать при открытии".
    /// Кодами являются коды столбцов, текстом - названия. Пустой код - "Нет"
    /// </summary>
    private EFPListComboBox efpStartColumn;

    //private string StartColumnName;

    //private List<string> CBStartColumnNames = new List<string>();

    #endregion

    #region Столбцы

    /*
     * Поле Tag каждой строки таблицы grColumns указывает либо на объект 
     * GridHandlerColumn, если столбец присоединен к просмотру, либо на объект
     * GridProducerColumn, если столбца нет
     */

    /// <summary>
    /// Заполняет таблицу столбцов из заданной конфигурации
    /// </summary>
    /// <param name="config">Конфигурация, откуда берутся размеры столбоы</param>
    private void WriteFormColumns(EFPDataGridViewConfig config)
    {
      // Строки таблицы столбцов, соответствующие объявлениям в GridProducer
      DataGridViewRow[] OrdRows = new DataGridViewRow[_GridProducer.Columns.Count];

      int CurrentColumnIndex = _TheControlProvider.CurrentColumnIndex; // чтобы не вычислять в цикле
      DataGridViewRow StartRow1 = null; // строка, соответствующая активному столбцу в просмотре
      DataGridViewRow StartRow2 = null; // первая строка, содержащая "галочку"

      ghColumns.BeginUpdate();
      try
      {
        grColumns.RowCount = 0;

        // Сначала добавляем строки из настройки
        //int cntFrozen = 0;
        for (int i = 0; i < config.Columns.Count; i++)
        {
          EFPDataGridViewConfigColumn Column = config.Columns[i];

          EFPGridProducerColumn ColumnProducer = _GridProducer.Columns[Column.ColumnName];
          if (ColumnProducer == null)
            continue;


          int ColWidth = Column.Width;
          if (ColWidth == 0)
            ColWidth = ColumnProducer.GetWidth(_TheControlProvider.Measures); // 30.03.2017

          grColumns.Rows.Add(true,
            ColumnProducer.DisplayName,
            ColWidth,
            Column.FillMode ?
              (object)(int)(Column.FillWeight) : null);
          int ColumnProducerIndex = _GridProducer.Columns.IndexOf(Column.ColumnName);
          OrdRows[ColumnProducerIndex] = grColumns.Rows[grColumns.Rows.Count - 1];

          DataGridViewRow Row = grColumns.Rows[grColumns.RowCount - 1];
          Row.Tag = _GridProducer.Columns[ColumnProducerIndex];
          /*
          if (GridProducer.UI.DebugShowIds)
          {
            Row.Cells[4].Value = ColumnProducer.ColumnName;
            DBxColumnList List = new DBxColumnList();
            ColumnProducer.GetColumnNames(List);
            Row.Cells[5].Value = String.Join(", ", List.ToArray());
          } */
          //if (Column.GridColumn.Frozen)
          //  cntFrozen++;
          if (CurrentColumnIndex == i)
            StartRow1 = Row;
          if (StartRow2 == null) // Исправлено 05.01.2021
            StartRow2 = Row;
        }

        // Теперь перебираем столбцы из GridProducer
        for (int i = 0; i < _GridProducer.Columns.Count; i++)
        {
          EFPGridProducerColumn Column = _GridProducer.Columns[i];
          if (OrdRows[i] != null)
            continue; // Столбец уже был добавлен

          EFPDataGridViewConfigColumn ColumnConfig = null;
          //        if (TheHandler.CurrentGridConfig != null)
          //          ColumnConfig = TheHandler.CurrentGridConfig.Columns[Column.ColumnName];

          int w = 0;
          bool FillMode = false;
          int FillWeight = 100;
          if (ColumnConfig != null)
          {
            w = ColumnConfig.Width;
            FillMode = ColumnConfig.FillMode;
            FillWeight = ColumnConfig.FillWeight;
          }
          if (w == 0)
            //w = TheControlProvider.Measures.GetTextColumnWidth(Column.TextWidth);
            w = Column.GetWidth(_TheControlProvider.Measures);

          int NextIndex;
          if (i > 0)
            NextIndex = OrdRows[i - 1].Index + 1;
          else
            NextIndex = 0;

          if (NextIndex >= grColumns.Rows.Count)
            grColumns.Rows.Add();
          else
            grColumns.Rows.Insert(NextIndex, 1);
          DataGridViewRow Row = grColumns.Rows[NextIndex];
          OrdRows[i] = Row;
          Row.Tag = Column;
          Row.Cells[0].Value = false;
          Row.Cells[1].Value = Column.DisplayName;
          Row.Cells[2].Value = w;
          if (FillMode)
            Row.Cells[3].Value = FillWeight;
          /*
          if (TheControlProvider.UI.DebugShowIds)
          {
            Row.Cells[4].Value = Column.ColumnName;
            DBxColumnList List = new DBxColumnList();
            Column.GetColumnNames(List);
            Row.Cells[5].Value = String.Join(", ", List.ToArray());
          } */
        }
      }
      finally
      {
        ghColumns.EndUpdate();
      }
      ghColumns.CommandItems.DefaultManualOrderRows = OrdRows; // востановление порядка по умолчанию

      // Активируем строку, соответствующую текущему столбцу в просмотре или с первой отметкой
      if (StartRow1 != null)
        ghColumns.CurrentGridRow = StartRow1;
      else
      {
        if (StartRow2 != null)
          ghColumns.CurrentGridRow = StartRow2;
      }
    }

    /// <summary>
    /// Заполняет конфигурацию из текущих значений в табличке столбцов
    /// </summary>
    /// <param name="config">Заполняемая конфигурация просмотра</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>true, если введенные в табличке значения не содержат ошибок</returns>
    private bool ReadFormColumns(EFPDataGridViewConfig config, out string errorText)
    {
      grColumns.EndEdit();

      for (int i = 0; i < grColumns.Rows.Count; i++)
      {
        DataGridViewRow Row = grColumns.Rows[i];
        if (!DataTools.GetBool(Row.Cells[0].Value))
          continue;
        EFPGridProducerColumn ColDef = GetProducerColumn(Row);
        EFPDataGridViewConfigColumn ColumnCfg = new EFPDataGridViewConfigColumn(ColDef.Name);
        ColumnCfg.Width = DataTools.GetInt(Row.Cells[2].Value);
        int Percent = DataTools.GetInt(Row.Cells[3].Value);
        if (Percent > 0)
        {
          ColumnCfg.FillMode = true;
          ColumnCfg.FillWeight = Percent;
        }
        config.Columns.Add(ColumnCfg);
      }
      if (config.Columns.Count == 0)
      {
        errorText = "Не выбрано ни одного столбца";
        ghColumns.SetFocus();
        return false;
      }


      errorText = null;
      return true;
    }

    void ghColumns_CellFinished(object sender, EFPDataGridViewCellFinishedEventArgs args)
    {
      InitStartColumnList();
    }

    void ghColumns_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      switch (args.ColumnIndex)
      {
        case 2: // ширина
        case 3: // процент
          DataGridViewRow Row = grColumns.Rows[args.RowIndex];
          EFPGridProducerColumn ColProd = GetProducerColumn(Row);
          if (ColProd == null)
            return;


          if (!ColProd.Resizable)
          {
            args.Grayed = true;
            args.ReadOnly = true;
            args.ReadOnlyMessage = "Для столбца \"" + ColProd.DisplayName + "\" нельзя менять ширину";
          }
          break;
      }
    }

    #endregion

    #region Столбец, активируемый при открытии

#if XXX
    private bool InsideSelectStartColumn = false;

    private void InitCBStartCol()
    {
      if (InsideSelectStartColumn)
        return;
      InsideSelectStartColumn = true;
      try
      {
        cbActivateCol.Items.Clear();
        cbActivateCol.Items.Add("[ Авто ]");
        CBStartColumnNames.Clear();
        int idx = 0;
        for (int i = 0; i < ghColumns.Control.RowCount; i++)
        {
          DataGridViewRow Row = ghColumns.Control.Rows[i];
          bool Flag = DataTools.GetBool(Row.Cells[0].Value);
          if (Flag)
          {
            GridProducerColumn Col = GetProducerColumn(Row);
            cbActivateCol.Items.Add(Col.DisplayName);
            CBStartColumnNames.Add(Col.ColumnName);
            if (Col.ColumnName == StartColumnName)
              idx = i + 1;
          }
        }
        cbActivateCol.SelectedIndex = idx;
      }
      finally
      {
        InsideSelectStartColumn = false;
      }
    }

    private void cbActivateCol_Enter(object sender, EventArgs e)
    {
      // При входе в элемент перестраиваем список
      InitCBStartCol();
    }

    private void cbActivateCol_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (InsideSelectStartColumn)
        return;
      InsideSelectStartColumn = true;
      try
      {
        if (cbActivateCol.SelectedIndex < 1)
          StartColumnName = string.Empty;
        else
          StartColumnName = CBStartColumnNames[cbActivateCol.SelectedIndex - 1];

      }
      finally
      {
        InsideSelectStartColumn = false;
      }
    }
#endif

    /// <summary>
    /// Инициализирует выпадающий список "Активировать при открытии" и пытается выбрать ранее выбранный код
    /// </summary>
    private void InitStartColumnList()
    {
      List<string> Codes = new List<string>();
      List<string> Names = new List<string>();

      Codes.Add(String.Empty);
      //Names.Add("[ Авто ]");
      Names.Add("[ Нет ]"); // 19.05.2021

      string CurrCode = efpStartColumn.SelectedCode;

      for (int i = 0; i < ghColumns.Control.RowCount; i++)
      {
        DataGridViewRow Row = ghColumns.Control.Rows[i];
        bool Flag = DataTools.GetBool(Row.Cells[0].Value);
        if (Flag)
        {
          EFPGridProducerColumn Col = GetProducerColumn(Row);
          Codes.Add(Col.Name);
          Names.Add(Col.DisplayName);
        }
      }

      cbStartColumn.BeginUpdate();
      try
      {
        // Сначала - Items, затем - Codes
        cbStartColumn.Items.Clear();
        cbStartColumn.Items.AddRange(Names.ToArray());
        efpStartColumn.Codes = Codes.ToArray();
        if (Codes.Contains(CurrCode))
          efpStartColumn.SelectedCode = CurrCode;
        else
          efpStartColumn.SelectedCode = String.Empty;
      }
      finally
      {
        cbStartColumn.EndUpdate();
      }
    }

    #endregion

    #region Всплывающие подсказки

    /*
     * Поле Tag каждой строки таблицы grToolTips содержит имя элемента подсказки
     */

    private void WriteFormToolTips(EFPDataGridViewConfig config)
    {
      // Строки таблицы подсказок, соответствующие объявлениям в GridProducer
      DataGridViewRow[] OrdRows = new DataGridViewRow[_GridProducer.ToolTips.Count];

      // Сначала добавляем подсказки из текущей конфигурации
      cbCurrentCellToolTip.Checked = config.CurrentCellToolTip;

      if (ghToolTips != null)
      {
        ghToolTips.BeginUpdate();
        try
        {
          grToolTips.RowCount = 0;

          for (int i = 0; i < config.ToolTips.Count; i++)
          {
            string Name = config.ToolTips[i].ToolTipName;
            EFPGridProducerToolTip ToolTip = _GridProducer.ToolTips[Name];
            if (ToolTip == null)
              continue; // неизвестно, что

            grToolTips.Rows.Add(true, ToolTip.DisplayName);

            DataGridViewRow Row = grToolTips.Rows[grToolTips.RowCount - 1];
            Row.Tag = ToolTip;
            /*
            if (TheControlProvider.UI.DebugShowIds)
            {
              Row.Cells[2].Value = ToolTip.Name;
              DBxColumnList List = new DBxColumnList();
              ToolTip.GetColumnNames(List);
              Row.Cells[3].Value = String.Join(", ", List.ToArray());
            } */

            OrdRows[_GridProducer.ToolTips.IndexOf(ToolTip)] = Row;
          }

          // Теперь перебираем подсказки из GridProducer
          for (int i = 0; i < _GridProducer.ToolTips.Count; i++)
          {
            if (OrdRows[i] != null)
              continue; // Подсказка уже была добавлена
            EFPGridProducerToolTip ToolTip = _GridProducer.ToolTips[i];

            int NextIndex;
            if (i > 0)
              NextIndex = OrdRows[i - 1].Index + 1;
            else
              NextIndex = 0;

            if (NextIndex >= grToolTips.Rows.Count)
              grToolTips.Rows.Add();
            else
              grToolTips.Rows.Insert(NextIndex, 1);
            DataGridViewRow Row = grToolTips.Rows[NextIndex];
            OrdRows[i] = Row;
            Row.Tag = ToolTip;
            Row.Cells[0].Value = false;
            Row.Cells[1].Value = ToolTip.DisplayName;
            /*
            if (TheControlProvider.UI.DebugShowIds)
            {
              Row.Cells[2].Value = ToolTip.Name;
              DBxColumnList List = new DBxColumnList();
              ToolTip.GetColumnNames(List);
              Row.Cells[3].Value = String.Join(", ", List.ToArray());
            } */
          }
        }
        finally
        {
          ghToolTips.EndUpdate();
        }

        ghToolTips.CommandItems.DefaultManualOrderRows = OrdRows;

        panGrToolTips.Visible = _GridProducer.ToolTips.Count > 0; // 11.10.2016
      }
    }

    private bool ReadFormToolTips(EFPDataGridViewConfig config, out string errorText)
    {
      if (ghToolTips != null)
      {
        config.CurrentCellToolTip = cbCurrentCellToolTip.Checked;
        for (int i = 0; i < grToolTips.Rows.Count; i++)
        {
          DataGridViewRow Row = grToolTips.Rows[i];
          if (!DataTools.GetBool(Row.Cells[0].Value))
            continue;
          EFPGridProducerToolTip ToolTip = (EFPGridProducerToolTip)(Row.Tag);
          config.ToolTips.Add(ToolTip.Name);
        }
      }
      else
      {
        // Копируем подсказки обратно без изменений
        config.CurrentCellToolTip = _OrgConfig.CurrentCellToolTip;
        for (int i = 0; i < _OrgConfig.ToolTips.Count; i++)
          config.ToolTips.Add(_OrgConfig.ToolTips[i]);
      }

      errorText = null;
      return true;
    }

    #endregion

    #region Проверка формы и создание конфигурации

    private EFPGridProducerColumn GetProducerColumn(DataGridViewRow row)
    {
      return row.Tag as EFPGridProducerColumn;
    }

    #endregion

    #region IEFPGridProducerEditor Members

    /// <summary>
    /// Перенос из <paramref name="config"/> в управляющие элементы формы
    /// </summary>
    /// <param name="config"></param>
    public void WriteFormValues(EFPDataGridViewConfig config)
    {
      WriteFormColumns(config);
      edFrozenColumns.Value = config.FrozenColumns;
      InitStartColumnList();
      efpStartColumn.SelectedCode = config.StartColumnName;

      WriteFormToolTips(config);
    }

    /// <summary>
    /// Перенос из управляющих элементов формы в <paramref name="config"/>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке, если метод возвращает false</param>
    /// <returns></returns>
    public bool ReadFormValues(EFPDataGridViewConfig config, out string errorText)
    {
      if (!ReadFormColumns(config, out errorText))
        return false;

      config.FrozenColumns = (int)(edFrozenColumns.Value);
      if (config.FrozenColumns < 0 || config.FrozenColumns >= config.Columns.Count)
      {
        errorText = "Недопустимое число замороженных столбцов";
        return false;
      }

      config.StartColumnName = efpStartColumn.SelectedCode;

      if (!ReadFormToolTips(config, out errorText))
        return false;

      return true;
    }

    public void GetDefaultConfigs(out string[] defaultConfigCodes, out EFPDataGridViewConfig[] defaultConfigs)
    {
      string[] FixedNames = _GridProducer.GetNamedConfigNames();
      defaultConfigCodes = new string[FixedNames.Length + 1];
      defaultConfigs = new EFPDataGridViewConfig[FixedNames.Length + 1];
      defaultConfigCodes[0] = String.Empty;
      if (_GridProducer.DefaultConfig == null)
        defaultConfigs[0] = _GridProducer.CreateDefaultConfig();
      else
        defaultConfigs[0] = _GridProducer.DefaultConfig;

      for (int i = 0; i < FixedNames.Length; i++)
      {
        defaultConfigCodes[i + 1] = FixedNames[i];
        defaultConfigs[i + 1] = _GridProducer.GetNamedConfig(FixedNames[i]);
      }
    }

    #endregion
  }

  #region IEFPGridProducerEditor

  /// <summary>
  /// Редактор настройки табличного просмотра с настройкой индивидуальных столбцов
  /// </summary>
  public interface IEFPGridProducerEditor
  {
    /// <summary>
    /// Перенести значения из <paramref name="config"/> в форму редактора.
    /// Метод вызывается при открытии формы, а также при выборе пользователем конфигурации из списка истории
    /// </summary>
    /// <param name="config">Настройка, откуда должны быть извлечены данные</param>
    void WriteFormValues(EFPDataGridViewConfig config);

    /// <summary>
    /// Перенести значения из формы редактора в <paramref name="config"/>.
    /// При этом выполняется проверка формы
    /// Метод вызывается при нажатии кнопки "ОК"
    /// </summary>
    /// <param name="config">Настройка, откуда должны быть извлечены данные</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке, если результат вызова метода - false</param>
    /// <returns>true, если значения в форме правильные и успешно прочитаны. false, если выбранная настройка является некорректной</returns>
    bool ReadFormValues(EFPDataGridViewConfig config, out String errorText);

    /// <summary>
    /// Получить список вариантов настроек по умолчанию.
    /// Метод возвращает два массива одинаковой длины - коды и соответствующие им настройки.
    /// </summary>
    /// <param name="defaultConfigCodes">Сюда записываются коды</param>
    /// <param name="defaultConfigs">Сюда записываются настройки</param>
    void GetDefaultConfigs(out string[] defaultConfigCodes, out EFPDataGridViewConfig[] defaultConfigs);
  }

  #endregion
}