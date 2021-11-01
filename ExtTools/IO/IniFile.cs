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
  /// ���� "����-��������" ��� ���������� �� Ini-�����
  /// </summary>
  public class IniKeyValue : IObjectWithCode
  {
    #region �����������

    /// <summary>
    /// ������� ���� "����-��������"
    /// </summary>
    /// <param name="key">��� ���������</param>
    /// <param name="value">��������</param>
    public IniKeyValue(string key, string value)
    {
      _Key = key;
      _Value = value;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����
    /// </summary>
    public string Key { get { return _Key; } }
    private string _Key;

    /// <summary>
    /// ��������
    /// </summary>
    public string Value { get { return _Value; } }
    private string _Value;

    /// <summary>
    /// ���������� ���� "����=��������"
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
  /// ��������� ������� � INI-������.
  /// ������ ����� ���� ���������� ����� �������� ������-������ ����� (����� IniFile)
  /// ��� �������� Windows API (����� IniFileWindows)
  /// </summary>
  public interface IIniFile : IReadOnlyObject
  {
    #region ������

    /// <summary>
    /// ���������� ������ ���� ���� ������
    /// </summary>
    /// <returns>������ ����</returns>
    string[] GetSectionNames();

    /// <summary>
    /// �������� ������ � ���� �������� � ���.
    /// ���� ������ �� ����������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="section">��� ������</param>
    void DeleteSection(string section);

    #endregion

    #region ��������

    /// <summary>
    /// ������ � ������ ���������� ��������.
    /// ���� ��� ������ ��� ����� ������ ��� �����, ������������ ������ ��������.
    /// ��� ������ ��������������� �������� ����������� �������� ������ ��� �����
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <param name="key">��� ���������</param>
    /// <returns>��������� ��������</returns>
    string this[string section, string key] { get; set; }

    /// <summary>
    /// ��������� �������� �������� � ��������� �������� �� ���������
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <param name="key">��� ���������</param>
    /// <param name="defaultValue">�������� �� ���������</param>
    /// <returns>��������� ��������</returns>
    string GetString(string section, string key, string defaultValue);

    /// <summary>
    /// ���������� ������ ���� ���� ���������� ��� �������� ������
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <returns>������ ����</returns>
    string[] GetKeyNames(string section);

    /// <summary>
    /// ���������� ������, ��� �������� ����� ������� foreach �� ����� "����-��������"
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <returns>������, ����������� ��������� IEnumerable</returns>
    IEnumerable<IniKeyValue> GetKeyValues(string section);

    /// <summary>
    /// �������� ��������� �� ������.
    /// ���� ������ ��� �������� �� ����������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <param name="key">��� ���������</param>
    void DeleteKey(string section, string key);

    #endregion
  }



  /// <summary>
  /// ������ � INI-����� � ������ ������ � ������ ����� ��� ������.
  /// ��������, � ��� ����� � �� ����������, �������� �� Windows.
  /// ��� �������� �������� � ���������� ��������� ����������� ������ Load() � Save().
  /// ��� ������ � INI-������� � ������� ������� Windows, ����������� ����� IniFileWindows.
  /// </summary>
  public class IniFile : IIniFile
  {
    #region ��������� ������

    /// <summary>
    /// ������ ������������
    /// </summary>
    private class IniSection : NamedList<IniKeyValue>, IObjectWithCode
    {
      #region �����������

      public IniSection(string section)
        : base(true)
      {
        _Section = section;
      }

      #endregion

      #region ��������

      public string Section { get { return _Section; } }
      private string _Section;

      string IObjectWithCode.Code { get { return _Section; } }

      #endregion
    }

    /// <summary>
    /// ������ ������ ������������
    /// </summary>
    private class IniSectionList : NamedList<IniSection>
    {
      #region �����������

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

    #region ������������

    /// <summary>
    /// ������� ������ ������ ������.
    /// ������ �������� � ��� ������ � ��� ������.
    /// </summary>
    public IniFile()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������ ������ ������
    /// </summary>
    /// <param name="isReadOnly">���� true, �� ������ ����� �������� ������ ��� ������</param>
    public IniFile(bool isReadOnly)
    {
      _Sections = new IniSectionList();
      _IsReadOnly = isReadOnly; // 28.12.2020
    }

    #endregion

    #region ������ �������� � ����������

    #region ������

    /// <summary>
    /// ��������� ������ �� ��������� �����, ��������� ���������, �������� �� ���������
    /// </summary>
    /// <param name="filePath">���� � �����. ���� ������ ������������</param>
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
    /// ��������� ������ �� ��������� �����
    /// </summary>
    /// <param name="filePath">���� � �����. ���� ������ ������������</param>
    /// <param name="encoding">���������</param>
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
    /// ��������� ������ �� ������
    /// </summary>
    /// <param name="stream">����� ��� ������</param>
    /// <param name="encoding">���������</param>
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

      Regex regexcomment = new Regex("^([\\s]*#.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
      Regex regexsection = new Regex("^[\\s]*\\[[\\s]*([^\\[\\s].*[^\\s\\]])[\\s]*\\][\\s]*$", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
      Regex regexkey = new Regex("^\\s*([^=]*[^\\s=])\\s*=(.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));

      while (!oReader.EndOfStream)
      {
        string line = oReader.ReadLine().Trim();
        if (String.IsNullOrEmpty(line))
          continue;
        Match m;

        m = regexcomment.Match(line);
        if (m.Success)
          continue; // ����������� �� ���������

        m = regexsection.Match(line);
        if (m.Success)
        {
          IniSection Sect = _Sections[m.Groups[1].Value];
          if (Sect == null)
            Sect = new IniSection(m.Groups[1].Value);
          else
            _Sections.Remove(Sect);
          _Sections.Add(Sect);
          continue;
        }

        m = regexkey.Match(line);
        if (m.Success && _Sections.Count > 0)
        {
          IniSection Sect = _Sections[_Sections.Count - 1];
          IniKeyValue v = new IniKeyValue(m.Groups[1].Value, m.Groups[2].Value);
          Sect.Remove(v.Key); // �� ������ ������
          Sect.Add(v);
        }
      }
    }

    #endregion

    #region ������

    /// <summary>
    /// ���������� ������ � ����, ��������� ���������, �������� �� ���������.
    /// ���� ����� ������ �������� ��� IsReadOnly=true.
    /// </summary>
    /// <param name="filePath">��� �����</param>
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
    /// ���������� ������ � ���� � ��������� ���������.
    /// ���� ����� ������ �������� ��� IsReadOnly=true.
    /// </summary>
    /// <param name="filePath">��� �����</param>
    /// <param name="encoding">���������</param>
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
    /// ���������� ������ � �����
    /// </summary>
    /// <param name="stream">�����</param>
    /// <param name="encoding">���������</param>
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
      foreach (IniSection Sect in _Sections)
      {
        oWriter.WriteLine(String.Format("[{0}]", Sect.Section));
        foreach (IniKeyValue v in Sect)
        {
          oWriter.WriteLine(String.Format("{0}={1}", v.Key, v.Value));
        }
      }
    }

    private AbsPath _FilePath;

    /// <summary>
    /// ���������� ��� �����, ��� �������� ��� ������ Load() ��� Save() ��� "no file"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (_FilePath.IsEmpty)
        return "no file";
      else
        return _FilePath.Path;
    }

    #endregion

    #endregion

    #region ������ ������

    private IniSectionList _Sections;

    #endregion

    #region IIniFile Members

    /// <summary>
    /// ������ � ������ ���������� ��������.
    /// ���� ��� ������ ��� ����� ������ ��� �����, ������������ ������ ��������.
    /// ��� ������ ��������������� �������� ����������� �������� ������ ��� �����
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <param name="key">��� ���������</param>
    /// <returns>��������� ��������</returns>
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
        IniSection Sect = _Sections[section];
        if (Sect == null)
        {
          Sect = new IniSection(section);
          _Sections.Add(Sect);
        }

        IniKeyValue v = new IniKeyValue(key, value);
        Sect.Remove(key);
        Sect.Add(v);
      }
    }

    /// <summary>
    /// ��������� �������� �������� � ��������� �������� �� ���������
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <param name="key">��� ���������</param>
    /// <param name="defaultValue">�������� �� ���������</param>
    /// <returns>��������� ��������</returns>
    public string GetString(string section, string key, string defaultValue)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");
      if (String.IsNullOrEmpty(key))
        throw new ArgumentNullException("key");

      IniSection Sect = _Sections[section];
      if (Sect == null)
        return defaultValue;
      IniKeyValue v = Sect[key];
      //if (v.Key == null)
      if (v == null) // ���������� 08.11.2019
        return defaultValue;
      else
        return v.Value;
    }

    /// <summary>
    /// ���������� ������ ���� ���� ������
    /// </summary>
    /// <returns>������ ����</returns>
    public string[] GetSectionNames()
    {
      return _Sections.GetCodes();
    }

    /// <summary>
    /// ���������� ������ ���� ���� ���������� ��� �������� ������
    /// </summary>
    /// <param name="section"></param>
    /// <returns>������ ����</returns>
    public string[] GetKeyNames(string section)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");

      IniSection Sect = _Sections[section];
      if (Sect == null)
        return DataTools.EmptyStrings;
      else
        return Sect.GetCodes();
    }

    /// <summary>
    /// �������� ������ � ���� �������� � ���.
    /// ���� ������ �� ����������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="section">��� ������</param>
    public void DeleteSection(string section)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");

      CheckNotReadOnly();
      _Sections.Remove(section);
    }

    /// <summary>
    /// �������� ��������� �� ������.
    /// ���� ������ ��� �������� �� ����������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <param name="key">��� ���������</param>
    public void DeleteKey(string section, string key)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");
      if (String.IsNullOrEmpty(key))
        throw new ArgumentNullException("key");

      CheckNotReadOnly();
      IniSection Sect = _Sections[section];
      if (Sect == null)
        return;
      Sect.Remove(key);
    }

    /// <summary>
    /// ���������� ������, ��� �������� ����� ������� foreach �� ����� "����-��������"
    /// </summary>
    /// <param name="section">��� ������</param>
    /// <returns>������, ����������� ��������� IEnumerable</returns>
    public IEnumerable<IniKeyValue> GetKeyValues(string section)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentNullException("section");
      IniSection Sect = _Sections[section];
      if (Sect == null)
        return new DummyEnumerable<IniKeyValue>();
      else
        return Sect;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ��������� ������ ������, �� �� ������.
    /// �������� � ������������
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }
}