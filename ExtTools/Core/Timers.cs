using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


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

#if XXX

namespace FreeLibSet.Timers
{
  /// <summary>
  /// Таймер, похожий на System.Timers.Timer. Не использует пул потоков, вместо этого 
  /// используется собственный поток, который спит большую часть времени.
  /// Гарантируется отсутствие реентрабельности.
  /// Класс является потокобезопасным.
  /// </summary>
  public sealed class FixedThreadTimer:DisposableObject
  {
    #region Конструктор и Dispose

    public FixedThreadTimer()
    {
      _Interval = 100;
      _Thread=new Thread(
    }

    protected override void Dispose(bool Disposing)
    {
      base.Dispose(Disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интервал времени в миллисекундах.
    /// По умолчанию - 100 мс.
    /// </summary>
    public int Interval 
    {
      get { return _Interval; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException();

        _Interval=value;
      }
    }
    private int _Interval;

    /// <summary>
    /// true, если событие Tick должно периодически вызываться.
    /// По умолчанию - false - таймер выключен
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set 
      {
      }
    }
    private bool _Enabled;

    #endregion

    #region Событие Tick

    /// <summary>
    /// Событие, вызываемое по таймеру
    /// </summary>
    public event EventHandler Tick;

    #endregion

    #region Поток

    /// <summary>
    /// Поток, в котором вызывается событие Tick.
    /// Создается при первой установке свойства Enabled=true и больше не меняется
    /// </summary>
    private Thread _Thread;

    #endregion

    #region Ожидание времени

    private AutoResetEvent _Event;

    #endregion
  }
}
#endif