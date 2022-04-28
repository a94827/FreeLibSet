// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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

      //_DefaultGridConfigName = String.Empty;
      //_DefaultTreeConfigName = String.Empty;
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

    // Пока не уверен, что надо
    /*
    /// <summary>
    /// Имя фиксированной настройки табличного про
    /// </summary>
    public string DefaultGridConfigName { get { return _DefaultGridConfigName; } set { _DefaultGridConfigName = value; } }
    private string _DefaultGridConfigName;

    public string DefaultTreeConfigName { get { return _DefaultTreeConfigName; } set { _DefaultTreeConfigName = value; } }
    private string _DefaultTreeConfigName;

    public object UserInitData { get { return _UserInitData; } set { _UserInitData = value; } }
    private object _UserInitData;
     * */

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
      DialogResult res = DialogResult.Cancel;

      using (DocTableViewForm form = new DocTableViewForm(DocTypeUI, GetDocTableViewMode(SelectionMode)))
      {
        form.Text = Title;
        form.CanBeEmpty = CanBeEmpty;
        try
        {
          if (MultiSelect)
            form.SelectedDocIds = DocIds;
          else
            form.CurrentDocId = DocId;
        }
        catch (Exception e)
        {
          if (MultiSelect)
            EFPApp.ShowException(e, "Не удалось установить выбранные документы \"" + DocTypeUI.DocType.PluralTitle + "\"");
          else
            EFPApp.ShowException(e, "Не удалось установить выбранный документ \"" + DocTypeUI.DocType.SingularTitle + "\" с DocId=" + DocId.ToString());
        }
        form.ExternalFilters = Filters;
        form.ExternalEditorCaller = EditorCaller;
        /*
        if (Form.ViewProvider.DocGridView != null)
        {
          Form.ViewProvider.DocGridView.DefaultConfigName = DefaultGridConfigName;
          Form.ViewProvider.DocGridView.UserInitData = UserInitData;
        }
        if (Form.ViewProvider.DocTreeView != null)
        {
          Form.ViewProvider.DocTreeView.DefaultConfigName = DefaultTreeConfigName;
          Form.ViewProvider.DocTreeView.UserInitData = UserInitData;
        }
          */
        switch (EFPApp.ShowDialog(form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              DocIds = form.SelectedDocIds;
            else
              DocId = form.CurrentDocId;
            res = DialogResult.OK;
            break;
          case DialogResult.No:
            DocId = 0;
            res = DialogResult.OK;
            break;
        }
      }
      return res;
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

    #region Диалог с текстовым представлением документов EFPDocSelTextGridView

    private DialogResult ShowDialogMultiList()
    {
      DialogResult res = DialogResult.Cancel;
      using (OKCancelGridForm form = new OKCancelGridForm())
      {
        form.Text = Title;
        form.FormProvider.ConfigSectionName = DocTypeUI.DocType.Name + "_MultiList";

        EFPDocSelTextGridView gh = new EFPDocSelTextGridView(form.ControlWithToolBar, DocTypeUI);
        if (Filters != null)
          gh.Filters = Filters;
        gh.CommandItems.CanEditFilters = false; // 09.07.2019
        gh.ExternalEditorCaller = EditorCaller;
        gh.Ids = DocIds;

        gh.CanBeEmpty = CanBeEmpty;
        gh.OrderMode = EFPDocSelGridViewOrderMode.Manual;

        if (EFPApp.ShowDialog(form, false, DialogPosition) == DialogResult.OK)
        {
          DocIds = gh.Ids;
          res = DialogResult.OK;
        }
      }

      return res;
    }

    #endregion

    #region Диалог выбора из фиксированного списка

    private DialogResult ShowDialogFixedIds()
    {
      DialogResult res = DialogResult.Cancel;
      using (OKCancelGridForm form = new OKCancelGridForm())
      {
        form.Text = Title;
        form.FormProvider.ConfigSectionName = DocTypeUI.DocType.Name + "_FixedIds";
        form.NoButtonProvider.Visible = CanBeEmpty;

        EFPDocGridView gh = new EFPDocGridView(form.ControlWithToolBar, DocTypeUI);
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

        switch (EFPApp.ShowDialog(form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              DocIds = gh.SelectedIds;
            else
              DocId = gh.CurrentId;
            res = DialogResult.OK;
            break;
          case DialogResult.No:
            DocId = 0;
            res = DialogResult.OK;
            break;
        }
      }
      return res;
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
      GroupDocTypeUI docTypeUI2 = (GroupDocTypeUI)_DocTypeUI;

      if (!GroupGridFilterForm.PerformEdit(docTypeUI2, Title, docTypeUI2.ImageKey, ref docTypeUI2.LastGroupId, ref docTypeUI2.LastIncludeNestedGroups, CanBeEmpty, DialogPosition))
        return DialogResult.Cancel;
      IdList groupIds = docTypeUI2.GetAuxFilterGroupIdList(docTypeUI2.LastGroupId, docTypeUI2.LastIncludeNestedGroups);
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
      //if (!Object.ReferenceEquals(subDocTypeUI.SubDocType, subDocs.SubDocType))
      //  throw new ArgumentException("SubDocTypeUI и SubDocs относятся к разным объектам SubDocType", "subDocs");

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
      DialogResult res = DialogResult.Cancel;
      using (SubDocTableViewForm form = new SubDocTableViewForm(SubDocTypeUI, DocSelectDialog.GetDocTableViewMode(SelectionMode), SubDocs))
      {
        form.Text = Title;
        form.CanBeEmpty = CanBeEmpty;
        try
        {
          if (MultiSelect)
            form.SelectedSubDocIds = SubDocIds;
          else
            form.CurrentSubDocId = SubDocId;
        }
        catch (Exception e)
        {
          if (MultiSelect)
            EFPApp.ShowException(e, "Не удалось установить выбранные поддокументы \"" + SubDocTypeUI.SubDocType.PluralTitle + "\"");
          else
            EFPApp.ShowException(e, "Не удалось установить выбранный поддокумент \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" с SubDocId=" + SubDocId.ToString());
        }
        //Form.ExternalFilters = Filters;

        switch (EFPApp.ShowDialog(form, false, DialogPosition))
        {
          case DialogResult.OK:
            if (MultiSelect)
              SubDocIds = form.SelectedSubDocIds;
            else
              SubDocId = form.CurrentSubDocId;
            res = DialogResult.OK;
            break;
          case DialogResult.No:
            SubDocId = 0;
            res = DialogResult.OK;
            break;
        }
      }
      return res;
    }

    #endregion
  }
}
