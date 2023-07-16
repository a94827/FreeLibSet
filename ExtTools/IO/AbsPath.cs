// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace FreeLibSet.IO
{
  /*
   * AbsPath.cs может использоваться не только в библиотеке ExtTools.cs, 
   * но и самостоятельно, например, в загрузчиках программ.
   * Поэтому класс AbsPath не может зависеть от других классов, например, FileTools.
   * Также он не может ссылаться на класс RelPath (наоборот можно).
   */

  /// <summary>
  /// Абсолютный путь к файлу или каталогу.
  /// Реализует методы для манипуляции с путями, заявленные в System.IO.Path.
  /// Не выполняет никаких действий с реальными файлами и каталогами.
  /// </summary>
  [Serializable]
  public struct AbsPath : IEquatable<AbsPath>
  {
    #region Конструкторы

    static AbsPath()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
        case PlatformID.Win32S:
        case PlatformID.WinCE:
          ComparisonType = StringComparison.OrdinalIgnoreCase;
          Comparer = StringComparer.OrdinalIgnoreCase;
          break;
        default:
          ComparisonType = StringComparison.Ordinal;
          Comparer = StringComparer.Ordinal;
          break;
      }

    }

    /// <summary>
    /// Создает объект, содержащий абсолютный путь к файлу или каталогу.
    /// Если аргумент заканчивается на слэш, то он удаляется.
    /// Допускается задание пути в Uri-формате "file:///"
    /// </summary>
    /// <param name="s">Путь, который нужно преобразовать</param>
    public AbsPath(string s)
    {
      try
      {
        // 11.12.2014 
        // Путь может быть заключен в кавычки
        string s2 = RemoveQuotes(s);
        s2 = RemoveDirNameSlash(s2);
        if (String.IsNullOrEmpty(s2))
          _Path = null;
        else
        {
          if (s2.StartsWith("file:///", StringComparison.OrdinalIgnoreCase)) // 28.11.2014
          {
            // TODO: 16.06.2017. Хорошо бы разобраться, как это правильно должно работать, а не методом Тыка

            switch (Environment.OSVersion.Platform)
            {
              case PlatformID.Win32NT:
              case PlatformID.Win32Windows:
              case PlatformID.Win32S:
              case PlatformID.WinCE:
                // Здесь, после трех "///" идет обычный путь, начиная с буквы диска
                s2 = s2.Substring(8).Replace('/', System.IO.Path.DirectorySeparatorChar);
                break;
              case PlatformID.Unix:
                // 16.06.2017
                // Здесь последняя из трех "///" задает корневую папку
                s2 = s2.Substring(7);
                break;
              default:
                throw new NotImplementedException();
            }
          }
#if XXXX
          for (int i = 0; i < System.IO.Path.InvalidPathChars.Length; i++)
          { 
            int p=s2.IndexOf(System.IO.Path.InvalidPathChars[i]);
            if (p>=0)
              throw new ParsingException("Недопустимый символ \""+System.IO.Path.InvalidPathChars[i]+"\" в позиции "+(p+1).ToString());
          }
#endif
          // 17.05.2016
          if (StartsWithUriScheme(s2))
            _Path = s2;
          else
          {
            s2 = System.IO.Path.GetFullPath(s2);
            _Path = s2;
          }
        }
      }
      catch (Exception e)
      {
        throw new ArgumentException("Не удалось преобразовать \"" + s + "\" в полный путь. " + e.Message, e);
      }
    }

    internal static string RemoveQuotes(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      if (s.Length < 2)
        return s;

      if (s[0] == '\"' && s[s.Length - 1] == '\"')
        return s.Substring(1, s.Length - 2);

      return s;
    }

    internal static string RemoveDirNameSlash(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      if (Environment.OSVersion.Platform != PlatformID.Unix)
      {
        if (s.Length == 3 && s.Substring(1, 2) == ":\\")
          return s;
      }

      if (s[s.Length - 1] == System.IO.Path.DirectorySeparatorChar)
        return s.Substring(0, s.Length - 1);

      return s;
    }

    /// <summary>
    /// Возвращает true, если имя начинается с протокола, например, "smb://"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static bool StartsWithUriScheme(string s)
    {
      int p1 = s.IndexOf("://", StringComparison.Ordinal);
      if (p1 < 0)
        return false;
      int p2 = s.IndexOf('/');
      if (p2 < p1)
        return false; // какая-то бяка
      int p3 = s.IndexOf(':');
      if (p3 < p1)
        return false; // какая-то бяка
      return true;
    }


    /// <summary>
    /// Создает путь на основе базового, с произвольным числом подкаталогов.
    /// </summary>
    /// <param name="basePath">Базовый каталог</param>
    /// <param name="subNames">Дочерние подкаталоги. Могут быть пустые строки, которые пропускаются без выброса исключения.</param>
    public AbsPath(AbsPath basePath, params string[] subNames)
    {
      AbsPath p1 = basePath;
      for (int i = 0; i < subNames.Length; i++)
      {
        /*
        #if DEBUG
                if (String.IsNullOrEmpty(SubNames[i]))
                  throw new ArgumentException("Пустая строка в подкаталоге с индексом " + i.ToString(), "SubNames");
                if (SubNames[i].IndexOf(System.IO.Path.DirectorySeparatorChar)>=0)
                  throw new ArgumentException("В массиве подкаталогов не должно быть символа-разделителя каталогов", "SubNames");
        #endif
                p1 = p1 + SubNames[i];
         * */

        // 11.12.2014
        // Убираем кавычки
        string s2 = RemoveQuotes(subNames[i]);

        // 10.09.2014
        // Разрешаем наличие пустых подкаталогов и символов разделителей
        if (String.IsNullOrEmpty(s2))
          continue;

        if (s2.IndexOf(System.IO.Path.DirectorySeparatorChar) >= 0)
        {
          string[] a = s2.Split(System.IO.Path.DirectorySeparatorChar);
          for (int j = 0; j < a.Length; j++)
          {
            if (String.IsNullOrEmpty(a[j]))
              continue;
            p1 = p1 + a[j];
          }
        }
        else
          p1 = p1 + s2;
      }
      _Path = p1._Path;
    }

    /// <summary>
    /// Создает объект <see cref="AbsPath"/>.
    /// Если задана пустая строка или возникает ошибка преобразования, возвращается <see cref="AbsPath.Empty"/>.
    /// См. описание соответствующего конструктора <see cref="AbsPath"/>.
    /// </summary>
    /// <param name="s">Путь, который нужно преобразовать</param>
    /// <returns>Объект <see cref="AbsPath"/></returns>
    [DebuggerStepThrough]
    public static AbsPath Create(string s)
    {
      if (String.IsNullOrEmpty(s))
        return AbsPath.Empty;
      try
      {
        return new AbsPath(s);
      }
      catch
      {
        return AbsPath.Empty;
      }
    }


    /// <summary>
    /// Создает объект <see cref="AbsPath"/>. 
    /// Если <paramref name="basePath"/>.IsEmpty=true или возникает ошибка преобразования,
    /// возвращается <see cref="AbsPath.Empty"/>.
    /// См. описание соответствующего конструктора <see cref="AbsPath"/>.
    /// </summary>
    /// <param name="basePath">Базовый каталог</param>
    /// <param name="subNames">Дочерние подкаталоги</param>
    /// <returns>Объект <see cref="AbsPath"/></returns>
    [DebuggerStepThrough]
    public static AbsPath Create(AbsPath basePath, params string[] subNames)
    {
      if (basePath.IsEmpty)
        return AbsPath.Empty;
      try
      {
        return new AbsPath(basePath, subNames);
      }
      catch
      {
        return AbsPath.Empty;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Полный путь к каталогу или файлу.
    /// Путь (обычно) не заканчивается обратным слэшем и хранится в форме, пригодной
    /// для использования методами классов в System.IO.
    /// Задается в конструкторе.
    /// </summary>
    public string Path { get { return _Path ?? String.Empty; /* 29.06.2023 */ } }
    private readonly string _Path;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована.
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Path); } }

    /// <summary>
    /// Возвращает свойство <see cref="Path"/>.
    /// Если <see cref="IsEmpty"/>=true, то возвращается пустая строка.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Path;
    }

    /// <summary>
    /// Возвращает путь, заканчивающийся обратным слэшем.
    /// Используется, когда объект хранит каталог и нужно получить строку с именем
    /// находящегося в каталоге файла.
    /// </summary>
    public string SlashedPath
    {
      get
      {
        if (String.IsNullOrEmpty(_Path))
          return String.Empty;
        if (_Path[_Path.Length - 1] == System.IO.Path.DirectorySeparatorChar)
          return _Path;
        else
          return _Path + System.IO.Path.DirectorySeparatorChar; // Исправлена бяка 17.03.2017
      }
    }

    /// <summary>
    /// Возвращает идентификатор ресурса "file://".
    /// Для пустого экземпляра (<see cref="IsEmpty"/>=true) возвращает null.
    /// </summary>
    public Uri Uri
    {
      get
      {
        if (IsEmpty)
          return null; // 29.06.2023
        else
          // ??? Может быть нужна замена пробелов на %20 ???
          return new Uri("file://" + Path);
      }
    }

    /// <summary>
    /// Путь в виде строки идентификатора ресурса "file://".
    /// В отличие от основного свойства <see cref="Uri"/>, для пустого пути (<see cref="Empty"/>=true) возвращает <see cref="String.Empty"/>, а не null.
    /// Используется метод <see cref="System.Uri.ToString()"/>.
    /// </summary>
    public string UriString
    {
      get
      {
        if (IsEmpty)
          return String.Empty;
        else
          return new Uri("file://" + Path).ToString();
      }
    }

    /// <summary>
    /// Возвращает путь, заключенный в кавычки (для передачи в качестве аргумента командной строки внешним программам).
    /// </summary>
    public string QuotedPath
    {
      get
      {
        if (IsEmpty)
          return String.Empty;

        // Все просто. Внутри имени не может быть кавычек
        return "\"" + Path + "\"";
      }
    }

    #endregion

    #region Добавление подкаталога

    /// <summary>
    /// Добавление относительного пути.
    /// Использует функцию <see cref="System.IO.Path.Combine(string, string)"/>.
    /// </summary>
    /// <param name="basePath">Исходный путь. Не может быть пустым</param>
    /// <param name="subDir">Подкаталог. Если задана пустая строка, возвращается <paramref name="basePath"/>.</param>
    /// <returns>Новый путь</returns>
    public static AbsPath operator +(AbsPath basePath, string subDir)
    {
      if (basePath.IsEmpty)
        //return new AbsPath(SubDir);
        throw new ArgumentException("Базовый каталог пустой", "basePath");

      if (String.IsNullOrEmpty(subDir))
        return basePath;

      return new AbsPath(System.IO.Path.Combine(basePath.Path, subDir));
    }

#if XXX
    /// <summary>
    /// Определение разностного пути.
    /// Возвращает относительный путь для DeepPath относительно ParentPath
    /// !!! Ограниченная реализация !!!
    /// </summary>
    /// <param name="DeepPath">Путь вложенного уровня</param>
    /// <param name="BasePath">Путь верхнего уровня</param>
    /// <returns>Разностный путь</returns>
    public static string operator -(AbsPath DeepPath, AbsPath BasePath)
    { 
      return 
    }
#endif

    #endregion

    #region Родительский и корневой каталоги

    /// <summary>
    /// Родительский каталог ("..").
    /// Использует метод <see cref="System.IO.Path.GetDirectoryName(string)"/>.
    /// </summary>
    public AbsPath ParentDir
    {
      get
      {
        if (IsEmpty)
          return AbsPath.Empty;
        else
        {
          switch (Environment.OSVersion.Platform)
          {
            case PlatformID.Win32NT:
            case PlatformID.Win32Windows:
            case PlatformID.Win32S: // на всякий случай
            case PlatformID.WinCE:
              // Используем стандартную функцию для извлечения
              return new AbsPath(System.IO.Path.GetDirectoryName(Path));
            default:
              // 15.06.2017
              // В Linux'е оно работает неправильно.
              // Например, путь
              //  /home/a94827/.local/share/Accoo2ClientNet4/miac-tmn.ru_8089/Accoo2Client.dll
              // превращается в 
              //  /home/a94827/accoo2/home/a94827/.local/share/Accoo2ClientNet4/miac-tmn.ru_8089
              // То есть, в начале дописалось /home/a94827/accoo2/ непонятно почему
              // Делаем руками
              int p = Path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
              if (p > 0) // путь и так начинается с "/", поэтому не >=0
                return new AbsPath(Path.Substring(0, p)); // без последней палочки
              else
                return AbsPath.Empty;
          }
        }
      }
    }

    /// <summary>
    /// Возвращает корневой каталог.
    /// Использует метод <see cref="System.IO.Path.GetPathRoot(string)"/>.
    /// Если <see cref="IsEmpty"/>=true, то возвращается пустой путь без выброса исключения.
    /// </summary>
    public AbsPath RootDir
    {
      get
      {
        if (IsEmpty)
          return AbsPath.Empty;
        else
          return new AbsPath(System.IO.Path.GetPathRoot(Path));
      }
    }

    #endregion

    #region Извлечение частей имени

    /// <summary>
    /// Получить имя файла или каталога самого вложенного уровня (последнюю часть пути после разделителя)
    /// (вызов <see cref="System.IO.Path.GetFileName(string)"/>).
    /// </summary>
    public string FileName
    {
      get
      {
        return System.IO.Path.GetFileName(Path);
      }
    }

    /// <summary>
    /// Получить имя файла или каталога самого вложенного уровня без расширения
    /// (вызов <see cref="System.IO.Path.GetFileNameWithoutExtension(string)"/>)
    /// </summary>
    public string FileNameWithoutExtension
    {
      get
      {
        return System.IO.Path.GetFileNameWithoutExtension(Path);
      }
    }

    /// <summary>
    /// Расширение имени файла или каталога самого вложенного уровня, включая точку
    /// (вызов <see cref="System.IO.Path.GetExtension(string)"/>)
    /// </summary>
    public string Extension
    {
      get
      {
        return System.IO.Path.GetExtension(Path);
      }
    }

    /// <summary>
    /// Изменить расширение.
    /// (вызов <see cref="System.IO.Path.ChangeExtension(string, string)"/>)
    /// Создает новый экземпляр <see cref="AbsPath"/>.
    /// Если <see cref="IsEmpty"/>=true, выбрасывается исключение.
    /// </summary>
    /// <param name="newExtension">Новое расширение, включающее ведущую точку</param>
    /// <returns>Путь к файлу с новым расширением</returns>
    public AbsPath ChangeExtension(string newExtension)
    {
      if (IsEmpty)
        throw new InvalidOperationException();
      return new AbsPath(System.IO.Path.ChangeExtension(Path, newExtension));
    }

    #endregion

    #region Сравнение путей

    /// <summary>
    /// Учет регистра при сравнении путей. Содержит Ordinal (Unix), или OrdinalIgnoreCase (Windows)
    /// </summary>
    internal static readonly StringComparison ComparisonType;

    /// <summary>
    /// Компаратор для сравнения строк
    /// </summary>
    internal static readonly StringComparer Comparer;

    /// <summary>
    /// Возвращает true, если два пути являются одинаковыми (свойства <see cref="Path"/> совпадают).
    /// Регистр символов учитывается или игнорируется, в зависимости от платформы.
    /// </summary>
    /// <param name="path1">Первый сравниваемый путь</param>
    /// <param name="path2">Второй сравниваемый путь</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(AbsPath path1, AbsPath path2)
    {
      if (path1.IsEmpty && path2.IsEmpty)
        return true;
      if (path1.IsEmpty || path2.IsEmpty)
        return false;

      return String.Equals(path1.Path, path2.Path, ComparisonType);
    }

    /// <summary>
    /// Возвращает false, если два пути являются одинаковыми (свойства <see cref="Path"/> совпадают).
    /// Регистр символов учитывается или игнорируется, в зависимости от платформы.
    /// </summary>
    /// <param name="path1">Первый сравниваемый путь</param>
    /// <param name="path2">Второй сравниваемый путь</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(AbsPath path1, AbsPath path2)
    {
      return !(path1 == path2);
    }

    /// <summary>
    /// Сравнение с другим <see cref="AbsPath"/>
    /// </summary>
    /// <param name="other">Второй сравниваемый путь</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(AbsPath other)
    {
      return this == other;
    }

    /// <summary>
    /// Сравнение с другим <see cref="AbsPath"/>
    /// </summary>
    /// <param name="other">Второй сравниваемый путь</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object other)
    {
      if (!(other is AbsPath))
        return false;

      return this == (AbsPath)other;
    }

    /// <summary>
    /// Поддержка коллекций.
    /// Возвращает хэш-код для пути.
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      if (IsEmpty)
        return 0;
      else
      {
        //return FPath.GetHashCode();
        // Так нельзя, т.к. мы сравниваем строки без учета регистра
        //return _Path.Length;

        // 29.06.2023
        return Comparer.GetHashCode(_Path);
      }
    }

    /// <summary>
    /// Возвращает true, если текущий путь равен <paramref name="path"/> или является вложенным по отношению к <paramref name="path"/>.
    /// Например, если <paramref name="path"/>=C:/Windows, то для текущего пути c:/WINDOWS и c:/Windows/temp/123.txt будет возвращено true.
    /// Если текущий каталог начинается так же, но не относится к <paramref name="path"/>, то возвращается false, например для
    /// c:/windows и c:/Windows2/temp/123.txt. Этим метод отличается от простого вызова метода <see cref="String.StartsWith(string)"/>.
    /// Регистр символов учитывается или не учитывается, в зависимости от платформы.
    /// Если текущий путь пустой (<see cref="IsEmpty"/>=true), возвращается false.
    /// Eсли <paramref name="path"/>.IsEmpty=true, то возвращается true.
    /// </summary>
    /// <param name="path">Проверяемый (родительский) путь</param>
    /// <returns>Вхождение проверяемого пути в текущий</returns>
    public bool StartsWith(AbsPath path)
    {
      if (this.IsEmpty)
        return false;

      if (path.IsEmpty)
        return true;

      if (String.Equals(this.Path, path.Path, ComparisonType))
        return true; // текущий каталог
      if (this.Path.StartsWith(path.SlashedPath, ComparisonType))
        return true; // вложенный путь

      return false;
    }

    /// <summary>
    /// Возвращает true, если каталог заканчивается заданными частями пути.
    /// Регистр учитывается или не учитывается, в зависимости от платформы.
    /// Например, в Windows, если текущий путь равен
    /// C:\Windows\System32\XPSViewer
    /// и вызвать EndsWith("system32", "XPSViewer"), то будет возвращено true.
    /// Если текущий объект пустой (<see cref="IsEmpty"/>=true), то возвращается false.
    /// Если <paramref name="relParts"/> - пустой список, возвращается true.
    /// </summary>
    /// <param name="relParts">Конечные части путей</param>
    /// <returns>true, если конечная часть пути совпадает</returns>
    public bool EndsWith(params string[] relParts)
    {
      if (IsEmpty)
        return false;

      if (relParts.Length == 0)
        return true; // 30.06.2023

      string relPath = String.Join(new string(System.IO.Path.DirectorySeparatorChar, 1), relParts);

      relPath = System.IO.Path.DirectorySeparatorChar + relPath; // ведущий слэш

      if (relPath[relPath.Length - 1] == System.IO.Path.DirectorySeparatorChar)
        relPath = relPath.Substring(0, relPath.Length - 1);

      return _Path.EndsWith(relPath, ComparisonType);
    }

    /// <summary>
    /// Возвращает true, если каталог заканчивается заданными частями пути.
    /// Регистр не учитывается, независимо от платформы.
    /// Для Windows метод совпадает с методом <see cref="EndsWith(string[])"/>.
    /// </summary>
    /// <param name="relParts">Конечные части путей</param>
    /// <returns>true, если конечная часть пути совпадает</returns>
    public bool EndsWithIgnoreCase(params string[] relParts)
    {
      if (IsEmpty)
        return false;

      if (relParts.Length == 0)
        return true; // 30.06.2023

      string relPath = String.Join(new string(System.IO.Path.DirectorySeparatorChar, 1), relParts);

      relPath = System.IO.Path.DirectorySeparatorChar + relPath; // ведущий слэш

      if (relPath[relPath.Length - 1] == System.IO.Path.DirectorySeparatorChar)
        relPath = relPath.Substring(0, relPath.Length - 1);

      return _Path.EndsWith(relPath, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Определение типа пути

    /// <summary>
    /// Возвращает true, если путь начинается с @"\\" (Windows)
    /// </summary>
    [Obsolete("Использование свойства не оправдано. Например, в Windows оно не позволяет отличить подключенные сетевые диски от обычных", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsNetwork
    {
      get
      {
        if (IsEmpty)
          return false;
        if (System.IO.Path.DirectorySeparatorChar == '\\')
        {
          return _Path.StartsWith("\\\\", StringComparison.Ordinal);
        }
        else
        {
          if (_Path.StartsWith("smb://", StringComparison.Ordinal))
            return true;
          // !!! Для Unix - не знаю других вариантов, кроме Самбы
        }
        return false;
      }
    }

    #endregion

    #region Статические экземпляры

    /// <summary>
    /// Пустая структура
    /// </summary>
    public static readonly AbsPath Empty = new AbsPath(String.Empty);

    #endregion
  }
}
