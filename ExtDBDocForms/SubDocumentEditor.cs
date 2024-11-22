// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using FreeLibSet.Forms;
using System.Collections.Generic;
using System.ComponentModel;

using FreeLibSet.DependedValues;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  #region Интерфейс ISubDocumentEditorCaller

  /// <summary>
  /// Интерфейс для инициализации значений нового поддокумента (метод <see cref="ISubDocumentEditorCaller.InitNewSubDocValues"/>)
  /// и проверки нового или отредактированного поддокумента перед записью (метод <see cref="ISubDocumentEditorCaller.ValidateSubDocValues"/>).
  /// Интерфейс реализуется фильтрами табличного просмотра или пользовательским
  /// кодом при наличии специальных требований к редактируемым поддокументам.
  /// Методы интерфейса вызываются редактором поддокумента <see cref="SubDocumentEditor"/>.
  /// </summary>
  public interface ISubDocumentEditorCaller
  {
    /// <summary>
    /// Инициализировать значения нового поддокумента.
    /// На момент вызова документ находится в режиме <see cref="UIDataState.Insert"/>
    /// Реализация интерфейса может устанавливать значения <paramref name="newSubDoc"/>.Values или добавлять поддокументы.
    /// </summary>
    /// <param name="newSubDoc">Создаваемый поддокумент</param>
    void InitNewSubDocValues(DBxSubDoc newSubDoc);

    /// <summary>
    /// Проверить поддокумент перед записью на соответствие внешним требованием
    /// (например, условиям фильтра).
    /// Реализация интерфейса не должна изменять значения полей в <paramref name="savingSubDoc"/>.
    /// </summary>
    /// <param name="savingSubDoc">Записываемый поддокумент</param>
    /// <param name="errorMessages">Сюда можно добавить предупреждения о
    /// несоответствии фильтра с помощью <paramref name="errorMessages"/>.AddWarning(). Может быть
    /// добавлено несколько предупреждений</param>
    void ValidateSubDocValues(DBxSubDoc savingSubDoc, ErrorMessageList errorMessages);
  }

  #endregion

  /// <summary>
  /// Редактор строки в таблице поддокументов в редакторе документа.
  /// Одновременно могут редактироваться несколько поддокументов, в том числе и относящиеся к разным документам.
  /// </summary>
  public class SubDocumentEditor : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает редактор поддокумента
    /// </summary>
    /// <param name="mainEditor">Редактор документа</param>
    /// <param name="subDocs">Список поддокументов</param>
    /// <param name="state">Режим просмотра/создания/редактирования</param>
    public SubDocumentEditor(DocumentEditor mainEditor, DBxMultiSubDocs subDocs, UIDataState state)
    {
      if (mainEditor == null)
        throw new ArgumentNullException("mainEditor");
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");

      _MainEditor = mainEditor;
      _SubDocs = subDocs;
      _SubDocTypeUI = mainEditor.UI.DocTypes[subDocs.Owner.DocType.Name].SubDocTypes[subDocs.SubDocType.Name];
      _State = state;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Основной редактор документа
    /// </summary>
    public DocumentEditor MainEditor { get { return _MainEditor; } }
    private readonly DocumentEditor _MainEditor;

    /// <summary>
    /// Описание вида поддокумента
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private readonly SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// Редактируемые поддокументы
    /// Обычно является подмножеством поддокументов, выбранных пользователем, а не основным набором 
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private readonly DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _SubDocs.DocSet.DocProvider; } }

    /// <summary>
    /// Интерфейс для доступа к документам
    /// </summary>
    public DBUI UI { get { return _SubDocTypeUI.UI; } }

    /// <summary>
    /// Текущие режим редактирования поддокумента.
    /// Он может не совпадать с режимом основного редактора.
    /// </summary>
    public UIDataState State { get { return _State; } }
    private UIDataState _State;

    /// <summary>
    /// True, если редактируется, просматривается или удаляется сразу
    /// несколько поддокументов. 
    /// Это свойство не совпадает с <see cref="DocumentEditor.MultiDocMode"/>.
    /// </summary>
    public bool MultiDocMode
    {
      get
      {
        return SubDocs.SubDocCount > 1;
      }
    }

    /// <summary>
    /// Список переходников для полей
    /// </summary>
    internal UIExtEditItemList EditItems { get { return _Dialog.EditItems; } }

    /// <summary>
    /// Отслеживание изменений для рисования звездочки в заголовке формы.
    /// Объект существует только в процессе работы редактора.
    /// </summary>
    public DepChangeInfoList ChangeInfo { get { return _Dialog.ChangeInfoList; } }

    /// <summary>
    /// Контекст справки
    /// </summary>
    public string HelpContext
    {
      get { return _Dialog.HelpContext; }
      set { _Dialog.HelpContext = value; }
    }

    private DBxArrayExtValues _OrgVals;

    /// <summary>
    /// Внешний инициализатор полей для нового поддокумента.
    /// Вызывается после обработки значений в <see cref="ColumnUIList"/>.
    /// </summary>
    public ISubDocumentEditorCaller Caller { get { return _Caller; } set { _Caller = value; } }
    private ISubDocumentEditorCaller _Caller;


    /// <summary>
    /// Если свойство не установлено, то в режиме создания или создания копии
    /// поддокумента выполняется инициализация начальных значений полей.
    /// Свойство устанавливается в true в режиме вставки одной строки из буфера обмена.
    /// </summary>
    public bool SuppressInsertColumnValues { get { return _SuppressInsertColumnValues; } set { _SuppressInsertColumnValues = value; } }
    private bool _SuppressInsertColumnValues;

    #endregion

    #region События

    /// <summary>
    /// Вызывается при завершении работы редактора
    /// </summary>
    public event EventHandler Executed;

    /// <summary>
    /// Вызывается после того, как были установлены значения всех полей перед началом
    /// редактирования. На момент вызова форма еще не выведена на экран. 
    /// </summary>
    public event SubDocEditEventHandler AfterReadValues;

    /// <summary>
    /// Вызывается при нажатии кнопок "ОК" перед тем, как данные записаны в строку поддокумента.
    /// Вызывается в режимах <see cref="State"/>=<see cref="UIDataState.Edit"/>, <see cref="UIDataState.Insert"/> и <see cref="UIDataState.InsertCopy"/>
    /// Установка <see cref="SubDocEditCancelEventArgs.Cancel"/>=true предотвращает запись данных и закрытие редактора.
    /// Программа должна вывести сообщение пользователю о причинах отмены.
    /// На момент вызова данные формы еще не перенесены в строку.
    /// </summary>
    public event SubDocEditCancelEventHandler BeforeWrite;

    /// <summary>
    /// Вызывается после того, как данные записаны в строку поддокумента
    /// </summary>
    public event SubDocEditEventHandler AfterWrite;

    #endregion

    #region Методы

    /// <summary>
    /// Открывает окно редактора в модальном режиме, если обработчик события <see cref="FreeLibSet.Forms.Docs.SubDocTypeUI.BeforeEdit"/> 
    /// не снял флаг <see cref="BeforeSubDocEditEventArgs.ShowEditor"/> и не выставил признак <see cref="BeforeSubDocEditEventArgs.Cancel"/>.
    /// </summary>
    /// <returns>True, если редактирование выполнено</returns>
    public bool Run()
    {
      // Используем сохраненные значения по умолчанию
      if ((State == UIDataState.Insert || State == UIDataState.InsertCopy) && (!SuppressInsertColumnValues))
      {
        try
        {
          SubDocTypeUI.Columns.PerformInsert(SubDocs.Values, State == UIDataState.InsertCopy);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка иницилизации новых значений подокумента \"" + SubDocTypeUI.SubDocType.SingularTitle + "\"");
        }

        if (Caller != null)
        {
          for (int i = 0; i < SubDocs.SubDocCount; i++)
            Caller.InitNewSubDocValues(SubDocs[i]);
        }
      }

      bool cancel;
      bool showEditor;
      _SubDocTypeUI.DoBeforeEdit(this, out cancel, out showEditor);
      if (cancel)
        return false;

      if (showEditor)
      {
        _Dialog = new DataEditDialog();
          _Dialog.DataState = this.State;
          _Dialog.ConfigSectionName = "Ed_" + _SubDocTypeUI.SubDocType.Name; // 09.06.2021
          // Переключаем синхронизацию на основную форму редактирования
          _Dialog.SyncProvider = MainEditor.Dialog.SyncProvider;

          SubDocTypeUI.PerformInitEditForm(this);

          // Только после инициализации диалога можно запомнить исходные значения
          _OrgVals = new DBxArrayExtValues(SubDocs.Values);

          // Инициализируем значения
          EditItems.ReadValues();

          // Не уверен, что хочу звездочку
          // Тогда нужно будет добавить предупреждение при нажатии "Отмена"
          // FForm.FormProvider.ChangeInfo = FChangeInfo; // звездочка в заголовке формы

          if (AfterReadValues != null)
          {
            SubDocEditEventArgs args = new SubDocEditEventArgs(this);
            AfterReadValues(this, args);
          }

          InitFormTitle();

          // Кнопка "Еще"
          InitMoreClientItems();

          switch (State)
          {
            case UIDataState.Edit:
              _Dialog.OKButtonToolTipText = "Закончить редактирование, сохранив внесенные изменения." + Environment.NewLine +
                "Для реальной записи изменений нажимите кнопку \"ОК\" или \"Запись\" в основном редакторе документа";
              _Dialog.CancelButtonToolTipText = "Закончить редактирование без сохранения внесенных изменений";
              break;
            case UIDataState.Insert:
            case UIDataState.InsertCopy:
              _Dialog.OKButtonToolTipText = "Создать новую запись и закончить редактирование." + Environment.NewLine +
                "Для сохранения записи нажимите кнопку \"ОК\" или \"Запись\" в основном редакторе документа";
              _Dialog.CancelButtonToolTipText = "Закончить редактирование без сохранения введенных значений";
              break;
            case UIDataState.Delete:
              _Dialog.OKButtonToolTipText = "Удалить просматриваемую запись";
              _Dialog.CancelButtonToolTipText = "Закрыть окно, не удаляя запись";
              break;
            case UIDataState.View:
              _Dialog.OKButtonToolTipText = "Закрыть окно";
              break;
          }
          _Dialog.ShowApplyButton= false;
          _Dialog.MoreButtonToolTipText = "Дополнительные команды редактора";

        _Dialog.Writing += Dialog_Writing;
          _Dialog.FormClosed += new EventHandler(Dialog_FormClosed);
        if (_Dialog.ShowDialog() != DialogResult.OK)
          return false;
        if (State == UIDataState.View)
          return false; // 11.11.2021
        //DoWrite();
      }
      else
      {
        // !ShowEditor
        if (!SubDocTypeUI.DoWriting(this))
          return false;
        DoWrite();
      }

      return true;
    }

    /// <summary>
    /// Закрыть окно редактора.
    /// На момент вызова окно редактора должно быть открыто.
    /// Возвращает true, если форма успешно закрыта.
    /// Возвращает false, если окно закрыть не удалось (например, не выполнены условия корректности введенных данных).
    /// </summary>
    /// <param name="isOk">true - выполнить запись поддокумента (симуляция нажатия кнопки "ОК"),
    /// false - выйти без записи (симуляция нажатия кнопки "Отмена")</param>
    /// <returns>Было ли закрыто окно редактора</returns>
    public bool CloseForm(bool isOk)
    {
      return _Dialog.CloseForm(isOk);
    }

    #endregion

    #region Внутренняя реализация

    internal DataEditDialog Dialog { get { return _Dialog; } }
    private DataEditDialog _Dialog;

    /// <summary>
    /// Инициализация заголовка формы
    /// </summary>
    private void InitFormTitle()
    {
      switch (State)
      {
        case UIDataState.Edit:
          if (MultiDocMode)
            _Dialog.Title = SubDocTypeUI.SubDocType.PluralTitle + " (Редактирование записей (" + SubDocs.SubDocCount.ToString() + "))";
          else
            _Dialog.Title = SubDocTypeUI.SubDocType.SingularTitle + " (Редактирование)";
          break;
        case UIDataState.Insert:
          _Dialog.Title = SubDocTypeUI.SubDocType.SingularTitle + " (Создание)";
          break;
        case UIDataState.InsertCopy:
          _Dialog.Title = SubDocTypeUI.SubDocType.SingularTitle + " (Создание копии)";
          break;
        case UIDataState.Delete:
          if (MultiDocMode)
            _Dialog.Title = SubDocTypeUI.SubDocType.PluralTitle + " (Удаление записей (" + SubDocs.SubDocCount.ToString() + "))";
          else
            _Dialog.Title = SubDocTypeUI.SubDocType.SingularTitle + " (Удаление)";
          break;
        case UIDataState.View:
          if (MultiDocMode)
            _Dialog.Title = SubDocTypeUI.SubDocType.PluralTitle + " (Просмотр записей (" + SubDocs.SubDocCount.ToString() + "))";
          else
            _Dialog.Title = SubDocTypeUI.SubDocType.SingularTitle + " (Просмотр)";
          break;
      }

      if (SubDocTypeUI.UI.DebugShowIds && State != UIDataState.Insert && (!MultiDocMode))
        _Dialog.Title += " Id=" + SubDocs.Values["Id"].AsString;
    }

    /// <summary>
    /// Проверка корректности значений в полях редактирования поддокумента и
    /// копирование их в строку таблицы поддокумента
    /// Выполняемые действия:
    /// 1. Событие SubDocumentEditor.BeforeWrite
    /// 2. Копирование полей ввода редактора в набор данных Values
    /// 3. Событие SubDocType.Writing
    /// </summary>
    /// <returns>true, если значения корректные</returns>
    public bool ValidateData()
    {
      return _Dialog.WriteData();
    }


    private void Dialog_Writing(object sender, CancelEventArgs args)
    {
      if (IsReadOnly)
        return;

      // Посылаем сообщение
      if (BeforeWrite != null)
      {
        SubDocEditCancelEventArgs args2 = new SubDocEditCancelEventArgs(this);
        BeforeWrite(this, args2);
        args.Cancel = args2.Cancel;
        if (args.Cancel)
          return;
      }

      // Записываем редактируемые значения в однострочный dataset
      EditItems.WriteValues();

      // Пользовательская коррекция данных перед записью
      if (!SubDocTypeUI.DoWriting(this))
      {
        args.Cancel = true;
        return;
      }
    }

    /// <summary>
    /// Выполняем запись полей в датасет и возвращаем true, если значения
    /// полей корректные.
    /// </summary>
    /// <returns></returns>
    private bool DoWrite()
    {
      // TODO:
      if (State == UIDataState.View)
        return true;

      if (State == UIDataState.Delete)
      {
        for (int i = 0; i < SubDocs.SubDocCount; i++)
          SubDocs[i].Delete();
        return true;
      }


#if XXX
      if (State == EFPDataGridViewState.Insert || State == EFPDataGridViewState.InsertCopy)
      {
        object[] ItemArray = FData.Table.Rows[0].ItemArray;
        // В режиме копирования строки уже существует значение у поля RefId.
        // Его надо обнулить, чтобы оно присвоилось заново
        // AsArray[FData.MainRow.Table.Columns.IndexOf("RefId")] = null;

        // 25.06.2008 Не всегда работает. Попробуем другой способ. Будем генерировать
        // случайные номера, пока не найдем свободный
        if (DocId == 0)
        {
          FSubDocRows = new DataRow[SubDocs.Owner.Count];
          for (int i = 0; i < SubDocs.Owner.Count; i++)
          {
            Int32 ThisDocId = SubDocs.Owner[i].DocId;
            ItemArray[FData.Table.Columns.IndexOf("Id")] = DataTools.GetRandomId(SubDocs.SubDocsData);
            ItemArray[FData.Table.Columns.IndexOf("DocId")] = ThisDocId;
            FSubDocRows[i]=SubDocs.SubDocsData.Rows.Add(ItemArray);
          }
        }
        else
        {
          FSubDocRows = new DataRow[1];
          ItemArray[FData.Table.Columns.IndexOf("Id")] = DataTools.GetRandomId(SubDocs.SubDocsData);
          ItemArray[FData.Table.Columns.IndexOf("DocId")] = DocId;
          FSubDocRows[0]=SubDocs.SubDocsData.Rows.Add(ItemArray);
        }
      }
      else
      {
        FSubDocRows = FOrgRows;
        for (int i = 0; i < FOrgRows.Length; i++)
          FOrgRows[i].ItemArray = FData.Table.Rows[i].ItemArray;
      }
#endif

      // Пользовательский обработчик после записи
      SubDocTypeUI.DoWrote(this);

      // Уведомление о выполнении записи
      if (AfterWrite != null)
      {
        SubDocEditEventArgs args = new SubDocEditEventArgs(this);
        AfterWrite(this, args);
      }


      // Сохраняем введенные значения полей для будущего использования
      try
      {
        SubDocTypeUI.Columns.PerformPost(SubDocs.Values, _OrgVals);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка сохранения копий значений поддокумента \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" для будущего использования");
      }

      return true;
    }

    void Dialog_FormClosed(object sender, EventArgs args)
    {
      if (Executed != null)
        Executed(this, null);
    }

    #endregion

    #region Кнопка "Еще" и добавление команд

    /// <summary>
    /// Сюда можно добавить команды меню, которые будут доступны при нажатии кнопки "Еще"
    /// </summary>
    public EFPCommandItems MoreCommandItems { get { return Dialog.MoreCommandItems; } }

    /// <summary>
    /// Нажатие кнопки "Еще" открывает локальное меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void MoreButton_Click(object sender, EventArgs args)
    {
      Button moreButton = (Button)sender;
      moreButton.ContextMenuStrip.Show(moreButton, moreButton.Width, moreButton.Height);
    }

    private void InitMoreClientItems()
    {
      if (SubDocTypeUI.UI.DebugShowIds)
      {
        EFPCommandItem ciDebugChanges = new EFPCommandItem("Edit", "Changes");
        ciDebugChanges.MenuText = "Отладка изменений";
        ciDebugChanges.Click += new EventHandler(ciDebugChanges_Click);
        ciDebugChanges.GroupBegin = true;
        ciDebugChanges.GroupEnd = true;
        MoreCommandItems.Add(ciDebugChanges);
      }

      MoreCommandItems.AddSeparator();
    }

    #region Отладка изменений

    private void ciDebugChanges_Click(object sender, EventArgs args)
    {
      FreeLibSet.Forms.Diagnostics.DebugTools.DebugChangeInfo(ChangeInfo, "Изменения значений");
    }

    #endregion

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// true, если пользователь не может изменять поля документа. 
    /// Это свойство не обязано совпадать с <see cref="MainEditor"/>.IsReadOnly.
    /// Основной редактор может находиться в режиме редактирования, а редактор документа - в режиме просмотра записей.
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        return _State == UIDataState.View || _State == UIDataState.Delete;
      }
    }

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }
}

