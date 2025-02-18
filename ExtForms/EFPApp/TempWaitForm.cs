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

    #region Статический метод запуска

    /// <summary>
    /// Статический экземпляр формы
    /// </summary>
    private static TempWaitForm _TheForm = null;

    public static void BeginWait(string message, string imageKey, bool updateImmediately)
    {
      #region Значения по умолчанию

      if (_WaitInfoStack.Count > 0)
      {
        // Если какой-либо параметр не указан - берем данные от предыдущего уровня

        WaitInfo prevObj = _WaitInfoStack.Peek();
        if (String.IsNullOrEmpty(message))
          message = prevObj.Message;
        if (String.IsNullOrEmpty(imageKey))
          imageKey = prevObj.ImageKey;
      }
      else
      {
        if (String.IsNullOrEmpty(message))
          message = Res.EFPApp_Phase_Wait;
      }

      #endregion

      WaitInfo waitObj = new WaitInfo();
      waitObj.Message = message;
      if (String.IsNullOrEmpty(imageKey))
        waitObj.ImageKey = "HourGlass";
      else
        waitObj.ImageKey = imageKey;

      _WaitInfoStack.Push(waitObj);

      try
      {
        if (_TheForm == null)
          _TheForm = new TempWaitForm();
        else if (_TheForm.IsDisposed)
          _TheForm = new TempWaitForm();

        _TheForm.TheLabel.Text = message;
        _TheForm.TheImg.Image = EFPApp.MainImages.Images[imageKey];
        _TheForm.Visible = true;
        if (updateImmediately)
          _TheForm.Update(); // 11.12.2018 
      }
      catch (Exception e) // 06.06.2017
      {
        LogBeginEndWaitException(e);
      }
    }

    public static void EndWait()
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
          WaitInfo waitObj = _WaitInfoStack.Peek();
          _TheForm.TheLabel.Text = waitObj.Message;
          _TheForm.TheImg.Image = EFPApp.MainImages.Images[waitObj.ImageKey];
        }
      }
      catch (Exception e) // 06.06.2017
      {
        LogBeginEndWaitException(e);
      }
    }

    #endregion

    #region Обработка ошибок

    /// <summary>
    /// Признак однократного логгирования ошибки в BeginWait() / EndWait()
    /// </summary>
    private static bool _BeginEndWaitErrorLogged = false;

    private static void LogBeginEndWaitException(Exception e)
    {
      if (_BeginEndWaitErrorLogged)
        return;
      _BeginEndWaitErrorLogged = true;
      LogoutTools.LogoutException(e, Res.EFPApp_ErrTitle_Wait);
    }

    #endregion

    #region Стек сообщений

    /// <summary>
    /// Объект для стека сообщений. Для вызовов WaitCursor без аргументов
    /// в стеке хранятся значения null
    /// </summary>
    private class WaitInfo
    {
      #region Поля

      public string Message;
      public string ImageKey;

      #endregion
    }

    /// <summary>                
    /// Стек вызовов
    /// </summary>
    private static Stack<WaitInfo> _WaitInfoStack = new Stack<WaitInfo>();

    #endregion
  }
}
