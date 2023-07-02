// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.IO
{
  /// <summary>
  /// Относительный или абсолютный путь к файлу или каталогу
  /// Реализует методы для манипуляции с путями, заявленные в <see cref="System.IO.Path"/>.
  /// Не выполняет никаких действий с реальными файлами и каталогами.
  /// </summary>
  [Serializable]
  public class RelPath
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
            throw new NotImplementedException("Задание относительного пути в формате URI не поддерживается");
          else
          {
            _Path = s2;
          }
        }
      }
      catch (Exception e)
      {
        throw new ArgumentException("Не удалось преобразовать \"" + s + "\" в относительный путь. " + e.Message, e);
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

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к каталогу или файлу.
    /// Путь хранится в форме, пригодной для использования методами классов в System.IO.
    /// Задается в конструкторе.
    /// </summary>
    public string Path { get { return _Path ?? String.Empty; } }
    private readonly string _Path;

    /// <summary>
    /// Возвращает true, если путь не задан
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Path); } }

    /// <summary>
    /// Возвращает true, если путь является абсолютным.
    /// Вызывает <see cref="System.IO.Path.IsPathRooted(string)"/>.
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
          return _Path + System.IO.Path.DirectorySeparatorChar; // Исправлена бяка 17.03.2017
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
    /// Преобразует в абсолютный путь, используя, при необходимости, текущий каталог в качестве базового.
    /// Если <paramref name="relPath"/> - пустой, то возвращается пустой путь, а не текущий рабочий каталог
    /// </summary>
    /// <param name="relPath">Относительный путь</param>
    /// <returns>Абсолютный путь</returns>
    public static implicit operator AbsPath(RelPath relPath)
    {
      return new AbsPath(relPath.Path);
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
      else
        return new AbsPath(basePath, subPath.Path);
    }

    #endregion
  }
}
