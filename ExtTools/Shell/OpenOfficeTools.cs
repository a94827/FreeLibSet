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
  #region Перечисление OpenOfficeKind

  /// <summary>
  /// Вид офиса: OpenOffice или LibreOffice
  /// </summary>
  public enum OpenOfficeKind
  {
    /// <summary>
    /// Нет установленного офиса
    /// </summary>
    Unknown,

    /// <summary>
    /// Установлен OpenOffice
    /// </summary>
    OpenOffice,

    /// <summary>
    /// Установлен Libre Office
    /// </summary>
    LibreOffice
  }

  #endregion

  #region Перечисление OpenOfficeArchitecture

  /// <summary>
  /// Разрядность приложений LibreOffice
  /// </summary>
  public enum OpenOfficePlatform
  {
    /// <summary>
    /// Разрядность неизвестна
    /// </summary>
    Unknown,

    /// <summary>
    /// 32-битное приложение
    /// </summary>
    x86,

    /// <summary>
    /// 64-битное приложение
    /// </summary>
    x64
  }

  #endregion

  /// <summary>
  /// Поддержка для OpenOffice / Libre Office.
  /// </summary>
  public static class OpenOfficeTools
  {
    #region Список установленных копий

    #region Перечисление InfoSource

    /// <summary>
    /// Откуда получена информация об установленной копии
    /// </summary>
    public enum InfoSource
    {
      /// <summary>
      /// Неизвестно
      /// </summary>
      Unknown,

      /// <summary>
      /// Из записи в реестре Windows.
      /// В этом случае свойство OfficeInfo.InfoSourceString содержит раздел реестра
      /// </summary>
      Registry,

      /// <summary>
      /// Из переменной окружения.
      /// В этом случае свойство OfficeInfo.InfoSourceString содержит имя переменной ("PATH")
      /// </summary>
      EnvironmentVariable,

      /// <summary>
      /// Поиск был выполнен по предопределенному пути
      /// </summary>
      PredefinedPath
    }

    #endregion

    /// <summary>
    /// Информация об одной установленной копии офиса
    /// </summary>
    public sealed class OfficeInfo
    {
      #region Конструкторы

      /// <summary>
      /// Версия конструктора без указания источника информации.
      /// </summary>
      /// <param name="programDir"></param>
      /// <param name="kind"></param>
      public OfficeInfo(AbsPath programDir, OpenOfficeKind kind)
        : this(programDir, kind, InfoSource.Unknown, String.Empty, OpenOfficePlatform.Unknown)
      {
      }

      /// <summary>
      /// Версия конструктора с указанием источника информации.
      /// </summary>
      /// <param name="programDir"></param>
      /// <param name="kind"></param>
      /// <param name="infoSource"></param>
      /// <param name="infoSourceString"></param>
      public OfficeInfo(AbsPath programDir, OpenOfficeKind kind, InfoSource infoSource, string infoSourceString)
        : this(programDir, kind, infoSource, infoSourceString, OpenOfficePlatform.Unknown)
      {
      }

      /// <summary>
      /// Версия конструктора с указанием платформы
      /// </summary>
      /// <param name="programDir">Каталог с программными файлами (в котором находится soffice.exe или soffice</param>
      /// <param name="kind">OpenOffice или LibeOffice</param>
      /// <param name="infoSource">Откуда получена информация об установленной копии</param>
      /// <param name="infoSourceString">Дополнительная информация, как была найдена эта копия (ключ реестра или имя переменной окружения)</param>
      /// <param name="platform">32-bit или 64-bit</param>
      public OfficeInfo(AbsPath programDir, OpenOfficeKind kind, InfoSource infoSource, string infoSourceString, OpenOfficePlatform platform)
      {
        #region Копирование аргументов

        if (programDir.IsEmpty)
          throw new ArgumentException("Не задан ProgramDir", "programDir");

        _ProgramDir = programDir;
        _Kind = kind;
        _InfoSource = infoSource;
        _InfoSourceString = infoSourceString;
        _Platform = platform;

        #endregion

        #region Определение версии

        try
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Unix:
              InitVersionUnix(); // 17.05.2016
              break;
            default:
              _Version = FileTools.GetFileVersion(OfficePath);
              break;
          }
        }
        catch
        {
          _Version = new Version(); // пустая версия
        }
        if (_Version == null)
          _Version = new Version();

        #endregion

        #region Определение наличия компонентов

        _HasWriter = IsCompExists("swriter");
        _HasCalc = IsCompExists("scalc");
        _HasImpress = IsCompExists("simpress");
        _HasDraw = IsCompExists("sdraw");
        _HasBase = IsCompExists("sbase");
        _HasMath = IsCompExists("smath");

        #endregion
      }

      /// <summary>
      /// Определение версии OpenOffice/LibreOffice в Linux.
      /// Понятия не имею, как это сделать правильно.
      /// Ни один файл не имеет версии, заданной в ресурсах, как в Windows.
      /// </summary>
      /// <returns></returns>
      private void InitVersionUnix()
      {
        try
        {
          if (InitVersionUnix_main_xcd())
            return;
        }
        catch(Exception e)
        {
          Trace.WriteLine("Error in InitVersionUnix_main_xcd(): "+e.Message);
        }
        try
        {
          if (InitVersionUnix_versionrc())
            return;
        }
        catch (Exception e)
        {
          Trace.WriteLine("Error in InitVersionUnix_versionrc(): " + e.Message);
        }
        Trace.WriteLine("Unable to detect the version of " + this.KindName+ " from path "+this.ProgramDir.Path); 
      }

      /// <summary>
      /// 11.07.2022.
      /// Новый вариант определения версии - из скрытого xml-файла main.xcd
      /// </summary>
      /// <returns><c>true</c>, if version unix main xcd was inited, <c>false</c> otherwise.</returns>
      private bool InitVersionUnix_main_xcd()
      {
        AbsPath xmlFile = new AbsPath(ProgramDir.ParentDir, "share", "registry", "main.xcd");
        if (!File.Exists(xmlFile.Path))
        {
          xmlFile = new AbsPath(ProgramDir.ParentDir, "share", ".registry", "main.xcd"); // скрытая папка
          if (!File.Exists(xmlFile.Path))
            return false;
        }

        XmlDocument xml = new XmlDocument();
        xml.Load(xmlFile.Path);
        XmlNamespaceManager nsm = new XmlNamespaceManager(xml.NameTable);
        nsm.AddNamespace("oor", "http://openoffice.org/2001/registry");
        XmlNodeList nl = xml.SelectNodes("oor:data/oor:component-data[@oor:name='Setup' and @oor:package=\'org.openoffice\']/node/prop", nsm);
        if (nl.Count==0)
          nl = xml.SelectNodes("oor:data/oor:component-data[@oor:name='Product' and @oor:package=\'org.openoffice\']/node/prop", nsm); // 20.07.2022

        Version ver1 = null;

        foreach (XmlNode node in nl)
        {
          XmlAttribute attrName = node.Attributes["oor:name"];
          if (attrName == null)
            continue;
          if (attrName.Value == "ooSetupVersionAboutBox") // полный номер версии "7.3.4.2"
          {
            XmlNode nodeValue = node.SelectSingleNode("value");
            if (nodeValue == null)
              continue;

            // _Version = new Version(nodeValue.InnerText);
            _Version = FileTools.GetVersionFromStr(nodeValue.InnerText); // 20.07.2022. Может быть плохая версия
            return true;
          }
          if (attrName.Value == "ooSetupVersion") // неполный номер версии "7.3"
          {
            XmlNode nodeValue = node.SelectSingleNode("value");
            if (nodeValue == null)
              continue;
            //ver1 = new Version(nodeValue.InnerText);
            ver1 = FileTools.GetVersionFromStr(nodeValue.InnerText); // 20.07.2022
          }
        } // цикл перебора узлов

        if (ver1 == null)
          return false;

        _Version = ver1;
        return true;
      }

      /// <summary>
      /// Старый вариант определения версии - в текстовом файле version.rc
      /// </summary>
      /// <returns>True, если удалось определить версию Office</returns>
      private bool InitVersionUnix_versionrc()
      {
        AbsPath textFile = new AbsPath(ProgramDir, "versionrc");
        if (!File.Exists(textFile.Path))
          return false;

        string[] aLines = System.IO.File.ReadAllLines(textFile.Path); // ?? кодировка
        for (int i = 0; i < aLines.Length; i++)
        {
          // Искомая строка выглядит так:
          // BuildVersion=1:5.0.3-rc2-0ubuntu1-trusty2

          if (aLines[i].StartsWith("BuildVersion="))
          {
            string s = aLines[i].Substring(13); // после знака равенства
            int p = s.IndexOf(':');
            if (p < 0)
              return false;
            s = s.Substring(p + 1);
            p = s.IndexOf('-');
            if (p >= 0)
              s = s.Substring(0, p);
            _Version = FileTools.GetVersionFromStr(s);
            return true;
          }
        }
        // не нашли строки
        return false;
      }

      private bool IsCompExists(string appName)
      {
        AbsPath filePath = new AbsPath(ProgramDir, appName + GetExeExtension());
        return File.Exists(filePath.Path);
      }

      #endregion

      #region Свойства офиса в-целом

      /// <summary>
      /// Возвращает тип установленного офиса или Unknown, если не установлен
      /// </summary>
      public OpenOfficeKind Kind { get { return _Kind; } }
      private readonly OpenOfficeKind _Kind;

      /// <summary>
      /// Получить каталог с программными файлами (в котором находится soffice.exe или soffice). 
      /// Возвращает AbsPath.Empty, если офис не установлен.
      /// </summary>
      public AbsPath ProgramDir { get { return _ProgramDir; } }
      private readonly AbsPath _ProgramDir;

      /// <summary>
      /// Возвращает версию офиса
      /// </summary>
      public Version Version { get { return _Version; } }
      private /*readonly */ Version _Version;

      /// <summary>
      /// Разрядность приложения
      /// </summary>
      public OpenOfficePlatform Platform { get { return _Platform; } }
      private readonly OpenOfficePlatform _Platform;


      /// <summary>
      /// Возвращает абсолютный путь к файлу soffice.exe (или soffice под Linux).
      /// </summary>
      public AbsPath OfficePath
      {
        get
        {
          return new AbsPath(ProgramDir, "soffice" + GetExeExtension());
        }
      }

      /// <summary>
      /// Возвращает "LibreOffice" или "OpenOffice"
      /// </summary>
      public string KindName
      {
        get
        {
          switch (Kind)
          {
            case OpenOfficeKind.LibreOffice: return "LibreOffice";
            case OpenOfficeKind.OpenOffice: return "OpenOffice";
            default: return "Unknown Office";
          }
        }
      }

      #endregion

      #region Наличие отдельных компонентов

      #region Writer

      /// <summary>
      /// Возвращает true, если компонент "Writer" установлен
      /// </summary>
      public bool HasWriter { get { return _HasWriter; } }
      private readonly bool _HasWriter;

      /// <summary>
      /// Возвращает полный путь к файлу swriter.exe
      /// </summary>
      public AbsPath WriterPath
      {
        get
        {
          if (HasWriter)
            return new AbsPath(ProgramDir, "swriter" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// Возвращает "OpenOffice Writer" или "LibreOffice Writer"
      /// </summary>
      public string WriterDisplayName
      {
        get { return KindName + " Writer"; }
      }

      #endregion

      #region Calc

      /// <summary>
      /// Возвращает true, если компонент "Calc" установлен
      /// </summary>
      public bool HasCalc { get { return _HasCalc; } }
      private readonly bool _HasCalc;

      /// <summary>
      /// Возвращает полный путь к файлу scalc.exe
      /// </summary>
      public AbsPath CalcPath
      {
        get
        {
          if (HasCalc)
            return new AbsPath(ProgramDir, "scalc" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// Возвращает "OpenOffice Calc" или "LibreOffice Calc"
      /// </summary>
      public string CalcDisplayName
      {
        get { return KindName + " Calc"; }
      }

      #endregion

      #region Impress

      /// <summary>
      /// Возвращает true, если компонент "Impress" установлен
      /// </summary>
      public bool HasImpress { get { return _HasImpress; } }
      private readonly bool _HasImpress;

      /// <summary>
      /// Возвращает полный путь к файлу simpress.exe
      /// </summary>
      public AbsPath ImpressPath
      {
        get
        {
          if (HasImpress)
            return new AbsPath(ProgramDir, "simpress" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// Возвращает "OpenOffice Impress" или "LibreOffice Impress"
      /// </summary>
      public string ImpressDisplayName
      {
        get { return KindName + " Impress"; }
      }

      #endregion

      #region Draw

      /// <summary>
      /// Возвращает true, если компонент "Draw" установлен
      /// </summary>
      public bool HasDraw { get { return _HasDraw; } }
      private readonly bool _HasDraw;

      /// <summary>
      /// Возвращает полный путь к файлу sdraw.exe
      /// </summary>
      public AbsPath DrawPath
      {
        get
        {
          if (HasDraw)
            return new AbsPath(ProgramDir, "sdraw" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// Возвращает "OpenOffice Draw" или "LibreOffice Draw"
      /// </summary>
      public string DrawDisplayName
      {
        get { return KindName + " Draw"; }
      }

      #endregion

      #region Base

      /// <summary>
      /// Возвращает true, если компонент "Base" установлен
      /// </summary>
      public bool HasBase { get { return _HasBase; } }
      private readonly bool _HasBase;

      /// <summary>
      /// Возвращает полный путь к файлу sbase.exe
      /// </summary>
      public AbsPath BasePath
      {
        get
        {
          if (HasBase)
            return new AbsPath(ProgramDir, "sbase" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// Возвращает "OpenOffice Base" или "LibreOffice Base"
      /// </summary>
      public string BaseDisplayName
      {
        get { return KindName + " Base"; }
      }

      #endregion

      #region Math

      /// <summary>
      /// Возвращает true, если компонент "Math" установлен
      /// </summary>
      public bool HasMath { get { return _HasMath; } }
      private readonly bool _HasMath;

      /// <summary>
      /// Возвращает полный путь к файлу smath.exe
      /// </summary>
      public AbsPath MathPath
      {
        get
        {
          if (HasCalc)
            return new AbsPath(ProgramDir, "smath" + GetExeExtension());
          else
            return AbsPath.Empty;
        }
      }

      /// <summary>
      /// Возвращает "OpenOffice Math" или "LibreOffice Math"
      /// </summary>
      public string MathDisplayName
      {
        get { return KindName + " Math"; }
      }

      #endregion

      /// <summary>
      /// Возвращает строку с установленными компонентами, например, "Writer,Calc,Impress". Разделитель-запятые.
      /// Предназначена для отладочных целей
      /// </summary>
      public string ComponentsCSVString
      {
        get
        {
          StringBuilder sb = new StringBuilder();
          AddToCSV(sb, HasWriter, "Writer");
          AddToCSV(sb, HasWriter, "Calc");
          AddToCSV(sb, HasWriter, "Impress");
          AddToCSV(sb, HasWriter, "Draw");
          AddToCSV(sb, HasWriter, "Base");
          AddToCSV(sb, HasWriter, "Math");
          return sb.ToString();
        }
      }

      private static void AddToCSV(StringBuilder sb, bool flag, string name)
      {
        if (!flag)
          return;
        if (sb.Length > 0)
          sb.Append(',');
        sb.Append(name);
      }

      #endregion

      #region Прочие свойства

      /// <summary>
      /// Как была найдена эта копия офиса (через реестр Windows, переменную окружения ...)
      /// </summary>
      public InfoSource InfoSource { get { return _InfoSource; } }
      private readonly InfoSource _InfoSource;

      /// <summary>
      /// Дополнительная информация, как была найдена эта копия (ключ реестра или имя переменной окружения)
      /// </summary>
      public string InfoSourceString { get { return _InfoSourceString; } }
      private readonly string _InfoSourceString;

      /// <summary>
      /// Возвращает название и версию офиса
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return KindName + " " + Version.ToString() + PlatformSuffix;
      }

      private string PlatformSuffix
      {
        get
        {
          switch (Platform)
          {
            case OpenOfficePlatform.x86:
              return " (32-bit)";
            case OpenOfficePlatform.x64:
              return " (64-bit)";
            default:
              return String.Empty;
          }
        }
      }

      #endregion

      #region Открытие файла в Open Office

      /// <summary>
      /// Открыть файл текстового документа в редакторе OpenOffice / LibreOffice Writer
      /// </summary>
      /// <param name="fileName">Полный путь к ODT-файлу</param>
      /// <param name="asTemplate">Если true, то файл используется как шаблон.
      /// В заголовке не будет показано имя файла, а команда "Сохранить" предложит выбрать имя файла.
      /// Используется для реализации команд "Отправить"</param>
      public void OpenWithWriter(AbsPath fileName, bool asTemplate)
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = WriterPath.Path;
        if (String.IsNullOrEmpty(psi.FileName))
          throw new BugException("Программа Writer не установлена");
        psi.Arguments = "\"" + fileName.Path + "\"";
        if (asTemplate)
          psi.Arguments = "-n " + psi.Arguments;
        using (new FileRedirectionSupressor())
        {
          Process.Start(psi);
        }
      }

      /// <summary>
      /// Открыть файл табличного документа в программе OpenOffice / LibreOffice Calc
      /// </summary>
      /// <param name="fileName">Полный путь к ODS-файлу</param>
      /// <param name="asTemplate">Если true, то файл используется как шаблон.
      /// В заголовке не будет показано имя файла, а команда "Сохранить" предложит выбрать имя файла.
      /// Используется для реализации команд "Отправить"</param>
      public void OpenWithCalc(AbsPath fileName, bool asTemplate)
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = CalcPath.Path;
        if (String.IsNullOrEmpty(psi.FileName))
          throw new BugException("Программа Calc не установлена");
        psi.Arguments = "\"" + fileName.Path + "\"";
        if (asTemplate)
          psi.Arguments = "-n " + psi.Arguments;

        using (new FileRedirectionSupressor())
        {
          Process.Start(psi);
        }
      }

      #endregion
    }

    /// <summary>
    /// Список обнаруженных копий OpenOffice и LibreOffice.
    /// Если офис установлен, то обычно массив содержит один элемент.
    /// Однако, могут быть установлено несколько различных копий офиса.
    /// В этом случае обычно следует пользоваться "предпочтительной" копией, на которую указывает элемент с индексом 0.
    /// Если нет установленного офиса, возвращается пустой массив.
    /// Если приложение использует ExtForms.dll, для определения "действуюшей" копии следует использовать свойство EFPApp.UsedOpenOffice
    /// </summary>
    public static OfficeInfo[] Installations { get { return _Installations; } }
    private static OfficeInfo[] _Installations = InitInstallations();

    #region Поиск установленных копий

    private static OfficeInfo[] InitInstallations()
    {
      // Этот метод не имеет права выбрасывать исключения.
      // Нельзя даже вывести исключение в log-файл
      try
      {
        List<OfficeInfo> lst = new List<OfficeInfo>();

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
        return new OfficeInfo[0];
      }
    }

    private static void FindFromUnoPath(List<OfficeInfo> srcList)
    {
      string s = Environment.GetEnvironmentVariable("UNO_PATH");
      FindOrAddItem(srcList, new AbsPath(s), OpenOfficeKind.Unknown, InfoSource.EnvironmentVariable, "UNO_PATH", OpenOfficePlatform.Unknown);
    }

    #region Поиск для Windows

    private static void FindFromRegistry(List<OfficeInfo> lst)
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


    private static void FindFromRegistry2(RegistryTree2 tree, string keyNameBase, List<OfficeInfo> lst, OpenOfficePlatform platform)
    {
      FindFromRegistry3(tree, keyNameBase + @"OpenOffice\UNO\InstallPath", lst, OpenOfficeKind.OpenOffice, platform); // 18.05.2016 - для OpenOffice 4.1.2
      FindFromRegistry3(tree, keyNameBase + @"OpenOffice.org\UNO\InstallPath", lst, OpenOfficeKind.OpenOffice, platform);
      FindFromRegistry3(tree, keyNameBase + @"LibreOffice\UNO\InstallPath", lst, OpenOfficeKind.LibreOffice, platform);
    }

    private static void FindFromRegistry3(RegistryTree2 tree, string keyName, List<OfficeInfo> lst, OpenOfficeKind kind, OpenOfficePlatform platform)
    {
      // 30.09.2013
      // Может не быть доступа к ключу реестра
      try
      {
        AbsPath programDir = new AbsPath(tree.GetString(keyName, String.Empty));
        if (programDir.IsEmpty)
          return;

        FindOrAddItem(lst, programDir, kind, InfoSource.Registry, keyName, platform);
      }
      catch
      {
      }
    }

    #endregion

    #region Поиск для Linux

    private static void FindFromPath(List<OfficeInfo> lst)
    {
      string pathVar = Environment.GetEnvironmentVariable("PATH");
      if (String.IsNullOrEmpty(pathVar))
        return;

      string[] a = pathVar.Split(System.IO.Path.PathSeparator);
      for (int i = 0; i < a.Length; i++)
        FindOrAddItem(lst, new AbsPath(a[i]), OpenOfficeKind.Unknown, InfoSource.EnvironmentVariable, "Path", OpenOfficePlatform.Unknown);
    }

    private static void FindFromPredefined(List<OfficeInfo> lst)
    {
      FindFromPredefined_lib(lst, "lib", OpenOfficePlatform.Unknown);
      FindFromPredefined_lib(lst, "lib32", OpenOfficePlatform.x86); // 20.07.2022
      FindFromPredefined_lib(lst, "lib64", OpenOfficePlatform.x64); // 20.07.2022
    }
    private static void FindFromPredefined_lib(List<OfficeInfo> lst, string lib, OpenOfficePlatform platform)
    {
      AbsPath dir = new AbsPath("/usr/"+lib+"/libreoffice/program");
      if (File.Exists(new AbsPath(dir, "soffice").Path))
        FindOrAddItem(lst, dir, OpenOfficeKind.LibreOffice, InfoSource.PredefinedPath, String.Empty, OpenOfficePlatform.Unknown);
      dir = new AbsPath("/usr/"+lib+"/openoffice/program"); // !! проверить имя папки
      if (File.Exists(new AbsPath(dir, "soffice").Path))
        FindOrAddItem(lst, dir, OpenOfficeKind.OpenOffice, InfoSource.PredefinedPath, String.Empty, OpenOfficePlatform.Unknown);
    }

    #endregion

    #region Вспомогательные методы поиска

    private static void FindOrAddItem(List<OfficeInfo> lst, AbsPath programDir, OpenOfficeKind kind, InfoSource infoSource, string infoSourceString, OpenOfficePlatform platform)
    {
      if (programDir.IsEmpty)
        return;

      if (!Directory.Exists(programDir.Path))
        return; // пустышка

      AbsPath sofficePath = new AbsPath(programDir, "soffice" + GetExeExtension());
      if (!File.Exists(sofficePath.Path))
        return;

      if (Environment.OSVersion.Platform == PlatformID.Unix)
      {
        AbsPath sofficeBinPath = new AbsPath(programDir, "soffice.bin");
        if (!File.Exists(sofficeBinPath.Path))
          return; // soffice может быть символьной ссылкой. Проверка не реализована
      }

      // Во избежание повторов, проверяем наличие в списке такого же пути
      for (int i = 0; i < lst.Count; i++)
      {
        if (lst[i].ProgramDir == programDir)
          return;
      }

      lst.Add(new OfficeInfo(programDir, kind, infoSource, infoSourceString, platform));
    }

    private static string GetExeExtension()
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

    #endregion

    #endregion

    /// <summary>
    /// Обновляет массив Installations.
    /// </summary>
    public static void RefreshInstalls()
    {
      _Installations = InitInstallations();
    }

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
      string language = ci.Name.Substring(0, 2);
      string country = String.Empty;
      if (ci.Name.Length == 5)
        country = ci.Name.Substring(3, 2);
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
    /// <param name="NumberFormat"></param>
    /// <param name="DateTimeFormat"></param>
    /// <param name="Language"></param>
    /// <param name="Country"></param>
    /// <returns>true - стиль добавлен. false - в текущей реализации данный формат
    /// не преобразуется</returns>
    public static bool ODFAddFormat(XmlElement elStyles, string formatText, string styleName,
      NumberFormatInfo NumberFormat, DateTimeFormatInfo DateTimeFormat, string Language, string Country)
    {
      if (String.IsNullOrEmpty(formatText))
        return false;

      if (DataTools.IndexOfAny(formatText, "yMdhmsDtTfFgGRruUY") >= 0)
        return ODFAddDateTimeFormat(elStyles, formatText, styleName,
          CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat, "ru", "RU");

      if (formatText.IndexOf('0') >= 0)
        return ODFAddNumberFormat(elStyles, formatText, styleName/*, NumberFormat*/);

      return false;
    }

    private static bool ODFAddNumberFormat(XmlElement elStyles, string formatText, string styleName/*,
      NumberFormatInfo NumberFormat*/)
    {
      XmlElement elStyle = elStyles.OwnerDocument.CreateElement("number:number-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", styleName, nmspcStyle);

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
      DateTimeFormatInfo formatInfo, string language, string Country)
    {
      // Заменяем стандартные стили
      switch (formatText)
      {
        case "d": formatText = formatInfo.ShortDatePattern; break;
        case "D": formatText = formatInfo.LongDatePattern; break;
        case "t": formatText = formatInfo.ShortTimePattern; break;
        case "T": formatText = formatInfo.LongTimePattern; break;
        case "f": formatText = formatInfo.LongDatePattern + " " + formatInfo.ShortTimePattern; break;
        case "F": formatText = formatInfo.FullDateTimePattern; break;
        case "g": formatText = formatInfo.ShortDatePattern + " " + formatInfo.ShortTimePattern; break;
        case "G": formatText = formatInfo.ShortDatePattern + " " + formatInfo.LongTimePattern; break;
        case "M":
        case "m": formatText = formatInfo.MonthDayPattern; break;
        case "R":
        case "r": formatText = formatInfo.RFC1123Pattern; break;
        case "s": formatText = formatInfo.SortableDateTimePattern; break;
        case "u": formatText = formatInfo.UniversalSortableDateTimePattern; break;
        case "U": formatText = formatInfo.FullDateTimePattern; break;
        case "Y":
        case "y": formatText = formatInfo.YearMonthPattern; break;
      }


      XmlElement elStyle = elStyles.OwnerDocument.CreateElement(IsTimeOnlyFormat(formatText) ? "number:time-style" : "number:date-style", nmspcNumber);
      elStyles.AppendChild(elStyle);
      SetAttr(elStyle, "style:name", styleName, nmspcStyle);
      SetAttr(elStyle, "number:language", language, nmspcNumber);
      SetAttr(elStyle, "number:country", Country, nmspcNumber);

      XmlElement elPart;
      string AllMaskChars = "yMdhms";

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
            case 'h':
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="formatText"></param>
    /// <returns></returns>
    public static bool IsTimeOnlyFormat(string formatText)
    {
      if (String.IsNullOrEmpty(formatText))
        return false;
      switch (formatText)
      {
        case "d":
        case "D":
        case "f":
        case "F":
        case "g":
        case "G":
        case "M":
        case "m":
        case "R":
        case "r":
        case "s":
        case "u":
        case "U":
        case "Y":
        case "y":
          return false;
        case "t":
        case "T":
          return true;
      }

      return DataTools.IndexOfAny(formatText, "dMy") < 0;
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
