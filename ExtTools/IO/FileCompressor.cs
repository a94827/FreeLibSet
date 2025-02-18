// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using FreeLibSet.Core;

namespace FreeLibSet.IO
{
  #region Перечисление FileComressorArchiveType

  /// <summary>
  /// Формат архива для <see cref="FileCompressor"/>
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
    /// Это значение возвращается методом <see cref="FileCompressor.GetArchiveTypeFromFileName(AbsPath)"/>, если файл имеет неизвестное расширение
    /// </summary>
    Unknown = 0,
  }

  #endregion

  /// <summary>
  /// Сжатие / распаковка файлов в форматах 7z и zip.
  /// Надстройка над библиотекой SevenZipSharp
  /// Указывается имя архива, каталог с файлами и другие параметры (зависит от действия).
  /// Затем вызывается один из методов Compress(), Decomress() или TestArchive().
  /// Поддержка пароля, многотомных архивов и процентного индикатора.
  /// В процессе работы создаются дополнительные потоки.
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
    /// Требуется для всех действий.
    /// Для многотомных архивов задается имя первого файла.
    /// </summary>
    public AbsPath ArchiveFileName { get { return _ArchiveFileName; } set { _ArchiveFileName = value; } }
    private AbsPath _ArchiveFileName;

    /// <summary>
    /// Каталог с исходными файлами для упаковки / распаковки.
    /// Требуется для <see cref="Compress()"/> и <see cref="Decompress()"/>
    /// </summary>
    public AbsPath FileDirectory { get { return _FileDirectory; } set { _FileDirectory = value; } }
    private AbsPath _FileDirectory;

    /// <summary>
    /// Необязательный массив шаблонов файлов или отдельных имен. Могут задаваться подкаталоги относительно <see cref="FileDirectory"/>. 
    /// Если список пустой, то предполагаются все файлы в каталоге (<see cref="Compress()"/>) или архиве (<see cref="Decompress()"/>).
    /// Шаблоны могут задавать подкаталоги.
    /// </summary>
    public FileTemplateList FileTemplates { get { return _FileTemplates; } }
    private readonly FileTemplateList _FileTemplates;

    /// <summary>
    /// Тип архива. Используется только при создании архива.
    /// По умолчанию - Auto - тип архива определяется по расширени файла архива (.7z, .zip)
    /// Свойство должно быть установлено, если используется нестандартное расширение, например, .docx
    /// </summary>
    public FileComressorArchiveType ArchiveType { get { return _ArchiveType; } set { _ArchiveType = value; } }
    private FileComressorArchiveType _ArchiveType;

    /// <summary>
    /// Размер многотомного архива при упаковке. По умолчанию - 0 - не использовать разбиение на тома.
    /// В остальных режимах многотомные архивы распознаются автоматически.
    /// Распаковка многотомных архивов с дискет с предложением замены дискеты не поддерживается.
    /// Многотомные архивы поддерживаются только для формата 7z.
    /// </summary>
    public int VolumeSize { get { return _VolumeSize; } set { _VolumeSize = value; } }
    private int _VolumeSize;

    /// <summary>
    /// Пароль на архив при упаковке, распаковке и тестировании архива.
    /// </summary>
    public string Password { get { return _Password; } set { _Password = value; } }
    private string _Password;

    /// <summary>
    /// Внешняя заставка. Если свойство установлено, то выводится процентный индикатор.
    /// Рекомендуется обязательное использование заставки, когда размер архива может быть большим или заранее неизвестен.
    /// Без заставки действие нельзя прервать.
    /// Если процентный индикатор должен управляться внешним кодом, а не <see cref="FileCompressor"/>, установите свойство <see cref="UseSplashPercent"/>=false.
    /// </summary>
    public ISimpleSplash Splash { get { return _Splash; } set { _Splash = value; } }
    private ISimpleSplash _Splash;

    /// <summary>
    /// Если true (по умолчанию), то в заставке <see cref="Splash"/> будет устанавливаться процентный индикатор для отображения прогресса.
    /// Если процентный индикатор должен управляться вызывающим кодом или не использоваться, установите свойство в false.
    /// Свойство действует только при установленном свойстве <see cref="Splash"/>.
    /// </summary>
    public bool UseSplashPercent { get { return _UseSplashPercent; } set { _UseSplashPercent = value; } }
    private bool _UseSplashPercent;

    #endregion

    #region Интерфейс выполнения действия

    private interface IArchiveHandler
    {
      #region Методы

      void Compress(FileCompressor caller);
      void Decompress(FileCompressor caller);
      bool TestArchive(FileCompressor caller);

      #endregion
    }

    /// <summary>
    /// Задать принудительное использование командной строки вместо SevenZipSharp.dll.
    /// Свойство игнорируется в Linux или при невозможнсти загрузить SevenZipSharp.dll.
    /// 
    /// Свойство преднозначено исключительно для целей тестирования и не должно использоваться в прикланом коде.
    /// </summary>
    public bool DebugForceCommandLine { get { return _DebugForceCommandLine; } set { _DebugForceCommandLine=value; } }
    private bool _DebugForceCommandLine;

    private IArchiveHandler CreateHandler()
    {
      switch (Environment.OSVersion.Platform)
      {
      case PlatformID.Win32NT:
      case PlatformID.Win32Windows:
        if (DebugForceCommandLine)
          return CreateCommandLineWindows();
        if (ZipFileTools.SevenZipSharpAvailable)
          return new SevenZipSharpHandler();
        return CreateCommandLineWindows();
      case PlatformID.Unix:
        return CreateCommandLineLinux();
      default:
        throw new NotImplementedException();
      }
    }

    private IArchiveHandler CreateCommandLineWindows()
    {
      AbsPath path;
      path = FindExeFile("7z.exe");
      if (!path.IsEmpty)
        return new CmdLine7zArchiveHandler(path.FileName);
      path = FindExeFile("7za.exe");
      if (!path.IsEmpty)
        return new CmdLine7zArchiveHandler(path.FileName);
      throw new FileNotFoundException(String.Format(Res.FileCompressor_Err_ArchiverNotFound, "7z.exe"));
    }

    private IArchiveHandler CreateCommandLineLinux()
    {
      AbsPath path;
      path = FindExeFile("7z");
      if (!path.IsEmpty)
        return new CmdLine7zArchiveHandler(path.FileName);
      throw new FileNotFoundException(String.Format(Res.FileCompressor_Err_ArchiverNotFound, "7z"));
    }

    private static AbsPath FindExeFile(string fileName)
    {
      try
      {
        AbsPath path1 = new AbsPath(FileTools.ApplicationBaseDir, fileName);
        if (System.IO.File.Exists(path1.Path))
          return path1;
      }
      catch { }

      return FileTools.FindExecutableFilePath(fileName);
    }

    #endregion

    #region Public-методы

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

    /// <summary>
    /// Выполнить архивацию файлов
    /// Перед запуском должны быть заданы свойства <see cref="ArchiveFileName"/> и <see cref="FileDirectory"/>. 
    /// Также могут быть заданы <see cref="FileTemplates"/>, <see cref="ArchiveType"/>, <see cref="VolumeSize"/>, <see cref="Password"/> и <see cref="Splash"/>.
    /// </summary>
    public void Compress()
    {
      if (ArchiveFileName.IsEmpty)
        throw new NullReferenceException(Res.FileCompressor_Err_ArchiveFileNameIsEmpty);
      if (FileDirectory.IsEmpty)
        throw new NullReferenceException(Res.FileCompressor_Err_SrcDirNameIsEmpty);
      if (!Directory.Exists(FileDirectory.Path))
        throw new FileNotFoundException(String.Format(Res.FileCompressor_Err_SrcDirNotFound, FileDirectory.Path));

      if (ArchiveType == FileComressorArchiveType.Auto)
        ArchiveType = GetArchiveTypeFromFileName(ArchiveFileName);

      CreateHandler().Compress(this);
    }

    /// <summary>
    /// Выполнить извлечение файлов из архива
    /// Перед запуском должны быть заданы свойства <see cref="ArchiveFileName"/> и <see cref="FileDirectory"/>. 
    /// Также могут быть заданы <see cref="FileTemplates"/>, <see cref="Password"/> и <see cref="Splash"/>.
    /// </summary>
    public void Decompress()
    {
      TestArchiveFileExists();
      if (FileDirectory.IsEmpty)
        throw new NullReferenceException(Res.FileCompressor_Err_DstDirNameIsEmpty);
      FileTools.ForceDirs(FileDirectory);

      CreateHandler().Decompress(this);
    }

    /// <summary>
    /// Выполнить проверку целостности архива
    /// Перед запуском должно быть задано свойство <see cref="ArchiveFileName"/>
    /// Также могут быть заданы <see cref="Password"/> и <see cref="Splash"/>.
    /// </summary>
    public bool TestArchive()
    {
      TestArchiveFileExists();
      return CreateHandler().TestArchive(this);
    }

    private void TestArchiveFileExists()
    {
      if (ArchiveFileName.IsEmpty)
        throw new NullReferenceException(Res.FileCompressor_Err_ArchiveFileNameIsEmpty);

      string sPath1 = ArchiveFileName.Path;
      string sPath2 = ArchiveFileName.Path + ".001"; // 17.12.2014
      if (File.Exists(sPath1) || File.Exists(sPath2))
        return;
      throw new FileNotFoundException(String.Format(Res.FileCompressor_Err_ArchiveNotFound, ArchiveFileName.Path));
    }

    #endregion

    #region Реализация для SevenZipSharp.dll

    private class SevenZipSharpHandler : IArchiveHandler
    {
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
        // Прерывание процесса сжатия / распаковки возможно только при переходе к следующему файлу
        // Когда пользователь устанавливает ISplash.Cancelled=true, устанавливается IntCancelled=true,
        // а у самого индикатора свойство ISplash.Cancelled сбрасывается обратно в false, вместе с AllowCancel
        // В текущую фазу добавляет текст о необходимости подождать

        public bool IntCancelled;

        #endregion

        #endregion

        #region Прочие поля

        public FileCompressor Caller;

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
              IntPhaseText = Res.FileCompressor_Phase_Cancelling;
            }
            else
              IntPhaseText = String.Format(Res.FileCompressor_Phase_FileAction, actionTitle, Path.GetFileName(args.FileName));
          }
        }

        protected void FileStartedHandler(SevenZip.FileInfoEventArgs args, string actionTitle)
        {
          lock (this)
          {
            if (IntCancelled)
            {
              args.Cancel = true;
              IntPhaseText = Res.FileCompressor_Phase_Cancelling;
            }
            else
              IntPhaseText = String.Format(Res.FileCompressor_Phase_FileAction, actionTitle, Path.GetFileName(args.FileInfo.FileName));
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
            switch (Caller.ArchiveType)
            {
              case FileComressorArchiveType.SevenZip:
                cmp.ArchiveFormat = SevenZip.OutArchiveFormat.SevenZip;
                break;
              case FileComressorArchiveType.Zip:
                cmp.ArchiveFormat = SevenZip.OutArchiveFormat.Zip;
                break;
              case FileComressorArchiveType.Unknown:
                throw new InvalidOperationException(Res.FileCompressor_Err_UnknownFileExtension);
              default:
                throw new BugException("Unknown archive format: " + Caller.ArchiveType.ToString());
            }

            cmp.Compressing += new EventHandler<SevenZip.ProgressEventArgs>(ProgressHandler);
            cmp.FileCompressionStarted += new EventHandler<SevenZip.FileNameEventArgs>(cmp_FileCompressionStarted);

            cmp.VolumeSize = Caller.VolumeSize;

            if (Caller.FileTemplates.Count == 0)
              cmp.CompressDirectory(Caller.FileDirectory.Path, Caller.ArchiveFileName.Path, Caller.Password, "*.*", false);
            else if (Caller.FileTemplates.Count == 1 && (!Caller.FileTemplates[0].HasSubDir))
              cmp.CompressDirectory(Caller.FileDirectory.Path, Caller.ArchiveFileName.Path, Caller.Password, Caller.FileTemplates[0].Template, Caller.FileTemplates[0].Recurse);
            else
            {
              lock (this)
              {
                IntPhaseText = Res.FileCompressor_Phase_FileList;
              }

              ISimpleSplash spl = null;
              if (Caller.UseSplashPercent)
                spl = Caller.Splash;
              string[] aFiles = Caller.FileTemplates.GetAbsFileNames(Caller.FileDirectory, spl);
              cmp.CompressFilesEncrypted(Caller.ArchiveFileName.Path, Caller.FileDirectory.SlashedPath.Length, Caller.Password, aFiles);
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
          base.FileStartedHandler(args, Res.FileCompressor_Action_Compress);
        }

        #endregion
      }

      /// <summary>
      /// Выполнить архивацию файлов
      /// Перед запуском должны быть заданы свойства <see cref="ArchiveFileName"/> и <see cref="FileDirectory"/>. 
      /// Также могут быть заданы <see cref="FileTemplates"/>, <see cref="ArchiveType"/>, <see cref="VolumeSize"/>, <see cref="Password"/> и <see cref="Splash"/>.
      /// </summary>
      public void Compress(FileCompressor caller)
      {
        AsyncCompress cmp = new AsyncCompress();
        ProcessArchiveOperation(cmp, new ThreadStart(cmp.CompressThreadProc), "Compressing " + caller.ArchiveFileName.FileName, caller);
      }

      private void ProcessArchiveOperation(AsyncBase asyncBase, ThreadStart threadMethod, string threadTitle, FileCompressor caller)
      {
        bool oldAllowCancel = false;
        string oldPhaseText = null;

        if (caller.Splash != null)
        {
          oldAllowCancel = caller.Splash.AllowCancel;
          oldPhaseText = caller.Splash.PhaseText;

          if (caller.UseSplashPercent)
            caller.Splash.PercentMax = 100;
          caller.Splash.AllowCancel = true;
        }

        asyncBase.Caller = caller;
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

            if (caller.Splash != null)
            {
              try
              {
                if (asyncBase.IntCancelled)
                  caller.Splash.PhaseText = String.Format(Res.FileCompression_Phase_WithCancellingOnNextFile, asyncBase.IntPhaseText);
                else
                  caller.Splash.PhaseText = asyncBase.IntPhaseText;
                if (caller.UseSplashPercent)
                  caller.Splash.Percent = asyncBase.IntPercent;
                caller.Splash.CheckCancelled();
              }
              catch
              {
                asyncBase.IntCancelled = true;
                try
                {
                  caller.Splash.AllowCancel = false; // сразу отменяем
                  if (caller.UseSplashPercent)
                    caller.Splash.Percent = asyncBase.IntPercent;
                }
                catch
                {
                  // На всякий случай. Вдруг кнопка "отмена" нажата прямо сейчас
                }
              }
            }
          }

          if (caller.Splash == null)
            Thread.Sleep(100);
          else
            caller.Splash.Sleep(100);
        }

        if (caller.Splash != null)
        {
          if (asyncBase.IntCancelled)
            throw new UserCancelException();

          if (caller.UseSplashPercent)
            caller.Splash.PercentMax = 0;
          caller.Splash.AllowCancel = oldAllowCancel;
          caller.Splash.PhaseText = oldPhaseText;
        }

        // Блокировка объекта не требуется, т.к. поток архивации уже завершен
        if (asyncBase.Exception != null)
          throw asyncBase.Exception;
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
            if (File.Exists(Caller.ArchiveFileName.Path))
              return Caller.ArchiveFileName.Path;
            if (File.Exists(Caller.ArchiveFileName.Path + ".001"))
              return Caller.ArchiveFileName.Path + ".001";
            throw new FileNotFoundException(String.Format(Res.FileCompressor_Err_ArchiveAndVolume1NotFound, Caller.ArchiveFileName.Path));
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
            if (String.IsNullOrEmpty(Caller.Password))
              ext = new SevenZip.SevenZipExtractor(ArchiveFileNamePath);
            else
              ext = new SevenZip.SevenZipExtractor(ArchiveFileNamePath, Caller.Password);
            try
            {
              ext.Extracting += new EventHandler<SevenZip.ProgressEventArgs>(ProgressHandler);
              ext.FileExtractionStarted += new EventHandler<SevenZip.FileInfoEventArgs>(ext_FileExtractionStarted);

              if (Caller.FileTemplates.Count == 0)
                ext.ExtractArchive(Caller.FileDirectory.Path);
              else
              {
                IntPhaseText = Res.FileCompressor_Phase_FileList;
                ICollection<string> AllFiles = ext.ArchiveFileNames;
                List<string> UsedFiles = new List<string>();
                foreach (string FileName in AllFiles)
                {
                  if (TestFileRelName(FileName))
                    UsedFiles.Add(FileName);
                }

                ext.ExtractFiles(Caller.FileDirectory.Path, UsedFiles.ToArray());
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
          for (int i = 0; i < Caller.FileTemplates.Count; i++)
          {
            if (FileTools.TestRelFileNameWildcards(fileName, Caller.FileTemplates[i].Template))
              return true;
          }
          return false;
        }

        public void TestThreadProc()
        {
          try
          {
            SevenZip.SevenZipExtractor test;
            if (String.IsNullOrEmpty(Caller.Password))
              test = new SevenZip.SevenZipExtractor(ArchiveFileNamePath);
            else
              test = new SevenZip.SevenZipExtractor(ArchiveFileNamePath, Caller.Password);
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
          base.FileStartedHandler(args, Res.FileCompressor_Action_Decompress);
        }

        void test_FileExtractionStarted(object sender, SevenZip.FileInfoEventArgs args)
        {
          base.FileStartedHandler(args, Res.FileCompressor_Action_Test);
        }

        #endregion
      }

      /// <summary>
      /// Выполнить извлечение файлов из архива
      /// Перед запуском должны быть заданы свойства <see cref="ArchiveFileName"/> и <see cref="FileDirectory"/>. 
      /// Также могут быть заданы <see cref="FileTemplates"/>, <see cref="Password"/> и <see cref="Splash"/>.
      /// </summary>
      public void Decompress(FileCompressor caller)
      {

        AsyncDecompress Async = new AsyncDecompress();
        ProcessArchiveOperation(Async, new ThreadStart(Async.DecompressThreadProc), "Extracting " + caller.ArchiveFileName.FileName, caller);
      }

      #endregion

      #region Тестирование архива

      /// <summary>
      /// Выполнить проверку целостности архива
      /// Перед запуском должно быть задано свойство <see cref="ArchiveFileName"/>
      /// Также могут быть заданы <see cref="Password"/> и <see cref="Splash"/>.
      /// </summary>
      public bool TestArchive(FileCompressor caller)
      {

        AsyncDecompress decomp = new AsyncDecompress();
        ProcessArchiveOperation(decomp, new ThreadStart(decomp.TestThreadProc), "Testing " + caller.ArchiveFileName.FileName, caller);

        return decomp.TestResult;
      }

      #endregion
    }
    #endregion

    #region Реализация для утилиты командной строки 7z

    private class CmdLine7zArchiveHandler : IArchiveHandler
    {
      #region Конструктор

      public CmdLine7zArchiveHandler(string exeFileName)
      {
        _ExeFileName = exeFileName;
      }
      private readonly string _ExeFileName;

      #endregion

      public void Compress(FileCompressor caller)
      {
        // Если задано свойство FileTemplates, то в списке могут быть как рекурсивные, так и нерекурсивные маски.
        // Если есть и те и другие, то требуется вызывать команду дважды: с ключем "-r" и без него
        if (caller.FileTemplates.CountWithRecurse > 0 && caller.FileTemplates.CountWithoutRecurse > 0)
        {
          DoCompress(caller, false);
          DoCompress(caller, true);
        }
        else if (caller.FileTemplates.CountWithRecurse > 0)
          DoCompress(caller, true);
        else
          DoCompress(caller, false);

      }

      private void DoCompress(FileCompressor caller, bool recurse)
      {
        StringBuilder sbArgs = new StringBuilder();
        if (recurse)
          sbArgs.Append("-r ");

        switch (caller.ArchiveType)
        {
          case FileComressorArchiveType.SevenZip:
            sbArgs.Append("-t7z ");
            break;
          case FileComressorArchiveType.Zip:
            sbArgs.Append("-tzip ");
            break;
          case FileComressorArchiveType.Auto:
            break;
          default:
            throw new BugException("Unknonw archive type: " + caller.ArchiveType.ToString());
        }

        if (caller.VolumeSize > 0)
        {
          sbArgs.Append("-v");
          sbArgs.Append(StdConvert.ToString(caller.VolumeSize));
          sbArgs.Append(" ");
        }
        AddPassword(caller, sbArgs);
        sbArgs.Append("a ");
        sbArgs.Append(caller.ArchiveFileName.QuotedPath);

        for (int i = 0; i < caller.FileTemplates.Count; i++)
        {
          if (caller.FileTemplates[i].Recurse == recurse)
          {
            sbArgs.Append(" ");
            sbArgs.Append(caller.FileTemplates[i].Template.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
          }
        }

        int res = RunCommand(sbArgs, caller.FileDirectory);
        if (res > 0)
          ThrowExitCodeException(Res.FileCompressor_Action_Compress, res);
      }

      private void ThrowExitCodeException(string actionName, int exitCode)
      {
        // https://7-zip.opensource.jp/chm/cmdline/exit_codes.htm
        string exitDescr;
        switch (exitCode)
        {
          case 0: exitDescr=Res.FileCompressor_Err_7z_Success; break;
          case 1: exitDescr = Res.FileCompressor_Err_7z_Warning; break;
          case 2: exitDescr = Res.FileCompressor_Err_7z_Fatal; break;
          case 7: exitDescr = Res.FileCompressor_Err_7z_InvalidCommandLine; break;
          case 8: exitDescr = Res.FileCompressor_Err_7z_InsufficientMemory; break;
          case 255: exitDescr = Res.FileCompressor_Err_7z_UserCancel; break;
          default: exitDescr = "Unknown result";break;
        }
        throw new InvalidOperationException(String.Format(Res.FileCompressor_Err_CmdResult, _ExeFileName, actionName, exitCode, exitDescr));
      }

      private void AddPassword(FileCompressor caller, StringBuilder sbArgs)
      {
        if (!String.IsNullOrEmpty(caller.Password))
        {
          if (caller.Password.IndexOf('\"') >= 0)
            throw new InvalidOperationException(Res.FileCompressor_Err_PasswordWithQuoteChar);
          sbArgs.Append("-p\"");
          sbArgs.Append(caller.Password);
          sbArgs.Append("\" ");
        }
      }

      public void Decompress(FileCompressor caller)
      {
        StringBuilder sbArgs = new StringBuilder();
        AbsPath archiveFileName = caller.ArchiveFileName;
        if (DetectVolumes(ref archiveFileName))
          sbArgs.Append("-va ");
        AddPassword(caller, sbArgs);
        sbArgs.Append("x ");
        sbArgs.Append(archiveFileName.QuotedPath);

        int res = RunCommand(sbArgs, caller.FileDirectory);
        if (res > 0)
          ThrowExitCodeException(Res.FileCompressor_Action_Decompress, res);
      }

      public bool TestArchive(FileCompressor caller)
      {
        StringBuilder sbArgs = new StringBuilder();
        AbsPath archiveFileName = caller.ArchiveFileName;
        if (DetectVolumes(ref archiveFileName))
          sbArgs.Append("-va ");
        AddPassword(caller, sbArgs);
        sbArgs.Append("t ");
        sbArgs.Append(archiveFileName.QuotedPath);


        int res = RunCommand(sbArgs, caller.FileDirectory);
        switch (res)
        {
          case 0: return true;
          case 2: return false;
          default:
            ThrowExitCodeException(Res.FileCompressor_Action_Test, res);
            throw new BugException(); // не может быть
        }
      }

      private int RunCommand(StringBuilder sbArgs, AbsPath workingDir)
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.UseShellExecute = false;
        psi.FileName = _ExeFileName;
        psi.Arguments = sbArgs.ToString();
        psi.WorkingDirectory = workingDir.Path;
        psi.CreateNoWindow = true;
        //psi.WindowStyle = ProcessWindowStyle.Hidden;
        //psi.WindowStyle = ProcessWindowStyle.Minimized;
        Process proc = Process.Start(psi);
        proc.WaitForExit();
        return proc.ExitCode;
      }

      /// <summary>
      /// Определение наличия многотомного архива
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      private static bool DetectVolumes(ref AbsPath path)
      {
        if (System.IO.File.Exists(path.Path))
          return false; // обычный архив

        string fileName001 = path.FileName + ".001";
        AbsPath path001 = new AbsPath(path.ParentDir, fileName001);
        if (System.IO.File.Exists(path001.Path))
        {
          path = path001;
          return true;
        }
        throw new FileNotFoundException(String.Format(Res.FileCompressor_Err_ArchiveNotFound, path.Path));
      }
    }

    #endregion
  }
}
