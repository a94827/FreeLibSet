// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using FreeLibSet.Remoting;
using System.Threading;
using System.Windows.Forms;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Функции работы с файлами и каталогами.
  /// Расширяет методы класса FileTools выводом заставок
  /// </summary>
  public static class EFPFileTools
  {
    #region Асинхронное выполнение с заставкой

    /// <summary>
    /// Процедура, которую можно выполнять асинхронно, но с ожиданием
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private delegate NamedValues ExecDelegate(NamedValues args);

    private class ThreadInfo
    {
      #region Поля

      public ExecDelegate Proc;

      public NamedValues Args;

      public volatile NamedValues Res;

      public volatile Exception Exception;

      #endregion

      #region Процедура в потоке

      public void AsyncProc(object dummy)
      {
        try
        {
          Res = Proc(Args);
          if (Res == null)
            throw new BugException("Процедура не вернула результат");
        }
        catch (Exception e)
        {
          e.Data["OrgStackTrace"] = e.StackTrace;
          Exception = e;
        }
      }

      #endregion
    }

    private static NamedValues ExecuteAsync(ExecDelegate proc, NamedValues args, string message, string imageKey)
    {
      ThreadInfo info = new ThreadInfo();
      info.Proc = proc;
      info.Args = args;

      EFPApp.BeginWait(message, imageKey);
      try
      {
        if (!ThreadPool.QueueUserWorkItem(info.AsyncProc))
          throw new InvalidOperationException("Не удалось поставить процедуру в очередь");

        while (info.Res == null && info.Exception == null)
        {
          Application.DoEvents();
          Thread.Sleep(0);
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (info.Exception == null)
        return info.Res;
      else
        throw (info.Exception);
    }
    #endregion

    #region Проверка имени файла и каталога

    /// <summary>
    /// Проверка имени каталога, завершающегося обратной чертой.
    /// Может проверять реальное существование каталога, в зависимости от режима.
    /// В режиме <paramref name="mode"/>=None всегда возвращает true.
    /// </summary>
    /// <param name="dirName">Имя каталога, выбранное пользователем</param>
    /// <param name="mode">Режим проверки. Значение FileExists не допускается</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true - имя каталога правильное</returns>
    public static bool TestDirSlashedPath(string dirName, TestPathMode mode, out string errorText)
    {
      bool res;
      switch (mode)
      {
        case TestPathMode.None:
        case TestPathMode.FormatOnly:
          // Заставка не нужна
          res = FileTools.TestDirSlashedPath(dirName, mode, out errorText);
          break;
        default:
          // res = FileTools.TestDirSlashedPath(dirName, mode, out errorText);
          NamedValues dispargs = new NamedValues();
          dispargs["DirName"] = dirName;
          dispargs["Mode"] = mode;
          NamedValues dispres = ExecuteAsync(AsyncTestDirSlashedPath, dispargs, "Проверка существования каталога \"" + dirName + "\"", "Open");
          errorText = (string)(dispres["ErrorText"]);
          res = (bool)(dispres["Res"]);
          break;
      }
      return res;
    }

    private static NamedValues AsyncTestDirSlashedPath(NamedValues dispargs)
    {
      string dirName = (string)(dispargs["DirName"]);
      TestPathMode mode = (TestPathMode)(dispargs["Mode"]);
      NamedValues dispres = new NamedValues();
      string errorText;
      dispres["Res"] = FileTools.TestDirSlashedPath(dirName, mode, out errorText);
      dispres["ErrorText"] = errorText;
      return dispres;
    }

    /// <summary>
    /// Проверка имени файла
    /// Может проверять реальное существование каталога и файла, в зависимости от режима.
    /// В режиме <paramref name="mode"/>=None всегда возвращает true.
    /// </summary>
    /// <param name="fileName">Имя файла, выбранное пользователем</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <param name="mode">Режим проверки</param>
    /// <returns>true - имя файла правильное</returns>
    public static bool TestFilePath(string fileName, TestPathMode mode, out string errorText)
    {
      bool res;
      switch (mode)
      {
        case TestPathMode.None:
        case TestPathMode.FormatOnly:
          // Заставка не нужна
          res = FileTools.TestFilePath(fileName, mode, out errorText);
          break;
        default:
          NamedValues dispargs = new NamedValues();
          dispargs["FileName"] = fileName;
          dispargs["Mode"] = mode;
          NamedValues dispres = ExecuteAsync(AsyncTestFilePath, dispargs, "Проверка существования файла \"" + fileName + "\"", "Open");
          errorText = (string)(dispres["ErrorText"]);
          res = (bool)(dispres["Res"]);
          break;
      }
      return res;
    }

    private static NamedValues AsyncTestFilePath(NamedValues dispargs)
    {
      string fileName = (string)(dispargs["FileName"]);
      TestPathMode mode = (TestPathMode)(dispargs["Mode"]);
      NamedValues dispres = new NamedValues();
      string errorText;
      dispres["Res"] = FileTools.TestFilePath(fileName, mode, out errorText);
      dispres["ErrorText"] = errorText;
      return dispres;
    }



    #endregion
  }
}
