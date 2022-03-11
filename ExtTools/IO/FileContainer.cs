// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.IO
{

  /// <summary>
  /// Структура для хранения атрибутов файла, сохраняемого в базе данных или передаваемого между компьютерами
  /// Содержит некоторые поля, объявленные в стандартном классе System.IO.FileInfo
  /// Поля времени объявлены Nullable, чтобы яснее указывать, что данные недоступны
  /// </summary>
  [Serializable]
  public struct StoredFileInfo
  {
    #region Конструкторы

    /// <summary>
    /// Заполнить структуру по заданному полному пути к файлу.
    /// Если файл не существует, то инициализируется пустая структура. В том числе, свойство Name будет пустой строкой.
    /// </summary>
    /// <param name="filePath"></param>
    public StoredFileInfo(AbsPath filePath)
    {
      if (File.Exists(filePath.Path))
      {
        FileInfo fi = new FileInfo(filePath.Path);
        _Name = fi.Name;
        _Length = fi.Length;
        fi.Refresh();
        if (fi.CreationTime.Year >= 1980)
          _CreationTime = DateTime.SpecifyKind(fi.CreationTime, DateTimeKind.Unspecified);
        else
          _CreationTime = null;
        if (fi.LastWriteTime.Year >= 1980)
          _LastWriteTime = DateTime.SpecifyKind(fi.LastWriteTime, DateTimeKind.Unspecified);
        else
          _LastWriteTime = null;
      }
      else
      {
        _Name = String.Empty;
        _Length = 0L;
        _CreationTime = null;
        _LastWriteTime = null;
      }
    }

    /// <summary>
    /// Заполняет структуру заданными значениями свойств.
    /// Существование файла на диске не предполагается
    /// </summary>
    /// <param name="name">Имя файла</param>
    /// <param name="length">Размер файла</param>
    /// <param name="creationTime">Время создания</param>
    /// <param name="lastWriteTime">Время записи</param>
    public StoredFileInfo(string name, long length, DateTime? creationTime, DateTime? lastWriteTime)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (name.IndexOf('\\') >= 0)
        throw new ArgumentException("Имя файла не может содержать путь", "name");
      if (length < 0L)
        throw new ArgumentOutOfRangeException("length", length, "Длина файла не может быть отрицательной");

      _Name = name;
      _Length = length;

      if (creationTime.HasValue)
        _CreationTime = DateTime.SpecifyKind(creationTime.Value, DateTimeKind.Unspecified);
      else
        _CreationTime = null;

      if (lastWriteTime.HasValue)
        _LastWriteTime = DateTime.SpecifyKind(lastWriteTime.Value, DateTimeKind.Unspecified);
      else
        _LastWriteTime = null;
    }

    /// <summary>
    /// Версия конструктора, задающая время создания файла равным текущему времени
    /// </summary>
    /// <param name="name">Имя файла</param>
    /// <param name="length">Размер файла</param>
    public StoredFileInfo(string name, long length)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (name.IndexOf('\\') >= 0)
        throw new ArgumentException("Имя файла не может содержать путь", "name");
      if (length < 0L)
        throw new ArgumentOutOfRangeException("length", length, "Длина файла не может быть отрицательной");

      _Name = name;
      _Length = length;
      _CreationTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
      _LastWriteTime = _CreationTime;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя и расширение файла (без пути)
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long Length { get { return _Length; } }
    private readonly long _Length;

    /// <summary>
    /// Дата и время создания файла.
    /// Если значение задано, то оно хранится в формате DateTimeKind.Unspecified, чтобы оно не поменялось при сериализации
    /// </summary>
    public DateTime? CreationTime { get { return _CreationTime; } }
    private readonly DateTime? _CreationTime;

    /// <summary>
    /// Дата и время последней модификации файла
    /// Если значение задано, то оно хранится в формате DateTimeKind.Unspecified, чтобы оно не поменялось при сериализации
    /// </summary>
    public DateTime? LastWriteTime { get { return _LastWriteTime; } }
    private readonly DateTime? _LastWriteTime;

    /// <summary>
    /// Возвращает true, если структура не была заполенена
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(Name); } }

    #endregion

    #region Методы

    /// <summary>
    /// Применить атрибуты CreationTime и LastWriteTime к заданному файлу.
    /// Атрибуты NamePart и Length не применятся. Используется имя файла, заданное в
    /// качестве аргумента
    /// </summary>
    /// <param name="filePath">Полный путь к файлу (а не только каталог)</param>
    public void ApplyToFile(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");
      if (!File.Exists(filePath.Path))
        throw new FileNotFoundException("Файл не найден", filePath.Path);
      CheckNoEmpty();

      //FileInfo fi = new FileInfo(FilePath.Path);
      if (LastWriteTime.HasValue)
      {
        DateTime dt1;
        if (CreationTime.HasValue)
          dt1 = DateTime.SpecifyKind(CreationTime.Value, DateTimeKind.Local);
        else
          dt1 = DateTime.SpecifyKind(LastWriteTime.Value, DateTimeKind.Local);

        DateTime dt2 = DateTime.SpecifyKind(LastWriteTime.Value, DateTimeKind.Local);

        // 03.10.2017
        // Повторяем попытку 3 раза
        for (int i = 0; i < 3; i++)
        {
          try
          {
            File.SetCreationTime(filePath.Path, dt1);
            File.SetLastWriteTime(filePath.Path, dt2);
            break;
          }
          catch (System.IO.IOException)
          {
            if (i == 2)
              throw;
          }
          System.Threading.Thread.Sleep(100);
        }
      }
    }

    private void CheckNoEmpty()
    {
      if (IsEmpty)
        throw new InvalidOperationException("Структура FileInfo не была заполнена");
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустой экземпляр структуры
    /// </summary>
    public static readonly StoredFileInfo Empty = new StoredFileInfo();

    #endregion
  }

  /// <summary>
  /// Класс для хранения файла вместе с именем файла и атрибутами, а также для передачи его между клиентом
  /// и сервером. Путь к файлу не хранится, но может быть задано свойство SubDir
  /// В отличие от StoredFileInfo, не может хранить пустое имя файла
  /// Объект целиком инициализируется в конструкторе. Все методы и свойства являются
  /// потокобезопасными
  /// Производные классы могут реализовывать динамическую загрузку данных
  /// </summary>
  [Serializable]
  public class FileContainer : ICloneable, IComparable<FileContainer>
  {
    #region Конструкторы

    /// <summary>
    /// Создать контейнер с загруженным содержимым
    /// </summary>
    /// <param name="path">Полный путь к файлу</param>
    public FileContainer(AbsPath path)
      : this(path, String.Empty)
    {
    }

    /// <summary>
    /// Создать контейнер с загруженным содержимым
    /// </summary>
    /// <param name="path">Полный путь к файлу. Аргумент SubDir не учитывается при загрузке файла</param>
    /// <param name="subDir">Используется для установки свойства</param>
    public FileContainer(AbsPath path, string subDir)
    {
      if (path.IsEmpty)
        throw new ArgumentNullException("path");

      if (!File.Exists(path.Path))
        throw new FileNotFoundException("Файл \"" + path + "\" не найден", path.Path);

      _FileInfo = new StoredFileInfo(path);
      _Contents = File.ReadAllBytes(path.Path);
      _SubDir = AddDirNameSlash(subDir);
    }

    /// <summary>
    /// Создает контейнер из файла в памяти
    /// </summary>
    /// <param name="fileInfo">Имя файла, длина и другие атрибуты</param>
    /// <param name="contents">Содержимое файла. Длина массива должна соответствовать заявленной длине в <paramref name="fileInfo"/>.</param>
    public FileContainer(StoredFileInfo fileInfo, byte[] contents)
      : this(fileInfo, contents, String.Empty)
    {
    }

    /// <summary>
    /// Создает контейнер из файла в памяти
    /// </summary>
    /// <param name="fileInfo">Имя файла, длина и другие атрибуты</param>
    /// <param name="contents">Содержимое файла. Длина массива должна соответствовать заявленной длине в <paramref name="fileInfo"/>.</param>
    /// <param name="subDir">Имя подкаталога</param>
    public FileContainer(StoredFileInfo fileInfo, byte[] contents, string subDir)
    {
      if (fileInfo.IsEmpty)
        throw new ArgumentException("Информация о файле должна быть заполнена", "fileInfo");

      if (contents == null)
        contents = new byte[0];

      if (fileInfo.Length != contents.Length)
        throw new ArgumentException("Длина массива (" + contents.Length.ToString() +
          ") не совпадает с заданной в FileInfo (" + fileInfo.Length.ToString() + ") для файла \"" + fileInfo.Name + "\"", "contents");

      _FileInfo = fileInfo;
      _Contents = contents;
      _SubDir = AddDirNameSlash(subDir);
    }

    /// <summary>
    /// Создает контейнер из файла в памяти.
    /// Время создания файла и время записи будут равны текущему времени.
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="contents">Содержимое файла</param>
    public FileContainer(string fileName, byte[] contents)
      : this(fileName, contents, String.Empty)
    {
    }

    /// <summary>
    /// Создает контейнер из файла в памяти
    /// Время создания файла и время записи будут равны текущему времени.
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="contents">Содержимое файла</param>
    /// <param name="subDir">Имя подкаталога</param>
    public FileContainer(string fileName, byte[] contents, string subDir)
    {
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      if (contents == null)
        _Contents = new byte[0];
      else
        _Contents = contents;

      //_FileInfo = new StoredFileInfo(fileName, contents.Length);
      _FileInfo = new StoredFileInfo(fileName, _Contents.Length); // 27.12.2020

      _SubDir = AddDirNameSlash(subDir);
    }

    /// <summary>
    /// Создает контейнер из XML-файла в памяти.
    /// Используется DataTools.XmlDocumentToByteArray() для сериализации XML-документа
    /// Время создания файла и время записи будут равны текущему времени.
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="xmlDoc">XML-документ. Не может быть null</param>
    public FileContainer(string fileName, XmlDocument xmlDoc)
      : this(fileName, xmlDoc, String.Empty)
    {
    }

    /// <summary>
    /// Создает контейнер из XML-файла в памяти.
    /// Используется DataTools.XmlDocumentToByteArray() для сериализации XML-документа
    /// Время создания файла и время записи будут равны текущему времени.
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="xmlDoc">XML-документ. Не может быть null</param>
    /// <param name="subDir">Имя подкаталога</param>
    public FileContainer(string fileName, XmlDocument xmlDoc, string subDir)
    {
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      if (xmlDoc == null)
        throw new ArgumentNullException("xmlDoc");

      _Contents = DataTools.XmlDocumentToByteArray(xmlDoc);
      int len;
      if (_Contents == null)
        len = 0;
      else
        len = _Contents.Length;
      _FileInfo = new StoredFileInfo(fileName, len);
      _SubDir = AddDirNameSlash(subDir);
    }

    /// <summary>
    /// Может использоваться переопределенным классом, реализующим собственную инициализацию контейнера
    /// </summary>
    protected FileContainer()
    {
    }

    ///// <summary>
    ///// Нужен для десериализации
    ///// </summary>
    ///// <param name="info"></param>
    ///// <param name="context"></param>
    //protected FileContainer(SerializationInfo info, StreamingContext context)
    //{
    //}


    /// <summary>
    /// Добавление к имени каталога символа "\"
    /// Если каталог не задан, то возвращается пустая строка
    /// </summary>
    /// <param name="dirName">Имя каталога, которое может содержать или не содержать на конце слэш</param>
    /// <returns>Имя каталога со слэшем на конце</returns>
    private static string AddDirNameSlash(string dirName)
    {
      if (String.IsNullOrEmpty(dirName))
        return String.Empty;
      if (!dirName.EndsWith("\\"))
        dirName += "\\";
      return dirName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя и атрибуты файла
    /// </summary>
    public virtual StoredFileInfo FileInfo { get { return _FileInfo; } }

    /// <summary>
    /// Если производный класс переопределяет свойство FileInfo, 
    /// он может испоользовать это поле для хранения данных
    /// </summary>
    protected StoredFileInfo FileInfoInternal { get { return _FileInfo; } set { _FileInfo = value; } }
    private StoredFileInfo _FileInfo;

    /// <summary>
    /// Содержимое файла
    /// </summary>
    public virtual byte[] Contents { get { return _Contents; } }

    /// <summary>
    /// Если производный класс переопределяет свойство Contents, 
    /// он может испоользовать это поле для хранения данных
    /// </summary>
    protected byte[] ContentsInternal { get { return _Contents; } set { _Contents = value; } }
    private byte[] _Contents;

    /// <summary>
    /// Подкаталог. Если задан, то заканчивается символом "\"
    /// </summary>
    public virtual string SubDir { get { return _SubDir; } }

    /// <summary>
    /// Если производный класс переопределяет свойство SubDir, 
    /// он может испоользовать это поле для хранения данных
    /// </summary>
    protected string SubDirInternal { get { return _SubDir; } set { _SubDir = value; } }
    private string _SubDir;

    #endregion

    #region Методы

    /// <summary>
    /// Запись файла в каталог.
    /// Учитывается каталог SubDir.
    /// Если каталог не существует, то он создается
    /// </summary>
    /// <param name="dir">Каталог для записи файла</param>
    public void Save(AbsPath dir)
    {
      if (dir.IsEmpty)
        throw new ArgumentException("Не задан каталог для сохранения", "dir");

      AbsPath Dir2 = dir + SubDir;
      FileTools.ForceDirs(Dir2);
      AbsPath ResPath = Dir2 + FileInfo.Name;
      File.WriteAllBytes(ResPath.Path, Contents);
      FileInfo.ApplyToFile(ResPath);
    }

    /// <summary>
    /// Записать файл под указанным именем.
    /// Текущее имя файла FileInfo.Name и каталог SubDir игнорируются
    /// Если каталог не существует, то он создается
    /// </summary>
    /// <param name="filePath">Имя файла для записи</param>
    public void SaveAs(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw new ArgumentException("Не задано имя файла для сохранения", "filePath");

      FileTools.ForceDirs(filePath.ParentDir);
      File.WriteAllBytes(filePath.Path, Contents);
      FileInfo.ApplyToFile(filePath);
    }

    /// <summary>
    /// текстовое представление "Каталог Имя файла"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _SubDir + _FileInfo.Name;
    }

    /// <summary>
    /// Получить в виде документа XML
    /// </summary>
    /// <returns></returns>
    public XmlDocument GetXml()
    {
      return DataTools.XmlDocumentFromByteArray(Contents);
    }

    /// <summary>
    /// Возвращает копию объекта, содержащую загруженные данные
    /// </summary>
    /// <returns>Копия объекта</returns>
    public FileContainer Clone()
    {
      return new FileContainer(this.FileInfo, this.Contents, this.SubDir);
    }

    /// <summary>
    /// Возвращает копию объекта, содержащую загруженные данные, с заменой SubDir
    /// </summary>
    /// <param name="newSubDir">Новое значение свойства SubDir</param>
    /// <returns>Копия объекта</returns>
    public FileContainer Clone(string newSubDir)
    {
      return new FileContainer(this.FileInfo, this.Contents, newSubDir);
    }

    #endregion

    #region ICloneable Members

    object ICloneable.Clone()
    {
      return this.Clone();
    }

    #endregion

    #region Сравнение двух файлов

    /// <summary>
    /// Сравнение двух файлов с учетом имен и времени модификации
    /// </summary>
    /// <param name="file1">Первый из двух сравниваемых файлов</param>
    /// <param name="file2">Второй из двух сравниваемых файлов</param>
    /// <returns>Результат сравнения</returns>
    public static int Compare(FileContainer file1, FileContainer file2)
    {
      return Compare(file1, file2, true, true);
    }

    /// <summary>
    /// Сравнение двух файлов
    /// Возвращает: 
    /// 0, если файлы совпадают;
    /// +1, если <paramref name="file1"/> длиннее, чем <paramref name="file2"/> или
    /// в другом смысле "больше";
    /// (-1), если <paramref name="file1"/> короче, чем <paramref name="file2"/>.
    /// </summary>
    /// <param name="file1">Первый из двух сравниваемых файлов</param>
    /// <param name="file2">Второй из двух сравниваемых файлов</param>
    /// <param name="compareNames">Нужно ли сравнивать имена файлов</param>
    /// <param name="compareTime">Нужно ли сравнивать время последней модификации</param>
    /// <returns>Результат сравнения</returns>
    public static int Compare(FileContainer file1, FileContainer file2, bool compareNames, bool compareTime)
    {
      if (file1 == null)
        return file2 == null ? 0 : -1;

      if (file2 == null)
        return +1;

      // 1. Сравниваем содержимое файлов
      if (file1.FileInfo.Length > file2.FileInfo.Length)
        return +1;
      else if (file1.FileInfo.Length < file2.FileInfo.Length)
        return -1;
      for (int i = 0; i < file1.Contents.Length; i++)
      {
        byte b1 = file1.Contents[i];
        byte b2 = file2.Contents[i];
        if (b1 > b2)
          return +1;
        else if (b1 < b2)
          return -1;
      }
      // содержимое совпадает

      // Имя файла
      if (compareNames)
      {
        string s1 = file1.SubDir + file1.FileInfo.Name;
        string s2 = file2.SubDir + file2.FileInfo.Name;
        int x = String.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);
        if (x != 0)
          return x;
      }

      // Время
      if (compareTime)
      {
        DateTime? dt1 = file1.FileInfo.LastWriteTime;
        DateTime? dt2 = file2.FileInfo.LastWriteTime;
        if (dt1.HasValue)
        {
          if (dt2.HasValue)
          {
            int x = DateTime.Compare(dt1.Value, dt2.Value);
            if (x != 0)
              return x;
          }
          else
            return +1;
        }
        else
        {
          if (dt2.HasValue)
            return -1;
        }
      }

      // все совпадает 
      return 0;
    }

    #endregion

    #region IComparable<AccDepFileContainerList> Members

    /// <summary>
    /// Реализует интерфейс IComparable.
    /// Вызывает Compare(this, Other).
    /// См. метод Compare()
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public int CompareTo(FileContainer other)
    {
      return Compare(this, other);
    }

    #endregion
  }

  /// <summary>
  /// Список для хранения нескольких файлов.
  /// Этот класс является потокобезопасным после вызова метода SetReadOnly()
  /// </summary>
  [Serializable]
  public class FileContainerList : ICloneable, IReadOnlyObject, ICollection<FileContainer>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public FileContainerList()
    {
      _Items = new List<FileContainer>();
    }

    private FileContainerList(bool isReadOnly)
      : this()
    {
      _IsReadOnly = isReadOnly;
    }

    ///// <summary>
    ///// Этот конструктор нужен для десериализации списка
    ///// </summary>
    ///// <param name="info"></param>
    ///// <param name="context"></param>
    //protected FileContainerList(SerializationInfo info, StreamingContext context)
    //  : base(/*info, context*/)
    //{
    //}

    #endregion

    #region Список файлов

    private List<FileContainer> _Items;

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Доступ к контейнеру по индексу в списке
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    /// <returns>Элемент списка</returns>
    public FileContainer this[int index] { get { return _Items[index]; } }

    /// <summary>
    /// Доступ к контейнеру по имени файла.
    /// Свойство FileContainer.SubDir не учитывается при поиске.
    /// Если нет контейнера с таким файлом, возвращается null.
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>Найденный элемент</returns>
    public FileContainer this[string fileName]
    {
      get
      {
        FileContainer File;
        int p = IndexOf(fileName);
        if (p < 0)
          File = null;
        else
          File = _Items[p];
        return File;
      }
    }

    #endregion

    #region Методы работы с коллекцией

    /// <summary>
    /// Добавление файла в список.
    /// Если в списке уже есть файл с таким именем, он заменяется на новый
    /// </summary>
    /// <param name="item"></param>
    public void Add(FileContainer item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item.FileInfo.Name);
      if (p >= 0)
        _Items[p] = item;
      else
        _Items.Add(item);
    }

    /// <summary>
    /// Удаляет файл из списка
    /// </summary>
    /// <param name="item">Удвляемый элемент</param>
    /// <returns>true, если элемент был найден в списке и удален</returns>
    public bool Remove(FileContainer item)
    {
      CheckNotReadOnly();

      return _Items.Remove(item);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _Items.Clear();
    }

    /// <summary>
    /// Определяет наличие контейнера в списке
    /// </summary>
    /// <param name="item">Элемент для поиска</param>
    /// <returns>Наличие в списке</returns>
    public bool Contains(FileContainer item)
    {
      return _Items.Contains(item);
    }

    /// <summary>
    /// Копирует элементы в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(FileContainer[] array)
    {
      _Items.CopyTo(array);
    }

    /// <summary>
    /// Копирует элементы в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Индекс в заполняемом массиве</param>
    public void CopyTo(FileContainer[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирует элементы в массив
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, FileContainer[] array, int arrayIndex, int count)
    {
      _Items.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// Возвращает иедекс контейнера с заданным именем файла.
    /// Свойство FileContainer.SubDir игнорируется.
    /// Если контейнер не найден, возвращается (-1)
    /// </summary>
    /// <param name="fileName">Имя файла для поиска</param>
    /// <returns>Индекс элемента</returns>
    public int IndexOf(string fileName)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        if (_Items[i].FileInfo.Name == fileName)
          return i;
      }

      return -1;
    }

    /// <summary>
    /// Копирует массив элементов в списке
    /// </summary>
    /// <returns>Новый массив</returns>
    public FileContainer[] ToArray()
    {
      FileContainer[] a;
      a = new FileContainer[_Items.Count];
      _Items.CopyTo(a, 0);
      return a;
    }

    #endregion

    #region IReadOnlyObject

    /// <summary>
    /// Если свойство установлено в true, то файлы нельзя добавлять или удалять
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит список в состояние "только чтение"
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если список находится в режиме "только чтение"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException("Список файлов находится в режиме ReadOnly");
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString() + (IsReadOnly ? ", ReadOnly" : "");
    }

    /// <summary>
    /// Запись всех файлов в контейнере в указанный каталог
    /// </summary>
    /// <param name="dir">Каталог для записи</param>
    public void Save(AbsPath dir)
    {
      FileContainer[] a = ToArray();
      for (int i = 0; i < a.Length; i++)
        a[i].Save(dir);
    }

    /// <summary>
    /// Возвращает массив имен файлов (с подкаталогами, если есть)
    /// </summary>
    /// <returns></returns>
    public string[] GetFileNames()
    {
      string[] a;
      a = new string[_Items.Count];
      for (int i = 0; i < _Items.Count; i++)
        a[i] = _Items[i].FileInfo.Name;
      return a;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка без признака ReadOnly.
    /// Если контейнеры класса являются производными (с дополнительными методами 
    /// загрузки), то они заменяются на обычные объекты AccDepFileContainer с
    /// загруженными данными
    /// </summary>
    /// <returns>Копия списка файлов</returns>
    public FileContainerList Clone()
    {
      FileContainerList Copy = new FileContainerList();
      for (int i = 0; i < _Items.Count; i++)
        Copy.Add(_Items[i].Clone());
      return Copy;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Статический объект

    /// <summary>
    /// Пустой список файлов, который нельзя менять
    /// </summary>
    public static readonly FileContainerList Empty = new FileContainerList(true);

    #endregion

    #region IEnumerable<FileContainer> Members

    /// <summary>
    /// Создает перечислитель по файлам в списке
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<FileContainer>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<FileContainer> IEnumerable<FileContainer>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion
  }
}
