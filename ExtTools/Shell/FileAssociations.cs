// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

//#define USE_TRACE // трассировка в пределах этого файла

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using System.Diagnostics;
using Microsoft.Win32;
using FreeLibSet.Win32;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Shell
{
  /// <summary>
  /// Описание команды "Открыть" или "Открыть с помощью"
  /// </summary>
  [Serializable]
  public class FileAssociationItem
  {
    #region Конструктор

    internal FileAssociationItem(string progId, AbsPath programPath, string arguments, string displayName, AbsPath iconPath, int iconIndex, bool useURL, string infoSourceString)
    {
      if (String.IsNullOrEmpty(progId))
        throw new ArgumentNullException("progId");
      _ProgId = progId;

      if (programPath.IsEmpty)
        throw new ArgumentNullException("programPath");
      _ProgramPath = programPath;

      if (String.IsNullOrEmpty(arguments))
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
            _Arguments = "\"%1\"";
            break;
          case PlatformID.Unix:
            _Arguments = "%U";
            break;
        }
      }
      else
        _Arguments = arguments;

      if (String.IsNullOrEmpty(displayName))
        _DisplayName = GetDisplayName(_ProgramPath);
      else
        _DisplayName = displayName;

      _IconPath = iconPath;
      _IconIndex = iconIndex;

      _UseURL = useURL;

#if DEBUG
      _InfoSourceString = infoSourceString;
#endif
    }

    /// <summary>
    /// Возвращает отображаемое имя программы для заданного пути.
    /// Если выполняемый файл не содержит свойств, возвращается имя файла без расширения
    /// </summary>
    /// <param name="programPath">Путь к выполняемому файлу</param>
    /// <returns>Отображаемое имя</returns>
    internal static string GetDisplayName(AbsPath programPath)
    {
      if (programPath.IsEmpty)
        return String.Empty;
      string displayName = String.Empty;
      if (System.IO.File.Exists(programPath.Path))
      {
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(programPath.Path);
        if (!String.IsNullOrEmpty(fvi.FileDescription))
          displayName = fvi.FileDescription;
      }
      if (String.IsNullOrEmpty(displayName))
        displayName = programPath.FileNameWithoutExtension;

      return displayName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор ProgId
    /// </summary>
    public string ProgId { get { return _ProgId; } }
    private readonly string _ProgId;

    /// <summary>
    /// Путь к выполняемому файлу приложения
    /// </summary>
    public AbsPath ProgramPath { get { return _ProgramPath; } }
    private readonly AbsPath _ProgramPath;

    /// <summary>
    /// Аргументы командной строки для запуска приложения.
    /// Аргумент "%1" заменяется на путь к файлу
    /// </summary>
    public string Arguments { get { return _Arguments; } }
    private readonly string _Arguments;

    //public string CommandLine { get { return FCommandLine; } set { FCommandLine = value; } }

    /// <summary>
    /// Отображаемое имя программы для команд "Открыть с помощью"
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    /// <summary>
    /// Путь к файлу в котором содержится значок.
    /// Может быть не задан
    /// </summary>
    public AbsPath IconPath { get { return _IconPath; } }
    private readonly AbsPath _IconPath;

    /// <summary>
    /// Индекс значка в файле.
    /// См. описание функции Windows ExtractIcon()
    /// </summary>
    public int IconIndex { get { return _IconIndex; } }
    private readonly int _IconIndex;
    /// <summary>
    /// Если true, то при подстановке имени файла в командную строку будет использоваться форма "file:///"
    /// </summary>
    public bool UseURL { get { return _UseURL; } }
    private readonly bool _UseURL;

#if DEBUG
    /// <summary>
    /// Дополнительная информация, как была найдена эта копия (ключ реестра или имя переменной окружения).
    /// Это свойство существует только в отладочном режиме
    /// </summary>
    public string InfoSourceString { get { return _InfoSourceString; } }
    private readonly string _InfoSourceString;
#endif

    /// <summary>
    /// Возвращает DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region Выполнение команды

    private ProcessStartInfo CreateProcessStartInfo(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException();

      ProcessStartInfo psi = new ProcessStartInfo();
      psi.UseShellExecute = false;
      psi.FileName = this.ProgramPath.Path;
      psi.Arguments = this.Arguments;
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          //case PlatformID.Win32S:
          //case PlatformID.WinCE:
          psi.Arguments = psi.Arguments.Replace("%1", filePath.Path);
          break;
        case PlatformID.Unix:
          psi.Arguments = psi.Arguments.Replace("%U", filePath.QuotedPath); // у LibreOffice с большой буквы
          psi.Arguments = psi.Arguments.Replace("%u", filePath.QuotedPath); // у FireFox - с маленькой. В чем разница?

          psi.Arguments = psi.Arguments.Replace("%F", filePath.QuotedPath); // у программы просмотра каталога Thunar
          psi.Arguments = psi.Arguments.Replace("%f", filePath.QuotedPath); // для комплекта
          break;
        default:
          throw new PlatformNotSupportedException();
      }

      return psi;
    }

    /// <summary>
    /// Открывает приложение и указанный документ в нем.
    /// </summary>
    /// <param name="filePath">Путь к документу</param>
    public void Execute(AbsPath filePath)
    {
      ProcessStartInfo psi = CreateProcessStartInfo(filePath);
#if USE_TRACE
      System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + ". Starting file association \"" + DisplayName + "\" for \"" + FilePath.Path + "\".");
      string s;
      try
      {
        s = LogoutTools.LogoutObjectToString(psi);
      }
      catch (Exception e)
      {
        s = "*** " + e.Message + " ***";
      }
      //System.Diagnostics.Trace.Indent();
      try
      {
        System.Diagnostics.Trace.WriteLine("Process information:");
        System.Diagnostics.Trace.WriteLine(s);
      }
      finally
      {
        //System.Diagnostics.Trace.Unindent();
      }
#endif

      try
      {
        Process.Start(psi);
      }
      catch (Exception e)
      {
        try
        {
          e.Data["ProcessStartInfo.FileName"] = psi.FileName;
          e.Data["ProcessStartInfo.Arguments"] = psi.Arguments;
          e.Data["ProcessStartInfo.UseShellExecute"] = psi.UseShellExecute;
        }
        catch { }

        throw;
      }
    }

    #endregion
  }

  /// <summary>
  /// Ассоциации для заданного типа файлов.
  /// Для получения ассоциаций используйте статический метод FromFileExtension()
  /// </summary>
  [Serializable]
  public class FileAssociations : IReadOnlyObject
  {
    #region Конструктор

    private FileAssociations(bool isReadOnly)
    {
      _OpenWithItems = new OpenWithItemList();
      if (isReadOnly)
        SetReadOnly();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Команда "Открыть"
    /// </summary>
    public FileAssociationItem OpenItem
    {
      get { return _OpenItem; }
      set
      {
        ((IReadOnlyObject)this).CheckNotReadOnly();
        _OpenItem = value;
      }
    }
    private FileAssociationItem _OpenItem;

    [Serializable]
    private class OpenWithItemList : ListWithReadOnly<FileAssociationItem>
    {
      // Первоначально использовался базовый класс NamedList.
      // Но не имеет смысла использовать, т.к. поиск существующего элемента ведется и по ProgId и по ProgramPath

      #region Методы

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    /// <summary>
    /// Команды "Открыть с помощью"
    /// </summary>
    public IList<FileAssociationItem> OpenWithItems { get { return _OpenWithItems; } }
    private readonly OpenWithItemList _OpenWithItems;

    private bool OpenWithContains(FileAssociationItem item)
    {
      return OpenWithIndexOf(item) >= 0;
    }

    private int OpenWithIndexOf(FileAssociationItem item)
    {
      for (int i = 0; i < _OpenWithItems.Count; i++)
      {
        if (String.Equals(_OpenWithItems[i].ProgId, item.ProgId, StringComparison.OrdinalIgnoreCase))
          return i;
        if (_OpenWithItems[i].ProgramPath == item.ProgramPath)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Пустой список, доступный только для чтения
    /// </summary>
    public static readonly FileAssociations Empty = new FileAssociations(true);

    /// <summary>
    /// Возвращает true, если извлечение ассоциаций реализовано для операционной системы
    /// </summary>
    public static bool IsSupported
    {
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
          //case PlatformID.Win32S:
          //case PlatformID.WinCE:
          case PlatformID.Unix:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// Если во время получения списка ассоциаций возникло исключение, оно сохраняется
    /// в этом поле.
    /// Если получение прошло успешно, то свойство содержит null.
    /// </summary>
    public Exception Exception
    {
      get { return _Exception; }
      set
      {
        ((IReadOnlyObject)this).CheckNotReadOnly();
        _Exception = value;
      }
    }
    [NonSerialized]
    private Exception _Exception;

    #endregion

    #region Получение для расширения файла

    /// <summary>
    /// Создает список ассоциаций для заданного расширения файла.
    /// Реализовано для Windows и Linux. 
    /// Для других операционных систем возвращает пустой список.
    /// Возвращаемый список ассоциаций переведен в режим "Только чтение".
    /// 
    /// Этот метод выполняет опрос системы при каждом вызове.
    /// Используйте свойство EFPApp.FileExtAssociations, которое поддерживает буферизацию
    /// </summary>
    /// <param name="fileExt">Расширение файла, включая точку</param>
    /// <returns>Список ассоциаций</returns>
    public static FileAssociations FromFileExtension(string fileExt)
    {
      try
      {
        FileAssociations faItems = DoFromFileExtension(fileExt);
        faItems.SetReadOnly();
        return faItems;
      }
      catch (Exception e)
      {
        e.Data["FileExt"] = fileExt;
        LogoutTools.LogoutException(e, "Ошибка загрузки файловых ассоциаций");
        return FromError(e);
      }
    }

    /// <summary>
    /// Возвращает пустой список ассоциаций с заданным объктом исключения.
    /// </summary>
    /// <param name="e">Перехваченное исключение</param>
    /// <returns>Пустой список</returns>
    private static FileAssociations FromError(Exception e)
    {
      FileAssociations faItems = new FileAssociations(false);
      faItems.Exception = e;
      faItems.SetReadOnly();
      return faItems;
    }

    private static FileAssociations DoFromFileExtension(string fileExt)
    {
      // В случае изменений не забыть про свойство IsSupported

      if (String.IsNullOrEmpty(fileExt))
        throw new ArgumentNullException("fileExt");
      if (fileExt[0] != '.')
        throw new ArgumentException("Расширение должно начинаться с точки", "fileExt");

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          return Windows.FromFileExtension(fileExt);
        case PlatformID.Unix:
          string mimeType = Linux.GetMimeTypeFromFileExtension(fileExt);
          if (mimeType.Length == 0)
            return Empty;
          else
            return Linux.FromMimeType(mimeType);
        default:
          return Empty;
      }
    }

    #endregion

    #region Получение для MIME-типа

    /// <summary>
    /// Создает список ассоциаций для заданного типа MIME.
    /// Реализовано для Windows и Linux. 
    /// Для других операционных систем возвращает пустой список.
    /// Возвращаемый список ассоциаций переведен в режим "Только чтение".
    /// 
    /// Для Windows этот метод практически бесполезен.
    /// </summary>
    /// <param name="mimeType">MIME-тип, например, "text/plain"</param>
    /// <returns>Список ассоциаций</returns>
    public static FileAssociations FromMimeType(string mimeType)
    {
      try
      {
        FileAssociations faItems = DoFromMimeType(mimeType);
        faItems.SetReadOnly();
        return faItems;
      }
      catch (Exception e)
      {
        e.Data["MimeType"] = mimeType;
        LogoutTools.LogoutException(e, "Ошибка загрузки файловых ассоциаций для MIME-типа");
        return FromError(e);
      }
    }

    private static FileAssociations DoFromMimeType(string mimeType)
    {
      // В случае изменений не забыть про свойство IsSupported


      if (String.IsNullOrEmpty(mimeType))
        throw new ArgumentNullException("mimeType");

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          return Windows.FromMimeType(mimeType);
        case PlatformID.Unix:
          return Linux.FromMimeType(mimeType);
        default:
          return Empty;
      }
    }

    #endregion

    #region Для каталога

    /// <summary>
    /// Создает список файловых ассоциаций для просмотра каталогов.
    /// Для Windows обычно возвращает единственный вариант - explorer.exe.
    /// 
    /// Используйте свойство EFPApp.FileAssociations.ShowDirectory
    /// </summary>
    /// <returns></returns>
    public static FileAssociations FromDirectory()
    {
      try
      {
        FileAssociations faItems = DoFromDirectory();
        faItems.SetReadOnly();
        return faItems;
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Ошибка загрузки файловых ассоциаций для каталогов");
        return FromError(e);
      }
    }

    private static FileAssociations DoFromDirectory()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          return Windows.FromDirectory();
        case PlatformID.Unix:
          return Linux.FromDirectory();
        default:
          return Empty;
      }
    }

    #endregion

    #region Из реестра Windows

    private static class Windows
    {
      #region FromFileExtension

      internal static FileAssociations FromFileExtension(string fileExt)
      {
        FileAssociations faItems = new FileAssociations(false);

        using (RegistryTree2 tree = new RegistryTree2(true))
        {
          FromFileExtensionExplorer(fileExt, faItems, tree);
          FromFileExtensionsHKCR(fileExt, faItems, tree);

          if (faItems.OpenItem == null && faItems.OpenWithItems.Count > 0)
            faItems.OpenItem = faItems.OpenWithItems[0];
          else if (faItems.OpenItem != null)
          {
            if (!faItems.OpenWithContains(faItems.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              faItems.OpenWithItems.Add(faItems.OpenItem);
          }
        }

        return faItems;
      }

      private static void FromFileExtensionExplorer(string fileExt, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key2 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\UserChoice"];
        if (key2 != null)
        {
          FileAssociationItem item2 = GetProgIdItem(tree, DataTools.GetString(key2.GetValue("progid")), key2.Name);
          if (item2 != null)
          {
            faItems.OpenItem = item2;
            if (!faItems.OpenWithContains(item2))
              faItems.OpenWithItems.Insert(0, item2);
          }
        }

        // Теперь - OpenWithList
        RegistryKey2 key3 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\OpenWithList"];
        if (key3 != null)
        {
          string mruList = DataTools.GetString(key3.GetValue("MRUList")); // кодируется отдельными буквами
          for (int i = 0; i < mruList.Length; i++)
          {
            string valName = new string(mruList[i], 1); // строка из одной буквы
            string progId3 = DataTools.GetString(key3.GetValue(valName));
            FileAssociationItem item = GetProgIdItem(tree, progId3, key3.Name);
            if (item != null && (!faItems.OpenWithContains(item)))
              faItems._OpenWithItems.Add(item);
          }
        }
      }

      private static void FromFileExtensionsHKCR(string fileExt, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key1 = tree[@"HKEY_CLASSES_ROOT\" + fileExt];
        if (key1 != null)
        {
          RegistryKey2 keyOWPI = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithProgIds"];
          if (keyOWPI != null)
          {
            string[] aProgIds = keyOWPI.GetValueNames();
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem item = GetProgIdItem(tree, aProgIds[i], keyOWPI.Name);
              if (item != null && (!faItems.OpenWithContains(item)))
                faItems._OpenWithItems.Add(item);
            }
          }
          RegistryKey2 keyOWL = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithList"];
          if (keyOWL != null)
          {
            string[] aProgIds = keyOWL.GetSubKeyNames(); // а не value names
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem item = GetProgIdItem(tree, aProgIds[i], keyOWL.Name);
              if (item != null && (!faItems.OpenWithContains(item)))
                faItems._OpenWithItems.Add(item);
            }
          }

          FileAssociationItem item0 = GetProgIdItem(tree, DataTools.GetString(key1.GetValue(String.Empty)), key1.Name);
          if (item0 != null)
          {
            int p = faItems.OpenWithIndexOf(item0);
            if (p < 0)
            {
              faItems.OpenWithItems.Insert(0, item0);
              p = 0;
            }
            faItems.OpenItem = faItems.OpenWithItems[p]; // а не Item0
          }
        }
      }

      private static FileAssociationItem GetProgIdItem(RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (String.IsNullOrEmpty(progId))
          return null;

        RegistryKey2 keyProgId = tree[@"HKEY_CLASSES_ROOT\" + progId];
        if (keyProgId == null)
          return GetProgIdItemForExeFile(tree, progId, infoSourceString);

        return DoGetProgIdItem(tree, progId, keyProgId, infoSourceString);
      }

      private static FileAssociationItem DoGetProgIdItem(RegistryTree2 tree, string progId, RegistryKey2 keyProgId, string infoSourceString)
      {
        string cmd = tree.GetString(keyProgId.Name + @"\shell\open\command", String.Empty);
        if (String.IsNullOrEmpty(cmd))
          return null;

        if (cmd.IndexOf(@"%1", StringComparison.Ordinal) < 0)
          // Обмен с помощью DDE не реализован
          return null;

        string fileName, arguments;
        if (!SplitFileNameAndArgs(cmd, out fileName, out arguments))
          return null;

        fileName = Environment.ExpandEnvironmentVariables(fileName);
        AbsPath path = AbsPath.Create(fileName);
        if (path.IsEmpty)
          return null; // 25.01.2019
        if (!System.IO.File.Exists(path.Path))
          return null;

        string displayName = String.Empty;

        if (path.FileName.ToLowerInvariant() == "rundll32.exe")
        {
          // 22.09.2019
          // Извлекаем данные из аргументов 

          string fileName2 = GetFileNameFromArgs(arguments);
          if (!String.IsNullOrEmpty(fileName2))
          {
            fileName2 = Environment.ExpandEnvironmentVariables(fileName2);
            AbsPath path2 = AbsPath.Create(fileName2);
            if (!path2.IsEmpty)
              displayName = FileAssociationItem.GetDisplayName(path2);
          }
        }

        AbsPath iconPath = AbsPath.Empty;
        int iconIndex = 0;
        RegistryKey2 keyDefIcon = tree[keyProgId.Name + @"\DefaultIcon"];
        if (keyDefIcon != null)
        {
          string s = DataTools.GetString(keyDefIcon.GetValue(String.Empty));
          if (!(s == "%1" || s == "\"%1\""))
          {
            ParseIconInfo(s, out iconPath, out iconIndex);
          }
        }
        if (iconPath.IsEmpty)
        {
          iconPath = path;
          iconIndex = 0;
        }

        return new FileAssociationItem(progId, path, arguments, displayName, iconPath, iconIndex, false,
          infoSourceString + Environment.NewLine + keyProgId.Name + @"\shell\open\command");
      }

      /// <summary>
      /// Получаем имя файла из строки аргументов.
      /// Имя файла может быть в кавычках
      /// </summary>
      /// <param name="arguments"></param>
      /// <returns></returns>
      private static string GetFileNameFromArgs(string arguments)
      {
        if (String.IsNullOrEmpty(arguments))
          return String.Empty;
        if (arguments[0] == '\"')
        {
          // Ищем закрывающую кавычку
          // TODO: Кавычка в имени файла
          string s = arguments.Substring(1);
          int p = s.IndexOf('\"');
          if (p < 0)
            return string.Empty; // ошибка
          else
            return s.Substring(0, p);
        }
        else
        {
          // Имя файла без кавычек.
          // Возвращаем все до первого пробела
          int p = arguments.IndexOf(' ');
          if (p >= 0)
            return arguments.Substring(0, p);
          else
            return arguments;
        }
      }

      private static FileAssociationItem GetProgIdItemForExeFile(RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (!progId.EndsWith(".EXE", StringComparison.OrdinalIgnoreCase))
          return null;

        FileAssociationItem faItem = GetProgIdItemForExeFileHKCRApplications(tree, progId, infoSourceString);
        if (faItem != null)
          return faItem;
        else
          return GetProgIdItemForExeFileAppPathes(tree, progId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileHKCRApplications(RegistryTree2 tree, string progId, string infoSourceString)
      {
        RegistryKey2 keyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        if (keyProgId == null && progId.IndexOf('\\') < 0)
        {
          // Может быть задано просто имя EXE-файла, например, "notepad.exe", тогда его надо искать
          // в подразделе "Applications"
          // Идентификатор приложения тоже надо изменить, иначе в списке будет два блокнота
          progId = @"Applications\" + progId;
          keyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        }
        if (keyProgId == null)
          return null;

        return DoGetProgIdItem(tree, progId, keyProgId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileAppPathes(RegistryTree2 tree, string progId, string infoSourceString)
      {
        try
        {
          // Последняя попытка - найти путь к приложению
          string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + progId;
          string filePath = tree.GetString(keyName, String.Empty);
          AbsPath path = AbsPath.Create(filePath); // 25.01.2019
          if (path.IsEmpty)
            return null;
          if (!System.IO.File.Exists(path.Path))
            return null;

          return new FileAssociationItem(progId, path, "\"%1\"", String.Empty,
            path, 0,
            tree.GetBool(keyName, "useURL"),
            infoSourceString + Environment.NewLine + keyName);
        }
        catch // System.Security.SecurityException
        {
          return null; // 27.05.2017
        }
      }

      private static void ParseIconInfo(string iconInfo, out AbsPath iconPath, out int iconIndex)
      {
        iconPath = AbsPath.Empty;
        iconIndex = 0;
        if (String.IsNullOrEmpty(iconInfo))
          return;

        string fileName;
        int p = iconInfo.LastIndexOf(',');
        if (p >= 0)
        {
          fileName = iconInfo.Substring(0, p);
          string sIconIndex = iconInfo.Substring(p + 1);
          int.TryParse(sIconIndex, out iconIndex);
        }
        else
        {
          fileName = iconInfo;
          iconIndex = 0;
        }
        fileName = Environment.ExpandEnvironmentVariables(fileName);
        if (fileName.IndexOf(System.IO.Path.DirectorySeparatorChar) < 0) // имя файла без пути
        {
          // 13.12.2018 Пытаемся найти в системном каталоге
          iconPath = AbsPath.Create(AbsPath.Create(Environment.SystemDirectory), fileName);
          //if (!System.IO.File.Exists(IconPath.Path))
          //  IconPath = FileTools.FindExecutableFilePath(FileName); 
        }
        if (iconPath.IsEmpty)
          iconPath = AbsPath.Create(fileName);
      }

      #endregion

      #region FromMimeType

      internal static FileAssociations FromMimeType(string mimeType)
      {
        FileAssociations faItems = new FileAssociations(false);

        using (RegistryTree2 tree = new RegistryTree2(true))
        {
          FromMimeTypeHKCR_MIME(mimeType, faItems, tree);

          if (faItems.OpenItem == null && faItems.OpenWithItems.Count > 0)
            faItems.OpenItem = faItems.OpenWithItems[0];
          else if (faItems.OpenItem != null)
          {
            if (!faItems.OpenWithContains(faItems.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              faItems.OpenWithItems.Add(faItems.OpenItem);
          }
        }

        return faItems;
      }

      private static void FromMimeTypeHKCR_MIME(string mimeType, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key1 = tree[@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType];
        if (key1 != null)
        {
          string clsId = DataTools.GetString(key1.GetValue("CLSID"));
          if (!String.IsNullOrEmpty(clsId))
          {
            RegistryKey2 key2 = tree[@"HKEY_CLASSES_ROOT\CLSID\" + clsId + @"\ProgId"];
            if (key2 != null)
            {
              string progId = DataTools.GetString(key2.GetValue(String.Empty));
              FileAssociationItem faItem = GetProgIdItem(tree, progId, key1.Name);
              if (faItem != null)
                faItems.OpenWithItems.Add(faItem);
            }
          }
        }
      }

      #endregion

      #region Ассоциации для каталога

      internal static FileAssociations FromDirectory()
      {
        // TODO: поиск замены explorer.exe

        FileAssociations faItems = new FileAssociations(false);
        AbsPath path = FileTools.FindExecutableFilePath("explorer.exe");
        if (path.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("explorer.exe not found");
#endif
          return faItems;
        }

        FileAssociationItem faItem = new FileAssociationItem("Explorer", path, "%1", "Windows Explorer",
          path, 0, false, "Fixed");
        faItems.OpenWithItems.Add(faItem);
        faItems.OpenItem = faItem;
        return faItems;
      }

      #endregion
    }

    #endregion

    #region Linux

    private static class Linux
    {
      #region Расширение файла -> MIME

      /// <summary>
      /// Словарь соответствий расширений MIME-типам.
      /// Заполняется при первом обращении
      /// Ключ - расширение файла (с точкой) в верхнем регистре
      /// Значение - тип MIME.
      /// </summary>
      private static Dictionary<string, string> _FileExtMimeDict = new Dictionary<string, string>();

      /// <summary>
      /// Возвращает mime-тип для расширения файла.
      /// </summary>
      /// <param name="fileExt">Расширение файла</param>
      /// <returns>MIME-тип</returns>
      internal static string GetMimeTypeFromFileExtension(string fileExt)
      {
        string mime;
        lock (_FileExtMimeDict)
        {
          if (_FileExtMimeDict.Count == 0)
            InitFileExtMimeDict();

          if (!_FileExtMimeDict.TryGetValue(fileExt.ToUpperInvariant(), out mime))
            mime = String.Empty;
        }
        return mime;
      }

      /// <summary>
      /// Инициализация словаря FileExtMimeDict
      /// </summary>
      private static void InitFileExtMimeDict()
      {
#if USE_TRACE
        Trace.WriteLine("Loading file mime types ...");
#endif

        #region Несколько стандартных

        _FileExtMimeDict[".TXT"] = "text/plain";
        _FileExtMimeDict[".HTM"] = "text/html";
        _FileExtMimeDict[".HTML"] = "text/html";
        _FileExtMimeDict[".XML"] = "text/xml";

        #endregion

        #region Загружаем из файлов XML

        // По идее, нужно анализировать файлы "/usr/share/mime/packages/*.xml", в особенности freedesktop.org.xml.

        try
        {
          string[] aFiles = System.IO.Directory.GetFiles("/usr/share/mime/packages", "*.xml", System.IO.SearchOption.TopDirectoryOnly);
          for (int i = 0; i < aFiles.Length; i++)
          {
#if USE_TRACE
            System.Diagnostics.Trace.WriteLine("Loading "+aFiles[i]+" ...");
#endif
            try
            {
              System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
              xmlDoc.Load(aFiles[i]);
              System.Xml.XmlNamespaceManager nmSpcMan = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
              nmSpcMan.AddNamespace("Def", @"http://www.freedesktop.org/standards/shared-mime-info");
              System.Xml.XmlNodeList mtnodes = xmlDoc.SelectNodes("Def:mime-info/Def:mime-type", nmSpcMan);
#if USE_TRACE
              System.Diagnostics.Trace.WriteLine("  mime-type count=" + mtnodes.Count.ToString());
#endif
              foreach (System.Xml.XmlNode mtnode in mtnodes)
              {
                string mimetype = GetAttrStr(mtnode, "type");
                if (mimetype.Length == 0)
                  continue;

                foreach (System.Xml.XmlNode globnode in mtnode.SelectNodes("Def:glob", nmSpcMan))
                {
                  string pattern = GetAttrStr(globnode, "pattern");
                  if (!pattern.StartsWith("*.", StringComparison.Ordinal))
                    continue;

                  _FileExtMimeDict[pattern.Substring(1).ToUpperInvariant()] = mimetype;
                }
              }
            }
            catch (Exception e)
            {
              LogoutTools.LogoutException(e, "Ошибка загрузки " + aFiles[i].Length);
            }
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка заполнения словаря mime-типов");
        }

        #endregion

#if USE_TRACE
        Trace.WriteLine("File mime types loaded. Count=" + FileExtMimeDict.Count.ToString()+".");
#endif
      }

      private static string GetAttrStr(System.Xml.XmlNode node, string name)
      {
        System.Xml.XmlAttribute attr = node.Attributes[name];
        if (attr == null)
          return String.Empty;
        else
          return attr.Value;
      }

      #endregion

      #region Ассоциации для mime-типов

      /// <summary>
      /// Так как класс FileAssociations предполагается потокобезопасным,
      /// иногда необходимо выполнять блокировку
      /// </summary>
      private static object _SyncRoot = new object();

      private static string _DefaultsListFilePath = "~/.local/share/applications/defaults.list";

      private static string _MimeinfoCacheFilePath = "/usr/share/applications/mimeinfo.cache";

      /// <summary>
      /// Время модификации файла "~/.local/share/applications/defaults.list"
      /// </summary>
      private static DateTime _DefaultsListFileTime;

      /// <summary>
      /// Время модификации файла "/usr/share/applications/mimeinfo.cache"
      /// </summary>
      private static DateTime _MimeinfoCacheFileTime;

      /// <summary>
      /// Таблица соответствий mime-типов и desktop-файлов.
      /// Ключ - MIME-тип, значение - список ярылков .desktop (через точку с запятой)
      /// </summary>
      private static Dictionary<string, string> _MimeDesktopFiles;

      internal static FileAssociations FromMimeType(string mimeType)
      {
        FileAssociations faItems;
        lock (_SyncRoot)
        {
          if (NeedsRecreateMimeDesktopFiles())
            CreateMimeDesktopFiles();

          faItems = DoFromMimeType(mimeType);
        }
        return faItems;
      }

      private static FileAssociations DoFromMimeType(string mimeType)
      {
        string sDesktopFiles;
        if (!_MimeDesktopFiles.TryGetValue(mimeType, out sDesktopFiles))
          return FileAssociations.Empty; // неизвестный mime-тип
        if (String.IsNullOrEmpty(sDesktopFiles))
          return FileAssociations.Empty; // нет ассоциации

#if USE_TRACE
        System.Diagnostics.Trace.WriteLine("Desktop files for MIMETYPE=\"" + MimeType + "\": \"" + sDesktopFiles + "\"");
#endif

        string[] aDesktopFiles = sDesktopFiles.Split(';');
        FileAssociations faItems = new FileAssociations(false);
        for (int i = 0; i < aDesktopFiles.Length; i++)
        {
          FileAssociationItem faItem = CreateFromDesktopFile(aDesktopFiles[i]);
          if (faItem != null)
            faItems.OpenWithItems.Add(faItem);
        }
        if (faItems.OpenWithItems.Count > 0)
          faItems.OpenItem = faItems.OpenWithItems[0];
        faItems.SetReadOnly();
        return faItems;
      }

      private static FileAssociationItem CreateFromDesktopFile(string desktopFileName)
      {
        if (String.IsNullOrEmpty(desktopFileName))
          return null;
        if (!desktopFileName.EndsWith(".desktop", StringComparison.Ordinal))
          desktopFileName += ".desktop";
        AbsPath DesktopFilePath = new AbsPath("/usr/share/applications/" + desktopFileName);
        if (!System.IO.File.Exists(DesktopFilePath.Path))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Desktop entry file not found: " + DesktopFilePath.Path);
#endif
          return null;
        }

        IniFile file = new IniFile(true);
        file.Load(DesktopFilePath);
        string displayName = file["Desktop Entry", "Name[" + LanguageStr + "]"]; // Name[ru]
        if (String.IsNullOrEmpty(displayName))
          displayName = file["Desktop Entry", "Name"];
        if (String.IsNullOrEmpty(displayName))
          displayName = desktopFileName;
        string sExec = file["Desktop Entry", "Exec"];
        if (String.IsNullOrEmpty(sExec))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("There is no \"Exec\" key in " + DesktopFilePath.Path);
#endif
          return null;
        }
        string fileName;
        string arguments;
        if (!SplitFileNameAndArgs(sExec, out fileName, out arguments))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot split \"Exec\" key=\"" + sExec + "\" found in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath programPath = FileTools.FindExecutableFilePath(fileName);
        if (programPath.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot find executable file \"" + FileName + "\", defined in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath iconPath = AbsPath.Empty;
        string sIcon = file["Desktop Entry", "Icon"];

#if USE_TRACE
        //System.Diagnostics.Trace.WriteLine("Icon=" + sIcon);
#endif
        if (!String.IsNullOrEmpty(sIcon))
        {
          try
          {
            /*

            AbsPath IndexThemePath = new AbsPath("/usr/share/icons/default/index.theme");
            if (System.IO.File.Exists(IndexThemePath.Path))
            {
              IniFile IndexThemeFile = new IniFile();
              IndexThemeFile.Load(IndexThemePath.Path);
              string Theme = IndexThemeFile["Icon Theme", "Inherits"];
#if USE_TRACE
              System.Diagnostics.Trace.WriteLine("Theme=" + Theme);
#endif
              if (!String.IsNullOrEmpty(Theme))
              {
                AbsPath IconPath2 = new AbsPath("/usr/share/icons/" + Theme + "/" + sIcon + ".png");
#if USE_TRACE
                System.Diagnostics.Trace.WriteLine("Icon Path=" + IconPath2);
#endif
                if (System.IO.File.Exists(IconPath2.Path))
                  IconPath = IconPath2;
              }
            }
             * 
             * */

            // Х.З., как правильно выбрать тему
            string[] a = System.IO.Directory.GetFiles("/usr/share/icons", sIcon + ".png", System.IO.SearchOption.AllDirectories);
            if (a.Length > 0)
              iconPath = new AbsPath(a[0]);
          }
          catch { }
        }

        return new FileAssociationItem(desktopFileName, programPath, arguments, displayName, iconPath, 0, false, DesktopFilePath.Path);
      }

      /// <summary>
      /// Возвращает идентификатор языка системы, например, "ru"
      /// </summary>
      private static string LanguageStr
      {
        get
        {
          string s = System.Globalization.CultureInfo.CurrentUICulture.Name;
          int p = s.IndexOf('-');
          if (p >= 0)
            s = s.Substring(0, p);
          return s;
        }
      }

      /// <summary>
      /// Надо ли обновить словарь ассоциаций MimeDesktopFiles.
      /// </summary>
      /// <returns></returns>
      private static bool NeedsRecreateMimeDesktopFiles()
      {
        if (_MimeDesktopFiles == null)
          return true;
        if (System.IO.File.Exists(_DefaultsListFilePath))
        {
          if (System.IO.File.GetLastWriteTime(_DefaultsListFilePath) != _DefaultsListFileTime)
            return true;
        }

        if (System.IO.File.Exists(_MimeinfoCacheFilePath))
        {
          if (System.IO.File.GetLastWriteTime(_MimeinfoCacheFilePath) != _MimeinfoCacheFileTime)
            return true;
        }
        return false;
      }

      private static void CreateMimeDesktopFiles()
      {
#if USE_TRACE
        System.Diagnostics.Trace.WriteLine("Recreating desktop file associations ...");
#endif
        _MimeDesktopFiles = new Dictionary<string, string>();

        #region Общий список

        if (System.IO.File.Exists(_MimeinfoCacheFilePath))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + MimeinfoCacheFilePath);
#endif
          FreeLibSet.IO.IniFile ini = new IniFile(true);
          ini.Load(new AbsPath(_MimeinfoCacheFilePath));
          foreach (IniKeyValue pair in ini.GetKeyValues("MIME Cache"))
            _MimeDesktopFiles[pair.Key] = pair.Value;
          _MimeinfoCacheFileTime = System.IO.File.GetLastWriteTime(_MimeinfoCacheFilePath);
        }

        #endregion

        #region Пользовательские настройки

        if (System.IO.File.Exists(_DefaultsListFilePath))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + DefaultsListFilePath);
#endif
          FreeLibSet.IO.IniFile ini = new IniFile(true);
          ini.Load(new AbsPath(_DefaultsListFilePath));
          foreach (IniKeyValue pair in ini.GetKeyValues("Default Applications"))
            _MimeDesktopFiles[pair.Key] = pair.Value;
          _DefaultsListFileTime = System.IO.File.GetLastWriteTime(_DefaultsListFilePath);
        }

        #endregion

#if USE_TRACE
        System.Diagnostics.Trace.WriteLine("Desktop file associations finished. MIME type count=" + MimeDesktopFiles.Count.ToString());
#endif
      }

      #endregion

      #region Ассоциации для каталогов

      internal static FileAssociations FromDirectory()
      {
        return FromMimeType("inode/directory");
      }

      #endregion
    }

    #endregion

    #region Вспомогательные методы

    private static bool SplitFileNameAndArgs(string commandLine, out string fileName, out string arguments)
    {
      fileName = String.Empty;
      arguments = String.Empty;
      if (String.IsNullOrEmpty(commandLine))
        return false;

      if (commandLine[0] == '\"')
      {
        // Имя программы в кавычках
        StringBuilder sb = new StringBuilder();
        int pEndQuota = -1;
        for (int i = 1; i < commandLine.Length; i++)
        {
          if (commandLine[i] == '\"')
          {
            if (i < (commandLine.Length - 1))
            {
              char nextChar = commandLine[i + 1];
              if (nextChar == '\"') // удвоенная кавычка
              {
                sb.Append('\"');
                i++; // пропускаем один символ
                continue;
              }
            }
            pEndQuota = i;
            break;
          }
          else
            sb.Append(commandLine[i]);
        }
        fileName = sb.ToString();
        if (pEndQuota < 0)
          return false;

        if (pEndQuota < (commandLine.Length - 1))
          arguments = commandLine.Substring(pEndQuota + 1).TrimStart(' ');
        return true;
      }
      else
      {
        // Имя программы от аргументов отделяется пробелом
        int p = commandLine.IndexOf(' ');
        if (p >= 0)
        {
          fileName = commandLine.Substring(0, p);
          arguments = commandLine.Substring(p + 1);
        }
        else
          fileName = commandLine;
        return true;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    bool IReadOnlyObject.IsReadOnly
    {
      get { return _OpenWithItems.IsReadOnly; }
    }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      if (_OpenWithItems.IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    private void SetReadOnly()
    {
      _OpenWithItems.SetReadOnly();
    }

    #endregion
  }
}
