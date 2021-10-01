using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.IO;
using System.IO;
using AgeyevAV.DBF;
using System.Data;
using AgeyevAV.ExtDB;
using System.Xml;
using System.Data.Common;
using System.Diagnostics;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.FIAS
{
  /// <summary>
  /// Источник обновления классификатора ФИАС
  /// </summary>
  public enum FiasDBUpdateSource
  {
    /// <summary>
    /// Загрузка ФИАС в формате XML
    /// </summary>
    Xml = 1,

    /// <summary>
    /// Загрузка ФИАС в формате DBF
    /// </summary>
    Dbf = 2
  }

  /// <summary>
  /// Содержит методы для выполнения обновления базы данных, используя локальные файлы.
  /// Одновременно может существовать не более одного экземпляра класса.
  /// </summary>
  public sealed class FiasDBUpdater : SimpleDisposableObject
  {
    // 03.01.2021
    // Можно использовать базовый класс без деструктора

    #region Конструктор и Dispose

    /// <summary>
    /// Создает объект для обновления базы.
    /// Автоматически определяется вариант первичной загрузки/обновления
    /// </summary>
    /// <param name="fiasDB">База данных</param>
    public FiasDBUpdater(FiasDB fiasDB)
      : this(fiasDB, fiasDB.IsEmpty)
    {
    }

    /// <summary>
    /// Создает объект для обновления базы.
    /// Автоматически определяется вариант первичной загрузки/обновления
    /// </summary>
    /// <param name="fiasDB">База данных</param>
    /// <param name="primaryLoad">true - первичное наполнение базы, false - обновление</param>
    public FiasDBUpdater(FiasDB fiasDB, bool primaryLoad)
    {
      if (fiasDB == null)
        throw new ArgumentNullException("fiasDB");

      if (!fiasDB.UpdateStruct)
        throw new InvalidOperationException("База данных ФИАС не предназначена для обновления");

      lock (fiasDB)
      {
        if (fiasDB.CurrentUpdater != null)
          throw new InvalidOperationException("Предыдущее обновление еще не завершено");
        fiasDB.CurrentUpdater = this;
      }
      _FiasDB = fiasDB;
      _Splash = new DummySplash();
      _Today = DateTime.Today;
      _PrimaryLoad = primaryLoad;
    }

    /// <summary>
    /// Вызывается из Dispose().
    /// Отсоединяет этот объект от FiasDB
    /// </summary>
    /// <param name="Disposing">true, если вызван метод Dispose(), а не деструктор</param>
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

    #region Свойства

    /// <summary>
    /// База данных для которой выполняется обновление
    /// </summary>
    public FiasDB FiasDB { get { return _FiasDB; } }
    private FiasDB _FiasDB;

    /// <summary>
    /// Заставка.
    /// Если свойство установлено, то используется во время загрузки файлов.
    /// Может не задаваться.
    /// Если значение не установлено, возвращается фиктивная заставка, а не null.
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
    /// Устанавливается в конструкторе в true, если выполняется первичная загрузка классификатора, а не обновление.
    /// При этом запись в таблицы может выполняться в режиме INSERT, а не INSERT_OR_UPDATE
    /// </summary>
    public bool PrimaryLoad { get { return _PrimaryLoad; } }
    private bool _PrimaryLoad;

    #endregion

    #region Загрузка DBF-файлов

    #region Основной метод

    /// <summary>
    /// Загрузка каталога с DBF-файлами
    /// </summary>
    /// <param name="dir">Каталог</param>
    /// <param name="actualDate">Дата актуальности обновления</param>
    /// <returns>Количество DBF-файлов, которые были обработаны.
    /// Ненужные для загрузки файлы не учитываются</returns>
    public int LoadDbfDir(AbsPath dir, DateTime actualDate)
    {
      if (!Directory.Exists(dir.Path))
        throw new FileNotFoundException("Не найден каталог \"" + dir.Path + "\"");

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

        string[] Files = Directory.GetFiles(dir.Path, "*.dbf", SearchOption.TopDirectoryOnly);
        Array.Sort<string>(Files);
        //Splash.PercentMax = Files.Length;
        //Splash.AllowCancel = true;
        for (int i = 0; i < Files.Length; i++)
        {
          AbsPath filePath = new AbsPath(Files[i]);
          if (filePath.FileNameWithoutExtension.ToUpperInvariant() == "SOCRBASE")
            continue;
          Splash.PhaseText = filePath.FileName;
          if (DoLoadDbfFile(con, filePath))
            cnt++;
          Splash.PhaseText = oldPT;
          Splash.IncPercent();
        }

        // TODO: таблицы удаления DADDROBJ, ...

        Splash.PhaseText = "Запись в таблицу истории обновлений";
        EndUpdate(con);
        Splash.PhaseText = oldPT;
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBUpdate finished. DBF files proceed: " + cnt.ToString());
      }

      GC.Collect(); // 08.09.2020

      return cnt;
    }
    private bool DoLoadDbfFile(DBxConBase con, AbsPath filePath)
    {
      string Name = filePath.FileNameWithoutExtension.ToUpperInvariant();
      if (Name == "SOCRBASE")
      {
        DoLoadDbfSocrBase(con, filePath);
        return true;
      }
      if (Name.StartsWith("ADDROB") && CheckDbfFileNameRegion(Name.Substring(6)))
      {
        DoLoadDbfAddrOb(con, filePath);
        return true;
      }
      if (Name.StartsWith("HOUSE") && CheckDbfFileNameRegion(Name.Substring(5)) && _FiasDB.DBSettings.UseHouse)
      {
        DoLoadDbfFileHouse(con, filePath);
        return true;
      }
      if (Name.StartsWith("ROOM") && CheckDbfFileNameRegion(Name.Substring(4)) && _FiasDB.DBSettings.UseRoom)
      {
        DoLoadDbfFileRoom(con, filePath);
        return true;
      }

      // 25.02.2021
      if (Name.StartsWith("DAD") || Name.StartsWith("DHOUSE") || Name.StartsWith("DROOM"))
      {
        throw new NotImplementedException("Не реализована обработка удаления записей ФИАС. Файл " + filePath.Path);
      }

      return false;
    }

    private bool CheckDbfFileNameRegion(string nameSuffix)
    {
      // Проверяем, что все символы - цифры и их не меньше двух
      if (nameSuffix.Length < 2)
        return false;
      for (int i = 0; i < nameSuffix.Length; i++)
      {
        if (nameSuffix[i] < '0' || nameSuffix[i] > '9')
          return false;
      }

      if (FiasDB.DBSettings.RegionCodes.Count == 0)
        return true;

      string regCode = nameSuffix.Substring(0, 2); // есть дополнительные DBF-файлы, например, "ROOM7701.DBF", когда основной файл слишком большой для DBF
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
          int Level = dbf.GetInt("LEVEL");
          string scName = dbf.GetString("SCNAME");
          string socrName = dbf.GetString("SOCRNAME");
          FindOrAddAOType(Level, scName, socrName);
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
                Splash.PhaseText = filePath.FileName + " (" + dbf.RecNo.ToString() + " из " + dbf.RecordCount.ToString() + ")";
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

              #region Внутренний идентификатор в AddressObjects

              wrt.SetGuid("AOID", DataTools.GetGuid(dbf.GetString("AOID")));
              if (_FiasDB.DBSettings.UseHistory)
              {
                wrt.SetGuid("PREVID", DataTools.GetGuid(dbf.GetString("PREVID")));
                wrt.SetGuid("NEXTID", DataTools.GetGuid(dbf.GetString("NEXTID")));
              }
              wrt.SetGuid("AOGUID", DataTools.GetGuid(dbf.GetString("AOGUID")));
              wrt.SetGuid("PARENTGUID", DataTools.GetGuid(dbf.GetString("PARENTGUID")));

              #endregion

              #region Наименование адресного объекта

              string OffName = dbf.GetString("OFFNAME");
              wrt["OFFNAME"] = OffName;

              if (_FiasDB.InternalSettings.FTSMode == FiasFTSMode.FTS3)
              {
                cmdFTS3.Parameters[0].Value = FiasTools.PrepareForFTS(OffName);
                Int32 NameId = DataTools.GetInt(cmdFTS3.ExecuteScalar());
                wrt["NameId"] = NameId;
              }

              #endregion

              #region Краткое наименование

              int Level = dbf.GetInt("AOLEVEL");
              string scName = dbf.GetString("SHORTNAME");
              wrt["AOTypeId"] = FindOrAddAOType(Level, scName, String.Empty);

              #endregion

              #region Поля, записываемые по-другому

              if (_FiasDB.DBSettings.UseHistory)
              {
                switch (dbf.GetInt("ACTSTATUS"))
                {
                  case 1: wrt["Actual"] = true; break;
                  case 0: wrt["Actual"] = false; break;
                  default:
                    throw new BugException("Неизвестное значение ACTSTATUS=" + dbf.GetInt("ACTSTATUS"));
                }

                switch (dbf.GetInt("LIVESTATUS"))
                {
                  case 1: wrt["Live"] = true; break;
                  case 0: wrt["Live"] = false; break;
                  default:
                    throw new BugException("Неизвестное значение LIVESTATUS=" + dbf.GetInt("LIVESTATUS"));
                }
              }

              #endregion

              #region Остальные поля

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
            } // перебор строк в DBF-файле

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
              Splash.PhaseText = filePath.FileName + " (" + dbf.RecNo.ToString() + " из " + dbf.RecordCount.ToString() + ")";
            }
            if (!_FiasDB.DBSettings.UseHistory)
            {
              if (!TestDateRange(dbf.GetDate("STARTDATE"), dbf.GetDate("ENDDATE")))
                continue;
            }

            #region Внутренний идентификатор в House

            wrt.SetGuid("HOUSEID", DataTools.GetGuid(dbf.GetString("HOUSEID")));
            wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(dbf.GetString("HOUSEGUID")));
            wrt.SetGuid("AOGUID", DataTools.GetGuid(dbf.GetString("AOGUID")));

            #endregion

            #region Остальные поля

            string sHouseNum = dbf.GetString("HOUSENUM");
            int nHouseNum = FiasTools.GetNumInt(ref sHouseNum);
            wrt["nHouseNum"] = nHouseNum; // 0 записывается
            wrt.SetString("HOUSENUM", sHouseNum);

            string sBuildNum = dbf.GetString("BUILDNUM");
            int nBuildNum = FiasTools.GetNumInt(ref sBuildNum);
            wrt["nBuildNum"] = nBuildNum; // 0 записывается
            wrt.SetString("BUILDNUM", sBuildNum);

            string sStrucNum = dbf.GetString("STRUCNUM");
            int nStrucNum = FiasTools.GetNumInt(ref sStrucNum);
            wrt["nStrucNum"] = nStrucNum; // 0 записывается
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
          } // цикл по dbf.Read()

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
              Splash.PhaseText = filePath.FileName + " (" + dbf.RecNo.ToString() + " из " + dbf.RecordCount.ToString() + ")";
            }

            if (!_FiasDB.DBSettings.UseHistory)
            {
              if (!TestDateRange(dbf.GetDate("STARTDATE"), dbf.GetDate("ENDDATE")))
                continue;
              if (dbf.GetInt("LIVESTATUS") != 1)
                continue;
            }

            #region Внутренний идентификатор в Room

            wrt.SetGuid("ROOMID", DataTools.GetGuid(dbf.GetString("ROOMID")));
            if (_FiasDB.DBSettings.UseHistory)
            {
              wrt.SetGuid("PREVID", DataTools.GetGuid(dbf.GetString("PREVID")));
              wrt.SetGuid("NEXTID", DataTools.GetGuid(dbf.GetString("NEXTID")));
            }
            wrt.SetGuid("ROOMGUID", DataTools.GetGuid(dbf.GetString("ROOMGUID")));
            wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(dbf.GetString("HOUSEGUID")));

            #endregion

            #region Поля, записываемые по-другому

            if (_FiasDB.DBSettings.UseHistory)
            {
              switch (dbf.GetInt("LIVESTATUS"))
              {
                case 1: wrt["Live"] = true; break;
                case 0: wrt["Live"] = false; break;
                default:
                  throw new BugException("Неизвестное значение LIVESTATUS=" + dbf.GetInt("LIVESTATUS"));
              }
            }

            #endregion

            #region Остальные поля

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
          } // цикл по строкам DBF-файла 

          wrt.Finish();
        }
      }

      EndLoadFile(filePath, cnt);
    }

    /// <summary>
    /// Получение числового значения с перехватом ошибки.
    /// Исключение выводится в Trace.
    /// Возвращается 0.
    /// В таблице room19.dbf классификатора от 02.03.2019 есть нечисловой почтовый индекс в строке 81502
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

    #region Загрузка XML-файлов

    #region Основной метод

    /// <summary>
    /// Загрузка каталога с DBF-файлами
    /// </summary>
    /// <param name="dir">Каталог</param>
    /// <param name="actualDate">Дата актуальности обновления</param>
    /// <returns>Количество DBF-файлов, которые были обработаны.
    /// Ненужные для загрузки файлы не учитываются</returns>
    public int LoadXmlDir(AbsPath dir, DateTime actualDate)
    {
      if (!Directory.Exists(dir.Path))
        throw new FileNotFoundException("Не найден каталог \"" + dir.Path + "\"");

      int cnt = 0;

      AbsPath filePath;
      string oldPT = Splash.PhaseText;

      using (DBxConBase con = CreateCon())
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBUpdate starting " + (_PrimaryLoad ? " load " : " update ") + " from XML...");
        Trace.WriteLine("  XML dir=" + dir.Path.ToString());
        Trace.WriteLine("  Connection string=" + FiasDB.UpdateEntry.UnpasswordedConnectionString);

        BeginUpdate(con, actualDate, FiasDBUpdateSource.Xml);

        #region Добавление данных

        filePath = GetXmlFilePath(dir, "AS_SOCRBASE");
        if (!filePath.IsEmpty)
        {
          Splash.PhaseText = filePath.FileName;
          DoLoadXmlSocrBase(con, filePath);
          Splash.PhaseText = oldPT;
          cnt++;
        }

        // В версии ФИАС от 26.12.2019 есть файл AS_ADDROBJ_20191226_9985b7cf-05ef-4113-8b40-1dcf76ce6363.XML
        // В версии ФИАС от 16.03.2020 есть файл AS_ADDRESS_OBJECTS_20200316_7d7eb721-53c7-4f43-9886-c02db9f3a18f.XML
        // Как хочешь, так и понимай.
        // Так как в ФИАС от 16.03.2020 нет файлов AS_DEL*, неизвестно, как называется файл удаленных адресов

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

        #region Удаление данных

        filePath = GetXmlFilePath(dir, "AS_DEL_ADDRESS_OBJECTS"); // на всякий случай
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

        Splash.PhaseText = "Запись в таблицу истории обновлений";
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
          throw new InvalidOperationException("В каталоге \"" + dir.Path + "\" имеется больше одного файла, соответствующего шаблону \"" + template + "\"");
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


                  #region Внутренний идентификатор в AddressObjects

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

                  #region Наименование адресного объекта

                  string OffName = DataTools.GetString(rdr.GetAttribute("OFFNAME"));
                  wrt["OFFNAME"] = OffName;

                  if (_FiasDB.InternalSettings.FTSMode == FiasFTSMode.FTS3)
                  {
                    cmdFTS3.Parameters[0].Value = FiasTools.PrepareForFTS(OffName);
                    Int32 NameId = DataTools.GetInt(cmdFTS3.ExecuteScalar());
                    wrt["NameId"] = NameId;
                  }

                  #endregion

                  #region Краткое наименование

                  int Level = DataTools.GetInt(rdr.GetAttribute("AOLEVEL"));
                  string scName = DataTools.GetString(rdr.GetAttribute("SHORTNAME"));
                  wrt["AOTypeId"] = FindOrAddAOType(Level, scName, String.Empty);

                  #endregion

                  #region Поля, записываемые по-другому

                  if (_FiasDB.DBSettings.UseHistory)
                  {
                    switch (DataTools.GetInt(rdr.GetAttribute("ACTSTATUS")))
                    {
                      case 1: wrt["Actual"] = true; break;
                      case 0: wrt["Actual"] = false; break;
                      default:
                        throw new BugException("Неизвестное значение ACTSTATUS=" + rdr.GetAttribute("ACTSTATUS"));
                    }

                    switch (DataTools.GetInt(rdr.GetAttribute("LIVESTATUS")))
                    {
                      case 1: wrt["Live"] = true; break;
                      case 0: wrt["Live"] = false; break;
                      default:
                        throw new BugException("Неизвестное значение LIVESTATUS=" + rdr.GetAttribute("LIVESTATUS"));
                    }
                  }

                  #endregion

                  #region Остальные поля

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

                #region Внутренний идентификатор в House

                wrt.SetGuid("HOUSEID", DataTools.GetGuid(rdr.GetAttribute("HOUSEID")));
                wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(rdr.GetAttribute("HOUSEGUID")));
                wrt.SetGuid("AOGUID", DataTools.GetGuid(rdr.GetAttribute("AOGUID")));

                #endregion

                #region Остальные поля

                string sHouseNum = rdr.GetAttribute("HOUSENUM");
                int nHouseNum = FiasTools.GetNumInt(ref sHouseNum);
                wrt["nHouseNum"] = nHouseNum; // 0 записывается
                wrt.SetString("HOUSENUM", sHouseNum);

                string sBuildNum = rdr.GetAttribute("BUILDNUM");
                int nBuildNum = FiasTools.GetNumInt(ref sBuildNum);
                wrt["nBuildNum"] = nBuildNum; // 0 записывается
                wrt.SetString("BUILDNUM", sBuildNum);

                string sStrucNum = rdr.GetAttribute("STRUCNUM");
                int nStrucNum = FiasTools.GetNumInt(ref sStrucNum);
                wrt["nStrucNum"] = nStrucNum; // 0 записывается
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


                #region Внутренний идентификатор в Room

                wrt.SetGuid("ROOMID", DataTools.GetGuid(rdr.GetAttribute("ROOMID")));
                if (_FiasDB.DBSettings.UseHistory)
                {
                  wrt.SetGuid("PREVID", DataTools.GetGuid(rdr.GetAttribute("PREVID")));
                  wrt.SetGuid("NEXTID", DataTools.GetGuid(rdr.GetAttribute("NEXTID")));
                }
                wrt.SetGuid("ROOMGUID", DataTools.GetGuid(rdr.GetAttribute("ROOMGUID")));
                wrt.SetGuid("HOUSEGUID", DataTools.GetGuid(rdr.GetAttribute("HOUSEGUID")));

                #endregion

                #region Поля, записываемые по-другому

                if (_FiasDB.DBSettings.UseHistory)
                {
                  switch (DataTools.GetInt(rdr.GetAttribute("LIVESTATUS")))
                  {
                    case 1: wrt["Live"] = true; break;
                    case 0: wrt["Live"] = false; break;
                    default:
                      throw new BugException("Неизвестное значение LIVESTATUS=" + rdr.GetAttribute("LIVESTATUS"));
                  }
                }

                #endregion

                #region Остальные поля

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
    /// Удаление записей из таблиц адресных объектов, домов, помещений
    /// </summary>
    /// <param name="con"></param>
    /// <param name="filePath">Путь к XML-файлу</param>
    /// <param name="elName">Имена тегов объектов</param>
    /// <param name="resTableName">Имя таблицы в базе данных</param>
    private void DoDelXml(DBxConBase con, AbsPath filePath, string elName, string resTableName)
    {
      BeginLoadFile(filePath);
      int cnt;

      using (ExtXmlReader rdr0 = new ExtXmlReader(filePath, Splash))
      {
        XmlReader rdr = rdr0.Reader;

        // Имя ключевого GUID-поля ("AOID")
        string PKName = FiasDB.DB.Struct.Tables[resTableName].PrimaryKey[0];
        List<Guid> DelIds = new List<Guid>();
        Splash.AllowCancel = true;
        while (rdr.Read())
        {
          if (rdr.NodeType == XmlNodeType.Element)
          {
            if (rdr.Name == elName)
            {
              rdr0.UpdateSplash();

              Guid g = DataTools.GetGuid(rdr.GetAttribute(PKName));
              DelIds.Add(g);

              if (DelIds.Count >= 500)
              {
                con.Delete(resTableName, new ValuesFilter(PKName, DelIds.ToArray()));
                DelIds.Clear();
              }
            }
          }
        }

        if (DelIds.Count > 0)
          con.Delete(resTableName, new ValuesFilter(PKName, DelIds.ToArray()));
        cnt = DelIds.Count;
      }

      EndLoadFile(filePath, cnt);
    }

    #endregion

    /// <summary>
    /// Читатель для XML-файла с поддержкой процентного индикатора
    /// </summary>
    private class ExtXmlReader : SimpleDisposableObject
    {
      // 03.01.2021
      // Можно использовать базовый класс без деструктора

      #region Конструктор и Dispose

      public ExtXmlReader(AbsPath path, ISplash splash)
      {
        _FileStream = new FileStream(path.Path, FileMode.Open);

        _Splash = splash;
        // XML-файл может быть несколько ГБ длиной.
        // Во избежание переполнения Int32, считаем в блоках по 64КБ.
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

      #region Свойства

      /// <summary>
      /// Поток для извлечения текущей позиции
      /// </summary>
      private Stream _FileStream;

      /// <summary>
      /// Основной объект
      /// </summary>
      public XmlReader Reader { get { return _Reader; } }
      private XmlReader _Reader;

      private ISplash _Splash;

      private int _UpdateSplashCounter;

      #endregion

      #region Обновление заставки

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
    /// Получение числового значения с перехватом ошибки.
    /// Исключение выводится в Trace.
    /// Возвращается 0.
    /// В таблице as_room*.xml классификатора от 16.03.2019 тожн есть нечисловой почтовый индекс.
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

    #region Загрузка каталога с XML или DBF

    /// <summary>
    /// Основной метод - загрузка данных
    /// </summary>
    /// <param name="dir">Каталог с файлами DBF или XML. Вложенные каталоги не просматриваются</param>
    /// <param name="actualDate">Дата актуальности обновления</param>
    /// <returns>Количество загруженных файлов</returns>
    public int LoadDir(AbsPath dir, DateTime actualDate)
    {
      if (dir.IsEmpty)
        throw new ArgumentException("Каталог не задан", "dir");

      Splash.PhaseText = "Определение наличия данных";

      if (!Directory.Exists(dir.Path))
        throw new FileNotFoundException("Не найден каталог \"" + dir.Path + "\"");

      string[] aXmlFiles = Directory.GetFiles(dir.Path, "*.xml", SearchOption.TopDirectoryOnly);
      string[] aDbfFiles = Directory.GetFiles(dir.Path, "*.dbf", SearchOption.TopDirectoryOnly);
      if (aXmlFiles.Length > 0 && aDbfFiles.Length == 0)
        return LoadXmlDir(dir, actualDate);
      if (aDbfFiles.Length > 0 && aXmlFiles.Length == 0)
        return LoadDbfDir(dir, actualDate);
      if (aXmlFiles.Length == 0 && aDbfFiles.Length == 0)
        return 0;

      throw new InvalidOperationException("В каталоге \"" + dir.Path + "\" присутствуют и XML и DBF-файлы");
    }

    #endregion

    #region Загрузка из архива

    /// <summary>
    /// Загрузка обновления из архива.
    /// Выполняет распаковку во временный каталог и вызывает LoadDir().
    /// Поддерживаемые форматы файлов архивов определяются классом FileCompressor
    /// </summary>
    /// <param name="path">Путь к файлу архива</param>
    /// <param name="actualDate">Дата актуальности обновления</param>
    /// <returns>Количество загруженных файлов из архива</returns>
    public int LoadArchiveFile(AbsPath path, DateTime actualDate)
    {
      if (path.IsEmpty)
        throw new ArgumentException("Файл не задан", "path");

      if (FileCompressor.GetArchiveTypeFromFileName(path) == FileComressorArchiveType.Unknown)
        throw new ArgumentException("Неправильное имя файла: " + path.FileName + ". Неподдерживаемый формат архива");

      Splash.PhaseText = "Распаковка архива " + path.FileName;
      Trace.WriteLine(_FileStartTime.ToString("G") + " Unpacking " + path.Path + " to the temp directory");

      if (!File.Exists(path.Path))
        throw new FileNotFoundException("Не найден файл \"" + path.Path + "\"");

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

    #region Диапазон STARTDATE - ENDDATE

    private DateTime _Today;

    private bool TestDateRange(DateTime startDate, DateTime endDate)
    {
      return startDate <= _Today && endDate >= _Today;
    }


    #endregion

    #region Таблицы идентификаторов

#if XXX
    /// <summary>
    /// Упреждающее заполнение таблиц "xxRecIds" и "xxGuids"
    /// </summary>
    /// <param name="srcTable">Таблица для извлечения идентификаторов</param>
    /// <param name="srcColNames"></param>
    /// <param name="con">Соединение с БД "fias"</param>
    /// <param name="resTableName"></param>
    /// <returns>Словарь идентификаторов</returns>
    private Dictionary<string, Int32> CreateForwardGuidRows(DataTable srcTable, string[] srcColNames, DBxCon con, string resTableName)
    {
    #region Находим все GUID'ы, используемые в таблице

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

      // Ключ - идентификатор AOID в ФИАСе
      // Значение - внутренний идентификатор записи в таблице "AddrOb"
      Dictionary<string, Int32> IdDict = new Dictionary<string, Int32>(GuidTable.Rows.Count);

      if (GuidTable.Rows.Count == 0)
        return IdDict; // никогда не будет

#endregion

    #region Поиск/добавление записей в базу данных

      Int32[] Ids = con.FindOrAddRecords(GuidTable);

#endregion

    #region Создаем словарь

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
        throw new BugException("В коллекции не найден GUID=\"" + Guid + "\" из поля \"" + columnName + "\"");
      return Id;
    }
#endif

    #endregion

    #region Буферизация таблицы AOType

    /// <summary>
    /// Буферизованная таблица типов адресных объектов.
    /// Сюда загружается полная таблицы "AOType" из классификатора методом LoadAOTypesTable().
    /// В процессе обработки внешних файлов сюда могут добавляться строки.
    /// Перед записью основных таблиц классификатора вызывается FlushAOTypesTable().
    /// Если в таблицу были добавлены строким
    /// </summary>
    private DataTable AOTypesTable;
    private int AOTypesTableRowCount;
    private Int32 AOTypesTableLastId1;
    private Int32 AOTypesTableLastId2;

    ///// <summary>
    ///// Идентификатор фиктивной записи в таблице AOType.
    ///// Используется для forward-строк в AddressObjects
    ///// </summary>
    //private Int32 DummyAOTypeId;

    private void LoadAOTypesTable(DBxConBase con)
    {
      if (AOTypesTable != null)
        return;

      //DummyAOTypeId = Con.FindOrAddRecord("AOType", new DBxColumns("LEVEL,SCNAME,SOCRNAME"), new object[] { 0, "?", "?" });

      AOTypesTable = con.FillSelect("AOType");
      DataTools.SetPrimaryKey(AOTypesTable, "Id");
      AOTypesTable.DefaultView.Sort = "LEVEL,SCNAME";

      AOTypesTableRowCount = AOTypesTable.Rows.Count;
      AOTypesTableLastId1 = DataTools.MaxInt(AOTypesTable, "Id", false) ?? 0;
      AOTypesTableLastId2 = AOTypesTableLastId1;
    }

    private void FlushAOTypesTable(DBxConBase con)
    {
      if (AOTypesTableRowCount == AOTypesTable.Rows.Count)
        return; // Нечего сбрасывать

      DataTable TempTable;
      using (DataView dv2 = new DataView(AOTypesTable))
      {
        dv2.RowFilter = "Id>" + AOTypesTableLastId1.ToString();
        TempTable = dv2.ToTable();
#if DEBUG
        int WantedRowCount = AOTypesTable.Rows.Count - AOTypesTableRowCount;
        if (WantedRowCount != TempTable.Rows.Count)
          throw new BugException("Неправильное число добавляемых записей в таблицу AOTypes: " +
            TempTable.Rows.Count.ToString() + ". Ожидалось: " + WantedRowCount.ToString());
#endif
      }

      con.AddRecords("AOType", TempTable);
      AOTypesTableRowCount = AOTypesTable.Rows.Count;
      AOTypesTableLastId1 = AOTypesTableLastId2;
    }

    /// <summary>
    /// Поиск или добавление записи в таблице AOType
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="scName">Сокращение</param>
    /// <param name="socrName">Полное наименование</param>
    /// <returns>Идентификатор найденной или созданной записи</returns>
    private Int32 FindOrAddAOType(int level, string scName, string socrName)
    {
      if (String.IsNullOrEmpty(scName))
        return 0;
      if (level <= 0)
        throw new ArgumentOutOfRangeException("level");

      int p = AOTypesTable.DefaultView.Find(new object[] { level, scName });
      DataRow Row;
      if (p >= 0)
      {
        // Существующее сокращение
        Row = AOTypesTable.DefaultView[p].Row;
        Int32 Id = DataTools.GetInt(Row, "Id");

        if ((!String.IsNullOrEmpty(socrName)) && Id < AOTypesTableLastId1)
        {
          if (socrName != DataTools.GetString(Row, "SOCRNAME"))
          {
            using (DBxCon Con = new DBxCon(FiasDB.DB.MainEntry))
            {
              Con.SetValue("AOType", Id, "SOCRNAME", socrName);
              Row["SOCRNAME"] = socrName;
            }
          }
        }
        return Id;
      }
      else
      {
        Row = AOTypesTable.NewRow();
        AOTypesTableLastId2++;
        Row["Id"] = AOTypesTableLastId2;
        Row["LEVEL"] = level;
        Row["SCNAME"] = scName;
        if (String.IsNullOrEmpty(socrName)) // неизвестное сокращение из основной таблицы объектов
          Row["SOCRNAME"] = scName;
        else
          Row["SOCRNAME"] = socrName;
        AOTypesTable.Rows.Add(Row);
        return AOTypesTableLastId2;
      }
    }

    #endregion

    #region Начало и окончание обновления

    private DateTime _ActualDate;

    private Int32 _UpdateId;

    /// <summary>
    /// Вызывается в начале обновления.
    /// Добавляет запись в таблицу "ClassifUpdate"
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
    /// Вызывается после успешного обновления.
    /// Изменяет запись в таблице "ClassifUpdate". Изменяет поля "ActualDate" и "UpdateTime" в таблице "ClassifInfo".
    /// 10.09.2020. Также обновляется статистика
    /// </summary>
    /// <param name="con"></param>
    private void EndUpdate(DBxConBase con)
    {
      DateTime FinishTime = DateTime.Now;
      con.SetValues("ClassifInfo", 1, new DBxColumns("ActualDate,UpdateTime"),
        new object[] { _ActualDate, FinishTime });
      con.SetValue("ClassifUpdate", _UpdateId, "FinishTime", FinishTime);

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

    #region Трассировка

    DateTime _FileStartTime;

    private void BeginLoadFile(AbsPath filePath)
    {
      _FileStartTime = DateTime.Now;
      FileInfo fi = new FileInfo(filePath.Path);
      Trace.WriteLine(_FileStartTime.ToString("G") + " Start loading from " + filePath.FileName+". FileSize="+FileTools.GetMBSizeText(fi.Length));
    }

    private void EndLoadFile(AbsPath filePath, int records)
    {
      DateTime EndTime = DateTime.Now;
      TimeSpan ts = EndTime - _FileStartTime;
      string txt = String.Empty;
      if (ts.TotalSeconds > 0.1)
      {
        double ops = (double)records / ts.TotalSeconds;
        txt = ". Records per sec=" + ops.ToString();
      }
      Trace.WriteLine(_FileStartTime.ToString("G") + " Finish loading from " + filePath.FileName + ", time=" + ts.ToString() + ", records=" + records.ToString() + txt);
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Открывает соединение для обновления
    /// </summary>
    /// <returns></returns>
    private DBxConBase CreateCon()
    {
      DBxConBase con = _FiasDB.UpdateEntry.CreateCon();
      if (con.TraceEnabled)
      {
        con.TraceEnabled = false; // иначе будет очень много записей, если логгирование включено
        Trace.WriteLine("  DBxConBase.TraceEnabled was set to false for updating connection");
        if (con is AgeyevAV.ExtDB.SQLite.SQLiteDBxCon)
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
      writerInfo.TransactionPulseRowCount = 100000; // SQLite может переполниться
      return writerInfo;
    }

    #endregion
  }
}
