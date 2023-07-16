using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;

namespace EFPAppRemoteExitDemo
{
  /// <summary>
  /// Эмуляция работы клиента.
  /// Один раз в секунду опрашивается "сервер".
  /// Если на нем инициализировано завершение работы, запускаем процедуру удаленного завершения
  /// </summary>
  public class ClientHandler:IEFPAppTimeHandler
  {
    #region Конструктор

    public ClientHandler(ServerForm ServerForm)
    {
      this.ServerForm = ServerForm;
      EFPApp.Timers.Add(this); // на все время работы приложения
    }

    #endregion

    #region Поля

    ServerForm ServerForm;

    #endregion

    #region IEFPAppTimeHandler Members

    public void TimerTick()
    {
      // В настоящем приложении выполняется удаленный вызов сервера.
      // И, наверное, не один раз в секунду, а реже

      bool Exit;
      string Message;
      if (ServerForm.ClientQuery(out Exit, out Message))
      {
        EFPApp.RemoteExitHandler.Active = Exit;
        EFPApp.RemoteExitHandler.Message = Message;
      }

      string txt = EFPApp.RemoteExitHandler.State.ToString();
      if (EFPApp.RemoteExitHandler.State == EFPAppRemoteExitState.Started)
        txt += " Ожидание " + (EFPApp.RemoteExitHandler.RemainderTime/1000).ToString() + " с.";
      ServerForm.SetClientStatus(txt);
    }

    #endregion
  }
}
