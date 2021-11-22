// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using FreeLibSet.Core;

// Работа с Реестром Windows с управлением способром виртулиазации для 64-битных версий Windows
// В Net Framework 4 это реализовано классом RegistryKey, но в Net Framework 2 такой возможности нет

namespace FreeLibSet.Win32
{
  #region Перечисление RegistryView2

  /// <summary>
  /// Способ виртуализации при просмотре реестра в RegistryKey2.
  /// Идентично перечислению Microsoft.Win32.RegistryView в Net Framework 4/
  /// </summary>
  public enum RegistryView2
  {
    /// <summary>
    /// 0x0000 operate on the default registry view
    /// </summary>
    Default = 0,

    /// <summary>
    /// 0x0100 operate on the 64-bit registry view
    /// </summary>
    Registry64 = 0x0100,

    /// <summary>
    /// 0x0200 operate on the 32-bit registry view
    /// </summary>
    Registry32 = 0x0200,
  };

  #endregion

  /// <summary>
  /// В Net Framework 2 нет возможности использовать RegistryView.
  /// Для доступа к узлам реестра используйте статический метод OpenBaseKey() и, далее, OpenSubKey()/CreateSubKey.
  /// Обычно удобнее использовать класс RegistryTree2, который автоматически откывает нужные родительские узлы
  /// и обеспечивает их буферизацию.
  /// Класс с большими упрощениями взят из Mono и модифицирован.
  /// </summary>
  public sealed class RegistryKey2 : DisposableObject
  {
    #region Поддержка операционной системы

    /// <summary>
    /// Возвращает true для OS Windows, которые поддерживают работу с реестром
    /// </summary>
    public static bool IsSupported
    {
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
            return true;
          default:
            return false;
        }
      }
    }

    #endregion

    #region Доступ к корневым узлам

    /// <summary>
    /// Открывает корневой раздел реестра
    /// </summary>
    /// <param name="hKey">Корневой раздел</param>
    /// <param name="view">Способ виртуализации</param>
    /// <returns>Корневой раздел</returns>
    public static RegistryKey2 OpenBaseKey(Microsoft.Win32.RegistryHive hKey, RegistryView2 view)
    {
      if (!IsSupported)
        throw new PlatformNotSupportedException();

      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        view = RegistryView2.Default; // 11.12.2018

      long dictKey = ((long)(int)view << 32) | ((uint)(int)hKey); // почему так - см. справку к предупреждению компилятора CS0675

      RegistryKey2 res;
      lock (_BaseKeys)
      {
        if (!_BaseKeys.TryGetValue(dictKey, out res))
        {
          string Name;
          switch (hKey)
          {
            case Microsoft.Win32.RegistryHive.ClassesRoot: Name = "HKEY_CLASSES_ROOT"; break;
            case Microsoft.Win32.RegistryHive.CurrentUser: Name = "HKEY_CURRENT_USER"; break;
            case Microsoft.Win32.RegistryHive.LocalMachine: Name = "HKEY_LOCAL_MACHINE"; break;
            case Microsoft.Win32.RegistryHive.Users: Name = "HKEY_USERS"; break;
            case Microsoft.Win32.RegistryHive.CurrentConfig: Name = "HKEY_CURRENT_CONFIG"; break;
            default:
              throw new ArgumentException("Неподдерживаемый hKey", "hKey");
          }

          res = new RegistryKey2(Name, (IntPtr)hKey, false, view);
          _BaseKeys[dictKey] = res;
        }
      }
      return res;
    }

    /// <summary>
    /// Так как RegistryKey2 является DisposableObject(), лучше не доводить дела до вызова деструктора, чтобы "не портить статистику" в отладочном режиме
    /// </summary>
    private static Dictionary<long, RegistryKey2> _BaseKeys = new Dictionary<long, RegistryKey2>(); // 22.05.2020

    #endregion

    #region Константы

    const int OpenRegKeyRead = 0x00020019;
    const int OpenRegKeyWrite = 0x00020006;

    // FIXME this is hard coded on Mono, can it be determined dynamically? 
    private static readonly int NativeBytesPerCharacter = Marshal.SystemDefaultCharSize;

    const int Int32ByteSize = 4;
    const int Int64ByteSize = 8;

    #endregion

    #region Конструкторы и Dispose

    //
    internal RegistryKey2(string name, IntPtr handle, bool ownHandle, RegistryView2 view)
    {
#if DEBUG
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (!IsValidHandle(handle))
      {
        ArgumentException ex = new ArgumentException("Handle=0", "handle");
        ex.Data["RegistryKey2.Name"] = name;
        ex.Data["RegistryKey2.OwnHandle"] = ownHandle;
        ex.Data["RegistryKey2.View"] = view;
        throw ex;
      }
#endif

      _Name = name;
      _View = view;
      _Handle = handle;
      _OwnHandle = ownHandle;
    }

    private static bool IsValidHandle(IntPtr handle)
    {
      return handle != IntPtr.Zero;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (_OwnHandle && IsValidHandle(_Handle))
      {
        NativeMethods.RegCloseKey(_Handle);
        _Handle = IntPtr.Zero;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Полный путь к узлу реестра, напаример "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows"
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    /// <summary>
    /// Режим доступа для Windows-32/64
    /// </summary>
    public RegistryView2 View { get { return _View; } }
    private RegistryView2 _View;

    private IntPtr _Handle;

    private bool _OwnHandle;

    #endregion

    #region Доступ к дочерним узлам

    /// <summary>
    /// Возвращает список имен дочерних узлов
    /// </summary>
    /// <returns>Список имен</returns>
    public string[] GetSubKeyNames()
    {
      StringBuilder buffer = new StringBuilder(BufferMaxLength);
      List<string> keys = new List<string>();

      for (int index = 0; true; index++)
      {
        int result = NativeMethods.RegEnumKey(_Handle, index, buffer, buffer.Capacity);

        if (result == Win32ResultCode.Success)
        {
          keys.Add(buffer.ToString());
          buffer.Length = 0;
          continue;
        }

        if (result == Win32ResultCode.NoMoreEntries)
          break;

        // should not be here!
        GenerateException(result);
      }
      return keys.ToArray();
    }

    /// <summary>
    /// Retrieves the count of subkeys of the current key.
    /// </summary>
    public int SubKeyCount
    {
      get
      {
        int index;
        StringBuilder stringBuffer = new StringBuilder(BufferMaxLength);

        for (index = 0; true; index++)
        {
          int result = NativeMethods.RegEnumKey(_Handle, index, stringBuffer,
            stringBuffer.Capacity);

          if (result == Win32ResultCode.Success)
            continue;

          if (result == Win32ResultCode.NoMoreEntries)
            break;

          // something is wrong!!
          GenerateException(result);
        }
        return index;
      }
    }

    /// <summary>
    /// Открывает дочерний узел с заданным именем на чтение или на запись.
    /// Возвращает null, если нет такого дочернего узла.
    /// </summary>
    /// <param name="name">Имя дочернего узла (без слэшей)</param>
    /// <param name="writable">true если требуется запись значений или дочерниз узлов</param>
    /// <returns></returns>
    public RegistryKey2 OpenSubKey(string name, bool writable)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      int access = OpenRegKeyRead;
      if (writable)
        access |= OpenRegKeyWrite;

      IntPtr ChildHandle;
      int result = NativeMethods.RegOpenKeyEx(_Handle,
          name,
          IntPtr.Zero,
          access | (int)View,
          out ChildHandle);

      // TODO: В Windows-98 ChildHandle=0 (11.12.2018)

      if (result == Win32ResultCode.FileNotFound || result == Win32ResultCode.MarkedForDeletion)
        return null;

      if (result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["OpenSubKey-SubKeyName"] = name;
          e.Data["OpenSubKey-Writeable"] = writable;
          throw;
        }
      }

      return new RegistryKey2(this.Name + "\\" + name, ChildHandle, true, View);
    }

    /// <summary>
    /// Создает дочерний узел
    /// </summary>
    /// <param name="name">Имя дочернего узла (без слэшей)</param>
    /// <returns>Созданный узел</returns>
    public RegistryKey2 CreateSubKey(string name)
    {
      IntPtr ChildHandle;
      int disposition;

      int result = NativeMethods.RegCreateKeyEx(_Handle,
          name,
          0, // reserved
          IntPtr.Zero, // lpClass
          0, // options
          OpenRegKeyWrite | (int)View, // access
          IntPtr.Zero, // securityAttrs
          out ChildHandle, out disposition);

      if (result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["CreateSubKey-SubKeyName"] = name;
          throw;
        }
      }
      return new RegistryKey2(this.Name + "\\" + name, ChildHandle, true, View);
    }

    /// <summary>
    ///	Delete the specified subkey.
    /// </summary>
    /// <param name="subkey"></param>
    public void DeleteSubKey(string subkey)
    {
      DeleteSubKey(subkey, true);
    }

    /// <summary>
    /// Deletes the specified subkey. The string subkey is not case-sensitive.
    /// </summary>
    /// <param name="subkey">The name of the subkey to delete</param>
    /// <param name="throwOnMissingSubKey">Indicates whether an exception should be raised if the specified subkey cannot
    /// be found. If this argument is true and the specified subkey does not exist,
    /// then an exception is raised. If this argument is false and the specified
    /// subkey does not exist, then no action is taken</param>
    public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
    {

      RegistryKey2 child = OpenSubKey(subkey, true);

      if (child == null)
      {
        if (throwOnMissingSubKey)
          throw new ArgumentException("Cannot delete a subkey tree"
            + " because the subkey does not exist.");
        return;
      }

      if (child.SubKeyCount > 0)
      {
        throw new InvalidOperationException("Registry key has subkeys"
          + " and recursive removes are not supported by this method.");
      }

      child.Dispose();

      int result = NativeMethods.RegDeleteKey(_Handle, subkey);

      if (result == Win32ResultCode.FileNotFound)
      {
        if (throwOnMissingSubKey)
          throw new ArgumentException("key " + subkey);
        return;
      }

      if (result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["DeleteSubKey-SubKeyName"] = subkey;
          throw;
        }
      }
    }

    /// <summary>
    /// Deletes a subkey and any child subkeys recursively. The string subkey is
    /// not case-sensitive.
    /// Эта версия выбрасывает исключение, если подраздела не существует.
    /// </summary>
    /// <param name="subkey">The subkey to delete.</param>
    public void DeleteSubKeyTree(string subkey)
    {
      DeleteSubKeyTree(subkey, true);
    }

    /// <summary>
    /// Deletes a subkey and any child subkeys recursively. The string subkey is
    /// not case-sensitive.
    /// </summary>
    /// <param name="subkey">The subkey to delete.</param>
    /// <param name="throwOnMissingSubKey">Надо ли выбрасывать исключение, если раздела не существует</param>
    public void DeleteSubKeyTree(string subkey, bool throwOnMissingSubKey)
    {
      RegistryKey2 child = OpenSubKey(subkey, true);
      if (child == null)
      {
        if (!throwOnMissingSubKey)
          return;

        throw new ArgumentException("Cannot delete a subkey tree"
          + " because the subkey does not exist.");
      }

      try
      {
        child.DeleteChildKeysAndValues();
      }
      finally
      {
        child.Dispose();
      }
      DeleteSubKey(subkey, false);
    }


    /// <summary>
    ///	Utility method to delelte a key's sub keys and values.
    ///	This method removes a level of indirection when deleting
    ///	key node trees.
    /// </summary>
    private void DeleteChildKeysAndValues()
    {
      if (!_OwnHandle)
        throw new InvalidOperationException("Корневой узел нельзя чистить");

      string[] subKeys = GetSubKeyNames();
      for (int i = 0; i < subKeys.Length; i++)
      {
        RegistryKey2 sub = OpenSubKey(subKeys[i], true);
        if (sub != null) // 27.12.2020
        {
          using (sub)
          {
            sub.DeleteChildKeysAndValues();
          }
        }
        DeleteSubKey(subKeys[i], false);
      }

      string[] values = GetValueNames();
      for (int i = 0; i < values.Length; i++)
        DeleteValue(values[i], false);
    }

    #endregion

    #region Извлечение значений

    const int BufferMaxLength = 1024;

    /// <summary>
    /// Retrieves the count of values in the key.
    /// </summary>
    public int ValueCount
    {
      get
      {
        int index, result, bufferCapacity;
        Microsoft.Win32.RegistryValueKind type;
        StringBuilder buffer = new StringBuilder(BufferMaxLength);

        for (index = 0; true; index++)
        {
          type = 0;
          bufferCapacity = buffer.Capacity;
          result = NativeMethods.RegEnumValue(_Handle, index,
                     buffer, ref bufferCapacity,
                     IntPtr.Zero, ref type,
                     IntPtr.Zero, IntPtr.Zero);

          if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData)
            continue;

          if (result == Win32ResultCode.NoMoreEntries)
            break;

          // something is wrong
          GenerateException(result);
        }
        return index;
      }
    }

    /// <summary>
    /// Возвращает список значений
    /// </summary>
    /// <returns>Массив имен</returns>
    public string[] GetValueNames()
    {
      List<string> values = new List<string>();

      for (int index = 0; true; index++)
      {
        StringBuilder buffer = new StringBuilder(BufferMaxLength);
        int bufferCapacity = buffer.Capacity;
        Microsoft.Win32.RegistryValueKind type = 0;

        int result = NativeMethods.RegEnumValue(_Handle, index, buffer, ref bufferCapacity,
              IntPtr.Zero, ref type, IntPtr.Zero, IntPtr.Zero);

        if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData)
        {
          values.Add(buffer.ToString());
          continue;
        }

        if (result == Win32ResultCode.NoMoreEntries)
          break;

        GenerateException(result);
      }

      return values.ToArray();
    }

    /// <summary>
    /// Возвращает значение
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public object GetValue(string name)
    {
      return GetValue(name, null);
    }

    /// <summary>
    /// Acctually read a registry value. Requires knowledge of the
    /// value's type and size.
    /// </summary>
    public object GetValue(string name, object defaultValue)
    {
      Microsoft.Win32.RegistryValueKind type = 0;
      int size = 0;
      object obj = null;
      int result = NativeMethods.RegQueryValueEx(_Handle, name, IntPtr.Zero, ref type, IntPtr.Zero, ref size);

      if (result == Win32ResultCode.FileNotFound || result == Win32ResultCode.MarkedForDeletion)
      {
        return defaultValue;
      }

      if (result != Win32ResultCode.MoreData && result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["GetValue-ValueName"] = name;
          e.Data["GetValue-DefaultValue"] = defaultValue;
          throw;
        }
      }

      if (type == Microsoft.Win32.RegistryValueKind.String)
      {
        byte[] data;
        result = GetBinaryValue(name, type, out data, size);
        obj = DecodeString(data, false);
      }
      else if (type == Microsoft.Win32.RegistryValueKind.ExpandString)
      {
        byte[] data;
        result = GetBinaryValue(name, type, out data, size);
        obj = DecodeString(data, false);
        obj = Environment.ExpandEnvironmentVariables((string)obj);
      }
      else if (type == Microsoft.Win32.RegistryValueKind.DWord)
      {
        int data = 0;
        result = NativeMethods.RegQueryValueEx(_Handle, name, IntPtr.Zero, ref type, ref data, ref size);
        obj = data;
      }
      else if (type == Microsoft.Win32.RegistryValueKind.QWord)
      {
        long data = 0;
        result = NativeMethods.RegQueryValueEx(_Handle, name, IntPtr.Zero, ref type, ref data, ref size);
        obj = data;
      }
      else if (type == Microsoft.Win32.RegistryValueKind.Binary)
      {
        byte[] data;
        result = GetBinaryValue(name, type, out data, size);
        obj = data;
      }
      else if (type == Microsoft.Win32.RegistryValueKind.MultiString)
      {
        obj = null;
        byte[] data;
        result = GetBinaryValue(name, type, out data, size);

        if (result == Win32ResultCode.Success)
          obj = DecodeString(data, true).Split('\0');
      }
      else
      {
        // should never get here
        throw new SystemException();
      }

      // check result codes again:
      if (result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["GetValue-ValueName"] = name;
          e.Data["GetValue-DefaultValue"] = defaultValue;
          throw;
        }
      }

      return obj;
    }


    /// <summary>
    ///	Get a binary value.
    /// </summary>
    private int GetBinaryValue(string name, Microsoft.Win32.RegistryValueKind type, out byte[] data, int size)
    {
      byte[] internalData = new byte[size];
      int result = NativeMethods.RegQueryValueEx(_Handle, name, IntPtr.Zero, ref type, internalData, ref size);
      data = internalData;
      return result;
    }

    private static Encoding GetEncoding()
    {
      if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        return Encoding.Unicode;
      else
        return Encoding.Default; // 13.12.2018
    }

    /// <summary>
    ///	decode a byte array as a string, and strip trailing nulls
    /// </summary>
    static internal string DecodeString(byte[] data, bool isMultiLine)
    {
      string res = GetEncoding().GetString(data);
      if (isMultiLine)
      {
        // 22.01.2019
        // Для многострочного текста удаляем последние два символа "\0\0"
        if (res.Length == 0)
          return res;
        if (res[res.Length - 1] == '\0')
          res = res.Substring(0, res.Length - 1);
        if (res.Length == 0)
          return res;
        if (res[res.Length - 1] == '\0')
          res = res.Substring(0, res.Length - 1);
        return res;
      }
      else
      {
        int idx = res.IndexOf('\0');
        if (idx >= 0)
          res = res.TrimEnd('\0');
      }
      return res;
    }

    /// <summary>
    /// Sets the specified name/value pair.
    /// </summary>
    /// <param name="name">The name of the value to store.</param>
    /// <param name="value">The data to be stored.</param>
    public void SetValue(string name, object value)
    {
      Type type = value.GetType();
      int result;

      if (type == typeof(int))
      {
        int rawValue = (int)value;
        result = NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.DWord, ref rawValue, Int32ByteSize);
      }
      else if (type == typeof(byte[]))
      {
        byte[] rawValue = (byte[])value;
        result = NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.Binary, rawValue, rawValue.Length);
      }
      else if (type == typeof(string[]))
      {
        string[] values = (string[])value;
        StringBuilder fullStringValue = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
          fullStringValue.Append(values[i]);
          fullStringValue.Append('\0');
        }
        fullStringValue.Append('\0');

        byte[] rawValue = GetEncoding().GetBytes(fullStringValue.ToString());

        result = NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.MultiString, rawValue, rawValue.Length);
      }
      else if (type.IsArray)
      {
        throw new ArgumentException("Only string and byte arrays can written as registry values");
      }
      else
      {
        string rawValue = String.Format("{0}{1}", value, '\0');
        result = NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.String, rawValue,
              rawValue.Length * NativeBytesPerCharacter);
      }

      // handle the result codes
      if (result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["SetValue-ValueName"] = name;
          e.Data["SetValue-Value"] = value;
          throw;
        }
      }
    }

    /// <summary>
    /// Sets the value of a name/value pair in the registry key, using the specified
    ///  registry data type.
    /// </summary>
    /// <param name="name">The name of the value to be stored.</param>
    /// <param name="value">The data to be stored.</param>
    /// <param name="valueKind">The registry data type to use when storing the data.</param>
    public void SetValue(string name, object value, Microsoft.Win32.RegistryValueKind valueKind)
    {
      Type type = value.GetType();

      switch (valueKind)
      {
        case Microsoft.Win32.RegistryValueKind.QWord:
          try
          {
            long rawValue = Convert.ToInt64(value);
            CheckResult(NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.QWord, ref rawValue, Int64ByteSize));
            return;
          }
          catch (OverflowException)
          {
          }
          break;
        case Microsoft.Win32.RegistryValueKind.DWord:
          try
          {
            int rawValue = Convert.ToInt32(value);
            CheckResult(NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.DWord, ref rawValue, Int32ByteSize));
            return;
          }
          catch (OverflowException)
          {
          }
          break;
        case Microsoft.Win32.RegistryValueKind.Binary:
          if (type == typeof(byte[]))
          {
            byte[] rawValue = (byte[])value;
            CheckResult(NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.Binary, rawValue, rawValue.Length));
            return;
          }
          break;
        case Microsoft.Win32.RegistryValueKind.MultiString:
          if (type == typeof(string[]))
          {
            string[] values = (string[])value;
            StringBuilder fullStringValue = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
              fullStringValue.Append(values[i]);
              fullStringValue.Append('\0');
            }
            fullStringValue.Append('\0');

            byte[] rawValue = GetEncoding().GetBytes(fullStringValue.ToString());

            CheckResult(NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, Microsoft.Win32.RegistryValueKind.MultiString, rawValue, rawValue.Length));
            return;
          }
          break;
        case Microsoft.Win32.RegistryValueKind.String:
        case Microsoft.Win32.RegistryValueKind.ExpandString:
          if (type == typeof(string))
          {
            string rawValue = String.Format("{0}{1}", value, '\0');
            CheckResult(NativeMethods.RegSetValueEx(_Handle, name, IntPtr.Zero, valueKind, rawValue,
                  rawValue.Length * NativeBytesPerCharacter));
            return;
          }
          break;
        default:
          if (type.IsArray)
          {
            throw new ArgumentException("Only string and byte arrays can written as registry values");
          }
          break;
      }

      throw new ArgumentException("Type does not match the valueKind");
    }


    /// <summary>
    /// Deletes the specified value from this key.
    /// </summary>
    /// <param name="name">The name of the value to delete.</param>
    public void DeleteValue(string name)
    {
      DeleteValue(name, true);
    }

    /// <summary>
    /// Deletes the specified value from this key.
    /// </summary>
    /// <param name="name">The name of the value to delete.</param>
    /// <param name="throwOnMissingValue">Indicates whether an exception should be raised if the specified value cannot
    /// be found. If this argument is true and the specified value does not exist,
    /// then an exception is raised. If this argument is false and the specified
    /// value does not exist, then no action is taken</param>
    public void DeleteValue(string name, bool throwOnMissingValue)
    {
      int result = NativeMethods.RegDeleteValue(_Handle, name);

      if (result == Win32ResultCode.MarkedForDeletion)
        return;

      if (result == Win32ResultCode.FileNotFound)
      {
        if (throwOnMissingValue)
          throw new ArgumentException("value " + name);
        return;
      }

      if (result != Win32ResultCode.Success)
      {
        try
        {
          GenerateException(result);
        }
        catch (Exception e) // 06.05.2019
        {
          e.Data["DeleteValue-ValueName"] = name;
          throw;
        }
      }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Writes all the attributes of the specified open registry key into the registry.
    /// </summary>
    public void Flush()
    {
      NativeMethods.RegFlushKey(_Handle);
    }

    /// <summary>
    /// convert a win32 error code into an appropriate exception.
    /// </summary>
    private void GenerateException(int errorCode)
    {
      try
      {
        switch (errorCode)
        {
          case Win32ResultCode.FileNotFound:
          case Win32ResultCode.InvalidParameter:
            throw new ArgumentException();
          case Win32ResultCode.AccessDenied:
            throw new SecurityException();
          case Win32ResultCode.NetworkPathNotFound:
            throw new IOException("The network path was not found.");
          case Win32ResultCode.InvalidHandle:
            throw new IOException("Invalid handle.");
          //case Win32ResultCode.MarkedForDeletion:
          //  throw RegistryKey.CreateMarkedForDeletionException();
          case Win32ResultCode.ChildMustBeVolatile:
            throw new IOException("Cannot create a stable subkey under a volatile parent key.");
          default:
            // unidentified system exception
            throw new System.ComponentModel.Win32Exception(errorCode);
        }
      }
      catch (Exception e)
      {
        e.Data["RegistryKey2"] = this.ToString(); // 06.05.2019
        throw;
      }
    }

    private void CheckResult(int result)
    {
      if (result != Win32ResultCode.Success)
      {
        GenerateException(result);
      }
    }

    /// <summary>
    /// Возвращает свойство Name
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return this.Name;
    }

    #endregion

    #region Импорт функций Windows

    private static class NativeMethods
    {
      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegOpenKeyEx")]
      public static extern int RegOpenKeyEx(IntPtr keyBase,
          string keyName, IntPtr reserved, int access,
          out IntPtr keyHandle);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegCreateKeyEx")]
      public static extern int RegCreateKeyEx(IntPtr keyBase, string keyName, int reserved,
        IntPtr lpClass, int options, int access, IntPtr securityAttrs,
        out IntPtr keyHandle, out int disposition);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegCloseKey")]
      public static extern int RegCloseKey(IntPtr keyHandle);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegEnumKey")]
      public static extern int RegEnumKey(IntPtr keyBase, int index, StringBuilder nameBuffer, int bufferLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegEnumValue")]
      public static extern int RegEnumValue(IntPtr keyBase,
          int index, StringBuilder nameBuffer,
          ref int nameLength, IntPtr reserved,
          ref Microsoft.Win32.RegistryValueKind type, IntPtr data, IntPtr dataLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegQueryValueEx")]
      public static extern int RegQueryValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, ref Microsoft.Win32.RegistryValueKind type,
          IntPtr zero, ref int dataSize);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegQueryValueEx")]
      public static extern int RegQueryValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, ref Microsoft.Win32.RegistryValueKind type,
          [Out] byte[] data, ref int dataSize);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegQueryValueEx")]
      public static extern int RegQueryValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, ref Microsoft.Win32.RegistryValueKind type,
          ref int data, ref int dataSize);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegQueryValueEx")]
      public static extern int RegQueryValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, ref Microsoft.Win32.RegistryValueKind type,
          ref long data, ref int dataSize);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegSetValueEx")]
      public static extern int RegSetValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, Microsoft.Win32.RegistryValueKind type,
          string data, int rawDataLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegSetValueEx")]
      public static extern int RegSetValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, Microsoft.Win32.RegistryValueKind type,
          byte[] rawData, int rawDataLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegSetValueEx")]
      public static extern int RegSetValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, Microsoft.Win32.RegistryValueKind type,
          ref int data, int rawDataLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegSetValueEx")]
      public static extern int RegSetValueEx(IntPtr keyBase,
          string valueName, IntPtr reserved, Microsoft.Win32.RegistryValueKind type,
          ref long data, int rawDataLength);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegDeleteKey")]
      public static extern int RegDeleteKey(IntPtr keyHandle, string valueName);

      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegDeleteValue")]
      public static extern int RegDeleteValue(IntPtr keyHandle, string valueName);


      [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, EntryPoint = "RegFlushKey")]
      public static extern int RegFlushKey(IntPtr keyHandle);
    }

    /// <summary>
    ///	These are some values for Win32 result codes.
    ///
    ///	NOTE: This code could be relocated into a common repository
    ///	for error codes, along with a utility to fetch the matching 
    ///	error messages. These messages should support globalization.
    ///	Maybe the 'glib' libraries provide support for this.
    ///	(see System/System.ComponentModel/Win32Exception.cs)
    /// </summary>
    internal static class Win32ResultCode
    {
      public const int Success = 0;
      public const int FileNotFound = 2;
      public const int AccessDenied = 5;
      public const int InvalidHandle = 6;
      public const int InvalidParameter = 87;
      public const int MoreData = 234;
      public const int NetworkPathNotFound = 53;
      public const int NoMoreEntries = 259;
      public const int MarkedForDeletion = 1018;
      public const int ChildMustBeVolatile = 1021;
    }

    #endregion
  }
}
