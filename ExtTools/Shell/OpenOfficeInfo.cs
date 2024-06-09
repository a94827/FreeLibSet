using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Win32;

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
    /// Установлен LibreOffice
    /// </summary>
    LibreOffice,

    /// <summary>
    /// Установлен AlterOffice.
    /// Имена файлов отличаются
    /// </summary>
    AlterOffice,
  }

  #endregion

  #region Перечисление OpenOfficePlatform

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

  #region Перечисление OpenOfficePart

  /// <summary>
  /// Компоненты OpenOffice, которые могут быть установлены
  /// </summary>
  public enum OpenOfficePart
  {
    /// <summary>
    /// Текстовый редактор
    /// </summary>
    Writer,

    /// <summary>
    /// Электронная таблица
    /// </summary>
    Calc,

    /// <summary>
    /// Презентации
    /// </summary>
    Impress,

    /// <summary>
    /// Графический редактор
    /// </summary>
    Draw,

    /// <summary>
    /// Доступ к базам данных
    /// </summary>
    Base,

    /// <summary>
    /// Редактор формул
    /// </summary>
    Math
  }

  #endregion

  /// <summary>
  /// Информация об одной установленной копии офиса.
  /// Для доступа используйте статическое свойство <see cref="OpenOfficeTools.Installations"/> или метод <see cref="OpenOfficeTools.GetPartInstallations(OpenOfficePart)"/>.
  /// </summary>
  public sealed class OpenOfficeInfo
  {
    #region Перечисление InfoSourceKind

    /// <summary>
    /// Откуда получена информация об установленной копии.
    /// Используется в отладочных целях.
    /// </summary>
    public enum InfoSourceKind
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
      PredefinedPath,

      /// <summary>
      /// Из файловой ассоциации
      /// </summary>
      FileAssociation,
    }

    #endregion

    #region Конструктор

    /// <summary>
    /// Возможное количество компонентов
    /// </summary>
    internal const int PartCount = 6;

    /// <summary>
    /// Версия конструктора с указанием платформы
    /// </summary>
    /// <param name="programDir">Каталог с программными файлами (в котором находится soffice.exe или soffice</param>
    /// <param name="kind">OpenOffice или LibeOffice</param>
    /// <param name="infoSource">Откуда получена информация об установленной копии</param>
    /// <param name="infoSourceString">Дополнительная информация, как была найдена эта копия (ключ реестра или имя переменной окружения)</param>
    /// <param name="platform">32-bit или 64-bit</param>
    internal OpenOfficeInfo(AbsPath programDir, OpenOfficeKind kind, InfoSourceKind infoSource, string infoSourceString, OpenOfficePlatform platform)
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

      _Parts = new OpenOfficePartInfo[PartCount];

      for (int i = 0; i < PartCount; i++)
      {
        if (IsCompExists(GetCompFileName((OpenOfficePart)i)))
          _Parts[i] = new OpenOfficePartInfo(this, (OpenOfficePart)i);
      }

      #endregion
    }

    internal string GetCompFileName(OpenOfficePart part)
    {
      switch (part)
      {
        case OpenOfficePart.Writer: return Kind == OpenOfficeKind.AlterOffice ? "atext" : "swriter";
        case OpenOfficePart.Calc: return Kind == OpenOfficeKind.AlterOffice ? "acell" : "scalc";
        case OpenOfficePart.Impress: return Kind == OpenOfficeKind.AlterOffice ? "aconcept" : "simpress";
        case OpenOfficePart.Draw: return Kind == OpenOfficeKind.AlterOffice ? "agraph" : "sdraw";
        case OpenOfficePart.Base: return Kind == OpenOfficeKind.AlterOffice ? "abase" : "sbase";
        case OpenOfficePart.Math: return Kind == OpenOfficeKind.AlterOffice ? "amath" : "smath";
        default:
          throw new ArgumentException();
      }
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
      catch (Exception e)
      {
        Trace.WriteLine("Error in InitVersionUnix_main_xcd(): " + e.Message);
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
      Trace.WriteLine("Unable to detect the version of " + this.KindName + " from path " + this.ProgramDir.Path);
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
      if (nl.Count == 0)
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

        if (aLines[i].StartsWith("BuildVersion=", StringComparison.Ordinal))
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
      AbsPath filePath = new AbsPath(ProgramDir, appName + OpenOfficeTools.GetExeExtension());
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
        if (Kind == OpenOfficeKind.AlterOffice)
          return new AbsPath(ProgramDir, "aoffice" + OpenOfficeTools.GetExeExtension());
        else
          return new AbsPath(ProgramDir, "soffice" + OpenOfficeTools.GetExeExtension());
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
          case OpenOfficeKind.AlterOffice: return "AlterOffice";
          default: return "Unknown Office";
        }
      }
    }

    #endregion

    #region Наличие отдельных компонентов

    /// <summary>
    /// Реализация свойства <see cref="OpenOfficeInfo.Parts"/>
    /// </summary>
    public struct PartCollection : IEnumerable<OpenOfficePartInfo>
    {
      #region Конструктор

      internal PartCollection(OpenOfficeInfo office)
      {
        _Office = office;
      }

      #endregion

      #region Свойства

      private OpenOfficeInfo _Office;

      /// <summary>
      /// Доступ к определенному компоненту.
      /// Если компонент установлен, возвращается описание <see cref="OpenOfficePartInfo"/>.
      /// Если компонент не установлен, возвращается null.
      /// Для просмотра только установленных компонентов, используйте перечислитель
      /// </summary>
      /// <param name="part">Требуемый компонент</param>
      /// <returns>Описание или null</returns>
      public OpenOfficePartInfo this[OpenOfficePart part]
      {
        get
        {
          return _Office._Parts[(int)part];
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает true, если компонент установлен
      /// </summary>
      /// <param name="part">Компонент</param>
      /// <returns>Признак установки</returns>
      public bool Contains(OpenOfficePart part)
      {
        return this[part] != null;
      }

      #endregion

      #region Перечислитель

      /// <summary>
      /// Перечислитель по установленным компонентам
      /// </summary>
      public struct Enumerator : IEnumerator<OpenOfficePartInfo>
      {
        #region Конструктор

        internal Enumerator(OpenOfficeInfo office)
        {
          _Office = office;
          _Index = -1;
        }

        #endregion

        #region Поля

        private readonly OpenOfficeInfo _Office;
        private int _Index;

        #endregion

        #region IEnumerator

        /// <summary>
        /// Текущий перебираемый компонент
        /// </summary>
        public OpenOfficePartInfo Current { get { return _Office._Parts[_Index]; } }

        object IEnumerator.Current { get { return Current; } }

        /// <summary>
        /// Ничего не делает
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Переход к следующему установленному компоненту
        /// </summary>
        /// <returns>Есть очередной компонент</returns>
        public bool MoveNext()
        {
          while (true)
          {
            _Index++;
            if (_Index >= OpenOfficeInfo.PartCount)
              return false;

            if (_Office._Parts[_Index] != null)
              break;
          }
          return true;
        }

        void IEnumerator.Reset()
        {
          _Index = -1;
        }

        #endregion
      }

      /// <summary>
      /// Создает перечислитель по установленным компонентам офиса
      /// </summary>
      /// <returns></returns>
      public Enumerator GetEnumerator()
      {
        return new Enumerator(_Office);
      }

      IEnumerator<OpenOfficePartInfo> IEnumerable<OpenOfficePartInfo>.GetEnumerator()
      {
        return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Список установленных компонентов
    /// </summary>
    public PartCollection Parts { get { return new PartCollection(this); } }

    private OpenOfficePartInfo[] _Parts;


    /// <summary>
    /// Возвращает строку с установленными компонентами, например, "Writer,Calc,Impress". Разделитель-запятые.
    /// Предназначена для отладочных целей
    /// </summary>
    public string ComponentsCSVString
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        foreach (OpenOfficePartInfo partInfo in Parts)
        {
          if (sb.Length > 0)
            sb.Append(',');
          sb.Append(partInfo.Part.ToString());
        }
        return sb.ToString();
      }
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Как была найдена эта копия офиса (через реестр Windows, переменную окружения ...)
    /// </summary>
    public InfoSourceKind InfoSource { get { return _InfoSource; } }
    private readonly InfoSourceKind _InfoSource;

    /// <summary>
    /// Дополнительная информация, как была найдена эта копия (ключ реестра или имя переменной окружения)
    /// </summary>
    public string InfoSourceString { get { return _InfoSourceString; } }
    private readonly string _InfoSourceString;

    /// <summary>
    /// Возвращает название и версию офиса
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return KindName + " " + Version.ToString() + PlatformSuffix;
    }

    internal string PlatformSuffix
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
  }

  /// <summary>
  /// Информация об одном установленном компоненте OpenOffice.
  /// Доступ к установленным компонентам возможен через свойство <see cref="OpenOfficeInfo.Parts"/>
  /// </summary>
  public sealed class OpenOfficePartInfo
  {
    #region Конструктор

    internal OpenOfficePartInfo(OpenOfficeInfo office, OpenOfficePart part)
    {
      _Office = office;
      _Part = part;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Офис, к которому относится компонент
    /// </summary>
    public OpenOfficeInfo Office { get { return _Office; } }
    private readonly OpenOfficeInfo _Office;

    /// <summary>
    /// Тип компонента
    /// </summary>
    public OpenOfficePart Part { get { return _Part; } }
    private readonly OpenOfficePart _Part;

    /// <summary>
    /// Возвращает полный путь к выполняемому файлу компонента
    /// </summary>
    public AbsPath Path
    {
      get
      {
        return new AbsPath(_Office.ProgramDir, _Office.GetCompFileName(_Part) + OpenOfficeTools.GetExeExtension());
      }
    }

    private static readonly string[] _AlterOfficePartNames = new string[OpenOfficeInfo.PartCount] { "AText", "ACell", "AConcept", "AGraph", "ABase", "AMath" };

    /// <summary>
    /// Возвращает название компонента в виде "OpenOffice Writer"
    /// </summary>
    public string DisplayName
    {
      get
      {
        string partName;
        if (_Office.Kind == OpenOfficeKind.AlterOffice)
          partName = _AlterOfficePartNames[(int)_Part];
        else
          partName = _Part.ToString();

        return _Office.KindName + " " + partName;
      }
    }

    /// <summary>
    /// Возвращает текстовое представление компонента, например, "LibrweOffice Calc 5.4.7.2 (32-bit)"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName + " "+ _Office.Version.ToString() + _Office.PlatformSuffix;
    }

    #endregion

    #region Открытие файла

    /// <summary>
    /// Открыть файл документа в редакторе OpenOffice / LibreOffice 
    /// </summary>
    /// <param name="fileName">Полный путь к открываемому файлу</param>
    /// <param name="asTemplate">Если true, то файл используется как шаблон.
    /// В заголовке не будет показано имя файла, а команда "Сохранить" предложит выбрать имя файла.
    /// Используется для реализации команд "Отправить"</param>
    public void OpenFile(AbsPath fileName, bool asTemplate)
    {
      ProcessStartInfo psi = new ProcessStartInfo();
      psi.FileName = Path.Path;

      psi.Arguments = "\"" + fileName.Path + "\"";
      if (asTemplate)
        psi.Arguments = "-n " + psi.Arguments;

      using (new Wow64FileRedirectionSupressor())
      {
        Process.Start(psi);
      }
    }

    #endregion

    #region Файловые ассоциации

    /// <summary>
    /// Возвращает файловую ассоциацию для открытия файла.
    /// </summary>
    public FileAssociationItem FileAssociation
    {
      get
      {
        if (_FileAssociation == null)
          _FileAssociation = GetFileAssociation();
        return _FileAssociation;
      }
    }
    private FileAssociationItem _FileAssociation;

    internal static readonly string[] PartMimeTypes = new string[OpenOfficeInfo.PartCount] {
      "application/vnd.oasis.opendocument.text",
      "application/vnd.oasis.opendocument.spreadsheet",
      "application/vnd.oasis.opendocument.presentation",
      "application/vnd.oasis.opendocument.graphics",
      "application/vnd.oasis.opendocument.database",
      "application/vnd.oasis.opendocument.formula" };

    internal static readonly string[] PartFileExts = new string[OpenOfficeInfo.PartCount] { ".odt", ".ods", ".odp", ".odg", ".odb", ".odf" };

    private FileAssociationItem GetFileAssociation()
    {
      FileAssociations fas = FileAssociations.FromMimeType(PartMimeTypes[(int)_Part]);
      FileAssociationItem fa = SelectFileAssociation(fas);
      if (fa != null)
        return fa;

      fas = FileAssociations.FromFileExtension(PartFileExts[(int)_Part]);
      fa = SelectFileAssociation(fas);
      if (fa != null)
        return fa;

      return new FileAssociationItem(Office.Kind.ToString() + "." + Part.ToString(), Path, "%1", DisplayName);
    }

    private FileAssociationItem SelectFileAssociation(FileAssociations fas)
    {
      FileAssociationItem fa = DoSelectFileAssociation(fas, false);
      if (fa != null)
        return fa;
      return DoSelectFileAssociation(fas, true);
    }

    private FileAssociationItem DoSelectFileAssociation(FileAssociations fas, bool useKindName)
    {
      foreach (FileAssociationItem fa in fas)
      {
        if (IsOurFileAssociation(fa, useKindName))
          return fa;
      }
      return null;
    }

    private bool IsOurFileAssociation(FileAssociationItem fa, bool useKindName)
    {
      if (fa == null)
        return false;

      if (useKindName)
      {
        if (fa.ProgId.StartsWith(Office.Kind.ToString(), StringComparison.OrdinalIgnoreCase))
          return true;
      }
      else
      {
        if (fa.ProgramPath.ParentDir == Office.OfficePath.ParentDir)
          return true;
      }
      return false;
    }

    #endregion
  }
}
