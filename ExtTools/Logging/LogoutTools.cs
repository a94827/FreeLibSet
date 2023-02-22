// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

//#define TRACE_LOGOUT // Используется для отладки самой системы logout

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Runtime;
using FreeLibSet.IO;
using FreeLibSet.Caching;
using System.Runtime.Remoting;
using System.Threading;
using System.Xml;
using System.Runtime.Remoting.Proxies;
using System.Runtime.InteropServices;
using FreeLibSet.Diagnostics;
using FreeLibSet.Win32;
using FreeLibSet.Core;
using FreeLibSet.Calendar;
using FreeLibSet.Data;

namespace FreeLibSet.Logging
{
  /*
   * Назначение
   * ----------
   * 1. Вывод информации при возникновении исключительной ситуации (метод LogoutException())
   * 2. Получение информации о приложении в текстовом виде для отладочных целей
   */

  #region Делегаты

  /// <summary>
  /// Аргументы события LogoutTools.LogoutInfoNeeded
  /// </summary>
  public sealed class LogoutInfoNeededEventArgs
  {
    #region Конструкторы

    /// <summary>
    /// Создает экземпляр объекта.
    /// Обычно не требуется создавать его в пользовательском коде
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="exception">Объект исключения или null</param>
    public LogoutInfoNeededEventArgs(TraceListener listener, Exception exception)
    {
      if (listener == null)
        throw new ArgumentNullException("listener");
      _Listener = listener;
      _Exception = exception;
    }

    #endregion

    #region Основные свойства и методы

    /// <summary>
    /// Объект исключения, для которого вызван LogoutException().
    /// Возвращает null, если вызван GetDebugInfo()
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private readonly Exception _Exception;

    /// <summary>
    /// Вывести строку текста. Слева будет добавлен отступ в соответствии с IndentLevel
    /// </summary>
    /// <param name="text">Записываемый текст</param>
    public void WriteLine(string text)
    {
      if (text == null)
        text = String.Empty;

      if (text.Contains(Environment.NewLine))
      {
        // 09.06.2015
        // Выводим каждую строку отдельно, чтобы не потерять отступы
        string[] a = text.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
        for (int i = 0; i < a.Length; i++)
          _Listener.WriteLine(CorrectText(a[i]));
      }
      else
        _Listener.WriteLine(CorrectText(text));
    }

    /// <summary>
    /// 19.02.2016
    /// В отладочной информации не должно быть управляющих символов CHR(0) - CHR(31),
    /// т.к. при этом может быть обрыв текста без выбрасывания исключений
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string CorrectText(string text)
    {
      #region Поиск плохого символа

      bool badFound = false;
      for (int i = 0; i < text.Length; i++)
      {
        if (text[i] < ' ')
        {
          badFound = true;
          break;
        }
      }

      #endregion

      if (!badFound)
        return text;

      #region Замена

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < text.Length; i++)
      {
        if (text[i] < ' ')
        {
          sb.Append(@"\x");
          sb.Append(((int)text[i]).ToString("X2"));
        }
        else
          sb.Append(text[i]);
      }

      return sb.ToString();

      #endregion
    }

    /// <summary>
    /// Текущий уровень отступа слева от текста. Методы, увеличивающие значение свойства, должны восстанавливать исходное значение.
    /// Конкретное число символов, соответствующее единичному отступа, определяется свойство IndentSize
    /// </summary>
    public int IndentLevel
    {
      get { return _Listener.IndentLevel; }
      set { _Listener.IndentLevel = value; }
    }

    /// <summary>
    /// Количество пробелов, задающее один уровень отступа
    /// </summary>
    public int IndentSize { get { return _Listener.IndentSize; } }

    private readonly TraceListener _Listener;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Вывести пустую строку
    /// </summary>
    public void WriteLine()
    {
      _Listener.WriteLine(String.Empty);
    }

    /// <summary>
    /// Вывести заголовочную строку, для которой выполняется центрирование
    /// </summary>
    /// <param name="text">Текст заголовка</param>
    public void WriteHeader(string text)
    {
      if (String.IsNullOrEmpty(text))
        WriteLine(new string('=', 60));
      else if (text.Length > 58)
        WriteLine(text);
      else
        WriteLine(DataTools.PadCenter(" " + text + " ", 60, '='));
    }

    /// <summary>
    /// Запись пары "Имя=Значение"
    /// </summary>
    /// <param name="name">Текст слева от от знака равенства. Может быть пустой строкой или null</param>
    /// <param name="value">Текст справа от от знака равенства. Может быть пустой строкой или null</param>
    public void WritePair(string name, string value)
    {
      if (name == null)
        name = String.Empty;
      if (value == null)
        value = String.Empty;

      if (value.Contains(Environment.NewLine))
      {
        // 12.05.2016
        // Если значение содержит символ новой строки, то выводим несколько строк
        string[] a = value.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
        WriteLine(name.PadRight(25) + " = " + a[0]);
        string spc = new string(' ', 25 + 3);
        for (int i = 1; i < a.Length; i++)
          _Listener.WriteLine(spc + a[i]);
      }
      else
        WriteLine(name.PadRight(25) + " = " + value);

    }

    #endregion
  }

  /// <summary>
  /// Делегат события LogoutTools.LogoutInfoNeeded
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void LogoutInfoNeededEventHandler(object sender, LogoutInfoNeededEventArgs args);

  #region Перечисление LogoutPropMode

  /// <summary>
  /// Режим вывода свойства в log-файл
  /// </summary>
  public enum LogoutPropMode
  {
    /// <summary>
    /// Обычный вывод. Дочерние свойства показываются, если не превышен уровень вложения
    /// </summary>
    Default,

    /// <summary>
    /// Свойство выводится как одна строка
    /// </summary>
    ToString,

    /// <summary>
    /// Свойство пропускается
    /// </summary>
    None
  }

  #endregion

  /// <summary>
  /// Формат обработчика для свойства LogoutPropEventArgs.Handler
  /// </summary>
  /// <param name="args"></param>
  /// <param name="obj"></param>
  public delegate void LogoutObjectHandler(LogoutInfoNeededEventArgs args, object obj);

  /// <summary>
  /// Аргументы события LogoutTools.LogoutProp
  /// </summary>
  public sealed class LogoutPropEventArgs
  {
    #region Конструктор

    internal LogoutPropEventArgs(object obj, int level)
    {
#if DEBUG
      if (obj == null)
        throw new ArgumentNullException("obj");
#endif
      _Object = obj;
      _Level = level;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект, свойства которого предполагается выводить в log-файл
    /// Не может быть null
    /// </summary>
    public object Object { get { return _Object; } }
    private readonly object _Object;

    /// <summary>
    /// Уровень вложения. Для объекта, для которого был вызван метод LogoutObject(), возвращается значение 0. 
    /// Для объектов, возвращаемых его свойствами, свойство имеет значение 1, и т.д.
    /// </summary>
    public int Level { get { return _Level; } }
    private readonly int _Level;

    /// <summary>
    /// Очередное свойство, которое предполагается вывести.
    /// Перед выводом свойств, событие вызывается для объекта в-целом, чтобы можно было подавить вывод всех свойств.
    /// При этом PropertyName=String.Empty
    /// </summary>
    public string PropertyName
    {
      get { return _PropertyName; }
      internal set
      {
        _PropertyName = value;
        _Mode = LogoutPropMode.Default;
      }
    }
    private string _PropertyName;

    /// <summary>
    /// Устанавливаемое значение, что сделать со свойством: вывести свойство с вложенными свойствами, вывести только ToString() или пропуститиь свойство
    /// </summary>
    public LogoutPropMode Mode
    {
      get { return _Mode; }
      set { _Mode = value; }
    }
    private LogoutPropMode _Mode;

    /// <summary>
    /// Если при вызове события установить обработчик, то после вывода свойств (в зависимости от Mode) будет вызван этот обработчик
    /// для вывода дополнительной информации по объекту.
    /// Обработчик можно устанавливать только для объекта (когда PropertyName=""), а не для свойства
    /// </summary>
    public LogoutObjectHandler Handler
    {
      get { return _Handler; }
      set { _Handler = value; }
    }
    private LogoutObjectHandler _Handler;

    #endregion
  }

  /// <summary>
  /// Делегат события LogoutTools.LogoutProp
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void LogoutPropEventHandler(object sender, LogoutPropEventArgs args);

  /// <summary>
  /// Аргументы события LogoutTools.ExceptionLogFileCreated
  /// </summary>
  public sealed class ExceptionLogFileCreatedEventArgs : EventArgs
  {
    #region Конструктор

    internal ExceptionLogFileCreatedEventArgs(AbsPath filePath)
    {
      _FilePath = filePath;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к созданному log-файлу
    /// </summary>
    public AbsPath FilePath { get { return _FilePath; } }
    private readonly AbsPath _FilePath;

    #endregion
  }

  /// <summary>
  /// Делегат события LogoutTools.ExceptionLogFileCreated
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void ExceptionLogFileCreatedEventHandler(object sender,
    ExceptionLogFileCreatedEventArgs args);

  #endregion

  /// <summary>
  /// Средства для вывода отладочной информации в файл
  /// Все свойства и методы класса являются потокобезопасными, как и методы работы с событиями
  /// </summary>
  public static class LogoutTools
  {
    #region Log-файлы

    /// <summary>
    /// Путь к подкаталогу LOG
    /// Сам каталог не создается, требуется вызов ForceDirs()
    /// </summary>
    public static AbsPath LogBaseDirectory
    {
      get
      {
        if (_LogBaseDirectory.IsEmpty)
        {
          string appName = EnvironmentTools.ApplicationName;
          if (String.IsNullOrEmpty(appName))
            appName = "Application";
          // Так нельзя.
          // Нет разрешение на создание папок и файлов в ("/var/log"
          //if (Environment.OSVersion.Platform==PlatformID.Unix)
          //  return new AbsPath(new AbsPath("/var/log"), AppName); // 12.05.2016
          //else
          return new AbsPath(new AbsPath(Path.GetTempPath()), "Log", appName);
        }
        else
          return _LogBaseDirectory;
      }
      set
      {
        _LogBaseDirectory = value;
      }
    }
    private static AbsPath _LogBaseDirectory;

    /// <summary>
    /// Получить имя для LOG-файла.
    /// Генерируется уникальное имя файла в заданном подкаталоге в каталоге Log,
    /// с использованием префикса и расширением ".log". Имя базируется на текущей
    /// дате и времени с использованием дополнительного суффикса, если файл уже
    /// существует.
    /// Функция потокобезопасна и создает пустой файл-заглушку с полученным именем.
    /// Предварительно создается цепочка недостающих каталогов.
    /// </summary>
    /// <param name="subDir">Имя подкаталога в каталоге Log, например, "Errors".
    /// Имя без слэшей в начале и конце, но могут быть слэши внутри.
    /// Может быть пустой строкой или null</param>
    /// <param name="prefix">Префикс имени файла. Может быть пустой строкой или null</param>
    /// <returns>Путь к файлу</returns>
    public static AbsPath GetLogFileName(string subDir, string prefix)
    {
      AbsPath fileName = AbsPath.Empty;
      lock (DataTools.InternalSyncRoot)
      {
        AbsPath dir = LogBaseDirectory;
        if (!String.IsNullOrEmpty(subDir))
          dir += subDir;
        FileTools.ForceDirs(dir);
        DateTime dt = DateTime.Now;
        if (prefix == null)
          prefix = String.Empty;
        string fileNameBase = dir.SlashedPath + prefix + dt.ToString("yyyyMMddHHmmss");
        fileName = new AbsPath(fileNameBase + ".log");
        int cnt = 0;
        while (File.Exists(fileName.Path))
        {
          if (cnt >= 100)
            throw new InvalidOperationException("Не удалось придумать уникальное имя файла");
          cnt++;
          fileName = new AbsPath(fileNameBase + cnt.ToString() + ".log");
        }

        // Создаем файл нулевой длины
        StreamWriter wrt = new StreamWriter(fileName.Path);
        wrt.Close();
      }
      return fileName;
    }

    /// <summary>
    /// Кодировка, используемая для log-файлов ошибок.
    /// Если свойство не установлено в явном виде, используется кодировка по умолчанию
    /// </summary>
    public static Encoding LogEncoding
    {
      get
      {
        if (_LogEncoding == null)
        {
          if (Encoding.Default.CodePage == 65001)
            return Encoding.UTF8; // 06.04.2018
          else
            return Encoding.Default; // В Mono, значение Encoding.Default возвращает кодировку без префикса EF BB BF 
        }
        else
          return _LogEncoding;
      }
      set
      {
        _LogEncoding = value;
      }
    }
    private static Encoding _LogEncoding;

    #endregion

    #region Таблица log-файлов

    /// <summary>
    /// Получить таблицу, содержащую список log-файлов.
    /// В таблице есть колонки:
    /// "Time" - время создания файла (исходя из имени, а не из атрибутов)
    /// "FilePath" - полный путь к файлу
    /// "ComputerName" - имя компьютера. Всегда заполняется значением Environment.MachineName.
    /// </summary>
    /// <param name="subDir">Имя подкаталога в каталоге Log, например, "Errors".
    /// Имя без слэшей в начале и конце, но могут быть слэши внутри.
    /// Может быть пустой строкой или null</param>
    /// <param name="prefix">Префикс имени файла. Может быть пустой строкой или null</param>
    /// <returns>Таблица со списком файлов</returns>
    public static DataTable CreateLogFileTable(string subDir, string prefix)
    {
      DataTable table = new DataTable("ErrorLogFiles");
      table.Columns.Add("Time", typeof(DateTime));
      table.Columns.Add("FilePath", typeof(string));
      table.Columns.Add("ComputerName", typeof(String));
      lock (DataTools.InternalSyncRoot)
      {
        AbsPath dir = LogBaseDirectory;
        if (!String.IsNullOrEmpty(subDir))
          dir += subDir;
        if (prefix == null)
          prefix = String.Empty;
        if (Directory.Exists(dir.Path))
        {
          string[] fileNames = Directory.GetFiles(dir.Path, prefix + "*.log");

          object compName = DBNull.Value;
          if (!String.IsNullOrEmpty(Environment.MachineName))
            compName = Environment.MachineName;

          for (int i = 0; i < fileNames.Length; i++)
          {
            string name1 = Path.GetFileNameWithoutExtension(fileNames[i]).Substring(prefix.Length);
            if (name1.Length < 14)
              continue; // старый формат не поддерживается
            name1 = name1.Substring(0, 14);
            DateTime time;
            if (!DateTime.TryParseExact(name1, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
              continue; // что-то неправильное

            // Подходящий файл
            DataRow row = table.NewRow();
            row["Time"] = time;
            row["FilePath"] = fileNames[i];
            row["ComputerName"] = compName;
            table.Rows.Add(row);
          }
        }
      }
      return table;
    }

    #endregion

    #region Пользовательские обработчики

    #region LogoutInfoNeeded

    /// <summary>
    /// Обработчик позволяет вывести в log-файл дополнительные сведения при возникновении исключительной ситуации (метод LogoutException())
    /// и при извлечении информации методом GetDebugInfo()
    /// Обработчик должен быть потокобезопасным
    /// </summary>
    public static event LogoutInfoNeededEventHandler LogoutInfoNeeded
    {
      add
      {
        lock (_LogoutInfoNeededList)
        {
          _LogoutInfoNeededList.Add(value);
#if TRACE_LOGOUT

          LogoutInfoNeededEventHandler [] Handlers = FLogoutInfoNeededList.ToArray();

          Trace.WriteLine("LogoutInfoNeededEventHandler #" + (Handlers.Length - 1).ToString() + " added. Stack trace:");
          Trace.WriteLine(Environment.StackTrace);
#endif
        }
      }
      remove
      {
        lock (_LogoutInfoNeededList)
        {
          _LogoutInfoNeededList.Remove(value);
        }
      }
    }
    private static readonly List<LogoutInfoNeededEventHandler> _LogoutInfoNeededList = new List<LogoutInfoNeededEventHandler>();

    [DebuggerStepThrough]
    private static void OnLogoutInfoNeeded(LogoutInfoNeededEventArgs args, LogoutDebugInfoSettings settings)
    {
      if (settings == null)
        throw new ArgumentNullException("settings");

#if TRACE_LOGOUT
      Trace.WriteLine("OnLogoutInfoNeeded started");
#endif

      int indentLevel = args.IndentLevel;

      bool outOfMemory = false; // 06.03.2018

      if (settings.AddSystemInfo)
      {
        try
        {
#if TRACE_LOGOUT
      Trace.WriteLine("LogoutSysInfo started");
#endif
          LogoutSysInfo(args);
#if TRACE_LOGOUT
      Trace.WriteLine("LogoutSysInfo finished");
#endif
        }
        catch (Exception e)
        {
          if (e is OutOfMemoryException)
            outOfMemory = true;

          args.IndentLevel++;
          DoLogoutException2(args, e, "*** Ошибка при добавлении системной информации ***", false);
        }
        args.IndentLevel = indentLevel; // восстанавливаем уровень отступов на случай сбоя
      }

      if (outOfMemory)
      {
        args.WriteLine("*** Вывод отладочной информации отменен из-за OutOfMemoryException ***");
        return;
      }

      if (settings.UseHandlers)
      {
        LogoutInfoNeededEventHandler[] handlers;
        lock (_LogoutInfoNeededList)
        {
          handlers = _LogoutInfoNeededList.ToArray();
        }

        for (int i = 0; i < handlers.Length; i++)
        {
#if TRACE_LOGOUT
      Trace.WriteLine("Handler #"+i.ToString()+" started");
#endif
          try
          {
            handlers[i](null, args);
#if TRACE_LOGOUT
      Trace.WriteLine("Handler #"+i.ToString()+" finished");
#endif
          }
          catch (Exception e)
          {
            if (e is OutOfMemoryException)
            {
              outOfMemory = true;
              break;
            }
            else
            {
              try
              {
                e.Data["LogoutTools.HandlerIndex"] = i;
                e.Data["LogoutTools.Handler"] = handlers[i].ToString(); // наверное, бесполезно
              }
              catch { }
            }
            args.IndentLevel++;
            DoLogoutException2(args, e, "*** Ошибка при добавлении информации обработчиком logout ***", false);
          }
          args.IndentLevel = indentLevel; // восстанавливаем уровень отступов на случай сбоя
        }
      }

      if (outOfMemory)
      {
        args.WriteLine("*** Вывод отладочной информации прекращен из-за OutOfMemoryException ***");
        return;
      }

      // Вызов дополнительного обработчика
      if (settings.AuxHandler != null)
      {
#if TRACE_LOGOUT
      Trace.WriteLine("Logout AuxHandler started");
#endif
        settings.AuxHandler(null, args);
#if TRACE_LOGOUT
      Trace.WriteLine("Logout AuxHandler finished");
#endif
      }

      if (settings.AddAssembliesInfo)
      {
#if TRACE_LOGOUT
      Trace.WriteLine("LogoutAssemblies started");
#endif
        LogoutAssemblies(args);
#if TRACE_LOGOUT
      Trace.WriteLine("LogoutAssemblies finished");
#endif
      }
    }

    #endregion

    #region LogoutProp

    /// <summary>
    /// Обработчик позволяет определить, какие свойства объекта следует выводить в log-файл, а какие нет
    /// Для большинства стандартных классов реализована обработка по умолчанию
    /// Если в программе требуется вывести в log-файл свойства собственных классов, может потребоваться обработчик
    /// для подавления вывода ненужных свойств
    /// Обработчик должен быть потокобезопасным.
    /// Обработчик вызывается для каждого выводимого в log-файл объекта (кроме объекта верхнего уровня, для которого вызван LogoutObject()),
    /// а затем - для каждого public не-static свойства этого объекта
    /// </summary>
    public static event LogoutPropEventHandler LogoutProp
    {
      add
      {
        lock (_LogoutPropList)
        {
          _LogoutPropList.Add(value);
        }
      }
      remove
      {
        lock (_LogoutPropList)
        {
          _LogoutPropList.Remove(value);
        }
      }
    }
    private static readonly List<LogoutPropEventHandler> _LogoutPropList = new List<LogoutPropEventHandler>();

    [DebuggerStepThrough]
    private static void OnLogoutProp(LogoutPropEventArgs args)
    {
      DefaultLogoutProp(args);

      LogoutPropEventHandler[] handlers;
      lock (_LogoutPropList)
      {
        handlers = _LogoutPropList.ToArray();
      }

      for (int i = 0; i < handlers.Length; i++)
      {
        try
        {
          handlers[i](null, args);
        }
        catch { } // Ошибки выводить некуда
      }
    }

    /// <summary>
    /// Подавление вывода свойств для стандартных типов
    /// </summary>
    /// <param name="args"></param>
    private static void DefaultLogoutProp(LogoutPropEventArgs args)
    {
      if (args.Object is Type ||
        args.Object.GetType().IsPrimitive ||
        args.Object is String ||
        args.Object is DateTime ||
        args.Object is TimeSpan || // Добавлено 29.11.2014
        args.Object is Version || // Добавлено 09.04.2015
        args.Object is XmlNodeList || // Добавлено 09.06.2015
        args.Object is MemberInfo || // Добавлено 10.06.2015
        args.Object is Uri || // Добавлено 04.07.2016
        args.Object is AbsPath ||
        args.Object is System.Security.Policy.Evidence || // Добавлено 23.09.2016
        args.Object is System.Net.IPAddress || // Добавлено 17.01.2017
        args.Object is DateRange ||  // 21.06.2021
        args.Object is YearMonth || // 21.06.2021
        args.Object is YearMonthRange // 21.06.2021
        )
      {
        if (args.Level > 0)
          args.Mode = LogoutPropMode.None;
        else
          args.Mode = LogoutPropMode.Default;
        return;
      }
      if (args.Object is XmlDocument)
      {
        args.Mode = LogoutPropMode.None;
        args.Handler = new LogoutObjectHandler(LogoutXmlDocument);
        return;
      }

      if (args.Object is Exception)
      {
        switch (args.PropertyName)
        {
          case "Message":
          case "StackTrace":
          case "InnerException":
          case "TargetSite":
            args.Mode = LogoutPropMode.None;
            break;
          default:
            args.Mode = LogoutPropMode.Default; // независимо от уровня вложения
            break;
        }
      }

      Type typ = args.Object.GetType();

      //if (Args.Object is IDictionary)
      if (typ.GetInterface("System.Collections.IDictionary") != null ||
        typ.GetInterface("System.Collections.Generic.IDictionary`2") != null)
      {
        switch (args.PropertyName)
        {
          case "Keys":
          case "Values":
          case "IsFixedSize":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is System.Collections.Specialized.StringDictionary)
      {
        // 16.04.2019 - StringDictionary почему-то не реализует интерфейс IDictionary
        switch (args.PropertyName)
        {
          case "Keys":
          case "Values":
          case "IsSynchronized":
          case "SyncRoot":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      //if (Args.Object is ICollection)
      if (typ.GetInterface("System.Collections.ICollection") != null ||
        typ.GetInterface("System.Collections.Generic.ICollection`1") != null)
      {
        switch (args.PropertyName)
        {
          case "SyncRoot":
          case "IsSynchronized":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is System.Threading.Thread)
      {
        switch (args.PropertyName)
        {
          case "CurrentCulture":
          case "CurrentUICulture":
            args.Mode = LogoutPropMode.ToString;
            break;
        }
      }

      // Добавлено 20.08.2015
      // Что-то не работает
      if (args.Object is System.Security.Policy.Hash)
      {
        args.Mode = LogoutPropMode.None; // выводится бескончная куча цифр
        return;
      }

      if (args.Object is AppDomain)
      {
        if (EnvironmentTools.IsMono)
        {
          args.Mode = LogoutPropMode.None;
          return; // 20.06.2017 В Mono все виснет
        }

        switch (args.PropertyName)
        {
          case "Evidence":
            args.Mode = LogoutPropMode.ToString;
            break;

          //case "FriendlyName": // 20.06.2017 Mono иногда аварийно завершается при получении этого свойства
          //case "SetupInformation": // 20.06.2017 В mono иногда зависает на получении полей класс AppDomainSetup
          //  if (EnvironmentTools.IsMono)
          //    Args.Mode = LogoutPropMode.None;
          //  break;

          // 07.01.2021
          // Эти свойства есть в Net Framework 4, но они выдают ошибку:
          // System.InvalidOperationException: "Когда слежение за ресурсами доменов приложений не включено, этот API недоступен"
          case "MonitoringTotalProcessorTime":
          case "MonitoringTotalAllocatedMemorySize":
          case "MonitoringSurvivedMemorySize":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is Delegate)
      {
        args.Mode = LogoutPropMode.Default;
        return;
      }

      if (args.Object is Array)
      {
        switch (args.PropertyName)
        {
          case "Length":
          case "LongLength":
          case "Rank":
            if (((Array)(args.Object)).Rank == 1)
              args.Mode = LogoutPropMode.None;
            break;
          case "IsReadOnly":
          case "IsFixedSize":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is System.Diagnostics.Switch) // 21.03.2017
      {
        switch (args.PropertyName)
        {
          case "Attributes":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is FileContainer) // 03.02.2018
      {
        switch (args.PropertyName)
        {
          case "Contents":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is ProcessStartInfo) // 27.10.2018
      {
        switch (args.PropertyName)
        {
          case "EnvironmentVariables":
          case "Verbs":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is TextWriter)
      {
        switch (args.PropertyName)
        {
          case "FormatProvider":
            args.Mode = LogoutPropMode.ToString; // 16.04.2019
            break;
        }
      }

      if (args.Object is CultureInfo) // 16.07.2020
      {
        string s = args.Object.ToString();
        if (s.Length > 0)
        {
          args.Mode = LogoutPropMode.ToString;
          return;
        }

        //CultureInfo ci=(CultureInfo )(args.Object);
        //if (ci==CultureInfo.InvariantCulture)

        switch (args.PropertyName)
        {
          case "Parent":
          case "CompareInfo":
          case "Calendar":
          case "OptionalCalendars":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is FileInfo) // 18.08.2021
      {
        switch (args.PropertyName)
        { 
          case "Directory":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }
      if (args.Object is DirectoryInfo) // 18.08.2021
      {
        switch (args.PropertyName)
        {
          case "Parent":
          case "Root":
            args.Mode = LogoutPropMode.None;
            break;
        }
      }

      if (args.Object is FileVersionInfo) // 09.02.2021
      {
        switch (args.PropertyName)
        {
          case "FileMajorPart":
          case "FileMinorPart":
          case "FileBuildPart":
          case "FilePrivatePart": // дублируются в свойстве "FileVersion"

          case "ProductMajorPart":
          case "ProductMinorPart":
          case "ProductBuildPart":
          case "ProductPrivatePart": // дублируются в свойстве "ProductVersion"

            args.Mode = LogoutPropMode.None;
            break;
        }
      }
    }


    private static void LogoutXmlDocument(LogoutInfoNeededEventArgs args, object obj)
    {
      XmlDocument xml = (XmlDocument)obj;
      string s = DataTools.XmlDocumentToString(xml);
      args.WriteLine(s);
    }

    #endregion

    #endregion

    #region EventLogException

    /// <summary>
    /// Записывает исключение в журнал EventLog
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок</param>
    public static void EventLogException(Exception e, string title)
    {
      EventLogException(e, title, EventLogEntryType.Error, AbsPath.Empty);
    }

    /// <summary>
    /// Записывает исключение в журнал EventLog
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок</param>
    /// <param name="entryType">Тип записи</param>
    public static void EventLogException(Exception e, string title, EventLogEntryType entryType)
    {
      EventLogException(e, title, entryType, AbsPath.Empty);
    }

    private static void EventLogException(Exception e, string title, EventLogEntryType entryType, AbsPath textFileName)
    {
      try
      {
        DoEventLogException(e, title, entryType, textFileName);
      }
      catch
      {
        BloodyHellSound();
      }
    }

    private static void DoEventLogException(Exception e, string title, EventLogEntryType entryType, AbsPath textFileName)
    {
      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        return;

      StringBuilder sb = new StringBuilder();
      sb.Append("Ошибка ");
      Assembly ass = Assembly.GetEntryAssembly();
      if (ass == null)
        sb.Append("[Нет GetEntryAssembly]");
      else
        sb.Append(ass.ToString());
      sb.Append(" (");
      sb.Append(Environment.CommandLine);
      sb.Append("). ");
      sb.Append(title);
      sb.Append(". ");
      sb.Append(e.Message);
      if (!textFileName.IsEmpty)
      {
        sb.Append(". LOG-файл: ");
        sb.Append(textFileName.Path);
      }

      EventLog evLog = new EventLog();
      evLog.Source = "Application";
      evLog.WriteEntry(sb.ToString(), entryType);
    }

    #endregion

    #region LogoutException

    /// <summary>
    /// Управляет наличием информации в log-файле исклжчения.
    /// Если true (по умолчанию), то метод LogoutException, после вывода свойств объекта Exception,
    /// выводит системную информацию и вызывает обработчики события LogoutInfoNeeded.
    /// Свойство можно установить в false, если критичным является размер log-файла, требуется быстрая
    /// обработка исключения или возникают проблемы при выводе отладочной информации.
    /// </summary>
    public static bool LogoutInfoForException
    {
      get { return _LogoutInfoForException; }
      set { _LogoutInfoForException = value; }
    }
    private static bool _LogoutInfoForException = true;

    private static bool _InsideLogoutException = false;

    /// <summary>
    /// Записывает исключение в log-файл
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок</param>
    public static void LogoutException(Exception e, string title)
    {
      LogoutExceptionToFile(e, title);
    }

    /// <summary>
    /// Записывает исключение в log-файл.
    /// Имя файла генерируется автоматически.
    /// Записывается сообщение в EventLog.
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок</param>
    /// <returns>Путь к созданному log-файлу на диске</returns>
    public static AbsPath LogoutExceptionToFile(Exception e, string title)
    {
      AbsPath filePath = AbsPath.Empty;
      lock (DataTools.InternalSyncRoot)
      {
        // В процессе перебора свойств может возникнуть исключение, которое может
        // быть перехвачено другим обработчиком, который, в свою очередь, выполнит
        // повторный вызов LogoutException

        if (!_InsideLogoutException)
        {
          _InsideLogoutException = true;
          try
          {
            filePath = GetLogFileName("Errors", null);

            try { Trace.WriteLine(DateTime.Now.ToString("G") + ". Exception log file \"" + filePath.Path + "\" creating... Exception class: " + e.GetType().ToString() + ". Message: " + e.Message); }
            catch { }


            LogoutExceptionToFile(filePath, e, title);
            try { Trace.WriteLine(DateTime.Now.ToString("G") + ". Exception log file \"" + filePath.Path + "\" created."); }
            catch { }

            EventLogException(e, title, EventLogEntryType.Warning, filePath);
          }
          catch (Exception e2)
          {
            // В случае ошибки помещаем запись в журнал событий Windows
            EventLogException(e2, "Ошибка при записи LOG-файла ошибки");
          }
          _InsideLogoutException = false;
        }
      }

      // 17.03.2017
      // Отправляем извещения
      OnExceptionLogFileCreated(filePath);

      return filePath;
    }

    /// <summary>
    /// Запись информации об исключении в произвольный файл.
    /// В отличие от методов LogoutException() и перегрузки LogoutExceptionToFile() с двумя аргументами,
    /// не делается запись в EventLog.
    /// Каталог для записи должен существовать.
    /// Вывод системной информации зависит от статического свойства LogoutInfoForException.
    /// </summary>
    /// <param name="filePath">Путь к записываемому файлу. Каталог должен существовать</param>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок исключения</param>
    public static void LogoutExceptionToFile(AbsPath filePath, Exception e, string title)
    {
      LogoutExceptionToFile(filePath, e, title, LogEncoding, LogoutInfoForException);
    }

    /// <summary>
    /// Запись информации об исключении в произвольный файл.
    /// В отличие от методов LogoutException() и перегрузки LogoutExceptionToFile() с двумя аргументами,
    /// не делается запись в EventLog.
    /// Каталог для записи должен существовать.
    /// </summary>
    /// <param name="filePath">Путь к записываемому файлу. Каталог должен существовать</param>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок исключения</param>
    /// <param name="encoding">Кодировка</param>
    /// <param name="logoutInfo">Если true, то метод LogoutException, после вывода свойств объекта Exception,
    /// выводит системную информацию и вызывает обработчики события LogoutInfoNeeded.</param>
    public static void LogoutExceptionToFile(AbsPath filePath, Exception e, string title, Encoding encoding, bool logoutInfo)
    {
      if (encoding == null)
        encoding = LogEncoding;

      StreamWriter wrt = new StreamWriter(filePath.Path, false, encoding);
      try
      {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
          wrt.AutoFlush = true; // временно 
        LogoutException(wrt, e, title, logoutInfo);
      }
      finally
      {
        wrt.Close();
        wrt.Dispose();
      }
    }

    /// <summary>
    /// Вывести исключение в объект TextWriter
    /// </summary>
    /// <param name="writer">Объект для записи текста</param>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок исключения</param>
    /// <param name="logoutInfo">Если true, то метод LogoutException, после вывода свойств объекта Exception,
    /// выводит системную информацию и вызывает обработчики события LogoutInfoNeeded.</param>
    public static void LogoutException(TextWriter writer, Exception e, string title, bool logoutInfo)
    {
      using (TextWriterTraceListener listener = new TextWriterTraceListener(writer))
      {
        listener.IndentSize = 2;
        LogoutInfoNeededEventArgs args = new LogoutInfoNeededEventArgs(listener, e);
        DoLogoutException2(args, e, title, logoutInfo);

        listener.Flush();
      }
    }

    /// <summary>
    /// Запись информации об исключении в строку
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок исключения</param>
    /// <param name="logoutInfo">Если true, то метод LogoutException, после вывода свойств объекта Exception,
    /// выводит системную информацию и вызывает обработчики события LogoutInfoNeeded.</param>
    public static string LogoutExceptionToString(Exception e, string title, bool logoutInfo)
    {
      StringWriter wrt = new StringWriter();
      LogoutException(wrt, e, title, logoutInfo);
      return wrt.ToString();
    }

    private static void DoLogoutException2(LogoutInfoNeededEventArgs args, Exception e, string title, bool logoutInfo)
    {
      if (!string.IsNullOrEmpty(title))
      {
        args.WriteLine(title);
        args.WriteLine("-----------------------");
        args.WriteLine();
      }

      // Предотвращаем зацикленный IntentalException
      Exception e1 = e;
      for (int i = 0; i < 10; i++)
      {
        if (e1 == null)
          break;
        DoLogoutOneException(args, e1, i == 0);
        e1 = e1.InnerException;
      }

      // Далее выводим обычную информацию
      if (logoutInfo) // 21.06.2017
        OnLogoutInfoNeeded(args, new LogoutDebugInfoSettings());
    }

    [DebuggerStepThrough]
    private static void DoLogoutOneException(LogoutInfoNeededEventArgs args, Exception e, bool topLevel)
    {
      args.WriteLine();
      args.WriteLine();
      args.WriteLine("************");
      if (!topLevel)
        args.WriteLine("Inner Exception");
      args.WritePair("ExceptionClass", e.GetType().ToString());
      args.WritePair("Message", e.Message);
      args.WriteLine("Properties: ");
      args.IndentLevel++;
      try
      {
        LogoutObject(args, e);
      }
      catch (Exception e2)
      {
        args.WriteLine("Ошибка при записи свойств: " + e2.Message);
      }
      args.IndentLevel--;
      args.WriteLine();
      args.WriteLine("Stack trace:");
      args.WriteLine(e.StackTrace);
    }

    #region Событие ExceptionLogFileCreated

    /// <summary>
    /// Событие вызывается после того, как создан log-файл с сообщением об ошибке.
    /// Это вызывается в том потоке, в котором был вызван метод LogoutException
    /// </summary>
    public static event ExceptionLogFileCreatedEventHandler ExceptionLogFileCreated;

    private static void OnExceptionLogFileCreated(AbsPath filePath)
    {
      ExceptionLogFileCreatedEventHandler ehExceptionLogFileCreated = ExceptionLogFileCreated; // 12.01.2021. Учитываем возможность асинхронного присоединения и отсоединения обработчиков событий
      if (ehExceptionLogFileCreated != null)
      {
        ExceptionLogFileCreatedEventArgs args = new ExceptionLogFileCreatedEventArgs(filePath);
        ehExceptionLogFileCreated(null, args);
      }
    }

    #endregion

    #endregion

    #region Получение информации GetDebugInfo()

    /// <summary>
    /// Получение отладочной информации в виде строки текста с настройками по умолчанию
    /// </summary>
    /// <returns>Строка текста</returns>
    public static string GetDebugInfo()
    {
      return GetDebugInfo(new LogoutDebugInfoSettings());
    }

    /// <summary>
    /// Получение отладочной информации в виде строки текста с заданными настройками
    /// </summary>
    /// <param name="settings">Настройки</param>
    /// <returns>Строка текста</returns>
    public static string GetDebugInfo(LogoutDebugInfoSettings settings)
    {
      string s;
      StringWriter sw = new StringWriter();
      GetDebugInfo(sw, settings);
      s = sw.ToString();
      return s;
    }

    /// <summary>
    /// Получение отладочной информации с заданными настройками с записью в заданный TraceListener
    /// </summary>
    /// <param name="listener">Объект для записи информации</param>
    /// <param name="settings">Настройки</param>
    public static void GetDebugInfo(TraceListener listener, LogoutDebugInfoSettings settings)
    {
      LogoutInfoNeededEventArgs args = new LogoutInfoNeededEventArgs(listener, null);
      OnLogoutInfoNeeded(args, settings);
    }

    /// <summary>
    /// Запись отладочной информации с заданными настройками в текстовый файл или в другой поток
    /// </summary>
    /// <param name="writer">Объект TextWriter или другой поток</param>
    /// <param name="settings">Настройки</param>
    public static void GetDebugInfo(TextWriter writer, LogoutDebugInfoSettings settings)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");

      using (TextWriterTraceListener listener = new TextWriterTraceListener(writer))
      {
        listener.IndentSize = 2;
        GetDebugInfo(listener, settings);
        listener.Flush();
      }
    }

    #endregion

    #region LogoutObject

    /// <summary>
    /// Вывод свойств объекта в log-файл
    /// Перед выводов свойств, вероятно, следует вывести заголовок, т.к. метод не выводит информации о типе объекта или его назначении
    /// Также, вероятно, должен быть вызов Args.IndentLevel++/--
    /// </summary>
    /// <param name="args">Аргументы обработчика события Logout</param>
    /// <param name="obj">Выводимый объект. Может быть null</param>
    public static void LogoutObject(LogoutInfoNeededEventArgs args, object obj)
    {
      try
      {
        Stack objStack = new Stack();
        DoLogoutObject(args, obj, objStack);
      }
      catch
      {
      }
    }

    /// <summary>
    /// Вывод свойств для одного объекта
    /// Рекурсивная функция: выводит свойства вложенных объектов, если Level не
    /// превышает заданный уровень
    /// </summary>
    //[DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
    private static void DoLogoutObject(LogoutInfoNeededEventArgs args, object obj, Stack objStack)
    {
      if (obj == null)
      {
        args.WriteLine("*** Null reference ***");
        return;
      }

      // 27.05.2015
      // Предотвращаем вывод объекта, если он уже был выведен, но есть обратные ссылки вида A->B->A

      // 23.05.2016
      // Проблема. Для поиска существующих элементов используется метод Stack.Contains().
      // Он, видимо, выполняет сравнение элементов, а не просто ссылок.
      // Реализуем поиск вручную

      // if (ObjStack.Contains(Object))
      if (StackContainsElement(objStack, obj))
      {
        if (Object.ReferenceEquals(objStack.Peek(), obj))
          args.WriteLine("Обратная ссылка равна текущему объекту [" + obj.GetType().ToString() + "]");
        else
          args.WriteLine("Обратная ссылка. Объект [" + obj.GetType().ToString() + "] уже был выведен ранее");
        return;
      }

      #region Запрос для объекта в-целом

      LogoutPropEventArgs args2 = new LogoutPropEventArgs(obj, objStack.Count);

      //if (ObjStack.Count > 0)
      //{
      if (objStack.Count > 4)
        args2.Mode = LogoutPropMode.None;
      else
        args2.Mode = LogoutPropMode.Default;

      OnLogoutProp(args2);
      //}
      //else
      //  Args2.Mode = LogoutPropMode.Default;

      if (objStack.Count == 0 && args2.Mode != LogoutPropMode.Default && args2.Handler == null)
      {
        args.WriteLine("Вывод объекта отменен");
        return;
      }

      if (args2.Mode == LogoutPropMode.None)
        return; // 23.10.2017

      #endregion

      objStack.Push(obj);
      try
      {
        if (args2.Mode == LogoutPropMode.Default)
          DoLogoutObject2(args, obj, objStack);

        if (args2.Handler != null)
          args2.Handler(args, obj); // после основного вывода для объекта
      }
      finally
      {
        objStack.Pop();
      }
    }

    private static bool StackContainsElement(Stack objStack, object obj)
    {
      switch (objStack.Count)
      {
        case 0:
          return false;
        case 1:
          return Object.ReferenceEquals(objStack.Peek(), obj);
        default:
          foreach (Object obj2 in objStack)
          {
            if (Object.ReferenceEquals(obj2, obj))
              return true;
          }
          return false;
      }
    }

    [DebuggerStepThrough]
    private static void DoLogoutObject2(LogoutInfoNeededEventArgs args, object obj, Stack objStack)
    {
      #region Вывод для строки

      if (obj is string)
      {
        args.WritePair("Length", obj.ToString().Length.ToString());
        args.WritePair("ToString()", obj.ToString());
        return;
      }

      #endregion

      #region Вывод списочных свойств

      Type typ = obj.GetType();
      if (typ.GetInterface(typeof(IDictionary).ToString()) != null)
      //if (Object is IDictionary)
      {
        DoLogoutDictionary(args, (IDictionary)obj, objStack);
        if (typ.IsArray)
          return; // остальные свойства не нужны
      }
      else if (typ.GetInterface(typeof(IEnumerable).ToString()) != null)
        //else if (Object is IEnumerable)
        DoLogoutEnumerable(args, (IEnumerable)obj, objStack);

      #endregion

      #region Вывод именных свойств

      LogoutPropEventArgs args2 = new LogoutPropEventArgs(obj, objStack.Count);

      PropertyInfo[] aProps = typ.GetProperties(BindingFlags.Instance | BindingFlags.Public);

      // 01.10.2016
      // Класс может иметь несколько одноименных свойств, как это не странно.
      // Это происходит, когда в производном классе есть свойство с модификатором "new".
      // Если оба свойства возвращают один и тот же объект, то второй раз выводить свойство явно не надо
      Dictionary<string, object> PropValDict = new Dictionary<string, object>();

      for (int i = 0; i < aProps.Length; i++)
      {
        try
        {

          args2.PropertyName = aProps[i].Name;
          args2.Mode = LogoutPropMode.Default;
          OnLogoutProp(args2);
          if (args2.Mode == LogoutPropMode.None)
            continue;

          bool isIndexed = false;
          ParameterInfo[] aPars = aProps[i].GetIndexParameters();
          if (aPars != null)
          {
            if (aPars.Length > 0)
              isIndexed = true; // индексированное свойство
          }
          if (isIndexed)
            //Args.WritePair(aProps[i].Name, "[ Индексированное свойство ]");
            continue; // не нужно
          else
          {
            object x;
            if (DoGetPropValue(aProps[i], obj, out x))
            {
              if (PropValDict.ContainsKey(aProps[i].Name))
              {
                if (Object.ReferenceEquals(PropValDict[aProps[i].Name], x))
                  continue;
              }
              PropValDict[aProps[i].Name] = x;

              args.WritePair(aProps[i].Name, GetObjString(x));
              if (x != null && args2.Mode == LogoutPropMode.Default)
              {
                args.IndentLevel++;
                try
                {
                  DoLogoutObject(args, x, objStack);
                }
                finally
                {
                  args.IndentLevel--;
                }
              }
            }
            else
            {
              Exception e = (Exception)x;
              if (e is TargetInvocationException && e.InnerException != null)
                e = e.InnerException; // 19.10.2018
              args.WritePair(aProps[i].Name, "*** Ошибка получения свойства. " + e.GetType().ToString() + " ***: " + e.Message);
            }
          }
        }
        catch (Exception e2)
        {
          args.WritePair(aProps[i].Name, " *** Ошибка получения свойства. " + e2.GetType().ToString() + " ***:" + e2.Message);
        }
      }

      #endregion

      #region Marshal-by-ref info

      if (obj is MarshalByRefObject)
      {
        int oldIndentLevel = args.IndentLevel;
        try
        {
          DoLogoutMarshalByRefObjectInfo(args, (MarshalByRefObject)obj);
        }
        catch (Exception e)
        {
          args.WriteLine("** Ошибка вывода информации MarshalByRefObject **. " + e.Message);
        }
        args.IndentLevel = oldIndentLevel;
      }

      #endregion
    }

    /// <summary>
    /// Вызывает метод PropertyInfo.GetValue() с перехватом исключения
    /// </summary>
    /// <param name="propInfo">Описатель свойства</param>
    /// <param name="obj">Объект, из которого извлекается свойство</param>
    /// <param name="x">Полученное значение или объект исключения</param>
    /// <returns>true-значение получено, false-возникло исключение</returns>
    [DebuggerStepThrough]
    private static bool DoGetPropValue(PropertyInfo propInfo, object obj, out object x)
    {
      try
      {
        x = propInfo.GetValue(obj, null);
        return true;
      }
      catch (Exception e)
      {
        x = e;
        return false;
      }
    }

    [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
    private static string GetObjString(object obj2)
    {
      if (obj2 == null)
        return "null";

      if (obj2 is Array)
      {
        // 16.09.2015
        // Для маccива в квадратных скобках выводим разщмерности.
        // То есть, вместо "System.String[]" выводим "System.String[10]"
        string s1 = obj2.ToString();
        int p = s1.IndexOf('[');
        if (p >= 0)
          s1 = s1.Substring(0, p);

        s1 += "[";
        Array a = (Array)obj2;
        for (int i = 0; i < a.Rank; i++)
        {
          if (i > 0)
            s1 += ", ";
          if (a.GetLowerBound(i) == 0)
            s1 += (a.GetUpperBound(i) + 1).ToString();
          else
            s1 += "{" + a.GetLowerBound(i).ToString() + " .. " + a.GetUpperBound(i).ToString() + "}";
        }
        s1 += "]";
        return s1;
      }

      try
      {
        // 10.06.2015
        // Если ToString() переопределен, то выводим в скобках тип объекта, иначе не всегда понятно,
        // к чему относится свойство, особенно для свойства Target в делегатах

        string s1 = obj2.ToString(); // основной текст
        if (obj2 is string)
        {
          return "\"" + s1 + "\"";
          // 12.04.2018 Отменено, т.к. неудобно отображается многострочный текст
          // return DataTools.StrToCSharpString(s1);
        }
        Type typ = obj2.GetType();
        string s2 = typ.ToString();
        if (s1 == s2)
          return s1;
        else if (typ.IsPrimitive /*|| (obj2 is String)*/)
          return s1;
        else if (typ.ToString().Contains(".KeyValuePair") || // может быть, есть способ лучше
          obj2 is DictionaryEntry)

          return s1;
        else if (obj2 is MemberInfo)
          return s1;
        else
          return s1 + " [" + s2 + "]";
      }
      catch (Exception e)
      {
        return "*** Ошибка " + e.GetType().ToString() + " ***. " + e.Message;
      }
    }

    #region Вывод списочных данных

    [DebuggerStepThrough]
    private static void DoLogoutEnumerable(LogoutInfoNeededEventArgs args, IEnumerable obj, Stack objStack)
    {
      if (obj is IdList)
        return;

      if (obj is Array)
      {
        Array a = (Array)obj;
        if (a.Rank == 1 && a.Length > 0)
        {
          object v = a.GetValue(0);
          if (v is Byte)
          {
            DoLogoutByteArray(args, (byte[])obj);
            return;
          }
        }
      }

      //Args.IndentLevel++;
      try
      {
        int cnt = 0;
        foreach (object obj2 in obj)
        {
          args.WritePair("[" + cnt.ToString() + "]", GetObjString(obj2));
          if (obj2 != null)
          {
            int IndentLevel = args.IndentLevel;
            args.IndentLevel++;
            try
            {
              DoLogoutObject(args, obj2, objStack);
            }
            catch (Exception e)
            {
              args.WriteLine("** Ошибка вывода элемента **. " + e.Message);
            }
            cnt++;
            args.IndentLevel = IndentLevel;
          }
        }
      }
      // ReSharper disable once RedundantEmptyFinallyBlock
      finally
      {
        //Args.IndentLevel--;
      }
    }

    private static void DoLogoutByteArray(LogoutInfoNeededEventArgs args, byte[] a)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i += 16)
      {
        sb.Length = 0;
        sb.Append(i.ToString("X8"));
        for (int j = 0; j < 16; j++)
        {
          int Index = i + j;
          if (Index >= a.Length)
            break;
          sb.Append(' ');
          sb.Append(a[Index].ToString("X2"));
        }
        args.WriteLine(sb.ToString());
      }
    }

    [DebuggerStepThrough]
    private static void DoLogoutDictionary(LogoutInfoNeededEventArgs args, IDictionary obj, Stack objStack)
    {
      //Args.IndentLevel++;
      try
      {
        foreach (DictionaryEntry pair in obj)
        {
          args.WritePair("[" + pair.Key.ToString() + "]", GetObjString(pair.Value));
          if (pair.Value != null)
          {
            int IndentLevel = args.IndentLevel;
            args.IndentLevel++;
            try
            {
              DoLogoutObject(args, pair.Value, objStack);
            }
            catch (Exception e)
            {
              args.WriteLine("** Ошибка вывода элемента **. " + e.Message);
            }
            args.IndentLevel = IndentLevel;
          }
        }
      }
      // ReSharper disable once RedundantEmptyFinallyBlock
      finally
      {
        //Args.IndentLevel--;
      }
    }

    #endregion

    #region MarshalByRefObject

    [DebuggerStepThrough]
    private static void DoLogoutMarshalByRefObjectInfo(LogoutInfoNeededEventArgs args, MarshalByRefObject obj)
    {
      string uri = RemotingServices.GetObjectUri(obj);
      if ((!RemotingServices.IsTransparentProxy(obj)) && uri == null /*01.06.2017*/)
        return;
      if (obj is System.Runtime.Remoting.Lifetime.ILease)
        return; // 01.06.2017

      args.WriteLine("Remote object info");
      args.IndentLevel++;
      int indentLevel = args.IndentLevel;


      args.WritePair("IsTransparentProxy()", RemotingServices.IsTransparentProxy(obj).ToString());
      args.WritePair("GetObjectUri()", uri);
      args.WritePair("IsObjectOutOfAppDomain()", RemotingServices.IsObjectOutOfAppDomain(obj).ToString());
      args.WritePair("IsObjectOutOfContext()", RemotingServices.IsObjectOutOfContext(obj).ToString());

      try
      {
        RealProxy proxy2 = RemotingServices.GetRealProxy(obj);
        if (proxy2 != null)
        {
          args.WritePair("GetRealProxy()", proxy2.ToString());
          args.IndentLevel++;
          LogoutTools.LogoutObject(args, proxy2);
        }
      }
      catch { }
      args.IndentLevel = indentLevel;

#if XXX // тоже вызывает ошибку (NullReferenceException)
      Args.WriteLine("GetUrlsForObject");
      Args.IndentLevel++;
      try
      {
        string[] a = System.Runtime.Remoting.Channels.ChannelServices.GetUrlsForObject(Object);
        LogoutObject(Args, a);
      }
      catch (Exception e)
      {
        Args.WriteLine("** " + e.Message + " **");
      }
      Args.IndentLevel = IndentLevel;
#endif
      args.WriteLine("GetChannelSinkProperties()");
      args.IndentLevel++;
      try
      {
        IDictionary dict = System.Runtime.Remoting.Channels.ChannelServices.GetChannelSinkProperties(obj);
        dict = HidePasssword(dict);
        LogoutObject(args, dict);
      }
      catch (Exception e)
      {
        args.WriteLine("** " + e.Message + " **");
      }
      args.IndentLevel = indentLevel;

#if XXX // Не работает для удаленного объекта. Всегда вызывает исключение
      if (!String.IsNullOrEmpty(Uri))
      {
        string txt;
        try
        {
          Type typ1 = RemotingServices.GetServerTypeForUri(Uri);
          if (typ1 == null)
            txt = null;
          else
            txt = typ1.ToString();
        }
        catch (Exception e)
        {
          txt = "** " + e.Message + " **";
        }
        Args.WritePair("GetServerTypeForUri()", txt);
      }
#endif

#if !XXX // Не работает для удаленного объекта
      object lts;
      try
      {
        //LTS = RemotingServices.GetLifetimeService(Object);
        // LTS = obj.GetLifetimeService();
        lts = System.Runtime.Remoting.RemotingServices.GetLifetimeService(obj);
        if (lts == null)
          args.WritePair("GetLifeTimeService()", "null");
        else
        {
          args.WritePair("GetLifeTimeService()", lts.GetType().ToString());
          args.IndentLevel++;
          LogoutTools.LogoutObject(args, lts);
          args.IndentLevel--;
        }
      }
      catch (Exception e)
      {
        args.WritePair("GetLifeTimeService()", "** " + e.Message + " **");
      }
#endif

      //Type typ = Object.GetType();
      Type typ = typeof(MarshalByRefObject);
      ObjRef objRef;
      try
      {
        objRef = obj.CreateObjRef(typ);
      }
      catch (Exception e)
      {
        args.WriteLine("** Ошибка вызова MarshalByRefObject.CreateObjRef(" + typ.ToString() + ") **. " + e.Message);
        return;
      }

      //if (Ref.IsFromThisAppDomain())
      //  return; // ничего не надо выводить

      args.WriteLine("MarshalByRefObject.ObjRef");
      args.IndentLevel++;
      //Args.WritePair("IsFromThisAppDomain()", Ref.IsFromThisAppDomain().ToString()); 
      //Args.WritePair("IsFromThisProcess()", Ref.IsFromThisProcess().ToString());
      Stack ObjStack = new Stack();
      DoLogoutObject(args, objRef, ObjStack);
      args.IndentLevel--;

      // Это не работает
      //Args.WriteLine("MarshalByRefObject.GetLifetimeService()");
      //DoLogoutObject(Args, Object.GetLifetimeService(), 0);

      args.IndentLevel--;
    }

    /// <summary>
    /// Прячет пароль, если в коллекции задан ключ "password"
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    internal static IDictionary HidePasssword(IDictionary dict)
    {
      if (dict == null)
        return null;
      if (!dict.Contains("password"))
        return dict;

      string value=DataTools.GetString(dict["password"]);
      if (value.Length == 0)
        return dict;

      Dictionary<object, object> dict2 = new Dictionary<object, object>(dict.Count);
      foreach (DictionaryEntry de in dict)
      {
        if (Object.Equals(de.Key, "password"))
          dict2.Add(de.Key, "*** Password is hidden for log ***");
        else
          dict2.Add(de.Key, de.Value);
      }
      return dict2;
    }

    #endregion

    #endregion

    #region Добавление информации по умолчанию

    [DebuggerStepThrough]
    private static void LogoutSysInfo(LogoutInfoNeededEventArgs args)
    {
      string s;
      int currIndentLevel = args.IndentLevel;

      #region Системная информация

      args.WriteHeader("Системная информация");
      args.WritePair("Computer Name", Environment.MachineName);
      args.WritePair("OS Version", EnvironmentTools.OSVersionText);
      args.WritePair(".NET Version", EnvironmentTools.NetVersionText);
      args.WritePair("Processor Count", Environment.ProcessorCount.ToString());
      args.WritePair("Command Line", Environment.CommandLine);
      args.WritePair("Current Dir", Environment.CurrentDirectory);
      try
      {
        try
        {
          args.WritePair("Domain name", Environment.UserDomainName + ", UserName: " + Environment.UserName);
        }
        catch (PlatformNotSupportedException) // 24.04.2015 - Environment.UserDomainName нет в Windows-98/Me
        {
          args.WritePair("UserName", Environment.UserName);
        }
        args.WritePair("UserInteractive", Environment.UserInteractive.ToString());
        args.WritePair("HasShutdownStarted", Environment.HasShutdownStarted.ToString());
      }
      catch { }
      args.WritePair("Current time", DateTime.Now.ToString());

      args.WritePair("App. path", FileTools.ApplicationPath.Path);
      if (!FileTools.ApplicationPath.IsEmpty)
      {
        args.IndentLevel++;
        try
        {
          if (System.IO.File.Exists(FileTools.ApplicationPath.Path))
          {
            FileInfo fi = new FileInfo(FileTools.ApplicationPath.Path);
            args.WriteLine("FileInfo");
            args.IndentLevel++;
            LogoutObject(args, fi);
            args.IndentLevel--;

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(FileTools.ApplicationPath.Path);
            args.WriteLine("FileVersionInfo");
            args.IndentLevel++;
            LogoutObject(args, fvi);
            args.IndentLevel--;
          }
          else
            args.WriteLine("*** Исполняемый файл не найден ***");
        }
        catch (Exception e)
        {
          args.WriteLine("*** Ошибка получения информации о выполняемом файле ***. " + e.Message);
        }
        args.IndentLevel=currIndentLevel;
      }

      args.WritePair("App. base dir", FileTools.ApplicationBaseDir.Path);
      args.WritePair("App. name", EnvironmentTools.ApplicationName);
      args.WritePair("Environment.NewLine", DataTools.StrToCSharpString(Environment.NewLine));
      try
      {
        args.WritePair("CurrentProcessSessionId", EnvironmentTools.CurrentProcessSessionId.ToString());
        args.WritePair("ActiveConsoleSessionId", EnvironmentTools.ActiveConsoleSessionId.ToString());
      }
      catch { }

      args.WriteLine();

      #endregion

      #region Устройства

      try
      {
        LogoutDriveInfo(args);
      }
      catch { }
      args.IndentLevel = currIndentLevel;

      args.WriteLine();

      #endregion

      #region Environment.StackTrace

      args.WriteLine("Environment.StackTrace");
      args.WriteLine(Environment.StackTrace); // 05.06.2017
      args.WriteLine();

      #endregion

      #region GC

      args.WritePair("GCSettings.IsServerGC", GCSettings.IsServerGC.ToString());
      try { LogoutLatencyMode(args); }
      catch { } // нет в .NET 2.0 без SP2
      args.WritePair("GC.MaxGeneration", GC.MaxGeneration.ToString());

      try
      {
        args.WriteLine("GC.CollectionCount");
        args.IndentLevel++;
        for (int i = 0; i <= GC.MaxGeneration; i++)
          args.WritePair("Generation " + i.ToString(), GC.CollectionCount(i).ToString());
        args.IndentLevel--;
      }
      catch { }

      try { args.WritePair("GC.GetTotalMemory", MBText(GC.GetTotalMemory(false))); } // вызов с true был бы очень затратным
      catch { }

      #endregion

      #region TempDirectory

      try
      {
        bool dirEx = false;
        string suffix;
        try
        {
          dirEx = Directory.Exists(TempDirectory.RootDir.Path);
          suffix = dirEx ? " (Exists)" : " (Doesn't exist)";
        }
        catch { suffix = " (Error)"; }
        args.WritePair("TempDirectory.RootDir", TempDirectory.RootDir.Path + suffix);
      }
      catch { }

      #endregion

      #region Remoting

      args.WriteLine("LifetimeServices");
      args.IndentLevel++;
      args.WritePair("LeaseManagerPollTime", System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime.ToString());
      args.WritePair("LeaseTime", System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseTime.ToString());
      args.WritePair("RenewOnCallTime", System.Runtime.Remoting.Lifetime.LifetimeServices.RenewOnCallTime.ToString());
      args.WritePair("SponsorshipTimeout", System.Runtime.Remoting.Lifetime.LifetimeServices.SponsorshipTimeout.ToString());
      args.IndentLevel--;

      #endregion

      #region Special folders

      args.WriteHeader("Special folders");
      Array a = Enum.GetValues(typeof(Environment.SpecialFolder));
      args.IndentLevel++;
      for (int i = 0; i < a.Length; i++)
      {

        Environment.SpecialFolder sf = (Environment.SpecialFolder)(a.GetValue(i));
        // В перечислении встречаются повторы
        if (i > 0)
        {
          Environment.SpecialFolder prevsf = (Environment.SpecialFolder)(a.GetValue(i - 1));
          if (sf == prevsf)
            continue;
        }
        try
        {
          s = Environment.GetFolderPath(sf);
        }
        catch (Exception e)
        {
          s = "*** " + e.Message + " ***";
        }
        args.WritePair(sf.ToString(), s);
      }
      args.IndentLevel--;
      args.WriteLine();

      #endregion

      #region Environment variables

      args.WriteHeader("Environment variables");
      args.IndentLevel++;
      foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
      {
        string sValue = de.Value.ToString();
        switch (de.Key.ToString().ToUpperInvariant())
        {
          case "PATH":
            // 12.05.2016
            // Переменная PATH содержит множество значений, разделенных точкой с запятой.
            // Их удобно вывести на отдельных строках
            string sCharSep = new string(System.IO.Path.PathSeparator, 1);
            sValue = sValue.Replace(sCharSep, sCharSep + Environment.NewLine);
            break;
        }
        args.WritePair(de.Key.ToString(), sValue);
      }
      args.IndentLevel--;

      args.WriteLine();

      #endregion

      #region TimeZone

      args.WriteLine("TimeZone.Current");
      args.IndentLevel++;
      try
      {
        args.WritePair("StandardName", TimeZone.CurrentTimeZone.StandardName);
        args.WritePair("DaylightName", TimeZone.CurrentTimeZone.DaylightName);
        DateTime dt1 = DateTime.Now;
        DateTime dt2 = dt1.ToUniversalTime();
        dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Unspecified);
        dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Unspecified);
        TimeSpan ts = dt1 - dt2;
        args.WritePair("Часовой пояс", "GMT " + (ts.Ticks < 0 ? "" : "+") + ts.TotalHours.ToString("0.#"));
      }
      catch (Exception e2)
      {
        args.WriteLine("Ошибка доступа к TimeZone. " + e2.Message);
      }
      args.IndentLevel--;
      args.WriteLine();

      #endregion

      #region MemoryTools

      args.WriteHeader("MemoryTools");
      try
      {
        if (MemoryTools.TotalPhysicalMemory == MemoryTools.UnknownMemorySize)
          s = "Unknown";
        else
          s = MBText(MemoryTools.TotalPhysicalMemory);
        args.WritePair("TotalPhysicalMemory", s);

        args.WritePair("AvailableMemoryState", MemoryTools.AvailableMemoryState.ToString());
        if (MemoryTools.MemoryLoad == MemoryTools.UnknownMemoryLoad)
          s = "Unknown";
        else
          s = MemoryTools.MemoryLoad + " %";
        args.WritePair("MemoryLoad", s);
      }
      catch { }

      #endregion

      #region Logout Info

      args.WriteHeader("LogoutTools");
      args.WritePair("LogBaseDirectory", LogoutTools.LogBaseDirectory.ToString());
      args.WritePair("LogEncoding", LogEncoding.EncodingName);
      args.IndentLevel++;
      args.WritePair("CodePage", LogEncoding.CodePage.ToString());
      args.WritePair("WebName", LogEncoding.WebName);
      args.WritePair("BodyName", LogEncoding.BodyName);
      try
      {
        args.WritePair("GetPreamble()", DataTools.BytesToHex(LogEncoding.GetPreamble(), true));
      }
      catch { }
      args.IndentLevel--;

      #endregion

      #region DNS

      try
      {
        args.WriteHeader("DNS");
        string name = System.Net.Dns.GetHostName();
        args.WritePair("Host name", name);
        System.Net.IPAddress[] addrs = System.Net.Dns.GetHostAddresses(name);
        args.WritePair("IP addresses", addrs.Length.ToString());
        args.IndentLevel++;
        for (int i = 0; i < addrs.Length; i++)
          args.WriteLine(addrs[i].ToString());
        args.IndentLevel--;
        string[] aliases = System.Net.Dns.GetHostEntry(name).Aliases;
        args.WritePair("Aliases", aliases.Length.ToString());
        args.IndentLevel++;
        for (int i = 0; i < aliases.Length; i++)
          args.WriteLine(aliases[i]);
        args.IndentLevel--;
#pragma warning disable 618 // Свойство System.Net.Sockets.Socket.SupportsIPv4 устарело в Net Framework 4.5
        args.WritePair("Supports IPv4", System.Net.Sockets.Socket.SupportsIPv4.ToString());
        args.WritePair("Supports IPv6", System.Net.Sockets.Socket.OSSupportsIPv6.ToString());
#pragma warning restore 618
      }
      catch (Exception e)
      {
        args.WriteLine("Ошибка получения информации о DNS. " + e.Message);
      }

      args.WriteLine();

      #endregion

      #region RemotingConfiguration

      try
      {
        args.WriteHeader("RemotingConfiguration");
        args.WritePair("ApplicationId", RemotingConfiguration.ApplicationId);
        args.WritePair("ApplicationName", RemotingConfiguration.ApplicationName);
        try
        {
          args.WritePair("CustomErrorsMode", RemotingConfiguration.CustomErrorsMode.ToString());
        }
        catch (Exception e)
        {
          args.WritePair("CustomErrorsMode", "*** Ошибка *** " + e.Message);
        }
        try
        {
          args.WritePair("ProcessId", RemotingConfiguration.ProcessId);
        }
        catch (Exception e)
        {
          args.WritePair("CustomErrorsMode", "*** Ошибка *** " + e.Message);
        }

        args.WriteLine("GetRegisteredActivatedClientTypes()");
        args.IndentLevel++;
        LogoutObject(args, RemotingConfiguration.GetRegisteredActivatedClientTypes());
        args.IndentLevel--;

        args.WriteLine("GetRegisteredActivatedServiceTypes()");
        args.IndentLevel++;
        LogoutObject(args, RemotingConfiguration.GetRegisteredActivatedServiceTypes());
        args.IndentLevel--;

        args.WriteLine("GetRegisteredWellKnownClientTypes()");
        args.IndentLevel++;
        LogoutObject(args, RemotingConfiguration.GetRegisteredWellKnownClientTypes());
        args.IndentLevel--;

        args.WriteLine("GetRegisteredWellKnownServiceTypes()");
        args.IndentLevel++;
        LogoutObject(args, RemotingConfiguration.GetRegisteredWellKnownServiceTypes());
        args.IndentLevel--;
      }
      catch (Exception e)
      {
        args.IndentLevel++;
        args.WriteLine("*** Ошибка получения информации из RemotingConfiguration *** " + e.Message);
        args.IndentLevel = currIndentLevel;
      }

      #endregion

      #region ChannelServices

      args.WriteLine("ChannelServices.RegisteredChannels");
      LogoutObject(args, System.Runtime.Remoting.Channels.ChannelServices.RegisteredChannels);
      args.WriteLine();

      #endregion

      #region Current Process

      try
      {
        Process prc = Process.GetCurrentProcess();
        if (prc != null)
        {
          args.WriteHeader("Process.GetCurrentProcess");
          LogoutProcess(args, prc);
        }
      }
      catch (Exception e2)
      {
        args.WriteLine("Нельзя получить CurrentProcess. " + e2.Message);
      }
      args.WriteLine();

      #endregion

      #region Current Thread

      try
      {
        args.WriteHeader("Thread.CurrentThread");
        LogoutObject(args, Thread.CurrentThread);
      }
      catch (Exception e2)
      {
        args.WriteLine("Ошибка получения свойств Thread.CurrentThread. " + e2.Message);
      }
      args.WriteLine();

      #endregion

      #region Cache

      if (Cache.IsActive)
        Cache.LogoutCache(args);
      else
        args.WritePair("Cache.IsActive", "false");

      args.WriteLine();

      #endregion

      #region Thread pool

      LogoutThreadPool(args);
      args.WriteLine();

      #endregion

      #region WTS

      LogoutWTS(args);

      #endregion

      #region DisposableObject

#if DEBUG
      args.WriteHeader("DisposableObject");
      DisposableObject.LogoutInfo(args);
#endif

      #endregion

      #region Trace and Debug

      // Trace.Listeners и Debug.Listeners - это один список, а не два
      // И остальные свойства классов синхронизированы

      args.WriteHeader("Trace and Debug");
      args.WritePair("AutoFlush", Trace.AutoFlush.ToString());
      args.WritePair("UseGlobalLock", Trace.UseGlobalLock.ToString());
      args.WritePair("Listeners", Trace.Listeners.Count.ToString());
      args.IndentLevel++;
      try
      {
        LogoutTools.LogoutObject(args, Trace.Listeners);
      }
      catch (Exception e)
      {
        args.WriteLine("*** " + e.Message + " ***");
      }
      args.IndentLevel = currIndentLevel;
      args.WriteLine();

      #endregion
    }

    /// <summary>
    /// Выделено в отдельный метод, т.к. GCSettings.LatencyMode есть не во всех версиях Net Framework
    /// </summary>
    /// <param name="args"></param>
    private static void LogoutLatencyMode(LogoutInfoNeededEventArgs args)
    {
      //Args.WritePair("GCSettings.LatencyMode", GCSettings.LatencyMode.ToString());
      // 28.04.2020
      // Так даже возникает ошибка компиляции на Net Framework 2.0.50727.42

      PropertyInfo pi = typeof(GCSettings).GetProperty("LatencyMode", BindingFlags.Public | BindingFlags.Static);
      if (pi == null)
        return;
      object x = pi.GetValue(null, DataTools.EmptyObjects);
      if (x != null)
        args.WritePair("GCSettings.LatencyMode", x.ToString());
    }

    internal static string MBText(long bytes)
    {
      long mb = bytes / FileTools.MByte;
      string s = mb.ToString("# ##0");
      return s.PadLeft(10) + " MB";
    }

    [DebuggerStepThrough]
    private static void LogoutDriveInfo(LogoutInfoNeededEventArgs args)
    {
      DriveInfo[] dis = DriveInfo.GetDrives();
      args.WriteLine("Дисковые устройства");
      args.IndentLevel++;
      for (int i = 0; i < dis.Length; i++)
      {
        int indentLevel = args.IndentLevel;
        try
        {
          args.WritePair(dis[i].Name, dis[i].DriveType.ToString() + (dis[i].IsReady ? "" : " (Not Ready)"));
          if (dis[i].IsReady)
          {
            args.IndentLevel++;
            args.WritePair("Label", dis[i].VolumeLabel);
            args.WritePair("DriveFormat", dis[i].DriveFormat.ToString());
            args.WritePair("TotalSize", MBText(dis[i].TotalSize));
            args.WritePair("TotalFreeSpace ", MBText(dis[i].TotalFreeSpace));
            args.WritePair("AvailableFreeSpace", MBText(dis[i].AvailableFreeSpace));
            args.IndentLevel--;
          }
        }
        catch (Exception e)
        {
          args.WriteLine("Ошибка при получении информации об устройстве. " + e.Message);
          args.IndentLevel = indentLevel;
        }
      }
      args.IndentLevel--;
    }

    [DebuggerStepThrough]
    private static void LogoutThreadPool(LogoutInfoNeededEventArgs args)
    {
      int indentLevel = args.IndentLevel;
      try
      {
        args.WriteLine("ThreadPool");
        args.IndentLevel++;
        int Min1, Min2, Max1, Max2, Avail1, Avail2;
        ThreadPool.GetMinThreads(out Min1, out Min2);
        ThreadPool.GetMaxThreads(out Max1, out Max2);
        ThreadPool.GetAvailableThreads(out Avail1, out Avail2);
        args.WritePair("Рабочие потоки", "Min=" + Min1.ToString() + ", Max=" + Max1.ToString() + ", Available=" + Avail1.ToString());
        args.WritePair("Потоки ввода-вывода", "Min=" + Min2.ToString() + ", Max=" + Max2.ToString() + ", Available=" + Avail2.ToString());
      }
      catch (Exception e)
      {
        args.WriteLine("Ошибка получения информации о ThreadPool. " + e.Message);
      }
      args.IndentLevel = indentLevel;
    }


    private static void LogoutWTS(LogoutInfoNeededEventArgs args)
    {
      int indentLevel = args.IndentLevel;
      try
      {
        if (Win32.WTSSession.IsSupported)
        {
          args.WriteHeader("Windows Terminal Session");
          args.WritePair("ActiveConsoleSessionId", EnvironmentTools.ActiveConsoleSessionId.ToString());
          if (EnvironmentTools.ActiveConsoleSessionId != EnvironmentTools.NoSessionId)
          {
            args.IndentLevel++;
            LogoutObject(args, new WTSSession());
            args.IndentLevel--;
          }
          if (EnvironmentTools.CurrentProcessSessionId != EnvironmentTools.NoSessionId &&
            EnvironmentTools.CurrentProcessSessionId != EnvironmentTools.ActiveConsoleSessionId)
          {
            args.WriteLine();
            args.WritePair("CurrentProcessSessionId", EnvironmentTools.CurrentProcessSessionId.ToString());
            args.IndentLevel++;
            LogoutObject(args, new WTSSession(EnvironmentTools.CurrentProcessSessionId));
            args.IndentLevel--;
          }
        }
      }
      catch (Exception e)
      {
        args.WriteLine("*** " + e.Message + " ***");
      }
      args.IndentLevel = indentLevel;
      args.WriteLine();
    }


    private static void LogoutAssemblies(LogoutInfoNeededEventArgs args)
    {
      args.WriteHeader("Current AppDomain");
      args.IndentLevel++;
      try
      {
#if XXX
        Args.WritePair("IsDefaultAppDomain", AppDomain.CurrentDomain.IsDefaultAppDomain().ToString());
        Args.WritePair("FiendlyName", AppDomain.CurrentDomain.FriendlyName);
        Args.WritePair("Id", AppDomain.CurrentDomain.Id.ToString());
        Args.WritePair("RelativeSearchPath", AppDomain.CurrentDomain.RelativeSearchPath);
        Args.WritePair("DynamicDirectory", AppDomain.CurrentDomain.DynamicDirectory);
        Args.WritePair("ShadowCopyFiles", AppDomain.CurrentDomain.ShadowCopyFiles.ToString());
#endif
        LogoutObject(args, AppDomain.CurrentDomain);
        args.WritePair("IsDefaultAppDomain()", AppDomain.CurrentDomain.IsDefaultAppDomain().ToString());
        args.WritePair("IsFinalizingForUnload()", AppDomain.CurrentDomain.IsFinalizingForUnload().ToString());

        if (AppDomain.CurrentDomain.SetupInformation != null)
        {
          if (!String.IsNullOrEmpty(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
          {
            AbsPath configPath = new AbsPath(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            if (File.Exists(configPath.Path))
            {
              args.WriteLine();
              args.WriteLine("Конфигурационный файл \"" + configPath.Path + "\"");
              try
              {
                string s = File.ReadAllText(configPath.Path);
                args.WriteLine(s);
              }
              catch
              {
                args.WriteLine("** Не удалось получить содержимое файла **");
              }
            }
            else
              args.WriteLine("Конфигурационный файл \"" + configPath.Path + "\" не найден");
          }
        }
      }
      catch (Exception e)
      {
        args.WriteLine("Ошибка получения сведений о текущем домене. " + e.Message);
      }
      args.IndentLevel--;

      args.WriteHeader("Contexts");
      args.WritePair("SynchronizationContext.Current",
        SynchronizationContext.Current == null ? "null" : SynchronizationContext.Current.ToString());
      if (SynchronizationContext.Current != null)
      {
        args.IndentLevel++;
        LogoutObject(args, SynchronizationContext.Current);
        args.IndentLevel--;
      }

      args.WriteLine("Context.DefaultContext");
      args.IndentLevel++;
      LogoutObject(args, System.Runtime.Remoting.Contexts.Context.DefaultContext);
      args.IndentLevel--;

      if (!Object.ReferenceEquals(System.Runtime.Remoting.Contexts.Context.DefaultContext, Thread.CurrentContext))
      {
        args.WriteLine("Thread.CurrentContext");
        args.IndentLevel++;
        LogoutObject(args, Thread.CurrentContext);
        args.IndentLevel--;
      }


      args.WriteLine();

      args.WriteHeader("Загруженные сборки");
      if (!IsAssemblyEntryPointAvailable)
        args.WriteLine("[ Информация Assembly.EntryPoint недоступна ]");
      Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
      int cnt = 0;
      args.WriteLine("Приватные");
      args.IndentLevel++;
      DoLogoutAssemblies1(args, ref cnt, asses, false);
      args.IndentLevel--;
      args.WriteLine("Из глобального кэша сборок");
      args.IndentLevel++;
      DoLogoutAssemblies1(args, ref cnt, asses, true);
      args.IndentLevel--;
    }

    /// <summary>
    /// Возвращает true, если разрешено обращаться к свойству Assembly.EntryPoint
    /// В mono под linux'ом возникает фатальная ошибка, и приложение завершается.
    /// </summary>
    private static bool IsAssemblyEntryPointAvailable
    {
      get
      {
        // Эта проблема неизвестно, когда точно существует
        // 26.07.2022
        // В Mono 6.12 точно уже починили

        if (Environment.OSVersion.Platform != PlatformID.Unix)
          return true;
        if (!EnvironmentTools.IsMono)
          return true;
        if (EnvironmentTools.MonoVersion < new Version(6,12))
          return false;
        return true;
      }
    }

    [DebuggerStepThrough]
    private static void DoLogoutAssemblies1(LogoutInfoNeededEventArgs args, ref int cnt, Assembly[] asses, bool isGAC)
    {
      for (int i = 0; i < asses.Length; i++)
      {
        if (asses[i].GlobalAssemblyCache != isGAC)
          continue;
        cnt++;

        bool debugMode = false;
        DebuggableAttribute attrDebug = (DebuggableAttribute)Attribute.GetCustomAttribute(asses[i], typeof(DebuggableAttribute));
        if (attrDebug != null)
        {
          if (attrDebug.IsJITTrackingEnabled)
            debugMode = true; // не очень хорошо, но ладно
        }

        bool isPIA = false;
        PrimaryInteropAssemblyAttribute attrPIA = (PrimaryInteropAssemblyAttribute)Attribute.GetCustomAttribute(asses[i], typeof(PrimaryInteropAssemblyAttribute));
        if (attrPIA != null)
        {
          isPIA = true; // Надо ли извлекать номер версии - не знаю
        }



        args.WriteLine(cnt.ToString() + ". " + asses[i].ToString());
        //" Version: " + asses[i].GetName().Version.ToString() + 

        args.IndentLevel++;
        args.WriteLine("Build: " + (debugMode ? " (Debug)" : " (Release)") + " (" + asses[i].GetName().ProcessorArchitecture.ToString() + ")" + (isPIA ? " [PrimaryInteropAssembly]" : String.Empty));
        args.WriteLine(asses[i].CodeBase);
        int indentLevel = args.IndentLevel;
        try
        {
          AbsPath filePath = new AbsPath(asses[i].CodeBase);
          if (File.Exists(filePath.Path))
          {
            args.IndentLevel++;
            FileInfo fi = new FileInfo(filePath.Path);
            args.WriteLine("LastWriteTime=" + fi.LastWriteTime.ToString() + ", Length=" + fi.Length.ToString());
          }
        }
        catch { }
        args.IndentLevel = indentLevel;

        if (asses[i].ReflectionOnly)
          args.WriteLine("Reflection only");
        if (IsAssemblyEntryPointAvailable)
        {
          if (asses[i].EntryPoint != null)
            args.WriteLine("Entry point: " + asses[i].EntryPoint.DeclaringType.Name + "." + asses[i].EntryPoint.Name); // TODO: Вывести правильно имя метода по синтаксису C# 
        }
        args.IndentLevel--;
      }
    }

    #endregion

    #region Дополнительные методы вывода информации, которые можно применять в обработчике Logout

    #region Вывод информации о процессе

    /// <summary>
    /// Вывод информации о процессе
    /// Не требуется вызывать для текущего процесса, т.к. эти сведения добавляются в log-файл автоматически
    /// </summary>
    /// <param name="args">Объект для вывода информации</param>
    /// <param name="prc">Процесс, о котором выводится информация</param>
    [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
    public static void LogoutProcess(LogoutInfoNeededEventArgs args, Process prc)
    {
      if (prc == null)
      {
        args.WriteLine("********* Процесс для вывода информации не указан *********");
        return;
      }

      try
      {
        try { args.WritePair("Process name", prc.ProcessName); }
        catch { }
        try { args.WritePair("ProcessId", prc.Id.ToString()); }
        catch { }
        try { args.WritePair("Priority class", prc.PriorityClass.ToString()); }
        catch { }

        //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Unix)
        {
          try
          {
            args.WritePair("Start time", prc.StartTime.ToString());
            TimeSpan span = DateTime.Now - prc.StartTime;
            args.WritePair("Work time", span.ToString());
          }
          catch { }

          try { args.WritePair("Working Set", MBText(prc.WorkingSet64) + ", peak: " + MBText(prc.WorkingSet64)); }
          catch { }

          //try { Res.WriteLine("                      min : " + (prc.MinWorkingSet.ToInt64() / FileTools.MByte).ToString() + " MB"); }
          //catch { }
          //try { Res.WriteLine("                      max : " + (prc.MinWorkingSet.ToInt64() / FileTools.MByte).ToString() + " MB"); }
          //catch { }

          try { args.WritePair("Non-paged system memory", MBText(prc.NonpagedSystemMemorySize64)); }
          catch { }

          try { args.WritePair("Paged system memory", MBText(prc.PagedSystemMemorySize64)); }
          catch { }

          try { args.WritePair("Paged memory", MBText(prc.PagedMemorySize64) + ", peak: " + MBText(prc.PeakPagedMemorySize64)); }
          catch { }

          try { args.WritePair("Virtual memory", MBText(prc.VirtualMemorySize64) + ", peak: " + MBText(prc.PeakVirtualMemorySize64)); }
          catch { }

          try { args.WritePair("Thread count", prc.Threads.Count.ToString()); }
          catch { }
          try { args.WritePair("SessionId", prc.SessionId.ToString()); }
          catch { }
        }
      }
      catch (Exception e)
      {
        args.WriteLine("********* Ошибка получения информации о процессе. " + e.Message);
      }
    }

    #endregion

    #region DataRow

    /// <summary>
    /// Выводит пары "Имя поля = Значение" для строки таблицы
    /// </summary>
    /// <param name="args">Аргументы события LogoutInfoNeeded</param>
    /// <param name="row">Строка DataRow, для которой выводится информация</param>
    public static void LogoutDataRow(LogoutInfoNeededEventArgs args, DataRow row)
    {
      if (row == null)
      {
        args.WriteLine("*** null reference ***");
        return;
      }

      if (row.RowState == DataRowState.Deleted)
      {
        args.WriteLine("*** deleted row ***");
        return;
      }

      for (int i = 0; i < row.Table.Columns.Count; i++)
      {
        string colName = row.Table.Columns[i].ColumnName;
        object value = row[i];
        string valueText = GetObjString(value);
        args.WritePair(colName, valueText);
      }
    }

    #endregion

    #region Реестр Windows

    /// <summary>
    /// Выводит в log-файл раздел реестра Windows и его подразделы.
    /// Также выводятся значения.
    /// Работает только в Windows. 
    /// Для 32-разрядных приложений в Windows 64bit, подмена узлов реестра на Wow6432Node не выполняется
    /// </summary>
    /// <param name="args">Сюда выводится отладочная информация</param>
    /// <param name="keyName">Путь к разделу реестра</param>
    /// <returns>Возвращает true, если раздел реестра существует и был прочитан</returns>
    public static bool LogoutRegistryKey(LogoutInfoNeededEventArgs args, string keyName)
    {
      return LogoutRegistryKey(args, keyName, RegistryTree2.GetDefaultView());
    }

    /// <summary>
    /// Выводит в log-файл раздел реестра Windows и его подразделы.
    /// Также выводятся значения.
    /// Работает только в Windows. 
    /// </summary>
    /// <param name="args">Сюда выводится отладочная информация</param>
    /// <param name="keyName">Путь к разделу реестра</param>
    /// <param name="view">Режим просмотра реестра</param>
    /// <returns>Возвращает true, если раздел реестра существует и был прочитан</returns>
    public static bool LogoutRegistryKey(LogoutInfoNeededEventArgs args, string keyName, RegistryView2 view)
    {
      if (!RegistryKey2.IsSupported)
      {
        args.WriteLine("Реестр не поддерживается операционной системой");
        return false;
      }

      int indentLevel = args.IndentLevel;
      bool res = false;
      try
      {
        int cnt = 0;
        using (RegistryTree2 tree = new RegistryTree2(true, view))
        {
          foreach (EnumRegistryEntry2 regEntry in tree.Enumerate(keyName, true))
          {
            cnt++;
            args.IndentLevel = indentLevel + regEntry.EnumKeyLevel;
            if (String.IsNullOrEmpty(regEntry.ValueName))
            {
              string s = regEntry.Key.Name;
              int p = s.LastIndexOf('\\');
              if (p >= 0)
                s = s.Substring(p + 1);
              args.WriteLine("[-] " + s);
              object defv = regEntry.Key.GetValue(String.Empty);
              if (defv != null)
              {
                args.IndentLevel++;
                args.WritePair("[Default]", GetRegistryValueStr(defv));
              }
            }
            else
            {
              args.IndentLevel++;
              args.WritePair(regEntry.ValueName, GetRegistryValueStr(regEntry.Key.GetValue(regEntry.ValueName)));
            }
          }
        }

        if (cnt == 0)
          args.WriteLine("Нет раздела реестра " + keyName);
        else
          res = true;
      }
      catch (Exception e)
      {
        args.WriteLine("*** Ошибка. " + e.Message + " ***");
      }
      args.IndentLevel = indentLevel;
      return res;
    }

    private static string GetRegistryValueStr(object defv) // 22.01.2019
    {
      if (defv == null)
        return String.Empty;
      string[] a = defv as string[];
      if (a != null)
      {
        if (a.Length == 0)
          return "string[0]";
        return String.Join(Environment.NewLine, a);
      }

      return defv.ToString();
    }

    #endregion

    #endregion

    #region Вывод информации для одного объекта

    /// <summary>
    /// Создает многострочный текст для вывода свойств одного объекта
    /// </summary>
    /// <param name="obj">Объект, информация о котором выводится</param>
    /// <returns>Текст с отладочной ифнормацией</returns>
    public static string LogoutObjectToString(object obj)
    {
      StringWriter sw = new StringWriter();
      LogoutObject(sw, obj);
      return sw.ToString();
    }

    /// <summary>
    /// Вывод информации о произвольном объекте в TextWriter.
    /// Заголовок не добавляется
    /// </summary>
    /// <param name="writer">Объект для записи информации</param>
    /// <param name="obj">Объект, информация о котором выводится</param>
    public static void LogoutObject(TextWriter writer, object obj)
    {
      TextWriterTraceListener listener = new TextWriterTraceListener(writer);
      LogoutInfoNeededEventArgs args = new LogoutInfoNeededEventArgs(listener, null);
      LogoutObject(args, obj);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Поиск исключения заданного класса в списке вложенных исключений.
    /// Если переданное исключение e имеет тип T или является классом-наследником,
    /// то оно возвращается, приведенное к заданному типу. Иначе просматривается
    /// цепочка исключений InnerException. Если обнаружен подходящий тип исключения,
    /// то возрашается это исключение.
    /// Если в цепочке исключений нет ни одного исключения подходящего класса, то
    /// возвращается null.
    /// Метод используется в catch-блоке для реализации специальной обработки
    /// исключения заданного вида, если нет уверенности, что исключение не "обернуто"
    /// другим исключением
    /// </summary>
    /// <typeparam name="T">Тип исключения, который должен быть найден</typeparam>
    /// <param name="e">Исходное исключение, пойманное в catch-блоке</param>
    /// <returns>Исключение требуемого типа из цепочки или null</returns>
    public static T GetException<T>(Exception e)
      where T : Exception
    {
      while (e != null)
      {
        if (e is T)
          return (T)e;

        e = e.InnerException;
      }
      return null;
    }

    // 06.01.2021
    // Метод GetStackTrace() убран.
    // Методы Thread.Suspend()/Resume() не зря помечены как плохие.
    // Конструктор StackTrace() вызывает внутренний метод StackTrace.GetStackFramesInternal(), который может не завершится.
    // Если это случится с главным потоком, то приложение "зависнет".

    // Можно, конечно, попробовать обойти:
    // Вызвать Thread.Suspend() в основном потоке
    // Создать отдельный поток и из него вызвать конструктор StackTrace()
    // Основной поток ждет отдельный поток втечение, скажем 100мс.
    // Дождался или не дождался - вызываем Thread.Resume() из основного потока.
    // Что при этом будет с отдельным потоком - не знаю. Скорее всего - все сдохнет.

#if XXX
    /// <summary>
    /// Получение стека вызовов для потока.
    /// В случае ошибки возвращает пустую строку
    /// </summary>
    /// <param name="thread">Поток</param>
    /// <returns>Стек вызовов в формате Environment.StackTrace</returns>
    public static string GetStackTrace(Thread thread)
    {
      try
      {
        return DoGetStackTrace(thread);
      }
      catch
      {
        return String.Empty;
      }
    }

#pragma warning disable 0618 // Подавление сообщения, что методы Thread.Suspend() и Resume() устарели
    private static string DoGetStackTrace(Thread thread)
    {
      if (thread == Thread.CurrentThread)
        return Environment.StackTrace;

      string res;

      thread.Suspend();
      try
      {
        StackTrace st = new StackTrace(thread, true);
        res = st.ToString();
      }
      finally
      {
        thread.Resume();
      }
      return res;
    }
#pragma warning restore 0618
#endif

    #endregion

    #region Неприличные звуки

    /// <summary>
    /// Выдать звуковой сигнал в случае полного выхода программы из строя
    /// </summary>
    public static void BloodyHellSound()
    {
      try
      {
        //System.Media.SystemSounds.Exclamation.Play();
        Console.Beep(400, 400);
        Console.Beep(200, 400);
        Console.Beep(400, 400);
        Console.Beep(200, 400);
      }
      catch
      {
      }
    }

    #endregion
  }

  /// <summary>
  /// Настройка информации, которую требуется вывести методу LogoutTools.GetDebugInfo
  /// </summary>
  public class LogoutDebugInfoSettings
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует набор настроек значениями по умолчанию
    /// </summary>
    public LogoutDebugInfoSettings()
    {
      AddSystemInfo = true;
      UseHandlers = true;
      AddAssembliesInfo = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Нужно ли вывести общую инфорацию о системе (кроме загруженных сборок).
    /// По умолчанию - true
    /// </summary>
    public bool AddSystemInfo { get { return _AddSystemInfo; } set { _AddSystemInfo = value; } }
    private bool _AddSystemInfo;

    /// <summary>
    /// Нужно ли использовать обработчики события LogoutTools.LogoutInfoNeeded.
    /// По умолчанию - true
    /// </summary>
    public bool UseHandlers { get { return _UseHandlers; } set { _UseHandlers = value; } }
    private bool _UseHandlers;

    /// <summary>
    /// Дополнительный обработчик, который будет вызван после стандартных.
    /// По умолчанию - null
    /// </summary>
    public LogoutInfoNeededEventHandler AuxHandler { get { return _AuxHandler; } set { _AuxHandler = value; } }
    private LogoutInfoNeededEventHandler _AuxHandler;

    /// <summary>
    /// Нужно ли вывести инфорацию о загруженных сборках.
    /// По умолчанию - true
    /// </summary>                                                
    public bool AddAssembliesInfo { get { return _AddAssembliesInfo; } set { _AddAssembliesInfo = value; } }
    private bool _AddAssembliesInfo;

    #endregion

    #region Дополнительные методы и свойства

    /// <summary>
    /// Отменяет вывод всей информации
    /// </summary>
    public void Clear()
    {
      AddSystemInfo = false;
      UseHandlers = false;
      AuxHandler = null;
      AddAssembliesInfo = false;
    }

    /// <summary>
    /// Возвращает true, если отключен вывод всей информации (после вызова Clear())
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return (!AddSystemInfo) && (!UseHandlers) && (AuxHandler == null) && (!AddAssembliesInfo);
      }
    }

    #endregion
  }

  /// <summary>
  /// Вывод дополнительной отладочной информации, актуальной только в текущем контексте.
  /// Содержит коллекцию именованных объектов.
  /// Использование:
  /// 1. Создать LocalLogoutObjects, используя конструкцию using
  /// 2. Добавить объекты в коллекцию, которые нужно показать в log-файле
  /// 3. Выполнить код, который вызывает LogoutTools.GetDebugInfo() или LogoutException
  /// </summary>
  public sealed class LocalLogoutObjects : Dictionary<string, object>, IDisposable
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает объект
    /// </summary>
    public LocalLogoutObjects()
    {
      EHLogoutInfoNeeded = new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
      LogoutTools.LogoutInfoNeeded += EHLogoutInfoNeeded;
    }

    /// <summary>
    /// Реализация интерфейса IDisposable
    /// </summary>
    public void Dispose()
    {
      // Можно упростить и не делать стандартную обработку IDisposable
      LogoutTools.LogoutInfoNeeded -= EHLogoutInfoNeeded;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если установлено, то выводится заголовок перед объектами
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    #endregion

    #region Обработчик LogoutInfoNeeded

    private readonly LogoutInfoNeededEventHandler EHLogoutInfoNeeded;

    //[DebuggerStepThrough]
    void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
      args.WriteHeader(Title);
      int indentLevel = args.IndentLevel;
      foreach (KeyValuePair<string, object> Pair in this)
      {
        string typeStr; // 13.03.2017 - добавляем тип объекта
        try
        {
          if (Pair.Value == null)
            typeStr = "null";
          else
            typeStr = "[" + Pair.Value.GetType().ToString() + "]";
        }
        catch (Exception e)
        {
          typeStr = e.Message;
        }

        args.WritePair(Pair.Key, typeStr);
        args.IndentLevel++;
        try
        {
          LogoutTools.LogoutObject(args, Pair.Value);
        }
        catch (Exception e)
        {
          args.WriteLine("** Ошибка ** " + e.Message);
        }
        args.IndentLevel = indentLevel;
        args.WriteLine();
      }
    }

    #endregion
  }

  /// <summary>
  /// Получение отладочной информации (сервера) с помощью выполняемой процедуры.
  /// Выполняет вызов метода LogoutTools.GetDebugInfo()
  /// Возвращает аргумент с именем "Text"
  /// Предотвращает одновременное получение информации двумя пользователями во избежание переполнения
  /// </summary>
  public class LogoutExecProc : FreeLibSet.Remoting.ExecProc
  {
    // TODO: Когда будет готово, вместо выдачи исключения ожидать освобождения блокировки

    private static bool _ExecutingFlag = false;
    private static readonly object _SyncRoot = new object();

    #region Выполнение

    /// <summary>
    /// Вызывает LogoutTools.GetDebugInfo()
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected override FreeLibSet.Remoting.NamedValues OnExecute(FreeLibSet.Remoting.NamedValues args)
    {
      FreeLibSet.Remoting.NamedValues res = new FreeLibSet.Remoting.NamedValues();

      lock (_SyncRoot)
      {
        if (_ExecutingFlag)
          throw new BusyException("Получение отладочной информации в данный момент выполняется другим пользователем");

        _ExecutingFlag = true;
      }
      try
      {
        BeginSplash("Получение отладочной информации");
        res["Text"] = LogoutTools.GetDebugInfo();
        EndSplash();
      }
      finally
      {
        lock (_SyncRoot)
        {
          _ExecutingFlag = false;
        }
      }

      return res;
    }

    #endregion
  }

  /// <summary>
  /// Извлечение информации из текстового log-файла отчета, созданного методом LogoutTools.LogoutException()
  /// </summary>
  [Serializable]
  public sealed class ExceptionLogFileExtractedInfo
  {
    #region Свойства

    /// <summary>
    /// Заголовок (первая строка файла), задаваемая аргументом Title метода LogoutTools.LogoutException()
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Класс исключения в виде текстовой строки. Например, "System.ArgumentException".
    /// </summary>
    /// <remarks>
    /// Нельзя использовать свойство типа Type, т.к. в том коде, который анализирует log-файл, может
    /// не быть доступа к исходной сборке, содержащей класс исключения
    /// </remarks>
    public string ExceptionClass { get { return _ExceptionClass; } set { _ExceptionClass = value; } }
    private string _ExceptionClass;

    /// <summary>
    /// Текст исключения (свойство Exception.Message)
    /// </summary>
    public string Message { get { return _Message; } set { _Message = value; } }
    private string _Message;

    #endregion

    #region Извлечение информации из файла

    /// <summary>
    /// Извлекает информацию из файла, загруженного в память.
    /// Если файл имеет неподходящий формат, возвращается null.
    /// При возникновении других ошибок генерируется исключение
    /// </summary>
    /// <param name="file">Контейнер с файлом. Не может быть null.</param>
    /// <returns>Извлеченная информация или null</returns>
    public static ExceptionLogFileExtractedInfo Extract(FileContainer file)
    {
      if (file == null)
        throw new ArgumentNullException("file");
      using (MemoryStream Stream = new MemoryStream(file.Contents))
      {
        return Extract(Stream);
      }
    }

    /// <summary>
    /// Извлекает информацию из файла на диске.
    /// Если файл имеет неподходящий формат, возвращается null.
    /// При возникновении других ошибок генерируется исключение
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Извлеченная информация или null</returns>
    public static ExceptionLogFileExtractedInfo Extract(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath", "Путь к файлу не задан");

      using (FileStream stream = new FileStream(filePath.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        return Extract(stream);
      }
    }

    /// <summary>
    /// Извлекает информацию из файлового потока
    /// Если файл имеет неподходящий формат, возвращается null.
    /// При возникновении других ошибок генерируется исключение
    /// </summary>
    /// <param name="stream">Открытый поток. Не может быть null.</param>
    /// <returns>Извлеченная информация или null</returns>
    public static ExceptionLogFileExtractedInfo Extract(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      ExceptionLogFileExtractedInfo info;
      using (StreamReader reader = new StreamReader(stream, LogoutTools.LogEncoding))
      {
        info = DoExtract(reader);
      }
      return info;
    }

    private static ExceptionLogFileExtractedInfo DoExtract(StreamReader reader)
    {
      ExceptionLogFileExtractedInfo info = new ExceptionLogFileExtractedInfo();

      int cnt = 0;
      while (reader.Peek() >= 0 && cnt < 10)
      {
        cnt++;
        string s = reader.ReadLine();

        // Заголовок в первой строке
        if (cnt == 1)
        {
          info.Title = s;
          continue;
        }

        // Пустые строки
        if (String.IsNullOrEmpty(s) || s[0] == '-' || s[0] == '*')
          continue;

        if (s.StartsWith("ExceptionClass"))
        {
          int p = s.IndexOf('=');
          if (p < 0)
            return null;
          info.ExceptionClass = s.Substring(p + 1).Trim();
          continue;
        }

        if (s.StartsWith("Message"))
        {
          int p = s.IndexOf('=');
          if (p < 0)
            return null;
          info.Message = s.Substring(p + 1).Trim();
          return info; // Это - последнее поле
        }
      }

      // Не нашли часть строк - неправильный файл
      return null;
    }

    #endregion
  }
}
