﻿using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Logging;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Объект, желающий получать сигналы от таймера, должен реализовывать этот
  /// интерфейс.
  /// </summary>
  public interface IEFPAppTimeHandler
  {
    /// <summary>
    /// Вызывается 1 раз в секунду из основного потока приложения
    /// </summary>
    void TimerTick();
  }

  /// <summary>
  /// Поддержка таймеров в приложении
  /// Реализация свойства EFPApp.Timers
  /// Части программы, которым нужная обработка сигналов 1 раз в секунду или реже,
  /// могут добавить себя в список. При выходе из области действия они должны удалить
  /// себя из списка
  /// </summary>
  public sealed class EFPAppTimers : DisposableObject/*, ICollection<IEFPAppTimeHandler>*/
  {
    // Нельзя использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose

    internal EFPAppTimers()
    {
      _List = new List<IEFPAppTimeHandler>();
    }

    internal void InitTimer()
    {
      _TheTimer = new System.Windows.Forms.Timer();
      _TheTimer.Interval = 1000;
      _TheTimer.Tick += new EventHandler(TheTimer_Tick);
      _TheTimer.Enabled = true;
      _ExecutingHandlers = new List<IEFPAppTimeHandler>();
    }

    /// <summary>
    /// Удаляет таймер
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (_TheTimer != null)
      {
        _TheTimer.Dispose();
        _TheTimer = null;
        _ExecutingHandlers.Clear();
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Список объектов

    private List<IEFPAppTimeHandler> _List;

    private IEFPAppTimeHandler[] _Array;

    /// <summary>
    /// Добавить получатель сигналов.
    /// Этот метод является асинхронным
    /// </summary>
    /// <param name="handler">Добавляемый объект-обработчик сигнала</param>
    public void Add(IEFPAppTimeHandler handler)
    {
#if DEBUG
      if (handler == null)
        throw new ArgumentNullException("handler");
#endif

      lock (_List)
      {
        _List.Add(handler);
        _Array = null;
      }
    }

    /// <summary>
    /// Удалить получатель сигналов
    /// Этот метод является асинхронным, может вызываться из любого потока или
    /// из обработчика сигнала таймера
    /// </summary>
    /// <param name="handler">Удаляемый объект-обработчик сигнала</param>
    public bool Remove(IEFPAppTimeHandler handler)
    {
      lock (_List)
      {
        if (_List.Remove(handler))
        {
          _Array = null;
          return true;
        }
        else
          return false;
      }
    }

    /// <summary>
    /// Получить список действующих сейчас обработчиков
    /// </summary>
    /// <returns>Массив ссылок на объекты</returns>
    public IEFPAppTimeHandler[] ToArray()
    {
      lock (_List)
      {
        if (_Array == null)
          _Array = _List.ToArray();
        return _Array;
      }
    }

    #endregion

    #region Таймер

    /// <summary>
    /// Счетчик тиков таймера
    /// Его можно используя для получения делителей, если обработка нужна реже, чем
    /// 1 раз в секунду. Счетчик меняется 0 до Int32.MaxValue
    /// </summary>
    public int TickCount { get { return _TickCount; } }
    private int _TickCount;

    private System.Windows.Forms.Timer _TheTimer;

    /// <summary>
    /// Этот метод является реентерабелельным, но вызывается всегда из основного потока приложения
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    void TheTimer_Tick(object sender, EventArgs args)
    {
      try
      {
#if DEBUG
        EFPApp.CheckMainThread();
#endif

        if (EFPApp.IdleSuspended)
          return; // 18.08.2021

        EFPApp.ProcessDelayedTempMessage();

        // В процессе перебора могут добавляться или удаляться обработчики
        unchecked { _TickCount++; }
        if (_TickCount < 0)
          _TickCount = 0; // реально до этого дело никогда не дойдет - надо 68 лет

        IEFPAppTimeHandler[] a = ToArray();
        for (int i = 0; i < a.Length; i++)
        {
          #region Предотвращение вложенного вызова

          bool Found = false;
          for (int j = 0; j < _ExecutingHandlers.Count; j++)
          {
            if (Object.ReferenceEquals(_ExecutingHandlers[j], a[i]))
            {
              Found = true;
              break;
            }
          }
          if (Found)
            continue;

          #endregion

          int LastCnt = _ExecutingHandlers.Count;
          _ExecutingHandlers.Add(a[i]);
          try
          {
            a[i].TimerTick();
          }
          catch (Exception e)
          {
            try
            {
              e.Data["IEFPTimeHandler"] = a[i].ToString();
            }
            catch { }
            DebugTools.LogoutException(e, "Ошибка при вызове IEFPTimeHandler.TimerTick");
          }

          // 07.05.2017
          // Обработчики могут удаляться не только в конце списка
          if (_ExecutingHandlers.Count == (LastCnt + 1))
          {
            if (Object.ReferenceEquals(_ExecutingHandlers[LastCnt], a[i]))
            {
              // Список ExecutingHandlers не поменялся.
              // Быстрое удаление
              _ExecutingHandlers.RemoveAt(LastCnt);
              continue;
            }
            // Список ExecutingHandlers поменялся.
            // Медленное удаление
            _ExecutingHandlers.Remove(a[i]);
          }
        }

        ProcessIdle();
      }
      catch (Exception e)
      {
        DebugTools.LogoutException(e, "Ошибка при обработке сигнала таймера");
      }
    }

    private void ProcessIdle()
    {
      if (TickCount%3!=0) // делитель
        return;

      if (EFPApp.IdleHandlers.IsDisposed)
        return;
      if (!EFPApp.IdleHandlers.IdleCalledFlag)
        EFPApp.IdleHandlers.Application_Idle(null, null);
      EFPApp.IdleHandlers.IdleCalledFlag = false;
    }

    /// <summary>
    /// Предотвращение повторного вызова "долгого" обработчика, если пришел повторный сигнал таймера.
    /// Список содержит элементы, которые сейчас обрабатываются
    /// </summary>
    private List<IEFPAppTimeHandler> _ExecutingHandlers;

    #endregion
  }

  /// <summary>
  /// Отложенная обработка событий по сигналу таймера.
  /// Используется, если некоторое событие может случиться несколько раз за короткий период времени.
  /// Требуется однократная обработка события, например сохранение данных в файле, но нет событий BeginUpdate/EndUpdate.
  /// Использование:
  /// 1. Создать EFPDelayedTrigger.
  /// 2. Присоединить обработчик события Tick, который будет обрабатывать событие (сохранять файл)
  /// 3. При получении внешнего события устанавливать свойство Active=true.
  /// Не надо присоединять этот объект к EFPApp.Timers. Это выполняется автоматически.
  /// </summary>
  public class EFPDelayedTrigger : IEFPAppTimeHandler, IEFPAppIdleHandler
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект с Delay=0
    /// </summary>
    public EFPDelayedTrigger()
    {
    }

    #endregion

    #region Событие Tick

    /// <summary>
    /// Это событие вызывается однократно после установки Active=true.
    /// Событие вызывается с некоторой задержкой.
    /// Обработчик события может снова установить свойство Active=true, например, если в данный момент действие не может быть выполнено.
    /// </summary>
    public event EventHandler Tick;

    /// <summary>
    /// Устанавливает свойство Active=false и Принудительно вызывает событие Tick
    /// </summary>
    public void PerformTick()
    {
      Active = false; // До вызова события. Обработчик может снова установить Active=true.
      if (Tick != null)
        Tick(this, EventArgs.Empty);
    }

    #endregion

    #region Свойство Active

    /// <summary>
    /// Установка значения true "взводит курок". После этого будет с задержкой вызвано событие Tick,
    /// после чего свойство будет сброшено в false.
    /// </summary>
    public bool Active
    {
      get { return _Active; }
      set
      {
        if (value)
        {
          _SkipCounter = _InitialSkipCounter;
          if (!_Active)
          {
            _Active = true;
            if (Delay == 0)
              EFPApp.IdleHandlers.Add(this);
            else
              EFPApp.Timers.Add(this);
          }
        }
        else
        {
          if (_Active)
          {
            if (Delay == 0)
              EFPApp.IdleHandlers.Remove(this);
            else
              EFPApp.Timers.Remove(this);
            _Active = false;
          }
        }
      }
    }
    private bool _Active;

    /// <summary>
    /// Счетчик пропускаемых тиков инициализируется при каждой установке свойства Active=true
    /// </summary>
    private int _SkipCounter;

    #endregion

    #region Свойство Delay

    /// <summary>
    /// Минимальная задержка в миллисекуедах между установкой свойство Active=true и приходом
    /// сигнала Tick.
    /// Так как используемый таймер в EFPAppTimers имеет низкую частоту, реальная задержка может быть
    /// значительно больше.
    /// Значение по умолчанию - 0 - событие Tick вызывается при обработке события Idle.
    /// Нельзя устанавливать свойство, если Active=true.
    /// </summary>
    public int Delay
    {
      get { return _Delay; }
      set
      {
        if (Active)
          throw new InvalidOperationException("Нельзя устанавливать свойство Delay, когда свойство Active=true");
        if (value < 0 || value > 86400000)
          throw new ArgumentOutOfRangeException();
        _Delay = value;
        _InitialSkipCounter = (value + 999) / 1000;
      }
    }
    private int _Delay;

    private int _InitialSkipCounter;

    #endregion

    #region IEFPAppTimeHandler Members

    void IEFPAppTimeHandler.TimerTick()
    {
      _SkipCounter--;
      if (_SkipCounter < 0)
        PerformTick();
    }

    #endregion

    #region IEFPAppIdleHandler Members

    /// <summary>
    /// Реализация IEFPAppIdleHandler.
    /// </summary>
    public void HandleIdle()
    {
      PerformTick();
    }

    #endregion
  }

  /*
   * 28.09.2017
   * Обработка сигналов Idle управляющими элементами
   * Важно для EFPDataGridView. Хочется вызывать OnCurrentCellChanged не из обработчика события,
   * а позже. Если OnCurrentCellChanged сам что-нибудь делает с табличным просмотрои и вызывает
   * Application.DoEvents(), может возникнуть Application.ThreadException
   * 
   * Свойство EFPApp.IdleHandlers содержит список объектов, реализующих интерфейс IEFPAppIdleHandler.
   * При вызове события Application.Idle, по списку вызывается метод IEFPAppIdleHandler.HandleIdle()
   * Когда ничего не происходит, событие Application.Idle вызывается только один раз. Чтобы HandleIdle
   * вызывался чаще, обработчик таймера, который 1 раз в секунду, проверяет, было ли событие событие Idle
   * с предыдущей проверки и, если не было, вызывает HandleIdle. 
   * 
   * Если управляющий элемент хочет обрабатывать Idle, должно быть установлено в true свойство
   * EFPControlBase.UseIdle. При этом EFPControlBase добавляется в список EFPApp.IdleHandlers. Он
   * автоматически удаляется оттуда, когда элемент удаляется, поэтому метод виртуальный метод
   * EFPControlBase.HandleIdle() вызывается, только если элемент выведен на экран.
   * 
   * Обработчик события DataGridView.CurrentCellChanged не вызывает немедленно OnCurrentCellChanged().
   * Вместо этого, просто устанавливается внутренний флаг в EFPDataGridView. HandleIdle() уже вызывает
   * OnCurrentCellChanged()
   * 
   * В процессе обработки Idle обработчики могут вызывать Application.DoEvents, в результате событие
   * Application.Idle будет вызвано рекурсивно. Это предотвращается
   */

  /// <summary>
  /// Объект, желающий получать сигналы от таймера, должен реализовывать этот
  /// интерфейс.
  /// </summary>
  public interface IEFPAppIdleHandler
  {
    /// <summary>
    /// Периодически вызывается из основного потока приложения
    /// </summary>
    void HandleIdle();
  }

  /// <summary>
  /// Поддержка таймеров в приложении
  /// Реализация свойства EFPApp.IdleHandlers
  /// Части программы, которым нужная обработка события Idle,
  /// могут добавить себя в список. При выходе из области действия они должны удалить
  /// себя из списка
  /// </summary>
  public sealed class EFPAppIdleHandlers : DisposableObject/*, ICollection<IEFPAppTimeHandler>*/
  {
    // Нельзя использовать SimpleDisposableObject в качестве базового класса


    #region Конструктор и Dispose

    internal EFPAppIdleHandlers()
    {
      _List = new List<IEFPAppIdleHandler>();
      _ExecutingHandlers = new List<IEFPAppIdleHandler>();
      _SingleActionList = new List<EventHandler>();
      System.Windows.Forms.Application.Idle += new EventHandler(Application_Idle);
    }

    /// <summary>
    /// Отключает обработчик события Application.Idle
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      System.Windows.Forms.Application.Idle -= new EventHandler(Application_Idle);

      if (disposing)
        _ExecutingHandlers.Clear();

      base.Dispose(disposing);
    }

    #endregion

    #region Список объектов

    private List<IEFPAppIdleHandler> _List;

    private IEFPAppIdleHandler[] _Array;

    /// <summary>
    /// Добавить получатель сигналов.
    /// Этот метод является асинхронным
    /// </summary>
    /// <param name="handler">Добавляемый объект-обработчик сигнала</param>
    public void Add(IEFPAppIdleHandler handler)
    {
#if DEBUG
      if (handler == null)
        throw new ArgumentNullException("handler");
#endif

      lock (_List)
      {
        _List.Add(handler);
        _Array = null;
      }
    }

    /// <summary>
    /// Удалить получатель сигналов
    /// Этот метод является асинхронным, может вызываться из любого потока или
    /// из обработчика сигнала таймера
    /// </summary>
    /// <param name="handler">Удаляемый объект-обработчик сигнала</param>
    public bool Remove(IEFPAppIdleHandler handler)
    {
      lock (_List)
      {
        if (_List.Remove(handler))
        {
          _Array = null;
          return true;
        }
        else
          return false;
      }
    }

    /// <summary>
    /// Получить список действующих сейчас обработчиков
    /// </summary>
    /// <returns>Массив ссылок на объекты</returns>
    public IEFPAppIdleHandler[] ToArray()
    {
      lock (_List)
      {
        if (_Array == null)
          _Array = _List.ToArray();
        return _Array;
      }
    }

    #endregion

    #region Однократно вызываемые обработчики

    /// <summary>
    /// Список однократных действий
    /// </summary>
    private List<EventHandler> _SingleActionList;

    /// <summary>
    /// Флаг наличия однократных действий.
    /// В отличие от основного списка EFPAppIdleHandler, однократные действия появляются очень редко,
    /// и лучше обойтись без лишней блокировки
    /// </summary>
    volatile bool _HasSingleActions;

    /// <summary>
    /// Добавляет в список обработчик однократно вызываемого события.
    /// Делегат будет вызван при ближайшем событии Idle, после чего удален из списка
    /// </summary>
    /// <param name="handler">Обработчик события</param>
    public void AddSingleAction(EventHandler handler)
    {
      if (handler == null)
        throw new ArgumentException("handler");

      lock (_SingleActionList)
      {
        _SingleActionList.Add(handler);
        _HasSingleActions = true;
      }
    }

    #endregion

    #region Idle

    /// <summary>
    /// Этот флаг устанавливается при вызове Application_Idle().
    /// Обработчик события таймера проверяет, если флаг не установлен, то вызывает Application_Idle().
    /// Затем флаг сбрасывается
    /// </summary>
    internal bool IdleCalledFlag;

    /// <summary>
    /// Этот метод является реентерабелельным, но вызывается всегда из основного потока приложения
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    internal void Application_Idle(object sender, EventArgs args)
    {
      if (!EFPApp.AppWasInit)
        return; // 18.06.2018

      if (EFPApp.IdleSuspended)
        return; // 18.08.2021

      try
      {
#if DEBUG
        EFPApp.CheckMainThread();
#endif

        #region Обработка однократных действий

        if (_HasSingleActions)
        {
          _HasSingleActions = false;
          EventHandler[] singleActionArray;
          lock (_SingleActionList)
          {
            singleActionArray = _SingleActionList.ToArray();
            _SingleActionList.Clear();
          }

          for (int i = 0; i < singleActionArray.Length; i++)
          {
            try
            {
              singleActionArray[i](this, EventArgs.Empty);
            }
            catch (Exception e)
            {
              LogoutTools.LogoutException(e, "Ошибка вызова обработчика однократного Idle-действия");
            }
          }
        }

        #endregion

        EFPApp.CommandItems.HandleIdle(); // 28.01.2021

        // В процессе перебора могут добавляться или удаляться обработчики
        IEFPAppIdleHandler[] a = ToArray();
        for (int i = 0; i < a.Length; i++)
        {
          #region Предотвращение вложенного вызова

          bool Found = false;
          for (int j = 0; j < _ExecutingHandlers.Count; j++)
          {
            if (Object.ReferenceEquals(_ExecutingHandlers[j], a[i]))
            {
              Found = true;
              break;
            }
          }
          if (Found)
            continue;

          #endregion

          int LastCnt = _ExecutingHandlers.Count;
          _ExecutingHandlers.Add(a[i]);
          try
          {
            a[i].HandleIdle();
          }
          catch (Exception e)
          {
            try
            {
              e.Data["IEFPIdleHandler"] = a[i].ToString();
            }
            catch { }
            DebugTools.LogoutException(e, "Ошибка при вызове IEFPIdleHandler.HandleIdle");
          }

          // Обработчики могут удаляться не только в конце списка
          if (_ExecutingHandlers.Count == (LastCnt + 1))
          {
            if (Object.ReferenceEquals(_ExecutingHandlers[LastCnt], a[i]))
            {
              // Список ExecutingHandlers не поменялся.
              // Быстрое удаление
              _ExecutingHandlers.RemoveAt(LastCnt);
              continue;
            }
            // Список ExecutingHandlers поменялся.
            // Медленное удаление
            _ExecutingHandlers.Remove(a[i]);
          }
        }


        EFPApp.FileExtAssociations.ResetFA(); // TODO: пока не будет отслеживание изменений
      }
      catch (Exception e)
      {
        DebugTools.LogoutException(e, "Ошибка при обработке сигнала таймера");
      }

      IdleCalledFlag = true;
    }

    /// <summary>
    /// Предотвращение повторного вызова "долгого" обработчика, если пришел повторный сигнал таймера.
    /// Список содержит элементы, которые сейчас обрабатываются
    /// </summary>
    private List<IEFPAppIdleHandler> _ExecutingHandlers;

    #endregion
  }
}
