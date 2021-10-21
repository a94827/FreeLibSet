using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2018, Ageyev A.V.
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
  /// <summary>
  /// Относительный или абсолютный путь к файлу или каталогу
  /// Реализует методы для манипуляции с путями, заявленные в System.IO.Path
  /// Не выполняет никаких действий с реальными файлами и каталогами
  /// </summary>
  [Serializable]
  public class RelPath
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, содержащий абсолютный путь к файлу или каталогу
    /// Если аргумент заканчивается на слэш, то он удаляется
    /// Задание пути в Uri-формате не допускается
    /// </summary>
    /// <param name="s"></param>
    public RelPath(string s)
    {
      try
      {
        // Путь может быть заключен в кавычки
        string s2 = AbsPath.RemoveQuotes(s);
        s2 = AbsPath.RemoveDirNameSlash(s2);
        if (String.IsNullOrEmpty(s2))
          _Path = String.Empty;
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
    /// Путь к каталогу или файлу
    /// Путь хранится в форме, пригодной
    /// для использования методами классов в System.IO
    /// Задается в конструкторе.
    /// </summary>
    public string Path { get { return _Path; } }
    private readonly string _Path;

    /// <summary>
    /// Возвращает true, если путь не задан
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Path); } }

    /// <summary>
    /// Возвращает true, если путь является абсолютным.
    /// Вызывает System.IO.Path.IsPathRooted().
    /// </summary>
    public bool IsAbsPath
    {
      get
      {
        return System.IO.Path.IsPathRooted(Path);
      }
    }

    /// <summary>
    /// Возвращает Path
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Path == null)
        return String.Empty;
      else
        return _Path;
    }

    /// <summary>
    /// Возвращает путь, заканчивающийся обратным слэшем
    /// Используется, когда объект хранит каталог и нужно получить строку с именем
    /// находящегося в каталоге файла
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
    /// </summary>
    /// <param name="relPath"></param>
    /// <returns></returns>
    public static implicit operator AbsPath(RelPath relPath)
    {
      return new AbsPath(relPath.Path);
    }

    #endregion

    #region Добавление подкаталога

    /// <summary>
    /// Добавление относительного пути.
    /// Использует функцию System.IO.Path.Combine()
    /// </summary>
    /// <param name="basePath">Исходный путь</param>
    /// <param name="subDir">Подкаталог</param>
    /// <returns>Новый относительный путь</returns>
    public static RelPath operator +(RelPath basePath, string subDir)
    {
      if (basePath.IsEmpty)
        //return new AbsPath(SubDir);
        throw new ArgumentException("Базовый каталог пустой", "basePath");

      if (String.IsNullOrEmpty(subDir))
        return basePath;

      return new RelPath(System.IO.Path.Combine(basePath.Path, subDir));
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
