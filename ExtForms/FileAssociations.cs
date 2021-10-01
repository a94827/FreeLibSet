/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

//#define USE_TRACE // трассировка в пределах этого файла

using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.IO;
using System.Diagnostics;
using Microsoft.Win32;
using AgeyevAV.Win32;
using AgeyevAV.Logging;

namespace AgeyevAV.Shell
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
        _DisplayName=GetDisplayName(_ProgramPath);
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
      string DisplayName = String.Empty;
      if (System.IO.File.Exists(programPath.Path))
      {
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(programPath.Path);
        if (!String.IsNullOrEmpty(fvi.FileDescription))
          DisplayName = fvi.FileDescription;
      }
      if (String.IsNullOrEmpty(DisplayName))
        DisplayName = programPath.FileNameWithoutExtension;

      return DisplayName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор ProgId
    /// </summary>
    public string ProgId { get { return _ProgId; } }
    private string _ProgId;

    /// <summary>
    /// Путь к выполняемому файлу приложения
    /// </summary>
    public AbsPath ProgramPath { get { return _ProgramPath; } }
    private AbsPath _ProgramPath;

    /// <summary>
    /// Аргументы командной строки для запуска приложения.
    /// Аргумент "%1" заменяется на путь к файлу
    /// </summary>
    public string Arguments { get { return _Arguments; } }
    private string _Arguments;

    //public string CommandLine { get { return FCommandLine; } set { FCommandLine = value; } }

    /// <summary>
    /// Отображаемое имя программы для команд "Открыть с помощью"
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    /// <summary>
    /// Путь к файлу в котором содержится значок.
    /// Может быть не задан
    /// </summary>
    public AbsPath IconPath { get { return _IconPath; } }
    private AbsPath _IconPath;

    /// <summary>
    /// Индекс значка в файле.
    /// См. описание функции Windows ExtractIcon()
    /// </summary>
    public int IconIndex { get { return _IconIndex; } }
    private int _IconIndex;
    /// <summary>
    /// Если true, то при подстановке имени файла в командную строку будет использоваться форма "file:///"
    /// </summary>
    public bool UserURL { get { return true; } }
    private bool _UseURL;

#if DEBUG
    /// <summary>
    /// Дополнительная информация, как была найдена эта копия (ключ реестра или имя переменной окружения).
    /// Это свойство существует только в отладочном режиме
    /// </summary>
    public string InfoSourceString { get { return _InfoSourceString; } }
    private string _InfoSourceString;
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
    private OpenWithItemList _OpenWithItems;

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
        FileAssociations Items = DoFromFileExtension(fileExt);
        Items.SetReadOnly();
        return Items;
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
      FileAssociations FA = new FileAssociations(false);
      FA.Exception = e;
      FA.SetReadOnly();
      return FA;
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
          string MimeType = Linux.GetMimeTypeFromFileExtension(fileExt);
          if (MimeType.Length == 0)
            return Empty;
          else
            return Linux.FromMimeType(MimeType);
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
        FileAssociations Items = DoFromMimeType(mimeType);
        Items.SetReadOnly();
        return Items;
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
        FileAssociations Items = DoFromDirectory();
        Items.SetReadOnly();
        return Items;
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
        FileAssociations FA = new FileAssociations(false);

        using (RegistryTree2 Tree = new RegistryTree2(true))
        {
          FromFileExtensionExplorer(fileExt, FA, Tree);
          FromFileExtensionsHKCR(fileExt, FA, Tree); 

          if (FA.OpenItem == null && FA.OpenWithItems.Count > 0)
            FA.OpenItem = FA.OpenWithItems[0];
          else if (FA.OpenItem != null)
          {
            if (!FA.OpenWithContains(FA.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              FA.OpenWithItems.Add(FA.OpenItem);
          }
        }

        return FA;
      }

      private static void FromFileExtensionExplorer(string fileExt, FileAssociations fa, RegistryTree2 tree)
      {
        RegistryKey2 Key2 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\UserChoice"];
        if (Key2 != null)
        {
          FileAssociationItem Item2 = GetProgIdItem(tree, DataTools.GetString(Key2.GetValue("progid")), Key2.Name);
          if (Item2 != null)
          {
            fa.OpenItem = Item2;
            if (!fa.OpenWithContains(Item2))
              fa.OpenWithItems.Insert(0, Item2);
          }
        }

        // Теперь - OpenWithList
        RegistryKey2 Key3 = tree[@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileExt + @"\OpenWithList"];
        if (Key3 != null)
        {
          string MRUList = DataTools.GetString(Key3.GetValue("MRUList")); // кодируется отдельными буквами
          for (int i = 0; i < MRUList.Length; i++)
          {
            string ValName = new string(MRUList[i], 1); // строка из одной буквы
            string ProgId3 = DataTools.GetString(Key3.GetValue(ValName));
            FileAssociationItem Item = GetProgIdItem(tree, ProgId3, Key3.Name);
            if (Item != null && (!fa.OpenWithContains(Item)))
              fa._OpenWithItems.Add(Item);
          }
        }
      }

      private static void FromFileExtensionsHKCR(string fileExt, FileAssociations fa, RegistryTree2 tree)
      {
        RegistryKey2 Key1 = tree[@"HKEY_CLASSES_ROOT\" + fileExt];
        if (Key1 != null)
        {
          RegistryKey2 KeyOWPI = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithProgIds"];
          if (KeyOWPI != null)
          {
            string[] aProgIds = KeyOWPI.GetValueNames();
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem Item = GetProgIdItem(tree, aProgIds[i], KeyOWPI.Name);
              if (Item != null && (!fa.OpenWithContains(Item)))
                fa._OpenWithItems.Add(Item);
            }
          }
          RegistryKey2 KeyOWL = tree[@"HKEY_CLASSES_ROOT\" + fileExt + @"\OpenWithList"];
          if (KeyOWL != null)
          {
            string[] aProgIds = KeyOWL.GetSubKeyNames(); // а не value names
            for (int i = 0; i < aProgIds.Length; i++)
            {
              FileAssociationItem Item = GetProgIdItem(tree, aProgIds[i], KeyOWL.Name);
              if (Item != null && (!fa.OpenWithContains(Item)))
                fa._OpenWithItems.Add(Item);
            }
          }

          FileAssociationItem Item0 = GetProgIdItem(tree, DataTools.GetString(Key1.GetValue(String.Empty)), Key1.Name);
          if (Item0 != null)
          {
            int p = fa.OpenWithIndexOf(Item0);
            if (p < 0)
            {
              fa.OpenWithItems.Insert(0, Item0);
              p = 0;
            }
            fa.OpenItem = fa.OpenWithItems[p]; // а не Item0
          }
        }
      }

      private static FileAssociationItem GetProgIdItem(RegistryTree2 tree, string progId, string infoSourceString)
      {
        if (String.IsNullOrEmpty(progId))
          return null;

        RegistryKey2 KeyProgId = tree[@"HKEY_CLASSES_ROOT\" + progId];
        if (KeyProgId == null)
          return GetProgIdItemForExeFile(tree, progId, infoSourceString);

        return DoGetProgIdItem(tree, progId, KeyProgId, infoSourceString);
      }

      private static FileAssociationItem DoGetProgIdItem(RegistryTree2 tree, string progId, RegistryKey2 keyProgId, string infoSourceString)
      {
        string Cmd = tree.GetString(keyProgId.Name + @"\shell\open\command", String.Empty);
        if (String.IsNullOrEmpty(Cmd))
          return null;

        if (Cmd.IndexOf(@"%1", StringComparison.Ordinal) < 0)
          // Обмен с помощью DDE не реализован
          return null;

        string FileName, Arguments;
        if (!SplitFileNameAndArgs(Cmd, out FileName, out Arguments))
          return null;

        FileName = Environment.ExpandEnvironmentVariables(FileName);
        AbsPath Path = AbsPath.Create(FileName);
        if (Path.IsEmpty)
          return null; // 25.01.2019
        if (!System.IO.File.Exists(Path.Path))
          return null;

        string DisplayName = String.Empty;

        if (Path.FileName.ToLowerInvariant() == "rundll32.exe")
        {
          // 22.09.2019
          // Извлекаем данные из аргументов 

          string FileName2 = GetFileNameFromArgs(Arguments);
          if (!String.IsNullOrEmpty(FileName2))
          { 
            FileName2 = Environment.ExpandEnvironmentVariables(FileName2);
            AbsPath Path2 = AbsPath.Create(FileName2);
            if (!Path2.IsEmpty)
              DisplayName = FileAssociationItem.GetDisplayName(Path2);
          }
        }

        AbsPath IconPath = AbsPath.Empty;
        int IconIndex = 0;
        RegistryKey2 KeyDefIcon = tree[keyProgId.Name + @"\DefaultIcon"];
        if (KeyDefIcon != null)
        {
          string s = DataTools.GetString(KeyDefIcon.GetValue(String.Empty));
          if (!(s == "%1" || s == "\"%1\""))
          {
            ParseIconInfo(s, out IconPath, out IconIndex);
          }
        }
        if (IconPath.IsEmpty)
        {
          IconPath = Path;
          IconIndex = 0;
        }

        return new FileAssociationItem(progId, Path, Arguments, DisplayName, IconPath, IconIndex, false,
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
          int p=s.IndexOf('\"');
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

        FileAssociationItem FAItem = GetProgIdItemForExeFileHKCRApplications(tree, progId, infoSourceString);
        if (FAItem != null)
          return FAItem;
        else
          return GetProgIdItemForExeFileAppPathes(tree, progId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileHKCRApplications(RegistryTree2 tree, string progId, string infoSourceString)
      {
        RegistryKey2 KeyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        if (KeyProgId == null && progId.IndexOf('\\') < 0)
        {
          // Может быть задано просто имя EXE-файла, например, "notepad.exe", тогда его надо искать
          // в подразделе "Applications"
          // Идентификатор приложения тоже надо изменить, иначе в списке будет два блокнота
          progId = @"Applications\" + progId;
          KeyProgId = tree[@"HKEY_CLASSES_ROOT\Applications\" + progId];
        }
        if (KeyProgId == null)
          return null;

        return DoGetProgIdItem(tree, progId, KeyProgId, infoSourceString);
      }

      private static FileAssociationItem GetProgIdItemForExeFileAppPathes(RegistryTree2 tree, string progId, string infoSourceString)
      {
        try
        {
          // Последняя попытка - найти путь к приложению
          string KeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + progId;
          string FilePath = tree.GetString(KeyName, String.Empty);
          AbsPath Path = AbsPath.Create(FilePath); // 25.01.2019
          if (Path.IsEmpty)
            return null;
          if (!System.IO.File.Exists(Path.Path))
            return null;


          return new FileAssociationItem(progId, Path, "\"%1\"", String.Empty,
            Path, 0,
            tree.GetBool(KeyName, "useURL"),
            infoSourceString + Environment.NewLine + KeyName);
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

        string FileName;
        int p = iconInfo.LastIndexOf(',');
        if (p >= 0)
        {
          FileName = iconInfo.Substring(0, p);
          string sIconIndex = iconInfo.Substring(p + 1);
          int.TryParse(sIconIndex, out iconIndex);
        }
        else
        {
          FileName = iconInfo;
          iconIndex = 0;
        }
        FileName = Environment.ExpandEnvironmentVariables(FileName);
        if (FileName.IndexOf(System.IO.Path.DirectorySeparatorChar) < 0) // имя файла без пути
        {
          // 13.12.2018 Пытаемся найти в системном каталоге
          iconPath = AbsPath.Create(AbsPath.Create(Environment.SystemDirectory), FileName);
          //if (!System.IO.File.Exists(IconPath.Path))
          //  IconPath = FileTools.FindExecutableFilePath(FileName); 
        }
        if (iconPath.IsEmpty)
          iconPath = AbsPath.Create(FileName);
      }

      #endregion

      #region FromMimeType

      internal static FileAssociations FromMimeType(string mimeType)
      {
        FileAssociations FA = new FileAssociations(false);

        using (RegistryTree2 Tree = new RegistryTree2(true))
        {
          FromMimeTypeHKCR_MIME(mimeType, FA, Tree);

          if (FA.OpenItem == null && FA.OpenWithItems.Count > 0)
            FA.OpenItem = FA.OpenWithItems[0];
          else if (FA.OpenItem != null)
          {
            if (!FA.OpenWithContains(FA.OpenItem))
              //FA.OpenWithItems.Insert(0, FA.OpenItem);
              FA.OpenWithItems.Add(FA.OpenItem);
          }
        }

        return FA;
      }

      private static void FromMimeTypeHKCR_MIME(string mimeType, FileAssociations fa, RegistryTree2 tree)
      {
        RegistryKey2 Key1 = tree[@"HKEY_CLASSES_ROOT\MIME\Database\Content Type\" + mimeType];
        if (Key1 != null)
        {
          string ClsId = DataTools.GetString(Key1.GetValue("CLSID"));
          if (!String.IsNullOrEmpty(ClsId))
          {
            RegistryKey2 Key2 = tree[@"HKEY_CLASSES_ROOT\CLSID\" + ClsId + @"\ProgId"];
            if (Key2 != null)
            {
              string ProgId = DataTools.GetString(Key2.GetValue(String.Empty));
              FileAssociationItem FAItem = GetProgIdItem(tree, ProgId, Key1.Name);
              if (FAItem != null)
                fa.OpenWithItems.Add(FAItem);
            }
          }
        }
      }

      #endregion

      #region Ассоциации для каталога

      internal static FileAssociations FromDirectory()
      {
        // TODO: поиск замены explorer.exe

        FileAssociations FA = new FileAssociations(false);
        AbsPath Path = FileTools.FindExecutableFilePath("explorer.exe");
        if (Path.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("explorer.exe not found");
#endif
          return FA;
        }

        FileAssociationItem FAItem = new FileAssociationItem("Explorer", Path, "%1", "Windows Explorer",
          Path, 0, false, "Fixed");
        FA.OpenWithItems.Add(FAItem);
        FA.OpenItem = FAItem;
        return FA;
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
              System.Xml.XmlDocument XmlDoc = new System.Xml.XmlDocument();
              XmlDoc.Load(aFiles[i]);
              System.Xml.XmlNamespaceManager NmSpcMan = new System.Xml.XmlNamespaceManager(XmlDoc.NameTable);
              NmSpcMan.AddNamespace("Def", @"http://www.freedesktop.org/standards/shared-mime-info");
              System.Xml.XmlNodeList mtnodes = XmlDoc.SelectNodes("Def:mime-info/Def:mime-type", NmSpcMan);
#if USE_TRACE
              System.Diagnostics.Trace.WriteLine("  mime-type count=" + mtnodes.Count.ToString());
#endif
              foreach (System.Xml.XmlNode mtnode in mtnodes)
              {
                string mimetype = GetAttrStr(mtnode, "type");
                if (mimetype.Length == 0)
                  continue;

                foreach (System.Xml.XmlNode globnode in mtnode.SelectNodes("Def:glob", NmSpcMan))
                {
                  string pattern = GetAttrStr(globnode, "pattern");
                  if (!pattern.StartsWith("*."))
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
        FileAssociations FAs;
        lock (_SyncRoot)
        {
          if (NeedsRecreateMimeDesktopFiles())
            CreateMimeDesktopFiles();

          FAs = DoFromMimeType(mimeType);
        }
        return FAs;
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
        FileAssociations FAs = new FileAssociations(false);
        for (int i = 0; i < aDesktopFiles.Length; i++)
        {
          FileAssociationItem FA = CreateFromDesktopFile(aDesktopFiles[i]);
          if (FA != null)
            FAs.OpenWithItems.Add(FA);
        }
        if (FAs.OpenWithItems.Count > 0)
          FAs.OpenItem = FAs.OpenWithItems[0];
        FAs.SetReadOnly();
        return FAs;
      }

      private static FileAssociationItem CreateFromDesktopFile(string desktopFileName)
      {
        if (String.IsNullOrEmpty(desktopFileName))
          return null;
        if (!desktopFileName.EndsWith(".desktop"))
          desktopFileName += ".desktop";
        AbsPath DesktopFilePath = new AbsPath("/usr/share/applications/" + desktopFileName);
        if (!System.IO.File.Exists(DesktopFilePath.Path))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Desktop entry file not found: " + DesktopFilePath.Path);
#endif
          return null;
        }

        IniFile File = new IniFile(true);
        File.Load(DesktopFilePath);
        string DisplayName = File["Desktop Entry", "Name[" + LanguageStr + "]"]; // Name[ru]
        if (String.IsNullOrEmpty(DisplayName))
          DisplayName = File["Desktop Entry", "Name"];
        if (String.IsNullOrEmpty(DisplayName))
          DisplayName = desktopFileName;
        string sExec = File["Desktop Entry", "Exec"];
        if (String.IsNullOrEmpty(sExec))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("There is no \"Exec\" key in " + DesktopFilePath.Path);
#endif
          return null;
        }
        string FileName;
        string Arguments;
        if (!SplitFileNameAndArgs(sExec, out FileName, out Arguments))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot split \"Exec\" key=\"" + sExec + "\" found in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath ProgramPath = FileTools.FindExecutableFilePath(FileName);
        if (ProgramPath.IsEmpty)
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("Cannot find executable file \"" + FileName + "\", defined in " + DesktopFilePath.Path);
#endif
          return null;
        }

        AbsPath IconPath = AbsPath.Empty;
        string sIcon = File["Desktop Entry", "Icon"];

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
              IconPath = new AbsPath(a[0]);
          }
          catch { }
        }

        return new FileAssociationItem(desktopFileName, ProgramPath, Arguments, DisplayName, IconPath, 0, false, DesktopFilePath.Path);
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
          AgeyevAV.IO.IniFile Ini = new IniFile(true);
          Ini.Load(new AbsPath(_MimeinfoCacheFilePath));
          foreach (IniKeyValue Pair in Ini.GetKeyValues("MIME Cache"))
            _MimeDesktopFiles[Pair.Key] = Pair.Value;
          _MimeinfoCacheFileTime = System.IO.File.GetLastWriteTime(_MimeinfoCacheFilePath);
        }

        #endregion

        #region Пользовательские настройки

        if (System.IO.File.Exists(_DefaultsListFilePath))
        {
#if USE_TRACE
          System.Diagnostics.Trace.WriteLine("  from " + DefaultsListFilePath);
#endif
          AgeyevAV.IO.IniFile Ini = new IniFile(true);
          Ini.Load(new AbsPath(_DefaultsListFilePath));
          foreach (IniKeyValue Pair in Ini.GetKeyValues("Default Applications"))
            _MimeDesktopFiles[Pair.Key] = Pair.Value;
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
              char NextChar = commandLine[i + 1];
              if (NextChar == '\"') // удвоенная кавычка
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
