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


namespace FreeLibSet.DBF
{
  /// <summary>
  /// Описание для одного поля DBF-таблицы.
  /// Структура однократной записи
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
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
    {
#if DEBUG
      if (!IsValidFieldName(name))
        throw new ArgumentException("Неправильное имя DBF-поля: \"" + name + "\"", "name");
      if ("CNDLMF".IndexOf(typeChar) < 0)
        throw new ArgumentException("Неправильный тип поля: \"" + typeChar + "\"", "typeChar");
#endif
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
#if DEBUG
      if (!IsValidFieldName(name))
        throw new ArgumentException("Неправильное имя DBF-поля: \"" + name + "\"");
#endif
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
    /// <param name="Name">Имя поля</param>
    /// <returns>Описание поля</returns>
    public static DbfFieldInfo CreateBool(string Name)
    {
      return new DbfFieldInfo(Name, 'L', 1, 0);
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
    private string _Name;

    /// <summary>
    /// Возвращает букву типа поля
    /// </summary>
    public char Type { get { return _Type; } }
    private char _Type;

    /// <summary>
    /// Возвращает размер поля в байтах
    /// </summary>
    public int Length { get { return _Length; } }
    private int _Length;

    /// <summary>
    /// Возвращает число знаков после запятой для числового поля
    /// </summary>
    public int Precision { get { return _Precision; } }
    private int _Precision;

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает true, если структура не заполнена.
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(_Name); } }

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

    /// <summary>
    /// Возвращает читаемое представление для типа поля и его длины
    /// </summary>
    public string TypeSizeText
    {
      get
      {
        string s = TypeText;
        switch (_Type)
        {
          case 'C':
            return s + " (" + _Length.ToString() + ")";
          case 'N':
            if (_Precision == 0)
              return s + " (" + _Length.ToString() + ")";
            else
              return s + " (" + _Length.ToString() + "," + _Precision.ToString() + ")";
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
    /// Возвращает тип для столбца, совместимый с DataColumn.DataType
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
          case 'F': // ??
            if (Precision == 0)
            {
              if (Length > 9)
                return typeof(long);
              else
                return typeof(int);
            }
            else
              return typeof(decimal);
          case 'D':
            return typeof(DateTime);
          case 'L':
            return typeof(bool);
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// Возвращает текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "[пусто]";
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
      string ValidFieldTypes;
      switch (fileFormat)
      {
        case DbfFileFormat.dBase2:
          ValidFieldTypes = "CNL";
          break;
        case DbfFileFormat.dBase3:
          ValidFieldTypes = "CNLDM";
          break;
        case DbfFileFormat.dBase4:
          ValidFieldTypes = "CNLDMF";
          break;
        default:
          ValidFieldTypes = null;
          break;
      }

      if (ValidFieldTypes != null)
      {
        if (ValidFieldTypes.IndexOf(Type) < 0)
        {
          errorText = "Неподдерживаемый тип \"" + Type + "\" поля \"" + Name + "\". Поддерживаемые типы: " + ValidFieldTypes;
          return false;
        }
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
      const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";
      if (String.IsNullOrEmpty(fieldName))
        return false;
      if (/*fieldName.Length < 1 || 27.12.2020 - только что проверили */ fieldName.Length > 10)
        return false;
      for (int i = 0; i < fieldName.Length; i++)
      {
        if (ValidChars.IndexOf(fieldName[i]) < 0)
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
    /// Если в списке нет поля с таким именем, возвращается неинициализированная структура
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
    /// Возвращает размер записи DBF-файла. Равно суммарной длине всех полей плюс один символ - маркер удаленной строки
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
        throw new ArgumentException("Item is empty", "item");

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
    /// Создает новую таблицу DataTable с заполненным списком Columns.
    /// Таблица не содержит строк.
    /// </summary>
    /// <returns>Новый объект DataTable</returns>
    public DataTable CreateTable()
    {
      DataTable Table = new DataTable();
      for (int i = 0; i < _Items.Count; i++)
        Table.Columns.Add(_Items[i].Name, _Items[i].DataType);
      return Table;
    }

    internal DataTable CreatePartialTable(string columnNames, out int[] dbfColPoss)
    {
      DataTable Table = new DataTable();
      string[] aColNames = columnNames.Split(',');
      dbfColPoss = new int[aColNames.Length];
      for (int i = 0; i < aColNames.Length; i++)
      {
        dbfColPoss[i] = this.IndexOf(aColNames[i]);
        if (dbfColPoss[i] < 0)
          throw new ArgumentException("DBF-файл не содержит поля \"" + aColNames[i] + "\"", "columnNames");

        Table.Columns.Add(aColNames[i], this[dbfColPoss[i]].DataType);
      }
      return Table;
    }

    /// <summary>
    /// Проверяет, что выбранный формат может быть использован для заданной структуры таблицы
    /// </summary>
    /// <param name="fileFormat">Проверяемый формат</param>
    /// <returns>true, если формат можно использовать</returns>
    public bool TestFormat(DbfFileFormat fileFormat)
    {
      string ErrorText;
      return TestFormat(fileFormat, out ErrorText);
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
          errorText = "dBase II поддерживает максимально 32 поля в таблице";
          return false;
        }
      }

      errorText = null;
      return true;
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
    /// Список блокируется при использовании в DbfFile.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new InvalidOperationException("Список полей находится в режиме ReadOnly");
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
      : base("Поле \"" + fieldName + "\" имеет неподходящий тип \"" + fieldType + "\"")
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
      StringBuilder sb = new StringBuilder();
      sb.Append("Строка ");
      sb.Append(recNo);
      sb.Append(", поле \"");
      sb.Append(fieldInfo.Name);
      sb.Append("\" содержит недопустимое значение для типа \"");
      sb.Append(fieldInfo.TypeText);
      sb.Append("\"");
      if (innerException != null)
      {
        sb.Append(". ");
        sb.Append(innerException.Message);
      }
      return sb.ToString();
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
  /// Класс для чтения/записи DBF файла в формате dBase-III (сигнатура 0x03/0x83)
  /// Поддерживаются memo-поля (файлы DBT)
  /// Индексы не поддерживаются
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
    /// Используется кодировка по умолчанию DefaultEncoding.
    /// Если в файле есть мемо-поля, одновременно открывается и DBT-файл.
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    public DbfFile(AbsPath dbfPath)
      : this(dbfPath, DefaultEncoding, true)
    {
    }

    /// <summary>
    /// Открывает существующий файл.
    /// Используется кодировка по умолчанию DefaultEncoding.
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
        throw new FileNotFoundException("Файл не найден: \"" + dbfPath.Path + "\"");

      ShouldDisposeStreams = true;

      if (isReadOnly)
        fsDBF = new FileStream(dbfPath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      else
        fsDBF = new FileStream(dbfPath.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); // ?? права
      try
      {
        if (fsDBF.Length < 1)
          throw new DbfFileFormatException("Файл \"" + dbfPath + "\" имеет нулевую длину");

        // Определяем наличие МЕМО-файла
        int Code = fsDBF.ReadByte();
        fsDBF.Position = 0L; // обязательно возвращаем на начало
        if (Code == 0x83)
        {
          AbsPath dbtPath = dbfPath.ChangeExtension(".DBT");
          if (!File.Exists(dbtPath.Path))
            throw new FileNotFoundException("МЕМО-файл не найден: \"" + dbtPath.Path + "\"");

          if (isReadOnly)
            fsDBT = new FileStream(dbtPath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
          else
            fsDBT = new FileStream(dbtPath.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); // ?? права
        }

        _Encoding = encoding;
        _IsReadOnly = isReadOnly;
        InitExisted();
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
    /// Используется кодировка по умолчанию DefaultEncoding.
    /// </summary>
    /// <param name="dbfStream">Открытый на чтение поток для DBF-файла</param>
    public DbfFile(Stream dbfStream)
      : this(dbfStream, (Stream)null, DefaultEncoding, true)
    {
    }

    /// <summary>
    /// Открывает DBF-файл в потоке только для чтения.
    /// Если файл содержит MEMO-поля, должен быть предоставлен поток для DBT-файла
    /// Используется кодировка по умолчанию DefaultEncoding.
    /// </summary>
    /// <param name="dbfStream">Открытый на чтение поток для DBF-файла</param>
    /// <param name="dbtStream">Открытый на чтение поток для DBT-файла или null</param>
    public DbfFile(Stream dbfStream, Stream dbtStream)
      : this(dbfStream, dbtStream, DefaultEncoding, true)
    {
    }

    /// <summary>
    /// Открывает DBF-файл в потоке только для чтения.
    /// Если файл содержит MEMO-поля, должен быть предоставлен поток для DBT-файла
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
    /// Если файл содержит MEMO-поля, должен быть предоставлен поток для DBT-файла
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

      ShouldDisposeStreams = false;
      fsDBF = dbfStream;
      fsDBT = dbtStream;
      _Encoding = encoding;
      _IsReadOnly = isReadOnly;

      InitExisted();
    }

    private void InitExisted()
    {
      #region Инициализация общих свойств

      if (_Encoding == null)
        _Encoding = DefaultEncoding;
      if (fsDBF == null)
        throw new NullReferenceException("DbfStream==null");

      SkipDeleted = true;

      #endregion

      #region 1. Читаем заголовок

      BinaryReader rdrDBF = new BinaryReader(fsDBF);

      bool HasMemoFile;
      // Идентификатор DBF-файла
      int DbfId = rdrDBF.ReadByte();
      switch (DbfId)
      {
        case 0x03:
          _Format = DbfFileFormat.dBase3;
          HasMemoFile = false;
          break;
        case 0x04:
          _Format = DbfFileFormat.dBase4;
          HasMemoFile = false;
          break;
        case 0x83:
          HasMemoFile = true;
          _Format = DbfFileFormat.dBase3;
          break;
        case 0x8B:
          HasMemoFile = true;
          _Format = DbfFileFormat.dBase4;
          break;
        default:
          throw new DbfFileFormatException("Не DBF-файл");
      }

      // Метка даты
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();

      // Число записей
      uint RecCount = rdrDBF.ReadUInt32();
      if (RecCount > (uint)(int.MaxValue))
        throw new DbfFileFormatException("Указано слишком много строк в заголовке: " + RecCount.ToString());
      FRecordCount = (int)RecCount;
      // Смещение до начала данных
      DataOffset = rdrDBF.ReadUInt16();
      // Размер записи
      RecSize = rdrDBF.ReadUInt16();
      if (RecSize < 1)
        throw new DbfFileFormatException("Размер записи не может быть равен 0");

      long WantedSize = DataOffset + (long)RecSize * (long)FRecordCount;
      if (rdrDBF.BaseStream.Length < WantedSize)
        throw new DbfFileFormatException("Длина файла (" + WantedSize.ToString() + ") не соответствует размеру и числу записей, заданных в заголовке. Длина файла должна быть не меньше, чем " + WantedSize.ToString());

      // Заполнитель
      rdrDBF.ReadBytes(20);

      #endregion

      #region 2. Читаем список полей

      byte[] bFldName = new byte[11];
      _DBStruct = new DbfStruct();
      while (true)
      {
        if (rdrDBF.Read(bFldName, 0, 1) < 1)
          throw new DbfFileFormatException("Неожиданный конец файла. Список полей не закончен");

        if (bFldName[0] == 0x0D)
          break; // Конец списка полей

        // Имя поля
        if (rdrDBF.Read(bFldName, 1, 10) < 10)
          throw new DbfFileFormatException("Неожиданный конец файла. Список полей не закончен");

        int EndPos = Array.IndexOf<byte>(bFldName, 0);
        if (EndPos < 0)
          throw new DbfFileFormatException("В списке полей для поля " + (DBStruct.Count + 1).ToString() + " не найден нулевой байт окончания имени поля");

        if (EndPos == 0)
          throw new DbfFileFormatException("В списке полей для поля " + (DBStruct.Count + 1).ToString() + " не задано имя поля");

        string FieldName = Encoding.GetString(bFldName, 0, EndPos);
        // Тип поля
        byte[] bFldType = rdrDBF.ReadBytes(1);
        string FieldType = Encoding.GetString(bFldType);

        rdrDBF.ReadBytes(4); // пропуск

        int Len;
        int Prec;
        if (FieldType == "C")
        {
          Len = rdrDBF.ReadUInt16();
          Prec = 0;
        }
        else
        {
          Len = rdrDBF.ReadByte();
          Prec = rdrDBF.ReadByte();
        }

        rdrDBF.ReadBytes(14);

        switch (FieldType)
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
            FieldType = "C";
            break;
        }

        _DBStruct.Add(new DbfFieldInfo(FieldName, FieldType[0], Len, Prec));

        _IsReadOnly = IsReadOnly;
      }

      if (DBStruct.RecordSize != RecSize)
        throw new DbfFileFormatException("Размер записи, заданный в заголовке (" + RecSize.ToString() +
          ") не совпадает с размером всех полей (" + DBStruct.RecordSize.ToString() + ")");

      if (DataOffset < rdrDBF.BaseStream.Position)
        throw new DbfFileFormatException("Неправильная позиция начала данных (" + DataOffset.ToString() + "). Описания полей перекрывают область данных");

      // Внутренние поля
      InitDBStructInternal();

      #endregion

      #region 3. Подготовка буфера строки

      _UseMemoFile = (fsDBT != null) && HasMemoFile;
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
      if (fsDBT != null)
      {
        BinaryReader rdrDBT = new BinaryReader(fsDBT);

        if (rdrDBT.BaseStream.Length < 5)
          throw new DbfFileFormatException("МЕМО-файл имеет недопустимо малую длину");

        _MemoBlockCount = rdrDBT.ReadUInt32();
        if (_MemoBlockCount < MinMemoBlockCount || _MemoBlockCount > MaxMemoBlockCount)
          throw new DbfFileFormatException("В заголовке МЕМО-файла указано недопустимое число 512-байтных блоков: " + _MemoBlockCount.ToString());

        long MinMemoFileSize = (long)(_MemoBlockCount - 1) * 512 + 2;
        //long MaxMemoFileSize = (long)(_MemoBlockCount) * 512 + 1;
        if (rdrDBT.BaseStream.Length < MinMemoFileSize)
          throw new DbfFileFormatException("Реальный размер МЕМО-файла (" + rdrDBT.BaseStream.Length +
            ") меньше, чем вычисленный минимально возможный размер, исходя из заголовка (" +
            MinMemoFileSize.ToString() + ")");
        // Максимально возможный размер не проверяем, вдруг там ЭЦП
      }

      #endregion
    }

    #endregion

    #region Конструкторы для создания нового файла

    /// <summary>
    /// Создает на диске новый DBF-файл заданной структуры.
    /// Если в структуре есть мемо-поля, также создается DBT-файл
    /// Если файл(ы) существует, он удаляется.
    /// Используется формат файла dBase3. Если в списке полей есть типы, отличные от "C", "N", "L", "D" и "M",
    /// используется формат dBase4.
    /// Используется кодировка по умолчанию DefaultEncoding.
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
    /// Если в структуре есть мемо-поля, также создается DBT-файл
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
    /// Если в структуре есть мемо-поля, также создается DBT-файл
    /// Если файл(ы) существует, он удаляется.
    /// Список <paramref name="dbStruct"/> переводится в состояние "только чтение".
    /// </summary>
    /// <param name="dbfPath">Путь к DBF-файлу на диске.</param>
    /// <param name="dbStruct">Заполненный список описаний полей</param>
    /// <param name="encoding">Используемая кодировка</param>
    /// <param name="fileFormat">Формат файла. Если задано значение Undefined,
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
        throw new ArgumentException("Структура таблицы не заполнена", "dbStruct");

      _DBStruct = dbStruct;

      _Encoding = encoding;

      _IsReadOnly = false;

      ShouldDisposeStreams = true;

      _Format = fileFormat;

      try
      {
        fsDBF = new FileStream(dbfPath.Path, FileMode.Create, FileAccess.ReadWrite);

        if (dbStruct.HasMemo)
        {
          AbsPath dbtFilePath = dbfPath.ChangeExtension(".dbt");
          fsDBT = new FileStream(dbtFilePath.Path, FileMode.Create, FileAccess.ReadWrite);
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
    /// Используется кодировка по умолчанию DefaultEncoding.
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
    /// Используется кодировка по умолчанию DefaultEncoding.
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
    /// <param name="fileFormat">Формат файла. Если задано значение Undefined,
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
        throw new ArgumentException("Структура таблицы не заполнена", "dbStruct");

      ShouldDisposeStreams = false;
      fsDBF = dbfStream;
      fsDBT = dbtStream;
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
        throw new DbfFileFormatException("Слишком большой размер одной записи");
      RecSize = (ushort)(DBStruct.RecordSize);

      BinaryWriter wrtDBF = new BinaryWriter(fsDBF);

      // Сигнатура
      if (DBStruct.HasMemo)
        wrtDBF.Write((byte)0x83);
      else
        wrtDBF.Write((byte)0x03);

      // Дата записи
      DateTime dt = DateTime.Today;
      wrtDBF.Write((byte)(dt.Year % 100));
      wrtDBF.Write((byte)(dt.Month));
      wrtDBF.Write((byte)(dt.Day));

      // Число записей
      FRecordCount = 0;
      wrtDBF.Write((uint)0); // Число записей

      // Смещение данных в файле
      DataOffset = (UInt16)((DBStruct.Count * 32) + 32 + 1);
      wrtDBF.Write(DataOffset);

      // Размер записи
      wrtDBF.Write((UInt16)(RecSize));
      // Заполнитель 20 байт
      WriteZeros(wrtDBF, 20);

      // Описания полей
      for (int i = 0; i < DBStruct.Count; i++)
      {
        byte[] bName = Encoding.GetBytes(DBStruct[i].Name);
        wrtDBF.Write(bName);
        WriteZeros(wrtDBF, 11 - bName.Length);

        byte[] bType = Encoding.GetBytes(new char[1] { DBStruct[i].Type });
        wrtDBF.Write(bType);
        wrtDBF.Write((UInt32)0); // пропуск 4 байта

        if (DBStruct[i].Type == 'N')
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
      wrtDBF.Flush();

      _UseMemoFile = DBStruct.HasMemo && (fsDBT != null);
      _RecordBuffer = new byte[RecSize];

      if (_UseMemoFile)
      {
        // DBT-файл содержит единственный блок
        fsDBT.WriteByte(0x01);
        fsDBT.WriteByte(0x00);
        fsDBT.WriteByte(0x00);
        fsDBT.WriteByte(0x00);
        fsDBT.SetLength(512);
        _MemoBlockCount = 1;
      }
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
      int CurrOffset = 1; // повторный проход
      _FieldOffsets = new int[DBStruct.Count];
      for (int i = 0; i < DBStruct.Count; i++)
      {
        _FieldOffsets[i] = CurrOffset;
        CurrOffset += DBStruct[i].Length;
        _FieldIndices.Add(DBStruct[i].Name, i);
      }
    }

    /// <summary>
    /// Сбрасывает на диск несохраненные изменения и закрывает файлы, если использовался конструктор,
    /// принимающий имя файла
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      FlushRecord();
      FlushHeader();
      DisposeStreams();
      base.Dispose(disposing);
    }

    private void DisposeStreams()
    {
      if (ShouldDisposeStreams)
      {
        if (fsDBT != null)
        {
          fsDBT.Close();
          fsDBT = null;
        }
        if (fsDBF != null)
        {
          fsDBF.Close();
          fsDBF = null;
        }
      }
    }

    #endregion

    #region Потоки

    /// <summary>
    /// Поток для DBF-файла
    /// </summary>
    private Stream fsDBF;

    /// <summary>
    /// Поток для DBT-файла
    /// </summary>
    private Stream fsDBT;

    /// <summary>
    /// Возвращает true, если используется memo-файл
    /// </summary>
    public bool HasMemoFile { get { return fsDBT != null; } }

    /// <summary>
    /// Нужно ли закрывать потоки при вызове Dispose()
    /// </summary>
    private bool ShouldDisposeStreams;

    /// <summary>
    /// Выполняет сброс на диск незаписанных данных, вызывая Stream.Flush()
    /// </summary>
    public void Flush()
    {
      fsDBF.Flush();
      if (fsDBT != null)
        fsDBT.Flush();
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
    /// Возвращает смещение заданного поля от начала строки
    /// </summary>
    /// <param name="FieldIndex"></param>
    /// <returns></returns>
    public int GetFieldOffset(int FieldIndex)
    {
      return _FieldOffsets[FieldIndex];
    }

    /// <summary>
    /// Версия формата файла
    /// Определяется автоматически при открытии существующего файла.
    /// Может быть задана в конструкторе при создании нового файла
    /// </summary>
    public DbfFileFormat Format { get { return _Format; } }
    private DbfFileFormat _Format;

    /// <summary>
    /// Признак использования memo-файла (наличие полей и DBT-файла)
    /// </summary>
    private bool _UseMemoFile;

    /// <summary>
    /// Число записанных блоков мемо-файла
    /// </summary>
    uint _MemoBlockCount;

    /// <summary>
    /// Получить максимальную длину полей
    /// Возвращает массив, размер которого равен DBStruct.Count.
    /// Для всех полей, кроме 'C' и 'M' возвращается стандартная длина из DbfFieldInfo.Length.
    /// Для строковых и MEMO-полей возвращается максимальная длина поля, возможно - 0.
    /// Для больших таблиц рекомендуется использовать перегрузку с аргументом Splash,
    /// чтобы пользователь мош прервать процесс.
    /// </summary>
    /// <returns>Массив длин</returns>
    public int[] GetMaxLengths()
    {
      return GetMaxLengths(null, false);
    }

    /// <summary>
    /// Получить максимальную длину полей
    /// Возвращает массив, размер которого равен DBStruct.Count.
    /// Для всех полей, кроме 'C' и 'M' возвращается стандартная длина из DbfFieldInfo.Length.
    /// Для строковых и MEMO-полей возвращается максимальная длина поля, возможно - 0.
    /// Если пользователь прерывает процесс, выбрасывается исключение UserCancelException.
    /// </summary>
    /// <param name="splash">Управляемая splash-заставка для индикации процесса и возможности прерывания.
    /// Если null, то процесс нельзя прервать</param>
    /// <returns>Массив длин</returns>
    public int[] GetMaxLengths(ISplash splash)
    {
      return GetMaxLengths(splash, false);
    }

    /// <summary>
    /// Получить максимальную длину полей
    /// Возвращает массив, размер которого равен DBStruct.Count.
    /// Для всех полей, кроме 'C' и 'M' возвращается стандартная длина из DbfFieldInfo.Length.
    /// Для строковых и MEMO-полей возвращается максимальная длина поля, возможно - 0.
    /// Эта версия позволяет получить результаты для части таблицы
    /// </summary>
    /// <param name="splash">Управляемая splash-заставка для индикации процесса и возможности прерывания.
    /// Если null, то процесс нельзя прервать</param>
    /// <param name="wheneverUserCancel">Что происходит, если пользователь прерывает процесс.
    /// Ксли true, то возвращается частичный результат, по той части, которую успели просмотреть.
    /// Если false, выбрасывается исключение UserCancelException.</param>
    /// <returns>Массив длин</returns>
    public int[] GetMaxLengths(ISplash splash, bool wheneverUserCancel)
    {
      int[] a = new int[DBStruct.Count];
      List<int> Indices = new List<int>();
      for (int i = 0; i < DBStruct.Count; i++)
      {
        switch (DBStruct[i].Type)
        {
          case 'C':
          case 'M':
            Indices.Add(i);
            break;
          default:
            a[i] = DBStruct[i].Length;
            break;
        }
      }

      if (Indices.Count == 0)
        return a; // ничего проверять не надо

      try
      {
        if (splash != null)
        {
          splash.PercentMax = RecordCount;
          splash.Percent = 0;
          splash.AllowCancel = true;
        }

        int OldRN = RecNo;
        try
        {
          RecNo = 0;
          while (Read() && Indices.Count > 0)
          {
            for (int i = Indices.Count - 1; i >= 0; i--)
            {
              int FieldIndex = Indices[i];
              int l = GetLength(FieldIndex);
              a[FieldIndex] = Math.Max(a[FieldIndex], l);
              if (DBStruct[FieldIndex].Type == 'C')
              {
                if (l == DBStruct[FieldIndex].Length)
                  // Достигнута максимальная длина строкового поля и дальнейший перебор смысла не имеет
                  Indices.RemoveAt(i);
              }
            }
            if (splash != null)
              splash.IncPercent();
          }
        }
        finally
        {
          RecNo = OldRN;
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
    /// Используемая кодировка. Задается в конструкторе. Если не задана в явном виде, возвращает DefaultEncoding
    /// </summary>
    public Encoding Encoding { get { return _Encoding; } }
    private Encoding _Encoding;

    /// <summary>
    /// Возвращает кодировку по умолчанию
    /// </summary>
    public static Encoding DefaultEncoding
    {
      get
      {
        int CodePage = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
        return Encoding.GetEncoding(CodePage);
      }
    }

    #endregion

    #region Поля для поиска записи

    /// <summary>
    /// Число записей
    /// </summary>
    public int RecordCount { get { return FRecordCount; } }
    private int FRecordCount;

    /// <summary>
    /// Смещение от начала файла до начала данных
    /// </summary>
    private ushort DataOffset;

    /// <summary>
    ///  Размер одной записи
    /// </summary>
    private ushort RecSize;

    #endregion

    #region Перебор записей

    /// <summary>
    /// Номер текущей записи, начиная с 1
    /// При инициализации устанавливается равным 0.
    /// Установка нулевого значения допускается, если требуется запуск цикла Read() после выполнения других операций
    /// </summary>
    public int RecNo
    {
      get { return _RecNo; }
      set
      {
        if (value < 0 || value > FRecordCount)
          throw new ArgumentOutOfRangeException("Нельзя перейти к записи " + value.ToString() + ", т.к. количество записей в таблице равно " + FRecordCount.ToString());

        if (value == _RecNo)
          return;

        FlushRecord();

        _RecNo = value;
        if (value == 0)
          DataTools.FillArray<byte>(_RecordBuffer, 0);
        else
        {
          fsDBF.Position = DataOffset + (value - 1) * RecSize;
          fsDBF.Read(_RecordBuffer, 0, RecSize);
        }
      }
    }
    private int _RecNo;

    /// <summary>
    /// Необходимость пропуска удаленных записей методом Read().
    /// По умолчанию - true - записи пропускаются
    /// </summary>
    public bool SkipDeleted { get { return _SkipDeleted; } set { _SkipDeleted = value; } }
    private bool _SkipDeleted;

    /// <summary>
    /// Метод для перебора записей в стиле DbReader.
    /// Строки, помеченные на удаление, пропускаются.
    /// </summary>
    /// <returns>true, если получена очередная запись, false, если достигнут конец файла</returns>
    public bool Read()
    {
      while (RecNo < RecordCount)
      {
        RecNo++;
        if (!RecordDeleted)
          return true;
      }
      return false;
    }

    #endregion

    #region Чтение значений полей

    /// <summary>
    /// Буфер строки таблицы размером RecSize.
    /// Прямая модификация буфера может привести к порче файла
    /// Также должно устанавливаться свойство RecordModified=true
    /// </summary>
    public byte[] RecordBuffer { get { return _RecordBuffer; } }
    private byte[] _RecordBuffer;

    private void CheckActiveRecord()
    {
      if (_RecordBuffer == null)
        throw new InvalidOperationException("Нет выбранной записи");
    }

    /// <summary>
    /// true - если текущая запись помечена на удаление
    /// При обычном переборе методом Read(), если свойство SkipDeleted не сброшено в false, удаленные записи
    /// пропускаются
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
    /// Получить значение поля
    /// Для пустых значений возвращается DBNull
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public object GetValue(string fieldName)
    {
      return GetValue(InternalIndexOfField(fieldName));
    }


    /// <summary>
    /// Получить значение поля с заданным индексом
    /// Для пустых значений возвращается DBNull
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
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
              return decimal.Parse(s, DataTools.DotNumberConv);
            else if (DBStruct[fieldIndex].Length > 9)
              return long.Parse(s);
            else
              return int.Parse(s);
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
    /// Получить строковое значение поля
    /// Для пустых значений возвращается DBNull
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public string GetString(string fieldName)
    {
      return GetString(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить строковое значение поля с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
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
        uint MemoBlockEntry = ReadMemoEntry(fieldIndex);
        if (MemoBlockEntry == 0)
          return String.Empty;

        if (fsDBT == null)
          throw new DbfFileFormatException("Запрошено чтение значения memo-поля \"" + DBStruct[fieldIndex].Name + "\", но memo-файл недоступен");

        byte[] b = ReadMemoValue(MemoBlockEntry, DBStruct[fieldIndex].Name);
        return Encoding.GetString(b);
      }
      else
      {
        // 18.03.2017
        // OpenOffice любит записывать символы \0 вместо пробелов, с целью показать значение NULL
        if (_RecordBuffer[_FieldOffsets[fieldIndex]] == '\0')
          return String.Empty;

        string s = Encoding.GetString(_RecordBuffer, _FieldOffsets[fieldIndex], DBStruct[fieldIndex].Length);
        s = s.TrimEnd(' ');
        return s;
      }
    }

    private void CheckFieldIndex(int fieldIndex)
    {
      if (fieldIndex < 0 || fieldIndex >= DBStruct.Count)
        throw new ArgumentOutOfRangeException("fieldIndex", fieldIndex, "Индекс поля должен быть в диапазоне от 0 до " +
          (DBStruct.Count - 1).ToString());
    }

    /// <summary>
    /// Получить числовое значение поля
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public int GetInt(string fieldName)
    {
      return GetInt(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить строковое значение поля с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
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
        case 'C':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
            if (s.Length == 0)
              return 0; // 21.03.2017
            return int.Parse(s);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public long GetInt64(string fieldName)
    {
      return GetInt64(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить строковое значение поля с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
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
        case 'C':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
            if (s.Length == 0)
              return 0L;
            return long.Parse(s);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public float GetSingle(string fieldName)
    {
      return GetSingle(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
    /// <returns></returns>
    public float GetSingle(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'C':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
            if (s.Length == 0)
              return 0f; // 21.03.2017
            return float.Parse(s, DataTools.DotNumberConv);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }

        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public double GetDouble(string fieldName)
    {
      return GetDouble(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
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
        case 'C':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
            if (s.Length == 0)
              return 0.0; // 21.03.2017
            return double.Parse(s, DataTools.DotNumberConv);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить числовое значение поля
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(string fieldName)
    {
      return GetDecimal(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить числовое значение поля с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
    /// <returns></returns>
    public decimal GetDecimal(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'N':
        case 'C':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
            if (s.Length == 0)
              return 0m; // 21.03.2017
            return decimal.Parse(s, DataTools.DotNumberConv);
          }
          catch (Exception e)
          {
            if (DBStruct[fieldIndex].Type == 'N')
              throw new DbfValueFormatException(RecNo, DBStruct[fieldIndex], GetString(fieldIndex), e);
            else
              throw;
          }
        default:
          throw new DbfFieldTypeException(DBStruct[fieldIndex]);
      }
    }

    /// <summary>
    /// Получить логическое значение поля
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public bool GetBool(string fieldName)
    {
      return GetBool(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить логическое значение поля
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
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
            return BoolFromByte(b) ?? false;
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
    /// Внутренний метод
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    internal static bool? BoolFromByte(byte b)
    {
      switch ((char)b)
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
          throw new ArgumentException("Непреобразуемое значение \"" + (char)b + "\"");
      }
    }

    /// <summary>
    /// Получить значение поля даты
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public DateTime? GetNullableDate(string fieldName)
    {
      return GetNullableDate(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить значение поля типа даты с заданным индексом
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
    /// <returns></returns>
    public DateTime? GetNullableDate(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'D':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
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
    /// Пустая дата возвращается как DateTime.MinValue.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Значение</returns>
    public DateTime GetDate(string fieldName)
    {
      return GetDate(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Получить значение поля типа даты с заданным индексом.
    /// Пустая дата возвращается как DateTime.MinValue.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля от 0 до DBStruct.Count-1</param>
    /// <returns></returns>
    public DateTime GetDate(int fieldIndex)
    {
#if DEBUG
      CheckActiveRecord();
      CheckFieldIndex(fieldIndex);
#endif

      switch (DBStruct[fieldIndex].Type)
      {
        case 'D':
          try
          {
            string s = GetString(fieldIndex);
            s = s.Trim(' ');
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
        throw new ArgumentNullException("fieldName"); // 27.12.2020 Перенесено в начало

      int FieldPos;
      if (!_FieldIndices.TryGetValue(fieldName.ToUpperInvariant(), out FieldPos))
        throw new ArgumentException("Таблица не содержит поля \"" + fieldName + "\"", "fieldName");
      return FieldPos;
    }

    private uint ReadMemoEntry(int fieldIndex)
    {
      int LastPos = _FieldOffsets[fieldIndex] + 9;
      int nc = 0;
      while (nc < 10)
      {
        byte b = _RecordBuffer[LastPos - nc];
        if (b >= (byte)'0' && b <= (byte)'9')
          nc++;
        else
          break;
      }

      if (nc == 0)
        return 0;

      string s = Encoding.ASCII.GetString(_RecordBuffer, LastPos - nc + 1, nc);
      return uint.Parse(s, DataTools.DotNumberConv);
    }

    private byte[] ReadMemoValue(uint memoBlockEntry, string fieldName)
    {
      if (memoBlockEntry < MinMemoBlockCount || memoBlockEntry > MaxMemoBlockCount)
        throw new DbfFileFormatException("В строке " + RecNo.ToString() + " в МЕМО-поле \"" + fieldName +
          "\" задан недопустимый номер блока: " + memoBlockEntry.ToString());

      long FileOff = (long)memoBlockEntry * 512L;
      if (fsDBT.Length <= FileOff)
        throw new DbfFileFormatException("В строке " + RecNo.ToString() + " в МЕМО-поле \"" + fieldName +
          "\" задан недопустимый номер блока: " + memoBlockEntry.ToString() +
          ", который выходит за пределы МЕМО-файла");

      // Сначала нужно найти символ 1A, который завершает текст
      fsDBT.Position = FileOff;
      int cnt = 0;
      while (true)
      {
        int v = fsDBT.ReadByte();
        if (v < 0)
          throw new DbfFileFormatException("Обнаружен неожиданный конец МЕМО-файла для блока " +
            memoBlockEntry.ToString() + ", указанного в строке " + RecNo.ToString() + " в поле \"" + fieldName);
        if (v == 0x1A)
          break;
        cnt++;
        if (cnt > 65535)
          throw new DbfFileFormatException("В МЕМО-файле для блока " +
            memoBlockEntry.ToString() + ", указанного в строке " + RecNo.ToString() + " в поле \"" + fieldName +
            " не найден маркер конца блока на протяжении 64Кб");
      }

      // Теперь можно прочитать все байты
      fsDBT.Position = FileOff;
      byte[] buff = new byte[cnt];
      if (fsDBT.Read(buff, 0, cnt) != cnt)
        throw new IOException("Не удалось прочитать значение MEMO-поля");
      return buff;
    }

    /// <summary>
    /// Возвращает true, если поле содержит значение NULL.
    /// Внутренняя структура DBF-файла позволяет отличать значения NULL от 0 и false, но нет от пустой строки.
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
    /// Внутренняя структура DBF-файла позволяет отличать значения NULL от 0 и false, но нет от пустой строки.
    /// Однако СУБД, например, Clipper, могут не различать значения NULL.
    /// Обычно не следует использовать этот метод в прикладном коде.
    /// Возвращает true, если все позиции поля в строке содержат символы "пробел" (0x20)
    /// </summary>
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0</param>
    /// <returns>true, если поле не содержит значения</returns>
    public bool IsDBNull(int fieldIndex)
    {
      string s = Encoding.GetString(_RecordBuffer, _FieldOffsets[fieldIndex], DBStruct[fieldIndex].Length);
      s = s.Trim(' ');
      return s.Length == 0;
    }


    /// <summary>
    /// Возвращает длину заданного поля
    /// Для полей типа 'C' возвращает длину заполненной части строки (от 0 до DbfFieldInfo.Length)
    /// Для полей типа 'M' возвращает длину значения MEMO-поля
    /// Для остальных полей возвращает DbfFieldInfo.Length, если поле заполнено и 0, если DBNull
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns>Длина</returns>
    public int GetLength(string fieldName)
    {
      return GetLength(InternalIndexOfField(fieldName));
    }

    /// <summary>
    /// Возвращает длину заданного поля
    /// Для полей типа 'C' возвращает длину заполненной части строки (от 0 до DbfFieldInfo.Length)
    /// Для полей типа 'M' возвращает длину значения MEMO-поля
    /// Для остальных полей возвращает DbfFieldInfo.Length, если поле заполнено и 0, если DBNull
    /// </summary>
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0</param>
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
          uint MemoBlockEntry = ReadMemoEntry(fieldIndex);
          if (MemoBlockEntry == 0)
            return 0;
          if (fsDBT == null)
            return 0;

          if (MemoBlockEntry < MinMemoBlockCount || MemoBlockEntry > MaxMemoBlockCount)
            return 0;

          long FileOff = (long)MemoBlockEntry * 512L;
          if (fsDBT.Length <= FileOff)
            return 0;

          // Сначала нужно найти символ 1A, который завершает текст
          fsDBT.Position = FileOff;
          int cnt = 0;
          while (true)
          {
            int v = fsDBT.ReadByte();
            if (v < 0)
              throw new DbfFileFormatException("Обнаружен неожиданный конец МЕМО-файла для блока " +
                MemoBlockEntry.ToString() + ", указанного в строке " + RecNo.ToString() + " в поле \"" + DBStruct[fieldIndex].Name);
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
    /// Генерирует исключение, если IsReadOnly=true.
    /// Вызывается пишущими методами
    /// </summary>
    protected void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ReadOnlyException("Таблица данных предназначена только для чтения");
    }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      throw new NotSupportedException("Свойство DbfFile.IsReadOnly устанавливается только в конструкторе и не может быть изменено в дальнейшем");
    }


    /// <summary>
    /// Возвращает true, если было установлено значение какого-либо поля в текущей записи
    /// </summary>
    public bool RecordModified
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
    public bool TableModified { get { return _TableModified; } }
    private bool _TableModified;

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetValue(string fieldName, object value)
    {
      SetValue(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
        SetNullableDate(fieldIndex, (DateTime)value);
        return;
      }
      if (value is Boolean)
      {
        SetBool(fieldIndex, (bool)value);
        return;
      }

      decimal x = DataTools.GetDecimal(value);
      SetDecimal(fieldIndex, x);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="value">Значение</param>
    public void SetString(string fieldName, string value)
    {
      SetString(InternalIndexOfField(fieldName), value);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
    /// </summary>
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
    /// <param name="value">Значение поля</param>
    public void SetString(int fieldIndex, string value)
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

      if (DBStruct[fieldIndex].Type == 'M')
      {
        byte[] Bytes = Encoding.GetBytes(value);
        uint MemoBlockEntry = WriteMemoValue(fieldIndex, Bytes);
        value = MemoBlockEntry.ToString("d10", DataTools.DotNumberConv);
        Encoding.GetBytes(value, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
        _RecordModified = true;
        _TableModified = true;
        return;
      }

      value = DataTools.PadRight(value, DBStruct[fieldIndex].Length);

      Encoding.GetBytes(value, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
      if (fsDBT == null)
        throw new InvalidOperationException("Нельзя записать значение в MEMO-поле, т.к. DBT-файл не открыт");

      // Число необходимых блоков
      int nBlocks = (value.Length + 511 + 1) / 512; // 1 байт для 0x1A

      uint FirstBlock = ReadMemoEntry(fieldIndex);
      int AvailBlocks = GetAvailMemoBlocks(FirstBlock);

      if (AvailBlocks >= nBlocks)
      {
        // Можно заменить существующий блок
        fsDBT.Position = (long)FirstBlock * 512L;
        fsDBT.Write(value, 0, value.Length);
        fsDBT.WriteByte(0x1A);

        // Затираем нулями старые значения
        long sz = (long)(AvailBlocks) * 512;
        long nZeros = sz - value.Length - 1;
        FileTools.WriteZeros(fsDBT, nZeros);
        return FirstBlock; // Возвращаем существующее значение
      }

      FirstBlock = _MemoBlockCount;
      if ((_MemoBlockCount + nBlocks) > MaxMemoBlockCount)
        throw new InvalidOperationException("Нельзя добавить MEMO-значение в DBT-файл, т.к. будет превышено максимально допустимое число блоков " + MaxMemoBlockCount.ToString());

      // Дополняем нулями, если файл короче, чем надо
      FillMemoToBlockCount();

      // Записываем значение
      fsDBT.Write(value, 0, value.Length);
      fsDBT.WriteByte(0x1A);

      _MemoBlockCount += (uint)nBlocks;
      // Дополняем
      FillMemoToBlockCount();

      return FirstBlock;
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
      if (off > fsDBT.Length)
        return 0; // ошибка

      fsDBT.Position = off;
      long cnt = 0;
      while (true)
      {
        int b = fsDBT.ReadByte();
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
      if (sz > fsDBT.Length)
        fsDBT.SetLength(sz);

      // Позиционируемся на конец файла
      fsDBT.Seek(0, SeekOrigin.End);
    }

    /// <summary>
    /// Устанавливает значение поля для текущей строки.
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
          string s = DataTools.PadLeft(value.ToString(DBStruct[fieldIndex].Mask, DataTools.DotNumberConv), DBStruct[fieldIndex].Length);
          Encoding.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
          string s = DataTools.PadLeft(value.ToString(DBStruct[fieldIndex].Mask, DataTools.DotNumberConv), DBStruct[fieldIndex].Length);
          Encoding.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
          string s = DataTools.PadLeft(value.ToString(DBStruct[fieldIndex].Mask, DataTools.DotNumberConv), DBStruct[fieldIndex].Length);
          Encoding.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
          string s = DataTools.PadLeft(value.ToString(DBStruct[fieldIndex].Mask, DataTools.DotNumberConv), DBStruct[fieldIndex].Length);
          Encoding.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
          Encoding.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
          Encoding.GetBytes(s, 0, DBStruct[fieldIndex].Length, _RecordBuffer, _FieldOffsets[fieldIndex]);
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
    /// <param name="fieldIndex">Индекс поля. Нумерация начинается с 0.</param>
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
    /// Возвращает true, если был вызов AppendRecord()
    /// </summary>
    public bool IsAppendRecord { get { return _IsAppendRecord; } }
    private bool _IsAppendRecord;

    /// <summary>
    /// Добавляет новую запись в конец файла.
    /// Предварительно записываются несохраненные изменения.
    /// </summary>
    public void AppendRecord()
    {
      CheckNotReadOnly();
      FlushRecord();

      DataTools.FillArray<byte>(_RecordBuffer, (byte)' ');
      FRecordCount++;
      _RecNo = FRecordCount;
      _IsAppendRecord = true;
      _TableModified = true;
    }

    private void FlushRecord()
    {
      if (IsAppendRecord)
      {
        //fsDBF.Position = DataOffset + (FRecNo - 1) * RecSize;
        fsDBF.Position = (long)DataOffset + ((long)_RecNo - 1L) * (long)RecSize; // 27.11.2018
        fsDBF.Write(_RecordBuffer, 0, RecSize);
        fsDBF.WriteByte((byte)0x1A);

        _IsAppendRecord = false;
        _RecordModified = false;
        return;
      }

      if (_RecordModified)
      {
        //fsDBF.Position = DataOffset + (FRecNo - 1) * RecSize;
        fsDBF.Position = (long)DataOffset + ((long)_RecNo - 1L) * (long)RecSize; // 27.11.2018
        fsDBF.Write(_RecordBuffer, 0, RecSize);
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

      DateTime dt = DateTime.Today;


      // Дата изменения
      int Y2 = dt.Year % 100;
      fsDBF.Position = 1L;
      fsDBF.WriteByte((byte)Y2);
      fsDBF.WriteByte((byte)(dt.Month));
      fsDBF.WriteByte((byte)(dt.Day));

      // Число записей
      fsDBF.Position = 4L;
      BinaryWriter wrtDBF = new BinaryWriter(fsDBF);
      wrtDBF.Write((uint)RecordCount);

      if (fsDBT != null)
      {
        fsDBT.Position = 0L;
        BinaryWriter wrtDbt = new BinaryWriter(fsDBT);
        wrtDbt.Write(_MemoBlockCount);
      }
    }

    #endregion

    #region Преобразование в DataTable

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать все поля DBF-файла.
    /// Для больших DBF-файлов можно использовать метод GetBlockedTable() для загрузки таблицы по частям.
    /// </summary>
    /// <returns>Заполненный объект DataTable</returns>
    public DataTable CreateTable()
    {
      return CreateTable(new DummySplash(), null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать все поля DBF-файла
    /// </summary>
    /// <param name="splash">Интерфейс управления процентным инидкатором.
    /// Если свойство AllowCancel установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <returns>Заполненный объект DataTable</returns>
    public DataTable CreateTable(ISplash splash)
    {
      return CreateTable(splash, null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать все поля DBF-файла.
    /// Эта версия позволяет пропускать ошибочные значения полей, которые не соответствуют типу данных.
    /// Для больших DBF-файлов можно использовать метод GetBlockedTable() для загрузки таблицы по частям.
    /// </summary>
    /// <param name="splash">Интерфейс управления процентным инидкатором.
    /// Если свойство AllowCancel установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <returns>Заполненный объект DataTable</returns>
    public DataTable CreateTable(ISplash splash, ErrorMessageList errors)
    {
      if (splash == null)
        splash = new DummySplash();

      DataTable Table = DBStruct.CreateTable();
      int oldRN = RecNo;
      try
      {
        RecNo = 0;
        splash.PercentMax = this.RecordCount;
        while (Read())
        {
          AddDataTableRow(Table, errors);
          splash.IncPercent();
        }
      }
      finally
      {
        RecNo = oldRN;
      }
      return Table;
    }

    private void AddDataTableRow(DataTable table, ErrorMessageList errors)
    {
      DataRow Row = table.NewRow();
      for (int i = 0; i < DBStruct.Count; i++)
      {
        if (errors == null)
          Row[i] = GetValue(i); // пусть выбрасывает исключение
        else
        {
          try
          {
            Row[i] = GetValue(i);
          }
          catch (DbfValueFormatException e)
          {
            errors.AddError(e.Message);
          }
          catch (Exception e)
          {
            errors.AddError("Строка " + RecNo.ToString() + ", поле \"" + DBStruct[i].Name + "\". Произошла ошибка. " + e.Message);
          }
        }
      }
      table.Rows.Add(Row);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать указанные поля DBF-файла.
    /// Для больших DBF-файлов можно использовать метод GetBlockedTable() для загрузки таблицы по частям.
    /// </summary>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <returns>Заполненный объект DataTable</returns>
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
    /// <param name="splash">Интерфейс управления процентным инидкатором
    /// Если свойство AllowCancel установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <returns>Заполненный объект DataTable</returns>
    public DataTable CreateTable(string columnNames, ISplash splash)
    {
      return CreateTable(columnNames, splash, null);
    }

    /// <summary>
    /// Возвращает заполненную таблицу данных.
    /// Таблица будет содержать указанные поля DBF-файла.
    /// Эта версия позволяет пропускать ошибочные значения полей, которые не соответствуют типу данных.
    /// Для больших DBF-файлов можно использовать метод GetBlockedTable() для загрузки таблицы по частям.
    /// </summary>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <param name="splash">Интерфейс управления процентным инидкатором
    /// Если свойство AllowCancel установлено в true, пользователь сможет прервать процесс загрузки</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <returns>Заполненный объект DataTable</returns>
    public DataTable CreateTable(string columnNames, ISplash splash, ErrorMessageList errors)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw new ArgumentNullException("columnNames");

      if (splash == null)
        splash = new DummySplash();

      int[] DbfColPoss;
      DataTable Table = DBStruct.CreatePartialTable(columnNames, out DbfColPoss);

      int oldRN = RecNo;
      try
      {
        RecNo = 0;
        splash.PercentMax = this.RecordCount;
        while (Read())
        {
          AddDataTableRow(Table, errors, DbfColPoss);
          splash.IncPercent();
        }
      }
      finally
      {
        RecNo = oldRN;
      }
      return Table;
    }

    private void AddDataTableRow(DataTable table, ErrorMessageList errors, int[] dbfColPoss)
    {
      DataRow Row = table.NewRow();
      for (int i = 0; i < dbfColPoss.Length; i++)
      {
        if (errors == null)
          Row[i] = GetValue(dbfColPoss[i]);// пусть выбрасывает исключение
        else
        {
          try
          {
            Row[i] = GetValue(dbfColPoss[i]);
          }
          catch (DbfValueFormatException e)
          {
            errors.AddError(e.Message);
          }
          catch (Exception e)
          {
            errors.AddError("Строка " + RecNo.ToString() + ", поле \"" + DBStruct[dbfColPoss[i]].Name + "\". Произошла ошибка. " + e.Message);
          }
        }
      }
      table.Rows.Add(Row);
    }

    /// <summary>
    /// Добавление значений из таблицы
    /// Идентификация полей выполняется по именам. Лишние поля пропускаются
    /// </summary>
    /// <param name="table"></param>
    public void Append(DataTable table)
    {
      CheckNotReadOnly();
      FlushRecord();

      int[] Maps = new int[table.Columns.Count];
      for (int i = 0; i < Maps.Length; i++)
        Maps[i] = DBStruct.IndexOf(table.Columns[i].ColumnName);

      foreach (DataRow Row in table.Rows)
      {
        AppendRecord();
        for (int i = 0; i < Maps.Length; i++)
        {
          if (Maps[i] >= 0)
            SetValue(Maps[i], Row[i]);
        }
      }

      FlushRecord();
    }


    /// <summary>
    /// Возвращает часть таблицы в виде DataTable.
    /// Таблица будет содержать все поля DBF-файла.
    /// В таблицу входят строки, начиная с очередного вызова Read() и до достижения конца таблицы,
    /// но не более <paramref name="maxRowCount"/> строк.
    /// Метод возвращает false, если в DBF-файле больше нет строк.
    /// Обычно перед вызовом следует установить RecNo=0, а затем вызывать метод в цикле, 
    /// пока возвращается true.
    /// </summary>
    /// <param name="maxRowCount">Максимальное количество строк в возвращаемой таблице (размер блока)</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <param name="table">Сюда помещается заполненная таблица DataTable, содержащая от 1
    /// до <paramref name="maxRowCount"/> строк</param>
    /// <returns>true, если данные прочитаны и false, если DBF-файл не содержит непрочитанных строк</returns>
    public bool CreateBlockedTable(int maxRowCount, ErrorMessageList errors, out DataTable table)
    {
      if (maxRowCount < 1)
        throw new ArgumentOutOfRangeException("maxRowCount");

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
    /// Возвращает часть таблицы в виде DataTable.
    /// Таблица будет содержать указанные поля DBF-файла
    /// В таблицу входят строки, начиная с очередного вызова Read() и до достижения конца таблицы,
    /// но не более <paramref name="maxRowCount"/> строк.
    /// Метод возвращает false, если в DBF-файле больше нет строк.
    /// Обычно перед вызовом следует установить RecNo=0, а затем вызывать метод в цикле, 
    /// пока возвращается true.
    /// </summary>
    /// <param name="maxRowCount">Максимальное количество строк в возвращаемой таблице (размер блока)</param>
    /// <param name="columnNames">Список имен полей DBF-файла через запятую.
    /// Если файл не содержит какого-либо поля, будет сгенерировано исключение</param>
    /// <param name="errors">Если аргумент передан, то при невозможности прочитать значение поля, будет добавлено сообщение об ошибке и чтение будет продолжено.
    /// Если null, то будет выброшено исключение</param>
    /// <param name="table">Сюда помещается заполненная таблица DataTable, содержащая от 1
    /// до <paramref name="maxRowCount"/> строк</param>
    /// <returns>true, если данные прочитаны и false, если DBF-файл не содержит непрочитанных строк</returns>
    public bool CreateBlockedTable(int maxRowCount, string columnNames, ErrorMessageList errors, out DataTable table)
    {
      if (String.IsNullOrEmpty(columnNames))
        throw new ArgumentNullException("columnNames");

      int[] DbfColPoss = null; // без присвоения значения будет ошибка компилятора

      table = null;
      while (Read())
      {
        if (table == null)
          table = DBStruct.CreatePartialTable(columnNames, out DbfColPoss);
        AddDataTableRow(table, errors, DbfColPoss);
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
     * 1. Создать два объекта DbfFile (источник и приемник)
     * 2. Создать объект DbfFileCopier
     * 3. Заполнить список полей для копирования
     * 4. Присоединить дополнительные обработчики к DbfFileCopier, если нужно
     * 5. Вызвать метод DbfFileCopier.AppendRecords()
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
        throw new ArgumentException("Конечный и исходный файлы не могут совпадать", "resFile");
      if (resFile.IsReadOnly)
        throw new ReadOnlyException("Конечный файл открыт только для чтения");

      _SrcFile = srcFile;
      _ResFile = resFile;

      MainCopyList = new Dictionary<int, int>();
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
    private readonly Dictionary<int, int> MainCopyList;

    /// <summary>
    /// Добавить правило для копирования.
    /// </summary>
    /// <param name="srcFieldIndex">Индекс столбца в исходной таблице</param>
    /// <param name="resFieldIndex">Индекс столбца в заполняемой таблице</param>
    public void AddField(int srcFieldIndex, int resFieldIndex)
    {
      CheckNotReadOnly();

      if (srcFieldIndex < 0 || srcFieldIndex >= SrcFile.DBStruct.Count)
        throw new ArgumentOutOfRangeException("srcFieldIndex", srcFieldIndex, "Индекс исходного поля должен быть в диапазоне от 0 до " + (SrcFile.DBStruct.Count - 1).ToString());
      if (resFieldIndex < 0 || resFieldIndex >= ResFile.DBStruct.Count)
        throw new ArgumentOutOfRangeException("resFieldIndex", resFieldIndex, "Индекс конечного поля должен быть в диапазоне от 0 до " + (ResFile.DBStruct.Count - 1).ToString());
      if (MainCopyList.ContainsKey(resFieldIndex))
        throw new InvalidOperationException("Для конечного поля \"" + ResFile.DBStruct[resFieldIndex].Name + "\" уже задано правило копирования");

      MainCopyList.Add(resFieldIndex, srcFieldIndex);
    }


    /// <summary>
    /// Добавить правило для копирования.
    /// </summary>
    /// <param name="srcFieldName">Имя столбца в исходной таблице</param>
    /// <param name="resFieldName">Имя столбца в заполняемой таблице</param>
    public void AddField(string srcFieldName, string resFieldName)
    {
      int SrcFieldIndex = SrcFile.DBStruct.IndexOf(srcFieldName);
      if (SrcFieldIndex < 0)
        throw new ArgumentException("Исходная таблица не содержит поля \"" + srcFieldName + "\"", "srcFieldName");
      int ResFieldIndex = ResFile.DBStruct.IndexOf(resFieldName);
      if (ResFieldIndex < 0)
        throw new ArgumentException("Конечная таблица не содержит поля \"" + resFieldName + "\"", "resFieldName");
      AddField(SrcFieldIndex, ResFieldIndex);
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
      for (int ResFieldIndex = 0; ResFieldIndex < ResFile.DBStruct.Count; ResFieldIndex++)
      {
        if (MainCopyList.ContainsKey(ResFieldIndex))
          continue; // уже есть правило

        string FieldName = ResFile.DBStruct[ResFieldIndex].Name;

        if (exceptedFields != null)
        {
          bool ExceptFlag = false;
          for (int i = 0; i < exceptedFields.Length; i++)
          {
            if (String.Equals(FieldName, exceptedFields[i], StringComparison.OrdinalIgnoreCase))
            {
              ExceptFlag = true;
              break;
            }
          }
          if (ExceptFlag)
            continue;
        }

        int SrcFieldIndex = SrcFile.DBStruct.IndexOf(FieldName);
        if (SrcFieldIndex < 0)
          continue; // Нет такого поля

        if (SrcFile.DBStruct[SrcFieldIndex].Type == 'M' && (!SrcFile.HasMemoFile))
          continue;

        AddField(SrcFieldIndex, ResFieldIndex);
      }
    }

    /// <summary>
    /// Генерирует исключение, если список правил переведен в режим "только чтение"
    /// </summary>
    protected void CheckNotReadOnly()
    {
      if (DirectCopyList != null)
        throw new ReadOnlyException("Объект уже использован для копирования и не может быть модифицирован");
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
    /// преобразование byte[]-byte[] иди прямое копирование.
    /// Иначе требуется преобразование byte[]-string-byte[]
    /// </summary>
    private SingleByteTranscoding Transcoding;

    #endregion

    #region Внутренние списки для копирования

    [StructLayout(LayoutKind.Auto)]
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
    private List<DirectCopyItem> DirectCopyList;


    [StructLayout(LayoutKind.Auto)]
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
    private List<SpaceFillItem> SpaceFillList;

    /// <summary>
    /// Буфер, содержащий символы ' '
    /// </summary>
    private byte[] SpaceBuffer;

    [StructLayout(LayoutKind.Auto)]
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
    private List<FieldPair> ByValueCopyList;

    /// <summary>
    /// Выполнение подготовительных действий.
    /// Вызывается из AppendRecords()
    /// </summary>
    protected void Prepare()
    {
      if (DirectCopyList != null)
        return; // повторный вызов

      if (SingleByteTranscoding.CanCreate(SrcFile.Encoding, ResFile.Encoding))
        Transcoding = new SingleByteTranscoding(SrcFile.Encoding, ResFile.Encoding);

      #region Заполнение списков

      DirectCopyList = new List<DirectCopyItem>();
      SpaceFillList = new List<SpaceFillItem>();
      ByValueCopyList = new List<FieldPair>();

      foreach (KeyValuePair<int, int> Pair in MainCopyList)
      {
        if (TestDirectCopy(Pair.Value, Pair.Key))
          continue;

        // Медленное копирование
        ByValueCopyList.Add(new FieldPair(Pair.Value, Pair.Key));
      }

      #endregion

      #region Cписок для прямого копирования

      DirectCopyList.Sort();

      for (int i = DirectCopyList.Count - 1; i > 0; i--)
      {
        DirectCopyItem Item1 = DirectCopyList[i - 1];
        DirectCopyItem Item2 = DirectCopyList[i];
        if ((Item1.SrcStart + Item1.Len == Item2.SrcStart) &&
          (Item1.ResStart + Item1.Len == Item2.ResStart))
        {
          // Два блока подряд
          DirectCopyItem Item3 = new DirectCopyItem(Item1.SrcStart, Item1.ResStart, Item1.Len + Item2.Len);
          DirectCopyList[i - 1] = Item3;
          DirectCopyList.RemoveAt(i);
        }
      }

      #endregion

      #region Cписок для заполнения пробелами

      /*
       * Оптимизация этого списка маловероятна.
       * Единственное возможное исключение: Сначала идет строковое поле, затем числовое, и оба поля длиннее
       * чем в исходной таблице
       */

      SpaceFillList.Sort();

      for (int i = SpaceFillList.Count - 1; i > 0; i--)
      {
        SpaceFillItem Item1 = SpaceFillList[i - 1];
        SpaceFillItem Item2 = SpaceFillList[i];
        if (Item1.ResStart + Item1.Len == Item2.ResStart)
        {
          // Два блока подряд
          SpaceFillItem Item3 = new SpaceFillItem(Item1.ResStart, Item1.Len + Item2.Len);
          SpaceFillList[i - 1] = Item3;
          SpaceFillList.RemoveAt(i);
        }
      }

      int MaxLen = 0;
      for (int i = 0; i < SpaceFillList.Count; i++)
        MaxLen = Math.Max(MaxLen, SpaceFillList[i].Len);
      if (MaxLen > 0)
      {
        SpaceBuffer = new byte[MaxLen];
        DataTools.FillArray<byte>(SpaceBuffer, (byte)' ');
      }

      #endregion
    }

    private bool TestDirectCopy(int srcFieldIndex, int resFieldIndex)
    {
      DbfFieldInfo SrcField = SrcFile.DBStruct[srcFieldIndex];
      DbfFieldInfo ResField = ResFile.DBStruct[resFieldIndex];
      if (SrcField.Type != ResField.Type)
        return false;

      int SrcOff = SrcFile.GetFieldOffset(srcFieldIndex);
      int ResOff = ResFile.GetFieldOffset(resFieldIndex);

      switch (SrcField.Type)
      {
        case 'M':
          return false;

        case 'C':
          if (Transcoding != null)
          {
            DirectCopyList.Add(new DirectCopyItem(SrcOff, ResOff, Math.Min(SrcField.Length, ResField.Length)));
            if (ResField.Length > SrcField.Length)
              SpaceFillList.Add(new SpaceFillItem(ResOff + SrcField.Length, ResField.Length - SrcField.Length));
            return true;
          }
          else
            return true;

        case 'D':
        case 'L':
          DirectCopyList.Add(new DirectCopyItem(SrcOff, ResOff, SrcField.Length));
          return true;

        case 'N':
          if (SrcField.Precision != ResField.Precision)
            return false;
          if (SrcField.Length > ResField.Length)
            return false; // нельзя укорачивать длину
          DirectCopyList.Add(new DirectCopyItem(SrcOff, ResOff, Math.Min(SrcField.Length, ResField.Length)));
          if (ResField.Length > SrcField.Length)
            SpaceFillList.Add(new SpaceFillItem(ResOff + SrcField.Length, ResField.Length - SrcField.Length));
          return true;
        case 'F':
          // ??? Наверное, можно выполнять удлинение, как и для 'N'
          return SrcField.Length == ResField.Length && SrcField.Precision == ResField.Precision;
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
      sb.Append(DirectCopyList.Count.ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Space fill blocks: ");
      sb.Append(SpaceFillList.Count.ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Transcoding: ");
      if (Transcoding == null)
        sb.Append("Not used");
      else if (Transcoding.IsDirect)
        sb.Append("Direct copy");
      else
        sb.Append(Transcoding.ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Slow copy fields: ");
      sb.Append(ByValueCopyList.Count.ToString());
      for (int i = 0; i < ByValueCopyList.Count; i++)
      {
        sb.Append(Environment.NewLine);
        sb.Append("  [");
        sb.Append(i);
        sb.Append("] ");
        DbfFieldInfo SrcField = SrcFile.DBStruct[ByValueCopyList[i].SrcFieldIndex];
        DbfFieldInfo ResField = ResFile.DBStruct[ByValueCopyList[i].ResFieldIndex];
        sb.Append(SrcField.Name);
        sb.Append("(");
        sb.Append(SrcField.TypeSizeText);
        sb.Append(") -> ");
        sb.Append(ResField.Name);
        sb.Append("(");
        sb.Append(ResField.TypeSizeText);
        sb.Append(")");
      }

      return sb.ToString();
    }

#endif

    #endregion

    #region Копирование

    /// <summary>
    /// Основной метод копирования.
    /// Перебирает все записи в исходной таблицы и вызывает AppendRecord в конечной из них для каждой строки.
    /// Для больших таблиц рекомендуется использовать перегрузку с аргументом Splash, чтобы пользователь мог прервать процесс.
    /// </summary>
    public void AppendRecords()
    {
      AppendRecords(null);
    }

    /// <summary>
    /// Основной метод копирования.
    /// Перебирает все записи в исходной таблицы и вызывает AppendRecord в конечной из них для каждой строки.
    /// </summary>
    /// <param name="splash">Управляемая splash-заставка. Если null, то пользователь не может прервать процесс.</param>
    public void AppendRecords(ISplash splash)
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
      byte[] SrcBuf = SrcFile.RecordBuffer;
      byte[] ResBuf = ResFile.RecordBuffer;

      #region Прямое копирование

      for (int i = 0; i < DirectCopyList.Count; i++)
      {
        DirectCopyItem Item = DirectCopyList[i];
        if (Transcoding == null)
          Array.Copy(SrcBuf, Item.SrcStart, ResBuf, Item.ResStart, Item.Len);
        else
          Transcoding.Transcode(SrcBuf, Item.SrcStart, ResBuf, Item.ResStart, Item.Len);
      }

      #endregion

      #region Заполнение пробелами

      for (int i = 0; i < SpaceFillList.Count; i++)
        Array.Copy(SpaceBuffer, 0, ResBuf, SpaceFillList[i].ResStart, SpaceFillList[i].Len);

      #endregion

      #region Медленное копирование

      for (int i = 0; i < ByValueCopyList.Count; i++)
      {
        object x = SrcFile.GetValue(ByValueCopyList[i].SrcFieldIndex);
        ResFile.SetValue(ByValueCopyList[i].ResFieldIndex, x);
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

  /// <summary>
  /// Прямой перекодировщик byte[]-byte[] без промежуточного преобразования в строку.
  /// Может быть применен только к однобайтным кодовым страницам
  /// </summary>
  public class SingleByteTranscoding
  {
    #region Конструктор

    /// <summary>
    /// Создает перекодировщик
    /// </summary>
    /// <param name="srcEncoding">Исходная кодировка</param>
    /// <param name="resEncoding">Конечная кодировка</param>
    public SingleByteTranscoding(Encoding srcEncoding, Encoding resEncoding)
    {
      if (srcEncoding == null)
        throw new ArgumentNullException("srcEncoding");
      if (resEncoding == null)
        throw new ArgumentNullException("resEncoding");

      if (!CanCreate(srcEncoding, resEncoding))
        throw new ArgumentException("Нельзя выполнять прямое перекодирование из " + srcEncoding.ToString() + " в " + resEncoding.ToString());

      _SrcEncoding = srcEncoding;
      _ResEncoding = resEncoding;
      _IsDirect = (resEncoding.CodePage == srcEncoding.CodePage);

      if (!_IsDirect)
      {
        TranscodeTable = new byte[256];
        for (int i = 0; i < 256; i++)
          TranscodeTable[i] = (byte)i;

        // Преобразуется только вторая половина таблицы
        string s = srcEncoding.GetString(TranscodeTable, 128, 128);
        resEncoding.GetBytes(s, 0, 128, TranscodeTable, 128);
      }
    }

    /// <summary>
    /// Возвращает true, если можно создать перекодировщик для заданных кодировок
    /// </summary>
    /// <param name="srcEncoding">Исходная кодировка</param>
    /// <param name="resEncoding">Конечная кодировка</param>
    /// <returns>true, если обе кодировки являются однобайтными</returns>
    public static bool CanCreate(Encoding srcEncoding, Encoding resEncoding)
    {
      return srcEncoding.IsSingleByte && resEncoding.IsSingleByte;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Исходная кодировка
    /// </summary>
    public Encoding SrcEncoding { get { return _SrcEncoding; } }
    private readonly Encoding _SrcEncoding;

    /// <summary>
    /// Конечная кодировка
    /// </summary>
    public Encoding ResEncoding { get { return _ResEncoding; } }
    private readonly Encoding _ResEncoding;

    /// <summary>
    /// Возвращает true, если исходная и конечная кодировки совпадают и перекодирование не нужно.
    /// Вместо него можно использовать прямое копирование
    /// </summary>
    public bool IsDirect { get { return _IsDirect; } }
    private bool _IsDirect;

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return SrcEncoding.CodePage.ToString() + "->" + ResEncoding.CodePage.ToString();
    }

    #endregion

    #region Перекодирование

    /// <summary>
    /// Таблица перекодирования размером 256 байт, если IsDirect=false
    /// </summary>
    private byte[] TranscodeTable;

    /// <summary>
    /// Выполнить преобразование byte[]-byte[] для части массива.
    /// Если IsDirect=true, вызывается Array.Copy()
    /// </summary>
    /// <param name="srcArray">Исходный массив байт, задающий символы в кодировке SrcEncoding</param>
    /// <param name="srcIndex">Начальная позиция в массиве <paramref name="srcArray"/></param>
    /// <param name="resArray">Заполняемый массив байт, задающий символы в кодировке ResEncoding</param>
    /// <param name="resIndex">Начальная позиция в массиве <paramref name="resArray"/></param>
    /// <param name="length">Количество байт для перекодировки</param>
    public void Transcode(byte[] srcArray, int srcIndex, byte[] resArray, int resIndex, int length)
    {
#if DEBUG
      if (srcArray == null)
        throw new ArgumentNullException("srcArray");
      if (resArray == null)
        throw new ArgumentNullException("resArray");

      if (srcArray.Rank != 1)
        throw new ArgumentException("Исходный массив должен быть одномерным", "srcArray");
      if (resArray.Rank != 1)
        throw new ArgumentException("Заполняемый массив должен быть одномерным", "resArray");
#endif

      if (_IsDirect)
        Array.Copy(srcArray, srcIndex, resArray, resIndex, length);
      else
      {
        for (int i = 0; i < length; i++)
        {
          int x = srcArray[srcIndex + i];
          resArray[resIndex + i] = TranscodeTable[x];
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование byte[]-byte[] для целого массива.
    /// Если IsDirect=true, вызывается Array.Copy()
    /// </summary>
    /// <param name="srcArray">Исходный массив байт, задающий символы в кодировке SrcEncoding</param>
    /// <param name="resArray">Заполняемый массив байт, задающий символы в кодировке ResEncoding</param>
    public void Transcode(byte[] srcArray, byte[] resArray)
    {
      if (resArray.Length != srcArray.Length)
        throw new ArgumentException("Массивы должны быть одной длины");
      Transcode(srcArray, 0, resArray, 0, srcArray.Length);
    }

    #endregion
  }
}
