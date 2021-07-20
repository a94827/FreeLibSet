// Это может быть определено в свойствах проекта
// #define DEBUG_SPLASHWATCHERS // Если определено, то будет выполняться трассировка ServerSplashWatcher и ClientSplashWatcher

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

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

namespace AgeyevAV
{

  /// <summary>
  /// Порция данных, передаваемая от сервера к клиенту.
  /// Это - пустой базовый класс.
  /// Данные передаются с помощью производных классов 
  /// </summary>
  [Serializable]
  public abstract class SplashInfoPack
  {
    #region Защищенный конструктор

    internal SplashInfoPack()
    {
    }

    #endregion

#if DEBUG_SPLASHWATCHERS
    internal Guid ServerWatcherGuid;
    internal int ServerCallId;
#endif
  }

  [Serializable]
  internal class SplashInfoPackCurrentPhase : SplashInfoPack
  {
    #region Поля

    public string PhaseText;

    public int Percent;

    public int PercentMax;

    public bool AllowCancel;

    #endregion
  }

  [Serializable]
  internal class SplashInfoPackSplash : SplashInfoPackCurrentPhase
  {
    #region Поля

    public SplashPhaseState[] PhaseStates;

    #endregion

    #region Методы

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append("PhaseStates={");
      for (int i = 0; i < PhaseStates.Length; i++)
      {
        if (i > 0)
          sb.Append("|");
        sb.Append(PhaseStates[i].ToString());
      }
      sb.Append("}");
      return sb.ToString();
    }

    #endregion
  }

  [Serializable]
  internal class SplashInfoPackStackItem : SplashInfoPackSplash
  {
    #region Поля

    public string[] Phases;

    public int StackSerialId;

    #endregion

    #region Методы

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append(", StackSerialId=");
      sb.Append(StackSerialId);
      sb.Append(", Phases={");
      sb.Append(String.Join("|", Phases));
      sb.Append("}");

      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Информация о полном стеке заставок
  /// </summary>
  [Serializable]
  internal class SplashInfoPackStack : SplashInfoPack
  {
    #region Поля

    /// <summary>
    /// Описания заставок в стеке.
    /// Порядок заставок соответствует принятому в  Stack.ToArray().
    /// Текущая заставка имеет индекс 0.
    /// </summary>
    public SplashInfoPackStackItem[] Stack;

    #endregion

    #region Методы

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append(", SplashCount=");
      sb.Append(Stack.Length);
      sb.Append(" {");
      for (int i = 0; i < Stack.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        sb.Append("{StackSerialId=");
        sb.Append(Stack[i].StackSerialId);
        sb.Append(", Phases={");
        sb.Append(String.Join("|", Stack[i].Phases));
        sb.Append("}");
      }
      sb.Append("}");

      return sb.ToString();
    }

    #endregion
  }

#if DEBUG_SPLASHWATCHERS

  /// <summary>
  /// Для отладки передаем не null, а объект-заглушку, когда в заставке нет никаких изменений
  /// </summary>
  [Serializable]
  internal class SplashInfoPackNone : SplashInfoPack
  {
  }

#endif


#if XXX
  /// <summary>
  /// Очередь для хранения объектов SplashInfoPack.
  /// Используется при асинхронных запросах.
  /// Этот класс не является потокобезопасным.
  /// </summary>
  public sealed class SplashInfoPackQueue
  {
  #region Конструктор

    public SplashInfoPackQueue()
    {
      _Queue = new Queue<SplashInfoPack>();
    }

  #endregion

  #region Очередь

    public void Enqueue(SplashInfoPack pack)
    { 
#if DEBUG_SPLASHWATCHERS
      if (pack == null)
        throw new ArgumentNullException("pack");
#endif

      if (pack == null)
        return;

      // TODO: Нужно более интеллектуальное удаление неактуальных объектов

      _Queue.Enqueue(pack);
    }

    public SplashInfoPack Dequeue()
    {
      if (_Queue.Count == null)
      {
#if DEBUG_SPLASHWATCHERS
        return new SplashInfoPackNone();
#else
        return null;
#endif
      }
      else
        return _Queue.Dequeue();
    }

    private Queue<SplashInfoPack> _Queue;

  #endregion
  }
#endif

  /// <summary>
  /// Интерфейс, реализуемый ServerSplashWatcher
  /// </summary>
  public interface IServerSplashWatcher
  {
    #region Методы

    /// <summary>
    /// Получить очередную порцию данных об изменениях в стеке заставок.
    /// При первом вызове метода или после ResetSplashIOfo() возвращает полную информацию о стеке.
    /// При повторных обращениях возвращает неполные данные или даже null, если изменений не было.
    /// Структура передаваемых данных SplashInfoPack является закрытой и используется исключительно для передачи данных
    /// от ServerSplashWatcher к ClientSplashWatcher.
    /// </summary>
    /// <returns>Порция данных</returns>
    SplashInfoPack GetSplashInfoPack();

    /// <summary>
    /// Сброс данных.
    /// После вызова этого метода, при следующем вызове GetSplashInfoPack() будет возвращена полная информация о стеке заставок.
    /// Используется для исправления ошибок после разрыва соединения.
    /// </summary>
    void ResetSplashInfo();

    /// <summary>
    /// Прервать выполнение процесса.
    /// Метод устанавливает свойство ISplash.Cancelled=true, если в стеке есть текущая заставка
    /// </summary>
    void Cancel();

    #endregion
  }

  /// <summary>
  /// Прослушиватель стека заставок на стороне сервера
  /// </summary>
  public sealed class ServerSplashWatcher : MarshalByRefObject, IServerSplashWatcher
  {
    #region Конструктор

    /// <summary>
    /// Создает прослушиватель для заданного стека заставок
    /// </summary>
    /// <param name="source">Стек заставок</param>
    public ServerSplashWatcher(ISplashStack source)
    {
      if (source == null)
        throw new ArgumentNullException();

      _Source = source;

#if DEBUG_SPLASHWATCHERS

      _WatcherGuid = Guid.NewGuid();
      Trace.WriteLine("ServerSplashWatcher " + _WatcherGuid.ToString() + " created");
#endif
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Прослушиваемый стек
    /// </summary>
    public ISplashStack Source { get { return _Source; } }
    private ISplashStack _Source;

#if DEBUG_SPLASHWATCHERS

    /// <summary>
    /// Для отладки - идентификатор объекта.
    /// Неудобно использовать числовые идентификаторы, так объекты создаются в разных AppDomain и разных процессах
    /// </summary>
    private Guid _WatcherGuid;

    /// <summary>
    /// Для отладки - счетчик вызовов GetSplashInfoPack()
    /// </summary>
    private int GetSplashInfoPackCount;

#endif

    #endregion

    #region IServerSplashWatcher Members

    /// <summary>
    /// Используется для вызова метода GetSplashStack()
    /// </summary>
    private int _PrevStackVersion;

    /// <summary>
    /// Номер текущей фазы при предыдущем вызове
    /// </summary>
    private int _PrevPhase;

    /// <summary>
    /// Информация о текущей фазе
    /// </summary>
    private SplashInfoPackCurrentPhase _PrevCurrentPhase;

    /// <summary>
    /// Ссылка на текущую заставку.
    /// Нельзя запрашивать свойство ISplashStack.Splash, т.к. после вызова GetSplashStack() могут быть асинхронные изменения в стеке
    /// </summary>
    private ISplash _PrevSplash;

    /// <summary>
    /// Основной метод - получение порции данных
    /// </summary>
    /// <returns></returns>
    public SplashInfoPack GetSplashInfoPack()
    {
      SplashInfoPack Pack = DoGetSplashInfoPack();

#if DEBUG_SPLASHWATCHERS

      string sPack;
      if (Pack == null)
        sPack = "null";
      else
        sPack = Pack.ToString();

      unchecked { GetSplashInfoPackCount++; }

      if (Pack != null)
      {
        Pack.ServerWatcherGuid = _WatcherGuid;
        Pack.ServerCallId = GetSplashInfoPackCount;
      }

      Trace.WriteLine("ServerSplashWatcher " + _WatcherGuid.ToString() + ". GetSplashInfoPack() # " + GetSplashInfoPackCount.ToString() + ". Returning " + sPack);
#endif

      return Pack;
    }

    private SplashInfoPack DoGetSplashInfoPack()
    {
      ISplash[] stack = _Source.GetSplashStack(ref _PrevStackVersion);
      if (stack != null)
      {
        #region 1. Возвращаем полную информацию о состоянии стека

        SplashInfoPackStack res1 = new SplashInfoPackStack();

        res1.Stack = new SplashInfoPackStackItem[stack.Length];
        for (int i = 0; i < stack.Length; i++)
        {
          res1.Stack[i] = new SplashInfoPackStackItem();
          res1.Stack[i].Phases = stack[i].Phases;
          res1.Stack[i].StackSerialId = stack[i].StackSerialId;
          res1.Stack[i].PhaseStates = stack[i].GetPhaseStates();
          InitCurrentPhase(stack[i], res1.Stack[i]);
        }

        if (stack.Length > 0)
        {
          _PrevPhase = stack[0].Phase;
          _PrevCurrentPhase = res1.Stack[0];
          _PrevSplash = stack[0];
        }
        else
        {
          _PrevPhase = -1;
          _PrevCurrentPhase = null;
          _PrevSplash = null;
        }

        return res1;

        #endregion
      }

      // Не было добавлено/убрано заставок с предудущего вызова
      if (_PrevSplash == null)
      {
        // заставок не было и нет. Возвращать нечего
#if DEBUG_SPLASHWATCHERS
        return new SplashInfoPackNone();
#else
        return null;
#endif
      }

      if (_PrevSplash.Phase != _PrevPhase)
      {
        #region 2. Возвращаем информацию о текущей заставке

        SplashInfoPackSplash res2 = new SplashInfoPackSplash();
        res2.PhaseStates = _PrevSplash.GetPhaseStates();
        InitCurrentPhase(_PrevSplash, res2);
        _PrevPhase = _PrevSplash.Phase;

        return res2;

        #endregion
      }

      #region 3. Возвращаем информцию о текущей фазе или null

      SplashInfoPackCurrentPhase res3 = new SplashInfoPackCurrentPhase();
      InitCurrentPhase(_PrevSplash, res3);

      if (res3.PercentMax == _PrevCurrentPhase.PercentMax &&
        res3.Percent == _PrevCurrentPhase.Percent &&
        res3.AllowCancel == _PrevCurrentPhase.AllowCancel &&
        res3.PhaseText == _PrevCurrentPhase.PhaseText)

        // ничего не изменилось
#if DEBUG_SPLASHWATCHERS
        return new SplashInfoPackNone();
#else
        return null;
#endif
      else
      {
        // что-то изменилось
        _PrevCurrentPhase = res3;
        return res3;
      }

      #endregion
    }

    private void InitCurrentPhase(ISplash src, SplashInfoPackCurrentPhase res)
    {
      res.PercentMax = src.PercentMax;
      res.Percent = src.Percent;
      res.AllowCancel = src.AllowCancel;
      res.PhaseText = src.PhaseText;
    }

    /// <summary>
    /// После вызова этого метода, следующий вызов GetSplashInfoPack() вернет полную информацию о стеке заставок.
    /// Метод используется для восстановления после ошибок соединения клиента с сервером.
    /// </summary>
    public void ResetSplashInfo()
    {
      _PrevStackVersion = 0;
    }

    /// <summary>
    /// Устанавливает свойство ISplash.Cancelled=true, если стек заставок не пустой
    /// </summary>
    public void Cancel()
    {
      ISplash spl = _Source.Splash;
      if (spl != null)
        spl.Cancelled = true;
    }

    #endregion
  }

  /// <summary>
  /// Парный объект для ServerSplashWatcher.
  /// Дополнительно позволяет выводить заставку на время, пока ServerSplashWatcher не предоставляет заставки
  /// Этот объект не является потокобезопасным. Все методы должны вызываться из одного потока, в котором 
  /// был вызван конструктор объекта
  /// </summary>
  public sealed class ClientSplashWatcher
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, который подсоединяется к ServerSplashWatcher для получения от него данных и управляющий стеком заставок на стороне клиента
    /// </summary>
    /// <param name="serverWatcher">Серверная часть, поставляющая данные</param>
    /// <param name="clientStack">Стек заставок на стороне клиента, которыми нужно управлять</param>
    public ClientSplashWatcher(IServerSplashWatcher serverWatcher, ISplashStack clientStack)
    {
      if (serverWatcher == null)
        throw new ArgumentNullException("serverWatcher");
      if (clientStack == null)
        throw new ArgumentNullException("clientStack");
      _ServerWatcher = serverWatcher;
      _ClientStack = clientStack;

      _ClientSplashes = new Stack<ISplash>();

#if DEBUG_SPLASHWATCHERS
      _WatcherGuid = Guid.NewGuid();
      Trace.WriteLine("ClientSplashWatcher " + _WatcherGuid.ToString() + " created");
#endif

      _UseDefaultSplash = false;
      _DefaultSplashPhaseText = SplashTools.DefaultSplashText;

#if DEBUG
      _TheThread = Thread.CurrentThread;
#endif
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Серверная часть, поставляющая данные
    /// </summary>
    public IServerSplashWatcher ServerWatcher { get { return _ServerWatcher; } }
    private IServerSplashWatcher _ServerWatcher;

    /// <summary>
    /// Стек заставок на стороне клиента, которыми нужно управлять
    /// </summary>
    public ISplashStack ClientStack { get { return _ClientStack; } }
    private ISplashStack _ClientStack;

#if DEBUG_SPLASHWATCHERS

    /// <summary>
    /// Для отладки - идентификатор объекта
    /// </summary>
    private Guid _WatcherGuid;

    /// <summary>
    /// Счетчик вызовов ProcessUpdateStack()
    /// </summary>
    private int ProcessUpdateStackCount;

#endif

    #endregion

    #region Метод ProcessSplash

    /// <summary>
    /// Этот метод должен периодически вызываться по таймеру.
    /// 1. Вызывает ServerSplashWatcher.GetSplashPackInfo() и обновляет состояние заставок с помощью ISplashStack
    /// 2. Проверяет ISplashStack.Cancelled и, при необходимости, вызывает ServerSplashWatcher.Cancel()
    /// </summary>
    public void ProcessSplash()
    {
#if DEBUG
      CheckThread();
#endif

      try
      {
        ProcessUpdateStack();
      }
      catch
      {
        ResetSplashInfo();
      }
      ShowDefaultSplash();
      ProcessCheckCancelled();
    }

    [DebuggerStepThrough]
    private void ResetSplashInfo()
    {
      try
      {
        _ServerWatcher.ResetSplashInfo();
      }
      catch { }
    }

    /// <summary>
    /// Вместо того, чтобы запрашивать заставки вызовом ClientStack.GetSplashStack(), храним копию списка.
    /// Здесь нет заставки "Идет процесс".
    /// Активная заставка находится на вершине
    /// </summary>
    private Stack<ISplash> _ClientSplashes;

    private void ProcessUpdateStack()
    {
      SplashInfoPack pack = GetSplashInfoPack(); // запрос данных по сети

#if DEBUG_SPLASHWATCHERS

      if (pack == null)
        throw new NullReferenceException("В отладочном режиме SplashInfoPack не может быть null");

      string sPack = "From " + pack.ServerWatcherGuid + ", CallId=" + pack.ServerCallId + ". " + pack.ToString();

      unchecked { ProcessUpdateStackCount++; }

      // Могут быть пустые сообщения в очереди
      //if (ProcessUpdateStackCount != pack.ServerCallId)
      //  throw new BugException("SplashWatcher call missmatch. ServerCallId=" + pack.ServerCallId.ToString() + ", ClientCallId=" + ProcessUpdateStackCount.ToString());

      Trace.WriteLine("ClientSplashWatcher " + _WatcherGuid.ToString() + ". ProcessUpdateStack() # " + ProcessUpdateStackCount.ToString() + ". Recieved " + sPack);
#endif

      if (pack == null)
        return; // нет изменений

      SplashInfoPackStack pack1 = pack as SplashInfoPackStack;
      if (pack1 != null)
      {
        // получили полный стек заставок 

        // Убираем свою заставку
        if (pack1.Stack.Length > 0 && _DefaultSplash != null)
        {
          _ClientStack.EndSplash();
          _DefaultSplash = null;
        }

        // Убираем закрытые заставки, которых нет в стеке
        // - по количеству
        while (_ClientSplashes.Count > pack1.Stack.Length)
        {
          _ClientStack.EndSplash();
          _ClientSplashes.Pop();
        }

        // - по соответствию
        for (int i = (pack1.Stack.Length - _ClientSplashes.Count); i < pack1.Stack.Length; i++)
        {
          if (_ClientSplashes.Peek().StackSerialId == pack1.Stack[i].StackSerialId)
            break;
          else
          {
            _ClientStack.EndSplash();
            _ClientSplashes.Pop();
          }
        }

        // Добавляем недостающие вкладки
        for (int i = pack1.Stack.Length - _ClientSplashes.Count - 1; i >= 0; i--)
        {
          ISplash spl = _ClientStack.BeginSplash(pack1.Stack[i].Phases);
          spl.StackSerialId = pack1.Stack[i].StackSerialId; // переопределили
          _ClientSplashes.Push(spl);
        }

        if (_ClientSplashes.Count != pack1.Stack.Length)
          throw new BugException("Расхождение количества заставок");

        ISplash[] ClientSplashes2 = _ClientSplashes.ToArray();
        // Инициализируем состояние всех заставок
        for (int i = 0; i < pack1.Stack.Length; i++)
          InitSplashStates(pack1.Stack[i], ClientSplashes2[i]);

        return;
      }

      SplashInfoPackSplash pack2 = pack as SplashInfoPackSplash;
      if (pack2 != null)
      {
        // Получена информация об обновлении одной заставки
        if (_ClientSplashes.Count == 0)
          throw new BugException("Обработка SplashInfoPackSplash. В списке _ClientSplashes нет заставок");
        InitSplashStates(pack2, _ClientSplashes.Peek());
        return;
      }

      SplashInfoPackCurrentPhase pack3 = pack as SplashInfoPackCurrentPhase;
      if (pack3 != null)
      {
        // Получена информация об обновлении процентного индикатора
        if (_ClientSplashes.Count == 0)
          throw new BugException("Обработка SplashInfoPackCurrentPhase. В списке _ClientSplashes нет заставок");
        InitSplashCurrentPhase(pack3, _ClientSplashes.Peek());
        return;
      }

#if DEBUG_SPLASHWATCHERS
      if (pack is SplashInfoPackNone) // затычка
        return;
#endif

      throw new BugException("Получен объект неизвестного типа " + pack.GetType().ToString());
    }

    /// <summary>
    /// Вызов IServerSplashWatcher.GetSplashInfoPack().
    /// Может возникнуть исключение, связанное с сетью.
    /// В этом случае, надо попытаться сбросить информацию о заставках
    /// </summary>
    /// <returns></returns>
    [DebuggerStepThrough]
    private SplashInfoPack GetSplashInfoPack()
    {
      try
      {
        return _ServerWatcher.GetSplashInfoPack();
      }
      catch
      {
        try
        {
          _ServerWatcher.ResetSplashInfo();
        }
        catch { }
        return null;
      }
    }

    /// <summary>
    /// Синхронизирует текущую фазу заставки с данными, полученными с сервера
    /// </summary>
    /// <param name="src">Информация о номере текущей фазы заставки</param>
    /// <param name="dst">Интерфейс заставки, которую требуется синхронизировать</param>
    private void InitSplashStates(SplashInfoPackSplash src, ISplash dst)
    {
      SplashPhaseState[] dstStates = dst.GetPhaseStates();
      if (dstStates.Length != src.PhaseStates.Length)
        throw new BugException("Разное количество фаз в заставке");
      for (int i = 0; i < dst.PhaseCount; i++)
      {
        if (dstStates[i] != src.PhaseStates[i])
        {
          if (src.PhaseStates[i] == SplashPhaseState.Complete || src.PhaseStates[i] == SplashPhaseState.Skipped)
          {
            if (dstStates[i] == SplashPhaseState.Current || dstStates[i] == SplashPhaseState.None)
            {
              // требуется перейти на следующую фазу
              if (src.PhaseStates[i] == SplashPhaseState.Complete)
                dst.Complete();
              else
                dst.Skip();
            }
            else
              break;
          }
        }
      }
      InitSplashCurrentPhase(src, dst);
    }


    /// <summary>
    /// Синхронизирует положение индикатора заставки с данными, полученными с сервера
    /// </summary>
    /// <param name="src">Информация о номере текущей фазы заставки</param>
    /// <param name="dst">Интерфейс заставки, которую требуется синхронизировать</param>
    private void InitSplashCurrentPhase(SplashInfoPackCurrentPhase src, ISplash dst)
    {
      if (dst.Phase >= dst.PhaseCount)
        return; // когда последняя фаза выполнена, уже нечего настраивать нельзя
      dst.SetPercent(src.Percent, src.PercentMax);
      dst.PhaseText = src.PhaseText;
      dst.AllowCancel = src.AllowCancel;
    }

    #endregion

    #region Заставка DefaultSplash

    /// <summary>
    /// Нужно ли выводить заставке "Идет процесс", если ServerSplashWatcher не возвращает ни одной заставки.
    /// По умолчанию - false - заставка не выводится.
    /// Если свойство устанавливается в true, то оно должно быть явно сброшено в false по окончании работы процесса,
    /// иначе заставка по умолчанию останется на экране.
    /// </summary>
    public bool UseDefaultSplash
    {
      get { return _UseDefaultSplash; }
      set
      {
#if DEBUG
        CheckThread();
#endif

        _UseDefaultSplash = value;
        if (!value) // 16.12.2019
          ShowDefaultSplash();
      }
    }
    private bool _UseDefaultSplash;

    /// <summary>
    /// Заставка "Идет процесс", если нет нормальных заставок
    /// </summary>
    private ISplash _DefaultSplash;

    /// <summary>
    /// Добавляем заставку "Идет процесс"
    /// </summary>
    private void ShowDefaultSplash()
    {
      if (_UseDefaultSplash)
      {
        if (_ClientSplashes.Count == 0 && _DefaultSplash == null)
        {
          _DefaultSplash = _ClientStack.BeginSplash(DefaultSplashPhaseText);

#if DEBUG_SPLASHWATCHERS
        Trace.WriteLine("ClientSplashWatcher " + _WatcherGuid.ToString() + ". Dummy splash created");
#endif
        }
      }
      else
      {
        if (_DefaultSplash != null)
        {
          _ClientStack.EndSplash();
          _DefaultSplash = null;
        }
      }
    }

    /// <summary>
    /// Текст заставки, выводимой по умолчанию при UseDefaultSplash=true.
    /// </summary>
    public string DefaultSplashPhaseText
    {
      get { return _DefaultSplashPhaseText; }
      set
      {
#if DEBUG
        CheckThread();
#endif

        if (value == null)
          value = SplashTools.DefaultSplashText;
        if (value == _DefaultSplashPhaseText)
          return;

        _DefaultSplashPhaseText = value;
        if (_DefaultSplash != null)
          _DefaultSplash.PhaseText = value;
      }
    }
    private string _DefaultSplashPhaseText;

    #endregion

    #region Нажатие кнопки "Отмена"

    /// <summary>
    /// Устанавливается в true, если был послан сигнал Cancel
    /// </summary>
    private bool _CancelSignalled;

    /// <summary>
    /// Проверка нажатия кнопки "Отмена"
    /// </summary>
    private void ProcessCheckCancelled()
    {
      ISplash spl = _ClientStack.Splash;
      if (spl == null)
        return;

      if (spl.AllowCancel && spl.Cancelled)
      {
        if (_CancelSignalled)
          return; // предотвращение повторной посылки сигнала

        // Посылаем сигнал
        _ServerWatcher.Cancel();
        _CancelSignalled = true;
      }
      else
        _CancelSignalled = false;
    }

    #endregion

    #region Очистка стека

    /// <summary>
    /// Выполняет вызов недостающих EndSplash().
    /// Перед вызовом этого метода, вероянтно, следует установить UseDefaultSplash=false
    /// </summary>
    public void ClearClientStack()
    {
#if DEBUG
      CheckThread();
#endif

      while (_ClientSplashes.Count > 0)
      {
        _ClientStack.EndSplash();
        _ClientSplashes.Pop();
      }
      ShowDefaultSplash();
    }

    #endregion

    #region Проверка потока

#if DEBUG

    private Thread _TheThread;

    private void CheckThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _TheThread))
        throw new DifferentThreadException(_TheThread);
    }

#endif

    #endregion
  }
}
