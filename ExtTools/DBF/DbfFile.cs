// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;
using FreeLibSet.IO;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Text;

namespace FreeLibSet.DBF
{
  /// <summary>
  /// Описание для одного поля DBF-таблицы.
  /// Структура однократной записи
  /// </summary>
  [Serializable]
  public struct DbfFieldInfo
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание.
    /// Используйте методы CreateXXX() для создания корректных описаний
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="typeChar">Тип поля (буква)</param>
    /// <param name="length">Длина поля</param>
    /// <param name="precision">Число знаков после запятой</param>
    public DbfFieldInfo(string name, char typeChar, int length, int precision)
      : this(name, typeChar, length, precision, true)
    {
    }

    internal DbfFieldInfo(string name, char typeChar, int length, int precision, bool validateArgs)
    {
      if (validateArgs)
      {
        if (!IsValidFieldName(name))
          throw new ArgumentException(String.Format(Res.DbfStr_Arg_InvalidFieldName, name), "name");
        if ("CNDLMF".IndexOf(typeChar) < 0)
          throw new ArgumentException(String.Format(Res.DbfStr_Arg_InvalidFieldType, typeChar), "typeChar");

        switch (typeChar)
        {
          case 'C':
            if (length < 1 || length > UInt16.MaxValue)
              throw ExceptionFactory.ArgOutOfRange("length", length, 1, UInt16.MaxValue);
            break;
          case 'N':
          case 'F':
            if (length < 1 || length > 255) throw new ArgumentException(Res.DbfStr_Arg_InvalidFieldLengthN, "length"); break;
          case 'D': if (length != 8) throw new ArgumentException(Res.DbfStr_Arg_InvalidFieldLengthD, "length"); break;
          case 'L': if (length != 1) throw new ArgumentException(Res.DbfStr_Arg_InvalidFieldLengthL, "length"); break;
          case 'M': if (length != 10) throw new ArgumentException(Res.DbfStr_Arg_InvalidFieldLengthM, "length"); break;
        }

        switch (typeChar)
        {
          case 'N':
          case 'F':
            if (precision > 0 && precision > (length - 2))
              throw new ArgumentException(String.Format(Res.DbfStr_Arg_PrecisionForLength, precision, length), "precision");
            break;
          default:
            if (precision != 0)
              throw new ArgumentException(String.Format(Res.DbfStr_Arg_PrecisionForType, typeChar), "precision");
            break;
        }
      }

      _Name = name.ToUpperInvariant();
      _Type = typeChar;
      _Length = length;
      _Precision = precision;
    }

    /// <summary>
    /// Создает копию описания поля с другим именем
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="otherInfo">Образец, откуда берутся тип, длина и точность</param>
    public DbfFieldInfo(string name, DbfFieldInfo otherInfo)
    {
      if (!IsValidFieldName(name))
        throw new ArgumentException(String.Format(Res.DbfStr_Arg_InvalidFieldName, name), "name");
      if (otherInfo.IsEmpty)
        throw new ArgumentNullException("otherInfo");

      _Name = name;
      _Type = otherInfo.Type;
      _Length = otherInfo.Length;
      _Precision = otherInfo.Precision;
    }

    /// <summary>
    /// Создает описание строкового поля типа "C"
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="length">Длина</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateString(string name, int length)
    {
      return new DbfFieldInfo(name, 'C', length, 0);
    }

    /// <summary>
    /// Создает описание поля даты "D".
    /// Поле даты не может хранить компонент времени.
    /// В DBF-файле даты хранятся в формате "ГГГГММДД".
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateDate(string name)
    {
      return new DbfFieldInfo(name, 'D', 8, 0);
    }

    /// <summary>
    /// Создает описание целочисленного поля типа "N"
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="length">Число символов для поля, включая знак "-"</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateNum(string name, int length)
    {
      return new DbfFieldInfo(name, 'N', length, 0);
    }

    /// <summary>
    /// Создает описание числового поля типа "N"
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="length">Число символов для поля, включая знак "-", десятичную точку и знаки после запятой</param>
    /// <param name="precision">Число знаков после запятой. 0-для целочисленного поля</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateNum(string name, int length, int precision)
    {
      return new DbfFieldInfo(name, 'N', length, precision);
    }

    /// <summary>
    /// Создает описание логического поля "L".
    /// В DBF-файле поле занимает 1 символ и хранит значения "T" или "F".
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateBool(string name)
    {
      return new DbfFieldInfo(name, 'L', 1, 0);
    }

    /// <summary>
    /// Создает описание мемо-поля "M".
    /// В DBF-файле мемо-поля хранятся в отдельном файле. В самом DBF хранится только числовое смещение.
    /// Поле занимает 10 символов.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateMemo(string name)
    {
      return new DbfFieldInfo(name, 'M', 10, 0);
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Возвращает имя поля
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Возвращает букву типа поля
    /// </summary>
    public char Type { get { return _Type; } }
    private readonly char _Type;

    /// <summary>
    /// Возвращает размер поля в байтах
    /// </summary>
    public int Length { get { return _Length; } }
    private readonly int _Length;

    /// <summary>
    /// Возвращает число знаков после запятой для числового поля
    /// </summary>
    public int Precision { get { return _Precision; } }
    private readonly int _Precision;

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает true, если структура не заполнена.
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Name); } }

#if XXX
    /// <summary>
    /// Возвращает читаемое представление для типа поля
    /// </summary>
    public string TypeText
    {
      get
      {
        switch (_Type)
        {
          case 'C':
            return "Строковый";
          case 'N':
            return "Числовой";
          case 'L':
            return "Логический";
          case 'D':
            return "Дата";
          case 'M':
            return "Мемо";
          case 'F':
            return "Float";
          case '\0':
            return string.Empty;
          default:
            return "Неизвестный";
        }
      }
    }
#endif

    /// <summary>
    /// Возвращает читаемое представление для типа поля и его длины, например, "C10", "N3.0", "N12.2", "D".
    /// Если <see cref="IsEmpty"/>=true, возвращается пустая строка.
    /// </summary>
    public string TypeSizeText
    {
      get
      {
        if (_Type == '\0')
          return String.Empty;
        string s = new string(_Type, 1);
        switch (_Type)
        {
          case 'C':
            return s + StdConvert.ToString(_Length);
          case 'N':
          case 'F':
            return s + StdConvert.ToString(_Length) + "." + StdConvert.ToString(_Precision);
          default:
            return s;
        }
      }
    }

    /// <summary>
    /// Возвращает маску для числового поля
    /// </summary>
    public string Mask
    {
      get
      {
        switch (Type)
        {
          case 'N':
          case 'F':
            if (Precision == 0)
              return "0";
            else
              return "0." + new string('0', Precision);
          default:
            return String.Empty;
        }
      }
    }

    /// <summary>
    /// Возвращает тип для столбца, совместимый с <see cref="System.Data.DataColumn.DataType"/>
    /// </summary>
    public Type DataType
    {
      get
      {
        switch (Type)
        {
          case 'C':
          case 'M':
            return typeof(string);
          case 'N':
            if (Precision == 0)
            {
              if (Length > 9)
                return typeof(long);
              else
                return typeof(int);
            }
            else
              return typeof(Decimal);
          case 'D':
            return typeof(DateTime);
          case 'L':
            return typeof(bool);
          case 'F':
            return typeof(Double); // 22.03.2023
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// Возвращает текстовое представление в формате "Name (TypeSizeText)", например "FIELD1 (N10.1)".
    /// В скобках выводится тип и размер поля как свойство <see cref="TypeSizeText"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return String.Empty;
      else
        return Name + " (" + TypeSizeText + ")";
    }

    #endregion

    #region Проверка формата

    /// <summary>
    /// Определяет, поддерживается ли тип поля, заданный текущим описанием, в файле заданного формата dBase.
    /// </summary>
    /// <param name="fileFormat">Формат DBF-файла</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>true, если поле может быть задано для файла с заданным форматом</returns>
    public bool TestFormat(DbfFileFormat fileFormat, out string errorText)
    {
      string validFieldTypes;
      switch (fileFormat)
      {
        case DbfFileFormat.dBase2:
          validFieldTypes = "CNL";
          break;
        case DbfFileFormat.dBase3:
          validFieldTypes = "CNLDM";
          break;
        case DbfFileFormat.dBase4:
          validFieldTypes = "CNLDMF";
          break;
        default:
          validFieldTypes = null;
          break;
      }

      if (validFieldTypes != null)
      {
        if (validFieldTypes.IndexOf(Type) < 0)
        {
          errorText = String.Format(Res.DbfStr_Msg_InvalidFieldTypeForDbFormat, Type, Name, fileFormat, validFieldTypes);
          return false;
        }
      }

      if (fileFormat == DbfFileFormat.dBase2 && Length > 255)
      {
        errorText = String.Format(Res.DbfStr_Msg_DBase2InvalidFieldLengthC, Name, Length);
        return false;
      }

      errorText = null;
      return true;
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Проверка корректности одиночного имени поля DBF-таблицы
    /// Возвращает true, если выполняются следующие условия:
    /// 1. Имя содержит от 1 до 10 знаков
    /// 2. Имя состоит только из заглавных латинских букв, цифр и знака "_"
    /// 3. Имя не начинается с цифры
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>true, если имя поля является корректным</returns>
    public static bool IsValidFieldName(string fieldName)
    {
      const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";
      if (String.IsNullOrEmpty(fieldName))
        return false;
      if (/*fieldName.Length < 1 || 27.12.2020 - только что проверили */ fieldName.Length > 10)
        return false;
      for (int i = 0; i < fieldName.Length; i++)
      {
        if (validChars.IndexOf(fieldName[i]) < 0)
          return false;
        if (i == 0 && fieldName[0] >= '0' && fieldName[0] <= '9')
          return false;
      }
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Структура полей DBF-таблицы
  /// </summary>
  [Serializable]
  public class DbfStruct : IEnumerable<DbfFieldInfo>, IReadOnlyObject
  {
    #region Константы

    /// <summary>
    /// Максимальный разамер одной записи
    /// </summary>
    public const int MaxRecordSize = 65535;

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает пустой список полей.
    /// </summary>
    public DbfStruct()
    {
      _Items = new List<DbfFieldInfo>();
    }

    #endregion

    #region Свойства

    private List<DbfFieldInfo> _Items;

    /// <summary>
    /// Возвращает описание поля по индексу
    /// </summary>
    /// <param name="index">Индекс поля. Нумерация начинается с 0</param>
    /// <returns>Описание поля</returns>
    public DbfFieldInfo this[int index]
    {
      get { return _Items[index]; }
    }

    /// <summary>
    /// Возвращает описание поля по имени.
    /// Если в списке нет поля с таким именем, возвращается неинициализированная структура.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Описание поля</returns>
    public DbfFieldInfo this[string name]
    {
      get
      {
        int p = IndexOf(name);
        if (p >= 0)
          return _Items[p];
        else
          return new DbfFieldInfo();
      }
    }

    /// <summary>
    /// Возвращает количество полей в структуре
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Возвращает размер записи DBF-файла. Равно суммарной длине всех полей плюс один символ - маркер удаленной строки.
    /// </summary>
    public int RecordSize
    {
      get
      {
        int n = 1;
        for (int i = 0; i < _Items.Count; i++)
          n += _Items[i].Length;
        return n;
      }
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы одно MEMO-поле
    /// </summary>
    public bool HasMemo
    {
      get
      {
        for (int i = 0; i < _Items.Count; i++)
        {
          if (_Items[i].Type == 'M')
            return true;
        }

        return false;
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Поиск поля по имени.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Индекс поля или (-1), если такого поля нет</returns>
    public int IndexOf(string name)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        //if (FItems[i].Name == Name)
        if (String.Equals(_Items[i].Name, name, StringComparison.OrdinalIgnoreCase)) // 09.10.2018
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Добавляет заполненное описание поля в список.
    /// </summary>
    /// <param name="item">Описание поля</param>
    public void Add(DbfFieldInfo item)
    {
      CheckNotReadOnly();

      if (item.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("item");

      _Items.Add(item);
    }

    #region Специализированные методы добавления

    /// <summary>
    /// Добавить строковое поле типа "C"
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="length">Длина</param>
    public void AddString(string name, int length)
    {
      Add(new DbfFieldInfo(name, 'C', length, 0));
    }

    /// <summary>
    /// Добавить поле даты типа "D"
    /// </summary>
    /// <param name="name">Имя поля</param>
    public void AddDate(string name)
    {
      Add(new DbfFieldInfo(name, 'D', 8, 0));
    }

    /// <summary>
    /// Добавить целочисленное поле типа "N"
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="length">Длина</param>
    public void AddNum(string name, int length)
    {
      Add(new DbfFieldInfo(name, 'N', length, 0));
    }

    /// <summary>
    /// Добавить числовое поле типа "N"
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <param name="length">Длина</param>
    /// <param name="preciosion">Число знаков после запрятой</param>
    public void AddNum(string name, int length, int preciosion)
    {
      Add(new DbfFieldInfo(name, 'N', length, preciosion));
    }

    /// <summary>
    /// Добавить логическое поле типа "L"
    /// </summary>
    /// <param name="name">Имя поля</param>
    public void AddBool(string name)
    {
      Add(new DbfFieldInfo(name, 'L', 1, 0));
    }

    /// <summary>
    /// Добавить мемо-поле типа "M"
    /// </summary>
    /// <param name="name">Имя поля</param>
    public void AddMemo(string name)
    {
      Add(new DbfFieldInfo(name, 'M', 10, 0));
    }

    #endregion

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + _Items.Count.ToString() + (IsReadOnly ? " ReadOnly" : "");
    }

    /// <summary>
    /// Создает новую таблицу <see cref="System.Data.DataTable"/> с заполненным списком <see cref="DataTable.Columns"/>.
    /// Таблица не содержит строк.
    /// </summary>
    /// <returns>Новый объект <see cref="DataTable"/></returns>
    public DataTable CreateTable()
    {
      DataTable table = new DataTable();
      for (int i = 0; i < _Items.Count; i++)
        table.Columns.Add(_Items[i].Name, _Items[i].DataType);
      return table;
    }

    internal DataTable CreatePartialTable(string columnNames, out int[] dbfColPoss)
    {
      DataTable table = new DataTable();
      string[] aColNames = columnNames.Split(',');
      dbfColPoss = new int[aColNames.Length];
      for (int i = 0; i < aColNames.Length; i++)
      {
        dbfColPoss[i] = this.IndexOf(aColNames[i]);
        if (dbfColPoss[i] < 0)
          throw new ArgumentException(String.Format(Res.DbfStr_Arg_UnknownFieldName, aColNames[i]), "columnNames");

        table.Columns.Add(aColNames[i], this[dbfColPoss[i]].DataType);
      }
      return table;
    }

    /// <summary>
    /// Проверяет, что выбранный формат может быть использован для заданной структуры таблицы
    /// </summary>
    /// <param name="fileFormat">Проверяемый формат</param>
    /// <returns>true, если формат можно использовать</returns>
    public bool TestFormat(DbfFileFormat fileFormat)
    {
      string errorText;
      return TestFormat(fileFormat, out errorText);
    }

    /// <summary>
    /// Проверяет, что выбранный формат может быть использован для заданной структуры таблицы
    /// </summary>
    /// <param name="fileFormat">Проверяемый формат</param>
    /// <param name="errorText">Сюда помещается текст сообщения об ошибке</param>
    /// <returns>true, если формат можно использовать</returns>
    public bool TestFormat(DbfFileFormat fileFormat, out string errorText)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        if (!_Items[i].TestFormat(fileFormat, out errorText))
          return false;
      }

      if (fileFormat == DbfFileFormat.dBase2)
      {
        if (_Items.Count > 32)
        {
          errorText = Res.DbfStr_Msg_DBase2TooManyFields;
          return false;
        }
      }

      errorText = null;
      return true;
    }

    /// <summary>
    /// Возвращает массив строк, содержащий имена всех полей
    /// </summary>
    /// <returns>Массив</returns>
    public string[] GetNames()
    {
      if (Count == 0)
        return DataTools.EmptyStrings;

      string[] a = new string[Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = this[i].Name;
      return a;
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Возвращает true, если список полей заблокирован от изменений
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит список полей в режим "только чтение".
    /// Список блокируется при использовании в <see cref="DbfFile"/>.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion

    #region IEnumerable<DbfFieldInfo> Members

    /// <summary>
    /// Возвращает перечислитель по списку полей.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<DbfFieldInfo>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<DbfFieldInfo> IEnumerable<DbfFieldInfo>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion
  }

  #region Перечисление DbfFileFormat

  /// <summary>
  /// Формат DBF-файла
  /// </summary>
  public enum DbfFileFormat
  {
    /// <summary>
    /// Формат не определен
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// dBase-II (сигнатура 02h)
    /// </summary>
    dBase2 = 2,

    /// <summary>
    /// dBase-III / Clipper (возможно наличие DBT-файла)(сигнатура 03h/83h)
    /// </summary>
    dBase3 = 3,

    /// <summary>
    /// dBase-IV  (возможно наличие DBT-файла)(сигнатура 04h/8Bh)
    /// </summary>
    dBase4 = 4
  }

  /// <summary>
  /// Формат Memo-файла
  /// </summary>
  public enum DbfMemoFormat
  {
    /// <summary>
    /// Нет dbt-файла
    /// </summary>
    None,

    /// <summary>
    /// Простой DBT-файл формата dBase-III/Clipper. Размер страницы 512 байт.
    /// Может хранить только текстовые данные, заканчивающиеся символом 0x1а.
    /// </summary>
    dBase3,

    /// <summary>
    /// DBT-файл dBase4. Может хранить двоичные данные. Размер текста задается в явном виде.
    /// Пока не поддерживается FreeLibSet.
    /// </summary>
    dBase4
  }

  #endregion

  /// <summary>
  /// Класс исключения, генерируемого при нарушении формата DBF-файла
  /// </summary>
  [Serializable]
  public class DbfFileFormatException : Exception
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным текстом
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public DbfFileFormatException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DbfFileFormatException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Класс исключения, генерируемого при нарушении формата DBF-файла
  /// </summary>
  [Serializable]
  public class DbfMemoFileMissingException : DbfFileFormatException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным текстом
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public DbfMemoFileMissingException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DbfMemoFileMissingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Класс исключения, генерируемого при обращении к полю неподходящего типа
  /// </summary>
  [Serializable]
  public class DbfFieldTypeException : Exception
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект исключения
    /// </summary>
    /// <param name="fieldInfo">Описание поля</param>
    public DbfFieldTypeException(DbfFieldInfo fieldInfo)
      : this(fieldInfo.Name, fieldInfo.Type)
    {
    }

    /// <summary>
    /// Создает объект исключения
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="fieldType">Тип поля</param>
    public DbfFieldTypeException(string fieldName, char fieldType)
      : base(String.Format(Res.DbfFieldTypeException_Err_Message, fieldName + fieldType))
    {
      _FieldName = fieldName;
      _FieldType = fieldType;
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DbfFieldTypeException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _FieldName = info.GetString("FieldName");
      _FieldType = info.GetChar("FieldType");
    }


    /// <summary>
    /// Используется для сериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("FieldName", _FieldName);
      info.AddValue("FieldType", _FieldType);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя поля
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// Тип поля (буква)
    /// </summary>
    public char FieldType { get { return _FieldType; } }
    private char _FieldType;

    #endregion
  }


  /// <summary>
  /// Класс исключения, генерируемого при получении значения поля, если значение в файле
  /// не соответствует типу поля
  /// </summary>
  [Serializable]
  public class DbfValueFormatException : Exception
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение
    /// </summary>
    /// <param name="recNo">Номер строки в DBF-файле. Нумерация начинается с 1.</param>
    /// <param name="fieldInfo">Имя и тип поля</param>
    /// <param name="stringBuffer">Внутренний буфер для поля в строке DBF-файла</param>
    /// <param name="innerException">Вложенное исключение</param>
    public DbfValueFormatException(int recNo, DbfFieldInfo fieldInfo, string stringBuffer, Exception innerException)
      : base(GetMessageText(recNo, fieldInfo, innerException))
    {
      _RecNo = recNo;
      _FieldName = fieldInfo.Name;
      _FieldType = fieldInfo.Type;
      _StringBuffer = stringBuffer;
    }

    private static string GetMessageText(int recNo, DbfFieldInfo fieldInfo, Exception innerException)
    {
      string msg = String.Format(Res.DbfValueFormatException_Err_Message, recNo, fieldInfo.Name, fieldInfo.Type);
      return ExceptionFactory.MergeInnerException(msg, innerException);
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DbfValueFormatException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _RecNo = info.GetInt32("RecNo");
      _FieldName = info.GetString("FieldName");
      _FieldType = info.GetChar("FieldType");
      _StringBuffer = info.GetString("StringBuffer");
    }

    /// <summary>
    /// Используется при сериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("RecNo", _RecNo);
      info.AddValue("FieldName", _FieldName);
      info.AddValue("FieldType", _FieldType);
      info.AddValue("StringBuffer", _StringBuffer);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Номер строки. Нумерация начинается с 1.
    /// </summary>
    public int RecNo { get { return _RecNo; } }
    private int _RecNo;

    /// <summary>
    /// Имя поля, в котором произошла ошибка
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// Тип поля
    /// </summary>
    public char FieldType { get { return _FieldType; } }
    private char _FieldType;

    /// <summary>
    /// Буфер строки
    /// </summary>
    public string StringBuffer { get { return _StringBuffer; } }
    private string _StringBuffer;

    #endregion
  }

  /// <summary>
  /// Класс для чтения/записи DBF файла в форматах dBase-II (сигнатура 0x02), dBase-III/Clipper (сигнатура 0x03/0x83),
  /// dBase-IV (сигнатура 0x0B)
  /// Поддерживаются memo-поля для dBase-III (файлы DBT). Мемо для dBase-IV не реализован.
  /// Индексы не поддерживаются.
  /// </summary>
  public class DbfFile : DisposableObject, IReadOnlyObject
  {
    #region Константы

    private const uint MinMemoBlockCount = 1;
    private const uint MaxMemoBlockCount = (uint)(FileTools.GByte * 2 / 512);

    #endregion

    // Использование:
    // using(DbfReader rdr=new DbfReader("c:\\test.dbf"))
    // {
    //   while (rdr.Read())
    //   {
    //     string s=rdr.GetString("Code");
    //     int x=rdr.GetInt(2);
    //   }
    // }

    #region Конструкторы и Dispose

    #region Конструкторы для открытия существующего файла

    /// <summary>
    /// Открывает существующий файл только для чтения.
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// Если в файле есть мемо-поля, одновременно открывается и DBT-файл.
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    public DbfFile(AbsPath dbfPath)
      : this(dbfPath, DefaultEncoding, true)
    {
    }

    /// <summary>
    /// Открывает существующий файл.
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// Если в файле есть мемо-поля, одновременно открывается и DBT-файл.
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="isReadOnly">true - открыть файл только для чтения,
    /// false - открыть файл для чтения и изменения</param>
    public DbfFile(AbsPath dbfPath, bool isReadOnly)
      : this(dbfPath, DefaultEncoding, isReadOnly)
    {
    }

    /// <summary>
    /// Открывает существующий файл только для чтения.
    /// Если в файле есть мемо-поля, одновременно открывается и DBT-файл.
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="encoding">Используемая кодировка</param>
    public DbfFile(AbsPath dbfPath, Encoding encoding)
      : this(dbfPath, encoding, true)
    {
    }

    /// <summary>
    /// Открывает существующий файл.
    /// Если в файле есть мемо-поля, одновременно открывается и DBT-файл.
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="encoding">Используемая кодировка</param>
    /// <param name="isReadOnly">true - открыть файл только для чтения,
    /// false - открыть файл для чтения и изменения</param>
    public DbfFile(AbsPath dbfPath, Encoding encoding, bool isReadOnly)
    {
      if (dbfPath.IsEmpty)
        throw new ArgumentNullException("dbfPath");

      if (!File.Exists(dbfPath.Path))
        throw ExceptionFactory.FileNotFound(dbfPath);

      _ShouldDisposeStreams = true;

      if (isReadOnly)
        _fsDBF = new FileStream(dbfPath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      else
        _fsDBF = new FileStream(dbfPath.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); // ?? права
      try
      {
        if (_fsDBF.Length < 1)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_ZeroLength, dbfPath));

        // Определяем наличие МЕМО-файла
        int code = _fsDBF.ReadByte();
        _fsDBF.Position = 0L; // обязательно возвращаем на начало
        if ((code & 0x80) != 0)
        {
          AbsPath dbtPath = dbfPath.ChangeExtension(".DBT");
          if (AbsPath.ComparisonType == StringComparison.Ordinal) // для Linux
          {
            if (!File.Exists(dbtPath.Path))
              dbtPath = dbfPath.ChangeExtension(".dbt");
          }
          if (File.Exists(dbtPath.Path))
          {
            if (isReadOnly)
              _fsDBT = new FileStream(dbtPath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            else
              _fsDBT = new FileStream(dbtPath.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); // ?? права
          }
          else
          {
            //throw new FileNotFoundException("МЕМО-файл не найден: \"" + dbtPath.Path + "\"");
            _fsDBT = null;
          }
        }

        _Encoding = encoding;
        _IsReadOnly = isReadOnly;
        InitExist();
      }
      catch
      {
        DisposeStreams();
        throw;
      }
    }

    /// <summary>
    /// Открывает DBF-файл в потоке только для чтения.
    /// Файл не может содержать MEMO-полей.
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// </summary>
    /// <param name="dbfStream">Открытый на чтение поток для DBF-файла</param>
    public DbfFile(Stream dbfStream)
      : this(dbfStream, (Stream)null, DefaultEncoding, true)
    {
    }

    /// <summary>
    /// Открывает DBF-файл в потоке только для чтения.
    /// Если файл содержит MEMO-поля, должен быть предоставлен поток для DBT-файла
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// </summary>
    /// <param name="dbfStream">Открытый на чтение поток для DBF-файла</param>
    /// <param name="dbtStream">Открытый на чтение поток для DBT-файла или null</param>
    public DbfFile(Stream dbfStream, Stream dbtStream)
      : this(dbfStream, dbtStream, DefaultEncoding, true)
    {
    }

    /// <summary>
    /// Открывает DBF-файл в потоке только для чтения.
    /// Если файл содержит MEMO-поля, должен быть предоставлен поток для DBT-файла.
    /// </summary>
    /// <param name="dbfStream">Открытый на чтение поток для DBF-файла</param>
    /// <param name="dbtStream">Открытый на чтение поток для DBT-файла или null</param>
    /// <param name="encoding">Используемая кодировка</param>
    public DbfFile(Stream dbfStream, Stream dbtStream, Encoding encoding)
      : this(dbfStream, dbtStream, encoding, true)
    {
    }

    /// <summary>
    /// Открывает DBF-файл в потоке.
    /// Если файл содержит MEMO-поля, должен быть предоставлен поток для DBT-файла.
    /// </summary>
    /// <param name="dbfStream">Открытый поток для DBF-файла</param>
    /// <param name="dbtStream">Открытый поток для DBT-файла или null</param>
    /// <param name="encoding">Используемая кодировка</param>
    /// <param name="isReadOnly">true - открыть файл только для чтения,
    /// false - открыть файл для чтения и изменения</param>
    public DbfFile(Stream dbfStream, Stream dbtStream, Encoding encoding, bool isReadOnly)
    {
      if (dbfStream == null)
        throw new ArgumentNullException("dbfStream");

      _ShouldDisposeStreams = false;
      _fsDBF = dbfStream;
      _fsDBT = dbtStream;
      _Encoding = encoding;
      _IsReadOnly = isReadOnly;

      InitExist();
    }

    private void InitExist()
    {
      #region Инициализация общих свойств

      if (_Encoding == null)
        _Encoding = DefaultEncoding;
      if (_fsDBF == null)
        throw new NullReferenceException("DbfStream==null");

      SkipDeleted = true;

      #endregion

      #region 1. Читаем сигнатуру

      BinaryReader rdrDBF = new BinaryReader(_fsDBF);

      _MemoFormat = DbfMemoFormat.None;
      // Идентификатор DBF-файла
      int dbfId = rdrDBF.ReadByte();
      switch (dbfId)
      {
        case 0x02:
          _Format = DbfFileFormat.dBase2;
          break;
        case 0x03:
          _Format = DbfFileFormat.dBase3;
          break;
        case 0x04:
        case 0x0B:
          _Format = DbfFileFormat.dBase4;
          break;
        case 0x83:
          _Format = DbfFileFormat.dBase3;
          _MemoFormat = DbfMemoFormat.dBase3;
          break;
        case 0x84:
          _Format = DbfFileFormat.dBase4;
          _MemoFormat = DbfMemoFormat.dBase3; // ??
          break;
        case 0x8B:
          _Format = DbfFileFormat.dBase4;
          _MemoFormat = DbfMemoFormat.dBase4;
          break;
        case 0x30: // 08.09.2022 Visual FoxPro без мемо. 
          _Format = DbfFileFormat.dBase4;
          // В текущей реализации можно только читать, но не записывать файлы
          break;

        // не знаю, как обрабатывать
        //case 0x31: // Visual FoxPro с автоинкрементом 
        //case 0x32: // Visual FoxPro с полями типов Varchar и/или Varbinary

        default:
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_UnknownSignature,dbfId.ToString("X2")));
      }

      if (_MemoFormat == DbfMemoFormat.dBase4)
        throw new NotImplementedException(Res.DbfFile_Err_DBase4MemoNotImplement);

      #endregion

      #region 2. Читаем заголовок и список полей

      if (_Format == DbfFileFormat.dBase2)
        InitExistDbf2(rdrDBF);
      else
        InitExistDbf34(rdrDBF);

      if (DBStruct.RecordSize != _RecSize)
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_HeaderRecSizeMismatch, _RecSize, DBStruct.RecordSize));

      if (_DataOffset < rdrDBF.BaseStream.Position)
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_DataStartPositionOverTheHeader, _DataOffset));

      // Внутренние поля
      InitDBStructInternal();

      #endregion

      #region 3. Подготовка буфера строки

      //if (_fsDBT == null)
      //  _MemoFormat = DbfMemoFormat.None;

      // Отсутствие мемо-файла при наличии заголовка 0x83 еще не является фатальной ошибкой
      // Ошибка возникнет при попытке чтени значения мемо-поля
      /*
      if (FHasMemoFile != HasMemoFile2)
      {
        if (FHasMemoFile)
          FErrors.AddError("В заголовке таблицы не задано использование DBT-файла, но в описании присутствуют MEMO-поля");
        else
          FErrors.AddWarning("В заголовке таблицы задано использование DBT-файла, хотя не объявлено ни одного MEMO-поля");
      }
      FHasMemoFile = HasMemoFile2;
       * */

      _RecordBuffer = new byte[DBStruct.RecordSize];

      #endregion

      // rdrDbf.Dispose(); // не нужен

      #region 4. Проверка заголовка МЕМО-файла

      _MemoBlockCount = 0;
      if (_fsDBT != null)
      {
        BinaryReader rdrDBT = new BinaryReader(_fsDBT);

        if (rdrDBT.BaseStream.Length < 5)
          throw new DbfFileFormatException(Res.DBfMemoFile_Err_FileIsTooShort);

        _MemoBlockCount = rdrDBT.ReadUInt32();
        if (_MemoBlockCount < MinMemoBlockCount || _MemoBlockCount > MaxMemoBlockCount)
          throw new DbfFileFormatException(String.Format(Res.DbfMemoFile_Err_WrongBlockCount, _MemoBlockCount));

        long minMemoFileSize = (long)(_MemoBlockCount - 1) * 512 + 2;
        //long MaxMemoFileSize = (long)(_MemoBlockCount) * 512 + 1;
        if (rdrDBT.BaseStream.Length < minMemoFileSize)
          throw new DbfFileFormatException(String.Format(Res.DbfMemoFile_Err_FileSizeLessThanHeader, 
            rdrDBT.BaseStream.Length, minMemoFileSize));
        // Максимально возможный размер не проверяем, вдруг там ЭЦП
      }

      #endregion
    }

    private void InitExistDbf2(BinaryReader rdrDBF)
    {
      // https://www.fileformat.info/format/dbf/corion-dbase-ii.htm

      // Число записей
      _RecordCount = rdrDBF.ReadUInt16();

      // Метка даты MDY
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();

      // Смещение до начала данных
      _DataOffset = 521; // фиксированное

      // Размер записи
      _RecSize = rdrDBF.ReadUInt16();
      if (_RecSize < 1)
        throw new DbfFileFormatException(Res.DbfFile_Err_RecSizeIsZero);

      long wantedSize = _DataOffset + (long)_RecSize * (long)_RecordCount;
      if (rdrDBF.BaseStream.Length < wantedSize)
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_FileTooShort, rdrDBF.BaseStream.Length, wantedSize));

      byte[] bFldName = new byte[11];
      _DBStruct = new DbfStruct();
      while (true)
      {
        if (rdrDBF.Read(bFldName, 0, 1) < 1)
          throw new DbfFileFormatException(Res.DbfFile_Err_FieldListUnexpectedEOF);

        if (bFldName[0] == 0x0D)
          break; // Конец списка полей

        // Имя поля
        if (rdrDBF.Read(bFldName, 1, 10) < 10)
          throw new DbfFileFormatException(Res.DbfFile_Err_FieldListUnexpectedEOF);

        int endPos = Array.IndexOf<byte>(bFldName, 0);
        if (endPos < 0)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_FieldListNoEndOfName, DBStruct.Count + 1));

        if (endPos == 0)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_FieldListEmptyName, DBStruct.Count + 1));

        string fieldName = Encoding.GetString(bFldName, 0, endPos);
        // Тип поля
        byte[] bFldType = rdrDBF.ReadBytes(1);
        string fieldType = Encoding.GetString(bFldType);

        int len = rdrDBF.ReadByte();
        rdrDBF.ReadBytes(2); // пропуск
        int prec = rdrDBF.ReadByte();

        _DBStruct.Add(new DbfFieldInfo(fieldName, fieldType[0], len, prec, false));
      }
    }

    private void InitExistDbf34(BinaryReader rdrDBF)
    {
      // Метка даты YMD
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();

      // Число записей
      uint recCount = rdrDBF.ReadUInt32();
      if (recCount > (uint)(int.MaxValue))
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_TooManyRecords, recCount));
      _RecordCount = (int)recCount;
      // Смещение до начала данных
      _DataOffset = rdrDBF.ReadUInt16();
      // Размер записи
      _RecSize = rdrDBF.ReadUInt16();
      if (_RecSize < 1)
        throw new DbfFileFormatException(Res.DbfFile_Err_RecSizeIsZero);

      long wantedSize = _DataOffset + (long)_RecSize * (long)_RecordCount;
      if (rdrDBF.BaseStream.Length < wantedSize)
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_FileTooShort, rdrDBF.BaseStream.Length, wantedSize));

      // Заполнитель
      rdrDBF.ReadBytes(20);

      byte[] bFldName = new byte[11];
      _DBStruct = new DbfStruct();
      while (true)
      {
        if (rdrDBF.Read(bFldName, 0, 1) < 1)
          throw new DbfFileFormatException(Res.DbfFile_Err_FieldListUnexpectedEOF);

        if (bFldName[0] == 0x0D)
          break; // Конец списка полей

        // Имя поля
        if (rdrDBF.Read(bFldName, 1, 10) < 10)
          throw new DbfFileFormatException(Res.DbfFile_Err_FieldListUnexpectedEOF);

        int endPos = Array.IndexOf<byte>(bFldName, 0);
        if (endPos < 0)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_FieldListNoEndOfName, DBStruct.Count + 1));

        if (endPos == 0)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_FieldListEmptyName, DBStruct.Count + 1));

        string fieldName = Encoding.GetString(bFldName, 0, endPos);
        // Тип поля
        byte[] bFldType = rdrDBF.ReadBytes(1);
        string fieldType = Encoding.GetString(bFldType);

        rdrDBF.ReadBytes(4); // пропуск

        int len;
        int prec;
        if (fieldType == "C")
        {
          len = rdrDBF.ReadUInt16();
          prec = 0;
        }
        else
        {
          len = rdrDBF.ReadByte();
          prec = rdrDBF.ReadByte();
        }

        rdrDBF.ReadBytes(14);

        switch (fieldType)
        {
          case "C":
          case "N":
          case "D":
          case "L":
          case "M":
          case "F":
            break;
          default:
            /*
            FErrors.AddError("Поле \"" + FieldName + "\" имеет неизвестный тип \"" + FieldType +
              "\". Загружено как строковое поле");
             * */
            fieldType = "C";
            break;
        }

        _DBStruct.Add(new DbfFieldInfo(fieldName, fieldType[0], len, prec, false));

      }
    }

    #endregion

    #region Конструкторы для создания нового файла

    /// <summary>
    /// Создает на диске новый DBF-файл заданной структуры.
    /// Если в структуре есть мемо-поля, также создается DBT-файл.
    /// Если файл(ы) существует, он удаляется.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    public DbfFile(AbsPath dbfPath, DbfStruct dbStruct)
      : this(dbfPath, dbStruct, DefaultEncoding, DbfFileFormat.Undefined)
    {
    }

    /// <summary>
    /// Создает на диске новый DBF-файл заданной структуры.
    /// Если в структуре есть мемо-поля, также создается DBT-файл.
    /// Если файл(ы) существует, он удаляется.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    /// <param name="encoding">Используемая кодировка</param>
    public DbfFile(AbsPath dbfPath, DbfStruct dbStruct, Encoding encoding)
      : this(dbfPath, dbStruct, encoding, DbfFileFormat.Undefined)
    {
    }

    /// <summary>
    /// Создает на диске новый DBF-файл заданной структуры.
    /// Если в структуре есть мемо-поля, также создается DBT-файл.
    /// Если файл(ы) существует, он удаляется.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    /// <param name="encoding">Используемая кодировка</param>
    /// <param name="fileFormat">Формат файла. Если задано значение <see cref="DbfFileFormat.Undefined"/>,
    /// используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Если формат задан, но в списке полей есть несовместимые с ним типы, генерируется исключение.
    ///</param>
    public DbfFile(AbsPath dbfPath, DbfStruct dbStruct, Encoding encoding, DbfFileFormat fileFormat)
    {
#if DEBUG
      if (dbfPath.IsEmpty)
        throw new ArgumentNullException("dbfPath");
      if (dbStruct == null)
        throw new ArgumentNullException("dbStruct");
#endif
      if (dbStruct.Count == 0)
        throw ExceptionFactory.ArgIsEmpty("dbStruct");

      _DBStruct = dbStruct;
      _Encoding = encoding;
      _IsReadOnly = false;
      _ShouldDisposeStreams = true;
      _Format = fileFormat;

      try
      {
        _fsDBF = new FileStream(dbfPath.Path, FileMode.Create, FileAccess.ReadWrite);

        if (dbStruct.HasMemo)
        {
          AbsPath dbtFilePath;
          if (dbfPath.Extension == ".DBF")
            dbtFilePath = dbfPath.ChangeExtension(".DBT");
          else
            dbtFilePath = dbfPath.ChangeExtension(".dbt");

          _fsDBT = new FileStream(dbtFilePath.Path, FileMode.Create, FileAccess.ReadWrite);
        }

        InitNew();
      }
      catch
      {
        DisposeStreams();
        throw;
      }
    }


    /// <summary>
    /// Инициализирует поток DBF-файла для заданной структуры.
    /// Если в структуре есть мемо-поля, генерируется исключение.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfStream">Поток для записи DBF-файла</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    public DbfFile(Stream dbfStream, DbfStruct dbStruct)
      : this(dbfStream, (Stream)null, dbStruct, DefaultEncoding, DbfFileFormat.Undefined)
    {
    }

    /// <summary>
    /// Инициализирует потоки DBF-файла и DBT-файла для заданной структуры.
    /// Наличие потока DBT-файла должно определяться наличием в структуре мемо-полей.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Используется кодировка по умолчанию <see cref="DefaultEncoding"/>.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfStream">Поток для записи DBF-файла</param>
    /// <param name="dbtStream">Поток для записи DBT-файла или null, если мемо-полей нет</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    public DbfFile(Stream dbfStream, Stream dbtStream, DbfStruct dbStruct)
      : this(dbfStream, dbtStream, dbStruct, DefaultEncoding, DbfFileFormat.Undefined)
    {
    }

    /// <summary>
    /// Инициализирует поток DBF-файла для заданной структуры.
    /// Если в структуре есть мемо-поля, генерируется исключение.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfStream">Поток для записи DBF-файла</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    /// <param name="encoding">Кодировка файла</param>
    public DbfFile(Stream dbfStream, DbfStruct dbStruct, Encoding encoding)
      : this(dbfStream, (Stream)null, dbStruct, encoding, DbfFileFormat.Undefined)
    {
    }

    /// <summary>
    /// Инициализирует потоки DBF-файла и DBT-файла для заданной структуры.
    /// Наличие потока DBT-файла должно определяться наличием в структуре мемо-полей.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfStream">Поток для записи DBF-файла</param>
    /// <param name="dbtStream">Поток для записи DBT-файла или null, если мемо-полей нет</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    /// <param name="encoding">Кодировка файла</param>
    public DbfFile(Stream dbfStream, Stream dbtStream, DbfStruct dbStruct, Encoding encoding)
      : this(dbfStream, dbtStream, dbStruct, encoding, DbfFileFormat.Undefined)
    {
    }

    /// <summary>
    /// Инициализирует потоки DBF-файла и DBT-файла для заданной структуры.
    /// Наличие потока DBT-файла должно определяться наличием в структуре мемо-полей.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfStream">Поток для записи DBF-файла</param>
    /// <param name="dbtStream">Поток для записи DBT-файла или null, если мемо-полей нет</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    /// <param name="encoding">Кодировка файла</param>
    /// <param name="fileFormat">Формат файла. Если задано значение <see cref="DbfFileFormat.Undefined"/>,
    /// используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Если формат задан, но в списке полей есть несовместимые с ним типы, генерируется исключение.
    ///</param>
    public DbfFile(Stream dbfStream, Stream dbtStream, DbfStruct dbStruct, Encoding encoding, DbfFileFormat fileFormat)
    {
#if DEBUG
      if (dbfStream == null)
        throw new ArgumentNullException("dbfStream");
      if (dbStruct == null)
        throw new ArgumentNullException("dbStruct");
#endif
      if (dbStruct.Count == 0)
        throw ExceptionFactory.ArgIsEmpty("dbStruct");

      _ShouldDisposeStreams = false;
      _fsDBF = dbfStream;
      _fsDBT = dbtStream;
      _DBStruct = dbStruct;
      _Encoding = encoding;
      _IsReadOnly = false;
      _Format = fileFormat; // 27.12.2020. Присвоение было потеряно

      InitNew();
    }

    private void InitNew()
    {
      if (_Encoding == null)
        _Encoding = DefaultEncoding;

      if (_Format == DbfFileFormat.Undefined)
      {
        if (DBStruct.TestFormat(DbfFileFormat.dBase3))
          _Format = DbfFileFormat.dBase3;
        else
          _Format = DbfFileFormat.dBase4;
      }



      InitDBStructInternal();
      if (DBStruct.RecordSize > DbfStruct.MaxRecordSize)
        throw new DbfFileFormatException(String.Format(Res.DbfStr_Err_RecordSizeTooBig, DBStruct.RecordSize, DbfStruct.MaxRecordSize));
      _RecSize = (ushort)(DBStruct.RecordSize);

      BinaryWriter wrtDBF = new BinaryWriter(_fsDBF); // нельзя использовать using, чтобы не закрыть поток
      if (_Format == DbfFileFormat.dBase2)
        InitNewDBF2(wrtDBF);
      else
        InitNewDBF34(wrtDBF);
      wrtDBF.Flush();

      if (DBStruct.HasMemo && (_fsDBT != null))
      {
        if (_Format == DbfFileFormat.dBase3)
          _MemoFormat = DbfMemoFormat.dBase3;
        else
          throw new NotImplementedException(Res.DbfFile_Err_DBase4MemoNotImplement);
      }
      _RecordBuffer = new byte[_RecSize];

      if (MemoFormat == DbfMemoFormat.dBase3 && _fsDBT != null)
      {
        // DBT-файл содержит единственный блок
        _fsDBT.WriteByte(0x01);
        _fsDBT.WriteByte(0x00);
        _fsDBT.WriteByte(0x00);
        _fsDBT.WriteByte(0x00);
        _fsDBT.SetLength(512);
        _MemoBlockCount = 1;
      }

      SkipDeleted = true; // 20.03.2023
    }

    private void InitNewDBF2(BinaryWriter wrtDBF)
    {
      // Сигнатура
      wrtDBF.Write((byte)0x02);

      // Число записей
      _RecordCount = 0;
      wrtDBF.Write((UInt16)0); // Число записей (2 байта, а не 4)

      // Дата записи (MDY)
      DateTime dt = DateTime.Today;
      wrtDBF.Write((byte)(dt.Month));
      wrtDBF.Write((byte)(dt.Day));
      wrtDBF.Write((byte)(dt.Year % 100));

      // Смещение данных в файле
      _DataOffset = (UInt16)(521); // фиксированный размер заголовка

      // Размер записи
      wrtDBF.Write((UInt16)(_RecSize));

      // Описания полей
      if (DBStruct.Count > 32)
        throw new InvalidOperationException(Res.DbfStr_Msg_DBase2TooManyFields);
      for (int i = 0; i < DBStruct.Count; i++)
      {
        byte[] bName = System.Text.Encoding.ASCII.GetBytes(DBStruct[i].Name);
        wrtDBF.Write(bName);
        WriteZeros(wrtDBF, 11 - bName.Length);

        byte[] bType = System.Text.Encoding.ASCII.GetBytes(new char[1] { DBStruct[i].Type });
        wrtDBF.Write(bType);
        wrtDBF.Write((byte)(DBStruct[i].Length));
        wrtDBF.Write((UInt16)0); // смещение данных используется только в памяти, но не в файле
        wrtDBF.Write((byte)(DBStruct[i].Precision));
      }
      // Конец списка
      wrtDBF.Write((byte)0x0D);

      // Дополнение заголовка
      for (int i = DBStruct.Count; i < 32; i++)
        WriteZeros(wrtDBF, 16);

      // Маркер конца данных
      wrtDBF.Write((byte)0x1A);
    }

    private void InitNewDBF34(BinaryWriter wrtDBF)
    {
      // Сигнатура
      switch (_Format)
      {
        case DbfFileFormat.dBase3:
          wrtDBF.Write((byte)(DBStruct.HasMemo ? 0x83 : 0x03));
          break;
        case DbfFileFormat.dBase4:
          wrtDBF.Write((byte)(DBStruct.HasMemo ? 0x8B : 0x0B));
          break;
        default:
          throw new BugException("Format=" + _Format.ToString());
      }

      // Дата записи (YMD)
      DateTime dt = DateTime.Today;
      wrtDBF.Write((byte)(dt.Year % 100));
      wrtDBF.Write((byte)(dt.Month));
      wrtDBF.Write((byte)(dt.Day));

      // Число записей
      _RecordCount = 0;
      wrtDBF.Write((uint)0); // Число записей

      // Смещение данных в файле
      _DataOffset = (UInt16)((DBStruct.Count * 32) + 32 + 1);
      wrtDBF.Write(_DataOffset);

      // Размер записи
      wrtDBF.Write((UInt16)(_RecSize));
      // Заполнитель 20 байт
      WriteZeros(wrtDBF, 20);

      // Описания полей
      for (int i = 0; i < DBStruct.Count; i++)
      {
        byte[] bName = System.Text.Encoding.ASCII.GetBytes(DBStruct[i].Name);
        wrtDBF.Write(bName);
        WriteZeros(wrtDBF, 11 - bName.Length);

        byte[] bType = System.Text.Encoding.ASCII.GetBytes(new char[1] { DBStruct[i].Type });
        wrtDBF.Write(bType);
        wrtDBF.Write((UInt32)0); // пропуск 4 байта

        if (DBStruct[i].Precision > 0)
        {
          wrtDBF.Write((byte)(DBStruct[i].Length));
          wrtDBF.Write((byte)(DBStruct[i].Precision));
        }
        else
          wrtDBF.Write((UInt16)(DBStruct[i].Length));

        WriteZeros(wrtDBF, 14);
      }
      // Конец списка
      wrtDBF.Write((byte)0x0D);

      // Маркер конца данных
      wrtDBF.Write((byte)0x1A);
    }

    private static void WriteZeros(BinaryWriter wrt, int count)
    {
      for (int i = 0; i < count; i++)
        wrt.Write((byte)0);
    }

    //private static void WriteSpaces(BinaryWriter wrt, int count)
    //{
    //  for (int i = 0; i < count; i++)
    //    wrt.Write((byte)0x20);
    //}

    #endregion

    private void InitDBStructInternal()
    {
      _DBStruct.SetReadOnly(); // блокируем возможность изменения
      _FieldIndices = new Dictionary<string, int>(DBStruct.Count);
      int currOffset = 1; // повторный проход
      _FieldOffsets = new int[DBStruct.Count];
      for (int i = 0; i < DBStruct.Count; i++)
      {
        _FieldOffsets[i] = currOffset;
        currOffset += DBStruct[i].Length;
        _FieldIndices.Add(DBStruct[i].Name, i);
      }
    }

    /// <summary>
    /// Сбрасывает на диск несохраненные изменения и закрывает файлы, если использовался конструктор,
    /// принимающий имя файла.
    /// Если <paramref name="disposing"/>=false, никаких действий не выполняется во избежания
    /// выброса необрабатываемого исключения.
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        FlushRecord();
        FlushHeader();
        DisposeStreams();
      }
      base.Dispose(disposing);
    }

    private void DisposeStreams()
    {
      if (_ShouldDisposeStreams)
      {
        if (_fsDBT != null)
        {
          _fsDBT.Close();
          _fsDBT = null;
        }
        if (_fsDBF != null)
        {
          _fsDBF.Close();
          _fsDBF = null;
        }
      }
    }

    #endregion

    #region Потоки

    /// <summary>
    /// Поток для DBF-файла
    /// </summary>
    private Stream _fsDBF;

    /// <summary>
    /// Поток для DBT-файла
    /// </summary>
    private Stream _fsDBT;

    /// <summary>
    /// Возвращает true, если используется memo-файл.
    /// Если memo-файл не используется, хотя должен быть, то <see cref="HasMemoFile"/>=false, а <see cref="MemoFileMissing"/>=true.
    /// </summary>
    public bool HasMemoFile { get { return _fsDBT != null; } }

    /// <summary>
    /// Нужно ли закрывать потоки при вызове Dispose()
    /// </summary>
    private bool _ShouldDisposeStreams;

    /// <summary>
    /// Выполняет сброс на диск незаписанных данных, вызывая <see cref="System.IO.Stream.Flush()"/>
    /// </summary>
    public void Flush()
    {
      FlushRecord(); // 21.03.2023

      _fsDBF.Flush();
      if (_fsDBT != null)
        _fsDBT.Flush();
    }

    #endregion

    #region Структура таблицы

    /// <summary>
    /// Список полей таблицы
    /// </summary>
    public DbfStruct DBStruct { get { return _DBStruct; } }
    private DbfStruct _DBStruct;

    /// <summary>
    /// Индексы полей для ускорения поиска.
    /// Ключ - имя поля, значение - индекс поля в DBStruct
    /// </summary>
    private Dictionary<string, int> _FieldIndices;

    /// <summary>
    /// Смещения данных для каждого поля.
    /// Длина списка совпадает с DBStruct.Count
    /// </summary>
    private int[] _FieldOffsets;

    /// <summary>
    /// Возвращает смещение заданного поля от начала строки.
    /// Этот метод вряд ли потребуется использовать в прикладном коде.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля в диапазоне от 0 до (<see cref="DbfStruct"/>.Count - 1)</param>
    /// <returns>Смещение поля в буфере записи.</returns>
    public int GetFieldOffset(int fieldIndex)
    {
      return _FieldOffsets[fieldIndex];
    }

    /// <summary>
    /// Версия формата файла.
    /// Определяется автоматически при открытии существующего файла.
    /// Может быть задана в конструкторе при создании нового файла.
    /// </summary>
    public DbfFileFormat Format { get { return _Format; } }
    private DbfFileFormat _Format;

    /// <summary>
    /// Формат файла MEMO.
    /// </summary>
    public DbfMemoFormat MemoFormat { get { return _MemoFormat; } }
    private DbfMemoFormat _MemoFormat;

    /// <summary>
    /// Свойство возвращает true, если структура DBF-файла предполагает наличие MEMO-файла, но DBT-файл отсутствует
    /// </summary>
    public bool MemoFileMissing
    {
      get { return _MemoFormat != DbfMemoFormat.None && _fsDBT == null; }
    }

    /// <summary>
    /// Число записанных блоков мемо-файла
    /// </summary>
    uint _MemoBlockCount;

    /// <summary>
    /// Получить максимальную длину полей.
    /// Возвращает массив, размер которого равен <see cref="DbfStruct.Count"/>.
    /// Для всех полей, кроме 'C' и 'M' возвращается стандартная длина из <see cref="DbfFieldInfo.Length"/>.
    /// Для строковых и MEMO-полей возвращается максимальная длина поля, возможно - 0.
    /// Для больших таблиц рекомендуется использовать перегрузку <see cref="GetMaxLengths(ISimpleSplash)"/>,
    /// чтобы пользователь мош прервать процесс.
    /// </summary>
    /// <returns>Массив длин</returns>
    public int[] GetMaxLengths()
    {
      return GetMaxLengths(null, false);
    }

    /// <summary>
    /// Получить максимальную длину полей.
    /// Возвращает массив, размер которого равен <see cref="DbfStruct.Count"/>.
    /// Для всех полей, кроме 'C' и 'M' возвращается стандартная длина из <see cref="DbfFieldInfo.Length"/>.
    /// Для строковых и MEMO-полей возвращается максимальная длина поля, возможно - 0.
    /// Если пользователь прерывает процесс, выбрасывается исключение <see cref="UserCancelException"/>.
    /// </summary>
    /// <param name="splash">Управляемая splash-заставка для индикации процесса и возможности прерывания.
    /// Если null, то процесс нельзя прервать</param>
    /// <returns>Массив длин</returns>
    public int[] GetMaxLengths(ISimpleSplash splash)
    {
      return GetMaxLengths(splash, false);
    }

    /// <summary>
    /// Получить максимальную длину полей.
    /// Возвращает массив, размер которого равен <see cref="DbfStruct.Count"/>.
    /// Для всех полей, кроме 'C' и 'M' возвращается стандартная длина из <see cref="DbfFieldInfo.Length"/>.
    /// Для строковых и MEMO-полей возвращается максимальная длина поля, возможно - 0.
    /// Эта версия позволяет получить результаты для части таблицы
    /// </summary>
    /// <param name="splash">Управляемая splash-заставка для индикации процесса и возможности прерывания.
    /// Если null, то процесс нельзя прервать</param>
    /// <param name="wheneverUserCancel">Что происходит, если пользователь прерывает процесс.
    /// Ксли true, то возвращается частичный результат, по той части, которую успели просмотреть.
    /// Если false, выбрасывается исключение UserCancelException.</param>
    /// <returns>Массив длин</returns>
    public int[] GetMaxLengths(ISimpleSplash splash, bool wheneverUserCancel)
    {
      int[] a = new int[DBStruct.Count];
      List<int> indices = new List<int>();
      for (int i = 0; i < DBStruct.Count; i++)
      {
        switch (DBStruct[i].Type)
        {
          case 'C':
          case 'M':
            indices.Add(i);
            break;
          default:
            a[i] = DBStruct[i].Length;
            break;
        }
      }

      if (indices.Count == 0)
        return a; // ничего проверять не надо

      try
      {
        if (splash != null)
        {
          splash.PercentMax = RecordCount;
          splash.Percent = 0;
          splash.AllowCancel = true;
        }

        int oldRN = RecNo;
        try
        {
          RecNo = 0;
          while (Read() && indices.Count > 0)
          {
            for (int i = indices.Count - 1; i >= 0; i--)
            {
              int FieldIndex = indices[i];
              int l = GetLength(FieldIndex);
              a[FieldIndex] = Math.Max(a[FieldIndex], l);
              if (DBStruct[FieldIndex].Type == 'C')
              {
                if (l == DBStruct[FieldIndex].Length)
                  // Достигнута максимальная длина строкового поля и дальнейший перебор смысла не имеет
                  indices.RemoveAt(i);
              }
            }
            if (splash != null)
              splash.IncPercent();
          }
        }
        finally
        {
          RecNo = oldRN;
        }
      }
      catch (UserCancelException)
      {
        if (!wheneverUserCancel)
          throw;
      }

      return a;
    }

    #endregion

    #region Кодировка

    /// <summary>
    /// Используемая кодировка. Задается в конструкторе. Если не задана в явном виде, возвращает <see cref="DefaultEncoding"/>
    /// </summary>
    public Encoding Encoding { get { return _Encoding; } }
    private Encoding _Encoding;

    /// <summary>
    /// Возвращает кодировку по умолчанию <see cref="System.Globalization.TextInfo.OEMCodePage"/>.
    /// </summary>
    public static Encoding DefaultEncoding
    {
      get
      {
        int codePage = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
        return Encoding.GetEncoding(codePage);
      }
    }

    #endregion

    #region Поля для поиска записи

    /// <summary>
    /// Число записей
    /// </summary>
    public int RecordCount { get { return _RecordCount; } }
    private int _RecordCount;

    /// <summary>
    /// Смещение от начала файла до начала данных
    /// </summary>
    private ushort _DataOffset;

    /// <summary>
    ///  Размер одной записи
    /// </summary>
    private ushort _RecSize;

    #endregion

    #region Перебор записей

    /// <summary>
    /// Номер текущей записи, начиная с 1.
    /// При инициализации устанавливается равным 0.
    /// Установка нулевого значения допускается, если требуется запуск цикла Read() после выполнения других операций.
    /// </summary>
    public int RecNo
    {
      get { return _RecNo; }
      set
      {
        if (value < 0 || value > _RecordCount)
          throw new ArgumentOutOfRangeException(String.Format(Res.DbfFile_Arg_RecNoOutOfRange, value, _RecordCount));

        if (value == _RecNo)
          return;

        FlushRecord();

        _RecNo = value;
        if (value == 0)
          DataTools.FillArray<byte>(_RecordBuffer, 0);
        else
        {
          _fsDBF.Position = _DataOffset + (value - 1) * _RecSize;
          _fsDBF.Read(_RecordBuffer, 0, _RecSize);
        }
      }
    }
    private int _RecNo;

    /// <summary>
    /// Необходимость пропуска удаленных записей методом <see cref="Read()"/>.
    /// По умолчанию - true - записи пропускаются.
    /// </summary>
    public bool SkipDeleted { get { return _SkipDeleted; } set { _SkipDeleted = value; } }
    private bool _SkipDeleted;

    /// <summary>
    /// Метод для перебора записей в стиле <see cref="System.Data.Common.DbDataReader"/>.
    /// Строки, помеченные на удаление, пропускаются, если <see cref="SkipDeleted"/>=false.
    /// Если требуется выполнить цикл повторно, или до этого выполнялись другие действия, устанавливающие текущую позицию,
    /// установите перед выполнением цикла свойство <see cref="RecNo"/>=0.
    /// </summary>
    /// <returns>true, если получена очередная запись, false, если достигнут конец файла</returns>
    public bool Read()
    {
      while (RecNo < RecordCount)
      {
        RecNo++;
        if ((!RecordDeleted) || (!SkipDeleted))
          return true;
      }
      return false;
    }

    #endregion

    #region Чтение значений полей

    /// <summary>
    /// Буфер строки таблицы размером <see cref="DbfStruct.RecordSize"/>.
    /// Обычно нет смысла использовать это свойство из прикладного поля.
    /// Прямая модификация буфера может привести к порче файла, особенно при наличии memo-файла.
    /// При модификации буфера также должно устанавливаться свойство <see cref="RecordModified"/>=true.
    /// </summary>
    public byte[] RecordBuffer { get { return _RecordBuffer; } }
    private byte[] _RecordBuffer;

    private void CheckActiveRecord()
    {
      if (_RecordBuffer == null)
        throw new InvalidOperationException(Res.DbfFile_Err_CheckActiveRecord);
    }

    /// <summary>
    /// true - если текущая запись помечена на удаление.
    /// При обычном переборе методом <see cref="Read()"/>, если свойство <see cref="SkipDeleted"/> не сброшено в false, удаленные записи
    /// пропускаются.
    /// </summary>
    public bool RecordDeleted
    {
      get
      {
        return _RecordBuffer[0] == '*';
      }
      set
      {
        CheckNotReadOnly();
#if DEBUG
        CheckActiveRecord();
#endif
        if (value == RecordDeleted)
          return; // не изменилось

        if (value)
          _RecordBuffer[0] = (byte)'*';
        else
          _RecordBuffer[0] = (byte)' ';

        _RecordModified = true;
        _TableModified = true;
      }
    }

    /// <summary>
    /// Получить значение поля с заданным индексом.
    /// Для пустых значений возвращается <see cref="System.DBNull"/> при любом типе поля, включая 'N' и 'L'.
    /// Для поля типа 'C' и 'M' возвращает строковое значение, при этом пробелы справа обрезаются.
    /// Для поля типа 'N' возвращает значение типа <see cref="System.Int32"/>, <see cref="System.Int64"/> или <see cref="System.Decimal"/>.
    /// Для поля типа 'F' возвращает значение типа <see cref="System.Double"/>.
    /// Для поля типа 'L' возвращает true или false.
    /// Для поля типа 'D' возвращает <see cref="System.DateTime"/>.
    /// Обычно удобнее использовать методы, возвращающие типизированное значение. Например, метод <see cref="GetInt(string)"/> возвращает числовое значение в том числе и для пустого поля.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public object GetValue(string fieldName)
    {
      return GetValue(InternalIndexOfField(fieldName));
    }


    /// <summary>
    /// Получить значение поля с заданным индексом.
    /// Для пустых значений возвращается <see cref="System.DBNull"/> при любом типе поля, включая 'N' и 'L'.
    /// Для поля типа 'C' и 'M' возвращает строковое значение, при этом пробелы справа обрезаются.
    /// Для поля типа 'N' возвращает значение типа <see cref="System.Int32"/>, <see cref="System.Int64"/> или <see cref="System.Decimal"/>.
    /// Для поля типа 'F' возвращает значение типа <see cref="System.Double"/>.
    /// Для поля типа 'L' возвращает true или false.
    /// Для поля типа 'D' возвращает <see cref="System.DateTime"/>.
    /// Обычно удобнее использовать методы, возвращающие типизированное значение. Например, метод <see cref="GetInt(int)"/> возвращает числовое значение в том числе и для пустого поля.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public object GetValue(int fieldIndex)
    {
      string s = GetString(fieldIndex);
      if (s.Length == 0)
        return DBNull.Value;
      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
          try
          {
            s = s.TrimStart(' ');
            if (DBStruct[fieldIndex].Precision > 0)
              return decimal.Parse(s, StdConvert.NumberFormat);
            else if (DBStruct[fieldIndex].Length > 9)
              return long.Parse(s);
            else
              return int.Parse(s);
          }
          catch (Exception e)
          {
            throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
          }
        case 'F':
          try
          {
            s = s.TrimStart(' ');
            return double.Parse(s, StdConvert.NumberFormat);
          }
          catch (Exception e)
          {
            throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
          }
        case 'L':
          return GetBool(fieldIndex); // 18.03.2017
        case 'D':
          return GetNullableDate(fieldIndex);
        default:
          return s;
      }
    }

    /// <summary>
    /// Получить строковое значение поля.
    /// Для пустых значений возвращается <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public string GetString(string fieldName)
    {
      return GetString(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить строковое значение поля с заданным индексом.
    /// Для пустых значений возвращается <see cref="String.Empty"/>.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public string GetString(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      if (DBStruct[fieldIndex].Type == 'M')
      {
        // Обработка MEMO-поля
        uint memoBlockEntry = ReadMemoEntry(fieldIndex);
        if (memoBlockEntry == 0)
          return String.Empty;

        if (_fsDBT == null)
          throw new DbfMemoFileMissingException(String.Format(Res.DbfFile_Err_MemoFileNotAttached, DBStruct[fieldIndex].Name));

        byte[] b = ReadMemoValue(memoBlockEntry, DBStruct[fieldIndex].Name);
        return Encoding.GetString(b);
      }
      else
      {
        // 18.03.2017
        // OpenOffice любит записывать символы \0 вместо пробелов, с целью показать значение NULL
        if (_RecordBuffer[_FieldOffsets[fieldIndex]] == '\0')
          return String.Empty;

        string s;
        if (DBStruct[fieldIndex].Type == 'C')
          s = Encoding.GetString(_RecordBuffer, _FieldOffsets[fieldIndex], DBStruct[fieldIndex].Length);
        else
          s = System.Text.Encoding.ASCII.GetString(_RecordBuffer, _FieldOffsets[fieldIndex], DBStruct[fieldIndex].Length);// 11.03.2024

        s = s.TrimEnd(' ');
        return s;
      }
    }

    private void CheckFieldIndex(int fieldIndex)
    {
      if (fieldIndex < 0 || fieldIndex >= DBStruct.Count)
        throw new ArgumentOutOfRangeException("fieldIndex", fieldIndex, String.Format(Res.DbfStr_Arg_FieldIndexOutOfRange,DBStruct.Count - 1));
    }

    /// <summary>
    /// Используется для полей 'N', 'L', 'D', 'F'.
    /// Для них не нужно использовать кодировку, заданную свойством <see cref="Encoding"/>.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns></returns>
    private string GetAsciiString(int fieldIndex)
    {
      // OpenOffice любит записывать символы \0 вместо пробелов, с целью показать значение NULL
      if (_RecordBuffer[_FieldOffsets[fieldIndex]] == '\0')
        return String.Empty;

      string s = System.Text.Encoding.ASCII.GetString(_RecordBuffer, _FieldOffsets[fieldIndex], DBStruct[fieldIndex].Length);
      return s.Trim(' ');
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Если есть дробная часть числа, то она отбрасывается.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public int GetInt(string fieldName)
    {
      return GetInt(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом.
    /// Если есть дробная часть числа, то она отбрасывается.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns></returns>
    public int GetInt(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif
      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
        case 'C':
        case 'M': // 11.03.2024
          try
          {
            string s = GetString(fieldIndex);
            int p = s.IndexOf('.');
            if (p >= 0)
              s = s.Substring(0, p); // 22.03.2023
            if (s.Length == 0)
              return 0; // 21.03.2017
            return int.Parse(s);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N' || DBStruct[fieldIndex].Type == 'F')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        case 'L': // 11.03.2024
          bool vBool = GetBool(fieldIndex);
          return vBool ? 1 : 0;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Если есть дробная часть числа, то она отбрасывается.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public long GetInt64(string fieldName)
    {
      return GetInt64(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить строковое значение поля с заданным индексом.
    /// Если есть дробная часть числа, то она отбрасывается.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns></returns>
    public long GetInt64(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
        case 'C':
        case 'M':
          try
          {
            string s = GetString(fieldIndex);
            int p = s.IndexOf('.');
            if (p >= 0)
              s = s.Substring(0, p); // 22.03.2023
            if (s.Length == 0)
              return 0L;
            return long.Parse(s);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N' || DBStruct[fieldIndex].Type == 'F')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        case 'L': // 11.03.2024
          bool vBool = GetBool(fieldIndex);
          return vBool ? 1L : 0L;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public float GetSingle(string fieldName)
    {
      return GetSingle(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public float GetSingle(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
        case 'C':
        case 'M': // 11.03.2024
          try
          {
            string s = GetString(fieldIndex);
            if (s.Length == 0)
              return 0f; // 21.03.2017
            return float.Parse(s, StdConvert.NumberFormat);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N' || DBStruct[fieldIndex].Type == 'F')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }

        case 'L': // 11.03.2024
          bool vBool = GetBool(fieldIndex);
          return vBool ? 1f : 0f;

        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public double GetDouble(string fieldName)
    {
      return GetDouble(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns></returns>
    public double GetDouble(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
        case 'C':
        case 'M':
          try
          {
            string s = GetString(fieldIndex);
            if (s.Length == 0)
              return 0.0; // 21.03.2017
            return double.Parse(s, StdConvert.NumberFormat);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N' || DBStruct[fieldIndex].Type == 'F')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        case 'L': // 11.03.2024
          bool vBool = GetBool(fieldIndex);
          return vBool ? 1.0 : 0.0;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(string fieldName)
    {
      return GetDecimal(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом.
    /// Для пустых значений возвращается 0.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
        case 'C':
        case 'M':
          try
          {
            string s = GetString(fieldIndex);
            if (s.Length == 0)
              return 0m; // 21.03.2017
            return decimal.Parse(s, StdConvert.NumberFormat);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N' || DBStruct[fieldIndex].Type == 'F')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        case 'L': // 11.03.2024
          bool vBool = GetBool(fieldIndex);
          return vBool ? 1m : 0m;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить логическое значение поля.
    /// Для пустых значений возвращается false.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public bool GetBool(string fieldName)
    {
      return GetBool(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить логическое значение поля.
    /// Для пустых значений возвращается false.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public bool GetBool(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'L':
          byte b = _RecordBuffer[_FieldOffsets[fieldIndex]];
          try
          {
            return BoolFromChar((char)b) ?? false;
          }
          catch (Exception e)
          {
            throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
          }
        case 'N':
        case 'F':
          decimal mValue1 = GetDecimal(fieldIndex);
          return mValue1 != 0m;
        case 'C':
        case 'M':
          string value = GetString(fieldIndex);
          if (value.Length == 0)
            return false;
          if ("1234567890+-".IndexOf(value[0]) >= 0)
          {
            decimal mValue2 = GetDecimal(fieldIndex);
            return mValue2 != 0m;
          }
          else if (value.Length == 1)
            return BoolFromChar(value[0]) ?? false;
          else
            throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), null);

        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Внутренний метод
    /// </summary>
    /// <param name="ch">Символ из DBF-файла</param>
    /// <returns></returns>
    internal static bool? BoolFromChar(char ch)
    {
      switch (ch)
      {
        case 'T':
        case 't':
        case 'Y':
        case 'y':
          return true;
        case 'F':
        case 'f':
        case 'N':
        case 'n':
          return false;
        case ' ':
        case '?':
        case '\0': // 18.03.2017
          return null;
        default:
          throw ExceptionFactory.ArgUnknownValue("ch", ch);
      }
    }

    /// <summary>
    /// Получить значение поля даты.
    /// Для пустых значений возвращается null.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public DateTime? GetNullableDate(string fieldName)
    {
      return GetNullableDate(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить значение поля типа даты с заданным индексом.
    /// Для пустых значений возвращается null.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public DateTime? GetNullableDate(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'D':
        case 'C': // 11.03.2024
        case 'M': // 11.03.2024
          try
          {
            string s = GetString(fieldIndex);
            if (s.Length == 0)
              return null;
            else
              return DateTime.ParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture);
          }
          catch (Exception e)
          {
            throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
          }
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить значение поля даты.
    /// Для пустого значения возвращается <see cref="DateTime.MinValue"/>.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public DateTime GetDate(string fieldName)
    {
      return GetDate(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить значение поля типа даты с заданным индексом.
    /// Для пустого значения возвращается <see cref="DateTime.MinValue"/>.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Значение</returns>
    public DateTime GetDate(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'D':
        case 'C': // 11.03.2024
        case 'M': // 11.03.2024
          try
          {
            string s = GetString(fieldIndex);
            if (s.Length == 0)
              return DateTime.MinValue;
            else
              return DateTime.ParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture);
          }
          catch (Exception e)
          {
            throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
          }
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    private int InternalIndexOfField(string fieldName)
    {
      if (String.IsNullOrEmpty(fieldName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("fieldName"); // 27.12.2020 Перенесено в начало

      int fieldPos;
      if (!_FieldIndices.TryGetValue(fieldName.ToUpperInvariant(), out fieldPos))
        throw new ArgumentException(String.Format(Res.DbfStr_Arg_UnknownFieldName, fieldName), "fieldName");
      return fieldPos;
    }

    private uint ReadMemoEntry(int fieldIndex)
    {
      int lastPos = _FieldOffsets[fieldIndex] + 9;
      int nc = 0;
      while (nc < 10)
      {
        byte b = _RecordBuffer[lastPos - nc];
        if (b >= (byte)'0' && b <= (byte)'9')
          nc++;
        else
          break;
      }

      if (nc == 0)
        return 0;

      string s = Encoding.ASCII.GetString(_RecordBuffer, lastPos - nc + 1, nc);
      return uint.Parse(s, StdConvert.NumberFormat);
    }

    private byte[] ReadMemoValue(uint memoBlockEntry, string fieldName)
    {
      if (memoBlockEntry < MinMemoBlockCount || memoBlockEntry > MaxMemoBlockCount)
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_MemoInvalidBlockNumber,
          RecNo,fieldName, memoBlockEntry));

      long fileOff = (long)memoBlockEntry * 512L;
      if (_fsDBT.Length <= fileOff)
        throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_MemoBlockNumberOutOfFile,
          RecNo, fieldName, memoBlockEntry));

      // Сначала нужно найти символ 1A, который завершает текст
      _fsDBT.Position = fileOff;
      int cnt = 0;
      while (true)
      {
        int v = _fsDBT.ReadByte();
        if (v < 0)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_MemoUnexpectedEOF,
            RecNo, fieldName, memoBlockEntry));
        if (v == 0x1A)
          break;
        cnt++;
        if (cnt > 65535)
          throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_MemoNoEndOfBlock,
            RecNo, fieldName, memoBlockEntry));
      }

      // Теперь можно прочитать все байты
      _fsDBT.Position = fileOff;
      byte[] buff = new byte[cnt];
      if (_fsDBT.Read(buff, 0, cnt) != cnt)
        throw new IOException(String.Format(Res.DbfFile_Err_MemoReadError,
          RecNo, fieldName));
      return buff;
    }

    /// <summary>
    /// Возвращает true, если поле содержит значение NULL.
    /// Внутренняя структура DBF-файла позволяет отличать значения NULL от 0 и false, но не от пустой строки.
    /// Однако СУБД, например, Clipper, могут не различать значения NULL.
    /// Обычно не следует использовать этот метод в прикладном коде.
    /// Возвращает true, если все позиции поля в строке содержат символы "пробел" (0x20)
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>true, если поле не содержит значение</returns>
    public bool IsDBNull(string fieldName)
    {
      return IsDBNull(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Возвращает true, если поле содержит значение NULL.
    /// Внутренняя структура DBF-файла позволяет отличать значения NULL от 0 и false, но не от пустой строки.
    /// Однако СУБД, например, Clipper, могут не различать значения NULL.
    /// Обычно не следует использовать этот метод в прикладном коде.
    /// Возвращает true, если все позиции поля в строке содержат символы "пробел" (0x20)
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>true, если поле не содержит значения</returns>
    public bool IsDBNull(int fieldIndex)
    {
      //string s = Encoding.GetString(_RecordBuffer, _FieldOffsets[fieldIndex], DBStruct[fieldIndex].Length);
      //s = s.Trim(' ');
      //return s.Length == 0;

      int p1 = _FieldOffsets[fieldIndex];
      int p2 = p1 + DBStruct[fieldIndex].Length;
      for (int i = p1; i < p2; i++)
      {
        switch (_RecordBuffer[i])
        {
          case 0:
          case 32:
            break;
          default:
            return false;
        }
      }
      return true;
    }


    /// <summary>
    /// Возвращает длину заданного поля.
    /// Для полей типа 'C' возвращает длину заполненной части строки (от 0 до <see cref="DbfFieldInfo.Length"/>).
    /// Для полей типа 'M' возвращает длину значения MEMO-поля.
    /// Возвращается длина в байтах с учетом используемой кодировки <see cref="Encoding"/>, а не длина строки.
    /// Для остальных полей возвращает <see cref="DbfFieldInfo.Length"/>, если поле заполнено и 0, если поле пустое.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Длина</returns>
    public int GetLength(string fieldName)
    {
      return GetLength(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Возвращает длину заданного поля.
    /// Для полей типа 'C' возвращает длину заполненной части строки (от 0 до <see cref="DbfFieldInfo.Length"/>).
    /// Для полей типа 'M' возвращает длину значения MEMO-поля.
    /// Возвращается длина в байтах с учетом используемой кодировки <see cref="Encoding"/>, а не длина строки.
    /// Для остальных полей возвращает <see cref="DbfFieldInfo.Length"/>, если поле заполнено и 0, если поле пустое.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <returns>Длина</returns>
    public int GetLength(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'C':
          for (int i = DBStruct[fieldIndex].Length - 1; i >= 0; i--)
          {
            if (_RecordBuffer[_FieldOffsets[fieldIndex] + i] != (byte)' ')
              return i + 1;
          }
          return 0;

        case 'M':
          uint memoBlockEntry = ReadMemoEntry(fieldIndex);
          if (memoBlockEntry == 0)
            return 0;
          if (_fsDBT == null)
            return 0;

          if (memoBlockEntry < MinMemoBlockCount || memoBlockEntry > MaxMemoBlockCount)
            return 0;

          long fileOff = (long)memoBlockEntry * 512L;
          if (_fsDBT.Length <= fileOff)
            return 0;

          // Сначала нужно найти символ 1A, который завершает текст
          _fsDBT.Position = fileOff;
          int cnt = 0;
          while (true)
          {
            int v = _fsDBT.ReadByte();
            if (v < 0)
              throw new DbfFileFormatException(String.Format(Res.DbfFile_Err_MemoUnexpectedEOF,
                RecNo, DBStruct[fieldIndex].Name, memoBlockEntry));
            if (v == 0x1A)
              break;
            cnt++;
          }
          return cnt;

        default:
          if (IsDBNull(fieldIndex))
            return 0;
          else
            return DBStruct[fieldIndex].Length;
      }
    }

    #endregion

    #region Запись значений полей

    /// <summary>
    /// Возвращает true, если не допускается добавление или изменение записей
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// Вызывается пишущими методами.
    /// </summary>
    protected void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      //throw new NotSupportedException("Свойство DbfFile.IsReadOnly устанавливается только в конструкторе и не может быть изменено в дальнейшем");
      CheckNotReadOnly();
    }


    /// <summary>
    /// Возвращает true, если было установлено значение какого-либо поля в текущей записи
    /// </summary>
    internal bool RecordModified
    {
      get { return _RecordModified; }
      set
      {
        CheckNotReadOnly();
#if DEBUG
        CheckActiveRecord();
#endif

        _RecordModified = value;
      }
    }
    private bool _RecordModified;

    /// <summary>
    /// Возвращает true, если была выполнена модификация каких-либо данных
    /// </summary>
    internal bool TableModified { get { return _TableModified; } }
    private bool _TableModified;

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// Выполняет вызов SetNull(), SetString(), SetInt(),..., в зависимости от переданного значения.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetValue(string fieldName, object value)
    {
      SetValue(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// Выполняет вызов SetNull(), SetString(), SetInt(),..., в зависимости от переданного значения.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetValue(int fieldIndex, object value)
    {
      if (value == null)
      {
        SetNull(fieldIndex);
        return;
      }
      if (value is DBNull)
      {
        SetNull(fieldIndex);
        return;
      }

      if (value is string)
      {
        SetString(fieldIndex, (string)value);
        return;
      }

      if (value is DateTime)
      {
        SetDate(fieldIndex, (DateTime)value);
        return;
      }
      if (value is Boolean)
      {
        SetBool(fieldIndex, (bool)value);
        return;
      }

      if (value is Single)
      {
        SetSingle(fieldIndex, (float)value);
        return;
      }

      if (value is Double)
      {
        SetDouble(fieldIndex, (double)value);
        return;
      }

      decimal x = DataTools.GetDecimal(value);
      SetDecimal(fieldIndex, x);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// Если значение превышает размер поля, то конец строки обрезается.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetString(string fieldName, string value)
    {
      SetString(InternalIndexOfField(fieldName), value, true);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    /// <param name="trim">Если true и значение превышает размер поля, то конец строки обрезается.
    /// Если false, то выбрасывается исключение <see cref="ArgumentOutOfRangeException"/></param>
    public void SetString(string fieldName, string value, bool trim)
    {
      SetString(InternalIndexOfField(fieldName), value, trim);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// Если значение превышает размер поля, то конец строки обрезается.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetString(int fieldIndex, string value)
    {
      SetString(fieldIndex, value, true);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
    /// <param name="value">Значение поля</param>
    /// <param name="trim">Если true и значение превышает размер поля, то конец строки обрезается.
    /// Если false, то выбрасывается исключение <see cref="ArgumentOutOfRangeException"/></param>
    public void SetString(int fieldIndex, string value, bool trim)
    {
      #region Обработка пустого значения

      if (value == null)
        value = String.Empty;
      value = value.TrimEnd(' ');
      if (value.Length == 0)
      {
        SetNull(fieldIndex);
        return;
      }

      #endregion

      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      int fieldLength = DBStruct[fieldIndex].Length;

      switch (DBStruct[fieldIndex].Type)
      {
        case 'M':
          byte[] bytes = this.Encoding.GetBytes(value);
          uint memoBlockEntry = WriteMemoValue(fieldIndex, bytes);
          value = memoBlockEntry.ToString("d10", StdConvert.NumberFormat);
          System.Text.Encoding.ASCII.GetBytes(value, 0, fieldLength, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;

        case 'C':
          int byteCount = Encoding.GetByteCount(value);
          if (byteCount > fieldLength)
          {
            if (!trim)
              throw new ArgumentOutOfRangeException(
                "value", value,
                String.Format(Res.DbfFile_Arg_StringIsTooLong,
                value.Length, DBStruct[fieldIndex].Name, fieldLength));
            // Нельзя сразу писать в буфер строки, так как затрутся поля справа.
            byte[] b2 = this.Encoding.GetBytes(value);
            Array.Copy(b2, 0, _RecordBuffer, _FieldOffsets[fieldIndex], fieldLength);
          }
          else
          {
            // Нельзя использовать PadRight() для дополнения пробелами до преобразования строки
            this.Encoding.GetBytes(value, 0, value.Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
            for (int i = byteCount; i < fieldLength; i++)
              _RecordBuffer[_FieldOffsets[fieldIndex] + i] = 0x20;
          }
          break;
        default:
          if (value.Length > DBStruct[fieldIndex].Length && (!trim))
            throw new ArgumentOutOfRangeException(
              "value", value,
                String.Format(Res.DbfFile_Arg_StringIsTooLong,
                value.Length, DBStruct[fieldIndex].Name, fieldLength));
          value = DataTools.PadRight(value, DBStruct[fieldIndex].Length);
          System.Text.Encoding.ASCII.GetBytes(value, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Запись значения в MEMO-поле
    /// </summary>
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0</param>
    /// <param name="value">Значение</param>
    private uint WriteMemoValue(int fieldIndex, byte[] value)
    {
      if (_fsDBT == null)
        throw new DbfMemoFileMissingException(String.Format(Res.DbfFile_Err_MemoFileNotAttached, DBStruct[fieldIndex].Name));

      // Число необходимых блоков
      int nBlocks = (value.Length + 511 + 1) / 512; // 1 байт для 0x1A

      uint firstBlock = ReadMemoEntry(fieldIndex);
      int availBlocks = GetAvailMemoBlocks(firstBlock);

      if (availBlocks >= nBlocks)
      {
        // Можно заменить существующий блок
        _fsDBT.Position = (long)firstBlock * 512L;
        _fsDBT.Write(value, 0, value.Length);
        _fsDBT.WriteByte(0x1A);

        // Затираем нулями старые значения
        long sz = (long)(availBlocks) * 512;
        long nZeros = sz - value.Length - 1;
        FileTools.WriteZeros(_fsDBT, nZeros);
        return firstBlock; // Возвращаем существующее значение
      }

      firstBlock = _MemoBlockCount;
      if ((_MemoBlockCount + nBlocks) > MaxMemoBlockCount)
        throw new InvalidOperationException(String.Format(Res.DbfFile_Err_MemoBlockCountLimitExceeded,MaxMemoBlockCount));

      // Дополняем нулями, если файл короче, чем надо
      FillMemoToBlockCount();

      // Записываем значение
      _fsDBT.Write(value, 0, value.Length);
      _fsDBT.WriteByte(0x1A);

      _MemoBlockCount += (uint)nBlocks;
      // Дополняем
      FillMemoToBlockCount();

      return firstBlock;
    }

    /// <summary>
    /// Возвращает число доступных блоков DBT-файла в цепочке, начинающейся с MemoBlockEntry
    /// </summary>
    /// <param name="memoBlockEntry">Номер первого MEMO-блока</param>
    /// <returns></returns>
    private int GetAvailMemoBlocks(uint memoBlockEntry)
    {
      if (memoBlockEntry == 0)
        return 0;

      long off = (long)memoBlockEntry * 512L;
      if (off > _fsDBT.Length)
        return 0; // ошибка

      _fsDBT.Position = off;
      long cnt = 0;
      while (true)
      {
        int b = _fsDBT.ReadByte();
        if (b < 0)
          break; // Неожиданный конец файла

        if (b == 0x1A)
          break;

        cnt++;
      }

      return (int)((cnt + 511L) / 512L);
    }

    private void FillMemoToBlockCount()
    {
      long sz = (long)_MemoBlockCount * 512;
      //long Delta = fsDBT.Length - sz;
      if (sz > _fsDBT.Length)
        _fsDBT.SetLength(sz);

      // Позиционируемся на конец файла
      _fsDBT.Seek(0, SeekOrigin.End);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// Если строковое представление значения не помещается в поле, выбрасывается исключение <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetInt(string fieldName, int value)
    {
      SetInt(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetInt(int fieldIndex, int value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
          string s = value.ToString(DBStruct[fieldIndex].Mask, StdConvert.NumberFormat);
          if (s.Length > DBStruct[fieldIndex].Length)
            throw new ArgumentOutOfRangeException("value", value,
              String.Format(Res.DbfFile_Err_NumberStringTooLong, s, s.Length, DBStruct[fieldIndex].Name, DBStruct[fieldIndex].Length));
          s = s.PadLeft(DBStruct[fieldIndex].Length, ' ');
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetInt64(string fieldName, long value)
    {
      SetInt64(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetInt64(int fieldIndex, long value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
          string s = value.ToString(DBStruct[fieldIndex].Mask, StdConvert.NumberFormat);
          if (s.Length > DBStruct[fieldIndex].Length)
            throw new ArgumentOutOfRangeException("value", value,
              String.Format(Res.DbfFile_Err_NumberStringTooLong, s, s.Length, DBStruct[fieldIndex].Name, DBStruct[fieldIndex].Length));
          s = s.PadLeft(DBStruct[fieldIndex].Length, ' ');
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetSingle(string fieldName, float value)
    {
      SetSingle(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetSingle(int fieldIndex, float value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
          string s = value.ToString(DBStruct[fieldIndex].Mask, StdConvert.NumberFormat);
          if (s.Length > DBStruct[fieldIndex].Length)
            throw new ArgumentOutOfRangeException("value", value,
              String.Format(Res.DbfFile_Err_NumberStringTooLong, s, s.Length, DBStruct[fieldIndex].Name, DBStruct[fieldIndex].Length));
          s = s.PadLeft(DBStruct[fieldIndex].Length, ' ');
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetDouble(string fieldName, double value)
    {
      SetDouble(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetDouble(int fieldIndex, double value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
          string s = value.ToString(DBStruct[fieldIndex].Mask, StdConvert.NumberFormat);
          if (s.Length > DBStruct[fieldIndex].Length)
            throw new ArgumentOutOfRangeException("value", value,
              String.Format(Res.DbfFile_Err_NumberStringTooLong, s, s.Length, DBStruct[fieldIndex].Name, DBStruct[fieldIndex].Length));
          s = s.PadLeft(DBStruct[fieldIndex].Length, ' ');
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetDecimal(string fieldName, decimal value)
    {
      SetDecimal(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetDecimal(int fieldIndex, decimal value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'F':
          string s = value.ToString(DBStruct[fieldIndex].Mask, StdConvert.NumberFormat);
          if (s.Length > DBStruct[fieldIndex].Length)
            throw new ArgumentOutOfRangeException("value", value,
              String.Format(Res.DbfFile_Err_NumberStringTooLong, s, s.Length, DBStruct[fieldIndex].Name, DBStruct[fieldIndex].Length));
          s = s.PadLeft(DBStruct[fieldIndex].Length, ' ');
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetBool(string fieldName, bool value)
    {
      SetBool(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetBool(int fieldIndex, bool value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'L':
          string s = value ? "T" : "F";
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetNullableDate(string fieldName, DateTime? value)
    {
      SetNullableDate(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetNullableDate(int fieldIndex, DateTime? value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'D':
          string s;
          if (value.HasValue)
            s = value.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
          else
            s = new string(' ', 8);
          System.Text.Encoding.ASCII.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetDate(string fieldName, DateTime value)
    {
      SetDate(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    /// <param name="value">Значение поля</param>
    public void SetDate(int fieldIndex, DateTime value)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'D':
          string s1 = value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
          System.Text.Encoding.ASCII.GetBytes(s1, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        case 'C':
        case 'M':
          string s2 = value.ToString(value.TimeOfDay.Ticks == 0L ? @"yyyyMMdd" : @"yyyyMMdd\-hhMMss", CultureInfo.InvariantCulture);
          System.Text.Encoding.ASCII.GetBytes(s2, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
          break;
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }

      _RecordModified = true;
      _TableModified = true;
    }

    /// <summary>
    /// Устанавливает значение поля равным NULL для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    public void SetNull(string fieldName)
    {
      SetNull(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Устанавливает значение поля равным NULL для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до (<see cref="DBStruct"/>.Count-1)</param>
    public void SetNull(int fieldIndex)
    {
      CheckNotReadOnly();
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      // Заполняем пробелами
      for (int i = 0; i < DBStruct[fieldIndex].Length; i++)
        _RecordBuffer[_FieldOffsets[fieldIndex] + i] = (byte)0x20;

      _RecordModified = true;
      _TableModified = true;
    }

    #endregion

    #region Добавление записи

    /// <summary>
    /// Возвращает true, если был вызов <see cref="AppendRecord()"/>
    /// </summary>
    public bool IsNewRecord { get { return _IsNewRecord; } }
    private bool _IsNewRecord;

    /// <summary>
    /// Добавляет новую запись в конец файла.
    /// Предварительно записываются несохраненные изменения.
    /// </summary>
    public void AppendRecord()
    {
      CheckNotReadOnly();
      FlushRecord();

      DataTools.FillArray<byte>(_RecordBuffer, (byte)' ');
      _RecordCount++;
      _RecNo = _RecordCount;
      _IsNewRecord = true;
      _TableModified = true;
    }

    private void FlushRecord()
    {
      if (IsNewRecord)
      {
        //fsDBF.Position = DataOffset + (FRecNo - 1) * RecSize;
        _fsDBF.Position = (long)_DataOffset + ((long)_RecNo - 1L) * (long)_RecSize; // 27.11.2018
        _fsDBF.Write(_RecordBuffer, 0, _RecSize);
        _fsDBF.WriteByte((byte)0x1A);

        _IsNewRecord = false;
        _RecordModified = false;
        return;
      }

      if (_RecordModified)
      {
        //fsDBF.Position = DataOffset + (FRecNo - 1) * RecSize;
        _fsDBF.Position = (long)_DataOffset + ((long)_RecNo - 1L) * (long)_RecSize; // 27.11.2018
        _fsDBF.Write(_RecordBuffer, 0, _RecSize);
        _RecordModified = false;
        return;
      }
    }

    /// <summary>
    /// Модификация заголовка для отображения числа записей и даты
    /// </summary>
    private void FlushHeader()
    {
      if (!_TableModified)
        return;

      if (_Format == DbfFileFormat.dBase2)
        FlushHeaderDBF2();
      else
        FlushHeaderDBF34();
    }

    private void FlushHeaderDBF2()
    {
      // Число записей
      _fsDBF.Position = 1L;
      BinaryWriter wrtDBF = new BinaryWriter(_fsDBF);
      wrtDBF.Write((UInt16)RecordCount);

      // Дата изменения MDY
      DateTime dt = DateTime.Today;
      int year2 = dt.Year % 100;
      wrtDBF.Write((byte)(dt.Month));
      wrtDBF.Write((byte)(dt.Day));
      wrtDBF.Write((byte)year2);
    }

    private void FlushHeaderDBF34()
    {
      // Дата изменения YMD
      DateTime dt = DateTime.Today;
      int year2 = dt.Year % 100;
      _fsDBF.Position = 1L;
      _fsDBF.WriteByte((byte)year2);
      _fsDBF.WriteByte((byte)(dt.Month));
      _fsDBF.WriteByte((byte)(dt.Day));

      // Число записей
      _fsDBF.Position = 4L;
      BinaryWriter wrtDBF = new BinaryWriter(_fsDBF);
      wrtDBF.Write((uint)RecordCount);

      if (_fsDBT != null)
      {
        _fsDBT.Position = 0L;
        BinaryWriter wrtDbt = new BinaryWriter(_fsDBT);
        wrtDbt.Write(_MemoBlockCount);
      }
    }

    #endregion

    #region Преобразование в DataTable

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать все поля DBF-файла.
    /// Для больших DBF-файлов можно использовать метод <see cref="CreateBlockedTable(int, ErrorMessageList, out DataTable)"/> для загрузки таблицы по частям.
    /// </summary>
    /// <returns>Заполненный объект <see cref="System.Data.DataTable"/></returns>
    public DataTable CreateTable()
    {
      return CreateTable(new DummySplash(), null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать все поля DBF-файла.
    /// </summary>
    /// <param name="splash">Интерфейс управления процентным инидкатором.
    /// Если свойство <see cref="ISimpleSplash.AllowCancel"/> установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <returns>Заполненный объект <see cref="System.Data.DataTable"/></returns>
    public DataTable CreateTable(ISimpleSplash splash)
    {
      return CreateTable(splash, null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать все поля DBF-файла.
    /// Эта версия позволяет пропускать ошибочные значения полей, которые не соответствуют типу данных.
    /// Для больших DBF-файлов можно использовать метод <see cref="CreateBlockedTable(int, ErrorMessageList, out DataTable)"/> для загрузки таблицы по частям.
    /// </summary>
    /// <param name="splash">Интерфейс управления процентным инидкатором.
    /// Если свойство <see cref="ISimpleSplash.AllowCancel"/> установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <returns>Заполненный объект <see cref="System.Data.DataTable"/></returns>
    public DataTable CreateTable(ISimpleSplash splash, ErrorMessageList errors)
    {
      if (splash == null)
        splash = new DummySplash();

      DataTable table = DBStruct.CreateTable();
      int oldRN = RecNo;
      try
      {
        RecNo = 0;
        splash.PercentMax = this.RecordCount;
        while (Read())
        {
          AddDataTableRow(table, errors);
          splash.IncPercent();
        }
      }
      finally
      {
        RecNo = oldRN;
      }
      return table;
    }

    private void AddDataTableRow(DataTable table, ErrorMessageList errors)
    {
      DataRow row = table.NewRow();
      for (int i = 0; i < DBStruct.Count; i++)
      {
        if (errors == null)
          row[i] = GetValue(i); // пусть выбрасывает исключение
        else
        {
          try
          {
            row[i] = GetValue(i);
          }
          catch (DbfValueFormatException e)
          {
            errors.AddError(e.Message);
          }
          catch (Exception e)
          {
            errors.AddError(String.Format(Res.DbfFile_Err_CellErrorMessage, RecNo, DBStruct[i].Name, e.Message));
          }
        }
      }
      table.Rows.Add(row);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать указанные поля DBF-файла.
    /// Для больших DBF-файлов можно использовать метод <see cref="CreateBlockedTable(int, string, ErrorMessageList, out DataTable)"/> для загрузки таблицы по частям.
    /// </summary>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <returns>Заполненный объект <see cref="System.Data.DataTable"/></returns>
    public DataTable CreateTable(string columnNames)
    {
      return CreateTable(columnNames, new DummySplash(), null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать указанные поля DBF-файла
    /// </summary>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <param name="splash">Интерфейс управления процентным инидкатором.
    /// Если свойство <see cref="ISimpleSplash.AllowCancel"/> установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <returns>Заполненный объект <see cref="System.Data.DataTable"/></returns>
    public DataTable CreateTable(string columnNames, ISimpleSplash splash)
    {
      return CreateTable(columnNames, splash, null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать указанные поля DBF-файла.
    /// Эта версия позволяет пропускать ошибочные значения полей, которые не соответствуют типу данных.
    /// Для больших DBF-файлов можно использовать метод <see cref="CreateBlockedTable(int, string, ErrorMessageList, out DataTable)"/> для загрузки таблицы по частям.
    /// </summary>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <param name="splash">Интерфейс управления процентным инидкатором.
    /// Если свойство <see cref="ISimpleSplash.AllowCancel"/> установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <returns>Заполненный объект <see cref="System.Data.DataTable"/></returns>
    public DataTable CreateTable(string columnNames, ISimpleSplash splash, ErrorMessageList errors)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnNames");

      if (splash == null)
        splash = new DummySplash();

      int[] dbfColPoss;
      DataTable table = DBStruct.CreatePartialTable(columnNames, out dbfColPoss);

      int oldRN = RecNo;
      try
      {
        RecNo = 0;
        splash.PercentMax = this.RecordCount;
        while (Read())
        {
          AddDataTableRow(table, errors, dbfColPoss);
          splash.IncPercent();
        }
      }
      finally
      {
        RecNo = oldRN;
      }
      return table;
    }

    private void AddDataTableRow(DataTable table, ErrorMessageList errors, int[] dbfColPoss)
    {
      DataRow row = table.NewRow();
      for (int i = 0; i < dbfColPoss.Length; i++)
      {
        if (errors == null)
          row[i] = GetValue(dbfColPoss[i]);// пусть выбрасывает исключение
        else
        {
          try
          {
            row[i] = GetValue(dbfColPoss[i]);
          }
          catch (DbfValueFormatException e)
          {
            errors.AddError(e.Message);
          }
          catch (Exception e)
          {
            errors.AddError(String.Format(Res.DbfFile_Err_CellErrorMessage, RecNo, DBStruct[i].Name, e.Message));
          }
        }
      }
      table.Rows.Add(row);
    }

    /// <summary>
    /// Добавление значений из таблицы <see cref="System.Data.DataTable"/>.
    /// Идентификация полей выполняется по именам без учета регистра. Лишние поля пропускаются.
    /// </summary>
    /// <param name="table">Таблица - источник данных</param>
    public void Append(DataTable table)
    {
      CheckNotReadOnly();
      FlushRecord();

      int[] maps = new int[table.Columns.Count];
      for (int i = 0; i < maps.Length; i++)
        maps[i] = DBStruct.IndexOf(table.Columns[i].ColumnName);

      foreach (DataRow row in table.Rows)
      {
        AppendRecord();
        for (int i = 0; i < maps.Length; i++)
        {
          if (maps[i] >= 0)
            SetValue(maps[i], row[i]);
        }
      }

      FlushRecord();
    }


    /// <summary>
    /// Возвращает часть таблицы в виде <see cref="System.Data.DataTable"/>.
    /// Таблица будет содержать все поля DBF-файла.
    /// В таблицу входят строки, начиная с очередного вызова <see cref="Read()"/> и до достижения конца таблицы,
    /// но не более <paramref name="maxRowCount"/> строк.
    /// Метод возвращает false, если в DBF-файле больше нет строк.
    /// Обычно перед вызовом следует установить <see cref="RecNo"/>=0, а затем вызывать метод в цикле, 
    /// пока возвращается true.
    /// </summary>
    /// <param name="maxRowCount">Максимальное количество строк в возвращаемой таблице (размер блока)</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <param name="table">Сюда помещается заполненная таблица <see cref="DataTable"/>, содержащая от 1
    /// до <paramref name="maxRowCount"/> строк</param>
    /// <returns>true, если данные прочитаны и false, если DBF-файл не содержит непрочитанных строк</returns>
    public bool CreateBlockedTable(int maxRowCount, ErrorMessageList errors, out DataTable table)
    {
      if (maxRowCount < 1)
        throw ExceptionFactory.ArgOutOfRange("maxRowCount", maxRowCount, 1, null);

      table = null;
      while (Read())
      {
        if (table == null)
        {
          table = DBStruct.CreateTable();
        }
        AddDataTableRow(table, errors);
        if (table.Rows.Count >= maxRowCount)
          break;
      }
      return table != null;
    }

    /// <summary>
    /// Возвращает часть таблицы в виде <see cref="System.Data.DataTable"/>.
    /// Таблица будет содержать указанные поля DBF-файла.
    /// В таблицу входят строки, начиная с очередного вызова <see cref="Read()"/> и до достижения конца таблицы,
    /// но не более <paramref name="maxRowCount"/> строк.
    /// Метод возвращает false, если в DBF-файле больше нет строк.
    /// Обычно перед вызовом следует установить <see cref="RecNo"/>=0, а затем вызывать метод в цикле, 
    /// пока возвращается true.
    /// </summary>
    /// <param name="maxRowCount">Максимальное количество строк в возвращаемой таблице (размер блока)</param>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <param name="table">Сюда помещается заполненная таблица <see cref="DataTable"/>, содержащая от 1
    /// до <paramref name="maxRowCount"/> строк</param>
    /// <returns>true, если данные прочитаны и false, если DBF-файл не содержит непрочитанных строк</returns>
    public bool CreateBlockedTable(int maxRowCount, string columnNames, ErrorMessageList errors, out DataTable table)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnNames");

      int[] dbfColPoss = null; // без присвоения значения будет ошибка компилятора

      table = null;
      while (Read())
      {
        if (table == null)
          table = DBStruct.CreatePartialTable(columnNames, out dbfColPoss);
        AddDataTableRow(table, errors, dbfColPoss);
        if (table.Rows.Count >= maxRowCount)
          break;
      }
      return table != null;
    }

    #endregion
  }

  /// <summary>
  /// Копировщик данных между таблицами с оптимизацией
  /// </summary>
  public class DbfFileCopier
  {
    /*
     * Применение копировщика
     * 1. Создать два объекта DbfFile (источник и приемник).
     * 2. Создать объект DbfFileCopier.
     * 3. Заполнить список полей для копирования.
     * 4. Присоединить дополнительные обработчики к DbfFileCopier, если нужно.
     * 5. Вызвать метод DbfFileCopier.AppendRecords().
     */

    #region Конструктор

    /// <summary>
    /// Создает копировщик
    /// </summary>
    /// <param name="srcFile">Исходная таблица, открытая для чтения</param>
    /// <param name="resFile">Заполняемая таблица, открытая для записи</param>
    public DbfFileCopier(DbfFile srcFile, DbfFile resFile)
    {
#if DEBUG
      if (srcFile == null)
        throw new ArgumentNullException("srcFile");
      srcFile.CheckNotDisposed();

      if (resFile == null)
        throw new ArgumentNullException("resFile");
      resFile.CheckNotDisposed();
#endif

      if (resFile == srcFile)
        throw ExceptionFactory.ArgAreSame("srcFile", "resFile");
      if (resFile.IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(resFile);

      _SrcFile = srcFile;
      _ResFile = resFile;

      _MainCopyList = new Dictionary<int, int>();
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Исходная таблица для чтения.
    /// Задается в конструкторе.
    /// </summary>
    public DbfFile SrcFile { get { return _SrcFile; } }
    private readonly DbfFile _SrcFile;

    /// <summary>
    /// Заполняемая таблица.
    /// Задается в конструкторе.
    /// </summary>
    public DbfFile ResFile { get { return _ResFile; } }
    private readonly DbfFile _ResFile;

    #endregion

    #region Заполняемый список полей для копирования

    /// <summary>
    /// Основной список полей для копирования.
    /// Ключом является индекс поля в ResFile, а значением - индекс поля в исходной таблице
    /// </summary>
    private readonly Dictionary<int, int> _MainCopyList;

    /// <summary>
    /// Добавить правило для копирования.
    /// </summary>
    /// <param name="srcFieldIndex">Индекс столбца в исходной таблице <see cref="SrcFile"/></param>
    /// <param name="resFieldIndex">Индекс столбца в заполняемой таблице <see cref="ResFile"/></param>
    public void AddField(int srcFieldIndex, int resFieldIndex)
    {
      CheckNotReadOnly();

      if (srcFieldIndex < 0 || srcFieldIndex >= SrcFile.DBStruct.Count)
        throw ExceptionFactory.ArgOutOfRange("srcFieldIndex", srcFieldIndex, 0, SrcFile.DBStruct.Count - 1);
      if (resFieldIndex < 0 || resFieldIndex >= ResFile.DBStruct.Count)
        throw ExceptionFactory.ArgOutOfRange("resFieldIndex", resFieldIndex, 0, ResFile.DBStruct.Count - 1);
      if (_MainCopyList.ContainsKey(resFieldIndex))
        throw new InvalidOperationException(String.Format(Res.DbfFileCopier_Err_FieldRuleExists, ResFile.DBStruct[resFieldIndex]));

      _MainCopyList.Add(resFieldIndex, srcFieldIndex);
    }


    /// <summary>
    /// Добавить правило для копирования.
    /// </summary>
    /// <param name="srcFieldName">Имя столбца в исходной таблице <see cref="SrcFile"/></param>
    /// <param name="resFieldName">Имя столбца в заполняемой таблице <see cref="ResFile"/></param>
    public void AddField(string srcFieldName, string resFieldName)
    {
      int srcFieldIndex = SrcFile.DBStruct.IndexOf(srcFieldName);
      if (srcFieldIndex < 0)
        throw ExceptionFactory.ArgUnknownValue("srcFieldName", srcFieldName);
      int resFieldIndex = ResFile.DBStruct.IndexOf(resFieldName);
      if (resFieldIndex < 0)
        throw new ArgumentException("resFieldName", resFieldName);
      AddField(srcFieldIndex, resFieldIndex);
    }

    /// <summary>
    /// Добавление всех одноименных столбцов, кроме тех, для которых уже заданы правила
    /// </summary>
    public void AddAllFields()
    {
      AddAllFieldsExcept(null);
    }

    /// <summary>
    /// Добавление всех одноименных столбцов, за исключением указанных, и кроме тех, для которых уже заданы правила
    /// </summary>
    /// <param name="exceptedFields">Список исключаемых полей. Может быть null</param>
    public void AddAllFieldsExcept(string[] exceptedFields)
    {
      for (int resFieldIndex = 0; resFieldIndex < ResFile.DBStruct.Count; resFieldIndex++)
      {
        if (_MainCopyList.ContainsKey(resFieldIndex))
          continue; // уже есть правило

        string fieldName = ResFile.DBStruct[resFieldIndex].Name;

        if (exceptedFields != null)
        {
          bool exceptFlag = false;
          for (int i = 0; i < exceptedFields.Length; i++)
          {
            if (String.Equals(fieldName, exceptedFields[i], StringComparison.OrdinalIgnoreCase))
            {
              exceptFlag = true;
              break;
            }
          }
          if (exceptFlag)
            continue;
        }

        int srcFieldIndex = SrcFile.DBStruct.IndexOf(fieldName);
        if (srcFieldIndex < 0)
          continue; // Нет такого поля

        if (SrcFile.DBStruct[srcFieldIndex].Type == 'M' && (!SrcFile.HasMemoFile))
          continue;

        AddField(srcFieldIndex, resFieldIndex);
      }
    }

    /// <summary>
    /// Генерирует исключение, если список правил переведен в режим "только чтение"
    /// </summary>
    protected void CheckNotReadOnly()
    {
      if (_DirectCopyList != null)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Пользовательские обработчики

    /// <summary>
    /// Вызывается после копирования полей для очередной строки
    /// </summary>
    public event EventHandler AfterCopyRecord;

    #endregion

    #region Перекодировка

    /// <summary>
    /// Если свойству присвоено значение, то для значение может использоваться прямое
    /// преобразование byte[]-byte[] или прямое копирование.
    /// Иначе требуется преобразование byte[]-string-byte[]
    /// </summary>
    private SingleByteTranscoder _Transcoding;

    #endregion

    #region Внутренние списки для копирования

    private struct DirectCopyItem : IComparable<DirectCopyItem>
    {
      #region Конструктор

      public DirectCopyItem(int srcStart, int resStart, int len)
      {
        this.SrcStart = srcStart;
        this.ResStart = resStart;
        this.Len = len;
      }

      #endregion

      #region Поля

      public int SrcStart, ResStart, Len;

      #endregion

      #region IComparable<DirectCopyItem> Members

      public int CompareTo(DirectCopyItem other)
      {
        return ResStart.CompareTo(other.ResStart);
      }

      #endregion
    }

    /// <summary>
    /// Список блоков, которые можно копировать напрямую без преобразования значений
    /// </summary>
    private List<DirectCopyItem> _DirectCopyList;

    private struct SpaceFillItem : IComparable<SpaceFillItem>
    {
      #region Конструктор

      public SpaceFillItem(int resStart, int len)
      {
        this.ResStart = resStart;
        this.Len = len;
      }

      #endregion

      #region Поля

      public int ResStart, Len;

      #endregion

      #region IComparable<SpaceFillItem> Members

      public int CompareTo(SpaceFillItem other)
      {
        return ResStart.CompareTo(other.ResStart);
      }

      #endregion
    }

    /// <summary>
    /// Список блоков в конечной строке, которые должны заполняться пробелами
    /// </summary>
    private List<SpaceFillItem> _SpaceFillList;

    /// <summary>
    /// Буфер, содержащий символы ' '
    /// </summary>
    private byte[] _SpaceBuffer;

    private struct FieldPair
    {
      #region Конструктор

      public FieldPair(int srcFieldIndex, int resFieldIndex)
      {
        this.SrcFieldIndex = srcFieldIndex;
        this.ResFieldIndex = resFieldIndex;
      }

      #endregion

      #region Поля

      public int SrcFieldIndex;
      public int ResFieldIndex;

      #endregion
    }

    /// <summary>
    /// Список полей, которые нужно копивать по значениям
    /// </summary>
    private List<FieldPair> _ByValueCopyList;

    /// <summary>
    /// Выполнение подготовительных действий.
    /// Вызывается из AppendRecords()
    /// </summary>
    protected void Prepare()
    {
      if (_DirectCopyList != null)
        return; // повторный вызов

      if (SingleByteTranscoder.CanCreate(SrcFile.Encoding, ResFile.Encoding))
        _Transcoding = new SingleByteTranscoder(SrcFile.Encoding, ResFile.Encoding);

      #region Заполнение списков

      _DirectCopyList = new List<DirectCopyItem>();
      _SpaceFillList = new List<SpaceFillItem>();
      _ByValueCopyList = new List<FieldPair>();

      foreach (KeyValuePair<int, int> pair in _MainCopyList)
      {
        if (TestDirectCopy(pair.Value, pair.Key))
          continue;

        // Медленное копирование
        _ByValueCopyList.Add(new FieldPair(pair.Value, pair.Key));
      }

      #endregion

      #region Cписок для прямого копирования

      _DirectCopyList.Sort();

      for (int i = _DirectCopyList.Count - 1; i > 0; i--)
      {
        DirectCopyItem item1 = _DirectCopyList[i - 1];
        DirectCopyItem item2 = _DirectCopyList[i];
        if ((item1.SrcStart + item1.Len == item2.SrcStart) &&
          (item1.ResStart + item1.Len == item2.ResStart))
        {
          // Два блока подряд
          DirectCopyItem item3 = new DirectCopyItem(item1.SrcStart, item1.ResStart, item1.Len + item2.Len);
          _DirectCopyList[i - 1] = item3;
          _DirectCopyList.RemoveAt(i);
        }
      }

      #endregion

      #region Cписок для заполнения пробелами

      /*
       * Оптимизация этого списка маловероятна.
       * Единственное возможное исключение: Сначала идет строковое поле, затем числовое, и оба поля длиннее
       * чем в исходной таблице
       */

      _SpaceFillList.Sort();

      for (int i = _SpaceFillList.Count - 1; i > 0; i--)
      {
        SpaceFillItem item1 = _SpaceFillList[i - 1];
        SpaceFillItem item2 = _SpaceFillList[i];
        if (item1.ResStart + item1.Len == item2.ResStart)
        {
          // Два блока подряд
          SpaceFillItem item3 = new SpaceFillItem(item1.ResStart, item1.Len + item2.Len);
          _SpaceFillList[i - 1] = item3;
          _SpaceFillList.RemoveAt(i);
        }
      }

      int maxLen = 0;
      for (int i = 0; i < _SpaceFillList.Count; i++)
        maxLen = Math.Max(maxLen, _SpaceFillList[i].Len);
      if (maxLen > 0)
      {
        _SpaceBuffer = new byte[maxLen];
        DataTools.FillArray<byte>(_SpaceBuffer, (byte)' ');
      }

      #endregion
    }

    private bool TestDirectCopy(int srcFieldIndex, int resFieldIndex)
    {
      DbfFieldInfo srcField = SrcFile.DBStruct[srcFieldIndex];
      DbfFieldInfo resField = ResFile.DBStruct[resFieldIndex];
      if (srcField.Type != resField.Type)
        return false;

      int srcOff = SrcFile.GetFieldOffset(srcFieldIndex);
      int resOff = ResFile.GetFieldOffset(resFieldIndex);

      switch (srcField.Type)
      {
        case 'M':
          return false;

        case 'C':
          if (_Transcoding != null)
          {
            _DirectCopyList.Add(new DirectCopyItem(srcOff, resOff, Math.Min(srcField.Length, resField.Length)));
            if (resField.Length > srcField.Length)
              _SpaceFillList.Add(new SpaceFillItem(resOff + srcField.Length, resField.Length - srcField.Length));
            return true;
          }
          else
            return true;

        case 'D':
        case 'L':
          _DirectCopyList.Add(new DirectCopyItem(srcOff, resOff, srcField.Length));
          return true;

        case 'N':
        case 'F':
          if (srcField.Precision != resField.Precision)
            return false;
          if (srcField.Length > resField.Length)
            return false; // нельзя укорачивать длину
          _DirectCopyList.Add(new DirectCopyItem(srcOff, resOff, Math.Min(srcField.Length, resField.Length)));
          if (resField.Length > srcField.Length)
            _SpaceFillList.Add(new SpaceFillItem(resOff + srcField.Length, resField.Length - srcField.Length));
          return true;
        default:
          return false;
      }
    }

#if DEBUG

    /// <summary>
    /// Возвращает отладочную информацию об установках копирования
    /// </summary>
    /// <returns>Строка с информацией (многострочный текст)</returns>
    public string GetDebugInfo()
    {
      Prepare();

      StringBuilder sb = new StringBuilder();
      sb.Append("Direct copy blocks: ");
      sb.Append(_DirectCopyList.Count.ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Space fill blocks: ");
      sb.Append(_SpaceFillList.Count.ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Transcoding: ");
      if (_Transcoding == null)
        sb.Append("Not used");
      else if (_Transcoding.IsDirect)
        sb.Append("Direct copy");
      else
        sb.Append(_Transcoding.ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Slow copy fields: ");
      sb.Append(_ByValueCopyList.Count.ToString());
      for (int i = 0; i < _ByValueCopyList.Count; i++)
      {
        sb.Append(Environment.NewLine);
        sb.Append("  [");
        sb.Append(i);
        sb.Append("] ");
        DbfFieldInfo srcField = SrcFile.DBStruct[_ByValueCopyList[i].SrcFieldIndex];
        DbfFieldInfo resField = ResFile.DBStruct[_ByValueCopyList[i].ResFieldIndex];
        sb.Append(srcField.Name);
        sb.Append("(");
        sb.Append(srcField.TypeSizeText);
        sb.Append(") -> ");
        sb.Append(resField.Name);
        sb.Append("(");
        sb.Append(resField.TypeSizeText);
        sb.Append(")");
      }

      return sb.ToString();
    }

#endif

    #endregion

    #region Копирование

    /// <summary>
    /// Основной метод копирования.
    /// Перебирает все записи в исходной таблицы <see cref="SrcFile"/> и вызывает <see cref="DbfFile.AppendRecord()"/> в конечной таблице <see cref="ResFile"/> для каждой строки.
    /// Для больших таблиц рекомендуется использовать перегрузку <see cref="AppendRecords(ISimpleSplash)"/>, чтобы пользователь мог прервать процесс.
    /// </summary>
    public void AppendRecords()
    {
      AppendRecords(null);
    }

    /// <summary>
    /// Основной метод копирования.
    /// Перебирает все записи в исходной таблицы <see cref="SrcFile"/> и вызывает <see cref="DbfFile.AppendRecord()"/> в конечной таблице <see cref="ResFile"/> для каждой строки.
    /// </summary>
    /// <param name="splash">Управляемая splash-заставка. Если null, то пользователь не может прервать процесс.</param>
    public void AppendRecords(ISimpleSplash splash)
    {
      Prepare();

      int oldRN = SrcFile.RecNo;
      try
      {
        SrcFile.RecNo = 0;
        if (splash != null)
        {
          splash.PercentMax = SrcFile.RecordCount;
          splash.Percent = 0;
          splash.AllowCancel = true;
        }
        while (SrcFile.Read())
        {
          ResFile.AppendRecord();
          DoCopy();
          if (splash != null)
            splash.IncPercent();
        }
      }
      finally
      {
        SrcFile.RecNo = oldRN;
        if (splash != null)
          splash.PercentMax = 0;
      }
    }

    /// <summary>
    /// Копирование одной записи
    /// </summary>
    private void DoCopy()
    {
      byte[] srcBuf = SrcFile.RecordBuffer;
      byte[] resBuf = ResFile.RecordBuffer;

      #region Прямое копирование

      for (int i = 0; i < _DirectCopyList.Count; i++)
      {
        DirectCopyItem item = _DirectCopyList[i];
        if (_Transcoding == null)
          Array.Copy(srcBuf, item.SrcStart, resBuf, item.ResStart, item.Len);
        else
          _Transcoding.Transcode(srcBuf, item.SrcStart, resBuf, item.ResStart, item.Len);
      }

      #endregion

      #region Заполнение пробелами

      for (int i = 0; i < _SpaceFillList.Count; i++)
        Array.Copy(_SpaceBuffer, 0, resBuf, _SpaceFillList[i].ResStart, _SpaceFillList[i].Len);

      #endregion

      #region Медленное копирование

      for (int i = 0; i < _ByValueCopyList.Count; i++)
      {
        object x = SrcFile.GetValue(_ByValueCopyList[i].SrcFieldIndex);
        ResFile.SetValue(_ByValueCopyList[i].ResFieldIndex, x);
      }

      #endregion

      #region Пользовательский обработчик

      if (AfterCopyRecord != null)
        AfterCopyRecord(this, EventArgs.Empty);

      #endregion

      #region Признак изменения

      // Признак должен устанавливаться для каждой строки, а не в конце, иначе может быть
      // не выполнен метод Flush()
      ResFile.RecordModified = true;

      #endregion
    }

    #endregion
  }
}
