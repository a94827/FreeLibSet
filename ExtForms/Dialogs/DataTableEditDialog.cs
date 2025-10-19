using FreeLibSet.UICore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
using FreeLibSet.Core;
using FreeLibSet.Data;

namespace FreeLibSet.Forms.Data
{
  /// <summary>
  /// Редактор строк табличного просмотра для <see cref="EFPDataGridView"/>, привязанного к <see cref="DataTable"/>
  /// </summary>
  public class DataTableEditDialog : DataEditDialog
  {
    #region Конструкторы

    /// <summary>
    /// Создает редактор для табличного просмотра
    /// </summary>
    /// <param name="controlProvider">Табличный или иерархический просмотр</param>
    public DataTableEditDialog(IEFPDataView controlProvider)
      : this(controlProvider.SourceAsDataTable, controlProvider.State, controlProvider.SelectedDataRows)
    {
      _ControlProvider = controlProvider;
    }

    /// <summary>
    /// Создает редактор
    /// </summary>
    /// <param name="table">Таблица данных</param>
    /// <param name="state">Режим редактирования</param>
    /// <param name="selectedRows">Редактируемые/просматриваемые строки</param>
    public DataTableEditDialog(DataTable table, UIDataState state, DataRow[] selectedRows)
    {
      base.DataState = state;
      switch (state)
      {
        case UIDataState.Insert:
          selectedRows = new DataRow[1] { table.NewRow() };
          break;
        case UIDataState.InsertCopy:
          if (selectedRows.Length == 0)
            throw ExceptionFactory.ArgIsEmpty("selectedRows");

          DataRow[] newRows = new DataRow[selectedRows.Length];
          for (int i = 0; i < selectedRows.Length; i++)
          {
            newRows[i] = table.NewRow();
            newRows[i].ItemArray = selectedRows[i].ItemArray;
          }
          selectedRows = newRows;
          break;
        default:
          if (selectedRows.Length == 0)
            throw ExceptionFactory.ArgIsEmpty("selectedRows");
          break;
      }

      if (selectedRows.Length == 1)
        base.DocumentTitle = Res.DBxDataTableExtEditor_Msg_TitleRecord;
      else
        base.DocumentTitle = String.Format(Res.DBxDataTableExtEditor_Msg_TitleRecords, selectedRows.Length);

      _ExtValues = new DBxDataRowArrayExtValues(table, selectedRows);
      switch (state)
      {
        case UIDataState.View:
        case UIDataState.Delete:
          _ExtValues.IsReadOnly = true;
          break;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактируемые значения
    /// </summary>
    public DBxDataRowArrayExtValues ExtValues { get { return _ExtValues; } }
    private readonly DBxDataRowArrayExtValues _ExtValues;

    private readonly IEFPDataView _ControlProvider;

    #endregion

    #region Событие инициализации формы

    /// <summary>
    /// Событие вызывается перед показом формы и должно инициализировать поля формы
    /// </summary>
    public event DBxExtValuesDialogInitEventHandler InitEditForm;

    /// <summary>
    /// Вызывает обработчик события <see cref="InitEditForm"/>, если он установлен.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnInitEditForm(DBxExtValuesDialogInitEventArgs args)
    {
      if (InitEditForm != null)
        InitEditForm(this, args);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Перед показом формы вызывается событие <see cref="InitEditForm"/>
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnLoad(EventArgs args)
    {
      DBxExtValuesDialogInitEventArgs args2 = new DBxExtValuesDialogInitEventArgs(this, _ExtValues);
      OnInitEditForm(args2);

      base.OnLoad(args);
    }

    private bool _OnApplyingCalled;

    /// <summary>
    /// Присоединяет новые строки к таблице в режимах добавления строк.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnApplying(CancelEventArgs args)
    {
      if (!_OnApplyingCalled)
      {
        switch (DataState)
        {
          case UIDataState.Insert:
          case UIDataState.InsertCopy:
            foreach (DataRow row in _ExtValues.Rows)
              _ExtValues.Table.Rows.Add(row);
            if (_ControlProvider != null)
              _ControlProvider.SelectedDataRows = _ExtValues.Rows;
            break;
          case UIDataState.Delete:
            foreach (DataRow row in _ExtValues.Rows)
              row.Delete();
            break;
        }
      }
      _OnApplyingCalled = true;

      base.OnApplying(args);
    }

    #endregion
  }
}
