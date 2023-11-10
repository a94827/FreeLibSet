// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using FreeLibSet.Win32;
using FreeLibSet.Core;

namespace FreeLibSet.Shell
{
  /// <summary>
  /// Поддержка для OpenOffice / LibreOffice.
  /// </summary>
  public static class OpenOfficeTools
  {
    #region Список установленных копий

    /// <summary>
    /// Список обнаруженных копий OpenOffice и LibreOffice.
    /// Если офис установлен, то обычно массив содержит один элемент.
    /// Однако, могут быть установлено несколько различных копий офиса.
    /// В этом случае обычно следует использовать метод GetPartInstallations(), который возвращает список офисов в порядке предпочтения пользователя.
    /// Если нет установленного офиса, возвращается пустой массив.
    /// Если приложение использует ExtForms.dll, для определения "действуюшей" копии следует использовать свойство EFPApp.UsedOpenOffice
    /// </summary>
    public static OpenOfficeInfo[] Installations { get { return _Installations; } }
    private static OpenOfficeInfo[] _Installations = InitInstallations();

    #region Поиск установленных копий

    private static OpenOfficeInfo[] InitInstallations()
    {
      // Этот метод не имеет права выбрасывать исключения.
      // Нельзя даже вывести исключение в log-файл
      try
      {
        List<OpenOfficeInfo> lst = new List<OpenOfficeInfo>();

        FindFromFileAssociations(lst); // 15.09.2023

        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
          case PlatformID.Win32S: // ???
            FindFromRegistry(lst);
            break;
          case PlatformID.Unix:
            FindFromPredefined(lst); // 11.05.2016
            FindFromPath(lst); // 22.05.2016
            break;
        }

        FindFromUnoPath(lst); // для Windows и Unix

        return lst.ToArray();
      }
      catch (Exception e)
      {
        Trace.WriteLine("Exception caught when detecting installed OpenOffices/LibreOffices: " + e.Message);
        return new OpenOfficeInfo[0];
      }
    }

    private static void FindFromUnoPath(List<OpenOfficeInfo> lst)
    {
      string s = Environment.GetEnvironmentVariable("UNO_PATH");
      FindOrAddItem(lst, new AbsPath(s), OpenOfficeKind.Unknown, OpenOfficeInfo.InfoSourceKind.EnvironmentVariable, "UNO_PATH", OpenOfficePlatform.Unknown);
    }

    private static void FindFromFileAssociations(List<OpenOfficeInfo> lst)
    {
      FileAssociations FAs = FileAssociations.FromFileExtension(".odt");
      foreach (FileAssociationItem fa in FAs)
      {
        FindOrAddItem(lst, GetProgramDir(fa), GetOfficeKind(fa), OpenOfficeInfo.InfoSourceKind.FileAssociation, fa.ProgId, OpenOfficePlatform.Unknown);
      }
    }

    private static AbsPath GetProgramDir(FileAssociationItem fa)
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Unix:
          try { return GetProgramDirLinux(fa); }
          catch { }
          break;
      }
      return fa.ProgramPath.ParentDir;
    }

    private static AbsPath GetProgramDirLinux(FileAssociationItem fa)
    {
      AbsPath programPath = FileTools.GetRealPath(fa.ProgramPath);
      if (!System.IO.File.Exists(programPath.Path))
        return AbsPath.Empty;

      // Так не работает, т.к. есть файл /usr/nin/soffice
      //AbsPath path1 = new AbsPath(programPath.ParentDir, "soffice");
      //AbsPath path2 = new AbsPath(programPath.ParentDir, "aoffice");
      //if (System.IO.File.Exists(path1.Path) || System.IO.File.Exists(path2.Path))
      //  return programPath.ParentDir;


      if(programPath.ParentDir != new AbsPath("/usr/bin"))
        return programPath.ParentDir;

      FileInfo fi = new FileInfo(programPath.Path);
      if (fi.Length >= 1024)
        return programPath.ParentDir;

      string[] lines = System.IO.File.ReadAllLines(programPath.Path);
      string cmdLine = null;
      for (int i = 0; i < lines.Length; i++)
      {
        if (lines[i].Length == 0)
          continue;
        if (lines[i][0] == '#')
          continue;
        if (cmdLine == null)
          cmdLine = lines[i];
        else
          return programPath.ParentDir;
      }
      if (cmdLine != null)
      {
        if (!cmdLine.StartsWith("exec "))
          return programPath.ParentDir;

        cmdLine = cmdLine.Substring(5).Trim();
        int p = cmdLine.IndexOf(' ');
        if (p >= 0)
          cmdLine = cmdLine.Substring(0, p);

        AbsPath newPath = new AbsPath(cmdLine);
        if (System.IO.File.Exists(newPath.Path))
          return newPath.ParentDir;
      }

      return programPath.ParentDir;
    }

    private static OpenOfficeKind GetOfficeKind(FileAssociationItem fa)
    {
      OpenOfficeKind res = DoGetOfficeKind(fa.ProgId);
      if (res != OpenOfficeKind.Unknown)
        return res;
      res = DoGetOfficeKind(fa.DisplayName); // 11.10.2023
      return res;
    }

    private static OpenOfficeKind DoGetOfficeKind(string s)
    {
      if (String.IsNullOrEmpty(s))
        return OpenOfficeKind.Unknown;

      if (s.IndexOf("OpenOffice", StringComparison.OrdinalIgnoreCase) >= 0)
        return OpenOfficeKind.OpenOffice;
      if (s.IndexOf("LibreOffice", StringComparison.OrdinalIgnoreCase) >= 0)
        return OpenOfficeKind.LibreOffice;
      if (s.IndexOf("AlterOffice", StringComparison.OrdinalIgnoreCase) >= 0)
        return OpenOfficeKind.AlterOffice;
      return OpenOfficeKind.Unknown;
    }

    #region Поиск для Windows

    private static void FindFromRegistry(List<OpenOfficeInfo> lst)
    {
      // Поиск через реестр
      // 11.01.2012
      // В 64-разрядной версии Windows ключи реестра расположены в подузле Wow6432Node

      // 22.05.2016
      // Ключи могут быть также HKEY_CURRENT_USER

      /*
       * На поиск вляет разрядность:
       * 1-приложения (Net Framework'а), т.к. в реестре выполняется подстановка ключей
       * 2-Windows
       * 3-LibreOffice
       * 
       * В таблице указаны ключи реестра, которые нужно искать
       * 
       * Приложение  Windows  LibreOffice  Ключ реестра                 Примечание
       *   32-bit     32-bit     32-bit    HKxx\SOFTWARE\
       *              64-bit     32-bit    HKxx\SOFTWARE\               Подстановка узла реестра Wow6432Node
       *                         64-bit    Не знаю, как найти
       *   64-bit     64-bit     32-bit    HKxx\SOFTWARE\Wow6432Node\
       *                         64-bit    HKxx\SOFTWARE
       */

      // 32-разрядная версия приложения
      if (EnvironmentTools.Is64BitOperatingSystem)
      {
        using (RegistryTree2 tree = new RegistryTree2(true, RegistryView2.Registry64))
        {
          FindFromRegistry2(tree, @"HKEY_CURRENT_USER\SOFTWARE\", lst, OpenOfficePlatform.x64);
          FindFromRegistry2(tree, @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\", lst, OpenOfficePlatform.x86);
          FindFromRegistry2(tree, @"HKEY_LOCAL_MACHINE\SOFTWARE\", lst, OpenOfficePlatform.x64);
          FindFromRegistry2(tree, @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\", lst, OpenOfficePlatform.x86);
        }
      }
      else
      {
        using (RegistryTree2 tree = new RegistryTree2(true, RegistryView2.Default))
        {
          FindFromRegistry2(tree, @"HKEY_CURRENT_USER\SOFTWARE\", lst, OpenOfficePlatform.x86);
          FindFromRegistry2(tree, @"HKEY_LOCAL_MACHINE\SOFTWARE\", lst, OpenOfficePlatform.x86);
        }
      }
    }


    private static void FindFromRegistry2(RegistryTree2 tree, string keyNameBase, List<OpenOfficeInfo> lst, OpenOfficePlatform platform)
    {
      FindFromRegistry3(tree, keyNameBase + @"OpenOffice\UNO\InstallPath", lst, OpenOfficeKind.OpenOffice, platform); // 18.05.2016 - для OpenOffice 4.1.2
      FindFromRegistry3(tree, keyNameBase + @"OpenOffice.org\UNO\InstallPath", lst, OpenOfficeKind.OpenOffice, platform);
      FindFromRegistry3(tree, keyNameBase + @"LibreOffice\UNO\InstallPath", lst, OpenOfficeKind.LibreOffice, platform);
    }

    private static void FindFromRegistry3(RegistryTree2 tree, string keyName, List<OpenOfficeInfo> lst, OpenOfficeKind kind, OpenOfficePlatform platform)
    {
      // 30.09.2013
      // Может не быть доступа к ключу реестра
      try
      {
        AbsPath programDir = new AbsPath(tree.GetString(keyName, String.Empty));
        if (programDir.IsEmpty)
          return;

        FindOrAddItem(lst, programDir, kind, OpenOfficeInfo.InfoSourceKind.Registry, keyName, platform);
      }
      catch
      {
      }
    }

    #endregion

    #region Поиск для Linux

    private static void FindFromPath(List<OpenOfficeInfo> lst)
    {
      string pathVar = Environment.GetEnvironmentVariable("PATH");
      if (String.IsNullOrEmpty(pathVar))
        return;

      string[] a = pathVar.Split(System.IO.Path.PathSeparator);
      for (int i = 0; i < a.Length; i++)
        FindOrAddItem(lst, new AbsPath(a[i]), OpenOfficeKind.Unknown, OpenOfficeInfo.InfoSourceKind.EnvironmentVariable, "Path", OpenOfficePlatform.Unknown);
    }

    private static void FindFromPredefined(List<OpenOfficeInfo> lst)
    {
      FindFromPredefined_lib(lst, "lib", OpenOfficePlatform.Unknown);
      FindFromPredefined_lib(lst, "lib32", OpenOfficePlatform.x86); // 20.07.2022
      FindFromPredefined_lib(lst, "lib64", OpenOfficePlatform.x64); // 20.07.2022
    }
    private static void FindFromPredefined_lib(List<OpenOfficeInfo> lst, string lib, OpenOfficePlatform platform)
    {
      if (!Directory.Exists("/usr/" + lib))
        return;

      FindFromPredefined_lib2(lst, lib, platform, "libreoffice*", OpenOfficeKind.LibreOffice);
      FindFromPredefined_lib2(lst, lib, platform, "LibreOffice*", OpenOfficeKind.LibreOffice);
      FindFromPredefined_lib2(lst, lib, platform, "openoffice*", OpenOfficeKind.OpenOffice);
      FindFromPredefined_lib2(lst, lib, platform, "OpenOffice*", OpenOfficeKind.OpenOffice);
    }

    private static void FindFromPredefined_lib2(List<OpenOfficeInfo> lst, string lib, OpenOfficePlatform platform, string subDirTemplate, OpenOfficeKind officeKind)
    {
      string[] aDirs = System.IO.Directory.GetDirectories("/usr/" + lib, subDirTemplate, System.IO.SearchOption.TopDirectoryOnly);
      for (int i = 0; i < aDirs.Length; i++)
      {
        AbsPath dir = new AbsPath(new AbsPath(aDirs[i]), "program");
        if (File.Exists(new AbsPath(dir, "soffice").Path))
          FindOrAddItem(lst, dir, officeKind, OpenOfficeInfo.InfoSourceKind.PredefinedPath, String.Empty, OpenOfficePlatform.Unknown);
      }

    }

    #endregion

    #region Вспомогательные методы поиска

    private static void FindOrAddItem(List<OpenOfficeInfo> lst, AbsPath programDir, OpenOfficeKind kind, OpenOfficeInfo.InfoSourceKind infoSource, string infoSourceString, OpenOfficePlatform platform)
    {
      if (programDir.IsEmpty)
        return;

      if (!Directory.Exists(programDir.Path))
        return; // пустышка

      AbsPath sofficePath = new AbsPath(programDir, "soffice" + GetExeExtension());
      if (!File.Exists(sofficePath.Path))
      {
        // AlterOffice
        sofficePath = new AbsPath(programDir, "aoffice" + GetExeExtension());
        if (File.Exists(sofficePath.Path))
        {
          if (kind == OpenOfficeKind.Unknown)
            kind = OpenOfficeKind.AlterOffice;
        }
        else
          return;
      }

      if (Environment.OSVersion.Platform == PlatformID.Unix)
      {
        AbsPath sofficeBinPath = sofficePath.ChangeExtension(".bin");
        if (!File.Exists(sofficeBinPath.Path))
          return; 
      }

      // Во избежание повторов, проверяем наличие в списке такого же пути
      for (int i = 0; i < lst.Count; i++)
      {
        if (lst[i].ProgramDir == programDir)
          return;
      }

      if (platform == OpenOfficePlatform.Unknown)
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
            bool? is64bit = FileTools.Is64bitPE(sofficePath);
            if (is64bit.HasValue)
              platform = is64bit.Value ? OpenOfficePlatform.x64 : OpenOfficePlatform.x86;
            break;
          case PlatformID.Win32Windows:
            platform = OpenOfficePlatform.x86;
            break;
          case PlatformID.Unix:
            if (IntPtr.Size == 8)
              platform = OpenOfficePlatform.x64;
            else
              platform = OpenOfficePlatform.x86;
            break;
        }
      }

      lst.Add(new OpenOfficeInfo(programDir, kind, infoSource, infoSourceString, platform));
    }

    internal static string GetExeExtension()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
        case PlatformID.Win32S:
        case PlatformID.WinCE: // ???
          return ".exe";
        default:
          return String.Empty;
      }
    }

    /// <summary>
    /// Обновляет массив Installations.
    /// </summary>
    public static void RefreshInstalls()
    {
      _Installations = InitInstallations();

      _PartInstallations = null;
    }

    #endregion

    #endregion

    #region Установленные копии для заданного компонента

    /// <summary>
    /// Сохраненные значения для GetPartInstallations()
    /// Первый индекс - OpenOfficePart
    /// Второй индекс - установки офиса в порядке предпочтений пользователя
    /// </summary>
    private static OpenOfficeInfo[][] _PartInstallations;

    /// <summary>
    /// Возвращает подмножество копий офиса из массива <see cref="Installations"/>, в котором есть установленный компонент <paramref name="part"/>.
    /// Порядок элементов массива может не соответствовать исходному; сортировка выполняется в соответствии с предпочтениями пользователя.
    /// </summary>
    /// <param name="part">Требуемый компонент</param>
    public static OpenOfficeInfo[] GetPartInstallations(OpenOfficePart part)
    {
      if (_PartInstallations == null)
        _PartInstallations = new OpenOfficeInfo[OpenOfficeInfo.PartCount][];

      if (_PartInstallations[(int)part] == null)
        _PartInstallations[(int)part] = CreatePartInstallations(part);
      return _PartInstallations[(int)part];
    }

    private static OpenOfficeInfo[] CreatePartInstallations(OpenOfficePart part)
    {
      if (Installations.Length == 0)
        return Installations;

      List<OpenOfficeInfo> lst1 = new List<OpenOfficeInfo>(Installations.Length);

      // Неотсортированный список офисов с компонентом
      for (int i = 0; i < Installations.Length; i++)
      {
        if (Installations[i].Parts.Contains(part))
          lst1.Add(Installations[i]);
      }
      if (lst1.Count < 2)
        return lst1.ToArray(); // нечего сортировать

      FileAssociations fas = FileAssociations.FromFileExtension(OpenOfficePartInfo.PartFileExts[(int)part]); // в нужном порядке

      // Отсортированный список
      List<OpenOfficeInfo> lst2 = new List<OpenOfficeInfo>(lst1.Count);
      foreach (FileAssociationItem fa in fas)
      {
        AbsPath programDir = GetProgramDir(fa);
        for (int j = 0; j < lst1.Count; j++)
        {
          if (lst1[j].ProgramDir == programDir)
          {
            lst2.Add(lst1[j]);
            lst1.RemoveAt(j);
            break;
          }
        }
      }

      // По идее быть не должно, но в теории могут быть копии офиса без ассоциии
      lst2.AddRange(lst1);
      return lst2.ToArray();
    }

    #endregion

    #endregion

    #region Файлы Open Document Format

    #region Константы

    const string nmspcStyle = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    const string nmspcNumber = "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0";
    const string nmspcLoext = "urn:org:documentfoundation:names:experimental:office:xmlns:loext:1.0"; // 18.11.2016

    #endregion

    #region Форматы чисел и даты в файлах Open Document Format

    //  TODO: Это надо делать как-то культурно, используя парсинг формата

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elStyles"></param>
    /// <param name="formatText"></param>
    /// <param name="styleName"></param>
    /// <returns></returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName)
    {
      return ODFAddFormat(elStyles, formatText, styleName, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elStyles"></param>
    /// <param name="formatText"></param>
    /// <param name="styleName"></param>
    /// <param name="ci"></param>
    /// <returns></returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName,
      CultureInfo ci)
    {
      int p = ci.Name.IndexOf('-');
      if (p < 0)
        throw new ArgumentException("Неправильная культура \"" + ci.Name + "\". Отсутствует \"-\"", "ci");
      string language = ci.Name.Substring(0, p);
      string country = ci.Name.Substring(p + 1);

      return ODFAddFormat(elStyles, formatText, styleName,
        ci.NumberFormat, ci.DateTimeFormat, language, country);
    }

    /// <summary>
    /// В Open Document Format, в отличие от файлов Microsoft Office, форматы чисел
    /// и дат задаются не в виде одной строки, например, "0.00", а в виде множества
    /// отдельных стилей
    /// Реализация не полная!
    /// </summary>
    /// <param name="elStyles">Узел "office:automatic-styles" для добавления форматов</param>
    /// <param name="formatText">Исходный формат числа</param>
    /// <param name="styleName">Имя создаваемого стиля</param>
    /// <param name="numberFormat"></param>
    /// <param name="dateTimeFormat"></param>
    /// <param name="language"></param>
    /// <param name="country"></param>
    /// <returns>true - стиль добавлен. false - в текущей реализации данный формат
    /// не преобразуется</returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName,
      NumberFormatInfo numberFormat, DateTimeFormatInfo dateTimeFormat, string language, string country)
    {
      if (String.IsNullOrEmpty(formatText))
        return false;

      if (DataTools.IndexOfAny(formatText, "yMdhHmsDtTfFgGRruUY") >= 0)
        return ODFAddDateTimeFormat(elStyles, formatText, styleName,
          dateTimeFormat, language, country);

      if (formatText.IndexOf('0') >= 0)
        return ODFAddNumberFormat(elStyles, formatText, styleName, numberFormat, language, country);

      return false;
    }

    private static bool ODFAddNumberFormat(XmlElement elStyles, string formatText, string styleName,
      NumberFormatInfo numberFormat, string language, string country)
    {
      XmlElement elStyle = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", styleName, nmspcStyle);
      SetAttr(elStyle, "number:language", language, nmspcNumber);
      SetAttr(elStyle, "number:country", country, nmspcNumber);

      // 18.11.2016
      // Запись форматов, состоящих из частей, разделенных запятыми
      // Последняя часть формата записывается непосредственно внутри блока number:number-style
      // Для других частей создаются отдельные блоки number:number-style с суффиксами P0, P1, ... 
      // внутри элемента elStyles, а в основном блоке number:number-style задаются ссылки с условиями

      string[] a = formatText.Split(';');
      XmlElement elStyleP0, elStyleP1;
      switch (a.Length)
      {
        case 1: // обычный формат, как было
          DoWriteNumberFormat(elStyle, formatText);
          break;
        case 2: // Части >=0 и < 0
          elStyleP0 = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
          elStyles.AppendChild(elStyle);
          SetAttr(elStyleP0, "style:name", styleName + "P0", nmspcStyle);
          DoWriteNumberFormat(elStyleP0, a[0]);
          DoWriteFormatRef(elStyle, "value()>=0", styleName + "P0");

          DoWriteNumberFormat(elStyle, a[1]);
          break;
        case 3: // Части >0, <0 и =0
          elStyleP0 = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
          elStyles.AppendChild(elStyleP0);
          SetAttr(elStyleP0, "style:name", styleName + "P0", nmspcStyle);
          DoWriteNumberFormat(elStyleP0, a[0]);
          DoWriteFormatRef(elStyle, "value()>0", styleName + "P0");

          elStyleP1 = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
          elStyles.AppendChild(elStyleP1);
          SetAttr(elStyleP1, "style:name", styleName + "P1", nmspcStyle);
          DoWriteNumberFormat(elStyleP1, a[1]);
          DoWriteFormatRef(elStyle, "value()<0", styleName + "P1");

          DoWriteNumberFormat(elStyle, a[2]);
          break;

        default:
          throw new ArgumentException("Числовой формат \"" + formatText + "\" состоит больше, чем из трех частей", "formatText");
      }

      return true;
    }

    private static void DoWriteFormatRef(XmlElement elStyle, string condition, string styleName)
    {
      XmlElement elMap = elStyle.OwnerDocument.CreateElement("style:map", nmspcStyle);
      elStyle.AppendChild(elMap);
      SetAttr(elMap, "style:condition", condition, nmspcStyle);
      SetAttr(elMap, "style:apply-style-name", styleName, nmspcStyle);
    }

    private static void DoWriteNumberFormat(XmlElement elStyle, string formatText)
    {
      if (String.IsNullOrEmpty(formatText))
      {
        DoWriteTextFormat(elStyle, String.Empty);
        return;
      }

      if (formatText[0] == '\"')
      {
        DoWriteTextFormat(elStyle, UnquoteText(formatText));
        return;
      }

      if (formatText[0] == '-')
      {
        // Знак числа не является частью формата, а является текстом
        DoWriteTextFormat(elStyle, "-");
        formatText = formatText.Substring(1);
      }


      /*
       * В Open Document Format не предусмотрено хранение необязательных цифр после запятой.
       * Если задать формат "0.0#", сохранить документ ODS,  закрыть и открыть заново, то формат заменяется на "0.00"
       */

      // Определяем наличие разделителя тысяч и убираем ведущие #
      bool thousandSep = false;
      for (int i = 0; i < formatText.Length; i++)
      {
        bool brk = false;
        switch (formatText[i])
        {
          case '#':
            break;
          case ',':
            thousandSep = true;
            break;
          default:
            formatText = formatText.Substring(i);
            brk = true;
            break;
        }
        if (brk)
          break;
      }

      int minIntDigs;
      int decimals;
      int p = formatText.IndexOf('.');
      if (p < 0)
      {
        decimals = 0;
        minIntDigs = formatText.Length;
      }
      else
      {
        decimals = formatText.Length - p - 1;
        minIntDigs = formatText.Length - decimals - 1;
      }

      int minDecimals = decimals;
      for (int i = 0; i < decimals; i++)
      {
        if (formatText[formatText.Length - i - 1] == '#')
          minDecimals--;
        else
          break;
      }

      XmlElement elNumber = elStyle.OwnerDocument.CreateElement("number:number", nmspcNumber);
      elStyle.AppendChild(elNumber);
      SetAttr(elNumber, "number:decimal-places", decimals.ToString(), nmspcNumber);
      SetAttr(elNumber, "number:min-integer-digits", minIntDigs.ToString(), nmspcNumber);
      if (minDecimals < decimals)
      {
        SetAttr(elNumber, "loext:min-decimal-places", minDecimals.ToString(), nmspcLoext);
        SetAttr(elNumber, "number:decimal-replacement", "", nmspcNumber);
      }

      if (thousandSep) // по идее, надо проверять, что запятая идет слева от "." и находится между "0#"
        SetAttr(elNumber, "number:grouping", "true", nmspcNumber);
    }

    private static string UnquoteText(string s)
    {
      if (s.Length < 2) // бяка
        return string.Empty;
      s = s.Substring(1, s.Length - 2);
      s = s.Replace("\"\"", "\"");

      // TODO: замена ESC-символов
      return s;
    }

    private static void DoWriteTextFormat(XmlElement elStyle, string s)
    {
      XmlElement elText = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
      elStyle.AppendChild(elText);
      if (!String.IsNullOrEmpty(s))
      {
        XmlText txtNode = elText.OwnerDocument.CreateTextNode(s);
        elText.AppendChild(txtNode);
      }
    }

    private static bool ODFAddDateTimeFormat(XmlElement elStyles, string formatText, string styleName,
      DateTimeFormatInfo formatInfo, string language, string country)
    {
      // Заменяем стандартные стили
      formatText = Formatting.FormatStringTools.ExpandDateTimeFormat(formatText, formatInfo);

      XmlElement elStyle = elStyles.OwnerDocument.CreateElement(Formatting.FormatStringTools.ContainsDate(formatText) ? "number:date-style": "number:time-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", styleName, nmspcStyle);
      SetAttr(elStyle, "number:language", language, nmspcNumber);
      SetAttr(elStyle, "number:country", country, nmspcNumber);

      XmlElement elPart;
      const string AllMaskChars = "yMdhHmst";

      // Перебираем символы в FormatText
      // использовать for неудобно, т.к. буду прыгать через символы
      int pos = 0;
      while (pos < formatText.Length)
      {
        if (AllMaskChars.IndexOf(formatText[pos]) >= 0)
        {
          // один из символов маски
          // находим все такие же символы
          int cnt = 1;
          for (int j = pos + 1; j < formatText.Length; j++)
          {
            if (formatText[j] == formatText[pos])
              cnt++;
            else
              break;
          }

          switch (formatText[pos])
          {
            case 'y':
              elPart = elStyle.OwnerDocument.CreateElement("number:year", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 2)
                SetAttr(elPart, "number:style", "long", nmspcNumber);
              break;
            case 'M':
              switch (cnt)
              {
                case 1: // "1"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  break;
                case 2: // "01"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);
                  break;
                case 3: // "янв"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:textual", "true", nmspcNumber);
                  break;
                case 4: // "январь"
                  elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:textual", "true", nmspcNumber);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);  // в чем разница ?????
                  break;
              }
              break;
            case 'd':
              switch (cnt)
              {
                case 1: // "1"
                  elPart = elStyle.OwnerDocument.CreateElement("number:day", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  break;
                case 2: // "01"
                  elPart = elStyle.OwnerDocument.CreateElement("number:day", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);
                  break;
                case 3: // "Чт"
                  elPart = elStyle.OwnerDocument.CreateElement("day-of-week", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  break;
                //case 4: // "Четверг"
                default:
                  elPart = elStyle.OwnerDocument.CreateElement("day-of-week", nmspcNumber);
                  elStyle.AppendChild(elPart);
                  SetAttr(elPart, "number:style", "long", nmspcNumber);
                  break;
              }
              break;
            case 'h': // время в 12-часовом формате
              // 12- и 24-часовой форматы отличаются, похоже, только наличием AM/PM
              elPart = elStyle.OwnerDocument.CreateElement("number:hours", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);
              break;

            case 'H': // время в 24-часовом формате
              elPart = elStyle.OwnerDocument.CreateElement("number:hours", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);
              break;

            case 'm':
              elPart = elStyle.OwnerDocument.CreateElement("number:minutes", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);

              break;
            case 's':
              elPart = elStyle.OwnerDocument.CreateElement("number:seconds", nmspcNumber);
              elStyle.AppendChild(elPart);
              if (cnt > 1)
                SetAttr(elPart, "number:style", "long", nmspcNumber);
              break;
            case 't':
              elPart = elStyle.OwnerDocument.CreateElement("number:am-pm", nmspcNumber);
              elStyle.AppendChild(elPart);
              break;
          }
          pos += cnt;
          continue;
        }

        string s;

        if (formatText[pos] == '\'')
        {
          // Ищем второй апостров
          int p = formatText.IndexOf('\'', pos + 1);
          if (p < 0) // ошибка - нет второго апострофа
            p = formatText.Length; // считаем, что строка идет до конца формата

          s = formatText.Substring(pos + 1, p - pos - 1);

          elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
          elStyle.AppendChild(elPart);
          elPart.InnerText = s; // !!! преобразование спецсимволов
          pos = p;
          continue;
        }

        // Простые символы и специальные символы
        // Вряд ли будет идти больше одного символа подряд
        switch (formatText[pos])
        {
          case '/':
            s = formatInfo.DateSeparator;
            break;
          case ':':
            s = formatInfo.TimeSeparator;
            break;
          default:
            s = new string(formatText[pos], 1);
            break;
        }

        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = s; // !!! преобразование спецсимволов

        pos++;
      }

      return true;

#if XXX
      // лень думать
      XmlElement elStyle = elStyles.OwnerDocument.CreateElement("number:date-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", StyleName, nmspcStyle);

      XmlElement elPart;
      elPart = elStyle.OwnerDocument.CreateElement("number:day", nmspcNumber);
      elStyle.AppendChild(elPart);
      SetAttr(elPart, "number:style", "long", nmspcNumber);

      elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
      elStyle.AppendChild(elPart);
      elPart.InnerText = ".";

      elPart = elStyle.OwnerDocument.CreateElement("number:month", nmspcNumber);
      elStyle.AppendChild(elPart);
      SetAttr(elPart, "number:style", "long", nmspcNumber);

      elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
      elStyle.AppendChild(elPart);
      elPart.InnerText = ".";

      elPart = elStyle.OwnerDocument.CreateElement("number:year", nmspcNumber);
      elStyle.AppendChild(elPart);
      SetAttr(elPart, "number:style", "long", nmspcNumber);

      if (FormatText.IndexOf(':') >= 0)
      {
        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = " ";

        elPart = elStyle.OwnerDocument.CreateElement("number:hours", nmspcNumber);
        elStyle.AppendChild(elPart);
        SetAttr(elPart, "number:style", "long", nmspcNumber);

        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = ":";

        elPart = elStyle.OwnerDocument.CreateElement("number:minutes", nmspcNumber);
        elStyle.AppendChild(elPart);
        SetAttr(elPart, "number:style", "long", nmspcNumber);

        elPart = elStyle.OwnerDocument.CreateElement("number:text", nmspcNumber);
        elStyle.AppendChild(elPart);
        elPart.InnerText = ":";

        elPart = elStyle.OwnerDocument.CreateElement("number:seconds", nmspcNumber);
        elStyle.AppendChild(elPart);
        SetAttr(elPart, "number:style", "long", nmspcNumber);
      }

      return true;
#endif
    }

    #endregion

    #region Вспомогательные методы

    private static void SetAttr(XmlElement el, string name, string value, string nmspc)
    {
      XmlAttribute attr;
      if (String.IsNullOrEmpty(nmspc))
        attr = el.OwnerDocument.CreateAttribute(name);
      else
        attr = el.OwnerDocument.CreateAttribute(name, nmspc);
      attr.Value = value;
      el.Attributes.Append(attr);
    }

    #endregion

    #endregion
  }
}
