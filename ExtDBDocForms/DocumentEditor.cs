// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using FreeLibSet.Forms;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Logging;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.UICore;
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Редактор документов всех видов
  /// </summary>
  public class DocumentEditor : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Основной вариант конструктора.
    /// Используется <see cref="FreeLibSet.Forms.Docs.DocTypeUI.PerformEditing(int, bool)"/>.
    /// Не создается напрямую в прикладном коде.
    /// </summary>
    /// <param name="ui">Доступ к пользовательскому интерфейсу</param>
    /// <param name="docTypeName">Имя типа документа</param>
    /// <param name="state">Режим редактирования</param>
    /// <param name="docIds">Список идентификаторов документов для редактирования, просмотра, удаления или копирования</param>
    public DocumentEditor(DBUI ui, string docTypeName, UIDataState state, IIdSet<Int32> docIds)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentNullException("ui");
      if (String.IsNullOrEmpty(docTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");
      // лишняя проверка
      //if (!ui.DocProvider.DocTypes.Contains(docTypeName))
      //  throw new ArgumentException("Неизвестный тип документа \"" + docTypeName + "\"", "docTypeName");
#endif
      _UI = ui;
      _State = state;

      DBxDocType docType = _UI.DocTypes[docTypeName].DocType;
      DBxAccessMode accessMode = ui.DocProvider.DBPermissions.TableModes[docTypeName];

      switch (accessMode)
      {
        case DBxAccessMode.None:
          EFPApp.ErrorMessageBox(String.Format(Res.Common_Err_DocTypeAccessDenied, docType.PluralTitle));
          return;
        case DBxAccessMode.ReadOnly:
          switch (_State)
          {
            case UIDataState.View:
              break;
            case UIDataState.Edit:
              _State = UIDataState.View;
              break;
            default:
              EFPApp.ErrorMessageBox(String.Format(Res.Common_Err_DocTypeReadOnly, docType.PluralTitle),
                Res.Common_ErrTitle_AccessDenied);
              return;
          }
          break;
      }


      _Documents = new DBxDocSet(ui.DocProvider);
      _Documents.CheckDocs = true;
      DBxMultiDocs mDocs = _Documents[docTypeName];

      if (_State == UIDataState.Edit && mDocs.Permissions == DBxAccessMode.ReadOnly)
        _State = UIDataState.View; // чтобы не выбрасывалось исключение

      try
      {
        switch (_State)
        {
          case UIDataState.Edit:
            try
            {
              mDocs.Edit(docIds);
            }
            catch (DBxAccessException)
            {
              _State = UIDataState.View;
              // 09.06.2015
              // В большинстве случаев, при этой ошибке документы находятся в режиме View,
              // но теоретически может быть, что часть документов перешло в режим Edit, а часть - нет.
              // Или, документы вообще не загрузились
              if (mDocs.DocCount != docIds.Count || mDocs.DocState != DBxDocState.View)
              {
                mDocs.ClearList();
                mDocs.View(docIds);
              }
            }
            //DebugTools.DebugDataSet(FDocuments.DebugDataSet, "загружено");
            //int x=Docs.DocCount;
            break;
          case UIDataState.Insert:
            mDocs.Insert();
            break;
          case UIDataState.InsertCopy:
            mDocs.InsertCopy(docIds);
            break;
          case UIDataState.View:
            mDocs.View(docIds);
            break;
          case UIDataState.Delete:
            mDocs.View(docIds);
            break;
          default:
            throw new InvalidEnumArgumentException("state", (int)_State, typeof(UIDataState));
        }
      }
      catch (DBxAccessException e)
      {
        EFPApp.ErrorMessageBox(e.Message, Res.Common_ErrTitle_AccessDenied);
        _Documents = null;
        return;
      }

      _DataChanged = false;
      Modal = false;
      // FSubDocs = new DocEditSubDocs(this);
      _DocumentsAreExternal = false;

      _Properties = new NamedValues();
      _DocumentTextValue = String.Empty;
    }

    /// <summary>
    /// Вариант конструктора с предварительно загруженными данными.
    /// При использовании этого конструктора выставляется свойство <see cref="DocumentsAreExternal"/>
    /// </summary>
    /// <param name="ui">Доступ к пользовательскому интерфейсу</param>
    /// <param name="documents">Загруженные документы</param>
    public DocumentEditor(DBUI ui, DBxDocSet documents)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentException("ui");
      if (documents == null)
        throw new ArgumentNullException("documents");
      if (documents.Count == 0)
        throw ExceptionFactory.ArgIsEmpty("documents");

      if (documents.DocProvider != ui.DocProvider)
        throw ExceptionFactory.ArgProperty("documents", documents, "DocProvider", documents.DocProvider, new object[] { ui.DocProvider });
#endif

      _UI = ui;
      _Documents = documents;
      _Documents.CheckDocs = true;

      switch (_Documents[0].DocState)
      {
        case DBxDocState.Edit:
          _State = UIDataState.Edit;
          break;
        case DBxDocState.Insert:
          _State = UIDataState.Insert;
          break;
        case DBxDocState.View:
          _State = UIDataState.View;
          break;
        default:
          throw ExceptionFactory.ObjectProperty(_Documents[0], "State", _Documents[0].DocState,
            new object[] { DBxDocState.Edit, DBxDocState.Insert, DBxDocState.View });
      }

      _DataChanged = false;
      Modal = false;
      // FSubDocs = new DocEditSubDocs(this);
      _DocumentsAreExternal = true;

      _Properties = new NamedValues();
    }

    /// <summary>
    /// Инициализация свойства <see cref="DBxDocSet.ActionInfo"/>
    /// </summary>
    private void InitActionInfo()
    {
      DBxMultiDocs mainDocs = _Documents[0];
      string text1;
      switch (_State)
      {
        case UIDataState.Edit:
          bool hasEdit = false;
          bool hasRestore = false;
          for (int i = 0; i < mainDocs.DocCount; i++)
          {
            if (mainDocs[i].Deleted)
              hasRestore = true;
            else
              hasEdit = true;
          }

          if (hasRestore)
          {
            if (hasEdit)
              text1 = Res.Editor_Msg_TitleEditRestore;
            else
              text1 = Res.Editor_Msg_TitleRestore;
          }
          else
            text1 = Res.Editor_Msg_TitleEdit;

          // 05.11.2015
          // UI.Text.GetTextValue(MainDocs[0]) не будет работать правильно, если текстовое представление документа
          // зависит от вычисляемых полей. Вычисление выполняется на стороне сервера.
          // Можно в ActionInfo сделать что-нибудь вроде %DOCTEXT% для подстановки на стороне сервера

          break;
        case UIDataState.Insert:
          text1 = Res.Editor_Msg_TitleInsert;
          break;
        case UIDataState.InsertCopy:
          text1 = Res.Editor_Msg_TitleInsertCopy;
          break;
        case UIDataState.Delete:
          text1 = Res.Editor_Msg_TitleDelete;
          break;
        case UIDataState.View:
          return;
        default:
          throw new BugException("State=" + State.ToString());
      }

      if (mainDocs.DocCount == 1)
        _Documents.ActionInfo = String.Format(Res.Editor_Msg_ActionSingle, text1, mainDocs.DocType.SingularTitle);
      else
        _Documents.ActionInfo = String.Format(Res.Editor_Msg_ActionMulti, text1, mainDocs.DocType.PluralTitle, mainDocs.DocCount);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Объекты редактируемых документов
    /// </summary>
    public DBxDocSet Documents { get { return _Documents; } }
    private readonly DBxDocSet _Documents;

    /// <summary>
    /// Обработчики пользовательского интерфейса
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Имя типа главного редактируемого документа
    /// </summary>
    public string DocTypeName { get { return Documents[0].DocType.Name; } }

    /// <summary>
    /// Обработчики пользовательского интерфейса для основного типа документов
    /// </summary>
    public DocTypeUI DocTypeUI { get { return UI.DocTypes[DocTypeName]; } }

    /// <summary>
    /// Возвращает true, если вызывался вариант конструктора, использующий
    /// предварительно загруженные документы, то есть свойство <see cref="Documents"/> было
    /// инициализировано снаружи редактора. 
    /// Возвращает false, если использовался основной вариант конструктора с 
    /// указанием типа документа, режима и идентификаторов.
    /// </summary>
    public bool DocumentsAreExternal { get { return _DocumentsAreExternal; } }
    private readonly bool _DocumentsAreExternal;

    /// <summary>
    /// Режим: Редактирование, добавление, удаление или просмотр документа
    /// </summary>
    public UIDataState State { get { return _State; } }
    private UIDataState _State;


    ///// <summary>
    ///// Массив идентификаторов редактируемых документов (поля "RefId")
    ///// </summary>
    //    public int[] Values{ get{return FIds;} }

    /// <summary>
    /// True, если редактируется, просматривается или удаляется сразу
    /// несколько документов. 
    /// Прикладной код может анализировать это свойство и отключать редактирование некоторых полей или не добавлять вкладки поддокументов,
    /// если это не совместимо с групповым редактированием.
    /// </summary>
    public bool MultiDocMode
    { get { return Documents[0].DocCount != 1; } }

    /// <summary>
    /// Доступ к значениям документов.
    /// Если в редакторе открыты документы нескольких видов, значения относятся к "основным" документам
    /// (<see cref="Documents"/>[0].Values).
    /// Если одновременно открыто несколько документов, то могут быть "серые" значения.
    /// </summary>
    public IDBxExtValues MainValues
    { get { return _Documents[0].Values; } }

    ///// <summary>
    ///// Доступ ко вложенным поддокументам (наборам строк)
    ///// </summary>
    //public DocEditSubDocs SubDocs {get{return FSubDocs;}}
    //private DocEditSubDocs FSubDocs;

    /// <summary>
    /// Если установить в true, то редактор будет запущен в модальном 
    /// режиме, иначе он будет встроен в интерфейс MDI
    /// </summary>
    public bool Modal
    {
      get { return _Modal; }
      set
      {
        // TODO:
        //if (IsExecuting)
        //  throw new InvalidOperationException("Свойство \"Modal\" должно устанавливаться до запуска редактора");
        _Modal = value;
      }
    }
    private bool _Modal;

    /// <summary>
    /// Внешний инициализатор полей для нового документа (для основного типа,
    /// заданного в конструкторе, который используется в <see cref="Documents"/>[0]).
    /// Вызывается после обработки значений в <see cref="DocTypeUIBase.Columns"/>.
    /// </summary>
    public DocumentViewHandler Caller
    {
      get { return _Caller; }
      set { _Caller = value; }
    }
    private DocumentViewHandler _Caller;

    /// <summary>
    /// Исходное состояние признака несохраненных изменений.
    /// По умолчанию - false (нет несохраненных изменений).
    /// Если установить в true, то в исходном состоянии редактора будет рисоваться
    /// звездочка в заголовке.
    /// Предупреждение. Редактор может запускаться в режиме несохраненных изменений,
    /// даже если это свойство не установлено, в случае, когда одна из процедур
    /// инициализации изменила загруженные данные.
    /// </summary>
    public bool StartWithChanges
    {
      get { return _StartWithChanges; }
      set
      {
        // TODO:
        //if (IsExecuting)
        //  throw new InvalidOperationException("Свойство \"StartWithChanges\" должно устанавливаться до запуска редактора");
        if (value && State == UIDataState.View)
          throw new InvalidOperationException(Res.Editor_Err_StartWithChangesInView);
        _StartWithChanges = value;
      }
    }
    private bool _StartWithChanges;

    /// <summary>
    /// Форма Windows многостраничного редактора документа
    /// </summary>
    internal DataEditDialog Dialog { get { return _Dialog; } }
    private DataEditDialog _Dialog;

    /// <summary>
    /// Объекты синхронизации для редактора
    /// </summary>
    public DepSyncCollection Syncs
    {
      get
      {
        if (_Dialog == null)
          return null;
        else
          return _Dialog.Syncs;
      }
    }

    internal UIExtEditItemList DocEditItems { get { return _Dialog.EditItems; } }

    /// <summary>
    /// Устанавливается после закрытия редактора в true, если данные
    /// были изменены (форма закрыта нажатием "ОК" или было нажатие "Apply").
    /// В режиме <see cref="State"/>=<see cref="UIDataState.Delete"/> показывает, что данные были удалены.
    /// </summary>
    public bool DataChanged { get { return _DataChanged; } }
    private bool _DataChanged;

    /// <summary>
    /// Отслеживание изменений для рисования звездочки в заголовке формы.
    /// В него входят объекты <see cref="DepChangeInfoItem"/> для управляющих элементов, привязанных к полям документа,
    /// а также <see cref="SubDocsChangeInfo"/> и <see cref="ExternalChangeInfo"/>.
    /// Объект существует только в процессе работы редактора.
    /// </summary>
    public DepChangeInfoList ChangeInfo { get { return _Dialog.ChangeInfoList; } }

    /// <summary>
    /// Объект для отслеживания изменений в поддокументах. Установка свойства
    /// <see cref="DepChangeInfoItem.Changed"/> в True означает наличие изменений.
    /// Имеется только один объект для всех поддокументов, в том числе разнотипных.
    /// Объект существует только в процессе работы редактора.
    /// </summary>
    public DepChangeInfoItem SubDocsChangeInfo { get { return _SubDocsChangeInfo; } }
    private DepChangeInfoItem _SubDocsChangeInfo;


    /// <summary>
    /// Внешний флаг наличия изменений.
    /// Объект существует только в процессе работы редактора.
    /// Исходное состояние признака изменений определяется свойством <see cref="StartWithChanges"/>
    /// </summary>
    public DepChangeInfoItem ExternalChangeInfo { get { return _ExternalChangeInfo; } }
    private DepChangeInfoItem _ExternalChangeInfo;

    ///// <summary>
    ///// Секция конфигурации, в которой следует хранить настройки редактора, например,
    ///// последнее установленное пользователем значение, которое можно использовать
    ///// при инициализации нового документа
    ///// </summary>
    //public ConfigSection ConfigSection { get { return DocType.EditorConfigSection; } }

    /// <summary>
    /// Оригинальные значения (по одному набору для каждого типа документов)
    /// для сравнения при сохранении значений
    /// </summary>
    private DBxArrayExtValues[] _OrgVals;

    ///// <summary>
    ///// Это свойство может быть задано перед запуском редактора в режиме создания
    ///// документов. В этом случае, перед записью документов будет вызван метод
    ///// GetImportId(), что приведет к созданию операции импорта, а все документы
    ///// получат идентификатор импорта.
    ///// Позволяет реализовать отложенный импорт, когда перед импортом показывается
    ///// окно редактора. Если пользователь решит не сохранять документ, то операция
    ///// импорта создана не будет
    ///// </summary>
    //public ProcessImportEventArgs ImportArgs;

    /// <summary>
    /// Свойство возвращает true, если была нажат кнопка "ОК" или "Запись"
    /// и в данный момент выполняется запись значений.
    /// Обработчик события <see cref="BeforeWrite"/>, <see cref="AfterWrite"/>"" или другой в <see cref="FreeLibSet.Forms.Docs.DocTypeUI"/>
    /// может проверить свойство, чтобы определить, выполняется ли нажатие кнопки
    /// записи или был программный вызов метода <see cref="WriteData()"/>.
    /// </summary>
    public bool IsInsideWriting
    {
      get
      {
        if (Dialog == null)
          return false;
        else
          return Dialog.FormState == ExtEditDialogState.ApplyClicked || Dialog.FormState == ExtEditDialogState.OKClicked;
      }
    }

    /// <summary>
    /// Пользовательские данные
    /// </summary>
    public NamedValues Properties { get { return _Properties; } }
    private readonly NamedValues _Properties;

    /// <summary>
    /// Идентификатор длительной блокировки, если она была установлена
    /// </summary>
    private Guid _LockGuid;

    #endregion

    #region Общие события

    /// <summary>
    /// Вызывается сразу после того, как форма редактора выведена на экран.
    /// Объекты синхронизации связаны в группы.
    /// </summary>
    public event DocEditEventHandler EditorShown;

    /// <summary>
    /// Вызывается после того, как были установлены значения всех полей перед началом
    /// редактирования. На момент вызова форма еще не выведена на экран и объекты
    /// синхронизации не связаны в группы.
    /// </summary>
    public event DocEditEventHandler AfterReadValues;

    /// <summary>
    /// Вызывается при нажатии кнопок "ОК" или "Запись" перед посылкой данных
    /// серверу. 
    /// Вызывается в режимах Edit, Insert и InsertCopy
    /// Установка Args.Cancel=true предотвращает запись данных и закрытие редактора.
    /// Программа должна вывести сообщение пользователю о причинах отмены.
    /// На момент вызова данные формы еще не перенесены в документы.
    /// Если требуется обработка значений в <see cref="FreeLibSet.Forms.Docs.DocumentEditor.Documents"/>, используйте
    /// событие <see cref="FreeLibSet.Forms.Docs.DocTypeUI.Writing"/>.
    /// </summary>
    public event DocEditCancelEventHandler BeforeWrite;

    /// <summary>
    /// Вызывается после успешного нажатия кнопок "ОК" или "Запись" и обновления
    /// полей документа сервером
    /// </summary>
    public event DocEditEventHandler AfterWrite;

    /// <summary>
    /// Вызывается при завершении работы редактора (даже если форма не выводилась)
    /// </summary>
    public event EventHandler Executed;

    #endregion

    #region Run()

    /// <summary>
    /// Открытие редактора.
    /// Если форма открывается в модальном режиме, то ожидается завершение работы с формой.
    /// </summary>
    public void Run()
    {
      try
      {
        DoRun();
      }
      catch (Exception e)
      {
        ClearEditLock();

        try
        {
          if (Documents != null)
            e.Data["DocumentEditor.Documents"] = Documents.ToString(); // Класс не сериализуемый
          e.Data["DocumentEditor.State"] = State;
          e.Data["DocumentEditor.Properties"] = Properties;
          e.Data["DocumentEditor.DocumentsAreExternal"] = DocumentsAreExternal;

        }
        catch { } // вложенные ошибки проглатываем

        EFPApp.ShowException(e, Res.Editor_ErrTitle_Run);
      }
    }

    private void ClearEditLock()
    {
      if (_LockGuid != Guid.Empty)
      {
        try
        {
          Documents.RemoveLongLock(_LockGuid);
        }
        catch { }
        _LockGuid = Guid.Empty;
      }
    }

    private void DoRun()
    {
      _DataChanged = false;
      if (_Documents == null)
        return;

      // TODO: Documents.AllowOrgValues = !IsReadOnly;

      // 27.09.2019
      // В режиме просмотра и редактирования обновляем строки просмотров документов
      switch (State)
      {
        case UIDataState.View:
        case UIDataState.Edit:
          DocTypeUI.Browsers.UpdateDBCacheAndRows(_Documents);
          break;
      }

      // Инициализация начальных значений для режима создания и копирования документа
      //TODO: AccDepClientExec.DocTypes.LoadSavedFieldValues();
      List<string> initNewDocTypes = new List<string>();
      InitNewDocValues(initNewDocTypes);

      // Запуск внешних средств редактирования
      bool cancel, showEditor;
      UI.DocTypes[DocTypeName].DoBeforeEdit(this, out cancel, out showEditor);
      if (cancel)
        return;

      // 25.03.2010. Инициализация начальных значений полей документов, добавленных в BeforeInsert
      InitNewDocValues(initNewDocTypes);

      // TODO:
      /*
      if (State == EFPDataGridViewState.Edit || State == EFPDataGridViewState.Delete)
      {
        for (int i = 0; i < Documents.Count; i++)
        {
          if (Documents[i].Count == 0)
            continue;
          string why;
          ClientDocType ThisDocType = (ClientDocType)(Documents[i].DocType);
          if (!ThisDocType.TestEditable(Documents[i], State, out why))
          {
            if (State == EFPDataGridViewState.Edit)
            {
              FState = EFPDataGridViewState.View;
              for (int j = 0; j < Documents.Count; j++)
              {
                if (Documents[j].Mode == DocMode.Edit)
                  Documents[j].View();
              }
              break;
            }
            else
            {
              EFPApp.MessageBox("Нельзя удалить документ \"" + ThisDocType.SingularTitle + "\": " + why,
                "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return;
            }
          }
        }
      }
      */

      if (State == UIDataState.Delete)
      {
        #region Проверка прав на удаление

        string errorText;
        if (!_Documents.DocProvider.DocPermissions.TestDocuments(_Documents, DBxDocPermissionReason.BeforeDelete, out errorText))
        {
          EFPApp.ErrorMessageBox(errorText, Res.Common_ErrTitle_CannotDeleteDoc);
          return;
        }

        #endregion

        #region Запрос подтверждения

        String msg;
        if (Documents.Count == 1 && Documents[0].DocCount == 1)
        {
          string docText;
          try
          {
            docText = UI.TextHandlers.GetTextValue(Documents[0][0]);
          }
          catch (Exception e) // на всякий случай
          {
            docText = e.Message;
          }
          msg = String.Format(Res.Editor_Msg_DeleteSingleDoc, Documents[0].DocType.SingularTitle, docText);
        }
        else
        {
          string docText = Documents.DocTypeAndCountToString();
          msg = String.Format(Res.Editor_Msg_DeleteMultiDocs, docText);
        }
        if (EFPApp.MessageBox(msg, Res.Common_Title_ConfirmDelete,
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
          return;

        #endregion

        showEditor = false;
      }

      if (showEditor)
      {
        DBxDocSelection editDocSel = _Documents.GetDocSelection(DBxDocState.Edit);
        if (!editDocSel.IsEmpty)
        {
          try
          {
            _LockGuid = Documents.AddLongLock(editDocSel);
          }
          catch (DBxDocsLockException e)
          {
            List<string> msgs = new List<string>();
            msgs.Add(Res.Editor_Err_LockForEdit);
            if (e.OldLock.UserId != 0)
              msgs.Add(String.Format(Res.Editor_Err_LockedByUser, UI.GetUserName(e.OldLock.UserId)));
            EFPApp.MessageBox(String.Join(Environment.NewLine, msgs.ToArray()),
              Res.Editor_ErrTitle_LockForEdit, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Documents.ChangeDocState(DBxDocState.Edit, DBxDocState.View);
            _State = UIDataState.View;
          }
        }

        EFPApp.BeginWait(Res.Editor_Phase_EditorOpen);
        try
        {
          _Dialog = new DataEditDialog();
          _Dialog.Tag = this;
          _Dialog.DataState = this.State;
          _Dialog.ConfigSectionName = "Ed_" + Documents[0].DocType.Name; // 09.06.2021

          #region Список изменений

          _SubDocsChangeInfo = new DepChangeInfoItem();
          _SubDocsChangeInfo.DisplayName = Res.Editor_Name_SubDocsChangeInfo;
          ChangeInfo.Add(_SubDocsChangeInfo);

          _ExternalChangeInfo = new DepChangeInfoItem();
          _ExternalChangeInfo.DisplayName = Res.Editor_Name_ExternalChangeInfo;
          ChangeInfo.Add(_ExternalChangeInfo);
          _ExternalChangeInfo.Changed = StartWithChanges;

          #endregion

          // Инициализация закладок
          for (int i = 0; i < Documents.Count; i++)
          {
            if (Documents[i].DocCount == 0)
              continue; // нет документов такого типа
            DocTypeUI thisType = UI.DocTypes[Documents[i].DocType.Name];
            thisType.PerformInitEditForm(this, Documents[i]);
          }

          // Только после инициализации всех закладок можно запомнить исходные значения
          _OrgVals = new DBxArrayExtValues[Documents.Count];
          if (!IsReadOnly)
          {
            for (int i = 0; i < Documents.Count; i++)
            {
              if (Documents[i].DocCount == 1)
                _OrgVals[i] = new DBxArrayExtValues(Documents[i][0].Values);
            }
          }

          // Кнопка "Еще"
          InitMoreCommands();

          // Подсказки для кнопок
          switch (State)
          {
            case UIDataState.Edit:
              _Dialog.OKButtonToolTipText = Res.DocumentEditor_ToolTip_OkEdit;
              _Dialog.CancelButtonToolTipText = Res.DocumentEditor_ToolTip_CancelEditInsert;
              _Dialog.ApplyButtonToolTipText = Res.DocumentEditor_ToolTip_ApplyEdit;
              break;
            case UIDataState.Insert:
            case UIDataState.InsertCopy:
              _Dialog.OKButtonToolTipText = Res.DocumentEditor_ToolTip_OkInsert;
              _Dialog.CancelButtonToolTipText = Res.DocumentEditor_ToolTip_CancelEditInsert;
              _Dialog.ApplyButtonToolTipText = Res.DocumentEditor_ToolTip_ApplyInsert;
              break;
            case UIDataState.Delete:
              _Dialog.OKButtonToolTipText = Res.DocumentEditor_ToolTip_OkDelete;
              _Dialog.CancelButtonToolTipText = Res.DocumentEditor_ToolTip_CancelDelete;
              break;
            case UIDataState.View:
              _Dialog.OKButtonToolTipText = Res.DocumentEditor_ToolTip_OkView;
              break;
          }
          _Dialog.MoreButtonToolTipText = Res.DocumentEditor_ToolTip_More;

          // Инициализируем значения
          DocEditItems.ReadValues();

          if (AfterReadValues != null)
          {
            DocEditEventArgs Args = new DocEditEventArgs(this);
            AfterReadValues(this, Args);
          }

          ChangeInfo.ResetChanges(); // 12.08.2015

          InitFormTitle(); // после вызова ReadValues, т.к. заголовок может быть другим

          _Dialog.FormChecks.Add(Dialog_FormCheck);
          _Dialog.Writing += Dialog_Writing;
          _Dialog.FormShown += Dialog_FormShown;
          _Dialog.FormClosed += new EventHandler(Dialog_FormClosed);

          // Посылка извещение при переключении страницы
          //FForm.MainTabControl.SelectedIndexChanged += new EventHandler(TabIndexChanged);
          //TabIndexChanged(null, null); // сразу посылаем извещение для первой страницы

          // Добавляем закладки для предварительного просмотра
          //Previews = new DocumentEditorPrintPreviews(this);

          //ClientExec.DebugServer = false;
          //System.Diagnostics.Trace.WriteLine("******* Щас будет форма");
        }
        finally
        {
          EFPApp.EndWait();
        }

        if (Modal)
          _Dialog.ShowDialog();
        else
          _Dialog.Show(); // 10.03.2016
      }
      else
      {
        // Редактирование без формы
        if (State != UIDataState.View)
          DoWrite(false);
        if (Executed != null)
          Executed(this, null);
      }
    }

    private void InitNewDocValues(List<string> initNewDocTypes)
    {
      for (int i = 0; i < Documents.Count; i++)
      {
        if (Documents[i].DocCount == 0)
          continue; // нет документов такого типа
        DocTypeUI thisType = UI.DocTypes[Documents[i].DocType.Name];

        // Используем сохраненные значения по умолчанию
        if (Documents[i].DocState == DBxDocState.Insert && (!DocumentsAreExternal))
        {
          if (initNewDocTypes.Contains(thisType.DocType.Name))
            continue; // Уже инициализировался
          try
          {
            thisType.Columns.PerformInsert(Documents[i][0].Values, State == UIDataState.InsertCopy);

            // В режиме создания нового документа, если редактор вызывается из RBExec, 
            // пытаемся применить установленные фильтры к новому документу до редактирования.
            // Возможно, некоторые поля окажутся заполненными
            if (i == 0 && State == UIDataState.Insert && Caller != null)
              Caller.InitNewDocValues(Documents[i][0]);
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, String.Format(Res.DocumentEditor_ErrTitle_InitNewValues, thisType.DocType.SingularTitle));
          }
          initNewDocTypes.Add(thisType.DocType.Name);
        }
      }
    }

    /// <summary>
    /// Отложенный запуск по сигналу таймера
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    internal void RunWhenIdle(object sender, EventArgs args)
    {
      Run();
    }

    #endregion

    #region Перезагрузка

    /// <summary>
    /// Повторная загрузка значений в редактор.
    /// Метод может вызываться только в процессе редактирования.
    /// Сначала должен вызываться <see cref="WriteData()"/>, затем выполняются манипуляции
    /// с <see cref="Documents"/>, затем вызывается <see cref="ReloadData()"/>.
    /// 
    /// Вызывает события чтения для элементов в <see cref="DocEditItems"/>, а также событие <see cref="AfterReadValues"/>.
    /// </summary>
    public void ReloadData()
    {
      foreach (IUIExtEditItem item in DocEditItems)
        item.BeforeReadValues();
      foreach (IUIExtEditItem item in DocEditItems)
      {
        try
        {
          item.ReadValues();
        }
        catch (Exception e)
        {
          string displayName;
          if (item.ChangeInfo == null)
            displayName = item.ToString();
          else
            displayName = item.ChangeInfo.DisplayName;
          EFPApp.ShowException(e, String.Format(Res.DocumentEditor_ErrTitle_ReloadValue, displayName));
        }
      }
      foreach (IUIExtEditItem item in DocEditItems)
        item.AfterReadValues();

      if (AfterReadValues != null)
      {
        DocEditEventArgs args = new DocEditEventArgs(this);
        AfterReadValues(this, args);
      }
    }

    #endregion

    #region Заголовок окна

    /// <summary>
    /// Статическое управление заголовком окна редактора.
    /// Если свойство установлено, то заголовок редактора будет иметь вид "Вид документа: DocumentTextValue (Редактирование)".
    /// Эту возможность следует использовать, если текстовое представление документа является слишком длинным.
    /// Обычно используется свойство <see cref="DocumentTextValueEx"/> (см. описание) для динамического управления заголовком.
    /// В заголовок не входит описание операции и признак наличия изменений (звездочка). Эти части присоединяются к заголовку автоматически.
    /// </summary>
    public string DocumentTextValue
    {
      get { return _DocumentTextValue; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _DocumentTextValue)
          return;
        _DocumentTextValue = value;

        if (_Dialog != null)
        {
          if (_Dialog.FormState != ExtEditDialogState.Initialization)
            InitFormTitle();
        }
      }
    }
    private string _DocumentTextValue;

    /// <summary>
    /// Динамическое управление заголовком окна редактора.
    /// Если, например, есть поле для ввода наименования документа <see cref="EFPTextBox"/>, то можно присоединить свойство
    /// Editor.DocumentTextValueEx=efpName.TextEx. При этом заголовок окна будет меняться синхронно с вводом пользователя.
    /// Можно использовать вычисляемые функции, если требуется собрать наименование из нескольких полей или, если "идентифицирующее" поле не является текстовым.
    /// Если свойство не установлено, то будет использоваться текстовое представление документа, но заголовок окна
    /// будет меняться только при нажатии кнопки "Запись".
    /// Если текстовое представление документа тоже не определено, используется <see cref="DBxDocTypeBase.SingularTitle"/>.
    /// Свойство игнорируется, если выполняется групповое редактирование.
    /// В заголовок не входит описание операции и признак наличия изменений (звездочка). Эти части присоединяются к заголовку автоматически.
    /// </summary>
    public DepValue<string> DocumentTextValueEx
    {
      get
      {
        InitDocumentTextValueEx();
        return _DocumentTextValueEx;
      }
      set
      {
        InitDocumentTextValueEx();
        _DocumentTextValueEx.Source = value;
      }
    }
    private DepInput<string> _DocumentTextValueEx;

    private void InitDocumentTextValueEx()
    {
      if (_DocumentTextValueEx == null)
      {
        _DocumentTextValueEx = new DepInput<string>(DocumentTextValue, DocumentTextValueEx_ValueChanged);
        _DocumentTextValueEx.OwnerInfo = new DepOwnerInfo(this, "DocumentTextValueEx");
      }
    }

    void DocumentTextValueEx_ValueChanged(object sender, EventArgs args)
    {
      this.DocumentTextValue = _DocumentTextValueEx.Value;
    }

    /// <summary>
    /// Инициализация заголовка формы
    /// Заголовок окна имеет формат: "(*) Имя-документа (Действие)"
    /// </summary>
    private void InitFormTitle()
    {
      #region 1. Название документа

      string s1;

      bool isSingle = Documents[0].DocCount == 1;
      DBxDocType docType = Documents[0].DocType;

      if (isSingle)
      {
        if (String.IsNullOrEmpty(DocumentTextValue))
        {
          Int32 docId = Documents[0][0].DocId;
          if (this.UI.TextHandlers.Contains(this.DocTypeUI.DocType.Name) && this.UI.DocProvider.IsRealDocId(docId))
            s1 = docType.SingularTitle + ": " + this.DocTypeUI.GetTextValue(docId);
          else
            s1 = docType.SingularTitle;
        }
        else
          s1 = docType.SingularTitle + ": " + DocumentTextValue;
      }
      else
        s1 = docType.PluralTitle + ": " + Documents[0].DocCount.ToString();

      #endregion

      #region 2. Операция

      string s2;

      switch (State)
      {
        case UIDataState.Edit:
          if (isSingle)
          {
            if (Documents[0][0].Deleted)
              s2 = Res.Editor_Msg_TitleRestore;
            else
              s2 = Res.Editor_Msg_TitleEdit;
          }
          else
            s2 = Res.Editor_Msg_TitleEdit;
          break;
        case UIDataState.Insert:
          s2 = Res.Editor_Msg_TitleInsert;
          break;
        case UIDataState.InsertCopy:
          s2 = Res.Editor_Msg_TitleInsertCopy;
          break;
        case UIDataState.Delete:
          s2 = Res.Editor_Msg_TitleDelete;
          break;
        case UIDataState.View:
          if (Documents.VersionView)
          {
            if (isSingle)
            {
              int version = Documents[0][0].Version;
              s2 = String.Format(Res.Editor_Msg_TitleViewVersion, version.ToString());
            }
            else
              s2 = Res.Editor_Msg_TitleViewPrevVersions;
          }
          else
          {
            // Обычный просмотр
            if (isSingle)
            {
              if (Documents[0][0].Deleted)
                s2 = Res.Editor_Msg_TitleViewDeleted;
              else
                s2 = Res.Editor_Msg_TitleView;
            }
            else
              //FForm.Text = DocType.PluralTitle + " (Просмотр " + RusNumberConvert.IntWithNoun(Documents[0].Count,
              //  "записи", "записей", "записей") + ")";
              s2 = Res.Editor_Msg_TitleView;
          }
          break;
        default:
          throw new BugException("State=" + State.ToString());
      }

      #endregion

      _Dialog.Title = String.Format(Res.Editor_Msg_Title, s1, s2);
      if (UI.DebugShowIds && State != UIDataState.Insert && isSingle)
        _Dialog.Title += " Id=" + Documents[0][0].DocId.ToString();
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Список просмотров для данного документа
    /// </summary>
    //private DocumentEditorPrintPreviews Previews;

    void Dialog_FormShown(object sender, EventArgs args)
    {
      try
      {
        // Форма редактора только что была выведена на экран.
        // Вызываем свое событие
        if (EditorShown != null)
        {
          DocEditEventArgs edArgs = new DocEditEventArgs(this);
          EditorShown(this, edArgs);
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private void Dialog_FormCheck(object sender, UIValidatingEventArgs args)
    {
      // Посылаем сообщение
      if (BeforeWrite != null)
      {
        DocEditCancelEventArgs args2 = new DocEditCancelEventArgs(this);
        BeforeWrite(this, args2);
        if (args2.Cancel)
          args.SetError(Res.Common_Err_Cancelled);
      }
    }



    private void Dialog_Writing(object sender, CancelEventArgs args)
    {
      if (args.Cancel)
        return;

      // Записываем редактируемые значения в dataset
      DocEditItems.WriteValues();

      // Пользовательская коррекция данных перед записью
      if (!UI.DocTypes[DocTypeName].DoWriting(this))
      {
        args.Cancel = false;
        return;
      }


      //if (!WriteData())
      //{
      //  args.Cancel = true;
      //  return;
      //}
      //DebugTools.DebugDataSet(FDocuments.DebugDataSet, "перед записью");

      if (!DoWrite(false))
      {
        args.Cancel = true;
        return;
      }

      if (_Dialog.FormState == ExtEditDialogState.ApplyClicked)
      {
        // Передаем изменение, возможно внесенные сервером, в управляющие элементы
        foreach (IUIExtEditItem item in DocEditItems)
          item.BeforeReadValues();
        foreach (IUIExtEditItem item in DocEditItems)
          item.ReadValues();
        foreach (IUIExtEditItem item in DocEditItems)
          item.AfterReadValues();

        Dialog.CancelButtonAsClose = true;
        ChangeInfo.ResetChanges();

        // Изменяем подсказки для кнопок
        switch (State)
        {
          case UIDataState.Edit:
            //FLastForm.OKButtonProvider.ToolTipText = "Закончить редактирование, сохранив внесенные изменения";
            _Dialog.CancelButtonToolTipText = Res.DocumentEditor_ToolTip_CancelEditApplied;
            //FLastForm.ApplyButtonProvider.ToolTipText = "Сохранить внесенные изменения и продолжить редактирование";
            break;
          case UIDataState.Insert:
          case UIDataState.InsertCopy:
            _Dialog.OKButtonToolTipText = Res.DocumentEditor_ToolTip_OkInsertApplied;
            _Dialog.CancelButtonToolTipText = Res.DocumentEditor_ToolTip_CancelInsertApplied;
            _Dialog.ApplyButtonToolTipText = Res.DocumentEditor_ToolTip_ApplyInsertApplied;
            break;
        }

        InitFormTitle();
      }
    }


    /// <summary>
    /// Проверка корректности введенных данных и копирование их из полей ввода
    /// редактора в редактируемый документ (<see cref="Documents"/>)
    /// Выполняемые действия:
    /// <list type="bullet">
    /// <item><description>1. Событие <see cref="FreeLibSet.Forms.Docs.DocumentEditor.BeforeWrite"/></description></item>
    /// <item><description>2. Проверка корректности значений полей <see cref="EFPFormProvider.ValidateForm()"/></description></item>
    /// <item><description>3. Запись полей в документ <see cref="FreeLibSet.UICore.IUIExtEditItem.WriteValues()"/></description></item>
    /// <item><description>4. Событие <see cref="FreeLibSet.Forms.Docs.DocTypeUI.Writing"/></description></item>
    /// </list>
    /// На шаге 1, 2 и 4 могут быть обнаружены ошибки. В этом случае дальнейшие
    /// действия не выполняются и возвращается false
    /// Обработчики событий <see cref="FreeLibSet.Forms.Docs.DocumentEditor.BeforeWrite"/> и <see cref="FreeLibSet.Forms.Docs.DocTypeUI.Writing"/> не
    /// должны вызывать метод <see cref="WriteData()"/>.
    /// Если редактор документа находится в режиме просмотра или удаления
    /// (<see cref="FreeLibSet.Forms.Docs.DocumentEditor.IsReadOnly"/>=true), то никакие действия не выполняются и
    /// возвращается true.
    /// </summary>
    /// <returns>true, если форма содержит корректные значения и обработчики не
    /// установили свойство Cancel</returns>
    public bool WriteData()
    {
      return _Dialog.WriteData();
    }

    ///// <summary>
    ///// Копирование данных из полей ввода в редактируемый документ (<see cref="Documents"/>)
    ///// без проверки корректности данных.
    ///// Выполняет те же действия, что и <see cref="WriteData()"/>:
    ///// 1. Событие <see cref="FreeLibSet.Forms.Docs.DocumentEditor.BeforeWrite"/>
    ///// 2. Запись полей в документ <see cref="FreeLibSet.UICore.IUIExtEditItem.WriteValues()"/>
    ///// 3. Событие <see cref="FreeLibSet.Forms.Docs.DocTypeUI.Writing"/>
    ///// Проверка значений полей не выполняется, ошибки, возникающие на шаге 1 и 3
    ///// игнорируются.
    ///// </summary>
    //public void NoValidateData()
    //{
    //  ValidateData2(false);
    //}

    private void ApplyClick2()
    {
      //DBxDocState DocState1 = Documents[0][0].DocState;

      if (!WriteData())
        return;
      DoWrite(true);

      //DBxDocState DocState2 = Documents[0][0].DocState;

    }

    /// <summary>
    /// Выполняем запись значений из управляющих элементов в документ,
    /// проверяем корректность данных и выполняем запись документа.
    /// Возвращаем true, если значения полей корректные и запись успешно
    /// выполнена.
    /// applyClicked=true при нажатии кнопки "Apply" и false при нажатии "ОК"
    /// </summary>
    /// <returns></returns>
    private bool DoWrite(bool applyClicked)
    {
      if (_InsideDoWrite)
      {
        EFPApp.ShowTempMessage(Res.DocumentEditor_Err_NestedWriting);
        return false;
      }
      _InsideDoWrite = true;
      bool res;
      try
      {
        res = DoWrite2(applyClicked);
      }
      finally
      {
        _InsideDoWrite = false;
      }
      return res;
    }

    private static bool _InsideDoWrite;

    private bool DoWrite2(bool applyClicked)
    {
      if (State == UIDataState.View)
        return true;

      if (String.IsNullOrEmpty(_Documents.ActionInfo))
        InitActionInfo();

      if (State == UIDataState.Delete)
      {
        // Ничего серверу передавать не надо
        try
        {
          foreach (DBxMultiDocs mDocs in Documents)
            mDocs.Delete();
          DoApplyChanges();
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, Res.DocumentEditor_ErrTitle_Deleting);
          return false;
        }

        _DataChanged = true;
        // Уведомление табличным просмотрам
        //AccDepClientExec.DocTableBrowsers.EditorExecuted(this);
        return true;
      } // ValidateState==Delete

      if (!TestCallerBeforeWrite())
        return false;

      try
      {
#if XXX
        if (State == EFPDataGridViewState.Insert && ImportArgs != null)
        {
          bool Imported = false;
          for (int i = 0; i < Documents.Count; i++)
          {
            if (Documents[i].DocState == DBxDocState.Insert)
            {
              if (Documents[i].SingleDoc.ImportId == 0)
              {
                Documents[i].SingleDoc.ImportId = ImportArgs.GetImportId(); // создается операция импорта
                ImportArgs.AddRefDocType(Documents[i].DocType);
                Imported = true;
              }
            }
          }
          if (Imported)
            ImportArgs.WriteRefDocTypes();
        }
#endif


        //Documents.ApplyChanges(true); // Все сразу
        DoApplyChanges(); // 26.05.2021

        // Посылаем извещение
        UI.DocTypes[DocTypeName].DoWrote(this);

        // Установка блокировки после создания документа
        if ((_State == UIDataState.Insert ||
           _State == UIDataState.InsertCopy) && // добавлено 05.07.2016
           _LockGuid == Guid.Empty)
        {
          DBxDocSelection editDocSel = _Documents.GetDocSelection(DBxDocState.Insert);
          if (!editDocSel.IsEmpty)
          {
            try
            {
              _LockGuid = Documents.AddLongLock(editDocSel);
            }
            catch (DBxDocsLockException e)
            {
              List<string> msgs = new List<string>();
              msgs.Add(Res.Editor_Err_LockForInsert);
              if (e.OldLock.UserId != 0)
                msgs.Add(String.Format(Res.Editor_Err_LockedByUser, UI.GetUserName(e.OldLock.UserId)));
              EFPApp.MessageBox(String.Join(Environment.NewLine, msgs.ToArray()),
                Res.Editor_ErrTitle_LockForInsert,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception e)
            {
              EFPApp.ShowException(e, Res.Editor_ErrTitle_LockForInsert);
            }
          }

        }
      }
      catch (Exception e)
      {
        /*
        if (LogoutTools.GetException<DBxAccessException>(e) != null)
        {
          DebugTools.LogoutException(e, "Редактор документа. Ошибка записи данных на сервере");
          EFPApp.MessageBox("Не удалось записать данные: " + e.Message,
            "Ошибка записи данных на сервере", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
          DebugTools.ShowException(e, "Ошибка записи данных на сервере");
         * */

        EFPApp.ShowException(e, Res.Editor_ErrTitle_Writing);

        return false;
      }
      // ???      FResultData=(DataSet)GetData(mode2);
      _DataChanged = true;

      if (_Dialog != null)
      {
        SubDocsChangeInfo.Changed = false;
        ExternalChangeInfo.Changed = false;
      }

      // Уведомление о выполнении записи
      if (AfterWrite != null)
      {
        DocEditEventArgs args = new DocEditEventArgs(this);
        AfterWrite(this, args);
      }

      // Уведомление табличным просмотрам
      //AccDepClientExec.DocTableBrowsers.EditorExecuted(this);

      // Сохраняем введенные значения полей для будущего использования
      for (int i = 0; i < Documents.Count; i++)
      {
        if (Documents[i].DocCount == 0)
          continue;
        DocTypeUI thisType = UI.DocTypes[Documents[i].DocType.Name];
        try
        {
          DBxArrayExtValues orgVals1 = null;
          if (_OrgVals != null)
            orgVals1 = _OrgVals[i];
          thisType.Columns.PerformPost(Documents[i].Values, orgVals1);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, String.Format(Res.DocumentEditor_ErrTitle_SaveColumnValues, thisType.DocType.SingularTitle));
        }
      }

      // Разрешаем команды меню "Еще"
      if (ciShowDocInfo != null)
        InitShowDocInfoEnabled();
      //if (ciShowDocRefs != null)
      //  ciShowDocRefs.Enabled = true;
      if (ciCopyDocRef != null)
        ciCopyDocRef.Enabled = true;
      if (ciSendDocSel != null)
        ciSendDocSel.Enabled = true;

      return true;
    }

    private void InitShowDocInfoEnabled()
    {
      // 11.04.2016
      for (int i = 0; i < Documents.Count; i++)
      {
        if (Documents[i].DocCount == 0)
          continue;
        if (!DocTypeViewHistoryPermission.GetAllowed(UI.DocProvider.UserPermissions, Documents[i].DocType.Name))
        {
          ciShowDocInfo.Enabled = false;
          return;
        }
      }
      ciShowDocInfo.Enabled = true;
    }

    private void DoApplyChanges()
    {
      DocumentViewHandler.CurrentHandler = Caller;
      try
      {
        Documents.ApplyChanges(true);
      }
      finally
      {
        DocumentViewHandler.CurrentHandler = null;
      }
    }

    private bool TestCallerBeforeWrite()
    {
      if (Caller == null)
        return true;
      /*  Заблокировано 15.03.2010 Выдаются ложные предупреждения для фильтров, 
       * использующих вычисляемые поля при создании документа (и при изменении!),
       * т.к. поля вычисляются сервером после записи
  
            ErrorMessageList Errors = new ErrorMessageList();
            for (int i = 0; i < Documents[0].Count; i++)
            {
              Caller.ValidateDocValues(Documents[0][i], Errors);
              if (Errors.Count > 0)
                break;
            }

            if (Errors.Count > 0)
            {
              StringBuilder sb = new StringBuilder();
              sb.Append("Введенные значения не соответствуют условиям фильтра.\r\h");
              for (int i=0; i<Errors.Count;i++)
              {
                if (Errors.Count>1)
                {
                  sb.Append((i+1));
                  sb.Append(". ");
                }
                sb.Append(Errors[i].Text );
                sb.Append(".\r\h");
              }
              if (Documents[0].Count==1)
                sb.Append("Сохранить документ? (Документ может оказаться невидимым после обновления табличного просмотра)");
              else
                sb.Append("Сохранить документы? (Документы могут оказаться невидимыми после обновления табличного просмотра)");
          
              return EFPApp.MessageBox(sb.ToString(),
                "Предупреждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
            }
       * */
      return true;
    }


    private void Dialog_FormClosed(object sender, EventArgs args)
    {
      if (Executed != null)
        Executed(this, null);

      ClearEditLock();

      //if (!Modal)
      //  DisposeWhenClosed();
    }


    /// <summary>
    /// Закрыть окно редактора.
    /// На момент вызова окно редактора должно быть открыто.
    /// Возвращает true, если форма успешно закрыта.
    /// Возвращает false, если окно закрыть не удалось (например, не выполнены условия корректности введенных данных).
    /// </summary>
    /// <param name="isOk">true - выполнить запись документа (симуляция нажатия кнопки "ОК"),
    /// false - выйти без записи. Проверка наличия несохраненных изменений не выполняется.</param>
    /// <returns>Было ли закрыто окно редактора</returns>
    public bool CloseForm(bool isOk)
    {
      return _Dialog.CloseForm(isOk);
    }

    #endregion

    #region Кнопка "Еще"

    /// <summary>
    /// Сюда можно добавить команды меню, которые будут доступны при нажатии кнопки "Ещё".
    /// Кроме команд, объявленных в прикладном коде, меню содержит команды "Информация о документе" и "Копировать ссылку на документ".
    /// </summary>
    public EFPCommandItems MoreCommandItems { get { return Dialog.MoreCommandItems; } }

    #region Добавление команд

    /// <summary>
    /// Добавление отладочных команд к меню "Еще"
    /// </summary>
    private void InitMoreCommands()
    {
      Dialog.MoreCommandItems.AddSeparator();

      if ((!MultiDocMode) && (!Documents.VersionView /* 09.09.2019 */))
      {
        ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
        ciShowDocInfo.MenuText = Res.Cmd_Menu_DocInfo;
        //ciDebugChanges.ToolTipText = "Информация о документе";
        //ciShowDocInfo.ShortCut = Keys.F12;
        ciShowDocInfo.ImageKey = "Information";
        ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
        ciShowDocInfo.GroupBegin = true;
        if (State == UIDataState.Insert || State == UIDataState.InsertCopy)
          ciShowDocInfo.Enabled = false;
        else
          InitShowDocInfoEnabled(); // 11.04.2016
        Dialog.MoreCommandItems.Add(ciShowDocInfo);

        /*
        ciShowDocRefs = new EFPCommandItem("Сервис", "ПросмотрСсылок");
        ciShowDocRefs.MenuText = "Ссылки на документ";
        ciShowDocRefs.ImageKey = "ПросмотрСсылок";
        ciShowDocRefs.Usage = EFPCommandItemUsage.Menu;
        ciShowDocRefs.Click += new EventHandler(ciShowDocRefs_Click);
        if (State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
          ciShowDocRefs.Enabled = false;
        Form.MoreButtonProvider.CommandItems.Add(ciShowDocRefs);
         * */
      }

      ciCopyDocRef = new EFPCommandItem("Edit", "CopyDocRec");
      if (MultiDocMode || Documents.Count > 1)
        ciCopyDocRef.MenuText = Res.Cmd_Menu_Edit_CopyMultiDocRefs;
      else
        ciCopyDocRef.MenuText = Res.Cmd_Menu_Edit_CopySingleDocRef;
      ciCopyDocRef.ImageKey = "Copy";
      ciCopyDocRef.Click += new EventHandler(ciCopyDocRef_Click);
      if (State == UIDataState.Insert || State == UIDataState.InsertCopy)
        ciCopyDocRef.Enabled = false;
      Dialog.MoreCommandItems.Add(ciCopyDocRef);

      ciSendDocSel = new EFPCommandItem("File", "SendToDocSel");
      if (MultiDocMode || Documents.Count > 1)
        ciSendDocSel.MenuText = Res.Cmd_Menu_File_SendToDocSelMultiDocs;
      else
        ciSendDocSel.MenuText = Res.Cmd_Menu_File_SendToDocSelSingleDoc;
      ciSendDocSel.ImageKey = "DBxDocSelection";
      ciSendDocSel.Click += new EventHandler(ciSendDocSel_Click);
      if (State == UIDataState.Insert || State == UIDataState.InsertCopy)
        ciSendDocSel.Enabled = false;
      Dialog.MoreCommandItems.Add(ciSendDocSel);

      if (UI.DebugShowIds)
        Dialog.AddDebugCommandItems();

      Dialog.MoreCommandItems.AddSeparator();

      if (State == UIDataState.View && ((!MultiDocMode) || UI.DocTypes[DocTypeName].CanMultiEdit) &&
        (!Documents.VersionView /* 09.09.2019 */))
      {
        EFPCommandItem ciOpenEdit = new EFPCommandItem("Edit", "ReopenForEdit");
        ciOpenEdit.MenuText = Res.Cmd_Menu_Edit_ReopenForEdit;
        ciOpenEdit.ImageKey = "Edit";
        ciOpenEdit.Click += ciOpenEdit_Click;
        ciOpenEdit.Enabled = UI.DocProvider.DBPermissions.TableModes[DocTypeName] == DBxAccessMode.Full;
        Dialog.MoreCommandItems.Add(ciOpenEdit);
      }
      if (State == UIDataState.InsertCopy)
      {
        EFPCommandItem ciViewSrcDoc = new EFPCommandItem("Edit", "ViewSourceDoc");
        ciViewSrcDoc.MenuText = Res.Cmd_Menu_View_ViewSourceDoc;
        ciViewSrcDoc.ImageKey = "View";
        ciViewSrcDoc.Click += ciViewSrcDoc_Click;
        Dialog.MoreCommandItems.Add(ciViewSrcDoc);
      }

      if ((State == UIDataState.Edit || State == UIDataState.View) &&
        (!Documents.VersionView /* 09.09.2019 */))
      {
        EFPCommandItem ciDeleteThis = new EFPCommandItem("Edit", "DeleteDoc");
        if (MultiDocMode)
          ciDeleteThis.MenuText = Res.Cmd_Menu_Edit_DeleteMultiDocs;
        else
          ciDeleteThis.MenuText = Res.Cmd_Menu_Edit_DeleteSingleDoc;
        ciDeleteThis.ImageKey = "Delete";
        ciDeleteThis.Click += ciDeleteThis_Click;
        ciDeleteThis.Enabled = UI.DocProvider.DBPermissions.TableModes[DocTypeName] == DBxAccessMode.Full;
        Dialog.MoreCommandItems.Add(ciDeleteThis);
      }

      Dialog.MoreCommandItems.AddSeparator();
    }

    #endregion

    #region Информация о документе

    private static string _LastDocTypeName = String.Empty;

    private EFPCommandItem ciShowDocInfo;

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (Documents.Count == 1)
        DoShowDocInfo(0);
      else
      {
        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_DocInfo);
        dlg.ImageKey = "Information";
        dlg.GroupTitle = Res.DocInfo_Title_SelectDoc;
        dlg.Items = new string[Documents.Count];
        for (int i = 0; i < Documents.Count; i++)
        {
          dlg.Items[i] = Documents[i].DocType.SingularTitle;
          if (Documents[i].DocType.Name == _LastDocTypeName)
            dlg.SelectedIndex = i;
        }

        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        _LastDocTypeName = Documents[dlg.SelectedIndex].DocType.Name;
        DoShowDocInfo(dlg.SelectedIndex);
      }
    }

    private void DoShowDocInfo(int docTypeIndex)
    {
      Int32 docId = Documents[docTypeIndex][0].RealDocId;
      if (docId == 0)
      {
        EFPApp.ShowTempMessage(String.Format(Res.Editor_Err_DocNotWritten, Documents[docTypeIndex].DocType.SingularTitle));
        return;
      }
      UI.DocTypes[Documents[docTypeIndex].DocType.Name].ShowDocInfo(docId);
    }

    #endregion

    #region Ссылки на документ

#if XXX
    private EFPCommandItem ciShowDocRefs;

    void ciShowDocRefs_Click(object Sender, EventArgs Args)
    {
      if (Documents.Count == 1)
        DoShowDocRefs(0);
      else
      {
        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = "Информация о документе";
        dlg.ImageKey = "ПросмотрСсылок";
        dlg.GroupTitle = "Документ";
        dlg.Items = new string[Documents.Count];
        for (int i = 0; i < Documents.Count; i++)
        {
          dlg.Items[i] = Documents[i].DocType.SingularTitle;
          if (Documents[i].DocType.Name == LastDocTypeName)
            dlg.SelectedIndex = i;
        }

        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        LastDocTypeName = Documents[dlg.SelectedIndex].DocType.Name;
        DoShowDocRefs(dlg.SelectedIndex);
      }
    }


    private void DoShowDocRefs(int DocTypeIndex)
    {
      Int32 DocId = Documents[DocTypeIndex].SingleDoc.RealDocId;
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("Документ \"" + Documents[DocTypeIndex].DocType.SingularTitle + "\" не был записан");
        return;
      }

      RowRefViewer.PerformView(Documents[DocTypeIndex].DocType.Name, DocId);
    }
#endif
    #endregion

    #region Копирование ссылки на документ

    private EFPCommandItem ciCopyDocRef;

    void ciCopyDocRef_Click(object sender, EventArgs args)
    {
      UI.CopyDocSel(Documents.DocSelection);
    }

    #endregion

    #region Открытие выборки документов

    private EFPCommandItem ciSendDocSel;

    void ciSendDocSel_Click(object sender, EventArgs args)
    {
      UI.ShowDocSel(Documents.DocSelection);
    }

    #endregion

    #region Переоткрыть на редактирование

    void ciOpenEdit_Click(object sender, EventArgs args)
    {
      if (!CloseForm(false))
        return;

      DocumentEditor de2 = new DocumentEditor(UI, DocTypeName, UIDataState.Edit, Documents[0].DocIds);
      de2.Modal = Modal;
      EFPApp.IdleHandlers.AddSingleAction(de2.RunWhenIdle); // 23.06.2021
    }

    #endregion

    #region Просмотр исходного документа при копировании

    void ciViewSrcDoc_Click(object sender, EventArgs args)
    {
      // TODO:
      throw new NotImplementedException();
      //DocType.PerformEditing(Documents[0].SourceDocId, true);
    }

    #endregion

    #region Удаление этого документа

    void ciDeleteThis_Click(object sender, EventArgs args)
    {
      if (!CloseForm(false))
        return;

      DocumentEditor de2 = new DocumentEditor(UI, DocTypeName, UIDataState.Delete, Documents[0].DocIds);
      de2.Modal = Modal;
      EFPApp.IdleHandlers.AddSingleAction(de2.RunWhenIdle); // 23.06.2021
    }

    #endregion

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Если true, то выполняется просмотр или удаление документа, поля
    /// документа не могут редактироваться.
    /// Возвращаемое значения зависит исключительно от свойства <see cref="State"/> и определяется в конструкторе.
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        return _State == UIDataState.View ||
          _State == UIDataState.Delete;
      }
    }

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Поиск редактора

    /// <summary>
    /// Поиск уже открытого окна редактора документов.
    /// Достаточно совпадения одного идентификатора документов.
    /// Фиктивные идентификаторы для несохраненных документов не учитываются.
    /// Никаких действий над найденным редактором не выполняется.
    /// </summary>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Найденный редактор или null, если редактор еще не открыт.</returns>
    public static DocumentEditor FindEditor(DBUI ui, string docTypeName, IIdSet<Int32> docIds)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentNullException("ui");
#endif

      DBxDocSelection docSel = ui.CreateDocSelection(docTypeName, docIds);
      return FindEditor(docSel);
    }

    /// <summary>
    /// Поиск уже открытого окна редактора документов.
    /// Достаточно совпадения одного идентификатора документов в выборке.
    /// Фиктивные идентификаторы для несохраненных документов не учитываются.
    /// Никаких действий над найденным редактором не выполняется.
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    /// <returns>Найденный редактор или null, если редактор еще не открыт.</returns>
    public static DocumentEditor FindEditor(DBxDocSelection docSel)
    {
#if DEBUG
      if (docSel == null)
        throw new ArgumentNullException("docSel");
#endif

      if (docSel.IsEmpty)
        return null;

      ExtEditDialog[] dlgs = ExtEditDialog.GetOpenForms();
      foreach (ExtEditDialog dlg in dlgs)
      {
        DocumentEditor de = dlg.Tag as DocumentEditor;
        if (de != null)
        {
          DBxDocSelection thisDocSel = de.Documents.DocSelection;
          if (thisDocSel.DBIdentity != docSel.DBIdentity)
            continue; // другой набор данных
          if (thisDocSel.ContainsAny(docSel))
            return de;
        }
      }

      return null;
    }

    #endregion
  }
}
