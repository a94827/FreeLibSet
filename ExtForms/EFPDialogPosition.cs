using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
  /// Позволяет задавать позицию блока диалога на экране при использовании соответствующей
  /// перегрузки EFPApp.ShowDialog().
  /// Экземпляр класса существует в некоторых стандартных блоках диалога ExtForms, например,
  /// RadioSelectDialog, что позволяет выводить их в нужной позиции.
  /// </summary>
  public class EFPDialogPosition : ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// По умолчанию блок диалога позиционируется по центру экрана, на котором находится главное
    /// окно приложения или активный блок диалога (см. свойство EFPApp.DefaultScreen).
    /// </summary>
    public EFPDialogPosition()
    {
    }

    /// <summary>
    /// Создает позицию для выпадающего окна
    /// </summary>
    /// <param name="popupOwnerControl">Управляющий элемент, откуда будет "выпадать" окно</param>
    public EFPDialogPosition(Control popupOwnerControl)
      : this()
    {
      this.PopupOwnerControl = popupOwnerControl;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если свойство установлено, то диалог будет выводится как выпадающее окно для заданного элемента
    /// (снизу или сверху, в зависимости от наличия свободного места).
    /// Для позиционирования диалога будет использован метод WinFormsTools.PlacePopupForm().
    /// </summary>
    public Control PopupOwnerControl
    {
      get { return _PopupOwnerControl; }
      set
      {
        _PopupOwnerBounds = Rectangle.Empty;
        if (value != null)
        {
          if (value.IsDisposed)
            throw new ObjectDisposedException("PopupOwnerControl");
        }
        _PopupOwnerControl = value;
      }
    }
    private Control _PopupOwnerControl;

    /// <summary>
    /// Если свойство установлено, то диалог будет выводится как выпадающее окно,
    /// снизу или сверху от заданной области, в зависимости от наличия свободного места).
    /// Область должна задаваться в экранных координатах.
    /// Для позиционирования диалога будет использован метод WinFormsTools.PlacePopupForm().
    /// </summary>
    public Rectangle PopupOwnerBounds
    {
      get { return _PopupOwnerBounds; }
      set
      {
        _PopupOwnerControl = null;
        _PopupOwnerBounds = value;
      }
    }
    private Rectangle _PopupOwnerBounds;

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_PopupOwnerControl != null)
        return "Popup для элемента: " + _PopupOwnerControl.ToString();
      else if (!_PopupOwnerBounds.IsEmpty)
        return "Popup для области: " + _PopupOwnerBounds.ToString();
      else
        return "По центру экрана";
    }

    #endregion

    #region Копирование

    /// <summary>
    /// Копирует поля текущего объекта в другой объект
    /// </summary>
    /// <param name="dest">Заполняемый оьъект</param>
    public void CopyTo(EFPDialogPosition dest)
    {
#if DEBUG
      if (dest == null)
        throw new ArgumentNullException("dest");
#endif

      dest._PopupOwnerBounds = _PopupOwnerBounds;
      dest._PopupOwnerControl = _PopupOwnerControl;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Новый объект</returns>
    public EFPDialogPosition Clone()
    {
      EFPDialogPosition res = new EFPDialogPosition();
      this.CopyTo(res);
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }
}
