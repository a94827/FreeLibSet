// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Config;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  #region EFPDBxGridViewDocSelEventArgs

  #region Перечисление

  /// <summary>
  /// Причина создания выборки документов для выбранных строк табличного просмотра
  /// (пользователь выбрал команду "Отправить" или копирование ячеек в буфер)
  /// Свойство GridHandlerDocSelEventArgs.Reason
  /// </summary>
  public enum EFPDBxGridViewDocSelReason
  {
    /// <summary>
    /// Пользователь выполнил команду "Отправить"
    /// Обработчик может вывести блок диалога, если возможны варианты создания
    /// выборки
    /// </summary>
    SendTo,

    /// <summary>
    /// Выполняется копирование ячеек в буфер обмена
    /// Обработчик не должен взаимодействовать с пользователем
    /// </summary>
    Copy,
  }

  #endregion

  /// <summary>
  /// Аргументы события EFPDBxTreeView.GetDocSel
  /// </summary>
  public class EFPDBxGridViewDocSelEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается
    /// </summary>
    /// <param name="controlProvider"></param>
    /// <param name="reason"></param>
    /// <param name="rowIndices"></param>
    public EFPDBxGridViewDocSelEventArgs(EFPDBxGridView controlProvider, EFPDBxGridViewDocSelReason reason, int[] rowIndices)
    {
      _ControlProvider = controlProvider;
      _DocSel = new DBxDocSelection(controlProvider.UI.DocProvider.DBIdentity);
      _Reason = reason;
      _RowIndices = rowIndices;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к табличному просмотру
    /// </summary>
    public EFPDBxGridView ControlProvider { get { return _ControlProvider; } }
    private EFPDBxGridView _ControlProvider;

    /// <summary>
    /// Индексы строк, для которых требуется создать выборку документов
    /// </summary>
    public Int32[] RowIndices
    {
      get
      {
        if (_RowIndices == null)
          _RowIndices = _ControlProvider.SelectedRowIndices;
        return _RowIndices;
      }
    }
    private Int32[] _RowIndices;

    /// <summary>
    /// Строки таблицы данных, для которых должна быть получена выборка.
    /// Если табличный просмотр не привязан к таблице данных, то массив содержит
    /// значения null. Для таких просмотров следует использовать RowIndices или
    /// GridRows
    /// </summary>
    public DataRow[] DataRows
    {
      get
      {
        if (_DataRows == null)
        {
          _DataRows = new DataRow[RowIndices.Length];
          for (int i = 0; i < _DataRows.Length; i++)
            _DataRows[i] = _ControlProvider.GetDataRow(RowIndices[i]);
        }
        return _DataRows;
      }
    }
    private DataRow[] _DataRows;

    /// <summary>
    /// Строки табличного просмотра, для которых должна быть получена выборка
    /// </summary>
    public DataGridViewRow[] GridRows
    {
      get
      {
        if (_GridRows == null)
        {
          _GridRows = new DataGridViewRow[RowIndices.Length];
          for (int i = 0; i < _GridRows.Length; i++)
            _GridRows[i] = _ControlProvider.Control.Rows[RowIndices[i]];
        }
        return _GridRows;
      }
    }
    private DataGridViewRow[] _GridRows;

    /// <summary>
    /// Причина, по которой требуется создать выборку
    /// </summary>
    public EFPDBxGridViewDocSelReason Reason { get { return _Reason; } }
    private EFPDBxGridViewDocSelReason _Reason;

    /// <summary>
    /// Сюда должны быть добавлены ссылки на документы
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private DBxDocSelection _DocSel;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить ссылки из поля таблицы
    /// Проверяется наличие в таблице Rows[0].Table поля ColumnName; если поля
    /// нет, то ничего не выполняется.
    /// Для извлечения идентификаторов используется DataTools.GetIdsFromField()
    /// Добавляется только ссылка на документ, без вызова обработчика ClientDocType.GetDocSel
    /// </summary>
    /// <param name="tableName">Имя таблицы документа ClientDocType</param>
    /// <param name="columnName">Имя ссылочного поля в таблице просмотра</param>
    public void AddFromColumn(string tableName, string columnName)
    {
      AddFromColumn(tableName, columnName, false);
    }
    /// <summary>
    /// Добавить ссылки из поля таблицы
    /// Проверяется наличие в таблице Rows[0].Table поля ColumnName; если поля
    /// нет, то ничего не выполняется.
    /// Для извлечения идентификаторов используется DataTools.GetIdsFromField()
    /// Если UseHandler=true, то используется обработчик ClientDocType.GetDocSel,
    /// при этом могут быть добавлены дополнительные ссылки. Если UseHandler=false,
    /// то добавляется только ссылка на документ
    /// </summary>
    /// <param name="tableName">Имя таблицы документа ClientDocType</param>
    /// <param name="columnName">Имя ссылочного поля в таблице просмотра</param>
    /// <param name="useHandler">Если true, то используется обработчик GetDocSel</param>
    public void AddFromColumn(string tableName, string columnName, bool useHandler)
    {
      if (DataRows.Length == 0)
        return; // 10.01.2019

      if (!DataRows[0].Table.Columns.Contains(columnName))
        return;

      Int32[] Ids = DataTools.GetIdsFromColumn(DataRows, columnName);
      if (useHandler)
      {
        DocTypeUIBase dtb = ControlProvider.UI.DocTypes.FindByTableName(tableName);
        if (dtb == null)
          throw new ArgumentException("Неизвестный вид документа или поддокумента \"" + tableName + "\"", "tableName");

        dtb.PerformGetDocSel(DocSel, Ids, Reason);
      }
      else
        DocSel.Add(tableName, Ids);
    }


    /// <summary>
    /// Загрузить из полей переменной ссылки "TableId" и "DocId"
    /// Добавляется только ссылка на документ, без вызова обработчика ClientDocType.GetDocSel
    /// </summary>
    /// <param name="tableIdColumnName">Имя поля, содержащего идентификатор таблицы документов (свойство DBxDocType.TableId)</param>
    /// <param name="docIdColumnName">Имя поля, содержащего идентификатор документа</param>
    public void AddFromVTReference(string tableIdColumnName, string docIdColumnName)
    {
      AddFromVTReference(tableIdColumnName, docIdColumnName, false);
    }

    /// <summary>
    /// Загрузить из полей переменной ссылки "TableId" и "DocId"
    /// Если UseHandler=true, то используется обработчик ClientDocType.GetDocSel,
    /// при этом могут быть добавлены дополнительные ссылки. Если UseHandler=false,
    /// то добавляется только ссылка на документ
    /// </summary>
    /// <param name="tableIdColumnName">Имя поля, содержащего идентификатор таблицы документов (свойство DBxDocType.TableId)</param>
    /// <param name="docIdColumnName">Имя поля, содержащего идентификатор документа</param>
    /// <param name="useHandler">Если true, то используется обработчик GetDocSel</param>
    public void AddFromVTReference(string tableIdColumnName, string docIdColumnName, bool useHandler)
    {
      if (DataRows.Length == 0)
        return; // 10.01.2019
      int pTable = DataRows[0].Table.Columns.IndexOf(tableIdColumnName);
      int pId = DataRows[0].Table.Columns.IndexOf(docIdColumnName);
      if (pTable < 0 || pId < 0)
        return;

      for (int i = 0; i < DataRows.Length; i++)
      {
        Int32 TableId = DataTools.GetInt(DataRows[i][pTable]);
        Int32 DocId = DataTools.GetInt(DataRows[i][pId]);
        if (TableId != 0 && DocId != 0)
        {
          DocTypeUI dtui = ControlProvider.UI.DocTypes.FindByTableId(TableId);
          if (dtui != null)
          {
            if (useHandler)
              dtui.PerformGetDocSel(DocSel, DocId, Reason);
            else
              DocSel.Add(dtui.DocType.Name, DocId);
          }
        }
      }
    }

    #endregion
  }


  /// <summary>
  /// Делегат события EFPDBxTreeView.GetDocSel
  /// </summary>
  /// <param name="sender">Провайдер EFPDBxTreeView или производного класса</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDBxGridViewDocSelEventHandler(object sender,
    EFPDBxGridViewDocSelEventArgs args);

  #endregion

  #region Интерфейс IEFPDBxView

  /// <summary>
  /// Общая часть для просмотров EFPDBxGridView и EFPDBxTreeView.
  /// Интерфейс используется для синхронизации нескольких просмотров одного вида документов или поддокументов,
  /// расположенных в одном окне, с помощью EFPDBxViewSync
  /// </summary>
  public interface IEFPDBxView : IEFPDataView, IEFPControlWithFilters
  {
    /// <summary>
    /// Интерфейс доступа к документам
    /// </summary>
    DBUI UI { get; }

    /// <summary>
    /// Идентификатор документа, поддокумента или записи в какой-либо другой таблице для текущей строки
    /// </summary>
    Int32 CurrentId { get; set;}

    /// <summary>
    /// Массив идентификаторов для выбранных строк таблицы или ущлов дерева
    /// </summary>
    Int32[] SelectedIds { get; set;}

    /// <summary>
    /// Событие вызывается, если пользователь изменяет выбор строк
    /// </summary>
    event EventHandler SelectionChanged;

    /// <summary>
    /// Возвращает true, если в провайдере установлен обработчик события GetDocSel или
    /// есть другой способ создания выборки документов
    /// </summary>
    bool HasGetDocSelHandler { get; }

    /// <summary>
    /// Создает выборку документов для отмеченных строк просмотра
    /// </summary>
    /// <param name="reason">Причина необходимости построения выборки</param>
    /// <returns>Созданная выборка документов</returns>
    DBxDocSelection CreateDocSel(EFPDBxGridViewDocSelReason reason);

    /// <summary>
    /// Пометить на обновление строки с заданными идентификаторами
    /// </summary>
    /// <param name="ids">Массив идентификаторов документов</param>
    void UpdateRowsForIds(Int32[] ids);

    /// <summary>
    /// Пометить на обновление строки с заданными идентификаторами
    /// </summary>
    /// <param name="ids">Список идентификаторов документов</param>
    void UpdateRowsForIds(IdList ids);
  }

  #endregion

  /// <summary>
  /// Расширение класса EFPDataGridView для работы с таблицами базы данных.
  /// Источником данных DataGridView.DataSource должна быть таблица DataTable или просмотр DataView
  /// Поддерживает идентификатор строки - поле Id. В DataTable могуть быть строки с незаданным или
  /// нулевым идентификатором
  /// Поддерживает объекты GridProducer и GridFilters, связанные с объектом DBUI 
  /// </summary>
  public class EFPDBxGridView : EFPStdConfigurableDataGridView, IEFPDBxView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDBxGridView(EFPBaseProvider baseProvider, DataGridView control, DBUI ui)
      : base(baseProvider, control)
    {
      Init(ui);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDBxGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DBUI ui)
      : base(controlWithToolBar)
    {
      Init(ui);
    }

    private void Init(DBUI ui)
    {
      _DelayedCurrentId = 0; // Отложенное позиционирование

      SelectedRowsMode = EFPDataGridViewSelectedRowsMode.PrimaryKey;

      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _SaveCurrentId = true;

      base.UseIdle = true; // 04.07.2021 - для вызова FlushDelayedId()
    }

#if XXX
    /// <summary>
    /// Вызывается при получении события DataGridView.DataBindingComplete.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs args)
    {
      base.OnDataBindingComplete(args);

      if (ProviderState == EFPControlProviderState.Attached)
      {
        // 22.01.2016
        // Перенесено в OnDataBindingComplete()
        // Раньше добавлялся собственный обработчик события DataBindingComplete, который не блокировал вложенный вызов
        FlushDelayedId();
      }
    }
#endif

    /// <summary>
    /// Выполняет отложенную установку CurrentId, если это не удалось выполнить в OnAttached()
    /// </summary>
    public override void HandleIdle()
    {
      base.HandleIdle();

      // 04.07.2021
      // Перенесено в HandleIdle() из OnDataBindingComplete()
      // Предпочтительно, чтобы  
      FlushDelayedId();
    }

    private void FlushDelayedId()
    {
      if (_DelayedCurrentId != 0)
      {
        _PrevSavedId = _DelayedCurrentId;
        CurrentId = _DelayedCurrentId;
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

    #region Чтение и запись настроек

    private Int32 _PrevSavedId;

    /// <summary>
    /// Помечает категорию "GridView" для сохранения текущей позиции в секции конфигурации
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (SaveCurrentId && (CurrentId != _PrevSavedId) && CurrentId!=0)
      {
        ConfigHandler.Changed[EFPConfigCategories.GridView] = true; // для сохранения текущей позиции
        _PrevSavedId = CurrentId;
      }

      base.OnSaveConfig();
    }

    /// <summary>
    /// Добавляет в список категорию "GridView", если свойство SaveCurrentId установлено в true.
    /// </summary>
    /// <param name="categories">Список категорий для заполнения</param>
    /// <param name="rwMode">Режим чтения или записи</param>
    /// <param name="actionInfo">Описание действия</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);
      if (SaveCurrentId)
        categories.Add(EFPConfigCategories.GridView);
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
        case EFPConfigCategories.GridView:
          if (SaveCurrentId && (!String.IsNullOrEmpty(ConfigSectionName)))
            cfg.SetInt("CurrentId", CurrentId);
          break;
      }
      base.WriteConfigPart(category, cfg, actionInfo);
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
        case EFPConfigCategories.GridView:
          if (SaveCurrentId && /*CurrentId*/_DelayedCurrentId /*23.11.2017*/ == 0)
          {
            try
            {
              CurrentId = cfg.GetInt("CurrentId");
            }
            catch { }
          }
          break;
      }
      base.ReadConfigPart(category, cfg, actionInfo);
    }

    #endregion

    #region Текущая позиция

    /// <summary>
    /// Идентификатор в текущей строке (поле Id). Используется отложенная
    /// установка свойства, если форма еще не выведена на экран
    /// </summary>
    public virtual Int32 CurrentId
    {
      get
      {
        if (_DelayedCurrentId != 0)
          return _DelayedCurrentId;
        if (Control.CurrentRow == null)
          return 0;
        return GetRowId(Control.CurrentRow);
      }
      set
      {
        SetCurrentId(value);
      }
    }

    private Int32 _DelayedCurrentId;


    /// <summary>
    /// Установка свойства CurrentId.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>true, если реальная строка выбрана</returns>
    private bool SetCurrentId(Int32 value)
    {
      if (!Control.IsHandleCreated)
      {
        // Откладываем установку
        if (value != 0) // 18.09.2018
          _DelayedCurrentId = value;
        return false;
      }
      if (Control.DataSource == null || (Control.RowCount == 0 && SourceAsDataView.Count > 0))
      {
        // Откладываем установку
        if (value != 0) // 18.09.2018
          _DelayedCurrentId = value;
        return false;
      }

      _DelayedCurrentId = 0;

      if (value == 0)
        return true;

      // Если текущая позиция не изменилась, то ничего не делаем
      if (Control.CurrentRow != null)
      {
        if (value == GetRowId(Control.CurrentRow))
          return true;
      }

      // Выполняем установку
      // 1. Ищем строку в DataRow. Тупо и глупо !!!

      for (int i = 0; i < Control.Rows.Count; i++)
      {
        if (value == GetRowId(i))
        {
          CurrentGridRow = Control.Rows[i];
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Получить идентификатор строки "Id" для строки таблицы.
    /// Соответственно, строка делается Unshared
    /// </summary>
    /// <param name="row">Объект строки</param>
    /// <returns>Значение поля "Id"</returns>
    public Int32 GetRowId(DataGridViewRow row)
    {
      DataRow Row2 = GetDataRow(row);
      if (Row2 == null)
        return 0;
      int p = Row2.Table.Columns.IndexOf("Id");
      if (p < 0) // 10.03.2016
        return 0;
      return DataTools.GetInt(Row2[p]);
    }

    /// <summary>
    /// Получить идентификатор Id по номеру строки в табличном просмотре
    /// Предпочтительный метод, т.к. не делает строку Unshared
    /// </summary>
    /// <param name="rowIndex">Номер строки</param>
    /// <returns>Значение поля "Id"</returns>
    public Int32 GetRowId(int rowIndex)
    {
      DataRow Row = GetDataRow(rowIndex);
      if (Row == null)
        return 0;
      int p = Row.Table.Columns.IndexOf("Id");
      if (p < 0) // 10.03.2016
        return 0;
      return DataTools.GetInt(Row[p]);
    }


    #endregion

    #region Доступ к выбранным строкам

    /// <summary>
    /// Расширение свойства SelectedRows для представления выбранных строк в виде
    /// массива идентификаторов Id
    /// </summary>
    public virtual Int32[] SelectedIds
    {
      get
      {
        DataRow[] Rows = SelectedDataRows;
        // return DataTools.GetIds(Rows);
        return DataTools.GetIdsFromColumn(Rows, "Id"); // 21.01.2016
      }
      set
      {
        DataRow[] Rows = DataTools.GetRowsFromIds(SourceAsDataTable, value);
        SelectedDataRows = Rows;
      }
    }

    #endregion

    #region Восстановление текущей позиции

    /// <summary>
    /// Управляющее свойство.
    /// Если установлено в true (по умолчанию), то при выводе элемента на экран устанавливается значение
    /// CurrentId, сохраненное в конфигурационных данных. Установка не выполняется, если свойство CurrentId
    /// предварительно было установлено в явном виде. При закрытии просмотра выполняется сохранение значения
    /// Игнорируется, если свойство ConfigSectionName не установлено
    /// Свойство SaveCurrentId можно устанавливать только до вывода элемента на экран
    /// Данные хранятся в секции конфигурации с категорией "GridView" в числовом поле "CurrentId"
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
    /// Вызывается при показе элемента.
    /// Выполняет отложенную установку свойства CurrentId.
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      // 04.07.2021
      // Обычно этот вызов срабатывает.
      // Но если источник данных еще не готов, то установка строки будет выполнена в HandleIdle()
      FlushDelayedId();
    }

    #endregion

    #region UpdateRows

    /// <summary>
    /// Обновление строк просмотра
    /// </summary>
    /// <param name="rowIndices">Массив номеров строк в просмотре или null, если требуется обновить все строки</param>
    public override void UpdateRows(int[] rowIndices)
    {
      if (Control.DataSource != null) // 18.08.2021
      {
        DataRow[] Rows;
        if (rowIndices == null)
        {
          DataView dv = SourceAsDataView;
          if (dv == null)
            throw new InvalidOperationException("Табличный просмотр не присоединен к DataView");
          Rows = DataTools.GetDataViewRows(dv);
        }
        else
          Rows = base.GetDataRows(rowIndices);

        Int32[] Ids = DataTools.GetIdsFromColumn(Rows, "Id");
        if (Ids.Length > 0)
          ClearCacheForUpdate(Ids); // 24.10.2017

        for (int i = 0; i < Rows.Length; i++)
        {
          if (Rows[i] != null)
            LoadDataRowForUpdate(Rows[i]);
        }
      }

      base.UpdateRows(rowIndices);
    }

    /// <summary>
    /// Очистка кэша для выбранных идентификаторов.
    /// Вызывается однократно перед вызовами LoadDataRowForUpdate()
    /// </summary>
    /// <param name="ids">Массив идентификаторов. Длина массива больше 0</param>
    protected virtual void ClearCacheForUpdate(Int32[] ids)
    {
    }

    /// <summary>
    /// Обновление строки табличного просмотра из кэша.
    /// Если метод не переопределен, никаких действий не выполняется
    /// </summary>
    /// <param name="row">Строка данных. Не может быть null</param>
    protected virtual void LoadDataRowForUpdate(DataRow row)
    {
    }


    /// <summary>
    /// Пометить на обновление строки с заданными идентификаторами
    /// </summary>
    /// <param name="ids">Массив идентификаторов документов</param>
    public void UpdateRowsForIds(Int32[] ids)
    {
      IdList IdList = new IdList(ids);
      UpdateRowsForIds(IdList);
    }

    /// <summary>
    /// Пометить на обновление строки с заданными идентификаторами
    /// </summary>
    /// <param name="ids">Список идентификаторов документов</param>
    public void UpdateRowsForIds(IdList ids)
    {
      if (ids == null)
        return;
      if (ids.Count == 0)
        return;

      if (Control.DataSource == null)
        return; // 03.04.2020

      if (SourceAsDataView == null)
        throw new InvalidOperationException("Просмотр не связан с DataView");

      if (IsPrimaryKeyById)
      {
        List<DataRow> Rows = new List<DataRow>(ids.Count);
        foreach (Int32 Id in ids)
        {
          DataRow Row = SourceAsDataTable.Rows.Find(Id);
          if (Row != null)
            Rows.Add(Row);
        }
        UpdateDataRows(Rows.ToArray());
        return;
      }

      List<int> RowIndices = new List<int>();

      for (int i = 0; i < Control.RowCount; i++)
      {
        Int32 Id = GetRowId(i);
        if (ids.Contains(Id))
          RowIndices.Add(i);
      }

      if (RowIndices.Count > 0)
        UpdateRows(RowIndices.ToArray());
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Свойство возвращает true, если табличный просмотр присоединен к таблицы с первичным ключом
    /// по полю "Id"
    /// </summary>
    public bool IsPrimaryKeyById
    {
      get
      {
        if (SourceAsDataTable == null)
          return false;

        return String.Compare(DataTools.GetPrimaryKey(SourceAsDataTable), "Id", StringComparison.OrdinalIgnoreCase) == 0;
      }
    }

#if XXX
    /// <summary>
    /// Получение списка имен полей для всех столбцов, разделенный запятыми. 
    /// Предназначен для создания SQL-запроса сервером. Имена без квадратных скобок
    /// </summary>
    /// <returns>CSV-строка с именами. Не содержит пробелов</returns>
    public string GetColumnNames()
    {
      string s = String.Empty;
      for (int i = 0; i < Control.Columns.Count; i++)
      {
        string ThisName = Control.Columns[i].DataPropertyName;
        if (String.IsNullOrEmpty(ThisName))
          continue;
        if (s.Length > 0)
          s += ",";
        s += ThisName;
      }
      return s;
    }
#endif

    event EventHandler IEFPDBxView.SelectionChanged
    {
      add { Control.CurrentCellChanged += value; }
      remove { Control.CurrentCellChanged -= value; }
    }

    #endregion

    #region Переопределенные методы для работы фильтра

    /// <summary>
    /// Коллекция фильтров для табличного просмотра. Если есть хотя бы один фильтр,
    /// то в локальном меню появляется команда "Фильтр". После установки пользователем
    /// фильтра вызывается обновление просмотра. Ответственность за обработку
    /// фильтров лежит на вызывающей программе
    /// Чтобы проверить наличие возможных фильтров, следует использовать свойство
    /// HasFilters, которое позволяет избежать создания лишних объектов
    /// </summary>
    public new GridFilters Filters
    {
      get
      {
        return (GridFilters)(base.Filters);
      }
      set
      {
        base.Filters = value;
      }
    }

    /// <summary>
    /// Создает GridFilters
    /// </summary>
    /// <returns>Фильтры табличного просмотра</returns>
    protected override IEFPGridFilters CreateGridFilters()
    {
      return new GridFilters();
    }

    /// <summary>
    /// Вызывается для дополнительной инициализации табличного просмотра в редакторе фильтров
    /// Переопределеннный метод может, например, инициализировать дополнительные команды меню
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
      EFPDataGridViewCommandItems CommandItems = (EFPDataGridViewCommandItems)sender;
      int[] FilterIndices = CommandItems.Owner.SelectedRowIndices;

      DBxDocSelection DocSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      for (int i = 0; i < FilterIndices.Length; i++)
      {
        IDBxDocSelectionFilter Item2 = Filters[FilterIndices[i]] as IDBxDocSelectionFilter;
        if (Item2 != null)
          Item2.GetDocSel(DocSel);
      }
      if (!DocSel.IsEmpty)
      {
        args.DataObject.SetData(DocSel);
        UI.OnAddCopyFormats(args.DataObject, DocSel);
      }
    }

    void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      EFPDataGridView efpFilterGrid = (EFPDataGridView)(fmtDocSel.Tag);

      if (!efpFilterGrid.CheckSingleRow())
        return;

      DBxCommonFilter Item = Filters[efpFilterGrid.CurrentRowIndex];
      IDBxDocSelectionFilter Item2 = Item as IDBxDocSelectionFilter;
      if (Item2 == null)
      {
        EFPApp.ShowTempMessage("Фильтр \"" + Item.DisplayName + "\" не поддерживает вставку выборки документов");
        return;
      }

      if (Item2.ApplyDocSel(fmtDocSel.DocSel))
        efpFilterGrid.PerformRefresh();
      else
        EFPApp.ShowTempMessage("Выборка документов в буфере обмена не подходит фильтру \"" + Item.DisplayName + "\"");
    }

    #endregion

    #region Доступ к выбранным ячейкам для источника данных DataView или DataTable

    /// <summary>
    /// Получить список полей, которые используются табличным просмотром.
    /// Собирает значения свойств DataGridViewColumn.DataPropertyName
    /// </summary>
    /// <param name="columnNames">Коллекция имен полей, куда добавляются значения</param>
    public void GetUsedColumns(DBxColumnList columnNames)
    {
      // Проверяем, что у нас есть все поля. Обработчик InitGrid мог добавить
      // столбцы
      for (int i = 0; i < Control.Columns.Count; i++)
      {
        DataGridViewColumn Column = Control.Columns[i];
        if (Columns[Column].ColumnProducer != null)
          continue;
        if (!String.IsNullOrEmpty(Column.DataPropertyName))
          columnNames.Add(Column.DataPropertyName);
      }
    }

    #endregion

    #region Расширение GridProducer

    /***************************************************************************************
     * Команда лоокального меню "Настройка просмотра" доступна при установке свойств
     * GridProducer и ConfigSectionName
     * 
     * Способы инициализации столбцов для табличного просмотра EFPDBxGridView
     *
     * 1. Без использования GridProducer.
     *    Столбцы добавляются методами Columns.AddXXX() после создания EFPDBxGridView.
     *    Свойство GridProducer не используется.
     *    Пользователь не может выбирать столбцы
     *    Этот способ может использоваться для любого EFPDataGridView
     * 
     * 2. Собственный GridProducer.
     *    Создается и заполняется объект GridProducer. После создания EFPDBxGridView 
     *    устанавливается свойство GridProducer. Также должно быть присвоено значение свойству
     *    ConfigSectionName.
     *    Пользователь может настраивать конфигурацию, которая сохраняется между вызовами.
     *    Если задать свойство DefaultConfigName, то, при первом открытии просмотра пользователем
     *    используется не настройка по умолчанию (GridProducer.DefaultConfig), а настройка с заданным
     *    именем. При повторном открытии просмотра будет восстановлена последняя сохраненная конфигурация 
     *    и свойство DefaultConfigName игнорируется. Пользователь может всегда выбрать любой готовый набор
     *    настроек.
     *    В качестве GridProducer можно использовать и DocTypeUI/SubDocTypeUI.GridProducer. При этом команды
     *    локального меню, связанного с документами/поддокументами не добавляется.
     *    Программа должна предоставлять источник данных, содержащий выбранные поля. Если таблица получается
     *    путем обращения к базе данных, может быть невыгодно, если запрос всегда возвращает все возможные
     *    поля. В этом случае следует:
     *    - Обработать событие CurrentConfigChanged
     *    - В нем получить список требуемых полей. Для этого вызвать GridProducer.GetColumnNames(), создав
     *      пустой DBxColumnList и передав текущую настройку:
     *      DBxColumnList lst=new DBxColumnList();
     *      ((EFPDBxGridView)Sender).GridProducer.GetColumnNames(((EFPDBxGridView)Sender).CurrentConfig, lst);
     *    - Загрузить таблицу с требуемым набором столбцов 
     *    - В том же обработчике установить свойство DataGridView.DataSource 
     * 
     * 3. Использование просмотра документа/поддокумента.
     *    Обычно в этом случае стоит использовать специализированные просмотры EFPDocGridView/EFPSubDocGridView.
     *    Они подддерживают, в том числе, и наборы, в которых не для всех строк задано значение Id (однако
     *    поле обязательно должно присутствовать в таблице).
     *    Если все-таки нужна собственная реализация просмотра, аналогичного EFPDocGridView, то следует:
     *    - Установить свойства GridProducer и ConfigSectionName, как в способе 2.
     *    - Обработать событие CurrentConfigChanged
     *    - В нем вызвать DocTypeUI/SubDocTypeUI.DoInitGrid(). В качестве ReInit следует использовать
     *      свойство EFPDBxGridView.HasBeenShown. При этом также создается список требуемых полей. 
     *    Далее, снова как в способе 2:
     *    - Загрузить таблицу с требуемым набором столбцов 
     *    - В том же обработчике установить свойство DataGridView.DataSource 
     * 
     * 4. Использование просмотра документа/поддокумента без возможности настройки просмотра.
     *    Этот вариант может быть полезным, если предполагается работа неквалифицированного пользователя.
     *    В этом случае свойство GridProducer не должно устанавливаться. Вместо этого метод
     *    DocTypeUI/SubDocTypeUI.DoInitGrid() должен быть вызван после конструктора EFPDBxGridView.
     *    Обработчик события CurrentConfigChanged ничем не отличается от способа 3
     * 
     ***************************************************************************************/


    /// <summary>
    /// Возвращает true, если указанная строка является подходящей для получения всплывающей подсказки с помощью GridProducer
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <returns></returns>
    protected override bool GetGridProducerToolTipRequired(int rowIndex)
    {
      DataRow Row = GetDataRow(rowIndex);
      if (Row == null)
        return false;
      if (Row.RowState == DataRowState.Deleted)
        return false;

      int p = Row.Table.Columns.IndexOf("Id");
      if (p >= 0)
      {
        return (DataTools.GetInt(Row[p]) > 0); // условие добавлено 12.08.2020
      }
      else
      {
        // 28.08.2020
        // Если в таблице нет поля Id совсем, значит оно не критично для вывода подсказок
        return true;
      }
    }

    #endregion

    #region Выбор и настройка конфигурации просмотра

#if XXX // Убрано 28.06.2021. Если это и нужно, то в EFPConfigurableDataGridView

    /// <summary>
    /// Сохранить текущую конфигурацию просмотра.
    /// Вызывается при закрытии окна табличного просмотра и при закрытии диалога настройки просмотра.
    /// На момент вызова свойства GridProducer и CurrentConfig должны быть установлены, иначе сохранение не будет выполнено
    /// </summary>
    /// <returns>true, если сохранение было выполнено</returns>
    public bool PerformSaveCurrentConfig()
    {
      if (GridProducer == null)
        return false;
      if (CurrentConfig == null)
        return false;
      // Текущая конфигурация с учетом реальных размеров столбцов
      EFPDataGridViewConfig Config2 = CurrentConfig.Clone(this);

      WriteGridConfig(Config2, String.Empty);
      ConfigHandler.Changed[EFPConfigCategories.GridConfig] = false;
      return true;
    }


    internal EFPDataGridViewConfig ReadGridConfig(string configName)
    {
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, EFPConfigCategories.GridConfig, configName);
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
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, EFPConfigCategories.GridConfig, configName);
      CfgPart cfg;
      using (ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
      {
        config.WriteConfig(cfg);
      }
    }

#endif

    #endregion

    #region Порядок строк

    /// <summary>
    /// Описатели команд меню "Порядок строк" - массив объектов, задающих режимы
    /// сортировки
    /// </summary>
    public new EFPDBxViewOrders Orders { get { return (EFPDBxViewOrders)(base.Orders); } }

    /// <summary>
    /// Создает объект EFPDBxGridViewOrders
    /// </summary>
    /// <returns>Список порядков сортировки строк табличного просмотра</returns>
    protected override EFPDataViewOrders CreateOrders()
    {
      return new EFPDBxViewOrders();
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPDBxGridViewCommandItems CommandItems { get { return (EFPDBxGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает EFPDBxGridViewCommandItems
    /// </summary>
    /// <returns>Команды локального меню</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPDBxGridViewCommandItems(this);
    }

    #endregion

    #region Событие GetDocSel

    /// <summary>
    /// Если обработчик установлен, то при копировании ячеек в буфер обмена будет
    /// помещена выборка документоа (объект DBxDocSelection).
    /// Также будет добавлена команда "Отправить" -> "Выборка"
    /// </summary>
    public event EFPDBxGridViewDocSelEventHandler GetDocSel;

    /// <summary>
    /// Вызывает обработчик события GetDocSel, если он установлен.
    /// В случае переопределения метода также должно быть переопределено свойство HasGetDocSelHandler
    /// </summary>
    /// <param name="args">Аргументы события GetDocSel</param>
    protected virtual void OnGetDocSel(EFPDBxGridViewDocSelEventArgs args)
    {
      if (GetDocSel != null)
        GetDocSel(this, args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик GetDocSel
    /// </summary>
    public virtual bool HasGetDocSelHandler { get { return GetDocSel != null; } }

    /// <summary>
    /// Создание выборки документов по текущим выбранным строкам просмотра
    /// </summary>
    /// <param name="reason">Причина создания выборки (для команды "Отправить" или копирования в буфер обмена)</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection CreateDocSel(EFPDBxGridViewDocSelReason reason)
    {
      return CreateDocSel(reason, null);
    }

    /// <summary>
    /// Создание выборки документов по заданным строкам просмотра
    /// </summary>
    /// <param name="reason">Причина создания выборки (для команды "Отправить" или копирования в буфер обмена)</param>
    /// <param name="rowIndices">Массив индексов строк просмотра.
    /// Если null, то будут использованы выбранные строки просмотра (свойство SelectedRowIndices)</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection CreateDocSel(EFPDBxGridViewDocSelReason reason, int[] rowIndices)
    {
      //if (GetDocSel == null)
      if (!HasGetDocSelHandler) // 22.02.2018
        return null;

      DBxDocSelection DocSel = null;
      try
      {
        EFPApp.BeginWait("Создание выборки документов", "Выборка");
        try
        {
          EFPDBxGridViewDocSelEventArgs Args = new EFPDBxGridViewDocSelEventArgs(this, reason, rowIndices);
          OnGetDocSel(Args);
          if (!Args.DocSel.IsEmpty)
            DocSel = Args.DocSel;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка создания выборки документов для табличного просмотра");
      }
      return DocSel;
    }

    #endregion

    #region Столбец для отметки строк с помощью флажков

    /// <summary>
    /// Список для отметки строк по идентификаторам.
    /// В DBxDocGridView и DBxSubDocGridView установите свойство и столбец будет добавлен в просмотр автоматически.
    /// Для собственной реализации класса вызывайте AddMarkRowsColumn() для добавления столбца с флажками.
    /// Установка свойства в процессе показа, приводит к обновлению столбца просмотра.
    /// </summary>
    public IdList MarkRowIds
    {
      get { return _MarkRowIds; }
      set
      {
        if (_MarkRowIds != null)
        {
          if (value == null)
            value = new IdList();
        }
        _MarkRowIds = value;
        if (base.MarkRowsColumnIndex >= 0)
        {
          Control.InvalidateColumn(MarkRowsColumnIndex);
          base.MarkRowsGridColumn.ReadOnly = _MarkRowIds.IsReadOnly;
        }
      }
    }
    private IdList _MarkRowIds;

    /// <summary>
    /// Добавить столбец с флажками для отметки строк.
    /// В просмотрах DBxDocGridView и DBxSubDocGridView этот метод вызывать не следует, т.к.
    /// столбец добавляется автоматически, если установлено свойство MarkRowIds.
    /// Вызов этого метода инициализирует свойство MarkRowIds, если это не было сделано ранее.
    /// Для доступа к столбцу можно использовать свойства MarkRowsColumn или MarkRowsGridColumn.
    /// </summary>
    public void AddMarkRowsColumn()
    {
      if (MarkRowIds == null)
        MarkRowIds = new IdList();

      bool FirstCall = base.MarkRowsGridColumn == null;
      base.MarkRowsGridColumn = base.Columns.AddBool("_MarkRows", false, String.Empty);
      base.MarkRowsGridColumn.ReadOnly = MarkRowIds.IsReadOnly;
      if (FirstCall)
      {
        Control.VirtualMode = true;
        Control.CellValueNeeded += new DataGridViewCellValueEventHandler(Control_CellValueNeeded);
        Control.CellValuePushed += new DataGridViewCellValueEventHandler(Control_CellValuePushed);
      }
    }

    private static bool _CellValueNeededExceptionLogged = false;

    void Control_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args)
    {
      try
      {
        DataGridViewColumn Col = Control.Columns[args.ColumnIndex];
        if (Object.ReferenceEquals(Col, base.MarkRowsGridColumn))
        {
          Int32 CurrId = this.GetRowId(args.RowIndex);
          if (CurrId != 0)
            args.Value = MarkRowIds.Contains(CurrId);
        }
      }
      catch (Exception e)
      {
        if (!_CellValueNeededExceptionLogged)
        {
          _CellValueNeededExceptionLogged = true;
          LogoutTools.LogoutException(e, "EFPDBxGridView.CellValueNeeded");
        }
      }
    }

    void Control_CellValuePushed(object sender, DataGridViewCellValueEventArgs args)
    {
      try
      {
        DataGridViewColumn Col = Control.Columns[args.ColumnIndex];
        if (Object.ReferenceEquals(Col, base.MarkRowsGridColumn))
        {
          Int32 CurrId = this.GetRowId(args.RowIndex);
          if (CurrId != 0)
          {
            bool Flag = DataTools.GetBool(args.Value);
            if (Flag)
              MarkRowIds.Add(CurrId);
            else
              MarkRowIds.Remove(CurrId);
          }
          else
            throw new InvalidOperationException("Для строки нет идентификатора");
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка установки маркировки строки");
      }
    }

    #endregion

    #region Поиск текста

    /// <summary>
    /// Добавляет кнопку "Выборка документов" в диалог поиска текста
    /// </summary>
    /// <returns>Объект EFPDBxGridViewSearchContext</returns>
    protected override IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPDBxGridViewSearchContext(this);
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для EFPDBxGridView.
  /// Добавляет команды "Отправить" - "Выборка документов"
  /// </summary>
  public class EFPDBxGridViewCommandItems : EFPConfigurableDataGridViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Вызывается из EFPDBxGridView
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDBxGridViewCommandItems(EFPDBxGridView controlProvider)
      : base(controlProvider)
    {
      #region Исходные значения свойств

      //???// Главное меню было инициализировано ?
      //???bool HasMainMenu = (AccDepMainMenu.SelectAll != null);

      #endregion

      #region Создание команд

#if XXX
      #region Настройка просмотра

      MenuCfg = new EFPCommandItem("Вид", "МенюНастройкиПросмотра");
      MenuCfg.MenuText = "Настройка просмотра";
      MenuCfg.ImageKey = "НастроитьКонфигурациюПросмотра";
      MenuCfg.Usage = EFPCommandItemUsage.Menu;
      Add(MenuCfg);

      ciCfgSelect = new EFPCommandItem("Вид", "ВыбратьКонфигурациюПросмотра");
      ciCfgSelect.Parent = MenuCfg;
      ciCfgSelect.MenuText = "Выбрать конфигурацию ...";
      ciCfgSelect.ImageKey = "ВыбратьКонфигурациюПросмотра";
      ciCfgSelect.Click += new EventHandler(ciCfgSelect_Click);
      ciCfgSelect.Usage = EFPCommandItemUsage.Menu;
      Add(ciCfgSelect);

      ciCfgEdit = new EFPCommandItem("Вид", "НастроитьКонфигурациюПросмотра");
      ciCfgEdit.Parent = MenuCfg;
      ciCfgEdit.MenuText = "Настроить ...";
      ciCfgEdit.ImageKey = "НастроитьКонфигурациюПросмотра";
      ciCfgEdit.Click += new EventHandler(ciCfgEdit_Click);
      ciCfgEdit.Usage = EFPCommandItemUsage.Menu;
      Add(ciCfgEdit);

      ciCfgSave = new EFPCommandItem("Вид", "СохранитьКонфигурациюПросмотра");
      ciCfgSave.Parent = MenuCfg;
      ciCfgSave.MenuText = "Сохранить";
      ciCfgSave.ImageKey = "СохранитьКонфигурациюПросмотра";
      ciCfgSave.Click += new EventHandler(ciCfgSave_Click);
      ciCfgSave.Usage = EFPCommandItemUsage.Menu;
      Add(ciCfgSave);

      ciCfgSaveAs = new EFPCommandItem("Вид", "СохранитьКонфигурациюПросмотраКак");
      ciCfgSaveAs.Parent = MenuCfg;
      ciCfgSaveAs.MenuText = "Сохранить как ...";
      //ciCfgSaveAs.ImageKey = "СохранитьКонфигурациюПросмотра";
      ciCfgSaveAs.Click += new EventHandler(ciCfgSaveAs_Click);
      ciCfgSaveAs.Usage = EFPCommandItemUsage.Menu;
      Add(ciCfgSaveAs);

      ciCfgManager = new EFPCommandItem("Вид", "МенеджерКонфигурацийПросмотра");
      ciCfgManager.Parent = MenuCfg;
      ciCfgManager.GroupBegin = true;
      ciCfgManager.MenuText = "Менеджер настроек ...";
      ciCfgManager.ImageKey = "МенеджерКонфигурацийПросмотра";
      ciCfgManager.Click += new EventHandler(ciCfgManager_Click);
      ciCfgManager.Usage = EFPCommandItemUsage.Menu;
      Add(ciCfgManager);

      InitCurrentCfgName();
      InitCurrentCfgModified();

      #endregion
#endif
      AddSeparator();

      #region Отправить

      ciSendTo = new EFPCommandItem("Send", "DocSel");
      ciSendTo.MenuText = "Выборка документов";
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
    public new EFPDBxGridView Owner { get { return (EFPDBxGridView)(base.Owner); } }

    /// <summary>
    /// Установка свойств EFPCommandItem.Usage
    /// </summary>
    protected override void BeforeControlAssigned()
    {
      base.BeforeControlAssigned();

      if (!Owner.HasGetDocSelHandler) // Есть обработчик
        ciSendTo.Usage = EFPCommandItemUsage.None;
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Добавляет выборку DBxDocSelection в список форматов для буфера обмена.
    /// </summary>
    /// <param name="args">Аргументы для добавления форматоы</param>
    protected override void OnAddCopyFormats(DataObjectEventArgs args)
    {
      DBxDocSelection DocSel = Owner.CreateDocSel(EFPDBxGridViewDocSelReason.Copy);
      if (DocSel != null)
      {
        args.DataObject.SetData(DocSel);
        Owner.UI.OnAddCopyFormats(args.DataObject, DocSel); // 06.02.2021
      }

      base.OnAddCopyFormats(args);
    }

    #endregion

    #region Отправить

    EFPCommandItem ciSendTo;

    private void ciSendToDocSel_Click(object sender, EventArgs args)
    {
      DBxDocSelection DocSel = Owner.CreateDocSel(EFPDBxGridViewDocSelReason.SendTo);
      if (DocSel == null || DocSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("Выборка не содержит документов");
        return;
      }
      Owner.UI.ShowDocSel(DocSel);
    }

    #endregion
  }


  /// <summary>
  /// Страница отчета, содержащая таблицу EFPDBxGridView
  /// </summary>
  public class EFPReportDBxGridPage : EFPReportStdConfigurableGridPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    /// <param name="ui">Интерфейс для доступа к документам</param>
    public EFPReportDBxGridPage(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      //base.ShowToolBar = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region Табличный просмотр

    /// <summary>
    /// Переопределенное свойство - провайдер табличного просмотра
    /// </summary>
    public new EFPDBxGridView ControlProvider
    {
      get { return (EFPDBxGridView)(base.ControlProvider); }
    }

    /// <summary>
    /// Создает EFPDBxGridView
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Провайдер табличного просмотра</returns>
    protected override EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      EFPDBxGridView Res = new EFPDBxGridView(BaseProvider, control, _UI);
      return Res;
    }

    #endregion
  }
}
