// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using System;
using System.Text;
using System.Collections.Generic;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.IO
{
  /// <summary>
  /// Пара "Ключ-Значение" для извлечения из Ini-файла
  /// </summary>
  public sealed class IniKeyValue : IObjectWithCode
  {
    // Не может быть структурой, так как используется в качестве элемента хранения внутри IniFile

    #region Конструктор

    /// <summary>
    /// Создает пару "Ключ-Значение"
    /// </summary>
    /// <param name="keyName">Имя параметра</param>
    /// <param name="value">Значение</param>
    public IniKeyValue(string keyName, string value)
    {
      _KeyName = keyName;
      _Value = value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ключ
    /// </summary>
    public string KeyName { get { return _KeyName; } }
    private readonly string _KeyName;

    /// <summary>
    /// Значение
    /// </summary>
    public string Value { get { return _Value; } }
    private readonly string _Value;

    /// <summary>
    /// Возвращает пару "Ключ=Значение"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return KeyName + "=" + Value;
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code
    {
      get { return KeyName; }
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс доступа к INI-файлам.
  /// Доступ может быть реализован путем обычного чтения-записи файла (класс <see cref="IniFile"/>)
  /// или вызовами Windows API (класс <see cref="FreeLibSet.Win32.IniFileWindows"/>)
  /// </summary>
  public interface IIniFile : IReadOnlyObject
  {
    #region Секции

    /// <summary>
    /// Возвращает массив имен всех секций
    /// </summary>
    /// <returns>Массив имен</returns>
    string[] GetSectionNames();

    /// <summary>
    /// Удаление секции и всех значений в ней.
    /// Если секция не существует, никаких действий не выполняется.
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    void DeleteSection(string sectionName);

    #endregion

    #region Значения

    /// <summary>
    /// Чтение и запись строкового значения.
    /// Если при чтении нет такой секции или ключа, возвращается пустое значение.
    /// При записи несуществующего значения выполняется создание секции или ключа
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="keyName">Имя параметра. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <returns>Строковое значение</returns>
    string this[string sectionName, string keyName] { get; set; }

    /// <summary>
    /// Получение строкого значения с указанием значения по умолчанию
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="keyName">Имя параметра. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <returns>Строковое значение</returns>
    string GetString(string sectionName, string keyName, string defaultValue);

    /// <summary>
    /// Возвращает массив имен всех параметров для заданной секции
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <returns>Массив имен</returns>
    string[] GetKeyNames(string sectionName);

    /// <summary>
    /// Возвращает объект, для которого можно вызвать foreach по парам "Ключ-Значение"
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <returns>Объект, реализующий интерфейс <see cref="IEnumerable{IniKeyValue}"/></returns>
    IEnumerable<IniKeyValue> GetKeyValues(string sectionName);

    /// <summary>
    /// Удаление параметра из секции.
    /// Если секция или значение не существуют, никаких действий не выполняется.
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="keyName">Имя параметра. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    void DeleteKey(string sectionName, string keyName);

    #endregion
  }

  /// <summary>
  /// Доступ к INI-файлу в режиме чтения и записи файла как целого.
  /// Работает, в том числе и на платформах, отличных от Windows.
  /// Для загрузки значений и сохранения изменений используйте методы <see cref="Load(AbsPath)"/> и <see cref="Save(AbsPath)"/>.
  /// Для работы с INI-файлами с помощью функций Windows, используйте класс <see cref="FreeLibSet.Win32.IniFileWindows"/>.
  /// </summary>
  public class IniFile : IIniFile
  {
    #region Вложенные классы

    /// <summary>
    /// Секция конфигурации
    /// </summary>
    private class IniSection : NamedList<IniKeyValue>, IObjectWithCode
    {
      #region Конструктор

      public IniSection(string sectionName)
        : base(true)
      {
        _SectionName = sectionName;
      }

      #endregion

      #region Свойства

      public string SectionName { get { return _SectionName; } }
      private readonly string _SectionName;

      string IObjectWithCode.Code { get { return _SectionName; } }

      #endregion
    }

    /// <summary>
    /// Список секций конфигурации
    /// </summary>
    private class IniSectionList : NamedList<IniSection>
    {
      #region Конструктор

      public IniSectionList()
        : base(true)
      {
      }

      #endregion

      //#region SetReadOnly

      //public new void SetReadOnly()
      //{
      //  base.SetReadOnly();
      //}

      //#endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает пустой список секций.
    /// Список доступен и для чтения и для записи.
    /// </summary>
    public IniFile()
      : this(false)
    {
    }

    /// <summary>
    /// Создает пустой список секций с указанием возможности доступа в режиме "Только чтения" (свойство <see cref="IsReadOnly"/>).
    /// В режиме "Только чтение" можно загрузить данные из файла методами Load().
    /// </summary>
    /// <param name="isReadOnly">Если true, то список будет доступен только для чтения</param>
    public IniFile(bool isReadOnly)
    {
      _Sections = new IniSectionList();
      _IsReadOnly = isReadOnly; // 28.12.2020
    }

    #endregion

    #region Методы загрузки и сохранения

    #region Чтение

    /// <summary>
    /// Загружает данные из заданного файла, используя кодировку, принятую по умолчанию
    /// </summary>
    /// <param name="filePath">Путь к файлу. Файл должен существовать</param>
    public void Load(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      using (StreamReader oReader = new StreamReader(filePath.Path))
      {
        DoLoad(oReader);
        oReader.Close();
      }
      _FilePath = filePath;
    }

    /// <summary>
    /// Загружает данные из заданного файла
    /// </summary>
    /// <param name="filePath">Путь к файлу. Файл должен существовать</param>
    /// <param name="encoding">Кодировка</param>
    public void Load(AbsPath filePath, Encoding encoding)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      using (StreamReader oReader = new StreamReader(filePath.Path, encoding))
      {
        DoLoad(oReader);
        oReader.Close();
      }
      _FilePath = filePath;
    }

    /// <summary>
    /// Загружает данные из потока
    /// </summary>
    /// <param name="stream">Поток для чтения</param>
    /// <param name="encoding">Кодировка</param>
    public void Load(Stream stream, Encoding encoding)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      using (StreamReader oReader = new StreamReader(stream, encoding))
      {
        DoLoad(oReader);
      }
    }

    private void DoLoad(StreamReader oReader)
    {
      _FilePath = AbsPath.Empty;

      Regex regexComment = new Regex("^([\\s]*#.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
      Regex regexSection = new Regex("^[\\s]*\\[[\\s]*([^\\[\\s].*[^\\s\\]])[\\s]*\\][\\s]*$", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
      Regex regexKey = new Regex("^\\s*([^=]*[^\\s=])\\s*=(.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));

      while (!oReader.EndOfStream)
      {
        string line = oReader.ReadLine().Trim();
        if (String.IsNullOrEmpty(line))
          continue;
        Match m;

        m = regexComment.Match(line);
        if (m.Success)
          continue; // комментарии не сохраняем

        m = regexSection.Match(line);
        if (m.Success)
        {
          IniSection sect = _Sections[m.Groups[1].Value];
          if (sect == null)
            sect = new IniSection(m.Groups[1].Value);
          else
            _Sections.Remove(sect);
          _Sections.Add(sect);
          continue;
        }

        m = regexKey.Match(line);
        if (m.Success && _Sections.Count > 0)
        {
          IniSection sect = _Sections[_Sections.Count - 1];
          IniKeyValue v = new IniKeyValue(m.Groups[1].Value, m.Groups[2].Value);
          sect.Remove(v.KeyName); // на случай ошибки
          sect.Add(v);
        }
      }
    }

    #endregion

    #region Запись

    /// <summary>
    /// Записывает данные в файл, используя кодировку, принятую по умолчанию.
    /// Этот метод нельзя вызывать при <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="filePath">Имя файла</param>
    public void Save(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      CheckNotReadOnly();
      using (StreamWriter oWriter = new StreamWriter(filePath.Path, false))
      {
        DoSave(oWriter);
        oWriter.Close();
      }
      _FilePath = filePath;
    }

    /// <summary>
    /// Записывает данные в файл в указанной кодировке.
    /// Этот метод нельзя вызывать при <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="filePath">Имя файла</param>
    /// <param name="encoding">Кодировка</param>
    public void Save(AbsPath filePath, Encoding encoding)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      CheckNotReadOnly();
      using (StreamWriter oWriter = new StreamWriter(filePath.Path, false, encoding))
      {
        DoSave(oWriter);
        oWriter.Close();
      }
      _FilePath = filePath;
    }

    /// <summary>
    /// Записывает данные в поток.
    /// Этот метод нельзя вызывать при <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="stream">Поток</param>
    /// <param name="encoding">Кодировка</param>
    public void Save(Stream stream, Encoding encoding)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      CheckNotReadOnly();
      using (StreamWriter oWriter = new StreamWriter(stream, encoding))
      {
        DoSave(oWriter);
      }
    }

    private void DoSave(StreamWriter oWriter)
    {
      _FilePath = AbsPath.Empty;
      foreach (IniSection sect in _Sections)
      {
        oWriter.WriteLine(String.Format("[{0}]", sect.SectionName));
        foreach (IniKeyValue v in sect)
        {
          oWriter.WriteLine(String.Format("{0}={1}", v.KeyName, v.Value));
        }
      }
    }

    private AbsPath _FilePath;

    /// <summary>
    /// Возвращает имя файла, для которого был вызван <see cref="Load(AbsPath)"/> или <see cref="Save(AbsPath)"/> или "no file"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_FilePath.IsEmpty)
        return "no file";
      else
        return _FilePath.Path;
    }

    #endregion

    #endregion

    #region Список секций

    private readonly IniSectionList _Sections;

    #endregion

    #region IIniFile Members

    /// <summary>
    /// Чтение и запись строкового значения.
    /// Если при чтении нет такой секции или ключа, возвращается пустое значение <see cref="string.Empty"/>.
    /// При записи несуществующего значения выполняется создание секции или ключа.
    /// Нельзя устанавливать свойство при <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="keyName">Имя параметра. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <returns>Строковое значение</returns>
    public string this[string sectionName, string keyName]
    {
      get
      {
        return GetString(sectionName, keyName, String.Empty);
      }
      set
      {
        ValidateSectionName(sectionName);
        ValidateKeyName(keyName);
        if (value == null)
          value = String.Empty;
        CheckNotReadOnly();
        IniSection sect = _Sections[sectionName];
        if (sect == null)
        {
          sect = new IniSection(sectionName);
          _Sections.Add(sect);
        }

        IniKeyValue v = new IniKeyValue(keyName, value);
        sect.Remove(keyName);
        sect.Add(v);
      }
    }

    /// <summary>
    /// Получение строкого значения с указанием значения по умолчанию
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="keyName">Имя параметра. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <returns>Строковое значение</returns>
    public string GetString(string sectionName, string keyName, string defaultValue)
    {
      ValidateSectionName(sectionName);
      ValidateKeyName(keyName);

      IniSection sect = _Sections[sectionName];
      if (sect == null)
        return defaultValue;
      IniKeyValue v = sect[keyName];
      //if (v.KeyName == null)
      if (v == null) // исправлено 08.11.2019
        return defaultValue;
      else
        return v.Value;
    }

    /// <summary>
    /// Возвращает массив имен всех секций
    /// </summary>
    /// <returns>Массив имен</returns>
    public string[] GetSectionNames()
    {
      return _Sections.GetCodes();
    }

    /// <summary>
    /// Возвращает массив имен всех параметров для заданной секции.
    /// Возвращает пустой массив строк, если секции не существует.
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <returns>Массив имен</returns>
    public string[] GetKeyNames(string sectionName)
    {
      ValidateSectionName(sectionName);

      IniSection sect = _Sections[sectionName];
      if (sect == null)
        return EmptyArray<string>.Empty;
      else
        return sect.GetCodes();
    }

    /// <summary>
    /// Удаление секции и всех значений в ней.
    /// Если секция не существует, никаких действий не выполняется.
    /// Этот метод нельзя вызывать при <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    public void DeleteSection(string sectionName)
    {
      ValidateSectionName(sectionName);

      CheckNotReadOnly();
      _Sections.Remove(sectionName);
    }

    /// <summary>
    /// Удаление параметра из секции.
    /// Если секция или значение не существуют, никаких действий не выполняется.
    /// Этот метод нельзя вызывать при <see cref="IsReadOnly"/>=true.
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <param name="keyName">Имя параметра. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    public void DeleteKey(string sectionName, string keyName)
    {
      ValidateSectionName(sectionName);
      ValidateKeyName(keyName);

      CheckNotReadOnly();
      IniSection sect = _Sections[sectionName];
      if (sect == null)
        return;
      sect.Remove(keyName);
    }

    /// <summary>
    /// Возвращает объект, для которого можно вызвать foreach по парам "Ключ-Значение"
    /// </summary>
    /// <param name="sectionName">Имя секции. Не может быть пустой строкой. Регистр символов не имеет значения</param>
    /// <returns>Объект, реализующий интерфейс <see cref="IEnumerable"/></returns>
    public IEnumerable<IniKeyValue> GetKeyValues(string sectionName)
    {
      ValidateSectionName(sectionName);
      IniSection sect = _Sections[sectionName];
      if (sect == null)
        return new DummyEnumerable<IniKeyValue>();
      else
        return sect;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если разрешено только чтение, но не запись.
    /// Задается в конструкторе
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private readonly bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion

    #region Проверка имени секции и ключа

    private static readonly CharArrayIndexer _SectionBadChars = new CharArrayIndexer("[]");

    /// <summary>
    /// Проверка корректности имени секции конфигурации.
    /// Имя не может содержать символы "[" и "]".
    /// Пустая строка считается ошибочной.
    /// </summary>
    /// <param name="sectionName">Проверяемое имя</param>
    /// <param name="errorText">Сюда текст сообщения об ошибке</param>
    /// <returns>true, если имя правильное</returns>
    public static bool IsValidSectionName(string sectionName, out string errorText)
    {
      if (String.IsNullOrEmpty(sectionName))
      {
        errorText = Res.IniFile_Err_SectionNameIsEmpty;
        return false;
      }
      if (sectionName[0] == ' ' || sectionName[sectionName.Length - 1] == ' ')
      {
        errorText = Res.IniFile_Err_SectionNameStartsOrEndsWithSpace;
        return false;
      }

      int p = _SectionBadChars.IndexOfAny(sectionName);
      if (p >= 0)
      {
        errorText = String.Format(Res.IniFile_Err_SectionNameInvalidChar, sectionName[p], p + 1);
        return false;
      }
      errorText = null;
      return true;
    }

    internal static void ValidateSectionName(string sectionName)
    {
      if (String.IsNullOrEmpty(sectionName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("sectionName");
      string errorText;
      if (!IsValidSectionName(sectionName, out errorText))
        throw new ArgumentException(String.Format(Res.IniFile_Arg_SectionName, sectionName, errorText), "sectionName");
    }


    private static readonly CharArrayIndexer _KeyBadChars = new CharArrayIndexer("[]=");

    /// <summary>
    /// Проверка корректности имени ключа.
    /// Имя не может содержать символы "=", "[" и "]".
    /// Пустая строка считается ошибочной.
    /// </summary>
    /// <param name="keyName">Проверяемое имя</param>
    /// <param name="errorText">Сюда текст сообщения об ошибке</param>
    /// <returns>true, если имя правильное</returns>
    public static bool IsValidKeyName(string keyName, out string errorText)
    {
      if (String.IsNullOrEmpty(keyName))
      {
        errorText = Res.IniFile_Err_KeyNameIsEmpty;
        return false;
      }
      if (keyName[0] == ' ' || keyName[keyName.Length - 1] == ' ')
      {
        errorText = Res.IniFile_Err_KeyNameStartsOrEndsWithSpace;
        return false;
      }

      int p = _SectionBadChars.IndexOfAny(keyName);
      if (p >= 0)
      {
        errorText = String.Format(Res.IniFile_Err_SectionNameInvalidChar, keyName[p], p + 1);
        return false;
      }
      errorText = null;
      return true;
    }

    internal static void ValidateKeyName(string keyName)
    {
      if (String.IsNullOrEmpty(keyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("keyName");
      string errorText;
      if (!IsValidSectionName(keyName, out errorText))
        throw new ArgumentException(String.Format(Res.IniFile_Arg_SectionName, keyName, errorText), "keyName");
    }

    #endregion
  }
}
