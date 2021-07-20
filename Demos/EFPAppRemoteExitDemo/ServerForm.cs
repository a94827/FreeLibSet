using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;

namespace EFPAppRemoteExitDemo
{
  public partial class ServerForm : Form
  {
    #region ����������� �����

    public ServerForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpMessage = new EFPTextBox(efpForm, edMessage);
      efpMessage.CanBeEmpty = true;
      efpMessage.WarningIfEmpty = true;

      btnExit.Image = EFPApp.MainImages.Images["Exit"];
      btnExit.ImageAlign = ContentAlignment.MiddleLeft;
      efpExit = new EFPButton(efpForm, btnExit);
      efpExit.Click += new EventHandler(efpExit_Click);

      btnResume.Image = EFPApp.MainImages.Images["Cancel"];
      btnResume.ImageAlign = ContentAlignment.MiddleLeft;
      efpResume = new EFPButton(efpForm, btnResume);
      efpResume.Click += new EventHandler(efpResume_Click);

      DebugFormDispose.SetPersist(this); // ����� �� �������
    }

    #endregion

    #region ������� ���������

    /// <summary>
    /// ���� ����� ���������� ��������
    /// </summary>
    /// <param name="Exit">true - ���� ���������� ������</param>
    /// <param name="Message">false - ���� ������������ ������</param>
    /// <returns>������� ������� �� ����������� / ����������� ������</returns>
    public bool ClientQuery(out bool Exit, out string Message)
    {
      bool Res;
      if (FExitQuery.HasValue)
      {
        Res = true;
        Exit = FExitQuery.Value;
        Message = FExitMessage;
      }
      else
      {
        Res = false;
        Exit = false;
        Message = null;
      }
      FExitQuery = null;
      return Res;
    }

    private bool? FExitQuery;

    private string FExitMessage;

    /// <summary>
    /// ���� ������� ������ ���������� ���� ���������
    /// </summary>
    /// <param name="Text">��������� �������</param>
    public void SetClientStatus(string Text)
    {
      if (!IsDisposed)
        lblClientStatus.Text = Text;
    }

    #endregion

    #region ����������� �����

    EFPTextBox efpMessage;

    EFPButton efpExit, efpResume;

    void efpExit_Click(object Sender, EventArgs Args)
    {
      FExitQuery = true;
      FExitMessage = efpMessage.Text;
    }

    void efpResume_Click(object Sender, EventArgs Args)
    {
      FExitQuery = false;
    }

    #endregion
  }
}