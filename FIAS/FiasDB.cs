// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Data.Common;
using FreeLibSet.Diagnostics;
using FreeLibSet.IO;
using System.Diagnostics;
using FreeLibSet.Config;
using FreeLibSet.Remoting;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  #region Делегаты

  /// <summary>
  /// Аргументы событий FiasDB.ExecProcXxx
  /// </summary>
  public sealed class FiasExecProcEventArgs : EventArgs
  {
    #region Конструктор

    internal FiasExecProcEventArgs(ExecProc execProc, object userData)
    {
      _ExecProc = execProc;
      _UserData = userData; // 25.12.2020
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект процедуры (закрытого класса)
    /// </summary>
    public ExecProc ExecProc { get { return _ExecProc; } }
    private ExecProc _ExecProc;

    /// <summary>
    /// Произвольные пользовательские данные, задаваемые свойством FiasDistributedSource.UserData
    /// </summary>
    public object UserData { get { return _UserData; } }
    private object _UserData;

    #endregion
  }

  /// <summary>
  /// Делегат событий FiasDB.ExecProcXxx
  /// </summary>
  /// <param name="sender">Объект FiasDB</param>
  /// <param name="args">Аргументы события</param>
  public delegate void FiasExecProcEventHandler(object sender, FiasExecProcEventArgs args);

  #endregion

  /// <summary>
  /// Реализует работу с базой данных классификатора. 
  /// Этот класс создается пользовательским кодом на стороне сервера в единственном экземпляре.
  /// Пользовательский код отвечает за удаление объекта
  /// Отвечает за инициализацию структуры базы данных.
  /// Содержит буферизованный источник данных для работы с классификатором на стороне сервера (свойство Source).
  /// </summary>
  public sealed class FiasDB : SimpleDisposableObject
  {
    // 03.01.2021
    // Можно использовать базовый класс без деструктора

    #region Конструктор и Dispose

    /// <summary>
    /// Создает объект сервера или объект для доступа к классификатору в монолитном приложении.
    /// Эта версия конструктора используется, если приложение использует готовую базу данных и не может ее обновлять.
    /// </summary>
    /// <param name="db">Созданное описание базы данных fias</param>
    public FiasDB(DBx db)
      : this(db, null, false)
    {
    }

    /// <summary>
    /// Создает объект сервера или объект для доступа к классификатору в монолитном приложении.
    /// Задавайте аргумент <paramref name="dbSettings"/>, если приложение отвечает за обновление базы данных классификатора.
    /// Если приложение обращается к базе данных только на просмотр, а за обновление классификатора отвечает другое
    /// приложение, задавайте null.
    /// Задание <paramref name="dbSettings"/> только создает при необходимости пустую базу данных и добавляет недостающие пустые таблицы.
    /// Если таблицы уже существуют, проверяется совместимость переданных настроек и сохраненных в базе данных
    /// Импорт классификатора ФИАС выполняется отдельно с помощью FiasDBUpdater.
    /// </summary>
    /// <param name="db">Созданное описание базы данных fias</param>
    /// <param name="dbSettings">Настройки базы данных. Вызывается FiasDBSettings.SetReadOnly()</param>
    public FiasDB(DBx db, FiasDBSettings dbSettings)
      : this(db, dbSettings, dbSettings != null)
    {
    }

    /// <summary>
    /// Создает объект сервера или объект для доступа к классификатору в монолитном приложении.
    /// Задавайте аргумент <paramref name="dbSettings"/>, если приложение отвечает за обновление базы данных классификатора.
    /// Если приложение обращается к базе данных только на просмотр, а за обновление классификатора отвечает другое
    /// приложение, задавайте null.
    /// Задание <paramref name="updateStruct"/> только создает при необходимости пустую базу данных и добавляет недостающие пустые таблицы.
    /// Если таблицы уже существуют, проверяется совместимость переданных настроек и сохраненных в базе данных.
    /// Импорт классификатора ФИАС выполняется отдельно с помощью FiasDBUpdater.
    /// </summary>
    /// <param name="db">Созданное описание базы данных fias</param>
    /// <param name="dbSettings">Настройки базы данных. Вызывается FiasDBSettings.SetReadOnly()</param>
    /// <param name="updateStruct">Будет ли приложение обновлять структуру базы данных</param>
    public FiasDB(DBx db, FiasDBSettings dbSettings, bool updateStruct)
    {
      if (db == null)
        throw new ArgumentNullException("db");

      _DB = db;
      _DBSettings = dbSettings;
      _UpdateStruct = updateStruct;

      _InternalSettings = new FiasInternalSettings();
      if (db.Manager != null)
        _InternalSettings.ProviderName = db.Manager.ProviderName; // 25.02.2021
      else
        _InternalSettings.ProviderName = String.Empty;
      _InternalSettings.UseIdTables = false;
      _InternalSettings.FTSMode = FiasFTSMode.None;
      if (db is FreeLibSet.Data.SQLite.SQLiteDBx)
      {
        _InternalSettings.UseOADates = true;
        _InternalSettings.FTSMode = FiasFTSMode.FTS3;
      }

      if (dbSettings != null)
        db.CreateIfRequired();
      else
      {
        if (!db.DatabaseExists)
          throw new ArgumentException("База данных не существует: " + db.DatabaseName);
      }

      InitIsEmpty(false); // этот метод работает до установки свойства DBx.Struct.

      if (dbSettings != null)
      {

        if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
        {
          // Таблица для поиска создается до создания таблицы AddrOb, чтобы в AddrOb можно было создать ссылочное поле
          using (DBxConBase con = db.MainEntry.CreateCon())
          {
            DBxSqlBuffer buffer = new DBxSqlBuffer(db.Formatter);
            buffer.SB.Append("CREATE VIRTUAL TABLE IF NOT EXISTS ");
            buffer.FormatTableName("AddrObFTS");
            buffer.SB.Append(" USING fts3(");
            buffer.FormatColumnName("OFFNAME");
            // Не будем ориентироваться на наличие токенайзера icu и то, правильно ли он
            // обрабатывает регистр русских букв.
            // Вместо этого будем руками приводить символы к верхнему регистру и убирать букву "Ё"
            //buffer.SB.Append(" tokenize=icu ru_RU");
            buffer.SB.Append(")");
            con.SQLExecuteScalar(buffer.SB.ToString());
          }
        }

        db.Struct = CreateDBStruct(false);
        db.UpdateStruct();

        using (DBxCon con = new DBxCon(db.MainEntry))
        {
          object oXML;
          if (con.GetValue("ClassifInfo", 1, "DBSettings", out oXML))
          {
            // Сравнение конфигураций
            TempCfg cfg = new TempCfg();
            cfg.AsXmlText = DataTools.GetString(oXML);
            FiasDBSettings dbSettinngs2 = new FiasDBSettings();
            dbSettinngs2.ReadConfig(cfg);
            cfg.Clear();
            dbSettinngs2.WriteConfig(cfg); // еще раз перезаписываем, т.к. формат хранения мог немного поменяться в связи с обновлением FIAS.dll

            TempCfg cfg1 = new TempCfg();
            DBSettings.WriteConfig(cfg1);
            if (cfg1.AsXmlText != cfg.AsXmlText)
              throw new FiasDBSettingsDifferentException("Настройки ФИАС, хранящиеся в базе данных не совпадают с переданными");
          }
          else
          {
            // Первый вызов
            TempCfg cfg = new TempCfg();
            DBSettings.WriteConfig(cfg);

            con.AddRecord("ClassifInfo",
              new DBxColumns("Id,DBSettings,ActualDate,UpdateTime"),
              new object[4] { 1, cfg.AsXmlText, FiasTools.UnknownActualDate, DateTime.Now });
          }
        }
      }
      else // dbSettings=null
      {
        db.Struct = CreateDBStruct(true); // только таблица ClassifInfo
        // не обновляем структуру! db.UpdateStruct();

        using (DBxCon con = new DBxCon(db.MainEntry))
        {
          object oXML;
          if (con.GetValue("ClassifInfo", 1, "DBSettings", out oXML))
          {
            TempCfg cfg = new TempCfg();
            cfg.AsXmlText = DataTools.GetString(oXML);
            _DBSettings = new FiasDBSettings();
            _DBSettings.ReadConfig(cfg);
          }
          else
            throw new DBxConsistencyException("В таблице \"ClassifInfo\" нет строки с идентификатом 1");
        }

        db.Struct = CreateDBStruct(false); // все таблицы
        if (_UpdateStruct)
          db.UpdateStruct();
      }

      if (_UpdateStruct)
      {
        if (db is FreeLibSet.Data.SQLite.SQLiteDBx)
        {
          DbConnectionStringBuilder csb = db.ProviderFactory.CreateConnectionStringBuilder();
          csb.ConnectionString = db.MainEntry.UnpasswordedConnectionString;
          if (MemoryTools.TotalPhysicalMemory >= 2 * FileTools.GByte)
          {
            //csb["cache size"] = 20000; // 1.3 ГБ при максимальном размере страницы 64кБ
            // 09.09.2020. 
            // Если забрать 1.3ГБ, то после этого свойство MemoryTools.AvailableMemoryState будет всегда возвращать Low,
            // т.к. MemoryFailPoint не сможет выделять блок памяти.
            // Х.З. почему, после обновления соединение закрывается и не должно возвращаться в пул, т.к. есть вызов ClearPool().
            // Это - затычка!
            csb["cache size"] = 10000; // 0.66 ГБ при максимальном размере страницы 64кБ
          }

          if (_IsEmpty)
          {
            csb["journal mode"] = "Off";
            csb["synchronous"] = "Off"; // испортится и не жалко.
          }
          //else
          //  csb["journal mode"] = "Default";

          _UpdateDB = _DB.Manager.CreateDBObject(csb.ToString());
          _UpdateDB.DisplayName = _DB.DisplayName + " for update";
          _UpdateDB.Struct = db.Struct;
          _UpdateEntry = _UpdateDB.MainEntry;
        }
        if (_UpdateEntry == null)
          _UpdateEntry = db.MainEntry;
      }

      InitIsEmpty(true); // еще раз, чтобы обновить дату актуальности

      _UnbufferedSource = new FiasDBUnbufferedSource(this);
      _Source = new FiasDBCachedSource(_UnbufferedSource.CreateProxy(), this); // обязательно после инициализации InitIsEmpty()

      //// Открываем соединение и выполняем фиктивную команду, чтобы выполнить текхническое обслуживание БД, если необходимо
      //using (DBxConBase con = DB.MainEntry.CreateCon())
      //{
      //  con.GetRecordCount("AOType");
      //}

      System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDB created. Cache key=" + DataTools.MD5SumFromString(DB.DBIdentity));
      System.Diagnostics.Trace.WriteLine("  Connection string=" + DB.MainEntry.UnpasswordedConnectionString);
    }

    /*
    /// <summary>
    /// Версия классификатора, использующая стандартную базу данных SQLite "fias.db"
    /// </summary>
    /// <param name="Settings"></param>
    public ClassifDB(ClassifSettings Settings)
      :this(CreateDefaultDB(), Settings)
    { 
    }

    private static DBx CreateDefaultDB()
    {
      return new FreeLibSet.ExtDB.SQLite.SQLiteDBx("DataSource=fias.db"); // ???
    } */

    /// <summary>
    /// Удаляет буферизованный источник данных Source.
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDB disposed. Cache key=" + DataTools.MD5SumFromString(DB.DBIdentity));

        if (_Source != null)
        {
          _Source.Dispose();
          _Source = null;
        }

        if (_UpdateDB != null)
        {
          _UpdateDB.Dispose();
          _UpdateDB = null;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Структура базы данных

    /// <summary>
    /// Содержит true, если создание индексов было отложено, так база данных пустая
    /// </summary>
    private bool _IndexDelayed;

    private DBxStruct CreateDBStruct(bool classifInfoOnly)
    {
      DBxStruct dbx = new DBxStruct();
      DBxTableStruct ts;

      #region "ClassifInfo"

      ts = dbx.Tables.Add("ClassifInfo");
      ts.Columns.AddId(); // Фиктивный идентификатор, всегда равен 1
      ts.Columns.AddXmlConfig("DBSettings"); // Данные FiasDBSettings
      ts.Columns.AddDate("ActualDate", false); // Дата актуальности классификатора
      ts.Columns.AddDateTime("UpdateTime", false); // Время загрузки или последнего обновления классификатора
      // 10.09.2020
      if (!classifInfoOnly)
      {
        ts.Columns.AddInt64("AddrObCount");
        if (_DBSettings.UseHouse)
          ts.Columns.AddInt64("HouseCount");
        if (_DBSettings.UseRoom)
          ts.Columns.AddInt64("RoomCount");
      }

      #endregion

      if (!classifInfoOnly)
      {
        _IndexDelayed = _IsEmpty;

        //bool NoPK = true; // Не использовать первичный ключ типа GUID в таблицах. Будет неявный первичный ключ по rowid.
        // А для полей AOID и прочих, используем индекс.
        bool noPK = (_DB is FreeLibSet.Data.SQLite.SQLiteDBx); // 30.07.2020


        #region "ClassifUpdate"

        ts = dbx.Tables.Add("ClassifUpdate");
        ts.Columns.AddId();
        ts.Columns.AddDate("ActualDate", false); // Дата актуальности классификатора
        ts.Columns.AddInt("Source", DataTools.GetEnumRange(typeof(FiasDBUpdateSource)), false); // Источник данных
        ts.Columns.AddBoolean("Cumulative", false); // false - первоначальная загрузка, true - накопительное обновление
        ts.Columns.AddDateTime("StartTime", false); // Время начала обновления
        ts.Columns.AddDateTime("FinishTime", true); // Время окончания обновления. Если null, то обновление не закончено
        ts.Columns.AddInt("ErrorCount", true); // Количество ошибок при установке обновления
        // 10.09.2020
        ts.Columns.AddInt64("AddrObCount");
        if (_DBSettings.UseHouse)
          ts.Columns.AddInt64("HouseCount");
        if (_DBSettings.UseRoom)
          ts.Columns.AddInt64("RoomCount");

        #endregion

        #region "AOType"

        ts = dbx.Tables.Add("AOType");
        ts.Columns.AddId();
        ts.Columns.AddInt("LEVEL", DataTools.GetEnumRange(typeof(FiasLevel)));
        ts.Columns.AddString("SCNAME", 30, false);
        ts.Columns.AddString("SOCRNAME", 50, false);

        // Не нужен. ts.Indices.Add("LEVEL,SCNAME");

        #endregion

        if (_InternalSettings.UseIdTables)
        {
          #region "AddrObRec"

          ts = dbx.Tables.Add("AddrObRec");
          ts.Columns.AddId();
          ts.Columns.AddGuid("Guid", false);
          ts.Indexes.Add("Guid");

          #endregion

          #region "AddrObGuid"

          ts = dbx.Tables.Add("AddrObGuid");
          ts.Columns.AddId();
          ts.Columns.AddGuid("Guid", false);
          ts.Indexes.Add("Guid");

          #endregion
        }

        if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
        {
          ts = dbx.Tables.Add("AddrObFTS");
          ts.AutoCreate = false; // Создается отдельным SQL-запросом
          ts.Columns.AddId("rowid");
          ts.Columns.AddString("OFFNAME", 120, false);
        }

        #region "AddrOb"

        ts = dbx.Tables.Add("AddrOb");
        // Поля для организации дерева объектов
        if (_InternalSettings.UseIdTables)
        {
          ts.Columns.AddReference("Id", "AddrObRec", false); // Идентификатор записи и первичный ключ. Задается как ссылка на таблицу, а не генерируется автоматически
          if (_DBSettings.UseHistory)
          {
            ts.Columns.AddReference("PrevId", "AddrObRec", true);  // Ссылки для отслеживания переименований
            ts.Columns.AddReference("NextId", "AddrObRec", true);  // Ссылки для отслеживания переименований
          }
          ts.Columns.AddReference("AOGuidRef", "AddrObGuid", false); // Идентификатор объекта классификатора
          ts.Columns.AddReference("ParentAOGuidRef", "AddrObGuid", true); // Родительский элемент
        }
        else
        {
          ts.Columns.AddGuid("AOID", false); // Идентификатор записи и первичный ключ. 
          if (DBSettings.UseHistory)
          {
            ts.Columns.AddGuid("PREVID", true);  // Ссылки для отслеживания переименований
            ts.Columns.AddGuid("NEXTID", true);  // Ссылки для отслеживания переименований
          }
          ts.Columns.AddGuid("AOGUID", false); // Идентификатор объекта классификатора
          ts.Columns.AddGuid("PARENTGUID", true); // Родительский элемент

          // 28.02.2020
          // Это - неверно! Поле PARENTGUID указывает на AOGUID родительского объекта, а не на AOID, которое является первичным ключом таблицы
          //ts.Columns.LastAdded.MasterTableName = "AddrOb";
          //ts.Columns.LastAdded.RefType = DBxRefType.Emulation;
        }
        // Поля для адресного объекта
        if (_DBSettings.UseHistory)
        {
          ts.Columns.AddBoolean("Actual", false); // Соответствует полю ACTUALSTATUS в ФИАС. Равен true, если ACTUALSTATUS=1.
          // Если исторические записи не нужны, то у всех записей Actual=true и Live=true и поля не нужны
          ts.Columns.AddBoolean("Live", false); // Соответствует полю LIVESTATUS в ФИАС
        }
        ts.Columns.AddInt("AOLEVEL", /*DataTools.GetEnumRange(typeof(FiasLevel))*/ 1, 99, false); // Уровень адресного объекта
        ts.Columns.AddString("OFFNAME", 120, false); // Наименование объекта. Нельзя делать короче, так как есть такие ужасные названия
        if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
          ts.Columns.AddReference("NameId", "AddrObFTS", false);
        ts.Columns.AddReference("AOTypeId", "AOType", false, DBxRefType.Emulation); // Псевдо-ссылка на таблицу "AOType". 
        //В ФИАС используется текстовое поле SHORTNAME. Заменено на ссылку для экономии места.
        ts.Columns.AddInt("CENTSTATUS", DataTools.GetEnumRange(typeof(FiasCenterStatus)), false); // Статус центра
        ts.Columns.AddInt("DIVTYPE", DataTools.GetEnumRange(typeof(FiasDivType)), false); // Признак адресации:
        ts.Columns.AddInt("REGIONCODE", 0, 99, false);	// Код региона. Для экономии места храним как число. Поле не может иметь значения NULL
        ts.Columns.AddInt("POSTALCODE", 0, 999999, false);	// Почтовый индекс
        if (_DBSettings.UseIFNS)
        {
          ts.Columns.AddInt("IFNSFL", 0, 9999, false);
          ts.Columns.AddInt("TERRIFNSFL", 0, 9999, false);
          ts.Columns.AddInt("IFNSUL", 0, 9999, false);
          ts.Columns.AddInt("TERRIFNSUL", 0, 9999, false);
        }
        if (_DBSettings.UseOKATO)
          ts.Columns.AddInt("OKATO", 0, 99999999999L, false);
        if (_DBSettings.UseOKTMO)
          ts.Columns.AddInt("OKTMO", 0, 99999999999L, false);

        if (_DBSettings.UseDates)
        {
          if (_InternalSettings.UseOADates)
          {
            ts.Columns.AddInt("dStartDate", false);
            ts.Columns.AddInt("dEndDate", false);
          }
          else
          {
            ts.Columns.AddDate("STARTDATE", false);
            ts.Columns.AddDate("ENDDATE", false);
          }
        }

        if (noPK)
          ts.PrimaryKey = DBxColumns.Empty;

        if (!_IndexDelayed)
        {
          if (!_InternalSettings.UseIdTables)
          {
            if (noPK)
              ts.Indexes.Add("AOID"); // псевдо-первичный ключ
            ts.Indexes.Add("AOGUID"); // используется, когда в адресе задан GUID адресного объекта, то есть в большинстве случаев
            ts.Indexes.Add("PARENTGUID"); // используетсч при загрузке буферизованной страницы.
          }
          if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
            ts.Indexes.Add("NameID"); // используется при полнотекстном поиске, чтобы получить найденные адресные объекты
        }

        #endregion

        if (_DBSettings.UseHouse)
        {
          //#region "HouseRec"

          //  ts = dbx.Tables.Add("HouseRec");
          //  ts.Columns.AddId();
          //  ts.Columns.AddGuid("Guid", false);
          //  ts.Indices.Add("Guid");

          //  #endregion

          //#region "HouseGuid"

          //  ts = dbx.Tables.Add("HouseGuid");
          //  ts.Columns.AddId();
          //  ts.Columns.AddGuid("Guid", false);
          //  ts.Indices.Add("Guid");

          //  #endregion

          #region "House"

          ts = dbx.Tables.Add("House");
          // Поля для организации дерева объектов
          /*
          ts.Columns.AddReference("Id", "HouseRec", false); // Идентификатор записи и первичный ключ. Задается как ссылка на таблицу, а не генерируется автоматически
          //ts.Columns.AddReference("PrevId", "HouseRec", true);  // Ссылки для отслеживания переименований
          //ts.Columns.AddReference("NextId", "HouseRec", true);  // Ссылки для отслеживания переименований
          ts.Columns.AddReference("HouseGuidRef", "HouseGuid", false); // Идентификатор объекта классификатора
          ts.Columns.AddReference("ParentAOGuidRef", "AddrObGuid", true); // Родительский элемент
           * */
          ts.Columns.AddGuid("HOUSEID", false); // Идентификатор записи и первичный ключ. 
          ts.Columns.AddGuid("HOUSEGUID", false); // Идентификатор объекта классификатора
          ts.Columns.AddGuid("AOGUID", true); // Родительский элемент


          // Поля для дома
          ts.Columns.AddInt("nHouseNum", 0, 255, false); // Функция FiasTools.GetNumInt(HOUSENUM)
          ts.Columns.AddString("HOUSENUM", 20, true); // Номер дома. Нельзя делать короче, так как есть такие ужасные дома
          ts.Columns.AddInt("nBuildNum", 0, 255, false); // Функция FiasTools.GetNumInt(BUILDNUM)
          ts.Columns.AddString("BUILDNUM", 10, true); // Номер корпуса.
          ts.Columns.AddInt("nStrucNum", 0, 255, false); // Функция FiasTools.GetNumInt(STRUCNUM)
          ts.Columns.AddString("STRUCNUM", 10, true); // Номер строения
          ts.Columns.AddInt("STRSTATUS", DataTools.GetEnumRange(typeof(FiasStructureStatus)), false); // Статус строения
          ts.Columns.AddInt("ESTSTATUS", DataTools.GetEnumRange(typeof(FiasEstateStatus)), false); // Признак владения
          //ts.Columns.AddInt("STARSTATUS", 0, 99, false); // размерность под вопросом

          ts.Columns.AddInt("DIVTYPE", DataTools.GetEnumRange(typeof(FiasDivType)), false); // Признак адресации:
          // Не нужен ts.Columns.AddInt("REGIONCODE", 0, 99, _UseNullableCodes);	// Код региона.
          ts.Columns.AddInt("POSTALCODE", 0, 999999, false);	// Почтовый индекс
          if (_DBSettings.UseIFNS)
          {
            ts.Columns.AddInt("IFNSFL", 0, 9999, false);
            ts.Columns.AddInt("TERRIFNSFL", 0, 9999, false);
            ts.Columns.AddInt("IFNSUL", 0, 9999, false);
            ts.Columns.AddInt("TERRIFNSUL", 0, 9999, false);
          }
          if (_DBSettings.UseOKATO)
            ts.Columns.AddInt("OKATO", 0, 99999999999L, false);
          if (_DBSettings.UseOKTMO)
            ts.Columns.AddInt("OKTMO", 0, 99999999999L, false);
          if (_DBSettings.UseDates)
          {
            if (_InternalSettings.UseOADates)
            {
              ts.Columns.AddInt("dStartDate", false);
              ts.Columns.AddInt("dEndDate", false);
            }
            else
            {
              ts.Columns.AddDate("STARTDATE", false);
              ts.Columns.AddDate("ENDDATE", false);
            }
          }

          if (noPK)
            ts.PrimaryKey = DBxColumns.Empty;

          if (!_IndexDelayed)
          {
            if (!_InternalSettings.UseIdTables)
            {
              if (noPK)
                ts.Indexes.Add("HOUSEID"); // псевдо-первичный ключ

              ts.Indexes.Add("HOUSEGUID"); // используется, если в адресе задан GUID здания
              ts.Indexes.Add("AOGUID"); // используется при загрузке буферизованной страницы
            }
          }

          #endregion
        }

        if (_DBSettings.UseRoom)
        {

          //#region "RoomRec"

          //ts = dbx.Tables.Add("RoomRec");
          //ts.Columns.AddId();
          //ts.Columns.AddGuid("Guid", false);
          //ts.Indices.Add("Guid");

          //#endregion

          //#region "RoomGuid"

          //ts = dbx.Tables.Add("RoomGuid");
          //ts.Columns.AddId();
          //ts.Columns.AddGuid("Guid", false);
          //ts.Indices.Add("Guid");

          //#endregion

          #region "Room"

          ts = dbx.Tables.Add("Room");
          // Поля для организации дерева объектов
          /*
          ts.Columns.AddReference("Id", "RoomRec", false); // Идентификатор записи и первичный ключ. Задается как ссылка на таблицу, а не генерируется автоматически
          ts.Columns.AddReference("PrevId", "RoomRec", true);  // Ссылки для отслеживания переименований
          ts.Columns.AddReference("NextId", "RoomRec", true);  // Ссылки для отслеживания переименований
          ts.Columns.AddReference("RoomGuidRef", "RoomGuid", false); // Идентификатор объекта классификатора
          ts.Columns.AddReference("ParentHouseGuidRef", "HouseGuid", true); // Родительский элемент
           * */
          ts.Columns.AddGuid("ROOMID", false); // Идентификатор записи и первичный ключ. 
          if (DBSettings.UseHistory)
          {
            ts.Columns.AddGuid("PREVID", true);  // Ссылки для отслеживания переименований
            ts.Columns.AddGuid("NEXTID", true);  // Ссылки для отслеживания переименований
          }
          ts.Columns.AddGuid("ROOMGUID", false); // Идентификатор объекта классификатора
          ts.Columns.AddGuid("HOUSEGUID", true); // Родительский элемент

          // Поля для объекта
          if (DBSettings.UseHistory)
            ts.Columns.AddBoolean("Live", false); // Соответствует полю LIVESTATUS в ФИАС
          ts.Columns.AddInt("nFlatNumber", 0, 255, false); // Функция FiasTools.GetNumInt(FLATNUMBER)
          ts.Columns.AddString("FLATNUMBER", 50, true); // Номер квартиры, офиса или прочего
          ts.Columns.AddInt("FLATTYPE", DataTools.GetEnumRange(typeof(FiasFlatType)), false); // Тип помещения
          ts.Columns.AddInt("nRoomNumber", 0, 255, false); // Функция FiasTools.GetNumInt(ROOMNUMBER)
          ts.Columns.AddString("ROOMNUMBER", 50, true); // Номер комнаты
          ts.Columns.AddInt("ROOMTYPE", DataTools.GetEnumRange(typeof(FiasRoomType)), false); // Тип комнаты
          ts.Columns.AddInt("POSTALCODE", 0, 999999, false);	// Почтовый индекс
          if (_DBSettings.UseDates)
          {
            if (_InternalSettings.UseOADates)
            {
              ts.Columns.AddInt("dStartDate", false);
              ts.Columns.AddInt("dEndDate", false);
            }
            else
            {
              ts.Columns.AddDate("STARTDATE", false);
              ts.Columns.AddDate("ENDDATE", false);
            }
          }

          if (noPK)
            ts.PrimaryKey = DBxColumns.Empty;

          if (!_IndexDelayed)
          {
            if (!_InternalSettings.UseIdTables)
            {
              if (noPK)
                ts.Indexes.Add("ROOMID"); // псевдопервичный ключ

              ts.Indexes.Add("ROOMGUID"); // используется, если в адресе задан GUID помещения
              ts.Indexes.Add("HOUSEGUID"); // используется при загрузке буферизованной страницы
            }
          }

          #endregion
        }
      } // !classifInfoOnly

      return dbx;
    }


    #endregion

    #region Свойства

    /// <summary>
    /// База данных
    /// </summary>
    public DBx DB { get { return _DB; } }
    private readonly DBx _DB;

    /// <summary>
    /// Альтернативный доступ к базе данных для обновления
    /// </summary>
    private DBx _UpdateDB;

    /// <summary>
    /// Точка подключения к базе данных для выполнения обновления (с кэшем большего размера)
    /// </summary>
    internal DBxEntry UpdateEntry { get { return _UpdateEntry; } }
    private readonly DBxEntry _UpdateEntry;

    /// <summary>
    /// Настройки базы данных
    /// </summary>
    public FiasDBSettings DBSettings { get { return _DBSettings; } }
    private readonly FiasDBSettings _DBSettings;

    /// <summary>
    /// Внутренние параметры классификатора, которые пользователь настраивать не может
    /// </summary>
    internal FiasInternalSettings InternalSettings { get { return _InternalSettings; } }
    private FiasInternalSettings _InternalSettings;

    internal bool UpdateStruct { get { return _UpdateStruct; } }
    private readonly bool _UpdateStruct;

    private FiasDBUnbufferedSource _UnbufferedSource;

    /// <summary>
    /// Буферизованный источник данных.
    /// Существует до вызова метода Dispose().
    /// Используется для инициализации объекта FiasHandler на стороне сервера.
    /// Передается клиенту через net remoting для создания собственного объекта FiasCachedSource.
    /// </summary>
    public FiasCachedSource Source { get { return _Source; } }
    private FiasDBCachedSource _Source;


    /// <summary>
    /// Возвращает true, если в данный момент классфикатор не загружен
    /// </summary>
    public bool IsEmpty { get { return _IsEmpty; } }
    private bool _IsEmpty;

    private void InitIsEmpty(bool updateStat)
    {
      using (DBxConBase con = _DB.MainEntry.CreateCon())
      {
        // Т.к. метод может вызываться до инициализации структуры базы данных, таблицы может не существовать
        con.Validator.NameCheckingEnabled = false;

        // Для SQLite можно было бы вызвать  SELECT COUNT(*) FROM sqlite_master WHERE type=="table" AND name=="AddrOb"

        DoInitIsEmpty(con, updateStat);

        //System.Data.DataTable xxx = con.FillSelect("AddrOb");
        //object[] a = xxx.Rows[0].ItemArray;
      }
    }

    [DebuggerStepThrough]
    private void DoInitIsEmpty(DBxConBase con, bool updateStat)
    {
      bool oldLSE = con.LogoutSqlExceptions;
      con.LogoutSqlExceptions = false; // 28.04.2020
      try
      {
        _IsEmpty = con.IsTableEmpty("AddrOb");
      }
      catch
      {
        _IsEmpty = true;
      }

      if (updateStat)
      {
        FiasDBStat stat = new FiasDBStat();
        if (!_IsEmpty)
          try
          {
            object oActualDate;
            if (con.GetValue("ClassifInfo", 1, "ActualDate", out oActualDate))
            {
              stat.ActualDate = DataTools.GetNullableDateTime(oActualDate) ?? FiasTools.UnknownActualDate;
              stat.AddrObCount = DataTools.GetInt64(con.GetValue("ClassifInfo", 1, "AddrObCount"));
              if (DBSettings.UseHouse)
                stat.HouseCount = DataTools.GetInt64(con.GetValue("ClassifInfo", 1, "HouseCount"));
              if (DBSettings.UseRoom)
                stat.RoomCount = DataTools.GetInt64(con.GetValue("ClassifInfo", 1, "RoomCount"));
            }
          }
          catch { }

        _DBStat = stat;
      }

      con.LogoutSqlExceptions = oldLSE;
    }

    /// <summary>
    /// Статистика по базе данных
    /// </summary>
    public FiasDBStat DBStat { get { return _DBStat; } }
    private volatile FiasDBStat _DBStat;

    /// <summary>
    /// Дата актуальности классификатора
    /// </summary>
    public DateTime ActualDate { get { return _DBStat.ActualDate; } }

    //private static object _SyncRoot = new object();

    #endregion

    #region Статистика по базе данных

    /// <summary>
    /// Получает статистику по базе данных.
    /// Этот метод выполняется медленно, т.к. требует выполнения запросов SELECT COUNT(*), которые не оптимизированы в SQLite
    /// </summary>
    /// <returns></returns>
    public FiasDBStat GetRealDBStat()
    {
      if (IsEmpty)
        return new FiasDBStat(); // 26.02.2021

      using (DBxCon con = new DBxCon(DB.MainEntry))
      {
        return GetRealDBStat(con);
      }
    }

    internal FiasDBStat GetRealDBStat(IDBxCon con)
    {
      FiasDBStat stat = new FiasDBStat();
      //      if (!IsEmpty)
      //      {
      stat.ActualDate = DataTools.GetNullableDateTime(con.GetValue("ClassifInfo", 1, "ActualDate")) ?? FiasTools.UnknownActualDate;
      stat.AddrObCount = con.GetRecordCount("AddrOb");
      if (DBSettings.UseHouse)
        stat.HouseCount = con.GetRecordCount("House");
      if (DBSettings.UseRoom)
        stat.RoomCount = con.GetRecordCount("Room");
      //      }
      return stat;
    }

    #endregion

    #region Процесс обновления

    /// <summary>
    /// Ссылка содержит значение, отличное от null, когда выполняется обновление классификатора
    /// </summary>
    public FiasDBUpdater CurrentUpdater
    {
      get { return _CurrentUpdater; }
      internal set { _CurrentUpdater = value; }
    }
    private FiasDBUpdater _CurrentUpdater;

    internal void AfterUpdate()
    {
      InitIsEmpty(true);
      if (_IndexDelayed && (!_IsEmpty))
      {
        // Требуется создать недостающие индексы
        _DB.Struct = CreateDBStruct(false); // теперь с индексами
        _DB.UpdateStruct();
      }

      _Source.UpdateActualDate();
    }

    /// <summary>
    /// Вызывается в UnbufferedSource
    /// </summary>
    internal void CheckIsReady()
    {
      CheckNotDisposed();
      FiasDBUpdater u = _CurrentUpdater;
      if (u != null)
      {
        if (u.PrimaryLoad)
          throw new InvalidOperationException("В данный момент выполняется первичная загрузка классификатора. Выполнять другие обращения в данный момент нельзя");
      }
    }

    #endregion

    #region События асинхонного выполнения процедур

    /// <summary>
    /// Вызывается при создании процедуры ExecProc для асинхронного выполнения запроса.
    /// Обработчик события может вызываться асинхронно.
    /// </summary>
    public event FiasExecProcEventHandler ExecProcCreated;

    internal void OnExecProcCreated(FiasExecProcEventArgs args)
    {
      if (ExecProcCreated != null)
      {
        try
        {
          ExecProcCreated(this, args);
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "FiasDB.ExecProcCreated");
        }
      }
    }

    /// <summary>
    /// Вызывается перед началом выполнения процедуры ExecProc для асинхронного выполнения запроса.
    /// Обработчик события может вызываться асинхронно.
    /// </summary>
    public event FiasExecProcEventHandler ExecProcBeforeExecute;

    internal void OnExecProcBeforeExecute(FiasExecProcEventArgs args)
    {
      if (ExecProcBeforeExecute != null)
      {
        try
        {
          ExecProcBeforeExecute(this, args);
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "FiasDB.ExecProcBeforeExecute");
        }
      }
    }

    /// <summary>
    /// Вызывается при окончании процедуры ExecProc для асинхронного выполнения запроса.
    /// Вызывается, в том числе, и при возникновении исключения
    /// Обработчик события может вызываться асинхронно.
    /// </summary>
    public event FiasExecProcEventHandler ExecProcAfterExecute;

    internal void OnExecProcAfterExecute(FiasExecProcEventArgs args)
    {
      if (ExecProcAfterExecute != null)
      {
        try
        {
          ExecProcAfterExecute(this, args);
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "FiasDB.ExecProcAfterExecute");
        }
      }
    }

    /// <summary>
    /// Вызывается при удалении процедуры ExecProc для асинхронного выполнения запроса.
    /// Обработчик события может вызываться асинхронно.
    /// </summary>
    public event FiasExecProcEventHandler ExecProcDisposed;

    internal void OnExecProcDisposed(FiasExecProcEventArgs args)
    {
      if (ExecProcDisposed != null)
      {
        try
        {
          ExecProcDisposed(this, args);
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "FiasDB.ExecProcDisposed");
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Статистика по базе данных классификатора ФИАС.
  /// Возвращается методом FiasDB/FiasSource.GetDBStat()
  /// </summary>
  [Serializable]
  public sealed class FiasDBStat
  {
    #region Защищенный конструктор

    internal FiasDBStat()
    {
      _ActualDate = FiasTools.UnknownActualDate;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Дата актуальности классификатора
    /// </summary>
    public DateTime ActualDate
    {
      get { return _ActualDate; }
      internal set { _ActualDate = DateTime.SpecifyKind(value, DateTimeKind.Unspecified); }
    }
    private DateTime _ActualDate;

    /// <summary>
    /// Количество записей в таблице адресных объектов
    /// </summary>
    public long AddrObCount { get { return _AddrObCount; } internal set { _AddrObCount = value; } }
    private long _AddrObCount;

    /// <summary>
    /// Количество записей в таблице домов
    /// </summary>
    public long HouseCount { get { return _HouseCount; } internal set { _HouseCount = value; } }
    private long _HouseCount;

    /// <summary>
    /// Количество записей в таблице помещений
    /// </summary>
    public long RoomCount { get { return _RoomCount; } internal set { _RoomCount = value; } }
    private long _RoomCount;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("Дата актуальности: ");
      sb.Append(ActualDate);
      sb.Append(", адресных объектов: ");
      sb.Append(AddrObCount);
      sb.Append(", зданий: ");
      sb.Append(HouseCount);
      sb.Append(", помещений: ");
      sb.Append(RoomCount);

      return sb.ToString();
    }

    #endregion
  }
}
