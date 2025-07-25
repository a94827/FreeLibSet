﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using FreeLibSet.Core;
using System.Data.Common;

namespace FreeLibSet.Data.Docs
{
  #region InitDBxEventHandler

  /// <summary>
  /// Аргументы события <see cref="DBxDocDBConnectionHelper.BeforeInitDB"/>
  /// </summary>
  public sealed class DBxDocDBConnectionHelperInitDBxEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Конструктор вызывается из DBxDocDBConnectionHelper.InitDB()
    /// </summary>
    internal DBxDocDBConnectionHelperInitDBxEventArgs(DBx db, DBxStruct dbStruct, string dbName)
    {
#if DEBUG
      if (db == null)
        throw new ArgumentNullException("db");
      if (dbStruct == null)
        throw new ArgumentNullException("dbStruct");
      dbStruct.CheckNotReadOnly();
      if (String.IsNullOrEmpty(dbName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("dbName");
#endif

      _DB = db;
      _Struct = dbStruct;
      _DBName = dbName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Созданное описание базы данных.
    /// Свойство <see cref="DBx.Struct"/> не установлено.
    /// </summary>
    public DBx DB { get { return _DB; } }
    private readonly DBx _DB;

    /// <summary>
    /// Структура базы данных.
    /// Пользовательский код может добавить описания таблиц и полей.
    /// </summary>
    public DBxStruct Struct { get { return _Struct; } }
    private readonly DBxStruct _Struct;

    /// <summary>
    /// Имя базы данных, переданное при вызове <see cref="DBxDocDBConnectionHelper.InitDB(string, DBxStruct)"/> ("db", "undo", "files", "files2", ...)
    /// </summary>
    public string DBName { get { return _DBName; } }
    private readonly string _DBName;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DBxDocDBConnectionHelper.BeforeInitDB"/>
  /// </summary>
  /// <param name="sender">Объект DBxDocDBConnectionHelper</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxDocDBConnectionHelperInitDBxEventHandler(object sender,
    DBxDocDBConnectionHelperInitDBxEventArgs args);

  #endregion

  /// <summary>
  /// Вспомогательный класс для создания объектов <see cref="DBx"/> для поддержки работы с документами.
  /// После создания экземпляра и настройки управляющих свойств, следует вызвать метод <see cref="DBxDocDBConnectionHelper.CreateRealDocProviderGlobal()"/>.
  /// </summary>
  public class DBxDocDBConnectionHelper
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект
    /// </summary>
    public DBxDocDBConnectionHelper()
    {
      _CommandTimeout = 30;
      _InitDocTableIds = true;
      _Errors = new ErrorMessageList();
      _UpdateStructOptions = new DBxUpdateStructOptions();
    }

    #endregion

    #region Управляющие параметры для создания БД

    /// <summary>
    /// Имя провайдера базы данных.
    /// </summary>
    public string ProviderName { get { return _ProviderName; } set { _ProviderName = value; } }
    private string _ProviderName;

    /// <summary>
    /// Строка соединения с основной базой данных
    /// </summary>
    public string ConnectionString { get { return _ConnectionString; } set { _ConnectionString = value; } }
    private string _ConnectionString;

#if UNUSED

    // Использование ConnectionStringSettings требует подключения System.Configuration.dll,
    // что вряд ли оправдано.
    // Если приложение хочет читать строку из app.config, пусть выполняет инициализацию свойств
    // ProviderName и ConnectionString вручную

    /// <summary>
    /// Имя провайдера базы данных и строка подключения.
    /// Объект может быть прочитан из конфигурационного файла приложения.
    /// Объект задает подключение для основной базы данных ("db"). Подключения к остальным базам
    /// определяются автоматически. В этой строке подключения имя базы данных может содержать 
    /// произвольный префикс, который используется и для других баз данных. Например, если в 
    /// строке подключения задано "DATABASE=myprog_db", то для базы данных undo будет использовано
    /// "DATABASE=myprog_undo"
    /// Если свойство не установлено (по умолчанию),
    /// то используется провайдер Microsoft SQL Server Express с размещением в DBDir.
    /// </summary>
    /// <remarks>
    /// Это свойство не хранится в явном виде. Вместо этого оно собирается из свойств
    /// ProviderName и ConnectionString.
    /// При каждом чтении свойства создается новый объект
    /// </remarks>
    public System.Configuration.ConnectionStringSettings ConnectionStringSettings
    {
      get 
      {
        if (String.IsNullOrEmpty(ConnectionString))
          return null;
        return new System.Configuration.ConnectionStringSettings("db", ConnectionString, ProviderName); 
      }
      set 
      {
        if (value == null)
        {
          ProviderName = String.Empty;
          ConnectionString = String.Empty;
        }
        else
        {
          ProviderName = value.ProviderName;
          ConnectionString = value.ConnectionString;
        }
      }
    }
#endif

    /// <summary>
    /// Каталог для размещения баз данных MS SQL Server Express.
    /// Свойство используется, если не задано <see cref="ConnectionString"/>
    /// </summary>
    public AbsPath DBDir { get { return _DBDir; } set { _DBDir = value; } }
    private AbsPath _DBDir;

    /// <summary>
    /// Время, отведенное на выполнение SQL-запроса в секундах.
    /// По умолчанию - 30с.
    /// 0 - бесконечное время выполнения
    /// </summary>
    public int CommandTimeout { get { return _CommandTimeout; } set { _CommandTimeout = value; } }
    private int _CommandTimeout;

    /// <summary>
    /// Управление заставкой на время создания/обновления структуры базы данных.
    /// Может быть не задано.
    /// </summary>
    public ISimpleSplash Splash { get { return _Splash; } set { _Splash = value; } }
    private ISimpleSplash _Splash;

    /// <summary>
    /// Сюда добавляются сообщения об ошибках при обновлении структуры базы данных.
    /// После обноввления структуры баз данных, программа может выгрузить список сообщений в log-файл или отправить администратору
    /// </summary>
    public ErrorMessageList Errors { get { return _Errors; } }
    private ErrorMessageList _Errors;

    /// <summary>
    /// Параметры обновления структуры баз данных.
    /// </summary>
    public DBxUpdateStructOptions UpdateStructOptions { get { return _UpdateStructOptions; } }
    private DBxUpdateStructOptions _UpdateStructOptions;

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается методом <see cref="InitDB(string, DBxStruct)"/>.
    /// Обработчик события может выполнить дополнительную инициализацию объекта <see cref="DBx"/> перед его использованием.
    /// На момент вызова установлены свойства <see cref="DBx.DisplayName"/> и <see cref="CommandTimeout"/>,
    /// но свойство <see cref="DBx.Struct"/> не установлено. 
    /// Проверка существования преальной базы данных не выполнена.
    /// </summary>
    public event DBxDocDBConnectionHelperInitDBxEventHandler BeforeInitDB;

    /// <summary>
    /// Вызов события <see cref="BeforeInitDB"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected void OnBeforeInitDB(DBxDocDBConnectionHelperInitDBxEventArgs args)
    {
      if (BeforeInitDB != null)
        BeforeInitDB(this, args);
    }

    #endregion

    #region Создание произвольных подключений

    /// <summary>
    /// Создание произвольной базы данных с заданным именем на основании управляющих параметров.
    /// Если базы данных не существует, она создается. Структура базы данных обновляется для
    /// добавления недостающих элементов структуры, если она задана.
    /// </summary>
    /// <param name="dbName">Имя базы данных, например, "db", "undo"</param>
    /// <param name="dbStruct">Структура базы данных. Если null, то создание базы данных и обновление структуры не выполняется</param>
    /// <returns>Объект подключения к базе данных</returns>
    public DBx InitDB(string dbName, DBxStruct dbStruct)
    {
      #region Проверка аргументов

      if (String.IsNullOrEmpty(dbName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("dbName");
      // if (DBStruct == null)
      //   throw new ArgumentNullException("DBStruct");

      #endregion

      DBx db;

      string oldSplashText = null;
      if (Splash != null)
      {
        oldSplashText = Splash.PhaseText;
        Splash.PhaseText = String.Format(Res.DBxDocConnectionHelper_Phase_DBUpdating, dbName);
      }
      try
      {
        if (String.IsNullOrEmpty(ConnectionString))
        {
          // TODO: Учет ProviderName для выбора формата базы данных

          if (DBDir.IsEmpty)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "DBDir");

          // Строка подключения не задана
#if NET
          Microsoft.Data.SqlClient.SqlConnectionStringBuilder csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
#else
          System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder();
#endif
          csb.DataSource = @".\SQLEXPRESS";
          csb.AttachDBFilename = DBDir.SlashedPath + dbName + ".mdf";
          csb.IntegratedSecurity = true;

          db = new FreeLibSet.Data.SqlClient.SqlDBx(csb);
        }
        else
        {
          if (String.IsNullOrEmpty(ProviderName))
            throw ExceptionFactory.ObjectPropertyNotSet(this, "ProviderName");
          DBxManager dbMan = DBxManager.Managers[ProviderName];
          if (dbMan == null)
          {
            if (DBxManager.Managers.Count == 0)
              throw new NullReferenceException(Res.DBxDocConnectionHelper_Err_DBxManagerListEmpty);
            else
              throw new NullReferenceException(String.Format(Res.DBxDocConnectionHelper_Err_UnknownProviderName,
                ProviderName, UICore.UITools.ValueListToString(DBxManager.Managers.GetCodes())));
          }
          string conStr = dbMan.ReplaceDBName(ConnectionString, "db", dbName);

          if (ProviderName == DBxProviderNames.SQLite && (!DocTypes.UseDeleted)) // 02.02.2022
          {
            DbConnectionStringBuilder csb = dbMan.CreateConnectionStringBuilder(conStr);
            csb["foreign keys"] = true;
            conStr = csb.ConnectionString;
          }

          db = dbMan.CreateDBObject(conStr);
        }

        db.DisplayName = dbName;
        db.CommandTimeout = CommandTimeout;


        if (dbStruct != null)
        {
          // 08.10.2018
          // Пользовательская инициализация
          DBxDocDBConnectionHelperInitDBxEventArgs Args = new DBxDocDBConnectionHelperInitDBxEventArgs(db, dbStruct, dbName);
          OnBeforeInitDB(Args);

          db.CreateIfRequired();
          db.Struct = dbStruct; // возможно, была изменена пользовательским обработчиком

          ISimpleSplash spl = this.Splash ?? new DummySimpleSplash();
          int oldErrorCount = Errors.Count;
          db.UpdateStruct(spl, Errors, UpdateStructOptions);
          Errors.SetPrefix(String.Format(Res.DBxDocConnectionHelper_Err_StructUpdatingPrefix, dbName), oldErrorCount);
        }
      }
      finally
      {
        if (Splash != null)
          Splash.PhaseText = oldSplashText;
      }
      return db;
    }

    /// <summary>
    /// Возвращает true, если база данных с заданным именем существует
    /// </summary>
    /// <param name="dbName">Имя базы данных</param>
    /// <returns>Существование базы данных</returns>
    public bool DatabaseExists(string dbName)
    {
      using (DBx DB = InitDB(dbName, null))
      {
        return DB.DatabaseExists;
      }
    }

    #endregion

    #region Создание DBxRealDocProviderGlobal

    /// <summary>
    /// Сюда должна быть присоединена заполненная структура с объявлениями документов.
    /// Установка свойства приводит к инициализации свойств <see cref="MainDBStruct"/> и <see cref="UndoDBStruct"/>.
    /// </summary>
    public DBxDocTypes DocTypes
    {
      get { return _DocTypes; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value.Count == 0)
          throw ExceptionFactory.ArgIsEmpty("value");
        if (_DocTypes != null)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "DocTypes");
        value.SetReadOnly();
        _DocTypes = value;

        _MainDBStruct = value.GetMainDBStruct();
        _UndoDBStruct = value.GetUndoDBStruct();
      }
    }
    private DBxDocTypes _DocTypes;

    /// <summary>
    /// Должна ли создаваться база данных истории.
    /// Если свойство не задано в явном виде, значение определяется автоматически, исходя
    /// из свойств <see cref="DBxDocTypes"/>.
    /// </summary>
    public bool UseUndo
    {
      get
      {
        if (_UseUndo.HasValue)
          return _UseUndo.Value;
        else if (DocTypes == null)
          return false;
        else
          return DocTypes.UseVersions && DocTypes.UseTime && DocTypes.UseDeleted;
      }
      set
      {
        _UseUndo = value;
      }
    }
    private bool? _UseUndo;

    /// <summary>
    /// Управление фрагментацией в <see cref="DBxBinDataHandler"/>.
    /// Если null (по умолчанию, то определяется автоматически).
    /// </summary>
    public bool? UseBinDataFragmentation
    {
      get { return _UseBinDataFragmentation; }
      set { _UseBinDataFragmentation = value; }
    }
    private bool? _UseBinDataFragmentation;


    /// <summary>
    /// Структура основной базы данных.
    /// Свойство инициализируется после установки <see cref="DocTypes"/>.
    /// Прикладной код может добавить собственные таблицы, не связанные с документами.
    /// </summary>
    public DBxStruct MainDBStruct { get { return _MainDBStruct; } }
    private DBxStruct _MainDBStruct;

    /// <summary>
    /// Структура базы данных истории.
    /// Свойство инициализируется после установки <see cref="DocTypes"/>.
    /// Может быть null, если история не может вестить.
    /// </summary>
    public DBxStruct UndoDBStruct { get { return _UndoDBStruct; } }
    private DBxStruct _UndoDBStruct;

    /// <summary>
    /// Управляющее свойство для <see cref="CreateRealDocProviderGlobal()"/>.
    /// Если true (по умолчанию), то будет выполнен вызов <see cref="DBxDocTypes.InitDocTableIds(DBxEntry)"/>.
    /// </summary>
    public bool InitDocTableIds { get { return _InitDocTableIds; } set { _InitDocTableIds = value; } }
    private bool _InitDocTableIds;

    /// <summary>
    /// Создает корневой объект для доступа к документам.
    /// Предварительно создаются объекты:
    /// - <see cref="DBxBinDataHandler"/>, если есть двоичные данные и/или хранимые файлы.
    /// - Базы данных <see cref="DBx"/> "db", "undo" (при UseUndo=true) и "files" (при необходимости).
    /// </summary>
    /// <returns>Новый <see cref="DBxRealDocProviderGlobal"/></returns>
    public DBxRealDocProviderGlobal CreateRealDocProviderGlobal()
    {
      if (DocTypes == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DocTypes");

      DBxBinDataHandler binDataHandler;
      DBxStruct filesDBStruct; // Структура баз данных files. Используется, если DBxBinDataHandler.UseFragmentation=true.
      InitBinDataHandler(out binDataHandler, out filesDBStruct);

      DBx mainDB = InitDB("db", MainDBStruct);
      DBx undoDB = null;
      if (UseUndo)
      {
        if (UndoDBStruct == null)
          throw new InvalidOperationException(Res.DBxDocConnectionHelper_Err_UndoNotAppliable);
        undoDB = InitDB("undo", UndoDBStruct);
      }

      if (binDataHandler != null)
      {
        binDataHandler.MainEntry = mainDB.MainEntry;
        if (filesDBStruct != null)
        {
          DBx filesDB = InitDB("files", filesDBStruct.Clone());
          binDataHandler.AddSectionEntry(filesDB.MainEntry);

          // Подключаем дополнительные базы данных
          for (int i = 2; i <= int.MaxValue; i++)
          {
            if (DatabaseExists("files" + i.ToString()))
            {
              filesDB = InitDB("files" + i.ToString(), filesDBStruct.Clone());
              binDataHandler.AddSectionEntry(filesDB.MainEntry);
            }
            else
              break;
          }

          binDataHandler.CheckBinDataSections();
          binDataHandler.DBSizeLimitExceeded += new EventHandler(BinDataHandler_DBSizeLimitExceeded);
        }
      }


      if (InitDocTableIds)
        DocTypes.InitDocTableIds(mainDB.MainEntry);

      this.Splash = null; // Объект может остаться "прицепленным" к DBxBinDataHelper

      DBxEntry mainDBEntry = mainDB.MainEntry;
      DBxEntry undoDBEntry = null;
      if (undoDB != null)
        undoDBEntry = undoDB.MainEntry;
      return new DBxRealDocProviderGlobal(DocTypes, mainDBEntry, undoDBEntry, binDataHandler);
    }

    private void InitBinDataHandler(out DBxBinDataHandler binDataHandler, out DBxStruct filesDBStruct)
    {
      binDataHandler = null;
      filesDBStruct = null;

      bool useBinData = DocTypes.HasBinDataRefs;
      bool useFiles = DocTypes.HasFileRefs;

      if (!(useBinData || useFiles))
        return;

      binDataHandler = new DBxBinDataHandler();
      binDataHandler.UseBinData = useBinData;
      binDataHandler.UseFiles = useFiles;

      if (_UseBinDataFragmentation.HasValue)
        binDataHandler.UseFragmentation = _UseBinDataFragmentation.Value;
      else
      {
        using (DBx dummyDB = InitDB("db", null))
        {
          binDataHandler.UseFragmentation = dummyDB.DBSizeLimit < FileTools.TByte;
        }
      }

      binDataHandler.AddMainTableStructs(MainDBStruct);
      if (binDataHandler.UseFragmentation)
      {
        filesDBStruct = new DBxStruct();
        binDataHandler.AddSectionTableStructs(filesDBStruct);
        filesDBStruct.SetReadOnly(); // потом все равно будут создаваться копии
      }
    }

    #endregion

    #region Создание БД files2, files3

    /// <summary>
    /// Обработчик события добавляет базу данных "files2", "files3"
    /// </summary>
    /// <param name="sender">Объект <see cref="DBxBinDataHandler"/></param>
    /// <param name="args">Не используется</param>
    void BinDataHandler_DBSizeLimitExceeded(object sender, EventArgs args)
    {
      DBxBinDataHandler binDataHandler = (DBxBinDataHandler)sender;

      System.Diagnostics.Trace.WriteLine("DBxBinDataHandler.DBSizeLimitExceeded event handler called");

      string dbName = "files" + (binDataHandler.SectionEntryCount + 1).ToString();
      DBxStruct dbs = new DBxStruct();
      binDataHandler.AddSectionTableStructs(dbs);
      DBx db = InitDB(dbName, dbs);
      binDataHandler.AddSectionEntry(db.MainEntry);

      System.Diagnostics.Trace.WriteLine("New database created: " + db.DisplayName);
    }

    #endregion
  }
}
