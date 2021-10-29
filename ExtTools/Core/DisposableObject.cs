using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using FreeLibSet.Collections;

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

namespace FreeLibSet.Core
{
  /// <summary>
  /// Расширение интерфейса IDisposable свойством IsDisposed для проверки разрушения объекта.
  /// Реализуется классами DisposableObject, SimpleDisposableObject и MarshalByRefDisposableObject
  /// </summary>
  public interface IDisposableObject : IDisposable
  {
    /// <summary>
    /// true, если метод Dispose() уже вызывался
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Проверить, что объект не был разрушен. Иначе выкинуть исключение
    /// </summary>
    void CheckNotDisposed();
}

  /// <summary>
  /// Реализация интерфейса IDisposable. Используйте этот класс в качестве
  /// предка, если не требуется другой базовый класс.
  /// В противном случае используйте собственную реализацию IDisposable и 
  /// вызывайте статические методы для отладки объектов
  /// </summary>
  public class DisposableObject : IDisposableObject
  {
    #region Конструктор и деструктор

    /// <summary>
    /// Создает новый объект.
    /// В отладочном режиме регистрирует объект в списке
    /// </summary>
    public DisposableObject()
    {
      _IsDisposed = false;
#if DEBUG
      RegisterDisposableObject(this);
#endif
    }

    /// <summary>
    /// Деструктор все равно нужен, даже если для объекта всегда используется явное завершение с помощью Dispose().
    /// Если в конструкторе производного объекта возникает исключение, деструктор будет вызван.
    /// Если был правильный вызов метода Dispose(), то вызов деструктора подавляется с помощью GC.SuppressFinalize()
    /// </summary>
    ~DisposableObject()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Этот метод должен быть вызван по окончании использования объекта, как положено при использовании IDisposable-объекта.
    /// Вызывает виртуальный метод Dispose() с аргументом true и предотвращает вызов деструктора.
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
    /// Устанавливает свойство IsDisposed равным true. В отладочном режиме убирает объект из списка.
    /// </summary>
    /// <param name="disposing">true, если метод вызван в явном виде из public-версии Dispose().
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
    /// true, если метод Dispose() уже вызывался
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
    /// Вызывается из конструктора DisposableObject и других классов, желающих отслеживать использование
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
        DisposableObjList ObjList = GetObjList(obj);
        ObjList.Add(obj);
      }
#endif
    }

    /// <summary>
    /// Отменить регистрирацию объекта в списке для отладочных целей.
    /// Вызывается из метода DisposableObject.Dispooe() и методов Dispose() других классов, 
    /// желающих отслеживать использование объектов.
    /// В конфигурации RELEASE никаких действий не выполняется.
    /// Обычно следкет использовать перегрузку, принимающую дополнительный аргумент Disposing
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
    /// Вызывается из метода DisposableObject.Dispose() и методов Dispose() других классов, 
    /// желающих отслеживать использование объектов.
    /// В конфигурации RELEASE никаких действий не выполняется.
    /// </summary>
    /// <param name="obj">Зарегистрированный объект</param>
    /// <param name="disposing">Значение аргумента disposing, полученное методом Dispose()</param>
    public static void UnregisterDisposableObject(object obj, bool disposing)
    {
#if DEBUG
      if (obj == null || ObjectTypes == null)
        return;
      lock (ObjectTypes)
      {
        DisposableObjList ObjList = GetObjList(obj);
        ObjList.Remove(obj);
        if (disposing)
          ObjList.DisposeCount++;
        else
          ObjList.FinalizeCount++;
      }
#endif
    }

#if DEBUG
    /// <summary>
    /// Получить массив объектов, где хранятся объекты нужного типа
    /// На момент вызова коллекция должна быть заблокирована
    /// </summary>
    /// <param name="obj">Объект, который будет добавляться или удаляться</param>
    /// <returns>Массив объектов</returns>
    private static DisposableObjList GetObjList(object obj)
    {
      string TypeName = obj.GetType().ToString();
      return GetObjListForType(TypeName);
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
    /// Эта версия возвращает количество существующих объектов и количество удаленных объектов
    /// </summary>
    /// <param name="objTypes">Сюда записываются типы объектов</param>
    /// <param name="counts">Сюда записываются количества объектов для каждого типа</param>
    /// <param name="disposeCounts">Сюда записываются количества объектов для каждого типа, которые были удалены вызовом Dispose()</param>
    /// <param name="finalizeCounts">Сюда записываются количества объектов для каждого типа, которые были удалены вызовом Finalize()</param>
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
      objTypes = DataTools.EmptyStrings;
      counts = DataTools.EmptyInts;
      disposeCounts = DataTools.EmptyInt64s;
      finalizeCounts = DataTools.EmptyInt64s;
#endif
    }

    /// <summary>
    /// Получить статистику по объектам.
    /// Упрощенная версия, которая возвращает только действующие объекты
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
      objTypes = DataTools.EmptyStrings;
      counts = DataTools.EmptyInts;
#endif
    }


    /// <summary>
    /// Получить массив зарегистрированных объектов
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns>Объекты</returns>
    public static object[] GetRegisteredObjects(string typeName)
    {
#if DEBUG
      object[] res;
      lock (ObjectTypes)
      {
        DisposableObjList List;
        if (ObjectTypes.TryGetValue(typeName, out List))
          res = List.ToArray();
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
    /// Получение таблицы данных, содержащей статистику по объектам
    /// Возвращает пустую таблицу, если информация об объектах недоступна (DEBUG не определено)
    /// </summary>
    /// <param name="includeZeros">Если true, то в таблицу включаются пустые строки для типов объектов, которые
    /// создавались, но были все разрущены</param>
    /// <returns>Таблица, содержащая поля "TypeName" и "Count"</returns>
    public static DataTable GetRegisteredObjectCountsTable(bool includeZeros)
    {
      DataTable Table = new DataTable("ObjectTypes");
      Table.Columns.Add("TypeName", typeof(string));
      Table.Columns.Add("Count", typeof(int));
      Table.Columns.Add("Disposed", typeof(long));
      Table.Columns.Add("Finalized", typeof(long));

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
        DataRow Row = Table.NewRow();
        Row[0] = objTypes[i];
        if (counts[i] > 0)
          Row[1] = counts[i];
        if (disposed[i] > 0)
          Row[2] = disposed[i];
        if (finalized[i] > 0)
          Row[3] = finalized[i];

        Table.Rows.Add(Row);
      }

#endif

      return Table;
    }

    /// <summary>
    /// Регистрирует тип объекта для вывода отладочной информации по каждому объекту.
    /// Этот метод ничего не делает в режиме Release.
    /// </summary>
    /// <param name="typ">Класс, производный от DisposableObject</param>
    public static void RegisterLogoutType(Type typ)
    {
#if DEBUG
      if (typ == null)
        throw new ArgumentNullException();
      // Не проверяем. Может быть MarshalByRefDisposableObject
      //if (!t.IsSubclassOf(typeof(DisposableObject)))
      //  throw new ArgumentException("Тип должен быть производным от DisposableObject");
      lock (LogoutTypes)
      {
        if (!LogoutTypes.ContainsKey(typ.ToString()))
          LogoutTypes.Add(typ.ToString(), null);
      }
#endif
    }


#if DEBUG

    /// <summary>
    /// Типы объектов, зарегистрированные для вывода отладочной информации.
    /// Ключ - Type.ToString(), значение - null
    /// </summary>
    private static readonly Dictionary<string, object> LogoutTypes = new Dictionary<string, object>();

    private class DebugItem : IComparable<DebugItem>
    {
      #region Поля

      public string DocType;
      public int Count;
      public long Disposed;
      public long Finalized;

      #endregion

      #region ToString()

      public override string ToString()
      {
        return DocType.PadRight(54) + ": " + Count.ToString().PadLeft(5) + " " +
          Disposed.ToString().PadLeft(8) + " " + Finalized.ToString().PadLeft(9);
      }

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

      args.WriteLine("Type".PadRight(54) + ": Count Disposed Finalized");
      args.WriteLine();
      DoLogoutList(args, lst1);
      if (lst2.Count > 0)
      {
        args.WriteLine();
        args.WriteLine("Finalised:");
        DoLogoutList(args, lst2);
      }
    }

    private static void DoLogoutList(FreeLibSet.Logging.LogoutInfoNeededEventArgs args, List<DebugItem> lst)
    {
      for (int i = 0; i < lst.Count; i++)
      {
        // Убрано 01.06.2017, т.к. теперь можно видеть количество удаленных объектов
        //if (Counts[i] == 0)
        //  continue;

        //Args.WritePair(DocTypes[i], Counts[i].ToString());
        args.WriteLine(lst[i].ToString());

        bool IsLogType;
        lock (LogoutTypes)
        {
          IsLogType = LogoutTypes.ContainsKey(lst[i].DocType);
        }

        if (IsLogType)
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
  /// Реализация интерфейса IDisposable. 
  /// Версия DisposableObject без деструктора. 
  /// Этот класс следует использовать в качестве базового, если в классе нет unmanaged-ресурсов,
  /// но требуется единообразная поддержка паттерна IDisposable
  /// </summary>
  public class SimpleDisposableObject : IDisposableObject
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект.
    /// В отладочном режиме регистрирует объект в списке
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
    /// Этот метод должен быть вызван по окончании использования объекта, как положено при использовании IDisposable-объекта.
    /// Вызывает виртуальный метод Dispose() с аргументом true.
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
    /// Устанавливает свойство IsDisposed равным true. В отладочном режиме убирает объект из списка.
    /// </summary>
    /// <param name="disposing">true, если метод вызван в явном виде из public-версии Dispose().
    /// false, если метод вызван из деструктора</param>
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
    /// true, если метод Dispose() уже вызывался
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
  /// Счетчик блокировок для объекта, поддерживающего интерфейс IDisposable.
  /// Когда счетчик становится равным нулю, вызывается метод Dispose() основного
  /// объекта
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
    /// Если для объекта был вызван Dispose(), свойство возвращает null.
    /// Если бы ссылка на объект удерживалась после разрушения, это бы не дало освободить память сборщиком мусора.
    /// </summary>
    public T Target { get { return _Target; } }
    private T _Target;

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (_Target == null)
        return "Disposed";
      else
        return "LockCount=" + _LockCount.ToString() + ", Target=" + Target.ToString();
    }

    /// <summary>
    /// Выбрасывает исключение, если для объекта уже был вызван метод Dispose().
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
    /// Если он достигает 0, для Target вызывается Dispose().
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
  /// Реализация интерфейса IDisposable для объектов, которые требуется передавать по ссылке через границы
  /// домена приложения.
  /// Реализует "самоспонсирование" для Net Remoting. Эту возможность можно отключить
  /// </summary>
  public class MarshalByRefDisposableObject : MarshalByRefObject, IDisposableObject, System.Runtime.Remoting.Lifetime.ISponsor
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

      _RenewOnSponsorTime = System.Runtime.Remoting.Lifetime.LifetimeServices.RenewOnCallTime;
    }

    /// <summary>
    /// Вызывает Dispose(false), если не было вызова Dispose()
    /// </summary>
    ~MarshalByRefDisposableObject()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Реализация интерфейса IDisposable.
    /// Вызывает Dispose(true) и подавляет вызов деструктора.
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
    /// <param name="disposing">true, если вызван метод Dispose(), false, если вызван деструктор.</param>
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
    /// true, если метод Dispose() уже вызывался
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
    /// Малоинформативное текстовое представление в виде "GetType() (Disposed)"
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

    #region Самоспонсирование

    /// <summary>
    /// Если свойство установить в true, то объект удаленного доступа будет жить вечно.
    /// Свойство следует использовать в исключительных случаях
    /// </summary>
    public bool EternalLife { get { return _EternalLife; } set { _EternalLife = value; } }
    private bool _EternalLife;

    /// <summary>
    /// Присоединяет к лицензии текущий объект в качестве спонсора, чтобы объект мог заниматься "самоспонированием".
    /// Если свойство EternalLife=true, то возвращается null
    /// </summary>
    /// <returns></returns>
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
    /// Время, на которотое продляется лицензия при спонсировании в методе OnRenewal.
    /// По умолчанию равно значению LifetimeServices.RenewOnCallTime, действующиему в момент создания объекта.
    /// Если установить свойство в TimeSpan.Zero, то лицензия продляться не будет.
    /// Для динамического управления продлением лицензии следует переопределить метод OnRenewal()
    /// </summary>
    public TimeSpan RenewOnSponsorTime { get { return _RenewOnSponsorTime; } set { _RenewOnSponsorTime = value; } }
    private TimeSpan _RenewOnSponsorTime;

    /// <summary>
    /// Продление лицензии объекта. Возвращает значение RenewOnSponsorTime.
    /// Переопределенный метод может вернуть другой интервал времени или отказаться от продления лицензии,
    /// вернув TimeSpan.Zero.
    /// Метод вызывается, если IsDisposed=false, а SelfSponsoring=true
    /// </summary>
    /// <param name="lease">Лицензия, которую нужно продлить</param>
    /// <returns>Время, на которое надо продлить лицензию</returns>
    protected virtual TimeSpan OnRenewal(System.Runtime.Remoting.Lifetime.ILease lease)
    {
      return RenewOnSponsorTime;
    }

    #endregion
  }

  /// <summary>
  /// Реализация статических ссылок на Disposable-объект, который должен быть разрушен при завершении работы 
  /// приложения или выгрузки домена
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
      EHDomainUnload = null;
      EHProcessExit = null;

      EHDomainUnload = new EventHandler(CurrentDomain_DomainUnload);
      EHProcessExit = new EventHandler(CurrentDomain_ProcessExit);

      AppDomain.CurrentDomain.DomainUnload += EHDomainUnload;
      AppDomain.CurrentDomain.ProcessExit += EHProcessExit;
    }

    #endregion

    #region Обработчики

    private readonly EventHandler EHDomainUnload;

    void CurrentDomain_DomainUnload(object Sender, EventArgs Args)
    {
      FreeTarget();
    }


    private readonly EventHandler EHProcessExit;

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

        AppDomain.CurrentDomain.DomainUnload -= EHDomainUnload;
        AppDomain.CurrentDomain.ProcessExit -= EHProcessExit;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылка на объект, переданный в конструкторе.
    /// После разрушения объекта получает значение null
    /// </summary>
    public T Target { get { return _Target; } }
    private T _Target;

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Вызов Dispose вызывает Dispose() для основного объекта, если он еще не был разрушен
    /// </summary>
    public void Dispose()
    {
      FreeTarget();
    }

    #endregion
  }

  /// <summary>
  /// Объект, реализующий интерфейс ISponsor.
  /// Продляет лицензию пока существует объект DisposableObject
  /// </summary>
  public class DisposableObjectSponsor : MarshalByRefObject, System.Runtime.Remoting.Lifetime.ISponsor
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект-спонсор, отслеживающий заданный объект.
    /// Лицензия продляется на заданное время
    /// </summary>
    /// <param name="mainObject">Основной объект, за которым выполняется слежение</param>
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
    /// Лицензия продляется на 2 минуты
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
    private IDisposableObject _MainObject;

    /// <summary>
    /// Время, на которое продляется лицензия.
    /// Задается в конструкторе
    /// </summary>
    public TimeSpan RenewalTime { get { return _RenewalTime; } }
    private TimeSpan _RenewalTime;

    #endregion

    #region ISponsor Members

    /// <summary>
    /// Реализация интерфейса ISponsor.
    /// Возвращает RenewalTime или 0, если MainObject.IsDisposed=true.
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
}
