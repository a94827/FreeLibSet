// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Табличный просмотр для выбора строк и назначения столбцов
  /// Используется в диалогах или мастерах вставки текстовых значений из буфера обмена.
  /// Для отображения данных следует установить свойство Data
  /// </summary>
  public class EFPSelRCDataGridView : EFPDataGridView
  {
    #region Константы

    private const string NoneColText = "[ Нет ]";

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает провайдер табличного просмотра.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Табличный просмотр</param>
    public EFPSelRCDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер табличного просмотра.
    /// </summary>
    /// <param name="controlWithToolBar">Табличный просмотр и панель инструментов</param>
    public EFPSelRCDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      base.UseRowImages = true;
      Control.VirtualMode = true;
      if (!DesignMode)
      {
        Control.CellValueNeeded += new DataGridViewCellValueEventHandler(Control_CellValueNeeded);
        Control.CellValuePushed += new DataGridViewCellValueEventHandler(Control_CellValuePushed);
      }

      base.ReadOnly = true; // 11.01.2017
      base.CanView = false; // 11.01.2017

      base.UseRowImages = true; // 12.01.2022

      // 12.01.2022
      Columns.AddBool("RowFlag", false, String.Empty); // будет удален при установке SourceData
      base.MarkRowsColumnIndex = 0;

      _AutoSelect = true;
    }

    #endregion

    #region Свойство Data

    /// <summary>
    /// Отображаемые данные (основное свойство)
    /// </summary>
    public UISelRCGridData Data
    {
      get { return _Data; }
      set
      {
        if (_Data != null)
          _Data.Changed -= ValidateAndRepaint;

        _Data = value;

        InitRowsAndColumns();

        if (AutoSelect)
        {
          Data.SelRows.Init();
          Data.SelColumns.Init();
        }

        if (_Data != null)
          _Data.Changed += ValidateAndRepaint;

        if (_Data == null)
          TopLeftCellToolTipText = String.Empty;
        else
          TopLeftCellToolTipText = "Всего строк: " + _Data.RowCount.ToString() + ", столбцов: " + _Data.ColumnCount.ToString();

        ValidateAndRepaint(null, null);
      }
    }
    private UISelRCGridData _Data;

    private void InitRowsAndColumns()
    {
      Columns.Clear();
      if (_Data == null)
      {
        Control.RowCount = 0;
        return;
      }

      Columns.AddBool("RowFlag", false, String.Empty);
      Columns.LastAdded.GridColumn.ToolTipText = "Использование строки";
      Columns.LastAdded.GridColumn.DividerWidth = 1; // 12.01.2022

      string[] displayNames = new string[Data.AvailableColumns.Count + 1];
      displayNames[0] = NoneColText;
      for (int i = 0; i < Data.AvailableColumns.Count; i++)
        displayNames[i + 1] = Data.AvailableColumns[i].DisplayName;
      int dropDownWidth = CalcDropDownWidth(displayNames);

      for (int i = 0; i < Data.ColumnCount; i++)
      {
        DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
        col.Name = "Col" + (i + 1).ToString(); // 12.01.2022
        col.HeaderText = (i + 1).ToString();
        //Col.DropDownWidth = Math.Max(DropDownWidth, Col.Width);
        col.DropDownWidth = dropDownWidth; // расширение до столбца выполняется автоматически
        col.Items.AddRange(displayNames);
        Control.Columns.Add(col);

        int maxLen = 7; // нужно место для стрелочки выпадающего списка
        for (int j = 0; j < Data.RowCount; j++)
        {
          string s = DataTools.GetString(Data.SourceData[j, i]);
          maxLen = Math.Max(s.Length, maxLen);
        }
        maxLen = Math.Min(maxLen, 30);

        Columns[col].TextWidth = maxLen;
      }

      Control.RowCount = Data.RowCount + 1;

      base.FrozenColumns = 1;
      Control.Rows[0].Frozen = true;
      Control.Rows[0].DividerHeight = 1; // 12.01.2022
      base.MarkRowsColumnIndex = 0;

      for (int i = 0; i < Data.RowCount; i++)
      {
        Control.Rows[i + 1].HeaderCell.Value = i + 1;
        for (int j = 0; j < Data.ColumnCount; j++)
        {
          DataGridViewComboBoxCell Cell = (DataGridViewComboBoxCell)(base.Control[j + 1, i + 1]);

          Cell.Items.Add(Data.SourceData[i, j]);
          Cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
          Cell.Value = Data.SourceData[i, j];
        }
      }
    }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то после установки свойств AllColumns и SourceData,
    /// будут автоматически выбраны все непустые строки (свойство SelRows) и назначены подходящие столбцы (свойство SelColumns).
    /// Для ручной инициализации столбцов сбросьте свойство AutoSelect в false до присвоения SourceData.
    /// После присоединения матрицы данных установите отметки для нужных строк или вызовите SelRows.Init().
    /// Назначьте нужные столбцы в SelColumns. Чтобы затем автоматически назначить недостающие столбцы,
    /// можно использовать метод SelColumns.Init().
    /// </summary>
    public bool AutoSelect
    {
      get { return _AutoSelect; }
      set { _AutoSelect = value; }
    }
    private bool _AutoSelect;

    #endregion

    #region Обработчики таблицы

    private int CalcDropDownWidth(string[] displayNames)
    {
      int maxW = 0;
      Graphics gr = Control.CreateGraphics();
      try
      {
        for (int i = 0; i < displayNames.Length; i++)
        {
          int w = (int)(gr.MeasureString(displayNames[i], Control.Font).Width);
          maxW = Math.Max(maxW, w);
        }
      }
      finally
      {
        gr.Dispose();
      }
      maxW += SystemInformation.VerticalScrollBarWidth;
      maxW += 4; // для рамочек
      Screen scr = Screen.FromControl(Control);
      maxW = Math.Min(maxW, scr.WorkingArea.Width);
      return maxW;
    }

    void Control_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args)
    {
      if (Data == null)
        return;

      try
      {
        if (args.ColumnIndex == 0)
        {
          if (args.RowIndex > 0)
            args.Value = Data.SelRows[args.RowIndex - 1];
        }
        else
        {
          //DataGridViewComboBoxCell Cell = (DataGridViewComboBoxCell)(Control[args.ColumnIndex, args.RowIndex]);
          if (args.RowIndex > 0)
            args.Value = Data.SourceData[args.RowIndex - 1, args.ColumnIndex - 1];
          else
          {
            UISelRCColumn Col = Data.SelColumns[args.ColumnIndex - 1];
            if (Col == null)
              args.Value = NoneColText;
            else
              args.Value = Col.DisplayName;
          }
        }
      }
      catch
      {
      }
    }

    void Control_CellValuePushed(object sender, DataGridViewCellValueEventArgs args)
    {
      try
      {
        if (Data == null)
          throw new ArgumentNullException("Data==null");

        if (args.ColumnIndex == 0)
        {
          if (args.RowIndex == 0)
            return;
          Data.SelRows[args.RowIndex - 1] = DataTools.GetBool(args.Value);
          Control.InvalidateRow(args.RowIndex);
          base.Validate();
        }

        if (args.ColumnIndex > 0 && args.RowIndex == 0)
        {
          string displayName = DataTools.GetString(args.Value);
          if (displayName == NoneColText)
            Data.SelColumns[args.ColumnIndex - 1] = null;
          else
          {
            bool found = false;
            for (int i = 0; i < Data.AvailableColumns.Count; i++)
            {
              if (Data.AvailableColumns[i].DisplayName == displayName)
              {
                Data.SelColumns[args.ColumnIndex - 1] = Data.AvailableColumns[i];
                found = true;
                break;
              }
            }
            if (!found)
              throw new InvalidOperationException("Не найден столбец с названием \"" + displayName + "\"");
          }

          Control.InvalidateColumn(args.ColumnIndex);
          base.Validate();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "CellValuePushed");
      }
    }

    /// <summary>
    /// Получение списка ошибок и предупреждений для строки, если есть ячейки с неправильным форматированием
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      if (Data == null)
        return;

      if (args.RowIndex >= 1 && Data.SelRows[args.RowIndex - 1]) // серые ячейки делаем поштучно
      {
        for (int i = 0; i < Data.ColumnCount; i++)
        {
          if (Data.SelColumns[i] != null)
          {
            string errorText;
            UIValidateState res = Data.Validate(args.RowIndex - 1, i, out errorText);
            switch (res)
            {
              case UIValidateState.Error:
                args.AddRowError(Data.SelColumns[i].DisplayName + ": " + errorText, Columns[i + 1].Name);
                break;
              case UIValidateState.Warning:
                args.AddRowWarning(Data.SelColumns[i].DisplayName + ": " + errorText, Columns[i + 1].Name);
                break;
            }
          }
        }
      }
      base.OnGetRowAttributes(args);
    }

    /// <summary>
    /// Раскраска серых ячеек
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      if (Data == null)
        return;

      if (args.RowIndex == 0)
      {
        // Строка выбора типа столбца
        if (args.ColumnIndex == 0)
        {
          // Пустая ячейка
          args.ContentVisible = false;
          args.ReadOnly = true;
          args.ReadOnlyMessage = "Пустая ячейка";
          return;
        }

        if (!args.GetGridCell().Selected)
        {
          // Так странно себя ведет
          if (Data.SelColumns.IsRepeated(args.ColumnIndex - 1))
            args.ColorType = EFPDataGridViewColorType.Error;
          else if (Data.SelColumns[args.ColumnIndex - 1] == null)
            args.Grayed = true;
        }
      }
      else
      {
        // Строка таблицы
        if (args.ColumnIndex >= 1)
        {
          // Столбец данных
          if (Data.SelRows[args.RowIndex - 1])
          {
            if (Data.SelColumns[args.ColumnIndex - 1] == null)
              args.Grayed = true;
          }
          else
            args.Grayed = true;
          args.ReadOnly = true;
          args.ReadOnlyMessage = "Нельзя редактировать значения";
        }
        // Ячейка с флажком никогда не делается серой
      }

      base.OnGetCellAttributes(args);
    }

    #endregion

    #region Validate

    private void ValidateAndRepaint(object sender, EventArgs args)
    {
      Validate();
      Control.Invalidate();

      if (CommandItemsIfAssigned != null)
        CommandItems.SetEnabled();
    }

    /// <summary>
    /// Проверка корректности введенных данных управляющего элемента
    /// Проверка значений импортируемых ячеек не выполняется, т.к. она может быть сравнительно длительной
    /// и вызвать "подвисание" компьютера при большом размере таблицы SourceData
    /// </summary>
    protected override void OnValidate()
    {
      if (Data == null)
      {
        SetError("Нет данных для выбора");
        return;
      }

      if (Data.SelColumns.IsEmpty)
      {
        base.SetError("Не выбрано ни одного столбца");
        return;
      }

      if (Data.SelColumns.HasRepeats)
      {
        SetError("Столбец \"" + Data.SelColumns[Data.SelColumns.FirstRepeatedColumnIndex] + "\" выбран дважды");
        return;
      }

      // Проверяем наличие выбранных строк и корректность данных
      bool hasRows = false;
      for (int i = 0; i < Data.RowCount; i++)
      {
        if (Data.SelRows[i])
        {
          hasRows = true;

          for (int j = 0; j < Data.ColumnCount; j++)
          {
            if (Data.SelColumns[j] != null)
            {
              string errorText;
              if (Data.Validate(i, j, out errorText) == UIValidateState.Error)
              {
                // 12.01.2022
                // Переход на ошибочную ячейку не выполняем.
                // Например, пусть выбрана строка с ошибкой и установленным флажком.
                // Пользователь выключает флажок.
                // Фокус прыгает на другую строку с ошибкой.
                // Теперь можно использовать Ctrl+] и Ctrl+[ для поиска ошибок.
                //Control.CurrentCell = Control[j + 1, i + 1];

                base.SetError("Строка " + (i + 1).ToString() + ", столбец " + (j + 1).ToString() + " (" + Data.SelColumns[j].DisplayName + "). " +
                  errorText);
                return;
              }
            }
          }
        }
      }

      if (!hasRows)
      {
        this.CurrentColumnIndex = 0;
        //EFPApp.ShowTempMessage("Нет ни одной выбранной строки данных");
        base.SetError("Нет ни одной выбранной строки данных"); // 12.01.2022
        return;
      }

      base.OnValidate();
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Возвращает список команд локального меню
    /// </summary>
    public new EFPSelRCDataGridViewCommandItems CommandItems
    {
      get { return (EFPSelRCDataGridViewCommandItems)(base.CommandItems); }
    }

    /// <summary>
    /// Создает список команд локального меню
    /// </summary>
    /// <returns>Новый список команд</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPSelRCDataGridViewCommandItems(this);
    }


    #endregion
  }

  /// <summary>
  /// Добавляет команду "Очистить привязку столбцов" для табличного просмотра выбора строк и столбцов
  /// </summary>
  public class EFPSelRCDataGridViewCommandItems : EFPDataGridViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPSelRCDataGridViewCommandItems(EFPSelRCDataGridView controlProvider)
      : base(controlProvider)
    {
      ciClearColumns = new EFPCommandItem("Edit", "ClearSelectedColumns");
      ciClearColumns.MenuText = "Очистить привязку столбцов";
      ciClearColumns.ImageKey = "No";
      ciClearColumns.ShortCut = Keys.F8;
      ciClearColumns.GroupBegin = true;
      ciClearColumns.Click += ciClearColumns_Click;
      base.Add(ciClearColumns);

      ciInitColumns = new EFPCommandItem("Edit", "InitSelectedColumns");
      ciInitColumns.MenuText = "Установить привязку столбцов";
      ciInitColumns.ImageKey = "Execute";
      ciInitColumns.ShortCut = Keys.F9;
      ciInitColumns.GroupEnd = true;
      ciInitColumns.Click += ciInitColumns_Click;
      base.Add(ciInitColumns);

      SetEnabled();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPSelRCDataGridView ControlProvider { get { return (EFPSelRCDataGridView)(base.ControlProvider); } }

    #endregion

    #region Команда очистки

    EFPCommandItem ciClearColumns;

    void ciClearColumns_Click(object sender, EventArgs args)
    {
      if (ControlProvider.Data == null)
        return;
      ControlProvider.Data.SelColumns.Clear();
    }

    #endregion

    #region Команда заполнения

    EFPCommandItem ciInitColumns;

    void ciInitColumns_Click(object sender, EventArgs args)
    {
      if (ControlProvider.Data == null)
        return;

      string oldCodes = ControlProvider.Data.SelColumns.AsString;
      ControlProvider.Data.SelColumns.Init();
      if (ControlProvider.Data.SelColumns.AsString == oldCodes)
        EFPApp.ShowTempMessage("Не удалось назначить другие столбцы");
    }

    #endregion

    #region Установка состояния команд

    internal void SetEnabled()
    {
      if (ControlProvider.Data == null)
      {
        ciClearColumns.Enabled = false;
        ciInitColumns.Enabled = false;
      }
      else
      {
        ciClearColumns.Enabled = !ControlProvider.Data.SelColumns.IsEmpty;
        if (ControlProvider.Data.SelRows.IsEmpty || ControlProvider.Data.SelColumns.IsFull)
          ciInitColumns.Enabled = false;
        else
          ciInitColumns.Enabled = ControlProvider.Data.SelColumns.UnassignedCodes.Length > 0;
      }
    }

    #endregion
  }

  /// <summary>
  /// Шаг мастера для привязки строк и столбцов
  /// Используется в мастерах вставки в справочники из буфера обмена
  /// </summary>
  public class WizardStepSelRC : WizardStepWithDataGridView
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера
    /// </summary>
    public WizardStepSelRC()
      : base(false)
    {
      DoInit(new EFPSelRCDataGridView(base.BaseProvider, new DataGridView()));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPSelRCDataGridView TheControlProvider
    {
      get { return (EFPSelRCDataGridView)(base.TheControlProvider); }
    }

    #endregion
  }
}
