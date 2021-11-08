using System;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Data;
using System.Drawing;
using System.Collections.Generic;

using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.UICore;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

// Блоки диалога для выбора документов и поддокументов

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Режимы выбора документов и поддокументов в диалогах DocSelectDialog и SubDocSelectDialog 
  /// </summary>
  public enum DocSelectionMode
  {
    /// <summary>
    /// Разрешается выбор только одного документа или поддокумента.
    /// Этот режим задан по умолчанию
    /// </summary>
    Single,

    /// <summary>
    /// Выбор нескольких документов или поддокументов выделением нескольких строк мышью или клавиши Shift/Control+стрелочки.
    /// Этот режим удобен, только если есть простая возможность выводить диалог выбора несколько раз подряд.
    /// </summary>
    MultiSelect,

    /// <summary>
    /// Выбор с помощью флажков
    /// </summary>
    MultiCheckBoxes,

    /// <summary>
    /// Показывается список с выбранными документами. В него можно добавлять документы из полного списка.
    /// </summary>
    MultiList
  }

  /// <summary>
  /// Диалог выбора одного или нескольких документов заданного вида
  /// </summary>
  public sealed class DocSelectDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог со значениями по умолчанию
    /// </summary>
    /// <param name="docTypeUI">Интерфейс для вида документа. Ссылка должна быть задана</param>
    public DocSelectDialog(DocTypeUI docTypeUI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      _DocTypeUI = docTypeUI;
      _SelectionMode = DocSelectionMode.Single;
      _DocIds = DataTools.EmptyIds;
      _CanBeEmpty = false;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс для доступа к документам.
    /// Задается в конструкторе.
    /// Не может быть null
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Режим выбора документа Single, MultiSelect, MultiCheckBoxes, MultiList.
    /// По умолчанию - Single
    /// </summary>
    public DocSelectionMode SelectionMode
    {
      get { return _SelectionMode; }
      set
      {
        switch (value)
        {
          case DocSelectionMode.Single:
          case DocSelectionMode.MultiSelect:
          case DocSelectionMode.MultiCheckBoxes:
          case DocSelectionMode.MultiList:
            _SelectionMode = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private DocSelectionMode _SelectionMode;

    /// <summary>
    /// Альтернативное значение для свойства SelectionMode.
    /// Содержит true, если разрешен выбор нескольких документов
    /// </summary>
    public bool MultiSelect
    {
      get { return _SelectionMode != DocSelectionMode.Single; }
      set { _SelectionMode = value ? DocSelectionMode.MultiSelect : DocSelectionMode.Single; }
    }

    /// <summary>
    /// Вход-выход. Идентификатор выбранного документа при MultiSelect=false
    /// </summary>
    public Int32 DocId
    {
      get
      {
        if (_DocIds.Length == 0)
          return 0;
        else
          return _DocIds[0];
      }
      set
      {
        if (value == 0)
          _DocIds = DataTools.EmptyIds;
        else
          _DocIds = new Int32[1] { value };
      }
    }

    /// <summary>
    /// Вход-выход. Идентификаторы выбранных документов при MultiSelect=true.
    /// </summary>
    public Int32[] DocIds
    {
      get { return _DocIds; }
      set
      {
        if (value == null)
          _DocIds = DataTools.EmptyIds;
        else
          _DocIds = value;
      }
    }
    private Int32[] _DocIds;

    /// <summary>
    /// Заголовок блока диалога.
    /// По умолчанию: "Выбор документа XXX" или "Выбор документов XXX" при MultiSelect=true
    /// </summary>
    public string Title
    {
      get
      {
        if (_Title == null)
        {
          if (MultiSelect)
            return "Выбор документов \"" + DocTypeUI.DocType.PluralTitle + "\"";
          else
            return "Выбор документа \"" + DocTypeUI.DocType.SingularTitle + "\"";
        }
        else
          return _Title;
      }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// Определяет наличие в диалоге кнопки "Нет", чтобы можно было установить DocId=0.
    /// По умолчанию - false;
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Набор фиксированных фильтров табличного просмотра.
    /// Значение null (по умолчанию) приводит к использованию текущего набора
    /// установленных пользователем фильтров.
    /// Действует только при FixedDocIds=null.
    /// </summary>
    public GridFilters Filters { get { return _Filters; } set { _Filters = value; } }
    private GridFilters _Filters;

    /// <summary>
    /// Массив фиксированных идентификаторов для выбора.
    /// Если null (по умолчанию), то выбор осуществляется с помощью формы DocTableViewForm, включая табличку фильтров.
    /// Если свойство установлено, то выбор осуществляется с помощью упрощенной формы
    /// </summary>
    public IdList FixedDocIds
    {
      get { return _FixedDocIds; }
      set { _FixedDocIds = value; }
    }
    private IdList _FixedDocIds;

    /// <summary>
    /// Внешний объект, определяющий начальные значения при создании документа внутри этого диалога.
    /// Если задан, то должен определять все значения, чтобы документ прошел условия фильтров, если заданы фильтры в свойстве Filters,
    /// т.к. сами фильтры не используются для установки значений.
    /// По умолчанию - null.
    /// Действует только при FixedDocIds=null.
    /// </summary>
    public DocumentViewHandler EditorCaller { get { return _EditorCaller; } set { _EditorCaller = value; } }
    private DocumentViewHandler _EditorCaller;

    /// <summary>
    /// Позиция вывода блока диалога.
    /// Может либо меняться существующий объект, либо быть передана ссылка на новый объкт EFPDialogPosition.
    /// Используется в комбоблоках
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region Вывод блока диалога

    #region Основной метод ShowDialog()

    /// <summary>
    /// Выводит блок диалога.
    /// Возвращает Ok, если пользователь сделал выбор, в том числе нажал кнопку "Нет"
    /// </summary>
    /// <returns></returns>
    public DialogResult ShowDialog()
    {
      if (FixedDocIds == null)
      {
        if (SelectionMode == DocSelectionMode.MultiList)
          return ShowDialogMultiList();
        else if (CanBeUsedGroupDialog())
          return ShowDialogForGroups();
        else
          return ShowDialogNormal();
      }
      else
        return ShowDialogFixedIds();
    }

    #endregion

    #region Обычный диалог с DocTableViewForm

    private DialogResult ShowDialogNormal()
    {
      DialogResult Res = DialogResult.Cancel;

      using (DocTableViewForm Form = new DocTableViewForm(DocTypeUI, GetDocTableViewMode(SelectionMode)))
      {
        Form.Text = Title;
        Form.CanBeEmpty = CanBeEmpty;
        try
        {
          if (MultiSelect)
            Form.SelectedDocIds = DocIds;
          else
            Form.CurrentDocId = DocId;
        }
        catch (Exception e)
        {
          if (MultiSelect)
            EFPApp.ShowException(e, "Не удалось установить выбранные документы \"" + DocTypeUI.DocType.PluralTitle + "\"");
          else
            EFPApp.ShowException(e, "Не удалось установить выбранный документ \"" + DocTypeUI.DocType.SingularTitle + "\" с DocId=" + DocId.ToString());
        }
        Form.ExternalFilters = Filters;
        Form.ExternalEditorCaller = EditorCaller;

        switch (EFPApp.ShowDialog(Form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              DocIds = Form.SelectedDocIds;
            else
              DocId = Form.CurrentDocId;
            Res = DialogResult.OK;
            break;
          case DialogResult.No:
            DocId = 0;
            Res = DialogResult.OK;
            break;
        }
      }
      return Res;
    }

    internal static DocTableViewMode GetDocTableViewMode(DocSelectionMode selectionMode)
    {
      switch (selectionMode)
      {
        case DocSelectionMode.Single: return DocTableViewMode.SelectSingle;
        case DocSelectionMode.MultiSelect:
        case DocSelectionMode.MultiList: return DocTableViewMode.SelectMulti;
        case DocSelectionMode.MultiCheckBoxes: return DocTableViewMode.SelectMultiWithFlags;
        default:
          throw new ArgumentException();
      }
    }

    #endregion

    #region Диалог с текстовым представлением доументов EFPDocSelTextGridView

    private DialogResult ShowDialogMultiList()
    {
      DialogResult Res = DialogResult.Cancel;
      using (OKCancelGridForm Form = new OKCancelGridForm())
      {
        Form.Text = Title;
        Form.FormProvider.ConfigSectionName = DocTypeUI.DocType.Name + "_MultiList";

        EFPDocSelTextGridView gh = new EFPDocSelTextGridView(Form.ControlWithToolBar, DocTypeUI);
        if (Filters != null)
          gh.Filters = Filters;
        gh.CommandItems.CanEditFilters = false; // 09.07.2019
        gh.ExternalEditorCaller = EditorCaller;
        gh.Ids = DocIds;

        gh.CanBeEmpty = CanBeEmpty;
        gh.OrderMode = EFPDocSelGridViewOrderMode.Manual;

        if (EFPApp.ShowDialog(Form, false, DialogPosition) == DialogResult.OK)
        {
          DocIds = gh.Ids;
          Res = DialogResult.OK;
        }
      }

      return Res;
    }

    #endregion

    #region Диалог выбора из фиксированного списка

    private DialogResult ShowDialogFixedIds()
    {
      DialogResult Res = DialogResult.Cancel;
      using (OKCancelGridForm Form = new OKCancelGridForm())
      {
        Form.Text = Title;
        Form.FormProvider.ConfigSectionName = DocTypeUI.DocType.Name + "_FixedIds";
        Form.NoButtonProvider.Visible = CanBeEmpty;

        EFPDocGridView gh = new EFPDocGridView(Form.ControlWithToolBar, DocTypeUI);
        gh.FixedDocIds = FixedDocIds;
        gh.Validating += new UIValidatingEventHandler(SelectSingleDoc_ValidateNotEmpty);
        gh.Control.MultiSelect = MultiSelect;
        gh.CommandItems.EnterAsOk = true;

        try
        {
          if (MultiSelect)
            gh.SelectedIds = DocIds;
          else
            gh.CurrentId = DocId;
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Не удалось установить выбранный документ \"" + DocTypeUI.DocType.SingularTitle + "\" с DocId=" + DocId.ToString());
        }

        switch (EFPApp.ShowDialog(Form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              DocIds = gh.SelectedIds;
            else
              DocId = gh.CurrentId;
            Res = DialogResult.OK;
            break;
          case DialogResult.No:
            DocId = 0;
            Res = DialogResult.OK;
            break;
        }
      }
      return Res;
    }

    private void SelectSingleDoc_ValidateNotEmpty(object sender, UIValidatingEventArgs args)
    {
      EFPDocGridView gh = (EFPDocGridView)sender;
      if (MultiSelect)
      {
        if (gh.SelectedRowCount == 0)
          args.SetError("Документы должны быть выбраны");
      }
      else
      {
        if (gh.CurrentId == 0)
          args.SetError("Документ должен быть выбран");
      }
    }

    #endregion

    #region Диалог выбора группы

    /// <summary>
    /// Возвращает true, если можно использовать диалог выбора группы
    /// </summary>
    /// <returns></returns>
    private bool CanBeUsedGroupDialog()
    {
      if (!(_DocTypeUI is GroupDocTypeUI))
        return false;
      if (SelectionMode != DocSelectionMode.MultiSelect)
        return false;
      if (DocIds.Length > 0)
        return false; // ?? может быть, можно выбрать группу
      if (_Filters != null)
      {
        if (_Filters.Count != 0)
          return false;
      }

      return true;
    }

    private DialogResult ShowDialogForGroups()
    {
      GroupDocTypeUI DocTypeUI2 = (GroupDocTypeUI)_DocTypeUI;

      if (!GroupGridFilterForm.PerformEdit(DocTypeUI2, Title, DocTypeUI2.ImageKey, ref DocTypeUI2.LastGroupId, ref DocTypeUI2.LastIncludeNestedGroups, CanBeEmpty, DialogPosition))
        return DialogResult.Cancel;
      IdList groupIds = DocTypeUI2.GetAuxFilterGroupIdList(DocTypeUI2.LastGroupId, DocTypeUI2.LastIncludeNestedGroups);
      if (groupIds != null)
        DocIds = groupIds.ToArray();
      else
        DocIds = DataTools.EmptyIds;

      return DialogResult.OK;
    }


    #endregion

    #endregion
  }
  /// <summary>
  /// Диалог выбора одного или нескольких поддокументов заданного вида
  /// </summary>
  public sealed class SubDocSelectDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог со значениями по умолчанию
    /// </summary>
    /// <param name="subDocTypeUI">Интерфейс для вида поддокумента. Ссылка должна быть задана</param>
    /// <param name="subDocs">Список поддокументов, из которых можно выбирать. Ссылка должна быть задана</param>
    public SubDocSelectDialog(SubDocTypeUI subDocTypeUI, DBxMultiSubDocs subDocs)
    {
      if (subDocTypeUI == null)
        throw new ArgumentNullException("subDocTypeUI");
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");

      _SubDocTypeUI = subDocTypeUI;
      _SubDocs = subDocs;
      _SelectionMode = DocSelectionMode.Single;
      _SubDocIds = DataTools.EmptyIds;
      _CanBeEmpty = false;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс для доступа к документам.
    /// Задается в конструкторе.
    /// Не может быть null
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// Список поддокументов, из которых можно выбирать.
    /// Задается в конструкторе.
    /// Не может быть null
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Режим выбора документа Single, MultiSelect или MultiCheckBoxes.
    /// Режим MultiList не поддерживается
    /// По умолчанию - Single.
    /// </summary>
    public DocSelectionMode SelectionMode
    {
      get { return _SelectionMode; }
      set
      {
        switch (value)
        {
          case DocSelectionMode.Single:
          case DocSelectionMode.MultiSelect:
          case DocSelectionMode.MultiCheckBoxes:
            _SelectionMode = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private DocSelectionMode _SelectionMode;

    /// <summary>
    /// Альтернативное значение для свойства SelectionMode.
    /// Содержит true, если разрешен выбор нескольких документов
    /// </summary>
    public bool MultiSelect
    {
      get { return _SelectionMode != DocSelectionMode.Single; }
      set { _SelectionMode = value ? DocSelectionMode.MultiSelect : DocSelectionMode.Single; }
    }

    /// <summary>
    /// Вход-выход. Идентификатор выбранного поддокумента при MultiSelect=false
    /// </summary>
    public Int32 SubDocId
    {
      get
      {
        if (_SubDocIds.Length == 0)
          return 0;
        else
          return _SubDocIds[0];
      }
      set
      {
        if (value == 0)
          _SubDocIds = DataTools.EmptyIds;
        else
          _SubDocIds = new Int32[1] { value };
      }
    }

    /// <summary>
    /// Вход-выход. Идентификаторы выбранных документов при MultiSelect=true.
    /// </summary>
    public Int32[] SubDocIds
    {
      get { return _SubDocIds; }
      set
      {
        if (value == null)
          _SubDocIds = DataTools.EmptyIds;
        else
          _SubDocIds = value;
      }
    }
    private Int32[] _SubDocIds;

    /// <summary>
    /// Заголовок блока диалога.
    /// По умолчанию: "Выбор поддокумента XXX" или "Выбор поддокументов XXX" при MultiSelect=true
    /// </summary>
    public string Title
    {
      get
      {
        if (_Title == null)
        {
          if (MultiSelect)
            return "Выбор поддокументов \"" + SubDocTypeUI.SubDocType.PluralTitle + "\"";
          else
            return "Выбор поддокумента \"" + SubDocTypeUI.SubDocType.SingularTitle + "\"";
        }
        else
          return _Title;
      }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// Определяет наличие в диалоге кнопки "Нет", чтобы можно было установить SubDocId=0.
    /// По умолчанию - false;
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    ///// <summary>
    ///// Набор фиксированных фильтров табличного просмотра.
    ///// Значение null (по умолчанию) приводит к использованию текущего набора
    ///// установленных пользователем фильтров.
    ///// Действует только при FixedDocIds=null.
    ///// </summary>
    //public GridFilters Filters { get { return _Filters; } set { _Filters = value; } }
    //private GridFilters _Filters;

    ///// <summary>
    ///// Массив фиксированных идентификаторов для выбора.
    ///// Если null (по умолчанию), то выбор осуществляется с помощью формы DocTableViewForm, включая табличку фильтров.
    ///// Если свойство установлено, то выбор осуществляется с помощью упрощенной формы
    ///// </summary>
    //public IdList FixedDocIds
    //{
    //  get { return _FixedDocIds; }
    //  set { _FixedDocIds = value; }
    //}
    //private IdList _FixedDocIds;

    /// <summary>
    /// Позиция вывода блока диалога.
    /// Может либо меняться существующий объект, либо быть передана ссылка на новый объкт EFPDialogPosition.
    /// Используется в комбоблоках
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region Вывод блока диалога

    /// <summary>
    /// Выводит блок диалога.
    /// Возвращает Ok, если пользователь сделал выбор, в том числе нажал кнопку "Нет"
    /// </summary>
    /// <returns></returns>
    public DialogResult ShowDialog()
    {
      DialogResult Res = DialogResult.Cancel;
      using (SubDocTableViewForm Form = new SubDocTableViewForm(SubDocTypeUI, DocSelectDialog.GetDocTableViewMode(SelectionMode), SubDocs))
      {
        Form.Text = Title;
        Form.CanBeEmpty = CanBeEmpty;
        try
        {
          if (MultiSelect)
            Form.SelectedSubDocIds = SubDocIds;
          else
            Form.CurrentSubDocId = SubDocId;
        }
        catch (Exception e)
        {
          if (MultiSelect)
            EFPApp.ShowException(e, "Не удалось установить выбранные поддокументы \"" + SubDocTypeUI.SubDocType.PluralTitle + "\"");
          else
            EFPApp.ShowException(e, "Не удалось установить выбранный поддокумент \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" с SubDocId=" + SubDocId.ToString());
        }
        //Form.ExternalFilters = Filters;

        switch (EFPApp.ShowDialog(Form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              SubDocIds = Form.SelectedSubDocIds;
            else
              SubDocId = Form.CurrentSubDocId;
            Res = DialogResult.OK;
            break;
          case DialogResult.No:
            SubDocId = 0;
            Res = DialogResult.OK;
            break;
        }
      }
      return Res;
    }

    #endregion
  }
}
