// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
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
    [DebuggerStepThrough]
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
}
