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
  #region ��������

  /// <summary>
  /// ��������� ������� FiasDB.ExecProcXxx
  /// </summary>
  public sealed class FiasExecProcEventArgs : EventArgs
  {
    #region �����������

    internal FiasExecProcEventArgs(ExecProc execProc, object userData)
    {
      _ExecProc = execProc;
      _UserData = userData; // 25.12.2020
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ ��������� (��������� ������)
    /// </summary>
    public ExecProc ExecProc { get { return _ExecProc; } }
    private ExecProc _ExecProc;

    /// <summary>
    /// ������������ ���������������� ������, ���������� ��������� FiasDistributedSource.UserData
    /// </summary>
    public object UserData { get { return _UserData; } }
    private object _UserData;

    #endregion
  }

  /// <summary>
  /// ������� ������� FiasDB.ExecProcXxx
  /// </summary>
  /// <param name="sender">������ FiasDB</param>
  /// <param name="args">��������� �������</param>
  public delegate void FiasExecProcEventHandler(object sender, FiasExecProcEventArgs args);

  #endregion

  /// <summary>
  /// ��������� ������ � ����� ������ ��������������. 
  /// ���� ����� ��������� ���������������� ����� �� ������� ������� � ������������ ����������.
  /// ���������������� ��� �������� �� �������� �������
  /// �������� �� ������������� ��������� ���� ������.
  /// �������� �������������� �������� ������ ��� ������ � ��������������� �� ������� ������� (�������� Source).
  /// </summary>
  public sealed class FiasDB : SimpleDisposableObject
  {
    // 03.01.2021
    // ����� ������������ ������� ����� ��� �����������

    #region ����������� � Dispose

    /// <summary>
    /// ������� ������ ������� ��� ������ ��� ������� � �������������� � ���������� ����������.
    /// ��� ������ ������������ ������������, ���� ���������� ���������� ������� ���� ������ � �� ����� �� ���������.
    /// </summary>
    /// <param name="db">��������� �������� ���� ������ fias</param>
    public FiasDB(DBx db)
      : this(db, null, false)
    {
    }

    /// <summary>
    /// ������� ������ ������� ��� ������ ��� ������� � �������������� � ���������� ����������.
    /// ��������� �������� <paramref name="dbSettings"/>, ���� ���������� �������� �� ���������� ���� ������ ��������������.
    /// ���� ���������� ���������� � ���� ������ ������ �� ��������, � �� ���������� �������������� �������� ������
    /// ����������, ��������� null.
    /// ������� <paramref name="dbSettings"/> ������ ������� ��� ������������� ������ ���� ������ � ��������� ����������� ������ �������.
    /// ���� ������� ��� ����������, ����������� ������������� ���������� �������� � ����������� � ���� ������
    /// ������ �������������� ���� ����������� �������� � ������� FiasDBUpdater.
    /// </summary>
    /// <param name="db">��������� �������� ���� ������ fias</param>
    /// <param name="dbSettings">��������� ���� ������. ���������� FiasDBSettings.SetReadOnly()</param>
    public FiasDB(DBx db, FiasDBSettings dbSettings)
      : this(db, dbSettings, dbSettings != null)
    {
    }

    /// <summary>
    /// ������� ������ ������� ��� ������ ��� ������� � �������������� � ���������� ����������.
    /// ��������� �������� <paramref name="dbSettings"/>, ���� ���������� �������� �� ���������� ���� ������ ��������������.
    /// ���� ���������� ���������� � ���� ������ ������ �� ��������, � �� ���������� �������������� �������� ������
    /// ����������, ��������� null.
    /// ������� <paramref name="updateStruct"/> ������ ������� ��� ������������� ������ ���� ������ � ��������� ����������� ������ �������.
    /// ���� ������� ��� ����������, ����������� ������������� ���������� �������� � ����������� � ���� ������.
    /// ������ �������������� ���� ����������� �������� � ������� FiasDBUpdater.
    /// </summary>
    /// <param name="db">��������� �������� ���� ������ fias</param>
    /// <param name="dbSettings">��������� ���� ������. ���������� FiasDBSettings.SetReadOnly()</param>
    /// <param name="updateStruct">����� �� ���������� ��������� ��������� ���� ������</param>
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
          throw new ArgumentException("���� ������ �� ����������: " + db.DatabaseName);
      }

      InitIsEmpty(false); // ���� ����� �������� �� ��������� �������� DBx.Struct.

      if (dbSettings != null)
      {

        if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
        {
          // ������� ��� ������ ��������� �� �������� ������� AddrOb, ����� � AddrOb ����� ���� ������� ��������� ����
          using (DBxConBase con = db.MainEntry.CreateCon())
          {
            DBxSqlBuffer buffer = new DBxSqlBuffer(db.Formatter);
            buffer.SB.Append("CREATE VIRTUAL TABLE IF NOT EXISTS ");
            buffer.FormatTableName("AddrObFTS");
            buffer.SB.Append(" USING fts3(");
            buffer.FormatColumnName("OFFNAME");
            // �� ����� ��������������� �� ������� ����������� icu � ��, ��������� �� ��
            // ������������ ������� ������� ����.
            // ������ ����� ����� ������ ��������� ������� � �������� �������� � ������� ����� "�"
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
            // ��������� ������������
            TempCfg cfg = new TempCfg();
            cfg.AsXmlText = DataTools.GetString(oXML);
            FiasDBSettings dbSettinngs2 = new FiasDBSettings();
            dbSettinngs2.ReadConfig(cfg);
            cfg.Clear();
            dbSettinngs2.WriteConfig(cfg); // ��� ��� ��������������, �.�. ������ �������� ��� ������� ���������� � ����� � ����������� FIAS.dll

            TempCfg cfg1 = new TempCfg();
            DBSettings.WriteConfig(cfg1);
            if (cfg1.AsXmlText != cfg.AsXmlText)
              throw new FiasDBSettingsDifferentException("��������� ����, ���������� � ���� ������ �� ��������� � �����������");
          }
          else
          {
            // ������ �����
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
        db.Struct = CreateDBStruct(true); // ������ ������� ClassifInfo
        // �� ��������� ���������! db.UpdateStruct();

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
            throw new DBxConsistencyException("� ������� \"ClassifInfo\" ��� ������ � ������������� 1");
        }

        db.Struct = CreateDBStruct(false); // ��� �������
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
            //csb["cache size"] = 20000; // 1.3 �� ��� ������������ ������� �������� 64��
            // 09.09.2020. 
            // ���� ������� 1.3��, �� ����� ����� �������� MemoryTools.AvailableMemoryState ����� ������ ���������� Low,
            // �.�. MemoryFailPoint �� ������ �������� ���� ������.
            // �.�. ������, ����� ���������� ���������� ����������� � �� ������ ������������ � ���, �.�. ���� ����� ClearPool().
            // ��� - �������!
            csb["cache size"] = 10000; // 0.66 �� ��� ������������ ������� �������� 64��
          }

          if (_IsEmpty)
          {
            csb["journal mode"] = "Off";
            csb["synchronous"] = "Off"; // ���������� � �� �����.
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

      InitIsEmpty(true); // ��� ���, ����� �������� ���� ������������

      _UnbufferedSource = new FiasDBUnbufferedSource(this);
      _Source = new FiasDBCachedSource(_UnbufferedSource.CreateProxy(), this); // ����������� ����� ������������� InitIsEmpty()

      //// ��������� ���������� � ��������� ��������� �������, ����� ��������� ������������ ������������ ��, ���� ����������
      //using (DBxConBase con = DB.MainEntry.CreateCon())
      //{
      //  con.GetRecordCount("AOType");
      //}

      System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDB created. Cache key=" + DataTools.MD5SumFromString(DB.DBIdentity));
      System.Diagnostics.Trace.WriteLine("  Connection string=" + DB.MainEntry.UnpasswordedConnectionString);
    }

    /*
    /// <summary>
    /// ������ ��������������, ������������ ����������� ���� ������ SQLite "fias.db"
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
    /// ������� �������������� �������� ������ Source.
    /// </summary>
    /// <param name="disposing">true, ���� ������ ����� Dispose()</param>
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

    #region ��������� ���� ������

    /// <summary>
    /// �������� true, ���� �������� �������� ���� ��������, ��� ���� ������ ������
    /// </summary>
    private bool _IndexDelayed;

    private DBxStruct CreateDBStruct(bool classifInfoOnly)
    {
      DBxStruct dbx = new DBxStruct();
      DBxTableStruct ts;

      #region "ClassifInfo"

      ts = dbx.Tables.Add("ClassifInfo");
      ts.Columns.AddId(); // ��������� �������������, ������ ����� 1
      ts.Columns.AddXmlConfig("DBSettings"); // ������ FiasDBSettings
      ts.Columns.AddDate("ActualDate", false); // ���� ������������ ��������������
      ts.Columns.AddDateTime("UpdateTime", false); // ����� �������� ��� ���������� ���������� ��������������
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

        //bool NoPK = true; // �� ������������ ��������� ���� ���� GUID � ��������. ����� ������� ��������� ���� �� rowid.
        // � ��� ����� AOID � ������, ���������� ������.
        bool noPK = (_DB is FreeLibSet.Data.SQLite.SQLiteDBx); // 30.07.2020


        #region "ClassifUpdate"

        ts = dbx.Tables.Add("ClassifUpdate");
        ts.Columns.AddId();
        ts.Columns.AddDate("ActualDate", false); // ���� ������������ ��������������
        ts.Columns.AddInt("Source", DataTools.GetEnumRange(typeof(FiasDBUpdateSource)), false); // �������� ������
        ts.Columns.AddBoolean("Cumulative", false); // false - �������������� ��������, true - ������������� ����������
        ts.Columns.AddDateTime("StartTime", false); // ����� ������ ����������
        ts.Columns.AddDateTime("FinishTime", true); // ����� ��������� ����������. ���� null, �� ���������� �� ���������
        ts.Columns.AddInt("ErrorCount", true); // ���������� ������ ��� ��������� ����������
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

        // �� �����. ts.Indices.Add("LEVEL,SCNAME");

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
          ts.AutoCreate = false; // ��������� ��������� SQL-��������
          ts.Columns.AddId("rowid");
          ts.Columns.AddString("OFFNAME", 120, false);
        }

        #region "AddrOb"

        ts = dbx.Tables.Add("AddrOb");
        // ���� ��� ����������� ������ ��������
        if (_InternalSettings.UseIdTables)
        {
          ts.Columns.AddReference("Id", "AddrObRec", false); // ������������� ������ � ��������� ����. �������� ��� ������ �� �������, � �� ������������ �������������
          if (_DBSettings.UseHistory)
          {
            ts.Columns.AddReference("PrevId", "AddrObRec", true);  // ������ ��� ������������ ��������������
            ts.Columns.AddReference("NextId", "AddrObRec", true);  // ������ ��� ������������ ��������������
          }
          ts.Columns.AddReference("AOGuidRef", "AddrObGuid", false); // ������������� ������� ��������������
          ts.Columns.AddReference("ParentAOGuidRef", "AddrObGuid", true); // ������������ �������
        }
        else
        {
          ts.Columns.AddGuid("AOID", false); // ������������� ������ � ��������� ����. 
          if (DBSettings.UseHistory)
          {
            ts.Columns.AddGuid("PREVID", true);  // ������ ��� ������������ ��������������
            ts.Columns.AddGuid("NEXTID", true);  // ������ ��� ������������ ��������������
          }
          ts.Columns.AddGuid("AOGUID", false); // ������������� ������� ��������������
          ts.Columns.AddGuid("PARENTGUID", true); // ������������ �������

          // 28.02.2020
          // ��� - �������! ���� PARENTGUID ��������� �� AOGUID ������������� �������, � �� �� AOID, ������� �������� ��������� ������ �������
          //ts.Columns.LastAdded.MasterTableName = "AddrOb";
          //ts.Columns.LastAdded.RefType = DBxRefType.Emulation;
        }
        // ���� ��� ��������� �������
        if (_DBSettings.UseHistory)
        {
          ts.Columns.AddBoolean("Actual", false); // ������������� ���� ACTUALSTATUS � ����. ����� true, ���� ACTUALSTATUS=1.
          // ���� ������������ ������ �� �����, �� � ���� ������� Actual=true � Live=true � ���� �� �����
          ts.Columns.AddBoolean("Live", false); // ������������� ���� LIVESTATUS � ����
        }
        ts.Columns.AddInt("AOLEVEL", /*DataTools.GetEnumRange(typeof(FiasLevel))*/ 1, 99, false); // ������� ��������� �������
        ts.Columns.AddString("OFFNAME", 120, false); // ������������ �������. ������ ������ ������, ��� ��� ���� ����� ������� ��������
        if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
          ts.Columns.AddReference("NameId", "AddrObFTS", false);
        ts.Columns.AddReference("AOTypeId", "AOType", false, DBxRefType.Emulation); // ������-������ �� ������� "AOType". 
        //� ���� ������������ ��������� ���� SHORTNAME. �������� �� ������ ��� �������� �����.
        ts.Columns.AddInt("CENTSTATUS", DataTools.GetEnumRange(typeof(FiasCenterStatus)), false); // ������ ������
        ts.Columns.AddInt("DIVTYPE", DataTools.GetEnumRange(typeof(FiasDivType)), false); // ������� ���������:
        ts.Columns.AddInt("REGIONCODE", 0, 99, false);	// ��� �������. ��� �������� ����� ������ ��� �����. ���� �� ����� ����� �������� NULL
        ts.Columns.AddInt("POSTALCODE", 0, 999999, false);	// �������� ������
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
              ts.Indexes.Add("AOID"); // ������-��������� ����
            ts.Indexes.Add("AOGUID"); // ������������, ����� � ������ ����� GUID ��������� �������, �� ���� � ����������� �������
            ts.Indexes.Add("PARENTGUID"); // ������������ ��� �������� �������������� ��������.
          }
          if (_InternalSettings.FTSMode == FiasFTSMode.FTS3)
            ts.Indexes.Add("NameID"); // ������������ ��� ������������� ������, ����� �������� ��������� �������� �������
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
          // ���� ��� ����������� ������ ��������
          /*
          ts.Columns.AddReference("Id", "HouseRec", false); // ������������� ������ � ��������� ����. �������� ��� ������ �� �������, � �� ������������ �������������
          //ts.Columns.AddReference("PrevId", "HouseRec", true);  // ������ ��� ������������ ��������������
          //ts.Columns.AddReference("NextId", "HouseRec", true);  // ������ ��� ������������ ��������������
          ts.Columns.AddReference("HouseGuidRef", "HouseGuid", false); // ������������� ������� ��������������
          ts.Columns.AddReference("ParentAOGuidRef", "AddrObGuid", true); // ������������ �������
           * */
          ts.Columns.AddGuid("HOUSEID", false); // ������������� ������ � ��������� ����. 
          ts.Columns.AddGuid("HOUSEGUID", false); // ������������� ������� ��������������
          ts.Columns.AddGuid("AOGUID", true); // ������������ �������


          // ���� ��� ����
          ts.Columns.AddInt("nHouseNum", 0, 255, false); // ������� FiasTools.GetNumInt(HOUSENUM)
          ts.Columns.AddString("HOUSENUM", 20, true); // ����� ����. ������ ������ ������, ��� ��� ���� ����� ������� ����
          ts.Columns.AddInt("nBuildNum", 0, 255, false); // ������� FiasTools.GetNumInt(BUILDNUM)
          ts.Columns.AddString("BUILDNUM", 10, true); // ����� �������.
          ts.Columns.AddInt("nStrucNum", 0, 255, false); // ������� FiasTools.GetNumInt(STRUCNUM)
          ts.Columns.AddString("STRUCNUM", 10, true); // ����� ��������
          ts.Columns.AddInt("STRSTATUS", DataTools.GetEnumRange(typeof(FiasStructureStatus)), false); // ������ ��������
          ts.Columns.AddInt("ESTSTATUS", DataTools.GetEnumRange(typeof(FiasEstateStatus)), false); // ������� ��������
          //ts.Columns.AddInt("STARSTATUS", 0, 99, false); // ����������� ��� ��������

          ts.Columns.AddInt("DIVTYPE", DataTools.GetEnumRange(typeof(FiasDivType)), false); // ������� ���������:
          // �� ����� ts.Columns.AddInt("REGIONCODE", 0, 99, _UseNullableCodes);	// ��� �������.
          ts.Columns.AddInt("POSTALCODE", 0, 999999, false);	// �������� ������
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
                ts.Indexes.Add("HOUSEID"); // ������-��������� ����

              ts.Indexes.Add("HOUSEGUID"); // ������������, ���� � ������ ����� GUID ������
              ts.Indexes.Add("AOGUID"); // ������������ ��� �������� �������������� ��������
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
          // ���� ��� ����������� ������ ��������
          /*
          ts.Columns.AddReference("Id", "RoomRec", false); // ������������� ������ � ��������� ����. �������� ��� ������ �� �������, � �� ������������ �������������
          ts.Columns.AddReference("PrevId", "RoomRec", true);  // ������ ��� ������������ ��������������
          ts.Columns.AddReference("NextId", "RoomRec", true);  // ������ ��� ������������ ��������������
          ts.Columns.AddReference("RoomGuidRef", "RoomGuid", false); // ������������� ������� ��������������
          ts.Columns.AddReference("ParentHouseGuidRef", "HouseGuid", true); // ������������ �������
           * */
          ts.Columns.AddGuid("ROOMID", false); // ������������� ������ � ��������� ����. 
          if (DBSettings.UseHistory)
          {
            ts.Columns.AddGuid("PREVID", true);  // ������ ��� ������������ ��������������
            ts.Columns.AddGuid("NEXTID", true);  // ������ ��� ������������ ��������������
          }
          ts.Columns.AddGuid("ROOMGUID", false); // ������������� ������� ��������������
          ts.Columns.AddGuid("HOUSEGUID", true); // ������������ �������

          // ���� ��� �������
          if (DBSettings.UseHistory)
            ts.Columns.AddBoolean("Live", false); // ������������� ���� LIVESTATUS � ����
          ts.Columns.AddInt("nFlatNumber", 0, 255, false); // ������� FiasTools.GetNumInt(FLATNUMBER)
          ts.Columns.AddString("FLATNUMBER", 50, true); // ����� ��������, ����� ��� �������
          ts.Columns.AddInt("FLATTYPE", DataTools.GetEnumRange(typeof(FiasFlatType)), false); // ��� ���������
          ts.Columns.AddInt("nRoomNumber", 0, 255, false); // ������� FiasTools.GetNumInt(ROOMNUMBER)
          ts.Columns.AddString("ROOMNUMBER", 50, true); // ����� �������
          ts.Columns.AddInt("ROOMTYPE", DataTools.GetEnumRange(typeof(FiasRoomType)), false); // ��� �������
          ts.Columns.AddInt("POSTALCODE", 0, 999999, false);	// �������� ������
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
                ts.Indexes.Add("ROOMID"); // ��������������� ����

              ts.Indexes.Add("ROOMGUID"); // ������������, ���� � ������ ����� GUID ���������
              ts.Indexes.Add("HOUSEGUID"); // ������������ ��� �������� �������������� ��������
            }
          }

          #endregion
        }
      } // !classifInfoOnly

      return dbx;
    }


    #endregion

    #region ��������

    /// <summary>
    /// ���� ������
    /// </summary>
    public DBx DB { get { return _DB; } }
    private readonly DBx _DB;

    /// <summary>
    /// �������������� ������ � ���� ������ ��� ����������
    /// </summary>
    private DBx _UpdateDB;

    /// <summary>
    /// ����� ����������� � ���� ������ ��� ���������� ���������� (� ����� �������� �������)
    /// </summary>
    internal DBxEntry UpdateEntry { get { return _UpdateEntry; } }
    private readonly DBxEntry _UpdateEntry;

    /// <summary>
    /// ��������� ���� ������
    /// </summary>
    public FiasDBSettings DBSettings { get { return _DBSettings; } }
    private readonly FiasDBSettings _DBSettings;

    /// <summary>
    /// ���������� ��������� ��������������, ������� ������������ ����������� �� �����
    /// </summary>
    internal FiasInternalSettings InternalSettings { get { return _InternalSettings; } }
    private FiasInternalSettings _InternalSettings;

    internal bool UpdateStruct { get { return _UpdateStruct; } }
    private readonly bool _UpdateStruct;

    private FiasDBUnbufferedSource _UnbufferedSource;

    /// <summary>
    /// �������������� �������� ������.
    /// ���������� �� ������ ������ Dispose().
    /// ������������ ��� ������������� ������� FiasHandler �� ������� �������.
    /// ���������� ������� ����� net remoting ��� �������� ������������ ������� FiasCachedSource.
    /// </summary>
    public FiasCachedSource Source { get { return _Source; } }
    private FiasDBCachedSource _Source;


    /// <summary>
    /// ���������� true, ���� � ������ ������ ������������ �� ��������
    /// </summary>
    public bool IsEmpty { get { return _IsEmpty; } }
    private bool _IsEmpty;

    private void InitIsEmpty(bool updateStat)
    {
      using (DBxConBase con = _DB.MainEntry.CreateCon())
      {
        // �.�. ����� ����� ���������� �� ������������� ��������� ���� ������, ������� ����� �� ������������
        con.Validator.NameCheckingEnabled = false;

        // ��� SQLite ����� ���� �� �������  SELECT COUNT(*) FROM sqlite_master WHERE type=="table" AND name=="AddrOb"

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
    /// ���������� �� ���� ������
    /// </summary>
    public FiasDBStat DBStat { get { return _DBStat; } }
    private volatile FiasDBStat _DBStat;

    /// <summary>
    /// ���� ������������ ��������������
    /// </summary>
    public DateTime ActualDate { get { return _DBStat.ActualDate; } }

    //private static object _SyncRoot = new object();

    #endregion

    #region ���������� �� ���� ������

    /// <summary>
    /// �������� ���������� �� ���� ������.
    /// ���� ����� ����������� ��������, �.�. ������� ���������� �������� SELECT COUNT(*), ������� �� �������������� � SQLite
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

    #region ������� ����������

    /// <summary>
    /// ������ �������� ��������, �������� �� null, ����� ����������� ���������� ��������������
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
        // ��������� ������� ����������� �������
        _DB.Struct = CreateDBStruct(false); // ������ � ���������
        _DB.UpdateStruct();
      }

      _Source.UpdateActualDate();
    }

    /// <summary>
    /// ���������� � UnbufferedSource
    /// </summary>
    internal void CheckIsReady()
    {
      CheckNotDisposed();
      FiasDBUpdater u = _CurrentUpdater;
      if (u != null)
      {
        if (u.PrimaryLoad)
          throw new InvalidOperationException("� ������ ������ ����������� ��������� �������� ��������������. ��������� ������ ��������� � ������ ������ ������");
      }
    }

    #endregion

    #region ������� ����������� ���������� ��������

    /// <summary>
    /// ���������� ��� �������� ��������� ExecProc ��� ������������ ���������� �������.
    /// ���������� ������� ����� ���������� ����������.
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
    /// ���������� ����� ������� ���������� ��������� ExecProc ��� ������������ ���������� �������.
    /// ���������� ������� ����� ���������� ����������.
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
    /// ���������� ��� ��������� ��������� ExecProc ��� ������������ ���������� �������.
    /// ����������, � ��� �����, � ��� ������������� ����������
    /// ���������� ������� ����� ���������� ����������.
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
    /// ���������� ��� �������� ��������� ExecProc ��� ������������ ���������� �������.
    /// ���������� ������� ����� ���������� ����������.
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
  /// ���������� �� ���� ������ �������������� ����.
  /// ������������ ������� FiasDB/FiasSource.GetDBStat()
  /// </summary>
  [Serializable]
  public sealed class FiasDBStat
  {
    #region ���������� �����������

    internal FiasDBStat()
    {
      _ActualDate = FiasTools.UnknownActualDate;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� ������������ ��������������
    /// </summary>
    public DateTime ActualDate
    {
      get { return _ActualDate; }
      internal set { _ActualDate = DateTime.SpecifyKind(value, DateTimeKind.Unspecified); }
    }
    private DateTime _ActualDate;

    /// <summary>
    /// ���������� ������� � ������� �������� ��������
    /// </summary>
    public long AddrObCount { get { return _AddrObCount; } internal set { _AddrObCount = value; } }
    private long _AddrObCount;

    /// <summary>
    /// ���������� ������� � ������� �����
    /// </summary>
    public long HouseCount { get { return _HouseCount; } internal set { _HouseCount = value; } }
    private long _HouseCount;

    /// <summary>
    /// ���������� ������� � ������� ���������
    /// </summary>
    public long RoomCount { get { return _RoomCount; } internal set { _RoomCount = value; } }
    private long _RoomCount;

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("���� ������������: ");
      sb.Append(ActualDate);
      sb.Append(", �������� ��������: ");
      sb.Append(AddrObCount);
      sb.Append(", ������: ");
      sb.Append(HouseCount);
      sb.Append(", ���������: ");
      sb.Append(RoomCount);

      return sb.ToString();
    }

    #endregion
  }
}
