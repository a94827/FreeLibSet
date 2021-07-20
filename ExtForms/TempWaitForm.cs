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
  /// <summary>
  /// Форма для вывода заставок EFPApp.BeginWait()/EndWait
  /// </summary>
  internal partial class TempWaitForm : TempBaseForm
  {
    #region Конструктор

    public TempWaitForm()
    {
      InitializeComponent();
    }

    #endregion

    #region Обработчики формы

    #endregion

    #region Статический метод запуска

    /// <summary>
    /// Статический экземпляр формы
    /// </summary>
    private static TempWaitForm _TheForm = null;

    #endregion

    public static void BeginWait(string message, int imageIndex, bool updateImmediately)
    {
      #region Значения по умолчанию

      if (_WaitInfoStack.Count > 0)
      {
        // Если какой-либо параметр не указан - берем данные от предыдущего уровня

        WaitInfo PrevObj = _WaitInfoStack.Peek();
        if (String.IsNullOrEmpty(message))
          message = PrevObj.Message;
        if (imageIndex < 0)
          imageIndex = PrevObj.ImageIndex;
      }
      else
      {
        if (String.IsNullOrEmpty(message))
          message = "Ждите";
        if (imageIndex < 0)
          imageIndex = EFPApp.MainImages.Images.IndexOfKey("HourGlass");
      }

      #endregion

      WaitInfo WaitObj = new WaitInfo();
      WaitObj.Message = message;
      WaitObj.ImageIndex = imageIndex;

      _WaitInfoStack.Push(WaitObj);

      try
      {
        if (_TheForm == null)
          _TheForm = new TempWaitForm();
        else if (_TheForm.IsDisposed)
          _TheForm = new TempWaitForm();

        _TheForm.TheLabel.Text = message;
        _TheForm.TheImg.Image = EFPApp.MainImages.Images[imageIndex];
        _TheForm.Visible = true;
        if (updateImmediately)
          _TheForm.Update(); // 11.12.2018 
      }
      catch (Exception e) // 06.06.2017
      {
        LogBeginEndWaitException(e);
      }
    }

    internal static void EndWait()
    {
      //#if DEBUG
      //      CheckTread();
      //#endif

      // Убираем себя из стека
      _WaitInfoStack.Pop();

      try
      {
        if (_WaitInfoStack.Count == 0)
          _TheForm.Visible = false;
        else
        {
          WaitInfo WaitObj = _WaitInfoStack.Peek();
          _TheForm.TheLabel.Text = WaitObj.Message;
          _TheForm.TheImg.Image = EFPApp.MainImages.Images[WaitObj.ImageIndex];
        }
      }
      catch (Exception e) // 06.06.2017
      {
        LogBeginEndWaitException(e);
      }

    }

    /// <summary>
    /// Признак однократного логгирования ошибки в BeginWait() / EndWait()
    /// </summary>
    private static bool _BeginEndWaitErrorLogged = false;

    private static void LogBeginEndWaitException(Exception e)
    {
      if (_BeginEndWaitErrorLogged)
        return;
      _BeginEndWaitErrorLogged = true;
      LogoutTools.LogoutException(e, "Перехват ошибки EFPApp.BeginWait()/EndWait(). Повторные сообщения не выводятся");
    }


    /// <summary>
    /// Объект для стека сообщений. Для вызовов WaitCursor без аргументов
    /// в стеке хранятся значения null
    /// </summary>
    private class WaitInfo
    {
      #region Поля

      public string Message;
      public int ImageIndex;

      #endregion
    }

    /// <summary>                
    /// Стек вызовов
    /// </summary>
    private static Stack<WaitInfo> _WaitInfoStack = new Stack<WaitInfo>();
  }
}