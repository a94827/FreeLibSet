// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FreeLibSet
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
      DeadLockThreadObj threadObj = new DeadLockThreadObj();
      threadObj.Obj1 = new object();
      threadObj.Obj2 = new object();

      lock (threadObj.Obj1)
      {
        Thread trd = new Thread(threadObj.Execute);
        trd.Start();

        // Дожидаемся запуска процедуры потока, когда она заблокирует Obj2
        while (!threadObj.Flag)
          Thread.Sleep(50);

        lock (threadObj.Obj2) // это не может быть выполнено
        {
        }
      }
    }

    #endregion
  }
}
