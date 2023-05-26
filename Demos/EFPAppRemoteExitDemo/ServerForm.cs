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
    #region Конструктор формы

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

      DebugFormDispose.SetPersist(this); // чтобы не ругался
    }

    #endregion

    #region Текущее состояние

    /// <summary>
    /// Этот метод вызывается клиентом
    /// </summary>
    /// <param name="Exit">true - надо прекратить работу</param>
    /// <param name="Message">false - надо возобновлить работу</param>
    /// <returns>Наличие сигнала на прекращение / продолжение работы</returns>
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
    /// Этим методом клиент показывает свое состояние
    /// </summary>
    /// <param name="Text">Состояние клиента</param>
    public void SetClientStatus(string Text)
    {
      if (!IsDisposed)
        lblClientStatus.Text = Text;
    }

    #endregion

    #region Обработчики формы

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
