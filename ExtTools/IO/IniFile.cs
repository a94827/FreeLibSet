﻿// Part of FreeLibSet.
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
  public class IniKeyValue : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает пару "Ключ-Значение"
    /// </summary>
    /// <param name="key">Имя параметра</param>
    /// <param name="value">Значение</param>
    public IniKeyValue(string key, string value)
    {
      _Key = key;
      _Value = value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ключ
    /// </summary>
    public string Key { get { return _Key; } }
    private string _Key;

    /// <summary>
    /// Значение
    /// </summary>
    public string Value { get { return _Value; } }
    private string _Value;

    /// <summary>
    /// Возвращает пару "Ключ=Значение"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Key + "=" + Value;
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code
    {
      get { return Key; }
    }

    #endregion
  }

  /// <summary>
  /// Интефрейс доступа к INI-файлам.
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
    /// <param name="section">Имя секции</param>
    void DeleteSection(string section);

    #endregion

    #region Значения

    /// <summary>
    /// Чтение и запись строкового значения.
    /// Если при чтении нет такой секции или ключа, возвращается пустое значение.
    /// При записи несуществующего значения выполняется создание секции или ключа
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <param name="key">Имя параметра</param>
    /// <returns>Строковое значение</returns>
    string this[string section, string key] { get; set; }

    /// <summary>
    /// Получение строкого значения с указанием значения по умолчанию
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <param name="key">Имя параметра</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <returns>Строковое значение</returns>
    string GetString(string section, string key, string defaultValue);

    /// <summary>
    /// Возвращает массив имен всех параметров для заданной секции
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <returns>Массив имен</returns>
    string[] GetKeyNames(string section);

    /// <summary>
    /// Возвращает объект, для которого можно вызвать foreach по парам "Ключ-Значение"
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <returns>Объект, реализующий интерфейс IEnumerable</returns>
    IEnumerable<IniKeyValue> GetKeyValues(string section);

    /// <summary>
    /// Удаление параметра из секции.
    /// Если секция или значение не существуют, никаких действий не выполняется.
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <param name="key">Имя параметра</param>
    void DeleteKey(string section, string key);

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

      public IniSection(string section)
        : base(true)
      {
        _Section = section;
      }

      #endregion

      #region Свойства

      public string Section { get { return _Section; } }
      private string _Section;

      string IObjectWithCode.Code { get { return _Section; } }

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
    /// Создает пустой список секций
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
        throw new ArgumentNullException("filePath");

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
        throw new ArgumentNullException("filePath");

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
          sect.Remove(v.Key); // на случай ошибки
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
        throw new ArgumentNullException("filePath");

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
        throw new ArgumentNullException("filePath");

      CheckNotReadOnly();
      using (StreamWriter oWriter = new StreamWriter(filePath.Path, false, encoding))
      {
        DoSave(oWriter);
        oWriter.Close();
      }
      _FilePath = filePath;
    }

    /// <summary>
    /// Записывает данные в поток
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
        oWriter.WriteLine(String.Format("[{0}]", sect.Section));
        foreach (IniKeyValue v in sect)
        {
          oWriter.WriteLine(String.Format("{0}={1}", v.Key, v.Value));
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

    private IniSectionList _Sections;

    #endregion

    #region IIniFile Members

    /// <summary>
    /// Чтение и запись строкового значения.
    /// Если при чтении нет такой секции или ключа, возвращается пустое значение.
    /// При записи несуществующего значения выполняется создание секции или ключа
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <param name="key">Имя параметра</param>
    /// <returns>Строковое значение</returns>
    public string this[string section, string key]
    {
      get
      {
        return GetString(section, key, String.Empty);
      }
      set
      {
        if (String.IsNullOrEmpty(section))
          throw new ArgumentNullException("section");
        if (String.IsNullOrEmpty(key))
          throw new ArgumentNullException("key");
        if (value == null)
          value = String.Empty;
        CheckNotReadOnly();
        IniSection sect = _Sections[section];
        if (sect == null)
        {
          sect = new IniSection(section);
          _Sections.Add(sect);
        }

        IniKeyValue v = new IniKeyValue(key, value);
        sect.Remove(key);
        sect.Add(v);
      }
    }

    /// <summary>
    /// Получение строкого значения с указанием значения по умолчанию
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <param name="key">Имя параметра</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <returns>Строковое значение</returns>
    public string GetString(string section, string key, string defaultValue)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");
      if (String.IsNullOrEmpty(key))
        throw new ArgumentNullException("key");

      IniSection sect = _Sections[section];
      if (sect == null)
        return defaultValue;
      IniKeyValue v = sect[key];
      //if (v.Key == null)
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
    /// Возвращает массив имен всех параметров для заданной секции
    /// </summary>
    /// <param name="section"></param>
    /// <returns>Массив имен</returns>
    public string[] GetKeyNames(string section)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");

      IniSection sect = _Sections[section];
      if (sect == null)
        return DataTools.EmptyStrings;
      else
        return sect.GetCodes();
    }

    /// <summary>
    /// Удаление секции и всех значений в ней.
    /// Если секция не существует, никаких действий не выполняется.
    /// </summary>
    /// <param name="section">Имя секции</param>
    public void DeleteSection(string section)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");

      CheckNotReadOnly();
      _Sections.Remove(section);
    }

    /// <summary>
    /// Удаление параметра из секции.
    /// Если секция или значение не существуют, никаких действий не выполняется.
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <param name="key">Имя параметра</param>
    public void DeleteKey(string section, string key)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");
      if (String.IsNullOrEmpty(key))
        throw new ArgumentNullException("key");

      CheckNotReadOnly();
      IniSection sect = _Sections[section];
      if (sect == null)
        return;
      sect.Remove(key);
    }

    /// <summary>
    /// Возвращает объект, для которого можно вызвать foreach по парам "Ключ-Значение"
    /// </summary>
    /// <param name="section">Имя секции</param>
    /// <returns>Объект, реализующий интерфейс <see cref="IEnumerable"/></returns>
    public IEnumerable<IniKeyValue> GetKeyValues(string section)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");
      IniSection sect = _Sections[section];
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
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }
}
