// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер табличного просмотра, предназначенного для редактирования данных <see cref="DataTable"/> "по месту".
  /// Инициализация столбцов просмотра выполняется при установке свойства <see cref="EFPInputDataGridView.Data"/>. 
  /// Заполнение свойства <see cref="EFPDataGridView.Columns"/> из прикаладного кода не выполняется.
  /// Данные хранятся в объекте <see cref="UIInputGridData"/>, который, кроме таблицы данных, содержит дополнительные свойства для столбцов и проверочные объекты.
  /// Свойство <see cref="EFPInputDataGridView.FixedRows"/> позволяет работать с таблицами произвольного размера или с фиксированным числом строк.
  /// </summary>
  public class EFPInputDataGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к <see cref="DataGridView"/>
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPInputDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к ControlWithToolBar.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPInputDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
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

      CanInsertCopy = false;
      CanView = false;
      CommandItems.ClipboardInToolBar = true;
      UseRowImages = true;

      _ValidatingResults = new Dictionary<int, RowValidatingResults>();
      _TempValidableObject = new UISimpleValidableObject();

      FixedRows = false; // 04.07.2023
    }

    #endregion

    #region Свойство FixedRows

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

    #region Свойство Data

    /// <summary>
    /// Просматриваемые данные и настройки столбцов.
    /// Установка свойства приводит к инициализации просмотра. Создаются столбцы и строки.
    /// В процессе редактирования таблица <see cref="UIInputGridData.Table"/> обновляется автоматически и всегда содержит актуальные данные.
    /// </summary>
    public UIInputGridData Data
    {
      get { return _Data; }
      set
      {
        if (_Data != null)
          _Data.Table.DefaultView.ListChanged -= DV_ListChanged;

        _Data = value;
        InitColumns();

        if (value == null)
          SourceAsDataView = null;
        else
        {
          SourceAsDataView = value.Table.DefaultView;
          if (ProviderState == EFPControlProviderState.Attached)
            _Data.Table.DefaultView.ListChanged += DV_ListChanged;
        }
      }
    }
    private UIInputGridData _Data;

    #endregion

    #region Инициализация столбцов

    private void InitColumns()
    {
      if (Columns.Count > 0)
        Columns.Clear();

      if (_Data == null)
        return;

      for (int i = 0; i < _Data.Table.Columns.Count; i++)
      {
        DataColumn col = _Data.Table.Columns[i];
        UIInputGridData.ColumnInfo colInfo = _Data.Columns[col.ColumnName];

        string title = col.Caption;
        if (String.IsNullOrEmpty(title))
          title = col.ColumnName;

        if (col.DataType == typeof(Boolean))
          Columns.AddBool(col.ColumnName, true, title);
        else if (col.DataType == typeof(DateTime))
        {
          if (String.IsNullOrEmpty(colInfo.Format))
            Columns.AddDate(col.ColumnName, true, title);
          else
            Columns.AddText(col.ColumnName, true, title);
        }
        else
          Columns.AddText(col.ColumnName, true, title);

        if (colInfo.TextWidth > 0)
          Columns.LastAdded.TextWidth = colInfo.TextWidth;
        if (colInfo.FillWeight > 0)
        {
          Columns.LastAdded.GridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          Columns.LastAdded.GridColumn.FillWeight = colInfo.FillWeight;
        }
        switch (colInfo.Align)
        {
          case UIHorizontalAlignment.Left: Columns.LastAdded.TextAlign = HorizontalAlignment.Left; break;
          case UIHorizontalAlignment.Center: Columns.LastAdded.TextAlign = HorizontalAlignment.Center; break;
          case UIHorizontalAlignment.Right: Columns.LastAdded.TextAlign = HorizontalAlignment.Right; break;
        }
        if (!String.IsNullOrEmpty(colInfo.Format))
          Columns.LastAdded.GridColumn.DefaultCellStyle.Format = colInfo.Format;
      }

      DisableOrdering();
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Реализует добавление и удаление строк
    /// </summary>
    /// <param name="args">Фиктивный аргумент</param>
    /// <returns></returns>
    protected override bool OnEditData(EventArgs args)
    {
      DataTable tbl = SourceAsDataTable;
      int[] rowIndices;

      switch (State)
      {
        case UIDataState.Insert:
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

        case UIDataState.Delete:
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

    #region Сжатие и проверка таблицы при закрытии формы

    /// <summary>
    /// При закрытии формы выполняет сжатие таблицы данных вызовом <see cref="DataTable.AcceptChanges()"/>.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();

      if (BaseProvider.ValidateReason == EFPFormValidateReason.Closing || BaseProvider.ValidateReason == EFPFormValidateReason.ValidateForm)
      {
        if ((!Control.ReadOnly) && (!FixedRows))
          CompactTable();

        if (!Control.ReadOnly)
        {
          SourceAsDataTable.AcceptChanges();
          _ValidatingResults.Clear(); // строки могли сдвинуться

          for (int i = 0; i < Data.Table.DefaultView.Count;i++ )
          {
            if (ValidateState == UIValidateState.Error)
              break;

            RowValidatingResults rvr = GetRowValidatingResults(i);

            if (rvr.CellErrors != null)
            {
              foreach (KeyValuePair<int, string> pair in rvr.CellErrors)
              {
                Control.CurrentCell = Control[pair.Key, i];
                SetError(pair.Value);
                break;
              }
            }

            if (ValidateState == UIValidateState.Ok && rvr.CellWarnings != null)
            {
              foreach (KeyValuePair<int, string> pair in rvr.CellWarnings)
              {
                Control.CurrentCell = Control[pair.Key, i];
                SetWarning(pair.Value);
                break;
              }
            }
          }
        }
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

    #region Обработка изменений

    /// <summary>
    /// Результаты проверки для одной строки
    /// </summary>
    private struct RowValidatingResults
    {
      #region Поля

      /// <summary>
      /// Ячейки с ошибками.
      /// Ключ - индекс столбца таблицы.
      /// Значение - текст сообщения.
      /// Если поле содержит null, то в строке нет сообщений об ошибке
      /// </summary>
      public Dictionary<int, string> CellErrors;

      /// <summary>
      /// Ячейки с предупреждениями.
      /// Ключ - индекс столбца таблицы.
      /// Значение - текст сообщения.
      /// Если поле содержит null, то в строке нет предупреждений
      /// </summary>
      public Dictionary<int, string> CellWarnings;

      #endregion
    }

    /// <summary>
    /// Буферизация проверки строк.
    /// Ключ - индекс строки в просмотре
    /// Значение - Результат проверки по строке
    /// </summary>
    private Dictionary<int, RowValidatingResults> _ValidatingResults;

    void DV_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs args)
    {
      switch (args.ListChangedType)
      {
        case System.ComponentModel.ListChangedType.ItemChanged:
          _ValidatingResults.Remove(args.NewIndex);
          break;
        case System.ComponentModel.ListChangedType.ItemAdded:
          // можно не очищать ValidatingResults.
          DataRow row = ((DataView)sender)[args.NewIndex].Row;
          bool otherRowsChanged;
          InitManualOrderColumnValue(new DataRow[1] { row }, out otherRowsChanged);
          _ValidatingResults.Clear(); // 07.07.2023
          break;
        default:// неохота проверять остальные режимы
          _ValidatingResults.Clear();
          break;
      }
    }

    /// <summary>
    /// Выполняет валидацию строки данных
    /// </summary>
    /// <param name="rowIndex">Индекс строки данных</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="reason">Причина вызова (ручное редактирование, буфер обмена, ...)</param>
    public override void OnCellFinished(int rowIndex, int columnIndex, EFPDataGridViewCellFinishedReason reason)
    {
      _ValidatingResults.Remove(rowIndex); // 07.07.2023
      base.OnCellFinished(rowIndex, columnIndex, reason);
    }

    /// <summary>
    /// Присоединение обработчика <see cref="DataView.ListChanged"/>.
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      if (_Data != null)
        _Data.Table.DefaultView.ListChanged += DV_ListChanged;
    }

    /// <summary>
    /// Отключение обработчика <see cref="DataView.ListChanged"/>.
    /// </summary>
    protected override void OnDetached()
    {
      if (_Data != null)
        _Data.Table.DefaultView.ListChanged -= DV_ListChanged;

      base.OnDetached();
    }

    #endregion

    #region Раскраска ячеек с ошибками

    /// <summary>
    /// Получение списка сообщений для строки.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      base.OnGetRowAttributes(args);
      if (Data == null)
        return;
      if (args.RowIndex < 0 || args.RowIndex >= Data.Table.DefaultView.Count)
        return; // заготовка новой строки

      RowValidatingResults rvr = GetRowValidatingResults(args.RowIndex);

      if (rvr.CellErrors != null)
      {
        foreach (KeyValuePair<int, string> pair in rvr.CellErrors)
        {
          string colTitle = Columns[pair.Key].GridColumn.HeaderText;
          args.AddRowError(colTitle + ". " + pair.Value, Data.Table.Columns[pair.Key].ColumnName);
        }
      }

      if (rvr.CellWarnings != null)
      {
        foreach (KeyValuePair<int, string> pair in rvr.CellWarnings)
        {
          string colTitle = Columns[pair.Key].GridColumn.HeaderText;
          args.AddRowWarning(colTitle + ". " + pair.Value, Data.Table.Columns[pair.Key].ColumnName);
        }
      }
    }

    private RowValidatingResults GetRowValidatingResults(int rowIndex)
    {
      RowValidatingResults rvr;

      if (!_ValidatingResults.TryGetValue(rowIndex, out rvr))
      {
        // Требуется проверка
        rvr = new RowValidatingResults();

        try
        {
          ValidateRow(rowIndex, ref rvr);
        }
        catch (Exception e)
        {
          rvr.CellErrors = new Dictionary<int, string>();
          rvr.CellErrors.Add(0, "Ошибка при проверке строки. " + e.Message);
        }

        _ValidatingResults.Add(rowIndex, rvr);
      }
      return rvr;
    }

    /// <summary>
    /// Чтобы не создавать для каждой ячейки
    /// </summary>
    UISimpleValidableObject _TempValidableObject;

    /// <summary>
    /// Проверка одной строки
    /// </summary>
    private void ValidateRow(int rowIndex, ref RowValidatingResults rvr)
    {
      DataRow row = GetDataRow(rowIndex);
      if (row == null)
        return;
      Data.InternalSetValidatingRow(row);

      for (int i = 0; i < Data.Table.Columns.Count; i++)
      {
        _TempValidableObject.Clear();
        if (row.IsNull(i))
        {
          switch (Data.Columns[i].CanBeEmptyMode)
          {
            case UIValidateState.Error:
              _TempValidableObject.SetError("Значение должно быть задано");
              break;
            case UIValidateState.Warning:
              _TempValidableObject.SetWarning("Значение, веротяно, должно быть задано");
              break;
          }
        }

        if (Data.Columns[i].HasValidators)
          Data.Columns[i].Validators.Validate(_TempValidableObject);

        switch (_TempValidableObject.ValidateState)
        {
          case UIValidateState.Error:
            if (rvr.CellErrors == null)
              rvr.CellErrors = new Dictionary<int, string>();
            rvr.CellErrors.Add(i, _TempValidableObject.Message);
            break;

          case UIValidateState.Warning:
            if (rvr.CellWarnings == null)
              rvr.CellWarnings = new Dictionary<int, string>();
            rvr.CellWarnings.Add(i, _TempValidableObject.Message);
            break;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Диалог для ввода табличных данных.
  /// Данные хранятся в виде объекта <see cref="UIInputGridData"/>.
  /// </summary>
  public class InputDataGridDialog : BaseInputDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект значениями по умолчанию.
    /// Перед выводом диалога должно быть установлено свойство DataSource
    /// </summary>
    public InputDataGridDialog()
    {
      Title = "Таблица";
      ImageKey = "Table";
      _Data = new UIInputGridData();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - редактируемая таблица данных.
    /// В таблицу должны быть добавлены столбцы перед показом диалога.
    /// Могут использоваться столбцы для просмотра, если установлено свойство <see cref="DataColumn.Expression"/>.
    /// Дополнительные параметры для добавленных столбцов, например, формат, задавайте с помощью методов коллекции <see cref="UIInputGridData.Columns"/>.
    /// 
    /// Если свойство <see cref="FixedRows"/>=true, то в таблицу следует добавить строки, 
    /// иначе пользователь не сможет ничего ввести.
    /// </summary>
    public UIInputGridData Data
    {
      get { return _Data; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Data = value;
      }
    }
    private UIInputGridData _Data;


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
        efpGrid.Data = this.Data;
        efpGrid.ReadOnly = ReadOnly;
        efpGrid.Control.ReadOnly = ReadOnly;
        efpGrid.FixedRows = FixedRows || ReadOnly;

        if (!String.IsNullOrEmpty(InfoText))
          form.AddInfoLabel(DockStyle.Bottom).Text = InfoText;

        res = EFPApp.ShowDialog(form, false, DialogPosition);
      }

      if (res == DialogResult.OK)
        this.Data.Table.AcceptChanges();

      return res;
    }

    #endregion
  }
}
