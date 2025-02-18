// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FreeLibSet.Core;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Интерфейс для простого процентного индикатора.
  /// Поддерживает установку значения индикатора, текста выполняемого действия, и возможность прерывания процесса.
  /// </summary>
  public interface ISimpleSplash
  {
    #region Процентный индикатор

    /// <summary>
    /// Установка диапазона для процентного индикатора. Нулевое значение означает
    /// отстутствие индикатора.
    /// При установке значения <see cref="PercentMax"/>, свойство <see cref="Percent"/> сбрасывается в 0.
    /// Проверка <see cref="CheckCancelled()"/> не вызывается.
    /// </summary>
    int PercentMax { get; set; }

    /// <summary>
    /// Текущее значение процентного индикатора в диапазоне от 0 до <see cref="PercentMax"/> включительно.
    /// Если <see cref="AllowCancel"/>=true, то вызывается <see cref="CheckCancelled()"/>.
    /// </summary>
    int Percent { get; set; }

    /// <summary>
    /// Одновременная установка свойств <see cref="Percent"/> и <see cref="PercentMax"/>.
    /// Проверка <see cref="CheckCancelled()"/> не вызывается.
    /// </summary>
    /// <param name="percent">Значение процентного индикатора</param>
    /// <param name="percentMax">Максимальное значение процентного индикатора</param>
    void SetPercent(int percent, int percentMax);

    /// <summary>
    /// Увеличить значение свойства <see cref="Percent"/> на единицу.
    /// Если <see cref="AllowCancel"/>=true, то вызывается <see cref="CheckCancelled()"/>.
    /// </summary>
    void IncPercent();

    #endregion

    #region Прерывание процесса

    /// <summary>
    /// Управление возможностью прерывания процесса пользователем (наличия доступной
    /// кнопки "Отмена").
    /// При переходе к следующей фазе свойство автоматически сбрасывается в false.
    /// Установка свойства в false не отменяет установку свойства <see cref="Cancelled"/> в false, если кнопка "Отмена" была нажата асинхронно.
    /// При последующей установке свойства в true проверяется свойство <see cref="Cancelled"/>. Если оно было установлено в true, генерируется исключение <see cref="UserCancelException"/>.
    /// </summary>
    bool AllowCancel { get; set; }

    /// <summary>
    /// Свойство возвращает true, если пользователь нажал кнопку "Отмена" и требуется
    /// прервать процесс.
    /// Обычно в прикладном коде следует вызывать метод <see cref="CheckCancelled()"/> а не проверять свойство.
    /// </summary>
    bool Cancelled { get; set; }

    /// <summary>
    /// Метод проверяет установку свойств <see cref="AllowCancel"/> и <see cref="Cancelled"/> и генерирует исключение
    /// <see cref="UserCancelException"/>, если пользователь прервал процесс.
    /// Метод автоматически вызывается при установке свойства <see cref="Percent"/> или вызове метода <see cref="IncPercent()"/>.
    /// </summary>
    void CheckCancelled();

    /// <summary>
    /// "Усыпление" потока на заданное время. Если интерфейс относится к основному потоку приложения Windows Forms, вызывается System.Windows.Forms.Application.DoEvents().
    /// Бесконечное время не допускается.
    /// </summary>
    /// <param name="milliseconds">Время в миллисекундах</param>
    void Sleep(int milliseconds);

    #endregion

    #region Текст

    /// <summary>
    /// Текст текущей фазы
    /// </summary>
    string PhaseText { get; set; }

    #endregion
  }

  #region Перечисление SplashItemState

  /// <summary>
  /// Текущее состояние фазы заставки
  /// </summary>
  public enum SplashPhaseState
  {
    /// <summary>
    /// Фаза еще не выполнялась
    /// </summary>
    None, // должно обязательно иметь значение 0

    /// <summary>
    /// Фаза сейчас выполняется
    /// </summary>
    Current,

    /// <summary>
    /// Фаза выполнена
    /// </summary>
    Complete,

    /// <summary>
    /// Фаза пропущена
    /// </summary>
    Skipped
  }

  #endregion

  /// <summary>
  /// Интерфейс splash-заставками, содержащей одну или несколько позиций
  /// Позволяет управлять уже существующей заставкой:
  /// <list type="bullet">
  /// <item><description>задание наличия и положения процентного индикатора;</description></item>
  /// <item><description>текущая фаза процесса;</description></item>
  /// <item><description>прерывание процесса.</description></item>
  /// </list>
  /// Не дает возможности создавать и закрывать заставку или открывать дочернюю заставку.
  /// </summary>
  public interface ISplash : ISimpleSplash
  {
    #region Список элементов

    /// <summary>
    /// Возвращает массив названий для всех фаз
    /// </summary>
    string[] Phases { get; }

    /// <summary>
    /// Возвращает общее количество фаз
    /// </summary>
    int PhaseCount { get; }

    /// <summary>
    /// Возвращает состояние заданной фазы
    /// </summary>
    /// <param name="phase">Индекс фазы от 0 до (PhaseCount-1)</param>
    /// <returns>Состояние для фазы</returns>
    SplashPhaseState GetPhaseState(int phase);

    /// <summary>
    /// Возвращает состояния сразу для всех фаз в виде массива значений.
    /// Длина массива равна <see cref="PhaseCount"/>.
    /// </summary>
    /// <returns>Массив состояний</returns>
    SplashPhaseState[] GetPhaseStates();

    #endregion

    #region Текущая фаза

    /// <summary>
    /// Номер текущей фазы.
    /// При открытии заставки возвращает значение 0. Методы <see cref="Complete()"/> и <see cref="Skip()"/> увеличивают значение на 1.
    /// Может вернуть значение, равное количеству строк в заставке, если <see cref="Complete()"/> или <see cref="Skip()"/> вызваны для последней фазы.
    /// </summary>
    int Phase { get; }

    /// <summary>
    /// Пометить текущую фазу как завершенную и перейти к следующей фазе.
    /// Процентный индикатор и возможность прерывания процесса при переходе к следующей фазе отключаются
    /// (устанавливаются свойства <see cref="ISimpleSplash.PercentMax"/>=0, <see cref="ISimpleSplash.Percent"/>=0, <see cref="ISimpleSplash.AllowCancel"/>=false).
    /// </summary>
    void Complete();

    /// <summary>
    /// Пометить текущую фазу как пропущенную и перейти к следующей фазе.
    /// Процентный индикатор и возможность прерывания процесса при переходе к следующей фазе отключаются
    /// (устанавливаются свойства <see cref="ISimpleSplash.PercentMax"/>=0, <see cref="ISimpleSplash.Percent"/>=0, <see cref="ISimpleSplash.AllowCancel"/>=false).
    /// </summary>
    void Skip();

    #endregion

    #region Служебные поля

    /// <summary>
    /// Служебное поле, заполняемое, если заставки образуют стек.
    /// Пользовательский код не должен использовать это свойство.
    /// </summary>
    int StackSerialId { get; set; }

    #endregion
  }

  /// <summary>
  /// Интерфейс для создания и удаления заставок с процентым индикатором.
  /// Предполагается парный вызов методов <see cref="BeginSplash()"/> и <see cref="EndSplash()"/>.
  /// </summary>
  public interface ISplashStack
  {
    #region Создание и удаление заставок

    /// <summary>
    /// Создать заставку, содержащую несколько фаз.
    /// </summary>
    /// <param name="phases">Список фаз</param>
    /// <returns>Интерфейс управления заставой</returns>
    ISplash BeginSplash(string[] phases);

    /// <summary>
    /// Создать заставку, содержащую одну фазу
    /// </summary>
    /// <param name="phase">Текст фазы</param>
    /// <returns>Интерфейс управления заставой</returns>
    ISplash BeginSplash(string phase);

    /// <summary>
    /// Создать заставку, содержащую одну фазу с текстом по умолчанию
    /// </summary>
    /// <returns></returns>
    ISplash BeginSplash();

    /// <summary>
    /// Закрыть текущую заставку. Количество вызовов <see cref="EndSplash()"/> должно
    /// соответствовать количеству <see cref="BeginSplash()"/>.
    /// </summary>
    void EndSplash();

    #endregion

    #region Получение заставок

    /// <summary>
    /// Возвращает интерфейс управления текущей заставкой или null, если нет
    /// непарных вызовов <see cref="BeginSplash()"/>.
    /// Возвращаемое значение соответствует <see cref="GetSplashStack()"/>[0] или null.
    /// </summary>
    ISplash Splash { get; }

    /// <summary>
    /// Возвращает стек заставок. 
    /// Текущая заставка, возвращаемая <see cref="Splash"/>, находится в начале массива (индекс 0).
    /// Если заставок нет, возрващается пустой массив.
    /// </summary>
    /// <returns>Массив заставок</returns>
    ISplash[] GetSplashStack();

    /// <summary>
    /// Возвращает стек заставок или null, если изменений не было.
    /// Эта версия используется объектом <see cref="MemorySplashStack"/>.
    /// Не предеазначено для использования в прикладном коде.
    /// </summary>
    /// <param name="stackVersion">Вход и выход.
    /// Если на входе задано значение 0 или оно не совпадает с внутренним состоянием стека, список заставок будет возвращен. Иначе возвращается null.
    /// На выходе по ссылке помещается текущее значение состояния стека, отличное от 0.
    /// Текущее состояние стека меняется при каждом вызове методов <see cref="BeginSplash()"/> и <see cref="EndSplash()"/>. Первоначально имеет значение 1</param>
    /// <returns>Массив заставок</returns>
    ISplash[] GetSplashStack(ref int stackVersion);

    #endregion
  }

  /// <summary>
  /// Заглушка для реализации интерфейса <see cref="ISimpleSplash"/>
  /// </summary>
  public class DummySimpleSplash : ISimpleSplash
  {
    #region ISimpleSplash Members

    /// <summary>
    /// Это свойство ни на что не влияет
    /// </summary>
    public int PercentMax
    {
      get { return _PercentMax; }
      set { _PercentMax = value; }
    }
    private int _PercentMax;

    /// <summary>
    /// Это свойство ни на что не влияет
    /// </summary>
    public int Percent
    {
      get { return _Percent; }
      set { _Percent = value; }
    }
    private int _Percent;

    /// <summary>
    /// Этот метод ни на что не влияет
    /// </summary>
    public void SetPercent(int percent, int percentMax)
    {
      _Percent = percent;
      _PercentMax = percentMax;
    }

    /// <summary>
    /// Этот метод ни на что не влияет
    /// </summary>
    public void IncPercent()
    {
      Percent++;
    }

    /// <summary>
    /// Это свойство ни на что не влияет. Всегда возвращает false.
    /// </summary>
    public bool AllowCancel
    {
      get { return false; }
      set { }
    }

    /// <summary>
    /// Всегда возвращает false
    /// </summary>
    public bool Cancelled { get { return false; } set { } }

    /// <summary>
    /// Этот метод ни на что не влияет
    /// </summary>
    public void CheckCancelled()
    {
      // Ничего не делаем
    }

    /// <summary>
    /// Это свойство ни на что не влияет
    /// </summary>
    public string PhaseText
    {
      get { return _PhaseText; }
      set { _PhaseText = value; }
    }
    private string _PhaseText;

    /// <summary>
    /// Вызывает <see cref="Thread.Sleep(int)"/>
    /// </summary>
    /// <param name="milliseconds">Интервал задержки в миллисекундах</param>
    public void Sleep(int milliseconds)
    {
      Thread.Sleep(milliseconds);
    }

    #endregion
  }

  /// <summary>
  /// Заглушка для реализации интерфейса <see cref="ISplash"/>
  /// </summary>
  public class DummySplash : DummySimpleSplash, ISplash
  {
    #region ISplash Members

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Complete()
    {
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Skip()
    {
    }


    /// <summary>
    /// Возвращает массив из одной строки
    /// </summary>
    public string[] Phases { get { return SplashTools.DefaultSplashItems; } }

    /// <summary>
    /// Возвращает 1
    /// </summary>
    public int PhaseCount { get { return 1; } }

    /// <summary>
    /// Возвращает <see cref="SplashPhaseState.Current"/>.
    /// </summary>
    /// <param name="phase">Не используется</param>
    /// <returns><see cref="SplashPhaseState.Current"/></returns>
    public SplashPhaseState GetPhaseState(int phase)
    {
      return SplashPhaseState.Current;
    }

    private static readonly SplashPhaseState[] _PhaseStates = new SplashPhaseState[1] { SplashPhaseState.Current };

    /// <summary>
    /// Возвращает 1 элемент <see cref="SplashPhaseState.Current"/>.
    /// </summary>
    public SplashPhaseState[] GetPhaseStates()
    {
      return _PhaseStates;
    }

    /// <summary>
    /// Возвращает 0
    /// </summary>
    public int Phase { get { return 0; } }

    /// <summary>
    /// Свойство интерфейса <see cref="ISplash"/>.
    /// </summary>
    public int StackSerialId { get { return _StackSerialId; } set { _StackSerialId = value; } }
    private int _StackSerialId;

    #endregion
  }

  /// <summary>
  /// Реализация заставки, используемой на стороне сервера.
  /// Сохраняет устанавливаемые значения.
  /// Значения затем могут быть прочитаны и переданы клиенту.
  /// Этот класс является потокобезопасным в режиме "несколько смотрят, один пишет".
  /// </summary>
  public sealed class MemorySplash : ISplash
  {
    #region Конструктор

    /// <summary>
    /// Создает объект заставки на сервере.
    /// Объекты не должны напрямую создаваться в пользовательском коде.
    /// </summary>
    /// <param name="phases">Список фаз</param>
    public MemorySplash(string[] phases)
    {
      if (phases == null)
        throw new ArgumentNullException("phases");
      for (int i = 0; i < phases.Length; i++)
      {
        if (String.IsNullOrEmpty(phases[i]))
          throw ExceptionFactory.ArgInvalidListItem("phases", phases, i);
      }

      _Phases = phases;
      _PhaseStates = new SplashPhaseState[phases.Length];
      if (phases.Length > 0)
        _PhaseStates[0] = SplashPhaseState.Current;
    }

    #endregion

    #region ISplash Members

    /// <summary>
    /// Названия позиций
    /// </summary>
    public string[] Phases { get { return _Phases; } }
    private readonly string[] _Phases;


    /// <summary>
    /// Возвращает общее количество фаз
    /// </summary>
    public int PhaseCount { get { return _Phases.Length; } }

    /// <summary>
    /// Возвращает состояние заданной фазы
    /// </summary>
    /// <param name="phase">Номер фазы от 0 до (<see cref="PhaseCount"/>-1)</param>
    /// <returns>Состояние фазы</returns>
    public SplashPhaseState GetPhaseState(int phase)
    {
#if DEBUG
      if (phase < 0 || phase >= _PhaseStates.Length)
        throw ExceptionFactory.ArgOutOfRange("phase", phase, 0, _PhaseStates.Length-1);
#endif
      return _PhaseStates[phase]; // можно без блокировки
    }

    /// <summary>
    /// Возвращает состояние всех фаз.
    /// Возвращает копию массива.
    /// </summary>
    public SplashPhaseState[] GetPhaseStates()
    {
      lock (_PhaseStates)
      {
        SplashPhaseState[] a = new SplashPhaseState[_PhaseStates.Length];
        _PhaseStates.CopyTo(a, 0);
        return a;
      }
    }
    private SplashPhaseState[] _PhaseStates;

    /// <summary>
    /// Индекс текущей фазы в диапазоне от 0 до <see cref="Phases"/>.Length включительно.
    /// </summary>
    public int Phase { get { return _Phase; } }
    private int _Phase;

    /// <summary>
    /// Текст текущей фазы
    /// </summary>
    public string PhaseText
    {
      get
      {
        return _PhaseText; // здесь блокировка не нужна
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          if (_Phase >= _Phases.Length)
            value = Res.Splash_Phase_Complete;
          else
            value = _Phases[_Phase];
        }

        _PhaseText = value; // тоже можно без блокировки
      }
    }
    private string _PhaseText;

    /// <summary>
    /// Установка состояния "Выполнено" для текущей фазы и переход к следующей фазе.
    /// </summary>
    public void Complete()
    {
      lock (_PhaseStates)
      {
        if (_Phase >= _Phases.Length)
          throw new InvalidOperationException(Res.Splash_Err_ExtraCompleteOrSkip);

        _PhaseStates[_Phase] = SplashPhaseState.Complete;
        _Phase++;
        if (_Phase < _Phases.Length)
          _PhaseStates[_Phase] = SplashPhaseState.Current;
        PhaseText = null;
        SetPercent(0, 0);
        AllowCancel = false;
      }
    }

    /// <summary>
    /// Установка состояния "Пропуск" для текущей фазы и переход к следующей фазе.
    /// </summary>
    public void Skip()
    {
      lock (_PhaseStates)
      {
        if (_Phase >= _Phases.Length)
          throw new InvalidOperationException(Res.Splash_Err_ExtraCompleteOrSkip);

        _PhaseStates[_Phase] = SplashPhaseState.Skipped;
        _Phase++;
        if (_Phase < _Phases.Length)
          _PhaseStates[_Phase] = SplashPhaseState.Current;
        PhaseText = null;
        SetPercent(0, 0);
        AllowCancel = false;
      }
    }

    #endregion

    #region ISimpleSplash Members

    /// <summary>
    /// Свойство интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public int PercentMax
    {
      get { return _PercentMax; }
      set
      {
        _PercentMax = value;
        _Percent = 0;
      }
    }
    private int _PercentMax;

    /// <summary>
    /// Свойство интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public int Percent
    {
      get { return _Percent; }
      set
      {
        _Percent = value;
        CheckCancelled();
      }
    }
    private int _Percent;

    /// <summary>
    /// Метод интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    /// <param name="percent">Текущее значение</param>
    /// <param name="percentMax">Максимальное значение</param>
    public void SetPercent(int percent, int percentMax)
    {
      _Percent = percent;
      _PercentMax = PercentMax;
      CheckCancelled();
    }

    /// <summary>
    /// Метод интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public void IncPercent()
    {
      Interlocked.Increment(ref _Percent);
      CheckCancelled();
    }

    /// <summary>
    /// Свойство интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public bool AllowCancel
    {
      get { return _AllowCancel; }
      set
      {
        if (value == _AllowCancel)
          return;
        _AllowCancel = value;
        if (value)
          CheckCancelled();
      }
    }
    private bool _AllowCancel;

    /// <summary>
    /// Свойство интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public bool Cancelled
    {
      get { return _Cancelled; }
      set { _Cancelled = value; }
    }
    private bool _Cancelled;

    /// <summary>
    /// Метод интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public void CheckCancelled()
    {
      if (_AllowCancel && _Cancelled)
        throw new UserCancelException();
    }

    /// <summary>
    /// Метод интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    /// <param name="milliseconds">Интервал в миллисекундах</param>
    public void Sleep(int milliseconds)
    {
      if (milliseconds < 0)
        throw ExceptionFactory.ArgOutOfRange("milliseconds", milliseconds, 0, null);
      Thread.Sleep(milliseconds);
    }

    /// <summary>
    /// Используется объектом <see cref="MemorySplashStack"/>
    /// </summary>
    public int StackSerialId { get { return _StackSerialId; } set { _StackSerialId = value; } }
    private int _StackSerialId;

    #endregion
  }


  /// <summary>
  /// Стек заставок на стороне сервера.
  /// Этот класс является потокобезопасным.
  /// </summary>
  public sealed class MemorySplashStack : ISplashStack
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой стек
    /// </summary>
    public MemorySplashStack()
    {
      _Stack = new Stack<MemorySplash>();
      _StackVersion = 1;
    }

    #endregion

    #region Поля

    private Stack<MemorySplash> _Stack;

    private int _StackVersion;

    #endregion

    #region ISplashStack Members

    /// <summary>
    /// Создает новую заставку <see cref="MemorySplash"/> и добавляет ее в стек.
    /// </summary>
    /// <param name="phases">Названия фаз. Список не может быть пустым</param>
    /// <returns>Интерфейс управления заставкой</returns>
    public ISplash BeginSplash(string[] phases)
    {
      MemorySplash spl = new MemorySplash(phases);
      lock (_Stack)
      {
        if (_StackVersion == int.MaxValue)
          _StackVersion = 1;
        else
          _StackVersion++;

        spl.StackSerialId = _StackVersion; // ну и пусть они будут не по порядку

        _Stack.Push(spl);
      }
      return spl;
    }

    /// <summary>
    /// Создает новую заставку <see cref="MemorySplash"/> с одной фазой и добавляет ее в стек
    /// </summary>
    /// <param name="phaseText">Название фазы</param>
    /// <returns>Интерфейс управления заставкой</returns>
    public ISplash BeginSplash(string phaseText)
    {
      return BeginSplash(new string[1] { phaseText });
    }

    /// <summary>
    /// Создает новую заставку <see cref="MemorySplash"/> с одной фазой "Идет процесс" и добавляет ее в стек
    /// </summary>
    /// <returns>Интерфейс управления заставкой</returns>
    public ISplash BeginSplash()
    {
      return BeginSplash(SplashTools.DefaultSplashItems);
    }

    /// <summary>
    /// Удаляет из стека заставку, созданную вызовом <see cref="BeginSplash()"/>.
    /// </summary>
    public void EndSplash()
    {
      lock (_Stack)
      {
        if (_StackVersion == int.MaxValue)
          _StackVersion = 1;
        else
          _StackVersion++;

        _Stack.Pop();
      }
    }

    /// <summary>
    /// Возвращает интерфейс управления теущей заставкой.
    /// Если стек заставок пустой, возвращает null.
    /// </summary>
    public ISplash Splash
    {
      get
      {
        lock (_Stack)
        {
          if (_Stack.Count == 0)
            return null;
          else
            return _Stack.Peek();
        }
      }
    }

    /// <summary>
    /// Возвращает массив заставок в стеке.
    /// Текущая (активная) заставка находится в позиции 0.
    /// Если стек заставок пустой, возвращается пустой массив.
    /// </summary>
    /// <returns>Массив интерфейсов управления закладками</returns>
    public ISplash[] GetSplashStack()
    {
      lock (_Stack)
      {
        return _Stack.ToArray();
      }
    }

    /// <summary>
    /// Этот метод используется в <see cref="FreeLibSet.Remoting.ServerSplashWatcher"/>
    /// </summary>
    /// <param name="stackVersion">Версия стека</param>
    /// <returns>Массив заставок</returns>
    public ISplash[] GetSplashStack(ref int stackVersion)
    {
      ISplash[] a = null;

      lock (_Stack)
      {
        if (stackVersion != _StackVersion)
        {
          a = _Stack.ToArray();
          stackVersion = _StackVersion;
        }
      }
      return a;
    }

    #endregion
  }

  /// <summary>
  /// Статические методы и свойства для работы с заставками.
  /// Класс является потокобезопасным
  /// </summary>
  public static class SplashTools
  {
    #region Статический конструктор

    static SplashTools()
    {
      _DefaultSplashText = "Идет процесс";
      _DefaultSplashItems = new string[1] { _DefaultSplashText };
    }

    #endregion

    #region DefaultSplashText и DefaultSplashItems

    /// <summary>
    /// Текст заставки, используемый при вызове метода <see cref="ISplashStack.BeginSplash()"/> без параметров.
    /// По умолчанию используется текст "Идет процесс".
    /// </summary>
    public static string DefaultSplashText
    {
      get { return _DefaultSplashText; }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _DefaultSplashText = value;
        _DefaultSplashItems = new string[] { value };
      }
    }
    private static string _DefaultSplashText;


    /// <summary>
    /// Возвращает массив названий фаз из одного элемента, равного <see cref="DefaultSplashText"/>.
    /// Не предназначено для использования в прикладном коде.
    /// </summary>
    public static string[] DefaultSplashItems { get { return _DefaultSplashItems; } }
    private static string[] _DefaultSplashItems;

    #endregion

    #region ThreadSplashStack

    /// <summary>
    /// Стек заставок, используемый в текущем потоке.
    /// Если заставки не использовались в текущем потоке, возвращает пустой стек.
    /// Вызовы <see cref="FreeLibSet.Remoting.ExecProc.BeginExecute(Remoting.NamedValues, AsyncCallback, object)"/> /
    /// <see cref="FreeLibSet.Remoting.ExecProc.EndExecute(IAsyncResult)"/> должны обязательно выполнятся в блоке try/finally, чтобы исключить
    /// засорение стека в случае ошибок (в отличие от использования <see cref="FreeLibSet.Remoting.ExecProc.BeginSplash()"/>
    /// / <see cref="FreeLibSet.Remoting.ExecProc.EndSplash()"/>).
    /// </summary>
    public static ISplashStack ThreadSplashStack
    {
      get
      {
        // Так как _ThreadSplashStack является ThreadStatic, блокировка не требуется
        if (_ThreadSplashStack == null)
          _ThreadSplashStack = new MemorySplashStack(); // фиктивный стек заставок
        return _ThreadSplashStack;
      }
    }
    [ThreadStatic]
    private static ISplashStack _ThreadSplashStack;

    /// <summary>
    /// Начать в текущем потоке работу с новым стеком заставок.
    /// Существующий стек запоминается до вызова <see cref="PopThreadSplashStack()"/>.
    /// </summary>
    /// <param name="splashStack">Новый стек заставок. Не может быть null.</param>
    public static void PushThreadSplashStack(ISplashStack splashStack)
    {
      if (splashStack == null)
        throw new ArgumentNullException();

      if (_ThreadSplashStackStack == null)
        _ThreadSplashStackStack = new Stack<ISplashStack>();
      _ThreadSplashStackStack.Push(_ThreadSplashStack); // может быть и null запихали
      _ThreadSplashStack = splashStack;
    }

    /// <summary>
    /// Начать в текущем потоке работу со стеком заставок - заглушкой.
    /// Этот метод используется, когда надо подавить вывод "мельтешащих" заставок.
    /// Существующий стек запоминается до вызова <see cref="PopThreadSplashStack()"/>.
    /// </summary>
    public static void PushThreadSplashStack()
    {
      PushThreadSplashStack(new MemorySplashStack());
    }

    /// <summary>
    /// Закончить в текущем потоке работу со стеком заставок, начатую <see cref="PushThreadSplashStack()"/>.
    /// Если стек заставок, добавленный ранее вызовом <see cref="PushThreadSplashStack()"/>, реализует интерфейс <see cref="IDisposable"/>,
    /// то обязанность по вызову метода <see cref="IDisposable.Dispose()"/> лежит на вызывающем коде.
    /// </summary>
    public static void PopThreadSplashStack()
    {
      if (_ThreadSplashStackStack == null)
        throw ExceptionFactory.UnpairedCall(null, "PushThreadSplashStack()", "PopThreadSplashStack()");
      if (_ThreadSplashStackStack.Count == 0)
        throw ExceptionFactory.UnpairedCall(null, "PushThreadSplashStack()", "PopThreadSplashStack()");

      _ThreadSplashStack = _ThreadSplashStackStack.Pop();
    }

    [ThreadStatic]
    private static Stack<ISplashStack> _ThreadSplashStackStack;

    #endregion
  }

  /// <summary>
  /// Класс для временной замены заставки для фазы выполнения процедуры.
  /// Используется в пользовательском коде, который, возможно, выполняется внутри <see cref="FreeLibSet.Remoting.ExecProc.OnExecute(Remoting.NamedValues)"/>.
  /// Если есть текущая заставка (был вызов <see cref="FreeLibSet.Remoting.ExecProc.BeginSplash()"/> и есть текущая фаза),
  /// то меняется текст текущей фазы, очищается процентный индикатор и отключается кнопка "Отмена".
  /// Если процедура выполняется, но нет заставки, которой можно управлять, вызывается <see cref="FreeLibSet.Remoting.ExecProc.BeginSplash()"/>.
  /// Если процедура не выполняется, то объект использует фиктивную заставку.
  /// В любом случае, пользовательский код может управлять процентным индикатором, текстом заставки и кнопкой "Отменить".
  /// Используется текущий стек вызовов <see cref="SplashTools.ThreadSplashStack"/>, поэтому вызовы всех методов и <see cref="IDisposable.Dispose()"/>
  /// должны выполняться в одном потоке.
  /// После вызова <see cref="IDisposable.Dispose()"/> восстанавливается предыдущее состояние заставки.
  /// </summary>
  public sealed class TempSplashPhaseHandler : SimpleDisposableObject, ISimpleSplash
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Создает объект, встраивая его в существующую заставку или создавая новую
    /// </summary>
    /// <param name="phaseText">Текст для фазы. Не может быть пустой строкой.
    /// В дальнейшем может быть изменен установкой свойства <see cref="PhaseText"/>.</param>
    public TempSplashPhaseHandler(string phaseText)
    {
      if (String.IsNullOrEmpty(phaseText))
        throw new ArgumentNullException("phaseText");

      _Splash = SplashTools.ThreadSplashStack.Splash;
      if (_Splash == null)
      {
        _Splash = SplashTools.ThreadSplashStack.BeginSplash(phaseText);
        _EndSplashRequired = true;
      }
      else if (_Splash.Phase >= _Splash.PhaseCount)
      {
        // Уже был последний вызов Splash.Complete()
        // Такие же действия, как если бы заставки не было совсем
        _Splash = SplashTools.ThreadSplashStack.BeginSplash(phaseText);
        _EndSplashRequired = true;
      }
      else
      {
        _OldPhaseText = _Splash.PhaseText;
        _OldPercentMax = _Splash.PercentMax;
        _OldPercent = _Splash.Percent;
        _OldAllowCancel = _Splash.AllowCancel;
        _ShouldRestore = true;

        _Splash.PhaseText = phaseText;
        _Splash.SetPercent(0, 0);
        _Splash.AllowCancel = false;
      }
#if DEBUG
      _TheThread = Thread.CurrentThread;
#endif
    }

    /// <summary>
    /// Закрытие объекта, отсоединение его от заставки.
    /// Текущая заставка возвращается в исходное состояние
    /// </summary>
    /// <param name="disposing">Если true, то был вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
#if DEBUG
        CheckThread();
#endif

        if (_ShouldRestore)
        {
          _Splash.PhaseText = _OldPhaseText;
          _Splash.SetPercent(_OldPercent, _OldPercentMax);
          _Splash.AllowCancel = _OldAllowCancel;
        }

        if (_EndSplashRequired)
          SplashTools.ThreadSplashStack.EndSplash();
      }

      _Splash = null;

      base.Dispose(disposing);
    }

    #endregion

    #region Поля

    private ISplash _Splash;

    private bool _EndSplashRequired;

    private bool _ShouldRestore;

    private string _OldPhaseText;

    private int _OldPercent;

    private int _OldPercentMax;

    private bool _OldAllowCancel;

    #endregion

    #region ISimpleSplash

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public int PercentMax
    {
      get { return _Splash.PercentMax; }
      set { _Splash.PercentMax = value; }
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public int Percent
    {
      get { return _Splash.Percent; }
      set { _Splash.Percent = value; }
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="percentMax"></param>
    public void SetPercent(int percent, int percentMax)
    {
      _Splash.SetPercent(percent, percentMax);
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public void IncPercent()
    {
      _Splash.IncPercent();
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public bool AllowCancel
    {
      get { return _Splash.AllowCancel; }
      set { _Splash.AllowCancel = value; }
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public bool Cancelled
    {
      get { return _Splash.Cancelled; }
      set { _Splash.Cancelled = value; }
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    public void CheckCancelled()
    {
      _Splash.CheckCancelled();
    }

    /// <summary>
    /// Реализация метода интерфейса <see cref="ISimpleSplash"/>
    /// </summary>
    /// <param name="milliseconds">Интервал в миллисекундах</param>
    public void Sleep(int milliseconds)
    {
      _Splash.Sleep(milliseconds);
    }

    #endregion

    #region ISplash

    /// <summary>
    /// Текст фазы
    /// </summary>
    public string PhaseText
    {
      get { return _Splash.PhaseText; }
      set { _Splash.PhaseText = value; }
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

namespace FreeLibSet.Remoting
{
  #region Интерфейс IAsyncResultWithSplash

  /// <summary>
  /// Расширение интерфейса асинхронного управления <see cref="IAsyncResult"/> возможностями работы с процентным индикатором.
  /// </summary>
  public interface IAsyncResultWithSplash : IAsyncResult
  {
    /// <summary>
    /// Получить информацию о готовности задания (свойство <see cref="IAsyncResult.IsCompleted"/>)
    /// вместе с обновлением заставок процентных индикаторов (метод <see cref="IServerSplashWatcher.GetSplashInfoPack()"/>) за одно обращение к серверу.
    /// </summary>
    /// <param name="splashInfoPack">Порция обновления для экранной заставки</param>
    /// <returns>Свойство <see cref="IAsyncResult.IsCompleted"/></returns>
    bool GetIsCompleted(out SplashInfoPack splashInfoPack);

    // Нельзя передавать интерфейс от сервера к клиенту для RemoteExecProc, возникает исключение RemotingException
    // "Этот посредник удаленного взаимодействия не имеет приемника канала. Это означает, что либо сервер не имеет зарегистрированных каналов для прослушивания, 
    // либо это приложение не имеет подходящих каналов клиентов для связи с данным сервером."
    // Нужно продублировать методы IServerSplashWatcher
    ///// <summary>
    ///// Объект для извлечения информации о заставках с сервера
    ///// </summary>
    //IServerSplashWatcher ServerSplashWatcher { get; }

    /// <summary>
    /// Сброс данных.
    /// После вызова этого метода, при следующем вызове <see cref="GetIsCompleted(out SplashInfoPack)"/> будет возвращена полная информация о стеке заставок.
    /// Используется для исправления ошибок после разрыва соединения.
    /// Дублирует метод интерфейса <see cref="IServerSplashWatcher"/>.
    /// </summary>
    void ResetSplashInfo();

    /// <summary>
    /// Прервать выполнение процесса.
    /// Метод устанавливает свойство <see cref="FreeLibSet.Core.ISimpleSplash.Cancelled"/>=true, если в стеке есть текущая заставка.
    /// Дублирует метод интерфейса <see cref="IServerSplashWatcher"/>.
    /// </summary>
    void Cancel();
  }

  #endregion

  /// <summary>
  /// Содержит ссылки на интерфейсы <see cref="IAsyncResultWithSplash"/> и <see cref="IServerSplashWatcher"/>.
  /// Метод <see cref="AsyncResultWithSplashHandler.GetIsCompleted()"/> вызывает метод интерфейса <see cref="IAsyncResultWithSplash"/> и сохраняет во внутреннем поле объект <see cref="SplashInfoPack"/>.
  /// Реализует метод интерфейса <see cref="IServerSplashWatcher.GetSplashInfoPack()"/>. При этом возвращается сохраненный <see cref="SplashInfoPack"/>.
  /// Остальная реализация интерфейса <see cref="IServerSplashWatcher"/> переадресуется по существующей ссылке.
  /// Используется в <see cref="ExecProcCallItem"/>.
  /// </summary>
  public sealed class AsyncResultWithSplashHandler : IServerSplashWatcher
  {
    #region Конструктор

    /// <summary>
    /// Создает объект. Обращается к свойству <paramref name="asyncResult"/>.ServerSplashWatcher для получения ссылки на интерфейс IServerSplashWatcher
    /// </summary>
    /// <param name="asyncResult">Интерфейс <see cref="IAsyncResultWithSplash"/>. Не может быть null.</param>
    public AsyncResultWithSplashHandler(IAsyncResultWithSplash asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException("asyncResult");

      _AsyncResult = asyncResult;
    }

    #endregion

    #region Свойства

    //public IAsyncResultWithSplash AsyncResult { get { return _AsyncResult; } }
    private readonly IAsyncResultWithSplash _AsyncResult;

    /// <summary>
    /// Запоминаем между GetIsCompleted() и IServerSplashWatcher.GetSplashInfoPack()
    /// </summary>
    private SplashInfoPack _SplashInfoPack;

#if DEBUG
    private bool _GetIsCompletedCalled;
#endif

    #endregion

    #region Основной метод

    /// <summary>
    /// Вызывает <see cref="IAsyncResultWithSplash.GetIsCompleted(out SplashInfoPack)"/> и запоминает пакет до вызова <see cref="IServerSplashWatcher.GetSplashInfoPack()"/>.
    /// Методы <see cref="GetIsCompleted()"/> и <see cref="GetSplashInfoPack()"/> должны вызываться поочередно.
    /// </summary>
    /// <returns>True, если выполнение асинхронного действия завершено</returns>
    public bool GetIsCompleted()
    {
#if DEBUG
      if (_GetIsCompletedCalled)
        throw new InvalidOperationException("Second call for GetIsCompleted()");
#endif

      bool res = _AsyncResult.GetIsCompleted(out _SplashInfoPack);

#if DEBUG
      _GetIsCompletedCalled = true;
#endif

#if DEBUG_SPLASHWATCHERS
      if (_ServerSplashWatcher != null && _SplashInfoPack == null)
        throw new NullReferenceException(_AsyncResult.GetType().ToString() + ".GetIsCompleted() не вернул SplashInfoStack");
#endif

      return res;
    }

    #endregion

    #region IServerSplashWatcher Members

    /// <summary>
    /// Возвращает <see cref="SplashInfoPack"/>, сохраненный при предыдущем вызове <see cref="GetIsCompleted()"/>.
    /// Методы <see cref="GetIsCompleted()"/> и <see cref="GetSplashInfoPack()"/> должны вызываться поочередно.
    /// </summary>
    /// <returns>Информация об изменениях процентного индикатора, которая нужна объекту <see cref="ClientSplashWatcher"/>.</returns>
    public SplashInfoPack GetSplashInfoPack()
    {
#if DEBUG
      if (!_GetIsCompletedCalled)
        throw ExceptionFactory.UnpairedCall(this, "GetIsCompleted()", "GetSplashInfoPack()");
      _GetIsCompletedCalled = false;
#endif

      return _SplashInfoPack;
    }

    /// <summary>
    /// Вызывает соответствующий метод по ссылке на интерфейс <see cref="IAsyncResultWithSplash"/>.
    /// </summary>
    public void ResetSplashInfo()
    {
      _AsyncResult.ResetSplashInfo();
    }

    /// <summary>
    /// Вызывает соответствующий метод по ссылке на интерфейс <see cref="IAsyncResultWithSplash"/>.
    /// </summary>
    public void Cancel()
    {
      _AsyncResult.Cancel();
    }

    #endregion
  }
}
