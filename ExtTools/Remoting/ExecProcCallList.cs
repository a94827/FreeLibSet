// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#define DEBUG_THREADS // Если объявлено, то потоки будут проверяться и в Release-сборке
#if DEBUG
#define DEBUG_THREADS
#endif

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Logging;
using System.Threading;
using FreeLibSet.Core;

/*
 * Вызов процедур ExecProc с централизованной обработкой
 */

namespace FreeLibSet.Remoting
{
  #region Делегаты события Finished

  /// <summary>
  /// Аргументы события ExecProcCallItem.Finished
  /// </summary>
  public class ExecProcCallEventArgs : EventArgs
  {
    #region Конструкторы

    internal ExecProcCallEventArgs(ExecProcCallItem item, NamedValues results, Exception exception)
    {
      _Item = item;
      _Results = results;
      _Exception = exception;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект, для которого вызвано событие
    /// </summary>
    public ExecProcCallItem Item { get { return _Item; } }
    private readonly ExecProcCallItem _Item;

    /// <summary>
    /// Результаты выполнения процедуры, если она завершена успешно
    /// </summary>
    public NamedValues Results { get { return _Results; } }
    private readonly NamedValues _Results;

    /// <summary>
    /// Объект исключения, если событие Finished вызвано по причине аварийного завершения процедуры
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private readonly Exception _Exception;

    #endregion
  }

  /// <summary>
  /// Делегат события ExecProcCallItem.Finished
  /// </summary>
  /// <param name="sender">Объект для которого вызвано событие</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ExecProcCallEventHandler(object sender, ExecProcCallEventArgs args);

  /// <summary>
  /// Аргументы события DistributedProcCallItem.Finished
  /// </summary>
  public class DistributedProcCallEventArgs : EventArgs
  {
    #region Конструкторы

    internal DistributedProcCallEventArgs(DistributedProcCallItem item, NamedValues results, Exception exception)
    {
      _Item = item;
      _Results = results;
      _Exception = exception;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект, для которого вызвано событие
    /// </summary>
    public DistributedProcCallItem Item { get { return _Item; } }
    private readonly DistributedProcCallItem _Item;

    /// <summary>
    /// Результаты выполнения процедуры, если она завершена успешно
    /// </summary>
    public NamedValues Results { get { return _Results; } }
    private readonly NamedValues _Results;

    /// <summary>
    /// Объект исключения, если событие Finished вызвано по причине аварийного завершения процедуры
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private readonly Exception _Exception;

    #endregion
  }

  /// <summary>
  /// Делегат события DistributedProcCallItem.Finished
  /// </summary>
  /// <param name="sender">Объект для которого вызвано событие</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DistributedProcCallEventHandler(object sender, DistributedProcCallEventArgs args);

  #endregion

  #region Перечисление ExecProcCallItemState

  /// <summary>
  /// Текущее состояние объекта ExecProcCallItem
  /// </summary>
  public enum ExecProcCallItemState
  {
    /// <summary>
    /// Объект ExecProcCallItem создан, но еще не запущен на выполнение
    /// </summary>
    NotStarted,

    /// <summary>
    /// Процедура выполняется
    /// </summary>
    Executing,

    /// <summary>
    /// Выполнение завершено успешно
    /// </summary>
    Finished,

    /// <summary>
    /// Выполнение завершено с ошибкой
    /// </summary>
    Failed
  }

  #endregion

  /// <summary>
  /// Базовый класс для ExecProcCallItem и DistributedProcCallItem
  /// </summary>
  public abstract class ExecProcCallItemBase : SimpleDisposableObject
  {
    #region Конструктор и Dispose

    internal ExecProcCallItemBase()
    {
      _UserData = new NamedValues();
      _State = ExecProcCallItemState.NotStarted;

#if DEBUG_THREADS
      _CallerThread = Thread.CurrentThread;
#endif
    }

    /// <summary>
    /// Очистка внутренних данных и вызов события Disposed
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
#if DEBUG_THREADS
        if (!IsAsync)
          CheckCallerThread();
#endif

        AsyncResult = null;
        AsyncResultHandler = null;
        if (SplashWatcher != null)
        {
          try
          {
            SplashWatcher.UseDefaultSplash = false;
            SplashWatcher.ClearClientStack();
          }
          catch (Exception e)
          {
            AddExceptionInfo(e);
            LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("ClientSplashWatcher.ClearClientStack()")); // тут не выйдет показать сообщение
            _State = ExecProcCallItemState.Failed; // 16.01.2020
          }
          SplashWatcher = null;
        }
      }

      base.Dispose(disposing);

      EventHandler ehDisposed = Disposed; // 12.01.2021. Учитываем возможность асинхронного присоединения и отсоединения обработчиков событий
      if (ehDisposed != null)
      {
        try
        {
          ehDisposed(this, EventArgs.Empty);
        }
        catch (Exception e)// 16.01.2020
        {
          AddExceptionInfo(e);
          LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("ExecProcCallItem.Disposed")); // тут не выйдет показать сообщение
          _State = ExecProcCallItemState.Failed;
        }
      }

      if (_State == ExecProcCallItemState.Executing)
        _State = ExecProcCallItemState.Failed; // 05.12.2019
    }

    /// <summary>
    /// Событие вызывается после завершения работы с процедурой в ExecProcCallList.
    /// На момент вызова события Disposed, событие Finished и метод ExecProc.Dispose() уже вызваны.
    /// Для синхронного вызова гарантируется, что вызов будет выполнен из этого же потока, в котором была запущена процедура.
    /// Для асинхронного вызова поток может быть любым.
    /// </summary>
    public event EventHandler Disposed;

    #endregion

    #region Поток

#if DEBUG_THREADS

    /// <summary>
    /// Поток, в котором был создан объект ExecProcCallItem
    /// </summary>
    private Thread _CallerThread;

    internal void CheckCallerThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _CallerThread))
        throw new DifferentThreadException(_CallerThread);
    }

#endif

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее состояние процедуры
    /// </summary>
    public ExecProcCallItemState State { get { return _State; } internal set { _State = value; } }
    private ExecProcCallItemState _State;

    /// <summary>
    /// Произвольные пользовательские данные, которые могут потребоваться обработчику события Finished.
    /// </summary>
    public NamedValues UserData { get { return _UserData; } }
    private readonly NamedValues _UserData;

    /// <summary>
    /// Идентификатор процедуры ExecProc
    /// </summary>
    public abstract Guid Guid { get; }

    /// <summary>
    /// Отображаемое название в списке процедур.
    /// Если свойство не задано в явном виде, используется IExecProc.DisplayName
    /// </summary>
    public string DisplayName
    {
      get { return _DisplayName; }
      set
      {
        CheckNotStarted();
#if DEBUG_THREADS
        CheckCallerThread();
#endif
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Если свойство установлено в true, то событие Finished будет вызвано и в случае аварийного
    /// завершения выполнения процедуры.
    /// По умолчанию (false), при возникновении ошибки, она обрабатывается без участия события Finished
    /// </summary>
    public bool UseFinishedWhenFailed
    {
      get { return _UseFinishedWhenFailed; }
      set
      {
        CheckNotStarted();
#if DEBUG_THREADS
        CheckCallerThread();
#endif
        _UseFinishedWhenFailed = value;
      }
    }
    private bool _UseFinishedWhenFailed;

    /// <summary>
    /// Свойство возвращает true, если процедура была запущена асинхронно.
    /// Свойство действительно только после запуска процедуры на выполнения
    /// </summary>
    public bool IsAsync { get { return _IsAsync; } }
    private bool _IsAsync;

    /// <summary>
    /// Объект процедуры ExecProc или RemoteExecProc
    /// </summary>
    internal abstract IExecProc InternalExecProc { get; }

    #endregion

    #region Событие Finished

    internal abstract bool HasFinishedEventHander { get; }

    internal abstract void OnFinished(NamedValues results, Exception exception);

    #endregion

    #region Закрытые свойства и методы

    /// <summary>
    /// Интерфейс управления асинхронным вызовом.
    /// Используется независимо от наличия SplashWatcher
    /// </summary>
    internal IAsyncResult AsyncResult;

    /// <summary>
    /// Стек заставок на стороне клиента, возвращаемый ExecProcCallList.CreateSplashStack() 
    /// </summary>
    internal ClientSplashWatcher SplashWatcher;

    /// <summary>
    /// Интерфейс управления асинхронным вызовом.
    /// Используется, если ExecProcCallList.CreateSplashStack() вернул стек заставок, которые нужно синхроизировать
    /// </summary>
    internal AsyncResultWithSplashHandler AsyncResultHandler;

    internal abstract NamedValues EndExecute(IAsyncResult asyncResult);

    #endregion

    #region IReadOnlyObject Members

    /*
    bool IReadOnlyObject.IsReadOnly { get { return FUserData.IsReadOnly; } }

    void IReadOnlyObject.CheckNotReadOnly()
    {
      if (FUserData.IsReadOnly)
        throw new ObjectReadOnlyException();
    }
    */

    private void CheckNotStarted()
    {
      if (State != ExecProcCallItemState.NotStarted)
        throw new InvalidOperationException(Res.ExecProc_Err_Started);
      // 19.08.2020
      // Проверка убрана, так как она провоцирует лишний сетевой вызов
      //if (!FreeLibSet.Remoting.ExecProc.GetCanStart(ExecProc.State))
      //  throw new InvalidOperationException(" Процедура " + ExecProc.DisplayName + " не готова к запуску, так как находится в состоянии " + ExecProc.State.ToString());
    }

    internal void SetStarted(bool isAsync)
    {
      CheckNotDisposed();
      //if (FState != ExecProcCallItemState.NotStarted)
      //  throw new InvalidOperationException("Повторное выполнение для одного и того же объекта ExecProcCallItem не допускается");
      CheckNotStarted();


      // Убрано 28.09.2017 FUserData.SetReadOnly();
      _State = ExecProcCallItemState.Executing;
      _IsAsync = isAsync;
    }

    #endregion

    #region Отладка

    internal virtual void AddExceptionInfo(Exception e)
    {
      try
      {
        e.Data["ExecProcCallItem.DisplayName"] = this.DisplayName;
        e.Data["ExecProcCallItem.State"] = this.State;
        e.Data["ExecProcCallItem.IsAsync"] = this.IsAsync;
        e.Data["ExecProcCallItem.UseFinishedWhenFailed"] = this.UseFinishedWhenFailed;
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// Процедура для вызова и обработчик завершения.
  /// Объект ExecProcCallItem (в отличие от ExecProc) является "одноразовым". 
  /// Если процедуру требуется выполнить еще раз, то следует создать новый объект (не забывая устанавливать
  /// свойство ExecProcCallItem.DisposeExecProc=false).
  /// Несмотря на реализацию интерфейса IDisposable, нет необходимости вызывать метод Dispose()
  /// из пользовательского кода. Уничтожение объекта выполняется ExecProcCallList.
  /// </summary>
  public sealed class ExecProcCallItem : ExecProcCallItemBase
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает элемент для заданной процедуры.
    /// </summary>
    /// <param name="execProc"></param>
    public ExecProcCallItem(IExecProc execProc)
    {
      if (execProc == null)
        throw new ArgumentNullException("execProc");
      _ExecProc = execProc;
      base.DisplayName = execProc.DisplayName;
    }

    /// <summary>
    /// Вызывает IExecProc.Dispose(), если <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (InternalExecProc.AutoDispose)
        {
          try
          {
            InternalExecProc.Dispose();
          }
          catch (Exception e) // 16.01.2020
          {
            AddExceptionInfo(e);
            LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("IExecProc.Dispose()")); // тут не выйдет показать сообщение
            base.State = ExecProcCallItemState.Failed;
          }
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает IExecProc.Guid
    /// </summary>
    public override Guid Guid { get { return _ExecProc.Guid; } }

    /// <summary>
    /// Объект процедуры ExecProc или RemoteExecProc
    /// </summary>
    public IExecProc ExecProc { get { return _ExecProc; } }
    internal override IExecProc InternalExecProc { get { return _ExecProc; } }
    private readonly IExecProc _ExecProc;

    internal override NamedValues EndExecute(IAsyncResult asyncResult)
    {
      return _ExecProc.EndExecute(asyncResult);
    }

    #endregion

    #region Событие Finished

    /// <summary>
    /// Событие, вызываемое при успешном завершении процедуры.
    /// Если установлено свойство UseFinishedWhenFailed, то событие будет вызвано также при аварийном
    /// завершении процедуры.
    /// Если обработчик не установлен, результаты, возвращаемые процедурой будут проигнорированы.
    /// Для синхронного вызова гарантируется, что вызов будет выполнен из этого же потока, в котором была запущена процедура.
    /// Для асинхронного вызова поток может быть любым.
    /// </summary>
    public event ExecProcCallEventHandler Finished;

    internal override bool HasFinishedEventHander { get { return Finished != null; } }

    internal override void OnFinished(NamedValues results, Exception exception)
    {
#if DEBUG_THREADS
      if (!IsAsync)
        CheckCallerThread();
#endif

      ExecProcCallEventHandler ehFinished = Finished; // 12.01.2021. Учитываем возможность асинхронного присоединения и отсоединения обработчиков событий
      if (ehFinished != null)
      {
        ExecProcCallEventArgs args = new ExecProcCallEventArgs(this, results, exception);
        ehFinished(this, args);
      }
    }

    #endregion

    #region Отладка

    internal override void AddExceptionInfo(Exception e)
    {
      base.AddExceptionInfo(e);

      try
      {
        e.Data["ExecProcCallItem.ExecProc.Info"] = this.InternalExecProc.GetInfo();
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// Процедура для вызова и обработчик завершения.
  /// Объект ExecProcCallItem (в отличие от ExecProc) является "одноразовым". 
  /// Если процедуру требуется выполнить еще раз, то следует создать новый объект (не забывая устанавливать
  /// свойство ExecProcCallItem.DisposeExecProc=false).
  /// Несмотря на реализацию интерфейса IDisposable, нет необходимости вызывать метод Dispose()
  /// из пользовательского кода. Уничтожение объекта выполняется ExecProcCallList.
  /// </summary>
  public sealed class DistributedProcCallItem : ExecProcCallItemBase
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает элемент для процедуры, начатой на стороне сервера.
    /// </summary>
    /// <param name="startData">Данные вызова ExecProc.StartDistributedCall() на стороне сервера и переданные клиенту для завершения</param>
    public DistributedProcCallItem(DistributedCallData startData)
    {
      if (startData == null)
        throw new ArgumentNullException("startData");
      base.DisplayName = startData.DisplayName;
      _DistributedProc = new RemoteDistributedProc(startData);
    }

    internal DistributedProcCallItem(RemoteDistributedProc distributed)
    {
      base.DisplayName = distributed.StartData.DisplayName;
      _DistributedProc = distributed;
    }

    /// <summary>
    /// Вызывает IExecProc.Dispose(), если <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          _DistributedProc.Dispose();
        }
        catch (Exception e)
        {
          AddExceptionInfo(e);
          LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("RemoteDistributedProc.Dispose()")); // тут не выйдет показать сообщение
          base.State = ExecProcCallItemState.Failed;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает DistributedCallData.Guid
    /// </summary>
    public override Guid Guid { get { return _DistributedProc.StartData.Guid; } }

    /// <summary>
    /// Объект процедуры RemoteExecProc или null
    /// </summary>
    internal override IExecProc InternalExecProc { get { return _DistributedProc.MainProc; } }

    /// <summary>
    /// Процедура в стиле APM.
    /// </summary>
    public RemoteDistributedProc DistributedProc { get { return _DistributedProc; } }
    private RemoteDistributedProc _DistributedProc;

    internal override NamedValues EndExecute(IAsyncResult asyncResult)
    {
      return _DistributedProc.EndExecute(asyncResult);
    }

    #endregion

    #region Событие Finished

    /// <summary>
    /// Событие, вызываемое при успешном завершении процедуры.
    /// Если установлено свойство UseFinishedWhenFailed, то событие будет вызвано также при аварийном
    /// завершении процедуры.
    /// Если обработчик не установлен, результаты, возвращаемые процедурой будут проигнорированы.
    /// Для синхронного вызова гарантируется, что вызов будет выполнен из этого же потока, в котором была запущена процедура.
    /// Для асинхронного вызова поток может быть любым.
    /// </summary>
    public event DistributedProcCallEventHandler Finished;

    internal override bool HasFinishedEventHander { get { return Finished != null; } }

    internal override void OnFinished(NamedValues results, Exception exception)
    {
#if DEBUG_THREADS
      if (!IsAsync)
        CheckCallerThread();
#endif

      DistributedProcCallEventHandler ehFinished = Finished; // 12.01.2021. Учитываем возможность асинхронного присоединения и отсоединения обработчиков событий
      if (ehFinished != null)
      {
        DistributedProcCallEventArgs args = new DistributedProcCallEventArgs(this, results, exception);
        ehFinished(this, args);
      }
    }

    #endregion

    #region Отладка

    internal override void AddExceptionInfo(Exception e)
    {
      base.AddExceptionInfo(e);

      try
      {
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// Список ожидания завершения асинхронного вызова процедур
  /// Единственный экземпляр списка создается в приложении клиента
  /// (если используется ExtForms, то можно использовать свойство EFPApp.ExecProcList) 
  /// Для запуска процедуры следует выполнить вызов метода Add(), которому передается аргумент Args и обработчик
  /// Finished. При этом выполняется ExecuteAsync()
  /// Далее по таймеру вызывается Process(). Если одна из добавленных процедур закончила работу, вызывается
  /// обработчик (в основном потоке). Если выполнение завершено с ошибкой, сохраненное исключение выбрасывается
  /// Данный класс НЕ является ПОТОКОБЕЗОПАСНЫМ. Некоторые методы должны вызываться из основного потока приложения.
  /// </summary>
  public class ExecProcCallList
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public ExecProcCallList()
    {
      _Items = new List<ExecProcCallItemBase>();

#if DEBUG_THREADS
      _MainThread = Thread.CurrentThread;
#endif
    }

    #endregion

    #region Поток

#if DEBUG_THREADS

    /// <summary>
    /// Основной поток (приложения) в котором создан ExecProcCallList
    /// </summary>
    private Thread _MainThread;

    internal void CheckMainThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _MainThread))
        throw new DifferentThreadException(_MainThread);
    }

#endif

    #endregion

    #region Список

    /// <summary>
    /// Список просматриваемых объектов.
    /// При обращении к списку он блокируется.
    /// </summary>
    private readonly List<ExecProcCallItemBase> _Items;

    /// <summary>
    /// Возвращает true, если процедура ExecProc еще не завершена (или завершение еще не обработано).
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="execProc">Искомая процедура</param>
    /// <returns></returns>
    public bool Contains(IExecProc execProc)
    {
      lock (_Items)
      {
        for (int i = 0; i < _Items.Count; i++)
        {
          if (_Items[i].InternalExecProc == execProc)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает копию массива процедур в списке.
    /// Этот метод может вызываться из любого потока
    /// </summary>
    /// <returns>Новый массив</returns>
    public IExecProc[] ToArray()
    {
      List<IExecProc> lst = new List<IExecProc>();
      lock (_Items)
        for (int i = 0; i < _Items.Count; i++)
        {
          IExecProc p = _Items[i].InternalExecProc;
          if (p != null)
            lst.Add(p);
        }
      return lst.ToArray();
    }

    /// <summary>
    /// Возвращает количество процедур в списке.
    /// К свойству можно обращаться из любого потока.
    /// Свойство можно использовать только в отладочных целях, так как список может меняться асинхронно.
    /// </summary>
    public int Count
    {
      get
      {
        lock (_Items)
        {
          return _Items.Count;
        }
      }
    }

    ///// <summary>
    ///// Возвращает процедуру по индексу
    ///// </summary>
    ///// <param name="index">Индекс в списке</param>
    ///// <returns>Объект IExecProc</returns>
    //public IExecProc this[int index]
    //{
    //  get { return _Items[index].ExecProc; }
    //}

    ///// <summary>
    ///// Возвращает отображаемое имя асинхронной процедуры.
    ///// </summary>
    ///// <param name="index">Индекс процедуры в списке ожидания</param>
    ///// <returns>Свойство IExecProc.DisplayName, если оно не было переопределено при вызове метода ExecuteAsync()</returns>
    //public string GetDisplayName(int index)
    //{
    //  return _Items[index].DisplayName;
    //}


    #endregion

    #region Создание заставки SplashStack

    /// <summary>
    /// Создает стек заставок, которым нужно управлять в процессе выполнения процедуры.
    /// Непереопределенный метод возвращает null, при этом заставки не используются.
    /// Этот метод может вызываться из любого потока, не обязательно из основного.
    /// </summary>
    /// <returns></returns>
    protected virtual ISplashStack CreateSplashStack()
    {
      //CheckMainThread();
      return null;
    }

    private bool? _SplashStackSupportedFlag;

    /// <summary>
    /// Возвращает true, если данный список поддерживает экранные заставки.
    /// Доступно из любого потока
    /// </summary>
    /// <returns></returns>
    private bool IsSplashStackSupported()
    {
      if (!_SplashStackSupportedFlag.HasValue)
        _SplashStackSupportedFlag = CreateSplashStack() != null;
      return _SplashStackSupportedFlag.Value;
    }

    #endregion

    #region Методы выполнения

    #region Для ExecProcCallItem

    #region Синхронное выполнение ExecuteSync()

    /// <summary>
    /// Синхронный метод запуска.
    /// Не рекомендуется вызывать метод для удаленной процедуры, так как может возникнуть ошибка тайм-аута.
    /// Используйте метод ExecuteAsyncAndWait().
    /// Этот метод может вызываться из любого потока, как основного, так и рабочего.
    /// </summary>
    /// <param name="item">Вызываемая процедура</param>
    /// <param name="args">Аргументы, передаваемые функции</param>
    /// <returns>Если процедура вызвана успешно, то возвращает результаты выполнения.
    /// Если в процедуре возникло исключение, но оно было обработано в обработчике Finished, то возвращается null</returns>
    public virtual NamedValues ExecuteSync(ExecProcCallItem item, NamedValues args)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      NamedValues results;

      try
      {
        try
        {
          item.SetStarted(false);
          try
          {
            if (IsSplashStackSupported())
            {
              if (item.InternalExecProc is ExecProc)
                results = DoExecuteSyncWithSyncSplash(item.InternalExecProc as ExecProc, args);
              else
                results = DoExecuteSyncWithAsyncSplash(item, args);
            }
            else
            {
              // Заставки не используются
              results = item.InternalExecProc.Execute(args);
            }
          }
          catch (Exception e2)
          {
            item.AddExceptionInfo(e2);
            OnFailed(item, e2);
            results = null;
          }

          if (results != null)
            OnFinished(item, results);
        }
        catch (Exception e1)
        {
          item.AddExceptionInfo(e1);
          throw;
        }
      }
      finally
      {
        try
        {
          item.Dispose();
        }
        catch { }
      }

      return results;
    }

    /// <summary>
    /// Выполнение с созданием заставки в текущем потоке
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private NamedValues DoExecuteSyncWithSyncSplash(ExecProc proc, NamedValues args)
    {
      NamedValues res;
      ISplashStack sstack = CreateSplashStack();
#if DEBUG
      if (sstack == null)
        throw new NullReferenceException("CreateSplashStack() returned null");
#endif

      ISplashStack oldss = proc.SplashStack;
      try
      {
        proc.SplashStack = sstack;
        // Никаких SplashWatcher'ов не нужно. Процедура сама управляет заставкой напрямую.

        res = proc.Execute(args);
      }
      finally
      {
        proc.SplashStack = oldss; // восстанавливаем, вдруг процедура будет еще раз вызвана вне этого ExecProcCallList и без заставки

        // Очищаем стек
        while (sstack.Splash != null)
          sstack.EndSplash();
      }

      return res;
    }


    private class AsyncSplashExecutor
    {
      #region Поля

      /// <summary>
      /// Процедура, которая выполнятся в основном потоке
      /// </summary>
      public ExecProcCallItem Item;

      /// <summary>
      /// Флажок, устанавливается после того, как выполнение  закончено
      /// </summary>
      public bool Finished;

      #endregion
    }

    /// <summary>
    /// Выполнение с созданием заставки в отдельном потоке
    /// </summary>
    /// <param name="item"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private NamedValues DoExecuteSyncWithAsyncSplash(ExecProcCallItem item, NamedValues args)
    {
      NamedValues res;

      AsyncSplashExecutor ase = new AsyncSplashExecutor();
      try
      {
        ase.Item = item;
        ThreadPool.QueueUserWorkItem(RunAsyncSplash, ase);
        res = item.InternalExecProc.Execute(args);
      }
      finally
      {
        ase.Finished = true;
      }

      return res;
    }

    private void RunAsyncSplash(object state)
    {
      AsyncSplashExecutor ase = (AsyncSplashExecutor)state;

      try
      {
        // Флажок Finished может быть установлен в любой момент.
        // Может быть, уже и сейчас установлен. 
        //Thread.Sleep(100); // пауза
        // this.Sleep(100); // 10.12.2019 Пусть вызывается DoEvents()
        Thread.Sleep(100); // 20.08.2020 Возвращаем обратно без вызова DoEvents


        if (ase.Finished)
          return;

        IServerSplashWatcher ssw = ase.Item.InternalExecProc.CreateSplashWatcher();
        ISplashStack sstack = CreateSplashStack();
#if DEBUG
        if (sstack == null)
          throw new NullReferenceException("CreateSplashStack() returned null");
#endif

        ClientSplashWatcher csw = new ClientSplashWatcher(ssw, sstack);
        try
        {
          csw.DefaultSplashPhaseText = String.Format(Res.ExecProc_Phase_Executing, ase.Item.DisplayName);
          csw.UseDefaultSplash = this.UseDefaultSplash;

          while (!ase.Finished)
          {
            try
            {
              for (int i = 0; i < 9; i++)
              {
                //Thread.Sleep(100);
                this.Sleep(100); // 21.01.2021. Метод Sleep разрешено вызывать асинхронно
                csw.UseDefaultSplash = this.UseDefaultSplash;
              }
              //Thread.Sleep(100);// 16.12.2019
              this.Sleep(100); // 21.01.2021

              csw.ProcessSplash();
            }
            catch { } // проглатываем ошибки, например, тайм-аута сети

          }
        }
        finally
        {
          // Очищаем стек
          csw.UseDefaultSplash = false;
          csw.ClearClientStack();
        }
      }
      catch (Exception e)
      {
        // Нельзя выпускать исключение из этого метода
        ase.Item.AddExceptionInfo(e);
        OnUnhandledException(e, LogoutTools.GetDefaultTitle());
      }
    }

    /// <summary>
    /// Синхронный метод запуска.
    /// Не рекомендуется вызывать метод для удаленной процедуры, так как может возникнуть ошибка тайм-аута.
    /// Используйте метод ExecuteAsyncAndWait().
    /// Этот метод может вызываться из любого потока, как основного, так и рабочего.
    /// </summary>
    /// <param name="execProc">Вызываемая процедура</param>
    /// <param name="args">Аргументы, передаваемые функции</param>
    /// <returns>Результаты выполнения.</returns>
    public NamedValues ExecuteSync(IExecProc execProc, NamedValues args)
    {
      ExecProcCallItem item = new ExecProcCallItem(execProc);
      return ExecuteSync(item, args);
    }

    #endregion

    #region Асинхронное выполнение ExecuteAsync()

    /// <summary>
    /// Вызывает процедуру асинхронно и добавляет текущую процедуру в список, вместе с обработчиком завершения.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="item">Процедура и обработчики</param>
    /// <param name="args">Аргументы, передаваемые функции</param>
    public virtual void ExecuteAsync(ExecProcCallItem item, NamedValues args)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      try
      {
        item.SetStarted(true);

        ISplashStack splStack = CreateSplashStack();
        if (splStack == null)
          item.AsyncResult = item.InternalExecProc.BeginExecute(args, null, null); // здесь может возникнуть исключение, если процедуру нельзя запускать
        else
        {
          IAsyncResultWithSplash ar2 = item.InternalExecProc.BeginExecuteWithSplash(args, null, null);
          item.AsyncResult = ar2;
          item.AsyncResultHandler = new AsyncResultWithSplashHandler(ar2);
          item.SplashWatcher = new ClientSplashWatcher(item.AsyncResultHandler, splStack);
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.DefaultSplashPhaseText = String.Format(Res.ExecProc_Phase_Executing, item.DisplayName);
        }
        lock (_Items)
        {
          _Items.Add(item);
        }
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);

        try
        {
          item.Dispose();
        }
        catch { }

        throw;
      }
    }

    #endregion

    #region Универсальный метод Execute()

    /// <summary>
    /// Выполняет синхронный или асинхронный вызов, в зависимости от <paramref name="isAsync"/>.
    /// Удобно, если одна процедура может вызываться и так и так.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="isAsync">true - асинхронно, false </param>
    /// <param name="item">Процедура и обработчики</param>
    /// <param name="args">Аргументы, передаваемые функции</param>
    public void Execute(bool isAsync, ExecProcCallItem item, NamedValues args)
    {
      if (isAsync)
        ExecuteAsync(item, args);
      else
        ExecuteSync(item, args);
    }

    #endregion

    #region ExecuteAsyncAndWait()

    /// <summary>
    /// Используется методом ExecuteAsyncAndWait, принимающим ExecProcCallItem.
    /// Вторая перегрузка, принимаюшая IExecProc, владеет собственным ExecProcCallItem и может использовать его как промежуточное хранилище.
    /// </summary>
    private class ExecProcExecuteAsyncAndWaitHandler
    {
      #region Поля

      public NamedValues Results;

      public Exception Exception;

      #endregion

      #region Метод

      public void Item_Finished(object sender, ExecProcCallEventArgs args)
      {
        Results = args.Results;
        Exception = args.Exception;
      }

      #endregion
    }

    /// <summary>
    /// Асинхронный вызов с ожиданием.
    /// В отличие от метода ExecuteAsync(), который сразу возвращает управление,
    /// вызов этого метода не завершается, пока выполнение не будет завершено.
    /// Метод может вызываться в любом потоке.
    /// Метод должен использоваться с удаленной процедурой, которая выполняется долго и может вызвать ошибку тайм-аута при использовании ExecuteSync(),
    /// а использовать ExecuteAsync() нельзя, так как требуется получить результаты в текущем потоке.
    /// Использование с ExecProc также допускается, хотя в некоторых случаях может быть выгоднее использовать ExecuteSync().
    /// Фактически, этот метод вызывает ExecuteAsync() и дожидается завершения процедуры. Затем результаты возвращаются в основном потоке.
    /// Если при выполнении процедуры возникло исключение, то оно перевыбрасывается в основном потоке. Предполагается, что в <paramref name="item"/>
    /// нет своей обработки ошибок.
    /// </summary>
    /// <param name="item">Объект процедуры</param>
    /// <param name="args">Аргументы процедуры</param>
    /// <returns>Результаты выполнения процедуры</returns>
    public virtual NamedValues ExecuteAsyncAndWait(ExecProcCallItem item, NamedValues args)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      // 09.12.2019
      // Создаем копию ExecProcCallItem.
      // У оригинала может быть не установлено свойство UseFinishedWhenFailed, но есть обработчик события Finished. Тогда обработчик события будет
      // вызван при появлении ошибки, к чему обработчик не готов
      ExecProcCallItem item2 = new ExecProcCallItem(item.InternalExecProc);
      item2.DisplayName = item.DisplayName;

      ExecProcExecuteAsyncAndWaitHandler tempHandler = new ExecProcExecuteAsyncAndWaitHandler();
      item2.UseFinishedWhenFailed = true;
      item2.Finished += tempHandler.Item_Finished;
      try
      {
        ExecuteAsync(item2, args);
        Thread.Sleep(0); // 19.08.2020
        // нельзя! ProcessItem(item2); // 20.08.2020

        while (item2.State == ExecProcCallItemState.Executing)
        {
          //_ProcessItemCalled = false;
          Sleep(250);
          //if (!_ProcessItemCalled)
          // нельзя!  Process(); // 20.08.2020. Делаем сами, раз переопределенный метод не вызвал
        }
      }
      finally
      {
        item2.Dispose();
        item.Dispose();
      }

      if (tempHandler.Exception != null)
      {
        tempHandler.Exception.Data["AsyncStackTrace"] = tempHandler.Exception.StackTrace;
        //OnFailed(item, TempHandler.Exception);
        // 19.12.2019
        // Не надо, чтобы вызывался виртуальный метод, который перехватит исключение.
        // Если собственный обработчик ошибок не установлен для ExecProcCallItem, то пусть исключение выбрасывается.
        DoOnFailed(item, tempHandler.Exception);
      }
      else
        OnFinished(item, tempHandler.Results);

      return tempHandler.Results;
    }

    /// <summary>
    /// Непереопределенный метод вызывает Thread.Sleep().
    /// Переопределенный метод может дополнительно обрабатывать очередь сообщений приложения, чтобы не "замораживался" пользовательский интерфейс.
    /// Этот метод может вызываться асинхронно.
    /// </summary>                                                                                       
    /// <param name="milliseconds">Интервал ожидания в миллисекундах</param>
    protected virtual void Sleep(int milliseconds)
    {
      // 21.01.2021.
      // Убрана проверка. Метод может вызываться асинхронно

      // #if DEBUG_THREADS
      //CheckMainThread();
      // #endif
      Thread.Sleep(milliseconds);
    }

    /// <summary>
    /// Асинхронный вызов с ожиданием.
    /// В отличие от метода ExecuteAsync(), который сразу возвращает управление,
    /// вызов этого метода не завершается, пока выполнение не будет завершено.
    /// Метод можно вызвать из любого потока.
    /// Метод должен использоваться с удаленной процедурой, которая выполняется долго и может вызвать ошибку тайм-аута при использовании ExecuteSync(),
    /// а использовать ExecuteAsync() нельзя, так как требуется получить результаты в текущем потоке.
    /// Использование с ExecProc также допускается.
    /// </summary>
    /// <param name="proc">Выполняемая процедура</param>
    /// <param name="args">Аргументы вызова</param>
    /// <returns>Результаты вызова</returns>
    public virtual NamedValues ExecuteAsyncAndWait(IExecProc proc, NamedValues args)
    {
      ExecProcCallItem item = new ExecProcCallItem(proc);
      item.UseFinishedWhenFailed = true;
      item.Finished += new ExecProcCallEventHandler(Item_Finished);
      ExecuteAsyncAndWait(item, args);
      NamedValues results = item.UserData["Results"] as NamedValues;
      if (results != null)
        return results;
      Exception Exception = item.UserData["Exception"] as Exception;
      if (Exception != null)
        throw Exception;
      throw new BugException("After the executiom there is neither result nor exception object");
    }

    #endregion

    #endregion

    #region Для DistributedProcCallItem

    #region Асинхронное выполнение ExecuteAsync()

    /// <summary>
    /// Вызывает процедуру асинхронно и добавляет текущую процедуру в список, вместе с обработчиком завершения.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="item">Процедура и обработчики</param>
    public virtual void ExecuteAsync(DistributedProcCallItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      try
      {
        item.SetStarted(true);

        ISplashStack splStack = CreateSplashStack();
        if (splStack == null)
          item.AsyncResult = item.DistributedProc.BeginExecute(null, null); // здесь может возникнуть исключение, если процедуру нельзя запускать
        else
        {
          IAsyncResultWithSplash ar2 = item.DistributedProc.BeginExecuteWithSplash(null, null);
          item.AsyncResult = ar2;
          item.AsyncResultHandler = new AsyncResultWithSplashHandler(ar2);
          item.SplashWatcher = new ClientSplashWatcher(item.AsyncResultHandler, splStack);
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.DefaultSplashPhaseText = String.Format(Res.ExecProc_Phase_Executing, item.DisplayName);
        }

        if (item.DistributedProc.StartData.IsCompleted)
        {
          NamedValues res = item.EndExecute(item.AsyncResult);
          OnFinished(item, res);
          item.Dispose();
        }
        else
        {
          lock (_Items)
          {
            _Items.Add(item);
          }
        }
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);

        try
        {
          item.Dispose();
        }
        catch { }

        throw;
      }
    }

    #endregion

    #region ExecuteAsyncAndWait()

    /// <summary>
    /// Используется методом ExecuteAsyncAndWait, принимающим ExecProcCallItem.
    /// Вторая перегрузка, принимаюшая IExecProc, владеет собственным ExecProcCallItem и может использовать его как промежуточное хранилище.
    /// </summary>
    private class DistributedProcExecuteAsyncAndWaitHandler
    {
      #region Поля

      public NamedValues Results;

      public Exception Exception;

      #endregion

      #region Метод

      public void Item_Finished(object sender, DistributedProcCallEventArgs args)
      {
        Results = args.Results;
        Exception = args.Exception;
      }

      #endregion
    }

    /// <summary>
    /// Асинхронный вызов с ожиданием.
    /// В отличие от метода ExecuteAsync(), который сразу возвращает управление,
    /// вызов этого метода не завершается, пока выполнение не будет завершено.
    /// Метод может вызываться в любом потоке.
    /// Метод должен использоваться с удаленной процедурой, которая выполняется долго и может вызвать ошибку тайм-аута при использовании ExecuteSync(),
    /// а использовать ExecuteAsync() нельзя, так как требуется получить результаты в текущем потоке.
    /// Фактически, этот метод вызывает ExecuteAsync() и дожидается завершения процедуры. Затем результаты возвращаются в основном потоке.
    /// Если при выполнении процедуры возникло исключение, то оно перевыбрасывается в основном потоке. Предполагается, что в <paramref name="item"/>
    /// нет своей обработки ошибок.
    /// </summary>
    /// <param name="item">Объект процедуры</param>
    /// <returns>Результаты выполнения процедуры</returns>
    public virtual NamedValues ExecuteAsyncAndWait(DistributedProcCallItem item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      // Создаем копию ExecProcCallItem.
      // У оригинала может быть не установлено свойство UseFinishedWhenFailed, но есть обработчик события Finished. Тогда обработчик события будет
      // вызван при появлении ошибки, к чему обработчик не готов
      DistributedProcCallItem item2 = new DistributedProcCallItem(item.DistributedProc);
      item2.DisplayName = item.DisplayName;

      DistributedProcExecuteAsyncAndWaitHandler tempHandler = new DistributedProcExecuteAsyncAndWaitHandler();
      item2.UseFinishedWhenFailed = true;
      item2.Finished += tempHandler.Item_Finished;
      try
      {
        ExecuteAsync(item2);
        Thread.Sleep(0);

        while (item2.State == ExecProcCallItemState.Executing)
        {
#if DEBUG
          if (item2.DistributedProc.StartData.IsCompleted)
            throw new BugException("The procedure has finished on the server, but it got stuck");
#endif
          Sleep(250);
        }
      }
      finally
      {
        item2.Dispose();
        item.Dispose();
      }

      if (tempHandler.Exception != null)
      {
        tempHandler.Exception.Data["AsyncStackTrace"] = tempHandler.Exception.StackTrace;
        //OnFailed(item, TempHandler.Exception);
        // 19.12.2019
        // Не надо, чтобы вызывался виртуальный метод, который перехватит исключение.
        // Если собственный обработчик ошибок не установлен для ExecProcCallItem, то пусть исключение выбрасывается.
        DoOnFailed(item, tempHandler.Exception);
      }
      else
        OnFinished(item, tempHandler.Results);

      return tempHandler.Results;
    }

    #endregion

    #endregion

    #region Завершение выполнения

    void Item_Finished(object sender, ExecProcCallEventArgs args)
    {
      if (args.Exception == null)
        args.Item.UserData["Results"] = args.Results;
      else
        args.Item.UserData["Exception"] = args.Exception;
    }

    /// <summary>
    /// Этот метод вызывается в случае успешного выполнения процедуры.
    /// Этот метод может быть вызван асинхронно в любом потоке: в потоке запуска процедуры, в основном потоке или в дополнительном.
    /// </summary>
    /// <param name="item">Описатель процедур</param>
    /// <param name="results">Результаты выполнения процедуры</param>
    protected virtual void OnFinished(ExecProcCallItemBase item, NamedValues results)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (results == null)
        throw new ArgumentNullException("results");

      item.State = ExecProcCallItemState.Finished;

      item.OnFinished(results, null);
      item.Dispose();
    }

    /// <summary>
    /// Этот метод вызывается в случае аварийного завершения процедуры.
    /// 1. Устанавливает состояние Failed
    /// 2. Вызывает обработчик OnFinished(), если UseFinishedWhenFailed = true, иначе перевыбрасывает искочение <paramref name="exception"/>.
    /// 3. Вызывает <paramref name="item"/>.Dispose() независимо от способа обработки ошибки.
    /// Метод вызывается в том потоке, который использовался для вызова процедуры.
    /// </summary>
    /// <param name="item">Описатель процедуры, для которай возникло исключение</param>
    /// <param name="exception">Объект исключения</param>
    protected virtual void OnFailed(ExecProcCallItemBase item, Exception exception)
    {
      DoOnFailed(item, exception);
    }

    private static void DoOnFailed(ExecProcCallItemBase item, Exception exception)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (exception == null)
        throw new ArgumentNullException("exception");

#if DEBUG_THREADS
      item.CheckCallerThread();
#endif

      item.State = ExecProcCallItemState.Failed;
      try
      {
        if (item.UseFinishedWhenFailed)
          item.OnFinished(null, exception);
        else
          throw exception;
      }
      finally
      {
        item.Dispose();
      }
    }

    #endregion

    #endregion

    #region Обработка сигнала таймера

    /// <summary>
    /// Индекс текущей процедуры в списке FItems.
    /// Меняется от 0 до _Items.Count-1 по кругу при каждом вызове метода Process()
    /// </summary>
    private int _CurrIndex;

    /// <summary>
    /// Предотвращение вложенных вызовов
    /// </summary>
    private bool _InsideProcess;

    /// <summary>
    /// Этот метод должен вызываться приложением клиента по таймеру.
    /// Проверяет завершение очередной процедуры из списка. За один вызов обрабатывается только одна процедура, чтобы метод мог завершиться быстро.
    /// Если в процессе вызова EndExecute() возникает ислючение, то вызывается метод OnFailed() и тема убирается из списка.
    /// На стороне клиента должен переопределяться метод OnFailed(). Он может перехватить исключение при вызове базового метода ExecProcCallList.OnFailed()
    /// и выдать сообщение об ошибке. Если метод OnFailed() не переопределен, то исключение будет выведено в log-файл.
    /// Этот метод никогда не выбрасывает исключений.
    /// </summary>
    public void Process()
    {
#if DEBUG_THREADS
      CheckMainThread();
#endif

      if (_InsideProcess)
        return;
      _InsideProcess = true;
      try
      {
        ExecProcCallItemBase item = null;
        lock (_Items)
        {
          if (_Items.Count > 0)
          {
            if (_CurrIndex >= _Items.Count)
              _CurrIndex = 0;

            item = _Items[_CurrIndex];

            _CurrIndex++;
          }
        }
        if (item != null)
          ProcessItem(item);
      }
      catch (Exception e)
      {
        OnUnhandledException(e, LogoutTools.GetDefaultTitle());
      }
      _InsideProcess = false;
    }


    ///// <summary>
    ///// Внутренний флажок устанавливается ProcessItem().
    ///// Используется при асинхронном вызове, чтобы проверить, что метод Sleep() выполнял вызов Process(),
    ///// а не застял почему-нибудь.
    ///// О блокировке не стоит беспокоится, не сработает разок, и ладно
    ///// </summary>
    //private bool _ProcessItemCalled; // 20.08.2020

    private void ProcessItem(ExecProcCallItemBase item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif

      if (item.IsDisposed)
        return;

      #region Проверка IsCompleted

      bool isCompleted;

      try
      {
        if (item.AsyncResultHandler == null)
          isCompleted = item.AsyncResult.IsCompleted; // стандартный вызов
        else
        {
          isCompleted = item.AsyncResultHandler.GetIsCompleted();
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.ProcessSplash();
        }
      }
      catch (Exception e)
      {
        try
        {
          item.AddExceptionInfo(e);
          OnFailed(item, e);
        }
        catch (Exception e2)
        {
          OnUnhandledException(e2, "Unhandled exception when called OnFailed() after an error in GetIsCompleted()");
        }
        lock (_Items)
        {
          _Items.Remove(item);
        }
        return;
      }

      #endregion

      if (!isCompleted)
        return;

      // 19.08.2020
      // Сначала убираем элемент из списка, а потом делаем все остальное. А не наоборот.
      // Обработчик события Finished может выполняться произвольно долго. Если не очистить список,
      // то в ExtForms будет опрашиваться ExecProcCallBack, так как приложение считает, что удаленная процедура все 
      // еще выполняется на сервере.

      lock (_Items)
      {
        _Items.Remove(item);
      }

      #region Вызов EndExecute()

      try
      {
        DoEndExecute(item);
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);
        try
        {
          OnFailed(item, e);
        }
        catch (Exception e2)
        {
          OnUnhandledException(e2, "Unhandled exception when called OnFailed() after an error in EndExecute()");
        }
      }

      #endregion

      item.Dispose(); // 05.12.2019
    }

#if XXX
    private void ProcessItem(ExecProcCallItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");

      CheckMainThread();
#endif

      //_ProcessItemCalled = true;

      if (item.IsDisposed)
        return;

    #region Проверка IsCompleted

      bool IsCompleted;

      try
      {
        if (item.AsyncResultHandler == null)
          IsCompleted = item.AsyncResult.IsCompleted; // стандартный вызов
        else
        {
          IsCompleted = item.AsyncResultHandler.GetIsCompleted();
          item.SplashWatcher.UseDefaultSplash = this.UseDefaultSplash;
          item.SplashWatcher.ProcessSplash();
        }
      }
      catch (Exception e)
      {
        lock (_Items)
        {
          _Items.Remove(item);
        }

        item.AddExceptionInfo(e);
        // нельзя вызывать! Можно только из потока item. OnFailed(item, e);
        OnUnhandledException(e, "Неперехваченная ошибка при вызове GetIsCompleted()");
        return;
      }

    #endregion

      if (!IsCompleted)
        return;

      // 19.08.2020
      // Сначала убираем элемент из списка, а потом делаем все остальное. А не наоборот.
      // Обработчик события Finished может выполняться произвольно долго. Если не очистить список,
      // то в ExtForms будет опрашиваться ExecProcCallBack, так как приложение считает, что удаленная процедура все 
      // еще выполняется на сервере.

      lock (_Items)
      {
        _Items.Remove(item);
      }

    #region Вызов EndExecute()

      try
      {
        DoEndExecute(item);    !!!!!
      }
      catch (Exception e)
      {
        item.AddExceptionInfo(e);
        // Нельзя вызывать!  OnFailed(item, e);
        LogoutTools.LogoutException(e, "Неперехваченная ошибка вызова EndExecute() при аварийном завершении вызова EndExecute()");
      }

    #endregion

      item.Dispose(); // 05.12.2019
    }

#endif

    private void DoEndExecute(ExecProcCallItemBase item)
    {
      NamedValues results;
      try
      {
        results = item.EndExecute(item.AsyncResult);
      }
      catch (Exception e)
      {
        OnFailed(item, e);
        return;
      }

      OnFinished(item, results);
    }


    /// <summary>
    /// Этот метод может вызываться приложением клиента, например, при выводе блоков диалога.
    /// Проверяет завершение всех процедур из списка. 
    /// Метод можно вызывать только в основном потоке.
    /// Если в процессе вызова EndExecute() возникает ислючение, то вызывается метод OnFailed() и тема убирается из списка.
    /// На стороне клиента должен переопределяться метод OnFailed(). Он может перехватить исключение при вызове базового метода ExecProcCallList.OnFailed()
    /// и выдать сообщение об ошибке. Если метод OnFailed() не переопределен, то исключение будет выведено в log-файл.
    /// Этот метод никогда не выбрасывает исключений в основном потоке.
    /// </summary>
    public void ProcessAll()
    {
#if DEBUG_THREADS
      CheckMainThread();
#endif

      if (_InsideProcess)
        return;
      _InsideProcess = true;
      try
      {
        ExecProcCallItemBase[] a;
        lock (_Items)
        {
          a = _Items.ToArray(); // отдельная копия, так как элементы могут убираться из списка в процессе перебора
        }

        for (int i = 0; i < a.Length; i++)
          ProcessItem(a[i]);
      }
      catch (Exception e)
      {
        OnUnhandledException(e, LogoutTools.GetDefaultTitle());
      }
      _InsideProcess = false;
    }


    #endregion

    #region Управление видимостью заставок

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то для запущенных процессов будет выводиться заставка по умолчанию
    /// </summary>
    public bool UseDefaultSplash
    {
      get { return _UseDefaultSplash; }
      set
      {
        if (value == _UseDefaultSplash)
          return;

        _UseDefaultSplash = value;
      }
    }
    private bool _UseDefaultSplash;

    #endregion

    #region Обработка ошибок

    /// <summary>
    /// Метод вызывается при возникновении ошибки, которую нельзя перехватить.
    /// Этот метод может вызываться из любого потока, основного или рабочего.
    /// Непереопределенный метод вызывает LogoutTools.LogoutException().
    /// Переопределенный метод может определить, яляется ли поток подходящим, и выдать, например, сообщение об ошибке.
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <param name="title">Заголовок</param>
    protected virtual void OnUnhandledException(Exception e, string title)
    {
      LogoutTools.LogoutException(e, title);
    }

    #endregion
  }
}
