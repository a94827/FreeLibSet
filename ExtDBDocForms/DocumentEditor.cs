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
    /// Используется ClientDocType.PerformEditing()
    /// </summary>
    /// <param name="ui">Доступ к пользовательскому интерфейсу</param>
    /// <param name="docTypeName">Имя типа документа</param>
    /// <param name="state">Режим редактирования</param>
    /// <param name="docIds">Список идентификаторов документов для редактирования, просмотра, удаления или копирования</param>
    public DocumentEditor(DBUI ui, string docTypeName, EFPDataGridViewState state, Int32[] docIds)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentNullException("ui");
      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      if (!ui.DocProvider.DocTypes.Contains(docTypeName))
        throw new ArgumentException("Неизвестный тип документа \"" + docTypeName + "\"", "docTypeName");
#endif
      _UI = ui;
      _State = state;

      DBxDocType docType = _UI.DocTypes[docTypeName].DocType;
      DBxAccessMode accessMode = ui.DocProvider.DBPermissions.TableModes[docTypeName];

      switch (accessMode)
      {
        case DBxAccessMode.None:
          EFPApp.ErrorMessageBox("У Вас нет права доступа к документам \"" + docType.PluralTitle + "\"");
          return;
        case DBxAccessMode.ReadOnly:
          switch (_State)
          {
            case EFPDataGridViewState.View:
              break;
            case EFPDataGridViewState.Edit:
              _State = EFPDataGridViewState.View;
              break;
            default:
              EFPApp.ErrorMessageBox("У Вас нет права добавлять, удалять или редактировать документы \"" + docType.PluralTitle + "\". Есть право только на просмотр", "Доступ запрещен");
              return;
          }
          break;
      }


      _Documents = new DBxDocSet(ui.DocProvider);
      _Documents.CheckDocs = true;
      DBxMultiDocs mDocs = _Documents[docTypeName];

      if (_State == EFPDataGridViewState.Edit && mDocs.Permissions == DBxAccessMode.ReadOnly)
        _State = EFPDataGridViewState.View; // чтобы не выбрасывалось исключение

      try
      {
        switch (_State)
        {
          case EFPDataGridViewState.Edit:
            try
            {
              mDocs.Edit(docIds);
            }
            catch (DBxAccessException)
            {
              _State = EFPDataGridViewState.View;
              // 09.06.2015
              // В большинстве случаев, при этой ошибке документы находятся в режиме View,
              // но теоретически может быть, что часть документов перешло в режим Edit, а часть - нет.
              // Или, документы вообще не загрузились
              if (mDocs.DocCount != docIds.Length || mDocs.DocState != DBxDocState.View)
              {
                mDocs.ClearList();
                mDocs.View(docIds);
              }
            }
            //DebugTools.DebugDataSet(FDocuments.DebugDataSet, "загружено");
            //int x=Docs.DocCount;
            break;
          case EFPDataGridViewState.Insert:
            mDocs.Insert();
            break;
          case EFPDataGridViewState.InsertCopy:
            mDocs.InsertCopy(docIds);
            break;
          case EFPDataGridViewState.View:
            mDocs.View(docIds);
            break;
          case EFPDataGridViewState.Delete:
            mDocs.View(docIds);
            break;
          default:
            throw new InvalidEnumArgumentException("state", (int)_State, typeof(EFPDataGridViewState));
        }
      }
      catch (DBxAccessException e)
      {
        EFPApp.ErrorMessageBox(e.Message, "Доступ запрещен");
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
    /// При использовании этого конструктора выставляется свойство DocumentsAreExternal
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
        throw new ArgumentException("Не задано ни одного типа документов", "documents");

      if (documents.DocProvider != ui.DocProvider)
        throw new ArgumentException("Используются разные объекты DocProvider");
#endif

      _UI = ui;
      _Documents = documents;
      _Documents.CheckDocs = true;

      switch (_Documents[0].DocState)
      {
        case DBxDocState.Edit:
          _State = EFPDataGridViewState.Edit;
          break;
        case DBxDocState.Insert:
          _State = EFPDataGridViewState.Insert;
          break;
        case DBxDocState.View:
          _State = EFPDataGridViewState.View;
          break;
        default:
          throw new ArgumentException("Загруженные документы " + _Documents[0].ToString() + " находятся в недопустимом состоянии");
      }

      _DataChanged = false;
      Modal = false;
      // FSubDocs = new DocEditSubDocs(this);
      _DocumentsAreExternal = true;

      _Properties = new NamedValues();
    }

    /// <summary>
    /// Инициализация свойства DBxDocSet.ActionInfo
    /// </summary>
    private void InitActionInfo()
    {
      DBxMultiDocs mainDocs = _Documents[0];
      switch (_State)
      {
        case EFPDataGridViewState.Edit:
          bool hasEdit = false;
          bool hasRestore = false;
          for (int i = 0; i < mainDocs.DocCount; i++)
          {
            if (mainDocs[i].Deleted)
              hasRestore = true;
            else
              hasEdit = true;
          }

          string text1;
          if (hasRestore)
          {
            if (hasEdit)
              text1 = "Редактирование / восстановление";
            else
              text1 = "Восстановление";
          }
          else
            text1 = "Редактирование";

          // 05.11.2015
          // UI.Text.GetTextValue(MainDocs[0]) не будет работать правильно, если текстовое представление документа
          // зависит от вычисляемых полей. Вычисление выполняется на стороне сервера.
          // Можно в ActionInfo сделать что-нибудь вроде %DOCTEXT% для подстановки на стороне сервера

          if (mainDocs.DocCount == 1)
            _Documents.ActionInfo = text1 + " документа \"" + mainDocs.DocType.SingularTitle + "\""; // "\": "+UI.Text.GetTextValue(MainDocs[0]);
          else
            //FDocuments.ActionInfo = Text1 + " " + RusNumberConvert.IntWithNoun(MainDocs.Count,
            //  "документа", "документов", "документов") + " \"" + DocType.PluralTitle + "\"";
            _Documents.ActionInfo = text1 + " документов \"" + mainDocs.DocType.PluralTitle + "\"";
          break;
        case EFPDataGridViewState.Insert:
          _Documents.ActionInfo = "Создание документа \"" + mainDocs.DocType.SingularTitle + "\""; // "\": "+UI.Text.GetTextValue(MainDocs[0]);
          break;
        case EFPDataGridViewState.InsertCopy:
          _Documents.ActionInfo = "Копирование документа \"" + mainDocs.DocType.SingularTitle + "\""; // "\": "+UI.Text.GetTextValue(MainDocs[0]);
          break;
        case EFPDataGridViewState.View:
          break;
        case EFPDataGridViewState.Delete:
          if (mainDocs.DocCount == 1)
            _Documents.ActionInfo = "Удаление документа \"" + mainDocs.DocType.SingularTitle + "\"";
          else
            //FDocuments.ActionInfo = "Удаление " + RusNumberConvert.IntWithNoun(MainDocs.Count,
            //  "документа", "документов", "документов") + " \"" + DocType.PluralTitle + "\"";
            _Documents.ActionInfo = "Удаление документов \"" + mainDocs.DocType.PluralTitle + "\"";
          break;
        default:
          throw new BugException("Неизвестное значение State=" + State.ToString());
      }
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Объекты редактируемых документов
    /// </summary>
    public DBxDocSet Documents { get { return _Documents; } }
    private DBxDocSet _Documents;

    /// <summary>
    /// Обработчики пользовательского интерфейса
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    /// <summary>
    /// Имя типа главного редактируемого документа
    /// </summary>
    public string DocTypeName { get { return Documents[0].DocType.Name; } }

    /// <summary>
    /// Обработчики пользовательского интерфейса для основного типа документов
    /// </summary>
    public DocTypeUI DocTypeUI { get { return UI.DocTypes[DocTypeName]; } }

    /// <summary>
    /// Возвращает true, если использовался вариант конструктора, использующий
    /// предварительно загруженные документы, то есть свойство Documents было
    /// инициализировано снаружи редактора. 
    /// Возвращает false, если использовался основной вариант конструктора с 
    /// указанием типа документа, режима и идентификаторов.
    /// </summary>
    public bool DocumentsAreExternal { get { return _DocumentsAreExternal; } }
    private bool _DocumentsAreExternal;

    /// <summary>
    /// Режим: Редактирование, добавление, удаление или просмотр документа
    /// </summary>
    public EFPDataGridViewState State { get { return _State; } }
    private EFPDataGridViewState _State;


    ///// <summary>
    ///// Массив идентификаторов редактируемых документов (поля "RefId")
    ///// </summary>
    //    public int[] Values{ get{return FIds;} }

    /// <summary>
    /// True, если редактируется, просматривается или удаляется сразу
    /// несколько документов. 
    /// </summary>
    public bool MultiDocMode
    { get { return Documents[0].DocCount != 1; } }

    /// <summary>
    /// Доступ к значениям документов.
    /// Если в редакторе открыты документы нескольких видов, значения относятся к "основным" документам
    /// (Documents[0].Values).
    /// Если одновременно открыто несколько документов, то могут быть "серые" значения.
    /// </summary>
    public IDBxDocValues MainValues
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
    /// заданного в конструкторе, который используется в Documents[0])
    /// Вызывается после обработки значений в ClientFields
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
    /// Предупреждение. Редактор может запускаться в режиме несохраненных изменений
    /// даже, если это свойство не установлено, в случае, когда одна из процедур
    /// инициализации изменила загруженные данные
    /// </summary>
    public bool StartWithChanges
    {
      get { return _StartWithChanges; }
      set
      {
        // TODO:
        //if (IsExecuting)
        //  throw new InvalidOperationException("Свойство \"StartWithChanges\" должно устанавливаться до запуска редактора");
        if (value && State == EFPDataGridViewState.View)
          throw new InvalidOperationException("Нельзя устанавливать свойство \"StartWithChanges\" в режиме просмотра");
        _StartWithChanges = value;
      }
    }
    private bool _StartWithChanges;

    /// <summary>
    /// Форма Windows многостраничного редактора документа
    /// </summary>
    internal DocEditForm Form { get { return _Form; } }
    private DocEditForm _Form;

    /// <summary>
    /// Объекты синхронизации для редактора
    /// </summary>
    public DepSyncCollection Syncs
    {
      get
      {
        if (_Form == null)
          return null;
        else
          return _Form.FormProvider.Syncs;
      }
    }

    internal DocEditItemList DocEditItems { get { return _Form.DocEditItems; } }

    /// <summary>
    /// Устанавливается после закрытия редактора в true, если данные
    /// были изменены (форма закрыта нажатием "ОК" или было нажатие "Apply")
    /// В режиме ValidateState=Delete показывает, что данные были удалены
    /// </summary>
    public bool DataChanged { get { return _DataChanged; } }
    private bool _DataChanged;

    /// <summary>
    /// Отслеживание изменений для рисования звездочки в заголовке формы
    /// Объект существует только в процессе работы редактора.
    /// </summary>
    public DepChangeInfoList ChangeInfo { get { return _Form.ChangeInfoList; } }

    /// <summary>
    /// Объект для отслеживания изменений в поддокументах. Установка свойства
    /// Changed в True означает наличие изменений.
    /// Объект существует только в процессе работы редактора.
    /// </summary>
    public DepChangeInfoItem SubDocsChangeInfo { get { return _SubDocsChangeInfo; } }
    private DepChangeInfoItem _SubDocsChangeInfo;


    /// <summary>
    /// Внешний флаг наличия изменений.
    /// Объект существует только в процессе работы редактора.
    /// Исходное состояние признака изменений определяется свойством StartWithChanges
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
    private DBxMemoryDocValues[] _OrgVals;

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
    /// Обработчик события "BeforeWrite", "AfterWrite" (или другой в ClientDocType)
    /// может проверить свойство, чтобы определить, выполняется ли нажатие кнопки
    /// записи или был программный вызов ValidateData().
    /// </summary>
    public bool IsInsideWriting { get { return _IsInsideWriting; } }
    private bool _IsInsideWriting;

    /// <summary>
    /// Пользовательские данные
    /// </summary>
    public NamedValues Properties { get { return _Properties; } }
    private NamedValues _Properties;

    /// <summary>
    /// Идентификатор длительной блокировки, если она была установлена
    /// </summary>
    private Guid _LockGuid;

    #endregion

    #region Общие события

    /// <summary>
    /// Вызывается сразу после того, как форма редактора выведена на экран.
    /// Объекты синхронизации связаны в группы
    /// </summary>
    public event DocEditEventHandler EditorShown;

    /// <summary>
    /// Вызывается после того, как были установлены значения всех полей перед началом
    /// редактирования. На момент вызова форма еще не выведена на экран и объекты
    /// синхронизации не связаны в группы
    /// </summary>
    public event DocEditEventHandler AfterReadValues;

    /// <summary>
    /// Вызывается при нажатии кнопок "ОК" или "Запись" перед посылкой данных
    /// серверу. 
    /// Вызывается в режимах Edit, Insert и InsertCopy
    /// Установка Args.Cancel=true предотвращает запись данных и закрытие редактора
    /// Программа должна вывести сообщение пользователю о причинах отмены.
    /// На момент вызова данные формы еще не перенесены в документы.
    /// Если требуется обработка значений в DocumentEditor.Documents, используйте
    /// событие ClientDocType.Writing
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
    /// Если форма открывается в модальном режиме, то ожидается завершение работы с формой
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

        EFPApp.ShowException(e, "Ошибка запуска редактора");
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
        case EFPDataGridViewState.View:
        case EFPDataGridViewState.Edit:
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

      if (State == EFPDataGridViewState.Delete)
      {
        #region Проверка прав на удаление

        string errorText;
        if (!_Documents.DocProvider.DocPermissions.TestDocuments(_Documents, DBxDocPermissionReason.BeforeDelete, out errorText))
        {
          EFPApp.ErrorMessageBox(errorText, "Нельзя удалить документ");
          return;
        }

        #endregion

        #region Запрос подтверждения

        StringBuilder sb = new StringBuilder();
        sb.Append("Удалить ");
        if (Documents.Count == 1 && Documents[0].DocCount == 1)
        {
          sb.Append("документ \"");
          sb.Append(Documents[0].DocType.SingularTitle);
          sb.Append("\"");

          // 25.11.2015
          sb.Append(" (");
          try
          {
            sb.Append(UI.TextHandlers.GetTextValue(Documents[0][0]));
          }
          catch { } // на всякий случай
          sb.Append(")");
        }
        else
        {
          for (int i = 0; i < Documents.Count; i++)
          {
            //if (Documents[i].Mode != DocMode.Delete)
            //  continue; проверка не действует

            if (i > 0)
            {
              if (i == Documents.Count - 1)
                sb.Append(" и ");
              else
                sb.Append(", ");
            }
            if (Documents[i].DocCount == 1)
            {
              sb.Append("1 документ \"");
              sb.Append(Documents[i].DocType.SingularTitle);
              sb.Append("\"");
            }
            else
            {
              sb.Append(Documents[i].DocCount.ToString());
              sb.Append(" документа(ов) \"");
              sb.Append(Documents[i].DocType.PluralTitle);
              sb.Append("\"");
            }
          }
        }
        sb.Append("?");
        if (EFPApp.MessageBox(sb.ToString(), "Подтверждение удаления",
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
            StringBuilder sb = new StringBuilder();
            sb.Append("Не удалось установить блокировку для редактирования. ");
            sb.Append("Документ заблокирован");
            if (e.OldLock.UserId != 0)
            {
              sb.Append(" пользователем ");
              sb.Append(UI.GetUserName(e.OldLock.UserId));
            }
            sb.Append(". Документ будет открыт для просмотра");
            EFPApp.MessageBox(sb.ToString(),
              "Нельзя редактировать документ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Documents.ChangeDocState(DBxDocState.Edit, DBxDocState.View);
            _State = EFPDataGridViewState.View;
          }
        }

        EFPApp.BeginWait("Открытие редактора");
        try
        {
          _Form = new DocEditForm(this, this.State);
          try
          {
            _Form.FormProvider.ConfigSectionName = "Ed_" + Documents[0].DocType.Name; // 09.06.2021

            #region Список изменений

            _SubDocsChangeInfo = new DepChangeInfoItem();
            _SubDocsChangeInfo.DisplayName = "Изменения в поддокументах";
            ChangeInfo.Add(_SubDocsChangeInfo);

            _ExternalChangeInfo = new DepChangeInfoItem();
            _ExternalChangeInfo.DisplayName = "Внешние изменения";
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
            _OrgVals = new DBxMemoryDocValues[Documents.Count];
            if (!IsReadOnly)
            {
              for (int i = 0; i < Documents.Count; i++)
              {
                if (Documents[i].DocCount == 1)
                  _OrgVals[i] = new DBxMemoryDocValues(Documents[i][0].Values);
              }
            }

            _Form.CorrectSize();
            if (State == EFPDataGridViewState.View)
            {
              _Form.CancelButtonProvider.Visible = false;
              _Form.CancelButton = _Form.OKButtonProvider.Control;
            }
            if (State == EFPDataGridViewState.View || State == EFPDataGridViewState.Delete)
              _Form.ApplyButtonProvider.Visible = false;
            else
            {
              _Form.ApplyButtonProvider.Control.Click += new EventHandler(ApplyClick);
            }

            // Кнопка "Еще"
            InitMoreCommands();
            _Form.MoreButtonProvider.Visible = (_Form.MoreButtonProvider.CommandItems.Count > 0);

            // Подсказки для кнопок
            switch (State)
            {
              case EFPDataGridViewState.Edit:
                _Form.OKButtonProvider.ToolTipText = "Закончить редактирование, сохранив внесенные изменения";
                _Form.CancelButtonProvider.ToolTipText = "Закончить редактирование без сохранения внесенных изменений";
                _Form.ApplyButtonProvider.ToolTipText = "Сохранить внесенные изменения и продолжить редактирование";
                break;
              case EFPDataGridViewState.Insert:
              case EFPDataGridViewState.InsertCopy:
                _Form.OKButtonProvider.ToolTipText = "Создать новую запись и закончить редактирование";
                _Form.CancelButtonProvider.ToolTipText = "Закончить редактирование без сохранения введенных значений";
                _Form.ApplyButtonProvider.ToolTipText = "Создать новую запись и продолжить ее редактирование";
                break;
              case EFPDataGridViewState.Delete:
                _Form.OKButtonProvider.ToolTipText = "Удалить просматриваемую запись";
                _Form.CancelButtonProvider.ToolTipText = "Закрыть просмотр, не удаляя запись";
                break;
              case EFPDataGridViewState.View:
                _Form.OKButtonProvider.ToolTipText = "Закрыть просмотр";
                break;
            }
            _Form.MoreButtonProvider.ToolTipText = "Дополнительные команды редактора";

            // Инициализируем значения
            DocEditItems.ReadValues();

            if (AfterReadValues != null)
            {
              DocEditEventArgs Args = new DocEditEventArgs(this);
              AfterReadValues(this, Args);
            }

            ChangeInfo.ResetChanges(); // 12.08.2015

            _Form.Icon = GetEditorStateIcon(State);
            InitFormTitle(); // после вызова ReadValues, т.к. заголовок может быть другим


            _Form.FormProvider.FormClosing += new FormClosingEventHandler(Form_FormClosing);
            _Form.FormProvider.FormClosed += new FormClosedEventHandler(Form_FormClosed);
            _Form.VisibleChanged += new EventHandler(Form_VisibleChanged);

            // Посылка извещение при переключении страницы
            //FForm.MainTabControl.SelectedIndexChanged += new EventHandler(TabIndexChanged);
            //TabIndexChanged(null, null); // сразу посылаем извещение для первой страницы

            // Добавляем закладки для предварительного просмотра
            //Previews = new DocumentEditorPrintPreviews(this);

            //ClientExec.DebugServer = false;
            //System.Diagnostics.Trace.WriteLine("******* Щас будет форма");
          }
          catch
          {
            _Form.Dispose();
            _Form = null;
            throw;
          }
        }
        finally
        {
          EFPApp.EndWait();
        }

        //if (AccDepClientExec.ModalEditing)
        //  FModal = true;

        if (Modal)
        {
          EFPApp.ShowDialog(_Form, true);
          // ClearEditLock();
        }
        else
        {
          //DisposeAfterExecute = false;
          // EFPApp.ShowMdiChild(FForm);
          EFPApp.ShowFormOrDialog(_Form); // 10.03.2016
        }
      }
      else
      {
        // Редактирование без формы
        if (State != EFPDataGridViewState.View)
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
            thisType.Columns.PerformInsert(Documents[i][0].Values, State == EFPDataGridViewState.InsertCopy);

            // В режиме создания нового документа, если редактор вызывается из RBExec, 
            // пытаемся применить установленные фильтры к новому документу до редактирования.
            // Возможно, некоторые поля окажутся заполненными
            if (i == 0 && State == EFPDataGridViewState.Insert && Caller != null)
              Caller.InitNewDocValues(Documents[i][0]);
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, "Ошибка при инициализации новых значений документа \"" + thisType.DocType.SingularTitle + "\" в редакторе");
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
    /// Сначала должен вызываться ValidateData(), затем выполняются манипуляции
    /// с Documents, затем вызывается ReloadData().
    /// Вызывает событие AfterReadValues
    /// </summary>
    public void ReloadData()
    {

      foreach (IDocEditItem item in DocEditItems)
        item.BeforeReadValues();
      foreach (IDocEditItem item in DocEditItems)
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
          EFPApp.ShowException(e, "Ошибка при считывании значения \"" + displayName + "\"");
        }
      }
      foreach (IDocEditItem item in DocEditItems)
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
    /// Обычно используется свойство DocumentTextValueEx (см. описание) для динамического управления.
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

        if (_Form != null)
        {
          if (_Form.FormProvider.HasBeenShown)
            InitFormTitle();
        }
      }
    }
    private string _DocumentTextValue;

    /// <summary>
    /// Динамическое управление заголовком окна редактора.
    /// Если, например, есть поле для ввода наименования документа EFPTextBox, то можно присоединить свойство
    /// Editor.DocumentTextValueEx=efpName.TextEx. При этом заголовок окна будет меняться синхронно с вводом пользователя.
    /// Можно использовать вычисляемые функции, если требуется собрать наименование из нескольких полей или, если "идентифицирующее" поле не является текстовым.
    /// Если свойство не установлено, то будет использоваться текстовое представление документа, но заголовок окна
    /// будет меняться только при нажатии кнопки "Запись".
    /// Если текстовое представление документа тоже не определено, используется DBxDocType.SingularTutle.
    /// Свойство игнорируется, если выполняется групповое редактирование.
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
        case EFPDataGridViewState.Edit:
          if (isSingle)
          {
            if (Documents[0][0].Deleted)
              s2 = "(Восстановление)";
            else
              s2 = "(Редактирование)";
          }
          else
            s2 = "(Редактирование)";
          break;
        case EFPDataGridViewState.Insert:
          s2 = "(Создание)";
          break;
        case EFPDataGridViewState.InsertCopy:
          s2 = "(Создание копии)";
          break;
        case EFPDataGridViewState.Delete:
          s2 = "(Удаление)";
          break;
        case EFPDataGridViewState.View:
          if (Documents.VersionView)
          {
            if (isSingle)
            {
              int version = Documents[0][0].Version;
              s2 = "(Просмотр версии " + version.ToString() + ")";
            }
            else
              s2 = "(Просмотр предыдущих версий)";
          }
          else
          {
            // Обычный просмотр
            if (isSingle)
            {
              if (Documents[0][0].Deleted)
                s2 = "(Просмотр удаленного документа)";
              else
                s2 = "(Просмотр)";
            }
            else
              //FForm.Text = DocType.PluralTitle + " (Просмотр " + RusNumberConvert.IntWithNoun(Documents[0].Count,
              //  "записи", "записей", "записей") + ")";
              s2 = "(Просмотр)";
          }
          break;
        default:
          throw new BugException("Неизвестное значение State=" + State.ToString());
      }

      #endregion

      _Form.Text = s1 + " " + s2;
      if (UI.DebugShowIds && State != EFPDataGridViewState.Insert && isSingle)
        _Form.Text += " Id=" + Documents[0][0].DocId.ToString();
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Список просмотров для данного документа
    /// </summary>
    //private DocumentEditorPrintPreviews Previews;

    void Form_VisibleChanged(object sender, EventArgs args)
    {
      try
      {
        if (_Form.Visible)
        {
          // Форма редактора только что была выведена на экран.
          // Вызываем свое событие
          if (EditorShown != null)
          {
            DocEditEventArgs edArgs = new DocEditEventArgs(this);
            EditorShown(this, edArgs);
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки события VisibleChanged");
      }
    }


    /*
    private bool ReadyPreviewPage(DocumentEditorPrintPreview obj)
    {
      if (!ValidateData())
        return false;

      try
      {
        obj.InitPreview();
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Ошибка инициализации просмотра документа");
      }
      return true;
    }
     * */

    /*
    /// <summary>
    /// После того, как было переключение на страницу
    /// </summary>
    private void TabIndexChanged(object Sender, EventArgs Args)
    {
      try
      {
        int CurrentPageIndex = FForm.MainTabControl.SelectedIndex;
        if (CurrentPageIndex < Pages.Count)
        {
          try
          {
            Pages[CurrentPageIndex].CheckPageShow();
          }
          catch (Exception e)
          {
            DebugTools.ShowException(e, "Ошибка обработчика активации страницы редактора документа");
          }
        }
        else
        {
          if (CurrentPageIndex < 0 || CurrentPageIndex >= FForm.MainTabControl.TabCount)
            return;
          DocumentEditorPrintPreview PreviewObj = Previews.FindPage(CurrentPageIndex);
          if (PreviewObj != null)
            ReadyPreviewPage(PreviewObj);
        }
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Неперехваченная ошибка переключения страницы в DocumentEditor");
      }
    }
    */

    /// <summary>
    /// Подавление проверки несохраненных изменений
    /// </summary>
    private bool _NoCheckUnsavedChanges;

    private void Form_FormClosing(object sender, FormClosingEventArgs args)
    {
      //DebugTools.DebugObject("Cancel=" + Args.Cancel.ToString() + ", NoCheckUnsavedChanges=" + NoCheckUnsavedChanges.ToString() + ", Changed=" + FChangeInfo.Changed.ToString(), "FormClosing");

      if (args.Cancel)
        return;
      if (_Form.DialogResult != DialogResult.OK)
      {
        if (!_NoCheckUnsavedChanges)
        {
          // Подтверждение закрытия формы без сохранения
          if (State == EFPDataGridViewState.Edit || State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
          {
            if (ChangeInfo.Changed)
            {
              EFPApp.Activate(_Form); // 07.06.2021
              StringBuilder sb = new StringBuilder();
              sb.Append("Данные в редакторе \"");
              sb.Append(_Form.Text);
              sb.Append("\" были изменены. Вы действительно хотите выйти и потерять изменения?");
              if (EFPApp.MessageBox(sb.ToString(),
                  "Выход без сохранения изменений", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                args.Cancel = true;
            }
          }
        }
        return;
      }

      if (_IsInsideWriting)
        EFPApp.ShowTempMessage("Предыдущая запись документа еще не закончена");

      _IsInsideWriting = true;
      try
      {
        DoFormClosingOk(args);
      }
      finally
      {
        _IsInsideWriting = false;
      }
    }

    private void DoFormClosingOk(CancelEventArgs args)
    {
      _Form.DialogResult = DialogResult.None;
      // Иначе, если при записи произойдет ошибка, нельзя будет закрыть
      // форму крестиком (можно только кнопкой "отмена")

      if (State == EFPDataGridViewState.Edit || State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
      {
        if (!ValidateData())
        {
          args.Cancel = true;
          return;
        }
      }
      //DebugTools.DebugDataSet(FDocuments.DebugDataSet, "перед записью");

      if (!DoWrite(false))
        args.Cancel = true;

      // Восстанавливаем значение, иначе форма не закроется
      if (!args.Cancel)
        _Form.DialogResult = DialogResult.OK;
    }

    /// <summary>
    /// Возвращает true во время работы метода ValidateData
    /// </summary>
    public bool InsideValidateData { get { return _InsideValidateData; } }
    private bool _InsideValidateData;

    /// <summary>
    /// Проверка корректности введенных данных и копирование их из полей ввода
    /// редактора в редактируемый документ (Documents)
    /// Выполняемые действия:
    /// 1. Событие DocumentEditor.BeforeWrite
    /// 2. Проверка корректности значений полей EFPFormProvider.ValidateForm()
    /// 3. Запись полей в документ IDocEditItem.WriteValues()
    /// 4. Событие ClientDocType.Writing
    /// На шаге 1, 2 и 4 могут быть обнаружены ошибки. В этом случае дальнейшие
    /// действия не выполняются и возвращается false
    /// Обработчики событий DocumentEditor.BeforeWrite и ClientDocType.Writing не
    /// должны вызывать метод ValidateData()
    /// Если редактор документа находится в режиме просмотра или удаления
    /// (DocumentEditor.DataReadOnly=true), то никакие действия не выполняются и
    /// возвращается true
    /// </summary>
    /// <returns>true, если форма содержит корректные значения и обработчики не
    /// установили свойство Cancel</returns>
    public bool ValidateData()
    {
      return ValidateData2(true);
    }

    /// <summary>
    /// Копирование данных из полей ввода в редактируемый документ (Documents)
    /// без проверки корректности данных
    /// Выполняет те же действия, что и ValudateData():
    /// 1. Событие DocumentEditor.BeforeWrite
    /// 2. Запись полей в документ IDocEditItem.WriteValues()
    /// 3. Событие ClientDocType.Writing
    /// Проверка значений полей не выполняется, ошибки, возникающие на шаге 1 и 3
    /// игнорируются
    /// </summary>
    public void NoValidateData()
    {
      ValidateData2(false);
    }

    private bool ValidateData2(bool validate)
    {
      if (_InsideValidateData)
        throw new BugException("Рекурсивный вызов DocumentEditor.ValidateData()");

      bool res;
      _InsideValidateData = true;
      try
      {
        res = ValidateData3(validate);
      }
      finally
      {
        _InsideValidateData = false;
      }
      return res;
    }

    private bool ValidateData3(bool validate)
    {
      if (IsReadOnly)
        return true;
      // Посылаем сообщение
      if (BeforeWrite != null)
      {
        DocEditCancelEventArgs args = new DocEditCancelEventArgs(this);
        BeforeWrite(this, args);
        if (validate)
        {
          if (args.Cancel)
            return false;
        }
      }

      if (validate)
      {
        bool res;
        // 12.08.2007
        // На время проверки надо устанавливать признак OK даже если нажимается
        // кнопка "Запись", иначе проверка не будет выполнена
        DialogResult oldDR = Form.DialogResult;
        try
        {
          Form.DialogResult = DialogResult.OK;
          res = Form.FormProvider.ValidateForm();
        }
        finally
        {
          Form.DialogResult = oldDR;
        }
        if (!res)
          return false;
      }

      // Записываем редактируемые значения в dataset
      DocEditItems.WriteValues();

      // Пользовательская коррекция данных перед записью
      if (!UI.DocTypes[DocTypeName].DoWriting(this))
      {
        if (validate)
          return false;
      }

      return true;
    }

    /// <summary>
    /// Нажата кнопка "Запись"
    /// </summary>
    private void ApplyClick(object sender, EventArgs args)
    {
      try
      {
        if (_IsInsideWriting)
          EFPApp.ShowTempMessage("Вложенный вызов нажатия кнопки \"Применить\"");
        else
        {
          _IsInsideWriting = true;
          try
          {
            ApplyClick2();
          }
          finally
          {
            _IsInsideWriting = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка нажатия кнопки \"Применить\"");
      }
    }

    private void ApplyClick2()
    {
      //DBxDocState DocState1 = Documents[0][0].DocState;

      if (!ValidateData())
        return;
      DoWrite(true);

      //DBxDocState DocState2 = Documents[0][0].DocState;

      // Передаем изменение, возможно внесенные сервером, в управляющие элементы
      foreach (IDocEditItem item in DocEditItems)
        item.BeforeReadValues();
      foreach (IDocEditItem item in DocEditItems)
        item.ReadValues();
      foreach (IDocEditItem item in DocEditItems)
        item.AfterReadValues();

      Form.CancelButtonProvider.Text = "Закрыть";
      ChangeInfo.ResetChanges();

      // Изменяем подсказки для кнопок
      switch (State)
      {
        case EFPDataGridViewState.Edit:
          //FLastForm.OKButtonProvider.ToolTipText = "Закончить редактирование, сохранив внесенные изменения";
          _Form.CancelButtonProvider.ToolTipText = "Закончить редактирование без сохранения изменений, внесенных после нажатия кнопки \"Запись\"";
          //FLastForm.ApplyButtonProvider.ToolTipText = "Сохранить внесенные изменения и продолжить редактирование";
          break;
        case EFPDataGridViewState.Insert:
        case EFPDataGridViewState.InsertCopy:
          _Form.OKButtonProvider.ToolTipText = "Закончить редактирование созданной записи, сохранив изменения, внесенных после нажатия кнопки \"Запись\"";
          _Form.CancelButtonProvider.ToolTipText = "Закончить редактирование созданной записи, без сохранения изменений, внесенных после нажатия кнопки \"Запись\"";
          _Form.ApplyButtonProvider.ToolTipText = "Сохранить внесенные изменения и продолжить редактирование созданной записи";
          break;
      }

      InitFormTitle();
    }

    /// <summary>
    /// Выполняем запись значений из управляющих элементов в документ,
    /// проверяем корректность данных и выполняем запись документа
    /// Возвращаем true, если значения полей корректные и запись успешно
    /// выполнена
    /// ApplyClicked=true при нажатии кнопки "Apply" и false при нажатии "ОК"
    /// </summary>
    /// <returns></returns>
    private bool DoWrite(bool applyClicked)
    {
      if (_InsideDoWrite)
      {
        EFPApp.ShowTempMessage("Предыдущая запись еще не закончена");
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
      if (State == EFPDataGridViewState.View)
        return true;

      if (String.IsNullOrEmpty(_Documents.ActionInfo))
        InitActionInfo();

      if (State == EFPDataGridViewState.Delete)
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
          EFPApp.ShowException(e, "Редактор документа. Ошибка удаления данных на сервере");
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
        if ((_State == EFPDataGridViewState.Insert ||
           _State == EFPDataGridViewState.InsertCopy) && // добавлено 05.07.2016
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
              StringBuilder sb = new StringBuilder();
              sb.Append("Не удалось установить блокировку для созданных документов. Документ заблокирован");
              if (e.OldLock.UserId != 0)
              {
                sb.Append(" пользователем ");
                sb.Append(UI.GetUserName(e.OldLock.UserId));
              }
              EFPApp.MessageBox(sb.ToString(),
                "Ошибка установки блокировки",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception e)
            {
              EFPApp.ShowException(e, "Ошибка блокировки созданных документов");
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

        EFPApp.ShowException(e, "Ошибка записи данных на сервере");

        return false;
      }
      // ???      FResultData=(DataSet)GetData(mode2);
      _DataChanged = true;

      if (_Form != null)
      {
        SubDocsChangeInfo.Changed = false;
        ExternalChangeInfo.Changed = false;
      }

      // Уведомление о выполнении записи
      if (AfterWrite != null)
      {
        DocEditEventArgs Args = new DocEditEventArgs(this);
        AfterWrite(this, Args);
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
          DBxMemoryDocValues orgVals1 = null;
          if (_OrgVals != null)
            orgVals1 = _OrgVals[i];
          thisType.Columns.PerformPost(Documents[i].Values, orgVals1);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка сохранения копий значений документа \"" + thisType.DocType.SingularTitle + "\" для будущего использования");
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


    private void Form_FormClosed(object sender, FormClosedEventArgs args)
    {
      if (Executed != null)
        Executed(this, null);

      ClearEditLock();

      //if (!Modal)
      //  DisposeWhenClosed();
    }

    /// <summary>
    /// Получить иконку для данного режима редактирования
    /// </summary>
    /// <param name="state">Режим:Редактирование,вставка,копирование,удаление или просмотр</param>
    /// <returns>Иконка для формы</returns>
    public static Icon GetEditorStateIcon(EFPDataGridViewState state)
    {
      string imageKey;
      switch (state)
      {
        case EFPDataGridViewState.Edit: imageKey = "Edit"; break;
        case EFPDataGridViewState.Insert: imageKey = "Insert"; break;
        case EFPDataGridViewState.InsertCopy: imageKey = "InsertCopy"; break;
        case EFPDataGridViewState.Delete: imageKey = "Delete"; break;
        default: imageKey = "View"; break;
      }
      return EFPApp.MainImages.Icons[imageKey];
    }


    /// <summary>
    /// Закрыть окно редактора.
    /// На момент вызова окно редактора должно быть открыто
    /// Возвращает true, если форма успешно закрыта.
    /// Возврает false, если окно закрыть не удалось (например, не выполнены условия корректности введенных данных)
    /// </summary>
    /// <param name="isOk">true - выполнить запись ддокумента (симуляция нажатия кнопки "ОК"),
    /// false - выйти без записи. Проверка наличия несохраненных изменений не выполняется</param>
    /// <returns>Было ли закрыто окно редактора</returns>
    public bool CloseForm(bool isOk)
    {
      bool res;
      _NoCheckUnsavedChanges = true;
      try
      {
        res = _Form.FormProvider.CloseForm(isOk ? DialogResult.OK : DialogResult.Cancel);
      }
      finally
      {
        _NoCheckUnsavedChanges = false;
      }
      return res;
    }

    #endregion

    #region Кнопка "Еще"

    /// <summary>
    /// Сюда можно добавить команды меню, которые будут доступны при нажатии кнопки "Еще"
    /// </summary>
    public EFPCommandItems MoreCommandItems { get { return Form.MoreButtonProvider.CommandItems; } }

    #region Добавление команд

    /// <summary>
    /// Добавление отладочных команд к меню "Еще"
    /// </summary>
    private void InitMoreCommands()
    {
      Form.MoreButtonProvider.CommandItems.AddSeparator();

      if ((!MultiDocMode) && (!Documents.VersionView /* 09.09.2019 */))
      {
        ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
        ciShowDocInfo.MenuText = "Информация о документе";
        //ciDebugChanges.ToolTipText = "Информация о документе";
        //ciShowDocInfo.ShortCut = Keys.F12;
        ciShowDocInfo.ImageKey = "Information";
        ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
        ciShowDocInfo.GroupBegin = true;
        if (State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
          ciShowDocInfo.Enabled = false;
        else
          InitShowDocInfoEnabled(); // 11.04.2016
        Form.MoreButtonProvider.CommandItems.Add(ciShowDocInfo);

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
        ciCopyDocRef.MenuText = "Копировать ссылки на документы";
      else
        ciCopyDocRef.MenuText = "Копировать ссылку на документ";
      ciCopyDocRef.ImageKey = "Copy";
      ciCopyDocRef.Click += new EventHandler(ciCopyDocRef_Click);
      if (State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
        ciCopyDocRef.Enabled = false;
      Form.MoreButtonProvider.CommandItems.Add(ciCopyDocRef);

      ciSendDocSel = new EFPCommandItem("File", "SendToDocSel");
      if (MultiDocMode || Documents.Count > 1)
        ciSendDocSel.MenuText = "Открыть выборку документов";
      else
        ciSendDocSel.MenuText = "Открыть выборку с документом";
      ciSendDocSel.ImageKey = "DBxDocSelection";
      ciSendDocSel.Click += new EventHandler(ciSendDocSel_Click);
      if (State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
        ciSendDocSel.Enabled = false;
      Form.MoreButtonProvider.CommandItems.Add(ciSendDocSel);

      if (UI.DebugShowIds)
      {
        EFPCommandItem ciDebugChanges = new EFPCommandItem("Debug", "Changes");
        ciDebugChanges.MenuText = "Отладка изменений";
        ciDebugChanges.Click += new EventHandler(ciDebugChanges_Click);
        ciDebugChanges.GroupBegin = true;
        Form.MoreButtonProvider.CommandItems.Add(ciDebugChanges);

        EFPCommandItem ciDebugCheckItems = new EFPCommandItem("Debug", "Form");
        ciDebugCheckItems.MenuText = "Отладка формы";
        ciDebugCheckItems.Click += new EventHandler(ciDebugCheckItems_Click);
        ciDebugCheckItems.GroupEnd = true;
        Form.MoreButtonProvider.CommandItems.Add(ciDebugCheckItems);
      }

      Form.MoreButtonProvider.CommandItems.AddSeparator();

      if (State == EFPDataGridViewState.View && ((!MultiDocMode) || UI.DocTypes[DocTypeName].CanMultiEdit) &&
        (!Documents.VersionView /* 09.09.2019 */))
      {
        EFPCommandItem ciOpenEdit = new EFPCommandItem("Edit", "ReopenForEdit");
        ciOpenEdit.MenuText = "Переоткрыть для редактирования";
        ciOpenEdit.ImageKey = "Edit";
        ciOpenEdit.Click += ciOpenEdit_Click;
        ciOpenEdit.Enabled = UI.DocProvider.DBPermissions.TableModes[DocTypeName] == DBxAccessMode.Full;
        Form.MoreButtonProvider.CommandItems.Add(ciOpenEdit);
      }
      if (State == EFPDataGridViewState.InsertCopy)
      {
        EFPCommandItem ciViewSrcDoc = new EFPCommandItem("Edit", "ViewSourceDoc");
        ciViewSrcDoc.MenuText = "Просмотр исходного документа";
        ciViewSrcDoc.ImageKey = "View";
        ciViewSrcDoc.Click += ciViewSrcDoc_Click;
        Form.MoreButtonProvider.CommandItems.Add(ciViewSrcDoc);
      }

      if ((State == EFPDataGridViewState.Edit || State == EFPDataGridViewState.View) &&
        (!Documents.VersionView /* 09.09.2019 */))
      {
        EFPCommandItem ciDeleteThis = new EFPCommandItem("Edit", "DeleteDoc");
        if (MultiDocMode)
          ciDeleteThis.MenuText = "Удалить эти документы";
        else
          ciDeleteThis.MenuText = "Удалить этот документ";
        ciDeleteThis.ImageKey = "Delete";
        ciDeleteThis.Click += ciDeleteThis_Click;
        ciDeleteThis.Enabled = UI.DocProvider.DBPermissions.TableModes[DocTypeName] == DBxAccessMode.Full;
        Form.MoreButtonProvider.CommandItems.Add(ciDeleteThis);
      }

      Form.MoreButtonProvider.CommandItems.AddSeparator();
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
        dlg.Title = "Информация о документе";
        dlg.ImageKey = "Information";
        dlg.GroupTitle = "Документ";
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
        EFPApp.ShowTempMessage("Документ \"" + Documents[docTypeIndex].DocType.SingularTitle + "\" не был записан");
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

      DocumentEditor de2 = new DocumentEditor(UI, DocTypeName, EFPDataGridViewState.Edit, Documents[0].DocIds);
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

      DocumentEditor de2 = new DocumentEditor(UI, DocTypeName, EFPDataGridViewState.Delete, Documents[0].DocIds);
      de2.Modal = Modal;
      EFPApp.IdleHandlers.AddSingleAction(de2.RunWhenIdle); // 23.06.2021
    }

    #endregion

    #region Отладка изменений

    private void ciDebugChanges_Click(object sender, EventArgs args)
    {
      DebugTools.DebugChangeInfo(ChangeInfo, "Изменения значений");
    }

    private void ciDebugCheckItems_Click(object sender, EventArgs args)
    {
      DebugTools.DebugBaseProvider(Form.FormProvider, "Form provider");
    }

    #endregion

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Если true, то выполняется просмотр или удаление документа, поля
    /// документа не могут редактироваться
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        return _State == EFPDataGridViewState.View ||
          _State == EFPDataGridViewState.Delete;
      }
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
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
    /// Никаких действий над найденным редактором не выполняется
    /// </summary>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Найденный редактор или null, если редактор еще не открыт.</returns>
    public static DocumentEditor FindEditor(DBUI ui, string docTypeName, Int32[] docIds)
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
    /// Никаких действий над найденным редактором не выполняется
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

      Form[] forms = EFPApp.GetDialogStack();
      DocumentEditor de = DoFindEditor(forms, docSel);
      if (de != null)
        return de;
      if (EFPApp.Interface != null)
      {
        forms = EFPApp.Interface.GetChildForms(false);
        de = DoFindEditor(forms, docSel);
        if (de != null)
          return de;
      }
      return null;
    }

    private static DocumentEditor DoFindEditor(Form[] forms, DBxDocSelection docSel)
    {
      for (int i = 0; i < forms.Length; i++)
      {
        DocEditForm frm = forms[i] as DocEditForm;
        if (frm == null)
          continue;
        if (frm.Editor == null)
          continue; // редактор поддокумента

        DBxDocSelection ThisDocSel = frm.Editor.Documents.DocSelection;
        if (ThisDocSel.DBIdentity != docSel.DBIdentity)
          continue; // другой набор данных
        if (ThisDocSel.ContainsAny(docSel))
          return frm.Editor;
      }
      return null;
    }

    #endregion
  }
}
