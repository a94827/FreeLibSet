// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using FreeLibSet.Config;
using System.ComponentModel;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Forms;
using FreeLibSet.Logging;
using FreeLibSet.Remoting;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  #region Делегаты

  /// <summary>
  /// Аргументы события DBUI.AddCopyFormats
  /// </summary>
  public class DBUIAddCopyFormatsEventArgs : DataObjectEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Вызывается из DBUI
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="docSel"></param>
    public DBUIAddCopyFormatsEventArgs(IDataObject dataObject, DBxDocSelection docSel)
      : base(dataObject)
    {
#if DEBUG
      if (docSel == null)
        throw new ArgumentNullException("docSel");
#endif

      _DocSel = docSel;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выборка документов, которая копируется в буфер обмена.
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private DBxDocSelection _DocSel;

    #endregion
  }

  /// <summary>
  /// Делегат события DBUI.AddCopyFormats
  /// </summary>
  /// <param name="sender">Объект DBUI</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBUIAddCopyFormatsEventHandler(object sender, DBUIAddCopyFormatsEventArgs args);

  #endregion


#if XXX // пока не вижу смысла в таких настройках
  /// <summary>
  /// Аргумент вызова метода DBUI.EndInit().
  /// Определяет, какие столбцы и подсказки нужно добавить к объявлениям документов и поддокументов
  /// </summary>
  [Flags]
  public enum DBUIEndInitOptions
  { 
    /// <summary>
    /// Ничего добавлять не надо
    /// </summary>
    None=0,

    /// <summary>
    /// В справочники документов нужно добавить столбец "Id", если он не добавлен в явном виде.
    /// Если столбец "Id" был добавлен в явном виде, то игнорируется
    /// </summary>
    DocIdColumn=1,

    /// <summary>
    /// В справочники документов добавляются столбцы "CreateUserId", "CreateTime", "ChangeUserId", "ChangeTime" и "Version".
    /// Если у пользователя отозвано разрешение на просмотр истории DocTypeViewHistoryPermission, то столбцы не добавляются.
    /// Если в объекте DBxDocTypes не установлены свойства UseTime, UseUsers, UseVersions, то соответствующие столбцы не добавляются.
    /// Столбец "Version" не добавляется, если флажок DocIdColumn не установлен.
    /// </summary>
    HistoryColumns=2,
  }

#endif

  /// <summary>
  /// Обработчики документов на стороне клиента
  /// Класс не является потокобезопасным.
  /// 
  /// Если используется сохранение композиции рабочего стола
  /// (EFPApp.LoadComposition()), то созданный объект DBUI
  /// должен быть добавлен в список EFPApp.FormCreators
  /// </summary>
  public class DBUI : IEFPFormCreator
  {
    #region Конструктор

    /// <summary>
    /// Статический конструктор приделывает обработчик ошибок
    /// </summary>
    static DBUI()
    {
      EFPApp.ExceptionShowing += new EFPAppExceptionEventHandler(EFPApp_ExceptionShowing);
      EFPApp.MainImages.Add(MainImagesResource.ResourceManager);
    }

    /// <summary>
    /// Создает экземпляр объекта.
    /// </summary>
    /// <param name="sourceProxy">Прокси провайдера доступа к документам (полученный от сервера).
    /// Не может быть null.
    /// Вызывайте DBxDocProvider.CreateProxy() на стороне сервера и передавайте DBxDcoProviderProxy клиенту,
    /// а не вызывайте CreateProxy() на стороне клиента
    /// Если требуется заменять провайдер в процессе работы, используйте метод SetDocProvider()</param>
    public DBUI(DBxDocProviderProxy sourceProxy)
    {
      if (sourceProxy == null)
        throw new ArgumentNullException("sourceProxy");

      _SourceDocProvider = sourceProxy.Source;
      _DocProvider = new DocProviderUI(sourceProxy, this);
      _DocTypes = new DocTypeList(this); // после инициализации FDocProvider 

      _TextHandlers = new DBxDocTextHandlers(_DocProvider.DocTypes, _DocProvider.DBCache);
      _ImageHandlers = new DBxDocImageHandlers(_DocProvider.DocTypes, _DocProvider.DBCache/*, FDocProvider.DBPermissions*/);

      _UserPermissionsUI = new UserPermissionsUI();
      DBxDocProvider.InitStdUserPermissionCreators(_UserPermissionsUI.Creators, DocProvider.DocTypes);
      InitStdUserPermissionsUI();

      InitGroupDocDict();

      // Это должно выполняться клиентом
      // EFPApp.FormCreators.Add(this);

      _ShowWaitMessages = true;
      _DefaultEmptyEditMode = MultiSelectEmptyEditMode.Select;
    }

    private void InitStdUserPermissionsUI()
    {
      UserPermissionsUI.Add(new WholeDBPermissionUI());
      UserPermissionsUI.Add(new TableUserPermissionUI());
      UserPermissionsUI.Add(new DocTypePermissionUI(this));
      UserPermissionsUI.Add(new DocTypeViewHistoryPermissionUI(this));
      UserPermissionsUI.Add(new ViewOtherUsersActionPermissionUI());
      UserPermissionsUI.Add(new RecalcColumnsPermissionUI());
    }


    private void InitGroupDocDict()
    {
      _GroupDocDict = new Dictionary<string, string>();
      _GroupDocList = new SingleScopeList<string>();
      foreach (DBxDocType dt in DocProvider.DocTypes)
      {
        if (!String.IsNullOrEmpty(dt.GroupRefColumnName))
        {

          DBxColumnStruct colDef = dt.Struct.Columns[dt.GroupRefColumnName];
          if (colDef == null)
            throw new BugException("Неизвестное имя столбца \"" + dt.GroupRefColumnName + "\" в документе \"" + dt.Name + "\", которое задается свойством GroupRefColumnName");
          if (String.IsNullOrEmpty(colDef.MasterTableName))
            throw new BugException("Столбец \"" + dt.GroupRefColumnName + "\" документа \"" + dt.Name + " не является ссылочным");
          DBxDocType dt2 = DocProvider.DocTypes[colDef.MasterTableName];
          if (dt2 == null)
            throw new BugException("Неизвестный вид документов \"" + colDef.MasterTableName + "\" в документе \"" + dt.Name + "\" для группировки");

          _GroupDocDict.Add(dt.Name, dt2.Name);
          _GroupDocList.Add(dt2.Name);
        }
      }

      // Инициализируем редакторы и просмотр
      foreach (string nm in _GroupDocList)
      {
        GroupDocTypeUI dtui = (GroupDocTypeUI)(DocTypes[nm]);
        dtui.GridProducer.Columns.AddText(dtui.NameColumnName, "Группа", 30, 5);
        dtui.GridProducer.DefaultConfig = new EFPDataGridViewConfig();
        dtui.GridProducer.DefaultConfig.Columns.AddFill(dtui.NameColumnName);
        dtui.ImageKey = "TreeViewClosedFolder";
        dtui.InitEditForm += new InitDocEditFormEventHandler(EditGroupDoc.InitDocEditForm);
      }
    }

    #endregion

    #region DocProvider

    /// <summary>
    /// Базовый провайдер, полученный от сервера
    /// </summary>
    private DBxDocProvider _SourceDocProvider;

    /// <summary>
    /// Обработчик документов, полученный с сервера.
    /// Свойство возвращает не тот провайдер, который был передан конструктору,
    /// а собственный экземпляр, создающий цепочку провадйеров.
    /// Базовый провайдер может быть заменен в процессе работы 
    /// вызовом метода SetDocProvider(). При этом будет создан и новый провайдер,
    /// возвращаемый этим свойством
    /// </summary>
    public DocProviderUI DocProvider { get { return _DocProvider; } }
    private DocProviderUI _DocProvider;

    /// <summary>
    /// Замена провайдера документов в процессе работы.
    /// Метод может использоваться при динамическом изменении прав пользователя.
    /// Предполагается, что <paramref name="sourceProxy"/> получен с сервера
    /// </summary>
    /// <param name="sourceProxy">Прокси нового провайдера (полученный от сервера)</param>
    public void SetDocProvider(DBxDocProviderProxy sourceProxy)
    {
      if (sourceProxy == null)
        throw new ArgumentNullException("sourceProxy");
      if (object.ReferenceEquals(sourceProxy.Source, this._SourceDocProvider)) // не надо сравнивать ссылки на DBxDocProviderProxy, так как эти объекты сериализуются заново, в отличие от MBR-объекта DBxDocProvider
        return; // ложный вызов

      //DocProviderUI OldProvider = this.DocProvider;

      this._SourceDocProvider = sourceProxy.Source;
      this._DocProvider = new DocProviderUI(sourceProxy, this);

      this._DocProvider.UserPermissionCreators = UserPermissionsUI.Creators;

      OnDocProviderChanged();
    }

    /// <summary>
    /// Этот метод вызывается после вызова SetDocProvider()
    /// </summary>
    protected virtual void OnDocProviderChanged()
    {
      _TextHandlers.DBCache = _DocProvider.DBCache;
      _ImageHandlers.DBCache = _DocProvider.DBCache;
      foreach (DocTypeUI dtui in DocTypes.Items.Values)
      {
        dtui.OnDocProviderChanged();
        dtui.Browsers.InvalidateControls();
      }

      InitMainMenuCommandItemsVisible();
    }

    #endregion

    #region Прочие свойства

    #region Статические свойства

    /// <summary>
    /// Начальное значение свойств <see cref="EFPMultiDocComboBox"/>.EmptyEditMode и <see cref="RefDocGridFilter"/>.EmptyEditMode.
    /// Позволяет не задавать значение для каждого комбоблока и фильтра отдельно
    /// По умолчанию - Select.
    /// </summary>
    public  MultiSelectEmptyEditMode DefaultEmptyEditMode { get { return _DefaultEmptyEditMode; } set { _DefaultEmptyEditMode = value; } }
    private MultiSelectEmptyEditMode _DefaultEmptyEditMode;

    #endregion

    /// <summary>
    /// Надо ли показывать в таблицах поле "Id" и другую отладочную информацию
    /// </summary>
    public bool DebugShowIds { get { return _DebugShowIds; } set { _DebugShowIds = value; } }
    private bool _DebugShowIds;

    /// <summary>
    /// Сюда могут быть добавлены классы разрешений.
    /// По умолчанию в списке присутствуют стандартные классы разрешений
    /// </summary>
    public UserPermissionsUI UserPermissionsUI { get { return _UserPermissionsUI; } }
    private UserPermissionsUI _UserPermissionsUI;


    /// <summary>
    /// Словарь групп для документов
    /// ключ - тип документа, который имеет GroupRefColumnName
    /// значение - тип документа группы (дерево).
    /// Теоретически, один вид документа групп может относиться к нескольким видам документв
    /// </summary>
    private Dictionary<string, string> _GroupDocDict;

    /// <summary>
    /// Список видов документов, реалищующих группы (деревья)
    /// </summary>
    private SingleScopeList<string> _GroupDocList;

    /// <summary>
    /// Если true (по умолчанию), то при выполнении обращений к базе данных
    /// будут выводиться временные сообщения с помощью EFPApp.BeginWait()/EndWait()
    /// </summary>
    public bool ShowWaitMessages
    {
      get { return _ShowWaitMessages; }
      set { _ShowWaitMessages = value; }
    }
    private bool _ShowWaitMessages;

    #endregion

    #region Список видов документов

    /// <summary>
    /// Список для свойства DBUI.DocTypes
    /// </summary>
    public class DocTypeList : IEnumerable<DocTypeUI>
    {
      #region Защищенный конструктор

      internal DocTypeList(DBUI ui)
      {
        _UI = ui;
        _Items = new Dictionary<string, DocTypeUI>(ui.DocProvider.DocTypes.Count);
      }

      #endregion

      #region Свойства

      private DBUI _UI;

      internal Dictionary<string, DocTypeUI> Items { get { return _Items; } }
      private Dictionary<string, DocTypeUI> _Items;

      /// <summary>
      /// Возвращает интерфейс вида документа по имени таблицы.
      /// Если в DBxDocTypes нет описания такого вида документов, генерируется исключение
      /// </summary>
      /// <param name="docTypeName">Имя вида документа</param>
      /// <returns></returns>
      public DocTypeUI this[string docTypeName]
      {
        get
        {
          DocTypeUI res;
          if (!_Items.TryGetValue(docTypeName, out res))
          {
            if (String.IsNullOrEmpty(docTypeName))
              throw new ArgumentNullException("docTypeName");
            if (!_UI.DocProvider.DocTypes.Contains(docTypeName))
              throw new ArgumentException("Неизвестный тип документов \"" + docTypeName + "\"", "docTypeName");

            if (_UI._GroupDocList.Contains(docTypeName))
              res = new GroupDocTypeUI(_UI, _UI.DocProvider.DocTypes[docTypeName]);
            else
              res = new DocTypeUI(_UI, _UI.DocProvider.DocTypes[docTypeName]);
            _Items.Add(docTypeName, res);

            string groupName;
            if (_UI._GroupDocDict.TryGetValue(docTypeName, out groupName))
            {
              GroupDocTypeUI dt2 = (GroupDocTypeUI)(this[groupName]);
              res.GroupDocType = dt2;
            }
          }
          return res;
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Поиск интерфейса документа или поддокумента по имени таблицы.
      /// Если в DBxDocTypes нет описания с заданным именем таблицы, возвращается false.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <param name="docTypeUI">Если найден документ или поддокумент, то сюда записывается ссылка на интерфейс документа</param>
      /// <param name="subDocTypeUI">Если найден поддокумент, то сюда записывается ссылка на интерфейс поддокумента.
      /// Если имя таблицы <paramref name="tableName"/> соответствует документу, а не поддокументу,
      /// тогда записывается значение null</param>
      /// <returns>True, если найдено описание документа или поддокумента.
      /// False, если имя таблицы не найдено</returns>
      public bool FindByTableName(string tableName, out DocTypeUI docTypeUI, out SubDocTypeUI subDocTypeUI)
      {
        DBxDocType docType;
        DBxSubDocType subDocType;
        if (_UI.DocProvider.DocTypes.FindByTableName(tableName, out docType, out subDocType))
        {
          docTypeUI = this[docType.Name];
          if (subDocType == null)
            subDocTypeUI = null;
          else
            subDocTypeUI = docTypeUI.SubDocTypes[subDocType.Name];
          return true;
        }
        else
        {
          docTypeUI = null;
          subDocTypeUI = null;
          return false;
        }
      }

      /// <summary>
      /// Поиск интерфейса документа или поддокумента по имени таблицы.
      /// Если в DBxDocTypes нет описания с заданным именем таблицы, возвращается null
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Интерфейс или null</returns>
      public DocTypeUIBase FindByTableName(string tableName)
      {
        DBxDocType docType;
        DBxSubDocType subDocType;
        if (_UI.DocProvider.DocTypes.FindByTableName(tableName, out docType, out subDocType))
        {
          DocTypeUI DocTypeUI = this[docType.Name];
          if (subDocType == null)
            return DocTypeUI;
          else
            return DocTypeUI.SubDocTypes[subDocType.Name];
        }
        else
          return null;
      }

      /// <summary>
      /// Поиск интерфейса документа по идентификатору таблицы.
      /// Если в DBxDocTypes нет описания вида документа с заданным идентификатором таблицы,
      /// возвращается null
      /// </summary>
      /// <param name="tableId">Свойство DBxDocType.TableId</param>
      /// <returns>Интерфейс вида документа или null</returns>
      public DocTypeUI FindByTableId(Int32 tableId)
      {
        DBxDocType docType = _UI.DocProvider.DocTypes.FindByTableId(tableId);
        if (docType != null)
          return this[docType.Name];
        else
          return null;
      }

      #endregion

      #region IEnumerable<DocTypeUI> Members

      /// <summary>
      /// Возвращает перечислитель по интерфейсам видов документов
      /// </summary>
      /// <returns></returns>
      public IEnumerator<DocTypeUI> GetEnumerator()
      {
        return _Items.Values.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _Items.Values.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Список интерфейсов для отдельных видов документов
    /// </summary>
    public DocTypeList DocTypes { get { return _DocTypes; } }
    private DocTypeList _DocTypes;

    #endregion

    #region Текстовое представление документов и значки

    /// <summary>
    /// Получение текстового представления
    /// </summary>
    public DBxDocTextHandlers TextHandlers { get { return _TextHandlers; } }
    private DBxDocTextHandlers _TextHandlers;

    /// <summary>
    /// Возвращает имя пользователя.
    /// Если <paramref name="userId"/>=0, возвращается пустая строка
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Имя пользователя</returns>
    public string GetUserName(Int32 userId)
    {
      return TextHandlers.GetTextValue(DocProvider.DocTypes.UsersTableName, userId);
    }

    #endregion

    #region Значки

    /// <summary>
    /// Получение значков
    /// </summary>
    public DBxDocImageHandlers ImageHandlers
    {
      get { return _ImageHandlers; }
    }
    private DBxDocImageHandlers _ImageHandlers;

    /// <summary>
    /// Закончить инициализацию интерфейсов документов.
    /// Для каждого вида документов в GridProducer будут добавлены столбцы с датой/пользователем, который
    /// изменил документ, и номером версии. Столбцы доступны, если у пользователя есть разрешение на просмотр истории
    /// </summary>
    public void EndInit()
    {
      EndInit(true);
    }

    /// <summary>
    /// Закончить инициализацию интерфейсов документов.
    /// </summary>
    /// <param name="addDefaultColumns">
    /// Если true, то для каждого вида документов в GridProducer будут добавлены столбцы с датой/пользователем, который
    /// изменил документ, и номером версии. Столбцы доступны, если у пользователя есть разрешение на просмотр истории
    /// </param>
    public void EndInit(bool addDefaultColumns)
    {

      _TextHandlers.SetReadOnly();
      _ImageHandlers.SetReadOnly();
      _UserPermissionsUI.SetReadOnly();
      if (_DocProvider.UserPermissionCreators == null)
        _DocProvider.UserPermissionCreators = UserPermissionsUI.Creators;
      else
        throw new InvalidOperationException("Свойство DocProvider.UserPermissionCreators не должно устанавливаться вручную");
      //{
      //  UserPermissionCreators Creators = new UserPermissionCreators();
      //  DBxDocProvider.InitStdUserPermissionCreators(Creators, FDocProvider.DocTypes);
      //  FDocProvider.UserPermissionCreators = Creators;
      //}

      if (addDefaultColumns)
        DoAddDefaultColumns(); // обязательно после инициализации разрешений

      // 21.05.2021
      // Блокируем GridProducer'ы от изменений и выполняем проверку корректности
      foreach (DocTypeUI dtui in DocTypes)
      {
        dtui.GridProducer.SetReadOnly();
        foreach (SubDocTypeUI sdtui in dtui.SubDocTypes)
          sdtui.GridProducer.SetReadOnly();
      }

      InitMainMenuCommandItemsVisible();
    }

    private void InitMainMenuCommandItemsVisible()
    {
      foreach (DocTypeUI dtui in DocTypes.Items.Values)
        dtui.InitMainMenuCommandItemVisible();
    }

    /// <summary>
    /// Возвращает имя значка в списке EFPApp.MainImages для пользователя.
    /// Если <paramref name="userId"/>=0, возвращается "EmptyImage"
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Значок для пользователя</returns>
    public string GetUserImageKey(Int32 userId)
    {
      return ImageHandlers.GetImageKey(DocProvider.DocTypes.UsersTableName, userId);
    }

    #endregion

    #region Поля и подсказки для всех типов документов

    private void DoAddDefaultColumns()
    {
      // Поле DocProvider.UserPermissions инициализируется только после EndInit()

#if XXX // Убрано 02.07.2021. Теперь есть произвольная сортировка
      // Так как DBxOrder является классом однократной записи, можно создать объекты один раз
      DBxOrder ChOrder1 = null;
      DBxOrder ChOrder2 = null;
      if (DocProvider.DocTypes.UseTime)
      {
        ChOrder1 = new DBxOrder(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ChangeTime"), new DBxColumn("CreateTime")), ListSortDirection.Ascending);
        ChOrder2 = new DBxOrder(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ChangeTime"), new DBxColumn("CreateTime")), ListSortDirection.Descending);
      }
#endif
      //DBxDocType DocTypeUsers = DocProvider.DocTypes[DocProvider.DocTypes.UsersTableName];
      //List<DBxOrderItemInfo> OrderItems = new List<DBxOrderItemInfo>();
      //for (int i = 0; i < DocTypeUsers.DefaultOrder.Infos.Length; i++)
      //{
      //  DBxOrderItemInfo SrcInfo = DocTypeUsers.DefaultOrder.Infos[i];
      //  OrderItems.Add(SrcInfo.SetColumnNamePrefix("CreateUser."));
      //}
      //OrderItems.Add(new DBxOrderItemInfo (new DBxOrderColumn("CreateTime")));
      //DBxOrder CrOrder3 = new DBxOrder(OrderItems); // по имени пользователя, создавшего документ

      // Исходные столбцы всплывающей подсказки по документу
      string aboutDocSrcColNames = null;
      if (DocProvider.DocTypes.UseTime)
      {
        aboutDocSrcColNames = "Id,CreateTime,ChangeTime";
        if (DocProvider.DocTypes.UseVersions)
          aboutDocSrcColNames += ",Version";
        if (DocProvider.DocTypes.UseUsers)
          aboutDocSrcColNames += ",CreateUserId,ChangeUserId";
        if (DocProvider.DocTypes.UseDeleted)
          aboutDocSrcColNames += ",Deleted";
      }


      foreach (DocTypeUI dt in DocTypes)
      {
        if (!dt.GridProducer.Columns.Contains("Id"))
        {
          EFPGridProducerColumn idCol = dt.GridProducer.Columns.AddInt("Id", "Id", 6);
          idCol.DisplayName = "Идентификатор документа";
          idCol.CanIncSearch = true;
          // Делаем первым в списке
          dt.GridProducer.Columns.RemoveAt(dt.GridProducer.Columns.Count - 1);
          dt.GridProducer.Columns.Insert(0, idCol);
        }

        bool historyAllowed = DocTypeViewHistoryPermission.GetAllowed(this.DocProvider.UserPermissions, dt.DocType.Name);

        // Информация о документе
        if (historyAllowed) // 11.04.2016
        {
          if (DocProvider.DocTypes.UseTime)
          {
            dt.GridProducer.Columns.AddDateTime("CreateTime", "Время создания документа");
          }

          if (DocProvider.DocTypes.UseUsers)
          {
            //dt2.GridProducer.Columns.AddText("CreateUser", "Документ создан пользователем", 20, 10);
            dt.GridProducer.Columns.AddUserText("CreateUserName", "ChangeUserId,CreateUserId",
              new EFPGridProducerValueNeededEventHandler(CreateUserNameColumnValueNeeded),
              "Документ создан пользователем", 20, 10);
            dt.GridProducer.Columns.LastAdded.SizeGroup = "UserName";
            dt.GridProducer.Columns.LastAdded.CanIncSearch = true;
          }

          if (DocProvider.DocTypes.UseTime)
          {
            //dt2.GridProducer.Columns.AddDateTime("ChangeTime", "Время изменения документа");
            dt.GridProducer.Columns.AddUserDateTime("ChangeTime2" /*21.05.2021 */, "ChangeTime,CreateTime",
              new EFPGridProducerValueNeededEventHandler(ChangeTimeColumnValueNeeded),
              "Время изменения документа");
          }

          if (DocProvider.DocTypes.UseUsers)
          {
            //dt2.GridProducer.Columns.AddText("ChangeUserId.ИмяПользователя", "Документ изменен пользователем", 20, 10);
            dt.GridProducer.Columns.AddUserText("ChangeUserName", "ChangeUserId,CreateUserId",
              new EFPGridProducerValueNeededEventHandler(ChangeUserNameColumnValueNeeded),
              "Документ изменен пользователем", 20, 10);
            dt.GridProducer.Columns.LastAdded.SizeGroup = "UserName";
            dt.GridProducer.Columns.LastAdded.CanIncSearch = true;
          }

          if (DocProvider.DocTypes.UseVersions)
          {
            dt.GridProducer.Columns.AddText("Version", "Версия документа", 4, 2);
            dt.GridProducer.Columns.LastAdded.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
          }

          // TODO:
          //if (DocProvider.DocTypes.UseUsers)
          //{ 
          //  DBxOrder CrUserOrder, ChUserOrder;
          //  if (DocProvider.DocTypes.UseTime)
          //  {
          //    CrUserOrder=new DBxOrder(new DBxOrderItemInfo[]{new DBxOrderItemInfo(new DBxColumn("CreateUserId."))}
          //  }
          //  GridProducerOrder gpo1=new GridProducerOrder(
          //}

#if XXX // Убрано 02.07.2021. Теперь есть произвольная сортировка
          if (DocProvider.DocTypes.UseTime)
          {
            dt.GridProducer.Orders.Add("CreateTime", "По времени создания (новые внизу)");
            dt.GridProducer.Orders.Add("CreateTime DESC", "По времени создания (новые сверху)");
          }
          // Не работает. Надо, чтобы столбец CreateUserId.ИмяПользователя был в таблице. Добавлять его в список фиксированных полей нежелательно
          //GridProducerOrder gpo3 = new GridProducerOrder(CrOrder3, "Документ создан пользователем");
          //gpo3.SortInfo = new EFPDataGridViewSortInfo("CreateUserName", false);
          //dt.GridProducer.Orders.Add(gpo3);


          if (DocProvider.DocTypes.UseTime)
          {
            dt.GridProducer.Orders.Add(ChOrder1, "По времени изменения (новые внизу)", new EFPDataGridViewSortInfo("ChangeTime2", ListSortDirection.Ascending));
            dt.GridProducer.Orders.Add(ChOrder2, "По времени изменения (новые сверху)", new EFPDataGridViewSortInfo("ChangeTime2", ListSortDirection.Descending));
          }
          //dt2.GridProducer.FixedColumns.Add("CreateUserId");
          //dt2.GridProducer.FixedColumns.Add("ChangeUserId");
          if (DocProvider.DocTypes.UseTime)
          {
            dt.GridProducer.FixedColumns.Add("CreateTime");
            dt.GridProducer.FixedColumns.Add("ChangeTime");
          }
#endif

          if (aboutDocSrcColNames != null)
          {
            EFPGridProducerToolTip ttDocumentInfo = dt.GridProducer.ToolTips.AddUserItem("DocumentInfo", aboutDocSrcColNames,
              new EFPGridProducerValueNeededEventHandler(AboutDoc_ToolTipTextNeeded));
            ttDocumentInfo.DisplayName = "Документ создан / изменен";
            ttDocumentInfo.Tag = dt;
          }
        } // HistoryAllowed

        // 04.12.2015
        // 16.09.2021 Больше не нужно. Если всегда есть столбец "Id", то и произвольная сортировка по нему возможна
        //if (dt.GridProducer.Orders.Count == 0)
        //{
        //  dt.GridProducer.Orders.Add(dt.DocType.DefaultOrder, "Основной порядок сортировки");
        //}
      }
    }

    private void CreateUserNameColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      args.Value = GetUserName(args.GetInt("CreateUserId"));
    }

    private void ChangeUserNameColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      Int32 changeUserId = args.GetInt("ChangeUserId");
      if (changeUserId != 0)
        args.Value = GetUserName(changeUserId);
      else
        args.Value = GetUserName(args.GetInt("CreateUserId"));
    }

    private void ChangeTimeColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateTime? dt = args.GetNullableDateTime("ChangeTime");
      if (dt.HasValue)
        args.Value = dt.Value;
      else
        args.Value = args.GetNullableDateTime("CreateTime");
    }

    /// <summary>
    /// Текст подсказки о создании / изменении документа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AboutDoc_ToolTipTextNeeded(object sender,
      EFPGridProducerValueNeededEventArgs args)
    {
      Int32 docId = args.GetInt("Id");
      if (docId == 0)
      {
        args.Value = "Нет выбранного документа";
        return;
      }

      DocTypeUI dt = (DocTypeUI)(args.Tag);
      StringBuilder sb = new StringBuilder();

      sb.Append("Документ \"");
      sb.Append(dt.DocType.SingularTitle);
      sb.Append("\" Id=");
      sb.Append(docId);

      if (DocProvider.DocTypes.UseVersions)
      {
        sb.Append(" Версия ");
        sb.Append(args.GetInt("Version"));
      }

      sb.Append(" создан ");
      DateTime? createTime = args.GetNullableDateTime("CreateTime");
      if (createTime.HasValue)
        sb.Append(createTime.Value.ToString());
      if (DocProvider.DocTypes.UseUsers)
      {
        sb.Append(" (");
        sb.Append(TextHandlers.GetTextValue("Пользователи", args.GetInt("CreateUserId")));
        sb.Append(")");
      }
      DateTime? changeTime = args.GetNullableDateTime("ChangeTime");
      if (changeTime.HasValue)
      {
        bool isDeleted = false;
        if (DocProvider.DocTypes.UseDeleted)
          isDeleted = args.GetBool("Deleted");
        if (isDeleted)
          sb.Append(". Удален ");
        else
          sb.Append(". Изменен ");
        sb.Append(changeTime.Value.ToString());
        if (DocProvider.DocTypes.UseUsers)
        {
          sb.Append(" (");
          sb.Append(TextHandlers.GetTextValue("Пользователи", args.GetInt("ChangeUserId")));
          sb.Append(")");
        }
      }

      args.Value = sb.ToString();
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Событие вызывается при копировании выборок документов в буфер обмена.
    /// Обработчик события в прикладном коде может посмотреть копируемую выборку документов и добавить собственные
    /// дополнительные форматы данных.
    /// Обработчик не может менять саму выборку документов, так как она уже скопирована в буфер обмена на момент вызова.
    /// Также не должны добавляться данные в текстовом формате, CSV и HTML, так как они используются табличными просмотрами.
    /// </summary>
    public event DBUIAddCopyFormatsEventHandler AddCopyFormats;

    /// <summary>
    /// Вызывает событие AddCopyFormats, если установлен обрабоботчик события.
    /// Сама выборка не добавляется в список копируемых форматов, это должен сделать вызывающий код.
    /// Этот метод обычно не вызывается из прикладного кода, за исключением собственных управляющих элементов.
    /// В прикладном коде, для копирования выборки документов используйте метод CopyDocSel.
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="docSel"></param>
    public void OnAddCopyFormats(IDataObject dataObject, DBxDocSelection docSel)
    {
#if DEBUG
      if (dataObject == null)
        throw new ArgumentNullException("dataObject");
      if (docSel == null)
        throw new ArgumentNullException("docSel");
#endif

      if (AddCopyFormats == null)
        return;

      try
      {
        DBUIAddCopyFormatsEventArgs args = new DBUIAddCopyFormatsEventArgs(dataObject, docSel);
        AddCopyFormats(this, args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка вызова обработчика события DBUI.AddCopyFormats");
      }
    }

    /// <summary>
    /// Копирование выборки в буфер обмена.
    /// Вызывается событие AddCopyFormats
    /// </summary>
    /// <param name="docSel">Копируемая выборка. Не может быть null</param>
    public void CopyDocSel(DBxDocSelection docSel)
    {
      EFPApp.BeginWait("Копирование выборки документов в буфер обмена", "Copy");
      try
      {
        DataObject dobj = new DataObject();
        dobj.SetData(docSel);
        OnAddCopyFormats(dobj, docSel); // 06.02.2021
        EFPApp.Clipboard.SetDataObject(dobj, true);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Вставка выборки документов из буфера обмена.
    /// Если буфер обмена не содержит выборки, возвращается null, при этом выводится всплывающее сообщение.
    /// Выборка не нормализуется.
    /// Проверяется, что выборка в буфере относится к той же базе данных, что и DocProvider
    /// </summary>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection PasteDocSel()
    {
      DBxDocSelection docSel = null;
      IDataObject dobj = EFPApp.Clipboard.GetDataObject();
      if (dobj != null)
        docSel = (DBxDocSelection)(dobj.GetData(typeof(DBxDocSelection)));

      if (docSel == null)
        return null;

      if (docSel.DBIdentity != DocProvider.DBIdentity)
      {
        EFPApp.ShowTempMessage("Выборка в буфере обмена относится к другой базе данных");
        return null;
      }

      return docSel;
    }

    /// <summary>
    /// Вставка из буфера обмена единственного идентификатора документа заданного вида. 
    /// Возвращает 0, если в буфере обмена нет таких документов. В этом случае выдаются соответствуюие сообщения пользователю.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа, для которого ожидается идентификатор в буфере обмена</param>
    /// <returns>Идентификатор документа или 0</returns>
    public Int32 PasteDocSelSingleId(string docTypeName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
#endif

      DBxDocType dt = DocTypes[docTypeName].DocType;

#if DEBUG
      if (dt == null)
        throw new ArgumentException("Неизвестный вид документа", "docTypeName");
#endif

      DBxDocSelection docSel = PasteDocSel();
      if (docSel == null)
      {
        EFPApp.ShowTempMessage("В буфере обмена нет выборки документов");
        return 0;
      }

      Int32 docId = docSel.GetSingleId(docTypeName);
      if (docId == 0)
        EFPApp.ShowTempMessage("В буфере обмена нет ссылки на документ \"" + dt.SingularTitle + "\"");
      return docId;
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

    #region Просмотр выборки документов

    /// <summary>
    /// Открывает модальное или немодальное окно выборки документов
    /// </summary>
    /// <param name="docSel">Заполненная выборка документов</param>
    public void ShowDocSel(DBxDocSelection docSel)
    {
      ShowDocSel(docSel, "Выборка документов");
    }

    /// <summary>
    /// Открывает модальное или немодальное окно выборки документов
    /// </summary>
    /// <param name="docSel">Заполненная выборка документов</param>
    /// <param name="title">Заголовок формы</param>
    public void ShowDocSel(DBxDocSelection docSel, string title)
    {
      if (docSel == null || docSel.IsEmpty)
      {
        // 06.04.2018
        EFPApp.MessageBox("Выборка не содержит ни одного документа", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      Form frm = new Form();
      frm.Text = title;
      frm.Icon = EFPApp.MainImages.Icons["DBxDocSelection"];
      frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
      EFPFormProvider efpForm = new EFPFormProvider(frm);
      efpForm.ConfigSectionName = "DBxDocSelection"; // 11.06.2021
      DocSelectionEditor dse = new DocSelectionEditor(this, efpForm, frm);
      dse.ReadOnly = false; // 27.09.2017
      dse.DocSel = docSel;
      EFPApp.ShowFormOrDialog(frm);
    }

    /// <summary>
    /// Создать выборку для заданных документов.
    /// Массив идентификаторов может содержать нулевые и фиктивные идентификаторы, которые пропускаются 
    /// </summary>
    /// <param name="docTypeName">Вид документов. Если заданое неправильное значение, генерируется исключение</param>
    /// <param name="docIds">Массив идентификаторов документов. Фиктивные и нулевые идентификаторы игнорируются</param>
    /// <returns>Выборка документов одного вида</returns>
    public DBxDocSelection CreateDocSelection(string docTypeName, Int32[] docIds)
    {
#if DEBUG
      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      if (docIds == null)
        throw new ArgumentNullException("docIds");
#endif
      if (!DocProvider.DocTypes.Contains(docTypeName))
        throw new ArgumentException("Неизвестный вид документов \"" + docTypeName + "\"", "docTypeName");

      DBxDocSelection docSel = new DBxDocSelection(DocProvider.DBIdentity);
      for (int i = 0; i < docIds.Length; i++)
      {
        if (!DocProvider.IsRealDocId(docIds[i]))
          continue;
        docSel.Add(docTypeName, docIds[i]);
      }
      return docSel;
    }

    #endregion

    #region Изображения для DBxDocState


    /// <summary>
    /// Получить имя изображения в EFPApp.MainImages для перечисления DBxDocState
    /// </summary>
    /// <param name="state">Состояние документа</param>
    /// <returns>Имя изображения в EFPApp.MainImages</returns>
    public static string GetDocStateImageKey(DBxDocState state)
    {
      switch (state)
      {
        case DBxDocState.None: return "EmptyImage";
        case DBxDocState.View: return "View";
        case DBxDocState.Edit: return "Edit";
        case DBxDocState.Insert: return "Insert";
        case DBxDocState.Delete: return "Delete";
        case DBxDocState.Mixed: return "UnknownState"; // ??
        default: return "Error";
      }
    }

    #endregion

    #region Изображения для прав пользователей
#if XXX

    public static string GetValueImageKey(DBxAccessMode Mode)
    {
      if ((int)Mode >= 0 && (int)Mode <= WholeDBPermissionUI.ValueImageKeys.Length)
        return WholeDBPermissionUI.ValueImageKeys[(int)Mode];
      else
        return "UnknownState";
    }

    public static string GetValueImageKey(string Code)
    {
      int p = Array.IndexOf<string>(DBUserPermission.ValueCodes, Code);
      if (p >= 0)
        return WholeDBPermissionUI.ValueImageKeys[p];
      else
        return "UnknownState";
    }

    public static string GetValueImageKey(DBxAccessMode AccessMode, int CountNone, int CountReadOnly)
    {
      switch (AccessMode)
      {
        case DBxAccessMode.Full:
          if (CountNone > 0)
          {
            if (CountReadOnly > 0)
              return "CircleGreenYellowRed";
            else
              return "CircleGreenRed";
          }
          else
          {
            if (CountReadOnly > 0)
              return "CircleGreenYellow";
            else
              return "CircleGreen";
          }

        case DBxAccessMode.ReadOnly:
          if (CountNone > 0)
            return "CircleYellowRed";
          else
            return "CircleYellow";

        case DBxAccessMode.None:
          return "CircleRed";

        default:
          return "UnknownState";
      }
    }
#endif

    #endregion

    #region Изображения для UndoAction

    /// <summary>
    /// Получить текстовое обозначение для перечисления UndoAction
    /// </summary>
    /// <param name="action">Действие</param>
    /// <returns>Текстовое представление</returns>
    public static string GetUndoActionName(UndoAction action)
    {
      switch (action)
      {
        case UndoAction.Base: return "Начальное состояние";
        case UndoAction.Insert: return "Создание";
        case UndoAction.Edit: return "Изменение";
        case UndoAction.Delete: return "Удаление";
        case UndoAction.Undo: return "Откат";
        case UndoAction.Redo: return "Повтор";
        default: return "Неизвестное действие " + ((int)action).ToString();
      }
    }

    /// <summary>
    /// Получить имя изображения в EFPApp.MainImages для перечисления UndoAction
    /// </summary>
    /// <param name="action">Действие</param>
    /// <returns>Имя изображения в EFPApp.MainImages</returns>
    public static string GetUndoActionImageKey(UndoAction action)
    {
      switch ((UndoAction)action)
      {
        case UndoAction.Base: return "New";
        case UndoAction.Insert: return "Insert";
        case UndoAction.Edit: return "Edit";
        case UndoAction.Delete: return "Delete";
        case UndoAction.Undo: return "Undo";
        case UndoAction.Redo: return "Redo";
        default: return "Error";
      }
    }
    #endregion

    #region Просмотр действий пользователя

    /// <summary>
    /// Запрашивает параметры и выводит окно действий пользователя
    /// </summary>
    public void ShowUserActions()
    {
      //ShowUserActionsParamsForm.PerformShow(this);
      new UserActionsReport(this).Run();
    }

    /// <summary>
    /// Выводит окно действий пользователя с заданными параметрами
    /// </summary>
    /// <param name="reportParams">Заполненные параметры отчета</param>
    public void ShowUserActions(UserActionsReportParams reportParams)
    {
      UserActionsReport report = new UserActionsReport(this);
      report.ReportParams = reportParams;
      report.Run();
    }

    #endregion

#if XXX
    #region Рабочая дата (для фильтра DateRangeGridFilter)

    /// <summary>
    /// Если свойство устновлено в true, то используется рабочая дата.
    /// Если не установлено (по умолчанию), то используется текущая дата
    /// Это свойство влияет только на отображаемый текст фильтра
    /// </summary>
    public bool UseWorkDate;

    /// <summary>
    /// Текущая рабочая или обычная текущая дата
    /// </summary>
    public virtual DateTime WorkDate { get { return DateTime.Today; } }

    #endregion
#endif

    #region Обработка исключений

    private static void EFPApp_ExceptionShowing(object sender, EFPAppExceptionEventArgs args)
    {
      // Этот метод должен быть static.
      // Прикладной код может, теоретически, создать несколько объектов DBUI.
      // Но обработчик исключений может быть только один. Нет способа узнать, к какому DBUI относится исключение

      if (args.Handled)
        return;
      if (LogoutTools.GetException<UserCancelException>(args.Exception) != null)
        return; // на всякий случай

      if (!Application.MessageLoop)
        return; // 28.08.2018 Для отладки ошибок восстановления интерфейса

      DBxAccessException eAE = LogoutTools.GetException<DBxAccessException>(args.Exception);
      if (eAE != null)
      {
        //EFPApp.ErrorMessageBox("Отказано в доступе." + Environment.NewLine + Environment.NewLine + eAE.Message, args.Title);
        EFPApp.ExceptionMessageBox("Отказано в доступе." + Environment.NewLine + Environment.NewLine + eAE.Message, args.Exception, args.Title); // 25.09.2020
        args.Handled = true;
        return;
      }

      DBxDocCannotDeleteException eCDE = LogoutTools.GetException<DBxDocCannotDeleteException>(args.Exception);
      if (eCDE != null)
      {
        //EFPApp.ErrorMessageBox("Ошибка удаления данных на сервере." + Environment.NewLine + Environment.NewLine + eCDE.Message, args.Title);
        EFPApp.ExceptionMessageBox("Ошибка удаления данных на сервере." + Environment.NewLine + Environment.NewLine + eCDE.Message, args.Exception, args.Title); // 25.09.2020
        args.Handled = true;
        return;
      }

      DBxDocsLockException eDL = LogoutTools.GetException<DBxDocsLockException>(args.Exception);
      if (eDL != null)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append("Ошибка установки блокировки.");
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        sb.Append("Не удалось установить блокировку. ");
        if (eDL.IsSameDocProvider)
        {
          // 04.07.2016
          // На самом деле, все равно не работает.
          // Некоторые действия, например, установка состояния формы, выполняются на стороне сервера
          // с созданием отдельного DBxRealDocProvider
          sb.Append("Документ заблокирован Вами в текущем сеансе работы");
        }
        else if (String.IsNullOrEmpty(eDL.OldLock.UserName))
          sb.Append("Документ заблокирован");
        else
        {
          sb.Append("Документ заблокирован пользователем ");
          sb.Append(eDL.OldLock.UserName);
        }
        sb.Append(".");
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        if (eDL.OldLock.Data.DocCount == 1)
          sb.Append("Заблокированный документ: ");
        else
          sb.Append("Заблокированные документы: ");
        sb.Append(eDL.OldLock.DocText);


        EFPApp.MessageBox(sb.ToString(),
          args.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        args.Handled = true;
        return;
      }
    }


    /// <summary>
    /// Событие вызывается при возникновении исключения при вызове метода на стороне сервера через
    /// DBxChainDocProvider.
    /// Пользовательский обработчик может проверить исключение и, если оно связано с сетью, попробовать
    /// восстановить соединение с сервером. После этого следует установить свойство Retry в аргументе события.
    /// Обработчику следует вывести экранную заставку на время восстановления и обеспечить пользователю
    /// возможность прервать ожидание.
    /// Рекомендуемой альтернативой присоединения обработчика является переопределение метода OnExceptionCaught()
    /// </summary>
    public event DBxRetriableExceptionEventHandler ExceptionCaught;

    /// <summary>
    /// Метод вызывается при возникновении исключения при вызове метода на стороне сервера через
    /// DBxChainDocProvider.
    /// Переопределенный метод может проверить исключение и, если оно связано с сетью, попробовать
    /// восстановить соединение с сервером. После этого следует установить свойство Retry в аргументе события.
    /// Обработчику следует вывести экранную заставку на время восстановления и обеспечить пользователю
    /// возможность прервать ожидание.
    /// Непереопределенный метод вызывает событие ExceptionCaught
    /// </summary>
    internal protected virtual void OnExceptionCaught(DBxRetriableExceptionEventArgs args)
    {
      if (ExceptionCaught != null)
        ExceptionCaught(this, args);
    }

    #endregion

    #region IEFPFormCreator Members

    Form IEFPFormCreator.CreateForm(EFPFormCreatorParams creatorParams)
    {
      if (creatorParams.ClassName == typeof(DocTableViewForm).ToString())
      {
        string docTypeName = creatorParams.ConfigSectionName;
        DocTableViewForm form = new DocTableViewForm(this.DocTypes[docTypeName], DocTableViewMode.Browse);
        return form;
      }

      if (creatorParams.ClassName == typeof(DocInfoReport).ToString())
      {
        DocInfoReport report = new DocInfoReport(this);
        return ((IEFPFormCreator)report).CreateForm(creatorParams);
      }

      if (creatorParams.ClassName == typeof(UserActionsReport).ToString())
      {
        UserActionsReport report = new UserActionsReport(this);
        return ((IEFPFormCreator)report).CreateForm(creatorParams);
      }

      return null;
    }

    #endregion

    #region Обновление табличных просмотров

    /// <summary>
    /// Обновление страниц кэша таблиц и строк табличных просмотров для набора документов.
    /// Предполагается, что набор документов был загружен из базы данных.
    /// Удаленные строки и строки с фиктивными идентификаторами (новые) пропускаются.
    /// Если набор содержит данные для просмотра истории документов, никаких действий не выполняется.
    /// </summary>
    /// <param name="docSet">Набор данных</param>
    public void UpdateDBCacheAndRows(DBxDocSet docSet)
    {
      // Реализация идентична DocumentViewHandlerList.UpdateDBCacheAndRows()

#if DEBUG
      if (docSet == null)
        throw new ArgumentNullException("docSet");
#endif

      if (docSet.VersionView)
        return;

      docSet.DocProvider.UpdateDBCache(docSet.DataSet);

      for (int i = 0; i < docSet.Count; i++)
      {
        DocTypeUI dtui = DocTypes[docSet[i].DocType.Name];
        dtui.Browsers.UpdateDBCacheAndRows(docSet);
        dtui.RefreshBufferedData(); // 03.02.2022

        Int32[] docIds = docSet[i].DocIds;
        if (docIds.Length > 0)
          dtui.Browsers.UpdateRowsForIds(docIds);
      }
    }

    #endregion
  }
}
