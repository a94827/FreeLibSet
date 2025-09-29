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
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Управляющие элементы, встраиваемые в редактор настройки табличного просмотра.
  /// Сама форма не используется.
  /// </summary>
  internal partial class EFPGridProducerEditor : Form, IEFPGridProducerEditor, IEFPConfigParamSetAuxTextHandler
  {
    #region Конструктор

    public EFPGridProducerEditor(EFPGridProducer gridProducer, IEFPDataView controlProvider, EFPBaseProvider baseProvider)
    {
      InitializeComponent();
      this._GridProducer = gridProducer;
      this._TheControlProvider = controlProvider;

      _IsTreeView = controlProvider.Control is FreeLibSet.Controls.TreeViewAdv;

      //efpForm.AddFormCheck(new EFPValidatingEventHandler(ValidateForm));
      TheTabControl.ImageList = EFPApp.MainImages.ImageList;
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
      ghColumns.CellInfoNeeded += new EFPDataGridViewCellInfoEventHandler(ghColumns_CellInfoNeeded);
      ghColumns.ReadOnly = true;
      ghColumns.CanInsert = false; // !!!
      ghColumns.CanDelete = false;
      ghColumns.CanView = false;
      ghColumns.Columns[1].CanIncSearch = true;
      grColumns.MultiSelect = false;
      grColumns.ReadOnly = false;
      //InitColumnsGrid();
      ghColumns.ManualOrderRows = true;
      ghColumns.MarkRowsColumnIndex = 0;
      ghColumns.CellFinished += new EFPDataGridViewCellFinishedEventHandler(ghColumns_CellFinished);
      ghColumns.MenuOutItems.Clear();
      ghColumns.ToolBarPanel = panSpbColumns;

      #endregion

      // Так нельзя. В просмотре могут быть заморожены посторонние столбцы,
      // например, картинка
      //edFrozenColumns.ValueEx = TheHandler.FrozenColumns;

      efpFrozenColumns = new EFPIntEditBox(baseProvider, edFrozenColumns);


      // Список выбора активного столбца
      //StartColumnName = ControlProvider.CurrentConfig.StartColumnName;
      //InitCBStartCol();

      efpStartColumn = new EFPListComboBox(baseProvider, cbStartColumn);
      efpStartColumn.ToolTipText = Res.EFPGridProducerEditor_ToolTip_StartColumn;

      if (_IsTreeView)
      {
        efpFrozenColumns.Visible = false;
        efpStartColumn.Visible = false;
      }

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
        ghToolTips.ManualOrderRows = true;
        ghToolTips.MarkRowsColumnIndex = 0;
        ghToolTips.MenuOutItems.Clear();
        ghToolTips.ToolBarPanel = panSpbToolTips;
      }
      else
      {
        tpToolTips.Controls.Clear();
        Label lbl = new Label();
        lbl.Dock = DockStyle.Fill;
        lbl.Text = Res.EFPGridProducerEditor_Msg_ToolTipsOff;
        lbl.TextAlign = ContentAlignment.MiddleCenter;
        lbl.BackColor = SystemColors.Info;
        lbl.ForeColor = SystemColors.InfoText;
        tpToolTips.Controls.Add(lbl);
      }

      #endregion
    }

    /// <summary>
    /// Если true, то не поддерживаются замороженные столбцы и активируемый столбец
    /// </summary>
    private bool _IsTreeView;

    #endregion

    #region Поля

    /// <summary>
    /// Копия конфигурации до начала редактирования
    /// </summary>
    private EFPDataViewConfig _OrgConfig;

    private EFPDataGridView ghColumns, ghToolTips;

    private EFPGridProducer _GridProducer;
    private IEFPDataView _TheControlProvider;

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
    /// <param name="config">Конфигурация, откуда берутся размеры столбцов</param>
    private void WriteFormColumns(EFPDataViewConfig config)
    {
      // Строки таблицы столбцов, соответствующие объявлениям в GridProducer
      DataGridViewRow[] ordRows = new DataGridViewRow[_GridProducer.Columns.Count];

      int currentColumnIndex = _TheControlProvider.CurrentColumnIndex; // чтобы не вычислять в цикле
      DataGridViewRow startRow1 = null; // строка, соответствующая активному столбцу в просмотре
      DataGridViewRow startRow2 = null; // первая строка, содержащая "галочку"

      ghColumns.BeginUpdate();
      try
      {
        grColumns.RowCount = 0;

        // Сначала добавляем строки из настройки
        //int cntFrozen = 0;
        for (int i = 0; i < config.Columns.Count; i++)
        {
          EFPDataViewConfigColumn column = config.Columns[i];

          EFPGridProducerColumn columnProducer = _GridProducer.Columns[column.ColumnName];
          if (columnProducer == null)
            continue;


          int colWidth = column.Width;
          if (colWidth == 0)
            colWidth = columnProducer.GetWidth(_TheControlProvider.Measures); // 30.03.2017

          grColumns.Rows.Add(true,
            columnProducer.DisplayName,
            colWidth,
            column.FillMode ?
              (object)(int)(column.FillWeight) : null);
          int columnProducerIndex = _GridProducer.Columns.IndexOf(column.ColumnName);
          ordRows[columnProducerIndex] = grColumns.Rows[grColumns.Rows.Count - 1];

          DataGridViewRow row = grColumns.Rows[grColumns.RowCount - 1];
          row.Tag = _GridProducer.Columns[columnProducerIndex];
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
          if (currentColumnIndex == i)
            startRow1 = row;
          if (startRow2 == null) // Исправлено 05.01.2021
            startRow2 = row;
        }

        // Теперь перебираем столбцы из GridProducer
        for (int i = 0; i < _GridProducer.Columns.Count; i++)
        {
          EFPGridProducerColumn column = _GridProducer.Columns[i];
          if (ordRows[i] != null)
            continue; // Столбец уже был добавлен

          EFPDataViewConfigColumn columnConfig = null;
          //        if (TheHandler.CurrentGridConfig != null)
          //          ColumnConfig = TheHandler.CurrentGridConfig.Columns[Column.ColumnName];

          int w = 0;
          bool fillMode = false;
          int fillWeight = 100;
          if (columnConfig != null)
          {
            w = columnConfig.Width;
            fillMode = columnConfig.FillMode;
            fillWeight = columnConfig.FillWeight;
          }
          if (w == 0)
            //w = TheControlProvider.Measures.GetTextColumnWidth(Column.TextWidth);
            w = column.GetWidth(_TheControlProvider.Measures);

          int nextIndex;
          if (i > 0)
            nextIndex = ordRows[i - 1].Index + 1;
          else
            nextIndex = 0;

          if (nextIndex >= grColumns.Rows.Count)
            grColumns.Rows.Add();
          else
            grColumns.Rows.Insert(nextIndex, 1);
          DataGridViewRow row = grColumns.Rows[nextIndex];
          ordRows[i] = row;
          row.Tag = column;
          row.Cells[0].Value = false;
          row.Cells[1].Value = column.DisplayName;
          row.Cells[2].Value = w;
          if (fillMode)
            row.Cells[3].Value = fillWeight;
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
      ghColumns.DefaultManualOrderRows = ordRows; // востановление порядка по умолчанию

      // Активируем строку, соответствующую текущему столбцу в просмотре или с первой отметкой
      if (startRow1 != null)
        ghColumns.CurrentGridRow = startRow1;
      else
      {
        if (startRow2 != null)
          ghColumns.CurrentGridRow = startRow2;
      }
    }

    /// <summary>
    /// Заполняет конфигурацию из текущих значений в табличке столбцов
    /// </summary>
    /// <param name="config">Заполняемая конфигурация просмотра</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>true, если введенные в табличке значения не содержат ошибок</returns>
    private bool ReadFormColumns(EFPDataViewConfig config, out string errorText)
    {
      grColumns.EndEdit();

      for (int i = 0; i < grColumns.Rows.Count; i++)
      {
        DataGridViewRow row = grColumns.Rows[i];
        if (!DataTools.GetBoolean(row.Cells[0].Value))
          continue;
        EFPGridProducerColumn colDef = GetProducerColumn(row);
        EFPDataViewConfigColumn columnCfg = new EFPDataViewConfigColumn(colDef.Name);
        columnCfg.Width = DataTools.GetInt32(row.Cells[2].Value);
        int percent = DataTools.GetInt32(row.Cells[3].Value);
        if (percent > 0)
        {
          columnCfg.FillMode = true;
          columnCfg.FillWeight = percent;
        }
        config.Columns.Add(columnCfg);
      }
      if (config.Columns.Count == 0)
      {
        errorText = Res.EFPGridProducerEditor_Err_NoColumnSelected;
        ghColumns.SetFocus();
        return false;
      }

      errorText = null;
      return true;
    }

    void ghColumns_CellFinished(object sender, EFPDataGridViewCellFinishedEventArgs args)
    {
      // 25.03.2024
      // Синхронизируем ширину столбцов с одинаковым SizeGroup
      if (args.ColumnIndex >= 2) // ширина в пикселях или процент
        SyncColumnSizes(args.RowIndex);

      InitStartColumnList();

      ghColumns.Control.InvalidateRow(args.RowIndex);
    }

    private void SyncColumnSizes(int rowIndex)
    {
      DataGridViewRow row = ghColumns.Control.Rows[rowIndex];
      EFPGridProducerColumn colDef = GetProducerColumn(row);
      if (!String.IsNullOrEmpty(colDef.SizeGroup))
      {
        for (int i = 0; i < ghColumns.Control.RowCount; i++)
        {
          if (i == rowIndex)
            continue;
          DataGridViewRow thisRow = ghColumns.Control.Rows[i];
          EFPGridProducerColumn thisColDef = GetProducerColumn(thisRow);
          if (thisColDef.SizeGroup == colDef.SizeGroup)
          {
            thisRow.Cells[2].Value = row.Cells[2].Value;
            thisRow.Cells[3].Value = row.Cells[3].Value;
          }
        }
      }
    }

    void ghColumns_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      switch (args.ColumnIndex)
      {
        case 2: // ширина
        case 3: // процент
          DataGridViewRow row = grColumns.Rows[args.RowIndex];
          EFPGridProducerColumn colProd = GetProducerColumn(row);
          if (colProd == null)
            return;

          if (!DataTools.GetBoolean(row.Cells[0].Value))
          {
            args.Grayed = true;
            args.ReadOnly = true;
            args.ReadOnlyMessage = Res.EFPGridProducerEditor_Err_WidthForHiddenColumn;
          }
          else if (!colProd.Resizable)
          {
            args.Grayed = true;
            args.ReadOnly = true;
            args.ReadOnlyMessage = String.Format(Res.EFPGridProducerEditor_Err_NotSizeableColumn, colProd.DisplayName);
          }
          break;
      }
    }

    #endregion

    #region Замороженные столбцы

    EFPIntEditBox efpFrozenColumns;

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
          bool Flag = DataTools.GetBoolean(Row.Cells[0].Value);
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

    private void cbActivateCol_Enter(object sender, EventArgs args)
    {
      // При входе в элемент перестраиваем список
      InitCBStartCol();
    }

    private void cbActivateCol_SelectedIndexChanged(object sender, EventArgs args)
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
      List<string> codes = new List<string>();
      List<string> names = new List<string>();

      codes.Add(String.Empty);
      //Names.Add("[ Авто ]");
      names.Add(Res.EFPGridProducerEditor_Msg_NoStartColumn); // 19.05.2021

      string currCode = efpStartColumn.SelectedCode;

      for (int i = 0; i < ghColumns.Control.RowCount; i++)
      {
        DataGridViewRow row = ghColumns.Control.Rows[i];
        bool flag = DataTools.GetBoolean(row.Cells[0].Value);
        if (flag)
        {
          EFPGridProducerColumn col = GetProducerColumn(row);
          codes.Add(col.Name);
          names.Add(col.DisplayName);
        }
      }

      cbStartColumn.BeginUpdate();
      try
      {
        // Сначала - Items, затем - Codes
        cbStartColumn.Items.Clear();
        cbStartColumn.Items.AddRange(names.ToArray());
        efpStartColumn.Codes = codes.ToArray();
        if (codes.Contains(currCode))
          efpStartColumn.SelectedCode = currCode;
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

    private void WriteFormToolTips(EFPDataViewConfig config)
    {
      // Строки таблицы подсказок, соответствующие объявлениям в GridProducer
      DataGridViewRow[] ordRows = new DataGridViewRow[_GridProducer.ToolTips.Count];

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
            string name = config.ToolTips[i].ToolTipName;
            EFPGridProducerToolTip toolTip = _GridProducer.ToolTips[name];
            if (toolTip == null)
              continue; // неизвестно, что

            grToolTips.Rows.Add(true, toolTip.DisplayName);

            DataGridViewRow row = grToolTips.Rows[grToolTips.RowCount - 1];
            row.Tag = toolTip;
            /*
            if (TheControlProvider.UI.DebugShowIds)
            {
              Row.Cells[2].Value = ToolTip.Name;
              DBxColumnList List = new DBxColumnList();
              ToolTip.GetColumnNames(List);
              Row.Cells[3].Value = String.Join(", ", List.ToArray());
            } */

            ordRows[_GridProducer.ToolTips.IndexOf(toolTip)] = row;
          }

          // Теперь перебираем подсказки из GridProducer
          for (int i = 0; i < _GridProducer.ToolTips.Count; i++)
          {
            if (ordRows[i] != null)
              continue; // Подсказка уже была добавлена
            EFPGridProducerToolTip toolTip = _GridProducer.ToolTips[i];

            int nextIndex;
            if (i > 0)
              nextIndex = ordRows[i - 1].Index + 1;
            else
              nextIndex = 0;

            if (nextIndex >= grToolTips.Rows.Count)
              grToolTips.Rows.Add();
            else
              grToolTips.Rows.Insert(nextIndex, 1);
            DataGridViewRow row = grToolTips.Rows[nextIndex];
            ordRows[i] = row;
            row.Tag = toolTip;
            row.Cells[0].Value = false;
            row.Cells[1].Value = toolTip.DisplayName;
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

        ghToolTips.DefaultManualOrderRows = ordRows;

        panGrToolTips.Visible = _GridProducer.ToolTips.Count > 0; // 11.10.2016
      }
    }

    private bool ReadFormToolTips(EFPDataViewConfig config, out string errorText)
    {
      if (ghToolTips != null)
      {
        config.CurrentCellToolTip = cbCurrentCellToolTip.Checked;
        for (int i = 0; i < grToolTips.Rows.Count; i++)
        {
          DataGridViewRow row = grToolTips.Rows[i];
          if (!DataTools.GetBoolean(row.Cells[0].Value))
            continue;
          EFPGridProducerToolTip toolTip = (EFPGridProducerToolTip)(row.Tag);
          config.ToolTips.Add(toolTip.Name);
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
    public void WriteFormValues(EFPDataViewConfig config)
    {
      WriteFormColumns(config);

      if (!_IsTreeView)
      {
        efpFrozenColumns.Value = config.FrozenColumns;
        InitStartColumnList();
        efpStartColumn.SelectedCode = config.StartColumnName;
      }

      WriteFormToolTips(config);
    }

    /// <summary>
    /// Перенос из управляющих элементов формы в <paramref name="config"/>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке, если метод возвращает false</param>
    /// <returns></returns>
    public bool ReadFormValues(EFPDataViewConfig config, out string errorText)
    {
      if (!ReadFormColumns(config, out errorText))
        return false;

      if (!_IsTreeView)
      {
        config.FrozenColumns = efpFrozenColumns.Value;
        if (config.FrozenColumns < 0 || config.FrozenColumns >= config.Columns.Count)
        {
          errorText = Res.EFPGridProducerEditor_Err_WrongFrozenColumns;
          return false;
        }

        config.StartColumnName = efpStartColumn.SelectedCode;
      }

      if (!ReadFormToolTips(config, out errorText))
        return false;

      return true;
    }

    public void GetDefaultConfigs(out string[] defaultConfigCodes, out EFPDataViewConfig[] defaultConfigs)
    {
      string[] fixedNames = _GridProducer.GetNamedConfigNames();
      defaultConfigCodes = new string[fixedNames.Length + 1];
      defaultConfigs = new EFPDataViewConfig[fixedNames.Length + 1];
      defaultConfigCodes[0] = String.Empty;
      if (_GridProducer.DefaultConfig == null)
        defaultConfigs[0] = _GridProducer.CreateDefaultConfig();
      else
        defaultConfigs[0] = _GridProducer.DefaultConfig;

      for (int i = 0; i < fixedNames.Length; i++)
      {
        defaultConfigCodes[i + 1] = fixedNames[i];
        defaultConfigs[i + 1] = _GridProducer.GetNamedConfig(fixedNames[i]);
      }
    }

    #endregion

    #region IEFPConfigParamSetAuxTextHandler

    public void BeginGetAuxText()
    {
    }

    public string GetAuxText(CfgPart cfg)
    {
      EFPDataViewConfig config = new EFPDataViewConfig();
      config.ReadConfig(cfg);
      StringBuilder sb = new StringBuilder();
      foreach (EFPDataViewConfigColumn cfgCol in config.Columns)
      {
        EFPGridProducerColumn prodCol = _GridProducer.Columns[cfgCol.ColumnName];
        if (prodCol != null)
        {
          if (sb.Length > 0)
            sb.Append(", ");
          sb.Append(prodCol.DisplayName);
        }
      }
      return sb.ToString();
    }

    public void EndGetAuxText()
    {
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
    void WriteFormValues(EFPDataViewConfig config);

    /// <summary>
    /// Перенести значения из формы редактора в <paramref name="config"/>.
    /// При этом выполняется проверка формы
    /// Метод вызывается при нажатии кнопки "ОК"
    /// </summary>
    /// <param name="config">Настройка, откуда должны быть извлечены данные</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке, если результат вызова метода - false</param>
    /// <returns>true, если значения в форме правильные и успешно прочитаны. false, если выбранная настройка является некорректной</returns>
    bool ReadFormValues(EFPDataViewConfig config, out String errorText);

    /// <summary>
    /// Получить список вариантов настроек по умолчанию.
    /// Метод возвращает два массива одинаковой длины - коды и соответствующие им настройки.
    /// </summary>
    /// <param name="defaultConfigCodes">Сюда записываются коды</param>
    /// <param name="defaultConfigs">Сюда записываются настройки</param>
    void GetDefaultConfigs(out string[] defaultConfigCodes, out EFPDataViewConfig[] defaultConfigs);
  }

  #endregion
}
