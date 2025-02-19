﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Win32
{

  /// <summary>
  /// Этот объект используется при переборе реестра методом <see cref="RegistryTree2.Enumerate(string)"/>
  /// </summary>
  public sealed class EnumRegistryEntry2
  {
    #region Конструктор

    internal EnumRegistryEntry2(RegistryKey2 key, int enumKeyLevel, string valueName)
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
    public RegistryKey2 Key { get { return _Key; } }
    private readonly RegistryKey2 _Key;

    /// <summary>
    /// Уровень раздела <see cref="Key"/> относительно раздела, с которого начато перечисление.
    /// Если сейчас перечисляется стартовый раздел, свойство возвращает 0, если один из его
    /// дочерних разделов, то 1, и т.д.
    /// </summary>
    public int EnumKeyLevel { get { return _EnumKeyLevel; } }
    private readonly int _EnumKeyLevel;

    /// <summary>
    /// Имя текущего значения.
    /// Сначала перечислитель вызывается для раздела, при этом свойство возвращает пустое значение.
    /// Затем, если перечисление значений включено, это свойство возвращает имя очередного значения.
    /// </summary>
    public string ValueName { get { return _ValueName; } }
    private readonly string _ValueName;

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
  /// Хранение коллекции объектов <see cref="RegistryKey2"/>.
  /// В отличие от <see cref="RegistryTree"/>, который использует стандартные объекты NetFramework (<see cref="Microsoft.Win32.RegistryKey"/>),
  /// эта коллекция позволяет получить доступ к реестру с правильной "визуализацией",
  /// когда 32-битное приложение работает в Windows-64.
  /// </summary>
  public class RegistryTree2 : DisposableObject, IReadOnlyObject
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает пустой список.
    /// Секции будут доступны для записи.
    /// Будет использован режим виртуализации <see cref="View"/>=Registry64 или Registry32 в зависимости от
    /// разрядности операционной системы, а не от разрядности приложения.
    /// </summary>
    public RegistryTree2()
      : this(false, GetDefaultView())
    {
    }

    /// <summary>
    /// Создает пустой список.
    /// Будет использован режим виртуализации <see cref="View"/>=Registry64 или Registry32 в зависимости от
    /// разрядности операционной системы, а не от разрядности приложения.
    /// </summary>
    /// <param name="isReadOnly">true, если данные будут доступны только для чтения</param>
    public RegistryTree2(bool isReadOnly)
      :this(isReadOnly, GetDefaultView())
    {
    }

    internal static RegistryView2 GetDefaultView()
    {
      if (EnvironmentTools.Is64BitOperatingSystem)
        return RegistryView2.Registry64;
      else
        return RegistryView2.Registry32;
    }

    /// <summary>
    /// Создает пустой список.
    /// </summary>
    /// <param name="isReadOnly">true, если данные будут доступны только для чтения</param>
    /// <param name="view">Режим виртуализации</param>
    public RegistryTree2(bool isReadOnly, RegistryView2 view)
    {
      if (!RegistryKey2.IsSupported)
        throw new PlatformNotSupportedException();

      _Items = new Dictionary<string, RegistryKey2>();
      _IsReadOnly = isReadOnly;
      _View = view;
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

    #region Доступ к объектам RegistryKey2

    /// <summary>
    /// Возвращает открытый объект <see cref="RegistryKey2"/> для доступа к ветви реестра.
    /// Также рекурсивно открываются все родительские ветви реестра.
    /// Объекты буферизуются во внутреннем списке и закрываются при вызове Close() или Dispose().
    /// Возвращает null, если ветвь реестра не существует.
    /// </summary>
    /// <param name="keyName">Путь к ветви реестра</param>
    /// <returns>Объект доступа к узлу реестра или null</returns>
    public RegistryKey2 this[string keyName]
    {
      get
      {
        CheckNotDisposed();

        RegistryKey2 item;
        if (_Items.TryGetValue(keyName, out item))
          return item; // узел уже был найден

        if (String.IsNullOrEmpty(keyName))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("keyName");

        int p = keyName.LastIndexOf('\\');
        if (p < 0)
        {
          RegistryKey2 root = GetRootKey(keyName);
          if (root == null)
            throw new ArgumentException(String.Format(Res.RegistryTree_Arg_UnknownRoot, keyName));
          return root;
        }

        // Запрошен составной узел
        string parentKeyName = keyName.Substring(0, p);
        RegistryKey2 parentKey = this[parentKeyName]; // рекурсивный вызов
        if (parentKey == null)
          return null;

        string subName = keyName.Substring(p + 1);
        item = parentKey.OpenSubKey(subName, !IsReadOnly);

        if (item == null && (!IsReadOnly))
          item = parentKey.CreateSubKey(subName);

        // Добавляем узел в коллекцию
        _Items.Add(keyName, item);
        return item;
      }
    }

    /// <summary>
    /// Возвращает корневой узел по имени.
    /// </summary>
    /// <param name="keyName">Имя, например, "HKEY_CLASSES_ROOT"</param>
    /// <returns>Статическое свойство из класса <see cref="Microsoft.Win32.Registry"/></returns>
    public RegistryKey2 GetRootKey(string keyName)
    {
      // Возврашаем корневой узел
      switch (keyName)
      {
        case "HKEY_CLASSES_ROOT": return RegistryKey2.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, View);
        case "HKEY_CURRENT_USER": return RegistryKey2.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, View);
        case "HKEY_LOCAL_MACHINE": return RegistryKey2.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, View);
        case "HKEY_USERS": return RegistryKey2.OpenBaseKey(Microsoft.Win32.RegistryHive.Users, View);
        case "HKEY_CURRENT_CONFIG": return RegistryKey2.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentConfig, View);
        default: return null;
      }
    }

    /// <summary>
    /// Список открытых узлов реестра. Корневые узлы не хранятся.
    /// Могут быть значения null для несуществующих узлов в режиме <see cref="IsReadOnly"/>=true.
    /// </summary>
    private Dictionary<string, RegistryKey2> _Items;

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Режим "только для чтения". Задается в конструкторе.
    /// Когда выполняется обращение к несуществующему узлу реестра, при <see cref="IsReadOnly"/>=true возвращается null,
    /// а при false - создается новый узел.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private readonly bool _IsReadOnly;

    /// <summary>
    /// Режим виртуализации для Windows 64.
    /// Задается в конструкторе
    /// </summary>
    public RegistryView2 View { get { return _View; } }
    private readonly RegistryView2 _View;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new InvalidOperationException(Res.RegistryTree_Err_IsReadOnly);
    }

    /// <summary>
    /// Закрывает все открытые секции, вызывая <see cref="RegistryKey2"/>.Dispose().
    /// </summary>
    public void Close()
    {
      if (IsDisposed)
        return;

      foreach (RegistryKey2 item in _Items.Values)
      {
        try
        {
          if (item != null)
            item.Dispose();
        }
        catch { }
      }

      _Items.Clear();
    }

    /// <summary>
    /// Закрывает открытые разделы реестра, начиная с указанного раздела (включительно), и все вложенные разделы.
    /// Если разделы не были открыты, никаких действий не выполняется
    /// </summary>
    /// <param name="keyName">Закрываемый раздел реестра</param>
    public void Close(string keyName)
    {
      DoClose(keyName, true);
    }

    /// <summary>
    /// Закрывает открытые разделы реестра, расположенные внутри, и все вложенные разделы.
    /// Сам раздел <paramref name="keyName"/>, если он был открыт, не закрывается.
    /// Если разделы не были открыты, никаких действий не выполняется
    /// </summary>
    /// <param name="keyName">Родительский раздел</param>
    public void CloseChildren(string keyName)
    {
      DoClose(keyName, false);
    }

    private void DoClose(string keyName, bool includeMain)
    {
      if (String.IsNullOrEmpty(keyName))
      {
        Close();
        return;
      }

      List<KeyValuePair<string, RegistryKey2>> lst = null;
      string keyName2 = keyName + "\\";
      foreach (KeyValuePair<string, RegistryKey2> pair in _Items)
      {
        if (includeMain &&
          String.Equals(pair.Key, keyName, StringComparison.OrdinalIgnoreCase) &&
          keyName.IndexOf("\\") >= 0) // корневой раздел не закрываем
        {
          if (lst == null) lst = new List<KeyValuePair<string, RegistryKey2>>();
          lst.Add(pair);
        }

        if (pair.Key.StartsWith(keyName2, StringComparison.OrdinalIgnoreCase))
        {
          if (lst == null) lst = new List<KeyValuePair<string, RegistryKey2>>();
          lst.Add(pair);
        }
      }

      if (lst == null)
        return;

      foreach (KeyValuePair<string, RegistryKey2> pair in lst)
      {
        if (pair.Value != null)
        {
          try { pair.Value.Dispose(); }
          catch { }
        }

        _Items.Remove(pair.Key);
      }
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
      RegistryKey2 parentKey = this[ParentKeyName];
      if (parentKey == null)
        return false; // 21.02.2020
      string subName = keyName.Substring(p + 1);

      RegistryKey2 subKey = parentKey.OpenSubKey(subName, !IsReadOnly);
      if (subKey == null)
        return false;

      _Items.Add(keyName, subKey);
      return true;
    }

    /// <summary>
    /// Удаляет раздел, задаваемый путем <paramref name="keyName"/> и, рекурсивно, все дочерние разделы.
    /// Для удаления используется метод <see cref="RegistryKey2.DeleteSubKeyTree(string)"/>.
    /// Применяйте метод с осторожностью!
    /// </summary>
    /// <param name="keyName">Путь к удаляемому реестру</param>
    public void DeleteTree(string keyName)
    {
      CheckNotDisposed();
      CheckNotReadOnly();

      if (!Exists(keyName))
        return;

      int p = keyName.LastIndexOf('\\');
      if (p < 0)
        throw new InvalidOperationException(Res.RegistryKey2_Err_Root);

      string parentKeyName = keyName.Substring(0, p);
      RegistryKey2 parentKey = this[parentKeyName];
      string subName = keyName.Substring(p + 1);
      Close(keyName);
      parentKey.DeleteSubKeyTree(subName);
    }

    #endregion

    #region Методы чтения значений

    /// <summary>
    /// Получить значение.
    /// Если узла нет, возвращается null.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
    /// <returns>Значение</returns>
    public object GetValue(string keyName, string valueName)
    {
      RegistryKey2 key = this[keyName];
      if (key == null)
        return null;
      else
        return key.GetValue(valueName, null);
    }

    /// <summary>
    /// Получить строковое значение.
    /// Если узла нет, возвращается пустая строка.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
    /// <returns>Значение</returns>
    public string GetString(string keyName, string valueName)
    {
      return DataTools.GetString(GetValue(keyName, valueName));
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если узла нет или значение является пустой строкой, возвращается 0.
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
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
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
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
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
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
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
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
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
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
    /// Если хранимое значение нельзя преобразовать в число, генерируется исключение.
    /// </summary>
    /// <param name="keyName">Путь к узлу реестра</param>
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую строку</param>
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

      public EnumInfo(RegistryKey2 currKey, bool enumerateValues)
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
      public RegistryKey2 CurrKey { get { return _CurrKey; } }
      private RegistryKey2 _CurrKey;

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
    /// При перечислении используется объект <see cref="EnumRegistryEntry"/>.
    /// При перечислении системных разделов реестра Windows у пользователя может не быть прав на запись.
    /// Используйте <see cref="RegistryTree2"/> в режиме <see cref="IsReadOnly"/>=true.
    /// Для однократного перечисления может быть выгоднее использовать статический метод <see cref="StaticEnumerate(string)"/>,
    /// который не требует явного создания и удаления объекта <see cref="RegistryTree2"/>.
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public IEnumerable<EnumRegistryEntry2> Enumerate(string keyName)
    {
      return Enumerate(keyName, true);
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем, если <paramref name="enumerateValues"/>=true, перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="keyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект <see cref="EnumRegistryEntry"/>.
    /// При перечислении системных разделов реестра Windows у пользователя может не быть прав на запись.
    /// Используйте <see cref="RegistryTree2"/> в режиме <see cref="IsReadOnly"/>=true.
    /// Для однократного перечисления может быть выгоднее использовать статический метод <see cref="StaticEnumerate(string)"/>,
    /// который не требует явного создания и удаления объекта <see cref="RegistryTree2"/>.
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <param name="enumerateValues">Нужно ли перечислять также все значения в разделах (true),
    /// или перечислять только разделы (false)</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public IEnumerable<EnumRegistryEntry2> Enumerate(string keyName, bool enumerateValues)
    {
      RegistryKey2 startKey = this[keyName];
      if (startKey == null)
        yield break;

      Stack<EnumInfo> stack = new Stack<EnumInfo>();
      stack.Push(new EnumInfo(startKey, enumerateValues));
      while (stack.Count > 0)
      {
        EnumInfo currInfo = stack.Peek();
        if (currInfo.SubKeyIndex < 0)
        {
          // первый такт цикла

          // Для ключа реестра
          yield return new EnumRegistryEntry2(currInfo.CurrKey, stack.Count - 1, String.Empty);

          // Для значений
          if (enumerateValues)
          {
            while (true)
            {
              currInfo.ValueIndex++;
              if (currInfo.ValueIndex >= currInfo.ValueNames.Length)
                break;
              if (String.IsNullOrEmpty(currInfo.ValueNames[currInfo.ValueIndex]))
                continue; // значение по умолчанию не перечисляем
              yield return new EnumRegistryEntry2(currInfo.CurrKey, stack.Count - 1, currInfo.ValueNames[currInfo.ValueIndex]);

            }
          }
        }

        currInfo.SubKeyIndex++;
        if (currInfo.SubKeyIndex < currInfo.SubKeyNames.Length)
        {
          string nm = currInfo.SubKeyNames[currInfo.SubKeyIndex];
          string ChildKeyName = currInfo.CurrKey.Name + "\\" + nm;
          RegistryKey2 ChildKey = this[ChildKeyName];
          if (ChildKey != null) // вдруг нет прав доступа?
            stack.Push(new EnumInfo(ChildKey, enumerateValues));
          continue;
        }

        // больше нет дочерних разделов
        stack.Pop();
      }
    }

    #endregion

    #region Статические версии перечислителя

    private class EnumeratorProxy : DisposableObject, IEnumerator<EnumRegistryEntry2>
    {
      #region Конструктор и Dispose

      public EnumeratorProxy(string keyName, bool enumerateValues)
      {
        _Tree = new RegistryTree2(true);
        _En2 = _Tree.Enumerate(keyName, enumerateValues).GetEnumerator();
      }

      private RegistryTree2 _Tree;
      private IEnumerator<EnumRegistryEntry2> _En2;

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

      public EnumRegistryEntry2 Current { get { return _En2.Current; } }

      #endregion

      #region IEnumerator Members

      object System.Collections.IEnumerator.Current { get { return _En2.Current; } }

      public bool MoveNext()
      {
#if DEBUG
        if (_En2.MoveNext())
        {
          if (Current == null)
            throw new BugException("Current item is null");
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

    private class EnumarableProxy : IEnumerable<EnumRegistryEntry2>
    {
      #region Поля

      public string KeyName;
      public bool EnumerateValues;

      #endregion

      #region IEnumerable<EnumRegistryEntry> Members

      public IEnumerator<EnumRegistryEntry2> GetEnumerator()
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
    /// При перечислении используется объект <see cref="EnumRegistryEntry"/>.
    /// Для перечисления создается временныый объект <see cref="RegistryTree2"/>. Если требуется многократное перечисление
    /// реестра, то следует использовать нестатическую версию метода <see cref="Enumerate(string)"/>.
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public static IEnumerable<EnumRegistryEntry2> StaticEnumerate(string keyName)
    {
      return StaticEnumerate(keyName, true);
    }

    /// <summary>
    /// Рекурсивное перечисление по реестру.
    /// Перечисление начинается с заданного пути реестра, затем, если <paramref name="enumerateValues"/>=true, перечисляются все значения
    /// (кроме "безымянного" значения по умолчанию), затем рекурсивно перечисляются дочерние разделы.
    /// Если раздела <paramref name="keyName"/> нет в реестре, перечислитель ни разу не вызывается.
    /// При перечислении используется объект <see cref="EnumRegistryEntry"/>.
    /// Для перечисления создается временныый объект <see cref="RegistryTree2"/>. Если требуется многократное перечисление
    /// реестра, то следует использовать нестатическую версию метода <see cref="Enumerate(string, bool)"/>.
    /// </summary>
    /// <param name="keyName">Путь к разделу реестра, который нужно перечислить</param>
    /// <param name="enumerateValues">Нужно ли перечислять также все значения в разделах (true),
    /// или перечислять только разделы (false)</param>
    /// <returns>Объект для использования в цикле foreach</returns>
    public static IEnumerable<EnumRegistryEntry2> StaticEnumerate(string keyName, bool enumerateValues)
    {
      EnumarableProxy proxy = new EnumarableProxy();
      proxy.KeyName = keyName;
      proxy.EnumerateValues = enumerateValues;
      return proxy;
    }

    #endregion
  }
}
