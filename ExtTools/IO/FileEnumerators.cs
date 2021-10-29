using System;
using System.Collections.Generic;
using System.Text;
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
  #region Перечисление PathEnumerateKind

  /// <summary>
  /// Что должно перечисляться: файлы, каталоги или то и другое
  /// </summary>
  public enum PathEnumerateKind
  {
    /// <summary>
    /// Перечислитель будет возвращать имена файлов.
    /// </summary>
    Files,

    /// <summary>
    /// Перечислитель будет возвращать имена каталогов. Файлы просматриваться не будут.
    /// Корневой каталог не возвращается при перечислении.
    /// </summary>
    Directories,

    /// <summary>
    /// Перечислитель будет возвращать и имена файлов и имена каталогов.
    /// Корневой каталог не возвращается при перечислении.
    /// </summary>
    FilesAndDirectories
  }

  #endregion

  #region Перечисление PathEnumerateMode

  /// <summary>
  /// Порядок перебора файлов и каталогов в пределах родительского каталога с помощью FileEnumerable
  /// </summary>
  public enum PathEnumerateMode
  {
    /// <summary>
    /// Сначала файлы, потом каталоги. Этот режим используется по умолчанию.
    /// </summary>
    FilesAndDirectories,

    /// <summary>
    /// Сначала вложенные каталоги, потом - файлы в текущем каталоге
    /// </summary>
    DirectoriesAndFiles,

    /// <summary>
    /// Нерекурсивный перебор файлов
    /// </summary>
    FilesOnly,

    /// <summary>
    /// Рекурсивный просмотр вложенных каталогов без просмотра файлов в текущем каталоге
    /// </summary>
    DirectoriesOnly,

    /// <summary>
    /// Текущий каталог должен быть пропущен
    /// </summary>
    None
  }

  #endregion

  #region Перечисление PathEnumarateSort

  /// <summary>
  /// Порядок сортировки файлов и каталогов при переборе
  /// </summary>
  public enum PathEnumerateSort
  {
    /// <summary>
    /// Порядок не определен.
    /// </summary>
    None,

    /// <summary>
    /// Сортировка по имени
    /// </summary>
    ByName,

    /// <summary>
    /// Сортировка по расширению.
    /// Для сложных расширений, типа "*.fb2.zip" используется только последнее расширение
    /// </summary>
    ByExtension
  }

  #endregion

  #region Перечисление PathEnumerateStage

  internal enum PathEnumerateStage
  {
    Start,
    Files,
    Directories,
    Finish
  }

  #endregion

  /// <summary>
  /// Аргументы события.
  /// Аргументы создаются AbsPathEnumerable.Enumerator в процессе перебора перед просмотром очередного каталога.
  /// </summary>
  public sealed class EnumDirectoryEventArgs : EventArgs
  {
    #region Защищенный конструктор

    internal EnumDirectoryEventArgs(AbsPath directory, RelPath directoryRel, int level)
    {
      _Directory = directory;
      _DirectoryRel = directoryRel;
      _Level = level;

      _Stage = PathEnumerateStage.Start;
    }

    /// <summary>
    /// Инициализация настраиваемых свойств
    /// </summary>
    /// <param name="source"></param>
    internal void Init(PathEnumerableBase source)
    {
      _EnumerateMode = source.EnumerateMode;
      _FileSearchPattern = source.FileSearchPattern;
      _FileSort = source.FileSort;
      _ReverseFiles = source.ReverseFiles;
      _DirectorySearchPattern = source.DirectorySearchPattern;
      _DirectorySort = source.DirectorySort;
      _ReverseDirectories = source.ReverseDirectories;
    }

    #endregion

    #region Фиксированные свойства

    /// <summary>
    /// Просматриваемый каталог - Абсолютный путь
    /// </summary>
    public AbsPath Directory { get { return _Directory; } }
    private AbsPath _Directory;


    /// <summary>
    /// Просматриваемый каталог - Путь относительно RootDirectory
    /// </summary>
    public RelPath DirectoryRel { get { return _DirectoryRel; } }
    private RelPath _DirectoryRel;

    /// <summary>
    /// Уровень вложения каталога Directory относительно базового каталога, с которого начинается перебор.
    /// При первом вызове события для RootDirectory имеет значение 0. Для подкаталогов первого уровня - 1, и т.д.
    /// </summary>
    public int Level { get { return _Level; } }
    private int _Level;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Directory.Path + " (Level=" + Level.ToString() + ")";
    }

    #endregion

    #region Настраиваемые свойства

    private void CheckNotReadOnly()
    {
      if (_Stage != PathEnumerateStage.Start)
        throw new ObjectReadOnlyException("Менять значения свойство можно только в обработчике события BeforeDirectory");
    }

    /// <summary>
    /// Режим просмотра каталога.
    /// Пользовательский обработчик может, например, пропустить ненужный каталог
    /// </summary>
    public PathEnumerateMode EnumerateMode
    {
      get { return _EnumerateMode; }
      set
      {
        CheckNotReadOnly();
        _EnumerateMode = value;
      }
    }
    private PathEnumerateMode _EnumerateMode;

    /// <summary>
    /// Маска для перечисления файлов. 
    /// </summary>
    public string FileSearchPattern
    {
      get { return _FileSearchPattern; }
      set
      {
        CheckNotReadOnly();
        _FileSearchPattern = value;
      }
    }
    private string _FileSearchPattern;

    /// <summary>
    /// Сортировка файлов при переборе. 
    /// </summary>
    public PathEnumerateSort FileSort
    {
      get { return _FileSort; }
      set
      {
        CheckNotReadOnly();
        _FileSort = value;
      }
    }
    private PathEnumerateSort _FileSort;

    /// <summary>
    /// Если установить в true, то файлы будут перебираться в обратном порядке сортировки.
    /// </summary>
    public bool ReverseFiles
    {
      get { return _ReverseFiles; }
      set
      {
        CheckNotReadOnly();
        _ReverseFiles = value;
      }
    }
    private bool _ReverseFiles;

    /// <summary>
    /// Маска для перечисления каталогов. 
    /// </summary>
    public string DirectorySearchPattern
    {
      get { return _DirectorySearchPattern; }
      set
      {
        CheckNotReadOnly();
        _DirectorySearchPattern = value;
      }
    }
    private string _DirectorySearchPattern;

    /// <summary>
    /// Сортировка каталогов при переборе. 
    /// </summary>
    public PathEnumerateSort DirectorySort
    {
      get { return _DirectorySort; }
      set
      {
        CheckNotReadOnly();
        _DirectorySort = value;
      }
    }
    private PathEnumerateSort _DirectorySort;

    /// <summary>
    /// Если установить в true, то файлы будут перебираться в обратном порядке сортировки.
    /// </summary>
    public bool ReverseDirectories
    {
      get { return _ReverseDirectories; }
      set
      {
        CheckNotReadOnly();
        _ReverseDirectories = value;
      }
    }
    private bool _ReverseDirectories;

    #endregion

    #region Внутренние поля, используемые перечислителем

    internal string[] Items;

    internal int CurrentIndex;

    /// <summary>
    /// Используется в состоянии Stage=Directory.
    /// </summary>
    internal bool SubDirFlag;

    internal PathEnumerateStage Stage { get { return _Stage; } }
    private PathEnumerateStage _Stage;

    /// <summary>
    /// Переходит к следующему этапу перебора.
    /// Только меняет свойство Stage с текущего состояние на следующее, исходя из EnumerateMode.
    /// Загрузка элементов не выполняется
    /// Для состояния Finish ничего не делает.
    /// </summary>
    private void SetNextStageValue()
    {
      switch (_Stage)
      {
        case PathEnumerateStage.Start:
          switch (_EnumerateMode)
          {
            case PathEnumerateMode.FilesOnly:
            case PathEnumerateMode.FilesAndDirectories:
              _Stage = PathEnumerateStage.Files;
              break;
            case PathEnumerateMode.DirectoriesOnly:
            case PathEnumerateMode.DirectoriesAndFiles:
              _Stage = PathEnumerateStage.Directories;
              break;
            default:
              _Stage = PathEnumerateStage.Finish;
              break;
          }
          break;
        case PathEnumerateStage.Files:
          switch (_EnumerateMode)
          {
            case PathEnumerateMode.FilesAndDirectories:
              _Stage = PathEnumerateStage.Directories;
              break;
            default:
              _Stage = PathEnumerateStage.Finish;
              break;
          }
          break;
        case PathEnumerateStage.Directories:
          switch (_EnumerateMode)
          {
            case PathEnumerateMode.DirectoriesAndFiles:
              _Stage = PathEnumerateStage.Files;
              break;
            default:
              _Stage = PathEnumerateStage.Finish;
              break;
          }
          break;
        default:
          _Stage = PathEnumerateStage.Finish;
          break;
      }
    }

    /// <summary>
    /// Переходит к следующему этапу перебора.
    /// и загружает списки файлов или каталогов, в зависимости от Stage.
    /// CurrentIndex принимает значение (-1).
    /// </summary>
    internal void SetNextStage()
    {
      SetNextStageValue();

      switch (_Stage)
      {
        case PathEnumerateStage.Files:
          Items = System.IO.Directory.GetFiles(Directory.Path,
            String.IsNullOrEmpty(FileSearchPattern) ? "*" : FileSearchPattern,
            System.IO.SearchOption.TopDirectoryOnly);
          SortItems(FileSort, ReverseFiles);
          break;
        case PathEnumerateStage.Directories:
          Items = System.IO.Directory.GetDirectories(Directory.Path,
            String.IsNullOrEmpty(DirectorySearchPattern) ? "*" : DirectorySearchPattern,
            System.IO.SearchOption.TopDirectoryOnly);
          SortItems(DirectorySort, ReverseDirectories);
          break;
        default:
          Items = null;
          break;
      }

      CurrentIndex = -1;
      SubDirFlag = false;
    }

    private void SortItems(PathEnumerateSort sort, bool reverse)
    {
      switch (sort)
      {
        case PathEnumerateSort.ByName:
          Array.Sort<string>(Items, ByNameComparison);
          break;
        case PathEnumerateSort.ByExtension:
          Array.Sort<string>(Items, ByExtensionComparison);
          break;
      }

      if (reverse)
        Array.Reverse(Items);
    }

    private static int ByNameComparison(string x, string y)
    {
      string x2 = System.IO.Path.GetFileName(x);
      string y2 = System.IO.Path.GetFileName(y);

      int res = String.Compare(x2, y2, StringComparison.OrdinalIgnoreCase);
      if (res != 0)
        return res;

      // Эта проверка лишняя.
      // В Windows никогда не будет одинаковых имен, отоичающихся регистром,
      // поэтому предыдущее условие всегда будет выполнено
      // if (AbsPath.ComparisonType == StringComparison.Ordinal)
      return String.Compare(x2, y2, StringComparison.Ordinal);
    }


    private static int ByExtensionComparison(string x, string y)
    {
      #region Сравнение расширения

      string x3 = System.IO.Path.GetExtension(x);
      string y3 = System.IO.Path.GetExtension(y);

      int res = String.Compare(x3, y3, StringComparison.OrdinalIgnoreCase);
      if (res != 0)
        return res;

      if (AbsPath.ComparisonType == StringComparison.Ordinal)
      {
        res = String.Compare(x3, y3, StringComparison.Ordinal);
        if (res != 0)
          return res;
      }

      #endregion

      #region Сравнение имени

      string x2 = System.IO.Path.GetFileNameWithoutExtension(x);
      string y2 = System.IO.Path.GetFileNameWithoutExtension(y);
      res = String.Compare(x2, y2, StringComparison.OrdinalIgnoreCase);
      if (res != 0)
        return res;

      return String.Compare(x2, y2, StringComparison.Ordinal);

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Делегат для события EnumDirectory
  /// </summary>
  /// <param name="sender">AbsPatheEnumerable или RelPathEnumerable</param>
  /// <param name="args">Аргументы события. В них можно задавать правила перебора для очередного каталога</param>
  public delegate void EnumDirectoryEventHandler(object sender, EnumDirectoryEventArgs args);

  /// <summary>
  /// Базовый класс для AbsPathEnumerable (и RelPathEnumerable, когда будет реализован)
  /// </summary>
  public class PathEnumerableBase
  {
    #region Защищенный конструктор

    /// <summary>
    /// Создает объект, присваивая значения свойствам RootDirectory и EnumerateKind.
    /// </summary>
    /// <param name="rootDirectory">Корневой каталог для перечисления. Должен быть задан</param>
    /// <param name="enumerateKind">Что должно возвращаться при перечислении: файлы и/или каталоги</param>
    protected PathEnumerableBase(AbsPath rootDirectory, PathEnumerateKind enumerateKind)
    {
      if (rootDirectory.IsEmpty)
        throw new ArgumentException("Не задан корневой каталог для перечисления");

      _RootDirectory = rootDirectory;
      _EnumerateKind = enumerateKind;

      _FileSearchPattern = "*";
      _FileSort = PathEnumerateSort.None;
      _DirectorySearchPattern = "*";
      _DirectorySort = PathEnumerateSort.None;
    }

    #endregion

    #region Свойства, управляющие перебором

    /// <summary>
    /// Корневой каталог для перечисления.
    /// Не может быть пустым.
    /// Задается в конструкторе.
    /// </summary>
    public AbsPath RootDirectory { get { return _RootDirectory; } }
    private AbsPath _RootDirectory;

    /// <summary>
    /// Что должно возвращаться при перечислении: файлы и/или каталоги.
    /// Задается в конструкторе.
    /// </summary>
    public PathEnumerateKind EnumerateKind { get { return _EnumerateKind; } }
    private PathEnumerateKind _EnumerateKind;

    /// <summary>
    /// Режим перебора. По умолчанию - FilesAndDirectories - сначала файлы, потом - подкаталоги.
    /// </summary>
    public PathEnumerateMode EnumerateMode { get { return _EnumerateMode; } set { _EnumerateMode = value; } }
    private PathEnumerateMode _EnumerateMode;

    /// <summary>
    /// Маска для перечисления файлов. По умолчанию - "*" - все файлы.
    /// </summary>
    public string FileSearchPattern { get { return _FileSearchPattern; } set { _FileSearchPattern = value; } }
    private string _FileSearchPattern;

    /// <summary>
    /// Сортировка файлов при переборе. По умолчанию - None - порядок не определен.
    /// </summary>
    public PathEnumerateSort FileSort { get { return _FileSort; } set { _FileSort = value; } }
    private PathEnumerateSort _FileSort;

    /// <summary>
    /// Если установить в true, то файлы будут перебираться в обратном порядке сортировки.
    /// По умолчанию - false.
    /// </summary>
    public bool ReverseFiles { get { return _ReverseFiles; } set { _ReverseFiles = value; } }
    private bool _ReverseFiles;

    /// <summary>
    /// Маска для перечисления каталогов. По умолчанию - "*" - все каталоги.
    /// </summary>
    public string DirectorySearchPattern { get { return _DirectorySearchPattern; } set { _DirectorySearchPattern = value; } }
    private string _DirectorySearchPattern;


    /// <summary>
    /// Сортировка каталогов при переборе. По умолчанию - None - порядок не определен.
    /// </summary>
    public PathEnumerateSort DirectorySort { get { return _DirectorySort; } set { _DirectorySort = value; } }
    private PathEnumerateSort _DirectorySort;

    /// <summary>
    /// Если установить в true, то файлы будут перебираться в обратном порядке сортировки.
    /// По умолчанию - false.
    /// </summary>
    public bool ReverseDirectories { get { return _ReverseDirectories; } set { _ReverseDirectories = value; } }
    private bool _ReverseDirectories;

    #endregion

    #region События перебора каталога

    /// <summary>
    /// Событие вызывается перед просмотром каждого каталога.
    /// Как минимум, вызывается один раз для просмотра корневого каталога.
    /// В аргументы события копируются настройки просмотра из этого объекта, а обработчик может их изменить.
    /// Например, можно пропустить ненужные каталоги
    /// </summary>
    public event EnumDirectoryEventHandler BeforeDirectory;

    /// <summary>
    /// Событие вызывается после просмотра каждого каталога, для которого вызывалось событие BeforeDirectory.
    /// Событие вызывается, даже если каталог пропускается установкой свойство EnumerateMode=None.
    /// Обработчик события не может менять свойства в аргументах события но может, например, удалить каталог.
    /// </summary>
    public event EnumDirectoryEventHandler AfterDirectory;

    #endregion

    #region Общая реализация перечислителя

    internal bool MoveNext(Stack<EnumDirectoryEventArgs> stack)
    {
      if (stack.Count == 0)
        PushDirectory(stack, String.Empty, 0);

      while (stack.Count > 0)
      {
        EnumDirectoryEventArgs args = stack.Peek();
        switch (args.Stage)
        {
          case PathEnumerateStage.Files:
            args.CurrentIndex++;
            if (args.CurrentIndex < args.Items.Length)
            {
              switch (EnumerateKind)
              {
                case PathEnumerateKind.Files:
                case PathEnumerateKind.FilesAndDirectories:
                  return true;

              }
            }
            args.SetNextStage();
            break;

          case PathEnumerateStage.Directories:
            if (!args.SubDirFlag)
            {
              args.CurrentIndex++;
              if (args.CurrentIndex < args.Items.Length)
              {
                args.SubDirFlag = true;
                switch (EnumerateKind)
                {
                  case PathEnumerateKind.Directories:
                  case PathEnumerateKind.FilesAndDirectories:
                    return true;  // требуется вернуть текущий каталог
                }
              }
              else
              {
                args.SetNextStage();
                continue;
              }
            }
            args.SubDirFlag = false;
            switch (args.EnumerateMode)
            {
              case PathEnumerateMode.DirectoriesOnly:
              case PathEnumerateMode.FilesAndDirectories:
              case PathEnumerateMode.DirectoriesAndFiles:
                string rel2 = System.IO.Path.GetFileName(args.Items[args.CurrentIndex]);
                if (args.DirectoryRel.Path.Length > 0)
                  rel2 = args.DirectoryRel.Path + System.IO.Path.DirectorySeparatorChar + rel2;
                PushDirectory(stack, rel2, args.Level + 1);
                break;
              default:
                args.SetNextStage();
                break;
            }
            break;

          default: // Stage=Finish;
            PopDirectory(stack);
            break;
        }
      }
      return false;
    }

    /// <summary>
    /// Создает новый объект EnumDirectoryEventArgs, вызывает событие BeforeDirectory, и добавляет объект в стек
    /// </summary>
    private void PushDirectory(Stack<EnumDirectoryEventArgs> stack, string relDir, int level)
    {
      EnumDirectoryEventArgs args = new EnumDirectoryEventArgs(RootDirectory + relDir, new RelPath(relDir), level);
      args.Init(this);
      if (BeforeDirectory != null)
        BeforeDirectory(this, args);

      // Корректируем режим перебора.
      // Если перебираются только каталоги, отменяем перебор файлов
      if (this.EnumerateKind == PathEnumerateKind.Directories)
      {
        switch (args.EnumerateMode)
        {
          case PathEnumerateMode.FilesAndDirectories:
          case PathEnumerateMode.DirectoriesAndFiles:
            args.EnumerateMode = PathEnumerateMode.DirectoriesOnly;
            break;
          case PathEnumerateMode.FilesOnly:
            args.EnumerateMode = PathEnumerateMode.None;
            break;
        }
      }

      args.SetNextStage();
      stack.Push(args);
    }

    /// <summary>
    /// Вызывает событие AfterDirectory и удаляет EnumDirectoryEventArgs из стека
    /// </summary>
    private void PopDirectory(Stack<EnumDirectoryEventArgs> stack)
    {
#if DEBUG
      if (stack.Count == 0)
        throw new BugException("Пустой стек");
#endif
      EnumDirectoryEventArgs args = stack.Peek();
      if (AfterDirectory != null)
        AfterDirectory(this, args);

      stack.Pop();
    }

    #endregion
  }

  /// <summary>
  /// Рекурсивное перечисление файлов и подкаталогов в каталоге.
  /// В отличие от System.Directory.GetFiles() и System.Directory.EnumerateFiles() (в Net Framework 4),
  /// позволяет управлять процессом перебора, чтобы не просматривать каталоги, которые не нужны.
  /// Имена файлов и каталогов задаются как структуры AbsPath.
  /// </summary>
  public sealed class AbsPathEnumerable : PathEnumerableBase, IEnumerable<AbsPath>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, присваивая значения свойствам RootDirectory и EnumerateKind.
    /// </summary>
    /// <param name="rootDirectory">Корневой каталог для перечисления. Должен быть задан</param>
    /// <param name="enumerateKind">Что должно возвращаться при перечислении: файлы и/или каталоги</param>
    public AbsPathEnumerable(AbsPath rootDirectory, PathEnumerateKind enumerateKind)
      : base(rootDirectory, enumerateKind)
    {
    }


    /// <summary>
    /// Создает объект, присваивая значения свойствам RootDirectory.
    /// EnumerateKind принимает значение Files, то есть перечисляться будут имена файлов.
    /// </summary>
    /// <param name="rootDirectory">Корневой каталог для перечисления. Должен быть задан</param>
    public AbsPathEnumerable(AbsPath rootDirectory)
      : this(rootDirectory, PathEnumerateKind.Files)
    {
    }

    #endregion

    #region Перечислитель

    /// <summary>
    /// Перечислитель
    /// </summary>
    public struct Enumerator : IEnumerator<AbsPath>
    {
      #region Защищенный конструктор

      internal Enumerator(AbsPathEnumerable owner)
      {
        _Owner = owner;
        _Stack = null;
      }

      #endregion

      #region Поля

      private AbsPathEnumerable _Owner;

      private Stack<EnumDirectoryEventArgs> _Stack;

      #endregion

      #region IEnumerator<AbsPath> Members

      /// <summary>
      /// Возвращает очередной файл или каталог
      /// </summary>
      public AbsPath Current
      {
        get
        {
          if (_Stack == null)
            return AbsPath.Empty;
          if (_Stack.Count == 0)
            return AbsPath.Empty;

          EnumDirectoryEventArgs args = _Stack.Peek();
          return new AbsPath(args.Items[args.CurrentIndex]);
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к следующему файлу или каталогу
      /// </summary>
      /// <returns>true, если перебор еще не закончен</returns>
      public bool MoveNext()
      {
        if (_Stack == null)
          _Stack = new Stack<EnumDirectoryEventArgs>();
        return _Owner.MoveNext(_Stack);
      }

      /// <summary>
      /// Сброс перечислителя в исходное состояние
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Stack = null;
      }

      #endregion
    }

    #endregion

    #region Интерфейс IEnumerable<AbsPath>

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<AbsPath> IEnumerable<AbsPath>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает количество файлов и/или каталогов, выполнив перечисление
    /// </summary>
    /// <returns></returns>
    public int GetCount()
    {
      int cnt = 0;
      foreach (AbsPath item in this)
        cnt++;
      return cnt;
    }

    /// <summary>
    /// Возвращает массив путей, выполнив перечисление
    /// </summary>
    public AbsPath[] ToArray()
    {
      List<AbsPath> lst = new List<AbsPath>();
      foreach (AbsPath item in this)
        lst.Add(item);
      return lst.ToArray();
    }

    #endregion
  }

  /// <summary>
  /// Рекурсивное перечисление файлов и подкаталогов в каталоге.
  /// В отличие от System.Directory.GetFiles() и System.Directory.EnumerateFiles() (в Net Framework 4),
  /// позволяет управлять процессом перебора, чтобы не просматривать каталоги, которые не нужны.
  /// Имена файлов и каталогов задаются как структуры RelPath.
  /// </summary>
  public sealed class RelPathEnumerable : PathEnumerableBase, IEnumerable<RelPath>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, присваивая значения свойствам RootDirectory и EnumerateKind.
    /// </summary>
    /// <param name="rootDirectory">Корневой каталог для перечисления. Должен быть задан</param>
    /// <param name="enumerateKind">Что должно возвращаться при перечислении: файлы и/или каталоги</param>
    public RelPathEnumerable(AbsPath rootDirectory, PathEnumerateKind enumerateKind)
      : base(rootDirectory, enumerateKind)
    {
      _RootDirectoryLen = rootDirectory.SlashedPath.Length;
    }


    /// <summary>
    /// Создает объект, присваивая значения свойствам RootDirectory.
    /// EnumerateKind принимает значение Files, то есть перечисляться будут имена файлов.
    /// </summary>
    /// <param name="rootDirectory">Корневой каталог для перечисления. Должен быть задан</param>
    public RelPathEnumerable(AbsPath rootDirectory)
      : this(rootDirectory, PathEnumerateKind.Files)
    {
    }

    // Количество символов в базовом каталоге, чтобы быстрее обрезать путь при переборе
    private int _RootDirectoryLen;

    #endregion

    #region Перечислитель

    /// <summary>
    /// Перечислитель
    /// </summary>
    public struct Enumerator : IEnumerator<RelPath>
    {
      #region Защищенный конструктор

      internal Enumerator(RelPathEnumerable owner)
      {
        _Owner = owner;
        _Stack = null;
      }

      #endregion

      #region Поля

      private RelPathEnumerable _Owner;

      private Stack<EnumDirectoryEventArgs> _Stack;

      #endregion

      #region IEnumerator<RelPath> Members

      /// <summary>
      /// Возвращает очередной файл или каталог
      /// </summary>
      public RelPath Current
      {
        get
        {
          if (_Stack == null)
            return new RelPath(String.Empty); // заглушка
          if (_Stack.Count == 0)
            return new RelPath(String.Empty); // заглушка

          EnumDirectoryEventArgs args = _Stack.Peek();
          return new RelPath(args.Items[args.CurrentIndex].Substring(_Owner._RootDirectoryLen));
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к следующему файлу или каталогу
      /// </summary>
      /// <returns>true, если перебор еще не закончен</returns>
      public bool MoveNext()
      {
        if (_Stack == null)
          _Stack = new Stack<EnumDirectoryEventArgs>();
        return _Owner.MoveNext(_Stack);
      }

      /// <summary>
      /// Сброс перечислителя в исходное состояние
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Stack = null;
      }

      #endregion
    }

    #endregion

    #region Интерфейс IEnumerable<RelPath>

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<RelPath> IEnumerable<RelPath>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает количество файлов и/или каталогов, выполнив перечисление
    /// </summary>
    /// <returns></returns>
    public int GetCount()
    {
      int cnt = 0;
      foreach (RelPath item in this)
        cnt++;
      return cnt;
    }

    /// <summary>
    /// Возвращает массив путей, выполнив перечисление
    /// </summary>
    public RelPath[] ToArray()
    {
      List<RelPath> lst = new List<RelPath>();
      foreach (RelPath item in this)
        lst.Add(item);
      return lst.ToArray();
    }

    #endregion
  }
}
