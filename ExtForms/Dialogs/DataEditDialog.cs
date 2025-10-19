using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.UICore;
using FreeLibSet.Core;

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
    /// Установка свойства меняет значение свойств <see cref="ExtEditDialog.ReadOnly"/>, <see cref="ExtEditDialog.ShowApplyButton"/>, <see cref="ExtEditDialog.UseDataWriting"/> и <see cref="ExtEditDialog.ImageKey"/>.
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

        ReadOnly = (value == UIDataState.View);

        switch (value)
        {
          case UIDataState.Edit:
          case UIDataState.Insert:
          case UIDataState.InsertCopy:
            ShowApplyButton = true;
            UseDataWriting = true;
            break;
          default:
            ShowApplyButton = false;
            UseDataWriting = false;
            InitTitle();
            break;
        }
      }
    }
    private UIDataState _DataState;

    /// <summary>
    /// Заголовок документа.
    /// Если свойство установлено, то устанавливается заголовок формы в виде "(*) DocumentTitle (Действие)".
    /// Заголовок также меняется при установке свойства <see cref="DataState"/>.
    /// Если свойство не установлено (по умолчанию), то управление заголовком должно выполняться вызывающим кодом.
    /// </summary>
    public string DocumentTitle
    {
      get { return _DocumentTitle ?? String.Empty; }
      set
      {
        _DocumentTitle = value;
        InitTitle();
      }
    }
    private string _DocumentTitle;

    private void InitTitle()
    {
      if (String.IsNullOrEmpty(DocumentTitle))
        return;

      string s2;

      switch (DataState)
      {
        case UIDataState.Edit: s2 = Res.Editor_Msg_TitleEdit; break;
        case UIDataState.Insert: s2 = Res.Editor_Msg_TitleInsert; break;
        case UIDataState.InsertCopy: s2 = Res.Editor_Msg_TitleInsertCopy; break;
        case UIDataState.Delete: s2 = Res.Editor_Msg_TitleDelete; break;
        case UIDataState.View: s2 = Res.Editor_Msg_TitleView; break;
        default:
          throw new BugException("State=" + DataState.ToString());
      }

      Title = String.Format(Res.Editor_Msg_Title, DocumentTitle, s2);
    }

    #endregion
  }
}
