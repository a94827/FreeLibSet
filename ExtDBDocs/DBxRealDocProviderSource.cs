using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

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

namespace AgeyevAV.ExtDB.Docs
{
  #region Делегаты

  /// <summary>
  /// Аргументы события DBxRealDocProviderSource.BeforeApplyChanges
  /// </summary>
  public class DBxBeforeApplyChangesEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Аргументы события не создаются в пользовательском коде
    /// </summary>
    /// <param name="docSet">Набор данных</param>
    public DBxBeforeApplyChangesEventArgs(DBxDocSet docSet)
    {
#if DEBUG
      if (docSet == null)
        throw new ArgumentNullException("docSet");
#endif

      _DocSet = docSet;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Набор данных, обновления из которого применяются
    /// </summary>
    public DBxDocSet DocSet { get { return _DocSet; } }
    private DBxDocSet _DocSet;

    #endregion
  }

  /// <summary>
  /// Делегат события DBxRealDocProviderSource.BeforeApplyChanges
  /// </summary>
  /// <param name="sender">Ссылка на DBxRealDocProviderSource</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxBeforeApplyChangesEventHandler(object sender,
    DBxBeforeApplyChangesEventArgs args);

  #endregion

  /// <summary>
  /// Потокобезопасный (в режиме ReadOnly) класс, хранящий общие данные для одного или нескольких DBxRealDocProvider 
  /// Объект хранит настройки разрешений для пользователя
  /// Этот класс не сериализуется и существует только на стороне сервера, 
  /// т.к. через свойство GlobalData содержит точки подключения к базе данных без ограничения доступа
  /// </summary>
  public class DBxRealDocProviderSource : IReadOnlyObject, IDBxCacheSource
  {
    #region Конструктор

    /// <summary>
    /// Создает объект DBxRealDocProviderSource.
    /// Для переданного <paramref name="globalData"/> будет вызван SetReadOnly().
    /// </summary>
    /// <param name="globalData">Общие данные по системе документов без выделения прав отдельных пользователей</param>
    public DBxRealDocProviderSource(DBxRealDocProviderGlobal globalData)
    {
#if DEBUG
      if (globalData == null)
        throw new ArgumentNullException("globalData");
#endif
      globalData.SetReadOnly(); // 17.11.2017
      _GlobalData = globalData;

      _MainDBEntry = globalData.MainDBEntry;

      _UserPermissions = UserPermissions.Empty;

      _DisplayName = "[ Без имени ]";

      _SyncRoot = new object();

      //_TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Общие данные для всех пользователей
    /// </summary>
    public DBxRealDocProviderGlobal GlobalData { get { return _GlobalData; } }
    private DBxRealDocProviderGlobal _GlobalData;

    /// <summary>
    /// Настраиваемые разрешения, назначенные пользователю
    /// Если свойство не установлено в явном виде, возвращается пустой список UserPermissions.Empty.
    /// Для установки разрешений требуется создать новый объект UserPermissions, заполнить его, 
    /// а затем - установить это свойство. При этом объект UserPermissions переводится в состояние ReadOnly
    /// </summary>
    public UserPermissions UserPermissions
    {
      get { return _UserPermissions; }
      set
      {
        CheckNotReadOnly();
        _UserPermissions = value;
        _UserPermissions.SetReadOnly();
      }
    }
    private UserPermissions _UserPermissions;

    /// <summary>
    /// Подключение к основной базе данных.
    /// Из нее извлекаются права пользователя на объекты базы данных
    /// Свойство должно быть установлено обязательно, иначе пользователь получит неограниченные права
    /// </summary>
    public DBxEntry MainDBEntry
    {
      get { return _MainDBEntry; }
      set
      {
        CheckNotReadOnly();
        if (value == null)
          throw new ArgumentNullException();
        _MainDBEntry = value;
      }
    }
    private DBxEntry _MainDBEntry;

    /// <summary>
    /// Идентификатор базы данных. Используется для работы буфера обмена.
    /// </summary>
    public string DBIdentity { get { return MainDBEntry.DB.DBIdentity; } }

    /// <summary>
    /// Отображаемое имя
    /// </summary>
    public string DisplayName
    {
      get { return _DisplayName; }
      set
      {
        CheckNotReadOnly();
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Возвращает DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region Кэширование

    /// <summary>
    /// Использовать в качестве блокируемого объекта GlobalData или typeof() не стоит
    /// </summary>
    private object _SyncRoot;

    /// <summary>
    /// Потокобезопасный кэш данных на стороне сервера
    /// Объекты DBxRealDocProvider, связанные с текущим источником, возвращают этот объект
    /// Пока IsReadOnly=false, свойство возвращает null
    /// </summary>
    public DBxCache DBCache
    {
      get
      {
        lock (_SyncRoot)
        {
          if (_DBCache == null)
          {
            if (IsReadOnly)
            {
              // сделано при переходе в режим ReadOnly
              //_StructSource = new InternalStructSource(this);
              _DBCache = new DBxCache(this, false);
            }
            else
              return null;
          }
          return _DBCache;
        }
      }
    }

    private DBxCache _DBCache;

    /// <summary>
    /// Вызывает DBxCache.ClearCache()
    /// </summary>
    public void ClearCache()
    {
      lock (_SyncRoot)
      {
        if (_DBCache != null)
          _DBCache.Clear();
      }
    }

    /// <summary>
    /// Вызывает статический метод DBxDocProvider.ClearCache()
    /// </summary>
    /// <param name="ds">Набор таблиц и DBxDocSet</param>
    public void ClearCache(DataSet ds)
    {
      lock (_SyncRoot)
      {
        if (_DBCache != null)
          DBxDocProvider.ClearCache(ds, _DBCache);
      }
    }

    #region IDBxCacheSource Members

    IDBxStructSource IDBxCacheSource.StructSource { get { return _StructSource; } }
    private DBxDocStructSource _StructSource;


    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      if (_StructSource == null)
        return null;
      else
        return _StructSource.GetTableCacheInfo(tableName);
    }

#if XXX
    /// <summary>
    /// Коллекция описаний полей таблиц для буферизации.
    /// Ключ - имя таблицы.
    /// На момент обращения коллекция блокируется для обеспечения потокобезопасности
    /// </summary>
    private Dictionary<string, DBxTableCacheInfo> _TableCacheInfos;

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo Info;
      lock (_SyncRoot)
      {
        if (!_TableCacheInfos.TryGetValue(tableName, out Info))
        {
          if (tableName == "FileNames")
          {
            Info = new DBxTableCacheInfo(GlobalData.BinDataHandler.MainEntry.DB.Struct.Tables[tableName]);
            Info.SetReadOnly();
          }
          else if (tableName == "BinData")
          {
            Info = new DBxTableCacheInfo(GlobalData.BinDataHandler.BinDataCacheTableStruct);
            Info.SetReadOnly();
          }
          else
          {
            DBxDocTypeBase DocTypeBase;
            if (!GlobalData.DocTypes.FindByTableName(tableName, out DocTypeBase))
              throw new ArgumentException("Неизвестное имя таблицы \"" + tableName + "\"", "TableName");

            if (DocTypeBase.IndividualCacheColumns.AreAllDefaults)
              Info = null;
            else
            {
              if (_StructSource == null)
                return null;
              Info = new DBxTableCacheInfo(_StructSource.GetTableStruct(tableName));
              for (int i = 0; i < Info.TableStruct.Columns.Count; i++)
              {
                string ColumnName = Info.TableStruct.Columns[i].ColumnName;
                if (DocTypeBase.Struct.Columns.Contains(ColumnName))
                  Info.IndividualColumnFlags[i] = DocTypeBase.IndividualCacheColumns[ColumnName];
                // Для полей типа "Id" нет соответствующего описания в структуре полей DBxDocTypeBase.Struct
              }
              Info.SetReadOnly();
            }
          }
          _TableCacheInfos.Add(tableName, Info);
        }
      }
      return Info;
    }
#endif

    DBxCacheLoadResponse IDBxCacheSource.LoadCachePages(DBxCacheLoadRequest request)
    {
      // В одном запросе могут быть обращения к таблицам основной базы данных и к таблице двоичных данных

      // Разбиваем по таблицам
      Dictionary<string, DBxCacheLoadRequest> dict1 = request.SplitByTables();

      // Разбиваем по базам данных
      Dictionary<DBxEntry, List<DBxCacheLoadRequest>> dict2 = new Dictionary<DBxEntry, List<DBxCacheLoadRequest>>();
      foreach (KeyValuePair<string, DBxCacheLoadRequest> pair1 in dict1)
      {
        DBxEntry entr = GetEntry(pair1.Key);
        List<DBxCacheLoadRequest> lst;
        if (!dict2.TryGetValue(entr, out lst))
        {
          lst = new List<DBxCacheLoadRequest>();
          dict2.Add(entr, lst);
        }
        lst.Add(pair1.Value);
      }

      List<DBxCacheLoadResponse> resps = new List<DBxCacheLoadResponse>();
      foreach (KeyValuePair<DBxEntry, List<DBxCacheLoadRequest>> pair2 in dict2)
      {
        using (DBxCon Con = new DBxCon(pair2.Key))
        {
          for (int i = 0; i < pair2.Value.Count; i++)
          {
            DBxCacheLoadResponse r = Con.LoadCachePages(pair2.Value[i]);
            resps.Add(r);
          }
        }
      }

      return DBxCacheLoadResponse.Merge(resps.ToArray());
    }

    void IDBxCacheSource.ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstId)
    {
      // Ничего не делаем
    }

    private DBxEntry GetEntry(string TableName)
    {
      if (TableName == "BinData" || TableName == "FileNames")
        return GlobalData.BinDataHandler.MainEntry;
      else
        return MainDBEntry;
    }

    #endregion

    /// <summary>
    /// Получение текстового представления для документа.
    /// Если у пользователя нет прав на чтение документов, возвращает только идентификатор.
    /// Наличие прав на отдельные поля не проверяется
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextValue(string tableName, Int32 id)
    {
      if (MainDBEntry.Permissions.TableModes[tableName] == DBxAccessMode.None)
        return "Id=" + id.ToString();
      else
        return GlobalData.TextHandlers.GetTextValue(tableName, id);
    }

    internal string InternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
    {
      if (MainDBEntry.Permissions.TableModes[tableName] == DBxAccessMode.None)
        return "Id=" + id.ToString();
      else
      {
        try
        {
          return GlobalData.TextHandlers.DoGetTextValue(tableName, id, primaryDS, false);
        }
        catch (Exception e)
        {
          return "Id=" + id.ToString() + ". Ошибка. " + e.Message;
        }
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект DBxRealDocProviderSource был переведен в режим "только чтение".
    /// При этом нельзя устанавливать управляющие свойства.
    /// Не имеет отношение к правам на изменение документов.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит объект в режим "только чтение".
    /// Нет необходимости вызывать метод из пользовательского кода, так как он вызывается при 
    /// создании DBxRealDocProvider.
    /// </summary>
    public void SetReadOnly()
    {
      if (_IsReadOnly)
        return;

      //if (FDocTypes == null)
      //  throw new NullReferenceException("Не установлено свойство DocTypes");
      if (_MainDBEntry == null)
        throw new NullReferenceException("Не установлено свойство MainDBEntry");

      _StructSource = new DBxDocStructSource(_GlobalData.DocTypes,
        _MainDBEntry.Permissions, new DBxBinDataHandlerInfo(_GlobalData.BinDataHandler));

      _IsReadOnly = true;
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Это событие вызывается до применения изменений к документам.
    /// Обработчик события может, например, отслеживать отключение пользователя.
    /// Обработчик должен быть потокобезопасным
    /// </summary>
    public event DBxBeforeApplyChangesEventHandler BeforeApplyChanges;

    /// <summary>
    /// Вызывает событие BeforeApplyChanges, если обработчик установлен
    /// </summary>
    /// <param name="docSet">Набор данных, для которого применяются изменения</param>
    public void PerformBeforeApplyChanges(DBxDocSet docSet)
    {
      if (BeforeApplyChanges != null)
      {
        DBxBeforeApplyChangesEventArgs Args = new DBxBeforeApplyChangesEventArgs(docSet);
        BeforeApplyChanges(this, Args);
      }
    }

    #endregion
  }

  /// <summary>
  /// Сериализуемая информация о DBxBinDataHandler, передаваемая в DBxDocProvider
  /// </summary>
  [Serializable]
  internal class DBxBinDataHandlerInfo
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта.
    /// Аргумент может быть null
    /// </summary>
    /// <param name="handler">Обработчик двоичных данных на стороне сервера или null</param>
    public DBxBinDataHandlerInfo(DBxBinDataHandler handler)
    {
      if (handler != null)
      {
        _UseBinData = handler.UseBinData;
        _UseFiles = handler.UseFiles;

        // Объект DBxStruct, в отличие от DBxTableStruct, не является сериализуемым.
        // Нужно хранить в полях DBxTableStruct
        DBxStruct dbs = new DBxStruct();
        handler.AddMainTableStructs(dbs);
        _BinDataTableStruct = dbs.Tables["BinData"];
        _FileNamesTableStruct = dbs.Tables["FileNames"];
      }
    }

    #endregion

    #region Свойства

    public bool UseBinData { get { return _UseBinData; } }
    private bool _UseBinData;

    public bool UseFiles { get { return _UseFiles; } }
    private bool _UseFiles;

    public DBxTableStruct BinDataTableStruct { get { return _BinDataTableStruct; } }
    private DBxTableStruct _BinDataTableStruct;

    public DBxTableStruct FileNamesTableStruct { get { return _FileNamesTableStruct; } }
    private DBxTableStruct _FileNamesTableStruct;

    #endregion

    #region Методы

    public DBxTableStruct GetTableStruct(string tableName)
    {
      if (_BinDataTableStruct != null)
      {
        if (tableName == _BinDataTableStruct.TableName)
          return _BinDataTableStruct;
      }

      if (_FileNamesTableStruct != null)
      {
        if (tableName == _FileNamesTableStruct.TableName)
          return _FileNamesTableStruct;
      }
      return null;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойств DBxDocProvider и DBxRealDocProviderSource.StructSource
  /// Получение "урезанных" структур, в соответствии с правами пользователя.
  /// Класс является потокобезопасным
  /// </summary>
  internal class DBxDocStructSource : IDBxStructSource
  {
    #region Конструктор

    internal DBxDocStructSource(DBxDocTypes docTypes, DBxPermissions dbPermissions, DBxBinDataHandlerInfo binDataInfo)
    {
      _DocTypes = docTypes;
      _DBPermissions = dbPermissions;
      _BinDataInfo = binDataInfo;

      _StructDict = new Dictionary<string, DBxTableStruct>(); // используем в качестве блокируемого объекта
    }

    #endregion

    #region Исходные данные

    DBxDocTypes _DocTypes;
    DBxPermissions _DBPermissions;
    DBxBinDataHandlerInfo _BinDataInfo;

    /// <summary>
    /// Объект для синхронизации во всех операциях.
    /// Также стараемся обходиться без блокировки
    /// </summary>
    private object SyncRoot { get { return _StructDict; } }

    #endregion

    #region Полная структура

    /// <summary>
    /// Структура основной базы данных, созданная DBxDocTypes.GetMainDBStruct().
    /// Содержит все поля документов и поддокументов, включая те, к которым нет доступа.
    /// Инициализируется при первом обращении
    /// </summary>
    /// <returns></returns>
    private DBxStruct MainDBstruct
    {
      get
      {
        if (_MainDBstruct == null)
          _MainDBstruct = _DocTypes.GetMainDBStruct();
        return _MainDBstruct;
      }
    }
    private DBxStruct _MainDBstruct;

    #endregion

    #region Список таблиц, к которым есть доступ

    /// <summary>
    /// Возвращает полный список таблиц, к которым у пользователя есть доступ
    /// Включает таблицы "BinData" и "FileNames"
    /// </summary>
    /// <returns></returns>
    public string[] GetAllTableNames()
    {
      if (_AllTableNames == null)
      {
        lock (SyncRoot)
        {
          if (_AllTableNames == null)
          {
            List<string> lst = new List<string>();
            foreach (DBxDocType DocType in _DocTypes)
            {
              if (_DBPermissions.TableModes[DocType.Name] == DBxAccessMode.None)
                continue;
              lst.Add(DocType.Name);
              foreach (DBxSubDocType SubDocType in DocType.SubDocs)
              {
                if (_DBPermissions.TableModes[SubDocType.Name] == DBxAccessMode.None)
                  continue;
                lst.Add(SubDocType.Name);
              }
            }

            if (_BinDataInfo.BinDataTableStruct != null)
              lst.Add(_BinDataInfo.BinDataTableStruct.TableName);
            if (_BinDataInfo.FileNamesTableStruct != null)
              lst.Add(_BinDataInfo.FileNamesTableStruct.TableName);

            _AllTableNames = lst.ToArray();
          }
        }
      }
      return _AllTableNames;
    }

    private volatile string[] _AllTableNames;

    #endregion

    #region Урезанные структуры

    /// <summary>
    /// Урезанные структуры.
    /// Ключ - имя таблицы, значение - структура
    /// </summary>
    private Dictionary<string, DBxTableStruct> _StructDict;

    /// <summary>
    /// Возвращает структуру таблицы документа или поддокумента, а также таблиц
    /// "BinData" и "FileNames".
    /// В структуру таблицы входят все служебные поля (при соответствующих
    /// установленных флажках в DBxDocTypes): Id, Deleted, CreateUserId и т.п.
    /// В структуру не входят поля, к которым у пользователя нет разрешения на доступ.
    /// Если запрошено несуществующее имя таблицы, генерируется исключение.
    /// Если у пользователя нет разрешения на просмотр таблицы, генерируется исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Пустая таблица, содержащая структуру. В ней присутствует поле Id </returns>
    public DBxTableStruct GetTableStruct(string tableName)
    {
      //try
      //{
      lock (SyncRoot)
      {
        DBxTableStruct ts;
        if (!_StructDict.TryGetValue(tableName, out ts))
        {
          ts = DoGetTableStruct(tableName);
          _StructDict[tableName] = ts;
        }
        return ts;
      }
      //}
      //catch (Exception e)
      //{
      //  AddExceptionInfo(e);
      //  throw;
      //}
    }

    private DBxTableStruct DoGetTableStruct(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      DBxTableStruct ts = _BinDataInfo.GetTableStruct(tableName);
      if (ts != null)
        return ts;

      DBxTableStruct Struct1 = MainDBstruct.Tables[tableName];
      if (Struct1 == null)
        throw new ArgumentException("Неизвестная таблица \"" + tableName + "\"", "tableName");

      if (_DBPermissions.TableModes[tableName] == DBxAccessMode.None)
        throw new DBxAccessException("Доступ к таблице \"" + tableName + "\" запрещен");

      // Создаем "урезанную" копию, проверяя права пользователя.
      DBxTableStruct Struct2 = new DBxTableStruct(tableName);
      for (int i = 0; i < Struct1.Columns.Count; i++)
      {
        if (_DBPermissions.ColumnModes[tableName, Struct1.Columns[i].ColumnName] == DBxAccessMode.None)
          continue;
        Struct2.Columns.Add(Struct1.Columns[i]);
      }
      Struct2.SetReadOnly();
      return Struct2;
    }

    #endregion

    /// <summary>
    /// Коллекция описаний полей таблиц для буферизации.
    /// Ключ - имя таблицы.
    /// На момент обращения коллекция блокируется для обеспечения потокобезопасности
    /// </summary>
    private Dictionary<string, DBxTableCacheInfo> _TableCacheInfos;

    public DBxTableCacheInfo GetTableCacheInfo(string tableName)
    {
      DBxTableCacheInfo Info;
      lock (SyncRoot)
      {
        if (_TableCacheInfos == null)
          _TableCacheInfos = new Dictionary<string, DBxTableCacheInfo>();

        if (!_TableCacheInfos.TryGetValue(tableName, out Info))
        {
          DBxTableStruct ts = GetTableStruct(tableName);
          if (ts == null)
            throw new ArgumentException("Неизвестное имя таблицы \"" + tableName + "\"", "tableName");
          Info = new DBxTableCacheInfo(ts);

          DBxDocTypeBase DocTypeBase;
          if (_DocTypes.FindByTableName(tableName, out DocTypeBase))
          {
            if (!DocTypeBase.IndividualCacheColumns.AreAllDefaults)
            {
              for (int i = 0; i < Info.TableStruct.Columns.Count; i++)
              {
                string ColumnName = Info.TableStruct.Columns[i].ColumnName;
                if (DocTypeBase.Struct.Columns.Contains(ColumnName))
                  Info.IndividualColumnFlags[i] = DocTypeBase.IndividualCacheColumns[ColumnName];
                // Для полей типа "Id" нет соответствующего описания в структуре полей DBxDocTypeBase.Struct
              }
            }
          }
          Info.SetReadOnly();
          _TableCacheInfos.Add(tableName, Info);
        }
      }
      return Info;
    }
  }
}
