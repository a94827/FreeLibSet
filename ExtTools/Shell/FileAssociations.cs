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
  /// Описание команды "Открыть" или "Открыть с помощью".
  /// Содержит данные, необходимые для открытия файла и необязательное описание значка.
  /// </summary>
  [Serializable]
  public sealed class FileAssociationItem : IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создает описатель без указания значка
    /// </summary>
    /// <param name="progId">Идентификатор приложения, например, "PBrush"</param>
    /// <param name="programPath">Путь к выполняемому файлу приложения</param>
    /// <param name="arguments">Аргументы командной строки, например, "%1"</param>
    /// <param name="displayName">Отображаемое имя приложения, например, "Paint"</param>
    public FileAssociationItem(string progId, AbsPath programPath, string arguments, string displayName)
      : this(progId, programPath, arguments, displayName, AbsPath.Empty, -1, false, String.Empty)
    {
    }

    /// <summary>
    /// Создает описатель
    /// </summary>
    /// <param name="progId">Идентификатор приложения, например, "PBrush"</param>
    /// <param name="programPath">Путь к выполняемому файлу приложения</param>
    /// <param name="arguments">Аргументы командной строки, например, "%1"</param>
    /// <param name="displayName">Отображаемое имя приложения, например, "Paint"</param>
    /// <param name="iconPath">Путь к файлу, содержащему значок. Часто совпадает с <paramref name="programPath"/>.</param>
    /// <param name="iconIndex">Индекс значка в файле <paramref name="iconPath"/></param>
    public FileAssociationItem(string progId, AbsPath programPath, string arguments, string displayName, AbsPath iconPath, int iconIndex)
      : this(progId, programPath, arguments, displayName, iconPath, iconIndex, false, String.Empty)
    {
    }

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
    /// Идентификатор ProgId. Должен быть уникальным в пределах списка <see cref="FileAssociations"/>.
    /// </summary>
    public string ProgId { get { return _ProgId; } }
    private readonly string _ProgId;

    string IObjectWithCode.Code { get { return _ProgId; } }

    /// <summary>
    /// Путь к выполняемому файлу приложения
    /// </summary>
    public AbsPath ProgramPath { get { return _ProgramPath; } }
    private readonly AbsPath _ProgramPath;

    /// <summary>
    /// Аргументы командной строки для запуска приложения.
    /// Аргумент "%1" заменяется на путь к файлу.
    /// </summary>
    public string Arguments { get { return _Arguments; } }
    private readonly string _Arguments;

    /// <summary>
    /// Отображаемое имя программы для команд "Открыть с помощью"
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    /// <summary>
    /// Путь к файлу в котором содержится значок.
    /// Может быть не задан.
    /// </summary>
    public AbsPath IconPath { get { return _IconPath; } }
    private readonly AbsPath _IconPath;

    /// <summary>
    /// Индекс значка в файле.
    /// См. описание функции Windows ExtractIcon().
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
    /// Возвращает <see cref="DisplayName"/>
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
  /// Для получения ассоциаций используйте статический метод <see cref="FromFileExtension(string)"/>.
  /// </summary>
  [Serializable]
  public class FileAssociations : NamedList<FileAssociationItem>
  {
    #region Конструктор

    private FileAssociations(bool isReadOnly)
    {
      if (isReadOnly)
        SetReadOnly();
    }

    #endregion

    #region Свойства

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

    /// <summary>
    /// Возвращает первый элемент списка, или null.
    /// </summary>
    public FileAssociationItem FirstItem
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[0];
      }
    }

    #endregion

    #region Общедоступные методы

    /// <summary>
    /// Возвращает файловую ассоциацию по значению свойства <see cref="FileAssociationItem.DisplayName"/>.
    /// Если <paramref name="displayName"/> - пустая строка или задает несуществующее значение, возвращается значение <see cref="FirstItem"/>.
    /// Если текущий объект не содержит ассоциаций, возвращается null.
    /// </summary>
    /// <param name="displayName"></param>
    /// <returns>Файловая ассоциаиция</returns>
    public FileAssociationItem GetByDisplayName(string displayName)
    {
      if (!String.IsNullOrEmpty(displayName))
      {
        for (int i = 0; i < Count; i++)
        {
          if (String.Equals(this[i].DisplayName, displayName, StringComparison.Ordinal))
            return this[i];
        }
      }
      return FirstItem;
    }

    #endregion

    #region Методы для добавления

    private bool ExtContains(FileAssociationItem item)
    {
      return ExtIndexOf(item) >= 0;
    }

    private int ExtIndexOf(FileAssociationItem item)
    {
      for (int i = 0; i < Count; i++)
      {
        if (String.Equals(this[i].ProgId, item.ProgId, StringComparison.OrdinalIgnoreCase))
          return i;
        if (this[i].ProgramPath == item.ProgramPath)
          return i;
      }
      return -1;
    }

    private bool _FirstItemAdded;

    private void ExtAdd(FileAssociationItem item, bool isFirst)
    {
      if (item == null)
        return;
      if (ExtContains(item))
        return;

      if (isFirst && (!_FirstItemAdded))
      {
        base.Insert(0, item);
        _FirstItemAdded = true;
      }
      else
        base.Add(item);
    }

    #endregion

    #region Статические свойства

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

    #endregion

    #region Получение для расширения файла

    /// <summary>
    /// Создает список ассоциаций для заданного расширения файла.
    /// Реализовано для Windows и Linux. 
    /// Для других операционных систем возвращает пустой список.
    /// Возвращаемый список ассоциаций переведен в режим "Только чтение".
    /// 
    /// Этот метод выполняет опрос системы при каждом вызове.
    /// Используйте свойство EFPApp.FileExtAssociations (ExtForms.dll), которое поддерживает буферизацию.
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
    /// Используйте свойство EFPApp.FileAssociations.ShowDirectory (ExtForms.dll).
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
        }

        return faItems;
      }

      private static void FromFileExtensionExplorer(string fileExt, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key2 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\UserChoice"];
        if (key2 != null)
        {
          FileAssociationItem item2 = GetProgIdItem(fileExt, tree, DataTools.GetString(key2.GetValue("progid")), key2.Name);
          faItems.ExtAdd(item2, true);
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
            FileAssociationItem item = GetProgIdItem(fileExt, tree, progId3, key3.Name);
            faItems.ExtAdd(item, false);
          }
        }
      }

      private static void FromFileExtensionsHKCR(string fileExt, FileAssociations faItems, RegistryTree2 tree)
      {
        RegistryKey2 key1 = tree[@"HKEY_CLASSES_ROOT\" + fileExt];
        if (key1 != null)
        {
          if (IsWindowsXP_or_Newer)
          {
            RegistryKey2 keyOWPI = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithProgIds"];
            if (keyOWPI != null)
            {
              string[] aProgIds = keyOWPI.GetValueNames();
              for (int i = 0; i < aProgIds.Length; i++)
              {
                FileAssociationItem item = GetProgIdItem(fileExt, tree, aProgIds[i], keyOWPI.Name);
                faItems.ExtAdd(item, false);
              }
            }
          }
          else
          {
            // Использовалось Для версий до Windows-XP
            RegistryKey2 keyOWL = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithList"];
            if (keyOWL != null)
            {
              string[] aProgIds = keyOWL.GetSubKeyNames(); // а не value names
              for (int i = 0; i < aProgIds.Length; i++)
              {
                FileAssociationItem item = GetProgIdItem(fileExt, tree, aProgIds[i], keyOWL.Name);
                faItems.ExtAdd(item, false);
              }
            }
          }

          string progId = DataTools.GetString(key1.GetValue(String.Empty)); // Например, для ".txt" это "txtFile"
          FileAssociationItem item0 = GetProgIdItem(fileExt, tree, progId, key1.Name);
          faItems.ExtAdd(item0, true);
        }
      }

      private static FileAssociationItem GetProgIdItem(string fileExt, RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (String.IsNullOrEmpty(progId))
          return null;

        RegistryKey2 keyProgId = tree[@"HKEY_CLASSES_ROOT\" + progId];
        if (keyProgId == null)
          return GetProgIdItemForExeFile(fileExt, tree, progId, infoSourceString);

        return DoGetProgIdItem(fileExt, tree, progId, keyProgId, infoSourceString);
      }

      private static FileAssociationItem DoGetProgIdItem(string fileExt, RegistryTree2 tree, string progId, RegistryKey2 keyProgId, string infoSourceString)
      {
        string cmd = tree.GetString(keyProgId.Name + @"\shell\open\command", String.Empty);
        if (String.IsNullOrEmpty(cmd))
        {
          cmd = tree.GetString(keyProgId.Name + @"\shell\edit\command", String.Empty); // 21.09.2023
          if (String.IsNullOrEmpty(cmd))
            return null;
        }

        string fileName, arguments;
        if (!SplitFileNameAndArgs(cmd, out fileName, out arguments))
          return null;

        fileName = Environment.ExpandEnvironmentVariables(fileName);
        AbsPath path;
        if (fileName.IndexOf('\\') >= 0)
          path = AbsPath.Create(fileName);
        else
          path = FileTools.FindExecutableFilePath(fileName); // 11.05.2023
        if (path.IsEmpty)
          return null; // 25.01.2019
        if (!System.IO.File.Exists(path.Path))
          return null;

        if (!IsWineBrowser(path)) // 21.06.2024
        {
          if (arguments.IndexOf(@"%1", StringComparison.Ordinal) < 0)
            // Обмен с помощью DDE не реализован
            return null;
        }

        string displayName = String.Empty;

        AbsPath iconPath = path;
        int iconIndex = 0;

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
            {
              displayName = FileAssociationItem.GetDisplayName(path2);
              iconPath = path2;
            }
          }
        }

        // Убрано 21.09.2023
        //RegistryKey2 keyDefIcon = tree[keyProgId.Name + @"\DefaultIcon"];
        //if (keyDefIcon != null)
        //{
        //  string s = DataTools.GetString(keyDefIcon.GetValue(String.Empty));
        //  if (!(s == "%1" || s == "\"%1\""))
        //  {
        //    ParseIconInfo(s, out iconPath, out iconIndex);
        //  }
        //}


        // Специальная реализация для Mono+Wine
        if (IsWineBrowser(path) &&
          (!String.IsNullOrEmpty(fileExt)))
        {
          //Console.WriteLine("winebrowser.exe. fileExt=" + fileExt);
          string mimeType = tree.GetString(@"HKEY_CLASSES_ROOT\" + fileExt, "Content Type");
          //Console.WriteLine("mimeType==" + mimeType);
          if (!String.IsNullOrEmpty(mimeType))
          {
            string clsId = tree.GetString(@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType, "Content Type");
            //Console.WriteLine("clsId==" + clsId);
            if (!String.IsNullOrEmpty(clsId))
            {
              string defIcon = tree.GetString(@"HKEY_CLASSES_ROOT\CLSID\" + clsId + @"\DefaultIcon", String.Empty);
              //Console.WriteLine("defIcon==" + defIcon);
              ParseIconInfo(defIcon, ref iconPath, ref iconIndex);
              //Console.WriteLine("IconPath=\"" + iconPath+"\", IconIndex="+iconIndex);
            }
          }
        }

        return new FileAssociationItem(progId, path, arguments, displayName, iconPath, iconIndex, false,
          infoSourceString + Environment.NewLine + keyProgId.Name + @"\shell\open\command");
      }

      private static bool IsWineBrowser(AbsPath path)
      {
        return EnvironmentTools.IsMono &&
         String.Equals(path.FileName, "winebrowser.exe", StringComparison.OrdinalIgnoreCase);
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
          // 11.10.2023 - или запятой
          int p = arguments.IndexOfAny(new char[] { ' ', ',' });
          if (p >= 0)
            return arguments.Substring(0, p);
          else
            return arguments;
        }
      }

      private static FileAssociationItem GetProgIdItemForExeFile(string fileExt, RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (!progId.EndsWith(".EXE", StringComparison.OrdinalIgnoreCase))
          return null;

        FileAssociationItem faItem = GetProgIdItemForExeFileHKCRApplications(fileExt, tree, progId, infoSourceString);
        if (faItem != null)
          return faItem;
        else
          return GetProgIdItemForExeFileAppPathes(tree, progId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileHKCRApplications(string fileExt, RegistryTree2 tree, string progId, string infoSourceString)
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

        return DoGetProgIdItem(fileExt, tree, progId, keyProgId, infoSourceString);
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

      private static void ParseIconInfo(string iconInfo, ref AbsPath iconPath, ref int iconIndex)
      {
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
        else
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
              FileAssociationItem item = GetProgIdItem(String.Empty, tree, progId, key1.Name);
              faItems.ExtAdd(item, false);
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

        FileAssociationItem item = new FileAssociationItem("Explorer", path, "%1", "Windows Explorer",
          path, 0, false, "Fixed");
        faItems.ExtAdd(item, true);

        return faItems;
      }

      #endregion

      #region Вспомогательные методы и свойства

      private static bool SplitFileNameAndArgs(string commandLine, out string fileName, out string arguments)
      {
        if (String.IsNullOrEmpty(commandLine))
        {
          fileName = String.Empty;
          arguments = String.Empty;
          return false;
        }

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
          {
            arguments = String.Empty;
            return false;
          }

          if (pEndQuota < (commandLine.Length - 1))
            arguments = commandLine.Substring(pEndQuota + 1).TrimStart(' ');
          else
            arguments = String.Empty;
          return true;
        }
        else
        {
          // Имя программы от аргументов отделяется пробелом
          // 15.09.2023: Может быть командная строка без кавычек, например: C:\Program Files\AlterOffice\program\atext.exe -o "%1"
          // 11.10.2023: Может быть командная строка без пути: rundll32.exe C:\WINDOWS\system32\shimgvw.dll,ImageView_Fullscreen %1
          // 17.10.2023: Может быть командная строка с путем: C:\Windows\System32\rundll32.exe "C:\Program Files (x86)\Windows Photo Viewer\PhotoViewer.dll", ImageView_Fullscreen %1
          // Чтобы не делать сложную логику, пытаемся выполнить разбиение по всем пробелам подряд
          int p1 = -1;
          while (true)
          {
            int p2 = commandLine.IndexOf(' ', p1 + 1);
            if (p2 < 0)
              break;

            fileName = commandLine.Substring(0, p2);
            arguments = commandLine.Substring(p2 + 1);
            p1 = p2 + 1;
            if (fileName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
              break;
            AbsPath path;
            try
            {
              path = new AbsPath(fileName);
            }
            catch
            {
              continue;
            }
            if (path.ContainsExtension(".exe") || path.ContainsExtension(".dll"))
              return true;
          }

          // нет пробела или неправильное имя
          fileName = commandLine;
          arguments = String.Empty;
          return true;
        }
      }

      public static bool IsWindowsXP_or_Newer
      {
        get
        {
          if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            return false;
          if (Environment.OSVersion.Version.Major < 5)
            return false;
          if (Environment.OSVersion.Version.Major > 5)
            return true;
          // Windows-2000: 5.0
          // Windows-XP: 5.1
          return Environment.OSVersion.Version.Minor > 0;
        }
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
      private static readonly object _SyncRoot = new object();

      /// <summary>
      /// INI-файл, содержащий секцию [MIME Cache] и пары mimetype=список_файлов_desktop.
      /// Если файла нет, содержит пустой путь
      /// </summary>
      private static readonly AbsPath _MimeinfoFilePath = TryGetFiles(new string[] {
        "/usr/share/applications/mimeinfo.cache" });

      /// <summary>
      /// INI-файл, содержащий секцию [Default Applications], в котором есть пары mimetype=список_файлов_desktop.
      /// Порядок desktop-файлов содержит пользовательские предпочтения.
      /// </summary>
      private static readonly AbsPath _UserFilePath = TryGetFiles(new string[] {
        "~/.local/share/applications/defaults.list",
        "~/.config/mimeapps.list" /* 21.09.2023 */});

      /// <summary>
      /// Время модификации файла "/usr/share/applications/mimeinfo.cache"
      /// </summary>
      private static DateTime _MimeinfoFileTime;

      /// <summary>
      /// Время модификации файла "~/.local/share/applications/defaults.list"
      /// </summary>
      private static DateTime _UserFileTime;

      private static AbsPath TryGetFiles(string[] a)
      {
        try
        {
          for (int i = 0; i < a.Length; i++)
          {
            AbsPath p = new AbsPath(a[i]);
            if (System.IO.File.Exists(p.Path))
              return p;
          }
        }
        catch { }
        return AbsPath.Empty;
      }

      /// <summary>
      /// Таблица соответствий mime-типов и desktop-файлов.
      /// Ключ - MIME-тип, значение - список ярлыков .desktop (через точку с запятой)
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
        // В массиве могут быть повторяющиеся элементы, так как добавлялись пользовательские предпочтения.
        SingleScopeStringList lstDesktopFiles = new SingleScopeStringList(aDesktopFiles.Length, false);
        for (int i = 0; i < aDesktopFiles.Length; i++)
        {
          if (aDesktopFiles[i].Length > 0)
            lstDesktopFiles.Add(aDesktopFiles[i]);
        }
        FileAssociations faItems = new FileAssociations(false);
        for (int i = 0; i < lstDesktopFiles.Count; i++)
        {
          FileAssociationItem item = CreateFromDesktopFile(lstDesktopFiles[i]);
          faItems.ExtAdd(item, false);
        }
        faItems.SetReadOnly();
        return faItems;
      }

      /// <summary>
      /// Список путей, по которым нужно искать значки приложений
      /// </summary>
      private static AbsPath[] SearchIconPathes
      {
        get
        {
          if (_SearchIconPathes == null)
            _SearchIconPathes = GetSearchIconPathes();
          return _SearchIconPathes;
        }
      }
      private static AbsPath[] _SearchIconPathes = null;

      private static AbsPath[] GetSearchIconPathes()
      {
        #region Список тем, откуда в порядке очереди берутся значки

        List<string> lstThemes = new List<string>();
        lstThemes.Add("hicolor");

        #endregion

        #region Список каталогов, в которых могут быть темы

        // См. https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html#directory_layout - порядок просмотра каталогов
        // См. https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html#variables - значения по умолчанию, когда переменные окружения не объявлены

        SingleScopeList<AbsPath> lstBaseDirs = new SingleScopeList<AbsPath>();
        AbsPath p;

        // #1
        p = new AbsPath("~/.icons");
        if (System.IO.Directory.Exists(p.Path))
          lstBaseDirs.Add(p);

        // #2
        if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("$XDG_DATA_HOME")))
          p = new AbsPath("~/.local/share");
        else
          p = new AbsPath(Environment.GetEnvironmentVariable("$XDG_DATA_HOME"));
        if (System.IO.Directory.Exists(p.Path))
          lstBaseDirs.Add(p);
        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("$XDG_DATA_DIRS")))
        {
          string[] a = Environment.GetEnvironmentVariable("$XDG_DATA_DIRS").Split(':');
          for (int i = 0; i < a.Length; i++)
          {
            p = new AbsPath(a[i]);
            if (System.IO.Directory.Exists(p.Path))
              lstBaseDirs.Add(p);
          }
        }

        // #3
        p = new AbsPath("/usr/share/pixmaps");
        if (System.IO.Directory.Exists(p.Path))
          lstBaseDirs.Add(p);

        // #4 Агеев
        p = new AbsPath("/usr/share/icons");
        if (System.IO.Directory.Exists(p.Path))
          lstBaseDirs.Add(p);

        #endregion

        #region Определение каталогов

        string[] sizeSubNames = new string[] { "32x32", "28x28", "36x36", "24x24", "22x22", "20x20", "16x16", "48x48" };

        List<AbsPath> lst = new List<AbsPath>();

        foreach (string theme in lstThemes)
        {
          foreach (string sizeSubName in sizeSubNames)
          {
            foreach (AbsPath baseDir in lstBaseDirs)
            {
              p = new AbsPath(baseDir, theme, sizeSubName, "apps");
              if (System.IO.Directory.Exists(p.Path))
                lst.Add(p);
            }
          }
        }

        #endregion

        return lst.ToArray();
      }

      private static FileAssociationItem CreateFromDesktopFile(string desktopFileName)
      {
        if (String.IsNullOrEmpty(desktopFileName))
          return null;
        if (!desktopFileName.EndsWith(".desktop", StringComparison.Ordinal))
          desktopFileName += ".desktop";
        AbsPath desktopFilePath = new AbsPath("/usr/share/applications/" + desktopFileName);
        if (!System.IO.File.Exists(desktopFilePath.Path))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Desktop entry file not found: " + DesktopFilePath.Path);
#endif
          return null;
        }

        IniFile file = new IniFile(true);
        file.Load(desktopFilePath);
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

        programPath = FileTools.GetRealPath(programPath); // 18.09.2023

        AbsPath iconPath = AbsPath.Empty;
        string sIcon = file["Desktop Entry", "Icon"];

#if USE_TRACE
        //System.Diagnostics.Trace.WriteLine("Icon=" + sIcon);
#endif
        if (!String.IsNullOrEmpty(sIcon))
        {
          try
          {
            if (sIcon[0] == '~' || sIcon[0] == '/')
              iconPath = new AbsPath(sIcon); // 21.09.2023
            else
            {

              //DoGetIconPath(ref iconPath, "~/.icons", sIcon); // 21.09.2023
              //DoGetIconPath(ref iconPath, "~/.local/share/icons", sIcon); // 21.09.2023
              //DoGetIconPath(ref iconPath, "/usr/share/icons", sIcon);

              foreach (AbsPath dir in SearchIconPathes)
              {
                AbsPath p = new AbsPath(dir, sIcon + ".png");
                if (System.IO.File.Exists(p.Path))
                {
                  iconPath = p;
                  break;
                }
              }

            }
          }
          catch { }
        }

        return new FileAssociationItem(desktopFileName, programPath, arguments, displayName, iconPath, 0, false, desktopFilePath.Path);
      }

      //private static void DoGetIconPath(ref AbsPath iconPath, string dir, string sIcon)
      //{
      //  if (!iconPath.IsEmpty)
      //    return; // нашли при предыдущем вызове
      //  if (!System.IO.Directory.Exists(dir))
      //    return; // нет каталога

      //  string[] a = System.IO.Directory.GetFiles(dir, sIcon + ".png", System.IO.SearchOption.AllDirectories);
      //  if (a.Length > 0)
      //    iconPath = new AbsPath(a[0]);
      //}

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
        if (!_MimeinfoFilePath.IsEmpty)
        {
          if (System.IO.File.GetLastWriteTime(_MimeinfoFilePath.Path) != _MimeinfoFileTime)
            return true;
        }

        if (!_UserFilePath.IsEmpty)
        {
          if (System.IO.File.GetLastWriteTime(_UserFilePath.Path) != _UserFileTime)
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

        if (!_MimeinfoFilePath.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + MimeinfoFilePath);
#endif
          FreeLibSet.IO.IniFile ini = new IniFile(true);
          ini.Load(_MimeinfoFilePath);
          foreach (IniKeyValue pair in ini.GetKeyValues("MIME Cache"))
            _MimeDesktopFiles[pair.Key] = pair.Value;
          _MimeinfoFileTime = System.IO.File.GetLastWriteTime(_MimeinfoFilePath.Path);
        }

        #endregion

        #region Пользовательские настройки

        if (!_UserFilePath.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + DefaultsListFilePath);
#endif
          FreeLibSet.IO.IniFile ini = new IniFile(true);
          ini.Load(_UserFilePath);
          foreach (IniKeyValue pair in ini.GetKeyValues("Default Applications"))
          {
            if (_MimeDesktopFiles.ContainsKey(pair.Key))
              _MimeDesktopFiles[pair.Key] = pair.Value + ";" + _MimeDesktopFiles[pair.Key]; // пользовательские предпочтения вперед
            else
              _MimeDesktopFiles.Add(pair.Key, pair.Value);
          }
          _UserFileTime = System.IO.File.GetLastWriteTime(_UserFilePath.Path);
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

      #region Вспомогательные методы

      // Этот метод отличается от того, который в Windows!

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
    }

    #endregion
  }
}
