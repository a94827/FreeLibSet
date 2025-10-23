using FreeLibSet.Config;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Remoting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms.Data;
using FreeLibSet.UICore;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms.Docs
{
  #region Делегат для работы с просмотром

  /// <summary>
  /// Аргументы событий <see cref="DocTypeUI.InitView"/> и <see cref="SubDocTypeUI.InitView"/>.
  /// </summary>
  public class InitEFPDBxViewEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается объектами <see cref="DocTypeUI"/> и <see cref="SubDocTypeUI"/>.
    /// </summary>
    /// <param name="controlProvider">Инициализируемый просмотр</param>
    internal InitEFPDBxViewEventArgs(IEFPDBxView controlProvider)
    {
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализируемый табличный просмотр
    /// </summary>
    public IEFPDBxView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPDBxView _ControlProvider;

    /// <summary>
    /// Пользовательские данные
    /// </summary>
    public object UserInitData { get { return _UserInitData; } set { _UserInitData = value; } }
    private object _UserInitData;

    #endregion
  }

  /// <summary>
  /// Делегат событий  <see cref="DocTypeUI.InitView"/> и <see cref="SubDocTypeUI.InitView"/>.
  /// </summary>
  /// <param name="sender">Интерфейс доступа к документам или поддокументам</param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitEFPDBxViewEventHandler(object sender, InitEFPDBxViewEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="DocTypeUI.InitDocSelView"/>.
  /// </summary>
  public class InitEFPDocSelViewEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается объектами <see cref="DocTypeUI "/>
    /// </summary>
    /// <param name="controlProvider">Инициализируемый просмотр</param>
    internal InitEFPDocSelViewEventArgs(IEFPDocSelView controlProvider)
    {
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализируемый табличный просмотр выборки документов
    /// </summary>
    public IEFPDocSelView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPDocSelView _ControlProvider;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DocTypeUI.InitDocSelView"/>
  /// </summary>
  /// <param name="sender">Интерфейс доступа к документам</param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitEFPDocSelViewEventHandler(object sender, InitEFPDocSelViewEventArgs args);

  #endregion

  #region DocTypeEditingEventHandler

  /// <summary>
  /// Аргументы события <see cref="DocTypeUI.Editing"/>.
  /// </summary>
  public class DocTypeEditingEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается в DocTypeUI.
    /// </summary>
    /// <param name="docType"></param>
    /// <param name="state"></param>
    /// <param name="editIds"></param>
    /// <param name="modal"></param>
    /// <param name="caller"></param>
    internal DocTypeEditingEventArgs(DocTypeUI docType, UIDataState state, IdArray<Int32> editIds, bool modal, DocumentViewHandler caller)
    {
      _DocType = docType;
      _State = state;
      _EditIds = editIds;
      _Modal = modal;
      _Caller = caller;
      _Handled = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализатор нового документа
    /// </summary>
    public DocumentViewHandler Caller { get { return _Caller; } }
    private readonly DocumentViewHandler _Caller;

    /// <summary>
    /// Тип документа
    /// </summary>
    public DocTypeUI DocType { get { return _DocType; } }
    private readonly DocTypeUI _DocType;

    /// <summary>
    /// Режим работы
    /// </summary>
    public UIDataState State { get { return _State; } }
    private readonly UIDataState _State;

    /// <summary>
    /// Список идентификаторов редактируемых, просматриваемых или
    /// удаляемых документов
    /// </summary>
    public IdArray<Int32> EditIds { get { return _EditIds; } }
    private readonly IdArray<Int32> _EditIds;

    /// <summary>
    /// True, если окно редактирования следует показать в модальном
    /// режиме и false, если оно должно быть встроено в интерфейс MDI.
    /// Поле может быть проигнорировано, если окно всегда выводится
    /// в модальном режиме.
    /// </summary>
    public bool Modal { get { return _Modal; } }
    private readonly bool _Modal;

    /// <summary>
    /// Должен быть установлен в true, чтобы предотвратить стандартный вызов редактора документа
    /// </summary>
    public bool Handled { get { return _Handled; } set { _Handled = value; } }
    private bool _Handled;

    /// <summary>
    /// Сюда может быть помещено значение, возвращаемое функкцией 
    /// <see cref="DocTypeUI.PerformEditing(int, bool)"/>, когда обработчик события <see cref="DocTypeUI.Editing"/> устанавливает
    /// <see cref="Handled"/>=true. Если обработчик оставляет <see cref="Handled"/>=false для показа формы, то он
    /// может установить <see cref="HandledResult"/>=true. В этом случае, метод <see cref="DocTypeUI.PerformEditing(int, bool)"/>
    /// вернет true, даже если пользователь не будет редактировать документ.
    /// До установки свойства в явном виде, оно имеет значение, совпадающее со
    /// свойством <see cref="Handled"/>.
    /// </summary>
    public bool HandledResult
    {
      get { return _HandledResult ?? Handled; }
      set { _HandledResult = value; }
    }
    private bool? _HandledResult;

    // TODO: public CfgPart EditorConfigSection { get { return DocType.EditorConfigSection; } }

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает объект <see cref="DBxDocSet"/> с единственным <see cref="DBxMultiDocs"/>, загруженный данными или инициализированный
    /// начальными значениями (в режиме <see cref="UIDataState.Insert"/>).
    /// </summary>
    /// <returns></returns>
    public DBxDocSet CreateDocs()
    {
      DBxDocSet docSet = new DBxDocSet(DocType.UI.DocProvider);
      DBxMultiDocs mDocs = docSet[DocType.DocType.Name];

      switch (State)
      {
        case UIDataState.Edit:
          mDocs.Edit(EditIds);
          break;
        case UIDataState.Insert:
          mDocs.Insert();
          if (Caller != null)
            Caller.InitNewDocValues(mDocs[0]);
          break;
        case UIDataState.InsertCopy:
          if (EditIds.Count != 1)
            throw new InvalidOperationException(Res.Common_Err_SingleDocRequired);
          mDocs.InsertCopy(EditIds[0]);
          break;
        default:
          mDocs.View(EditIds);
          break;
      }

      // TODO: DocSet.CheckDocs = true;
      return docSet;
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DocTypeUI.Editing"/>.
  /// </summary>
  /// <param name="sender">Интерфейс пользователя для вида документов</param>
  /// <param name="args">Аргумент события</param>
  public delegate void DocTypeEditingEventHandler(object sender, DocTypeEditingEventArgs args);

  #endregion

  #region DocEditEventHandler

  /// <summary>
  /// Аргументы для нескольких событий класса <see cref="DocumentEditor"/>
  /// </summary>
  public class DocEditEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создание не должно выполняться в пользовательском коде
    /// </summary>
    /// <param name="editor">Редактор документов</param>
    internal DocEditEventArgs(DocumentEditor editor)
    {
      _Editor = editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор основного документа
    /// </summary>
    public DocumentEditor Editor { get { return _Editor; } }
    private readonly DocumentEditor _Editor;

    #endregion
  }

  /// <summary>
  /// Делегат для нескольких событий класса <see cref="DocumentEditor"/>.
  /// </summary>
  /// <param name="sender">Объект - источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DocEditEventHandler(object sender, DocEditEventArgs args);

  /// <summary>
  /// Аргументы событий <see cref="DocTypeUI.Writing"/> и <see cref="DocumentEditor.BeforeWrite"/>.
  /// </summary>
  public class DocEditCancelEventArgs : DocEditEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создание аргументов события не должно выполняться в пользовательском коде
    /// </summary>
    /// <param name="editor">Редактор документа</param>
    internal DocEditCancelEventArgs(DocumentEditor editor)
      : base(editor)
    {
      _Cancel = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Это свойство можно установить в true, чтобы отменить действие
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    #endregion
  }

  /// <summary>
  /// Делегат событий <see cref="DocTypeUI.Writing"/> и <see cref="DocumentEditor.BeforeWrite"/>.
  /// </summary>
  /// <param name="sender">Объект - источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DocEditCancelEventHandler(object sender, DocEditCancelEventArgs args);

  #endregion

  #region BeforeDocEditEventHandler

  /// <summary>
  /// Аргументы события <see cref="DocTypeUI.BeforeEdit"/>.
  /// </summary>
  public class BeforeDocEditEventArgs : DocEditEventArgs
  {
    #region Конструктор

    internal BeforeDocEditEventArgs(DocumentEditor editor)
      : base(editor)
    {
      _Cancel = false;
      _ShowEditor = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Установка этого поля в true приводит к отказу от работы редактора.
    /// Запись результатов выполняться не будет.
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    /// <summary>
    /// Установка этого поля в false приводит к пропуску работы редактора.
    /// Сразу выполняется запись результатов.
    /// </summary>
    public bool ShowEditor { get { return _ShowEditor; } set { _ShowEditor = value; } }
    private bool _ShowEditor;

    /// <summary>
    /// Возвращает имя столбца, активного в табличном просмотре, из которого документ открыт на редактирование.
    /// Если редактор запускается не из просмотра, или информация о текущем столбце недоступна, возвращается пустая строка.
    /// </summary>
    public string CurrentColumnName
    {
      get
      {
        if (Editor.Caller == null)
          return String.Empty;
        else
          return Editor.Caller.CurrentColumnName;
      }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DocTypeUI.BeforeEdit"/>
  /// </summary>
  /// <param name="sender">Интерфейс документов <see cref="DocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void BeforeDocEditEventHandler(object sender, BeforeDocEditEventArgs args);

  #endregion

  #region DocTypeDocSelEventHandler

  /// <summary>
  /// Аргументы события <see cref="DocTypeUIBase.GetDocSel"/>
  /// </summary>
  public class DocTypeDocSelEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Объект не предназначен для создания в пользовательском коде
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="docSel"></param>
    /// <param name="tableName"></param>
    /// <param name="ids"></param>
    /// <param name="reason"></param>
    internal DocTypeDocSelEventArgs(DBUI ui, DBxDocSelection docSel, string tableName, IdArray<Int32> ids, EFPDBxViewDocSelReason reason)
    {
      _UI = ui;
      _DocSel = docSel;
      _TableName = tableName;
      _Ids = ids;
      _Reason = reason;
    }

    /// <summary>
    /// Объект не предназначен для создания в пользовательском коде
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="docSel"></param>
    /// <param name="tableName"></param>
    /// <param name="rows"></param>
    /// <param name="reason"></param>
    internal DocTypeDocSelEventArgs(DBUI ui, DBxDocSelection docSel, string tableName, DataRow[] rows, EFPDBxViewDocSelReason reason)
    {
      _UI = ui;
      _DocSel = docSel;
      _TableName = tableName;
      _Rows = rows;
      _Ids = IdTools.AsIdArray<Int32>(IdTools.GetIds<Int32>(rows));
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной объект пользовательского интерфейса
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Имя таблицы документа или поддокумента (к которому присоединен обработчик события)
    /// </summary>
    public string TableName { get { return _TableName; } }
    private readonly string _TableName;

    /// <summary>
    /// Массив идентификаторов выбранных документов или поддокументов, для которых
    /// требуется построить выборку документов
    /// </summary>
    public IdArray<Int32> Ids { get { return _Ids; } }
    private readonly IdArray<Int32> _Ids;

    /// <summary>
    /// Массив строк выбранных поддокументов, для которых требуется построить
    /// выборку документов.
    /// Используется только для поддокументов в редакторе документа, когда некоторые
    /// поддокументы еще не записаны.
    /// </summary>
    public DataRow[] Rows { get { return _Rows; } }
    private readonly DataRow[] _Rows;

    /// <summary>
    /// Причина, по которой требуется создать выборку
    /// </summary>
    public EFPDBxViewDocSelReason Reason { get { return _Reason; } }
    private readonly EFPDBxViewDocSelReason _Reason;

    /// <summary>
    /// Сюда должны быть добавлены ссылки на документы
    /// Добавлять ссылки на сами документы (AllIds) не требуется
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private readonly DBxDocSelection _DocSel;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить ссылки из ссылочного поля
    /// </summary>
    /// <param name="refTableName">Имя типа документа, на который ссылается поле</param>
    /// <param name="refColumnName">Имя ссылочного поля. Может содержать точки</param>
    public void AddFromColumn(string refTableName, string refColumnName)
    {
      for (int i = 0; i < Ids.Count; i++)
      {
        Int32 refId = DataTools.GetInt32(GetRowValue(i, refColumnName));
        DocSel.Add(refTableName, refId);
      }
    }

    /// <summary>
    /// Возвращает значение для поля <paramref name="columnName"/> для идентификатора из массива Ids
    /// с индексом <paramref name="rowIndex"/>. Если вызов выполнен из <see cref="EFPSubDocGridView "/> и установлено свойство
    /// Rows, то значение извлекается из строки данных с указанным индексом. Иначе
    /// значение извлекается с использованием системы буферизации.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля <paramref name="columnName"/></returns>
    public object GetRowValue(int rowIndex, string columnName)
    {
      if (Rows != null)
      {
        #region Из текущей строки таблицы поддокументов

        int p = columnName.IndexOf('.');
        string baseColumnName;
        if (p < 0)
          baseColumnName = columnName;
        else
          baseColumnName = columnName.Substring(0, p);

        int p2 = Rows[rowIndex].Table.Columns.IndexOf(baseColumnName);
        if (p2 < 0)
          return null; // нет такого поля
        object BaseValue = Rows[rowIndex][p2];
        if (p < 0)
          return BaseValue;

        // Ссылочное поле с точкой
        Int32 refId = DataTools.GetInt32(BaseValue);
        if (refId < 0)
          return null; // тоже фиктивный идентификатор
        return _UI.TextHandlers.DBCache[TableName].GetRefValue(columnName, refId);

        #endregion
      }
      else
      {
        #region По идентификатору с использованием BufTables

        if (Ids[rowIndex] >= 0)
          return _UI.TextHandlers.DBCache[TableName].GetValue(Ids[rowIndex], columnName);
        else
          return null; // фиктивный идентификатор

        #endregion
      }
    }

#if XXX
    /// <summary>
    /// Загрузить из полей переменной ссылки "PrefixТаблица" и "PrefixИдентификатор"
    /// </summary>
    /// <param name="prefix"></param>
    public void AddFromVTReference(string prefix)
    {
      for (int i = 0; i < Ids.Length; i++)
      {
        Int32 TableId = DataTools.GetInt32(GetRowValue(i, prefix + "Таблица"));
        Int32 DocId = DataTools.GetInt32(GetRowValue(i, prefix + "Идентификатор"));
        if (TableId != 0 && DocId != 0)
        {
          DBxDocType dt = _UI.DocProvider.DocTypes.FindByTableId(TableId);
          if (dt != null)
            DocSel.Add(dt.Name, DocId);
        }
      }
    }
#endif

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DocTypeUIBase.GetDocSel"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="DocTypeUI"/> или <see cref="SubDocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void DocTypeDocSelEventHandler(object sender, DocTypeDocSelEventArgs args);

  #endregion


#if XXX
  // Не нужен

  public class DocumentBrowserList : IEnumerable<IDocumentBrowser>
  {
  #region Конструктор

    internal DocumentBrowserList()
    {
      FItems = new List<IDocumentBrowser>();
    }

  #endregion

  #region Доступ к элементам

    private List<IDocumentBrowser> FItems;

    public int Count { get { return FItems.Count; } }

    public IDocumentBrowser this[int Index] { get { return FItems[Index]; } }

    internal void Add()

  #endregion

  #region IEnumerable<IDocumentBrowser> Members

    public IEnumerator<IDocumentBrowser> GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

  #endregion
  }

#endif

  /// <summary>
  /// Базовый класс для <see cref="DocTypeUI"/> и <see cref="SubDocTypeUI"/>
  /// </summary>
  public abstract class DocTypeUIBase
  {
    #region Защищенный конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeBase">Описание вида документа или поддокумента</param>
    protected DocTypeUIBase(DBUI ui, DBxDocTypeBase docTypeBase)
    {
#if DEBUG
      if (ui == null)
        throw new ArgumentNullException("ui");
      if (docTypeBase == null)
        throw new ArgumentNullException("docTypeBase");
#endif

      _UI = ui;
      //_GridProducer.SetCache(ui.DocProvider.DBCache, docTypeBase.Name); // 12.07.2016
      _Columns = new ColumnUIList(this);
      _DocTypeBase = docTypeBase;

      // 13.06.2019
      // Для столбцов "ParentId" нужно установить режим для новых документов
      if (!String.IsNullOrEmpty(docTypeBase.TreeParentColumnName))
        _Columns[docTypeBase.TreeParentColumnName].NewMode = ColumnNewMode.Saved;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Описание вида документа или поддокумента
    /// </summary>
    public DBxDocTypeBase DocTypeBase { get { return _DocTypeBase; } }
    private readonly DBxDocTypeBase _DocTypeBase;

    /// <summary>
    /// Разрешение пользователя на доступ к таблице документа или поддокумента.
    /// </summary>
    public DBxAccessMode TableMode { get { return _UI.DocProvider.DBPermissions.TableModes[_DocTypeBase.Name]; } }

    /// <summary>
    /// Возвращает <see cref="DBxDocTypeBase.Name"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DocTypeBase.Name;
    }

    #endregion

    #region Свойства и методы буферизации данных

    /// <summary>
    /// Возвращает текстовое представление для документа/поддокумента
    /// </summary>
    /// <param name="id">Идентификатор документа/поддокумента</param>
    /// <returns>Текст</returns>
    public string GetTextValue(Int32 id)
    {
      return UI.TextHandlers.GetTextValue(_DocTypeBase.Name, id);
    }

    /// <summary>
    /// Система кэширования для всех документов
    /// </summary>
    public DBxCache DBCache { get { return UI.TextHandlers.DBCache; } }

    /// <summary>
    /// Система кэширования для текущего вида документов/поддокументов
    /// </summary>
    public DBxTableCache TableCache { get { return DBCache[_DocTypeBase.Name]; } }

    /// <summary>
    /// Получение нескольких значений поля для документа/поддокумента. 
    /// Буферизация используется, если она разрешена, иначе выполняется обращение к серверу.
    /// Поля могут быть ссылочными, то есть содержать точки.
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="columnNames">Массив имен полей</param>
    /// <returns>Массив объектов, содержащих значения</returns>
    public object[] GetValues(Int32 id, string[] columnNames)
    {
      return TableCache.GetValues(id, columnNames);
    }

    /// <summary>
    /// Получение нескольких значений поля для документа/поддокумента. 
    /// Буферизация используется, если она разрешена, иначе выполняется обращение к серверу.
    /// Поля могут быть ссылочными, то есть содержать точки.
    /// </summary>
    /// <param name="id">Идентификатор документа/поддокумента</param>
    /// <param name="columnNames">Имена полей, разделенные запятыми</param>
    /// <returns>Массив объектов, содержащих значения</returns>
    public object[] GetValues(Int32 id, string columnNames)
    {
      return GetValues(id, columnNames.Split(new char[] { ',' }));
    }

    /// <summary>
    /// Получение значения одного поля для документа/поддокумента. 
    /// Буферизация используется, если она разрешена, иначе выполняется обращение к серверу.
    /// Поля могут быть ссылочными, то есть содержать точки.
    /// </summary>
    /// <param name="id">Идентификатор документа/поддокумента</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public object GetValue(Int32 id, string columnName)
    {
      return TableCache.GetValue(id, columnName);
    }

    #endregion

    #region Значок документа

    /// <summary>
    /// Основной значок вида документа или поддокумента.
    /// Установка свойства предполагает, что обработчик не будет использоваться для определения значка.
    /// Используйте метод <see cref="AddImageHandler(string, DBxColumns, DBxImageValueNeededEventHandler)"/>, если требуется обработчик
    /// </summary>
    public string ImageKey
    {
      get
      {
        return _UI.ImageHandlers.GetImageKey(_DocTypeBase.Name);
      }
      set
      {
        _UI.ImageHandlers.Add(_DocTypeBase.Name, value);
      }
    }

    /// <summary>
    /// Задать обработчик получения изображения для документа или поддокумента
    /// </summary>
    /// <param name="imageKey">Имя основного изображения в списке <see cref="EFPApp.MainImages"/></param>
    /// <param name="columnNames">Список столбцов (через запятую), которые использует обработчик</param>
    /// <param name="imageValueNeeded">Обработчик, который позволяет получить изображение, раскраску и всплывающую подсказку 
    /// для конкретного документа и поддокумента.
    /// Обработчик должен выполняться быстро, так как вызывается при прорисовке кажой строки табличного просмотра</param>
    public void AddImageHandler(string imageKey, string columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
    {
      _UI.ImageHandlers.Add(_DocTypeBase.Name, imageKey, columnNames, imageValueNeeded);
    }

    /// <summary>
    /// Задать обработчник получения изображения для документа или поддокумента
    /// </summary>
    /// <param name="imageKey">Имя основного изображения в списке <see cref="EFPApp.MainImages"/></param>
    /// <param name="columnNames">Список столбцов, которые использует обработчик</param>
    /// <param name="imageValueNeeded">Обработчик, который позволяет получить изображение, раскраску и всплывающую подсказку 
    /// для конкретного документа и поддокумента.
    /// Обработчик должен выполняться быстро, так как вызывается при прорисовке кажой строки табличного просмотра</param>
    public void AddImageHandler(string imageKey, DBxColumns columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
    {
      _UI.ImageHandlers.Add(_DocTypeBase.Name, imageKey, columnNames, imageValueNeeded);
    }

    /// <summary>
    /// Значок для одного документа.
    /// Возвращает имя изображения из <see cref="EFPApp.MainImages"/>.
    /// Если значок не был задан в <see cref="DBxDocImageHandlers"/> в явном виде, возвращает "Item".
    /// </summary>
    public string SingleDocImageKey
    {
      get
      {
        return _UI.ImageHandlers.GetSingleDocImageKey(_DocTypeBase.Name);
      }
    }

    /// <summary>
    /// Значок для таблицы документов.
    /// Возвращает имя изображения из <see cref="EFPApp.MainImages"/>.
    /// Если значок не был задан в <see cref="DBxDocImageHandlers"/> в явном виде, возвращает "Table".
    /// </summary>
    public string TableImageKey
    {
      get
      {
        return _UI.ImageHandlers.GetTableImageKey(_DocTypeBase.Name);
      }
    }


    /// <summary>
    /// Получить изображение для заданного идентификатора
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <returns>Изображение в списке <see cref="EFPApp.MainImages"/></returns>
    public Image GetImageValue(Int32 id)
    {
      return EFPApp.MainImages.Images[GetImageKey(id)];
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <returns>Имя изображения в списке <see cref="EFPApp.MainImages"/></returns>
    public string GetImageKey(Int32 id)
    {
      return _UI.ImageHandlers.GetImageKey(_DocTypeBase.Name, id);
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора документа
    /// </summary>
    /// <param name="row">Строка с частью заполненных полей документа или поддокумента</param>
    /// <returns>Изображение в списке <see cref="EFPApp.MainImages"/></returns>
    public Image GetImageValue(DataRow row)
    {
      return EFPApp.MainImages.Images[GetImageKey(row)];
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора документа, извлекаемого из строки данных
    /// </summary>
    /// <param name="row">Строка с частью заполненных полей документа или поддокумента</param>
    /// <returns>Имя изображения в списке <see cref="EFPApp.MainImages"/></returns>
    public string GetImageKey(DataRow row)
    {
      return _UI.ImageHandlers.GetImageKey(_DocTypeBase.Name, row);
    }

    /// <summary>
    /// Получить раскраску строки документа или поддокумента с заданным идентификатором
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="colorType">Сюда помещается цвет строки в справочнике</param>
    /// <param name="grayed">Сюда помещается true, если запись должна быть отмечена серым цветом</param>
    public void GetRowColor(Int32 id, out UIDataViewColorType colorType, out bool grayed)
    {
      _UI.ImageHandlers.GetRowColor(_DocTypeBase.Name, id, out colorType, out grayed);
    }

    /// <summary>
    /// Получить раскраску строки документа или поддокумента с заданным идентификатором 
    /// и применить ее при обработке события <see cref="EFPDataGridView.RowInfoNeeded"/>.
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="args">Аргументы события, в которых заполняются поля</param>
    public void GetRowColor(Int32 id, EFPDataGridViewRowInfoEventArgs args)
    {
      UIDataViewColorType colorType;
      bool grayed;
      GetRowColor(id, out colorType, out grayed);
      args.ColorType = colorType;
      args.Grayed = grayed;
    }

    /// <summary>
    /// Получить раскраску строки документа или поддокумента и применить ее при обработке события <see cref="EFPDataGridView.RowInfoNeeded"/>.
    /// Идентификатор документа или поддокумента извлекается из <paramref name="args"/>.DataRow из поля "Id".
    /// </summary>
    /// <param name="args">Аргументы события, в которых заполняются поля</param>
    public void GetRowColor(EFPDataGridViewRowInfoEventArgs args)
    {
      if (args.DataRow == null)
        return;
      GetRowColor(DataTools.GetInt32(args.DataRow, "Id"), args);
    }

    /// <summary>
    /// Создает <see cref="EFPReportFilterItem"/> для добавления в табличку фильтра.
    /// </summary>
    /// <param name="displayName">Заголовок фильтра (обычно, имя поля)</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <returns>Описание строки таблички фильтра</returns>
    public EFPReportFilterItem CreateReportFilterItem(string displayName, Int32 id)
    {
      EFPReportFilterItem item = new EFPReportFilterItem(displayName);
      item.Value = GetTextValue(id);
      if (EFPApp.ShowListImages)
        item.ImageKey = GetImageKey(id);
      return item;
    }

    #endregion

    #region Всплывающие подсказки

    /// <summary>
    /// Получить всплывающую подсказку для документа или поддокумента
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <returns>Всплывающая подсказка</returns>
    public string GetToolTipText(Int32 id)
    {
      return _UI.ImageHandlers.GetToolTipText(_DocTypeBase.Name, id);
    }

    /// <summary>
    /// Получить всплывающую подсказку для документа или поддокумента
    /// </summary>
    /// <param name="row">Строка таблицы документа или поддокумента.
    /// Значения полей, нужных для создания подсказки, приоритетно извлекаются из
    /// строки или набора данных, к которому относится строка (для ссылочных полей)</param>
    /// <returns>Всплывающая подсказка</returns>
    public string GetToolTipText(DataRow row)
    {
      return _UI.ImageHandlers.GetToolTipText(_DocTypeBase.Name, row);
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Параметры интерфейса пользователя для отдельных столбцов
    /// </summary>
    public ColumnUIList Columns { get { return _Columns; } }
    private readonly ColumnUIList _Columns;

    #endregion

    #region Создание выборки документов

    /// <summary>
    /// Это событие вызывается при запросе копирования строк табличного просмотра
    /// документа в буфер обмена или выполнении команды "Отправить - выборка документов".
    /// Также используется при копировании ссылки в <see cref="EFPDocComboBox"/>.
    /// Используйте это событие вместо установки обработчика <see cref="EFPDBxGridView.GetDocSel"/>
    /// при инициализации табличного просмотра в обработчике <see cref="DocTypeUI.InitView"/>.
    /// Ссылки на непосредственно выбранные документы добавляются автоматически
    /// </summary>
    public event DocTypeDocSelEventHandler GetDocSel;

    /// <summary>
    /// Возвращает true, если обработчик события <see cref="GetDocSel"/> установлен.
    /// </summary>
    public bool HasGetDocSel { get { return GetDocSel != null; } }

    /// <summary>
    /// Создать выборку документов.
    /// В выборку попадают выбранные документы.
    /// Если задан обработчик события <see cref="GetDocSel"/>, то будут добавлены связанные
    /// документы, на которые есть ссылочные поля.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="ids">Идентификаторы выбранных документов или поддокументов</param>
    /// <param name="reason">Причина построения выборки</param>
    /// <returns>Выборка</returns>
    public abstract void PerformGetDocSel(DBxDocSelection docSel, IEnumerable<Int32> ids, EFPDBxViewDocSelReason reason);

    /// <summary>
    /// Вызывает обработчик события <see cref="GetDocSel"/>, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="ids">Идентификаторы выбранных документов или поддокументов</param>
    /// <param name="reason">Причина построения выборки</param>
    protected void OnGetDocSel(DBxDocSelection docSel, IIdSet<Int32> ids, EFPDBxViewDocSelReason reason)
    {
      if (GetDocSel != null)
      {
        // Есть обработчик события
        DocTypeDocSelEventArgs args = new DocTypeDocSelEventArgs(_UI, 
          docSel, 
          _DocTypeBase.Name,  
          IdTools.AsIdArray<Int32>(ids), 
          reason);
        try
        {
          GetDocSel(this, args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }
    }

    /// <summary>
    /// Вызывает обработчик события <see cref="GetDocSel"/>, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="rows">Строки таблицы документов или поддокументов, откуда извлекаются идентификаторы</param>
    /// <param name="reason">Причина построения выборки</param>
    protected void OnGetDocSel(DBxDocSelection docSel, DataRow[] rows, EFPDBxViewDocSelReason reason)
    {
      if (GetDocSel != null)
      {
        // Есть обработчик события
        DocTypeDocSelEventArgs args = new DocTypeDocSelEventArgs(_UI, docSel, _DocTypeBase.Name, rows, reason);
        try
        {
          GetDocSel(this, args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }
    }

    /// <summary>
    /// Получить выборку документов.
    /// Вызывает обработчик события <see cref="GetDocSel"/>, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="reason">Причина построения выборки</param>
    public void PerformGetDocSel(DBxDocSelection docSel, Int32 id, EFPDBxViewDocSelReason reason)
    {
      if (id != 0)
        PerformGetDocSel(docSel, new Int32[1] { id }, reason);
    }

    /// <summary>
    /// Получить выборку документов.
    /// Вызывает обработчик события <see cref="GetDocSel"/>, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="ids">Список идентификаторов документа или поддокумента</param>
    /// <param name="reason">Причина построения выборки</param>
    public void PerformGetDocSel(DBxDocSelection docSel, IdCollection<Int32> ids, EFPDBxViewDocSelReason reason)
    {
      if (ids != null)
        PerformGetDocSel(docSel, ids.ToArray(), reason);
    }

    #endregion
  }

  /// <summary>
  /// Обработчики стороны клиента для одного вида документов
  /// </summary>
  public class DocTypeUI : DocTypeUIBase
  {
    #region Защищенный конструктор

    internal DocTypeUI(DBUI ui, DBxDocType docType)
      : base(ui, docType)
    {
      _DocType = docType;

      CanInsertCopy = false;
      //FDataBuffering = false;

      _GridProducer = new EFPDocTypeGridProducer(this);
      CanMultiEdit = false;

      _SubDocTypes = new SubDocTypeList(this);

      _Browsers = new DocumentViewHandlerList(this);

      // 13.06.2019
      // Для столбцов "GroupId" нужно установить режим для новых документов
      // Должно быть после вызова InitGroupDocDict()
      if (!String.IsNullOrEmpty(docType.GroupRefColumnName))
        this.Columns[docType.GroupRefColumnName].NewMode = ColumnNewMode.Saved;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описание вида документов
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private /*readonly меняется при смене DocProvider */ DBxDocType _DocType;

    /// <summary>
    /// True, если допускается одновременное редактирование или
    /// просмотр нескольких выбранных документов. По умолчанию -
    /// false (нельзя).
    /// </summary>
    public bool CanMultiEdit { get { return _CanMultiEdit; } set { _CanMultiEdit = value; } }
    private bool _CanMultiEdit;

    /// <summary>
    /// true, если разрешено создание нового документа на основании существующего
    /// </summary>
    public bool CanInsertCopy { get { return _CanInsertCopy; } set { _CanInsertCopy = value; } }
    private bool _CanInsertCopy;

    /// <summary>
    /// Разрешение на вид документов, заданное с помощью <see cref="DocTypePermission"/>.
    /// Возвращаемое значение может отличаться от свойства <see cref="DocTypeUIBase.TableMode"/>.
    /// У пользователя может не быть разрешения на вид документа, но быть разрешение на таблицу.
    /// В большинстве случаев следует использовать свойство <see cref="DocTypeUIBase.TableMode"/>.
    /// Вызывает метод <see cref="DocTypePermission.GetAccessMode(UserPermissions, string)"/>.
    /// </summary>
    public DBxAccessMode DocTypePermissionMode
    {
      get
      {
        return DocTypePermission.GetAccessMode(UI.DocProvider.UserPermissions, DocType.Name);
      }
    }

    /// <summary>
    /// Генератор табличного просмотра.
    /// Обычно в прикладном коде сюда следует добавить описания столбцов.
    /// </summary>
    public EFPDocTypeGridProducer GridProducer { get { return _GridProducer; } }
    private readonly EFPDocTypeGridProducer _GridProducer;

    #endregion

    #region Обновление DocProvider

    /// <summary>
    /// Вызывается при смене свойства DBUI.DocProvider
    /// </summary>
    internal void OnDocProviderChanged()
    {
      _DocType = UI.DocProvider.DocTypes.GetRequired(_DocType.Name);
      foreach (SubDocTypeUI sdtui in SubDocTypes.Items.Values)
        sdtui.OnDocProviderChanged();
    }

    #endregion

    #region Свойства, связанные с группами

    /// <summary>
    /// Вид документа групп, если текущий вид документов использует группировку (свойство DocType.GroupRefColumnName установлено).
    /// Если группировка не предусмотрена, свойство возвращает null
    /// </summary>
    public GroupDocTypeUI GroupDocType
    {
      get { return _GroupDocType; }
      internal set { _GroupDocType = value; }
    }
    private GroupDocTypeUI _GroupDocType;

    /// <summary>
    /// Текстовое значение для узла в дереве, задающего отсутствие иерархии.
    /// По умолчанию возвращает "Все документы".
    /// Скобок "[]" не содержит.
    /// </summary>
    public string AllGroupsDisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_AllGroupsDisplayName))
          return Res.DocTypeUI_Msg_AllGroupsDisplayName;
        else
          return _AllGroupsDisplayName;
      }
      set
      {
        _AllGroupsDisplayName = value;
      }
    }
    private string _AllGroupsDisplayName;

    /// <summary>
    /// Текстовое значение для узла в дереве, задающего документы, не относящиеся ни к одной из групп.
    /// По умолчанию возвращает "Документы без группы".
    /// Скобок "[]" не содержит.
    /// </summary>
    public string NoGroupDisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_NoGroupDisplayName))
          return Res.DocTypeUI_Msg_NoGroupDisplayName;
        else
          return _NoGroupDisplayName;
      }
      set
      {
        _NoGroupDisplayName = value;
      }
    }
    private string _NoGroupDisplayName;

    #endregion

    #region Список видов поддокументов

    /// <summary>
    /// Список для реализации свойства <see cref="SubDocTypes"/>.
    /// </summary>
    public class SubDocTypeList : IEnumerable<SubDocTypeUI>
    {
      #region Защищенный конструктор

      internal SubDocTypeList(DocTypeUI owner)
      {
        _Owner = owner;
        _Items = new Dictionary<string, SubDocTypeUI>(owner.DocType.SubDocs.Count);
      }

      #endregion

      #region Свойства

      private readonly DocTypeUI _Owner;

      internal Dictionary<string, SubDocTypeUI> Items { get { return _Items; } }
      private readonly Dictionary<string, SubDocTypeUI> _Items;

      /// <summary>
      /// Доступ к интерфейса поддокумента по имени таблицы поддокумента.
      /// Если запрошен несуществующий вид поддокумента, которого нет в списке <see cref="DBxDocType.SubDocs"/>
      /// (или поддокумент относится к другому документу), генерируется исключение.
      /// </summary>
      /// <param name="subDocTypeName">Имя таблицы поддокумента</param>
      /// <returns>Интерфейс видов поддокументов</returns>
      public SubDocTypeUI this[string subDocTypeName]
      {
        get
        {
          SubDocTypeUI res;
          if (!_Items.TryGetValue(subDocTypeName, out res))
          {
            if (String.IsNullOrEmpty(subDocTypeName))
              throw ExceptionFactory.ArgStringIsNullOrEmpty("subDocTypeName");
            if (!_Owner.DocType.SubDocs.Contains(subDocTypeName))
              throw new ArgumentException(String.Format(Res.Common_Arg_UnknownSubDocType,
                subDocTypeName, this), "subDocTypeName");

            res = new SubDocTypeUI(_Owner, _Owner.DocType.SubDocs[subDocTypeName]);
            _Items.Add(subDocTypeName, res);
          }
          return res;
        }
      }

      #endregion

      #region IEnumerable<SubDocTypeUI> Members

      /// <summary>
      /// Возвращает перечислитель по списку инициализированных поддокументов
      /// </summary>
      /// <returns></returns>
      public Dictionary<string, SubDocTypeUI>.ValueCollection.Enumerator GetEnumerator()
      {
        return _Items.Values.GetEnumerator();
      }

      IEnumerator<SubDocTypeUI> IEnumerable<SubDocTypeUI>.GetEnumerator()
      {
        return GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Интерфейсы пользователя для видов поддокументов
    /// </summary>
    public SubDocTypeList SubDocTypes { get { return _SubDocTypes; } }
    private readonly SubDocTypeList _SubDocTypes;

    #endregion

    #region Свойства и методы буферизации данных

    /// <summary>
    /// Установка в true позволяет держать полный набор в
    /// памяти клиента. 
    /// </summary>
    public bool DataBuffering
    {
      get
      {
        return _DataBuffering;
      }
      set
      {
        _DataBuffering = value;
        _BufferedData = null;
      }
    }
    private bool _DataBuffering;

    /// <summary>
    /// Обобщенный метод получения буферизованных или небуферизованных данных.
    /// Буферизация используется, если свойство <see cref="DataBuffering"/> установлено в true.
    /// Всегда создает новый объект <see cref="System.Data.DataView"/>, <see cref="DataTable.DefaultView"/> не используется.
    /// Следует вызывать <see cref="DataView"/>.Dispose() по окончании использования.
    /// </summary>
    /// <param name="columns">Требуемые поля</param>
    /// <param name="filter">Условия фильтрации строк или null</param>
    /// <param name="showDeleted">true, если надо показать удаленные строки</param>
    /// <param name="orderBy">Порядок сортировки строк</param>
    /// <returns>Объект DataView, который можно использовать для табличного просмотра или перебора строк</returns>
    public DataView CreateDataView(DBxColumns columns, DBxFilter filter, bool showDeleted, DBxOrder orderBy)
    {
      return CreateDataView(columns, filter, showDeleted, orderBy, 0);
    }

    /// <summary>
    /// Обобщенный метод получения буферизованных или небуферизованных данных.
    /// Буферизация используется, если свойство <see cref="DataBuffering"/> установлено в true.
    /// Можно задать ограничение на максимальное число записей (игнорируется при <see cref="DataBuffering"/>=true).
    /// Если ограничение сработало, то у таблицы <see cref="DataView.Table"/> выставляется свойство <see cref="DataTable.ExtendedProperties"/> "Limited".
    /// Всегда создает новый объект <see cref="System.Data.DataView"/>, <see cref="DataTable.DefaultView"/> не используется.
    /// Следует вызывать <see cref="DataView"/>.Dispose() по окончании использования.
    /// </summary>
    /// <param name="columns">Требуемые поля</param>
    /// <param name="filter">Условия фильтрации строк или null</param>
    /// <param name="showDeleted">true, если надо показать удаленные строки</param>
    /// <param name="orderBy">Порядок сортировки строк</param>
    /// <param name="maxRecordCount">Ограничение на число записей. 0 - нет ограничения</param>
    /// <returns>Объект DataView, который можно использовать для табличного просмотра или перебора строк</returns>
    public DataView CreateDataView(DBxColumns columns, DBxFilter filter, bool showDeleted, DBxOrder orderBy, int maxRecordCount)
    {
      DataView dv;

      DBxColumnList colLst = new DBxColumnList(columns);
      if (DataBuffering)
      {
        if (orderBy != null)
        {
          // Требуется, чтобы поле сортировки присутствовало в выборке
          orderBy.GetColumnNames(colLst);
        }
        if ((!showDeleted) && UI.DocProvider.DocTypes.UseDeleted)
        {
          DBxFilter filter2 = DBSDocType.DeletedFalseFilter;
          if (filter == null)
            filter = filter2;
          else
            filter = new AndFilter(filter, filter2);
        }
        if (filter != null)
          filter.GetColumnNames(colLst); // 09.01.2018
        DataTable dt = GetBufferedData(new DBxColumns(colLst));
        dv = new DataView(dt);
        if (filter == null)
          dv.RowFilter = String.Empty;
        else
          dv.RowFilter = filter.ToString();

        if (orderBy != null)
          dv.Sort = orderBy.ToString();
      }
      else
      {
        DataTable dt = GetUnbufferedData(columns, filter, showDeleted, orderBy, maxRecordCount);
        dv = new DataView(dt);
      }
        
#if DEBUG
      int p = dv.Table.Columns.IndexOf("Id");
      if (p >= 0)
      {
        if (dv.Table.Columns[p].DataType != typeof(Int32))
          throw new BugException("Table " + dv.Table.TableName + " has column 'Id' of wrong type " + dv.Table.Columns[p].DataType.ToString());
      }
#endif

      return dv;
    }

    /// <summary>
    /// Обобщенный метод получения буферизованных или небуферизованных данных.
    /// Буферизация используется, если свойство <see cref="DataBuffering"/> установлено в true.
    /// Эта версия загружает все документы, кроме удаленных. 
    /// Всегда создает новый объект <see cref="System.Data.DataView"/>, <see cref="DataTable.DefaultView"/> не используется.
    /// Следует вызывать <see cref="DataView"/>.Dispose() по окончании использования.
    /// </summary>
    /// <param name="columns">Требуемые поля</param>
    /// <returns>Объект DataView, который можно использовать для табличного просмотра или перебора строк</returns>
    public DataView CreateDataView(DBxColumns columns)
    {
      return CreateDataView(columns, null, false, null, 0);
    }

#if XXX
    /// <summary>
    /// Получить просмотр для фиксированного набора строк
    /// </summary>
    /// <param name="Columns">Требуемые поля</param>
    /// <param name="Values">Массив идентификаторов документов</param>
    /// <returns>Просмотр DataView, содержащий Values.Length строк</returns>
    public DataView GetDataView(DBxColumns Columns, Int32[] Ids)
    {
      DataTable dt;
      if (DataBuffering)
      {
        dt = GetBufferedData(Columns);
        try
        {
          dt = DataTools.CloneOrSameTableForSelectedIds(dt, Ids);
        }
        catch
        {
          // 26.02.2015
          // Возможна ситуация, когда другой пользователь создает новый документ
          // После этого текущий пользователь получает список требуемых идентификаторов Ids, обращаясь к серверу.
          // Буферизованная таблица данных может не содержать нового документа.
          // В результате, метод клонирования сгенерирует исключение.
          // 
          // Перехватываем исключение и выполняем повторную попытку получить буферизованную таблицу

          RefreshBufferedData();
          dt = GetBufferedData(Columns);
          dt = DataTools.CloneOrSameTableForSelectedIds(dt, Ids);
        }
      }
      else
        dt = UI.DocProvider.FillSelect(DocType.Name, Columns, new IdsFilter(Ids));

      DataView dv = new DataView(dt);
      dv.Sort = String.Empty;
      return dv;
    }
#endif

    /// <summary>
    /// Получение документов с сервера без использования буферизации, независимо от
    /// свойства <see cref="DataBuffering"/>. Выполняет непосредственное обращение к серверу.
    /// Добавляет к списку полей <paramref name="columns"/>, если он задан, поле "Id".
    /// Если <paramref name="showDeleted"/>=false, то добавляется фильтр по полю "Deleted" (если <see cref="DBxDocTypes.UseDeleted"/>=true).
    /// Этот метод, в основном, предназначен для внутреннего использования.
    /// В прикладном коде обычно следует использовать вызовы <see cref="DBxDocProvider.FillSelect(DBxSelectInfo)"/>, которые не выполняют дополнительных действий с запросом.
    /// </summary>
    /// <param name="columns">Требуемые поля</param>
    /// <param name="filter">Условия фильтрации строк или null</param>
    /// <param name="showDeleted">true, если надо показать удаленные строки</param>
    /// <param name="orderBy">Порядок сортировки строк</param>
    /// <returns>Таблица <see cref="DataTable"/></returns>
    public DataTable GetUnbufferedData(DBxColumns columns, DBxFilter filter, bool showDeleted, DBxOrder orderBy)
    {
      return GetUnbufferedData(columns, filter, showDeleted, orderBy, 0);
    }

    /// <summary>
    /// Получение документов с сервера без использования буферизации, независимо от
    /// свойства <see cref="DataBuffering"/>. Выполняет непосредственное обращение к серверу.
    /// Добавляет к списку полей <paramref name="columns"/>, если он задан, поле "Id".
    /// Если <paramref name="showDeleted"/>=false, то добавляется фильтр по полю "Deleted" (если <see cref="DBxDocTypes.UseDeleted"/>=true).
    /// Если задано ограничение на число записей <paramref name="maxRecordCount"/>, то через дополнительное свойство <see cref="DataTable.ExtendedProperties"/> с
    /// именем "Limited" возвращается true, если лимит был превышен.
    /// Этот метод, в основном, предназначен для внутреннего использования.
    /// В прикладном коде обычно следует использовать вызовы <see cref="DBxDocProvider.FillSelect(DBxSelectInfo)"/>, которые не выполняют дополнительных действий с запросом.
    /// </summary>
    /// <param name="columns">Требуемые поля</param>
    /// <param name="filter">Условия фильтрации строк или null</param>
    /// <param name="showDeleted">true, если надо показать удаленные строки</param>
    /// <param name="orderBy">Порядок сортировки строк</param>
    /// <param name="maxRecordCount">Ограничение на максимальное число строк. 0-нет ограничения</param>
    /// <returns>Таблица DataTable</returns>
    public DataTable GetUnbufferedData(DBxColumns columns, DBxFilter filter, bool showDeleted, DBxOrder orderBy, int maxRecordCount)
    {
      if ((!showDeleted) && UI.DocProvider.DocTypes.UseDeleted /* 19.12.2017 */)
      {
        DBxFilter filter2 = DBSDocType.DeletedFalseFilter;
        if (filter == null)
          filter = filter2;
        else
          filter = new AndFilter(filter, filter2);
      }

      int MaxRecordCount2 = maxRecordCount;
      if (maxRecordCount > 0)
        MaxRecordCount2++;

      if (columns != null)
      {
        if (!columns.Contains("Id"))
          columns += "Id"; // 25.12.2017
      }

      //DataTable Table = UI.DocProvider.FillSelect(DocType.Name, columns, filter, orderBy, MaxRecordCount2);
      // 27.11.2019
      DBxSelectInfo info = new DBxSelectInfo();
      info.TableName = DocType.Name;
      info.Expressions.AddRange(columns);
      info.Where = filter;
      info.OrderBy = orderBy;
      info.MaxRecordCount = MaxRecordCount2;
      DataTable Table = UI.DocProvider.FillSelect(info);
      DataTools.CheckPrimaryKey(Table, "Id");
      //DebugTools.DebugDataTable(Table, "GetUnbufferedData");
      if (maxRecordCount > 0 && Table.Rows.Count == MaxRecordCount2)
      {
        Table.Rows.RemoveAt(MaxRecordCount2 - 1);
        Table.ExtendedProperties["Limited"] = true;
        Table.AcceptChanges();
      }

      return Table;
    }

    /// <summary>
    /// Получение доступа к буферизованным данным
    /// </summary>
    private DataTable GetBufferedData(DBxColumns columns)
    {
      if (!_DataBuffering)
        return null;

      if (columns == null)
        throw new ArgumentNullException("columns");

      DBxColumnList colLst = new DBxColumnList(columns);
      colLst.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted)
        colLst.Add("Deleted");

      if (_BufferedData != null)
      {
        // Проверяем, все ли поля есть
        if (colLst.HasMoreThan(_BufferedColumns))
        {
          // Некоторых полей не хватает
          _BufferedData = null;
          colLst.AddRange(_BufferedColumns);
        }
      }

      if (_BufferedData == null)
      {
        _BufferedData = UI.DocProvider.FillSelect(DocType.Name, new DBxColumns(colLst), null);
        DataTools.CheckPrimaryKey(_BufferedData, "Id");
        _BufferedColumns = columns;
      }
      return _BufferedData;
    }

    /// <summary>
    /// Существующие буферизованные данные (если загружены)
    /// </summary>
    internal DataTable _BufferedData;

    /// <summary>
    /// Список полей, которые содержаться в FBufferedData
    /// </summary>
    private DBxColumns _BufferedColumns;

    /// <summary>
    /// Очистка буферизованных данных. При следующем обращении к
    /// <see cref="GetBufferedData(DBxColumns)"/> данные будут снова загружены с сервера.
    /// </summary>
    public void RefreshBufferedData()
    {
      _BufferedData = null;
      /*
      for (int i = 0; i < SubDocs.Count; i++)
      {
        SubDocs[i].RefreshBufferedData();
        // @@@ ClientDocType.ValueBuffer.Reset(SubDocs[i].NamePart);
        AccDepClientExec.BufTables.Clear(SubDocs[i].Name);
      }
      // Общий буфер сбрасывается всегда
      // @@@ ClientDocType.ValueBuffer.Reset(NamePart);
      AccDepClientExec.BufTables.Clear(Name);
       * */
    }

    #endregion

    #region Инициализация табличного просмотра

    #region Основной метод

    /// <summary>
    /// Инициализация табличного просмотра документов
    /// </summary>
    /// <param name="controlProvider">Обработчик табличного просмотра</param>
    /// <param name="reInit">true при повторном вызове метода (после изменения конфигурации просмотра)
    /// и false при первом вызове</param>
    /// <param name="columns">Сюда помещается список имен полей, которые требуются для текущей конфигурации просмотра</param>
    /// <param name="userInitData">Свойство <see cref="InitEFPDBxViewEventArgs.UserInitData"/>, передаваемое обработчику <see cref="InitView"/> (если он установлен)</param>
    public void PerformInitGrid(EFPDBxGridView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      if (reInit)
      {
        try
        {
          controlProvider.Columns.Clear();
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, LogoutTools.GetTitleForCall("EFPDataGridViewColumns.Clear()"));
        }
      }
      // Добавляем столбец с картинками
      // 27.05.2015
      // В первоначальном варианте столбец с картинками добавлялся после GridProducer.InitGrid() с помощью Insert()
      // Возникала непонятная ошибка при вызове Form.Dispose(), если DataGridView находится на закладке TabPage и
      // ни разу не отображается
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
        imgCol.Name = "Image";
        imgCol.HeaderText = String.Empty;
        imgCol.ToolTipText = String.Format(Res.Common_ToolTip_DocImageColumn, DocType.SingularTitle);
        imgCol.Width = controlProvider.Measures.ImageColumnWidth;
        imgCol.FillWeight = 1; // 08.02.2017
        imgCol.Resizable = DataGridViewTriState.False;
        //string ImgName = SingleDocImageKey;
        //ImgCol.Image = EFPApp.MainImages.Images[ImgName];
        // ImgCol_CellToopTextNeeded
        controlProvider.Control.Columns.Add(imgCol);
      }
      if (controlProvider.MarkRowIds != null)
        controlProvider.AddMarkRowsColumn();

      controlProvider.GridProducer.InitGridView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021

      controlProvider.FrozenColumns = controlProvider.CurrentConfig.FrozenColumns + (EFPApp.ShowListImages ? 1 : 0);

      if (!reInit)
        CallInitView(controlProvider, userInitData);

      columns.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted)
        columns.Add("Deleted"); //,CheckState,CheckTime";


      //AccDepClientExec.AddGridDebugIdColumn(DocGridHandler.MainGrid);
      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        //Columns += "CreateTime,ChangeTime";
        controlProvider.Columns.AddInteger("Id", true, String.Empty, 5);
        controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        controlProvider.Columns.LastAdded.CanIncSearch = true;
        //TODO: if (UseHieView)
        //TODO: {
        //TODO:   ControlProvider.Columns.AddInt32("GroupId");
        //TODO:   ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        //TODO: }

#if XXX // Убрано 02.07.2021. Теперь есть произвольная сортировка
        if (controlProvider.UseGridProducerOrders) // 24.11.2015
        {
          controlProvider.AutoSort = true;
          if (controlProvider.Orders.Count == 0)
          {
            if (DocType.DefaultOrder.ToString() != "Id")
            {
              EFPDBxViewOrder MainOrder = controlProvider.Orders.Add(DocType.DefaultOrder, "Основной порядок");
              MainOrder.Order.GetColumnNames(columns);
            }
          }
          if (controlProvider.Orders.IndexOfItemForGridColumn("Id") < 0)
            controlProvider.Orders.Add("Id", "По идентификатору Id");

          if (UI.DocProvider.DocTypes.UseTime)
          {
            // Этот фрагмент никогда не выполняется, т.к. порядок сортировки по столбцам CreateTime/ChangeTime
            // всегда добавляется в DBUI.EndInit()
            if (controlProvider.Orders.IndexOfItemForGridColumn("CreateTime") < 0 &&
              controlProvider.Orders.IndexOfItemForGridColumn("ChangeTime") < 0)
            {
              DBxOrder ChOrder1 = new DBxOrder(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ChangeTime"), new DBxColumn("CreateTime")), ListSortDirection.Ascending);
              DBxOrder ChOrder2 = new DBxOrder(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ChangeTime"), new DBxColumn("CreateTime")), ListSortDirection.Descending);
              controlProvider.Orders.Add(ChOrder1, "По времени изменения (новые внизу)", new EFPDataGridViewSortInfo("ChangeTime", ListSortDirection.Ascending));
              controlProvider.Orders.Add(ChOrder2, "По времени изменения (новые сверху)", new EFPDataGridViewSortInfo("ChangeTime", ListSortDirection.Descending));
              columns.Add("CreateTime");
              columns.Add("ChangeTime");
            }
          }
        }
#endif
      }

      // 28.01.2022
      controlProvider.SetColumnsReadOnly(true);
      if (controlProvider.MarkRowsColumn != null)
        controlProvider.MarkRowsColumn.GridColumn.ReadOnly = false;

      if (!reInit)
      {
        controlProvider.Control.VirtualMode = true;
        // значков состояния документа нет. ControlProvider.UseRowImages = true; 
        controlProvider.RowInfoNeeded += new EFPDataGridViewRowInfoEventHandler(ControlProvider_RowInfoNeeded);
        //if (CondFields != null)
        //  ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributesForCond);
        if (EFPApp.ShowListImages)
          controlProvider.CellInfoNeeded += new EFPDataGridViewCellInfoEventHandler(ControlProvider_CellInfoNeeded);
        //DocGridHandler.MainGrid.CellPainting += new DataGridViewCellPaintingEventHandler(TheGrid_CellPainting);
        //DocGridHandler.MainGrid.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(TheGrid_CellToolTipTextNeeded);

        // Добавляем команды локального меню
        InitCommandItems(controlProvider);
        controlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(DocGrid_GetDocSel);

        //ControlProvider.UserConfigModified = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      //      if (ReInit)
      //        FToolTipExtractor = null;

      controlProvider.PerformGridProducerPostInit();
    }

    #endregion

    #region Событие InitView

    /// <summary>
    /// Вызывается при инициализации таблицы просмотра документов
    /// для добавления столбцов. Если обработчик не установлен, выполняется инициализация по умолчанию.
    /// Событие вызывается однократно. При изменении настроек просмотра не вызывается.
    /// </summary>
    public event InitEFPDBxViewEventHandler InitView;

    private void CallInitView(IEFPDBxView controlProvider, object userInitData)
    {
      if (InitView != null)
      {
        try
        {
          InitEFPDBxViewEventArgs args = new InitEFPDBxViewEventArgs(controlProvider);
          args.UserInitData = userInitData;
          InitView(this, args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }
    }

    /// <summary>
    /// Вызывается при инициализации таблицы просмотра выборки документов.
    /// В отличие от основного события <see cref="InitView"/>, обработчик этого события не должен добавлять фильтры в просмотр.
    /// Обработчик может добавить команды локального меню, например, для группового добавления ссылок на документы в выборку из других документов.
    /// </summary>
    public event InitEFPDocSelViewEventHandler InitDocSelView;

    /// <summary>
    /// Вызывает обработчик события <see cref="InitDocSelView"/>, если он установлен, с перехватом ошибок.
    /// </summary>
    /// <param name="controlProvider">Провайдер просмотра выборки документов</param>
    public void CallInitDocSelView(IEFPDocSelView controlProvider)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");

      if (InitDocSelView != null)
      {
        try
        {
          InitEFPDocSelViewEventArgs args = new InitEFPDocSelViewEventArgs(controlProvider);
          InitDocSelView(this, args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }
    }

    #endregion

    #region Оформление просмотра

    void ControlProvider_RowInfoNeeded(object sender, EFPDataGridViewRowInfoEventArgs args)
    {
      DataRow row = args.DataRow;
      if (row == null)
        return;

      // 24.11.2017
      // Вызываем пользовательский обработчик и для удаленных документов
      //if (DataTools.GetBoolean(Row, "Deleted"))
      //  Args.Grayed = true;
      //else
      //{
      UIDataViewColorType colorType;
      bool grayed;
      UI.ImageHandlers.GetRowColor(DocType.Name, row, out colorType, out grayed);
      args.ColorType = colorType;
      args.Grayed = grayed;
      //}
#if XXX
      int CheckState = DataTools.GetInt32(Row, "CheckState");
      switch (CheckState)
      {
        case DocumentCheckState.Unchecked:
          Args.ImageKind = GridHandlerImageKind.Information; // не проверена
          Args.UserImage = EFPApp.MainImages.Images["UnknownState"];
          if (Args.Reason == GridAttributesReason.ToolTip)
            Args.ToolTipText = "Проверка не выполнялась. Выполните команду проверки документа";
          break;
        case DocumentCheckState.Ok:
          if (Args.Reason == GridAttributesReason.ToolTip)
            Args.ToolTipText = "Нет ошибок";
          break;
        case DocumentCheckState.Warnings:
          Args.ImageKind = GridHandlerImageKind.Warning;
          if (Args.Reason == GridAttributesReason.ToolTip)
            Args.ToolTipText = "Есть предупреждения";
          break;
        case DocumentCheckState.Errors:
          Args.ImageKind = GridHandlerImageKind.Error;
          if (Args.Reason == GridAttributesReason.ToolTip)
            Args.ToolTipText = "Есть ошибки";
          break;
        case DocumentCheckState.FatalErrors:
          Args.ImageKind = GridHandlerImageKind.Error;
          Args.UserImage = EFPApp.MainImages.Images["FatalError"];
          if (Args.Reason == GridAttributesReason.ToolTip)
            Args.ToolTipText = "!!! Опасная ошибка !!!";
          break;
        default:
          Args.ImageKind = GridHandlerImageKind.Error;
          if (Args.Reason == GridAttributesReason.ToolTip)
            throw new BugException("Неизвестное значение CheckState=" + CheckState.ToString());
          break;
      }
      if (Args.Reason == GridAttributesReason.ToolTip)
      {
        if (CheckState != DocumentCheckState.Unchecked)
        {
          DateTime CheckTime = DataTools.GetDateTime(Row, "CheckTime");
          TimeSpan Span = DateTime.Now - CheckTime;
          Args.ToolTipText += Environment.NewLine+"Проверка выполнена " + DataConv.TimeSpanToStr(Span);
        }
      }
#endif
    }

    void ControlProvider_GetRowAttributesForCond(object sender, EFPDataGridViewRowInfoEventArgs args)
    {
      // TODO: 
#if XXX

      if (Args.DataRow == null)
        return;
      string Why;
      DBxAccessMode Mode =  AccDepClientExec.Permissions.CheckRowCondition(Name, Args.DataRow, out Why);
      if (Mode == AccDepAccessMode.Full)
        return;
      EFPAccDepGrid GridHandler = (EFPAccDepGrid)Sender;
      if (GridHandler.ReadOnly && Mode == AccDepAccessMode.ReadOnly)
        return;

      if (Args.Reason == GridAttributesReason.ToolTip)
        Args.ToolTipText += Environment.NewLine + (Mode == AccDepAccessMode.ReadOnly ? "Разрешен только просмотр" : "Доступ запрещен") + ". " + Why;
      Args.Grayed = true;
#endif
    }

    void ControlProvider_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.ColumnName == "Image")
      {
        DataRow row = args.DataRow;
        if (row == null)
          return;

        switch (args.Reason)
        {
          case EFPDataViewInfoReason.View:
            args.Value = GetImageValue(row);
            break;
          case EFPDataViewInfoReason.ToolTip:
            if (args.ControlProvider.CurrentConfig != null)
            {
              if (args.ControlProvider.CurrentConfig.CurrentCellToolTip)
              {
                string s1 = GetToolTipText(row);
                args.ToolTipText = StringTools.JoinNotEmptyStrings(Environment.NewLine, new string[] { s1, args.ToolTipText }); // 06.02.2018
              }

              if (args.ControlProvider.CurrentConfig.ToolTips.Count > 0)
              {
                string s2 = null;
                try
                {
                  EFPDataViewRowValues rowValues = args.ControlProvider.GetRowValues(args.RowIndex);
                  s2 = GridProducer.ToolTips.GetToolTipText(args.ControlProvider.CurrentConfig, rowValues);
                  args.ControlProvider.FreeRowValues(rowValues);
                }
                catch (Exception e)
                {
                  s2 = String.Format(Res.Common_Err_ErrorMessage, e.Message);
                }
                args.ToolTipText = StringTools.JoinNotEmptyStrings(EFPGridProducerToolTips.ToolTipTextSeparator, new string[] { args.ToolTipText, s2 });
              }
            }
            break;
        }
      }
    }

#if XXXXXXXXXXXXX
    void TheGrid_CellToolTipTextNeeded(object Sender, DataGridViewCellToolTipTextNeededEventArgs Args)
    {
      try
      {
        DoTheGrid_CellToolTipTextNeeded(Sender, Args);
      }
      catch (Exception e)
      {
        Args.ToolTipText = "Внутренняя ошибка: " + e.Message;
      }
    }

    void DoTheGrid_CellToolTipTextNeeded(object Sender, DataGridViewCellToolTipTextNeededEventArgs Args)
    {
      if (Args.RowIndex < 0)
        return;
      if (Args.ColumnIndex >= 0)
        return;

      DataGridView Grid = (DataGridView)Sender;

      DataRow Row = GridHandler.GetDataRow(Grid, Args.RowIndex);
      int CheckState = DataTools.GetInt32(Row, "CheckState");
      if (CheckState == 0)
        Args.ToolTipText = "Проверка не выполнялась. Выполните команду проверки документа";
      else
      {
        switch (CheckState)
        {
          case 1:
            Args.ToolTipText = "Нет ошибок";
            break;
          case 2:
            Args.ToolTipText = "Есть предупреждения";
            break;
          case 3:
            Args.ToolTipText = "Есть ошибки";
            break;
          case 4:
            Args.ToolTipText = "!!! Опасная ошибка !!!";
            break;
          default:
            throw new BugException("Неизвестное значение CheckState=" + CheckState.ToString());
        }
        DateTime CheckTime = DataTools.GetDateTime(Row, "CheckTime");
        TimeSpan Span = DateTime.Now - CheckTime;
        Args.ToolTipText += Environment.NewLine+"Проверка выполнена " + DataConv.TimeSpanToStr(Span);
      }
    }
#endif

#if XXXXXXXXXXXXXXXXXx
    void TheGrid_CellPainting(object Sender, DataGridViewCellPaintingEventArgs Args)
    {
      if (Args.RowIndex < 0)
        return;
      if (Args.ColumnIndex >= 0)
        return;
      Args.PaintBackground(Args.ClipBounds, false);
      Args.PaintContent(Args.ClipBounds);

      DataGridView Grid = (DataGridView)Sender;
      DataRow Row = GridHandler.GetDataRow(Grid, Args.RowIndex);
      int CheckState = DataTools.GetInt32(Row, "CheckState");
      Image img = null;
      switch (CheckState)
      {
        case 0:
          img = EFPApp.MainImages.Images["UnknownState"];
          break;
        case 2:
          img = EFPApp.MainImages.Images["Warning"];
          break;
        case 3:
          img = EFPApp.MainImages.Images["Error"];
          break;
        case 4:
          img = EFPApp.MainImages.Images["FatalError"];
          break;
      }
      if (img != null)
      {
        Rectangle r = Args.CellBounds;
        r.Inflate(-3, -3);
        r.X += r.Width / 2;
        r.Width = r.Width / 2;
        //r.Width -= 3;
        //      Args.Graphics.FillRectangle(new SolidBrush(Args.CellStyle.BackColor));
        //Args.Graphics.FillRectangle(Grid.RowHeadersDefaultCellStyle.BackColor, r);
        Args.Graphics.DrawImage(img, r.Location);
      }
      Args.Handled = true;
    }
#endif

    /// <summary>
    /// 01.07.2011
    /// Для картинки во всплывающей подсказке выводим текстовое представление
    /// </summary>
    private void ImgCol_CellToopTextNeeded(object sender, EFPDataGridViewCellToolTipTextNeededEventArgs args)
    {
      //Int32 Id = DataTools.GetInt32(Args.Row, "Id");
      //Args.ToolTipText = GetToolTipText(Id);
    }

    #endregion

    #endregion

    #region Список столбцов для табличного просмотра

    /// <summary>
    /// Получить список столбцов, необходимых для табличного просмотра с заданной конфигурации.
    /// Заполняется такой же список столбов, как и в методе <see cref="PerformInitGrid(EFPDBxGridView, bool, DBxColumnList, object)"/>, но без создания самого просмотра.
    /// </summary>
    /// <param name="columns">Заполняемый список столбцов</param>
    /// <param name="config">Конфигурация табличного просмотра. Если null, то используется конфигурация по умолчанию</param>
    public void GetColumnNames(DBxColumnList columns, EFPDataViewConfig config)
    {
      columns.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted) // 16.05.2018
        columns.Add("Deleted");
      DocType.DefaultOrder.GetColumnNames(columns);

      if (config == null)
      {
        config = GridProducer.DefaultConfig;
        if (config == null)
          return; // только поле Id
      }

      GridProducer.GetColumnNames(config, columns);
    }

    #endregion

    #region Инициализация TreeViewAdv

    #region Основной метод

    /// <summary>
    /// Инициализация иерархического просмотра
    /// </summary>
    /// <param name="controlProvider">Провайдер иерархического просмотра, в который требуется добавить столбцы</param>
    /// <param name="reInit">True, если выполняется повторная инициализации после настройки просмотра.
    /// False - первичная инициализация при выводе формы на экран</param>
    /// <param name="columns">Сюда добавляются имена полей, которые должны быть в наборе данных</param>
    /// <param name="userInitData">Свойство <see cref="InitEFPDBxViewEventArgs.UserInitData"/>, передаваемое обработчику <see cref="InitView"/> (если он установлен)</param>
    public void PerformInitTree(EFPDocTreeView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      if (reInit)
      {
        try
        {
          controlProvider.Columns.Clear();
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, LogoutTools.GetTitleForCall("EFPDataTreeViewColumns.Clear()"));
        }
      }


      controlProvider.GridProducer.InitTreeView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021
      TreeViewCachedValueAdapter.InitColumns(controlProvider, UI.TextHandlers.DBCache, GridProducer);

      if (controlProvider.Control.Columns.Count > 0)
      {
        NodeStateIcon ni = new NodeStateIcon();
        //ni.DataPropertyName = "Icon";
        ni.VirtualMode = true;
        ni.ValueNeeded += new EventHandler<NodeControlValueEventArgs>(controlProvider.NodeStateIconValueNeeded);
        ni.ParentColumn = controlProvider.Control.Columns[0];
        controlProvider.Control.NodeControls.Insert(0, ni);

        controlProvider.Control.Columns[0].Width += 24; // ???
      }



      if (!reInit)
        CallInitView(controlProvider, userInitData);

      columns.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted) // 16.05.2018
        columns.Add("Deleted"); //,CheckState,CheckTime";
      columns.Add(DocType.TreeParentColumnName);

      //Columns += "CreateTime,ChangeTime";
      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        controlProvider.Columns.AddInt32("Id", true, "Id", 6);
        controlProvider.Columns.LastAdded.TreeColumn.Sortable = false;
        controlProvider.Columns.LastAdded.CanIncSearch = true;
      }

#if XXX
      if (ControlProvider.UseGridProducerOrders) // 24.11.2015
      {
        ControlProvider.AutoSort = true;
        if (ControlProvider.Orders.Count == 0)
        {
          if (DocType.DefaultOrder.ToString() != "Id")
          {
            EFPDBxGridViewOrder MainOrder = ControlProvider.Orders.Add(DocType.DefaultOrder, "Основной порядок");
            MainOrder.Order.GetColumnNames(Columns);
          }
        }
        if (ControlProvider.Orders.IndexOfFirstColumnName("Id") < 0)
          ControlProvider.Orders.Add("Id", "По идентификатору Id");

        if (UI.DocProvider.DocTypes.UseTime)
        {
          // Этот фрагмент никогда не выполняется, т.к. порядок сортировки по столбцам CreateTime/ChangeTime
          // всегда добавляется в DBUI.EndInit()
          if (ControlProvider.Orders.IndexOfFirstColumnName("CreateTime") < 0 &&
            ControlProvider.Orders.IndexOfFirstColumnName("ChangeTime") < 0)
          {
            DBxOrder ChOrder1 = new DBxOrder(new DBxOrderColumnIfNull("ChangeTime", typeof(DateTime), new DBxOrderColumn("CreateTime")), false);
            DBxOrder ChOrder2 = new DBxOrder(new DBxOrderColumnIfNull("ChangeTime", typeof(DateTime), new DBxOrderColumn("CreateTime")), true);
            ControlProvider.Orders.Add(ChOrder1, "По времени изменения (новые внизу)", new EFPDataGridViewSortInfo("ChangeTime", false));
            ControlProvider.Orders.Add(ChOrder2, "По времени изменения (новые сверху)", new EFPDataGridViewSortInfo("ChangeTime", true));
            Columns.Add("CreateTime");
            Columns.Add("ChangeTime");
          }
        }
      }
#endif

      controlProvider.SetColumnsReadOnly(true);

      if (!reInit)
      {
        // Добавляем команды локального меню
        InitCommandItems(controlProvider);
        controlProvider.GetDocSel += new EFPDBxTreeViewDocSelEventHandler(DocTree_GetDocSel);

        // TODO: ControlProvider.UserConfigModified = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      //      if (ReInit)
      //        FToolTipExtractor = null;
      controlProvider.PerformGridProducerPostInit();
    }

    #endregion


    void DocTree_GetDocSel(object sender, EFPDBxTreeViewDocSelEventArgs args)
    {
      IIdSet<Int32> docIds = IdTools.GetIdsFromColumn<Int32>(args.DataRows, "Id");
      PerformGetDocSel(args.DocSel, docIds, args.Reason);
    }

    #endregion

    #region Команды локального меню

    private void InitCommandItems(IEFPDBxView controlProvider)
    {
      EFPCommandItem ci;

#if XXX
      EFPCommandItem ciCheckDocuments = new EFPCommandItem("Вид", "ПроверитьДокументы");
      ciCheckDocuments.MenuText = "Проверить выделенные документы";
      ciCheckDocuments.ToolTipText = "Проверить выделенные документы";
      ciCheckDocuments.ImageKey = "ПроверитьДокумент";
      ciCheckDocuments.Tag = ControlProvider;
      ciCheckDocuments.Click += new EventHandler(ciCheckDocuments_Click);
      ControlProvider.CommandItems.Add(ciCheckDocuments);
#endif

      if (DocType.HasCalculatedColumns &&
        UI.DocProvider.DBPermissions.TableModes[DocType.Name] == DBxAccessMode.Full /*&& (!ControlProvider.ReadOnly)*/)
      {
        RecalcColumnsPermissionMode mode = RecalcColumnsPermission.GetMode(UI.DocProvider.UserPermissions);

        if (mode != RecalcColumnsPermissionMode.Disabled)
        {
          EFPCommandItem MenuRecalcDocuments = new EFPCommandItem("Service", "RecalcDocsMenu");
          MenuRecalcDocuments.MenuText = Res.Cmd_Menu_Service_RecalcDocsMenu;
          MenuRecalcDocuments.Usage = EFPCommandItemUsage.Menu;
          MenuRecalcDocuments.ImageKey = "RecalcColumns";
          MenuRecalcDocuments.Enabled = !controlProvider.ReadOnly;
          controlProvider.CommandItems.Add(MenuRecalcDocuments);

          ci = new EFPCommandItem("Service", "RecalcSelectedDocs");
          ci.MenuText = Res.Cmd_Menu_Service_RecalcSelectedDocs;
          ci.Parent = MenuRecalcDocuments;
          ci.Tag = controlProvider;
          ci.Click += new EventHandler(RecalcSelDocuments_Click);
          controlProvider.CommandItems.Add(ci);

          ci = new EFPCommandItem("Service", "RecalcDocsInView");
          ci.MenuText = Res.Cmd_Menu_Service_RecalcDocsInView;
          ci.Parent = MenuRecalcDocuments;
          ci.Tag = controlProvider;
          ci.Click += new EventHandler(RecalcViewDocuments_Click);
          controlProvider.CommandItems.Add(ci);

          if (mode == RecalcColumnsPermissionMode.All)
          {
            ci = new EFPCommandItem("Service", "RecalcAllDocs");
            ci.MenuText = Res.Cmd_Menu_Service_RecalcAllDocs;
            ci.Parent = MenuRecalcDocuments;
            ci.Tag = controlProvider;
            ci.Click += new EventHandler(RecalcAllDocuments_Click);
            controlProvider.CommandItems.Add(ci);
          }
        }
      }

      ci = new EFPCommandItem("View", "DocInfo");
      ci.MenuText = Res.Cmd_Menu_DocInfo;
      ci.ShortCut = Keys.F12;
      ci.ImageKey = "Information";
      ci.Click += new EventHandler(ShowDocInfoItem_Click);
      ci.Tag = controlProvider;
      ci.Enabled = DocTypeViewHistoryPermission.GetAllowed(UI.DocProvider.UserPermissions, DocType.Name); // 11.04.2016
      controlProvider.CommandItems.Add(ci);


#if XXX
      ControlProvider.CommandItems.InitGoto += new InitGotoItemsEventHandler(DocGrid_InitGoto);

      if (this.WholeRefBook != null)
        this.WholeRefBook.InitCommandItems(ControlProvider);
#endif

#if XXX
      AccDepFileType FileType = AccDepFileType.CreateDataSetXML();
      FileType.SaveCode = FileType.OpenCode + "Export";
      FileType.ImageKey = "XMLDataSet";
      FileType.DisplayName = "Данные для экспорта (XML)";
      FileType.Save += new AccDepFileEventHandler(SaveDataSet);
      FileType.Tag = ControlProvider;
      ControlProvider.CommandItems.SaveTypes.Add(FileType);

#endif
    }

#if XXX
    void ShowBufRow_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(ci.Tag);
      DataRow Row = AccDepClientExec.BufTables.GetTableRow(Name, GridHandler.CurrentId);
      DebugTools.DebugDataRow(Row, Name + " (Id=" + GridHandler.CurrentId.ToString() + ")");
    }
    void ciCheckDocuments_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(ci.Tag);

      Int32[] DocIds = GridHandler.SelectedIds;
      DocCheckExec.CheckDocIds(this, DocIds);
      GridHandler.PerformRefresh();
    }
#endif

    private delegate void PerformRefreshDelegate();

    // TODO: Пока не знаю как делать. Нужно вызывать пересчет асинхронно, иначе приложение зависнет,
    // если выбрано много записей.
    void RecalcSelDocuments_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      IEFPDBxView ControlProvider = (IEFPDBxView)(ci.Tag);

      IIdSet<Int32> docIds = ControlProvider.SelectedIds;
      this.RecalcColumns(docIds, new PerformRefreshDelegate(ControlProvider.PerformRefresh));
    }

    void RecalcViewDocuments_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      IEFPDBxView ControlProvider = (IEFPDBxView)(ci.Tag);

      if (ControlProvider.SourceAsDataView == null)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoData);
        return;
      }

      IIdSet<Int32> docIds = IdTools.GetIdsFromColumn<Int32>(ControlProvider.SourceAsDataView, "Id");
      this.RecalcColumns(docIds, new PerformRefreshDelegate(ControlProvider.PerformRefresh));
    }

    void RecalcAllDocuments_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      IEFPDBxView ControlProvider = (IEFPDBxView)(ci.Tag);

      this.RecalcColumns(null, new PerformRefreshDelegate(ControlProvider.PerformRefresh));
    }

#if XXX
    //void DocGrid_InitGoto(object Sender, InitGotoItemsEventArgs Args)
    //{
    //  GotoMenuItem Item = new GotoMenuItem("Id", "По идентификатору документа",
    //    new EventHandler(GotoId_Click), "ПоказыватьИдентификаторы", Args.GridHandler);
    //  Args.Items.Add(Item);
    //}

    void GotoId_Click(object Sender, EventArgs Args)
    {
      GotoMenuItem Item = (GotoMenuItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(Item.Tag);

      Int32 Id = GridHandler.CurrentId;

      if (!EFPApp.IntInputBox(ref Id, "Перейти по идентификатору", "Идентификатор"))
        return;

      if (Id == 0)
        EFPApp.ShowTempMessage("Идентификатор должен быть задан");

      GridHandler.CurrentId = Id;
      if (GridHandler.CurrentId != Id)
      {
        if (AccDepClientExec.FindRecord(Name, new IdsFilter(Id)) == 0)
          EFPApp.ShowTempMessage("Документ \"" + SingularTitle + "\" с идентификатором " + Id.ToString() + " не существует в базе данных");
        else
          EFPApp.ShowTempMessage("Не удалось найти документ \"" + SingularTitle + "\" с идентификатором " + Id.ToString() + " в этой таблице");
      }
    }
#endif

    void DocGrid_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      IIdSet<Int32> docIds = IdTools.GetIdsFromColumn<Int32>(args.DataRows, "Id");
      PerformGetDocSel(args.DocSel, docIds, args.Reason);
    }

    /*
    void SaveDataSet(object Sender, AccDepFileEventArgs Args)
    {
      AccDepFileType FileType = (AccDepFileType)Sender;
      EFPAccDepIdGrid DocGridHandler = (EFPAccDepIdGrid)(FileType.Tag);

      bool AllRows = Args.Config.GetBoolean("ВсеСтроки");
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = "Сохранение данных для экспорта";
      dlg.ImageKey = "XMLDataSet";
      dlg.GroupTitle = "Что сохранить";
      dlg.Items = new string[] { 
        "Все документы \""+PluralTitle+"\" в просмотре ("+DocGridHandler.Control.RowCount.ToString()+")", 
        "Только для выбранных строк ("+DocGridHandler.SelectedRowCount+")" };
      dlg.SelectedIndex = AllRows ? 0 : 1;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      AllRows = dlg.SelectedIndex == 0;
      Args.Config.SetBoolean("ВсеСтроки", AllRows);

      Int32[] DocIds;
      if (AllRows)
        DocIds = DataTools.GetIds(DocGridHandler.SourceAsDataView);
      else
        DocIds = DocGridHandler.SelectedIds;
      DataSet ds = AccDepClientExec.CreateExportDataSet(Name, DocIds);
      AccDepClientExec.SaveExportDataSet(Args.FileName.Path, ds);
    }
     * */

    private void ShowDocInfoItem_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;

      IEFPDBxView controlProvider = (IEFPDBxView)(ci.Tag);

      Int32 docId = controlProvider.CurrentId;
      if (docId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return;
      }

      ShowDocInfo(docId);
    }

    #endregion

    #region Список открытых табличных просмотров

    /// <summary>
    /// Список открытых табличных просмотров для данного типа документов
    /// </summary>
    public DocumentViewHandlerList Browsers { get { return _Browsers; } }
    private readonly DocumentViewHandlerList _Browsers;

    #endregion

    #region Создание команд меню

    /// <summary>
    /// Команда главного меню "Журналы" или "Справочники".
    /// Свойство устанавливается вызовом <see cref="CreateMainMenuItem(EFPCommandItem)"/>, а до этого содержит null.
    /// </summary>
    public EFPCommandItem MainMenuCommandItem { get { return _MainMenuCommandItem; } set { _MainMenuCommandItem = value; } }
    private EFPCommandItem _MainMenuCommandItem;

    /// <summary>
    /// Создает команду главного меню для просмотра справочника или другого вида документа.
    /// Команда сохраняется в свойстве <see cref="MainMenuCommandItem"/>. После этого происходит автоматическое
    /// управление видимостью команды.
    /// Нельзя использовать метод для создания команд локального меню.
    /// </summary>
    /// <param name="parent">Родительская команда меню, например "Справочники".
    /// Теоретически, может быть null, хотя создание команд непосредственно в строке меню является неправильным</param>
    /// <returns>Созданная команда меню</returns>
    public EFPCommandItem CreateMainMenuItem(EFPCommandItem parent)
    {
      MainMenuCommandItem = new EFPCommandItem("RefBooks", DocType.Name);
      MainMenuCommandItem.Parent = parent;
      MainMenuCommandItem.MenuText = DocType.PluralTitle;
      MainMenuCommandItem.ImageKey = ImageKey;
      MainMenuCommandItem.Click += new EventHandler(CommandItem_Click);
      EFPApp.CommandItems.Add(MainMenuCommandItem);
      InitMainMenuCommandItemVisible();
      return MainMenuCommandItem;
    }

    private void CommandItem_Click(object sender, EventArgs args)
    {
      ShowOrOpen(null, 0, "Menu");
    }

    /// <summary>
    /// Если была создана команда главного меню (свойство <see cref="MainMenuCommandItem"/>), то метод
    /// инициализирует видимость команды в зависимости от разрешений текущего пользователя.
    /// Этот метод вызывается из <see cref="DBUI.EndInit()"/> и <see cref="DBUI.OnDocProviderChanged()"/>.
    /// </summary>
    public void InitMainMenuCommandItemVisible()
    {
      if (MainMenuCommandItem == null)
        return;

      DBxAccessMode accessMode = DBxAccessMode.Full;
      for (int i = UI.DocProvider.UserPermissions.Count - 1; i >= 0; i--)
      {
        UserPermission up = UI.DocProvider.UserPermissions[i];
        DocTypePermission dtp = up as DocTypePermission;
        if (dtp != null)
        {
          if (dtp.DocTypeNames == null)
          {
            accessMode = dtp.Mode;
            break;
          }
          else if (StringTools.IndexOf(dtp.DocTypeNames, DocType.Name, StringComparison.OrdinalIgnoreCase) >= 0)
          {
            accessMode = dtp.Mode;
            break;
          }
        }
        WholeDBPermission dbp = up as WholeDBPermission;
        if (dbp != null)
        {
          accessMode = dbp.Mode;
          break;
        }
      }

      MainMenuCommandItem.Visible = (accessMode != DBxAccessMode.None);
    }

    #endregion

    #region Окно просмотра списка документов

    /// <summary>
    /// Открытие новой или активация существующей формы просмотра документов с заданными фильтрами.
    /// </summary>
    /// <param name="externalFilters">Фильтры, которые должны быть установлены в просмотре. Ксли null,
    /// то используется последняя сохраненная конфигурация фильтров, как если бы просмотр был открыт командой меню.
    /// Аргумент используется только при создании новой формы</param>
    /// <param name="currentDocId">Идентификатор документа, строка которого должна быть выбрана. Если 0, то выбор не
    /// осуществляется. Если документ не проходит условия фильтра, выбор также не осуществляется.</param>
    /// <param name="formSearchKey">Ключ для поиска существующего просмотра (свойство 
    /// DocTableViewForm.FormSearchKey). Если аргумент равен null (но не пустой строке), то, если аргумент
    /// <paramref name="externalFilters"/> задан, то ключ создается из настроек фильтров.
    /// Иначе используется пустая строка в качестве ключа</param>
    /// <returns>Объект формы</returns>
    public DocTableViewForm ShowOrOpen(EFPDBxGridFilters externalFilters, Int32 currentDocId, string formSearchKey)
    {
      bool hasExternalFilters = false;
      if (externalFilters != null)
      {
        externalFilters.SqlFilterRequired = true;
        hasExternalFilters = externalFilters.Count > 0;
      }

      if (formSearchKey == null)
      {
        if (hasExternalFilters)
        {
          TempCfg Cfg = new TempCfg();
          externalFilters.WriteConfig(Cfg);
          formSearchKey = Cfg.AsXmlText;
        }
        else
          formSearchKey = String.Empty;
      }

      DocTableViewForm form = null;
      if (EFPApp.ActiveDialog == null) // 10.03.2016
        form = FindAndActivate(formSearchKey);
      if (form == null)
      {
        form = new DocTableViewForm(this, DocTableViewMode.Browse);
        form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        form.FormSearchKey = formSearchKey;

        form.ExternalFilters = externalFilters;
        if (hasExternalFilters)
          form.FormProvider.ConfigClassName = String.Empty; // 02.11.2018


        form.CurrentDocId = currentDocId;
        EFPApp.ShowFormOrDialog(form);
      }
      else if (currentDocId != 0)
        form.CurrentDocId = currentDocId;

      return form;
    }

    /// <summary>
    /// Открытие новой или активация существующей формы просмотра документов с заданными фильтрами.
    /// Эта версия метода использует ключ для поиска, создаваемый из <paramref name="externalFilters"/>
    /// </summary>
    /// <param name="externalFilters">Фильтры, которые должны быть установлены в просмотре. Если null,
    /// то используется последняя сохраненная конфигурация фильтров, как если бы просмотр был открыт командой меню.
    /// Аргумент используется только при создании новой формы</param>
    /// <param name="currentDocId">Идентификатор документа, строка которого должна быть выбрана. Если 0, то выбор не
    /// осуществляется. Если документ не проходит условия фильтра, выбор также не осуществляется.</param>
    /// <returns>Объект формы</returns>
    public DocTableViewForm ShowOrOpen(EFPDBxGridFilters externalFilters, Int32 currentDocId)
    {
      return ShowOrOpen(externalFilters, currentDocId, null);
    }

    /// <summary>
    /// Открытие новой или активация существующей формы просмотра документов с заданными фильтрами
    /// Эта версия метода использует ключ для поиска, создаваемый из <paramref name="externalFilters"/>.
    /// Выбор документа не выполняется
    /// </summary>
    /// <param name="externalFilters">Фильтры, которые должны быть установлены в просмотре. Если null,
    /// то используется последняя сохраненная конфигурация фильтров, как если бы просмотр был открыт командой меню.
    /// Аргумент используется только при создании новой формы</param>
    /// <returns>Объект формы</returns>
    public DocTableViewForm ShowOrOpen(EFPDBxGridFilters externalFilters)
    {
      return ShowOrOpen(externalFilters, 0, null);
    }

    /// <summary>
    /// Поиск открытой формы просмотра списка документов.
    /// Если форма найдена, то она активируется.
    /// Если нет найденной формы, никаких действий не выполняется и возвращается null.
    /// Обычно, справочники, отрываемые из главного меню не имеют ключа поиска.
    /// Для специальных окон, например, просмотра определенного фиксированного подмножества документов,
    /// часто используется ключ.
    /// Ключ используется для точного поиска таких форм.
    /// Формы справочников разных видов документов могут иметь одинаковые ключи.
    /// </summary>
    /// <param name="formSearchKey">Ключ поиска формы. 
    /// Если не задан, то будет найдена первая попавшаяся форма для текущего вида документов</param>
    /// <returns>Найденная форма или null</returns>
    public DocTableViewForm FindAndActivate(string formSearchKey)
    {
      if (EFPApp.Interface != null)
      {
        Form[] forms = EFPApp.Interface.GetChildForms(true);
        for (int i = 0; i < forms.Length; i++)
        {
          if (forms[i] is DocTableViewForm)
          {
            DocTableViewForm frm = (DocTableViewForm)(forms[i]);
            if (frm.DocTypeName != this.DocType.Name)
              continue;
            if (!String.IsNullOrEmpty(formSearchKey))
            {
              if (frm.FormSearchKey != formSearchKey)
                continue;
            }

            EFPApp.Activate(frm); // 07.06.2021
            return frm;
          }
        }
      }
      return null;
    }

    #endregion

    #region Выбор документов

    #region Выбор одного документа

    /// <summary>
    /// Выбор одного документа.
    /// Возвращает идентификатор документа или 0, если выбор не сделан.
    /// Используется диалог <see cref="DocSelectDialog"/>.
    /// </summary>
    /// <returns>Идентификатор документа или 0</returns>
    public Int32 SelectDoc()
    {
      return SelectDoc(String.Empty);
    }

    /// <summary>
    /// Выбор одного документа.
    /// Возвращает идентификатор документа или 0, если выбор не сделан.
    /// Используется диалог <see cref="DocSelectDialog"/>.
    /// </summary>
    /// <param name="title">Заголовок блока диалога</param>
    /// <returns>Идентификатор документа или 0</returns>
    public Int32 SelectDoc(string title)
    {
      Int32 docId = 0;
      if (SelectDoc(ref docId, title, false))
        return docId;
      else
        return 0;
    }

    /// <summary>
    /// Выбор документа из справочника.
    /// Пользователь должен выбрать документ или нажать кнопку "Отмена".
    /// Используется диалог <see cref="DocSelectDialog"/>.
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа.
    /// На выходе не может быть 0</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 docId)
    {
      return SelectDoc(ref docId, String.Empty, false);
    }

    /// <summary>
    /// Выбор документа из справочника.
    /// Используется диалог <see cref="DocSelectDialog"/>.
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="canBeEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 docId, string title, bool canBeEmpty)
    {
      return SelectDoc(ref docId, title, canBeEmpty, (EFPDBxGridFilters)null);
    }

    /// <summary>
    /// Выбор документа из справочника документов с использованием заданного набора фильтров просмотра,
    /// переопределяющего текущие установки пользователя. Пользователь может выбирать только подходящие
    /// документы, проходящие фильтр, т.к. не может его редактировать.
    /// Используйте класс <see cref="DocSelectDialog"/> для задания большего количества параметров.
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="canBeEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <param name="filters">Фиксированный набор фильтров. Значение null приводит к использованию текущего набора
    /// установленных пользователем фильтров</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 docId, string title, bool canBeEmpty, EFPDBxGridFilters filters)
    {
      DocSelectDialog dlg = new DocSelectDialog(this);
      dlg.DocId = docId;
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.CanBeEmpty = canBeEmpty;
      dlg.Filters = filters;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        docId = dlg.DocId;
        return true;
      }
      else
        return false;
    }


    #endregion

    #region Выбор одного документа из фиксированного списка

    /// <summary>
    /// Выбор документа из заданного множества документов.
    /// Испольщуйте класс <see cref="DocSelectDialog"/> для задания дополнительных параметров.
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="canBeEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <param name="fixedDocIds">Массив идентификаторов документов, из которого можно выбирать</param>
    public bool SelectDoc(ref Int32 docId, string title, bool canBeEmpty, IdCollection<Int32> fixedDocIds)
    {
      DocSelectDialog dlg = new DocSelectDialog(this);
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.CanBeEmpty = canBeEmpty;
      dlg.FixedDocIds = fixedDocIds;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        docId = dlg.DocId;
        return true;
      }
      else
        return false;
    }

    #endregion

    #region SelectDocs() Выбор нескольких документов без флажков

    /// <summary>
    /// Выбор одного или нескольких документов из справочника документов.
    /// Используются текущие настройки фильтров, которые пользователь может менять.
    /// </summary>
    /// <returns>Массив идентификаторов выбранных документов или пустой массив, если выбор не сделан</returns>
    public IIdSet<Int32> SelectDocs()
    {
      return SelectDocs(null, (EFPDBxGridFilters)null);
    }

    /// <summary>
    /// Выбор одного или нескольких документов из справочника документов.
    /// Используются текущие настройки фильтров, которые пользователь может менять.
    /// </summary>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <returns>Массив идентификаторов выбранных документов или пустой массив, если выбор не сделан</returns>
    public IIdSet<Int32> SelectDocs(string title)
    {
      return SelectDocs(title, (EFPDBxGridFilters)null);
    }

    /// <summary>
    /// Выбор одного или нескольких документов из справочника документов с использованием заданного набора фильтров просмотра,
    /// переопределяющего текущие установки пользователя. Пользователь может выбирать только подходящие
    /// документы, проходящие фильтр, т.к. не может его редактировать.
    /// Используйте класс <see cref="DocSelectDialog"/> для задания дополнительных параметров.
    /// </summary>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="filters">Фиксированный набор фильтров. Значение null приводит к использованию текущего набора
    /// установленных пользователем фильтров</param>
    /// <returns>Массив идентификаторов выбранных документов или пустой массив, если выбор не сделан</returns>
    public IIdSet<Int32> SelectDocs(string title, EFPDBxGridFilters filters)
    {
      // Переопределяется в GroupDocTypeUI

      DocSelectDialog dlg = new DocSelectDialog(this);
      dlg.SelectionMode = DocSelectionMode.MultiSelect;
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.Filters = filters;
      dlg.CanBeEmpty = false;
      if (dlg.ShowDialog() == DialogResult.OK)
        return dlg.DocIds;
      else
        return IdArray<Int32>.Empty;
    }

    #endregion

    #endregion

    #region Редактирование документов

    /// <summary>                                            
    /// Выполнение создания, редактирования, удаления или просмотра документа.
    /// </summary>
    /// <param name="editIds">Идентификаторы документов в режиме просмотра, редактирования или удаления</param>
    /// <param name="state">Требуемый режим</param>
    /// <param name="modal">True для запуска в модальном режиме</param>
    /// <param name="caller">Обработчик, связанный с табличным просмотром.
    /// Если он задан, то в режиме создания документа будут использованы установленные в просмотре
    /// фильтры для инициализации полей документа.</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (свойство <see cref="DocumentEditor.DataChanged"/>)</returns>
    public bool PerformEditing(IEnumerable<Int32> editIds, UIDataState state, bool modal, DocumentViewHandler caller)
    {
      IdArray<Int32> editIds2 = IdTools.AsIdArray<Int32>(editIds);

      switch (state)
      {
        case UIDataState.Insert:
          break;
        case UIDataState.InsertCopy:
          if (editIds2.Count != 1)
          {
            EFPApp.ShowTempMessage(Res.Common_Err_SingleDocRequired);
            return false;
          }
          break;

        case UIDataState.Delete: // 19.08.2016
          if (editIds2.Count < 1)
          {
            EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
            return false;
          }
          break;

        default:
          if (editIds2.Count < 1)
          {
            EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
            return false;
          }
          if (editIds2.Count > 1 && (!CanMultiEdit))
          {
            EFPApp.ShowTempMessage(String.Format(Res.Common_Err_NoMultiEdit, DocType.PluralTitle));
            return false;
          }
          break;
      }

      bool fixedResult = false;
      if (Editing != null)
      {
        DocTypeEditingEventArgs args = new DocTypeEditingEventArgs(this, state, editIds2, modal, caller);
        Editing(this, args);
        if (args.Handled)
          return args.HandledResult;
        else
          fixedResult = args.HandledResult;
      }

      // 21.11.2018
      // Активируем уже открытый редактор документов
      if (state != UIDataState.Insert && state != UIDataState.InsertCopy)
      {
        DBxDocSelection docSel = UI.CreateDocSelection(DocType.Name, editIds2);
        DocumentEditor oldDE = DocumentEditor.FindEditor(docSel);
        if (oldDE != null)
        {
          DBxDocSelection oldDocSel = oldDE.Documents.DocSelection;
          oldDocSel = new DBxDocSelection(oldDocSel, DocType.Name); // другие виды документов не интересны
          List<string> lst = new List<string>();
          lst.Add(Res.Editor_Err_AlreadyOpen);
          if (!oldDocSel.ContainsAll(docSel))
            lst.Add(Res.Editor_Err_AlreadyOpenPartial);
          else if (!docSel.ContainsAll(oldDocSel))
            lst.Add(Res.Editor_Err_AlreadyOpenExtra);

          if (EFPApp.ActiveDialog == null)
            EFPApp.Interface.CurrentChildForm = oldDE.Dialog.Form;
          else
          {
            lst.Add(Res.Editor_Err_AlreadyOpenCannotActivate);
          }

          EFPApp.MessageBox(String.Join(Environment.NewLine, lst.ToArray()), Res.Editor_ErrTitle_AlreadyOpen);
          return false;
        }
      }

      DocumentEditor de = new DocumentEditor(UI, DocType.Name, state, editIds2);
      de.Modal = modal;
      de.Caller = caller;
      de.Run();

      return de.DataChanged || fixedResult;
    }

    /// <summary>
    /// Выполнение создания, редактирования, удаления или просмотра документа.
    /// </summary>
    /// <param name="editIds">Идентификаторы документов в режиме просмотра, редактирования или удаления</param>
    /// <param name="state">Требуемый режим</param>
    /// <param name="modal">True для запуска в модальном режиме</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (свойство <see cref="DocumentEditor.DataChanged"/>)</returns>
    public bool PerformEditing(IEnumerable<Int32> editIds, UIDataState state, bool modal)
    {
      return PerformEditing(editIds, state, modal, null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра документа
    /// </summary>
    /// <param name="editId">Идентификатор редактируемого документа</param>
    /// <param name="readOnly">True, если требуется просмотр, false-редактирование</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (свойство <see cref="DocumentEditor.DataChanged"/>)</returns>
    public bool PerformEditing(Int32 editId, bool readOnly)
    {
      return PerformEditing(editId, readOnly, (DocumentViewHandler)null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра документа
    /// </summary>
    /// <param name="editId">Идентификатор редактируемого документа</param>
    /// <param name="readOnly">True, если требуется просмотр, false-редактирование</param>
    /// <param name="caller"></param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (свойство <see cref="DocumentEditor.DataChanged"/>)</returns>
    public bool PerformEditing(Int32 editId, bool readOnly, DocumentViewHandler caller)
    {
      if (editId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return false;
      }

      Int32[] editIds = new int[1];
      editIds[0] = editId;

      UIDataState state = readOnly ? UIDataState.View : UIDataState.Edit;
      return PerformEditing(editIds, state, true, caller);
    }


    /// <summary>
    /// Выполнение редактирования или просмотра одиночного документа.
    /// Если выборка содержит несколько документов данного типа, то показывается выборка с этими документами.
    /// Групповое редактирование, как в <see cref="PerformEditing(int, bool)"/> не применяется.
    /// Документы других типов, которые могут присутствовать в исходной выборке, не показываются.
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    /// <param name="readOnly">true - режим просмотра, а не редактирования.
    /// Игнорируется в режиме просмотра выборки документов</param>
    public void PerformEditingOrShowDocSel(DBxDocSelection docSel, bool readOnly)
    {
      PerformEditingOrShowDocSel(docSel, readOnly, (DocumentViewHandler)null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра одиночного документа.
    /// Если выборка содержит несколько документов данного типа, то показывается выборка с этими документами.
    /// Групповое редактирование, как в <see cref="PerformEditing(int, bool)"/> не применяется.
    /// Документы других типов, которые могут присутствовать в исходной выборке, не показываются.
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    /// <param name="readOnly">true - режим просмотра, а не редактирования.
    /// Игнорируется в режиме просмотра выборки документов</param>
    /// <param name="caller"></param>
    public void PerformEditingOrShowDocSel(DBxDocSelection docSel, bool readOnly, DocumentViewHandler caller)
    {
      if (docSel == null)
        throw new ArgumentNullException("docSel");
      PerformEditingOrShowDocSel(docSel[DocType.Name], readOnly, (DocumentViewHandler)null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра одиночного документа.
    /// Если массив содержит несколько идентификаторов документов, то показывается выборка с этими документами.
    /// Групповое редактирование, как в <see cref="PerformEditing(int, bool)"/> не применяется.
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <param name="readOnly">true - режим просмотра, а не редактирования.
    /// Игнорируется в режиме просмотра выборки документов</param>
    public void PerformEditingOrShowDocSel(IEnumerable<Int32> docIds, bool readOnly)
    {
      PerformEditingOrShowDocSel(docIds, readOnly, (DocumentViewHandler)null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра одиночного документа.
    /// Если массив содержит несколько идентификаторов документов, то показывается выборка с этими документами.
    /// Групповое редактирование, как в <see cref="PerformEditing(int, bool)"/> не применяется.
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <param name="readOnly">true - режим просмотра, а не редактирования.
    /// Игнорируется в режиме просмотра выборки документов</param>
    /// <param name="caller"></param>
    public void PerformEditingOrShowDocSel(IEnumerable<Int32> docIds, bool readOnly, DocumentViewHandler caller)
    {
      IIdSet<Int32> docIds2=IdTools.AsIdSet<Int32>(docIds);

      switch (docIds2.Count)
      {
        case 0:
          EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_NoSelectedDocs, DocType.PluralTitle));
          break;
        case 1:
          PerformEditing(docIds2.SingleId, readOnly, caller);
          break;
        default:
          // Показываем как выборку документов
          DBxDocSelection docSel2 = new DBxDocSelection(UI.DocProvider.DBIdentity, DocType.Name, docIds2);
          UI.ShowDocSel(docSel2, String.Format(Res.DocSel_Title_SelectedDocs, DocType.PluralTitle));
          break;
      }
    }

    /// <summary>
    /// Открывает окно информации о документе
    /// </summary>
    /// <param name="docId">Идентификатор документа</param>
    public void ShowDocInfo(Int32 docId)
    {
      if (docId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return;
      }
      try
      {
        DocInfoReport.PerformShow(this, docId);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_DocInfo));
      }
    }

    /// <summary>
    /// Инициализация начального значения поля "GroupId" для нового документа.
    /// Вызывается реализациями <see cref="DocumentViewHandler"/> для просмотров документов.
    /// Если текущий вид документов не имеет связанного дерева групп, метод не выполняет никаких действий.
    /// </summary>
    /// <param name="newDoc">Создаваемый документ до вывода на экран</param>
    /// <param name="auxFilterGroupIds">    
    /// Дополнительный фильтр по идентификатором групп документов.
    /// Значение null означает отсутствие фильтра ("Все документы").
    /// Массив нулевой длины означает фильтр "Документы без группы".
    ///</param>
    public void InitNewDocGroupIdValue(DBxSingleDoc newDoc, Int32[] auxFilterGroupIds)
    {
      if (this.GroupDocType == null)
        return; // нет дерева групп
      // Взято из EFPDocTreeView
      if (auxFilterGroupIds == null)
        return;

      int p = newDoc.Values.IndexOf(this.DocType.GroupRefColumnName);
      if (p < 0)
        return; // вообще-то это ошибка

      DBxExtValue value = newDoc.Values[p];

      if (auxFilterGroupIds.Length == 1)
      {
        // В режиме, когда в фильре выбрана единственная группа (то есть группа самого вложенного уровня
        // или включен флажок "Скрыть документы во вложенных группах"
        Int32 GroupId = auxFilterGroupIds[0];
        value.SetInt32(GroupId);
      }
      else
      {
        // 10.06.2019
        // В режиме фильтра, когда выбрано несколько групп, или выбран режим "Документы без групп" проверяем,
        // что текущая выбранная в документе группа есть в списке. Если выбрана группа не в фильтре, значение очищается
        Int32 CurrGroupId = value.AsInt32;
        if (CurrGroupId != 0 && Array.IndexOf<Int32>(auxFilterGroupIds, CurrGroupId) < 0)
          value.SetNull();
      }
    }

    #endregion

    #region События редактирования документов

    /// <summary>
    /// Вызывается при запуске редактирования до вывода окна релактора на экран
    /// и до создание объекта <see cref="DocumentEditor"/>.
    /// Обработчик может выполнить собственные действия, вместо редактирования
    /// с помощью стандартного редактора, установив свойство <see cref="DocTypeEditingEventArgs.Handled"/>=true.
    /// Обработчик не вызывается, если объект <see cref="DocumentEditor"/> создается и 
    /// запускается не с помощью метода <see cref="DocTypeUI.PerformEditing(int, bool)"/>
    /// </summary>
    public event DocTypeEditingEventHandler Editing;

    /// <summary>
    /// Возвращает true, если есть хотя бы один установленный обработчик
    /// <see cref="Editing"/>, <see cref="BeforeEdit"/> или <see cref="InitEditForm"/>, то есть возможность редактирования 
    /// была инициализирована.
    /// Если свойство возвращает false, то вызов <see cref="PerformEditing(int, bool)"/>.
    /// </summary>
    public bool HasEditorHandlers
    {
      get
      {
        return Editing != null || BeforeEdit != null || InitEditForm != null;
      }
    }

    /// <summary>
    /// Вызывается до создания окна редактирования. Может потребовать
    /// отказаться от редактирования, установив <see cref="BeforeDocEditEventArgs.Cancel"/>.
    /// </summary>
    public event BeforeDocEditEventHandler BeforeEdit;

    internal void DoBeforeEdit(DocumentEditor editor, out bool cancel, out bool showEditor)
    {
      BeforeDocEditEventArgs args = new BeforeDocEditEventArgs(editor);
      if (BeforeEdit != null)
        BeforeEdit(this, args);

      cancel = args.Cancel;
      showEditor = args.ShowEditor;
    }

    /// <summary>
    /// Вызывается при инициализации окна редактирования документа. 
    /// Должен создать закладки. 
    /// </summary>
    public event InitDocEditFormEventHandler InitEditForm;

    internal void PerformInitEditForm(DocumentEditor editor)
    {
      PerformInitEditForm(editor, editor.Documents[0]);
    }

    internal void PerformInitEditForm(DocumentEditor editor, DBxMultiDocs multiDocs)
    {
      if (multiDocs.DocType.Name != this.DocType.Name)
        throw ExceptionFactory.ArgProperty("multiDocs", multiDocs, "DocType.Name", multiDocs.DocType.Name, new object[] { this.DocType.Name });

      InitDocEditFormEventArgs args = new InitDocEditFormEventArgs(editor, multiDocs);
      if (InitEditForm != null)
        InitEditForm(this, args);
    }

    /// <summary>
    /// Вызывается из редактора документа перед записью значений в режимах
    /// Edit, Insert и InsertCopy. На момент вызова значения полей формы переписаны
    /// в поля <see cref="DocumentEditor.Documents"/>. Обработчик может скорректировать эти значения
    /// (например, не заданные поля).
    /// Также обработчик может отменить запись документов на сервере, установив
    /// <see cref="DocEditCancelEventArgs.Cancel"/>=true. При этом следует вывести сообщение об ошибке. Этот обработчик не может
    /// установить фокус ввода на "плохое" поле в редакторе, как обработчик <see cref="DocumentEditor.BeforeWrite"/>.
    /// </summary>
    public event DocEditCancelEventHandler Writing;

    internal bool DoWriting(DocumentEditor editor)
    {
      if (Writing != null)
      {
        DocEditCancelEventArgs args = new DocEditCancelEventArgs(editor);
        Writing(this, args);
        if (args.Cancel)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Вызывается после нажатия кнопки "ОК" или "Применить" в режиме
    /// копирования, создания или редактирования. Может вызываться
    /// многократно.
    /// </summary>
    public event DocEditEventHandler Wrote; // или Wriiten ???

    internal void DoWrote(DocumentEditor editor)
    {
      if (Wrote != null)
      {
        try
        {
          DocEditEventArgs args = new DocEditEventArgs(editor);
          Wrote(this, args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }
    }

    #endregion

    #region Пересчет вычисляемых полей

    /// <summary>
    /// Пересчет вычисляемых полей документов и поддокументов.
    /// Пересчет выполняется асинхронно.
    /// После пересчета полей никаких действий не выполняется. Используйте перегрузки с аргументом "AfterRecalc"
    /// </summary>
    /// <param name="docIds">Идентификаторы документов для пересчета.</param>
    public void RecalcColumns(IIdSet<Int32> docIds)
    {
      RecalcColumns(docIds, null, null);
    }

    /// <summary>
    /// Пересчет вычисляемых полей документов и поддокументов.
    /// Пересчет выполняется асинхронно.
    /// Эта версия предполагает, что пользовательский обработчик <paramref name="afterRecalc"/> не получает параметров.
    /// </summary>
    /// <param name="docIds">Идентификаторы документов для пересчета.</param>
    /// <param name="afterRecalc">Пользовательский метод вызывается, когда пересчет полей выполнен.
    /// Метод вызывается в основном потоке приложения</param>
    public void RecalcColumns(IIdSet<Int32> docIds, Delegate afterRecalc)
    {
      RecalcColumns(docIds, afterRecalc, null);
    }

    /// <summary>
    /// Пересчет вычисляемых полей документов и поддокументов.
    /// Пересчет выполняется асинхронно.
    /// </summary>
    /// <param name="docIds">Идентификаторы документов для пересчета.</param>
    /// <param name="afterRecalc">Пользовательский метод вызывается, когда пересчет полей выполнен.
    /// Метод вызывается в основном потоке приложения.</param>
    /// <param name="afterRecalcParams">Параметры, которые передаются пользовательскому методу <paramref name="afterRecalc"/>.
    /// Значение null задает пересчет всех существующих документов.</param>
    public void RecalcColumns(IIdSet<Int32> docIds, Delegate afterRecalc, params object[] afterRecalcParams)
    {
      NamedValues dispArgs = new NamedValues();
      dispArgs["Action"] = "RecalcColumns";
      dispArgs["DocTypeName"] = DocType.Name;
      dispArgs["DocIds"] = docIds;
      DistributedCallData startData = UI.DocProvider.StartServerExecProc(dispArgs);

      DistributedProcCallItem callItem = new DistributedProcCallItem(startData);
      callItem.DisplayName = String.Format(Res.Common_Phase_RecalcColumns, DocType.PluralTitle);
      callItem.UserData["AfterRecalc"] = afterRecalc;
      callItem.UserData["AfterRecalcParams"] = afterRecalcParams;
      callItem.Finished += new DistributedProcCallEventHandler(RecalcColumnsCallItem_Finished);
      EFPApp.ExecProcList.ExecuteAsync(callItem);
    }

    void RecalcColumnsCallItem_Finished(object sender, DistributedProcCallEventArgs args)
    {
      Delegate afterRecalc = (Delegate)(args.Item.UserData["AfterRecalc"]);
      object[] afterRecalcParams = (object[])(args.Item.UserData["AfterRecalcParams"]);
      if (afterRecalc != null)
        afterRecalc.DynamicInvoke(afterRecalcParams);
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Получение выборки документов текущего вида.
    /// В выборку сразу добавляются все переданные идентификаторы <paramref name="docIds"/>.
    /// Затем вызывается метод <see cref="DocTypeUIBase.OnGetDocSel(DBxDocSelection, DataRow[], EFPDBxViewDocSelReason)"/>для вызова обработчика события.
    /// Если для документа используются группы, то добавляются ссылки на документы вида <see cref="GroupDocType"/>.
    /// </summary>
    /// <param name="docSel">Выборка документов, обычно пустая, куда добавляются ссылки на документы</param>
    /// <param name="docIds">Массив идентификаторов документов. 
    /// Если null или пустой массив, никаких действий не выполняется</param>
    /// <param name="reason">Причина создания выборки</param>
    public override void PerformGetDocSel(DBxDocSelection docSel, IEnumerable<Int32> docIds, EFPDBxViewDocSelReason reason)
    {
      IIdSet<Int32> docIds2 = IdTools.AsIdSet<Int32>(docIds);
      if (docIds2.Count == 0)
        return;

      // Ссылки на выбранные документы
      docSel.Add(DocType.Name, docIds2);

      base.OnGetDocSel(docSel, docIds2, reason);

      // 18.11.2017 Ссылку на группу добавляем в конце выборки
      if (this.GroupDocType != null)
      {
        IdCollection<Int32> groupIds = new IdCollection<Int32>();
        foreach (Int32 docId in docIds2)
          groupIds.Add(TableCache.GetInt32(docId, DocType.GroupRefColumnName));
        docSel.Add(this.GroupDocType.DocType.Name, groupIds);
      }
    }

    /// <summary>
    /// Создать выборку для заданных документов.
    /// Массив идентификаторов может содержать нулевые и фиктивные идентификаторы, которые пропускаются.
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов. Фиктивные и нулевые идентификаторы игнорируются</param>
    /// <returns>Выборка документов одного вида</returns>
    public DBxDocSelection CreateDocSelection(IEnumerable<Int32> docIds)
    {
      IIdSet<Int32> docIds2 = IdTools.AsIdSet<Int32>(docIds);

      DBxDocSelection docSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      foreach (Int32 docId in docIds2)
      {
        if (!UI.DocProvider.IsRealDocId(docId))
          continue;
        docSel.Add(DocType.Name, docIds);
      }
      return docSel;
    }

    #endregion

    #region Таблицы ссылок

    /// <summary>
    /// Возвращает таблицу возможных ссылок на данный вид документов и его поддокументы
    /// </summary>
    public DBxDocTypeRefInfo[] ToDocTypeRefs
    {
      get
      {
        return UI.DocProvider.DocTypes.GetToDocTypeRefs(this.DocType.Name);
      }
    }

    /// <summary>
    /// Получить таблицу ссылок на документ
    /// </summary>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="showDeleted">true - включить в таблицу удаленные документы и ссылки на удаленные поддокументы</param>
    /// <param name="unique">Убрать повторные ссылки от одного и того жн документа</param>
    /// <returns>Таблица ссылок</returns>
    public DataTable GetDocRefTable(Int32 docId, bool showDeleted, bool unique)
    {
      return UI.DocProvider.GetDocRefTable(this.DocType.Name, docId, showDeleted, unique);
    }

    /// <summary>
    /// Получить таблицу ссылок на документ
    /// </summary>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="showDeleted">true - включить в таблицу удаленные документы и ссылки на удаленные поддокументы</param>
    /// <param name="unique">Убрать повторные ссылки от одного и того жн документа</param>
    /// <param name="fromSingleDocTypeName">Единственный вид документа, из которого берутся ссылки. Если не задано, то берутся ссылки из всех документов</param>
    /// <param name="fromSingleDocId">Идентификатор единственного документа, из которого берутся ссылки. Если 0, то берутся ссылки из всех документов</param>
    /// <returns>Таблица ссылок</returns>
    public DataTable GetDocRefTable(Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      return UI.DocProvider.GetDocRefTable(this.DocType.Name, docId, showDeleted, unique, fromSingleDocTypeName, fromSingleDocId);
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс вида документа, задающего группы
  /// </summary>
  public class GroupDocTypeUI : DocTypeUI
  {
    #region Защищенный конструктор

    internal GroupDocTypeUI(DBUI ui, DBxDocType docType)
      : base(ui, docType)
    {
      #region Инициализация полей

      if (String.IsNullOrEmpty(docType.TreeParentColumnName))
        throw ExceptionFactory.ArgProperty("docType", docType, "TreeParentColumnName", docType.TreeParentColumnName, null);

      for (int i = 0; i < docType.Struct.Columns.Count; i++)
      {
        DBxColumnStruct colDef = docType.Struct.Columns[i];
        if (colDef.ColumnType == DBxColumnType.String && (!colDef.Nullable))
        {
          _NameColumnName = colDef.ColumnName;
          break;
        }
      }
      if (String.IsNullOrEmpty(_NameColumnName))
        throw new ArgumentException(String.Format(Res.GroupDocTypeUI_Arg_NoTextColumn, docType.PluralTitle), "docType");

      this.Columns[_NameColumnName].NewMode = ColumnNewMode.AlwaysDefaultValue; // 10.06.2019 - название группы должно быть пустым

      #endregion
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя столбца, содержащего название группы
    /// </summary>
    public string NameColumnName { get { return _NameColumnName; } }
    private readonly string _NameColumnName;

    ///// <summary>
    ///// TreeParentColumnName
    ///// </summary>
    //public string ParentIdColumnName { get { return DocType.TreeParentColumnName; } }

    #endregion

    #region Выбор документов

    internal Int32 LastGroupId;

    internal bool LastIncludeNestedGroups;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает массив идентификаторов групп документов.
    /// Используется для фильтрации документов групп.
    /// Если выбраны "Все документы" (<paramref name="groupId"/>=0), возвращает null.
    /// Если выбраны "Документы без групп", возвращает массив нулевой длины.
    /// Если есть выбранная группа, возвращает массив из одного или нескольких элементов,
    /// в зависимости от <paramref name="includeNestedGroups"/>.
    /// </summary>
    /// <param name="groupId">Идентификатор узла группы. 0 задает "Все документы" или "Документы без групп"</param>
    /// <param name="includeNestedGroups">Признак "Включать вложенные группы"</param>
    public IdCollection<Int32> GetAuxFilterGroupIdList(Int32 groupId, bool includeNestedGroups)
    {
      if (groupId == 0)
      {
        if (includeNestedGroups)
          return null;
        else
          return IdCollection<Int32>.Empty;
      }
      else
      {
        if (includeNestedGroups)
        {
          DBxDocTreeModel model = new DBxDocTreeModel(UI.DocProvider,
            DocType,
            new DBxColumns(new string[] { "Id", DocType.TreeParentColumnName }));

          return new IdCollection<Int32>(model.GetIdWithChildren(groupId));
        }
        else
          return IdCollection<Int32>.FromId(groupId);
      }
    }

    #endregion
  }
}
