// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// Расширение <see cref="DBxDocProvider"/> для использования в DBUI
  /// Выполняет обновление табличных просмотров в ответ на ApplyChanges()
  /// </summary>
  internal class DocProviderUI : DBxChainDocProvider
  {
    #region Защищенный конструктор

    internal DocProviderUI(DBxDocProviderProxy sourceProxy, DBUI ui)
      : base(sourceProxy, true)
    {
      _UI = ui;

      base.ExceptionCaught += new FreeLibSet.Data.Docs.DBxRetriableExceptionEventHandler(DocProviderUI_ExceptionCaught);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к документам, к которому относится провайдер
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region Внутренние методы для выдачи заставок

    private string GetTableImageKey(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        return "Table";
      DocTypeUIBase dtui = UI.DocTypes.FindByTableName(tableName);
      if (dtui == null)
        return "Table";
      else
        return dtui.ImageKey;
    }

    private void BeginWait(string message, string imageKey)
    {
      if (_UI.ShowWaitMessages)
        EFPApp.BeginWait(message, imageKey);
    }

    private void EndWait()
    {
      if (_UI.ShowWaitMessages)
        EFPApp.EndWait();
    }

    #endregion

    #region Переопределение методов для выдачи заставок

    /// <summary>
    /// Выполнение SQL-запроса SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры для запроса</param>
    /// <returns>Таблица данных</returns>
    protected override DataTable DoFillSelect(DBxSelectInfo info)
    {
      DataTable table;
      BeginWait("Запрос таблицы \"" + info.TableName + "\"", GetTableImageKey(info.TableName));
      try
      {
        // 29.08.2020
        // Используем процедуру на случай длительного вызова

        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "FillSelect";
        DispArgs["SelectInfo"] = info;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "SELECT для таблицы \"" + info.TableName + "\"");
        table = (DataTable)(DispRes["Table"]);
      }
      finally
      {
        EndWait();
      }
      return table;
    }

    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    protected override DataTable DoFillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      DataTable table;
      BeginWait("Получение списка значений для таблицы \"" + tableName + "\"", GetTableImageKey(tableName));
      try { table = base.DoFillUniqueColumnValues(tableName, columnName, where); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Получить таблицу истории для документа
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Таблица истории</returns>
    protected override DataTable DoGetDocHistTable(string docTypeName, Int32 docId)
    {
      DataTable table;
      BeginWait("Получение таблицы истории документа", "Information");
      try { table = base.DoGetDocHistTable(docTypeName, docId); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Получить набор таблиц для просмотра ссылок на один документ.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа, на который ищутся ссылки</param>
    /// <param name="docId">Идентификатор документа, на который ищутся ссылки</param>
    /// <param name="showDeleted">Надо ли включать в таблицу ссылки из удаленных документов и поддокументов,
    /// а также ссылки на удаленные поддокументы выбранного документа.
    /// Не имеет значение, помечен ли сам документ <paramref name="docId"/> на удаление (за исключением ссылок документа на самого себя</param>
    /// <param name="unique">Если true, то будет возвращено только по одной ссылке из каждого документа.
    /// Это рекомендуемый вариант для показа таблицы ссылок. Если false, то в таблице может быть несколько ссылок из одного исходного документа</param>
    /// <param name="fromSingleDocTypeName">Единственный вид документа, из которого берутся ссылки. Если не задано, то берутся ссылки из всех документов</param>
    /// <param name="fromSingleDocId">Идентификатор единственного документа, из которого берутся ссылки. Если 0, то берутся ссылки из всех документов</param>
    /// <returns>Таблица ссылок</returns>
    protected override DataTable DoGetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      DataTable table;
      BeginWait("Получение таблицы ссылок для документа", "DocRefs");
      try { table = base.DoGetDocRefTable(docTypeName, docId, showDeleted, unique, fromSingleDocTypeName, fromSingleDocId); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Получить таблицу действий пользователя или всех пользователей за определенный период
    /// </summary>
    /// <param name="firstDate">Начальная дата</param>
    /// <param name="lastDate">Конечная дата</param>
    /// <param name="userId">Идентификатор пользователя. 0-все пользователи</param>
    /// <param name="singleDocTypeName">Имя таблицы документа. Пустая строка - документы всех видов</param>
    /// <returns>Таблица действий</returns>
    protected override DataTable DoGetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName)
    {
      DataTable table;
      BeginWait("Получение списка действий пользователя", "UserActions");
      try { table = base.DoGetUserActionsTable(firstDate, lastDate, userId, singleDocTypeName); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoGetUserActionDocTable(Int32 actionId)
    {
      DataTable table;
      BeginWait("Получение таблицы документов для действия пользователя", "UserActions");
      try { table = base.DoGetUserActionDocTable(actionId); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Метод получения двоичных данных, реализуемый в DBxRealDocProvider.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор двоичных данных</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и двочных данных, которые нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и двочных данных,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор двоичных данных. Значение - загруженные данные</returns>
    protected override Dictionary<Int32, byte[]> DoInternalGetBinData2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      Dictionary<Int32, byte[]> res;
      BeginWait("Получение двоичных данных", GetTableImageKey(tableName));
      try
      {
        // 14.10.2020
        // Используем процедуру на случай длительного вызова
        //res = base.InternalGetBinData2(tableName, columnName, wantedId, docVersion, preloadIds); 
        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "InternalGetBinData2";
        DispArgs["TableName"] = tableName;
        DispArgs["ColumnName"] = columnName;
        DispArgs["WantedId"] = wantedId;
        DispArgs["DocVersion"] = docVersion;
        DispArgs["PreloadIds"] = preloadIds;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "Получение двоичных данных");
        res = (Dictionary<Int32, byte[]>)(DispRes["Data"]);
      }
      finally
      {
        EndWait();
      }
      return res;
    }

    /// <summary>
    /// Внутренний метод получения хранимого файла
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор файла</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и файла, который нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и файлов,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор файла. Значение - контейнер с файлом</returns>
    protected override Dictionary<Int32, FreeLibSet.IO.FileContainer> DoInternalGetDBFile2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      Dictionary<Int32, FreeLibSet.IO.FileContainer> res;
      BeginWait("Получение файла из базы данных", GetTableImageKey(tableName));
      try
      {
        // 14.10.2020
        // Используем процедуру на случай длительного вызова
        //res = base.InternalGetDBFile2(tableName, columnName, wantedId, docVersion, preloadIds);
        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "InternalGetDBFile2";
        DispArgs["TableName"] = tableName;
        DispArgs["ColumnName"] = columnName;
        DispArgs["WantedId"] = wantedId;
        DispArgs["DocVersion"] = docVersion;
        DispArgs["PreloadIds"] = preloadIds;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "Получение содержимого файла");
        res = (Dictionary<Int32, FreeLibSet.IO.FileContainer>)(DispRes["Data"]);
      }
      finally
      {
        EndWait();
      }
      return res;
    }

    /// <summary>
    /// Загрузить страницу таблицы кэша
    /// </summary>
    /// <param name="request">Параметры запроса</param>
    /// <returns>Кэш страницы</returns>
    protected override DBxCacheLoadResponse DoLoadCachePages(DBxCacheLoadRequest request)
    {
      DBxCacheLoadResponse res;
      BeginWait("Получение кэшированных страниц для " + request.ToString(), "Database");
      try { res = base.DoLoadCachePages(request); }
      finally { EndWait(); }
      return res;
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoLoadDocData(string docTypeName, DBxFilter filter)
    {
      DataTable table;
      BeginWait("Загрузка документов \"" + DocTypes[docTypeName].PluralTitle + "\"", GetTableImageKey(docTypeName));
      try { table = base.DoLoadDocData(docTypeName, filter); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoLoadDocData(string docTypeName, Int32[] docIds)
    {
      DataTable table;
      BeginWait("Загрузка документов \"" + DocTypes[docTypeName].PluralTitle + "\" по фильтру", GetTableImageKey(docTypeName));
      try { table = base.DoLoadDocData(docTypeName, docIds); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// Загрузить поддокументы.
    /// Предполагается, что таблица документов уже загружена
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docIds">Массив идентификаторов документов, для которых загружаются поддокументы</param>
    /// <returns>Таблица поддокументов</returns>
    protected override DataTable DoLoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds)
    {
      DataTable table;
      BeginWait("Загрузка поддокументов", GetTableImageKey(subDocTypeName));
      try { table = base.DoLoadSubDocData(docTypeName, subDocTypeName, docIds); }
      finally { EndWait(); }
      return table;
    }

    #endregion

    #region ApplyChanges()

    /// <summary>
    /// Применение изменений.
    /// Выполняется создание, изменение и удаление документов и поддокументов.
    /// Если <paramref name="reloadData"/>=true, то выполняется обновление открытых просмотров
    /// документов вызовами DocumnetViewHandler.ApplyChanges()
    /// </summary>
    /// <param name="dataSet">Набор данных</param>
    /// <param name="reloadData">Если true, то будет возвращен тот же набор данных.
    /// В нем фиктивные идентификаторы новых документов и поддокументов будут заменены на реальные,
    /// а перекрестные ссылки на них исправлены. Это требуется в интерфейсе пользователя, когда
    /// пользователь нажимает кнопку "Применить", чтобы можно было продолжить сеанс редактирования.
    /// Если false, то исправленный набор данных не возвращается</param>
    /// <returns>Набор с исправленными ссылками или null</returns>
    protected override DataSet DoApplyChanges(DataSet dataSet, bool reloadData)
    {
      DataSet res;
      BeginWait("Запись изменений в документах", "Save");
      try { res = DoApplyChanges2(dataSet, reloadData); }
      finally { EndWait(); }
      return res;
    }

    private DataSet DoApplyChanges2(DataSet dataSet, bool reloadData)
    {
      if (dataSet == null)
        throw new ArgumentNullException("dataSet");

      bool UseAsyncCall = DataTools.GetBool(dataSet.ExtendedProperties["UseAsyngWriting"]);

      DataSet dataSet2;
      if (UseAsyncCall)
      {
        // Асинхронный вызов
        NamedValues dispArgs = new NamedValues();
        dispArgs["Action"] = "ApplyChanges";
        dispArgs["DataSet"] = dataSet;
        dispArgs["ReloadData"] = reloadData;
        NamedValues DispRes = ExecuteServerAsync(dispArgs, "Запись изменений документов");
        dataSet2 = (DataSet)(DispRes["DataSet"]);
      }
      else
      {
        // Синхронный вызов, который используется чаще всего
        dataSet2 = base.DoApplyChanges(dataSet, reloadData);
      }

      // Сброс буферизации выполняем независимо от того, выполняется ли возврат строк от сервера
      // (признака ReloadData)
      // Сброс кэша выполняется до обновления табличных просмотров
      DBxDocProvider.ClearCache(dataSet, UI.TextHandlers.DBCache);

      if (reloadData)
      {
        if (dataSet2 != null)
          AfterApplyChanges(dataSet2);
        else
        {
          try
          {
            NullReferenceException e = new NullReferenceException("При вызове OnApplyChanges() не получен ответный набор данных при ReloadData=true");
            e.Data["UseAsyncCall"] = UseAsyncCall;
          }
          catch (Exception e2)
          {
            LogoutTools.LogoutException(e2);
          }
        }
      }

      return dataSet2;
    }

    private void AfterApplyChanges(DataSet dataSet2)
    {
      SingleScopeList<DocTypeUI> docTypeUIs = new SingleScopeList<DocTypeUI>();
      foreach (DataTable table in dataSet2.Tables)
      {
        DocTypeUI docTypeUI;
        SubDocTypeUI subDocTypeUI;
        if (!_UI.DocTypes.FindByTableName(table.TableName, out docTypeUI, out subDocTypeUI))
          continue; // Недоразумение

        docTypeUIs.Add(docTypeUI);
      }

      // Обновление табличных просмотров
      Guid browserGuid;
      if (DocumentViewHandler.CurrentHandler == null)
        browserGuid = Guid.Empty;
      else
        browserGuid = DocumentViewHandler.CurrentHandler.BrowserGuid;

      foreach (DocTypeUI docTypeUI in docTypeUIs)
      {
        for (int i = 0; i < docTypeUI.Browsers.Count; i++)
        {
          DocumentViewHandler dvh=docTypeUI.Browsers[i];
          bool IsCaller = (dvh.BrowserGuid == browserGuid) && browserGuid != Guid.Empty;
          try
          {
            dvh.ApplyChanges(dataSet2, IsCaller);
          }
          catch (Exception e) // 17.06.2019
          {
            e.Data["DocumentViewHandler.GetType()"] = dvh.GetType().ToString();
            e.Data["DocumentViewHandler"] = dvh.ToString();
            e.Data["BrowserGuid"] = dvh.BrowserGuid;
            e.Data["IsCaller"] = IsCaller;
            EFPApp.ShowException(e, "Ошибка обновления просмотра после записи изменений для документов \"" + docTypeUI.DocType.PluralTitle+"\"");
          }
        }
        docTypeUI.RefreshBufferedData(); // 03.02.2022
      }
      //DebugTools.DebugDataSet(DataSet, "Обновленный");
    }

    #endregion

    #region Асинхронное выполнение

    /// <summary>
    /// Вызывает серверную процедуру, получаемую CreateServerExecProc()
    /// </summary>
    /// <param name="dispArgs">Аргументы вызова</param>
    /// <param name="displayName">Отображаемое имя в списке процедур</param>
    /// <returns>Результаты</returns>
    private NamedValues ExecuteServerAsync(NamedValues dispArgs, string displayName)
    {
      DistributedCallData startData = base.StartServerExecProc(dispArgs);

      DistributedProcCallItem callItem = new DistributedProcCallItem(startData);
      callItem.DisplayName = displayName;
      NamedValues dispRes = EFPApp.ExecProcList.ExecuteAsyncAndWait(callItem);
      return dispRes;
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Возвращает текстовое представление документа или поддокумента,
    /// используя DBUI.TextHandlers
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    protected override string DoGetTextValue(string tableName, Int32 id)
    {
      return _UI.TextHandlers.GetTextValue(tableName, id);
    }

    #endregion

    #region Обработка исключений

    void DocProviderUI_ExceptionCaught(object sender, FreeLibSet.Data.Docs.DBxRetriableExceptionEventArgs args)
    {
      System.Net.Sockets.SocketException se = LogoutTools.GetException<System.Net.Sockets.SocketException>(args.Exception);
      if (se != null)
      {
        if (se.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
        {
          if (args.RepeatCount > 1)
            return; // 21.08.2020
        }
      }

      _UI.OnExceptionCaught(args);
    }

    #endregion
  }
}
