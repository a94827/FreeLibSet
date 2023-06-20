// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using FreeLibSet.Core;

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
    /// Возвращает открытый объект <see cref="RegistryKey"/> для доступа к ветви реестра.
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

        RegistryKey item;
        if (_Items.TryGetValue(keyName, out item))
          return item; // узел уже был найден

        if (String.IsNullOrEmpty(keyName))
          throw new ArgumentNullException("keyName");

        int p = keyName.LastIndexOf('\\');
        if (p < 0)
        {
          RegistryKey root = GetRootKey(keyName);
          if (root == null)
            throw new ArgumentException("Неизвестный корневной узел реестра \"" + keyName + "\"", "keyName");
          return root;
        }

        // Запрошен составной узел
        string parentKeyName = keyName.Substring(0, p);
        RegistryKey parentKey = this[parentKeyName]; // рекурсивный вызов
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
    /// <returns>Статическое свойство из класса <see cref="Registry"/></returns>
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
    /// Закрывает все открытые разделы, вызывая <see cref="RegistryKey.Close()"/>
    /// </summary>
    public void Close()
    {
      if (IsDisposed)
        return;

      foreach (RegistryKey item in _Items.Values)
      {
        try
        {
          if (item != null)
            item.Close();
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

      List<KeyValuePair<string, RegistryKey>> lst = null;
      string keyName2 = keyName + "\\";
      foreach (KeyValuePair<string, RegistryKey> pair in _Items)
      {
        if (includeMain && 
          String.Equals(pair.Key, keyName, StringComparison.OrdinalIgnoreCase) && 
          keyName.IndexOf("\\")>=0) // корневой раздел не закрываем
        {
          if (lst == null) lst = new List<KeyValuePair<string, RegistryKey>>();
          lst.Add(pair);
        }

        if (pair.Key.StartsWith(keyName2, StringComparison.OrdinalIgnoreCase))
        {
          if (lst == null) lst = new List<KeyValuePair<string, RegistryKey>>();
          lst.Add(pair);
        }
      }

      if (lst == null)
        return;

      foreach (KeyValuePair<string, RegistryKey> pair in lst)
      {
        if (pair.Value != null)
        {
          try { pair.Value.Close(); }
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

      string parentKeyName = keyName.Substring(0, p);

      if (!Exists(parentKeyName)) // рекурсивный вызов
        return false;

      // Нужен родительский узел
      RegistryKey parentKey = this[parentKeyName];
      if (parentKey == null)
        return false; // 21.02.2020
      string subName = keyName.Substring(p + 1);

      RegistryKey subKey = parentKey.OpenSubKey(subName, !IsReadOnly);
      if (subKey == null)
        return false;

      _Items.Add(keyName, subKey);
      return true;
    }

    /// <summary>
    /// Удаляет раздел, задаваемый путем <paramref name="keyName"/> и, рекурсивно, все дочерние разделы.
    /// Для удаления используется метод <see cref="RegistryKey.DeleteSubKeyTree(string)"/>.
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
        throw new InvalidOperationException("Нельзя удалить корневой узел");

      string parentKeyName = keyName.Substring(0, p);
      RegistryKey parentKey = this[parentKeyName];
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
    /// <param name="valueName">Имя значения. Для получения значения по умолчанию задайте пустую стрку</param>
    /// <returns>Значение</returns>
    public object GetValue(string keyName, string valueName)
    {
      RegistryKey key = this[keyName];
      if (key == null)
        return null;
      else
        return key.GetValue(valueName);
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
      RegistryKey startKey = this[keyName];
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
          yield return new EnumRegistryEntry(currInfo.CurrKey, stack.Count - 1, String.Empty);

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
              yield return new EnumRegistryEntry(currInfo.CurrKey, stack.Count - 1, currInfo.ValueNames[currInfo.ValueIndex]);

            }
          }
        }

        currInfo.SubKeyIndex++;
        if (currInfo.SubKeyIndex < currInfo.SubKeyNames.Length)
        {
          string nm = currInfo.SubKeyNames[currInfo.SubKeyIndex];
          string childKeyName = currInfo.CurrKey.Name + "\\" + nm;
          RegistryKey childKey = this[childKeyName];
          if (childKey != null) // вдруг нет прав доступа?
            stack.Push(new EnumInfo(childKey, enumerateValues));
          continue;
        }

        // больше нет дочерних разделов
        stack.Pop();
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
      EnumarableProxy proxy = new EnumarableProxy();
      proxy.KeyName = keyName;
      proxy.EnumerateValues = enumerateValues;
      return proxy;
    }

    #endregion
  }
}
