﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

//#define DEBUG_SPLASH

#if !DEBUG
#undef DEBUG_SPLASH
#endif

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using FreeLibSet.Logging;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Окно с процентным индикатором
  /// Окно может быть создано из любого потока, но дальнейшая вся работа должна выполняться из него же.
  /// Для созданного окна в блоке finally должен быть вызван метод <see cref="Close()"/> или можно использовать конструкцию using
  /// </summary>
  public class Splash : ISplash, IDisposable
  {
    #region Конструкторы и закрытие

    /// <summary>
    /// Эта версия конструктора создает заставку, состояющую из одной фазы.
    /// Текст заставки - "Идет процесс" (<see cref="SplashTools.DefaultSplashItems"/>)
    /// </summary>
    public Splash()
      : this(SplashTools.DefaultSplashItems)
    {
    }

    /// <summary>
    /// Эта версия конструктора создает заставку, состояющую из одной фазы.
    /// </summary>
    /// <param name="phase">Текст заставки</param>
    public Splash(string phase)
      : this(new string[] { phase })
    {
    }

    /// <summary>
    /// Эта версия конструктора создает заставку, состояющую из нескольких фаз
    /// </summary>
    /// <param name="phases">Строки фаз</param>
    public Splash(string[] phases)
      : this(phases, new SplashForm(), false)
    {
    }

    internal Splash(string[] phases, SplashForm form, bool isWizard)
    {
#if DEBUG
      // 14.07.2015
      // Может вызываться из любого потока, лишь бы из одного и того же
      //EFPApp.CheckTread();

      _DebugTheThread = Thread.CurrentThread;
#endif

      if (phases == null)
        throw new ArgumentNullException("phases");

#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("Form");
#endif

      _Phases = phases;
      _CurrentIndex = 0;

      int i;
      _Form = form;

#if DEBUG_SPLASH
      unchecked
      {
        DebugCounter++;
        _DebugSplashId = DebugCounter;
        _DebugStackTrace = Environment.StackTrace;
        _Form.Text += " #" + _DebugSplashId.ToString();
        _Form.btnDebug.Visible = true;
        _Form.btnDebug.Click += new EventHandler(btnDebug_Click);
      }
#endif

      // Заполняем список
      _Form.PhasesListView.Items.Clear();
      for (i = 0; i < _Phases.Length; i++)
        _Form.PhasesListView.Items.Add(_Phases[i], 0);

      _AllowCancel = false;
      _Form.Cancelled = false;
      _Form.btnCancel.Enabled = false;
      _Form.lblWaitCancel.Visible = false;

      DoSetItem();

      if (isWizard)
        _Form.btnCancel.Visible = false;
      else
      {
        EFPApp.ShowFormInternal(_Form);
        _Form.Update();
      }
    }

    /// <summary>
    /// Закрывает окно заставки
    /// </summary>
    public void Close()
    {
      if (_TheTimer != null)
      {
        _TheTimer.Dispose();
        _TheTimer = null;
      }

      if (_Form == null)
        return; // 20.07.2018

      if (_Form.IsDisposed)
      {
        // 06.12.2019
        _Form = null;
        return;
      }

      if (_Form.Visible)
        EFPApp.DefaultScreen = Screen.FromControl(_Form); // 16.03.2016

      try
      {
        _Form.Visible = false;
        _Form.Dispose();
      }
      catch (Exception e) // 07.03.2018
      {
        LogoutTools.LogoutException(e);
      }

      _Form = null;
      Application.DoEvents();
    }

#if DEBUG
    /// <summary>
    /// Форма заставки.
    /// Только для отладочных целей
    /// </summary>
    public Form DebugForm { get { return _Form; } }
#endif
    internal SplashForm _Form;

    /// <summary>
    /// Список названий фаз
    /// </summary>
    public string[] Phases { get { return _Phases; } }
    private readonly string[] _Phases;

    /// <summary>
    /// Возвращает количество фаз в заставке
    /// </summary>
    public int PhaseCount { get { return _Phases.Length; } }


    void IDisposable.Dispose()
    {
      Close();
    }

    #endregion

    #region Таймер

    /// <summary>
    /// Таймер для обновления с интервалом 0.5с.
    /// Используется для обновления свойств PhaseText, Percent и PercentMax.
    /// Создается при первой установке свойства PhaseText и PercentMax (отличное от 0).
    /// Используем таймер WinForms, т.к. событие возникает в том потоке, в котором создан таймер
    /// </summary>
    private System.Windows.Forms.Timer _TheTimer;

    private void InitTimer()
    {
      if (_TheTimer != null)
        return;
      if (_Form == null)
        return;

      _TheTimer = new System.Windows.Forms.Timer();
      _TheTimer.Tick += TheTimer_Tick;
      _TheTimer.Interval = 500;
      _TheTimer.Enabled = true;
    }

    void TheTimer_Tick(object sender, EventArgs args)
    {
      if (_Form == null)
        return;
      _Form.lblCurrent.Text = _PhaseText;

      /*if (_Form.lblCurrent.BackColor == Color.Yellow)
        _Form.lblCurrent.BackColor = Color.Green;
      else
        _Form.lblCurrent.BackColor = Color.Yellow;*/


      if (_PercentMax == 0)
        _Form.pb1.Visible = false;
      else
      {
        _Form.pb1.Maximum = _PercentMax;
        _Form.pb1.Value = _Percent;
        _Form.pb1.Visible = true;
      }
    }

    #endregion

    #region Текущая фаза

    /// <summary>
    /// Номер текущей фазы в диапазоне от 0 до <see cref="PhaseCount"/>.
    /// Установка свойства из прикладного кода не рекомендуется.
    /// Используйте методы <see cref="Complete()"/> и <see cref="Skip()"/>.
    /// </summary>
    public int Phase
    {
      get
      {
        return _CurrentIndex;
      }
      set
      {
        if (value == _CurrentIndex)
          return;
        if (value < _CurrentIndex || value >= _Phases.Length)
          throw ExceptionFactory.ArgOutOfRange("value", value, _CurrentIndex, _Phases.Length - 1);
        while (_CurrentIndex < value)
          Complete();
      }
    }

    /// <summary>
    /// Устанавливается в true при первой установке свойства PhaseText в текущей фазе.
    /// При этом выполняется немедленное обновление текста фазы, в отличие от последующих установок,
    /// которые отображаются только по таймеру
    /// </summary>
    private bool _PhaseTextHasBeenSetInPhase;

    /// <summary>
    /// Текст в рамке формы для текущей фазы
    /// </summary>
    public string PhaseText
    {
      get
      {
        return _PhaseText;
      }
      set
      {
        InitTimer();

        if (value == null)
        {
          if (_CurrentIndex < _Phases.Length)
            _PhaseText = _Phases[_CurrentIndex];
          else
            _PhaseText = String.Empty;
        }
        else
          _PhaseText = value;

        if (!_PhaseTextHasBeenSetInPhase)
        {
          _PhaseTextHasBeenSetInPhase = true;
          TheTimer_Tick(null, null); // 21.07.2022. Не дожидаясь появления события
        }

        Application.DoEvents();
      }
    }
    private string _PhaseText;

    /// <summary>
    /// Закрываем текущую фазу установкой отметки "Выполнено" и переходим к следующей
    /// </summary>
    public void Complete()
    {
      DoNextItem(true);
    }

    /// <summary>
    /// Закрываем текущую фазу установкой отметки "Отменено" и переходим к следующей
    /// </summary>
    public void Skip()
    {
      DoNextItem(false);
    }

    private int _CurrentIndex;

    /// <summary>
    /// Установка фазы на текущую позицию 
    /// </summary>
    private void DoSetItem()
    {
      ListViewItem li = _Form.PhasesListView.Items[_CurrentIndex];
      li.SubItems.Add(Res.Splash_Msg_Current);
      li.ImageIndex = 1;
      _Form.lblCurrent.Text = _Phases[_CurrentIndex];
      _PhaseText = _Phases[_CurrentIndex];
      _PercentMax = 0;
      _Percent = 0;
      _Form.pb1.Visible = false;
      //FCancelButton.EnabledEx = false;
      li.Focused = true;
      li.Selected = true;
      _Form.PhasesListView.EnsureVisible(_CurrentIndex);
      Application.DoEvents();
    }

    private void DoNextItem(bool isComplete)
    {
      if (_CurrentIndex < _Form.PhasesListView.Items.Count) // 07.09.2017
      {
        ListViewItem li = _Form.PhasesListView.Items[_CurrentIndex];
        if (isComplete)
        {
          li.SubItems[1].Text = Res.Splash_Msg_Complete;
          li.ImageIndex = 2;
        }
        else
        {
          li.SubItems[1].Text = Res.Splash_Msg_Skipped;
          li.ImageIndex = 3;
        }
        if (_CurrentIndex < (_Form.PhasesListView.Items.Count - 1))
        {
          _CurrentIndex++;
          DoSetItem();
        }
      }
      _PhaseTextHasBeenSetInPhase = false;
    }

    /// <summary>
    /// Возвращает состояние заданной фазы
    /// </summary>
    /// <param name="phase">Индекс фазы от 0 до <see cref="PhaseCount"/>-1</param>
    /// <returns>Состояние фазы</returns>
    public SplashPhaseState GetPhaseState(int phase)
    {
      if (phase < 0 || phase >= _Phases.Length)
        throw ExceptionFactory.ArgOutOfRange("phase", phase, 0, _Phases.Length);

      if (phase == _CurrentIndex)
        return SplashPhaseState.Current;
      if (phase > _CurrentIndex)
        return SplashPhaseState.None;

      ListViewItem li = _Form.PhasesListView.Items[phase];
      if (li.ImageIndex == 2)
        return SplashPhaseState.Complete;
      else
        return SplashPhaseState.Skipped;
    }

    /// <summary>
    /// Получить состояния для всех фаз
    /// </summary>
    /// <returns>Массив состояний</returns>
    public SplashPhaseState[] GetPhaseStates()
    {
      SplashPhaseState[] a = new SplashPhaseState[_Phases.Length];
      if (_CurrentIndex < _Phases.Length)
        a[_CurrentIndex] = SplashPhaseState.Current;

      for (int i = 0; i < _CurrentIndex; i++)
      {
        ListViewItem li = _Form.PhasesListView.Items[i];
        if (li.ImageIndex == 2)
          a[i] = SplashPhaseState.Complete;
        else
          a[i] = SplashPhaseState.Skipped;
      }
      return a;
    }

    #endregion

    #region Прерывание процесса

    /// <summary>
    /// Возможность прерывания процесса пользователем
    /// (доступность кнопки "Отмена"). По умолчанию - false.
    /// Если программа устанавливает свойство, она должна
    /// периодически опрашивать свойство <see cref="Cancelled"/>
    /// </summary>
    public bool AllowCancel
    {
      get { return _AllowCancel; }
      set
      {
        if (value == _AllowCancel)
          return;

        _AllowCancel = value;
        InitCancelButton();
        _Form.lblWaitCancel.Visible = false;

        if (value)
          CheckCancelled(); // 11.12.2019
        else
          Application.DoEvents();
      }
    }
    private bool _AllowCancel;

    /// <summary>
    /// Инициализация доступности кнопки "Отмена" при установке свойства <see cref="AllowCancel"/>=true.
    /// Если заставка встраивается в <see cref="Wizard"/>, то выполняются другие действия.
    /// </summary>
    protected virtual void InitCancelButton()
    {
      _Form.btnCancel.Enabled = _AllowCancel;
      _Form.btnCancel.Select();
      _Form.btnCancel.Update();
    }

    /// <summary>
    /// Признак прерывания процесса пользователем
    /// </summary>
    public bool Cancelled
    {
      get
      {
        if (_Form == null) // 20.07.2018
          return false;
        else
          return _Form.Cancelled;
      }
      set
      {
        if (_Form != null) // 20.07.2018
          _Form.Cancelled = value;
      }
    }

    /// <summary>
    /// Проверяет свойство <see cref="Cancelled"/> и генерирует исключение <see cref="UserCancelException"/>, если пользователь прервал процесс
    /// </summary>
    public void CheckCancelled()
    {
      Application.DoEvents();
      if (Cancelled && AllowCancel /* 11.12.2019 */)
        throw new UserCancelException();
    }

    /// <summary>
    /// Вызывает <see cref="Application.DoEvents()"/> и <see cref="Thread.Sleep(int)"/>
    /// </summary>
    /// <param name="milliseconds">Время ожидания в миллисекундах</param>
    public void Sleep(int milliseconds)
    {
      Application.DoEvents();
      System.Threading.Thread.Sleep(milliseconds);
      Application.DoEvents(); // 31.08.2017
    }

    #endregion

    #region Процентный индикатор

    /// <summary>
    /// Диапазон для процентного индикатора. 0-нет процентного индикатора.
    /// Установка значения свойства сбрасывает <see cref="Percent"/> в 0
    /// </summary>
    public int PercentMax
    {
      get { return _PercentMax; }
      set { SetPercent(0, value); }
    }

    private int _PercentMax;

    /// <summary>
    /// Установка процентного индикатора.
    /// <see cref="CheckCancelled()"/> не вызывается.
    /// </summary>
    /// <param name="percent">Текущее значение процентного индикатора</param>
    /// <param name="percentMax">Максимальное значение процентного индикатора</param>
    public void SetPercent(int percent, int percentMax)
    {
      if (percentMax < 0)
        percentMax = 0;
      if (percent < 0)
        percent = 0;
      if (percent > percentMax)
        percent = percentMax;
      if (percent == _Percent && percentMax == _PercentMax)
        return;
      _PercentMax = percentMax;
      _Percent = percent;

      if (percentMax > 0)
        InitTimer();

      Application.DoEvents();
    }

    /// <summary>
    /// Текущее значение для процентного индикатора.
    /// Установка свойства вызывает <see cref="CheckCancelled()"/>, если <see cref="AllowCancel"/>=true.
    /// Вызывается <see cref="Application.DoEvents()"/>.
    /// </summary>
    public int Percent
    {
      get
      {
        return _Percent;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > _PercentMax)
          value = _PercentMax;
        if (value == _Percent)
          return;
        _Percent = value;

        if (AllowCancel)
          CheckCancelled(); // 05.03.2013
        else
          Application.DoEvents();
      }
    }
    private int _Percent;

    /// <summary>
    /// Увеличение значения процентного индикатора на 1.
    /// Установка свойства вызывает <see cref="CheckCancelled()"/>, если <see cref="AllowCancel"/>=true.
    /// Вызывается <see cref="Application.DoEvents()"/>.
    /// </summary>
    public void IncPercent()
    {
      Percent = _Percent + 1;
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Используется объектом <see cref="SplashStack"/>.
    /// Не предназначено для использования в пользовательском коде.
    /// </summary>
    public int StackSerialId { get { return _StackSerialId; } set { _StackSerialId = value; } }
    private int _StackSerialId;

    #endregion

    #region Проверка потока

#if DEBUG

    /// <summary>
    /// Поток, из которого был создано окно.
    /// Только для отладочных целей.
    /// </summary>
    public Thread DebugTheThread { get { return _DebugTheThread; } }
    private Thread _DebugTheThread;

    /// <summary>
    /// Возвращает true, если заставка была создана из основного потока <see cref="EFPApp.MainThread"/>.
    /// Только для отладочных целей.
    /// </summary>
    public bool DebugIsMainThread { get { return Object.ReferenceEquals(_DebugTheThread, EFPApp.MainThread); } }

    /// <summary>
    /// Возвращает true, если заставка была создана в текущем потоке <see cref="Thread.CurrentThread"/>.
    /// Только для отладочных целей.
    /// </summary>
    public bool DebugIsCurrentThread { get { return Object.ReferenceEquals(_DebugTheThread, Thread.CurrentThread); } }

    /// <summary>
    /// Генерирует исключение <see cref="DifferentThreadException"/> при вызове из потока,
    /// отличного от того, где была создана заставка.
    /// </summary>
    internal void CheckThread()
    {
      if (!Object.ReferenceEquals(Thread.CurrentThread, _DebugTheThread))
        throw new DifferentThreadException(_DebugTheThread);
    }

#endif

    #endregion

    #region Отладка стека вызовов

#if DEBUG_SPLASH

    /// <summary>
    /// Стек вызовов на момент открытия заставки.
    /// Только для отладочных целей
    /// </summary>
    public string DebugStackTrace { get { return _DebugStackTrace; } }
    private string _DebugStackTrace;

    private static int DebugCounter = 0;

    /// <summary>
    /// Условный порядковый номер.
    /// Только для отладочных целей
    /// </summary>
    public int DebugSplashId { get { return _DebugSplashId; } }
    private int _DebugSplashId;

    void btnDebug_Click(object Sender, EventArgs Args)
    {
      // EFPApp.MessageBox(StackTrace, "Стек вызовов для заставки #" + DebugId.ToString());
/*
      StringBuilder sb=new StringBuilder();
      sb.Append("Splash constructor call stack:");
      sb.Append(Environment.NewLine);
      sb.Append(_DebugStackTrace);
      sb.Append(Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append("Splash constructor thread:");
      sb.Append(Environment.NewLine);
      sb.Append(LogoutTools.LogoutObjectToString(DebugTheThread));
      sb.Append(Environment.NewLine);
      sb.Append("Splash form:");
      sb.Append(Environment.NewLine);
      sb.Append(LogoutTools.LogoutObjectToString(_Form));
      sb.Append(Environment.NewLine);
  
      DebugTools.ShowText(sb.ToString(), "Заставка #" + _DebugSplashId.ToString());
 * */
      DebugTools.ShowText(LogoutTools.LogoutObjectToString(this), "Заставка #" + _DebugSplashId.ToString());
    }

#endif

    #endregion
  }

  /// <summary>
  /// Стек экранных заставок. Управляет объектами <see cref="FreeLibSet.Forms.Splash"/> (окнами заставок Windows Forms).
  /// Объект предназначен для работы только в одном потоке, но не обязательно в основном потоке, в котором вызывался <see cref="EFPApp.InitApp()"/>.
  /// </summary>
  public sealed class SplashStack : SimpleDisposableObject, ISplashStack
  {
    // Можно использовать базовый класс без деструктора

    #region Конструктор и Dispose

    /// <summary>
    /// Создает пустой стек заставок.
    /// Может вызываться из любого потока. Все последующие вызовы методов объекта должны выполняться из потока, в котором вызван конструктор.
    /// Эта версия конструктора создает автономный объект, ни к чему его не присоединяя.
    /// </summary>
    public SplashStack()
      : this(false)
    {
    }

    /// <summary>
    /// Создает пустой стек заставок и, опционально, вызывает <see cref="SplashTools.PushThreadSplashStack()"/>.
    /// Когда будет вызван метод Dispose(), то, при необходимости, вызывается <see cref="SplashTools.PopThreadSplashStack()"/>.
    /// Используется, когда стек заставок создается для выполнения ограниченных действий, чтобы ограничиться одним блоком using.
    /// </summary>
    /// <param name="pushForThread">Если true, то будут вызываться <see cref="SplashTools.PushThreadSplashStack()"/> 
    /// и <see cref="SplashTools.PopThreadSplashStack()"/></param>
    public SplashStack(bool pushForThread)
    {
#if DEBUG
      _TheThread = Thread.CurrentThread;
#endif

      _Stack = new Stack<Splash>();

      if (pushForThread)
      {
        SplashTools.PushThreadSplashStack(this);
        _PushedForThread = true;
      }
    }

    /// <summary>
    /// Закрытие стека заставок.
    /// Если вызван из метода Dispose(), то закрывает все заставки, для которых нее было парного вызова <see cref="EndSplash()"/>.
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose().</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_PushedForThread)
        {
          _PushedForThread = false;
          SplashTools.PopThreadSplashStack();
        }

        Clear();
      }
      base.Dispose(disposing);
    }

    private void Clear()
    {
#if DEBUG
      CheckThread();
#endif

      while (_Stack.Count > 0)
        EndSplash();
    }

    /// <summary>
    /// Устанавливается конструктором в true, если был вызов SplashTools.PushThreadSplashStack().
    /// </summary>
    private bool _PushedForThread;

    #endregion

    #region ISplashStack Members

    private Stack<Splash> _Stack;

    private int _StackVersion;

    /// <summary>
    /// Создает новую экранную заставку <see cref="FreeLibSet.Forms.Splash"/> и помещает ее в стек.
    /// Заставка немедленно отображается на экране.
    /// </summary>
    /// <param name="phases">Список названий фаз. Список не может быть пустым</param>
    /// <returns>Интферфейс управления заставкой</returns>
    public ISplash BeginSplash(string[] phases)
    {
#if DEBUG
      CheckThread();
#endif

      if (_StackVersion == int.MaxValue)
        _StackVersion = 1;
      else
        _StackVersion++;

      Splash spl = new Splash(phases);
      spl.StackSerialId = _StackVersion;
      _Stack.Push(spl);

      return spl;
    }

    /// <summary>
    /// Создает новую экранную заставку <see cref="FreeLibSet.Forms.Splash"/> из одной фазы и помещает ее в стек.
    /// Заставка немедленно отображается на экране.
    /// </summary>
    /// <param name="phaseText">Название фазы</param>
    /// <returns>Интферфейс управления заставкой</returns>
    public ISplash BeginSplash(string phaseText)
    {
      return BeginSplash(new string[1] { phaseText });
    }

    /// <summary>
    /// Создает новую экранную заставку <see cref="FreeLibSet.Forms.Splash"/> из одной фазы "Идет процесс" (<see cref="SplashTools.DefaultSplashItems"/>) 
    /// и помещает ее в стек.
    /// Заставка немедленно отображается на экране.
    /// </summary>
    /// <returns>Интферфейс управления заставкой</returns>
    public ISplash BeginSplash()
    {
      return BeginSplash(SplashTools.DefaultSplashItems);
    }

    /// <summary>
    /// Закрывает заставку, открытую последним вызовом <see cref="BeginSplash()"/>.
    /// Заставка немедленно убирается с экрана.
    /// </summary>
    public void EndSplash()
    {
#if DEBUG
      CheckThread();
#endif

      if (_StackVersion == int.MaxValue)
        _StackVersion = 1;
      else
        _StackVersion++;

      Splash spl = _Stack.Pop();
      spl.Close();
    }

    /// <summary>
    /// Возвращает итерфейс управления текущей (активной) заставкой на экране.
    /// Если стек заставок пустой, то возвращается null.
    /// Если на экране есть другие заставки <see cref="FreeLibSet.Forms.Splash"/>, созданные вне данного стека, 
    /// то к ним не доступа из текущего объекта <see cref="SplashStack"/>.
    /// </summary>
    public ISplash Splash
    {
      get
      {
#if DEBUG
        CheckThread();
#endif

        if (_Stack.Count == 0)
          return null;
        else
          return _Stack.Peek();
      }
    }

    /// <summary>
    /// Возвращает массив экранных заставок в данном стеке.
    /// Текущая (активная) заставка имеет индекс 0.
    /// Если стек не содержит заставок, возвращается пустой массив.
    /// Если на экране есть другие заставки <see cref="FreeLibSet.Forms.Splash"/>, созданные вне данного стека, то они не учитываются.
    /// </summary>
    /// <returns>Массив интерфейсов управления экранными заставками</returns>
    public ISplash[] GetSplashStack()
    {
#if DEBUG
      CheckThread();
#endif

      return _Stack.ToArray();
    }

    /// <summary>
    /// Метод предназначен для использования в <see cref="MemorySplashStack"/>.
    /// </summary>
    /// <param name="stackVersion"></param>
    /// <returns></returns>
    public ISplash[] GetSplashStack(ref int stackVersion)
    {
#if DEBUG
      CheckThread();
#endif

      ISplash[] a = null;
      if (stackVersion != _StackVersion)
        a = _Stack.ToArray();
      stackVersion = _StackVersion;
      return a;
    }

    #endregion

    #region Проверка потока

#if DEBUG

    /// <summary>
    /// Поток, из которого был вызван конструктор стека.
    /// Не использовать в прикладном коде.
    /// </summary>
    public Thread TheThread { get { return _TheThread; } }
    private Thread _TheThread;

    /// <summary>
    /// Генерирует исключение <see cref="DifferentThreadException"/> при вызове из потока,
    /// отличного от того, где была создана заставка.
    /// Не использовать в прикладном коде.
    /// </summary>
    public void CheckThread()
    {
      if (Thread.CurrentThread != _TheThread)
        throw new DifferentThreadException(_TheThread);
    }

#endif

    #endregion
  }
}
