// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using FreeLibSet.IO;
using System.Globalization;
using FreeLibSet.Core;

namespace FreeLibSet.OLE
{
  /// <summary>
  /// Вспомогательный класс для работы с серверами OLE, например,
  /// Microsoft Excel.
  /// Реализует позднее связывание объектов
  /// </summary>
  public class OLEHelper : DisposableObject
  {
    #region Конструктор и Disposing

    /// <summary>
    /// Создает OLEHelper
    /// </summary>
    public OLEHelper()
    {
      _Args1 = new object[1];
      _Args2 = new object[2];
      _Args3 = new object[3];
      _Args4 = new object[4];
    }

    /// <summary>
    /// Освобождает ресурсы, вызывая Marshal.ReleaseComObject()
    /// </summary>
    /// <param name="disposing">true, если вызов из Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      // 02.01.2021
      //if (disposing)
      //{
      // Разрушение ресурсов
      if (_MainObj != null)
      {
        Marshal.ReleaseComObject(_MainObj);
        _MainObj = null;
      }
      //}
      base.Dispose(disposing);
    }

    #endregion

    #region Основной объект (обычно - приложение)

    /// <summary>
    /// Основной объект (например, "Word.Application"). 
    /// Инициализируется вызовом CreateMainObj() или GetActiveMainObj()
    /// </summary>
    public object MainObj { get { return _MainObj; } }
    private object _MainObj;

    /// <summary>
    /// Создание нового экземпляра объекта
    /// </summary>
    /// <param name="progId">Идентификатор приложения COM, например, "Word.Application". Используется при вызове метода Type.GetTypeFromProgID()</param>
    public void CreateMainObj(string progId)
    {
      CheckNotDisposed();
      if (_MainObj != null)
        throw ExceptionFactory.RepeatedCall(this, "CreateMainObj()");

      Type mainObjType = Type.GetTypeFromProgID(progId);
      if (mainObjType == null)
        throw ExceptionFactory.ArgUnknownValue("progId", progId);
      _MainObj = Activator.CreateInstance(mainObjType);
    }

#if NET
    // Нет метода Marshal.GetActiveObject(String)
    // https://stackoverflow.com/questions/58010510/no-definition-found-for-getactiveobject-from-system-runtime-interopservices-mars

    private static class Marshal2
    {
      #region Native Methods

      internal const string OLEAUT32 = "oleaut32.dll";
      internal const string OLE32 = "ole32.dll";

      [DllImport(OLE32, PreserveSig = false)]
      [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
      [System.Security.SuppressUnmanagedCodeSecurity]
      [System.Security.SecurityCritical]  // auto-generated
      private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

      [DllImport(OLE32, PreserveSig = false)]
      [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
      [System.Security.SuppressUnmanagedCodeSecurity]
      [System.Security.SecurityCritical]  // auto-generated
      private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

      [DllImport(OLEAUT32, PreserveSig = false)]
      [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
      [System.Security.SuppressUnmanagedCodeSecurity]
      [System.Security.SecurityCritical]  // auto-generated
      private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);

      #endregion

      #region GetActiveObject()

      [System.Security.SecurityCritical]  // auto-generated_required
      public static object GetActiveObject(string progID)
      {
        object obj;
        Guid clsid;

        // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
        // CLSIDFromProgIDEx doesn't exist.
        try
        {
          CLSIDFromProgIDEx(progID, out clsid);
        }
        catch (Exception)
        {
          CLSIDFromProgID(progID, out clsid);
        }

        GetActiveObject(ref clsid, IntPtr.Zero, out obj);
        return obj;
      }

      #endregion
    }

#endif

    /// <summary>
    /// Получение активного объекта и сохранение ссылки на него в MainObj
    /// </summary>
    /// <param name="progId">Идентификатор приложения COM, например, "Word.Application". Используется при вызове метода Marshal.GetActiveObject()</param>
    public void GetActiveMainObj(string progId)
    {
      CheckNotDisposed();
      if (_MainObj != null)
        throw ExceptionFactory.RepeatedCall(this, "GetActiveMainObj()");

#if NET
      _MainObj = Marshal2.GetActiveObject(progId);
#else
      _MainObj = Marshal.GetActiveObject(progId);
#endif
    }

    #endregion

    #region Текущая культура

    /// <summary>
    /// Культура, используемая при вызове свойств и методов.
    /// По умолчанию - null
    /// </summary>
    public CultureInfo CultureInfo
    {
      get { return _CultureInfo; }
      set { _CultureInfo = value; }
    }
    private CultureInfo _CultureInfo;

    /// <summary>
    /// Идентификатор локали.
    /// Возвращает CultureInfo.LCID.
    /// Для приложений Microsoft Office свойство должно быть установлено в 0x0409
    /// </summary>
    public int LCID
    {
      get
      {
        if (_CultureInfo == null)
          return CultureInfo.CurrentCulture.LCID;
        else
          return _CultureInfo.LCID;
      }
      set
      {
        _CultureInfo = CultureInfo.GetCultureInfo(value);
        if (_CultureInfo == null)
          throw ExceptionFactory.ArgUnknownValue("value", value);
      }
    }

    #endregion

    #region Установка свойств

    /// <summary>
    /// Получить свойство <paramref name="name"/> объекта <paramref name="obj"/>
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя свойства</param>
    /// <returns>Значение свойства</returns>
    public object GetProp(object obj, string name)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      return obj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, obj, null, CultureInfo);
    }

    /// <summary>
    /// Установить свойство <paramref name="name"/> объекта <paramref name="obj"/>
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя свойства</param>
    /// <param name="value">Значение свойства</param>
    public void SetProp(object obj, string name, object value)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args1[0] = value;
      obj.GetType().InvokeMember(name, BindingFlags.SetProperty, null, obj, _Args1, CultureInfo);
    }

    /// <summary>
    /// Получить значение индексированного свойства <paramref name="name"/> объекта <paramref name="obj"/>
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя свойства</param>
    /// <param name="index">Индекс</param>
    /// <returns>Значение свойства</returns>
    public object GetIndexProp(object obj, string name, object index)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args1[0] = index;
      return obj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, obj, _Args1, CultureInfo);
    }

    /// <summary>
    /// Получить значение индексированного свойства <paramref name="name"/> объекта <paramref name="obj"/>
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя свойства</param>
    /// <param name="index1">Первый индекс</param>
    /// <param name="index2">Второй индекс</param>
    /// <returns>Значение свойства</returns>
    public object GetIndexProp(object obj, string name, object index1, object index2)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args2[0] = index1;
      _Args2[1] = index2;
      return obj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, obj, _Args2, CultureInfo);
    }

    #endregion

    #region Вызов методов

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> без аргумента
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object Call(object obj, string name)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, null, CultureInfo);
    }

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> с одним аргументом
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <param name="arg1">Аргумент</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object Call(object obj, string name, object arg1)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args1[0] = arg1;
      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, _Args1, CultureInfo);
    }

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> с двумя аргументами
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object Call(object obj, string name, object arg1, object arg2)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args2[0] = arg1;
      _Args2[1] = arg2;
      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, _Args2, CultureInfo);
    }

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> с тремя аргументами
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <param name="arg3">Третий аргумент</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object Call(object obj, string name, object arg1, object arg2, object arg3)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args3[0] = arg1;
      _Args3[1] = arg2;
      _Args3[2] = arg3;
      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, _Args3, CultureInfo);
    }

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> с тремя аргументами
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <param name="arg3">Третий аргумент</param>
    /// <param name="arg4">Четвертый аргумент</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object Call(object obj, string name, object arg1, object arg2, object arg3, object arg4)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args4[0] = arg1;
      _Args4[1] = arg2;
      _Args4[2] = arg3;
      _Args4[3] = arg4;
      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, _Args4, CultureInfo);
    }

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> с произвольным количеством аргументов
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <param name="args">Аргументы</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object CallWithArgs(object obj, string name, object[] args)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, args, CultureInfo);
    }

    /// <summary>
    /// Вызвать метод с именем <paramref name="name"/> для объекта <paramref name="obj"/> с произвольным количеством аргументов
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя метода</param>
    /// <param name="args">Аргументы</param>
    /// <returns>Значение, возвращаемое методом</returns>
    public object CallWithArgs0409(object obj, string name, object[] args)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, args, _CultureInfo0409);
    }

    #endregion

    #region Установка свойств для LCID 0409

    private static readonly System.Globalization.CultureInfo _CultureInfo0409 =
      System.Globalization.CultureInfo.GetCultureInfo(0x0409);

    /// <summary>
    /// Чтение свойства в международном формате (например, NumberFormat в Excel)
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя свойства</param>
    /// <returns>Значение свойства</returns>
    public object GetProp0409(object obj, string name)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      return obj.GetType().InvokeMember(name, BindingFlags.GetProperty, null, obj, null, null, _CultureInfo0409, null);
    }

    /// <summary>
    /// Установка свойства в международном формате (например, NumberFormat в Excel)
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="name">Имя свойства</param>
    /// <param name="value">Значение свойства</param>
    public void SetProp0409(object obj, string name, object value)
    {
#if DEBUG
      CheckNotDisposed();
#endif

      _Args1[0] = value;
      obj.GetType().InvokeMember(name, BindingFlags.SetProperty, null, obj, _Args1, null, _CultureInfo0409, null);
    }

    #endregion

    #region Внутренняя реализация

    private readonly object[] _Args1;
    private readonly object[] _Args2;
    private readonly object[] _Args3;
    private readonly object[] _Args4;

    #endregion

    #region Статические методы

    /*
     * Какое безобразие!
     * Неужели нет готовых функций?
     */

    /// <summary>
    /// Получить идентификатор класс для идентификатора ProgID
    /// (я не нашел, где есть такая стандартная функция)
    /// </summary>
    /// <param name="progID">Идентификатор программы, например, "Excel.Application"</param>
    /// <param name="clsID">Сюда записывается GUID</param>
    /// <returns>true, если идентификатор получен</returns>
    public static bool GetClsIDForProgID(string progID, out Guid clsID)
    {
      clsID = new Guid();
      if (String.IsNullOrEmpty(progID))
        return false;
      try
      {
        string KeyName = "HKEY_CLASSES_ROOT\\" + progID + "\\CLSID";
        string s = (string)(Microsoft.Win32.Registry.GetValue(KeyName, String.Empty, String.Empty));
        if (String.IsNullOrEmpty(s))
          return false;
        clsID = new Guid(s);
      }
      catch
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Получить путь к серверу (узел LocalServer32)
    /// </summary>
    /// <param name="clsID">GUID, поиск которого осуществляется в HKEY_CLASSES_ROOT/CLSID</param>
    /// <returns>Путь из реестра. Убираются ключи, например "/Automation", если они заданы в реестре</returns>
    public static AbsPath GetLocalServer32Path(Guid clsID)
    {
      string s;
      try
      {
        string keyName = "HKEY_CLASSES_ROOT\\CLSID\\" + clsID.ToString("B") + "\\LocalServer32";
        s = (string)(Microsoft.Win32.Registry.GetValue(keyName, String.Empty, String.Empty));
        if (String.IsNullOrEmpty(s))
        {
          // 11.01.2013
          // Для 64-битной версии Windows и 32-разрядной версии Office
          keyName = "HKEY_CLASSES_ROOT\\Wow6432Node\\CLSID\\" + clsID.ToString("B") + "\\LocalServer32";
          s = (string)(Microsoft.Win32.Registry.GetValue(keyName, String.Empty, String.Empty));
          if (String.IsNullOrEmpty(s))
            return AbsPath.Empty;
        }
      }
      catch
      {
        return AbsPath.Empty;
      }

      int p = s.IndexOf('/'); // Может быть ключ "/Automation"
      if (p >= 0)
        s = s.Substring(0, p).Trim();
      return new AbsPath(s);
    }

    /// <summary>
    /// Возвращает путь к серверу OLE.
    /// Если сервер не зарегистрирован, возвращает пустой путь AbsPath.Empty.
    /// </summary>
    /// <param name="progID">Идентификатор ProgId, например, "Word.Application"</param>
    /// <returns>Путь к серверу</returns>
    public static AbsPath GetLocalServer32Path(string progID)
    {
      Guid clsID;
      if (!GetClsIDForProgID(progID, out clsID))
        return AbsPath.Empty;
      return GetLocalServer32Path(clsID);
    }

    /// <summary>
    /// Возвращает версию сервера OLE. Версия извлекается из ресурсов EXE/DLL-файла
    /// Если сервер не зарегистрирован, возвращает null.
    /// </summary>
    /// <param name="progID">Идентификатор ProgId, например, "Word.Application"</param>
    /// <returns>Версия</returns>
    public static Version GetLocalServer32Version(string progID)
    {
      AbsPath filePath = GetLocalServer32Path(progID);
      return FileTools.GetFileVersion(filePath);
    }

    #endregion
  }

  /// <summary>
  /// Ссылка на интерфейс + ссылка на Helper
  /// </summary>
  public struct ObjBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="helper">OLEHelper</param>
    public ObjBase(Object obj, OLEHelper helper)
    {
#if DEBUG
      if (obj == null)
        throw new ArgumentNullException("obj");
      Type t = obj.GetType();
      if (!t.IsCOMObject)
        throw new ArgumentException("Not a COM-object", "obj");
      if (helper == null)
        throw new ArgumentNullException("helper");
#endif
      _Obj = obj;
      _Helper = helper;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект OLE
    /// </summary>
    public object Obj { get { return _Obj; } }
    private readonly object _Obj;

    /// <summary>
    /// OLEHelper
    /// </summary>
    public OLEHelper Helper { get { return _Helper; } }
    private readonly OLEHelper _Helper;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _Obj == null; } }

    #endregion
  }
}
