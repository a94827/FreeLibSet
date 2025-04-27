// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Редактор фильтра табличного просмотра по документам
  /// </summary>
  internal partial class RefDocGridFilterForm : Form
  {
    #region Конструктор

    internal RefDocGridFilterForm(DocTypeUI docTypeUI, bool nullable)
    {
      InitializeComponent();

      _DocTypeUI = docTypeUI;
      _Nullable = nullable;

      EFPFormProvider efpForm = new EFPFormProvider(this);

      List<string> modeNames = new List<string>();
      modeNames.Add(Res.RefDocFilterMode_Msg_NoFilter);
      if (String.IsNullOrEmpty(docTypeUI.DocType.TreeParentColumnName))
      {
        modeNames.Add(Res.RefDocFilterMode_Msg_Include);
        modeNames.Add(Res.RefDocFilterMode_Msg_Exclude);
      }
      else
      {
        modeNames.Add(Res.RefDocFilterMode_Msg_IncludeNested);
        modeNames.Add(Res.RefDocFilterMode_Msg_ExcludeNested);
      }
      if (nullable)
      {
        modeNames.Add(Res.RefDocFilterMode_Msg_NotNull);
        modeNames.Add(Res.RefDocFilterMode_Msg_Null);
      }
      cbMode.Items.AddRange(modeNames.ToArray());
      if (EFPApp.ShowListImages)
        new ListControlImagePainter(cbMode, new ListControlImageEventHandler(PaintModeItem));
      efpMode = new EFPListComboBox(efpForm, cbMode);

      efpDocSel = new EFPDocSelTextGridView(efpForm, grDocSel, docTypeUI);
      efpDocSel.OrderMode = EFPDocSelGridViewOrderMode.Natural;
      efpDocSel.ToolBarPanel = panSpeedButtons;
      efpDocSel.CanBeEmpty = false;
      efpDocSel.Label = grpDocs; // Иначе не найдет из-за таблички фильтров FilterGrid

      EFPGridFilterGridView filterView = new EFPGridFilterGridView(efpDocSel, FilterGrid);

      ActiveControl = grDocSel;

      efpMode.SelectedIndexEx.ValueChanged += efpMode_ValueChanged;
      efpMode_ValueChanged(null, null);
      efpForm.ConfigSectionName = "RefDocGridFilterForm";
    }

    void efpMode_ValueChanged(object sender, EventArgs args)
    {
      bool useDocs;
      switch (Mode)
      {
        case RefDocFilterMode.Include:
          grpDocs.Text = String.Format(Res.RefDocGridFilter_Msg_RefsToIncluded, DocTypeUI.DocType.PluralTitle);
          useDocs = true;
          break;
        case RefDocFilterMode.Exclude:
          grpDocs.Text = String.Format(Res.RefDocGridFilter_Msg_RefsToExcluded, DocTypeUI.DocType.PluralTitle);
          useDocs = true;
          break;
        default:
          useDocs = false;
          break;
      }

      grpDocs.Visible = useDocs;
      efpDocSel.Enabled = useDocs; // чтобы проверка не срабатывала
    }

    private static void PaintModeItem(object sender, ListControlImageEventArgs args)
    {
      if (args.ItemIndex == 0)
        args.ImageKey = EFPGridFilterTools.NoFilterImageKey;
      else
        args.ImageKey = EFPGridFilterTools.DefaultFilterImageKey;
    }

    #endregion

    #region Поля

    /// <summary>
    /// Режим фильтра
    /// </summary>
    public EFPListComboBox efpMode;

    public RefDocFilterMode Mode { get { return (RefDocFilterMode)(efpMode.SelectedIndex); } }

    public EFPDocSelTextGridView efpDocSel;

    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Наличие в списке режимо efpMode вариантов "Ссылка на любой документ" и "Ссылка не задана".
    /// </summary>
    public bool Nullable { get { return _Nullable; } }
    private bool _Nullable;

    #endregion

    #region Статический метод редактирования фильтра

    //public static bool PerformEdit(string title, DocTypeUI docTypeUI, bool nullable, ref RefDocFilterMode mode, ref Int32[] ids)
    //{
    //  return PerformEdit(title, docTypeUI, nullable, ref mode, ref ids, null, null);
    //}

    public static bool PerformEdit(string title, DocTypeUI docTypeUI, bool nullable,
      ref RefDocFilterMode mode, ref Int32[] ids,
      EFPDBxGridFilters docFilters,
      EFPDialogPosition dialogPosition, MultiSelectEmptyEditMode emptyEditMode)
    {
#if DEBUG
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
#endif

      #region Для пустого фильтра - сразу выбор документов

      if (mode == RefDocFilterMode.NoFilter && emptyEditMode != MultiSelectEmptyEditMode.EmptyList)
      {
        DocSelectDialog dlg1 = new DocSelectDialog(docTypeUI);
        //dlg.Title = "Добавление документов \"" + docTypeUI.DocType.PluralTitle + "\" в фильтр \""+title+"\"";
        dlg1.Title = title;
        dlg1.SelectionMode = DocSelectionMode.MultiSelect;
        dlg1.CanBeEmpty = false;
        dlg1.DialogPosition = dialogPosition;
        dlg1.Filters = docFilters; // 10.09.2021
        if (dlg1.ShowDialog() == DialogResult.OK)
        {
          mode = RefDocFilterMode.Include;
          ids = dlg1.DocIds;
          if (emptyEditMode == MultiSelectEmptyEditMode.Select)
            return true;
        }
      }

      #endregion

      #region Показ основной формы

      bool res = false;
      RefDocGridFilterForm dlg2 = new RefDocGridFilterForm(docTypeUI, nullable);
      try
      {
        dlg2.Text = title;
        dlg2.Icon = EFPApp.MainImages.Icons["Filter"];
        if ((int)mode >= 0 && (int)mode < dlg2.cbMode.Items.Count)
          dlg2.efpMode.SelectedIndex = (int)(mode);
        dlg2.efpDocSel.Ids = ids;
        dlg2.efpDocSel.Filters = docFilters;
        dlg2.efpDocSel.CommandItems.CanEditFilters = false; // 09.07.2019

        switch (EFPApp.ShowDialog(dlg2, false, dialogPosition))
        {
          case DialogResult.OK:
            mode = dlg2.Mode;
            ids = dlg2.efpDocSel.Ids;
            res = true;
            break;
          case DialogResult.No:
            mode = RefDocFilterMode.NoFilter;
            ids = null;
            //Values = null;
            res = true;
            break;
        }
      }
      finally
      {
        dlg2.Dispose();
      }
      return res;

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Режим редактирования ссылочного фильтра, когда он пустой.
  /// Свойство <see cref="RefDocGridFilter.EmptyEditMode"/>.
  /// </summary>
  public enum MultiSelectEmptyEditMode
  {
    /// <summary>
    /// Показывается диалог выбора документов. Если пользователь нажимает "ОК", то фильтр устанавливается в режим <see cref="RefDocCommonFilter.Mode"/>=<see cref="RefDocFilterMode.Include"/>. 
    /// Если пользователь нажимает "Отмена", то показывается обычный диалог с возможностью выбора режима.
    /// </summary>
    Select,

    /// <summary>
    /// Показывается обычный диалог выбора с пустым списком выбранных документов
    /// </summary>
    EmptyList,

    /// <summary>
    /// Комбинированный режим. Сначала показывается диалог выбора документов. Затем показывается обычный диалог выбора фильтра.
    /// Если в первом диалоге была нажата кнопка "ОК", то устанавливается режим "Выбранные документы", иначе "Нет фильтра".
    /// </summary>
    SelectThenShowList,
  }

  #region Интерфейс IDBxDocSelFilter

  /// <summary>
  /// Интерфейс фильтра, поддерживающего работу с выборками документов <see cref="DBxDocSelection"/> 
  /// </summary>
  public interface IDBxDocSelectionFilter
  {
    /// <summary>
    /// Добавить документы, соответствующие фильтру, в выборку
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    void GetDocSel(DBxDocSelection docSel);

    /// <summary>
    /// Заменить документы в фильтре документами в выборке.
    /// Если выборка не содержит подходящих значений, то фильтр не изменяется и
    /// возвращается false.
    /// </summary>
    /// <param name="docSel">Выборка из буфера обмена</param>
    /// <returns>trye, если удалось что-нибудь использовать</returns>
    bool ApplyDocSel(DBxDocSelection docSel);
  }

  #endregion


  /// <summary>
  /// Фильтр по значению ссылочного поля на документ.
  /// Возможен фильтр по нескольким идентификаторам и режим "Исключить".
  /// Для полей, поддерживающих пустое значение (<see cref="DBxColumnStruct.Nullable"/>=true), возможны фильтры на NULL и NOT NULL.
  /// </summary>
  public class RefDocGridFilter : RefDocCommonFilter, IEFPGridFilterWithImageKey, IDBxDocSelectionFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docTypeUI">Обработчики интерфейса для вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocGridFilter(DocTypeUI docTypeUI, string columnName)
      : base(docTypeUI.UI.DocProvider, docTypeUI.DocType, columnName)
    {
      _UI = docTypeUI.UI;
      _DocFilters = new EFPDBxGridFilters();
      _DocFilters.SqlFilterRequired = true;
      _Nullable = false;
      _EmptyEditMode = _UI.DefaultEmptyEditMode;
    }

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="ui">Пользовательский интерфейс для базы данных</param>
    /// <param name="docTypeName">Имя вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocGridFilter(DBUI ui, string docTypeName, string columnName)
      : this(ui.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Пользовательский интерфейс для базы данных.
    /// Задается в конструкторе.
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Пользовательский интерфейс вида документов.
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _UI.DocTypes[DocTypeName]; } }

    /// <summary>
    /// Фильтр на документы, из которых пользователь может делать выбор.
    /// По умолчанию список фильтров пустой.
    /// Пользователь не имеет возможности изменить эти фильтры.
    /// </summary>
    public EFPDBxGridFilters DocFilters { get { return _DocFilters; } }
    private readonly EFPDBxGridFilters _DocFilters;

    /// <summary>
    /// Определяет возможность задания фильтров NotNull и Null.
    /// По умолчанию - false - режимы недоступны.
    /// Так как есть только имя ссылочного поля, а не описание этого поля, свойство следует устанавливать в true, если ссылочное поле поддерживает значение NULL.
    /// </summary>
    public bool Nullable { get { return _Nullable; } set { _Nullable = value; } }
    private bool _Nullable;

    /// <summary>
    /// Режим редактирования фильтра, когда <see cref="RefDocCommonFilter.Mode"/>=<see cref="RefDocFilterMode.NoFilter"/>.
    /// Инициализируется в конструкторе значением <see cref="DBUI.DefaultEmptyEditMode"/> (обычно это значение <see cref="MultiSelectEmptyEditMode.Select"/>).
    /// Если на момент вызова <see cref="ShowFilterDialog(EFPDialogPosition)"/> фильтр непустой, то свойство игнорируется.
    /// </summary>
    public MultiSelectEmptyEditMode EmptyEditMode { get { return _EmptyEditMode; } set { _EmptyEditMode = value; } }
    private MultiSelectEmptyEditMode _EmptyEditMode;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Вызов события <see cref="DBxCommonFilter.Changed"/> и уведомление объекта-владельца об изменении фильтра
    /// </summary>
    protected override void OnChanged()
    {
      _FilterText = null;
      _FilterImageKey = null;
      base.OnChanged();
    }

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (_FilterText == null)
          InitFilterText();
        return _FilterText;
      }
    }
    private string _FilterText;

    private void InitFilterText()
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          _FilterText = String.Empty;
          break;
        case RefDocFilterMode.Include:
        case RefDocFilterMode.Exclude:
          if (DocIds.Count == 0)
          {
            if (Mode == RefDocFilterMode.Include)
              _FilterText = String.Format(Res.RefDocGridFilter_Msg_TextIncludeNoDocs, DocTypeUI.DocType.PluralTitle); // 24.07.2019
            else
              _FilterText = Res.RefDocGridFilter_Msg_TextExcludeNoDocs; // 24.07.2019
          }
          else
          {
            List<string> lst = new List<string>(SelectedDocIds.Count);
            foreach (Int32 id in SelectedDocIds)
            {
              string s = UI.TextHandlers.GetTextValue(DocTypeName, id);
              if (UI.DebugShowIds)
                s += " (Id=" + id.ToString() + ")";
              lst.Add(s);
            }
            string sDocs = String.Join(", ", lst.ToArray());
            if (Mode == RefDocFilterMode.Include)
              _FilterText = sDocs;
            else
              _FilterText = String.Format(Res.RefDocGridFilter_Msg_TextExcludeDocs, sDocs);
          }
          break;
        case RefDocFilterMode.NotNull:
          _FilterText = String.Format(Res.RefDocFilterMode_Msg_TextNotNull, DocTypeUI.DocType.SingularTitle);
          break;
        case RefDocFilterMode.Null:
          _FilterText = Res.RefDocFilterMode_Msg_TextNull;
          break;
        default:
          throw new BugException("Unknown mode " + Mode.ToString());
      }
    }


    /// <summary>
    /// Значок фильтра.
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        if (_FilterImageKey == null)
          InitFilterImageKey();
        return _FilterImageKey;
      }
    }
    private string _FilterImageKey;

    private void InitFilterImageKey()
    {
      _FilterImageKey = GetFilterImageKey(Mode);
      switch (Mode)
      {
        case RefDocFilterMode.Include:
          if (DocIds.Count == 1)
            _FilterImageKey = DocTypeUI.GetImageKey(DocIds.SingleId);
          break;
      }
    }

    /// <summary>
    /// Возвращает имя значка изображения для фильтра.
    /// Может использоваться в прикладном коде, если диалог установки фильтра реализуется самостоятельно.
    /// Для режима <see cref="RefDocFilterMode.Include"/> обычно следует использовать значок вида документа.
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <returns>Имя значка в <see cref="EFPApp.MainImages"/></returns>
    public static string GetFilterImageKey(RefDocFilterMode mode)
    {
      switch (mode)
      {
        case RefDocFilterMode.NoFilter: return EFPGridFilterTools.NoFilterImageKey;
        case RefDocFilterMode.NotNull: return "Item";
        case RefDocFilterMode.Null: return "Delete";
        default: return EFPGridFilterTools.DefaultFilterImageKey;
      }
    }

    /// <summary>
    /// Выводит блок диалога установки фильтра
    /// </summary>
    /// <returns>true, если фильтр установлен</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      RefDocFilterMode mode = base.Mode;
      Int32[] ids = null;
      if (base.SelectedDocIds != null)
        ids = base.SelectedDocIds.ToArray();

      bool res = RefDocGridFilterForm.PerformEdit(DisplayName, UI.DocTypes[DocTypeName], Nullable, ref mode, ref ids, DocFilters, dialogPosition, EmptyEditMode);
      if (res)
        SetFilter(mode, ids);
      return res;
    }

    //    public override bool CanAsCurrRow(DataRow Row)
    //    {
    //      Int32 ThisId = DataTools.GetInt(Row, ColumnName);
    //      if (ThisId == 0 || ThisId == SingleDocId)
    //        return false;
    //      return true;
    //    }

    //    public override void SetAsCurrRow(DataRow Row)
    //    {
    //      Int32 ThisId = DataTools.GetInt(Row, ColumnName);
    //      SingleDocId = ThisId;
    //    }

    /// <summary>
    /// Вызывает <see cref="DBxDocTextHandlers.GetTextValue(string, int)"/> для получения текстового представления
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      return new string[] { UI.TextHandlers.GetTextValue(DocTypeName, DataTools.GetInt(columnValues[0])) };
    }

    #endregion

    #region IDBxDocSelFilter Members

    /// <summary>
    /// Добавляет в выборку ссылки на выбранные документы.
    /// Действует и в режимах <see cref="RefDocCommonFilter.Mode"/>=<see cref="RefDocFilterMode.Include"/> и <see cref="RefDocFilterMode.Exclude"/>.
    /// Использует вызов <see cref="FreeLibSet.Forms.Docs.DocTypeUI.PerformGetDocSel(DBxDocSelection, int[], EFPDBxViewDocSelReason)"/> в режиме <see cref="EFPDBxViewDocSelReason.Copy"/>.
    /// В выборку могут быть добавлены ссылки на связанные документы, если есть обработчик события
    /// <see cref="DocTypeUIBase.GetDocSel"/>
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    public void GetDocSel(DBxDocSelection docSel)
    {
      if (Mode != RefDocFilterMode.NoFilter)
        DocTypeUI.PerformGetDocSel(docSel, DocIds, EFPDBxViewDocSelReason.Copy);
    }

    /// <summary>
    /// Устанавливает фильтр, если в выборке <paramref name="docSel"/> есть документы подходящего вида.
    /// Если на момент вызова фильтр не установлен или находится в режиме, отличном от <see cref="RefDocFilterMode.Exclude"/>, то
    /// устанавливается режим Mode=Include.
    /// Если же на момент вызова фильтр находится в режиме <see cref="RefDocFilterMode.Exclude"/>, то этот режим сохраняется.
    /// Если выборка не содержит документов нужного вида, никаких действий не выполняется.
    /// </summary>
    /// <param name="docSel">Выборка документов, откуда берутся ссылки на документ</param>
    /// <returns>True, если была выполнена установка фильтра.
    /// False, если в выборке нет ссылок на документы подходящего вида</returns>
    public bool ApplyDocSel(DBxDocSelection docSel)
    {
      Int32[] newIds = docSel[DocType.Name];
      if (newIds.Length == 0)
        return false;
      RefDocFilterMode newMode;
      //if (Mode == RefDocFilterMode.NoFilter)
      //  newMode = RefDocFilterMode.Include;
      //else
      //  newMode = Mode;

      // 17.04.2024
      if (Mode == RefDocFilterMode.Exclude)
        newMode = RefDocFilterMode.Exclude;
      else
        newMode = RefDocFilterMode.Include;

      SetFilter(newMode, newIds);
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Набор из фильтра по группе и фильтра по документам
  /// </summary>
  public class RefDocGridFilterSet : EFPDBxGridFilterSet
  {
    #region Конструкторы

    /// <summary>
    /// Создает набор из одного или двух фильтров
    /// </summary>
    /// <param name="docTypeUI">Обработчики интерфейса для вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocGridFilterSet(DocTypeUI docTypeUI, string columnName)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      if (docTypeUI.GroupDocType != null)
      {
        _GroupFilter = new RefDocGridFilter(docTypeUI.GroupDocType, columnName + "." + docTypeUI.DocType.GroupRefColumnName);
        // FGroupFilter.DisplayName = ColumnName + " (группа)";
        _GroupFilter.DisplayName = _GroupFilter.DocType.SingularTitle + " - " + columnName; // 23.09.2019
        _GroupFilter.Nullable = docTypeUI.DocType.Struct.Columns[docTypeUI.DocType.GroupRefColumnName].Nullable;
        Add(_GroupFilter);
      }

      _DocFilter = new RefDocGridFilter(docTypeUI, columnName);
      Add(_DocFilter);
    }

    /// <summary>
    /// Создает набор из одного или двух фильтров
    /// </summary>
    /// <param name="ui">Пользовательский интерфейс для базы данных</param>
    /// <param name="docTypeName">Имя вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocGridFilterSet(DBUI ui, string docTypeName, string columnName)
      : this(ui.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Фильтр по группе документов.
    /// Если вид документов, на которое ссылается поле, не использует группы, свойство возвращает null.
    /// </summary>
    public RefDocGridFilter GroupFilter { get { return _GroupFilter; } }
    private readonly RefDocGridFilter _GroupFilter;

    /// <summary>
    /// Основной фильтр
    /// </summary>
    public RefDocGridFilter DocFilter { get { return _DocFilter; } }
    private readonly RefDocGridFilter _DocFilter;

    /// <summary>
    /// Дублирует свойство <see cref="DocFilter"/>.DisplayName.
    /// При установке синхронно устанавливается свойство <see cref="GroupFilter"/>.DisplayName
    /// </summary>
    public string DisplayName
    {
      get { return DocFilter.DisplayName; }
      set
      {
        DocFilter.DisplayName = value;
        if (GroupFilter != null)
          //GroupFilter.DisplayName = DocFilter.DisplayName + " (группа)";
          GroupFilter.DisplayName = GroupFilter.DocType.SingularTitle + " - " + DocFilter.DisplayName; // 23.09.2019
      }
    }

    /// <summary>
    /// Дублирует свойство <see cref="DocFilter"/>.Nullable.
    /// <see cref="GroupFilter"/>.Nullable не используется и обычно равно true.
    /// </summary>
    public bool Nullable
    {
      get { return DocFilter.Nullable; }
      set { DocFilter.Nullable = value; }
    }

    /// <summary>
    /// Установка единственного выбранного документа.
    /// Дублирует свойство <see cref="DocFilter"/>.SingleDocId.
    /// Если есть <see cref="GroupFilter"/>, то он очищается.
    /// </summary>
    public Int32 SingleDocId
    {
      get
      {
        if (GroupFilter != null)
        {
          if (!GroupFilter.IsEmpty)
            return 0;
        }
        return DocFilter.SingleDocId;
      }
      set
      {
        if (GroupFilter != null)
          GroupFilter.Clear();
        DocFilter.SingleDocId = value;
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по виду документа.
  /// Текущим значением числового поля является идентификатор таблицы документа <see cref="DBxDocType.TableId"/>.
  /// </summary>
  public class DocTableIdGridFilter : DocTableIdCommonFilter, IEFPGridFilterWithImageKey
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="ui">Интерфейс пользователя для доступа к документам</param>
    /// <param name="columnName">Столбец типа <see cref="Int32"/>, хранящий идентификатор вида документа из таблицы "DocTables"</param>
    public DocTableIdGridFilter(DBUI ui, string columnName)
      : base(columnName)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");

      _UI = ui;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс пользователя для доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Список типов документов, из которых осуществляется выбор.
    /// Если свойство не было установлено явно, то можно выбрать любой существующий тип документа.
    /// Установка свойства никак не влияет на текущее значение.
    /// </summary>
    public DocTypeUI[] DocTypeUIs
    {
      get
      {
        if (_DocTypeUIs == null)
        {
          string[] allNames = UI.DocProvider.DocTypes.GetDocTypeNames();
          _DocTypeUIs = new DocTypeUI[allNames.Length];
          for (int i = 0; i < allNames.Length; i++)
            _DocTypeUIs[i] = UI.DocTypes[allNames[i]];
        }
        return _DocTypeUIs;
      }
      set
      {
        if (value != null)
        {
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] == null)
              throw ExceptionFactory.ArgInvalidEnumerableItem("value", value, null);
            if (value[i].UI != _UI)
              throw new ArgumentException(String.Format(Res.DocTableIdGridFilter_Arg_AlienDBUI, i));
          }
        }
        _DocTypeUIs = value;
      }
    }
    private DocTypeUI[] _DocTypeUIs;

    /// <summary>
    /// Список видов типов документов, из которых осуществляется выбор.
    /// Если свойство не было установлено явно, то можно выбрать любой существующий тип документа.
    /// Установка свойства никак не влияет на текущее значение.
    /// </summary>
    public string[] DocTypeNames
    {
      get
      {
        string[] a = new string[DocTypeUIs.Length];
        for (int i = 0; i < DocTypeUIs.Length; i++)
          a[i] = DocTypeUIs[i].DocType.Name;
        return a;
      }
      set
      {
        if (value == null)
          DocTypeUIs = null;
        else
        {
          DocTypeUI[] a = new DocTypeUI[value.Length];
          for (int i = 0; i < value.Length; i++)
          {
            a[i] = UI.DocTypes[value[i]];
            // не требуется, т.к. проверяется в предыдущей строке
            //if (a[i] == null)
            //  throw new ArgumentException("Неизвестный вид документа \"" + value[i] + "\"");
          }
          DocTypeUIs = a;
        }
      }
    }

    #endregion

    #region Текущий выбор - расширенные свойства

    /// <summary>
    /// Интерфейс текущего выбранного вида документов
    /// </summary>
    public DocTypeUI CurrentDocTypeUI
    {
      get
      {
        if (CurrentTableId == 0)
          return null;
        else
          return UI.DocTypes.FindByTableId(CurrentTableId);
      }
      set
      {
        if (value == null)
          CurrentTableId = 0;
        else
          CurrentTableId = value.DocType.TableId;
      }
    }

    /// <summary>
    /// Имя вида документа.
    /// Альтернативное свойство для установки фильтра.
    /// </summary>
    public string CurrentDocTypeName
    {
      get
      {
        if (CurrentTableId == 0)
          return String.Empty;
        else
          return CurrentDocTypeUI.DocType.Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          CurrentTableId = 0;
        else
          CurrentDocTypeUI = UI.DocTypes[value];
      }
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public string FilterText
    {
      get
      {
        if (CurrentTableId == 0)
          return string.Empty;
        else
        {
          if (CurrentDocTypeUI == null)
            return String.Format(Res.DocTableIdGridFilter_Msg_TextUnknownTableId, CurrentTableId);
          else
          {
            string s = CurrentDocTypeUI.DocType.PluralTitle;
            if (UI.DebugShowIds)
              s += " (TableId=" + CurrentTableId.ToString() + ")";
            return s;
          }
        }
      }
    }

    /// <summary>
    /// Возвращает значок вида документов
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        if (CurrentDocTypeUI != null)
          return CurrentDocTypeUI.ImageKey;
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.Items = new string[DocTypeUIs.Length + 1];
      dlg.Items[0] = Res.DocTableIdGridFilter_Msg_NoFilter;
      dlg.ImageKeys[0] = EFPGridFilterTools.NoFilterImageKey;
      if (CurrentTableId == 0)
        dlg.SelectedIndex = 0;
      for (int i = 0; i < DocTypeUIs.Length; i++)
      {
        dlg.Items[i + 1] = DocTypeUIs[i].DocType.PluralTitle;
        if (UI.DebugShowIds)
          dlg.Items[i + 1] += " (TableId=" + DocTypeUIs[i].DocType.TableId.ToString() + ")";
        dlg.ImageKeys[i + 1] = DocTypeUIs[i].ImageKey;
        if (CurrentTableId == DocTypeUIs[i].DocType.TableId)
          dlg.SelectedIndex = i + 1;
      }
      dlg.ConfigSectionName = "GridFilter." + this.Code;
      dlg.DialogPosition = dialogPosition;
      if (dlg.ShowDialog() != DialogResult.OK)
        return false;
      if (dlg.SelectedIndex == 0)
        CurrentTableId = 0;
      else
        CurrentTableId = DocTypeUIs[dlg.SelectedIndex - 1].DocType.TableId; // 22.12.2020
      return true;
    }

    /// <summary>
    /// Возвращает свойство <see cref="DBxDocTypeBase.PluralTitle"/>, если задан фильтр по виду документов.
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      string s;
      Int32 thisTableId = DataTools.GetInt(columnValues[0]);
      if (thisTableId == 0)
        s = "Нет";
      else
      {
        DBxDocType docType = UI.DocProvider.DocTypes.FindByTableId(thisTableId);
        if (docType == null)
          s = String.Format(Res.DocTableIdGridFilter_Msg_TextUnknownTableId, thisTableId);
        else
        {
          s = docType.PluralTitle;
          if (UI.DebugShowIds)
            s += " (TableId=" + docType.TableId.ToString() + ")";
        }
      }
      return new string[] { s };
    }

    #endregion
  }

}
