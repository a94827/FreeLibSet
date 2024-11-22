using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Data
{
  /// <summary>
  /// Расширение диалога <see cref="ExtEditDialog"/> для поддержки состояний <see cref="UIDataState"/>
  /// </summary>
  public class DataEditDialog : ExtEditDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог.
    /// </summary>
    public DataEditDialog()
    {
      DataState = UIDataState.Edit;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее состояние (по умолчанию - <see cref="UIDataState.Edit"/>).
    /// Установка свойства меняет значение <see cref="ExtEditDialog.ImageKey"/>.
    /// </summary>
    public UIDataState DataState
    {
      get { return _DataState; }
      set
      {
        _DataState = value;
        ImageKey = EFPApp.GetDataStateImageKey(value);
        switch (value)
        {
          case UIDataState.Insert:
          case UIDataState.InsertCopy:
            base.SaveCurrentPage = false;
            break;
          default:
            base.SaveCurrentPage = true;
            break;
        }
        ReadOnly = (DataState == UIDataState.View);
      }
    }
    private UIDataState _DataState;

    #endregion
  }
}
