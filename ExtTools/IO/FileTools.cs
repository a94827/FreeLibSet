﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
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
  #region Перечисление TestPathMode

  /// <summary>
  /// Режимы проверки для методов FileTools.TestFilePath() и TestDirSlashedPath()
  /// </summary>
  public enum TestPathMode
  {
    /// <summary>
    /// Тестирование не выполняется.
    /// Этот режим используется в управляющих элементах, если они должны допускать ввод произвольного текста.
    /// Методы класса FileTools ничего не делают в этом режиме.
    /// </summary>
    None,

    /// <summary>
    /// Проверяется только структура пути, но не наличие реальных каталогов.
    /// Обращения к файловой системе не происходит, поэтому проверка выполняется быстро.
    /// В частности, не проверяется наличие диска с заданной буквой и сетевой путь.
    /// </summary>
    FormatOnly,

    /// <summary>
    /// Проверяется существование корневого каталога. Будет выдана ошибка, если задана буква несуществующего диска
    /// или недействительный сетевой путь. При этом проверяется наличие диска в дисководе.
    /// Этот режим используется, если программа создаст недостающие каталоги, например, с помощью FileTools.ForceDirs()
    /// </summary>
    RootExists,

    /// <summary>
    /// Проверяется существование каталога.
    /// Этот режим используется, если программа собирается выполнить чтение файлов из каталога.
    /// Также используется, если планируется запись файла, но каталог уже должен быть.
    /// </summary>
    DirectoryExists,

    /// <summary>
    /// Проверяется существование файла.
    /// Обычно используется, если планируется выполнить чтение файла.
    /// Этот режим можно использовать только с FileTools.TestFileExists().
    /// </summary>
    FileExists
  }

  #endregion

  /// <summary>
  /// Статические методы для работы с файловой системой
  /// </summary>
  public static class FileTools
  {
    #region Константы

    /// <summary>
    /// Количество байт в одном килобайте
    /// </summary>
    public const long KByte = 1024L;

    /// <summary>
    /// Количество байт в одном мегабайте
    /// </summary>
    public const long MByte = 1024L * 1024L;

    /// <summary>
    /// Количество байт в одном гигабайте
    /// </summary>
    public const long GByte = 1024L * 1024L * 1024L;

    /// <summary>
    /// Количество байт в одном терабайте
    /// </summary>
    public const long TByte = 1024L * 1024L * 1024L * 1024L;

    /// <summary>
    /// Количество байт в одном петабайте
    /// </summary>
    public const long PByte = 1024L * 1024L * 1024L * 1024L * 1024L;

    /// <summary>
    /// Возвращает текстовое представление для размера (файла) в мегабайтах в виде "X XXX MB"
    /// </summary>
    /// <param name="length">Размер в байтах</param>
    /// <returns>Текст</returns>
    public static string GetMBSizeText(long length)
    {
      return ((double)length / (double)MByte).ToString("#,##0") + "MB";
    }

    #endregion

    // Аргументы имен файлов и каталогов должны иметь тип AbsPath, а не String для большинтва методов

    /// <summary>
    /// Функции и структуры, специфические для платформы Windows
    /// </summary>
    private static class WindowsNative
    {
      internal static bool IsWindowsPlatform
      {
        get
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Win32NT:
            case PlatformID.Win32Windows:
            case PlatformID.Win32S:
              return true;
            default:
              return false;
          }
        }
      }

      [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
      internal static extern int GetLongPathName(
              [MarshalAs(UnmanagedType.LPTStr)]
                        string path,
              [MarshalAs(UnmanagedType.LPTStr)]
                        StringBuilder longPath,
              int longPathLength
      );

      [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
      internal static extern int GetShortPathName(
              [MarshalAs(UnmanagedType.LPTStr)]
                        string path,
              [MarshalAs(UnmanagedType.LPTStr)]
                        StringBuilder shortPath,
              int shortPathLength
      );
      [DllImport("netapi32.dll", EntryPoint = "NetServerEnum")]
      internal static extern NERR NetServerEnum([MarshalAs(UnmanagedType.LPWStr)] string ServerName, int Level, out IntPtr BufPtr, int PrefMaxLen, ref int EntriesRead, ref int TotalEntries, SV_101_TYPES ServerType, [MarshalAs(UnmanagedType.LPWStr)] string Domain, int ResumeHandle);

      [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
      internal static extern NERR NetApiBufferFree(IntPtr Buffer);

      [Flags]
      internal enum SV_101_TYPES : uint
      {
        SV_TYPE_WORKSTATION = 0x00000001,
        SV_TYPE_SERVER = 0x00000002,
        SV_TYPE_SQLSERVER = 0x00000004,
        SV_TYPE_DOMAIN_CTRL = 0x00000008,
        SV_TYPE_DOMAIN_BAKCTRL = 0x00000010,
        SV_TYPE_TIME_SOURCE = 0x00000020,
        SV_TYPE_AFP = 0x00000040,
        SV_TYPE_NOVELL = 0x00000080,
        SV_TYPE_DOMAIN_MEMBER = 0x00000100,
        SV_TYPE_PRINTQ_SERVER = 0x00000200,
        SV_TYPE_DIALIN_SERVER = 0x00000400,
        SV_TYPE_XENIX_SERVER = 0x00000800,
        SV_TYPE_SERVER_UNIX = SV_TYPE_XENIX_SERVER,
        SV_TYPE_NT = 0x00001000,
        SV_TYPE_WFW = 0x00002000,
        SV_TYPE_SERVER_MFPN = 0x00004000,
        SV_TYPE_SERVER_NT = 0x00008000,
        SV_TYPE_POTENTIAL_BROWSER = 0x00010000,
        SV_TYPE_BACKUP_BROWSER = 0x00020000,
        SV_TYPE_MASTER_BROWSER = 0x00040000,
        SV_TYPE_DOMAIN_MASTER = 0x00080000,
        SV_TYPE_SERVER_OSF = 0x00100000,
        SV_TYPE_SERVER_VMS = 0x00200000,
        SV_TYPE_WINDOWS = 0x00400000,
        SV_TYPE_DFS = 0x00800000,
        SV_TYPE_CLUSTER_NT = 0x01000000,
        SV_TYPE_TERMINALSERVER = 0x02000000,
        SV_TYPE_CLUSTER_VS_NT = 0x04000000,
        SV_TYPE_DCE = 0x10000000,
        SV_TYPE_ALTERNATE_XPORT = 0x20000000,
        SV_TYPE_LOCAL_LIST_ONLY = 0x40000000,
        SV_TYPE_DOMAIN_ENUM = 0x80000000,
        SV_TYPE_ALL = 0xFFFFFFFF,
      }

      [StructLayout(LayoutKind.Sequential)]
      internal struct SERVER_INFO_101
      {
        [MarshalAs(UnmanagedType.U4)]
        public uint sv101_platform_id;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string sv101_name;
        [MarshalAs(UnmanagedType.U4)]
        public uint sv101_version_major;
        [MarshalAs(UnmanagedType.U4)]
        public uint sv101_version_minor;
        [MarshalAs(UnmanagedType.U4)]
        public uint sv101_type;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string sv101_comment;
      }


#if XXX
    internal enum PLATFORM_ID : uint
    {
      PLATFORM_ID_DOS = 300,
      PLATFORM_ID_OS2 = 400,
      PLATFORM_ID_NT = 500,
      PLATFORM_ID_OSF = 600,
      PLATFORM_ID_VMS = 700,
    }
#endif


      internal enum NERR
      {
        NERR_Success = 0,
        ERROR_ACCESS_DENIED = 5,
        ERROR_NOT_ENOUGH_MEMORY = 8,
        ERROR_BAD_NETPATH = 53,
        ERROR_NETWORK_BUSY = 54,
        ERROR_INVALID_PARAMETER = 87,
        ERROR_INVALID_LEVEL = 124,
        ERROR_MORE_DATA = 234,
        ERROR_EXTENDED_ERROR = 1208,
        ERROR_NO_NETWORK = 1222,
        ERROR_INVALID_HANDLE_STATE = 1609,
        ERROR_NO_BROWSER_SERVERS_FOUND = 6118,
      }
    }

    #region Имена файлов

    /// <summary>
    /// Возвращает true, если при сравнении путей учитывается регистр (Unix), или нет (Windows)
    /// </summary>
    public static bool CaseSensitive
    {
      // Реализация находится там, т.к. файл AbsPath.cs может использоваться отдельно, без FileTools
      get { return AbsPath.ComparisonType == StringComparison.Ordinal; }
    }

    /// <summary>
    /// Получить последовательное имя файла для заданного базового имени и
    /// индекса файла. Например, если <paramref name="fileName"/>="f0001.bmp", а <paramref name="fileIndex"/>,
    /// то будет возвращено "f0004.bmp"
    /// Для <paramref name="fileIndex"/>=0 всегда возвращается исходное имя файла <paramref name="fileName"/>.
    /// Учитывается расширение файла, но не делается предположений о наличии или отсутствии каталога.
    /// Метод не выполняет обращений к файловой системе.
    /// </summary>
    /// <param name="fileName">Исходное имя файла</param>
    /// <param name="fileIndex">Номер файла</param>
    /// <returns>Имя файла с заданным индексом</returns>
    public static string GetSerialFileName(string fileName, int fileIndex)
    {
      string s1, s2;
      int p = fileName.LastIndexOf('.');
      if (p >= 0)
      {
        s1 = fileName.Substring(0, p); // без точки
        s2 = fileName.Substring(p); // начиная с точки
      }
      else
      {
        s1 = fileName;
        s2 = String.Empty;
      }

      // Сколько цифр есть в конце имени
      int nDigs = 0;
      for (int i = s1.Length - 1; i >= 0; i--)
      {
        if (s1[i] >= '0' && s1[i] <= '9')
          nDigs++;
        else
          break;
      }

      string s3 = s1.Substring(s1.Length - nDigs, nDigs);
      int StartNom;
      if (int.TryParse(s3, out StartNom))
      {
        StartNom += fileIndex;
        s1 = s1.Substring(0, s1.Length - nDigs);
        if (StartNom.ToString().Length > nDigs)
          return s1 + StartNom.ToString() + s2;
        else
          return s1 + StartNom.ToString(new string('0', nDigs)) + s2;
      }
      else
      {
        if (fileIndex > 0)
          return s1 + (fileIndex + 1).ToString() + s2;
        else
          return s1 + s2;
      }
    }

    /// <summary>
    /// Возвращает массив последовательных имен файлов. См. метод GetSerialFileName().
    /// Количество элементов в массиве будет равно <paramref name="fileCount"/>.
    /// Учитывается расширение файла, но не делается предположений о наличии или отсутствии каталога.
    /// Метод не выполняет обращений к файловой системе.
    /// </summary>
    /// <param name="fileName">Исходное имя файла</param>
    /// <param name="fileCount">Количество файлов</param>
    /// <returns>Массив имен</returns>
    public static string[] GetSerialFileNames(string fileName, int fileCount)
    {
      string[] a = new string[fileCount];
      for (int i = 0; i < fileCount; i++)
        a[i] = GetSerialFileName(fileName, i);
      return a;
    }

    #endregion

    #region Маски ? и * (Wildcards)

    /// <summary>
    /// Проверяет относительное имя файла на соответствие шаблону.
    /// Имя файла и шаблон может содержать символ разделителя каталога. Для успешной проверки число разделителей в шаблоне и в имени файла должно совпадать
    /// (одинаковый уровень вложения). Каждая часть пути проверяется отдельно с помощью TestFileNameWildcards
    /// в расширении
    /// </summary>
    /// <param name="fileName">Проверяемое имя файла</param>
    /// <param name="template">Шаблон</param>
    /// <returns>true, если имя файла соответствует шаблону</returns>
    public static bool TestRelFileNameWildcards(string fileName, string template)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      if (String.IsNullOrEmpty(template))
        throw new ArgumentNullException("template");
#endif

      if (fileName.IndexOf(Path.DirectorySeparatorChar) < 0 && template.IndexOf(Path.DirectorySeparatorChar) < 0)
        // Нет разделителей каталога - простая проверка
        return TestFileNameWildcards(fileName, template);
      else
      {
        // Есть подкаталоги
        string[] aFileNames = fileName.Split(Path.DirectorySeparatorChar);
        string[] aTemplates = template.Split(Path.DirectorySeparatorChar);
        if (aFileNames.Length != aTemplates.Length)
          return false; // Не одинаковый уровень вложения
        for (int i = 0; i < aFileNames.Length; i++)
        {
          if (!TestFileNameWildcards(aFileNames[i], aTemplates[i]))
            return false;
        }
        return true;
      }


    }

    /// <summary>
    /// Проверяет имя файла на соответствие шаблону. Имя файла и шаблон не могут содержать символ разделителя каталога. 
    /// Шаблон может содержать символы "?" и "*". Звездочка "*" может присутствовать не более, чем по одному разу в имени и расширении
    /// в расширении
    /// </summary>
    /// <param name="fileName">Проверяемое имя файла</param>
    /// <param name="template">Шаблон</param>
    /// <returns>true, если имя файла соответствует шаблону</returns>
    public static bool TestFileNameWildcards(string fileName, string template)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      if (String.IsNullOrEmpty(template))
        throw new ArgumentNullException("template");
#endif

      if (fileName.IndexOfAny(new char[] { '*', '?' }) >= 0)
        throw new ArgumentException("Имя файла не может содержать шаблонные символы \"*\" и \"?\"", "fileName");


      int p;
      string FileName1, FileName2;
      p = fileName.LastIndexOf('.');
      if (p < 0)
      {
        FileName1 = fileName;
        FileName2 = String.Empty;
      }
      else
      {
        FileName1 = fileName.Substring(0, p);
        FileName2 = fileName.Substring(p + 1);
      }

      string Template1, Template2;
      p = template.LastIndexOf('.');
      if (p < 0)
      {
        Template1 = template;
        Template2 = "*";
      }
      else
      {
        Template1 = template.Substring(0, p);
        Template2 = template.Substring(p + 1);
      }

      return TestFileNameWildcardsPart(FileName1, Template1) && TestFileNameWildcardsPart(FileName2, Template2);
    }

    private static bool TestFileNameWildcardsPart(string fileName, string template)
    {
      if (fileName.Length == 0 && template.Length == 0)
        return true;

      int p = template.IndexOf('*');
      if (p >= 0)
      {
        // Есть звездочка
        if (p > 0)
        {
          // Есть часть шаблона до звездочки
          if (fileName.Length < p)
            return false;
          if (!TestFileNameWildcardsPart2(fileName.Substring(0, p), template.Substring(0, p)))
            return false;

          // Отрезаем проверенную часть
          fileName = fileName.Substring(p);
        }

        int nRight = template.Length - p - 1; // сколько символов в шаблоне справа от звездочки
        if (nRight > 0)
        {
          if (fileName.Length < nRight)
            return false;
          if (!TestFileNameWildcardsPart2(fileName.Substring(fileName.Length - nRight), template.Substring(template.Length - nRight)))
            return false;
        }
        return true;
      }
      else
      {
        if (fileName.Length < template.Length)
          return false;
        if (!TestFileNameWildcardsPart2(fileName, template))
          return false;
        return true;
      }
    }

    // Проверка части имени файла на шаблон "?"
    private static bool TestFileNameWildcardsPart2(string fileName, string template)
    {
      if (fileName.Length != template.Length)
        return false;

      if (template.IndexOf('?') >= 0)
      {
        StringBuilder sb = new StringBuilder(fileName);
        for (int i = 0; i < template.Length; i++)
        {
          if (template[i] == '?')
            sb[i] = '?';
        }
        fileName = sb.ToString();
      }

      return String.Equals(fileName, template, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Проверка имени файла и каталога

    // Эти методы используют аргументы String, а не AbsPath, т.к. их целью является проверка наличия символов backslash в конце пути

    #region Режим FormatOnly

    /// <summary>
    /// Проверка имени каталога, завершающегося обратной чертой
    /// Реальное существование каталога не проверяется.
    /// Перегрузка соответствует режиму TestPathMode.FormatOnly.
    /// </summary>
    /// <param name="dirName">Имя каталога, выбранное пользователем</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true - имя каталога правильное</returns>
    public static bool TestDirSlashedPath(string dirName, out string errorText)
    {
      if (!TestChars(dirName, out errorText))
        return false;

      if (dirName[dirName.Length - 1] != System.IO.Path.DirectorySeparatorChar)
      {
        errorText = "Имя каталога должно заканчиваться символом \"" + System.IO.Path.DirectorySeparatorChar + "\"";
        return false;
      }

      return true;
    }

    /// <summary>
    /// Проверка имени файла
    /// Реальное существование файла и пути к нему не проверяется
    /// Перегрузка соответствует режиму TestPathMode.FormatOnly.
    /// </summary>
    /// <param name="fileName">Имя файла, выбранное пользователем</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true - имя файла правильное</returns>
    public static bool TestFilePath(string fileName, out string errorText)
    {
      if (!TestChars(fileName, out errorText))
        return false;

      if (fileName[fileName.Length - 1] == System.IO.Path.DirectorySeparatorChar)
      {
        errorText = "Имя файла не должно заканчиваться символом \"" + System.IO.Path.DirectorySeparatorChar + "\"";
        return false;
      }

      switch (System.Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
        case PlatformID.Win32S:
        case PlatformID.WinCE:
          if (fileName.EndsWith(":"))
          {
            errorText = "Имя файла не должно заканчиваться символом \":\"";
            return false;
          }

          // 31.12.2020
          if (fileName.StartsWith("\\\\"))
          {
            if (DataTools.GetCharCount(fileName, '\\') < 4)
            {
              errorText = "Если имя начинается с \\\\, то должно быть еще два разделителя: \\Сервер\\Ресурс\\ИмяФайла";
              return false;
            }
          }

          break;
      }

      return true;
    }

    private static bool TestChars(string name, out string errorText)
    {
      if (String.IsNullOrEmpty(name))
      {
        errorText = "Путь не задан";
        return false;
      }

      char[] BadChars = System.IO.Path.GetInvalidPathChars();
      for (int i = 0; i < name.Length; i++)
      {
        if (Array.IndexOf<char>(BadChars, name[i]) >= 0)
        {
          errorText = "Недопустимый символ \"" + name[i] + "\" в позиции " + (i + 1).ToString();
          return false;
        }

        if (name[i] == System.IO.Path.AltDirectorySeparatorChar &&
          System.IO.Path.AltDirectorySeparatorChar != System.IO.Path.DirectorySeparatorChar)
        {
          errorText = "Недопустимый символ \"" + name[i] + "\" в позиции " + (i + 1).ToString() + ". Для разделения каталогов должен использоваться символ \"" +
            System.IO.Path.DirectorySeparatorChar + "\"";
          return false;
        }

        if (name[i] == '*' || name[i] == '?')
        {
          errorText = "Недопустимый символ \"" + name[i] + "\" в позиции " + (i + 1).ToString() + ". Шаблонные символы не допускаются";
          return false;
        }

        switch (System.Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
          case PlatformID.Win32S:
          case PlatformID.WinCE:
            if (!TestWindowsChars(name, out errorText))
              return false;
            break;
        }
      }

      if (name[0] == ' ')
      {
        errorText = "Путь не должен начинаться с пробела";
        return false;
      }

      if (name[name.Length - 1] == ' ')
      {
        errorText = "Путь не должен заканчиваться пробелом";
        return false;
      }

      int p = name.IndexOf(System.IO.Path.DirectorySeparatorChar + " ", StringComparison.Ordinal);
      if (p >= 0)
      {
        errorText = "Недопустимое сочетание символов в позиции " + (p + 1).ToString() + ". После разделителя каталога не может идти пробел";
        return false;
      }

      p = name.IndexOf(" " + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal);
      if (p >= 0)
      {
        errorText = "Недопустимое сочетание символов в позиции " + (p + 1).ToString() + ". Перед разделителем каталога не может идти пробел";
        return false;
      }

      errorText = null;
      return true;
    }

    private static bool TestWindowsChars(string name, out string errorText)
    {
      int p = name.IndexOf("\\\\", StringComparison.Ordinal);
      if (p >= 0)
      {
        if (p > 0 || name.LastIndexOf("\\\\", StringComparison.Ordinal) != p)
        {
          errorText = "Два символа \"\\\\\" подряд (признак сетевого) имени может быть только в начале имени";
          return false;
        }
      }

      p = name.IndexOf(':');
      if (p >= 0)
      {
        if (p != 1 || name.LastIndexOf(':') != p)
        {
          errorText = "Символ \":\" может быть только вторым в строке";
          return false;
        }

        char FirstChar = char.ToUpperInvariant(name[0]);
        if (FirstChar < 'A' || FirstChar > 'Z')
        {
          errorText = "Перед символом \":\" должна идти буква диска";
          return false;
        }
      }

      errorText = null;
      return true;
    }

    #endregion

    #region Проверка с доступом к файловой системе

    /// <summary>
    /// Проверка имени каталога, завершающегося обратной чертой.
    /// Может проверять реальное существование каталога, в зависимости от режима.
    /// В режиме <paramref name="mode"/>=None всегда возвращает true.
    /// </summary>
    /// <param name="dirName">Имя каталога, выбранное пользователем</param>
    /// <param name="mode">Режим проверки. Значение FileExists не допускается</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true - имя каталога правильное</returns>
    public static bool TestDirSlashedPath(string dirName, TestPathMode mode, out string errorText)
    {
      if (mode == TestPathMode.None)
      {
        errorText = null;
        return true;
      }

      if (!TestDirSlashedPath(dirName, out errorText))
        return false; // неправильный формат

      switch (mode)
      {
        case TestPathMode.FormatOnly:
          return true;
        case TestPathMode.RootExists:
          try
          {
            if (Directory.Exists(new AbsPath(dirName).RootDir.Path))
              return true;
            else
            {
              errorText = "Не найден корневой каталог \"" + new AbsPath(dirName).RootDir.Path + "\"";
              return false;
            }
          }
          catch (Exception e)
          {
            errorText = e.Message;
            return false;
          }
        case TestPathMode.DirectoryExists:
          try
          {
            if (Directory.Exists(new AbsPath(dirName).Path))
              return true;
            else
            {
              errorText = "Не найден каталог \"" + new AbsPath(dirName).Path + "\"";
              return false;
            }
          }
          catch (Exception e)
          {
            errorText = e.Message;
            return false;
          }
        default:
          throw new ArgumentException("Неизвестный режим " + mode.ToString(), "mode");
      }
    }

    /// <summary>
    /// Проверка имени файла
    /// Может проверять реальное существование каталога и файла, в зависимости от режима.
    /// В режиме <paramref name="mode"/>=None всегда возвращает true.
    /// </summary>
    /// <param name="fileName">Имя файла, выбранное пользователем</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <param name="mode">Режим проверки</param>
    /// <returns>true - имя файла правильное</returns>
    public static bool TestFilePath(string fileName, TestPathMode mode, out string errorText)
    {
      if (mode == TestPathMode.None)
      {
        errorText = null;
        return true;
      }

      if (!TestFilePath(fileName, out errorText))
        return false; // неправильный формат

      AbsPath path = new AbsPath(fileName);

      if (!TestDirSlashedPath(path.ParentDir.SlashedPath, mode == TestPathMode.FileExists ? TestPathMode.DirectoryExists : mode, out errorText))
        return false;

      if (mode == TestPathMode.FileExists)
      {
        try
        {
          if (File.Exists(path.Path))
            return true;
          else if (Directory.Exists(path.Path))
          {
            errorText = "\"" + path.Path + "\" является каталогом, а не файлом";
            return false;
          }
          else
          {
            errorText = "Файл не найден: \"" + path.Path + "\"";
            return false;
          }
        }
        catch (Exception e)
        {
          errorText = e.Message;
          return false;
        }
      }
      else
        return true;
    }

    #endregion

    #endregion

    #region Проверка существования

    /// <summary>
    /// Проверяет наличие файла на диске.
    /// Если файл не существует, то генерируется исключение FileNotFoundException
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    public static void CheckFileExists(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentException("Путь не задан", "filePath");

      if (!File.Exists(filePath.Path))
        throw new FileNotFoundException("Файл " + filePath.QuotedPath + " не найден");
    }

    /// <summary>
    /// Проверяет наличие каталога на диске.
    /// Если каталога не существует, то генерируется исключение DirectoryNotFoundException
    /// </summary>
    /// <param name="dirPath">Путь к каталогу</param>
    public static void CheckDirectoryExists(AbsPath dirPath)
    {
      if (dirPath.IsEmpty)
        throw new ArgumentException("Путь не задан", "dirPath");

      if (!Directory.Exists(dirPath.Path))
        throw new DirectoryNotFoundException("Каталог " + dirPath.QuotedPath + " не найден");
    }

    #endregion

    #region Удаление файла

    /// <summary>
    /// Удаление файла.
    /// Если для файла установлен атрибут "Только для чтения", атрибут снимается
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    [DebuggerStepThrough]
    public static void DeleteFile(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      try
      {
        File.Delete(filePath.Path);
      }
      catch (UnauthorizedAccessException /*e*/)
      {
        File.SetAttributes(filePath.Path, FileAttributes.Normal);
        File.Delete(filePath.Path);
      }
    }

    /// <summary>
    /// Удаление файлов.
    /// Комбинация вызова метода System.IO.Directory.GetFiles() и 
    /// удаления каждого файла с помощью DeleteFile().
    /// Если для файла установлен атрибут "Только для чтения", атрибут снимается.
    /// В режиме рекурсивного удаления каталоги не удаляются.
    /// Если требуется очистить каталог от всех файлов и подкаталогов, используйте
    /// метод ForceDirsAndClear().
    /// В случае невозможности удалить один из файлов выбрасывается исключение.
    /// При этом некоторые файлы уже могут быть удалены.
    /// </summary>
    /// <param name="dirPath">Путь к каталогу. Должен быть задан</param>
    /// <param name="searchPattern">Маска поиска файлов</param>
    /// <param name="searchOption">Выполнить удаление только из текущего каталога,
    /// или рекурсивно просмотреть каталоги</param>
    public static void DeleteFiles(AbsPath dirPath, string searchPattern, SearchOption searchOption)
    {
      if (dirPath.IsEmpty)
        throw new ArgumentNullException("dirPath");
      string[] aFiles = Directory.GetFiles(dirPath.Path, searchPattern, searchOption);
      for (int i = 0; i < aFiles.Length; i++)
        DeleteFile(new AbsPath(aFiles[i]));
    }


    /// <summary>
    /// Удаление файлов из текущего каталога.
    /// Комбинация вызова метода System.IO.Directory.GetFiles() и 
    /// удаления каждого файла с помощью DeleteFile().
    /// Если для файла установлен атрибут "Только для чтения", атрибут снимается.
    /// Подкаталоги не просматриваются.
    /// В случае невозможности удалить один из файлов выбрасывается исключение.
    /// При этом некоторые файлы уже могут быть удалены.
    /// </summary>
    /// <param name="dirPath">Путь к каталогу. Должен быть задан</param>
    /// <param name="searchPattern">Маска поиска файлов</param>
    public static void DeleteFiles(AbsPath dirPath, string searchPattern)
    {
      DeleteFiles(dirPath, searchPattern, SearchOption.TopDirectoryOnly);
    }

    #endregion

    #region Создание цепочки каталогов

    /// <summary>
    /// Обеспечение существования каталога. Имя каталога может, но не обязано заканчиваться на слэш
    /// </summary>
    /// <param name="dirPath">Каталог, который должен существовать</param>
    public static void ForceDirs(AbsPath dirPath)
    {
      if (dirPath.IsEmpty)
        throw new ArgumentNullException("dirPath");
      if (!Directory.Exists(dirPath.Path))
        Directory.CreateDirectory(dirPath.Path);

    }

    /// <summary>
    /// Создать каталог, если его не существует, затем удалить все файлы, и 
    /// вложенные каталоги, которые в нем есть
    /// Если очистка не удалась, вызывается исключения
    /// </summary>
    /// <param name="dirPath">Имя создаваемого каталога</param>
    public static void ForceDirsAndClear(AbsPath dirPath)
    {
      ForceDirsAndClear(dirPath, true);
    }

    /// <summary>
    /// Создать каталог, если его не существует, затем удалить все файлы, и 
    /// вложенные каталоги, которые в нем есть
    /// Возможна либо обязательная очистка, либо очистка "по возможности"
    /// </summary>
    /// <param name="dirPath">Имя создаваемого каталога</param>
    /// <param name="mustClear">Надо ли выбрасывать исключение при невозможности очистки</param>
    public static void ForceDirsAndClear(AbsPath dirPath, bool mustClear)
    {
      ForceDirs(dirPath);

      if (mustClear)
      {
        string[] Files = Directory.GetFiles(dirPath.Path);
        for (int i = 0; i < Files.Length; i++)
          DeleteFile(new AbsPath(Files[i]));
        string[] SubDirs = Directory.GetDirectories(dirPath.Path);
        for (int i = 0; i < SubDirs.Length; i++)
          Directory.Delete(SubDirs[i], true);
      }
      else
        ClearDirAsPossible(dirPath);
    }

    /// <summary>
    /// Очищает каталог от файлов и вложенных каталогов, насколько это возможно
    /// Сам каталог <paramref name="dirPath"/> не удаляется
    /// В отличие от методов рекурсивного удаления Directory.Delete(), невозможность удаления одного из файлов не прекращает процесс
    /// Предупреждение. Неверное применение метода может привести к разрушительным последствиям
    /// </summary>
    /// <param name="dirPath">Очищаемый каталог</param>
    /// <returns>true, если очистка успешно выполнена</returns>
    [DebuggerStepThrough]
    public static bool ClearDirAsPossible(AbsPath dirPath)
    {
      if (dirPath.IsEmpty)
        throw new ArgumentNullException("dirPath");

      try
      {
        if (!Directory.Exists(dirPath.Path))
          return true; // 25.04.2017
      }
      catch // 27.12.2019. Перехватываем IOException на случай повреждения структуры каталогов
      {
        return false;
      }

      bool AllDeleted = true;

      #region Очистка файлов

      string[] a;
      try
      {
        a = Directory.GetFiles(dirPath.Path);
      }
      catch // 27.12.2019. Перехватываем IOException на случай повреждения структуры каталогов
      {
        a = DataTools.EmptyStrings;
        AllDeleted = false;
      }
      for (int i = 0; i < a.Length; i++)
      {
        try
        {
          File.Delete(a[i]); // здесь не используем DeleteFile(), т.к. снимать атрибут не надо
        }
        catch
        {
          AllDeleted = false;
          // но break не делаем
        }
      }

      #endregion

      #region Удаление вложенных каталогов

      try
      {
        a = Directory.GetDirectories(dirPath.Path);
      }
      catch // 27.12.2019. Перехватываем IOException на случай повреждения структуры каталогов
      {
        a = DataTools.EmptyStrings;
        AllDeleted = false;
      }
      for (int i = 0; i < a.Length; i++)
      {
        AbsPath SubDir = new AbsPath(a[i]);
        if (!ClearDirAsPossible(SubDir))
        {
          AllDeleted = false;
          continue;
        }

        try
        {
          Directory.Delete(SubDir.Path);
        }
        catch
        {
          AllDeleted = false;
        }
      }

      #endregion

      return AllDeleted;
    }

    /// <summary>
    /// Очищает каталог от файлов и вложенных каталогов, насколько это возможно
    /// В отличие от ClearDirAsPossible(), сам каталог <paramref name="dirPath"/> также удаляется
    /// В отличие от методов рекурсивного удаления Directory.Delete(), невозможность удаления одного из файлов не прекращает процесс
    /// Предупреждение. Неверное применение метода может привести к разрушительным последствиям
    /// </summary>
    /// <param name="dirPath">Удаляемый каталог</param>
    /// <returns>true, если очистка успешно выполнена</returns>
    public static bool DeleteDirAsPossible(AbsPath dirPath)
    {
      if (!ClearDirAsPossible(dirPath))
        return false;

      try
      {
        Directory.Delete(dirPath.Path);
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Рекурсивное удаление пустых каталогов.
    /// Файлы не удаляются. Каталоги, в которых есть файлы также не удаляются.
    /// В отличие от метода Directory.Delete(каталог, true), удаление пустых каталогов не прекращается, даже встретился 
    /// непустой каталог. Также, при <paramref name="deleteRootDir"/>=false можно удалить подкаталоги, оставив корневой каталог.
    /// </summary>
    /// <param name="dirPath">Каталог, начиная с которого надо выполнить удаление</param>
    /// <param name="deleteRootDir">Если true, то сам каталог <paramref name="dirPath"/> также будет удален, если он пустой.
    /// Если false, то корневой каталог не проверяется</param>
    /// <returns>true, если были удалены все каталоги</returns>
    public static bool DeleteEmptyDirs(AbsPath dirPath, bool deleteRootDir)
    {
      if (dirPath.IsEmpty)
        throw new ArgumentException("Каталог не задан", "dirPath");

      bool res = true;

      try
      {
        if (!System.IO.Directory.Exists(dirPath.Path))
          return true;

        #region Удаляем подкаталоги

        string[] aSubDirs = System.IO.Directory.GetDirectories(dirPath.Path);
        for (int i = 0; i < aSubDirs.Length; i++)
        {
          try
          {
            if (!DeleteEmptyDirs(new AbsPath(aSubDirs[i]), true)) // рекурсивный вызов
              res = false;
          }
          catch { res = false; }
        }

        #endregion

        #region Проверяем наличие файлов

        if (res)
        {
          try
          {
            if (System.IO.Directory.GetFiles(dirPath.Path).Length > 0)
              res = false;
          }
          catch { res = false; }
        }

        #endregion

        #region Удаляем текущий каталог

        if (deleteRootDir && res)
        {
          try
          {
            System.IO.Directory.Delete(dirPath.Path);
          }
          catch { res = false; }

        }

        #endregion
      }
      catch
      {
        res = false;
      }

      return res;
    }

    #endregion

    #region Короткие имена файлов 8.3 в Windows

    /// <summary>
    /// Для Windows возвращает длинное имя файла, соответствующее короткому имени в формате 8.3.
    /// Для других платформ возвращает аргумент неизменным.
    /// </summary>
    /// <param name="shortPath">Путь с указанием короткого имени файла</param>
    /// <returns>Длинное имя</returns>
    public static AbsPath GetLongPath(AbsPath shortPath)
    {
      if (shortPath.IsEmpty)
        return AbsPath.Empty;

      if (!WindowsNative.IsWindowsPlatform)
        return shortPath;

      if (shortPath.Path.Length == 3 && shortPath.Path.EndsWith(":\\"))
        return shortPath; // только имя диска, например, "C:\"
      StringBuilder sbLongPath = new StringBuilder(255);
      int res = WindowsNative.GetLongPathName(shortPath.Path, sbLongPath, sbLongPath.Capacity);
      if (res == 0 || res > sbLongPath.Capacity)
        MyThrowWin32Exception(shortPath);
      return new AbsPath(sbLongPath.ToString());
    }

    /// <summary>
    /// Для Windows возвращает короткое имя файла в формате 8.3, соответствующее длинному имени.
    /// Для других платформ возвращает аргумент неизменным.
    /// </summary>
    /// <param name="longPath">Длинное имя</param>
    /// <returns>Короткое имя</returns>
    public static AbsPath GetShortPath(AbsPath longPath)
    {
      if (longPath.IsEmpty)
        return AbsPath.Empty;

      if (!WindowsNative.IsWindowsPlatform)
        return longPath;

      if (longPath.Path.Length == 3 && longPath.Path.EndsWith(":\\"))
        return longPath; // только имя диска, например, "C:\"

      StringBuilder sbShortPath = new StringBuilder(255);
      int res = WindowsNative.GetShortPathName(longPath.Path, sbShortPath, sbShortPath.Capacity);
      if (res == 0 || res > sbShortPath.Capacity)
        MyThrowWin32Exception(longPath);
      return new AbsPath(sbShortPath.ToString());
    }

    private static void MyThrowWin32Exception(AbsPath path)
    {
      Win32Exception e1 = new Win32Exception();
      Win32Exception e2 = new Win32Exception(e1.ErrorCode, e1.Message + ". Имя: \"" + path.Path + "\"");
      throw e2;
    }

    #endregion

    #region Чтение и запись Stream

    /// <summary>
    /// Записать двоичный поток в файл
    /// </summary>
    /// <param name="filePath">Имя файла</param>
    /// <param name="sourceStream">Исходный поток</param>
    public static void WriteStream(AbsPath filePath, Stream sourceStream)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      using (FileStream fs = new FileStream(filePath.Path, FileMode.Create, FileAccess.Write))
      {
        CopyStream(sourceStream, fs);
      }
    }


    /// <summary>
    /// Копирование исходного потока SrcStream в результирующий DstStream
    /// Выполняется позиционирование исходного потока на начало
    /// </summary>
    /// <param name="srcStream">Исходный поток</param>
    /// <param name="dstStream">Записываемый поток</param>
    public static void CopyStream(Stream srcStream, Stream dstStream)
    {
      CopyStream(srcStream, dstStream, true);
    }

    /// <summary>
    /// Копирование исходного потока SrcStream в результирующий DstStream
    /// Выполняется позиционирование исходного потока на начало
    /// </summary>
    /// <param name="srcStream">Исходный поток</param>
    /// <param name="dstStream">Записываемый поток</param>
    /// <param name="seekStart">Если true, то в исходном потоке будет выполнено позиционирование на начало потока (поток должен поддерживать позиционирование).
    /// Если false, то позиционирование не выполняется. Чтение начинается с текущей позиции.</param>
    public static void CopyStream(Stream srcStream, Stream dstStream, bool seekStart)
    {
#if DEBUG
      if (srcStream == null)
        throw new ArgumentNullException("srcStream");
      if (dstStream == null)
        throw new ArgumentNullException("dstStream");
#endif

      int BufLen = 1024;
      if (seekStart)
      {
        srcStream.Seek(0, SeekOrigin.Begin);

        long l = srcStream.Length;
        if (l == 0L)
          return;
        if (l > 0L && l < 1024L)
          BufLen = Math.Max(BufLen, (int)l);
      }

      byte[] Buffer = new byte[BufLen];
      while (true)
      {
        int n = srcStream.Read(Buffer, 0, Buffer.Length);
        if (n == 0)
          break;

        dstStream.Write(Buffer, 0, n);
      }
    }

    /// <summary>
    /// Читает все данные до конца потока и возвращает их в виде массива байтов.
    /// Если на момент вызова уже достигнут конец потока, возвращается пустой массив
    /// Позиционирование в потоке не выполняется.
    /// </summary>
    /// <param name="srcStream">Поток, откуда выполняется чтение</param>
    /// <returns>Результирующий массив</returns>
    public static byte[] ReadAllBytes(Stream srcStream)
    {
#if DEBUG
      if (srcStream == null)
        throw new ArgumentNullException("srcStream");
#endif

      byte[] b;
      using (MemoryStream ms = new MemoryStream())
      {
        CopyStream(srcStream, ms, false);
        ms.Flush();
        b = ms.ToArray();
      }
      return b;
    }

    /// <summary>
    /// Записывает в поток все данные из массива <paramref name="buffer"/>.
    /// Этот метод реализован только для симметрии с ReadAllBytes(). 
    /// Он просто вызывает метод Stream.Write() для всего массива.
    /// </summary>
    /// <param name="dstStream">Записываемый поток</param>
    /// <param name="buffer">Данные</param>
    public static void WriteAllBytes(Stream dstStream, byte[] buffer)
    {
#if DEBUG
      if (dstStream == null)
        throw new ArgumentNullException("dstStream");
      if (buffer == null)
        throw new ArgumentNullException("buffer");
#endif

      dstStream.Write(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Проверка сигнатуры в файле.
    /// Возвращает true, если в текущей позиции потока находятся символы, задающие заданную сигнатуру
    /// </summary>
    /// <param name="reader">Объект для считывания текста</param>
    /// <param name="signature">Проверяемая сигнатура</param>
    /// <returns>true, если сигнатура есть</returns>
    public static bool TestSignature(TextReader reader, string signature)
    {
#if DEBUG
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (String.IsNullOrEmpty(signature))
        throw new ArgumentNullException("signature");
#endif

      char[] buf = new char[signature.Length];
      if (reader.Read(buf, 0, signature.Length) < signature.Length)
        return false; // файл закончился раньше

      for (int i = 0; i < signature.Length; i++)
      {
        if (buf[i] != signature[i])
          return false;
      }
      return true;
    }

    /// <summary>
    /// Массив нулей
    /// </summary>
    private static readonly byte[] ZeroBytes = new byte[1024];

    /// <summary>
    /// Запись заданного количество нулей в поток с оптимизацией по скорости
    /// </summary>
    /// <param name="resStream"></param>
    /// <param name="count"></param>
    public static void WriteZeros(Stream resStream, long count)
    {
      while (count >= 1024)
      {
        resStream.Write(ZeroBytes, 0, 1024);
        count -= 1024;
      }

      if (count > 0)
        resStream.Write(ZeroBytes, 0, (int)count);
    }

    #endregion

    #region Проверка начала потока

    /// <summary>
    /// Возвращает true, если поток, начиная с текущей позиции, содержит указанные байты.
    /// После вызова метода текущая позиция восстанавливается.
    /// Поток должен поддерживать позиционирование, так как используется свойство Position. В противном случае будет сгенерировано NotSupportedException
    /// </summary>
    /// <param name="testStream">Поток, открытый на чтение</param>
    /// <param name="bytes">Проверяемая последовательность байтов</param>
    /// <returns></returns>
    public static bool StartsWith(Stream testStream, byte[] bytes)
    {
      if (testStream == null)
        throw new ArgumentNullException("testStream");
      if (bytes == null)
        throw new ArgumentNullException("bytes");

      bool Res;
      long StartPos = testStream.Position;
      try
      {
        byte[] Bytes2 = new byte[bytes.Length];
        if (testStream.Read(Bytes2, 0, Bytes2.Length) == bytes.Length)
        {
          Res = true;
          for (int i = 0; i < bytes.Length; i++)
          {
            if (Bytes2[i] != bytes[i])
            {
              Res = false;
              break;
            }
          }
        }
        else
          Res = false;
      }
      finally
      {
        testStream.Position = StartPos;
      }

      return Res;
    }

    #endregion

    #region Текстовые файлы

    /// <summary>
    /// Возвращает кодировку для текстовых файлов, чтобы их можно было открывать средствами ОС (блокнотом).
    /// Для Windows-NT, Linux возвращает UTF-8.
    /// Для Windows-98/Me возвращает Encoding.Default
    /// </summary>
    public static Encoding TextFileEncoding
    {
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32Windows:
          case PlatformID.Win32S:
            return Encoding.Default;
          default:
            return Encoding.UTF8;
        }
      }
    }

    #endregion

    #region Чтение и запись XML-документов

    #region Запись

    /// <summary>
    /// Запись XML-документа в текстовый файл.
    /// Используется кодировка, заданная в декларации XML-документа или unicode,
    /// если декларации нет
    /// Выполняется красивое форматирование документа
    /// </summary>
    /// <param name="filePath">Имя файла для записи</param>
    /// <param name="xmlDoc">Записываемый документ</param>
    public static void WriteXmlDocument(AbsPath filePath, XmlDocument xmlDoc)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      Encoding enc = DataTools.GetXmlEncoding(xmlDoc);
      XmlWriterSettings Settings = new XmlWriterSettings();
      Settings.NewLineChars = "\r\n";
      Settings.Encoding = enc;
      Settings.Indent = true;
      Settings.IndentChars = "  ";
      XmlWriter wrt = XmlWriter.Create(filePath.Path, Settings);
      try
      {
        xmlDoc.WriteTo(wrt);
      }
      finally
      {
        wrt.Close();
      }
    }

    /// <summary>
    /// Запись XML-документа в текстовый файл, заданный как поток (например, MemoryStream).
    /// Используется кодировка, заданная в декларации XML-документа или unicode,
    /// если декларации нет
    /// Выполняется красивое форматирование документа
    /// </summary>
    /// <param name="outStream">Поток для записи</param>
    /// <param name="xmlDoc">Записываемый документ</param>
    public static void WriteXmlDocument(Stream outStream, XmlDocument xmlDoc)
    {
      if (outStream==null)
        throw new ArgumentNullException("outStream");
      if (xmlDoc== null)
        throw new ArgumentNullException("xmlDoc");

      Encoding enc = DataTools.GetXmlEncoding(xmlDoc);
      XmlWriterSettings Settings = new XmlWriterSettings();
      Settings.NewLineChars = "\r\n";
      Settings.Encoding = enc;
      Settings.Indent = true;
      Settings.IndentChars = "  ";
      StreamWriter wrt1 = new StreamWriter(outStream, enc);
      XmlWriter wrt2 = XmlWriter.Create(wrt1, Settings);
      try
      {
        xmlDoc.WriteTo(wrt2);
      }
      finally
      {
        wrt2.Close();
        wrt1.Close();
      }
    }

    #endregion

    #region Чтение

    /// <summary>
    /// Чтение XML-документа.
    /// Вызывает XmlDocument.Load()
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>XML-документ</returns>
    public static XmlDocument ReadXmlDocument(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      XmlDocument Doc = new XmlDocument();
      Doc.Load(filePath.Path);
      return Doc;
    }

    /// <summary>
    /// Чтение XML-документа.
    /// Вызывает XmlDocument.Load()
    /// </summary>
    /// <param name="inStream">Поток для загрузки XML-документа</param>
    /// <returns>XML-документ</returns>
    public static XmlDocument ReadXmlDocument(Stream inStream)
    {
      if (inStream == null)
        throw new ArgumentNullException("inStream");

      XmlDocument Doc = new XmlDocument();
      Doc.Load(inStream);
      return Doc;
    }

    #endregion

    #region Проверка начала файла


    /// <summary>
    /// Начало любого XML-файла в однобайтной кодировке (например, 1251 или 866)
    /// </summary>
    private static readonly byte[] XmlStartAnsiBytes = new byte[] { 0x3c, 0x3f, 0x78, 0x6d, 0x6c, 0x20 };


    /// <summary>
    /// Начало любого XML-файла в кодировке utf-8
    /// </summary>
    private static readonly byte[] XmlStartUtf8Bytes = new byte[] { 0xEF, 0xBB, 0xBF, 0x3c, 0x3f , 0x78 , 0x6d , 0x6c , 0x20 };


    /// <summary>
    /// Начало любого XML-файла в кодировке utf-16
    /// </summary>
    private static readonly byte[] XmlStartUtf16Bytes = new byte[] { 0xFF, 0xFE , 0x3c , 0x00 , 0x3f , 0x00 , 0x78 , 0x00 , 0x6d, 0x00, 0x6c, 0x00, 0x20, 0x00 };

    /// <summary>
    /// Начало любого XML-файла в кодировке utf-16BE
    /// </summary>
    private static readonly byte[] XmlStartUtf16BEBytes = new byte[] { 0xFE, 0xFF , 0x00 , 0x3c , 0x00 , 0x3f , 0x00 , 0x78 , 0x00, 0x6d, 0x00, 0x6c, 0x00, 0x20 };

    /// <summary>
    /// Начало любого XML-файла в кодировке utf-32
    /// </summary>
    private static readonly byte[] XmlStartUtf32Bytes = new byte[] { 0xFF , 0xFE , 0x00 , 0x00 ,
                                                                     0x3c , 0x00 , 0x00 , 0x00 ,
                                                                     0x3f, 0x00, 0x00, 0x00,
                                                                     0x78, 0x00, 0x00, 0x00,
                                                                     0x6d, 0x00, 0x00, 0x00,
                                                                     0x6c, 0x00, 0x00, 0x00,
                                                                     0x20, 0x00, 0x00, 0x00};
    /// <summary>
    /// Начало любого XML-файла в кодировке utf-32BE
    /// </summary>
    private static readonly byte[] XmlStartUtf32BEBytes = new byte[] { 0x00, 0x00, 0xFE, 0xFF,
                                                                       0x00, 0x00, 0x00, 0x3c,
                                                                       0x00, 0x00, 0x00, 0x3f,
                                                                       0x00, 0x00, 0x00, 0x78,
                                                                       0x00, 0x00, 0x00, 0x6d,
                                                                       0x00, 0x00, 0x00, 0x6c,
                                                                       0x00, 0x00, 0x00, 0x20};

    /// <summary>
    /// Массив возможных "начал" файлов
    /// </summary>
    private static readonly byte[][] XmlStartAnyBytes = new byte[][] { XmlStartAnsiBytes, XmlStartUtf8Bytes,
      XmlStartUtf16Bytes, XmlStartUtf16BEBytes, XmlStartUtf32Bytes, XmlStartUtf32BEBytes };

    /// <summary>
    /// Возвращает true, если байты потока, начиная с текущей позиции, соответствуют XML-файлу в любой (определяемой) кодировке
    /// Функция применяется для проверки загруженного в память файла неизвестного
    /// содержимого перед вызовом XmlDocument.Load(), чтобы избежать лишнего вызова
    /// исключения, когда файл может быть не-XML документом.
    /// Проверяется только начало файла, а не корректность всего XML-документа.
    /// Поток должен поддерживать позиционирование, так как используется свойство Position. В противном случае будет сгенерировано NotSupportedException
    /// </summary>
    /// <param name="inStream">Открытый на чтение поток</param>
    /// <returns>true, если есть смысл попытаться преобразовать файл в XML-формат</returns>
    public static bool IsValidXmlStart(Stream inStream)
    {
      if (inStream == null)
        return false;

      for (int i = 0; i < XmlStartAnyBytes.Length; i++)
      {
        if (StartsWith(inStream, XmlStartAnyBytes[i]))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Возвращает true, если байты потока, начиная с текущей позиции, соответствуют XML-файлу в любой (определяемой) кодировке
    /// Функция применяется для проверки загруженного в память файла неизвестного
    /// содержимого перед вызовом XmlDocument.Load(), чтобы избежать лишнего вызова
    /// исключения, когда файл может быть не-XML документом.
    /// Проверяется только начало файла, а не корректность всего XML-документа.
    /// Рекомендуется использовать перегрузку с аргументом InStream, во избежание повторного открытия файла, если планируется дальнейшая загрузка документа
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>true, если есть смысл попытаться преобразовать файл в XML-формат</returns>
    public static bool IsValidXmlStart(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        return false;
      if (!File.Exists(filePath.Path))
        return false;

      using (FileStream Stream = new FileStream(filePath.Path, FileMode.Open, FileAccess.Read))
      {
        return IsValidXmlStart(Stream);
      }
    }


    #endregion

    #endregion

    #region Чтение и запись файлов DataSet

    /// <summary>
    /// Записать набор данных DataSet в двоичном формате в файл
    /// Поскольку формат двоичного файла не документирован и теоретически
    /// может измениться в будущих версиях NET Framework, двоичный формат
    /// следует использовать только для временных файлов, которые можно
    /// будет удалить.
    /// Если файл существует, то он будет перезаписан
    /// </summary>
    /// <param name="filePath">Путь создаваемого файла</param>
    /// <param name="ds"></param>
    public static void WriteDataSetBinary(AbsPath filePath, DataSet ds)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      ds.RemotingFormat = SerializationFormat.Binary;
      ds.AcceptChanges();
      FileStream fs = new FileStream(filePath.Path, FileMode.Create);
      try
      {
        //DebugTools.DebugDataSet(ds, FilePath);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, ds);
      }
      finally
      {
        fs.Close();
      }
    }

    /// <summary>
    /// Прочитать набор данных DataSet из файла двоичного формата,
    /// созданного WriteDataSetBinary()
    /// </summary>
    /// <param name="filePath">Путь к файлу. Файл должен существовать</param>
    /// <returns>Загруженный набор данных</returns>
    public static DataSet ReadDataSetBinary(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      DataSet ds;
      FileStream fs = new FileStream(filePath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      try
      {
        BinaryFormatter bf = new BinaryFormatter();
        ds = (DataSet)(bf.Deserialize(fs));
      }
      finally
      {
        fs.Close();
      }
      return ds;
    }

    #endregion

    #region Версия файла

    /// <summary>
    /// Получить версию выполняемого файла с заданным путем
    /// Если файл не существует, или для него не задана сводка, то возвращается null
    /// </summary>
    /// <param name="filePath">Путь к EXE, DLL или другому файлу</param>
    /// <returns>Версия файла</returns>
    public static Version GetFileVersion(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        return null;

      if (!File.Exists(filePath.Path))
        return null;

      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath.Path);
      return GetFileVersion(fvi);
    }

    /// <summary>
    /// Получить версию в виде объекта Version из объекта FileVersionInfo
    /// Берется поле FileVersionInfo.FileVersion, а при его отсутствии - ProductVersion
    /// Версия файла задается как строка, поэтому может содержать посторонние 
    /// символы. Эти символы удаляются, пытаемся получить то, что можно
    /// </summary>
    /// <param name="fvi">FileVersiobnInfo. Если null, то будет возвращен null</param>
    /// <returns>Версия или null</returns>
    public static Version GetFileVersion(FileVersionInfo fvi)
    {
      if (fvi == null)
        return null;

      //DebugTools.DebugObject(fvi, "FileVersionInfo");
      Version ver = GetFileVersion1(fvi.FileVersion, fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
      if (ver == null)
        ver = GetFileVersion1(fvi.ProductVersion, fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
      return ver;

    }

    private static Version GetFileVersion1(string str, int majorPart, int minorPart, int buildPart, int privatePart)
    {
      Version ver1 = GetVersionFromStr(str);
      Version ver2 = new Version(majorPart, minorPart, buildPart, privatePart);
      if (ver1 != null)
      {
        // Номер версии из чисел - это здорово, но для больших номеров он может
        // не поместиться в 16-ричное число и быть обрезан

        if (ver1.Major > ver2.Major || ver1.Minor > ver2.Minor || ver1.Build > ver2.Build || ver1.Revision > ver2.Revision)
          return ver1;
      }

      if (ver2.Major == 0 && ver2.Minor == 0 && ver2.Build == 0 && ver2.Revision == 0)
        return null; // 27.12.2020

      return ver2;
    }

    /// <summary>
    /// Попытка преобразовать строковый номер версии в объект Version.
    /// В отличие от просто конструктора объекта Version, сначала убираются
    /// плохие символы из строки. 
    /// Если попытка преобразования заканчивается неудачно, возвращается null
    /// </summary>
    /// <param name="versionStr">Версия в виде строки текста</param>
    /// <returns>Оьъект версии или null</returns>
    public static Version GetVersionFromStr(string versionStr)
    {
      if (String.IsNullOrEmpty(versionStr))
        return null;

      const string ValidChars = "0123456789.";
      for (int i = 0; i < versionStr.Length; i++)
      {
        if (ValidChars.IndexOf(versionStr[i]) < 0)
        {
          versionStr = versionStr.Substring(0, i);
          break;
        }
      }

      Version ver = null;
      try
      {
        ver = new Version(versionStr);
      }
      catch
      {
      }
      return ver;
    }

    #endregion

    #region Инофрмация из PE-файла

    /// <summary>
    /// Извлечение информации из заголовка PE-файла (EXE или DLL)
    /// Возвращает true, если приложение является 64-битным, false-если 32-битным.
    /// Возвращает null, если определить разрядность не удалось.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Разрядность</returns>
    public static bool? Is64bitPE(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        return null;

      const int IMAGE_FILE_MACHINE_I386 = 0x014c;
      const int IMAGE_FILE_MACHINE_IA64 = 0x0200;
      const int IMAGE_FILE_MACHINE_AMD64 = 0x8664;

      try
      {
        using (FileStream fs = new FileStream(filePath.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          BinaryReader rdr = new BinaryReader(fs);
          // Проверка сигнатуры MZ
          if (rdr.ReadUInt16() != 0x5A4D)
            return null;

          // Указатель на заголовок PE
          fs.Position = 0x3c;
          long PEPos = rdr.ReadUInt32();

          // Проверка сигнатуры PE
          fs.Position = PEPos;
          if (rdr.ReadUInt32() != 0x00004550)
            return null;
          UInt16 Machine = rdr.ReadUInt16();
          switch (Machine)
          {
            case IMAGE_FILE_MACHINE_I386:
              return false;
            case IMAGE_FILE_MACHINE_IA64:
            case IMAGE_FILE_MACHINE_AMD64:
              return true;
            default:
              return null;
          }
        }
      }
      catch
      {
        return null;
      }
    }

    #endregion

    #region Дискеты

    /// <summary>
    /// Возвращает true, если путь Path относится к дискете, которую надо бы проверить
    /// после записи на нее файлов
    /// </summary>
    /// <param name="path">Путь к файлу или каталогу</param>
    /// <returns>true-дискета, false - не дискета</returns>
    public static bool IsFloppyDriveDir(AbsPath path)
    {
      if (!WindowsNative.IsWindowsPlatform)
        return false; // ???

      if (path.IsEmpty)
        return false;

      if (path.Path.StartsWith("\\\\"))
      {
        return false; // !!!
      }
      else
      {
        if (path.Path.Length < 3)
          return false;
        if (path.Path.Substring(1, 2) != ":\\")
          return false;
        DriveInfo di = new DriveInfo(path.Path.Substring(0, 1));
        if (di.TotalSize <= 1457664)
          return true;
        return false;
      }
    }

    #endregion

    #region Список компьютеров в сети

    /// <summary>
    /// Свойство возвращает true, если функция GetNetworkMachineNames() поддерживается
    /// </summary>
    public static bool GetNetworkMachineNamesSupported
    {
      get
      {
        return Environment.OSVersion.Platform == PlatformID.Win32NT; // ???
      }
    }

    /// <summary>
    /// Получить список имен компьютеров в сетевом окружении
    /// Не совместимо с Windows-98
    /// </summary>
    /// <returns></returns>
    public static string[] GetNetworkMachineNames()
    {
      if (!WindowsNative.IsWindowsPlatform)
        throw new NotSupportedException();

      string[] a = GetServerList(WindowsNative.SV_101_TYPES.SV_TYPE_ALL);

      // Надо, чтобы наш компьютер был в списке
      string CompName = Environment.MachineName;
      if (!String.IsNullOrEmpty(CompName))
      {
        int p = Array.IndexOf<string>(a, CompName);
        if (p < 0)
        {
          string[] a2 = new string[a.Length + 1];
          a.CopyTo(a2, 1);
          a2[0] = CompName;
          a = a2;
        }
      }
      return a;
    }

    private static string[] GetServerList(WindowsNative.SV_101_TYPES type101)
    {
      WindowsNative.SERVER_INFO_101 si;
      IntPtr pInfo = IntPtr.Zero;
      int etriesread = 0;
      int totalentries = 0;
      List<string> srvs = new List<string>();

      try
      {
        WindowsNative.NERR err = WindowsNative.NetServerEnum(null, 101, out pInfo, -1, ref etriesread, ref totalentries, type101, null, 0);
        if ((err == WindowsNative.NERR.NERR_Success || err == WindowsNative.NERR.ERROR_MORE_DATA) && pInfo != IntPtr.Zero)
        {
          int ptr = pInfo.ToInt32();
          for (int i = 0; i < etriesread; i++)
          {
            si = (WindowsNative.SERVER_INFO_101)Marshal.PtrToStructure(new IntPtr(ptr), typeof(WindowsNative.SERVER_INFO_101));
            srvs.Add(si.sv101_name);

            ptr += Marshal.SizeOf(si);
          }
        }
      }
      finally
      {
        if (pInfo != IntPtr.Zero)
        {
          WindowsNative.NetApiBufferFree(pInfo);
        }
      }

      return (srvs.ToArray());
    }

    #endregion

    #region Копирование файлов

    /// <summary>
    /// Копирование файлов с указанием шаблонов и процентного индикатора
    /// </summary>
    /// <param name="srcDir">Каталог с исходными файлами</param>
    /// <param name="resDir">Каталог для записи файлов</param>
    /// <param name="templates">Шаблоны файлов. Если не указано, то копируются все файлы без рекурсии</param>
    /// <param name="splash">Необязательный процентный индикатор</param>
    public static void Copy(AbsPath srcDir, AbsPath resDir, FileTemplateList templates, ISplash splash)
    {
      if (srcDir.IsEmpty)
        throw new ArgumentNullException("srcDir");
      if (resDir.IsEmpty)
        throw new ArgumentNullException("resDir");
      if (templates == null)
      {
        templates = new FileTemplateList();
        templates.Add("*.*", false);
      }

      if (splash == null)
        splash = new DummySplash();

      string OldPhaseText = splash.PhaseText;
      bool OldAllowCancel = splash.AllowCancel;

      splash.PhaseText = "Построение списка файлов";
      splash.AllowCancel = false;
      string[] aFiles = templates.GetRelFileNames(srcDir, splash);

      splash.PhaseText = "Вычисление размера файлов";
      splash.PercentMax = aFiles.Length;
      splash.AllowCancel = true;
      long TotalSize = 0;
      for (int i = 0; i < aFiles.Length; i++)
      {
        FileInfo fi = new FileInfo(srcDir.SlashedPath + aFiles[i]);
        TotalSize += fi.Length;
        splash.IncPercent();
      }

      //Splash.PhaseText = "Копирование файлов";
      splash.PhaseText = OldPhaseText; // Тот текст заставки, который был задан
      splash.PercentMax = 100;
      splash.AllowCancel = true;

      byte[] Buffer = new byte[32768]; // Размер буфера копирования
      long CopiedSize = 0;
      for (int i = 0; i < aFiles.Length; i++)
      {
        #region Копирование с помощью потока

        FileStream fsSrc = new FileStream(srcDir.SlashedPath + aFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
          AbsPath ResFileName = new AbsPath(resDir, aFiles[i]);
          // Каталог для конечного файла
          ForceDirs(ResFileName.ParentDir);

          FileStream fsDst = new FileStream(ResFileName.Path, FileMode.Create, FileAccess.Write, FileShare.None);
          try
          {
            while (true)
            {
              int Count = fsSrc.Read(Buffer, 0, Buffer.Length);
              if (Count == 0)
                break;
              fsDst.Write(Buffer, 0, Count);
              CopiedSize += Count;
              if (TotalSize > 0)
                splash.Percent = (int)((double)CopiedSize / (double)TotalSize * 100.0);
            }

            fsDst.Close();
            fsSrc.Close();
          }
          finally
          {
            fsDst.Dispose();
          }
        }
        finally
        {
          fsSrc.Dispose();
        }

        #endregion

        #region Установка атрибутов файла

        FileInfo fiSrc = new FileInfo(srcDir.SlashedPath + aFiles[i]);
        FileInfo fiDst = new FileInfo(resDir.SlashedPath + aFiles[i]);
        if (fiSrc.CreationTime.Year < 1980)
          fiDst.CreationTime = fiSrc.LastWriteTime;
        else
          fiDst.CreationTime = fiSrc.CreationTime;
        fiDst.LastWriteTime = fiSrc.LastWriteTime;

        #endregion
      }

      splash.PercentMax = 0;
      splash.AllowCancel = OldAllowCancel;
      //Splash.PhaseText = OldPhaseText;
    }

    #endregion

    #region Среда выполнения

    /// <summary>
    /// Путь к приложению.
    /// Удаляет загрузчик ".vshost", если программа запущена из Visual Studio
    /// </summary>
    public static AbsPath ApplicationPath { get { return _ApplicationPath; } }
    private static readonly AbsPath _ApplicationPath = GetApplicationPath();

    private static AbsPath GetApplicationPath()
    {
      AbsPath Path;
      Assembly asm = Assembly.GetEntryAssembly();
      if (asm == null)
      {
        try
        {
          Path = new AbsPath(Process.GetCurrentProcess().MainModule.FileName);
        }
        catch
        {
          Path = new AbsPath(Environment.GetCommandLineArgs()[0]);
        }
      }
      else
        Path = new AbsPath(asm.Location);

      if (!Path.IsEmpty)
      {
        if (Path.FileNameWithoutExtension.EndsWith(".vshost", StringComparison.OrdinalIgnoreCase))
        {
          string Name = Path.FileNameWithoutExtension;
          Name = Name.Substring(0, Name.Length - 7);
          Path = new AbsPath(Path.ParentDir, (Name + Path.Extension));
        }
      }
      return Path;
    }

    /// <summary>
    /// Каталог приложения.
    /// Обычно возвращает ApplicationPath.ParentDir, но при запуске в VisualStudio или после
    /// компиляции программы обрезает каталоги "Debug", "Release", "x86" и др.
    /// </summary>
    public static AbsPath ApplicationBaseDir
    {
      get
      {
        if (_ApplicationPath.IsEmpty)
          return AbsPath.Empty;

        AbsPath Dir1 = _ApplicationPath.ParentDir;
        AbsPath Dir2 = Dir1;
        string Last = Dir2.FileName.ToUpper();
        if (Last == "DEBUG" || Last == "RELEASE")
        {
          Dir2 = Dir2.ParentDir;
          Last = Dir2.FileName.ToUpper();
          if (Last == "X86" || Last == "X64" || Last == "IA64")
          {
            Dir2 = Dir2.ParentDir;
            Last = Dir2.FileName.ToUpper();
          }
          if (Last == "BIN")
            return Dir2.ParentDir;
        }
        return Dir1;
      }
    }

    #endregion

    #region Поиск в PATH

    /// <summary>
    /// Выполняет поиск выполняемого файла с использованием переменной PATH.
    /// Если файл не найден, возвращает AbsPath.Empty.
    /// Поиск в текущем каталоге НЕ выполняется. 
    /// Если задан абсолютный путь к файлу, то проверяется наличие файла на диске. Если файл существует, возвращается <paramref name="fileName"/> без изменений.
    /// Иначе возвращается AbsPath.Empty.
    /// </summary>
    /// <param name="fileName">Имя выполняемого файла (с расширением). Может быть задан абсолютный или относительный путь.</param>
    /// <returns></returns>
    public static AbsPath FindExecutableFilePath(string fileName)
    {
      if (String.IsNullOrEmpty(fileName))
        return AbsPath.Empty; // 04.12.2018

      if (System.IO.Path.IsPathRooted(fileName)) // задан абсолютный путь к файлу?
      {
        AbsPath File = new AbsPath(fileName);
        try
        {
          if (System.IO.File.Exists(File.Path))
            return File;
        }
        catch { } // может не быть доступа к каталогу
        return AbsPath.Empty;
      }

      string sPaths = Environment.GetEnvironmentVariable("PATH");
      string[] aPaths = sPaths.Split(System.IO.Path.PathSeparator);
      for (int i = 0; i < aPaths.Length; i++)
      {
        AbsPath Dir = new AbsPath(Environment.ExpandEnvironmentVariables(aPaths[i]));
        if (Dir.IsEmpty)
          continue; // 04.12.2018
        AbsPath File = new AbsPath(Dir, fileName);
        try
        {
          if (System.IO.File.Exists(File.Path))
            return File;
        }
        catch { } // может не быть доступа к каталогу
      }
      return AbsPath.Empty;
    }

    #endregion

    #region Вычисление ХЭШ-сумм


    /// <summary>
    /// Получение хэш-суммы данных из потока по алгоритму MD5.
    /// Возвращает результат в виде 32-разрядной строки с 16-ричными символами
    /// Если Stream=null, то возвращается хэш-сумма для массива нулевой длины
    /// </summary>
    /// <param name="strm">Поток данных для чтения</param>
    /// <returns>Строка хэш-суммы</returns>
    public static string MD5Sum(Stream strm)
    {
      if (strm == null)
        return DataTools.MD5Sum(new byte[0]);
      System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
      byte[] HashRes = md5Hasher.ComputeHash(strm);
      return DataTools.BytesToHex(HashRes, false);
    }

    /// <summary>
    /// Получение хэш-суммы для файла по алгоритму MD5.
    /// Возвращает результат в виде 32-разрядной строки с 16-ричными символами.
    /// Файл должен существовать.
    /// </summary>
    /// <param name="filePath">Имя файла (с указанием пути)</param>
    /// <returns>Строка хэш-суммы</returns>
    public static string MD5Sum(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");

      string str;
      FileStream fs = new FileStream(filePath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      try
      {
        str = MD5Sum(fs);
      }
      finally
      {
        fs.Close();
      }
      return str;
    }

    #endregion
  }

  /// <summary>
  /// Временный каталог
  /// Конструктор объекта создает пустой каталог, метод Dispose() очищает все
  /// файлы и вложенные каталоги, затем удаляет сам каталог.
  /// Временные каталоги могут создаваться и удаляться асинхронно
  /// </summary>
  public class TempDirectory : DisposableObject
  {
    #region Корневой каталог для размещения файлов

    /// <summary>
    /// Корневой каталог для размещения временных файлов
    /// Если свойство не установлено в явном виде, возвращается временный каталог системы плюс вложенный каталог, совпадающий с именем программы
    /// </summary>
    public static AbsPath RootDir
    {
      get { return _RootDir; }
      set
      {
        if (value.IsEmpty)
          value = InitRootDir();
        _RootDir = value;
      }
    }
    private static AbsPath _RootDir = InitRootDir();

    private static AbsPath InitRootDir()
    {
      string AppName = null;
      try
      {
        AppName = EnvironmentTools.ApplicationName;
      }
      catch { }
      if (String.IsNullOrEmpty(AppName))
        AppName = "NonameApp";

      return new AbsPath(Path.GetTempPath()) + AppName;
    }


    #endregion

    #region Конструктор и Dispose

    /// <summary>
    /// Создает временный каталог со случайным именем в пределах RootDir.
    /// Созданный каталог не содержит файлов.
    /// </summary>
    public TempDirectory()
    {
      _DeleteOnDispose = true;

      AbsPath TempRoot = RootDir;
      lock (DataTools.InternalSyncRoot)
      {
        while (true)
        {
          _Dir = TempRoot + DataTools.TheRandom.Next().ToString("x8");
          if (!Directory.Exists(_Dir.Path))
          {
            FileTools.ForceDirs(_Dir);
            break;
          }
        }
      }
    }

    /// <summary>
    /// Удаляет все файлы и сам временный каталог.
    /// Если какой-либо файл заблокирован, и не может быть удален, он пропускается без выброса исключения, 
    /// и каталог не удаляется. Сохранение и удаление других файлов в каталоге при этом не регламентируется.
    /// Удаление файлов выполняется, даже не было явного вызова метода Dispose(), а объект разрушается деструктором.
    /// </summary>
    /// <param name="disposing">true, если метод вызван из Dispose()</param>
    [DebuggerStepThrough]
    protected override void Dispose(bool disposing)
    {
      if ((!_Dir.IsEmpty) && DeleteOnDispose)
      {
        try
        {
          if (Directory.Exists(_Dir.Path))
            Directory.Delete(_Dir.Path, true);
        }
        catch { }
        _Dir = AbsPath.Empty;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает имя временного каталога.
    /// </summary>
    public AbsPath Dir { get { return _Dir; } }
    private AbsPath _Dir;

    /// <summary>
    /// Если true (по умолчанию), то файлы и сам каталог будут удалены при вызове Dispose() или деструктора объекта.
    /// Если сбросить в false, то каталог и файлы будут сохранены. Это можно использовать, например, в отладочных целях.
    /// </summary>
    public bool DeleteOnDispose { get { return _DeleteOnDispose; } set { _DeleteOnDispose = value; } }
    private bool _DeleteOnDispose;

    /// <summary>
    /// Возвращает путь к каталогу, если не было вызова Dispose()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (_Dir.IsEmpty)
        return "[ Disposed ]";
      else
        return _Dir.Path;
    }

    #endregion
  }

  /// <summary>
  /// Разделяемый временный каталог
  /// В отличие от TempDirectory, имя каталога не является уникальным. Несколько копий программы будут
  /// использовать один и тот же каталог. Каталог не создается в конструкторе; вместо этого создание выполняется
  /// при запросе временного файла
  /// Может применяться, например, для реализации команды "Файл-Передать" в Microsoft Word
  /// Все методы, кроме Dispose(), являются потокобезопасными
  /// Так как вызов Dispose() выполняет очистку каталога, рекомендуется создать объект SharedTempDirectory 
  /// при запуске программы и удалять при завершении.
  /// </summary>
  public class SharedTempDirectory : DisposableObject
  {
    #region Конструктор

    /// <summary>
    /// Создает временный каталог по указанному пути.
    /// Выполняется попытка очистить каталог
    /// </summary>
    /// <param name="dir">Путь к каталогу</param>
    public SharedTempDirectory(AbsPath dir)
    {
      if (dir.IsEmpty)
        throw new ArgumentNullException("dir", "Не задан временный каталог");
      _Dir = dir;

      // Сразу пытаемся очистить каталог
      Clear();
    }

    /// <summary>
    /// Создает временный каталог по пути "TempDirectory.RootDir\Shared".
    /// Выполняется попытка очистить каталог
    /// </summary>
    public SharedTempDirectory()
      : this(GetDefaultDir())
    {
    }

    private static AbsPath GetDefaultDir()
    {
      return new AbsPath(TempDirectory.RootDir, "Shared");
    }

    /// <summary>
    /// Пытается очистить каталог, если вызван метод Dispose()
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        Clear();
        _Dir = AbsPath.Empty;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает имя временного каталога.
    /// </summary>
    public AbsPath Dir { get { return _Dir; } }
    private AbsPath _Dir;

    /// <summary>
    /// Возвращает путь к каталогу, если не было вызова Dispose()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (_Dir.IsEmpty)
        return "[ Disposed ]";
      else
        return _Dir.Path;
    }

    #endregion

    #region Очистка каталога

    /// <summary>
    /// Попытка очистки каталога
    /// </summary>
    public void Clear()
    {
      CheckNotDisposed();

      //lock (typeof(SharedTempDirectory))
      lock (DataTools.InternalSyncRoot) // 27.12.2020
      {
        if (Directory.Exists(Dir.Path))
          FileTools.ClearDirAsPossible(Dir);
      }
    }

    #endregion

    #region Получение имен файлов

    /// <summary>
    /// Получить имя временного файла с заданным расширением
    /// Генерируется уникальное имя файла 
    /// Возвращается полный путь к файлу
    /// Если временный каталог не существует, он создается
    /// </summary>
    /// <param name="extension">Расширение файла без ведущей точки</param>
    /// <returns>Путь к файлу</returns>
    public AbsPath GetTempFileName(string extension)
    {
      CheckNotDisposed();

      FileTools.ForceDirs(Dir);

      string Name = Guid.NewGuid().ToString("D");
      if (!String.IsNullOrEmpty(extension))
        Name += "." + extension;

      return new AbsPath(Dir, Name);
    }

    /// <summary>
    /// Получить имя временного файла с заданным именем (и расширением)
    /// Возвращается полный путь к файлу
    /// Если временный каталог не существует, он создается
    /// Создается вложенный временный каталог с уникальным именем, внутри которого будет размещен файл
    /// </summary>
    /// <param name="fileName">Имя файла (с расширением), но без указания пути</param>
    /// <returns>Путь к файлу</returns>
    public AbsPath GetFixedTempFileName(string fileName)
    {
      CheckNotDisposed();

      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");


      string SubDirName = Guid.NewGuid().ToString("D");
      AbsPath Dir2 = new AbsPath(Dir, SubDirName);
      FileTools.ForceDirs(Dir2);

      return new AbsPath(Dir2, fileName);
    }

    #endregion
  }

  #region Список шаблонов файлов

  /// <summary>
  /// Элемент списка шаблонов файлов
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct FileTemplateItem
  {
    #region Конструктор

    /// <summary>
    /// Создает шаблон
    /// </summary>
    /// <param name="template">Шаблон</param>
    /// <param name="recurse">true, если используется рекурсия по подкаталогам</param>
    public FileTemplateItem(string template, bool recurse)
    {
      _Template = template;
      _Recurse = recurse;
    }

    /// <summary>
    /// Создает шаблон.
    /// Рекурсия по подкаталогам не задается
    /// </summary>
    /// <param name="template">Шаблон</param>
    public FileTemplateItem(string template)
    {
      _Template = template;
      _Recurse = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Шаблон, который может содержать символы маски "*" и "?". Также может содержать необязательные подкаталоги,
    /// например "DOC\*.txt". Подкаталог не может содержать масок.
    /// </summary>
    public string Template { get { return _Template; } }
    private readonly string _Template;

    /// <summary>
    /// Если true, то предполагается рекурсивный поиск файлов по шаблону
    /// </summary>
    public bool Recurse { get { return _Recurse; } }
    private readonly bool _Recurse;

    /// <summary>
    /// Возвращает true, если шаблон содержит подкаталог
    /// </summary>
    public bool HasSubDir
    {
      get
      {
        return Template.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0;
      }
    }

    /// <summary>
    /// Возвращает шаблон и "(Recurse)"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Template + (Recurse ? " (Recurse)" : "");
    }

    #endregion

    #region Методы

    /// <summary>
    /// Если шаблон Template содержит подкаталоги, то возвращается путь, к которому добавляются подкаталоги, и
    /// шаблон без 
    /// </summary>
    /// <param name="rootDir"></param>
    /// <param name="newRootDir"></param>
    /// <param name="newTemplate"></param>
    public void GetNormalTemplate(AbsPath rootDir, out AbsPath newRootDir, out string newTemplate)
    {
      int p = Template.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
      if (p < 0)
      {
        newRootDir = rootDir;
        newTemplate = Template;
      }
      else
      {
        newRootDir = new AbsPath(rootDir, Template.Substring(0, p));
        newTemplate = Template.Substring(p + 1);
      }
    }

    #endregion
  }

  /// <summary>
  /// Список шаблонов файлов
  /// </summary>
  public class FileTemplateList : List<FileTemplateItem>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public FileTemplateList()
    {
    }

    /// <summary>
    /// Создает список с одним элементом
    /// </summary>
    /// <param name="template">Шаблон</param>
    /// <param name="recurse">true, если используется рекурсия по подкаталогам</param>
    public FileTemplateList(string template, bool recurse)
    {
      Add(template, recurse);
    }

    /// <summary>
    /// Создает список с одним элементом
    /// </summary>
    /// <param name="template">Шаблон. Рекурсия по подкаталогам не задается</param>
    public FileTemplateList(string template)
    {
      Add(template, false);
    }

    /// <summary>
    /// Создает список с несколькими шаблонами
    /// </summary>
    /// <param name="templates">Шаблоны</param>
    /// <param name="recurse">true, если используется рекурсия по подкаталогам</param>
    public FileTemplateList(string[] templates, bool recurse)
    {
      for (int i = 0; i < templates.Length; i++)
        Add(templates[i], recurse);
    }

    /// <summary>
    /// Создает список с несколькими шаблонами
    /// </summary>
    /// <param name="templates">Шаблоны. Рекурсия по подкаталогам не задается</param>
    public FileTemplateList(string[] templates)
    {
      for (int i = 0; i < templates.Length; i++)
        Add(templates[i], false);
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Добавляет шаблон в список
    /// </summary>
    /// <param name="template">Шаблон</param>
    /// <param name="recurse">true, если используется рекурсия по подкаталогам</param>
    public void Add(string template, bool recurse)
    {
      base.Add(new FileTemplateItem(template, recurse));
    }

    /// <summary>
    /// Добавляет шаблон в список
    /// </summary>
    /// <param name="template">Шаблон. Рекурсия по подкаталогам не задается</param>
    public void Add(string template)
    {
      base.Add(new FileTemplateItem(template));
    }

    /// <summary>
    /// Возвращает спиcок шаблонов через точку с запятой.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (Count == 0)
        return "{Empty}";
      else
      {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
          if (i > 0)
            sb.Append(";");
          sb.Append(this[i].ToString());
        }
        return sb.ToString();
      }
    }

    #endregion

    #region Получение списка файлов

    /// <summary>
    /// Добавляет несколько шаблонов в список
    /// </summary>
    /// <param name="templates">Шаблоны. Рекурсия по подкаталогам не задается</param>
    public void AddRange(string[] templates)
    {
      AddRange(templates, false);
    }

    /// <summary>
    /// Добавляет несколько шаблонов в список
    /// </summary>
    /// <param name="templates">Шаблоны</param>
    /// <param name="recurse">true, если используется рекурсия по подкаталогам</param>
    public void AddRange(string[] templates, bool recurse)
    {
      for (int i = 0; i < templates.Length; i++)
        Add(templates[i], recurse);
    }

    /// <summary>
    /// Добавляет несколько шаблонов в список
    /// </summary>
    /// <param name="templates">Шаблоны. Рекурсия по подкаталогам не задается</param>
    public void AddRange(ICollection<string> templates)
    {
      AddRange(templates, false);
    }

    /// <summary>
    /// Добавляет несколько шаблонов в список
    /// </summary>
    /// <param name="templates">Шаблоны</param>
    /// <param name="recurse">true, если используется рекурсия по подкаталогам</param>
    public void AddRange(ICollection<string> templates, bool recurse)
    {
      foreach (string Template in templates)
        Add(Template, recurse);
    }

    /// <summary>
    /// Получение списка относительных путей файлов, удовлетворяющих шаблонам
    /// Если файл входит больше, чем в два шаблона, повторы отбрасываются
    /// Список файлов сортируется по алфавиту
    /// </summary>
    /// <param name="root">Корневой каталог, относительно которого заданы шаблоны</param>
    /// <param name="splash">Необязательная экранная заставка</param>
    /// <returns></returns>
    public string[] GetRelFileNames(AbsPath root, ISimpleSplash splash)
    {
      if (root.IsEmpty)
        throw new ArgumentNullException("root", "Не задан базовый каталог для поиска");

      if (splash == null)
        splash = new DummySimpleSplash();

      if (!Directory.Exists(root.Path))
        throw new DirectoryNotFoundException("Каталог не существует: \"" + root + "\"");

      SortedDictionary<string, object> lst = new SortedDictionary<string, object>(); // Используем только ключи

      // Длина обрезки каталога
      int BaseLen = root.SlashedPath.Length;

      // Файлы могут входить сразу в несколько шаблонов
      for (int i = 0; i < Count; i++)
      {
        AbsPath NewRoot;
        string NewTemplate;
        this[i].GetNormalTemplate(root, out NewRoot, out NewTemplate);
        if (Directory.Exists(NewRoot.Path))
        {
          string[] a = Directory.GetFiles(NewRoot.Path, NewTemplate, this[i].Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
          for (int j = 0; j < a.Length; j++)
          {
            string s = a[j].Substring(BaseLen);
            if (!lst.ContainsKey(s))
              lst.Add(s, null);
          }
        }
      }

      string[] a2 = new string[lst.Count];
      lst.Keys.CopyTo(a2, 0);

      Array.Sort<string>(a2);

      return a2;
    }

    /// <summary>
    /// Получение списка абсолютных путей файлов, удовлетворяющих шаблонам
    /// Если файл входит больше, чем в два шаблона, повторы отбрасываются
    /// Список файлов сортируется по алфавиту
    /// </summary>
    /// <param name="root">Корневой каталог, относительно которого заданы шаблоны</param>
    /// <param name="splash">Необязательная экранная заставка</param>
    /// <returns></returns>
    public string[] GetAbsFileNames(AbsPath root, ISimpleSplash splash)
    {
      if (root.IsEmpty)
        throw new ArgumentNullException("root", "Не задан базовый каталог для поиска");

      if (splash == null)
        splash = new DummySimpleSplash();

      if (!Directory.Exists(root.Path))
        throw new DirectoryNotFoundException("Каталог не существует: \"" + root + "\"");

      SortedDictionary<string, object> lst = new SortedDictionary<string, object>(); // Используем только ключи

      // Файлы могут входить сразу в несколько шаблонов
      for (int i = 0; i < Count; i++)
      {
        AbsPath NewRoot;
        string NewTemplate;
        this[i].GetNormalTemplate(root, out NewRoot, out NewTemplate);
        if (Directory.Exists(NewRoot.Path))
        {
          string[] a = Directory.GetFiles(NewRoot.Path, NewTemplate, this[i].Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
          for (int j = 0; j < a.Length; j++)
          {
            if (!lst.ContainsKey(a[j]))
              lst.Add(a[j], null);
          }
        }
      }

      string[] a2 = new string[lst.Count];
      lst.Keys.CopyTo(a2, 0);

      Array.Sort<string>(a2);

      return a2;
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// Подавление подстановки каталогов (Redirection) в 64-разрядных версиях windows.
  /// Использование:
  /// using(new FileRedirectionSupressor() )
  /// { 
  ///   // Код, в котором используются истинные имена без подстановок
  /// }
  /// Используются функции Wow64DisableWow64FsRedirection() и Wow64RevertWow64FsRedirection
  /// Если операционная система не поддерживает redirection, код в блоке using выполняется без дополнительных действий
  /// </summary>
  public sealed class FileRedirectionSupressor : SimpleDisposableObject
  {
    #region Функции Widnows

    private static class WindowsNative
    {
      [DllImport("kernel32.dll")]
      internal extern static Int32 Wow64DisableWow64FsRedirection(out IntPtr oldValue);

      [DllImport("kernel32.dll")]
      internal extern static Int32 Wow64RevertWow64FsRedirection(IntPtr oldValue);
    }

    #endregion

    #region Конструктор и Dispose

    /// <summary>
    /// Создает объект.
    /// Если операционная система не поддерживает redirection, ничего не делается.
    /// </summary>
    public FileRedirectionSupressor()
    {
      if (OSSupported)
      {
        if (WindowsNative.Wow64DisableWow64FsRedirection(out OldValue) != 0)
          _Active = true;
      }
    }

    /// <summary>
    /// Отключает подавление, если оно было включено.
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (_Active)
      {
        _Active = false;
        WindowsNative.Wow64RevertWow64FsRedirection(OldValue);
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если redirection был включен в конструкторе
    /// </summary>
    public bool Active { get { return _Active; } }
    private bool _Active;

    private IntPtr OldValue;

    #endregion

    #region Статическое свойство и метод

    /// <summary>
    /// Свойство возвращает true, если операционная система имеет функции для redirection
    /// </summary>
    public static bool OSSupported
    {
      get
      {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
          //if (IntPtr.Size==8)
          //  return true;
          if (Environment.OSVersion.Version.Major > 6)
            return true;
          if (Environment.OSVersion.Version.Major < 6)
            return false;

          // Проверка для WinXP 64-bit - должно возвращать true
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Свойство возвращает true, если есть redirection 
    /// </summary>
    public static bool TestRedirection
    {
      get
      {
        bool Res;
        using (FileRedirectionSupressor Obj = new FileRedirectionSupressor())
        {
          Res = Obj.Active;
        }
        return Res;
      }
    }

    #endregion
  }
}