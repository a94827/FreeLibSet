using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtForms;

namespace EFPAppRemoteExitDemo
{
  /// <summary>
  /// �������� ������ �������.
  /// ���� ��� � ������� ������������ "������".
  /// ���� �� ��� ���������������� ���������� ������, ��������� ��������� ���������� ����������
  /// </summary>
  public class ClientHandler:IEFPAppTimeHandler
  {
    #region �����������

    public ClientHandler(ServerForm ServerForm)
    {
      this.ServerForm = ServerForm;
      EFPApp.Timers.Add(this); // �� ��� ����� ������ ����������
    }

    #endregion

    #region ����

    ServerForm ServerForm;

    #endregion

    #region IEFPAppTimeHandler Members

    public void TimerTick()
    {
      // � ��������� ���������� ����������� ��������� ����� �������.
      // �, ��������, �� ���� ��� � �������, � ����

      bool Exit;
      string Message;
      if (ServerForm.ClientQuery(out Exit, out Message))
      {
        EFPApp.RemoteExitHandler.Active = Exit;
        EFPApp.RemoteExitHandler.Message = Message;
      }

      string txt = EFPApp.RemoteExitHandler.State.ToString();
      if (EFPApp.RemoteExitHandler.State == EFPAppRemoteExitState.Started)
        txt += " �������� " + (EFPApp.RemoteExitHandler.RemainderTime/1000).ToString() + " �.";
      ServerForm.SetClientStatus(txt);
    }

    #endregion
  }
}
