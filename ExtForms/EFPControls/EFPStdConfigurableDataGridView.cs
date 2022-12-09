// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Data;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Расширение провайдера табличного просмотра для использования класса продюсера EFPGridProducer, вместо интефрейса IEFPGridProducer.
  /// Позволяет использовать повторитель таблиц EFPGridProducerDataTableRepeater.
  /// </summary>
  public class EFPStdConfigurableDataGridView : EFPConfigurableDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создание провайдера
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPStdConfigurableDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
    }

    /// <summary>
    /// Создание провайдера
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPStdConfigurableDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// Генератор столбцов таблицы. Если задан, то в локальном меню доступны
    /// команды настройки столбцов таблицы.
    /// Установка свойства также устанавливает свойство AutoSort=true.
    /// При необходимости использования события CurrentOrderChanged, установите AutoSort=false обратно.
    /// </summary>
    public new EFPGridProducer GridProducer
    {
      // Тип EFPGridProducer нужен, т.к. он содержит метод InitGridView() с расширенным списком аргументов.
      // Используется в DocTypeUI.PerformInitGrid()

      get { return (EFPGridProducer)(base.GridProducer); }
      set
      {
        base.GridProducer = value;
        if (value != null)
          base.AutoSort = true; // 16.07.2021
      }
    }

    /// <summary>
    /// Вызывает InitTableRepeaterForGridProducer(), если хотя бы раз выполнялась установка свойств MasterDataTable или MasterDataView
    /// </summary>
    protected override void OnGridProducerPostInit()
    {
      if (_MasterDataTableHasBeenSet) // была ли установка свойства MasterDataTable или MasterDataView?
      {
        if (TableRepeater == null)
          InitTableRepeaterForGridProducer(SourceAsDataTable); // вдруг теперь понадобится повторитель
        else
          InitTableRepeaterForGridProducer(TableRepeater.MasterTable); // создаем новый повторитель
      }

      base.OnGridProducerPostInit(); // вызов пользовательского обработчика
    }

    #endregion

    #region MasterDataTable

    /// <summary>
    /// Базовый источник для автоматического использования таблицы-повторителя.
    /// Должен использоваться вместо непосредственной установки DataGridView.DataSource или EFPDataGridView.SourceAsDataTable
    /// </summary>
    public new DataTable MasterDataTable
    {
      get
      {
        if (TableRepeater == null)
          return SourceAsDataTable;
        else
          return TableRepeater.MasterTable;
      }
      set
      {
        _MasterDataTableHasBeenSet = true;
        if (CurrentConfig != null && GridProducer != null)
          InitTableRepeaterForGridProducer(value);
        else
          SourceAsDataTable = value; // вызов до загрузки конфигурации
      }
    }
    private bool _MasterDataTableHasBeenSet;

    /// <summary>
    /// Альтеранативная установка свойства MasterDataTable.
    /// Проверяет, есть ли у присоединяемого DataView фильтр по строкам данных.
    /// Если есть, то создает еще одну промежуточную таблицу.
    /// </summary>
    public new DataView MasterDataView
    {
      get
      {
        if (TableRepeater == null)
          return SourceAsDataView;
        else
          return TableRepeater.MasterTable.DefaultView;
      }
      set
      {
        _MasterDataTableHasBeenSet = true;
        if (CurrentConfig != null && GridProducer != null)
        {
          if (value == null)
            MasterDataTable = null;
          else if (String.IsNullOrEmpty(value.RowFilter))
            MasterDataTable = value.Table;
          else if (TableRepeaterRequired(value.Table))
            MasterDataTable = value.ToTable(); // создается еще одна копия таблицы
          else
            MasterDataTable = value.Table; // можно обойтись без копии
        }
        else
          SourceAsDataView = value;
      }
    }

    #endregion

    #region InitTableRepeaterForGridProducer

    /// <summary>
    /// Проверяет столбцы просмотра на наличие полей в таблице данных <paramref name="masterTable"/>.
    /// Если есть вычисляемые столбцы, то создается таблица-повторитель EFPGridProducerDataTableRepeater.
    /// После этого устанавливаются свойства DataGridViewColumn.DataPropertyName, чтобы значения столбцов
    /// не вычислялись, а брались из таблицы-повторителя. Это позволяет использовать для этих столбцов произвольную сортировку.
    /// 
    /// Если в просмотре нет подходящих вычисляемых столбцов, свойство SourceAsDataTable устанавливается напрямую на <paramref name="masterTable"/>.
    /// Если раньше использовалась таблица-повторитель, она удаляется.
    /// 
    /// Свойство GridProducer должно быть установлено.
    /// 
    /// В прикладном коде предпочтительнее использовать свойства MasterDataTable или MasterDataView.
    /// </summary>
    /// <param name="masterTable">Исходная таблица данных</param>
    public void InitTableRepeaterForGridProducer(DataTable masterTable)
    {
      if (GridProducer == null)
        throw new NullReferenceException("Свойство GridProducer не установлено");

      if (masterTable == null)
      {
        TableRepeater = null;
        return;
      }

      //EFPDataGridViewSelection oldSel = Selection;

      if (TableRepeaterRequired(masterTable))
      {
        //bool initColumnSortModeRequired = false;

        EFPGridProducerDataTableRepeater rep = new EFPGridProducerDataTableRepeater(GridProducer, this);
        rep.SlaveTable = masterTable.Clone();
        AddGridColumnsToRepeaterTable(rep);

        // Не нужно. DataTable.Clone() копирует и первичный ключ
        //DataTools.SetPrimaryKey(rep.SlaveTable, DataTools.GetPrimaryKey(masterTable));

        rep.SlaveTable.TableName = "Repeater";

        rep.MasterTable = masterTable;
        TableRepeater = rep;

        //if (initColumnSortModeRequired)
        //  InitColumnSortMode(); // обновляем SortMode
      }
      else
      {
        TableRepeater = null;
        SourceAsDataTable = masterTable;
      }
      //Selection = oldSel;
    }

    /// <summary>
    /// Добавляет в <paramref name="rep"/>.SlaveTable столбцы <see cref="DataColumn"/> для всех
    /// столбцов табличного просмотра, кроме <see cref="DataGridViewImageColumn"/>, чтобы их можно было использовать для произвольной сортировки.
    /// </summary>
    /// <param name="rep">Созданная таблица-повторитель</param>
    private void AddGridColumnsToRepeaterTable(DataTableRepeater rep)
    {
      foreach (EFPDataGridViewColumn col1 in this.Columns)
      {
        EFPGridProducerColumn col2 = col1.ColumnProducer as EFPGridProducerColumn;
        if (col2 == null)
          continue; // столбец создан вручную, а не с помощью GridProducer
        if (col2.SourceColumnNames == null)
          continue; // данные столбца извлекаются из источника (таблицы), а не вычисляются
        if (col2.SourceColumnNames.Length == 0)
          continue; // вычисляемый столбец не нуждается в данных
        if (!rep.SlaveTable.Columns.Contains(col2.Name))
        {

          Type typ = col2.DataType;
          if (typ == null)
            typ = typeof(string);
          rep.SlaveTable.Columns.Add(col2.Name, typ);
        }
        if (String.IsNullOrEmpty(col1.CustomOrderColumnName))
        {
          if (!(col1.GridColumn is DataGridViewImageColumn))
          {
            col1.CustomOrderColumnName = col2.Name;
            //if (CustomOrderActive)
            //  initColumnSortModeRequired = true; // для этого столбца нужно будет установить SortMode, иначе возникет ошибка при попытке нарисовать треугольник после щелчка мыши
          }
        }
        if (col1.CustomOrderColumnName == col2.Name)
          col1.GridColumn.DataPropertyName = col2.Name; // столбец больше не является вычисляемым, а берет значение из базы данных
      }
    }

    private bool TableRepeaterRequired(DataTable masterTable)
    {
      foreach (EFPDataGridViewColumn col1 in Columns)
      {
        EFPGridProducerColumn col2 = col1.ColumnProducer as EFPGridProducerColumn;
        if (col2 == null)
          continue;
        if (col2.SourceColumnNames == null)
          continue;
        if (col2.SourceColumnNames.Length == 0)
          continue;
        return true;
      }
      return false;
    }

    #endregion
  }

  /// <summary>
  /// Повторитель таблицы, вычисляющий поля с помощью EFPGridProducerColumn.GetValue()
  /// </summary>
  public class EFPGridProducerDataTableRepeater : DataTableRepeater
  {
    #region Конструктор

    /// <summary>
    /// Создает повторитель
    /// </summary>
    /// <param name="gridProducer">Генератор табличного просмотра. Должен быть задан</param>
    /// <param name="controlProvider">Ссылка на провайдер табличного просмотра. Передается методу EFPGridProducerColumn.GetValue()</param>
    public EFPGridProducerDataTableRepeater(EFPGridProducer gridProducer, IEFPDataView controlProvider)
    {
      if (gridProducer == null)
        throw new ArgumentNullException("gridProducer");

      _GridProducer = gridProducer;
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Генератор табличного просмотра.
    /// Задается в конструкторе.
    /// </summary>
    public EFPGridProducer GridProducer { get { return _GridProducer; } }
    EFPGridProducer _GridProducer;

    /// <summary>
    /// Провадйер управляющего элемента
    /// </summary>
    public IEFPDataView ControlProvider { get { return _ControlProvider; } }
    private IEFPDataView _ControlProvider;

    #endregion

    #region Словарь

    /// <summary>
    /// Ключ - имя столбца вычисляемого поля
    /// Значение - столбец в EFPGridProducer
    /// </summary>
    private Dictionary<string, EFPGridProducerColumn> _ColumnDict;

    private DataTableValueArray _VA;

    /// <summary>
    /// Создает внутренний словарь вычисляемых полей
    /// </summary>
    protected override void OnMasterTableChanged()
    {
      base.OnMasterTableChanged();
      if (MasterTable == null)
      {
        _ColumnDict = null;
        _VA = null;
      }
      else
      {
        _ColumnDict = new Dictionary<string, EFPGridProducerColumn>();
        foreach (DataColumn col1 in SlaveTable.Columns)
        {
          EFPGridProducerColumn col2 = _GridProducer.Columns[col1.ColumnName];
          if (col2 == null)
            continue;
          if (col2.SourceColumnNames == null)
            continue; // невычисляемый столбец
          if (col2.SourceColumnNames.Length == 0)
            continue; // столбец типа №п/п
          // Проверяем, что есть все исходные столбцы
          bool allPresents = true;
          for (int j = 0; j < col2.SourceColumnNames.Length; j++)
          {
            if (!MasterTable.Columns.Contains(col2.SourceColumnNames[j]))
            {
              allPresents = false;
              break;
            }
          }

          if (allPresents)
            _ColumnDict.Add(col1.ColumnName, col2);
        }
        _VA = new DataTableValueArray(MasterTable);
      }
    }

    #endregion

    #region OnValueNeeded()

    /// <summary>
    /// Получает значение для вычисляемого поля
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnValueNeeded(DataTableRepeaterValueNeededEventArgs args)
    {
      EFPGridProducerColumn col;
      if (_ColumnDict.TryGetValue(args.ColumnName, out col))
      {
        _VA.CurrentRow = args.SourceRow;
        EFPDataViewRowInfo rowInfo = new EFPDataViewRowInfo(_ControlProvider, args.SourceRow, _VA, -1);
        args.Value = col.GetValue(rowInfo);
      }

      base.OnValueNeeded(args);
    }

    #endregion
  }
}
