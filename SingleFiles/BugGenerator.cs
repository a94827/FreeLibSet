// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FreeLibSet
{
  /// <summary>
  /// ��������� ������ (��� ������������)
  /// </summary>
  public static class BugGenerator
  {
    #region Dead lock

    private class DeadLockThreadObj
    {
      #region ����

      public object Obj1;

      public object Obj2;

      public volatile bool Flag;

      #endregion

      #region ����������� ���������

      public void Execute()
      {
        lock (Obj2)
        {
          Flag = true;
          lock (Obj1) // �� ����� ���� ���������
          { 
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// ������� "������� ����������" � ������� ������.
    /// ����� ��������� �������������� �����, � ����� ������� �������� ���������� ����� ������ � �������, ������ Monitor.Enter()
    /// ��� ���� �������� � ��������������� �������
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

        // ���������� ������� ��������� ������, ����� ��� ����������� Obj2
        while (!ThreadObj.Flag)
          Thread.Sleep(50);

        lock (ThreadObj.Obj2) // ��� �� ����� ���� ���������
        { 
        }
      }
    }

    #endregion
  }
}