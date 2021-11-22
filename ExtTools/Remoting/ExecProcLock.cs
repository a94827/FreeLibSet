// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Logging;
using System.Threading;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Remoting
{
  /// <summary>
  /// Блокировка доступа к какому-либо ресурсу из процедуры ExecProc. На время
  /// ожидания освобождения ресурса ожидающая процедура выводит индикатор, показывающий
  /// ход выполнения процедуры, занимающей ресурс.
  /// Обычно существует статический экземпляр объекта ExecProcLock или производного класса.
  /// Когда процедуре требуется доступ к разделяемому ресурсу, создается объект ExecProcLockKey с помощью оператора using на время
  /// работы с ресурсом.
  /// Если приложение использует AppDomain, то следует учитывать, что статические объекты в разных доменах не связаны друг с другом.
  /// Этот класс является потокобезопасным
  /// </summary>
  public class ExecProcLock
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, который можно блокировать
    /// </summary>
    /// <param name="displayName">Отображаемое имя блокируемого объекта. Должно быть задано</param>
    public ExecProcLock(string displayName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(displayName))
        throw new ArgumentNullException("displayName");
#endif

      _MainSyncRoot = new object();

      _DisplayName = displayName;
      _LockCount = 0;
      _OwnCaller = null;
      _CheckIfOwnCallerIsDisposed = true;

      _Waiters = new SyncCollection<ExecProcLockLogger>(new List<ExecProcLockLogger>());
    }

    #endregion

    #region Методы блокировки

    internal void CheckLocked()
    {
      if (LockCount == 0)
        throw new BugException("Объект " + DisplayName + " не был заблокирован");
    }

    /// <summary>
    /// Объект используется для кратковременной блокировки при
    /// обращении к полям LockCount и OwnCaller
    /// </summary>
    private readonly object _MainSyncRoot;

    /// <summary>
    /// Счетчик блокировок.
    /// Одна и та же процедура ExecProc может блокировать объекь ExecProcLock несколько раз (аналогично классу Monitor).
    /// Удачная попытка TryLock() или Lock() увеличивает счетчик на 1, а вызов Unlock() - уменьшает на 1.
    /// Ресурс освобождается, когда, счетчик становится равным 0.
    /// </summary>
    public int LockCount { get { return _LockCount; } }
    private int _LockCount;

    /// <summary>
    /// Текущий владелец блокировки.
    /// Возвращает null, если блокировка не установлена.
    /// Это свойство можно использовать только в целях отладки так как оно меняется асинхронно.
    /// </summary>
    public ExecProc OwnCaller { get { return _OwnCaller; } }
    private ExecProc _OwnCaller;

    /// <summary>
    /// Идентификатор установленной блокировки. 
    /// Устанавливается, когда LockCount меняется с 0 на 1, то есть при удачной блокировке объекта процедурой. Вложенные методы Lock() не меняют свойства.
    /// Используется только в отладочных целях.
    /// </summary>
    public int LockingId { get { return _LockingId; } }
    private int _LockingId;

    private static int LastLockingId = 0;

#if DEBUGSTACK

    public string OwnCallerStackTrace
    {
      get
      {
        lock (_MainSyncRoot)
        {
          return _OwnCallerStackTrace;
        }
      }
    }
    private string _OwnCallerStackTrace;

    /// <summary>
    /// Время, когда установлена блокировка
    /// </summary>
    public DateTime LockStartTime
    {
      get
      {
        lock (_MainSyncRoot)
        {
          return _LockStartTime;
        }
      }
    }
    private DateTime _LockStartTime;

#endif

    /// <summary>
    /// Попытка блокировки объекта. Возвращает true, если блокировка выполнена.
    /// В случае успеха должен быть вызов Unlock().
    /// Этот метод не выполняет ожидание и завершается сразу.
    /// </summary>
    /// <param name="caller">Процедура, которая пытается получить блокировку</param>
    /// <returns>True в случае успеха</returns>
    public bool TryLock(ExecProc caller)
    {
      ExecProcLock LockedObj;
      return TryLock(caller, out LockedObj);
    }

    /// <summary>
    /// Попытка блокировки объекта. Возвращает true, если блокировка выполнена.
    /// В случае успеха должен быть вызов Unlock().
    /// Этот метод не выполняет ожидание и завершается сразу.
    /// Эта перегрузка возвращает, в случае неудачи блокировки, ссылку на ExecProcLock, который не удалось заблокировать.
    /// </summary>
    /// <param name="caller">Процедура, которая пытается получить блокировку</param>
    /// <param name="lockedObj">Ссылка на объект, который не удалось заблокировать.
    /// Для простых ExecProcLock возвращается ссылка на текущий объект. Для ExecProcMultiLock это может быть и один из дочерних объектов.</param>
    /// <returns>True в случае успеха</returns>
    public virtual bool TryLock(ExecProc caller, out ExecProcLock lockedObj)
    {
      if (caller == null)
        throw new ArgumentNullException("caller");
      bool Res;

      lock (_MainSyncRoot)
      {
        Res = TryLock2(caller, out lockedObj);
      }
      return Res;
    }

    private bool TryLock2(ExecProc caller, out ExecProcLock lockedObj)
    {
      if (!System.Threading.Monitor.TryEnter(this))
      {
        lockedObj = this;
        return false;
      }

      try
      {
        if (_LockCount == 0)
        {
          if (!DoTryLock(caller, out lockedObj))
          {
            System.Threading.Monitor.Exit(this);
            return false;
          }
          _OwnCaller = caller;
#if DEBUGSTACK
          _OwnCallerStackTrace = Environment.StackTrace;
          _LockStartTime = DateTime.Now;
#endif

          //lock (typeof(ExecProcLock))
          lock (DataTools.InternalSyncRoot) // 27.12.2020
          {
            unchecked
            {
              LastLockingId++;
              if (LastLockingId <= 0)
                LastLockingId = 1;
              _LockingId = LastLockingId;
            }
          }
        }

        // Удачная попытка
        lockedObj = null;
        _LockCount++;
        return true;
      }
      catch
      {
        System.Threading.Monitor.Exit(this);
        throw;
      }
    }

    /// <summary>
    /// Блокировка объекта с ожиданием. Пока выполняется ожидание, выдается заставка.
    /// Если пользователь нажмет кнопку "Отмена" в заставке для прерывания ожидания, выбрасывается исключение
    /// Каждому вызову Lock() должен соответствовать вызов Unlock().
    /// </summary>
    /// <param name="caller">Процедура, которая пытается получить блокировку</param>
    public virtual void Lock(ExecProc caller)
    {
      if (TryLock(caller))
        return; // Удалось с первой попытки
      Thread.Sleep(50);
      if (TryLock(caller))
        return; // Удалось со второй попытки
      Thread.Sleep(100);

      DateTime PretendingStartTime = DateTime.Now;

      ExecProcLockStepInfo StepInfo = null; // используется вместо FirstFlag=true
      //int PrevLockingId = 0;
      // TODO: ExecProc OldOwner = null;
      ExecProcLockLogger Logger = null;
      TempSplashPhaseHandler WaitSPH = null;
      try
      {
        ExecProcLock LockedObj;
        while (!TryLock(caller, out LockedObj))
        {
          if (LockedObj == null)
            throw new BugException("Метод TryLock не вернул LockedObj");


          // 29.11.2014
          // Может быть так, что свойство LockedObj.OwnCaller переключится на null в параллельном потоке, уже после вызова TryLock
          ExecProc LockedObjOwnCaller = LockedObj.OwnCaller; // теперь никуда не денется
          try
          {
            if (LockedObjOwnCaller != null)
            {
              if (StepInfo == null) // первый такт ожидания
              {
                if (LockedObjOwnCaller == caller)
                  throw new BugException("Ошибка вложенной блокировки. Процедура " + caller.ToString() + " пытается повторно установить блокировку объекта " + LockedObj.ToString());

                // Синхронизируем splash-заставки.
                // Эта заставки используется для проформы
                WaitSPH = new TempSplashPhaseHandler(LockedObj.DisplayName);
                WaitSPH.AllowCancel = true;
                InitWaitSPH(WaitSPH, caller, LockedObj, LockedObjOwnCaller);
                // TODO: OldOwner = Caller.ExternalSplashOwner;
                // TODO: if (FOwnCaller == Caller)
                // TODO:   Caller.ExternalSplashOwner = null;
                // TODO: else
                // TODO:   Caller.ExternalSplashOwner = FOwnCaller;

                Logger = new ExecProcLockLogger(this, caller, LockedObj, LockedObjOwnCaller);
                Waiters.Add(Logger);
                caller.LongLockStarted(Logger);
                StepInfo = new ExecProcLockStepInfo(Logger);

              }
              else
              {
                //if (LockedObj.LockingId != PrevLockingId)
                // Всегда обновляем заставку, так как может поменяться процентный индикатор
                InitWaitSPH(WaitSPH, caller, LockedObj, LockedObjOwnCaller);
              }
              StepInfo.LockedObj = LockedObj;
              StepInfo.LockedObjOwnCaller = LockedObjOwnCaller;
              //PrevLockingId = LockedObj.LockingId;
              caller.LongLockStep(StepInfo);
            }

            // Здесь WaitSPH не может быть null.
            // Дрыхнем полсекунды
            WaitSPH.Sleep(500); 
            // Проверяем прерывание ожидания
            WaitSPH.CheckCancelled();
          }

          catch (UserCancelException e)
          {
            // 29.11.2014
            // Надо же узнать, почему 
            e.Data["ThisExecProcLock"] = this.ToString();
            if (Object.ReferenceEquals(LockedObj, this))
              e.Data["LockedObj"] = "=this";
            else
              e.Data["LockedObj"] = LockedObj.ToString();
            if (LockedObjOwnCaller == null)
              e.Data["LockedObjOwnCaller"] = null;
            else
              e.Data["LockedObjOwnCaller"] = LockedObjOwnCaller.ToString();
            e.Data["Pretender"] = caller.ToString();
#if DEBUGSTACK
            e.Data["LockOwnerStackTrace"] = LockedObj.OwnCallerStackTrace;
            e.Data["PretenderStackTrace"] = Environment.StackTrace;

            e.Data["LockOwnerTimeSpan"] = DateTime.Now - LockedObj.LockStartTime;
#endif
            e.Data["PretenderTimeSpan"] = DateTime.Now - PretendingStartTime;

            LogoutTools.LogoutException(e, "Отказ от ожидания блокировки");
            throw;
          }
        }
      }
      finally
      {
        if (StepInfo != null)
        {
          // TODO: Caller.ExternalSplashOwner = OldOwner;
          WaitSPH.Dispose(); // WaitSPH больше недействителен
        }
        if (Logger != null)
        {
          caller.LongLockFinished(Logger);
          Waiters.Remove(Logger);
          Logger.Dispose();
        }
      }
    }

    private static void InitWaitSPH(TempSplashPhaseHandler waitSPH, ExecProc caller, ExecProcLock lockedObj, ExecProc lockedObjOwnCaller)
    {
      try
      {
        try
        {
          StringBuilder sb = new StringBuilder();
          sb.Append("Ожидание блокировки объекта \"");
          sb.Append(lockedObj.DisplayName);
          sb.Append("\"");
          //sb.Append(". Ожидающая процедура: ");
          //sb.Append(Caller.ToString());
          sb.Append(". Заблокировал: ");
          if (lockedObjOwnCaller == null)
            sb.Append("[ нет данных]");
          else
          {
            sb.Append(lockedObjOwnCaller.ToString());

            ISplash orgSplash = lockedObjOwnCaller.SplashStack.Splash;
            if (orgSplash != null)
            {
              sb.Append(" - " + orgSplash.PhaseText);
              waitSPH.SetPercent(orgSplash.Percent, orgSplash.PercentMax);
            }
            else
            {
              waitSPH.SetPercent(0, 0);
            }
          }

          waitSPH.PhaseText = sb.ToString();
        }
        catch (Exception e)
        {
          waitSPH.PhaseText = "Ошибка при получении описания заблокированного объекта. " + e.Message;
        }
      }
      catch { }
    }

    /// <summary>
    /// Снятие блокировки, выполненной Lock() или удачным TryLock().
    /// Уменьшает на 1 значение свойства LockCount. ExecProcLock считается освободженным, когда LockCount становится равным 0
    /// </summary>
    public virtual void Unlock()
    {
      lock (_MainSyncRoot)
      {
        CheckLocked();

        if (_LockCount == 1)
        {
          try
          {
            DoUnlock();
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "Ошибка разблокировки " + this.ToString());
          }
        }

        _LockCount--;
        if (_LockCount == 0)
          _OwnCaller = null;
        System.Threading.Monitor.Exit(this);
      }

      // После снятия блокировки передаем ненадолго управление другому потоку
      if (Waiters.Count > 0) // предварительная проверка
      {
        ExecProcLockLogger[] Others = Waiters.ToArray();
        if (Others.Length > 0)
        {
          Thread OtherThread = Others[0].Pretender.ExecThread;
          if (OtherThread != null)
            OtherThread.Join(20); // Пусть попробует выполнить блокировку
        }
      }
    }

    #endregion

    #region Методы блокировки дочерних объектов

    /// <summary>
    /// Выполнить попытку блокировки дочерних объектов.
    /// Этот метод вызывается из TryLock(), если в текущем объекте ExecProcLock можно установить блокировку.
    /// Метод переопределяется в классах, где есть связанные объекты ExecProcLock(), которые можно блокировать.
    /// Переопределенный метод не должен вызывать базовый метод в ExecProcLock.
    /// Если в классе-наследнике есть несколько связанных объектов и один из них не удалось заблокировать, то для тех из них, которые были заблокированы
    /// до него, требуется вызвать Unlock(), чтобы отменить блокировку.
    /// Непереопределенный метод всегда возвращает true, так как нет связанных объектов.
    /// </summary>
    /// <param name="caller">Процедура, которая пытается установить блокировку</param>
    /// <param name="lockedObj">Сюда записывается ссылка на связанный Объект ExecProcLock, который не удалось заблокировать</param>
    /// <returns>True, если удалось установить блокировку всех связанных объектов</returns>
    protected virtual bool DoTryLock(ExecProc caller, out ExecProcLock lockedObj)
    {
      lockedObj = null;
      return true;
    }

    /// <summary>
    /// Снять блокировку с дочерних объектов.
    /// Это метод вызывается в Unlock() до разблокировки основного объекта.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    protected virtual void DoUnlock()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя объекта в сообщении "Ожидание блокировки объекта XXX"
    /// Задается в конструкторе
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    /// <summary>
    /// Текстовое представление объекта блокировки (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName + " (LockCount=" + LockCount.ToString() + ")";
    }

    /// <summary>
    /// Если сброшено в false, то при ожидании блокировки, если ранее выполнена
    /// блокировка процедурой, которая уже завершена, исключение не выбрасывается,
    /// а выдается обычное окно ожидания
    /// По умолчанию - true - выбрасывается исключение
    /// </summary>
    public bool CheckIfOwnCallerIsDisposed { get { return _CheckIfOwnCallerIsDisposed; } set { _CheckIfOwnCallerIsDisposed = value; } }
    private bool _CheckIfOwnCallerIsDisposed;

    /// <summary>
    /// Список описателей процедур, ожидающих этой блокировки.
    /// Для множественных блокировок процедура попадает только в список для блокировки верхнего уровня, то есть той, для которой был вызван Lock()
    /// </summary>
    public SyncCollection<ExecProcLockLogger> Waiters { get { return _Waiters; } }
    private SyncCollection<ExecProcLockLogger> _Waiters;

    /// <summary>
    /// Пустой массив блокировок
    /// </summary>
    public static readonly ExecProcLock[] EmptyArray = new ExecProcLock[0];

    #endregion

    #region Отладочные средства

    internal static void GetDebugCallStack(LogoutInfoNeededEventArgs args, string stackTrace, string title)
    {
      if (!String.IsNullOrEmpty(title))
      {
        args.WriteLine(title);
        args.IndentLevel++;
      }

      if (String.IsNullOrEmpty(stackTrace))
        args.WriteLine("Стек вызовов недоступен");
      else
      {

        string[] a = stackTrace.Split(DataTools.NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < a.Length; i++)
          args.WriteLine(a[i].Trim());
      }

      if (!String.IsNullOrEmpty(title))
        args.IndentLevel--;
    }

    /// <summary>
    /// Добавляет отладочную информацию
    /// </summary>
    /// <param name="args">Объект для записи log-данных</param>
    public virtual void GetDebugInfo(LogoutInfoNeededEventArgs args)
    {
      args.WritePair("Блокировка", DisplayName);
      lock (_MainSyncRoot)
      {
        args.WritePair("Уровень блокировки", LockCount.ToString());
        if (OwnCaller != null)
        {
          args.WritePair("Владелец блокировки", OwnCaller.ToString());
#if DEBUGSTACK
          GetDebugCallStack(Args, OwnCallerStackTrace, "Стек вызовов:");
          Args.WritePair("Блокировка установлена", LockStartTime.ToString() + ", время: " + CommonTools.TimeSpanToString(DateTime.Now - LockStartTime));
#endif
        }


        ExecProcLockLogger[] Others = Waiters.ToArray();
        args.WriteLine("Процедуры, ожидающие освобождения блокировки (" + Others.Length.ToString() + ")");
        args.IndentLevel++;
        for (int i = 0; i < Others.Length; i++)
        {
          args.WriteLine(Others[i].Pretender.ToString());
        }
        args.IndentLevel--;
      }
    }

    #endregion
  }

  /// <summary>
  /// Отслеживание ожидания блокировок.
  /// Объект существует во время работы метода ExecProcLock.Lock()
  /// Класс является потокобезопасным. Все свойства устанавливаются в закрытом конструкторе объекта.
  /// </summary>
  public sealed class ExecProcLockLogger : DisposableObject
  {
    // 02.01.2021
    // Не знаю, можно ли использовать SimpleDisposableObject

    #region Конструктор и Dispose

    internal ExecProcLockLogger(ExecProcLock execLock, ExecProc pretender, ExecProcLock lockedObj, ExecProc lockedObjOwnCaller)
    {
      _Lock = execLock;
      _Pretender = pretender;
      _LockedObj = lockedObj;
      _LockedObjOwnCaller = lockedObjOwnCaller;

      _StartTime = DateTime.Now;

#if DEBUGSTACK
      FPretenderStackTrace = Environment.StackTrace;
      FOwnCallerStackTrace = Lock.OwnCallerStackTrace; // Запоминаем в конструкторе, т.к. потом свойство Lock.OwnCallerStackTrace может поменяться
#endif

      lock (ActiveLocks.SyncRoot)
      {
        unchecked
        {
          LastWaitingId++;
          if (LastWaitingId < 1)
            LastWaitingId = 1;
        }
        _WaitingId = LastWaitingId;
      }

      ActiveLocks.Add(this);
    }

    /// <summary>
    /// Удаляет текущий объект из списка активных блокировок
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      ActiveLocks.Remove(this);

      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект, блокировка которого ожидается
    /// </summary>
    public ExecProcLock Lock { get { return _Lock; } }
    private ExecProcLock _Lock;

    /// <summary>
    /// Процедура, которая ожидает блокировку
    /// </summary>
    public ExecProc Pretender { get { return _Pretender; } }
    private readonly ExecProc _Pretender;

#if DEBUGSTACK

    /// <summary>
    /// Стек вызовов ожидающей процедуры
    /// </summary>
    public string PretenderStackTrace { get { return FPretenderStackTrace; } }
    private string FPretenderStackTrace;

#endif

    /// <summary>
    /// Заблокированный объект. Обычно совпадает с Lock, но может отличаться для зависимых блокировок
    /// </summary>
    public ExecProcLock LockedObj { get { return _LockedObj; } }
    private readonly ExecProcLock _LockedObj;

    /// <summary>
    /// Процедура, заблокировавшая ресурс. В некоторых случаях может быть null
    /// </summary>
    public ExecProc LockedObjOwnCaller { get { return _LockedObjOwnCaller; } }
    private readonly ExecProc _LockedObjOwnCaller;

#if DEBUGSTACK

    /// <summary>
    /// Стек вызовов процедуры, занявшей блокировку
    /// </summary>
    public string OwnCallerStackTrace { get { return FOwnCallerStackTrace; } }
    private string FOwnCallerStackTrace;

#endif

    /// <summary>
    /// Время начала ожидания блокировки
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } }
    private readonly DateTime _StartTime;

    /// <summary>
    /// Идентификатор сеанса ожидания
    /// </summary>
    public int WaitingId { get { return _WaitingId; } }
    private readonly int _WaitingId;

    private static int LastWaitingId = 0;

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Id=" + WaitingId.ToString();
    }

    #endregion

    #region Отладочная информация

    /// <summary>
    /// Добавляет отладочную информацию о текущем объекте
    /// </summary>
    /// <param name="args">Аргументы события получения отладочной информации</param>
    public void GetDebugInfo(LogoutInfoNeededEventArgs args)
    {
      args.WritePair("Идентификатор ожидания", WaitingId.ToString() + ", начало ожидания: " + StartTime.ToString() + ", время: " + (DateTime.Now - StartTime).ToString());
      args.WritePair("Ожидание блокировки", Lock.DisplayName);
      if (LockedObj != Lock)
        args.WritePair("Блокированный объект", LockedObj.DisplayName);
#if DEBUGSTACK
      Args.WriteLine("");
#endif
      args.WritePair("Ожидающая процедура", Pretender.ToString());
#if DEBUGSTACK
      ExecProcLock.GetDebugCallStack(Args, PretenderStackTrace, "Стек вызовов ожидающей процедуры");
      Args.WriteLine("");
#endif

      if (LockedObjOwnCaller == null)
        args.WriteLine("Нет информации о процедуре, установившей блокировку");
      else
      {
        args.WriteLine("Заблокировавшая процедура :" + LockedObjOwnCaller.ToString());
#if DEBUGSTACK
        ExecProcLock.GetDebugCallStack(Args, OwnCallerStackTrace, "Стек вызовов заблокировавшей процедуры");
#endif
      }
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Статический список ожидающих блокировок
    /// </summary>
    public static readonly SyncCollection<ExecProcLockLogger> ActiveLocks =
      new SyncCollection<ExecProcLockLogger>(new List<ExecProcLockLogger>());

    /// <summary>
    /// Пустой массив
    /// </summary>
    public static readonly ExecProcLockLogger[] EmptyArray = new ExecProcLockLogger[0];

    #endregion
  }

  /// <summary>
  /// Информация для очередного шага ожидания длительной блокировки.
  /// Аргумент StepInfo метода ExecProcLock.LongLockStep().
  /// В отличие от ExecProcLockStepLogger, свойства которого устанавливаются в конструкторе и больше не изменяются, свойства
  /// этого объекта отличаются при каждом вызове LongLockStep()
  /// </summary>
  public sealed class ExecProcLockStepInfo
  {
    #region Защищенный конструктор

    internal ExecProcLockStepInfo(ExecProcLockLogger logger)
    {
      _Logger = logger;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Информация о входе в состояние ожидания.
    /// Этот объект не меняется в течение всего цикла ожидания Lock()
    /// </summary>
    public ExecProcLockLogger Logger { get { return _Logger; } }
    private readonly ExecProcLockLogger _Logger;

    /// <summary>
    /// Текущий объект, блокировка которого не удалась.
    /// </summary>
    public ExecProcLock LockedObj { get { return _LockedObj; } internal set { _LockedObj = value; } }
    private ExecProcLock _LockedObj;

    /// <summary>
    /// Процедура, удерживающая блокировку LockedObj
    /// </summary>
    public ExecProc LockedObjOwnCaller { get { return _LockedObjOwnCaller; } internal set { _LockedObjOwnCaller = value; } }
    private ExecProc _LockedObjOwnCaller;

    #endregion

    #region Отладочная информация

    /// <summary>
    /// Добавляет отладочную информацию о текущем объекте
    /// </summary>
    /// <param name="args">Аргументы события получения отладочной информации</param>
    public void GetDebugInfo(LogoutInfoNeededEventArgs args)
    {
      Logger.GetDebugInfo(args);
      args.WritePair("Текущая блокировка", LockedObj.ToString());
      args.WritePair("Идентификатор блокировки", LockedObj.LockingId.ToString());
      args.WriteLine("Текущий блокирующий объект");
      args.IndentLevel++;
      LockedObjOwnCaller.GetDebugInfo(args);
      args.IndentLevel--;
    }

    #endregion
  }


  /// <summary>
  /// Класс блокировки множества объектов с избеганием Deadlock'а
  /// Все методы этого объекта являются потокобезопасными
  /// </summary>
  public class ExecProcMultiLock : ExecProcLock
  {
    #region Конструктор

    /// <summary>
    /// Создает объект множественной блокировки.
    /// Будет вызван виртуальный метод для инициализации списка дочерних блокировок
    /// </summary>
    /// <param name="displayName"></param>
    public ExecProcMultiLock(string displayName)
      : this(displayName, null)
    {
    }

    /// <summary>
    /// Создает объект множественной блокировки.
    /// Если список Children равен null, то будет вызван виртуальный метод для инициализации списка
    /// </summary>
    /// <param name="displayName">Отображаемое имя этого объекта блокировки</param>
    /// <param name="children">Список дочерних блокировок или null, если список должен быть поучен позднее</param>
    public ExecProcMultiLock(string displayName, ExecProcLock[] children)
      : base(displayName)
    {
      if (children != null)
        CheckChildren(children);
      _Children = children;
    }

    private static void CheckChildren(ExecProcLock[] children)
    {
#if DEBUG
      if (children == null)
        throw new ArgumentNullException("children");
#endif

      for (int i = 0; i < children.Length; i++)
      {
        if (children[i] == null)
          throw new ArgumentException("LockObjects[" + i.ToString() + "]=null", "children");
      }
    }

    #endregion

    #region Доступ к списку зависимых объектов


    /// <summary>
    /// Возвращает массив дочерних блокировок.
    /// Если массив не задан в конструкторе, однократно вызывается виртуальный метод CreateChildren() для создания списка
    /// </summary>
    /// <returns></returns>
    public ExecProcLock[] GetChildren()
    {
      lock (this)
      {
        if (_Children == null)
        {
          _Children = CreateChildren();
          CheckChildren(_Children);
        }

        return _Children;
      }
    }
    private ExecProcLock[] _Children;

    /// <summary>
    /// Этот метод должен быть переопределен, если вызывается конструктор объекта без присоединенного списка
    /// дочерних блокировок
    /// Метод вызывается однократно.
    /// Вызывать этот базовый метод нельзя
    /// </summary>
    /// <returns></returns>
    protected virtual ExecProcLock[] CreateChildren()
    {
      throw new NotImplementedException("Этот метод должен быть переопределен в производном классе, если список дочерних блокировок не задается в конструкторе");
    }

    #endregion

    #region Методы блокировки

    /// <summary>
    /// Выполняет блокировку всех зависимых объектов, вызывая для них TryLock().
    /// Если один из них не удалось заблокировать, то для уже заблокированных вызывается метод Unlock() и возвращается false
    /// </summary>
    /// <param name="caller">Процедура ExecProc, пытающаяся установить блокировку</param>
    /// <param name="lockedObj">Если не удалось выполнить блокировку, то сюда записывается ссылка на зависимый объект, который не удалось заблокировать</param>
    /// <returns>True, если блокировка установлена</returns>
    protected override bool DoTryLock(ExecProc caller, out ExecProcLock lockedObj)
    {
      ExecProcLock[] Children = GetChildren();

      for (int i = 0; i < Children.Length; i++)
      {
        if (!Children[i].TryLock(caller, out lockedObj))
        {
          // Снимаем блокировку с остальных объектов
          for (int j = 0; j < i; j++)
            Children[j].Unlock();
          return false;
        }
      }

      lockedObj = null;
      return true;
    }

    /// <summary>
    /// Снять блокировку всех зависимых объектов
    /// </summary>
    protected override void DoUnlock()
    {
      ExecProcLock[] Children = GetChildren();

      for (int i = 0; i < Children.Length; i++)
        Children[i].Unlock();
    }

    #endregion
  }

  /// <summary>
  /// Объект для выполнения блокировки заданного объекта с помощью оператора using.
  /// Конструктор объекта вызывает метод ExecProcLock.Lock(), а Dispose - Unlock().
  /// На момент вызова в текущем потоке должна выполняться процедура
  /// </summary>
  public class ExecProcLockKey : SimpleDisposableObject
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Устанавливает блокировку вызывом ExecProcLock.Lock().
    /// Если ресурс заблокирован другой процедурой, выполняется ожидание, пока блокировка не освободится.
    /// Вызывающей считается текущая процедура ExecProc.CurrentProc.
    /// Создавать блокировку вне выполняющейся процедуры нельзя
    /// </summary>
    /// <param name="theLock">Блокируемый объект</param>
    public ExecProcLockKey(ExecProcLock theLock)
    {
      if (theLock == null)
        throw new ArgumentNullException("theLock");
      _Caller = ExecProc.CurrentProc;
      if (_Caller == null)
        throw new InvalidOperationException("В текущем потоке нет выполняющейся процедуры ExecProc");

      theLock.Lock(_Caller);
      _Lock = theLock;
    }

    /// <summary>
    /// Снимает блокировку, установленную в конструкторе
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Lock != null)
        {
          _Lock.Unlock();
          _Lock = null;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заблокированный объект
    /// </summary>
    public ExecProcLock Lock { get { return _Lock; } }
    private ExecProcLock _Lock;

    /// <summary>
    /// Процедура, которая создала объект
    /// </summary>
    public ExecProc Caller { get { return _Caller; } }
    private readonly ExecProc _Caller;

    #endregion
  }
}
