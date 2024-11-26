using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.UICore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms.Docs
{
  #region SubDocEditEventHandler

  /// <summary>
  /// Аргументы событий, связанных с редактированием поддкокументов
  /// </summary>
  public class SubDocEditEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Не создается в пользовательском коде
    /// </summary>
    /// <param name="editor">Редактор поддокумента</param>
    internal SubDocEditEventArgs(SubDocumentEditor editor)
    {
      _Editor = editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор поддокумента
    /// </summary>
    public SubDocumentEditor Editor { get { return _Editor; } }
    private readonly SubDocumentEditor _Editor;

    /// <summary>
    /// Редактор основного документа
    /// </summary>
    public DocumentEditor MainEditor { get { return _Editor.MainEditor; } }

    #endregion
  }

  /// <summary>
  /// Делегаты событий, связанных с редактированием поддкокументов
  /// </summary>
  /// <param name="sender">Вызывающий объект</param>
  /// <param name="args">Аргументы события</param>
  public delegate void SubDocEditEventHandler(object sender, SubDocEditEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="SubDocumentEditor.BeforeWrite"/>
  /// </summary>
  public class SubDocEditCancelEventArgs : SubDocEditEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Не создается в пользовательском коде.
    /// </summary>
    /// <param name="editor">Редактор поддокумента</param>
    internal SubDocEditCancelEventArgs(SubDocumentEditor editor)
      : base(editor)
    {
      _Cancel = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если это свойство установить в true, до действие будет отменено
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="SubDocumentEditor.BeforeWrite"/>
  /// </summary>
  /// <param name="sender">Редактор поддокумента</param>
  /// <param name="args">Аргументы события</param>
  public delegate void SubDocEditCancelEventHandler(object sender, SubDocEditCancelEventArgs args);

  #endregion

  #region BeforeSubDocEditEventHandler

  /// <summary>
  /// Аргументы события <see cref="SubDocTypeUI.BeforeEdit"/>
  /// </summary>
  public class BeforeSubDocEditEventArgs : SubDocEditEventArgs
  {
    #region Конструктор

    internal BeforeSubDocEditEventArgs(SubDocumentEditor editor)
      : base(editor)
    {
      _Cancel = false;
      _ShowEditor = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Установка этого поля в true приводит к отказу от работы редактора 
    /// Запись результатов выполняться не будет
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    /// <summary>
    /// Установка этого поля в false приводит к пропуску работы редактора.
    /// Сразу выполняется запись значений полей.
    /// </summary>
    public bool ShowEditor { get { return _ShowEditor; } set { _ShowEditor = value; } }
    private bool _ShowEditor;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="SubDocTypeUI.BeforeEdit"/>
  /// </summary>
  /// <param name="sender">Интерфейс поддокументов <see cref="SubDocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void BeforeSubDocEditEventHandler(object sender, BeforeSubDocEditEventArgs args);

  #endregion

  #region PasteSubDocRowsEventHandler

  /// <summary>
  /// Аргументы события <see cref="SubDocTypeUI.AdjustPastedRow"/>
  /// </summary>
  public class PasteSubDocRowsEventArgs : EventArgs
  {
    #region Конструктор

    internal PasteSubDocRowsEventArgs(DBxSingleSubDocs subDocs, DataRow[] sourceRows, string sourceTableName, IEFPSubDocView controlProvider)
    {
      _SubDocs = subDocs;
      _SourceRows = sourceRows;
      _SourceTableName = sourceTableName;
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной документ, куда будет добавлена строка поддокумента
    /// </summary>
    public DBxSingleDoc MainDoc { get { return _SubDocs.Doc; } }

    /// <summary>
    /// Список поддокументов, для которых нужно вызывать метод <see cref="DBxSingleSubDocs.Insert()"/>.
    /// </summary>
    public DBxSingleSubDocs SubDocs { get { return _SubDocs; } }
    private readonly DBxSingleSubDocs _SubDocs;

    /// <summary>
    /// Исходные строки (которые могут содержать посторонние поля или не содержать нужные поля),
    /// откуда взяты значения. Нельзя изменять.
    /// </summary>
    public DataRow[] SourceRows { get { return _SourceRows; } }
    private readonly DataRow[] _SourceRows;

    /// <summary>
    /// Имя исходной таблицы документа или поддокумента поддокумента, откуда вставляются строки
    /// </summary>
    public string SourceTableName { get { return _SourceTableName; } }
    private readonly string _SourceTableName;

    /// <summary>
    /// Исходный вставляемый набор данных. Нельзя изменять.
    /// </summary>
    public DataSet SourceDataSet { get { return _SourceRows[0].Table.DataSet; } }

    /// <summary>
    /// Провайдер табличного или иерархического просмотра, для которого выполняется вставка.
    /// Может быть null, если метод <see cref="SubDocTypeUI.PerformPasteRows(DBxSingleSubDocs, DataRow[], DocTypeUIBase, IEFPSubDocView)"/> вызывается из прикладного кода.
    /// </summary>
    public IEFPSubDocView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPSubDocView _ControlProvider;

    /// <summary>
    /// Свойство должно быть установлено в true по окончании добавления строк.
    /// В противном случае будет использован стандартный механизм вставки с использованием события <see cref="SubDocTypeUI.AdjustPastedRow"/>.
    /// </summary>
    public bool Handled { get { return _Handled; } set { _Handled = value; } }
    private bool _Handled;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="SubDocTypeUI.PasteRows"/>
  /// </summary>
  /// <param name="sender">Интерфейс поддокументов <see cref="SubDocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void PasteSubDocRowsEventHandler(object sender, PasteSubDocRowsEventArgs args);

  #endregion

  #region AdjustPastedSubDocRowEventHandler

  /// <summary>
  /// Аргументы события <see cref="SubDocTypeUI.AdjustPastedRow"/>
  /// </summary>
  public class AdjustPastedSubDocRowEventArgs : CancelEventArgs
  {
    #region Конструктор

    internal AdjustPastedSubDocRowEventArgs(DBxSingleDoc mainDoc,
      DBxSubDoc subDoc,
      DataRow sourceRow,
      string sourceTableName,
      bool isFirstRow,
      IEFPSubDocView controlProvider)
    {
      _MainDoc = mainDoc;
      _SubDoc = subDoc;
      _SourceRow = sourceRow;
      _SourceTableName = sourceTableName;
      _IsFirstRow = isFirstRow;
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной документ, куда будет добавлена строка поддокумента
    /// </summary>
    public DBxSingleDoc MainDoc { get { return _MainDoc; } }
    private readonly DBxSingleDoc _MainDoc;

    /// <summary>
    /// Поддокумент, куда можно внести изменения, установив или очистив неподходящие
    /// значения
    /// </summary>
    public DBxSubDoc SubDoc { get { return _SubDoc; } }
    private readonly DBxSubDoc _SubDoc;

    /// <summary>
    /// Свойство возвращает true, если в исходной таблице находится только одна строка,
    /// поэтому после вставки будет открыт редактор, а не выполнена прямая запись поддокументов.
    /// Это позволяет оставить строку, даже если в ней не заполнены какие-либо важные поля.
    /// </summary>
    public bool EditorWillBeOpen
    {
      get
      {
        return SourceDataSet.Tables[SourceTableName].Rows.Count == 1;
      }
    }

    /// <summary>
    /// Исходная строка (которая может содержать посторонние поля или не содержать нужные поля),
    /// откуда взяты значения. Нельзя изменять.
    /// </summary>
    public DataRow SourceRow { get { return _SourceRow; } }
    private readonly DataRow _SourceRow;

    /// <summary>
    /// Возвращает true для первой строки в операции вставки. Если требуется
    /// запрашивать у пользователя дополнительные данные, то это следует делать
    /// при обработке первой строки, затем использовать сохраненные значения.
    /// </summary>
    public bool IsFirstRow { get { return _IsFirstRow; } }
    private readonly bool _IsFirstRow;

    /// <summary>
    /// Исходный вставляемый набор данных. Нельзя изменять.
    /// </summary>
    public DataSet SourceDataSet { get { return _SourceRow.Table.DataSet; } }

    /// <summary>
    /// Имя исходной таблицы документа или поддокумента поддокумента, откуда вставляются строки.
    /// </summary>
    public string SourceTableName { get { return _SourceTableName; } }
    private readonly string _SourceTableName;

    /// <summary>
    /// Провайдер табличного или иерархического просмотра, для которого выполняется вставка.
    /// Может быть null, если метод <see cref="SubDocTypeUI.PerformPasteRows(DBxSingleSubDocs, DataRow[], DocTypeUIBase, IEFPSubDocView)"/> вызывается из прикладного кода.
    /// </summary>
    public IEFPSubDocView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPSubDocView _ControlProvider;

    /// <summary>
    /// Сюда может быть помещено сообщение, почему нельзя вставить строку
    /// (при установке свойства <see cref="CancelEventArgs.Cancel"/>=true)
    /// </summary>
    public string ErrorMessage { get { return _ErrorMessage; } set { _ErrorMessage = value; } }
    private string _ErrorMessage;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="SubDocTypeUI.AdjustPastedRow"/>
  /// </summary>
  /// <param name="sender">Интерфейс поддокументов <see cref="SubDocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void AdjustPastedSubDocRowEventHandler(object sender, AdjustPastedSubDocRowEventArgs args);

  #endregion

  /// <summary>
  /// Обработчики стороны клиента для одного вида поддокументов
  /// </summary>
  public class SubDocTypeUI : DocTypeUIBase
  {
    // Пока не используется
#if XXX
    #region Переопределенный GridProducer

    /// <summary>
    /// Переопределение метода поиска недостающего значения для "точечного" поля
    /// </summary>
    private class MyGridProducer : GridProducer
    {
    #region Конструктор

      public MyGridProducer(SubDocTypeUI Owner)
      {
        FOwner = Owner;
      }

    #endregion

    #region Свойства

      private SubDocTypeUI FOwner;

    #endregion

    #region Переопределенный метод

      protected internal override object OnGetColumnValue(DataRow Row, string ColumnName)
      {
        int p = ColumnName.IndexOf('.');
        if (p < 0)
        {
          // return base.OnGetColumnValue(Row, ColumnName);
          // 12.07.2016
          if (Row.IsNull("Id"))
            return null;
          Int32 Id = (Int32)(Row["Id"]);
          return FOwner.TableCache.GetValue(Id, ColumnName);
        }
        string RefColName = ColumnName.Substring(0, p);
        Int32 RefId = DataTools.GetInt(Row, RefColName);
        return FOwner.TableCache.GetRefValue(RefColName, RefId);
      }

    #endregion
    }

    #endregion

#endif
    #region Защищенный конструктор

    internal SubDocTypeUI(DocTypeUI docTypeUI, DBxSubDocType subDocType)
      : base(docTypeUI.UI, subDocType)
    {
      _DocTypeUI = docTypeUI;
      _SubDocType = subDocType;

      _GridProducer = new EFPSubDocTypeGridProducer(this);

      CanMultiEdit = false;
      CanInsertCopy = false;

      _PasteTableNames = new List<string>();
      _PasteTableNames.Add(subDocType.Name);
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Интерфейс документов, к которому относятся поддокументы
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private /*readonly*/ DocTypeUI _DocTypeUI;

    /// <summary>
    /// Описание вида поддокументов
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocType; } }
    private /*readonly*/ DBxSubDocType _SubDocType;

    /// <summary>
    /// Описание вида документов, к которым относятся поддокументы
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }

    /// <summary>
    /// True, если допускается одновременное редактирование или
    /// просмотр нескольких выбранных документов. По умолчанию -
    /// false (нельзя).
    /// </summary>
    public bool CanMultiEdit { get { return _CanMultiEdit; } set { _CanMultiEdit = value; } }
    private bool _CanMultiEdit;

    /// <summary>
    /// True, если допускается создание поддокумента на основании существующего
    /// (копирование). По умолчанию - false (нельзя).
    /// </summary>
    public bool CanInsertCopy { get { return _CanInsertCopy; } set { _CanInsertCopy = value; } }
    private bool _CanInsertCopy;

    /// <summary>
    /// Возможность создания поддокументов для нескольких документов при групповом редактировании.
    /// Если false (по умолчанию), то можно выбрать только один документ.
    /// Установка в true может требовать дополнительных усилий со стороны прикладного кода, чтобы все необходимые поля 
    /// поддокумента редактировались при <see cref="SubDocumentEditor.MultiDocMode"/>=true. Если при групповом редактировании
    /// обязательные поля будут пропущены, будут созданы некорректные поддокументы.
    /// Установка в true также разрешает вставку поддокументов из буфера обмена в несколько целевых документов.
    /// Свойство имеет смысл только при <see cref="FreeLibSet.Forms.Docs.DocTypeUI.CanMultiEdit"/>=true.
    /// Свойство <see cref="CanMultiEdit"/> должно быть установлено, иначе будет выброшено исключение в <see cref="DBUI.EndInit()"/>.
    /// </summary>
    public bool CanMultiInsert { get { return _CanMultiInsert; } set { _CanMultiInsert = value; } }
    private bool _CanMultiInsert;

    /// <summary>
    /// Генератор табличного просмотра.
    /// Обычно в прикладном коде сюда следует добавить описания столбцов.
    /// </summary>
    public EFPSubDocTypeGridProducer GridProducer { get { return _GridProducer; } }
    private readonly EFPSubDocTypeGridProducer _GridProducer;

    #endregion

    #region Смена DBxDocProvider

    internal void OnDocProviderChanged()
    {
      DBxSubDocType newSubDocType = DocType.SubDocs[_SubDocType.Name]; // DocTypeUI.DocType уже обновлено на момент вызова
      if (newSubDocType == null)
        throw new NullReferenceException("Не найден новый SubDocType для SubDocTypeName=\"" + _SubDocType.Name + "\" документа \"" + DocType.Name + "\"");
      _SubDocType = newSubDocType;
    }

    #endregion

    #region Инициализация табличного просмотра

    #region Основной метод

    /// <summary>
    /// Инициализация табличного просмотра поддокументов, не связанного с просмотром поддокументов
    /// на странице редактора основного документа.
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого выполняется инициализация</param>
    /// <param name="reInit">False - первоначальная инициализация при выводе просмотра на экран.
    /// True - повторная инициализация после настройки табличного просмотра пользователем</param>
    /// <param name="columns">Сюда добавляются имена столбцов, которые должны присутствовать в таблице данных</param>
    /// <param name="userInitData">Пользовательские даннные, передаваемые обработчику события <see cref="SubDocTypeUI.InitView"/></param>
    public void PerformInitGrid(EFPDBxGridView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      PerformInitGrid(controlProvider, reInit, columns, userInitData, false, false);
    }

    /// <summary>
    /// Инициализация табличного просмотра поддокументов
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого выполняется инициализация</param>
    /// <param name="reInit">False - первоначальная инициализация при выводе просмотра на экран.
    /// True - повторная инициализация после настройки табличного просмотра пользователем</param>
    /// <param name="columns">Сюда добавляются имена столбцов, которые должны присутствовать в таблице данных</param>
    /// <param name="userInitData">Пользовательские даннные, передаваемые обработчику события <see cref="SubDocTypeUI.InitView"/></param>
    /// <param name="showDocId">Если true, то будет добавлен столбец "DocId", независимо от установленного
    /// свойства <see cref="DBUI.DebugShowIds"/>. Используется <see cref="EFPSubDocGridView"/>, когда отображаются поддокументы
    /// при групповом редактировании документов</param>
    /// <param name="isSubDocsGrid">True. если инициализация выполняется для <see cref="EFPSubDocGridView"/>.
    /// False, если инициализация выполняется для просмотра, не связанного с 
    /// поддокументами на странице редактора основного документа</param>
    public void PerformInitGrid(EFPDBxGridView controlProvider, bool reInit, DBxColumnList columns, object userInitData, bool showDocId, bool isSubDocsGrid)
    {
      if (reInit)
        controlProvider.Columns.Clear();

      // Добавляем столбец с картинками
      // См. примечание от 27.05.2015 в методе DocTypeUI.DoInitGrid()
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
        imgCol.Name = "Image";
        imgCol.HeaderText = String.Empty;
        imgCol.ToolTipText = "Значок поддокумента \"" + SubDocType.SingularTitle + "\"";
        imgCol.Width = controlProvider.Measures.ImageColumnWidth;
        imgCol.FillWeight = 1; // 08.02.2017
        imgCol.Resizable = DataGridViewTriState.False;

        controlProvider.Control.Columns.Add(imgCol);
        if (EFPApp.ShowToolTips)
          controlProvider.Columns[0].CellToolTextNeeded += new EFPDataGridViewCellToolTipTextNeededEventHandler(ImgCol_CellToopTextNeeded);
      }
      if (controlProvider.MarkRowIds != null)
        controlProvider.AddMarkRowsColumn();

      //if (showDocId)
      //{
      //  controlProvider.Columns.AddInt("DocId", true, "DocId", 5);
      //  controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
      //}


      //GridProducer.InitGrid(controlProvider, reInit, controlProvider.CurrentConfig, columns);
      controlProvider.GridProducer.InitGridView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021

      controlProvider.FrozenColumns = controlProvider.CurrentConfig.FrozenColumns + (EFPApp.ShowListImages ? 1 : 0);

      if (!reInit)
      {
        // Обработчик для печати (д.б. до вызова пользовательского блока инициализации)
        if (controlProvider.DefaultOutItem != null)
          controlProvider.DefaultOutItem.DisplayName = SubDocType.PluralTitle;

        CallInitView(controlProvider, userInitData);
      }

      columns.Add("Id");
      columns.Add("DocId");
      if (UI.DocProvider.DocTypes.UseDeleted) // 16.05.2018
        columns.Add("Deleted");

      // Так не получается. Обработчик GetCellAttributes не имеет доступа к 
      //if (showDocId)
      //{
      //  controlProvider.Columns.AddText("DocId.DocumentText", false, DocType.SingularTitle, 20);
      //  controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
      //  if(!reInit)
      //    controlProvider.GetCellAttributes += Grid_GetCellAttributes;
      //}

      if (UI.DebugShowIds)
      {
        if (!controlProvider.CurrentConfig.Columns.Contains("Id")) // 16.09.2021
        {
          controlProvider.Columns.AddInt("Id");
          controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
          controlProvider.Columns.LastAdded.CanIncSearch = true;
        }
        if (!showDocId)
        {
          controlProvider.Columns.AddInt("DocId");
          controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
          controlProvider.Columns.LastAdded.CanIncSearch = true;
        }

        // Почему-то не работает, столбцы остаются пустыми
        //if (UI.DocProvider.DocTypes.UseVersions && 
        //  DocTypeViewHistoryPermission.GetAllowed(UI.DocProvider.UserPermissions, DocTypeUI.DocType.Name))
        //{
        //  columns.Add("StartVersion");
        //  columns.Add("Version2");

        //  controlProvider.Columns.AddInt("StartVersion");
        //  controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

        //  controlProvider.Columns.AddInt("Version2");
        //  controlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        //}
      }


      // 28.01.2022
      controlProvider.SetColumnsReadOnly(true);
      if (controlProvider.MarkRowsColumn != null)
        controlProvider.MarkRowsColumn.GridColumn.ReadOnly = false;

      // 16.06.2021 что за бяка? controlProvider.GetUsedColumns(columns);

      if (!reInit)
      {
        controlProvider.Control.VirtualMode = true;
        controlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ControlProvider_GetRowAttributes);
        if (EFPApp.ShowListImages)
          controlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ControlProvider_GetCellAttributes);

        if (HasGetDocSel)
        {
          if (isSubDocsGrid)
            controlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(SubDocGrid_GetDocSelWithTable);
          else
            controlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(SubDocGrid_GetDocSelWithIds);
        }

        controlProvider.ConfigHandler.Changed[EFPConfigCategories.GridConfig] = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      // TODO: if (ReInit)
      // TODO:   FToolTipExtractor = null;

      controlProvider.PerformGridProducerPostInit();
    }

    #endregion

    #region Событие InitView

    /// <summary>
    /// Вызывается при инициализации таблицы просмотра поддокументов
    /// для добавления столбцов. Если обработчик не установлен, выполняется
    /// инициализация по умолчанию.
    /// </summary>
    public event InitEFPDBxViewEventHandler InitView;

    private void CallInitView(IEFPDBxView controlProvider, object userInitData)
    {
      if (InitView != null)
      {
        try
        {
          InitEFPDBxViewEventArgs args = new InitEFPDBxViewEventArgs(controlProvider);
          args.UserInitData = userInitData;
          InitView(this, args);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка обработчика события InitView");
        }
      }
    }

    #endregion

    #region Оформление просмотра

    void ControlProvider_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      DataRow row = args.DataRow;

      if (row == null)
        return;
      // 24.11.2017
      // Вызываем пользовательский обработчик и для удаленных документов
      //if (DataTools.GetBool(Row, "Deleted"))
      //  Args.Grayed = true;
      //else
      //{
      UIDataViewColorType colorType;
      bool grayed;
      UI.ImageHandlers.GetRowColor(SubDocType.Name, row, out colorType, out grayed);
      args.ColorType = colorType;
      args.Grayed = grayed;
      //}
    }

    void ControlProvider_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnName == "Image")
      {
        DataRow row = args.DataRow;
        if (row == null)
          return;

        switch (args.Reason)
        {
          case EFPDataGridViewAttributesReason.View:
            args.Value = GetImageValue(row);
            break;

          case EFPDataGridViewAttributesReason.ToolTip:
            EFPDataGridViewConfig cfg = ((EFPDBxGridView)args.ControlProvider).CurrentConfig;
            if (cfg != null)
            {
              if (cfg.CurrentCellToolTip)
              {
                string s1 = GetToolTipText(row);
                args.ToolTipText = DataTools.JoinNotEmptyStrings(Environment.NewLine, new string[] { s1, args.ToolTipText }); // 06.02.2018
              }
            }
            if (args.ControlProvider.CurrentConfig.ToolTips.Count > 0)
            {
              string s2 = null;
              try
              {
                EFPDataViewRowInfo rowInfo = args.ControlProvider.GetRowInfo(args.RowIndex);
                s2 = GridProducer.ToolTips.GetToolTipText(args.ControlProvider.CurrentConfig, rowInfo);
                args.ControlProvider.FreeRowInfo(rowInfo);
              }
              catch (Exception e)
              {
                s2 = "Ошибка: " + e.Message;
              }
              args.ToolTipText = String.Join(EFPGridProducerToolTips.ToolTipTextSeparator, new string[] { args.ToolTipText, s2 });
            }

            break;
        }
      }
    }

    /// <summary>
    /// 01.07.2011
    /// Для картинки во всплывающей подсказке выводим текстовое представление
    /// </summary>
    void ImgCol_CellToopTextNeeded(object sender, EFPDataGridViewCellToolTipTextNeededEventArgs args)
    {
      // TODO: Int32 Id = DataTools.GetInt(Args.Row, "Id");
      // TODO: Args.ToolTipText = GetImageCellToolTipText(Id);
    }

    #endregion

    #region Выборка документов

    void SubDocGrid_GetDocSelWithTable(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      PerformGetDocSel(args.DocSel, args.DataRows, args.Reason);
    }

    void SubDocGrid_GetDocSelWithIds(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      Int32[] Ids = DataTools.GetIds(args.DataRows);
      PerformGetDocSel(args.DocSel, Ids, args.Reason);
    }

    #endregion

    #endregion

    #region Список столбцов для табличного просмотра

    /// <summary>
    /// Получить список столбцов, необходимых для табличного просмотра с заданной конфигурации
    /// Заполняется такой же список столбов, как и в методе <see cref="PerformInitGrid(EFPDBxGridView, bool, DBxColumnList, object, bool, bool)"/>, но без создания самого просмотра
    /// </summary>
    /// <param name="columns">Заполняемый список столбцов</param>
    /// <param name="config">Конфигурация табличного просмотра. Если null, то используется конфигурация по умолчанию</param>
    public void GetColumnNames(DBxColumnList columns, EFPDataGridViewConfig config)
    {
      columns.Add("Id");
      columns.Add("DocId");
      if (UI.DocProvider.DocTypes.UseDeleted) // 16.05.2018
        columns.Add("Deleted");
      DocType.DefaultOrder.GetColumnNames(columns);

      if (config == null)
      {
        config = GridProducer.DefaultConfig;
        if (config == null)
          return; // только поле Id
      }

      GridProducer.GetColumnNames(config, columns);
    }

    #endregion

    #region Инициализация TreeViewAdv

    #region Основной метод

    /// <summary>
    /// Инициализация иерархического просмотра поддокументов <see cref="EFPSubDocTreeView"/>.
    /// </summary>
    /// <param name="controlProvider">Инициализируемый провайдер просмотра</param>
    /// <param name="reInit">False - первоначальная инициализация при выводе просмотра на экран.
    /// True - повторная инициализация после настройки табличного просмотра пользователем</param>
    /// <param name="columns">Сюда добавляются имена столбцов, которые должны присутствовать в таблице данных</param>
    /// <param name="userInitData">Пользовательские даннные, передаваемые обработчику события <see cref="SubDocTypeUI.InitView"/></param>
    public void PerformInitTree(EFPSubDocTreeView controlProvider, bool reInit, DBxColumnList columns, object userInitData)
    {
      if (!reInit)
      {
        //if (CanMultiEdit)
        //  ControlProvider.CanMultiEdit = true;
        if (CanInsertCopy)
          controlProvider.CanInsertCopy = true;
      }
      else
        controlProvider.Control.Columns.Clear();

      //GridProducer.InitTree(controlProvider, reInit, controlProvider.CurrentConfig, columns);
      controlProvider.GridProducer.InitTreeView(controlProvider, reInit, controlProvider.CurrentConfig, columns); // 25.03.2021
      TreeViewCachedValueAdapter.InitColumns(controlProvider, UI.TextHandlers.DBCache, GridProducer);

      controlProvider.SetColumnsReadOnly(true);

      if (controlProvider.Control.Columns.Count > 0)
      {
        NodeStateIcon ni = new NodeStateIcon();
        //ni.DataPropertyName = "Icon";
        ni.VirtualMode = true;
        ni.ValueNeeded += new EventHandler<NodeControlValueEventArgs>(controlProvider.NodeStateIconValueNeeded);
        ni.ParentColumn = controlProvider.Control.Columns[0];
        controlProvider.Control.NodeControls.Insert(0, ni);

        controlProvider.Control.Columns[0].Width += 24; // ???
      }



      if (!reInit)
      {
        if (controlProvider.DefaultOutItem != null)
          controlProvider.DefaultOutItem.DisplayName = SubDocType.PluralTitle;

#if XXX
        for (int i = 0; i < PrintTypes.Count; i++)
        {
          AccDepGridPrintDoc pd;
          pd = new AccDepGridPrintDoc(PrintTypes[i], ControlProvider, this, false);
          ControlProvider.PrintList.Add(pd);
          // 24.11.2011 Не нужно. Есть печать из "Сервис" - "Бланки"
          // pd = new AccDepGridPrintDoc(PrintTypes[i], GridHandler, this, true);
          // GridHandler.PrintList.Add(pd);
        }
#endif

        CallInitView(controlProvider, userInitData);
      }
#if XXX
      // Добавляем столбец с картинками
      if (EFPApp.ShowListImages)
      {
        DataGridViewImageColumn ImgCol = new DataGridViewImageColumn();
        ImgCol.Name = "Image";
        ImgCol.HeaderText = String.Empty;
        ImgCol.ToolTipText = "Значок документа \"" + DocType.SingularTitle + "\"";
        ImgCol.Width = ControlProvider.Measures.ImageColumnWidth;
        ImgCol.Resizable = DataGridViewTriState.False;
        //string ImgName = SingleDocImageKey;
        //ImgCol.Image = EFPApp.MainImages.Images[ImgName];
        if (ControlProvider.Control.Columns.Count > 0 && ControlProvider.Control.Columns[0].Frozen)
          ImgCol.Frozen = true;
        ControlProvider.Control.Columns.Insert(0, ImgCol);
        if (EFPApp.ShowToolTips)
          ControlProvider.Columns[0].CellToopTextNeeded += new EFPDataGridViewCellToolTipTextNeededEventHandler(ImgCol_CellToopTextNeeded);
      }
#endif
      columns.Add("Id");
      columns.Add("DocId");
      columns.Add(SubDocType.TreeParentColumnName);

      if (UI.DebugShowIds &&
        (!controlProvider.CurrentConfig.Columns.Contains("Id"))) // 16.09.2021
      {
        controlProvider.Columns.AddInt("Id", true, "Id", 6);
        //ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        //ControlProvider.Columns.LastAdded.CanIncSearch = true;
        controlProvider.Columns.AddInt("DocId", true, "DocId", 6);
        //ControlProvider.Columns.LastAdded.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
      }

      // TODO:
      //if (EFPApp.ShowListImages)
      //  Columns = Columns + ColumnsForImage;
      //Columns = Columns + FieldsForColors;
      //Columns = Columns + ControlProvider.GetUsedFields();
      //DataFields CondFields = AccDepClientExec.AllDocConditions.GetFields(Name);
      //Columns += CondFields;

      if (!reInit)
      {
        //ControlProvider.Control.VirtualMode = true;
        //ControlProvider.UseRowImages = true;
        //ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ControlProvider_GetRowAttributes);
        //if (CondFields != null)
        //  ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GridHandler_GetRowAttributesForCond);
        //if (EFPApp.ShowListImages)
        //  ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ControlProvider_GetCellAttributes);
        //DocGridHandler.MainGrid.CellPainting += new DataGridViewCellPaintingEventHandler(TheGrid_CellPainting);
        //DocGridHandler.MainGrid.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(TheGrid_CellToolTipTextNeeded);

        if (HasGetDocSel)
        {
          // ???
          //if (IsSubDocsGrid)
          //  ControlProvider.GetDocSel += new EFPDBxTreeViewDocSelEventHandler(SubDocTree_GetDocSelWithTable);
          //else
          controlProvider.GetDocSel += new EFPDBxTreeViewDocSelEventHandler(SubDocTree_GetDocSelWithIds);

          controlProvider.ConfigHandler.Changed[EFPConfigCategories.TreeConfig] = false;
        }

        //??controlProvider.UserConfigModified = false;
      }

      // После изменения конфигурации, возможно, выводятся другие всплывающие подсказки
      //      if (ReInit)
      //        FToolTipExtractor = null;
    }

    #endregion

#if XXX
    void SubDocTree_GetDocSelWithTable(object sender, EFPDBxTreeViewDocSelEventArgs args)
    {
      PerformGetDocSel(args.DocSel, args.DataRows, args.Reason);
    }
#endif

    void SubDocTree_GetDocSelWithIds(object sender, EFPDBxTreeViewDocSelEventArgs args)
    {
      Int32[] subDocIds = DataTools.GetIds(args.DataRows);
      PerformGetDocSel(args.DocSel, subDocIds, args.Reason);
    }

    #endregion

    #region Свойства и методы буферизации данных

    /// <summary>
    /// Получить идентификаторы всех поддокументов, кроме удаленных, для документа
    /// с заданным идентификатором.
    /// </summary>
    /// <param name="docId">Идентификатор основного документа. Не может быть 0</param>
    /// <returns>Массив идентификаторов поддокументов</returns>
    public Int32[] GetSubDocIds(Int32 docId)
    {
      if (docId == 0)
        throw new ArgumentException("Идентификатор документа должен быть задан", "docId");

      // TODO: !!!
      /*
      if (DataBuffering)
      {
        DataTable Table = GetBufferedData(null);
        DataView dv = new DataView(Table);
        dv.RowFilter = "DocId=" + DocId.ToString() + " AND (ISNULL(Deleted) OR (Deleted=FALSE))";

        int[] SubDocIds = new int[dv.Count];
        for (int i = 0; i < SubDocIds.Length; i++)
          SubDocIds[i] = DataTools.GetInt(dv[i]["Id"]);
        return SubDocIds;
      }
      else 
       * */
      return UI.DocProvider.GetSubDocIds(DocType.Name, SubDocType.Name, docId);
    }


    #endregion

    #region Выбор поддокумента

    /// <summary>
    /// Выбор поддокумента в процессе редактирования документа.
    /// Позволяет выбирать поддокумент из загруженного в память набора <see cref="DBxDocSet"/>.
    /// Используйте класс <see cref="SubDocSelectDialog"/> для задания дополнительных параметров.
    /// </summary>
    /// <param name="subDocs">Список поддокументов, из которых можно выбирать</param>
    /// <param name="subDocId">Вход и выход: Идентификатор выбраного поддокумента.
    /// 0, если нет выбранного документа</param>
    /// <param name="title">Заголовок блока диалога.
    /// Если не задано, используется "Выбор поддокументов X"</param>
    /// <param name="canBeEmpty">Если true, то допускается нажатие кнопки "ОК", если не выбрано ни одного поддокумента.
    /// Также в диалоге присутствует кнопка "Нет".
    /// Если false, то пользователь должен выбрать хотя бы один поддокумент из списка</param>
    /// <returns>True, если пользователь выбрал поддокументы и нажал "ОК" или "Нет"</returns>
    public bool SelectSubDoc(DBxMultiSubDocs subDocs, ref Int32 subDocId, string title, bool canBeEmpty)
    {
      SubDocSelectDialog dlg = new SubDocSelectDialog(this, subDocs);
      dlg.SelectionMode = DocSelectionMode.Single;
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.CanBeEmpty = canBeEmpty;
      dlg.SubDocId = subDocId;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        subDocId = dlg.SubDocId;
        return true;
      }
      else
        return false;
    }

#if XXXX
    // Пока неохота делать

    public bool SelectSubDoc(ref Int32 SubDocId, string Title, bool CanBeEmpty, GridFilters Filters)
    {
      SubDocTableViewForm Form = new SubDocTableViewForm(this, DocTableViewMode.SelectDoc);
      bool Res = false;
      try
      {
        if (String.IsNullOrEmpty(Title))
          Form.Text = "Выбор документа \"" + DocType.SingularTitle + "\"";
        else
          Form.Text = Title;
        Form.TheButtonPanel.Visible = true;
        Form.CanBeEmpty = CanBeEmpty;
        Form.CurrentDocId = DocId;
        Form.ExternalFilters = Filters;
        Form.ExternalEditorCaller = ExternalEditorCaller;
        switch (EFPApp.ShowDialog(Form, false))
        {
          case DialogResult.OK:
            DocId = Form.CurrentDocId;
            Res = true;
            break;
          case DialogResult.No:
            DocId = 0;
            Res = true;
            break;
          default:
            Res = false;
            break;
        }
      }
      finally
      {
        Form.Dispose();
      }
      return Res;
    }
#endif


    /// <summary>
    /// Выбор поддокумента в процессе редактирования документа
    /// Позволяет выбирать поддокументы из загруженного в память набора <see cref="DBxDocSet"/>.
    /// Используйте класс <see cref="SubDocSelectDialog"/> для задания дополнительных параметров.
    /// </summary>
    /// <param name="subDocs">Список поддокументов, из которых можно выбирать</param>
    /// <param name="subDocIds">Вход и выход: Массив идентификаторов выбранных поддокументов.
    /// Пустой массив, если нет выбранных поддокументов</param>
    /// <param name="title">Заголовок блока диалога.
    /// Если не задано, используется "Выбор поддокументов X"</param>
    /// <param name="canBeEmpty">Если true, то допускается нажатие кнопки "ОК", если не выбрано ни одного поддокумента.
    /// Также в диалоге присутствует кнопка "Нет".
    /// Если false, то пользователь должен выбрать хотя бы один поддокумент из списка</param>
    /// <returns>True, если пользователь выбрал поддокументы и нажал "ОК" или "Нет"</returns>
    public bool SelectSubDocs(DBxMultiSubDocs subDocs, ref Int32[] subDocIds, string title, bool canBeEmpty)
    {
      SubDocSelectDialog dlg = new SubDocSelectDialog(this, subDocs);
      dlg.SelectionMode = DocSelectionMode.MultiSelect;
      if (!String.IsNullOrEmpty(title))
        dlg.Title = title;
      dlg.CanBeEmpty = canBeEmpty;
      dlg.SubDocIds = subDocIds;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        subDocIds = dlg.SubDocIds;
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Выбор документа

    /// <summary>
    /// Выбор документов для добавления поддокумента.
    /// Возможность выбора нескольких документов определяется свойством <see cref="SubDocTypeUI.CanMultiInsert"/>.
    /// Если редактируется единственный документ, то возвращается его идентификатор без показа диалога выбора.
    /// Используется <see cref="EFPSubDocGridView"/> и <see cref="EFPSubDocTreeView"/>.
    /// </summary>
    /// <param name="controlProvider">Просмотр, в котором выполняется редактирование</param>
    /// <returns>Массив идентификаторов <see cref="DBxSingleDoc.DocId"/>, если выбор сделан или null, если пользователь нажал "Отмена" в блоке диалога</returns>
    internal Int32[] SelectDocsForInsert(IEFPSubDocView controlProvider)
    {
      if (controlProvider.SubDocs.Owner.DocCount == 1)
      {
        Int32 docId = controlProvider.SubDocs.Owner[0].DocId;
        return new Int32[1] { docId };
      }

      Int32 currDocId = 0;
      if (controlProvider.CurrentDataRow != null)
        currDocId = DataTools.GetInt(controlProvider.CurrentDataRow, "DocId");

      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = SubDocType.SingularTitle + " (Создание)";
      dlg.ImageKey = "Insert";
      dlg.ListTitle = SubDocType.DocType.SingularTitle;
      dlg.Items = new string[controlProvider.SubDocs.Owner.DocCount];
      dlg.ImageKeys = new string[controlProvider.SubDocs.Owner.DocCount];
      dlg.MultiSelect = controlProvider.SubDocTypeUI.CanMultiInsert;
      dlg.CanBeEmpty = false;
      for (int i = 0; i < controlProvider.SubDocs.Owner.DocCount; i++)
      {
        //dlg.Items[i + 1] = (i + 1).ToString() + ". DocId=" + controlProvider.SubDocs.Owner[i].DocId.ToString(); // !!! Названия для документов
        DBxSingleDoc doc = controlProvider.SubDocs.Owner[i];
        dlg.Items[i] = UI.TextHandlers.GetTextValue(doc);
        dlg.ImageKeys[i] = UI.ImageHandlers.GetImageKey(doc);
        if (controlProvider.SubDocs.Owner[i].DocId == currDocId)
        {
          dlg.SelectedIndex = i;
          if (dlg.MultiSelect)
            dlg.Selections[i] = true;
        }
      }

      if (dlg.ShowDialog() != DialogResult.OK)
        return null;

      if (dlg.MultiSelect)
      {
        List<Int32> docIds = new List<int>();
        for (int i = 0; i < controlProvider.SubDocs.Owner.DocCount; i++)
        {
          if (dlg.Selections[i])
            docIds.Add(controlProvider.SubDocs.Owner[i].DocId);
        }
        return docIds.ToArray();
      }
      else
      {
        Int32 docId = controlProvider.SubDocs.Owner[dlg.SelectedIndex].DocId;
        return new Int32[1] { docId };
      }
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Возвращает true, если есть хотя бы один установленный обработчик
    /// <see cref="BeforeEdit"/> или <see cref="InitEditForm"/>, то есть возможность редактирования  была инициализирована.
    /// </summary>
    public bool HasEditorHandlers
    {
      get
      {
        return /*Editing != null || */BeforeEdit != null || InitEditForm != null;
      }
    }

    /// <summary>
    /// Вызывается при попытке вставить, отредактировать или просмотреть поддокумент.
    /// Обработчик может установить свойство <see cref="BeforeSubDocEditEventArgs.Cancel"/> для предотвращения вывода на
    /// экран формы редактора.
    /// </summary>
    public event BeforeSubDocEditEventHandler BeforeEdit;

    /// <summary>
    /// Вызывает событие <see cref="BeforeEdit"/>
    /// </summary>
    internal void DoBeforeEdit(SubDocumentEditor editor, out bool cancel, out bool showEditor)
    {
      if (BeforeEdit == null)
      {
        cancel = false;
        showEditor = true;
      }
      else
      {
        BeforeSubDocEditEventArgs args = new BeforeSubDocEditEventArgs(editor);
        BeforeEdit(this, args);
        cancel = args.Cancel;
        showEditor = args.ShowEditor;
      }
    }

    /// <summary>
    /// Вызывается при инициализации окна редактирования поддокументов.
    /// Обработчик события должен добавить вкладки в редактор.
    /// </summary>
    public event InitSubDocEditFormEventHandler InitEditForm;

    internal void PerformInitEditForm(SubDocumentEditor editor)
    {
      InitSubDocEditFormEventArgs args = new InitSubDocEditFormEventArgs(editor);
      if (InitEditForm != null)
        InitEditForm(this, args);
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "ОК" в редакторе поддокумента.
    /// Значения из полей редактирования уже перенесены в <see cref="SubDocumentEditor.SubDocs"/> и их
    /// можно там подкорректировать.
    /// Установка свойства <see cref="SubDocEditCancelEventArgs.Cancel"/> предотвращает закрытие редактора поддокумента.
    /// </summary>
    public event SubDocEditCancelEventHandler Writing;

    internal bool DoWriting(SubDocumentEditor editor)
    {
      if (Writing != null)
      {
        SubDocEditCancelEventArgs args = new SubDocEditCancelEventArgs(editor);
        Writing(this, args);
        if (args.Cancel)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Вызывется после нажатия кнопки "ОК" в редакторе поддокумента в режиме
    /// копирования, создания или редактирования. Может вызываться
    /// многократно.
    /// </summary>
    public event SubDocEditEventHandler Wrote;

    internal void DoWrote(SubDocumentEditor editor)
    {
      if (Wrote != null)
      {
        SubDocEditEventArgs args = new SubDocEditEventArgs(editor);
        Wrote(this, args);
      }
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Получение выборки документов, связанных с заданными поддокументами.
    /// Вызывает обработчик событие <see cref="DocTypeUIBase.GetDocSel"/>, если он установлен. 
    /// Иначе не выполняется никаких действий.
    /// </summary>
    /// <param name="docSel">Выборка, в которую выполняется добавление документов</param>
    /// <param name="subDocIds">Идентификаторы поддокументов</param>
    /// <param name="reason">Причина создания выборки</param>
    public override void PerformGetDocSel(DBxDocSelection docSel, Int32[] subDocIds, EFPDBxViewDocSelReason reason)
    {
      if (subDocIds == null || subDocIds.Length == 0)
        return;

      base.OnGetDocSel(docSel, subDocIds, reason);
    }


    /// <summary>
    /// Создать выборку связанных документов если задан обработчик события <see cref="DocTypeUIBase.GetDocSel"/>.
    /// Если обработчик не задан, то возвращается пустая выборка документов
    /// Эта версия метода используется в <see cref="EFPSubDocGridView"/>, когда некоторые поддокументы
    /// могут быть еще не записаны.
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    /// <param name="rows">Выбранные строки таблицы поддокументов</param>
    /// <param name="reason">Причина построения выборки</param>
    /// <returns>Выборка</returns>
    public void PerformGetDocSel(DBxDocSelection docSel, DataRow[] rows, EFPDBxViewDocSelReason reason)
    {
      if (rows == null || rows.Length == 0)
        return;

      // Собственные идентификаторы не добавляются

      base.OnGetDocSel(docSel, rows, reason);

      return;
    }
    #endregion

    #region Ручная сортировка поддокументов

    /// <summary>
    /// Имя столбца, предназначенного для ручной сортировки строк или пустая строка, если
    /// порядок строк определяется в <see cref="GridProducer"/> или <see cref="DBxDocTypeBase.DefaultOrder"/>.
    /// </summary>
    public string ManualOrderColumn
    {
      get { return _ManualOrderColumn; }
      set
      {
        if (!String.IsNullOrEmpty(value))
        {
          if (!SubDocType.Struct.Columns.Contains(value))
            throw new ArgumentException("Таблица \"" + SubDocType.Name + "\" не содержит столбца \"" + value + "\"");
          this.Columns[value].NewMode = ColumnNewMode.AlwaysDefaultValue; // чтобы новые поддокументы попадали в конец списка
        }
        _ManualOrderColumn = value;
      }
    }
    private string _ManualOrderColumn;

    /// <summary>
    /// Заполнение поля <see cref="ManualOrderColumn"/>.
    /// Обнаруживает среди всех поддокументов, те в которых значение поля для ручной сортировки равно 0.
    /// Для них устанавливается значение этого поля так, чтобы поддокументы оказались в конце списка.
    /// Если используется иерархия поддокументов, то она учитывается, при этом может поменяться значение поля <see cref="ManualOrderColumn"/> у других поддокументов.
    /// Если свойство <see cref="ManualOrderColumn"/> не установлено, никаких действий не выполняется.
    /// </summary>
    /// <param name="subDocs">Поддокументы</param>
    /// <returns>true, если был найден хотя бы один поддокумент, у которого <see cref="ManualOrderColumn"/> равно 0</returns>
    public bool InitManualOrderColumnValue(DBxMultiSubDocs subDocs)
    {
      if (String.IsNullOrEmpty(ManualOrderColumn))
        return false;

      if (subDocs.SubDocCount == 0)
        return false; // пустой список

      if (subDocs.Owner.DocCount != 1)
        throw new InvalidOperationException("Не реализовано для наборов, в которых редактируется несколько документов");

      int pOrderCol = subDocs.Values.IndexOf(ManualOrderColumn);
      if (pOrderCol < 0)
        throw new BugException("Не найдено поле \"" + ManualOrderColumn + "\"");
      List<DataRow> lstRows = null;
      foreach (DBxSubDoc sd in subDocs)
      {
        if (sd.SubDocState == DBxDocState.Delete)
          continue; // 17.06.2022
        if (sd.Values[pOrderCol].AsInteger == 0)
        {
          DataRow row = subDocs.SubDocsView.Table.Rows.Find(sd.SubDocId);
          if (row == null)
            throw new BugException("Не найдена строка поддокумента для SubDocId=" + sd.SubDocId);

          if (lstRows == null)
            lstRows = new List<DataRow>();
          lstRows.Add(row);
        }
      }

      if (lstRows == null)
        return false;

      IDataReorderHelper helper;
      bool otherRowsChanged;
      if (String.IsNullOrEmpty(SubDocType.TreeParentColumnName))
      {
        helper = new DataTableReorderHelper(subDocs.SubDocsView, ManualOrderColumn);
        helper.InitRows(lstRows.ToArray(), out otherRowsChanged);
      }
      else
      {
        using (DBxSubDocTreeModel model = new DBxSubDocTreeModel(subDocs, null))
        {
          helper = new DataTableTreeReorderHelper(model, ManualOrderColumn);
          helper.InitRows(lstRows.ToArray(), out otherRowsChanged);
        }
      }

      return true;
    }

    internal bool InitManualOrderColumnValueAfterEdit(DBxMultiSubDocs mainSubDocs, DBxMultiSubDocs subDocs2)
    {
      if (String.IsNullOrEmpty(ManualOrderColumn))
        return false;

      List<DataRow> lstRows1 = null;
      foreach (DBxSubDoc sd2 in subDocs2)
      {
        if (sd2.Values[ManualOrderColumn].AsInteger == 0)
        {
          DataRow row1 = mainSubDocs.SubDocsView.Table.Rows.Find(sd2.SubDocId);
          if (row1 == null)
            throw new BugException("Не найдена строка поддокумента для SubDocId=" + sd2.SubDocId);

          if (lstRows1 == null)
            lstRows1 = new List<DataRow>();
          lstRows1.Add(row1);
        }
      }
      if (lstRows1 == null)
        return false;

      IDataReorderHelper helper;
      bool otherRowsChanged;
      if (String.IsNullOrEmpty(SubDocType.TreeParentColumnName))
      {
        helper = new DataTableReorderHelper(mainSubDocs.SubDocsView, ManualOrderColumn);
        helper.InitRows(lstRows1.ToArray(), out otherRowsChanged);
      }
      else
      {
        using (DBxSubDocTreeModel model = new DBxSubDocTreeModel(mainSubDocs, null))
        {
          helper = new DataTableTreeReorderHelper(model, ManualOrderColumn);
          helper.InitRows(lstRows1.ToArray(), out otherRowsChanged);
        }
      }

      return true;
    }

    #endregion

    #region Вставка в EFPSubDocGridView

    /// <summary>
    /// Список имен документов и поддокументов, из которых возможна вставка табличных
    /// данных. Вставка выполняется в редакторе основного документа (в <see cref="EFPSubDocGridView"/>).
    /// По умолчанию в списке находится имя текущего поддокумента, то есть
    /// вставлять можно только аналогичные поддокументы.
    /// </summary>
    public List<string> PasteTableNames { get { return _PasteTableNames; } }
    private readonly List<string> _PasteTableNames;


    /// <summary>
    /// Событие вызывается при вставке строк поддокументов из буфера обмена.
    /// Обработчик может выполнить добавление поддокументов самостоятельно и установить свойство <see cref="PasteSubDocRowsEventArgs.Handled"/>=true.
    /// При этом событие <see cref="AdjustPastedRow"/> не вызывается.
    /// </summary>
    public event PasteSubDocRowsEventHandler PasteRows;

    /// <summary>
    /// Событие вызывается при вставке копии строки поддокумента из буфера
    /// обмена. Обработчик может изменить значения полей в строке.
    /// </summary>
    public event AdjustPastedSubDocRowEventHandler AdjustPastedRow;

    /// <summary>
    /// Вызывает обработчик события <see cref="AdjustPastedRow"/>, если обработчик установлен.
    /// </summary> 
    /// <param name="args">Аргументы события</param>
    private void PerformAdjustPastedRow(AdjustPastedSubDocRowEventArgs args)
    {
      if (AdjustPastedRow != null)
        AdjustPastedRow(this, args);
    }

    /// <summary>
    /// Выполнить вставку поддокументов в один документ из строк таблицы данных.
    /// Вызывается обработчик события <see cref="PasteRows"/>. Если обработчик не задан или свойство <see cref="PasteSubDocRowsEventArgs.Handled"/> не установлено,
    /// то для каждой строки выполняется копирование полей в новый поддокумент и вызывается событие <see cref="AdjustPastedRow"/>.
    /// </summary>
    /// <param name="subDocs">Поддокументы для документа, в который выполняется добавление</param>
    /// <param name="srcRows">Массив исходных строк для добавления.</param>
    /// <param name="srcDocTypeBase">Описание документа или поддокумента, к которому относятся исходные строки</param>
    /// <param name="controlProvider">Табличный или иерархический просмотр, для которого выполняется вставка.
    /// Ссылка передается обработчикам событий. Может быть null.</param>
    /// <returns>Массив созданных документов или null, если вставка не выполнена</returns>
    public DBxSubDoc[] PerformPasteRows(DBxSingleSubDocs subDocs, DataRow[] srcRows, DocTypeUIBase srcDocTypeBase, IEFPSubDocView controlProvider)
    {
      if (srcRows.Length == 0)
        return null;

      // Нельзя использовать в качестве оригинала полученную строку, т.к. таблица в буфере обмена может быть неполной
      DBxMultiSubDocs subDocs2 = new DBxMultiSubDocs(subDocs.SubDocs, DataTools.EmptyIds);

      int orgSubDocCount = subDocs.SubDocs.SubDocCount;

      PasteSubDocRowsEventArgs args1 = new PasteSubDocRowsEventArgs(subDocs, srcRows, srcDocTypeBase.DocTypeBase.Name, controlProvider);
      if (PasteRows != null)
        PasteRows(this, args1);
      if (!args1.Handled)
      {
        int cntCancelled = 0;
        ErrorMessageList errors = new ErrorMessageList();
        for (int i = 0; i < srcRows.Length; i++)
        {
          DBxSubDoc subDoc2 = subDocs2.Insert();
          DBxExtValue.CopyValues(srcRows[i], subDoc2.Values);
          if (!String.IsNullOrEmpty(ManualOrderColumn))
            subDoc2.Values[ManualOrderColumn].SetNull(); // до пользовательского обработчика

          // Вызываем пользовательский обработчик
          AdjustPastedSubDocRowEventArgs args2 = new AdjustPastedSubDocRowEventArgs(subDocs.Doc, subDoc2, srcRows[i], srcDocTypeBase.DocTypeBase.Name, i == 0, controlProvider);
          if (AdjustPastedRow != null)
            AdjustPastedRow(this, args2);

          if (args2.Cancel)
          {
            cntCancelled++;
            if (String.IsNullOrEmpty(args2.ErrorMessage))
              args2.ErrorMessage = "Нельзя добавить строку";
            errors.AddWarning("Строка " + (i + 1).ToString() + " пропускается. " + args2.ErrorMessage);
            subDoc2.Delete();
          }
        }
        // Убираем отмененные строки и возвращаем состояние Insert на место
        subDocs2.SubDocsView.Table.AcceptChanges();
        foreach (DataRow row in subDocs2.SubDocsView.Table.Rows)
          DataTools.SetRowState(row, DataRowState.Added);

        if (cntCancelled > 0)
        {
          if (subDocs2.SubDocCount == 0)
            errors.AddError("Ни одна из строк (" + srcRows.Length.ToString() + " шт.) не может быть добавлена");
          EFPApp.ShowErrorMessageListDialog(errors, "Вставка");
          if (subDocs2.SubDocCount == 0)
            return null;
        }

        DocumentEditor mainEditor = null;
        if (controlProvider != null)
          mainEditor = controlProvider.MainEditor;

        if (HasEditorHandlers && subDocs2.SubDocCount == 1 && mainEditor != null)
        {
          // Открытие редактора поддокумента
          // Режим должен быть обязательно InsertCopy, иначе значения не прочитаются
          SubDocumentEditor sde = new SubDocumentEditor(mainEditor, subDocs2, UIDataState.InsertCopy);
          sde.SuppressInsertColumnValues = true; // не нужна инициализация, иначе некоторые поля с режимом NewMode=AlwaysDefaultValue очистятся
          if (!sde.Run())
            return null;
        }
        else
        {
          if (EFPApp.MessageBox("Вставить " + (srcDocTypeBase.DocTypeBase.IsSubDoc ?
            "копии поддокументов" : "копии документов") + " (" + subDocs2.SubDocCount.ToString() + " шт.)?",
            "Подтверждение вставки поддокументов \"" + SubDocType.PluralTitle + "\"",
            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)

            return null;
        }
      }
      orgSubDocCount = subDocs.SubDocs.SubDocCount; // перечитываем еще раз на случай посторонних действий пользователя
      subDocs.SubDocs.MergeSubSet(subDocs2);
      InitManualOrderColumnValueAfterEdit(subDocs.SubDocs, subDocs2);
      DBxSubDoc[] a = new DBxSubDoc[subDocs.SubDocs.SubDocCount - orgSubDocCount];
      List<DataRow> resRows = new List<DataRow>();
      for (int i = 0; i < a.Length; i++)
        a[i] = subDocs.SubDocs[orgSubDocCount + i];
      return a;
    }

    /// <summary>
    /// Выполнить вставку поддокументов в несколько документов из строк таблицы данных.
    /// Если задано несколько документов <paramref name="docIds"/>, то для каждого из них вставляется полный набор поддокументов из строк <paramref name="srcRows"/>.
    /// Вызывается обработчик события <see cref="PasteRows"/>. Если обработчик не задан или свойство <see cref="PasteSubDocRowsEventArgs.Handled"/> не установлено,
    /// то для каждой строки выполняется копирование полей в новый поддокумент и вызывается событие <see cref="AdjustPastedRow"/>.
    /// </summary>
    /// <param name="mSubDocs">Поддокументы</param>
    /// <param name="docIds">Идентификаторы документов, в которые добавляются документы. Идентификаторы должны принадлежать
    /// набору <see cref="DBxMultiSubDocs.Owner"/>. Могут быть фиктивные идентификаторы для новых документов.
    /// Документы должны находиться в состоянии создания или редактирования. В наборе могут быть и другие документы, кроме <paramref name="docIds"/>, их состояние не имеет значения.</param>
    /// <param name="srcRows">Массив исходных строк для добавления.</param>
    /// <param name="srcDocTypeBase">Описание документа или поддокумента, к которому относятся исходные строки</param>
    /// <param name="controlProvider">Табличный или иерархический просмотр, для которого выполняется вставка.
    /// Ссылка передается обработчикам событий. Может быть null.</param>
    /// <returns>Массив созданных документов или null, если вставка не выполнена</returns>
    public DBxSubDoc[] PerformPasteRows(DBxMultiSubDocs mSubDocs, Int32[] docIds, DataRow[] srcRows, DocTypeUIBase srcDocTypeBase, IEFPSubDocView controlProvider)
    {
      if (srcRows.Length == 0)
        return null;
      if (docIds == null)
        docIds = mSubDocs.Owner.DocIds;

      // Нельзя использовать в качестве оригинала полученную строку, т.к. таблица в буфере обмена может быть неполной
      DBxMultiSubDocs subDocs2 = new DBxMultiSubDocs(mSubDocs, DataTools.EmptyIds);

      int orgSubDocCount = mSubDocs.SubDocCount;
      int cntCancelled = 0;
      bool allPasteHandled = true;
      ErrorMessageList errors = new ErrorMessageList();
      for (int j = 0; j < docIds.Length; j++)
      {
        DBxSingleDoc doc = mSubDocs.Owner.GetDocById(docIds[j]);
        PasteSubDocRowsEventArgs args1 = new PasteSubDocRowsEventArgs(doc.SubDocs[mSubDocs.SubDocType.Name], srcRows, srcDocTypeBase.DocTypeBase.Name, controlProvider);
        if (PasteRows != null)
          PasteRows(this, args1);
        if (!args1.Handled)
        { 
          allPasteHandled = false;
          for (int i = 0; i < srcRows.Length; i++)
          {
            DBxSubDoc subDoc2 = subDocs2.Insert(doc);
            DBxExtValue.CopyValues(srcRows[i], subDoc2.Values);
            if (!String.IsNullOrEmpty(ManualOrderColumn))
              subDoc2.Values[ManualOrderColumn].SetNull(); // до пользовательского обработчика

            // Вызываем пользовательский обработчик
            AdjustPastedSubDocRowEventArgs args2 = new AdjustPastedSubDocRowEventArgs(doc, subDoc2, srcRows[i], srcDocTypeBase.DocTypeBase.Name, i == 0 && j == 0, controlProvider);
            if (AdjustPastedRow != null)
              AdjustPastedRow(this, args2);

            if (args2.Cancel)
            {
              cntCancelled++;
              if (String.IsNullOrEmpty(args2.ErrorMessage))
                args2.ErrorMessage = "Нельзя добавить строку";
              errors.AddWarning("Строка " + (i + 1).ToString() + " пропускается. " + args2.ErrorMessage);
              subDoc2.Delete();
            }
          }
        }
      } // цикл по документам

      if (!allPasteHandled)
      {
        // Убираем отмененные строки и возвращаем состояние Insert на место
        subDocs2.SubDocsView.Table.AcceptChanges();
        foreach (DataRow row in subDocs2.SubDocsView.Table.Rows)
          DataTools.SetRowState(row, DataRowState.Added);

        if (cntCancelled > 0)
        {
          if (subDocs2.SubDocCount == 0)
            errors.AddError("Ни одна из строк (" + srcRows.Length.ToString() + " шт.) не может быть добавлена");
          EFPApp.ShowErrorMessageListDialog(errors, "Вставка");
          if (subDocs2.SubDocCount == 0)
            return null;
        }

        DocumentEditor mainEditor = null;
        if (controlProvider != null)
          mainEditor = controlProvider.MainEditor;

        bool canUseEditor;
        if (subDocs2.SubDocCount == 1)
          canUseEditor = true;
        else
          canUseEditor = CanMultiInsert;

        if (HasEditorHandlers && canUseEditor && mainEditor != null)
        {
          // Открытие редактора поддокумента
          // Режим должен быть обязательно InsertCopy, иначе значения не прочитаются
          SubDocumentEditor sde = new SubDocumentEditor(mainEditor, subDocs2, UIDataState.InsertCopy);
          sde.SuppressInsertColumnValues = true; // не нужна инициализация, иначе некоторые поля с режимом NewMode=AlwaysDefaultValue очистятся
          if (!sde.Run())
            return null;
        }
        else
        {
          if (EFPApp.MessageBox("Вставить " + (srcDocTypeBase.DocTypeBase.IsSubDoc ?
            "копии поддокументов" : "копии документов") + " (" + subDocs2.SubDocCount.ToString() + " шт.)?",
            "Подтверждение вставки поддокументов \"" + SubDocType.PluralTitle + "\"",
            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)

            return null;
        }
      }
      orgSubDocCount = mSubDocs.SubDocCount; // перечитываем еще раз на случай посторонних действий пользователя
      mSubDocs.MergeSubSet(subDocs2);
      InitManualOrderColumnValueAfterEdit(mSubDocs, subDocs2);
      DBxSubDoc[] a = new DBxSubDoc[mSubDocs.SubDocCount - orgSubDocCount];
      List<DataRow> resRows = new List<DataRow>();
      for (int i = 0; i < a.Length; i++)
        a[i] = mSubDocs[orgSubDocCount + i];
      return a;
    }

    #endregion
  }
}
