using System;
using System.Collections.Generic;
using System.Text;
//using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using System.Runtime.Serialization;
//using SevenZip;
using System.Threading;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace FreeLibSet.IO
{
  /// <summary>
  /// Создание Zip-файлов для составных документов Open Office, MS Ofiice-2007 и других
  /// Для использования данного класса необходимо наличие загруженной сборки
  /// ICSharpCode.SharpZipLib.dll
  /// Используйте свойство ZipFileTools.ZipLibAvailable для предварительной проверки наличия библиотеки
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
      _File = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(zipFilePath);
      _File.BeginUpdate();
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
    /// и можно создавать объекты ZipFileCreator
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
    /// Выбрасывает исключение
    /// </summary>
    public static void CheckZipLibAvailable()
    {
      if (!ZipLibAvailable)
        throw new SharpZipLibNotFoundException();
    }

    #endregion

    #region Наличие библиотеки SevenZipSharp

    /// <summary>
    /// Возвращает true, если библиотека ICSharpCode.SharpZipLib.dll загружена
    /// и можно создавать объекты ZipFileCreator
    /// </summary>
    public static bool SevenZipSharpAvailable
    {
      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      get
      {
        lock (DataTools.InternalSyncRoot)
        {
          if (!_SevenZipSharpAvailable.HasValue)
          {
            try
            {
              TrySevenZipSharp();
              _SevenZipSharpAvailable = true;
            }
            catch
            {
              _SevenZipSharpAvailable = false;
            }
          }

          return _SevenZipSharpAvailable.Value;
        }
      }
    }
    private static bool? _SevenZipSharpAvailable = null;

    /// <summary>
    /// Это должно быть в отдельном методе, т.к. он может не запускаться
    /// </summary>
    private static void TrySevenZipSharp()
    {
      Type SomeType = typeof(SevenZip.SevenZipCompressor);

      // Инициализация библиотеки 7z.dll
      string FileName = (IntPtr.Size == 8) ? "7z64.dll" : "7z.dll";

      AbsPath RootDir = new AbsPath(SomeType.Assembly.GetName().CodeBase).ParentDir;
      if (File.Exists((RootDir + FileName).Path))
      {
        // ReSharper disable once AccessToStaticMemberViaDerivedType
        SevenZip.SevenZipCompressor.SetLibraryPath((RootDir + FileName).Path);
        return;
      }

      RootDir = FileTools.ApplicationBaseDir;
      if (File.Exists((RootDir + FileName).Path))
      {
        // ReSharper disable once AccessToStaticMemberViaDerivedType
        SevenZip.SevenZipCompressor.SetLibraryPath((RootDir + FileName).Path);
        return;
      }

      throw new FileNotFoundException("Не найдена библиотека " + FileName);
    }

    /// <summary>
    /// Выбрасывает исключение
    /// </summary>
    public static void CheckSevenZipSharpAvailable()
    {
      if (!SevenZipSharpAvailable)
        throw new FileNotFoundException("Не найдена библиотека SevenZipSharp.dll"); // ??
    }

    #endregion
  }


  /// <summary>
  /// Специальный тип исключения об отсутствии библиотеки SharpZipLib
  /// </summary>
  [Serializable]
  public class SharpZipLibNotFoundException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает исключение
    /// </summary>
    public SharpZipLibNotFoundException()
      : base("Не удалось загрузить библиотеку ICSharpCode.SharpZipLib.dll. Без нее невозможно создание сжатых файлов; в частности документов Microsoft Office 2007/2010 и Open Office")
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected SharpZipLibNotFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
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
    /// Внешняя заставка. Если свойство установлено, то выводится процентный индикатор
    /// Рекомендуется обязательное использование заставки, когда размер архива может быть большим или заранее неизвестен.
    /// Без заставки действие нельзя прервать
    /// </summary>
    public ISplash Splash { get { return _Splash; } set { _Splash = value; } }
    private ISplash _Splash;

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

            string[] aFiles = Owner.FileTemplates.GetAbsFileNames(Owner.FileDirectory, Owner.Splash);
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


      AsyncCompress Async = new AsyncCompress();
      ProcessArchiveOperation(Async, new ThreadStart(Async.CompressThreadProc), "Compressing " + ArchiveFileName.FileName);
    }

    private void ProcessArchiveOperation(AsyncBase async, ThreadStart threadMethod, string threadTitle)
    {
      bool OldAllowCancel = false;
      string OldPhaseText = null;

      if (Splash != null)
      {
        OldAllowCancel = Splash.AllowCancel;
        OldPhaseText = Splash.PhaseText;

        Splash.PercentMax = 100;
        Splash.AllowCancel = true;
      }

      async.Owner = this;
      async.IntFinished = false;

      Thread SubThread = new Thread(threadMethod);
      SubThread.Name = threadTitle;
      SubThread.Start();
      Thread.Sleep(50);

      while (true)
      {
        lock (async)
        {
          if (async.IntFinished)
            break;

          if (Splash != null)
          {
            try
            {
              if (async.IntCancelled)
                Splash.PhaseText = async.IntPhaseText + ". Прерывание будет выполнено при обработке следующего файла";
              else
                Splash.PhaseText = async.IntPhaseText;
              Splash.Percent = async.IntPercent;
              Splash.CheckCancelled();
            }
            catch
            {
              async.IntCancelled = true;
              try
              {
                Splash.AllowCancel = false; // сразу отменяем
                Splash.Percent = async.IntPercent;
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
        if (async.IntCancelled)
          throw new UserCancelException();

        Splash.PercentMax = 0;
        Splash.AllowCancel = OldAllowCancel;
        Splash.PhaseText = OldPhaseText;
      }

      // Блокировка объекта не требуется, т.к. поток архивации уже завершен
      if (async.Exception != null)
        throw async.Exception;
    }

    /// <summary>
    /// Получить тип архива из имени файла.
    /// Реальное наличие файла и его содержимое не проверяется.
    /// </summary>
    /// <param name="archiveFileName">Имя файла архива</param>
    /// <returns>Тип архива</returns>
    public static FileComressorArchiveType GetArchiveTypeFromFileName(AbsPath archiveFileName)
    {
      string Ext = System.IO.Path.GetExtension(archiveFileName.FileName).ToUpperInvariant();
      switch (Ext)
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

      string Path1 = ArchiveFileName.Path;
      string Path2 = ArchiveFileName.Path + ".001"; // 17.12.2014
      if (File.Exists(Path1) || File.Exists(Path2))
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

      AsyncDecompress Async = new AsyncDecompress();
      ProcessArchiveOperation(Async, new ThreadStart(Async.TestThreadProc), "Testing " + ArchiveFileName.FileName);

      return Async.TestResult;
    }

    #endregion
  }

}
