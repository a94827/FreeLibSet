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
   * Также нельзя использовать локализацию с помощью ресурсов.
   * 
   * Также в этом файле находится структура RelPath, так как AbsPath и RelPath зависят друг от друга
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
      // Объявлено в Net Framework 4
      const System.Environment.SpecialFolder UserProfileSpecialFolder = (System.Environment.SpecialFolder)0x28;

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
        case PlatformID.Win32S:
        case PlatformID.WinCE:
          ComparisonType = StringComparison.OrdinalIgnoreCase;
          Comparer = StringComparer.OrdinalIgnoreCase;
          IsWindows = true;
          UnixHomePath = String.Empty;
          break;
        default:
          ComparisonType = StringComparison.Ordinal;
          Comparer = StringComparer.Ordinal;
          IsWindows = false;
          //UnixHomePath = Environment.GetEnvironmentVariable("HOME");
          UnixHomePath = Environment.GetFolderPath(UserProfileSpecialFolder); // 21.11.2023
          break;
      }

    }

    /// <summary>
    /// Создает объект, содержащий абсолютный путь к файлу или каталогу.
    /// Если аргумент заканчивается на слэш, то он удаляется.
    /// Допускается задание пути в Uri-формате "file:///".
    /// Если в Linux путь начинается на "~", то символ заменяется на путь к домашней папке.
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

            if (IsWindows)
              // Здесь, после трех "///" идет обычный путь, начиная с буквы диска
              s2 = s2.Substring(8).Replace('/', System.IO.Path.DirectorySeparatorChar);
            else
              // 16.06.2017
              // Здесь последняя из трех "///" задает корневую папку
              s2 = s2.Substring(7);
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
            if (s2[0] == '~' && UnixHomePath.Length > 0)
              s2 = UnixHomePath + s2.Substring(1);
            string s3 = System.IO.Path.GetFullPath(s2);
            _Path = s3;
          }
        }
      }
      catch (Exception e)
      {
        throw new ArgumentException(String.Format("Cannot convert \"{0}\" to the full path. {1}", s, e.Message), e);
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

      if (IsWindows && s.Length == 3 && s.Substring(1, 2) == @":\")
        return s;

      if (s.Length > 1 /* 04.09.2023 */ && s[s.Length - 1] == System.IO.Path.DirectorySeparatorChar)
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

    private static readonly bool IsWindows;

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
        throw new ArgumentException("Base directory is empty", "basePath");

      if (String.IsNullOrEmpty(subDir))
        return basePath;

      return new AbsPath(System.IO.Path.Combine(basePath.Path, subDir));
    }

    /// <summary>
    /// Определение разностного пути.
    /// Возвращает относительный путь для <paramref name="childPath"/> относительно <paramref name="basePath"/>.
    /// Если <paramref name="basePath"/> не задан, возвращается полный путь к <paramref name="childPath"/>.
    /// Если <paramref name="childPath"/> не задан, возвращается пустой путь.
    /// Если оба каталога совпадают, возвращается путь "."
    /// Если <paramref name="childPath"/> является прямым потомком от <paramref name="basePath"/>, возвращается <see cref="AbsPath.FileName"/>.
    /// Если он является "внуком" и т.п., возвращается относительный путь с подкаталогами.
    /// Если <paramref name="childPath"/> не является подкаталогом относительно <paramref name="basePath"/>, то
    /// возвращается либо путь с ".." (возможно, несколькими), либо абсолютный путь к <paramref name="childPath"/>,
    /// в зависимости от наличия общего корня.
    /// </summary>
    /// <param name="childPath">Путь вложенного уровня</param>
    /// <param name="basePath">Путь верхнего уровня</param>
    /// <returns>Разностный путь</returns>
    public static RelPath operator -(AbsPath childPath, AbsPath basePath)
    {
      if (childPath.IsEmpty)
        return new RelPath(String.Empty);
      if (basePath.IsEmpty)
        return new RelPath(childPath.Path);

      if (childPath == basePath)
        return new RelPath("."); // эта проверка самая простая

      if (childPath.Path.StartsWith(basePath.SlashedPath, ComparisonType))
        return new RelPath(childPath.Path.Substring(basePath.SlashedPath.Length)); // Наиболее частый случай. Уже проверено, что пути не совпадают

      AbsPath root = childPath.RootDir;
      if (root == childPath)
        return new RelPath(childPath.Path); // задан корень диска

      if (root != basePath.RootDir)
        return new RelPath(childPath.Path); // разные корни

      string s1 = childPath.Path.Substring(root.SlashedPath.Length);
      string s2 = basePath.Path.Substring(root.SlashedPath.Length);

      string[] a1 = s1.Split(System.IO.Path.DirectorySeparatorChar);
      string[] a2 = s2.Split(System.IO.Path.DirectorySeparatorChar);

      // Поиск общих элементов
      int n = Math.Min(a1.Length, a2.Length);
      int nCommon = 0;
      for (int i = 0; i < n; i++)
      {
        if (String.Equals(a1[i], a2[i], ComparisonType))
          nCommon++;
        else
          break;
      }

      // Возврат к родительским каталогам
      List<string> lst = new List<string>();
      for (int i = nCommon; i < a2.Length; i++)
        lst.Add("..");

      // Добавление путей
      for (int i = nCommon; i < a1.Length; i++)
        lst.Add(a1[i]);

      if (lst.Count == 1)
        return new RelPath(lst[0]);
      else
        return new RelPath(String.Join(new string(System.IO.Path.DirectorySeparatorChar, 1), lst.ToArray()));
    }


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
          if (IsWindows)
          {
            // Используем стандартную функцию для извлечения
            return new AbsPath(System.IO.Path.GetDirectoryName(Path));
          }
          else
          {
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
        {
          string s = System.IO.Path.GetPathRoot(Path);
          //Console.WriteLine ("GetPathRoot=" + s);
          return new AbsPath(s);
        }
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
    /// См. описание метода <see cref="System.IO.Path.ChangeExtension(string, string)"/>.
    /// Создает новый экземпляр <see cref="AbsPath"/>.
    /// Если <see cref="IsEmpty"/>=true, выбрасывается исключение.
    /// Для удаления расширения используйте <paramref name="newExtension"/>=null, а не пустую строку
    /// </summary>
    /// <param name="newExtension">Новое расширение, включающее ведущую точку</param>
    /// <returns>Путь к файлу с новым расширением</returns>
    public AbsPath ChangeExtension(string newExtension)
    {
      if (IsEmpty)
        throw new InvalidOperationException();
      return new AbsPath(System.IO.Path.ChangeExtension(Path, newExtension));
    }

    /// <summary>
    /// Метод возвращает true, если расширение имени файла (или каталога) (свойство <see cref="Extension"/>), совпадает с запрашиваемым.
    /// Также проверяются составные расширения. Например, если имя файла "book.fb2.zip", то возвращается true и для ".zip" и для ".fb2.zip".
    /// При сравнении регистр символов учитывается или не учитывается, в зависимости от операционной системы.
    /// Если текущий путь пустой (<see cref="IsEmpty"/>=true), то возвращается false.
    /// Если <paramref name="extension"/> - пустая строка, то проверяется, что файл не имеет расширения.
    /// Не разрешается использования подстановочных символов "*" и "?".
    /// </summary>
    /// <param name="extension">Проверяемое расширение. Если непустая строка, то должно начинаться с точки</param>
    /// <returns>true, если расширение совпадает</returns>
    public bool ContainsExtension(string extension)
    {
      if (extension == null)
        extension = String.Empty;
      if (extension.Length > 0)
      {
        if (extension[0] != '.')
          throw new ArgumentException("File extension must start with a period", "extension");
        if (extension.Length == 1)
          throw new ArgumentException("File extension must be given. To check the absent extension use the String.Empty, not a \".\" string", "extension");
      }

      if (IsEmpty)
        return false;

      if (extension.Length == 0)
        return this.Extension.Length == 0;
      else
      {
        string fileName = this.FileName;
        if (fileName.Length == extension.Length)
          return false; // имя файла может начинаться с точки. Если имя файла ".xyz", то это не считается расширением.
        return fileName.EndsWith(extension, ComparisonType);
      }
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
    /// Под Linux содержит путь к папке "~" без завершающего символа "/".
    /// Для других ОС - пустая строка
    /// </summary>
    internal static readonly string UnixHomePath;

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

    #region Split() / Join()

    /// <summary>
    /// Разбивает путь на отдельные компоненты по символу-разделителю <see cref="System.IO.Path.DirectorySeparatorChar"/>.
    /// Первым элементом массива является корневой путь <see cref="RootDir"/>, он может содержать неубранные разделители.
    /// Из полученных частей можно выполнить обратную сборку методом <see cref="Join(string[])"/>. Сборку можно выполнить не из полного
    /// массива, а из произвольного количества элементов, начиная с нулевого.
    /// Для пустого объекта (<see cref="IsEmpty"/>=true) возвращает пустой массив строк.
    /// </summary>
    /// <returns></returns>
    public string[] Split()
    {
      if (IsEmpty)
        return new string[0]; // нельзя использовать DataTools.EmptySrings, чтобы не добавлять ссылку на файл

      AbsPath r = RootDir;
      if (r == this)
        return new string[1] { r.Path };

      string s2 = _Path.Substring(r.SlashedPath.Length);

#if XXX
      string[] a1 = s2.Split(System.IO.Path.DirectorySeparatorChar);
      string[] res = new string[a1.Length + 1];
      res[0] = r.Path;
      a1.CopyTo(res, 1);
#else
      int cntSeps = 0;
      for (int i = 0; i < s2.Length; i++)
      {
        if (s2[i] == System.IO.Path.DirectorySeparatorChar)
          cntSeps++;
      }

      string[] res = new string[cntSeps + 2];
      res[0] = r.Path;
      int currPos = 0;
      int resIndex = 1;
      for (int i = 0; i < s2.Length; i++)
      {
        if (s2[i] == System.IO.Path.DirectorySeparatorChar)
        {
          res[resIndex] = s2.Substring(currPos, i - currPos);
          resIndex++;
          currPos = i + 1;
        }
      }
      // Последний элемент
      res[resIndex] = s2.Substring(currPos);

#endif
      return res;
    }

    /// <summary>
    /// Собирает путь из строкового массива компонента.
    /// Элемент <paramref name="parts"/>[0] задает корневой каталог, остальные элементы - подкаталоги.
    /// Если передан пустой массив или null, возвращается <see cref="AbsPath.Empty"/>.
    /// </summary>
    /// <param name="parts"></param>
    /// <returns>Собранный путь</returns>
    public static AbsPath Join(string[] parts)
    {
      if (parts == null)
        return AbsPath.Empty;
      switch (parts.Length)
      {
        case 0:
          return AbsPath.Empty;
        case 1:
          return new AbsPath(parts[0]);
        default:
          StringBuilder sb = new StringBuilder();
          sb.Append(parts[0]);
          for (int i = 1; i < parts.Length; i++)
          {
            if (sb[sb.Length - 1] != System.IO.Path.DirectorySeparatorChar)
              sb.Append(System.IO.Path.DirectorySeparatorChar);
            sb.Append(parts[i]);
          }
          return new AbsPath(sb.ToString());
      }
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Получение общей части для двух путей. Например, если <paramref name="path1"/>="C:\AAA\BBB\CCC", а <paramref name="path2"/>="C:\AAA\BBB\DDD",
    /// то возвращается "C:\AAA\BBB".
    /// Если нет общей части пути, то возвращается <see cref="AbsPath.Empty"/>.
    /// Если один или оба пути не заданы, то возвращается <see cref="AbsPath.Empty"/>.
    /// Порядок аргументов не имеет значения.
    /// </summary>
    /// <param name="path1">Первый путь</param>
    /// <param name="path2">Второй путь</param>
    /// <returns>Общий путь</returns>
    public static AbsPath operator &(AbsPath path1, AbsPath path2)
    {
      if (path1.IsEmpty || path2.IsEmpty)
        return AbsPath.Empty;

      if (path1 == path2)
        return path1;

      string[] a1 = path1.Split();
      string[] a2 = path2.Split();
      int n = Math.Min(a1.Length, a2.Length);
      int cntCommon = 0;
      for (int i = 0; i < n; i++)
      {
        if (String.Equals(a1[i], a2[i], ComparisonType))
          cntCommon++;
        else
          break;
      }

      if (cntCommon == 0)
        return AbsPath.Empty;

      string[] aRes = new string[cntCommon];
      Array.Copy(a1, 0, aRes, 0, cntCommon);
      return Join(aRes);
    }

    #endregion

    #region Статические экземпляры

    /// <summary>
    /// Пустая структура
    /// </summary>
    public static readonly AbsPath Empty = new AbsPath(String.Empty);

    #endregion
  }

  /// <summary>
  /// Относительный или абсолютный путь к файлу или каталогу.
  /// Реализует методы для манипуляции с путями, заявленные в <see cref="System.IO.Path"/>.
  /// Не выполняет никаких действий с реальными файлами и каталогами.
  /// В Linux поддерживаются пути, начинающиеся с символа домашней папки "~". Так как такие пути не поддерживаются файловыми функциями
  /// (этот символ распознается только командной оболочкой), для открытия файлов и других действий следует использовать преобразование
  /// в <see cref="AbsPath"/> и передавать функциям полный путь к файлу.
  /// </summary>
  [Serializable]
  public struct RelPath
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, содержащий абсолютный или относительный путь к файлу или каталогу.
    /// Если аргумент заканчивается на слэш, то он удаляется (кроме некоторых путей вида "C:\")
    /// Задание пути в Uri-формате не допускается
    /// </summary>
    /// <param name="s">Задаваемый путь</param>
    public RelPath(string s)
    {
      try
      {
        // Путь может быть заключен в кавычки
        string s2 = AbsPath.RemoveQuotes(s);
        s2 = AbsPath.RemoveDirNameSlash(s2);
        if (String.IsNullOrEmpty(s2))
          _Path = null;
        else
        {
          if (AbsPath.StartsWithUriScheme(s2))
            throw new NotImplementedException("URI format for a relative path is not implemented");
          else
          {
            _Path = s2;
          }
        }
      }
      catch (Exception e)
      {
        throw new ArgumentException(String.Format("Cannot convert \'{0}\' to the relative path. {1}", s, e.Message), e);
      }
    }


    /// <summary>
    /// Создает путь на основе базового, с произвольным числом подкаталогов
    /// </summary>
    /// <param name="basePath">Базовый каталог</param>
    /// <param name="subNames">Дочерние подкаталоги</param>
    public RelPath(RelPath basePath, params string[] subNames)
    {
      RelPath p1 = basePath;
      for (int i = 0; i < subNames.Length; i++)
      {
        // Убираем кавычки
        string s2 = AbsPath.RemoveQuotes(subNames[i]);

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
    /// Создает объект <see cref="RelPath"/>.
    /// Если задана пустая строка или возникает ошибка преобразования, возвращается <see cref="RelPath.Empty"/>.
    /// См. описание соответствующего конструктора <see cref="RelPath"/>.
    /// </summary>
    /// <param name="s">Путь, который нужно преобразовать</param>
    /// <returns>Объект <see cref="RelPath"/></returns>
    [DebuggerStepThrough]
    public static RelPath Create(string s)
    {
      if (String.IsNullOrEmpty(s))
        return RelPath.Empty;
      try
      {
        return new RelPath(s);
      }
      catch
      {
        return RelPath.Empty;
      }
    }


    /// <summary>
    /// Создает объект <see cref="RelPath"/>. 
    /// Если <paramref name="basePath"/>.IsEmpty=true или возникает ошибка преобразования,
    /// возвращается <see cref="RelPath.Empty"/>.
    /// См. описание соответствующего конструктора <see cref="RelPath"/>.
    /// </summary>
    /// <param name="basePath">Базовый каталог</param>
    /// <param name="subNames">Дочерние подкаталоги</param>
    /// <returns>Объект <see cref="RelPath"/></returns>
    [DebuggerStepThrough]
    public static RelPath Create(RelPath basePath, params string[] subNames)
    {
      if (basePath.IsEmpty)
        return RelPath.Empty;
      try
      {
        return new RelPath(basePath, subNames);
      }
      catch
      {
        return RelPath.Empty;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к каталогу или файлу.
    /// Задается в конструкторе.
    /// 
    /// </summary>
    public string Path { get { return _Path ?? String.Empty; } }
    private readonly string _Path;

    /// <summary>
    /// Пустой путь
    /// </summary>
    public static readonly RelPath Empty = new RelPath();

    /// <summary>
    /// Возвращает true, если путь не задан
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Path); } }

    /// <summary>
    /// Возвращает true, если путь является абсолютным.
    /// Вызывает <see cref="System.IO.Path.IsPathRooted(string)"/>.
    ///
    /// В Linux считает путь "~" или начинающийся с него относительным.
    /// Вызов <see cref="ToAbsolute(AbsPath)"/> или преобразование к типу <see cref="AbsPath"/> заменяет "~" на путь к домашней папке.
    /// </summary>
    public bool IsAbsPath
    {
      get
      {
        return System.IO.Path.IsPathRooted(Path);
      }
    }

    /// <summary>
    /// Возвращает <see cref="Path"/>
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
          return _Path + System.IO.Path.DirectorySeparatorChar; 
      }
    }

    /// <summary>
    /// Возвращает путь, заключенный в кавычки (для передачи в качестве аргумента внешним программам)
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

    #region Преобразование

    /// <summary>
    /// Преобразует в абсолютный путь, используя, при необходимости, текущий каталог <see cref="Environment.CurrentDirectory"/> в качестве базового.
    /// Если <paramref name="relPath"/> - пустой, то возвращается пустой путь, а не текущий рабочий каталог.
    /// Требуется явное преобразование (explicit), так в нем неявно участвует текущий каталог.
    /// </summary>
    /// <param name="relPath">Относительный путь</param>
    /// <returns>Абсолютный путь</returns>
    public static explicit operator AbsPath(RelPath relPath)
    {
      return new AbsPath(relPath.Path);
    }

    /// <summary>
    /// Преобразует абсолютный путь в объект <see cref="RelPath"/>. При этом сам путь не преобразуется и сохраняется как абсолютный,
    /// то есть будет возвращаться <see cref="RelPath.IsAbsPath"/>=true.
    /// Если <paramref name="absPath"/> - пустой, то возвращается пустой путь <see cref="RelPath.Empty"/>.
    /// Преобразование объявлено явным, так как обычно требуется получение относительного пути как разности двух объектов <see cref="AbsPath"/>,
    /// а не просто преобразование.
    /// </summary>
    /// <param name="absPath">Абсолютный путь</param>
    /// <returns>Объект <see cref="RelPath"/></returns>
    public static explicit operator RelPath(AbsPath absPath)
    {
      return new RelPath(absPath.Path);
    }

    /// <summary>
    /// Преобразует в форму абсолютного пути.
    /// Если <see cref="IsAbsPath"/>=true, возвращает текущий объект без изменений, а <paramref name="basePath"/> игнорируется.
    /// Если <see cref="IsEmpty"/>=true, возвращается пустой объект <see cref="Empty"/>, а не <paramref name="basePath"/>.
    /// Если текущий объект задает относительный путь, а <paramref name="basePath"/> не задан, то в качестве базового каталога используется текущий каталог <see cref="Environment.CurrentDirectory"/>.
    /// </summary>
    /// <param name="basePath">Базовый каталог, используемый для преобразования относительного пути</param>
    /// <returns>Путь с <see cref="IsAbsPath"/>=true</returns>
    public RelPath ToAbsolute(AbsPath basePath)
    {
      if (IsEmpty || IsAbsPath)
        return this;
      else
        return (RelPath)(basePath + this);
    }

    /// <summary>
    /// Преобразует в форму относительного пути.
    /// Если <see cref="IsAbsPath"/>=false, возвращает текущий объект без изменений, а <paramref name="basePath"/> игнорируется.
    /// Если <see cref="IsEmpty"/>=true, возвращается пустой объект <see cref="Empty"/>, а не <paramref name="basePath"/>.
    /// Если текуший объект совпадает с <paramref name="basePath"/>, то возвращается каталог ".".
    /// Если <see cref="IsAbsPath"/>=true, то вычисляется разностный каталог, если это возможно. См. описания оператора вычитания.
    /// Если текущий объект задает абсолютный путь, а <paramref name="basePath"/> не задан, то в качестве базового каталога используется текущий каталог <see cref="Environment.CurrentDirectory"/>.
    /// </summary>
    /// <param name="basePath">Базовый каталог, используемый для вычитания из текущего объекта</param>
    /// <returns></returns>
    public RelPath ToRelative(AbsPath basePath)
    {
      if (IsAbsPath)
      {
        if (basePath.IsEmpty)
          basePath = new AbsPath(Environment.CurrentDirectory);
        return (AbsPath)this - basePath;
      }
      else
        return this;
    }

    #endregion

    #region Добавление подкаталога

    /// <summary>
    /// Добавление относительного пути.
    /// Использует функцию <see cref="System.IO.Path.Combine(string, string)"/>.
    /// </summary>
    /// <param name="basePath">Исходный путь</param>
    /// <param name="subDir">Подкаталог</param>
    /// <returns>Новый относительный путь</returns>
    public static RelPath operator +(RelPath basePath, string subDir)
    {
      if (basePath.IsEmpty)
        //throw new ArgumentException("Базовый каталог пустой", "basePath");
        return new RelPath(subDir); // 30.06.2023

      if (String.IsNullOrEmpty(subDir))
        return basePath;

      return basePath + new RelPath(subDir);
    }

    /// <summary>
    /// Добавление относительного пути.
    /// Использует функцию <see cref="System.IO.Path.Combine(string, string)"/>.
    /// Если <paramref name="path2"/> задает полный путь, то он возвращается, а первый путь отбрасывается.
    /// </summary>
    /// <param name="path1">Исходный путь</param>
    /// <param name="path2">Добавляемый путь</param>
    /// <returns>Новый относительный путь</returns>
    public static RelPath operator +(RelPath path1, RelPath path2)
    {
      if (path1.IsEmpty)
        return path2;

      if (path2.IsEmpty)
        return path1;

      if (path2.IsAbsPath)
        return path2;

      return new RelPath(System.IO.Path.Combine(path1.Path, path2.Path));
    }

    /// <summary>
    /// Присоединение относительного пути к абсолютному.
    /// Если <paramref name="subPath"/> задает абсолютный путь, а не относительный,
    /// он возвращается, а <paramref name="basePath"/> игнорируется.
    /// Если <paramref name="subPath"/> задает относительный путь, а <paramref name="basePath"/> не задан, то в качестве базового каталога используется текущий каталог <see cref="Environment.CurrentDirectory"/>.
    /// </summary>
    /// <param name="basePath">Абсолютный базовый путь</param>
    /// <param name="subPath">Относительный путь</param>
    /// <returns>Абсолютный путь</returns>
    public static AbsPath operator +(AbsPath basePath, RelPath subPath)
    {
      if (subPath.IsEmpty)
        return basePath;
      if (subPath.IsAbsPath)
        return new AbsPath(subPath.Path);
      if (AbsPath.UnixHomePath.Length>0 && subPath.Path[0]=='~')
        return new AbsPath(subPath.Path); // 21.11.2023
      else if (basePath.IsEmpty)
        return new AbsPath(subPath.Path);
      else
        return new AbsPath(basePath, subPath.Path);
    }

    #endregion
  }
}
