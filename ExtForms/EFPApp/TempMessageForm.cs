// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  internal partial class TempMessageForm : TempBaseForm
  {
    #region Конструктор

    public TempMessageForm()
    {
      InitializeComponent();
      AutoScaleMode = AutoScaleMode.None;
    }

    #endregion

    #region Обработчик формы

    private void TempTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs args)
    {
      try
      {
        TempTimer.Enabled = false;
        SetTempMessage(null);
      }
      catch
      {
      }
    }

    void MsgLabel_Click(object sender, EventArgs args)
    {
      try
      {
        Visible = false;
      }
      catch
      {
      }
    }

    #endregion

    #region Статический метод запуска

    /// <summary>
    /// Статический экземпляр формы
    /// </summary>
    private static TempMessageForm _TheForm = null;

    public static void SetTempMessage(string s)
    {
      if (_TheForm == null)
      {
        if (String.IsNullOrEmpty(s))
          return;

        _TheForm = new TempMessageForm();
      }
      else if (_TheForm.IsDisposed)
        _TheForm = new TempMessageForm();

      if (!String.IsNullOrEmpty(s))
      {
        s = s.Replace("\r", string.Empty);
        s = s.Replace('\n', ' ');
        s = s.Replace('\t', ' ');
        _TheForm.MsgLabel.Text = "  " + s;
      }

      // Таймер надо выключить и включить снова, чтобы заново отсчитать время
      _TheForm.TempTimer.Enabled = false;
      if (String.IsNullOrEmpty(s))
      {
        _TheForm.Visible = false;
      }
      else
      {
        _TheForm.Visible = true;
        _TheForm.TempTimer.Enabled = true;
      }
    }

    #endregion
  }
}
