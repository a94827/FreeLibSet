// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FreeLibSet.Diagnostics
{
  /// <summary>
  /// Отслеживание прошедшего времени.
  /// Создается объект TimeSpanTracker. В конструкторе запоминается текущее время.
  /// Периодически (по сигналу таймера, с любой периодичностью) вызывается метод 
  /// Update(), которому передается аргумент Append. Если Add=true, то прошедшее с
  /// проследнего вызова время добававляется к интервалу, иначе - пропускается
  /// </summary>
  public class TimeSpanTracker
  {
    #region Конструктор

    /// <summary>
    /// Создает объект и запоминает текущее время
    /// </summary>
    public TimeSpanTracker()
    {
      _PrevTicks = Stopwatch.GetTimestamp();
      _CurrentTicks = 0;
    }

    #endregion

    #region Прошедшее время

    /// <summary>
    /// Накопленный интервал времени
    /// </summary>
    public TimeSpan Current { get { return TimeSpan.FromSeconds((double)_CurrentTicks / (double)Stopwatch.Frequency); } }
    private long _CurrentTicks;

    private long _PrevTicks;

    /// <summary>
    /// Текстовое представление - выводит значение Current
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Current.ToString();
    }

    #endregion

    #region Сигнал таймера

    /// <summary>
    /// Добавляет время, прошедшее с предыдущего вызова
    /// </summary>
    public void Update()
    {
      Update(true);
    }

    /// <summary>
    /// Добавляет или пропускает время, прошедшее с предыдущего вызова
    /// </summary>
    /// <param name="append">true, если значение Current должно быть увеличено</param>
    public void Update(bool append)
    {
      long thisTicks = Stopwatch.GetTimestamp();
      long delta = thisTicks - _PrevTicks;
      if (delta > 0L)
      {
        // При переводе часов могут получиться отрицательные интервалы времени. Их пропускаем
        if (append)
          _CurrentTicks += delta;
      }
      _PrevTicks = thisTicks;
    }

    #endregion
  }
}
