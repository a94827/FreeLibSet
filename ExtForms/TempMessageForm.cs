using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.Logging;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms
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