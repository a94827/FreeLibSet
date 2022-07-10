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
      modeNames.Add("Нет фильтра");
      modeNames.Add("Выбранные документы");
      modeNames.Add("Кроме выбранных документов");
      if (nullable)
      {
        modeNames.Add("Ссылка на любой документ");
        modeNames.Add("Ссылка не задана");
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
          grpDocs.Text = "Ссылки на документы \"" + DocTypeUI.DocType.PluralTitle + "\"";
          useDocs = true;
          break;
        case RefDocFilterMode.Exclude:
          grpDocs.Text = "Ссылки на документы  \"" + DocTypeUI.DocType.PluralTitle + "\", которые надо исключить";
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

    public static bool PerformEdit(string title, DocTypeUI docTypeUI, bool nullable, ref RefDocFilterMode mode, ref Int32[] ids, GridFilters docFilters, EFPDialogPosition dialogPosition, RefDocGridFilterEmptyEditMode emptyEditMode)
    {
#if DEBUG
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
#endif

      #region Для пустого фильтра - сразу выбор документов

      if (mode == RefDocFilterMode.NoFilter && emptyEditMode != RefDocGridFilterEmptyEditMode.ShowFilterDialog)
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
          if (emptyEditMode == RefDocGridFilterEmptyEditMode.SelectIncudedDocs)
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
  /// Свойство RefDocGridFilter.EmptyEditMode
  /// </summary>
  public enum RefDocGridFilterEmptyEditMode
  {
    /// <summary>
    /// Показывается диалог выбора документов. Если пользователь нажимает "ОК", то фильтр устанавливается в режим Mode=RefDocFilterMode.Include. 
    /// Если пользователь нажимает "Отмена", то показывается обычный диалог с возможностью выбора режима.
    /// </summary>
    SelectIncudedDocs,

    /// <summary>
    /// Показывается обычный диалог фильтра для выбора режима и с таблицей выбранных документов
    /// </summary>
    ShowFilterDialog,

    /// <summary>
    /// Комбинированный режим. Сначала показывается диалог выбора документов. Затем показывается обычный диалог выбора фильтра.
    /// Если в первом диалоге была нажата кнопка "ОК", то устанавливается режим "Выбранные документы", иначе "Нет фильтра".
    /// </summary>
    SelectDocsThenShowFilterDialog,
  }

  /// <summary>
  /// Фильтр по значению ссылочного поля на документ
  /// Возможен фильтр по нескольким идентификаторам и режим "Исключить"
  /// Фильтр по пустому значению поля невозможен
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
      _DocFilters = new GridFilters();
      _Nullable = false;
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
    private DBUI _UI;

    /// <summary>
    /// Пользовательский интерфейс вида документов.
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _UI.DocTypes[DocTypeName]; } }

    /// <summary>
    /// Фильтр на документы, из которых пользователь может делать выбор.
    /// По умолчанию список фильтров пустой.
    /// Пользователь не имеет возможности изменить эти фильтры.
    /// </summary>
    public GridFilters DocFilters { get { return _DocFilters; } }
    private GridFilters _DocFilters;

    /// <summary>
    /// Определяет возможность задания фильтров NotNull и Null.
    /// По умолчанию - false - режимы недоступны.
    /// Так как есть только имя ссылочного поля, а не описание этого поля, свойство следует устанавливать в true, если ссылочное поле поддерживает значение NULL.
    /// </summary>
    public bool Nullable { get { return _Nullable; } set { _Nullable = value; } }
    private bool _Nullable;

    #endregion

    #region Статические свойства

    /// <summary>
    /// Режим редактирования фильтра, когда Mode=NoFilter.
    /// По умолчанию - SelectIncludedDocs.
    /// Если на момент вызова ShowFilterDialog() фильтр непустой, то свойство игнорируется.
    /// </summary>
    public static RefDocGridFilterEmptyEditMode EmptyEditMode { get { return _EmptyEditMode; } set { _EmptyEditMode = value; } }
    private static RefDocGridFilterEmptyEditMode _EmptyEditMode = RefDocGridFilterEmptyEditMode.SelectIncudedDocs;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Вызов события Changed и уведомление объекта-владельца об изменении фильтра
    /// </summary>
    protected override void OnChanged()
    {
      _FilterText = null;
      _FilterImageKey = null;
      base.OnChanged();
    }

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
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
      StringBuilder sb = new StringBuilder();
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          _FilterText = String.Empty;
          return;
        case RefDocFilterMode.Include:
          if (DocIds.Count == 0)
          {
            _FilterText = "Нет ни одной ссылки на документы \"" + DocTypeUI.DocType.PluralTitle + "\""; // 24.07.2019
            return;
          }
          break;
        case RefDocFilterMode.Exclude:
          if (DocIds.Count == 0)
          {
            _FilterText = "Фиктивное значение фильтра - все ссылки"; // 24.07.2019
            return;
          }
          sb.Append("Кроме ");
          break;
        case RefDocFilterMode.NotNull:
          _FilterText = "Ссылка на любой документ \"" + DocTypeUI.DocType.SingularTitle + "\"";
          return;
        case RefDocFilterMode.Null:
          _FilterText = "Ссылка не задана";
          return;
        default:
          throw new BugException("Неизвестный режим " + Mode.ToString());
      }


      bool isFirst = true;
      foreach (Int32 id in DocIds)
      {
        if (isFirst)
          isFirst = false;
        else
          sb.Append(", ");
        sb.Append(UI.TextHandlers.GetTextValue(DocTypeName, id));
        if (UI.DebugShowIds)
        {
          sb.Append(" (Id=)");
          sb.Append(id);
          sb.Append(")");
        }
      }
      _FilterText = sb.ToString();
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
    /// Для режима Include обычно используется значок вида документа
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <returns>Имя значка в EFPApp.MainImages</returns>
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
      if (base.DocIds != null)
        ids = base.DocIds.ToArray();

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
    /// Вызывает DBxDocTextHandlers.GetTextValue() для получения текстового представления
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
    /// Действует и в режиме Mode=Include и Mode=Exclude.
    /// Использует вызов DocTypeUI.PerformGetDocSel() в режиме EFPDBxGridViewDocSelReason.Copy.
    /// В выборку могут быть добавлены ссылки на связанные документы, если есть обработчик события
    /// DocTypeUI.GetDocSel
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    public void GetDocSel(DBxDocSelection docSel)
    {
      if (Mode != RefDocFilterMode.NoFilter)
        DocTypeUI.PerformGetDocSel(docSel, DocIds, EFPDBxGridViewDocSelReason.Copy);
    }

    /// <summary>
    /// Устанавливает фильтр, если в выборке <paramref name="docSel"/> есть документы подходящего вида.
    /// Если на момент вызова фильтр не установлен или находится в режиме Mode=Incude, то
    /// устанавливается режим Mode=Include.
    /// Если же на момент вызова фильтр находится в режиме Mode=Exclude, то этот режим сохраняется.
    /// Если выборка не содержит документов нужного вида, никаких действий не выполнеятся.
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
      if (Mode == RefDocFilterMode.NoFilter)
        newMode = RefDocFilterMode.Include;
      else
        newMode = Mode;
      SetFilter(newMode, newIds);
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Набор из фильтра по группе и фильтра по документам
  /// </summary>
  public class RefDocGridFilterSet : DBxClientFilterSet
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
        throw new ArgumentNullException("columnName");

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
    /// Если вид документов, на которое ссылается поле, не использует группы, свойство возвращает null
    /// </summary>
    public RefDocGridFilter GroupFilter { get { return _GroupFilter; } }
    private RefDocGridFilter _GroupFilter;

    /// <summary>
    /// Основной фильтр
    /// </summary>
    public RefDocGridFilter DocFilter { get { return _DocFilter; } }
    private RefDocGridFilter _DocFilter;

    /// <summary>
    /// Дублирует свойство DocFilter.DisplayName.
    /// При установке синхролнно устанавливается свойство GroupFilter.DisplayName
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
    /// Дублирует свойство DocFilter.Nullable.
    /// GroupFilter.Nullable не используется и обычно равно true
    /// </summary>
    public bool Nullable
    {
      get { return DocFilter.Nullable; }
      set { DocFilter.Nullable = value; }
    }

    /// <summary>
    /// Установка единственного выбранного документа
    /// Дублирует свойство DocFilter.SingleDocId.
    /// Если есть GroupFilter, то он очищается
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

}