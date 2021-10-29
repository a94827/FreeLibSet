using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер табличного просмотра, предназначенного для редактирования данных DataTable "по месту".
  /// Расширяет EFPDataGridView, в основном, для поддержки операций вставки из буфера обмена с автоматическим
  /// добавлением строк.
  /// Инициализация столбцов выполняется при инициализации свойства DataGridView.DataSource ссылкой на DataTable.
  /// Свойство FixedRows позволяет работать с таблицами произвольного размера или с фиксированным числом строк.
  /// В режиме 
  /// </summary>
  public class EFPInputDataGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к DataGridView
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPInputDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элмент и панель инструментов</param>
    public EFPInputDataGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      Control.AutoGenerateColumns = false; // Сами добавляем
      Control.ReadOnly = false;
      Control.MultiSelect = true;
      Control.AllowUserToOrderColumns = false;
      Control.DataSourceChanged += new EventHandler(Control_DataSourceChanged);

      CanInsertCopy = false;
      CanView = false;
      CommandItems.ClipboardInToolBar = true;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Если свойство установлено в true, то пользователь не может добавлять или удалять строки.
    /// Если false (по умолчанию), то можно добавлять строки
    /// </summary>
    public bool FixedRows
    {
      get { return !Control.AllowUserToAddRows; }
      set
      {
        Control.AllowUserToAddRows = !value;
        CanInsert = !value;
        CanDelete = !value;
      }
    }

    #endregion

    #region Обработчики просмотра

    void Control_DataSourceChanged(object sender, EventArgs args)
    {
      if (Columns.Count > 0)
        Columns.Clear();
      DataTable table = Control.DataSource as DataTable;
      if (table == null)
        return;
      for (int i = 0; i < table.Columns.Count; i++)
      {
        string colName = table.Columns[i].ColumnName;
        string title = table.Columns[i].Caption;
        string format = DataTools.GetString(table.Columns[i].ExtendedProperties["Format"]);
        if (String.IsNullOrEmpty(title))
          title = colName;
        int TextWidth = DataTools.GetInt(table.Columns[i].ExtendedProperties["TextWidth"]);
        int MinTextWidth = DataTools.GetInt(table.Columns[i].ExtendedProperties["MinTextWidth"]);
        int FillWeight = DataTools.GetInt(table.Columns[i].ExtendedProperties["FillWeight"]);
        string Align = DataTools.GetString(table.Columns[i].ExtendedProperties["Align"]);

        if (table.Columns[i].DataType == typeof(Boolean))
          Columns.AddBool(colName, true, title);
        else if (table.Columns[i].DataType == typeof(DateTime))
        {
          if (String.IsNullOrEmpty(format))
            Columns.AddDate(colName, true, title);
          else
          {
            Columns.AddText(colName, true, title);
            Columns.LastAdded.TextAlign = HorizontalAlignment.Center;
          }
        }
        else
          Columns.AddText(colName, true, title);

        if (TextWidth > 0)
          Columns.LastAdded.TextWidth = TextWidth;
        if (FillWeight > 0)
        {
          Columns.LastAdded.GridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          Columns.LastAdded.GridColumn.FillWeight = FillWeight;
        }
        switch (Align.ToUpperInvariant())
        {
          case "LEFT": Columns.LastAdded.TextAlign = HorizontalAlignment.Left; break;
          case "CENTER": Columns.LastAdded.TextAlign = HorizontalAlignment.Center; break;
          case "RIGHT": Columns.LastAdded.TextAlign = HorizontalAlignment.Right; break;
        }
        if (!String.IsNullOrEmpty(format))
          Columns.LastAdded.GridColumn.DefaultCellStyle.Format = format;
      }

      DisableOrdering();
    }


    #endregion

    #region Редактирование

    /// <summary>
    /// Реализует добавление и удаление строк
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected override bool OnEditData(EventArgs args)
    {
      DataTable tbl = SourceAsDataTable;
      int[] rowIndices;

      switch (State)
      {
        case EFPDataGridViewState.Insert:
          if (ReadOnly || FixedRows)
            return true;
          // Добавляем перед текущей строкой столько строк, сколько выбрано
          int nAdded = SelectedRowCount;
          if (nAdded == 0)
            nAdded = 1;
          int index = CurrentRowIndex;
          if (index < 0)
            index = 0;
          rowIndices = new int[nAdded];
          for (int i = 0; i < nAdded; i++)
          {
            tbl.Rows.InsertAt(tbl.NewRow(), index + i);
            rowIndices[i] = index + i;
          }
          SelectedRowIndices = rowIndices;
          return true;

        case EFPDataGridViewState.Delete:
          if (ReadOnly || FixedRows)
            return true;
          rowIndices = SelectedRowIndices;
          Array.Sort<int>(rowIndices);
          for (int i = rowIndices.Length - 1; i >= 0; i--)
          {
            if (rowIndices[i] < tbl.Rows.Count) // последняя строка может быть виртуальной
              tbl.Rows.RemoveAt(rowIndices[i]);
          }
          return true;
      }
      return base.OnEditData(args);
    }

    #endregion

    #region Вставка текста из буфера обмена

    /// <summary>
    /// Можно добавлять строки при вставке, если FixedRows=false.
    /// </summary>
    protected override bool AutoAddRowsAllowed { get { return !FixedRows; } }

    /// <summary>
    /// Добавляет недостающие строки при вставке из буфера обмена
    /// </summary>
    /// <param name="addCount">Количество строк, которые нужно добавить</param>
    protected override void AddInsuficientRows(int addCount)
    {
      if (FixedRows)
        throw new InvalidOperationException("FixedRows=true");

      DataTable tbl = SourceAsDataTable;
      for (int i = 0; i < addCount; i++)
        tbl.Rows.Add();
    }

    #endregion

    #region Сжатие таблицы при закрытии формы

    /// <summary>
    /// При закрытии формы выполняет сжатие таблицы данных
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();

      if (BaseProvider.ValidateReason == EFPFormValidateReason.Closing || BaseProvider.ValidateReason == EFPFormValidateReason.ValidateForm)
      {
        if ((!Control.ReadOnly) && (!FixedRows))
          CompactTable();

        if (!Control.ReadOnly)
          SourceAsDataTable.AcceptChanges();
      }
    }

    private void CompactTable()
    {
      DataTable tbl = SourceAsDataTable;
      for (int i = tbl.Rows.Count - 1; i >= 0; i--)
      {
        DataRow row = tbl.Rows[i];
        bool isRowEmpty = true;
        for (int j = 0; j < tbl.Columns.Count; j++)
        {
          if (!String.IsNullOrEmpty(tbl.Columns[j].Expression))
            continue;
          if (row.IsNull(j))
            continue;
          if (tbl.Columns[j].DataType == typeof(string))
          {
            if (row[j].ToString().Trim().Length == 0)
              continue;
          }

          isRowEmpty = false;
          break;
        }
        if (isRowEmpty)
          tbl.Rows.RemoveAt(i);
      }
    }

    #endregion
  }

  /// <summary>
  /// Класс для установки свойств DataColumn.ExtendedProperties для табличного просмотра EFPInputGridView и
  /// диалога InputGridDataDialog.
  /// Прикладной код может создавать объект для таблицы, которая будет присоединена к EFPInputGridView.
  /// Класс InputGridDataDialog имеет свойство Columns для работы со столбцами.
  /// </summary>
  public class EFPInputDataGridViewColumnProperties : FreeLibSet.RI.InputGridDataColumnProperties
  {
    // Класс переопределяет методы для типа данных HorizontalAlignment

    #region Конструкторы

    /// <summary>
    /// Создает объект, присоединенный к таблице, разрешающий изменение свойста
    /// </summary>
    /// <param name="table">Таблица, свойствами столбцов которой нужно управлять</param>
    public EFPInputDataGridViewColumnProperties(DataTable table)
      : this(table, false)
    {
    }

    /// <summary>
    /// Создает объект, присоединенный к таблице
    /// </summary>
    /// <param name="table">Таблица, свойствами столбцов которой нужно управлять</param>
    /// <param name="isReadOnly">Если true, то объект позволит только читать свойства, но не устанавливать их</param>
    public EFPInputDataGridViewColumnProperties(DataTable table, bool isReadOnly)
      :base(table, isReadOnly)
    {
    }

    #endregion

    #region Align

    /// <summary>
    /// Установить горизонтальное выравнивание для столбца.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.</param>
    /// <param name="value">Устанавливаемое значение</param>
    public /*new */void SetAlign(string columnName, HorizontalAlignment value)
    {
      base.SetAlign(columnName, (FreeLibSet.RI.HorizontalAlignment)(int)value);
    }

    /// <summary>
    /// Установить горизонтальное выравнивание для столбца.
    /// На момент вызова в таблице Table должен быть добавлен столбец, для которого устанавливается значение
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null,
    /// то значение будет применено к последнему добавленному столбцу таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <param name="value">Устанавливаемое значение</param>
    public /*new */void SetAlign(DataColumn column, HorizontalAlignment value)
    {
      base.SetAlign(column, (FreeLibSet.RI.HorizontalAlignment)(int)value);
    }

    /// <summary>
    /// Получить горизонтальное выравнивание для столбца.
    /// Если значение не было установлено в явном виде, будет возвращено значение по умолчанию.
    /// </summary>
    /// <param name="columnName">Имя столбца DataColumn.ColumnName. Если задана пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.</param>
    /// <returns>Установленное значение</returns>
    public new HorizontalAlignment GetAlign(string columnName)
    {
      return (HorizontalAlignment)(int)(base.GetAlign(columnName));
    }

    /// <summary>
    /// Получить горизонтальное выравнивание для столбца.
    /// Если значение не было установлено в явном виде, будет возвращено значение по умолчанию.
    /// </summary>
    /// <param name="column">Столбец таблицы. Если задано null или пустая строка,
    /// то будет возвращено значение для последнего добавленного столбца таблицы Table.
    /// Удобнее использовать перегрузку, принимающую пустую строку, чтобы не задавать явное приведение типа "(DataColumn)null".
    /// </param>
    /// <returns>Установленное значение</returns>
    public new HorizontalAlignment GetAlign(DataColumn column)
    {
      return (HorizontalAlignment)(int)(base.GetAlign(column));
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода табличных данных.
  /// </summary>
  public class InputGridDataDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект значениями по умолчанию.
    /// Перед выводом диалога должно быть установлено свойство DataSource
    /// </summary>
    public InputGridDataDialog()
    {
      Title = "Таблица";
      ImageKey = "Table";
      _Table = new DataTable();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - редактируемая таблица данных.
    /// Список столбцов должен быть заполнен перед показом диалога или свойство должно быть установлено.
    /// По умолчанию содержит ссылку на пустую таблицу
    /// </summary>
    public DataTable Table
    {
      get { return _Table; }
      set 
      { 
        _Table = value;
        _Columns = null;
      }
    }
    private DataTable _Table;

    /// <summary>
    /// Доступ к расширенным свойствам столбцов
    /// </summary>
    public EFPInputDataGridViewColumnProperties Columns
    {
      get
      {
        if (_Columns == null && _Table != null)
          _Columns = new EFPInputDataGridViewColumnProperties(_Table);
        return _Columns;
      }
    }
    private EFPInputDataGridViewColumnProperties _Columns;

    /// <summary>
    /// Фиксированные строки.
    /// Если false (по умолчанию), то пользователь может добавлять и удалять строки в таблицу.
    /// Если true, то пользователь может только редактировать существующие строки в таблице.
    /// </summary>
    public bool FixedRows
    {
      get { return _FixedRows; }
      set { _FixedRows = value; }
    }
    private bool _FixedRows;

    /// <summary>
    /// Если true, то просмотр будет предназначен только для просмотра, но не для редактирования данных.
    /// По умолчанию - false.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Информационный текст, выводимый в нижней части диалога
    /// </summary>
    public string InfoText
    {
      get
      {
        if (_InfoText == null)
        {
          if (ReadOnly || FixedRows)
            return String.Empty;
          else
            return "При вставке из буфера обмена недостающие строки будут добавлены автоматически";
        }
        else
          return _InfoText;
      }
      set { _InfoText = value; }
    }
    private string _InfoText;

    #endregion

    #region Показ диалога

    /// <summary>
    /// Показывает блок диалога с таблицей
    /// </summary>
    /// <returns>Результат выполнения диалога</returns>
    public override DialogResult ShowDialog()
    {
      if (_Table == null)
      {
        EFPApp.ErrorMessageBox("Данные не присоединены", Title);
        return DialogResult.Cancel;
      }

      DialogResult res;

      using (OKCancelGridForm form = new OKCancelGridForm(!String.IsNullOrEmpty(Prompt)))
      {
        InitFormTitle(form);
        form.FormProvider.HelpContext = HelpContext;
        if (!String.IsNullOrEmpty(Prompt))
          form.GroupBox.Text = Prompt;
        if (ReadOnly)
          WinFormsTools.OkCancelFormToOkOnly(form);

        EFPInputDataGridView efpGrid = new EFPInputDataGridView(form.ControlWithToolBar);
        efpGrid.Control.DataSource = Table;
        efpGrid.ReadOnly = ReadOnly;
        efpGrid.Control.ReadOnly = ReadOnly;
        efpGrid.FixedRows = FixedRows || ReadOnly;

        if (!String.IsNullOrEmpty(InfoText))
          form.AddInfoLabel(DockStyle.Bottom).Text = InfoText;

        res = EFPApp.ShowDialog(form, false, DialogPosition);
      }

      if (res == DialogResult.OK)
        Table.AcceptChanges();

      return res;
    }

    #endregion
  }
}
