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
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  #region EFPDBxGridViewDocSelEventArgs

  #region Перечисление

  /// <summary>
  /// Причина создания выборки документов для выбранных строк табличного просмотра
  /// (пользователь выбрал команду "Отправить" или копирование ячеек в буфер).
  /// Свойство <see cref="EFPDBxViewDocSelEventArgs.Reason"/>.
  /// </summary>
  public enum EFPDBxViewDocSelReason
  {
    /// <summary>
    /// Пользователь выполнил команду "Отправить"
    /// Обработчик может вывести блок диалога, если возможны варианты создания
    /// выборки
    /// </summary>
    SendTo,

    /// <summary>
    /// Выполняется копирование ячеек в буфер обмена.
    /// Обработчик не должен взаимодействовать с пользователем.
    /// </summary>
    Copy,
  }

  #endregion

  /// <summary>
  /// Базовый класс аргументов события для <see cref="EFPDBxGridViewDocSelEventArgs"/> и <see cref="EFPDBxTreeViewDocSelEventArgs"/>
  /// </summary>
  public abstract class EFPDBxViewDocSelEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается провайдером управляющего элемента, для которого генерируется событие.
    /// Не используется в прикладном коде.
    /// </summary>
    /// <param name="controlProvider"></param>
    /// <param name="reason"></param>
    public EFPDBxViewDocSelEventArgs(IEFPDBxView controlProvider, EFPDBxViewDocSelReason reason)
    {
      _ControlProvider = controlProvider;
      _DocSel = new DBxDocSelection(controlProvider.UI.DocProvider.DBIdentity);
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к табличному просмотру
    /// </summary>
    public IEFPDBxView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPDBxView _ControlProvider;

    /// <summary>
    /// Строки таблицы данных, для которых должна быть получена выборка.
    /// Если табличный просмотр не привязан к таблице данных, то массив содержит
    /// значения null. 
    /// </summary>
    public DataRow[] DataRows
    {
      get
      {
        if (_DataRows == null)
          _DataRows = CreateDataRows();
        return _DataRows;
      }
    }
    private DataRow[] _DataRows;

    /// <summary>
    /// Создает массив строк
    /// </summary>
    /// <returns></returns>
    protected abstract DataRow[] CreateDataRows();

    /// <summary>
    /// Причина, по которой требуется создать выборку
    /// </summary>
    public EFPDBxViewDocSelReason Reason { get { return _Reason; } }
    private readonly EFPDBxViewDocSelReason _Reason;

    /// <summary>
    /// Сюда должны быть добавлены ссылки на документы
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private readonly DBxDocSelection _DocSel;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить ссылки из поля таблицы.
    /// Проверяется наличие в таблице <see cref="DataRows"/>[0].Table поля <paramref name="columnName"/>; если поля
    /// нет, то ничего не выполняется.
    /// Для извлечения идентификаторов используется <see cref="DataTools.GetIdsFromColumn(ICollection{DataRow}, string)"/>.
    /// Добавляется только ссылка на документ, без вызова обработчика <see cref="DocTypeUIBase.GetDocSel"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа <see cref="DocTypeUI"/></param>
    /// <param name="columnName">Имя ссылочного поля в таблице просмотра</param>
    public void AddFromColumn(string tableName, string columnName)
    {
      AddFromColumn(tableName, columnName, false);
    }
    /// <summary>
    /// Добавить ссылки из поля таблицы
    /// Проверяется наличие в таблице <see cref="DataRows"/>[0].Table поля <paramref name="columnName"/>; если поля
    /// нет, то ничего не выполняется.
    /// Для извлечения идентификаторов используется <see cref="DataTools.GetIdsFromColumn(ICollection{DataRow}, string)"/>.
    /// Если UseHandler=true, то используется обработчик <see cref="DocTypeUIBase.GetDocSel"/>,
    /// при этом могут быть добавлены дополнительные ссылки. Если <paramref name="useHandler"/>=false,
    /// то добавляется только ссылка на документ.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа <see cref="DocTypeUI"/></param>
    /// <param name="columnName">Имя ссылочного поля в таблице просмотра</param>
    /// <param name="useHandler">Если true, то используется обработчик <see cref="DocTypeUIBase.GetDocSel"/></param>
    public void AddFromColumn(string tableName, string columnName, bool useHandler)
    {
      if (DataRows.Length == 0)
        return; // 10.01.2019

      if (!DataRows[0].Table.Columns.Contains(columnName))
        return;

      Int32[] ids = DataTools.GetIdsFromColumn(DataRows, columnName);
      if (useHandler)
      {
        DocTypeUIBase dtb = ControlProvider.UI.DocTypes.FindByTableName(tableName);
        if (dtb == null)
          throw new ArgumentException("Неизвестный вид документа или поддокумента \"" + tableName + "\"", "tableName");

        dtb.PerformGetDocSel(DocSel, ids, Reason);
      }
      else
        DocSel.Add(tableName, ids);
    }

    /// <summary>
    /// Загрузить из полей переменной ссылки "TableId" и "DocId" (имена полей задаются).
    /// Добавляется только ссылка на документ, без вызова обработчика <see cref="DocTypeUIBase.GetDocSel"/>
    /// </summary>
    /// <param name="tableIdColumnName">Имя поля, содержащего идентификатор таблицы документов (свойство <see cref="DBxDocType.TableId"/>)</param>
    /// <param name="docIdColumnName">Имя поля, содержащего идентификатор документа</param>
    public void AddFromVTReference(string tableIdColumnName, string docIdColumnName)
    {
      AddFromVTReference(tableIdColumnName, docIdColumnName, false);
    }

    /// <summary>
    /// Загрузить из полей переменной ссылки "TableId" и "DocId" (имена полей задаются).
    /// Если UseHandler=true, то используется обработчик <see cref="DocTypeUIBase.GetDocSel"/>,
    /// при этом могут быть добавлены дополнительные ссылки. Если <paramref name="useHandler"/>=false,
    /// то добавляется только ссылка на документ.
    /// </summary>
    /// <param name="tableIdColumnName">Имя поля, содержащего идентификатор таблицы документов (свойство <see cref="DBxDocType.TableId"/>)</param>
    /// <param name="docIdColumnName">Имя поля, содержащего идентификатор документа</param>
    /// <param name="useHandler">Если true, то используется обработчик <see cref="DocTypeUIBase.GetDocSel"/></param>
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
        Int32 tableId = DataTools.GetInt(DataRows[i][pTable]);
        Int32 docId = DataTools.GetInt(DataRows[i][pId]);
        if (tableId != 0 && docId != 0)
        {
          DocTypeUI dtui = ControlProvider.UI.DocTypes.FindByTableId(tableId);
          if (dtui != null)
          {
            if (useHandler)
              dtui.PerformGetDocSel(DocSel, docId, Reason);
            else
              DocSel.Add(dtui.DocType.Name, docId);
          }
        }
      }
    }

    #endregion
  }

  ///// <summary>
  ///// Делегат события <see cref="IEFPDBxView.GetDocSel"/>
  ///// </summary>
  ///// <param name="sender">Провайдер управляющего элемента</param>
  ///// <param name="args">Аргументы события</param>
  //public delegate void EFPDBxViewDocSelEventHandler(object sender, EFPDBxViewDocSelEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="EFPDBxGridView.GetDocSel"/>
  /// </summary>
  public class EFPDBxGridViewDocSelEventArgs : EFPDBxViewDocSelEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается провайдером управляющего элемента, для которого генерируется событие.
    /// Не используется в прикладном коде.
    /// </summary>
    /// <param name="controlProvider"></param>
    /// <param name="reason"></param>
    /// <param name="rowIndices"></param>
    public EFPDBxGridViewDocSelEventArgs(EFPDBxGridView controlProvider, EFPDBxViewDocSelReason reason, int[] rowIndices)
      :base(controlProvider, reason)
    {
      _RowIndices = rowIndices;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к табличному просмотру
    /// </summary>
    public new EFPDBxGridView ControlProvider { get { return (EFPDBxGridView)(base.ControlProvider); } }

    /// <summary>
    /// Индексы строк, для которых требуется создать выборку документов
    /// </summary>
    public Int32[] RowIndices
    {
      get
      {
        if (_RowIndices == null)
          _RowIndices = ControlProvider.SelectedRowIndices;
        return _RowIndices;
      }
    }
    private Int32[] _RowIndices;

    /// <summary>
    /// Создание списка строк таблицы <see cref="DataTable"/>, соответствующий <see cref="RowIndices"/> 
    /// </summary>
    /// <returns>Массив строк</returns>
    protected override DataRow[] CreateDataRows()
    {
      DataRow[] rows = new DataRow[RowIndices.Length];
      for (int i = 0; i < rows.Length; i++)
        rows[i] = ControlProvider.GetDataRow(RowIndices[i]);
      return rows;
    }

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
            _GridRows[i] = ControlProvider.Control.Rows[RowIndices[i]];
        }
        return _GridRows;
      }
    }
    private DataGridViewRow[] _GridRows;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPDBxGridView.GetDocSel"/>
  /// </summary>
  /// <param name="sender">Провайдер <see cref="EFPDBxGridView"/> или производного класса</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDBxGridViewDocSelEventHandler(object sender,
    EFPDBxGridViewDocSelEventArgs args);

  #endregion

  #region Интерфейс IEFPDBxView

  /// <summary>
  /// Общая часть для просмотров <see cref="EFPDBxGridView"/> и <see cref="EFPDBxTreeView"/>.
  /// Интерфейс используется для синхронизации нескольких просмотров одного вида документов или поддокументов,
  /// расположенных в одном окне, с помощью <see cref="EFPDBxViewSync"/>.
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
    Int32 CurrentId { get; set; }

    /// <summary>
    /// Массив идентификаторов для выбранных строк таблицы или ущлов дерева
    /// </summary>
    Int32[] SelectedIds { get; set; }

    /// <summary>
    /// Событие вызывается, если пользователь изменяет выбор строк
    /// </summary>
    event EventHandler SelectionChanged;

    // Можно сделать событие, но невозможно сделать один event.
    // Не особенно нужно.
    ///// <summary>
    ///// Событие для создания выборки документов
    ///// </summary>
    //event EFPDBxViewDocSelEventHandler GetDocSel;

    /// <summary>
    /// Возвращает true, если в провайдере установлен обработчик события GetDocSel или
    /// есть другой способ создания выборки документов.
    /// </summary>
    bool HasGetDocSelHandler { get; }

    /// <summary>
    /// Создает выборку документов для отмеченных строк просмотра
    /// </summary>
    /// <param name="reason">Причина необходимости построения выборки</param>
    /// <returns>Созданная выборка документов</returns>
    DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason);

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

    /// <summary>
    /// Возвращает фильтры табличного просмотра
    /// </summary>
    new EFPDBxGridFilters Filters { get; set; }
  }

  #endregion

  /// <summary>
  /// Расширение класса <see cref="EFPDataGridView"/> для работы с таблицами базы данных.
  /// Источником данных <see cref="DataGridView.DataSource"/> должна быть таблица <see cref="DataTable"/> или просмотр <see cref="DataView"/>.
  /// Поддерживает идентификатор строки - поле Id. В <see cref="DataTable"/> могут быть строки с незаданным или
  /// нулевым идентификатором. В частности, это может быть табличный просмотр в отчете.
  /// Поддерживает объекты GridProducer и GridFilters, связанные с объектом <see cref="DBUI"/>.
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
    private /*readonly*/ DBUI _UI; // инициализируется в Init(), а не в конструкторе

    #endregion

    #region Чтение и запись настроек

    private Int32 _PrevSavedId;

    /// <summary>
    /// Помечает категорию "GridView" для сохранения текущей позиции в секции конфигурации
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (SaveCurrentId && (CurrentId != _PrevSavedId) && CurrentId != 0)
      {
        ConfigHandler.Changed[EFPConfigCategories.GridView] = true; // для сохранения текущей позиции
        _PrevSavedId = CurrentId;
      }

      base.OnSaveConfig();
    }

    /// <summary>
    /// Добавляет в список категорию "GridView", если свойство <see cref="SaveCurrentId"/> установлено в true.
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
    /// Идентификатор в текущей строке (поле "Id"). Используется отложенная
    /// установка свойства, если форма еще не выведена на экран.
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
    /// Строка должна быть Unshared.
    /// </summary>
    /// <param name="row">Объект строки</param>
    /// <returns>Значение поля "Id"</returns>
    private static Int32 GetRowId(DataGridViewRow row)
    {
      DataRow row2 = WinFormsTools.GetDataRow(row);
      if (row2 == null)
        return 0;
      int p = row2.Table.Columns.IndexOf("Id");
      if (p < 0) // 10.03.2016
        return 0;
      return DataTools.GetInt(row2[p]);
    }

    /// <summary>
    /// Получить идентификатор "Id" по номеру строки в табличном просмотре.
    /// Предпочтительный метод, т.к. не делает строку Unshared
    /// </summary>
    /// <param name="rowIndex">Номер строки</param>
    /// <returns>Значение поля "Id"</returns>
    public Int32 GetRowId(int rowIndex)
    {
      DataRow row = GetDataRow(rowIndex);
      if (row == null)
        return 0;
      int p = row.Table.Columns.IndexOf("Id");
      if (p < 0) // 10.03.2016
        return 0;
      return DataTools.GetInt(row[p]);
    }


    #endregion

    #region Доступ к выбранным строкам

    /// <summary>
    /// Расширение свойства <see cref="EFPDataGridView.SelectedDataRows"/> для представления выбранных строк в виде
    /// массива идентификаторов "Id".
    /// </summary>
    public virtual Int32[] SelectedIds
    {
      get
      {
        DataRow[] rows = SelectedDataRows;
        // return DataTools.GetIds(Rows);
        return DataTools.GetIdsFromColumn(rows, "Id"); // 21.01.2016
      }
      set
      {
        DataRow[] rows = DataTools.GetRowsFromIds(SourceAsDataTable, value);
        SelectedDataRows = rows;
      }
    }

    #endregion

    #region Восстановление текущей позиции

    /// <summary>
    /// Управляющее свойство.
    /// Если установлено в true (по умолчанию), то при выводе элемента на экран устанавливается значение
    /// <see cref="CurrentId"/>, сохраненное в конфигурационных данных. Установка не выполняется, если свойство <see cref="CurrentId"/>
    /// предварительно было установлено в явном виде. При закрытии просмотра выполняется сохранение значения.
    /// Игнорируется, если свойство <see cref="EFPControlBase.ConfigSectionName"/> не установлено.
    /// Свойство <see cref="SaveCurrentId"/> можно устанавливать только до вывода элемента на экран.
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
    /// Выполняет отложенную установку свойства <see cref="CurrentId"/>.
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
        DataRow[] rows;
        if (rowIndices == null)
        {
          DataView dv = SourceAsDataView;
          if (dv == null)
            throw new InvalidOperationException("Табличный просмотр не присоединен к DataView");
          rows = DataTools.GetDataViewRows(dv);
        }
        else
          rows = base.GetDataRows(rowIndices);

        Int32[] ids = DataTools.GetIdsFromColumn(rows, "Id");
        if (ids.Length > 0)
          ClearCacheForUpdate(ids); // 24.10.2017

        for (int i = 0; i < rows.Length; i++)
        {
          if (rows[i] != null)
            LoadDataRowForUpdate(rows[i]);
        }
      }

      base.UpdateRows(rowIndices);
    }

    /// <summary>
    /// Очистка кэша для выбранных идентификаторов.
    /// Вызывается однократно перед вызовами <see cref="LoadDataRowForUpdate(DataRow)"/>.
    /// </summary>
    /// <param name="ids">Массив идентификаторов. Длина массива больше 0</param>
    protected virtual void ClearCacheForUpdate(Int32[] ids)
    {
    }

    /// <summary>
    /// Обновление строки табличного просмотра из кэша.
    /// Если метод не переопределен, никаких действий не выполняется.
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
      IdList idList = new IdList(ids);
      UpdateRowsForIds(idList);
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
        List<DataRow> rows = new List<DataRow>(ids.Count);
        foreach (Int32 id in ids)
        {
          DataRow row = SourceAsDataTable.Rows.Find(id);
          if (row != null)
            rows.Add(row);
        }
        UpdateDataRows(rows.ToArray());
        return;
      }

      List<int> rowIndices = new List<int>();

      for (int i = 0; i < Control.RowCount; i++)
      {
        Int32 id = GetRowId(i);
        if (ids.Contains(id))
          rowIndices.Add(i);
      }

      if (rowIndices.Count > 0)
        UpdateRows(rowIndices.ToArray());
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Свойство возвращает true, если табличный просмотр присоединен к таблицы с первичным ключом
    /// по полю "Id".
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
    /// фильтров лежит на вызывающей программе.
    /// Чтобы проверить наличие возможных фильтров, следует использовать свойство
    /// <see cref="EFPConfigurableDataGridView.HasFilters"/>, которое позволяет избежать создания лишних объектов
    /// </summary>
    public new EFPDBxGridFilters Filters
    {
      get { return (EFPDBxGridFilters)(base.Filters); }
      set
      {
        if (value != null)
          value.SqlFilterRequired = true;
        base.Filters = value;
      }
    }

    /// <summary>
    /// Создает <see cref="EFPDBxGridFilters"/>.
    /// </summary>
    /// <returns>Фильтры табличного просмотра</returns>
    protected override IEFPGridFilters CreateGridFilters()
    {
      EFPDBxGridFilters filters = new EFPDBxGridFilters();
      filters.SqlFilterRequired = true;
      return filters;
    }

    /// <summary>
    /// Вызывается для дополнительной инициализации табличного просмотра в редакторе фильтров.
    /// Переопределеннный метод может, например, инициализировать дополнительные команды меню.
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
      int[] filterIndices = CommandItems.ControlProvider.SelectedRowIndices;

      DBxDocSelection docSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      for (int i = 0; i < filterIndices.Length; i++)
      {
        IDBxDocSelectionFilter item2 = Filters[filterIndices[i]] as IDBxDocSelectionFilter;
        if (item2 != null)
          item2.GetDocSel(docSel);
      }
      if (!docSel.IsEmpty)
      {
        args.DataObject.SetData(docSel);
        UI.OnAddCopyFormats(args.DataObject, docSel);
      }
    }

    void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      EFPDataGridView efpFilterGrid = (EFPDataGridView)(fmtDocSel.Tag);

      if (!efpFilterGrid.CheckSingleRow())
        return;

      DBxCommonFilter item = Filters[efpFilterGrid.CurrentRowIndex];
      IDBxDocSelectionFilter item2 = item as IDBxDocSelectionFilter;
      if (item2 == null)
      {
        EFPApp.ShowTempMessage("Фильтр \"" + item.DisplayName + "\" не поддерживает вставку выборки документов");
        return;
      }

      if (item2.ApplyDocSel(fmtDocSel.DocSel))
        efpFilterGrid.PerformRefresh();
      else
        EFPApp.ShowTempMessage("Выборка документов в буфере обмена не подходит фильтру \"" + item.DisplayName + "\"");
    }

    #endregion

    #region Доступ к выбранным ячейкам для источника данных DataView или DataTable

    /// <summary>
    /// Получить список полей, которые используются табличным просмотром.
    /// Собирает значения свойств <see cref="DataGridViewColumn.DataPropertyName"/>
    /// </summary>
    /// <param name="columnNames">Коллекция имен полей, куда добавляются значения</param>
    public void GetUsedColumns(DBxColumnList columnNames)
    {
      // Проверяем, что у нас есть все поля. Обработчик InitGrid мог добавить
      // столбцы
      for (int i = 0; i < Control.Columns.Count; i++)
      {
        DataGridViewColumn gridColumn = Control.Columns[i];
        if (Columns[gridColumn].ColumnProducer != null)
          continue;
        if (!String.IsNullOrEmpty(gridColumn.DataPropertyName))
          columnNames.Add(gridColumn.DataPropertyName);
      }
    }

    #endregion

    #region Расширение GridProducer

    /***************************************************************************************
     * Команда локального меню "Настройка просмотра" доступна при установке свойств
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
     *    Они поддерживают, в том числе, и наборы, в которых не для всех строк задано значение Id (однако
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
      DataRow row = GetDataRow(rowIndex);
      if (row == null)
        return false;
      if (row.RowState == DataRowState.Deleted)
        return false;

      int p = row.Table.Columns.IndexOf("Id");
      if (p >= 0)
      {
        return (DataTools.GetInt(row[p]) > 0); // условие добавлено 12.08.2020
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

    #region Команды локального меню

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPDBxGridViewCommandItems CommandItems { get { return (EFPDBxGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает <see cref="EFPDBxGridViewCommandItems"/>
    /// </summary>
    /// <returns>Команды локального меню</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPDBxGridViewCommandItems(this);
    }

    #endregion

    #region Событие GetDocSel

    /// <summary>
    /// Если обработчик установлен, то при копировании ячеек в буфер обмена будет
    /// помещена выборка документов (объект <see cref="DBxDocSelection"/>).
    /// Также будет добавлена команда "Отправить" -> "Выборка".
    /// </summary>
    public event EFPDBxGridViewDocSelEventHandler GetDocSel;

    /// <summary>
    /// Вызывает обработчик события <see cref="GetDocSel"/>, если он установлен.
    /// В случае переопределения метода также должно быть переопределено свойство <see cref="HasGetDocSelHandler"/>.
    /// </summary>
    /// <param name="args">Аргументы события GetDocSel</param>
    protected virtual void OnGetDocSel(EFPDBxGridViewDocSelEventArgs args)
    {
      if (GetDocSel != null)
        GetDocSel(this, args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик <see cref="GetDocSel"/>.
    /// </summary>
    public virtual bool HasGetDocSelHandler { get { return GetDocSel != null; } }

    /// <summary>
    /// Создание выборки документов по текущим выбранным строкам просмотра
    /// </summary>
    /// <param name="reason">Причина создания выборки (для команды "Отправить" или копирования в буфер обмена)</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason)
    {
      return CreateDocSel(reason, null);
    }

    /// <summary>
    /// Создание выборки документов по заданным строкам просмотра
    /// </summary>
    /// <param name="reason">Причина создания выборки (для команды "Отправить" или копирования в буфер обмена)</param>
    /// <param name="rowIndices">Массив индексов строк просмотра.
    /// Если null, то будут использованы выбранные строки просмотра (свойство <see cref="EFPDataGridView.SelectedRowIndices"/>).</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason, int[] rowIndices)
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
          EFPDBxGridViewDocSelEventArgs args = new EFPDBxGridViewDocSelEventArgs(this, reason, rowIndices);
          OnGetDocSel(args);
          if (!args.DocSel.IsEmpty)
            DocSel = args.DocSel;
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
    /// В <see cref="EFPDocGridView"/> и <see cref="EFPSubDocGridView"/> установите свойство и столбец будет добавлен в просмотр автоматически.
    /// Для собственной реализации класса вызывайте <see cref="AddMarkRowsColumn()"/> для добавления столбца с флажками.
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
    /// В просмотрах <see cref="EFPDocGridView"/> и <see cref="EFPSubDocGridView"/> этот метод вызывать не следует, т.к.
    /// столбец добавляется автоматически, если установлено свойство <see cref="MarkRowIds"/>.
    /// Вызов этого метода инициализирует свойство <see cref="MarkRowIds"/>, если это не было сделано ранее.
    /// Для доступа к столбцу можно использовать свойства <see cref="EFPDataGridView.MarkRowsColumn"/> или <see cref="EFPDataGridView.MarkRowsGridColumn"/>.
    /// </summary>
    public void AddMarkRowsColumn()
    {
      if (MarkRowIds == null)
        MarkRowIds = new IdList();

      bool isFirstCall = base.MarkRowsGridColumn == null;
      base.MarkRowsGridColumn = base.Columns.AddBool("_MarkRows", false, String.Empty);
      base.MarkRowsGridColumn.ReadOnly = MarkRowIds.IsReadOnly;
      if (isFirstCall)
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
        DataGridViewColumn gridColumn = Control.Columns[args.ColumnIndex];
        if (Object.ReferenceEquals(gridColumn, base.MarkRowsGridColumn))
        {
          Int32 currId = this.GetRowId(args.RowIndex);
          if (currId != 0)
            args.Value = MarkRowIds.Contains(currId);
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
        DataGridViewColumn gridColumn = Control.Columns[args.ColumnIndex];
        if (Object.ReferenceEquals(gridColumn, base.MarkRowsGridColumn))
        {
          Int32 currId = this.GetRowId(args.RowIndex);
          if (currId != 0)
          {
            bool flag = DataTools.GetBool(args.Value);
            if (flag)
              MarkRowIds.Add(currId);
            else
              MarkRowIds.Remove(currId);
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
    /// <returns>Объект <see cref="EFPDBxGridViewSearchContext"/></returns>
    protected override IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPDBxGridViewSearchContext(this);
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для <see cref="EFPDBxGridView"/>.
  /// Добавляет команды "Отправить" - "Выборка документов".
  /// </summary>
  public class EFPDBxGridViewCommandItems : EFPConfigurableDataGridViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Вызывается из <see cref="EFPDBxGridView"/>
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDBxGridViewCommandItems(EFPDBxGridView controlProvider)
      : base(controlProvider)
    {
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
    public new EFPDBxGridView ControlProvider { get { return (EFPDBxGridView)(base.ControlProvider); } }

    /// <summary>
    /// Установка свойств <see cref="EFPCommandItem.Usage"/>.
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      if (!ControlProvider.HasGetDocSelHandler) // Есть обработчик
        ciSendTo.Usage = EFPCommandItemUsage.None;
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Добавляет выборку <see cref="DBxDocSelection"/> в список форматов для буфера обмена.
    /// </summary>
    /// <param name="args">Аргументы для добавления форматоы</param>
    protected override void OnAddCopyFormats(DataObjectEventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxViewDocSelReason.Copy);
      if (docSel != null)
      {
        args.DataObject.SetData(docSel);
        ControlProvider.UI.OnAddCopyFormats(args.DataObject, docSel); // 06.02.2021
      }

      base.OnAddCopyFormats(args);
    }

    #endregion

    #region Отправить

    EFPCommandItem ciSendTo;

    private void ciSendToDocSel_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxViewDocSelReason.SendTo);
      if (docSel == null || docSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("Выборка не содержит документов");
        return;
      }
      ControlProvider.UI.ShowDocSel(docSel);
    }

    #endregion
  }


  /// <summary>
  /// Страница отчета, содержащая таблицу <see cref="EFPDBxGridView"/>
  /// </summary>
  public class EFPReportDBxGridPage : EFPReportStdConfigurableGridPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    /// <param name="ui">Интерфейс для доступа к документам. Не может быть null</param>
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
    private readonly DBUI _UI;

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
    /// Создает <see cref="EFPDBxGridView"/>
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Провайдер табличного просмотра</returns>
    protected override EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      EFPDBxGridView res = new EFPDBxGridView(BaseProvider, control, _UI);
      return res;
    }

    #endregion
  }
}
