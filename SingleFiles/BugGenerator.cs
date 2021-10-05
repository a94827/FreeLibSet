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

namespace AgeyevAV
{
  /// <summary>
  /// Генерация ошибок (для тестирования)
  /// </summary>
  public static class BugGenerator
  {
    #region Dead lock

    private class DeadLockThreadObj
    {
      #region Поля

      public object Obj1;

      public object Obj2;

      public volatile bool Flag;

      #endregion

      #region Выполняемая процедура

      public void Execute()
      {
        lock (Obj2)
        {
          Flag = true;
          lock (Obj1) // не может быть выполнена
          { 
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Создать "мертвую блокировку" в текущем потоке.
    /// Метод запускает дополнительный поток, а затем создает взаимную блокировку этого потока с текущим, вызвав Monitor.Enter()
    /// для двух объектов в противоположном порядке
    /// </summary>
    public static void PerformDeadLock()
    {
      DeadLockThreadObj ThreadObj = new DeadLockThreadObj();
      ThreadObj.Obj1 = new object();
      ThreadObj.Obj2 = new object();

      lock (ThreadObj.Obj1)
      {
        Thread trd = new Thread(ThreadObj.Execute);
        trd.Start();

        // Дожидаемся запуска процедуры потока, когда она заблокирует Obj2
        while (!ThreadObj.Flag)
          Thread.Sleep(50);

        lock (ThreadObj.Obj2) // это не может быть выполнено
        { 
        }
      }
    }

    #endregion
  }
}