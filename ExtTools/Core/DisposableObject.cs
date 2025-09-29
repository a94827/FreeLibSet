// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using FreeLibSet.Collections;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Расширение интерфейса <see cref="IDisposable"/> свойством <see cref="IsDisposed"/> для проверки разрушения объекта.
  /// Реализуется классами <see cref="DisposableObject"/>, <see cref="SimpleDisposableObject"/> и <see cref="MarshalByRefDisposableObject"/>.
  /// </summary>
  public interface IDisposableObject : IDisposable
  {
    /// <summary>
    /// true, если метод <see cref="IDisposable.Dispose()"/> уже вызывался
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Проверить, что объект не был разрушен. Иначе выбросить исключение
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    void CheckNotDisposed();
  }

  /// <summary>
  /// Реализация интерфейса <see cref="IDisposable"/>. Используйте этот класс в качестве
  /// предка, если не требуется другой базовый класс.
  /// В противном случае используйте собственную реализацию <see cref="IDisposable"/> и 
  /// вызывайте статические методы для отладки объектов.
  /// Eсли в классе нет unmanaged-ресурсов, то не требуется выполнять удаление из деструктора. 
  /// В этом случае следует использовать <see cref="SimpleDisposableObject"/> вместо этого класса.
  /// </summary>
  public class DisposableObject : IDisposableObject
  {
    #region Конструктор и деструктор

    /// <summary>
    /// Создает новый объект.
    /// В отладочном режиме регистрирует объект в списке.
    /// </summary>
    public DisposableObject()
    {
      _IsDisposed = false;
#if DEBUG
      RegisterDisposableObject(this);
#endif
    }

    /// <summary>
    /// Деструктор все равно нужен, даже если для объекта всегда используется явное завершение с помощью <see cref="Dispose()"/>.
    /// Если в конструкторе производного объекта возникает исключение, деструктор будет вызван.
    /// Если был успешный вызов метода <see cref="Dispose()"/>, то вызов деструктора подавляется с помощью <see cref="GC.SuppressFinalize(object)"/>.
    /// </summary>
    ~DisposableObject()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Этот метод должен быть вызван по окончании использования объекта, как положено при использовании <see cref="IDisposable"/>-объекта.
    /// Вызывает виртуальный метод <see cref="Dispose(bool)"/> с аргументом true и предотвращает вызов деструктора.
    /// Повторный вызов метода не выполняет никиких действий.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Этот метод должен быть переопределен.
    /// Базовый метод обязательно должен быть вызван, обычно после выполнения собственных действий.
    /// Устанавливает свойство <see cref="IsDisposed"/> равным true. В отладочном режиме убирает объект из списка.
    /// </summary>
    /// <param name="disposing">true, если метод вызван в явном виде из public-версии <see cref="Dispose()"/>.
    /// false, если метод вызван из деструктора</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_IsDisposed)
      {
#if DEBUG
        UnregisterDisposableObject(this, disposing);
#endif
        _IsDisposed = true;
      }
    }

    /// <summary>
    /// true, если метод <see cref="Dispose()"/> уже вызывался.
    /// </summary>
    public bool IsDisposed { get { return _IsDisposed; } }
    private bool _IsDisposed;

    /// <summary>
    /// Проверить, что объект не был разрушен. Иначе выкинуть исключение
    /// </summary>
    public void CheckNotDisposed()
    {
      if (IsDisposed)
        // Нельзя использовать this.ToString(), т.к. этот метод может сам требовать
        // неразрушенных ресурсов
        throw new ObjectDisposedException(GetType().ToString());
    }

    /// <summary>
    /// Выводит тип объекта с возможным добавлением " (Disposed)"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = base.ToString();
      if (IsDisposed)
        s += " (Disposed)";
      return s;
    }

    #endregion

    #region Отладочные средства

#if DEBUG

    /// <summary>
    /// Коллекция объектов одного типа со слабыми ссылками
    /// </summary>
    private class DisposableObjList : WeakReferenceCollection<object>
    {
      #region Конструктор

      public DisposableObjList(string typeName)
      {
        _TypeName = typeName;
      }

      private string _TypeName;

      public override string ToString()
      {
        return "ObjList for the type " + _TypeName;
      }

      #endregion

      #region Поля

      /// <summary>
      /// Количество объектов, которые были удалены вызовом Dispose()
      /// </summary>
      public long DisposeCount;

      /// <summary>
      /// Количество объектов, которые были удалены вызовом деструктора
      /// </summary>
      public long FinalizeCount;

      #endregion
    }

    /// <summary>
    /// Коллекция типов объектов по именам типов объектов. 
    /// В каждом элементе коллекции хранится массив ArrayList, содержащий ссылки
    /// на объекты.
    /// Все действия с коллекцией выполняются внутри блокировки самой коллекции
    /// </summary>
    private static readonly Dictionary<String, DisposableObjList> ObjectTypes = new Dictionary<string, DisposableObjList>();
#endif

    /// <summary>
    /// Зарегистрировать объект в списке для отладочных целей.
    /// Вызывается из конструктора <see cref="DisposableObject"/> и других классов, желающих отслеживать использование
    /// объектов.
    /// В конфигурации RELEASE никаких действий не выполняется.
    /// </summary>
    /// <param name="obj">Регистрируемый объект</param>
    public static void RegisterDisposableObject(object obj)
    {
#if DEBUG
      if (obj == null || ObjectTypes == null)
        return;
      lock (ObjectTypes)
      {
        DisposableObjList objList = GetObjList(obj);
        objList.Add(obj);
      }
#endif
    }

    /// <summary>
    /// Отменить регистрирацию объекта в списке для отладочных целей.
    /// Вызывается из метода <see cref="DisposableObject.Dispose()"/> и методов <see cref="IDisposable.Dispose()"/> других классов, 
    /// желающих отслеживать использование объектов.
    /// В конфигурации RELEASE никаких действий не выполняется.
    /// Обычно следует использовать перегрузку, принимающую дополнительный аргумент disposing.
    /// </summary>
    /// <param name="obj">Зарегистрированный объект</param>
    public static void UnregisterDisposableObject(object obj)
    {
#if DEBUG
      UnregisterDisposableObject(obj, true);
#endif
    }

    /// <summary>
    /// Отменить регистрирацию объекта в списке для отладочных целей.
    /// Вызывается из метода <see cref="DisposableObject.Dispose()"/> и методов Dispose() других классов, 
    /// желающих отслеживать использование объектов.
    /// В конфигурации RELEASE никаких действий не выполняется.
    /// </summary>
    /// <param name="obj">Зарегистрированный объект</param>
    /// <param name="disposing">Значение аргумента disposing, полученное методом <see cref="Dispose(bool)"/></param>
    public static void UnregisterDisposableObject(object obj, bool disposing)
    {
#if DEBUG
      if (obj == null || ObjectTypes == null)
        return;
      lock (ObjectTypes)
      {
        DisposableObjList objList = GetObjList(obj);
        objList.Remove(obj);
        if (disposing)
          objList.DisposeCount++;
        else
          objList.FinalizeCount++;
      }
#endif
    }

#if DEBUG
    /// <summary>
    /// Получить массив объектов, где хранятся объекты нужного типа.
    /// На момент вызова коллекция должна быть заблокирована.
    /// </summary>
    /// <param name="obj">Объект, который будет добавляться или удаляться</param>
    /// <returns>Массив объектов</returns>
    private static DisposableObjList GetObjList(object obj)
    {
      string typeName = obj.GetType().ToString();
      return GetObjListForType(typeName);
    }

    private static DisposableObjList GetObjListForType(string typeName)
    {
      DisposableObjList res;
      if (!ObjectTypes.TryGetValue(typeName, out res))
      {
        res = new DisposableObjList(typeName);
        ObjectTypes.Add(typeName, res);
      }
      return res;
    }
#endif

    /// <summary>
    /// Получить статистику по объектам.
    /// Эта версия возвращает количество существующих объектов и количество удаленных объектов.
    /// В конфигурации Release возвращает пустые массивы.
    /// </summary>
    /// <param name="objTypes">Сюда записываются типы объектов</param>
    /// <param name="counts">Сюда записываются количества объектов для каждого типа</param>
    /// <param name="disposeCounts">Сюда записываются количества объектов для каждого типа, которые были удалены вызовом <see cref="Dispose()"/></param>
    /// <param name="finalizeCounts">Сюда записываются количества объектов для каждого типа, которые были удалены вызовом <see cref="Finalize()"/></param>
    public static void GetRegisteredObjectCounts(out string[] objTypes, out int[] counts, out long[] disposeCounts, out long[] finalizeCounts)
    {
#if DEBUG
      lock (ObjectTypes)
      {
        objTypes = new string[ObjectTypes.Count];
        counts = new int[ObjectTypes.Count];
        disposeCounts = new long[ObjectTypes.Count];
        finalizeCounts = new long[ObjectTypes.Count];
        int cnt = 0;
        foreach (KeyValuePair<string, DisposableObjList> Pair in ObjectTypes)
        {
          objTypes[cnt] = Pair.Key;
          counts[cnt] = Pair.Value.Count;
          disposeCounts[cnt] = Pair.Value.DisposeCount;
          finalizeCounts[cnt] = Pair.Value.FinalizeCount;
          cnt++;
        }
      }
#else
      objTypes = EmptyArray<string>.Empty;
      counts = EmptyArray<Int32>.Empty;
      disposeCounts = EmptyArray<Int64>.Empty;
      finalizeCounts = EmptyArray<Int64>.Empty;
#endif
    }

    /// <summary>
    /// Получить статистику по объектам.
    /// Упрощенная версия, которая возвращает только действующие объекты.
    /// В конфигурации Release возвращает пустые массивы.
    /// </summary>
    /// <param name="objTypes">Сюда записываются типы объектов</param>
    /// <param name="counts">Сюда записываются количества объектов для каждого типа</param>
    public static void GetRegisteredObjectCounts(out string[] objTypes, out int[] counts)
    {
#if DEBUG
      lock (ObjectTypes)
      {
        objTypes = new string[ObjectTypes.Count];
        counts = new int[ObjectTypes.Count];
        int cnt = 0;
        foreach (KeyValuePair<string, DisposableObjList> Pair in ObjectTypes)
        {
          objTypes[cnt] = Pair.Key;
          counts[cnt] = Pair.Value.Count;
          cnt++;
        }
      }
#else
      objTypes = EmptyArray<string>.Empty;
      counts = EmptyArray<Int32>.Empty;
#endif
    }

    /// <summary>
    /// Получить массив зарегистрированных объектов заданного типа.
    /// В конфигурации Release возвращает пустой массив.
    /// </summary>
    /// <param name="typeName">Имя типа объекта <see cref="Type.ToString()"/></param>
    /// <returns>Объекты</returns>
    public static object[] GetRegisteredObjects(string typeName)
    {
#if DEBUG
      object[] res;
      lock (ObjectTypes)
      {
        DisposableObjList list;
        if (ObjectTypes.TryGetValue(typeName, out list))
          res = list.ToArray();
        else
          res = new object[0];
      }
      return res;
#else
      return new object[0];
#endif
    }

    /// <summary>
    /// true, если доступна отладочная информация об объектах
    /// </summary>
    public static bool DebugInformationAvailable
    {
      get
      {
#if DEBUG
        return true;
#else
        return false;
#endif
      }
    }

    /// <summary>
    /// Получение таблицы данных, содержащей статистику по объектам.
    /// Возвращает пустую таблицу, если информация об объектах недоступна (в конфигурации Release).
    /// </summary>
    /// <param name="includeZeros">Если true, то в таблицу включаются пустые строки для типов объектов, которые
    /// создавались, но были все разрущены</param>
    /// <returns>Таблица, содержащая поля "TypeName" и "Count"</returns>
    public static DataTable GetRegisteredObjectCountsTable(bool includeZeros)
    {
      DataTable table = new DataTable("ObjectTypes");
      table.Columns.Add("TypeName", typeof(string));
      table.Columns.Add("Count", typeof(int));
      table.Columns.Add("Disposed", typeof(long));
      table.Columns.Add("Finalized", typeof(long));

#if DEBUG

      string[] objTypes;
      int[] counts;
      long[] disposed;
      long[] finalized;
      GetRegisteredObjectCounts(out objTypes, out counts, out disposed, out finalized);
      for (int i = 0; i < objTypes.Length; i++)
      {
        if (!includeZeros)
        {
          if (counts[i] == 0)
            continue;
        }
        DataRow row = table.NewRow();
        row[0] = objTypes[i];
        if (counts[i] > 0)
          row[1] = counts[i];
        if (disposed[i] > 0)
          row[2] = disposed[i];
        if (finalized[i] > 0)
          row[3] = finalized[i];

        table.Rows.Add(row);
      }

#endif

      return table;
    }

    /// <summary>
    /// Регистрирует тип объекта для вывода отладочной информации по каждому объекту.
    /// Этот метод ничего не делает в конфигурации Release.
    /// </summary>
    /// <param name="type">Класс, производный от <see cref="DisposableObject"/> или <see cref="SimpleDisposableObject"/>.
    /// Также можно использовать другие классы, реализующие <see cref="IDisposable"/>, если они вызывают <see cref="RegisterDisposableObject(object)"/> и <see cref="UnregisterDisposableObject(object, bool)"/>.</param>
    public static void RegisterLogoutType(Type type)
    {
#if DEBUG
      if (type == null)
        throw new ArgumentNullException("type");
      // Не проверяем. Может быть MarshalByRefDisposableObject
      //if (!t.IsSubclassOf(typeof(DisposableObject)))
      //  throw new ArgumentException("Тип должен быть производным от DisposableObject");
      lock (_LogoutTypes)
      {
        if (!_LogoutTypes.ContainsKey(type.ToString()))
          _LogoutTypes.Add(type.ToString(), null);
      }
#endif
    }

#if DEBUG

    /// <summary>
    /// Типы объектов, зарегистрированные для вывода отладочной информации.
    /// Ключ - Type.ToString(), значение - null
    /// </summary>
    private static readonly Dictionary<string, object> _LogoutTypes = new Dictionary<string, object>();

    private class DebugItem : IComparable<DebugItem>
    {
      #region Поля

      public string DocType;
      public int Count;
      public long Disposed;
      public long Finalized;

      #endregion

      #region ToString()

      #endregion

      #region IComparable<DebugItem> Members

      public int CompareTo(DebugItem other)
      {
        //return DocType.CompareTo(other.DocType);
        return String.Compare(DocType, other.DocType, StringComparison.Ordinal);
      }

      #endregion
    }

    internal static void LogoutInfo(FreeLibSet.Logging.LogoutInfoNeededEventArgs args)
    {
      string[] objTypes;
      int[] counts;
      long[] disposed;
      long[] finalized;
      GetRegisteredObjectCounts(out objTypes, out counts, out disposed, out finalized);

      int maxObjTypeLen = 30;
      for (int i = 0; i < objTypes.Length; i++)
        maxObjTypeLen = Math.Max(objTypes[i].Length, maxObjTypeLen);

      List<DebugItem> lst1 = new List<DebugItem>(); // Существующие объекты
      List<DebugItem> lst2 = new List<DebugItem>(); // Удаленные объекты
      for (int i = 0; i < objTypes.Length; i++)
      {
        DebugItem Item = new DebugItem();
        Item.DocType = objTypes[i];
        Item.Count = counts[i];
        Item.Disposed = disposed[i];
        Item.Finalized = finalized[i];

        if (counts[i] > 0)
          lst1.Add(Item);
        else
          lst2.Add(Item);
      }

      lst1.Sort();
      lst2.Sort();

      args.WriteLine("Type".PadRight(maxObjTypeLen) + ": Count Disposed Finalized");
      args.WriteLine();
      DoLogoutList(args, lst1, maxObjTypeLen);
      if (lst2.Count > 0)
      {
        args.WriteLine();
        args.WriteLine("Finalised:");
        DoLogoutList(args, lst2, maxObjTypeLen);
      }
    }

    private static void DoLogoutList(FreeLibSet.Logging.LogoutInfoNeededEventArgs args, List<DebugItem> lst, int maxObjTypeLen)
    {
      for (int i = 0; i < lst.Count; i++)
      {
        // Убрано 01.06.2017, т.к. теперь можно видеть количество удаленных объектов
        //if (Counts[i] == 0)
        //  continue;

        //Args.WritePair(DocTypes[i], Counts[i].ToString());
        args.WriteLine(lst[i].DocType.PadRight(maxObjTypeLen) + ": " +
          lst[i].Count.ToString().PadLeft(5) + " " +
          lst[i].Disposed.ToString().PadLeft(8) + " " +
          lst[i].Finalized.ToString().PadLeft(9));

        bool isLogType;
        lock (_LogoutTypes)
        {
          isLogType = _LogoutTypes.ContainsKey(lst[i].DocType);
        }

        if (isLogType)
        {
          object[] a = GetRegisteredObjects(lst[i].DocType);

          int IndentLevel = args.IndentLevel;
          try
          {
            args.IndentLevel++;
            FreeLibSet.Logging.LogoutTools.LogoutObject(args, a);
          }
          finally
          {
            args.IndentLevel = IndentLevel;
          }
        }
      }
    }

#endif

    #endregion
  }

  /// <summary>
  /// Реализация интерфейса <see cref="IDisposable"/>. 
  /// Версия <see cref="DisposableObject"/> без деструктора. 
  /// Этот класс следует использовать в качестве базового, если в классе нет unmanaged-ресурсов,
  /// но требуется единообразная поддержка паттерна <see cref="IDisposable"/>.
  /// </summary>
  public class SimpleDisposableObject : IDisposableObject
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект.
    /// В отладочном режиме регистрирует объект в списке.
    /// </summary>
    public SimpleDisposableObject()
    {
      _IsDisposed = false;
#if DEBUG
      DisposableObject.RegisterDisposableObject(this);
#endif
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Этот метод должен быть вызван по окончании использования объекта, как положено при использовании <see cref="IDisposable"/>-объекта.
    /// Вызывает виртуальный метод <see cref="Dispose(bool)"/> с аргументом true.
    /// Повторный вызов метода не выполняет никиких действий.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;
      Dispose(true);
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Этот метод должен быть переопределен.
    /// Базовый метод обязательно должен быть вызван, обычно после выполнения собственных действий.
    /// Устанавливает свойство <see cref="IsDisposed"/> равным true. В отладочном режиме убирает объект из списка.
    /// </summary>
    /// <param name="disposing">Всегда true</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_IsDisposed)
      {
#if DEBUG
        DisposableObject.UnregisterDisposableObject(this, disposing);
#endif
        _IsDisposed = true;
      }
    }

    /// <summary>
    /// true, если метод <see cref="Dispose()"/> уже вызывался.
    /// </summary>
    public bool IsDisposed { get { return _IsDisposed; } }
    private bool _IsDisposed;

    /// <summary>
    /// Проверить, что объект не был разрушен. Иначе выбросить исключение <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void CheckNotDisposed()
    {
      if (IsDisposed)
        // Нельзя использовать this.ToString(), т.к. этот метод может сам требовать
        // неразрушенных ресурсов
        throw new ObjectDisposedException(GetType().ToString());
    }

    /// <summary>
    /// Текстовое представление.
    /// Выводит тип объекта с возможным добавлением " (Disposed)"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string s = base.ToString();
      if (IsDisposed)
        s += " (Disposed)";
      return s;
    }

    #endregion
  }

  /// <summary>
  /// Счетчик блокировок для объекта, поддерживающего интерфейс <see cref="IDisposable"/>.
  /// Когда счетчик становится равным нулю, вызывается метод <see cref="IDisposable.Dispose()"/> основного объекта.
  /// </summary>
  /// <typeparam name="T">Тип объекта</typeparam>
  public class DisposableRefCounter<T>
    where T : IDisposable
  {
    #region Конструктор

    /// <summary>
    /// Создает счетчик и устаналивает начальное значение счетчика равным 0
    /// </summary>
    /// <param name="target">Объект, для которого подсчитываются ссылки. Не может быть null</param>
    public DisposableRefCounter(T target)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      _Target = target;
      _LockCount = 0;
    }

    #endregion

    #region Основной объект

    /// <summary>
    /// Объект, для которого подсчитываются ссылки.
    /// Задается в конструкторе.
    /// Если для объекта был вызван <see cref="IDisposable.Dispose()"/>, свойство возвращает null.
    /// Если бы ссылка на объект удерживалась после разрушения, это бы не дало освободить память сборщиком мусора.
    /// </summary>
    public T Target { get { return _Target; } }
    private T _Target;

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Target == null)
        return "Disposed";
      else
        return "LockCount=" + _LockCount.ToString() + ", Target=" + Target.ToString();
    }

    /// <summary>
    /// Выбрасывает исключение <see cref="ObjectDisposedException"/>, если для объекта уже был вызван метод <see cref="IDisposable.Dispose()"/>.
    /// </summary>
    public void CheckNotDisposed()
    {
      if (_Target == null)
        throw new ObjectDisposedException("Target");
    }

    #endregion

    #region Счетчик

    /// <summary>
    /// Текущее значение счетчика
    /// </summary>
    public int LockCount { get { return _LockCount; } }
    private int _LockCount;

#if XXXX

    // ЭТО вызывает ошибку компилятора (с вылетанием) при компиляции использующего
    // кода. Обойдемся без оператора

    public static DisposableRefCounter<T> operator ++(DisposableRefCounter<T> refCounter)
    {
      refCounter.CheckNotDisposed();
      refCounter._LockCount++;
      return refCounter;
    }

    public static DisposableRefCounter<T> operator --(DisposableRefCounter<T> refCounter)
    {
      refCounter.CheckNotDisposed();
      refCounter._LockCount--;
      if (refCounter._LockCount == 0)
      {
        refCounter._Target.Dispose();
        refCounter._Target = default(T);
      }
      return refCounter;
    }
#endif

    /// <summary>
    /// Увеличивает счетчик ссылок
    /// </summary>
    public void AddRef()
    {
      CheckNotDisposed();
      _LockCount++;
    }

    /// <summary>
    /// Уменьшает счетчик ссылок.
    /// Если он достигает 0, для <see cref="Target"/> вызывается <see cref="IDisposable.Dispose()"/>.
    /// </summary>
    public void Release()
    {
      CheckNotDisposed();
      _LockCount--;
      if (_LockCount == 0)
      {
        _Target.Dispose();
        _Target = default(T);
      }
    }

    #endregion
  }

  /// <summary>
  /// Реализация интерфейса <see cref="IDisposable"/> для объектов, которые требуется передавать по ссылке через границы
  /// домена приложения.
  /// Реализует "самоспонсирование" для Net Remoting. Эту возможность можно отключить.
  /// </summary>
  public class MarshalByRefDisposableObject : MarshalByRefObject, IDisposableObject
#if !NET
  , System.Runtime.Remoting.Lifetime.ISponsor
  #endif
  {
    #region Конструктор и деструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public MarshalByRefDisposableObject()
    {
      _IsDisposed = false;
#if DEBUG
      DisposableObject.RegisterDisposableObject(this);
#endif

#if !NET
      _RenewOnSponsorTime = System.Runtime.Remoting.Lifetime.LifetimeServices.RenewOnCallTime;
#endif
    }

    /// <summary>
    /// Вызывает <see cref="Dispose(bool)"/> c аргументом false, если не было вызова <see cref="Dispose()"/>.
    /// </summary>
    ~MarshalByRefDisposableObject()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Реализация интерфейса <see cref="IDisposable"/>.
    /// Вызывает <see cref="Dispose(bool)"/> с аргументом true и подавляет вызов деструктора.
    /// Повторные вызовы отбрасываются.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Вызывается при завершении объекта.
    /// </summary>
    /// <param name="disposing">true, если вызван метод <see cref="Dispose()"/>, false, если вызван деструктор.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_IsDisposed)
      {
#if DEBUG
        DisposableObject.UnregisterDisposableObject(this, disposing);
#endif
        _IsDisposed = true;
      }
    }

    /// <summary>
    /// true, если метод <see cref="Dispose()"/> уже вызывался.
    /// </summary>
    public bool IsDisposed { get { return _IsDisposed; } }
    private bool _IsDisposed;

    /// <summary>
    /// Проверить, что объект не был разрушен. Иначе выкинуть исключение.
    /// </summary>
    public void CheckNotDisposed()
    {
      if (IsDisposed)
        // Нельзя использовать this.ToString(), т.к. этот метод может сам требовать
        // неразрушенных ресурсов
        throw new ObjectDisposedException(GetType().ToString());
    }

    /// <summary>
    /// Малоинформативное текстовое представление в виде "GetType() (Disposed)"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = base.ToString();
      if (IsDisposed)
        s += " (Disposed)";
      return s;
    }

    #endregion

    #region Самоспонсирование

#if !NET
    /// <summary>
    /// Если свойство установить в true, то объект удаленного доступа будет жить вечно.
    /// Свойство следует использовать в исключительных случаях.
    /// </summary>
    public bool EternalLife { get { return _EternalLife; } set { _EternalLife = value; } }
    private bool _EternalLife;

    /// <summary>
    /// Присоединяет к лицензии текущий объект в качестве спонсора, чтобы объект мог заниматься "самоспонированием".
    /// Если свойство <see cref="EternalLife"/>=true, то возвращается null
    /// </summary>
    /// <returns>Объект, реализующий <see cref="System.Runtime.Remoting.Lifetime.ILease"/></returns>
    public override object InitializeLifetimeService()
    {
      if (EternalLife)
        return null;

      System.Runtime.Remoting.Lifetime.ILease lease = (System.Runtime.Remoting.Lifetime.ILease)base.InitializeLifetimeService();
      if (lease.CurrentState == System.Runtime.Remoting.Lifetime.LeaseState.Initial)
      {
        lease.Register(this);
      }
      return lease;
    }

    TimeSpan System.Runtime.Remoting.Lifetime.ISponsor.Renewal(System.Runtime.Remoting.Lifetime.ILease lease)
    {
      if (this.IsDisposed)
        return TimeSpan.Zero;

      return OnRenewal(lease);
    }

    /// <summary>
    /// Время, на которое продляется лицензия при спонсировании в методе <see cref="OnRenewal(System.Runtime.Remoting.Lifetime.ILease)"/>.
    /// По умолчанию равно значению <see cref="System.Runtime.Remoting.Lifetime.LifetimeServices.RenewOnCallTime"/>, действующиему в момент создания объекта.
    /// Если установить свойство в <see cref="TimeSpan.Zero"/>, то лицензия продляться не будет.
    /// Для динамического управления продлением лицензии следует переопределить метод <see cref="OnRenewal(System.Runtime.Remoting.Lifetime.ILease)"/>.
    /// </summary>
    public TimeSpan RenewOnSponsorTime { get { return _RenewOnSponsorTime; } set { _RenewOnSponsorTime = value; } }
    private TimeSpan _RenewOnSponsorTime;

    /// <summary>
    /// Продление лицензии объекта. Возвращает значение <see cref="RenewOnSponsorTime"/>.
    /// Переопределенный метод может вернуть другой интервал времени или отказаться от продления лицензии,
    /// вернув <see cref="TimeSpan.Zero"/>.
    /// Метод вызывается, если <see cref="IsDisposed"/>=false.
    /// </summary>
    /// <param name="lease">Лицензия, которую нужно продлить</param>
    /// <returns>Время, на которое надо продлить лицензию</returns>
    protected virtual TimeSpan OnRenewal(System.Runtime.Remoting.Lifetime.ILease lease)
    {
      return RenewOnSponsorTime;
    }
#endif
    #endregion
  }

  /// <summary>
  /// Реализация статических ссылок на <see cref="IDisposable"/>-объект, который должен быть разрушен при завершении работы 
  /// приложения или выгрузки домена.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct AutoDisposeReference<T> : IDisposable
    where T : IDisposable
  {
    // Шаблон применения
    //
    // Класс, объект которого должен быть в одном экземпляре в течение жизни домена приложения
    // class MyClass:IDisposable
    // {
    // }
    //
    // Статическая ссылка на объект
    // public static readonly AutoDisposeReference<MyClass> TheObj=new AutoDisposeReference<MyClass>(new MyClass());

    #region Конструктор

    /// <summary>
    /// Конструктор ссылки. Обычно вызывается из статического инициализатора
    /// </summary>
    /// <param name="target"></param>
    public AutoDisposeReference(T target)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      _Target = target;

      AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
    }

    #endregion

    #region Обработчики

    void CurrentDomain_DomainUnload(object Sender, EventArgs Args)
    {
      FreeTarget();
    }


    void CurrentDomain_ProcessExit(object sender, EventArgs args)
    {
      FreeTarget();
    }


    private void FreeTarget()
    {
      if (_Target != null)
      {
        _Target.Dispose();
        _Target = default(T); // null

        AppDomain.CurrentDomain.DomainUnload -= new EventHandler(CurrentDomain_DomainUnload);
        AppDomain.CurrentDomain.ProcessExit -= new EventHandler(CurrentDomain_ProcessExit);
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылка на объект, переданный в конструкторе.
    /// После разрушения объекта получает значение null.
    /// </summary>
    public T Target { get { return _Target; } }
    private T _Target;

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Вызывает <see cref="IDisposable.Dispose()"/> для основного объекта, если он еще не был разрушен.
    /// </summary>
    public void Dispose()
    {
      FreeTarget();
    }

    #endregion
  }

#if !NET
  /// <summary>
  /// Объект, реализующий интерфейс <see cref="System.Runtime.Remoting.Lifetime.ISponsor"/>.
  /// Продляет лицензию пока существует основной объект <see cref="DisposableObject"/> (или другого класса, реализующего <see cref="IDisposableObject"/>).
  /// </summary>
  public class DisposableObjectSponsor : MarshalByRefObject, System.Runtime.Remoting.Lifetime.ISponsor
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект-спонсор, отслеживающий заданный объект.
    /// Лицензия продляется на заданное время.
    /// </summary>
    /// <param name="mainObject">Основной объект, за которым выполняется слежение. Не может быть null.</param>
    /// <param name="renewalTime">Время, на которое продляется лицензия</param>
    public DisposableObjectSponsor(IDisposableObject mainObject, TimeSpan renewalTime)
    {
      if (mainObject == null)
        throw new ArgumentNullException("mainObject");
      _MainObject = mainObject;
      _RenewalTime = renewalTime;
    }


    /// <summary>
    /// Создает объект-спонсор, отслеживающий заданный объект.
    /// Лицензия продляется на 2 минуты (значение по умолчанию для <see cref="System.Runtime.Remoting.Lifetime.ClientSponsor.RenewalTime"/>).
    /// </summary>
    /// <param name="mainObject">Основной объект, за которым выполняется слежение</param>
    public DisposableObjectSponsor(DisposableObject mainObject)
      : this(mainObject,
      TimeSpan.FromMinutes(2) // Значение по умолчанию для ClientSponsor
      )
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной объект, за которым выполняется слежение.
    /// Задается в конструкторе.
    /// </summary>
    public IDisposableObject MainObject { get { return _MainObject; } }
    private readonly IDisposableObject _MainObject;

    /// <summary>
    /// Время, на которое продляется лицензия.
    /// Задается в конструкторе
    /// </summary>
    public TimeSpan RenewalTime { get { return _RenewalTime; } }
    private readonly TimeSpan _RenewalTime;

    #endregion

    #region ISponsor Members

    /// <summary>
    /// Реализация интерфейса <see cref="System.Runtime.Remoting.Lifetime.ISponsor"/>.
    /// Возвращает <see cref="RenewalTime"/> или 0, если <see cref="MainObject"/>.IsDisposed=true.
    /// </summary>
    /// <param name="lease">Лицензия объекта</param>
    /// <returns>Время, на которое продляется лицензия</returns>
    public TimeSpan Renewal(System.Runtime.Remoting.Lifetime.ILease lease)
    {
      if (_MainObject.IsDisposed)
        return TimeSpan.Zero;
      else
        return _RenewalTime;
    }

    #endregion
  }
  #endif
}
