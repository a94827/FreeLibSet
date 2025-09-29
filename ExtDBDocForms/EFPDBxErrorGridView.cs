// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Data;

namespace FreeLibSet.Forms.Docs
{
  #region EFPDBxErrorGridViewDocSelEventArgs

  /// <summary>
  /// Аргументы события <see cref="EFPDBxErrorGridView.GetDocSel"/>
  /// </summary>
  public class EFPDBxErrorGridViewDocSelEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается в <see cref="EFPDBxErrorGridView"/>
    /// </summary>
    /// <param name="items"></param>
    /// <param name="ui"></param>
    /// <param name="reason"></param>
    public EFPDBxErrorGridViewDocSelEventArgs(ErrorMessageItem[] items, DBUI ui, EFPDBxViewDocSelReason reason)
    {
      _Items = items;
      _UI = ui;
      _DocSel = new DBxDocSelection(ui.DocProvider.DBIdentity);
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Сообщения об ошибках, для которых требуется создать выборку документов
    /// </summary>
    public ErrorMessageItem[] Items { get { return _Items; } }
    private readonly ErrorMessageItem[] _Items;

    /// <summary>
    /// Доступ к интерфейсу документов
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

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
    /// Добавить ссылку на документ в выборку.
    /// Также добавляются все связанные документы.
    /// </summary>
    /// <param name="docTypeName">Вид документа</param>
    /// <param name="docId">Идентификатор документа</param>
    public void Add(string docTypeName, Int32 docId)
    {
      UI.DocTypes[docTypeName].PerformGetDocSel(DocSel, docId, Reason);
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPDBxErrorGridView.GetDocSel"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="EFPDBxErrorGridView"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDBxErrorGridViewDocSelEventHandler(object sender,
    EFPDBxErrorGridViewDocSelEventArgs args);

  #endregion

  /// <summary>
  /// Расширение табличного просмотра со списком ошибок <see cref="EFPErrorDataGridView"/> для работы со ссылками на документы.
  /// Добавляется событие <see cref="EFPDBxErrorGridView.GetDocSel"/>.
  /// Поддерживает просмотр и редактирование документов, на которые есть ссылки в сообщениях в списке.
  /// </summary>
  public class EFPDBxErrorGridView : EFPErrorDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер табличного просмотра
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Табличный просмотр</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDBxErrorGridView(EFPBaseProvider baseProvider, DataGridView control, DBUI ui)
      : base(baseProvider, control)
    {
      Init(ui);
    }

    /// <summary>
    /// Создает провайдер табличного просмотра
    /// </summary>
    /// <param name="controlWithPreview">Табличный просмотр и панель инструментов</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDBxErrorGridView(IEFPControlWithToolBar<DataGridView> controlWithPreview, DBUI ui)
      : base(controlWithPreview)
    {
      Init(ui);
    }

    private void Init(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      base.CanView = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Система работы с документами на стороне клиента.
    /// Задается в конструкторе.
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вызывает обработчик события <see cref="EFPErrorDataGridView.ErrorMessagesChanged"/>, если он присоединен.
    /// </summary>
    protected override void OnErrorMessagesChanged()
    {
      base.OnErrorMessagesChanged();
      InitEditCommandItems();
    }

    /// <summary>
    /// Инициализация свойства <see cref="EFPDataGridView.ReadOnly"/> и <see cref="EFPDataGridView.CanView"/> после изменения списка сообщений и присоединения обработчика.
    /// Учитывается наличие обработчика события <see cref="GetDocSel"/>.
    /// </summary>
    protected override void InitEditCommandItems()
    {
      base.InitEditCommandItems();

      CanView = HasGetDocSelHandler && ErrorMessages != null;
      if (HasGetDocSelHandler)
      {
        base.CanMultiEdit = true;
        ReadOnly = (ErrorMessages == null);
        // if (Control == null)
        if (!CommandItems.IsReadOnly) // 05.12.2016
          CommandItems.EnterAsOk = false;
      }
      // иначе базовый класс правильно установил свойство
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Редактирование или просмотр документов.
    /// Вызывает обработчик события <see cref="GetDocSel"/> для получения выборки документов, связанных с выбранными пользователем сообщениями.
    /// Затем, если получена непустая выборка, документы открываются на просмотр или редактирование.
    /// Если разрешено групповое редактирование (<see cref="DocTypeUI.CanMultiEdit"/>=true, вызывается <see cref="DocTypeUI.PerformEditing(int, bool)"/> .
    /// Если выбрано несколько документов, а групповое редактирование запрещено, показывается окно выборки документов.
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Возвращает true, если событие было обработано</returns>
    protected override bool OnEditData(EventArgs args)
    {
      if (State == UIDataState.Edit && base.HasEditMessageHandler)
        return base.OnEditData(args);

      switch (State)
      {
        case UIDataState.Edit:
        case UIDataState.View:
          DBxDocSelection docSel = CreateDocSel(EFPDBxViewDocSelReason.Copy);
          if (docSel != null)
          {
            if (!docSel.IsEmpty)
            {
              // Всегда берем первую таблицу для редактирования
              string docTypeName = docSel.TableNames[0];
              IIdSet<Int32> docIds = docSel[docTypeName];
              if (docIds.Count > 1 && (!UI.DocTypes[docTypeName].CanMultiEdit))
                UI.ShowDocSel(docSel); // групповое редактирование запрещено
              else
                UI.DocTypes[docTypeName].PerformEditing(docIds, State, false);
              return true;
            }
          }

          EFPApp.ShowTempMessage(Res.EFPDBxErrorGridView_Err_NoDocs);
          return true;
      }

      return base.OnEditData(args);
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Возвращает команды локального меню
    /// </summary>
    public new EFPDBxErrorGridViewCommandItems CommandItems { get { return (EFPDBxErrorGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает EFPDBxErrorGridViewCommandItems
    /// </summary>
    /// <returns>Созданный список команд</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPDBxErrorGridViewCommandItems(this);
    }

    #endregion

    #region Событие GetDocSel

    /// <summary>
    /// Если обработчик установлен, то при копировании ячеек в буфер обмена будет
    /// помещена выборка документов (объект <see cref="DBxDocSelection"/>).
    /// Также будет добавлена команда "Отправить" -> "Выборка".
    /// </summary>
    public event EFPDBxErrorGridViewDocSelEventHandler GetDocSel
    {
      add
      {
        _GetDocSel += value;
        InitEditCommandItems();
      }
      remove
      {
        _GetDocSel -= value;
        InitEditCommandItems();
      }
    }
    private EFPDBxErrorGridViewDocSelEventHandler _GetDocSel;

    /// <summary>
    /// В случае переопределения метода также должно быть переопределено свойство <see cref="HasGetDocSelHandler"/>.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnGetDocSel(EFPDBxErrorGridViewDocSelEventArgs args)
    {
      if (_GetDocSel != null)
        _GetDocSel(this, args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик <see cref="GetDocSel"/>.
    /// </summary>
    public virtual bool HasGetDocSelHandler { get { return _GetDocSel != null; } }

    /// <summary>
    /// Создание выборки документов для выбранных строк в табличном просмотре.
    /// </summary>
    /// <param name="reason">Причина создания выборки</param>
    /// <returns>Выборка документов или null</returns>
    public DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason)
    {
      return CreateDocSel(reason, null);
    }

    /// <summary>
    /// Создание выборки документов для указанных строк в табличном просмотре.
    /// </summary>
    /// <param name="reason">Причина создания выборки</param>
    /// <param name="rowIndices">Индексы строк</param>
    /// <returns>Выборка документов или null</returns>
    public DBxDocSelection CreateDocSel(EFPDBxViewDocSelReason reason, int[] rowIndices)
    {
      if (!HasGetDocSelHandler)
        return null;

      if (ErrorMessages == null)
        return null;
      if (ErrorMessages.Count == 0)
        return null;

      if (rowIndices == null)
        rowIndices = base.SelectedRowIndices;

      ErrorMessageItem[] items = new ErrorMessageItem[rowIndices.Length];
      for (int i = 0; i < rowIndices.Length; i++)
        items[i] = ErrorMessages[rowIndices[i]];

      DBxDocSelection docSel = null;
      try
      {
        EFPApp.BeginWait(Res.Common_Phase_DocSelCreation, "DBxDocSelection");
        try
        {
          EFPDBxErrorGridViewDocSelEventArgs args = new EFPDBxErrorGridViewDocSelEventArgs(items, UI, reason);
          OnGetDocSel(args);
          if (!args.DocSel.IsEmpty)
            docSel = args.DocSel;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
      return docSel;
    }

    #endregion
  }


  /// <summary>
  /// Команды локального меню для <see cref="EFPDBxErrorGridView"/>.
  /// Добавляет команду "Отправить" - "Выборка документов".
  /// Поддерживает копирование выборки документов в буфер обмена.
  /// </summary>
  public class EFPDBxErrorGridViewCommandItems : EFPDataGridViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Используется <see cref="EFPDBxErrorGridView"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDBxErrorGridViewCommandItems(EFPDBxErrorGridView controlProvider)
      : base(controlProvider)
    {
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPDBxErrorGridView ControlProvider { get { return (EFPDBxErrorGridView)(base.ControlProvider); } }

    /// <summary>
    /// Добавляет команду "Отправить" - "Выборка документов"
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      if (ControlProvider.HasGetDocSelHandler) // Есть обработчик
      {
        EFPCommandItem ci = new EFPCommandItem("SendTo", "DocSel");
        ci.MenuText = Res.Cmd_Menu_SendTo_DocSel;
        ci.ImageKey = "DBxDocSelection";
        ci.Parent = base.MenuSendTo;
        ci.Click += ciSendToDocSel_Click;
        ci.Usage = EFPCommandItemUsage.Menu; // без кнопки
        Add(ci);
      }
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Добавляет выборку документов (объект <see cref="DBxDocSelection"/>) в набор форматов для буфера обмена
    /// </summary>
    /// <param name="args"></param>
    protected override void OnAddCopyFormats(DataObjectEventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxViewDocSelReason.Copy);
      if (docSel != null)
        args.DataObject.SetData(docSel);

      base.OnAddCopyFormats(args);
    }

    #endregion

    #region Отправить

    private void ciSendToDocSel_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.CreateDocSel(EFPDBxViewDocSelReason.SendTo);
      if (docSel == null || docSel.IsEmpty)
      {
        EFPApp.ShowTempMessage(Res.DocSel_Msg_IsEmpty);
        return;
      }
      ControlProvider.UI.ShowDocSel(docSel);
    }

    #endregion
  }

  /// <summary>
  /// Страница отчета со списком ошибок <see cref="EFPDBxErrorGridView"/>
  /// </summary>
  public class EFPReportDBxErrorMessageListPage : EFPReportErrorMessageListPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    /// <param name="ui">Интерфейс для доступа к документам</param>
    public EFPReportDBxErrorMessageListPage(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;
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
    /// Создает <see cref="EFPDBxErrorGridView"/>
    /// </summary>
    /// <param name="control">Табличный просмотр</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPErrorDataGridView CreateControlProvider(DataGridView control)
    {
      EFPDBxErrorGridView controlProvider = new EFPDBxErrorGridView(BaseProvider, control, UI);
      if (GetDocSel != null)
        controlProvider.GetDocSel += new EFPDBxErrorGridViewDocSelEventHandler(ControlProvider_GetDocSel);
      return controlProvider;
    }

    #endregion

    #region Событие GetDocSel

    /// <summary>
    /// Событие для получения выборки документов
    /// </summary>
    public event EFPDBxErrorGridViewDocSelEventHandler GetDocSel;

    void ControlProvider_GetDocSel(object sender, EFPDBxErrorGridViewDocSelEventArgs args)
    {
      if (GetDocSel != null)
        GetDocSel(this, args);
    }

    #endregion
  }
}
