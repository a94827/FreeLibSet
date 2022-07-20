// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.DependedValues;
using FreeLibSet.Data;
using System.ComponentModel;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Редактор выборки документов
  /// Содержит объект TabControl, в котором по одной вкладке для каждого вида документа
  /// </summary>
  internal class DocSelectionEditor
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект DocSelectionEditor. Элемент TabControl первоначально не содержит ни одной вкладки
    /// Затем должно быть присвоено значеник свойству DocSel, при этом будут добавлены вкладки для видов
    /// документов
    /// </summary>
    /// <param name="ui">Доступ к пользовательскому интерфейсу</param>
    /// <param name="baseProvider"></param>
    /// <param name="parentControl">Родительский управляющий элемент или форма, в которой размещается TabControl</param>
    public DocSelectionEditor(DBUI ui, EFPBaseProvider baseProvider, Control parentControl)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");
      if (parentControl == null)
        throw new ArgumentNullException("parentControl");

      _UI = ui;
      _BaseProvider = baseProvider;
      _ReadOnly = false;

      _TheTabControl = new TabControl();
      _TheTabControl.Dock = DockStyle.Fill;
      _TheTabControl.ImageList = EFPApp.MainImages.ImageList;
      parentControl.Controls.Add(_TheTabControl);
      _TableHandlers = new Dictionary<string, EFPPageGridView>();
    }

    #endregion

    #region Свойства

    public DBUI UI { get { return _UI; } }
    private DBUI _UI;


    private TabControl _TheTabControl;

    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Для встраивания в редактор документа
    /// </summary>
    public DepChangeInfoItem ChangeInfo { get { return _ChangeInfo; } set { _ChangeInfo = value; } }
    private DepChangeInfoItem _ChangeInfo;

    /// <summary>
    /// Если установить в true, то в редакторе будер недоступно изменение выборки.
    /// Свойство должно устанавливаться до присоединения выборки
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        if (_TableHandlers.Count > 0)
          throw new InvalidOperationException("Свойство ReadOnly должно устанавливаться до присоединения выборки");
        _ReadOnly = value;
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Редактируемая выборка данных
    /// </summary>
    public DBxDocSelection DocSel
    {
      get
      {
        DBxDocSelection docSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
        foreach (EFPPageGridView gh in _TableHandlers.Values)
          docSel.Add(gh.DocType.Name, gh.BufTable);
        return docSel;
      }
      set
      {
        if (value == null)
          value = new DBxDocSelection(UI.DocProvider.DBIdentity);

        SingleScopeList<string> tableNames = new SingleScopeList<string>(value.TableNames);
        foreach (string TableName in _TableHandlers.Keys)
          tableNames.Add(TableName);

        for (int i = 0; i < tableNames.Count; i++)
        {
          string tableName = tableNames[i];
          EFPPageGridView gh = ReadyTable(tableName);
          DataTable table2 = UI.DocProvider.DBCache[tableName].CreateTable(value[tableName], gh.UsedColumnNames);
          if (gh.BufTable == null)
            gh.BufTable = table2;
          else
          {
            EFPDataGridViewSelection oldSel = gh.Selection;
            gh.BufTable.BeginLoadData();
            try
            {
              gh.BufTable.Rows.Clear();
              DataTools.CopyRowsToRows(table2, gh.BufTable, false, true);
            }
            finally
            {
              gh.BufTable.EndLoadData();
            }
            gh.Selection = oldSel;
          }
          gh.InitTitle();
        }
        if (_ChangeInfo != null)
          _ChangeInfo.Changed = true;
      }
    }

    private EFPPageGridView GetInfo(int pageIndex)
    {
      TabPage page = _TheTabControl.TabPages[pageIndex];
      return (EFPPageGridView)(page.Tag);
    }

    /// <summary>
    /// Переключиться на страницу выборки. 
    /// </summary>
    public string SelectedPage
    {
      get
      {
        if (_TheTabControl.SelectedIndex >= _TableHandlers.Count)
          return String.Empty;
        if (_TheTabControl.SelectedIndex < 0)
          return String.Empty;
        else
          return GetInfo(_TheTabControl.SelectedIndex).DocType.Name;
      }
      set
      {
        _TheTabControl.SelectedTab = ReadyTable(value).Page;
        //Page2.SelectPage();
      }
    }

    /// <summary>
    /// Режим удаления:
    /// 0-ссылки из выборки
    /// 1-документы
    /// </summary>
    private int _DeleteRowsMode;

    #endregion

    #region Методы

    private EFPPageGridView ReadyTable(string tableName)
    {
      EFPPageGridView gh;
      if (_TableHandlers.TryGetValue(tableName, out gh))
        return gh;

      DBxDocType docType = UI.DocTypes[tableName].DocType;
      if (docType == null)
        throw new ArgumentException("Тип документа \"" + tableName + "\" не найден", "tableName");

      // Добавляем новую вкладку
      _TheTabControl.TabPages.Add(docType.PluralTitle);
      gh = new EFPPageGridView(_BaseProvider,
        _TheTabControl.TabPages[_TheTabControl.TabCount - 1],
        this,
        docType.Name);

      _TableHandlers.Add(tableName, gh);
      return gh;
    }

    /// <summary>
    /// Очистка выборки и закрытие всех страниц редактора 
    /// </summary>
    public void Clear()
    {
      if (_TableHandlers.Count == 0)
        return;
      _TheTabControl.TabPages.Clear();
      _TableHandlers.Clear();
      if (_ChangeInfo != null)
        _ChangeInfo.Changed = true;
      // TODO:??? Page2.Title = "Ссылки (0)";
    }


#if XXX
    private void Add(DBxDocSelection Src)
    {
      if (Src == null)
        return;
      for (int i = 0; i < Src.TableNames.Length; i++)
      {
        string TableName = Src.TableNames[i];
        Int32[] Ids = Src[TableName];
        if (Ids.Length == 0)
          continue;
        EFPPageGridView gh = ReadyTable(TableName);
        List<Int32> NewIds = new List<Int32>();
        for (int j = 0; j < Ids.Length; j++)
        {
          DataRow[] Rows = gh.BufTable.Select(new ValueFilter("Id", Ids[j]).ToString());
          if (Rows.Length > 0)
            continue;
          NewIds.Add(Ids[j]);
        }
        if (NewIds.Count == 0)
          continue;
        DataTable Table2 = UI.DocProvider.DBCache[TableName].CreateTable(NewIds.ToArray(), gh.UsedColumnNames);
        gh.BufTable.BeginLoadData();
        try
        {
          DataTools.CopyRowsToRows(Table2, gh.BufTable, false, true);
        }
        finally
        {
          gh.BufTable.EndLoadData();
        }
        gh.SelectedIds = NewIds.ToArray();
        TheTabControl.SelectedTab = gh.Page;
        if (FChangeInfo != null)
          FChangeInfo.Changed = true;
        gh.InitTitle();
      }
    }
#endif

    #endregion

    #region Вложенные классы

    /// <summary>
    /// Расширение табличного просмотра для вкладки редактора выборки
    /// </summary>
    private class EFPPageGridView : EFPDocGridView
    {
      #region Конструктор

      public EFPPageGridView(EFPBaseProvider baseProvider, TabPage page, DocSelectionEditor editor, string docTypeName)
        : base(baseProvider, CreateDataGridView(page), editor.UI, docTypeName)
      {
        _Page = page;
        _Editor = editor;

        _Page.Tag = this;
        if (String.IsNullOrEmpty(DocTypeUI.ImageKey))
          _Page.ImageKey = "Table";
        else
          _Page.ImageKey = DocTypeUI.ImageKey;


        base.ConfigSectionName = docTypeName;
        //DBxColumnList ColumnNames2 = new DBxColumnList();
        //Info.DocType.DoInitGrid(Info.ControlProvider, false, ColumnNames2, null);
        //???Info.ControlProvider.CurrentConfigChanged += new CancelEventHandler(Info.GridHandler_CurrentGridConfigChanged);
        base.CommandItems.CanEditFilters = false; // иначе вылезет меню "Фильтр"
        base.UseShowDeleted = false;
        base.ShowDeleted = true; // 16.02.2018
        //ColumnNames2.Add("Id"); // на всякий случай

        //base.ReadOnly = UI.DocProvider.DBPermissions.TableModes[Info.DocTypeName]!=DBxAccessMode.Full; // 08.07.2016
        base.ReadOnly = editor.ReadOnly; // 27.09.2017
        base.CanInsert = false;
        base.CanInsertCopy = false; // всегда
        base.CanDelete = !this.ReadOnly;
        base.CanMultiEdit = DocTypeUI.CanMultiEdit;
        base.Control.MultiSelect = true;
        base.CanView = true;
        base.Control.AllowUserToDeleteRows = true;

        base.SelectedRowsMode = EFPDataGridViewSelectedRowsMode.RowIndex; // 16.02.2018 - нет первичного ключа

        base.CommandItems.Cut += new EventHandler(GridHandler_Cut);

        DBxDocSelectionPasteFormat fmtDocSel = new DBxDocSelectionPasteFormat(UI); // все типы документов
        fmtDocSel.Paste += new EFPPasteDataObjectEventHandler(fmtDocSel_Paste);
        base.CommandItems.PasteHandler.Add(fmtDocSel);
        base.CommandItems.ClipboardInToolBar = true;

        //Info.ControlProvider.Orders.ReadConfig();

        /*
        AccDepFileType FileType = AccDepFileType.CreateDataSetXML();
        FileType.SaveCode = FileType.OpenCode + "Export";
        FileType.ImageKey = "XMLDataSet";
        FileType.DisplayName = "Данные для экспорта (XML)";
        FileType.Save += new AccDepFileEventHandler(Info.SaveDataSet);
        Info.ControlProvider.CommandItems.SaveTypes.Replace(FileType);
        */


        EFPCommandItem ci;
        base.CommandItems.AddSeparator();

        ci = new EFPCommandItem("Edit", "CopyDocSel");
        ci.MenuText = "Скопировать всю выборку документов";
        ci.Click += new EventHandler(ciCopyDocSel_Click);
        base.CommandItems.Add(ci);

        ci = new EFPCommandItem("View", "CloneDocSel");
        ci.MenuText = "Открыть копию выборки";
        ci.Click += new EventHandler(ciCloneView_Click);
        base.CommandItems.Add(ci);

        base.CommandItems.AddSeparator();

        Panel panSpb = new Panel();
        panSpb.Dock = DockStyle.Top;
        page.Controls.Add(panSpb);
        base.ToolBarPanel = panSpb;

        //Info.BufTable = Info.DocType.TableCache.CreateTable(DataTools.EmptyIds, Info.ControlProvider.UsedColumnNames);
        //Grid.DataSource = Info.BufTable.DefaultView;
        //Info.InitTitle();
      }

      private static DataGridView CreateDataGridView(TabPage page)
      {
        DataGridView grid = new DataGridView();
        grid.ReadOnly = true;
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        page.Controls.Add(grid);

        return grid;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Редактор - владелец
      /// </summary>
      public DocSelectionEditor Editor { get { return _Editor; } }
      private DocSelectionEditor _Editor;

      /// <summary>
      /// Родительский управляющий элемент
      /// </summary>
      public TabPage Page { get { return _Page; } }
      private TabPage _Page;

      /// <summary>
      /// Таблица с отображаемыми полями
      /// </summary>
      public DataTable BufTable;

      #endregion

      #region Свойства

      public override string ToString()
      {
        return base.DocType.Name;
      }

      #endregion

      #region Редактирование

      protected override void OnInitSourceDataView()
      {
        // 16.02.2018 Вместо собственного обработчика

        Int32[] docIds = Editor.DocSel[DocType.Name];
        BufTable = DocTypeUI.TableCache.CreateTable(docIds, base.UsedColumnNames);
        base.InitTableRepeaterForGridProducer(BufTable);
      }

      protected override bool OnEditData(EventArgs args)
      {
        // Базовый метод не вызываем
        // return base.OnEditData(Args);

        Int32[] docIds;
        switch (base.State)
        {
          case EFPDataGridViewState.Edit:
          case EFPDataGridViewState.View:

            docIds = base.SelectedIds;
            if (docIds.Length == 0)
            {
              EFPApp.ShowTempMessage("Строки не выбраны");
              return true;
            }
            if (!DocTypeUI.CanMultiEdit)
            {
              if (docIds.Length > 1)
              {
                EFPApp.ShowTempMessage("Множественное редактирование документов \"" + DocType.PluralTitle + "\" не разрешено");
                return true;
              }
            }
            DocTypeUI.PerformEditing(docIds, base.State, true);
            return true;

          //case EFPDataGridViewState.Insert:
          //  DoInsert();
          //  break;

          case EFPDataGridViewState.Delete:
            DoDelete();
            return true;
          default:
            EFPApp.ShowTempMessage("Неподдерживаемый режим редактирования");
            return true;
        }
      }

      //private void DoInsert()
      //{
      //  Int32[] DocIds = DocType.SelectDocs();
      //  if (DocIds.Length == 0)
      //    return;

      //  bool Added = false;
      //  DataView dv = new DataView(BufTable);
      //  try
      //  {
      //    dv.Sort = "Id";
      //    IdList NewIds = new IdList();
      //    for (int i = 0; i < DocIds.Length; i++)
      //    {
      //      int p = dv.Find(DocIds[i]);
      //      if (p < 0)
      //      {
      //        Added = true;
      //        NewIds.Add(DocIds[i]);
      //      }
      //    }
      //    if (NewIds.Count > 0)
      //    {
      //      DataTable SrcTable = DocType.TableCache.CreateTable(NewIds.ToArray(), ControlProvider.UsedColumnNames);
      //      DataTools.CopyRowsToRows(SrcTable, BufTable, false, true);
      //    }
      //  }
      //  finally
      //  {
      //    dv.Dispose();
      //  }

      //  ControlProvider.SelectedIds = DocIds;
      //  if (Added)
      //  {
      //    if (Owner.FChangeInfo != null)
      //      Owner.FChangeInfo.Changed = true;
      //    InitTitle();
      //  }
      //}

      private void DoDelete()
      {
        DataRow[] rows = base.SelectedDataRows;
        if (rows.Length == 0)
        {
          EFPApp.ShowTempMessage("Нет выбранных документов");
          return;
        }

        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = "Удаление строк документов \""+DocTypeUI.DocType.PluralTitle+"\" (" + rows.Length + ")";
        dlg.ImageKey = "Delete";
        dlg.GroupTitle = "Что требуется удалить";
        dlg.Items = new string[]{
              "Ссылки из этой выборки документов (документы останутся)",
              "Сами документы"};
        dlg.ImageKeys = new string[] { "DBxDocSelection", "Delete" };
        dlg.SelectedIndex = Editor._DeleteRowsMode;
        //if (DocTypeUI.DocTypePermissionMode != DBxAccessMode.Full)
        if (DocTypeUI.TableMode != DBxAccessMode.Full)
        {
          // 12.09.2019
          // Если нет права радактирование документов, то режим недоступен
          dlg.EnabledItemFlags[1] = false;
          dlg.SelectedIndex = 0;
        }

        #region Не помечены ли все документы на удаление?

        // 16.02.2018
        // Если все документы уже помечены на удаления, то удалять "сами документы" еще раз нельзя

        bool hasNotDeletedDocs = false;
        if (UI.DocProvider.DocTypes.UseDeleted)
        {
          for (int i = 0; i < rows.Length; i++)
          {
            if (!DataTools.GetBool(rows[i], "Deleted"))
            {
              hasNotDeletedDocs = true;
              break;
            }
          }
        }
        else
          hasNotDeletedDocs = true;

        if (!hasNotDeletedDocs)
        {
          dlg.SelectedIndex = 0;
          dlg.EnabledItemFlags[1] = false;
        }

        #endregion

        if (dlg.ShowDialog() != DialogResult.OK)
          return;
        Editor._DeleteRowsMode = dlg.SelectedIndex;

        if (Editor._DeleteRowsMode == 1)
        {
          if (!DocTypeUI.PerformEditing(this.SelectedIds, EFPDataGridViewState.Delete, true))
            return;
          InvalidateDataRows(rows); // 16.02.2018. Не удаляем строки из таблицы
        }
        else
        {
          for (int i = 0; i < rows.Length; i++)
            rows[i].Delete();

          if (Editor._ChangeInfo != null)
            Editor._ChangeInfo.Changed = true;
          InitTitle();
        }
      }


#if XXX
      /// <summary>
      /// Пользователь изменил настройку табличного просмотра
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="args"></param>
      public void GridHandler_CurrentGridConfigChanged(object sender, CancelEventArgs args)
      {
        //DBxColumnList ColumnNames2 = new DBxColumnList();
        //DocType.DoInitGrid(ControlProvider, true, ColumnNames2, null);
        //ColumnNames2.Add("Id"); // на всякий случай
        //ColumnNames = new DBxColumns(ColumnNames2);

        base.PerformRefresh();
        args.Cancel = true;
      }
#endif

#if XXX
      /// <summary>
      /// Сохранение в формате XML данных для экспорта
      /// Переопределяет основную реализацию в ClientDocType,
      /// чтобы можно было сохранить всю выборку
      /// </summary>
      /// <param name="Sender"></param>
      /// <param name="Args"></param>
      public void SaveDataSet(object Sender, AccDepFileEventArgs Args)
      {
        AccDepFileType FileType = (AccDepFileType)Sender;

        bool ThisDocTypeOnly = Args.Config.GetBool("ТолькоВыбранныйТип");
        bool AllRows = Args.Config.GetBool("ВсеСтроки"); // для совместимости с основным обработчиом

        AccDepDocSel DocSel = Owner.DocSel;

        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = "Сохранение данных для экспорта";
        dlg.ImageKey = "XMLDataSet";
        dlg.GroupTitle = "Что сохранить";
        dlg.Items = new string[] { 
        "Вся выборка ("+RusNumberConvert.IntWithNoun(DocSel.TotalCount, "документ", "документа", "документов")+")",
        "Все документы \""+DocType.PluralTitle+"\" в просмотре ("+ControlProvider.Control.RowCount.ToString()+")", 
        "Только для выбранных строк ("+ControlProvider.SelectedRowCount+")" };
        dlg.SelectedIndex = ThisDocTypeOnly ? (AllRows ? 1 : 2) : 0;
        if (dlg.ShowDialog() != DialogResult.OK)
          return;
        ThisDocTypeOnly = dlg.SelectedIndex > 0;
        if (ThisDocTypeOnly)
          AllRows = dlg.SelectedIndex == 1;
        Args.Config.SetBool("ТолькоВыбранныйТип", ThisDocTypeOnly);
        Args.Config.SetBool("ВсеСтроки", AllRows);

        DataSet ds;
        if (ThisDocTypeOnly)
        {
          // Вариант, идентичный реализации в ClientDocType
          Int32[] DocIds;
          if (AllRows)
            DocIds = DataTools.GetIds(ControlProvider.SourceAsDataView);
          else
            DocIds = ControlProvider.SelectedIds;
          ds = AccDepClientExec.CreateExportDataSet(DocType.Name, DocIds);
        }
        else
        {
          // Выбока в-целом
          ds = AccDepClientExec.CreateExportDataSet(DocSel);
        }
        AccDepClientExec.SaveExportDataSet(Args.FileName.Path, ds);
      }
#endif

#endregion

#region Буфер обмена

      public void GridHandler_Cut(object sender, EventArgs args)
      {
        if (!base.CommandItems.PerformCopy())
          return;
        base.InlineEditData(EFPDataGridViewState.Delete);
        if (Editor._ChangeInfo != null)
          Editor._ChangeInfo.Changed = true;
        InitTitle();
      }

      void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
      {
        DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
        DoPaste(fmtDocSel.DocSel);
      }

      private static int _PasteMode = 0;

      private void DoPaste(DBxDocSelection sel2)
      {
        if (sel2 == null)
        {
          EFPApp.ShowTempMessage("Буфер обмена не содержит выборки документов");
          return;
        }
        // TODO: Sel2.Normalize(AccDepClientExec.BufTables);

        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = "Вставка выборки документов";
        dlg.ImageKey = "Paste";
        dlg.Items = new string[] { 
          "Добавить ссылки (объединение выборок)", 
          "Удалить ссылки (разность выборок)",
          "Оставить общие (пересечение выборок)"};
        dlg.ImageKeys = new string[] { "Insert", "Delete", "SignMultiply" };
        dlg.GroupTitle = "Действие с выборкой";
        dlg.SelectedIndex = _PasteMode;
        if (dlg.ShowDialog() != DialogResult.OK)
          return;
        _PasteMode = dlg.SelectedIndex;

        DBxDocSelection docSel1 = Editor.DocSel;
        int cnt;
        switch (_PasteMode)
        {
          case 0:
            cnt = docSel1.Add(sel2);
            if (cnt > 0)
              Editor.DocSel = docSel1;
            else
              EFPApp.ShowTempMessage("Новых ссылок не добавлено");
            break;
          case 1:
            cnt = docSel1.Remove(sel2);
            if (cnt > 0)
              Editor.DocSel = docSel1;
            else
              EFPApp.ShowTempMessage("Ни одной ссылки не удалено");
            break;
          case 2:
            cnt = docSel1.RemoveNeg(sel2);
            if (cnt > 0)
              Editor.DocSel = docSel1;
            else
              EFPApp.ShowTempMessage("Ни одной ссылки не удалено");
            break;
          default:
            throw new BugException();
        }
      }

      void ciCopyDocSel_Click(object sender, EventArgs args)
      {
        EFPApp.Clipboard.SetDataObject(Editor.DocSel, true);
      }

#endregion

#region Другие методы

      void ciCloneView_Click(object sender, EventArgs args)
      {
        UI.ShowDocSel(Editor.DocSel, "Копия выборки");
      }

      public void InitTitle()
      {
        Page.Text = DocType.PluralTitle + " (" + BufTable.DefaultView.Count.ToString() + ")";

        // Заголовок вкладки "Ссылки"
        int cnt = 0;
        foreach (EFPPageGridView gh in Editor._TableHandlers.Values)
          cnt += gh.BufTable.DefaultView.Count;

        // TODO:?? Owner.Page2.Title = "Ссылки (" + cnt.ToString() + ")";
      }

#endregion
    }

    private Dictionary<string, EFPPageGridView> _TableHandlers;

#endregion
  }
}
