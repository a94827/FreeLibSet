// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Позволяет задавать позицию блока диалога на экране при использовании соответствующей
  /// перегрузки <see cref="EFPApp.ShowDialog(Form, bool, EFPDialogPosition)"/>.
  /// Экземпляр класса существует в некоторых стандартных блоках диалога ExtForms, например,
  /// <see cref="RadioSelectDialog"/>, что позволяет выводить их в нужной позиции.
  /// </summary>
  public class EFPDialogPosition : ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// По умолчанию блок диалога позиционируется по центру экрана, на котором находится главное
    /// окно приложения или активный блок диалога (см. свойство <see cref="EFPApp.DefaultScreen"/>).
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
    /// Для позиционирования диалога будет использован метод <see cref="WinFormsTools.PlacePopupForm(Form, Control)"/>.
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
    /// Для позиционирования диалога будет использован метод <see cref="WinFormsTools.PlacePopupForm(Form, Rectangle)"/>.
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
