using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using System.IO;
using System.Xml;
using FreeLibSet.IO;
using Newtonsoft.Json;
using System.Globalization;
using System.Diagnostics;
using FreeLibSet.Core;
//using FreeLibSet.FIAS.WebService;


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

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// �������� �� ������������� ���������� ��������������, ����� PerformUpdate() ���������� ����������� ������� �� �������.
  /// ��������� ������� ���������� � ������� WEB-������. ��������� ���������� ���������� ���������� � �������� ������ FiasDBUpdater ��� �� ��������
  /// </summary>
  public sealed class FiasDBWebUpdater
  {
    #region �����������

    /// <summary>
    /// ������� ��������� �������
    /// </summary>
    /// <param name="fiasDB">���� ������ ����</param>
    public FiasDBWebUpdater(FiasDB fiasDB)
    {
#if DEBUG
      if (fiasDB == null)
        throw new ArgumentNullException("fiasDB");
#endif

      _FiasDB = fiasDB;
      _Splash = new DummySplash();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� ������ ����. �������� � ������������
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
    /// ������� ������������� ����������
    /// </summary>
    public int UpdateCount { get { return _UpdateCount; } }
    private int _UpdateCount;

    #endregion

    #region �������� �����

    /// <summary>
    /// ��������� ����� � �������� ����������� ����������, ������ �� ���� ������������ FiasDB.ActualDate.
    /// </summary>
    public void PerformUpdate()
    {
      string oldPT = Splash.PhaseText;
      DoPerformUpdate();
      Splash.PhaseText = oldPT;
    }

    private void DoPerformUpdate()
    {
      Splash.PhaseText = "��������� ������ ����������";

      Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. Start loading update list from " + FiasWebLoader.HttpBase);

      DataTable table = FiasWebLoader.GetUpdateTable();
      table.DefaultView.Sort = "Date";
      if (table.Rows.Count == 0)
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. List is empty");
        return;
      }

      DateTime FirstDate = (DateTime)(table.DefaultView[0].Row["Date"]);
      DateTime LastDate = (DateTime)(table.DefaultView[table.DefaultView.Count - 1].Row["Date"]);
      Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. List contains " + table.DefaultView.Count.ToString() + " update(s) from " +
        FirstDate.ToString("d") + " to " + LastDate.ToString("d") + ". The actual update date is " + _FiasDB.ActualDate.ToString("d"));

      if (FirstDate.Year < 2011)
        throw new BugException("������������ ������ ���� � ������ ����������: " + FirstDate.ToString("d"));

      if (FirstDate > _FiasDB.ActualDate)
        throw new BugException("������ ���� � ������ ���������� : " + FirstDate.ToString("d") + " ������ ���������� ���� �������������� " +
          _FiasDB.ActualDate.ToString("d"));

      if (LastDate <= _FiasDB.ActualDate)
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. There are no suitable updates");
        return; // ��� ������ ����������
      }

      using (FiasDBUpdater updater = new FiasDBUpdater(_FiasDB))
      {
        updater.Splash = Splash;

        // ���������� ����������
        foreach (DataRowView drv in table.DefaultView)
        {
          DateTime dt = (DateTime)(drv.Row["Date"]);
          if (dt <= _FiasDB.ActualDate)
            continue;

          // ����� ��������� ����������
          string uri = drv.Row["FiasDeltaXmlUrl"].ToString();
          if (uri.Length == 0)
            continue;
          if (uri[0] == '0')
            continue;

          Splash.PhaseText = "�������� ����� ���������� �� " + dt.ToString("d") + " �� " + uri;
          Splash.AllowCancel = true;
          Splash.CheckCancelled();

          using (TempDirectory td = new TempDirectory())
          {
            AbsPath path = new AbsPath(td.Dir, FiasWebLoader.GetFileNameFromUrl(uri));
            WebClient client = new WebClient();
            DateTime SingleStartTime = DateTime.Now;
            Trace.WriteLine(SingleStartTime.ToString("G") + " FiasDBWebUpdater. Start loading from " + uri + " ...");
            client.DownloadFile(uri, path.Path);
            Splash.CheckCancelled();
            Splash.AllowCancel = false;
            if (!File.Exists(path.Path))
              throw new FileNotFoundException("�� ������ ����, ��������� � ����� ����", path.Path);

            FileInfo fi = new FileInfo(path.Path);
            TimeSpan DownloadTime = DateTime.Now - SingleStartTime;
            Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. FileSize=" + (fi.Length / FileTools.MByte) + "MB. Download time=" + DownloadTime.TotalMinutes.ToString("0") + " minute(s). Start updating DB ...");
            updater.LoadArchiveFile(path, dt);
          }

          _UpdateCount++;
          Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. Update no " + _UpdateCount.ToString() + " from " + dt.ToString("d") + " succesfully installed");
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// �������� ���������� �������������� ��� �� ���������
  /// </summary>
  public sealed class FiasWebLoader
  {
    #region �����������

    /// <summary>
    /// ������� ������, ������������� ��������
    /// </summary>
    /// <param name="dir">�������, ���� ������ ����������� ����������. ������ ���� �����</param>
    /// <param name="actualDate">���� ������������</param>
    public FiasWebLoader(AbsPath dir, DateTime actualDate)
    {
      if (dir.IsEmpty)
        throw new ArgumentNullException("dir");
      _Dir = dir;

      _ActualDate = actualDate;

      _Format = FiasDBUpdateSource.Xml;
      _Splash = new DummySplash();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ��� ������ ������. �������� � ������������.
    /// � ��� ����� ������� ����������� � ������� � ������� ��������.
    /// � ������ ����������� ����� �� ������ ������ ����������.
    /// </summary>
    public AbsPath Dir { get { return _Dir; } }
    private AbsPath _Dir;

    /// <summary>
    /// ���� ������������.
    /// ����� ����������� ������ ����� ����� ����������.
    /// �������� � ������������
    /// </summary>
    public DateTime ActualDate { get { return _ActualDate; } }
    private DateTime _ActualDate;

    /// <summary>
    /// ������ ����������. �� ��������� - XML
    /// </summary>
    public FiasDBUpdateSource Format { get { return _Format; } set { _Format = value; } }
    private FiasDBUpdateSource _Format;

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

    #endregion

    #region �������� ������ ����������

    /// <summary>
    /// ���� � ��������� ������ �����������.
    /// �������� ������������� ����� ������ ������ DownloadFiles()
    /// </summary>
    public AbsPath[] Files { get { return _Files; } }
    private AbsPath[] _Files;

    /// <summary>
    /// ���������� ��������� ������
    /// </summary>
    public int FileCount
    {
      get
      {
        if (_Files == null)
          return 0;
        else
          return _Files.Length;
      }
    }

    /// <summary>
    /// �������� ������ ���������� � ��������� �������
    /// </summary>
    public void DownloadFiles()
    {
      List<AbsPath> files = new List<AbsPath>();
      try
      {
        DoDownloadFiles(files);
      }
      finally
      {
        _Files = files.ToArray();
      }
    }

    private void DoDownloadFiles(List<AbsPath> files)
    {
      Splash.PhaseText = "��������� ������ ����������";

      DataTable table = GetUpdateTable();
      table.DefaultView.Sort = "Date";
      if (table.Rows.Count == 0)
        return;

      DateTime FirstDate = (DateTime)(table.DefaultView[0].Row["Date"]);
      DateTime LastDate = (DateTime)(table.DefaultView[table.DefaultView.Count - 1].Row["Date"]);

      if (FirstDate.Year < 2011)
        throw new BugException("������������ ������ ���� � ������ ����������: " + FirstDate.ToString("d"));

      if (FirstDate > ActualDate)
        return;

      if (LastDate <= ActualDate)
        return; // ��� ������ ����������

      // ���������� ����������
      Splash.AllowCancel = true;
      foreach (DataRowView drv in table.DefaultView)
      {
        DateTime dt = (DateTime)(drv.Row["Date"]);
        if (dt <= ActualDate)
          continue;

        // ����� ��������� ����������
        string uri = drv.Row[Format == FiasDBUpdateSource.Xml ? "FiasDeltaXmlUrl" : "FiasDeltaDbfUrl"].ToString();
        if (uri.Length == 0)
          continue;
        if (uri[0] == '0')
          continue;

        Splash.PhaseText = "�������� ����� ���������� �� " + dt.ToString("d") + " �� " + uri;
        Splash.CheckCancelled();

        string fileName = GetFileNameFromUrl(uri);

        AbsPath path = new AbsPath(Dir, dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture.DateTimeFormat), fileName);
        FileTools.ForceDirs(path.ParentDir);
        WebClient client = new WebClient();
        client.DownloadFile(uri, path.Path);
        Splash.CheckCancelled();
        if (!File.Exists(path.Path))
          throw new FileNotFoundException("�� ������ ����, ��������� � ����� ����", path.Path);
      }
    }

    #endregion

    #region ��������� ������ ����������

    private class Record
    {
      #region ����

#pragma warning disable 649 // Field is never assigned to, and will always have its default value.
      // ���� ��������������� � ������� Reflection

      public string Date;
      public string TextVersion;
      public string FiasCompleteDbfUrl;
      public string FiasCompleteXmlUrl;
      public string FiasDeltaDbfUrl;
      public string FiasDeltaXmlUrl;
      public string GarXMLFullURL;
      public string GarXMLDeltaURL;

#pragma warning disable 649

      #endregion
    }

    /// <summary>
    /// �������� ������ ���������� � ���� ������� DataTable.
    /// ������ ������� ����� ���� ������������������, ��� ��� �� ������� ����������� ������ ����.
    /// ��������� ���������� �� ���� Date.
    /// </summary>
    /// <returns>������� ������</returns>
    public static DataTable GetUpdateTable()
    {
      string sFile = LoadJsonFile();

      DataTable table = new DataTable();
      // ������ ����� JSON-����� GetAllDownloadFileInfo ���� �� ��������� �� 20.02.2021
      // �� ����� � ����������.
      table.Columns.Add("Date", typeof(DateTime));
      //table.Columns.Add("VersionId", typeof(string));
      table.Columns.Add("TextVersion", typeof(string));
      table.Columns.Add("FiasCompleteDbfUrl", typeof(string));
      table.Columns.Add("FiasCompleteXmlUrl", typeof(string));
      table.Columns.Add("FiasDeltaDbfUrl", typeof(string));
      table.Columns.Add("FiasDeltaXmlUrl", typeof(string));
      table.Columns.Add("GarXMLFullURL", typeof(string));
      table.Columns.Add("GarXMLDeltaURL", typeof(string));

      // ��� ��������� JSON-����� ���������� JSon.NET
      // https://www.newtonsoft.com/json

      // ������� � JSON-�������:
      // � ���������� ������� ���� ������
      // � ��������� ������� ���� ������
      // � ������� ���� �������� � ���� "���"=��������

      // ��� ���� �������� ��������

      Record[] a = JsonConvert.DeserializeObject<Record[]>(sFile);

      for (int i = 0; i < a.Length; i++)
      {
        DateTime dt = DateTime.ParseExact(a[i].Date, @"dd\.MM\.yyyy", CultureInfo.InvariantCulture);
        table.Rows.Add(dt, a[i].TextVersion,
          a[i].FiasCompleteDbfUrl, a[i].FiasCompleteXmlUrl,
          a[i].FiasDeltaDbfUrl, a[i].FiasDeltaXmlUrl,
          a[i].GarXMLFullURL, a[i].GarXMLDeltaURL);
      }

      return table;
    }


    internal const string HttpBase = @"http://fias.nalog.ru/WebServices/Public/";

    private static string LoadJsonFile()
    {
      WebClient client = new WebClient();
      byte[] b = client.DownloadData(HttpBase + "GetAllDownloadFileInfo");
      return Encoding.UTF8.GetString(b);
    }

    #endregion

    #region �������������

    /// <summary>
    /// ���������� ��� ������� ����� � ����������� �� url ��� �������� �����.
    /// </summary>
    /// <param name="uri">������</param>
    /// <returns></returns>
    public static string GetFileNameFromUrl(string uri)
    {
      // ������ ����� ��� "https://fias.nalog.ru/DownloadUpdates?file=fias_delta_dbf.zip&version=20210223"


      if (String.IsNullOrEmpty(uri))
        throw new ArgumentNullException("uri");
      try
      {
        int p = uri.IndexOf('?');
        if (p < 0)
        {
          // throw new ArgumentException("�� ������ ������ \"?\"");
          // 21.03.2021
          // ������ ���� ����� ���: "https://fias.nalog.ru/downloads/2021.02.26/fias_delta_xml.zip"
          // ������� ��� �� ������ �����������...
          p = uri.LastIndexOf('/');
          if (p < 0)
            throw new ArgumentException("�� ������� ������� \"?\" � \"/\"");
          string sFileName = uri.Substring(p + 1);
          if (sFileName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("��� ����� \"" + sFileName + "\" �������� ������������ �������");
          return sFileName;
        }
        string[] a = uri.Substring(p + 1).Split('&');
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i].StartsWith("file=", StringComparison.OrdinalIgnoreCase))
            return a[i].Substring(5);
        }
        throw new ArgumentException("�� ������ �������� file");
      }
      catch (Exception e)
      {
        e.Data["uri"] = uri;
        throw;
      }
    }


    #endregion
  }
}


