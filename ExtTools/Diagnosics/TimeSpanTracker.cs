using System;
using System.Collections.Generic;
using System.Text;
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
