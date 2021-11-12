#define OLD_CACHE

#if OLD_CACHE
#define USE_GCCOLLECT // Если определено, то будем вызывать GC.Collect(), иначе - нет
#endif
//#define USE_CHECKMEMORYTIME // Если определено, то есть параметр статистики CheckMemoryWaitTime
//#define TRACE_CHECKMEMORY

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using FreeLibSet.IO;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using FreeLibSet.Diagnostics;
using FreeLibSet.Logging;
using FreeLibSet.Remoting;
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

/*
 * Система позволяет кэшировать данные:
 * - в памяти
 * - во временном каталоге
 * - в каталоге постоянного хранения
 * Способ хранения данных определяется при обращении к этим данным (метод GetItem).
 * 
 * Когда данных нет в буфере, создается новый объект InternalCacheItem и с помощью Factory создается
 * объект данных.
 * В зависимости от способа хранения выполняются действия:
 * - MemoryOnly, MemoryAndTempDir - данные помещаются в StrongRef
 * - TempDirOnly, PersistOnly - данные записываются в файл
 * - MemoryAndPersist - данные записываются в файл и помещаются в MemValue
 * 
 * Если при обращении данные есть в кэше, соответствующий CacheItem переносится наверх списка MRU.
 * Для типов MemoryAndTempDir и MemoryAndPersist, когда данные находятся в файле, а не в памяти,
 * после загрузки из файла они сохраняются в поле MemValue.
 * 
 * Опрос по таймеру
 * ----------------
 * По сигналу таймера вызывается метод Cache.TestMemory(). Он проверяет свойство AvailableMemoryState.
 * Если состояние Low (все плохо), начинается удаление самых старых элементов кэша. Удаление выполняется
 * блоками по 100 элементов. После каждого блока вызывается GC.Collect и AvailableMemoryState проверяется
 * еще раз. Если очищен весь кэш, а состояние так и осталось Low, никаких других действий не выполняется.
 * 
 * Если состояние Swapping, то задачей является освободить оперативную память, но не место на диске.
 * Опять же начинается просмотр блоками по 100 элементов, начиная со старых элементов. Элементы в состоянии
 * MemoryOnly удаляются. Элементы в состоянии MemoryAndTempDir записываются на диск, если это еще не было
 * сделано ранее, а ссылка из памяти удаляется. Для элементов в состоянии MemoryAndPersist удаляется
 * ссылка из памяти, файл на диске уже существует с момента создания элемента. Элементы в состояниях
 * TempDirOnly и PersistOnly пропускаются, т.к. они не занимают память. Опять же, если очистка не исправила
 * состояние, никаких других действий не предпринимаются.
 * 
 * Перехват ошибки
 * ---------------
 * Если в процессе получения элемента возникло исключение OutOfMemoryException, то выполняется вызов
 * TestMemory(). Если метод в данный момент уже выполняется в другом потоке, ожидается его завершение.
 * После вызова TestMemory() выполняется попытка еще одного вызова. Если она тоже завершилась OutOfMemoryException,
 * выполняется полная очистка кэша и выполняется третья попытка, уже без перехвата исключения
 * 
 * Удаление элементов кэша
 * -----------------------
 * Возможно две причины удаление элемента кэша.
 * 1. Явный вызов метода Clear(), означающий, что данные в кэше устарели. При этом должны быть удалены
 *    данные с диска в состоянии PersistOnly и MemoryAndPersist
 * 2. Удаление элемента в связи с переполнением кэша. В этом случае файлы PersistOnly и MemoryAndPersist
 *    удалять не нужно.
 * 
 * 24.04.2017
 * Вносимые изменения:
 * 1. Класс InternalCacheItem должен иметь поле Persistance, т.к. метод TestMemory() должен отличать состояния
 *    MemoryAndTempDir от MemoryAndPersist. Также это позволяет не проверять наличие файла на диске
 *    при удалении элемента с типом MemoryOnly
 * 2. Поле InternalCacheItem.MemValue больше не является WeakReference. Вместо этот используется обычная "жесткая"
 *    ссылка. Если ссылка содержит null, значит для типов MemoryAndTempDir и MemoryAndPersist, данные 
 *    хранятся в файле, а не в памяти.
 * 3. Изменяется способ удаления файлов, связанных с CacheItem 
 * 
 * 28.08.2020 Версия кэша
 * ----------------------
 * Версия, в первую очередь, нужна при использовании постоянных файлов, когда задан PersistDir. 
 * Например, классификатор адресов ФИАС может обновляться, при этом требуется очищать кэш.
 * Версия, с точки зрения Cache, является строкой. Проверяется точное соответствие версии, а не "больше" или "меньше".
 * Прикладной код должен вызвать Cache.SetVersion() в начале работы и при возможном обнаружении изменений.
 * SetVersion() получает тип объекта и необязательные ключи. Версия распространяется на ветвь кэша с заданными ключами и на дочерние ветви.
 * Имена файлов данных в PerisistDir (но не каталоги) включают в себя версию. Могут быть 2 работающих копии программы, которые используют
 * общий PersistDir. При этом не гарантируется, что обновление версии (классификатора адресов) произойдет строго одновременно. Каждая копия
 * должна пользоваться файлами своей версии.
 * В постоянном каталоге PersistDir файлы данных имеют имена: PersistDir\[Ключ\][Ключ\]Ключ#Версия.bin
 * Также есть специальные файлы version.txt, которые используются при первом вызове SetVersion(), чтобы обнаружить изменение версии без
 * необходимости просмотра всех файлов ветви кэша.
 */

#if OLD_CACHE

namespace FreeLibSet.Caching
{
  /*
   * Буферизация объектов в оперативной памяти и/или в файлах на диске
   */

  #region Перечисление CachePersistance

  /// <summary>
  /// Способ хранение элемента кэша
  /// </summary>
  public enum CachePersistance
  {
    /// <summary>
    /// Элемент хранится только в оперативной памяти.
    /// В случае нехватки памяти элемент удаляется и требуется повторное создание данных
    /// </summary>
    MemoryOnly = 1,

    /// <summary>
    /// Копия элемента хранится во временном каталоге в течение сеанса работы приложения.
    /// Элемент должен быть сериализуемым
    /// </summary>
    MemoryAndTempDir = 3,

    /// <summary>
    /// Даннные хранятся только во временном каталоге.
    /// Этот вариант следует использовать для элементов большого объема, которые нужны сравнительно редко.
    /// Элемент должен быть сериализуемым
    /// </summary>
    TempDirOnly = 2,

    /// <summary>
    /// Данные хранятся в оперативной памяти, копия - в специально отведенном каталоге.
    /// Данные сохраняются между сеансами работы.
    /// Элемент должен быть сериализуемым.
    /// </summary>
    MemoryAndPersist = 5,

    /// <summary>
    /// Данные хранятся в специально отведенном каталоге, но не в оперативной памяти.
    /// Данные сохраняются между сеансами работы.
    /// Элемент должен быть сериализуемым.
    /// </summary>
    PersistOnly = 4
  }

  #endregion

  /// <summary>
  /// Интерфейс, который должен реализовываться в прикладном коде для создания данных кэша
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ICacheFactory<T>
    where T : class
  {
    /// <summary>
    /// Создает новый элемент кэша
    /// </summary>
    /// <param name="keys">Ключи</param>
    /// <returns>Элемент кэша</returns>
    T CreateCacheItem(string[] keys);
  }

  /// <summary>
  /// Параметры кэша
  /// </summary>
  public class CacheParams : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект параметра со значениями по умолчанию
    /// </summary>
    internal CacheParams()
    {
      _Capacity = 10000;
      _CheckMemoryInterval = 60000;
      _CriticalMemoryLoad = 70;
      _ClearCacheBlockSize = 100;
      //_LockTimeout = 60000;
      _LockTimeout = System.Threading.Timeout.Infinite;
    }

    #endregion

    #region Параметры

    /// <summary>
    /// Максимальное число элементов, которые могут хранится в кэше.
    /// По умолчанию - 10000 элементов
    /// </summary>
    public int Capacity
    {
      get { return _Capacity; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", value, "Значение не можеть быть меньше 1");

        lock (this)
        {
          CheckNotReadOnly();
          _Capacity = value;
        }
      }
    }
    private int _Capacity;

    /// <summary>
    /// Каталог, используемый для хранения элементов со способом хранения MemoryAndPersist и PeristOnly.
    /// 
    /// Внимание! Каталог не должен использоваться ни для каких других целей, кроме хранения кэша.
    /// Метод Cache.Clear() рекурсивно очищает все файлы в каталоге.
    /// Будьте внимательны при установке свойства во избежание утери файлов.
    /// 
    /// По умолчанию каталог не задан, использовать указанные способы хранения нельзя.
    /// </summary>
    public AbsPath PersistDir
    {
      get { return _PersistDir; }
      set
      {
        lock (this)
        {
          CheckNotReadOnly();
          _PersistDir = value;
        }
      }
    }
    private AbsPath _PersistDir;

    /// <summary>
    /// Путь ко временному каталогу
    /// Свойство автоматически устанавливается при инициализации кэша
    /// </summary>
    public AbsPath TempDir
    {
      get { return _TempDir; }
      internal set
      {
        _TempDir = value;
      }
    }
    private AbsPath _TempDir;

    /// <summary>
    /// Периодичность опроса наличной памяти по таймеру в миллисекундах.
    /// По умолчанию - 60000 (1 раз в минуту).
    /// Значение Timeout.Infinite отключает использование таймера
    /// </summary>
    public int CheckMemoryInterval
    {
      get { return _CheckMemoryInterval; }
      set
      {
        CheckNotReadOnly();
        _CheckMemoryInterval = value;
      }
    }
    private int _CheckMemoryInterval;

    /// <summary>
    /// Процент занятой физической памяти, при котором начинается запись в файл элементом со способом
    /// хранения MemoryAndTempDir и освобождение памяти для элементов MemoryOnly и MemoryAndPersist.
    /// По умолчанию - 70%
    /// </summary>
    public int CriticalMemoryLoad
    {
      get { return _CriticalMemoryLoad; }
      set
      {
        CheckNotReadOnly();
        if (value < 0 || value > 100)
          throw new ArgumentOutOfRangeException();
        _CriticalMemoryLoad = value;
      }
    }
    private int _CriticalMemoryLoad;

    /// <summary>
    /// Количество элементов, освобождаемых за один раз, когда обнаруживается нехватка памяти.
    /// По умолчанию - 100 элементов
    /// </summary>
    public int ClearCacheBlockSize
    {
      get { return _ClearCacheBlockSize; }
      set
      {
        CheckNotReadOnly();
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _ClearCacheBlockSize = value;
      }
    }
    private int _ClearCacheBlockSize;

    /// <summary>
    /// <para>Максимальный интервал времени в миллисекундах, на который может устанавливаться блокировка для получения
    /// страницы кэша. Если за указанный интервал объект не освобожден другим потоком, то генерируется исключение
    /// LockTimeoutException.</para>
    /// <para>Значение System.Threading.Timeout.Infinite (по умолчанию) означает отсутствие таймаута и приведет к вечному зависанию программы.
    /// В любом случае, появление исключения LockTimeoutException означает ошибку в приложении, которую необходимо устранить.</para>
    /// <para>Значение свойства используется для перегрузок методов Cache.GetItem(), GetItemIfExists() и SetItem(),
    /// в которых нет аргумента lockTimeout</para>
    /// </summary>
    public int LockTimeout
    {
      get { return _LockTimeout; }
      set
      {
        CheckNotReadOnly();
        CheckLockTimeOutValue(value);
        _LockTimeout = value;
      }
    }
    private int _LockTimeout;

    internal static void CheckLockTimeOutValue(int lockTimeout)
    {
      if (lockTimeout < 1 && lockTimeout != System.Threading.Timeout.Infinite)
        throw new ArgumentOutOfRangeException("lockTimeout", lockTimeout, "Значение должно быть больше 0 или равна System.Threading.Timeout.Infinite");
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если параметры уже присоединены и их нельзя менять
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если параметры нельзя менять
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      lock (this)
      {
        _IsReadOnly = true;
      }
    }

    #endregion
  }

  /// <summary>
  /// Результат, возвращаемый методом Cache.SetVersion(), который нужно передавать методу SyncVersion(), если используется синхронное
  /// управление версиями для нескольких типов объектов.
  /// Большинство полей класса являются внутренними
  /// </summary>
  public sealed class CacheSetVersionResult
  {
    #region Защищенный конструктор

    internal CacheSetVersionResult(Type objType, string[] keys, string version, bool persistFilesCleared)
    {
      _ObjType = objType;
      _Keys = keys;
      _Version = version;
      _PersistFilesCleared = persistFilesCleared;
    }

    #endregion

    #region Свойства

    internal Type ObjType { get { return _ObjType; } }
    private readonly Type _ObjType;

    internal string[] Keys { get { return _Keys; } }
    private readonly string[] _Keys;

    internal string Version { get { return _Version; } }
    private readonly string _Version;

    /// <summary>
    /// Возвращает true, если были очищены файлы в постоянном каталоге, так как версия изменилась, или это был первый вызов SetVersion
    /// </summary>
    public bool PersistFilesCleared { get { return _PersistFilesCleared; } }
    private readonly bool _PersistFilesCleared;

    #endregion
  }

  /// <summary>
  /// Система буферизации сериализуемых объектов с хранением в памяти и/или на диске
  /// Все public-свойства и методы являются потокобезопасными
  /// </summary>
  public static class Cache
  {
    /// <summary>
    /// Внутренняя реализация кэша
    /// </summary>
    private class InternalCache : DisposableObject
    {
      #region Конструктор и Dispose

      public InternalCache(CacheParams cacheParams)
      {
        cacheParams.SetReadOnly();
        _Params = cacheParams;

        _TheTypeDict = new TypeDict();
        _TempDir = new TempDirectory();
        _Params.TempDir = _TempDir.Dir;

        _TypeStats = new Dictionary<Type, CacheStat>();

        if (cacheParams.CheckMemoryInterval > 0)
          _CheckMemoryTimer = new Timer(new TimerCallback(CheckMemoryTimer_Tick), null, cacheParams.CheckMemoryInterval, cacheParams.CheckMemoryInterval);

        _DelayedClearList = new List<DelayedClearInfo>();
      }

      protected override void Dispose(bool disposing)
      {
        if (_DelayedClearTimer != null)
        {
          _DelayedClearTimer.Dispose();
          _DelayedClearTimer = null;
        }
        if (_CheckMemoryTimer != null)
        {
          _CheckMemoryTimer.Dispose();
          _CheckMemoryTimer = null;
        }
        if (_TempDir != null)
        {
          _TempDir.Dispose();
          _TempDir = null;
        }

        base.Dispose(disposing);
      }

      #endregion

      #region Доступ по ключам

      /*
       * Для каждого типа буферизуемых данных используется отдельная коллекция KeyDict.
       * Доступ к конкретному объекту осуществляется по массиву ключей. Массив должен иметь не менее одного ключа
       * Следует создавать для каждой цели отдельный тип хранимых хранимых данных, чтобы ключи не путались, а
       * не использовать, например, DataSet
       *
       * Блокирование.
       * // Коллекция по типам (TheDict) блокируется на время поиска/создания корневого объекта KeyDict для выбранного
       * // типа.
       * // Метод GetInternalItem(), выполняющий поиск/создания страниц KeyDict по ключам и создание объекта 
       * // InternalCacheItem, выполняет блокирование корневой коллекции KeyDict.
       * Не выгодно использовать две блокировки. Лучше использовать одну общую блокировку по по объекту TheDict
       * 
       * После того, как GetInternalItem() вернул объект InternalCacheItem, общая блокировка снимается.
       * Объект InternalCacheItem блокируется на момент проверки наличия данных, создания данных с помощью
       * Factrory и сброса сериализованных данных на диск
       * 
       * После того, как нужные данные получены, проверяется переполнение списка MRU. 
       * Берется самый "старый" элемент. Он блокируется и очищается (удаляется файл на диске).
       * Затем элемент удаляется из цепочки (с блокировкой KeyDict)
       * 
       * Таким образом, при выполнении длительных действий (создание данных, чтение/запись файла), выполняется
       * блокировка только конкретного объекта InternalCacheItem. Блокировка коллекций выполняется кратковременно
       */

      /// <summary>
      /// Коллекция страниц по типам
      /// </summary>
      private TypeDict _TheTypeDict;

      #endregion

      #region Параметры

      private readonly CacheParams _Params;

      #endregion

      #region MRU-список

      private InternalCacheItem _FirstItem;

      private InternalCacheItem _LastItem;

      private bool MRUContainsItem(InternalCacheItem item)
      {
        if (item.PrevItem == null && item.NextItem == null)
          return this._FirstItem == item || this._LastItem == item;
        else
          return true;
      }

      private void MRUAdd(InternalCacheItem item)
      {
#if DEBUG
        if (MRUContainsItem(item))
          throw new InvalidOperationException("Повторный вызов MRUAdd()");

        CheckMRUState(item);
#endif

        item.NextItem = this._FirstItem;
        if (this._FirstItem == null)
          this._LastItem = item;
        else
          this._FirstItem.PrevItem = item;
        this._FirstItem = item;

#if DEBUG
        CheckMRUState(item);
#endif
      }

      private bool MRURemove(InternalCacheItem item)
      {
#if DEBUG
        CheckMRUState(item);
#endif

        if (MRUContainsItem(item))
        {
          if (item.NextItem == null)
            this._LastItem = item.PrevItem;
          else
            item.NextItem.PrevItem = item.PrevItem;
          if (item.PrevItem == null)
            this._FirstItem = item.NextItem;
          else
            item.PrevItem.NextItem = item.NextItem;
          item.NextItem = null;
          item.PrevItem = null;
#if DEBUG
          CheckMRUState(item);
#endif
          return true;
        }
        else
          return false;
      }


      private void MRUToFirst(InternalCacheItem item)
      {
        if (this._FirstItem == item)
          return; // и так наверху
        // ? Оптимизация
        MRURemove(item);
        MRUAdd(item);
      }

#if DEBUG

      private void CheckMRUState(InternalCacheItem item)
      {
        if (item.PrevItem == null)
        {
          if (item.NextItem != null && this._FirstItem != null && this._FirstItem != item)
            throw new BugException("FirstItem должен указывать на элемент, т.к. Item.PrevItem==null");
        }
        else
        {
          if (this._FirstItem == item)
            throw new BugException("FirstItem не должен указывать на элемент, т.к. Item.PrevItem!=null");
          if (item.PrevItem == item)
            throw new BugException("Item.PrevItem указывает сам на себя");
          if (item.PrevItem.NextItem != item)
            throw new BugException("Item.PrevItem.NextItem != Item");
        }

        if (item.NextItem == null)
        {
          if (item.PrevItem != null && this._LastItem != null && this._LastItem != item)
            throw new BugException("LastItem должен указывать на элемент, т.к. Item.NextItem==null");
        }
        else
        {
          if (this._LastItem == item)
            throw new BugException("LastItem не должен указывать на элемент, т.к. Item.NextItem!=null");
          if (item.NextItem == item)
            throw new BugException("Item.NextItem указывает сам на себя");
          if (item.NextItem.PrevItem != item)
            throw new BugException("Item.NextItem.PrevItem != Item");
        }

        if ((this._FirstItem == null) != (this._LastItem == null))
          throw new BugException("FirstItem и LastItem могут быть null только одновременно");

        if ((item.PrevItem != null || item.NextItem != null) && this._FirstItem == null)
          throw new BugException("Item находится в списке, но список считается пустым");
      }

#endif

      #endregion

      #region Доступ к элементам

      //public int Count
      //{
      //  get
      //  {
      //    lock (_TheTypeDict)
      //    {
      //      return _Count;
      //    }
      //  }
      //}
      private int _Count;

      private InternalCacheItem GetInternalItem(Type objType, string[] keys, bool create)
      {
        if (objType == null)
          throw new ArgumentNullException("objType");
        if (keys.Length < 1)
          throw new ArgumentException("Длина списка ключей не может быть меньше 1", "keys");

        InternalCacheItem Res;

        lock (_TheTypeDict)
        {
          #region Вход для типа данных

          TypeInfo ti;
          if (!_TheTypeDict.TryGetValue(objType, out ti))
          {
            if (create)
            {
              ti = new TypeInfo(this, objType);
              _TheTypeDict.Add(objType, ti);
            }
            else
              return null;
          }

          #endregion

          #region Создание промежуточных страниц

          KeyDict Dict = ti;

          for (int i = 0; i < (keys.Length - 1); i++)
          {
            object Dict2;
            if (!Dict.TryGetValue(keys[i], out Dict2))
            {
              if (create)
              {
                Dict2 = new KeyDict(this, objType, Dict, keys[i]);
                Dict.Add(keys[i], Dict2);
              }
              else
                return null;
            }
            Dict = (KeyDict)Dict2;
          }

          #endregion

          #region Создание InternalCacheItem

          object Res2;
          string LastKey = keys[keys.Length - 1];
          if (Dict.TryGetValue(LastKey, out Res2))
          {
            Res = (InternalCacheItem)Res2;
            MRUToFirst(Res);
          }
          else
          {
            if (create)
            {
              Res = new InternalCacheItem(Dict, LastKey);
              Res.Persistance = ItemNotInit;
              Dict.Add(LastKey, Res);
              // Добавляем в MRU
              MRUAdd(Res);
              _Count++;
              IncStat(objType, CacheStatParam.AddCount);
              IncStat(objType, CacheStatParam.Count);
            }
            else
              return null;
          }

          #endregion

          IncStat(objType, CacheStatParam.AccessCount);
        }

        return Res;
      }


      private void TrimExcess()
      {
        InternalCacheItem ItemToDelete = null;

        lock (_TheTypeDict)
        {
          if (_Count > _Params.Capacity)
          {
            ItemToDelete = this._LastItem;
          }
        }

        if (ItemToDelete != null)
          Remove(ItemToDelete, false);
      }

      #endregion

      #region Чтение / запись в файл

      /// <summary>
      /// Временный каталог для режимов MemoryAndTempDir и TempDirOnly
      /// </summary>
      private TempDirectory _TempDir;

      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      private object LoadFromFile(AbsPath rootDir, Type objType, string[] keys, bool isPersistDir)
      {
        AbsPath FilePath = GetBinFilePath(rootDir, objType, keys, isPersistDir, true);
        if (!File.Exists(FilePath.Path))
          return null;

        try
        {
          object obj;
          FileStream fs = new FileStream(FilePath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
          try
          {
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(fs);
          }
          finally
          {
            fs.Close();
          }

          if (obj.GetType() == objType)
          {
            IncStat(objType, CacheStatParam.LoadFileCount);

            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(objType, keys) + "Loaded from file");

            return obj;
          }
          else
          {
            IncStat(objType, CacheStatParam.LoadFileErrors);
            return null; // Получили какой-то другой объект
          }
        }
        catch
        {
          IncStat(objType, CacheStatParam.LoadFileErrors);
          return null;
        }
      }

      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      private void WriteToFile(AbsPath rootDir, Type objType, string[] keys, object value, bool isPersistDir)
      {
        try
        {
          AbsPath FilePath = GetBinFilePath(rootDir, objType, keys, isPersistDir, true);
          FileTools.ForceDirs(FilePath.ParentDir);

          FileStream fs = new FileStream(FilePath.Path, FileMode.Create);
          try
          {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, value);
          }
          finally
          {
            fs.Close();
          }
          IncStat(objType, CacheStatParam.SaveFileCount);

          if (Cache.TraceSwitch.Enabled)
            Trace.WriteLine(GetTracePrefix(objType, keys) + "Saved to file");
        }
        catch
        {
          IncStat(objType, CacheStatParam.SaveFileErrors);
        }
      }

      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      private void DeleteFile(AbsPath rootDir, Type objType, string[] keys, bool isPersistDir)
      {
        try
        {
          AbsPath FilePath = GetBinFilePath(rootDir, objType, keys, isPersistDir, true);
          if (File.Exists(FilePath.Path)) // чтобы не нарываться на исключение
          {
            File.Delete(FilePath.Path);
            // Удалять дерево каталогов пока не будем
            IncStat(objType, CacheStatParam.DelFileCount);
            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(objType, keys) + "File removed");
          }
        }
        catch
        {
          IncStat(objType, CacheStatParam.DelFileErrors);
        }
      }

      private AbsPath GetBinFilePath(AbsPath rootDir, Type objType, string[] keys, bool isPersistDir, bool isFile)
      {
        AbsPath TypePath = new AbsPath(rootDir, objType.ToString());
        AbsPath FilePath = new AbsPath(TypePath, keys);
        if (isFile)
        {
          if (isPersistDir)
          {
            string version = GetVesion(objType, keys);
            string ExtFileName = FilePath.FileName + "#" + version + ".bin";
            FilePath = new AbsPath(FilePath.ParentDir, ExtFileName);
          }
          else
            FilePath = FilePath.ChangeExtension(".bin");
        }
        return FilePath;
      }

      /// <summary>
      /// Возвращает true, если файл существует
      /// </summary>
      /// <param name="rootDir"></param>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      /// <param name="isPersistDir"></param>
      /// <returns></returns>
      private bool TestFileExists(AbsPath rootDir, Type objType, string[] keys, bool isPersistDir)
      {
        AbsPath FilePath = GetBinFilePath(rootDir, objType, keys, isPersistDir, true);
        return File.Exists(FilePath.Path);
      }

      #endregion

      #region Доп. константы для CachePersistance

      /// <summary>
      /// Элемент был только что создан, но еще не инициализирован
      /// </summary>
      private const CachePersistance ItemNotInit = (CachePersistance)0;

      /// <summary>
      /// Элемент был удален
      /// </summary>
      private const CachePersistance ItemDeleted = (CachePersistance)(-1);

      #endregion

      #region Основной метод доступа

      internal T GetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory, int lockTimeout)
        where T : class
      {
        // Делаем три попытки с перехватом OutOfMemoryException

        try
        {
          // Первая попытка
          return DoGetItem<T>(keys, persistance, factory, lockTimeout);
        }
        catch (OutOfMemoryException)
        {
          // Первая попытка закончилась неудачно
          CheckMemory();
          try
          {
            // Вторая попытка
            return DoGetItem<T>(keys, persistance, factory, lockTimeout);
          }
          catch (OutOfMemoryException)
          {
            // Вторая попытка закончилась неудачно
            Clear(false);

            // Третья попытка
            return DoGetItem<T>(keys, persistance, factory, lockTimeout);
          }
        }
      }


      private T DoGetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory, int lockTimeout)
        where T : class
      {
        InternalCacheItem item = GetInternalItem(typeof(T), keys, true);

        T Res;

        //lock (item)
        if (!Monitor.TryEnter(item, lockTimeout))
          ThrowLockTimeoutException(typeof(T), keys); // 21.09.2020
        try
        {
          // Проверка реентрантного вызова должна выполняться внутри блокировки, а не снаружи
          if (item.InsideGetItemFlag)
            throw new ReenteranceException("Вложенный вызов GetItem() для объекта " + typeof(T).ToString() +
              " с ключами {" + String.Join(", ", keys) + "}");

          item.InsideGetItemFlag = true;
          try
          {
            Res = DoGetItem2<T>(item, keys, persistance, factory);
          }
          finally
          {
            item.InsideGetItemFlag = false;
          }
        }
        finally
        {
          Monitor.Exit(item);
        }

        // После того, как данные получены, можно удалить лишние элементы из списка
        TrimExcess();

        return Res;
      }

      private void ThrowLockTimeoutException(Type objType, string[] keys)
      {
        // Нельзя выводить в log-файл исключение, так как при этом снова возникнет блокировка

        //try
        //{
        throw new LockTimeoutException("Тайм-аут установки блокировки на страницу кэша типа " + objType.ToString() + " с ключами {" + String.Join(", ", keys) + "}");
        //}
        //catch (LockTimeoutException e)
        //{
        //  LogoutTools.LogoutException(e, "Немедленный перехват LockTimeoutException в Cache");
        //  throw;
        //}
      }

      /// <summary>
      /// Этот метод вызывается, пока InternalCacheItem заблокирован
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="item"></param>
      /// <param name="keys"></param>
      /// <param name="persistance"></param>
      /// <param name="factory"></param>
      /// <returns></returns>
      private T DoGetItem2<T>(InternalCacheItem item, string[] keys, CachePersistance persistance, ICacheFactory<T> factory)
        where T : class
      {
        T Res;

        #region Из памяти

        if (item.MemValue != null)
        {
          IncStat(typeof(T), CacheStatParam.FromMemCount);

          return (T)(item.MemValue);
        }

        #endregion

        #region Из файла

        if ((persistance & CachePersistance.PersistOnly) == CachePersistance.PersistOnly)
        {
          if (_Params.PersistDir.IsEmpty)
          {
            // Если постоянный каталог не задан, используем временный каталог
            if (persistance == CachePersistance.PersistOnly)
              persistance = CachePersistance.TempDirOnly;
            else
              persistance = CachePersistance.MemoryAndTempDir;
          }
          else
          {
            Res = (T)(LoadFromFile(_Params.PersistDir, typeof(T), keys, true));
            if (Res != null)
            {
              if (persistance == CachePersistance.MemoryAndPersist)
                item.MemValue = Res; // "приклеили" данные обратно к элементу

              return Res;
            }
          }
        }

        if (item.Persistance == persistance) // иначе не пытаемся загружать
        {
          if ((persistance & CachePersistance.TempDirOnly) == CachePersistance.TempDirOnly)
          {
            Res = (T)(LoadFromFile(_TempDir.Dir, typeof(T), keys, false));
            if (Res != null)
            {
              if (persistance == CachePersistance.MemoryAndTempDir)
                item.MemValue = Res; // "приклеили" данные обратно к элементу

              return Res;
            }
          }
        }

        #endregion

        if (factory == null)
          return null;

        #region Создаем новые данные

        if (TraceSwitch.Enabled)
        {
          DateTime tmStart = DateTime.Now;
          Res = factory.CreateCacheItem(keys);
          TimeSpan ts = DateTime.Now - tmStart;
          Trace.WriteLine(Cache.GetTracePrefix(typeof(T), keys) + "Item created. Time=" + ts.ToString());
        }
        else
          Res = factory.CreateCacheItem(keys);

        if (Res == null)
          throw new NullReferenceException("Объект кэша не был создан в " + factory.ToString());

        IncStat(typeof(T), CacheStatParam.CreateCount);

        #endregion

        item.Persistance = persistance;

        #region Запись в файл

        switch (persistance)
        {
          case CachePersistance.MemoryAndPersist:
          case CachePersistance.PersistOnly:
            WriteToFile(_Params.PersistDir, typeof(T), keys, Res, true);
            break;
          // Потом перебросим case CachePersistance.MemoryAndTempDir:
          case CachePersistance.TempDirOnly:
            WriteToFile(_TempDir.Dir, typeof(T), keys, Res, false);
            break;
        }

        #endregion

        #region Запись в память

        switch (persistance)
        {
          case CachePersistance.MemoryOnly:
          case CachePersistance.MemoryAndPersist:
          case CachePersistance.MemoryAndTempDir:
            item.MemValue = Res;
            break;
        }

        #endregion

        return Res;
      }

      #endregion

      #region Вспомогательные методы доступа

      internal T GetItemIfExists<T>(string[] keys, CachePersistance persistance, int lockTimeout)
        where T : class
      {
        // Делаем три попытки с перехватом OutOfMemoryException

        try
        {
          InternalCacheItem item = GetInternalItem(typeof(T), keys, (persistance & CachePersistance.PersistOnly) != 0);
          if (item == null)
            return null;

          T Res;

          //lock (Item)
          if (!Monitor.TryEnter(item, lockTimeout))
            ThrowLockTimeoutException(typeof(T), keys); // 21.09.2020
          try
          {
            if (item.InsideGetItemFlag)
              Res = null;
            else
              Res = DoGetItem2<T>(item, keys, persistance, null);
          }
          finally
          {
            Monitor.Exit(item);
          }

          return Res;
        }
        catch
        {
          return null;
        }
      }


      internal void SetItem<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
        where T : class
      {
        // Делаем три попытки с перехватом OutOfMemoryException
        try
        {
          // Первая попытка
          DoSetItem<T>(keys, persistance, newValue, lockTimeout);
        }
        catch (OutOfMemoryException)
        {
          // Первая попытка закончилась неудачно
          CheckMemory();
          try
          {
            // Вторая попытка
            DoSetItem<T>(keys, persistance, newValue, lockTimeout);
          }
          catch (OutOfMemoryException)
          {
            // Вторая попытка закончилась неудачно
            Clear(false);

            // Третья попытка
            DoSetItem<T>(keys, persistance, newValue, lockTimeout);
          }
        }
      }

      private void DoSetItem<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
        where T : class
      {
        // TODO: Метод SetItem(), вероятно, нуждается в оптимизации

        // Используем блокировку на время замены, так как между удалением и записью значения,
        // параллельный поток может обратиться к значению и загрузить другое значение с помощью своей фабрики - лишние действия
        InternalCacheItem item = GetInternalItem(typeof(T), keys, true);
        //lock (item)
        if (!Monitor.TryEnter(item, lockTimeout))
          ThrowLockTimeoutException(typeof(T), keys); // 21.09.2020
        try
        {
          // Проверка реентрантного вызова должна выполняться внутри блокировки, а не снаружи
          if (item.InsideGetItemFlag)
            throw new ReenteranceException("Вложенный вызов SetItem() при вызове GetItem() для объекта " + typeof(T).ToString() +
              " с ключами {" + String.Join(", ", keys) + "}");

          Clear(typeof(T), keys, true); // удаляем
          DoGetItem<T>(keys, persistance, new DummyFactory<T>(newValue), lockTimeout /* Фиктивный параметр, т.к. уже заблокировано */ ); // добавляем
        }
        finally
        {
          Monitor.Exit(item);
        }
      }

      internal bool SetItemIfNew<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
        where T : class
      {
        bool res = false;

        InternalCacheItem item = GetInternalItem(typeof(T), keys, true);
        if (!Monitor.TryEnter(item, lockTimeout))
          return false;
        try
        {
          if (!DoGetItemContainsData(item, typeof(T), keys))
          {
            DoGetItem<T>(keys, persistance, new DummyFactory<T>(newValue), lockTimeout /* Фиктивный параметр, т.к. уже заблокировано */ ); // добавляем
            res = true;
          }
        }
        finally
        {
          Monitor.Exit(item);
        }
        return res;
      }

      /// <summary>
      /// Проверяет наличие данных в памяти или в файле.
      /// На момент вызова <paramref name="item"/> должен быть заблокирован
      /// </summary>
      /// <param name="item"></param>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      /// <returns></returns>
      private bool DoGetItemContainsData(InternalCacheItem item, Type objType, string[] keys)
      {
        #region Из памяти

        if (item.MemValue != null)
          return true;

        #endregion

        #region Из файла

        if ((item.Persistance & CachePersistance.PersistOnly) == CachePersistance.PersistOnly)
        {
          if (TestFileExists(_Params.PersistDir, objType, keys, true))
            return true;
        }

        if ((item.Persistance & CachePersistance.TempDirOnly) == CachePersistance.TempDirOnly)
        {
          if (TestFileExists(_TempDir.Dir, objType, keys, false))
            return true;
        }

        #endregion

        return false;
      }

      #endregion

      #region Версии

      private string DoSetVersion(Type objType, string[] keys, string version, bool useVersionTxt)
      {
        string oldVersion;
        lock (_TheTypeDict)
        {
          #region Вход для типа данных

          TypeInfo ti;
          if (!_TheTypeDict.TryGetValue(objType, out ti))
          {
            ti = new TypeInfo(this, objType);
            _TheTypeDict.Add(objType, ti);
          }

          #endregion

          #region Инициализация и проверка словаря версий

          if (ti.VesrionDict == null)
            ti.SetVersionInfo(keys.Length, useVersionTxt);
          else
          {
            if (keys.Length != ti.VersionKeyLen)
              throw new ArgumentException("Неправильная длина массива ключей: " + keys.Length.ToString() +
                ". При прошлом вызове SetVersion()/SyncVersion() для типа " + objType.ToString() + " длина массива ключей была равна " + ti.VersionKeyLen.ToString(), "keys");
            if (useVersionTxt != ti.UseVersionTxt)
              throw new InvalidOperationException("Для одного типа данных нельзя вызывать попеременно SetVersion() и SyncVersion()");
          }

          #endregion

          #region Запись в словарь

          string sKeys = String.Join("|", keys);

          if (!ti.VesrionDict.TryGetValue(sKeys, out oldVersion))
            oldVersion = String.Empty;
          ti.VesrionDict[sKeys] = version;

          #endregion
        }

        return oldVersion;
      }

      internal CacheSetVersionResult SetVersion(Type objType, string[] keys, string version)
      {
        if (keys == null)
          keys = DataTools.EmptyStrings;

        string oldVersion = DoSetVersion(objType, keys, version, true);
        bool changed = !String.Equals(version, oldVersion, StringComparison.Ordinal);
        bool clearPersist;
        if (oldVersion.Length == 0)
        {
          // Первый вызов
          oldVersion = ReadVersionTxt(objType, keys);
          clearPersist = !String.Equals(version, oldVersion, StringComparison.Ordinal);
          if (clearPersist)
            WriteVersionTxt(objType, keys, version);
        }
        else
          clearPersist = changed;
        if (changed)
          Clear(objType, keys, clearPersist);
        return new CacheSetVersionResult(objType, keys, version, clearPersist);
      }

      internal void SyncVersion(Type objType, string[] keys, CacheSetVersionResult setResult)
      {
        string oldVersion = DoSetVersion(objType, keys, setResult.Version, false);
        bool changed = !String.Equals(setResult.Version, oldVersion, StringComparison.Ordinal);
        if (changed)
          Clear(objType, keys, setResult.PersistFilesCleared);
      }

      internal string GetVesion(Type objType, string[] keys)
      {
        if (keys == null)
          keys = DataTools.EmptyStrings;

        string version = String.Empty;
        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (_TheTypeDict.TryGetValue(objType, out ti))
          {
            if (ti.VesrionDict != null)
            {
              if (keys.Length >= ti.VersionKeyLen) // иначе версия не определена
              {
                string sKeys;
                switch (ti.VersionKeyLen)
                {
                  case 0: sKeys = String.Empty; break;
                  case 1: sKeys = keys[0]; break;
                  default:
                    // По-хорошему, это должно встречаться крайне редко
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < ti.VersionKeyLen; i++)
                    {
                      if (i > 0)
                        sb.Append('|');
                      sb.Append(keys[i]);
                    }
                    sKeys = sb.ToString();
                    break;
                }

                if (!ti.VesrionDict.TryGetValue(sKeys, out version))
                  version = String.Empty;
              }
            }
          }
        }

        return version;
      }

      #region Чтение/запись файла version.txt

      private string ReadVersionTxt(Type objType, string[] keys)
      {
        if (_Params.PersistDir.IsEmpty)
          return String.Empty;

        AbsPath path = GetVersionTxtPath(objType, keys);
        try
        {
          if (System.IO.File.Exists(path.Path))
            return System.IO.File.ReadAllText(path.Path, Encoding.UTF8);
          else
            return String.Empty;
        }
        catch
        {
          return String.Empty;
        }
      }

      public void WriteVersionTxt(Type objType, string[] keys, string version)
      {
        if (_Params.PersistDir.IsEmpty)
          return;

        AbsPath path = GetVersionTxtPath(objType, keys);
        try
        {
          FileTools.ForceDirs(path.ParentDir);
          System.IO.File.WriteAllText(path.Path, version, Encoding.UTF8);
        }
        catch { }
      }

      private AbsPath GetVersionTxtPath(Type objType, string[] keys)
      {
        AbsPath TypePath = new AbsPath(_Params.PersistDir, objType.ToString());
        AbsPath KeyPath = new AbsPath(TypePath, keys);
        return new AbsPath(KeyPath, "version.txt");
      }

      #endregion

      #endregion

      #region Методы очистки

      /// <summary>
      /// Удалить элемент.
      /// Если ClearPersist=true, то метод явно, так как кэш устарел.
      /// Надо удалить все файлы
      /// </summary>
      /// <param name="item"></param>
      /// <param name="clearPersist"></param>
      private void Remove(InternalCacheItem item, bool clearPersist)
      {
        // Запоминаем заранее
        string[] keys = item.Keys;

        // Удаляем из списков до блокировки Item, чтобы порядок блокировки не нарушался
        lock (_TheTypeDict)
        {
          KeyDict Dict = item.Parent;
          string ParentKey = item.ParentKey;
          while (Dict != null)
          {
            Dict.Remove(ParentKey);
            if (Dict.Count > 0)
              break;
            ParentKey = Dict.ParentKey;
            Dict = Dict.Parent;
          }
          if (MRURemove(item))
          {
            _Count--;
            IncStat(item.Parent.ObjType, CacheStatParam.RemoveCount);
            DecStat(item.Parent.ObjType, CacheStatParam.Count);

            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(item.Parent.ObjType, keys), "Item removed");
          }
        }

        // Даже если сейчас другой поток вызовет GetItem() с такими же ключами, будет создан другой экземпляр
        // InternalCacheItem 

        if (Monitor.TryEnter(item, 50))
        {
          try
          {
            DoClearItemInternal(item, clearPersist, keys);
          }
          finally
          {
            Monitor.Exit(item);
          }
        }
        else if (!IsDisposed)
        {
          lock (_DelayedClearList)
          {
            _DelayedClearList.Add(new DelayedClearInfo(item, clearPersist, keys));
            IncStat(item.Parent.ObjType, CacheStatParam.DelayedDelStarted);
            if (_DelayedClearTimer == null)
              _DelayedClearTimer = new Timer(DelayedClearTimer_Tick, null, 200, 200);
          }
        }
      }

      /// <summary>
      /// Очистка одного элемента.
      /// На момент вызова объект <paramref name="item"/> заблокирован
      /// </summary>
      /// <param name="item">Элемент</param>
      /// <param name="clearPersist">Если true, то нужно удалить постоянный файл</param>
      /// <param name="keys">Ключи</param>
      private void DoClearItemInternal(InternalCacheItem item, bool clearPersist, string[] keys)
      {
        item.MemValue = null; // освобождаем память

        // Тут возможна проблема. Другой поток как раз сейчас может создавать файл с тем же именем
        // Значит мы его сотрем :)
        switch (item.Persistance)
        {
          case CachePersistance.MemoryAndTempDir:
          case CachePersistance.TempDirOnly:
            DeleteFile(_TempDir.Dir, item.Parent.ObjType, keys, false);
            break;
          case CachePersistance.PersistOnly:
          case CachePersistance.MemoryAndPersist:
            if (clearPersist)
              DeleteFile(_Params.PersistDir, item.Parent.ObjType, keys, true);
            break;
        }

        item.Persistance = ItemDeleted; // чтобы второй раз не пытаться удалять файлы
      }

      //internal void Clear<T>(string[] keys, bool clearPersist)
      //  where T : class
      //{
      //  Clear(typeof(T), keys, clearPersist);
      //}

      internal void Clear(Type objType, string[] keys, bool clearPersist)
      {
        if (keys == null)
          keys = DataTools.EmptyStrings; // запрошено удаление всех объектов

        DoClear(objType, keys, clearPersist);

        // 25.04.2017
        // Удаляем файлы устойчивых объектов после основной очистки кэша, чтобы правильно отображалась статистика
        if (clearPersist)
          DeletePersistFiles(objType, keys);
      }

      private void DoClear(Type objType, string[] keys, bool clearPersist)
      {

        List<InternalCacheItem> Items;

        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (!_TheTypeDict.TryGetValue(objType, out ti))
            return; // Не было ни одного объекта заданного типа

          KeyDict Dict = ti;
          object CurrObj = Dict;
          for (int i = 0; i < keys.Length; i++)
          {
            if (!Dict.TryGetValue(keys[i], out CurrObj))
              return; // заданного ключа нет
            if (i < (keys.Length - 1))
            {
              Dict = CurrObj as KeyDict;
              if (Dict == null)
                throw new ArgumentException("Неправильная длина списка ключей: " + keys.Length.ToString());
            }
          }

          if (CurrObj is InternalCacheItem)
          {
            Items = new List<InternalCacheItem>(1);
            Items.Add((InternalCacheItem)CurrObj);
          }
          else
          {
            Items = new List<InternalCacheItem>();
            AddItems(Items, (KeyDict)CurrObj);
          }
        }

        // Список Items содержит массив объектов для удаления
        for (int i = 0; i < Items.Count; i++)
          Remove(Items[i], clearPersist);
      }

      [DebuggerStepThrough]
      private void DeletePersistFiles(Type objType, string[] keys)
      {
        if (Params.PersistDir.IsEmpty)
          return;
        // Ключи могут задавать путь к файлу или каталогу
        try
        {
          AbsPath DirPath = GetBinFilePath(Params.PersistDir, objType, keys, true, false);
          if (Directory.Exists(DirPath.Path))
            FileTools.ClearDirAsPossible(DirPath);
          else if (keys.Length > 0) // условие добавлено 19.10.2020
          {
            AbsPath FilePath = GetBinFilePath(Params.PersistDir, objType, keys, true, true);
            //if (File.Exists(FilePath.Path))
            //  File.Delete(FilePath.Path);
            // 25.07.2020
            // Удаляем все файлы *.bin
            //string FileMask = "*.bin";
            // 19.10.2020
            // Так тоже неправильно. Удалили все файлы последнего уровня, а не только для текущего ключа.
            // string FileMask = keys[keys.Length - 1];
            // int p = FileMask.IndexOf('#');
            //#if DEBUG
            //            if (p < 0)
            //              throw new BugException("Имя файла " + FilePath.Path + " не содержит символа \"#\"");
            //#endif
            // FileMask = FileMask.Substring(0, p + 1) + "*.bin";

            // 04.01.2021
            // И опять неправильно!
            string FileMask = keys[keys.Length - 1] + "*.bin";

            if (Directory.Exists(FilePath.ParentDir.Path))
              FileTools.DeleteFiles(FilePath.ParentDir, FileMask, SearchOption.TopDirectoryOnly);
          }

          // Восстановление файлов version.info
          RestoreVersionTxtFiles(objType, keys);
        }
        catch
        {
        }
      }

      /// <summary>
      /// Восстановление файлов version.txt
      /// </summary>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      private void RestoreVersionTxtFiles(Type objType, string[] keys)
      {
        TypeInfo ti;
        lock (_TheTypeDict)
        {
          if (_TheTypeDict.TryGetValue(objType, out ti))
          {
            if (ti.UseVersionTxt)
            {
              if (keys == null)
              {
                foreach (KeyValuePair<string, string> pair in ti.VesrionDict)
                {
                  string[] ThisKeys = pair.Key.Split('|');
                  WriteVersionTxt(objType, ThisKeys, pair.Value);
                }
              }
              else
              {
                string sKeys1 = String.Join("|", keys);
                string sKeys2 = sKeys1 + "|";
                foreach (KeyValuePair<string, string> pair in ti.VesrionDict)
                {
                  if (pair.Key == sKeys1 || pair.Key.StartsWith(sKeys2))
                  {
                    string[] ThisKeys = pair.Key.Split('|');
                    WriteVersionTxt(objType, ThisKeys, pair.Value);
                  }
                }
              }
            }
          }
        }
      }

      internal void Clear(bool clearPersist)
      {
        Type[] typs;
        lock (_TheTypeDict)
        {
          typs = new Type[_TheTypeDict.Keys.Count];
          _TheTypeDict.Keys.CopyTo(typs, 0);
        }

        if (clearPersist && (!Params.PersistDir.IsEmpty))
          FileTools.ClearDirAsPossible(Params.PersistDir); // 10.04.2020. Надо удалять, в том числе, и те типы данных, которые (пока) не использовались в программе

        for (int i = 0; i < typs.Length; i++)
        {
          Clear(typs[i], null, /*clearPersist*/ false /* больше не нужно, все файлы уже стерты */);
          if (clearPersist && (!Params.PersistDir.IsEmpty))
            RestoreVersionTxtFiles(typs[i], null);
        }
      }

      /// <summary>
      /// Рекурсивная процедура добавления элементов в список
      /// </summary>
      /// <param name="items"></param>
      /// <param name="dict"></param>
      private static void AddItems(List<InternalCacheItem> items, KeyDict dict)
      {
        foreach (KeyValuePair<string, object> Pair in dict)
        {
          if (Pair.Value is InternalCacheItem)
            items.Add((InternalCacheItem)(Pair.Value));
          else
            // рекурсивный вызов
            AddItems(items, (KeyDict)(Pair.Value));
        }
      }

      internal void FreeMemory()
      {
        while (FlushBlock(true))
        {
        }
      }

      #endregion

      #region Обработка сигнала таймера проверки памяти

      private Timer _CheckMemoryTimer;

      private bool _InsideCheckMemoryTimer_Tick;

      private void CheckMemoryTimer_Tick(object state)
      {
        try
        {
          if (!_InsideCheckMemoryTimer_Tick)
          {
            _InsideCheckMemoryTimer_Tick = true;
            try
            {
              CheckMemory();
            }
            finally
            {
              _InsideCheckMemoryTimer_Tick = false;
            }
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка очистки кэша по таймеру");
        }
      }

      /// <summary>
      /// Выполнить удаление лишних элементов кэша по таймеру, из-за ошибки или вызовом пользователя.
      /// Этот метод должен правильно отрабатывать многопоточность. Если он вызван из другого потока,
      /// пока обработка выполняется в первом потоке, второй поток должен дождаться завершения
      /// </summary>
      internal void CheckMemory()
      {
        if (Monitor.TryEnter(Cache._CheckMemoryStat))
        {
          try
          {
            Cache._CheckMemoryStat.Inc(CacheCheckMemoryStatParam.CallCount);
            DoCheckMemory1();
            DoCheckMemory2();
          }
          finally
          {
            Monitor.Exit(Cache._CheckMemoryStat);
          }
          return;
        }
        // Ожидаем блокировку, но ничего не делаем
        lock (Cache._CheckMemoryStat)
        {
        }
      }

      private void DoCheckMemory1()
      {
        bool flag = false;
        bool resolved = true;
        bool HasDel = false;

        while (!MemoryTools.CheckSufficientMemory(MemoryTools.LowMemorySizeMB))
        {
          flag = true;
          if (!DeleteBlockWhenMemoryLow())
          {
            resolved = false;
            break; // 28.08.2019
          }

#if USE_GCCOLLECT
          GC.Collect();
#endif
          HasDel = true;
        }

        if (flag)
        {
          Cache._CheckMemoryStat.Inc(CacheCheckMemoryStatParam.InsufficientMemoryTotalCount);
          if (resolved)
            Cache._CheckMemoryStat.Inc(CacheCheckMemoryStatParam.InsufficientMemoryResolvedCount);
        }

        if (HasDel)
        {
          // После того, как освободили критическое количество виртуальной памяти,
          // перекидываем один блок в файлы
          FlushBlock(false);
        }
      }

      private void DoCheckMemory2()
      {
        bool flag = false;
        bool resolved = true;

        while (MemoryTools.MemoryLoad >= Params.CriticalMemoryLoad)
        {
          flag = true;
          if (!FlushBlock(true))
          {
            resolved = false;
            break;
          }
#if USE_GCCOLLECT
          GC.Collect();
#endif
        }

        if (flag)
        {
          Cache._CheckMemoryStat.Inc(CacheCheckMemoryStatParam.MemoryLoadTotalCount);
          if (resolved)
            Cache._CheckMemoryStat.Inc(CacheCheckMemoryStatParam.MemoryLoadResolvedCount);
        }
      }

      /// <summary>
      /// Удаление 100 самых старых элементов кэша
      /// </summary>
      /// <returns></returns>
      private bool DeleteBlockWhenMemoryLow()
      {
        bool Res = false;
        for (int i = 0; i < Params.ClearCacheBlockSize; i++)
        {
          InternalCacheItem ItemToDelete;
          lock (_TheTypeDict)
          {
            ItemToDelete = this._LastItem;
          }

          if (ItemToDelete != null)
          {
            Remove(ItemToDelete, false);
            IncStat(ItemToDelete.Parent.ObjType, CacheStatParam.LowMemoryRemoveCount);
            Res = true;
          }
          else
            break;
        }
        return Res;
      }

      private bool FlushBlock(bool deleteMemoryOnly)
      {
        #region Собираем список

        List<InternalCacheItem> items = new List<InternalCacheItem>();
        lock (_TheTypeDict)
        {
          InternalCacheItem Item = this._LastItem;
          while (Item != null && items.Count < 100)
          {
            if (Item.MemValue != null)
            {
              if (deleteMemoryOnly || (Item.Persistance != CachePersistance.MemoryOnly))
                items.Add(Item);
            }

            Item = Item.PrevItem;
          }
        }

        #endregion

        if (items.Count == 0)
          return false;

        #region Удаляем/очищаем элементы

        for (int i = 0; i < items.Count; i++)
        {
          switch (items[i].Persistance) // можно обойтись без блокировки
          {
            case CachePersistance.MemoryOnly:
              Remove(items[i], false);
              IncStat(items[i].Parent.ObjType, CacheStatParam.LowMemoryRemoveCount);
              break;
            case CachePersistance.MemoryAndTempDir:
              //lock (Items[i])
              if (Monitor.TryEnter(items[i])) // 21.09.2020 - не ждем блокировку
              {
                try
                {
                  if (items[i].MemValue != null) // значение могло исчезнуть
                  {
                    // Сбрасываем в файл
                    WriteToFile(_TempDir.Dir, items[i].Parent.ObjType, items[i].Keys, items[i].MemValue, false);
                    IncStat(items[i].Parent.ObjType, CacheStatParam.LowMemorySaveFileCount);
                    items[i].MemValue = null;
                  }
                }
                finally
                {
                  Monitor.Exit(items[i]);
                }
              }
              break;
            default:
              //lock (items[i])
              if (Monitor.TryEnter(items[i])) // 21.09.2020 - не ждем блокировку
              {
                try
                {
                  items[i].MemValue = null;
                }
                finally
                {
                  Monitor.Exit(items[i]);
                }
              }
              break;
          }
        }
        #endregion

        return true;
      }

      #endregion

      #region Отложенная очистка элементов

      /// <summary>
      /// Класс однократной записи
      /// </summary>
      private class DelayedClearInfo
      {
        #region Конструктор

        internal DelayedClearInfo(InternalCacheItem item, bool clearPersist, string[] keys)
        {
          _Item = item;
          _ClearPersist = clearPersist;
          _Keys = keys;
        }

        #endregion

        #region Свойства

        internal InternalCacheItem Item { get { return _Item; } }
        private readonly InternalCacheItem _Item;

        internal bool ClearPersist { get { return _ClearPersist; } }
        private readonly bool _ClearPersist;

        internal string[] Keys { get { return _Keys; } }
        private readonly string[] _Keys;

        #endregion
      }

      /// <summary>
      /// Список объектов для асинхронного удаления.
      /// На момент обращения используется блокировка этого объекта, поэтому нет необходимости использовать SyncCollection 
      /// </summary>
      private List<DelayedClearInfo> _DelayedClearList;

      /// <summary>
      /// Односекундный таймер для удаления объектов.
      /// Для создания/удаления/обращения требуется блокировка объекта _DelayedClearList
      /// </summary>
      private Timer _DelayedClearTimer;

      private bool _InsideDelayedClearTimer_Tick;

      private void DelayedClearTimer_Tick(object state)
      {
        try
        {
          if (!_InsideDelayedClearTimer_Tick)
          {
            _InsideDelayedClearTimer_Tick = true;
            try
            {
              lock (_DelayedClearList)
              {
                for (int i = _DelayedClearList.Count - 1; i >= 0; i--)
                {
                  DelayedClearInfo info = _DelayedClearList[i];

                  if (info == null)
                    throw new NullReferenceException("info=null"); // 06.07.2021. Поиск ошибки

                  if (Monitor.TryEnter(info.Item))
                  {
                    try
                    {
                      DoClearItemInternal(info.Item, info.ClearPersist, info.Keys);
                      _DelayedClearList.RemoveAt(i);
                      IncStat(info.Item.Parent.ObjType, CacheStatParam.DelayedDelFinished);
                    }
                    finally
                    {
                      Monitor.Exit(info.Item);
                    }
                  }
                }

                if (_DelayedClearList.Count == 0)
                {
                  _DelayedClearTimer.Dispose();
                  _DelayedClearTimer = null;
                }
              }
            }
            finally
            {
              _InsideDelayedClearTimer_Tick = false;
            }
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка очистки кэша по таймеру");
        }
      }

      #endregion

      #region Сбор статистики

      /// <summary>
      /// Параметры для сбора статистики.
      /// При всех обращениях объект коллекции кратковременно блокируется
      /// </summary>
      private Dictionary<Type, CacheStat> _TypeStats;

      internal void IncStat(Type objType, CacheStatParam statParam)
      {
        lock (_TypeStats)
        {
          CacheStat Stat;
          if (!_TypeStats.TryGetValue(objType, out Stat))
          {
            Stat = new CacheStat();
            _TypeStats.Add(objType, Stat);
          }
          Stat.Inc(statParam);
        }
      }

      internal void DecStat(Type objType, CacheStatParam statParam)
      {
        lock (_TypeStats)
        {
          CacheStat Stat;
          if (!_TypeStats.TryGetValue(objType, out Stat))
          {
            Stat = new CacheStat();
            _TypeStats.Add(objType, Stat);
          }
          Stat.Dec(statParam);
        }
      }

      public CacheStat GetStat(Type objType)
      {
        lock (_TypeStats)
        {
          CacheStat Stat;
          if (_TypeStats.TryGetValue(objType, out Stat))
            return Stat.Clone();
          else
            return new CacheStat();
        }
      }

      public void GetStat(out Type[] objTypes, out CacheStat[] stats)
      {
        lock (_TheTypeDict)
        {
          objTypes = new Type[_TheTypeDict.Count];
          _TheTypeDict.Keys.CopyTo(objTypes, 0);
        }

        stats = new CacheStat[objTypes.Length];
        for (int i = 0; i < objTypes.Length; i++)
          stats[i] = GetStat(objTypes[i]);
      }

      public void GetKeys(Type objType, List<string[]> keyList)
      {
        if (objType == null)
          throw new ArgumentNullException("objType");

        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (_TheTypeDict.TryGetValue(objType, out ti))
            GetKeys2(ti, keyList); // рекурсивный метод
        }
      }

      private void GetKeys2(KeyDict dict, List<string[]> keyList)
      {
        foreach (KeyValuePair<string, object> Pair in dict)
        {
          InternalCacheItem Item = Pair.Value as InternalCacheItem;
          if (Item == null)
            GetKeys2((KeyDict)(Pair.Value), keyList); // рекурсивный вызов
          else
            keyList.Add(Item.Keys);
        }
      }


      public void GetKeysCommaString(Type objType, StringBuilder sb)
      {
        if (objType == null)
          throw new ArgumentNullException("objType");

        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (_TheTypeDict.TryGetValue(objType, out ti))
            GetKeysCommaString2(ti, sb); // рекурсивный метод
        }
      }

      private void GetKeysCommaString2(KeyDict dict, StringBuilder sb)
      {
        foreach (KeyValuePair<string, object> Pair in dict)
        {
          InternalCacheItem Item = Pair.Value as InternalCacheItem;
          if (Item == null)
            GetKeysCommaString2((KeyDict)(Pair.Value), sb); // рекурсивный вызов
          else
          {
            FreeLibSet.Text.CsvTextConvert cnv = new Text.CsvTextConvert();
            cnv.ToString(sb, Item.Keys);
            sb.Append(Environment.NewLine);
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Фиктивная одноразовая фабрика
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class DummyFactory<T> : ICacheFactory<T>
      where T : class
    {
      #region Конструктор

      public DummyFactory(T newValue)
      {
        _newValue = newValue;
      }

      private T _newValue;

      #endregion

      #region ICacheFactory<T> Members

      public T CreateCacheItem(string[] keys)
      {
        return _newValue;
      }

      #endregion
    }

    /// <summary>
    /// Элемент кэша, хранящий ссылку на данные в памяти и являющийся элементом двунаправленного списка.
    /// Важно, чтобы это был класс, а не структура, т.к. ссылки на экземпляр могут существовать и в основном потоке, и в фоновом.
    /// </summary>
    private class InternalCacheItem
    {
      #region Конструктор

      public InternalCacheItem(KeyDict parent, string parentKey)
      {
        _Parent = parent;
        _ParentKey = parentKey;
      }

      #endregion

      #region Основные свойства

      public KeyDict Parent { get { return _Parent; } }
      private readonly KeyDict _Parent;

      /// <summary>
      /// Ключ последнего уровня, который используется в FDict для данного объекта
      /// </summary>
      public string ParentKey { get { return _ParentKey; } }
      private readonly string _ParentKey;

      /// <summary>
      /// Данные в памяти (жесткая ссылка)
      /// </summary>
      public object MemValue;

      /// <summary>
      /// Способ хранения данных
      /// </summary>
      public CachePersistance Persistance;

      #endregion

      #region Дополнительные свойства

      /// <summary>
      /// Массив ключей
      /// </summary>
      public string[] Keys
      {
        get
        {
          // Нет необходимости блокировать коллекции, т.к. свойства Parent и ParentKey задаются в конструкторе InternalCacheItem
          int cnt = 0;
          KeyDict Dict = Parent;
          while (Dict != null)
          {
            cnt++;
            Dict = Dict.Parent;
          }

          string[] a = new string[cnt];
          Dict = Parent;
          a[a.Length - 1] = ParentKey;
          cnt = 1;
          while (Dict.Parent != null)
          {
            cnt++;
            a[a.Length - cnt] = Dict.ParentKey;
            Dict = Dict.Parent;
          }

          return a;
        }
      }

      /// <summary>
      /// Текстовое предтавление - список ключей Keys.
      /// </summary>
      /// <returns>Текстовое предтавление</returns>
      public override string ToString()
      {
        return String.Join(", ", Keys);
      }

      /// <summary>
      /// Устанавливается на время вызова GetItem() для предотвращения реентрантного вызова
      /// </summary>
      public bool InsideGetItemFlag;

      #endregion

      #region MRU-список

      // В качестве индикации того, что элемент не находится в списке MRU, используются значения null
      // Чтобы обозначить терминальные элементы, используется признак NextItem=this и PrevItem=this


      /// <summary>
      /// Двусвязный список - следующий элемент
      /// null, если элемент является последним в списке
      /// </summary>
      internal InternalCacheItem NextItem;

      /// <summary>
      /// Двусвязный список - предыдущий элемент
      /// null, если элемент является первым в списке
      /// </summary>
      internal InternalCacheItem PrevItem;

      #endregion
    }


    /// <summary>
    /// Страница элементов кэша.
    /// Объектом-значением может быть либо дочерний KeyDict, либо InternalCacheItem
    /// </summary>
    private class KeyDict : Dictionary<string, object>
    {
      #region Конструктор

      public KeyDict(InternalCache cache, Type objType, KeyDict parent, string parentKey)
      {
        _Cache = cache;
        _ObjType = objType;
        _Parent = parent;
        _ParentKey = parentKey;
      }

      #endregion

      #region Свойства

      //public InternalCache Cache { get { return _Cache; } }
      private readonly InternalCache _Cache;

      public Type ObjType { get { return _ObjType; } }
      private readonly Type _ObjType;

      public KeyDict Parent { get { return _Parent; } }
      private readonly KeyDict _Parent;

      /// <summary>
      /// Ключ текущего объекта в коллекции Parent
      /// </summary>
      public string ParentKey { get { return _ParentKey; } }
      private readonly string _ParentKey;

      #endregion
    }

    /// <summary>
    /// Стран
    /// </summary>
    private class TypeInfo : KeyDict
    {
      #region Конструктор

      public TypeInfo(InternalCache cache, Type objType)
        : base(cache, objType, null, null)
      {
      }

      #endregion

      #region Свойства для версии

      public void SetVersionInfo(int versionKeyLen, bool useVersionTxt)
      {
        _VersionKeyLen = versionKeyLen;
        _UseVersionTxt = useVersionTxt;
        _VesrionDict = new Dictionary<string, string>();
      }

      /// <summary>
      /// Длина ключа, используемого для версии.
      /// При вызове SetVersion() проверяется, что длина ключа каждый раз совпадает
      /// </summary>
      public int VersionKeyLen { get { return _VersionKeyLen; } }
      private int _VersionKeyLen;

      /// <summary>
      /// Используется ли файл version.txt.
      /// Равно true, если был вызван метод SetVersion() и false, если SyncVersion().
      /// </summary>
      public bool UseVersionTxt { get { return _UseVersionTxt; } }
      private bool _UseVersionTxt;


      /// <summary>
      /// Коллекция версий ключей для данного типа.
      /// В отличие от KeyDict, не используется иерархия объектов. Все версии для объекта хранятся в одной таблице
      /// Ключ коллекции - подмножество ключей, для которого задана версия. Может быть пустой строкой. 
      /// Если версии используются для ключей с уровнем, больше 1, отдельные ключи разделяются вертикальной чертой.
      /// Значение коллекции - версия.
      /// </summary>
      public Dictionary<string, string> VesrionDict { get { return _VesrionDict; } }
      private Dictionary<string, string> _VesrionDict;

      #endregion
    }

    /// <summary>
    /// Коллекция страниц по типам хранимых данных
    /// </summary>
    private class TypeDict : Dictionary<Type, TypeInfo>
    {
    }

    #region Статические свойства и методы

    /// <summary>
    /// Параметры настройки кэша.
    /// Могут устанавливаться только до активации системы, происходящей при первом обращении к методу GetItem(), SetItem(), Clear(), SetVersion()
    /// и некоторых других
    /// </summary>
    public static CacheParams Params { get { return _StaticParams; } }
    private static CacheParams _StaticParams = new CacheParams();


    /// <summary>
    /// Основной метод получение буферизованных данных
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="factory">Объект, выполняющий создание данных, если их нет в кэше</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static T GetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory)
      where T : class
    {
      if (factory == null)
        throw new ArgumentNullException("factory");

      return GetMainObj(GetMainObjMode.Create).GetItem<T>(keys, persistance, factory, Params.LockTimeout);
    }

    /// <summary>
    /// Основной метод получение буферизованных данных
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="factory">Объект, выполняющий создание данных, если их нет в кэше</param>
    /// <param name="lockTimeout">Максимальное время ожидания блокировки страницы кэша в миллисекундах. 
    /// Значение должно быть больше нуля или равно System.Threading.Timeout.Infinite</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static T GetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory, int lockTimeout)
      where T : class
    {
      if (factory == null)
        throw new ArgumentNullException("factory");

      return GetMainObj(GetMainObjMode.Create).GetItem<T>(keys, persistance, factory, lockTimeout);
    }


#if XXX
    /// <summary>
    /// Вспомогательный метод получение буферизованных данных.
    /// Возвращает объект кэша, только если он есть в кэше. Иначе возвращается null.
    /// Эта версия проверяет только наличие объектов в памяти (CachePersistance.MemoryOnly)
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <returns>Сохраненный объект или null</returns>
    [Obsolete("Используйте перегрузку метода с аргументом persistance", false)]
    public static T GetItemIfExists<T>(string[] keys)
      where T : class
    {
      return GetItemIfExists<T>(keys, CachePersistance.MemoryOnly, Params.LockTimeout);
    }
#endif

    /// <summary>
    /// Вспомогательный метод получение буферизованных данных.
    /// Возвращает объект кэша, только если он есть в кэше. Иначе возвращается null
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <returns>Сохраненный объект или null</returns>
    public static T GetItemIfExists<T>(string[] keys, CachePersistance persistance)
      where T : class
    {
      return GetItemIfExists<T>(keys, persistance, Params.LockTimeout);
    }

    /// <summary>
    /// Вспомогательный метод получение буферизованных данных.
    /// Возвращает объект кэша, только если он есть в кэше. Иначе возвращается null
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="lockTimeout">Максимальное время ожидания блокировки страницы кэша в миллисекундах. 
    /// Значение должно быть больше нуля или равно System.Threading.Timeout.Infinite</param>
    /// <returns>Сохраненный объект или null</returns>
    public static T GetItemIfExists<T>(string[] keys, CachePersistance persistance, int lockTimeout)
      where T : class
    {
      InternalCache ic = GetMainObj(persistance == CachePersistance.MemoryOnly ? GetMainObjMode.DontCreate : GetMainObjMode.Create);
      if (ic == null)
        return null;
      return ic.GetItemIfExists<T>(keys, persistance, lockTimeout);
    }

    /// <summary>
    /// Очистка данных выбранного типа.
    /// Если для данного типа данных еще не было вызова GetItem(), никаких действий не выполняется.
    /// Рекомендуется вызывать нетипизированную версию метода Clear() с аргументом objType.
    /// </summary>
    /// <typeparam name="T">Тип очищаемых данных</typeparam>
    /// <param name="keys">Ключи могут задавать либо точный объект, либо подмножество, подлежащее удалению</param>
    public static void Clear<T>(string[] keys)
      where T : class
    {
      Clear(typeof(T), keys);
    }


    /// <summary>
    /// Очистка данных выбранного типа.
    /// Если для данного типа данных еще не было вызова GetItem(), никаких действий не выполняется
    /// </summary>
    /// <param name="objType">Тип очищаемых данных</param>
    /// <param name="keys">Ключи могут задавать либо точный объект, либо подмножество, подлежащее удалению</param>
    public static void Clear(Type objType, string[] keys)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      InternalCache MainObj = GetMainObj(GetMainObjMode.CreateIfPersistDir);
      if (MainObj != null)
        MainObj.Clear(objType, keys, true);
    }

    /// <summary>
    /// Очистка всех хранящихся данных
    /// Метод используется для реализации команды "Сброс буферизации данных" или аналогичной
    /// Если IsActive=false, никаких действий не выполняется.
    /// Если задан каталог для хранения устойчивых данных (свойство CacheParams.PersistDir),
    /// то он рекурсивно очищается. Это необходимо, так как в каталоге могут быть файлы для типов кэшируемых объектов,
    /// которые еще не использовались в текущем сеансе работы, но которые также становятся неактуальными.
    /// </summary>
    public static void Clear()
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.CreateIfPersistDir);
      if (MainObj != null)
        MainObj.Clear(true);
    }

    /// <summary>
    /// Освобождение оперативной памяти.
    /// Выполняются те же действия, что и при нехватке оперативной памяти, только более полно, не учитывая наличие свободной памяти
    /// Данные в режиме MemoryOnly очищаются.
    /// Данные в режиме MemoryAndTempDir записываются на диск и удаляются из памяти.
    /// Данные в режиме MemoryAndPersist удаляются из памяти но остаются на диске.
    /// 
    /// Этот метод вряд ли стоит вызывать из прикладного кода.
    /// </summary>
    public static void FreeMemory()
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.FreeMemory();
    }


    /// <summary>
    /// Вспомогательный метод замены буферизованных данных.
    /// Выполняет очистку существующей записи кэша и замену объекта
    /// Обычно следует использовать метод GetItem(), передавая ему фабрику для загрузки данных по необходимости.
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="newValue">Объект, который надо поместить в кэш</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static void SetItem<T>(string[] keys, CachePersistance persistance, T newValue)
      where T : class
    {
      if (newValue == null)
        throw new ArgumentNullException("newValue");

      GetMainObj(GetMainObjMode.Create).SetItem<T>(keys, persistance, newValue, Params.LockTimeout);
    }

    /// <summary>
    /// Вспомогательный метод записи буферизованных данных, если их еще не существует.
    /// Если данные уже есть в кэше, они не заменяются.
    /// Имеет смысл использовать вместо SetItem() в режимах <paramref name="persistance"/>, отличных от MemoryOnly, 
    /// если данные являются редко меняющимися и есть вероятность того, что кэш записывается в нескольких местах приложения.
    /// Эквивалентно вызову: if(GetItemIfExists(...)!=null) SetItem(...);, 
    /// но выполняется быстрее, т.к. данные не считываются из файла, а только проверяется существание файла.
    /// Этот метод перехватывет исключения. 
    /// Используется тайм-аут в 1 мс, независимо от установок CacheParams.LockTimeout.
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="newValue">Объект, который надо поместить в кэш</param>
    /// <returns>True, если данные были помещены в кэш. false, если данные уже были или возникла ошибка</returns>
    [DebuggerStepThrough]
    public static bool SetItemIfNew<T>(string[] keys, CachePersistance persistance, T newValue)
      where T : class
    {
      if (newValue == null)
        throw new ArgumentNullException("newValue");

      bool res;
      try
      {
        res = GetMainObj(GetMainObjMode.Create).SetItemIfNew<T>(keys, persistance, newValue, 1);
      }
      catch
      {
        res = false;
      }
      return res;
    }

    /// <summary>
    /// Вспомогательный метод замены буферизованных данных.
    /// Выполняет очистку существующей записи кэша и замену объекта
    /// Обычно следует использовать метод GetItem(), передавая ему фабрику для загрузки данных по необходимости.
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="newValue">Объект, который надо поместить в кэш</param>
    /// <param name="lockTimeout">Максимальное время ожидания блокировки страницы кэша в миллисекундах. 
    /// Значение должно быть больше нуля или равно System.Threading.Timeout.Infinite</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static void SetItem<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
      where T : class
    {
      if (newValue == null)
        throw new ArgumentNullException("newValue");

      GetMainObj(GetMainObjMode.Create).SetItem<T>(keys, persistance, newValue, lockTimeout);
    }



    /// <summary>
    /// Возвращает true, если система буферизации в данный момент активна.
    /// Так как обращение к кэшу является многопоточным, этот метод можно использовать только в отладочных целях
    /// </summary>
    public static bool IsActive
    {
      get
      {
        //return !Object.ReferenceEquals(_MainObjRef, null); // можно без блокировки
        // 27.12.2020
        return !Object.ReferenceEquals(_MainObjRef.Target, null); // можно без блокировки
      }
    }

    /// <summary>
    /// Получение статистики работы кэша для заданного типа данных
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой объект статистики
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить статистику</param>
    /// <returns>Объект со счетчиками статистики</returns>
    public static CacheStat GetStat(Type objType)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj == null)
        return new CacheStat();
      else
        return MainObj.GetStat(objType);
    }

    /// <summary>
    /// Получение статистики работы кэша по всем типам данных, для которых было хотя бы одно обращение к
    /// методу GetItem().
    /// Если не было ни одного вызова GetItem(), возвращаются пустые массивы.
    /// Статистика возвращается и для тех типов данных, для которых сейчас нет хранящихся данных
    /// </summary>
    /// <param name="objTypes">Сюда записывается массив типов хранимых данных, для которых было обращение GetItem()</param>
    /// <param name="stats">Массив статистики по каждому типу. Длина массива равна <paramref name="objTypes"/></param>
    public static void GetStat(out Type[] objTypes, out CacheStat[] stats)
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj == null)
      {
        objTypes = new Type[0];
        stats = new CacheStat[0];
      }
      else
        MainObj.GetStat(out objTypes, out stats);
    }

    /// <summary>
    /// Получение итоговой статистики обращений ко всем типам объектов.
    /// Если система буферизации еще не запущена, возвращается пустая статистика
    /// </summary>
    /// <returns>Объект статистики</returns>
    public static CacheStat GetStat()
    {
      Type[] ObjTypes;
      CacheStat[] Stats;
      GetStat(out ObjTypes, out Stats);
      CacheStat Res = new CacheStat();
      for (int i = 0; i < Stats.Length; i++)
        Res.Add(Stats[i]);
      return Res;
    }

    /// <summary>
    /// Получение списка ключей для буфезованных данных заданного типа.
    /// Первый индекс возвращаемого массива соответствует хранимым объектам.
    /// Второй индекс соответствует ключам, используемым при вызове GetItem().
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой массив.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить ключи</param>
    /// <returns>Массив ключей</returns>
    public static string[][] GetKeys(Type objType)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      List<string[]> KeyList = new List<string[]>();
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.GetKeys(objType, KeyList);
      return KeyList.ToArray();
    }

    /// <summary>
    /// Получение списка ключей для буфезованных данных заданного типа.
    /// Возвращается текст в формате CSV.
    /// Каждая строка соответствует одному хранимому объекту.
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой текст.
    /// Метод используется в отладочных целях.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить ключи</param>
    /// <returns>Текстовое представление ключей</returns>
    public static string GetKeysCommaString(Type objType)
    {
      StringBuilder sb = new StringBuilder();
      GetKeysCommaString(sb, objType);
      return sb.ToString();
    }

    /// <summary>
    /// Получение списка ключей для буфезованных данных заданного типа.
    /// Возвращается текст в формате CSV.
    /// Каждая строка соответствует одному хранимому объекту.
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой текст.
    /// Метод используется в отладочных целях.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить ключи</param>
    /// <param name="sb">Сюда записывается текстовое представление ключей</param>
    public static void GetKeysCommaString(StringBuilder sb, Type objType)
    {
      //List<string[]> KeyList = new List<string[]>();
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.GetKeysCommaString(objType, sb);
    }

    /// <summary>
    /// Установка версии для ветви кэша.
    /// Контроль версий предназначен, в основном, для элементов, которые записываются в постоянный каталог.
    /// При установке версии выполняется очистка ветви кэша, если новая версия отличается от текущей.
    /// Все элементы кэща с уровнем ключей, больше заданного, будут иметь такую же версию кэша
    /// До вызова метода считается, что ветвь кэша имеет версию "".
    /// Используются только строковые версии. При необходимости работы с версиями других типов, они должны быть
    /// преобразованы в строку. Система кэширования проверяет версии только на равенство, не определяется, является
    /// ли версия "новее" или "старше".
    /// Заданная версия сохраняется до конца работы AppDomain и переживает даже полную очистку кэша методом Clear().
    /// Следует избегать задания версий для слишком большого количества ключей, так как версии занимают память.
    /// Порядок ключей для объектов следует определять так, чтобы версию можно было задавать для ключа верхнего уровня.
    /// </summary>
    /// <param name="objType">Тип хранимых данных</param>
    /// <param name="keys">Массив ключей для ветви кэша. Может быть пустым массивом. 
    /// Длина массива должна быть всегда одинаковой для заданного типа</param>
    /// <param name="version">Версия. Не может быть пустой строкой</param>
    public static CacheSetVersionResult SetVersion(Type objType, string[] keys, string version)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");
      if (String.IsNullOrEmpty(version))
        throw new ArgumentNullException("version");

      return GetMainObj(GetMainObjMode.Create).SetVersion(objType, keys, version);
    }

    /// <summary>
    /// Синхронизированная установка версии для ветви кэша.
    /// Используется, когда есть несколько типов кэшируемых данных, относящихся к одному источнику (например, кэши страниц классификатора адресов ФИАС).
    /// Позволяет обойтись единственным файлом version.txt на диске.
    /// Сначала вызывается SetVersion() для главного типа данных. Затем вызывается SyncVersion(), вместо SetVersion(), для остальных типов данных.
    /// Это выполняется и при начальной инициализации и при возможном обновлении версии источника.
    /// </summary>
    /// <param name="objType">Тип хранимых данных ("дополнительный" тип)</param>
    /// <param name="keys">Массив ключей для ветви кэша. Может быть пустым массивом.</param>
    /// <param name="setResult">Результат вызова SetVersion() для "основного" типа данных</param>
    public static void SyncVersion(Type objType, string[] keys, CacheSetVersionResult setResult)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");
      if (setResult == null)
        throw new ArgumentNullException("setResult");

      GetMainObj(GetMainObjMode.Create).SyncVersion(objType, keys, setResult);
    }

    /// <summary>
    /// Возвращает версию, установленную для ветви кэша.
    /// Длина массива ключей может быть больше, чем использованная при установке версии.
    /// При этом для дочерних уровней возвращается версия, заданная для базового уровня.
    /// Если метод SetVersion() или SyncVersion() не вызывался, возвращается пустая строка.
    /// Наличие и содержимое файла version.txt не учитывается.
    /// </summary>
    /// <param name="objType">Тип хранимых данных</param>
    /// <param name="keys">Массив ключей для ветви кэша. Может быть пустым массивом</param>
    /// <returns>Версия</returns>
    public static string GetVersion(Type objType, string[] keys)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        return MainObj.GetVesion(objType, keys);
      else
        return string.Empty;
    }

    #endregion

    #region Экземпляр InternalCache

    private enum GetMainObjMode
    {
      Create,
      DontCreate,
      CreateIfPersistDir
    }

    /// <summary>
    /// Ссылка на единственный экземпляр объекта
    /// При обращении выполняется блокировка lock (typeof(InternalCache))
    /// </summary>
    private static AutoDisposeReference<InternalCache> _MainObjRef;

    private static InternalCache GetMainObj(GetMainObjMode mode)
    {
      lock (typeof(InternalCache))
      {
        InternalCache Res = _MainObjRef.Target;
        if (Res == null)
        {
          if (mode == GetMainObjMode.CreateIfPersistDir)
            mode = _StaticParams.PersistDir.IsEmpty ? GetMainObjMode.DontCreate : GetMainObjMode.Create;

          if (mode == GetMainObjMode.Create)
          {
            Res = new InternalCache(_StaticParams);
            _MainObjRef = new AutoDisposeReference<InternalCache>(Res);
            _StartTime = DateTime.Now;
          }
        }
        return Res;
      }
    }

    /// <summary>
    /// Возвращает время запуска системы кэширования или null, если она еще не запущена
    /// </summary>
    public static DateTime? StartTime
    {
      get
      {
        lock (typeof(InternalCache))
        {
          return _StartTime;
        }
      }
    }
    private static DateTime? _StartTime;

    #endregion

    #region Освобождение при нехватке оперативной памяти

    /// <summary>
    /// Выполнить проверку доступной оперативной памяти и удалить, при необходимости, часть или все элементы кэша.
    /// Обычно эти действия выполняются по таймеру с интервалом, определяемым свойством Params.CheckMemoryInterval,
    /// но может потребоваться выполнить внеплановую проверку.
    /// </summary>
    public static void CheckMemory()
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.CheckMemory();
    }


    /// <summary>
    /// Возвращает статистику вызовов метода CheckMemory().
    /// При каждом обращении к методу возвращается новый объект
    /// </summary>
    public static CacheCheckMemoryStat GetCheckMemoryStat()
    {
      CacheCheckMemoryStat res;
      lock (_CheckMemoryStat)
      {
        res = _CheckMemoryStat.Clone();
      }
      return res;
    }

    /// <summary>
    /// Текущая статистика по CheckMemory()
    /// Объект также используется для блокирования в CheckMemory()
    /// </summary>
    private static CacheCheckMemoryStat _CheckMemoryStat = new CacheCheckMemoryStat();

    #endregion

    #region Трассировка

    /// <summary>
    /// Управляет трассировкой работы с кэшем
    /// </summary>
    public static readonly BooleanSwitch TraceSwitch = new BooleanSwitch("TraceCache", "Трассировка создания / удаления элементов кэша и сброса/загрузки с диска");

    private static string GetTracePrefix(Type type, string[] keys)
    {
      StringBuilder sbTrace = new StringBuilder();
      sbTrace.Append("Cache <");
      sbTrace.Append(type.ToString());
      sbTrace.Append("> Keys [");
      if (keys != null)
      {
        for (int i = 0; i < keys.Length; i++)
        {
          if (i > 0)
            sbTrace.Append(",");
          sbTrace.Append(keys[i]);
        }
      }
      sbTrace.Append("]. ");
      return sbTrace.ToString();
    }

    #endregion

    #region log-файл

    internal static void LogoutCache(LogoutInfoNeededEventArgs args)
    {
      args.WriteHeader("Cache");
      args.WriteLine("CacheParams");
      args.IndentLevel++;
      LogoutTools.LogoutObject(args, Cache.Params);
      //Args.WritePair("StartTime", Cache.StartTime.ToString());
      //Args.WritePair("Capacity", Cache.Params.Capacity.ToString());
      //Args.WritePair("TempDir", Cache.Params.TempDir.ToString());
      //Args.WritePair("PersistDir", Cache.Params.PersistDir.ToString());
      args.IndentLevel--;

      Type[] ObjTypes;
      CacheStat[] Stats;
      Cache.GetStat(out ObjTypes, out Stats);
      CacheStat Total = new CacheStat();
      for (int i = 0; i < ObjTypes.Length; i++)
      {
        args.WriteLine(ObjTypes[i].FullName);
        LogoutCacheStat(args, Stats[i]);
        Total.Add(Stats[i]);
      }
      args.WriteLine("Общая статистика");
      LogoutCacheStat(args, Total);
      args.WriteLine("Статистика по CheckMemory");
      args.IndentLevel++;
      foreach (KeyValuePair<CacheCheckMemoryStatParam, long> Pair in Cache.GetCheckMemoryStat())
        args.WritePair(Pair.Key.ToString().ToString(), Pair.Value.ToString().PadRight(8));
      args.IndentLevel--;
    }

    private static void LogoutCacheStat(LogoutInfoNeededEventArgs args, CacheStat stat)
    {
      args.IndentLevel++;
      foreach (KeyValuePair<CacheStatParam, long> Pair in stat)
      {
        string s = Pair.Value.ToString().PadRight(8);
        switch (Pair.Key)
        {
          case CacheStatParam.CreateCount:
          case CacheStatParam.LoadFileCount:
          case CacheStatParam.FromMemCount:
            if (stat[CacheStatParam.AccessCount] > 0)
            {
              s += " (" + (Pair.Value * 100 / stat[CacheStatParam.AccessCount]).ToString().PadLeft(3) + "%)";
            }
            break;
        }
        args.WritePair(Pair.Key.ToString(), s);
      }
      args.IndentLevel--;
    }

    #endregion
  }

  #region Перечисление CacheStatParam

  /// <summary>
  /// Индексатор параметров статистики
  /// </summary>
  public enum CacheStatParam
  {
    // При добавлении элементов не забыть исправить константу CacheStat.ParamCount !


    /// <summary>
    /// Количество буферизованных в настоящий момент объектов.
    /// Может быть 0, если все объекты были удалены из буфера
    /// </summary>
    Count,

    /// <summary>
    /// Количество добавленных в буфер объектов
    /// </summary>
    AddCount,

    /// <summary>
    /// Количество удаленных из буфера объектов.
    /// Включает объекты, явно удаленные вызовом Clear(), и вытесненные в связи с переполнением буфера
    /// </summary>
    RemoveCount,

    /// <summary>
    /// Количество объектов, удаленных из буфера по причине нехватки памяти (из общего количества RemoveCount).
    /// </summary>
    LowMemoryRemoveCount,

    /// <summary>
    /// Количество вызовов GetItem()
    /// </summary>
    AccessCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которые находились в оперативной памяти
    /// </summary>
    FromMemCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которых не было в буфере, и которые пришлось создавать
    /// </summary>
    CreateCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которые были загружены с диска
    /// </summary>
    LoadFileCount,

    /// <summary>
    /// Количество попыток загрузки объектов с диска, закончившихся неудачей. Такие объекты пришлось 
    /// создать повторно
    /// </summary>
    LoadFileErrors,

    /// <summary>
    /// Количество записанных файлов на диск
    /// </summary>
    SaveFileCount,

    /// <summary>
    /// Количество записанных файлов на диск по причине нехватки памяти
    /// </summary>
    LowMemorySaveFileCount,

    /// <summary>
    /// Количество ошибок, возникших при записи файлов на диск
    /// </summary>
    SaveFileErrors,

    /// <summary>
    /// Количество выполненных удалений буферизованных данных с диска
    /// Включает объекты, явно удаленные вызовом Clear(), и вытесненные в связи с переполнением буфера
    /// </summary>
    DelFileCount,

    /// <summary>
    /// Количество ошибок, возникших при попытке удаления файлов, например, при обрашении из разных потоков
    /// </summary>
    DelFileErrors,

    /// <summary>
    /// Количество элементов, помещенных в очередь на удаление.
    /// Обычно элементы удаляются немедленно методом Cache.Clear(), но некоторые могут быть в этот момент заблокированы.
    /// Тогда они удаляются асинхронно по таймеру.
    /// </summary>
    DelayedDelStarted,

    /// <summary>
    /// Количество элементов, которые были помещены в очередь на удаление и успешно удалены по таймеру
    /// </summary>
    DelayedDelFinished,
  }

  #endregion

  /// <summary>
  /// Объект статистики кэширования для одного типа данных, или итоговой статистики по всем типам.
  /// Объекты возвращаются методом Cache.GetStat()
  /// Объект содержит коллекцию числовых значений статистики, индексируемых перечислением CacheStatParam.
  /// Сам по себе этот класс не является потокобезопасным в части записи, но методы Cache.GetStat() возвращают каждый раз новые экземпляры объектов.
  /// </summary>
  [Serializable]
  public sealed class CacheStat : ICloneable, IEnumerable<KeyValuePair<CacheStatParam, long>>
  {
    internal const int ParamCount = (int)(CacheStatParam.DelayedDelFinished) + 1;

    #region Конструктор

    /// <summary>
    /// Создает новый объект с нулевыми значениями
    /// </summary>
    public CacheStat()
    {
      _Items = new long[ParamCount];
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получить значение счетчика
    /// </summary>
    /// <param name="statParam">Идентификатор счетчик</param>
    /// <returns>Значение</returns>
    public long this[CacheStatParam statParam]
    {
      get
      {
        return _Items[(int)statParam];
      }
    }

    private long[] _Items;

    #endregion

    #region Математические действия

    /// <summary>
    /// Добавляет значения к текущему объекту
    /// </summary>
    /// <param name="src">Добавляемый объект</param>
    public void Add(CacheStat src)
    {
      unchecked
      {
        for (int i = 0; i < ParamCount; i++)
          _Items[i] += src._Items[i];
      }
    }

    /// <summary>
    /// Вычитает значения из текущего объекта
    /// </summary>
    /// <param name="src">Добавляемый объект</param>
    public void Substract(CacheStat src)
    {
      unchecked
      {
        for (int i = 0; i < ParamCount; i++)
          _Items[i] -= src._Items[i];
      }
    }

    /// <summary>
    /// Увеличивет на 1 значение выбранного параметра
    /// </summary>
    /// <param name="statParam">Изменяемый параметр</param>
    internal void Inc(CacheStatParam statParam)
    {
      unchecked
      {
        _Items[(int)statParam]++;
      }
    }

    /// <summary>
    /// Уменьшает на 1 значение выбранного параметра
    /// </summary>
    /// <param name="statParam">Изменяемый параметр</param>
    internal void Dec(CacheStatParam statParam)
    {
      unchecked
      {
        _Items[(int)statParam]--;
      }
    }

    /// <summary>
    /// Складывает два набора статистики и помещает их в новый набор.
    /// Исходные наборы не меняются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат сложения</returns>
    public static CacheStat operator +(CacheStat a, CacheStat b)
    {
      CacheStat res = a.Clone();
      res.Add(b);
      return res;
    }

    /// <summary>
    /// Вычитает один набор статистики из другого и помещает результат в новый набор.
    /// Исходные наборы не меняются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат вычитания</returns>
    public static CacheStat operator -(CacheStat a, CacheStat b)
    {
      CacheStat res = a.Clone();
      res.Substract(b);
      return res;
    }

    #endregion

    #region ICloneable Members

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Копия</returns>
    public CacheStat Clone()
    {
      CacheStat Res = new CacheStat();
      Res.Add(this);
      return Res;
    }

    #endregion

    #region IEnumerable<KeyValuePair<CacheStatParam,long>> Members

    /// <summary>
    /// Перечислитель
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<CacheStatParam, long>>
    {
      #region Конструктор

      internal Enumerator(CacheStat owner)
      {
        _Owner = owner;
        _Index = -1;
      }

      #endregion

      #region Поля

      private CacheStat _Owner;

      private int _Index;

      #endregion

      #region Методы перечислителя

      /// <summary>
      /// Текущее значение
      /// </summary>
      public KeyValuePair<CacheStatParam, long> Current
      {
        get { return new KeyValuePair<CacheStatParam, long>((CacheStatParam)_Index, _Owner[(CacheStatParam)_Index]); }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      /// <summary>
      /// Переход к следующему элементу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _Index++;
        return _Index < CacheStat.ParamCount;
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель, в котором доступны пары Параметр:Значение
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<KeyValuePair<CacheStatParam, long>> IEnumerable<KeyValuePair<CacheStatParam, long>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }

  #region Перечисление CacheCheckMemoryStatParam

  /// <summary>
  /// Статистические показатели, собираемые в CacheCheckMemoryStat
  /// </summary>
  [Serializable]
  public enum CacheCheckMemoryStatParam
  {
    // При добавлении элементов не забыть исправить константу CacheStat.ParamCount !


    /// <summary>
    /// Общее количество вызовов метода Cache.CheckMemory()
    /// </summary>
    CallCount,

    /// <summary>
    /// Количество вызовов, при которых MemoryTools.CheckSufficientMemory() вернул false
    /// </summary>
    InsufficientMemoryTotalCount,

    /// <summary>
    /// Количество InsufficientMemoryTotalCount, которые удалось исправить
    /// </summary>
    InsufficientMemoryResolvedCount,

    /// <summary>
    /// Количество вызовов, при которых MemoryTools.MemoryLoad вернуло значение, большее Params.CriticalMemoryLoad.
    /// </summary>
    MemoryLoadTotalCount,

    /// <summary>
    /// Количество MemoryLoadTotalCount, которые удалось исправить
    /// </summary>
    MemoryLoadResolvedCount,
  }

  #endregion

  /// <summary>
  /// Статистика вызовов метода Cache.CheckMemory()
  /// </summary>
  [Serializable]
  public sealed class CacheCheckMemoryStat : ICloneable, IEnumerable<KeyValuePair<CacheCheckMemoryStatParam, long>>
  {
    #region Константа

    internal const int ParamCount = (int)(CacheCheckMemoryStatParam.MemoryLoadResolvedCount) + 1;

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает объект статистики с одними нулями
    /// </summary>
    public CacheCheckMemoryStat()
    {
      _Items = new long[ParamCount];
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получить значение счетчика
    /// </summary>
    /// <param name="statParam">Счетчик</param>
    /// <returns>Значение</returns>
    public long this[CacheCheckMemoryStatParam statParam]
    {
      get
      {
        return _Items[(int)statParam];
      }
    }

    private long[] _Items;

    #endregion

    #region Математические действия

    /// <summary>
    /// Добавляет значения к текущему объекту
    /// </summary>
    /// <param name="src">Добавляемый объект</param>
    public void Add(CacheCheckMemoryStat src)
    {
      unchecked
      {
        for (int i = 0; i < ParamCount; i++)
          _Items[i] += src._Items[i];
      }
    }

    /// <summary>
    /// Вычитает значения из текущего объекта
    /// </summary>
    /// <param name="src">Добавляемый объект</param>
    public void Substract(CacheCheckMemoryStat src)
    {
      unchecked
      {
        for (int i = 0; i < ParamCount; i++)
          _Items[i] -= src._Items[i];
      }
    }

    /// <summary>
    /// Увеличивет на 1 значение выбранного параметра
    /// </summary>
    /// <param name="statParam">Изменяемый параметр</param>
    internal void Inc(CacheCheckMemoryStatParam statParam)
    {
      unchecked
      {
        _Items[(int)statParam]++;
      }
    }

    ///// <summary>
    ///// Уменьшает на 1 значение выбранного параметра
    ///// </summary>
    ///// <param name="statParam">Изменяемый параметр</param>
    //internal void Dec(CacheCheckMemoryStatParam statParam)
    //{
    //  unchecked
    //  {
    //    _Items[(int)statParam]--;
    //  }
    //}

    /// <summary>
    /// Складывает два набора статистики и помещает их в новый набор.
    /// Исходные наборы не меняются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат сложения</returns>
    public static CacheCheckMemoryStat operator +(CacheCheckMemoryStat a, CacheCheckMemoryStat b)
    {
      CacheCheckMemoryStat res = a.Clone();
      res.Add(b);
      return res;
    }

    /// <summary>
    /// Вычитает один набор статистики из другого и помещает результат в новый набор.
    /// Исходные наборы не меняются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат вычитания</returns>
    public static CacheCheckMemoryStat operator -(CacheCheckMemoryStat a, CacheCheckMemoryStat b)
    {
      CacheCheckMemoryStat res = a.Clone();
      res.Substract(b);
      return res;
    }

    #endregion

    #region ICloneable Members

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Копия</returns>
    public CacheCheckMemoryStat Clone()
    {
      CacheCheckMemoryStat Res = new CacheCheckMemoryStat();
      Res.Add(this);
      return Res;
    }

    #endregion

    #region IEnumerable<KeyValuePair<CacheCheckMemoryStatParam,long>> Members

    /// <summary>
    /// Перечислитель
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<CacheCheckMemoryStatParam, long>>
    {
      #region Конструктор

      internal Enumerator(CacheCheckMemoryStat owner)
      {
        _Owner = owner;
        _Index = -1;
      }

      #endregion

      #region Поля

      private CacheCheckMemoryStat _Owner;

      private int _Index;

      #endregion

      #region Методы перечислителя

      /// <summary>
      /// Текущий элемента
      /// </summary>
      public KeyValuePair<CacheCheckMemoryStatParam, long> Current
      {
        get { return new KeyValuePair<CacheCheckMemoryStatParam, long>((CacheCheckMemoryStatParam)_Index, _Owner[(CacheCheckMemoryStatParam)_Index]); }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      /// <summary>
      /// Переход к следующему значению
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _Index++;
        return _Index < CacheCheckMemoryStat.ParamCount;
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель, в котором доступны пары Параметр:Значение
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<KeyValuePair<CacheCheckMemoryStatParam, long>> IEnumerable<KeyValuePair<CacheCheckMemoryStatParam, long>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }
}

#else //! OLD_CACHE
namespace FreeLibSet.Caching
{
  /*
   * Буферизация объектов в оперативной памяти и/или в файлах на диске
   */

#region Перечисление CachePersistance

  /// <summary>
  /// Способ хранение элемента кэша
  /// </summary>
  public enum CachePersistance
  {
    /// <summary>
    /// Элемент хранится только в оперативной памяти.
    /// В случае нехватки памяти элемент удаляется и требуется повторное создание данных
    /// </summary>
    MemoryOnly = 1,

    /// <summary>
    /// Копия элемента хранится во временном каталоге в течение сеанса работы приложения.
    /// Элемент должен быть сериализуемым
    /// </summary>
    MemoryAndTempDir = 3,

    /// <summary>
    /// Даннные хранятся только во временном каталоге.
    /// Этот вариант следует использовать для элементов большого объема, которые нужны сравнительно редко.
    /// Элемент должен быть сериализуемым
    /// </summary>
    TempDirOnly = 2,

    /// <summary>
    /// Данные хранятся в оперативной памяти, копия - в специально отведенном каталоге.
    /// Данные сохраняются между сеансами работы.
    /// Элемент должен быть сериализуемым.
    /// </summary>
    MemoryAndPersist = 5,

    /// <summary>
    /// Данные хранятся в специально отведенном каталоге, но не в оперативной памяти.
    /// Данные сохраняются между сеансами работы.
    /// Элемент должен быть сериализуемым.
    /// </summary>
    PersistOnly = 4
  }

  #endregion

  /// <summary>
  /// Интерфейс, который должен реализовываться в прикладном коде для создания данных кэша
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ICacheFactory<T>
    where T : class
  {
    /// <summary>
    /// Создает новый элемент кэша
    /// </summary>
    /// <param name="keys">Ключи</param>
    /// <returns>Элемент кэша</returns>
    T CreateCacheItem(string[] keys);
  }

  /// <summary>
  /// Параметры кэша
  /// </summary>
  public class CacheParams : IReadOnlyObject
  {
#region Конструктор

    /// <summary>
    /// Создает объект параметра со значениями по умолчанию
    /// </summary>
    internal CacheParams()
    {
      _Capacity = 10000;
      _CheckMemoryInterval = 60000;
      _CriticalMemoryLoad = 70;
      _ClearCacheBlockSize = 100;
      //_LockTimeout = 60000;
      _LockTimeout = System.Threading.Timeout.Infinite;

      _SerializationBufferBytesLimit = IntPtr.Size == 4 ? (64L * FileTools.MByte) : (2L * FileTools.GByte);
      long sbl1 = MemoryTools.TotalPhysicalMemory;
      if (sbl1 > 0L)
        _SerializationBufferBytesLimit = Math.Min(_SerializationBufferBytesLimit, sbl1 / 10L);

      _DelayedWriteQueueCapacity = 1000;
    }

    #endregion

#region Параметры

    /// <summary>
    /// Максимальное число элементов, которые могут хранится в кэше.
    /// По умолчанию - 10000 элементов
    /// </summary>
    public int Capacity
    {
      get { return _Capacity; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", value, "Значение не можеть быть меньше 1");

        lock (this)
        {
          CheckNotReadOnly();
          _Capacity = value;
        }
      }
    }
    private int _Capacity;

    /// <summary>
    /// Каталог, используемый для хранения элементов со способом хранения MemoryAndPersist и PeristOnly.
    /// 
    /// Внимание! Каталог не должен использоваться ни для каких других целей, кроме хранения кэша.
    /// Метод Cache.Clear() рекурсивно очищает все файлы в каталоге.
    /// Будьте внимательны при установке свойства во избежание утери файлов.
    /// 
    /// По умолчанию каталог не задан, использовать указанные способы хранения нельзя.
    /// </summary>
    public AbsPath PersistDir
    {
      get { return _PersistDir; }
      set
      {
        lock (this)
        {
          CheckNotReadOnly();
          _PersistDir = value;
        }
      }
    }
    private AbsPath _PersistDir;

    /// <summary>
    /// Путь ко временному каталогу
    /// Свойство автоматически устанавливается при инициализации кэша
    /// </summary>
    public AbsPath TempDir
    {
      get { return _TempDir; }
      internal set
      {
        _TempDir = value;
      }
    }
    private AbsPath _TempDir;

    /// <summary>
    /// Периодичность опроса наличной памяти по таймеру в миллисекундах.
    /// По умолчанию - 60000 (1 раз в минуту).
    /// Значение Timeout.Infinite отключает использование таймера
    /// </summary>
    public int CheckMemoryInterval
    {
      get { return _CheckMemoryInterval; }
      set
      {
        CheckNotReadOnly();
        _CheckMemoryInterval = value;
      }
    }
    private int _CheckMemoryInterval;

    /// <summary>
    /// Процент занятой физической памяти, при котором начинается запись в файл элементом со способом
    /// хранения MemoryAndTempDir и освобождение памяти для элементов MemoryOnly и MemoryAndPersist.
    /// По умолчанию - 70%
    /// </summary>
    public int CriticalMemoryLoad
    {
      get { return _CriticalMemoryLoad; }
      set
      {
        CheckNotReadOnly();
        if (value < 0 || value > 100)
          throw new ArgumentOutOfRangeException();
        _CriticalMemoryLoad = value;
      }
    }
    private int _CriticalMemoryLoad;

    /// <summary>
    /// Количество элементов, освобождаемых за один раз, когда обнаруживается нехватка памяти.
    /// По умолчанию - 100 элементов
    /// </summary>
    public int ClearCacheBlockSize
    {
      get { return _ClearCacheBlockSize; }
      set
      {
        CheckNotReadOnly();
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _ClearCacheBlockSize = value;
      }
    }
    private int _ClearCacheBlockSize;

    /// <summary>
    /// Лимит для объектов кэша с Persistance=MemoryAndTempDir.
    /// Для таких элементов в памяти хранятся сериализованные данные данные, готовые для записи во временный файл.
    /// Лимит задает общий размер таких данных, прежде чем они не начнут сбрасываться на диск.
    /// Нулевое значение задает сброс данных, как и для MemoryAndPersisDir.
    /// 
    /// Значение по умолчанию составляет 10% от объема физической памяти, но не более 64Мб/2Гб для 32/64 разрядной среды выполнения
    /// </summary>
    public long SerializationBufferBytesLimit
    {
      get { return _SerializationBufferBytesLimit; }
      set
      {
        CheckNotReadOnly();
        if (value < 0L)
          throw new ArgumentOutOfRangeException();
        _SerializationBufferBytesLimit = value;
      }
    }
    private long _SerializationBufferBytesLimit;


    /// <summary>
    /// Максимальное длина очередей на отложенную запись файлов.
    /// По умолчанию - 1000 элементов.
    /// Если лимит превышен, то запись файла выполняется немедленно в том потоке, который добавляет элемент в кэш.
    /// </summary>
    public int DelayedWriteQueueCapacity
    {
      get { return _DelayedWriteQueueCapacity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", value, "Значение не можеть быть меньше 0");

        lock (this)
        {
          CheckNotReadOnly();
          _DelayedWriteQueueCapacity = value;
        }
      }
    }
    private int _DelayedWriteQueueCapacity;

    /// <summary>
    /// <para>Максимальный интервал времени в миллисекундах, на который может устанавливаться блокировка для получения
    /// страницы кэша. Если за указанный интервал объект не освобожден другим потоком, то генерируется исключение
    /// LockTimeoutException.</para>
    /// <para>Значение System.Threading.Timeout.Infinite (по умолчанию) означает отсутствие таймаута и приведет к вечному зависанию программы.
    /// В любом случае, появление исключения LockTimeoutException означает ошибку в приложении, которую необходимо устранить.</para>
    /// <para>Значение свойства используется для перегрузок методов Cache.GetItem(), GetItemIfExists() и SetItem(),
    /// в которых нет аргумента lockTimeout</para>
    /// </summary>
    public int LockTimeout
    {
      get { return _LockTimeout; }
      set
      {
        CheckNotReadOnly();
        CheckLockTimeOutValue(value);
        _LockTimeout = value;
      }
    }
    private int _LockTimeout;

    internal static void CheckLockTimeOutValue(int lockTimeout)
    {
      if (lockTimeout < 1 && lockTimeout != System.Threading.Timeout.Infinite)
        throw new ArgumentOutOfRangeException("lockTimeout", lockTimeout, "Значение должно быть больше 0 или равна System.Threading.Timeout.Infinite");
    }

    #endregion

#region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если параметры уже присоединены и их нельзя менять
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если параметры нельзя менять
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      lock (this)
      {
        _IsReadOnly = true;
      }
    }

    #endregion
  }

  /// <summary>
  /// Результат, возвращаемый методом Cache.SetVersion(), который нужно передавать методу SyncVersion(), если используется синхронное
  /// управление версиями для нескольких типов объектов.
  /// Большинство полей класса являются внутренними
  /// </summary>
  public sealed class CacheSetVersionResult
  {
#region Защищенный конструктор

    internal CacheSetVersionResult(Type objType, string[] keys, string version, bool persistFilesCleared)
    {
      _ObjType = objType;
      _Keys = keys;
      _Version = version;
      _PersistFilesCleared = persistFilesCleared;
    }

    #endregion

#region Свойства

    internal Type ObjType { get { return _ObjType; } }
    private readonly Type _ObjType;

    internal string[] Keys { get { return _Keys; } }
    private readonly string[] _Keys;

    internal string Version { get { return _Version; } }
    private readonly string _Version;

    /// <summary>
    /// Возвращает true, если были очищены файлы в постоянном каталоге, так как версия изменилась, или это был первый вызов SetVersion
    /// </summary>
    public bool PersistFilesCleared { get { return _PersistFilesCleared; } }
    private readonly bool _PersistFilesCleared;

    #endregion
  }

  /// <summary>
  /// Система буферизации сериализуемых объектов с хранением в памяти и/или на диске
  /// Все public-свойства и методы являются потокобезопасными
  /// </summary>
  public static class Cache
  {
#region Внутренняя реализация

    /// <summary>
    /// Внутренняя реализация кэша
    /// </summary>
    private class InternalCache : DisposableObject
    {
      // Нужно использовать в качестве базового класса DisposableObject, а не SimpleDisposableObject,
      // так как есть таймер и требуется очистка временного каталога

#region Доп. константы для CachePersistance

      /// <summary>
      /// Элемент был только что создан, но еще не инициализирован
      /// </summary>
      private const CachePersistance ItemNotInit = (CachePersistance)0;

      /// <summary>
      /// Элемент был удален
      /// </summary>
      private const CachePersistance ItemDeleted = (CachePersistance)(0x10);

      #endregion

#region Конструктор и Dispose

      public InternalCache(CacheParams cacheParams)
      {
        cacheParams.SetReadOnly();
        _Params = cacheParams;

        _TheTypeDict = new TypeDict();
        _TempDir = new TempDirectory();
        _Params.TempDir = _TempDir.Dir;

        _TypeStats = new Dictionary<Type, CacheStat>();
        _TotalStat = new CacheStat();

        _CheckMemorySyncRoot = new object();
        if (cacheParams.CheckMemoryInterval > 0)
          _CheckMemoryTimer = new Timer(new TimerCallback(CheckMemoryTimer_Tick), null, cacheParams.CheckMemoryInterval, cacheParams.CheckMemoryInterval);

        _DelayedWriteList = new SyncQueue<InternalCacheItem>();
        _DelayedClearList = new SyncQueue<DelayedClearInfo>();

        _StartBackgroundEvent = new ManualResetEvent(false);

        _BackgroundThread = new Thread(BackgroundThreadProc);
        _BackgroundThread.IsBackground = true;
        _BackgroundThread.Name = "CacheBackground";
        _BackgroundThread.Start();
      }

      protected override void Dispose(bool disposing)
      {
        if (disposing)
        {
          _DelayedClearList.Clear();
          _DelayedWriteList.Clear();

          // Завершаем фоновый поток с ожиданием
          _DisposingFlag = true;
          _StartBackgroundEvent.Set();
          _BackgroundThread.Join();
          _BackgroundThread = null;

          _StartBackgroundEvent.Close();
          _StartBackgroundEvent = null;
        }
        if (_CheckMemoryTimer != null)
        {
          _CheckMemoryTimer.Dispose();
          _CheckMemoryTimer = null;
        }
        if (_TempDir != null)
        {
          _TempDir.Dispose();
          _TempDir = null;
        }

        base.Dispose(disposing);
      }

      #endregion

#region Доступ по ключам

      /*
       * Для каждого типа буферизуемых данных используется отдельная коллекция KeyDict.
       * Доступ к конкретному объекту осуществляется по массиву ключей. Массив должен иметь не менее одного ключа
       * Следует создавать для каждой цели отдельный тип хранимых хранимых данных, чтобы ключи не путались, а
       * не использовать, например, DataSet
       *
       * Блокирование.
       * // Коллекция по типам (TheDict) блокируется на время поиска/создания корневого объекта KeyDict для выбранного
       * // типа.
       * // Метод GetInternalItem(), выполняющий поиск/создания страниц KeyDict по ключам и создание объекта 
       * // InternalCacheItem, выполняет блокирование корневой коллекции KeyDict.
       * Не выгодно использовать две блокировки. Лучше использовать одну общую блокировку по по объекту TheDict
       * 
       * После того, как GetInternalItem() вернул объект InternalCacheItem, общая блокировка снимается.
       * Объект InternalCacheItem блокируется на момент проверки наличия данных, создания данных с помощью
       * Factrory и сброса сериализованных данных на диск
       * 
       * После того, как нужные данные получены, проверяется переполнение списка MRU. 
       * Берется самый "старый" элемент. Он блокируется и очищается (удаляется файл на диске).
       * Затем элемент удаляется из цепочки (с блокировкой KeyDict)
       * 
       * Таким образом, при выполнении длительных действий (создание данных, чтение/запись файла), выполняется
       * блокировка только конкретного объекта InternalCacheItem. Блокировка коллекций выполняется кратковременно
       */

      /// <summary>
      /// Коллекция страниц по типам
      /// </summary>
      private readonly TypeDict _TheTypeDict;

      #endregion

#region Параметры

      private readonly CacheParams _Params;

      #endregion

#region MRU-список

      private InternalCacheItem _FirstItem;

      private InternalCacheItem _LastItem;

      //public int Count
      //{
      //  get
      //  {
      //    lock (_TheTypeDict)
      //    {
      //      return _Count;
      //    }
      //  }
      //}
      private int _Count;


      private bool MRUContainsItem(InternalCacheItem item)
      {
        if (item.PrevItem == null && item.NextItem == null)
          return this._FirstItem == item || this._LastItem == item;
        else
          return true;
      }

      private void MRUAdd(InternalCacheItem item)
      {
#if DEBUG
        if (MRUContainsItem(item))
          throw new InvalidOperationException("Повторный вызов MRUAdd()");

        CheckMRUState(item);
#endif

        item.NextItem = this._FirstItem;
        if (this._FirstItem == null)
          this._LastItem = item;
        else
          this._FirstItem.PrevItem = item;
        this._FirstItem = item;

        _Count++;

#if DEBUG
        CheckMRUState(item);
#endif
      }

      private bool MRURemove(InternalCacheItem item)
      {
#if DEBUG
        CheckMRUState(item);
#endif

        if (MRUContainsItem(item))
        {
          if (item.NextItem == null)
            this._LastItem = item.PrevItem;
          else
            item.NextItem.PrevItem = item.PrevItem;
          if (item.PrevItem == null)
            this._FirstItem = item.NextItem;
          else
            item.PrevItem.NextItem = item.NextItem;
          item.NextItem = null;
          item.PrevItem = null;

          _Count--;

#if DEBUG
          CheckMRUState(item);
#endif
          return true;
        }
        else
          return false;
      }


      private void MRUToFirst(InternalCacheItem item)
      {
        if (this._FirstItem == item)
          return; // и так наверху
        // ? Оптимизация
        MRURemove(item);
        MRUAdd(item);
      }

#if DEBUG

      private void CheckMRUState(InternalCacheItem item)
      {
        if (item.PrevItem == null)
        {
          if (item.NextItem != null && this._FirstItem != null && this._FirstItem != item)
            throw new BugException("FirstItem должен указывать на элемент, т.к. Item.PrevItem==null");
        }
        else
        {
          if (this._FirstItem == item)
            throw new BugException("FirstItem не должен указывать на элемент, т.к. Item.PrevItem!=null");
          if (item.PrevItem == item)
            throw new BugException("Item.PrevItem указывает сам на себя");
          if (item.PrevItem.NextItem != item)
            throw new BugException("Item.PrevItem.NextItem != Item");
        }

        if (item.NextItem == null)
        {
          if (item.PrevItem != null && this._LastItem != null && this._LastItem != item)
            throw new BugException("LastItem должен указывать на элемент, т.к. Item.NextItem==null");
        }
        else
        {
          if (this._LastItem == item)
            throw new BugException("LastItem не должен указывать на элемент, т.к. Item.NextItem!=null");
          if (item.NextItem == item)
            throw new BugException("Item.NextItem указывает сам на себя");
          if (item.NextItem.PrevItem != item)
            throw new BugException("Item.NextItem.PrevItem != Item");
        }

        if ((this._FirstItem == null) != (this._LastItem == null))
          throw new BugException("FirstItem и LastItem могут быть null только одновременно");

        if ((item.PrevItem != null || item.NextItem != null) && this._FirstItem == null)
          throw new BugException("Item находится в списке, но список считается пустым");
      }

#endif

      #endregion

#region Доступ к элементам

      private InternalCacheItem GetInternalItem(Type objType, string[] keys, bool create)
      {
        if (objType == null)
          throw new ArgumentNullException("objType");
        if (keys.Length < 1)
          throw new ArgumentException("Длина списка ключей не может быть меньше 1");

        InternalCacheItem Res;

        lock (_TheTypeDict)
        {
#region Вход для типа данных

          TypeInfo ti;
          if (!_TheTypeDict.TryGetValue(objType, out ti))
          {
            if (create)
            {
              ti = new TypeInfo(this, objType);
              _TheTypeDict.Add(objType, ti);
            }
            else
              return null;
          }

          #endregion

#region Создание промежуточных страниц

          KeyDict Dict = ti;

          for (int i = 0; i < (keys.Length - 1); i++)
          {
            object Dict2;
            if (!Dict.TryGetValue(keys[i], out Dict2))
            {
              if (create)
              {
                Dict2 = new KeyDict(this, objType, Dict, keys[i]);
                Dict.Add(keys[i], Dict2);
              }
              else
                return null;
            }
            Dict = (KeyDict)Dict2;
          }

          #endregion

#region Создание InternalCacheItem

          object Res2;
          string LastKey = keys[keys.Length - 1];
          if (Dict.TryGetValue(LastKey, out Res2))
          {
            Res = (InternalCacheItem)Res2;
            MRUToFirst(Res);
          }
          else
          {
            if (create)
            {
              Res = new InternalCacheItem(Dict, LastKey);
              Res.Persistance = ItemNotInit;
              Dict.Add(LastKey, Res);
              // Добавляем в MRU
              MRUAdd(Res);
              IncStat(objType, CacheStatParam.AddCount);
              IncStat(objType, CacheStatParam.ItemCount);
            }
            else
              return null;
          }

          #endregion
        }

        return Res;
      }


      private void TrimExcess()
      {
        InternalCacheItem ItemToDelete = null;

        lock (_TheTypeDict)
        {
          if (_Count > _Params.Capacity)
          {
            ItemToDelete = this._LastItem;
          }
        }

        if (ItemToDelete != null)
          Remove(ItemToDelete, ItemClearMode.MRUOut);
      }

      #endregion

#region Чтение / запись в файл

      /// <summary>
      /// Временный каталог для режимов MemoryAndTempDir и TempDirOnly
      /// </summary>
      private TempDirectory _TempDir;

      /// <summary>
      /// Синхронная запись файла.
      /// Ошибки перехватываются.
      /// </summary>
      /// <param name="isPersistDir"></param>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      /// <returns></returns>
      //[DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      private object LoadFromFile(bool isPersistDir, Type objType, string[] keys)
      {
        try
        {
          AbsPath FilePath = GetBinFilePath(isPersistDir, objType, keys, true);

          if (!File.Exists(FilePath.Path))
            return null;

          object obj;
          FileStream fs = new FileStream(FilePath.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
          try
          {
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(fs);
            IncStat(objType, CacheStatParam.DeserializationCount);
          }
          finally
          {
            fs.Close();
          }

          if (obj.GetType() == objType)
          {
            IncStat(objType, CacheStatParam.LoadFileCount);

            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(objType, keys) + "Loaded from file");

            return obj;
          }
          else
          {
            IncStat(objType, CacheStatParam.LoadFileErrors);
            return null; // Получили какой-то другой объект
          }
        }
        catch
        {
          IncStat(objType, CacheStatParam.LoadFileErrors);
          return null;
        }
      }

      private void WriteSerializedToFile(bool isPersistDir, Type objType, string[] keys, byte[] serializedData)
      {
        try
        {
          AbsPath filePath = GetBinFilePath(isPersistDir, objType, keys, true);
#if DEBUG
          if (serializedData == null)
            throw new ArgumentNullException("serializedData");
#endif

          FileTools.ForceDirs(filePath.ParentDir);

          System.IO.File.WriteAllBytes(filePath.Path, serializedData);
          IncStat(objType, CacheStatParam.SaveFileCount);

          if (Cache.TraceSwitch.Enabled)
            Trace.WriteLine(GetTracePrefix(objType, keys) + "Saved to file " + filePath.FileName);
        }
        catch
        {
          IncStat(objType, CacheStatParam.SaveFileErrors);
        }
      }

      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      private void DeleteFile(bool isPersistDir, Type objType, string[] keys)
      {
        try
        {
          AbsPath FilePath = GetBinFilePath(isPersistDir, objType, keys, true);
          if (File.Exists(FilePath.Path)) // чтобы не нарываться на исключение
          {
            File.Delete(FilePath.Path);
            // Удалять дерево каталогов пока не будем
            IncStat(objType, CacheStatParam.DelFileCount);
            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(objType, keys) + "File removed");
          }
        }
        catch
        {
          IncStat(objType, CacheStatParam.DelFileErrors);
        }
      }

      /// <summary>
      /// Возвращает путь к файлу
      /// </summary>
      /// <param name="isPersistDir">true-постоянный файл, false - временный</param>
      /// <param name="objType">Тип объекта</param>
      /// <param name="keys">Ключи</param>
      /// <param name="isFile">true - файл, false - каталог</param>
      /// <returns>Путь к файлу *.bin или к каталогу</returns>
      private AbsPath GetBinFilePath(bool isPersistDir, Type objType, string[] keys, bool isFile)
      {
        AbsPath rootDir = isPersistDir ? Params.PersistDir : _TempDir.Dir;
#if DEBUG
        if (objType == null)
          throw new ArgumentNullException("objType");
#endif
        AbsPath TypePath = new AbsPath(rootDir, objType.ToString());
        AbsPath FilePath = new AbsPath(TypePath, keys);
        if (isFile)
        {
          if (isPersistDir)
          {
            string version = GetVesion(objType, keys);
            string ExtFileName = FilePath.FileName + "#" + version + ".bin";
            FilePath = new AbsPath(FilePath.ParentDir, ExtFileName);
          }
          else
            FilePath = FilePath.ChangeExtension(".bin");
        }
        return FilePath;
      }

      /// <summary>
      /// Возвращает true, если файл существует
      /// </summary>
      /// <param name="isPersistDir"></param>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      /// <returns></returns>
      private bool TestFileExists(bool isPersistDir, Type objType, string[] keys)
      {
        AbsPath FilePath = GetBinFilePath(isPersistDir, objType, keys, true);
        return File.Exists(FilePath.Path);
      }

      #endregion

#region Основной метод доступа

      internal T GetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory, int lockTimeout)
        where T : class
      {
        WaitCheckMemory(); // 05.01.2021

        // Делаем три попытки с перехватом OutOfMemoryException
        try
        {
          // Первая попытка
          return DoGetItem<T>(keys, persistance, factory, lockTimeout);
        }
        catch (OutOfMemoryException)
        {
          // Первая попытка закончилась неудачно
          CheckMemory(CacheStatParam.CheckMemoryEmergencyCallCount);
          try
          {
            // Вторая попытка
            return DoGetItem<T>(keys, persistance, factory, lockTimeout);
          }
          catch (OutOfMemoryException)
          {
            // Вторая попытка закончилась неудачно
            Clear(false);

            // Третья попытка
            return DoGetItem<T>(keys, persistance, factory, lockTimeout);
          }
        }
      }


      private T DoGetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory, int lockTimeout)
        where T : class
      {
        InternalCacheItem item = GetInternalItem(typeof(T), keys, true);

        T Res;

        //lock (item)
        if (!Monitor.TryEnter(item, lockTimeout))
          ThrowLockTimeoutException(typeof(T), keys); // 21.09.2020
        try
        {
          // Проверка реентрантного вызова должна выполняться внутри блокировки, а не снаружи
          if (item.InsideGetItemFlag)
            throw new ReenteranceException("Вложенный вызов GetItem() для объекта " + typeof(T).ToString() +
              " с ключами {" + String.Join(", ", keys) + "}");

          item.InsideGetItemFlag = true;
          try
          {
            Res = DoGetItem2<T>(item, keys, persistance, factory);
          }
          finally
          {
            item.InsideGetItemFlag = false;
          }
        }
        finally
        {
          Monitor.Exit(item);
        }

        // После того, как данные получены, можно удалить лишние элементы из списка
        TrimExcess();

        return Res;
      }

      private void ThrowLockTimeoutException(Type objType, string[] keys)
      {
        // Нельзя выводить в log-файл исключение, так как при этом снова возникнет блокировка

        //try
        //{
        throw new LockTimeoutException("Тайм-аут установки блокировки на страницу кэша типа " + objType.ToString() + " с ключами {" + String.Join(", ", keys) + "}");
        //}
        //catch (LockTimeoutException e)
        //{
        //  LogoutTools.LogoutException(e, "Немедленный перехват LockTimeoutException в Cache");
        //  throw;
        //}
      }

      /// <summary>
      /// Этот метод вызывается, пока InternalCacheItem заблокирован
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="item"></param>
      /// <param name="keys"></param>
      /// <param name="persistance"></param>
      /// <param name="factory"></param>
      /// <returns></returns>
      private T DoGetItem2<T>(InternalCacheItem item, string[] keys, CachePersistance persistance, ICacheFactory<T> factory)
        where T : class
      {
        T res;

        IncStat(typeof(T), CacheStatParam.AccessCount);

#region Из памяти

        if (item.MemValue != null)
        {
          IncStat(typeof(T), CacheStatParam.FromMemCount);

          return (T)(item.MemValue);
        }

        #endregion

#region Из еще незаписанного файла

        if (item.SerializedData != null)
        {
          res = (T)(SerializationTools.DeserializeBinary(item.SerializedData));
          IncStat(typeof(T), CacheStatParam.FromMemSerializedCount);
          IncStat(typeof(T), CacheStatParam.DeserializationCount);

          if ((persistance & CachePersistance.MemoryOnly) != 0)
            item.MemValue = res; // помещаем обратно ссылку
          return res;
        }

        #endregion

#region Из файла

        bool isPersistDir = (persistance & CachePersistance.PersistOnly) != 0;

        if ((persistance & (CachePersistance.PersistOnly | CachePersistance.TempDirOnly)) != 0)
        {
          res = (T)(LoadFromFile(isPersistDir, typeof(T), keys));
          if (res != null)
          {
            if ((persistance & CachePersistance.MemoryOnly) != 0)
              item.MemValue = res; // "приклеили" данные обратно к элементу

            return res;
          }
        }

        #endregion

        if (factory == null)
          return null;

#region Создаем новые данные

        if (TraceSwitch.Enabled)
        {
          Stopwatch sw = Stopwatch.StartNew();
          res = factory.CreateCacheItem(keys);
          sw.Stop();
          Trace.WriteLine(Cache.GetTracePrefix(typeof(T), keys) + "Item created. Time=" + sw.Elapsed.ToString());
        }
        else
          res = factory.CreateCacheItem(keys);

        if (res == null)
          throw new NullReferenceException("Объект кэша не был создан в " + factory.ToString());

        IncStat(typeof(T), CacheStatParam.CreateCount);

        #endregion

#region Присвоение ссылки в памяти

        if ((persistance & CachePersistance.MemoryOnly) != 0)
          item.MemValue = res;
        else
          item.MemValue = null;

        item.Persistance = persistance;

        #endregion

#region Сериализация для отложенной записи в файл

        if ((persistance & (CachePersistance.TempDirOnly | CachePersistance.PersistOnly)) != 0)
        {
          // Данные всегда нужно сериализовать здесь, а запись в файл можно выполнить когда-нибудь потом
          item.SetSerializedData(this, SerializationTools.SerializeBinary(res));
          IncStat(typeof(T), CacheStatParam.SerializationCount);

          if (persistance != CachePersistance.MemoryAndTempDir || // для этого режима не надо ставить в очередь
            GetStatValue(CacheStatParam.SerializationBufferBytes) > Params.SerializationBufferBytesLimit) // 06.01.2021
          {
            if (_DelayedWriteList.Enqueue(item, Params.DelayedWriteQueueCapacity))
            {
              IncStat(typeof(T), CacheStatParam.DelayedWriteStarted);
              // поставили в очередь. Пробуждаем фоновый поток
              _StartBackgroundEvent.Set();
            }
            else
            {
              // Очередь переполнена.
              // Файл записывается немедленно
              WriteSerializedToFile(isPersistDir, typeof(T), keys, item.SerializedData);
              item.SetSerializedData(this, null); // не зачем занимать память
            }
          }
        }
        else // MemoryOnly
          item.SetSerializedData(this, null);

        #endregion

        return res;
      }

      #endregion

#region Вспомогательные методы доступа

      internal T GetItemIfExists<T>(string[] keys, CachePersistance persistance, int lockTimeout)
        where T : class
      {
        // Делаем три попытки с перехватом OutOfMemoryException

        try
        {
          InternalCacheItem item = GetInternalItem(typeof(T), keys, (persistance & CachePersistance.PersistOnly) != 0);
          if (item == null)
            return null;

          T Res;

          //lock (Item)
          if (!Monitor.TryEnter(item, lockTimeout))
            ThrowLockTimeoutException(typeof(T), keys); // 21.09.2020
          try
          {
            if (item.InsideGetItemFlag)
              Res = null;
            else
              Res = DoGetItem2<T>(item, keys, persistance, null);
          }
          finally
          {
            Monitor.Exit(item);
          }

          return Res;
        }
        catch
        {
          return null;
        }
      }

      internal void SetItem<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
        where T : class
      {
        WaitCheckMemory(); // 05.01.2021

        // Делаем три попытки с перехватом OutOfMemoryException
        try
        {
          // Первая попытка
          DoSetItem<T>(keys, persistance, newValue, lockTimeout);
        }
        catch (OutOfMemoryException)
        {
          // Первая попытка закончилась неудачно
          CheckMemory(CacheStatParam.CheckMemoryEmergencyCallCount);
          try
          {
            // Вторая попытка
            DoSetItem<T>(keys, persistance, newValue, lockTimeout);
          }
          catch (OutOfMemoryException)
          {
            // Вторая попытка закончилась неудачно
            Clear(false);

            // Третья попытка
            DoSetItem<T>(keys, persistance, newValue, lockTimeout);
          }
        }
      }

      private void DoSetItem<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
        where T : class
      {
        // Используем блокировку на время замены, так как между удалением и записью значения,
        // параллельный поток может обратиться к значению и загрузить другое значение с помощью своей фабрики - лишние действия
        InternalCacheItem item = GetInternalItem(typeof(T), keys, true);
        //lock (item)
        if (!Monitor.TryEnter(item, lockTimeout))
          ThrowLockTimeoutException(typeof(T), keys); // 21.09.2020
        try
        {
          // Проверка реентрантного вызова должна выполняться внутри блокировки, а не снаружи
          if (item.InsideGetItemFlag)
            throw new ReenteranceException("Вложенный вызов SetItem() при вызове GetItem() для объекта " + typeof(T).ToString() +
              " с ключами {" + String.Join(", ", keys) + "}");

          // 04.12.2020
          // Не нужно удалять из списка сам элемент
          //Clear(typeof(T), keys, true); // удаляем
          //DoGetItem<T>(keys, persistance, new DummyFactory<T>(newValue), lockTimeout /* Фиктивный параметр, т.к. уже заблокировано */ ); // добавляем

          item.InsideGetItemFlag = true;
          try
          {
            DoClearItemInternal(item, ItemClearMode.Clear, keys);
            DoGetItem2<T>(item, keys, persistance, new DummyFactory<T>(newValue)); // добавляем
          }
          finally
          {
            item.InsideGetItemFlag = false;
          }
        }
        finally
        {
          Monitor.Exit(item);
        }
      }

      internal bool SetItemIfNew<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
        where T : class
      {
        bool res = false;

        InternalCacheItem item = GetInternalItem(typeof(T), keys, true);
        if (!Monitor.TryEnter(item, lockTimeout))
          return false;
        try
        {
          if (!DoGetItemContainsData(item, typeof(T), keys))
          {
            DoGetItem<T>(keys, persistance, new DummyFactory<T>(newValue), lockTimeout /* Фиктивный параметр, т.к. уже заблокировано */ ); // добавляем
            res = true;
          }
        }
        finally
        {
          Monitor.Exit(item);
        }
        return res;
      }

      /// <summary>
      /// Проверяет наличие данных в памяти или в файле.
      /// На момент вызова <paramref name="item"/> должен быть заблокирован
      /// </summary>
      /// <param name="item"></param>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      /// <returns></returns>
      private bool DoGetItemContainsData(InternalCacheItem item, Type objType, string[] keys)
      {
#region Из памяти

        if (item.MemValue != null || item.SerializedData != null)
          return true;

        #endregion

#region Из файла

        if ((item.Persistance & CachePersistance.PersistOnly) == CachePersistance.PersistOnly)
        {
          if (TestFileExists(true, objType, keys))
            return true;
        }
        else if ((item.Persistance & CachePersistance.TempDirOnly) == CachePersistance.TempDirOnly)
        {

          if (TestFileExists(false, objType, keys))
            return true;
        }

        #endregion

        return false;
      }

      #endregion

#region Версии

      private string DoSetVersion(Type objType, string[] keys, string version, bool useVersionTxt)
      {
        string oldVersion;
        lock (_TheTypeDict)
        {
#region Вход для типа данных

          TypeInfo ti;
          if (!_TheTypeDict.TryGetValue(objType, out ti))
          {
            ti = new TypeInfo(this, objType);
            _TheTypeDict.Add(objType, ti);
          }

          #endregion

#region Инициализация и проверка словаря версий

          if (ti.VesrionDict == null)
            ti.SetVersionInfo(keys.Length, useVersionTxt);
          else
          {
            if (keys.Length != ti.VersionKeyLen)
              throw new ArgumentException("Неправильная длина массива ключей: " + keys.Length.ToString() +
                ". При прошлом вызове SetVersion()/SyncVersion() для типа " + objType.ToString() + " длина массива ключей была равна " + ti.VersionKeyLen.ToString(), "keys");
            if (useVersionTxt != ti.UseVersionTxt)
              throw new InvalidOperationException("Для одного типа данных нельзя вызывать попеременно SetVersion() и SyncVersion()");
          }

          #endregion

#region Запись в словарь

          string sKeys = String.Join("|", keys);

          if (!ti.VesrionDict.TryGetValue(sKeys, out oldVersion))
            oldVersion = String.Empty;
          ti.VesrionDict[sKeys] = version;

          #endregion
        }

        return oldVersion;
      }

      internal CacheSetVersionResult SetVersion(Type objType, string[] keys, string version)
      {
        if (keys == null)
          keys = DataTools.EmptyStrings;

        string oldVersion = DoSetVersion(objType, keys, version, true);
        bool changed = !String.Equals(version, oldVersion, StringComparison.Ordinal);
        bool clearPersist;
        if (oldVersion.Length == 0)
        {
          // Первый вызов
          oldVersion = ReadVersionTxt(objType, keys);
          clearPersist = !String.Equals(version, oldVersion, StringComparison.Ordinal);
          if (clearPersist)
            WriteVersionTxt(objType, keys, version);
        }
        else
          clearPersist = changed;
        if (changed)
          Clear(objType, keys, clearPersist);
        return new CacheSetVersionResult(objType, keys, version, clearPersist);
      }

      internal void SyncVersion(Type objType, string[] keys, CacheSetVersionResult setResult)
      {
        string oldVersion = DoSetVersion(objType, keys, setResult.Version, false);
        bool changed = !String.Equals(setResult.Version, oldVersion, StringComparison.Ordinal);
        if (changed)
          Clear(objType, keys, setResult.PersistFilesCleared);
      }

      internal string GetVesion(Type objType, string[] keys)
      {
        if (keys == null)
          keys = DataTools.EmptyStrings;

        string version = String.Empty;
        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (_TheTypeDict.TryGetValue(objType, out ti))
          {
            if (ti.VesrionDict != null)
            {
              if (keys.Length >= ti.VersionKeyLen) // иначе версия не определена
              {
                string sKeys;
                switch (ti.VersionKeyLen)
                {
                  case 0: sKeys = String.Empty; break;
                  case 1: sKeys = keys[0]; break;
                  default:
                    // По-хорошему, это должно встречаться крайне редко
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < ti.VersionKeyLen; i++)
                    {
                      if (i > 0)
                        sb.Append('|');
                      sb.Append(keys[i]);
                    }
                    sKeys = sb.ToString();
                    break;
                }

                if (!ti.VesrionDict.TryGetValue(sKeys, out version))
                  version = String.Empty;
              }
            }
          }
        }

        return version;
      }

#region Чтение/запись файла version.txt

      private string ReadVersionTxt(Type objType, string[] keys)
      {
        if (_Params.PersistDir.IsEmpty)
          return String.Empty;

        AbsPath path = GetVersionTxtPath(objType, keys);
        try
        {
          if (System.IO.File.Exists(path.Path))
            return System.IO.File.ReadAllText(path.Path, Encoding.UTF8);
          else
            return String.Empty;
        }
        catch
        {
          return String.Empty;
        }
      }

      public void WriteVersionTxt(Type objType, string[] keys, string version)
      {
        if (_Params.PersistDir.IsEmpty)
          return;

        AbsPath path = GetVersionTxtPath(objType, keys);
        try
        {
          FileTools.ForceDirs(path.ParentDir);
          System.IO.File.WriteAllText(path.Path, version, Encoding.UTF8);
        }
        catch { }
      }

      private AbsPath GetVersionTxtPath(Type objType, string[] keys)
      {
        AbsPath TypePath = new AbsPath(_Params.PersistDir, objType.ToString());
        AbsPath KeyPath = new AbsPath(TypePath, keys);
        return new AbsPath(KeyPath, "version.txt");
      }

      #endregion

      #endregion

#region Методы очистки

      /// <summary>
      /// Удалить элемент.
      /// В зависимости от <paramref name="clearMode"/>, будкт удалены файлы на диске.
      /// </summary>
      /// <param name="item"></param>
      /// <param name="clearMode"></param>
      private void Remove(InternalCacheItem item, ItemClearMode clearMode)
      {
        // Запоминаем заранее
        string[] keys = null;
        if (clearMode != ItemClearMode.Emergency)
          keys = item.Keys; // тут может быть выделение памяти

        // Удаляем из списков до блокировки Item, чтобы порядок блокировки не нарушался
        lock (_TheTypeDict)
        {
          KeyDict Dict = item.Parent;
          string ParentKey = item.ParentKey;
          while (Dict != null)
          {
            Dict.Remove(ParentKey);
            if (Dict.Count > 0)
              break;
            ParentKey = Dict.ParentKey;
            Dict = Dict.Parent;
          }
          if (MRURemove(item))
          {
            IncStat(item.Parent.ObjType, CacheStatParam.RemoveCount);
            DecStat(item.Parent.ObjType, CacheStatParam.ItemCount);

            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(item.Parent.ObjType, keys), "Item removed");
          }
        }

        // Даже если сейчас другой поток вызовет GetItem() с такими же ключами, будет создан другой экземпляр
        // InternalCacheItem 

        if (Monitor.TryEnter(item, 50))
        {
          try
          {
            DoClearItemInternal(item, clearMode, keys);
          }
          finally
          {
            Monitor.Exit(item);
          }
        }
        else if (!IsDisposed)
        {
          IncStat(item.Parent.ObjType, CacheStatParam.DelayedDelStarted);
          _DelayedClearList.Enqueue(new DelayedClearInfo(item, clearMode, keys));
          _StartBackgroundEvent.Set();
        }
      }

      /// <summary>
      /// Режим очистки элемента кэша
      /// </summary>
      internal enum ItemClearMode
      {
        /// <summary>
        /// Обычная очистка в связи с выходом за пределы MRU-списка.
        /// Временный файл удаляется, постоянный файл сохраняется
        /// </summary>
        MRUOut,

        /// <summary>
        /// Полная очистка элемента при вызове Cache.Clear().
        /// Удаляются все файлы
        /// </summary>
        Clear,

        /// <summary>
        /// Аварийная очистка в связи с нехваткой памяти.
        /// Файлы не удаляются
        /// </summary>
        Emergency
      }

      /// <summary>
      /// Очистка одного элемента.
      /// На момент вызова объект <paramref name="item"/> заблокирован
      /// </summary>
      /// <param name="item">Элемент</param>
      /// <param name="clearMode">Режим очистки</param>
      /// <param name="keys">Ключи. В режиме <paramref name="clearMode"/>=Emergency не нужны</param>
      private void DoClearItemInternal(InternalCacheItem item, ItemClearMode clearMode, string[] keys)
      {
        item.MemValue = null; // освобождаем память
        item.SetSerializedData(this, null);

        if (clearMode != ItemClearMode.Emergency)
        {
          // Тут возможна проблема. Другой поток как раз сейчас может создавать файл с тем же именем
          // Значит мы его сотрем :)
          if ((item.Persistance & CachePersistance.TempDirOnly) != 0)
            DeleteFile(false, item.Parent.ObjType, keys);

          if (clearMode == ItemClearMode.Clear)
          {
            if ((item.Persistance & CachePersistance.PersistOnly) != 0)
              DeleteFile(true, item.Parent.ObjType, keys);
          }
        }

        item.Persistance = ItemDeleted; // чтобы второй раз не пытаться удалять файлы
      }

      //internal void Clear<T>(string[] keys, bool clearPersist)
      //  where T : class
      //{
      //  Clear(typeof(T), keys, clearPersist);
      //}

      internal void Clear(Type objType, string[] keys, bool clearPersist)
      {
        if (keys == null)
          keys = DataTools.EmptyStrings; // запрошено удаление всех объектов

        DoClear(objType, keys, clearPersist);

        // 25.04.2017
        // Удаляем файлы устойчивых объектов после основной очистки кэша, чтобы правильно отображалась статистика
        if (clearPersist)
          DeletePersistFiles(objType, keys);
      }

      private void DoClear(Type objType, string[] keys, bool clearPersist)
      {

        List<InternalCacheItem> Items;

        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (!_TheTypeDict.TryGetValue(objType, out ti))
            return; // Не было ни одного объекта заданного типа

          KeyDict Dict = ti;
          object CurrObj = Dict;
          for (int i = 0; i < keys.Length; i++)
          {
            if (!Dict.TryGetValue(keys[i], out CurrObj))
              return; // заданного ключа нет
            if (i < (keys.Length - 1))
            {
              Dict = CurrObj as KeyDict;
              if (Dict == null)
                throw new ArgumentException("Неправильная длина списка ключей: " + keys.Length.ToString());
            }
          }

          if (CurrObj is InternalCacheItem)
          {
            Items = new List<InternalCacheItem>(1);
            Items.Add((InternalCacheItem)CurrObj);
          }
          else
          {
            Items = new List<InternalCacheItem>();
            AddItems(Items, (KeyDict)CurrObj);
          }
        }

        // Список Items содержит массив объектов для удаления
        for (int i = 0; i < Items.Count; i++)
          Remove(Items[i], clearPersist ? ItemClearMode.Clear : ItemClearMode.MRUOut);
      }

      [DebuggerStepThrough]
      private void DeletePersistFiles(Type objType, string[] keys)
      {
        if (Params.PersistDir.IsEmpty)
          return;
        // Ключи могут задавать путь к файлу или каталогу
        try
        {
          AbsPath DirPath = GetBinFilePath(true, objType, keys, false);
          if (Directory.Exists(DirPath.Path))
            FileTools.ClearDirAsPossible(DirPath);
          else if (keys.Length > 0) // условие добавлено 19.10.2020
          {
            AbsPath FilePath = GetBinFilePath(true, objType, keys, true);
            //if (File.Exists(FilePath.Path))
            //  File.Delete(FilePath.Path);
            // 25.07.2020
            // Удаляем все файлы *.bin
            //string FileMask = "*.bin";
            // 19.10.2020
            // Так тоже неправильно. Удалили все файлы последнего уровня, а не только для текущего ключа.
            //            string FileMask = keys[keys.Length - 1];
            //            int p = FileMask.IndexOf('#');
            //#if DEBUG
            //            if (p < 0)
            //              throw new BugException("Имя файла " + FilePath.Path + " не содержит символа \"#\"");
            //#endif
            //            FileMask = FileMask.Substring(0, p + 1) + "*.bin";

            // 04.01.2021
            // И опять неправильно!
            string FileMask = keys[keys.Length - 1] + "*.bin";

            if (Directory.Exists(FilePath.ParentDir.Path))
              FileTools.DeleteFiles(FilePath.ParentDir, FileMask, SearchOption.TopDirectoryOnly);
          }

          // Восстановление файлов version.info
          RestoreVersionTxtFiles(objType, keys);
        }
        catch
        {
        }
      }

      /// <summary>
      /// Восстановление файлов version.txt
      /// </summary>
      /// <param name="objType"></param>
      /// <param name="keys"></param>
      private void RestoreVersionTxtFiles(Type objType, string[] keys)
      {
        TypeInfo ti;
        lock (_TheTypeDict)
        {
          if (_TheTypeDict.TryGetValue(objType, out ti))
          {
            if (ti.UseVersionTxt)
            {
              if (keys == null)
              {
                foreach (KeyValuePair<string, string> pair in ti.VesrionDict)
                {
                  string[] ThisKeys = pair.Key.Split('|');
                  WriteVersionTxt(objType, ThisKeys, pair.Value);
                }
              }
              else
              {
                string sKeys1 = String.Join("|", keys);
                string sKeys2 = sKeys1 + "|";
                foreach (KeyValuePair<string, string> pair in ti.VesrionDict)
                {
                  if (pair.Key == sKeys1 || pair.Key.StartsWith(sKeys2))
                  {
                    string[] ThisKeys = pair.Key.Split('|');
                    WriteVersionTxt(objType, ThisKeys, pair.Value);
                  }
                }
              }
            }
          }
        }
      }

      internal void Clear(bool clearPersist)
      {
        Type[] typs;
        lock (_TheTypeDict)
        {
          typs = new Type[_TheTypeDict.Keys.Count];
          _TheTypeDict.Keys.CopyTo(typs, 0);
        }

        if (clearPersist && (!Params.PersistDir.IsEmpty))
          FileTools.ClearDirAsPossible(Params.PersistDir); // 10.04.2020. Надо удалять, в том числе, и те типы данных, которые (пока) не использовались в программе

        for (int i = 0; i < typs.Length; i++)
        {
          Clear(typs[i], null, clearPersist);
          if (clearPersist && (!Params.PersistDir.IsEmpty))
            RestoreVersionTxtFiles(typs[i], null);
        }
      }

      /// <summary>
      /// Рекурсивная процедура добавления элементов в список
      /// </summary>
      /// <param name="items"></param>
      /// <param name="dict"></param>
      private static void AddItems(List<InternalCacheItem> items, KeyDict dict)
      {
        foreach (KeyValuePair<string, object> Pair in dict)
        {
          if (Pair.Value is InternalCacheItem)
            items.Add((InternalCacheItem)(Pair.Value));
          else
            // рекурсивный вызов
            AddItems(items, (KeyDict)(Pair.Value));
        }
      }

      internal void FreeMemory()
      {
#if TRACE_CHECKMEMORY
        DoTrace("Cache.FreeMemory() started");
#endif
        while (FlushBlock(true))
        {
        }

#if TRACE_CHECKMEMORY
        DoTrace("Cache.FreeMemory() finished");
#endif
      }

#if TRACE_CHECKMEMORY
      private static void DoTrace(string s)
      {
        try
        {
          Trace.WriteLine(DateTime.Now.ToString("G") + ": " + s);
        }
        catch { }
      }
#endif

      #endregion

#region Обработка сигнала таймера проверки памяти

      private Timer _CheckMemoryTimer;

      private bool _InsideCheckMemoryTimer_Tick;

      private void CheckMemoryTimer_Tick(object state)
      {
        try
        {
          if (!_InsideCheckMemoryTimer_Tick)
          {
            _InsideCheckMemoryTimer_Tick = true;
            try
            {
              // Тут не нужно вызывать WaitCheckMemory(), 
              // т.к. если метод CheckMemory() сейчас выполняется, то ничего делать не нужно.
              CheckMemory(CacheStatParam.CheckMemoryTimerCallCount);
            }
            finally
            {
              _InsideCheckMemoryTimer_Tick = false;
            }
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка очистки кэша по таймеру");
        }
      }

      /// <summary>
      /// Используется для блокировки потока при вызове CheckMemory()
      /// </summary>
      private readonly object _CheckMemorySyncRoot; // 05.11.2020

      /// <summary>
      /// Ожидание готовности CheckMemory()
      /// Если все в порядке, метод сразу же возвращает управление
      /// Если в данный момент выполняется работа по освобожению памяти,
      /// текущий поток будет заморожен.
      /// Это предотвращает ситуацию, когда первый поток столкнулся с исключением OutOfMemoryException(),
      /// а другие потоки в это время будут обращаться к кэшу и съедать память дальше. Пусть подождут.
      /// </summary>
      private void WaitCheckMemory()
      {
        // Ожидаем блокировку, но ничего не делаем
#if USE_CHECKMEMORYTIME
          long startWait = DateTime.Now.Ticks;
#endif

        lock (_CheckMemorySyncRoot)
        {
        }

#if USE_CHECKMEMORYTIME
          long waitTicks = DateTime.Now.Ticks - startWait;
          long waitMS = waitTicks / 10000L;
          if (waitMS >= 100)
            IncStat(null, CacheStatParam.CheckMemoryWaitTime);
#endif
      }

      /// <summary>
      /// Выполнить удаление лишних элементов кэша по таймеру, из-за ошибки или вызовом пользователя.
      /// Этот метод должен правильно отрабатывать многопоточность. Если он вызван из другого потока,
      /// пока обработка выполняется в первом потоке, второй поток должен дождаться завершения
      /// </summary>
      /// <param name="checkMemoryType">Причина вызова метода</param>
      internal void CheckMemory(CacheStatParam checkMemoryType)
      {
        IncStat(null, CacheStatParam.CheckMemoryWaitThreadCount);
        try
        {
          if (Monitor.TryEnter(_CheckMemorySyncRoot))
          {
            try
            {
              IncStat(null, checkMemoryType);
#if TRACE_CHECKMEMORY
              DoTrace("Cache.CheckMemory() started. GC.GetTotalMemory()=" + LogoutTools.MBText(GC.GetTotalMemory(false)));
#endif

              try
              {
                // 06.01.2021
                // Если вызов произошел из за OutOfMemoryException, освобождаем память аварийным способом
                if (checkMemoryType == CacheStatParam.CheckMemoryEmergencyCallCount)
                  DeleteBlockWhenMemoryLow(ItemClearMode.Emergency);

                DoCheckMemory1();
                DoCheckMemory2();
              }
              catch (Exception e)
              {
                // 06.01.2021
                // Аварийная очистка всего кэша
                lock (_TheTypeDict)
                {
                  _TheTypeDict.Clear();
                  _FirstItem = null;
                  _LastItem = null;
                  _Count = 0;
                  _DelayedWriteList.Clear();
                  // не нужно удалять список на очистку _DelayedClearList.Clear();

                  IncStat(null, CacheStatParam.EmergencyClearCount);

                  // Только после освобожения памяти можно вывести log-файл
                  LogoutTools.LogoutException(e, "Cache.CheckMemory() emergency clear");
                }
              }

#if TRACE_CHECKMEMORY
              DoTrace("Cache.CheckMemory() finished. GC.GetTotalMemory()=" + LogoutTools.MBText(GC.GetTotalMemory(false)));
#endif
            }
            finally
            {
              Monitor.Exit(_CheckMemorySyncRoot);
            }
            return;
          }

          WaitCheckMemory();
        }
        finally
        {
          DecStat(null, CacheStatParam.CheckMemoryWaitThreadCount);
        }
      }

      private void DoCheckMemory1()
      {
        bool flag = false;
        bool resolved = true;
        bool HasDel = false;

        while (!MemoryTools.CheckSufficientMemory(MemoryTools.LowMemorySizeMB))
        {
          flag = true;
          if (!DeleteBlockWhenMemoryLow(ItemClearMode.MRUOut))
          {
            resolved = false;
            break; // 28.08.2019
          }
          //#if USE_GCCOLLECT
          //          GC.Collect();
          //#endif
          HasDel = true;
        }

        if (flag)
        {
          IncStat(null, CacheStatParam.InsufficientMemoryTotalCount);
          if (resolved)
            IncStat(null, CacheStatParam.InsufficientMemoryResolvedCount);
        }

        if (HasDel)
        {
          // После того, как освободили критическое количество виртуальной памяти,
          // перекидываем один блок в файлы
          FlushBlock(false);
        }
      }

      private void DoCheckMemory2()
      {
        bool flag = false; // было ли состояние CriticalMemoryLoad?
        bool resolved = true; // удалось ли полностью исправить ситуацию CriticalMemoryLoad?

        while (MemoryTools.AvailableMemoryState==AvailableMemoryState.Low || // 06.01.2021
          MemoryTools.MemoryLoad >= Params.CriticalMemoryLoad)
        {
          flag = true;
          if (!FlushBlock(true))
          {
            resolved = false;
            break;
          }
          //#if USE_GCCOLLECT

          // 05.01.2021
          // Надо обязательно почистить кучу, иначе память не будет освобождена и следующий вызов MemoryLoad() снова вернет нехватку памяти
          GC.Collect();
          //#endif
        }

        if (flag)
        {
          IncStat(null, CacheStatParam.CriticalMemoryLoadTotalCount);
          if (resolved)
            IncStat(null, CacheStatParam.CriticalMemoryLoadResolvedCount);
        }
      }

      /// <summary>
      /// Удаление 100 самых старых элементов кэша
      /// </summary>
      /// <returns></returns>
      private bool DeleteBlockWhenMemoryLow(ItemClearMode clearMode)
      {
        bool Res = false;
        for (int i = 0; i < Params.ClearCacheBlockSize; i++)
        {
          InternalCacheItem ItemToDelete;
          lock (_TheTypeDict)
          {
            ItemToDelete = this._LastItem;
          }

          if (ItemToDelete != null)
          {
            Remove(ItemToDelete, clearMode);
            IncStat(ItemToDelete.Parent.ObjType, CacheStatParam.LowMemoryRemoveCount);
            Res = true;
          }
          else
            break;
        }
        return Res;
      }

      /// <summary>
      /// Выполняет сброс на диск элементов кэша, у которых есть сериализованные данные.
      /// Этот метод всегда вызывается в цикле.
      /// За один вызов сбрасывается не более 100 элементов кэша.
      /// </summary>
      /// <param name="deleteMemoryOnly">Если true, то будут удалены элементы кэша с CachePersistance.MemoryOnly</param>
      /// <returns>true, если хотя бы что-то было удалено.
      /// Если false, то освободить память не удалось. Может помочь только очистка кэша</returns>
      private bool FlushBlock(bool deleteMemoryOnly)
      {
#region Собираем список

        List<InternalCacheItem> items = new List<InternalCacheItem>();
        lock (_TheTypeDict)
        {
          InternalCacheItem Item = this._LastItem;
          while (Item != null && items.Count < 100)
          {
            if (Item.MemValue != null)
            {
              if (deleteMemoryOnly || ((Item.Persistance & (CachePersistance.TempDirOnly | CachePersistance.PersistOnly)) != 0))
                items.Add(Item);
            }

            Item = Item.PrevItem;
          }
        }

        #endregion

        if (items.Count == 0)
          return false;

#region Удаляем/очищаем элементы

        for (int i = 0; i < items.Count; i++)
        {
          if (items[i].Persistance == CachePersistance.MemoryOnly)
          {
            Remove(items[i], ItemClearMode.MRUOut);
            IncStat(items[i].Parent.ObjType, CacheStatParam.LowMemoryRemoveCount);
          }
          else if ((items[i].Persistance & CachePersistance.MemoryOnly) != 0)
          {
            bool isPersistDir = (items[i].Persistance & CachePersistance.PersistOnly) != 0;
            //lock (Items[i])
            if (Monitor.TryEnter(items[i])) // 21.09.2020 - не ждем блокировку
            {
              try
              {
                if (items[i].SerializedData != null) // значение могло исчезнуть
                {
                  // Сбрасываем в файл
                  WriteSerializedToFile(isPersistDir, items[i].Parent.ObjType, items[i].Keys, items[i].SerializedData);
                  IncStat(items[i].Parent.ObjType, CacheStatParam.LowMemorySaveFileCount);
                  items[i].SetSerializedData(this, null);
                  items[i].MemValue = null;
                }
              }
              finally
              {
                Monitor.Exit(items[i]);
              }
            }
          }
        }
        #endregion

        return true;
      }

      #endregion

#region Отложенная запись файлов и очистка элементов

      /// <summary>
      /// Фоновый поток, выполняющий отложенную запись файлов и очистку элементов
      /// </summary>
      private Thread _BackgroundThread;

      /// <summary>
      /// Используется для запуска фонового процесса
      /// </summary>
      private ManualResetEvent _StartBackgroundEvent;

      /// <summary>
      /// Устанавливается в true методом Dispose
      /// </summary>
      private volatile bool _DisposingFlag;

      private void BackgroundThreadProc()
      {
        bool IsComplete = true;

        while (true) // бесконечный цикл, прерываемый сигналом Abort.
        {
          // Когда мы уже обработали очередь, могли появиться новые элементы для записи.
          // В этом случае семафор уже переведен в сигнальное состояние.
          // Тогда бесконечный цикл будет выполнен еще раз

          // Ожидание запуска
          if (IsComplete)
            _StartBackgroundEvent.WaitOne();
          else
            Thread.Sleep(100);
          // Сброс сигнала
          _StartBackgroundEvent.Reset();

          if (_DisposingFlag)
            return;

          IncStat(null, CacheStatParam.BackgroundCircleCount);

          IsComplete = true;
          try
          {
            if (!BackgroundThreadProcessWrite())
              IsComplete = false;
          }
          catch (Exception e)
          { LogoutTools.LogoutException(e, "Ошибка отложенной записи файлов кэша"); }
          SetStat(null, CacheStatParam.DelayedWriteQueueLength, _DelayedWriteList.Count);

          try
          {
            if (!BackgroundThreadProcessDelete())
              IsComplete = false;
          }
          catch (Exception e)
          { LogoutTools.LogoutException(e, "Ошибка отложенной очистки кэша"); }
          SetStat(null, CacheStatParam.DelayedDelQueueLength, _DelayedClearList.Count);
        } // бесконечный цикл
      }

#region Запись файлов

      /// <summary>
      /// Очередь элементов, для которых нужно выполнить запись файлов
      /// </summary>
      private readonly SyncQueue<InternalCacheItem> _DelayedWriteList;

      private bool BackgroundThreadProcessWrite()
      {
        InternalCacheItem item;
        List<InternalCacheItem> busyItems = null; // элементы, которые не удалось заблокировать

        while (_DelayedWriteList.TryDequeue(out item))
        {
          if (_DisposingFlag)
            break;

          if (!BackgroundThreadProcessWriteOneFile(item))
          {
            if (busyItems == null)
              busyItems = new List<InternalCacheItem>();
            busyItems.Add(item); // Добавляем для последующего повторного вызова
          }
        }

#region Повторное добавление элемента в очередь

        if (busyItems == null)
          return true;
        else
        {
          for (int i = 0; i < busyItems.Count; i++)
            _DelayedWriteList.Enqueue(busyItems[i]);
          return false;
        }

        #endregion
      }

      private bool BackgroundThreadProcessWriteOneFile(InternalCacheItem item)
      {
        CachePersistance persistance;
        byte[] SerializedData;
        Type objType;
        string[] keys;

        try
        {
#region Первый доступ к элементу

          if (!Monitor.TryEnter(item))
            return false;
          try
          {
            persistance = item.Persistance;
            SerializedData = item.SerializedData;
            objType = item.Parent.ObjType;
            keys = item.Keys;
            item.SetSerializedData(this, null); // очищаем сразу
          }
          finally
          {
            Monitor.Exit(item);
          }

          #endregion

          if (SerializedData == null)
            return true;

          if ((persistance & (CachePersistance.PersistOnly | CachePersistance.TempDirOnly)) == 0)
            throw new BugException("Item.Persistance=" + persistance.ToString());

          bool isPersistDir = (persistance & CachePersistance.PersistOnly) != 0;

          try
          {
#region Запись

            AbsPath filePath = GetBinFilePath(isPersistDir, objType, keys, true);
            AbsPath tempPath = filePath.ChangeExtension(".tmp");
            FileTools.ForceDirs(tempPath.ParentDir);
            System.IO.File.WriteAllBytes(tempPath.Path, SerializedData);
            if (Cache.TraceSwitch.Enabled)
              Trace.WriteLine(GetTracePrefix(objType, keys) + "Temp file created " + tempPath.FileName);

            #endregion

#region Второй доступ

            lock (item) // ждем, сколько надо
            {
              if (item.Persistance == persistance && Object.ReferenceEquals(item.SerializedData, null))
              {
                File.Delete(filePath.Path);
                File.Move(tempPath.Path, filePath.Path);
                if (Cache.TraceSwitch.Enabled)
                  Trace.WriteLine(GetTracePrefix(objType, keys) + "Temp file renamed to " + filePath.FileName);

                IncStat(objType, CacheStatParam.SaveFileCount);
                IncStat(objType, CacheStatParam.DelayedWriteFinished);
              }
              else
                System.IO.File.Delete(tempPath.Path);
            }

            #endregion
          }
          catch
          {
            IncStat(objType, CacheStatParam.SaveFileErrors);
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Cache. Ошибка отложенной записи файла");
        }

        return true;
      }

      #endregion

#region Очистка элементов

      private class DelayedClearInfo
      {
#region Конструктор

        internal DelayedClearInfo(InternalCacheItem item, ItemClearMode clearMode, string[] keys)
        {
          _Item = item;
          _ClearMode = clearMode;
          _Keys = keys;
        }

        #endregion

#region Свойства

        internal InternalCacheItem Item { get { return _Item; } }
        private readonly InternalCacheItem _Item;

        internal ItemClearMode ClearMode { get { return _ClearMode; } }
        private readonly ItemClearMode _ClearMode;

        internal string[] Keys { get { return _Keys; } }
        private readonly string[] _Keys;

        #endregion
      }

      /// <summary>
      /// Список объектов для асинхронного удаления.
      /// </summary>
      private readonly SyncQueue<DelayedClearInfo> _DelayedClearList;

      private bool BackgroundThreadProcessDelete()
      {
        DelayedClearInfo info;
        List<DelayedClearInfo> busyItems = null; // элементы, которые не удалось заблокировать
        while (_DelayedClearList.TryDequeue(out info))
        {
          if (_DisposingFlag)
            break;

#region Обработка одного элемента

          if (Monitor.TryEnter(info.Item))
          {
            try
            {
              DoClearItemInternal(info.Item, info.ClearMode, info.Keys);
              IncStat(info.Item.Parent.ObjType, CacheStatParam.DelayedDelFinished);
            }
            finally
            {
              Monitor.Exit(info.Item);
            }
          }
          else
          {
            if (busyItems == null)
              busyItems = new List<DelayedClearInfo>();
            busyItems.Add(info);
          }

          #endregion
        }

#region Повторное добавление элемента в очередь

        if (busyItems == null)
          return true;
        else
        {
          for (int i = 0; i < busyItems.Count; i++)
            _DelayedClearList.Enqueue(busyItems[i]);
          return false;
        }

        #endregion
      }

      #endregion

      #endregion

#region Сбор статистики

      /// <summary>
      /// Параметры для сбора статистики.
      /// При всех обращениях объект коллекции кратковременно блокируется
      /// </summary>
      private readonly Dictionary<Type, CacheStat> _TypeStats;

      /// <summary>
      /// Общая статистика, которая не разбивается по типам данных.
      /// Сюда не дублируются данные из _TypeStats.
      /// При всех обращениях блокируется _TypeStats.
      /// </summary>
      private readonly CacheStat _TotalStat;

      /// <summary>
      /// Увеличить счетчик статистики.
      /// </summary>
      /// <param name="objType">Тип данных или null для общей статистики</param>
      /// <param name="statParam">Параметр</param>
      internal void IncStat(Type objType, CacheStatParam statParam)
      {
        IncStat(objType, statParam, 1L);
      }


      /// <summary>
      /// Уменьшить счетчик статистики.
      /// </summary>
      /// <param name="objType">Тип данных или null для общей статистики</param>
      /// <param name="statParam">Параметр</param>
      internal void DecStat(Type objType, CacheStatParam statParam)
      {
        IncStat(objType, statParam, -1L);
      }

      /// <summary>
      /// Увеличить счетчик статистики.
      /// </summary>
      /// <param name="objType">Тип данных или null для общей статистики</param>
      /// <param name="statParam">Параметр</param>
      /// <param name="delta">На сколько нужно увеличить значение</param>
      internal void IncStat(Type objType, CacheStatParam statParam, long delta)
      {
        lock (_TypeStats)
        {
          if (objType != null)
          {
            CacheStat Stat;
            if (!_TypeStats.TryGetValue(objType, out Stat))
            {
              Stat = new CacheStat();
              _TypeStats.Add(objType, Stat);
            }
            Stat.Inc(statParam, delta);
          }
          _TotalStat.Inc(statParam, delta);
        }
      }

      /// <summary>
      /// Установить счетчик статистики.
      /// </summary>
      /// <param name="objType">Тип данных или null для общей статистики</param>
      /// <param name="statParam">Параметр</param>
      /// <param name="value">Новое значение</param>
      internal void SetStat(Type objType, CacheStatParam statParam, long value)
      {
        lock (_TypeStats)
        {
          if (objType != null)
          {
            CacheStat Stat;
            if (!_TypeStats.TryGetValue(objType, out Stat))
            {
              Stat = new CacheStat();
              _TypeStats.Add(objType, Stat);
            }
            Stat[statParam] = value;
          }
          _TotalStat[statParam] = value;
        }
      }

      internal CacheStat GetStat(Type objType)
      {
        lock (_TypeStats)
        {
          CacheStat Stat;
          if (_TypeStats.TryGetValue(objType, out Stat))
            return Stat.Clone();
          else
            return new CacheStat();
        }
      }

      public void GetStat(out Type[] objTypes, out CacheStat[] stats, out CacheStat totalStat)
      {
        lock (_TheTypeDict)
        {
          objTypes = new Type[_TheTypeDict.Count];
          _TheTypeDict.Keys.CopyTo(objTypes, 0);
        }

        stats = new CacheStat[objTypes.Length];
        lock (_TypeStats)
        {
          for (int i = 0; i < objTypes.Length; i++)
          {
            CacheStat Stat;
            if (_TypeStats.TryGetValue(objTypes[i], out Stat))
              stats[i] = Stat.Clone();
            else
              stats[i] = new CacheStat();
          }
          totalStat = _TotalStat.Clone();
        }
      }

      internal CacheStat GetStat()
      {
        lock (_TypeStats)
        {
          return _TotalStat.Clone();
        }
      }

      internal long GetStatValue(CacheStatParam statParam)
      {
        lock (_TypeStats)
        {
          return _TotalStat[statParam];
        }
      }

      public void GetKeys(Type objType, List<string[]> keyList)
      {
        if (objType == null)
          throw new ArgumentNullException("objType");

        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (_TheTypeDict.TryGetValue(objType, out ti))
            GetKeys2(ti, keyList); // рекурсивный метод
        }
      }

      private void GetKeys2(KeyDict dict, List<string[]> keyList)
      {
        foreach (KeyValuePair<string, object> Pair in dict)
        {
          InternalCacheItem Item = Pair.Value as InternalCacheItem;
          if (Item == null)
            GetKeys2((KeyDict)(Pair.Value), keyList); // рекурсивный вызов
          else
            keyList.Add(Item.Keys);
        }
      }


      public void GetKeysCommaString(Type objType, StringBuilder sb)
      {
        if (objType == null)
          throw new ArgumentNullException("objType");

        lock (_TheTypeDict)
        {
          TypeInfo ti;
          if (_TheTypeDict.TryGetValue(objType, out ti))
            GetKeysCommaString2(ti, sb); // рекурсивный метод
        }
      }

      private void GetKeysCommaString2(KeyDict dict, StringBuilder sb)
      {
        foreach (KeyValuePair<string, object> Pair in dict)
        {
          InternalCacheItem Item = Pair.Value as InternalCacheItem;
          if (Item == null)
            GetKeysCommaString2((KeyDict)(Pair.Value), sb); // рекурсивный вызов
          else
          {
            DataTools.CommaStringFromArray(sb, Item.Keys);
            sb.Append(Environment.NewLine);
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Фиктивная одноразовая фабрика
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class DummyFactory<T> : ICacheFactory<T>
      where T : class
    {
#region Конструктор

      public DummyFactory(T newValue)
      {
        _newValue = newValue;
      }

      private readonly T _newValue;

      #endregion

#region ICacheFactory<T> Members

      public T CreateCacheItem(string[] keys)
      {
        return _newValue;
      }

      #endregion
    }

    /// <summary>
    /// Элемент кэша, хранящий ссылку на данные в памяти и являющийся элементом двунаправленного списка.
    /// Важно, чтобы это был класс, а не структура, т.к. ссылки на экземпляр могут существовать и в основном потоке, и в фоновом.
    /// </summary>
    private class InternalCacheItem
    {
#region Конструктор

      public InternalCacheItem(KeyDict parent, string parentKey)
      {
        _Parent = parent;
        _ParentKey = parentKey;
      }

      #endregion

#region Основные свойства

      public KeyDict Parent { get { return _Parent; } }
      private readonly KeyDict _Parent;

      /// <summary>
      /// Ключ последнего уровня, который используется в Parent для данного объекта
      /// </summary>
      public string ParentKey { get { return _ParentKey; } }
      private readonly string _ParentKey;

      /// <summary>
      /// Данные (несериализованные) в памяти.
      /// Используются в режимах CachePersistance.MemoryOnly, MemoryAndTempDir и MemoryAndPersist.
      /// Причем, в режимах MemoryAndXXX ссылка может быть очищена при нехватке памяти
      /// </summary>
      public object MemValue;

      /// <summary>
      /// Данные, ожидающие записи в файл.
      /// Сериализация объекта всегда должна выполняться незамедлительно в момент получения объекта с помощью
      /// фабрики. Некоторые классы, например, DataTable имеют проблемы с асинхронной сериализацией.
      /// Например, во время сериализации свойство DataTable.DefaultView.Sort может временно очищаться.
      /// Если в этот момент таблица используется прикладным кодом для чтения, может возникнуть "мерцающая" ошибка.
      /// Запись уже сериализованных данных в файл может выполняться в произвольный момент времени.
      /// 
      /// Время сброса данных в файл зависит от режима Persistance.
      /// TempDirOnly, PersistOnly и MemoryAndPersist - ставятся в очередь для асинхронной записи файла. 
      /// Если очередь переполнена, то запись выполняется немедленно. При этом в вызывающем методе будет задержка.
      /// MemoryAndTempDir - выполняется только в ситуации нехватки памяти. Если дефицита памяти нет, данные никогда
      /// не будут записаны на диск. Очередь не используется.
      /// </summary>
      public byte[] SerializedData { get { return _SerializedData; } }
      private byte[] _SerializedData;

      public void SetSerializedData(InternalCache owner, byte[] value)
      {
        int oldCount = 0;
        int oldSize = 0;
        if (_SerializedData != null)
        {
          oldCount = 1;
          oldSize = _SerializedData.Length;
        }

        int newCount = 0;
        int newSize = 0;
        if (value != null)
        {
          newCount = 1;
          newSize = value.Length;
        }

        _SerializedData = value;

        if (newCount != oldCount)
          owner.IncStat(_Parent.ObjType, CacheStatParam.SerializationBufferCount, newCount - oldCount);
        if (newSize != oldSize)
          owner.IncStat(_Parent.ObjType, CacheStatParam.SerializationBufferBytes, newSize - oldSize);
      }

      /// <summary>
      /// Способ хранения данных
      /// </summary>
      public CachePersistance Persistance;

      #endregion

#region Дополнительные свойства

      /// <summary>
      /// Массив ключей
      /// </summary>
      public string[] Keys
      {
        get
        {
          // Нет необходимости блокировать коллекции, т.к. свойства Parent и ParentKey задаются в конструкторе InternalCacheItem
          int cnt = 0;
          KeyDict Dict = Parent;
          while (Dict != null)
          {
            cnt++;
            Dict = Dict.Parent;
          }

          string[] a = new string[cnt];
          Dict = Parent;
          a[a.Length - 1] = ParentKey;
          cnt = 1;
          while (Dict.Parent != null)
          {
            cnt++;
            a[a.Length - cnt] = Dict.ParentKey;
            Dict = Dict.Parent;
          }

          return a;
        }
      }

      /// <summary>
      /// Текстовое предтавление - список ключей Keys.
      /// </summary>
      /// <returns>Текстовое предтавление</returns>
      public override string ToString()
      {
        return String.Join(", ", Keys);
      }

      /// <summary>
      /// Устанавливается на время вызова GetItem() для предотвращения реентрантного вызова
      /// </summary>
      public bool InsideGetItemFlag;

      #endregion

#region MRU-список

      // В качестве индикации того, что элемент не находится в списке MRU, используются значения null
      // Чтобы обозначить терминальные элементы, используется признак NextItem=this и PrevItem=this

      /// <summary>
      /// Двусвязный список - следующий элемент.
      /// Равно this, если элемент является последним в списке.
      /// Равно null, если элемент не находится в списке MRU.
      /// </summary>
      internal InternalCacheItem NextItem;

      /// <summary>
      /// Двусвязный список - предыдущий элемент
      /// Равно this, если элемент является первым в списке.
      /// Равно null, если элемент не находится в списке MRU.
      /// </summary>
      internal InternalCacheItem PrevItem;

      #endregion
    }


    /// <summary>
    /// Страница элементов кэша.
    /// Объектом-значением может быть либо дочерний KeyDict, либо InternalCacheItem
    /// </summary>
    private class KeyDict : Dictionary<string, object>
    {
#region Конструктор

      public KeyDict(InternalCache cache, Type objType, KeyDict parent, string parentKey)
      {
        _Cache = cache;
        _ObjType = objType;
        _Parent = parent;
        _ParentKey = parentKey;
      }

      #endregion

#region Свойства

      //public InternalCache Cache { get { return _Cache; } }
      private readonly InternalCache _Cache;

      public Type ObjType { get { return _ObjType; } }
      private readonly Type _ObjType;

      public KeyDict Parent { get { return _Parent; } }
      private readonly KeyDict _Parent;

      /// <summary>
      /// Ключ текущего объекта в коллекции Parent
      /// </summary>
      public string ParentKey { get { return _ParentKey; } }
      private readonly string _ParentKey;

      #endregion
    }

    /// <summary>
    /// Стран
    /// </summary>
    private class TypeInfo : KeyDict
    {
#region Конструктор

      public TypeInfo(InternalCache cache, Type objType)
        : base(cache, objType, null, null)
      {
      }

      #endregion

#region Свойства для версии

      public void SetVersionInfo(int versionKeyLen, bool useVersionTxt)
      {
        _VersionKeyLen = versionKeyLen;
        _UseVersionTxt = useVersionTxt;
        _VesrionDict = new Dictionary<string, string>();
      }

      /// <summary>
      /// Длина ключа, используемого для версии.
      /// При вызове SetVersion() проверяется, что длина ключа каждый раз совпадает
      /// </summary>
      public int VersionKeyLen { get { return _VersionKeyLen; } }
      private int _VersionKeyLen;

      /// <summary>
      /// Используется ли файл version.txt.
      /// Равно true, если был вызван метод SetVersion() и false, если SyncVersion().
      /// </summary>
      public bool UseVersionTxt { get { return _UseVersionTxt; } }
      private bool _UseVersionTxt;


      /// <summary>
      /// Коллекция версий ключей для данного типа.
      /// В отличие от KeyDict, не используется иерархия объектов. Все версии для объекта хранятся в одной таблице
      /// Ключ коллекции - подмножество ключей, для которого задана версия. Может быть пустой строкой. 
      /// Если версии используются для ключей с уровнем, больше 1, отдельные ключи разделяются вертикальной чертой.
      /// Значение коллекции - версия.
      /// </summary>
      public Dictionary<string, string> VesrionDict { get { return _VesrionDict; } }
      private Dictionary<string, string> _VesrionDict;

      #endregion
    }

    /// <summary>
    /// Коллекция страниц по типам хранимых данных
    /// </summary>
    private class TypeDict : Dictionary<Type, TypeInfo>
    {
    }

    #endregion

#region Статические свойства и методы

    /// <summary>
    /// Параметры настройки кэша.
    /// Могут устанавливаться только до активации системы, происходящей при первом обращении к методу GetItem(), SetItem(), Clear(), SetVersion()
    /// и некоторых других
    /// </summary>
    public static CacheParams Params { get { return _StaticParams; } }
    private static readonly CacheParams _StaticParams = new CacheParams();


    /// <summary>
    /// Заменяет режимы с постоянным файлом на режимы с временным файлом, если в параметрах не задан путь к постоянному каталогу
    /// </summary>
    /// <param name="persistance">Режим, заданный пользователем</param>
    /// <returns>Скорректированный режим</returns>
    private static CachePersistance CorrectPersistance(CachePersistance persistance)
    {
      switch (persistance)
      {
        case CachePersistance.MemoryOnly:
        case CachePersistance.TempDirOnly:
        case CachePersistance.MemoryAndTempDir:
          return persistance;
        case CachePersistance.PersistOnly:
          if (Params.PersistDir.IsEmpty)
            return CachePersistance.TempDirOnly;
          else
            return persistance;
        case CachePersistance.MemoryAndPersist:
          if (Params.PersistDir.IsEmpty)
            return CachePersistance.MemoryAndTempDir;
          else
            return persistance;
        default:
          throw new ArgumentException("Недопустимый режим CachePersistance=" + persistance.ToString(), "persistance");
      }
    }

    /// <summary>
    /// Основной метод получение буферизованных данных
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="factory">Объект, выполняющий создание данных, если их нет в кэше</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static T GetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory)
      where T : class
    {
      persistance = CorrectPersistance(persistance);
      if (factory == null)
        throw new ArgumentNullException("factory");

      return GetMainObj(GetMainObjMode.Create).GetItem<T>(keys, persistance, factory, Params.LockTimeout);
    }

    /// <summary>
    /// Основной метод получение буферизованных данных
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="factory">Объект, выполняющий создание данных, если их нет в кэше</param>
    /// <param name="lockTimeout">Максимальное время ожидания блокировки страницы кэша в миллисекундах. 
    /// Значение должно быть больше нуля или равно System.Threading.Timeout.Infinite</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static T GetItem<T>(string[] keys, CachePersistance persistance, ICacheFactory<T> factory, int lockTimeout)
      where T : class
    {
      persistance = CorrectPersistance(persistance);
      if (factory == null)
        throw new ArgumentNullException("factory");

      return GetMainObj(GetMainObjMode.Create).GetItem<T>(keys, persistance, factory, lockTimeout);
    }


#if XXX
    /// <summary>
    /// Вспомогательный метод получение буферизованных данных.
    /// Возвращает объект кэша, только если он есть в кэше. Иначе возвращается null.
    /// Эта версия проверяет только наличие объектов в памяти (CachePersistance.MemoryOnly)
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <returns>Сохраненный объект или null</returns>
    [Obsolete("Используйте перегрузку метода с аргументом persistance", false)]
    public static T GetItemIfExists<T>(string[] keys)
      where T : class
    {
      return GetItemIfExists<T>(keys, CachePersistance.MemoryOnly, Params.LockTimeout);
    }
#endif

    /// <summary>
    /// Вспомогательный метод получение буферизованных данных.
    /// Возвращает объект кэша, только если он есть в кэше. Иначе возвращается null
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <returns>Сохраненный объект или null</returns>
    public static T GetItemIfExists<T>(string[] keys, CachePersistance persistance)
      where T : class
    {
      return GetItemIfExists<T>(keys, persistance, Params.LockTimeout);
    }

    /// <summary>
    /// Вспомогательный метод получение буферизованных данных.
    /// Возвращает объект кэша, только если он есть в кэше. Иначе возвращается null
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="lockTimeout">Максимальное время ожидания блокировки страницы кэша в миллисекундах. 
    /// Значение должно быть больше нуля или равно System.Threading.Timeout.Infinite</param>
    /// <returns>Сохраненный объект или null</returns>
    public static T GetItemIfExists<T>(string[] keys, CachePersistance persistance, int lockTimeout)
      where T : class
    {
      persistance = CorrectPersistance(persistance);
      InternalCache ic = GetMainObj(persistance == CachePersistance.MemoryOnly ? GetMainObjMode.DontCreate : GetMainObjMode.Create);
      if (ic == null)
        return null;
      return ic.GetItemIfExists<T>(keys, persistance, lockTimeout);
    }

    /// <summary>
    /// Очистка данных выбранного типа.
    /// Если для данного типа данных еще не было вызова GetItem(), никаких действий не выполняется.
    /// Рекомендуется вызывать нетипизированную версию метода Clear() с аргументом objType.
    /// </summary>
    /// <typeparam name="T">Тип очищаемых данных</typeparam>
    /// <param name="keys">Ключи могут задавать либо точный объект, либо подмножество, подлежащее удалению</param>
    public static void Clear<T>(string[] keys)
      where T : class
    {
      Clear(typeof(T), keys);
    }


    /// <summary>
    /// Очистка данных выбранного типа.
    /// Если для данного типа данных еще не было вызова GetItem(), никаких действий не выполняется
    /// </summary>
    /// <param name="objType">Тип очищаемых данных</param>
    /// <param name="keys">Ключи могут задавать либо точный объект, либо подмножество, подлежащее удалению</param>
    public static void Clear(Type objType, string[] keys)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      InternalCache MainObj = GetMainObj(GetMainObjMode.CreateIfPersistDir);
      if (MainObj != null)
        MainObj.Clear(objType, keys, true);
    }

    /// <summary>
    /// Очистка всех хранящихся данных
    /// Метод используется для реализации команды "Сброс буферизации данных" или аналогичной
    /// Если IsActive=false, никаких действий не выполняется.
    /// Если задан каталог для хранения устойчивых данных (свойство CacheParams.PersistDir),
    /// то он рекурсивно очищается. Это необходимо, так как в каталоге могут быть файлы для типов кэшируемых объектов,
    /// которые еще не использовались в текущем сеансе работы, но которые также становятся неактуальными.
    /// </summary>
    public static void Clear()
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.CreateIfPersistDir);
      if (MainObj != null)
        MainObj.Clear(true);
    }

    /// <summary>
    /// Освобождение оперативной памяти.
    /// Выполняются те же действия, что и при нехватке оперативной памяти, только более полно, не учитывая наличие свободной памяти
    /// Данные в режиме MemoryOnly очищаются.
    /// Данные в режиме MemoryAndTempDir записываются на диск и удаляются из памяти.
    /// Данные в режиме MemoryAndPersist удаляются из памяти но остаются на диске.
    /// 
    /// Этот метод вряд ли стоит вызывать из прикладного кода.
    /// </summary>
    public static void FreeMemory()
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.FreeMemory();
    }


    /// <summary>
    /// Вспомогательный метод замены буферизованных данных.
    /// Выполняет очистку существующей записи кэша и замену объекта
    /// Обычно следует использовать метод GetItem(), передавая ему фабрику для загрузки данных по необходимости.
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="newValue">Объект, который надо поместить в кэш</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static void SetItem<T>(string[] keys, CachePersistance persistance, T newValue)
      where T : class
    {
      persistance = CorrectPersistance(persistance);
      if (newValue == null)
        throw new ArgumentNullException("newValue");

      GetMainObj(GetMainObjMode.Create).SetItem<T>(keys, persistance, newValue, Params.LockTimeout);
    }

    /// <summary>
    /// Вспомогательный метод записи буферизованных данных, если их еще не существует.
    /// Если данные уже есть в кэше, они не заменяются.
    /// Имеет смысл использовать вместо SetItem() в режимах <paramref name="persistance"/>, отличных от MemoryOnly, 
    /// если данные являются редко меняющимися и есть вероятность того, что кэш записывается в нескольких местах приложения.
    /// Эквивалентно вызову: if(GetItemIfExists(...)!=null) SetItem(...);, 
    /// но выполняется быстрее, т.к. данные не считываются из файла, а только проверяется существание файла.
    /// Этот метод перехватывет исключения. 
    /// Используется тайм-аут в 1 мс, независимо от установок CacheParams.LockTimeout.
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="newValue">Объект, который надо поместить в кэш</param>
    /// <returns>True, если данные были помещены в кэш. false, если данные уже были или возникла ошибка</returns>
    [DebuggerStepThrough]
    public static bool SetItemIfNew<T>(string[] keys, CachePersistance persistance, T newValue)
      where T : class
    {
      persistance = CorrectPersistance(persistance);
      if (newValue == null)
        throw new ArgumentNullException("newValue");

      bool res;
      try
      {
        res = GetMainObj(GetMainObjMode.Create).SetItemIfNew<T>(keys, persistance, newValue, 1);
      }
      catch
      {
        res = false;
      }
      return res;
    }

    /// <summary>
    /// Вспомогательный метод замены буферизованных данных.
    /// Выполняет очистку существующей записи кэша и замену объекта
    /// Обычно следует использовать метод GetItem(), передавая ему фабрику для загрузки данных по необходимости.
    /// Первый вызов метода активирует систему кэширования
    /// </summary>
    /// <typeparam name="T">Тип буферизованных данных</typeparam>
    /// <param name="keys">Массив ключей для поиска объекта. Для заданного типа данных длина списка ключей должна быть всегда одинаковой.
    /// Длина ключа не может быть меньше 1. Ключи используются для организации структуры каталогов хранилища с соответствующими ограничениями на допустимые символы</param>
    /// <param name="persistance">Место хранения данных</param>
    /// <param name="newValue">Объект, который надо поместить в кэш</param>
    /// <param name="lockTimeout">Максимальное время ожидания блокировки страницы кэша в миллисекундах. 
    /// Значение должно быть больше нуля или равно System.Threading.Timeout.Infinite</param>
    /// <returns>Сохраненный или созданный объект</returns>
    public static void SetItem<T>(string[] keys, CachePersistance persistance, T newValue, int lockTimeout)
      where T : class
    {
      persistance = CorrectPersistance(persistance);
      if (newValue == null)
        throw new ArgumentNullException("newValue");

      GetMainObj(GetMainObjMode.Create).SetItem<T>(keys, persistance, newValue, lockTimeout);
    }



    /// <summary>
    /// Возвращает true, если система буферизации в данный момент активна.
    /// Так как обращение к кэшу является многопоточным, этот метод можно использовать только в отладочных целях
    /// </summary>
    public static bool IsActive
    {
      get
      {
        //return !Object.ReferenceEquals(_MainObjRef, null); // можно без блокировки
        // 27.12.2020
        return !Object.ReferenceEquals(_MainObjRef.Target, null); // можно без блокировки
      }
    }

    /// <summary>
    /// Получение статистики работы кэша для заданного типа данных
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой объект статистики.
    /// В статистике есть только значения, для которых CacheStat.IsTotalOnly() возвращает false.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить статистику</param>
    /// <returns>Объект со счетчиками статистики</returns>
    public static CacheStat GetStat(Type objType)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj == null)
        return new CacheStat();
      else
        return MainObj.GetStat(objType);
    }

    /// <summary>
    /// Получение статистики работы кэша по всем типам данных, для которых было хотя бы одно обращение к
    /// методу GetItem().
    /// Если не было ни одного вызова GetItem(), возвращаются пустые массивы.
    /// Статистика возвращается и для тех типов данных, для которых сейчас нет хранящихся данных.
    /// </summary>
    /// <param name="objTypes">Сюда записывается массив типов хранимых данных, для которых было обращение GetItem()</param>
    /// <param name="stats">Массив статистики по каждому типу. Длина массива равна <paramref name="objTypes"/>.
    /// В статистике есть только значения, для которых CacheStat.IsTotalOnly() возвращает false</param>
    /// <returns>Общая статистика, включая статистику, которая не делится по типам данных</returns>
    public static CacheStat GetStat(out Type[] objTypes, out CacheStat[] stats)
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj == null)
      {
        objTypes = new Type[0];
        stats = new CacheStat[0];
        return new CacheStat();
      }
      else
      {
        CacheStat totalStat;
        MainObj.GetStat(out objTypes, out stats, out totalStat);
        //CacheStat Res = new CacheStat();

        //for (int i = 0; i < stats.Length; i++)
        //  Res.Add(stats[i]);
        //Res.Add(totalStat);

        //return Res;

        // 06.01.2021
        // Теперь totalStats содержит полную статистику
        return totalStat;
      }
    }




    /// <summary>
    /// Получение итоговой статистики обращений ко всем типам объектов, включая статистику, которая не делится по типам данных.
    /// Если система буферизации еще не запущена, возвращается пустая статистика.
    /// Если требуется только какое-то одно значение, а не вся статистика, используйте GetStatValue()
    /// </summary>
    /// <returns>Объект статистики</returns>
    public static CacheStat GetStat()
    {
      //Type[] objTypes;
      //CacheStat[] stats;
      //return GetStat(out objTypes, out stats);

      // 06.01.2021
      // Не надо собирать все
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj == null)
        return new CacheStat();
      else
        return MainObj.GetStat();
    }


    /// <summary>
    /// Получение одного значения из итоговой статистики обращений ко всем типам объектов.
    /// Если система буферизации еще не запущена, возвращается 0.
    /// Если требуется несколько значений, используйте GetStat() и обращайтесь к свойствам объекта CacheStat.
    /// </summary>
    /// <param name="statParam">Параметр</param>
    /// <returns>Значение параметра общей статистики или 0</returns>
    public static long GetStatValue(CacheStatParam statParam)
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj == null)
        return 0L;
      else
        return MainObj.GetStatValue(statParam);
    }

    /// <summary>
    /// Получение списка ключей для буфезованных данных заданного типа.
    /// Первый индекс возвращаемого массива соответствует хранимым объектам.
    /// Второй индекс соответствует ключам, используемым при вызове GetItem().
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой массив.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить ключи</param>
    /// <returns>Массив ключей</returns>
    public static string[][] GetKeys(Type objType)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      List<string[]> KeyList = new List<string[]>();
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.GetKeys(objType, KeyList);
      return KeyList.ToArray();
    }

    /// <summary>
    /// Получение списка ключей для буфезованных данных заданного типа.
    /// Возвращается текст в формате CSV.
    /// Каждая строка соответствует одному хранимому объекту.
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой текст.
    /// Метод используется в отладочных целях.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить ключи</param>
    /// <returns>Текстовое представление ключей</returns>
    public static string GetKeysCommaString(Type objType)
    {
      StringBuilder sb = new StringBuilder();
      GetKeysCommaString(sb, objType);
      return sb.ToString();
    }

    /// <summary>
    /// Получение списка ключей для буфезованных данных заданного типа.
    /// Возвращается текст в формате CSV.
    /// Каждая строка соответствует одному хранимому объекту.
    /// Если для данного типа данных еще не было вызова GetItem(), возвращается пустой текст.
    /// Метод используется в отладочных целях.
    /// </summary>
    /// <param name="objType">Тип хранимых данных, по которому нужно получить ключи</param>
    /// <param name="sb">Сюда записывается текстовое представление ключей</param>
    public static void GetKeysCommaString(StringBuilder sb, Type objType)
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.GetKeysCommaString(objType, sb);
    }

    /// <summary>
    /// Установка версии для ветви кэша.
    /// Контроль версий предназначен, в основном, для элементов, которые записываются в постоянный каталог.
    /// При установке версии выполняется очистка ветви кэша, если новая версия отличается от текущей.
    /// Все элементы кэща с уровнем ключей, больше заданного, будут иметь такую же версию кэша
    /// До вызова метода считается, что ветвь кэша имеет версию "".
    /// Используются только строковые версии. При необходимости работы с версиями других типов, они должны быть
    /// преобразованы в строку. Система кэширования проверяет версии только на равенство, не определяется, является
    /// ли версия "новее" или "старше".
    /// Заданная версия сохраняется до конца работы AppDomain и переживает даже полную очистку кэша методом Clear().
    /// Следует избегать задания версий для слишком большого количества ключей, так как версии занимают память.
    /// Порядок ключей для объектов следует определять так, чтобы версию можно было задавать для ключа верхнего уровня.
    /// </summary>
    /// <param name="objType">Тип хранимых данных</param>
    /// <param name="keys">Массив ключей для ветви кэша. Может быть пустым массивом. 
    /// Длина массива должна быть всегда одинаковой для заданного типа</param>
    /// <param name="version">Версия. Не может быть пустой строкой</param>
    public static CacheSetVersionResult SetVersion(Type objType, string[] keys, string version)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");
      if (String.IsNullOrEmpty(version))
        throw new ArgumentNullException("version");

      return GetMainObj(GetMainObjMode.Create).SetVersion(objType, keys, version);
    }

    /// <summary>
    /// Синхронизированная установка версии для ветви кэша.
    /// Используется, когда есть несколько типов кэшируемых данных, относящихся к одному источнику (например, кэши страниц классификатора адресов ФИАС).
    /// Позволяет обойтись единственным файлом version.txt на диске.
    /// Сначала вызывается SetVersion() для главного типа данных. Затем вызывается SyncVersion(), вместо SetVersion(), для остальных типов данных.
    /// Это выполняется и при начальной инициализации и при возможном обновлении версии источника.
    /// </summary>
    /// <param name="objType">Тип хранимых данных ("дополнительный" тип)</param>
    /// <param name="keys">Массив ключей для ветви кэша. Может быть пустым массивом.</param>
    /// <param name="setResult">Результат вызова SetVersion() для "основного" типа данных</param>
    public static void SyncVersion(Type objType, string[] keys, CacheSetVersionResult setResult)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");
      if (setResult == null)
        throw new ArgumentNullException("setResult");

      GetMainObj(GetMainObjMode.Create).SyncVersion(objType, keys, setResult);
    }

    /// <summary>
    /// Возвращает версию, установленную для ветви кэша.
    /// Длина массива ключей может быть больше, чем использованная при установке версии.
    /// При этом для дочерних уровней возвращается версия, заданная для базового уровня.
    /// Если метод SetVersion() или SyncVersion() не вызывался, возвращается пустая строка.
    /// Наличие и содержимое файла version.txt не учитывается.
    /// </summary>
    /// <param name="objType">Тип хранимых данных</param>
    /// <param name="keys">Массив ключей для ветви кэша. Может быть пустым массивом</param>
    /// <returns>Версия</returns>
    public static string GetVersion(Type objType, string[] keys)
    {
      if (objType == null)
        throw new ArgumentNullException("objType");

      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        return MainObj.GetVesion(objType, keys);
      else
        return string.Empty;
    }

    #endregion

#region Экземпляр InternalCache

    private enum GetMainObjMode
    {
      Create,
      DontCreate,
      CreateIfPersistDir
    }

    /// <summary>
    /// Ссылка на единственный экземпляр объекта
    /// При обращении выполняется блокировка lock (typeof(InternalCache))
    /// </summary>
    private static AutoDisposeReference<InternalCache> _MainObjRef;

    private static InternalCache GetMainObj(GetMainObjMode mode)
    {
      lock (typeof(InternalCache))
      {
        InternalCache Res = _MainObjRef.Target;
        if (Res == null)
        {
          if (mode == GetMainObjMode.CreateIfPersistDir)
            mode = _StaticParams.PersistDir.IsEmpty ? GetMainObjMode.DontCreate : GetMainObjMode.Create;

          if (mode == GetMainObjMode.Create)
          {
            Res = new InternalCache(_StaticParams);
            _MainObjRef = new AutoDisposeReference<InternalCache>(Res);
            _StartTime = DateTime.Now;
          }
        }
        return Res;
      }
    }

    /// <summary>
    /// Возвращает время запуска системы кэширования или null, если она еще не запущена
    /// </summary>
    public static DateTime? StartTime
    {
      get
      {
        lock (typeof(InternalCache))
        {
          return _StartTime;
        }
      }
    }
    private static DateTime? _StartTime;

    #endregion

#region Освобождение при нехватке оперативной памяти

    /// <summary>
    /// Выполнить проверку доступной оперативной памяти и удалить, при необходимости, часть или все элементы кэша.
    /// Обычно эти действия выполняются по таймеру с интервалом, определяемым свойством Params.CheckMemoryInterval,
    /// но может потребоваться выполнить внеплановую проверку.
    /// </summary>
    public static void CheckMemory()
    {
      InternalCache MainObj = GetMainObj(GetMainObjMode.DontCreate);
      if (MainObj != null)
        MainObj.CheckMemory(CacheStatParam.CheckMemoryManualCallCount);
    }


    #endregion

#region Трассировка

    /// <summary>
    /// Управляет трассировкой работы с кэшем
    /// </summary>
    public static readonly BooleanSwitch TraceSwitch = new BooleanSwitch("TraceCache", "Трассировка создания / удаления элементов кэша и сброса/загрузки с диска");

    private static string GetTracePrefix(Type type, string[] keys)
    {
      StringBuilder sbTrace = new StringBuilder();
      sbTrace.Append("Cache <");
      sbTrace.Append(type.ToString());
      sbTrace.Append("> Keys [");
      if (keys != null)
      {
        for (int i = 0; i < keys.Length; i++)
        {
          if (i > 0)
            sbTrace.Append(",");
          sbTrace.Append(keys[i]);
        }
      }
      sbTrace.Append("]. ");
      return sbTrace.ToString();
    }

    #endregion

#region log-файл

    internal static void LogoutCache(LogoutInfoNeededEventArgs args)
    {
      args.WriteHeader("Cache");
      args.WriteLine("CacheParams");
      args.IndentLevel++;
      LogoutTools.LogoutObject(args, Cache.Params);
      args.IndentLevel--;

      Type[] objTypes;
      CacheStat[] stats;
      CacheStat totalStat = Cache.GetStat(out objTypes, out stats);
      for (int i = 0; i < objTypes.Length; i++)
      {
        args.WriteLine(objTypes[i].FullName);
        LogoutCacheStat(args, stats[i], false);
      }
      args.WriteLine("Общая статистика");
      LogoutCacheStat(args, totalStat, true);
      args.IndentLevel--;
    }

    private static void LogoutCacheStat(LogoutInfoNeededEventArgs args, CacheStat stat, bool isTotal)
    {
      int MaxL = 1;
      foreach (KeyValuePair<CacheStatParam, long> Pair in stat)
        MaxL = Math.Max(MaxL, Pair.Key.ToString().Length);

      args.IndentLevel++;
      foreach (KeyValuePair<CacheStatParam, long> Pair in stat)
      {
        if (!isTotal)
        {
          if (CacheStat.IsTotalOnly(Pair.Key))
            continue;
        }

        string s = Pair.Value.ToString().PadRight(8);
        switch (Pair.Key)
        {
          case CacheStatParam.ItemCount:
            if (isTotal && Params.Capacity > 0)
            {
              s += " (" + ((double)(Pair.Value) * 100 / (double)(Params.Capacity)).ToString("0.0").PadLeft(5) + "%)";
            }
            break;
          case CacheStatParam.CreateCount:
          case CacheStatParam.LoadFileCount:
          case CacheStatParam.FromMemCount:
          case CacheStatParam.FromMemSerializedCount:
            if (stat[CacheStatParam.AccessCount] > 0)
            {
              s += " (" + ((double)(Pair.Value) * 100 / (double)(stat[CacheStatParam.AccessCount])).ToString("0.0").PadLeft(5) + "%)";
            }
            break;
          case CacheStatParam.SerializationBufferCount:
            if (stat[CacheStatParam.ItemCount] > 0)
            {
              s += " (" + ((double)(Pair.Value) * 100 / (double)(stat[CacheStatParam.ItemCount])).ToString("0.0").PadLeft(5) + "%)";
            }
            break;
          case CacheStatParam.SerializationBufferBytes:
            if (isTotal && Params.SerializationBufferBytesLimit > 0)
            {
              s += " (" + ((double)(Pair.Value) * 100 / (double)(Params.SerializationBufferBytesLimit)).ToString("0.0").PadLeft(5) + "%)";
            }
            break;
          case CacheStatParam.DelayedWriteQueueLength:
            if (isTotal && Params.DelayedWriteQueueCapacity> 0)
            {
              s += " (" + ((double)(Pair.Value) * 100 / (double)(Params.DelayedWriteQueueCapacity)).ToString("0.0").PadLeft(5) + "%)";
            }
            break;
        }
        args.WritePair(Pair.Key.ToString().PadRight(MaxL), s);
      }
      args.IndentLevel--;
    }

    #endregion
  }

#region Перечисление CacheStatParam

  /// <summary>
  /// Индексатор параметров статистики
  /// </summary>
  public enum CacheStatParam
  {
    // При добавлении элементов не забыть исправить константу CacheStat.ParamCount !

    /// <summary>
    /// Количество буферизованных в настоящий момент объектов.
    /// Может быть 0, если все объекты были удалены из буфера
    /// </summary>
    ItemCount,

    /// <summary>
    /// Количество добавленных в буфер объектов
    /// </summary>
    AddCount,

    /// <summary>
    /// Количество удаленных из буфера объектов.
    /// Включает объекты, явно удаленные вызовом Clear(), и вытесненные в связи с переполнением буфера
    /// </summary>
    RemoveCount,

    /// <summary>
    /// Количество объектов, удаленных из буфера по причине нехватки памяти (из общего количества RemoveCount).
    /// </summary>
    LowMemoryRemoveCount,

    /// <summary>
    /// Количество вызовов GetItem()
    /// </summary>
    AccessCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которые находились в оперативной памяти в готовом виде
    /// </summary>
    FromMemCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которые находились в оперативной памяти в сериализованном
    /// формате, подготовленном для записи в файл на диске.
    /// Это значение обычно меньше, чем DeserializationCount, т.к. десериализация в нормальных условиях выполняется
    /// из файла, а не из памяти.
    /// </summary>
    FromMemSerializedCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которых не было в буфере, и которые пришлось создавать
    /// </summary>
    CreateCount,

    /// <summary>
    /// Количество обращений GetItem() к объектам, которые были загружены с диска
    /// </summary>
    LoadFileCount,

    /// <summary>
    /// Количество попыток загрузки объектов с диска, закончившихся неудачей. Такие объекты пришлось 
    /// создать повторно
    /// </summary>
    LoadFileErrors,

    /// <summary>
    /// Количество записанных файлов на диск
    /// </summary>
    SaveFileCount,

    /// <summary>
    /// Количество записанных файлов на диск по причине нехватки памяти
    /// </summary>
    LowMemorySaveFileCount,

    /// <summary>
    /// Количество ошибок, возникших при записи файлов на диск
    /// </summary>
    SaveFileErrors,

    /// <summary>
    /// Количество выполненных удалений буферизованных данных с диска
    /// Включает объекты, явно удаленные вызовом Clear(), и вытесненные в связи с переполнением буфера
    /// </summary>
    DelFileCount,

    /// <summary>
    /// Количество ошибок, возникших при попытке удаления файлов, например, при обрашении из разных потоков
    /// </summary>
    DelFileErrors,

    /// <summary>
    /// Количество выполненных сериализаций объектов.
    /// Сериализация выполняется в потоке, в котором было помещено значение в кэш (в режимах, отличных от MemoryOnly).
    /// Нельзя выполнять асинхронную отложенную сериализацию, т.к. многие типы данных, например, DataTable, 
    /// ее не поддерживают и могут переходить в неправильное состояние.
    /// </summary>
    SerializationCount,

    /// <summary>
    /// Количество выполненных десериализаций объектов.
    /// Десериализация выполняется при загрузе страницы кэша из постоянного или временного файла.
    /// Также может выполняться из памяти, если отложенная запись файла еще не была выполнена.
    /// </summary>
    DeserializationCount,

    /// <summary>
    /// Количество элементов кэша, для которых в памяти хранятся сериализованные данные
    /// </summary>
    SerializationBufferCount,

    /// <summary>
    /// Количество байт в памяти, где хранятся сериализованные данные
    /// </summary>
    SerializationBufferBytes,
    /// <summary>
    /// Количество элементов, помещенных в очередь на запись в файл.
    /// </summary>
    DelayedWriteStarted,

    /// <summary>
    /// Количество элементов, которые были помещены в очередь на очередь на запись в файл и успешно сохранены по таймеру
    /// </summary>
    DelayedWriteFinished,

    /// <summary>
    /// Текущая длина очереди на запись.
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    DelayedWriteQueueLength,

    /// <summary>
    /// Количество элементов, помещенных в очередь на удаление.
    /// Обычно элементы удаляются немедленно методом Cache.Clear(), но некоторые могут быть в этот момент заблокированы.
    /// Тогда они удаляются асинхронно по таймеру.
    /// </summary>
    DelayedDelStarted,

    /// <summary>
    /// Количество элементов, которые были помещены в очередь на удаление и успешно удалены по таймеру
    /// </summary>
    DelayedDelFinished,

    /// <summary>
    /// Текущая длина очереди на отложенное удаление.
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    DelayedDelQueueLength,

    /// <summary>
    /// Количество циклов фоновой процедуры, которая выполняет отложенную запись файлов и удаление записей.
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    BackgroundCircleCount,

    /// <summary>
    /// Общее количество вызовов метода Cache.CheckMemory() по таймеру
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    CheckMemoryTimerCallCount,

    /// <summary>
    /// Общее количество вызовов метода Cache.CheckMemory() из прикладного кода
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    CheckMemoryManualCallCount,

    /// <summary>
    /// Общее количество вызовов метода Cache.CheckMemory() из за возникновения исключения OutOfMemoryException
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    CheckMemoryEmergencyCallCount,

    /// <summary>
    /// Количество потоков, которые ожидают выполнения CheckMemory().
    /// Один из них выполняет сжатие, а остальные - ждут завершения.
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    CheckMemoryWaitThreadCount,

#if USE_CHECKMEMORYTIME

    /// <summary>
    /// Время, в миллисекундах, затраченное на ожидание потоком вызов CheckMemory() из другого потока
    /// </summary>
    CheckMemoryWaitTime,

#endif

    /// <summary>
    /// CheckMemory(). Количество вызовов, при которых MemoryTools.CheckSufficientMemory() вернул false
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    InsufficientMemoryTotalCount,

    /// <summary>
    /// CheckMemory(). Количество InsufficientMemoryTotalCount, которые удалось исправить
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    InsufficientMemoryResolvedCount,

    /// <summary>
    /// CheckMemory(). Количество вызовов, при которых MemoryTools.MemoryLoad вернуло значение, большее Params.CriticalMemoryLoad.
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    CriticalMemoryLoadTotalCount,

    /// <summary>
    /// CheckMemory(). Количество MemoryLoadTotalCount, которые удалось исправить
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// </summary>
    CriticalMemoryLoadResolvedCount,

    /// <summary>
    /// Количество аварий при вызове CheckMemory(), которые привели к полной очистке кэша.
    /// Это значение доступно только для общей статистики, а не для одного типа данных.
    /// При этом вся другая статистика становится неверным
    /// </summary>
    EmergencyClearCount,
  }

  #endregion

  /// <summary>
  /// Объект статистики кэширования для одного типа данных, или итоговой статистики по всем типам.
  /// Объекты возвращаются методом Cache.GetStat()
  /// Объект содержит коллекцию числовых значений статистики, индексируемых перечислением CacheStatParam.
  /// Сам по себе этот класс не является потокобезопасным в части записи, но методы Cache.GetStat() возвращают каждый раз новые экземпляры объектов.
  /// </summary>
  [Serializable]
  public sealed class CacheStat : ICloneable, IEnumerable<KeyValuePair<CacheStatParam, long>>
  {
#region Статические методы

    internal const int ParamCount = (int)(CacheStatParam.EmergencyClearCount) + 1;

    /// <summary>
    /// Возвращает true, если данный параметр доступен только для итоговой статистики, а не для отдельных объектов
    /// </summary>
    /// <param name="statParam">Параметр</param>
    /// <returns>Только итог?</returns>
    public static bool IsTotalOnly(CacheStatParam statParam)
    {
      switch (statParam)
      {
        case CacheStatParam.DelayedWriteQueueLength:
        case CacheStatParam.DelayedDelQueueLength:
        case CacheStatParam.BackgroundCircleCount:
        case CacheStatParam.CheckMemoryTimerCallCount:
        case CacheStatParam.CheckMemoryManualCallCount:
        case CacheStatParam.CheckMemoryEmergencyCallCount:
        case CacheStatParam.CheckMemoryWaitThreadCount:
        case CacheStatParam.InsufficientMemoryTotalCount:
        case CacheStatParam.InsufficientMemoryResolvedCount:
        case CacheStatParam.CriticalMemoryLoadTotalCount:
        case CacheStatParam.CriticalMemoryLoadResolvedCount:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Возвращает true, если данный параметр статистики содержит накапливаемое значение.
    /// Возвращает false, если данный параметр статистики содержит значение, действующее только в данный момент времени
    /// </summary>
    /// <param name="statParam">Параметр</param>
    /// <returns>Только итог?</returns>
    public static bool IsIncremental(CacheStatParam statParam)
    {
      switch (statParam)
      {
        case CacheStatParam.DelayedWriteQueueLength:
        case CacheStatParam.DelayedDelQueueLength:
        case CacheStatParam.ItemCount:
        case CacheStatParam.CheckMemoryWaitThreadCount:
        case CacheStatParam.SerializationBufferCount:
        case CacheStatParam.SerializationBufferBytes:
          return false;
        default:
          return true;
      }
    }

    #endregion

#region Конструктор

    /// <summary>
    /// Создает новый объект с нулевыми значениями
    /// </summary>
    public CacheStat()
    {
      _Items = new long[ParamCount];
    }

    #endregion

#region Доступ к значениям

    /// <summary>
    /// Получить значение счетчика
    /// </summary>
    /// <param name="statParam">Идентификатор счетчик</param>
    /// <returns>Значение</returns>
    public long this[CacheStatParam statParam]
    {
      get
      {
        return _Items[(int)statParam];
      }
      internal set
      {
        _Items[(int)statParam] = value;
      }
    }

    private readonly long[] _Items;

    #endregion

#region Математические действия

    /// <summary>
    /// Добавляет значения к текущему объекту
    /// </summary>
    /// <param name="src">Добавляемый объект</param>
    public void Add(CacheStat src)
    {
      unchecked
      {
        for (int i = 0; i < ParamCount; i++)
          _Items[i] += src._Items[i];
      }
    }

    /// <summary>
    /// Вычитает значения из текущего объекта
    /// </summary>
    /// <param name="src">Добавляемый объект</param>
    public void Substract(CacheStat src)
    {
      unchecked
      {
        for (int i = 0; i < ParamCount; i++)
          _Items[i] -= src._Items[i];
      }
    }

    /// <summary>
    /// Увеличивет на 1 значение выбранного параметра
    /// </summary>
    /// <param name="statParam">Изменяемый параметр</param>
    internal void Inc(CacheStatParam statParam)
    {
      unchecked
      {
        _Items[(int)statParam]++;
      }
    }

    /// <summary>
    /// Увеличивет на значение выбранного параметра на произвольное значение
    /// </summary>
    /// <param name="statParam">Изменяемый параметр</param>
    /// <param name="delta">На сколько нужно увеличить значение</param>
    internal void Inc(CacheStatParam statParam, long delta)
    {
      unchecked
      {
        _Items[(int)statParam] += delta;
      }
    }

    /// <summary>
    /// Уменьшает на 1 значение выбранного параметра
    /// </summary>
    /// <param name="statParam">Изменяемый параметр</param>
    internal void Dec(CacheStatParam statParam)
    {
      unchecked
      {
        _Items[(int)statParam]--;
      }
    }

    /// <summary>
    /// Складывает два набора статистики и помещает их в новый набор.
    /// Исходные наборы не меняются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат сложения</returns>
    public static CacheStat operator +(CacheStat a, CacheStat b)
    {
      CacheStat res = a.Clone();
      res.Add(b);
      return res;
    }

    /// <summary>
    /// Вычитает один набор статистики из другого и помещает результат в новый набор.
    /// Исходные наборы не меняются.
    /// </summary>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns>Результат вычитания</returns>
    public static CacheStat operator -(CacheStat a, CacheStat b)
    {
      CacheStat res = a.Clone();
      res.Substract(b);
      return res;
    }

    #endregion

#region ICloneable Members

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Копия</returns>
    public CacheStat Clone()
    {
      CacheStat Res = new CacheStat();
      Res.Add(this);
      return Res;
    }

    #endregion

#region IEnumerable<KeyValuePair<CacheStatParam,long>> Members

    /// <summary>
    /// Перечислитель
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<KeyValuePair<CacheStatParam, long>>
    {
#region Конструктор

      internal Enumerator(CacheStat owner)
      {
        _Owner = owner;
        _Index = -1;
      }

      #endregion

#region Поля

      private readonly CacheStat _Owner;

      private int _Index;

      #endregion

#region Методы перечислителя

      /// <summary>
      /// Текущее значение
      /// </summary>
      public KeyValuePair<CacheStatParam, long> Current
      {
        get { return new KeyValuePair<CacheStatParam, long>((CacheStatParam)_Index, _Owner[(CacheStatParam)_Index]); }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      /// <summary>
      /// Переход к следующему элементу
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _Index++;
        return _Index < CacheStat.ParamCount;
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель, в котором доступны пары Параметр:Значение
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<KeyValuePair<CacheStatParam, long>> IEnumerable<KeyValuePair<CacheStatParam, long>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }
}

#endif