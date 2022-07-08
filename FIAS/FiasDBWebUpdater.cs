// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Отвечает за периодическое обновление классификатора, Метод PerformUpdate() вызывается приложением сервера по таймеру.
  /// Проверяет наличие обновлений с помощью WEB-службы. Выполняет распаковку полученных обновлений и вызывает методы FiasDBUpdater для их загрузки
  /// </summary>
  public sealed class FiasDBWebUpdater
  {
    #region Конструктор

    /// <summary>
    /// Создает экземпляр объекта
    /// </summary>
    /// <param name="fiasDB">База данных ФИАС</param>
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

    #region Свойства

    /// <summary>
    /// База данных ФИАС. Задается в конструкторе
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
    /// Счетчик установленных обновлений
    /// </summary>
    public int UpdateCount { get { return _UpdateCount; } }
    private int _UpdateCount;

    #endregion

    #region Основной метод

    /// <summary>
    /// Выполнить поиск и загрузку недостающих обновлений, исходя из даты актуальности FiasDB.ActualDate.
    /// </summary>
    public void PerformUpdate()
    {
      string oldPT = Splash.PhaseText;
      DoPerformUpdate();
      Splash.PhaseText = oldPT;
    }

    private void DoPerformUpdate()
    {
      Splash.PhaseText = "Получение списка обновлений";

      Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. Start loading update list from " + FiasWebLoader.HttpBase);

      DataTable table = FiasWebLoader.GetUpdateTable();
      table.DefaultView.Sort = "Date";
      if (table.Rows.Count == 0)
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. List is empty");
        return;
      }

      DateTime firstDate = (DateTime)(table.DefaultView[0].Row["Date"]);
      DateTime lastDate = (DateTime)(table.DefaultView[table.DefaultView.Count - 1].Row["Date"]);
      Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. List contains " + table.DefaultView.Count.ToString() + " update(s) from " +
        firstDate.ToString("d") + " to " + lastDate.ToString("d") + ". The actual update date is " + _FiasDB.ActualDate.ToString("d"));

      if (firstDate.Year < 2011)
        throw new BugException("Неправильная первая дата в списке обновлений: " + firstDate.ToString("d"));

      if (firstDate > _FiasDB.ActualDate)
        throw new BugException("Первая дата в списке обновлений : " + firstDate.ToString("d") + " больше актуальной даты классификатора " +
          _FiasDB.ActualDate.ToString("d"));

      if (lastDate <= _FiasDB.ActualDate)
      {
        Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. There are no suitable updates");
        return; // Нет свежих обновлений
      }

      using (FiasDBUpdater updater = new FiasDBUpdater(_FiasDB))
      {
        updater.Splash = Splash;

        // Перебираем обновления
        foreach (DataRowView drv in table.DefaultView)
        {
          DateTime dt = (DateTime)(drv.Row["Date"]);
          if (dt <= _FiasDB.ActualDate)
            continue;

          // Нужно поставить обновление
          string uri = drv.Row["FiasDeltaXmlUrl"].ToString();
          if (uri.Length == 0)
            continue;
          if (uri[0] == '0')
            continue;

          Splash.PhaseText = "Загрузка файла обновления от " + dt.ToString("d") + " из " + uri;
          Splash.AllowCancel = true;
          Splash.CheckCancelled();

          using (TempDirectory td = new TempDirectory())
          {
            AbsPath path = new AbsPath(td.Dir, FiasWebLoader.GetFileNameFromUrl(uri));
            WebClient client = new WebClient();
            DateTime singleStartTime = DateTime.Now;
            Trace.WriteLine(singleStartTime.ToString("G") + " FiasDBWebUpdater. Start loading from " + uri + " ...");
            client.DownloadFile(uri, path.Path);
            Splash.CheckCancelled();
            Splash.AllowCancel = false;
            if (!File.Exists(path.Path))
              throw new FileNotFoundException("Не найден файл, скачанный с сайта ФИАС", path.Path);

            FileInfo fi = new FileInfo(path.Path);
            TimeSpan downloadTime = DateTime.Now - singleStartTime;
            Trace.WriteLine(DateTime.Now.ToString("G") + " FiasDBWebUpdater. FileSize=" + (fi.Length / FileTools.MByte) + "MB. Download time=" + downloadTime.TotalMinutes.ToString("0") + " minute(s). Start updating DB ...");
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
  /// Загрузка обновлений классификатора без их установки
  /// </summary>
  public sealed class FiasWebLoader
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, инициализируя свойства
    /// </summary>
    /// <param name="dir">Каталог, куда должны сохраняться обновления. Должен быть задан</param>
    /// <param name="actualDate">Дата актуальности</param>
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

    #region Свойства

    /// <summary>
    /// Каталог для записи файлов. Задается в конструкторе.
    /// В нем будут созданы подкаталоги с именами в формате ГГГГММДД.
    /// В каждом подкаталоге будет по одному архиву обновления.
    /// </summary>
    public AbsPath Dir { get { return _Dir; } }
    private AbsPath _Dir;

    /// <summary>
    /// Дата актуальности.
    /// Будут загружаться только более новый обновления.
    /// Задается в конструкторе
    /// </summary>
    public DateTime ActualDate { get { return _ActualDate; } }
    private DateTime _ActualDate;

    /// <summary>
    /// Формат обновлений. По умолчанию - XML
    /// </summary>
    public FiasDBUpdateSource Format { get { return _Format; } set { _Format = value; } }
    private FiasDBUpdateSource _Format;

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

    #endregion

    #region Загрузка файлов обновлений

    /// <summary>
    /// Пути к скачанным файлам обновлениям.
    /// Свойство действительно после вызова метода DownloadFiles()
    /// </summary>
    public AbsPath[] Files { get { return _Files; } }
    private AbsPath[] _Files;

    /// <summary>
    /// Количество скачанных файлов
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
    /// Загрузка файлов обновлений в указанный каталог
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
      Splash.PhaseText = "Получение списка обновлений";

      DataTable table = GetUpdateTable();
      table.DefaultView.Sort = "Date";
      if (table.Rows.Count == 0)
        return;

      DateTime firstDate = (DateTime)(table.DefaultView[0].Row["Date"]);
      DateTime lastDate = (DateTime)(table.DefaultView[table.DefaultView.Count - 1].Row["Date"]);

      if (firstDate.Year < 2011)
        throw new BugException("Неправильная первая дата в списке обновлений: " + firstDate.ToString("d"));

      if (firstDate > ActualDate)
        return;

      if (lastDate <= ActualDate)
        return; // Нет свежих обновлений

      // Перебираем обновления
      Splash.AllowCancel = true;
      foreach (DataRowView drv in table.DefaultView)
      {
        DateTime dt = (DateTime)(drv.Row["Date"]);
        if (dt <= ActualDate)
          continue;

        // Нужно поставить обновление
        string uri = drv.Row[Format == FiasDBUpdateSource.Xml ? "FiasDeltaXmlUrl" : "FiasDeltaDbfUrl"].ToString();
        if (uri.Length == 0)
          continue;
        if (uri[0] == '0')
          continue;

        Splash.PhaseText = "Загрузка файла обновления от " + dt.ToString("d") + " из " + uri;
        Splash.CheckCancelled();

        string fileName = GetFileNameFromUrl(uri);

        AbsPath path = new AbsPath(Dir, dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture.DateTimeFormat), fileName);
        FileTools.ForceDirs(path.ParentDir);
        WebClient client = new WebClient();
        client.DownloadFile(uri, path.Path);
        Splash.CheckCancelled();
        if (!File.Exists(path.Path))
          throw new FileNotFoundException("Не найден файл, скачанный с сайта ФИАС", path.Path);
      }
    }

    #endregion

    #region Получение списка обновлений

    private class Record
    {
      #region Поля

#pragma warning disable 649 // Field is never assigned to, and will always have its default value.
      // Поля устанавливаются с помощью Reflection

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
    /// Получить список обновлений в виде таблицы DataTable.
    /// Строки таблицы могут быть неотсортированными, так как их порядок определятся сайтом ФИАС.
    /// Выполните сортировку по полю Date.
    /// </summary>
    /// <returns>Таблица данных</returns>
    public static DataTable GetUpdateTable()
    {
      string sFile = LoadJsonFile();

      DataTable table = new DataTable();
      // Список тегов JSON-файла GetAllDownloadFileInfo взят по состоянию на 20.02.2021
      // Он может и поменяться.
      table.Columns.Add("Date", typeof(DateTime));
      //table.Columns.Add("VersionId", typeof(string));
      table.Columns.Add("TextVersion", typeof(string));
      table.Columns.Add("FiasCompleteDbfUrl", typeof(string));
      table.Columns.Add("FiasCompleteXmlUrl", typeof(string));
      table.Columns.Add("FiasDeltaDbfUrl", typeof(string));
      table.Columns.Add("FiasDeltaXmlUrl", typeof(string));
      table.Columns.Add("GarXMLFullURL", typeof(string));
      table.Columns.Add("GarXMLDeltaURL", typeof(string));

      // Для обработки JSON-файла используем JSon.NET
      // https://www.newtonsoft.com/json

      // Коротко о JSON-формате:
      // В квадратных скобках идет массив
      // В фигургных скобках идет объект
      // В объекте идут свойства в виде "Имя"=Значение

      // Наш файл является массивом

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

    #region Дополнительно

    /// <summary>
    /// Возвращает имя арихива файла с расширением из url для загрузки файла.
    /// </summary>
    /// <param name="uri">Запрос</param>
    /// <returns></returns>
    public static string GetFileNameFromUrl(string uri)
    {
      // Запрос имеет вид "https://fias.nalog.ru/DownloadUpdates?file=fias_delta_dbf.zip&version=20210223"


      if (String.IsNullOrEmpty(uri))
        throw new ArgumentNullException("uri");
      try
      {
        int p = uri.IndexOf('?');
        if (p < 0)
        {
          // throw new ArgumentException("Не найден символ \"?\"");
          // 21.03.2021
          // Теперь путь имеет вид: "https://fias.nalog.ru/downloads/2021.02.26/fias_delta_xml.zip"
          // Достали они со своими изменениями...
          p = uri.LastIndexOf('/');
          if (p < 0)
            throw new ArgumentException("Не найдены символы \"?\" и \"/\"");
          string sFileName = uri.Substring(p + 1);
          if (sFileName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Имя файла \"" + sFileName + "\" содержит недопустимые символы");
          return sFileName;
        }
        string[] a = uri.Substring(p + 1).Split('&');
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i].StartsWith("file=", StringComparison.OrdinalIgnoreCase))
            return a[i].Substring(5);
        }
        throw new ArgumentException("Не найден параметр file");
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


