// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Threading;
using FreeLibSet.Core;

namespace FreeLibSet.IO
{
  /// <summary>
  /// Создание Zip-файлов для составных документов Open Office, MS Ofiice-2007 и других.
  /// Для использования данного класса необходимо наличие загруженной сборки ICSharpCode.SharpZipLib.dll.
  /// Используйте свойство <see cref="ZipFileTools.ZipLibAvailable"/> для предварительной проверки наличия библиотеки.
  /// </summary>
  public class ZipFileCreator
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="zipFilePath">Путь к создаваемому архиву</param>
    public ZipFileCreator(string zipFilePath)
    {
      _ZipFilePath = zipFilePath;

      ValidateDefaultCodePage();
      _File = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(zipFilePath);
      _File.BeginUpdate();
    }

    [DebuggerStepThrough]
    private static void ValidateDefaultCodePage()
    {
      // 26.07.2022
      // в Linux'е может по умолчанию использоваться русский язык, но не установлена поддержка для кодовой страницы 866.
      // ZipConstants.DefaultCodePage по умолчанию инициализируется равным Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage,
      // при этом наличие поддержки для этой страницы не проверяется.
      // При попытке создания архива возникнет исключение NotSupportedException.
      // Заменяем на кодовую страницу ASCII, которой достаточно для создания файлов docx, ods и других форматов, т.к.
      // внутри архива используются только латинские буквы.

      bool defCodePagePresents;
      try
      {
        defCodePagePresents = Encoding.GetEncoding(ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage) != null;
      }
      catch
      {
        defCodePagePresents = false;
      }
      if (!defCodePagePresents)
        ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = Encoding.ASCII.CodePage;
    }

    /// <summary>
    /// Заканчивает создание архива и записывает изменения на диск
    /// </summary>
    public void Close()
    {
      _File.CommitUpdate();
      _File.Close();
      _File = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к файлу архива.
    /// Задается в конструкторе.
    /// </summary>
    public string ZipFilePath { get { return _ZipFilePath; } }
    private string _ZipFilePath;

    /// <summary>
    /// Внутренний объект библиотеки ZharpZipLib
    /// </summary>
    public ICSharpCode.SharpZipLib.Zip.ZipFile File { get { return _File; } }
    private ICSharpCode.SharpZipLib.Zip.ZipFile _File;

    #endregion

    #region Вложенные источники данных

    private class ByteDataSource : ICSharpCode.SharpZipLib.Zip.IStaticDataSource
    {
      #region Конструктор

      public ByteDataSource(byte[] data)
      {
        _Data = data;
      }

      #endregion

      #region Поля

      private readonly byte[] _Data;

      #endregion

      #region IDataSource Members

      /// <summary>
      /// Get a Stream for this IStaticDataSource
      /// </summary>
      /// <returns>Returns a <see cref="Stream"/></returns>
      public Stream GetSource()
      {
        return new MemoryStream(_Data);
      }

      #endregion
    }

    private class MemoryStreamDataSource : ICSharpCode.SharpZipLib.Zip.IStaticDataSource
    {
      #region Конструктор

      public MemoryStreamDataSource()
      {
        _Stream = new MemoryStream();
      }

      #endregion

      #region Поля

      public MemoryStream Stream { get { return _Stream; } }
      private MemoryStream _Stream;

      #endregion

      #region IDataSource Members

      /// <summary>
      /// Get a Stream for this IStaticDataSource
      /// </summary>
      /// <returns>Returns a <see cref="Stream"/></returns>
      public Stream GetSource()
      {
        _Stream.Position = 0;
        return _Stream;
      }

      #endregion
    }

    #endregion

    #region Добавление компонентов файла

    /// <summary>
    /// Добавление несжатого файла mimetype, предусмотренного стандартом Open Document
    /// Этот метод должен вызываться непосредственно после конструктора
    /// </summary>
    /// <param name="value"></param>
    public void AddMimeType(string value)
    {
      ByteDataSource ds = new ByteDataSource(Encoding.ASCII.GetBytes(value));
      _File.Add(ds, "mimetype", ICSharpCode.SharpZipLib.Zip.CompressionMethod.Stored);
    }

    /// <summary>
    /// Добавить XML-файл к архиву
    /// </summary>
    /// <param name="xmlFileName">Имя XML-файла, которое будет в архиве (а не путь к файлу на диске)</param>
    /// <param name="xmlDoc">XML-документ, который будет записан в поток</param>
    public void AddXmlFile(string xmlFileName, XmlDocument xmlDoc)
    {
      MemoryStreamDataSource ds = new MemoryStreamDataSource();
      xmlDoc.Save(ds.Stream);
      _File.Add(ds, xmlFileName);
    }

    #endregion

    #region Отладка
#if XXX

    public void DebugOutFiles()
    { 
      ProcessStartInfo psi=new ProcessStartInfo();
      psi.FileName="7za.exe";
      if (System.IO.Directory.Exists("C:\\TEMP\\XXX"))
        System.IO.Directory.Delete("C:\\TEMP\\XXX", true);
      System.IO.Directory.CreateDirectory("C:\\TEMP\\XXX");
      psi.WorkingDirectory = "C:\\TEMP\\XXX";
      psi.Arguments = "x " + ZipFilePath;
      Process prc=Process.Start(psi);
      prc.WaitForExit();
    }
#endif

    #endregion
  }

  /// <summary>
  /// Проверка доступности библиотек, необходимых для работы с архивами
  /// </summary>
  public static class ZipFileTools
  {
    #region Наличие библиотеки SharpZipLib

    /// <summary>
    /// Возвращает true, если библиотека ICSharpCode.SharpZipLib.dll загружена
    /// и можно создавать объекты <see cref="ZipFileCreator"/>.
    /// </summary>
    public static bool ZipLibAvailable
    {
      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      get
      {
        lock (DataTools.InternalSyncRoot)
        {
          if (!_ZipLibAvailable.HasValue)
          {
            try
            {
              TryTestZipFile();
              _ZipLibAvailable = true;
            }
            catch
            {
              _ZipLibAvailable = false;
            }
          }

          return _ZipLibAvailable.Value;
        }
      }
    }
    private static bool? _ZipLibAvailable = null;

    /// <summary>
    /// Это должно быть в отдельном методе, т.к. оно может не запускаться
    /// </summary>
    private static void TryTestZipFile()
    {
      Type dummy = typeof(ICSharpCode.SharpZipLib.Zip.ZipFile);
    }

    /// <summary>
    /// Выбрасывает исключение, если <see cref="ZipLibAvailable"/>=false.
    /// </summary>
    public static void CheckZipLibAvailable()
    {
      if (!ZipLibAvailable)
        throw new DllNotFoundException("Не удалось загрузить библиотеку ICSharpCode.SharpZipLib.dll. Без нее невозможно создание сжатых zip-файлов");
    }

    #endregion

    #region Наличие библиотеки SevenZipSharp

    /// <summary>
    /// Возвращает true, если библиотека SevenZipSharp.dll загружена и можно работать с форматом архива 7z.
    /// </summary>
    public static bool SevenZipSharpAvailable
    {
      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      get
      {
        lock (DataTools.InternalSyncRoot)
        {
          if (_SevenZipSharpAvailabilityError == null)
          {
            try
            {
              TrySevenZipSharp();
              _SevenZipSharpAvailabilityError = String.Empty;
            }
            catch (Exception e)
            {
              _SevenZipSharpAvailabilityError = e.Message;
            }
          }

          return _SevenZipSharpAvailabilityError.Length == 0;
        }
      }
    }

    /// <summary>
    /// Возвращает сообщение об ошибке, если <see cref="SevenZipSharpAvailable"/> возвращает false.
    /// В прикладном коде обычно следует использовать метод <see cref="CheckSevenZipSharpAvailable()"/>.
    /// </summary>
    public static string SevenZipSharpAvailabilityError
    {
      get
      {
        bool dummy = SevenZipSharpAvailable;
        return _SevenZipSharpAvailabilityError;
      }
    }

    /// <summary>
    /// Если null, то проверка еще не выполнялась.
    /// Если пустая строка, то библиотека присутствует.
    /// Если непустая строка, то содержит текст сообщения об ошибке
    /// </summary>
    private static string _SevenZipSharpAvailabilityError = null;

    /// <summary>
    /// Это должно быть в отдельном методе, т.к. он может не запускаться
    /// </summary>
    private static void TrySevenZipSharp()
    {
      Type someType = typeof(SevenZip.SevenZipCompressor);

      // Инициализация библиотеки 7z.dll
      string fileName = (IntPtr.Size == 8) ? "7z64.dll" : "7z.dll";

      try
      {
        AbsPath rootDir = new AbsPath(someType.Assembly.GetName().CodeBase).ParentDir;
        if (File.Exists((rootDir + fileName).Path))
        {
          // ReSharper disable once AccessToStaticMemberViaDerivedType
          SevenZip.SevenZipCompressor.SetLibraryPath((rootDir + fileName).Path);
          return;
        }
      }
      catch { }

      try
      {
        AbsPath rootDir = FileTools.ApplicationBaseDir;
        if (File.Exists((rootDir + fileName).Path))
        {
          // ReSharper disable once AccessToStaticMemberViaDerivedType
          SevenZip.SevenZipCompressor.SetLibraryPath((rootDir + fileName).Path);
          return;
        }
      }
      catch { }

      throw new FileNotFoundException("Не найден файл " + fileName);
    }

    /// <summary>
    /// Выбрасывает исключение, если <see cref="SevenZipSharpAvailable"/>=false.
    /// </summary>
    public static void CheckSevenZipSharpAvailable()
    {
      if (!SevenZipSharpAvailable)
        throw new DllNotFoundException("Не найдена библиотека SevenZipSharp.dll для работы с архивами 7z. " + SevenZipSharpAvailabilityError); // ??
    }

    #endregion

    #region Распаковка Zip

    /// <summary>
    /// Распаковка ресурса, упакованного в Zip-формате, в указанный путь.
    /// Предполагается, что архив содержит единственный файл, который требуется распаковать в каталог с указанным именем.
    /// Каталог <paramref name="resFilePath"/>.Parent должен существовать.
    /// Исходное имя файла в архиве игнорируется.
    /// </summary>
    /// <param name="zipFileBytes">Массив байтов, содержащих zip-архив</param>
    /// <param name="resFilePath">Путь к сохраняемому файлу на диске</param>
    public static void ExtractZipResourceFile(byte[] zipFileBytes, AbsPath resFilePath)
    {
      if (resFilePath.IsEmpty)
        throw new ArgumentNullException("resFilePath");

      CheckZipLibAvailable();

      int cnt = 0;
      using (MemoryStream zipFileStream = new MemoryStream(zipFileBytes))
      {
        ICSharpCode.SharpZipLib.Zip.ZipInputStream zis = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(zipFileStream);
        ICSharpCode.SharpZipLib.Zip.ZipEntry ze;
        while ((ze = zis.GetNextEntry()) != null)
        {
          if (ze.IsFile)
          {
            if (cnt == 0)
              FileTools.WriteStream(resFilePath, zis);
            else
              throw new ArgumentException("Архив содержит несколько файлов");
            cnt++;
          }
        }
        if (cnt == 0)
          throw new ArgumentException("Архив не содержит файлов");
      }
    }

    #endregion
  }

  #region Перечисление

  /// <summary>
  /// Формат архива для FileComressor
  /// </summary>
  public enum FileComressorArchiveType
  {
    /// <summary>
    /// Автоматическон определение типа архива.
    /// При сжатии тип определяется по имени файла архива.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Формат 7z
    /// </summary>
    SevenZip = 1,

    /// <summary>
    /// Формат Zip
    /// </summary>
    Zip = 2,

    /// <summary>
    /// Это значение возвращается методом FileCompressor.GetArchiveTypeFromFileName(), если файл имеет неизвестное расширение
    /// </summary>
    Unknown = 0,
  }

  #endregion

  /// <summary>
  /// Сжатие / распаковка файлов в форматах 7z и zip.
  /// Надстройка над библиотекой SharpZipLib
  /// Указывается имя архива, каталог с файлами и другие параметры (зависит от действия).
  /// Затем вызывается один из методов Compress(), Decomress() или TestArchive()
  /// Поддержка пароля, многотомных архивов и процентного индикатора
  /// В процессе работы создаются дополнительные потоки
  /// </summary>
  public class FileCompressor
  {
    #region Конструктор

    /// <summary>
    /// Создает неинициализированный объект
    /// </summary>
    public FileCompressor()
    {
      _ArchiveType = FileComressorArchiveType.Auto;
      _FileTemplates = new FileTemplateList();
      _UseSplashPercent = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Полный путь к файлу архива.
    /// Требуется для всех действий
    /// Для многотомных архивов задается имя первого файла
    /// </summary>
    public AbsPath ArchiveFileName { get { return _ArchiveFileName; } set { _ArchiveFileName = value; } }
    private AbsPath _ArchiveFileName;

    /// <summary>
    /// Каталог с исходными файлами для упаковки / распаковки
    /// Требуется для Compress() и Decompress()
    /// </summary>
    public AbsPath FileDirectory { get { return _FileDirectory; } set { _FileDirectory = value; } }
    private AbsPath _FileDirectory;

    /// <summary>
    /// Необязательный массив шаблонов файлов или отдельных имен. Могут задаваться подкаталоги относительно FileDirectory. 
    /// Если список пустой, то предполагаются все файлы в каталоге (Compress) или архиве (Decompress)
    /// Шаблоны могут задавать подкаталоги
    /// </summary>
    public FileTemplateList FileTemplates { get { return _FileTemplates; } }
    private FileTemplateList _FileTemplates;

    /// <summary>
    /// Тип архива. Используется только при создании архива.
    /// По умолчанию - Auto - тип архива определяется по расширени файла архива (.7z, .zip)
    /// Свойство должно быть установлено, если используется нестандартное расширение, например, .docx
    /// </summary>
    public FileComressorArchiveType ArchiveType { get { return _ArchiveType; } set { _ArchiveType = value; } }
    private FileComressorArchiveType _ArchiveType;

    /// <summary>
    /// Размер многотомного архива при упаковке. По умолчанию - 0 - не использовать разбиение на тома
    /// В остальных режимах многотомные архивы распознаются автоматически
    /// Распаковка многотомных архивов с дискет с предложением замены дискеты не поддерживается
    /// Многотомные архивы поддерживаются только для 7z
    /// </summary>
    public int VolumeSize { get { return _VolumeSize; } set { _VolumeSize = value; } }
    private int _VolumeSize;

    /// <summary>
    /// Пароль на архив при упаковке, распаковке и тестировании архива
    /// </summary>
    public string Password { get { return _Password; } set { _Password = value; } }
    private string _Password;

    /// <summary>
    /// Внешняя заставка. Если свойство установлено, то выводится процентный индикатор.
    /// Рекомендуется обязательное использование заставки, когда размер архива может быть большим или заранее неизвестен.
    /// Без заставки действие нельзя прервать.
    /// Если процентный индикатор должен управляться внешним кодом, а не FileCompressor, установите свойство <see cref="UseSplashPercent"/>=false.
    /// </summary>
    public ISplash Splash { get { return _Splash; } set { _Splash = value; } }
    private ISplash _Splash;

    /// <summary>
    /// Если true (по умолчанию), то в заставке <see cref="Splash"/> будет устанавливаться процентный индикатор для отображения прогресса.
    /// Если процентный индикатор должен управляться вызывающим кодом или не использоваться, установите свойство в false.
    /// Свойство действует только при установленном свойстве <see cref="Splash"/>.
    /// </summary>
    public bool UseSplashPercent { get { return _UseSplashPercent; } set { _UseSplashPercent = value; } }
    private bool _UseSplashPercent;

    #endregion

    #region Создание архива

    /// <summary>
    /// Базовый класс для AsyncCompress и AsyncDecompress
    /// Отдельный поток для запуска архивации/распаковки нужен, т.к. события от 7z.dll могут приходить асинхронно, а не из того потока, откуда
    /// архивация запущена. Взаимодействие с заставкой Splash должно быть из одного потока. Требуется промежуточная буферизация положения индикатора
    /// </summary>
    private class AsyncBase
    {
      #region Поля для связи между потоками

      /*
       * При обращении к этим полям выполняется блокировка объекта AsyncBase
       */

      /// <summary>
      /// Устанавливается в true, когда поток архивации завершается (успешно или с ошибкой)
      /// </summary>
      public bool IntFinished;

      #region Для обмена с процентным индикатором

      public int IntPercent;
      public string IntPhaseText;

      // Проблема.
      // Прерывание ппроцесса сжатия / распаковки возможно только при переходе к следующему файлу
      // Когда пользователь устанавливает ISplash.Cancelled=true, устанавливается IntCancelled=true,
      // а у самого индикатора свойство ISplash.Cancelled сбрасывается обратно в false, вместе с AllowCancel
      // В текущую фазу добавляет текст о необходимости подождать

      public bool IntCancelled;

      #endregion

      #endregion

      #region Прочие поля

      public FileCompressor Owner;

      /// <summary>
      /// Если в потоке архивации возникает ошибка, исключение сохраняется здесь, чтобы его можно было перевыбросить из основного потока
      /// </summary>
      public Exception Exception;

      #endregion

      #region Обработчики

      protected void ProgressHandler(object sender, SevenZip.ProgressEventArgs args)
      {
        lock (this)
        {
          IntPercent = args.PercentDone;
          // Это не работает 
          //Args.Cancel = IntCancelled;
        }
      }

      protected void FileStartedHandler(SevenZip.FileNameEventArgs args, string actionTitle)
      {
        lock (this)
        {
          if (IntCancelled)
          {
            args.Cancel = true;
            IntPhaseText = "Работа с архивом сейчас будет прервана";
          }
          else
            IntPhaseText = actionTitle + " " + Path.GetFileName(args.FileName);
        }
      }

      protected void FileStartedHandler(SevenZip.FileInfoEventArgs args, string actionTitle)
      {
        lock (this)
        {
          if (IntCancelled)
          {
            args.Cancel = true;
            IntPhaseText = "Работа с архивом сейчас будет прервана";
          }
          else
            IntPhaseText = actionTitle + " " + Path.GetFileName(args.FileInfo.FileName);
        }
      }

      #endregion
    }

    /// <summary>
    /// Реализация потока для сжатия
    /// </summary>
    private class AsyncCompress : AsyncBase
    {
      #region Методы, выполняющиеся асинхронно

      /// <summary>
      /// Этот метод вызывается в отдельном потоке
      /// </summary>
      public void CompressThreadProc()
      {
        try
        {
          SevenZip.SevenZipCompressor cmp = new SevenZip.SevenZipCompressor();
          switch (Owner.ArchiveType)
          {
            case FileComressorArchiveType.SevenZip:
              cmp.ArchiveFormat = SevenZip.OutArchiveFormat.SevenZip;
              break;
            case FileComressorArchiveType.Zip:
              cmp.ArchiveFormat = SevenZip.OutArchiveFormat.Zip;
              break;
            case FileComressorArchiveType.Unknown:
              throw new InvalidOperationException("Не удалось определить тип архива по имени файла");
            default:
              throw new InvalidOperationException("Неизвестный формат архива: " + Owner.ArchiveType.ToString());
          }

          cmp.Compressing += new EventHandler<SevenZip.ProgressEventArgs>(ProgressHandler);
          cmp.FileCompressionStarted += new EventHandler<SevenZip.FileNameEventArgs>(cmp_FileCompressionStarted);

          cmp.VolumeSize = Owner.VolumeSize;

          if (Owner.FileTemplates.Count == 0)
            cmp.CompressDirectory(Owner.FileDirectory.Path, Owner.ArchiveFileName.Path, Owner.Password, "*.*", false);
          else if (Owner.FileTemplates.Count == 1 && (!Owner.FileTemplates[0].HasSubDir))
            cmp.CompressDirectory(Owner.FileDirectory.Path, Owner.ArchiveFileName.Path, Owner.Password, Owner.FileTemplates[0].Template, Owner.FileTemplates[0].Recurse);
          else
          {
            lock (this)
            {
              IntPhaseText = "Построение списка файлов";
            }

            ISplash spl = null;
            if (Owner.UseSplashPercent)
              spl = Owner.Splash;
            string[] aFiles = Owner.FileTemplates.GetAbsFileNames(Owner.FileDirectory, spl);
            cmp.CompressFilesEncrypted(Owner.ArchiveFileName.Path, Owner.FileDirectory.SlashedPath.Length, Owner.Password, aFiles);
          }

        }
        catch (Exception e)
        {
          Exception = e;
        }
        lock (this)
        {
          IntFinished = true;
        }
      }

      void cmp_FileCompressionStarted(object sender, SevenZip.FileNameEventArgs args)
      {
        base.FileStartedHandler(args, "Сжатие");
      }

      #endregion
    }

    /// <summary>
    /// Выполнить архивацию файлов
    /// Перед запуском должны быть заданы свойства ArchiveFileName и FileDirectory. 
    /// Также могут быть заданы FileTemplates, ArchiveType, VolumeSize, Password и Splash
    /// </summary>
    public void Compress()
    {
      ZipFileTools.CheckSevenZipSharpAvailable();

      if (ArchiveFileName.IsEmpty)
        throw new NullReferenceException("Не задано имя файла архива");
      if (FileDirectory.IsEmpty)
        throw new NullReferenceException("Не задано имя каталога сжимаемых файлов");
      if (!Directory.Exists(FileDirectory.Path))
        throw new FileNotFoundException("Каталог сжимаемых файлов \"" + FileDirectory.Path + "\" не найден");

      if (ArchiveType == FileComressorArchiveType.Auto)
        ArchiveType = GetArchiveTypeFromFileName(ArchiveFileName);


      AsyncCompress cmp = new AsyncCompress();
      ProcessArchiveOperation(cmp, new ThreadStart(cmp.CompressThreadProc), "Compressing " + ArchiveFileName.FileName);
    }

    private void ProcessArchiveOperation(AsyncBase asyncBase, ThreadStart threadMethod, string threadTitle)
    {
      bool oldAllowCancel = false;
      string oldPhaseText = null;

      if (Splash != null)
      {
        oldAllowCancel = Splash.AllowCancel;
        oldPhaseText = Splash.PhaseText;

        if (UseSplashPercent)
          Splash.PercentMax = 100;
        Splash.AllowCancel = true;
      }

      asyncBase.Owner = this;
      asyncBase.IntFinished = false;

      Thread subThread = new Thread(threadMethod);
      subThread.Name = threadTitle;
      subThread.Start();
      Thread.Sleep(50);

      while (true)
      {
        lock (asyncBase)
        {
          if (asyncBase.IntFinished)
            break;

          if (Splash != null)
          {
            try
            {
              if (asyncBase.IntCancelled)
                Splash.PhaseText = asyncBase.IntPhaseText + ". Прерывание будет выполнено при обработке следующего файла";
              else
                Splash.PhaseText = asyncBase.IntPhaseText;
              if (UseSplashPercent)
                Splash.Percent = asyncBase.IntPercent;
              Splash.CheckCancelled();
            }
            catch
            {
              asyncBase.IntCancelled = true;
              try
              {
                Splash.AllowCancel = false; // сразу отменяем
                if (UseSplashPercent)
                  Splash.Percent = asyncBase.IntPercent;
              }
              catch
              {
                // На всякий случай. Вдруг кнопка "отмена" нажата прямо сейчас
              }
            }
          }
        }

        if (Splash == null)
          Thread.Sleep(100);
        else
          Splash.Sleep(100);
      }

      if (Splash != null)
      {
        if (asyncBase.IntCancelled)
          throw new UserCancelException();

        if (UseSplashPercent)
          Splash.PercentMax = 0;
        Splash.AllowCancel = oldAllowCancel;
        Splash.PhaseText = oldPhaseText;
      }

      // Блокировка объекта не требуется, т.к. поток архивации уже завершен
      if (asyncBase.Exception != null)
        throw asyncBase.Exception;
    }

    /// <summary>
    /// Получить тип архива из имени файла.
    /// Реальное наличие файла и его содержимое не проверяется.
    /// </summary>
    /// <param name="archiveFileName">Имя файла архива</param>
    /// <returns>Тип архива</returns>
    public static FileComressorArchiveType GetArchiveTypeFromFileName(AbsPath archiveFileName)
    {
      string fileExt = System.IO.Path.GetExtension(archiveFileName.FileName).ToUpperInvariant();
      switch (fileExt)
      {
        case ".7Z": return FileComressorArchiveType.SevenZip;
        case ".ZIP": return FileComressorArchiveType.Zip;
        default: return FileComressorArchiveType.Unknown;
      }
    }

    #endregion

    #region Распаковка

    private class AsyncDecompress : AsyncBase
    {
      #region Свойства

      /// <summary>
      /// Результат проверки целостности архива
      /// </summary>
      public bool TestResult;

      #endregion

      #region Методы, выполняющиеся асинхронно

      private string ArchiveFileNamePath
      {
        get
        {
          if (File.Exists(Owner.ArchiveFileName.Path))
            return Owner.ArchiveFileName.Path;
          if (File.Exists(Owner.ArchiveFileName.Path + ".001"))
            return Owner.ArchiveFileName.Path + ".001";
          throw new FileNotFoundException("Не найден файл архива \"Owner.ArchiveFileName.Path\" или первый файл многотомного архива", Owner.ArchiveFileName.Path);
        }
      }

      /// <summary>
      /// Этот метод вызывается в отдельном потоке
      /// </summary>
      public void DecompressThreadProc()
      {
        try
        {
          SevenZip.SevenZipExtractor ext;
          if (String.IsNullOrEmpty(Owner.Password))
            ext = new SevenZip.SevenZipExtractor(ArchiveFileNamePath);
          else
            ext = new SevenZip.SevenZipExtractor(ArchiveFileNamePath, Owner.Password);
          try
          {
            ext.Extracting += new EventHandler<SevenZip.ProgressEventArgs>(ProgressHandler);
            ext.FileExtractionStarted += new EventHandler<SevenZip.FileInfoEventArgs>(ext_FileExtractionStarted);

            if (Owner.FileTemplates.Count == 0)
              ext.ExtractArchive(Owner.FileDirectory.Path);
            else
            {
              IntPhaseText = "Получение списка файлов";
              ICollection<string> AllFiles = ext.ArchiveFileNames;
              List<string> UsedFiles = new List<string>();
              foreach (string FileName in AllFiles)
              {
                if (TestFileRelName(FileName))
                  UsedFiles.Add(FileName);
              }

              ext.ExtractFiles(Owner.FileDirectory.Path, UsedFiles.ToArray());
            }
          }
          finally
          {
            ext.Dispose();
          }
        }
        catch (Exception e)
        {
          Exception = e;
        }
        lock (this)
        {
          IntFinished = true;
        }
      }

      private bool TestFileRelName(string fileName)
      {
        for (int i = 0; i < Owner.FileTemplates.Count; i++)
        {
          if (FileTools.TestRelFileNameWildcards(fileName, Owner.FileTemplates[i].Template))
            return true;
        }
        return false;
      }

      public void TestThreadProc()
      {
        try
        {
          SevenZip.SevenZipExtractor test;
          if (String.IsNullOrEmpty(Owner.Password))
            test = new SevenZip.SevenZipExtractor(ArchiveFileNamePath);
          else
            test = new SevenZip.SevenZipExtractor(ArchiveFileNamePath, Owner.Password);
          try
          {
            test.Extracting += new EventHandler<SevenZip.ProgressEventArgs>(ProgressHandler);
            test.FileExtractionStarted += new EventHandler<SevenZip.FileInfoEventArgs>(test_FileExtractionStarted);

            TestResult = test.Check();
          }
          finally
          {
            test.Dispose();
          }
        }
        catch (Exception e)
        {
          Exception = e;
        }
        lock (this)
        {
          IntFinished = true;
        }
      }

      void ext_FileExtractionStarted(object sender, SevenZip.FileInfoEventArgs args)
      {
        base.FileStartedHandler(args, "Распаковка");
      }

      void test_FileExtractionStarted(object sender, SevenZip.FileInfoEventArgs args)
      {
        base.FileStartedHandler(args, "Проверка");
      }

      #endregion
    }

    /// <summary>
    /// Выполнить извлечение файлов из архива
    /// Перед запуском должны быть заданы свойства ArchiveFileName и FileDirectory. 
    /// Также могут быть заданы FileTemplates, Password и Splash
    /// </summary>
    public void Decompress()
    {
      ZipFileTools.CheckSevenZipSharpAvailable();

      TestArchiveFileExists();
      if (FileDirectory.IsEmpty)
        throw new NullReferenceException("Не задано имя каталога для распаковки");
      FileTools.ForceDirs(FileDirectory);


      AsyncDecompress Async = new AsyncDecompress();
      ProcessArchiveOperation(Async, new ThreadStart(Async.DecompressThreadProc), "Extracting " + ArchiveFileName.FileName);
    }

    private void TestArchiveFileExists()
    {
      if (ArchiveFileName.IsEmpty)
        throw new NullReferenceException("Не задано имя файла архива");

      string sPath1 = ArchiveFileName.Path;
      string sPath2 = ArchiveFileName.Path + ".001"; // 17.12.2014
      if (File.Exists(sPath1) || File.Exists(sPath2))
        return;
      throw new FileNotFoundException("Архив \"" + ArchiveFileName.Path + "\" не найден");
    }

    #endregion

    #region Тестирование архива

    /// <summary>
    /// Выполнить проверку целостности архива
    /// Перед запуском должно быть задано свойство ArchiveFileName
    /// Также могут быть заданы Password и Splash
    /// </summary>
    public bool TestArchive()
    {
      ZipFileTools.CheckSevenZipSharpAvailable();
      TestArchiveFileExists();

      AsyncDecompress decomp = new AsyncDecompress();
      ProcessArchiveOperation(decomp, new ThreadStart(decomp.TestThreadProc), "Testing " + ArchiveFileName.FileName);

      return decomp.TestResult;
    }

    #endregion
  }

}
