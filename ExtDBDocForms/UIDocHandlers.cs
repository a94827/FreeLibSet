using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.ExtDB;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using AgeyevAV.Config;

namespace AgeyevAV.ExtForms.Docs
{
  /// <summary>
  /// Обработчики документов на стороне клиента
  /// </summary>
  public class UIDocHandlers
  {
    #region Конструктор

    public UIDocHandlers(DBxDocProvider DocProvider)
    {
      if (DocProvider == null)
        throw new ArgumentNullException("DocProvider");
      FDocProvider = DocProvider;
      FDocTypes = new DocTypeList(this); // после инициализации FDocProvider 
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обработчик документов, полученный с сервера
    /// </summary>
    public DBxDocProvider DocProvider { get { return FDocProvider; } }
    private DBxDocProvider FDocProvider;

    #endregion

    #region Список видов документов

    public class DocTypeList
    {
      #region Защищенный конструктор

      internal DocTypeList(UIDocHandlers Owner)
      {
        FOwner = Owner;
        FItems = new Dictionary<string, UIDocTypeHandlers>(Owner.DocProvider.DocTypes.Count);
      }

      #endregion

      #region Свойства

      private UIDocHandlers FOwner;

      private Dictionary<string, UIDocTypeHandlers> FItems;

      public UIDocTypeHandlers this[string DocTypeName]
      {
        get
        {
          UIDocTypeHandlers Res;
          if (!FItems.TryGetValue(DocTypeName, out Res))
          {
            if (String.IsNullOrEmpty(DocTypeName))
              throw new ArgumentNullException("DocTypeName");
            if (!FOwner.DocProvider.DocTypes.Contains(DocTypeName))
              throw new ArgumentException("Неизвестный тип документов \"" + DocTypeName + "\"", "DocTypeName");

            Res = new UIDocTypeHandlers(FOwner, FOwner.DocProvider.DocTypes[DocTypeName]);
            FItems.Add(DocTypeName, Res);
          }
          return Res;
        }
      }

      #endregion
    }

    public DocTypeList DocTypes { get { return FDocTypes; } }
    private DocTypeList FDocTypes;

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Копирование выборки в буфер обмена
    /// </summary>
    /// <param name="DocSel"></param>
    public static void CopyDocSel(DBxDocSelection DocSel)
    {
      EFPApp.BeginWait("Копирование выборки документов в буфер обмена", "Copy");
      try
      {
        DataObject dobj = new DataObject();
        dobj.SetData(DocSel);
        Clipboard.SetDataObject(dobj);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Вставка выборки документов из буфера обмена
    /// Если буфер обмена не содержит выборки, возвращается null
    /// Выборка не нормализуется
    /// </summary>
    /// <returns></returns>
    public DBxDocSelection PasteDocSel()
    {
      DBxDocSelection DocSel = null;
      EFPApp.BeginWait("Вставка выборки документов из буфера обмена", "Paste");
      try
      {
        IDataObject dobj = Clipboard.GetDataObject();
        if (dobj != null)
          DocSel = (DBxDocSelection)(dobj.GetData(typeof(DBxDocSelection)));
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (DocSel == null)
        return null;

      if (DocSel.DBIdentity != DocProvider.DBIdentity)
      {
        EFPApp.ShowTempMessage("Выборка в буфере обмена относится к другой базе данных");
        return null;
      }

      return DocSel;
    }

    /// <summary>
    /// Вставка из буфера обмена единственного идентификатора документв заданного
    /// вида. Возвращает 0, если в буфере обмена нет таких документов. В этом случае
    /// выдаются соответствуюие сообщения пользователю
    /// </summary>
    /// <param name="DocTypeName"></param>
    /// <returns></returns>
    public Int32 PasteDocSelSingleId(string DocTypeName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(DocTypeName))
        throw new ArgumentNullException("DocTypeName");
#endif

      DBxDocType dt = DocTypes[DocTypeName].DocType;

#if DEBUG
      if (dt == null)
        throw new ArgumentException("Неизвестный вид документа", "DocTypeName");
#endif

      DBxDocSelection DocSel = PasteDocSel();
      if (DocSel == null)
      {
        EFPApp.ShowTempMessage("В буфере обмена нет выборки документов");
        return 0;
      }

      Int32 Id = DocSel.GetSingleId(DocTypeName);
      if (Id == 0)
        EFPApp.ShowTempMessage("В буфере обмена нет ссылки на документ \"" + dt.SingularTitle + "\"");
      return Id;
    }

#if XXX
    /// <summary>
    /// Вставка выборки документов из буфера обмена
    /// Если буфер обмена не содержит выборки, возвращается null
    /// Выборка не нормализуется
    /// </summary>
    /// <returns></returns>
    public static FilterClipboardInfo PasteFilter()
    {
      FilterClipboardInfo Info = null;
      EFPApp.BeginWait("Вставка фильтра из буфера обмена", "Paste");
      try
      {
        IDataObject dobj = Clipboard.GetDataObject();
        if (dobj != null)
          Info = (FilterClipboardInfo)(dobj.GetData(typeof(FilterClipboardInfo)));
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (Info.WorkAreaIdentity != WorkAreaIdentity)
      {
        EFPApp.ShowTempMessage("Табличные фильтры в буфере обмена относятся к другой базе данных");
        return null;
      }

      return Info;
    }
#endif

    #endregion
  }

  #region Делегат для работы с просмотром

  public class InitGridEventArgs : EventArgs
  {
    #region Конструктор

    public InitGridEventArgs(EFPDataGridViewWithIds ControlProvider)
    {
      FControlProvider = ControlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализируемый табличный просмотр
    /// </summary>
    public EFPDataGridViewWithIds ControlProvider { get { return FControlProvider; } }
    private EFPDataGridViewWithIds FControlProvider;

    /// <summary>
    /// Пользовательские данные
    /// </summary>
    public object Tag;

    #endregion
  }

  public delegate void InitGridEventHandler(object Sender, InitGridEventArgs Args);

  #endregion


  #region DocTypeEditingEventHandler

  public class DocTypeEditingEventArgs : EventArgs
  {
    #region Конструктор

    public DocTypeEditingEventArgs(UIDocTypeHandlers DocType, EFPDataGridViewState State, Int32[] EditIds, bool Modal, IDocumentEditorCaller Caller)
    {
      FDocType = DocType;
      FState = State;
      FEditIds = EditIds;
      FModal = Modal;
      FCaller = Caller;
      Handled = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализатор нового документа
    /// </summary>
    public IDocumentEditorCaller Caller { get { return FCaller; } }
    private IDocumentEditorCaller FCaller;

    /// <summary>
    /// Тип документа
    /// </summary>
    UIDocTypeHandlers DocType { get { return FDocType; } }
    private UIDocTypeHandlers FDocType;

    /// <summary>
    /// Режим работы
    /// </summary>
    public EFPDataGridViewState State { get { return FState; } }
    private EFPDataGridViewState FState;

    /// <summary>
    /// Список идентификаторов редактируемых, просматриваемых или
    /// удаляемых документов
    /// </summary>
    public Int32[] EditIds { get { return FEditIds; } }
    private Int32[] FEditIds;

    /// <summary>
    /// True, если окно редактирования следует показать в модальном
    /// режиме и false, если оно должно быть встроено в интерфейс MDI
    /// Поле может быть проигнорировано, если окно всегда выводится
    /// в модальном режиме
    /// </summary>
    public bool Modal { get { return FModal; } }
    private bool FModal;

    /// <summary>
    /// Должен быть установлен в true, чтобы предотвратить стандартный вызов редактора документа
    /// </summary>
    public bool Handled;

    /// <summary>
    /// Сюда может быть помещено значение, возвращаемое функкцией 
    /// ClientDocType.PerformEditing(), когда обработчик события Editing устанавливает
    /// Handled=true. Если обработчик оставляет Handled=false для показа формы, то он
    /// может установить HandledResult=true. В этом случае, метод PerformEditing()
    /// вернет true, даже если пользователь не будет редактировать документ
    /// До установки свойства в явном виде, оно имеет значение, совпадающее со
    /// свойством Handled
    /// </summary>
    public bool HandledResult;
    private bool? FHandledResult { get { return FHandledResult ?? Handled; } set { FHandledResult = value; } }

    // TODO: public CfgPart EditorConfigSection { get { return DocType.EditorConfigSection; } }

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает объект MultiDocs, загруженный данными или инициализированный
    /// начальными значениями (в режиме Insert).
    /// </summary>
    /// <returns></returns>
    public DBxDocSet CreateDocs()
    {
      DBxDocSet DocSet = new DBxDocSet(DocType.DocHandlers.DocProvider);
      DBxMultiDocs Docs = DocSet[DocType.DocType.Name];

      switch (State)
      {
        case EFPDataGridViewState.Edit:
          Docs.Edit(EditIds);
          break;
        case EFPDataGridViewState.Insert:
          Docs.Insert();
          if (Caller != null)
            Caller.InitNewDocValues(Docs[0]);
          break;
        case EFPDataGridViewState.InsertCopy:
          if (EditIds.Length != 1)
            throw new Exception("Должен быть задан единственный идентфикатор документа");
          Docs.InsertCopy(EditIds[0]);
          break;
        default:
          Docs.View(EditIds);
          break;
      }

      // TODO: DocSet.CheckDocs = true;
      return DocSet;
    }

    #endregion
  }

  public delegate void DocTypeEditingEventHandler(object Sender,
    DocTypeEditingEventArgs Args);

  #endregion

  #region DocEditEventHandler

  public class DocEditEventArgs : EventArgs
  {
    #region Конструктор

    public DocEditEventArgs(DocumentEditor Editor)
    {
      FEditor = Editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор основного документа
    /// </summary>
    public DocumentEditor Editor { get { return FEditor; } }
    private DocumentEditor FEditor;

    #endregion
  }

  public delegate void DocEditEventHandler(object Sender, DocEditEventArgs Args);

  public class DocEditCancelEventArgs : DocEditEventArgs
  {
    #region Конструктор

    public DocEditCancelEventArgs(DocumentEditor Editor)
      : base(Editor)
    {
      Cancel = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Это свойство можно установить в true, чтобы отменить действие
    /// </summary>
    public bool Cancel;

    #endregion
  }

  public delegate void DocEditCancelEventHandler(object Sender, DocEditCancelEventArgs Args);

  #endregion

  #region BeforeDocEditEventHandler

  public class BeforeDocEditEventArgs : DocEditEventArgs
  {
    #region Конструктор

    public BeforeDocEditEventArgs(DocumentEditor Editor)
      : base(Editor)
    {
      Cancel = false;
      ShowEditor = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Установка этого поля в true приводит к отказу от работы редактора 
    /// Запись результатов выполяться не будет
    /// </summary>
    public bool Cancel;

    /// <summary>
    /// Установка этого поля в false приводит к пропуску работы редактора
    /// Сразу выполняется запись результатов
    /// </summary>
    public bool ShowEditor;

    #endregion
  }

  public delegate void BeforeDocEditEventHandler(object Sender, BeforeDocEditEventArgs Args);

  #endregion


  /// <summary>
  /// Обработчики для одного вида документов
  /// </summary>
  public class UIDocTypeHandlers
  {
    #region Защищенный конструктор

    internal UIDocTypeHandlers(UIDocHandlers DocHandlers, DBxDocType DocType)
    {
      FDocHandlers = DocHandlers;
      FDocType = DocType;

      CanInsertCopy = false;
      //FDataBuffering = false;

      FGridProducer = new GridProducer();
      FGridProducer.FixedColumns.Add("Id");
      CanMultiEdit = false;
    }

    #endregion

    #region Свойства

    public UIDocHandlers DocHandlers { get { return FDocHandlers; } }
    private UIDocHandlers FDocHandlers;

    public DBxDocType DocType { get { return FDocType; } }
    private DBxDocType FDocType;

    public GridProducer GridProducer { get { return FGridProducer; } }
    private GridProducer FGridProducer;

    /// <summary>
    /// True, если допускается одновременное редактирование или
    /// просмотр нескольких выбранных документов. По умолчанию -
    /// false (нельзя)
    /// </summary>
    public bool CanMultiEdit;

    /// <summary>
    /// Изображение, асоциированное с документом данного вида
    /// </summary>
    /// <summary>
    /// true, если разрешено создание нового документа на основании существующего
    /// </summary>
    public bool CanInsertCopy;


    #endregion

    #region Значок документа

    /// <summary>
    /// Значок для одного документа.
    /// По умолчанию - null
    /// </summary>
    public string ImageKey
    {
      get { return FImageKey; }
      set { FImageKey = value; }
    }
    private string FImageKey;

    public string SingleDocImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(FImageKey))
          return "Item";
        else
          return FImageKey;
      }
    }

    public string TableImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(FImageKey))
          return "Table";
        else
          return FImageKey;
      }
    }


    public Image GetImageValue(int DocId)
    {
      return EFPApp.MainImages.Images[GetImageKeyValue(DocId)];
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора документа
    /// </summary>
    /// <param name="DocId"></param>
    /// <returns></returns>
    public string GetImageKeyValue(Int32 Id)
    {
      if (Id == 0)
        return null;

      return SingleDocImageKey;
      // TODO:
      /*
      try
      {
        ReadyGetImageValueNames();
        object[] Values = GetValues(Id, GetImageValueNames);
        if (DataTools.GetBool(Values[0])) // Deleted=true
          DocTypeGetImageEventArgs.TheArgsForImage.ImageKey = "Cancel";
        else
        {
          DocTypeGetImageEventArgs.TheArgsForImage.ImageKey = null;
          if (GetImage != null)
          {
            if (GetImageValueDocValues == null)
              GetImageValueDocValues = new MemoryDocValues(GetImageValueNames);
            GetImageValueDocValues.Values = Values;
            DocTypeGetImageEventArgs.TheArgsForImage.FValues = GetImageValueDocValues;
            DocTypeGetImageEventArgs.TheArgsForImage.ImageKey = null;
            GetImage(this, DocTypeGetImageEventArgs.TheArgsForImage);
          }
          if (String.IsNullOrEmpty(DocTypeGetImageEventArgs.TheArgsForImage.ImageKey))
          {
            if (String.IsNullOrEmpty(ImageKey))
              DocTypeGetImageEventArgs.TheArgsForImage.ImageKey = "Item";
            else
              DocTypeGetImageEventArgs.TheArgsForImage.ImageKey = ImageKey;
          }
        }
        return DocTypeGetImageEventArgs.TheArgsForImage.ImageKey;
      }
      catch
      {
        return "Error";
      }
       * */
    }

    public string GetToolTipText(Int32 DocId)
    {              
      // TODO:
      return "???";
    }

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
        return FDataBuffering;
      }
      set
      {
        FDataBuffering = value;
        FBufferedData = null;
      }
    }
    private bool FDataBuffering;

    /// <summary>
    /// Обобщенный метод получения буферизованных или небуферизованных данных.
    /// Буферизация используется, если свойство DataBuffering установлено в true
    /// </summary>
    /// <param name="Fields">Требуемые поля</param>
    /// <param name="Filter">Условия фильтрации строк или null</param>
    /// <param name="ShowDeleted">true, если надо показать удаленные строки</param>
    /// <param name="OrderBy">Порядок сортировки строк</param>
    /// <returns>Объект DataView, который можно использовать для табличного просмотра или перебора строк</returns>
    public DataView GetDataView(DBxColumns Columns, DBxFilter Filter, bool ShowDeleted, DBxOrder OrderBy)
    {
      DataView dv;

      DBxColumnList ColLst = new DBxColumnList(Columns);
      if (DataBuffering)
      {
        if (OrderBy != null)
        {
          // Требуется, чтобы поле сортировки присутствовало в выборке
          OrderBy.GetColumnNames(ColLst);
        }
        DataTable dt = GetBufferedData(new DBxColumns(ColLst));
        if (!ShowDeleted)
        {
          DBxFilter Filter2 = DBxDocProvider.DeletedFalseFilter;
          if (Filter == null)
            Filter = Filter2;
          else
            Filter = new AndFilter(Filter, Filter2);
        }
        dv = new DataView(dt);
        if (Filter == null)
          dv.RowFilter = String.Empty;
        else
          dv.RowFilter = Filter.ToString();

        if (OrderBy != null)
          dv.Sort = OrderBy.ToString();
      }
      else
      {
        DataTable dt = GetUnbufferedData(Columns, Filter, ShowDeleted, OrderBy);
        dv = dt.DefaultView;
      }
      return dv;
    }

    public DataView GetDataView(DBxColumns Columns)
    {
      return GetDataView(Columns, null, false, null);
    }

    /// <summary>
    /// Получить просмотр для фиксированного набора строк
    /// </summary>
    /// <param name="Fields">Требуемые поля</param>
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
        dt = DocHandlers.DocProvider.FillSelect(DocType.Name, Columns, new IdsFilter(Ids));

      DataView dv = new DataView(dt);
      dv.Sort = String.Empty;
      return dv;
    }

    /// <summary>
    /// Получение документов с сервера без использования буферизации, независимо от
    /// свойства DataBuffering. Выполняет непосредственное обращение к серверу
    /// </summary>
    /// <param name="Fields">Требуемые поля</param>
    /// <param name="Filter">Условия фильтрации строк или null</param>
    /// <param name="ShowDeleted">true, если надо показать удаленные строки</param>
    /// <param name="OrderBy">Порядок сортировки строк</param>
    /// <returns>Таблица DataTable</returns>
    public DataTable GetUnbufferedData(DBxColumns Columns, DBxFilter Filter, bool ShowDeleted, DBxOrder OrderBy)
    {
      if (!ShowDeleted)
      {
        DBxFilter Filter2 = DBxDocProvider.DeletedFalseFilter;
        if (Filter == null)
          Filter = Filter2;
        else
          Filter = new AndFilter(Filter, Filter2);
      }
      return DocHandlers.DocProvider.FillSelect(DocType.Name, Columns, Filter, OrderBy);
    }

    /// <summary>
    /// Получение доступа к буферизованным данным
    /// </summary>
    private DataTable GetBufferedData(DBxColumns Columns)
    {
      if (!FDataBuffering)
        return null;

      if (Columns == null)
        throw new ArgumentNullException("Columns");

      DBxColumnList ColLst = new DBxColumnList(Columns);
      ColLst.Add("Id");
      ColLst.Add("Deleted");

      if (FBufferedData != null)
      {
        // Проверяем, все ли поля есть
        if (ColLst.HasMoreThan(FBufferedColumns))
        {
          // Некоторых полей не хватает
          FBufferedData = null;
          ColLst.Add(FBufferedColumns);
        }
      }

      if (FBufferedData == null)
      {
        FBufferedData = DocHandlers.DocProvider.FillSelect(DocType.Name, new DBxColumns(ColLst), null);
        DataTools.CheckPrimaryKey(FBufferedData, "Id");
        FBufferedColumns = Columns;
      }
      return FBufferedData;
    }

    /// <summary>
    /// Существующие буферизованные данные (если загружены)
    /// </summary>
    internal DataTable FBufferedData;

    /// <summary>
    /// Список полей, которые содержаться в FBufferedData
    /// </summary>
    private DBxColumns FBufferedColumns;

    /// <summary>
    /// Очистка буферизованных данных. При следующем обращении к
    /// BufferedData данные будут снова загружены с сервера
    /// </summary>
    public void RefreshBufferedData()
    {
      FBufferedData = null;
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

#if XXX
    /// <summary>
    /// Получение нескольких значений поля для документа. Буферизация
    /// используется, если она разрешена, иначе выполняется обращение 
    /// к серверу
    /// </summary>
    /// <param name="RefId">Идентификатор документа</param>
    /// <param name="FieldNames">Массив имен полей</param>
    /// <returns>Массив объектов, содержащих значения</returns>
    public object[] GetValues(int Id, string[] FieldNames)
    {
      if (DataBuffering)
        return AccDepClientExec.BufTables.GetValues(Name, Id, FieldNames);
      else
        // Без буферизации выполняем обращение к серверу
        return AccDepClientExec.GetValues(Name, Id, FieldNames);
    }

    public object[] GetValues(Int32 Id, string FieldNames)
    {
      return GetValues(Id, FieldNames.Split(new char[] { ',' }));
    }

    public object GetValue(Int32 Id, string FieldName)
    {
      string[] FieldNames = new string[1];
      FieldNames[0] = FieldName;
      object[] res = GetValues(Id, FieldNames);
      return res[0];
    }
#endif

    #endregion

    #region Инициализация табличного просмотра

    #region Основной метод

    /// <summary>
    /// Вызывается при инициализации таблицы просмотра документов
    /// для добавления столбцов. Если обработчик не установлен, выполняется
    /// инициализация по умолчанию
    /// </summary>
    public event InitGridEventHandler InitGrid;

    /// <summary>
    /// Инициализация табличного просмотра документов
    /// </summary>
    /// <param name="DocGridHandler">Обработчик табличного просмотра</param>
    /// <param name="ReInit">true при повторном вызове метода (после изменения конфигурации просмотра)
    /// и false при первом вызове</param>
    /// <param name="Fields">Сюда помещается список имен полей, которые требуются для текущей конфигурации просмотра</param>
    /// <param name="InitGridData">Свойство Args.Tag, передаваемое обработчику InitGrid (если он установлен)</param>
    public void DoInitGrid(EFPDataGridViewWithIds ControlProvider, bool ReInit, DBxColumnList Columns, object InitGridTag)
    {
      if (!ReInit)
      {
        if (CanMultiEdit)
          ControlProvider.CanMultiEdit = true;
        if (CanInsertCopy)
          ControlProvider.CanInsertCopy = true;
      }
      else
        ControlProvider.Control.Columns.Clear();

      // TODO: GridProducer.InitGrid(ControlProvider, ReInit, ControlProvider.CurrentGridConfig, out Columns);
      GridProducer.InitGrid(ControlProvider, ReInit, null, Columns);

      if (!ReInit)
      {

#if XXX
        // Обработчик для печати (д.б. до вызова пользовательского блока инициализации)
        ControlProvider.AddGridPrint().DocumentName = DocType.PluralTitle;
        for (int i = 0; i < PrintTypes.Count; i++)
        {
          AccDepGridPrintDoc pd;
          pd = new AccDepGridPrintDoc(PrintTypes[i], ControlProvider, this, false);
          ControlProvider.PrintList.Add(pd);
          // 24.11.2011 Не нужно. Есть печать из "Сервис" - "Бланки"
          // pd = new AccDepGridPrintDoc(PrintTypes[i], GridHandler, this, true);
          // GridHandler.PrintList.Add(pd);
        }
#endif

        if (InitGrid != null)
        {
          InitGridEventArgs Args = new InitGridEventArgs(ControlProvider);
          Args.Tag = InitGridTag;
          InitGrid(this, Args);
        }
      }
      // Добавляем столбец с картинками
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn ImgCol = new DataGridViewImageColumn();
        ImgCol.Name = "Image";
        ImgCol.HeaderText = String.Empty;
        ImgCol.ToolTipText = "Значок документа \"" + DocType.SingularTitle + "\"";
        ImgCol.Width = ControlProvider.Measures.ImageColumnWidth;
        ImgCol.Resizable = DataGridViewTriState.False;
        string ImgName = ImageKey;
        if (String.IsNullOrEmpty(ImgName))
          ImgName = "Item";
        ImgCol.Image = EFPApp.MainImages.Images[ImgName];
        if (ControlProvider.Control.Columns.Count > 0 && ControlProvider.Control.Columns[0].Frozen)
          ImgCol.Frozen = true;
        ControlProvider.Control.Columns.Insert(0, ImgCol);
        if (EFPApp.ShowToolTips)
          ControlProvider.Columns[0].CellToopTextNeeded += new EFPDataGridViewCellToolTipTextNeededEventHandler(ImgCol_CellToopTextNeeded);
      }

      if (!ReInit)
      {
        //int StartColumnIndex = DocGridHandler.FirstIncSearchColumnIndex;
        //if (StartColumnIndex < 0)
        //  StartColumnIndex = 1; // чтобы активным был не столбец с картинкой
        //DocGridHandler.CurrentColumnIndex = StartColumnIndex;
        ControlProvider.CurrentColumnIndex++;

      }
      Columns.Add("Id");
      Columns.Add("Deleted"); //,CheckState,CheckTime";


      //AccDepClientExec.AddGridDebugIdColumn(DocGridHandler.MainGrid);
#if XXX
      if (AccDepClientExec.DebugShowIds)
      {
        //Columns += "CreateTime,ChangeTime";
        ControlProvider.Columns.AddInt("Id");
        ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        ControlProvider.Columns.LastAdded.CanIncSearch = true;
        if (UseHieView)
        {
          ControlProvider.Columns.AddInt("GroupId");
          ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        ControlProvider.Columns.AddInt("ImportId");
        ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

        ControlProvider.AutoSort = true;
        if (ControlProvider.Orders.Count == 0)
        {
          if ((!String.IsNullOrEmpty(DefaultOrder)) && DefaultOrder != "Id")
          {
            GridHandlerDataOrder MainOrder = ControlProvider.Orders.Add(DefaultOrder, "Основной порядок");
            Columns += MainOrder.Order.GetFieldNames();
          }
        }
        if (ControlProvider.Orders.IndexOfFirstColumnName("Id") < 0)
          ControlProvider.Orders.Add("Id", "По идентификатору Id");

        DataOrder ChOrder1 = new DataOrder(new DataOrderFieldIfNull("ChangeTime", typeof(DateTime), new DataOrderField("CreateTime")), false);
        DataOrder ChOrder2 = new DataOrder(new DataOrderFieldIfNull("ChangeTime", typeof(DateTime), new DataOrderField("CreateTime")), true);
        ControlProvider.Orders.Add(ChOrder1, "По времени изменения (новые внизу)", new EFPDataGridViewSortInfo("ChangeTime", false));
        ControlProvider.Orders.Add(ChOrder2, "По времени изменения (новые сверху)", new EFPDataGridViewSortInfo("ChangeTime", true));
      }
#endif

      // TODO:
      //if (EFPApp.ShowListImages)
      //  Columns = Columns + ColumnsForImage;
      //Columns = Columns + FieldsForColors;
      //Columns = Columns + ControlProvider.GetUsedFields();
      //DataFields CondFields = AccDepClientExec.AllDocConditions.GetFields(Name);
      //Columns += CondFields;

      if (!ReInit)
      {
        ControlProvider.Control.VirtualMode = true;
        ControlProvider.UseRowImages = true;
        ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributes);
        //if (CondFields != null)
        //  ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributesForCond);
        if (EFPApp.ShowListImages)
          ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(GridHandler_GetCellAttributes);
        //DocGridHandler.MainGrid.CellPainting += new DataGridViewCellPaintingEventHandler(TheGrid_CellPainting);
        //DocGridHandler.MainGrid.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(TheGrid_CellToolTipTextNeeded);

        // Добавляем команды локального меню
        InitGridCommandItems(ControlProvider);

        // TODO: 
        // ControlProvider.CurrentCfgModified = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
//      if (ReInit)
//        FToolTipExtractor = null;
    }

    #endregion

    #region Оформление просмотра

    void GridHandler_GetRowAttributes(object Sender, EFPDataGridViewRowAttributesEventArgs Args)
    {
#if XXX
      DataRow Row = Args.DataRow;
      //if (Row.RowState == DataRowState.Deleted)
      //{
      //  Args.Grayed = true;
      //  return;
      //}
      if (Row == null)
        return;
      if (DataTools.GetBool(Row, "Deleted"))
        Args.Grayed = true;

      int CheckState = DataTools.GetInt(Row, "CheckState");
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
            throw new Exception("Неизвестное значение CheckState=" + CheckState.ToString());
          break;
      }
      if (Args.Reason == GridAttributesReason.ToolTip)
      {
        if (CheckState != DocumentCheckState.Unchecked)
        {
          DateTime CheckTime = DataTools.GetDateTime(Row, "CheckTime");
          TimeSpan Span = DateTime.Now - CheckTime;
          Args.ToolTipText += "\r\nПроверка выполнена " + DataConv.TimeSpanToStr(Span);
        }
      }
#endif
    }

    void GridHandler_GetRowAttributesForCond(object Sender, EFPDataGridViewRowAttributesEventArgs Args)
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
        Args.ToolTipText += "\r\n" + (Mode == AccDepAccessMode.ReadOnly ? "Разрешен только просмотр" : "Доступ запрещен") + ". " + Why;
      Args.Grayed = true;
#endif
    }

    void GridHandler_GetCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
    {
#if XXX
      if (Args.ColumnName == "Image")
      {
        DataRow Row = Args.DataRow;
        if (Row == null)
          return;
        try
        {
          Image Image = EFPApp.MainImages.Images[GetImageKeyNoDefault(Row)];
          if (Image == null)
          {
            if (String.IsNullOrEmpty(ImageKey))
              Args.Value = EFPApp.MainImages.Images["Item"];
            else
              Args.Value = EFPApp.MainImages.Images[ImageKey];
          }
          else
            Args.Value = Image;
        }
        catch
        {
        }
      }
#endif
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
      int CheckState = DataTools.GetInt(Row, "CheckState");
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
            throw new Exception("Неизвестное значение CheckState=" + CheckState.ToString());
        }
        DateTime CheckTime = DataTools.GetDateTime(Row, "CheckTime");
        TimeSpan Span = DateTime.Now - CheckTime;
        Args.ToolTipText += "\r\nПроверка выполнена " + DataConv.TimeSpanToStr(Span);
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
      int CheckState = DataTools.GetInt(Row, "CheckState");
      Image img = null;
      switch (CheckState)
      {
        case 0:
          img = ClientApp.MainImages.Images["UnknownState"];
          break;
        case 2:
          img = ClientApp.MainImages.Images["Warning"];
          break;
        case 3:
          img = ClientApp.MainImages.Images["Error"];
          break;
        case 4:
          img = ClientApp.MainImages.Images["FatalError"];
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
    void ImgCol_CellToopTextNeeded(object Sender, EFPDataGridViewCellToolTipTextNeededEventArgs Args)
    {
      Int32 Id = DataTools.GetInt(Args.Row, "Id");
      // TODO: Args.ToolTipText = GetImageCellToolTipText(Id);
    }

    #endregion

    #region Команды локального меню

    private void InitGridCommandItems(EFPDataGridViewWithIds ControlProvider)
    {
#if XXX
      EFPCommandItem ciCheckDocuments = new EFPCommandItem("Вид", "ПроверитьДокументы");
      ciCheckDocuments.MenuText = "Проверить выделенные документы";
      ciCheckDocuments.ToolTipText = "Проверить выделенные документы";
      ciCheckDocuments.ImageKey = "ПроверитьДокумент";
      ciCheckDocuments.Tag = ControlProvider;
      ciCheckDocuments.Click += new EventHandler(ciCheckDocuments_Click);
      ControlProvider.CommandItems.Add(ciCheckDocuments);

      EFPCommandItem MenuRecalcDocuments = new EFPCommandItem("Сервис", "ПересчитатьДокументы");
      MenuRecalcDocuments.MenuText = "Пересчитать вычисляемые поля";
      MenuRecalcDocuments.Usage = EFPCommandItemUsage.Menu;
      //TODO: MenuRecalcDocuments.Enabled = HasCalcFields;
      ControlProvider.CommandItems.Add(MenuRecalcDocuments);

      EFPCommandItem ci = new EFPCommandItem("Сервис", "ПересчитатьВыбранныеДокументы");
      ci.MenuText = "Выделенные";
      ci.Parent = MenuRecalcDocuments;
      ci.Tag = ControlProvider;
      ci.Click += new EventHandler(RecalcSelDocuments_Click);
      ControlProvider.CommandItems.Add(ci);

      ci = new EFPCommandItem("Сервис", "ПересчитатьВидимыеДокументы");
      ci.MenuText = "Все в просмотре";
      ci.Parent = MenuRecalcDocuments;
      ci.Tag = ControlProvider;
      ci.Click += new EventHandler(RecalcViewDocuments_Click);
      ControlProvider.CommandItems.Add(ci);

      ci = new EFPCommandItem("Сервис", "ПересчитатьВсеДокументы");
      ci.MenuText = "Все существующие";
      ci.Parent = MenuRecalcDocuments;
      ci.Tag = ControlProvider;
      ci.Click += new EventHandler(RecalcAllDocuments_Click);
      ControlProvider.CommandItems.Add(ci);

      //TODO: AddShowDocumentInfoItem(ControlProvider);

      ci = new EFPCommandItem("Сервис", "ПросмотрСсылок");
      ci.MenuText = "Ссылки на документ";
      ci.ImageKey = "ПросмотрСсылок";
      ci.Usage = EFPCommandItemUsage.Menu;
      ci.Tag = ControlProvider;
      ci.Click += new EventHandler(ShowDocRefs_Click);
      ControlProvider.CommandItems.Add(ci);

#if XXX
      ControlProvider.CommandItems.InitGoto += new InitGotoItemsEventHandler(DocGrid_InitGoto);

      if (this.WholeRefBook != null)
        this.WholeRefBook.InitCommandItems(ControlProvider);
#endif

      ControlProvider.Control.RowHeaderMouseDoubleClick += new DataGridViewCellMouseEventHandler(Grid_RowHeaderMouseDoubleClick);

#if XXX
      AccDepFileType FileType = AccDepFileType.CreateDataSetXML();
      FileType.SaveCode = FileType.OpenCode + "Export";
      FileType.ImageKey = "XMLDataSet";
      FileType.DisplayName = "Данные для экспорта (XML)";
      FileType.Save += new AccDepFileEventHandler(SaveDataSet);
      FileType.Tag = ControlProvider;
      ControlProvider.CommandItems.SaveTypes.Add(FileType);

      ControlProvider.CommandItems.GetDocSel += new GridHandlerDocSelEventHandler(DocGrid_GetDocSel);
#endif
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

    void RecalcSelDocuments_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(ci.Tag);

      int[] DocIds = GridHandler.SelectedIds;
      DocRecalcExec.RecalcIds(this, DocIds);
      GridHandler.PerformRefresh();
    }

    void RecalcViewDocuments_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(ci.Tag);
      if (GridHandler.SourceAsDataView == null)
      {
        EFPApp.ShowTempMessage("Набор данных не присоединен к табличному просмотру");
        return;
      }

      Int32[] DocIds = DataTools.GetIds(GridHandler.SourceAsDataView);
      DocRecalcExec.RecalcIds(this, DocIds);
      GridHandler.PerformRefresh();
    }

    void RecalcAllDocuments_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(ci.Tag);

      DocRecalcExec.RecalcAll(this);
      GridHandler.PerformRefresh();
    }

    void ShowDocRefs_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPAccDepIdGrid GridHandler = (EFPAccDepIdGrid)(ci.Tag);

      RowRefViewer.PerformView(Name, GridHandler.CurrentId);
    }

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
    /*
    void DocGrid_GetDocSel(object Sender, GridHandlerDocSelEventArgs Args)
    {
      Int32[] Ids = DataTools.GetIds(Args.DataRows);
      PerformGetDocSel(Args.DocSel, Ids, Args.Reason);
    }
  */
    /*
    void SaveDataSet(object Sender, AccDepFileEventArgs Args)
    {
      AccDepFileType FileType = (AccDepFileType)Sender;
      EFPAccDepIdGrid DocGridHandler = (EFPAccDepIdGrid)(FileType.Tag);

      bool AllRows = Args.Config.GetBool("ВсеСтроки");
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
      Args.Config.SetBool("ВсеСтроки", AllRows);

      Int32[] DocIds;
      if (AllRows)
        DocIds = DataTools.GetIds(DocGridHandler.SourceAsDataView);
      else
        DocIds = DocGridHandler.SelectedIds;
      DataSet ds = AccDepClientExec.CreateExportDataSet(Name, DocIds);
      AccDepClientExec.SaveExportDataSet(Args.FileName.Path, ds);
    }
     * */

    void Grid_RowHeaderMouseDoubleClick(object Sender, DataGridViewCellMouseEventArgs Args)
    {
      DataGridView Grid = (DataGridView)Sender;
      if (Args.RowIndex >= 0)
      {
        DataRow Row = EFPDataGridView.GetDataRow(Grid, Args.RowIndex);
        int DocId = DataTools.GetInt(Row, "Id");
        if (DocId == 0)
        {
          EFPApp.ShowTempMessage("Документ не выбран");
          return;
        }
        // TODO: DocInfoReport.ShowInfo(this, DocId);
      }
    }

    #endregion

    #endregion

    #region Создание команд меню

    public EFPCommandItem CreateMainMenuItem(EFPCommandItem Parent)
    {
      EFPCommandItem ci= new EFPCommandItem("Документы", DocType.Name);
      ci.Parent = Parent;
      ci.MenuText = DocType.PluralTitle;
      ci.ImageKey = ImageKey;
      ci.Click += new EventHandler(CommandItem_Click);
      EFPApp.CommandItems.Add(ci);
      return ci;
    }

    void CommandItem_Click(object Sender, EventArgs Args)
    {
      ShowOrOpen(null, 0, "Menu");
    }

    #endregion

    #region Окно просмотра документов

    public DocTableViewForm ShowOrOpen(InitGridEventHandler InitFilters, Int32 DocId, string FormSearchKey)
    {
      if (FormSearchKey == null)
        FormSearchKey = String.Empty;

      DocTableViewForm Form = FindAndActivate(FormSearchKey);
      if (Form == null)
      {
        Form = new DocTableViewForm(this, DocTableViewMode.Browse);
        Form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        Form.FormSearchKey = FormSearchKey;
        EFPApp.ShowMdiChild(Form);
      }
      return Form;
    }

    private DocTableViewForm FindAndActivate(string FormSearchKey)
    {
      Form[] Forms = EFPApp.MdiChildren;
      for (int i = 0; i < Forms.Length; i++)
      {
        if (Forms[i] is DocTableViewForm)
        {
          DocTableViewForm frm = (DocTableViewForm)(Forms[i]);
          if (frm.DocTypeName!=this.DocType.Name)
            continue;
          if (!String.IsNullOrEmpty(FormSearchKey))
          {
            if (frm.FormSearchKey != FormSearchKey)
              continue;
          }

          frm.BringToFront();
          return frm;
        }
      }
      return null;
    }

    #endregion

    #region Выбор документов

    /// <summary>
    /// Выбор одного документа
    /// Возвращает идентификатор документа или 0, если выбор не сделан
    /// </summary>
    /// <returns></returns>
    public Int32 SelectDoc()
    {
      return SelectDoc(String.Empty);
    }

    public Int32 SelectDoc(string Title)
    {
      int DocId = 0;
      if (SelectDoc(ref DocId, Title, false))
        return DocId;
      else
        return 0;
    }

    public bool SelectDoc(ref Int32 DocId)
    {
      return SelectDoc(ref DocId, String.Empty, false);
    }

    public bool SelectDoc(ref Int32 DocId, string Title, bool CanEmpty)
    {
      return SelectDoc(ref DocId, Title, CanEmpty, (GridFilters)null);
    }

    /// <summary>
    /// Выбор документа из справочника документов с использованием заданного набора фильтров просмотра,
    /// переопределяющего текущие установки пользователя. Пользователь может выбирать только подходящие
    /// документы, проходящие фильтр, т.к. не может его редактировать
    /// </summary>
    /// <param name="DocTypeName">Тип документа</param>
    /// <param name="DocId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="Title">Заголовок формы выбора документа</param>
    /// <param name="CanEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <param name="Filters">Фиксированный набор фильтров. Значение null приводит к использованию текущего набора
    /// установленных пользователем фильтров</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 DocId, string Title, bool CanEmpty, GridFilters Filters)
    {
      return SelectDoc(ref DocId, Title, CanEmpty, Filters, null);
    }

    /// <summary>
    /// Выбор документа из справочника документов с использованием заданного набора фильтров просмотра,
    /// переопределяющего текущие установки пользователя. Пользователь может выбирать только подходящие
    /// документы, проходящие фильтр, т.к. не может его редактировать.
    /// Эта версия метода позволяет задать внешний объект, который определяет начальные значения полей при 
    /// создании нового документа из просмотра. При этом GridFilters не используется для задания начальных 
    /// значений. Эта версия нужна, когда выбор допускается из большого множества строк, но есть 
    /// предпочтительные значения для документа
    /// </summary>
    /// <param name="DocTypeName">Тип документа</param>
    /// <param name="DocId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="Title">Заголовок формы выбора документа</param>
    /// <param name="CanEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <param name="Filters">Фиксированный набор фильтров. Значение null приводит к использованию текущего набора
    /// установленных пользователем фильтров</param>
    /// <param name="ExternalEditorCaller">Внешний объект, определяющий начальные значения при создании документа.
    /// Если задан, то должен определять все значения, чтобы документ прошел условия фильтров <paramref name="Filters"/> (если заданы),
    /// т.к. сами фильтры не используются для установки значений</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 DocId, string Title, bool CanEmpty, GridFilters Filters, IDocumentEditorCaller ExternalEditorCaller)
    {
      DocTableViewForm Form = new DocTableViewForm(this, DocTableViewMode.SelectDoc);
      bool Res = false;
      try
      {
        Form.Text = Title;
        Form.TheButtonPanel.Visible=true;
        Form.CanEmpty = CanEmpty;
        Form.DocView.CurrentId = DocId;
        // TODO: Form.ExternalFilters = Filters;
        // TODO: Form.ExternalEditorCaller = ExternalEditorCaller;
        if (EFPApp.ShowDialog(Form, false) == DialogResult.OK)
        {
          DocId = Form.DocView.CurrentId;
          Res = true;
        }
      }
      finally
      {
        Form.Dispose();
      }
      return Res;
    }


    #endregion

    #region Редактирование документов

    /// <summary>
    /// Выполнение создания, редактирования, удаления или просмотра документа 
    /// </summary>
    /// <param name="ValidateState">Требуемый режим</param>
    /// <param name="EditIds">Идентификаторы документов в режиме просмотра, редактирования или удаления</param>
    /// <param name="Modal">True для запуска в модальном режиме</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (DocumentEditor.DataChanged)</returns>
    public bool PerformEditing(Int32[] EditIds, EFPDataGridViewState State, bool Modal, IDocumentEditorCaller Caller)
    {

      switch (State)
      {
        case EFPDataGridViewState.Insert:
          break;
        case EFPDataGridViewState.InsertCopy:
          if (EditIds.Length != 1)
          {
            EFPApp.ShowTempMessage("Должен быть выбран один документ");
            return false;
          }
          break;
        default:
          if (EditIds.Length < 1)
          {
            EFPApp.ShowTempMessage("Не выбрано ни одного документа");
            return false;
          }
          break;
      }

      bool FixedResult = false;
      if (Editing != null)
      {
        DocTypeEditingEventArgs Args = new DocTypeEditingEventArgs(this, State, EditIds, Modal, Caller);
        Editing(this, Args);
        if (Args.Handled)
          return Args.HandledResult;
        else
          FixedResult = Args.HandledResult;
      }

      DocumentEditor de = new DocumentEditor(DocHandlers, DocType.Name, State, EditIds);
      de.Modal = Modal;
      de.Caller = Caller;
      de.Run();

      return de.DataChanged || FixedResult;
    }

    public bool PerformEditing(Int32[] EditIds, EFPDataGridViewState State, bool Modal)
    {
      return PerformEditing(EditIds, State, Modal, null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра документа
    /// </summary>
    /// <param name="EditId">Идентификатор редактируемого документа</param>
    /// <param name="DataReadOnly">True, если требуется просмотр, false-редактирование</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен</returns>
    public bool PerformEditing(Int32 EditId, bool ReadOnly)
    {
      if (EditId == 0)
      {
        EFPApp.ShowTempMessage("Должен быть выбран один документ");
        return false;
      }

      Int32[] EditIds = new int[1];
      EditIds[0] = EditId;

      EFPDataGridViewState State = ReadOnly ? EFPDataGridViewState.View : EFPDataGridViewState.Edit;
      return PerformEditing(EditIds, State, true, null);
    }

    #endregion

    #region События редактирования документов

    /// <summary>
    /// Вызывается при запуске редактирования до вывода окна релактора на экран
    /// и до создание объекта DocumentEditor.
    /// Обработчик может выполнить собственные действия, вместо редактирования
    /// с помощью стандартного редактора, установив свойство Args.Handled=true
    /// Обработчик не вызывается, если объект DocumentEditor создается и 
    /// запускается не с помощью метода ClientDocType.PerformEditing()
    /// </summary>
    public event DocTypeEditingEventHandler Editing;

    /// <summary>
    /// Возвращает true, если есть хотя бы один установвленный обработчик
    /// Editing, BeforeEdit или InitEditForm, то есть возможность редактирования 
    /// была инициализирована.
    /// Если свойство возвращает false, то вызов PerformEditing() бесполезен 
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
    /// отказаться от редактирования, установив Cancel.
    /// </summary>
    public event BeforeDocEditEventHandler BeforeEdit;
    internal void DoBeforeEdit(DocumentEditor Editor, out bool Cancel, out bool ShowEditor)
    {
      BeforeDocEditEventArgs Args = new BeforeDocEditEventArgs(Editor);
      if (BeforeEdit != null)
        BeforeEdit(this, Args);

      Cancel = Args.Cancel;
      ShowEditor = Args.ShowEditor;
    }


    /// <summary>
    /// Вызывается при инициализации окна редактирования документа. 
    /// Должен создать закладки. 
    /// </summary>
    public event InitDocEditFormEventHandler InitEditForm;
    public void DoInitEditForm(DocumentEditor Editor)
    {
      DoInitEditForm(Editor, Editor.Documents[0]);
    }

    public void DoInitEditForm(DocumentEditor Editor, DBxMultiDocs MultiDocs)
    {
      if (MultiDocs.DocType.Name != this.DocType.Name)
        throw new ArgumentException("Попытка инициализации для чужого типа документов", "MultiDocs");

      InitDocEditFormEventArgs Args = new InitDocEditFormEventArgs(Editor, MultiDocs);
      if (InitEditForm != null)
        InitEditForm(this, Args);
    }

    /// <summary>
    /// Вызывается из редактора документа перед записью значений в режимах
    /// Edit, Insert и InsertCopy. На момент вызова значения полей формы переписаны
    /// в поля DocumentEditor.Documents. Обработчик может скорректировать эти значения
    /// (например, не заданные поля).
    /// Также обработчик может отменить запись документов на сервере, установив
    /// Args.Cancel=true
    /// </summary>
    public event DocEditCancelEventHandler Writing;

    internal bool DoWriting(DocumentEditor Editor)
    {
      if (Writing != null)
      {
        DocEditCancelEventArgs Args = new DocEditCancelEventArgs(Editor);
        Writing(this, Args);
        if (Args.Cancel)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Вызывется после нажатия кнопки "ОК" или "Применить" в режиме
    /// копирования, создания или редактирования. Может вызываться
    /// многократно.
    /// </summary>
    public event DocEditEventHandler Wrote; // или Wriiten ???
    internal void DoWrote(DocumentEditor Editor)
    {
      if (Wrote != null)
      {
        try
        {
          DocEditEventArgs Args = new DocEditEventArgs(Editor);
          Wrote(this, Args);
        }
        catch (Exception e)
        {
          DebugTools.ShowException(e, "Ошибка при обработке события после записи значений");
        }
      }
    }

    #endregion
  }
}
