// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Logging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization;
using FreeLibSet.Core;
using FreeLibSet.Collections;

/*
 * Система вызова процедур с контекстами выполнения
 * 
 * Процедуры создаются как классы, производные от ExecProc. В них должен быть переопределен абстрактный метод 
 * OnExecute(), который выполняет содержательную часть работы
 * После создания экземпляра ExecProc, к нему присоединяется контекст ExecProcContext
 * После этого можно вызывать Execute() или BeginExecute()
 * 
 * Вызов процедур сервера
 * ----------------------
 * В системе клиент-сервер процедуры на стороне сервера должны запускаться асинхронно и через удаленный доступ
 * Так как класс ExecProc нельзя передавать за пределы домена, используется marshal-by-reference 
 * переходник ExecProcProxy, а на стороне клиента создается RemoteExecProc.
 * 
 * Возможные схемы вызова
 * ----------------------
 * 1. Простой вызов ExecProc
 * 2. Удаленный доступ: RemoteExecProc -> ExecProcProxy -> ExecProc
 * 3. Удаленный доступ с промежуточным доступом. Используется, когда технология Remoting не разрешает
 *    прямое использование клиентом объектов в домене с процедурой. Используется, если сервер загружает
 *    процедуру не в основной домен приложения, а в дополнительный домен. Также может применяться в
 *    многозвенной архитектуре:
 *    RemoteExecProc -> ExecProcProxy2[ -> ExecProcProxy2, ...] -> ExecProcProxy -> ExecProc
 * 
 * Синхронность вызовов
 * --------------------
 * Поддерживаются вызовы:
 * 1. Синхронный (с ожиданием завершения)
 * 2. Асинхронный (с периодическим опросом, вызовом процедуры обратного вызова или использованием WaitHandle),
 *    с использованием семантики IAsyncResult
 * 3. Асинхронный без ожидания завершения
 */

namespace FreeLibSet.Remoting
{
  #region Перечисление ExecProcState

  /// <summary>
  /// Состояние выполняемой процедуры
  /// </summary>
  [Serializable]
  public enum ExecProcState
  {
    /// <summary>
    /// Процедура еще не запускалась
    /// </summary>
    NotStarted,

    /// <summary>
    /// Процедура поставлена в очередь на выполнение
    /// </summary>
    Queued,

    /// <summary>
    /// Процедура в данный момент выполняется
    /// </summary>
    Executing,

    /// <summary>
    /// Выполнение процедуры завершилось успешно
    /// </summary>
    Success,

    /// <summary>
    /// Выполнение процедуры завершилось с ошибкой
    /// </summary>
    Error,

    /// <summary>
    /// Был вызван метод Dispose()
    /// </summary>
    Disposed
  }

  #endregion

  #region Интерфейс IExecProc

  /// <summary>
  /// Интерфейс вызова процедуры, реализуемый классами ExecProc и RemoteExecProc
  /// </summary>
  public interface IExecProc : IDisposable
  {
    /// <summary>
    /// Возвращает уникальный идентификатор ExecProc
    /// Если интерфейс реализован не через ExecProc, метод должен возвращать Guid какого-либо другого объекта
    /// </summary>
    Guid Guid { get; }

    /// <summary>
    /// Отображаемое имя.
    /// Если не установлено, возвращает пустую строку
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Если true (по умолчанию), то процедура ExecProc является "саморазрушаемой". 
    /// После вызова Execute() или EndExecute() выполняется вызов ExecProc.Dispose().
    /// Свойство можно устанавливать в любое время.
    /// </summary>
    bool AutoDispose { get; set; }

    /// <summary>
    /// Интервал времени в миллисекундах, в течение которого вызов BeginExecute() будет ожидать завершения потока.
    /// Нулевое значение означает отсутствие ожидания.
    /// По умолчанию свойство принимает значение ExecProc.DefaultSyncTime
    /// </summary>
    int SyncTime { get; set; }

    /// <summary>
    /// Текущее состояние выполнения процедуры
    /// </summary>
    ExecProcState State { get; }

    // Можно было бы объединить получение свойств IsExecuting, Results и Exception в один метод
    // Это сократило бы накладные расходы при получении результатов в системе клиент-сервер, т.к. был бы
    // один вызов вместо двух
    // Но для мониторинга выгоднее получать эти свойства по отдельности

    /// <summary>
    /// Возвращает информацию об объекте ExecProc в сериализуемом формате
    /// Если интерфейс реализован не через ExecProc, метод может возвращать null
    /// </summary>
    /// <returns></returns>
    ExecProcInfo GetInfo();

    /// <summary>
    /// Синхронный вызов процедуры. 
    /// Если на момент вызова IsExecuting имеет значение true, генерируется исключение AlreadyExecutingException.
    /// Если в процессе выполнения возникает исключение, оно не перехватывается.
    /// Если выполнение прерывается, то генерируется UserCancelException
    /// </summary>
    /// <param name="args">Аргументы, передаваемые в основную процедуру OnExecute()</param>
    /// <returns>Результаты, возвращаемые OnExecute()</returns>
    NamedValues Execute(NamedValues args);

    /// <summary>
    /// Начать асинхронное выполнение процедуры
    /// </summary>
    /// <param name="args">Аргументы, передаваемые в основную процедуру OnExecute()</param>
    /// <param name="userCallback"></param>
    /// <param name="stateObject"></param>
    /// <returns></returns>
    IAsyncResult BeginExecute(NamedValues args, AsyncCallback userCallback, object stateObject);

    /// <summary>
    /// Начать асинхронное выполнение процедуры, используя SplashWatcher
    /// </summary>
    /// <param name="args">Аргументы, передаваемые в основную процедуру OnExecute()</param>
    /// <param name="userCallback"></param>
    /// <param name="stateObject"></param>
    /// <returns></returns>
    IAsyncResultWithSplash BeginExecuteWithSplash(NamedValues args, AsyncCallback userCallback, object stateObject);

    /// <summary>
    /// Закончить асинхронное выполнение процедуры.
    /// </summary>
    /// <param name="asyncResult">Интерфейс, полученный от BeginExecute()</param>
    /// <returns>Результаты, возвращенные OnExecute()</returns>
    NamedValues EndExecute(IAsyncResult asyncResult);

    /// <summary>
    /// Асинхронный вызов процедуры
    /// Если на момент вызова IsExecuting имеет значение true, генерируется исключение AlreadyRunningException.
    /// После старта нового потока управление возвращается.
    /// Этот метод используется, когда получать результат выполнения не требуется.
    /// Также невозможно узнать об ошибках, возникших в процессе выполнения.
    /// По окончании выполнения объект разрушается. Требуется, чтобы свойство AutoDispose было установлено в true
    /// </summary>
    /// <param name="args">Аргументы, передаваемые в основную процедуру OnExecute()</param>
    void ExecuteNoResults(NamedValues args);

    /// <summary>
    /// Создает объект для отслеживания стека заставок.
    /// Этот метод не должен использоваться в прикладном коде
    /// </summary>
    /// <returns>Ссылка на новый объект ServerSplashWatcher</returns>
    IServerSplashWatcher CreateSplashWatcher();
  }

  #endregion

  #region Интерфейс IExecProcTracer

  /// <summary>
  /// Интерфейс трассировщика процедур
  /// </summary>
  public interface IExecProcTracer
  {
    /// <summary>
    /// Вызывается перед выполнением процедуры
    /// </summary>
    /// <param name="caller">Процедура, которая будет выполняться</param>
    /// <param name="args">Аргументы, переданные процедуре</param>
    void TraceBeforeExecute(ExecProc caller, NamedValues args);

    /// <summary>
    /// Вызывается после выполнения процедуры (успешного или аварийного завершения
    /// </summary>
    /// <param name="caller">Выполненная процедура</param>
    /// <param name="args">Аргументы, переданные процедуре перед вызовом</param>
    /// <param name="results">Значения, возвращаемые процедурой. Если выполнение завершилось аварийно, то null</param>
    /// <param name="exception">Объект ошибки, из-за которой процедура аварийно завершена.
    /// При нормальном завершении - null</param>
    void TraceAfterExecute(ExecProc caller, NamedValues args, NamedValues results, Exception exception);
  }


  #endregion

  /// <summary>
  /// Базовый класс для реализации удаленно выполняемых процедур.
  /// Порядок синхронного вызова.
  /// 1. Создается экземпляр класса, производного от ExecProc.
  /// 2. К нему (однократно) присоединяется контекст выполнения
  /// 3. Вызывается метод Execute()
  /// Этот класс нельзя передавать между доменами ни по ссылке, ни по значению. 
  /// Для удаленных вызовов следует использовать переходник RemoteExecProc
  /// </summary>
  public abstract class ExecProc : SimpleDisposableObject, IExecProc
  {
    // 02.01.2021
    // Можно использовать SimpleDisposableObject в качестве базового класса.
    // Единственное действие, выполняемое в деструкторе (в Dispose(false)) - удаление из списка AllProcs.
    // Но этот список хранит слабые ссылки, которые самоочищаются, если процедура разрушена

    #region Конструктор и Dispose

    /// <summary>
    /// Создает новую процедуру в состоянии NotStarted
    /// </summary>
    public ExecProc()
    {
      _SyncRoot = new object();

      _Guid = Guid.NewGuid();
      _CreationTime = DateTime.Now;
      _DisplayName = String.Empty;
      _State = ExecProcState.NotStarted;
      _LogoutErrors = DefaultLogoutErrors;
      // 21.06.2016 - Свойство "ambient"-типа 
      // _TraceEnabled = TraceSwitch.Enabled;

      _ActionName = String.Empty;
      _AutoDispose = true;
      _SyncTime = DefaultSyncTime;

      _AllProcs.Add(this);

      _SplashStack = new MemorySplashStack();
    }

    /// <summary>
    /// Инициализация вывода отладочной информации
    /// </summary>
    static ExecProc()
    {
      LogoutTools.LogoutInfoNeeded += new LogoutInfoNeededEventHandler(LogoutTools_LogoutInfoNeeded);
      LogoutTools.LogoutProp += new LogoutPropEventHandler(LogoutTools_LogoutProp);
    }

    /// <summary>
    /// Разрушение объекта ExecProc
    /// </summary>
    /// <param name="disposing">true, если вызван ExecProc.Dispose(), false - если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // 12.02.2017
        // Прекращаем выполнение процедуры
        try
        {
          Thread th = this.ExecThread; // Обязательно так. Свойство ExecThread может меняться динамически
          if (th != null && th != Thread.CurrentThread)
            th.Abort();
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка прекращения выполнения процедуры при удалении");
        }

        if (_Proxy != null)
        {
          ((ExecProcProxy1.InternalHandler1)(_Proxy.Handler)).ExecProc = null; // чтобы ExecProc не "завис" в памяти
          _Proxy = null;
        }
      }
      _State = ExecProcState.Disposed;
      base.Dispose(disposing);
      _AllProcs.Remove(this);
    }

    #endregion

    #region Информация о создании процедуры

    /// <summary>
    /// Уникальный идентификатор процедуры.
    /// Устанавливается в конструкторе
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private Guid _Guid;

    /// <summary>
    /// Время, когда объект был создан (для отладочных целей)
    /// </summary>
    public DateTime CreationTime { get { return _CreationTime; } }
    private DateTime _CreationTime;

    /// <summary>
    /// Время существования процедуры (для отладки)
    /// </summary>
    public TimeSpan LifeTime { get { return DateTime.Now - _CreationTime; } }

    /// <summary>
    /// Отображаемое имя.
    /// Если не установлено в явном виде, возвращает пустую строку
    /// </summary>
    public string DisplayName
    {
      get
      {
        lock (_SyncRoot)
        {
          return _DisplayName;
        }
      }
      set
      {
        lock (_SyncRoot)
        {
          if (value == null)
            _DisplayName = String.Empty;
          else
            _DisplayName = value;
        }
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Выполняемое действие.
    /// Если не установлено в явном виде, возвращает пустую строку.
    /// В отличие от DisplayName, это свойство очищается перед каждым запуском процедуры.
    /// </summary>
    public string ActionName
    {
      get
      {
        lock (_SyncRoot)
        {
          return _ActionName;
        }
      }
      protected set
      {
        lock (_SyncRoot)
        {
          if (value == null)
            _ActionName = String.Empty;
          else
            _ActionName = value;
        }
      }
    }
    private string _ActionName;

    /// <summary>
    /// Текстовое представление в виде "DisplayName (State)" или "GUID (State)"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string s = DisplayName;
      if (String.IsNullOrEmpty(s))
        s = Guid.ToString();
      string s2 = ActionName;
      if (!String.IsNullOrEmpty(s2))
        s += " - " + s2;
      s += " (" + State.ToString() + ")";
      return s;
    }

    /// <summary>
    /// Если true (по умолчанию), то процедура ExecProc является "саморазрушаемой". 
    /// После вызова Execute() или EndExecute() выполняется вызов ExecProc.Dispose().
    /// Свойство можно устанавливать в любое время.
    /// </summary>
    public bool AutoDispose
    {
      get { return _AutoDispose; }
      set { _AutoDispose = value; }
    }
    private bool _AutoDispose;

    /// <summary>
    /// Интервал времени в миллисекундах, в течение которого вызов BeginExecute() будет ожидать завершения потока.
    /// Нулевое значение означает отсутствие ожидания.
    /// По умолчанию свойство принимает значение DefaultSyncTime
    /// </summary>
    public int SyncTime
    {
      get { return _SyncTime; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _SyncTime = value;
      }
    }
    private int _SyncTime;

    /// <summary>
    /// Интервал времени по умолчанию в миллисекундах, в течение которого вызов BeginExecute() будет ожидать завершения потока.
    /// Нулевое значение означает отсутствие ожидания.
    /// Значение по умолчанию - 500 мс.
    /// </summary>
    public static int DefaultSyncTime
    {
      get { return _DefaultSyncTime; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _DefaultSyncTime = value;
      }
    }
    private static int _DefaultSyncTime = 500;

    #endregion

    #region Контекст выполнения

    /// <summary>
    /// Контекст выполнения процедуры
    /// Свойство должно быть установлено перед выполнением процедуры с помощью вызова SetContext()
    /// </summary>
    protected NamedValues Context { get { return _Context; } }
    private NamedValues _Context;

    /// <summary>
    /// Присоединение контекста выполнения.
    /// Метод должен вызываться однократно перед вызовом Execute()
    /// Коллекция элементов контекста переводится в режим "Только для чтения"
    /// </summary>
    /// <param name="context">Устанавливаемый контекст. Не может быть null.</param>
    public virtual void SetContext(NamedValues context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      context.SetReadOnly();

      lock (_SyncRoot)
      {
        if (_Context != null)
          throw new InvalidOperationException("Повторный вызов SetContext()");

        // Проверять IsExecuting не надо

        _Context = context;
      }
    }

    /// <summary>
    /// Свойство возвращает true, если контекст выполнения был установлен
    /// </summary>
    public bool HasContext
    {
      get
      {
        lock (_SyncRoot)
        {
          return _Context != null;
        }
      }
    }

    #endregion

    #region Логгинг ошибок

    /// <summary>
    /// Если свойство установлено в true, то, если возникает исключение при выполнении процедуры,
    /// выполняется LogoutTools.LogoutException() перед выдачей исключения наружу.
    /// Может использоваться в отладочных целях в системе клиент-сервер на стороне сервера
    /// При создании объекта, свойство получает значение DefaultLogoutErrors
    /// </summary>
    public bool LogoutErrors
    {
      get { return _LogoutErrors; }
      set { _LogoutErrors = value; }
    }
    private bool _LogoutErrors;

    /// <summary>
    /// Значение по умолчанию, которое присваивается LogoutErrors.
    /// По умолчанию - false
    /// </summary>
    public static bool DefaultLogoutErrors
    {
      get { return _DefaultLogoutErrors; }
      set { _DefaultLogoutErrors = value; }
    }
    private static bool _DefaultLogoutErrors = false;

    #endregion

    #region Объект синхронизации

    /// <summary>
    /// Объект синхронизации для ExecProc
    /// </summary>
    internal object SyncRoot { get { return _SyncRoot; } }
    private object _SyncRoot;

    #endregion

    #region Выполнение

    #region Общая часть

    /// <summary>
    /// Текущее состояние процедуры
    /// </summary>
    public ExecProcState State { get { return _State; } }
    private ExecProcState _State;

    /// <summary>
    /// Возвращает true, если текущее состояние позволяет запуск процедуры
    /// </summary>
    public bool CanStart { get { return ExecProc.GetCanStart(State); } }

    /// <summary>
    /// Возвращает true для State=Queued и Execiting
    /// </summary>
    public bool IsExecuting { get { return ExecProc.GetIsExecuting(State); } }

    /// <summary>
    /// Возвращает true для состояний NotStarted, Success и Error
    /// </summary>
    /// <param name="state">Текущее состояние</param>
    /// <returns>true, если можно запустить процедуру на выполнение</returns>
    public static bool GetCanStart(ExecProcState state)
    {
      switch (state)
      {
        case ExecProcState.NotStarted:
        case ExecProcState.Success:
        case ExecProcState.Error:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Возвращает true для состояний Queued и Executing
    /// </summary>
    /// <param name="state">Текущее состояние</param>
    /// <returns>true, если процедура выполняется или поставлена в очередь</returns>
    public static bool GetIsExecuting(ExecProcState state)
    {
      switch (state)
      {
        case ExecProcState.Queued:
        case ExecProcState.Executing:
          return true;
        default:
          return false;
      }
    }

    [DebuggerStepThrough]
    private NamedValues DoExecute2(NamedValues args)
    {
#if DEBUG
      if (args == null)
        throw new ArgumentNullException("args");
      if (_State != ExecProcState.Queued)
        throw new BugException("State=" + _State.ToString());
#endif

      NamedValues Res;

      lock (SyncRoot)
      {
        if (_ExecThread != null)
          throw new AlreadyExecutingException(this);
        _ExecThread = Thread.CurrentThread;
        _State = ExecProcState.Executing; // переставлено местами 26.01.2021

        _ParentProc = _CurrentProc;
        _CurrentProc = this;

        SplashTools.PushThreadSplashStack(this.SplashStack);
      }
      _ExecutingProcs.Add(this);
      try
      {
        OnBeforeExecute(args);
        try
        {
          Res = OnExecute(args);
          _State = ExecProcState.Success;
        }
        catch (Exception e)
        {
          e.Data["ExecProcStackTrace"] = e.StackTrace;
          _State = ExecProcState.Error;
          OnAfterExecute(args, null, e);
          throw;
        }
        OnAfterExecute(args, Res, null);
      }
      finally
      {
        _ExecutingProcs.Remove(this);

        lock (SyncRoot)
        {
          if (IsDisposed)
            _State = ExecProcState.Disposed; // был вызов во время выполнения

          SplashTools.PopThreadSplashStack();
          _ExecThread = null;
          _CurrentProc = _ParentProc;
          _ParentProc = null;
        }
      }

      if (Res == null)
        Res = NamedValues.Empty;
      else
        Res.SetReadOnly();

      return Res;
    }

    private void DoLogoutError(Exception e)
    {
      if (!LogoutErrors)
        return;
      if (LogoutTools.GetException<UserCancelException>(e) != null)
        return; // 12.12.2019
      if (LogoutTools.GetException<ThreadAbortException>(e) != null)
        return; // 16.02.2021

      try
      {
        e.Data["ExecProcInfo"] = this.GetInfo(); // 16.02.2021
      }
      catch { }

      LogoutTools.LogoutException(e, "Ошибка выполнения процедуры");
    }

    /// <summary>
    /// Если при выполнении процедуры возникла ошибка, то содержит объект исключения.
    /// Если выполнение закончилось успешно, то содержит null.
    /// На время обращения используется блокировка SyncRoot
    /// </summary>
    private Exception _ErrorDebugException;

    /// <summary>
    /// Если при выполнении процедуры возникла ошибка, то содержит "упрощенные" аргументы запуска.
    /// Если выполнение закончилось успешно, то содержит null.
    /// На время обращения используется блокировка SyncRoot
    /// </summary>
    private NamedValues _ErrorDebugArgs;

    /// <summary>
    /// Создает "упрощенные" аргументы
    /// </summary>
    /// <param name="args">Исходные аргументы, с которыми вызвана процедура</param>
    /// <returns></returns>
    private static NamedValues CreateDebugArgs(NamedValues args)
    {
      if (args == null)
        return null;
      if (args.IsEmpty)
        return args;

      NamedValues Args2 = new NamedValues();
      foreach (KeyValuePair<string, object> Pair in args)
      {
        // Надо перехватывать ошибки, т.к. среди аргументов могут быть "просроченные" MarshalByReference-объекты
        try
        {
          object v = CreateDebugValue(Pair.Value);
          Args2[Pair.Key] = v;
        }
        catch (Exception e)
        {
          Args2[Pair.Key] = "*** Ошибка получения аргумента ***. " + e.Message;
        }
      }
      Args2.SetReadOnly();
      return Args2;
    }

    private static object CreateDebugValue(object v)
    {
      if (v == null)
        return null;
      if (v.GetType().IsPrimitive)
        return v;
      if ((v is DBNull) || (v is DateTime) || (v is TimeSpan) || (v is String) || (v is Decimal))
        return v;

      return v.ToString();
    }


    private void CheckIsFromExecute()
    {
      if (_State != ExecProcState.Executing)
        throw new InvalidCastException("Вызов допускается только при работе процедуры. State=" + State.ToString());

      if (Thread.CurrentThread != _ExecThread)
        throw new DifferentThreadException(_ExecThread);
    }

    #endregion

    #region Синхронное

    /// <summary>
    /// Выполнение процедуры в текущем потоке
    /// </summary>
    /// <param name="args">Аргументы, передаваемые в OnExecute().
    /// Если аргументы не требуются, используйте NamedValues.Empty</param>
    /// <returns></returns>
    public NamedValues Execute(NamedValues args)
    {
      CheckNotDisposed();

      if (args == null)
        throw new ArgumentException("args");
      args.SetReadOnly();

      if (_Context == null)
        throw new NullReferenceException("Не установлен контекст выполнения (свойство Context)");

      NamedValues Res = null;
      Exception ex2 = null;

      if (_CurrentProc != null)
      {
        lock (_CurrentProc.SyncRoot)
        {
          if (_CurrentProc._ChildProc != null)
            throw new BugException("Свойство CurrentProc.ChildProc уже установлено");
          _CurrentProc._ChildProc = this;
        }
      }

      try
      {
        lock (_SyncRoot)
        {
          _IsAsyncStarted = false;
          _StartCount++;
          _StartTime = DateTime.Now;
          _State = ExecProcState.Queued;
          _ActionName = String.Empty;
          _ErrorDebugException = null;
          _ErrorDebugArgs = null;
        }

        try
        {
          try
          {
            Res = DoExecute2(args);
          }
          catch (Exception e)
          {
            DoLogoutError(e);

            ex2 = e;
            throw;
          }
        }
        finally
        {
          lock (_SyncRoot)
          {
            _ErrorDebugException = ex2;
            if (ex2 != null)
              _ErrorDebugArgs = CreateDebugArgs(args);
          }
        }
      }
      finally
      {
        if (_CurrentProc != null)
        {
          lock (_CurrentProc.SyncRoot)
          {
            _CurrentProc._ChildProc = null;
          }
        }

        if (AutoDispose)
          Dispose();
      }

      return Res;
    }

    #endregion

    #region Асинхронное

    /// <summary>
    /// Реализация IAsyncResult для ExecProc
    /// </summary>
    private class ExecProcAsyncResult : IAsyncResultWithSplash
    {
      #region Поля

      /// <summary>
      /// Объект-владелец
      /// </summary>
      public ExecProc Owner;

      /// <summary>
      /// Аргументы, передаваемые в OnExecute()
      /// </summary>
      public NamedValues Args;

      /// <summary>
      /// Результаты, возвращаемые OnExecute() в случае успешного завершения
      /// </summary>
      public NamedValues Results;

      /// <summary>
      /// Исключение, возникшее при вызове OnExecute()
      /// </summary>
      public Exception Exception;

      /// <summary>
      /// Свойство устанавливается в true после вызова EndExecute()
      /// </summary>
      public bool EndExecuteCalled;

      public AsyncCallback UserCallback;

      /// <summary>
      /// Если был вызван метод ExecProc.BeginExecuteWithSplash(), то содержит серверную часть наблюдения за заставкой
      /// </summary>
      public ServerSplashWatcher SplashWatcher;

      #endregion

      #region IAsyncResult Members

      /// <summary>
      /// Пользовательские данные, переданные BeginExecute()
      /// </summary>
      public object AsyncState { get { return _AsyncState; } internal set { _AsyncState = value; } }
      private object _AsyncState;

      public WaitHandle AsyncWaitHandle
      {
        get
        {
          // Проблема.
          // В процессе получения этого свойства процедура может как раз закончить выполнение
          // и вызвать свой метод Dispose().
          // Поэтому, объект ManualResetEvent, который является Disposable, должен хранится здесь,
          // а не в ExecProc.
          // Предполагается, что EndExecute() будет когда-нибудь вызван. Он и выполнит WaitHandle.Dispose()

          lock (Owner.SyncRoot)
          {
            if (AsyncWaitHandleField == null)
            {
              if (!EndExecuteCalled)
                AsyncWaitHandleField = new ManualResetEvent(_IsCompleted);
            }
            return AsyncWaitHandleField;
          }
        }
      }
      /// <summary>
      /// На время обращения к полю должна выполняться блокировка ExecProc.SyncRoot
      /// (изменено 29.08.2020. Раньше блокировался объект ExecProcAsyncResult, то есть this)
      /// </summary>
      internal ManualResetEvent AsyncWaitHandleField;

      public bool CompletedSynchronously
      {
        get { return _CompletedSynchronously; }
        internal set { _CompletedSynchronously = value; }
      }
      private bool _CompletedSynchronously;

      public bool IsCompleted { get { return _IsCompleted; } }
      /// <summary>
      /// Это поле устанавливается асинхронно
      /// </summary>
      private bool _IsCompleted;

      #endregion

      #region IAsyncResultWithSplash Members

      public bool GetIsCompleted(out SplashInfoPack splashInfoPack)
      {
        if (SplashWatcher == null)
          throw new InvalidOperationException("Использовался вызов ExecProc.BeginExecute(), а не BeginExecuteWithSplash()");

        splashInfoPack = SplashWatcher.GetSplashInfoPack();
        return _IsCompleted;
      }

      void IAsyncResultWithSplash.Cancel()
      {
        SplashWatcher.Cancel();
      }

      void IAsyncResultWithSplash.ResetSplashInfo()
      {
        SplashWatcher.ResetSplashInfo();
      }

      #endregion

      #region Окончание завершения

      /// <summary>
      /// Сигнал завершения работы
      /// Этот метод вызывается из того потока, где выполнялась процедура.
      /// </summary>
      internal void Finish()
      {
        ManualResetEvent AsyncWaitHandle2;
        lock (Owner.SyncRoot)
        {
          _IsCompleted = true;
          // должно быть установлено до обращения к FAsyncWaitHandle,
          // т.к. объект WaintHandle может между lock и вызовом Reset
          AsyncWaitHandle2 = AsyncWaitHandleField;
        }

        // Это нельзя делать внутри блокировки
        if (AsyncWaitHandle2 != null)
          AsyncWaitHandle2.Set();

        if (UserCallback != null)
          UserCallback(this);
      }

      #endregion
    }

    /// <summary>
    /// Запуск на асинхронное выполнение
    /// </summary>
    /// <param name="args"></param>
    /// <param name="userCallback">Пользовательский метод, который получит уведомление при окончании завершения процедуры.
    /// Если null, то предполагается периодический опрос или использование WaitHandle</param>
    /// <param name="stateObject">Произвольные пользовательские данные</param>
    public IAsyncResult BeginExecute(NamedValues args, AsyncCallback userCallback, Object stateObject)
    {
      return DoBeginExecute(args, userCallback, stateObject, false);
    }

    /// <summary>
    /// Запуск на асинхронное выполнение с использованием SplashWatcher
    /// </summary>
    /// <param name="args"></param>
    /// <param name="userCallback">Пользовательский метод, который получит уведомление при окончании завершения процедуры.
    /// Если null, то предполагается периодический опрос или использование WaitHandle</param>
    /// <param name="stateObject">Произвольные пользовательские данные</param>
    public IAsyncResultWithSplash BeginExecuteWithSplash(NamedValues args, AsyncCallback userCallback, Object stateObject)
    {
      return DoBeginExecute(args, userCallback, stateObject, true);
    }

    private ExecProcAsyncResult DoBeginExecute(NamedValues args, AsyncCallback userCallback, Object stateObject, bool useSplashWatcher)
    {
      CheckNotDisposed();

      if (args == null)
        throw new ArgumentException("args");
      args.SetReadOnly();

      if (_Context == null)
        throw new NullReferenceException("Не установлен контекст выполнения (свойство Context)");

      ExecProcAsyncResult ar = new ExecProcAsyncResult();
      ar.Owner = this;
      ar.Args = args;
      ar.AsyncState = stateObject;
      ar.UserCallback = userCallback;
      if (useSplashWatcher)
        ar.SplashWatcher = new ServerSplashWatcher(this.SplashStack);

      lock (_SyncRoot)
      {
        _IsAsyncStarted = true;
        _StartCount++;
        _StartTime = DateTime.Now;
        _State = ExecProcState.Queued;
        _ActionName = String.Empty;
      }
      //FExecThread.Start(ar);
      ThreadPool.QueueUserWorkItem(DoExecuteAsync, ar);

      if (SyncTime > 0)
      {
        ar.AsyncWaitHandle.WaitOne(SyncTime, false /* Перегрузки без аргумента exitContext нет в NetFramework 2 без SP */);
        if (ar.IsCompleted)
          ar.CompletedSynchronously = true;
      }

      return ar;
    }

    private void DoExecuteAsync(object startValue)
    {
      ExecProcAsyncResult ar = (ExecProcAsyncResult)startValue;
      NamedValues Args = ar.Args;
      ar.Args = null; // освобождаем память

      try
      {
        ar.Results = DoExecute2(Args);
      }
      catch (Exception e)
      {
        DoLogoutError(e);
        ar.Exception = e;
        //throw;
        // Исключение проглатывается, т.к. мы не в вызывающем потоке
      }

      lock (_SyncRoot)
      {
        _ErrorDebugException = ar.Exception;
        if (_ErrorDebugException != null)
          _ErrorDebugArgs = CreateDebugArgs(Args);
      }

      ar.Finish();

      if (AutoDispose)
        Dispose();
    }

    /// <summary>
    /// Завершение асинхронного выполнения, начатого BeginExecute().
    /// Если в процессе выполнения произошла ошибка, то исключение будет выброшено сейчас.
    /// </summary>
    /// <param name="asyncResult">Получено от BeginExecute()</param>
    /// <returns>Результаты успешнного выполнения</returns>
    public NamedValues EndExecute(IAsyncResult asyncResult)
    {
      ExecProcAsyncResult ar = (ExecProcAsyncResult)asyncResult;
      if (ar == null)
        throw new ArgumentNullException("asyncResult");
      if (!Object.ReferenceEquals(ar.Owner, this))
        throw new ArgumentException("Объект AsyncResult относится к другой процедуре", "asyncResult");

      if (ar.EndExecuteCalled)
        throw new InvalidOperationException("Повторный вызов EndExecute()");

      if (!ar.IsCompleted)
      {
        ar.AsyncWaitHandle.WaitOne(); // 02.05.2017
      }

      lock (SyncRoot)
      {
        ar.EndExecuteCalled = true;
        if (ar.AsyncWaitHandleField != null)
        {
          ((IDisposable)(ar.AsyncWaitHandleField)).Dispose();
          ar.AsyncWaitHandleField = null;
        }
      }

      //if (AutoDispose)
      //  Dispose();

      if (ar.Exception != null)
      {
        ar.Exception.Data["OriginalStackTrace"] = ar.Exception.StackTrace; // 03.05.2018
        throw ar.Exception;
      }
      else
        return ar.Results;
    }

    #endregion

    #region Без получения результатов

    /// <summary>
    /// Асинхронное выполнение, когда не требуется получить результаты выполнения.
    /// Если в процессе выполнения возникает ошибка, то исключение проглатывается.
    /// </summary>
    /// <param name="args">Аргументы вызова процедуры</param>
    public void ExecuteNoResults(NamedValues args)
    {
      if (!AutoDispose)
        throw new InvalidOperationException("Свойство AutoDispose должно быть установлено в true");
      SyncTime = 0; // нечего ждать.

      BeginExecute(args, new AsyncCallback(ExecuteNoResultsFinished), null);
    }

    private void ExecuteNoResultsFinished(IAsyncResult ar)
    {
      try
      {
        EndExecute(ar);
      }
      catch
      {
        // Проглатываем исключение
      }

      // Это точно избыточный вызов.
      // Нужен, если только свойство AutoDispose было зачем-то сброшено в false в процессе выполнения процедуры
      Dispose();
    }

    #endregion

    #endregion

    #region Информация о выполнении процедуры

    /// <summary>
    /// Текущий экземпляр выполняемой процедуры в текущем потоке
    /// Это свойство может быть вызвано во время выполнения метода Execute(), когда
    /// сложно получить ссылку на объект, откуда выполняется вызов.
    /// При попытке вызвать свойство из потока, в котором нет выполняющейся процедуры,
    /// возвращается null.
    /// </summary>
    public static ExecProc CurrentProc { get { return _CurrentProc; } }
    [ThreadStatic]
    private static ExecProc _CurrentProc;


    /// <summary>
    /// Поток, в котором происходит выполнение процедура.
    /// Если процедура в данный момент не выполняется, возвращается null.
    /// </summary>
    /// <remarks>
    /// Значение null может возвращаться даже в состоянии Executing, если (когда-нибудь будет) реализовано
    /// асинхронное выполнение с переключением между потоками
    /// </remarks>
    public Thread ExecThread { get { return _ExecThread; } }
    private Thread _ExecThread;

    /// <summary>
    /// Родительская процедура, которая запустила данную процедуру.
    /// Свойство устанавливается равным CurrentProc при вызове Execute() или ExecuteAsync().
    /// После завершения выполнения свойство сбрасывается в null (во избежание удержания объектов в памяти)
    /// </summary>
    public ExecProc ParentProc
    {
      get
      {
        //lock (FSyncRoot)
        //{
        return _ParentProc;
        //}
      }
    }
    private ExecProc _ParentProc;

    /// <summary>
    /// Дочерняя процедура, которая была запущена синхронно из текущей процедуры, завершение которой сейчас
    /// ожидается.
    /// Свойство устанавливается для CurrentProc при вызове Execute().
    /// Вызов ExecuteAsync() не устанавливает свойство, т.к. нет состояния ожидания
    /// После завершения выполнения свойство сбрасывается в null (во избежание удержания объектов в памяти)
    /// </summary>
    public ExecProc ChildProc
    {
      get
      {
        //lock (FSyncRoot)
        //{
        return _ChildProc;
        //}
      }
    }
    private ExecProc _ChildProc;

    /// <summary>
    /// Свойство возвращает true, если последний запуск процедуры был в асинхронном режиме, 
    /// false - в синхронном
    /// </summary>
    public bool IsAsyncStarted
    {
      get
      {
        //lock (FSyncRoot)
        //{
        return _IsAsyncStarted;
        //}
      }
    }
    private bool _IsAsyncStarted;

    /// <summary>
    /// Счетчик запуска процедуры на выполнение.
    /// Увеличивается на 1 при каждом вызове Execute() и ExecuteAsync().
    /// Если процедура еще не запускалась, содержит 0
    /// </summary>
    public int StartCount
    {
      get
      {
        lock (_SyncRoot)
        {
          return _StartCount;
        }
      }
    }
    private int _StartCount;

    /// <summary>
    /// Время последнего запуска процедуры.
    /// Если StartCount=0 возвращаемое значение не имеет смысла
    /// </summary>
    public DateTime StartTime
    {
      get
      {
        lock (_SyncRoot) // здесь - надо, т.к. DateTime может быть прочитан частично
        {
          return _StartTime;
        }
      }
    }

    /// <summary>
    /// Для доступа к полю всегда должна использоваться инструкция Lock(FSyncRoot)
    /// </summary>
    private DateTime _StartTime;

    /// <summary>
    /// Дочерняя процедура обратного вызова, завершение которой сейчас ожидается.
    /// Свойство устанавливается для CurrentProc при вызове ExecProcCallBack.Execute().
    /// После завершения выполнения свойство сбрасывается в null (во избежание удержания объектов в памяти)
    /// </summary>
    public ExecProcCallBack ChildProcCallBack
    {
      get
      {
        //lock (FSyncRoot)
        //{
        return _ChildProcCallBack;
        //}
      }
      internal set
      {
        //lock (FSyncRoot)
        //{
        _ChildProcCallBack = value;
        //}
      }
    }
    private ExecProcCallBack _ChildProcCallBack;

    /// <summary>
    /// Возвращает всю информацию о текущей процедуре в виде объекта, поддерживающего сериализацию.
    /// Эта перегрузка метода не заполняет StackTrace, если процедура выполняется (стек исключения заполняется)
    /// </summary>
    /// <returns>Информация о текущей процедуре</returns>
    public ExecProcInfo GetInfo()
    {
      return GetInfo(false);
    }

    /// <summary>
    /// Возвращает всю информацию о текущей процедуре в виде объекта, поддерживающего сериализацию.
    /// </summary>
    /// <param name="useStackTrace">Нужно ли заполнять стек вызовов для выполняющихся процедур.
    /// Загрузка стека вызовов является ресурсоемким действием</param>
    /// <returns>Информация о текущей процедуре</returns>
    public ExecProcInfo GetInfo(bool useStackTrace)
    {
      ExecProcInfo Res = new ExecProcInfo();
      lock (_SyncRoot)
      {
        Res.InfoTime = DateTime.Now;
        Res.Guid = _Guid;
        Res.CreationTime = _CreationTime;
        //Res.AssemblyName = GetType().AssemblyQualifiedName;
        Res.AssemblyName = GetType().Assembly.FullName; // 06.06.2019
        Res.TypeName = GetType().FullName;
        Res.DisplayName = _DisplayName;
        Res.ActionName = _ActionName;

        //Res.StartTime = FStartTime;
        Res.StartCount = _StartCount;
        if (_StartCount > 0)
          Res.StartTime = _StartTime;
        Res.IsAsyncStarted = _IsAsyncStarted;

        Res.State = State;
        Res.AutoDispose = AutoDispose;
        Res.SyncTime = SyncTime;

        Thread trd = _ExecThread; // может обратиться в null асинхронно
        if (trd == null)
          Res.ThreadState = System.Threading.ThreadState.Unstarted;
        else
        {
          Res.ManagedThreadId = trd.ManagedThreadId; // 28.04.2018
          Res.ThreadState = trd.ThreadState;
        }

        ExecProc pp = _ParentProc;
        if (pp != null)
          Res.ParentProcGuid = pp.Guid;
        ExecProc cp = _ChildProc;
        if (cp != null)
          Res.ChildProcGuid = cp.Guid;
        ExecProcCallBack cpcb = _ChildProcCallBack;
        if (cpcb != null)
          Res.ChildProcCallBackGuid = cpcb.Guid;

        if (_ErrorDebugException != null)
        {
          Res.ExceptionMessage = _ErrorDebugException.Message;
          Res.ExceptionType = _ErrorDebugException.GetType().ToString();
          if (_ErrorDebugException.Data.Contains("OriginalStackTrace"))
            Res.StackTrace = _ErrorDebugException.Data["OriginalStackTrace"].ToString(); // 03.05.2018
          else
            Res.StackTrace = _ErrorDebugException.StackTrace;
          Res.ExceptionDebugArgs = _ErrorDebugArgs;
        }
        // 06.01.2021 - убрано
        //else if (useStackTrace && Res.IsExecuting && trd != null)
        //  Res.StackTrace = LogoutTools.GetStackTrace(trd); // 29.04.2018

        Res.InitDomainInfo();
      }
      return Res;
    }

    /// <summary>
    /// Создает новый объект ServerSplashWatcher.
    /// Этот метод не предназначен для использования в пользовательском коде
    /// </summary>
    /// <returns>Ссылка на объект</returns>
    public IServerSplashWatcher CreateSplashWatcher()
    {
      return new ServerSplashWatcher(SplashStack);
    }

    #endregion

    #region Переопределяемые методы выполнения процедуры

    /// <summary>
    /// Основной метод выполнения, который должен быть переопределен
    /// </summary>
    /// <param name="args">Аргументы вызова процедуры</param>
    /// <returns>Результат выполнения процедуры. Если процедура не возвращает результатов, следует вернуть NamedValues.Empty</returns>
    protected abstract NamedValues OnExecute(NamedValues args);

    /// <summary>
    /// Метод может быть переопределен, если требуется выполнять инициализацию перед вызовом основного метода
    /// </summary>
    /// <param name="args">Аргументы вызова процедуры</param>
    protected virtual void OnBeforeExecute(NamedValues args)
    {
#if DEBUG
      args.CheckIfMarshallable();
#endif

      if (TraceEnabled)
      {
        try
        {
          TraceBeforeExecute(args);
        }
        catch { }
      }

      _SplashStackStartIndex = _SplashStack.GetSplashStack().Length; // обычно 0
    }

    /// <summary>
    /// Метод может быть переопределен, если требуется выполнять действия после выполнения основного метода,
    /// даже если в нем возникла исключительная ситуация
    /// </summary>
    /// <param name="args">Аргументы выполнения процедуры</param>
    /// <param name="results">Результаты выполнения метода Execute(), если он выполнен успешно.
    /// Коллекция доступна для изменения, если метод Execute() не вызвал SetReadOnly()</param>
    /// <param name="exception">Объект ошибки, если при выполнении Execute() возникла исключительная ситуация</param>
    protected virtual void OnAfterExecute(NamedValues args, NamedValues results, Exception exception)
    {
#if DEBUG
      if (results != null)
      {
        results.CheckIfMarshallable();
      }
#endif

      // Дозакрываем заставки, для которых не было парного вызова EndExecute()
      while (_SplashStack.GetSplashStack().Length > _SplashStackStartIndex)
        _SplashStack.EndSplash();

      if (TraceEnabled)
      {
        try
        {
          TraceAfterExecute(args, results, exception);
        }
        catch { }
      }
    }

    #endregion

    #region Процентный индикатор

    /// <summary>
    /// Стек управления экранными заставками.
    /// В конструкторе инициализируется новым объектом ServerSplashStack, но это может быть изменено явной установкой свойства до выполнения процедуры
    /// </summary>
    public ISplashStack SplashStack
    {
      get { return _SplashStack; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (IsExecuting)
          throw new InvalidOperationException("Свойство SplashStack нельзя устанавливать во время выполнения процедуры");

        _SplashStack = value;
      }
    }
    private ISplashStack _SplashStack;

    /// <summary>
    /// Создать заставку, содержащую несколько фаз.
    /// </summary>
    /// <param name="phases">Список фаз</param>
    /// <returns>Интерфейс управления заставкой</returns>
    public ISplash BeginSplash(string[] phases)
    {
      CheckIsFromExecute();
      return _SplashStack.BeginSplash(phases);
    }

    /// <summary>
    /// Создать заставку, содержащую одну фазу
    /// </summary>
    /// <param name="phase">Текст фазы</param>
    /// <returns>Интерфейс управления заставкой</returns>
    public ISplash BeginSplash(string phase)
    {
      CheckIsFromExecute();
      return _SplashStack.BeginSplash(phase);
    }

    /// <summary>
    /// Создать заставку, содержащую одну фазу с текстом по умолчанию
    /// </summary>
    /// <returns>Интерфейс управления заставкой</returns>
    public ISplash BeginSplash()
    {
      CheckIsFromExecute();
      return _SplashStack.BeginSplash();
    }

    /// <summary>
    /// Закрыть текущую заставку. Количество вызовов EndSplash() должно
    /// соответствовать количеству BeginSplash()
    /// </summary>
    public void EndSplash()
    {
      CheckIsFromExecute();
      _SplashStack.EndSplash();
    }

    private int _SplashStackStartIndex;

    #endregion

    #region Статические списки

    #region Все процедуры

    /// <summary>
    /// Возвращает список процедур, созданных в текущем AppDomain, для которых не было вызова Dispose().
    /// </summary>
    /// <returns></returns>
    public static ExecProc[] GetAllProcs()
    {
      return _AllProcs.ToArray();
    }

    private static WeakReferenceCollection<ExecProc> _AllProcs = new WeakReferenceCollection<ExecProc>();

    /// <summary>
    /// Возвращает массив объектов ExecProcInfo для всех процедур ExecProc, для которых не было вызова Dispose().
    /// Информация о стеке вызовов не заполняется.
    /// </summary>
    /// <returns>Массив сведений о процедурах</returns>
    public static ExecProcInfo[] GetAllProcInfos()
    {
      return GetAllProcInfos(false);
    }

    /// <summary>
    /// Возвращает массив объектов ExecProcInfo для всех процедур ExecProc, для которых не было вызова Dispose().
    /// </summary>
    /// <param name="useStackTrace">Нужно ли заполнять стек вызовов для выполняющихся процедур.
    /// Загрузка стека вызовов является ресурсоемким действием</param>
    /// <returns>Массив сведений о процедурах</returns>
    public static ExecProcInfo[] GetAllProcInfos(bool useStackTrace)
    {
      ExecProc[] a1 = GetAllProcs();
      ExecProcInfo[] a2 = new ExecProcInfo[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].GetInfo(useStackTrace);
      return a2;
    }

    /// <summary>
    /// Возвращает количество процедур, созданных в текущем AppDomain, для которых не было вызова Dispose().
    /// Так как количество процедур может меняться асинхронно, это свойство может использоваться только
    /// в информативных целях. Для получения фиксированного списка используйте метод GetAllProcs()
    /// </summary>
    public static int AllProcCount
    {
      get
      {
        return _AllProcs.Count;
      }
    }

    #endregion

    #region Выполняющиеся процедуры

    /// <summary>
    /// Возвращает список процедур, выполняющихся в данный момент в текущем AppDomain.
    /// Так как процедуры могут запускаться и завершаться асинхронно, не гарантируется, что список
    /// содержит все процедуры, и все процедуры имеют состояние IsExecuting=true
    /// </summary>
    /// <returns></returns>
    public static ExecProc[] GetExecutingProcs()
    {
      return _ExecutingProcs.ToArray();
    }

    private static SyncCollection<ExecProc> _ExecutingProcs = new SyncCollection<ExecProc>();

    /// <summary>
    /// Возвращает количество процедур, выполняющихся в данный момент в текущем AppDomain.
    /// Так как процедуры могут запускаться и завершаться асинхронно, свойство может возвращать разные
    /// значения при последовательных вызовах
    /// </summary>
    public static int ExecutingProcCount
    {
      get
      {
        return _ExecutingProcs.Count;
      }
    }

    /// <summary>
    /// Возвращает массив объектов ExecProcInfo для всех выполняющихся процедур ExecProc
    /// Информация о стеке вызовов не заполняется.
    /// </summary>
    /// <returns>Массив информации о процедурах</returns>
    public static ExecProcInfo[] GetExecutingProcInfos()
    {
      return GetExecutingProcInfos(false);
    }

    /// <summary>
    /// Возвращает массив объектов ExecProcInfo для всех выполняющихся процедур ExecProc
    /// </summary>
    /// <param name="useStackTrace">Нужно ли заполнять стек вызовов.
    /// Загрузка стека вызовов является ресурсоемким действием</param>
    /// <returns>Массив информации о процедурах</returns>
    public static ExecProcInfo[] GetExecutingProcInfos(bool useStackTrace)
    {
      ExecProc[] a1 = GetExecutingProcs();
      ExecProcInfo[] a2 = new ExecProcInfo[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].GetInfo(useStackTrace);
      return a2;
    }

    #endregion

    #endregion

    #region Отладочные средства

    /// <summary>
    /// Получить отладочную информацию о процедуре в виде многострочного текста
    /// </summary>
    /// <returns></returns>
    public string CurrentDebugInfo
    {
      get
      {
        StringWriter sw = new StringWriter();
        TextWriterTraceListener Res = new TextWriterTraceListener(sw);
        LogoutInfoNeededEventArgs Args = new LogoutInfoNeededEventArgs(Res, null);
        GetDebugInfo(Args);
        Res.Flush();
        return sw.ToString();

      }
    }

    internal void GetDebugInfo(LogoutInfoNeededEventArgs args)
    {
      //Res.WriteLine("Серверная процедура");
      AddProcDetails(args);

      int OldLevel = args.IndentLevel;

      // TODO: ExecProc Ex2 = ExternalSplashOwner;
      // TODO: for (int i = 0; i < 100; i++)
      // TODO: {
      // TODO: if (Ex2 == null)
      // TODO:     break;

      // TODO:   Args.IndentLevel++;
      // TODO:   Args.WriteHeader("Вложенная процедура №" + (i + 1).ToString());
      // TODO:   AddProcDetails(Args);
      // TODO:   Args.IndentLevel--;
      // TODO:   Ex2 = Ex2.ExternalSplashOwner;
      // TODO: }

      args.IndentLevel = OldLevel;
    }

    /// <summary>
    /// Добавление отладочной информации о процедуре.
    /// Переопределенный метод может добавить свою информацию.
    /// Внимание! Метод вызывается асинхронно. Требуется блокировка
    /// </summary>
    /// <param name="args"></param>
    protected virtual void AddProcDetails(LogoutInfoNeededEventArgs args)
    {
      ExecProcInfo inf = this.GetInfo(true); // 29.04.2018
      LogoutTools.LogoutObject(args, inf);
    }

#if XXX
    /// <summary>
    /// Метод вызывается, когда при выполнении процедуры, вызванной клиентом,
    /// возникла ошибка.
    /// Метод вызывается в потоке выполнения процедуры и может добавлять
    /// данные в коллекцию Exception.Data.
    /// Метод не вызывается, если коллекция данных недоступна.
    /// Добавлять можно только сериализуемые значения (желательно типа string).
    /// Метод вызывается ПЕРЕД AfterExecute().
    /// </summary>
    /// <param name="e">Сгенерированное исключение</param>
    protected virtual void AddExecExceptionData(Exception e)
    {
      e.Data["Server.ComputerName"] = Environment.MachineName;

      // TODO: 
      try
      {
        CSEnvironment Env = new CSEnvironment();
        e.Data["Server.OSVersion"] = Env.OSVersionText;
      }
      catch
      {
      }
      e.Data["Server.NETVersion"] = Environment.Version.ToString();
      e.Data["Server.IntPtrSize"] = IntPtr.Size == 4 ? "32 bit" : "64 bit";
      try
      {
        Process prc = Process.GetCurrentProcess();
        if (prc != null)
        {
          e.Data["Server.ProcessStarted"] = prc.StartTime;
          e.Data["Server.CurrentTime"] = DateTime.Now;
          e.Data["Server.WorkTime"] = DateTime.Now - prc.StartTime;
        }
      }
      catch { }

      e.Data["Server.Class"] = this.GetType().ToString();
      e.Data["Server.ExecId"] = CurrentExecId.ToString();
      try
      {
        if (CurrentSplash != null)
          e.Data["Splash.PhaseText"] = CurrentSplash.PhaseText;
      }
      catch { }

      e.Data["Server.ExecStartTime"] = ExecStartTime;
      e.Data["Server.ExecStopTime"] = ExecStopTime;
    }
#endif

    /// <summary>
    /// Вывод отладочной информации обо всех выполняющихся процедурах
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
      args.WriteHeader("ExecProc");
      args.WritePair("Current AppDomain", AppDomain.CurrentDomain.FriendlyName);
      args.WritePair("Current ExecProc", ExecProc.CurrentProc == null ? "none" : ExecProc.CurrentProc.ToString());

      int IndentLevel = args.IndentLevel;

      ExecProc[] Procs1 = ExecProc.GetExecutingProcs();
      args.WritePair("Executing ExecProcs", Procs1.Length.ToString());
      int cnt = 0;
      for (int i = 0; i < Procs1.Length; i++)
      {
        DoLogoutOneProc(args, Procs1[i], cnt);
        cnt++;
        args.IndentLevel = IndentLevel;
      }

      ExecProc[] Procs2 = ExecProc.GetAllProcs();
      args.WritePair("All ExecProcs", (Procs2.Length - Procs1.Length).ToString());
      for (int i = 0; i < Procs2.Length; i++)
      {
        if (Array.IndexOf<ExecProc>(Procs1, Procs2[i]) >= 0)
          continue; // выполняющая процедура была выведена
        DoLogoutOneProc(args, Procs2[i], cnt);
        cnt++;
        args.IndentLevel = IndentLevel;
      }

      cnt = 0;
      RemoteExecProc[] Procs3 = RemoteExecProc.GetExecutingProcs();
      args.WritePair("Executing RemoteExecProcs", Procs3.Length.ToString());

      for (int i = 0; i < Procs3.Length; i++)
      {
        DoLogoutOneProc(args, Procs3[i], cnt);
        cnt++;
        args.IndentLevel = IndentLevel;
      }

      RemoteExecProc[] Procs4 = RemoteExecProc.GetAllProcs();
      args.WritePair("All RemoteExecProcs", (Procs4.Length - Procs3.Length).ToString());
      for (int i = 0; i < Procs4.Length; i++)
      {
        if (Array.IndexOf<RemoteExecProc>(Procs3, Procs4[i]) >= 0)
          continue; // выполняющая процедура была выведена
        DoLogoutOneProc(args, Procs4[i], cnt);
        cnt++;
        args.IndentLevel = IndentLevel;
      }
    }

    private static void DoLogoutOneProc(LogoutInfoNeededEventArgs args, IExecProc proc, int cnt)
    {
      args.WritePair("[" + cnt.ToString() + "]", proc.ToString());
      args.IndentLevel++;
      try
      {
        if (proc is ExecProc)
          ((ExecProc)proc).GetDebugInfo(args); // могут быть дополнительные сведения
        else
        {
          ExecProcInfo inf = proc.GetInfo();
          LogoutTools.LogoutObject(args, inf);
        }
      }
      catch (Exception e)
      {
        args.WriteLine("*** Ошибка получения сведений о процедуре. *** " + e.Message);
      }
    }

    static void LogoutTools_LogoutProp(object sender, LogoutPropEventArgs args)
    {
      if (args.Object is ExecProc)
      {
        switch (args.PropertyName)
        {
          case "CurrentDebugInfo": // 13.01.2016
          case "Proxy": // 29.05.2017
            args.Mode = LogoutPropMode.None;
            break;
          //case "Results": // 16.01.2016
          //  Args.Mode = LogoutPropMode.ToString;
          //  break;
        }
      }
      //else if (Args.Object is RemoteExecProc)
      //{
      //  switch (Args.PropertyName)
      //  {
      //    case "Results": // 16.01.2016
      //      Args.Mode = LogoutPropMode.ToString;
      //      break;
      //  }
      //}
    }

    #endregion

    #region Трассировка

    /// <summary>
    /// Вывод трассировочной информации (переключатель).
    /// Автоматически инициализируется, если в конфигурационном файле приложения задан ключ "TraceExecProc"="1"
    /// При создании объекта ExecProc, начальное значение свойства TraceEnabled устанавливается на основании этого значения
    /// </summary>
    public static readonly System.Diagnostics.BooleanSwitch TraceSwitch = new System.Diagnostics.BooleanSwitch("TraceExecProc", "Вывод отладочной информации при выполнении процедур");

    /// <summary>
    /// Управляет выводом трассировочной информации для этой процедуры.
    /// Если свойство не установлено в явном виде для этой процедуры, то оно возвращает значение в соответствии с TraceSwitch.
    /// Если свойство установлено выводится информация о запуске и завершении процедуры
    /// </summary>
    public bool TraceEnabled { get { return _TraceEnabled ?? TraceSwitch.Enabled; } set { _TraceEnabled = value; } }
    private bool? _TraceEnabled;

    /// <summary>
    /// Восстанавливает значение свойства TraceEnabled
    /// </summary>
    public void ResetTraceEnabled()
    {
      _TraceEnabled = null;
    }

    /// <summary>
    /// Трассирощик процедур по умолчанию.
    /// Используется, если в контексте выполнения процедуры нет элемента с ключом "Tracer"
    /// </summary>
    public static IExecProcTracer DefaultTracer
    {
      get { return _DefaultTracer; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _DefaultTracer = value;
      }
    }
    private static IExecProcTracer _DefaultTracer = new ExecProcDefaultTracer();

    private class ExecProcDefaultTracer : IExecProcTracer
    {
      #region IExecProcTracer Members

      public void TraceBeforeExecute(ExecProc caller, NamedValues args)
      {
        System.Diagnostics.Trace.WriteLine(GetTracePrefix() + "Procedure " + caller.ToString() + " started");
      }

      public void TraceAfterExecute(ExecProc caller, NamedValues args, NamedValues results, Exception exception)
      {
        if (exception == null)
        {
          TimeSpan ts = DateTime.Now - caller.StartTime;
          System.Diagnostics.Trace.WriteLine(GetTracePrefix() + "Procedure " + caller.ToString() + " finished. Executing time=" + ts.ToString());
        }
        else
          System.Diagnostics.Trace.WriteLine(GetTracePrefix() + "Procedure " + caller.ToString() + " failed. " + exception.Message);
      }

      private static string GetTracePrefix()
      {
        string s = EnvironmentTools.ApplicationName + " [" + Process.GetCurrentProcess().Id.ToString() + "], ThreadId=" + Thread.CurrentThread.ManagedThreadId;
        if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
          s += "AppDomain=\"" + AppDomain.CurrentDomain.FriendlyName + "\"";
        return s + ". ";
      }

      #endregion
    }

    /// <summary>
    /// Метод вызывается при трассировке начала выполнения процедуры.
    /// Если этот метод не переопределен в производном классе, то из контекста извлекается элемент "Tracer".
    /// Если его нет, берется значение из свойства DefayultTracer. После этого вызывается метод интерфейса IExecProcTracer
    /// </summary>
    /// <param name="args">Аргументы, которая получила процедура перед выполнением</param>
    protected virtual void TraceBeforeExecute(NamedValues args)
    {
      IExecProcTracer Tracer = Context["Tracer"] as IExecProcTracer;
      if (Tracer == null)
        Tracer = DefaultTracer;
      Tracer.TraceBeforeExecute(this, args);
    }

    /// <summary>
    /// Метод вызывается при трассировке окончания выполнения процедуры или если выполнение завершилось исключением
    /// Если этот метод не переопределен в производном классе, то из контекста извлекается элемент "Tracer".
    /// Если его нет, берется значение из свойства DefayultTracer. После этого вызывается метод интерфейса IExecProcTracer
    /// </summary>
    /// <param name="args">Аргументы, которая получила процедура перед выполнением</param>
    /// <param name="results">Результаты выполнения, если процедура завершена успешно</param>
    /// <param name="exception">Исключение, если выполнение завершилось ошибкой</param>
    protected virtual void TraceAfterExecute(NamedValues args, NamedValues results, Exception exception)
    {
      IExecProcTracer Tracer = Context["Tracer"] as IExecProcTracer;
      if (Tracer == null)
        Tracer = DefaultTracer;
      Tracer.TraceAfterExecute(this, args, results, exception);
    }

    #endregion

    #region Блокировки

    /// <summary>
    /// Метод вызывается в начале ожидания "длительной" блокировки (после 0.3 секунд ожидания, когда выводится заставка)
    /// Предназначен для отладки
    /// </summary>
    /// <param name="logger">Объект с информацией о состояния ожидания</param>
    internal protected virtual void LongLockStarted(ExecProcLockLogger logger)
    {
    }

    /// <summary>
    /// Метод вызывается с определенным интервалом в процессе ожидания "длительной" блокировки (начавшейся после 0.3 секунд ожидания, когда выводится заставка)
    /// Предназначен для отладки.
    /// </summary>
    /// <param name="stepInfo">Текущий владелец блокировки и заблокированный объект
    /// В процессе ожидания значения свойств могут меняться, при этом используется единственный экземпляр ServerExecLockStepInfo</param>
    internal protected virtual void LongLockStep(ExecProcLockStepInfo stepInfo)
    {
    }

    /// <summary>
    /// Метод вызывается в конце ожидания "длительной" блокировки (начавшейся после 0.3 секунд ожидания, когда выводится заставка)
    /// Предназначен для отладки.
    /// Переопределенный метод может вычислить время ожидания блокировки
    /// </summary>
    /// <param name="logger">Объект с информацией о переходе в состояние ожидания</param>
    internal protected virtual void LongLockFinished(ExecProcLockLogger logger)
    {
    }

    #endregion

    #region Завершение работы процедуры

    /// <summary>
    /// Ищет среди выполняющихся процедур с заданным идентификатором.
    /// Если процедура найдена и выполняется, для ее потока вызывается ThreadAbort().
    /// Если в процессе поиска процедура уже завершилась, никаких действий не выполняется и возвращается true
    /// </summary>
    /// <param name="guid">Идентификатор процедуры</param>
    /// <returns>true, если есть выполняющаяся процедура с заданным идентификатором</returns>
    public static bool AbortExecution(Guid guid)
    {
      ExecProc[] Procs = GetExecutingProcs();
      for (int i = 0; i < Procs.Length; i++)
      {
        if (Procs[i].Guid == guid)
        {
          Thread th = Procs[i].ExecThread;
          if (th != null)
            th.Abort();
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Ищет среди выполняющихся процедур с заданным идентификатором.
    /// Если процедура найдена и выполняется, для ее потока вызывается ThreadAbort().
    /// После этого для процедуры вызывается Dispose()
    /// </summary>
    /// <param name="guid">Идентификатор процедуры</param>
    /// <param name="milliseconds">Интервал времени ожидания завершения в миллисекундах.
    /// Может быть задано значение Timeout.Infinite для бесконечного ожидания</param>
    /// <returns>true, если есть выполняющаяся процедура с заданным идентификатором</returns>
    public static bool AbortExecutionAndDispose(Guid guid, int milliseconds)
    {
      ExecProc[] Procs = GetAllProcs();
      for (int i = 0; i < Procs.Length; i++)
      {
        if (Procs[i].Guid == guid)
        {
          Procs[i].AbortExecutionAndDispose(milliseconds);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Прерывание выполнения процедуры с ожиданием.
    /// Если процедура выполняется, для ее потока вызывается ThreadAbort().
    /// После этого для процедуры вызывается Dispose()
    /// </summary>
    /// <param name="milliseconds">Интервал времени ожидания завершения в миллисекундах.
    /// Может быть задано значение Timeout.Infinite для бесконечного ожидания</param>
    /// <returns>true, если есть выполняющаяся процедура с заданным идентификатором</returns>
    public void AbortExecutionAndDispose(int milliseconds)
    {
      Thread th = this.ExecThread; // Обязательно так. Свойство ExecThread может меняться динамически
      if (th != null)
      {
        //if (th == Thread.CurrentThread)
        //  throw new InvalidOperationException("Нельзя прекратить выполнение процедуры из ее собственного потока");
        th.Abort();
        th.Join(milliseconds);
      }
      this.Dispose();
    }

    #endregion

    #region Proxy

    /// <summary>
    /// Создает переходник для удаленного доступа.
    /// Этот объект должен передаваться клиенту для реализации удаленного доступа.
    /// Метод можно вызывать только один раз при StateUnstarted, если свойство AutoDispose=true.
    /// Для "многоразовых" процедур возвращается один и тот же объект
    /// </summary>
    public ExecProcProxy CreateProxy()
    {
      if (_Proxy == null)
      {
        if (AutoDispose)
        {
          if (State != ExecProcState.NotStarted)
            throw new InvalidOperationException("Процедура находится в состоянии " + State.ToString());
        }
        _Proxy = new ExecProcProxy1(this);
      }
      else
      {
        if (AutoDispose)
          throw new InvalidOperationException("Повторный вызов CreateProxy()");
      }
      return _Proxy;
    }

    internal ExecProcProxy Proxy { get { return _Proxy; } }
    private ExecProcProxy1 _Proxy;

    /// <summary>
    /// Этот метод вызывается, когда ExecProcProxy запрашивает продление времени жизни объекта удаленного доступа.
    /// Не вызывается, если State=Disposed или State=Executing.
    /// По умолчанию метод возвращает true, если AutoDispose=false. Для одноразовой процедуры возвращается false.
    /// Переопределенный метод может не продлять существование объекта, если нет гарантии удаления серверной процедуры клиентом.
    /// </summary>
    /// <returns>Надо ли продлять существование объекта</returns>
    internal protected virtual bool WantsRenewalProxy()
    {
      return !AutoDispose; // 29.08.2020
    }

    #endregion

    #region Быстрый вызов со стороны клиента

    /// <summary>
    /// Реализация "разделенного" запроса клиента с минимальным количеством сетевых вызовов.
    /// Запрос называется "разделенным", так как запуск процедуры с передачей аргументов инициализируется на сервере,
    /// а заканчивается получением результатов на стороне клиента
    /// 
    /// Процедура ExecProc в таком сценарии является "одноразовой".
    /// Клиент запрашивает у сервера выполнение некоторых действий, вероятно, передавая ему аргументы.
    /// Сервер создает процедуру ExecProc и подготавливает контекст. Можно настроить свойство SyncTime.
    /// Затем вызывается этот метод, а объект DistributedCallData возвращается клиенту.
    /// Клиент может создать объект RemoteDistributedProc и вызвать его методы BeginExedcute() и EndExecute().
    /// Альтернативно, клиент создает объект DistributedProcCallItem, передавая DistributedCallData конструктору. 
    /// Затем вызывает метод ExecProcCallList.ExecuteAync() или ExecuteAsyncAndWait() и получает результаты.
    /// 
    /// 
    /// Этот метод создает ExecProcProxy и начинает асинхронное выполнение вызовом BeginExecute(). В течение периода SyncTime
    /// ожидается завершение. Если процедура успела завершится, выполняется EndExecute() и вызывается Dispose().
    /// Клиенту возвращаются готовые результаты. Если при выполнении возникло исключение, оно перевыбрасывается этим методом
    /// и обрабатывается клиентом.
    /// Если процедура выполняется медленно, клиенту передается ExecProcProxy, для которой он будет выполнять обычные действия.
    /// 
    /// Таким образом, при быстром выполнении, достаточно одного запроса сервера, как при синхронном вызове, но с сохранением
    /// возможности асинхронного выполнения длительной процедуры.
    /// </summary>
    /// <param name="args">Аргументы, которые передаются процедуры</param>
    /// <returns>Готовые результаты или прокси для асинхронного управления клиентом</returns>
    public DistributedCallData StartDistributedCall(NamedValues args)
    {
      if (!AutoDispose)
        throw new InvalidOperationException("Свойство AutoDispose должно иметь значение true");

      ExecProcProxy proxy = CreateProxy(); // здесь проверяется и состояние процедуры
      NamedValues res = proxy.Handler.InternalBeginExecute(args, true, SyncTime, true);
      if (res == null)
        return new DistributedCallData(Guid, DisplayName, null, proxy);

      // Вызывать Dispose() не надо, так как это уже сделано или будет сделано очень скоро

      // Эта проверка недействительна!
      // Метод Dispose() вызывает из потока, в котором выполнялась процедура и до Dispose() могла не дойти очередь.
      //      if (!IsDisposed)
      //        throw new BugException("Результаты получены, но не было вызова Dispose");

      return new DistributedCallData(Guid, DisplayName, res, null);
    }

    #endregion
  }

  /// <summary>
  /// Параметры, управляющие интервалами времени лицензии Net Remoting для ExecProcProxy и ExecProcProxy2
  /// </summary>
  [Serializable]
  public class ExecProcLeaseSettings : ICloneable, IReadOnlyObject
  {
    #region Защищенный конструктор

    /// <summary>
    /// Создает новый объект со значениями по умолчанию.
    /// Как правило, следует использовать метод Clone()
    /// </summary>
    public ExecProcLeaseSettings()
    {
      _InitialLeaseTime = System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseTime;
      _RenewOnCallTime = System.Runtime.Remoting.Lifetime.LifetimeServices.RenewOnCallTime;
      _SponsorshipTimeout = System.Runtime.Remoting.Lifetime.LifetimeServices.SponsorshipTimeout;
      _RenewalTime = TimeSpan.FromMinutes(2); // Значение по умолчанию для ClientSponsor
    }

    /// <summary>
    /// Используется для клонирования
    /// </summary>
    /// <param name="dummy"></param>
    private ExecProcLeaseSettings(bool dummy)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Начальное время лицензии
    /// </summary>
    public TimeSpan InitialLeaseTime
    {
      get { return _InitialLeaseTime; }
      set
      {
        CheckNotReadOnly();
        _InitialLeaseTime = value;
      }
    }
    private TimeSpan _InitialLeaseTime;

    /// <summary>
    /// Срок, на который продляется лицензия при вызове любого метода
    /// </summary>
    public TimeSpan RenewOnCallTime
    {
      get { return _RenewOnCallTime; }
      set
      {
        CheckNotReadOnly();
        _RenewOnCallTime = value;
      }
    }
    private TimeSpan _RenewOnCallTime;

    /// <summary>
    /// Время ожидания ответа спонсора
    /// </summary>
    public TimeSpan SponsorshipTimeout
    {
      get { return _SponsorshipTimeout; }
      set
      {
        CheckNotReadOnly();
        _SponsorshipTimeout = value;
      }
    }
    private TimeSpan _SponsorshipTimeout;

    /// <summary>
    /// Время, на которое продляется время жизни объекта.
    /// Продление выполняется, пока не разрушен базовый объект ExecProc.
    /// </summary>
    public TimeSpan RenewalTime
    {
      get { return _RenewalTime; }
      set
      {
        CheckNotReadOnly();
        _RenewalTime = value;
      }
    }
    private TimeSpan _RenewalTime;

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию объекта со сброшенным свойством IsReadOnly
    /// </summary>
    /// <returns>Копия объекта</returns>
    public ExecProcLeaseSettings Clone()
    {
      ExecProcLeaseSettings Res = new ExecProcLeaseSettings(false);
      Res._InitialLeaseTime = InitialLeaseTime;
      Res._RenewOnCallTime = RenewOnCallTime;
      Res._SponsorshipTimeout = SponsorshipTimeout;
      Res._RenewalTime = RenewalTime;
      return Res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если параметры менять нельзя
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Проверка возможности изменения параметров
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Установка свойства IsReadOnly в true
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Значения по умолчанию

    /// <summary>
    /// Начальные значения параметров, используемые ExecProcProxy и ExecProcProxy2.
    /// Значения параметров можно инициализировать до первого создания объектов
    /// </summary>
    public static ExecProcLeaseSettings DefaultSettings { get { return _DefaultSettings; } }
    private static readonly ExecProcLeaseSettings _DefaultSettings = new ExecProcLeaseSettings();

    #endregion

    #region Инициализация лицензии

    /// <summary>
    /// Устанавливает значения свойств объкта ILease.
    /// </summary>
    /// <param name="lease">Интерфейс заполняемого объекта</param>
    public void InitLease(System.Runtime.Remoting.Lifetime.ILease lease)
    {
      lease.InitialLeaseTime = InitialLeaseTime;
      lease.RenewOnCallTime = RenewOnCallTime;
      lease.SponsorshipTimeout = SponsorshipTimeout;
    }

    #endregion
  }

  /// <summary>
  /// Данные, передаваемые от сервера к клиенту для реализации "разделенного" вызова серверной процедуры
  /// Результат вызова ExecProc.DistributedCall().
  /// Объект должен быть возвращен клиенту и передан в конструктор RemoteDistributed или DistributedProcCallItem.
  /// </summary>
  [Serializable]
  public sealed class DistributedCallData
  {
    #region Защищенный конструктор

    internal DistributedCallData(Guid guid, string displayName, NamedValues results, ExecProcProxy proxy)
    {
      if (results == null && proxy == null)
        throw new ArgumentNullException();

      _Guid = guid;
      _DisplayName = displayName;
      _Results = results;
      _Proxy = proxy;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор процедуры ExecProc
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private Guid _Guid;

    /// <summary>
    /// Копия свойства ExecProc.DisplayName.
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    // Свойства ActionInfo нет, так как не гарантировано, что на момент создания RemoteSingleAsyncCallData 
    // оно будет установлено в ExecProc, т.к. установка выполняется асинхронно.

    /// <summary>
    /// Если процедура выполнилась быстро, в течение ExecProc.SyncTime, сюда помещается результат выполнения.
    /// Если процедура выполнилась быстро, но с выбросом исключения, исключение передается клиенту без использования этого объекта
    /// </summary>
    internal NamedValues Results { get { return _Results; } }
    private NamedValues _Results;

    /// <summary>
    /// Если процедура не успела выполниться быстро, сюда помещается объект для асинхронного управления процедурой
    /// </summary>
    internal ExecProcProxy Proxy { get { return _Proxy; } }
    private ExecProcProxy _Proxy;

    /// <summary>
    /// Возвращает true, если процедура успела завершиться быстро и этот объект содержит готовые данные.
    /// Впрочем, это ни на что не влияет в прикладном коде
    /// </summary>
    public bool IsCompleted { get { return _Results != null; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = DisplayName;
      if (String.IsNullOrEmpty(s))
        s = Guid.ToString();
      return s + ", IsCompleted=" + IsCompleted.ToString();
    }

    #endregion

    #region Замена прокси

    /// <summary>
    /// Создает копию с заменой прокси на ExecProcProxy2.
    /// Используется, когда на сервере используется AppDomain, отличный от того, который взаимодействует с клиентом,
    /// то есть в тех случаях, когда требуется класс ExecProcProxy2.
    /// </summary>
    /// <returns>Объект с замененным ExecProcProxy</returns>
    public DistributedCallData CreateProxy2()
    {
      if (_Results != null)
        // можно ничего не делать
        return this;
      else
        return new DistributedCallData(Guid, DisplayName, null, new ExecProcProxy2(_Proxy));
    }

    #endregion
  }

  /// <summary>
  /// Прокси для удаленного вызова процедур.
  /// Прикладной код сервера должен использовать свойство ExecProc.Proxy и передавать объект клиенту.
  /// При сложной переадресации, прикладной код клиента может создать дополнительный объект ExecProcProxy2.
  /// Затем, прикладной код клиента создает экземпляр RemoteExecProc, который принимает ExecProcProxy в качестве аргумента.
  /// 
  /// Класс ExecProcProxy имеет мало открытых свойств и методов
  /// </summary>
  [Serializable]
  public abstract class ExecProcProxy
  {
    /*
     * 25.08.2020
     * Чтобы уменьшить количество вызовов сервера, используем составной прокси.
     * Класс ExecProcProxy является передаваемым Marshal-By-Value. Он содержит фиксированную информацию
     * Класс ExecProcProxy.InternalHandler является Marshal-By-Reference и содержит методы, вызываемые через Remoting.
     */

    #region Защищенный конструктор

    internal ExecProcProxy(Guid guid, string displayName, bool autoDispose, int syncTime, InternalHandler handler)
    {
      _Guid = guid;
      _DisplayName = displayName;
      _AutoDispose = autoDispose;
      _SyncTime = syncTime;
      _Handler = handler;
    }

    #endregion

    #region Фиксированная информация о процедуре

    /// <summary>
    /// Guid процедуры ExecProc.Guid.
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private readonly Guid _Guid;

    /// <summary>
    /// Отображаемое имя процедуры
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;


    /// <summary>
    /// Исходное значение свойства ExecProc.AutoDispose.
    /// Для RemoteExecProc есть своя копия свойства, значение которого может быть переопределено
    /// </summary>
    internal bool AutoDispose { get { return _AutoDispose; } }
    private bool _AutoDispose;

    /// <summary>
    /// Исходное значение свойства ExecProc.AutoDispose.
    /// Для RemoteExecProc есть своя копия свойства, значение которого может быть переопределено
    /// </summary>
    internal int SyncTime { get { return _SyncTime; } }
    private int _SyncTime;

    #endregion

    #region Управление временем жизни

    /// <summary>
    /// Настройки управления лицензией для Net Remoting.
    /// Значения по умолчанию берутся из ExecProcLeaseSettings.DefaultSettings
    /// Параметры можно менять до того, как ссылка на ExecProcProxy передана клиенту.
    /// На стороне клиента свойства объекта ExecProcLeaseSettings доступны только для чтения.
    /// </summary>
    public ExecProcLeaseSettings LeaseSettings
    {
      get
      {
        if (_LeaseSettings == null)
        {
          if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(_Handler))
          {
            ExecProcLeaseSettings ls = _Handler.LeaseSettings.Clone();
            ls.SetReadOnly();
            _LeaseSettings = ls;
          }
          else
            _LeaseSettings = _Handler.LeaseSettings;
        }
        return _LeaseSettings;
      }
    }

    /// <summary>
    /// Копия ссылки на InternalHandler.LeaseSettings или отдельная копия "только для просмотра" для клиента.
    /// Пока не было обращения к свойству, поле содержит null.
    /// </summary>
    [NonSerialized]
    private ExecProcLeaseSettings _LeaseSettings;

    #endregion

    #region InternalHandler

    /// <summary>
    /// Внутренний интерфейс, реализуемый ExecProcProxy и ExecProcProxy2.
    /// Пользовательский код не должен использовать методы этого интерфейса, но передавать ссылку через Net Remoting для создания объекта
    /// RemoteExecProc на стороне клиента
    /// </summary>
    internal abstract class InternalHandler : MarshalByRefObject, System.Runtime.Remoting.Lifetime.ISponsor
    {
      #region Защищенный конструктор

      internal InternalHandler()
      {
        ExecProcLeaseSettings.DefaultSettings.SetReadOnly();
        _LeaseSettings = ExecProcLeaseSettings.DefaultSettings.Clone();
      }

      #endregion

      #region Абстрактные методы

      // Не актуально?
      //// 16.03.2018
      //// Нельзя делать реализацию методов интерфейса IExecProcProxy защищенной (internal)
      //// Для совместимости с MONO,
      //// все методы должны быть public, так как они вызываются через удаленный интерфейс	

      /// <summary>
      /// Возвращает текущее состояние процедуры
      /// </summary>
      public abstract ExecProcState State { get; }

      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      /// <param name="args"></param>
      /// <param name="autoDispose"></param>
      /// <returns></returns>
      public abstract NamedValues InternalExecute(NamedValues args, bool autoDispose);

      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      /// <param name="args">Аргументы вызова процедуры</param>
      /// <param name="autoDispose">Значение свойства RemoteExecProc.AutoDispose, действующее на момент вызова</param>
      /// <param name="syncTime">Значение свойства RemoteExecProc.SyncTime</param>
      /// <param name="useSplashWatcher">Используется ли наблюдение за прогрессом выполнения</param>
      /// <returns>Обычно возвращается null, что означает, что процедура выполняется и должен быть выполнен вызов InternalEndExecute().
      /// Если процедура выполнилась в течение интервала <paramref name="syncTime"/>, то уже выполнен вызов EndExecute()</returns>
      public abstract NamedValues InternalBeginExecute(NamedValues args, bool autoDispose, int syncTime, bool useSplashWatcher);


      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      public abstract NamedValues InternalTryEndExecute(bool splashInfoNeeded, out SplashInfoPack splashInfoPack);

      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      /// <param name="args"></param>
      public abstract void InternalExecuteNoResults(NamedValues args);

      /// <summary>
      /// Возвращает подробную информацию о состоянии процедуры
      /// </summary>
      /// <returns></returns>
      public abstract ExecProcInfo GetInfo();

      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      public abstract bool WantsRenewal { get; }

      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      public abstract void Dispose();

      /// <summary>
      /// Прерывает выполнение процедуры, если она создала процентный индикатор и установила AllowCancel=true.
      /// </summary>
      public abstract void Cancel();

      /// <summary>
      /// После сброса, следующий вызов InternalGetIsCompleted() вернет полную информацию о стеке заставок
      /// </summary>
      public abstract void ResetSplashInfo();

      /// <summary>
      /// Создает объект для отслеживания стека заставок.
      /// Этот метод не должен использоваться в прикладном коде
      /// </summary>
      /// <returns>Ссылка на новый объект ServerSplashWatcher</returns>
      public abstract IServerSplashWatcher CreateSplashWatcher();

      #endregion

      #region Управлением временем жизни и ISponsor members

      /// <summary>
      /// Настройки управления лицензией для Net Remoting.
      /// Значения по умолчанию берутся из ExecProcLeaseSettings.DefaultSettings
      /// Параметры можно менять до вызова InitializeLifetimeService()
      /// </summary>
      public ExecProcLeaseSettings LeaseSettings { get { return _LeaseSettings; } }
      private ExecProcLeaseSettings _LeaseSettings;

      /// <summary>
      /// Инициализирует объект ILease установками LeaseSettings.
      /// Переводит LeaseSettings в режим IsReadOnly.
      /// </summary>
      /// <returns>Объект, реализующий ILease</returns>
      public override object InitializeLifetimeService()
      {
        System.Runtime.Remoting.Lifetime.ILease Lease = (System.Runtime.Remoting.Lifetime.ILease)base.InitializeLifetimeService();
        if (Lease.CurrentState == System.Runtime.Remoting.Lifetime.LeaseState.Initial)
        {
          _LeaseSettings.SetReadOnly();
          _LeaseSettings.InitLease(Lease);
          Lease.Register(this);
        }
        return Lease;
      }

      TimeSpan System.Runtime.Remoting.Lifetime.ISponsor.Renewal(System.Runtime.Remoting.Lifetime.ILease lease)
      {
        if (WantsRenewal)
          return _LeaseSettings.RenewalTime;
        else
          return TimeSpan.Zero;
      }

      #endregion
    }

    /// <summary>
    /// Основной Marshal-By-Reference объект для вызова процедуры на сервере
    /// </summary>
    internal InternalHandler Handler { get { return _Handler; } }
    private readonly InternalHandler _Handler;

    #endregion
  }

  /// <summary>
  /// Переходник для ExecProc для поддержки удаленных вызовов.
  /// Для доступа к объекту используется свойство ExecProc.Proxy.
  /// Этот объект должен быть передан от сервера клиенту, где он передается конструктору RemoteExecProc.
  /// Пользовательский код не должен использовать методы этого объекта, начинающиеся с Internal
  /// </summary>
  [Serializable]
  internal sealed class ExecProcProxy1 : ExecProcProxy
  {
    #region Защищенный конструктор

    internal ExecProcProxy1(ExecProc execProc)
      : base(execProc.Guid, execProc.DisplayName, execProc.AutoDispose, execProc.SyncTime, new InternalHandler1(execProc))
    {
    }

    #endregion

    #region InternalHandler1

    internal sealed class InternalHandler1 : InternalHandler
    {
      #region Конструктор

      internal InternalHandler1(ExecProc execProc)
      {
        ExecProc = execProc;
        _Guid = execProc.Guid;
      }

      #endregion

      #region Управление временем жизни

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override bool WantsRenewal
      {
        get
        {
          ExecProc ep = ExecProc;
          if (ep == null)
            return false;
          else if (ep.State == ExecProcState.Disposed)
            return false;
          else if (ep.State == ExecProcState.Executing)
            return true; // 29.08.2020
          else
            return ep.WantsRenewalProxy();
        }
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Ссылка на процедуру-владелец.
      /// Это поле сбрасывается в null при вызове ExecProc.Dispose()
      /// </summary>
      internal volatile ExecProc ExecProc;

      /// <summary>
      /// Возвращает true, если серверная процедура была удалена.
      /// </summary>
      public override ExecProcState State
      {
        get
        {
          ExecProc ep = ExecProc;
          if (ep == null)
            return ExecProcState.Disposed;
          else
            return ep.State;
        }
      }

      #endregion

      #region Выполнение

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override NamedValues InternalExecute(NamedValues args, bool autoDispose)
      {
        CheckNotDisposed();
        ExecProc.AutoDispose = autoDispose;

        NamedValues Results = ExecProc.Execute(args);
        return Results;
      }

      /// <summary>
      /// На время асихронного выполнения держим в закрытом поле интерфейс IAsyncResult
      /// </summary>
      private IAsyncResult _AsyncResult2;

      /// <summary>
      /// На время асихронного выполнения держим в закрытом поле интерфейс IAsyncResult
      /// </summary>
      private IAsyncResultWithSplash _AsyncResult2spl;

      /// <summary>
      /// На время асихронного выполнения держим еще одну копию ссылки
      /// </summary>
      private ExecProc _ExecProc2;

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override NamedValues InternalBeginExecute(NamedValues args, bool autoDispose, int syncTime, bool useSplashWatcher)
      {
        ExecProc ep = ExecProc;
        if (ep == null)
          ep = _ExecProc2;
        if (ep == null)
          throw new ObjectDisposedException("Процедура уже была завершена");
        CheckNotDisposed();

        if (_AsyncResult2 != null)
        {
          Exception e = new InvalidOperationException("Предыдущий асинхронный вызов еще не завершен");
          try
          {
            e.Data["PrevExecProcInfo"] = ep.GetInfo(true);
          }
          catch { }
          throw e;
        }

        ExecProc.AutoDispose = autoDispose;
        ExecProc.SyncTime = syncTime;
        if (useSplashWatcher)
        {
          _AsyncResult2spl = ExecProc.BeginExecuteWithSplash(args, null, null);
          _AsyncResult2 = _AsyncResult2spl;
        }
        else
        {
          _AsyncResult2 = ExecProc.BeginExecute(args, null, null);
        }

        _ExecProc2 = ep;

        if (_AsyncResult2.IsCompleted)
        {
          SplashInfoPack dummy;
          try
          {
            return InternalTryEndExecute(false, out dummy);
          }
          finally
          {
            _ExecProc2 = null;
          }
        }
        else
          return null;
      }

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override NamedValues InternalTryEndExecute(bool splashInfoNeeded, out SplashInfoPack splashInfoPack)
      {
        if (_AsyncResult2 == null)
          throw new ArgumentException("Не было вызова BeginExecute()");

        bool isCompleted;
        if (splashInfoNeeded)
          isCompleted = _AsyncResult2spl.GetIsCompleted(out splashInfoPack);
        else
        {
          splashInfoPack = null;
          isCompleted = _AsyncResult2.IsCompleted;
        }

        if (isCompleted)
        {
          // Освобождаем ссылку до вызова базового метода
          IAsyncResult AsyncResult3 = _AsyncResult2;
          ExecProc ExecProc3 = _ExecProc2;
          _AsyncResult2 = null;
          _ExecProc2 = null;

          NamedValues Results = ExecProc3.EndExecute(AsyncResult3);
          return Results;
        }
        else
          return null;
      }

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override void InternalExecuteNoResults(NamedValues args)
      {
        ExecProc.AutoDispose = true;
        ExecProc.ExecuteNoResults(args);
      }

      /// <summary>
      /// Прерывает выполнение процедуры, если она использует процентный индикатор и установила AllowCancel=true.
      /// </summary>
      public override void Cancel()
      {
        _AsyncResult2spl.Cancel();
      }

      /// <summary>
      /// Этот метод не должен использоваться в прикладном коде
      /// </summary>
      public override void ResetSplashInfo()
      {
        _AsyncResult2spl.ResetSplashInfo();
      }

      #endregion

      #region Прочие методы

      private void CheckNotDisposed()
      {
        if (ExecProc == null)
          throw new ObjectDisposedException("ExecProc");
      }

#if XXX
    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public NamedValues InternalGetFixedInfo()
    {
      CheckNotDisposed();
      NamedValues FixedInfo = new NamedValues();
      FixedInfo["State"] = State; // 19.08.2020
      FixedInfo["Guid"] = _ExecProc.Guid;
      FixedInfo["DisplayName"] = _ExecProc.DisplayName;
      return FixedInfo;
    }
#endif

      private Guid _Guid;

      /// <summary>
      /// Извлекает информацию о процедуре, если она еще не была удалена.
      /// Для отсоединенной процедуры возвращается только свойство Guid
      /// </summary>
      /// <returns></returns>
      public override ExecProcInfo GetInfo()
      {
        if (ExecProc == null)
        {
          ExecProcInfo Info = new ExecProcInfo();
          Info.Guid = _Guid;
          Info.State = ExecProcState.Disposed;
          return Info;
        }
        else
          return ExecProc.GetInfo();
      }

      /// <summary>
      /// Вызывает для процедуры метод Dispose(), если она еще не была отсоединена.
      /// В нормальных условиях этот метод не должен вызываться. Процедура разрушается вызовом
      /// ExecProc.Dispose() (при локальной работе) или RemoteExecProc.Dispose() (при удаленном вызове)
      /// </summary>
      public override void Dispose()
      {
        if (ExecProc != null)
          ExecProc.Dispose();
      }

      /// <summary>
      /// Строковое представление "Guid (State)"
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return _Guid.ToString() + " (" + State.ToString() + ")";
      }

      /// <summary>
      /// Метод интерфейса не должен вызываться из пользовательского кода.
      /// </summary>
      /// <returns></returns>
      public override IServerSplashWatcher CreateSplashWatcher()
      {
        return ExecProc.CreateSplashWatcher();
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Переходник второго уровня для поддержки удаленных вызовов.
  /// Используется, если выполняемая процедура ExecProc на сервере создается не в основном домене приложения,
  /// а в дополнительном модуле. Ссылка на процедуру должна быть передана клиенту по сети. Нельзя передать
  /// ExecProcProxy клиенту напрямую, т.к. запрос клиента в дополнительный домен сервера завершится
  /// RemotingException.
  /// Сервер, получив ExecProcProxy из дополнительного домена, оборачивает его в ExecProcProxy2,
  /// который и передает клиенту.
  /// </summary>
  [Serializable]
  public sealed class ExecProcProxy2 : ExecProcProxy
  {
    #region Защищенный конструктор

    /// <summary>
    /// Создает переходник второго уровня на основании базового объекта
    /// </summary>
    /// <param name="proxy">Объект ExecProcProxy</param>
    public ExecProcProxy2(ExecProcProxy proxy)
      : base(proxy.Guid, proxy.DisplayName, proxy.AutoDispose, proxy.SyncTime, new InternalHandler2(proxy.Guid, proxy.Handler))
    {
      //if (proxy == null)
      //  throw new ArgumentNullException("proxy");
    }

    #endregion

    #region InternalHandler2

    internal sealed class InternalHandler2 : InternalHandler
    {
      #region Защищенный конструктор

      internal InternalHandler2(Guid guid, InternalHandler source)
      {
        _Guid = guid;
        if (source == null)
          throw new ArgumentNullException("source");
        _Source = source;
      }

      #endregion

      #region Управление временем жизни

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override bool WantsRenewal
      {
        get
        {
          if (_Source == null)
            return false;
          else
            return true; // дождемся обращения к свойству State
        }
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Ссылка на процедуру-владелец.
      /// Устанавливается в null, когда объект-владелец переходит в состояние Disposed.
      /// Так как переход происходит только в потоке вызова (синхронно), то обращение выполняется без специальной обработки
      /// </summary>
      private ExecProcProxy.InternalHandler _Source;

      /// <summary>
      /// Возвращает true, если серверная процедура была удалена.
      /// </summary>
      public override ExecProcState State
      {
        get
        {
          if (_Source == null)
            return ExecProcState.Disposed;

          ExecProcState st = _Source.State;
          if (st == ExecProcState.Disposed)
            _Source = null;
          return st;
        }
      }

      #endregion

      #region Выполнение

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override NamedValues InternalExecute(NamedValues args, bool autoDispose)
      {
        CheckNotDisposed();
        NamedValues Results = _Source.InternalExecute(args, autoDispose);
        if (autoDispose)
          _Source = null;
        return Results;
      }

      private bool _SavedAutoDispose;

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override NamedValues InternalBeginExecute(NamedValues args, bool autoDispose, int syncTime, bool useSplashWatcher)
      {
        CheckNotDisposed();
        _SavedAutoDispose = autoDispose;
        NamedValues res;
        try
        {
          res = _Source.InternalBeginExecute(args, autoDispose, syncTime, useSplashWatcher);
        }
        catch
        {
          if (_SavedAutoDispose)
            _Source = null;
          throw;
        }
        return res;
      }

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override NamedValues InternalTryEndExecute(bool splashInfoNeeded, out SplashInfoPack splashInfoPack)
      {
        if (_Source == null)
          throw new NullReferenceException("Source==null");
        else
        {
          try
          {
            NamedValues results = _Source.InternalTryEndExecute(splashInfoNeeded, out splashInfoPack);
            if (results == null)
              return null;

            if (_SavedAutoDispose)
              _Source = null;
            return results;
          }
          catch
          {
            if (_SavedAutoDispose)
              _Source = null;
            throw;
          }
        }
      }

      /// <summary>
      /// Не должно использоваться в пользовательском коде
      /// </summary>
      public override void InternalExecuteNoResults(NamedValues args)
      {
        CheckNotDisposed();
        //InternalExecuteNoResults(args);
        _Source.InternalExecuteNoResults(args); // 27.12.2020
        _Source = null;
      }

      /// <summary>
      /// Создает объект для отслеживания стека заставок.
      /// Этот метод не должен использоваться в прикладном коде
      /// </summary>
      /// <returns>Ссылка на новый объект ServerSplashWatcher</returns>
      public override IServerSplashWatcher CreateSplashWatcher()
      {
        return _Source.CreateSplashWatcher();
      }

      #endregion

      #region Прочие методы

      private void CheckNotDisposed()
      {
        if (_Source == null)
          throw new ObjectDisposedException("Proxy");
      }

      private Guid _Guid;

      /// <summary>
      /// Извлекает информацию о процедуре, если она еще не была удалена.
      /// Для отсоединенной процедуры возвращается только свойство Guid
      /// </summary>
      /// <returns></returns>
      public override ExecProcInfo GetInfo()
      {
        if (_Source == null)
        {
          ExecProcInfo Info = new ExecProcInfo();
          Info.Guid = _Guid;
          Info.State = ExecProcState.Disposed;
          return Info;
        }
        else
          return _Source.GetInfo();
      }

      /// <summary>
      /// Вызывает для процедуры метод Dispose(), если она еще не была отсоединена.
      /// В нормальных условиях этот метод не должен вызываться. Процедура разрушается вызовом
      /// ExecProc.Dispose() (при локальной работе) или RemoteExecProc.Dispose() (при удаленном вызове)
      /// </summary>
      public override void Dispose()
      {
        if (_Source != null)
        {
          _Source.Dispose();
          _Source = null;
        }
      }

      /// <summary>
      /// Строковое представление в виде "Guid (State)"
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return _Guid.ToString() + " (" + State.ToString() + ")";
      }


      /// <summary>
      /// Метод не должен вызываться из пользовательского кода.
      /// </summary>
      /// <returns></returns>
      public override void Cancel()
      {
        _Source.Cancel();
      }

      /// <summary>
      /// Метод не должен вызываться из пользовательского кода.
      /// </summary>
      /// <returns></returns>
      public override void ResetSplashInfo()
      {
        _Source.ResetSplashInfo();
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Исключение вызывается при попытке выполнения ExecProc.Execute() или ExecuteAsync(),
  /// когда предыдушее выполнение еще не завершено
  /// </summary>
  [Serializable]
  public class AlreadyExecutingException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект исключения с текстом сообщения по умолчанию
    /// </summary>
    /// <param name="execProc">Ссылка на ExecProc или RemoteExecProc</param>
    public AlreadyExecutingException(IExecProc execProc)
      : base("Вложенный запуск процедуры на выполнение")
    {
      if (execProc != null)
        _ExecProcInfo = execProc.GetInfo();
    }


    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected AlreadyExecutingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _ExecProcInfo = (ExecProcInfo)(info.GetValue("ExecProcInfo", typeof(ExecProcInfo)));
    }

    /// <summary>
    /// используется для сериализации
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("ExecProcInfo", _ExecProcInfo);
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Информация о процедуре, которую попытались выполнить еще раз
    /// </summary>
    public ExecProcInfo ExecProcInfo { get { return _ExecProcInfo; } }
    private ExecProcInfo _ExecProcInfo;

    #endregion
  }

  /// <summary>
  /// Информация об объекте ExecProc
  /// В отличие от ExecProc, данный класс является сериализуемым.
  /// Для получения информации о процедуре используйте метод IExecProc.GetInfo()
  /// </summary>
  [Serializable]
  public sealed class ExecProcInfo
  {
    #region Защищенный конструктор

    internal ExecProcInfo()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Время, когда вызван GetInfo()
    /// </summary>
    public DateTime InfoTime { get { return _InfoTime; } internal set { _InfoTime = value; } }
    private DateTime _InfoTime;

    /// <summary>
    /// Идентификатор объекта процедуры
    /// </summary>
    public Guid Guid { get { return _Guid; } internal set { _Guid = value; } }
    private Guid _Guid;

    /// <summary>
    /// Время создания объекта ExecProc
    /// </summary>
    public DateTime CreationTime { get { return _CreationTime; } internal set { _CreationTime = value; } }
    private DateTime _CreationTime;

    /// <summary>
    /// Время существования процедуры (для отладки)
    /// </summary>
    public TimeSpan LifeTime { get { return _InfoTime - _CreationTime; } }

    /// <summary>
    /// Имя сборки, в которой определен класс, производный от ExecProc
    /// </summary>
    public string AssemblyName { get { return _AssemblyName; } internal set { _AssemblyName = value; } }
    private string _AssemblyName;

    /// <summary>
    /// Имя класса, производного от ExecProc
    /// </summary>
    public string TypeName { get { return _TypeName; } internal set { _TypeName = value; } }
    private string _TypeName;

    /// <summary>
    /// Возвращает true, если текущий объект ExecProcInfo описывает процедуру, тип которой задан как <paramref name="t"/>.
    /// Используются свойства AssemblyName и TypeName.
    /// </summary>
    /// <param name="t">Тип класса, производного от ExecProc</param>
    /// <returns>Совпадение типа</returns>
    public bool IsExecProcType(Type t)
    {
      if (t == null)
        return false;
      return String.Equals(this.TypeName, t.FullName, StringComparison.Ordinal) &&
        String.Equals(this.AssemblyName, t.Assembly.FullName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Отображаемое имя процедуры
    /// </summary>
    public string DisplayName { get { return _DisplayName; } internal set { _DisplayName = value; } }
    private string _DisplayName;

    /// <summary>
    /// Выполняемое действие
    /// </summary>
    public string ActionName { get { return _ActionName; } internal set { _ActionName = value; } }
    private string _ActionName;


    /// <summary>
    /// Если true, то процедура ExecProc является "саморазрушаемой". 
    /// </summary>
    public bool AutoDispose { get { return _AutoDispose; } internal set { _AutoDispose = value; } }
    private bool _AutoDispose;

    /// <summary>
    /// Интервал времени в миллисекундах, в течение которого вызов BeginExecute() будет ожидать завершения потока.
    /// Нулевое значение означает отсутствие ожидания.
    /// </summary>
    public int SyncTime { get { return _SyncTime; } internal set { _SyncTime = value; } }
    private int _SyncTime;

    /// <summary>
    /// Состояние выполнения процедуры
    /// </summary>
    public ExecProcState State { get { return _State; } internal set { _State = value; } }
    private ExecProcState _State;

    /// <summary>
    /// Возвращает true для State=Queued и Execiting
    /// </summary>
    public bool IsExecuting { get { return ExecProc.GetIsExecuting(State); } }

    /// <summary>
    /// Состояние выполнения потока.
    /// Если в данный момент процедура не выполняется (IsExecuting=false), то свойство возвращает ThreadState.Unstarted
    /// </summary>
    public System.Threading.ThreadState ThreadState { get { return _ThreadState; } internal set { _ThreadState = value; } }
    private System.Threading.ThreadState _ThreadState;

    /// <summary>
    /// Свойство Thread.ManagedThreadId, если процедура в данный момент выполняется.
    /// Если информация о потоке недоступна, свойство возвращает 0
    /// </summary>
    public int ManagedThreadId { get { return _ManagedThreadId; } internal set { _ManagedThreadId = value; } }
    private int _ManagedThreadId;

    /// <summary>
    /// Возвращает true, если последний запуск был выполнен в асинхронном режиме
    /// </summary>
    public bool IsAsyncStarted { get { return _IsAsyncStarted; } internal set { _IsAsyncStarted = value; } }
    private bool _IsAsyncStarted;

    /// <summary>
    /// Количество запусков
    /// </summary>
    public int StartCount { get { return _StartCount; } internal set { _StartCount = value; } }
    private int _StartCount;

    /// <summary>
    /// Время последнего запуска процедуры.
    /// Свойство имеет смысл, если StartCount больше 0
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } internal set { _StartTime = value; } }
    private DateTime _StartTime;

    /// <summary>
    /// Время работы, если процедура сейчас выполняется (для отладки).
    /// Иначе возвращает TimeSpan.Zero
    /// </summary>
    public TimeSpan ExecutingTime
    {
      get
      {
        if (IsExecuting)
          return InfoTime - StartTime;
        else
          return TimeSpan.Zero;
      }
    }

    /// <summary>
    /// Идентификатор родительской процедуры ExecProc, выполнившей запуск этой процедуры в синхронном
    /// или асинхронном режиме
    /// </summary>
    public Guid ParentProcGuid { get { return _ParentProcGuid; } internal set { _ParentProcGuid = value; } }
    private Guid _ParentProcGuid;

    /// <summary>
    /// Идентификатор дочерней процедуры, которая была запущена этой процедурой в синхронном режиме,
    /// и окончание которой ожидается
    /// </summary>
    public Guid ChildProcGuid { get { return _ChildProcGuid; } internal set { _ChildProcGuid = value; } }
    private Guid _ChildProcGuid;

    /// <summary>
    /// Идентификатор объекта ExecProcCallBack, для которого был вызван Execute() из данной процедуры,
    /// и ожидание ответа для которого ожидается
    /// </summary>
    public Guid ChildProcCallBackGuid { get { return _ChildProcCallBackGuid; } internal set { _ChildProcCallBackGuid = value; } }
    private Guid _ChildProcCallBackGuid;

    /// <summary>
    /// Имя класса исключения, если при выполнении процедуры возникла ошибка
    /// </summary>
    public string ExceptionType { get { return _ExceptionType; } internal set { _ExceptionType = value; } }
    private string _ExceptionType;

    /// <summary>
    /// Текст сообщения об ошибке, если при последнем выполнении процедуры возникло исключение
    /// Можно было бы вернуть весь объект Exception, но он может быть очень толстым за счет связанных данных
    /// </summary>
    public string ExceptionMessage { get { return _ExceptionMessage; } internal set { _ExceptionMessage = value; } }
    private string _ExceptionMessage;

    /// <summary>
    /// Стек вызова, который привел к возникновению ошибки или, если процедура сейчас выполняется, то текущий стек вызовов
    /// </summary>
    public string StackTrace { get { return _StackTrace; } internal set { _StackTrace = value; } }
    private string _StackTrace;

    /// <summary>
    /// Аргументы запуска процедуры, если выполнение завершилось с ошибкой.
    /// Аргументы "упрощены" и могут применяться только в отладочных целях.
    /// Упрощение означает замену всех типов, кроме примитивных, на их строковое представление
    /// </summary>
    public NamedValues ExceptionDebugArgs { get { return _ExceptionDebugArgs; } internal set { _ExceptionDebugArgs = value; } }
    private NamedValues _ExceptionDebugArgs;

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return TypeName + " " + Guid.ToString() + ", State=" + State.ToString();
    }

    #endregion

    #region Информация о месте выполнения процедуры (AppDomain)

    /// <summary>
    /// Имя компьютера, на котором выполняется приложение (сервера), в котором создана процедура ExecProc
    /// </summary>
    public string MachineName { get { return _MachineName; } internal set { _MachineName = value; } }
    private string _MachineName;

    /// <summary>
    /// Имя процесса (свойство Process.ProcessName), в котором выполняется ExecProc
    /// </summary>
    public string ProcessName { get { return _ProcessName; } internal set { _ProcessName = value; } }
    private string _ProcessName;

    /// <summary>
    /// Идентификатор (свойство Process.Id), в котором выполняется ExecProc
    /// </summary>
    public int ProcessId { get { return _ProcessId; } internal set { _ProcessId = value; } }
    private int _ProcessId;

    /// <summary>
    /// Имя домена (свойство AppDomain.FriendlyName), в котором выполняется ExecProc
    /// </summary>
    public string AppDomainName { get { return _AppDomainName; } internal set { _AppDomainName = value; } }
    private string _AppDomainName;

    /// <summary>
    /// Инициализация полей текущего процесса и домена
    /// </summary>
    internal void InitDomainInfo()
    {
      try
      {
        MachineName = Environment.MachineName;
        Process prc = Process.GetCurrentProcess();
        ProcessName = prc.ProcessName;
        ProcessId = prc.Id;
        AppDomainName = AppDomain.CurrentDomain.FriendlyName;
      }
      catch { }
    }

    #endregion
  }

  /// <summary>
  /// Переходник для вызова процедур посредством Remoting.
  /// Создается на стороне клиента, используя в качестве аргумента ExecProcProxy, полученный от сервера
  /// Объект не является сериализуемым.
  /// </summary>
  public class RemoteExecProc : SimpleDisposableObject, IExecProc
  {
    // 02.01.2021
    // Можно использовать SimpleDisposableObject в качестве базового класса.
    // Единственное действие, выполняемое в деструкторе (в Dispose(false)) - удаление из списка AllProcs.
    // Но этот список хранит слабые ссылки, которые самоочищаются, если процедура разрушена

    #region Конструкторы и Dispose

    /// <summary>
    /// Создает объект удаленной процедуры для заданного прокси.
    /// </summary>
    /// <param name="proxy">Прокси</param>
    public RemoteExecProc(ExecProcProxy proxy)
    {
      if (proxy == null)
        throw new ArgumentNullException("proxy");

      _Proxy = proxy;

      //if (proxy.State == ExecProcState.Disposed)
      //ExecProcState proxyState = (ExecProcState)(FixedInfo["State"]); // 19.08.2020
      //if (proxyState == ExecProcState.Disposed)
      //  throw new ObjectDisposedException("proxy", "Ссылка на ExecProc является недействительной, так как процедура на сервере уже была разрушена");
      _Guid = proxy.Guid;
      _DisplayName = proxy.DisplayName;
      _AutoDispose = proxy.AutoDispose;
      _SyncTime = proxy.SyncTime;

      _AllProcs.Add(this);
    }

    /// <summary>
    /// Вызывается при разрушении RemoteExecProc
    /// </summary>
    /// <param name="disposing">true, если вызван Dispose(), false, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Proxy != null)
          _Proxy.Handler.Dispose();
      }
      base.Dispose(disposing);
      _AllProcs.Remove(this);

      if (disposing && _DependentDisposableObjects != null)
      {
        for (int i = 0; i < _DependentDisposableObjects.Count; i++)
          _DependentDisposableObjects[i].Dispose();
      }

      _DependentDisposableObjects = null;
    }

    #endregion

    #region Фиксированные свойства

    /// <summary>
    /// Объект, реализующий удаленное взаимодействие.
    /// Может принять значение null.
    /// </summary>
    private /*readonly */ ExecProcProxy _Proxy;

    private ExecProcProxy GetProxyWithCheck()
    {
      ExecProcProxy proxy = _Proxy;
      if (proxy == null)
        throw new NullReferenceException("Объект Proxy был отсоединен");
      return proxy;
    }

    /// <summary>
    /// Возвращает уникальный идентификатор объекта ExecProc
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private readonly Guid _Guid;

    /// <summary>
    /// Отображаемое имя процедуры
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    #endregion

    #region Выполнение

    /// <summary>
    /// Если true, то процедура ExecProc является "саморазрушаемой". 
    /// После вызова Execute() или EndExecute() выполняется вызов ExecProc.Dispose().
    /// Свойство можно устанавливать в любое время, пока процедура не выполняется.
    /// По умолчанию значение свойства совпадает со свойством в ExecProc (обычно true),
    /// но может быть переопределено.
    /// </summary>
    public bool AutoDispose
    {
      get { return _AutoDispose; }
      set { _AutoDispose = value; }
    }
    private bool _AutoDispose;

    /// <summary>
    /// Интервал времени в миллисекундах, в течение которого вызов BeginExecute() будет ожидать завершения потока.
    /// Нулевое значение означает отсутствие ожидания.
    /// Свойство можно устанавливать в любое время, пока процедура не выполняется.
    /// По умолчанию значение свойства совпадает со свойством в ExecProc (обычно ExecProc.DefaultSyncTime),
    /// но может быть переопределено.
    /// </summary>
    public int SyncTime
    {
      get { return _SyncTime; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _SyncTime = value;
      }
    }
    private int _SyncTime;

    /// <summary>
    /// Синхронное выполнение удаленной процедуры.
    /// В случае возникновения ошибки выполнения или связи с сервером, возникает исключение.
    /// Так как обычно существует тайм-аут на удаленный вызов процедур через Remoting, 
    /// синхронный вызов следует использовать только для быстро выполняющихся процедур.
    /// </summary>
    /// <param name="args">Аргументы, передаваемые процедуры</param>
    /// <returns>Результаты выполнения.</returns>
    //[DebuggerStepThrough]
    public NamedValues Execute(NamedValues args)
    {
      CheckNotDisposed();
#if DEBUG
      if (args != null)
        args.CheckIfMarshallable();
#endif

      _ExecutingProcs.Add(this);

      NamedValues Results;

      try
      {
        ExecProcProxy proxy = GetProxyWithCheck();

        try
        {
          Results = proxy.Handler.InternalExecute(args, AutoDispose);
          if (AutoDispose)
          {
            _Proxy = null;
            this.Dispose();
          }
        }
        catch//(Exception e)
        {
          //e.Data["RemoteExecProc.Execute() stack trace"] = e.StackTrace;

          if (proxy.Handler.State == ExecProcState.Disposed)
            this.Dispose();
          throw;
        }
      }
      finally
      {
        _ExecutingProcs.Remove(this);
      }

      return Results;
    }

    /// <summary>
    /// Возвращает текущее состояние выполнения
    /// </summary>
    public ExecProcState State
    {
      get
      {
        if (IsDisposed)
          return ExecProcState.Disposed;
        ExecProcProxy proxy = _Proxy;
        if (proxy == null)
          return ExecProcState.Disposed;
        else
          return proxy.Handler.State;
      }
    }

    /// <summary>
    /// Возвращает true для State=NotStarted, Success и Error
    /// </summary>
    public bool CanStart { get { return ExecProc.GetCanStart(State); } }

    /// <summary>
    /// Возвращает true для State=Queued и Execiting
    /// </summary>
    public bool IsExecuting { get { return ExecProc.GetIsExecuting(State); } }


    /// <summary>
    /// Нельзя использовать основной объект ExecProc.ExecProcAsyncResult
    /// </summary>
    private class RemoteExecProcAsyncResult : IAsyncResultWithSplash
    {
      #region Поля

      internal RemoteExecProc Owner;

      /// <summary>
      /// Свойство устанавливается в true после вызова EndExecute()
      /// </summary>
      internal bool EndExecuteCalled;

      internal AsyncCallback UserCallback;

      /// <summary>
      /// Если InternalBeginExecute() успешно завершился с досрочным возвратом результатов
      /// </summary>
      internal NamedValues Results;

      /// <summary>
      /// Если InternalBeginExecute() завершился аварийно
      /// </summary>
      internal Exception Exception;

      #endregion

      #region IAsyncResult Members

      public object AsyncState { get { return _AsyncState; } }
      internal object _AsyncState;

      public WaitHandle AsyncWaitHandle
      {
        get
        {
          lock (this)
          {
            if (_AsyncWaitHandle == null)
            {
              if (!EndExecuteCalled)
                _AsyncWaitHandle = new ManualResetEvent(_IsCompleted);
            }
            return _AsyncWaitHandle;
          }
        }
      }
      /// <summary>
      /// На время обращения к полю должна выполняться блокировка this
      /// </summary>
      internal ManualResetEvent _AsyncWaitHandle;

      public bool CompletedSynchronously
      {
        get { return _CompletedSynchronously; }
        internal set { _CompletedSynchronously = value; }
      }
      private bool _CompletedSynchronously;

      public bool IsCompleted { get { return _IsCompleted; } internal set { _IsCompleted = value; } }
      /// <summary>
      /// Устанавливается асинхронно
      /// </summary>
      internal volatile bool _IsCompleted;

      internal bool UseSplashWatcher;

      internal SplashInfoPack _SplashInfoPack;

      #endregion

      #region IAsyncResultWithSplash Members

      public bool GetIsCompleted(out SplashInfoPack splashInfoPack)
      {
        splashInfoPack = _SplashInfoPack;
#if DEBUG_SPLASHWATCHERS
        if (splashInfoPack == null) // так может быть, если сигнал таймера еще не приходил
          // throw new NullReferenceException("Поле _SplashInfoPack=null");
          splashInfoPack = new SplashInfoPackNone();
#endif
        _SplashInfoPack = null; // очищаем внутреннее поле, иначе больше не получим нового состояния с сервера

        if (Owner.IsDisposed)
          return true; // 12.12.2019
        else
          return _IsCompleted;
      }

      public void Cancel()
      {
        ExecProcProxy proxy = Owner._Proxy;
        if (proxy != null)
          proxy.Handler.Cancel();
      }

      public void ResetSplashInfo()
      {
        ExecProcProxy proxy = Owner._Proxy;
        if (proxy != null)
          proxy.Handler.ResetSplashInfo();
      }

      #region Обработка завершена

      internal void Finish()
      {
        ManualResetEvent AsyncWaitHandle2;
        lock (this)
        {
          _IsCompleted = true;
          // не нужно очищать! Еще будет вызов GetUsCompleted() _SplashInfoPack = null;
          // должно быть установлено до обращения к FAsyncWaitHandle,
          // т.к. объект WaintHandle может между lock и вызовом Reset
          AsyncWaitHandle2 = _AsyncWaitHandle;
        }

        // Это нельзя делать внутри блокировки
        if (AsyncWaitHandle2 != null)
          AsyncWaitHandle2.Set();

        if (UserCallback != null)
          UserCallback(this);
      }

      #endregion

      #endregion

      #region Увеличение интервала при длительном ожидании

      /// <summary>
      /// Счетчик вызовов
      /// </summary>
      private int _Counter;

      /// <summary>
      /// Сначала попытка опроса сервера выполняется при каждом сигнале таймера (4 раза в секунду).
      /// Затем уменьшаем количество запросов, пропуская часть попыток
      /// </summary>
      /// <returns></returns>
      internal bool WantToTry()
      {
        _Counter++;
        if (_Counter > 10000)
          _Counter = 1000;

        if (_Counter <= 4)
          return true; // первую секунду часто
        if (_Counter <= 12)
          return (_Counter % 2) == 0; // потом 2 секунды через раз
        else if (_Counter < 60)
          return (_Counter % 4) == 0; // потом 1 раз в секунду (до 15 секунд)
        else if (_Counter < 240)
          return (_Counter % 8) == 0; // а потом 1 раз в 2 секунды (до 1 минуты)
        else
          return (_Counter % 40) == 0; // а потом - 1 раз в 10 секунд
      }

      #endregion
    }

    #region Опрос процедур по таймеру

    private static readonly SyncCollection<RemoteExecProcAsyncResult> _WaitAsyncResults = new SyncCollection<RemoteExecProcAsyncResult>();

    private static Timer _AsyncTimer = null;

    /// <summary>
    /// Предотвращение вложенного вызова
    /// </summary>
    private static bool _InsideAsyncTimerTick = false;

    private static void AsyncTimerTick(object state)
    {
      if (_InsideAsyncTimerTick)
        return;
      _InsideAsyncTimerTick = true;
      try
      {
        try
        {
          DoAsyncTimerTick();
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка опроса RemoteExecProc по таймеру");
        }
      }
      finally
      {
        _InsideAsyncTimerTick = false;
      }
    }

    private static void DoAsyncTimerTick()
    {
      RemoteExecProcAsyncResult[] ars = _WaitAsyncResults.ToArray();
      for (int i = 0; i < ars.Length; i++)
      {
        try
        {
          if (ars[i].WantToTry())
          {
            DoAsyncTimerTick2(ars[i]);

            if (ars[i]._IsCompleted)
            {
              ars[i].Finish();
              _WaitAsyncResults.Remove(ars[i]);
            }
          }
        }
        catch (Exception e)
        {
          e.Data["RemoteExecProc.Guid"] = ars[i].Owner.Guid;
          e.Data["RemoteExecProc.DisplayName"] = ars[i].Owner.DisplayName;
          try
          {
            e.Data["RemoteExecProc.GetInfo"] = ars[i].Owner.GetInfo();
          }
          catch (Exception e2)
          {
            e.Data["RemoteExecProc.GetInfo"] = "Исключение " + e2.GetType().ToString() + ". " + e2.Message;
          }
          LogoutTools.LogoutException(e, "Ошибка опроса RemoteExecProc по таймеру");

          // Убираем, чтобы ошибка не повторялась
          ars[i]._IsCompleted = true;
          ars[i]._SplashInfoPack = null;
          _WaitAsyncResults.Remove(ars[i]);
        }
      }

      lock (_WaitAsyncResults)
      {
        if (_WaitAsyncResults.Count == 0)
        {
          // Прекращаем обработку по таймеру
          if (_AsyncTimer != null)
          {
            _AsyncTimer.Dispose();
            _AsyncTimer = null;
          }
        }
      }
    }

    private static void DoAsyncTimerTick2(RemoteExecProcAsyncResult ar)
    {
      bool splashInfoNeeded = ar.UseSplashWatcher;
      if (splashInfoNeeded && ar._SplashInfoPack != null)
      {
        splashInfoNeeded = false; // иначе будет переполнение
#if DEBUG_SPLASHWATCHERS
            Trace.WriteLine("Предотвращено переполнение очереди заставки в DoAsyncTimerTick.RemoteExecProc()");
#endif
      }

      if (ar.Owner.IsDisposed)
      {
        ar._IsCompleted = true;
        ar._SplashInfoPack = null;
      }
      else
      {
        SplashInfoPack splashInfoPack;
        //Type tt = ar.Owner._Proxy.GetType();
        try
        {
          ar.Results = ar.Owner._Proxy.Handler.InternalTryEndExecute(splashInfoNeeded, out splashInfoPack);
          if (ar.Results != null)
            ar.IsCompleted = true;
        }
        catch (Exception e)
        {
          ar.Exception = e;
          ar.IsCompleted = true;
          splashInfoPack = null;
        }

        if (splashInfoNeeded)
          ar._SplashInfoPack = splashInfoPack;
      }

    }

    #endregion

    /// <summary>
    /// Начинает асинхронное выполнение удаленной процедуры в стиле APM.
    /// Должен быть вызван EndExecute()
    /// </summary>
    /// <param name="args">Аргументы, передаваемые процедуре</param>
    /// <param name="userCallback">Пользовательский метод, вызываемый при завершении процедуры</param>
    /// <param name="stateObject">Пользовательский объект, помещаемый в IAsyncResult.AsyncState</param>
    /// <returns>Интерфейс IAsyncResult</returns>
    public IAsyncResult BeginExecute(NamedValues args, AsyncCallback userCallback, object stateObject)
    {
      return DoBeginExecute(true, args, userCallback, stateObject, false);
    }

    /// <summary>
    /// Начинает асинхронное выполнение удаленной процедуры в стиле APM.
    /// Возвращает расширенный интерфейс, который реализует метод GetIsCompleted
    /// Должен быть вызван EndExecute().
    /// </summary>
    /// <param name="args">Аргументы, передаваемые процедуре</param>
    /// <param name="userCallback">Пользовательский метод, вызываемый при завершении процедуры</param>
    /// <param name="stateObject">Пользовательский объект, помещаемый в IAsyncResult.AsyncState</param>
    /// <returns>Интерфейс IAsyncResult</returns>
    public IAsyncResultWithSplash BeginExecuteWithSplash(NamedValues args, AsyncCallback userCallback, object stateObject)
    {
      return DoBeginExecute(true, args, userCallback, stateObject, true);
    }

    /// <summary>
    /// Начинает асинхронное выполнение удаленной процедуры для RemoteSingleCallExecProc
    /// </summary>
    internal IAsyncResult InternalNoBeginExecute(AsyncCallback userCallback, object stateObject)
    {
      return DoBeginExecute(false, null, userCallback, stateObject, false);
    }

    /// <summary>
    /// Начинает асинхронное выполнение удаленной процедуры для RemoteSingleCallExecProc
    /// </summary>
    internal IAsyncResultWithSplash InternalNoBeginExecuteWithSplash(AsyncCallback userCallback, object stateObject)
    {
      return DoBeginExecute(false, null, userCallback, stateObject, true);
    }


    private RemoteExecProcAsyncResult DoBeginExecute(bool isBeginExecute, NamedValues args, AsyncCallback userCallback, object stateObject, bool useSplashWatcher)
    {
#if DEBUG
      if (args != null)
        args.CheckIfMarshallable();
#endif

      ExecProcProxy proxy = GetProxyWithCheck();
      RemoteExecProcAsyncResult ar = new RemoteExecProcAsyncResult();
      ar.Owner = this;
      ar.UseSplashWatcher = useSplashWatcher;

      if (isBeginExecute)
      {
        try
        {
          ar.Results = proxy.Handler.InternalBeginExecute(args, AutoDispose, SyncTime, useSplashWatcher);
        }
        catch (Exception e)
        {
          ar.Exception = e;
        }
      }

      if (ar.Results == null && ar.Exception == null)
      {
        // ставим в очередь на выполнение

        ar.UserCallback = userCallback;
        ar._AsyncState = stateObject;
        lock (_WaitAsyncResults)
        {
          _WaitAsyncResults.Add(ar);
          if (_AsyncTimer == null)
            _AsyncTimer = new Timer(AsyncTimerTick, null, 250, 250);
        }

        _ExecutingProcs.Add(this);
      }
      else
      {
        // Досрочное завершение процедуры
        ar.IsCompleted = true;
        ar.CompletedSynchronously = true;
      }

      return ar;
    }

    /// <summary>
    /// Заканчивает асихронное выполнение, начатое BeginExecute().
    /// Если во время выполнения произошла ошибка, исключение будет выброшено сейчас.
    /// </summary>
    /// <param name="asyncResult">Объект, полученный от BeginExecute()</param>
    /// <returns>Значение, возвращенные ExecProc</returns>
    [DebuggerStepThrough]
    public NamedValues EndExecute(IAsyncResult asyncResult)
    {
      RemoteExecProcAsyncResult ar = (RemoteExecProcAsyncResult)asyncResult;
      if (ar == null)
        throw new ArgumentNullException("asyncResult");
      if (!Object.ReferenceEquals(ar.Owner, this))
        throw new ArgumentException("Объект AsyncResult относится к другой процедуре", "asyncResult");

      if (!ar.IsCompleted)
      {
        if (ar.AsyncWaitHandle == null)
          throw new NullReferenceException("AsyncWaitHandle=null");
        ar.AsyncWaitHandle.WaitOne(); // 02.05.2017
        if (!ar.IsCompleted)
          throw new BugException("После ожидания AsyncWaitHandler не установлено состояние IsCompleted");
      }

      _ExecutingProcs.Remove(this); // Процедуры может и не быть в списке, но это не важно

      if (ar.EndExecuteCalled)
        throw new InvalidOperationException("Повторный вызов EndExecute()");

      lock (ar)
      {
        ar.EndExecuteCalled = true;
        if (ar._AsyncWaitHandle != null)
        {
          ((IDisposable)(ar._AsyncWaitHandle)).Dispose();
          ar._AsyncWaitHandle = null;
        }
      }

      if (AutoDispose)
      {
        _Proxy = null;
        Dispose();
      }

      if (ar.Results != null)
        return ar.Results;
      else if (ar.Exception != null)
        throw ar.Exception;
      else
        throw new BugException("Процедура считается завершенной, но нет ни результатов, ни объекта исключения");
    }

    /// <summary>
    /// Запускает процедуру на асинхронное выполнение.
    /// После вызова, объект немедленно уничтожается и управление возвращается.
    /// Связанная процедура ExecProc уничтожается по окончании выполнения.
    /// Узнать результаты выполнения или получить сигнал об ошибке нельзя.
    /// </summary>
    /// <param name="args"></param>
    public void ExecuteNoResults(NamedValues args)
    {
      CheckNotDisposed();

      ExecProcProxy proxy = GetProxyWithCheck();

      if (!AutoDispose)
        throw new InvalidOperationException("Свойство AutoDispose должно быть установлено в true");

      proxy.Handler.InternalExecuteNoResults(args);
      _Proxy = null; // чтобы сигнал Dispose не попал в ExecProc
      Dispose();
    }

    #endregion

    #region Информация о выполнении

    /// <summary>
    /// Возвращает всю информацию о текущей процедуре в виде объекта, поддерживающего сериализацию
    /// </summary>
    /// <returns>Информация о процедуре</returns>
    public ExecProcInfo GetInfo()
    {
      ExecProcProxy proxy = _Proxy;
      if (proxy == null)
      {
        ExecProcInfo info = new ExecProcInfo();
        info.Guid = _Guid;
        info.DisplayName = _DisplayName;
        return info;
      }
      else
      {
        ExecProcInfo info = proxy.Handler.GetInfo();
        info.AutoDispose = AutoDispose;
        info.SyncTime = SyncTime;
        return info;
      }
    }

    /// <summary>
    /// Для вывода отладочной информации
    /// </summary>
    public ExecProcInfo DebugInfo
    {
      get
      {
        return GetInfo();
      }
    }

    /// <summary>
    /// Текстовое представление в виде "DisplayName (Disposed)"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return DisplayName + (IsDisposed ? " (Disposed)" : "");
    }

    /// <summary>
    /// Создает объект для отслеживания стека заставок в процедуре ExecProc.
    /// Этот метод не должен использоваться в прикладном коде
    /// </summary>
    /// <returns>Ссылка на новый объект ServerSplashWatcher</returns>
    public IServerSplashWatcher CreateSplashWatcher()
    {
      ExecProcProxy proxy = GetProxyWithCheck();
      return proxy.Handler.CreateSplashWatcher();
    }

    #endregion

    #region Список связанных объектов

    /// <summary>
    /// Список связанных объектов, которые требуется удалить при вызове метода Dispose().
    /// Это свойство не является потокобезопасным. Предполагается, что в список добавляются объекты из контекста процедуры
    /// </summary>
    public IList<IDisposable> DependentDisposableObjects
    {
      get
      {
        if (_DependentDisposableObjects == null)
          _DependentDisposableObjects = new List<IDisposable>();
        return _DependentDisposableObjects;
      }
    }
    private List<IDisposable> _DependentDisposableObjects;

    #endregion

    #region Статические списки

    #region Все процедуры

    /// <summary>
    /// Возвращает список RemoteExecProc, созданных в текущем AppDomain, для которых не было вызова Dispose().
    /// </summary>
    /// <returns></returns>
    public static RemoteExecProc[] GetAllProcs()
    {
      return _AllProcs.ToArray();
    }

    private static WeakReferenceCollection<RemoteExecProc> _AllProcs = new WeakReferenceCollection<RemoteExecProc>(); // 02.01.2021

    /// <summary>
    /// Возвращает массив объектов ExecProcInfo для всех процедур RenoteExecProc, для которых не было вызова Dispose
    /// </summary>
    /// <returns>Массив описателей процедур</returns>
    public static ExecProcInfo[] GetAllProcInfos()
    {
      RemoteExecProc[] a1 = GetAllProcs();
      ExecProcInfo[] a2 = new ExecProcInfo[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].GetInfo();
      return a2;
    }

    /// <summary>
    /// Возвращает количество процедур, созданных в текущем AppDomain, для которых не было вызова Dispose().
    /// </summary>
    public static int AllProcCount
    {
      get
      {
        return _AllProcs.Count;
      }
    }

    /// <summary>
    /// Свойство возвращает true, если ссылка на текущий объект есть в статическом списке GetAllProcs().
    /// Свойство автоматически устанавливается в true в конструкторе и сбрасывается в false в Dispose().
    /// Если нельзя хранить ссылку на процедуру в общем списке, можно сбросить свойство в false в конструкторе производного класса.
    /// Не следует использовать эту возможность без особой необходимости
    /// </summary>
    public bool ContainsInAllProcs
    {
      get
      {
        return _AllProcs.Contains(this);
      }
      set
      {
        if (value)
        {
          if (!_AllProcs.Contains(this))
            _AllProcs.Add(this);
        }
        else
          _AllProcs.Remove(this);
      }
    }

    #endregion

    #region Выполняющиеся процедуры

    /// <summary>
    /// Возвращает список RemoteExecProc, выполняющихся в данный момент в текущем AppDomain.
    /// Так как процедуры могут запускаться и завершаться асинхронно, не гарантируется, что список
    /// содержит все процедуры, и все процедуры имеют состояние IsExecuting=true
    /// </summary>
    /// <returns></returns>
    public static RemoteExecProc[] GetExecutingProcs()
    {
      return _ExecutingProcs.ToArray();
    }

    private static readonly SyncCollection<RemoteExecProc> _ExecutingProcs = new SyncCollection<RemoteExecProc>();

    /// <summary>
    /// Возвращает количество процедур, выполняющихся в данный момент в текущем AppDomain.
    /// Так как процедуры могут запускаться и завершаться асинхронно, свойство может возвращать разные
    /// значения при последовательных вызовах
    /// </summary>
    public static int ExecutingProcCount
    {
      get
      {
        return _ExecutingProcs.Count;
      }
    }

    /// <summary>
    /// Возвращает массив объектов ExecProcInfo для всех выполняющихся процедур ExecProc
    /// </summary>
    /// <returns>Массив описателей процедур</returns>
    public static ExecProcInfo[] GetExecutingProcInfos()
    {
      RemoteExecProc[] a1 = GetExecutingProcs();
      ExecProcInfo[] a2 = new ExecProcInfo[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].GetInfo();
      return a2;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Заглушка реализации интерфейса IAsyncResult.
  /// Возвращает состояние "всегда готов" (IsCompleted=true, CompletedSynchronously=true)
  /// </summary>
  public sealed class DummyAsyncResult : MarshalByRefDisposableObject, IAsyncResultWithSplash
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Создает заглушку, которая будет хранить пользовательский объект AsyncState.
    /// </summary>
    /// <param name="userCallback">Если передано, то оно получит уведомление прямо в конструкторе</param>
    /// <param name="stateObject">Значение свойства AsyncState</param>
    public DummyAsyncResult(AsyncCallback userCallback, object stateObject)
    {
      _AsyncState = stateObject;

      if (userCallback != null)
        userCallback(this);
    }

    /// <summary>
    /// Удаляет объект синхронизации, если было обращение к свойству AsyncWaitHandle.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && _AsyncWaitHandle != null)
      {
        ((IDisposable)_AsyncWaitHandle).Dispose();
        _AsyncWaitHandle = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region IAsyncResult Members

    /// <summary>
    /// Вовзвращает объект, переданный конструктору
    /// </summary>
    public object AsyncState { get { return _AsyncState; } }
    private object _AsyncState;

    /// <summary>
    /// Возвращает объект сигнализации, который всегда находится в сигнальном состоянии
    /// </summary>
    public WaitHandle AsyncWaitHandle
    {
      get
      {
        if (_AsyncWaitHandle == null)
          _AsyncWaitHandle = new ManualResetEvent(true);
        return _AsyncWaitHandle;
      }
    }
    private ManualResetEvent _AsyncWaitHandle;

    /// <summary>
    /// Всегда возвращает true
    /// </summary>
    public bool CompletedSynchronously { get { return true; } }

    /// <summary>
    /// Всегда возвращает true
    /// </summary>
    public bool IsCompleted { get { return true; } }

    #endregion

    #region IAsyncResultWithSplash Members

    /// <summary>
    /// Всегда возвращает true
    /// </summary>
    /// <param name="splashInfoPack">Сюда записывается null</param>
    /// <returns>True</returns>
    public bool GetIsCompleted(out SplashInfoPack splashInfoPack)
    {
      splashInfoPack = null;
      return true;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void ResetSplashInfo()
    {
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Cancel()
    {
    }

    #endregion
  }

  /// <summary>
  /// Переходник для завершения выполнения на стороне клиента одноразовой процедуры, выполнение которой начато на стороне сервера
  /// в сценарии "разделенного" запроса.
  /// Для полученного объекта должны вызываться методы согласно модели асинхронных вызовов APM BeginExecute() и EndExecute().
  /// </summary>
  public sealed class RemoteDistributedProc : SimpleDisposableObject
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="startData">Данные вызова ExecProc.StartDistributedCall() на стороне сервера и переданные клиенту для завершения</param>
    public RemoteDistributedProc(DistributedCallData startData)
    {
      if (startData == null)
        throw new ArgumentNullException("startData");
      _StartData = startData;
      if (startData.Proxy != null)
        _MainProc = new RemoteExecProc(startData.Proxy);
    }

    /// <summary>
    /// Удаляет связанные данные
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && _MainProc != null)
      {
        _MainProc.Dispose();
        _MainProc = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Данные вызова ExecProc.StartDistributedCall()
    /// </summary>
    public DistributedCallData StartData { get { return _StartData; } }
    private DistributedCallData _StartData;

    /// <summary>
    /// Основной объект, занимающийся ожиданием.
    /// Если процедура была выполнена на стороне сервера, содержит null
    /// </summary>
    internal RemoteExecProc MainProc { get { return _MainProc; } }
    private RemoteExecProc _MainProc;

    #endregion

    #region Выполнение

    /// <summary>
    /// Запуск на асинхронное выполнение.
    /// Так как процедура уже запущена, обращение к серверу не выполняется.
    /// Поэтому же нет аргументов процедуры, так как они уже были переданы на стороне сервера.
    /// </summary>
    /// <param name="userCallback">Пользовательский метод, который получит уведомление при окончании завершения процедуры.
    /// Если null, то предполагается периодический опрос или использование WaitHandle</param>
    /// <param name="stateObject">Произвольные пользовательские данные</param>
    public IAsyncResult BeginExecute(AsyncCallback userCallback, Object stateObject)
    {
      CheckNotDisposed();
      if (_StartData.Results != null)
        return new DummyAsyncResult(userCallback, stateObject);
      else
        return _MainProc.InternalNoBeginExecute(userCallback, stateObject);
    }

    /// <summary>
    /// Запуск на асинхронное выполнение.
    /// Так как процедура уже запущена, обращение к серверу не выполняется.
    /// Поэтому же нет аргументов процедуры, так как они уже были переданы на стороне сервера.
    /// </summary>
    /// <param name="userCallback">Пользовательский метод, который получит уведомление при окончании завершения процедуры.
    /// Если null, то предполагается периодический опрос или использование WaitHandle</param>
    /// <param name="stateObject">Произвольные пользовательские данные</param>
    public IAsyncResultWithSplash BeginExecuteWithSplash(AsyncCallback userCallback, Object stateObject)
    {
      CheckNotDisposed();
      if (_StartData.Results != null)
        return new DummyAsyncResult(userCallback, stateObject);
      else
        return _MainProc.InternalNoBeginExecuteWithSplash(userCallback, stateObject);
    }

    /// <summary>
    /// Завершение асинхронного выполнения, начатого BeginExecute().
    /// Если в процессе выполнения произошла ошибка, то исключение будет выброшено сейчас.
    /// Вызывается также метод Dispose(), так как процедура всегда одноразовая
    /// </summary>
    /// <param name="asyncResult">Получено от BeginExecute()</param>
    /// <returns>Результаты успешнного выполнения</returns>
    public NamedValues EndExecute(IAsyncResult asyncResult)
    {
      NamedValues res;
      try
      {
        if (_MainProc == null)
        {
          res = _StartData.Results;
          ((DummyAsyncResult)asyncResult).Dispose();
        }
        else
          res = _MainProc.EndExecute(asyncResult);
      }
      finally
      {
        Dispose();
      }

      return res;
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс обратного вызова.
  /// Объявляет методы, используемые клиентом для опроса callback-вызова
  /// Клиент, реализующий опрос по таймеру, должен предотвращать вложенные вызовы.
  /// </summary>
  public interface IExecProcCallBack
  {
    #region Методы обработки Callback-вызова

    /// <summary>
    /// Метод, периодически вызываемый клиентом. Если он возвращает значение, отличное от null, значит
    /// требуется обработка callback вызова. Клиент должен выполнить необходимые действия и вызвать Resume()
    /// Повторный вызов GetSuspended(), вернувшего не null, без вызова Resume() или SetExeption() считается ошибкой
    /// </summary>
    /// <returns></returns>
    NamedValues GetSuspended();

    /// <summary>
    /// Этот метод вызывается клиентом после выполнения успешной обработки вызова
    /// </summary>
    /// <param name="results">Результаты, которые будут возвращены Execute()</param>
    void Resume(NamedValues results);

    /// <summary>
    /// Этот метод вызывается клиентом после возникновения исключения при обработке вызова
    /// </summary>
    /// <param name="exception">Исключение, которое будет выброшено в потоке, вызвавшем Execute</param>
    void SetException(Exception exception);

    /// <summary>
    /// Метод вызывается ExecProcCallBackSelector.Dispose()
    /// </summary>
    void SetDisposed();

    #endregion
  }

  /// <summary>
  /// Обратный вызов, когда серверу требуется управление клиентом.
  /// Другая процедура (в ExecProc.OnExecute) вызывает ExecProcCallBack.Execute().
  /// При этом текущий поток приостанавливается.
  /// Клиент должен периодически вызывать метод GetSuspended(). Если метод вернул объект NamedValues, а не null,
  /// клиент должен выполнить необходимые действия и вызвать Resume()
  /// Объект передается клиенту по ссылке (Marshal-by-reference)
  /// </summary>
  public class ExecProcCallBack : MarshalByRefSponsoredObject, IExecProc, IExecProcCallBack
  {
    #region Конструктор

    /// <summary>
    /// Создать новый объект обратного вызова без привязки к процедуре
    /// </summary>
    public ExecProcCallBack()
    {
      _SyncRoot = new object();

      _Guid = Guid.NewGuid();
      _CreationTime = DateTime.Now;
      _DisplayName = String.Empty;
    }

    /// <summary>
    /// Создать новый объект обратного вызова из процедуры.
    /// Для созданного объекта инициализируется DisplayName и создается объект спонсирования
    /// </summary>
    /// <param name="creator"></param>
    public ExecProcCallBack(ExecProc creator)
      : this()
    {
      if (creator != null)
      {
        this.DisplayName = creator.DisplayName;
        this.ExternalSponsor = creator.Proxy.Handler;
      }
    }

    #endregion

    #region Информация о создании процедуры

    /// <summary>
    /// Уникальный идентификатор процедуры.
    /// Устанавливается в конструкторе
    /// </summary>
    public Guid Guid { get { return _Guid; } }
    private readonly Guid _Guid;

    /// <summary>
    /// Время, когда объект был создан (для отладочных целей)
    /// </summary>
    public DateTime CreationTime { get { return _CreationTime; } }
    private readonly DateTime _CreationTime;


    /// <summary>
    /// Отображаемое имя.
    /// Если не установлено в явном виде, возвращает пустую строку
    /// </summary>
    public string DisplayName
    {
      get
      {
        lock (_SyncRoot)
        {
          return _DisplayName;
        }
      }
      set
      {
        lock (_SyncRoot)
        {
          if (value == null)
            _DisplayName = String.Empty;
          else
            _DisplayName = value;
        }
      }
    }
    private string _DisplayName;

    #endregion

    #region Поля

    /// <summary>
    /// Сигнализатор.
    /// Так как этот объект является Disposable(), он должен удаляться каждый раз
    /// </summary>
    private AutoResetEvent _Signaller;

    /// <summary>
    /// Хранение аргументов между вызовами Execute() и GetSuspended()
    /// </summary>
    private NamedValues _SuspendedArgs;

    /// <summary>
    /// Хранение результатов между Resume() и завершением Execute()
    /// </summary>
    public NamedValues Results
    {
      get
      {
        lock (_SyncRoot)
        {
          return _Results;
        }
      }
    }
    private NamedValues _Results;

    /// <summary>
    /// Исключение, устанавливаемое SetException()
    /// Оно будет перевыброшено в Execute()
    /// </summary>
    public Exception Exception
    {
      get
      {
        lock (_SyncRoot)
        {
          return _Exception;
        }
      }
    }
    private Exception _Exception;

    /// <summary>
    /// Объект для блокировки
    /// </summary>
    private object _SyncRoot;

    #endregion

    #region Метод, вызываемый из другой процедуры

    /// <summary>
    /// "Вывернутый" вызов с ожиданием.
    /// Устанавливает семафор и ожидает, пока внешний объект (клиент) обнаружит состояние ожидания, 
    /// выполнит действия и сообщит о выполнении.
    /// </summary>
    /// <param name="args">Аргументы</param>
    /// <returns>Результаты</returns>
    public NamedValues Execute(NamedValues args)
    {
      if (args == null)
        throw new ArgumentException("Args");

      if (ExecProc.CurrentProc == null)
        throw new InvalidOperationException("Метод должен вызываться из другой процедуры");

      if (ExecProc.CurrentProc.ChildProcCallBack != null)
        throw new BugException("Свойство ExecProc.ChildProcCallBack уже установлено");

      if (SetDisposedCalled)
        Thread.CurrentThread.Abort();

      lock (_SyncRoot)
      {
        if (!ExecProc.GetCanStart(State))
          throw new AlreadyExecutingException(this);

        _Signaller = new AutoResetEvent(false);
        _SuspendedArgs = args;
        _Results = null;
        _Exception = null;
        _ParentProc = ExecProc.CurrentProc;
        _ParentProc.ChildProcCallBack = this;
        _StartCount++;
        _StartTime = DateTime.Now;
      }

      try
      {
        _Signaller.WaitOne();
      }
      finally
      {
        lock (_SyncRoot)
        {
          _ParentProc.ChildProcCallBack = null;
          _ParentProc = null;
          _SuspendedArgs = null;
          _Signaller.Close();
          _Signaller = null;
        }
      }

      if (SetDisposedCalled)
        Thread.CurrentThread.Abort();

      if (_Exception != null)
        throw _Exception;
      else
        return _Results;
    }

    //public void ExecuteAsync(NamedValues Args)
    //{
    //  throw new NotSupportedException("Для ExecProcCallBack поддерживается только синхронный вызов");
    //}

    #endregion

    #region Информация о вызове Execute

    /// <summary>
    /// Возвращает true, если в данный момент вызван Execute(), который ожидает обработки обратного вызова
    /// </summary>
    public ExecProcState State
    {
      get
      {
        lock (_SyncRoot)
        {
          if (_Signaller != null)
            return ExecProcState.Executing;
          else
            return ExecProcState.NotStarted;
        }
      }
    }

    /// <summary>
    /// Возвращается процедура, которая сейчас ожидает завершения метода Execute()
    /// </summary>
    public ExecProc ParentProc
    {
      get
      {
        lock (_SyncRoot)
        {
          return _ParentProc;
        }
      }
    }
    private ExecProc _ParentProc;


    /// <summary>
    /// Счетчик запуска процедуры на выполнение.
    /// Увеличивается на 1 при каждом вызове Execute().
    /// Если процедура еще не запускалась, содержит 0
    /// </summary>
    public int StartCount
    {
      get
      {
        lock (_SyncRoot)
        {
          return _StartCount;
        }
      }
    }
    private int _StartCount;

    /// <summary>
    /// Время последнего вызова Execute().
    /// Если StartCount=0 возвращаемое значение не имеет смысла
    /// </summary>
    public DateTime StartTime
    {
      get
      {
        lock (_SyncRoot)
        {
          return _StartTime;
        }
      }
    }
    private DateTime _StartTime;

    /// <summary>
    /// Возвращает всю информацию о текущем объекте ExecProcCallBack в виде объекта, поддерживающего сериализацию
    /// </summary>
    /// <returns></returns>
    public ExecProcInfo GetInfo()
    {
      ExecProcInfo Res = new ExecProcInfo();
      lock (_SyncRoot)
      {
        Res.InfoTime = DateTime.Now;
        Res.Guid = _Guid;
        Res.CreationTime = _CreationTime;
        //Res.AssemblyName = GetType().AssemblyQualifiedName;
        Res.AssemblyName = GetType().Assembly.FullName; // 06.06.2019
        Res.TypeName = GetType().FullName;
        Res.DisplayName = _DisplayName;
        Res.State = State;

        // Res.StartTime = FStartTime;
        Res.StartCount = _StartCount;
        if (_StartCount > 0)
          Res.StartTime = _StartTime;
        Res.State = State;
        if (Res.State == ExecProcState.Executing)
          Res.ThreadState = System.Threading.ThreadState.Running;
        else
          Res.ThreadState = System.Threading.ThreadState.Unstarted;
        if (_ParentProc != null)
          Res.ParentProcGuid = _ParentProc.Guid;
        //if (FException != null)
        //  Res.ExceptionMessage = FException.Message;
        Res.InitDomainInfo();
      }
      return Res;
    }

    #endregion

    #region Методы, вызываемые из клиента, обрабатывающего Call Back

    /// <summary>
    /// Метод, периодически вызываемый клиентом. Если он возвращает значение, отличное от null, значит
    /// требуется обработка callback вызова. Клиент должен выполнить необходимые действия и вызвать Resume()
    /// </summary>
    /// <returns></returns>
    public NamedValues GetSuspended()
    {
      lock (_SyncRoot)
      {
        return _SuspendedArgs;
      }
    }

    /// <summary>
    /// Этот метод вызывается клиентом после выполнения успешной обработки вызова
    /// </summary>
    /// <param name="results">Результаты, которые будут возвращены Execute()</param>
    public void Resume(NamedValues results)
    {
      if (results == null)
        throw new ArgumentNullException("results");
      results.SetReadOnly();

      lock (_SyncRoot)
      {
        if (_Signaller == null)
          throw new InvalidOperationException("Метод должен вызываться только из состояния ожидания");
        if (_Results != null || _Exception != null)
          throw new InvalidOperationException("Повторный вызов Resume()/SetException()");
        _Results = results;
        _SuspendedArgs = null;
      }

      _Signaller.Set();
    }

    /// <summary>
    /// Этот метод вызывается клиентом после выполнения успешной обработки вызова
    /// </summary>
    /// <param name="exception">Исключение, которое будет выброшено в потоке, вызвавшем Execute</param>
    public void SetException(Exception exception)
    {
      if (exception == null)
        throw new ArgumentNullException("exception");

      lock (_SyncRoot)
      {
        if (_Signaller == null)
          throw new InvalidOperationException("Метод должен вызываться только из состояния ожидания");
        if (_Results != null || _Exception != null)
          throw new InvalidOperationException("Повторный вызов Resume()/SetException()");
        _Exception = exception;
        _SuspendedArgs = null;
      }

      _Signaller.Set();
    }

    #endregion

    #region SetDisposed

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public void SetDisposed()
    {
      if (_SetDisposedCalled)
        return; // повторный вызов

      _SetDisposedCalled = true;

      AutoResetEvent Signaller2;

      lock (_SyncRoot)
      {
        Signaller2 = _Signaller;
        _Results = null;
        _Exception = null;
        _SuspendedArgs = null;
      }

      // Нельзя было вызвать из-под lock
      if (Signaller2 != null)
        Signaller2.Set();
    }

    /// <summary>
    /// Возвращает true, если SetDisposed() был вызван
    /// </summary>
    public bool SetDisposedCalled { get { return _SetDisposedCalled; } }
    private bool _SetDisposedCalled;

    #endregion

    #region Заглушки IDisposabe

    void IDisposable.Dispose()
    {
    }


    #endregion

    #region IExecProc Members

    bool IExecProc.AutoDispose { get { return false; } set { } }

    int IExecProc.SyncTime { get { return 0; } set { } }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public IAsyncResult BeginExecute(NamedValues args, AsyncCallback userCallback, object stateObject)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public IAsyncResultWithSplash BeginExecuteWithSplash(NamedValues args, AsyncCallback userCallback, object stateObject)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public NamedValues EndExecute(IAsyncResult asyncResult)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public void ExecuteNoResults(NamedValues args)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public IServerSplashWatcher CreateSplashWatcher()
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Селектор выбора процедуры обратного вызова
  /// Содержит потокобезопасный список дочерних объектов IExecProcCallBack и объединяет их в один интерфейс.
  /// Когда клиент вызывает GetSuspended(), выполняется поиск ожидающего дочернего объекта.
  /// После чего он запоминается, чтобы методы Resume() и SetException() были перенаправлены именно на тот же
  /// объект
  /// Селектор создается сервером, если один клиент может запускать одновременно множество ExecProc, которые
  /// могут асинхронно делать запросы клиенту
  /// </summary>
  public class ExecProcCallBackSelector : MarshalByRefDisposableObject, ICollection<IExecProcCallBack>, IExecProcCallBack
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает селектор
    /// </summary>
    public ExecProcCallBackSelector()
    {
      _List = new List<IExecProcCallBack>();
    }

    /// <summary>
    /// Вызывается при завершении объекта
    /// </summary>
    /// <param name="disposing">true, если вызван Dispose(), false, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          ((IExecProcCallBack)this).SetDisposed();
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка вызова ExecProcCallBackSelector.SetDisposed()");
        }
      }
      base.Dispose(disposing);
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public void SetDisposed()
    {
      lock (_List)
      {
        for (int i = 0; i < _List.Count; i++)
        {
          try
          {
            _List[i].SetDisposed();
          }
          catch (Exception e) // 14.09.2017 Может быть ошибка обращения к удаленному объекту
          {
            e.Data["List.Count"] = _List.Count;
            e.Data["List.Index"] = i;
            LogoutTools.LogoutException(e, "Ошибка вызова IExecProcCallBackSelector.SetDisposed()");
          }
        }
      }
    }

    #endregion

    #region Список дочерних объектов

    // Класс ExecProcCallBackSelector нельзя было просто унаследовать от SyncCollection, т.к SyncCollection не является производным от MarshalByRefObject
    // Также нам нужна особая обработка для AppDomainUnhandledException, когда SynCollection.Remove() не сможет удалить объект

    /// <summary>
    /// Потокобезопасный список
    /// </summary>
    private readonly List<IExecProcCallBack> _List;

    /// <summary>
    /// Регистрирует процедуру в селекторе
    /// </summary>
    /// <param name="item">Регистрируемая процедура</param>
    public void Add(IExecProcCallBack item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      CheckNotDisposed();
      lock (_List)
      {
        _List.Add(item);
      }
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public void Clear()
    {
      lock (_List)
      {
        _List.Clear();
      }
    }

    /// <summary>
    /// Возвращает true, если процедура зарегистритрована в селекторе
    /// </summary>
    /// <param name="item">Искомая процедура</param>
    /// <returns>Наличие в списке</returns>
    public bool Contains(IExecProcCallBack item)
    {
      lock (_List)
      {
        return _List.Contains(item);
      }
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public void CopyTo(IExecProcCallBack[] array, int arrayIndex)
    {
      lock (_List)
      {
        _List.CopyTo(array, arrayIndex);
      }
    }

    /// <summary>
    /// Возвращает количество зарегистрированных процедур.
    /// Так как список является асинхронным, использование свойства допускается только в информативных целях
    /// </summary>
    public int Count
    {
      get
      {
        lock (_List)
        {
          return _List.Count;
        }
      }
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public bool IsReadOnly { get { return false; } }

    /// <summary>
    /// Удаляет процедуру из списка
    /// </summary>
    /// <param name="item">Удаляемая процедура</param>
    /// <returns>true, если процедура была найдена и удалена</returns>
    public bool Remove(IExecProcCallBack item)
    {
      lock (_List)
      {
        return _List.Remove(item);
      }
    }

    /// <summary>
    /// Возвращает перечислитель по копии списка процедур.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns></returns>
    public ArrayEnumerable<IExecProcCallBack>.Enumerator GetEnumerator()
    {
      IExecProcCallBack[] a = ToArray();
      return new ArrayEnumerable<IExecProcCallBack>.Enumerator(a);
    }

    IEnumerator<IExecProcCallBack> IEnumerable<IExecProcCallBack>.GetEnumerator()
    {
      IExecProcCallBack[] a = ToArray();
      return new ArrayEnumerable<IExecProcCallBack>.Enumerator(a);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      IExecProcCallBack[] a = ToArray();
      return new ArrayEnumerable<IExecProcCallBack>.Enumerator(a);
    }

    /// <summary>
    /// Создает копию списка процедур
    /// </summary>
    /// <returns>Новый массив</returns>
    public IExecProcCallBack[] ToArray()
    {
      lock (_List)
      {
        return _List.ToArray();
      }
    }

    #endregion

    #region IExecProcCallBack Members

    /// <summary>
    /// Текущий выбранный объект
    /// Синхронизация не нужна, т.к. селектор приниципиально может использоваться только одним клиентом
    /// </summary>
    private IExecProcCallBack _SelectedItem;

    /// <summary>
    /// Возвращает первый элемент в списке, вернувший ненулевой ExecProcCallBack.GetSuspended().
    /// </summary>
    /// <returns></returns>
    public NamedValues GetSuspended()
    {
      _SelectedItem = null; // отцепляем предыдущий объект

      lock (_List)
      {
        for (int i = 0; i < _List.Count; i++)
        {
          NamedValues Args;
          try
          {
            Args = _List[i].GetSuspended();
          }
          catch (Exception e)
          {
            _List.RemoveAt(i);
            throw new InvalidOperationException("Один из дочерних объектов в callback-селекторе создал исключение при вызове GetSuspended(). Этот объект удален из списка", e);
          }

          if (Args != null)
          {
            _SelectedItem = _List[i];
            return Args;
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Вызов IExecProcCallBack.Resume()
    /// </summary>
    /// <param name="results"></param>
    public void Resume(NamedValues results)
    {
      if (_SelectedItem == null)
        throw new InvalidOperationException("Не было успешного вызова GetSuspended()");

      _SelectedItem.Resume(results);
      _SelectedItem = null;
    }

    /// <summary>
    /// Не должно использоваться в пользовательском коде
    /// </summary>
    public void SetException(Exception exception)
    {
      if (_SelectedItem == null)
        throw new InvalidOperationException("Не было успешного вызова GetSuspended()");

      _SelectedItem.SetException(exception);
      _SelectedItem = null;
    }

    #endregion
  }

  /// <summary>
  /// Список для наблюдения за ExecProc, используемых клиентом.
  /// Этот класс может использоваться сервером, поддерживающим множество соединений с клиентами.
  /// Для каждого клиента на сервере создается один список ExecProcWatchList.
  /// Когда клиент создает процедуру на сервере, она регистрируется в его списке.
  /// Список позволяет отслеживать процедуры (для отладки) и удалить все процедуры в случае разрыва
  /// соединения.
  /// Позволяет регистрировать как процедуры ExecProc, созданные в основном домене приложения сервера,
  /// так и в дополнительных доменах (ExecProcProxy).
  /// Отменять регистрацию процедур не требуется.
  /// Класс является потокобезопасным.
  /// </summary>
  public class ExecProcWatchList : DisposableObject
  {
    // 02.01.2021
    // Этот класс, вероятно, не следует выводить из SimpleDisposableObject, 
    // так как удаление процедур выполняется и из деструктора.

    #region Конструктор и Dispose

    /// <summary>
    /// Создает список
    /// </summary>
    public ExecProcWatchList()
    {
      _Procs = new WeakReferenceCollection<IExecProc>();
      _Proxies = new WeakReferenceCollection<ExecProcProxy>();
    }

    /// <summary>
    /// Удаляет список, вызывая для каждой процедуры Dispose()
    /// </summary>
    /// <param name="disposing">trum, если вызван Dispose(), false, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      ExecProcProxy[] a1 = _Proxies.ToArray();
      for (int i = 0; i < a1.Length; i++)
        a1[i].Handler.Dispose();

      IExecProc[] a2 = _Procs.ToArray();
      for (int i = 0; i < a2.Length; i++)
        a2[i].Dispose();

      base.Dispose(disposing);
    }

    #endregion

    #region Списки

    private readonly WeakReferenceCollection<IExecProc> _Procs;
    private readonly WeakReferenceCollection<ExecProcProxy> _Proxies;

    /// <summary>
    /// Зарегистрировать процедуру в основном домене приложения сервера
    /// </summary>
    /// <param name="proc">Процедура</param>
    public void Add(IExecProc proc)
    {
      if (proc != null)
        _Procs.Add(proc);
    }

    /// <summary>
    /// Зарегистрировать процедуру в произвольном домене
    /// </summary>
    /// <param name="proxy">Прокси для процедуры</param>
    public void Add(ExecProcProxy proxy)
    {
      if (proxy != null)
        _Proxies.Add(proxy);
    }

    #endregion

    #region Извлечение информации для отладки

    /// <summary>
    /// Возврващает количество процедур, зарегистрированных в списке
    /// </summary>
    public int Count { get { return _Proxies.Count + _Procs.Count; } }

    private static readonly ExecProcInfo[] EmptyArray = new ExecProcInfo[0];

    /// <summary>
    /// Возвращает информацию для всех процедур в списке
    /// </summary>
    /// <returns>Информация о процедуре</returns>
    public ExecProcInfo[] GetInfo()
    {
      ExecProcProxy[] a1 = _Proxies.ToArray();
      IExecProc[] a2 = _Procs.ToArray();
      if (a1.Length + a2.Length == 0)
        return EmptyArray;

      Dictionary<Guid, ExecProcInfo> dict = new Dictionary<Guid, ExecProcInfo>(a1.Length + a2.Length);
      for (int i = 0; i < a1.Length; i++)
        dict[a1[i].Guid] = a1[i].Handler.GetInfo();
      for (int i = 0; i < a2.Length; i++)
        dict[a2[i].Guid] = a2[i].GetInfo();

      ExecProcInfo[] a = new ExecProcInfo[dict.Count];
      dict.Values.CopyTo(a, 0);
      return a;
    }

    #endregion
  }

  /// <summary>
  /// Выполняемая процедура, вызывающая произвольный делегат.
  /// Используется, когда некий метод должен обязательно выполняться в контексте ExecProc
  /// (например, если используется блокировка).
  /// Делегат может иметь произвольный список параметров (кроме ссылочных) и возвращать значения.
  /// Для вызова пользовательского метода используется Delegate.DynamicInvoke().
  /// При выполнении процедуры, в методе Execute() не используются ни входные аргументы, не результаты
  /// в формате NamedValues
  /// </summary>
  public sealed class MethodInvokeExecProc : ExecProc
  {
    #region Конструктор

    /// <summary>
    /// Эта версия конструктора используется, если пользовательский метод не получает аргументов
    /// </summary>
    /// <param name="userDelegate">Пользовательский делегат</param>
    public MethodInvokeExecProc(Delegate userDelegate)
      : this(userDelegate, null)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет передать пользователскому делегату произвольный набор параметров
    /// </summary>
    /// <param name="userDelegate">Пользовательский делегат</param>
    /// <param name="methodParams">Параметры, передаваемые пользовательскому методу. 
    /// null означает отсутствие параметров</param>
    public MethodInvokeExecProc(Delegate userDelegate, params object[] methodParams)
    {
      if (userDelegate == null)
        throw new ArgumentNullException("userDelegate");

      _Delegate = userDelegate;
      _Params = methodParams;

      SetContext(NamedValues.Empty);
    }

    #endregion

    #region Поля

    /// <summary>
    /// Пользовательский делегат.
    /// Задается в конструкторе.
    /// </summary>
    public Delegate Delegate { get { return _Delegate; } }
    private readonly Delegate _Delegate;

    /// <summary>
    /// Арргументы, передаваемые пользовательскому методу.
    /// Задаются в конструкторе.
    /// Может быть null, если параметры не используются
    /// </summary>
    public object[] Params { get { return _Params; } }
    private readonly object[] _Params;

    /// <summary>
    /// Результат, возвращенный пользовательским методом.
    /// Если метод не возвращает значение, свойство содержит null
    /// </summary>
    public object Result { get { return _Result; } }
    private object _Result;

    #endregion

    #region Выполнение

    /// <summary>
    /// Вызывает Delegate.DynamicInvoke() 
    /// </summary>
    /// <param name="args">Аргументы</param>
    /// <returns>Результаты</returns>
    protected override NamedValues OnExecute(NamedValues args)
    {
      _Result = _Delegate.DynamicInvoke(_Params);
      return NamedValues.Empty;
    }

    #endregion

    #region Статический метод запуска

    /// <summary>
    /// Выполнение пользовательского метода в контексте ExecProc.
    /// Если в вызывающем потоке выполняется ExecProc, пользовательский делегат вызывается напрямую,
    /// без использования объекта MethodInvokeExecProc.
    /// Если ExecProc не выполняется, создается MethodInvokeExecProc, а затем вызывается его метод Execute()
    /// </summary>
    /// <param name="userDelegate">Пользовательский делегат, который должен быть вызван</param>
    /// <param name="methodParams">Параметры, передаваемые пользовательскому методу. 
    /// null означает отсутсвие параметров</param>
    /// <returns>Значение, возвращаемое пользовательским методом. Возвращается null,
    /// если пользовательский метод не возвращает значения</returns>
    public static object RunWithExecProc(Delegate userDelegate, params object[] methodParams)
    {
      if (ExecProc.CurrentProc == null)
      {
        using (MethodInvokeExecProc Proc = new MethodInvokeExecProc(userDelegate, methodParams)) //27.12.2020 - Добавлен using
        {
          Proc.Execute(NamedValues.Empty);
          return Proc.Result;
        }
      }
      else
        return userDelegate.DynamicInvoke(methodParams);
    }

    /// <summary>
    /// Выполнение пользовательского метода в контексте ExecProc.
    /// Если в вызывающем потоке выполняется ExecProc, пользовательский делегат вызывается напрямую,
    /// без использования объекта MethodInvokeExecProc.
    /// Если ExecProc не выполняется, создается MethodInvokeExecProc, а затем вызывается его метод Execute().
    /// Эта версия используется, если пользовательский метод не имеет аргументов
    /// </summary>
    /// <param name="userDelegate">Пользовательский делегат, который должен быть вызван</param>
    /// <returns>Значение, возвращаемое пользовательским методом. Возвращается null,
    /// если пользователський метод не возвращает значения</returns>
    public static object RunWithExecProc(Delegate userDelegate)
    {
      return RunWithExecProc(userDelegate, null);
    }

    #endregion
  }
}
