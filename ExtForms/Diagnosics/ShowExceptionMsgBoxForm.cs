// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.IO;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms.Diagnostics
{
  internal partial class ShowExceptionMsgBoxForm : Form
  {
    #region Конструктор

    public ShowExceptionMsgBoxForm()
    {
      InitializeComponent();
      btnMore.Image = EFPApp.MainImages.Images["Debug"];
      btnMore.ImageAlign = ContentAlignment.MiddleLeft;

      EFPFormProvider efpForm = new EFPFormProvider(this);
      EFPButton efpMore = new EFPButton(efpForm, btnMore);
      efpMore.Click += new EventHandler(efpMore_Click);
    }

    void efpMore_Click(object sender, EventArgs args)
    {
      if (LogFilePath.IsEmpty)
        LogFilePath = LogoutTools.LogoutExceptionToFile(Exception, this.Text); // 02.09.2020. Запись в файл выполняется только при открытии детальной формы.

      ShowExceptionForm.ShowException(Exception, this.Text, null, LogFilePath);
    }

    #endregion

    #region Поля

    public Exception Exception;

    public AbsPath LogFilePath;

    #endregion
  }
}