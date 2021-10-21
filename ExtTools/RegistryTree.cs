using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
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

namespace FreeLibSet.Win32
{
  /// <summary>
  /// Этот объект используется при переборе реестра методом RegistryTree.Enumerate()
  /// </summary>
  public sealed class EnumRegistryEntry
  {
    #region Конструктор

    internal EnumRegistryEntry(RegistryKey key, int enumKeyLevel, string valueName)
    {
      _Key = key;
      _EnumKeyLevel = enumKeyLevel;
      _ValueName = valueName;
      //System.Diagnostics.Trace.WriteLine("Init: " + this.ToString());
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущий раздел реестра
    /// </summary>
    public RegistryKey Key { get { return _Key; } }
    private RegistryKey _Key;

    /// <summary>
    /// Уровень раздела Key относительно раздела, с которого начато перечисление.
    /// Если сейчас перечисляется стартновый раздел, свойство возвращает 0, если один из его
    /// дочерних разделов, то 1, и т.д.
    /// </summary>
    public int EnumKeyLevel { get { return _EnumKeyLevel; } }
    private int _EnumKeyLevel;

    /// <summary>
    /// Имя текущего значения.
    /// Сначала перечислитель вызываетсz для раздела, при этом свойство возвращает true.
    /// Затем, если перечисление значений включено, это свойство возвращает имя очередного значения.
    /// </summary>
    public string ValueName { get { return _ValueName; } }
    private string _ValueName;

    /// <summary>
    /// Возвращает "Key.Name" или "Key.Name : Value", если сейчас перебирается значение
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      //string s = Key.Name;
      if (String.IsNullOrEmpty(ValueName))
        return Key.Name;
      else
        return Key.Name + " : " + ValueName;
    }

    #endregion
  }

  /// <summary>
  /// Хранение коллекции объектов RegistryKey.
  /// В пользовательском коде используйте класс RegistryCfg для доступа к реестру.
  /// </summary>
  public class RegistryTree : DisposableObject, IReadOnlyObject
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает пустой список.
    /// Секции будут доступны для записи.
    /// </summary>
    public RegistryTree()
      : this(false)
    {
    }

    /// <summary>
    /// Создает пустой список.
    /// </summary>
    /// <param name="isReadOnly">true, если данные будут доступны только для чтения</param>
    public RegistryTree(bool isReadOnly)
    {
      _Items = new Dictionary<string, RegistryKey>();
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// Закрывает все открытые секции
    /// </summary>
    /// <param name="disposing">Вызов из метода Dispose()?</param>
    protected override void Dispose(bool disposing)
    {
      if (_Items != null)
      {
        Close();
        _Items = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Доступ к объектам RegistryKey

    /// <summary>
    /// Возвращает открытый объект RegistryKey для доступа к ветви реестра.
    /// Также рекурсивно отрываются все родительскте ветви реестра.
    /// Объекты буферизуются во внутреннем списке и закрываются при вызове Close() или Dispose().
    /// Возвращает null, если ветвь реестра не существует.
    /// </summary>
    /// <param name="keyName">Путь к ветви реестра</param>
    /// <returns>Объект доступа к узлу реестра или null</returns>
    public RegistryKey this[string keyName]
    {
      get
      {
        CheckNotDisposed();

        RegistryKey Item;
        if (_Items.TryGetValue(keyName, out Item))
          return Item; // узел уже был найден

        if (String.IsNullOrEmpty(keyName))
          throw new ArgumentNullException("keyName");

        int p = keyName.LastIndexOf('\\');
        if (p < 0)
        {
          RegistryKey Root = GetRootKey(keyName);
          if (Root == null)
            throw new ArgumentException("Неизвестный корневной узел реестра \"" + keyName + "\"", "keyName");
          return Root;
        }

        // Запрошен составной узел
        string ParentKeyName = keyName.Substring(0, p);
        RegistryKey ParentKey = this[ParentKeyName]; // рекурсивный вызов
        if (ParentKey == null)
          return null;

        string SubName = keyName.Substring(p + 1);
        Item = ParentKey.OpenSubKey(SubName, !IsReadOnly);

        if (Item == null && (!IsReadOnly))
          Item = ParentKey.CreateSubKey(SubName);

        // Добавляем узел в коллекцию
        _Items.Add(keyName, Item);
        return Item;
      }
    }

    /// <summary>
    /// Возвращает корневой узел по имени.
    /// </summary>
    /// <param name="keyName">Имя, например, "HKEY_CLASSES_ROOT"</param>
    /// <returns>Статическое свойство из класса Registry</returns>
    public static RegistryKey GetRootKey(string keyName)
    {
      // Возврашаем корневой узел
      switch (keyName)
      {
        case "HKEY_CLASSES_ROOT": return Registry.ClassesRoot;
        case "HKEY_CURRENT_USER": return Registry.CurrentUser;
        case "HKEY_LOCAL_MACHINE": return Registry.LocalMachine;
        case "HKEY_USERS": return Registry.Users;
        case "HKEY_CURRENT_CONFIG": return Registry.CurrentConfig;
        default: return null;
      }
    }

    /// <summary>
    /// Список открытых узлов реестра. Корневые узлы не хранятся.
    /// Могут быть значения null для несуществующих узлов в режиме IsReadOnly
    /// </summary>
    private Dictionary<string, RegistryKey> _Items;

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Режим "только для чтения". Задается в конструкторе.
    /// Когда выполняется обращение к несуществующему узлу реестра, при IsReadOnly=true возвращается null,
    /// а при false - создается новый узел
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private readonly bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new InvalidOperationException("Коллекция узлов реестра открыта только для чтения");
    }

    /// <summary>
    /// Закрывает все открытые секции, вызывая RegistryKey.Close()
    /// </summary>
    public void Close()
    {
      if (IsDisposed)
        return;

      foreach (RegistryKey Item in _Items.Values)
      {
        try
        {
          if (Item != null)
            Item.Close();
        }
        catch { }
      }

      _Items.Clear();
    }

    /// <summary>
    /// Возвращает true, если существует указанный путь к ветви реестра
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <returns>Существование ветви</returns>
    public bool Exists(string keyName)
    {
      CheckNotDisposed();

      if (String.IsNullOrEmpty(keyName))
        return false;

      if (_Items.ContainsKey(keyName))
        return true;

      int p = keyName.LastIndexOf('\\');
      if (p < 0)
        return GetRootKey(keyName) != null;

      string ParentKeyName = keyName.Substring(0, p);

      if (!Exists(ParentKeyName)) // рекурсивный вызов
        return false;

      // Нужен родительский узел
      RegistryKey ParentKey = this[ParentKeyName];
      if (ParentKey == null)
        return false; // 21.02.2020
      string SubName = keyName.Substring(p + 1);

      RegistryKey SubKey = ParentKey.OpenSubKey(SubName, !IsReadOnly);
      if (SubKey == null)
        return false;

      _Items.Add(keyName, SubKey);
      return true;
    }

    #endregion

    #region Методы чтения значений

    /// <summary>
    /// Получить значение.
    /// Если узла нет, возвращается null.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public object GetValue(string keyName, string valueName)
    {
      RegistryKey Key = this[keyName];
      if (Key == null)
        return null;
      else
        return Key.GetValue(valueName);
    }

    /// <summary>
    /// Получить строковое значение.
    /// Если узла нет, возвращается пустая строка.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public string GetString(string keyName, string valueName)
    {
      return DataTools.GetString(GetValue(keyName, valueName));
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если узла нет или значение является пустой строкой, возвращается 0.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public int GetInt(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0;
      return StdConvert.ToInt32(s);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если узла нет или значение является пустой строкой, возвращается 0.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public long GetInt64(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0;
      return StdConvert.ToInt64(s);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если узла нет или значение является пустой строкой, возвращается 0.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public float GetSingle(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0f;
      return StdConvert.ToSingle(s);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если узла нет или значение является пустой строкой, возвращается 0.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public double GetDouble(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0.0;
      return StdConvert.ToDouble(s);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если узла нет или значение является пустой строкой, возвращается 0.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0m;
      return StdConvert.ToDecimal(s);
    }

    /// <summary>
    /// Получить логическое значение.
    /// Если узла нет или значение является пустой строкой, возвращается false.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public bool GetBool(string keyName, string valueName)
    {
      return GetInt(keyName, valueName) != 0;
    }

    #endregion

    #region Перечислитель

    /// <summary>
    /// Один уровень при перечислении ключей.
    /// Нужно хранить в этом классе все имена как дочерних разделов, так и имен.
    /// Также нужно хранить счетчики, так как локальные переменные в Enumerate() действуют неверно
    /// </summary>
    private class EnumInfo
    {
      #region Конструктор

      public EnumInfo(RegistryKey currKey, bool enumerateValues)
      {
#if DEBUG
        if (currKey == null)
          throw new ArgumentNullException("currKey");
#endif
        _CurrKey = currKey;

        _SubKeyNames = currKey.GetSubKeyNames();
        SubKeyIndex = -1;

        if (enumerateValues)
          _ValueNames = currKey.GetValueNames();
        ValueIndex = -1;

        // System.Diagnostics.Trace.WriteLine("EnumInfo: " + CurrKey.Name + ", SubKeyNames: " + String.Join(", ", SubKeyNames) +
        //   ", ValueNames: " + String.Join(", ", ValueNames));
      }

      #endregion

      #region Поля

      /// <summary>
      /// Раздел реестра, который сейчас перебирается
      /// </summary>
      public RegistryKey CurrKey { get { return _CurrKey; } }
      private RegistryKey _CurrKey;

      /// <summary>
      /// Список имен дочерних реестров
      /// </summary>
      public string[] SubKeyNames { get { return _SubKeyNames; } }
      private string[] _SubKeyNames;

      /// <summary>
      /// Индекс текущего дочернего узла
      /// </summary>
      public int SubKeyIndex;

      public string[] ValueNames { get { return _ValueNames; } }
      private string[] _ValueNames;

      public int ValueIndex;

      public override string ToString()
      {
        return CurrKey.Name;
      }

      #endregion
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="keyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект EnumRegistryEntry.
    /// При перечислении системных разделов реестра Windows у пользователя может не быть прав на запись.
    /// Используйте RegistryTree в режиме IsReadOnly=true.
    /// Для однократного перечисления может быть выгоднее использовать статический метод StaticEnumerate(),
    /// который не требует явного создания и удаления объекта RegistryTree.
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public IEnumerable<EnumRegistryEntry> Enumerate(string keyName)
    {
      return Enumerate(keyName, true);
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем, если <paramref name="enumerateValues"/>=true, перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="keyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект EnumRegistryEntry.
    /// При перечислении системных разделов реестра Windows у пользователя может не быть прав на запись.
    /// Используйте RegistryTree в режиме IsReadOnly=true.
    /// Для однократного перечисления может быть выгоднее использовать статический метод StaticEnumerate(),
    /// который не требует явного создания и удаления объекта RegistryTree.
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <param name="enumerateValues">Нужно ли перечислять также все значения в разделах (true),
    /// или перечислять только разделы (false)</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public IEnumerable<EnumRegistryEntry> Enumerate(string keyName, bool enumerateValues)
    {
      RegistryKey StartKey = this[keyName];
      if (StartKey == null)
        yield break;

      Stack<EnumInfo> Stack = new Stack<EnumInfo>();
      Stack.Push(new EnumInfo(StartKey, enumerateValues));
      while (Stack.Count > 0)
      {
        EnumInfo CurrInfo = Stack.Peek();
        if (CurrInfo.SubKeyIndex < 0)
        {
          // первый такт цикла

          // Для ключа реестра
          yield return new EnumRegistryEntry(CurrInfo.CurrKey, Stack.Count - 1, String.Empty);

          // Для значений
          if (enumerateValues)
          {
            while (true)
            {
              CurrInfo.ValueIndex++;
              if (CurrInfo.ValueIndex >= CurrInfo.ValueNames.Length)
                break;
              if (String.IsNullOrEmpty(CurrInfo.ValueNames[CurrInfo.ValueIndex]))
                continue; // значение по умолчанию не перечисляем
              yield return new EnumRegistryEntry(CurrInfo.CurrKey, Stack.Count - 1, CurrInfo.ValueNames[CurrInfo.ValueIndex]);

            }
          }
        }

        CurrInfo.SubKeyIndex++;
        if (CurrInfo.SubKeyIndex < CurrInfo.SubKeyNames.Length)
        {
          string nm = CurrInfo.SubKeyNames[CurrInfo.SubKeyIndex];
          string ChildKeyName = CurrInfo.CurrKey.Name + "\\" + nm;
          RegistryKey ChildKey = this[ChildKeyName];
          if (ChildKey != null) // вдруг нет прав доступа?
            Stack.Push(new EnumInfo(ChildKey, enumerateValues));
          continue;
        }

        // больше нет дочерних разделов
        Stack.Pop();
      }
    }

    #endregion

    #region Статические версии перечислителя

    private class EnumeratorProxy : DisposableObject, IEnumerator<EnumRegistryEntry>
    {
      #region Конструктор и Dispose

      public EnumeratorProxy(string keyName, bool enumerateValues)
      {
        _Tree = new RegistryTree(true);
        _En2 = _Tree.Enumerate(keyName, enumerateValues).GetEnumerator();
      }

      private RegistryTree _Tree;
      private IEnumerator<EnumRegistryEntry> _En2;

      protected override void Dispose(bool disposing)
      {
        if (disposing && _Tree != null)
        {
          _Tree.Dispose();
          _En2.Dispose();
        }
        _Tree = null;
        _En2 = null;

        base.Dispose(disposing);
      }

      #endregion

      #region IEnumerator<EnumRegistryEntry> Members

      public EnumRegistryEntry Current { get { return _En2.Current; } }

      #endregion

      #region IEnumerator Members

      object System.Collections.IEnumerator.Current { get { return _En2.Current; } }

      public bool MoveNext()
      {
#if DEBUG
        if (_En2.MoveNext())
        {
          if (Current == null)
            throw new BugException("Текущий элемент равен null");
          return true;
        }
        else
          return false;
#else
        return _En2.MoveNext();
#endif
      }

      public void Reset()
      {
        _En2.Reset();
      }

      #endregion
    }

    // Бесполезно возвращать GetEnumerator(), с ним foreach не работает
#if XXX
    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="KeyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект EnumRegistryEntry.
    /// Статическая версия создает временный объект RegistryTree.
    /// </summary>
    /// <param name="KeyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public static IEnumerator<EnumRegistryEntry> GetEnumerator(string KeyName)
    {
      return GetEnumerator(KeyName, true);
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем, если <paramref name="EnumerateValues"/>=true, перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="KeyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект EnumRegistryEntry.
    /// Статическая версия создает временный объект RegistryTree.
    /// </summary>
    /// <param name="KeyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <param name="EnumerateValues">Нужно ли перечислять также все значения в разделах (true),
    /// или перечислять только разделы (false)</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public static IEnumerator<EnumRegistryEntry> GetEnumerator(string KeyName, bool EnumerateValues)
    {
      return new EnumeratorProxy(KeyName, EnumerateValues);
    }

#endif


    private class EnumarableProxy : IEnumerable<EnumRegistryEntry>
    {
      #region Поля

      public string KeyName;
      public bool EnumerateValues;

      #endregion

      #region IEnumerable<EnumRegistryEntry> Members

      public IEnumerator<EnumRegistryEntry> GetEnumerator()
      {
        return new EnumeratorProxy(KeyName, EnumerateValues);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new EnumeratorProxy(KeyName, EnumerateValues);
      }

      #endregion
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="keyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект EnumRegistryEntry.
    /// Для перечисления создается временныый объект RegistryTree. Если требуется многократное перечисление
    /// реестра, то следует использовать нестатическую версию метода Enumerate().
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public static IEnumerable<EnumRegistryEntry> StaticEnumerate(string keyName)
    {
      return StaticEnumerate(keyName, true);
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем, если <paramref name="enumerateValues"/>=true, перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="keyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект EnumRegistryEntry.
    /// Для перечисления создается временныый объект RegistryTree. Если требуется многократное перечисление
    /// реестра, то следует использовать нестатическую версию метода Enumerate().
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <param name="enumerateValues">Нужно ли перечислять также все значения в разделах (true),
    /// или перечислять только разделы (false)</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public static IEnumerable<EnumRegistryEntry> StaticEnumerate(string keyName, bool enumerateValues)
    {
      EnumarableProxy Proxy = new EnumarableProxy();
      Proxy.KeyName = keyName;
      Proxy.EnumerateValues = enumerateValues;
      return Proxy;
    }

    #endregion
  }
}
