// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using System.IO;
using FreeLibSet.DBF;
using System.Data;
using FreeLibSet.Data;
using System.Xml;
using System.Data.Common;
using System.Diagnostics;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// �������� ���������� �������������� ����
  /// </summary>
  public enum FiasDBUpdateSource
  {
    /// <summary>
    /// �������� ���� � ������� XML
    /// </summary>
    Xml = 1,

    /// <summary>
    /// �������� ���� � ������� DBF
    /// </summary>
    Dbf = 2
  }

  /// <summary>
  /// �������� ������ ��� ���������� ���������� ���� ������, ��������� ��������� �����.
  /// ������������ ����� ������������ �� ����� ������ ���������� ������.
  /// </summary>
  public sealed class FiasDBUpdater : SimpleDisposableObject
  {
    // 03.01.2021
    // ����� ������������ ������� ����� ��� �����������

    #region ����������� � Dispose

    /// <summary>
    /// ������� ������ ��� ���������� ����.
    /// ������������� ������������ ������� ��������� ��������/����������
    /// </summary>
    /// <param name="fiasDB">���� ������</param>
    public FiasDBUpdater(FiasDB fiasDB)
      : this(fiasDB, fiasDB.IsEmpty)
    {
    }

    /// <summary>
    /// ������� ������ ��� ���������� ����.
    /// ������������� ������������ ������� ��������� ��������/����������
    /// </summary>
    /// <param name="fiasDB">���� ������</param>
    /// <param name="primaryLoad">true - ��������� ���������� ����, false - ����������</param>
    public FiasDBUpdater(FiasDB fiasDB, bool primaryLoad)
    {
      if (fiasDB == null)
        throw new ArgumentNullException("fiasDB");

      if (!fiasDB.UpdateStruct)
        throw new InvalidOperationException("���� ������ ���� �� ������������� ��� ����������");

      lock (fiasDB)
      {
        if (fiasDB.CurrentUpdater != null)
          throw new InvalidOperationException("���������� ���������� ��� �� ���������");
        fiasDB.CurrentUpdater = this;
      }
      _FiasDB = fiasDB;
      _Splash = new DummySplash();
      _Today = DateTime.Today;
      _PrimaryLoad = primaryLoad;
    }

    /// <summary>
    /// ���������� �� Dispose().
    /// ����������� ���� ������ �� FiasDB
    /// </summary>
    /// <param name="Disposing">true, ���� ������ ����� Dispose(), � �� ����������</param>
    protected override void Dispose(bool Disposing)
    {
      if (_FiasDB != null && Disposing)
      {
        _FiasDB.AfterUpdate();

        _FiasDB.CurrentUpdater = null;
        _FiasDB = null;
      }
      base.Dispose(Disposing);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� ������ ��� ������� ����������� ����������
    /// </summary>
    public FiasDB FiasDB { get { return _FiasDB; } }
    private FiasDB _FiasDB;

    /// <summary>
    /// ��������.
    /// ���� �������� �����������, �� ������������ �� ����� �������� ������.
    /// ����� �� ����������.
    /// ���� �������� �� �����������, ������������ ��������� ��������, � �� null.
    /// </summary>
    public ISplash Splash
    {
      get { return _Splash; }
      set
      {
        if (value == null)
          _Splash = new DummySplash();
        else
          _Splash = value;
      }
    }
    private ISplash _Splash;

    /// <summary>
    /// ��������������� � ������������ � true, ���� ����������� ��������� �������� ��������������, � �� ����������.
    /// ��� ���� ������ � ������� ����� ����������� � ������ INSERT, � �� INSERT_OR_UPDATE
    /// </summary>
    public bool PrimaryLoad { get { return _PrimaryLoad; } }
    private bool _PrimaryLoad;

    #endregion

    #region �������� DBF-������

    #region �������� �����

    /// <summary>
    /// �������� �������� � DBF-�������
    /// </summary>
    /// <param name="dir">�������</param>
    /// <param name="actualDate">���� ������������ ����������</param>
    /// <returns>���������� DBF-������, ������� ���� ����������.
    /// �������� ��� �������� ����� �� �����������</returns>
    public int LoadDbfDir(AbsPath dir, DateTime actualDate)
    {
      if (!Directory.Exists(dir.Path))
        throw new FileNotFoundException("�� ������ ������� \"" + dir.Path + "\"");

      int cnt = 0;
      string oldPT = Splash.PhaseText;

      using (DBxConBase con = CreateCon())
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBUpdate starting " + (_PrimaryLoad ? " load " : " update ") + " from DBF...");
        Trace.WriteLine("  DBF dir=" + dir.Path.ToString());
        Trace.WriteLine("  Connection string=" + FiasDB.UpdateEntry.UnpasswordedConnectionString);

        BeginUpdate(con, actualDate, FiasDBUpdateSource.Dbf);

        if (File.Exists(new AbsPath(dir, "SOCRBASE.DBF").Path))
        {
          Splash.PhaseText = "SOCRBASE.DBF";
          DoLoadDbfFile(con, new AbsPath(dir, "SOCRBASE.DBF"));
          Splash.PhaseText = oldPT;
          cnt++;
        }

        string[] files = Directory.GetFiles(dir.Path, "*.dbf", SearchOption.TopDirectoryOnly);
        Array.Sort<string>(files);
        //Splash.PercentMax = Files.Length;
        //Splash.AllowCancel = true;
        for (int i = 0; i < files.Length; i++)
        {
          AbsPath filePath = new AbsPath(files[i]);
          if (filePath.FileNameWithoutExtension.ToUpperInvariant() == "SOCRBASE")
            continue;
          Splash.PhaseText = filePath.FileName;
          if (DoLoadDbfFile(con, filePath))
            cnt++;
          Splash.PhaseText = oldPT;
          Splash.IncPercent();
        }

        // TODO: ������� �������� DADDROBJ, ...

        Splash.PhaseText = "������ � ������� ������� ����������";
        EndUpdate(con);
        Splash.PhaseText = oldPT;
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBUpdate finished. DBF files proceed: " + cnt.ToString());
      }

      GC.Collect(); // 08.09.2020

      return cnt;
    }
    private bool DoLoadDbfFile(DBxConBase con, AbsPath filePath)
    {
      string name = filePath.FileNameWithoutExtension.ToUpperInvariant();
      if (name == "SOCRBASE")
      {
        DoLoadDbfSocrBase(con, filePath);
        return true;
      }
      if (name.StartsWith("ADDROB") && CheckDbfFileNameRegion(name.Substring(6)))
      {
        DoLoadDbfAddrOb(con, filePath);
        return true;
      }
      if (name.StartsWith("HOUSE") && CheckDbfFileNameRegion(name.Substring(5)) && _FiasDB.DBSettings.UseHouse)
      {
        DoLoadDbfFileHouse(con, filePath);
        return true;
      }
      if (name.StartsWith("ROOM") && CheckDbfFileNameRegion(name.Substring(4)) && _FiasDB.DBSettings.UseRoom)
      {
        DoLoadDbfFileRoom(con, filePath);
        return true;
      }

      // 25.02.2021
      if (name.StartsWith("DAD") || name.StartsWith("DHOUSE") || name.StartsWith("DROOM"))
      {
        throw new NotImplementedException("�� ����������� ��������� �������� ������� ����. ���� " + filePath.Path);
      }

      return false;
    }

    private bool CheckDbfFileNameRegion(string nameSuffix)
    {
      // ���������, ��� ��� ������� - ����� � �� �� ������ ����
      if (nameSuffix.Length < 2)
        return false;
      for (int i = 0; i < nameSuffix.Length; i++)
      {
        if (nameSuffix[i] < '0' || nameSuffix[i] > '9')
          return false;
      }

      if (FiasDB.DBSettings.RegionCodes.Count == 0)
        return true;

      string regCode = nameSuffix.Substring(0, 2); // ���� �������������� DBF-�����, ��������, "ROOM7701.DBF", ����� �������� ���� ������� ������� ��� DBF
      return FiasDB.DBSettings.RegionCodes.Contains(regCode);
    }

    #endregion

    #region SOCRBASE.DBF

    private void DoLoadDbfSocrBase(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      LoadAOTypesTable(con);
      using (DbfFile dbf = new DbfFile(filePath))
      {
        while (dbf.Read())
        {
          int level = dbf.GetInt("LEVEL");
          string scName = dbf.GetString("SCNAME");
          string socrName = dbf.GetString("SOCRNAME");
          FindOrAddAOType(level, scName, socrName);
          cnt++;
        }
      }
      FlushAOTypesTable(con);

      EndLoadFile(filePath, cnt);
    }

    #endregion

    #region ADDROBxx.DBF

    private void DoLoadDbfAddrOb(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      LoadAOTypesTable(con);

      DbCommand cmdFTS3 = CreateFTS3Command(con);
      try
      {
        using (DbfFile dbf = new DbfFile(filePath))
        {
          DBxDataWriterInfo writerInfo = CreateWriterInfo("AddrOb");
          writerInfo.ExpectedRowCount = dbf.RecordCount;
          using (DBxDataWriter wrt = con.CreateWriter(writerInfo))
          {
            Splash.PercentMax = dbf.RecordCount;
            Splash.AllowCancel = true;
            while (dbf.Read())
            {
              if ((dbf.RecNo % 1000) == 0)
              {
                Splash.Percent = dbf.RecNo - 1;
                Splash.PhaseText = filePath.FileName + " (" + dbf.RecNo.ToString() + " �� " + dbf.RecordCount.ToString() + ")";
              }

              if (!_FiasDB.DBSettings.UseHistory)
              {
                if (dbf.GetInt("ACTSTATUS") != 1)
                  continue;
                if (dbf.GetInt("LIVESTATUS") != 1)
                  continue;
                if (!TestDateRange(dbf.GetDate("STARTDATE"), dbf.GetDate("ENDDATE")))
                  continue;
              }

              #region ���������� ������������� � AddressObjects

              wrt.SetGuid("AOID", DataTools.GetGuid(dbf.GetString("AOID")));
              if (_FiasDB.DBSettings.UseHistory)
              {
                wrt.SetGuid("PREVID", DataTools.GetGuid(dbf.GetString("PREVID")));
                wrt.SetGuid("NEXTID", DataTools.GetGuid(dbf.GetString("NEXTID")));
              }
              wrt.SetGuid("AOGUID", DataTools.GetGuid(dbf.GetString("AOGUID")));
              wrt.SetGuid("PARENTGUID", DataTools.GetGuid(dbf.GetString("PARENTGUID")));

              #endregion

              #region ������������ ��������� �������

              string offName = dbf.GetString("OFFNAME");
              wrt["OFFNAME"] = offName;

              if (_FiasDB.InternalSettings.FTSMode == FiasFTSMode.FTS3)
              {
                cmdFTS3.Parameters[0].Value = FiasTools.PrepareForFTS(offName);
                Int32 NameId = DataTools.GetInt(cmdFTS3.ExecuteScalar());
                wrt["NameId"] = NameId;
              }

              #endregion

              #region ������� ������������

              int level = dbf.GetInt("AOLEVEL");
              string scName = dbf.GetString("SHORTNAME");
              wrt["AOTypeId"] = FindOrAddAOType(level, scName, String.Empty);

              #endregion

              #region ����, ������������ ��-�������

              if (_FiasDB.DBSettings.UseHistory)
              {
                switch (dbf.GetInt("ACTSTATUS"))
                {
                  case 1: wrt["Actual"] = true; break;
                  case 0: wrt["Actual"] = false; break;
                  default:
                    throw new BugException("����������� �������� ACTSTATUS=" + dbf.GetInt("ACTSTATUS"));
                }

                switch (dbf.GetInt("LIVESTATUS"))
                {
                  case 1: wrt["Live"] = true; break;
                  case 0: wrt["Live"] = false; break;
                  default:
                    throw new BugException("����������� �������� LIVESTATUS=" + dbf.GetInt("LIVESTATUS"));
                }
              }

              #endregion

              #region ��������� ����

              wrt["AOLEVEL"] = dbf.GetInt("AOLEVEL");
              wrt["CENTSTATUS"] = dbf.GetInt("CENTSTATUS");
              wrt["DIVTYPE"] = dbf.GetInt("DIVTYPE");

              wrt["POSTALCODE"] = CheckedGetInt(filePath, dbf, "POSTALCODE");
              wrt["REGIONCODE"] = dbf.GetInt("REGIONCODE");
              if (_FiasDB.DBSettings.UseIFNS)
              {
                wrt["IFNSFL"] = dbf.GetInt("IFNSFL");
                wrt["TERRIFNSFL"] = dbf.GetInt("TERRIFNSFL");
                wrt["IFNSUL"] = dbf.GetInt("IFNSUL");
                wrt["TERRIFNSUL"] = dbf.GetInt("TERRIFNSUL");
              }
              if (_FiasDB.DBSettings.UseOKATO)
                wrt["OKATO"] = dbf.GetInt64("OKATO");
              if (_FiasDB.DBSettings.UseOKTMO)
                wrt["OKTMO"] = dbf.GetInt64("OKTMO");

              if (_FiasDB.DBSettings.UseDates)
              {
                if (_FiasDB.InternalSettings.UseOADates)
                {
                  wrt["dStartDate"] = (int)(dbf.GetDate("STARTDATE").ToOADate());
                  wrt["dEndDate"] = (int)(dbf.GetDate("ENDDATE").ToOADate());
                }
                else
                {
                  wrt["STARTDATE"] = dbf.GetDate("STARTDATE");
                  wrt["ENDDATE"] = dbf.GetDate("ENDDATE");
                }
              }

              #endregion

              wrt.Write();
              cnt++;
            } // ������� ����� � DBF-�����

            wrt.Finish();
          }
        }
      }
      finally
      {
        if (cmdFTS3 != null)
          cmdFTS3.Dispose();
      }

      FlushAOTypesTable(con);

      EndLoadFile(filePath, cnt);
    }

    private DbCommand CreateFTS3Command(DBxConBase con)
    {
      if (_FiasDB.InternalSettings.FTSMode == FiasFTSMode.FTS3)
      {
        DBxSqlBuffer buffer = new DBxSqlBuffer(con.DB.Formatter);
        buffer.SB.Append("INSERT INTO ");
        buffer.FormatTableName("AddrObFTS");
        buffer.SB.Append("(");
        buffer.FormatColumnName("OFFNAME");
        buffer.SB.Append(") VALUES(");
        buffer.FormatParamPlaceholder(0);
        buffer.SB.Append(");SELECT last_insert_rowid()");
        DbCommand cmdFTS3 = con.CreateDbCommand();
        cmdFTS3.CommandText = buffer.SB.ToString();

        DbParameter p = con.DB.ProviderFactory.CreateParameter();
        p.ParameterName = "P1".ToString();
        cmdFTS3.Parameters.Add(p);
        cmdFTS3.Prepare();
        return cmdFTS3;
      }
      else
        return null;
    }

    #endregion

    #region HOUSExx.DBF

    private void DoLoadDbfFileHouse(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      using (DbfFile dbf = new DbfFile(filePath))
      {
        DBxDataWriterInfo writerInfo = CreateWriterInfo("House");
        writerInfo.ExpectedRowCount = dbf.RecordCount;
        using (DBxDataWriter wrt = con.CreateWriter(writerInfo))
        {
          Splash.PercentMax = dbf.RecordCount;
          Splash.AllowCancel = true;
          while (dbf.Read())
          {
            if ((dbf.RecNo % 1000) == 0)
            {
              Splash.Percent = dbf.RecNo - 1;
              Splash.PhaseText = filePath.FileName + " (" + dbf.RecNo.ToString() + " �� " + dbf.RecordCount.ToString() + ")";
            }
            if (!_FiasDB.DBSettings.UseHistory)
            {
              if (!TestDateRange(dbf.GetDate("STARTDATE"), dbf.GetDate("ENDDATE")))
                continue;
            }

            #region ���������� ������������� � House

            wrt.SetGuid("HOUSEID", DataTools.GetGuid(dbf.GetString("HOUSEID")));
            wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(dbf.GetString("HOUSEGUID")));
            wrt.SetGuid("AOGUID", DataTools.GetGuid(dbf.GetString("AOGUID")));

            #endregion

            #region ��������� ����

            string sHouseNum = dbf.GetString("HOUSENUM");
            int nHouseNum = FiasTools.GetNumInt(ref sHouseNum);
            wrt["nHouseNum"] = nHouseNum; // 0 ������������
            wrt.SetString("HOUSENUM", sHouseNum);

            string sBuildNum = dbf.GetString("BUILDNUM");
            int nBuildNum = FiasTools.GetNumInt(ref sBuildNum);
            wrt["nBuildNum"] = nBuildNum; // 0 ������������
            wrt.SetString("BUILDNUM", sBuildNum);

            string sStrucNum = dbf.GetString("STRUCNUM");
            int nStrucNum = FiasTools.GetNumInt(ref sStrucNum);
            wrt["nStrucNum"] = nStrucNum; // 0 ������������
            wrt.SetString("STRUCNUM", sStrucNum);

            wrt["STRSTATUS"] = dbf.GetInt("STRSTATUS");
            wrt["ESTSTATUS"] = dbf.GetInt("ESTSTATUS");
            wrt["DIVTYPE"] = dbf.GetInt("DIVTYPE");

            wrt["POSTALCODE"] = CheckedGetInt(filePath, dbf, "POSTALCODE");

            if (_FiasDB.DBSettings.UseIFNS)
            {
              wrt["IFNSFL"] = dbf.GetInt("IFNSFL");
              wrt["TERRIFNSFL"] = dbf.GetInt("TERRIFNSFL");
              wrt["IFNSUL"] = dbf.GetInt("IFNSUL");
              wrt["TERRIFNSUL"] = dbf.GetInt("TERRIFNSUL");
            }
            if (_FiasDB.DBSettings.UseOKATO)
              wrt["OKATO"] = dbf.GetInt64("OKATO");
            if (_FiasDB.DBSettings.UseOKTMO)
              wrt["OKTMO"] = dbf.GetInt64("OKTMO");

            if (_FiasDB.DBSettings.UseDates)
            {
              if (_FiasDB.InternalSettings.UseOADates)
              {
                wrt["dStartDate"] = (int)(dbf.GetDate("STARTDATE").ToOADate());
                wrt["dEndDate"] = (int)(dbf.GetDate("ENDDATE").ToOADate());
              }
              else
              {
                wrt["STARTDATE"] = dbf.GetDate("STARTDATE");
                wrt["ENDDATE"] = dbf.GetDate("ENDDATE");
              }
            }

            #endregion

            wrt.Write();
            cnt++;
          } // ���� �� dbf.Read()

          wrt.Finish();
        }
      }

      EndLoadFile(filePath, cnt);
    }

    #endregion

    #region ROOMxx.DBF

    private void DoLoadDbfFileRoom(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      using (DbfFile dbf = new DbfFile(filePath))
      {
        DBxDataWriterInfo writerInfo = CreateWriterInfo("Room");
        writerInfo.ExpectedRowCount = dbf.RecordCount;
        using (DBxDataWriter wrt = con.CreateWriter(writerInfo))
        {
          Splash.PercentMax = dbf.RecordCount;
          Splash.AllowCancel = true;
          while (dbf.Read())
          {
            if ((dbf.RecNo % 1000) == 0)
            {
              Splash.Percent = dbf.RecNo - 1;
              Splash.PhaseText = filePath.FileName + " (" + dbf.RecNo.ToString() + " �� " + dbf.RecordCount.ToString() + ")";
            }

            if (!_FiasDB.DBSettings.UseHistory)
            {
              if (!TestDateRange(dbf.GetDate("STARTDATE"), dbf.GetDate("ENDDATE")))
                continue;
              if (dbf.GetInt("LIVESTATUS") != 1)
                continue;
            }

            #region ���������� ������������� � Room

            wrt.SetGuid("ROOMID", DataTools.GetGuid(dbf.GetString("ROOMID")));
            if (_FiasDB.DBSettings.UseHistory)
            {
              wrt.SetGuid("PREVID", DataTools.GetGuid(dbf.GetString("PREVID")));
              wrt.SetGuid("NEXTID", DataTools.GetGuid(dbf.GetString("NEXTID")));
            }
            wrt.SetGuid("ROOMGUID", DataTools.GetGuid(dbf.GetString("ROOMGUID")));
            wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(dbf.GetString("HOUSEGUID")));

            #endregion

            #region ����, ������������ ��-�������

            if (_FiasDB.DBSettings.UseHistory)
            {
              switch (dbf.GetInt("LIVESTATUS"))
              {
                case 1: wrt["Live"] = true; break;
                case 0: wrt["Live"] = false; break;
                default:
                  throw new BugException("����������� �������� LIVESTATUS=" + dbf.GetInt("LIVESTATUS"));
              }
            }

            #endregion

            #region ��������� ����

            string sFlatNumber = dbf.GetString("FLATNUMBER");
            int nFlatNumber = FiasTools.GetNumInt(ref sFlatNumber);
            wrt["nFlatNumber"] = nFlatNumber;
            wrt.SetString("FLATNUMBER", sFlatNumber);

            string sRoomNumber = dbf.GetString("ROOMNUMBER");
            int nRoomNumber = FiasTools.GetNumInt(ref sRoomNumber);
            wrt["nRoomNumber"] = nRoomNumber;
            wrt.SetString("ROOMNUMBER", sRoomNumber);

            wrt["FLATTYPE"] = dbf.GetInt("FLATTYPE");
            wrt["ROOMTYPE"] = dbf.GetInt("ROOMTYPE");

            wrt["POSTALCODE"] = CheckedGetInt(filePath, dbf, "POSTALCODE");

            if (_FiasDB.DBSettings.UseDates)
            {
              if (_FiasDB.InternalSettings.UseOADates)
              {
                wrt["dStartDate"] = (int)(dbf.GetDate("STARTDATE").ToOADate());
                wrt["dEndDate"] = (int)(dbf.GetDate("ENDDATE").ToOADate());
              }
              else
              {
                wrt["STARTDATE"] = dbf.GetDate("STARTDATE");
                wrt["ENDDATE"] = dbf.GetDate("ENDDATE");
              }
            }

            #endregion

            wrt.Write();
            cnt++;
          } // ���� �� ������� DBF-����� 

          wrt.Finish();
        }
      }

      EndLoadFile(filePath, cnt);
    }

    /// <summary>
    /// ��������� ��������� �������� � ���������� ������.
    /// ���������� ��������� � Trace.
    /// ������������ 0.
    /// � ������� room19.dbf �������������� �� 02.03.2019 ���� ���������� �������� ������ � ������ 81502
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="dbf"></param>
    /// <param name="colName"></param>
    /// <returns></returns>
    private static int CheckedGetInt(AbsPath filePath, DbfFile dbf, string colName)
    {
      try
      {
        return dbf.GetInt(colName);
      }
      catch
      {
        Trace.WriteLine("Error reading integer value from dbf. File: " + filePath.Path + ", RecNo: " + dbf.RecNo.ToString() + ", Field=" + colName);
        return 0;
      }
    }

    #endregion

    #endregion

    #region �������� XML-������

    #region �������� �����

    /// <summary>
    /// �������� �������� � DBF-�������
    /// </summary>
    /// <param name="dir">�������</param>
    /// <param name="actualDate">���� ������������ ����������</param>
    /// <returns>���������� DBF-������, ������� ���� ����������.
    /// �������� ��� �������� ����� �� �����������</returns>
    public int LoadXmlDir(AbsPath dir, DateTime actualDate)
    {
      if (!Directory.Exists(dir.Path))
        throw new FileNotFoundException("�� ������ ������� \"" + dir.Path + "\"");

      int cnt = 0;

      AbsPath filePath;
      string oldPT = Splash.PhaseText;

      using (DBxConBase con = CreateCon())
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBUpdate starting " + (_PrimaryLoad ? " load " : " update ") + " from XML...");
        Trace.WriteLine("  XML dir=" + dir.Path.ToString());
        Trace.WriteLine("  Connection string=" + FiasDB.UpdateEntry.UnpasswordedConnectionString);

        BeginUpdate(con, actualDate, FiasDBUpdateSource.Xml);

        #region ���������� ������

        filePath = GetXmlFilePath(dir, "AS_SOCRBASE");
        if (!filePath.IsEmpty)
        {
          Splash.PhaseText = filePath.FileName;
          DoLoadXmlSocrBase(con, filePath);
          Splash.PhaseText = oldPT;
          cnt++;
        }

        // � ������ ���� �� 26.12.2019 ���� ���� AS_ADDROBJ_20191226_9985b7cf-05ef-4113-8b40-1dcf76ce6363.XML
        // � ������ ���� �� 16.03.2020 ���� ���� AS_ADDRESS_OBJECTS_20200316_7d7eb721-53c7-4f43-9886-c02db9f3a18f.XML
        // ��� ������, ��� � �������.
        // ��� ��� � ���� �� 16.03.2020 ��� ������ AS_DEL*, ����������, ��� ���������� ���� ��������� �������

        filePath = GetXmlFilePath(dir, "AS_ADDROBJ");
        if (!filePath.IsEmpty)
        {
          Splash.PhaseText = filePath.FileName;
          DoLoadXmlAddrObj(con, filePath);
          Splash.PhaseText = oldPT;
          cnt++;
        }
        else
        {
          filePath = GetXmlFilePath(dir, "AS_ADDRESS_OBJECTS");
          if (!filePath.IsEmpty)
          {
            Splash.PhaseText = filePath.FileName;
            DoLoadXmlAddrObj(con, filePath);
            Splash.PhaseText = oldPT;
            cnt++;
          }
        }

        if (_FiasDB.DBSettings.UseHouse)
        {
          filePath = GetXmlFilePath(dir, "AS_HOUSE");
          if (!filePath.IsEmpty)
          {
            Splash.PhaseText = filePath.FileName;
            DoLoadXmlHouse(con, filePath);
            Splash.PhaseText = oldPT;
            cnt++;
          }
        }

        if (_FiasDB.DBSettings.UseRoom)
        {
          filePath = GetXmlFilePath(dir, "AS_ROOM");
          if (!filePath.IsEmpty)
          {
            Splash.PhaseText = filePath.FileName;
            DoLoadXmlRoom(con, filePath);
            Splash.PhaseText = oldPT;
            cnt++;
          }
        }

        #endregion

        #region �������� ������

        filePath = GetXmlFilePath(dir, "AS_DEL_ADDRESS_OBJECTS"); // �� ������ ������
        if (!filePath.IsEmpty)
        {
          Splash.PhaseText = filePath.FileName;
          DoDelXml(con, filePath, "Object", "AddrOb");
          Splash.PhaseText = oldPT;
          cnt++;
        }
        else
        {
          filePath = GetXmlFilePath(dir, "AS_DEL_ADDROBJ");
          if (!filePath.IsEmpty)
          {
            Splash.PhaseText = filePath.FileName;
            DoDelXml(con, filePath, "Object", "AddrOb");
            Splash.PhaseText = oldPT;
            cnt++;
          }
        }

        if (_FiasDB.DBSettings.UseHouse)
        {
          filePath = GetXmlFilePath(dir, "AS_DEL_HOUSE");
          if (!filePath.IsEmpty)
          {
            Splash.PhaseText = filePath.FileName;
            DoDelXml(con, filePath, "House", "House");
            Splash.PhaseText = oldPT;
            cnt++;
          }
        }

        if (_FiasDB.DBSettings.UseRoom)
        {
          filePath = GetXmlFilePath(dir, "AS_DEL_ROOM");
          if (!filePath.IsEmpty)
          {
            Splash.PhaseText = filePath.FileName;
            DoDelXml(con, filePath, "Room", "Room");
            Splash.PhaseText = oldPT;
            cnt++;
          }
        }

        #endregion

        Splash.PhaseText = "������ � ������� ������� ����������";
        EndUpdate(con);
        Splash.PhaseText = oldPT;

        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBUpdate finished. XML files proceed: " + cnt.ToString());
      }

      GC.Collect(); // 08.09.2020

      return cnt;
    }

    private static AbsPath GetXmlFilePath(AbsPath dir, string prefix)
    {
      string template = prefix + "_*.xml";
      string[] aFiles = Directory.GetFiles(dir.Path, template, SearchOption.AllDirectories /* 10.09.2020 */);
      switch (aFiles.Length)
      {
        case 0: return AbsPath.Empty;
        case 1: return new AbsPath(aFiles[0]);
        default:
          throw new InvalidOperationException("� �������� \"" + dir.Path + "\" ������� ������ ������ �����, ���������������� ������� \"" + template + "\"");
      }
    }

    #endregion

    #region AS_SOCRBASE_*.XML

    private void DoLoadXmlSocrBase(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      LoadAOTypesTable(con);
      using (XmlReader rdr = XmlReader.Create(filePath.Uri.ToString()))
      {
        while (rdr.Read())
        {
          if (rdr.NodeType == XmlNodeType.Element)
          {
            if (rdr.Name == "AddressObjectType")
            {
              int Level = DataTools.GetInt(rdr.GetAttribute("LEVEL"));
              string scName = DataTools.GetString(rdr.GetAttribute("SCNAME"));
              string socrName = DataTools.GetString(rdr.GetAttribute("SOCRNAME"));
              FindOrAddAOType(Level, scName, socrName);
              cnt++;
            }
          }
        }
      }
      FlushAOTypesTable(con);

      EndLoadFile(filePath, cnt);
    }

    #endregion

    #region AS_ADDROBJ_*.XML

    private void DoLoadXmlAddrObj(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      LoadAOTypesTable(con);
      DbCommand cmdFTS3 = CreateFTS3Command(con);
      try
      {

        using (ExtXmlReader rdr0 = new ExtXmlReader(filePath, Splash))
        {
          XmlReader rdr = rdr0.Reader;
          DBxDataWriterInfo writerInfo = CreateWriterInfo("AddrOb");
          //???writerInfo.ExpectedRowCount = dbf.RecordCount;
          using (DBxDataWriter wrt = con.CreateWriter(writerInfo))
          {
            while (rdr.Read())
            {
              if (rdr.NodeType == XmlNodeType.Element)
              {
                if (rdr.Name == "Object")
                {
                  rdr0.UpdateSplash();

                  if (!CheckXmlRegionCode(rdr))
                    continue;

                  if (!_FiasDB.DBSettings.UseHistory)
                  {
                    if (DataTools.GetInt(rdr.GetAttribute("ACTSTATUS")) != 1)
                      continue;
                    if (DataTools.GetInt(rdr.GetAttribute("LIVESTATUS")) != 1)
                      continue;
                    if (!TestDateRange(DataTools.GetDateTime(rdr.GetAttribute("STARTDATE")),
                      DataTools.GetDateTime(rdr.GetAttribute("ENDDATE"))))
                      continue;
                  }


                  #region ���������� ������������� � AddressObjects

                  //if (_FiasDB.InternalSettings.UseIdTables)
                  //{
                  //  ResRow["Id"] = GetId(RecGuidDict, SrcRow, "AOID");
                  //  if (_FiasDB.DBSettings.UseHistory)
                  //  {
                  //    ResRow["PrevId"] = GetId(RecGuidDict, SrcRow, "PREVID");
                  //    ResRow["NextId"] = GetId(RecGuidDict, SrcRow, "NEXTID");
                  //  }
                  //  ResRow["AOGuidRef"] = GetId(AOGuidDict, SrcRow, "AOGUID");
                  //  ResRow["ParentAOGuidRef"] = GetId(AOGuidDict, SrcRow, "PARENTGUID");
                  //}
                  //else
                  //{
                  wrt.SetGuid("AOID", DataTools.GetGuid(rdr.GetAttribute("AOID")));
                  if (_FiasDB.DBSettings.UseHistory)
                  {
                    wrt.SetGuid("PREVID", DataTools.GetGuid(rdr.GetAttribute("PREVID")));
                    wrt.SetGuid("NEXTID", DataTools.GetGuid(rdr.GetAttribute("NEXTID")));
                  }
                  wrt.SetGuid("AOGUID", DataTools.GetGuid(rdr.GetAttribute("AOGUID")));
                  wrt.SetGuid("PARENTGUID", DataTools.GetGuid(rdr.GetAttribute("PARENTGUID")));
                  //}

                  #endregion

                  #region ������������ ��������� �������

                  string offName = DataTools.GetString(rdr.GetAttribute("OFFNAME"));
                  wrt["OFFNAME"] = offName;

                  if (_FiasDB.InternalSettings.FTSMode == FiasFTSMode.FTS3)
                  {
                    cmdFTS3.Parameters[0].Value = FiasTools.PrepareForFTS(offName);
                    Int32 NameId = DataTools.GetInt(cmdFTS3.ExecuteScalar());
                    wrt["NameId"] = NameId;
                  }

                  #endregion

                  #region ������� ������������

                  int level = DataTools.GetInt(rdr.GetAttribute("AOLEVEL"));
                  string scName = DataTools.GetString(rdr.GetAttribute("SHORTNAME"));
                  wrt["AOTypeId"] = FindOrAddAOType(level, scName, String.Empty);

                  #endregion

                  #region ����, ������������ ��-�������

                  if (_FiasDB.DBSettings.UseHistory)
                  {
                    switch (DataTools.GetInt(rdr.GetAttribute("ACTSTATUS")))
                    {
                      case 1: wrt["Actual"] = true; break;
                      case 0: wrt["Actual"] = false; break;
                      default:
                        throw new BugException("����������� �������� ACTSTATUS=" + rdr.GetAttribute("ACTSTATUS"));
                    }

                    switch (DataTools.GetInt(rdr.GetAttribute("LIVESTATUS")))
                    {
                      case 1: wrt["Live"] = true; break;
                      case 0: wrt["Live"] = false; break;
                      default:
                        throw new BugException("����������� �������� LIVESTATUS=" + rdr.GetAttribute("LIVESTATUS"));
                    }
                  }

                  #endregion

                  #region ��������� ����

                  wrt["AOLEVEL"] = DataTools.GetInt(rdr.GetAttribute("AOLEVEL"));
                  wrt["CENTSTATUS"] = DataTools.GetInt(rdr.GetAttribute("CENTSTATUS"));
                  wrt["DIVTYPE"] = DataTools.GetInt(rdr.GetAttribute("DIVTYPE"));

                  wrt["POSTALCODE"] = CheckedGetInt(filePath, rdr, "POSTALCODE");
                  wrt["REGIONCODE"] = DataTools.GetInt(rdr.GetAttribute("REGIONCODE"));
                  if (_FiasDB.DBSettings.UseIFNS)
                  {
                    wrt["IFNSFL"] = DataTools.GetInt(rdr.GetAttribute("IFNSFL"));
                    wrt["TERRIFNSFL"] = DataTools.GetInt(rdr.GetAttribute("TERRIFNSFL"));
                    wrt["IFNSUL"] = DataTools.GetInt(rdr.GetAttribute("IFNSUL"));
                    wrt["TERRIFNSUL"] = DataTools.GetInt(rdr.GetAttribute("TERRIFNSUL"));
                  }
                  if (_FiasDB.DBSettings.UseOKATO)
                    wrt["OKATO"] = DataTools.GetInt64(rdr.GetAttribute("OKATO"));
                  if (_FiasDB.DBSettings.UseOKTMO)
                    wrt["OKTMO"] = DataTools.GetInt64(rdr.GetAttribute("OKTMO"));

                  if (_FiasDB.DBSettings.UseDates)
                  {
                    if (_FiasDB.InternalSettings.UseOADates)
                    {
                      wrt["dStartDate"] = (int)(DataTools.GetDateTime(rdr.GetAttribute("STARTDATE")).ToOADate());
                      wrt["dEndDate"] = (int)(DataTools.GetDateTime(rdr.GetAttribute("ENDDATE")).ToOADate());
                    }
                    else
                    {
                      wrt["STARTDATE"] = DataTools.GetDateTime(rdr.GetAttribute("STARTDATE"));
                      wrt["ENDDATE"] = DataTools.GetDateTime(rdr.GetAttribute("ENDDATE"));
                    }
                  }

                  #endregion

                  wrt.Write();
                  cnt++;
                }
              }
            }
            Splash.AllowCancel = false;
            wrt.Finish();
          }
        }
      }
      finally
      {
        if (cmdFTS3 != null)
          cmdFTS3.Dispose();
      }

      FlushAOTypesTable(con);

      EndLoadFile(filePath, cnt);
    }

    #endregion

    #region AS_HOUSE_*.XML

    private void DoLoadXmlHouse(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      using (ExtXmlReader rdr0 = new ExtXmlReader(filePath, Splash))
      {
        XmlReader rdr = rdr0.Reader;
        DBxDataWriterInfo writerInfo = CreateWriterInfo("House");
        //???writerInfo.ExpectedRowCount = dbf.RecordCount;
        using (DBxDataWriter wrt = con.CreateWriter(writerInfo))
        {
          rdr0.UpdateSplash();
          while (rdr.Read())
          {
            if (rdr.NodeType == XmlNodeType.Element)
            {
              if (rdr.Name == "House")
              {
                rdr0.UpdateSplash();

                if (!CheckXmlRegionCode(rdr))
                  continue;

                if (!_FiasDB.DBSettings.UseHistory)
                {
                  if (!TestDateRange(DataTools.GetDateTime(rdr.GetAttribute("STARTDATE")),
                    DataTools.GetDateTime(rdr.GetAttribute("ENDDATE"))))
                    continue;
                }

                #region ���������� ������������� � House

                wrt.SetGuid("HOUSEID", DataTools.GetGuid(rdr.GetAttribute("HOUSEID")));
                wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(rdr.GetAttribute("HOUSEGUID")));
                wrt.SetGuid("AOGUID", DataTools.GetGuid(rdr.GetAttribute("AOGUID")));

                #endregion

                #region ��������� ����

                string sHouseNum = rdr.GetAttribute("HOUSENUM");
                int nHouseNum = FiasTools.GetNumInt(ref sHouseNum);
                wrt["nHouseNum"] = nHouseNum; // 0 ������������
                wrt.SetString("HOUSENUM", sHouseNum);

                string sBuildNum = rdr.GetAttribute("BUILDNUM");
                int nBuildNum = FiasTools.GetNumInt(ref sBuildNum);
                wrt["nBuildNum"] = nBuildNum; // 0 ������������
                wrt.SetString("BUILDNUM", sBuildNum);

                string sStrucNum = rdr.GetAttribute("STRUCNUM");
                int nStrucNum = FiasTools.GetNumInt(ref sStrucNum);
                wrt["nStrucNum"] = nStrucNum; // 0 ������������
                wrt.SetString("STRUCNUM", sStrucNum);

                wrt["STRSTATUS"] = DataTools.GetInt(rdr.GetAttribute("STRSTATUS"));
                wrt["ESTSTATUS"] = DataTools.GetInt(rdr.GetAttribute("ESTSTATUS"));
                wrt["DIVTYPE"] = DataTools.GetInt(rdr.GetAttribute("DIVTYPE"));

                wrt["POSTALCODE"] = CheckedGetInt(filePath, rdr, "POSTALCODE");

                if (_FiasDB.DBSettings.UseIFNS)
                {
                  wrt["IFNSFL"] = DataTools.GetInt(rdr.GetAttribute("IFNSFL"));
                  wrt["TERRIFNSFL"] = DataTools.GetInt(rdr.GetAttribute("TERRIFNSFL"));
                  wrt["IFNSUL"] = DataTools.GetInt(rdr.GetAttribute("IFNSUL"));
                  wrt["TERRIFNSUL"] = DataTools.GetInt(rdr.GetAttribute("TERRIFNSUL"));
                }
                if (_FiasDB.DBSettings.UseOKATO)
                  wrt["OKATO"] = DataTools.GetInt64(rdr.GetAttribute("OKATO"));
                if (_FiasDB.DBSettings.UseOKTMO)
                  wrt["OKTMO"] = DataTools.GetInt64(rdr.GetAttribute("OKTMO"));

                if (_FiasDB.DBSettings.UseDates)
                {
                  if (_FiasDB.InternalSettings.UseOADates)
                  {
                    wrt["dStartDate"] = (int)(DataTools.GetDateTime(rdr.GetAttribute("STARTDATE")).ToOADate());
                    wrt["dEndDate"] = (int)(DataTools.GetDateTime(rdr.GetAttribute("ENDDATE")).ToOADate());
                  }
                  else
                  {
                    wrt["STARTDATE"] = DataTools.GetDateTime(rdr.GetAttribute("STARTDATE"));
                    wrt["ENDDATE"] = DataTools.GetDateTime(rdr.GetAttribute("ENDDATE"));
                  }
                }

                #endregion

                wrt.Write();
                cnt++;
              }
            }
          }
          Splash.AllowCancel = false;
          wrt.Finish();
        }
      }

      EndLoadFile(filePath, cnt);
    }

    #endregion

    #region AS_ROOM_*.XML

    private void DoLoadXmlRoom(DBxConBase con, AbsPath filePath)
    {
      BeginLoadFile(filePath);
      int cnt = 0;

      using (ExtXmlReader rdr0 = new ExtXmlReader(filePath, Splash))
      {
        XmlReader rdr = rdr0.Reader;
        DBxDataWriterInfo writerInfo = CreateWriterInfo("Room");
        //???writerInfo.ExpectedRowCount = dbf.RecordCount;
        using (DBxDataWriter wrt = con.CreateWriter(writerInfo))
        {
          rdr0.UpdateSplash();

          while (rdr.Read())
          {
            if (rdr.NodeType == XmlNodeType.Element)
            {
              if (rdr.Name == "Room")
              {
                rdr0.UpdateSplash();
                if (!CheckXmlRegionCode(rdr))
                  continue;

                if (!_FiasDB.DBSettings.UseHistory)
                {
                  if (DataTools.GetInt(rdr.GetAttribute("LIVESTATUS")) != 1)
                    continue;
                  if (!TestDateRange(DataTools.GetDateTime(rdr.GetAttribute("STARTDATE")),
                    DataTools.GetDateTime(rdr.GetAttribute("ENDDATE"))))
                    continue;
                }


                #region ���������� ������������� � Room

                wrt.SetGuid("ROOMID", DataTools.GetGuid(rdr.GetAttribute("ROOMID")));
                if (_FiasDB.DBSettings.UseHistory)
                {
                  wrt.SetGuid("PREVID", DataTools.GetGuid(rdr.GetAttribute("PREVID")));
                  wrt.SetGuid("NEXTID", DataTools.GetGuid(rdr.GetAttribute("NEXTID")));
                }
                wrt.SetGuid("ROOMGUID", DataTools.GetGuid(rdr.GetAttribute("ROOMGUID")));
                wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(rdr.GetAttribute("HOUSEGUID")));

                #endregion

                #region ����, ������������ ��-�������

                if (_FiasDB.DBSettings.UseHistory)
                {
                  switch (DataTools.GetInt(rdr.GetAttribute("LIVESTATUS")))
                  {
                    case 1: wrt["Live"] = true; break;
                    case 0: wrt["Live"] = false; break;
                    default:
                      throw new BugException("����������� �������� LIVESTATUS=" + rdr.GetAttribute("LIVESTATUS"));
                  }
                }

                #endregion

                #region ��������� ����

                string sFlatNumber = rdr.GetAttribute("FLATNUMBER");
                int nFlatNumber = FiasTools.GetNumInt(ref sFlatNumber);
                wrt["nFlatNumber"] = nFlatNumber;
                wrt.SetString("FLATNUMBER", sFlatNumber);

                string sRoomNumber = rdr.GetAttribute("ROOMNUMBER");
                int nRoomNumber = FiasTools.GetNumInt(ref sRoomNumber);
                wrt["nRoomNumber"] = nRoomNumber;
                wrt.SetString("ROOMNUMBER", sRoomNumber);

                wrt["FLATTYPE"] = DataTools.GetInt(rdr.GetAttribute("FLATTYPE"));
                wrt["ROOMTYPE"] = DataTools.GetInt(rdr.GetAttribute("ROOMTYPE"));

                wrt["POSTALCODE"] = CheckedGetInt(filePath, rdr, "POSTALCODE");

                if (_FiasDB.DBSettings.UseDates)
                {
                  if (_FiasDB.InternalSettings.UseOADates)
                  {
                    wrt["dStartDate"] = (int)(DataTools.GetDateTime(rdr.GetAttribute("STARTDATE")).ToOADate());
                    wrt["dEndDate"] = (int)(DataTools.GetDateTime(rdr.GetAttribute("ENDDATE")).ToOADate());
                  }
                  else
                  {
                    wrt["STARTDATE"] = DataTools.GetDateTime(rdr.GetAttribute("STARTDATE"));
                    wrt["ENDDATE"] = DataTools.GetDateTime(rdr.GetAttribute("ENDDATE"));
                  }
                }

                #endregion

                wrt.Write();
                cnt++;
              }
            }
          }
          Splash.AllowCancel = false;
          wrt.Finish();
        }
      }
      EndLoadFile(filePath, cnt);
    }

    #endregion

    #region AS_DEL_*.XML

    /// <summary>
    /// �������� ������� �� ������ �������� ��������, �����, ���������
    /// </summary>
    /// <param name="con"></param>
    /// <param name="filePath">���� � XML-�����</param>
    /// <param name="elName">����� ����� ��������</param>
    /// <param name="resTableName">��� ������� � ���� ������</param>
    private void DoDelXml(DBxConBase con, AbsPath filePath, string elName, string resTableName)
    {
      BeginLoadFile(filePath);
      int cnt;

      using (ExtXmlReader rdr0 = new ExtXmlReader(filePath, Splash))
      {
        XmlReader rdr = rdr0.Reader;

        // ��� ��������� GUID-���� ("AOID")
        string pkName = FiasDB.DB.Struct.Tables[resTableName].PrimaryKey[0];
        List<Guid> delIds = new List<Guid>();
        Splash.AllowCancel = true;
        while (rdr.Read())
        {
          if (rdr.NodeType == XmlNodeType.Element)
          {
            if (rdr.Name == elName)
            {
              rdr0.UpdateSplash();

              Guid g = DataTools.GetGuid(rdr.GetAttribute(pkName));
              delIds.Add(g);

              if (delIds.Count >= 500)
              {
                con.Delete(resTableName, new ValuesFilter(pkName, delIds.ToArray()));
                delIds.Clear();
              }
            }
          }
        }

        if (delIds.Count > 0)
          con.Delete(resTableName, new ValuesFilter(pkName, delIds.ToArray()));
        cnt = delIds.Count;
      }

      EndLoadFile(filePath, cnt);
    }

    #endregion

    /// <summary>
    /// �������� ��� XML-����� � ���������� ����������� ����������
    /// </summary>
    private class ExtXmlReader : SimpleDisposableObject
    {
      // 03.01.2021
      // ����� ������������ ������� ����� ��� �����������

      #region ����������� � Dispose

      public ExtXmlReader(AbsPath path, ISplash splash)
      {
        _FileStream = new FileStream(path.Path, FileMode.Open);

        _Splash = splash;
        // XML-���� ����� ���� ��������� �� ������.
        // �� ��������� ������������ Int32, ������� � ������ �� 64��.
        _Splash.PercentMax = (int)(_FileStream.Length >> 16);
        _Splash.AllowCancel = true;

        _Reader = XmlReader.Create(_FileStream);
      }

      protected override void Dispose(bool disposing)
      {
        if (disposing)
        {
          _Splash.AllowCancel = false;
          _Splash.PercentMax = 0;

          _Reader.Close();
        }
        base.Dispose(disposing);
      }

      #endregion

      #region ��������

      /// <summary>
      /// ����� ��� ���������� ������� �������
      /// </summary>
      private Stream _FileStream;

      /// <summary>
      /// �������� ������
      /// </summary>
      public XmlReader Reader { get { return _Reader; } }
      private XmlReader _Reader;

      private ISplash _Splash;

      private int _UpdateSplashCounter;

      #endregion

      #region ���������� ��������

      public void UpdateSplash()
      {
        _UpdateSplashCounter++;
        if (_UpdateSplashCounter >= 1000)
        {
          _UpdateSplashCounter = 0;
          _Splash.Percent = (int)(_FileStream.Position >> 16);
        }
      }

      #endregion
    }

    private bool CheckXmlRegionCode(XmlReader rdr)
    {
      if (_FiasDB.DBSettings.RegionCodes.Count == 0)
        return true;
      string code = DataTools.GetString(rdr.GetAttribute("REGIONCODE"));
      if (code.Length == 0)
        return true;
      return _FiasDB.DBSettings.RegionCodes.Contains(code);
    }

    /// <summary>
    /// ��������� ��������� �������� � ���������� ������.
    /// ���������� ��������� � Trace.
    /// ������������ 0.
    /// � ������� as_room*.xml �������������� �� 16.03.2019 ���� ���� ���������� �������� ������.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="rdr"></param>
    /// <param name="attrName"></param>
    /// <returns></returns>
    private static int CheckedGetInt(AbsPath filePath, XmlReader rdr, string attrName)
    {
      try
      {
        return DataTools.GetInt(rdr.GetAttribute(attrName));
      }
      catch
      {
        Trace.WriteLine("Error reading integer value from xml File: " + filePath.Path + ", AttrName=" + attrName);
        return 0;
      }
    }


    #endregion

    #region �������� �������� � XML ��� DBF

    /// <summary>
    /// �������� ����� - �������� ������
    /// </summary>
    /// <param name="dir">������� � ������� DBF ��� XML. ��������� �������� �� ���������������</param>
    /// <param name="actualDate">���� ������������ ����������</param>
    /// <returns>���������� ����������� ������</returns>
    public int LoadDir(AbsPath dir, DateTime actualDate)
    {
      if (dir.IsEmpty)
        throw new ArgumentException("������� �� �����", "dir");

      Splash.PhaseText = "����������� ������� ������";

      if (!Directory.Exists(dir.Path))
        throw new FileNotFoundException("�� ������ ������� \"" + dir.Path + "\"");

      string[] aXmlFiles = Directory.GetFiles(dir.Path, "*.xml", SearchOption.TopDirectoryOnly);
      string[] aDbfFiles = Directory.GetFiles(dir.Path, "*.dbf", SearchOption.TopDirectoryOnly);
      if (aXmlFiles.Length > 0 && aDbfFiles.Length == 0)
        return LoadXmlDir(dir, actualDate);
      if (aDbfFiles.Length > 0 && aXmlFiles.Length == 0)
        return LoadDbfDir(dir, actualDate);
      if (aXmlFiles.Length == 0 && aDbfFiles.Length == 0)
        return 0;

      throw new InvalidOperationException("� �������� \"" + dir.Path + "\" ������������ � XML � DBF-�����");
    }

    #endregion

    #region �������� �� ������

    /// <summary>
    /// �������� ���������� �� ������.
    /// ��������� ���������� �� ��������� ������� � �������� LoadDir().
    /// �������������� ������� ������ ������� ������������ ������� FileCompressor
    /// </summary>
    /// <param name="path">���� � ����� ������</param>
    /// <param name="actualDate">���� ������������ ����������</param>
    /// <returns>���������� ����������� ������ �� ������</returns>
    public int LoadArchiveFile(AbsPath path, DateTime actualDate)
    {
      if (path.IsEmpty)
        throw new ArgumentException("���� �� �����", "path");

      if (FileCompressor.GetArchiveTypeFromFileName(path) == FileComressorArchiveType.Unknown)
        throw new ArgumentException("������������ ��� �����: " + path.FileName + ". ���������������� ������ ������");

      Splash.PhaseText = "���������� ������ " + path.FileName;
      Trace.WriteLine(_FileStartTime.ToString("G") + " Unpacking " + path.Path + " to the temp directory");

      if (!File.Exists(path.Path))
        throw new FileNotFoundException("�� ������ ���� \"" + path.Path + "\"");

      int nFiles;

      using (TempDirectory tempDir = new TempDirectory())
      {
        FileCompressor fc = new FileCompressor();
        fc.ArchiveFileName = path;
        fc.FileDirectory = tempDir.Dir;
        fc.Splash = Splash;
        fc.Decompress();

        nFiles = LoadDir(tempDir.Dir, actualDate);
      }
      return nFiles;
    }

    #endregion

    #region �������� STARTDATE - ENDDATE

    private DateTime _Today;

    private bool TestDateRange(DateTime startDate, DateTime endDate)
    {
      return startDate <= _Today && endDate >= _Today;
    }


    #endregion

    #region ������� ���������������

#if XXX
    /// <summary>
    /// ����������� ���������� ������ "xxRecIds" � "xxGuids"
    /// </summary>
    /// <param name="srcTable">������� ��� ���������� ���������������</param>
    /// <param name="srcColNames"></param>
    /// <param name="con">���������� � �� "fias"</param>
    /// <param name="resTableName"></param>
    /// <returns>������� ���������������</returns>
    private Dictionary<string, Int32> CreateForwardGuidRows(DataTable srcTable, string[] srcColNames, DBxCon con, string resTableName)
    {
    #region ������� ��� GUID'�, ������������ � �������

      DataTable GuidTable = new DataTable(resTableName);
      GuidTable.Columns.Add("Guid", typeof(string));
      DataTools.SetPrimaryKey(GuidTable, "Guid");
      foreach (DataRow Row in srcTable.Rows)
      {
        for (int i = 0; i < srcColNames.Length; i++)
        {
          string Guid = DataTools.GetString(Row, srcColNames[i]);
          if (Guid.Length > 0)
            DataTools.FindOrAddPrimaryKeyRow(GuidTable, Guid);
        }
      }

      // ���� - ������������� AOID � �����
      // �������� - ���������� ������������� ������ � ������� "AddrOb"
      Dictionary<string, Int32> IdDict = new Dictionary<string, Int32>(GuidTable.Rows.Count);

      if (GuidTable.Rows.Count == 0)
        return IdDict; // ������� �� �����

    #endregion

    #region �����/���������� ������� � ���� ������

      Int32[] Ids = con.FindOrAddRecords(GuidTable);

    #endregion

    #region ������� �������

      for (int i = 0; i < GuidTable.Rows.Count; i++)
        IdDict.Add(GuidTable.Rows[i][0].ToString(), Ids[i]);

    #endregion

      return IdDict;
    }

    private static object GetId(Dictionary<string, Int32> guidDict, DataRow row, string columnName)
    {
      string Guid = DataTools.GetString(row, columnName);
      if (Guid.Length == 0)
        return DBNull.Value;
      Int32 Id;
      if (!guidDict.TryGetValue(Guid, out Id))
        throw new BugException("� ��������� �� ������ GUID=\"" + Guid + "\" �� ���� \"" + columnName + "\"");
      return Id;
    }
#endif

    #endregion

    #region ����������� ������� AOType

    /// <summary>
    /// �������������� ������� ����� �������� ��������.
    /// ���� ����������� ������ ������� "AOType" �� �������������� ������� LoadAOTypesTable().
    /// � �������� ��������� ������� ������ ���� ����� ����������� ������.
    /// ����� ������� �������� ������ �������������� ���������� FlushAOTypesTable().
    /// ���� � ������� ���� ��������� �������
    /// </summary>
    private DataTable _AOTypesTable;
    private int _AOTypesTableRowCount;
    private Int32 _AOTypesTableLastId1;
    private Int32 _AOTypesTableLastId2;

    ///// <summary>
    ///// ������������� ��������� ������ � ������� AOType.
    ///// ������������ ��� forward-����� � AddressObjects
    ///// </summary>
    //private Int32 DummyAOTypeId;

    private void LoadAOTypesTable(DBxConBase con)
    {
      if (_AOTypesTable != null)
        return;

      //DummyAOTypeId = Con.FindOrAddRecord("AOType", new DBxColumns("LEVEL,SCNAME,SOCRNAME"), new object[] { 0, "?", "?" });

      _AOTypesTable = con.FillSelect("AOType");
      DataTools.SetPrimaryKey(_AOTypesTable, "Id");
      _AOTypesTable.DefaultView.Sort = "LEVEL,SCNAME";

      _AOTypesTableRowCount = _AOTypesTable.Rows.Count;
      _AOTypesTableLastId1 = DataTools.MaxInt(_AOTypesTable, "Id", false) ?? 0;
      _AOTypesTableLastId2 = _AOTypesTableLastId1;
    }

    private void FlushAOTypesTable(DBxConBase con)
    {
      if (_AOTypesTableRowCount == _AOTypesTable.Rows.Count)
        return; // ������ ����������

      DataTable tempTable;
      using (DataView dv2 = new DataView(_AOTypesTable))
      {
        dv2.RowFilter = "Id>" + _AOTypesTableLastId1.ToString();
        tempTable = dv2.ToTable();
#if DEBUG
        int wantedRowCount = _AOTypesTable.Rows.Count - _AOTypesTableRowCount;
        if (wantedRowCount != tempTable.Rows.Count)
          throw new BugException("������������ ����� ����������� ������� � ������� AOTypes: " +
            tempTable.Rows.Count.ToString() + ". ���������: " + wantedRowCount.ToString());
#endif
      }

      con.AddRecords("AOType", tempTable);
      _AOTypesTableRowCount = _AOTypesTable.Rows.Count;
      _AOTypesTableLastId1 = _AOTypesTableLastId2;
    }

    /// <summary>
    /// ����� ��� ���������� ������ � ������� AOType
    /// </summary>
    /// <param name="level">�������</param>
    /// <param name="scName">����������</param>
    /// <param name="socrName">������ ������������</param>
    /// <returns>������������� ��������� ��� ��������� ������</returns>
    private Int32 FindOrAddAOType(int level, string scName, string socrName)
    {
      if (String.IsNullOrEmpty(scName))
        return 0;
      if (level <= 0)
        throw new ArgumentOutOfRangeException("level");

      int p = _AOTypesTable.DefaultView.Find(new object[] { level, scName });
      DataRow row;
      if (p >= 0)
      {
        // ������������ ����������
        row = _AOTypesTable.DefaultView[p].Row;
        Int32 id = DataTools.GetInt(row, "Id");

        if ((!String.IsNullOrEmpty(socrName)) && id < _AOTypesTableLastId1)
        {
          if (socrName != DataTools.GetString(row, "SOCRNAME"))
          {
            using (DBxCon Con = new DBxCon(FiasDB.DB.MainEntry))
            {
              Con.SetValue("AOType", id, "SOCRNAME", socrName);
              row["SOCRNAME"] = socrName;
            }
          }
        }
        return id;
      }
      else
      {
        row = _AOTypesTable.NewRow();
        _AOTypesTableLastId2++;
        row["Id"] = _AOTypesTableLastId2;
        row["LEVEL"] = level;
        row["SCNAME"] = scName;
        if (String.IsNullOrEmpty(socrName)) // ����������� ���������� �� �������� ������� ��������
          row["SOCRNAME"] = scName;
        else
          row["SOCRNAME"] = socrName;
        _AOTypesTable.Rows.Add(row);
        return _AOTypesTableLastId2;
      }
    }

    #endregion

    #region ������ � ��������� ����������

    private DateTime _ActualDate;

    private Int32 _UpdateId;

    /// <summary>
    /// ���������� � ������ ����������.
    /// ��������� ������ � ������� "ClassifUpdate"
    /// </summary>
    /// <param name="con"></param>
    /// <param name="actualDate"></param>
    /// <param name="source"></param>
    private void BeginUpdate(DBxConBase con, DateTime actualDate, FiasDBUpdateSource source)
    {
      _UpdateId = con.AddRecordWithIdResult("ClassifUpdate", new DBxColumns("ActualDate,Source,Cumulative,StartTime"),
        new object[] { actualDate, (int)source, !_PrimaryLoad, DateTime.Now });
      _ActualDate = actualDate;
    }

    /// <summary>
    /// ���������� ����� ��������� ����������.
    /// �������� ������ � ������� "ClassifUpdate". �������� ���� "ActualDate" � "UpdateTime" � ������� "ClassifInfo".
    /// 10.09.2020. ����� ����������� ����������
    /// </summary>
    /// <param name="con"></param>
    private void EndUpdate(DBxConBase con)
    {
      DateTime finishTime = DateTime.Now;
      con.SetValues("ClassifInfo", 1, new DBxColumns("ActualDate,UpdateTime"),
        new object[] { _ActualDate, finishTime });
      con.SetValue("ClassifUpdate", _UpdateId, "FinishTime", finishTime);

      FiasDBStat stat = _FiasDB.GetRealDBStat(con);
      con.SetValue("ClassifInfo", 1, "AddrObCount", stat.AddrObCount);
      con.SetValue("ClassifUpdate", _UpdateId, "AddrObCount", stat.AddrObCount);
      if (_FiasDB.DBSettings.UseHouse)
      {
        con.SetValue("ClassifInfo", 1, "HouseCount", stat.HouseCount);
        con.SetValue("ClassifUpdate", _UpdateId, "HouseCount", stat.HouseCount);
      }
      if (_FiasDB.DBSettings.UseRoom)
      {
        con.SetValue("ClassifInfo", 1, "RoomCount", stat.RoomCount);
        con.SetValue("ClassifUpdate", _UpdateId, "RoomCount", stat.RoomCount);
      }


      con.ClearPool(); // 09.09.2020
    }

    #endregion

    #region �����������

    DateTime _FileStartTime;

    private void BeginLoadFile(AbsPath filePath)
    {
      _FileStartTime = DateTime.Now;
      FileInfo fi = new FileInfo(filePath.Path);
      Trace.WriteLine(_FileStartTime.ToString("G") + " Start loading from " + filePath.FileName + ". FileSize=" + FileTools.GetMBSizeText(fi.Length));
    }

    private void EndLoadFile(AbsPath filePath, int records)
    {
      DateTime endTime = DateTime.Now;
      TimeSpan ts = endTime - _FileStartTime;
      string txt = String.Empty;
      if (ts.TotalSeconds > 0.1)
      {
        double ops = (double)records / ts.TotalSeconds;
        txt = ". Records per sec=" + ops.ToString();
      }
      Trace.WriteLine(_FileStartTime.ToString("G") + " Finish loading from " + filePath.FileName + ", time=" + ts.ToString() + ", records=" + records.ToString() + txt);
    }

    #endregion

    #region ������

    /// <summary>
    /// ��������� ���������� ��� ����������
    /// </summary>
    /// <returns></returns>
    private DBxConBase CreateCon()
    {
      DBxConBase con = _FiasDB.UpdateEntry.CreateCon();
      if (con.TraceEnabled)
      {
        con.TraceEnabled = false; // ����� ����� ����� ����� �������, ���� ������������ ��������
        Trace.WriteLine("  DBxConBase.TraceEnabled was set to false for updating connection");
        if (con is FreeLibSet.Data.SQLite.SQLiteDBxCon)
        {
          con.SQLExecuteNonQuery("PRAGMA locking_mode=EXCLUSIVE");
          con.SQLExecuteNonQuery("PRAGMA count_changes=OFF");
        }
      }
      return con;
    }


    private DBxDataWriterInfo CreateWriterInfo(string tableName)
    {
      DBxTableStruct ts = _FiasDB.DB.Struct.Tables[tableName];
      DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
      writerInfo.Mode = _PrimaryLoad ? DBxDataWriterMode.Insert : DBxDataWriterMode.InsertOrUpdate;
      writerInfo.TableName = ts.TableName;
      writerInfo.Columns = ts.Columns.Columns;
      writerInfo.SearchColumns = new DBxColumns(ts.Columns[0].ColumnName); // 02.08.2020
      writerInfo.TransactionPulseRowCount = 100000; // SQLite ����� �������������
      return writerInfo;
    }

    #endregion
  }
}
