using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.ExtDB;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using AgeyevAV.Config;
using System.ComponentModel;
using AgeyevAV.ExtForms.NodeControls;
using AgeyevAV.ExtForms;
using AgeyevAV.Logging;
using AgeyevAV.Remoting;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtForms.Docs
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

          DBxColumnStruct ColDef = dt.Struct.Columns[dt.GroupRefColumnName];
          if (ColDef == null)
            throw new BugException("Неизвестное имя столбца \"" + dt.GroupRefColumnName + "\" в документе \"" + dt.Name + "\", которое задается свойством GroupRefColumnName");
          if (String.IsNullOrEmpty(ColDef.MasterTableName))
            throw new BugException("Столбец \"" + dt.GroupRefColumnName + "\" документа \"" + dt.Name + " не является ссылочным");
          DBxDocType dt2 = DocProvider.DocTypes[ColDef.MasterTableName];
          if (dt2 == null)
            throw new BugException("Неизвестный вид документов \"" + ColDef.MasterTableName + "\" в документе \"" + dt.Name + "\" для группировки");

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

      internal DocTypeList(DBUI owner)
      {
        _Owner = owner;
        _Items = new Dictionary<string, DocTypeUI>(owner.DocProvider.DocTypes.Count);
      }

      #endregion

      #region Свойства

      private DBUI _Owner;

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
            if (!_Owner.DocProvider.DocTypes.Contains(docTypeName))
              throw new ArgumentException("Неизвестный тип документов \"" + docTypeName + "\"", "docTypeName");

            if (_Owner._GroupDocList.Contains(docTypeName))
              res = new GroupDocTypeUI(_Owner, _Owner.DocProvider.DocTypes[docTypeName]);
            else
              res = new DocTypeUI(_Owner, _Owner.DocProvider.DocTypes[docTypeName]);
            _Items.Add(docTypeName, res);

            string GroupName;
            if (_Owner._GroupDocDict.TryGetValue(docTypeName, out GroupName))
            {
              GroupDocTypeUI dt2 = (GroupDocTypeUI)(this[GroupName]);
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
        if (_Owner.DocProvider.DocTypes.FindByTableName(tableName, out docType, out subDocType))
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
        if (_Owner.DocProvider.DocTypes.FindByTableName(tableName, out docType, out subDocType))
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
        DBxDocType DocType = _Owner.DocProvider.DocTypes.FindByTableId(tableId);
        if (DocType != null)
          return this[DocType.Name];
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
      string AboutDocSrcColNames = null;
      if (DocProvider.DocTypes.UseTime)
      {
        AboutDocSrcColNames = "Id,CreateTime,ChangeTime";
        if (DocProvider.DocTypes.UseVersions)
          AboutDocSrcColNames += ",Version";
        if (DocProvider.DocTypes.UseUsers)
          AboutDocSrcColNames += ",CreateUserId,ChangeUserId";
        if (DocProvider.DocTypes.UseDeleted)
          AboutDocSrcColNames += ",Deleted";
      }


      foreach (DocTypeUI dt in DocTypes)
      {
        if (!dt.GridProducer.Columns.Contains("Id"))
        {
          EFPGridProducerColumn IdCol = dt.GridProducer.Columns.AddInt("Id", "Id", 6);
          IdCol.DisplayName = "Идентификатор документа";
          IdCol.CanIncSearch = true;
          // Делаем первым в списке
          dt.GridProducer.Columns.RemoveAt(dt.GridProducer.Columns.Count - 1);
          dt.GridProducer.Columns.Insert(0, IdCol);
        }

        bool HistoryAllowed = DocTypeViewHistoryPermission.GetAllowed(this.DocProvider.UserPermissions, dt.DocType.Name);

        // Информация о документе
        if (HistoryAllowed) // 11.04.2016
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

          if (AboutDocSrcColNames != null)
          {
            EFPGridProducerToolTip ttDocumentInfo = dt.GridProducer.ToolTips.AddUserItem("DocumentInfo", AboutDocSrcColNames,
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
      Int32 ChangeUserId = args.GetInt("ChangeUserId");
      if (ChangeUserId != 0)
        args.Value = GetUserName(ChangeUserId);
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
      Int32 DocId = args.GetInt("Id");
      if (DocId == 0)
      {
        args.Value = "Нет выбранного документа";
        return;
      }

      DocTypeUI dt = (DocTypeUI)(args.Tag);
      StringBuilder sb = new StringBuilder();

      sb.Append("Документ \"");
      sb.Append(dt.DocType.SingularTitle);
      sb.Append("\" Id=");
      sb.Append(DocId);

      if (DocProvider.DocTypes.UseVersions)
      {
        sb.Append(" Версия ");
        sb.Append(args.GetInt("Version"));
      }

      sb.Append(" создан ");
      DateTime? CreateTime = args.GetNullableDateTime("CreateTime");
      if (CreateTime.HasValue)
        sb.Append(CreateTime.Value.ToString());
      if (DocProvider.DocTypes.UseUsers)
      {
        sb.Append(" (");
        sb.Append(TextHandlers.GetTextValue("Пользователи", args.GetInt("CreateUserId")));
        sb.Append(")");
      }
      DateTime? ChangeTime = args.GetNullableDateTime("ChangeTime");
      if (ChangeTime.HasValue)
      {
        bool IsDeleted = false;
        if (DocProvider.DocTypes.UseDeleted)
          IsDeleted = args.GetBool("Deleted");
        if (IsDeleted)
          sb.Append(". Удален ");
        else
          sb.Append(". Изменен ");
        sb.Append(ChangeTime.Value.ToString());
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
    /// Вставка выборки документов из буфера обмена
    /// Если буфер обмена не содержит выборки, возвращается null
    /// Выборка не нормализуется.
    /// Проверяется, что выборка в буфере относится к той же базе данных, что и DocProvider
    /// </summary>
    /// <returns></returns>
    public DBxDocSelection PasteDocSel()
    {
      DBxDocSelection DocSel = null;
      IDataObject dobj = EFPApp.Clipboard.GetDataObject();
      if (dobj != null)
        DocSel = (DBxDocSelection)(dobj.GetData(typeof(DBxDocSelection)));

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
    /// Вставка из буфера обмена единственного идентификатора документа заданного вида. 
    /// Возвращает 0, если в буфере обмена нет таких документов. В этом случае выдаются соответствуюие сообщения пользователю.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа, для которого ожидается идентификатор в буфере обмена</param>
    /// <returns></returns>
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

      DBxDocSelection DocSel = PasteDocSel();
      if (DocSel == null)
      {
        EFPApp.ShowTempMessage("В буфере обмена нет выборки документов");
        return 0;
      }

      Int32 Id = DocSel.GetSingleId(docTypeName);
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
      frm.Icon = EFPApp.MainImageIcon("DBxDocSelection");
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

      DBxDocSelection DocSel = new DBxDocSelection(DocProvider.DBIdentity);
      for (int i = 0; i < docIds.Length; i++)
      {
        if (!DocProvider.IsRealDocId(docIds[i]))
          continue;
        DocSel.Add(docTypeName, docIds[i]);
      }
      return DocSel;
    }

    #endregion

    #region Инициализация изображений

    // TODO: Надо сделать, чтобы InitImages() вызывался автоматически

    /// <summary>
    /// Добавление изображений библиотеки ExtDBDocForms
    /// </summary>
    public static void InitImages()
    {
      if (!EFPApp.IsMainThread)
        throw new InvalidOperationException("Не было вызова EFPApp.InitApp() или вызов не из основного потока приложения");

      if (_ImagesWasInit)
        return;

      DummyForm frm = new DummyForm();
      EFPApp.AddMainImages(frm.MainImageList);
    }

    private static bool _ImagesWasInit = false;

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
      UserActionsReport Report = new UserActionsReport(this);
      Report.ReportParams = reportParams;
      Report.Run();
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
        string DocTypeName = creatorParams.ConfigSectionName;
        DocTableViewForm Form = new DocTableViewForm(this.DocTypes[DocTypeName], DocTableViewMode.Browse);
        return Form;
      }

      if (creatorParams.ClassName == typeof(DocInfoReport).ToString())
      {
        DocInfoReport Report = new DocInfoReport(this);
        return ((IEFPFormCreator)Report).CreateForm(creatorParams);
      }

      if (creatorParams.ClassName == typeof(UserActionsReport).ToString())
      {
        UserActionsReport Report = new UserActionsReport(this);
        return ((IEFPFormCreator)Report).CreateForm(creatorParams);
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

        Int32[] DocIds = docSet[i].DocIds;
        if (DocIds.Length > 0)
          dtui.Browsers.UpdateRowsForIds(DocIds);
      }
    }

    #endregion
  }

  #region Делегат для работы с просмотром

  /// <summary>
  /// Аргументы событий DocTypeUI.InitView и SubDocTypeUI.InitView
  /// </summary>
  public class InitEFPDBxViewEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается объектами DocTypeUI и SubDocTypeUI
    /// </summary>
    /// <param name="controlProvider">Инициализируемый просмотр</param>
    public InitEFPDBxViewEventArgs(IEFPDBxView controlProvider)
    {
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализируемый табличный просмотр
    /// </summary>
    public IEFPDBxView ControlProvider { get { return _ControlProvider; } }
    private IEFPDBxView _ControlProvider;

    /// <summary>
    /// Пользовательские данные
    /// </summary>
    public object UserInitData { get { return _UserInitData; } set { _UserInitData = value; } }
    private object _UserInitData;

    #endregion
  }

  /// <summary>
  /// Делегат событий DocTypeUI.InitView и SubDocTypeUI.InitView
  /// </summary>
  /// <param name="sender">Интерфейс доступа к документам или поддокументам</param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitEFPDBxViewEventHandler(object sender, InitEFPDBxViewEventArgs args);

  /// <summary>
  /// Аргументы события DocTypeUI.InitDocGridView 
  /// </summary>
  public class InitEFPDocSelViewEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается объектами DocTypeUI 
    /// </summary>
    /// <param name="controlProvider">Инициализируемый просмотр</param>
    public InitEFPDocSelViewEventArgs(IEFPDocSelView controlProvider)
    {
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Инициализируемый табличный просмотр выборки документов
    /// </summary>
    public IEFPDocSelView ControlProvider { get { return _ControlProvider; } }
    private IEFPDocSelView _ControlProvider;

    #endregion
  }

  /// <summary>
  /// Делегат события DocTypeUI.InitDocSelView
  /// </summary>
  /// <param name="sender">Интерфейс доступа к документам</param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitEFPDocSelViewEventHandler(object sender, InitEFPDocSelViewEventArgs args);

  #endregion

  #region DocTypeEditingEventHandler

  /// <summary>
  /// Аргументы события DocTypeUI.Editing
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
    public DocTypeEditingEventArgs(DocTypeUI docType, EFPDataGridViewState state, Int32[] editIds, bool modal, DocumentViewHandler caller)
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
    private DocumentViewHandler _Caller;

    /// <summary>
    /// Тип документа
    /// </summary>
    public DocTypeUI DocType { get { return _DocType; } }
    private DocTypeUI _DocType;

    /// <summary>
    /// Режим работы
    /// </summary>
    public EFPDataGridViewState State { get { return _State; } }
    private EFPDataGridViewState _State;

    /// <summary>
    /// Список идентификаторов редактируемых, просматриваемых или
    /// удаляемых документов
    /// </summary>
    public Int32[] EditIds { get { return _EditIds; } }
    private Int32[] _EditIds;

    /// <summary>
    /// True, если окно редактирования следует показать в модальном
    /// режиме и false, если оно должно быть встроено в интерфейс MDI
    /// Поле может быть проигнорировано, если окно всегда выводится
    /// в модальном режиме
    /// </summary>
    public bool Modal { get { return _Modal; } }
    private bool _Modal;

    /// <summary>
    /// Должен быть установлен в true, чтобы предотвратить стандартный вызов редактора документа
    /// </summary>
    public bool Handled { get { return _Handled; } set { _Handled = value; } }
    private bool _Handled;

    /// <summary>
    /// Сюда может быть помещено значение, возвращаемое функкцией 
    /// ClientDocType.PerformEditing(), когда обработчик события Editing устанавливает
    /// Handled=true. Если обработчик оставляет Handled=false для показа формы, то он
    /// может установить HandledResult=true. В этом случае, метод PerformEditing()
    /// вернет true, даже если пользователь не будет редактировать документ
    /// До установки свойства в явном виде, оно имеет значение, совпадающее со
    /// свойством Handled
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
    /// Возвращает объект MultiDocs, загруженный данными или инициализированный
    /// начальными значениями (в режиме Insert).
    /// </summary>
    /// <returns></returns>
    public DBxDocSet CreateDocs()
    {
      DBxDocSet DocSet = new DBxDocSet(DocType.UI.DocProvider);
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
            throw new InvalidOperationException("Должен быть задан единственный идентификатор документа");
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

  /// <summary>
  /// Делегат события DocTypeUI.Editing
  /// </summary>
  /// <param name="sender">Интерфейс пользователя для вида документов</param>
  /// <param name="args">Аргумент события</param>
  public delegate void DocTypeEditingEventHandler(object sender, DocTypeEditingEventArgs args);

  #endregion

  #region DocEditEventHandler

  /// <summary>
  /// Аргументы для нескольких событий класса DocumentEditor
  /// </summary>
  public class DocEditEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создание не должно выполняться в пользовательском коде
    /// </summary>
    /// <param name="editor">Редактор документов</param>
    public DocEditEventArgs(DocumentEditor editor)
    {
      _Editor = editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор основного документа
    /// </summary>
    public DocumentEditor Editor { get { return _Editor; } }
    private DocumentEditor _Editor;

    #endregion
  }

  /// <summary>
  /// Делегат для нескольких событий класса DocumentEditor
  /// </summary>
  /// <param name="sender">Объект - источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DocEditEventHandler(object sender, DocEditEventArgs args);

  /// <summary>
  /// Аргументы событий DocTypeUI.Writing и DocumentEditor.BeforeWrite.
  /// </summary>
  public class DocEditCancelEventArgs : DocEditEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создание аргументов события не должно выполняться в пользовательском коде
    /// </summary>
    /// <param name="editor">Редактор документа</param>
    public DocEditCancelEventArgs(DocumentEditor editor)
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
  /// Делегат событий DocTypeUI.Writing и DocumentEditor.BeforeWrite.
  /// </summary>
  /// <param name="sender">Объект - источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DocEditCancelEventHandler(object sender, DocEditCancelEventArgs args);

  #endregion

  #region BeforeDocEditEventHandler

  /// <summary>
  /// Аргументы события DocTypeUI.BeforeEdit
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
    /// Установка этого поля в true приводит к отказу от работы редактора 
    /// Запись результатов выполяться не будет
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    /// <summary>
    /// Установка этого поля в false приводит к пропуску работы редактора
    /// Сразу выполняется запись результатов
    /// </summary>
    public bool ShowEditor { get { return _ShowEditor; } set { _ShowEditor = value; } }
    private bool _ShowEditor;

    /// <summary>
    /// Возвращает имя столбца, активного в табличном просмотре, из которого документ открыт на редактирование.
    /// Если редактор запускается не из просмотра, или информация о текущем столбце недоступна, возвращается пустая строка
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
  /// Делегат события DocTypeUI.BeforeEdit
  /// </summary>
  /// <param name="sender">Интерфейс документов DocTypeUI</param>
  /// <param name="args">Аргументы события</param>
  public delegate void BeforeDocEditEventHandler(object sender, BeforeDocEditEventArgs args);

  #endregion

  #region DocTypeDocSelEventHandler

  /// <summary>
  /// Аргументы события DocTypeUIBase.GetDocSel
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
    public DocTypeDocSelEventArgs(DBUI ui, DBxDocSelection docSel, string tableName, Int32[] ids, EFPDBxGridViewDocSelReason reason)
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
    public DocTypeDocSelEventArgs(DBUI ui, DBxDocSelection docSel, string tableName, DataRow[] rows, EFPDBxGridViewDocSelReason reason)
    {
      _UI = ui;
      _DocSel = docSel;
      _TableName = tableName;
      _Rows = rows;
      _Ids = DataTools.GetIds(rows);
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной объект пользовательского интерфейса
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    /// <summary>
    /// Имя таблицы документа или поддокумента (к которому присоединен обработчик
    /// события)
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Массив идентификаторов выбранных документов или поддокументов, для которых
    /// требуется построить выборку документов
    /// </summary>
    public Int32[] Ids { get { return _Ids; } }
    private Int32[] _Ids;

    /// <summary>
    /// Массив строк выбранных поддокументов, для которых требуется построить
    /// выборку документов.
    /// Используется только для поддокументов в редакторе документа, когда некоторые
    /// поддокументы еще не записаны
    /// </summary>
    public DataRow[] Rows { get { return _Rows; } }
    private DataRow[] _Rows;

    /// <summary>
    /// Причина, по которой требуется создать выборку
    /// </summary>
    public EFPDBxGridViewDocSelReason Reason { get { return _Reason; } }
    private EFPDBxGridViewDocSelReason _Reason;

    /// <summary>
    /// Сюда должны быть добавлены ссылки на документы
    /// Добавлять ссылки на сами документы (AllIds) не требуется
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private DBxDocSelection _DocSel;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить ссылки из ссылочного поля
    /// </summary>
    /// <param name="refTableName">Имя типа документа, на который ссылается поле</param>
    /// <param name="refColumnName">Имя ссылочного поля. Может содержать точки</param>
    public void AddFromColumn(string refTableName, string refColumnName)
    {
      for (int i = 0; i < Ids.Length; i++)
      {
        Int32 RefId = DataTools.GetInt(GetRowValue(i, refColumnName));
        DocSel.Add(refTableName, RefId);
      }
    }

    /// <summary>
    /// Возвращает значение для поля ColumnName для идентификатора из массива Ids
    /// с индексом RowIndex. Если вызов выполнен из SubDocsGrid и установлено свойство
    /// Rows, то значение извлекается из строки данных с указанным индексом. Иначе
    /// значение извлекается с использованием системы буферизации
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public object GetRowValue(int rowIndex, string columnName)
    {
      if (Rows != null)
      {
        #region Из текущей строки таблицы поддокументов

        int p = columnName.IndexOf('.');
        string BaseColumnName;
        if (p < 0)
          BaseColumnName = columnName;
        else
          BaseColumnName = columnName.Substring(0, p);

        int p2 = Rows[rowIndex].Table.Columns.IndexOf(BaseColumnName);
        if (p2 < 0)
          return null; // нет такого поля
        object BaseValue = Rows[rowIndex][p2];
        if (p < 0)
          return BaseValue;

        // Ссылочное поле с точкой
        Int32 RefId = DataTools.GetInt(BaseValue);
        if (RefId < 0)
          return null; // тоже фиктивный идентификатор
        return _UI.TextHandlers.DBCache[TableName].GetRefValue(columnName, RefId);

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
        Int32 TableId = DataTools.GetInt(GetRowValue(i, prefix + "Таблица"));
        Int32 DocId = DataTools.GetInt(GetRowValue(i, prefix + "Идентификатор"));
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
  /// Делегат события DocTypeUIBase.GetDocSel
  /// </summary>
  /// <param name="sender">Объект DocTypeUI или SubDocTypeUI</param>
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
  /// Базовый класс для DocTypeUI и SubDocTypeUI
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
    private DBUI _UI;

    /// <summary>
    /// Описание вида документа или поддокумента
    /// </summary>
    public DBxDocTypeBase DocTypeBase { get { return _DocTypeBase; } }
    private DBxDocTypeBase _DocTypeBase;

    /// <summary>
    /// Разрешение пользователя на доступ к таблице документа или поддокумента.
    /// Для документов в DocTypeUI есть свойство DocTypePermissonMode
    /// </summary>
    public DBxAccessMode TableMode { get { return _UI.DocProvider.DBPermissions.TableModes[_DocTypeBase.Name]; } }

    /// <summary>
    /// Возвращает DocTypeName или SubDocTypeName
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
    /// Используйте метод AddImageHandler(), если требуется обработчик
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
    /// <param name="imageKey">Имя основного изображения в списке EFPApp.MainImages</param>
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
    /// <param name="imageKey">Имя основного изображения в списке EFPApp.MainImages</param>
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
    /// Возвращает имя изображения из EFPApp.MainImages.
    /// Если значок не был задан в DBxDocImageHandlers в явном виде, возвращает "Item".
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
    /// Возвращает имя изображения из EFPApp.MainImages.
    /// Если значок не был задан в DBxDocImageHandlers в явном виде, возвращает "Table".
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
    /// <returns>Изображение в списке EFPApp.MainImages</returns>
    public Image GetImageValue(Int32 id)
    {
      return EFPApp.MainImages.Images[GetImageKey(id)];
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <returns>Имя изображения в списке EFPApp.MainImages</returns>
    public string GetImageKey(Int32 id)
    {
      return _UI.ImageHandlers.GetImageKey(_DocTypeBase.Name, id);
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора документа
    /// </summary>
    /// <param name="row">Строка с частью заполненных полей документа или поддокумента</param>
    /// <returns>Изображение в списке EFPApp.MainImages</returns>
    public Image GetImageValue(DataRow row)
    {
      return EFPApp.MainImages.Images[GetImageKey(row)];
    }

    /// <summary>
    /// Получить изображение для заданного идентификатора документа, извлекаемого из строки данных
    /// </summary>
    /// <param name="row">Строка с частью заполненных полей документа или поддокумента</param>
    /// <returns>Имя изображения в списке EFPApp.MainImages</returns>
    public string GetImageKey(DataRow row)
    {
      return _UI.ImageHandlers.GetImageKey(_DocTypeBase.Name, row);
    }

    /// <summary>
    /// Получить раскраску строки докаумента или поддокумента с заданным идентификатором
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="colorType">Сюда помещается цвет строки в справочнике</param>
    /// <param name="grayed">Сюда помещается true, если запись должна быть отмечена серым цветом</param>
    public void GetRowColor(Int32 id, out EFPDataGridViewColorType colorType, out bool grayed)
    {
      _UI.ImageHandlers.GetRowColor(_DocTypeBase.Name, id, out colorType, out grayed);
    }

    /// <summary>
    /// Создает EFPReportFilterItem для добавления в табличку фильтра.
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
    private ColumnUIList _Columns;

    #endregion

    #region Создание выборки документов

    /// <summary>
    /// Это событие вызывается при запросе копирования строк табличного просмотра
    /// документа в буфер обмена или выполнении команды "Отправить - выборка документов".
    /// Также используется при копировании ссылки в EFPDocComboBox
    /// Используйте это событие вместо установки обработчика EFPDataGridView.CommandItems.GetDocSel
    /// при инициализации табличного просмотра в InitGrid
    /// Ссылки на непосредственно выбранные документы добавляются автоматически
    /// </summary>
    public event DocTypeDocSelEventHandler GetDocSel;

    /// <summary>
    /// Возвращает true, если обработчик события GetDocSel установлен
    /// </summary>
    public bool HasGetDocSel { get { return GetDocSel != null; } }

    /// <summary>
    /// Создать выборку документов.
    /// В выборку попадают выбранные документы с идентификаторами AllIds.
    /// Если задан обработчик события GetDocSel, то будут добавлены связанные
    /// документы, на которые есть ссылочные поля
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="ids">Идентификаторы выбранных документов или поддокументов</param>
    /// <param name="reason">Причина построения выборки</param>
    /// <returns>Выборка</returns>
    public abstract void PerformGetDocSel(DBxDocSelection docSel, Int32[] ids, EFPDBxGridViewDocSelReason reason);

    /// <summary>
    /// Вызывает обработчик события GetDocSel, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="ids">Идентификаторы выбранных документов или поддокументов</param>
    /// <param name="reason">Причина построения выборки</param>
    protected void OnGetDocSel(DBxDocSelection docSel, Int32[] ids, EFPDBxGridViewDocSelReason reason)
    {
      if (GetDocSel != null)
      {
        // Есть обработчик события
        DocTypeDocSelEventArgs Args = new DocTypeDocSelEventArgs(_UI, docSel, _DocTypeBase.Name, ids, reason);
        try
        {
          GetDocSel(this, Args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка создания выборки связанных документов");
        }
      }
    }

    /// <summary>
    /// Вызывает обработчик события GetDocSel, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="rows">Строки таблицы документов или поддокументов, откуда извлекаются идентификаторы</param>
    /// <param name="reason">Причина построения выборки</param>
    protected void OnGetDocSel(DBxDocSelection docSel, DataRow[] rows, EFPDBxGridViewDocSelReason reason)
    {
      if (GetDocSel != null)
      {
        // Есть обработчик события
        DocTypeDocSelEventArgs Args = new DocTypeDocSelEventArgs(_UI, docSel, _DocTypeBase.Name, rows, reason);
        try
        {
          GetDocSel(this, Args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка создания выборки связанных документов");
        }
      }
    }

    /// <summary>
    /// Получить выборку документов.
    /// Вызывает обработчик события GetDocSel, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="reason">Причина построения выборки</param>
    public void PerformGetDocSel(DBxDocSelection docSel, Int32 id, EFPDBxGridViewDocSelReason reason)
    {
      if (id != 0)
        PerformGetDocSel(docSel, new Int32[1] { id }, reason);
    }

    /// <summary>
    /// Получить выборку документов.
    /// Вызывает обработчик события GetDocSel, если он установлен.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="ids">Список идентификаторов документа или поддокумента</param>
    /// <param name="reason">Причина построения выборки</param>
    public void PerformGetDocSel(DBxDocSelection docSel, IdList ids, EFPDBxGridViewDocSelReason reason)
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
    private DBxDocType _DocType;

    /// <summary>
    /// True, если допускается одновременное редактирование или
    /// просмотр нескольких выбранных документов. По умолчанию -
    /// false (нельзя)
    /// </summary>
    public bool CanMultiEdit { get { return _CanMultiEdit; } set { _CanMultiEdit = value; } }
    private bool _CanMultiEdit;

    /// <summary>
    /// Изображение, асоциированное с документом данного вида
    /// </summary>
    /// <summary>
    /// true, если разрешено создание нового документа на основании существующего
    /// </summary>
    public bool CanInsertCopy { get { return _CanInsertCopy; } set { _CanInsertCopy = value; } }
    private bool _CanInsertCopy;

    /// <summary>
    /// Разрешение на вид документов, заданное с помощью DBxDocTypePermission.
    /// Возвращаемое значение может отличаться от свойства TableMode.
    /// У пользователя может не быть разрешения на вид документа, но быть разрешение на таблицу.
    /// В большинстве случаев следует использовать свойство TableMode.
    /// Вызывает метод DocTypePermission.GetAccessMode().
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
    /// Обычно в прикладном коде сюда следует добавить описания столбцов
    /// </summary>
    public EFPDocTypeGridProducer GridProducer { get { return _GridProducer; } }
    private EFPDocTypeGridProducer _GridProducer;

    #endregion

    #region Обновление DocProvider

    /// <summary>
    /// Вызывается при смене свойства DBUI.DocProvider
    /// </summary>
    internal void OnDocProviderChanged()
    {
      DBxDocType NewDocType = UI.DocProvider.DocTypes[_DocType.Name];
      if (NewDocType == null)
        throw new NullReferenceException("Не найден новый DocType для DocTypeName=\"" + _DocType.Name + "\"");
      _DocType = NewDocType;
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
          return "Все документы";
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
          return "Документы без иерархии";
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
    /// Список для реализации свойства DocTypeUI.SubDocTypes
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

      private DocTypeUI _Owner;

      internal Dictionary<string, SubDocTypeUI> Items { get { return _Items; } }
      private Dictionary<string, SubDocTypeUI> _Items;

      /// <summary>
      /// Доступ к интерфейса поддокумента по имени таблицы поддокумента.
      /// Если запрошен несуществующий вид поддокумента, которого нет в списке DBxDocType.SubDocs
      /// (или поддокумент относится к другому документу), генерируется исключение.
      /// </summary>
      /// <param name="subDocTypeName">Имя таблицы поддокумента</param>
      /// <returns>Интерфейс видов поддокументов</returns>
      public SubDocTypeUI this[string subDocTypeName]
      {
        get
        {
          SubDocTypeUI Res;
          if (!_Items.TryGetValue(subDocTypeName, out Res))
          {
            if (String.IsNullOrEmpty(subDocTypeName))
              throw new ArgumentNullException("subDocTypeName");
            if (!_Owner.DocType.SubDocs.Contains(subDocTypeName))
              throw new ArgumentException("Неизвестный тип поддокументов \"" + subDocTypeName + "\"", "subDocTypeName");

            Res = new SubDocTypeUI(_Owner, _Owner.DocType.SubDocs[subDocTypeName]);
            _Items.Add(subDocTypeName, Res);
          }
          return Res;
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
    private SubDocTypeList _SubDocTypes;

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
    /// Буферизация используется, если свойство DataBuffering установлено в true.
    /// Всегда создает новый объект DataView, DataTable.DefaultView не используется.
    /// Рекомендуется вызывать DataView.Dispose() сразу по окончании использования.
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
    /// Буферизация используется, если свойство DataBuffering установлено в true.
    /// Можно задать ограничение на максимальное число записей (игнорируется при DataBuffering=true).
    /// Если ограничение сработало, то у таблицы DataView.Table выставляется свойство ExtendedProperties "Limited"
    /// Всегда создает новый объект DataView, DataTable.DefaultView не используется.
    /// Рекомендуется вызывать DataView.Dispose() сразу по окончании использования.
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

      DBxColumnList ColLst = new DBxColumnList(columns);
      if (DataBuffering)
      {
        if (orderBy != null)
        {
          // Требуется, чтобы поле сортировки присутствовало в выборке
          orderBy.GetColumnNames(ColLst);
        }
        if ((!showDeleted) && UI.DocProvider.DocTypes.UseDeleted)
        {
          DBxFilter Filter2 = DBSDocType.DeletedFalseFilter;
          if (filter == null)
            filter = Filter2;
          else
            filter = new AndFilter(filter, Filter2);
        }
        if (filter != null)
          filter.GetColumnNames(ColLst); // 09.01.2018
        DataTable dt = GetBufferedData(new DBxColumns(ColLst));
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
          throw new BugException("Таблица " + dv.Table.TableName + " имеет поле Id неправильного типа " + dv.Table.Columns[p].DataType.ToString());
      }
#endif

      return dv;
    }

    /// <summary>
    /// Обобщенный метод получения буферизованных или небуферизованных данных.
    /// Буферизация используется, если свойство DataBuffering установлено в true.
    /// Эта версия загружает все документы, кроме удаленных. 
    /// Всегда создает новый объект DataView, DataTable.DefaultView не используется.
    /// Рекомендуется вызывать DataView.Dispose() сразу по окончании использования.
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
    /// свойства DataBuffering. Выполняет непосредственное обращение к серверу.
    /// Добавляет к списку полей <paramref name="columns"/>, если он задан, поле "Id".
    /// Если <paramref name="showDeleted"/>=false, то добавляется фильт по полю "Deleted" (если DBxDocTypes.UseDeleted=true).
    /// Этот метод, в основном, предназначен для внутреннего использования.
    /// В прикладном коде обычно следует использовать вызовы DBxDocProvider.FillSelect(), которые не выполняют дополнительных действий с запросом.
    /// </summary>
    /// <param name="columns">Требуемые поля</param>
    /// <param name="filter">Условия фильтрации строк или null</param>
    /// <param name="showDeleted">true, если надо показать удаленные строки</param>
    /// <param name="orderBy">Порядок сортировки строк</param>
    /// <returns>Таблица DataTable</returns>
    public DataTable GetUnbufferedData(DBxColumns columns, DBxFilter filter, bool showDeleted, DBxOrder orderBy)
    {
      return GetUnbufferedData(columns, filter, showDeleted, orderBy, 0);
    }

    /// <summary>
    /// Получение документов с сервера без использования буферизации, независимо от
    /// свойства DataBuffering. Выполняет непосредственное обращение к серверу.
    /// Добавляет к списку полей <paramref name="columns"/>, если он задан, поле "Id".
    /// Если <paramref name="showDeleted"/>=false, то добавляется фильт по полю "Deleted" (если DBxDocTypes.UseDeleted=true).
    /// Если задано ограничение на число записей <paramref name="maxRecordCount"/>, то через дополнительное свойство DataTable.ExtendeedProperties с
    /// именем "Limited" возвращается true, если лимит был превышен.
    /// Этот метод, в основном, предназначен для внутреннего использования.
    /// В прикладном коде обычно следует использовать вызовы DBxDocProvider.FillSelect(), которые не выполняют дополнительных действий с запросом.
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
        DBxFilter Filter2 = DBSDocType.DeletedFalseFilter;
        if (filter == null)
          filter = Filter2;
        else
          filter = new AndFilter(filter, Filter2);
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
      info.Expressions.Add(columns);
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

      DBxColumnList ColLst = new DBxColumnList(columns);
      ColLst.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted)
        ColLst.Add("Deleted");

      if (_BufferedData != null)
      {
        // Проверяем, все ли поля есть
        if (ColLst.HasMoreThan(_BufferedColumns))
        {
          // Некоторых полей не хватает
          _BufferedData = null;
          ColLst.Add(_BufferedColumns);
        }
      }

      if (_BufferedData == null)
      {
        _BufferedData = UI.DocProvider.FillSelect(DocType.Name, new DBxColumns(ColLst), null);
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
    /// BufferedData данные будут снова загружены с сервера
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
    /// <param name="userInitData">Свойство Args.Tag, передаваемое обработчику InitGrid (если он установлен)</param>
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
          EFPApp.ShowException(e, "Ошибка очистки списка столбцов");
        }
      }
      // Добавляем столбец с картинками
      // 27.05.2015
      // В первоначальном варианте столбец с картинками добавлялся после GridProducer.InitGrid() с помощью Insert()
      // Возникала непонятная ошибка при вызове Form.Dispose(), если DataGridView находится на закладке TabPage и
      // ни разу не отображается
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn ImgCol = new DataGridViewImageColumn();
        ImgCol.Name = "Image";
        ImgCol.HeaderText = String.Empty;
        ImgCol.ToolTipText = "Значок документа \"" + DocType.SingularTitle + "\"";
        ImgCol.Width = controlProvider.Measures.ImageColumnWidth;
        ImgCol.FillWeight = 1; // 08.02.2017
        ImgCol.Resizable = DataGridViewTriState.False;
        //string ImgName = SingleDocImageKey;
        //ImgCol.Image = EFPApp.MainImages.Images[ImgName];
        // ImgCol_CellToopTextNeeded
        controlProvider.Control.Columns.Add(ImgCol);
      }
      if (controlProvider.MarkRowIds != null)
        controlProvider.AddMarkRowsColumn();

      //GridProducer.InitGrid(controlProvider, reInit, controlProvider.CurrentConfig, columns);
      controlProvider.GridProducer.InitGridView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021

      controlProvider.FrozenColumns = controlProvider.CurrentConfig.FrozenColumns + (EFPApp.ShowListImages ? 1 : 0);

      if (!reInit)
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

        //string ControlName = ControlProvider.Control.Name;

        CallInitView(controlProvider, userInitData);
      }

      columns.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted)
        columns.Add("Deleted"); //,CheckState,CheckTime";


      //AccDepClientExec.AddGridDebugIdColumn(DocGridHandler.MainGrid);
      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        //Columns += "CreateTime,ChangeTime";
        controlProvider.Columns.AddInt("Id");
        controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        controlProvider.Columns.LastAdded.CanIncSearch = true;
        //TODO: if (UseHieView)
        //TODO: {
        //TODO:   ControlProvider.Columns.AddInt("GroupId");
        //TODO:   ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        //TODO: }

        // TODO: ControlProvider.Columns.AddInt("ImportId");
        // TODO: ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

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

      if (!reInit)
      {
        controlProvider.Control.VirtualMode = true;
        // значков состояния документа нет. ControlProvider.UseRowImages = true; 
        controlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ControlProvider_GetRowAttributes);
        //if (CondFields != null)
        //  ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributesForCond);
        if (EFPApp.ShowListImages)
          controlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ControlProvider_GetCellAttributes);
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
    /// для добавления столбцов. Если обработчик не установлен, выполняется
    /// инициализация по умолчанию.
    /// Событие вызывается однократно. При изменении настроек просмотра не вызывается.
    /// </summary>
    public event InitEFPDBxViewEventHandler InitView;

    private void CallInitView(IEFPDBxView controlProvider, object userInitData)
    {
      if (InitView != null)
      {
        try
        {
          InitEFPDBxViewEventArgs Args = new InitEFPDBxViewEventArgs(controlProvider);
          Args.UserInitData = userInitData;
          InitView(this, Args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка обработчика события InitView");
        }
      }
    }

    /// <summary>
    /// Вызывается при инициализации таблицы просмотра выборки документов.
    /// В отличие от основного события InitView, обработчик этого события не должен добавлять фильтры в просмотр.
    /// Обработчик может добавить команды локального меню, например, для группового добавления ссылок на документы в выборку из других документов.
    /// </summary>
    public event InitEFPDocSelViewEventHandler InitDocSelView;

    /// <summary>
    /// Вызывает обработчик события InitDocSelView, если он установлен, с перехватом ошибок
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
          InitEFPDocSelViewEventArgs Args = new InitEFPDocSelViewEventArgs(controlProvider);
          InitDocSelView(this, Args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка обработчика события InitDocSelView");
        }
      }
    }

    #endregion

    #region Оформление просмотра

    void ControlProvider_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      DataRow Row = args.DataRow;
      if (Row == null)
        return;

      // 24.11.2017
      // Вызываем пользовательский обработчик и для удаленных документов
      //if (DataTools.GetBool(Row, "Deleted"))
      //  Args.Grayed = true;
      //else
      //{
      EFPDataGridViewColorType ColorType;
      bool Grayed;
      UI.ImageHandlers.GetRowColor(DocType.Name, Row, out ColorType, out Grayed);
      args.ColorType = ColorType;
      args.Grayed = Grayed;
      //}
#if XXX
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

    void ControlProvider_GetRowAttributesForCond(object sender, EFPDataGridViewRowAttributesEventArgs args)
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

    void ControlProvider_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnName == "Image")
      {
        DataRow Row = args.DataRow;
        if (Row == null)
          return;

        switch (args.Reason)
        {
          case EFPDataGridViewAttributesReason.View:
            args.Value = GetImageValue(Row);
            break;
          case EFPDataGridViewAttributesReason.ToolTip:
            if (args.ControlProvider.CurrentConfig != null)
            {
              if (args.ControlProvider.CurrentConfig.CurrentCellToolTip)
              {
                string s1 = GetToolTipText(Row);
                args.ToolTipText = DataTools.JoinNotEmptyStrings(Environment.NewLine, new string[] { s1, args.ToolTipText }); // 06.02.2018
              }

              if (args.ControlProvider.CurrentConfig.ToolTips.Count > 0)
              {
                string s2 = null;
                try
                {
                  EFPDataViewRowInfo rowInfo = args.ControlProvider.GetRowInfo(args.RowIndex);
                  s2 = GridProducer.ToolTips.GetToolTipText(args.ControlProvider.CurrentConfig, rowInfo);
                  args.ControlProvider.FreeRowInfo(rowInfo);
                }
                catch (Exception e)
                {
                  s2 = "Ошибка: " + e.Message;
                }
                args.ToolTipText = DataTools.JoinNotEmptyStrings(EFPGridProducerToolTips.ToolTipTextSeparator, new string[] { args.ToolTipText, s2 });
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
      int CheckState = DataTools.GetInt(Row, "CheckState");
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
      //Int32 Id = DataTools.GetInt(Args.Row, "Id");
      //Args.ToolTipText = GetToolTipText(Id);
    }

    #endregion

    #endregion

    #region Список столбцов для табличного просмотра

    /// <summary>
    /// Получить список столбцов, необходимых для табличного просмотра с заданной конфигурации
    /// Заполняется такой же список столбов, как и в методе DoInitGrid(), но без создания самого просмотра
    /// </summary>
    /// <param name="columns">Заполняемый список столбцов</param>
    /// <param name="config">Конфигурация табличного просмотра. Если null, то используется конфигурация по умолчанию</param>
    public void GetColumnNames(DBxColumnList columns, EFPDataGridViewConfig config)
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
    /// <param name="userInitData"></param>
    public void PerformInitTree(EFPDocTreeView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      if (!reInit)
      {
        //if (CanMultiEdit)
        //  ControlProvider.CanMultiEdit = true;
        if (CanInsertCopy)
          controlProvider.CanInsertCopy = true;
      }
      else
        controlProvider.Control.Columns.Clear();

      //GridProducer.InitTree(controlProvider, reInit, controlProvider.CurrentConfig, columns);
      controlProvider.GridProducer.InitTreeView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021
      TreeViewCachedValueAdapter.InitColumns(controlProvider, UI.TextHandlers.DBCache, GridProducer);

      controlProvider.SetColumnsReadOnly(true);

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
        CallInitView(controlProvider, userInitData);
      }
#if XXX
      // Добавляем столбец с картинками
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn ImgCol = new DataGridViewImageColumn();
        ImgCol.Name = "Image";
        ImgCol.HeaderText = String.Empty;
        ImgCol.ToolTipText = "Значок документа \"" + DocType.SingularTitle + "\"";
        ImgCol.Width = ControlProvider.Measures.ImageColumnWidth;
        ImgCol.Resizable = DataGridViewTriState.False;
        //string ImgName = SingleDocImageKey;
        //ImgCol.Image = EFPApp.MainImages.Images[ImgName];
        if (ControlProvider.Control.Columns.Count > 0 && ControlProvider.Control.Columns[0].Frozen)
          ImgCol.Frozen = true;
        ControlProvider.Control.Columns.Insert(0, ImgCol);
        if (EFPApp.ShowToolTips)
          ControlProvider.Columns[0].CellToopTextNeeded += new EFPDataGridViewCellToolTipTextNeededEventHandler(ImgCol_CellToopTextNeeded);
      }
#endif
      columns.Add("Id");
      if (UI.DocProvider.DocTypes.UseDeleted) // 16.05.2018
        columns.Add("Deleted"); //,CheckState,CheckTime";
      columns.Add(DocType.TreeParentColumnName);

      //Columns += "CreateTime,ChangeTime";
      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        controlProvider.Columns.AddInt("Id", true, "Id", 6);
        // пока нельзя сделать
        // controlProvider.Columns.LastAdded.CanIncSearch = true;
      }
      //TODO: if (UseHieView)
      //TODO: {
      //TODO:   ControlProvider.Columns.AddInt("GroupId");
      //TODO:   ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
      //TODO: }

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

      if (!reInit)
      {
        //ControlProvider.Control.VirtualMode = true;
        //ControlProvider.UseRowImages = true;
        //ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ControlProvider_GetRowAttributes);
        //if (CondFields != null)
        //  ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributesForCond);
        //if (EFPApp.ShowListImages)
        //  controlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ControlProvider_GetCellAttributes);
        //DocGridHandler.MainGrid.CellPainting += new DataGridViewCellPaintingEventHandler(TheGrid_CellPainting);
        //DocGridHandler.MainGrid.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(TheGrid_CellToolTipTextNeeded);

        // Добавляем команды локального меню
        InitCommandItems(controlProvider);
        controlProvider.GetDocSel += new EFPDBxTreeViewDocSelEventHandler(DocTree_GetDocSel);

        // TODO: ControlProvider.UserConfigModified = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      //      if (ReInit)
      //        FToolTipExtractor = null;
    }

    #endregion


    void DocTree_GetDocSel(object sender, EFPDBxTreeViewDocSelEventArgs args)
    {
      Int32[] Ids = DataTools.GetIdsFromField(args.DataRows, "Id");
      PerformGetDocSel(args.DocSel, Ids, args.Reason);
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
        RecalcColumnsPermissionMode Mode = RecalcColumnsPermission.GetMode(UI.DocProvider.UserPermissions);

        if (Mode != RecalcColumnsPermissionMode.Disabled)
        {
          EFPCommandItem MenuRecalcDocuments = new EFPCommandItem("Service", "RecalcDocsMenu");
          MenuRecalcDocuments.MenuText = "Пересчитать вычисляемые поля";
          MenuRecalcDocuments.Usage = EFPCommandItemUsage.Menu;
          MenuRecalcDocuments.ImageKey = "RecalcColumns";
          MenuRecalcDocuments.Enabled = !controlProvider.ReadOnly;
          controlProvider.CommandItems.Add(MenuRecalcDocuments);

          ci = new EFPCommandItem("Service", "RecalcSelectedDocs");
          ci.MenuText = "Выделенные";
          ci.Parent = MenuRecalcDocuments;
          ci.Tag = controlProvider;
          ci.Click += new EventHandler(RecalcSelDocuments_Click);
          controlProvider.CommandItems.Add(ci);

          ci = new EFPCommandItem("Service", "RecalcDocsInView");
          ci.MenuText = "Все в просмотре";
          ci.Parent = MenuRecalcDocuments;
          ci.Tag = controlProvider;
          ci.Click += new EventHandler(RecalcViewDocuments_Click);
          controlProvider.CommandItems.Add(ci);

          if (Mode == RecalcColumnsPermissionMode.All)
          {
            ci = new EFPCommandItem("Service", "RecalcAllDocs");
            ci.MenuText = "Все существующие";
            ci.Parent = MenuRecalcDocuments;
            ci.Tag = controlProvider;
            ci.Click += new EventHandler(RecalcAllDocuments_Click);
            controlProvider.CommandItems.Add(ci);
          }
        }
      }

      ci = new EFPCommandItem("View", "DocInfo");
      ci.MenuText = "Информация о документе";
      ci.ToolTipText = "Информация о документе";
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

      Int32[] DocIds = ControlProvider.SelectedIds;
      this.RecalcColumns(DocIds, new PerformRefreshDelegate(ControlProvider.PerformRefresh));
    }

    void RecalcViewDocuments_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      IEFPDBxView ControlProvider = (IEFPDBxView)(ci.Tag);

      if (ControlProvider.SourceAsDataView == null)
      {
        EFPApp.ShowTempMessage("Набор данных не присоединен к табличному просмотру");
        return;
      }

      Int32[] DocIds = DataTools.GetIdsFromField(ControlProvider.SourceAsDataView, "Id");
      this.RecalcColumns(DocIds, new PerformRefreshDelegate(ControlProvider.PerformRefresh));
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
      Int32[] Ids = DataTools.GetIdsFromField(args.DataRows, "Id");
      PerformGetDocSel(args.DocSel, Ids, args.Reason);
    }

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

    private void ShowDocInfoItem_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;

      IEFPDBxView ControlProvider = (IEFPDBxView)(ci.Tag);

      Int32 DocId = ControlProvider.CurrentId;
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("Документ в табличном просмотре не выбран");
        return;
      }

      ShowDocInfo(DocId);
    }

    #endregion

    #region Список открытых табличных просмотров

    /// <summary>
    /// Список открытых табличных просмотров для данного типа документов
    /// </summary>
    public DocumentViewHandlerList Browsers { get { return _Browsers; } }
    private DocumentViewHandlerList _Browsers;

    #endregion

    #region Создание команд меню

    /// <summary>
    /// Команда главного меню "Журналы" или "Справочники".
    /// Свойство устанавливается вызовом CreateMainMenuItem(), а до этого содержит null
    /// </summary>
    public EFPCommandItem MainMenuCommandItem { get { return _MainMenuCommandItem; } set { _MainMenuCommandItem = value; } }
    private EFPCommandItem _MainMenuCommandItem;

    /// <summary>
    /// Создает команду главного меню для просмотра справочника или другого вида документа.
    /// Команда сохраняется в свойстве MainMenuCommandItem. После этого происходит автоматическое
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
    /// Если была создана команда главного меню (свойство MainMenuCommandItem), то метод
    /// инициализирует видимость команды в зависимости от разрений текущего пользователя.
    /// Этот метод вызывается из DBUI.EndInit() и OnDocProviderChanged()
    /// </summary>
    public void InitMainMenuCommandItemVisible()
    {
      if (MainMenuCommandItem == null)
        return;

      DBxAccessMode AccessMode = DBxAccessMode.Full;
      for (int i = UI.DocProvider.UserPermissions.Count - 1; i >= 0; i--)
      {
        UserPermission up = UI.DocProvider.UserPermissions[i];
        DocTypePermission dtp = up as DocTypePermission;
        if (dtp != null)
        {
          if (dtp.DocTypeNames == null)
          {
            AccessMode = dtp.Mode;
            break;
          }
          else if (DataTools.IndexOf(dtp.DocTypeNames, DocType.Name, true) >= 0)
          {
            AccessMode = dtp.Mode;
            break;
          }
        }
        WholeDBPermission dbp = up as WholeDBPermission;
        if (dbp != null)
        {
          AccessMode = dbp.Mode;
          break;
        }
      }

      MainMenuCommandItem.Visible = (AccessMode != DBxAccessMode.None);
    }

    #endregion

    #region Окно просмотра списка документов

    /// <summary>
    /// Открытие новой или активация существующей формы просмотра документов с заданными фильтрами
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
    public DocTableViewForm ShowOrOpen(GridFilters externalFilters, Int32 currentDocId, string formSearchKey)
    {
      bool HasExternalFilters = false;
      if (externalFilters != null)
        HasExternalFilters = externalFilters.Count > 0;

      if (formSearchKey == null)
      {
        if (HasExternalFilters)
        {
          TempCfg Cfg = new TempCfg();
          externalFilters.WriteConfig(Cfg);
          formSearchKey = Cfg.AsXmlText;
        }
        else
          formSearchKey = String.Empty;
      }

      DocTableViewForm Form = null;
      if (EFPApp.ActiveDialog == null) // 10.03.2016
        Form = FindAndActivate(formSearchKey);
      if (Form == null)
      {
        Form = new DocTableViewForm(this, DocTableViewMode.Browse);
        Form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        Form.FormSearchKey = formSearchKey;

        Form.ExternalFilters = externalFilters;
        if (HasExternalFilters)
          Form.FormProvider.ConfigClassName = String.Empty; // 02.11.2018


        Form.CurrentDocId = currentDocId;
        EFPApp.ShowFormOrDialog(Form);
      }
      else if (currentDocId != 0)
        Form.CurrentDocId = currentDocId;

      return Form;
    }

    /// <summary>
    /// Открытие новой или активация существующей формы просмотра документов с заданными фильтрами.
    /// Эта версия метода использует ключ для поиска, создаваемый из <paramref name="externalFilters"/>
    /// </summary>
    /// <param name="externalFilters">Фильтры, которые должны быть установлены в просмотре. Ксли null,
    /// то используется последняя сохраненная конфигурация фильтров, как если бы просмотр был открыт командой меню.
    /// Аргумент используется только при создании новой формы</param>
    /// <param name="currentDocId">Идентификатор документа, строка которого должна быть выбрана. Если 0, то выбор не
    /// осуществляется. Если документ не проходит условия фильтра, выбор также не осуществляется.</param>
    /// <returns>Объект формы</returns>
    public DocTableViewForm ShowOrOpen(GridFilters externalFilters, Int32 currentDocId)
    {
      return ShowOrOpen(externalFilters, currentDocId, null);
    }

    /// <summary>
    /// Открытие новой или активация существующей формы просмотра документов с заданными фильтрами
    /// Эта версия метода использует ключ для поиска, создаваемый из <paramref name="externalFilters"/>.
    /// Выбор документа не выполняется
    /// </summary>
    /// <param name="externalFilters">Фильтры, которые должны быть установлены в просмотре. Ксли null,
    /// то используется последняя сохраненная конфигурация фильтров, как если бы просмотр был открыт командой меню.
    /// Аргумент используется только при создании новой формы</param>
    /// <returns>Объект формы</returns>
    public DocTableViewForm ShowOrOpen(GridFilters externalFilters)
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
        Form[] Forms = EFPApp.Interface.GetChildForms(true);
        for (int i = 0; i < Forms.Length; i++)
        {
          if (Forms[i] is DocTableViewForm)
          {
            DocTableViewForm frm = (DocTableViewForm)(Forms[i]);
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
    /// </summary>
    /// <returns>Идентификатор документа или 0</returns>
    public Int32 SelectDoc()
    {
      return SelectDoc(String.Empty);
    }

    /// <summary>
    /// Выбор одного документа.
    /// Возвращает идентификатор документа или 0, если выбор не сделан.
    /// </summary>
    /// <param name="title">Заголовок блока диалога</param>
    /// <returns>Идентификатор документа или 0</returns>
    public Int32 SelectDoc(string title)
    {
      int DocId = 0;
      if (SelectDoc(ref DocId, title, false))
        return DocId;
      else
        return 0;
    }

    /// <summary>
    /// Выбор документа из справочника.
    /// Пользователь должен выбрать документ или нажать кнопку "Отмена".
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
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="canBeEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 docId, string title, bool canBeEmpty)
    {
      return SelectDoc(ref docId, title, canBeEmpty, (GridFilters)null);
    }

    /// <summary>
    /// Выбор документа из справочника документов с использованием заданного набора фильтров просмотра,
    /// переопределяющего текущие установки пользователя. Пользователь может выбирать только подходящие
    /// документы, проходящие фильтр, т.к. не может его редактировать.
    /// Используйте класс DocSelectDialog для задания большего количества параметров
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="canBeEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <param name="filters">Фиксированный набор фильтров. Значение null приводит к использованию текущего набора
    /// установленных пользователем фильтров</param>
    /// <returns>true, если пользователь сделал выбор</returns>
    public bool SelectDoc(ref Int32 docId, string title, bool canBeEmpty, GridFilters filters)
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
    /// Испольщуйте класс DocSelectDialog для задания дополнительных параметров
    /// </summary>
    /// <param name="docId">Вход-выход идентификатор выбранного документа</param>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="canBeEmpty">Возможность выбора пустого документа (кнопка "Нет")</param>
    /// <param name="fixedDocIds">Массив идентификаторов документов, из которого можно выбирать</param>
    public bool SelectDoc(ref Int32 docId, string title, bool canBeEmpty, IdList fixedDocIds)
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
    /// Используются текущие настройки фильтров, которые пользователь может менять
    /// </summary>
    /// <returns>Массив идентификаторов выбранных документов или пустой массив, если выбор не сделан</returns>
    public Int32[] SelectDocs()
    {
      return SelectDocs(null, (GridFilters)null);
    }

    /// <summary>
    /// Выбор одного или нескольких документов из справочника документов.
    /// Используются текущие настройки фильтров, которые пользователь может менять
    /// </summary>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <returns>Массив идентификаторов выбранных документов или пустой массив, если выбор не сделан</returns>
    public Int32[] SelectDocs(string title)
    {
      return SelectDocs(title, (GridFilters)null);
    }

    /// <summary>
    /// Выбор одного или нескольких документов из справочника документов с использованием заданного набора фильтров просмотра,
    /// переопределяющего текущие установки пользователя. Пользователь может выбирать только подходящие
    /// документы, проходящие фильтр, т.к. не может его редактировать.
    /// </summary>
    /// <param name="title">Заголовок формы выбора документа</param>
    /// <param name="filters">Фиксированный набор фильтров. Значение null приводит к использованию текущего набора
    /// установленных пользователем фильтров</param>
    /// <returns>Массив идентификаторов выбранных документов или пустой массив, если выбор не сделан</returns>
    public Int32[] SelectDocs(string title, GridFilters filters)
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
        return DataTools.EmptyIds;
    }

    #endregion

    #endregion

    #region Редактирование документов

    /// <summary>                                            
    /// Выполнение создания, редактирования, удаления или просмотра документа 
    /// </summary>
    /// <param name="editIds">Идентификаторы документов в режиме просмотра, редактирования или удаления</param>
    /// <param name="state">Требуемый режим</param>
    /// <param name="modal">True для запуска в модальном режиме</param>
    /// <param name="caller">Обработчик, связанный с табличным просмотром.
    /// Если он задан, то в режиме создания документа будут использованы установленные в просмотре
    /// фильтры для инициализации полей документа.</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (DocumentEditor.DataChanged)</returns>
    public bool PerformEditing(Int32[] editIds, EFPDataGridViewState state, bool modal, DocumentViewHandler caller)
    {
      switch (state)
      {
        case EFPDataGridViewState.Insert:
          break;
        case EFPDataGridViewState.InsertCopy:
          if (editIds.Length != 1)
          {
            EFPApp.ShowTempMessage("Должен быть выбран один документ");
            return false;
          }
          break;

        case EFPDataGridViewState.Delete: // 19.08.2016
          if (editIds.Length < 1)
          {
            EFPApp.ShowTempMessage("Не выбрано ни одного документа");
            return false;
          }
          break;

        default:
          if (editIds.Length < 1)
          {
            EFPApp.ShowTempMessage("Не выбрано ни одного документа");
            return false;
          }
          if (editIds.Length > 1 && (!CanMultiEdit))
          {
            EFPApp.ShowTempMessage("Множественное редактирование документов \"" + DocType.PluralTitle + "\" не допускается");
            return false;
          }
          break;
      }

      bool FixedResult = false;
      if (Editing != null)
      {
        DocTypeEditingEventArgs Args = new DocTypeEditingEventArgs(this, state, editIds, modal, caller);
        Editing(this, Args);
        if (Args.Handled)
          return Args.HandledResult;
        else
          FixedResult = Args.HandledResult;
      }

      // 21.11.2018
      // Активируем уже открытый редактор документов
      if (state != EFPDataGridViewState.Insert && state != EFPDataGridViewState.InsertCopy)
      {
        DBxDocSelection DocSel = UI.CreateDocSelection(DocType.Name, editIds);
        DocumentEditor OldDE = DocumentEditor.FindEditor(DocSel);
        if (OldDE != null)
        {
          DBxDocSelection OldDocSel = OldDE.Documents.DocSelection;
          OldDocSel = new DBxDocSelection(OldDocSel, DocType.Name); // другие виды документов не интересны
          StringBuilder sb = new StringBuilder();
          sb.Append("Редактор уже открыт.");
          if (!OldDocSel.ContainsAll(DocSel))
          {
            sb.Append(Environment.NewLine);
            sb.Append("Вы пытаетесь открыть сразу несколько документов, но не все они присутствуют в открытом редакторе.");
          }
          else if (!DocSel.ContainsAll(OldDocSel))
          {
            sb.Append(Environment.NewLine);
            sb.Append("Открытый редактор использует груповое редактирование документов, в том числе других.");
          }

          if (EFPApp.ActiveDialog == null)
            EFPApp.Interface.CurrentChildForm = OldDE.Form;
          else
          {
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Так как есть открытые диалогове окна, нельзя активировать окно редактора.");
          }

          EFPApp.MessageBox(sb.ToString(), "Повторное открытие редактора");
          return false;
        }
      }

      DocumentEditor de = new DocumentEditor(UI, DocType.Name, state, editIds);
      de.Modal = modal;
      de.Caller = caller;
      de.Run();

      return de.DataChanged || FixedResult;
    }

    /// <summary>
    /// Выполнение создания, редактирования, удаления или просмотра документа 
    /// </summary>
    /// <param name="editIds">Идентификаторы документов в режиме просмотра, редактирования или удаления</param>
    /// <param name="state">Требуемый режим</param>
    /// <param name="modal">True для запуска в модальном режиме</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен (DocumentEditor.DataChanged)</returns>
    public bool PerformEditing(Int32[] editIds, EFPDataGridViewState state, bool modal)
    {
      return PerformEditing(editIds, state, modal, null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра документа
    /// </summary>
    /// <param name="editId">Идентификатор редактируемого документа</param>
    /// <param name="readOnly">True, если требуется просмотр, false-редактирование</param>
    /// <returns>True, если выполнялось редактирование и документ был сохранен</returns>
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
    /// <returns>True, если выполнялось редактирование и документ был сохранен</returns>
    public bool PerformEditing(Int32 editId, bool readOnly, DocumentViewHandler caller)
    {
      if (editId == 0)
      {
        EFPApp.ShowTempMessage("Должен быть выбран один документ");
        return false;
      }

      Int32[] EditIds = new int[1];
      EditIds[0] = editId;

      EFPDataGridViewState State = readOnly ? EFPDataGridViewState.View : EFPDataGridViewState.Edit;
      return PerformEditing(EditIds, State, true, caller);
    }


    /// <summary>
    /// Выполнение редактирования или просмотра одиночного документа.
    /// Если выборка содержит несколько документов данного типа, то показывается выборка с этими документами.
    /// Групповое редактирование, как в PerformEditing(), не применяется
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
    /// Групповое редактирование, как в PerformEditing(), не применяется
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
    /// Групповое редактирование, как в PerformEditing(), не применяется
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <param name="readOnly">true - режим просмотра, а не редактирования.
    /// Игнорируется в режиме просмотра выборки документов</param>
    public void PerformEditingOrShowDocSel(Int32[] docIds, bool readOnly)
    {
      PerformEditingOrShowDocSel(docIds, readOnly, (DocumentViewHandler)null);
    }

    /// <summary>
    /// Выполнение редактирования или просмотра одиночного документа.
    /// Если массив содержит несколько идентификаторов документов, то показывается выборка с этими документами.
    /// Групповое редактирование, как в PerformEditing(), не применяется
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    /// <param name="readOnly">true - режим просмотра, а не редактирования.
    /// Игнорируется в режиме просмотра выборки документов</param>
    /// <param name="caller"></param>
    public void PerformEditingOrShowDocSel(Int32[] docIds, bool readOnly, DocumentViewHandler caller)
    {
      if (docIds == null)
        docIds = DataTools.EmptyIds;

      switch (docIds.Length)
      {
        case 0:
          EFPApp.ShowTempMessage("Нет выбранного документа \"" + DocType.SingularTitle + "\"");
          break;
        case 1:
          PerformEditing(docIds[0], readOnly, caller);
          break;
        default:
          // Показываем как выборку документов
          DBxDocSelection DocSel2 = new DBxDocSelection(UI.DocProvider.DBIdentity, DocType.Name, docIds);
          UI.ShowDocSel(DocSel2, "Выбранные документы \"" + DocType.PluralTitle + "\"");
          break;
      }
    }


    /// <summary>
    /// Открывает окно информации о документе
    /// </summary>
    /// <param name="docId"></param>
    public void ShowDocInfo(Int32 docId)
    {
      if (docId == 0)
      {
        EFPApp.ShowTempMessage("Документ для просмотра информации не выбран");
        return;
      }
      try
      {
        DocInfoReport.PerformShow(this, docId);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Просмотр информации о документе");
      }
    }

    /// <summary>
    /// Инициализация начального значения поля "GroupId" для нового документа.
    /// Вызывается реализациями DocumentViewHandler для просмотров документов.
    /// Если текуший вид документов не имеет связанного дерева групп, метод не выполняет никаких действий.
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

      DBxDocValue Value = newDoc.Values[p];

      if (auxFilterGroupIds.Length == 1)
      {
        // В режиме, когда в фильре выбрана единственная группа (то есть группа самого вложенного уровня
        // или включен флажок "Скрыть документы во вложенных группах"
        Int32 GroupId = auxFilterGroupIds[0];
        Value.SetInteger(GroupId);
      }
      else
      {
        // 10.06.2019
        // В режиме фильтра, когда выбрано несколько групп, или выбран режим "Документы без групп" проверяем,
        // что текущая выбранная в документе группа есть в списке. Если выбрана группа не в фильтре, значение очищается
        Int32 CurrGroupId = Value.AsInteger;
        if (CurrGroupId != 0 && Array.IndexOf<Int32>(auxFilterGroupIds, CurrGroupId) < 0)
          Value.SetNull();
      }
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

    internal void DoBeforeEdit(DocumentEditor editor, out bool cancel, out bool showEditor)
    {
      BeforeDocEditEventArgs Args = new BeforeDocEditEventArgs(editor);
      if (BeforeEdit != null)
        BeforeEdit(this, Args);

      cancel = Args.Cancel;
      showEditor = Args.ShowEditor;
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
        throw new ArgumentException("Попытка инициализации для чужого типа документов", "multiDocs");

      InitDocEditFormEventArgs Args = new InitDocEditFormEventArgs(editor, multiDocs);
      if (InitEditForm != null)
        InitEditForm(this, Args);
    }

    /// <summary>
    /// Вызывается из редактора документа перед записью значений в режимах
    /// Edit, Insert и InsertCopy. На момент вызова значения полей формы переписаны
    /// в поля DocumentEditor.Documents. Обработчик может скорректировать эти значения
    /// (например, не заданные поля).
    /// Также обработчик может отменить запись документов на сервере, установив
    /// Args.Cancel=true. При этом следует вывести сообщение об ошибке. Этот обработчик не может
    /// установить фокус ввода на "плохое" поле в редакторе, как обработчик DocumentEditor.BeforeWrite
    /// </summary>
    public event DocEditCancelEventHandler Writing;

    internal bool DoWriting(DocumentEditor editor)
    {
      if (Writing != null)
      {
        DocEditCancelEventArgs Args = new DocEditCancelEventArgs(editor);
        Writing(this, Args);
        if (Args.Cancel)
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
          DocEditEventArgs Args = new DocEditEventArgs(editor);
          Wrote(this, Args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка при обработке события после записи значений");
        }
      }
    }

    #endregion

    #region Пересчет вычисляемых полей

    /// <summary>
    /// Пересчет вычисляемых полей документов и поддокументов.
    /// Пересчет выполняется асинхронно.
    /// После пересчета полей никаких действий не выполняется. Испольщуйте перегрузки с аргументом "AfterRecalc"
    /// </summary>
    /// <param name="docIds">Идентификаторы документов для пересчета.</param>
    public void RecalcColumns(Int32[] docIds)
    {
      RecalcColumns(docIds, null, null);
    }

    /// <summary>
    /// Пересчет вычисляемых полей документов и поддокументов.
    /// Пересчет выполняется асинхронно.
    /// Эта версия предполагает, что пользовательский обработчик <paramref name="afterRecalc"/> не получает параметров
    /// </summary>
    /// <param name="docIds">Идентификаторы документов для пересчета.</param>
    /// <param name="afterRecalc">Пользовательский метод вызывается, когда пересчет полей выполнен.
    /// Метод вызывается в основном потоке приложения</param>
    public void RecalcColumns(Int32[] docIds, Delegate afterRecalc)
    {
      RecalcColumns(docIds, afterRecalc, null);
    }

    /// <summary>
    /// Пересчет вычисляемых полей документов и поддокументов.
    /// Пересчет выполняется асинхронно.
    /// </summary>
    /// <param name="docIds">Идентификаторы документов для пересчета.</param>
    /// <param name="afterRecalc">Пользовательский метод вызывается, когда пересчет полей выполнен.
    /// Метод вызывается в основном потоке приложения</param>
    /// <param name="afterRecalcParams">Параметры, которые передаются пользовательскому методу <paramref name="afterRecalc"/>.
    /// Значение null задает пересчет всех существующих документов</param>
    public void RecalcColumns(Int32[] docIds, Delegate afterRecalc, params object[] afterRecalcParams)
    {
      NamedValues dispArgs = new NamedValues();
      dispArgs["Action"] = "RecalcColumns";
      dispArgs["DocTypeName"] = DocType.Name;
      dispArgs["DocIds"] = docIds;
      DistributedCallData startData = UI.DocProvider.StartServerExecProc(dispArgs);

      DistributedProcCallItem CallItem = new DistributedProcCallItem(startData);
      CallItem.UserData["AfterRecalc"] = afterRecalc;
      CallItem.UserData["AfterRecalcParams"] = afterRecalcParams;
      CallItem.Finished += new DistributedProcCallEventHandler(RecalcColumnsCallItem_Finished);
      EFPApp.ExecProcList.ExecuteAsync(CallItem);
    }

    void RecalcColumnsCallItem_Finished(object sender, DistributedProcCallEventArgs args)
    {
      Delegate AfterRecalc = (Delegate)(args.Item.UserData["AfterRecalc"]);
      object[] AfterRecalcParams = (object[])(args.Item.UserData["AfterRecalcParams"]);
      if (AfterRecalc != null)
        AfterRecalc.DynamicInvoke(AfterRecalcParams);
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Получение выборки документов текущего вида.
    /// В выборку сразу добавляются все переданные идентификаторы <paramref name="docIds"/>.
    /// Затем вызывается метод OnGetDocSel() для вызова обработчика события.
    /// Если для документа используются группы, то добавляются ссылки на документы вида GroupDocType.
    /// </summary>
    /// <param name="docSel">Выборка документов, обычно пустая, куда добавляются ссылки на документы</param>
    /// <param name="docIds">Массив идентификаторов документов. 
    /// Если null или пустой массив, никаких действий не выполняется</param>
    /// <param name="reason">Причина создания выборки</param>
    public override void PerformGetDocSel(DBxDocSelection docSel, Int32[] docIds, EFPDBxGridViewDocSelReason reason)
    {
      if (docIds == null || docIds.Length == 0)
        return;

      // Ссылки на выбранные документы
      docSel.Add(DocType.Name, docIds);

      base.OnGetDocSel(docSel, docIds, reason);

      // 18.11.2017 Ссылку на группу добавляем в конце выборки
      if (this.GroupDocType != null)
      {
        IdList GroupIds = new IdList();
        for (int i = 0; i < docIds.Length; i++)
          GroupIds.Add(TableCache.GetInt(docIds[i], DocType.GroupRefColumnName));
        docSel.Add(this.GroupDocType.DocType.Name, GroupIds);
      }
    }

    /// <summary>
    /// Создать выборку для заданных документов.
    /// Массив идентификаторов может содержать нулевые и фиктивные идентификаторы, которые пропускаются 
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов. Фиктивные и нулевые идентификаторы игнорируются</param>
    /// <returns>Выборка документов одного вида</returns>
    public DBxDocSelection CreateDocSelection(Int32[] docIds)
    {
#if DEBUG
      if (docIds == null)
        throw new ArgumentNullException("docIds");
#endif

      DBxDocSelection DocSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      for (int i = 0; i < docIds.Length; i++)
      {
        if (!UI.DocProvider.IsRealDocId(docIds[i]))
          continue;
        DocSel.Add(DocType.Name, docIds[i]);
      }
      return DocSel;
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


  #region SubDocEditEventHandler

  /// <summary>
  /// Аргументы событий, связанных с редактированием поддкокументы
  /// </summary>
  public class SubDocEditEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Не создается в пользовательском коде
    /// </summary>
    /// <param name="editor">Редактор поддокумента</param>
    public SubDocEditEventArgs(SubDocumentEditor editor)
    {
      _Editor = editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор поддокумента
    /// </summary>
    public SubDocumentEditor Editor { get { return _Editor; } }
    private SubDocumentEditor _Editor;

    /// <summary>
    /// Редактор основного документа
    /// </summary>
    public DocumentEditor MainEditor { get { return _Editor.MainEditor; } }

    #endregion
  }

  /// <summary>
  /// Делегаты событий, связанных с редактированием поддкокументы
  /// </summary>
  /// <param name="sender">Вызывающий объект</param>
  /// <param name="args">Аргументы события</param>
  public delegate void SubDocEditEventHandler(object sender, SubDocEditEventArgs args);

  /// <summary>
  /// Аргументы события SubDocumentEditor.BeforeWrite
  /// </summary>
  public class SubDocEditCancelEventArgs : SubDocEditEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Не создается в пользовательском коде.
    /// </summary>
    /// <param name="editor">Редактор поддокумента</param>
    public SubDocEditCancelEventArgs(SubDocumentEditor editor)
      : base(editor)
    {
      _Cancel = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если это свойство установить в true, до действие будет отменено
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    #endregion
  }

  /// <summary>
  /// Делегат события SubDocumentEditor.BeforeWrite
  /// </summary>
  /// <param name="sender">Редактор поддокумента</param>
  /// <param name="args">Аргументы события</param>
  public delegate void SubDocEditCancelEventHandler(object sender, SubDocEditCancelEventArgs args);

  #endregion

  #region BeforeSubDocEditEventHandler

  /// <summary>
  /// Аргументы события SubDocTypeUI.BeforeEdit
  /// </summary>
  public class BeforeSubDocEditEventArgs : SubDocEditEventArgs
  {
    #region Конструктор

    internal BeforeSubDocEditEventArgs(SubDocumentEditor editor)
      : base(editor)
    {
      _Cancel = false;
      _ShowEditor = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Установка этого поля в true приводит к отказу от работы редактора 
    /// Запись результатов выполяться не будет
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    /// <summary>
    /// Установка этого поля в false приводит к пропуску работы редактора
    /// Сразу выполняется запись значений полей, установленных в Editor.Values
    /// </summary>
    public bool ShowEditor { get { return _ShowEditor; } set { _ShowEditor = value; } }
    private bool _ShowEditor;

    #endregion
  }

  /// <summary>
  /// Делегат события SubDocTypeUI.BeforeEdit
  /// </summary>
  /// <param name="sender">Интерфейс поддокументов SubDicTypeUI</param>
  /// <param name="args">Аргументы события</param>
  public delegate void BeforeSubDocEditEventHandler(object sender, BeforeSubDocEditEventArgs args);

  #endregion

  #region AdjustPastedSubDocRowEventHandler

  /// <summary>
  /// Аргументы события SubDocTypeUI.AdjustPastedRow
  /// </summary>
  public class AdjustPastedSubDocRowEventArgs : CancelEventArgs
  {
    #region Конструктор

    internal AdjustPastedSubDocRowEventArgs(DBxSubDoc subDoc, DataRow sourceRow,
      DataSet sourceDataSet, string sourceTableName, DBxSingleDoc mainDoc, bool isFirstRow)
    {
      _SubDoc = subDoc;
      _SourceRow = sourceRow;
      _SourceDataSet = sourceDataSet;
      _SourceTableName = sourceTableName;
      _MainDoc = mainDoc;
      _IsFirstRow = isFirstRow;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Сюда может быть помещено сообщение, почему нельзя вставить строку
    /// (при установке свойства Cancel=true)
    /// </summary>
    public string ErrorMessage { get { return _ErrorMessage; } set { _ErrorMessage = value; } }
    private string _ErrorMessage;

    /// <summary>
    /// Поддокумент, куда можно внести изменения, установив или очистив неподходящие
    /// значения
    /// </summary>
    public DBxSubDoc SubDoc { get { return _SubDoc; } }
    private DBxSubDoc _SubDoc;

    /// <summary>
    /// Исходная строка (которая может содержать посторонние поля или не содержать нужные поля),
    /// откуда взяты значения. Нельзя изменять
    /// </summary>
    public DataRow SourceRow { get { return _SourceRow; } }
    private DataRow _SourceRow;

    /// <summary>
    /// Возвращает true для первой строки в операции вставки. Если требуется
    /// запрашивать у пользователя дополнительные данные, то это следует делать
    /// при обработке первой строки, затем использовать сохраненные значения
    /// </summary>
    public bool IsFirstRow { get { return _IsFirstRow; } }
    private bool _IsFirstRow;

    /// <summary>
    /// Исходный вставляемый набор данных. Нельзя изменять
    /// </summary>
    public DataSet SourceDataSet { get { return _SourceDataSet; } }
    private DataSet _SourceDataSet;

    /// <summary>
    /// Имя исходной таблицы
    /// </summary>
    public string SourceTableName { get { return _SourceTableName; } }
    private string _SourceTableName;

    /// <summary>
    /// Основной документ, куда будет добавлена строка поддокумента
    /// </summary>
    public DBxSingleDoc MainDoc { get { return _MainDoc; } }
    private DBxSingleDoc _MainDoc;

    /// <summary>
    /// Свойство возвращает true, если в исходной таблице находится только одна строка,
    /// поэтому после вставки будет открыт редактор, а не выполнена прямая запись поддокументов.
    /// Это позволяет оставить строку, даже если в ней не заполнены какие-либо важные поля
    /// </summary>
    public bool EditorWillBeOpen
    {
      get
      {
        return SourceDataSet.Tables[SourceTableName].Rows.Count == 1;
      }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события SubDocTypeUI.AdjustPastedRow
  /// </summary>
  /// <param name="sender">Интерфейс поддокументов SubDocTypeUI</param>
  /// <param name="args">Аргументы события</param>
  public delegate void AdjustPastedSubDocRowEventHandler(object sender, AdjustPastedSubDocRowEventArgs args);

  #endregion


  /// <summary>
  /// Обработчики стороны клиента для одного вида поддокументов
  /// </summary>
  public class SubDocTypeUI : DocTypeUIBase
  {
    // Пока не используется
#if XXX
    #region Переопределенный GridProducer

    /// <summary>
    /// Переопределение метода поиска недостающего значения для "точечного" поля
    /// </summary>
    private class MyGridProducer : GridProducer
    {
    #region Конструктор

      public MyGridProducer(SubDocTypeUI Owner)
      {
        FOwner = Owner;
      }

    #endregion

    #region Свойства

      private SubDocTypeUI FOwner;

    #endregion

    #region Переопределенный метод

      protected internal override object OnGetColumnValue(DataRow Row, string ColumnName)
      {
        int p = ColumnName.IndexOf('.');
        if (p < 0)
        {
          // return base.OnGetColumnValue(Row, ColumnName);
          // 12.07.2016
          if (Row.IsNull("Id"))
            return null;
          Int32 Id = (Int32)(Row["Id"]);
          return FOwner.TableCache.GetValue(Id, ColumnName);
        }
        string RefColName = ColumnName.Substring(0, p);
        Int32 RefId = DataTools.GetInt(Row, RefColName);
        return FOwner.TableCache.GetRefValue(RefColName, RefId);
      }

    #endregion
    }

    #endregion

#endif
    #region Защищенный конструктор

    internal SubDocTypeUI(DocTypeUI docTypeUI, DBxSubDocType subDocType)
      : base(docTypeUI.UI, subDocType)
    {
      _DocTypeUI = docTypeUI;
      _SubDocType = subDocType;

      _GridProducer = new EFPSubDocTypeGridProducer(this);

      CanMultiEdit = false;
      CanInsertCopy = false;

      _PasteTableNames = new List<string>();
      _PasteTableNames.Add(subDocType.Name);
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Интерфейс документов, к которому относятся поддокументы
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Описание вида поддокументов
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocType; } }
    private DBxSubDocType _SubDocType;

    /// <summary>
    /// Описание вида документов, к которым относятся поддокументы
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }

    /// <summary>
    /// True, если допускается одновременное редактирование или
    /// просмотр нескольких выбранных документов. По умолчанию -
    /// false (нельзя)
    /// </summary>
    public bool CanMultiEdit { get { return _CanMultiEdit; } set { _CanMultiEdit = value; } }
    private bool _CanMultiEdit;

    /// <summary>
    /// True, если допускается создание поддокумента на основании существующего
    /// (копирование). По умолчанию - false (нельзя)
    /// </summary>
    public bool CanInsertCopy { get { return _CanInsertCopy; } set { _CanInsertCopy = value; } }
    private bool _CanInsertCopy;


    /// <summary>
    /// Генератор табличного просмотра.
    /// Обычно в прикладном коде сюда следует добавить описания столбцов
    /// </summary>
    public EFPSubDocTypeGridProducer GridProducer { get { return _GridProducer; } }
    private EFPSubDocTypeGridProducer _GridProducer;

    #endregion

    #region Смена DBxDocProvider

    internal void OnDocProviderChanged()
    {
      DBxSubDocType NewSubDocType = DocType.SubDocs[_SubDocType.Name]; // DocTypeUI.DocType уже обновлено на момент вызова
      if (NewSubDocType == null)
        throw new NullReferenceException("Не найден новый SubDocType для SubDocTypeName=\"" + _SubDocType.Name + "\" документа \"" + DocType.Name + "\"");
      _SubDocType = NewSubDocType;
    }

    #endregion

    #region Инициализация табличного просмотра

    #region Основной метод

    /// <summary>
    /// Инициализация табличного просмотра поддокументов, не связанного с просмотром поддокументов
    /// на странице редактора основного документа
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого выполняется инициализация</param>
    /// <param name="reInit">False - первоначальная инициализация при выводе просмотра на экран.
    /// True - повторная инициализация после настройки табличного просмотра пользователем</param>
    /// <param name="columns">Сюда добавляются имена столбцов, которые должны присутствовать в таблице данных</param>
    /// <param name="userInitData">Пользовательские даннные, передаваемые обработчику события SubDocTypeUI.InitView</param>
    public void PerformInitGrid(EFPDBxGridView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      PerformInitGrid(controlProvider, reInit, columns, userInitData, false, false);
    }

    /// <summary>
    /// Инициализация табличного просмотра поддокументов
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого выполняется инициализация</param>
    /// <param name="reInit">False - первоначальная инициализация при выводе просмотра на экран.
    /// True - повторная инициализация после настройки табличного просмотра пользователем</param>
    /// <param name="columns">Сюда добавляются имена столбцов, которые должны присутствовать в таблице данных</param>
    /// <param name="userInitData">Пользовательские даннные, передаваемые обработчику события SubDocTypeUI.InitView</param>
    /// <param name="showDocId">Если true, то будет добавлен столбец "DocId", независимо от установленного
    /// свойства DBUI.DebugShowIds. Используется EFPSubDocGridView, когда отображаются поддокументы
    /// при групповом редактировании документов</param>
    /// <param name="isSubDocsGrid">True. если инициализация выполняется для EFPSubDocGridView.
    /// False, если инициализация выполняется для просмотра, не связанного с 
    /// поддокументами на странице редактора основного документа</param>
    public void PerformInitGrid(EFPDBxGridView controlProvider, bool reInit, DBxColumnList columns, object userInitData, bool showDocId, bool isSubDocsGrid)
    {
      if (reInit)
        controlProvider.Columns.Clear();

      // Добавляем столбец с картинками
      // См. примечание от 27.05.2015 в методе DocTypeUI.DoInitGrid()
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn ImgCol = new DataGridViewImageColumn();
        ImgCol.Name = "Image";
        ImgCol.HeaderText = String.Empty;
        ImgCol.ToolTipText = "Значок поддокумента \"" + SubDocType.SingularTitle + "\"";
        ImgCol.Width = controlProvider.Measures.ImageColumnWidth;
        ImgCol.FillWeight = 1; // 08.02.2017
        ImgCol.Resizable = DataGridViewTriState.False;

        controlProvider.Control.Columns.Add(ImgCol);
        if (EFPApp.ShowToolTips)
          controlProvider.Columns[0].CellToolTextNeeded += new EFPDataGridViewCellToolTipTextNeededEventHandler(ImgCol_CellToopTextNeeded);
      }
      if (controlProvider.MarkRowIds != null)
        controlProvider.AddMarkRowsColumn();

      if (showDocId)
      {
        controlProvider.Columns.AddInt("DocId", true, "DocId", 5);
        controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
      }


      //GridProducer.InitGrid(controlProvider, reInit, controlProvider.CurrentConfig, columns);
      controlProvider.GridProducer.InitGridView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021

      controlProvider.FrozenColumns = controlProvider.CurrentConfig.FrozenColumns + (EFPApp.ShowListImages ? 1 : 0);

      if (!reInit)
      {
        // Обработчик для печати (д.б. до вызова пользовательского блока инициализации)
        // TODO: ControlProvider.AddGridPrint().DocumentName = PluralTitle;

        CallInitView(controlProvider, userInitData);
      }

      columns.Add("Id");
      columns.Add("DocId");
      if (UI.DocProvider.DocTypes.UseDeleted) // 16.05.2018
        columns.Add("Deleted");

      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        controlProvider.Columns.AddInt("Id");
        controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        controlProvider.Columns.LastAdded.CanIncSearch = true;
        if (!showDocId)
        {
          controlProvider.Columns.AddInt("DocId");
          controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
      }

      // 16.06.2021 что за бяка? controlProvider.GetUsedColumns(columns);

      if (!reInit)
      {
        controlProvider.Control.VirtualMode = true;
        controlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ControlProvider_GetRowAttributes);
        if (EFPApp.ShowListImages)
          controlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ControlProvider_GetCellAttributes);

        if (HasGetDocSel)
        {
          if (isSubDocsGrid)
            controlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(SubDocGrid_GetDocSelWithTable);
          else
            controlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(SubDocGrid_GetDocSelWithIds);
        }

        controlProvider.ConfigHandler.Changed[EFPConfigCategories.GridConfig] = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      // TODO: if (ReInit)
      // TODO:   FToolTipExtractor = null;

      controlProvider.PerformGridProducerPostInit();
    }

    #endregion

    #region Событие InitView

    /// <summary>
    /// Вызывается при инициализации таблицы просмотра поддокументов
    /// для добавления столбцов. Если обработчик не установлен, выполняется
    /// инициализация по умолчанию
    /// </summary>
    public event InitEFPDBxViewEventHandler InitView;

    private void CallInitView(IEFPDBxView controlProvider, object userInitData)
    {
      if (InitView != null)
      {
        try
        {
          InitEFPDBxViewEventArgs Args = new InitEFPDBxViewEventArgs(controlProvider);
          Args.UserInitData = userInitData;
          InitView(this, Args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка обработчика события InitView");
        }
      }
    }

    #endregion

    #region Оформление просмотра

    void ControlProvider_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      DataRow Row = args.DataRow;

      if (Row == null)
        return;
      // 24.11.2017
      // Вызываем пользовательский обработчик и для удаленных документов
      //if (DataTools.GetBool(Row, "Deleted"))
      //  Args.Grayed = true;
      //else
      //{
      EFPDataGridViewColorType ColorType;
      bool Grayed;
      UI.ImageHandlers.GetRowColor(SubDocType.Name, Row, out ColorType, out Grayed);
      args.ColorType = ColorType;
      args.Grayed = Grayed;
      //}
    }

    void ControlProvider_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnName == "Image")
      {
        DataRow Row = args.DataRow;
        if (Row == null)
          return;

        switch (args.Reason)
        {
          case EFPDataGridViewAttributesReason.View:
            args.Value = GetImageValue(Row);
            break;

          case EFPDataGridViewAttributesReason.ToolTip:
            EFPDataGridViewConfig Cfg = ((EFPDBxGridView)args.ControlProvider).CurrentConfig;
            if (Cfg != null)
            {
              if (Cfg.CurrentCellToolTip)
              {
                string s1 = GetToolTipText(Row);
                args.ToolTipText = DataTools.JoinNotEmptyStrings(Environment.NewLine, new string[] { s1, args.ToolTipText }); // 06.02.2018
              }
            }
            if (args.ControlProvider.CurrentConfig.ToolTips.Count > 0)
            {
              string s2 = null;
              try
              {
                EFPDataViewRowInfo rowInfo = args.ControlProvider.GetRowInfo(args.RowIndex);
                s2 = GridProducer.ToolTips.GetToolTipText(args.ControlProvider.CurrentConfig, rowInfo);
                args.ControlProvider.FreeRowInfo(rowInfo);
              }
              catch (Exception e)
              {
                s2 = "Ошибка: " + e.Message;
              }
              args.ToolTipText = String.Join(EFPGridProducerToolTips.ToolTipTextSeparator, new string[] { args.ToolTipText, s2 });
            }

            break;
        }
      }
    }

    /// <summary>
    /// 01.07.2011
    /// Для картинки во всплывающей подсказке выводим текстовое представление
    /// </summary>
    void ImgCol_CellToopTextNeeded(object sender, EFPDataGridViewCellToolTipTextNeededEventArgs args)
    {
      // TODO: Int32 Id = DataTools.GetInt(Args.Row, "Id");
      // TODO: Args.ToolTipText = GetImageCellToolTipText(Id);
    }

    #endregion

    #region Выборка документов

    void SubDocGrid_GetDocSelWithTable(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      PerformGetDocSel(args.DocSel, args.DataRows, args.Reason);
    }

    void SubDocGrid_GetDocSelWithIds(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      Int32[] Ids = DataTools.GetIds(args.DataRows);
      PerformGetDocSel(args.DocSel, Ids, args.Reason);
    }

    #endregion

    #endregion

    #region Список столбцов для табличного просмотра

    /// <summary>
    /// Получить список столбцов, необходимых для табличного просмотра с заданной конфигурации
    /// Заполняется такой же список столбов, как и в методе DoInitGrid(), но без создания самого просмотра
    /// </summary>
    /// <param name="columns">Заполняемый список столбцов</param>
    /// <param name="config">Конфигурация табличного просмотра. Если null, то используется конфигурация по умолчанию</param>
    public void GetColumnNames(DBxColumnList columns, EFPDataGridViewConfig config)
    {
      columns.Add("Id");
      columns.Add("DocId");
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
    /// Инициализация иерархического просмотра поддокументов EFPSubDocTreeView
    /// </summary>
    /// <param name="controlProvider">Инициализируемый провайдер просмотра</param>
    /// <param name="reInit">False - первоначальная инициализация при выводе просмотра на экран.
    /// True - повторная инициализация после настройки табличного просмотра пользователем</param>
    /// <param name="columns">Сюда добавляются имена столбцов, которые должны присутствовать в таблице данных</param>
    /// <param name="userInitData">Пользовательские даннные, передаваемые обработчику события SubDocTypeUI.InitView</param>
    public void PerformInitTree(EFPSubDocTreeView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      if (!reInit)
      {
        //if (CanMultiEdit)
        //  ControlProvider.CanMultiEdit = true;
        if (CanInsertCopy)
          controlProvider.CanInsertCopy = true;
      }
      else
        controlProvider.Control.Columns.Clear();

      //GridProducer.InitTree(controlProvider, reInit, controlProvider.CurrentConfig, columns);
      controlProvider.GridProducer.InitTreeView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021
      TreeViewCachedValueAdapter.InitColumns(controlProvider, UI.TextHandlers.DBCache, GridProducer);

      controlProvider.SetColumnsReadOnly(true);

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

        CallInitView(controlProvider, userInitData);
      }
#if XXX
      // Добавляем столбец с картинками
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn ImgCol = new DataGridViewImageColumn();
        ImgCol.Name = "Image";
        ImgCol.HeaderText = String.Empty;
        ImgCol.ToolTipText = "Значок документа \"" + DocType.SingularTitle + "\"";
        ImgCol.Width = ControlProvider.Measures.ImageColumnWidth;
        ImgCol.Resizable = DataGridViewTriState.False;
        //string ImgName = SingleDocImageKey;
        //ImgCol.Image = EFPApp.MainImages.Images[ImgName];
        if (ControlProvider.Control.Columns.Count > 0 && ControlProvider.Control.Columns[0].Frozen)
          ImgCol.Frozen = true;
        ControlProvider.Control.Columns.Insert(0, ImgCol);
        if (EFPApp.ShowToolTips)
          ControlProvider.Columns[0].CellToopTextNeeded += new EFPDataGridViewCellToolTipTextNeededEventHandler(ImgCol_CellToopTextNeeded);
      }
#endif
      columns.Add("Id");
      columns.Add("DocId");
      columns.Add(SubDocType.TreeParentColumnName);

      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        controlProvider.Columns.AddInt("Id", true, "Id", 6);
        //ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        //ControlProvider.Columns.LastAdded.CanIncSearch = true;
        controlProvider.Columns.AddInt("DocId", true, "DocId", 6);
        //ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
      }

      // TODO:
      //if (EFPApp.ShowListImages)
      //  Columns = Columns + ColumnsForImage;
      //Columns = Columns + FieldsForColors;
      //Columns = Columns + ControlProvider.GetUsedFields();
      //DataFields CondFields = AccDepClientExec.AllDocConditions.GetFields(Name);
      //Columns += CondFields;

      if (!reInit)
      {
        //ControlProvider.Control.VirtualMode = true;
        //ControlProvider.UseRowImages = true;
        //ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ControlProvider_GetRowAttributes);
        //if (CondFields != null)
        //  ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributesForCond);
        //if (EFPApp.ShowListImages)
        //  ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ControlProvider_GetCellAttributes);
        //DocGridHandler.MainGrid.CellPainting += new DataGridViewCellPaintingEventHandler(TheGrid_CellPainting);
        //DocGridHandler.MainGrid.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(TheGrid_CellToolTipTextNeeded);

        if (HasGetDocSel)
        {
          // ???
          //if (IsSubDocsGrid)
          //  ControlProvider.GetDocSel += new EFPDBxTreeViewDocSelEventHandler(SubDocTree_GetDocSelWithTable);
          //else
          controlProvider.GetDocSel += new EFPDBxTreeViewDocSelEventHandler(SubDocTree_GetDocSelWithIds);
        }

        //??controlProvider.UserConfigModified = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      //      if (ReInit)
      //        FToolTipExtractor = null;
    }

    #endregion

#if XXX
    void SubDocTree_GetDocSelWithTable(object sender, EFPDBxTreeViewDocSelEventArgs args)
    {
      PerformGetDocSel(args.DocSel, args.DataRows, args.Reason);
    }
#endif

    void SubDocTree_GetDocSelWithIds(object sender, EFPDBxTreeViewDocSelEventArgs args)
    {
      Int32[] Ids = DataTools.GetIds(args.DataRows);
      PerformGetDocSel(args.DocSel, Ids, args.Reason);
    }

    #endregion

    #region Свойства и методы буферизации данных

    /// <summary>
    /// Получить идентификаторы всех поддокументов, кроме удаленных, для документа
    /// с заданным идентифкатором
    /// </summary>
    /// <param name="docId">Идентификатор основного документа. Не может быть 0</param>
    /// <returns>Массив идентификаторов поддокументов</returns>
    public Int32[] GetSubDocIds(Int32 docId)
    {
      if (docId == 0)
        throw new ArgumentException("Идентификатор документа должен быть задан", "docId");

      // TODO: !!!
      /*
      if (DataBuffering)
      {
        DataTable Table = GetBufferedData(null);
        DataView dv = new DataView(Table);
        dv.RowFilter = "DocId=" + DocId.ToString() + " AND (ISNULL(Deleted) OR (Deleted=FALSE))";

        int[] SubDocIds = new int[dv.Count];
        for (int i = 0; i < SubDocIds.Length; i++)
          SubDocIds[i] = DataTools.GetInt(dv[i]["Id"]);
        return SubDocIds;
      }
      else 
       * */
      return UI.DocProvider.GetSubDocIds(DocType.Name, SubDocType.Name, docId);
    }


    #endregion

    #region Выбор поддокумента

    /// <summary>
    /// Выбор поддокумента в процессе редактирования документа
    /// Позволяет выбирать поддокумент из загруженного в память набора DBxDocSet
    /// </summary>
    /// <param name="subDocs">Список поддокументов, из которых можно выбирать</param>
    /// <param name="subDocId">Вход и выход: Идентификатор выбраного поддокумента.
    /// 0, если нет выбранного документа</param>
    /// <param name="title">Заголовок блока диалога.
    /// Если не задано, используется "Выбор поддокументов X"</param>
    /// <param name="canBeEmpty">Если true, то допускается нажатие кнопки "ОК", если не выбрано ни одного поддокумента.
    /// Также в диалоге присутствует кнопка "Нет".
    /// Если false, то пользователь должен выбрать хотя бы один поддокумент из списка</param>
    /// <returns>True, если пользователь выбрал поддокументы и нажал "ОК" или "Нет"</returns>
    public bool SelectSubDoc(DBxMultiSubDocs subDocs, ref Int32 subDocId, string title, bool canBeEmpty)
    {
      SubDocSelectDialog dlg = new SubDocSelectDialog(this, subDocs);
      dlg.SelectionMode = DocSelectionMode.Single;
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.CanBeEmpty = canBeEmpty;
      dlg.SubDocId = subDocId;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        subDocId = dlg.SubDocId;
        return true;
      }
      else
        return false;
    }

#if XXXX
    // Пока неохота делать

    public bool SelectSubDoc(ref Int32 SubDocId, string Title, bool CanBeEmpty, GridFilters Filters)
    {
      SubDocTableViewForm Form = new SubDocTableViewForm(this, DocTableViewMode.SelectDoc);
      bool Res = false;
      try
      {
        if (String.IsNullOrEmpty(Title))
          Form.Text = "Выбор документа \"" + DocType.SingularTitle + "\"";
        else
          Form.Text = Title;
        Form.TheButtonPanel.Visible = true;
        Form.CanBeEmpty = CanBeEmpty;
        Form.CurrentDocId = DocId;
        Form.ExternalFilters = Filters;
        Form.ExternalEditorCaller = ExternalEditorCaller;
        switch (EFPApp.ShowDialog(Form, false))
        {
          case DialogResult.OK:
            DocId = Form.CurrentDocId;
            Res = true;
            break;
          case DialogResult.No:
            DocId = 0;
            Res = true;
            break;
          default:
            Res = false;
            break;
        }
      }
      finally
      {
        Form.Dispose();
      }
      return Res;
    }
#endif


    /// <summary>
    /// Выбор поддокумента в процессе редактирования документа
    /// Позволяет выбирать поддокументы из загруженного в память набора DBxDocSet
    /// </summary>
    /// <param name="subDocs">Список поддокументов, из которых можно выбирать</param>
    /// <param name="subDocIds">Вход и выход: Массив идентификаторов выбранных поддокументов.
    /// Пустой массив, если нет выбранных поддокументов</param>
    /// <param name="title">Заголовок блока диалога.
    /// Если не задано, используется "Выбор поддокументов X"</param>
    /// <param name="canBeEmpty">Если true, то допускается нажатие кнопки "ОК", если не выбрано ни одного поддокумента.
    /// Также в диалоге присутствует кнопка "Нет".
    /// Если false, то пользователь должен выбрать хотя бы один поддокумент из списка</param>
    /// <returns>True, если пользователь выбрал поддокументы и нажал "ОК" или "Нет"</returns>
    public bool SelectSubDocs(DBxMultiSubDocs subDocs, ref Int32[] subDocIds, string title, bool canBeEmpty)
    {
      SubDocSelectDialog dlg = new SubDocSelectDialog(this, subDocs);
      dlg.SelectionMode = DocSelectionMode.MultiSelect;
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.CanBeEmpty = canBeEmpty;
      dlg.SubDocIds = subDocIds;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        subDocIds = dlg.SubDocIds;
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Выбор документа

    private static bool _LastSelectOneDocAll = false;

    /// <summary>
    /// Выбор одного документа для режима добавления поддокумента.
    /// Возможно выбрать режим "Для всех документов".
    /// Используется EFPSubDocGridView и EFPSubDocTreeView.
    /// </summary>
    /// <param name="controlProvider">Просмотр, в котором выполняется редактирование</param>
    /// <param name="docId">Идентификатор выбранного документа</param>
    /// <returns>true, если выбор сделан</returns>
    internal bool SelectOneDoc(IEFPSubDocView controlProvider, out Int32 docId)
    {
      if (controlProvider.SubDocs.Owner.DocCount == 1)
      {
        docId = controlProvider.SubDocs.Owner[0].DocId;
        return true;
      }

      Int32 CurrDocId = 0;
      if (!_LastSelectOneDocAll)
      {
        if (controlProvider.CurrentDataRow != null)
          CurrDocId = DataTools.GetInt(controlProvider.CurrentDataRow, "DocId");
      }

      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = SubDocType.SingularTitle + " (Создание)";
      dlg.ImageKey = "Insert";
      dlg.ListTitle = SubDocType.DocType.SingularTitle;
      dlg.Items = new string[controlProvider.SubDocs.Owner.DocCount + 1];
      dlg.ImageKeys = new string[controlProvider.SubDocs.Owner.DocCount + 1];
      dlg.Items[0] = "[ Для всех документов ]";
      dlg.ImageKeys[0] = "Table";
      dlg.SelectedIndex = 0;
      for (int i = 0; i < controlProvider.SubDocs.Owner.DocCount; i++)
      {
        dlg.Items[i + 1] = (i + 1).ToString() + ". DocId=" + controlProvider.SubDocs.Owner[i].DocId.ToString(); // !!! Названия для документов
        DBxSingleDoc Doc = controlProvider.SubDocs.Owner[i];
        dlg.ImageKeys[i + 1] = UI.ImageHandlers.GetImageKey(Doc);
        if (controlProvider.SubDocs.Owner[i].DocId == CurrDocId)
          dlg.SelectedIndex = i + 1;
      }

      if (dlg.ShowDialog() != DialogResult.OK)
      {
        docId = 0;
        return false;
      }
      if (dlg.SelectedIndex == 0)
      {
        docId = 0;
        _LastSelectOneDocAll = true;
      }
      else
      {
        docId = controlProvider.SubDocs.Owner[dlg.SelectedIndex - 1].DocId;
        _LastSelectOneDocAll = false;
      }
      return true;
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Возвращает true, если есть хотя бы один установвленный обработчик
    /// BeforeEdit или InitEditForm, то есть возможность редактирования 
    /// была инициализирована.
    /// </summary>
    public bool HasEditorHandlers
    {
      get
      {
        return /*Editing != null || */BeforeEdit != null || InitEditForm != null;
      }
    }

    /// <summary>
    /// Вызывается при попытке вставить, отредактировать или просмотреть поддокумент
    /// Обработчик может установить свойство Cancel для предотвращения вывода на
    /// экран формы редактора
    /// </summary>
    public event BeforeSubDocEditEventHandler BeforeEdit;

    /// <summary>
    /// Вызывает событие BeforeEdit и возвращает признак Cancel
    /// </summary>
    /// <returns></returns>
    internal void DoBeforeEdit(SubDocumentEditor editor, out bool cancel, out bool showEditor)
    {
      if (BeforeEdit == null)
      {
        cancel = false;
        showEditor = true;
      }
      else
      {
        BeforeSubDocEditEventArgs Args = new BeforeSubDocEditEventArgs(editor);
        BeforeEdit(this, Args);
        cancel = Args.Cancel;
        showEditor = Args.ShowEditor;
      }
    }

    /// <summary>
    /// Вызывается при инициализации окна редактирования строки в. 
    /// таблице поддокументов SubDocsGrid.
    /// </summary>
    public event InitSubDocEditFormEventHandler InitEditForm;

    internal void PerformInitEditForm(SubDocumentEditor editor)
    {
      InitSubDocEditFormEventArgs Args = new InitSubDocEditFormEventArgs(editor);
      if (InitEditForm != null)
        InitEditForm(this, Args);
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "ОК" в редакторе поддокумента
    /// Значения из полей редактирования уже перенесены в Editor.Values и их
    /// можно там подкорректировать.
    /// Установка свойства Cancel предотвращает закрытие редактора поддокумента
    /// </summary>
    public event SubDocEditCancelEventHandler Writing;

    internal bool DoWriting(SubDocumentEditor editor)
    {
      if (Writing != null)
      {
        SubDocEditCancelEventArgs Args = new SubDocEditCancelEventArgs(editor);
        Writing(this, Args);
        if (Args.Cancel)
          return false;
      }
      return true;
    }


    /// <summary>
    /// Вызывется после нажатия кнопки "ОК" в редакторе поддокумента в режиме
    /// копирования, создания или редактирования. Может вызываться
    /// многократно.
    /// </summary>
    public event SubDocEditEventHandler Wrote;

    internal void DoWrote(SubDocumentEditor editor)
    {
      if (Wrote != null)
      {
        SubDocEditEventArgs Args = new SubDocEditEventArgs(editor);
        Wrote(this, Args);
      }
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Получение выборки документов, связанных с заданными поддокументами.
    /// Вызывает обработчик событие GetDocSel, если он установлен. 
    /// Иначе не выполняется никаких действий.
    /// </summary>
    /// <param name="docSel">Выборка, в которую выполняется добавление документов</param>
    /// <param name="subDocIds">Идентификаторы поддокументов</param>
    /// <param name="reason">Причина создания выборки</param>
    public override void PerformGetDocSel(DBxDocSelection docSel, Int32[] subDocIds, EFPDBxGridViewDocSelReason reason)
    {
      if (subDocIds == null || subDocIds.Length == 0)
        return;

      base.OnGetDocSel(docSel, subDocIds, reason);
    }


    /// <summary>
    /// Создать выборку связанных документов если задан обработчик события GetDocSel
    /// Если обработчик не задан, то возвращается пустая выборка документов
    /// Эта версия метода используется в SubDocsGrid, когда некоторые поддокументы
    /// могут быть еще не записаны
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="rows">Выбранные строки таблицы поддокументов</param>
    /// <param name="reason">Причина построения выборки</param>
    /// <returns>Выборка</returns>
    public void PerformGetDocSel(DBxDocSelection docSel, DataRow[] rows, EFPDBxGridViewDocSelReason reason)
    {
      if (rows == null || rows.Length == 0)
        return;

      // Собственные идентификаторы не добавляются

      base.OnGetDocSel(docSel, rows, reason);

      return;
    }
    #endregion

    #region Вставка в EFPSubDocGridView

    /// <summary>
    /// Список имен документов и поддокументов, из которых возможна вставка табличных
    /// данных. Вставка выполняется в редакторе основного документа (в SubDocsGrid).
    /// По умолчанию в списке находится имя текущего поддокумента, то есть
    /// вставлять можно только аналогичные поддокументы
    /// </summary>
    public List<string> PasteTableNames { get { return _PasteTableNames; } }
    private List<string> _PasteTableNames;

    /// <summary>
    /// Событие вызывается при вставке копии строки поддокумента из буфера
    /// обмена. Обработчик может изменить значения полей в строке
    /// </summary>
    public event AdjustPastedSubDocRowEventHandler AdjustPastedRow;

    /// <summary>
    /// Вызывает обработчик события AdjustPastedRow, если обработчик установлен
    /// </summary> 
    /// <param name="args">Аргументы события</param>
    public void PerformAdjustPastedRow(AdjustPastedSubDocRowEventArgs args)
    {
      if (AdjustPastedRow != null)
        AdjustPastedRow(this, args);
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
        throw new ArgumentException("Для документов \"" + docType.PluralTitle + "\" не задано свойство TreeParentColumnName. Эти документы не образуют дерево групп", "docType");

      for (int i = 0; i < docType.Struct.Columns.Count; i++)
      {
        DBxColumnStruct ColDef = docType.Struct.Columns[i];
        if (ColDef.ColumnType == DBxColumnType.String && (!ColDef.Nullable))
        {
          _NameColumnName = ColDef.ColumnName;
          break;
        }
      }
      if (String.IsNullOrEmpty(_NameColumnName))
        throw new ArgumentException("У документов \"" + docType.PluralTitle + "\" нет ни одного текстового столбца. Эти документы не образуют дерево групп", "docType");

      this.Columns[_NameColumnName].NewMode = ColumnNewMode.AlwaysDefaultValue; // 10.06.2019 - название группы должно быть пустым

      #endregion
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя столбца, содержащего название группы
    /// </summary>
    public string NameColumnName { get { return _NameColumnName; } }
    private string _NameColumnName;

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
    public IdList GetAuxFilterGroupIdList(Int32 groupId, bool includeNestedGroups)
    {
      if (groupId == 0)
      {
        if (includeNestedGroups)
          return null;
        else
          return IdList.Empty;
      }
      else
      {
        if (includeNestedGroups)
        {
          DBxDocTreeModel Model = new DBxDocTreeModel(UI.DocProvider,
            DocType,
            new DBxColumns(new string[] { "Id", DocType.TreeParentColumnName }));

          return new IdList(Model.GetIdWithChildren(groupId));
        }
        else
          return IdList.FromId(groupId);
      }
    }

    #endregion
  }
}
